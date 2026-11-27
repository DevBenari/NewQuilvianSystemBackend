# Perencanaan & Permintaan — Roadmap Frontend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `perencanaan-permintaan` (`approved`, `PBJ-DEC-034`) |
| Revisi roadmap | `2` · status `draft`. Rev 2 menambah `FE-PBJ-009` (`PBJ-FE-004`) |
| Tanggal | 12 November 2026; rev 2 17 November 2026 |
| Kontrak | set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`); `03-frontend-architecture.md` |
| Kewenangan UI | `PBJ-FE-001` (nama menu SOURCE, `approved`), `PBJ-DEC-032` (grup Pengaturan, `approved`), `PBJ-FE-002` (rupa: `DEV_DISCRETION`), `PBJ-FE-003` (invariant), `PBJ-FE-004` (standar layar daftar, `approved` 17 November 2026) |
| SHA | FE `763d665b` (branch `sukmagpV2`), BE `100c011d` |
| Repository target | `V2QuilvianSystemFrontendDev` |

**Kebijakan uji frontend** (`rules/frontend/test-policy.md`): bukti utama adalah verifikasi manual kontrol
interaktif, lint, dan build. Test unit `node:test` direkomendasikan untuk logika murni (mis. pemetaan label
status), tetapi tidak wajib. Uji browser memakai Playwright dari scratchpad dengan backend di
`https://localhost:7184`. Tidak ada task yang gagal hanya karena tidak menambah test.

**Kontrak dikunci:** frontend boleh mulai paralel dengan backend hanya untuk endpoint kontrak set `1.0`. Bila
respons nyata berbeda dari kontrak, kontrak yang direvisi, bukan frontend yang menyesuaikan diam-diam.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; terbukti di laporan task |
| 🟡 | Sebagian |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Tiga blok karena jumlah node (17 > 15). `FE-PBJ-001` dan `FE-PBJ-002` pada blok 2, serta `FE-PBJ-005`, `007`, `008`
pada blok 3, adalah node yang sama dengan blok sebelumnya (sambungan).

```text
BE-PBJ-003 [BE] ✅ ─> FE-PBJ-001 ✅ ─────┐
                                         │
BE-PBJ-004 [BE] ✅ ─> FE-PBJ-002 ✅ ─┐   │
                                     │   │
BE-PBJ-006 [BE] ✅ ──────────────────┴───┴─> FE-PBJ-003 ✅ ─┬─> FE-PBJ-004 ✅ ─┬─> FE-PBJ-005 ✅
                                                            │                  │
                                       BE-PBJ-007 [BE] ✅ ──┘                  │
                                                                               │
                                       BE-PBJ-008 [BE] ✅ ─────────────────────┘
```

```text
FE-PBJ-001 ✅ ─────────────────┐                 BE-PBJ-010 [BE] ✅ ─┐
                               │                                     │
FE-PBJ-002 ✅ ─────────────┐   │                                     │
                           │   │                                     │
BE-PBJ-009 [BE] ✅ ────────┴───┴─> FE-PBJ-006 ✅ ─┬──────────────────┴─> FE-PBJ-007 ✅
                                                  │
                                                  └──────────────────┬─> FE-PBJ-008 ✅
                                                                     │
                                                 BE-PBJ-011 [BE] ✅ ─┘
```

```text
FE-PBJ-005 ✅ ──────────────┐
                            │
FE-PBJ-007 ✅ ──────────┐   │
                        │   │
FE-PBJ-008 ✅ ──────┐   │   │
                    │   │   │
BE-PBJ-012 [BE] ✅ ─┴───┴───┴─> FE-PBJ-009 ✅
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja. Status backend per 17 November 2026: `BE-PBJ-001`..`012`
✅. Gerbang registry `PBJ-OQ-002` sudah ditutup (`PBJ-DEC-035`).

Jumlah pasangan prasyarat → task: **20**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-PBJ-003` | `FE-PBJ-001` ✅ — 16 November 2026 |
| 1 | `BE-PBJ-004` | `FE-PBJ-002` ✅ — 16 November 2026 |
| 2 | `FE-PBJ-001`, `FE-PBJ-002`, `BE-PBJ-006` | `FE-PBJ-003` ✅ — 16 November 2026 |
| 2 | `FE-PBJ-001`, `FE-PBJ-002`, `BE-PBJ-009` | `FE-PBJ-006` ✅ — 16 November 2026 |
| 3 | `FE-PBJ-003`, `BE-PBJ-007` | `FE-PBJ-004` ✅ — 16 November 2026 |
| 3 | `FE-PBJ-006`, `BE-PBJ-010` | `FE-PBJ-007` ✅ — 16 November 2026 |
| 3 | `FE-PBJ-006`, `BE-PBJ-011` | `FE-PBJ-008` ✅ — 16 November 2026 |
| 4 | `FE-PBJ-004`, `BE-PBJ-008` | `FE-PBJ-005` ✅ — 16 November 2026 |
| 5 | `FE-PBJ-005`, `FE-PBJ-007`, `FE-PBJ-008`, `BE-PBJ-012` | `FE-PBJ-009` ✅ — 17 November 2026 |

Gelombang dihitung dari prasyarat frontend. Prasyarat `[BE]` menahan mulai hingga task backend-nya ✅. Per
16 November 2026 seluruh prasyarat `[BE]` frontend sudah ✅.

## Tabel task

| Task ID | Outcome | Layar | Kontrak / izin | Reuse | Dependency | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FE-PBJ-001` | Grup menu Pengadaan + Pengaturan; layanan API dasar; katalog, kategori, sumber dana | `FE-PBJ-PP-18`, `-19` | API §Master Data; `ProcurementItem`, `ProcurementMasterData` | `InstanceAxios`, pola daftar/form existing, design token | `BE-PBJ-003` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-001.md)) |
| `FE-PBJ-002` | Komponen lampiran (daftar, unggah, unduh, versi) yang dipakai ulang semua layar | komponen | API §Attachment; `ProcurementAttachment` | base component unggah existing bila ada | `BE-PBJ-004` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-002.md)) |
| `FE-PBJ-003` | Daftar PR dan form/detail draft | `-09`, `-10` (mode draft) | API §Purchase Request GET/POST/PUT/history | — | `FE-PBJ-001`, `FE-PBJ-002`, `BE-PBJ-006` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-003.md)) |
| `FE-PBJ-004` | Ajukan, batal, tarik; sisa rencana; layar Mendesak, Riwayat, Menunggu Persetujuan | `-10` (aksi), `-14`, `-15`, `-16` | API submit/cancel/plan-line-options; STATE §1 | — | `FE-PBJ-003`, `BE-PBJ-007` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-004.md)) |
| `FE-PBJ-005` | Antrean verifikasi, ruang kerja verifikasi, hasil cek stok, pintu kotak masuk `Wfl`, ringkasan pagu di detail PR | `-11`, `-12`, `-13`, `-17`; route 11.2 | API §Verification, `budgets/remaining`; kotak masuk `Wfl` existing | layar kotak masuk HR (`approval-inbox-view`) sebagai acuan | `FE-PBJ-004`, `BE-PBJ-008` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-005.md)) |
| `FE-PBJ-006` | Rencana kebutuhan, revisi, banding versi | `-01`, `-02`, `-08` (bagian rencana) | API §Need Plan | — | `FE-PBJ-001`, `FE-PBJ-002`, `BE-PBJ-009` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-006.md)) |
| `FE-PBJ-007` | Pagu dan revisinya | `-04`, `-05`, `-08` (bagian pagu) | API §Budget | — | `FE-PBJ-006`, `BE-PBJ-010` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-007.md)) |
| `FE-PBJ-008` | Konsolidasi dan rencana paket | `-03`, `-06`, `-07` | API §Consolidation, §Package Plan | — | `FE-PBJ-006`, `BE-PBJ-011` | ✅ 16 November 2026 ([laporan](../task/report/frontend/FE-PBJ-008.md)) |
| `FE-PBJ-009` | Baris filter standar satu baris dan hero tanpa tombol di semua layar daftar Pengadaan | `-02`, `-03`, `-05`, `-06`, `-09`, `-11`, `-13`..`-19` | API 1.4 "Saringan tanggal daftar"; `PBJ-FE-004`; FE §4.7 | `DataFilter`, `FilterDatePicker`, `FilterSelect`, `BaseButton`; rujukan `administrator/master-data/supplier` | `FE-PBJ-005`, `FE-PBJ-007`, `FE-PBJ-008`, `BE-PBJ-012` | ✅ 17 November 2026 ([laporan](../task/report/frontend/FE-PBJ-009.md)) |

## Kartu task

Ketentuan bersama untuk semua kartu:
- Setiap kartu yang memperkenalkan layar wajib **mendaftarkan butir menunya** (atau jalur masuk layar anak)
  sesuai `03-frontend-architecture.md` §3. Itu bagian dari acceptance criteria.
- Rupa tampilan `DEV_DISCRETION`.
- Tombol tanpa izin disembunyikan.
- Angka hasil hitungan hanya diambil dari respons server.
- Keempat keadaan (memuat, kosong, gagal, berisi) beserta data basi dan pengiriman ganda mengikuti §6.

### ✅ `FE-PBJ-001` — Menu Pengadaan dan katalog

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir tingkat 0 "Pengadaan Barang & Jasa" dengan grup sesuai `02-module-map.md` §3 (hanya butir yang layarnya sudah ada; butir lain ditambahkan oleh task layarnya); grup "Pengaturan Pengadaan"; `src/lib/services/corporate/procurement/procurement-master-data-service.js`; layar `-18` (daftar, tambah, ubah, nonaktif/aktif katalog) dan `-19` (kategori, sumber dana, jenis baca-saja) |
| Acceptance criteria | (1) Menu tampil hanya bagi pemegang `ProcurementItem : Read`. (2) Tambah barang menampilkan kode `KTL-…` dari server. (3) Pesan `PBJ-VAL-070`/`071` tampil terbaca. (4) Keadaan kosong/gagal sesuai skema |
| Verifikasi | Lint; build; uji browser manual dengan akun Tim Pengadaan dan akun tanpa izin; tangkapan layar (`AT-PP-63` sebagian) |
| DoD | Kriteria + laporan `task/report/frontend/FE-PBJ-001.md` |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: tambah barang `KTL-000013`, `PBJ-VAL-070`/`071`, menu tersembunyi tanpa `ProcurementItem : Read`. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-001.md) |

### ✅ `FE-PBJ-002` — Komponen lampiran

| Aspek | Isi |
| --- | --- |
| Cakupan | Komponen fitur `procurement-attachment-panel` (props `documentType`, `documentId`, `readOnly`); daftar versi berlaku, riwayat versi, unggah (validasi awal ekstensi dan ukuran di klien hanya sebagai bantuan), unduh lewat endpoint `download` |
| Acceptance criteria | (1) Penolakan server `PBJ-VAL-080`..`083` tampil terbaca. (2) Unggah pengganti menampilkan versi baru. (3) `readOnly` menyembunyikan unggah |
| Verifikasi | Lint; build; uji manual pada halaman uji sementara atau layar pertama yang memakainya (`FE-PBJ-003`) |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: unggah, ganti versi, `PBJ-VAL-081`, ekstensi terlarang dicegah, `readOnly`. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-002.md) |

### ✅ `FE-PBJ-003` — Daftar dan draft PR

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu 3 dan "Pengajuan Pembelian Barang & Jasa"; `purchase-request-service.js`; layar `-09`; layar `-10` mode buat/ubah draft (kepala, baris dengan pemilih katalog, satuan, harga perkiraan), tab lampiran (`FE-PBJ-002`), tab riwayat; penanganan 409 data basi dan 404 cakupan |
| Acceptance criteria | (1) Saringan unit hanya menampilkan unit dalam cakupan. (2) Simpan draft → muncul di daftar sebagai "Draf". (3) `AT-PP-08` dari sisi FE: tautan PR unit lain menampilkan pesan 404. (4) Data basi → pesan + tombol Muat ulang tanpa membuang isian |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: daftar dan saringan, simpan draf, 409 + Muat ulang, 404 cakupan. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-003.md) |

### ✅ `FE-PBJ-004` — Pengajuan, batal, tarik, dan daftar turunan

| Aspek | Isi |
| --- | --- |
| Cakupan | Aksi Ajukan (konfirmasi, `Idempotency-Key` per niat aksi), Batalkan, Tarik; pilihan baris rencana dengan sisa (`plan-line-options`) dan isian alasan luar rencana per baris; tanda Mendesak/Di luar rencana; layar `-14`, `-15`, `-16` beserta butir menunya (termasuk menu 1 "Pengajuan Menunggu Persetujuan") |
| Acceptance criteria | (1) Galat `PBJ-VAL-001`..`012` tampil di dekat isian. (2) Klik ganda Ajukan tidak membuat dua pengajuan. (3) Setelah diajukan, nomor `PR-…` dan pembagian dalam/luar rencana tampil dari respons |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: galat baris, klik ganda Ajukan = 1 pengajuan, Batalkan, Tarik, layar -14/-15/-16. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-004.md) |

### ✅ `FE-PBJ-005` — Verifikasi dan persetujuan

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "Verifikasi Kebutuhan", "Pemeriksaan Stock", "Antrean persetujuan", "Validasi Pengajuan" (route kedua ke layar `-11`); layar `-11`, `-12` (cek stok per baris, Selesaikan, Kembalikan, Tolak, Batalkan PR disetujui), `-13`, `-17` (menautkan atau menyaring kotak masuk `Wfl` kategori Procurement, tanpa membuat kotak masuk kedua); tab Persetujuan di `-10`; ringkasan sisa pagu di `-10` |
| Acceptance criteria | (1) Tombol verifikasi tersembunyi bagi pembuat PR. (2) `PBJ-VAL-091` tampil sebagai "Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator." (3) Tanda lewat pagu dan nilainya tampil setelah verifikasi. (4) Penyetuju dapat menyetujui dari kotak masuk, dan status PR berubah setelah dimuat ulang |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: cek stok, selesaikan, Kembalikan, Tolak, `PBJ-VAL-091`, 4 persetujuan dari antrean, Batalkan PR disetujui. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-005.md) |

### ✅ `FE-PBJ-006` — Rencana kebutuhan dan revisi

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "Rencana Kebutuhan Tahunan" dan "Revisi Perencanaan"; layar `-01`, `-02` (kolom baca "Sudah dipakai PR", "Dialokasikan paket"; tombol Buat Revisi), `-08` bagian rencana dengan panel banding versi |
| Acceptance criteria | (1) Rencana kedua unit-tahun sama → pesan `PBJ-VAL-040` dengan tautan ke rencana yang ada. (2) Banding versi menampilkan sebelum → sesudah per baris. (3) `PBJ-VAL-042` tampil di baris yang bermasalah |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: `PBJ-VAL-040` + tautan, Buat Revisi + `PBJ-VAL-042`, banding versi. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-006.md) |

### ✅ `FE-PBJ-007` — Pagu

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "Rencana Anggaran"; layar `-04` (pagu, komitmen, sisa), `-05`; bagian pagu di `-08` |
| Acceptance criteria | (1) Kolom sisa = respons server. (2) `PBJ-VAL-060`/`062` terbaca. (3) Nominal tidak tampil bagi pengguna tanpa `ProcurementBudget : Read` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: sisa pagu, `PBJ-VAL-060` + tautan, Buat Revisi + `PBJ-VAL-062`, nominal tersembunyi tanpa izin. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-007.md) |

### ✅ `FE-PBJ-008` — Konsolidasi dan rencana paket

| Aspek | Isi |
| --- | --- |
| Cakupan | Butir menu "Konsolidasi Kebutuhan Unit" dan "Rencana Paket Pengadaan"; layar `-03` (gabungan per barang, rincian unit, pilih dan "Buat rencana paket dari pilihan"), `-06`, `-07` |
| Acceptance criteria | (1) Rincian unit asal tampil per barang. (2) `PBJ-VAL-050`/`051` terbaca. (3) Rencana paket yang dibuat dari konsolidasi memuat unit asal |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 16 November 2026.** Uji layar: konsolidasi → paket dengan unit asal, `PBJ-VAL-050`. `npm run lint:errors` PASS; `npm run build` PASS (24 rute); unit test 4749/4800 lulus, 51 gagal di modul lain (A/B identik dengan `HEAD`). Laporan: [laporan](../task/report/frontend/FE-PBJ-008.md) |

### ✅ `FE-PBJ-009` — Standar baris filter dan hero

| Aspek | Isi |
| --- | --- |
| Outcome | Semua layar daftar Pengadaan tampil seragam dengan modul lain: satu baris filter dan tombol aksi di filter, bukan di hero |
| Jejak | `PBJ-FE-004`, `PBJ-DEC-056`, `PBJ-DEC-057`; `03-frontend-architecture.md` §4.7; API 1.4 |
| Cakupan | Layar daftar PR, Mendesak, Riwayat, Menunggu Persetujuan, Antrean Verifikasi, Pemeriksaan Stock, Rencana Kebutuhan, Rencana Anggaran, Rencana Paket, Revisi Perencanaan, Katalog, Kategori & Sumber Dana, Antrean Persetujuan, Konsolidasi. Baris: Tanggal awal, Tanggal akhir, Periode, saringan relevan §4.7, Status, Pencarian, Jumlah baris, ↻. Tombol aksi di `actions` `DataFilter` varian `primary`. Halaman form/detail dokumen tidak diubah |
| Acceptance criteria | `AT-PP-66`; saringan tanggal/periode mengubah permintaan ke server dan hasilnya; ↻ mengembalikan semua saringan; Jumlah baris mengubah ukuran halaman; pengecualian `PBJ-DEC-057` sesuai |
| Verifikasi | Gerbang keputusan base component, lint, build, uji layar Playwright per layar pada lebar desktop |
| DoD | Kriteria + laporan `task/report/frontend/FE-PBJ-009.md` |
| **Status** | ✅ **SELESAI 17 November 2026.** Uji layar 81/81 `PASS` (15 layar × 1440/1920 px: satu baris, hero tanpa tombol, tombol primary; interaksi Periode, Jumlah baris, ↻, `period` `Wfl`). `npm run lint:errors` dan `npm run build` exit 0 (24 rute). Antrean Persetujuan ikut pola penuh (`PBJ-DEC-058`); Konsolidasi tetap pengecualian. Catatan: label terpotong di 1440 px. Laporan: [laporan](../task/report/frontend/FE-PBJ-009.md) |
