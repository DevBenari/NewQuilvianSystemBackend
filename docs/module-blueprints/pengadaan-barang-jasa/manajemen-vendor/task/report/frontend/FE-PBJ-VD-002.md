# Laporan Perubahan Frontend — `FE-PBJ-VD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-VD-002` |
| Judul | Daftar dan detail vendor |
| Slice | Gelombang 2 roadmap frontend `manajemen-vendor` revisi 1 |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | Layar `FE-PBJ-VD-01`, `-02`, `-03`, `-05`, `-08`; `PBJ-DEC-010`, `015`, `051`, `052`, `053`; `PBJ-ASM-004`; `PBJ-FE-003`, `004` |
| Contract version | `PBJ-VD-API` 1.0 §Vendor + delta D-9 `BE-PBJ-VD-003` (usulan 1.1: rekening tersamarkan dan saringan bank di daftar) |
| Wewenang UI | `PBJ-FE-001` nama menu SOURCE, `PBJ-FE-004` standar layar daftar, `PBJ-FE-003` invariant (rekening tersamarkan, tombol tersembunyi ≠ penjaga server); rupa lain `PBJ-FE-002` `DEV_DISCRETION` |
| Dependency | `FE-PBJ-VD-001` ✅, `BE-PBJ-VD-003` ✅ (diperbarui 18 November 2026 untuk `-05`) |
| Klasifikasi | `MEDIUM` — 5 layar, 3 hook, 1 service, perluasan 2 komponen bersama modul; tanpa state global |
| Task mode | `FRONTEND` (instruksi pengguna "lanjutkan", 18 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b1b360efa` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `a6df5619` (branch `sukmagp`, berisi `BE-PBJ-VD-001`..`008` + delta `BE-PBJ-VD-003`, belum di-commit) |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Butir menu "Data Penyedia/ Vendor" sudah ada sejak `FE-PBJ-VD-001`, tetapi semua rutenya berujung 404. Backend vendor (`BE-PBJ-VD-003`) sudah menyediakan daftar, ringkasan, detail, kategori, dan riwayat.

Satu celah kontrak ditemukan saat merancang layar `-05`:

- Arsitektur frontend bagian 4.6 meminta daftar rekening dengan saringan **Bank**.
- `GET /vendors` tidak membawa rekening maupun saringan bank. Rekening tersamarkan hanya ada di detail satu vendor.

Pilihan ini diajukan ke pemilik, dan pemilik memilih **"Perluas backend"**. Perubahan backend dikerjakan lewat builder backend sebagai pembaruan `BE-PBJ-VD-003`. Rinciannya ada di laporan `BE-PBJ-VD-003` bagian "Pembaruan 18 November 2026".

---

## 2. Proses bisnis dari sisi pengguna

1. **Daftar Vendor (`-01`).** Tim Pengadaan melihat semua supplier Master Data sebagai vendor. Setiap vendor menampilkan status (Aktif, Dibatasi, Diblokir, Nonaktif di Master Data), kategori, keadaan dokumen, dan nilai penilaian terakhir. Ringkasan di atas menghitung vendor per status serta dokumen yang kedaluwarsa atau segera kedaluwarsa. Layar ini tidak punya tombol tambah, karena vendor baru dibuat di Master Data Supplier (`PBJ-DEC-053`). Pemegang `Supplier : Create` melihat keterangan dan tombol "Buka Master Data Supplier".
2. **Detail Vendor (`-02`).** Layar ini punya tiga tab:
   - **Profil & Kategori:** identitas dibaca dari Master Data, ditambah ringkasan dokumen dan penilaian. Tombol **Atur kategori** (`ProcurementVendor : Update`) membuka daftar centang kategori aktif. Kategori nonaktif yang sudah melekat hanya bisa dilepas. Setelah disimpan, lencana kategori, status, dan riwayat langsung berganti tanpa memuat ulang halaman. Bila orang lain mengubah vendor yang sama lebih dulu, layar menahan isian dan menawarkan **Muat ulang**.
   - **Rekening:** nomor rekening tersamarkan, misalnya `****7890`.
   - **Riwayat:** linimasa vendor.
3. **Kategori Vendor (`-03`).** Pilih kategori untuk melihat vendor yang melayaninya, lalu buka vendor untuk mengubah kategorinya.
4. **Rekening Vendor (`-05`).** Daftar rekening tersamarkan, baca saja, dengan saringan Bank. Tombol "Buka" langsung membuka tab Rekening di detail vendor.
5. **Riwayat Vendor (`-08`).** Pilih satu vendor lewat pencarian di server, lalu saring kejadiannya per tanggal, periode, jenis kejadian, atau kata kunci.

Contoh: supplier "Sehat Medika" baru dibuat di Master Data dan belum punya data Pengadaan. Tim Pengadaan membuka detailnya dan mencentang kategori Obat dan Alkes. Profil Pengadaan lahir saat itu juga (`AT-VD-01`). Riwayat lalu menampilkan "Dibuat · — → Aktif · Status blokir awal dari Master Data: Tidak diblokir" dan dua baris "Kategori ditambahkan".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`03-frontend-architecture.md` bagian 2–4.6; `contracts/api-contract.md` §Vendor; `testing/acceptance-test-matrix.md` `AT-VD-01`..`04`; backend `VendorsController.cs`, `VendorDtos.cs`, `ProcurementVendorProfileService.cs`, `DocumentHistoryDtos.cs`, `ProcurementPeriodFilter`. Frontend:

- detail PR: `purchase-request-detail-view.jsx`, `use-purchase-request-detail.jsx`, rute `requests/[slug]`;
- komponen bersama: `procurement-document-tabs.jsx`, `procurement-history-panel.jsx`, `procurement-status-badge.jsx`;
- pemilih item: `use-procurement-options.jsx` dan pola server-side `BaseSelectField` di `need-plan-detail-view.jsx`;
- base component: `filter-select.jsx`, `information-alert.jsx`, `base-checkbox-card.jsx`, `base-form-control.jsx`;
- `plan-consolidation-view.jsx` (daftar centang).

### 3.2 Berkas yang berubah

| Berkas (`V2QuilvianSystemFrontendDev`) | Status | Isi |
| --- | --- | --- |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | Diubah | `VENDOR_STATUS` ditambah `INACTIVE`; Diblokir bernada `danger`; `VENDOR_DOCUMENT_STATE`; `VENDOR_EVALUATION_RECOMMENDATION`; 14 label riwayat vendor di `HISTORY_ACTION_LABELS` |
| `src/utils/corporate/procurement/procurement-utils.jsx` | Diubah | Fungsi murni `resolvePeriodRange` (aturan sama dengan `ProcurementPeriodFilter` backend), `isWithinRange`, `formatEvaluationPeriod` (sama dengan `PeriodLabel` backend), `formatScore` |
| `src/components/view/corporate/procurement/shared/procurement-status-badge.jsx` | Diubah | `kind` baru `vendor` dan `vendorDocument`; bawaan `plan`/`request` tidak berubah |
| `src/components/view/corporate/procurement/shared/procurement-history-panel.jsx` | Diubah | Prop opsional `statusLabel` untuk menerjemahkan kode status; bawaannya menampilkan kode apa adanya seperti sebelumnya |
| `src/lib/services/corporate/procurement/vendor-service.js` | Baru | `filters/metadata`, `summary`, daftar, detail, riwayat, `PUT categories` lewat `InstanceAxios` |
| `src/lib/hooks/corporate/procurement/use-vendor-list.jsx` | Baru | Daftar berhalaman (`useProcurementDocumentList`), ringkasan, pilihan saringan (kategori, status, keadaan dokumen, bank) |
| `src/lib/hooks/corporate/procurement/use-vendor-detail.jsx` | Baru | Detail, riwayat, editor kategori; respons `PUT` dipakai sebagai detail terbaru; `PBJ-VAL-090` → keadaan basi |
| `src/lib/hooks/corporate/procurement/use-vendor-history.jsx` | Baru | Riwayat satu vendor dengan saringan dan halaman di layar; pencarian vendor di server |
| `src/components/view/corporate/procurement/vendors/vendor-list-view.jsx` | Baru | Satu view untuk tiga mode: `list` (`-01`), `categories` (`-03`), `bank` (`-05`) |
| `src/components/view/corporate/procurement/vendors/vendor-detail-view.jsx` | Baru | Layar `-02` |
| `src/components/view/corporate/procurement/vendors/vendor-history-view.jsx` | Baru | Layar `-08` |
| `src/app/corporate/procurement/vendors/page.jsx`, `categories/page.jsx`, `bank-accounts/page.jsx`, `history/page.jsx` | Baru | Entry point + `metadata` |
| `src/app/corporate/procurement/vendors/[slug]/page.jsx` | Baru | Validasi GUID `supplierId` (lain → 404 tanpa memanggil API); `?tab=bank\|history` membuka tab tertentu |

Tidak ada stylesheet baru. Kelas yang dipakai (`card`, `cardTitle`, `cardHeader`, `summaryList`, `unitList`, `flagRow`, `tabs`, `actionRow`, `filterRow`, `muted`) sudah ada di `procurement.module.css`.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `page.jsx` (entry point + `metadata`) → view → hook → service `InstanceAxios`. Konstanta dan fungsi murni tetap di berkas modul. Tidak ada Redux slice baru, sama dengan modul Pengadaan lain.
- Rute detail memakai GUID di URL seperti detail PR, rencana, pagu, dan paket. Ini deviasi modul yang sudah ada dari token rute privat; GUID tidak ditampilkan di layar.
- Kolom tabel didefinisikan di dalam view, seperti layar Pengadaan lain.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman | `REUSE` | `Hero`; tanpa tombol pada `-01`, `-03`, `-05`, `-08`; Kembali + Atur kategori pada detail `-02` |
| Kartu ringkasan | `REUSE` | `SummaryGrid` (`-01`) |
| Baris filter | `REUSE` | `DataFilter` + `periodFilterControls` + `FilterSelect` + `pageSizeControl` |
| Pemilih vendor dengan cari di server (`-08`) | `REUSE` | `FilterSelect` `serverSide`/`onSearchChange`/`onOpen`, pola katalog di `need-plan-detail-view.jsx` |
| Tabel dan paginasi | `REUSE` | `DataTable` + `Pagination` |
| Lencana kategori | `REUSE` | `StatusBadge` |
| Keterangan Master Data, galat + Coba lagi | `REUSE` | `InformationAlert` dengan `children` berisi `BaseButton` |
| Notifikasi | `REUSE` | `ToastStack` lewat `useProcurementAction` |
| Akses ditolak | `REUSE` | `AccessDeniedGate` |
| Status vendor dan keadaan dokumen | `EXTEND` (komponen bersama modul) | `ProcurementStatusBadge` diberi `kind` baru; bawaan tetap |
| Linimasa riwayat di tab detail | `EXTEND` (komponen bersama modul) | `ProcurementHistoryPanel` diberi `statusLabel` opsional; bawaan tetap |
| Tab detail | `COMPOSE` | `BaseButton role="tab"`, pola `procurement-document-tabs.jsx` |
| Identitas, rekening, ringkasan dokumen | `COMPOSE` | `dl.summaryList`, pola `HeaderRead` detail PR |
| Editor kategori | `COMPOSE` | `BaseCheckboxField` per kategori dalam `ul.unitList`, pola `plan-consolidation-view.jsx` |
| Riwayat `-08` | `COMPOSE` | `DataTable` berisi hasil saringan di layar |

Kedua `EXTEND` menyasar komponen milik modul Pengadaan di `procurement/shared/`, bukan base component. Prop baru bersifat opsional dan perilaku bawaannya tidak berubah.

Pilihan untuk elemen bukan `REUSE`:

1. **Lencana status vendor**
   1. `EXTEND` `ProcurementStatusBadge` dengan `kind` baru **(rekomendasi)** — satu komponen untuk semua status Pengadaan; risiko regresi rendah karena `kind` lama tidak disentuh.
   2. Komponen `vendor-status-badge.jsx` terpisah — menduplikasi pemetaan label dan nada.
2. **Tab detail**
   1. `COMPOSE` `BaseButton role="tab"` **(rekomendasi)** — sama dengan tab dokumen PR; tanpa komponen baru.
   2. `NEW` komponen Tabs bersama — butuh keputusan pengguna; tidak dikerjakan.
3. **Editor kategori**
   1. `COMPOSE` daftar centang di kartu Kategori **(rekomendasi)** — semua kategori terlihat sekaligus, sama dengan daftar centang konsolidasi.
   2. `EXTEND` `FilterSelect` menjadi pilih-banyak — mengubah base component yang dipakai seluruh aplikasi; risiko regresi tinggi.
4. **Riwayat `-08`**
   1. `COMPOSE` `DataTable` dengan saringan dan halaman di layar **(rekomendasi)** — baris filter standar `PBJ-FE-004` (`AT-VD-63`) tetap berlaku.
   2. Linimasa `ProcurementHistoryPanel` saja — tanpa saringan dan halaman; tidak memenuhi standar baris filter.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` pada base component.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
| --- | --- |
| Memuat | `DataTable loading` "Mengambil data vendor…" / "Mengambil rekening vendor…" / "Mengambil riwayat vendor…"; detail "Memuat vendor..." |
| Kosong | Daftar "Belum ada vendor pada saringan ini."; `-08` tanpa vendor "Pilih vendor untuk melihat riwayatnya."; kategori "Belum ada kategori."; rekening "Data rekening dikelola Finance dan belum tercatat di Master Data."; riwayat "Belum ada riwayat."; penilaian "Belum ada penilaian final." |
| Gagal | `InformationAlert` "Data vendor gagal dimuat." / "Riwayat vendor gagal dimuat." + tombol **Coba lagi**; detail gagal → pesan + Kembali ke daftar + Coba lagi |
| Tidak ditemukan | Detail 404 → "Vendor tidak ditemukan."; slug bukan GUID → halaman 404 tanpa memanggil API |
| Akses ditolak | `AccessDeniedGate` (403 daftar atau detail) |
| Data basi | 409 `PBJ-VAL-090` saat simpan kategori → peringatan "Data vendor sudah diubah orang lain…" + **Muat ulang**; isian tidak dibuang sampai dimuat ulang |
| Validasi | 422 `PBJ-VAL-100` (kategori nonaktif) → pesan server di layar dan toast. Kategori nonaktif yang belum melekat tidak ditawarkan |
| Pengiriman ganda | Tombol Simpan kategori ber-`loading`; Batal nonaktif saat memproses; `useProcurementAction` menolak aksi tumpang tindih |
| Hanya baca | Tanpa `ProcurementVendor : Update` tombol Atur kategori tidak tampil; server tetap menjaga (`PBJ-FE-003`) |
| Privasi | Nomor rekening hanya tersamarkan dari server; nomor utuh tidak pernah ada di halaman (diuji pada isi HTML) |

---

## 5. Endpoint yang dikonsumsi

Base `api/v1/corporate/procurement-management/vendors`, semua lewat `InstanceAxios` dengan `AbortController`.

| Method dan path | Dipakai di | Hak akses |
| --- | --- | --- |
| `GET /filters/metadata` (`itemCategoryOptions`, `vendorStatusOptions`, `documentStateOptions`, `bankOptions`) | pilihan saringan `-01`, `-03`, `-05`; daftar kategori editor `-02` | `ProcurementVendor : Read` |
| `GET /summary` | ringkasan `-01` | `ProcurementVendor : Read` |
| `GET /` (`search`, `itemCategoryId`, `vendorStatus`, `documentState`, `bankName`, `startDate`, `endDate`, `customPeriod`, `pageNumber`, `pageSize`) | `-01`, `-03`, `-05`, pemilih vendor `-08` | `ProcurementVendor : Read` |
| `GET /{supplierId}` | `-02` | `ProcurementVendor : Read` |
| `GET /{supplierId}/history` | tab Riwayat `-02`, `-08` | `ProcurementVendor : Read` |
| `PUT /{supplierId}/categories` (`itemCategoryIds`, `expectedRowVersion`) | Atur kategori `-02` | `ProcurementVendor : Update` |

### Delta terhadap kontrak dan rancangan

| ID | Delta | Keputusan |
| --- | --- | --- |
| F-1 | `-05` butuh rekening dan saringan bank di daftar | Diselesaikan di backend (D-9 `BE-PBJ-VD-003`, usulan `PBJ-VD-API` 1.1) atas pilihan pemilik "Perluas backend", 18 November 2026 |
| F-2 | `-03` tanpa kartu ringkasan | `GET /summary` tidak menerima saringan kategori; ringkasan global di bawah saringan kategori akan menyesatkan |
| F-3 | `-08` menyaring tanggal, periode, jenis kejadian, kata kunci, dan halaman di layar | Endpoint riwayat mengembalikan seluruh linimasa satu vendor tanpa saringan; aturan periode disamakan dengan backend |
| F-4 | Tab Dokumen, Status, Penilaian belum ada di detail | Sesuai kartu task: menyusul `FE-PBJ-VD-003`..`005` |

---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000` milik pengguna, yang memuat source terkini dari repository yang sama. Panggilan `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (berisi delta `BE-PBJ-VD-003`) dengan database `QuilvianNewDevSukma`. Server milik pengguna tidak dihentikan.

Aktor non-SuperAdmin `TEST-PBJ … FEVD002 20261118140409`:

| Aktor | Izin |
| --- | --- |
| `proc` (Tim Pengadaan) | `ProcurementVendor : Read, Update` + `ProcurementVendorDocument : Read` + `ProcurementVendorEvaluation : Read` |
| `ro` | `ProcurementVendor : Read` |
| `sup` | `ProcurementVendor : Read` + `Supplier : Read, Create` |
| `req` (Unit Peminta) | `ProcurementRequest : Read, Create` |

Data uji: kategori A dan B aktif, C dinonaktifkan; supplier S1 (bank uji, rekening `1234567890`), S2 (tanpa rekening), S3 (bank lain). Skrip uji: `ui_fe_vd002.cjs`, tangkapan layar di `shots-fevd002/` scratchpad sesi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` berkas vendor + komponen bersama | 0 error; 2 warning `react-hooks/set-state-in-effect` (`setLoading` di efek muat, pola sama dengan `use-procurement-document-list`) | `PASS` | keluaran perintah |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fevd002-lint.log` |
| `npm run build` (berdampingan dengan `next dev` milik pengguna, tanpa menghentikannya) | `Compiled successfully in 98s`, exit 0; 32 rute `/corporate/procurement/**` termasuk 5 rute vendor baru (`vendors`, `vendors/[slug]`, `bank-accounts`, `categories`, `history`) | `PASS` | `fevd002-build.log` |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: test baru tidak wajib; fungsi murni baru (resolvePeriodRange, formatEvaluationPeriod) diuji lewat skenario layar UI-11` | `NOT RUN` | — |
| Uji API delta backend `R-01`..`R-10` | 10/10 | `PASS` | laporan `BE-PBJ-VD-003` |
| `UI-01` Menu Tim Pengadaan: 6 butir vendor + Penilaian Vendor | 7/7 tampil | `PASS` | `ui-fevd002.results.run1.json` |
| `UI-02` `-01` hero tanpa tombol, 6 kartu ringkasan, 9 kontrol filter **satu baris**, tanpa tombol tambah, tanpa tautan Master Data (`proc` tanpa `Supplier : Create`) | Sesuai | `PASS` | `01-list.png` |
| `UI-03` `-01` cari "FEVD002" (3 baris) + Keadaan dokumen `documentState=NONE` + Status `vendorStatus=ACTIVE` digabung (3 baris) + Atur ulang mengosongkan | Sesuai | `PASS` | run1 |
| `UI-04` "Buka" S1 → `/vendors/{GUID}`; Identitas (Master Data); lencana "Belum punya data Pengadaan" | Sesuai | `PASS` | `02-profile-new.png` |
| `UI-05` `AT-VD-01` lewat layar: Atur kategori A+B; C nonaktif tidak ditawarkan; `PUT` 200; lencana kategori berganti, lencana profil hilang, toast "Kategori tersimpan"; penanda `window` tetap ada (tanpa muat ulang) | Sesuai | `PASS` | `02-categories-saved.png` |
| `UI-06` Tab Riwayat: "Dibuat · — → Aktif · Status blokir awal …" + "Kategori ditambahkan" ×2 | Sesuai | `PASS` | run1 |
| `UI-07` `AT-VD-03` tab Rekening: `****7890`, "PT Sehat Medika Uji", "Tidak tersedia di Master Data"; `1234567890` tidak ada di HTML halaman | Sesuai | `PASS` | `02-bank.png` |
| `UI-08` Data basi: tab browser kedua melepas B (`PUT` 200); simpan dari tab pertama → 409 → peringatan + Muat ulang → kartu menampilkan A saja | Sesuai | `PASS` | run1 |
| `UI-09` `-03` saringan Kategori A (`itemCategoryId`) → S1 tampil, S2 tidak; filter satu baris, hero tanpa tombol | Sesuai | `PASS` | `03-categories.png` |
| `UI-10` `-05` kolom Bank/Nomor Rekening (`****7890`, `****6655`); saringan Bank (`bankName`) → 1 baris; nomor utuh tidak ada di HTML; "Buka" → `?tab=bank`, tab Rekening aktif | Sesuai | `PASS` | `05-bank.png`, `ui-fevd002.results.json` |
| `UI-11` `-08` kosong sebelum memilih; pilih S1 lewat cari server → 4 kejadian; "Kategori dilepas" → 1; Periode "Bulan lalu" → kosong; Atur ulang mempertahankan vendor (4) | Sesuai | `PASS` | `08-history.png` |
| `UI-12` Tidak ada `pageerror` selama sesi Tim Pengadaan | 0 | `PASS` | run1 |
| `UI-13` Aktor `ro`: detail tanpa "Atur kategori" | Sesuai | `PASS` | run1 |
| `UI-14` Aktor `sup` (`Supplier : Create`): keterangan + "Buka Master Data Supplier" → `/administrator/master-data/supplier` | Sesuai | `PASS` | run1 |
| `UI-15` `AT-VD-04` Unit Peminta: 0 dari 7 butir menu vendor tampil; `/corporate/procurement/vendors` → 403 + "Akses Ditolak" | Sesuai | `PASS` | `01-denied.png` |

Total hasil terakhir per skenario: **15/15 `PASS`**. Pada jalan pertama 14/15 lulus. `UI-10` gagal karena skrip menunggu `bankName` dengan spasi, sedangkan axios mengirim spasi sebagai `+`. Request di layar sudah benar (`bankName=Bank+Uji+FEVD002+140409`, 200). Predikat skrip diperbaiki, lalu `UI-10` diulang dan lulus.

Uji manual: `PASS`.

**Tidak dijalankan:**

- Tes unit baru (opsional).
- `test:uat`, sesuai larangan repository.
- Uji pada lebar layar selain 1440 px.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-VD-01` atur kategori vendor tanpa profil | Terpenuhi | `UI-04`, `UI-05`, `UI-06` (layar); API di laporan `BE-PBJ-VD-003` |
| `AT-VD-03` rekening `****` | Terpenuhi | `UI-07`, `UI-10`; `R-04`, `R-10` |
| `AT-VD-04` Unit Peminta 403, menu 5 dan 10 tidak tampil | Terpenuhi | `UI-15`; `R-09` |
| Atur kategori memperbarui tampilan tanpa muat ulang halaman | Terpenuhi | `UI-05` (penanda `window` bertahan) |
| Cakupan `-01`, `-02` (3 tab), `-03`, `-05`, `-08` | Terpenuhi | `UI-02`..`UI-11` |
| `AT-VD-63` satu baris filter pada layar daftar | Terpenuhi untuk `-01`, `-03`, `-05`, `-08` | `UI-02`, `09`, `10`, `11` |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 warning lint `set-state-in-effect` (pola muat-data) |
| Masalah yang diketahui | Saringan bank memakai nama bank teks bebas dari Master Data; ejaan berbeda untuk bank yang sama tampil sebagai dua pilihan |
| Keputusan pemilik yang dipakai | "Perluas backend (Recommended)" untuk `-05`, 18 November 2026 |
| Pekerjaan terbuka pemilik | Pembaruan dokumen `PBJ-VD-API` ke 1.1 (D-9) bersama `PBJ-VD-INT` 1.1 |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` |
| Data uji | Fixture `TEST-PBJ … FEVD002` run `20261118140409` di `QuilvianNewDevSukma`: 1 unit, 4 jabatan, 4 akun, policy, 3 kategori, 3 supplier, profil vendor S1 + kategori dan riwayatnya. Id di `fevd002.fixture.json` scratchpad. **Sudah dibersihkan 18 November 2026** atas izin pemilik ("lanjutkan" atas tawaran pembersihan): dry run lalu apply `cleanup_vendor_fixture.py` dalam satu transaksi — hapus lunak 3 kategori, 3 supplier (dijaga nama `TEST-PBJ%`), 1 profil vendor + 1 kategori vendor, 10 policy, 4 pegawai, 4 jabatan, 1 unit (kode diberi akhiran `-DEL-20261118140409`); 4 akun dinonaktifkan dan dikunci. Riwayat vendor tidak disentuh |
| Interupsi | Sesi terhenti setelah patch layar `-05`; dilanjutkan dari kondisi terverifikasi (berkas rute dan patch dicek lewat `git status`) tanpa menulis ulang |
| Langkah berikutnya | `FE-PBJ-VD-003` (dokumen legalitas) |
| Status Git | Frontend: `M` `procurement-constants.jsx`, `procurement-utils.jsx`, `procurement-status-badge.jsx`, `procurement-history-panel.jsx`; `??` `src/app/corporate/procurement/vendors/`, `src/components/view/corporate/procurement/vendors/`, `use-vendor-list.jsx`, `use-vendor-detail.jsx`, `use-vendor-history.jsx`, `vendor-service.js` (ditambah berkas `FE-PBJ-VD-001` yang juga belum di-commit). Backend: `VendorDtos.cs`, `ProcurementVendorProfileService.cs`, laporan. Tidak ada stage/commit/push |
