# PLAN-REPAIR-002 — Bangun ulang tampilan enam layar Administrasi Kepegawaian dengan base component dan CSS Module berbasis token, lalu perkuat skill frontend agar pola ini tertangkap

```yaml
plan_id: PLAN-REPAIR-HRD-002
issue: ../issue/issue-002-tampilan-workforce-core-tanpa-gaya.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-11-03"
diputuskan_oleh: "User (pemilik) — instruksi 03-11-2026: perbaiki seluruh tampilan sesuai standar UI dan base component, perkuat skill build-module-frontend"
tanggal_keputusan: "2026-11-03"
basis_source_backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (MHamzah)"
basis_source_frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (HamzahV2)"
```

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-HRD-002-01` | `ISS-HRD-002-01`, `-06` | Satu CSS Module domain berbasis token, utilitas tampilan bersama, dan shell halaman standar | FE | 1 | `FE-HRD-022` s.d. `FE-HRD-027` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-022.md`, `../../task/report/frontend/FE-HRD-024.md` |
| `FIX-HRD-002-02` | `-01`, `-02`, `-03`, `-06`, `-T1`, `-T2` | Daftar SK: saringan tampil, ringkasan dari `/summary`, lencana dan tombol benar, kolom di berkas terpisah | FE | 1 | `FE-HRD-022` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-022.md` |
| `FIX-HRD-002-03` | `-01`, `-03`, `-04`, `-06` | Buat SK: field standar, pilihan bercari pengganti UUID, validasi layar mencerminkan server | FE | 1 | `FE-HRD-024` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-024.md` (cakupan pilihan pegawai: lihat `FIX-HRD-002-11`) |
| `FIX-HRD-002-04` | `-01`, `-02`, `-03`, `-05`, `-06`, `-07`, `-T3` | Detail SK: kartu seksi, `ConfirmModal` untuk Tolak/Batal (kirim `reason`), lampiran dapat diunduh dan diganti selama Draf | FE | 1 | `FE-HRD-024` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-024.md` |
| `FIX-HRD-002-05` | `-01`, `-02`, `-03`, `-04`, `-06`, `-T4` | Employee 360: kartu profil, tab standar, `DataTable` riwayat SK, tanpa UUID; alamat API dan test diperbaiki | FE | 1 | `FE-HRD-025` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-025.md` |
| `FIX-HRD-002-06` | `-01`, `-02`, `-03`, `-05`, `-06` | Batch: tampilan standar (`FilterSelect`, `SummaryGrid`, `DataTable`, `ConfirmModal`) dan field DTO yang benar, tanpa mengubah alur | FE | 2 | `FE-HRD-026` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-026.md` (tampilan; task tetap 🟡 karena `FIX-HRD-002-09`) |
| `FIX-HRD-002-07` | `-01`, `-03`, `-04`, `-06` | Pengajuan Atas Nama: tampilan standar dan pilihan pegawai bercari, tanpa mengubah kontrak | FE | 2 | `FE-HRD-027` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-027.md` (tampilan; task tetap 🟡 karena `FIX-HRD-002-09`) |
| `FIX-HRD-002-08` | `-08` | Tambahkan menu Dokumen SK Kepegawaian dan Batch Data Awal ke sidebar | FE | 2 | `FE-HRD-022` | ✅ SELESAI | `../../task/report/frontend/FE-HRD-022.md` |
| `FIX-HRD-002-09` | `-T5` | Sambungkan alur Batch (template → validasi → buat → eksekusi) dan Atas Nama (multipart) ke kontrak backend | FE (+ BE) | 3 | — | ⛔ TERBLOKIR — menunggu K-02 dan perbaikan backend LBT-WFC-09, LBT-WFC-10, KI-05 | — |
| `FIX-HRD-002-10` | Pola §8 issue | Perkuat skill `build-module-frontend` dan aturan `rules/frontend/` (larangan Tailwind, grep baru, pemeriksa kunci CSS Module, bukti visual wajib) | Skill | 1 | Dokumen (repo `QuilvianEngineeringSkills`) | 🟡 SEBAGIAN — 4 dari 4 kriteria terpenuhi (`sync-rules.ps1 -Check` lulus, versi 1.21.0) dan di-commit pemilik sebagai `db27872`; belum di-push, sehingga plugin terpasang belum memakainya | `QuilvianEngineeringSkills/Claude/INSTALASI.md` riwayat versi 1.21.0 |
| `FIX-HRD-002-11` | `ISS-HRD-002-T7` | Endpoint opsi profil tenaga kerja (karyawan + dokter, bernilai `workforceProfileId`) dan pengalihan resource `workforceProfiles` ke endpoint itu | BE + FE | 2 | — | BELUM DIKERJAKAN — menunggu K-03 | — |

**Ringkasan: 8 dari 9 perbaikan yang disetujui selesai; `FIX-HRD-002-10` sebagian (sudah di-commit `db27872`, menunggu push dan pembaruan plugin); `FIX-HRD-002-09` terblokir; `FIX-HRD-002-11` menunggu persetujuan (K-03).**

## 2. Solusi terpilih per temuan

### ISS-HRD-002-01 — Lima layar ditulis dengan class Tailwind

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pasang Tailwind | Source tidak perlu diubah | Bahasa desain kedua di aplikasi, preflight Tailwind mengubah gaya global, dilarang `AGENTS.md` ("penggantian design system") |
| B | Bangun ulang dengan base component + satu CSS Module domain berbasis token | Konsisten dengan layar lain, mengikuti aturan, radius dampak hanya enam view | Pekerjaan lebih banyak |
| C | Ganti class Tailwind dengan utilitas Bootstrap satu-per-satu | Cepat | Tetap tidak memakai base component; kartu, form, dan dialog tetap buatan sendiri |

**Solusi terpilih: Opsi B.**

1. `rules/frontend/frontend-architecture.md` mewajibkan CSS Module untuk style baru, dan
   `design-tokens.md` mewajibkan token. Opsi B memenuhi keduanya.
2. Layar Exit Clearance (`exit-clearance-detail-view.jsx` + `exit-clearance.module.css`) sudah
   memecahkan bentuk layar yang sama (kartu seksi, grid informasi, aksi persetujuan) dengan token.
   Itu menjadi rujukan visual.

**Kenapa bukan opsi lain.** A melanggar larangan penggantian design system. C hanya mengganti
nama class tanpa menutup akar masalah, yaitu base component yang tidak dipakai.

### ISS-HRD-002-02 dan ISS-HRD-002-03 — Lencana dan varian tombol

Hanya ada satu cara wajar: pakai nilai prop yang sah. Status backend dipetakan sekali di utilitas
bersama ke kunci `StatusBadge`, dan tombol sekunder memakai `variant="secondary"`.

### ISS-HRD-002-04 — UUID diketik

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | `ResourceFilterSelect` + `useSelectResource` | Pola yang sudah dipakai Jurnal Akuntansi dan Hemodialisa; pencarian di server | — |
| B | Muat semua opsi lalu `FilterSelect` biasa | Sederhana | Tidak berskala untuk ratusan pegawai |

**Solusi terpilih: Opsi A.** Resource `organizationUnits`, `departments`, dan `positions` dipakai
apa adanya. Pegawai dan atasan memakai konfigurasi lepas bernilai `workforceProfileId` di atas
endpoint opsi karyawan, sama seperti `NURSE_SELECT_RESOURCE` Hemodialisa. Tidak ada endpoint baru.

### ISS-HRD-002-05 — Dialog buatan sendiri

**Solusi terpilih: `ConfirmModal`.** Tolak dan Batal memakai `requireReason` beserta
`maxReasonLength` 500 (batas kolom `RejectionReason` dan `CancellationReason`). Buat Batch memakai
`ConfirmModal` dengan `children` berisi isian nama batch.

### ISS-HRD-002-06 — Pesan galat teknis

**Solusi terpilih:** satu fungsi `getWorkforceCoreErrorMessage` di
`src/utils/hr/workforce-core/workforce-core-display-utils.js`. Urutannya: pesan backend, lalu
peta kode HTTP ke kalimat Bahasa Indonesia, lalu fallback per aksi. `err.message` dari Axios tidak
pernah ditampilkan.

### ISS-HRD-002-07 — Scan tidak dapat diganti dan diunduh

**Solusi terpilih:** kartu Berkas menampilkan daftar lampiran beserta tombol **Unduh** (endpoint yang
sudah ada) dan formulir **Ganti Berkas** selama status Draf. Backend sudah memakai unggahan terakhir
sebagai dokumen bertanda tangan, sehingga backend tidak perlu diubah.

### ISS-HRD-002-08 — Menu sidebar

**Solusi terpilih:** tambahkan dua menu sesuai tabel menu `03-frontend-architecture.md`. Employee 360
dan Pengajuan Atas Nama tetap tanpa menu, sesuai desain.

## 3. Skema tampilan sebelum → sesudah

### 3.1 Buat SK (`/hr/workforce-core/personnel-actions/create`)

**SEBELUM** (lampiran 3)

```text
Buat SK Kepegawaian Baru (hero)
ⓘ Penerbitan SK Kepegawaian adalah satu-satunya jalur resmi ...
ID Pegawai (Workforce Profile ID) *[ 11111111-1111-...   ]
Masukkan UUID profil staf yang akan diterbitkan SK-nya.
Kategori Tindakan Kepegawaian *[ Mutasi ▾ ]
Tanggal Efektif SK *[ 03/11/2026 ]
Usulan Gaji Pokok Baru (IDR) — Opsional[      ]
Target Unit Organisasi (ID) — Opsional[ UUID Unit Organisasi ]
... (label dan isian menempel, tanpa kartu)
[ Batal ][ Simpan Draf SK ]      ← keduanya berwarna sama
```

**SESUDAH**

```text
+- Hero: Administrasi Kepegawaian · Buat SK Kepegawaian Baru ------------- [ ← Kembali ] -+
| ⓘ SK adalah satu-satunya jalur resmi memindahkan unit, jabatan, atau atasan (HRD-DEC-048) |
+- Pegawai & Tindakan ----------------------------------------------------------------------+
| Pegawai *            [ Cari nama atau kode pegawai  ▾ ]  Kategori *  [ Mutasi          ▾ ] |
| Tanggal Efektif *    [ 03/11/2026                  📅 ]                                     |
+- Penempatan Baru — isi hanya yang berubah ------------------------------------------------+
| Unit Organisasi      [ Pilih unit                  ▾ ]  Departemen  [ Pilih departemen ▾ ] |
| Jabatan              [ Pilih jabatan               ▾ ]  Atasan      [ Cari pegawai     ▾ ] |
| Usulan Gaji Pokok    [                  ]  "Kosongkan bila tidak ada penyesuaian gaji"     |
+- Dasar Pertimbangan ----------------------------------------------------------------------+
| Alasan *   [                                                              ]  0/1500        |
| Catatan    [                                                              ]  0/2000        |
+----------------------------------------------------------- [ Batal ] [ Simpan Draf SK ] --+
```

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Hero | Judul, deskripsi, tombol Kembali | — | `Hero` + `BaseButton secondary` |
| Pegawai & Tindakan | Pegawai, kategori, tanggal efektif | `GET .../master-data/employees/options` (nilai `workforceProfileId`) | `BaseFormControl` + `ResourceFilterSelect`, `BaseSelectField`, `BaseDateField` |
| Penempatan Baru | Unit, departemen, jabatan, atasan, gaji | Resource `organizationUnits`, `departments` (opsi), `positions` (tersaring departemen), opsi karyawan | `ResourceFilterSelect`, `BaseInputField` number |
| Dasar Pertimbangan | Alasan, catatan | — | `BaseTextAreaField` dengan batas 1.500 / 2.000 |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali / Batal | `secondary` | Selalu, kecuali saat menyimpan | Kembali ke Daftar SK |
| Simpan Draf SK | `primary`, `type=submit` | Selalu, kecuali saat menyimpan | Validasi layar; bila lolos `POST /personnel-action-orders`, lalu ke Detail SK |

| Keadaan | Tampilan |
| --- | --- |
| Isian kurang | Pesan per isian di bawahnya, ditambah `InformationAlert` ringkas; tombol tetap dapat ditekan |
| Gagal server | `InformationAlert danger` berisi pesan backend dalam Bahasa Indonesia |
| Memuat opsi | Teks "Memuat …" di dalam daftar pilihan |

### 3.2 Detail SK (`/hr/workforce-core/personnel-actions/{id}`)

**SEBELUM** (lampiran 2 dan 4)

```text
SK: SK-RSMMC-20261103-0001 (hero)
Status Dokumen: (lencana nyaris tak terbaca) |Kategori: Mutasi
[ ← Kembali ][ Lihat Employee 360 ]   ← warna tombol utama
Identitas Pegawai & Tanggal Efektif
Nama Pegawai:
Siti ...                                ← label dan nilai bertumpuk tanpa kartu
...
(dialog Tolak/Batal muncul sebagai blok biasa di bawah halaman)
```

**SESUDAH**

```text
+- Hero: SK-RSMMC-20261103-0001 · Mutasi --------- [ ← Kembali ] [ Lihat Employee 360 ] -+
| (Draf) lencana status                                                                  |
+- Pegawai & Tanggal ---------------------------------------------------------------------+
| Nama Pegawai  Perawat A   | Kode  WFP-…-0037 | Tgl Efektif 02 Nov 2026 | Tgl Posting … |
+- Penempatan Baru -----------------------------------------------------------------------+
| Unit IGD | Departemen Keperawatan | Jabatan Perawat IGD | Atasan — | Gaji Pokok —      |
+- Dasar Pertimbangan --------------------------------------------------------------------+
| Alasan …                    ⚠ Alasan Penolakan / Pembatalan (bila ada)                  |
+- Berkas SK Bertanda Tangan (HRD-DEC-056) -----------------------------------------------+
| sk-mutasi.pdf · 826 B · 03 Nov 2026 10.08                               [ Unduh ]       |
| (Draf) [ Pilih berkas … ] [ Unggah / Ganti Berkas ]                                     |
+- Jejak Dokumen -------------------------------------------------------------------------+
| Diajukan 03 Nov 2026 10.08 | Disetujui — | Diterapkan — | Ditolak — | Dibatalkan —       |
+- Aksi ----------------------------------------------------------------------------------+
| *Pembuat dilarang menyetujui sendiri (HRD-DEC-031)   [ Batalkan SK ] [ Ajukan Persetujuan ] |
+-----------------------------------------------------------------------------------------+
```

| Tombol | Jenis | Kapan tampil | Yang terjadi |
| --- | --- | --- | --- |
| Ajukan Persetujuan | `primary` | Draf | `POST …/submit` |
| Setujui & Terapkan SK | `primary` | Menunggu Persetujuan | `POST …/approve` |
| Tolak SK | `danger` | Menunggu Persetujuan | `ConfirmModal` alasan wajib → `POST …/reject { reason }` |
| Batalkan SK | `secondary` | Draf, Menunggu Persetujuan, Terjadwal | `ConfirmModal` alasan wajib → `POST …/cancel { reason }` |
| Unduh | `secondary` sm | Ada lampiran | `GET …/attachments/{attachmentId}` → unduhan berkas |
| Unggah / Ganti Berkas | `primary` sm | Draf | `POST …/attachments` |

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Teks "Memuat rincian SK…" di dalam shell halaman |
| Tidak ditemukan / gagal | `InformationAlert danger` + tombol Kembali |
| Aksi gagal | `InformationAlert danger` di atas kartu; isi modal dipertahankan |
| Aksi berhasil | `ToastStack` sukses, data dimuat ulang |

### 3.3 Daftar SK (`/hr/workforce-core/personnel-actions`)

**SEBELUM**: kartu ringkasan dihitung dari halaman aktif, tanpa saringan Status/Kategori, tombol
"Buat SK" berdiri sendiri, lencana "Dibatalkan" terpotong.

**SESUDAH**

```text
+- Hero: Dokumen SK Kepegawaian ----------------------------------- [ + Buat SK Kepegawaian ] -+
+- [Total SK] [Menunggu Persetujuan] [Terjadwal] [Diterapkan] [Draf]   ← dari /summary ------+
+- [ cari nomor SK / nama / kode ] [ Status ▾ ] [ Kategori ▾ ] [↻] ----------------------------+
| No SK | Pegawai | Kategori | Tujuan Penempatan | Tgl Efektif | Status | Dokumen Fisik | Aksi |
| …     | Perawat A | Mutasi | IGD · Perawat IGD | 02 Nov 2026 | (Diterapkan) | (Terunggah) | [ Rincian ] |
+-----------------------------------------------------------------------------------------------+
```

### 3.4 Employee 360 (`/hr/workforce-core/employee-360/{workforceProfileId}`)

**SESUDAH**

```text
+- Hero: Employee 360 · <nama pegawai> ------------------------------------- [ ← Kembali ] -+
+- Kartu profil: (A) Perawat A  (Aktif)   Kode · Email · Telepon      (Mode: HR Penuh) -----+
+- [ Kedinasan ] [ Riwayat SK & Penempatan ] [ Kompetensi ] [ Finansial* ] [ Pribadi* ] -----+
+- Kartu seksi isi tab (grid informasi / DataTable riwayat SK / daftar linimasa) -------------+
| *Mode Atasan Langsung: tab rahasia menampilkan InformationAlert "Akses Dibatasi"           |
```

### 3.5 Batch Data Awal (`/hr/workforce-core/initial-batches`)

**SEBELUM** (lampiran 1): tombol batch mentah, tabel HTML tanpa gaya, dialog Buat Batch menempel di
halaman, pesan "Request failed with status code 404".

**SESUDAH**

```text
+- Hero: Batch Data Awal Penempatan ------------------------------- [ + Buat Batch Baru ] -+
| ⓘ Inisialisasi dilakukan satu kali (HRD-DEC-054)                                         |
| ⚠ Pesan galat Bahasa Indonesia (bila layanan belum tersambung — lihat FIX-HRD-002-09)    |
+- Batch: [ BATCH-INIT-…-0002 · Baseline Keperawatan ▾ ]  ------------------------------------+
| [Total Baris] [Valid] [Gagal] [Status]                                                      |
| Ringkasan eksekusi …                       [ Validasi Integritas ] [ Eksekusi Batch ]        |
+- DataTable: No | Pegawai | Unit | Departemen | Jabatan | Atasan | Status Validasi | Catatan -+
ConfirmModal "Buat Batch Baru" berisi isian Nama Batch
```

Alur tombolnya tidak berubah pada rencana ini (lihat `FIX-HRD-002-09`).

### 3.6 Pengajuan Atas Nama (`/hr/workforce-core/employee-profile-changes/create-on-behalf`)

**SESUDAH**: Hero + `InformationAlert warning` SoD, kartu "Pegawai & Kategori" (pilihan pegawai
bercari, kategori), kartu "Perubahan Data" (nama data, nilai lama, nilai baru, rujukan berkas),
kartu "Alasan Kuasa", lalu baris tombol `[ Batal ] [ Kirim Pengajuan ]`. Kontrak pengiriman tidak
berubah (lihat `FIX-HRD-002-09`).

## 4. Rincian perbaikan

### FIX-HRD-002-01 — CSS Module domain, utilitas tampilan bersama, dan shell standar

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `ISS-HRD-002-06` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | Baru: `src/style/hr/workforce-core/workforce-core.module.css`, `src/utils/hr/workforce-core/workforce-core-display-utils.js` |
| **Radius dampak** | Hanya layar Administrasi Kepegawaian yang mengimpornya |
| **Bergantung pada** | Tidak ada |

**Langkah.**

1. Tulis CSS Module dengan class layout (kartu, grid informasi, grid form, tab, kartu profil, bar
   aksi, daftar lampiran) yang seluruh nilainya `var(--…)` dari `globals.css`.
2. Tulis utilitas: `formatDate`, `formatDateTime`, `formatCurrency`, peta status SK dan batch ke kunci
   `StatusBadge`, label kategori, dan `getWorkforceCoreErrorMessage`.
3. Pakai shell `administrator-region-page` + `baseStyles.dataPage` + `themeStyles.regionTheme` /
   `region-page-shell` + `baseStyles.dataShell` pada keenam view, seperti layar rekap.

**Acceptance criteria.**

1. `grep` class khas Tailwind pada enam view dan CSS Module baru menghasilkan **0** baris.
2. Pemeriksa kunci CSS Module melaporkan **0** kunci yang tidak ada di modul yang diimpor.
3. CSS Module baru tidak memuat literal warna `#…`/`rgb(…)`, `!important`, maupun `font-size` literal.
4. Respons 404 tanpa isi menampilkan "Data atau layanan yang diminta tidak ditemukan." dan tidak
   pernah menampilkan "Request failed with status code …".

**Verifikasi.** `npx eslint` pada berkas yang diubah, grep anti-regresi checklist G (termasuk grep
baru dari `FIX-HRD-002-10`), dan screenshot setiap layar.

**Risiko.** Rendah; berkas baru.

### FIX-HRD-002-02 — Daftar SK

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-02`, `-03`, `-06`, `-T1`, `-T2` |
| **Berkas yang diubah** | `personnel-action-order-list-view.jsx`; baru `personnel-action-order-table-columns.jsx`; `personnel-action-order-service.js` (tambah `getSummary`) |
| **Radius dampak** | Daftar SK (`/hr/workforce-core/personnel-actions` dan alias `/hr/personnel-action-orders`) |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Saringan **Status** dan **Kategori** tampil. Memilih Status "Dibatalkan" mengirim
   `orderStatus=Cancelled` dan tabel hanya memuat SK dibatalkan.
2. Tombol ↻ mengosongkan kotak cari dan kedua saringan.
3. Kartu ringkasan berasal dari `GET …/personnel-action-orders/summary` dan memuat kartu Terjadwal.
4. Lencana status memakai warna sesuai peta: Diterapkan hijau, Menunggu Persetujuan kuning,
   Ditolak/Dibatalkan merah, Draf/Terjadwal biru-hijau. Lencana tidak terpotong.

### FIX-HRD-002-03 — Buat SK

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-03`, `-04`, `-06` |
| **Berkas yang diubah** | `personnel-action-order-create-view.jsx` |
| **Radius dampak** | Buat SK saja |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Tidak ada isian teks berisi UUID. Mengetik "Siti" pada Pegawai menampilkan pegawai yang
   cocok, dan memilihnya mengisi `workforceProfileId` pada payload.
2. Pilihan Jabatan tersaring menurut Departemen yang dipilih.
3. Alasan kurang dari 10 karakter, alasan lebih dari 1.500 karakter, gaji pokok ≤ 0, atau
   tanggal kosong ditahan di layar dengan pesan di bawah isiannya, tanpa permintaan ke server.
4. Simpan berhasil → pindah ke Detail SK baru.

### FIX-HRD-002-04 — Detail SK

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-02`, `-03`, `-05`, `-06`, `-07`, `-T3` |
| **Berkas yang diubah** | `personnel-action-order-detail-view.jsx`; `personnel-action-order-service.js` (tambah `downloadAttachment`) |
| **Radius dampak** | Detail SK saja |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Tolak SK membuka `ConfirmModal`. Tombol konfirmasi tidak dapat ditekan selama alasan kosong.
   Alasan "Kualifikasi belum terpenuhi" mengirim `{ "reason": "Kualifikasi belum terpenuhi" }` dan
   SK menjadi **Ditolak** (HTTP 200, bukan 500).
2. Batalkan SK pada SK Terjadwal mengirim `{ "reason": … }` dan SK menjadi **Dibatalkan**.
3. Daftar lampiran menampilkan nama, ukuran, dan waktu unggah. Tombol Unduh mengunduh berkasnya.
4. Selama Draf, berkas dapat diganti; setelah diajukan, formulir unggah tidak tampil.
5. Nilai identitas pengguna (UUID) tidak ditampilkan di Jejak Dokumen.

### FIX-HRD-002-05 — Employee 360

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-02`, `-03`, `-04`, `-06`, `-T4` |
| **Berkas yang diubah** | `employee-360-view.jsx`, `employee-360-service.js`, `tests/unit/workforce-core-target-state.test.mjs` |
| **Radius dampak** | Employee 360 (dua route: `/hr/workforce-core/employee-360/[id]` dan alias `/hr/employee-360/[id]`) |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Service memanggil `/v1/corporate/human-resource/workforce-profiles/{id}/employee-360`, dan
   test unit menuntut alamat itu.
2. Profil pegawai yang ada tampil lengkap dalam mode HR. Tab tampil sebagai tab dengan penanda tab
   aktif, dan riwayat SK memakai `DataTable`.
3. Bila `viewMode` adalah `DirectManager`, tab Finansial dan Pribadi menampilkan peringatan
   "Akses Dibatasi" dan tidak memuat nilai rahasia.
4. Tidak ada UUID yang tampil.

### FIX-HRD-002-06 — Batch Data Awal (tampilan)

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-02`, `-03`, `-05`, `-06` |
| **Berkas yang diubah** | `initial-placement-batch-view.jsx` |
| **Radius dampak** | Batch Data Awal (dua route: `/hr/workforce-core/initial-batches` dan alias `/hr/initial-placement-batch`) |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Dialog Buat Batch tampil sebagai dialog dengan latar gelap dan tombol tutup.
2. Galat 404 tampil sebagai pesan Bahasa Indonesia.
3. Status batch memakai nilai backend yang sebenarnya (`Draft`, `Validated`, `Executed`, `Failed`)
   dan baris memakai `isValid` / `validationMessage`.
4. Tidak ada `<button>` atau `<table>` mentah di view.

### FIX-HRD-002-07 — Pengajuan Atas Nama (tampilan)

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-01`, `-03`, `-04`, `-06` |
| **Berkas yang diubah** | `employee-profile-change-on-behalf-view.jsx` |
| **Radius dampak** | Dua route: `.../employee-profile-changes/create-on-behalf` dan `.../on-behalf` |
| **Bergantung pada** | `FIX-HRD-002-01` |

**Acceptance criteria.**

1. Pegawai dipilih dari daftar bercari, bukan diketik.
2. Semua isian memakai field standar dengan label, bintang wajib, dan pesan di bawah isian.
3. Kontrak pengiriman tidak berubah; penolakan server tampil sebagai pesan Bahasa Indonesia.

### FIX-HRD-002-08 — Menu sidebar

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-08` |
| **Berkas yang diubah** | `src/utils/menu-sidebar/menu-items.jsx` |
| **Radius dampak** | Sidebar grup Administrasi Kepegawaian |

**Acceptance criteria.** Grup Administrasi Kepegawaian memuat "Dokumen SK Kepegawaian"
(`/hr/workforce-core/personnel-actions`) dan "Batch Data Awal" (`/hr/workforce-core/initial-batches`).
Menu aktif ikut menyala pada halaman detail dan buat SK.

### FIX-HRD-002-09 — Sambungkan alur Batch dan Atas Nama ke kontrak

⛔ Terblokir. Lihat K-02 dan `ISS-HRD-002-T5`. Begitu backend menolak eksekusi batch yang belum
`Validated` dan membuka jalur pengajuan draf atas nama, layar Batch perlu diubah menjadi alur
"ambil template → isi jabatan/atasan/gaji per baris → validasi → buat → eksekusi", dan form Atas Nama
perlu mengirim `multipart/form-data` dengan `Details[i]` dari `allowedFields` serta `EvidenceFile`.

### FIX-HRD-002-11 — Endpoint opsi profil tenaga kerja

| | |
| --- | --- |
| **Menutup** | `ISS-HRD-002-T7` |
| **Area** | Backend + Frontend |
| **Berkas yang diubah** | Backend: controller opsi profil tenaga kerja baru (lokasi ditetapkan task backend). Frontend: `src/lib/hooks/select/hr/hr-select-resources.js` resource `workforceProfiles` |
| **Bergantung pada** | K-03 |

**Acceptance criteria.**

1. Mengetik nama seorang dokter tetap pada isian Pegawai di form Buat SK menampilkan dokter tersebut.
2. Nilai yang terkirim adalah `workforceProfileId`, dan daftar memuat seluruh 55 profil aktif di DB dev.

### FIX-HRD-002-10 — Perkuat skill `build-module-frontend` dan aturan frontend

| | |
| --- | --- |
| **Menutup** | Pola pada issue §8 |
| **Area** | Repository `QuilvianEngineeringSkills` |
| **Berkas yang diubah** | `agents/rules/frontend/frontend-architecture.md`, `design-tokens.md`, `base-component-catalog.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`; `SKILL.md` `build-module-frontend` pada tiga edisi; mirror dan salinan reference lewat `tooling/sync-rules.ps1` |
| **Radius dampak** | Setiap task frontend Quilvian berikutnya |

**Acceptance criteria.**

1. Aturan menyatakan Tailwind tidak terpasang dan dilarang, dan `className` literal hanya boleh
   memakai class global yang terbukti ada.
2. Checklist G memuat grep class Tailwind, grep dialog buatan sendiri, grep nilai prop base component
   yang tidak sah, dan perintah pemeriksa kunci CSS Module, masing-masing dengan target 0 hasil.
3. Skill mewajibkan screenshot setiap layar yang disentuh. Laporan tanpa tabel keputusan, baris
   `UI GATE`, hasil grep, dan screenshot tidak boleh berstatus ✅.
4. `tooling/sync-rules.ps1 -Check` lulus setelah sinkronisasi.

## 5. Urutan pengerjaan

```text
FIX-HRD-002-10 🟡   tanpa dependency — di-commit db27872, menunggu push
FIX-HRD-002-01 ✅
├── FIX-HRD-002-02 ✅
│   └── FIX-HRD-002-08 ✅   menu menunjuk ke layar yang sudah rapi
├── FIX-HRD-002-03 ✅
│   └── FIX-HRD-002-11      endpoint opsi profil tenaga kerja (menunggu K-03)
├── FIX-HRD-002-04 ✅
├── FIX-HRD-002-05 ✅
├── FIX-HRD-002-06 ✅
└── FIX-HRD-002-07 ✅
K-02 (keputusan pemilik) + perbaikan backend LBT-WFC-09, -10, KI-05
└── FIX-HRD-002-09 ⛔
```

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- |
| K-01 | Sudah diambil 03-11-2026: perbaiki seluruh tampilan dan perkuat skill | — | — |
| K-02 | Kapan alur Batch dan Atas Nama disambungkan | Setelah backend menutup LBT-WFC-09, -10 dan KI-05 | `FIX-HRD-002-09` |
| K-03 | Setujui task backend endpoint opsi profil tenaga kerja agar dokter dan tenaga tanpa data karyawan dapat dipilih di SK | Setujui; perubahan frontend cukup satu baris endpoint | `FIX-HRD-002-11` |

## 7. Dokumen hulu yang ikut direvisi

Tidak ada. Seluruh perbaikan mengembalikan source ke desain yang sudah disetujui pada
`03-frontend-architecture.md` 4.2a dan 4.2b serta tabel menu. Tidak ada `DESIGN_CHANGE`.

## 8. Verifikasi menyeluruh

1. Login sebagai HR-A. Buka Dokumen SK dari sidebar, saring Status = Draf, lalu buat SK mutasi
   dengan memilih pegawai, unit, departemen, dan jabatan dari daftar.
2. Unggah scan, ganti dengan berkas lain, unduh berkas, lalu ajukan.
3. Login sebagai HR-B. Tolak SK dengan alasan. Status harus menjadi Ditolak dan alasannya tampil.
4. Buka Employee 360 dari Detail SK. Profil tampil lengkap dalam mode HR.
5. Buka Batch Data Awal dan Pengajuan Atas Nama dari sidebar/URL. Tampilan rapi, dialog tampil
   sebagai dialog, dan galat berbahasa Indonesia.
6. Jalankan seluruh grep anti-regresi pada berkas yang diubah: 0 hasil.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-03 | Rencana dibuat: 10 perbaikan. Perintah pemilik pada laporan menyetujui `FIX-HRD-002-01` s.d. `-08` dan `-10`; `-09` ditahan K-02 | `diagnose-module-issue` |
| 2026-11-03 | `FIX-HRD-002-01` s.d. `-08` ✅ (laporan `FE-HRD-022`, `-024`, `-025`, `-026`, `-027`): `check-css-usage` 0 temuan, lint 0 error, 22/22 test Workforce Core lulus, `npm run build` lulus, screenshot 01–27. `FE-HRD-026` dan `FE-HRD-027` dikoreksi ke 🟡 di roadmap karena acceptance criteria-nya menunggu `FIX-HRD-002-09`. `FIX-HRD-002-10` 🟡 (repo skill di-commit pemilik sebagai `db27872`, belum di-push). `FIX-HRD-002-11` ditambahkan dari temuan `T7` | Pengerjaan perbaikan |
