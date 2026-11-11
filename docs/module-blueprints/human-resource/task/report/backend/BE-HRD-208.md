# Laporan Penyelesaian Task: BE-HRD-208 Service Verifikasi Sumber Primer (PSV) Kemenkes/KKI & Alur Registrasi Lisensi

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-208`
* **Nama Task**: Pembangunan Layanan Bisnis Verifikasi Sumber Primer (PSV) Terintegrasi Portal Kemenkes/KKI & Pengelolaan Siklus Hidup Lisensi Nakes
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Services/ICredentialVerificationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialVerificationService.cs`

---

## 2. Latar Belakang & Kebutuhan Standar Akreditasi Rumah Sakit
Standar Akreditasi Rumah Sakit Kementerian Kesehatan (STARKES) dan *Joint Commission International* (JCI) mewajibkan bahwa setiap rumah sakit melakukan **Primary Source Verification (PSV)** untuk seluruh ijazah, STR, dan SIP staf medis:
1. **Mandatori PSV Sebelum Pengajuan Kredensial (`HRD-DEC-146`, `VR-CRD-02`)**: Seorang dokter atau perawat tidak boleh diproses permohonan kredensialing atau diterbitkan kewenangan klinisnya sebelum STR dan SIP-nya diverifikasi keabsahannya langsung ke sumber penerbit primer (SatuSehat SDMK Kemenkes atau Konsil Kedokteran Indonesia / KTKI).
2. **Pencatatan Audit Trail Forensik (`PsvAuditLogJson`)**: Setiap kegiatan audit verifikasi merekam identitas auditor, tanggal/jam verifikasi, URL sumber pencarian, status keabsahan, dan catatan pemeriksa dalam format JSON terstruktur yang tidak dapat dimanipulasi.
3. **Eksekusi Tindakan Fail-Closed Otomatis H-0 (`INT-HR-69`, `INT-HR-71`)**: Ketika masa berlaku SIP habis, metode `ExecuteFailClosedForExpiredLicensesAsync` secara otomatis menangguhkan kewenangan klinis aktif nakes (`ClinicalPrivilegeStatus.Suspended`), memblokir order medis di SIMRS (`IsClinicalServiceBlocked = true`), serta menutup penugasan jadwal di roster Sub-06 Scheduling (`IsSchedulingBlocked = true`).

---

## 3. Skenario Riil Operasional Rumah Sakit

### Skenario Verifikasi Sumber Primer (PSV)
1. Analis Kredensial Rumah Sakit (Ibu Ratna) menerima berkas STR Dokter Spesialis Penyakit Dalam (dr. Budi).
2. Ibu Ratna membuka portal SatuSehat SDMK Kemenkes RI dan memasukkan nomor STR dr. Budi.
3. Portal menampilkan data valid bahwa dr. Budi terdaftar resmi dengan status aktif.
4. Ibu Ratna menekan tombol *Verifikasi Sumber Primer (PSV)* di sistem Quilvian, menempelkan URL bukti SatuSehat, dan mencatat nomor registrasi.
5. Sistem mengeksekusi `PerformPrimarySourceVerificationAsync`:
   - Menandai `IsPsvVerified = true` dan `VerificationStatus = CredentialVerificationStatus.Verified`.
   - Merekam log JSON forensik dan menstempel user ID Ibu Ratna sebagai auditor resmi.
   - Status berkas dr. Budi kini berstatus *Siap Diajukan ke Komite Medik*.

---

## 4. Rincian Metode Layanan `ICredentialVerificationService`

| Nama Metode Layanan | Deskripsi Fungsional & Aturan Bisnis |
| :--- | :--- |
| `RegisterLicenseAsync` | Mendaftarkan lisensi baru dengan validasi kepatuhan STR seumur hidup vs SIP 5 tahun (`VR-CRD-01`). Menetapkan status awal PSV ke `PendingVerification`. |
| `PerformPrimarySourceVerificationAsync` | Melakukan audit PSV ke sumber primer (SatuSehat SDMK / KKI), memperbarui status verifikasi, dan mencatat log audit forensik (`PsvAuditLogJson`). |
| `GetLicenseDetailAsync` | Mengambil detail lisensi lengkap beserta kalkulasi status radar kedaluwarsa H-90 s.d. H-0. |
| `GetLicensesByEmployeeAsync` | Menampilkan seluruh lisensi (STR dan portofolio SIP 1 s.d. 3) milik satu tenaga kerja tertentu. |
| `RenewLicenseAsync` | Memperbarui masa berlaku SIP 5 tahun baru dan secara otomatis me-reset status PSV ke `PendingVerification` agar diaudit kembali keabsahannya. |
| `GetLicenseExpiringRadarListAsync` | Mengambil daftar lisensi yang berada pada ambang batas kedaluwarsa (<= 90 hari) dengan filter tingkat urgensi radar. |
| `ExecuteFailClosedForExpiredLicensesAsync` | Mengeksekusi penonaktifan darurat (Fail-Closed) bagi nakes yang SIP-nya telah kedaluwarsa: memblokir order SIMRS (`INT-HR-69`) dan penugasan roster dinas Sub-06 (`INT-HR-71`). |

---

## 5. Verifikasi Kompilasi & Keamanan Data
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Keamanan Audit**: Data audit log forensik tersimpan dalam format JSON dan terikat pada relasi pengguna sistem ASP.NET Core Identity.
