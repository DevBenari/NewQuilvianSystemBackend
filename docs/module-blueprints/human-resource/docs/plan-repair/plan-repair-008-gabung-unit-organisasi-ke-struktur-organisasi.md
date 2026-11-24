# PLAN-REPAIR-008 — Gabungkan Unit Organisasi ke Struktur Organisasi, Pisahkan Departemen dan Posisi ke Menu Sendiri

```yaml
plan_id: PLAN-REPAIR-HRD-008
issue: ../issue/issue-008-gabung-unit-organisasi-ke-struktur-organisasi.md
status_rencana: SELESAI
tanggal_rencana: "2026-11-24"
diputuskan_oleh: "Pengguna (pemilik teknis, HRD-DEC-015)"
tanggal_keputusan: "2026-11-24"
basis_source_backend: "3ef27c96 (MHamzah)"
basis_source_frontend: "fe8aebf2f (HamzahV2)"
```

Arah perbaikan (Opsi A) dipilih pemilik pada 24-11-2026, lalu seluruh rencana disetujui pada hari
yang sama: "oke saya setujui kerjakan semua dan selesaikan". Keputusan tercatat sebagai
`HRD-DEC-248` s.d. `HRD-DEC-250`.

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-HRD-008-01` | `ISS-HRD-008-01`, `ISS-HRD-008-02` | Revisi dokumen desain Menu 05 dan catat keputusan Opsi A | Dokumen | 1 | dokumen (`HRD-DEC-248`) | ✅ SELESAI | `../../00-interview-decisions.md` bagian 58; `../../03-frontend-architecture.md` 34.7; roadmap bagian 32 |
| `FIX-HRD-008-03` | `ISS-HRD-008-01`, `ISS-HRD-008-T1`, `ISS-HRD-008-T4` | Halaman unit menjadi "Struktur Organisasi" dengan mode Daftar dan Hierarki; tombol pohon membuka detail unit | FE | 1 | `FE-HRD-410` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-410.md` |
| `FIX-HRD-008-02` | `ISS-HRD-008-02`, `ISS-HRD-008-T4` | Halaman Departemen dan halaman Posisi berdiri sendiri | FE | 1 | `FE-HRD-411` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-411.md` |
| `FIX-HRD-008-04` | `ISS-HRD-008-01`, `ISS-HRD-008-02` | Sidebar baru, pengalihan alamat lama, tautan Penempatan Awal, dan uji sidebar | FE | 1 | `FE-HRD-412` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-412.md` |
| `FIX-HRD-008-05` | `ISS-HRD-008-02`, `ISS-HRD-008-T2`, `ISS-HRD-008-T4` | Form unit: level otomatis dan petunjuk memilih departemen | FE | 2 | `FE-HRD-413` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-413.md` |
| `FIX-HRD-008-06` | `ISS-HRD-008-T3` | Respons detail unit membawa nama legal entity, fasilitas, dan departemen | BE | 3 | `BE-HRD-463` | ✅ SELESAI | `../../task/report/backend/BE-HRD-463.md` |
| `FIX-HRD-008-07` | `ISS-HRD-008-T3` | Detail unit menampilkan nama, bukan ID | FE | 3 | `FE-HRD-414` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-414.md` |
| `FIX-HRD-008-08` | `ISS-HRD-008-T5` | Server menolak Departemen Terkait yang tidak ada atau nonaktif | BE | 3 | `BE-HRD-464` | ✅ SELESAI | `../../task/report/backend/BE-HRD-464.md` |

**Ringkasan: 8 dari 8 perbaikan selesai (24 November 2026).** Uji runtime dengan backend dan akun nyata belum dijalankan; backend lokal perlu di-restart agar `FIX-HRD-008-06` terkirim ke layar.

Register diurutkan menurut urutan pengerjaan, bukan nomor. `FIX-HRD-008-03` dikerjakan sebelum
`FIX-HRD-008-02` supaya pohon sudah tampil di tempat barunya sebelum dicabut dari tempat lama.

---

## 2. Solusi Terpilih per Temuan

### ISS-HRD-008-01 dan ISS-HRD-008-02 — Susunan menu Organisasi

Kedua butir diselesaikan oleh satu susunan menu, jadi opsinya dibahas bersama.

**Tahap 1: bentuk menu.**

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Departemen dan Posisi masing-masing mendapat menu sendiri. Struktur Organisasi berisi daftar unit, pohon, dan tambah/ubah unit. Menu Unit Organisasi dihapus. | Sesuai kalimat analis. Satu menu untuk satu tugas, sama seperti Rumpun Jabatan, Level Jabatan, dan Grade. | Sidebar bertambah satu butir bersih: dua butir dihapus, tiga ditambah |
| B | Departemen dan Posisi tetap berupa tab di Struktur Organisasi, ditambah tab "Struktur" | Perubahan sidebar lebih sedikit | Departemen tetap ditambah dari menu Struktur Organisasi, jadi bertentangan dengan kalimat 3 analis |

**Solusi terpilih: Opsi A.** Pilihan ini sudah diputuskan pemilik pada 24-11-2026.

**Tahap 2: alamat halaman.** Keputusan ini teknis dan tidak mengubah apa yang dilihat petugas.

| Opsi | Struktur Organisasi | Departemen / Posisi | Kelebihan | Kekurangan |
| --- | --- | --- | --- | --- |
| A1 | Memakai halaman unit yang sudah ada: `/hr/master-data/organization-unit` | `/hr/master-data/organization/departments` dan `/hr/master-data/organization/positions` | Halaman tambah, detail, dan ubah unit sudah ada dan tidak disentuh | Alamat di peramban berbunyi "organization-unit" walau menunya "Struktur Organisasi" |
| A2 | Memakai alamat lama `/hr/master-data/organization`, lalu daftar unit dipindah ke sana | Sama dengan A1 | Alamat dan nama menu selaras | Halaman tambah, detail, dan ubah unit tetap tinggal di `/organization-unit`, jadi menu aktif di sidebar hilang saat petugas berada di sana. Lebih banyak berkas berpindah |

**Solusi terpilih: A1.**

1. Halaman tambah, detail, dan ubah unit beserta token rutenya tetap bekerja tanpa diubah
   (`src/app/hr/master-data/organization-unit/create`, `[slug]`, `[slug]/update`).
2. Sidebar menandai menu aktif lewat awalan alamat terpanjang
   (`left-sidebar-menu-handle.jsx:239-245`). Dengan A1, halaman
   `/hr/master-data/organization-unit/create` tetap menyalakan "Struktur Organisasi". Halaman
   `/hr/master-data/organization/departments/create` menyalakan "Departemen".
3. Halaman Departemen dan Posisi diletakkan sejajar folder `[type]/create` dan `[type]/[slug]`
   yang sudah ada. Cukup satu berkas halaman baru: `src/app/hr/master-data/organization/[type]/page.jsx`.
4. Setiap halaman dijaga satu hak akses. Struktur Organisasi memakai `OrganizationUnit`.
   Departemen dan Posisi memakai `Organization`.
5. Dua tautan yang sudah ada tetap sah tanpa diubah: tujuan `MASTER_ORGANIZATION_UNIT` di
   Penempatan Awal (`initial-placement-batch-view.jsx:46`) dan tombol di pohon.

**Kenapa bukan opsi lain.** Opsi B bertentangan dengan kalimat 3 analis dan sudah ditolak
pemilik. A2 memindahkan lebih banyak berkas hanya demi nama alamat, dan membuat sidebar
kehilangan penanda di halaman tambah, detail, dan ubah unit.

**Mode bawaan halaman Struktur Organisasi: Daftar.** Halaman unit sekarang juga terbuka di
daftar, dan tombol "+ Tambah Unit Organisasi" ada di mode itu. Pohon tetap satu klik di tombol
`Hierarki`.

### ISS-HRD-008-T1 — Tombol di pohon tidak membuka unit yang dipilih

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Tombol membuka halaman detail unit secara langsung memakai token rute, sama seperti klik dua kali di daftar | Satu klik sampai ke detail. Memakai mekanisme yang sudah ada (`use-master-data-organization-unit.jsx:362-378`) | Komponen pohon perlu tahu konfigurasi token unit |
| B | Halaman daftar diajari membaca `?search=` dari alamat | Tautan lama ikut bekerja | Petugas tetap harus klik dua kali barisnya. Menambah logika baca alamat ke hook daftar |

**Solusi terpilih: Opsi A.**

1. Desain Menu 05 baris 304 meminta tombol ini membuka **detail** unit.
2. Pola token yang sama sudah dipakai daftar unit, jadi tidak ada cara baru.

**Kenapa bukan opsi lain.** Opsi B tetap menyisakan satu langkah cari-dan-klik bagi petugas.

### ISS-HRD-008-T2 — Isian Level Hierarki diabaikan server

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Hapus isian dari form, lalu tampilkan keterangan "Level ditentukan otomatis dari unit induk" | Layar jujur terhadap perilaku server | Petugas tidak melihat angka level di form; angka tetap tampil di daftar, detail, dan pohon |
| B | Isian tetap tampil terkunci, berisi level induk + 1 yang dihitung di layar | Petugas melihat angkanya sebelum menyimpan | Rumus server disalin ke layar dan bisa menyimpang di kemudian hari |
| C | Server diubah agar memakai angka dari petugas | — | Bertentangan dengan `FIN-HRD-05-BE-001` dan desain Menu 05 baris 76 |

**Solusi terpilih: Opsi A.**

1. Server sudah memegang rumus level (`OrganizationUnitController.cs:418-440`, `505-516`).
2. Tidak ada logika yang disalin ke layar.

**Kenapa bukan opsi lain.** Opsi B menggandakan rumus. Opsi C membalik keputusan yang sudah
dikunci.

### ISS-HRD-008-T3 — Detail unit menampilkan ID mentah

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Server menambahkan `LegalEntityName`, `HospitalSiteName`, dan `DepartmentName` ke respons detail. Field yang sudah ada tidak diubah. Layar menampilkan nama itu. | Satu permintaan, nama selalu benar, termasuk untuk data yang sudah nonaktif | Menyentuh backend |
| B | Layar mencari nama dari tiga daftar pilihan (legal entity, fasilitas, departemen) | Tanpa perubahan backend | Halaman detail memanggil tiga permintaan tambahan. Data nonaktif tidak ada di daftar pilihan, jadi namanya tetap tidak muncul |

**Solusi terpilih: Opsi A.**

1. Penambahan field tidak mengubah pemakai lain. Hal ini sesuai disiplin kontrak API di
   `AGENTS.md` backend ("pertahankan backward compatibility").
2. Pola yang sama sudah dipakai untuk unit induk (`ParentUnitName`, `OrganizationUnitDtos.cs:21`).

**Kenapa bukan opsi lain.** Opsi B gagal pada unit yang terhubung ke departemen nonaktif, padahal
justru kasus itu yang paling perlu terbaca.

### ISS-HRD-008-T4 — Teks Inggris

Hanya ada satu cara yang wajar: mengganti teks layar ke Bahasa Indonesia **di dalam perbaikan
yang memang menyentuh berkasnya**, yaitu `FIX-HRD-008-02`, `FIX-HRD-008-03`, dan
`FIX-HRD-008-05`. Pesan server tidak disentuh supaya cakupan tetap kecil (lihat issue T4).

### ISS-HRD-008-T5 — Departemen Terkait tidak diperiksa server

Hanya ada satu cara yang wajar: tambahkan pemeriksaan di `ValidateRequestAsync`. Pemeriksaannya
mengikuti pola legal entity dan fasilitas yang sudah ada di berkas yang sama
(`OrganizationUnitController.cs:749-770`).

---

## 3. Skema Tampilan Sebelum → Sesudah

Tanda rangka mengikuti kontrak plan repair: `[ Tombol ]`, `[ isian ]`, `▾` daftar pilihan,
`*` wajib, `[ o]` sakelar. Data contoh memakai nama samaran.

### 3.1 Sidebar grup "Organisasi & Struktur Jabatan"

**SEBELUM** (`menu-items.jsx:229-299`)

```text
Organisasi & Struktur Jabatan
  Struktur Organisasi      → tab Departemen | Posisi + tombol Hierarki
  Legal Entity
  Fasilitas Rumah Sakit
  Unit Organisasi          → daftar unit + tambah/ubah unit
  Lokasi Kerja
  Pusat Biaya
  Rumpun Jabatan
  Level Jabatan
  Grade Karyawan
  Uraian Tugas
  Atasan Langsung
```

**SESUDAH**

```text
Organisasi & Struktur Jabatan
  Struktur Organisasi      → daftar unit + pohon + tambah/ubah unit
  Departemen               → master departemen
  Posisi                   → master posisi
  Legal Entity
  Fasilitas Rumah Sakit
  Lokasi Kerja
  Pusat Biaya
  Rumpun Jabatan
  Level Jabatan
  Grade Karyawan
  Uraian Tugas
  Atasan Langsung
```

| Butir | `key` | `pathname` | Hak akses halaman | Keterangan |
| --- | --- | --- | --- | --- |
| Struktur Organisasi | `corporateOrganizationUnit` | `/hr/master-data/organization-unit` | `OrganizationUnit : Read` | `key` lama dipakai ulang, label diganti |
| Departemen | `corporateDepartment` (baru) | `/hr/master-data/organization/departments` | `Organization : Read` | Halaman baru, isi dari tab lama |
| Posisi | `corporatePosition` (baru) | `/hr/master-data/organization/positions` | `Organization : Read` | Halaman baru, isi dari tab lama |
| *(dihapus)* | `corporateOrganization` | `/hr/master-data/organization` | — | Alamat dialihkan, lihat `FIX-HRD-008-04` |

Butir lain tidak berubah. Grup berisi 12 butir, sebelumnya 11. Butir di grup ini tidak memakai
`requiredPermission`, jadi kebiasaan tampil di sidebar tidak berubah.

### 3.2 Halaman Struktur Organisasi — mode Daftar

**SEBELUM.** Halaman `/hr/master-data/organization`, berjudul "Manajemen Organization", berisi
tab Departemen dan Posisi. Halaman unit terpisah di menu "Unit Organisasi", berjudul
"Manajemen Unit Organisasi".

```text
+--------------------------------------------------------------------------------+
| Master Data                                                                     |
| Manajemen Organization                                    [ Daftar ] [Hierarki] |
| Kelola departemen, posisi, dan gambaran hierarki organisasi ...                |
+--------------------------------------------------------------------------------+
| [Departemen] [Posisi]                                  [ + Tambah Departemen ]  |
| [ Cari kode, nama, atau departemen... ] [Tgl mulai] [Tgl akhir] [Period ▾] ... |
| Tabel departemen ...                                                            |
+--------------------------------------------------------------------------------+
```

**SESUDAH.** Halaman `/hr/master-data/organization-unit`.

```text
+--------------------------------------------------------------------------------+
| Data Master                                                                     |
| Struktur Organisasi                                       [ Daftar ] [Hierarki] |
| Susun unit kerja rumah sakit: direktorat, divisi, instalasi, departemen,        |
| seksi, tim, dan unit.                                                           |
+--------------------------------------------------------------------------------+
| [Total Unit 42] [Aktif 40] [Nonaktif 2] [Operasional 31] [Unit Teratas 1]       |
+--------------------------------------------------------------------------------+
| Daftar Unit Organisasi                              [ + Tambah Unit Organisasi ]|
| [ Cari kode, nama, tipe, atau deskripsi... ] [Tgl mulai] [Tgl akhir] [Periode ▾]|
| [Status ▾] [25 baris ▾] [ Atur Ulang ]                                          |
+----+----------+----------------------------+-----------+-------------------+----+
| No | Kode     | Nama Unit                  | Tipe      | Unit Induk        | .. |
|  1 | ORG-0001 | Direktorat Utama           | Direktorat| —                 |    |
|  2 | ORG-0007 | Direktorat Penunjang Medis | Direktorat| Direktorat Utama  |    |
|  3 | ORG-0031 | Instalasi Farmasi          | Instalasi | Direktorat Penun… |    |
+----+----------+----------------------------+-----------+-------------------+----+
| ‹ 1 2 ›                                                                         |
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Kepala halaman | Judul "Struktur Organisasi", deskripsi, tombol mode | Teks tetap di `ORGANIZATION_UNIT_CONFIG` | `Hero` + tombol mode yang dipindah dari `master-data-organization-view.jsx:369-384` |
| Kartu ringkasan | Total, Aktif, Nonaktif, Operasional, Unit Teratas (sebelumnya "Root Unit") | `GET .../organization-units/summary` | `SummaryCards` (sudah ada) |
| Penyaring + tombol tambah | Pencarian, tanggal, periode, status, jumlah baris | `GET .../organization-units/filters/metadata` | `DataFilter`, `FilterSelect`, `FilterDatePicker` (sudah ada) |
| Tabel | Kode, Nama Unit, Tipe, Unit Induk (sebelumnya "Parent Unit"), Level, Sub-Unit (sebelumnya "Child"), Operasional, Berlaku Sejak, Tanggal Dibuat, Status | `GET .../organization-units` | `DataTable` (sudah ada) |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| `Daftar` | Tombol mode | Selalu; tertekan saat mode Daftar | Menampilkan wilayah di atas |
| `Hierarki` | Tombol mode | Selalu | Menampilkan pohon, lihat 3.3 |
| `+ Tambah Unit Organisasi` | Utama | Tidak sedang memuat | Membuka `/hr/master-data/organization-unit/create` |
| Klik dua kali baris | — | Ada data | Membuka detail unit (sudah ada) |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Tabel dan kartu memakai keadaan memuat bawaan |
| Kosong | "Data unit organisasi tidak ditemukan." beserta saran ubah penyaring atau tambah data |
| Gagal | Pesan galat di atas tabel (sudah ada) |
| Tidak berhak (403) | `AccessDeniedGate` menutup seluruh halaman (sudah ada) |

### 3.3 Halaman Struktur Organisasi — mode Hierarki

**SEBELUM.** Pohon berada di halaman "Manajemen Organization". Tombolnya berlabel
"Lihat Detail Unit Lengkap" dan membuka daftar unit di menu lain.

```text
+----------------------------------------------+---------------------------------+
| ▾ Direktorat Utama                           | Instalasi Farmasi      (Aktif)  |
|   ▾ Direktorat Penunjang Medis               | Tipe Unit      : Installation   |
|     • Instalasi Farmasi   <terpilih>         | Parent Unit    : Direktorat ... |
|                                              | [ Lihat Detail Unit Lengkap ]   |  → daftar penuh menu lain
+----------------------------------------------+---------------------------------+
```

**SESUDAH**

```text
+--------------------------------------------------------------------------------+
| Struktur Organisasi                                       [Daftar] [ Hierarki ] |
+--------------------------------------------------------------------------------+
| Pohon Unit Organisasi                                                           |
| Tingkatan unit kerja dari unit teratas hingga unit operasional.                 |
|        [Tampilkan Semua Status] [Buka Semua] [Tutup Semua] [Muat Ulang]         |
+----------------------------------------------+---------------------------------+
| ▾ Direktorat Utama                           | Instalasi Farmasi      (Aktif)  |
|   ▾ Direktorat Penunjang Medis               | Kode        : ORG-0031          |
|     ▸ Instalasi Laboratorium                 | Tipe Unit   : Instalasi         |
|     • Instalasi Farmasi   <terpilih>         | Level       : Tingkat 3         |
|   ▸ Direktorat Pelayanan Medis               | Unit Induk  : Direktorat Penun… |
|   ▸ Direktorat Umum dan Keuangan             | Sub-Unit    : 0 unit            |
|                                              | Operasional : Ya                |
|                                              | Deskripsi   : -                 |
|                                              | [ Buka Detail Unit ]            |
+----------------------------------------------+---------------------------------+
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Bilah alat pohon | Judul, keterangan, empat tombol | — | `OrganizationHierarchyView` (sudah ada, berkas tidak dipindah) |
| Pohon | Unit bertingkat, bisa dibuka-tutup dengan tetikus dan papan ketik | `GET .../organization-units/hierarchy?onlyActive=` | `OrganizationHierarchyView` |
| Panel detail | Ringkasan unit terpilih; tipe unit memakai label Indonesia dari `ORGANIZATION_UNIT_TYPE_OPTIONS` | Node pohon terpilih | `OrganizationHierarchyView`, `StatusBadge` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| `Tampilkan Semua Status` / `Hanya Unit Aktif` | Sekunder | Selalu | Memuat ulang pohon dengan atau tanpa unit nonaktif |
| `Buka Semua` / `Tutup Semua` | Sekunder | Ada data | Membuka atau menutup semua cabang |
| `Muat Ulang` (sebelumnya "Refresh") | Sekunder | Selalu | Mengambil ulang pohon |
| `Buka Detail Unit` (sebelumnya "Lihat Detail Unit Lengkap") | Utama | Ada unit terpilih | Membuka `/hr/master-data/organization-unit/<token>`, yaitu detail unit terpilih |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | "Memuat struktur hierarki..." (sudah ada) |
| Kosong | "Belum Ada Data Hierarki" (sudah ada) |
| Gagal | "Terjadi Kesalahan" + tombol "Coba Lagi" (sudah ada) |
| Tidak berhak (403) | `AccessDeniedGate` halaman. Halaman dan pohon kini memakai hak akses yang sama, jadi tidak ada lagi keadaan "daftar tampil, pohon gagal" |

Pohon tetap **hanya untuk dilihat**. Tidak ada geser-pindah unit dan tidak ada ubah langsung di
pohon, sesuai desain Menu 05 baris 91 dan 304. Tambah dan ubah unit dilakukan di mode Daftar
atau dari halaman detail.

### 3.4 Halaman Departemen dan halaman Posisi

**SEBELUM.** Keduanya berupa tab di halaman "Manajemen Organization" (lihat 3.2 SEBELUM).

**SESUDAH — Departemen** (`/hr/master-data/organization/departments`)

```text
+--------------------------------------------------------------------------------+
| Master Data                                                                     |
| Departemen                                                                      |
| Kelola daftar departemen yang dipakai data pegawai dan unit organisasi.         |
+--------------------------------------------------------------------------------+
| [Kartu ringkasan yang sudah ada]                                                |
+--------------------------------------------------------------------------------+
| Daftar Departemen                                       [ + Tambah Departemen ] |
| [ Cari kode atau nama departemen... ] [Tgl mulai] [Tgl akhir] [Periode ▾]       |
| [Status ▾] [25 baris ▾] [ Atur Ulang ]                                          |
+----+-----------+---------------------------+---------+--------------------------+
| No | Kode      | Nama Departemen           | Status  | (kolom lain yang ada)    |
|  1 | DPT-001   | Farmasi                   | Aktif   |                          |
|  2 | DPT-002   | Keperawatan               | Aktif   |                          |
+----+-----------+---------------------------+---------+--------------------------+
```

**SESUDAH — Posisi** (`/hr/master-data/organization/positions`). Susunannya sama dengan
Departemen, dengan tiga perbedaan: penyaring tambahan "Semua Departemen ▾", tombol
"+ Tambah Posisi", dan tombol "Buka Uraian" di setiap baris (sudah ada,
`organization-table-columns.jsx:202-217`).

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Kepala halaman | Judul "Departemen" atau "Posisi". Tanpa tab dan tanpa tombol Daftar/Hierarki | Teks per jenis di `organization-constants.jsx` | `Hero` |
| Kartu ringkasan | Sama seperti sekarang | `GET .../organization/summary` | `SummaryCards` |
| Penyaring + tombol tambah | Sama seperti tab sekarang | `GET .../organization/filters/metadata`, `.../departments/options` | `DataFilter`, `FilterSelect`, `FilterDatePicker` |
| Tabel | Kolom sama seperti tab sekarang | `GET .../organization/departments` atau `.../positions` | `DataTable` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| `+ Tambah Departemen` / `+ Tambah Posisi` | Utama | Tidak sedang memuat | Membuka `/hr/master-data/organization/{departments\|positions}/create` (sudah ada) |
| Klik dua kali baris | — | Ada data | Membuka detail (sudah ada) |
| `Buka Uraian` (Posisi saja) | Sekunder | Ada baris | Membuka Uraian Tugas posisi itu (sudah ada) |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | "Mengambil data departemen..." atau "Mengambil data posisi..." (sebelumnya "Mengambil data organization...") |
| Kosong | "Data departemen tidak ditemukan." atau "Data posisi tidak ditemukan." |
| Gagal | `InformationAlert` (sudah ada) |
| Tidak berhak (403) | `AccessDeniedGate` (sudah ada) |
| Alamat jenis tidak dikenal, misalnya `/hr/master-data/organization/abc` | Halaman tidak ditemukan |

### 3.5 Form tambah dan ubah unit

**SEBELUM** (`organization-unit-constants.jsx:41-142`)

```text
| Legal Entity *           [ Pilih legal entity             ▾ ] |
| Fasilitas Rumah Sakit    [ Tidak terikat fasilitas        ▾ ] |
| Parent Unit Organisasi   [ Tidak memiliki parent          ▾ ] |
| Departemen Referensi     [ Tidak terkait departemen       ▾ ] |
| Nama Unit Organisasi *   [                                  ] |
| Tipe Unit *              [ Unit                           ▾ ] |
| Level Hierarki *         [ 1   ]  Level minimal 1. Sesuaikan  |
|                                   dengan kedalaman hierarki.  |
| Unit Operasional         [ o]                                 |
| Tanggal Mulai Berlaku    [          ]                         |
| Tanggal Selesai Berlaku  [          ]                         |
| Deskripsi                [                                  ] |
```

**SESUDAH**

```text
| Legal Entity *           [ Pilih legal entity             ▾ ] |
| Fasilitas Rumah Sakit    [ Tidak terikat fasilitas        ▾ ] |
| Unit Induk               [ Tidak memiliki unit induk      ▾ ] |
|                          Level unit ditentukan otomatis dari  |
|                          unit induk.                          |
| Departemen Terkait       [ Tidak terkait departemen       ▾ ] |
|                          Pilih dari master Departemen.        |
|                          Departemen baru ditambahkan lewat    |
|                          menu Departemen.                     |
| Nama Unit Organisasi *   [                                  ] |
| Tipe Unit *              [ Unit                           ▾ ] |
| Unit Operasional         [ o]                                 |
| Tanggal Mulai Berlaku    [          ]                         |
| Tanggal Selesai Berlaku  [          ]                         |
| Deskripsi                [                                  ] |
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Isian relasi | Legal Entity, Fasilitas, Unit Induk, Departemen Terkait | Daftar pilihan yang sudah dimuat editor (`use-master-data-organization-unit-editor.jsx:131-136`) | Field select bawaan editor |
| Isian unit | Nama, Tipe, Unit Operasional, tanggal berlaku, deskripsi | — | Field bawaan editor |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Simpan / Batal | Sama seperti sekarang | Sama seperti sekarang | Payload **tidak lagi** membawa `levelNumber`. Nilai bawaan DTO adalah 1 dan lolos `[Range(1, int.MaxValue)]` (`OrganizationUnitDtos.cs:157-158`). Setelah itu server menimpanya dengan hasil hitungan |

| Keadaan | Tampilan |
| --- | --- |
| Galat isian dari server | Sama seperti sekarang |
| Daftar departemen kosong | Pilihan hanya "Tidak terkait departemen". Petunjuk di bawah isian memberi tahu tempat menambahkannya |

### 3.6 Halaman detail unit

**SEBELUM** (`organization-unit-constants.jsx:348-475`)

```text
| Kode Unit        : ORG-0031                                   |
| Nama Unit        : Instalasi Farmasi                          |
| Tipe Unit        : Instalasi                                  |
| Parent Unit      : Direktorat Penunjang Medis                 |
| Level Hierarki   : 3                                          |
| ID Legal Entity  : 3f2a9c1e-…                                 |
| ID Fasilitas RS  : 8b10d4f2-…                                 |
| ID Parent Unit   : 51c7e0aa-…                                 |
| ID Departemen    : c93e1b77-…                                 |
```

**SESUDAH**

```text
| Kode Unit             : ORG-0031                              |
| Nama Unit             : Instalasi Farmasi                     |
| Tipe Unit             : Instalasi                             |
| Unit Induk            : Direktorat Penunjang Medis            |
| Level Hierarki        : 3                                     |
| Legal Entity          : PT Contoh Sehat Sejahtera             |
| Fasilitas Rumah Sakit : RS Contoh Sehat                       |
| Departemen Terkait    : Farmasi                               |
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Ringkasan unit | Seperti di atas. Baris "ID Parent Unit" dibuang karena namanya sudah ada di "Unit Induk" | `GET .../organization-units/{id}` dengan field nama baru dari `FIX-HRD-008-06` | Halaman detail bawaan |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali / Perbarui / Hapus | Sama seperti sekarang | — | Kembali ke `/hr/master-data/organization-unit`, yaitu menu Struktur Organisasi |

| Keadaan | Tampilan |
| --- | --- |
| Unit tanpa fasilitas atau departemen | "-" |
| Departemen terkait sudah nonaktif | Nama tetap tampil, karena server mengirim nama apa pun status departemennya |

---

## 4. Rincian Perbaikan

### FIX-HRD-008-01 — Revisi dokumen desain Menu 05 dan catat keputusan Opsi A

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-01`, `ISS-HRD-008-02` (sisi dokumen) |
| **Area** | Dokumen blueprint |
| **Jenis perubahan** | Dokumen desain, lewat jalur blueprint yang berwenang (`manage-module-blueprint` dalam MODULE BLUEPRINT MODE) |
| **Berkas yang diubah** | Lihat bagian 7 |
| **Radius dampak** | Dokumen saja. Agent berikutnya membaca desain yang sudah selaras, jadi tidak "memperbaiki balik" source ke bentuk lama |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Catat keputusan baru di `00-interview-decisions.md` dengan nomor `HRD-DEC` berikutnya. Isinya
   KD-01 dari issue, tanggal 24-11-2026, dan rujukan `ISSUE-HRD-008`.
2. Revisi baris desain Menu 05 dan tabel 34.2 sesuai bagian 7.
3. Daftarkan `FIX-HRD-008-02` sampai `FIX-HRD-008-08` sebagai task di roadmap. Task frontend
   masuk `frontend-roadmap.md` slice `S-FIN-M05`, task backend masuk `backend-roadmap.md`. Nomor
   task diambil saat pendaftaran, lalu dicatat balik ke kolom Task ID di register ini.

**Acceptance criteria.**

1. Keputusan Opsi A tercatat dengan nomor `HRD-DEC`, tanggal 2026-11-24, dan tautan ke issue ini.
2. Desain Menu 05 tidak lagi mewajibkan tab Department/Position di `/hr/master-data/organization`.
   Baris lama diberi catatan amandemen, tidak dihapus diam-diam.
3. Tabel 34.2 baris "Struktur Organisasi & Jabatan" menyebut owner navigasi yang baru.
4. Register di bagian 1 mencantumkan Task ID untuk setiap perbaikan yang disetujui.

**Verifikasi.** Baca ulang setiap baris di bagian 7. Cari kalimat "pertahankan tab Position pada
`/hr/master-data/organization`" dan pastikan setiap kemunculannya sudah diberi catatan amandemen.

**Risiko.** Nomor `HRD-DEC` bertabrakan dengan sesi lain yang menulis keputusan bersamaan.
Deteksinya: periksa nomor tertinggi tepat sebelum menulis.

---

### FIX-HRD-008-03 — Halaman unit menjadi "Struktur Organisasi" dengan mode Daftar dan Hierarki

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-01`, `ISS-HRD-008-T1`, `ISS-HRD-008-T4` (teks halaman unit dan pohon) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/components/view/hr/master-data/organization-unit/master-data-organization-unit-view.jsx`: tombol `Daftar \| Hierarki` di `Hero` dan render `OrganizationHierarchyView` pada mode Hierarki · `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx`: `heroTitle`, `heroDescription`, judul kartu dan kolom ("Unit Teratas", "Unit Induk", "Sub-Unit") · `src/components/view/hr/master-data/organization/hierarchy/organization-hierarchy-view.jsx`: tombol detail memakai token rute, label tipe unit, dan teks · `src/app/hr/master-data/organization-unit/page.jsx`: judul tab peramban "Struktur Organisasi" |
| **Radius dampak** | Halaman daftar unit. Komponen pohon hanya dipakai di dua view: halaman unit dan halaman organisasi lama. Halaman lama dilepas di `FIX-HRD-008-02`. Halaman tambah, detail, dan ubah unit tidak disentuh |
| **Bergantung pada** | `FIX-HRD-008-01` |

**Langkah.**

1. Pindahkan pola tombol mode dari `master-data-organization-view.jsx:255-257` dan `369-384` ke
   `Hero` halaman unit. Pakai stylesheet yang sama
   (`@/style/hr/master-data/organization/organization-hierarchy.module.css`). Mode bawaan: Daftar.
2. Pada mode Hierarki, render `OrganizationHierarchyView`. Pada mode Daftar, render kartu,
   penyaring, dan tabel yang sudah ada.
3. Di `organization-hierarchy-view.jsx:178-183`, ganti navigasi `?search=` dengan pembuatan token
   lewat `registerPrivateRouteToken`. Pakai `scope` dan `type` dari `ORGANIZATION_UNIT_CONFIG`,
   sama seperti `use-master-data-organization-unit.jsx:362-378`, lalu buka
   `/hr/master-data/organization-unit/{token}`. Ganti label tombol menjadi "Buka Detail Unit".
4. Tampilkan tipe unit di panel memakai label dari `ORGANIZATION_UNIT_TYPE_OPTIONS`, misalnya
   "Instalasi" untuk `Installation`.
5. Ganti teks: "Refresh" → "Muat Ulang", "Parent Unit" → "Unit Induk", "Root (Unit Teratas)" →
   "Unit Teratas", "Pohon Hierarki Unit Organisasi" → "Pohon Unit Organisasi".
6. Ganti teks konfigurasi halaman: judul "Struktur Organisasi", deskripsi seperti 3.2, kartu
   "Root Unit" → "Unit Teratas", kolom "Parent Unit" → "Unit Induk", kolom "Child" → "Sub-Unit".

**Acceptance criteria.**

1. Membuka `/hr/master-data/organization-unit` menampilkan judul "Struktur Organisasi", tombol
   `Daftar` dan `Hierarki`, serta daftar unit dengan tombol "+ Tambah Unit Organisasi" sebagai
   tampilan pertama.
2. Di mode Hierarki, memilih "Instalasi Farmasi" lalu menekan "Buka Detail Unit" langsung
   membuka halaman detail Instalasi Farmasi. Alamatnya `/hr/master-data/organization-unit/<token>`.
   Daftar tidak muncul lebih dulu.
3. Panel pohon menampilkan "Tipe Unit: Instalasi", bukan "Installation".
4. Petugas menambah unit "Instalasi Gizi" di mode Daftar, lalu menekan `Hierarki`. Instalasi Gizi
   muncul di bawah induknya tanpa muat ulang halaman, karena pohon diambil ulang saat mode dibuka.
5. Peran yang punya `OrganizationUnit : Read` tetapi tidak punya `Organization : Read` dapat
   memakai kedua mode tanpa galat 403.
6. Tidak ada lagi teks "Refresh", "Parent Unit", "Child", atau "Root Unit" di halaman ini.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`. Setelah itu uji
manual AC 1–4 di peramban, desktop dan ponsel.

**Risiko.** Token rute dari pohon tidak dikenali halaman detail bila `scope`/`type` berbeda,
sehingga detail tampil kosong. Deteksinya lewat AC 2. Pencegahannya: ambil kedua nilai dari
`ORGANIZATION_UNIT_CONFIG`, jangan diketik ulang.

---

### FIX-HRD-008-02 — Halaman Departemen dan halaman Posisi berdiri sendiri

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-02`, `ISS-HRD-008-T4` (teks halaman organisasi) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | **Baru** `src/app/hr/master-data/organization/[type]/page.jsx`: hanya menerima `departments` dan `positions`, jenis lain → `notFound()`, judul tab peramban per jenis · `src/components/view/hr/master-data/organization/master-data-organization-view.jsx`: terima jenis tetap dari halaman; buang tab, tombol `Daftar \| Hierarki`, dan impor `OrganizationHierarchyView`; judul per jenis · `src/lib/hooks/hr/master-data/organization/use-master-data-organization.jsx`: jenis aktif dari parameter route, menggantikan `?type=` di baris 315-317 · `src/lib/constants/hr/master-data/organization/organization-constants.jsx`: "Daftar Departemen" dan "Daftar Posisi" · `src/lib/hooks/hr/master-data/organization/use-master-data-organization-editor.jsx:303` dan `use-master-data-organization-detail.jsx:158`: alamat kembali `/hr/master-data/organization/{type}` · `src/app/hr/master-data/organization/[type]/create/page.jsx:4`, `[type]/[slug]/page.jsx`, `[type]/[slug]/update/page.jsx`: judul tab peramban per jenis |
| **Radius dampak** | Layar Departemen dan Posisi: daftar, tambah, detail, ubah. Endpoint `.../organization/departments/options` dan `.../positions/options` yang dipakai editor Karyawan, Dokter, Pengguna Eksternal, dan Pengadaan **tidak berubah**. Tombol "Buka Uraian" (`organization-table-columns.jsx:202-217`) tetap |
| **Bergantung pada** | `FIX-HRD-008-01`, `FIX-HRD-008-03` (pohon harus sudah tampil di halaman Struktur Organisasi sebelum dicabut dari sini) |

**Langkah.**

1. Buat `[type]/page.jsx`. Halaman ini memvalidasi `type` terhadap `ORGANIZATION_TABS`, lalu
   merender view organisasi dengan jenis tetap.
2. Ubah hook daftar agar memakai jenis dari halaman. `changeType` (baris 659) dan pembacaan
   `?type=` tidak dipakai lagi oleh halaman ini.
3. Ubah view: tab, tombol mode, dan impor pohon dibuang. Judul `Hero` menjadi "Departemen" atau
   "Posisi". Ganti teks "Organization" pada pesan memuat dan pesan kosong (baris 71-75, 366,
   483-484).
4. Ubah alamat kembali di hook editor dan hook detail ke `/hr/master-data/organization/{type}`.
5. Ganti judul tab peramban "Tambah Organization" dan yang serupa menjadi "Tambah Departemen",
   "Tambah Posisi", dan seterusnya.

**Acceptance criteria.**

1. `/hr/master-data/organization/departments` menampilkan judul "Departemen", tombol
   "+ Tambah Departemen", tanpa tab Posisi, dan tanpa tombol Daftar/Hierarki.
2. `/hr/master-data/organization/positions` menampilkan judul "Posisi", penyaring "Semua
   Departemen", tombol "+ Tambah Posisi", dan tombol "Buka Uraian" di setiap baris.
3. Petugas menambah departemen "Gizi Klinik", menyimpan, lalu menekan "Kembali". Petugas kembali
   ke `/hr/master-data/organization/departments`, dan "Gizi Klinik" ada di daftar.
4. Petugas mengubah posisi "Kepala Instalasi Farmasi", lalu menekan "Kembali" dari detail.
   Petugas kembali ke `/hr/master-data/organization/positions`.
5. `/hr/master-data/organization/abc` menampilkan halaman tidak ditemukan.
6. Kata "Organization" tidak muncul di judul halaman, judul tab peramban, pesan memuat, atau
   pesan kosong.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`. Setelah itu uji
manual AC 1–5.

**Risiko.** Ada alamat kembali yang terlewat dan masih menunjuk `?type=`. Setelah
`FIX-HRD-008-04`, alamat itu tetap sampai ke tujuan lewat pengalihan, tetapi petugas sempat
melewati dua lompatan. Deteksinya lewat AC 3–4: perhatikan alamat akhirnya.

---

### FIX-HRD-008-04 — Sidebar baru, pengalihan alamat lama, tautan Penempatan Awal, dan uji sidebar

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-01`, `ISS-HRD-008-02` (sisi navigasi) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend + uji unit |
| **Berkas yang diubah** | `src/utils/menu-sidebar/menu-items.jsx:229-299`: susunan seperti 3.1, ikon memakai yang sudah diimpor · `src/app/hr/master-data/organization/page.jsx`: menjadi halaman pengalih dengan pola `src/app/finance/master-data/petty-cash-budget/page.jsx`; `organization-client.jsx` dihapus karena tidak dipakai lagi · `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx:44-45`: tujuan `MASTER_POSITION` dan `MASTER_DEPARTMENT` · `tests/unit/hr-master-data-sidebar-grouping.test.mjs:40-69`: jumlah butir, `key`, dan `pathname` |
| **Radius dampak** | Seluruh pengguna sidebar SDM. Tautan perbaikan data di layar Penempatan Awal Pegawai (Menu 07) |
| **Bergantung pada** | `FIX-HRD-008-02`, `FIX-HRD-008-03` |

**Langkah.**

1. Susun ulang grup `corporateHrMasterGroupOrganization` seperti tabel 3.1: label
   `corporateOrganizationUnit` menjadi "Struktur Organisasi" dan diletakkan pertama; tambah
   `corporateDepartment` dan `corporatePosition`; buang `corporateOrganization`.
2. Ganti isi `src/app/hr/master-data/organization/page.jsx` dengan halaman pengalih. Halaman ini
   membaca `searchParams.type`:

   | Alamat lama | Dialihkan ke |
   | --- | --- |
   | `/hr/master-data/organization?type=positions` | `/hr/master-data/organization/positions` |
   | `/hr/master-data/organization?type=departments` | `/hr/master-data/organization/departments` |
   | `/hr/master-data/organization` atau `type` lain | `/hr/master-data/organization-unit` |

3. Ubah `SAFE_DESTINATIONS.MASTER_POSITION` menjadi `/hr/master-data/organization/positions` dan
   `MASTER_DEPARTMENT` menjadi `/hr/master-data/organization/departments`.
   `MASTER_ORGANIZATION_UNIT` tetap.
4. Perbarui uji sidebar: `expectedItemCount: 12`, daftar `key` dan `pathname` baru. Tambah uji
   `findFullPathByPathname` untuk dua alamat:
   `/hr/master-data/organization/departments/create` → butir Departemen, dan
   `/hr/master-data/organization-unit/create` → butir Struktur Organisasi.

**Acceptance criteria.**

1. Grup "Organisasi & Struktur Jabatan" berisi 12 butir dengan urutan Struktur Organisasi,
   Departemen, Posisi, Legal Entity, Fasilitas Rumah Sakit, Lokasi Kerja, Pusat Biaya, Rumpun
   Jabatan, Level Jabatan, Grade Karyawan, Uraian Tugas, Atasan Langsung. Butir "Unit Organisasi"
   tidak ada.
2. Saat petugas berada di halaman tambah, detail, atau ubah unit, butir yang menyala di sidebar
   adalah "Struktur Organisasi".
3. Saat petugas berada di `/hr/master-data/organization/departments/create`, butir yang menyala
   adalah "Departemen", bukan "Struktur Organisasi".
4. Bookmark lama berpindah dengan benar: `/hr/master-data/organization` → Struktur Organisasi;
   `?type=positions` → Posisi; `?type=departments` → Departemen.
5. Di Penempatan Awal Pegawai, baris dengan alasan `POSITION_NOT_ASSIGNED` membawa petugas ke
   halaman Posisi. Baris dengan alasan `ORGANIZATION_UNIT_DEPARTMENT_MISMATCH` membawa petugas ke
   Struktur Organisasi.
6. `npm run test:unit` lulus, termasuk uji sidebar yang diperbarui.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`. Setelah itu uji
manual AC 1–5.

**Risiko.** Peran yang hanya punya `Organization : Read` dulu bisa membuka "Struktur Organisasi"
untuk melihat Departemen dan Posisi. Setelah perubahan, menu dengan nama yang sama memakai
`OrganizationUnit : Read`, sehingga peran itu melihat halaman "akses ditolak". Departemen dan
Posisi tetap terbuka bagi peran itu. Deteksinya: sebelum rilis, minta admin memeriksa peran mana
yang punya `Organization` tetapi tidak punya `OrganizationUnit` (lihat K-01).

---

### FIX-HRD-008-05 — Form unit: level otomatis dan petunjuk memilih departemen

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-02` (kalimat 3: departemen dipilih dari master), `ISS-HRD-008-T2`, `ISS-HRD-008-T4` (teks form) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx:41-142` dan `497-505` |
| **Radius dampak** | Form tambah dan ubah unit saja. Konstanta ini juga dipakai daftar dan detail unit, tetapi yang diubah hanya `ORGANIZATION_UNIT_CREATE_FIELDS` dan `transformPayload` |
| **Bergantung pada** | `FIX-HRD-008-03`. Berkasnya sama, jadi dikerjakan berurutan untuk menghindari benturan |

**Langkah.**

1. Buang field `levelNumber` dari `ORGANIZATION_UNIT_CREATE_FIELDS` (baris 99-109).
2. Pastikan payload tidak membawa `levelNumber`. Periksa cara editor menyusun payload dan
   `transformPayload` (baris 497-505).
3. Ubah field `parentOrganizationUnitId`: label "Unit Induk", pilihan kosong "Tidak memiliki unit
   induk", keterangan "Level unit ditentukan otomatis dari unit induk."
4. Ubah field `departmentId`: label "Departemen Terkait", keterangan "Pilih dari master
   Departemen. Departemen baru ditambahkan lewat menu Departemen."
5. Sesuaikan `helperText` editor (baris 340-341) dengan istilah "unit induk".

**Acceptance criteria.**

1. Form tambah dan ubah unit tidak lagi memuat isian "Level Hierarki".
2. Petugas membuat "Instalasi Farmasi" dengan unit induk "Direktorat Penunjang Medis" (level 2).
   Unit tersimpan dan detailnya menampilkan "Level Hierarki: 3".
3. Petugas mengubah unit induk "Instalasi Farmasi" menjadi "Direktorat Utama" (level 1). Setelah
   disimpan, level menjadi 2.
4. Isian "Departemen Terkait" menampilkan petunjuk menu Departemen, dan pilihannya hanya
   departemen aktif dari master.
5. Tidak ada teks "Parent" di form.

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`. Setelah itu uji
manual AC 2–3. Periksa juga payload di tab Network peramban: `levelNumber` tidak dikirim.

**Risiko.** Bila editor tetap mengirim `levelNumber: ""` atau `0`, server menolak lewat
`[Range(1, int.MaxValue)]` dengan kode 400. Deteksinya lewat AC 2.

---

### FIX-HRD-008-06 — Respons detail unit membawa nama legal entity, fasilitas, dan departemen

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-T3` (sisi server) |
| **Area** | Backend |
| **Jenis perubahan** | Source backend, kontrak respons ditambah tanpa mengubah field lama. Tanpa perubahan tabel, tanpa migration |
| **Berkas yang diubah** | `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/OrganizationUnitDtos.cs:40-45` (`OrganizationUnitDetailResponse`) · `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs:353-401` (`GetById`) |
| **Radius dampak** | Hanya respons `GET {id}`. Daftar, opsi, dan pohon tidak berubah. Pemakai respons detail yang lain tidak terganggu karena field hanya ditambah |
| **Bergantung pada** | Tidak ada |

Endpoint yang berubah:

#### Corporate / Human Resource / Master Data / Organization Unit

Base URL: `api/v1/corporate/human-resource/master-data/organization-units`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}` | Mengambil detail satu unit, kini beserta nama legal entity, fasilitas, dan departemen terkait | `OrganizationUnit : Read` | - | `OrganizationUnitDetailResponse` + `LegalEntityName`, `HospitalSiteName`, `DepartmentName` (baru, boleh kosong) |

Kode status tidak berubah. 200 berarti data ditemukan. 404 berarti unit tidak ada atau sudah
dihapus. 403 berarti pengguna tidak punya hak akses melihat unit.

**Langkah.**

1. Tambahkan tiga properti `string?` ke `OrganizationUnitDetailResponse`.
2. Isi ketiganya di `GetById` dari relasi `LegalEntity`, `HospitalSite`, dan `Department`, apa pun
   status aktifnya.

**Acceptance criteria.**

1. `GET .../organization-units/{id}` untuk "Instalasi Farmasi" yang terhubung ke departemen
   "Farmasi" mengembalikan `departmentName: "Farmasi"`.
2. Unit tanpa fasilitas mengembalikan `hospitalSiteName: null` tanpa galat.
3. Unit yang terhubung ke departemen nonaktif tetap mengembalikan nama departemennya.
4. Field lama dalam respons tetap ada dengan nama dan tipe yang sama.

**Verifikasi.** Sesuai `build-module-backend`: `dotnet build` dan pemeriksaan QBE yang diwajibkan.
Bila folder `bin` terkunci oleh backend yang sedang berjalan, arahkan `OutDir` ke folder
sementara. Uji AC 1–3 lewat Swagger lokal hanya bila pemilik mengizinkan backend dijalankan.

**Risiko.** Kueri detail memuat tiga relasi tambahan. Untuk satu baris, dampaknya kecil.

---

### FIX-HRD-008-07 — Detail unit menampilkan nama, bukan ID

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-T3` (sisi layar) |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx:348-475` (`detailFields`) |
| **Radius dampak** | Halaman detail unit saja |
| **Bergantung pada** | `FIX-HRD-008-06`. Sebaiknya juga setelah `FIX-HRD-008-05` karena berkasnya sama |

**Langkah.**

1. Ganti baris "ID Legal Entity", "ID Fasilitas Rumah Sakit", dan "ID Departemen" menjadi "Legal
   Entity", "Fasilitas Rumah Sakit", dan "Departemen Terkait". Isinya dibaca dari
   `legalEntityName`, `hospitalSiteName`, dan `departmentName`.
2. Buang baris "ID Parent Unit". Ubah label "Parent Unit" menjadi "Unit Induk" dan "Jumlah Child
   Unit" menjadi "Jumlah Sub-Unit".

**Acceptance criteria.**

1. Detail "Instalasi Farmasi" menampilkan "Departemen Terkait: Farmasi". Tidak ada deretan huruf
   dan angka acak (UUID) di mana pun di halaman.
2. Unit tanpa fasilitas menampilkan "Fasilitas Rumah Sakit: -".

**Verifikasi.** `npm run lint:errors`, `npm run test:unit`, `npm run build`. Setelah itu uji
manual AC 1–2.

**Risiko.** Bila `FIX-HRD-008-06` belum terpasang di backend yang dipakai, ketiga baris
menampilkan "-". Deteksinya lewat AC 1. Jangan rilis FIX ini lebih dulu dari pasangan
backend-nya.

---

### FIX-HRD-008-08 — Server menolak Departemen Terkait yang tidak ada atau nonaktif

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-008-T5` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend, validasi tambahan. Tanpa perubahan tabel, tanpa migration |
| **Berkas yang diubah** | `Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs:715-804` (`ValidateRequestAsync`) |
| **Radius dampak** | `POST /` dan `PUT /{id}` unit organisasi. Data yang sudah tersimpan tidak disentuh |
| **Bergantung pada** | Tidak ada |

Endpoint yang terdampak:

#### Corporate / Human Resource / Master Data / Organization Unit

Base URL: `api/v1/corporate/human-resource/master-data/organization-units`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat unit baru | `OrganizationUnit : Create` | `CreateOrganizationUnitRequest` | `object` (`Id`, `UnitCode`, `UnitName`) |
| `PUT` | `/{id}` | Mengubah unit | `OrganizationUnit : Update` | `UpdateOrganizationUnitRequest` | sama seperti sekarang |

Kode status baru yang mungkin muncul: **400** dengan pesan "Departemen tidak ditemukan atau tidak
aktif." Artinya petugas memilih departemen yang sudah dinonaktifkan atau dihapus, sehingga harus
memilih departemen lain atau mengaktifkannya dulu di menu Departemen.

**Langkah.**

1. Bila `DepartmentId` terisi dan bukan `Guid.Empty`, periksa `MstDepartment` dengan syarat ada,
   aktif, dan tidak dihapus. Ikuti pola pemeriksaan legal entity di baris 749-756.
2. Bila tidak lolos, kembalikan pesan di atas.

**Acceptance criteria.**

1. Petugas menyimpan unit dengan departemen aktif "Farmasi". Berhasil, sama seperti sekarang.
2. Departemen "Gizi Lama" dinonaktifkan, lalu dikirim lewat API saat membuat unit. Server
   menjawab 400 dengan pesan "Departemen tidak ditemukan atau tidak aktif."
3. `departmentId` acak yang tidak ada di database dijawab 400 dengan pesan yang sama, bukan 500.
4. Unit tanpa departemen (`departmentId: null`) tetap bisa disimpan.

**Verifikasi.** Sesuai `build-module-backend`: `dotnet build` dan pemeriksaan QBE yang diwajibkan.
AC 2–3 diuji lewat Swagger lokal hanya bila pemilik mengizinkan backend dijalankan.

**Risiko.** Unit lama yang sudah terhubung ke departemen nonaktif tidak bisa disimpan ulang
sebelum departemennya diganti. Perilaku ini memang disengaja, dan pesan 400 menjelaskan
penyebabnya.

---

## 5. Urutan Pengerjaan

```text
FIX-HRD-008-01 ✅            dokumen desain + keputusan — dikerjakan lebih dulu
└── FIX-HRD-008-03 ✅        halaman Struktur Organisasi (unit + pohon)
    ├── FIX-HRD-008-02 ✅    halaman Departemen dan Posisi (mencabut pohon dari halaman lama)
    │   └── FIX-HRD-008-04 ✅    sidebar, pengalihan alamat lama, tautan Penempatan Awal, uji sidebar
    └── FIX-HRD-008-05 ✅    form unit: level otomatis, petunjuk departemen
FIX-HRD-008-06 ✅ [BE]       nama pada detail unit — tanpa dependency, boleh paralel
└── FIX-HRD-008-07 ✅        detail unit menampilkan nama (setelah FIX-05 karena berkas sama)
FIX-HRD-008-08 ✅ [BE]       validasi Departemen Terkait — tanpa dependency, boleh paralel
```

`FIX-HRD-008-02`, `03`, dan `04` sebaiknya dirilis bersamaan. Bila dirilis terpisah, ada jeda di
mana dua menu menampilkan pohon yang sama (setelah 03) atau Departemen dan Posisi belum punya
butir sidebar (setelah 02).

---

## 6. Keputusan Pemilik yang Dibutuhkan

**Sudah diambil:**

| No | Keputusan | Diambil oleh | Tanggal |
| ---: | --- | --- | --- |
| KD-01 | Opsi A: Departemen dan Posisi masing-masing mendapat menu sendiri. Struktur Organisasi berisi pohon dan pengelolaan unit. Menu Unit Organisasi dihapus dari sidebar. | Pemilik, atas masukan analis | 2026-11-24 |

**Masih dibutuhkan:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 *(diputuskan 24-11-2026: (a), `HRD-DEC-249`)* | Label hak akses di layar Akses Role ("Organization" untuk Departemen dan Posisi, "Organization Unit" untuk Struktur Organisasi) perlu diganti? | (a) Biarkan dulu; (b) Ganti label tampilan di atribut `[AccessController]` backend | (a). Beri admin catatan pemetaan di panduan pengguna (bagian 7). Penggantian label diperiksa dalam task tersendiri karena menyentuh sinkronisasi daftar hak akses | Tidak ada. Risiko di `FIX-HRD-008-04` tetap perlu diperiksa admin |
| K-02 *(diputuskan 24-11-2026: (a), `HRD-DEC-250`)* | Tipe unit "Departemen" di Struktur Organisasi tetap dipakai? | (a) Tetap; (b) Hapus dari pilihan | (a). Kebingungan diatasi petunjuk di `FIX-HRD-008-05` | Tidak ada |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Revisi dikerjakan lewat `FIX-HRD-008-01` di jalur blueprint yang berwenang, bukan oleh
`diagnose-module-issue`.

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `docs/update/struktur-organisasi/INSTRUKSI_AGENT_HRD_MENU_05_STRUKTUR_ORGANISASI_DAN_JABATAN.md` | Kepala dokumen | — | Catatan amandemen: "Diubah oleh `ISSUE-HRD-008` / `HRD-DEC-<baru>` (24-11-2026): Unit Kerja digabung ke Struktur Organisasi; Departemen dan Posisi menjadi menu sendiri." |
| Dokumen yang sama | Baris 5 (cakupan submenu) | "6 Submenu target: Unit Kerja, Struktur Organisasi, Jabatan, …" | "Unit Kerja digabung ke Struktur Organisasi. Departemen dan Jabatan (Posisi) masing-masing punya menu sendiri." |
| Dokumen yang sama | Baris 80 (Unit Kerja, frontend) | "pertahankan route `/hr/master-data/organization-unit`" | "route `/hr/master-data/organization-unit` menjadi halaman menu Struktur Organisasi: daftar unit + mode Hierarki" |
| Dokumen yang sama | Baris 86–88 (Struktur Organisasi) | "`/hr/master-data/organization` … tab Department dan Position … tidak boleh dihapus"; "Mode Daftar mempertahankan tab Department/Position existing" | "Struktur Organisasi = daftar unit + mode Hierarki pada `/hr/master-data/organization-unit`. Department dan Position pindah ke `/hr/master-data/organization/departments` dan `/positions`." |
| Dokumen yang sama | Baris 91 (keamanan pohon) | "mode hierarki adalah read-only kecuali pengguna masuk ke capability Unit Kerja" | "pohon tetap hanya untuk dilihat; tambah dan ubah unit lewat mode Daftar di halaman yang sama" |
| Dokumen yang sama | Baris 102 (Jabatan) | "pertahankan tab Position pada `/hr/master-data/organization`" | "Position punya halaman sendiri di `/hr/master-data/organization/positions`" |
| Dokumen yang sama | Baris 213–216 (peta layar) | Unit Kerja dan Struktur Organisasi dua route | Satu baris Struktur Organisasi (`/organization-unit`) + baris Departemen dan Posisi |
| Dokumen yang sama | Baris 272–306 (wireframe Struktur Organisasi) | Wireframe "Manajemen Organization [Daftar] [Hierarki]" dengan tab Department/Position | Rujuk skema 3.2–3.4 plan repair ini |
| `03-frontend-architecture.md` | Bagian 34.2, baris "Struktur Organisasi & Jabatan" (baris 3920) | "Struktur, unit, jabatan, rumpun, uraian tugas, dan relasi atasan tidak digandakan ke parent baru" · `UNCHANGED / REUSED` | Ditambah: "Unit Organisasi digabung ke Struktur Organisasi; Departemen dan Posisi menu sendiri (`ISSUE-HRD-008`)" · `AMENDED` |
| `00-interview-decisions.md` | Keputusan baru | — | `HRD-DEC-<baru>` = KD-01, beserta alasan dan rujukan issue |
| `roadmap/frontend-roadmap.md` | Bagian 29.3, `FE-HRD-361` dan `FE-HRD-362` | Target layar `/hr/master-data/organization-unit` dan mode Hierarki di `/hr/master-data/organization` | Catatan: "target layar diubah oleh `ISSUE-HRD-008`", ditambah task baru dari register ini |
| `docs/UserGuide/01-master-data-hr/PANDUAN_PENGGUNA.md` | Baris 10, 27, 37 | "Struktur Organisasi (`/hr/master-data/organization`)" | Alamat dan nama menu baru, ditambah catatan pemetaan hak akses dari K-01 |

---

## 8. Verifikasi Menyeluruh

Jalankan setelah seluruh perbaikan yang disetujui berstatus `✅`. Pakai akun HR yang punya
`Organization` dan `OrganizationUnit` lengkap, serta satu akun yang hanya punya
`Organization : Read`.

1. **Sidebar.** Buka SDM > Master Data > Organisasi & Struktur Jabatan. Pastikan ada Struktur
   Organisasi, Departemen, dan Posisi, dan tidak ada Unit Organisasi.
2. **Membuat departemen dari masternya.** Buka Departemen, tambah "Gizi Klinik", simpan, kembali.
   "Gizi Klinik" ada di daftar dan butir Departemen menyala di sidebar.
3. **Membuat posisi dari masternya.** Buka Posisi, tambah "Kepala Instalasi Gizi" pada departemen
   "Gizi Klinik", lalu tekan "Buka Uraian" pada barisnya. Layar Uraian Tugas posisi itu terbuka.
4. **Menyusun struktur.** Buka Struktur Organisasi, tambah "Instalasi Gizi" dengan unit induk
   "Direktorat Penunjang Medis" dan Departemen Terkait "Gizi Klinik". Tidak ada isian level.
   Detail menampilkan Level 3, nama departemen, nama legal entity, dan nama fasilitas, tanpa
   UUID.
5. **Pohon.** Tekan `Hierarki`. "Instalasi Gizi" ada di bawah induknya dengan tipe "Instalasi".
   Tekan "Buka Detail Unit". Detail Instalasi Gizi langsung terbuka.
6. **Alamat lama.** Buka bookmark `/hr/master-data/organization`,
   `/hr/master-data/organization?type=positions`, dan `?type=departments`. Ketiganya sampai ke
   halaman yang benar.
7. **Penempatan Awal.** Di Penempatan Awal Pegawai, tekan tautan perbaikan pada baris
   `POSITION_NOT_ASSIGNED` dan `ORGANIZATION_UNIT_DEPARTMENT_MISMATCH`. Masing-masing sampai ke
   Posisi dan Struktur Organisasi.
8. **Hak akses.** Masuk dengan akun yang hanya punya `Organization : Read`. Departemen dan Posisi
   terbuka. Struktur Organisasi menampilkan halaman "akses ditolak" yang rapi, bukan galat
   setengah halaman.
9. **Validasi server.** Nonaktifkan "Gizi Klinik", lalu ubah "Instalasi Gizi" tanpa mengganti
   departemen. Server menolak dengan pesan "Departemen tidak ditemukan atau tidak aktif."
10. **Ukuran layar.** Ulangi langkah 4–5 pada lebar ponsel. Pohon dan panel detail tetap terbaca.

Tangkapan layar uji tidak boleh memuat NIK, nama pegawai asli, atau nominal gaji.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-24 | Rencana dibuat: 8 perbaikan (1 dokumen, 5 frontend, 2 backend), arah Opsi A dari pemilik, menunggu persetujuan per perbaikan | `diagnose-module-issue` |
| 2026-11-24 | Pemilik menyetujui seluruh rencana ("oke saya setujui kerjakan semua dan selesaikan"), termasuk rekomendasi K-01 (a) dan K-02 (a). `status_rencana` → `DISETUJUI`. Keputusan dicatat `HRD-DEC-248` s.d. `HRD-DEC-250`; task didaftarkan: `FE-HRD-410` s.d. `FE-HRD-414` (roadmap frontend bagian 32), `BE-HRD-463`, `BE-HRD-464` (roadmap backend bagian 32) | `manage-module-blueprint` |
| 2026-11-24 | Seluruh 8 perbaikan dikerjakan: `FIX-HRD-008-01` (dokumen) dan tujuh task `BE-HRD-463`, `BE-HRD-464`, `FE-HRD-410` s.d. `FE-HRD-414` berstatus ✅ dengan laporan tracked. `status_rencana` → `SELESAI`. Satu tambahan kecil di luar daftar teks issue T4: label periode bawaan "Period" pada halaman Departemen/Posisi diganti "Periode" (`FE-HRD-411`). Uji runtime dengan backend dan akun nyata belum dijalankan | `build-module-backend` / pengerjaan frontend langsung |
