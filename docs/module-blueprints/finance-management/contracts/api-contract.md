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
