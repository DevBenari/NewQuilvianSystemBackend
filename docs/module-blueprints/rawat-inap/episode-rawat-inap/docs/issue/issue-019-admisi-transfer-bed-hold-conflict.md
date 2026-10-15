# ISSUE-019 — Galat 409 Conflict (BED_HOLD_CONFLICT) pada Finalisasi Admisi Pasien Transfer Rawat Inap

```yaml
issue_id: ISSUE-EPS-019
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Tab Antrean Pasien Transfer IGD/Poliklinik (Langkah 10 Konfirmasi Admisi Transfer)"
sumber_laporan: "Laporan pengguna: terjadi eror pada admisi tranfer pasien, selalu konflik di pasien manapun, tolong atasi anda analisis masalah tersebut. POST /admission-transfers/admit -> 409 Conflict BED_HOLD_CONFLICT Data bed berubah bersamaan. Muat ulang data."
tanggal_issue: "2026-10-14"
status: SELESAI
keparahan_tertinggi: Fatal
source_sha_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab"
source_sha_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b"
rencana_perbaikan: ../plan-repair/plan-repair-019-admisi-transfer-bed-hold-conflict.md
```

---

## 1. Ringkasan Masalah

Pada modul **Admisi Rawat Inap (Inpatient Management)**, petugas admisi memproses pasien transfer rujukan dari Instalasi Gawat Darurat (IGD) maupun Poliklinik Rawat Jalan (*Outpatient*). Petugas menyelesaikan pengisian formulir wizard 10 langkah (identitas, penjamin, kamar tidur, deposit awal, dan persetujuan).

Namun, saat menekan tombol submit konfirmasi akhir untuk mengeksekusi admisi:
* **Request URL**: `https://localhost:7184/api/v1/health-services/inpatient-management/admission-transfers/admit`
* **Request Method**: `POST`
* **Status Code**: `409 Conflict`
* **Payload Respons**:
```json
{
    "success": false,
    "statusCode": 409,
    "message": "Data bed berubah bersamaan. Muat ulang data.",
    "data": null,
    "errors": {
        "code": "BED_HOLD_CONFLICT"
    },
    "timestamp": "2026-10-14T15:50:53.0690017+07:00"
}
```

Galat ini terjadi secara konsisten **pada pasien manapun**, sehingga proses penerimaan pasien rawat inap hasil transfer IGD/Poliklinik lumpuh total di tahap finalisasi.

---

## 2. Laporan Asli Pengguna

| No. | Kalimat Pelapor | Konteks Layar |
| :---: | --- | --- |
| 1 | *"terjadi eror pada admisi tranfer pasien. selalu konflik di pasien manapun , tolong atasi anda analisis masalah tersebut"* | Layar Admisi Rawat Inap — Wizard Admisi Transfer Pasien Langkah 10 Konfirmasi Akhir |

---

## 3. Ringkasan Temuan Masalah

| ID Temuan | Masalah | Jenis | Area | Keparahan | Status Bukti | Solusi Rencana |
| --- | --- | :---: | :---: | :---: | :---: | --- |
| `ISS-EPS-019-01` | Entitas `RegPatientEncounterGuarantor` masuk ChangeTracker dengan status `Modified` alih-alih `Added` karena properti `Id` sudah terisi nilai GUID baru saat instansiasi, menyebabkan `DbUpdateConcurrencyException` saat `UPDATE` 0 baris | `BUG` | Backend EF Core | Fatal | TERBUKTI LENGKAP (`task-925.log` baris 103–104) | `FIX-EPS-019-01` |
| `ISS-EPS-019-02` | `InpBedMutationCoordinator` menangkap seluruh turunan `DbUpdateException` secara global dan membungkusnya menjadi `409 BED_HOLD_CONFLICT: "Data bed berubah bersamaan. Muat ulang data."` sehingga menyamarkan akar masalah Entity Framework | `MASKING_EXCEPTION` | Backend Coordinator | High | TERBUKTI LENGKAP (`InpBedMutationCoordinator.cs:107-114`) | `FIX-EPS-019-02` |
| `ISS-EPS-019-03` | Tabel basis data PostgreSQL `InpEpisode` kehilangan 6 kolom konteks admisi bayi baru lahir dan preferensi billing keluarga yang baru ditambahkan di model C#, memicu galat Postgres 42703 `column "FamilyBillingPreference" of relation "InpEpisode" does not exist` | `SCHEMA_DRIFT` | Database PostgreSQL | Fatal | TERBUKTI LENGKAP (Log Startup Npgsql 42703) | `FIX-EPS-019-03` |

---

## 4. Rincian dan Analisis Akar Masalah

### Akar Masalah 1: Status EntityState `Modified` pada Objek Penjamin Baru (`ISS-EPS-019-01`)

1. Pada [InpAdmissionTransferService.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs), alur admisi transfer memanggil:
   ```csharp
   var result = await episodeService.OpenAdmissionAsync(...);
   ```
   Langkah ini membuat dan menyimpan `RegPatientEncounter` baru ke database, dan instance `encounter` berada di dalam EF Core `ChangeTracker` dalam status `Unchanged`.
2. Selanjutnya dipanggil:
   ```csharp
   await paymentSources.InitializeAdmissionAsync(...);
   ```
   Di dalam [EncounterPaymentSourceService.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs):
   ```csharp
   encounter.PaymentSource = new RegPatientEncounterGuarantor
   {
       EncounterId = encounter.Id,
       PatientId = encounter.PatientId,
       PaymentSourceNumber = await _numbers.AllocatePaymentSourceNumberAsync(cancellationToken),
       PaymentType = EncounterPaymentType.Cash,
       ...
   };
   await _dbContext.SaveChangesAsync(cancellationToken);
   ```
3. Perhatikan definisi kelas [RegPatientEncounterGuarantor.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs):
   ```csharp
   public Guid Id { get; set; } = Guid.NewGuid();
   ```
4. **Mekanisme Perilaku EF Core:**
   Ketika sebuah entitas anak ditetapkan ke properti navigasi entitas induk (`encounter.PaymentSource = ...`) yang sudah dilacak (`Unchanged`), EF Core ChangeTracker memeriksa kunci utama `Id` dari entitas anak tersebut. Karena `Id` bukan `Guid.Empty` (melainkan GUID hasil `Guid.NewGuid()`), EF Core menganggap entitas tersebut **sudah ada sebelumnya di database** dan menetapkan statusnya menjadi `EntityState.Modified` (bukan `EntityState.Added`).
5. Saat `_dbContext.SaveChangesAsync()` dijalankan, EF Core menghasilkan perintah:
   ```sql
   UPDATE public."RegPatientEncounterGuarantor" SET ... WHERE "Id" = '...';
   ```
   Karena baris tersebut belum pernah di-insert, query `UPDATE` menghasilkan `0 rows affected`.
6. EF Core secara otomatis melempar:
   ```text
   Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException: The database operation was expected to affect 1 row(s), but actually affected 0 row(s); data may have been modified or deleted since entities were loaded.
   ```

---

### Akar Masalah 2: Masking Exception pada Koordinator Mutasi Bed (`ISS-EPS-019-02`)

Pada [InpBedMutationCoordinator.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedMutationCoordinator.cs) baris 107–114:
```csharp
catch (DbUpdateException ex)
{
    await tx.RollbackAsync(ct);
    db.ChangeTracker.Clear();
    throw new InpBedOperationException(409, "BED_HOLD_CONFLICT", "Data bed berubah bersamaan. Muat ulang data.");
}
```
`DbUpdateConcurrencyException` merupakan turunan langsung dari `DbUpdateException`. Seluruh galat database yang terjadi di dalam blok mutasi (termasuk galat penjamin pendaftaran di atas) tertangkap oleh blok ini dan diubah menjadi pesan `409 BED_HOLD_CONFLICT: "Data bed berubah bersamaan. Muat ulang data."`. Hal ini membuat staf rumah sakit mengira kamar/bed yang dipilih sedang direbut oleh orang lain, padahal tempat tidur dalam kondisi kosong dan siap pakai.

---

### Akar Masalah 3: Schema Drift Kolom Bayi pada Tabel `InpEpisode` (`ISS-EPS-019-03`)

Model [InpEpisode.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs) memiliki 6 kolom baru:
- `NewbornAdmissionSource` (`varchar(20)`)
- `NewbornCareMode` (`varchar(20)`)
- `NewbornCareSource` (`varchar(200)`)
- `RequiresNeonatalIntensiveCare` (`boolean`)
- `FamilyBillingPreference` (`varchar(20)`)
- `PlannedDepositAmount` (`numeric(18,2)`)

Kolom-kolom ini belum sempat diterapkan pada tabel PostgreSQL server development, sehingga `INSERT INTO "InpEpisode"` pertama kali gagal dengan error Postgres 42703:
```text
Npgsql.PostgresException (0x80004005): 42703: column "FamilyBillingPreference" of relation "InpEpisode" does not exist
```

---

## 5. Dampak Klinis dan Operasional Rumah Sakit

1. **Pasien Gawat Darurat dan Rujukan Poli Tertahan:** Pasien IGD dengan kondisi dehidrasi sedang, observasi infeksi, maupun persiapan operasi rawat inap tidak dapat dipindahkan ke bangsal rawat inap secara sistemik.
2. **Deposit Pasien Tidak Masuk ke Kasir:** Uang muka/deposit rawat inap yang disetorkan keluarga pasien tidak dapat diterbitkan tanda terimanya (*receipt* kasir tidak terbentuk).
3. **Kamar Bed Terlihat "Bermasalah":** Petugas admisi berulang kali memuat ulang data dan mencoba memilih ranjang tidur lain karena mengira terjadi konflik konkurensi ranjang tidur.
