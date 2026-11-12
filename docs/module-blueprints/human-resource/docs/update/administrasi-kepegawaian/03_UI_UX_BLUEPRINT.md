# Cetak Biru Antarmuka Pengguna (UI/UX Blueprint) Administrasi Kepegawaian SIMRS Quilvian V2

**Dokumen**: `03_UI_UX_BLUEPRINT.md`  
**Modul**: Corporate / Human Resource Management (HRD)  
**Menu Induk**: Administrasi Kepegawaian  
**Tanggal**: 11 November 2026  
**Status**: Disetujui untuk Standarisasi Desain  

---

## 1. Arsitektur Navigasi dan Struktur Menu

Struktur menu sidebar modul HRD tetap mempertahankan seluruh nama anak (*child menu*) existing tanpa melakukan penggantian nama sepihak:

```text
Corporate
└── HRD / Kepegawaian
    ├── Administrasi Kepegawaian (corporateHumanResourceWorkforceCore)
    │   ├── Perubahan Data Karyawan (corporateEmployeeProfileChanges)      -> /hr/workforce-core/employee-profile-changes
    │   ├── Penempatan Organisasi (corporateOrganizationAssignments)      -> /hr/workforce-core/organization-assignments
    │   ├── Penempatan Jabatan (corporatePositionAssignments)            -> /hr/workforce-core/position-assignments
    │   ├── Relasi Atasan (corporateManagerAssignments)                  -> /hr/workforce-core/manager-assignments
    │   ├── Riwayat Kepegawaian (corporateEmploymentHistories)          -> /hr/workforce-core/employment-histories
    │   ├── Dokumen SK Kepegawaian (corporatePersonnelActionOrders)      -> /hr/workforce-core/personnel-actions
    │   ├── Surat Tugas Resmi [USULAN SUBMENU]                           -> /hr/workforce-core/official-duties
    │   ├── Registri Dokumen Administrasi [USULAN SUBMENU]               -> /hr/workforce-core/administrative-documents
    │   └── Batch Data Awal (corporateInitialPlacementBatches)           -> /hr/workforce-core/initial-batches
```

---

## 2. Template Tata Letak Standar Seluruh Halaman

Seluruh antarmuka di bawah induk `Administrasi Kepegawaian` wajib mengikuti kanon desain Quilvian:

```text
+----------------------------------------------------------------------------------------------------+
| Sidebar HRD | Breadcrumb: HRD / Administrasi Kepegawaian / [Nama Submenu]                          |
|-------------+--------------------------------------------------------------------------------------|
|             | Judul Halaman Submenu                             [Tombol Aksi Utama / + Buat Baru]  |
|             | Deskripsi fungsi halaman dalam bahasa Indonesia yang baku                            |
|             |--------------------------------------------------------------------------------------|
|             | KARTU KPI / RINGKASAN RINGKAS:                                                       |
|             | [ Total Data: XX ]  [ Draf: XX ]  [ Menunggu: XX ]  [ Disetujui: XX ]  [ Selesai: XX]|
|             |--------------------------------------------------------------------------------------|
|             | KONTROL PENYARING (DataFilter):                                                      |
|             | [ Cari kata kunci... ] [ Saringan Status ] [ Saringan Jenis ] [ Rentang Tanggal ]   |
|             |--------------------------------------------------------------------------------------|
|             | TABEL DATA OPERASIONAL (DataTable):                                                  |
|             | +----------------------------------------------------------------------------------+ |
|             | | No. Dokumen | Nama Pegawai | Kategori/Unit | Tgl Efektif | Status   | Aksi       | |
|             | |-------------+--------------+---------------+-------------+----------+------------| |
|             | | SK-2026-001 | Dr. Ahmad    | Promosi/ICU   | 01/12/2026  | [Badge]  | [Rincian]  | |
|             | +----------------------------------------------------------------------------------+ |
|             |--------------------------------------------------------------------------------------|
|             | PAGINASI & METADATA:                                                                 |
|             | Menampilkan 1 - 25 dari 142 data                               [ < 1 [2] 3 4 5 > ]   |
+----------------------------------------------------------------------------------------------------+
```

---

## 3. Cetak Biru Antarmuka Detail per Cakupan

### 3.1. Cakupan 1: Surat Keputusan (SK Kepegawaian)
*Komponen Acuan*: `personnel-action-order-list-view.jsx` & `personnel-action-order-create-view.jsx`.

#### A. Layar Daftar SK Kepegawaian
- **Ringkasan KPI**: Total SK, Draf, Menunggu Persetujuan, Berlaku Aktif, Ditolak.
- **Tabel Kolom**:
  1. `Nomor SK`: Nomor formal (misal: `042/SK-DIR/RSQ/XI/2026`).
  2. `Pegawai`: Nama lengkap beserta NIP/Kode Profil.
  3. `Kategori Tindakan`: Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara.
  4. `Unit & Jabatan Asal -> Tujuan`: Indikasi pergerakan organisasi.
  5. `Tanggal Efektif`: Tanggal mulai berlaku sahnya keputusan.
  6. `Status`: Badge (Draf, Menunggu Approval, Disetujui, Diterapkan, Ditolak, Dibatalkan).
  7. `Aksi`: Tombol *Lihat Detail*, *Unduh Berkas Pindaian*.

#### B. Layar Pembuatan Draf SK (Form Bertahap Komparatif)
```text
+----------------------------------------------------------------------------------------------------+
| Buat Draf Surat Keputusan Kepegawaian                                    [Kembali ke Daftar SK]    |
|----------------------------------------------------------------------------------------------------|
| 1. DATA PEGAWAI & TINDAKAN                                                                         |
|    Pegawai: [Pilih Pegawai (Cari Nama / NIP)...]                                                   |
|    Kategori: [ Promosi / Mutasi / Rotasi / Demosi ]    Tgl Efektif: [ DD / MM / YYYY ]             |
|----------------------------------------------------------------------------------------------------|
| 2. KOMPARASI PENEMPATAN (Otomatis memuat data aktif dari MstWorkforceProfile)                      |
|    +------------------------------------+-----------------------------------------------------+    |
|    | PENEMPATAN SAAT INI (Read-Only)    | PENEMPATAN BARU (Isi yang berubah)                  |    |
|    +------------------------------------+-----------------------------------------------------+    |
|    | Unit: Instalasi Rawat Inap Lt. 3   | Unit Tujuan: [ Pilih Unit Organisasi Tujuan... ]   |    |
|    | Departemen: Keperawatan Medikal    | Departemen:  [ Pilih Departemen Tujuan... ]        |    |
|    | Jabatan: Perawat Pelaksana         | Jabatan Baru: [ Pilih Jabatan Baru... ]             |    |
|    | Atasan: Ns. Hendra, S.Kep          | Atasan Baru: [ Pilih Atasan Baru... ]              |    |
|    | Gaji Pokok: Rp 5.200.000           | Penyesuaian Gaji Pokok: [ Rp .................. ]  |    |
|    +------------------------------------+-----------------------------------------------------+    |
|----------------------------------------------------------------------------------------------------|
| 3. DASAR PERTIMBANGAN & CATATAN ADMINISTRATIF                                                      |
|    Alasan Resmi Penerbitan SK (Min 10 karakter):                                                   |
|    [ Kebutuhan pemenuhan formasi jabatan Kepala Ruangan ICU sesuai SK Direktur No. 012...        ] |
|    Catatan Khusus: [ Masa orientasi manajerial selama 3 bulan pertama.                           ] |
|----------------------------------------------------------------------------------------------------|
| [Batal]                                                  [Simpan Draf]   [Ajukan untuk Persetujuan]|
+----------------------------------------------------------------------------------------------------+
```

---

### 3.2. Cakupan 2: Surat Tugas (Official Duty Assignment)

#### A. Layar Daftar Multi-Tab Peran
Daftar dipisahkan dalam tab berbasis peran pengguna:
1. `Pengajuan Unit Saya` (Terlihat untuk Kepala Unit / Pemohon).
2. `Antrean Verifikasi HRD` (Terlihat untuk Staf Administrasi HRD).
3. `Menunggu Pengesahan` (Terlihat untuk Pejabat Direksi / Kepala Bagian Berwenang).
4. `Surat Tugas Terbit` (Daftar arsip publik unit yang sah).

#### B. Formulir Pengajuan Kepala Unit (Mendukung Multi-Pegawai & Peringatan Jadwal)
```text
+----------------------------------------------------------------------------------------------------+
| Formulir Pengajuan Surat Tugas Dinas                                                               |
|----------------------------------------------------------------------------------------------------|
| Unit Pengaju: Instalasi Gawat Darurat [Terkunci]      Pemohon: dr. Bambang (Kepala IGD) [Terkunci] |
| Jenis Kegiatan: [ Pelayanan Luar RS / Pelatihan-Seminar / Pengawalan Ambulans / Rapat Dinas ]     |
| Nama Kegiatan:  [ Pengawalan Ambulans Pasien Rujukan Kritis ke RS Jantung Harapan Kita           ] |
| Lokasi Tujuan:  [ Jakarta Barat ]                     Transportasi: [ Mobil Ambulans RSQ-02 ]      |
| Waktu Mulai:    [ 12/11/2026 08:00 ]                  Waktu Selesai: [ 13/11/2026 18:00 ]          |
| Uraian Tugas:   [ Melakukan stabilisasi tanda vital pasien selama rujukan antar-rumah sakit.     ] |
|----------------------------------------------------------------------------------------------------|
| DAFTAR PEGAWAI YANG DITUGASKAN                                           [+ Tambah Pegawai Bawahan]|
| +------------------------------------------------------------------------------------------------+ |
| | No | Nama & NIP               | Jabatan        | Status STR/SIP | Pengecekan Jadwal / Konflik  | |
| |----+--------------------------+----------------+----------------+------------------------------| |
| | 1  | dr. Ahmad Fauzi (Dokter) | Dokter Jaga    | Aktif          | [V] Tidak ada benturan shift | |
| | 2  | Ns. Siti R. (Perawat)    | Perawat Mahir  | Aktif          | [!] BENTURAN: Shift Malam    | |
| |    |                          |                |                | (Wajib covering shift unit)  | |
| +------------------------------------------------------------------------------------------------+ |
| Dokumen Disposisi / Undangan: [ Pilih Berkas PDF / JPG (Maks 5 MB)... ]                           |
|----------------------------------------------------------------------------------------------------|
| [Batal]                                                  [Simpan Draf]   [Ajukan ke Verifikasi HRD]|
+----------------------------------------------------------------------------------------------------+
```

#### C. Antarmuka Verifikasi HRD & Terbit Surat A4 Kop Resmi
- **Sisi Kiri**: Metadata permohonan, profil pegawai, dan dokumen undangan.
- **Sisi Kanan (Checklist Verifikasi HRD)**:
  - `[ ]` Staf bertatus aktif di master pegawai.
  - `[ ]` Kualifikasi/kompetensi staf sesuai dengan kegiatan penugasan.
  - `[ ]` Ketersediaan petugas pengganti jaga telah disetujui unit.
  - `Aksi HRD`: Tombol *Minta Revisi* (wajib alasan), *Tolak* (wajib alasan), atau *Luluskan & Teruskan ke Pejabat Pengesah*.
- **Tampilan Terbit Naskah Dinas A4**:
  - Kop Surat resmi RS Quilvian Utama (Logo, Nama RS, Alamat Lengkap, Kontak).
  - Judul: **SURAT PERINTAH TUGAS**.
  - Nomor: `089/SPT-DIR/RSQ/XI/2026`.
  - Dasar Penugasan, Identitas Pejabat Pemberi Tugas, Tabel Personel Bertugas, Uraian Tugas, Lokasi & Waktu.
  - Tanda tangan digital pejabat pengesah & QR Code validasi keaslian.

---

### 3.3. Cakupan 3: Surat Keterangan Resmi

#### A. Katalog Kartu Jenis Surat (Dynamic Catalog Grid)
Menampilkan jenis-jenis surat resmi yang aktif di master:
1. `Surat Keterangan Bekerja Aktif` (SLA: 24 Jam, Tanpa data finansial).
2. `Surat Keterangan Penghasilan & KPR` (SLA: 48 Jam, Memerlukan Slip Gaji Sub-10 & Persetujuan C&B).
3. `Surat Keterangan Masa Kerja / Pengalaman Bekerja` (SLA: 48 Jam).
4. `Surat Izin Praktik Luar Rumah Sakit` (SLA: 72 Jam, Memerlukan Rekomendasi Komite Medis).

#### B. Formulir Permohonan Adaptif (Conditional Dynamic Form)
- Memilih pegawai sasaran: Sistem secara otomatis menampilkan NIP, jabatan, unit, tanggal mulai bekerja, dan status kepegawaian terkini.
- Jika jenis surat berlabel **Finansial/Penghasilan**:
  - Kolom masukan gaji manual ditiadakan.
  - Sistem menampilkan kartu status slip gaji terakhir: *“Slip Gaji Periode Oktober 2026 (Finalized) - Bruto: Rp 8.500.000 / Netto: Rp 7.200.000”*.
  - Jika slip gaji belum final, muncul peringatan merah pemblokiran pengajuan sesuai aturan VR-HRS-06.

---

### 3.4. Cakupan 4: Registri Dokumen Administrasi Terpadu

#### A. Layar Registri Tunggal Seluruh Arsip
- **Filter Kerahasiaan & Jenis**: Saringan Jenis (SK, Surat Tugas, Surat Keterangan, Berkas Kualifikasi STR/SIP/Ijazah), Status (Berlaku, Kedaluwarsa, Dicabut), Klasifikasi (Publik, Terbatas, Rahasia Finansial).
- **Tabel Arsip Terpadu**:
  1. `Nomor Dokumen`: Nomor naskah dinas atau nomor sertifikat.
  2. `Jenis Dokumen`: Kategori dokumen arsip.
  3. `Pemilik Dokumen`: Nama pegawai dan NIP pemilik berkas.
  4. `Penerbit / Sumber`: Sistem SIMRS Quilvian (Generated) atau Unggahan Pegawai (Uploaded).
  5. `Tanggal Terbit & Berlaku`: Rentang masa berlaku sah.
  6. `Status Legal`: Badge *Berlaku*, *Kedaluwarsa*, atau *DICABUT (BAP)*.
  7. `Aksi`: *Preview Aman*, *Unduh Berkas*, *Riwayat Versi*, *Pencabutan Resmi*.

#### B. Drawer Detail & Manajemen Pencabutan (Revocation Drawer)
- Menampilkan pratinjau PDF di dalam iframe terproteksi (tanpa membocorkan lokasi folder fisik server).
- Jika dokumen dicabut, pratinjau secara otomatis menampilkan cap miring merah tebal bertuliskan: **DICABUT / TIDAK BERLAKU**.
- Modal pencabutan mewajibkan staf HRD mengisi:
  - `Nomor Berita Acara Pencabutan (BAP)`: Wajib diisi (misal: `BAP-SDM/REV/2026/014`).
  - `Alasan Pencabutan`: Wajib diisi minimal 20 karakter.

---

### 3.5. Cakupan 5: Pengajuan Perubahan Data Karyawan
*Komponen Acuan*: `employee-profile-changes-view.jsx`.

#### A. Antarmuka Rekapitulasi & Modal Rincian Verifikasi
- **Tabel Daftar**: Menampilkan No. Permohonan, Nama Pegawai, Kategori (Alamat/Kontak/Identitas/Rekening), Tanggal Pengajuan, Status, dan Tombol *Lihat Rincian*.
- **Peringatan Anti-Self-Approval**: Jika akun HRD yang sedang login membuka permohonan yang diajukan oleh dirinya sendiri, muncul kotak merah peringatan: *“Anda adalah pemilik permohonan ini. Sesuai prinsip Four-Eyes, Anda dilarang memverifikasi permohonan Anda sendiri.”* Tombol verifikasi dinonaktifkan otomatis.
- **Tabel Komparasi Field**:
  - Kolom: *Entitas Target*, *Nama Field*, *Nilai Lama (Sebelum)*, *Nilai Baru (Sesudah)*, *Status Verifikasi Bukti*.
  - Menandai nilai baru dengan teks tebal berwarna biru untuk mempermudah pengecekan visual.
- **Tautan Unduh Bukti**: Mengunduh berkas KTP/KK/Buku Tabungan melalui rute aman `/api/v1/corporate/human-resource/employee-profile-changes/{id}/evidence/download`.
- **Aksi Final**: Tombol *Terapkan Data (Apply)* hanya aktif jika permohonan telah berstatus `Approved` dan dieksekusi tepat 1 kali.

---

## 4. Standar Responsivitas Tampilan (Responsive Breakpoints)

| Viewport | Perilaku Layout | Penanganan Tabel | Penanganan Modal / Drawer |
|---|---|---|---|
| **Desktop Lebar (1440 px)** | Layout multi-kolom penuh, KPI 4-5 kolom horizontal. | Tabel tampil penuh dengan seluruh kolom operasional. | Modal dialog ukuran besar (`size="lg"` / `size="xl"`). |
| **Desktop Standar (1024 px)** | Sidebar tetap terbuka, KPI 3-4 kolom horizontal. | Kolom sekunder dapat di-scroll horizontal dalam kontainer tabel. | Modal dialog menyesuaikan padding 24px. |
| **Tablet (768 px)** | Sidebar otomatis menciut (*collapsed*), KPI 2 kolom. | Tabel dapat di-scroll horizontal tanpa membuat badan halaman terpotong. | Modal memenuhi 90% lebar layar. |
| **Mobile (390 px)** | Sidebar menjadi offcanvas, KPI bertumpuk 1 kolom vertikal. | Tabel di-wrap horizontal pada kontainer berbatas, tombol aksi menjadi ikon ringkas. | Modal berubah menjadi kartu full-screen vertikal yang nyaman dioperasikan satu tangan. |

---

## 5. Penanganan State Kosong, Memuat, dan Kesalahan Sistem

1. **State Memuat (Loading State)**:
   - Menggunakan skeleton shimmer untuk kartu KPI dan tabel data.
   - Tombol aksi menampilkan spinner mikro dan berstatus `disabled` selama proses request asinkron berjalan untuk mencegah penekanan ganda (*double-submit*).
2. **State Kosong (Empty State)**:
   - Jika tidak ada data yang cocok dengan penyaring: menampilkan ilustrasi dokumen kosong, judul informatif *"Tidak ada data yang cocok dengan penyaringan"*, dan tombol *"Reset Saringan"*.
   - Jika belum ada data sama sekali di unit tersebut: menampilkan pesan panduan langkah awal pengguna.
3. **State Kesalahan API (Error State)**:
   - Dilarang keras menutupi kegagalan API dengan menampilkan data tiruan (*fake mock data*) atau mengonversi galat menjadi status sukses palsu.
   - Menampilkan komponen `InformationAlert` bervarian bahaya (`variant="danger"`) yang memuat pesan resmi dari backend atau penjelas kesalahan jaringan yang ramah pengguna.
