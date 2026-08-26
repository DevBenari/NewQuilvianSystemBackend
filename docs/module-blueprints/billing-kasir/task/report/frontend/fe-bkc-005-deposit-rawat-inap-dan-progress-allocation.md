# FE-BKC-005 — Deposit Rawat Inap dan Progress Allocation

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-005` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` mengikuti presedens `FE-BKC-003`/`FE-BKC-004` |
| Branch frontend | `yasmina` |
| Status task | Source selesai, lulus lint/build/`test:unit`. Belum di-commit, belum diverifikasi manual. |

## Ringkasan untuk pembaca umum

Halaman detail invoice sekarang punya panel baru **"Deposit Rawat Inap"**, tapi hanya muncul untuk
invoice dengan jenis layanan **Rawat Inap (RANAP)** — pasien rawat jalan/IGD tidak punya konsep
deposit ini. Panel menampilkan:

1. **Saldo deposit** yang tersedia dan riwayat pergerakannya (top-up, alokasi, dst).
2. **Top-Up** — mencatat dana yang diterima kasir dari keluarga pasien sebagai deposit. Dana ini
   *belum* menjadi pembayaran atas tagihan mana pun — hanya tersimpan sebagai saldo.
3. **Alokasikan ke Invoice Ini** — memindahkan sebagian saldo deposit menjadi pembayaran atas
   invoice yang sedang dilihat, **tanpa menutup invoice** (invoice tetap bisa menerima item baru
   setelahnya, khas rawat inap yang tagihannya terus berjalan selama pasien dirawat).

Ini bukan pembayaran biasa — deposit adalah "tabungan sementara" milik pasien di sistem Billing.
Kasir tidak boleh menganggap uang yang di-top-up sudah otomatis melunasi tagihan; harus ada
langkah kedua (alokasi) yang eksplisit.

## Proses bisnis

### Proses 1 — Top-Up Deposit

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Mencatat dana yang diterima dari keluarga pasien rawat inap sebagai saldo deposit, sebelum dana itu dipakai untuk membayar tagihan tertentu. |
| Pelaku | Kasir dengan hak `BillingDeposit : Create`. |
| Pemicu | Kasir menekan "+ Top-Up" pada panel Deposit Rawat Inap. |
| Prasyarat | Kunjungan (encounter) berjenis rawat inap, tidak dibatalkan/tidak-hadir. Metode pembayaran aktif, tersedia untuk Billing, dan **bukan** metode penjamin (asuransi/perusahaan/membership) serta tidak butuh nomor referensi/approval/lampiran tambahan. Bila metode pembayaran adalah tunai, kasir wajib punya shift kasir yang sedang aktif (fitur shift ada di `FE-BKC-007`, belum dibangun — bila belum ada shift aktif, top-up tunai akan ditolak backend dengan pesan jelas). |
| Langkah utama | 1) Kasir membuka dialog Top-Up. 2) Memilih metode pembayaran dari daftar yang sudah difilter otomatis (metode penjamin disembunyikan). 3) Mengisi nominal dan alasan. 4) Submit. 5) Sistem mencatat movement `TOP_UP`, menambah saldo, dan bila account deposit belum pernah ada untuk kunjungan ini, sistem otomatis membuatnya (nomor account baru dialokasikan). 6) Panel menampilkan saldo terbaru dan baris ledger baru. |
| Aturan bisnis | Nominal harus positif, maksimal dua angka desimal. Top-up **tidak terikat status invoice** — tetap bisa dilakukan meskipun invoice sudah `FINAL`/`CLOSED`, karena deposit adalah milik kunjungan (encounter), bukan milik satu invoice. |
| Contoh konkret | Keluarga pasien menyerahkan Rp8.000.000 tunai untuk deposit rawat inap. Kasir (dengan shift aktif) mencatat top-up Rp8.000.000 dengan alasan "Deposit awal rawat inap sesuai kesepakatan keluarga". Saldo deposit menjadi Rp8.000.000, tercatat sebagai satu baris ledger bertipe Top-Up. |
| Perubahan status | Account deposit: tidak ada → `ACTIVE` (saat top-up pertama) atau tetap `ACTIVE`. |
| Jalur tidak normal | • Metode pembayaran penjamin/butuh approval → ditolak dengan pesan jelas ("Metode penjamin tidak dapat digunakan sebagai top-up deposit." / "...memerlukan settlement/tender yang belum tersedia."). • Tunai tanpa shift aktif → ditolak. • Permintaan yang sama terkirim dua kali (network putus lalu retry) → backend mendeteksi via Idempotency-Key dan mengembalikan hasil yang sama, saldo **tidak** bertambah dua kali. • Data deposit sudah berubah sejak dimuat (`ExpectedRowVersion` tidak cocok) → `409`, toast "Data sudah berubah" dan reload otomatis. |
| Hasil akhir | Saldo deposit bertambah sejumlah nominal top-up, tercatat permanen di ledger dengan alasan dan waktu. |

### Proses 2 — Mengalokasikan Deposit ke Invoice (Progress Allocation)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Memakai sebagian saldo deposit untuk membayar (sebagian) tagihan yang sedang berjalan, tanpa menutup invoice — memungkinkan pasien rawat inap membayar bertahap selama masih dirawat. |
| Pelaku | Kasir dengan hak `BillingDeposit : Allocate`. |
| Pemicu | Kasir menekan "Alokasikan ke Invoice Ini" pada panel Deposit, dari halaman detail invoice yang sedang dilihat. |
| Prasyarat | Invoice berstatus `OPEN` dan berada pada kunjungan (encounter) yang sama dengan deposit. Invoice sudah punya hasil kalkulasi (pernah di-Hitung Ulang minimal sekali). Nominal alokasi tidak melebihi saldo deposit **dan** tidak melebihi sisa tagihan yang belum terbayar (outstanding) — dua batas ini diperiksa terpisah oleh backend. |
| Langkah utama | 1) Kasir menekan "Alokasikan ke Invoice Ini" (tombol otomatis nonaktif bila saldo deposit Rp0 atau invoice bukan `OPEN`). 2) Dialog menampilkan saldo deposit tersedia sebagai referensi. 3) Kasir mengisi nominal dan alasan. 4) Submit. 5) Sistem memindahkan dana: saldo deposit berkurang, invoice mendapat pembayaran sejumlah nominal tersebut, dicatat sebagai satu settlement dan satu movement `ALLOCATION`. 6) Notifikasi hasil menampilkan saldo deposit tersisa dan outstanding invoice terbaru. |
| Aturan bisnis | Invoice **tetap `OPEN`** setelah alokasi — ini bukan pelunasan yang menutup invoice, hanya mengurangi outstanding. Bila nominal alokasi melebihi outstanding invoice yang sebenarnya (dana lebih), kelebihannya **tidak hilang**: sistem mencatatnya sebagai *refundable credit* (dana lebih yang bisa dikembalikan/dipakai nanti), bukan otomatis dianggap lunas berlebih. Total tagihan yang tampil di halaman **tidak otomatis berubah** setelah alokasi — kasir perlu menekan "Hitung Ulang" (`FE-BKC-003`) agar kalkulasi terbaru memperhitungkan pembayaran ini. |
| Contoh konkret (mengikuti `BIL-AT-007`) | Saldo deposit Rp8.000.000 (dari Proses 1). Kasir mengalokasikan Rp5.000.000 ke invoice yang outstanding-nya saat itu Rp6.000.000, dengan alasan "Pembayaran cicilan minggu pertama sesuai instruksi kasir kepala". Hasil: saldo deposit tersisa Rp3.000.000, outstanding invoice berkurang menjadi Rp1.000.000, invoice tetap `OPEN`. Bila kasir kemudian mengalokasikan Rp1.500.000 lagi (melebihi outstanding Rp1.000.000 yang tersisa), sistem menyerap Rp1.000.000 sebagai pembayaran dan mencatat Rp500.000 sebagai *refundable credit* — bukan kesalahan, tapi juga bukan otomatis dicairkan tunai. |
| Perubahan status | Invoice: tetap `OPEN` (tidak berubah). Deposit: saldo berkurang, tetap `ACTIVE`. |
| Jalur tidak normal | • Nominal melebihi saldo deposit ATAU outstanding invoice → ditolak dengan pesan "Dana deposit atau saldo tagihan tidak mencukupi.", divalidasi juga di frontend sebelum submit sebagai bantuan (bukan pengganti validasi backend). • Invoice bukan `OPEN`, atau data deposit/invoice/kalkulasi sudah berubah sejak dimuat → `409`, toast dan reload otomatis. • Dua kasir mencoba mengalokasikan bersamaan pada versi data yang sama → hanya satu yang berhasil, satu lagi mendapat `409` dan diminta memuat ulang. |
| Hasil akhir | Sebagian atau seluruh saldo deposit berpindah menjadi pembayaran invoice yang masih berjalan; riwayat alokasi tercatat permanen; kelebihan dana (bila ada) tercatat sebagai refundable credit. |

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Lihat saldo dan ledger deposit kunjungan rawat inap | `BillingDeposit : Read` | — | `ApiResponse<DepositResponse>` (`404` bila belum pernah top-up — ditangani sebagai state kosong, bukan error) |
| `POST` | `/deposits/{encounterId}/top-ups` | Catat top-up deposit | `BillingDeposit : Create` | `DepositTopUpRequest` (`paymentMethodId`, `amount`, `expectedRowVersion?`, `reason`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<SettlementResponse>` (berisi `deposit` hasil terbaru) |
| `POST` | `/deposits/{encounterId}/allocations` | Alokasikan dana deposit ke invoice tanpa menutupnya | `BillingDeposit : Allocate` | `DepositAllocationRequest` (`invoiceId`, `amount`, `expectedDepositRowVersion`, `expectedInvoiceRowVersion`, `expectedCalculationVersion`, `reason`, `correlationId`, `causationId`) + header `Idempotency-Key` | `ApiResponse<AllocationResponse>` |

### Health Services / Billing Management / Master Data / Payment Method

Base URL: `api/v1/health-services/billing-management/master-data/payment-methods`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/options` | Daftar metode pembayaran untuk picker top-up | `PaymentMethod : Read` | `isAvailableForBilling=true`, `onlyActive=true` | `ApiResponse<List<PaymentMethodOptionResponse>>` |

Master data ini existing (direuse, bukan bagian kontrak baru billing-kasir), tapi **belum pernah
dikonsumsi frontend di modul manapun sebelum task ini** — dibangun slice read-only baru khusus
untuk kebutuhan picker ini (lihat bukti kode).

Ketiga endpoint Patient Funds sudah diimplementasikan backend sejak commit `22bf9cf` — dokumen
`contracts/api-contract.md` masih menandainya "Rencana (belum tersedia)"; temuan yang sama seperti
dilaporkan pada `FE-BKC-003`/`FE-BKC-004`.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil. Saldo/ledger/hasil alokasi langsung tampil. |
| `404` | Belum ada deposit untuk kunjungan ini (untuk `GET`, ditampilkan sebagai state kosong) atau invoice/metode pembayaran tidak ditemukan (untuk `POST`). |
| `409` | Data sudah berubah sejak dimuat, atau permintaan idempotent tidak konsisten. Halaman menampilkan pesan dan memuat ulang otomatis. |
| `422` | Aturan bisnis tidak terpenuhi — contoh: metode pembayaran tidak sesuai, saldo/outstanding tidak cukup, invoice bukan `OPEN`, kunjungan bukan rawat inap. |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs` (ketiga endpoint), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs:53-239` (`TopUpAsync` — validasi metode pembayaran, shift kasir untuk tunai, pembuatan account otomatis, idempotency), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs:29-227` (`AllocateDepositAsync` — batas ganda saldo/outstanding, invoice tetap OPEN, refundable credit saat dana lebih), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/MasterData/Controllers/PaymentMethodController.cs:218-` (`GET /options`, termasuk flag `IsInsurance`/`IsCompanyGuarantor`/`IsMembership`/`IsNeedReferenceNumber`/dst. yang dipakai untuk filter picker) — master data existing, bukan bagian commit `22bf9cf`.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/billing-deposit-slice.jsx` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/master-data-payment-method-slice.jsx` (**baru**, read-only) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-deposit.js` (**baru** — filter metode pembayaran, dua alur dialog top-up/allocate) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-deposit-panel.jsx`, `top-up-deposit-modal.jsx`, `allocate-deposit-modal.jsx` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx` (panel dirender hanya untuk `serviceType === "RANAP"`) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/state/store.jsx` (registrasi 2 reducer baru) — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-005`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Contoh Rp8 juta/Rp5 juta benar | **Terpenuhi (source, sesuai `BIL-AT-007`)** | Lihat Proses 2 di atas — nominal dan alur mengikuti persis skenario acceptance test. Nominal alokasi tidak dihitung ulang di frontend, selalu dari response backend. |
| Invoice tetap OPEN | **Terpenuhi** | Panel tidak menutup/mengubah status invoice; `AllocateDepositAsync` memang tidak mengubah `invoice.Status`. Toast sukses eksplisit mengingatkan invoice tetap Open dan perlu Hitung Ulang. |
| Insufficient/conflict error jelas | **Terpenuhi** | Validasi client-side (nominal ≤ saldo tersedia) sebagai bantuan; pesan `422`/`409` backend ditampilkan apa adanya via toast, `409` memicu reload otomatis. |
| Double-submit aman | **Terpenuhi (source)** | `Idempotency-Key`/`correlationId`/`causationId` dibuat sekali saat dialog dibuka (top-up dan allocate masing-masing dialog terpisah), dipakai ulang untuk retry pada dialog yang sama. Tombol submit nonaktif selama `submitting`. |

## Simplifikasi dan batasan yang dicatat (bukan celah)

1. **Reversal top-up tidak dibangun** — `ReverseTopUpAsync` sudah ada di backend, tapi di luar
   scope roadmap `FE-BKC-005` ("Ledger, top-up form, allocation preview, available/outstanding
   after action, refundable credit panel" — tidak menyebut reversal). Kandidat task terpisah.
2. **Refundable credit hanya tampil sebagai hasil transien setelah alokasi** (dari
   `AllocationResponse.RefundableCredit`), bukan panel saldo permanen — tidak ada endpoint GET
   yang mengembalikan saldo refundable credit invoice secara independen di luar konteks aksi
   alokasi. Dicatat sebagai keterbatasan data, bukan kelalaian implementasi.
3. **Saldo/outstanding "before" tidak ditampilkan sebagai preview sebelum submit alokasi** —
   backend tidak punya endpoint terpisah untuk itu; yang tersedia hanya saldo deposit (dari `GET
   deposits/{encounterId}`, ditampilkan sebagai batas atas nominal) dan hasil "after" dari response
   alokasi. Frontend tidak mengarang angka outstanding sendiri untuk menghindari risiko salah
   hitung dibanding backend.
4. Panel deposit hanya dipasang di halaman detail invoice (butuh invoice `RANAP` yang sudah ada)
   — kasus deposit yang di-top-up sebelum encounter punya invoice sama sekali tidak tercakup pada
   task ini.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint <8 file yang diubah/baru>` (severity penuh) | **PASS** | Tanpa output. |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 82s`. |
| `npm run test:unit` | **PASS** | 38 test, 38 pass, 0 fail (`ISSUE-FE-005` sudah diperbaiki sebelumnya). Tidak ada test yang menguji kode deposit. |
| Smoke-test browser headless tanpa login | **PARTIAL PASS** | 0 exception JS pada halaman detail invoice (kini memuat panel deposit + 2 modal baru), redirect ke `/login` bersih setelah `401`. |
| Verifikasi manual ter-autentikasi (top-up, alokasi, kasus insufficient/conflict) | **NOT DONE** | Sama seperti task sebelumnya — tidak ada kredensial. Dev server masih berjalan di `localhost:3000`. |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint/build/test:unit lulus bersih, tapi
klik-coba langsung dengan encounter rawat inap dan akun sungguhan belum dijalankan.

## Langkah berikutnya yang direkomendasikan

1. Login di `localhost:3000` dengan akun yang punya `BillingDeposit:Create`/`Allocate` pada
   encounter rawat inap yang punya invoice `OPEN`, lalu verifikasi manual keempat acceptance
   criteria di atas — termasuk kasus tunai tanpa shift aktif (harus ditolak jelas).
2. Tambahkan unit/component test untuk `use-billing-deposit.js` (filter metode pembayaran, alur
   dua dialog).
3. Pertimbangkan `FE-BKC-007` (operasi shift kasir) berikutnya — dependency langsung untuk
   skenario top-up tunai di atas bisa benar-benar dicoba end-to-end.
