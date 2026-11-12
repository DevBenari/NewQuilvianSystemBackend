# Laporan Penyelesaian Task: BE-HRD-206 Model EF Core, Enums & Migrasi PostgreSQL Kredensialing

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-206`
* **Nama Task**: Pembuatan Model EF Core, Enums, Perluasan Entitas Lisensi & Migrasi PostgreSQL Credentialing & Clinical Privilege
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/CredentialingEnums.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/MstCredentialingCommittee.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxSpkRkkIssuance.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/TrxClinicalPrivilegeEmergencyOverrideLog.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpCredentialLicense.cs` (diperluas)
  - `NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs` (registrasi DbSet)
  - `NewQuilvianSystemBackend/Migrations/20261109150000_AddCredentialingAndClinicalPrivilegeEnhancements.cs` (berkas migrasi)

---

## 2. Latar Belakang & Kebutuhan Regulasi Kredensial Rumah Sakit
Dalam operasional pelayanan kesehatan rumah sakit di Indonesia, keselamatan pasien (*Patient Safety*) sangat bergantung pada kepastian bahwa setiap dokter, perawat, dan tenaga kesehatan lainnya hanya melakukan tindakan medis sesuai batas kompetensi yang telah diuji dan disahkan secara hukum:
1. **Diferensiasi Regulasi STR Seumur Hidup vs SIP 5 Tahun (`VR-CRD-01`)**: Sesuai amanat Undang-Undang Republik Indonesia No. 17 Tahun 2023 tentang Kesehatan, Surat Tanda Registrasi (STR) tenaga medis dan kesehatan dapat berstatus seumur hidup. Sebaliknya, Surat Izin Praktik (SIP) **wajib berjangka waktu 5 tahun** dan terikat pada tempat praktik faskes tertentu (maksimal 3 tempat praktik/faskes: SIP-1, SIP-2, atau SIP-3). SIP dilarang keras berstatus seumur hidup.
2. **Master Tiga Komite Kredensial Mandiri (`CRD-B1`)**: Rumah sakit wajib memisahkan tata kelola kredensialing ke dalam 3 komite mandiri:
   - Komite Medik: Dokter Spesialis, Subspesialis, Dokter Umum, Dokter Gigi.
   - Komite Keperawatan: Ners, Perawat Vokasi, Bidan.
   - Komite Tenaga Kesehatan Lainnya: Apoteker, TTK, Radiografer, ATLM, Fisioterapis, Nutrisionis/Dietisien, Perekam Medis.
3. **Surat Penugasan Klinis & Rincian Kewenangan Klinis (SPK-RKK, `CRD-B3`)**: Kewenangan klinis diterbitkan oleh Direktur Utama berjangka waktu 3 tahun, dilengkapi pengesahan digital e-Sign dan QR Code.
4. **Audit Bypass Tindakan Darurat Penyelamatan Nyawa (*Life-Saving Emergency Override Log*, `INT-HR-70`)**: Dalam kondisi darurat bencana atau henti jantung, dokter diizinkan melakukan tindakan di luar RKK regulernya, namun sistem secara mandatori mencatat log audit real-time yang ditembuskan ke Komite Mutu & Keselamatan Pasien (KMKP).

---

## 3. Skenario Riil Rumah Sakit
* **Skenario Registrasi Dokter Bedah Baru**: Dr. Hendra Spesialis Bedah Onkologi bergabung ke RS Quilvian. Beliau menyerahkan STR seumur hidup KKI No. `STR-KKI-2023-88992` (`IsLifetimeStr = true`) dan SIP-1 dari Dinas Kesehatan Kota No. `SIP-503/001/DS/2024` yang berlaku 5 tahun (`ExpiredDate = 2029-05-10`). Sistem memvalidasi bahwa SIP terikat pada faskes RS Quilvian Utama sebagai tempat praktik pertama.
* **Skenario Pelaksanaan Sidang Pleno Komite**: Komite Medik membentuk Tim Kredensial yang terdiri dari 1 Ketua Sub-Komite dan 2 orang Mitra Bestari (*Peer Reviewer*) sebidang bedah onkologi untuk menguji kompetensi Dr. Hendra.
* **Skenario Penerbitan SPK-RKK**: Setelah sidang pleno dan rekomendasi diterbitkan, Direktur Utama menandatangani Surat Penugasan Klinis (SPK) No. `SPK/DIR/MED/XI/2026/0014` dengan lampiran Rincian Kewenangan Klinis (RKK) yang berlaku selama 3 tahun.

---

## 4. Matriks Entitas Database & Skema Perubahan

| Nama Tabel / Entitas | Tipe Skema | Peran dan Penjelasan Fungsional | Hubungan & Relasi |
| :--- | :---: | :--- | :--- |
| `WfpCredentialLicense` | Perluasan Tabel | Menyimpan lisensi STR & SIP nakes, audit PSV Kemenkes/KKI, nomor urut faskes SIP (SIP-1 s.d. 3), dan status radar kedaluwarsa H-90 s.d. H-0. | Relasi ke `MstWorkforceProfile` |
| `MstCredentialingCommittee` | Tabel Baru | Master 3 komite kredensial rumah sakit (Medik, Keperawatan, Nakes Lain) beserta struktur ketua, sekretaris, dan minimal 2 Mitra Bestari. | Master tata kelola kredensial |
| `TrxSpkRkkIssuance` | Tabel Baru | Transaksi pencatatan Surat Penugasan Klinis (SPK) dan Rincian Kewenangan Klinis (RKK) resmi ber-e-Sign Direktur Utama dengan siklus 3 tahun. | Relasi ke `MstWorkforceProfile`, `MstCredentialingCommittee` |
| `TrxClinicalPrivilegeEmergencyOverrideLog` | Tabel Baru | Pencatatan audit trail bypass tindakan klinis gawat darurat demi keselamatan nyawa pasien (*Life-Saving Override*) ke Komite Mutu (KMKP). | Relasi ke `MstWorkforceProfile`, `WfpClinicalPrivilege` |

---

## 5. Verifikasi Kompilasi & Kesiapan Migrasi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Berkas Migrasi**: `Migrations/20261109150000_AddCredentialingAndClinicalPrivilegeEnhancements.cs` telah siap diaplikasikan ke database PostgreSQL target secara mandiri oleh tim administrator basis data.
