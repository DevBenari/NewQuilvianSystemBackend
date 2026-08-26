# FE-BKC-008 — Pengecualian Finansial (Refund, Adjustment, Write-off)

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-008` |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Frontend, vertical slice, panel baru pada halaman invoice detail yang sudah ada |
| Task mode | `FRONTEND` (backend read-only, dipakai sebagai bukti kontrak dan perilaku *as-is*) |
| Write target | `QuilvianSystemFrontendDev` (source); laporan ini + evidence roadmap ditulis di `NewQuilvianSystemBackend` mengikuti presedens task sebelumnya |
| Branch frontend | `yasmina` |
| Lokasi UI | Panel baru "Pengecualian Finansial" pada `/health-services/billing-management/billing/invoices/[slug]` (bukan halaman baru — lihat alasan pada bagian Temuan) |
| Status task | Source selesai, lulus lint/build/`test:unit`. Belum di-commit, belum diverifikasi manual. |

## Ringkasan untuk pembaca umum

Panel baru pada halaman detail invoice untuk mengajukan dan menyetujui tiga jenis pengecualian
finansial: **Refund** (mengembalikan dana ke pasien dari kredit yang bisa dikembalikan), **Adjustment**
(koreksi saldo invoice, debit atau kredit, append-only), dan **Write-off** (menghapuskan saldo
outstanding tanpa dianggap sebagai pembayaran). Ketiganya memakai pola maker-checker: pengaju dan
penyetuju harus pengguna berbeda, dan case yang sudah `POSTED` (adjustment/write-off) dapat
dibalik lewat entry reversal baru — bukan menghapus/mengedit entry asal.

**Temuan penting yang membatasi seberapa mulus fitur ini bisa dipakai saat ini — dan ini adalah gap
backend paling signifikan yang ditemukan sepanjang task `FE-BKC-003`–`008`**: `BillingFinancialExceptionsController`
**sama sekali tidak memiliki satu pun endpoint `GET`**. Bukan hanya tanpa daftar per-invoice
(seperti kekurangan `FE-BKC-007` pada shift kasir) — di sini **bahkan tidak ada `GET` by-id**.
Satu-satunya jejak sebuah refund/adjustment/write-off case adalah response JSON saat case itu
dibuat atau disetujui. Konsekuensinya:

1. **Tidak ada cara sistem bagi seorang approver untuk menemukan case yang menunggu persetujuannya.**
   Pola maker-checker yang menjadi acceptance criteria utama task ini (lihat `BIL-VAL-017`) secara
   inheren membutuhkan approver bisa *menemukan* apa yang perlu direview — kemampuan itu tidak ada
   sama sekali di API saat ini.
2. **Tidak ada `RefundableCreditId` yang bisa ditemukan lewat aplikasi.** `CreateRefundRequest`
   mewajibkan `RefundableCreditId` (GUID entitas `BilRefundableCredit`), tapi tidak ada satu pun
   endpoint di seluruh modul Billing yang mengembalikan ID ini — respons alokasi deposit
   (`AllocateDepositResponse.RefundableCredit`, dipakai `FE-BKC-005`) hanya berupa **nominal**
   (`decimal`), bukan ID entitasnya.
3. **Case yang sudah diajukan tidak bisa "di-refresh" statusnya secara normal**, karena tidak ada
   `GET`. Frontend memakai jalan memutar yang sah secara desain backend (bukan celah): endpoint
   `Create*` bersifat idempotent dikunci `Idempotency-Key` + payload hash (bukan `ExpectedRowVersion`),
   sehingga mengirim ulang body+header **persis sama** akan memicu jalur *replay* dan mengembalikan
   entity **versi terkini** (status/RowVersion sekarang), bukan snapshot lama. Tombol "Refresh"
   pada case yang terlacak memakai trik ini. Untuk reversal, trik serupa memakai fakta bahwa
   `ReverseAsync` mengecek "apakah case ini sudah pernah direversal" **sebelum** memvalidasi
   `ExpectedRowVersion` — jadi memanggil ulang `/reverse` dengan ID yang sama aman dipakai untuk
   memeriksa ulang status reversal.

Karena tidak ada cara mencari/melihat case sama sekali, panel ini **melacak case secara lokal per
invoice di `localStorage` browser** (mirip pola `FE-BKC-006`, tapi lebih terbatas — di sana
setidaknya ada `GET /settlements/{id}`). Untuk menyetujui atau mereversal case yang diajukan dari
luar browser ini (mis. oleh kasir lain), pengguna harus memasukkan Case ID + Row Version secara
manual, direlai dari pengaju di luar aplikasi — pola yang sama seperti `ISSUE-FE-006` pada
`FE-BKC-007`, tapi di sini berlaku untuk **seluruh siklus hidup case**, bukan hanya sebagian aksi.
Ini didokumentasikan secara eksplisit sebagai keterbatasan backend, bukan kekurangan implementasi
frontend — dan karena keterbatasan inilah panel ini dibangun sebagai bagian dari halaman invoice
detail yang sudah ada (bukan "workbench"/halaman case-list mandiri seperti disebut pada scope
roadmap), karena tidak ada data case-list apa pun yang bisa ditampilkan di halaman semacam itu.

## Proses bisnis

### Proses 1 — Refund

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Mengembalikan dana kepada pasien dari kredit yang bisa dikembalikan (`BilRefundableCredit`), mis. kelebihan alokasi deposit atau kelebihan settlement. |
| Pelaku | Pengaju dengan hak `BillingRefund : Create`; penyetuju dengan hak `BillingRefund : Approve` (harus pengguna berbeda dari pengaju). |
| Prasyarat | Invoice **bukan** rawat inap (`ServiceType != RANAP` — refund normal tidak berlaku untuk RANAP). Refundable credit berstatus `AVAILABLE` dan terkait invoice yang sama. Belum ada refund case aktif lain atas kredit yang sama. Invoice punya minimal satu tender berhasil dengan metode pembayaran yang `IsAvailableForRefund`. |
| Langkah utama | 1) Pengaju mengisi Refundable Credit ID (didapat dari luar aplikasi — lihat Temuan #2), nominal, dan alasan. 2) Sistem membuat case `SUBMITTED` dengan baris-baris (`RefundLine`) yang dialokasikan **proporsional** ke tender asal berdasarkan nominal masing-masing. 3) Penyetuju (pengguna lain) menyetujui — sistem otomatis mengeksekusi setiap baris: tunai langsung `SUCCEEDED`, non-tunai dikirim ke payment provider. 4) Status case menjadi `EXECUTED` jika semua baris berhasil, `PARTIALLY_EXECUTED` jika sebagian, atau tetap `APPROVED` jika belum ada yang berhasil (baris `PENDING` bisa dicoba ulang dengan memanggil approve lagi). |
| Aturan bisnis | Pengaju tidak boleh menyetujui pengajuannya sendiri (`BIL-VAL-017`). Nominal tidak boleh melebihi saldo kredit yang tersedia. Alokasi ke tender proporsional berdasarkan nominal, bukan urutan. |
| Contoh konkret | Invoice punya 2 tender berhasil: tunai Rp1.000.000 dan kartu Rp2.000.000 (total Rp3.000.000, kartu `IsAvailableForRefund=true`). Refund diminta Rp900.000 dari kredit terkait → dialokasikan proporsional: tunai Rp300.000, kartu Rp600.000, sebagai dua `RefundLine` terpisah. |
| Perubahan status | Tidak ada → `SUBMITTED` → `APPROVED` → `EXECUTED`/`PARTIALLY_EXECUTED` (approve memicu eksekusi otomatis; `APPROVED`/`PARTIALLY_EXECUTED` bisa di-*retry* lewat approve lagi untuk baris yang masih `PENDING`). |
| Jalur tidak normal | • Kredit sudah `EXHAUSTED`/tidak tersedia → `422`. • Kredit sudah punya case aktif lain → `409`. • Nominal melebihi saldo/kapasitas tender yang bisa direfund → `422`. • Pengaju mencoba approve sendiri → `403`. |
| Hasil akhir | Dana dikembalikan ke metode pembayaran asal secara proporsional, tercatat lengkap per baris dengan referensi provider yang di-*mask*. Refund case **tidak bisa direversal** (tidak ada endpoint reverse untuk refund). |

### Proses 2 — Adjustment

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Koreksi saldo invoice (menambah/mengurangi tagihan pasien) tanpa mengubah item invoice, mis. koreksi kesalahan hitung manual atau penyesuaian kebijakan. |
| Pelaku | Pengaju dengan hak `BillingAdjustment : Create`; penyetuju dengan hak `BillingAdjustment : Approve` (berbeda dari pengaju). |
| Prasyarat | Invoice bukan `CLOSED`/`SETTLED_BY_WRITE_OFF`. `ExpectedInvoiceRowVersion` sesuai versi invoice terkini. |
| Langkah utama | 1) Pengaju memilih arah (`DEBIT` menambah tagihan pasien / `CREDIT` mengurangi), mengisi nominal dan alasan. 2) Sistem membuat entry `SUBMITTED`, invoice `RowVersion` berubah (dianggap sebagai perubahan pada ledger invoice). 3) Penyetuju (pengguna lain) menyetujui → entry menjadi `POSTED`, invoice `RowVersion` berubah lagi. |
| Aturan bisnis | Append-only — entry yang sudah `POSTED` tidak pernah diedit/dihapus, hanya bisa dibalik lewat reversal (entry baru dengan arah berlawanan). Pengaju tidak boleh menyetujui pengajuannya sendiri. |
| Contoh konkret | Invoice dengan porsi pasien Rp2.500.000 dikoreksi `CREDIT` Rp50.000 (kelebihan tagih) → setelah `POSTED`, outstanding efektif berkurang Rp50.000 pada perhitungan `CalculateOutstandingAsync` (dipakai internal oleh write-off, belum diekspos langsung di `InvoiceDetailResponse`). |
| Perubahan status | Tidak ada → `SUBMITTED` → `POSTED` (approve) atau tetap `SUBMITTED` menunggu (belum ada endpoint reject eksplisit — lihat Temuan #4). |
| Jalur tidak normal | • Invoice `CLOSED`/`SETTLED_BY_WRITE_OFF` → `422`. • `ExpectedInvoiceRowVersion` usang → `409`. • Pengaju mencoba approve sendiri → `403`. |
| Hasil akhir | Saldo invoice terkoreksi dengan jejak audit lengkap (siapa mengajukan, siapa menyetujui, alasan masing-masing tahap). |

### Proses 3 — Write-off

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Menghapuskan saldo outstanding invoice (mis. piutang tak tertagih) **tanpa** dianggap sebagai pembayaran — beda secara fundamental dari settlement. |
| Pelaku | Pengaju dengan hak `BillingWriteOff : Create`; penyetuju dengan hak `BillingWriteOff : Approve` (berbeda dari pengaju). |
| Prasyarat | Invoice bukan `CLOSED`/`SETTLED_BY_WRITE_OFF`. Nominal tidak melebihi saldo outstanding invoice **saat diajukan maupun saat disetujui** (dicek dua kali). |
| Langkah utama | 1) Pengaju mengisi nominal dan alasan. 2) Sistem membuat case `SUBMITTED` setelah memverifikasi nominal ≤ outstanding saat ini. 3) Penyetuju menyetujui → sistem menghitung ulang outstanding (bisa sudah berubah sejak pengajuan), jika masih cukup case menjadi `POSTED`; jika nominal write-off == outstanding penuh, invoice berubah status menjadi `SETTLED_BY_WRITE_OFF`. |
| Aturan bisnis | **Write-off tidak pernah dilabel "PAID"/lunas** (`BIL-VAL-018`) — status invoice yang dihasilkan adalah `SETTLED_BY_WRITE_OFF`, bukan status pembayaran biasa; UI (badge invoice, `BILLING_INVOICE_STATUS_BADGE_CONFIG`) sudah membedakan label ini sejak `FE-BKC-001`. |
| Contoh konkret | Outstanding Rp150.000 (piutang kecil tak tertagih). Write-off Rp150.000 diajukan dan disetujui → `IsFullSettlement=true`, invoice menjadi `SETTLED_BY_WRITE_OFF`, outstanding setelahnya Rp0. |
| Perubahan status | Tidak ada → `SUBMITTED` → `POSTED`. Invoice: (tidak berubah) → `SETTLED_BY_WRITE_OFF` **hanya jika** write-off menutup outstanding sepenuhnya. |
| Jalur tidak normal | • Nominal melebihi outstanding saat diajukan **atau** saat disetujui (outstanding bisa berkurang di antara keduanya, mis. ada pembayaran lain masuk) → `422`, pesan meminta ajukan ulang. • Invoice `CLOSED`/`SETTLED_BY_WRITE_OFF` → `422`. |
| Hasil akhir | Saldo outstanding terhapus tanpa memalsukan status sebagai "dibayar", tercatat lengkap dengan outstanding sebelum/sesudah untuk audit. |

### Proses 4 — Reversal (Adjustment dan Write-off saja)

| Aspek | Keterangan |
| --- | --- |
| Tujuan | Membalik efek adjustment/write-off yang sudah `POSTED`, tanpa mengubah/menghapus entry asal (append-only, sesuai prinsip ledger). |
| Pelaku | Pengguna dengan hak `BillingFinancialException : Reverse` (tidak ada pembatasan maker-checker eksplisit pada reversal itu sendiri di service). |
| Prasyarat | Case asal berstatus `POSTED`. Belum pernah direversal sebelumnya (dicek lebih dulu — pemanggilan kedua dengan ID yang sama akan mengembalikan reversal yang sudah ada, bukan membuat baru). |
| Langkah utama | 1) Pengguna memasukkan tipe (Adjustment/Write-off), Case ID, Row Version case asal, dan alasan. 2) Sistem membuat **entry Adjustment baru** dengan arah berlawanan (untuk reversal adjustment) atau `DEBIT` senilai write-off (untuk reversal write-off) — **keduanya selalu menghasilkan entry Adjustment**, bukan "write-off case" baru. 3) Jika yang direversal adalah write-off yang tadinya menutup invoice penuh (`IsFullSettlement`), status invoice `SETTLED_BY_WRITE_OFF` dikembalikan menjadi `OPEN`. |
| Aturan bisnis | Reversal **tidak menghapus atau mengedit** entry asal — hanya menambah entry penyeimbang baru. Entry asal tetap `POSTED` selamanya, tetap tampak di riwayat. |
| Contoh konkret | Write-off Rp150.000 (full settlement, invoice `SETTLED_BY_WRITE_OFF`) ternyata keliru, direversal → entry Adjustment `DEBIT` Rp150.000 dibuat, invoice kembali `OPEN`, write-off asal tetap tercatat `POSTED` sebagai riwayat. |
| Perubahan status | Case asal: tidak berubah (tetap `POSTED`). Invoice: `SETTLED_BY_WRITE_OFF` → `OPEN` (hanya jika reversal atas write-off full-settlement). |
| Jalur tidak normal | • Case asal belum `POSTED` (mis. masih `SUBMITTED`) → `422`. • `ExpectedRowVersion` usang → `409` (**kecuali** case sudah pernah direversal — di situ short-circuit ke reversal yang ada, RowVersion diabaikan). • Tipe selain `adjustments`/`write-offs` → `422`. |
| Hasil akhir | Efek finansial dibalik dengan jejak audit lengkap dua arah (entry asal + entry reversal), tanpa memalsukan sejarah dengan mengedit entry lama. **Refund case tidak bisa direversal lewat endpoint ini sama sekali** — tidak ada jalur reversal untuk refund di backend. |

## Temuan — keterbatasan backend yang memengaruhi UX (bukan celah UI)

1. **`ISSUE-FE-008` (baru, paling signifikan pada task ini): tidak ada satu pun endpoint `GET` untuk
   refund/adjustment/write-off case.** Bukan sekadar tanpa daftar (seperti `FE-BKC-007`) — di sini
   bahkan tidak ada `GET` by-id. Approver di sesi/perangkat lain **tidak memiliki cara sistem apa
   pun** untuk menemukan case yang perlu direview; ini melemahkan acceptance criteria maker-checker
   itu sendiri, karena "checker" idealnya bisa *menemukan* apa yang perlu dicek, bukan hanya
   menerima ID lewat kanal di luar aplikasi. **Rekomendasi**: tambahkan minimal `GET
   /{type}/{id}` (baca satu case) dan idealnya `GET /invoices/{invoiceId}/financial-exceptions`
   (daftar semua case per invoice, mencakup ketiga tipe) serta `GET
   /financial-exceptions/pending-approval` (daftar case `SUBMITTED` yang menunggu approver saat
   ini, difilter oleh permission).
2. **Tidak ada endpoint untuk mencari `RefundableCreditId`.** `CreateRefundRequest` mewajibkan ID
   ini, tapi tidak ada satu pun response API di seluruh modul Billing yang mengembalikannya — hanya
   nominalnya (`AllocateDepositResponse.RefundableCredit: decimal`). Form "Ajukan Refund" terpaksa
   memakai input teks bebas untuk ID ini. **Rekomendasi**: tambahkan `GET
   /invoices/{invoiceId}/refundable-credits` (daftar kredit `AVAILABLE` milik invoice, dengan ID
   masing-masing) — pola yang sama seperti kebutuhan master data pada `ISSUE-FE-007`.
3. **Tidak ada endpoint reject eksplisit untuk adjustment/write-off/refund.** Status `REJECTED`
   didefinisikan di enum backend (`BillingAdjustmentStatuses.Rejected` dst.) tapi tidak ada jalur
   API mana pun yang pernah men-set status ini — hanya `approve`. Case yang tidak disetujui hanya
   tetap `SUBMITTED` selamanya, tidak ada cara formal menandainya "ditolak". **Rekomendasi**:
   tambahkan endpoint reject sejajar dengan approve, atau jelaskan bila `REJECTED` memang disengaja
   tidak dipakai dan harus dihapus dari enum.
4. Karena keterbatasan #1, panel ini dibangun di halaman invoice detail (bukan halaman
   workbench/case-list mandiri seperti tersirat pada scope roadmap "Case list/detail") — tidak ada
   data case-list apa pun yang bisa ditampilkan di halaman terpisah. Case yang diajukan dari browser
   ini dilacak di `localStorage` per invoice (`quilvian_billing_financial_exception_cases_<invoiceId>`)
   dan tombol "Refresh" pada case terlacak memakai jalur *replay* idempotency-key (untuk create) atau
   pemanggilan ulang `/reverse` (untuk reversal) sebagai satu-satunya cara memeriksa ulang status
   tanpa `GET` — didokumentasikan sebagai trik resmi yang didukung desain backend (dijelaskan di
   Ringkasan), bukan penyalahgunaan API.

## Endpoint yang dikonsumsi

### Health Services / Billing Management / Billing / Financial Exceptions

Base URL: `api/v1/health-services/billing-management/billing/financial-exceptions`

| Method | Path | Kegunaan | Hak akses | Idempotency-Key | Request | Response |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/refunds` | Mengajukan refund case | `BillingRefund : Create` | Wajib | `CreateRefundRequest` (`invoiceId`, `refundableCreditId`, `requestedAmount`, `reason`, `correlationId`, `causationId`) | `ApiResponse<RefundResponse>` |
| `POST` | `/refunds/{id}/approve` | Menyetujui (dan mengeksekusi) refund case | `BillingRefund : Approve` | Tidak dipakai | `RefundApprovalRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<RefundResponse>` |
| `POST` | `/adjustments` | Mengajukan adjustment | `BillingAdjustment : Create` | Wajib | `CreateAdjustmentRequest` (`invoiceId`, `direction`, `amount`, `expectedInvoiceRowVersion`, `reason`, `correlationId`, `causationId`) | `ApiResponse<AdjustmentResponse>` |
| `POST` | `/adjustments/{id}/approve` | Menyetujui adjustment | `BillingAdjustment : Approve` | Tidak dipakai | `AdjustmentApprovalRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<AdjustmentResponse>` |
| `POST` | `/write-offs` | Mengajukan write-off | `BillingWriteOff : Create` | Wajib | `CreateWriteOffRequest` (`invoiceId`, `amount`, `expectedInvoiceRowVersion`, `reason`, `correlationId`, `causationId`) | `ApiResponse<WriteOffResponse>` |
| `POST` | `/write-offs/{id}/approve` | Menyetujui write-off | `BillingWriteOff : Approve` | Tidak dipakai | `WriteOffApprovalRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<WriteOffResponse>` |
| `POST` | `/{type}/{id}/reverse` | Membalik adjustment/write-off yang `POSTED` (`type` = `adjustments`\|`write-offs`) | `BillingFinancialException : Reverse` | Tidak dipakai | `ReverseExceptionRequest` (`expectedRowVersion`, `reason`) | `ApiResponse<AdjustmentResponse>` (reversal selalu berupa entry Adjustment baru) |

**Tidak ada endpoint `GET` sama sekali di controller ini** — lihat Temuan #1.

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil (approve/reverse), atau create yang merupakan replay dari `Idempotency-Key` yang sama. |
| `201` | Case/entry baru berhasil dibuat. |
| `403` | Bukan pihak yang berwenang — mis. pengaju mencoba menyetujui pengajuannya sendiri. |
| `404` | Invoice, refundable credit, atau case tidak ditemukan. |
| `409` | Row Version usang, correlation/idempotency key sudah diproses, atau kredit sudah punya case aktif lain. |
| `422` | Aturan bisnis tidak terpenuhi — nominal melebihi saldo/outstanding, invoice tidak lagi bisa dimutasi, refund untuk invoice RANAP, dsb. |

Bukti kode (repository, path, baris, commit):

- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinancialExceptionsController.cs` (ketujuh endpoint, tidak ada `GET`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs:32-126` (`CreateAdjustmentAsync`), `:128-199` (`ApproveAdjustmentAsync` — self-approval block baris `156-158`), `:201-300` (`CreateWriteOffAsync`), `:302-383` (`ApproveWriteOffAsync` — cek outstanding dua kali), `:385-497` (`ReverseAdjustmentAsync`), `:499-593` (`ReverseWriteOffAsync`), `:595-629` (`CalculateOutstandingAsync` — belum diekspos ke `InvoiceDetailResponse`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs:34-161` (`CreateAsync` — blok RANAP baris `74-76`, alokasi proporsional `BuildProportionalLines:473-490`), `:163-249` (`ApproveAsync` — retry lewat status `Approved`/`PartiallyExecuted` baris `199-202`), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundableCredit.cs` (bukti tidak ada endpoint yang mengekspos `Id` entitas ini), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingAllocationDtos.cs:29` (`RefundableCredit` hanya `decimal`, bukan ID), commit `22bf9cf`.
- `NewQuilvianSystemBackend`, `Areas/HealthServices/BillingManagement/Billing/Models/BilAdjustment.cs:37-42`, `BilWriteOffCase.cs:29-34`, `BilRefundCase.cs:33-40`, `BilRefundLine.cs:28-33` (enum status, termasuk `Rejected` yang tidak pernah di-set — Temuan #3), commit `22bf9cf`.
- `QuilvianSystemFrontendDev`, `src/lib/state/slice/health-services/billing-management/billing-financial-exception-slice.jsx` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-financial-exception.js`, `billing-financial-exception-constants.js` (**baru**) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-financial-exception-panel.jsx`, `create-refund-modal.jsx`, `create-adjustment-modal.jsx`, `create-write-off-modal.jsx`, `approve-exception-modal.jsx`, `reverse-exception-modal.jsx` (**baru**, 6 file) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/components/view/health-services/billing-management/billing-invoices/detail/billing-invoice-detail-view.jsx` (panel dirender, gating `refundEnabled`/`ledgerMutable`) — belum di-commit.
- `QuilvianSystemFrontendDev`, `src/lib/state/store.jsx` (registrasi reducer baru) — belum di-commit.

## Acceptance criteria (dari `roadmap/frontend-roadmap.md`, `FE-BKC-008`)

| Acceptance criteria | Status | Bukti |
| --- | --- | --- |
| Write-off tidak dilabel PAID | **Terpenuhi** | Invoice hasil write-off penuh berstatus `SETTLED_BY_WRITE_OFF`, badge status sudah membedakannya sejak `FE-BKC-001` (`BILLING_INVOICE_STATUS_BADGE_CONFIG`), bukan status "lunas". |
| Maker tidak bisa approve pengajuannya sendiri | **Terpenuhi** | Tombol "Setujui" pada case terlacak dinonaktifkan bila `requestedBy === currentUserId` (hint UI); backend menegakkan dengan `403` (`BillingFinancialExceptionForbiddenException`/`BillingRefundForbiddenException`) sebagai penegak final. Untuk case yang direlai manual, tidak ada cara UI mengetahui pengaju sebelumnya — mengandalkan `403` backend. |
| Partial outcome terlihat | **Terpenuhi** | Refund case menampilkan `RequestedAmount` vs `ExecutedAmount` dan status `PARTIALLY_EXECUTED` beserta status per baris (`RefundLineResponse.Status`) di panel. |
| Riwayat asal immutable | **Terpenuhi** | Reversal selalu membuat entry Adjustment baru (`ReversesAdjustmentId`/`ReversesWriteOffCaseId`); entry/case asal tidak pernah diedit atau dihapus — dikonfirmasi langsung dari kode service (`BillingFinancialExceptionService.ReverseAdjustmentAsync`/`ReverseWriteOffAsync`). |
| Case list/detail (scope roadmap) | **Tidak terpenuhi — gap backend** | Tidak ada endpoint apa pun untuk mendaftar/melihat case selain lewat response create/approve sendiri. Lihat `ISSUE-FE-008`. Diselesaikan sebagian dengan pelacakan lokal (localStorage) + relay ID manual, bukan case-list sungguhan. |
| Impact preview sebelum submit | **Tidak terpenuhi — gap backend** | Tidak ada endpoint "preview" (mis. estimasi outstanding setelah write-off) yang bisa dipanggil sebelum submit; `CalculateOutstandingAsync` hanya dipakai internal backend, tidak diekspos. Form hanya menampilkan info statis (`InformationAlert`) tentang aturan yang berlaku. |

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint <11 file yang diubah/baru>` (severity penuh) | **PASS** | Tanpa output (setelah memperbaiki 2 warning React hooks — `set-state-in-effect` dan `refs during render` — pada `use-billing-financial-exception.js`, dirapikan sebelum lint final). |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 64s`. |
| `npm run test:unit` | **PASS** | 38 test, 38 pass, 0 fail. Tidak ada test yang menguji kode pengecualian finansial. |
| Smoke-test browser headless tanpa login (3 route) | **PARTIAL PASS** | 0 exception JS pada `/login`, daftar invoice, dan daftar shift kasir. Halaman detail invoice (tempat panel ini dirender) butuh route token/ID invoice nyata + auth, tidak bisa dijangkau tanpa login — konsisten dengan keterbatasan smoke-test pada task-task sebelumnya. |
| Verifikasi manual ter-autentikasi (≥2 akun dengan hak Create+Approve berbeda) | **NOT DONE** | Tidak ada kredensial. Butuh minimal 2 akun dengan permission berbeda (satu `Create`, satu `Approve`) untuk menguji maker-checker penuh, plus data invoice OPEN dengan tender berhasil (untuk refund) dan refundable credit yang diketahui ID-nya (lewat query manual database, karena tidak ada endpoint pencariannya — lihat Temuan #2). |

**Task ini belum bisa ditandai selesai sepenuhnya** — lint/build/test:unit lulus bersih, tapi
klik-coba langsung (ajukan refund/adjustment/write-off, approve oleh pengguna kedua, reversal)
belum dijalankan sama sekali, dan refund secara khusus tidak bisa diuji end-to-end tanpa akses
database langsung untuk menemukan `RefundableCreditId` yang valid.

## Langkah berikutnya yang direkomendasikan

1. **Prioritas tinggi**: tambahkan endpoint `GET` untuk financial-exceptions (minimal by-id,
   idealnya juga daftar per-invoice dan daftar "menunggu approval saya") — lihat `ISSUE-FE-008`.
   Tanpa ini, fitur maker-checker inti dari task ini secara struktural tidak bisa dipakai dengan
   layak oleh approver yang bukan pengaju sendiri.
2. Tambahkan endpoint pencarian `RefundableCreditId` per invoice, supaya form refund tidak lagi
   memakai input teks bebas.
3. Login dengan minimal 2 akun (hak `Create` dan `Approve` berbeda) di `localhost:3000`, siapkan
   invoice OPEN dengan tender berhasil dan refundable credit yang diketahui, verifikasi manual
   seluruh acceptance criteria di atas.
4. Pertimbangkan menambahkan endpoint reject eksplisit, atau hapus status `REJECTED` yang tidak
   pernah dipakai dari enum backend (Temuan #3), supaya kontrak lebih jujur terhadap perilaku nyata.
5. Lanjut ke `FE-BKC-009` (finalisasi invoice) atau `FE-BKC-010` (security/privacy/concurrency).
