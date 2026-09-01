# FE-BKC-013 — Billing Item Category (CRUD Master Data)

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-013` (task baru — entity `BillingItemCategory` sudah punya backend CRUD penuh sejak awal modul, tapi frontend-nya belum pernah dibangun; hanya ada slice Redux `options` untuk dropdown picker) |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice baru (master data CRUD) |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak) |
| Write target | `QuilvianSystemFrontendDev` (source, branch `yasmina`); laporan ini ditulis di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Status task | Source selesai ditulis, lint dan build lulus bersih (route `list`/`create`/`[id]/update` terkonfirmasi ter-compile). Belum di-commit. Belum diverifikasi manual ter-autentikasi. |

## Catatan penting sebelum implementasi

Working tree frontend sempat terlihat "kosong" (`git status` bersih) di awal task ini karena
seluruh pekerjaan sesi-sesi sebelumnya (`FE-BKC-011`, `FE-BKC-012`, dan lainnya) ternyata sudah
di-commit oleh pengguna sendiri secara paralel (`d21042c39 fix billing invoice detail`,
`7642e9e10 update fe billing`, dkk., tanggal 2026-08-30) — bukan hilang. Pengguna juga sempat
mengubah pola `use-tax-rule-editor.js` dari "cache dari list" menjadi GET-by-id murni
(`getTaxRuleById`/`selectTaxRuleDetail`) sebelum task ini dimulai. Task ini SENGAJA mengikuti pola
GET-by-id yang lebih baru itu (bukan pola cache-list yang saya bangun sebelumnya), karena
`BillingItemCategoryController.cs` memang punya endpoint `GET {id:guid}` asli.

## Ringkasan untuk pembaca umum

Sebelumnya, tidak ada halaman sama sekali untuk mengelola "Billing Item Category" — kategori
klasifikasi setiap item tagihan (obat, tindakan, biaya admin, dst.) yang menentukan bagaimana item
itu diperlakukan oleh seluruh modul Billing (apakah kena diskon, kena pajak, ditanggung penjamin
secara default, butuh approval dokter, dan seterusnya). Backend-nya sudah lengkap sejak awal
modul; task ini membangun tampilannya.

Tiga halaman baru:

1. **Daftar Billing Item Category** — tabel dengan filter (cari, sumber item, status), plus
   ringkasan angka (Total, Aktif, Nonaktif, dan beberapa kategori populer) karena entity ini
   satu-satunya di antara master data billing yang punya endpoint `/summary` siap pakai.
2. **Tambah/Perbarui Billing Item Category** — form dengan 19 flag boolean (Biaya Registrasi,
   Biaya Administrasi, Tindakan, Laboratorium, dst.) dikelompokkan ke 4 bagian bermakna
   (Identitas, Jenis Layanan, Perilaku Finansial, Kontrol di Kasir) plus bagian Lainnya —
   memakai layar form BERKELOMPOK (`BaseGroupedEditorView`), bukan form polos, karena jumlah
   field jauh melebihi ambang ~10 field yang jadi patokan kapan memakai varian ini.
3. Aksi **Aktifkan/Nonaktifkan/Hapus** langsung dari baris tabel, masing-masing dengan
   konfirmasi.

## Base Component Decision Gate

`UI GATE: 3 elemen — REUSE 2, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Catatan |
| --- | --- | --- | --- | --- |
| Halaman daftar (Hero, filter, tabel, ringkasan) | `Hero`, `DataFilter`, `FilterSelect`, `DataTable`, `StatusBadge`, `SummaryGrid`, `ConfirmModal`, `ToastStack`, `AccessDeniedGate` | Identik dengan `tax-rule-view.jsx` yang sudah ada, ditambah `SummaryGrid` (dipakai apa adanya, bukan varian baru) | REUSE | `SummaryGrid` sengaja ditambahkan di sini meski tidak dipakai 5 entity billing lain — backend entity ini satu-satunya yang punya endpoint `/summary` nyata, dan `SummaryGrid` memang bagian pola kanonik halaman list (`ui-consistency-checklist.md` bagian B) yang selama ini terlewat di entity lain karena datanya memang belum ada. |
| Form create/update 24 field (19 boolean + 5 field identitas) | `BaseGroupedEditorView`/`BaseGroupedEditorForm`/`BaseGroupedEditorField` | `src/components/features/base-features/base-grouped-editor-view.jsx`, `base-grouped-editor-form.jsx` — dibaca langsung untuk memastikan kontrak `groups: [{key,title,description,fields:[name,...]}]` | REUSE | Bukan `BaseEditorView` polos yang dipakai 5 entity lain — field jauh lebih banyak (24 vs ~8), dan katalog eksplisit menyebut `BaseGroupedEditorView` untuk kasus "lebih dari sekitar sepuluh field". `layout` per grup sengaja dibiarkan default (tidak diisi token seperti `pair`/`region`) karena tidak ada token bawaan yang cocok untuk "grid checkbox" — grid dasar `groupGrid` sudah cukup tanpa perlu override. |
| Pengelompokan 19 flag ke 4 bagian bermakna (bukan satu daftar checkbox datar) | Tidak ada base component tersendiri untuk "grouping semantik" — ini keputusan konten (field mana masuk grup mana), bukan komponen visual baru | — | COMPOSE | Murni penyusunan `groups` config berdasarkan makna domain (Jenis Layanan vs Perilaku Finansial vs Kontrol Kasir), memakai mekanisme `groups` yang sudah disediakan `BaseGroupedEditorForm`. |

## Files changed

| Layer | File |
| --- | --- |
| Redux slice (extend) | `master-data-billing-item-category-slice.jsx` — thunk `getBillingItemCategoryOptions` (lama, dipertahankan) + baru: `getBillingItemCategories`, `getBillingItemCategorySummary`, `getBillingItemCategoryById`, `createBillingItemCategory`, `updateBillingItemCategory`, `activateBillingItemCategory` (PATCH, bukan POST — mengikuti method asli backend), `deactivateBillingItemCategory` (PATCH), `deleteBillingItemCategory` |
| Constants | `billing-item-category-constants.js` — routes, `ITEM_SOURCE_TYPE_OPTIONS` (11 opsi tetap dari `AllowedItemSourceTypes` backend), status options/badge |
| Editor config | `billing-item-category-editor-config.jsx` — 24 field + 5 `groups` |
| List hook | `use-billing-item-category-list.js` |
| Editor hook | `use-billing-item-category-editor.js` — pola GET-by-id (lihat catatan di atas) |
| List view | `billing-item-category-view.jsx` |
| Form view | `form/billing-item-category-form-view.jsx` |
| Routes | `app/.../billing-item-category/page.jsx`, `.../create/page.jsx`, `.../[id]/update/page.jsx` |
| Menu | `menu-items.jsx` — entri baru "Billing Item Category" di submenu Master Data |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `eslint` (full severity) | **PASS** | `npx eslint <11 file berubah/baru>` → 0 error, 1 warning (`react-hooks/set-state-in-effect` pada `use-billing-item-category-editor.js` — pola yang SAMA persis dengan seluruh hook editor sibling lain di modul ini, bukan regresi baru melainkan pola yang diikuti secara konsisten). |
| `npm run build` | **PASS** | Exit 0, `postbuild` selesai normal. Ketiga route (`list`, `create`, `[id]/update`) terkonfirmasi ada di `.next/server/app/health-services/billing-management/master-data/billing-item-category/`. |
| Grep anti-regresi (checklist C/D/G) | **PASS** | Nol hasil untuk hex/rgb literal, override typography, tombol non-base, `<table>` mentah, utility typography Bootstrap, dan `!important` pada kedua file view baru. |
| Kontrak API | **Dibaca langsung dari source**, bukan ditebak | `BillingItemCategoryController.cs` dibaca penuh sebelum implementasi — nama field, tipe (19 boolean spesifik), method HTTP `activate`/`deactivate` (`PATCH`, beda dari Tax Rule yang `POST`), dan daftar `ItemSourceType` tetap semuanya diambil langsung dari controller, bukan diasumsikan dari pola sibling. |
| Test otomatis | `AUTOMATED TEST: SKIPPED (opsional) — repo tidak memakai Jest (test-policy.md)` | — |
| Verifikasi manual (klik-coba create/update/aktifkan/nonaktifkan/hapus dengan data nyata) | **NOT DONE** | Tidak ada kredensial login yang tersedia untuk builder. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint dan build lulus bersih, tapi klik-coba
langsung (terutama submit 19 flag boolean sekaligus dan konfirmasi payload yang terkirim sesuai)
belum diverifikasi di browser nyata.

## Langkah berikutnya yang direkomendasikan

1. Login dan buka Billing Management → Master Data → Billing Item Category, buat satu kategori
   baru dengan kombinasi flag yang representatif (mis. kategori "Tindakan" dengan
   `isProcedure=true`, `isCoveredByInsuranceDefault=true`, `isNeedDoctor=true`), lalu verifikasi
   payload yang terkirim ke backend membawa seluruh 19 flag dengan nilai yang benar.
2. Uji Aktifkan/Nonaktifkan/Hapus dari tabel, konfirmasi `PATCH`/`DELETE` terkirim ke path yang
   benar (`/activate`, `/deactivate` — bukan `POST` seperti Tax Rule).
3. Konfirmasi ringkasan (`SummaryGrid`) di halaman daftar menampilkan angka yang cocok dengan
   data aktual setelah create/delete.
