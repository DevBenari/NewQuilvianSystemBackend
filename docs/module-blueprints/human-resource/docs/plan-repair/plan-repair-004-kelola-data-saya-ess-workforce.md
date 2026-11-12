# PLAN-REPAIR-004 — Rencana Perbaikan Kelola Data Saya (ESS): Eliminasi Error 400, Tab Informasi Pribadi, Matriks Read-Only vs Approval, dan Kerapihan Visual

```yaml
plan_id: PLAN-REPAIR-HRD-004
issue: ../issue/issue-004-kelola-data-saya-ess-workforce.md
status_rencana: MENUNGGU_PERSETUJUAN
tanggal_rencana: "2026-11-11"
diputuskan_oleh: null
tanggal_keputusan: null
basis_source_backend: "f751f8ca (MHamzah)"
basis_source_frontend: "d09dc74da (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti Verifikasi |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-HRD-004-01` | `ISS-HRD-004-01` | Penyelarasan kontrak payload pengajuan alamat & rekening bank mandiri serta dukungan backend DTO | BE + FE | 1 | — | SELESAI | Kompilasi C# & `AllowedFields` terdaftar; frontend memetakan `TargetEntityName`, `FieldGroup`, `FieldName` PascalCase |
| `FIX-HRD-004-02` | `ISS-HRD-004-02` | Redesain modal tinjauan pengajuan: eliminasi dump teks string teknis JSON dan kolom nilai baru mentah | FE | 2 | — | SELESAI | Modal tinjauan diredesain jadi 4 kolom bersih (Menu Profil, Jenis Perubahan, Rincian Data Usulan, Sifat Field); eliminasi string dump |
| `FIX-HRD-004-03` | `ISS-HRD-004-03` | Penambahan tab kanonikal "Informasi Pribadi" dan penghapusan tombol/modal pop-up di hero banner | FE | 1 | — | SELESAI | Tab `personalInfo` aktif di `EMPLOYEE_WORKFORCE_TABS`, tombol hero dihapus, panel baca & ubah in-place terhubung ke proposal |
| `FIX-HRD-004-04` | `ISS-HRD-004-04`, `ISS-HRD-004-T1` | Penerapan matriks proteksi hak akses (Read-Only vs Approval) dan pengaktifan aksi tambah pada Kontak Darurat, Pendidikan, Dokumen, Keluarga | FE + BE | 1 | — | SELESAI | `ESS_EDITABLE_TABS` (7 tab mandiri + personalInfo) menampilkan aksi Tambah & Ubah; 9 tab korporat terkunci `readOnly: true` |
| `FIX-HRD-004-05` | `ISS-HRD-004-05` | Penyediaan jarak aman scroll bawah (*bottom safe space*) dan perbaikan kontras tipografi label kartu | FE | 2 | — | SELESAI | CSS `.wfp-detail-shell` safe padding clamp(140px, 15vh, 180px); label diperbesar ke 11px color #475569 |
| `FIX-HRD-004-06` | `ISS-HRD-004-06` | Pengayaan endpoint `current-profile` backend dan sinkronisasi data kepegawaian resmi pada header & ringkasan ESS | BE + FE | 1 | — | SELESAI | Backend `EmployeeCurrentProfileResponse` diperkaya dengan 11 field kepegawaian master; frontend memetakan Departemen, Posisi, Grade, Lokasi, Tgl Bergabung, Akhir Kontrak |

**Ringkasan: 6 dari 6 perbaikan selesai.**

---

## 2. Solusi Terpilih Per Temuan

### ISS-HRD-004-01 — Penolakan HTTP 400 Bad Request saat Submit Alamat & Rekening Bank

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Penyelarasan Kontrak Penuh**: Frontend memetakan properti PascalCase lengkap (`TargetEntityName`, `FieldGroup`, `FieldName`, `Description`) sesuai kontrak DTO C#, dan backend memperluas `AllowedFields` untuk mengenali entitas relasi WFP (`WfpAddress`, `WfpBankAccount`). | Bersih, sesuai standar arsitektur REST ASP.NET Core, lolos validasi model state, dan data tersimpan terstruktur. | Memerlukan penyelarasan pada kedua sisi (frontend serializer dan backend service). |
| **B** | Melemahkan validasi backend dengan menghapus atribut `[Required]` pada `FieldGroup` dan `TargetEntityName`. | Cepat diselesaikan di backend. | Merusak integritas data model, audit trail menjadi bolong, dan berpotensi memicu `NullReferenceException` saat proses apply data. |
| **C** | Mengirimkan penambahan alamat/bank langsung melalui endpoint master korporat biasa tanpa workflow. | Tidak perlu integrasi ke sistem profile changes. | **Melanggar aturan tata kelola RS**: Karyawan biasa tidak boleh memutasi data master tanpa verifikasi HRD. |

**Solusi terpilih: Opsi A.**
1. Memenuhi kontrak validasi ASP.NET Core dengan menjamin field `TargetEntityName` dan `FieldGroup` selalu terisi valid.
2. Memastikan backend mampu memproses usulan penambahan entitas relasi WFP secara transaksional di dalam Workflow Engine.

*Kenapa bukan opsi lain:* Opsi B merusak validasi database dan audit trail, sedangkan Opsi C melanggar aturan tata kelola rumah sakit.

---

### ISS-HRD-004-02 — Eliminasi Kolom Nilai Baru Mentah pada Modal Tinjauan Pengajuan

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Sederhanakan Format Tinjauan**: Hapus kolom "Nilai Baru (Diusulkan)" yang memajang dump teks string teknis. Gantikan dengan kolom "Rincian Data yang Diusulkan" yang hanya menampilkan informasi bermakna (misal: *"Alamat Domisili: Jl. Rasuna Said No. 10"* tanpa atribut kode atau GUID teknis). | Tampilan sangat bersih, manusiawi, mudah dipahami staf rumah sakit, dan tidak memajang informasi teknis internal database. | Memerlukan formatter khusus per jenis entitas usulan di frontend. |
| **B** | Menghapus modal tinjauan sama sekali dan langsung melakukan submit instan saat tombol ditekan. | Sangat sederhana. | Pengguna kehilangan kesempatan memeriksa ulang apa saja data yang diubah sebelum permohonan terkunci di workflow HRD. |
| **C** | Membiarkan tabel apa adanya tetapi menyembunyikan kolom nilai baru menggunakan CSS `display: none`. | Mudah dikerjakan. | Data mentah tetap dikirim di DOM dan payload JSON tetap memuat dump string tak berguna. |

**Solusi terpilih: Opsi A.**
1. Memenuhi permintaan langsung pelapor bahwa kolom nilai baru mentah dan dump string JSON tidak diperlukan.
2. Menjaga fungsi verifikasi visual sebelum submit tanpa membingungkan pengguna dengan GUID teknis.

*Kenapa bukan opsi lain:* Opsi B menghilangkan gerbang review pengguna, sedangkan Opsi C adalah perbaikan kosmetik yang tidak menyelesaikan kebersihan payload JSON.

---

### ISS-HRD-004-03 — Redesain: Tab "Informasi Pribadi" Menggantikan Modal Pop-up Hero

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Integrasi Tab Kanonikal**: Hapus tombol "Mode Ubah Data Pribadi" dari hero banner. Tambahkan tab baru **"Informasi Pribadi"** di panel sisi kiri. Di dalam panel tab tersebut, sajikan data identitas kependudukan dan kontak dalam mode baca, disertai tombol *"Ubah Informasi Pribadi"* yang membuka laci editor standar. | Sesuai 100% dengan permintaan pelapor, mengakhiri fragmentasi UI, dan menyatukan seluruh tata kelola ke sistem tab yang konsisten. | Memerlukan registrasi tab baru di `EMPLOYEE_WORKFORCE_TABS` dan pembuatan panel tampilan identitas pribadi. |
| **B** | Mempertahankan tombol hero dan hanya mempercantik tampilan modal pop-up agar lebih responsif. | Perubahan kode lebih sedikit. | **Menolak permintaan langsung pelapor**; modal pop-up tetap terasa terpisah dari konteks tab workforce lainnya. |
| **C** | Menggabungkan form ubah identitas pribadi ke dalam tab "Ringkasan Profil". | Tidak perlu menambah tab baru. | Tab ringkasan profil menjadi penuh sesak dan mengaburkan fungsinya sebagai ringkasan eksekutif (*executive summary*). |

**Solusi terpilih: Opsi A.**
1. Menjawab kebutuhan pelapor secara presisi: alur menjadi natural, tab Informasi Pribadi berdiri mandiri, dan karyawan dapat membaca datanya lebih dulu sebelum memutuskan untuk mengubah.
2. Alur perubahan data pribadi tetap terlindungi karena aksi edit dialirkan ke daftar usulan perubahan (*Proposal Changes List*) menuju approval HRD.

*Kenapa bukan opsi lain:* Opsi B tidak menghiraukan masukan pelapor, sedangkan Opsi C membuat tab ringkasan profil menjadi tumpang tindih fungsinya.

---

### ISS-HRD-004-04 — Matriks Hak Akses Karyawan (Read-Only vs Approval HRD)

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Terapkan Matriks Tata Kelola Rumah Sakit Berbasis Kebijakan HR**: Kunci 9 tab sensitif korporat (Organisasi, Jabatan, Atasan, Gaji, Kontrak, SK, Kredensial, Hak Klinis, Rekam Medis) sebagai `readOnly: true` di ESS; aktifkan aksi tambah/ubah pada 7 tab mandiri (Informasi Pribadi, Alamat, Bank, Kontak Darurat, Pendidikan, Dokumen, Keluarga) yang dialirkan ke proposal approval HRD. | Mematuhi regulasi Kemenkes RI, tata kelola korporat rumah sakit, dan prinsip Four-Eyes (*anti-self-approval*). | Memerlukan penyelarasan konfigurasi tab di frontend dan pemuatan state resource mandiri. |
| **B** | Mengizinkan seluruh tab dapat diedit oleh karyawan asal melalui persetujuan HRD. | Fleksibel bagi pengguna. | **Pelanggaran Kritis**: Karyawan tidak memiliki hak mengusulkan mutasi unit kerja, jabatan struktural, atau hak klinis dokter melalui portal mandiri. |
| **C** | Mengunci seluruh tab menjadi read-only kecuali Informasi Pribadi. | Paling aman dari segi risiko. | Menghambat efisiensi operasional karena karyawan tidak bisa memperbarui kontak darurat atau rekening banknya secara mandiri. |

**Solusi terpilih: Opsi A.**
1. Merupakan standar emas manajemen SDM rumah sakit modern yang memisahkan hak mandiri pegawai (*self-managed*) dari wewenang mutlak manajemen rumah sakit (*corporate governance*).
2. Membuka tombol tambah data yang hilang pada Kontak Darurat, Pendidikan, dan Dokumen dengan jalur persetujuan sah.

*Kenapa bukan opsi lain:* Opsi B menimbulkan risiko tata kelola yang fatal, sedangkan Opsi C membuat portal ESS tidak bermanfaat.

---

### ISS-HRD-004-05 — Jarak Aman Scroll Bawah & Peningkatan Kontras Tipografi

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Safe Scroll Space & Kontras Terstandar**: Tambahkan `padding-bottom: clamp(130px, 15vh, 160px)` pada kontainer halaman ESS dan naikkan ukuran font label `.wfp-kv-item span` dari `9px` ke `11px` (warna `#475569`) serta teks nilai ke `13px` (warna `#0f172a`). | Teks tidak pernah lagi tertimpa oleh footer fixed saat di-scroll, dan kontras keterbacaan memenuhi standar aksesibilitas WCAG AA. | Menambah sedikit ruang kosong di bawah halaman saat halaman berada di posisi paling atas. |
| **B** | Mengubah bilah footer fixed menjadi static footer di bawah konten. | Footer tidak akan pernah menutupi konten. | Menyentuh komponen global footer aplikasi (`footer.jsx`) yang dipakai oleh seluruh modul sistem Quilvian lainnya. |
| **C** | Hanya mengubah ukuran teks tanpa menambahkan padding bawah. | Sangat sedikit perubahan. | Teks tetap tertutup dan buram oleh kaca blur footer saat pengguna melihat bagian bawah kartu. |

**Solusi terpilih: Opsi A.**
1. Mengatasi akar masalah secara tuntas di lapisan halaman ESS tanpa mengganggu perilaku global footer modul lain.
2. Memberikan kenyamanan membaca yang optimal bagi staf rumah sakit.

*Kenapa bukan opsi lain:* Opsi B memiliki radius dampak terlalu luas ke modul lain, sedangkan Opsi C membiarkan masalah utama (tertutup footer) tetap terjadi.

---

### ISS-HRD-004-06 — Sinkronisasi Data Kepegawaian Lengkap di ESS

| Opsi | Cara | Kelebihan | Kekurangan |
| :--- | :--- | :--- | :--- |
| **A (Terpilih)** | **Pengayaan Payload Backend & Visibilitas Permanen**: Tambahkan `PrimaryDepartmentName`, `PrimaryPositionName`, `GradeLevel`, `JoinDate`, `ContractEndDate`, `EmploymentType` pada DTO backend `EmployeeCurrentProfileResponse`, sinkronkan mapping frontend, dan set `alwaysVisible: true` pada dashboard. | Data ESS 100% identik dan konsisten dengan Master Data Karyawan, dan tidak ada lagi baris informasi yang hilang atau garis pembatas patah. | Memerlukan pembaruan kecil pada DTO dan service backend. |
| **B** | Memanggil langsung endpoint master data korporat dari halaman ESS. | Tidak perlu mengubah DTO `current-profile`. | **Ditolak oleh Keamanan**: Karyawan biasa tidak memiliki izin membaca API master data korporat (HTTP 403 Forbidden). |

**Solusi terpilih: Opsi A.**
1. Mengikuti pola keamanan kanonikal di mana portal mandiri memiliki endpoint berwewenang mandiri (*self-service endpoint*) yang menyajikan data resmi lengkap milik karyawan login.
2. Memastikan seluruh slot informasi tampil rapi dengan tanda `"-"` jika ada data master yang belum diisi HRD.

*Kenapa bukan opsi lain:* Opsi B melanggar batas otorisasi pengguna biasa.

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Skema Halaman Utama Kelola Data Saya (ESS)

#### SEBELUM (Keadaan Masalah pada Gambar 1 & 2):
```text
+----------------------------------------------------------------------------------------------------+
| Layanan Mandiri Pegawai (ESS)                                                                      |
| Kelola Data Saya -- Mira Safitri                     [ Riwayat Pengajuan Saya ] [ Mode Ubah Data ] |
+----------------------------------------------------------------------------------------------------+
| [ FOTO ]  AKUN & AKSES         IDENTITAS KARYAWAN                KEPEGAWAIAN                       |
|   MS      Akun Login:          Nomor: RSMMC-20260404001          Lokasi: RS Metropolitan...        |
|           Belum dibuat         Status: Status Aktif              (Departemen, Posisi, Grade HILANG)|
+----------------------------------------------------------------------------------------------------+
| [!] Mode kelola profil aktif                                                                       |
+---------------------------------------+------------------------------------------------------------+
| MENU WORKFORCE                        | RINGKASAN PROFIL WORKFORCE                                 |
| (•) Ringkasan Profil              (1) | +--------------------+ +-------------------+ +------------+|
| ( ) Alamat                        (0) | | Nama Karyawan      | | Kode Karyawan     | | No. Karyawan||
| ( ) Rekening Bank                 (0) | | Mira Safitri       | | RSMMC-...         | | RSMMC-...   ||
| ( ) Pendidikan                    (0) | +--------------------+ +-------------------+ +------------+|
| ( ) Dokumen                       (0) | | (label buram)      | | (label buram)     | | (buram)    ||
| ( ) Kontak Darurat                (0) | | mira.safitri@...   | | RS Metropolitan.. | | Belum ada.. ||
| ... (tidak ada tab Info Pribadi)      | +--------------------+ +-------------------+ +------------+|
|                                       | ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~ ~|
|                                       | [BILAH FOOTER FIXED KACA MENUTUPI DAN MEMBURAMKAN KARTU INI]|
+---------------------------------------+------------------------------------------------------------+
```

#### SESUDAH (Hasil Perbaikan yang Direncanakan):
```text
+----------------------------------------------------------------------------------------------------+
| Layanan Mandiri Pegawai (ESS)                                                                      |
| Kelola Data Saya -- Mira Safitri                     [ Riwayat Pengajuan Saya (0) ]                |
| (Tombol pop-up hero ditiadakan, dialihkan ke Tab Informasi Pribadi)                                |
+----------------------------------------------------------------------------------------------------+
| [ FOTO ]  AKUN & AKSES         IDENTITAS KARYAWAN                KEPEGAWAIAN & PENEMPATAN          |
|  [Avatar] Akun Login:          Nomor: RSMMC-20260404001          Grade Level: STAFF                |
|  Perawat  Belum dibuat         Departemen: Keperawatan           Lokasi Kerja: Poliklinik Rawat Jln|
|           Status: Aktif        Posisi: Perawat Rawat Inap        Bergabung: 04 Apr 2026            |
|                                Status Karyawan: Aktif            Akhir Kontrak: 04 Apr 2027        |
+----------------------------------------------------------------------------------------------------+
| [i] Mode Pengelolaan Profil Mandiri: Data master resmi dilindungi. Seluruh usulan melalui HRD.     |
+---------------------------------------+------------------------------------------------------------+
| MENU WORKFORCE                        | PANEL KONTEN TAB AKTIF: INFORMASI PRIBADI                  |
| ( ) Ringkasan Profil                  |                                                            |
| (•) Informasi Pribadi             [!] | IDENTITAS KEPENDUDUKAN                [ Ubah Data Pribadi ]|
| ( ) Alamat                        (1) | +---------------------------------------------------------+|
| ( ) Rekening Bank                 (1) | | Nama Lengkap: Mira Safitri    Jenis Identitas: KTP      ||
| ( ) Kontak Darurat                (1) | | NIK: 3171012345670001         Jenis Kelamin: Perempuan  ||
| ( ) Pendidikan                    (1) | | Tempat/Tgl Lahir: Jakarta, 24 Jun 1994                  ||
| ( ) Dokumen Pribadi               (1) | | Agama: Islam                  Status Nikah: Belum Kawin ||
| ( ) Keluarga & Tanggungan         (0) | +---------------------------------------------------------+|
| ------------------------------------- | KONTAK PRIBADI                                             |
| [Kategori Mutlak Read-Only]           | +---------------------------------------------------------+|
| ( ) Penempatan Organisasi         [x] | | Nomor HP: 0812-3456-7890      Nomor WhatsApp: 0812-34...||
| ( ) Penempatan Jabatan            [x] | | Email Pribadi: mira.safitri@rsmmc.local                 ||
| ( ) Relasi Atasan                 [x] | +---------------------------------------------------------+|
| ( ) Penetapan Gaji                [x] |                                                            |
| ( ) Riwayat Kontrak & SK          [x] |                                                            |
| ( ) Kredensial & Hak Klinis       [x] |                                                            |
|                                       | (Ruang scroll aman di bawah: Konten tidak tertabrak footer)|
+---------------------------------------+------------------------------------------------------------+
| [BILAH FOOTER FIXED APLIKASI DI BAWAH RUANG AMAN SCROLL -- TIDAK MENUTUPI KONTEN APAPUN]           |
+----------------------------------------------------------------------------------------------------+
```

---

### 3.2 Skema Modal Tinjauan Pengajuan Mandiri (Approval HRD)

#### SEBELUM (Keadaan Masalah pada Gambar Lampiran Pelapor):
```text
+----------------------------------------------------------------------------------------------------+
| Tinjauan Pengajuan Perubahan Data Mandiri (Approval HRD)                                       [X] |
+----------------------------------------------------------------------------------------------------+
| Rincian usulan data:                                                                               |
| +-----------+-------------------+-------------+------------------------------------+--------------+|
| | Menu      | Field / Data      | Nilai Lama  | Nilai Baru (Diusulkan)             | Sifat Field  ||
| +-----------+-------------------+-------------+------------------------------------+--------------+|
| | [Alamat]  | Penambahan Alamat | (Data Baru) | addressType: Identity, addressLine:| Data Umum    ||
| |           |                   |             | test, countryId: e19e1af2-1cd7...  |              ||
| +-----------+-------------------+-------------+------------------------------------+--------------+|
|                                                                                                    |
| Alasan / Catatan untuk HRD: [                                                                    ] |
|                                                                                                    |
| [!] Request failed with status code 400                                                            |
|                                                                                                    |
|                                                [ Kembali Edit ] [ Kirim ke HRD untuk Approval ]    |
+----------------------------------------------------------------------------------------------------+
```

#### SESUDAH (Hasil Perbaikan):
```text
+----------------------------------------------------------------------------------------------------+
| Tinjauan Pengajuan Perubahan Data Mandiri (Approval HRD)                                       [X] |
+----------------------------------------------------------------------------------------------------+
| Berikut adalah daftar usulan perubahan data Anda yang akan diverifikasi oleh HRD:                  |
| +---------------+--------------------------+---------------------------------------+--------------+|
| | Menu Profil   | Jenis Perubahan          | Rincian Data yang Diusulkan           | Kategori     ||
| +---------------+--------------------------+---------------------------------------+--------------+|
| | Alamat        | Tambah Alamat Domisili   | Jl. Rasuna Said No. 10, Jakarta       | Data Umum    ||
| | Rekening Bank | Tambah Rekening Payroll  | Bank Mandiri - 1230009876543          | Data Umum    ||
| | Info Pribadi  | Pembaruan Nomor Telepon  | 0812-9988-7766                        | Data Umum    ||
| +---------------+--------------------------+---------------------------------------+--------------+|
| (Kolom nilai baru mentah berformat JSON/GUID dihilangkan sepenuhnya)                               |
|                                                                                                    |
| Berkas Bukti Pendukung: (Hanya muncul jika mengubah data sensitif NIK/Nama/Tgl Lahir)              |
| [ Pilih Berkas (PDF/JPG/PNG, Maks. 5MB) ]                                                          |
|                                                                                                    |
| Alasan / Catatan untuk HRD (Opsional):                                                             |
| [ Pembaruan nomor rekening payroll dan alamat domisili terbaru...                                ] |
|                                                                                                    |
| (i) Aturan Proteksi Master Data: Data master rumah sakit TIDAK BERUBAH sebelum disetujui HRD.      |
|                                                                                                    |
|                                                [ Batal / Kembali ] [ Ajukan Permohonan ke HRD ]    |
+----------------------------------------------------------------------------------------------------+
```

---

## 4. Rincian Perbaikan

### `FIX-HRD-004-01` — Penyelarasan Kontrak Payload Pengajuan Alamat & Rekening Bank

- **Area:** Frontend (`employee-profile-changes-self-service-view.jsx`) & Backend (`EmployeeProfileChangeDtos.cs`, `EmployeeProfileChangeService.cs`)
- **Tindakan:**
  1. Pada frontend, perbaiki serializer pembuatan `detailsList`:
     - Sertakan properti `TargetEntityName`:
       - Alamat: `"WfpAddress"`
       - Rekening Bank: `"WfpBankAccount"`
       - Kontak Darurat: `"WfpEmergencyContact"`
       - Data Karyawan: `"MstEmployee"`
       - Profil Umum: `"MstWorkforceProfile"`
     - Sertakan properti `FieldGroup`: disesuaikan dengan nama grup ("Address", "BankAccount", "EmergencyContact", "PersonalData", "Contact").
     - Hapus payload dump JSON yang menggabungkan seluruh key-value ke string mentah. Kirim rincian spesifik yang jelas.
  2. Pada backend, daftarkan entitas relasi `WfpAddress`, `WfpBankAccount`, dan `WfpEmergencyContact` ke dalam daftar entitas yang didukung pada `AllowedFields` atau tangani usulan entitas baru (*proposed child entities*) pada saat workflow disetujui.

---

### `FIX-HRD-004-02` — Redesain Modal Tinjauan Pengajuan Perubahan Data

- **Area:** Frontend (`employee-profile-changes-self-service-view.jsx`)
- **Tindakan:**
  1. Hapus kolom "Nilai Baru (Diusulkan)" yang memajang teks mentah berformat kode JSON dan GUID database.
  2. Susun ulang tabel tinjauan menjadi 4 kolom bersih: **Menu Profil**, **Jenis Usulan**, **Rincian Data Usulan**, dan **Sifat Data**.
  3. Format rincian data usulan menggunakan teks manusiawi (misalnya: *"Alamat KTP: Jl. Sudirman No. 12, RT 01/RW 02"* alih-alih `"addressType: Identity, addressLine: Jl. Sudirman..."`).

---

### `FIX-HRD-004-03` — Penambahan Tab Kanonikal "Informasi Pribadi" & Eliminasi Modal Hero

- **Area:** Frontend (`employee-workforce-tabs.jsx`, `employee-profile-changes-self-service-view.jsx`)
- **Tindakan:**
  1. Hapus tombol *"Mode Ubah Data Pribadi"* dari Hero banner dan hapus komponen modal pop-up `Modal Ubah Data Pribadi (Ringkasan Profil)`.
  2. Daftarkan tab kanonikal baru `personalInfo` berlabel **"Informasi Pribadi"** di posisi awal setelah/sebelum Ringkasan Profil.
  3. Buat tampilan panel `PersonalInfoPanel` yang merender kartu data identitas kependudukan dan kontak pribadi dalam mode baca.
  4. Sediakan tombol *"Ubah Data Pribadi"* pada panel tersebut yang membuka editor laci (*drawer editor*) standar untuk memasukkan usulan edit data pribadi ke alur approval HRD.

---

### `FIX-HRD-004-04` — Penerapan Matriks Wewenang (Read-Only vs Approval HRD) & Pemuatan State Sub-Menu

- **Area:** Frontend (`employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`)
- **Tindakan:**
  1. Terapkan flag `readOnly: true` permanen pada portal ESS untuk tab: Penempatan Organisasi, Penempatan Jabatan, Relasi Atasan, Penetapan Gaji, Riwayat Kontrak, Riwayat Kepegawaian, Kredensial Medis, Hak Klinis, Asesmen Kompetensi, dan Rekam Kesehatan K3RS.
  2. Aktifkan mode kelola (*manage mode*) dan tombol tambah/ubah untuk: Informasi Pribadi, Alamat, Rekening Bank, Kontak Darurat, Pendidikan Formal, Dokumen Pribadi, dan Anggota Keluarga.
  3. Hubungkan pemanggilan data resource aktif melalui hook Redux/service agar data existing milik karyawan (seperti kontak darurat yang sudah ada) dapat muncul di tabel masing-masing menu.

---

### `FIX-HRD-004-05` — Jarak Aman Scroll Bawah (*Safe Scroll Space*) & Kontras Tipografi

- **Area:** Frontend (`employee-workforce-detail.css`, `wfp-workforce-detail.css`)
- **Tindakan:**
  1. Tambahkan safe space pada kontainer halaman:
     ```css
     .employee-workforce-page,
     .employee-workforce-shell,
     .wfp-page {
       padding-bottom: clamp(130px, 15vh, 160px) !important;
     }
     ```
  2. Tingkatkan ukuran font dan kontras:
     - `.wfp-kv-item span` (Label): `font-size: 11px; font-weight: 600; color: #475569;`
     - `.wfp-kv-item strong` (Value): `font-size: 13px; font-weight: 600; color: #0f172a;`
     - Berikan `min-height: 56px` dan padding yang proporsional.

---

### `FIX-HRD-004-06` — Pengayaan Data Endpoint `current-profile` & Sinkronisasi Header Workforce

- **Area:** Backend (`EmployeeProfileChangeDtos.cs`, `EmployeeProfileChangeService.cs`) & Frontend (`employee-profile-changes-self-service-view.jsx`, `workforce-profile-dashboard.jsx`)
- **Tindakan:**
  1. Pada backend DTO `EmployeeCurrentProfileResponse`, tambahkan: `PrimaryDepartmentName`, `PrimaryPositionName`, `GradeLevel`, `EmploymentType`, `ProfessionType`, `JoinDate`, `ContractEndDate`, `CreateDateTime`, `ProfilePhotoUrl`.
  2. Pada backend service `GetCurrentProfileAsync`, isi field-field tersebut dari tabel `MstEmployee` dan `MstWorkforceProfile`.
  3. Pada frontend ESS, petakan field tersebut ke properti kanonikal dashboard (`primaryDepartmentName`, `primaryPositionName`, `gradeLevel`, `joinDate`, `contractEndDate`).
  4. Pada dashboard workforce, set `alwaysVisible: true` pada field profil utama agar slot informasi selalu tampil rapi dengan tanda `"-"` bila belum diatur.

---

## 5. Urutan Pengerjaan

```text
Fase 1: Backend Kontrak & Data (Pondasi Transaksi)
  FIX-HRD-004-06 (Pengayaan DTO current-profile & data kepegawaian)
    └── FIX-HRD-004-01 (Dukungan entitas relasi WFP & validasi DTO)
          │
Fase 2: Frontend Alur Data & Struktur Menu
          ├── FIX-HRD-004-03 (Tab Informasi Pribadi & eliminasi modal hero)
          ├── FIX-HRD-004-04 (Matriks hak akses Read-Only vs Approval & state Kontak Darurat)
          │
Fase 3: Frontend Presentasi & User Experience
          ├── FIX-HRD-004-02 (Pembersihan modal tinjauan dari dump teks string JSON)
          └── FIX-HRD-004-05 (Safe space scroll bawah & kontras tipografi kartu)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan yang Dibutuhkan | Rekomendasi Solusi | Perbaikan yang Ditahan |
| :---: | :--- | :--- | :---: |
| **K-01** | Pengesahan Matriks Pembagian Wewenang: Menetapkan secara permanen bahwa Penempatan Organisasi, Jabatan, Gaji, Kontrak, Kredensial, dan Hak Klinis berstatus **Mutlak Read-Only** bagi karyawan di portal ESS. | **Disetujui.** Sesuai tata kelola resmi RS. | `FIX-HRD-004-04` |
| **K-02** | Pengesahan Alur Usulan Mandiri: Menghilangkan modal pop-up hero dan mengalihkan seluruh pengelolaan data identitas pribadi ke Tab Kanonikal **"Informasi Pribadi"**. | **Disetujui.** Memberikan alur interaksi terstandar dan rapi. | `FIX-HRD-004-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Lokasi Berkas | Yang Diubah |
| :--- | :--- | :--- |
| **Buku Panduan Sistem Modul HRD V2** | `docs/Buku_Panduan_Sistem_Modul_HRD_Quilvian_V2_.md` | Penjelasan alur pengajuan mandiri melalui tab Informasi Pribadi dan batasan data read-only |
| **Plan Repair 003 Lama** | `docs/plan-repair/plan-repair-003-employee-self-service-profile-change.md` | Pencatatan suksesi perbaikan ke `PLAN-REPAIR-HRD-004` |

---

## 8. Verifikasi Menyeluruh

| Langkah | Aksi | Hasil yang Diharapkan |
| :---: | :--- | :--- |
| **1** | Buka halaman `/self-services/human-resource/employee/profile-changes`. | Halaman terbuka, bilah hero bersih tanpa tombol modal pop-up, header card memuat foto avatar, Departemen ("Keperawatan"), Posisi ("Perawat Rawat Inap"), Grade ("STAFF"), Lokasi, Tanggal Bergabung, dan Akhir Kontrak. |
| **2** | Periksa menu samping kiri. | Terdapat tab kanonikal **"Informasi Pribadi"** di posisi awal, serta tab-tab manajerial (Organisasi, Jabatan, Gaji, Kredensial) terkunci rapi sebagai read-only. |
| **3** | Buka tab **"Informasi Pribadi"** dan klik *"Ubah Data Pribadi"*. | Laci editor terbuka, ubah nomor telepon/alamat, klik simpan usulan. Tampil notifikasi usulan masuk ke daftar perubahan siap diajukan. |
| **4** | Buka tab **"Alamat"** dan klik *"+ Tambah Data"*. | Form tambah alamat muncul, isi alamat domisili, klik simpan usulan. Berhasil masuk ke daftar usulan tanpa error 400. |
| **5** | Buka tab **"Kontak Darurat"** dan klik *"+ Tambah Data"*. | Form tambah kontak darurat muncul dan dapat diisi dengan lancar. |
| **6** | Tekan tombol *"Simpan dan Ajukan Perubahan Data"*. | Modal tinjauan terbuka: tabel bersih tanpa kolom nilai baru mentah atau kode GUID teknis, teks alasan terisi rapi. |
| **7** | Tekan *"Kirim ke HRD untuk Approval"*. | Request `POST /api/v1/self-services/human-resource/profile-changes` menghasilkan respon **HTTP 200 OK / 201 Created** (bebas error 400). Permohonan masuk ke workflow HRD dengan status `Submitted`. |
| **8** | Gulir layar ke paling bawah. | Seluruh kartu informasi dan kartu kepatuhan dokumen berada di atas bilah footer fixed, teks tajam, jelas terbaca, dan tidak buram. |

---

## 9. Riwayat Dokumen

| Tanggal | Penulis | Perubahan |
| :---: | :---: | :--- |
| **2026-11-11** | Antigravity AI Agent | Penyusunan rencana perbaikan resmi `PLAN-REPAIR-HRD-004` sebagai respon atas diagnosis `ISSUE-HRD-004`. |
