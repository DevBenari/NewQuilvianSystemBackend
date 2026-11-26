# Akun Bagian SDM — Database Dev

| Butir | Keterangan |
|---|---|
| Dibuat | 25 November 2026 |
| Lingkungan | **Database dev** `QuilvianNewDevHamzah` (backend lokal `https://localhost:7184`) |
| Kegunaan | Akun kerja Bagian SDM selama pengembangan, sekaligus akun pengujian per peran |
| Cara dibuat | Lewat API resmi aplikasi (jabatan, izin, pegawai + akun login, penempatan, atasan langsung), bukan SQL langsung |

> ⚠️ **Hanya untuk database dev.** Jangan buat akun dan sandi yang sama di staging atau produksi. Keempat
> akun memakai satu sandi bersama. Kalau sandi diganti, perbarui dokumen ini.

## 1. Daftar akun

| # | Nama | Login (email) | Sandi | No. Pegawai | Departemen / Jabatan | Unit | Atasan langsung | Peran |
|---|---|---|---|---|---|---|---|---|
| 1 | Hartini Sulistyowati, S.Psi., M.M. | `hartini.sulistyowati@rsmmc.local` | `Rsmmc#Sdm2026` | RSMMC-20150302001 | Human Resource / **Kepala Bagian SDM** | — | — | Penyetuju (Kepala SDM) |
| 2 | Yudha Hermawan, S.E. | `yudha.hermawan@rsmmc.local` | `Rsmmc#Sdm2026` | RSMMC-20210701001 | Human Resource / **Staf Administrasi Kepegawaian** | — | Hartini Sulistyowati | Pembuat (petugas personalia) |
| 3 | Ns. Endang Purwanti, S.Kep. | `endang.purwanti@rsmmc.local` | `Rsmmc#Sdm2026` | RSMMC-20120903001 | Keperawatan / Kepala Ruangan | Instalasi Rawat Inap (**pimpinan unit**) | — | Atasan langsung perawat Rawat Inap |
| 4 | Novi Ariyanti, A.Md.Kep. | `novi.ariyanti@rsmmc.local` | `Rsmmc#Sdm2026` | RSMMC-20250106001 | Keperawatan / Perawat Rawat Inap | Instalasi Rawat Inap | Ns. Endang Purwanti | Pegawai biasa (tanpa akses SDM) |

Data kepegawaian tambahan:

| Akun | Jenis pegawai | Jenis kontrak (isian master pegawai) | Mulai bekerja |
|---|---|---|---|
| Hartini | Pegawai Tetap | PKWTT | 2 Mar 2015 |
| Yudha | Pegawai Tetap | PKWTT | 1 Jul 2021 |
| Endang | Pegawai Tetap | PKWTT | 3 Sep 2012 |
| Novi | Pegawai Kontrak (PKWT) | PKWT, 6 Jan 2025 – 31 Des 2026 | 6 Jan 2025 |

> Kontrak keempat akun juga sudah tercatat di menu **Kontrak & Masa Kerja → Perjanjian Kerja**
> (lihat bagian 5).

## 2. Cara login

1. Buka aplikasi, lalu masukkan email dan sandi dari tabel di atas.
2. Bila browser meminta **izin lokasi**, pilih *Allow*. Tanpa izin lokasi, login tertahan.
3. Sandi sudah diganti dari sandi awal, jadi tidak ada permintaan ganti sandi saat login.

## 3. Izin per jabatan

Izin di Quilvian melekat pada pasangan **Departemen × Jabatan** (layar Administrator → Akses Role). Siapa pun
yang memegang jabatan yang sama akan mendapat izin yang sama.

| Jabatan | Izin dasar | Total izin |
|---|---|---|
| Kepala Bagian SDM *(baru)* | Paket izin bersih modul SDM (penyetuju/pengawas), ESS & referensi gaji *(ditata 26 Nov 2026)* | 814 |
| Staf Administrasi Kepegawaian *(baru)* | Paket izin bersih modul SDM (operator/pembuat), ESS & referensi gaji *(ditata 26 Nov 2026)* | 664 |
| Kepala Ruangan *(sudah ada)* | Izin lama dipertahankan + 3 izin tambahan di bawah | 13 |
| Perawat Rawat Inap *(sudah ada)* | Tidak diubah | 34 |

Izin khusus menu **Kontrak & Masa Kerja**:

| Izin | Kepala Bagian SDM | Staf Adm. Kepegawaian | Kepala Ruangan | Perawat Rawat Inap |
|---|:-:|:-:|:-:|:-:|
| `WorkforceEmploymentContract : Read` (lihat kontrak) | ✓ | ✓ | ✓ *(hanya unit yang dipimpin)* | – |
| `WorkforceEmploymentContract : ReadAllUnits` (semua unit) | ✓ | ✓ | – | – |
| `WorkforceEmploymentContract : ViewAmount` (lihat nominal gaji) | ✓ | – | – | – |
| `WorkforceEmploymentContract : MarkSigned` | – | ✓ | – | – |
| `WorkforceEmploymentContract : SubmitDecision` | – | ✓ | – | – |
| `WorkforceEmploymentContract : RecordNotification` | – | ✓ | – | – |
| `WorkforceEmploymentContract : RequestCorrection` | – | ✓ | – | – |
| `WorkforceEmploymentContract : UploadDocument` | – | ✓ | – | – |
| `WorkforceProbation : Read` | ✓ | ✓ | ✓ | – |
| `WorkforceProbation : Submit` (ajukan penilaian) | – | – | ✓ | – |
| `ApprovalInbox : Read / Approve / Reject` | ✓ | – | – | – |

Pembuat dan penyetuju sengaja dipisah, karena backend menolak orang yang menyetujui pengajuannya sendiri:

- Keputusan kontrak: **Yudha** mengajukan, **Hartini** menyetujui.
- Penilaian masa percobaan: **Endang** mengajukan, **Hartini** menyetujui.

> Tambahan izin pada jabatan **Kepala Ruangan** juga berlaku untuk akun lain yang memegang jabatan itu, yaitu
> Ns. Rina Marlina (`spv.keperawatan@rsmmc.local`).

### Hasil verifikasi (25 Nov 2026)

| Akun | Daftar kontrak | Daftar masa percobaan | Kotak Masuk Persetujuan |
|---|---|---|---|
| Hartini | 200 | 200 | 200 |
| Yudha | 200 | 200 | 403 (tidak berhak) |
| Endang | 200 | 200 | 403 (tidak berhak) |
| Novi | 403 (tidak berhak) | 403 (tidak berhak) | 403 (tidak berhak) |

## 4. Pemakaian untuk pengujian menu Kontrak & Masa Kerja

| Akun | Dipakai untuk |
|---|---|
| Yudha | Melihat daftar dan rincian kontrak, mencatat tanda tangan, mengajukan keputusan kontrak (perpanjang / angkat tetap / tidak diperpanjang), koreksi, catat pemberitahuan, unggah berkas lama. Untuk menguji tombol *Unggah Berkas Lama* tanpa izin, cabut sementara `UploadDocument` dari jabatan Staf Administrasi Kepegawaian di Akses Role |
| Endang | Mengajukan dan menarik penilaian masa percobaan. Memastikan ia hanya melihat kontrak pegawai Instalasi Rawat Inap |
| Hartini | Data Cetak kontrak dengan gaji tampil, dan Kotak Masuk Persetujuan |
| Novi | Pesan "Akses terbatas" saat alamat menu kontrak dibuka langsung |

## 5. Data kontrak uji

Diisi 25 November 2026. Tujuh pegawai contoh di bawah **tidak punya akun login**. Semuanya perawat Instalasi
Rawat Inap dengan atasan langsung Ns. Endang Purwanti.

| Pegawai | No. Pegawai | Kontrak | Kondisi per 25 Nov 2026 | Dipakai untuk |
|---|---|---|---|---|
| Hartini, Yudha, Endang | (lihat bagian 1) | `KTR/RSMMC/2015/0007`, `2021/0041`, `2012/0019` — PKWTT aktif | Pegawai tetap lama | Data dasar. Endang tidak boleh melihat kontrak Hartini dan Yudha |
| Novi Ariyanti | RSMMC-20250106001 | `KTR/RSMMC/2025/0003` — PKWT 6 Jan 2025 – 31 Des 2026 | Berakhir dalam 36 hari (H-60), belum ada usulan | Pengingat H-60 |
| Rizky Maulana, A.Md.Kep. | RSMMC-20251206001 | `KTR/RSMMC/2025/0118` — PKWT s.d. 5 Des 2026 | H-14, belum ada usulan | Mengajukan **Perpanjang** |
| Desi Susanti, A.Md.Kep. | RSMMC-20251201001 | `KTR/RSMMC/2025/0112` — PKWT s.d. 30 Nov 2026 | H-7, belum ada usulan | **Tidak diperpanjang** + catat pemberitahuan |
| Taufan Permana, A.Md.Kep. | RSMMC-20240301001 | `KTR/RSMMC/2024/0027` → `-R1` → `-R2` (aktif s.d. 28 Feb 2027) | Sudah diperpanjang 2 kali; batas PKWT habis | Perpanjang ditolak; **Angkat pegawai tetap** |
| Siska Amalia, A.Md.Kep. | RSMMC-20251121001 | `KTR/RSMMC/2025/0109` — PKWT s.d. 20 Nov 2026 | Sudah lewat tanpa keputusan | Proses harian mengubahnya menjadi **Lewat Tanpa Keputusan** (paling lambat 1 jam setelah data diisi, selama backend menyala) |
| Galih Pratomo, A.Md.Kep. | RSMMC-20251211001 | `KTR/RSMMC/2025/0121` s.d. 10 Des 2026 + `KTR/RSMMC/2025/0121-R1` (**Draft**, 11 Des 2026 – 10 Des 2027) | Usulan **CDA-2026-000001** Perpanjang 12 bulan, **Disetujui** (diajukan Yudha 10 Nov, disetujui Hartini 12 Nov) | **Catat Tanda Tangan** kontrak lanjutan; daftar Perpanjangan Kontrak |
| Ns. Rosa Damayanti, S.Kep. | RSMMC-20260901001 | `KTR/RSMMC/2026/0088` — PKWTT mulai 1 Sep 2026 | Masa percobaan s.d. **30 Nov 2026** (jatuh tempo) | Endang mengajukan penilaian |
| Ns. Bagas Wicaksono, S.Kep. | RSMMC-20261102001 | `KTR/RSMMC/2026/0103` — PKWTT mulai 2 Nov 2026 | Masa percobaan s.d. 1 Feb 2027 (berjalan) | Daftar masa percobaan |

Hasil pembacaan lewat aplikasi segera setelah data diisi:

| Layar | Angka |
|---|---|
| Perjanjian Kerja | Kontrak Aktif 11 (menjadi 10 setelah kontrak Siska ditutup), Menunggu TTD 1, Akan Berakhir 4, Perlu Pemeriksaan 1 |
| Perpanjangan Kontrak | H-60 = 2, H-14 = 1, H-7 = 1, Lewat Tenggat 1 (menjadi 0 setelah kontrak Siska ditutup) |
| Masa Percobaan | Berjalan 2, Jatuh Tempo 1 |
| Daftar kontrak Endang | 12 baris (staf Rawat Inap + kontraknya sendiri), tanpa kontrak Hartini dan Yudha |

> Usulan Galih berstatus *Disetujui* dimasukkan langsung ke database, karena alur persetujuan belum
> tersambung ke Kotak Masuk (lihat bagian 6). Semua kontrak uji diberi penanda `[SEED-SDM-1125]` pada kolom
> deskripsi (tidak tampil di layar).

## 6. Keterbatasan yang diketahui

1. **Kolom unit di profil keempat akun tampil kosong.** Penempatan utama pegawai baru tidak berunit, dan unit
   utama hanya bisa diubah lewat SK Mutasi atau Batch Data Awal. Endang, Novi, dan pegawai contoh dipasang
   di Instalasi Rawat Inap lewat **penempatan tambahan** (mulai 1 Nov 2026; Endang sebagai pimpinan).
   Pembatasan data per unit dan relasi atasan tetap berjalan.
2. **Data Cetak kontrak** yang bukan berasal dari rekrutmen menampilkan "-" pada rumah sakit, jabatan, unit,
   dan gaji, karena datanya hanya dibaca dari offer rekrutmen.
3. **Persetujuan keputusan kontrak, penilaian masa percobaan, koreksi kontrak, dan batch data awal belum
   tersambung ke Kotak Masuk Persetujuan.** Pengajuan berhenti di status *Diajukan*, jadi peran penyetuju
   Hartini belum bisa diuji sampai bagian itu diperbaiki.
4. **Bug backend:** menambah penempatan tambahan dengan departemen, jabatan, dan tanggal mulai yang sama
   dengan penempatan utama menghasilkan galat 500. Data penempatannya tetap tersimpan, tetapi penyalinan
   ke tabel izin akun gagal. Hindari dengan memakai tanggal mulai yang berbeda.

## 7. Menonaktifkan akun dan membersihkan data uji

- **Akun dan pegawai contoh** tidak dihapus. Bila tidak dipakai lagi, nonaktifkan lewat menu data pegawai
  atau manajemen pengguna. Jabatan baru (*Kepala Bagian SDM*, *Staf Administrasi Kepegawaian*) tetap ada dan
  bisa dipakai pegawai lain.
- **Data kontrak uji** bisa dihapus dengan SQL pembersih yang disimpan bersama skrip pengisinya. Pembersih
  ikut menghapus turunannya: kontrak lanjutan, usulan, koreksi, serta masa percobaan beserta penilaiannya,
  termasuk yang dibuat penguji. Skrip itu ada di folder lokal `QuilvianSystemFrontendDev/test-with-agy/scripts/`
  (`sdm-1125-cleanup.sql`) milik pembuat data, tidak ikut repositori.
