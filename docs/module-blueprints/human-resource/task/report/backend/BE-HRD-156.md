# Laporan Penyelesaian Backend Task: BE-HRD-156

## 1. Identitas Task
* **ID Task**: `BE-HRD-156`
* **Nama Task**: Alur resolusi justifikasi audit variansi & gerbang penolakan submission batch (`PayrollVarianceResolutionService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-31: Integrasi Potongan Koperasi, Variance Guard, & Alur Otorisasi 3-Jenjang`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Setelah tiket anomali variansi dibangkitkan oleh Payroll Variance Guard, proses bisnis rumah sakit mewajibkan investigasi audit sebelum berkas diajukan ke level pimpinan. Layanan ini mengelola dua pilar integritas:
1. **Validasi Alasan Justifikasi Audit (`VR-PAY-15`)**:
   - Payroll Officer yang menangani batch wajib mengisi penjelasan formal mengenai penyebab deviasi (contoh: *"Kenaikan gaji diakibatkan tugas dinas back-up ruang isolasi PINERE selama 14 hari dan rapel lembur shift malam yang baru diverifikasi"*).
   - Sistem menegakkan batas minimal **20 karakter** secara ketat. Pengisian alasan yang terlalu singkat atau asal-asalan (misalnya *"lembur"*, *"sesuai"*, *"ok"*) akan ditolak langsung oleh API dengan kode **HTTP 400 Bad Request**.
   - Setelah justifikasi valid diterima, status alert berubah dari `PendingReview` menjadi `Justified`, lengkap dengan pencatatan stempel waktu dan identitas user yang bertanggung jawab (`ResolvedByUserId`, `ResolvedAt`).
2. **Gerbang Pemblokir Keras Submission Batch (`PAY-B3`, `VR-PAY-14`)**:
   - Sebelum batch penggajian dapat disubmit ke HR Manager (Tier 1 Approval), sistem menjalankan verifikasi gerbang (*pre-submission gate check*).
   - Apabila masih ditemukan bahkan **1 tiket alert variansi** yang berstatus `PendingReview`, sistem memblokir aksi submit secara mutlak dengan kode **HTTP 422 Unprocessable Entity**, sehingga mencegah pengajuan berkas yang belum tervalidasi.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollVarianceDtos.cs`
* DTO yang dibuat:
  - `ResolveVarianceAlertCommand`: Payload input justifikasi dengan anotasi validasi atribut `[MinLength(20)]` dan `[MaxLength(2000)]`.
  - `BatchVarianceSubmissionCheckDto`: Hasil verifikasi kelayakan submission (status kelolosan `CanBeSubmitted`, jumlah tiket pending, dan daftar ringkasan anomali yang masih menggantung).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollVarianceResolutionService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollVarianceResolutionService.cs`
* Logika Bisnis:
  1. Validasi String Justifikasi: Memverifikasi panjang input teks $\ge 20$ karakter setelah pemangkasan spasi (*trim*).
  2. Proteksi Immutability: Menolak justifikasi jika batch penggajian terkait telah berada dalam kondisi terkunci (`IsLocked == true`).
  3. Pembaruan Status Alert:
     - `alert.ResolutionStatus = "Justified"`
     - `alert.JustificationNotes = command.JustificationNotes.Trim()`
     - `alert.ResolvedByUserId = actorUserId`
     - `alert.ResolvedAt = DateTime.UtcNow`
  4. Pemeriksaan Gerbang Submission:
     - Mengueri seluruh tiket variansi aktif pada batch yang masih berstatus `PendingReview`.
     - Mengembalikan `CanBeSubmitted = false` beserta rincian nama staf jika ditemukan tiket yang belum selesai dijustifikasi.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/variances/{alertId}/resolve` | Berikan catatan justifikasi resmi pada 1 tiket alert variansi | 200 OK / 400 Bad Request / 422 Unprocessable | Update |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/{id}/variances/check-submission` | Pengecekan gerbang kelayakan pengajuan submit batch | 200 OK | Read |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Skenario Percobaan Pengisian Alasan Singkat**:
  - Payroll Officer memasukkan catatan: *"Kelebihan dinas malam"* (19 karakter).
  - Respon Sistem: API langsung menolak dengan `HTTP 400 Bad Request`:
    `"Alasan justifikasi variansi wajib minimal 20 karakter untuk audit compliance rumah sakit (VR-PAY-15)."`
* **Skenario Pengisian Lengkap & Lolos Gerbang**:
  - Payroll Officer merevisi catatan menjadi: *"Penambahan lembur intensif ICU akibat lonjakan pasien rujukan dan rapel penyesuaian masa kerja SPK perawat"* (112 karakter).
  - Respon Sistem: Berhasil di-resolve (`HTTP 200 OK`), status alert berubah menjadi `Justified`.
  - Saat diperiksa via endpoint `check-submission`:
    `CanBeSubmitted: true, Message: "Seluruh alert anomali variansi telah terjustifikasi. Batch siap diajukan untuk review manajer."`

---

## 5. Verifikasi Acceptance Criteria
* [x] **Validasi Panjang Justifikasi Minimum 20 Karakter (`VR-PAY-15`)**: Menolak tegas teks di bawah 20 karakter dengan HTTP 400.
* [x] **Perekaman Audit Trail Petugas**: Menyimpan ID officer, stempel waktu presisi UTC, dan catatan lengkap pada database.
* [x] **Gerbang Pemblokir Keras Submission (`PAY-B3`, `VR-PAY-14`)**: Mengunci tombol pengajuan submit ke manajer jika masih terdapat alert `PendingReview`.
* [x] **Zero Warning Kompilasi Baru**: Solusi backend lulus `dotnet build` dengan **0 Error(s)**.
