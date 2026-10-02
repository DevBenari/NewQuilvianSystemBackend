# Kontrak API — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-API-1.0` |
| Status | `draft` |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | — / — |
| Input revision | `00-interview-decisions.md` revisi 1, `02-backend-architecture.md` revisi 1 |
| Input hash (decisions) | `74529813218c0354e9289b4f9eea5a2bc68205572e195333b0a784e11cfccc2a` |
| Dampak kompatibilitas | **Nol.** Seluruh endpoint di bawah baru; tidak ada endpoint existing yang berubah bentuk atau dihapus |
| Backend SHA | `09101d05` |

Seluruh endpoint pada dokumen ini berlabel **Rencana (belum tersedia)** kecuali dinyatakan
lain. Belum ada satu baris pun yang ditulis di source.

## Aturan yang berlaku untuk semua grup

| Hal | Ketentuan |
|---|---|
| Pembungkus respons | `ApiResponse<T>` — `Ok(data, pesan)` untuk berhasil, `Fail(kode, pesan)` untuk gagal |
| Bentuk daftar | `ApiResponse<PagedResult<T>>` dengan `PageNumber`, `PageSize`, `TotalData`, `TotalPage`, `Items` |
| Paging bawaan | `PageNumber = 1`, `PageSize = 25`, batas `PageSize` 1–100 |
| Route | Satu route kanonik `api/v1/corporate/finance-management/...`. Alias `health-services/...` **MUST NOT** dipakai controller baru |
| Autentikasi | `[Authorize]` di seluruh controller |
| Perintah yang memindahkan uang | Wajib header `Idempotency-Key` bertipe `Guid` (`FIN-DES-006`) |
| Perintah yang mengubah data | Wajib `ExpectedRowVersion` di body (`FIN-DES-005`) |

### Arti kode status bagi pengguna

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Permintaan berhasil |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `401` | Pengguna belum masuk atau sesinya sudah berakhir |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |
| `404` | Data yang dicari tidak ditemukan |
| `409` | Data sudah diubah orang lain, atau data yang sama sudah pernah dibuat. Muat ulang sebelum melanjutkan |
| `422` | Permintaannya benar bentuknya, tetapi melanggar aturan bisnis — misalnya saldo tidak cukup atau pengaju menyetujui permohonannya sendiri |

---

## Corporate / Finance Management / Billing Intake

Base URL: `api/v1/corporate/finance-management/billing-intake`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar fakta dari Billing beserta apakah sudah berhasil diolah | `FinanceBillingIntake : Read` | `BillingIntakeQuery` | `ApiResponse<PagedResult<BillingIntakeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Melihat rincian satu fakta, termasuk pesan galat bila gagal | `FinanceBillingIntake : Read` | — | `ApiResponse<BillingIntakeDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah fakta per status untuk kartu pemantauan | `FinanceBillingIntake : Read` | — | `ApiResponse<BillingIntakeSummaryResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/consume` | Menarik dan mengolah fakta baru dari Billing secara manual | `FinanceBillingIntake : Consume` | `Idempotency-Key`, `ConsumeIntakeRequest` | `ApiResponse<BillingIntakeConsumeResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/retry` | Mengulang pengolahan satu fakta yang sebelumnya gagal | `FinanceBillingIntake : Consume` | `Idempotency-Key`, `RetryIntakeRequest` | `ApiResponse<BillingIntakeResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Receivable

Base URL: `api/v1/corporate/finance-management/receivables`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar piutang dengan penyaringan penjamin, status, dan jatuh tempo | `FinanceReceivable : Read` | `ReceivableQuery` | `ApiResponse<PagedResult<ReceivableResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu piutang beserta item, dokumen, dan riwayat pelunasan | `FinanceReceivable : Read` | — | `ApiResponse<ReceivableDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan total piutang, sudah tertagih, dan sisa | `FinanceReceivable : Read` | `ReceivableSummaryQuery` | `ApiResponse<ReceivableSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/aging` | Umur piutang dalam empat kelompok 0-30, 31-60, 61-90, di atas 90 hari | `FinanceReceivable : Read` | `ReceivableAgingQuery` | `ApiResponse<ReceivableAgingResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/filters/metadata` | Daftar pilihan penyaring untuk layar daftar | `FinanceReceivable : Read` | — | `ApiResponse<ReceivableFilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id:guid}/claim-status` | Memperbarui status kelengkapan berkas klaim | `FinanceReceivable : Update` | `UpdateClaimStatusRequest` | `ApiResponse<ReceivableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/documents` | Menambah berkas klaim ke daftar periksa | `FinanceReceivable : Update` | `AddReceivableDocumentRequest` | `ApiResponse<ReceivableDocumentResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id:guid}/documents/{documentId:guid}` | Menandai satu berkas sudah diterima | `FinanceReceivable : Update` | `UpdateReceivableDocumentRequest` | `ApiResponse<ReceivableDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/adjustments` | Mengajukan koreksi nilai piutang | `FinanceReceivable : RequestAdjustment` | `Idempotency-Key`, `CreateReceivableAdjustmentRequest` | `ApiResponse<ReceivableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{adjustmentId:guid}/approve` | Menyetujui koreksi yang diajukan orang lain | `FinanceReceivable : ApproveAdjustment` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<ReceivableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{adjustmentId:guid}/reject` | Menolak koreksi beserta alasannya | `FinanceReceivable : ApproveAdjustment` | `RejectRequest` | `ApiResponse<ReceivableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/write-offs` | Mengajukan penghapusan piutang tak tertagih | `FinanceReceivable : RequestWriteOff` | `Idempotency-Key`, `CreateWriteOffRequest` | `ApiResponse<ReceivableWriteOffResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/write-offs/{writeOffId:guid}/approve` | Menyetujui penghapusan yang diajukan orang lain | `FinanceReceivable : ApproveWriteOff` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<ReceivableWriteOffResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/write-offs/{writeOffId:guid}/reject` | Menolak penghapusan beserta alasannya | `FinanceReceivable : ApproveWriteOff` | `RejectRequest` | `ApiResponse<ReceivableWriteOffResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Receipt

Base URL: `api/v1/corporate/finance-management/receipts`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar penerimaan dengan penyaringan metode, shift, dan tanggal | `FinanceReceipt : Read` | `ReceiptQuery` | `ApiResponse<PagedResult<ReceiptResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu penerimaan beserta seluruh alokasinya | `FinanceReceipt : Read` | — | `ApiResponse<ReceiptDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/register` | Buku penerimaan kasir: penerimaan per tanggal dengan telusur ke tender dan kwitansi | `FinanceReceipt : Read` | `ReceiptRegisterQuery` | `ApiResponse<PagedResult<ReceiptRegisterResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/shift-reconciliation` | Membandingkan total penerimaan tunai Finance dengan `SystemCash` milik Billing per shift | `FinanceReceipt : Read` | `ShiftReconciliationQuery` | `ApiResponse<ShiftReconciliationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat penerimaan yang tidak berasal dari kasir, misalnya transfer penjamin | `FinanceReceipt : Create` | `Idempotency-Key`, `CreateReceiptRequest` | `ApiResponse<ReceiptResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/allocations` | Mengalokasikan uang penerimaan ke satu atau beberapa piutang | `FinanceReceipt : Allocate` | `Idempotency-Key`, `AllocateReceiptRequest` | `ApiResponse<ReceiptDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/allocations/{allocationId:guid}/reverse` | Membatalkan satu alokasi dengan membuat baris pembalik | `FinanceReceipt : Allocate` | `Idempotency-Key`, `ReverseAllocationRequest` | `ApiResponse<ReceiptDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reverse` | Membalik seluruh penerimaan karena tender aslinya dibatalkan Billing | `FinanceReceipt : Reverse` | `Idempotency-Key`, `ReverseReceiptRequest` | `ApiResponse<ReceiptResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Supplier Payable

Base URL: `api/v1/corporate/finance-management/supplier-payables`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar utang supplier beserta sisa yang belum dibayar | `FinanceSupplierPayable : Read` | `SupplierPayableQuery` | `ApiResponse<PagedResult<SupplierPayableResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu utang beserta item dan riwayat pembayaran | `FinanceSupplierPayable : Read` | — | `ApiResponse<SupplierPayableDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/aging` | Umur utang supplier per kelompok jatuh tempo | `FinanceSupplierPayable : Read` | `PayableAgingQuery` | `ApiResponse<PayableAgingResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menginput faktur supplier menjadi utang | `FinanceSupplierPayable : Create` | `Idempotency-Key`, `CreateSupplierPayableRequest` | `ApiResponse<SupplierPayableResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbaiki data utang yang belum pernah dibayar | `FinanceSupplierPayable : Update` | `UpdateSupplierPayableRequest` | `ApiResponse<SupplierPayableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/adjustments` | Mengajukan koreksi atau retur atas utang | `FinanceSupplierPayable : RequestAdjustment` | `Idempotency-Key`, `CreatePayableAdjustmentRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{adjustmentId:guid}/approve` | Menyetujui koreksi utang | `FinanceSupplierPayable : ApproveAdjustment` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Doctor Payable

Base URL: `api/v1/corporate/finance-management/doctor-payables`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar utang jasa medis dokter | `FinanceDoctorPayable : Read` | `DoctorPayableQuery` | `ApiResponse<PagedResult<DoctorPayableResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu utang dokter beserta layanan penyusunnya | `FinanceDoctorPayable : Read` | — | `ApiResponse<DoctorPayableDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/unpaid-summary` | Ringkasan per dokter: berapa fee yang belum dibayar dan sejak periode kapan | `FinanceDoctorPayable : Read` | `DoctorUnpaidSummaryQuery` | `ApiResponse<PagedResult<DoctorUnpaidSummaryResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/recognize` | Membentuk utang dari fee dokter yang sudah disetujui Medical Fee | `FinanceDoctorPayable : Create` | `Idempotency-Key`, `RecognizeDoctorPayableRequest` | `ApiResponse<DoctorPayableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/adjustments` | Mengajukan koreksi atas utang dokter | `FinanceDoctorPayable : RequestAdjustment` | `Idempotency-Key`, `CreatePayableAdjustmentRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{adjustmentId:guid}/approve` | Menyetujui koreksi utang dokter | `FinanceDoctorPayable : ApproveAdjustment` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |

`GET /unpaid-summary` adalah jawaban langsung atas keluhan yang disebut owner pada
`FIN-DEC-019`: dokter yang terlewat dibayar sebelumnya hanya ketahuan lewat Excel terpisah.

---

## Corporate / Finance Management / Payment

Base URL: `api/v1/corporate/finance-management/payments`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pembayaran keluar beserta statusnya | `FinancePayment : Read` | `PaymentQuery` | `ApiResponse<PagedResult<PaymentResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu pembayaran beserta seluruh utang yang dilunasinya | `FinancePayment : Read` | — | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menyusun pembayaran baru, boleh mencakup banyak utang sekaligus | `FinancePayment : Create` | `Idempotency-Key`, `CreatePaymentRequest` | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Mengubah rincian pembayaran yang masih berstatus draf | `FinancePayment : Update` | `UpdatePaymentRequest` | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan pembayaran untuk disetujui | `FinancePayment : Submit` | `SubmitPaymentRequest` | `ApiResponse<PaymentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui pembayaran yang diajukan orang lain | `FinancePayment : Approve` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PaymentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak pembayaran beserta alasannya | `FinancePayment : Approve` | `RejectRequest` | `ApiResponse<PaymentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/mark-paid` | Menandai uang sudah benar-benar keluar, beserta nomor bukti transfer | `FinancePayment : MarkPaid` | `Idempotency-Key`, `MarkPaymentPaidRequest` | `ApiResponse<PaymentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan pembayaran yang belum dibayar | `FinancePayment : Cancel` | `CancelPaymentRequest` | `ApiResponse<PaymentResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Bank Deposit

Base URL: `api/v1/corporate/finance-management/bank-deposits`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar setoran bank | `FinanceBankDeposit : Read` | `BankDepositQuery` | `ApiResponse<PagedResult<BankDepositResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu setoran | `FinanceBankDeposit : Read` | — | `ApiResponse<BankDepositResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/available-balance` | Melihat berapa kas yang masih boleh disetor hari ini | `FinanceBankDeposit : Read` | `AvailableBalanceQuery` | `ApiResponse<AvailableCashBalanceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf setoran | `FinanceBankDeposit : Create` | `CreateBankDepositRequest` | `ApiResponse<BankDepositResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/post` | Memposting setoran; saldo tersedia dihitung ulang saat ini juga | `FinanceBankDeposit : Post` | `Idempotency-Key`, `PostBankDepositRequest` | `ApiResponse<BankDepositResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/verify` | Menandai setoran sudah cocok dengan rekening koran bank | `FinanceBankDeposit : Verify` | `VerifyBankDepositRequest` | `ApiResponse<BankDepositResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan setoran yang belum diposting | `FinanceBankDeposit : Cancel` | `CancelBankDepositRequest` | `ApiResponse<BankDepositResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Daily Cash

Base URL: `api/v1/corporate/finance-management/daily-cash`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/current` | Posisi kas hari berjalan | `FinanceDailyCash : Read` | — | `ApiResponse<DailyCashResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Riwayat posisi kas per tanggal | `FinanceDailyCash : Read` | `DailyCashQuery` | `ApiResponse<PagedResult<DailyCashResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{cashDate}/breakdown` | Rincian penyusun angka satu tanggal, dapat ditelusuri ke penerimaan dan setoran | `FinanceDailyCash : Read` | — | `ApiResponse<DailyCashBreakdownResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{cashDate}/close` | Menutup kas satu tanggal; angkanya dibekukan setelah ini | `FinanceDailyCash : Close` | `Idempotency-Key`, `CloseDailyCashRequest` | `ApiResponse<DailyCashResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Master Data / Bank

Base URL: `api/v1/corporate/finance-management/master-data/banks`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar bank | `FinanceBank : Read` | `BankQuery` | `ApiResponse<PagedResult<BankResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Daftar pilihan bank untuk isian formulir | `FinanceBank : Read` | `onlyActive`, `search` | `ApiResponse<List<BankOptionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu bank | `FinanceBank : Read` | — | `ApiResponse<BankResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah bank | `FinanceBank : Create` | `CreateBankRequest` | `ApiResponse<BankResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbarui bank | `FinanceBank : Update` | `UpdateBankRequest` | `ApiResponse<BankResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan atau menonaktifkan bank | `FinanceBank : Update` | `UpdateStatusRequest` | `ApiResponse<BankResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id:guid}` | Menghapus bank yang belum pernah dipakai | `FinanceBank : Delete` | — | `ApiResponse<BankDeleteResponse>` | **Rencana (belum tersedia)** |

## Corporate / Finance Management / Master Data / Bank Account

Base URL: `api/v1/corporate/finance-management/master-data/bank-accounts`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar rekening rumah sakit | `FinanceBankAccount : Read` | `BankAccountQuery` | `ApiResponse<PagedResult<BankAccountResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan rekening untuk formulir setoran dan pembayaran | `FinanceBankAccount : Read` | `accountType`, `onlyActive` | `ApiResponse<List<BankAccountOptionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu rekening | `FinanceBankAccount : Read` | — | `ApiResponse<BankAccountResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah rekening | `FinanceBankAccount : Create` | `CreateBankAccountRequest` | `ApiResponse<BankAccountResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbarui rekening | `FinanceBankAccount : Update` | `UpdateBankAccountRequest` | `ApiResponse<BankAccountResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan atau menonaktifkan rekening | `FinanceBankAccount : Update` | `UpdateStatusRequest` | `ApiResponse<BankAccountResponse>` | **Rencana (belum tersedia)** |

## Corporate / Finance Management / Master Data / Currency

Base URL: `api/v1/corporate/finance-management/master-data/currencies`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar mata uang | `FinanceCurrency : Read` | `CurrencyQuery` | `ApiResponse<PagedResult<CurrencyResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan mata uang untuk formulir | `FinanceCurrency : Read` | `onlyActive` | `ApiResponse<List<CurrencyOptionResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah mata uang | `FinanceCurrency : Create` | `CreateCurrencyRequest` | `ApiResponse<CurrencyResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbarui mata uang | `FinanceCurrency : Update` | `UpdateCurrencyRequest` | `ApiResponse<CurrencyResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan atau menonaktifkan mata uang | `FinanceCurrency : Update` | `UpdateStatusRequest` | `ApiResponse<CurrencyResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}/exchange-rates` | Riwayat kurs satu mata uang | `FinanceCurrency : Read` | `ExchangeRateQuery` | `ApiResponse<PagedResult<ExchangeRateResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/exchange-rates` | Menambah kurs satu tanggal | `FinanceCurrency : Update` | `CreateExchangeRateRequest` | `ApiResponse<ExchangeRateResponse>` | **Rencana (belum tersedia)** |

---

## Corporate / Finance Management / Accounting Event Monitor

Base URL: `api/v1/corporate/finance-management/accounting-events`
Contract version: `FIN-API-1.0` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kejadian yang akan dikirim ke Accounting beserta statusnya | `FinanceAccountingEvent : Read` | `AccountingEventQuery` | `ApiResponse<PagedResult<AccountingEventResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu kejadian beserta riwayat percobaan kirim | `FinanceAccountingEvent : Read` | — | `ApiResponse<AccountingEventDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah kejadian per status untuk kartu pemantauan | `FinanceAccountingEvent : Read` | — | `ApiResponse<AccountingEventSummaryResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/retry` | Mengirim ulang satu kejadian yang gagal | `FinanceAccountingEvent : Retry` | `Idempotency-Key`, `RetryEventRequest` | `ApiResponse<AccountingEventResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/subledger-balances` | Daftar saldo subledger per periode | `FinanceAccountingEvent : Read` | `SubledgerBalanceQuery` | `ApiResponse<PagedResult<SubledgerBalanceResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/subledger-balances` | Menyusun saldo subledger satu periode untuk dikirim ke Accounting | `FinanceAccountingEvent : Submit` | `Idempotency-Key`, `CreateSubledgerBalanceRequest` | `ApiResponse<SubledgerBalanceResponse>` | **Rencana (belum tersedia)** |

`POST /{id}/retry` **tidak** membuat kejadian baru. Ia hanya mengirim ulang baris yang sama,
karena nomor kejadian dan identitas sumbernya sudah terkunci dua unique index.

---

## Endpoint yang sudah tersedia dan tidak diubah

Kedua grup berikut sudah berjalan dan **tidak** disentuh desain ini. Dicantumkan agar pembaca
tahu keseluruhan permukaan API modul.

| Grup | Base URL | Status |
|---|---|---|
| `Corporate / Finance Management / Petty Cash / Budget` | `api/v1/corporate/finance-management/petty-cash/budget` | **Tersedia** — 9 endpoint |
| `Corporate / Finance Management / Master Data / Petty Cash Category` | `api/v1/corporate/finance-management/master-data/petty-cash-categories` | **Tersedia** — 9 endpoint |

Keduanya juga memiliki route alias `api/v1/health-services/billing-management/...` yang masih
berlaku untuk kompatibilitas. Alias itu **MUST NOT** ditiru controller baru.

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Contract version | `FIN-API-0.2` — status `locked` 20 September 2026 |
| Tanggal | 20 September 2026 |
| Keputusan | `FIN-DES-025`..`028` |
| Dampak kompatibilitas | **Nol konsumen rusak.** Seluruh endpoint yang berubah berlabel `Rencana (belum tersedia)` dan belum pernah ada kodenya |

## A.1 Grup yang diganti namanya

Grup **Corporate / Finance Management / Doctor Payable** pada revisi 1 **digantikan** grup di
bawah. Base URL, nama DTO, dan hak aksesnya ikut berubah.

## Corporate / Finance Management / Medical Service Payable

Base URL: `api/v1/corporate/finance-management/medical-service-payables`
Contract version: `FIN-API-0.2` — status `locked` 20 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar utang jasa tenaga medis, dapat disaring per jenis penerima dan periode | `FinanceMedicalServicePayable : Read` | `MedicalServicePayableQuery` | `ApiResponse<PagedResult<MedicalServicePayableResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu utang beserta layanan penyusunnya | `FinanceMedicalServicePayable : Read` | — | `ApiResponse<MedicalServicePayableDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/unpaid-summary` | Ringkasan per penerima: berapa jasa yang belum dibayar dan sejak periode kapan | `FinanceMedicalServicePayable : Read` | `UnpaidSummaryQuery` | `ApiResponse<PagedResult<UnpaidSummaryResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/recognize` | Membentuk utang dari hasil jasa yang sudah disetujui Medical Fee | `FinanceMedicalServicePayable : Create` | `Idempotency-Key`, `RecognizeMedicalServicePayableRequest` | `ApiResponse<MedicalServicePayableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/adjustments` | Mengajukan koreksi atas utang jasa | `FinanceMedicalServicePayable : RequestAdjustment` | `Idempotency-Key`, `CreatePayableAdjustmentRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{adjustmentId:guid}/approve` | Menyetujui koreksi utang jasa | `FinanceMedicalServicePayable : ApproveAdjustment` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PayableAdjustmentResponse>` | **Rencana (belum tersedia)** |

## A.2 Endpoint baru pada grup Payment

Base URL: `api/v1/corporate/finance-management/payments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id:guid}/deductions` | Daftar potongan dan tambahan pada satu pembayaran | `FinancePayment : Read` | — | `ApiResponse<List<PaymentDeductionResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/deductions` | Menambah satu pos potongan atau tambahan | `FinancePayment : Update` | `CreatePaymentDeductionRequest` | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id:guid}/deductions/{deductionId:guid}` | Menghapus pos yang salah, selama pembayaran masih draf | `FinancePayment : Update` | — | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |

Ketiganya hanya tersedia selama pembayaran berstatus `DRAFT`. Setelah diajukan, potongan tidak
dapat diubah — koreksi memakai pembayaran baru.

## A.3 Perubahan bentuk response `PaymentDetailResponse`

Tiga field ditambahkan. Tidak ada field yang dihapus atau berganti arti.

| Field | Tipe | Keterangan |
|---|---|---|
| `deductionAmount` | `decimal` | Jumlah seluruh potongan |
| `additionAmount` | `decimal` | Jumlah seluruh tambahan |
| `netTransferAmount` | `decimal` | Uang yang benar-benar ditransfer |

Layar **MUST** menampilkan `netTransferAmount` sebagai angka yang ditransfer, dan
`totalAmount` sebagai jumlah utang yang dilunasi. Menampilkan salah satunya saja akan
menyesatkan: yang pertama tidak menjelaskan utang mana yang lunas, yang kedua tidak sama dengan
uang yang keluar.

---

# AMENDMENT REVISI 4

| Field | Nilai |
|---|---|
| Contract version | `FIN-API-1.1` — status `locked` 25 September 2026 (disetujui Yasmin bersama `FIN-DES-037`..`044`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-045`..`055`, `FIN-DES-037`..`044` |
| Dampak kompatibilitas | **Nol konsumen rusak.** Seluruh endpoint di bawah baru; tidak ada endpoint `FIN-API-1.0` yang berubah bentuk atau dihapus |

## B.1 Corporate / Finance Management / Purchasing / Purchase Order

Base URL: `api/v1/corporate/finance-management/purchasing/purchase-orders`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar PO dengan penyaringan supplier, status, tanggal | `FinancePurchaseOrder : Read` | `PurchaseOrderQuery` | `ApiResponse<PagedResult<PurchaseOrderResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian PO beserta baris item dan riwayat approval | `FinancePurchaseOrder : Read` | — | `ApiResponse<PurchaseOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat PO baru berstatus `DRAFT` | `FinancePurchaseOrder : Create` | `Idempotency-Key`, `CreatePurchaseOrderRequest` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbaiki PO selama masih `DRAFT` | `FinancePurchaseOrder : Update` | `UpdatePurchaseOrderRequest` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan PO untuk approval; `ApprovalTier` dihitung backend | `FinancePurchaseOrder : Submit` | `Idempotency-Key` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui PO — ditolak `422` bila pengaju = penyetuju | `FinancePurchaseOrder : Approve` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak PO beserta alasan | `FinancePurchaseOrder : Approve` | `RejectRequest` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan PO yang belum ada GR | `FinancePurchaseOrder : Update` | `Idempotency-Key`, `CancelRequest` | `ApiResponse<PurchaseOrderResponse>` | **Rencana (belum tersedia)** |

## B.2 Corporate / Finance Management / Purchasing / Goods Receipt

Base URL: `api/v1/corporate/finance-management/purchasing/goods-receipts`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar Tanda Terima Barang, disaring per PO/supplier | `FinanceGoodsReceipt : Read` | `GoodsReceiptQuery` | `ApiResponse<PagedResult<GoodsReceiptResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu GR beserta baris item | `FinanceGoodsReceipt : Read` | — | `ApiResponse<GoodsReceiptDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat penerimaan barang terhadap satu PO | `FinanceGoodsReceipt : Create` | `Idempotency-Key`, `CreateGoodsReceiptRequest` | `ApiResponse<GoodsReceiptResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan GR yang belum menjadi Tukar Faktur | `FinanceGoodsReceipt : Update` | `Idempotency-Key`, `CancelRequest` | `ApiResponse<GoodsReceiptResponse>` | **Rencana (belum tersedia)** |

## B.3 Corporate / Finance Management / Purchasing / Invoice Exchange

Base URL: `api/v1/corporate/finance-management/purchasing/invoice-exchanges`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar Tukar Faktur, disaring supplier/status | `FinanceInvoiceExchange : Read` | `InvoiceExchangeQuery` | `ApiResponse<PagedResult<InvoiceExchangeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian satu Tukar Faktur | `FinanceInvoiceExchange : Read` | — | `ApiResponse<InvoiceExchangeDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat Tukar Faktur; `EstimatedDueDate` dihitung backend dari TOP supplier | `FinanceInvoiceExchange : Create` | `Idempotency-Key`, `CreateInvoiceExchangeRequest` | `ApiResponse<InvoiceExchangeResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan Tukar Faktur yang belum menjadi Purchasing Invoice | `FinanceInvoiceExchange : Update` | `Idempotency-Key`, `CancelRequest` | `ApiResponse<InvoiceExchangeResponse>` | **Rencana (belum tersedia)** |

## B.4 Corporate / Finance Management / Purchasing / Purchasing Invoice

Base URL: `api/v1/corporate/finance-management/purchasing/purchasing-invoices`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar Purchasing Invoice, disaring supplier/status/jatuh tempo | `FinancePurchasingInvoice : Read` | `PurchasingInvoiceQuery` | `ApiResponse<PagedResult<PurchasingInvoiceResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian invoice beserta PPN, DP, potongan | `FinancePurchasingInvoice : Read` | — | `ApiResponse<PurchasingInvoiceDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat Purchasing Invoice dari satu Tukar Faktur yang belum terpakai | `FinancePurchasingInvoice : Create` | `Idempotency-Key`, `CreatePurchasingInvoiceRequest` | `ApiResponse<PurchasingInvoiceResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Memperbaiki invoice selama masih `DRAFT` | `FinancePurchasingInvoice : Update` | `UpdatePurchasingInvoiceRequest` | `ApiResponse<PurchasingInvoiceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan invoice untuk approval | `FinancePurchasingInvoice : Submit` | `Idempotency-Key` | `ApiResponse<PurchasingInvoiceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui invoice; membuat `FinSupplierPayable` dan menyiapkan (bukan mengirim) kejadian PPN Masukan | `FinancePurchasingInvoice : Approve` | `Idempotency-Key`, `ApproveRequest` | `ApiResponse<PurchasingInvoiceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak invoice beserta alasan | `FinancePurchasingInvoice : Approve` | `RejectRequest` | `ApiResponse<PurchasingInvoiceResponse>` | **Rencana (belum tersedia)** |

## B.5 Corporate / Finance Management / Purchasing / Supplier Return

Base URL: `api/v1/corporate/finance-management/purchasing/supplier-returns`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar retur pembelian | `FinanceSupplierReturn : Read` | `SupplierReturnQuery` | `ApiResponse<PagedResult<SupplierReturnResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian retur beserta status Deposit Retur turunannya | `FinanceSupplierReturn : Read` | — | `ApiResponse<SupplierReturnDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat retur atas satu Purchasing Invoice; menerbitkan `FinSupplierReturnDeposit` | `FinanceSupplierReturn : Create` | `Idempotency-Key`, `CreateSupplierReturnRequest` | `ApiResponse<SupplierReturnResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/deposits` | Daftar Deposit Retur beserta saldo tersedia, disaring per supplier | `FinanceSupplierReturn : Read` | `SupplierReturnDepositQuery` | `ApiResponse<PagedResult<SupplierReturnDepositResponse>>` | **Rencana (belum tersedia)** |
| ~~`POST`~~ | ~~`/deposits/{depositId:guid}/apply`~~ | **DICABUT oleh `FIN-API-1.2`** (AMENDMENT REVISI 5, `FIN-DES-045`) — deposit kini dipakai di dalam pembayaran lewat `POST /payments/{id}/return-deposits`, lihat C.1 | — | — | — | — |

## B.6 Corporate / Finance Management / Purchasing / Reports

Base URL: `api/v1/corporate/finance-management/purchasing/reports`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

Seluruh endpoint pada grup ini **read-only**, tidak melahirkan tabel baru (`FIN-DES-044`).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| ~~`GET`~~ | ~~`/aging`~~ | **DICABUT 25 September 2026** (`FIN-DEC-059`, `/grill-me`) — menduplikasi `GET api/finance/payable/aging` yang sudah berjalan atas `FinSupplierPayable`, yang sejak `FIN-DEC-045` juga memuat utang dari Purchasing Invoice. Layar Purchasing/AP memakai endpoint existing itu | — | — | — | — |
| `GET` | `/summary` | Rekap Purchasing AP periodik | `FinancePurchasingReport : Read` | `PurchasingSummaryQuery` | `ApiResponse<PurchasingSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/invoice-exchanges` | Laporan Tukar Faktur beserta status keterkaitan ke Purchasing Invoice | `FinancePurchasingReport : Read` | `InvoiceExchangeReportQuery` | `ApiResponse<PagedResult<InvoiceExchangeReportResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/due-dates` | Laporan Purchasing Invoice mendekati/lewat jatuh tempo | `FinancePurchasingReport : Read` | `DueDateReportQuery` | `ApiResponse<PagedResult<DueDateReportResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/reconciliation` | Rekonsiliasi Tukar Faktur yang belum menghasilkan Purchasing Invoice | `FinancePurchasingReport : Read` | `ReconciliationQuery` | `ApiResponse<PagedResult<ReconciliationResponse>>` | **Rencana (belum tersedia)** |

## B.7 Corporate / Finance Management / Receivable Invoice Batch

Base URL: `api/v1/corporate/finance-management/receivable-invoice-batches`
Contract version: `FIN-API-1.1` — status `locked` 25 September 2026

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar batch, disaring penjamin/status | `FinanceReceivableInvoiceBatch : Read` | `ReceivableInvoiceBatchQuery` | `ApiResponse<PagedResult<ReceivableInvoiceBatchResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian batch beserta daftar `FinReceivable` anggota | `FinanceReceivableInvoiceBatch : Read` | — | `ApiResponse<ReceivableInvoiceBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/eligible-receivables` | Daftar `FinReceivable` yang memenuhi syarat digabung untuk satu penjamin/periode | `FinanceReceivableInvoiceBatch : Read` | `EligibleReceivableQuery` | `ApiResponse<PagedResult<EligibleReceivableResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat batch `DRAFT` dari daftar `FinReceivable` terpilih | `FinanceReceivableInvoiceBatch : Create` | `Idempotency-Key`, `CreateReceivableInvoiceBatchRequest` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/issue` | Menerbitkan batch, mengunci daftar anggotanya | `FinanceReceivableInvoiceBatch : Issue` | `Idempotency-Key` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}/document` | Dokumen tagihan gabungan, memuat rincian per invoice dari Billing | `FinanceReceivableInvoiceBatch : Read` | — | `ApiResponse<ReceivableInvoiceBatchDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan batch `DRAFT` yang belum diterbitkan | `FinanceReceivableInvoiceBatch : Update` | `Idempotency-Key`, `CancelRequest` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |

## B.8 Endpoint baru pada grup Receipt

Base URL: `api/v1/corporate/finance-management/receipts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id:guid}/deductions` | Daftar potongan sisi penerimaan (PPh 23, biaya admin bank) pada satu penerimaan | `FinanceReceipt : Read` | — | `ApiResponse<List<ReceiptDeductionResponse>>` | **Rencana (belum tersedia)** |
| ~~`POST`~~ | ~~`/{id:guid}/deductions`~~ | **DICABUT oleh `FIN-API-1.2`** (AMENDMENT REVISI 5, `FIN-DES-048`) — potongan kini dicatat di dalam `POST /receipts/{id}/allocations`, lihat C.2 | — | — | — | — |

~~Endpoint `POST .../deductions` memakai hak akses yang sama dengan alokasi.~~ Pencabutannya
dijelaskan di C.2: setelah uang penerimaan habis teralokasi, endpoint terpisah itu akan ditolak
`FIN-VAL-120`, sehingga tidak pernah dapat dipakai pada kasus PPh 23 biasa.

## B.9 Gerbang PPN Masukan Pembelian

Endpoint `POST /purchasing/purchasing-invoices/{id}/approve` **MUST** menulis baris outbox
`PPN-MASUKAN-PEMBELIAN` berstatus `PENDING`, tetapi worker pengiriman kejadian ini **MUST NOT**
diaktifkan sampai Accounting meratifikasi kode tersebut (`FIN-OQ-020`, `FIN-DEC-046`,
`evidence/06`). Ini bukan endpoint terpisah — gerbangnya ada di level worker, bukan di level API,
sama seperti pola kode lain yang pernah menunggu ratifikasi (`FIN-DES-029`).


---

# AMENDMENT REVISI 5

| Field | Nilai |
|---|---|
| Contract version | `FIN-API-1.2` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-057`, `058`, `059`, `061`, `062`; `FIN-DES-045`..`050` |
| Dampak kompatibilitas | **Nol konsumen rusak.** Dua endpoint `FIN-API-1.1` yang dicabut (`/supplier-returns/deposits/{id}/apply`, `POST /receipts/{id}/deductions`) dan satu dari revisi yang sama (`/purchasing/reports/aging`, `FIN-DEC-059`) belum pernah punya kode. Dua endpoint yang sudah **berjalan** berubah bentuk secara aditif: `PaymentDetailResponse` bertambah field, `AllocateReceiptRequest` bertambah field opsional |

## C.1 Endpoint baru pada grup Payment — Deposit Retur sebagai sumber dana

Base URL: `api/v1/corporate/finance-management/payments`
Contract version: `FIN-API-1.2` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id:guid}/return-deposits` | Daftar Deposit Retur yang dipakai pembayaran ini, termasuk yang sudah dilepas | `FinancePayment : Read` | — | `ApiResponse<List<PaymentReturnDepositResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/return-deposits` | Mencadangkan sebagian/seluruh saldo satu Deposit Retur sebagai sumber dana pembayaran | `FinancePayment : Update` | `Idempotency-Key`, `AddPaymentReturnDepositRequest` | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id:guid}/return-deposits/{usageId:guid}` | Melepas satu baris pemakaian; saldo deposit kembali. Baris tidak dihapus, statusnya `RELEASED` | `FinancePayment : Update` | `ExpectedRowVersion` (query) | `ApiResponse<PaymentDetailResponse>` | **Rencana (belum tersedia)** |

Ketiganya hanya tersedia selama pembayaran `DRAFT` — sama seperti endpoint potongan (A.2).

**Kode status yang perlu diketahui konsumen:**

| Kode | Kapan |
|---|---|
| `400` | `UsedAmount` ≤ 0 |
| `404` | Pembayaran atau deposit tidak ditemukan |
| `409` | `ExpectedRowVersion` basi, atau deposit yang sama sudah aktif di pembayaran ini |
| `422` | Pembayaran bukan `DRAFT`; pembayaran bukan `SUPPLIER`; deposit milik supplier lain; saldo deposit kurang; `NetTransferAmount` akan menjadi negatif |

**Contoh request `POST /payments/{id}/return-deposits`**

```json
{
  "supplierReturnDepositId": "3f1c9a2e-0000-4000-8000-000000000001",
  "usedAmount": 2500000.00,
  "expectedRowVersion": "9b2e7c10-0000-4000-8000-000000000002"
}
```

**Contoh response (dipotong)**

```json
{
  "success": true,
  "data": {
    "paymentNumber": "PAY-2026-09-00088",
    "totalAmount": 10000000.00,
    "deductionAmount": 0.00,
    "additionAmount": 0.00,
    "depositAppliedAmount": 2500000.00,
    "netTransferAmount": 7500000.00,
    "returnDeposits": [
      { "returnNumber": "RTR-2026-08-00012", "usedAmount": 2500000.00, "status": "RESERVED" }
    ]
  }
}
```

## C.2 Perubahan bentuk pada endpoint yang SUDAH BERJALAN

**`PaymentDetailResponse`** — dua field ditambahkan, tidak ada yang dihapus atau berganti arti:

| Field | Tipe | Keterangan |
|---|---|---|
| `depositAppliedAmount` | `decimal` | Bagian utang yang dilunasi dari Deposit Retur |
| `returnDeposits` | `PaymentReturnDepositResponse[]` | Baris pemakaian beserta statusnya |

`netTransferAmount` **tetap** "uang yang benar-benar ditransfer"; rumusnya bertambah satu
pengurang. Layar yang sudah menampilkannya tidak perlu berubah.

**`POST /receipts/{id}/allocations`** — setiap baris `AllocateReceiptRequest` boleh membawa
`deductions[]` (opsional; tanpa field ini perilakunya persis seperti hari ini):

| Field per potongan | Tipe | Wajib | Keterangan |
|---|---|:---:|---|
| `deductionType` | `string` | Ya | `PPH23`, `BANK_ADMIN_FEE`, `OTHER` |
| `amount` | `decimal` | Ya | > 0 |
| `reason` | `string?` | Bila `OTHER` | Maks 500 |
| `referenceNumber` | `string?` | Tidak | Nomor bukti potong / bukti bank |

Potongan hanya boleh pada baris ber-`targetType = RECEIVABLE`. Uang baris + jumlah potongannya
MUST ≤ sisa piutang.

**Contoh request**

```json
{
  "expectedRowVersion": "b0a1c2d3-0000-4000-8000-000000000003",
  "lines": [
    {
      "targetType": "RECEIVABLE",
      "receivableId": "c4d5e6f7-0000-4000-8000-000000000004",
      "amount": 9745000.00,
      "deductions": [
        { "deductionType": "PPH23", "amount": 230000.00, "referenceNumber": "BP-23-0091" },
        { "deductionType": "BANK_ADMIN_FEE", "amount": 25000.00 }
      ]
    }
  ]
}
```

Hasil: piutang Rp 10.000.000 menjadi `SETTLED`; `UnallocatedAmount` penerimaan Rp 0.

## C.3 Endpoint yang dicabut

| Endpoint | Dicabut oleh | Pengganti |
|---|---|---|
| `POST /purchasing/supplier-returns/deposits/{depositId}/apply` | `FIN-DES-045` | `POST /payments/{id}/return-deposits` |
| `POST /receipts/{id}/deductions` | `FIN-DES-048` | Field `deductions[]` pada `POST /receipts/{id}/allocations` |
| `GET /purchasing/reports/aging` | `FIN-DEC-059` | `GET api/finance/payable/aging` yang sudah berjalan |

`GET /purchasing/supplier-returns/deposits` dan `GET /receipts/{id}/deductions` **tetap** — keduanya
baca saja.

# AMENDMENT REVISI 13 — Pelacakan klaim penjamin dan dua permukaan baca baru

`last_changed_in`: `FIN-API-1.3` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-094`..`FIN-DEC-098`; dirancang `FIN-DES-070`..`FIN-DES-073`.
Dampak kompatibilitas: **aditif murni.** Nol endpoint berubah bentuk, nol endpoint dicabut.

## D.1 Corporate / Finance Management / Receivable Invoice Batch — tiga aksi klaim

Base URL: `api/v1/corporate/finance-management/receivable-invoice-batches`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{id}/claim/verify` | Menandai berkas klaim sudah diterima dan dinyatakan lengkap oleh penjamin | `FinanceReceivableInvoiceBatch : Update` | `ClaimVerifyRequest` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/claim/approve` | Mencatat nominal yang disetujui penjamin | `FinanceReceivableInvoiceBatch : Update` | `ClaimApproveRequest` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/claim/close` | Menutup klaim; tidak ada tindak lanjut lagi | `FinanceReceivableInvoiceBatch : Update` | `ClaimCloseRequest` | `ApiResponse<ReceivableInvoiceBatchResponse>` | **Rencana (belum tersedia)** |

Ketiganya memakai bentuk `POST /{id}/<aksi>` sesuai `transaction-endpoint-standard`, bukan
`PATCH /{id}/status` generik. Daftar dan rincian klaim **tidak** mendapat endpoint sendiri —
layar Manajemen Klaim memakai `GET /receivable-invoice-batches` dan `GET /{id}` yang sudah ada,
yang responsnya bertambah field klaim (lihat `D.3`).

### Request

| DTO | Jenis | Field |
|---|---|---|
| `ClaimVerifyRequest` | Status | `ExpectedRowVersion` (`Guid`, wajib); `PayerClaimReference` (`string(100)`, opsional); `ClaimNote` (`string(500)`, opsional) |
| `ClaimApproveRequest` | Status | `ExpectedRowVersion` (`Guid`, wajib); `ApprovedAmount` (`decimal(18,2)`, **wajib**); `PayerClaimReference` (`string(100)`, opsional); `ClaimNote` (`string(500)`, **wajib bila** `ApprovedAmount` lebih kecil dari `TotalAmount`) |
| `ClaimCloseRequest` | Status | `ExpectedRowVersion` (`Guid`, wajib); `ClaimNote` (`string(500)`, opsional) |

`ExpectedRowVersion` mengikuti pola `ReceivableInvoiceBatchRowVersionRequest` yang sudah dipakai
`issue` dan `cancel` — bukan mekanisme baru.

### Kode status dan artinya bagi pengguna

| Kode | Arti |
|---|---|
| `200` | Perubahan status klaim tersimpan |
| `400` | Isian tidak lengkap atau tidak masuk akal, misalnya nominal disetujui bernilai minus |
| `403` | Pengguna tidak punya hak mengelola batch tagihan |
| `404` | Batch tidak ditemukan |
| `409` | Batch sudah diubah pengguna lain sejak layar dibuka; layar perlu dimuat ulang |
| `422` | Perpindahan status tidak sah menurut `state-transition-matrix.md` `D.1`, atau nominal disetujui melebihi total tagihan |

## D.2 Dua permukaan baca baru

| Grup `[Tags(...)]` | Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|---|
| `Corporate / Finance Management / Receivable` | `GET` | `/receivables/write-offs` | Daftar penghapusan piutang lintas piutang, untuk layar "Pemutihan Piutang" | `FinanceReceivable : Read` | `ReceivableWriteOffQuery` | `ApiResponse<PagedResult<ReceivableWriteOffRowResponse>>` | **Rencana (belum tersedia)** |
| `Corporate / Finance Management / Receipt` | `GET` | `/receipts/reversed-allocations` | Daftar baris alokasi penerimaan yang dibalik, untuk layar "Receiveable AR Canceled" | `FinanceReceipt : Read` | `ReversedAllocationQuery` | `ApiResponse<PagedResult<ReversedAllocationRowResponse>>` | **Rencana (belum tersedia)** |

Base URL keduanya mengikuti grup masing-masing:
`api/v1/corporate/finance-management/receivables` dan
`api/v1/corporate/finance-management/receipts`.

| DTO | Jenis | Field |
|---|---|---|
| `ReceivableWriteOffQuery` | PagedQuery | `StartDate`, `EndDate` (`DateOnly?`); `Status` (`string?` — mengikuti status write-off yang sudah ada); `DebtorReferenceId` (`Guid?`); `PageNumber`, `PageSize`, `SortBy`, `SortDirection` |
| `ReceivableWriteOffRowResponse` | Response | `Id`, `ReceivableId`, `ReceivableNumber`, `DebtorReferenceId`, `Amount`, `Reason`, `Status`, `RequestedAt`, `DecidedAt` |
| `ReversedAllocationQuery` | PagedQuery | `StartDate`, `EndDate` (`DateTimeOffset?` — disesuaikan `BE-FIN-054` mengikuti tipe `ReceiptRegisterQuery` yang sudah ada pada file DTO yang sama, bukan `DateOnly?`); `ReceiptId` (`Guid?`); `PageNumber`, `PageSize`. **Tanpa `SortBy`/`SortDirection`** — urutan tetap `AllocatedAt` menurun, mengikuti pola `ReceiptRegisterQuery` |
| `ReversedAllocationRowResponse` | Response | `AllocationId`, `ReceiptId`, `ReceiptNumber`, `ReceivableId`, `ReceivableNumber`, `Amount`, `ReversalOfAllocationId`, `ReversedAt`. **`ReversalReason` DICABUT** (`BE-FIN-054`) — `FinReceiptAllocation` tidak punya kolom alasan sama sekali, dan `ReverseAllocationAsync` tidak menerima parameter alasan. Bukan dikarang; dicatat sebagai delta kontrak-vs-source |

Keduanya **membaca saja**. Pembuatan write-off tetap lewat `POST /receivables/{id}/write-offs`
beserta maker-checker-nya, dan pembalikan alokasi tetap lewat
`POST /receipts/{id}/allocations/{allocationId}/reverse` — keduanya sudah berjalan dan **tidak**
berubah.

## D.3 Response yang bertambah field — `ReceivableInvoiceBatchResponse`

Aditif murni; konsumen lama yang mengabaikan field baru tetap berjalan.

| Field baru | Tipe | Arti |
|---|---|---|
| `ClaimStatus` | `string?` | Kosong, `SUBMITTED`, `PAYER_VERIFIED`, `APPROVED`, atau `CLOSED` |
| `ApprovedAmount` | `decimal?` | Nominal yang disetujui penjamin; kosong selama belum `APPROVED` |
| `ClaimVarianceAmount` | `decimal?` | **Dihitung, tidak disimpan**: `TotalAmount` dikurangi `ApprovedAmount`. Kosong selama belum `APPROVED` |
| `PayerClaimReference` | `string?` | Nomor rujukan klaim milik penjamin |
| `ClaimNote` | `string?` | Keterangan terakhir yang dicatat petugas |
| `PayerVerifiedAt`, `ClaimApprovedAt`, `ClaimClosedAt` | `DateTimeOffset?` | Tanda waktu tiap tahap |

`ClaimVarianceAmount` **MUST** dihitung di backend dan **MUST NOT** dihitung ulang di layar,
mengikuti aturan "nol perhitungan uang di klien" yang berlaku di seluruh blueprint ini.

## E.1 Corporate / Finance Management / Non Patient Receivable

`last_changed_in`: `FIN-API-1.4` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-099`..`FIN-DEC-104`; dirancang `FIN-DES-074`..`FIN-DES-077`.
Dampak kompatibilitas: **aditif murni** — grup endpoint baru, nol endpoint lama disentuh.

Base URL: `api/v1/corporate/finance-management/non-patient-receivables`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar tagihan sewa, bersaring kategori, status, periode, jatuh tempo | `FinanceNonPatientReceivable : Read` | `NonPatientReceivableQuery` | `ApiResponse<PagedResult<NonPatientReceivableResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian satu tagihan beserta riwayat pelunasannya | `FinanceNonPatientReceivable : Read` | — | `ApiResponse<NonPatientReceivableDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/aging` | Umur piutang sewa per kelompok, bersaring kategori | `FinanceNonPatientReceivable : Read` | `NonPatientReceivableAgingQuery` | `ApiResponse<List<ReceivableAgingBucketResult>>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan nominal per status | `FinanceNonPatientReceivable : Read` | `NonPatientReceivableSummaryQuery` | `ApiResponse<NonPatientReceivableSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/filters/metadata` | Isi pilihan saringan (kategori, status) | `FinanceNonPatientReceivable : Read` | — | `ApiResponse<NonPatientReceivableFilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat tagihan sewa baru untuk satu periode | `FinanceNonPatientReceivable : Create` | `CreateNonPatientReceivableRequest` | `ApiResponse<NonPatientReceivableResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengoreksi tagihan yang belum menerima pembayaran sama sekali | `FinanceNonPatientReceivable : Update` | `UpdateNonPatientReceivableRequest` | `ApiResponse<NonPatientReceivableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/settlements` | Mencatat pembayaran yang diterima dari penyewa | `FinanceNonPatientReceivable : Update` | `CreateNonPatientReceivableSettlementRequest` | `ApiResponse<NonPatientReceivableDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/write-off` | Menghapus piutang sewa yang tidak tertagih | `FinanceNonPatientReceivable : Update` | `WriteOffNonPatientReceivableRequest` | `ApiResponse<NonPatientReceivableResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan tagihan yang salah dicatat | `FinanceNonPatientReceivable : Update` | `CancelNonPatientReceivableRequest` | `ApiResponse<NonPatientReceivableResponse>` | **Rencana (belum tersedia)** |

Mengikuti `transaction-endpoint-standard`: **nol** `DELETE /{id}` (pembatalan memakai aksi `cancel`),
**nol** `PATCH /{id}/status` generik, dan **nol** `GET /options` (kapabilitas ini bukan master data
yang dirujuk entity lain).

### Request dan response

| DTO | Jenis | Field |
|---|---|---|
| `NonPatientReceivableQuery` | PagedQuery | `Category` (`string?` — `PARKING`/`TENANT`); `Status` (`string?`); `PeriodStart`, `PeriodEnd`, `DueDateFrom`, `DueDateTo` (`DateOnly?`); `Search` (`string?` — nama penyewa atau objek sewa); `PageNumber`, `PageSize`, `SortBy`, `SortDirection` |
| `NonPatientReceivableAgingQuery` | PagedQuery | `AsOfDate` (`DateOnly?`); `Category` (`string?`) |
| `CreateNonPatientReceivableRequest` | Create | `Category` (**wajib**); `CounterpartyName` (`string(200)`, **wajib**); `RentedObject` (`string(200)`, **wajib**); `PeriodStart`, `PeriodEnd` (**wajib**); `DueDate` (**wajib**); `BilledAmount` (`decimal`, **wajib**); `LateFeeAmount` (`decimal`, opsional, bawaan nol); `Note` (`string(500)`, opsional) |
| `UpdateNonPatientReceivableRequest` | Update | Sama dengan Create, ditambah `ExpectedRowVersion` (**wajib**) |
| `CreateNonPatientReceivableSettlementRequest` | Create | `ExpectedRowVersion` (**wajib**); `SettlementDate` (**wajib**); `Amount` (`decimal`, **wajib**, boleh negatif untuk membatalkan pelunasan sebelumnya); `PaymentMethod` (`string(50)`, **wajib**); `ReferenceNumber` (`string(100)`, opsional); `Note` (`string(500)`, opsional) |
| `WriteOffNonPatientReceivableRequest` | Status | `ExpectedRowVersion` (**wajib**); `Reason` (`string(500)`, **wajib**) |
| `CancelNonPatientReceivableRequest` | Status | `ExpectedRowVersion` (**wajib**); `Reason` (`string(500)`, **wajib**) |
| `NonPatientReceivableResponse` | Response | `Id`, `ReceivableNumber`, `Category`, `CounterpartyName`, `RentedObject`, `PeriodStart`, `PeriodEnd`, `DueDate`, `BilledAmount`, `LateFeeAmount`, `TotalBilledAmount`, `SettledAmount`, `OutstandingAmount`, `DaysPastDue`, `Status`, `Note`, `RowVersion` |
| `NonPatientReceivableDetailResponse` | Response | Seluruh field di atas, ditambah `Settlements` (daftar `SettlementRowResponse`) |
| `SettlementRowResponse` | Response | `Id`, `SettlementDate`, `Amount`, `PaymentMethod`, `ReferenceNumber`, `Note` |

`TotalBilledAmount`, `SettledAmount`, `OutstandingAmount`, dan `DaysPastDue` **MUST** dihitung
backend dan **MUST NOT** dihitung ulang di layar.

### Kode status dan artinya bagi pengguna

| Kode | Arti |
|---|---|
| `200` | Permintaan berhasil |
| `201` | Tagihan sewa baru tersimpan |
| `400` | Isian tidak lengkap atau tidak masuk akal, misalnya nominal tagihan bernilai minus |
| `403` | Pengguna tidak punya hak mengelola piutang sewa |
| `404` | Tagihan tidak ditemukan |
| `409` | Tagihan sudah diubah pengguna lain sejak layar dibuka; layar perlu dimuat ulang |
| `422` | Langkah tidak sah menurut `state-transition-matrix.md` bagian `E`, misalnya mengoreksi tagihan yang sudah menerima pembayaran |

### Yang **tidak** dilakukan endpoint ini

| Yang mungkin disangka | Kenyataannya |
|---|---|
| Pelunasan tercatat sebagai kas masuk di kas harian atau setoran bank | **Tidak.** Kedua layar itu membaca `FinReceipt`, yang tidak dilewati jalur ini. Ini **keputusan**: sewa dikelola terpisah dari kas — `FIN-DES-075`, `FIN-DEC-109` |
| Pencatatan tagihan menerbitkan kejadian akuntansi | **Tidak.** Nol kode kejadian terbit dari kapabilitas ini pada rilis pertama, menunggu ratifikasi Accounting — `FIN-OQ-044(b)`, `FIN-DEC-110` |
| Piutang sewa ikut terhitung pada umur piutang pasien | **Tidak.** `GET /receivables/aging` dan `GET /non-patient-receivables/aging` adalah dua laporan terpisah dengan definisi kelompok umur yang sama |

---

# Bagian F — Revisi 14: jalur pengiriman, buku mutasi, dan cutover

| Field | Nilai |
|---|---|
| Contract version | `FIN-API-1.5` — status **`draft`** |
| Naik dari | `FIN-API-1.4` (`approved` 1 Oktober 2026) |
| `input_revision` | `00-interview-decisions.md` closure pass 1 Oktober 2026 (`FIN-DEC-111`..`137`) |
| Owner | Yasmin (Product/Domain Finance) |
| `approved_by`, `approved_at` | — belum |
| Traceability | `FIN-DES-078`..`FIN-DES-091` |
| Dampak kompatibilitas | **ADA PERUBAHAN MEMUTUS** pada dua endpoint yang sudah berjalan — bagian `F.8` |

Seluruh endpoint baru berlabel **`Rencana (belum tersedia)`**.

## F.1 Corporate / Finance Management / Subledger Setup

Base URL: `api/v1/corporate/finance-management/subledger-setup`
`[Tags("Corporate / Finance Management / Subledger Setup")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/control-accounts` | Daftar pemetaan kelompok dan segmen ke kode akun control | `FinanceSubledgerSetup : Read` | `ControlAccountMapPagedQuery` | `ApiResponse<PagedResult<ControlAccountMapResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/control-accounts/coverage` | Memeriksa kelengkapan cakupan sebelum snapshot; mengembalikan kelompok dan segmen yang belum terpetakan | `FinanceSubledgerSetup : Read` | — | `ApiResponse<ControlAccountCoverageResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/control-accounts` | Menambah satu pemetaan | `FinanceSubledgerSetup : Create` | `CreateControlAccountMapRequest` | `ApiResponse<ControlAccountMapResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/control-accounts/{id:guid}` | Mengoreksi pemetaan yang belum dipakai snapshot | `FinanceSubledgerSetup : Update` | `UpdateControlAccountMapRequest` | `ApiResponse<ControlAccountMapResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/control-accounts/{id:guid}/deactivate` | Menonaktifkan pemetaan; barisnya disimpan sebagai riwayat | `FinanceSubledgerSetup : Update` | `DeactivateControlAccountMapRequest` | `ApiResponse<ControlAccountMapResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/opening-balances` | Daftar saldo awal per kelompok | `FinanceSubledgerSetup : Read` | — | `ApiResponse<List<OpeningBalanceResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/opening-balances` | Mencatat saldo awal satu kelompok, status `DRAFT` | `FinanceSubledgerSetup : Create` | `CreateOpeningBalanceRequest` | `ApiResponse<OpeningBalanceResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/opening-balances/{id:guid}` | Mengoreksi saldo awal yang masih `DRAFT` | `FinanceSubledgerSetup : Update` | `UpdateOpeningBalanceRequest` | `ApiResponse<OpeningBalanceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/opening-balances/{id:guid}/approve` | Menyetujui saldo awal | `FinanceSubledgerSetup : Approve` | `ApproveOpeningBalanceRequest` | `ApiResponse<OpeningBalanceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/opening-balances/{id:guid}/lock` | Mengunci; sesudahnya nilainya tidak dapat diubah | `FinanceSubledgerSetup : Approve` | `LockOpeningBalanceRequest` | `ApiResponse<OpeningBalanceResponse>` | **Rencana (belum tersedia)** |

Kode status beserta artinya bagi pengguna:

| Kode | Artinya bagi pengguna |
|---|---|
| `200` | Berhasil |
| `201` | Pemetaan atau saldo awal berhasil dicatat |
| `400` | Isian tidak lengkap atau tidak sah, misalnya segmen yang tidak dikenal untuk kelompok itu |
| `403` | Hak akses tidak mencukupi |
| `404` | Baris tidak ditemukan |
| `409` | Bertabrakan dengan keadaan sekarang, misalnya kelompok itu sudah punya baris aktif, atau saldo awal sudah `LOCKED` |
| `422` | Nilai melanggar aturan bisnis, misalnya saldo awal kelompok piutang diisi selain nol |

## F.2 Corporate / Finance Management / Opening Item Batch

Base URL: `api/v1/corporate/finance-management/opening-item-batches`
`[Tags("Corporate / Finance Management / Opening Item Batch")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar batch migrasi, bersaring jenis dan status | `FinanceOpeningItemBatch : Read` | `OpeningItemBatchPagedQuery` | `ApiResponse<PagedResult<OpeningItemBatchResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Rincian batch beserta hasil validasi per baris | `FinanceOpeningItemBatch : Read` | — | `ApiResponse<OpeningItemBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/template` | Mengunduh templat sesuai jenis item **dan format** | `FinanceOpeningItemBatch : Read` | `?itemKind=&format=` | Berkas | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengunggah berkas **CSV atau XLSX**; membuat batch `DRAFT` | `FinanceOpeningItemBatch : Create` | `multipart/form-data` | `ApiResponse<OpeningItemBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/validate` | Menjalankan validasi per baris; batch menjadi `VALIDATED` bila nol galat | `FinanceOpeningItemBatch : Update` | — | `ApiResponse<OpeningItemBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/declare-accounting-opening` | Menyatakan total saldo awal Accounting beserta rujukan dokumennya | `FinanceOpeningItemBatch : Update` | `DeclareAccountingOpeningRequest` | `ApiResponse<OpeningItemBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Membuat item piutang atau utang beserta mutasi pembukanya, lalu mengunci batch | `FinanceOpeningItemBatch : Approve` | `ApproveOpeningItemBatchRequest` | `ApiResponse<OpeningItemBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak batch beserta alasannya | `FinanceOpeningItemBatch : Update` | `RejectOpeningItemBatchRequest` | `ApiResponse<OpeningItemBatchResponse>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
|---|---|
| `400` | Berkas tidak dapat dibaca, kolom templat tidak lengkap, atau `format`/`itemKind` di luar nilai yang sah |
| `409` | Batch sudah `APPROVED`, `LOCKED`, atau `REJECTED` sehingga tindakan itu tidak berlaku lagi |
| `422` | Total sisa item tidak sama dengan saldo awal Accounting yang dinyatakan, atau masih ada baris bergalat |

### Dua format pada satu endpoint (revisi 15, `FIN-DEC-140`, `FIN-DES-093`)

| Hal | Ketentuan |
|---|---|
| Nilai `format` pada `GET /template` | `CSV` atau `XLSX`. **Wajib**, sama wajibnya dengan `itemKind`. Nilai lain ditolak `400` |
| Jumlah berkas templat | **Empat**: dua jenis item kali dua format. Kolom pada pasangan CSV/XLSX jenis yang sama **MUST** identik |
| Penentuan format saat unggah | Dari **tipe media dan ekstensi berkas**, bukan dari ruas yang diisi pengguna |
| Format di luar CSV/XLSX saat unggah | Ditolak `400` beserta keterangan format yang diterima |
| Paritas hasil urai | Berkas CSV dan XLSX yang isinya sama **MUST** menghasilkan baris terurai yang identik — kriteria penerimaan tersendiri, bukan harapan |
| Format angka dan tanggal CSV | Dikunci templat. Baris yang tidak sesuai ditolak **beserta nomor barisnya**, **MUST NOT** ditebak |
| `SourceFormat` pada response | `OpeningItemBatchResponse` membawa `sourceFormat` (`CSV`/`XLSX`) supaya layar dapat menampilkan asal berkas tanpa menerka dari nama berkas |

## F.3 Corporate / Finance Management / Transaction Proof

Base URL: `api/v1/corporate/finance-management/transaction-proofs`
`[Tags("Corporate / Finance Management / Transaction Proof")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/` | Mengunggah bukti pembayaran; mengembalikan `ProofId` untuk dipakai pada pembayaran | `FinanceTransactionProof : Create` | `multipart/form-data` | `ApiResponse<TransactionProofResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Mengunduh berkas bukti | `FinanceTransactionProof : Read` | — | Berkas | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}/metadata` | Keterangan berkas tanpa mengunduh isinya | `FinanceTransactionProof : Read` | — | `ApiResponse<TransactionProofResponse>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
|---|---|
| `400` | Berkas kosong, nama lebih dari 255 karakter, tanpa ekstensi, ekstensi tidak diterima, tipe media tidak cocok dengan ekstensinya, atau jalur simpan keluar akar penyimpanan |
| `403` | Hak akses tidak mencukupi |
| `404` | Bukti tidak ditemukan |
| `409` | Bukti sudah terpakai pada pembayaran lain |
| `413` | Ukuran berkas melewati batas konfigurasi |
| `503` | **Batas ukuran belum dikonfigurasi** — sistem belum siap menerima berkas apa pun. Pesannya **MUST** menyebut konfigurasi yang belum lengkap, bukan menyalahkan berkas pengguna (`FIN-OQ-082`) |

### Aturan berkas bukti (revisi 15, `FIN-DEC-139`, `FIN-DES-092`)

| Hal | Ketentuan |
|---|---|
| Jenis yang diterima | `.pdf`, `.jpg`, `.jpeg`, `.png` beserta tipe medianya `application/pdf`, `image/jpeg`, `image/png`. Daftarnya dari `FinanceManagement:TransactionProof:AllowedExtensions` |
| Tipe media | **Diperiksa**, bukan hanya ekstensinya. Ekstensi lolos tetapi tipe media tidak cocok → `400` |
| Batas ukuran | Dari `FinanceManagement:TransactionProof:MaxFileSizeBytes`. **Tanpa nilai itu, unggah ditolak `503`** — bukan dianggap tak terbatas |
| Lama simpan | Sistem **tidak pernah** menghapus bukti otomatis. Retensinya mengikuti kebijakan dokumen keuangan rumah sakit, di luar modul ini |
| Penggantian | **Tidak dapat diganti** sesudah baris mutasi tertulis. Karena itu grup ini **nol** endpoint `PUT` dan `DELETE` — bukan endpoint yang ada lalu menolak. Koreksi = **balik pembayarannya, lalu catat ulang** beserta bukti baru |
| Bukti menggantung | Bukti yang terunggah tetapi pembayarannya gagal **dibiarkan** dan tetap sah dipakai percobaan berikutnya. Yang dilarang hanya memakai bukti yang **sudah** terpakai satu mutasi (`409`) |
| Penghapusan | Hanya penandaan `IsDelete`. Berkas fisiknya **tidak** ikut dihapus |
| Akses | Siapa pun pemegang `FinanceTransactionProof : Read`, **tanpa** pembatasan per pemilik transaksi pada rilis pertama — batas yang diterima sadar, dicatat `permission-audit-matrix.md` `H.3` |
| Privasi | Isi berkas dapat memuat data pihak ketiga. Jalur unduh dijaga hak akses, dan isi berkas **MUST NOT** dicatat logger |

## F.4 Corporate / Finance Management / Master Data / Direct Payment Threshold

Base URL: `api/v1/corporate/finance-management/master-data/direct-payment-threshold`
`[Tags("Corporate / Finance Management / Master Data / Direct Payment Threshold")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Ambang aktif beserta alasan perubahan terakhir | `MstDirectPaymentThreshold : Read` | — | `ApiResponse<DirectPaymentThresholdResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengubah ambang; alasan **wajib** | `MstDirectPaymentThreshold : Update` | `UpdateDirectPaymentThresholdRequest` | `ApiResponse<DirectPaymentThresholdResponse>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
|---|---|
| `404` | Ambang belum pernah ditetapkan — seluruh pembayaran langsung sedang ditolak |
| `422` | Alasan perubahan belum diisi, atau nilainya bukan angka positif |

## F.5 Permukaan baca buku mutasi

Ditambahkan pada grup yang sudah ada, bukan grup baru.

| Grup | Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|---|
| `Receivable` | `GET` | `/receivables/{id:guid}/movements` | Buku mutasi satu piutang, berurut tanggal | `FinanceReceivable : Read` | **Tersedia** (`BE-FIN-063`) |
| `Supplier Payable` | `GET` | `/supplier-payables/{id:guid}/movements` | Buku mutasi satu utang | `FinanceSupplierPayable : Read` | **Tersedia** (`BE-FIN-063`) |
| `Daily Cash` | `GET` | `/daily-cash/cash-movements` | Buku mutasi kas, bersaring tanggal, arah, dan jenis | `FinanceCashManagement : Read` | **Tersedia** (`BE-FIN-063`) |


## F.6 Permukaan baca posisi dan selisih

Ditambahkan pada grup `Accounting Events` yang sudah ada.
Base URL: `api/v1/corporate/finance-management/accounting-events`

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/subledger-balances/position` | Posisi terhitung per kelompok dan segmen pada satu tanggal, dari saldo awal ditambah buku mutasi | `FinanceAccountingEvent : Read` | **Rencana (belum tersedia)** |
| `GET` | `/subledger-balances/{accountingPeriodCode}/variance` | Selisih rekap kas harian terhadap posisi kas terhitung, beserta mutasi yang menjelaskannya (`FIN-DEC-125`) | `FinanceAccountingEvent : Read` | **Rencana (belum tersedia)** |
| `POST` | `/subledger-balances/restate` | Memeriksa dan menerbitkan ulang saldo yang berubah untuk satu periode (`FIN-DEC-114`) | `FinanceAccountingEvent : Create` | **Rencana (belum tersedia)** |

## F.7 Endpoint yang berubah perilakunya tanpa berubah bentuk

| Endpoint | Yang berubah | Bentuk request/response |
|---|---|---|
| `POST /accounting-events/subledger-balances/generate` | Jumlah baris tidak lagi tetap empat, melainkan sebanyak pemetaan aktif; nilai negatif **tidak lagi** dipotong ke nol; gagal tertutup bila pemetaan tidak lengkap | **Tidak berubah** — `GenerateSubledgerSnapshotsRequest` tetap, tetapi keempat ruas override kode akun menjadi **tidak dipakai** dan ditandai usang |
| `GET /accounting-events/subledger-balances/{accountingPeriodCode}` | `IsComplete` tidak lagi dihitung dari `items.Count >= 4` | Bentuk response tetap; arti `IsComplete` berubah |
| `POST /billing-intake/cashier-shift-closure-markers/sync` | Mencakup status belum final dan menerbitkan `PEMBUKAAN-SHIFT-KASIR`; menulis mutasi kas untuk shift final | Response menambah dua penghitung baru |

## F.8 Perubahan memutus pada endpoint yang sudah berjalan

Dua endpoint di bawah **mengubah kontrak requestnya**. Keduanya dicatat terbuka karena konsumen
frontend-nya perlu disesuaikan bersamaan — lihat `03-frontend-architecture.md` bagian 20.

**Koreksi path (dicatat di sini agar tidak diulang).** Revisi sebelumnya menulis path gaya
REST ber-parameter (`/receivables/{id:guid}/payment`, `/supplier-payables/{id:guid}/direct-payment`)
yang **tidak** cocok dengan controller yang benar-benar berjalan. `FIN-DES-085` hanya menambah
ruas pada **body** endpoint yang sudah ada — ia tidak pernah menuntut path baru. Path di bawah
sudah dikoreksi memakai route literal yang dibaca langsung dari
`FinanceArController`/`FinanceApController` dan sudah dipakai frontend saat ini.

| Endpoint | Sebelum | Sesudah | Alasan |
|---|---|---|---|
| `POST api/finance/receivable/payment` (body `ReceivableId`, bukan path parameter) | `PaymentMethod` punya bawaan `"TRANSFER"` dan **diabaikan**; `ReferenceNumber` dan `Notes` diterima lalu dibuang | `PaymentMethod` **wajib**; ditambah `FundingSourceType`, `FundingSourceId`, `ReferenceNumber`, dan `ProofId` — seluruhnya **wajib** kecuali `FundingSourceId` saat metodenya tunai | `FIN-DEC-126`, `FIN-DEC-135` |
| `POST api/finance/payable/payment` (body `SupplierPayableId`, bukan path parameter) | `BankAccountId`, `PaymentMethod`, `Notes` diterima lalu **dibuang** | Ruas yang sama menjadi **wajib dan disimpan**; ditambah `FundingSourceType`, `ProofId` wajib | `FIN-DEC-130`, `FIN-DEC-135` |

Kode status baru pada keduanya:

| Kode | Artinya bagi pengguna |
|---|---|
| `404` | Ambang pembayaran langsung belum ditetapkan, sehingga jalur ini sedang ditutup |
| `422` | Nilai pembayaran melewati ambang — gunakan jalur pembayaran berjenjang |

Keduanya juga mulai menulis satu baris buku mutasi, dan — bila metodenya tunai — satu baris mutasi
kas. Itu **tidak** mengubah bentuk response, tetapi mengubah akibatnya, sehingga dicatat di sini.

## F.9 Yang sengaja tidak ditambahkan

| Yang ditolak | Alasan |
|---|---|
| `DELETE` pada tabel mutasi mana pun | Baris mutasi tidak pernah dihapus; koreksi menambah baris |
| `PUT` pada baris mutasi | Alasan yang sama |
| Endpoint saldo rekening bank | Ditolak `FIN-DEC-137` — milik Accounting |
| Endpoint menjalankan worker pengiriman secara manual | Pengiriman dijalankan penjadwal; pemicu manual akan menjadi jalan memutar gerbang `FIN-DES-078` |
| Endpoint membuka kembali rekap kas harian | Ditolak `FIN-DEC-125` lewat pilihan menghitung posisi langsung |
