# `FE-ACC-002` — Daftar dan form daftar akun

| Field | Isi |
|---|---|
| Task ID | `FE-ACC-002` |
| Blueprint | `ACC-BP-001` revisi 10, `roadmap/frontend-roadmap.md` gelombang `MVP-1` |
| Task mode | `FRONTEND` |
| Kontrak | `ACC-API-0.3` grup Chart of Account; `ACC-VALIDATION-0.3` bagian 1 |
| Wewenang UI | `ACC-FE-001` pilihan B, `ACC-FE-004`..`008` `DEV_DISCRETION` |
| Repository target tulis | `QuilvianSystemFrontendDev`, branch `RizkiV2` @ `1a86d9322` |
| Snapshot backend | `rizkiG` @ `822d48a` — impact scan: **0 berkas** Chart of Account berubah sejak baseline |
| Status | **`IMPLEMENTED`** — menunggu verifikasi manual owner di peramban |
| Tanggal | 4 September 2026 |

## Ringkasan untuk pembaca umum

Administrator kini dapat **menyusun daftar akun rumah sakit lewat layar**, bukan lewat Swagger.
Layarnya menyediakan:

1. **Tabel akun berhalaman** dengan penyaring badan hukum, jenis akun, status aktif, dan
   akun-menerima-transaksi.
2. **Tampilan susunan induk-anak** sebagai alternatif tabel, supaya kedalaman daftar akun terbaca.
3. **Form tambah dan perbarui**, termasuk pilihan akun induk yang otomatis dibatasi pada badan
   hukum yang sedang dipakai.
4. **Tombol nonaktifkan berkonfirmasi**, dengan isian alasan opsional.

Inilah layar yang menutup `BLK-ACC-02` pada `testing/readiness-report.md`: daftar akun yang masih
kosong kini dapat diisi tanpa menyentuh Swagger.

## 1. Gerbang pemakaian ulang

| Kebutuhan | Dipakai ulang | Putusan |
|---|---|---|
| Slice CRUD | **`createMasterDataResourceSlice`** — roadmap menyebutnya keharusan | **REUSE** |
| Pemilih badan hukum | `useAccountingLegalEntity` + `AccountingLegalEntitySelect` dari `FE-ACC-001` | **REUSE** |
| Tabel, penyaring, paginasi | `DataTable`, `DataFilter`, `FilterSelect`, `RegionPagination` | **REUSE** |
| Form | `BaseEditorView` — menangani tata letak, pratinjau, toast, dan tombol sekaligus | **REUSE** |
| Rincian | `BaseDetailView` | **REUSE** |
| Konfirmasi | `ConfirmModal`, dengan isian alasan lewat slot `children` | **REUSE** |
| Gaya | `base-data-components.module.css`; satu CSS Module baru hanya untuk kartu konteks dan pohon | **REUSE + tambahan minimal** |

## 2. Dua penyesuaian terhadap factory — dan alasannya

Factory dibangun untuk master data Administrator. API Accounting berbeda bentuk di dua titik, dan
keduanya diselesaikan **tanpa menyentuh factory**, karena factory itu dipakai lima slice lain dan
mengubahnya bukan wewenang modul Accounting.

| # | Selisih | Penyelesaian |
|---|---|---|
| 1 | `deactivateItem` bawaan factory mengirim `patch(path, null)` — **tanpa badan permintaan**. Endpoint Accounting menyatakan `[FromBody] DeactivateChartOfAccountRequest`, sehingga permintaan tanpa badan ditolak `400` oleh model binding sebelum aturan bisnisnya sempat berjalan | Thunk tersendiri `deactivateChartOfAccount` yang selalu mengirim objek `{ reason }`, walau alasannya kosong |
| 2 | `GET /tree` tidak punya padanan di factory | Thunk tersendiri `getChartOfAccountTree` |

Thunk `getFilterMetadata`, `getSummary`, dan `deleteItem` **tidak diekspor**: endpoint
`/filters/metadata`, `/summary`, dan `DELETE` memang tidak ada pada API daftar akun. Akun
dinonaktifkan, tidak dihapus — kode akun yang pernah dipakai jurnal harus tetap dapat ditelusuri.

## 3. Acceptance

| # | Acceptance | Keadaan | Dasar |
|---|---|---|---|
| (1) | Daftar berhalaman dengan `pageSize` bawaan 25 | **TERBUKTI di source** | `filterDefaults.pageSize: 25`; penyaring ukuran halaman menyediakan 10/25/50/100 |
| (2) | Pesan galat `409` tampil sebagai kalimat Bahasa Indonesia | **TERBUKTI di source, belum dilihat berjalan** | Pesan backend ditampilkan **apa adanya** lewat toast; nol kalimat pengganti buatan layar |
| (3) | Penonaktifan akun bersaldo menampilkan pesan backend beserta jumlah saldonya | **TERBUKTI di source, belum dilihat berjalan** | Sama seperti (2). Backend mengirim "Akun masih bersaldo Rp … dan tidak dapat dinonaktifkan" |
| (4) | Slice terdaftar di `store.jsx` | **TERBUKTI** | `accountingChartOfAccount: accountingChartOfAccountSlice` |

Butir (2) dan (3) bergantung pada satu keputusan yang sengaja diambil: **layar tidak pernah
mengarang pesan galat sendiri.** Setiap penolakan backend diteruskan apa adanya, sehingga angka
saldo dan nomor jurnal yang disebut backend sampai ke petugas.

## 4. Berkas

| Lapisan | Berkas | Baris |
|---|---|---:|
| Slice | `src/lib/state/slice/corporate/accounting/accounting-chart-of-account-slice.jsx` | 176 |
| Konstanta | `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | 376 |
| Utils | `src/utils/corporate/accounting/chart-of-account/chart-of-account-utils.jsx` | 224 |
| Hook daftar | `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account.jsx` | 335 |
| Hook form | `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-editor.jsx` | 295 |
| Hook rincian | `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-detail.jsx` | 124 |
| View daftar | `src/components/view/corporate/accounting/chart-of-account/chart-of-account-view.jsx` | 380 |
| View form | `.../chart-of-account/form/chart-of-account-form-view.jsx` | 47 |
| View rincian | `.../chart-of-account/detail/chart-of-account-detail-view.jsx` | 60 |
| Gaya | `src/style/corporate/accounting/chart-of-account-view.module.css` | 105 |
| Rute | `src/app/corporate/accounting/chart-of-accounts/` — 5 berkas | 79 |

Diubah: `src/lib/state/store.jsx` (import + reducer), `src/utils/menu-sidebar/menu-items.jsx`
(butir menu), `src/lib/constants/corporate/accounting/accounting-constants.jsx`
(`available: true` + `href`).

## 5. Validasi

| Perintah | Hasil |
|---|---|
| `npm run lint:errors` | **PASS**, exit 0 |
| `npm run build` | **PASS**, exit 0 |
| Unit test 434 | **PASS**, 0 gagal |
| Rute ter-build | `/corporate/accounting/chart-of-accounts` beserta `create`, `[slug]`, `[slug]/update` — keempatnya ada di `app-paths-manifest.json` |

## 6. Verifikasi manual — `MANUAL TEST: NOT FEASIBLE`

Sesi ini tidak memiliki kredensial, dan mengambilnya dari `.env` dilarang aturan keselamatan
lingkungan. Seluruh acceptance yang menuntut sesi login **tidak** saya klaim lulus.

### Skrip uji untuk owner

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| 1 | Buka **Akuntansi → Master Data → COA** | Tabel tampil, pemilih badan hukum sudah terisi pilihan dari `FE-ACC-001` |
| 2 | Tekan **+ Tambah Akun**, isi kode `1-1001`, nama `Kas Besar`, jenis **Aset**, saldo normal **Debit**, centang **Menerima Transaksi**, simpan | `200`, kembali ke daftar, akun muncul |
| 3 | **Inti acceptance (2).** Tambah akun lagi dengan kode `1-1001` | Ditolak, toast memuat pesan backend yang menyebut kodenya |
| 4 | Tambah akun induk `1-0000` **tanpa** centang Menerima Transaksi, lalu tambah anak dengan induk itu | Keduanya tersimpan |
| 5 | Buka tampilan **Susunan Induk-Anak** | `1-0000` tampil dengan `1-1001` menjorok di bawahnya |
| 6 | Susun jurnal yang memakai `1-1001` sampai **disahkan**, lalu kembali ke daftar akun dan tekan **Nonaktifkan** pada akun itu | **Inti acceptance (3).** Ditolak, toast memuat jumlah saldonya |
| 7 | Nonaktifkan akun yang belum bersaldo, isi alasan | Berhasil, status menjadi **Nonaktif**, tombol berubah menjadi **Aktifkan** |
| 8 | Klik dua kali sebuah baris | Halaman rincian terbuka, memuat penanda **Punya Akun Turunan** dan **Sudah Dipakai Jurnal Disahkan** |
| 9 | Perkecil jendela selebar ponsel | Kartu konteks dan tombol menjadi satu kolom |

## 7. Delta terhadap kontrak

**Nol delta perilaku.** Dua catatan bentuk:

1. Daftar DTO `ACC-API-0.3` memakai akhiran `Dto`, source memakai `Request`/`Response` —
   `ACC-GAP-004`, sudah tercatat.
2. `UpdateChartOfAccountRequest` tidak memuat `LegalEntityId`, `AccountType`, maupun
   `NormalBalance`. Layar mengikuti source: ketiganya hanya muncul saat menambah.

## 8. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Acceptance (1)–(3) belum dilihat berjalan | **Sedang** | Skrip bagian 6 menutupnya dalam sekitar sepuluh menit |
| Nol test otomatis | Sedang | Repository tidak punya pola test komponen React; 434 test yang ada seluruhnya menguji helper murni |
| Susunan pohon dibatasi enam tingkat | Rendah | Penjaga terhadap data melingkar. Kamus data membatasi tingkat akun 1–5, jadi batas ini tidak pernah menggigit data yang sah |

## 9. Langkah berikutnya

`FE-ACC-005` daftar jurnal, atau jalankan skrip bagian 6 supaya task ini naik ke `DONE`.
