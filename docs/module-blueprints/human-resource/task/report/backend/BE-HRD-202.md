# Laporan Penyelesaian Task: BE-HRD-202 Mesin Rendering Naskah Dinas PDF Berbasis Template Token Dinamis Liquid

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-202`
* **Nama Task**: Mesin Rendering Naskah Dinas PDF Berbasis Template Token Dinamis Liquid
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IEmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs`

---

## 2. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Sebagai institusi pelayanan kesehatan berstandar tinggi, Rumah Sakit Quilvian memerlukan format naskah dinas resmi kepegawaian yang rapi, profesional, seragam (*standardized branding*), dan dilengkapi identitas kop surat rumah sakit, nomor surat dinas terpusat, serta tanda tangan digital pejabat yang sah.

Sebelumnya, staf HR sering menyusun surat secara manual menggunakan aplikasi pengolah kata (*word processor*), yang menyebabkan:
- Format kop surat dan tata letak teks sering berbeda-beda antar staf HR.
- Token penomoran surat berisiko salah kutip atau tertukar.
- Lambatnya proses penerbitan surat karena staf harus menunggu penandatanganan basah secara fisik saat pejabat dinas sedang bertugas di luar kota atau melakukan tindakan operasi medis.

Task `BE-HRD-202` menghadirkan **Mesin Rendering Naskah Dinas Dinamis**:
1. **Template Token Dinamis**: Menggantikan token standar dalam template naskah dinas kepegawaian:
   - `[NOMOR_SURAT]`: Nomor resmi surat dinas dari mesin gapless numbering (`IDocumentAutoNumberingService`), misal `SKP/RSQ-SDM/XI/2026/000042`.
   - `[TANGGAL_SURAT]`: Tanggal penerbitan format Indonesia, misal `9 November 2026`.
   - `[NAMA_LENGKAP]`, `[NIP]`, `[JABATAN]`, `[UNIT_KERJA]`: Data identitas pegawai yang terintegrasi langsung dari Sub-02 Workforce Master Data.
   - `[GAJI_BRUTO]`, `[GAJI_NETTO]`: Nilai nominal penghasilan terformat rupiah (khusus surat finansial).
   - `[KEPERLUAN]`: Uraian keperluan pengajuan surat (misal pengajuan KPR / visa).
   - `[NAMA_PENANDATANGAN]`, `[JABATAN_PENANDATANGAN]`: Pejabat berwenang yang mengesahkan.
   - `[VERIFIKASI_URL]`: Tautan verifikasi keaslian dokumen publik.
2. **Penyimpanan Snapshot Dokumen & Checksum SHA-256**: Dokumen yang telah dirender disimpan ke disk storage rumah sakit (`Storage/documents/issued/`), dihitung ukuran byte (`FileSizeBytes`), dan dihitung *checksum* SHA-256 (`FileChecksum`) untuk mencegah perubahan isi dokumen di masa mendatang (*anti-tampering*).

---

## 3. Skenario Riil Rumah Sakit
* **Skenario**: dr. Anwar Sanusi, Sp.JP (Dokter Spesialis Jantung) mengajukan Surat Keterangan Bekerja Aktif untuk keperluan pendaftaran fellowship kardiologi intervensi.
* **Proses Rendering**:
  1. Manajer SDM menekan tombol "Otorisasi & Terbitkan Surat".
  2. Sistem mengambil nomor urut resmi: `SKK/RSQ-SDM/XI/2026/000055`.
  3. Mesin template menyusun dokumen lengkap berkop RS Quilvian:
     - Nama: dr. Anwar Sanusi, Sp.JP
     - NIP: 19840512-201501-1-042
     - Jabatan: Dokter Spesialis Jantung & Pembuluh Darah
     - Unit Kerja: Instalasi Pelayanan Jantung Terpadu
     - Keperluan: Persyaratan Pendaftaran Fellowship Kardiologi Intervensi
  4. Sistem menyimpan file HTML/PDF resmi ke server dan menghitung checksum SHA-256.
  5. dr. Anwar dapat langsung mengunduh surat resmi bertanda tangan digital melalui portal ESS dalam format siap cetak.

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Document Issuance & Verification")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/services/documents/{id}/download` | Mengunduh berkas fisik PDF/HTML naskah dinas resmi yang telah diotorisasi | `Authorize` (Staf pemilik atau HR Staff) | `200 OK` (File), `403 Forbidden`, `404 Not Found` |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/revoke` | Mencabut/membatalkan surat dinas resmi dengan nomor BAP pencabutan | `SuperAdmin`, `HRAdmin`, `HRManager` | `200 OK`, `400 Bad Request` |

---

## 5. Bukti Verifikasi & Keamanan
1. **Otorisasi Unduh Terproteksi**: Endpoint download memverifikasi bahwa pengunduh adalah pemilik dokumen yang sah (`WorkforceProfile.UserAccount.Id == currentUserId`) atau tim HR berwenang. Staf lain yang mencoba mengunduh ditolak dengan status `403 Forbidden`.
2. **Integritas Berkas (*File Checksum*)**: Kolom `FileChecksum` menyimpan hash SHA-256 isi berkas. Jika ada manipulasi teks berkas pada server, hash tidak akan cocok sehingga dokumen terdeteksi telah dimodifikasi.
3. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
