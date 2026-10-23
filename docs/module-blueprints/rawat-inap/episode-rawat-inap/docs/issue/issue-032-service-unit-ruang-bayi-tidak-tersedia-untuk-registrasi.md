# ISSUE-032 — Kegagalan Pembentukan Kunjungan Pasien Bayi (HTTP 400): Service Unit Ruang Bayi (SU-BABY-001) Berstatus Tidak Tersedia untuk Registrasi

```yaml
issue_id: ISSUE-EPS-032
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 4 Penempatan dan Pemesanan Bayi (FE-INP-026)"
sumber_laporan: "Laporan pengguna 22 Oktober 2026: HTTP 400 Bad Request pada POST patient-encounters/admin saat memilih bed di kamar Perinatologi (kamar BAYI-1 / BAYI-2) dengan pesan 'Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.'"
tanggal_issue: "2026-10-22"
status: SELESAI
tanggal_selesai: "2026-10-22"
keparahan_tertinggi: Kritis
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-032-service-unit-ruang-bayi-tidak-tersedia-untuk-registrasi.md
```

---

## 1. Ringkasan

Ketika petugas mendaftarkan admisi bayi baru lahir pada mode perawatan Ruang Perinatologi dan memilih tempat tidur di kamar Perinatologi (`BAYI-1` atau `BAYI-2`) di Langkah 4 ("Penempatan dan Pemesanan"), penekanan tombol untuk membuat draf episode memicu panggilan API:
- **Endpoint**: `POST /api/v1/health-services/registration-management/patient-encounters/admin`
- **Hasil**: HTTP 400 Bad Request
- **Pesan Server**: `"Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi."`
- **Dampak Antarmuka**: Muncul banner merah di atas denah: *"Unit layanan untuk bed ini baru saja tidak tersedia untuk registrasi rawat inap. Daftar bed sudah diperbarui; silakan pilih bed lain."*, sehingga proses admisi bayi terhenti dan tidak dapat berlanjut ke Langkah 5.

---

## 2. Laporan Asli Pelapor

```json
Request URL: https://localhost:7184/api/v1/health-services/registration-management/patient-encounters/admin
Request method: POST
Status code: 400 Bad Request

Payload:
{
  "doctorId": "bc389b2c-9b4e-47a7-8a28-98033ef7f97a",
  "encounterType": 3,
  "inheritedPaymentSourceExpectedAt": "2026-10-19T08:57:28.459488Z",
  "inheritedPaymentSourceId": "f5000000-0000-4000-8000-000000000238",
  "notes": null,
  "patientClassId": "eaad1fd0-5237-4733-a6b2-e08ffc821907",
  "patientCompanyGuarantorId": null,
  "patientId": "b2d01512-f40f-448f-9cd6-ff47670ab4ac",
  "patientInsuranceId": null,
  "paymentMethodId": null,
  "paymentType": 2,
  "registrationSource": 1,
  "serviceUnitId": "dea54fab-052c-4c87-8fb6-b9e916c50b2d",
  "visitType": 1
}

Response:
{
  "success": false,
  "statusCode": 400,
  "message": "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.",
  "data": null,
  "errors": null,
  "timestamp": "2026-10-22T10:00:47.1397633+07:00"
}
```

---

## 3. Analisis Akar Masalah (Root Cause)

### A. Penelusuran Baris Kode Backend
Pada `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` baris 2076–2099:

```csharp
var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
    .AsNoTracking()
    .AnyAsync(x =>
        x.Id == request.ServiceUnitId &&
        x.IsActive &&
        !x.IsDelete &&
        x.IsAvailableForRegistration);

if (!serviceUnitExists)
{
    return (false, "Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi.");
}

if (request.EncounterType == EncounterType.Inpatient)
{
    var isInpatientUnit = await _dbContext.Set<MstServiceUnit>()
        .AsNoTracking()
        .Where(ServiceUnitRegistrationRules.InpatientRegistrationEligible)
        .AnyAsync(x => x.Id == request.ServiceUnitId);

    if (!isInpatientUnit)
    {
        return (false, "Unit tujuan untuk kunjungan rawat inap wajib merupakan unit layanan bertipe rawat inap.");
    }
}
```

Dan pada `Areas/HealthServices/MasterData/Services/ServiceUnitRegistrationRules.cs` baris 14–18:

```csharp
public static Expression<Func<MstServiceUnit, bool>> InpatientRegistrationEligible =>
    unit => unit.IsActive &&
            !unit.IsDelete &&
            unit.IsAvailableForRegistration &&
            unit.ServiceUnitType == ServiceUnitType.Inpatient;
```

### B. Audit Data Riil Database `QuilvianNewDevHamzah`
Berdasarkan query langsung ke tabel `public."MstServiceUnit"`:

| ServiceUnitCode | ServiceUnitName | ServiceUnitType | IsActive | IsDelete | IsAvailableForRegistration | Keterangan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `SU-BABY-001` | **Ruang Bayi** | 2 (`Inpatient`) | `true` | `false` | **`false`** ❌ | **Pemicu Error HTTP 400** |
| `SU-NICU-001` | **NICU** | 2 (`Inpatient`) | `true` | `false` | **`false`** ❌ | Berpotensi error yang sama jika dipilih |
| `SU-ICU-001` | **ICU** | 2 (`Inpatient`) | `true` | `false` | **`false`** ❌ | Berpotensi error yang sama jika dipilih |
| `SU-HCU-001` | **HCU** | 2 (`Inpatient`) | `true` | `false` | **`false`** ❌ | Berpotensi error yang sama jika dipilih |
| `SU-IPD-001` | Rawat Inap | 2 (`Inpatient`) | `true` | `false` | `true` ✅ | Unit rawat inap umum (berfungsi normal) |

### C. Kesimpulan Penyebab
Kamar `BAYI-1`, `BAYI-2`, dan `Ruang NICU` bernaung di bawah unit layanan `SU-BABY-001` (Ruang Bayi). Karena kolom `IsAvailableForRegistration` bernilai `false`, endpoint `PatientEncounterController.cs` menolak pembentukan kunjungan pasien baru dengan kode 400.

---

## 4. Rekomendasi Solusi

Mengaktifkan bendera `IsAvailableForRegistration = true` pada unit layanan rawat inap khusus di tabel `public."MstServiceUnit"` pada database `QuilvianNewDevHamzah`:
1. `SU-BABY-001` (Ruang Bayi) — Prioritas P0 (langsung memulihkan admisi bayi).
2. `SU-NICU-001` (NICU) — Prioritas P1 (mencegah error pada alur NICU).
3. `SU-ICU-001` (ICU) & `SU-HCU-001` (HCU) — Prioritas P1 (mencegah error pada alur admisi intensif).

---

## 5. Hubungan dengan Dokumen Terkait

| Dokumen | Keterangan |
| :--- | :--- |
| `issue-022-perbaikan-alur-admisi-bayi.md` | Pernah mencatat pentingnya sinkronisasi `IsAvailableForRegistration` antara query ketersediaan bed dan pembentukan encounter. |
| `FIX-EPS-022-06.md` | Menegaskan empat syarat `ServiceUnitRegistrationRules.InpatientRegistrationEligible` (`IsActive`, `!IsDelete`, `IsAvailableForRegistration`, `ServiceUnitType == Inpatient`). |
| `plan-repair-031-tiga-tipe-kamar-bayi-rawat-gabung-perinatologi-nicu.md` | Konfigurasi 3 jenis perawatan bayi yang menempatkan boks pada Ruang Bayi (Perinatologi) dan Ruang NICU. |

---

## 6. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen issue dibuat berdasarkan laporan HTTP 400 pada pendaftaran kunjungan bayi di Ruang Bayi. | `diagnose-module-issue` |
| 2026-10-22 | Status ditutup SELESAI: Eksekusi SQL pembaruan `IsAvailableForRegistration = true` pada unit `SU-BABY-001`, `SU-NICU-001`, `SU-ICU-001`, dan `SU-HCU-001` sukses di DB `QuilvianNewDevHamzah`. | `build-module-backend` / DB Ops |
