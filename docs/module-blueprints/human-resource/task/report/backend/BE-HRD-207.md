# Laporan Penyelesaian Task: BE-HRD-207 DTOs & Request/Response Models Lisensi, Peer Review & SPK-RKK

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-207`
* **Nama Task**: Penyusunan Data Transfer Objects (DTOs), Validasi Bisnis & Model Permintaan/Respons Lisensi Nakes, Audit PSV & Radar Kedaluwarsa
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/DTOs/CredentialLicenseDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/CredentialingEnums.cs`

---

## 2. Latar Belakang & Aturan Bisnis Validasi
Kompleksitas regulasi ketenagakerjaan nakes di Indonesia menuntut validasi data yang sangat presisi sebelum data lisensi disimpan ke basis data:
1. **Validasi STR vs SIP (`VR-CRD-01`)**:
   - Jika jenis lisensi adalah `STR`: Sistem mengizinkan `IsLifetimeStr = true` tanpa tanggal kedaluwarsa. Namun jika bukan seumur hidup, `ExpiredDate` wajib diisi dan harus lebih besar dari `IssueDate`.
   - Jika jenis lisensi adalah `SIP`: Sistem **melarang keras** `IsLifetimeStr = true`. SIP wajib berjangka waktu 5 tahun dengan nomor tempat praktik `SIP-1`, `SIP-2`, atau `SIP-3` (maksimal 3 faskes sesuai UU Praktik Kedokteran).
2. **DTO Audit Sumber Primer (PSV - Primary Source Verification)**: Memuat payload verifikasi ke portal resmi (KKI / SatuSehat SDMK Kemenkes), URL bukti pengecekan, catatan auditor, dan status keabsahan.
3. **DTO Radar Peringatan Kedaluwarsa SIP 5 Tahun**: Menghitung sisa hari (`DaysRemaining`), menentukan tier radar (H-90, H-60, H-30, H-7, Expired H-0), serta menyajikan instruksi tindakan preventif dan status pemblokiran SIMRS/Roster.

---

## 3. Rincian Kelas DTO yang Diimplementasikan

### 1. `RegisterProfessionalLicenseDto` (Implements `IValidatableObject`)
* Mendaftarkan STR Seumur Hidup atau SIP 5 tahun.
* Logika validasi runtime:
  - Mencegah `IsLifetimeStr = true` pada SIP.
  - Memastikan tanggal kedaluwarsa valid pada lisensi non-seumur-hidup.
  - Memvalidasi format nomor faskes SIP (`SIP-1`, `SIP-2`, `SIP-3`).

### 2. `PsvVerificationRequestDto` & `PsvVerificationResultDto`
* Memfasilitasi auditor kredensial rumah sakit dalam merekam hasil verifikasi keaslian STR/SIP langsung dari sumber primer SatuSehat SDMK Kemenkes atau KKI.

### 3. `ProfessionalLicenseDto`
* Menyajikan rincian lengkap lisensi nakes beserta data profil nakes (`DisplayName`, `ProfileCode`, `PositionName`, `DepartmentName`), status verifikasi PSV, tautan bukti cek, dan status radar kedaluwarsa.

### 4. `RenewProfessionalLicenseDto`
* Mengakomodasi perpanjangan SIP 5 tahun yang telah habis masa berlakunya dengan tanggal terbit baru dan nomor registrasi dinas kesehatan yang baru.

### 5. `LicenseRadarAlertDto`
* Digunakan oleh dasbor pemantauan eksekutif dan modul notifikasi untuk menampilkan nakes yang masa berlaku lisensinya berada pada tier peringatan H-90, H-60, H-30, H-7, atau H-0 (Fail-Closed).

---

## 4. Verifikasi Kompilasi & Kepatuhan Kontrak
* **Status Kompilasi**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Kepatuhan Kontrak**: Seluruh struktur field selaras 100% dengan Kontrak API v18 pada dokumen blueprint `03-api-and-integration-contracts.md` Bagian 30.2.
