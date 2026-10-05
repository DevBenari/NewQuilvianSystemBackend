# Laporan Perubahan Frontend — `FE-FIN-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-017` |
| Judul | Tujuh layar pandangan tersaring rumpun A/R terjangkau dari menu |
| Slice | `REV-13A` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`; `FIN-DES-073`; `03-frontend-architecture.md` §17.2, §17.3 |
| Contract version | `FIN-API-1.4` — endpoint `GET /receivable-invoice-batches`, `GET /receivables`, `GET /receipts/register`, seluruhnya `approved`, tidak ada endpoint baru dikonsumsi |
| Wewenang UI | `FIN-DEC-094` approved product brief — mengunci layar mana yang berdiri sendiri dan saringan bawaan mana yang terkunci (§17.3). Tata letak, warna, ikon tetap `DEV_DISCRETION` |
| Dependency | `FE-FIN-016` 🟡 (grup menu "Transaksi A/R" harus sudah berdiri) — source lengkap dan lint PASS, build belum dikonfirmasi; dilanjutkan atas arahan eksplisit pengguna |
| Klasifikasi | `HEAVY` — satu repository (skor 0); 20 berkas baru + 1 diubah (skor 2); 5 hook baru + 7 view baru (skor 2); nol endpoint baru dikonsumsi, seluruhnya endpoint existing (skor 0); database — tidak relevan (skor 0); keamanan/auth — nol resource/action baru, dua koreksi permission pada butir existing (skor 1); UI/workflow — 7 layar baru (skor 2). Total 7 → `HEAVY` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk memverifikasi `[Route]`/`[AccessPermission]` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/app/finance/**`, `src/components/view/finance/**`, `src/lib/hooks/finance/receivable/**`, `src/utils/menu-sidebar/menu-items.jsx`, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `99df41e1` — branch `Yasmina` (read-only) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak, `npm run lint:errors` **PASS**. `npm run test:unit`/`npm run build` **NOT RUN** — menunggu konfirmasi eksplisit pengguna |

---

## 1. Keadaan yang ditemukan di awal

`FE-FIN-016` membangun grup menu "Transaksi A/R" dengan 7 butir yang rutenya sudah ada, tetapi 7
butir lain pada peta menu V1 (§17.2) tidak terjangkau sama sekali — belum ada satu pun halaman
untuk Canceled Invoice, Report Canceled Invoice, Report Receiveable AR, Report Payment AR, Report
Closed Billing, Report AR Created, dan Piutang Korporat/Penjamin, walaupun ketiga endpoint yang
mereka butuhkan (`GET /receivable-invoice-batches`, `GET /receivables`, `GET /receipts/register`)
sudah ada dan berfungsi sejak `FE-FIN-012`/`FE-FIN-002`/`BE-FIN-050`.

**Temuan penting sebelum implementasi dimulai (bukan dugaan, dibuktikan dari source):** dua butir
menu yang didaftarkan `FE-FIN-016` — "Receivable AR/Invoice" dan "Piutang Tagihan" — ternyata
memakai `requiredPermission` yang **tidak cocok** dengan controller yang sesungguhnya dipanggil
halaman `/finance/receivable`. Rinciannya di §3.3. Ini dikoreksi sebagai bagian task ini karena
ditemukan langsung di area yang sedang dikerjakan, bukan dicari-cari di luar cakupan.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Petugas AR Finance, Supervisor/Manajer Finance (seluruhnya **baca saja** — ketujuh
layar ini murni laporan/pandangan tersaring, nol aksi tulis).

**Pemicu:** Pengguna membuka salah satu dari tujuh butir baru pada grup "Transaksi A/R".

**Langkah normal (sama untuk ketujuhnya):**

1. Layar terbuka, saringan bawaan (bila ada) sudah terkunci dan langsung terkirim ke backend —
   pengguna **tidak** melihat kontrol untuk mengubah saringan yang terkunci itu.
2. Pengguna boleh menambah saringan bebas (penjamin, status, tipe debitur, rentang tanggal, atau
   kata kunci pencarian — tergantung layar) di atas saringan bawaan.
3. Tabel menampilkan data sesuai gabungan saringan terkunci + saringan bebas, dengan paginasi di
   backend.
4. Pengguna dapat mereset saringan bebas (saringan terkunci **tidak ikut tereset** — selalu
   kembali ke nilai kuncinya, bukan ke kosong).

**Jalur tidak normal:** Kosong → kalimat baku per layar (mis. "Tidak ada Batch Tagihan AR yang
dibatalkan."); gagal → `InformationAlert` beserta pesan error dari backend; tanpa hak akses →
`AccessDeniedGate` menggantikan seluruh isi halaman.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `03-frontend-architecture.md` §17.2 (tabel butir menu persis, pathname, hak akses) dan §17.3
  (bentuk skematik "digambar sekali, dipakai sembilan layar", tabel saringan bawaan terkunci)
- `src/components/view/finance/receivable/invoice-batch/receivable-invoice-batch-view.jsx`,
  `*-table-columns.jsx` — modul referensi visual untuk Canceled Invoice/Report Canceled Invoice
- `src/components/view/finance/receivable/finance-receivable-view.jsx`,
  `finance-receivable-table-columns.jsx` — modul referensi visual untuk keempat layar `/receivables`
- `src/components/view/finance/receivable/receipt/register/finance-receipt-register-view.jsx`,
  `*-table-columns.jsx`, `use-finance-receipt-register.jsx` — modul referensi **dan** sumber
  REUSE langsung untuk Report Payment AR
- `src/components/view/finance/purchasing/reports/due-date-report-view.jsx`,
  `use-purchasing-reports.jsx` — pola "locked default filter + free filter" pada hook state lokal
  (bukan Redux), dipakai sebagai acuan arsitektur hook baru
- `src/lib/constants/finance/receivable/receivable-constants.jsx`,
  `receivable-invoice-batch-constants.jsx`, `finance-receipt-constants.jsx` — nilai enum,
  endpoint base, dan opsi filter yang sudah ada
- `src/lib/hooks/finance/receivable/use-finance-receivable.jsx` — dibaca untuk memastikan
  `RECEIVABLE_CONFIG.endpoint` ("/finance/receivable") adalah target API `fetchReceivables`,
  bukan sekadar nilai `routeBase`
- `NewQuilvianSystemBackend/Areas/.../FinanceArController.cs` (read-only) — dibaca penuh untuk
  membuktikan `[Route("api/finance/receivable")]` dan `[AccessPermission("Finance.AR","View")]`
  adalah controller nyata yang dipanggil `/finance/receivable`, terpisah dari
  `FinanceReceivablesController` governed
- `NewQuilvianSystemBackend/.../FinanceReceivableService.cs` (read-only, dibaca pada sesi
  sebelumnya) — dikonfirmasi ulang `SortBy` pada `GetPagedAsync` tidak punya cabang untuk tanggal
  terbit/`recognizedAt` (lihat §3.3 dan §8)
- `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` — dikonfirmasi ulang
  (sama seperti `FE-FIN-016`) nol dampak dari penambahan 7 butir baru

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/finance/receivable/use-canceled-receivable-invoice-batch-list.jsx` **(baru)** | Hook state lokal — `GET /receivable-invoice-batches` dengan `status` terkunci `CANCELLED`; dipakai **dua** layar (Canceled Invoice dan Report Canceled Invoice) |
| `src/lib/hooks/finance/receivable/use-ar-report-receivable-list.jsx` **(baru)** | Hook state lokal — `GET /v1/corporate/finance-management/receivables` (governed), nol saringan terkunci |
| `src/lib/hooks/finance/receivable/use-closed-billing-receivable-list.jsx` **(baru)** | Sama, `status` terkunci `SETTLED` |
| `src/lib/hooks/finance/receivable/use-created-receivable-list.jsx` **(baru)** | Sama, nol saringan terkunci — dimaksudkan "diurutkan tanggal terbit" tetapi backend tidak mendukungnya (lihat §3.3) |
| `src/lib/hooks/finance/receivable/use-corporate-receivable-list.jsx` **(baru)** | Sama, `debtorType` terkunci `PAYER` |
| `src/components/view/finance/receivable/invoice-batch/canceled-invoice-batch-view.jsx` **(baru)** | Layar "Canceled Invoice" — bentuk operasional, klik dua kali baris membuka rincian batch |
| `src/components/view/finance/ar-report/report-canceled-invoice-view.jsx` **(baru)** | Layar "Report Canceled Invoice" — data identik di atas, bentuk laporan murni (nol interaksi klik-ke-rincian) |
| `src/components/view/finance/ar-report/report-receivable-view.jsx` **(baru)** | Layar "Report Receiveable AR" |
| `src/components/view/finance/ar-report/report-closed-billing-view.jsx` **(baru)** | Layar "Report Closed Billing" |
| `src/components/view/finance/ar-report/report-created-view.jsx` **(baru)** | Layar "Report AR Created" — kolom "Tanggal Terbit" ditambahkan ke `buildReceivableColumns()` dasar |
| `src/components/view/finance/ar-report/report-payment-view.jsx` **(baru)** | Layar "Report Payment AR" — REUSE penuh `useFinanceReceiptRegister` dan `buildFinanceReceiptRegisterColumns` dari `FE-FIN-004`/`BE-FIN-050`, bingkai presentasi laporan |
| `src/components/view/finance/receivable/corporate/corporate-receivable-view.jsx` **(baru)** | Layar "Piutang Korporat/Penjamin" |
| `src/app/finance/receivable-invoice-batches/canceled/page.jsx` **(baru)** | Route tipis untuk Canceled Invoice |
| `src/app/finance/ar-report/canceled-invoice/page.jsx` **(baru)** | Route tipis untuk Report Canceled Invoice |
| `src/app/finance/ar-report/receivable/page.jsx` **(baru)** | Route tipis untuk Report Receiveable AR |
| `src/app/finance/ar-report/payment/page.jsx` **(baru)** | Route tipis untuk Report Payment AR |
| `src/app/finance/ar-report/closed-billing/page.jsx` **(baru)** | Route tipis untuk Report Closed Billing |
| `src/app/finance/ar-report/created/page.jsx` **(baru)** | Route tipis untuk Report AR Created |
| `src/app/finance/receivable/corporate/page.jsx` **(baru)** | Route tipis untuk Piutang Korporat/Penjamin |
| `src/utils/menu-sidebar/menu-items.jsx` | 7 butir baru disisipkan ke grup "Transaksi A/R" pada posisi persis urutan V1 (§17.2); **dua butir existing dikoreksi** ("Receivable AR/Invoice", "Piutang Tagihan") — lihat §3.3 |

Nol berkas backend disentuh — seluruhnya `FRONTEND MODE`, backend dibaca read-only.

### 3.3 Kepatuhan arsitektur frontend

**Alur dependensi.** Seluruh 7 layar mengikuti `src/app -> components/view -> lib/hooks ->
InstanceAxios -> Backend API`, tanpa pembalikan. `page.jsx` hanya metadata dan satu baris render.

**Base component — seluruhnya `REUSE`, nol `NEW`/`EXTEND`:**

| Elemen | Status | Sumber |
| --- | --- | --- |
| `AccessDeniedGate`, `DataFilter`, `DataTable`, `FilterSelect`, `FilterDatePicker`, `ResourceFilterSelect`, `Hero`, `InformationAlert`, `ToastStack`, `Pagination`, `FinanceBreadcrumb` | `REUSE` | `src/components/features/base-features/**`, `src/components/view/finance/shared/**` — dipakai apa adanya, nol prop baru di luar yang sudah didukung |
| `StatusBadge` (lewat `buildReceivableInvoiceBatchColumns`/`buildReceivableColumns`) | `REUSE` | Dipakai tidak langsung lewat kolom yang di-reuse, nol pemanggilan baru |

Karena seluruh elemen `REUSE`, gerbang keputusan (langkah 3 skill) **tidak menghasilkan pilihan
bernomor apa pun** — tidak ada keputusan yang menunggu user pada lapisan komponen.

**Reuse data-layer yang dilakukan secara eksplisit** (bukan sekadar komponen UI):

- `buildReceivableInvoiceBatchColumns` (`FE-FIN-012`) dipakai ulang persis oleh **dua** layar baru
  (Canceled Invoice, Report Canceled Invoice) — nol duplikasi definisi kolom.
- `buildReceivableColumns` (`FE-FIN-002`) dipakai ulang persis oleh **tiga** layar baru (Report
  Receiveable AR, Report Closed Billing, Piutang Korporat/Penjamin), dan **diperluas** (bukan
  diduplikasi) oleh layar keempat (Report AR Created, menambah satu kolom "Tanggal Terbit" lewat
  `splice` atas array yang sama).
- `useFinanceReceiptRegister` **dan** `buildFinanceReceiptRegisterColumns` (`FE-FIN-004` lanjutan,
  `BE-FIN-050`) dipakai ulang **seutuhnya, nol baris kode data-layer baru** untuk Report Payment
  AR — satu-satunya perbedaan dari layar "Buku Register Penerimaan" yang sudah ada
  (`/finance/receipts/register`, belum tertaut menu) adalah bingkai presentasi (Hero/breadcrumb
  laporan, nol tombol "Kembali ke Daftar Penerimaan").

**Lima hook baru** (bukan satu hook generik lintas-domain — tiap hook fokus pada satu
endpoint+saringan terkunci, mengikuti pola duplikasi yang sudah mapan di repo ini, dicontohkan
`use-receivable-invoice-batch-list.jsx` vs `use-finance-receipt-register.jsx` yang juga
nyaris identik strukturnya tetapi sengaja terpisah file) memakai state lokal (`useState`/
`useEffect`), **bukan** Redux — mengikuti pola `usePurchasingReports` (laporan Purchasing/AP),
karena ketujuh layar ini masing-masing berdiri sendiri dan tidak butuh state lintas halaman.

**Koreksi permission pada dua butir `FE-FIN-016` (ditemukan dan diperbaiki sebagai bagian task
ini):**

`03-frontend-architecture.md` §17.2 mencantumkan `requiredPermission: FinanceReceivable : Read`
untuk "Receivable AR/Invoice" (`FIN-LYR-AR-02`) dan "Piutang Tagihan" (`FIN-LYR-AR-17`), keduanya
menunjuk pathname `/finance/receivable`. **Dibuktikan salah** lewat pembacaan rantai sumber penuh:

```text
/finance/receivable (page.jsx)
  -> FinanceReceivablePage -> finance-receivable-view.jsx
  -> useFinanceReceivable() -> dispatch(fetchReceivables(filters))
  -> GET `${RECEIVABLE_CONFIG.endpoint}` = GET "/finance/receivable"
  -> Backend: FinanceArController, [Route("api/finance/receivable")]
  -> [AccessPermission("Finance.AR", "View")]   <- BUKAN "FinanceReceivable"
```

`FinanceArController` adalah controller **V2 legacy** (dikonfirmasi dari doc comment kelasnya
sendiri: "Endpoint API Finance Management V2") yang memanggil `FinanceReceivableService` yang
SAMA dengan `FinanceReceivablesController` governed, tetapi lewat route dan skema permission yang
berbeda — keduanya bukan endpoint yang sama. Halaman `/finance/receivable` (dibangun `FE-FIN-002`,
sebelum revisi ini) **belum pernah** dipindahkan ke controller governed, dan memindahkannya
**di luar cakupan** task ini (payload `POST /payment`/`POST /writeoff` pada controller V2 adalah
aksi langsung, sedangkan controller governed memakai maker-checker `RequestAdjustment`/
`RequestWriteOff` — model bisnis yang berbeda, bukan sekadar endpoint yang dipindah).

Dibiarkan memakai `FinanceReceivable : Read` berarti pengguna yang hanya memegang hak itu (tanpa
`Finance.AR : View`) akan **melihat butir menu tetapi mendapat `403` saat halaman memuat**, dan
pengguna yang hanya memegang `Finance.AR : View` (tanpa hak granular) **tidak akan melihat butir
menu sama sekali** walau sebenarnya berhak memakai halamannya. Diperbaiki ke `Finance.AR : View`,
sama dengan "Report AR"/"Laporan Aging AR" yang sudah benar memakai permission V2 ini. Dicatat di
`menu-items.jsx` sebagai komentar inline supaya alasannya tidak hilang pada task berikutnya.

Temuan ini **bukan** hasil penyimpangan kontrak secara bisnis — murni ketidaksesuaian dokumen
rancangan terhadap source yang sudah ada sebelum revisi ini ditulis, dan **MUST** dilaporkan, bukan
dilewatkan (sama seperti koreksi `ReversalReason` pada `BE-FIN-054` dan "KASIR" pada `BE-FIN-055`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `DataTable` menampilkan kerangka baris (`loading`/`loadingText`) sampai respons tiba |
| Kosong | Kalimat khusus per layar, mis. "Tidak ada Batch Tagihan AR yang dibatalkan.", "Tidak ada data pada saringan ini." |
| Gagal | `InformationAlert` variant `danger` menampilkan pesan error dari backend (lewat `getApiErrorMessage`) |
| Tanpa hak akses | `AccessDeniedGate` menggantikan seluruh isi halaman |

Keempat state ini identik polanya dengan layar referensi (`receivable-invoice-batch-view.jsx`,
`finance-receivable-view.jsx`, `finance-receipt-register-view.jsx`) — nol penanganan state baru
yang menyimpang dari pola existing.

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receivable-invoice-batches` | Canceled Invoice, Report Canceled Invoice (saringan `status=CANCELLED` terkunci) | `FinanceReceivableInvoiceBatch : Read` |

#### Corporate / Finance Management / Receivable

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receivables` | Report Receiveable AR (nol saringan terkunci), Report Closed Billing (`status=SETTLED` terkunci), Report AR Created (nol saringan terkunci), Piutang Korporat/Penjamin (`debtorType=PAYER` terkunci) | `FinanceReceivable : Read` |

#### Corporate / Finance Management / Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/receipts/register` | Report Payment AR (nol saringan terkunci) | `FinanceReceipt : Read` |

Seluruh tiga endpoint **sudah ada dan tidak diubah** oleh task ini — read-only dari sisi frontend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`eslint . --quiet`, exit code 0) | `PASS` | Keluaran perintah |
| `npm run test:unit` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| `npm run build` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| Review diff/scope | 20 berkas baru + 1 diubah, persis sesuai §17.2/§17.3 | `PASS` | `git status --short` |
| Review setiap import path (base-feature, hook, util, style) | Seluruhnya dikonfirmasi ada di disk lewat pemeriksaan file satu per satu | `PASS` | §3.1, pemeriksaan manual |
| Review saringan bawaan terkunci MUST dikirim ke backend, MUST NOT disaring di klien | Keempat hook dengan saringan terkunci (`status`/`debtorType`) menaruh nilai kunci langsung di objek `params` yang dikirim `InstanceAxios.get`, bukan memfilter array `items` sesudah respons tiba | `PASS` | Pembacaan source kelima hook baru |
| Grep anti-regresi UI (6 pola — warna literal, typography override, tombol non-base, tabel mentah, utility `fw-`/`fs-` dalam tabel, `!important`) | Nol temuan pada seluruh 7 file view baru | `PASS` | §3.3 tabel komponen; keluaran grep kosong di keenam pola |
| Review permission dua butir existing (`FE-FIN-016`) | Dibuktikan `Finance.AR : View` adalah permission yang benar, bukan `FinanceReceivable : Read` | `PASS` — delta dikoreksi | §3.3, pembacaan `FinanceArController.cs` penuh |
| QBE preflight (read-only, memverifikasi kontrak sisi backend) | Ketiga controller (`FinanceReceivableInvoiceBatchesController`, `FinanceReceivablesController`, `FinanceReceiptsController`) dan `FinanceArController` dibaca langsung dari source, bukan diasumsikan dari dokumen | `PASS` | §3.1 |

Uji manual: `NOT FEASIBLE` — server (`npm run dev`/`npm run build`) tidak dijalankan pada task
ini, sehingga ketujuh layar tidak dapat diverifikasi secara visual atau interaktif di browser.
Verifikasi dilakukan lewat pembacaan source, perbandingan terhadap modul referensi, dan grep
anti-regresi.

**AUTOMATED TEST: NOT APPLICABLE — repository ini tidak memakai Jest; menulis test baru bersifat
opsional (`rules/frontend/test-policy.md`), tidak diminta eksplisit pada task ini.**

**Tidak dijalankan:** `npm run test:unit`, `npm run build` (menunggu konfirmasi eksplisit
pengguna); `npm run test:e2e`/`npm run test:uat` (tidak diminta, environment server tidak aktif);
verifikasi manual/visual di browser (server tidak dijalankan).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-017`) | Status | Bukti |
| --- | --- | --- |
| Saringan bawaan tiap layar **dikirim ke backend**, bukan disaring di klien | Terpenuhi | §6 |
| Paginasi benar pada data lebih dari satu halaman | Terpenuhi (source) — pola `pageNumber`/`pageSize`/`totalData`/`totalPage` identik dengan hook referensi yang sudah terbukti benar paginasinya | Tidak diverifikasi secara runtime — server tidak dijalankan |
| Keempat keadaan (memuat/kosong/gagal/berisi) ada | Terpenuhi | §4 |
| Lint PASS | Terpenuhi | `npm run lint:errors` PASS |
| Build PASS | **Belum terpenuhi** | `npm run build` `NOT RUN`, menunggu konfirmasi pengguna |
| Butir menu terdaftar | Terpenuhi | §3.2, urutan diverifikasi lewat `grep` label dalam grup "Transaksi A/R" |
| Laporan task tracked ada | Terpenuhi | Laporan ini |

Task ini **belum** dapat ditandai ✅ selama `npm run build` belum dikonfirmasi pengguna.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` belum dijalankan sama sekali untuk task ini — bukan "dijalankan lalu gagal", melainkan sengaja tidak dijalankan mengikuti konvensi sesi saat ini. Risiko dinilai rendah untuk 7 file view/5 hook baru (lint PASS, seluruh import diverifikasi manual ada di disk), tetapi **tidak** dianggap terbukti sampai build benar-benar dijalankan |
| Masalah yang diketahui | **(1)** "Report AR Created" **tidak benar-benar terurut tanggal terbit** — `FinanceReceivableService.GetPagedAsync` (backend) tidak punya opsi `SortBy` untuk tanggal terbit/`recognizedAt`, jatuh ke default urut jatuh tempo. Kolom "Tanggal Terbit" tetap ditampilkan agar datanya terlihat, tetapi urutannya **MUST** dipahami sebagai keterbatasan backend, bukan cacat implementasi frontend — perlu task backend terpisah bila urutan sungguhan dibutuhkan. **(2)** Dua butir "Receivable AR/Invoice" dan "Piutang Tagihan" dikoreksi permission-nya (§3.3) — `03-frontend-architecture.md` §17.2 MUST diperbarui pada task dokumentasi berikutnya supaya tidak menyesatkan task `FE-FIN-019`+ yang membaca tabel yang sama. **(3)** Parameter `?view=billed` pada "Piutang Tagihan" tetap **inert** (tidak dibaca `finance-receivable-view.jsx` sama sekali) — temuan `FE-FIN-016`, bukan baru, dicatat ulang di sini karena relevan dengan area yang sama |
| Dependency backend | Tidak ada endpoint baru — seluruh 3 endpoint yang dirujuk sudah ada dan tidak disentuh task ini. `FE-FIN-016` (grup menu induk) masih 🟡, belum dikonfirmasi build — risiko ditanggung bersama |
| Perubahan sampingan | `NONE` — koreksi permission pada §3.3 adalah bagian langsung dari task ini (ditemukan di berkas yang sama yang sedang disunting), bukan perbaikan di luar cakupan |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan 20 berkas baru + 1 berkas diubah, seluruhnya sesuai §3.2 |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run build` dan mengonfirmasi hasilnya untuk `FE-FIN-016` **dan** `FE-FIN-017` sekaligus; (2) `FE-FIN-018` (`REV-13A`, berdiri sendiri, rumpun A/P) sebagai lanjutan tercepat; (3) Koreksi `03-frontend-architecture.md` §17.2 (permission dua butir) sebagai task dokumentasi kecil, disarankan digabung saat menutup `FE-FIN-016`/`017` |
