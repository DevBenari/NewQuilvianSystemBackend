# Matriks Hak Akses dan Audit — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.0` |
| Status | `draft` |
| Owner | Security Owner bersama Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | — / — |
| Input revision | `00-interview-decisions.md` revisi 1, `contracts/api-contract.md` `FIN-API-1.0` |
| Dampak kompatibilitas | Nol — seluruh Resource baru |

String `[AccessPermission(...)]` ditulis apa adanya agar implementer menyalin, bukan
menerjemahkan. Kolom "Dicatat logger" mengikuti konvensi project: `GET` tidak dicatat.

---

## 1. Daftar Resource baru

| Resource | Cakupan |
|---|---|
| `FinanceBillingIntake` | Pemantauan dan pengolahan fakta dari Billing |
| `FinanceReceivable` | Piutang, dokumen klaim, koreksi, penghapusan |
| `FinanceReceipt` | Penerimaan dan alokasinya |
| `FinanceSupplierPayable` | Utang supplier |
| `FinanceDoctorPayable` | Utang jasa medis dokter |
| `FinancePayment` | Pembayaran keluar |
| `FinanceBankDeposit` | Setoran bank |
| `FinanceDailyCash` | Posisi kas harian |
| `FinanceBank`, `FinanceBankAccount`, `FinanceCurrency` | Data induk Finance |
| `FinanceAccountingEvent` | Pemantauan kejadian ke Accounting |

Dua Resource yang **sudah ada** dan tidak diubah: `PettyCashBudget` dan `PettyCashCategory`.

---

## 2. Billing Intake

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /billing-intake` | `FinanceBillingIntake` | `Read` | `[AccessPermission("FinanceBillingIntake", "Read")]` | Tidak |
| `GET /billing-intake/{id}` | `FinanceBillingIntake` | `Read` | `[AccessPermission("FinanceBillingIntake", "Read")]` | Tidak |
| `GET /billing-intake/summary` | `FinanceBillingIntake` | `Read` | `[AccessPermission("FinanceBillingIntake", "Read")]` | Tidak |
| `POST /billing-intake/consume` | `FinanceBillingIntake` | `Consume` | `[AccessPermission("FinanceBillingIntake", "Consume")]` | Ya |
| `POST /billing-intake/{id}/retry` | `FinanceBillingIntake` | `Consume` | `[AccessPermission("FinanceBillingIntake", "Consume")]` | Ya |

## 3. Piutang

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /receivables` | `FinanceReceivable` | `Read` | `[AccessPermission("FinanceReceivable", "Read")]` | Tidak |
| `GET /receivables/{id}` | `FinanceReceivable` | `Read` | `[AccessPermission("FinanceReceivable", "Read")]` | Tidak |
| `GET /receivables/summary` | `FinanceReceivable` | `Read` | `[AccessPermission("FinanceReceivable", "Read")]` | Tidak |
| `GET /receivables/aging` | `FinanceReceivable` | `Read` | `[AccessPermission("FinanceReceivable", "Read")]` | Tidak |
| `GET /receivables/filters/metadata` | `FinanceReceivable` | `Read` | `[AccessPermission("FinanceReceivable", "Read")]` | Tidak |
| `PATCH /receivables/{id}/claim-status` | `FinanceReceivable` | `Update` | `[AccessPermission("FinanceReceivable", "Update")]` | Ya |
| `POST /receivables/{id}/documents` | `FinanceReceivable` | `Update` | `[AccessPermission("FinanceReceivable", "Update")]` | Ya |
| `PATCH /receivables/{id}/documents/{documentId}` | `FinanceReceivable` | `Update` | `[AccessPermission("FinanceReceivable", "Update")]` | Ya |
| `POST /receivables/{id}/adjustments` | `FinanceReceivable` | `RequestAdjustment` | `[AccessPermission("FinanceReceivable", "RequestAdjustment")]` | Ya |
| `POST /receivables/adjustments/{id}/approve` | `FinanceReceivable` | `ApproveAdjustment` | `[AccessPermission("FinanceReceivable", "ApproveAdjustment")]` | Ya |
| `POST /receivables/adjustments/{id}/reject` | `FinanceReceivable` | `ApproveAdjustment` | `[AccessPermission("FinanceReceivable", "ApproveAdjustment")]` | Ya |
| `POST /receivables/{id}/write-offs` | `FinanceReceivable` | `RequestWriteOff` | `[AccessPermission("FinanceReceivable", "RequestWriteOff")]` | Ya |
| `POST /receivables/write-offs/{id}/approve` | `FinanceReceivable` | `ApproveWriteOff` | `[AccessPermission("FinanceReceivable", "ApproveWriteOff")]` | Ya |
| `POST /receivables/write-offs/{id}/reject` | `FinanceReceivable` | `ApproveWriteOff` | `[AccessPermission("FinanceReceivable", "ApproveWriteOff")]` | Ya |

**Pemisahan `Request*` dan `Approve*` bukan hiasan.** Inilah wujud teknis `FIN-DEC-012`: hak
mengajukan dan hak menyetujui adalah dua butir berbeda, sehingga peran yang hanya boleh
mengajukan tidak dapat sekaligus menyetujui. Pemisahan ini bekerja bersama pemeriksaan
"pengaju ≠ penyetuju" di service dan check constraint di database — tiga lapis untuk satu
aturan.

## 4. Penerimaan

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /receipts` | `FinanceReceipt` | `Read` | `[AccessPermission("FinanceReceipt", "Read")]` | Tidak |
| `GET /receipts/{id}` | `FinanceReceipt` | `Read` | `[AccessPermission("FinanceReceipt", "Read")]` | Tidak |
| `GET /receipts/register` | `FinanceReceipt` | `Read` | `[AccessPermission("FinanceReceipt", "Read")]` | Tidak |
| `GET /receipts/shift-reconciliation` | `FinanceReceipt` | `Read` | `[AccessPermission("FinanceReceipt", "Read")]` | Tidak |
| `POST /receipts` | `FinanceReceipt` | `Create` | `[AccessPermission("FinanceReceipt", "Create")]` | Ya |
| `POST /receipts/{id}/allocations` | `FinanceReceipt` | `Allocate` | `[AccessPermission("FinanceReceipt", "Allocate")]` | Ya |
| `POST /receipts/{id}/allocations/{allocationId}/reverse` | `FinanceReceipt` | `Allocate` | `[AccessPermission("FinanceReceipt", "Allocate")]` | Ya |
| `POST /receipts/{id}/reverse` | `FinanceReceipt` | `Reverse` | `[AccessPermission("FinanceReceipt", "Reverse")]` | Ya |

## 5. Utang

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /supplier-payables` | `FinanceSupplierPayable` | `Read` | `[AccessPermission("FinanceSupplierPayable", "Read")]` | Tidak |
| `GET /supplier-payables/{id}` | `FinanceSupplierPayable` | `Read` | `[AccessPermission("FinanceSupplierPayable", "Read")]` | Tidak |
| `GET /supplier-payables/aging` | `FinanceSupplierPayable` | `Read` | `[AccessPermission("FinanceSupplierPayable", "Read")]` | Tidak |
| `POST /supplier-payables` | `FinanceSupplierPayable` | `Create` | `[AccessPermission("FinanceSupplierPayable", "Create")]` | Ya |
| `PUT /supplier-payables/{id}` | `FinanceSupplierPayable` | `Update` | `[AccessPermission("FinanceSupplierPayable", "Update")]` | Ya |
| `POST /supplier-payables/{id}/adjustments` | `FinanceSupplierPayable` | `RequestAdjustment` | `[AccessPermission("FinanceSupplierPayable", "RequestAdjustment")]` | Ya |
| `POST /supplier-payables/adjustments/{id}/approve` | `FinanceSupplierPayable` | `ApproveAdjustment` | `[AccessPermission("FinanceSupplierPayable", "ApproveAdjustment")]` | Ya |
| `GET /doctor-payables` | `FinanceDoctorPayable` | `Read` | `[AccessPermission("FinanceDoctorPayable", "Read")]` | Tidak |
| `GET /doctor-payables/{id}` | `FinanceDoctorPayable` | `Read` | `[AccessPermission("FinanceDoctorPayable", "Read")]` | Tidak |
| `GET /doctor-payables/unpaid-summary` | `FinanceDoctorPayable` | `Read` | `[AccessPermission("FinanceDoctorPayable", "Read")]` | Tidak |
| `POST /doctor-payables/recognize` | `FinanceDoctorPayable` | `Create` | `[AccessPermission("FinanceDoctorPayable", "Create")]` | Ya |
| `POST /doctor-payables/{id}/adjustments` | `FinanceDoctorPayable` | `RequestAdjustment` | `[AccessPermission("FinanceDoctorPayable", "RequestAdjustment")]` | Ya |
| `POST /doctor-payables/adjustments/{id}/approve` | `FinanceDoctorPayable` | `ApproveAdjustment` | `[AccessPermission("FinanceDoctorPayable", "ApproveAdjustment")]` | Ya |

## 6. Pembayaran keluar

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /payments` | `FinancePayment` | `Read` | `[AccessPermission("FinancePayment", "Read")]` | Tidak |
| `GET /payments/{id}` | `FinancePayment` | `Read` | `[AccessPermission("FinancePayment", "Read")]` | Tidak |
| `POST /payments` | `FinancePayment` | `Create` | `[AccessPermission("FinancePayment", "Create")]` | Ya |
| `PUT /payments/{id}` | `FinancePayment` | `Update` | `[AccessPermission("FinancePayment", "Update")]` | Ya |
| `POST /payments/{id}/submit` | `FinancePayment` | `Submit` | `[AccessPermission("FinancePayment", "Submit")]` | Ya |
| `POST /payments/{id}/approve` | `FinancePayment` | `Approve` | `[AccessPermission("FinancePayment", "Approve")]` | Ya |
| `POST /payments/{id}/reject` | `FinancePayment` | `Approve` | `[AccessPermission("FinancePayment", "Approve")]` | Ya |
| `POST /payments/{id}/mark-paid` | `FinancePayment` | `MarkPaid` | `[AccessPermission("FinancePayment", "MarkPaid")]` | Ya |
| `POST /payments/{id}/cancel` | `FinancePayment` | `Cancel` | `[AccessPermission("FinancePayment", "Cancel")]` | Ya |

`MarkPaid` sengaja dipisah dari `Approve`. Menyetujui pembayaran dan menyatakan uangnya sudah
benar-benar keluar adalah dua tanggung jawab berbeda: yang pertama milik penyetuju keuangan,
yang kedua milik Treasury yang memegang bukti transfer.

## 7. Kas

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /bank-deposits` | `FinanceBankDeposit` | `Read` | `[AccessPermission("FinanceBankDeposit", "Read")]` | Tidak |
| `GET /bank-deposits/{id}` | `FinanceBankDeposit` | `Read` | `[AccessPermission("FinanceBankDeposit", "Read")]` | Tidak |
| `GET /bank-deposits/available-balance` | `FinanceBankDeposit` | `Read` | `[AccessPermission("FinanceBankDeposit", "Read")]` | Tidak |
| `POST /bank-deposits` | `FinanceBankDeposit` | `Create` | `[AccessPermission("FinanceBankDeposit", "Create")]` | Ya |
| `POST /bank-deposits/{id}/post` | `FinanceBankDeposit` | `Post` | `[AccessPermission("FinanceBankDeposit", "Post")]` | Ya |
| `POST /bank-deposits/{id}/verify` | `FinanceBankDeposit` | `Verify` | `[AccessPermission("FinanceBankDeposit", "Verify")]` | Ya |
| `POST /bank-deposits/{id}/cancel` | `FinanceBankDeposit` | `Cancel` | `[AccessPermission("FinanceBankDeposit", "Cancel")]` | Ya |
| `GET /daily-cash/current` | `FinanceDailyCash` | `Read` | `[AccessPermission("FinanceDailyCash", "Read")]` | Tidak |
| `GET /daily-cash` | `FinanceDailyCash` | `Read` | `[AccessPermission("FinanceDailyCash", "Read")]` | Tidak |
| `GET /daily-cash/{cashDate}/breakdown` | `FinanceDailyCash` | `Read` | `[AccessPermission("FinanceDailyCash", "Read")]` | Tidak |
| `POST /daily-cash/{cashDate}/close` | `FinanceDailyCash` | `Close` | `[AccessPermission("FinanceDailyCash", "Close")]` | Ya |

## 8. Data induk

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /master-data/banks` dan `/options` dan `/{id}` | `FinanceBank` | `Read` | `[AccessPermission("FinanceBank", "Read")]` | Tidak |
| `POST /master-data/banks` | `FinanceBank` | `Create` | `[AccessPermission("FinanceBank", "Create")]` | Ya |
| `PUT /master-data/banks/{id}` | `FinanceBank` | `Update` | `[AccessPermission("FinanceBank", "Update")]` | Ya |
| `PATCH /master-data/banks/{id}/status` | `FinanceBank` | `Update` | `[AccessPermission("FinanceBank", "Update")]` | Ya |
| `DELETE /master-data/banks/{id}` | `FinanceBank` | `Delete` | `[AccessPermission("FinanceBank", "Delete")]` | Ya |
| `GET /master-data/bank-accounts` dan `/options` dan `/{id}` | `FinanceBankAccount` | `Read` | `[AccessPermission("FinanceBankAccount", "Read")]` | Tidak |
| `POST /master-data/bank-accounts` | `FinanceBankAccount` | `Create` | `[AccessPermission("FinanceBankAccount", "Create")]` | Ya |
| `PUT /master-data/bank-accounts/{id}` | `FinanceBankAccount` | `Update` | `[AccessPermission("FinanceBankAccount", "Update")]` | Ya |
| `PATCH /master-data/bank-accounts/{id}/status` | `FinanceBankAccount` | `Update` | `[AccessPermission("FinanceBankAccount", "Update")]` | Ya |
| `GET /master-data/currencies` dan turunannya | `FinanceCurrency` | `Read` | `[AccessPermission("FinanceCurrency", "Read")]` | Tidak |
| `POST /master-data/currencies` dan `/{id}/exchange-rates` | `FinanceCurrency` | `Create` | `[AccessPermission("FinanceCurrency", "Create")]` | Ya |
| `PUT /master-data/currencies/{id}` | `FinanceCurrency` | `Update` | `[AccessPermission("FinanceCurrency", "Update")]` | Ya |
| `PATCH /master-data/currencies/{id}/status` | `FinanceCurrency` | `Update` | `[AccessPermission("FinanceCurrency", "Update")]` | Ya |

## 9. Kejadian ke Accounting

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /accounting-events` dan `/{id}` dan `/summary` | `FinanceAccountingEvent` | `Read` | `[AccessPermission("FinanceAccountingEvent", "Read")]` | Tidak |
| `POST /accounting-events/{id}/retry` | `FinanceAccountingEvent` | `Retry` | `[AccessPermission("FinanceAccountingEvent", "Retry")]` | Ya |
| `GET /accounting-events/subledger-balances` | `FinanceAccountingEvent` | `Read` | `[AccessPermission("FinanceAccountingEvent", "Read")]` | Tidak |
| `POST /accounting-events/subledger-balances` | `FinanceAccountingEvent` | `Submit` | `[AccessPermission("FinanceAccountingEvent", "Submit")]` | Ya |

---

## 10. Usulan pemetaan peran

Ini **usulan**, bukan keputusan. Penetapan peran adalah wewenang Security Owner bersama owner
Finance, dan dilakukan lewat konfigurasi permission — bukan nama jabatan yang ditanam di kode
(`FIN-DEC-012`).

| Peran | Butir hak akses yang diusulkan |
|---|---|
| Finance AR Staff | `FinanceReceivable : Read/Update/RequestAdjustment/RequestWriteOff`, `FinanceReceipt : Read/Create/Allocate`, `FinanceBillingIntake : Read` |
| Finance AP Staff | `FinanceSupplierPayable : Read/Create/Update/RequestAdjustment`, `FinanceDoctorPayable : Read/Create/RequestAdjustment`, `FinancePayment : Read/Create/Update/Submit` |
| Finance Supervisor | Seluruh `Approve*`, `FinancePayment : Approve/Cancel`, `FinanceReceipt : Reverse` |
| Cashier/Treasury | `FinanceBankDeposit : Read/Create/Post/Verify`, `FinanceDailyCash : Read/Close`, `FinancePayment : MarkPaid` |
| Finance Admin | Seluruh `FinanceBank`, `FinanceBankAccount`, `FinanceCurrency` |
| Auditor/Manager | Seluruh `Read` saja, tanpa satu pun butir pengubah |
| Accounting Staff | `FinanceAccountingEvent : Read` — **tanpa** hak apa pun atas subledger Finance |

**Aturan yang MUST dipatuhi saat menyusun peran:** satu pengguna **MUST NOT** memegang
`RequestAdjustment` dan `ApproveAdjustment` sekaligus untuk Resource yang sama. Kalau itu
terjadi, aturan "pengaju ≠ penyetuju" masih tertahan lapis service dan database, tetapi
pemisahan tugasnya secara organisasi sudah hilang.

---

## 11. Aturan pencatatan audit

| Hal | Ketentuan |
|---|---|
| Yang dicatat | Seluruh `POST`, `PUT`, `PATCH`, `DELETE`. `GET` tidak dicatat |
| Isi catatan | `EntityId`, controller, action, status hasil, dan pengguna pelaku |
| Yang **MUST NOT** masuk catatan | `PatientId`, `EncounterId`, `BenefitOwnerId`, `BenefitRelationship`, `DoctorId`, `AccountNumber` — seluruhnya bertanda **Sensitif** pada kamus data |
| Jejak persetujuan | `RequestedBy`, `RequestedAt`, `ApprovedBy`, `ApprovedAt` tersimpan di barisnya sendiri, bukan hanya di log |
| Jejak nilai | Perubahan saldo tersimpan sebagai baris baru, bukan sebagai catatan log. Log bukan sumber kebenaran angka |

Ketentuan terakhir yang paling penting: **log bukan sumber kebenaran angka**. Setiap perubahan
nilai uang ditelusuri lewat baris alokasi, koreksi, atau pembalikan — bukan dengan membaca log.
Log dipakai untuk menjawab "siapa yang menekan tombol", bukan "berapa saldonya".

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-0.2` — status `locked` 20 September 2026 |
| Tanggal | 20 September 2026 |
| Keputusan | `FIN-DES-025`, `FIN-DES-026` |

## A.1 Resource yang diganti namanya

Resource `FinanceDoctorPayable` pada revisi 1 **digantikan** `FinanceMedicalServicePayable`.
Belum ada satu pun butir hak akses yang terpasang di sistem, jadi tidak ada yang perlu
dimigrasikan.

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /medical-service-payables` | `FinanceMedicalServicePayable` | `Read` | `[AccessPermission("FinanceMedicalServicePayable", "Read")]` | Tidak |
| `GET /medical-service-payables/{id}` | `FinanceMedicalServicePayable` | `Read` | `[AccessPermission("FinanceMedicalServicePayable", "Read")]` | Tidak |
| `GET /medical-service-payables/unpaid-summary` | `FinanceMedicalServicePayable` | `Read` | `[AccessPermission("FinanceMedicalServicePayable", "Read")]` | Tidak |
| `POST /medical-service-payables/recognize` | `FinanceMedicalServicePayable` | `Create` | `[AccessPermission("FinanceMedicalServicePayable", "Create")]` | Ya |
| `POST /medical-service-payables/{id}/adjustments` | `FinanceMedicalServicePayable` | `RequestAdjustment` | `[AccessPermission("FinanceMedicalServicePayable", "RequestAdjustment")]` | Ya |
| `POST /medical-service-payables/adjustments/{id}/approve` | `FinanceMedicalServicePayable` | `ApproveAdjustment` | `[AccessPermission("FinanceMedicalServicePayable", "ApproveAdjustment")]` | Ya |

## A.2 Potongan pembayaran

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /payments/{id}/deductions` | `FinancePayment` | `Read` | `[AccessPermission("FinancePayment", "Read")]` | Tidak |
| `POST /payments/{id}/deductions` | `FinancePayment` | `Update` | `[AccessPermission("FinancePayment", "Update")]` | Ya |
| `DELETE /payments/{id}/deductions/{deductionId}` | `FinancePayment` | `Update` | `[AccessPermission("FinancePayment", "Update")]` | Ya |

Potongan sengaja memakai Resource `FinancePayment`, bukan Resource tersendiri. Alasannya:
menyusun potongan adalah bagian dari menyusun pembayaran, dan memisahkannya akan membuat satu
orang bisa mengubah nilai transfer tanpa punya hak atas pembayarannya.

## A.3 Pembaruan usulan pemetaan peran

| Peran | Perubahan |
|---|---|
| Finance AP Staff | `FinanceDoctorPayable : *` → `FinanceMedicalServicePayable : Read/Create/RequestAdjustment`. Tetap memegang `FinancePayment : Update` sehingga dapat menyusun potongan |
| Finance Supervisor | `FinanceMedicalServicePayable : ApproveAdjustment` menggantikan padanan lamanya |
| Auditor/Manager | `FinanceMedicalServicePayable : Read` saja |

## A.4 Catatan privasi yang bertambah

| Data | Ketentuan |
|---|---|
| `PayeeReferenceId` dan `PayeeType` | **Sensitif.** Bersama-sama keduanya mengungkap identitas tenaga medis beserta penghasilannya. MUST NOT masuk custom logger dan MUST NOT menyeberang ke Accounting |
| `FinPaymentDeduction.Reason` | **Sensitif.** Dapat memuat keterangan pribadi seperti alasan kasbon. MUST NOT masuk custom logger |
| `FinPaymentDeduction.Amount` per pos | Nilai potongan pajak dan kasbon adalah informasi pribadi penerima. Hanya peran yang berhak atas pembayaran yang boleh melihatnya; Auditor melihat totalnya, bukan rinciannya |

Butir terakhir adalah ketentuan baru yang tidak ada pada revisi 1, karena revisi 1 memang belum
menyimpan data sepribadi ini.


---

# AMENDMENT REVISI 4

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.1` — status `locked` 25 September 2026 (disetujui Yasmin bersama `FIN-DES-037`..`044`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-045`..`055` |

## B.1 Resource baru

| Resource | Cakupan |
|---|---|
| `FinancePurchaseOrder` | Purchase Order dan baris itemnya |
| `FinanceGoodsReceipt` | Tanda Terima Barang |
| `FinanceInvoiceExchange` | Tukar Faktur |
| `FinancePurchasingInvoice` | Purchasing Invoice |
| `FinanceSupplierReturn` | Retur Pembelian dan Deposit Retur |
| `FinancePurchasingReport` | Aging AP, Rekap, Laporan Tukar Faktur, Laporan Jatuh Tempo, Rekonsiliasi |
| `FinanceReceivableInvoiceBatch` | AR Invoice Agregat |

Tidak ada Resource baru untuk potongan penerimaan AR — mengikuti pola `FIN-DES-026`/A.2,
endpoint potongannya memakai Resource `FinanceReceipt` yang sudah ada, karena menyusun potongan
adalah bagian dari menyusun alokasi penerimaan, bukan permohonan terpisah.

## B.2 Purchasing / Purchase Order

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /purchasing/purchase-orders` | `FinancePurchaseOrder` | `Read` | `[AccessPermission("FinancePurchaseOrder", "Read")]` | Tidak |
| `GET /purchasing/purchase-orders/{id}` | `FinancePurchaseOrder` | `Read` | `[AccessPermission("FinancePurchaseOrder", "Read")]` | Tidak |
| `POST /purchasing/purchase-orders` | `FinancePurchaseOrder` | `Create` | `[AccessPermission("FinancePurchaseOrder", "Create")]` | Ya |
| `PUT /purchasing/purchase-orders/{id}` | `FinancePurchaseOrder` | `Update` | `[AccessPermission("FinancePurchaseOrder", "Update")]` | Ya |
| `POST /purchasing/purchase-orders/{id}/submit` | `FinancePurchaseOrder` | `Submit` | `[AccessPermission("FinancePurchaseOrder", "Submit")]` | Ya |
| `POST /purchasing/purchase-orders/{id}/approve` | `FinancePurchaseOrder` | `Approve` | `[AccessPermission("FinancePurchaseOrder", "Approve")]` | Ya |
| `POST /purchasing/purchase-orders/{id}/reject` | `FinancePurchaseOrder` | `Approve` | `[AccessPermission("FinancePurchaseOrder", "Approve")]` | Ya |
| `POST /purchasing/purchase-orders/{id}/cancel` | `FinancePurchaseOrder` | `Cancel` | `[AccessPermission("FinancePurchaseOrder", "Cancel")]` | Ya |

## B.3 Purchasing / Goods Receipt dan Invoice Exchange

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /purchasing/goods-receipts` | `FinanceGoodsReceipt` | `Read` | `[AccessPermission("FinanceGoodsReceipt", "Read")]` | Tidak |
| `GET /purchasing/goods-receipts/{id}` | `FinanceGoodsReceipt` | `Read` | `[AccessPermission("FinanceGoodsReceipt", "Read")]` | Tidak |
| `POST /purchasing/goods-receipts` | `FinanceGoodsReceipt` | `Create` | `[AccessPermission("FinanceGoodsReceipt", "Create")]` | Ya |
| `POST /purchasing/goods-receipts/{id}/cancel` | `FinanceGoodsReceipt` | `Cancel` | `[AccessPermission("FinanceGoodsReceipt", "Cancel")]` | Ya |
| `GET /purchasing/invoice-exchanges` | `FinanceInvoiceExchange` | `Read` | `[AccessPermission("FinanceInvoiceExchange", "Read")]` | Tidak |
| `GET /purchasing/invoice-exchanges/{id}` | `FinanceInvoiceExchange` | `Read` | `[AccessPermission("FinanceInvoiceExchange", "Read")]` | Tidak |
| `POST /purchasing/invoice-exchanges` | `FinanceInvoiceExchange` | `Create` | `[AccessPermission("FinanceInvoiceExchange", "Create")]` | Ya |
| `POST /purchasing/invoice-exchanges/{id}/cancel` | `FinanceInvoiceExchange` | `Cancel` | `[AccessPermission("FinanceInvoiceExchange", "Cancel")]` | Ya |

## B.4 Purchasing / Purchasing Invoice dan Retur

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /purchasing/purchasing-invoices` | `FinancePurchasingInvoice` | `Read` | `[AccessPermission("FinancePurchasingInvoice", "Read")]` | Tidak |
| `GET /purchasing/purchasing-invoices/{id}` | `FinancePurchasingInvoice` | `Read` | `[AccessPermission("FinancePurchasingInvoice", "Read")]` | Tidak |
| `POST /purchasing/purchasing-invoices` | `FinancePurchasingInvoice` | `Create` | `[AccessPermission("FinancePurchasingInvoice", "Create")]` | Ya |
| `PUT /purchasing/purchasing-invoices/{id}` | `FinancePurchasingInvoice` | `Update` | `[AccessPermission("FinancePurchasingInvoice", "Update")]` | Ya |
| `POST /purchasing/purchasing-invoices/{id}/submit` | `FinancePurchasingInvoice` | `Submit` | `[AccessPermission("FinancePurchasingInvoice", "Submit")]` | Ya |
| `POST /purchasing/purchasing-invoices/{id}/approve` | `FinancePurchasingInvoice` | `Approve` | `[AccessPermission("FinancePurchasingInvoice", "Approve")]` | Ya |
| `POST /purchasing/purchasing-invoices/{id}/reject` | `FinancePurchasingInvoice` | `Approve` | `[AccessPermission("FinancePurchasingInvoice", "Approve")]` | Ya |
| `GET /purchasing/supplier-returns` | `FinanceSupplierReturn` | `Read` | `[AccessPermission("FinanceSupplierReturn", "Read")]` | Tidak |
| `GET /purchasing/supplier-returns/{id}` | `FinanceSupplierReturn` | `Read` | `[AccessPermission("FinanceSupplierReturn", "Read")]` | Tidak |
| `POST /purchasing/supplier-returns` | `FinanceSupplierReturn` | `Create` | `[AccessPermission("FinanceSupplierReturn", "Create")]` | Ya |
| `GET /purchasing/supplier-returns/deposits` | `FinanceSupplierReturn` | `Read` | `[AccessPermission("FinanceSupplierReturn", "Read")]` | Tidak |
| ~~`POST /purchasing/supplier-returns/deposits/{id}/apply`~~ | — | — | **DICABUT `FIN-PERM-1.2`** — Action `ApplyDeposit` tidak dibuat; lihat C.1 | — |

## B.5 Purchasing / Reports dan Receivable Invoice Batch

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /purchasing/reports/*` (**empat** endpoint — `/aging` dicabut `FIN-DEC-059`) | `FinancePurchasingReport` | `Read` | `[AccessPermission("FinancePurchasingReport", "Read")]` | Tidak |
| `GET /receivable-invoice-batches` | `FinanceReceivableInvoiceBatch` | `Read` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Read")]` | Tidak |
| `GET /receivable-invoice-batches/{id}` | `FinanceReceivableInvoiceBatch` | `Read` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Read")]` | Tidak |
| `GET /receivable-invoice-batches/eligible-receivables` | `FinanceReceivableInvoiceBatch` | `Read` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Read")]` | Tidak |
| `POST /receivable-invoice-batches` | `FinanceReceivableInvoiceBatch` | `Create` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Create")]` | Ya |
| `POST /receivable-invoice-batches/{id}/issue` | `FinanceReceivableInvoiceBatch` | `Issue` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Issue")]` | Ya |
| `GET /receivable-invoice-batches/{id}/document` | `FinanceReceivableInvoiceBatch` | `Read` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Read")]` | Tidak |
| `POST /receivable-invoice-batches/{id}/cancel` | `FinanceReceivableInvoiceBatch` | `Update` | `[AccessPermission("FinanceReceivableInvoiceBatch", "Update")]` | Ya |

## B.6 Potongan penerimaan (tambahan grup Receipt)

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /receipts/{id}/deductions` | `FinanceReceipt` | `Read` | `[AccessPermission("FinanceReceipt", "Read")]` | Tidak |
| ~~`POST /receipts/{id}/deductions`~~ | — | — | **DICABUT `FIN-PERM-1.2`** — potongan dicatat lewat `POST /receipts/{id}/allocations` yang sudah memakai `FinanceReceipt : Allocate` | — |

## B.7 Pembaruan usulan pemetaan peran

| Peran | Butir hak akses yang diusulkan |
|---|---|
| Finance AP Staff (diperluas) | Tambah `FinancePurchaseOrder : Read/Create/Update/Submit/Cancel`, `FinanceGoodsReceipt : Read/Create/Cancel`, `FinanceInvoiceExchange : Read/Create/Cancel`, `FinancePurchasingInvoice : Read/Create/Update/Submit`, `FinanceSupplierReturn : Read/Create` |
| Supervisor Finance (baru) | `FinancePurchaseOrder : Approve`, `FinancePurchasingInvoice : Approve` — **hanya** di bawah ambang Rp 50.000.000 (`FIN-DEC-052`); pemisahan berdasarkan nominal ditegakkan service dan database, bukan hanya permission |
| Manajer Finance (baru) | `FinancePurchaseOrder : Approve`, `FinancePurchasingInvoice : Approve` — seluruh nominal, termasuk di atas ambang |
| Finance AR Staff (diperluas) | Tambah `FinanceReceivableInvoiceBatch : Read/Create/Issue` |
| Auditor/Manager | Tambah `FinancePurchaseOrder`, `FinanceGoodsReceipt`, `FinanceInvoiceExchange`, `FinancePurchasingInvoice`, `FinanceSupplierReturn`, `FinancePurchasingReport`, `FinanceReceivableInvoiceBatch : Read` saja |

**Supervisor Finance dan Manajer Finance adalah peran baru**, terpisah dari "Finance
Supervisor" yang sudah ada pada bagian 10 dasar (penyetuju koreksi/penghapusan/pembayaran).
Checkpoint approval PO/Purchasing Invoice (`FIN-DES-039`) adalah permohonan yang berbeda dari
approval pembayaran — walau ambang nominalnya sama, string permission-nya sengaja berbeda
Resource (`FinancePurchaseOrder`/`FinancePurchasingInvoice` vs `FinancePayment`), sehingga
pemegang satu tidak otomatis memegang yang lain.

## B.8 Catatan privasi

| Data | Ketentuan |
|---|---|
| `MstSupplier` data bank dan NPWP | Sudah diatur `cash-and-master-data.md`/kamus data existing — tidak berubah oleh amendment ini |
| `FinPurchasingInvoiceItem.UnitPrice`, `FinPurchaseOrderItem.UnitPrice` | Tidak ditandai Sensitif (bukan data pribadi), namun **SHOULD** dibatasi ke peran Purchasing/AP dan Auditor — harga beli adalah informasi komersial, bukan privasi individu, sehingga di luar cakupan aturan Sensitif kamus data |
| `FinReceiptDeduction.Reason` | **Sensitif** bila memuat keterangan yang bisa mengidentifikasi pihak ketiga (mis. nama bank pemotong PPh 23 atas nama tertentu) — mengikuti pola `FinPaymentDeduction.Reason` (`A.4`) |

---

# AMENDMENT REVISI 5

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.2` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-057`, `058`, `059`; `FIN-DES-045`, `048` |

## C.1 Pemakaian Deposit Retur di dalam pembayaran

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
|---|---|---|---|:---:|
| `GET /payments/{id}/return-deposits` | `FinancePayment` | `Read` | `[AccessPermission("FinancePayment", "Read")]` | Tidak |
| `POST /payments/{id}/return-deposits` | `FinancePayment` | `Update` | `[AccessPermission("FinancePayment", "Update")]` | Ya |
| `DELETE /payments/{id}/return-deposits/{usageId}` | `FinancePayment` | `Update` | `[AccessPermission("FinancePayment", "Update")]` | Ya |

Memakai Resource `FinancePayment`, bukan `FinanceSupplierReturn`, dengan alasan yang sama seperti
potongan pembayaran (A.2): memilih sumber dana adalah bagian dari menyusun pembayaran. Bila
dipisah, satu orang dapat mengubah nilai transfer tanpa punya hak atas pembayarannya.
Konsekuensinya Action `FinanceSupplierReturn : ApplyDeposit` (B.1, B.4) **tidak dibuat**.

## C.2 Potongan AR

Tidak ada butir hak akses baru. Potongan dicatat lewat `POST /receipts/{id}/allocations` yang
sudah memakai `[AccessPermission("FinanceReceipt", "Allocate")]`; pembalikannya ikut
`POST /receipts/{id}/allocations/{allocationId}/reverse` dengan hak akses yang sama.

## C.3 Pembaruan usulan pemetaan peran

| Peran | Perubahan |
|---|---|
| Finance AP Staff | **Tidak** lagi diusulkan `FinanceSupplierReturn : ApplyDeposit` (B.7) — pemakaian deposit mengikuti `FinancePayment : Update` yang sudah dipegangnya |
| Seluruh peran | Nol perubahan lain |
