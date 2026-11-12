# Laporan Penyelesaian Backend Task: BE-HRD-172

## 1. Identitas Task
* **ID Task**: `BE-HRD-172`
* **Nama Task**: Layanan penerbitan voucher kasir darurat medis cito nakes ber-token SHA-256 kedaluwarsa 48 jam
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-35 & Verifikasi: Darurat Kasir 24/7, Exit Clearance & Verifikasi Akhir`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam kondisi darurat medis staf tenaga medis (misalnya insiden pajanan jarum suntik terkontaminasi/needle stick injury, kecelakaan saat dinas ambulans, atau tindakan cito IGD di luar faskes RS Quilvian), nakes tidak dapat menunggu jadwal transfer gaji bulanan tanggal 25 untuk penggantian biaya. Rumah sakit menyediakan jalur cepat pencairan tunai langsung melalui Loket Kasir RS 24/7 (`HRD-DEC-133`).

Layanan `IssueVoucherAsync` menyediakan mekanisme penerbitan voucher kasir elektronik dengan batasan keamanan tinggi:
1. **Khusus Klaim Darurat Cito Approved (`VR-DED-13`)**: Voucher hanya dapat diterbitkan untuk klaim yang telah melewati persetujuan manajemen HRD (`ClaimStatus = "Approved"`) dan memiliki penandaan kedaruratan medis nakes (`IsCitoEmergency = true`). Pengajuan klaim reguler ditolak otomatis untuk mencegah penyalahgunaan kas fisik kasir.
2. **Pembangkitan Kriptografi SHA-256 Satu Kali Pakai**: Sistem membangkitkan token acak kriptografis 16-byte (`RandomNumberGenerator`), menyimpan representasi hash SHA-256 di database (`SecurityTokenHash`), dan mengembalikan token teks biasa satu kali kepada pemohon untuk diverifikasi oleh petugas kasir.
3. **Masa Berlaku Kedaluwarsa Tepat 48 Jam (`VR-DED-14`)**: Voucher memiliki batas waktu klaim tepat 48 jam sejak diterbitkan (`ExpiredAt = now.AddHours(48)`). Jika melewati batas waktu tanpa dicairkan, status otomatis kedaluwarsa (`Expired`).
4. **Pencegahan Penerbitan Ganda**: Klaim yang telah memiliki voucher aktif (`VoucherIssued`) atau telah dicairkan (`DisbursedViaCashier`) diblokir keras dari penerbitan ulang voucher.
5. **Peringatan Dini Batas Waktu (`DED-W3`)**: Jika voucher belum dicairkan dan waktu tersisa $\le 12$ jam (sudah $> 36$ jam berlalu), sistem mengaktifkan penanda `IsExpiringSoonWarning` sebagai sinyal pengingat ke pegawai.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/CashierVoucherDtos.cs`
* `IssueCashierVoucherRequestDto`:
  - `ClaimId`: ID klaim restitusi darurat cito yang telah disetujui.
  - `Notes`: Catatan disposisi penerbitan voucher.
* `CashierVoucherResponseDto`:
  - `Id`: ID entitas voucher kasir.
  - `VoucherNumber`: Nomor unik voucher (`VCH-YYYYMM-XXXXXX`).
  - `ClaimId`, `ClaimNumber`, `ClaimCategory`: Referensi klaim restitusi.
  - `EmployeeId`, `EmployeeName`, `EmployeeNik`: Data nakes penerima dana.
  - `VoucherAmount`: Nominal dana tunai yang dapat dicairkan di kasir.
  - `SecurityToken`: Token otentikasi satu kali pakai (32 karakter hex).
  - `IssuedAt`, `ExpiredAt`: Rentang waktu keabsahan voucher (48 jam).
  - `VoucherStatus`: Status voucher (`Issued`).
  - `IsExpiringSoonWarning`: Indikator waktu kritis mendekati kedaluwarsa.
  - `RemainingHours`: Sisa jam aktif voucher.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/ICashierVoucherDisbursementService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/CashierVoucherDisbursementService.cs`
Metode: `IssueVoucherAsync`, `GetVoucherByIdAsync`, `GetVouchersAsync`

### C. Controller REST API
File: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/CashierVoucherController.cs`
Tag Swagger: `[Tags("HR Deductions - Cashier Vouchers & Schedules")]`

---

## 4. Spesifikasi Endpoint REST API (Swagger)

| Method | Path | Deskripsi | Otorisasi | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/vouchers` | Penerbitan voucher kasir darurat medis cito nakes 24/7 ber-token SHA-256 (48 jam) | Bearer JWT | `IssueCashierVoucherRequestDto` | `CashierVoucherResponseDto` |
| `GET` | `/api/hr-deductions/vouchers` | Mengambil daftar voucher kasir darurat dengan filter status dan tanggal | Bearer JWT | `CashierVoucherFilterDto` | `List<CashierVoucherResponseDto>` |
| `GET` | `/api/hr-deductions/vouchers/{id}` | Mengambil detail voucher kasir berdasarkan ID | Bearer JWT | `id: Guid` | `CashierVoucherResponseDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Insiden Tertusuk Jarum Nakes IGD Shift Malam**:
  - **Kondisi**: dr. Hendra (Dokter IGD) tertusuk jarum saat resusitasi pasien infeksius pukul 02:00 dini hari. Diperlukan tindakan profilaksis pasca-pajanan (PPP) dan obat segera di fasilitas rekanan senilai Rp 2.500.000.
  - **Pengajuan**: dr. Hendra mengajukan klaim darurat di ESS dengan opsi `IsCitoEmergency = true`. Supervisor IGD dan Manajer Medis langsung menyetujui klaim via approval cepat.
  - **Penerbitan Voucher**: HR Officer menerbitkan voucher via `POST /api/hr-deductions/vouchers`:
    * Voucher Nomor: `VCH-202611-9F3C12`
    * Token Keamanan: `A9C7882FE1D692B8943C880193E07F81`
    * Masa Berlaku: 48 jam sejak terbit (hingga 8 November 2026 pukul 02:30).
  - **Hasil**: dr. Hendra menerima token SMS/notifikasi ESS dan langsung menuju Loket Kasir RS 24/7 untuk mencairkan uang tunai darurat.

---

## 6. Verifikasi & Integritas Bukti
* **Pencegahan Bypass Token**: Token asli tidak disimpan di basis data melainkan disimpan dalam bentuk hash SHA-256 (`ComputeSha256`), mencegah penyalahgunaan oleh administrator basis data.
* **Traceability Penuh**: Relasi one-to-one antara klaim darurat dengan entitas voucher `TrxCashierDisbursementVoucher` terikat kuat dan terindeks performa.
