# Laporan Perubahan Frontend — `FE-BUI-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-001` |
| Judul | Filter Tanggal dan Default Data Layar Billing |
| Slice | Gelombang `MVP-30` — Revisi UI Billing: Perbaikan Logika Backend (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-001`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-001` — Filter Tanggal dan Default Data Layar Billing |
| Trace | `BUI-DEC-001` (Filter Tanggal Awal/Akhir), `BUI-DEC-002` (Default invoice hari ini status `OPEN`), `FR-BUI-001`, `FR-BUI-002` — keduanya `approved` 24 September 2026 |
| Contract version | `BIL-API-1.4` (**approved**, era sebelum revisi 1.6) — `GET /invoices` sudah menerima `StartDate`/`EndDate`/`Status` sebelum task ini; task ini murni memakainya dari sisi frontend, nol perubahan kontrak |
| Wewenang UI | `DEV_DISCRETION` — komponen date picker mengikuti konvensi project (sesuai roadmap). Seluruh elemen `REUSE` (lihat §3.3); tidak ada elemen `NEW`/`EXTEND` yang menunggu keputusan pengguna |
| Dependency | Tidak ada — task ini **tidak bergantung** pada `BE-BUI-001`/`002` (memakai endpoint dan field yang sudah ada sejak `BIL-API-1.4`, era sebelum revisi 1.6), dikonfirmasi eksplisit oleh roadmap |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 9–20 (skor 1: view, hook, constants, dua layar Billing lain sebagai referensi reuse, base component `FilterDatePicker`); berkas diubah 3 (skor 0); logika sederhana — default state dan satu validasi rentang tanggal, pola sudah ada di layar sebelah (skor 0); kontrak API memakai yang sudah ada (skor 1); database `NOT APPLICABLE` (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow satu halaman, cakupan filter saja (skor 1). Total skor 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/lib/hooks/health-services/billing-management/billing-invoices/**`, `QuilvianSystemFrontendDev/src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) — persis baseline SHA yang dicatat roadmap |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | 🟡 **SEBAGIAN — source selesai, validasi otomatis lulus, uji manual peramban belum.** Ketiga acceptance criteria terpenuhi lewat pembacaan kode dan verifikasi kontrak backend; `npm run lint:errors`, `npm run test:unit`, `npm run build` seluruhnya lulus. Uji manual network tab (bukti verifikasi yang diminta eksplisit oleh roadmap) **belum dijalankan** — tidak ada tool browser/E2E pada sesi ini — lihat §6 |

---

## 1. Keadaan yang ditemukan di awal

Layar Running Invoice (`billing-invoices-view.jsx`) sudah memiliki filter `search`, `period` (preset "Semua Periode"/"Hari ini"/dst — bukan tanggal eksplisit), `status`, dan `serviceType`, tetapi:

1. **Tidak ada default apa pun saat layar pertama kali dibuka.** `DEFAULT_BILLING_INVOICE_FILTERS` sebelumnya berisi `period: ""`, `status: ""` — kosong. Query pertama yang terkirim ke `GET /invoices` benar-benar tanpa filter tanggal maupun status, menampilkan **seluruh** invoice sepanjang masa dengan **seluruh** status (`OPEN`, `FINAL`, `CLOSED`, `SETTLED_BY_WRITE_OFF`) sekaligus — bukan pekerjaan hari ini yang relevan bagi kasir.
2. **Tidak ada input tanggal eksplisit.** Filter `period` hanya preset relatif ("Hari ini", "Bulan Ini", dst), tidak ada dua input tanggal bebas (mulai/akhir) seperti yang sudah tersedia di layar Billing lain dalam folder yang sama (`cashier-overview-view.jsx`, memakai `FilterDatePicker`).

Backend (`BillingInvoiceService.GetPagedAsync`, dikonfirmasi dari source) **sudah** menerima `StartDate`/`EndDate`/`Status` pada `BillingInvoiceQuery` sejak sebelum task ini — celahnya murni di frontend yang belum pernah mengirimkan default maupun input eksplisit untuk field itu.

---

## 2. Proses bisnis dari sisi pengguna

1. Kasir membuka menu Running Invoice (Billing Management → Running Invoice).
2. **Sebelum task ini**: layar langsung memuat seluruh invoice tanpa filter — daftar panjang, tidak fokus ke pekerjaan hari ini, dan kasir harus mengatur filter secara manual setiap kali membuka layar.
3. **Sesudah task ini**: layar dibuka dan **langsung** mengirim query `StartDate=EndDate=hari ini, Status=OPEN` — kasir langsung melihat invoice `OPEN` (aktif, belum final/closed) yang relevan untuk hari itu.
4. Kasir dapat mengganti tanggal lewat dua `FilterDatePicker` baru ("Tanggal Mulai", "Tanggal Akhir") — begitu salah satu diganti, query baru langsung menggantikan default, dikirim ke backend (bukan disaring di sisi klien atas data yang sudah termuat).
5. **Jalur tidak normal**: bila kasir memilih Tanggal Akhir yang lebih awal dari Tanggal Mulai, permintaan **tidak dikirim ke backend sama sekali** — pesan "Tanggal Mulai tidak boleh setelah Tanggal Akhir." tampil di atas tabel, dan tabel tetap menampilkan data hasil query valid terakhir sampai kasir memperbaiki rentang tanggalnya.
6. Filter `period` (preset relatif "Hari ini"/"Bulan Ini"/dst, sudah ada sebelum task ini) tetap berfungsi seperti semula dan tidak disentuh task ini — memilih preset itu tetap diprioritaskan backend di atas dua tanggal eksplisit baru (perilaku `ResolveDateRangeUtc` yang sudah ada, bukan perubahan task ini; lihat §3.3).
7. Tombol "Reset" pada `DataFilter` (sudah ada sebelum task ini) tetap mengembalikan **seluruh** filter ke kosong (tanpa tanggal/status) — bukan ke default hari ini/`OPEN`. Ini keputusan desain yang sengaja dipertahankan apa adanya (lihat §8).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx` (layar target)
- `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoices.js` (hook target)
- `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` (konstanta target)
- `src/components/view/health-services/billing-management/billing-invoices/cashier-overview/cashier-overview-view.jsx` dan `use-cashier-billing-overview.js` (layar Billing lain, pola filter tanggal + validasi rentang yang di-reuse)
- `src/components/features/base-features/filter-date-picker.jsx` (base component yang di-reuse)
- `src/utils/shared/date-picker-utils.jsx` (`toDateInputValue`, di-reuse untuk menghitung "hari ini")
- `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` (memastikan param apa pun diteruskan apa adanya ke Axios, tanpa transformasi nama)
- Backend (read-only, referensi kontrak): `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs` (`BillingInvoiceQuery`), `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` (`GetPagedAsync`, `ResolveDateRangeUtc`) — dibaca untuk memverifikasi `StartDate`/`EndDate`/`Status` sudah diterima backend dan bagaimana `period` berinteraksi dengannya, TIDAK diubah

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` | `DEFAULT_BILLING_INVOICE_FILTERS` mendapat dua field baru `startDate: ""`, `endDate: ""` — melengkapi bentuk objek filter (dipakai `handleReset`, tetap kosong/"tampilkan semua" sesuai perilaku Reset yang sudah ada) |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoices.js` | Ditambah fungsi `getDefaultBillingInvoiceFilters()` yang menghitung `status: "OPEN"`, `startDate`/`endDate` = hari ini (`toDateInputValue(new Date())`, di-reuse dari `date-picker-utils.jsx`) sebagai state awal `useState` (bukan lagi `DEFAULT_BILLING_INVOICE_FILTERS` polos). `buildListParams` menambahkan `startDate`/`endDate` ke query bila terisi. Ditambah `dateValidationError` (`useMemo`, pola identik `use-cashier-billing-overview.js`) yang menolak `EndDate < StartDate` dengan pesan yang sama persis; `useEffect` pemanggil `getBillingInvoices` diberi guard `if (dateValidationError) return;` sehingga request **tidak dikirim** saat rentang tidak valid. `dateValidationError` diekspos ke view |
| `src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx` | Import `FilterDatePicker` (base component, sudah dipakai `cashier-overview-view.jsx` di folder yang sama). Dua `FilterDatePicker` baru ("Tanggal Mulai", "Tanggal Akhir") ditambahkan pada baris filter, sebelum dropdown "Semua Periode" yang sudah ada. `dateValidationError` diterima dari hook dan ditampilkan lewat `InformationAlert variant="danger"` (pola identik `cashier-overview-view.jsx`) di atas tabel, di bawah alert error API yang sudah ada |

### 3.3 Kepatuhan arsitektur frontend

Seluruh elemen UI pada task ini **`REUSE`** murni — tidak ada component baru, tidak ada pola state/HTTP baru:

| Elemen | Status | Bukti |
| --- | --- | --- |
| Input tanggal (Mulai/Akhir) | `REUSE` | `FilterDatePicker` (`src/components/features/base-features/filter-date-picker.jsx`), sudah dipakai `cashier-overview-view.jsx` (folder yang sama) untuk kebutuhan identik |
| Alert validasi rentang tanggal | `REUSE` | `InformationAlert variant="danger"`, sudah dipakai `billing-invoices-view.jsx` sendiri (untuk `errorMessage`) dan `cashier-overview-view.jsx` (untuk `dateValidationError` dengan pesan persis sama) |
| Perhitungan "hari ini" (`YYYY-MM-DD`) | `REUSE` | `toDateInputValue` (`src/utils/shared/date-picker-utils.jsx`), fungsi yang sama dipakai `FilterDatePicker` sendiri secara internal |
| Pola validasi rentang tanggal + guard fetch | `REUSE` (pola, bukan kode bersama — disalin sesuai konvensi repository "ikuti implementasi terdekat") | `dateValidationError` (`useMemo`) + `if (dateValidationError) return;` pada `useEffect`, identik `use-cashier-billing-overview.js` baris 106-125 |
| State filter (`useState`, `updateFilter`, `buildListParams`) | `REUSE` | Pola hook existing `use-billing-invoices.js` sendiri, hanya ditambah dua field, tidak direstrukturisasi |

Tidak ada elemen `NEW`, `EXTEND`, `COMPOSE`, atau `WRAP` pada task ini — sehingga tidak ada tabel keputusan bernomor yang perlu disajikan ke pengguna dan tidak ada `UI GATE` yang menahan task ini.

Alur dependensi mengikuti `rules/frontend/frontend-architecture.md`: view → hook (`use-billing-invoices.js`) → Redux thunk (`getBillingInvoices`, tidak diubah) → Axios (`InstanceAxios`, tidak diubah) → backend. Penempatan folder tidak berubah — seluruh perubahan berada di dalam folder module `billing-invoices` yang sudah ada.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah — `DataTable` menampilkan `loadingText="Mengambil daftar invoice..."` (sudah ada sebelum task ini) |
| Kosong | Tidak berubah — `emptyTitle`/`emptyDescription` existing. Dengan default baru (hari ini + `OPEN`), kondisi kosong akan lebih sering terlihat pada hari tanpa invoice `OPEN` — pesan existing ("Coba ubah kata kunci pencarian atau filter periode/status/jenis layanan.") tetap relevan dan mengarahkan kasir mengubah filter tanggal/status |
| Gagal | Tidak berubah — `InformationAlert` `errorMessage` dari `listError` Redux |
| Rentang tanggal tidak valid (baru) | `InformationAlert variant="danger"`: "Tanggal Mulai tidak boleh setelah Tanggal Akhir." — tabel tidak memuat ulang, tetap menampilkan data hasil query valid terakhir |
| Tanpa hak akses | Tidak berubah — `AccessDeniedGate` membungkus seluruh layar, tidak disentuh task ini |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru maupun perubahan kontrak. Endpoint yang sudah dikonsumsi sebelum task ini kini menerima dua parameter query tambahan dari frontend:

#### Billing Invoices

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/billing-management/billing/invoices` | Daftar Running Invoice — kini menyertakan `startDate`, `endDate` (selain `search`, `status`, `serviceType`, `period`/`periodPreset`, `pageNumber`, `pageSize` yang sudah ada) | Tidak berubah — otorisasi endpoint ini tidak disentuh task ini |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | `eslint . --quiet` selesai tanpa error | `PASS` | Keluaran perintah, exit code 0 |
| `npm run test:unit` | 1693 test, 1685 pass, 8 fail — **kedelapan kegagalan sudah ada sebelum task ini** (`FE-RWI-042`/`043`, Bank Darah `M0`, sidebar Keuangan, satu test registrasi route/menu/store global), tidak satu pun menyinggung `billing-invoice`/`FilterDatePicker`/`date-picker-utils` | `EXISTING / ENVIRONMENT ISSUE` | Dibuktikan dengan `git stash` atas ketiga berkas task ini lalu menjalankan ulang `npm run test:unit` pada baseline — kedelapan kegagalan **persis sama** muncul tanpa perubahan task ini sama sekali; `git stash pop` mengembalikan perubahan sesudahnya |
| `npm run build` | `✓ Compiled successfully in 53s`; 406/406 halaman statis; `postbuild` (`prepare-standalone.mjs`) `Berhasil menyalin static assets`/`public assets`, `Standalone runtime siap dijalankan` | `PASS` | Keluaran perintah lengkap, exit code 0, nol warning/error pada log |
| Verifikasi kontrak backend: `StartDate`/`EndDate`/`Status` diterima `BillingInvoiceQuery` dan diproses `GetPagedAsync`/`ResolveDateRangeUtc` | Dikonfirmasi ada sejak sebelum task ini, tidak diubah | `PASS` | Pembacaan source `BillingInvoiceDtos.cs` baris 5-19, `BillingInvoiceService.cs` baris 80-99 dan 2116+ |
| Query pertama yang terkirim saat layar dibuka = `StartDate=EndDate=hari ini, Status=OPEN` | Ditelusuri lewat pembacaan kode: `useState(getDefaultBillingInvoiceFilters)` mengisi ketiganya sebelum render pertama, `useEffect` fetch pertama membaca state ini | `PASS` (analisis statis) | §3.2; belum diverifikasi lewat network tab peramban sungguhan |
| `EndDate < StartDate` ditolak sebelum request terkirim | Ditelusuri: `dateValidationError` di-set, `useEffect` `return` sebelum `dispatch(getBillingInvoices(...))` dipanggil | `PASS` (analisis statis) | §3.2; belum diverifikasi manual di peramban |

Uji manual: `NOT FEASIBLE` — sesi ini tidak memiliki tool browser/E2E (tidak ada Playwright MCP atau setara terpasang), sehingga verifikasi network tab peramban sungguhan (query pertama yang benar-benar terkirim, efek mengganti filter tanggal, penolakan rentang terbalik) tidak dapat dijalankan langsung oleh agent. Risiko utama task ("query kosong lalu difilter klien") sudah dihindari secara struktural (`startDate`/`endDate` dikirim sebagai parameter Axios yang sama seperti filter lain, bukan disaring sesudah data diterima) dan terbukti lewat pembacaan kode, tetapi ini **bukan pengganti** bukti verifikasi manual yang diminta eksplisit oleh roadmap (`Bukti verifikasi | ...; verifikasi manual query pertama yang terkirim (network tab)`).

`AUTOMATED TEST: SKIPPED (opsional)` — tidak diminta secara eksplisit pada task ini; repository tidak memakai Jest dan menulis test baru bersifat opsional sesuai `rules/frontend/test-policy.md`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Layar dibuka tanpa interaksi → query pertama `StartDate=EndDate=hari ini, Status=OPEN` | Terpenuhi lewat analisis statis, **belum diverifikasi manual di peramban** | §6 |
| Filter tanggal diterapkan → menggantikan default, terkirim ke backend | Terpenuhi — `updateFilter` men-set state baru yang langsung mengalir ke `buildListParams` pada render berikutnya | §3.2 |
| `EndDate < StartDate` ditolak sebelum request (`BUI-VAL-06`) | Terpenuhi | §3.2, §6 |
| `npm run lint:errors` lulus | Terpenuhi | §6 |
| `npm run test:unit` — nol regresi baru | Terpenuhi (8 kegagalan pre-existing, dibuktikan tidak berubah dengan/tanpa task ini) | §6 |
| `npm run build` lulus | Terpenuhi | §6 |
| Verifikasi manual query pertama yang terkirim (network tab) — diminta eksplisit roadmap | **Belum terpenuhi** — `NOT FEASIBLE` pada sesi ini, tidak ada tool browser/E2E | §6 |
| `git status --short` dilaporkan | Terpenuhi | §8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Uji manual peramban (network tab), bukti verifikasi yang diminta eksplisit roadmap, belum dijalankan pada sesi ini — `NOT FEASIBLE` karena tidak ada tool browser/E2E; menunggu verifikasi pengguna |
| Masalah yang diketahui | Delapan kegagalan `npm run test:unit` sudah ada sebelum task ini (`FE-RWI-042`/`043`, Bank Darah, sidebar Keuangan, satu test registrasi route/menu/store global) — di luar scope task ini, tidak diperbaiki sesuai `AGENTS.md` ("jangan memperbaiki warning/masalah existing yang tidak terkait") |
| Dependency backend | Tidak ada — kontrak `GET /invoices` yang dipakai (`StartDate`/`EndDate`/`Status`) sudah ada sejak `BIL-API-1.4`, dikonfirmasi lewat pembacaan source backend, tidak menunggu `BE-BUI-001`/`002` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx`, `M src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js`, `M src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoices.js` |
| Langkah berikutnya | Verifikasi manual di peramban (network tab) untuk memastikan query pertama benar-benar `StartDate=EndDate=hari ini&Status=OPEN`, dan bahwa mengganti filter tanggal benar-benar mengirim request baru ke backend, bukan menyaring data yang sudah termuat. Perhatikan juga interaksi dengan dropdown "Semua Periode" existing — memilih preset periode akan diprioritaskan backend di atas dua tanggal eksplisit baru ini (perilaku `ResolveDateRangeUtc` yang sudah ada), bukan bug task ini, tapi baik diketahui QA saat menguji |
