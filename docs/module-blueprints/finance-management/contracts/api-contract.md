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
