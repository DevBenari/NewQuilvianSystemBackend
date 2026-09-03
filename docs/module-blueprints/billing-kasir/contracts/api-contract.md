# Billing dan Kasir — API Contract

`contract_version: BIL-API-0.4` · status **approved** · owner API/Billing/Security · approved 20 Agustus 2026 · input decision `0.2`/hash tercatat di manifest · kompatibilitas: additive API baru.

**Amendment 2 September 2026 (approved Product/Domain Owner, `BKC-DEC-059`–`062`)**: dua endpoint baru ditambahkan pada grup Invoices di bawah (`POST catalog-charges`, `GET catalog-charges/coverage-preview`) untuk entri manual berbasis katalog tarif + coverage per item pada form "Buat Invoice Manual (Testing)". Keduanya **Rencana (belum tersedia)** — desain disetujui, belum ada di source (belum diimplementasikan). Detail desain: [`02-backend-architecture.md`](../02-backend-architecture.md#amendment-2-september-2026--entri-manual-berbasis-katalog-tarif--coverage-per-item).

**Update 3 September 2026 (`BE-BKC-019`)**: `POST catalog-charges` sudah diimplementasikan di backend source (lihat status baris di bawah) — status "Diimplementasikan" berarti source ada dan sudah dibaca langsung dari controller/service terkait, BUKAN berarti sudah diverifikasi lewat klik-coba ter-autentikasi atau Swagger sudah disanitasi; keduanya masih tertunda. Lihat `task/report/backend/BE-BKC-019.md`.

**Update 3 September 2026 (`BE-BKC-020`)**: `GET catalog-charges/coverage-preview` juga sudah diimplementasikan di backend source pada sesi yang sama — status yang sama ("Diimplementasikan", belum diverifikasi manual/build) berlaku. Lihat `task/report/backend/BE-BKC-020.md`.

**Rekonsiliasi 25 Agustus 2026 (`ISSUE-FE-003`)**: seluruh endpoint transaksi di bawah **sudah diimplementasikan** di backend source sejak commit `1d61a5b` (part 1) dan `22bf9cf` (part 2) pada branch `Yasmina`/`AgentCodexBackend` — dokumen ini sebelumnya masih menandainya "Rencana (belum tersedia)" secara menyeluruh, padahal source sudah ada. Status "Diimplementasikan" pada tabel di bawah berarti **source backend ada dan sudah dibaca langsung dari controller/service terkait**, BUKAN berarti sudah diverifikasi lewat klik-coba ter-autentikasi atau migration sudah dieksekusi ke database bersama — keduanya masih tertunda untuk sebagian besar slice (lihat `MODULE-STATUS.md`). Endpoint `GET` tambahan pada Cashier Shifts (`{id}`) dan Financial Exceptions (`invoices/{invoiceId}`, `invoices/{invoiceId}/refundable-credits`, `refunds/{id}`, `adjustments/{id}`, `write-offs/{id}`), serta seluruh Master Data / Register, ditambahkan hari ini (`ISSUE-FE-006`, `ISSUE-FE-007`, `ISSUE-FE-008`) dan **belum tercantum** di tabel endpoint di bawah karena dokumen ini belum ditulis ulang penuh — lihat laporan task masing-masing untuk detail endpoint baru tersebut.

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Cari invoice | `BillingInvoice : Read` | query filter | `ApiResponse<Paged<InvoiceSummaryResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/{id}` | Detail dan versi | `BillingInvoice : Read` | — | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/from-source` | Tambah/update charge idempotent | `BillingInvoice : Create` | `UpsertChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/recalculate` | Buat calculation version | `BillingInvoice : Update` | `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/items/{itemId}/void` | Void item eligible | `BillingInvoice : Update` | `VoidInvoiceItemRequest` | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/discounts` | Terapkan diskon | `BillingDiscount : Create` | `ApplyDiscountRequest` | `ApiResponse<DiscountResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/discounts/{discountId}/approve` | Approve diskon dokter | `BillingDoctorDiscount : Approve` | `ApproveDiscountRequest` | `ApiResponse<DiscountResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/catalog-charges` | Tambah charge dari katalog `MstTariff` — harga diambil server-side, tidak dapat diinput manual (`BKC-DEC-059`) | `BillingInvoice : Create` | `AddCatalogChargeRequest` | `ApiResponse<InvoiceDetailResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/catalog-charges/coverage-preview` | Preview coverage satu tarif untuk encounter (advisory, read-only) — query `encounterId`, `tariffId`, `quantity` (`BKC-DEC-060`) | `BillingInvoice : Read` | — | `ApiResponse<CatalogChargeCoveragePreviewResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Lihat saldo/ledger | `BillingDeposit : Read` | — | `ApiResponse<DepositResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/deposits/{encounterId}/top-ups` | Top-up deposit | `BillingDeposit : Create` | `DepositTopUpRequest` | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/deposits/{encounterId}/allocations` | Progress allocation | `BillingDeposit : Allocate` | `DepositAllocationRequest` | `ApiResponse<AllocationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/settlements` | Mulai pembayaran split | `BillingPayment : Create` | `CreateSettlementRequest` | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/settlements/{id}/tenders` | Tambah attempt tender | `BillingPayment : Create` | `CreateTenderRequest` | `ApiResponse<TenderResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/settlements/{id}` | Status tender/alokasi | `BillingPayment : Read` | — | `ApiResponse<SettlementResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Financial Exceptions

Base URL: `api/v1/health-services/billing-management/billing/financial-exceptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/adjustments` | Ajukan koreksi | `BillingAdjustment : Create` | `CreateAdjustmentRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/adjustments/{id}/approve` | Approve Finance | `BillingAdjustment : Approve` | `ApprovalRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/refunds` | Ajukan refund | `BillingRefund : Create` | `CreateRefundRequest` | `ApiResponse<RefundResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/refunds/{id}/approve` | Approve refund | `BillingRefund : Approve` | `ApprovalRequest` | `ApiResponse<RefundResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/write-offs` | Ajukan write-off | `BillingWriteOff : Create` | `CreateWriteOffRequest` | `ApiResponse<WriteOffResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/write-offs/{id}/approve` | Approve write-off | `BillingWriteOff : Approve` | `ApprovalRequest` | `ApiResponse<WriteOffResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{type}/{id}/reverse` | Entry reversal | `BillingFinancialException : Reverse` | `ReverseExceptionRequest` | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Cashier / Shifts

Base URL: `api/v1/health-services/billing-management/cashier/shifts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/open` | Buka shift | `CashierShift : Create` | `OpenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/current` | Shift aktif | `CashierShift : Read` | — | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/handover` | Serah-terima dua kasir | `CashierShift : Handover` | `HandoverShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/close` | Catat fisik dan tutup | `CashierShift : Close` | `CloseShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/variance-reviews` | Review selisih | `CashierShift : Review` | `ReviewVarianceRequest` | `ApiResponse<CashVarianceResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/reopen` | Reopen berotorisasi | `CashierShift : Reopen` | `ReopenShiftRequest` | `ApiResponse<CashierShiftResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/invoices/{invoiceId}/preview` | Validasi kesiapan | `BillingFinalization : Read` | — | `ApiResponse<FinalizationPreviewResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/invoices/{invoiceId}` | Finalisasi dan handoff | `BillingFinalization : Create` | `FinalizeInvoiceRequest` | `ApiResponse<FinalizationResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `GET` | `/{id}/handoffs` | Status AR/AP handoff | `BillingFinalization : Read` | — | `ApiResponse<HandoffStatusResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Administration Fee Policy

Base URL: `api/v1/health-services/billing-management/master-data/administration-fee-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar versi policy | `AdministrationFeePolicy : Read` | filter tanggal/status | `ApiResponse<Paged<AdministrationFeePolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat versi effective-dated | `AdministrationFeePolicy : Create` | `CreateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi versi belum efektif | `AdministrationFeePolicy : Update` | `UpdateAdministrationFeePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan tanpa hapus histori | `AdministrationFeePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<AdministrationFeePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Discount Policy

Base URL: `api/v1/health-services/billing-management/master-data/discount-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar policy | `DiscountPolicy : Read` | filter | `ApiResponse<Paged<DiscountPolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat policy | `DiscountPolicy : Create` | `CreateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `DiscountPolicy : Update` | `UpdateDiscountPolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `DiscountPolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<DiscountPolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Tax Rule

Base URL: `api/v1/health-services/billing-management/master-data/tax-rules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar tax rule | `TaxRule : Read` | filter | `ApiResponse<Paged<TaxRuleResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat tax rule | `TaxRule : Create` | `CreateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `TaxRule : Update` | `UpdateTaxRuleRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `TaxRule : Update` | `DeactivatePolicyRequest` | `ApiResponse<TaxRuleResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

### Health Services / Billing Management / Master Data / Room Charge Policy

Base URL: `api/v1/health-services/billing-management/master-data/room-charge-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar room rule | `RoomChargePolicy : Read` | filter | `ApiResponse<Paged<RoomChargePolicyResponse>>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/` | Buat room rule | `RoomChargePolicy : Create` | `CreateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `PUT` | `/{id}` | Koreksi sebelum efektif | `RoomChargePolicy : Update` | `UpdateRoomChargePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |
| `POST` | `/{id}/deactivate` | Nonaktifkan | `RoomChargePolicy : Update` | `DeactivatePolicyRequest` | `ApiResponse<RoomChargePolicyResponse>` | **Diimplementasikan (backend, belum diverifikasi manual)** |

Existing master tetap memakai tags existing `Health Services / Billing Management / Master Data / Payment Method` dan `... / Billing Item Category`; kontraknya direuse dan tidak diklaim sebagai endpoint baru.

## HTTP semantics

`200/201` sukses; `400` input tidak valid; `403` hak tidak ada; `404` resource tidak ditemukan; `409` version/state/idempotency conflict; `422` aturan bisnis tidak terpenuhi; `502/504` provider belum memberi hasil definitif dan tender tetap `PENDING`. Semua command membawa `Idempotency-Key` dan version token bila aggregate mutable. Trace: `BKC-DEC-031`–`044`, validation `BIL-VAL-*`, tests `BIL-AT-001`–`024`.

Security/privacy: response memakai data pasien minimum, field sensitif dimask, dan provider token/payload tidak pernah menjadi DTO atau custom log. Exception provider, concurrency, dan unauthorized harus mempertahankan correlation ID tanpa membocorkan payload.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-API-0.5` · status **draft** · owner API/Billing/Security · input keputusan `BKC-DEC-065`–`069` (approved) dan `BKC-DES-001`–`009` (draft) · kompatibilitas: **additive** — satu endpoint `GET` baru, dan field tambahan pada DTO kalkulasi yang sudah ada (tidak ada field yang dihapus atau berubah arti).

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{id}/insurance-invoice-document` | Menyusun lembar "Invoice Asuransi" satu invoice: identitas pasien, informasi perusahaan asuransi, rincian item yang ditanggung asuransi beserta rupiahnya, dan totalnya (`BKC-DEC-065`–`069`) | `BillingInvoice : Read` | — | `ApiResponse<InsuranceInvoiceDocumentResponse>` | **Rencana (belum tersedia)** |

**Kode status dan artinya bagi pengguna:**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Permintaan berhasil dibaca. **Tidak berarti dokumen dapat dicetak** — periksa `isPrintable` dan `warnings`. Kunjungan tunai, kunjungan penjamin perusahaan, dan tagihan tanpa item tercover semuanya menghasilkan `200` dengan penjelasan di `warnings` (`BKC-DES-008`) |
| `401` | Sesi berakhir; masuk kembali |
| `403` | Pengguna tidak punya hak akses untuk membaca invoice ini |
| `404` | Invoice tidak ditemukan atau sudah dihapus |
| `422` | Tagihan tidak dapat dihitung, misalnya ada dua aturan pajak aktif pada waktu yang sama. Pesan aslinya dari mesin kalkulasi diteruskan apa adanya — **tidak** disamarkan menjadi dokumen kosong |

**Perubahan pada response yang sudah ada** (dikembalikan `GET /{id}/calculation-preview`, `POST /{id}/recalculate`, dan `GET /{id}` lewat `calculationVersions[]`):

| DTO | Field baru | Arti |
| --- | --- | --- |
| `CalculationItemResponse` | `coveredNetAmount`, `coveredTaxAmount`, `coveredAmount`, `unresolvedAmount`, `patientAmount` | Pecahan rupiah per baris item: berapa ditanggung penjamin, berapa belum jelas, berapa jadi porsi pasien |
| `AdministrationFeeCalculationResponse` | `coveredNetAmount`, `coveredTaxAmount`, `coveredAmount` | Porsi biaya administrasi yang ditanggung penjamin |
| `RoomChargeCalculationResponse` | `coveredNetAmount`, `coveredTaxAmount`, `coveredAmount` | Porsi biaya kamar yang ditanggung penjamin |
| `CoverageCalculationResponse` | `isPerItemAllocationAvailable` | `true` bila rincian per baris tersedia. Bernilai `false` untuk versi kalkulasi yang tersimpan sebelum `BIL-CALCULATION-0.5` — consumer **MUST** memeriksa penanda ini sebelum memercayai angka per baris (`BKC-DES-004`) |
| `BillingCalculationContract.Version` | nilai berubah | `"BIL-CALCULATION-0.4"` → `"BIL-CALCULATION-0.5"`. Informasi investigasi saja; **MUST NOT** dipakai program sebagai penentu ketersediaan rincian |

Consumer lama yang tidak membaca field baru **tidak terpengaruh** — seluruh tambahan bersifat aditif dengan bawaan `0`/`false`.

**Bentuk `InsuranceInvoiceDocumentResponse`** (nilai contoh memakai data samaran):

| Field | Tipe | Keterangan |
| --- | --- | --- |
| `invoiceId` | `Guid` | Identitas invoice |
| `documentNumber` | `string` | Sama dengan `invoiceNumber` (`BKC-DES-007`) — tidak ada seri nomor tersendiri |
| `invoiceNumber`, `invoiceStatus`, `serviceType`, `invoiceDate` | `string`/`DateTimeOffset?` | Konteks tagihan |
| `payerKind` | `string` | `INSURANCE`, `CASH`, `COMPANY_GUARANTOR`, atau `UNKNOWN` |
| `isPrintable` | `bool` | `false` berarti layar **MUST** menyembunyikan tombol cetak dan menampilkan `warnings` |
| `isFromLockedSnapshot` | `bool` | `true` bila angkanya dibaca dari versi kalkulasi tersimpan (invoice `FINAL`/`CLOSED`), `false` bila dihitung segar (invoice `OPEN`) |
| `isPerItemBreakdownAvailable` | `bool` | `false` untuk snapshot lama; rincian per baris tidak dapat ditampilkan |
| `calculationVersionNo`, `calculationContractVersion`, `calculatedAt` | `int`/`string`/`DateTimeOffset` | Penelusuran lembar tercetak ke versi kalkulasi |
| `patient` | objek | `medicalRecordNumber`, `fullName`, `gender`, `ageText`, `encounterNumber`, `encounterDate`, `encounterType`, `serviceUnitName`, `roomName`, `patientClassName` |
| `payer` | objek | `insuranceProviderName`, `insuranceGroupName`, `providerType`, `claimMethod`, `contractNumber`, `officeAddress`, `policyNumber`, `memberNumber`, `planName`, `className`, `benefitPlanCode`, `effectiveStartDate`, `effectiveEndDate`, `isEligible`, `isPolicyActive` |
| `items` | daftar objek | `kind` (`ITEM`/`ADMINISTRATION_FEE`/`ROOM_CHARGE`), `invoiceItemId`, `description`, `categoryCode`, `categoryName`, `quantity`, `unitPrice`, `grossAmount`, `itemDiscount`, `netAmount`, `taxAmount`, `coveredNetAmount`, `coveredTaxAmount`, `coveredAmount`, `patientAmount`. **Hanya baris dengan `coveredAmount > 0`** (`BKC-DEC-068`) |
| `totals` | objek | `eligibleAmount`, `coveredNetAmount`, `coveredTaxAmount`, `totalCoveredAmount`, `primaryAmount`, `excessAmount`, `unresolvedCoverageAmount`, `patientAmount` |
| `warnings` | daftar teks | Pesan berbahasa Indonesia siap tampil. Kosong bila tidak ada yang perlu diberitahukan |

**Contoh angka.** Tagihan berisi Konsultasi Dokter Umum Rp 100.000 (ditanggung 100%), Fisioterapi Rp 300.000 (ditanggung 80%), Vitamin C Rp 25.000 (tidak ada aturan cocok), dan biaya administrasi Rp 15.000 (ditanggung 100%). Response memuat **tiga** baris `items` — Vitamin C tidak ikut — dengan `totals.totalCoveredAmount = 355000`, sama persis dengan `totals.primaryAmount`. Porsi pasien Rp 85.000 tercantum pada `totals.patientAmount` sebagai keterangan, bukan sebagai baris tabel.

**Yang secara sengaja TIDAK ada di response ini:** `ruleCode`, `ruleName`, `approvalInstruction`, `billingInstruction` (isi kesepakatan komersial RS–asuransi), `cardNumber` (nomor kartu asuransi), serta kontak PIC perusahaan asuransi. Lihat `02-backend-architecture.md` § Yang sengaja tidak dibuat.
