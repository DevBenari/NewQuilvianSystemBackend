# Laporan Perubahan Frontend — `FE-FIN-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-001` |
| Judul | Pengelolaan data induk Finance |
| Slice | `MVP-0` — Bank, rekening, mata uang, kurs — CRUD beserta aktif/nonaktif |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task) dan bagian 8 (urutan) |
| Trace | `FR-FIN-001`..`004`; `FIN-DEC-024`..`029` (UI brief); `FIN-API-1.0`, `FIN-PERM-1.0` |
| Contract version | `FIN-API-1.0` (locked 20 September 2026), `FIN-PERM-1.0` (locked 20 September 2026) — dipatuhi apa adanya |
| Wewenang UI | UI brief closed 23 September 2026 (`FIN-DEC-024`..`029`, `00-interview-decisions.md`) — struktur rute, penamaan menu, dan tata letak `DEV_DISCRETION` sesuai roadmap bagian 6 |
| Dependency | `BE-FIN-004` — ✅ selesai 23 September 2026 (`dotnet build` PASS, migration diterapkan, endpoint diuji langsung — [laporan](../backend/BE-FIN-004.md)) |
| Klasifikasi | `HEAVY` — dua fitur master data penuh (Rekening Bank, Mata Uang) mengikuti `master-data-feature-standard.md` (7 berkas + 2 registrasi per fitur), ditambah sub-resource riwayat kurs dan perbaikan wiring Redux lintas fitur (`masterDataBank` Administrator yang belum pernah didaftarkan ke store) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/constants/finance/master-data/`, `src/lib/state/slice/finance/master-data/`, `src/utils/finance/master-data/`, `src/lib/hooks/finance/master-data/`, `src/components/view/finance/master-data/`, `src/app/finance/master-data/`, `src/lib/state/store.jsx` (registrasi reducer), `src/utils/menu-sidebar/menu-items.jsx` (entri menu) |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | Working tree pada branch `yasmina` |
| Commit backend yang dijadikan rujukan | Working tree pada branch `Yasmina`, `NewQuilvianSystemBackend` — `BE-FIN-004` sudah selesai |
| Tanggal | 23 September 2026 |
| Status | 🟡 **SEBAGIAN — source lengkap untuk dua fitur (Rekening Bank, Mata Uang+Kurs) selesai ditulis mengikuti `master-data-feature-standard.md`.** `npm run lint:errors`/`npm run build` **belum dijalankan** — lihat bagian 6 |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, grup `corporateFinance` di sidebar hanya berisi dua butir (Kategori Petty Cash,
Anggaran Petty Cash — `FIN-CAP-015`). Tidak ada satu pun layar untuk Rekening Bank atau Mata Uang.
Modul rujukan otoritatif untuk bentuk fitur master data (`master-data-feature-standard.md`) adalah
`hr/master-data/job-level` — source-nya dibaca penuh (constants, slice, utils, tiga hook, tiga
view, lima berkas route) sebagai templat sebelum menulis kode baru, sesuai `AGENTS.md`
("ikuti kode yang sudah ada").

**Temuan yang mengubah cakupan task**: form Rekening Bank butuh select Bank (`BankId`, FK ke
`MstBank` milik Administrator — Finance tidak punya CRUD Bank sendiri, lihat laporan `BE-FIN-004`
bagian 1). Slice Administrator Bank (`master-data-bank-slice.jsx`) sudah ada dan lengkap
(`getBankOptions`, `selectBankOptions`), tetapi **tidak pernah didaftarkan** ke
`src/lib/state/store.jsx` — `state.masterDataBank` karena itu selalu `undefined` dan pilihan Bank
di form manapun yang memakainya tidak akan pernah terisi. Ini persis skenario yang diperingatkan
`master-data-feature-standard.md` bagian 8 ("lengkapi wiring-nya, catat sebagai temuan") — bukan
gap yang boleh didiamkan karena task ini bergantung langsung padanya. Diperbaiki dengan
mendaftarkan `masterDataBank: masterDataBankSlice` ke store (satu baris import + satu baris
registrasi), di luar folder Finance tetapi merupakan prasyarat langsung fitur ini.

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Rekening Bank

1. Staf Finance membuka **Keuangan → Master Data → Rekening Bank** dari sidebar.
2. Layar daftar menampilkan ringkasan (total/aktif/nonaktif), filter (tanggal, periode, status,
   pencarian nomor rekening/nama pemilik), dan tabel rekening.
3. Klik **+ Tambah Rekening Bank** membuka form: pilih Bank (select, sumber `MstBank`
   Administrator), isi Nomor Rekening, Nama Pemilik Rekening, Jenis Rekening (Operasional/
   Penerimaan/Pembayaran), dan Mata Uang (select, sumber fitur Mata Uang pada task ini juga).
4. Simpan. Backend menolak nomor rekening yang sama pada bank yang sama dengan pesan
   "Nomor rekening ini sudah terdaftar untuk bank yang sama." (`UAT-01`) — pesan itu tampil
   sebagai toast merah persis dari respons `409`, tidak ditulis ulang di frontend.
5. Klik dua kali baris data membuka detail (Kembali/Perbarui/Hapus — bukan Aktifkan/Nonaktifkan,
   sesuai `AGENTS.md` bagian Aturan Master Data).

### 2.2 Mata Uang dan Kurs

1. Staf Finance membuka **Keuangan → Master Data → Mata Uang & Kurs**.
2. Daftar, filter, dan form tambah/ubah mengikuti pola yang sama (kode ISO 4217, nama, simbol,
   jumlah desimal, penanda Mata Uang Dasar). Menandai mata uang kedua sebagai dasar padahal sudah
   ada satu ditolak backend dengan pesan yang tampil apa adanya (`UAT-02`).
3. Halaman detail **tidak** menampilkan tombol Hapus — `FIN-API-1.0` sengaja tidak menyediakan
   `DELETE` untuk grup Currency (mata uang hanya bisa dinonaktifkan, lihat `BE-FIN-004` bagian 1).
   Ini penyimpangan sah dari baku tiga-tombol master data karena keterbatasan kontrak, bukan
   pilihan tampilan — dicatat eksplisit sebagai komentar di source.
4. Di bawah kartu detail, staf melihat **Riwayat Kurs Harian** (tabel append-only) dan form ringkas
   untuk mencatat kurs baru (tanggal, kurs beli, kurs jual, kurs tengah, sumber). Mencatat kurs
   dua kali untuk tanggal yang sama ditolak backend (`409`) dan pesannya tampil apa adanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `rules/frontend/master-data-feature-standard.md`, `base-component-catalog.md`,
  `base-component-decision-gate.md`, `page-composition-patterns.md`, `frontend-architecture.md`
- `src/lib/constants/hr/master-data/job-level/job-level-constants.jsx` dan lima berkas
  sekerabatnya (modul rujukan otoritatif) — dibaca penuh sebagai templat
- `src/app/finance/master-data/petty-cash-category/` — preseden route Finance yang sudah ada
- `Areas/Corporate/FinanceManagement/MasterData/{DTOs,Controllers}/BankAccountDtos.cs`,
  `CurrencyDtos.cs`, `BankAccountsController.cs`, `CurrenciesController.cs` (backend,
  `NewQuilvianSystemBackend`) — kontrak API persis, bukan tebakan
- `Areas/Administrator/MasterData/Controllers/BankController.cs` — kontrak `GET /options` untuk
  select Bank
- `src/lib/state/slice/administrator/master-data/master-data-bank-slice.jsx`,
  `src/lib/state/store.jsx` — memastikan Bank options benar-benar dapat dipakai ulang

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/master-data/bank-account/bank-account-constants.jsx` | Baru — `BANK_ACCOUNT_CONFIG` |
| `src/lib/state/slice/finance/master-data/master-data-bank-account-slice.jsx` | Baru — 9 thunk baseline |
| `src/utils/finance/master-data/bank-account/bank-account-utils.jsx` | Baru — fungsi murni form/validasi/payload/detail |
| `src/lib/hooks/finance/master-data/bank-account/use-master-data-bank-account.jsx` | Baru — controller list |
| `src/lib/hooks/finance/master-data/bank-account/use-master-data-bank-account-detail.jsx` | Baru — controller detail |
| `src/lib/hooks/finance/master-data/bank-account/use-master-data-bank-account-editor.jsx` | Baru — controller create/update, memuat opsi Bank (Administrator) dan Mata Uang |
| `src/components/view/finance/master-data/bank-account/master-data-bank-account-view.jsx` | Baru — halaman list |
| `src/components/view/finance/master-data/bank-account/detail/bank-account-detail-view.jsx` | Baru — halaman detail |
| `src/components/view/finance/master-data/bank-account/add/bank-account-form-view.jsx` | Baru — halaman create/update |
| `src/app/finance/master-data/bank-account/{page.jsx,bank-account-client.jsx,create/page.jsx,[slug]/page.jsx,[slug]/update/page.jsx}` | Baru — route |
| `src/lib/constants/finance/master-data/currency/currency-constants.jsx` | Baru — `CURRENCY_CONFIG` |
| `src/lib/state/slice/finance/master-data/master-data-currency-slice.jsx` | Baru — 8 thunk baseline (minus `DELETE`, sesuai kontrak) + 2 thunk sub-resource kurs |
| `src/utils/finance/master-data/currency/currency-utils.jsx` | Baru |
| `src/lib/hooks/finance/master-data/currency/use-master-data-currency.jsx` | Baru — controller list |
| `src/lib/hooks/finance/master-data/currency/use-master-data-currency-detail.jsx` | Baru — controller detail + riwayat kurs |
| `src/lib/hooks/finance/master-data/currency/use-master-data-currency-editor.jsx` | Baru — controller create/update |
| `src/components/view/finance/master-data/currency/master-data-currency-view.jsx` | Baru — halaman list |
| `src/components/view/finance/master-data/currency/detail/currency-detail-view.jsx` | Baru — halaman detail + riwayat kurs (tanpa tombol Hapus) |
| `src/components/view/finance/master-data/currency/add/currency-form-view.jsx` | Baru — halaman create/update |
| `src/app/finance/master-data/currency/{page.jsx,currency-client.jsx,create/page.jsx,[slug]/page.jsx,[slug]/update/page.jsx}` | Baru — route |
| `src/lib/state/store.jsx` | Disunting — registrasi `masterDataBankAccount`, `masterDataCurrency`, dan `masterDataBank` (Administrator, sebelumnya belum terdaftar — bagian 1) |
| `src/utils/menu-sidebar/menu-items.jsx` | Disunting — dua entri baru di submenu Master Data grup `corporateFinance`: "Rekening Bank", "Mata Uang & Kurs" (`FIN-DEC-025`) |

### 3.3 Kepatuhan arsitektur frontend

Bentuk berkas, sembilan-endpoint-ke-sembilan-thunk, token route privat, dan larangan UUID di
layar seluruhnya mengikuti `master-data-feature-standard.md` persis (dibandingkan baris-per-baris
terhadap `job-level`). Base component 100% pakai ulang — tidak ada component baru: `Hero`,
`SummaryCards`, `DataFilter`, `DataTable`, `FilterSelect`, `FilterDatePicker`, `StatusBadge`,
`AccessDeniedGate`, `BaseButton`, `BaseDetailView`, `BaseEditorView`, `RegionPagination`, dan untuk
form kurs: `BaseDateField`/`BaseTextField`/`InformationAlert` yang sudah ada di
`base-form-control.jsx`. Tidak ada CSS Module baru. Tidak ada factory/hook master-data generik.

Satu select filter dari metadata (`isActive`) sesuai bagian 7.1 — filter `accountType` yang
didukung backend sengaja **tidak** dirender sebagai select kedua, dicatat sebagai komentar di
`bank-account-constants.jsx` (`DEV_DISCRETION`, boleh ditambahkan lewat keputusan produk terpisah).

**Tabel keputusan base component** (bagian 3 governance `build-module-frontend`):

| Elemen layar | Status | Bukti/alasan |
| --- | --- | --- |
| Hero, SummaryCards, DataFilter, DataTable, FilterSelect, FilterDatePicker, StatusBadge, AccessDeniedGate, BaseButton, BaseDetailView, BaseEditorView, RegionPagination | `REUSE` | Dipakai identik dengan `job-level`/`petty-cash-category`, tanpa modifikasi |
| BaseDateField, BaseTextField, InformationAlert (form kurs) | `REUSE` | Primitif form yang sudah ada di `base-form-control.jsx`, dipakai di luar `BaseEditorView` untuk sub-form kecil — pola yang sama seperti field individual pada editor lain |
| Tabel Riwayat Kurs | `REUSE` (`DataTable`, `pagination={false}`) | Kurs bukan entity master data baris-per-baris dengan CRUD penuh (append-only, tanpa update/delete), jadi tidak dijadikan fitur master data ketujuh-berkas terpisah — cukup ditampilkan lewat `DataTable` yang sudah ada di dalam halaman detail Mata Uang |

Tidak ada elemen berstatus `NEW`/`EXTEND` yang mengubah perilaku default base component, sehingga
tidak ada keputusan yang menunggu Product Owner pada gerbang ini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable`/`BaseDetailCard` menampilkan `loadingText` ("Mengambil data rekening bank...", dst.), bukan layar kosong |
| Kosong | `emptyTitle`/`emptyDescription` — "Data rekening bank tidak ditemukan. Coba gunakan filter lain atau tambahkan data baru." |
| Gagal | `errorMessage` dari respons backend tampil apa adanya lewat `AccessDeniedGate`/toast merah, termasuk pesan `409` nomor rekening ganda dan `422` dua mata uang dasar |
| Tanpa hak akses | `AccessDeniedGate` menerima `errorMessage` dari kegagalan request (403 dari `[AccessPermission]` backend) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Master Data / Bank Account

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/master-data/bank-accounts/filters/metadata` | Metadata filter halaman list dan editor | `BankAccount : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/bank-accounts/summary` | Kartu ringkasan | `BankAccount : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/bank-accounts` | Tabel daftar rekening | `BankAccount : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/bank-accounts/{id}` | Halaman detail dan prefill form update | `BankAccount : Read` |
| `POST` | `/v1/corporate/finance-management/master-data/bank-accounts` | Simpan rekening baru | `BankAccount : Create` |
| `PUT` | `/v1/corporate/finance-management/master-data/bank-accounts/{id}` | Perbarui rekening | `BankAccount : Update` |
| `PATCH` | `/v1/corporate/finance-management/master-data/bank-accounts/{id}/status` | Aktifkan/nonaktifkan (hook tersedia, tombolnya sengaja tidak dirender di halaman detail — `AGENTS.md` Aturan Master Data) | `BankAccount : Update` |
| `DELETE` | `/v1/corporate/finance-management/master-data/bank-accounts/{id}` | Hapus dari halaman detail | `BankAccount : Delete` |

#### Corporate / Finance Management / Master Data / Currency

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/master-data/currencies/filters/metadata` | Metadata filter | `Currency : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/currencies/summary` | Kartu ringkasan | `Currency : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/currencies` | Tabel daftar mata uang | `Currency : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/currencies/options` | Select Mata Uang pada form Rekening Bank | `Currency : Read` |
| `GET` | `/v1/corporate/finance-management/master-data/currencies/{id}` | Detail dan prefill update | `Currency : Read` |
| `POST` | `/v1/corporate/finance-management/master-data/currencies` | Simpan mata uang baru | `Currency : Create` |
| `PUT` | `/v1/corporate/finance-management/master-data/currencies/{id}` | Perbarui mata uang | `Currency : Update` |
| `PATCH` | `/v1/corporate/finance-management/master-data/currencies/{id}/status` | Aktifkan/nonaktifkan | `Currency : Update` |
| `GET` | `/v1/corporate/finance-management/master-data/currencies/{id}/exchange-rates` | Riwayat kurs di halaman detail | `Currency : Read` |
| `POST` | `/v1/corporate/finance-management/master-data/currencies/{id}/exchange-rates` | Catat kurs baru | `Currency : Update` |

`DELETE` **sengaja tidak dikonsumsi** untuk Currency — tidak ada di `FIN-API-1.0` untuk grup ini.

#### Administrator / Master Data / Bank

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/administrator/master-data/banks/options` | Select Bank pada form Rekening Bank | `Bank : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Tidak dijalankan | `NOT RUN` | Belum dijalankan pada sesi ini |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Belum dijalankan pada sesi ini |
| Review manual: sembilan thunk BankAccount ↔ sembilan endpoint backend | Cocok satu-satu | `PASS` | Perbandingan manual `master-data-bank-account-slice.jsx` vs `BankAccountsController.cs` |
| Review manual: delapan thunk Currency (minus DELETE) + dua thunk kurs ↔ endpoint backend | Cocok | `PASS` | Perbandingan manual `master-data-currency-slice.jsx` vs `CurrenciesController.cs` |
| Review manual: `masterDataBank` sekarang benar-benar dibaca `combineReducers` | Dikonfirmasi — import dan entri reducer ditambahkan di `store.jsx`, diverifikasi lewat pembacaan berkas ulang setelah edit | `PASS` | `src/lib/state/store.jsx` |

Uji manual (buka layar di browser sungguhan): `NOT FEASIBLE` — dev server tidak dijalankan pada
sesi ini.

`AUTOMATED TEST: NOT APPLICABLE` — repository ini tidak memakai Jest; menulis test baru bersifat
opsional sesuai `test-policy.md` dan tidak diminta task ini.

**Tidak dijalankan:** `npm run lint:errors`, `npm run build`, `npm run test:unit`, uji manual di
peramban — seluruhnya menunggu verifikasi mandiri pengguna, sama seperti pola validasi backend
pada sesi-sesi sebelumnya di modul ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nomor rekening ganda ditolak dengan pesan yang dapat dibaca petugas (`UAT-01`) | **Tervalidasi kode, belum diuji runtime** — pesan `409` backend diteruskan apa adanya ke toast, tidak ditulis ulang | Bagian 2.1 poin 4; `handleSubmit` pada `use-master-data-bank-account-editor.jsx` |
| `UAT-02` (data induk siap dipakai) | **Tervalidasi kode, belum diuji runtime** — memerlukan `npm run build` dan sesi peramban sungguhan | Bagian 6 |
| Aksi mengikuti `FIN-PERM-1.0` | **Terpenuhi secara struktural** — endpoint dan aksi dipetakan 1:1 ke tabel bagian 5, tidak ada aksi yang ditebak | Bagian 5 |
| Nol perhitungan di klien | **Terpenuhi** — seluruh nilai uang/kurs disalin apa adanya dari respons backend, tidak ada `+`/`-`/`*` atas nilai kurs atau saldo di source frontend | `bank-account-utils.jsx`, `currency-utils.jsx` |
| Rute baru di bawah `/finance/...`; menu masuk grup `corporateFinance` yang sudah ada | **Terpenuhi** — `/finance/master-data/bank-account`, `/finance/master-data/currency`; submenu Master Data yang sudah ada, sesuai `FIN-DEC-024`/`025` | Bagian 3.2 |
| Halaman detail hanya `Kembali`/`Perbarui`/`Hapus` | **Terpenuhi untuk Rekening Bank.** **Menyimpang sah untuk Mata Uang** (tanpa Hapus — kontrak tidak menyediakan `DELETE`) | Bagian 2.2 poin 3 |

**Belum terpenuhi**: verifikasi runtime (`npm run lint:errors`, `npm run build`, uji manual di
peramban) — dicatat sebagai `NOT RUN`/`NOT FEASIBLE` pada bagian 6, bukan diklaim `PASS`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengguna **wajib** menjalankan `npm run lint:errors` dan `npm run build` sebelum menganggap task ini selesai — belum ada satu pun perintah validasi frontend yang dijalankan pada sesi ini |
| Masalah yang diketahui | (1) Filter `accountType` didukung backend tetapi sengaja tidak dirender sebagai select kedua (bagian 3.3, `DEV_DISCRETION`). (2) Riwayat kurs tidak berpaging penuh (`pagination={false}`, `pageSize: 25` tetap) — cukup untuk kebutuhan saat ini, dicatat sebagai penyederhanaan sub-resource, bukan fitur master data tersendiri |
| Dependency backend | `BE-FIN-004` ✅ selesai — tidak ada dependency backend yang menahan task ini |
| Perubahan sampingan | `masterDataBank` (Administrator) didaftarkan ke `store.jsx` — perbaikan wiring yang sebelumnya hilang, bukan fitur baru, dan diperlukan langsung oleh select Bank pada task ini (bagian 1). Tidak ada perubahan lain di luar cakupan |
| Interupsi | `NONE` pada task ini |
| Status Git | 9 baris (`git status --short`): `M src/lib/state/store.jsx`, `M src/utils/menu-sidebar/menu-items.jsx`, `?? src/app/finance/master-data/bank-account/`, `?? src/app/finance/master-data/currency/`, `?? src/components/view/finance/master-data/`, `?? src/lib/constants/finance/`, `?? src/lib/hooks/finance/`, `?? src/lib/state/slice/finance/`, `?? src/utils/finance/` |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run lint:errors` dan `npm run build`. (2) Uji manual di peramban: duplikasi nomor rekening (`UAT-01`), dua mata uang dasar, duplikasi kurs pada tanggal yang sama, navigasi token route privat detail/update. (3) `FE-FIN-002` (Buku Piutang) dapat mulai — dependency backend-nya (`BE-FIN-009`) sudah selesai, UI brief sudah closed |
