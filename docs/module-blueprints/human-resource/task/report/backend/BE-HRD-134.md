# Laporan Penyelesaian Task Backend: BE-HRD-134

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-134` |
| **Nama Task** | Service SPKL pra-dinas terencana H-1 & penugasan kelompok tim medis (`OvertimePlanService`) |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-116`, `HRD-ASM-26`, `02-backend-architecture.md` 22.3, `contracts/api-contract.md` Tag `[Tags("HR Overtime - Plans & ESS Requests")]` |

---

## 2. Deskripsi & Tujuan Bisnis

Surat Perintah Kerja Lembur (SPKL) terencana pra-dinas merupakan instrumen formal yang memastikan setiap penugasan lembur di rumah sakit memiliki dasar pembenaran operasional pelayanan yang sah sebelum pekerjaan fisik dilaksanakan.

Task ini mengimplementasikan service dan controller pengajuan SPKL terencana dengan kapabilitas:
1. **Penomoran Otomatis Baku**: Menghasilkan nomor registrasi SPKL unik per bulan dengan format `SPKL/YYYY/MM/XXXX` (misal `SPKL/2026/11/0001`).
2. **Penugasan Lembur Kelompok Tim Medis (`HRD-ASM-26`)**: Mendukung pengajuan satu dokumen SPKL induk yang memuat banyak staf pelaksana (`TrxOvertimePlanDetail`), seperti tim operasi IBS, tim resusitasi IGD, atau tim penunjang laboratorium/radiologi.
3. **Pencegahan Konflik Cuti Aktif (`VR-OT-18`)**: Memeriksa silang ke modul cuti Sub-08 (`WfpLeaveRequest`) untuk memastikan tidak ada anggota tim medis yang sedang dalam jadwal cuti aktif yang disetujui pada tanggal dinas lembur.
4. **Penegakan Kuota Otomatis (`VR-OT-04`)**: Mengintegrasikan `OvertimeQuotaEnforcementGuard` untuk memvalidasi batas akumulasi harian (4 jam) dan mingguan (18 jam) PP 35/2021 sebelum SPKL disimpan.
5. **Pembatalan Pra-Dinas Resmi (`VR-OT-23`)**: Mengizinkan pembatalan rencana lembur hanya sebelum jam pelaksanaan dinas dimulai (`DateTime.UtcNow < PlanStartDate`).

---

## 3. Komponen yang Dikerjakan

1. **Interface & Service (`IOvertimePlanService.cs` & `OvertimePlanService.cs`)**:
   - `GetMyPlansAsync`: Kueri daftar SPKL staf aktif dengan pagination dan filter status/periode.
   - `GetPlanByIdAsync`: Mengambil rincian SPKL dan daftar nakes pelaksana.
   - `CreatePlanAsync`: Validasi FluentValidation, evaluasi konflik cuti `VR-OT-18`, evaluasi kuota PP 35/2021 `VR-OT-04`, pembentukan sequence nomor SPKL, dan penyimpanan entitas induk-rincian.
   - `CancelPlanAsync`: Validasi batas waktu jam mulai `VR-OT-23` dan pembaruan status ke `Cancelled`.

2. **Controller Bergaya Swagger (`OvertimePlanEssController.cs`)**:
   - Tag Swagger: `[Tags("HR Overtime - Plans & ESS Requests")]`.
   - Rute ganda: `/api/v1/corporate/human-resource/overtime/plans` dan `/api/overtime/plans`.
   - Endpoint:
     - `GET /my`: Daftar SPKL staf aktif (`[AccessPermission("OvertimePlan", "ViewSelf")]`).
     - `GET /{id}`: Detail rincian SPKL (`[AccessPermission("OvertimePlan", "View")]`).
     - `POST /`: Pengajuan SPKL pra-dinas terencana (`[AccessPermission("OvertimePlan", "Create")]`).
     - `POST /{id}/cancel`: Pembatalan rencana SPKL (`[AccessPermission("OvertimePlan", "CancelSelf")]`).

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario: Penugasan Lembur Tim Hemodialisa
* **Kondisi**: Kepala Ruangan Hemodialisa mengajukan SPKL untuk 2 perawat pada hari Minggu untuk menangani lonjakan pasien cuci darah darurat.
* **Proses Sistem**:
  1. Kepala Ruangan mengirim `POST /api/v1/corporate/human-resource/overtime/plans` dengan daftar 2 perawat.
  2. Sistem memverifikasi kedua perawat tidak sedang cuti (`VR-OT-18`) dan kuota mingguannya < 18 jam (`VR-OT-04`).
  3. Sistem menerbitkan nomor `SPKL/2026/11/0002` berstatus `WaitingApproval`.
  4. SPKL otomatis muncul pada kotak masuk approval atasan hierarki.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| SPKL terencana menghasilkan nomor registrasi otomatis format `SPKL/YYYY/MM/XXXX` | ✅ Lolos | Diimplementasikan via generator `SPKL/{year}/{month:D2}/{sequence:D4}`. |
| Satu nomor SPKL induk tim medis memuat banyak detail staf pelaksana | ✅ Lolos | Entitas `TrxOvertimePlan` menyimpan relasi koleksi `TrxOvertimePlanDetail`. |
| Status awal pengajuan adalah `WaitingApproval` | ✅ Lolos | Diinisialisasi dengan `PlanStatus = "WaitingApproval"` dan `DetailStatus = "WaitingApproval"`. |
| Cek konflik cuti aktif sehari penuh menolak dengan HTTP 409 Conflict | ✅ Lolos | Evaluasi query `_context.Set<WfpLeaveRequest>()` mengembalikan 409 Conflict bila bertabrakan cuti disetujui (`VR-OT-18`). |
| Pembatalan setelah jam pelaksanaan dinas dimulai ditolak dengan HTTP 400 Bad Request | ✅ Lolos | Diuji via `DateTime.UtcNow >= planStart` mengembalikan pesan baku `VR-OT-23`. |
