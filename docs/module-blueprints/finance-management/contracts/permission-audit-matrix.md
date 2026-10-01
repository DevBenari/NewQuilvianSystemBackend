# Matriks Hak Akses dan Audit — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.4` |
| `last_changed_in` | `FIN-PERM-1.4` — AMENDMENT REVISI 7, 29 September 2026 (koreksi D.2, D.6.1, D.6.2 mengikuti `FIN-DEC-082`/`FIN-DEC-083`) |
| Status | Revisi 1-5 `locked`; Revisi 6 `approved` (Yasmin, 28 September 2026 lewat `FIN-DEC-078` & `FIN-DEC-079`); **Revisi 7 `draft`** — menurunkan `FIN-DES-066`..`069` yang sendiri masih `draft` |
| Owner | Security Owner bersama Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Revisi 6: Yasmin / 28 September 2026. Revisi 7: **belum** — menunggu Security Owner (lihat `FIN-OQ-038`) |
| Input revision | `00-interview-decisions.md` revisi 29 September 2026 (`FIN-DEC-082`, `FIN-DEC-083`), `02-backend-architecture.md` AMENDMENT REVISI 11 (`FIN-DES-066`..`069`) |
| `input_hash` | `00-interview-decisions.md` = `c1cba136cb3c665bd1eeefe944b9d902d6fe5f55e123cf4bd83ae53456a7dee2` |
| Dampak kompatibilitas | Revisi 6: penyelarasan 6 controller legacy (rename string resource, **sudah diimplementasikan** `BE-FIN-042`) + penambahan resource payung. Revisi 7: **nol dampak pada kode yang sudah berjalan** — mengoreksi nama payung (`Finance.AP.Umbrella`/`Finance.AR.Umbrella`) dan titik tulis ekspansi sebelum satu baris pun ditulis |

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

---

# AMENDMENT REVISI 7 — Koreksi Nama Resource Payung dan Titik Tulis Ekspansi (`FIN-DEC-082`, `FIN-DEC-083`)

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.4` |
| Status | `draft` — menurunkan `FIN-DES-066`..`069` yang masih `draft`; menunggu Security Owner lewat `FIN-OQ-039` |
| Tanggal | 29 September 2026 |
| Keputusan | `FIN-DEC-082` (nama payung bukan `Finance.AP`/`Finance.AR` melainkan `Finance.AP.Umbrella`/`Finance.AR.Umbrella`), `FIN-DEC-083` (ekspansi materialized saat admin memberi grant, bukan saat request) |
| Masalah yang diselesaikan | Bentrok nama `Finance.AP`/`Finance.AR` antara resource payung baru dan `FinanceApController`/`FinanceArController` V2 yang sudah berjalan (ditemukan saat implementasi `BE-FIN-042`). Premis seeder `FIN-DEC-079` terbukti keliru — seeder tidak pernah menulis `SysAccessPolicy` |
| Gerbang terbuka | `FIN-OQ-039` — menahan implementasi mekanisme ekspansi; **tidak** menahan D.5 rename controller maupun pekerjaan frontend |
| Sinkronisasi | `02-backend-architecture.md` AMENDMENT REVISI 11 (`FIN-DES-066`..`069`), `00-interview-decisions.md` Amendment pass 29 September 2026 |

Amendment ini **mengoreksi** bagian D.2 (nama payung) dan D.6.2 (titik tulis ekspansi) dari REVISI 6 di bawah, dan menambahkan sub-seksi D.3a–D.3b (tabel peta aksi granular terstruktur).

---

# AMENDMENT REVISI 6 — Pemetaan Resource Payung (Umbrella) ke Granular dan Penyelarasan Nama Controller Legacy

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.3` |
| Status | `approved` (Disetujui Yasmin 28 September 2026 lewat `FIN-DEC-078` & `FIN-DEC-079`) |
| Tanggal | 28 September 2026 |
| Keputusan | `FIN-DEC-078`, `FIN-DEC-079`; diturunkan ke `FIN-DES-061`, `FIN-DES-062`, `FIN-DES-063` |
| Masalah yang diselesaikan | `FIN-CQ-08` (Penyelarasan 6 controller legacy), `FIN-CAP-043` (Audit kapabilitas controller), `FIN-OQ-036` (Granularitas filter menu frontend vs endpoint backend) |

Dokumen amendment ini menetapkan secara definitif pemetaan hak akses **Payung ke Granular** (Umbrella-to-Granular) untuk modul Finance Management serta rencana teknis penyelarasan nama Resource pada 6 controller legacy. Seluruh ketentuan di bagian ini mengikat arsitektur backend, seeder otorisasi peran, dan antarmuka frontend.

---

## D.1 Latar Belakang & Masalah Bisnis-Teknis (FIN-CQ-08 / FIN-CAP-043 / FIN-OQ-036)

Dalam operasional rumah sakit sehari-hari, tata kelola keuangan menuntut pemisahan wewenang (*segregation of duties*) yang ketat antara:
1. **Bagian Hutang & Pengadaan (Account Payable - AP):** Mengurus pesanan obat/alkes farmasi, tanda terima gudang medis, verifikasi faktur tagihan supplier/PBF, pemotongan retur barang, hingga pengajuan dan eksekusi pembayaran kas/bank keluar serta pelunasan jasa medis dokter.
2. **Bagian Piutang & Penerimaan Kas (Account Receivable - AR):** Mengurus penagihan klaim BPJS Kesehatan, asuransi komersial, jaminan perusahaan mitra, penerimaan kasir pelayanan, rekonsiliasi uang muka pasien, serta permohonan koreksi atau penghapusan piutang tak tertagih.

Audit menyeluruh `/trace-existing-capabilities` (§ 16.3) menemukan dua persoalan teknis otorisasi yang berdampak langsung pada operasional sistem:
- **Konflik Nama Resource Controller Legacy (`FIN-CQ-08` / `FIN-CAP-043`):** Enam controller awal Finance (`FinancePaymentsController`, `FinanceReceiptsController`, `FinanceReceivablesController`, `FinanceSupplierPayablesController`, `FinanceBillingIntakeController`, dan `FinanceAccountingEventsController`) menggunakan nama resource pendek pada atribut `[AccessPermission]` (misalnya `"Payment"`, `"Receipt"`). Hal ini bertentangan dengan kontrak resmi `permission-audit-matrix.md` dan 7 controller Purchasing baru yang sudah memakai awalan kanonikal (`"FinancePayment"`, `"FinanceReceipt"`, `"FinancePurchaseOrder"`). Melalui `FIN-DEC-078`, pemilik produk menetapkan bahwa **kode mengikuti kontrak**; keenam controller lama diselaraskan ke nama kanonikal lengkap beserta migrasi data peran.
- **Ketidakcocokan Filter Menu Frontend (`FIN-OQ-036`):** Menu sidebar navigasi frontend (`src/utils/menu-sidebar/corporateFinance.js`) membatasi tampilan menu menggunakan resource payung tingkat tinggi, yaitu `Finance.AP` dan `Finance.AR`. Namun, resource payung ini belum tercantum di kontrak backend, dan endpoint Purchasing menuntut resource granular seperti `FinancePurchaseOrder`. Akibatnya, ada risiko staf rumah sakit yang berhak melihat menu di frontend ditolak oleh API backend (`403 Forbidden`). Melalui `FIN-DEC-079`, pemilik produk menetapkan bahwa `Finance.AP` dan `Finance.AR` **didaftarkan sebagai Resource Resmi Tingkat Kelompok (Umbrella)**, di mana pemegang hak payung otomatis mewarisi seluruh hak akses granular di bawah kelompoknya melalui seeder peran dan runtime expansion.

---

## D.2 Definisi Resource Payung (Group-Level Umbrella Resources)

> **DIKOREKSI — `FIN-PERM-1.4`, 29 September 2026 (`FIN-DEC-082`, `FIN-DEC-083`).** Nama kedua
> resource payung dan mekanisme kerjanya di bawah ini sudah dikoreksi. Nama lama `Finance.AP`/
> `Finance.AR` **tidak dipakai sebagai payung** karena sudah menjadi milik dua controller V2 yang
> berjalan.

Resource payung adalah entitas hak akses tingkat kelompok yang mewakili ranah kerja fungsional staf rumah sakit:
1. **`Finance.AP.Umbrella`**: Payung kewenangan operasional Hutang, Pengadaan, dan Pembayaran Kas Keluar Rumah Sakit.
2. **`Finance.AR.Umbrella`**: Payung kewenangan operasional Piutang, Penagihan Penjamin, dan Penerimaan Kasir Rumah Sakit.

Keduanya memiliki tiga aksi: `View` (`AccessType` `Read`), `Operate` (`Create`), dan `Approve`
(`Update`) — pemetaan aksinya ada di D.4.

Mekanisme kerja resource payung:
- **Di Frontend:** Payung **tidak pernah** diperiksa penyaring menu. Butir menu dijaga resource
  **granular** yang sama persis dengan yang dituntut endpointnya (mis. `FinancePurchaseOrder : Read`
  untuk butir Purchase Order). Sesudah ekspansi, pemegang payung benar-benar memiliki pasangan
  granular itu pada `SysAccessPolicy`, sehingga penyaring menu granular menampilkannya tanpa
  perlakuan khusus. Butir menu V2 yang sudah ada tetap dijaga `Finance.AP`/`Finance.AR` milik
  controller V2 dan **tidak disentuh**. Lihat `FIN-DES-069`.
- **Di Backend:** Saat admin memberi izin payung kepada satu pasangan **Departemen x Posisi** lewat
  layar Akses Role, mekanisme ekspansi menuliskan baris `SysAccessPolicy` granular yang bersesuaian
  **pada saat itu juga** (*materialized*), di dalam transaksi yang sama. Pemeriksaan hak akses saat
  request tidak berubah sama sekali — ia tetap membaca `SysAccessPolicy` apa adanya. Lihat D.6.2 dan
  `FIN-DES-067`.

---

## D.3 Tabel Pemetaan Resource Payung ke Resource Granular

Berikut adalah pemetaan resmi satu per satu dari resource payung ke seluruh resource granular di lingkungan sistem Quilvian:

### 1. Kelompok Payung `Finance.AP` (Hutang & Pengadaan Rumah Sakit)

Mencakup 9 (sembilan) resource granular berikut:

| # | Resource Granular | Cakupan Fungsi Bisnis Rumah Sakit | Aksi Granular yang Didukung |
|---|---|---|---|
| 1 | `FinancePayment` | Pembayaran kas/bank keluar, pelunasan utang supplier, pencairan honor dokter, dan pencatatan bukti transfer bank | `Read`, `Create`, `Update`, `Submit`, `Approve`, `MarkPaid`, `Cancel` |
| 2 | `FinanceSupplierPayable` | Pengelolaan utang faktur supplier/distributor farmasi & alat kesehatan | `Read`, `Create`, `Update`, `RequestAdjustment`, `ApproveAdjustment` |
| 3 | `FinanceMedicalServicePayable` | Pengelolaan kewajiban utang jasa medis dokter spesialis dan tenaga kesehatan | `Read`, `Create`, `RequestAdjustment`, `ApproveAdjustment` |
| 4 | `FinancePurchaseOrder` | Penerbitan dan persetujuan Surat Pesanan Pembelian (PO) obat, BHP, dan perlengkapan medis | `Read`, `Create`, `Update`, `Submit`, `Approve`, `Cancel` |
| 5 | `FinanceGoodsReceipt` | Penerimaan fisik barang di gudang farmasi/logistik medis berdasarkan PO | `Read`, `Create`, `Cancel` |
| 6 | `FinanceInvoiceExchange` | Pencatatan dokumen Tukar Faktur tagihan dari distributor/vendor rekanan RS | `Read`, `Create`, `Cancel` |
| 7 | `FinancePurchasingInvoice` | Verifikasi tagihan faktur pembelian vendor terhadap penerimaan barang fisik | `Read`, `Create`, `Update`, `Submit`, `Approve` |
| 8 | `FinanceSupplierReturn` | Pengajuan dan konfirmasi nota retur barang rusak/kedaluwarsa ke pihak pemasok | `Read`, `Create`, `Confirm`, `Cancel` |
| 9 | `FinancePurchasingReport` | Laporan rekapitulasi pembelian logistik, analisa pengadaan, dan evaluasi vendor | `Read` |

### 2. Kelompok Payung `Finance.AR` (Piutang & Penerimaan Rumah Sakit)

Mencakup 4 (empat) resource granular berikut:

| # | Resource Granular | Cakupan Fungsi Bisnis Rumah Sakit | Aksi Granular yang Didukung |
|---|---|---|---|
| 1 | `FinanceReceivable` | Pengelolaan saldo piutang pasien umum, klaim BPJS Kesehatan, dan asuransi/perusahaan rekanan | `Read`, `Update`, `RequestAdjustment`, `ApproveAdjustment`, `RequestWriteOff`, `ApproveWriteOff` |
| 2 | `FinanceReceipt` | Pencatatan tanda terima kas/bank, alokasi pelunasan ke nomor tagihan/piutang, serta pembatalan/pembalikan alokasi | `Read`, `Create`, `Allocate`, `Reverse` |
| 3 | `FinanceReceivableInvoiceBatch` | Pembuatan dan penerbitan berkas invoice tagihan gabungan (*batch billing*) ke perusahaan penjamin/BPJS | `Read`, `Create`, `Issue`, `Update` |
| 4 | `FinanceBillingIntake` | Pemantauan dan penerimaan serah terima (*handoff*) transaksi piutang dan pembayaran dari modul Kasir/Billing | `Read`, `Consume` |

### 3. Resource Mandiri (Standalone — Di Luar Payung AP/AR)

Resource berikut **tidak** dimasukkan ke dalam payung `Finance.AP` maupun `Finance.AR` karena memiliki ranah tata kelola terpisah (manajemen kas umum, integrasi buku besar akuntansi, dan master data bersama):

| Resource | Alasan Berdiri Sendiri | Aksi yang Didukung |
|---|---|---|
| `FinanceBankDeposit` | Merupakan fungsi rekonsiliasi penyetoran uang fisik kasir ke bank, ditangani petugas kasir utama / treasury | `Read`, `Create`, `Post`, `Cancel` |
| `FinanceDailyCash` | Pemantauan posisi saldo brankas kas harian RS lintas unit penerimaan dan pengeluaran | `Read`, `Close`, `Breakdown` |
| `FinanceBank`, `FinanceBankAccount`, `FinanceCurrency` | Data induk perbankan dan rekening operasional RS, dikelola Administrator Keuangan | `Read`, `Create`, `Update`, `Delete` |
| `FinanceAccountingEvent` | Jembatan audit kejadian keuangan ke jurnal akuntansi umum, hanya diakses Auditor dan staf Akuntansi | `Read` |
| `PettyCashBudget`, `PettyCashCategory` | Anggaran kas kecil operasional unit kerja RS, tunduk pada tata kelola kasir/petty cash yang sudah baku | `Read`, `Create`, `Update`, `Allocate` |

---

## D.3a Tabel Ekspansi Lengkap — `Finance.AP.Umbrella` ke Pasangan Granular

> **Sumber kebenaran peta ini.** Berkas kode (setelah platform mendukung resource tanpa endpoint —
> lihat `FIN-OQ-039`) harus dapat di-*diff* langsung terhadap tabel ini. Setiap baris yang ada di
> sini MUST ada di kode, dan setiap baris di kode MUST ada di sini.

| Tier Payung | Resource Granular | Aksi | Kode Aksi di `[AccessPermission]` |
|---|---|---|---|
| **`View`** | `FinancePayment` | Read | `"FinancePayment", "Read"` |
| **`View`** | `FinanceSupplierPayable` | Read | `"FinanceSupplierPayable", "Read"` |
| **`View`** | `FinanceMedicalServicePayable` | Read | `"FinanceMedicalServicePayable", "Read"` |
| **`View`** | `FinancePurchaseOrder` | Read | `"FinancePurchaseOrder", "Read"` |
| **`View`** | `FinanceGoodsReceipt` | Read | `"FinanceGoodsReceipt", "Read"` |
| **`View`** | `FinanceInvoiceExchange` | Read | `"FinanceInvoiceExchange", "Read"` |
| **`View`** | `FinancePurchasingInvoice` | Read | `"FinancePurchasingInvoice", "Read"` |
| **`View`** | `FinanceSupplierReturn` | Read | `"FinanceSupplierReturn", "Read"` |
| **`View`** | `FinancePurchasingReport` | Read | `"FinancePurchasingReport", "Read"` |
| **`Operate`** | `FinancePayment` | Create | `"FinancePayment", "Create"` |
| **`Operate`** | `FinancePayment` | Update | `"FinancePayment", "Update"` |
| **`Operate`** | `FinancePayment` | Submit | `"FinancePayment", "Submit"` |
| **`Operate`** | `FinanceSupplierPayable` | Create | `"FinanceSupplierPayable", "Create"` |
| **`Operate`** | `FinanceSupplierPayable` | Update | `"FinanceSupplierPayable", "Update"` |
| **`Operate`** | `FinanceSupplierPayable` | RequestAdjustment | `"FinanceSupplierPayable", "RequestAdjustment"` |
| **`Operate`** | `FinanceMedicalServicePayable` | Create | `"FinanceMedicalServicePayable", "Create"` |
| **`Operate`** | `FinanceMedicalServicePayable` | RequestAdjustment | `"FinanceMedicalServicePayable", "RequestAdjustment"` |
| **`Operate`** | `FinancePurchaseOrder` | Create | `"FinancePurchaseOrder", "Create"` |
| **`Operate`** | `FinancePurchaseOrder` | Update | `"FinancePurchaseOrder", "Update"` |
| **`Operate`** | `FinancePurchaseOrder` | Submit | `"FinancePurchaseOrder", "Submit"` |
| **`Operate`** | `FinanceGoodsReceipt` | Create | `"FinanceGoodsReceipt", "Create"` |
| **`Operate`** | `FinanceInvoiceExchange` | Create | `"FinanceInvoiceExchange", "Create"` |
| **`Operate`** | `FinancePurchasingInvoice` | Create | `"FinancePurchasingInvoice", "Create"` |
| **`Operate`** | `FinancePurchasingInvoice` | Update | `"FinancePurchasingInvoice", "Update"` |
| **`Operate`** | `FinancePurchasingInvoice` | Submit | `"FinancePurchasingInvoice", "Submit"` |
| **`Operate`** | `FinanceSupplierReturn` | Create | `"FinanceSupplierReturn", "Create"` |
| **`Approve`** | `FinancePayment` | Approve | `"FinancePayment", "Approve"` |
| **`Approve`** | `FinancePayment` | MarkPaid | `"FinancePayment", "MarkPaid"` |
| **`Approve`** | `FinancePayment` | Cancel | `"FinancePayment", "Cancel"` |
| **`Approve`** | `FinanceSupplierPayable` | ApproveAdjustment | `"FinanceSupplierPayable", "ApproveAdjustment"` |
| **`Approve`** | `FinanceMedicalServicePayable` | ApproveAdjustment | `"FinanceMedicalServicePayable", "ApproveAdjustment"` |
| **`Approve`** | `FinancePurchaseOrder` | Approve | `"FinancePurchaseOrder", "Approve"` |
| **`Approve`** | `FinancePurchaseOrder` | Cancel | `"FinancePurchaseOrder", "Cancel"` |
| **`Approve`** | `FinanceGoodsReceipt` | Cancel | `"FinanceGoodsReceipt", "Cancel"` |
| **`Approve`** | `FinanceInvoiceExchange` | Cancel | `"FinanceInvoiceExchange", "Cancel"` |
| **`Approve`** | `FinancePurchasingInvoice` | Approve | `"FinancePurchasingInvoice", "Approve"` |
| **`Approve`** | `FinanceSupplierReturn` | Confirm | `"FinanceSupplierReturn", "Confirm"` |
| **`Approve`** | `FinanceSupplierReturn` | Cancel | `"FinanceSupplierReturn", "Cancel"` |

**Total pasangan granular yang ditulis saat admin memberi ketiga tier sekaligus:** 38 baris
(`View` = 9, `Operate` = 18, `Approve` = 11) ditambah 3 baris payung itu sendiri = **41 baris
`SysAccessPolicy`**.

---

## D.3b Tabel Ekspansi Lengkap — `Finance.AR.Umbrella` ke Pasangan Granular

| Tier Payung | Resource Granular | Aksi | Kode Aksi di `[AccessPermission]` |
|---|---|---|---|
| **`View`** | `FinanceReceivable` | Read | `"FinanceReceivable", "Read"` |
| **`View`** | `FinanceReceipt` | Read | `"FinanceReceipt", "Read"` |
| **`View`** | `FinanceReceivableInvoiceBatch` | Read | `"FinanceReceivableInvoiceBatch", "Read"` |
| **`View`** | `FinanceBillingIntake` | Read | `"FinanceBillingIntake", "Read"` |
| **`Operate`** | `FinanceReceivable` | Update | `"FinanceReceivable", "Update"` |
| **`Operate`** | `FinanceReceivable` | RequestAdjustment | `"FinanceReceivable", "RequestAdjustment"` |
| **`Operate`** | `FinanceReceivable` | RequestWriteOff | `"FinanceReceivable", "RequestWriteOff"` |
| **`Operate`** | `FinanceReceipt` | Create | `"FinanceReceipt", "Create"` |
| **`Operate`** | `FinanceReceipt` | Allocate | `"FinanceReceipt", "Allocate"` |
| **`Operate`** | `FinanceReceivableInvoiceBatch` | Create | `"FinanceReceivableInvoiceBatch", "Create"` |
| **`Operate`** | `FinanceReceivableInvoiceBatch` | Update | `"FinanceReceivableInvoiceBatch", "Update"` |
| **`Operate`** | `FinanceReceivableInvoiceBatch` | Issue | `"FinanceReceivableInvoiceBatch", "Issue"` |
| **`Operate`** | `FinanceBillingIntake` | Consume | `"FinanceBillingIntake", "Consume"` |
| **`Approve`** | `FinanceReceivable` | ApproveAdjustment | `"FinanceReceivable", "ApproveAdjustment"` |
| **`Approve`** | `FinanceReceivable` | ApproveWriteOff | `"FinanceReceivable", "ApproveWriteOff"` |
| **`Approve`** | `FinanceReceipt` | Reverse | `"FinanceReceipt", "Reverse"` |

**Total pasangan granular yang ditulis saat admin memberi ketiga tier sekaligus:** 16 baris
(`View` = 4, `Operate` = 9, `Approve` = 3) ditambah 3 baris payung itu sendiri = **19 baris
`SysAccessPolicy`**.

> **Catatan sinkronisasi (FIN-DES-068).** Setiap resource granular baru di rumpun AP/AR MUST
> ditambahkan ke D.3, D.3a, atau D.3b (sesuai rumpunnya) **dan** ke berkas kode peta pada
> perubahan yang sama. Menambah ke salah satu saja adalah cacat yang MUST ditolak saat review.

---

## D.4 Matriks Pewarisan Aksi Payung ke Aksi Granular (Action Propagation Rule)

Pewarisan aksi dari tingkat payung ke tingkat granular mengikuti prinsip hirarki peran operasional rumah sakit:

| Aksi pada Payung | Peran & Tanggung Jawab Rumah Sakit | Aksi Granular yang Diwariskan (Hasil Ekspansi) |
|---|---|---|
| `View` | **Auditor / Pengamat:** Staf yang bertugas memantau daftar transaksi, membaca umur piutang/utang (aging), dan mencetak laporan tanpa wewenang mengubah angka | Otomatis memberikan `Read` pada **seluruh** resource granular di bawah kelompok payung terkait. |
| `Operate` | **Staf Pelaksana (Maker):** Staf operasional AP atau AR yang bertugas menginput transaksi harian, membuat draf PO, mencatat penerimaan barang, mengajukan faktur tagihan, serta mengajukan draf pembayaran atau koreksi piutang | Memberikan seluruh aksi **Maker / Operasional**: <br>• Pada `Finance.AP`: `Create`, `Update`, `Submit` (pada Payment, PO, Invoice, Return, Goods Receipt, Tukar Faktur), serta `RequestAdjustment` (pada Utang Supplier/Medis). <br>• Pada `Finance.AR`: `Create`, `Update`, `Allocate` (pada Receipt), `RequestAdjustment`, `RequestWriteOff` (pada Receivable), `Create`, `Issue` (pada Invoice Batch), dan `Consume` (pada Billing Intake). |
| `Approve` | **Pejabat Otorisasi (Checker):** Supervisor Keuangan, Manajer Keuangan, atau Direktur yang berwenang menyetujui transaksi, mengotorisasi pengeluaran uang RS, dan mengesahkan nota penyesuaian | Memberikan seluruh aksi **Checker / Otorisasi**: <br>• Pada `Finance.AP`: `Approve` (pada PO dan Purchasing Invoice < 50jt untuk Supervisor, seluruh nominal untuk Manajer), `Approve` (pada Pembayaran Kas/Bank), `MarkPaid` (validasi transfer bank), `Confirm` (retur supplier), serta `ApproveAdjustment` (koreksi utang). <br>• Pada `Finance.AR`: `ApproveAdjustment` (koreksi piutang), `ApproveWriteOff` (penghapusan piutang), dan `Reverse` (pembatalan alokasi penerimaan). |

### Contoh Pemetaan Peran Nyata di Rumah Sakit:

> **Nama payung yang dipakai di bawah adalah nama kanonikal pasca `FIN-DEC-082`:**
> `Finance.AP.Umbrella` dan `Finance.AR.Umbrella`. Nama lama `Finance.AP`/`Finance.AR` tetap
> dimiliki `FinanceApController`/`FinanceArController` V2 dan tidak disentuh.

1. **Staf AP Farmasi & Logistik (Maker):**
   - Diberi hak payung: `Finance.AP.Umbrella : View, Operate`.
   - Hasil ekspansi sistem: 9 baris `Read` + 18 baris aksi Maker (lihat D.3a). Berhak membuat draf PO, menerima barang gudang, menukar faktur, dan mengajukan pembayaran utang; **tidak** dapat menyetujui PO sendiri atau mengesahkan pencairan kas bank.
2. **Supervisor Akun Hutang / AP (Checker Batas Nominal):**
   - Diberi hak payung: `Finance.AP.Umbrella : View, Operate, Approve`.
   - Hasil ekspansi sistem: 38 baris granular + 3 baris payung = 41 baris `SysAccessPolicy`. Ditegakkan aturan nominal service: Berhak menyetujui PO dan faktur pembelian hingga Rp 50.000.000 (`FIN-DEC-052`).
3. **Manajer Keuangan / Direktur RS (Final Approver):**
   - Diberi hak payung: `Finance.AP.Umbrella : View, Operate, Approve` dan `Finance.AR.Umbrella : View, Operate, Approve`.
   - Hasil ekspansi sistem: 41 + 19 = 60 baris `SysAccessPolicy`. Berhak menyetujui seluruh nominal pembayaran keluar, menyetujui penghapusan piutang asuransi (*bad debt write-off*), dan mengotorisasi jurnal pembalik.
4. **Staf Piutang & Penagihan / AR (Maker):**
   - Diberi hak payung: `Finance.AR.Umbrella : View, Operate`.
   - Hasil ekspansi sistem: 4 baris `Read` + 9 baris aksi Maker (lihat D.3b). Berhak membuat batch invoice penjamin, mengalokasikan pembayaran kasir ke tagihan pasien, dan mengajukan draf koreksi klaim.
5. **Auditor Eksternal / Tim SPI Rumah Sakit:**
   - Diberi hak payung: `Finance.AP.Umbrella : View` dan `Finance.AR.Umbrella : View` serta hak mandiri `FinanceDailyCash : Read`, `FinanceBankDeposit : Read`.
   - Hasil ekspansi sistem: 9 + 4 = 13 baris `Read` granular. Dapat membaca seluruh laporan dan jejak audit transaksi keuangan tanpa memiliki tombol aksi tulis/ubah sedikit pun.

---

## D.5 Penyelarasan Nama 6 Controller Legacy (FIN-DEC-078)

Sesuai ketetapan `FIN-DEC-078`, keenam controller lama diselaraskan agar menggunakan nama Resource kanonikal yang diawali dengan `"Finance"`. Implementer backend wajib mengubah string parameter pada atribut `[AccessPermission]` di masing-masing controller sebagai berikut:

| # | Controller & Berkas C# | Resource Lama | Resource Baru (Kanonikal) | Daftar String `[AccessPermission]` yang Diselaraskan |
|---|---|---|---|---|
| 1 | `FinancePaymentsController`<br>`Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` | `Payment` | `FinancePayment` | • `[AccessPermission("FinancePayment", "Read")]`<br>• `[AccessPermission("FinancePayment", "Create")]`<br>• `[AccessPermission("FinancePayment", "Update")]`<br>• `[AccessPermission("FinancePayment", "Submit")]`<br>• `[AccessPermission("FinancePayment", "Approve")]`<br>• `[AccessPermission("FinancePayment", "MarkPaid")]`<br>• `[AccessPermission("FinancePayment", "Cancel")]` |
| 2 | `FinanceReceiptsController`<br>`Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` | `Receipt` | `FinanceReceipt` | • `[AccessPermission("FinanceReceipt", "Read")]`<br>• `[AccessPermission("FinanceReceipt", "Create")]`<br>• `[AccessPermission("FinanceReceipt", "Allocate")]`<br>• `[AccessPermission("FinanceReceipt", "Reverse")]` |
| 3 | `FinanceReceivablesController`<br>`Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | `Receivable` | `FinanceReceivable` | • `[AccessPermission("FinanceReceivable", "Read")]`<br>• `[AccessPermission("FinanceReceivable", "Update")]`<br>• `[AccessPermission("FinanceReceivable", "RequestAdjustment")]`<br>• `[AccessPermission("FinanceReceivable", "ApproveAdjustment")]`<br>• `[AccessPermission("FinanceReceivable", "RequestWriteOff")]`<br>• `[AccessPermission("FinanceReceivable", "ApproveWriteOff")]` |
| 4 | `FinanceSupplierPayablesController`<br>`Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceSupplierPayablesController.cs` | `SupplierPayable` | `FinanceSupplierPayable` | • `[AccessPermission("FinanceSupplierPayable", "Read")]`<br>• `[AccessPermission("FinanceSupplierPayable", "Create")]`<br>• `[AccessPermission("FinanceSupplierPayable", "Update")]`<br>• `[AccessPermission("FinanceSupplierPayable", "RequestAdjustment")]`<br>• `[AccessPermission("FinanceSupplierPayable", "ApproveAdjustment")]` |
| 5 | `FinanceBillingIntakeController`<br>`Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` | `BillingIntake` | `FinanceBillingIntake` | • `[AccessPermission("FinanceBillingIntake", "Read")]`<br>• `[AccessPermission("FinanceBillingIntake", "Consume")]` |
| 6 | `FinanceAccountingEventsController`<br>`Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | `AccountingEvents` (jamak) | `FinanceAccountingEvent` (tunggal) | • `[AccessPermission("FinanceAccountingEvent", "Read")]` |

---

## D.6 Strategi Migrasi Data Peran & Mekanisme Seeder

### 1. Skrip Migrasi SQL Idempotent Database (`SysRolePermissions`)

Perubahan nama resource pada atribut kode C# wajib diiringi dengan pembaruan data pada tabel izin peran sistem yang sudah terlanjur tersimpan di basis data lingkungan berjalan (Development, UAT, Staging, maupun Produksi). Jika tidak dimigrasikan, pengguna yang sudah memegang peran akan mendapati pesan kesalahan `403 Forbidden` saat mengakses API karena string resource di database masih merujuk ke nama lama.

> **DIKOREKSI — `FIN-PERM-1.4`, 29 September 2026 (AMENDMENT REVISI 7).** Skrip di bawah ini
> **DICABUT**: ia menyasar tabel `SysRolePermissions` berkolom `ResourceName`, dan **tabel maupun
> kolom itu tidak ada** pada skema backend ini. Dijalankan apa adanya, ia gagal
> `relation "SysRolePermissions" does not exist`. Koreksi ini ditemukan saat implementasi
> `BE-FIN-042`; penggantinya sudah ditulis dan berada di
> `Migrations/scripts/be-fin-042-role-permissions-migration.sql`.

**Mekanisme yang sebenarnya.** Hak akses disimpan pada `SysAccessPolicy` sebagai **kunci asing**
`(DepartmentId, PositionId, ControllerAccessId, ActionAccessId)` — berbasis **Departemen + Posisi**,
bukan "Role", dan menunjuk registry `SysControllerAccess`/`SysActionAccess` lewat Id, bukan lewat
string nama resource. Karena itu rename nama resource **tidak dapat** dikerjakan dengan `UPDATE`
kolom nama. Yang terjadi sesungguhnya:

1. `AccessMenuSeeder` (berjalan otomatis saat aplikasi start) **membuat baris registry BARU** untuk
   nama kanonikal, dan **menutup** baris lama (`IsActive=false`, `IsDelete=true`) — ia tidak pernah
   mengganti nilai `ControllerName` pada baris yang sudah ada.
2. Baris `SysAccessPolicy` yang sudah ada **masih menunjuk Id lama**. Tanpa migrasi, setiap
   Departemen x Posisi yang sudah diberi hak kehilangannya (`403`) begitu registry lama ditutup.

**Bentuk skrip pengganti** (sudah ditulis, mengikuti pola `be-sec-003b-policy-expansion.sql` milik
`platform-authorization`): peta rename enam pasang → resolusi identitas lama dan baru dari registry →
dry-run baca-saja → Tahap 1 melestarikan `SysAccessPolicy` ke Id baru (idempotent, digerbang
prasyarat) → verifikasi parity ditinjau manusia → Tahap 2 menonaktifkan policy lama → bagian
rollback. Dua tahap sengaja **dua transaksi terpisah**.

**Urutan eksekusi yang MUST dipatuhi, tidak boleh dibalik:** deploy source hasil rename → jalankan
aplikasi sekali dalam jendela pemeliharaan (supaya seeder membuat registry baru) → Tahap 1 →
verifikasi → Tahap 2.

### 2. Logika Ekspansi — DIKOREKSI `FIN-PERM-1.4` (`FIN-DEC-082`, `FIN-DEC-083`)

> **DIKOREKSI — 29 September 2026 (AMENDMENT REVISI 7).** Rancangan semula menempatkan ekspansi di
> `AccessMenuSeeder`. Itu **tidak dapat dilaksanakan**: seeder tersebut secara eksplisit **tidak
> pernah** menulis `SysAccessPolicy` — ia hanya mengelola tiga tabel registry
> (`SysApplicationModule`, `SysControllerAccess`, `SysActionAccess`), dan komentar kelasnya sendiri
> menyatakan "kemampuan yang baru terdaftar tetap ditolak untuk semua orang sampai admin
> memberikannya lewat layar Akses Role". Selain itu nama payungnya berubah (lihat butir 0 di bawah).
> Rancangan yang berlaku sekarang diturunkan `FIN-DES-066`..`069` (`02-backend-architecture.md`
> AMENDMENT REVISI 11).

**0. Nama resource payung berubah (`FIN-DEC-082`).** Payung **bukan** `Finance.AP`/`Finance.AR`,
melainkan **`Finance.AP.Umbrella`** dan **`Finance.AR.Umbrella`**. Alasannya: `Finance.AP` dan
`Finance.AR` ternyata **sudah dipakai** dua controller yang berjalan nyata — `FinanceApController`
(`api/finance/payable`) dan `FinanceArController` (`api/finance/receivable`), endpoint "V2" AP/AR —
dengan aksi mereka sendiri (`View`, `Payment`, `Create`). Memakai nama itu untuk payung akan menimpa
arti hak akses yang sudah dipegang kedua endpoint tersebut. Kedua controller V2 **tidak disentuh**.

**1. Titik tulis (`FIN-DEC-083`, `FIN-DES-067`).** Ekspansi terjadi **saat admin memberi hak lewat
layar Akses Role**, bukan saat seeding. Tempatnya `RoleAccessController.ApplyPoliciesAsync` — satu
satunya jalur penulisan `SysAccessPolicy` di aplikasi ini, dipakai bersama oleh
`POST /role-access/policies` dan `POST /role-access/policies/copy`.

**2. Algoritma.** Bila permintaan simpan memuat pasangan payung, sebelum gerbang validasi registry
yang sudah ada, tambahkan seluruh pasangan granular yang dicakup tier payung itu (D.3 + D.4).
Seluruhnya berjalan di dalam transaksi yang sudah dibuka method tersebut, sehingga payung dan
granularnya tersimpan atau batal bersama. Idempotent: upsert sudah berkunci alami
`(Departemen, Posisi, Controller, Action)`.

**3. Penolakan yang dapat ditindaklanjuti.** Bila satu identitas granular pada peta tidak
terselesaikan di registry (sudah pensiun, tersembunyi, atau system-only), permintaan **ditolak** dan
pesannya **MUST menyebut pasangan mana** yang hilang — bukan ditulis sebagian secara diam-diam.

**4. Pencabutan.** Melepas centang payung saja **tidak** mencabut granularnya: baris granular ikut
tampil tercentang di layar dan terkirim ulang pada penyimpanan berikutnya. Mencabut granular adalah
langkah eksplisit admin. Ini perilaku yang disengaja (`FIN-DEC-083`), bukan cacat.

**5. `HasAccessAsync` tidak disentuh.** Pemeriksaan hak akses saat request tetap pencarian langsung
atas `SysAccessPolicy`. Nol perubahan pada algoritma otorisasi yang dipakai seluruh modul aplikasi.

**6. Jaminan integritas masa depan.** Setiap resource granular baru di rumpun AP/AR **wajib**
ditambahkan ke D.3 dokumen ini **dan** ke peta di kode pada perubahan yang sama.

**7. Gerbang: pembawa resource payung.** Resource payung wajib punya pembawa yang terdaftar di
registry; platform hari ini **belum dapat** mendaftarkan resource yang tidak punya satu pun endpoint.

- `FIN-OQ-038` (pilihan pembawanya) — **CLOSED 29 September 2026** oleh **`FIN-DEC-084`**: dipilih
  **perluasan `platform-authorization`** supaya resource tanpa endpoint dapat dideklarasikan lewat
  opt-in eksplisit, dengan penjaga anti-typo yang ada sekarang **tetap dipertahankan**. Membuat
  controller pembawa di Finance ditolak; membatalkan payung juga tidak dipilih.
- `FIN-OQ-039` (persetujuan dan penjadwalan perluasan itu) — **TERBUKA**, ditujukan kepada Security
  Owner + pemilik `platform-authorization`. `FIN-DEC-084` adalah keputusan **sisi Finance**: ia
  meminta, bukan menyetujui atas nama modul lain.

Sampai `FIN-OQ-039` turun, mekanisme ekspansi **MUST NOT** diimplementasikan. Ini **tidak** menahan
penyelarasan nama enam controller (D.5) yang sudah selesai, maupun pekerjaan frontend mana pun.


# AMENDMENT REVISI 13 — Pelacakan klaim penjamin: nol resource dan nol action baru

`last_changed_in`: `FIN-PERM-1.5` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-098`; dirancang `FIN-DES-072`.

## E.1 Ringkasan dampak

| Hal | Nilai |
|---|---|
| Resource baru | **NOL** |
| Action baru | **NOL** |
| Baris registry baru | **NOL** |
| Ketergantungan pada `FIN-OQ-039` (perluasan registry resource tanpa endpoint) | **TIDAK ADA** — amendment ini dapat diimplementasikan penuh sementara gerbang itu masih tertutup |

Kelima endpoint baru memakai pasangan yang **sudah terdaftar** dari pemindaian atribut source:

| Endpoint baru | Pasangan hak akses | Sudah terdaftar lewat |
|---|---|---|
| `POST /receivable-invoice-batches/{id}/claim/verify` | `FinanceReceivableInvoiceBatch : Update` | `[AccessAction("Update", ...)]` pada `FinanceReceivableInvoiceBatchesController` (`BE-FIN-039`) |
| `POST /receivable-invoice-batches/{id}/claim/approve` | `FinanceReceivableInvoiceBatch : Update` | idem |
| `POST /receivable-invoice-batches/{id}/claim/close` | `FinanceReceivableInvoiceBatch : Update` | idem |
| `GET /receivables/write-offs` | `FinanceReceivable : Read` | `FinanceReceivablesController` (`BE-FIN-018`) |
| `GET /receipts/reversed-allocations` | `FinanceReceipt : Read` | `FinanceReceiptsController` (`BE-FIN-016`) |

## E.2 Kewenangan yang **tidak** dijaga mesin hak akses

Dicatat apa adanya, karena inilah bagian yang tidak terbaca dari daftar endpoint:

| Kewenangan | Dijaga oleh | Yang **tidak** dijaganya | Risiko yang diterima |
|---|---|---|---|
| Siapa yang boleh menyatakan nominal persetujuan penjamin | `FinanceReceivableInvoiceBatch : Update` saja | Mesin hak akses **tidak** membedakan petugas yang menerbitkan tagihan dari petugas yang mencatat jawaban penjamin — satu orang dapat melakukan keduanya | **Diterima sadar** lewat `FIN-DEC-098`: owner memilih tanpa jenjang approval. Risikonya nominal persetujuan dicatat keliru atau sepihak tanpa ada pemeriksa kedua |
| Besarnya selisih yang dihapus dari buku | **Maker-checker write-off yang sudah ada** (`BE-FIN-018`), bukan oleh aksi klaim | Aksi klaim sendiri tidak pernah mengurangi piutang | Rendah — penghapusan tetap melewati penyetuju, sesuai `FIN-DES-071` |

Penegasan yang **MUST** dipegang implementasi: longgarnya wewenang pada sumbu klaim **MUST NOT**
dipakai sebagai alasan melonggarkan jenjang write-off. Keduanya proses berbeda.

## E.3 Pencatatan audit

Mengikuti konvensi project tanpa pengecualian: ketiga `POST` aksi klaim **dicatat** logger
(`EntityId`, controller, action, status); kedua `GET` baru **tidak** dicatat.

Kolom sensitif: nol kolom baru ditandai sensitif. `ClaimNote` dan `PayerClaimReference` memuat
rujukan administratif penjamin, **bukan** data medis pasien — keduanya **MUST NOT** diisi
diagnosis atau keterangan klinis, dan antarmuka **MUST NOT** mengarahkan petugas ke sana.

# AMENDMENT REVISI 13 (lanjutan) — Resource hak akses baru: piutang sewa non-pasien

`last_changed_in`: `FIN-PERM-1.6` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-103`; dirancang `FIN-DES-077`.

## F.1 Resource dan action baru

| Resource | Action | `[AccessPermission(...)]` persis | Dipakai endpoint |
|---|---|---|---|
| `FinanceNonPatientReceivable` | `Read` | `[AccessPermission("FinanceNonPatientReceivable", "Read")]` | `GET /`, `GET /{id}`, `GET /aging`, `GET /summary`, `GET /filters/metadata` |
| `FinanceNonPatientReceivable` | `Create` | `[AccessPermission("FinanceNonPatientReceivable", "Create")]` | `POST /` |
| `FinanceNonPatientReceivable` | `Update` | `[AccessPermission("FinanceNonPatientReceivable", "Update")]` | `PUT /{id}`, `POST /{id}/settlements`, `POST /{id}/write-off`, `POST /{id}/cancel` |

Ketiganya terdaftar lewat **pemindaian atribut biasa** karena controllernya nyata dan punya
endpoint. Berbeda dari resource payung pada `FIN-OQ-039` yang tertahan justru karena tidak punya
endpoint — amendment ini **nol ketergantungan** pada gerbang itu.

Penghapusan piutang dan pembatalan sengaja **tidak** mendapat action sendiri. `FIN-DEC-103`
menyamakan wewenangnya dengan perubahan biasa; memberinya action terpisah akan menyiratkan adanya
jenjang yang sebenarnya tidak ada, dan membuat admin menyangka ia dapat memisahkan keduanya.

## F.2 Kewenangan yang **tidak** dijaga mesin hak akses

Bagian ini adalah yang paling penting pada kapabilitas ini, dan ditulis apa adanya.

| Kewenangan | Dijaga oleh | Yang **tidak** dijaganya | Risiko yang diterima |
|---|---|---|---|
| Menghapus piutang sewa yang tidak tertagih | `FinanceNonPatientReceivable : Update` saja | **Nol pemeriksa kedua.** Petugas yang mencatat tagihan dapat menghapusnya sendiri pada hari yang sama, tanpa sepengetahuan siapa pun | **Diterima sadar** lewat `FIN-DEC-103`, sesudah konsekuensinya disodorkan. Ini **berbeda** dari piutang pasien yang memakai maker-checker |
| Mencatat pelunasan yang tidak pernah benar-benar diterima | `FinanceNonPatientReceivable : Update` saja | Tidak ada pencocokan ke rekening koran maupun kas harian, karena jalur ini sengaja dikelola terpisah dari kas (`FIN-DES-075`, `FIN-DEC-109`) | Tinggi, risiko diterima sadar oleh pemilik (`FIN-DEC-109`, `FIN-DEC-110`). Penahannya: alasan wajib, jejak `IdentityModel`, dan banner di layar. Kejadian akuntansi menunggu `FIN-OQ-044(b)` |
| Besarnya denda keterlambatan | Tidak dijaga sama sekali | Denda adalah angka yang diketik petugas; tidak ada rumus, batas atas, maupun pembanding | Diterima lewat `FIN-DEC-102` |

**Mitigasi yang tersedia tanpa mengubah keputusan mana pun:** alasan wajib diisi pada penghapusan
dan pembatalan (`FIN-VAL-158`), seluruh aksi non-`GET` tercatat logger, dan kolom audit
`IdentityModel` menyimpan siapa yang melakukannya.

**Batas penularan yang MUST dijaga:** kelonggaran pada kapabilitas ini **MUST NOT** dijadikan dasar
melonggarkan maker-checker pada `FinReceivableWriteOff`, `FinReceivableAdjustment`, atau jalur
persetujuan pembayaran mana pun yang sudah berjalan.

## F.3 Peta peran

| Peran rumah sakit | Butir hak akses yang diberikan | Catatan |
|---|---|---|
| Staf AR Finance | `FinanceNonPatientReceivable : Read`, `Create`, `Update` | Seluruh kapabilitas — `FIN-DEC-103` |
| Supervisor/Manajer Finance | `FinanceNonPatientReceivable : Read` | Pemantauan. Tidak ada aksi yang khusus menuntut jenjang ini, karena memang tidak ada jenjang |
| Peran lain | — | Tidak diberikan secara bawaan |

Pemberian sesungguhnya tetap dilakukan admin lewat layar Akses Role; tabel ini usulan, bukan seeder.

## F.4 Pencatatan audit dan kolom sensitif

`GET` tidak dicatat; `POST` dan `PUT` dicatat (`EntityId`, controller, action, status), mengikuti
konvensi project tanpa pengecualian.

Kolom sensitif: **nol**. `CounterpartyName` adalah nama badan usaha atau penyewa komersial, bukan
data pasien. `RentedObject`, `ReferenceNumber`, dan `Note` **MUST NOT** diisi data pasien atau
keterangan klinis, dan antarmuka **MUST NOT** mengarahkan petugas ke sana.

---

# Bagian G — Revisi 14: hak akses cutover, bukti, dan ambang

| Field | Nilai |
|---|---|
| Contract version | `FIN-PERM-1.7` — status **`draft`** |
| Naik dari | `FIN-PERM-1.6` (`approved` 1 Oktober 2026) |
| Traceability | `FIN-DEC-128`..`136`; `FIN-DES-078`, `086`..`089` |
| Dampak | **Empat resource baru**, satu action baru (`Approve`) pada resource baru |
| Ketergantungan pada `FIN-OQ-039` | **Nol.** Keempat resource punya controller nyata beserta endpoint |

## G.1 Resource dan action baru

| Resource | Action | String yang dipakai |
|---|---|---|
| `FinanceSubledgerSetup` | `Read` | `[AccessPermission("FinanceSubledgerSetup", "Read")]` |
| `FinanceSubledgerSetup` | `Create` | `[AccessPermission("FinanceSubledgerSetup", "Create")]` |
| `FinanceSubledgerSetup` | `Update` | `[AccessPermission("FinanceSubledgerSetup", "Update")]` |
| `FinanceSubledgerSetup` | `Approve` | `[AccessPermission("FinanceSubledgerSetup", "Approve")]` |
| `FinanceOpeningItemBatch` | `Read` | `[AccessPermission("FinanceOpeningItemBatch", "Read")]` |
| `FinanceOpeningItemBatch` | `Create` | `[AccessPermission("FinanceOpeningItemBatch", "Create")]` |
| `FinanceOpeningItemBatch` | `Update` | `[AccessPermission("FinanceOpeningItemBatch", "Update")]` |
| `FinanceOpeningItemBatch` | `Approve` | `[AccessPermission("FinanceOpeningItemBatch", "Approve")]` |
| `FinanceTransactionProof` | `Read` | `[AccessPermission("FinanceTransactionProof", "Read")]` |
| `FinanceTransactionProof` | `Create` | `[AccessPermission("FinanceTransactionProof", "Create")]` |
| `MstDirectPaymentThreshold` | `Read` | `[AccessPermission("MstDirectPaymentThreshold", "Read")]` |
| `MstDirectPaymentThreshold` | `Update` | `[AccessPermission("MstDirectPaymentThreshold", "Update")]` |

## G.2 Atribut controller

Mengikuti pola `FinanceReceivablesController` apa adanya.

```csharp
[AccessController("CORPORATE_FINANCE_MANAGEMENT_SUBLEDGER_SETUP",
    "Corporate Finance Management Subledger Setup", "Subledger Setup",
    AreaName = "Corporate", ControllerName = "FinanceSubledgerSetup",
    Description = "Pemetaan akun control dan saldo awal cutover", SortOrder = 70)]
[Tags("Corporate / Finance Management / Subledger Setup")]

[AccessController("CORPORATE_FINANCE_MANAGEMENT_OPENING_ITEM_BATCH",
    "Corporate Finance Management Opening Item Batch", "Opening Item Batch",
    AreaName = "Corporate", ControllerName = "FinanceOpeningItemBatch",
    Description = "Migrasi tagihan lama lewat batch spreadsheet", SortOrder = 71)]
[Tags("Corporate / Finance Management / Opening Item Batch")]

[AccessController("CORPORATE_FINANCE_MANAGEMENT_TRANSACTION_PROOF",
    "Corporate Finance Management Transaction Proof", "Transaction Proof",
    AreaName = "Corporate", ControllerName = "FinanceTransactionProof",
    Description = "Bukti pembayaran langsung piutang dan utang", SortOrder = 72)]
[Tags("Corporate / Finance Management / Transaction Proof")]

[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DIRECT_PAYMENT_THRESHOLD",
    "Corporate Finance Management Direct Payment Threshold", "Master Data",
    AreaName = "Corporate", ControllerName = "MstDirectPaymentThreshold",
    Description = "Ambang nilai pembayaran langsung", SortOrder = 73)]
[Tags("Corporate / Finance Management / Master Data / Direct Payment Threshold")]
```

`SortOrder` di atas **usulan**, bukan nilai final — ia urutan tampil pada layar hak akses dan boleh
disesuaikan admin tanpa mengubah desain.

## G.3 Matriks endpoint dan pencatatan logger

Mengikuti konvensi project: `GET` **tidak** dicatat logger.

| Endpoint | Resource | Action | Dicatat logger |
|---|---|---|:---:|
| `GET /subledger-setup/control-accounts` | `FinanceSubledgerSetup` | `Read` | Tidak |
| `GET /subledger-setup/control-accounts/coverage` | `FinanceSubledgerSetup` | `Read` | Tidak |
| `POST /subledger-setup/control-accounts` | `FinanceSubledgerSetup` | `Create` | **Ya** |
| `PUT /subledger-setup/control-accounts/{id}` | `FinanceSubledgerSetup` | `Update` | **Ya** |
| `POST /subledger-setup/control-accounts/{id}/deactivate` | `FinanceSubledgerSetup` | `Update` | **Ya** |
| `GET /subledger-setup/opening-balances` | `FinanceSubledgerSetup` | `Read` | Tidak |
| `POST /subledger-setup/opening-balances` | `FinanceSubledgerSetup` | `Create` | **Ya** |
| `PUT /subledger-setup/opening-balances/{id}` | `FinanceSubledgerSetup` | `Update` | **Ya** |
| `POST /subledger-setup/opening-balances/{id}/approve` | `FinanceSubledgerSetup` | `Approve` | **Ya** |
| `POST /subledger-setup/opening-balances/{id}/lock` | `FinanceSubledgerSetup` | `Approve` | **Ya** |
| `GET /opening-item-batches` | `FinanceOpeningItemBatch` | `Read` | Tidak |
| `GET /opening-item-batches/{id}` | `FinanceOpeningItemBatch` | `Read` | Tidak |
| `GET /opening-item-batches/template` | `FinanceOpeningItemBatch` | `Read` | Tidak |
| `POST /opening-item-batches` | `FinanceOpeningItemBatch` | `Create` | **Ya** |
| `POST /opening-item-batches/{id}/validate` | `FinanceOpeningItemBatch` | `Update` | **Ya** |
| `POST /opening-item-batches/{id}/declare-accounting-opening` | `FinanceOpeningItemBatch` | `Update` | **Ya** |
| `POST /opening-item-batches/{id}/approve` | `FinanceOpeningItemBatch` | `Approve` | **Ya** |
| `POST /opening-item-batches/{id}/reject` | `FinanceOpeningItemBatch` | `Update` | **Ya** |
| `POST /transaction-proofs` | `FinanceTransactionProof` | `Create` | **Ya** — hanya `EntityId`, controller, action, status |
| `GET /transaction-proofs/{id}` | `FinanceTransactionProof` | `Read` | Tidak |
| `GET /transaction-proofs/{id}/metadata` | `FinanceTransactionProof` | `Read` | Tidak |
| `GET /master-data/direct-payment-threshold` | `MstDirectPaymentThreshold` | `Read` | Tidak |
| `PUT /master-data/direct-payment-threshold` | `MstDirectPaymentThreshold` | `Update` | **Ya** |
| `GET /receivables/{id}/movements` | `FinanceReceivable` | `Read` | Tidak |
| `GET /supplier-payables/{id}/movements` | `FinanceSupplierPayable` | `Read` | Tidak |
| `GET /daily-cash/cash-movements` | `FinanceCashManagement` | `Read` | Tidak |
| `GET /accounting-events/subledger-balances/position` | `FinanceAccountingEvent` | `Read` | Tidak |
| `GET /accounting-events/subledger-balances/{kode}/variance` | `FinanceAccountingEvent` | `Read` | Tidak |
| `POST /accounting-events/subledger-balances/restate` | `FinanceAccountingEvent` | `Create` | **Ya** |

**Payload logger MUST NOT memuat kolom bertanda sensitif** pada kamus data: `Notes` pada ketiga buku
mutasi, dan `ValidationSummaryJson` pada batch migrasi. Keduanya dapat memuat nama debitur,
supplier, atau pihak ketiga.

## G.4 Kenapa `Approve` adalah action baru, dan kenapa itu tidak bertentangan

Blueprint ini dua kali **menolak** jenjang approval: `FIN-DEC-098` (status klaim) dan `FIN-DEC-103`
(piutang sewa). Action `Approve` di sini **tidak** membalik keduanya.

| Hal | `FIN-DEC-098`/`103` | Revisi 14 |
|---|---|---|
| Yang diputuskan | Transaksi **harian** oleh staf AR | **Pembukaan buku**: saldo awal dan migrasi tagihan lama |
| Frekuensi | Berkali-kali sehari | **Satu kali** saat cutover |
| Akibat bila salah | Satu tagihan atau satu status | Seluruh rekonsiliasi Finance dan Accounting salah sejak periode pertama |
| Dapat dikoreksi | Ya, lewat jalur yang sudah ada | **Tidak** — baris `LOCKED` tidak dapat diubah |

Karena itu `Approve` dipisahkan dari `Update`: ia menandai tindakan yang tidak dapat ditarik.
Pemisahan ini **MUST NOT** dipakai sebagai dasar menambahkan jenjang approval pada transaksi harian.

## G.5 Hak akses yang TIDAK dijaga mesin, dicatat apa adanya

| Yang tidak dijaga | Akibatnya | Mitigasi yang ada |
|---|---|---|
| Satu orang dapat memegang `Create` **dan** `Approve` sekaligus | Maker-checker pada saldo awal dan batch migrasi dapat dilewati satu orang | Mesin hak akses tidak mengenal pemisahan tugas. Pencegahannya **pemberian hak** oleh admin, dan itu **MUST** disebutkan saat menyerahkan modul |
| Perubahan ambang tidak punya jenjang | Pejabat berwenang dapat menaikkannya lalu membayar di bawahnya | `ChangeReason` wajib dan tercatat logger (`FIN-DEC-134`) |
| Siapa boleh melihat berkas bukti | Hak akses hanya membedakan `Read` dan `Create`, bukan per pemilik transaksi | Menunggu `FIN-OQ-075` |
| Pembayaran dipecah di bawah ambang | Tidak terdeteksi | Diterima sadar (`FIN-DEC-134`) |

## G.6 Nol resource untuk hosted service

Ketiga hosted service **tidak** mendapat resource hak akses: mereka tidak punya endpoint dan tidak
dipanggil pengguna. Kredensial yang mereka pakai untuk memanggil kotak masuk Accounting adalah
**akun layanan**, dan mekanismenya masih terbuka bersama Platform dan Accounting (G3) — **bukan**
bagian kontrak hak akses ini.

Pemicu manual untuk worker pengiriman **sengaja tidak dibuat** (`FIN-API-1.5` bagian `F.9`), sehingga
tidak ada permukaan hak akses yang dapat memutar gerbang `FIN-DES-078`.
