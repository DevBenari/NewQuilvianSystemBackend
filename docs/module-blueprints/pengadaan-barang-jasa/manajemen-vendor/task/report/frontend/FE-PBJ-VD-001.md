# Laporan Perubahan Frontend — `FE-PBJ-VD-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-PBJ-VD-001` |
| Judul | Menu dan Pengaturan vendor |
| Slice | Gelombang 1 roadmap frontend `manajemen-vendor` revisi 1 |
| Roadmap | `roadmap/frontend-roadmap.md` (tabel task dan kartu task) |
| Trace | Layar `FE-PBJ-VD-11`, `-12`, `-13`; menu 5 dan 10; `PBJ-FE-001`, `PBJ-FE-004`; `PBJ-DEC-050`, `061`; `PBJ-OQ-033` |
| Contract version | Set `1.0` (`PBJ-VD-API` §Master Data, `-PERM`, `-STATE`, `-VAL`); `03-frontend-architecture.md` bagian 2–4.6 |
| Wewenang UI | `PBJ-FE-001` nama menu SOURCE, `PBJ-FE-004` standar layar daftar (approved), `PBJ-FE-003` invariant, rupa lain `PBJ-FE-002` `DEV_DISCRETION`; nama submenu menu 10 menunggu `PBJ-OQ-033` |
| Dependency | `BE-PBJ-VD-002` ✅ (delta D-1..D-8 di laporan backend) |
| Klasifikasi | `MEDIUM` — 3 layar, 1 hook, 1 service, konstanta, menu; tanpa state global |
| Task mode | `FRONTEND` (instruksi pengguna "lanjutkan", 18 November 2026) |
| Target tulis | `V2QuilvianSystemFrontendDev` `src/**`; laporan dan status roadmap di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `b1b360efa` (branch `sukmagpV2`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `a6df5619` (branch `sukmagp`, berisi `BE-PBJ-VD-001`..`008` yang belum di-commit) |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terbukti di layar |

---

## 1. Keadaan yang ditemukan di awal

Backend vendor sudah lengkap (`BE-PBJ-VD-001`..`008` ✅), tetapi frontend belum punya satu pun berkas vendor:

- Sidebar "Pengadaan Barang & Jasa" hanya memuat grup Dashboard, Rencana, Permintaan, Persetujuan & Laporan, dan Pengaturan (Katalog, Kategori & Sumber Dana).
- Belum ada service yang memanggil `master-data/vendor-document-types`, `category-document-requirements`, atau `evaluation-criteria`.
- Petugas pengadaan tidak bisa mengatur jenis dokumen legalitas, dokumen wajib per kategori, maupun bobot kriteria penilaian kecuali lewat API langsung.

---

## 2. Proses bisnis dari sisi pengguna

Ketiga layar ini adalah pengaturan yang harus beres sebelum vendor dikelola dan dinilai.

1. **Jenis Dokumen Legalitas (`-11`).** Tim Pengadaan mencatat jenis dokumen yang dikenal rumah sakit, misalnya NIB atau Izin Pedagang Besar Farmasi, dan menandai jenis yang wajib punya tanggal berlaku. Jenis yang masih diwajibkan sebuah kategori tidak bisa dinonaktifkan; server menolak dengan 409 dan pesan "Jenis dokumen … masih diwajibkan untuk kategori …".
2. **Dokumen Wajib per Kategori (`-12`).** Daftar ini dimulai kosong (`PBJ-DEC-061`). Pemilik kebijakan menambah pasangan kategori × jenis dokumen, lalu menekan **Berlakukan** saat SOP sudah menetapkannya. Sejak itu vendor kategori tersebut tanpa dokumen terverifikasi jenis itu menjadi tidak layak dipilih. Kewajiban yang pernah berlaku tidak bisa diubah pasangannya atau dihapus, hanya dinonaktifkan.
3. **Kriteria Penilaian (`-13`).** Tim Pengadaan menyusun kriteria beserta bobotnya per kategori. Kartu "Total Bobot Aktif per Kategori" menunjukkan jumlah bobot kriteria aktif tiap kategori. Contohnya kategori Alat Kesehatan dengan Kualitas 60% saja tampil "60% · Belum 100% (kurang 40%)". Sebuah peringatan juga menyebut kategori itu, karena penilaian vendor untuk kategori tersebut ditolak server sampai bobotnya tepat 100%.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`menu-items.jsx`; `procurement-item-view.jsx` dan `procurement-reference-view.jsx` (rujukan visual Pengaturan Pengadaan); `use-procurement-master-data.jsx`, `use-procurement-document-list.jsx`, `use-procurement-action.jsx`; `procurement-master-data-service.js`; `procurement-list-filter.jsx`; `procurement-constants.jsx`; `procurement-utils.jsx`; base component `base-form-control.jsx`, `base-checkbox-card.jsx`, `summary-grid.jsx`, `information-alert.jsx`, `data-filter.jsx`; filter menu `left-sidebar-items-virtualized.jsx`, `filter-menu-items-by-permission.jsx`; backend `ProcurementVendorMasterDtos.cs`, tiga controller dan service master vendor.

### 3.2 Berkas yang berubah

| Berkas (`V2QuilvianSystemFrontendDev`) | Status | Isi |
| --- | --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah | Grup "Data Penyedia/ Vendor" (6 butir) dan "Evaluasi Penyedia" (1 butir "Penilaian Vendor", label sementara `PBJ-OQ-033`) disisipkan sebelum "Persetujuan & Laporan Pengadaan"; 3 butir baru di "Pengaturan Pengadaan". Setiap butir ber-`requiredPermission` `Read`; ikon memakai impor yang sudah ada |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | Diubah | Endpoint vendor (7), route vendor (10), izin vendor (7), peta status `VENDOR_STATUS`, `VENDOR_DOCUMENT_STATUS`, `VENDOR_STATUS_REQUEST_STATUS`, `VENDOR_EVALUATION_STATUS` (dipakai task berikutnya) |
| `src/lib/services/corporate/procurement/vendor-master-data-service.js` | Baru | Sembilan fungsi endpoint master (`filters/metadata`, `summary`, daftar, `options`, detail, `POST`, `PUT`, `PATCH status`, `DELETE`) untuk tiga jenis master, lewat `InstanceAxios` |
| `src/lib/hooks/corporate/procurement/use-vendor-master-data.jsx` | Baru | Controller layar: daftar berhalaman, ringkasan mengikuti saringan kategori, pilihan kategori/jenis dari metadata, editor, validasi bobot, simpan/ubah status/hapus |
| `src/components/view/corporate/procurement/settings/vendor-document-type-view.jsx` | Baru | Layar `-11` |
| `src/components/view/corporate/procurement/settings/category-document-requirement-view.jsx` | Baru | Layar `-12` |
| `src/components/view/corporate/procurement/settings/evaluation-criterion-view.jsx` | Baru | Layar `-13` beserta kartu total bobot per kategori |
| `src/app/corporate/procurement/settings/vendor-document-types/page.jsx` | Baru | Entry point dan `metadata` |
| `src/app/corporate/procurement/settings/category-document-requirements/page.jsx` | Baru | Entry point dan `metadata` |
| `src/app/corporate/procurement/settings/evaluation-criteria/page.jsx` | Baru | Entry point dan `metadata` |

Tidak ada stylesheet baru; seluruh kelas berasal dari `procurement.module.css` dan `base-data-components.module.css`.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi sama dengan modul Pengadaan lain: `page.jsx` (entry point + `metadata`) → view → hook → service `InstanceAxios`. Konstanta ada di `procurement-constants.jsx`, sesuai `03-frontend-architecture.md` bagian 1.
- Tidak ada Redux slice baru.
- **Deviasi dari standar master data** (`<FEATURE>_CONFIG`, sembilan thunk Redux, halaman detail, token route privat) dipertahankan dengan sengaja:
  - Kartu task mewajibkan "standar master Pengaturan Pengadaan".
  - Deviasi yang sama sudah dicatat di `FE-PBJ-001`.
  - Ketiga layar tampil berdampingan dengan Katalog dan Kategori & Sumber Dana, yang memakai editor sebaris di atas daftar.
  - Hook `useVendorMasterData(kind)` melayani tiga jenis master, mengikuti `useProcurementMasterData(kind)` yang sudah ada. Ini bukan factory master-data generik lintas modul.
- Definisi kolom tabel ditulis di dalam view, sama dengan layar Pengaturan Pengadaan lain.

### Keputusan base component (gerbang keputusan)

| Elemen layar | Status | Komponen / bukti |
| --- | --- | --- |
| Judul halaman | `REUSE` | `Hero` tanpa tombol (`PBJ-FE-004`) |
| Kartu ringkasan angka | `REUSE` | `SummaryGrid` (`total`, `active`, `inactive`, plus satu angka khas per layar) |
| Saringan, pencarian, atur ulang, jumlah baris | `REUSE` | `DataFilter` + `periodFilterControls` + `FilterSelect` + `pageSizeControl` (`procurement-list-filter.jsx`) |
| Tombol tambah di baris filter | `REUSE` | `BaseButton variant="primary"` di `actions` `DataFilter` |
| Tabel dan paginasi | `REUSE` | `DataTable` + `Pagination` lewat `PaginationComponent` |
| Label status | `REUSE` | `StatusBadge` (label "Aktif"/"Nonaktif"; "Berlaku"/"Nonaktif" untuk kewajiban) |
| Konfirmasi hapus | `REUSE` | `ConfirmModal` |
| Notifikasi hasil aksi | `REUSE` | `ToastStack` lewat `useProcurementAction` |
| Galat dan akses ditolak | `REUSE` | `InformationAlert`, `AccessDeniedGate` |
| Isian form | `REUSE` | `BaseTextField`, `BaseInputField type="number"`, `BaseTextAreaField`, `BaseSelectField`, `BaseCheckboxField` |
| Editor sebaris di atas daftar | `COMPOSE` | `section.card` + `cardTitle` + `formGrid` + `actionRow` — sama persis dengan `procurement-item-view.jsx` |
| Kartu "Total Bobot Aktif per Kategori" | `COMPOSE` | `section.card` + `InformationAlert variant="warning"` + `SummaryGrid` satu kartu per kategori |
| Sel nama + keterangan (`-11`) | `COMPOSE` | Nama di baris pertama, keterangan sebagai `styles.muted` di bawahnya (kelas yang sudah ada) |

Pilihan untuk elemen bukan `REUSE`:

1. **Editor sebaris**
   1. `COMPOSE` kartu editor di atas daftar seperti Katalog **(rekomendasi)** — konsisten dengan dua layar Pengaturan Pengadaan yang sudah ada; tanpa komponen atau CSS baru; risiko regresi nol.
   2. Halaman editor terpisah `BaseEditorView` mengikuti standar master data — lebih seragam dengan modul HR generasi baru, tetapi tiga layar Pengaturan Pengadaan jadi berbeda pola satu sama lain; biaya sedang.
2. **Total bobot per kategori**
   1. `COMPOSE` kartu + `SummaryGrid` per kategori + peringatan **(rekomendasi)** — semua kategori terbaca sekaligus tanpa menyaring; memakai base component saja.
   2. Kolom "Total bobot kategori" di tabel — angka yang sama terulang di setiap baris kategori itu dan kategori tanpa kriteria tidak tampil; biaya rendah tetapi sulit dibaca.
   3. `NEW` grafik batang bobot — butuh keputusan pengguna; tidak dikerjakan.
3. **Keterangan jenis dokumen**
   1. `COMPOSE` keterangan di bawah nama **(rekomendasi)** — kolom Aksi tetap satu baris pada lebar 1440 px.
   2. Kolom Keterangan tersendiri — dicoba lebih dulu; pada 1440 px tombol Hapus terpotong dan tabel bergulir menyamping karena kode `IZIN_DISTRIBUSI_ALKES` melebarkan kolom Kode.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` yang mengubah perilaku bawaan base component.

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
| --- | --- |
| Memuat | `DataTable loading` dengan teks "Mengambil …"; `SummaryGrid loading` sebelum ringkasan pertama tiba |
| Kosong | `-11` "Belum ada jenis dokumen pada saringan ini."; `-12` "Belum ada dokumen wajib pada saringan ini." + "Belum ada kategori yang mewajibkan dokumen. Tambahkan kewajiban bila SOP sudah menetapkannya."; `-13` "Belum ada kriteria pada saringan ini." Kartu bobot per kategori tidak tampil bila belum ada kriteria sama sekali |
| Gagal muat | `InformationAlert` danger berisi pesan server + "Coba muat ulang halaman." |
| Akses ditolak | `AccessDeniedGate` "Ups! Akses Ditolak" (403 dari daftar) |
| Isian wajib kosong | "Lengkapi isian bertanda bintang." tanpa request |
| Bobot salah | "Bobot harus lebih dari 0 dan paling tinggi 100." / "Bobot paling banyak dua angka di belakang koma." tanpa request; teks sama dengan pesan server `PBJ-VAL-113` |
| Tolakan server (409/400/422) | Toast "Tindakan gagal" berisi pesan server; pada simpan, pesan juga tampil di kartu editor |
| Pengiriman ganda | Tombol Simpan ber-`loading`; tombol ubah status nonaktif selama aksi berjalan; `useProcurementAction` menolak aksi kedua yang tumpang tindih |
| Hanya baca | Tanpa izin `Create`/`Update`/`Delete`, tombol tambah, Ubah, ubah status, dan Hapus tidak tampil; server tetap menjaga (`PBJ-FE-003`) |
| Kewajiban yang pernah berlaku | Tombol Ubah dan Hapus disembunyikan; tersisa Nonaktifkan/Berlakukan |

---

## 5. Endpoint yang dikonsumsi

Base `api/v1/corporate/procurement-management/master-data`. Semua lewat `InstanceAxios`, dengan `AbortController` pada setiap muat.

| Grup `[Tags]` | Method dan path | Dipakai di | Hak akses |
| --- | --- | --- | --- |
| Jenis dokumen legalitas | `GET /vendor-document-types` (`search`, `isActive`, `requiresExpiryDate`, `startDate`, `endDate`, `customPeriod`, `pageNumber`, `pageSize`) | daftar `-11` | `ProcurementVendorDocumentType : Read` |
| | `GET /vendor-document-types/summary` | ringkasan `-11` | `… : Read` |
| | `POST /vendor-document-types`, `PUT /{id}` (`code`, `name`, `requiresExpiryDate`, `description`, `isActive`) | editor `-11` | `… : Create` / `Update` |
| | `PATCH /{id}/status`, `DELETE /{id}` | aksi baris `-11` | `… : Update` / `Delete` |
| Dokumen wajib per kategori | `GET /category-document-requirements/filters/metadata` | pilihan kategori dan jenis dokumen `-12` | `ProcurementCategoryDocumentRequirement : Read` |
| | `GET /category-document-requirements` (`itemCategoryId`, `documentTypeId`, saringan baku) | daftar `-12` | `… : Read` |
| | `GET /category-document-requirements/summary?itemCategoryId=` | ringkasan `-12` | `… : Read` |
| | `POST`, `PUT /{id}` (`itemCategoryId`, `documentTypeId`, `isActive`), `PATCH /{id}/status`, `DELETE /{id}` | editor dan aksi baris `-12` | `… : Create` / `Update` / `Delete` |
| Kriteria penilaian | `GET /evaluation-criteria/filters/metadata` | pilihan kategori `-13` | `ProcurementEvaluationCriterion : Read` |
| | `GET /evaluation-criteria` (`itemCategoryId`, saringan baku) | daftar `-13` | `… : Read` |
| | `GET /evaluation-criteria/summary?itemCategoryId=` (`categories[].activeWeightTotal`, `isComplete`, `activeWeightTotal`) | ringkasan dan kartu bobot `-13` | `… : Read` |
| | `POST`, `PUT /{id}` (`itemCategoryId`, `name`, `weightPercent`, `description`, `isActive`), `PATCH /{id}/status`, `DELETE /{id}` | editor dan aksi baris `-13` | `… : Create` / `Update` / `Delete` |

`GET /options` dan `GET /{id}` tersedia di service, tetapi tidak dipanggil ketiga layar ini. Pilihan diambil dari `filters/metadata`, dan editor diisi dari baris daftar.

### Delta terhadap kontrak dan standar

| ID | Delta | Keputusan |
| --- | --- | --- |
| F-1 | Kewajiban baru (`-12`) dikirim `isActive=false` kecuali kotak "Langsung berlakukan" dicentang | Mengikuti bawaan server (`IsActive` tanpa default = `false`) dan `PBJ-DEC-061`: kewajiban berlaku hanya atas tindakan sadar |
| F-2 | Ubah/Hapus disembunyikan untuk kewajiban yang pernah berlaku (`activatedAt` terisi) | Server menolak keduanya (409 ganti pasangan, 400 `PBJ-VAL-071` hapus); tombol disembunyikan agar tidak memancing galat |
| F-3 | Jenis dokumen memakai kode isian pengguna | Kontrak `UpsertProcurementVendorDocumentTypeRequest.Code` wajib; berbeda dari butir 7 standar master data (kode dari server) dan sama dengan Kategori Barang |
| F-4 | Butir 5.1–5.6 dan "Penilaian Vendor" sudah tampil, tetapi rutenya baru dibuat `FE-PBJ-VD-002`..`005`; sampai saat itu tautan berujung 404 | Sesuai cakupan kartu task (menu 5 dan 10 lengkap di task ini) |
| F-5 | Label butir 5.1 "Daftar vendor" (huruf kecil) | Sama persis dengan SOURCE `PBJ-FE-001` di `02-module-map.md` |

---

## 6. Verifikasi

Uji layar memakai Playwright terhadap `next dev` `localhost:3000` yang dijalankan dari repository frontend. Panggilan ke `https://localhost:7184` dialihkan ke build backend scratch `https://localhost:7185` (`BE-PBJ-VD-001`..`008`) dengan database `QuilvianNewDevSukma`.

Aktor uji non-SuperAdmin dibuat lewat API dengan unit `TEST-PBJ Pengadaan Uji FEVD001 <run>`:

- `adm`: CRUD tiga master vendor + `Read` `ProcurementVendor`, `ProcurementVendorDocument`, `ProcurementVendorEvaluation`.
- `ro`: hanya `ProcurementVendorDocumentType : Read`.
- `none`: hanya `ProcurementItem : Read`.

Ada juga dua kategori uji A dan B. Skrip uji adalah `ui_fe_vd001.cjs`, dengan tangkapan layar di `shots-fevd001/` scratchpad sesi.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 10 berkas yang diubah | 0 error, 0 warning | `PASS` | `fevd001-eslint-files.log` |
| `npm run lint:errors` (seluruh repo) | exit 0 | `PASS` | `fevd001-lint2.log` |
| `npm run build` (kode final, setelah server uji dihentikan) | `Compiled successfully in 78s`, exit 0; 27 rute `/corporate/procurement/**` termasuk 3 rute baru `settings/vendor-document-types`, `category-document-requirements`, `evaluation-criteria` | `PASS` | `fevd001-build2.log` |
| `AUTOMATED TEST` | `SKIPPED (opsional) — test-policy: test baru tidak wajib; tidak ada logika murni baru di luar hook layar` | `NOT RUN` | — |
| `UI-01` Menu aktor penuh: 6 butir vendor, "Penilaian Vendor", 3 butir Pengaturan tampil (pencarian sidebar, tautan per `href`) | Sesuai | `PASS` | `ui-fevd001.results.run3c.json` |
| `UI-02` `-11` hero, ringkasan 4 kartu, kontrol filter, "+ Tambah Jenis" | Sesuai | `PASS` | `11-list.png` |
| `UI-03` Simpan kosong → "Lengkapi isian bertanda bintang." tanpa `POST` | Sesuai | `PASS` | `run3.json` |
| `UI-04` Tambah jenis wajib tanggal berlaku → 200, baris "Wajib … Aktif", ringkasan dimuat ulang | Sesuai | `PASS` | `run3.json` |
| `UI-05` Saringan tanggal berlaku: "Tanpa" (`requiresExpiryDate=false`) menyembunyikan baris uji, "Wajib" (`=true`) menampilkannya | Sesuai | `PASS` | `run3.json` |
| `UI-06` Periode "Bulan ini" (`customPeriod=thismonth`), "10 baris" (`pageSize=10`), Status Aktif (`isActive=true`) bersamaan; Atur ulang mengosongkan pencarian dan saringan | Sesuai | `PASS` | `run3.json` |
| `UI-07` Ubah nama (`PUT` 200), Nonaktifkan lalu Aktifkan (`PATCH` 200 ×2), sel status berganti | Sesuai | `PASS` | `run3.json` |
| `UI-08` Hapus jenis belum terpakai lewat `ConfirmModal` → `DELETE` 200, baris hilang | Sesuai | `PASS` | `run3.json` |
| `UI-09` `-12` tambah kewajiban tanpa centang → "Belum pernah berlaku", Nonaktif, tombol Ubah/Berlakukan/Hapus | Sesuai | `PASS` | `12-draft.png` |
| `UI-10` Berlakukan → "Berlaku", "Mulai Berlaku 18 Nov 2026, 10.32 · TEST-PBJ adm FEVD001", tersisa tombol Nonaktifkan | Sesuai | `PASS` | `run3.json` |
| `UI-11` Saringan Kategori → `itemCategoryId` terkirim ke daftar **dan** `summary`; saringan Jenis dokumen → `documentTypeId`; hasil 1 baris | Sesuai | `PASS` | `12-filtered.png` |
| `UI-12` `-11` Nonaktifkan jenis yang diwajibkan ("1 kategori") → server 409, toast "masih diwajibkan", status tetap Aktif | Sesuai | `PASS` | `run3.json` |
| `UI-13` `-13` bobot `0` dan `60.555` ditolak di layar tanpa `POST` | Sesuai | `PASS` | `run3.json` |
| `UI-14` Kriteria 60% kategori A → kartu A "60% · Belum 100% (kurang 40%)" dan peringatan menyebut kategori A | Sesuai | `PASS` | `13-incomplete.png` |
| `UI-15` Tambah 40% kategori A → kartu A "100% · 2 kriteria aktif · Lengkap", peringatan tidak lagi menyebut A; saringan A → kartu "Total Bobot Aktif" dari `summary?itemCategoryId=` | Sesuai | `PASS` | `13-filtered-complete.png` |
| `UI-16` Kategori B 50% → peringatan menyebut B (50%); Nonaktifkan 40% A → peringatan menyebut A (60%) | Sesuai | `PASS` | `13-two-warnings.png`, `run3b.json` |
| `UI-17` Hapus kriteria B lewat `ConfirmModal` → `DELETE` 200 | Sesuai | `PASS` | `run3b.json` |
| `UI-18` Tidak ada `pageerror` selama sesi aktor penuh | 0 | `PASS` | `run3b.json` |
| `UI-19` Menu aktor `ro`: dari 10 butir vendor hanya "Jenis Dokumen Legalitas" | Sesuai | `PASS` | `run3.json` |
| `UI-20` `-11` aktor `ro`: daftar tampil tanpa "+ Tambah Jenis", Ubah, Hapus | Sesuai | `PASS` | `11-readonly.png` |
| `UI-21` `-12` lewat URL langsung oleh `ro` → 403, "Akses Ditolak" | Sesuai | `PASS` | `run3.json` |
| `UI-22` Menu aktor `none`: 10 butir vendor tersembunyi, "Katalog Barang/Jasa" tetap tampil | Sesuai | `PASS` | `run3.json` |
| `UI-23` `-13` lewat URL langsung oleh `none` → "Akses Ditolak" | Sesuai | `PASS` | `13-denied.png` |
| `UI-24` `AT-VD-63` pada `-11`, `-12`, `-13`: 8/9/8 kontrol filter dalam **satu** baris, hero tanpa tombol, tombol tambah `primary` | Sesuai | `PASS` | `ui-fevd001.results.run3d.json` |

Total hasil terakhir per skenario: **24/24 `PASS`**.

Riwayat jalan uji, ditulis apa adanya:

| Jalan | Hasil | Penyebab gagal | Tindakan |
| --- | --- | --- | --- |
| 1 | 3/9, berhenti di `UI-10` | Skrip: regex "Periode" (tombol berlabel "Semua periode"), `check()` pada input kartu yang tersembunyi, promise `waitForResponse` tak tertangani | Skrip diperbaiki |
| 2 | 19/23 | Skrip: pencarian mengetik ke kotak "Cari menu..." sidebar; teks tombol "Nonaktifkan" terbaca sebagai status "Nonaktif"; judul kartu `SummaryGrid` tampil kapital | Skrip diperbaiki. Temuan layar: kolom Aksi `-11` terpotong dan tombol `-13` terbungkus dua baris pada 1440 px, jadi lebar kolom diatur ulang dan keterangan `-11` dipindah ke bawah nama |
| 3 (fixture baru) | 22/23 | `UI-16`: baris sisa jalan 2 bernama sama ikut cocok pencarian | Baris dicakup ke kategori run; `UI-16`..`18` diulang 3/3 |
| 3c, 3d | 2/2, 1/1 | — | `UI-01`, `UI-02` diulang setelah perubahan kolom `-11`; `UI-24` ditambahkan untuk `AT-VD-63` |

Uji manual: `PASS`.

**Tidak dijalankan:**

- Tes unit baru (opsional menurut `test-policy.md`).
- `test:uat`, sesuai larangan repository.
- Uji pada lebar layar selain 1440 px.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Butir menu tampil hanya bagi pemegang izin baca | Terpenuhi | `UI-01` (aktor penuh: 10/10), `UI-19` (`ro`: 1/10), `UI-22` (`none`: 0/10) |
| (2) Master CRUD dengan baris filter `PBJ-FE-004` | Terpenuhi | `UI-02`..`12`, `UI-13`..`17`; `UI-24` satu baris filter |
| (3) `-13` menampilkan total bobot aktif per kategori dan peringatan bila ≠ 100% | Terpenuhi | `UI-14`..`16` |
| `AT-VD-63` | Terpenuhi untuk `-11`..`-13` | `UI-24` |
| DoD: laporan ini | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Butir 5.1–5.6 dan "Penilaian Vendor" berujung 404 sampai `FE-PBJ-VD-002`..`005` selesai (F-4). Baris filter `-12` memuat 9 kontrol sehingga label FilterSelect terpotong ("Semua k…"); tetap satu baris sesuai `PBJ-FE-004` |
| Temuan lain | `menu-items.jsx` sudah memuat key ganda `healthServicesDoctorQueue` sebelum task ini (bukan dari perubahan ini; tidak disentuh) |
| Dependency backend | Tidak ada yang tertunda |
| Perubahan sampingan | `NONE` — `git status --short` frontend hanya memuat 10 entri milik task ini |
| Data uji | Dua fixture `TEST-PBJ … FEVD001` di `QuilvianNewDevSukma` (run `20261118102353` dan `20261118103205`): 1 unit, 3 jabatan, 3 akun, policy, 2 kategori, jenis dokumen, kewajiban, dan kriteria uji per run. Id tercatat di `fevd001.fixture.run1.json` dan `fevd001.fixture.json` scratchpad. **Dibersihkan 18 November 2026** atas izin pemilik ("lanjutkan"): hapus lunak per run 2 kriteria, 2 kategori, 1 kewajiban, 1 jenis dokumen, 17 policy, 3 pegawai, 3 jabatan, 1 unit (kode diberi akhiran `-DEL-<run>`), 3 akun dinonaktifkan + lockout; baris yang sudah dihapus lewat layar tidak terhitung ulang. Sesudahnya master vendor kembali ke data awal: 5 jenis dokumen, 0 kewajiban, 3 kriteria kategori Umum |
| Interupsi | Pemadatan konteks sesi; dilanjutkan dari survei yang sudah ada tanpa menulis ulang berkas |
| Langkah berikutnya | `FE-PBJ-VD-002` (daftar dan detail vendor); `PBJ-OQ-033` untuk nama submenu menu 10 |
| Status Git | `git status --short` (frontend): `M src/lib/constants/corporate/procurement/procurement-constants.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`; `??` tiga folder rute `settings/`, tiga view `settings/`, `use-vendor-master-data.jsx`, `vendor-master-data-service.js`. Tidak ada stage/commit/push |
