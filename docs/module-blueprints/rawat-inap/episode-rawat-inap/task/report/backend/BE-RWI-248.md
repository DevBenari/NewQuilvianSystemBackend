# Laporan Pelaksanaan Task Backend: BE-RWI-248

| Metadata | Nilai |
| --- | --- |
| **Task ID** | `BE-RWI-248` |
| **Blueprint / Sub-modul** | `RWI-BP-001` / `episode-rawat-inap` |
| **Delivery Planning ID** | `ODC-DEL-20261017-01` |
| **Kontrak Target** | `0.15.0` (Status: `APPROVED_DESIGN_PLANNED`) |
| **Tanggal Eksekusi** | 17 Oktober 2026 |
| **Branch Backend** | `MHamzah` (`def6cf6ab5f945cb7bf228847d90fcb10a2d2d94`) |
| **Status Penyelesaian** | `SELESAI (KODE SUMBER & DTO DIPERBARUI)` |

---

## 1. Backend Governance Preflight

- **Area / Module / Submodule**: `HealthServices` / `InPatientManagement` / `episode-rawat-inap`.
- **Owner & Entity Prefix**: `Inp` (InPatient Management Registry).
- **Keberlakuan**: `TOUCHED LEGACY` (Ekstensi DTO dan Service Kueri Ketersediaan Bed yang sudah ada).
- **Status Registry**: `APPROVED_DESIGN_PLANNED` (Kontrak target `0.15.0`).
- **QBE Conformance**: Mengikuti standar kueri read-only transaksi dan master data tanpa mengubah arsitektur dependency injection atau memperkenalkan generic repository.
- **Kebijakan Pengujian**: `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
- **Kebijakan Build & Migrasi**: *Bypassed per instruksi eksplisit pengguna* ("tanpa melakukan dotnet build dan migration jika ada file migration di siapkan saja file saya akan melakuakan dotnet build dan migration secara mandiri").

---

## 2. Ringkasan Kebutuhan dan Desain

Sesuai dengan keputusan bisnis `RWI-DEC-329`, `RWI-DEC-330`, dan kebutuhan `FR-ODC-007` / `RWI-AC-465`:
1. Petugas pendaftaran admisi One Day Care (ODC) memerlukan penyaringan tempat tidur ketersediaan yang secara khusus diperuntukkan bagi pasien ODC (`isOdcBed = true`) atau ruangan khusus ODC (`isOdcRoom = true`).
2. Respon ketersediaan tempat tidur `GET /api/v1/health-services/inpatient-management/bed-occupancies/available-beds` wajib menyertakan flag `isOdcBed` agar antarmuka frontend dapat mengenali dan menandai tempat tidur transit ODC secara visual.

---

## 3. Rincian Perubahan Kode Sumber

### 3.1 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs`
- Pada kelas `AvailableBedQuery`:
  - Menambahkan properti opsional `public bool? IsOdcBed { get; set; }` untuk menyaring tempat tidur khusus One Day Care (ODC).
  - Menambahkan properti opsional `public bool? IsOdcRoom { get; set; }` untuk menyaring tempat tidur yang berada di ruangan khusus One Day Care (ODC).
- Pada kelas `AvailableBedResponse`:
  - Menambahkan properti boolean `public bool IsOdcBed { get; set; }` untuk menandai apakah tempat tidur merupakan tempat tidur khusus ODC.

### 3.2 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
- Pada method `SearchAvailableBedsAsync`:
  - Menambahkan filter LINQ pada `bedQuery`:
    ```csharp
    // BE-RWI-248: saring tempat tidur khusus ODC dan ruangan ODC.
    if (query.IsOdcBed.HasValue)
    {
        bedQuery = bedQuery.Where(x => x.IsOdcBed == query.IsOdcBed.Value);
    }

    if (query.IsOdcRoom.HasValue)
    {
        bedQuery = bedQuery.Where(x => x.Room != null && x.Room.IsOdcRoom == query.IsOdcRoom.Value);
    }
    ```
  - Pada pemetaan DTO `AvailableBedResponse`:
    ```csharp
    IsReservable = x.IsReservable,
    IsOdcBed = x.IsOdcBed
    ```

---

## 4. Status Database & Migrasi EF Core

- **Kebutuhan Kolom Baru**: **TIDAK ADA (0 Migrasi EF Core Baru)**.
- **Verifikasi Skema PostgreSQL**:
  - Kolom `IsOdcBed` (tipe `boolean`, default `false`) telah ada pada model `MstBed` (`Areas/HealthServices/MasterData/Models/MstBed.cs:35`).
  - Kolom `IsOdcRoom` (tipe `boolean`, default `false`) telah ada pada model `MstRoom` (`Areas/HealthServices/MasterData/Models/MstRoom.cs:45`).
- Tidak diperlukan berkas migrasi Entity Framework baru untuk task ini.

---

## 5. Matriks Acceptance Criteria & Bukti Verifikasi

| ID Kriteria | Deskripsi Kriteria | Target Verifikasi | Status | Catatan / Bukti |
| --- | --- | --- | :---: | --- |
| `RWI-AC-465` (Kueri Filter) | Kueri pencarian tempat tidur menerima parameter `isOdcBed` dan `isOdcRoom` | `AvailableBedQuery` | `TERVERIFIKASI` | DTO diperluas dengan `IsOdcBed` dan `IsOdcRoom` bertipe `bool?`, controller mengikat query string via `[FromQuery]` |
| `VAL-ADM-10` (Bagian Filter) | Tempat tidur ODC terpetakan dengan benar pada hasil kueri | `SearchAvailableBedsAsync` | `TERVERIFIKASI` | LINQ query menyaring `x.IsOdcBed == query.IsOdcBed.Value` dan `x.Room.IsOdcRoom == query.IsOdcRoom.Value` |
| `FR-ODC-007` | Respon API menyertakan status ODC tempat tidur | `AvailableBedResponse` | `TERVERIFIKASI` | Field `IsOdcBed` ditambahkan pada DTO respon dan dipetakan langsung dari entitas `MstBed` |
| Kompilasi & Build | `dotnet build` mandiri oleh pengguna | CLI dotnet | `DITANGGUHKAN MANDIRI` | Sesuai instruksi pengguna, build dan migrasi tidak dieksekusi otomatis oleh agent dan akan dijalankan mandiri oleh pengguna |

---

## 6. Berkas yang Disentuh

1. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs`
2. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
3. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-248.md` (Berkas laporan ini)

---

## 7. Penyerahan ke Task Berikutnya

Task dependensi berikutnya adalah **`BE-RWI-249`** (Penegakan invariant kelayakan penempatan bed ODC, pengecualian transfer bed > 24 jam, dan validasi DPJP wajib). Parameter dan filter kueri yang disiapkan pada `BE-RWI-248` ini siap digunakan oleh aturan bisnis `BE-RWI-249`.
