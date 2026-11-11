# Laporan Penyelesaian Backend Task: BE-HRD-183

## 1. Identitas Task
* **ID Task**: `BE-HRD-183`
* **Nama Task**: Penegakan batas usia anak $\le 21$ tahun non-mahasiswa & $\le 25$ tahun mahasiswa aktif berlampiran surat dekanat (`BEN-B3`, `VR-BEN-05`..`08`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-37: Administrasi Asuransi, Tanggungan Keluarga ESS & Verifikasi Berkas Fisik`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam kebijakan jaminan kesehatan ketenagakerjaan dan asuransi rumah sakit, pertanggungan anak memiliki batas usia kedewasaan mandiri. Sistem Benefit RS Quilvian menetapkan aturan batasan usia yang ketat berbasis bukti akademik:

1. **Batas Usia Standar Anak Non-Mahasiswa (`BEN-B3`, `VR-BEN-05`)**:
   - Anak kandung/adopsi sah yang tidak berstatus mahasiswa aktif hanya berhak ditanggung hingga usia maksimal **21 tahun 0 bulan** pada tanggal pendaftaran atau audit tahunan.
   - Pendaftaran anak non-kuliah di atas usia 21 tahun ditolak otomatis dengan pesan: *"Pendaftaran anak ditolak. Batas usia tanggungan anak non-mahasiswa adalah maksimal 21 tahun (VR-BEN-05, BEN-B3)."*
2. **Perpanjangan Hak Bagi Mahasiswa Aktif Hingga 25 Tahun (`BEN-B3`, `VR-BEN-06`)**:
   - Jika anak berstatus mahasiswa aktif (`IsActiveStudent = true`), hak pertanggungan dapat diperpanjang hingga maksimal usia **25 tahun 0 bulan**.
   - Pendaftaran anak yang telah berusia di atas 25 tahun (meskipun masih kuliah) ditolak dengan pesan: *"Pendaftaran anak ditolak. Batas usia maksimal anak berstatus mahasiswa aktif adalah 25 tahun (VR-BEN-06, BEN-B3)."*
3. **Kewajiban Surat Keterangan Mahasiswa Aktif dari Dekanat (`VR-BEN-07`)**:
   - Untuk anak berusia di atas 21 tahun, pegawai wajib mengunggah salinan resmi Surat Keterangan Mahasiswa Aktif dari dekanat perguruan tinggi (`StudentVerificationLetterPath`).
   - Pendaftaran anak tanpa lampiran surat dekanat ditolak dengan pesan: *"Surat Keterangan Mahasiswa Aktif dari perguruan tinggi wajib dilampirkan untuk anak usia di atas 21 tahun (VR-BEN-07)."*
4. **Masa Berlaku Surat Kuliah Maksimal 31 Agustus (`VR-BEN-08`)**:
   - Surat keterangan mahasiswa aktif hanya berlaku untuk satu tahun ajaran akademik dan kedaluwarsa per tanggal **31 Agustus** tahun ajaran berjalan (`StudentVerificationExpiryDate`).
   - Jika masa berlaku telah terlewati, sistem menolak pendaftaran atau menandai berkas kedaluwarsa.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeBenefitDependent.cs`
* Kolom pendukung validasi batas usia dan perkuliahan:
  - `IsActiveStudent` (BOOLEAN, default `false`).
  - `StudentVerificationLetterPath` (VARCHAR(255), path berkas surat dekanat).
  - `StudentVerificationExpiryDate` (TIMESTAMPTZ, tanggal kedaluwarsa surat).

### B. Perhitungan Umur Deterministik pada Service
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitEnrollmentService.cs`
* Metode: `CreateEssDependentAsync`.
* Formula perhitungan umur akurat per tanggal hari ini:
  ```csharp
  var ageYears = today.Year - birthDate.Year;
  if (birthDate.Date > today.AddYears(-ageYears)) ageYears--;
  ```
* Evaluasi kombinasi `ageYears`, flag `IsActiveStudent`, keberadaan surat dekanat, dan tanggal kedaluwarsa surat kuliah.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Anak Berusia 22 Tahun Kuliah di Fakultas Kedokteran**:
  - Ns. Anwar mendaftarkan anaknya, Bima, yang lahir 15 Agustus 2004 (usia 22 tahun).
  - Anwar mencentang `IsActiveStudent = true`, mengunggah Surat Dekanat FK UI, dan menetapkan tanggal akhir semester `StudentVerificationExpiryDate = 31 Agustus 2027`.
  - Sistem memvalidasi usia $22 \le 25$, surat dekanat terlampir, dan tanggal kedaluwarsa belum lewat. Berkas berhasil disimpan dengan status `Submitted` untuk diverifikasi HR.

* **Skenario 2 — Anak Berusia 22 Tahun Tidak Kuliah**:
  - Anwar mencoba mendaftarkan anak keduanya yang berusia 22 tahun tetapi sudah bekerja / tidak kuliah (`IsActiveStudent = false`).
  - Sistem langsung menolak pengajuan dengan pesan `VR-BEN-05`.

* **Skenario 3 — Anak Berusia 26 Tahun Masih Kuliah S2**:
  - Dokter Suryo mencoba mendaftarkan putranya yang berusia 26 tahun yang sedang menempuh pendidikan spesialis/magister.
  - Sistem menolak pengajuan dengan pesan `VR-BEN-06` karena batas usia absolut pertanggungan asuransi korporat adalah 25 tahun.

---

## 5. Verifikasi & Integritas Bukti
* **Pencegahan Beban Klaim Non-Eligible**: Mencegah anak yang telah dewasa/mandiri membebani plafon asuransi rumah sakit tanpa bukti studi aktif.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-06` dan `AT-HRD-BEN-07`.
