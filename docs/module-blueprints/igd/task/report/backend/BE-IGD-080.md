# Laporan Perubahan Backend — `BE-IGD-080`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-080` |
| Judul | Layanan Reservasi Pra-Kedatangan Pasien Prioritas & Dispatch Ambulans IGD |
| Slice | `EPIC IGD-16` · Vertical Slice B5 / Reservasi Pra-Kedatangan Pasien Prioritas |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.19 |
| Requirement | `FR-IGD-122`, `FR-IGD-123`; `AT-IGD-242`, `AT-IGD-243` |
| Keputusan | `IGD-DEC-314`, `IGD-DEC-315` |
| Contract version | API `0.20.0` §13; validation `0.19.0` §15; data dictionary §12 |
| Dependency | `BE-IGD-053` ✅, `BE-IGD-055` ✅ |
| Klasifikasi | `MEDIUM` — penambahan entitas reservasi pra-kedatangan, integrasi ambulans, penugasan pendamping (escort), dan aksi check-in promosi kunjungan aktif IGD |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): enum, model, EF configuration, DTOs, service, controller, ApplicationDbContext, Program.cs |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ **DONE — source selesai, build 0 error, QBE Strict PASS.** Entitas `EmgPreArrivalReservation`, service, dan controller `EmergencyPreArrivalReservationController` telah diimplementasikan penuh. Panggilan hotline pra-kedatangan dapat dicatat beserta pesanan ambulans dan penugasan pendamping, serta dapat dipromosikan langsung menjadi kunjungan IGD aktif saat pasien tiba secara fisik. |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY`. |
| Keberlakuan | `NEW CODE` untuk entitas `EmgPreArrivalReservation` di bawah modul IGD |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-NAM-003`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` |
| `Program.cs` | Registrasi scoped `EmergencyPreArrivalReservationService`. |
| Migration | Siap dibuat dan diaplikasikan oleh pemilik (`dotnet ef migrations add AddEmgPreArrivalReservation --no-build` & `dotnet ef database update`). |

---

## 1. Latar Belakang & Kebutuhan Bisnis

Sesuai standar operasional Rumah Sakit Umum Elite di Indonesia (seperti RS Pondok Indah, Siloam MRCCC, Mayapada Hospital, dan Medistra) dan keputusan bisnis `IGD-DEC-314` serta `IGD-DEC-315`:
1. **Layanan Pra-Kedatangan Pasien Prioritas (VIP/Executive CRM-Membership):**
   - Pasien member prioritas atau keluarganya sering kali menghubungi hotline gawat darurat atau WhatsApp RS sebelum tiba secara fisik.
   - Admisi IGD perlu mencatat data pra-kedatangan meliputi: identitas pemanggil, nomor telepon, estimasi waktu kedatangan (ETA), keluhan gawat darurat awal, identitas pasien atau nomor RM, dan keanggotaan CRM aktif (`MstPatientMembership`).
2. **Integrasi Dispatch Ambulans:**
   - Bila pemanggil membutuhkan ambulans gawat darurat, admisi dapat mencatat order penjemputan dengan memilih moda ambulans (Transport / Medis ICU dari `EmgArrivalMode`) dan alamat penjemputan lengkap.
3. **Penugasan Pendamping Khusus (*Guest Relations Officer / Escort*):**
   - Pasien prioritas dipersiapkan tim pendamping khusus yang akan menyambut di pintu masuk / drop-off IGD dan mengawal kenyamanan keluarga.
4. **Promosi Satu-Klik ke Kunjungan Aktif (*Check-in / Confirm Arrival*):**
   - Saat pasien tiba di lobi IGD, petugas admisi cukup menekan satu tombol check-in. Sistem otomatis memvalidasi pasien, memeriksa episode ganda, dan melahirkan kunjungan IGD aktif (`EmgVisit`) bernomor resmi `IGD-yyMMddHHmmss-XXXXXX`, sehingga pasien langsung siap ditriage tanpa antre input ulang di loket umum.
5. **Independensi Triase Klinis:**
   - Derajat triase klinis (ATS / ESI) tetap independen dan otonom berdasarkan kegawatan medis tanpa diskriminasi, sesuai standar akreditasi Kemenkes, STARKES, dan JCI. Keistimewaan pasien prioritas adalah fasilitas keramahan (*hospitality privilege*) dan administrasi di samping tempat tidur (*bedside administration*).

---

## 2. Rincian Berkas yang Dibuat & Diubah

| Berkas | Jenis | Perubahan Teknis |
| :--- | :--- | :--- |
| `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyPreArrivalStatus.cs` | Baru | Enum status reservasi: `PendingArrival = 1`, `Arrived = 2`, `Cancelled = 3`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgPreArrivalReservation.cs` | Baru | Model entitas `EmgPreArrivalReservation` turunan `IdentityModel` dengan relasi ke `MstPatient`, `MstPatientMembership`, `MstMembershipTier`, `EmgArrivalMode`, dan `EmgVisit`. |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgPreArrivalReservationConfiguration.cs` | Baru | Pemetaan EF Core: tabel `"EmgPreArrivalReservation"` schema `"public"`, indeks unik `ReservationNumber`, konfigurasi timestamp with time zone, foreign key restrictions. |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyPreArrivalReservationDtos.cs` | Baru | DTO request/response: `CreateEmergencyPreArrivalReservationRequest`, `UpdateEmergencyPreArrivalReservationRequest`, `CancelEmergencyPreArrivalReservationRequest`, `CheckInEmergencyPreArrivalReservationRequest`, dan `EmergencyPreArrivalReservationResponse`. |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyPreArrivalReservationService.cs` | Baru | Logika bisnis: pembuatan nomor reservasi `RES-EMG-yyMMddHHmmss-XXXXXX`, auto-detect membership CRM aktif pasien, pencarian paged filter, pembaruan, pembatalan beralasan, serta check-in promosi kunjungan aktif `EmgVisit` dengan pencegahan episode ganda (`EmergencyEpisodeRule`). |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyPreArrivalReservationController.cs` | Baru | Controller RESTful dengan dekorator `[AccessController]`, `[AccessAction]`, `[AccessPermission]` sesuai RBAC Quilvian, audit logging, dan 6 endpoint operasi. |
| `Repositories/ApplicationDbContext.cs` | Diubah | Penambahan `public DbSet<EmgPreArrivalReservation> EmgPreArrivalReservations { get; set; }`. |
| `Program.cs` | Diubah | Pendaftaran scoped DI: `builder.Services.AddScoped<EmergencyPreArrivalReservationService>();`. |

---

## 3. Spesifikasi Endpoint Bergaya Swagger

### `[Tags("Health Services / Emergency Installation Management / Emergency Pre-Arrival Reservation")]`

| Method | Path | Deskripsi | Auth / Role | Request Payload | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations` | Mengambil daftar reservasi pra-kedatangan berhalaman dengan saringan pencarian, status, dan rentang waktu tiba | `EmergencyPreArrivalReservation:Read` | Query: `search`, `status`, `fromDate`, `toDate`, `page`, `pageSize` | `ApiResponse<PagedResult<EmergencyPreArrivalReservationResponse>>` |
| `GET` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}` | Mengambil detail lengkap reservasi pra-kedatangan berdasarkan ID | `EmergencyPreArrivalReservation:Read` | Path: `id` (GUID) | `ApiResponse<EmergencyPreArrivalReservationResponse>` |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations` | Membuat reservasi pra-kedatangan baru, nomor otomatis `RES-EMG-yyMMddHHmmss-XXXXXX`, dan mendeteksi keanggotaan CRM aktif pasien | `EmergencyPreArrivalReservation:Create` | `CreateEmergencyPreArrivalReservationRequest` | `ApiResponse<EmergencyPreArrivalReservationResponse>` (201 Created) |
| `PUT` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}` | Memperbarui informasi reservasi (pemanggil, ETA, keluhan, moda ambulans, staf pendamping) selama masih berstatus `PendingArrival` | `EmergencyPreArrivalReservation:Update` | `UpdateEmergencyPreArrivalReservationRequest` | `ApiResponse<EmergencyPreArrivalReservationResponse>` |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/cancel` | Membatalkan reservasi pra-kedatangan dengan alasan pembatalan tertulis | `EmergencyPreArrivalReservation:Update` | `CancelEmergencyPreArrivalReservationRequest` (`cancellationReason`) | `ApiResponse<EmergencyPreArrivalReservationResponse>` |
| `POST` | `/api/v1/health-services/emergency-installation-management/pre-arrival-reservations/{id}/check-in` | Mengonfirmasi kedatangan fisik pasien: memvalidasi status, memeriksa penjaga episode ganda, melahirkan `EmgVisit` aktif, dan menautkan nomor kunjungan | `EmergencyPreArrivalReservation:Update` | `CheckInEmergencyPreArrivalReservationRequest` | `ApiResponse<EmergencyPreArrivalReservationResponse>` |

---

## 4. Bukti Verifikasi

### 4.1 Kompilasi Backend C#
```powershell
dotnet build -p:RunAnalyzers=false
```
Hasil:
```text
  QuilvianSystemBackend -> C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend\bin\Debug\net9.0\QuilvianSystemBackend.dll

Build succeeded.
    10 Warning(s)
    0 Error(s)
Time Elapsed 00:00:03.71
```

### 4.2 Evaluasi QBE Strict Conformance
```powershell
powershell -ExecutionPolicy Bypass -File "tooling\qbe\Invoke-QbeConformanceCheck.ps1" -Mode Strict
```
Hasil:
```text
QBE Conformance Report
Checker mode: Strict
Scope: WorkingTree
Files evaluated: 9
Generated files excluded (bin/obj): 0
Unchanged migration moves excluded (Migrations/, R100): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

### 4.3 Kebijakan Komentar Kode & Garis Akhir
- Seluruh kode baru dan modifikasi pada berkas C# memenuhi aturan tanpa komentar kode baru (nol komentar).
- Seluruh berkas disimpan dengan format akhir baris CRLF tanpa UTF-8 BOM.

---

## 5. Pemetaan Acceptance Criteria

| # | Kriteria | Status | Bukti |
| ---: | --- | :---: | --- |
| 1 | Enum `EmergencyPreArrivalStatus` (`PendingArrival` = 1, `Arrived` = 2, `Cancelled` = 3) dibuat di namespace Enums IGD | ✅ Terpenuhi | `Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyPreArrivalStatus.cs` |
| 2 | Model `EmgPreArrivalReservation` terdefinisi dengan atribut relasi ke `MstPatient`, `MstPatientMembership`, `MstMembershipTier`, `EmgArrivalMode`, dan `EmgVisit` | ✅ Terpenuhi | `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgPreArrivalReservation.cs` |
| 3 | Konfigurasi EF Core `EmgPreArrivalReservationConfiguration` dan migrasi database siap dieksekusi | ✅ Terpenuhi | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgPreArrivalReservationConfiguration.cs` |
| 4 | Endpoint `POST /pre-arrival-reservations` berhasil membuat reservasi baru dengan nomor reservasi unik berformat `RES-EMG-yyMMddHHmmss-XXXXXX` | ✅ Terpenuhi | `EmergencyPreArrivalReservationService.CreateReservationAsync` dan `EmergencyPreArrivalReservationController.Create` |
| 5 | Endpoint `GET /pre-arrival-reservations` dan `GET /{id}` mengembalikan daftar dan detail reservasi dengan filter status dan pencarian | ✅ Terpenuhi | `EmergencyPreArrivalReservationController.GetAll` & `GetById` |
| 6 | Endpoint `POST /{id}/cancel` memvalidasi alasan pembatalan dan memperbarui status reservasi menjadi `Cancelled` (3) | ✅ Terpenuhi | `EmergencyPreArrivalReservationController.Cancel` |
| 7 | Endpoint `POST /{id}/check-in` memvalidasi status `PendingArrival`, otomatis membuat / mempromosikan kunjungan `EmgVisit`, menautkan `ResultEmergencyVisitId`, dan memperbarui status reservasi menjadi `Arrived` (2) | ✅ Terpenuhi | `EmergencyPreArrivalReservationController.CheckIn` & `EmergencyPreArrivalReservationService.CheckInReservationAsync` |
| 8 | Build backend `dotnet build -p:RunAnalyzers=false` lulus 0 error, QBE Strict PASS, nol komentar baru di kode | ✅ Terpenuhi | Build succeeded (0 errors, 10 warnings NuGet baseline), QBE Strict PASS (0 violation) |
