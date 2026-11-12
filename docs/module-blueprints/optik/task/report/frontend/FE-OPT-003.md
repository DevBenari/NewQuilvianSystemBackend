# Laporan Perubahan Frontend — `FE-OPT-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-003` |
| Judul | Lapisan data pesanan: pesanan, status bayar, keluhan, katalog, pengaturan, laporan, diskon Billing; format rupiah dan tanggal WIB |
| Slice | Lapisan data (paralel dengan backend), prasyarat `FE-OPT-020`, `021`, `022`, `030`, `031`, `032` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-002` / `FE-OPT-003` |
| Trace | `03-frontend-architecture.md` bagian 3–4; `OPT-DEC-021`, `027`, `028`, `035`, `044`, `067`, `075`, `076`; screen brief `05-screen-brief.md` (approved 2026-11-04, r2) bagian 5.7–5.12 |
| Contract version | `OPT-API-v1` grup Opt Order, Opt Warranty Claim, Opt Report, Opt Reference Data (katalog, profil, pengaturan) — `approved`, terkunci hash; delta `OPT-API-v1.1` dari `BE-OPT-054` ✅ (bentuk respons dari [laporan `BE-OPT-054`](../backend/BE-OPT-054.md) bagian 4); Billing `POST invoices/{id}/discounts` as-is |
| Wewenang UI | `NOT APPLICABLE` — task tanpa layar |
| Dependency | `BE-OPT-005` ✅, `032` ✅, `034` ✅, `040` ✅, `041` ✅, `042` ✅, `050` ✅, `052` ✅, `054` ✅; `FE-OPT-002` ✅ (pola dan utilitas bersama) |
| Klasifikasi | `MEDIUM` — 1 service, 3 slice baru, 1 slice diperluas, 1 utilitas format, perluasan konstanta, registrasi store, 1 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `src/lib/{constants,services,state}/…/optical-management/`, `src/utils/health-services/optical-management/`, `src/lib/state/store.jsx`, `tests/unit/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `2bb756de7` (branch `YogaV2`, upstream `origin/YogaV2`) + `FE-OPT-002` di working tree |
| Commit backend yang dijadikan rujukan | `37ae965c` (branch `yoga`) + `BE-OPT-054` di working tree |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi (uji unit, lint, build). Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

`FE-OPT-002` sudah menyediakan lapisan data klinis dan aturan baca galat bersama. Untuk pesanan, keluhan, katalog,
pengaturan, dan laporan belum ada pemanggil sama sekali. Format rupiah dan tanggal WIB yang ditunda dari `FE-OPT-001`
juga belum ada.

Fakta dari backend dan frontend yang menentukan bentuk lapisan ini:

| Fakta | Bukti |
| --- | --- |
| Bentrok versi pesanan memakai `409` `OPT-VAL-058`; keluhan dan pengaturan memakai `OPT-CONCURRENCY` | `OptOrderService`, `OptWarrantyClaimService`, `OptReferenceDataService` |
| Status tidak sesuai `OPT-VAL-057` dan pengajuan batal menunggu `OPT-VAL-055` juga `409`, tetapi bukan basi | `OptOrderService` |
| Endpoint pesanan tidak membaca kunci idempoten | `OptOrderController` (tanpa `FromHeader`) |
| Diskon diajukan di Billing; thunk `applyBillingDiscount`, `cancelBillingDiscount`, dan `getBillingInvoiceById` sudah ada | `billing-invoice-slice.jsx` |
| Baris tagihan barang Optik: `sourceDomain` `OPTICAL`, `sourceDetailId` = id baris pesanan | `InvoiceItemResponse`; uji `BE-OPT-054` |
| `formatCurrencyIDR` sudah ada di `src/utils/Formatters.jsx` | Repository |

---

## 2. Proses bisnis dari sisi pengguna

Task ini tidak punya layar. Yang disiapkan untuk layar pesanan, keluhan, Master Data, dan laporan:

1. **Petugas menyimpan draf, melihat perkiraan harga, lalu menagihkan.** Perkiraan diambil dari backend dan disimpan
   terpisah dari galat aksi. **Contoh:** frame Rp 450.000 + lensa Rp 600.000 → perkiraan Rp 1.050.000. Bila draf
   ternyata sudah ditagihkan, pesan server *"Harga pesanan yang sudah ditagihkan dibaca dari tagihan."* muncul di area
   perkiraan, bukan sebagai kegagalan tombol. Sesudah aksi apa pun berhasil, perkiraan lama dibuang.
2. **Setiap tombol aksi pesanan** (Ubah, Tagihkan, Kirim ke Pengerjaan, Tandai Siap, Serahkan, Batal, Ajukan Batal,
   Putuskan Batal, Tutup Tidak Diambil) mengirim versi pesanan yang terakhir dilihat. Tekan dua kali hanya mengirim
   satu permintaan. Sesudah berhasil, status bayar dimuat ulang karena kasir dapat mengubahnya kapan saja.
3. **Bila pesanan sudah diubah petugas lain** (`OPT-VAL-058`), detail dan status bayar dimuat ulang otomatis, lalu
   pesan server tampil. Penolakan lain diteruskan apa adanya tanpa muat ulang, misalnya *"Uang muka belum memenuhi
   minimum Rp 525.000."*, *"Masih ada pengajuan batal yang menunggu keputusan."*, atau `503` saat Billing tidak
   terjangkau.
4. **Kepala Optik menyaring daftar pesanan** dengan "menunggu persetujuan batal" dan "lewat batas ambil". Empat tanda
   baris (terlambat, perlu dihubungi, menunggu keputusan batal, lewat batas ambil) tersimpan apa adanya dari server.
5. **Keluhan garansi**: catat, periksa ulang, dan putuskan. Periksa ulang dan putuskan membawa versi; bentrok memuat
   ulang detail keluhan.
6. **Master Data**: katalog barang (ubah profil langsung memperbarui baris katalog yang sama) dan pengaturan (bentrok
   versi memuat ulang pengaturan).
7. **Laporan**: empat tab menyimpan datanya masing-masing. Periode tidak sah, mis. *"Periode laporan paling panjang
   366 hari."*, hanya tampil di tab itu.
8. **Ajukan Diskon** memakai thunk Billing yang sudah ada. Baris tagihan untuk tiap barang dicari dengan
   `matchOpticalInvoiceLines`.
9. **Format**: `Rp 1.050.000`; harga kosong tampil "—" (tarif belum ada, bukan gratis); tanggal `dd-MM-yyyy` dan jam
   `HH.mm` menurut WIB. **Contoh:** `2026-11-03T17:30:00Z` tampil `04-11-2026`; perkiraan jadi `2026-11-05` tampil
   `05-11-2026` tanpa digeser zona.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Pola `FE-OPT-002` (`optical-constants.js`, `optical-request-rules.js`, slice klinis); `billing-invoice-slice.jsx` (thunk
diskon dan detail tagihan); `src/utils/Formatters.jsx`; `store.jsx`. Backend (baca-saja): `OptOrderController`,
`OptWarrantyClaimController`, `OptReportController`, `OptReferenceDataController`, DTO terkait, `OptOrderService`,
`OptWarrantyClaimService`, `OptReferenceDataService`, `InvoiceItemResponse`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/optical-management/optical-constants.js` | **Diperbarui.** Endpoint `orders`, `warrantyClaims`, `reports`; penyaring bawaan pesanan, keluhan, katalog; `OPTICAL_INVOICE_SOURCE_DOMAIN`; `OPT-VAL-058` ditambahkan ke kode basi |
| `src/lib/services/health-services/optical-management/optical-order.service.js` | **Baru.** 27 fungsi endpoint: pesanan 14 (termasuk `price-preview`), keluhan 5, laporan 4, katalog/profil/pengaturan 4 |
| `src/lib/state/slice/health-services/optical-management/optical-order-slice.jsx` | **Baru.** Daftar + penyaring v1.1, detail, status bayar, perkiraan harga, buat, dan 9 aksi ber-versi |
| `src/lib/state/slice/health-services/optical-management/optical-warranty-claim-slice.jsx` | **Baru.** Daftar, detail, catat, periksa ulang, putuskan |
| `src/lib/state/slice/health-services/optical-management/optical-report-slice.jsx` | **Baru.** Empat laporan, satu bagian state per tab |
| `src/lib/state/slice/health-services/optical-management/optical-reference-data-slice.jsx` | **Diperbarui.** Metadata tetap (perilaku `FE-OPT-002` tidak berubah) + katalog, ubah profil, pengaturan |
| `src/utils/health-services/optical-management/optical-format-rules.js` | **Baru.** `formatOpticalRupiah`, `formatOpticalDateWib`, `formatOpticalDateTimeWib`, `todayOpticalWib`, `matchOpticalInvoiceLines` |
| `src/lib/state/store.jsx` | **Diperbarui.** Reducer `opticalOrder`, `opticalWarrantyClaim`, `opticalReport` (CRLF dipertahankan) |
| `tests/unit/optical-order-slices.test.mjs` | **Baru.** 14 uji dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alurnya sama dengan `FE-OPT-002`: `slice → service → InstanceAxios`, endpoint hanya di berkas konstanta, `signal`
diteruskan, utilitas murni di `utils`. Komponen yang dipakai ulang tanpa menduplikasi:

- **Diskon:** thunk Billing yang ada dipakai, tanpa service diskon kedua.
- **Rupiah:** `formatCurrencyIDR` dibungkus agar nilai kosong tampil "—".
- **Galat:** pembaca galat Optik dari `FE-OPT-002`.

Tidak ada pola baru selain yang sudah dijelaskan di laporan `FE-OPT-002`.

---

## 4. State yang ditangani di layar

| State | Yang disediakan lapisan data untuk layar |
| --- | --- |
| Memuat | `listLoading`, `detailLoading`, `paymentStatusLoading`, `pricePreviewLoading`, `actionLoading`, `catalogLoading`, `settingLoading`, `loading` per laporan |
| Kosong | Daftar selalu larik dan paginasi berbentuk tetap; laporan `data: null` sebelum dimuat |
| Gagal | Pesan server apa adanya + status + kode, per bagian (daftar, detail, status bayar, perkiraan, aksi, laporan per tab) |
| Tanpa hak akses | `403` tersimpan dengan status dan pesannya pada bagian yang memintanya |
| Data basi | `actionStale: true` sesudah detail (dan status bayar untuk pesanan) dimuat ulang |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/orders` | Daftar + penyaring (termasuk `hasPendingCancellation`, `pastNotCollectedLimit`) | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}` | Detail | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}/payment-status` | Status bayar | `OptOrder : Read` |
| `GET` | `/v1/health-services/optical-management/orders/{id}/price-preview` | Perkiraan harga draf | `OptOrder : Read` |
| `POST` | `/v1/health-services/optical-management/orders` | Buat draf | `OptOrder : Create` |
| `PUT` | `/v1/health-services/optical-management/orders/{id}` | Ubah draf | `OptOrder : Update` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/bill` | Tagihkan | `OptOrder : Bill` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/send-to-work` | Kirim ke pengerjaan | `OptOrder : SendToWork` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/mark-ready` | Tandai siap | `OptOrder : MarkReady` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/hand-over` | Serahkan | `OptOrder : HandOver` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancel` | Batal sebelum pengerjaan | `OptOrder : Cancel` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancellation-requests` | Ajukan batal | `OptOrder : RequestCancel` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/cancellation-requests/{requestId}/decision` | Putuskan batal | `OptOrder : DecideCancel` |
| `POST` | `/v1/health-services/optical-management/orders/{id}/close-not-collected` | Tutup tidak diambil | `OptOrder : CloseNotCollected` |

#### Health Services / Optical Management / Opt Warranty Claim

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/warranty-claims` | Daftar keluhan | `OptWarrantyClaim : Read` |
| `GET` | `/v1/health-services/optical-management/warranty-claims/{id}` | Detail | `OptWarrantyClaim : Read` |
| `POST` | `/v1/health-services/optical-management/warranty-claims` | Catat keluhan | `OptWarrantyClaim : Create` |
| `POST` | `/v1/health-services/optical-management/warranty-claims/{id}/examination` | Periksa ulang | `OptWarrantyClaim : Examine` |
| `POST` | `/v1/health-services/optical-management/warranty-claims/{id}/decision` | Putuskan | `OptWarrantyClaim : Decide` |

#### Health Services / Optical Management / Opt Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reports/visits-by-examiner` | Tab Kunjungan per Pemeriksa | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/sales` | Tab Penjualan | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/open-orders` | Tab Pesanan Terbuka | `OptReport : Read` |
| `GET` | `/v1/health-services/optical-management/reports/warranty` | Tab Keluhan & Garansi | `OptReport : Read` |

#### Health Services / Optical Management / Opt Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/item-catalog` | Katalog barang | `OptReferenceData : Read` |
| `PUT` | `/v1/health-services/optical-management/reference-data/item-profiles/{drugId}` | Ubah profil barang | `OptReferenceData : Update` |
| `GET` | `/v1/health-services/optical-management/reference-data/settings` | Pengaturan | `OptReferenceData : Read` |
| `PUT` | `/v1/health-services/optical-management/reference-data/settings` | Ubah pengaturan | `OptReferenceData : UpdateSetting` |

#### Billing (dipakai ulang, tanpa perubahan)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/billing-management/billing/invoices/{id}` | Baris tagihan + harga sesudah ditagihkan (`getBillingInvoiceById`) | Izin baca tagihan Billing |
| `POST` | `/v1/health-services/billing-management/billing/invoices/{id}/discounts` | Ajukan diskon (`applyBillingDiscount`) | `BillingDiscount : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-order-slices.test.mjs` | 14 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Method + path 27 endpoint | Sama persis dengan controller backend | `PASS` | Uji 1 |
| Penyaring v1.1 dan empat tanda | Parameter terkirim, nilai kosong dibuang, tanda tersimpan | `PASS` | Uji 2 |
| `expectedVersion` | Dikirim di 9 aksi pesanan; 12 aksi tanpa versi (pesanan, keluhan, pengaturan) ditolak sebelum mengirim | `PASS` | Uji 3 |
| Sesudah aksi berhasil | Status bayar dimuat ulang, baris daftar diperbarui, perkiraan lama dibuang | `PASS` | Uji 4 |
| Klik ganda Tagihkan | Satu permintaan | `PASS` | Uji 5 |
| `409` basi | `OPT-VAL-058` → `GET` detail + status bayar, `actionStale`; keluhan/pengaturan `OPT-CONCURRENCY` → muat ulang masing-masing | `PASS` | Uji 6, 8 |
| `409` bukan basi, `400`/`403`/`503` | Tanpa muat ulang; pesan, status, dan kode diteruskan | `PASS` | Uji 7 |
| Perkiraan harga | Hasil (termasuk tarif kosong) tersimpan; `409` sesudah ditagihkan masuk galat pratinjau, bukan galat aksi | `PASS` | Uji 9 |
| Katalog, laporan | Profil memperbarui baris sama; laporan per tab, galat periode hanya di tab itu | `PASS` | Uji 10, 11 |
| Format | Rupiah, "—" untuk kosong, tanggal WIB lintas tengah malam, tanggal saja tidak bergeser | `PASS` | Uji 12 |
| Pasangan baris tagihan dan diskon | Dipasangkan per `sourceDetailId` tanpa beda huruf; thunk Billing mengirim `invoiceItemId` yang benar | `PASS` | Uji 13, 14 |
| Uji mutasi: `OPT-VAL-058` dibuang dari kode basi / muat ulang status bayar dihapus | Uji 6 dan uji 4 gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| Uji Optik bersama (`FE-OPT-001`, `002`, `003`) | 49 lulus | `PASS` | Keluaran perintah |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.282 lulus, 72 gagal dari 4.358; daftar gagal **identik** dengan sebelum task (`diff` kosong), seluruhnya modul lain | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar `not ok` |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 67s`; 566 halaman statis; standalone siap; exit 0 | `PASS` | Keluaran build |

Uji manual: `NOT APPLICABLE` — task tanpa layar. Perilaku di layar diuji pada `FE-OPT-020`..`032`.

**Tidak dijalankan:** `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh); E2E
(belum ada layar).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Service Axios per grup `[Tags]` dan slice Redux | Terpenuhi | Bagian 3.2, 5 |
| Header `Idempotency-Key` per niat aksi | `NOT APPLICABLE` untuk grup ini: endpoint pesanan, keluhan, dan referensi tidak membaca kunci. Pengulangan dijaga `expectedVersion`, penahan klik ganda, dan keadaan pesanan di backend | Uji 3, 5 |
| `ExpectedVersion` dikirim | Terpenuhi | Uji 3 |
| Galat `400/403/409/503` dipetakan ke pesan backend | Terpenuhi | Uji 6–9, 11 |
| `409` memicu muat ulang | Terpenuhi (basi saja) | Uji 6, 8 |
| Format rupiah dan tanggal WIB (dari `FE-OPT-001`) | Terpenuhi | Uji 12 |
| DoD: uji unit lulus | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Membuat pesanan dan mencatat keluhan tidak punya kunci idempoten di backend; hanya dijaga dari klik ganda. Bila jaringan putus sesudah server menyimpan, kiriman ulang dapat membuat draf atau keluhan kedua (catatan backend yang sama dengan resep di `FE-OPT-002`) |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal sejak sebelum task (bagian 6) |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Dua mutasi uji dibuat dan dipulihkan |
| Interupsi | `NONE` |
| Status Git | `M src/lib/state/store.jsx`; `??` folder Optik baru (konstanta, service, slice, utilitas) dan dua berkas uji Optik |
| Langkah berikutnya | `FE-OPT-010` (menu Optik, daftar kunjungan, pendaftaran, ringkasan kunjungan) |
