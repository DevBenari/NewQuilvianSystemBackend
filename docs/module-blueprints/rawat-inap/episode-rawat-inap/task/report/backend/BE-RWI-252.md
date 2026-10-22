# Laporan Pelaksanaan Task Backend: BE-RWI-252

| Metadata | Nilai |
| --- | --- |
| **Task ID** | `BE-RWI-252` |
| **Blueprint / Sub-modul** | `RWI-BP-001` / `episode-rawat-inap` (bentuk `COMPOSITE`) |
| **Delivery Planning ID** | `TRF-DEL-20261019-01` |
| **Kontrak Target** | `0.17.0` (Status: `APPROVED_DESIGN_PLANNED`) |
| **Tanggal Eksekusi** | 20 Oktober 2026 |
| **Branch Backend** | `MHamzah` |
| **Status Penyelesaian** | `✅ SELESAI (KODE SUMBER & DTO DIPERBARUI)` |

---

## 1. Backend Governance Preflight

- **Area / Module / Submodule**: `HealthServices` / `InPatientManagement` / `episode-rawat-inap`.
- **Owner & Entity Prefix**: `InPatientManagement` / `Inp`.
- **Keberlakuan**: `TOUCHED LEGACY` (Ekstensi DTO transfer dan kueri relasional transfer rawat inap yang sudah ada).
- **Status Registry**: `APPROVED_DESIGN_PLANNED` (Kontrak target `0.17.0`; Bab 18 API Contract, Bab 15 Integration Contract, Bab 20 Backend Architecture).
- **QBE Conformance**: `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-AUD-001`.
- **Kebijakan Pengujian**: `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
- **Kebijakan Build & Migrasi**: *Bypassed per instruksi eksplisit pengguna* ("tidak perlu melakukan dotnet build , saya lakukan dotnet build secara mandiri saja. Cukup Implementasi Code saja. jika ada yang ingin di lakukan migration buatkan file migration ya saja , nanti saya tinggal lakukan migration with command terminal").

---

## 2. Ringkasan Kebutuhan dan Desain

Sesuai dengan keputusan bisnis `RWI-DEC-338`, `RWI-DEC-339`, `RWI-DEC-341`, dan functional requirement `FR-TRF-008`, `FR-TRF-012`:
1. Respon DTO `InpatientAdmissionTransferItemResponse` diperluas dengan field status kedaruratan transfer IGD (`IsCito`) dan hak keanggotaan antrean prioritas CRM (`IsPriorityMember`, `MemberTierName`, `MemberPriorityLevel`).
2. Method kueri `TransferQuery` pada `InpAdmissionTransferService.Queries.cs` memproyeksikan status CITO dari formulir `EmgAdmissionRequest` yang berstatus `Signed` (`IsCito == true`). Untuk transfer asal non-IGD (Poliklinik), `IsCito` diproyeksikan `false`.
3. Evaluasi membership dieksekusi secara real-time pada kueri: `MembershipStatus == Active`, tanggal hari ini berada di antara `PeriodStartDate` dan `ExpiredDate` (atau null), serta tier keanggotaan memiliki `PriorityQueue == true`.
4. Jika pasien non-member, membership kedaluwarsa/inaktif, atau tier tidak memiliki hak antrean prioritas (`PriorityQueue == false`), sistem memetakan `IsPriorityMember = false`, `MemberTierName = null`, dan `MemberPriorityLevel = null`.
5. Kueri dirancang secara fail-safe (null-safe) sehingga ketiadaan relasi data IGD atau CRM tidak menyebabkan galat HTTP 500 (`RWI-AC-493`).
6. Endpoint detail `GET /api/v1/health-services/inpatient-management/admission-transfers/{id}` secara konsisten mengembalikan metadata yang sama (`RWI-AC-492`).

---

## 3. Rincian Perubahan Kode Sumber

### 3.1 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`
Pada kelas `InpatientAdmissionTransferItemResponse`, ditambahkan empat properti kontrak `0.17.0`:
```csharp
/// <summary>
/// Menandakan apakah permintaan transfer berstatus gawat darurat / cito dari IGD (EmgAdmissionRequest.IsCito == true).
/// </summary>
public bool IsCito { get; set; }

/// <summary>
/// Menandakan apakah pasien memiliki membership aktif yang berhak atas antrean prioritas (PriorityQueue == true).
/// </summary>
public bool IsPriorityMember { get; set; }

/// <summary>
/// Nama tier keanggotaan aktif pasien (misal "Platinum", "Gold", "Silver"). Bernilai null jika reguler/non-member.
/// </summary>
public string? MemberTierName { get; set; }

/// <summary>
/// Level prioritas tier keanggotaan (angka lebih kecil = prioritas lebih tinggi). Bernilai null jika reguler/non-member.
/// </summary>
public int? MemberPriorityLevel { get; set; }
```

### 3.2 `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs`
1. Ditambahkan namespace import:
   ```csharp
   using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
   using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
   ```
2. Pada awal method `TransferQuery(string? sourceType)`, dihitung tanggal kalender UTC saat ini:
   ```csharp
   var today = DateTime.UtcNow.Date;
   ```
3. Pada proyeksi kueri `emergency`:
   - `IsCito`:
     ```csharp
     IsCito = _dbContext.Set<EmgAdmissionRequest>().Any(f =>
         f.DispositionId == x.Id && !f.IsDelete
         && f.Status == EmergencyAdmissionRequestStatus.Signed
         && f.IsCito),
     ```
   - `IsPriorityMember`, `MemberTierName`, dan `MemberPriorityLevel`:
     Diproyeksikan langsung dari `_dbContext.MstPatientMemberships` yang terhubung ke `x.EmergencyVisit.PatientId` dengan filter `MembershipStatus == Active`, validitas tanggal `today`, dan tier `PriorityQueue == true`.
4. Pada proyeksi kueri `outpatient`:
   - `IsCito = false` (surat pengantar rawat jalan poliklinik tidak berstatus CITO IGD).
   - `IsPriorityMember`, `MemberTierName`, dan `MemberPriorityLevel` diproyeksikan dari `_dbContext.MstPatientMemberships` yang terhubung ke `p.Id` dengan filter yang identik.

---

## 4. Status Database & Migrasi EF Core

- **Kebutuhan Kolom Baru**: **TIDAK ADA (0 Migrasi EF Core Baru)**.
- **Verifikasi Skema PostgreSQL**:
  - Kolom `IsCito` telah ada pada entitas `EmgAdmissionRequest` (`Areas/HealthServices/EmergencyInstallationManagement/Models/EmgAdmissionRequest.cs:39`).
  - Kolom `MembershipStatus`, `PeriodStartDate`, `ExpiredDate` telah ada pada `MstPatientMembership` (`Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs`).
  - Kolom `PriorityQueue`, `PriorityLevel`, `TierName` telah ada pada `MstMembershipTier` (`Areas/Administrator/MasterData/Models/MstMembershipTier.cs`).
- **Kesimpulan**: Nol tabel baru, nol kolom baru, dan nol migrasi EF Core. Tidak ada eksekusi `dotnet ef migrations add` maupun `dotnet ef database update`.

---

## 5. Matriks Acceptance Criteria & Bukti Verifikasi

| ID Kriteria | Deskripsi Kriteria | Target Verifikasi | Status | Catatan / Bukti |
| --- | --- | --- | :---: | --- |
| `RWI-AC-481` | DTO response memuat field CITO & Membership | `InpatientAdmissionTransferItemResponse` | `TERVERIFIKASI` | Properti `IsCito`, `IsPriorityMember`, `MemberTierName`, `MemberPriorityLevel` terpasang di DTO |
| `RWI-AC-483` | Evaluasi real-time membership aktif & priority queue | `InpAdmissionTransferService.Queries.cs` | `TERVERIFIKASI` | Kueri memeriksa `MembershipStatus == Active`, rentang tanggal hari ini, dan `PriorityQueue == true` |
| `RWI-AC-484` | Membership inaktif/expired/non-prioritas = reguler | `InpAdmissionTransferService.Queries.cs` | `TERVERIFIKASI` | Evaluasi mengembalikan `IsPriorityMember = false`, `MemberTierName = null`, `MemberPriorityLevel = null` |
| `RWI-AC-485` | Status CITO sah dari formulir transfer IGD yang Signed | `InpAdmissionTransferService.Queries.cs` | `TERVERIFIKASI` | Filter `Status == EmergencyAdmissionRequestStatus.Signed && f.IsCito` |
| `RWI-AC-492` | Endpoint detail `/{id}` membawa field CITO & Membership konsisten | `GetTransferByIdAsync` | `TERVERIFIKASI` | Endpoint memanggil `TransferQuery` yang sama sehingga payload detail identik dengan list |
| `RWI-AC-493` | Fail-safe fallback bila data CRM/IGD null tanpa crash 500 | `TransferQuery` | `TERVERIFIKASI` | Kueri memakai LINQ subquery/left join yang null-safe tanpa melempar exception bila baris tidak ditemukan |
| `git diff --check` | Pemeriksaan whitespace & conflict marker | CLI git | `PASS` | Kode bersih tanpa conflict marker atau trailing whitespace issue |
| Kompilasi & Build | `dotnet build` mandiri oleh pengguna | CLI dotnet | `DITANGGUHKAN MANDIRI` | Sesuai instruksi eksplisit pengguna, build dan migrasi dijalankan mandiri oleh pengguna |

---

## 6. Berkas yang Disentuh

1. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionTransferDtos.cs`
2. `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.Queries.cs`
3. `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-252.md` (Berkas laporan ini)

---

## 7. Penyerahan ke Task Berikutnya

Task dependensi berikutnya adalah **`BE-RWI-253`** (Algoritma pengurutan multi-kriteria deterministik CITO → Member Prioritas → Reguler FIFO). Seluruh properti `IsCito`, `IsPriorityMember`, `MemberPriorityLevel`, dan `DecidedAt` kini telah tersedia pada `itemsQuery` dan siap diterapkan pada rantai `.OrderByDescending()` / `.ThenBy()` di `InpAdmissionTransferService.cs`.
