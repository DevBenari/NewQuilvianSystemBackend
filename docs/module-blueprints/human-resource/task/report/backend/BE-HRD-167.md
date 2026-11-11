# Laporan Penyelesaian Backend Task: BE-HRD-167

## 1. Identitas Task
* **ID Task**: `BE-HRD-167`
* **Nama Task**: Layanan pengajuan klaim restitusi mandiri di portal ESS (filter kuitansi < 60 hari & upload lampiran nota)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-34: Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam rangka mempercepat pelayanan kesejahteraan tenaga medis dan meniadakan birokrasi formulir kertas manual, portal Employee Self-Service (ESS) menyediakan fitur pengajuan klaim mandiri. Pegawai rumah sakit dapat mengajukan penggantian biaya kacamata kerja atau perawatan darurat luar rumah sakit kapan saja melalui gawai ponsel pintar maupun komputer kerja (`HRD-DEC-130`).

Namun, tata kelola akuntansi rumah sakit mewajibkan kepatuhan batas kedaluwarsa dokumen keuangan:
1. **Aturan Batas Kedaluwarsa Kuitansi $\le 60$ Hari (`VR-DED-09`)**: Nota/kuitansi asli yang diunggah tidak boleh bertanggal lebih dari 60 hari kalender sejak tanggal transaksi. Jika melewati batas 60 hari, pengajuan otomatis ditolak oleh sistem guna mencegah klaim basi tahun lampau.
2. **Kewajiban Bukti Lampiran Nota Asli Digital**: Setiap klaim wajib melampirkan tautan berkas foto/PDF kuitansi yang sah (`ReceiptAttachmentUrl`).
3. **Penandaan Jalur Cepat Darurat Nakes (*IsCitoEmergency*)**: Jika tenaga medis mengalami kecelakaan kerja atau tertusuk jarum/paparan patogen cito yang butuh penanganan instan di luar RS, klaim dapat ditandai *Cito* (`HRD-DEC-133`) untuk diarahkan ke voucher kasir 24/7.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `SubmitRestitutionClaimRequestDto`:
  - `EmployeeId`: ID pegawai pemohon.
  - `ClaimCategory`: Kategori klaim (`KacamataKerja`, `FaskesLuarDarurat`, `DinasCitoMedis`).
  - `ExpenseDate`: Tanggal transaksi kuitansi.
  - `ClaimAmount`: Nominal uang yang diajukan penggantian.
  - `ReceiptAttachmentUrl`: Tautan file bukti kuitansi (PDF/JPG/PNG).
  - `MedicalResumeAttachmentUrl`: Resume medis dokter (opsional).
  - `ReasonNotes`: Keterangan alasan pengajuan.
  - `IsCitoEmergency`: Flag darurat medis.
* `RestitutionClaimResponseDto`: Berkas klaim lengkap mencakup nomor registrasi unik `CLM-YYYYMM-XXXXXX`, status alur, dan jejak waktu.
* `RestitutionClaimFilterDto`: Parameter filter pencarian daftar klaim.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode: `SubmitClaimAsync`, `GetClaimsAsync`, `GetClaimByIdAsync`
Alur Logika Validasi:
1. Validasi Tanggal: Menolak kuitansi di masa depan (`request.ExpenseDate > now`).
2. Validasi Batas Waktu 60 Hari (`VR-DED-09`):
   $$\text{DaysDifference} = (\text{now.Date} - \text{request.ExpenseDate.Date}).\text{TotalDays}$$
   Jika $\text{DaysDifference} > 60$, lempar `InvalidOperationException` dengan pesan audit baku.
3. Evaluasi Kelayakan & Kuota: Memastikan masa kerja staf mencukupi dan sisa kuota masih tersedia ($> 0$).
4. Registrasi Nomor Dokumen Unik: Pembangkitan kode registrasi `CLM-{Year}{Month:D2}-{RandomHex6}`.
5. Inisialisasi Status: Menetapkan status awal `Submitted` dan mencatat jejak audit pembuatan.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitution Claims")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/claims` | Pengajuan klaim restitusi mandiri oleh pegawai di ESS | Bearer JWT | `SubmitRestitutionClaimRequestDto` | `RestitutionClaimResponseDto` |
| `GET` | `/api/hr-deductions/claims` | Daftar seluruh klaim restitusi dengan filter kategori & status | Bearer JWT | `RestitutionClaimFilterDto` | `List<RestitutionClaimResponseDto>` |
| `GET` | `/api/hr-deductions/claims/{id}` | Detail satu berkas klaim restitusi berdasarkan ID | Bearer JWT | `id: Guid` | `RestitutionClaimResponseDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 (Pengajuan Berhasil Kacamata Apoteker)**:
  - Apt. Indah membeli kacamata baca resep pada tanggal 20 Oktober 2026 seharga Rp 1.100.000.
  - Diajukan di ESS pada tanggal 6 November 2026 (usia nota = 17 hari kalender $\le 60$ hari).
  - Hasil: Pengajuan sukses, sistem menerbitkan nomor klaim `CLM-202611-AB12CD` berstatus `Submitted` dan masuk ke daftar kerja supervisor.
* **Skenario 2 (Penolakan Otomatis Kuitansi Basi)**:
  - Staf mengunggah kuitansi optik tertanggal 1 Juli 2026 (usia nota = 128 hari).
  - Hasil: Sistem seketika menolak pengajuan dengan pesan HTTP 400: *"Pengajuan ditolak otomatis: Kuitansi bertanggal 2026-07-01 telah melebihi batas kedaluwarsa maksimal 60 hari kalender (VR-DED-09)."*
