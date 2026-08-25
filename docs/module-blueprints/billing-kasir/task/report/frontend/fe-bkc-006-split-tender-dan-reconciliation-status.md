# FE-BKC-006 — Split Tender dan Reconciliation Status

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-006` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` mengikuti presedens `FE-BKC-003`–`005` |
| Branch frontend | `yasmina` |
| Status task | Source selesai, lulus lint/build/`test:unit`. Belum di-commit, belum diverifikasi manual. |
| Catatan urutan | Roadmap mencatat `FE-BKC-006` idealnya menunggu `FE-BKC-007` (shift kasir) karena tender tunai butuh shift aktif. Pemilik task memilih tetap mengerjakan `FE-BKC-006` lebih dulu — dampaknya: tender **tunai** tidak bisa benar-benar berhasil diverifikasi manual sampai `FE-BKC-007` ada (backend akan menolak dengan pesan jelas, bukan gagal senyap). Tender non-tunai tidak terpengaruh urutan ini. |

## Ringkasan untuk pembaca umum

Halaman detail invoice sekarang punya panel **"Pembayaran (Split Tender)"**, tersedia untuk semua
jenis invoice (bukan hanya rawat inap). Ini untuk skenario kasir menerima pembayaran yang dibagi
ke beberapa metode sekaligus — misalnya sebagian tunai, sebagian QRIS — dan salah satu metode bisa
saja gagal atau masih menunggu tanpa membatalkan bagian yang sudah berhasil.

**Temuan penting yang memengaruhi cara kerja fitur ini di environment saat ini**: backend belum
punya integrasi provider pembayaran sungguhan (`DeferredBillingPaymentProviderAdapter` — memang
disengaja, dicatat sebagai blocker `BKC-BLK-PROV-001` di blueprint). Akibatnya, **setiap tender
non-tunai (QRIS, transfer, kartu, dst.) akan selalu berstatus Pending** sampai integrasi provider
sungguhan tersedia — bukan bug di frontend. Hanya tender **tunai** yang langsung berhasil (sinkron,
tidak lewat provider eksternal).

## Proses bisnis

### Proses 1 — Membuat Pembayaran dan Menambah Tender

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Mencatat penerimaan pembayaran pasien terhadap invoice yang sedang berjalan, boleh dibagi ke beberapa metode pembayaran sekaligus. |
| Pelaku | Kasir dengan hak `BillingPayment : Create`. |
| Pemicu | Kasir menekan "+ Buat Pembayaran" pada panel, lalu "+ Tambah Tender" untuk tiap metode. |
| Prasyarat | Invoice `OPEN` dan sudah punya hasil kalkulasi terkini. Nominal settlement tidak melebihi saldo pasien yang belum terbayar. Metode pembayaran aktif, tersedia untuk Billing, dan bukan metode penjamin (asuransi/perusahaan/membership). Tender tunai butuh shift kasir aktif milik kasir yang menambahkan. |
| Langkah utama | 1) Kasir membuat settlement dengan mengisi total nominal yang akan dibayar sekarang. 2) Kasir menambah tender pertama (misal tunai Rp300.000) — untuk tunai, hasil langsung "Berhasil". 3) Kasir menambah tender kedua (misal QRIS Rp700.000) — di environment ini akan langsung "Pending" karena provider belum terintegrasi. 4) Panel menampilkan ringkasan: total diminta, berhasil, pending, dan sisa yang harus dibayar. |
| Aturan bisnis | Total seluruh tender (berhasil + pending) tidak boleh melebihi nominal yang diminta pada settlement — divalidasi backend per tender ditambahkan, dan diberi bantuan validasi client-side sebelum submit. Nominal tender tunai maupun non-tunai sama-sama tidak boleh melebihi *collectible amount* (sisa diminta dikurangi yang sudah berhasil dan yang masih pending). |
| Contoh konkret (mengikuti `BIL-AT-005`) | Settlement dibuat untuk Rp1.000.000. Kasir menambah tender tunai Rp300.000 → langsung Berhasil, sisa Rp700.000. Kasir menambah tender QRIS Rp700.000 → di environment ini berstatus Pending (bukan gagal) karena provider belum terintegrasi. Bagian tunai **tidak terpengaruh** kegagalan/pending-nya QRIS — status settlement menjadi "Sebagian Lunas" (`PARTIALLY_SETTLED`), bukan gagal total. |
| Perubahan status | Settlement: `DRAFT` → `IN_PROGRESS`/`PARTIALLY_SETTLED`/`SETTLED`/`FAILED` mengikuti kombinasi status seluruh tender-nya (dihitung backend, ditampilkan apa adanya). |
| Jalur tidak normal | • Total tender melebihi sisa yang harus dibayar → ditolak dengan pesan jelas, divalidasi juga di frontend sebelum submit. • Tunai tanpa shift aktif → ditolak backend dengan pesan jelas (relevan sebelum `FE-BKC-007` selesai). • Metode penjamin dipilih → tidak muncul di daftar pilihan (difilter di frontend berdasarkan flag asli, backend tetap menolak independen). • Data settlement sudah berubah sejak dimuat → `409`, toast dan reload otomatis. |
| Hasil akhir | Satu atau lebih tender tercatat pada settlement; status settlement mencerminkan gabungan seluruh tender secara akurat, tanpa harus mengulang tender yang sudah berhasil hanya karena tender lain gagal/pending. |

### Proses 2 — Memeriksa Status Tender yang Pending (Bukan Auto-Resubmit)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memeriksa apakah tender yang statusnya belum pasti (Pending) sudah dikonfirmasi provider, **tanpa** membuat percobaan pembayaran baru untuk permintaan yang sama. |
| Pelaku | Kasir yang sama, atau kasir lain yang membuka invoice yang sama. |
| Pemicu | Kasir menekan "Refresh Status" pada panel. |
| Prasyarat | Settlement sudah ada. |
| Langkah utama | 1) Kasir menekan "Refresh Status". 2) Halaman memuat ulang data settlement dari server (`GET settlements/{id}`) dan menampilkan status terbaru seluruh tender. |
| Aturan bisnis | **Tender Pending tidak pernah di-resubmit otomatis oleh frontend.** Idempotency-Key setiap tender dibuat sekali saat dialog "Tambah Tender" dibuka dan tidak dibuat ulang oleh tombol Refresh — refresh murni membaca status, tidak mengirim permintaan pembayaran baru. |
| Contoh konkret | Tender QRIS Rp700.000 berstatus Pending. Kasir menekan Refresh Status berkali-kali — status tetap Pending (di environment ini, karena tidak ada provider yang pernah mengonfirmasi), dan **tidak ada tender QRIS kedua yang tercipta** akibat penekanan berulang. |
| Perubahan status | Tender: `PENDING` → `SUCCEEDED`/`FAILED`/`EXPIRED` (bila provider akhirnya mengonfirmasi — belum bisa terjadi di environment ini) atau tetap `PENDING`. |
| Jalur tidak normal | Refresh pada settlement yang sudah `SETTLED`/`FAILED` tetap diperbolehkan (hanya baca), tapi tombol "Tambah Tender" disembunyikan untuk kedua status akhir ini. |
| Hasil akhir | Kasir tahu persis status pembayaran terkini tanpa risiko dobel-charge. |

### Ketahanan terhadap refresh browser (risiko bernama di roadmap)

Roadmap `FE-BKC-006` secara eksplisit mencatat risiko "Browser refresh kehilangan key". Karena
**tidak ada endpoint untuk mencari settlement aktif berdasarkan invoice** (hanya bisa GET by
settlement ID), task ini menyimpan `settlementId` beserta Idempotency-Key pembuatannya ke
`localStorage` browser (kunci per invoice, `quilvian_billing_settlement_draft_<invoiceId>`) segera
setelah dibuat. Saat halaman invoice dibuka ulang (termasuk setelah refresh), frontend memeriksa
localStorage dan langsung memuat ulang settlement yang sudah ada alih-alih meminta pengguna
membuat settlement baru (yang akan ditolak backend sebagai duplikat/`409`). Entri localStorage
dihapus otomatis begitu settlement mencapai status akhir (`SETTLED`/`FAILED`).

**Batasan yang diakui**: perlindungan ini mencakup level *pembuatan settlement*. Untuk *tender
individual*, kunci idempotensi hanya bertahan selama dialog "Tambah Tender" terbuka di memori
(pola yang sama seperti `FE-BKC-003`–`005`) — refresh persis di tengah satu submit tender (bukan
di tengah pembuatan settlement) tetap berisiko kecil kehilangan key tersebut. Dicatat sebagai
risiko sisa yang diketahui, bukan diabaikan.

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/settlements` | Membuat settlement pembayaran invoice | `BillingPayment : Create` | `CreateSettlementRequest` (`invoiceId`, `purpose="INVOICE_PAYMENT"`, `requestedAmount`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<SettlementResponse>` |
| `POST` | `/settlements/{id}/tenders` | Menambah satu tender (metode pembayaran) pada settlement | `BillingPayment : Create` | `CreateTenderRequest` (`paymentMethodId`, `amount`, `expectedRowVersion`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<TenderResponse>` (`502`/`504` untuk tender non-tunai di environment ini — payload tender Pending tetap disertakan) |
| `GET` | `/settlements/{id}` | Membaca status settlement dan seluruh tender-nya | `BillingPayment : Read` | — | `ApiResponse<SettlementResponse>` |

Ketiganya sudah diimplementasikan backend sejak commit `22bf9cf`; `contracts/api-contract.md`
masih menandainya "Rencana (belum tersedia)" — temuan yang sama seperti dilaporkan pada task
sebelumnya, tidak diulang detail.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200`/`201` | Berhasil (200 untuk replay/GET, 201 untuk settlement/tender baru). |
| `404` | Settlement, invoice, atau metode pembayaran tidak ditemukan. |
| `409` | Data sudah berubah, atau sudah ada settlement aktif lain untuk invoice ini. Toast dan reload otomatis. |
| `422` | Aturan bisnis tidak terpenuhi — nominal melebihi sisa, metode tidak sesuai, tunai tanpa shift aktif, invoice bukan `OPEN`. |
| `502`/`504` | Tender non-tunai belum bisa dipastikan provider (di environment ini: **selalu**, karena provider belum terintegrasi). Ditampilkan sebagai status Pending, bukan error merah — instruksi eksplisit untuk memakai Refresh Status, bukan submit ulang. |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingSettlementsController.cs` (ketiga endpoint, termasuk penanganan `BillingSettlementProviderPendingException` yang tetap menyertakan payload tender), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs:154-337` (`AddTenderAsync` — tunai sinkron via `ReconcileTenderAsync` internal, non-tunai lewat `_providerAdapter.SubmitAsync`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs:786-828` (`RecalculateSettlement`/`CalculateCollectibleAmount` — status settlement dari kombinasi status tender, bukan status tender manapun secara individual), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingPaymentProviderAdapter.cs:48-58` (`DeferredBillingPaymentProviderAdapter` — bukti bahwa provider memang belum terintegrasi, selalu melempar timeout), tidak bagian commit `22bf9cf` khusus, existing sejak part 2.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/billing-settlement-slice.jsx` (**baru** — penanganan khusus respons `502`/`504` sebagai hasil "sukses dengan status pending", bukan rejection biasa) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-settlement.js` (**baru** — localStorage resume, dua alur dialog) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-settlement-panel.jsx`, `create-settlement-modal.jsx`, `add-tender-modal.jsx` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx` (panel dirender untuk semua invoice, tidak dibatasi `serviceType`) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/state/store.jsx` (registrasi reducer baru) — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-006`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Tunai Rp300 ribu tetap sukses saat QRIS Rp700 ribu gagal; outstanding Rp700 ribu | **Terpenuhi (source, sesuai `BIL-AT-005`)** | Lihat Proses 1 — masing-masing tender independen; kegagalan/pending satu tender tidak membatalkan tender lain yang sudah berhasil. Belum diverifikasi manual (butuh kredensial). |
| PENDING tidak auto-resubmit | **Terpenuhi** | Idempotency-Key dibuat sekali per dialog, Refresh Status murni `GET`, tidak pernah memicu `POST` tender baru. |
| Payment draft recovery yang aman | **Terpenuhi** | Resume settlement lewat localStorage setelah refresh browser (lihat bagian "Ketahanan terhadap refresh browser" di atas). |
| No provider payload logs | **Terpenuhi (mengikuti backend)** | Frontend hanya menampilkan `providerReferenceMasked` yang sudah di-mask backend; tidak ada payload provider mentah yang disimpan atau di-log di frontend. |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint <7 file yang diubah/baru>` (severity penuh) | **PASS** | Tanpa output. |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 68s`. |
| `npm run test:unit` | **PASS** | 38 test, 38 pass, 0 fail. Tidak ada test yang menguji kode settlement. |
| Smoke-test browser headless tanpa login | **PARTIAL PASS** | 0 exception JS pada halaman detail invoice (kini memuat panel settlement + 2 modal baru). |
| Verifikasi manual ter-autentikasi (split tender sungguhan) | **NOT DONE** | Tidak ada kredensial. Tender tunai juga tidak bisa lulus sampai `FE-BKC-007` (shift kasir) tersedia. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint/build/test:unit lulus bersih, tapi
klik-coba langsung (termasuk kasus tunai yang butuh shift aktif dari `FE-BKC-007`) belum
dijalankan.

## Langkah berikutnya yang direkomendasikan

1. Selesaikan `FE-BKC-007` (operasi shift kasir) agar skenario tender tunai bisa benar-benar
   diverifikasi manual end-to-end.
2. Login dan verifikasi manual keempat acceptance criteria di atas, termasuk memastikan tender
   non-tunai memang selalu Pending di environment ini (bukti provider belum terintegrasi, bukan
   bug).
3. Saat integrasi provider pembayaran sungguhan tersedia (`BKC-BLK-PROV-001` resolved), pertimbangkan
   auto-refresh berjeda (bukan hanya tombol manual) untuk tender Pending — di luar scope saat ini
   karena provider belum ada untuk diuji.
