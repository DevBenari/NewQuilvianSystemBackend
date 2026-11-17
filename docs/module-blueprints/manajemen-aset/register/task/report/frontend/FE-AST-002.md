# Laporan Perubahan Frontend — `FE-AST-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-002` |
| Judul | Delapan layar master |
| Slice | `REG-FE-S1` — Fondasi dan pencatatan |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-002` |
| Trace | `CAP-02.*`, `CAP-EXT-05`; `SCR-AST-REG-02..09`; `03-frontend-architecture.md` §3.1, §6 |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `2093cb9a`, termasuk delta `filters/metadata`, `summary`, `DELETE`, dan paging `pageNumber` yang dicatat `BE-AST-006` §3.3 |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-001` (dikerjakan bersamaan), `BE-AST-006` ✅ |
| Klasifikasi | `HEAVY` — 8 layar (6 master CRUD, 1 daftar baca-saja, 1 pengaturan), 37 berkas baru, 3 berkas diubah |
| Task mode | `FRONTEND`, diminta Rizki 16 November 2026 |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`), plus laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `605d26dc3` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `2093cb9a` (branch `rizkiG`) |
| Tanggal | 16 November 2026 |
| Status | ✅ SELESAI 16 November 2026 — keempat kriteria terpetakan ke source, lulus uji layar, dan build pemilik berhasil |

---

## 1. Keadaan yang ditemukan di awal

- Tidak ada satu pun layar Aset di frontend.
- Backend menyediakan, untuk tiap master koleksi, sembilan endpoint baseline
  (`filters/metadata`, `summary`, daftar, `options`, rincian, tambah, ubah, `PATCH status`, `DELETE`),
  ditambah `PUT categories/{id}/document-requirements`, `GET groups/inventory-codes`, dan
  `GET`/`PUT settings`.
- Galat backend dibungkus `ApiResponse` dengan `errors.code`: `AST-VAL-REG-050` kode ganda,
  `051` format kode inventaris, `052` kode cadangan, `053` kode terkunci, `055` unit lokasi,
  `056` rentang pengaturan, `AST-MST-FIELD`/`AST-MST-REFERENCE` isian wajib/rujukan,
  `AST-ENTITY-NOT-CONFIGURED` (503) badan hukum belum diatur.
- Data contoh seeder sudah ada di basis data dev: 7 kategori, 7 kelompok, 4 jenis kepemilikan,
  1 lokasi, 2 klasifikasi, 7 jenis dokumen.

---

## 2. Proses bisnis dari sisi pengguna

**Daftar (tujuh layar: Kategori, Kelompok, Kode Inventaris, Jenis Kepemilikan, Lokasi, Klasifikasi, Jenis Dokumen)**

1. Petugas membuka butir menu. Layar menampilkan judul, kartu ringkasan (misalnya Kelompok:
   Total 7, Aktif 7, Nonaktif 0, Medis 1, Kode Terkunci 0), kotak saringan, lalu tabel.
2. Petugas dapat mencari kode/nama, menyaring status Aktif/Nonaktif, saringan khusus layar
   (contoh: Kategori → Penyusutan; Kelompok → Kategori, Klasifikasi, Jenis; Lokasi → Unit), dan
   mengubah jumlah baris. Tombol putar-ulang mengembalikan semua saringan.
3. Klik dua kali sebuah baris membuka rincian. Tombol ⋮ di kolom Aksi memberi pilihan
   Lihat detail, Perbarui, dan Nonaktifkan/Aktifkan (dua terakhir hanya bagi pemegang hak `Update`).
4. Layar Kode Inventaris bersifat baca-saja: menampilkan nomor aset terakhir yang sudah terbit
   (contoh `AST-ALKMED-000123`) dan membuka rincian **Kelompok** saat baris diklik dua kali.

**Rincian** — menampilkan seluruh isian, status, dan jejak waktu, dengan tombol **Kembali**,
**Perbarui**, **Hapus** saja. Hapus meminta konfirmasi; bila data sudah dipakai, pesan backend
("…tidak dapat dihapus karena sudah dipakai…") tampil sebagai notifikasi dan modal tetap terbuka.
Pada rincian **Kategori** ada bagian **Dokumen Wajib**: daftar jenis dokumen aktif berupa kartu
centang dan tombol **Simpan Dokumen Wajib** yang baru aktif setelah ada perubahan.

**Tambah/Perbarui** — formulir satu kolom; isian bertanda bintang wajib. Contoh aturan:
- kode diketik huruf kecil otomatis menjadi huruf besar (`pny` → `PNY`);
- formulir kosong tidak dikirim; tiap isian wajib diberi pesan "… wajib diisi.";
- kode yang sudah dipakai menampilkan "Kode ini sudah dipakai data lain." **di bawah isian Kode**;
- kode inventaris `A` menampilkan "Kode inventaris harus 2–11 huruf besar atau angka tanpa spasi.",
  kode `PNY` menampilkan "Kode ini dipakai untuk nomor dokumen aset. Pilih kode lain." — keduanya di
  bawah isian Kode Inventaris;
- kelompok yang kodenya sudah dipakai nomor aset menampilkan isian Kode Inventaris terkunci beserta
  alasannya "Kode inventaris tidak dapat diubah karena sudah dipakai pada nomor aset.";
- setelah simpan berhasil, pengguna diarahkan ke rincian data tersebut.

**Pengaturan Aset** — satu formulir untuk badan hukum aktif (contoh: "Berlaku untuk PT Metropolitan
Medical Centre · Metropolitan Medical Centre Hospital. Terakhir diubah …"). Isian angka dikunci pada
rentangnya (Ambang menganggur 1–3650 hari, Peringatan dokumen 1–365 hari, Batas pemanfaatan 1–120
bulan). **Batalkan Perubahan** mengembalikan nilai tersimpan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Sama dengan `FE-AST-001` §3.1, ditambah controller dan service `MasterData` backend,
`AssetMasterDataDtos.cs`, `ApiResponse.cs`, base component `base-editor-*`, `base-detail-*`,
`data-filter`, `filter-select`, `resource-filter-select`, `row-action-menu`, `toast-stack`,
`base-form-control`, `use-select-resource`, `private-route-token-utils`, `route-param-utils`,
`use-permission`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/asset-management/master-data/asset-{category,classification,group,ownership-type,location,document-type,inventory-code,setting}-constants.jsx` | Baru. Satu objek `*_CONFIG` per layar: endpoint, route, resource hak akses, kartu ringkasan, saringan, kolom, isian formulir, baris rincian |
| `src/lib/constants/corporate/asset-management/master-data/asset-master-registry.jsx` | Baru. Peta kunci route → config |
| `src/lib/state/slice/corporate/asset-management/master-data/asset-master-data-slices.jsx` | Baru. Tujuh resource dari factory `createMasterDataResourceSlice` (9 thunk per resource, `signal` diteruskan) dan thunk `setAssetCategoryDocumentRequirements` |
| `src/lib/state/slice/corporate/asset-management/master-data/asset-setting-slice.jsx` | Baru. `GET`/`PUT settings` (bukan koleksi, factory tidak cocok) |
| `src/lib/state/store.jsx` | Mendaftarkan 7 reducer master + `assetMasterSetting` |
| `src/lib/hooks/corporate/asset-management/master-data/use-asset-master-{list,detail,editor,record,route}.jsx`, `asset-master-features.js` | Baru. Controller daftar, rincian, editor; resolusi token route privat; navigasi |
| `src/lib/hooks/corporate/asset-management/master-data/use-asset-category-document-requirements.jsx`, `use-asset-setting.jsx`, `../use-asset-toasts.jsx` | Baru |
| `src/utils/corporate/asset-management/master-data/asset-master-utils.js` | Baru. Fungsi murni: baca isian, format sel, bentuk form/payload, validasi, pemetaan galat backend ke isian, token route |
| `src/components/view/corporate/asset-management/master-data/shared/*` | Baru. View daftar, rincian, formulir, kolom tabel, saringan relasi |
| `src/components/view/corporate/asset-management/master-data/category/asset-category-document-requirements.jsx` | Baru. Bagian Dokumen Wajib |
| `src/components/view/corporate/asset-management/master-data/setting/asset-setting-view.jsx` | Baru |
| `src/app/corporate/asset-management/master-data/[feature]/{page,create/page,[slug]/page,[slug]/update/page}.jsx`, `settings/page.jsx` | Baru. Route tipis; kunci layar tak dikenal, `create` untuk layar baca-saja, dan token kosong/cadangan → `notFound()` |
| `src/style/corporate/asset-management/asset-category-document-requirements.module.css` | Baru. Grid kartu centang; hanya token `--space-*` dan `--color-text-muted` |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi `app → view → hooks → state → InstanceAxios` dijaga; view tidak memanggil Axios;
utils murni. Seluruh request lewat `InstanceAxios` dan factory yang sudah ada.

**Pola yang menyimpang dari standar master data, beserta alasannya:**

| Standar | Yang dikerjakan | Alasan |
| --- | --- | --- |
| Tujuh berkas per fitur, tanpa hook/view master-data generik (`AGENTS.md`) | Satu config per layar; slice, hook, view, dan route **dipakai bersama** dalam lingkup modul Aset saja (route dinamis `[feature]`) | Rizki meminta eksplisit 16 November 2026: tanpa boilerplate, komponen dan hook reusable. Enam master berbentuk sama; pola per-fitur akan menghasilkan ±17.000 baris salinan. Abstraksi tidak keluar dari folder `asset-management` |
| Saringan tanggal/periode tetap dirender walau backend belum mendukung, dengan komentar alasan (§9) | Tidak dirender | `AssetMasterPagedQuery` tidak menerima tanggal dan `CustomPeriods` backend kosong; kontrol yang tidak berpengaruh menyesatkan; aturan tanpa komentar |
| Hidrasi `DefaultFilter` dari metadata | Default lokal dipakai langsung | Nilainya identik dengan default backend; hidrasi hanya menambah satu request daftar |
| Panel pratinjau editor | `showPreview={false}` | Permintaan Rizki: tanpa deskripsi yang tidak penting |
| Hapus dengan alasan | Hapus tanpa alasan (`requireReason: false`) | `DELETE` backend tidak menerima body |

**Tabel keputusan base component**

`UI GATE: 14 elemen — REUSE 11, EXTEND 1, COMPOSE 1, WRAP 1, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx` | REUSE | — |
| Kartu ringkasan | `SummaryGrid` | `base-features/summary-grid.jsx` | REUSE | — |
| Saringan + pencarian | `DataFilter`, `FilterSelect` | `data-filter.jsx`, `filter-select.jsx` | REUSE | — |
| Saringan relasi (kategori, klasifikasi, unit) | `ResourceFilterSelect` + `useSelectResource` | `resource-filter-select.jsx` | WRAP | `AssetMasterResourceFilter` menambah opsi "Semua …" agar saringan dapat dikosongkan |
| Tabel + paginasi | `DataTable`, `Pagination` | `data-table.jsx` | REUSE | `sortLatestFirst={false}` karena backend sudah menyortir |
| Status | `StatusBadge` | `status-badge.jsx` | REUSE | — |
| Aksi baris | `RowActionMenu` | `row-action-menu.jsx` | REUSE | — |
| Galat / akses ditolak | `InformationAlert`, `AccessDeniedGate` | — | REUSE | — |
| Notifikasi | `ToastStack` | — | REUSE | — |
| Rincian | `BaseDetailView` | — | REUSE | — |
| Konfirmasi hapus | `ConfirmModal` lewat `BaseDetailView` | — | REUSE | — |
| Formulir | `BaseEditorView` (+ `selectedOptionMap`) | `base-editor-form.jsx` | REUSE | — |
| Select relasi di formulir | `BaseEditorField` + registry `SELECT_RESOURCES` | `select-resource-registry.js` | EXTEND | Berkas registry domain baru, didaftarkan seperti Finance; perilaku bawaan tidak berubah |
| Dokumen wajib kategori | `BaseDetailSection` + `BaseCheckboxCard` + `BaseButton` | `base-detail-section.jsx`, `base-checkbox-card.jsx` | COMPOSE | Satu CSS Module kecil untuk grid |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka kartu ringkasan, "Mengambil data kelompok aset...", "Mengambil detail kategori aset...", tombol "Menyimpan..."/"Memperbarui..."/"Menghapus..." |
| Kosong | "Data kelompok aset tidak ditemukan." + "Coba gunakan filter lain atau tambahkan data baru." (kalimat kedua tanpa ajakan tambah bagi yang tidak berhak); Dokumen Wajib: "Belum ada jenis dokumen aktif." |
| Gagal | Pesan backend apa adanya di `InformationAlert`; galat simpan yang terpetakan tampil di bawah isian, sisanya sebagai notifikasi merah. Tautan rincian dari sesi lain: "Link detail sudah kedaluwarsa … Silakan buka ulang dari daftar data." Pengaturan dengan 503: pesan backend tampil dan tombol simpan disembunyikan |
| Tanpa hak akses | `403` → "Ups! Akses Ditolak — Anda tidak memiliki akses ke menu atau fitur ini."; tanpa `Create`/`Update`/`Delete` tombol terkait tidak dirender |

---

## 5. Endpoint yang dikonsumsi

Untuk enam master koleksi berikut, bentuk endpoint-nya sama. Contoh lengkap pada Kategori; grup
lain memakai path dan resource hak aksesnya sendiri.

#### Corporate / Asset Management / Master Data / Asset Category

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/asset-management/master-data/categories/filters/metadata` | Pilihan saringan flag dan jumlah baris | `AssetCategory : Read` |
| `GET` | `/v1/corporate/asset-management/master-data/categories/summary` | Kartu ringkasan | `AssetCategory : Read` |
| `GET` | `/v1/corporate/asset-management/master-data/categories` | Tabel daftar | `AssetCategory : Read` |
| `GET` | `/v1/corporate/asset-management/master-data/categories/options` | Select kategori pada formulir/saringan Kelompok | `AssetCategory : Read` |
| `GET` | `/v1/corporate/asset-management/master-data/categories/{id}` | Rincian dan isi formulir perbarui | `AssetCategory : Read` |
| `POST` | `/v1/corporate/asset-management/master-data/categories` | Tambah | `AssetCategory : Create` |
| `PUT` | `/v1/corporate/asset-management/master-data/categories/{id}` | Perbarui | `AssetCategory : Update` |
| `PATCH` | `/v1/corporate/asset-management/master-data/categories/{id}/status` | Aktifkan/Nonaktifkan dari daftar | `AssetCategory : Update` |
| `PUT` | `/v1/corporate/asset-management/master-data/categories/{id}/document-requirements` | Simpan Dokumen Wajib | `AssetCategory : Update` |
| `DELETE` | `/v1/corporate/asset-management/master-data/categories/{id}` | Hapus dari rincian | `AssetCategory : Delete` |

#### Corporate / Asset Management / Master Data / Asset Group

Sembilan endpoint baseline seperti di atas pada `/groups` (`AssetGroup : Read/Create/Update/Delete`), ditambah:

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/asset-management/master-data/groups/inventory-codes` | Layar Kode Inventaris | `AssetGroup : Read` |

#### Corporate / Asset Management / Master Data / Asset Classification · Asset Ownership Type · Asset Location · Asset Document Type

Sembilan endpoint baseline pada `/classifications`, `/ownership-types`, `/locations`,
`/document-types` dengan resource `AssetClassification`, `AssetOwnershipType`, `AssetLocation`,
`AssetDocumentType`. `GET /document-types/options` juga dipakai bagian Dokumen Wajib kategori.
Formulir Lokasi memakai pilihan unit organisasi (`organizationUnits`, HR) dan ruangan klinis
(`rooms`, Pelayanan Kesehatan) yang sudah terdaftar di registry pilihan.

#### Corporate / Asset Management / Master Data / Asset Setting

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/asset-management/master-data/settings` | Isi formulir pengaturan | `AssetSetting : Read` |
| `PUT` | `/v1/corporate/asset-management/master-data/settings` | Simpan pengaturan | `AssetSetting : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 39 berkas | 0 error, 0 warning (tiga peringatan `react-hooks/set-state-in-effect` di putaran awal sudah diperbaiki) | `PASS` | Keluaran perintah |
| Pemeriksaan statis import dan export | 38 berkas, 0 masalah | `PASS` | Skrip Node |
| Tujuh layar daftar memuat (SuperAdmin) | Semua `200`; baris 7/7/7/4/1/2/7 sesuai seed; tanpa geser horizontal | `PASS` | `hasil-uji.jsonl` langkah `list-*`, `list-*.png` |
| Cari "ALAT" di Kategori | Request `search=ALAT`, 4 baris | `PASS` | langkah `search-categories` |
| Saringan Penyusutan = Tidak disusutkan | Opsi "Semua penyusutan/Disusutkan/Tidak disusutkan"; request `isDepreciable=false`, 1 baris; digabung dengan pencarian | `PASS` | `flag-filter-categories` |
| Atur ulang saringan | Request kembali ke default, 7 baris | `PASS` | `reset-categories` |
| Jumlah baris 10 | Request `pageSize=10` | `PASS` | `page-size-categories` |
| Saringan relasi Kategori di Kelompok | Opsi dari `/categories/options`, request `categoryId=…`, 1 baris | `PASS` | `resource-filter-groups` |
| Klik dua kali baris Kelompok | URL bertoken tanpa UUID; tombol hanya Kembali/Perbarui/Hapus; baris "Status Kode" ada | `PASS` | `detail-group`, `detail-group.png` |
| Kode inventaris `A` lalu `pny` | `400` + pesan `051` di bawah isian; `pny` → `PNY`, `400` + pesan `052` di bawah isian | `PASS` | `group-inventory-code-errors`, `update-group-errors.png` |
| Simpan formulir kosong | 3 pesan wajib, `POST` tidak dikirim | `PASS` | `create-empty-validation` |
| Kode ganda `stnk` | `409` + "Kode ini sudah dipakai data lain." di bawah isian Kode | `PASS` | `create-duplicate-code` |
| Tambah → Perbarui → Nonaktifkan → Hapus jenis dokumen uji | `200` semuanya; diarahkan ke rincian bertoken; isian perbarui terisi; badge Nonaktif + notifikasi; hapus kembali ke daftar | `PASS` | langkah `create-`, `update-`, `toggle-`, `delete-document-type` |
| Rincian Kategori | Bagian Dokumen Wajib dengan 7 kartu centang; tombol simpan nonaktif sebelum ada perubahan | `PASS` | `detail-category.png` |
| Select Unit Pemilik pada formulir Lokasi | Opsi unit organisasi termuat | `PASS` | `location-unit-options` |
| Pengaturan: ketik 0, kosongkan, simpan | Isian dikunci ke 1 oleh base component; isian kosong → pesan wajib; `PUT` tidak terkirim (diblokir di jaringan sebagai pengaman); Batalkan Perubahan → 90 | `PASS` | `settings-validation`, `settings-reset` |
| Lebar 400 px | Tanpa geser horizontal halaman | `PASS` | `narrow-groups.png` |
| Galat runtime | 0 `pageerror`; 0 respons 5xx; galat konsol hanya 400/409 yang sengaja dipicu | `PASS` | langkah `summary` |
| Akun LOKET tanpa izin Aset | 3 request `403` → layar Akses Ditolak; tombol Tambah dan aksi baris tidak ada | `PASS` | `uji-hak-akses.mjs`, `hak-akses-loket.png` |
| `npm run build` (Rizki) | Berhasil; `postbuild` menyiapkan standalone | `PASS` | `npm run build` dijalankan Rizki 16 November 2026 — berhasil (`BUILD_ID` 14.20.20, `postbuild` standalone selesai; manifest memuat route `master-data/[feature]`, `[feature]/create`, `[feature]/[slug]`, `[feature]/[slug]/update`, `settings`) |

Seluruh bukti ada di `QuilvianSystemFrontendDev/test-with-agy/asset-management/fe-ast-002-20261116/`
(folder diabaikan git). Uji manual: `PASS`.

**Tidak dijalankan:** kasus "punya `Read` tetapi tanpa `Create`/`Update`" — tidak ada akun uji
dengan hak sebagian, dan membuatnya lewat `POST role-access` menimpa seluruh policy peran
(`NOT FEASIBLE`). Perilakunya dijamin source: `canCreate`/`canUpdate` dari `usePermission`.
Unit test tidak ditulis (`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `SCR-AST-REG-02..09` dengan bentuk daftar bersama dan formulir | Terpenuhi | Tujuh daftar lewat `asset-master-list-view.jsx` + formulir `asset-master-form-view.jsx`; Pengaturan `asset-setting-view.jsx`; uji bagian 6 |
| 2. Pesan `AST-VAL-REG-050..056` tampil per isian | Terpenuhi | `ASSET_MASTER_FIELD_ERROR_CODES` + `resolveFieldErrors`; `050`, `051`, `052` diuji runtime; `053` dipetakan ke isian yang sama dan isiannya terkunci sebelum dikirim; `055` dipetakan ke Unit Pemilik; `056` dicegah oleh rentang `min`/`max` per isian (base component mengunci nilai) dan validasi klien per isian |
| 3. Kode kelompok terkunci menampilkan alasan | Terpenuhi (source) | `lockedBy: "isInventoryCodeLocked"` → isian nonaktif dengan alasan; rincian "Status Kode: Terkunci. …". Belum ada kelompok terkunci di data dev (perlu aset bernomor dari `BE-AST-007`), jadi tampilannya belum teramati di layar |
| 4. Tombol Tambah/Ubah tersembunyi tanpa hak `Create`/`Update` | Terpenuhi | `usePermission(resource, "Create"/"Update"/"Delete")`; diuji dengan akun LOKET |
| DoD: `V-FE` dicatat apa adanya | Terpenuhi | Lint `PASS`, uji manual `PASS`, `npm run build` `PASS` (Rizki) |
| DoD: laporan + roadmap + traceability | Terpenuhi | Berkas ini, kartu roadmap, baris traceability |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | NONE pada berkas task ini |
| Masalah yang diketahui | Tampilan kode terkunci belum teramati di layar karena belum ada aset bernomor. Kolom "Dibuat Oleh/Diubah Oleh" di rincian tampil "-" karena backend hanya mengirim ID pengguna (UUID tidak boleh tampil) |
| Dependency backend | NONE untuk task ini |
| Perubahan sampingan | **Data dev tersentuh oleh uji.** (1) Satu putaran uji awal mengirim `PUT settings` dengan Ambang Aset Menganggur = 1 karena input angka mengunci 0 menjadi 1; nilainya sudah dikembalikan lewat layar ke 90/30/12/tanpa cutover (dibaca ulang setelah muat ulang). Dua entri audit `AssetSetting.Update` tercatat. Skrip uji kini memblokir `PUT settings`. (2) Tiga jenis dokumen uji `UJIFE#####` dibuat lalu dihapus (hapus lunak — barisnya tetap ada dengan `IsDelete = true`). (3) Upaya ubah kelompok dengan kode tidak valid ditolak `400`, tidak ada perubahan |
| Interupsi | NONE |
| Status Git | Frontend `RizkiV2`: 3 berkas diubah (`store.jsx`, `select-resource-registry.js`, `menu-items.jsx`) dan 37 berkas baru di bawah `asset-management`/`select/corporate`. 11 berkas Nurse View/IGD lain yang juga tampil di `git status` adalah pekerjaan sebelumnya, tidak disentuh. Belum di-commit |
| Langkah berikutnya | `FE-AST-003` setelah `BE-AST-007` selesai, memakai pola konfigurasi + hook/view bersama yang sama |
