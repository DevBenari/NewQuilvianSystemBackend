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

---

## Amendment 4 September 2026 — Pecahan tanggungan per baris, anomali data, dan gerbang PPN

`last_changed_in: BIL-API-0.6` · status **draft** · owner API/Billing/Security · `approved_by`/`approved_at`: belum ada. Input: `BKC-DEC-069`–`079`, keputusan arsitektur `BKC-DES-010`–`020`. Dampak kompatibilitas: **additive** — tidak ada endpoint baru, tidak ada field yang dihapus, tidak ada nilai enum yang berubah arti. Satu field berubah **makna dokumentasinya** (`unresolvedCoverageAmount`) tanpa berubah nama maupun tipe.

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing-invoices`

**Tidak ada endpoint baru pada amendment ini.** Seluruh perubahan terbawa oleh dua endpoint yang sudah ada.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{id:guid}/calculation-preview` | Menghitung ulang tagihan tanpa menyimpannya, untuk ditampilkan di Menu Pembayaran | `BillingInvoice : Read` | Path `id` | `ApiResponse<CalculationResponse>` | Diimplementasikan — **payload bertambah field** |
| `POST` | `/{id:guid}/recalculate` | Menghitung ulang tagihan dan menyimpannya sebagai versi kalkulasi baru | `BillingInvoice : Update` | Path `id` + body `RecalculateInvoiceRequest` | `ApiResponse<CalculationResponse>` | Diimplementasikan — **payload bertambah field** |
| `GET` | `/{id:guid}/insurance-invoice-document` | Menyusun lembar Invoice Asuransi untuk dicetak | `BillingInvoice : Read` | Path `id` | `ApiResponse<InsuranceInvoiceDocumentResponse>` | **Rencana (belum tersedia)** — dari amendment 3 September 2026 |

### Field yang bertambah pada `breakdown.items[]`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `itemPrimaryAmount` | angka | Rupiah pokok item ini yang ditanggung penjamin, sebelum pajak | Sudah ada di working tree (`BE-BKC-FIX-003`), belum di-commit |
| `itemUnresolvedAmount` | angka | Residual pokok yang menurut kontrak penjamin tidak boleh ditagihkan ke pasien | Sudah ada di working tree — **makna dipersempit** oleh `BKC-DES-013` |
| `taxPrimaryAmount` | angka | Rupiah pajak item ini yang ditanggung penjamin | Sudah ada di working tree |
| `taxUnresolvedAmount` | angka | Residual pajak yang tidak boleh ditagihkan ke pasien | Sudah ada di working tree — makna dipersempit |
| `itemDataAnomalyAmount` | angka | Rupiah pokok item ini yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah | **Rencana (belum tersedia)** |
| `taxDataAnomalyAmount` | angka | Rupiah pajak item ini yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah | **Rencana (belum tersedia)** |

Porsi pasien per baris **tidak** dikirim sebagai field tersendiri; ia diturunkan konsumen: `porsi pasien = (grossAmount − itemDiscount + taxAmount) − itemPrimaryAmount − taxPrimaryAmount − itemUnresolvedAmount − taxUnresolvedAmount − itemDataAnomalyAmount − taxDataAnomalyAmount`. Identitas ini selalu benar menurut cara nilainya dibentuk (`BKC-DES-015`).

### Field yang bertambah pada `breakdown.administrationFee` dan `breakdown.roomCharge`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `primaryAmount` | angka | Rupiah komponen ini yang ditanggung penjamin | Sudah ada di working tree |
| `unresolvedAmount` | angka | Residual yang tidak boleh ditagihkan ke pasien | Sudah ada di working tree — makna dipersempit |
| `dataAnomalyAmount` | angka | Rupiah yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah | **Rencana (belum tersedia)** |

### Field yang bertambah pada `breakdown.coverage`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `dataAnomalyAmount` | angka | Total rupiah yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah | **Rencana (belum tersedia)** |
| `hasDataAnomaly` | boolean | Menyatakan ada masalah data penjamin pada kunjungan ini. Disediakan terpisah dari `dataAnomalyAmount > 0` karena masalah data dapat terjadi pada tagihan yang nominalnya masih nol | **Rencana (belum tersedia)** |
| `anomalyCodes` | daftar teks | Kode program, tidak diterjemahkan: `PAYER_NOT_ELIGIBLE`, `POLICY_INACTIVE`, `INSURANCE_PROVIDER_MISSING`, `ENCOUNTER_NOT_FOUND` | **Rencana (belum tersedia)** |
| `anomalyMessages` | daftar teks | Kalimat berbahasa Indonesia siap tampil, sejajar indeksnya dengan `anomalyCodes` | **Rencana (belum tersedia)** |
| `isPerItemAllocationAvailable` | boolean | Menyatakan rincian per baris pada payload ini boleh dipercaya. Bernilai `false` pada snapshot yang ditulis sebelum pembaruan ini | **Rencana (belum tersedia)** — `BKC-DES-017` |

### Field yang maknanya berubah tanpa berubah nama

| Field | Makna lama | Makna baru | Dasar |
| --- | --- | --- | --- |
| `coverage.unresolvedAmount` | Campuran dari lima keadaan: tidak ada aturan cocok, `NotCovered`, menunggu approval, penjamin tidak layak, dan residual yang tidak boleh ditagihkan | **Hanya satu keadaan**: residual yang menurut kontrak penjamin tidak boleh ditagihkan ke pasien (`IsAllowExcessPaymentByPatient = false`) | `BKC-DES-013` |
| `coverage.excessAmount`, `coverage.excessStatus` | Cadangan untuk penjamin kedua | Dipertahankan sebagai field yang **permanen** bernilai `0` dan `"NOT_CONFIGURED"`, dan dihapus dari tampilan | `BKC-DES-014` |

Perubahan makna ini **MUST** disosialisasikan ke konsumen sebelum deploy. Field yang berganti arti tanpa berganti nama adalah bentuk perubahan yang paling sulit ditemukan konsumen, karena kodenya tetap berjalan dan angkanya saja yang berbeda.

### Field yang tidak jadi ditambahkan

Amendment 3 September 2026 merancang `coveredNetAmount`, `coveredTaxAmount`, `coveredAmount`, dan `patientAmount` sebagai field per baris. Keempatnya **tidak jadi ditambahkan** (`BKC-DES-015`): seluruhnya dapat dihitung konsumen dari field di atas, dan menyimpan angka turunan berarti dua sumber untuk satu angka.

### Kode status

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Perhitungan berhasil. **Termasuk** ketika ada anomali data penjamin — anomali bukan kegagalan permintaan |
| `409` | Data tagihan berubah pihak lain, atau lebih dari satu aturan pajak aktif bersamaan |
| `422` | Perhitungan melanggar batas yang dijaga (`BIL-VAL-035`–`037`) |

---

## Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off

`last_changed_in: BIL-API-0.7` · status **draft** · owner Backend/API + Product/Billing/Finance · `approved_by`/`approved_at`: belum ada. Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`. Dampak kompatibilitas: **additive** — **tidak ada endpoint baru**, tidak ada field yang dihapus atau berganti nama, dan field request yang bertambah bersifat opsional berbawaan sehingga konsumen yang sudah ada tidak rusak.

### `[Tags("Health Services / Billing Management / Billing / Financial Exceptions")]`

Base URL: `api/v1/health-services/billing-management/billing/financial-exceptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/write-offs` | Ajukan write-off — **bertambah** field `category` | `BillingWriteOff : Create` | `CreateWriteOffRequest` (+`category`) | `ApiResponse<WriteOffResponse>` (+`category`) | **Diimplementasikan; field `category` Rencana (belum tersedia)** |
| `POST` | `/write-offs/{id}/approve` | Setujui dan posting write-off — penjaga bercabang kategori | `BillingWriteOff : Approve` | `WriteOffApprovalRequest` (tidak berubah) | `ApiResponse<WriteOffResponse>` (+`category`) | **Diimplementasikan; perilaku per kategori Rencana (belum tersedia)** |
| `GET` | `/write-offs/{id}` | Ambil satu kasus write-off | `BillingWriteOff : Read` | — | `ApiResponse<WriteOffResponse>` (+`category`) | **Diimplementasikan; field `category` Rencana (belum tersedia)** |
| `GET` | `/invoices/{invoiceId}` | Daftar pengecualian finansial satu tagihan — **bertambah** sisa residual | `BillingFinancialException : Read` | — | `ApiResponse<…>` (+`nonBillableResidualRemaining`) | **Diimplementasikan; field baru Rencana (belum tersedia)** |
| `POST` | `/{type}/{id}/reverse` | Reversal write-off — perilaku bercabang kategori | `BillingFinancialException : Reverse` | `ReversalRequest` (tidak berubah) | `ApiResponse<AdjustmentResponse>` | **Diimplementasikan; perilaku per kategori Rencana (belum tersedia)** |

**Tidak ada endpoint baru pada amendment ini.** Pengajuan write-off atas selisih yang tidak dapat ditagihkan memakai endpoint yang sama dengan write-off piutang pasien, dengan alur maker-checker yang sama. Endpoint kedua berarti dua alur persetujuan yang harus dijaga tetap identik selamanya.

### Field yang bertambah pada `CreateWriteOffRequest`

| Field | Tipe | Wajib | Bawaan | Batas | Arti | Status |
| --- | --- | :---: | --- | --- | --- | --- |
| `category` | teks | Tidak | `"PATIENT_AR"` | Maksimal 30 karakter; hanya `"PATIENT_AR"` atau `"NON_BILLABLE_RESIDUAL"` | Menyatakan uang apa yang sedang ditulis-off: piutang pasien, atau selisih yang menurut kontrak penjamin tidak dapat ditagihkan kepada siapa pun | **Rencana (belum tersedia)** — `BKC-DES-024` |

Teks di luar dua nilai itu ditolak `422` (`BIL-VAL-042`), **bukan** diam-diam diperlakukan sebagai nilai bawaan. Field ini **MUST** ikut masuk perhitungan `PayloadHash` idempotency; tanpa itu, dua pengajuan bernominal sama dengan kategori berbeda akan dianggap pengulangan permintaan yang sama.

### Field yang bertambah pada `WriteOffResponse`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `category` | teks | `"PATIENT_AR"` atau `"NON_BILLABLE_RESIDUAL"`; selalu terisi, termasuk pada kasus lama yang seluruhnya `"PATIENT_AR"` | **Rencana (belum tersedia)** |

### Field yang bertambah pada `GET /invoices/{invoiceId}`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `nonBillableResidualRemaining` | angka | Sisa selisih yang tidak dapat ditagihkan pada tagihan ini yang **belum** ditutup write-off. Sudah memperhitungkan kasus yang direversal, dan sudah dijepit tidak negatif | **Rencana (belum tersedia)** — `BKC-DES-025` |

Angka ini dihitung server dan **MUST NOT** dihitung ulang di layar dari daftar kasus. Perhitungan uang di sisi layar akan menyimpang dari server begitu ada satu kasus yang tidak ikut terkirim.

### `[Tags("Health Services / Billing Management / Billing / Invoices")]`

Base URL: `api/v1/health-services/billing-management/billing/invoices`

Tidak ada endpoint baru. Field di bawah terbawa oleh `GET /{id}/calculation-preview` dan `GET /{id}/calculations/{versionNo}` yang sudah ada.

#### Field yang bertambah pada `breakdown.coverage`

| Field | Tipe | Arti | Status |
| --- | --- | --- | --- |
| `nonBillableResidualAmount` | angka | Total rupiah sisa perhitungan tanggungan yang menurut kontrak penjamin **tidak boleh ditagihkan ke pasien**, sehingga menjadi tanggungan rumah sakit lewat write-off | **Rencana (belum tersedia)** — `BKC-DES-021` |
| `hasNonBillableResidual` | boolean | Menyatakan tagihan ini memuat selisih semacam itu pada versi kalkulasi terkini, terlepas dari sudah atau belum ditulis-off | **Rencana (belum tersedia)** |

#### Field yang bertambah pada `breakdown.items[]`, `breakdown.administrationFee`, dan `breakdown.roomCharge`

| Field | Tempat | Tipe | Arti | Status |
| --- | --- | --- | --- | --- |
| `itemNonBillableResidualAmount` | `items[]` | angka | Porsi selisih tidak dapat ditagihkan milik baris itu | **Rencana (belum tersedia)** |
| `taxNonBillableResidualAmount` | `items[]` | angka | Porsi selisih tidak dapat ditagihkan milik pajak baris itu | **Rencana (belum tersedia)** |
| `nonBillableResidualAmount` | `administrationFee`, `roomCharge` | angka | Porsi selisih tidak dapat ditagihkan milik komponen itu | **Rencana (belum tersedia)** |

Rincian per baris ini yang memungkinkan alasan write-off menyebut item mana yang menyumbang selisih. Tanpa rincian, Finance hanya melihat satu angka gabungan dan tidak dapat menulis alasan yang berarti bagi auditor.

### Field yang maknanya berubah tanpa berubah nama

| Field | Makna pada revisi `0.7` | Makna pada revisi `0.8` | Dasar |
| --- | --- | --- | --- |
| `coverage.unresolvedAmount` | Residual yang menurut kontrak penjamin tidak boleh ditagihkan ke pasien — mencakup jalur aturan `NotCovered` **dan** jalur sisa perhitungan | **Menyisakan satu jalur**: aturan `NotCovered` dengan `IsAllowExcessPaymentByPatient = false`. Sisa perhitungan pindah ke `nonBillableResidualAmount` | **`BKC-DEC-080`**, `BKC-DES-021` |

Perubahan ini **MUST** disosialisasikan ke konsumen sebelum deploy, dengan alasan yang sama seperti amendment sebelumnya: field yang berganti arti tanpa berganti nama adalah bentuk perubahan yang paling sulit ditemukan konsumen.

**Panduan tampilan yang menyertainya.** Layar kasir **MUST** tetap menampilkan **satu** baris "Selisih Tidak Ditagihkan (kontrak penjamin)" berisi `unresolvedAmount + nonBillableResidualAmount`. Kasir tidak berkepentingan membedakan sebabnya; pemisahannya berguna di layar Pengecualian Finansial. Karena kedua field selalu dijumlah di layar itu, angka yang dilihat kasir **tidak berubah sama sekali** oleh amendment ini.

### Kode status

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Pengajuan/persetujuan/reversal berhasil, atau perhitungan berhasil |
| `201` | Kasus write-off berhasil diajukan |
| `403` | Pengguna tidak berwenang mengajukan atau menyetujui write-off |
| `409` | Data tagihan berubah pihak lain, atau kasus sudah pernah direversal |
| `422` | Melanggar batas yang dijaga (`BIL-VAL-040`–`043`), atau pengaju menyetujui pengajuannya sendiri (`BIL-VAL-017`) |

Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`. Test mapping `BIL-AT-055`–`061`.
| `404` | Tagihan atau kunjungan tidak ditemukan |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |

Trace `BKC-DEC-069`–`079`, `BKC-DES-010`–`020`. Tests `BIL-AT-036`–`048`.
