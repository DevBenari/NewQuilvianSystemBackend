# Laporan Perubahan Frontend — `FE-PBJ-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-001` |
| Judul | Menu Pengadaan dan katalog |
| Slice | Gelombang 1 (`MVP-0`) |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | `FE-PBJ-PP-18`, `-19`; `PBJ-DEC-032`; API §Master Data; `BE-PBJ-003` ✅ |
| Contract version | Set `1.0` (`PBJ-PP-API`, `-PERM`, `-STATE`, `-VAL`) dengan delta backend 1.1 yang dicatat pada laporan BE-PBJ-006..011; `03-frontend-architecture.md` |
| Wewenang UI | `PBJ-FE-001` nama menu SOURCE (approved), `PBJ-DEC-032` grup Pengaturan (approved), `PBJ-FE-002` rupa `DEV_DISCRETION`, `PBJ-FE-003` invariant |
| Dependency | `BE-PBJ-003` ✅ |
| Klasifikasi | `MEDIUM` — 2 layar, 1 hook, 1 service, menu; tanpa state global |
| Task mode | `FRONTEND` (instruksi pengguna "BE lalu FE", 16 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `266e3fc6e` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `27fac19c` (branch `sukmagp`, berisi pekerjaan BE-PBJ-002..011 yang belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Frontend belum punya satu pun berkas Pengadaan: tidak ada butir menu "Pengadaan Barang & Jasa", tidak ada service, dan tidak ada layar katalog. Backend master Pengadaan (`BE-PBJ-003`) sudah tersedia: katalog barang/jasa dengan kode `KTL-…` dibuat server, kategori, sumber dana, dan jenis pengadaan baca-saja.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Tim Pengadaan membuka **Pengadaan Barang & Jasa → Pengaturan Pengadaan → Katalog Barang/Jasa**.
2. Layar menampilkan ringkasan (total, aktif, nonaktif, barang, jasa), saringan status/jenis/kategori, dan daftar katalog.
3. **Tambah Barang/Jasa** membuka form: nama, jenis, kategori, satuan, spesifikasi bawaan. Kode tidak diisi; setelah disimpan, daftar menampilkan kode dari server, misalnya `KTL-000013`.
4. Bila kombinasi nama-jenis-kategori-satuan sudah ada, form menampilkan "Barang serupa sudah ada: KTL-000013 …" (`PBJ-VAL-070`).
5. **Nonaktifkan** menyembunyikan barang dari pilihan dokumen baru. **Hapus** barang yang sudah dipakai ditolak dengan pesan backend "Barang/jasa sudah dipakai pada permintaan pengadaan. Nonaktifkan saja agar tidak dipakai lagi." (`PBJ-VAL-071`).
6. Layar **Kategori & Sumber Dana** memiliki tab Kategori Barang, Sumber Dana (tambah/ubah/nonaktif/hapus), dan Jenis Pengadaan (baca-saja, tanpa tombol tambah).
7. Jalur tidak normal: pengguna tanpa `ProcurementItem : Read` tidak melihat butir menu; membuka URL langsung menampilkan "Akses Ditolak". Pengguna tanpa `Measurement : Read` melihat pesan "Satuan tidak ditemukan atau Anda belum berhak membaca master satuan." pada pilihan satuan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` §2–§3, `02-module-map.md` §3, `api-contract.md` §Master Data, `permission-audit-matrix.md`, `validation-matrix.md`, modul acuan `corporate/accounting` (daftar dan form), `src/utils/menu-sidebar/menu-items.jsx`, `left-sidebar-items-virtualized.jsx` (penyaring `requiredPermission`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Blok tingkat 0 "Pengadaan Barang & Jasa" dengan grup Dashboard Pengadaan, Rencana Pengadaan, Permintaan Pengadaan, Persetujuan & Laporan Pengadaan, dan Pengaturan Pengadaan; setiap butir ber-`requiredPermission` sesuai §3 |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | Endpoint, rute, butir izin, label status, pilihan saringan, label riwayat dan `Wfl` |
| `src/utils/corporate/procurement/procurement-utils.jsx` | Pembaca respons/galat (`code`, `lineNo`, `existingId`), kunci idempoten, format angka/tanggal |
| `src/lib/services/corporate/procurement/procurement-master-data-service.js` | Katalog, kategori, sumber dana, jenis; pilihan satuan dan unit |
| `src/lib/hooks/corporate/procurement/use-procurement-document-list.jsx`, `use-procurement-action.jsx`, `use-procurement-master-data.jsx` | Controller daftar ber-saringan, runner aksi dengan toast dan kunci klik ganda, controller master |
| `src/components/view/corporate/procurement/settings/procurement-item-view.jsx` | Layar `-18` |
| `src/components/view/corporate/procurement/settings/procurement-reference-view.jsx` | Layar `-19` |
| `src/app/corporate/procurement/settings/items/page.jsx`, `settings/references/page.jsx` | Entry point rute |
| `src/style/corporate/procurement/procurement.module.css` | Gaya fitur berbasis token |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi mengikuti `frontend-architecture.md`: `src/app/corporate/procurement/**/page.jsx` hanya entry point dan `metadata` → view di `src/components/view/corporate/procurement/**` → hook di `src/lib/hooks/corporate/procurement/**` → service `InstanceAxios` di `src/lib/services/corporate/procurement/**`; konstanta di `src/lib/constants/corporate/procurement/procurement-constants.jsx`; fungsi murni di `src/utils/corporate/procurement/procurement-utils.jsx`; gaya di `src/style/corporate/procurement/procurement.module.css` (token saja, tanpa warna literal, tanpa `!important`, tanpa inline style). Tidak ada Redux slice baru: state layar bersifat lokal per layar dan dibaca ulang dari server (roadmap mengunci service per dokumen). Deviasi terhadap checklist yang dipertahankan: definisi kolom tabel ditulis di dalam view (bukan berkas `-table-columns.jsx` terpisah) dan layar form/detail memakai susunan `COMPOSE` (lihat keputusan di bawah). Aturan tipografi pada `procurement.module.css` hanya menyasar kelas fitur (judul kartu, teks redup, tabel baris), memakai token, dan tidak menimpa komponen shared. Standar master data (`<FEATURE>_CONFIG` + sembilan thunk Redux) tidak diikuti karena roadmap mengunci `procurement-master-data-service.js` dan jenis pengadaan hanya baca-saja; deviasi ini dicatat.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman dan aksi utama | `REUSE` | `Hero` (`actions`) |
| Kartu ringkasan angka | `REUSE` | `SummaryGrid` |
| Saringan, pencarian, atur ulang | `REUSE` | `DataFilter` + `FilterSelect` |
| Tabel daftar dan paginasi | `REUSE` | `DataTable` + `Pagination` lewat `PaginationComponent` |
| Label status dokumen | `WRAP` | `StatusBadge` dibungkus `procurement-status-badge.jsx` (peta kode status → label Bahasa Indonesia + nada warna; teks selalu tampil, BRD B10) |
| Konfirmasi dan alasan wajib | `REUSE` | `ConfirmModal` (`requireReason`) |
| Notifikasi hasil aksi | `REUSE` | `ToastStack` |
| Pesan galat / info | `REUSE` | `InformationAlert`, `AccessDeniedGate` |
| Tombol | `REUSE` | `BaseButton` |
| Isian form | `REUSE` | `BaseTextField`, `BaseInputField`, `BaseSelectField`, `BaseDateField`, `BaseTextAreaField`, `BaseCheckboxField` |
| Tabel baris dokumen yang dapat diisi | `COMPOSE` | `<table>` + field base di dalam sel, mengikuti `journal-form-view.jsx` (Akuntansi); tabel baca-saja memakai `data-flat-table="true"` |
| Susunan halaman form/detail dokumen | `COMPOSE` | `Hero` + kartu bagian (`procurement.module.css`) + `ConfirmModal` + `ToastStack` |
| Tab Lampiran / Persetujuan / Riwayat | `COMPOSE` | `BaseButton` ber-`role="tab"` (belum ada base Tabs; pola sama dengan layar Akuntansi) |

Pilihan untuk elemen bukan `REUSE`:

1. **Susunan form/detail dokumen**
   1. `COMPOSE` Hero + kartu bagian, seperti yang dibangun **(rekomendasi)** — satu layar melayani mode draf (ubah) dan mode baca/verifikasi beserta tab alur; konsistensi visual terjaga karena semua bagian memakai token dan base field; risiko regresi nol pada komponen shared; biaya rendah.
   2. Migrasi ke `BaseDetailView` dengan isi editor sebagai `children` — lebih seragam dengan layar detail Akuntansi, tetapi mode ubah perlu dipisah ke `BaseEditorView` sehingga satu dokumen punya dua layar; biaya sedang.
2. **Tabel baris yang dapat diisi**
   1. `COMPOSE` tabel + field base **(rekomendasi)** — sama dengan baris jurnal; tidak menyentuh `DataTable`.
   2. `EXTEND` `DataTable` agar menerima sel isian — menyentuh komponen shared; risiko regresi di semua modul.
3. **Tab**
   1. `COMPOSE` `BaseButton role="tab"` **(rekomendasi)** — tanpa komponen baru.
   2. `NEW` komponen Tabs bersama — butuh keputusan pengguna; tidak dikerjakan.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` yang mengubah perilaku bawaan base component.


---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka `DataTable` "Mengambil katalog barang/jasa..." dan `SummaryGrid` memuat |
| Kosong | "Belum ada barang/jasa pada saringan ini." + "Atur ulang saringan atau tambahkan barang/jasa baru." |
| Gagal | Pesan backend + "Coba muat ulang halaman." |
| Tanpa hak akses | Butir menu tersembunyi; URL langsung → `AccessDeniedGate` "Akses Ditolak" |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Procurement Management / Master Data / Item

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/items` | Daftar katalog ber-saringan | `ProcurementItem : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/items/summary` | Ringkasan | `ProcurementItem : Read` |
| `POST` | `/v1/corporate/procurement-management/master-data/items` | Tambah barang (kode dari server) | `ProcurementItem : Create` |
| `PUT` | `/v1/corporate/procurement-management/master-data/items/{id}` | Ubah barang | `ProcurementItem : Update` |
| `PATCH` | `/v1/corporate/procurement-management/master-data/items/{id}/status` | Aktif/nonaktif | `ProcurementItem : Update` |
| `DELETE` | `/v1/corporate/procurement-management/master-data/items/{id}` | Hapus (ditolak bila terpakai) | `ProcurementItem : Delete` |
#### Corporate / Procurement Management / Master Data / Item Category

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/item-categories` | Daftar dan saringan | `ProcurementItemCategory : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/item-categories/options` | Pilihan kategori di form barang | `ProcurementItemCategory : Read` |
| `POST/PUT/PATCH/DELETE` | `/v1/corporate/procurement-management/master-data/item-categories[/{id}]` | Kelola kategori | `ProcurementItemCategory : Create/Update/Delete` |
#### Corporate / Procurement Management / Master Data / Fund Source

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/fund-sources` | Daftar | `ProcurementFundSource : Read` |
| `POST/PUT/PATCH/DELETE` | `/v1/corporate/procurement-management/master-data/fund-sources[/{id}]` | Kelola sumber dana | `ProcurementFundSource : Create/Update/Delete` |
#### Corporate / Procurement Management / Master Data / Procurement Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/master-data/procurement-types/options` | Tab jenis (baca-saja) dan pilihan jenis | `ProcurementType : Read` |
#### Health Services / Master Data / Measurement

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/measurements/options` | Pilihan satuan | `Measurement : Read` |


---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000`; panggilan API `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (sumber `sukmagp` berisi BE-PBJ-002..011) dengan database `QuilvianNewDevSukma`. Aktor uji non-SuperAdmin `TEST-PBJ … 20261116110649` (pengusul IGD `reqA`, kepala IGD `headA`, Tim Pengadaan `proc`, Manajer `mgr`, Direktur `dirk`, Keuangan `keu`) memakai bundel izin `permission-audit-matrix.md` §3. Tangkapan layar tersimpan di folder `shots/` scratchpad sesi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada berkas Pengadaan | 0 error, 9 warning `react-hooks/set-state-in-effect` (pola muat-data di efek, sama dengan modul lain) | `PASS` | keluaran perintah, 16 November 2026 |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fe-lint3.log` scratchpad |
| `node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 4800 test, 4749 lulus, 51 gagal — seluruhnya modul lain; 8 berkas gagal yang membaca menu diuji A/B dengan `menu-items.jsx` versi `HEAD` dan hasilnya identik; tidak ada test yang mengimpor berkas Pengadaan | `EXISTING / ENVIRONMENT ISSUE` | baseline 28 Okt 2026 tercatat 41 gagal di modul lain |
| `npm run build` | `Compiled successfully`; 24 rute `/corporate/procurement/**` terbentuk | `PASS` | `fe-build4.log` scratchpad |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: test baru tidak wajib; logika murni berada di procurement-utils.jsx` | `NOT RUN` | — |
| -18 daftar, ringkasan, saringan Nonaktif (`isActive=false` terkirim; 12 → 2 baris) | Sesuai | `PASS` | `ui-results-a.json`, `a-18-items-inactive.png` |
| -18 form Ubah terbuka berisi | Sesuai | `PASS` | `ui-results-a.json` |
| -18 tambah barang → `KTL-000013` dari server | Sesuai | `PASS` | `ui-results-f.json`, `f-18-created.png` (izin `Measurement : Read` diberikan sementara ke jabatan uji lalu dikembalikan) |
| -18 `PBJ-VAL-070` "Barang serupa sudah ada: KTL-000013 …" (409) | Sesuai | `PASS` | `f-18-val070.png` |
| -18 `PBJ-VAL-071` hapus barang terpakai | Pesan backend tampil | `PASS` | `e-18-val071.png` |
| -19 tab Kategori, Sumber Dana, Jenis baca-saja tanpa tombol tambah | Sesuai | `PASS` | `a-19-types.png` |
| Pengguna Keuangan (tanpa `ProcurementItem : Read`): butir Katalog tidak ada; URL langsung "Akses Ditolak" | Sesuai | `PASS` | `ui-results-d.json` |
| Form tambah oleh Tim Pengadaan tanpa `Measurement : Read` | Pesan izin satuan tampil | `PASS` | `ui-results-d.json` |


Uji manual: `PASS`.

**Tidak dijalankan:** tes unit baru (opsional menurut `test-policy.md`); uji `test:uat` tidak dijalankan sesuai larangan repository.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Menu tampil hanya bagi pemegang `ProcurementItem : Read` | Terpenuhi | Uji aktor `keu` (menu tidak ada, URL → Akses Ditolak) vs `proc` |
| (2) Tambah barang menampilkan kode `KTL-…` dari server | Terpenuhi | `KTL-000013` |
| (3) Pesan `PBJ-VAL-070`/`071` tampil terbaca | Terpenuhi | 070: teks matriks; 071: teks backend berbeda dari matriks (lihat catatan) |
| (4) Keadaan kosong/gagal sesuai skema | Terpenuhi | State di §4; teks baku |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 9 warning lint `set-state-in-effect` (pola muat-data); tidak ada error |
| Masalah yang diketahui | `PBJ-VAL-071` backend mengembalikan 400 tanpa `errors.code` dengan teks "Barang/jasa sudah dipakai … Nonaktifkan saja …", sedangkan `validation-matrix.md` menulis 409 "Barang sudah dipakai dan hanya dapat dinonaktifkan." — delta kontrak backend; frontend menampilkan pesan backend apa adanya |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Interupsi | Pemadatan konteks sesi; dilanjutkan dari berkas yang sudah ada tanpa menulis ulang |
| Temuan izin | `Measurement : Read` tidak ada di bundel Tim Pengadaan — tabel lengkap temuan izin ada di laporan `FE-PBJ-003` §8 |
| Langkah berikutnya | Keputusan pemilik soal bundel izin (lihat `FE-PBJ-003` §8) |
| Status Git | `git status --short` (frontend): `M src/utils/menu-sidebar/menu-items.jsx`; `??` `src/app/corporate/procurement/`, `src/components/view/corporate/procurement/`, `src/lib/constants/corporate/procurement/`, `src/lib/hooks/corporate/procurement/`, `src/lib/services/corporate/`, `src/style/corporate/procurement/`, `src/utils/corporate/procurement/`. Tidak ada stage/commit/push. |
