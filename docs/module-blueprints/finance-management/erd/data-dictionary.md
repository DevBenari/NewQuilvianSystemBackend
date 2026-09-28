# Kamus Data — Modul Finance Management

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Backend SHA | `09101d05` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Seluruh desain di
dokumen ini **MUST NOT** mengandalkan baris benar-benar hilang dari tabel.

Seluruh kolom uang bertipe `decimal` dengan `HasPrecision(18, 2)`. Seluruh kolom waktu
berzona bertipe `DateTimeOffset` dengan `HasColumnType("timestamp with time zone")`.

Kolom bertanda **Sensitif = Ya**: MUST NOT masuk custom logger, MUST NOT dipakai sebagai contoh
berisi data asli, dan SHOULD ditinjau kebutuhan maskingnya pada response DTO.

---

## 1. Billing Intake

### 1.1 `FinBillingHandoffIntake` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `HandoffType` | `string(30)` | Ya | — | Index gabungan | — | — | Tidak | `AR`, `AP`, `COLLECTION`, `ADJUSTMENT`, **`DEPOSIT_MOVEMENT`**, **`REFUNDABLE_CREDIT`**, **`REFUND_CASE`**, **`CASH_VARIANCE_REVIEW`** — empat nilai terakhir ditambahkan AMENDMENT REVISI 3 (`FIN-DES-029`), menuntut satu migration untuk mengubah check constraint |
| `SourceHandoffId` | `Guid` | Ya | — | Index | Id baris handoff milik Billing | — | Tidak | Tidak dijadikan FK agar tidak mengunci tabel modul lain |
| `SourceHandoffKey` | `Guid` | Ya | — | UK bersama `HandoffType` | — | — | Tidak | Kunci idempotensi milik Billing (`FIN-DES-009`) |
| `Status` | `string(30)` | Ya | `NEW` | Index | — | — | Tidak | `NEW`, `CONSUMED`, `ACKNOWLEDGED`, `ERROR` |
| `TargetEntityId` | `Guid?` | Tidak | — | — | Piutang/penerimaan yang lahir | — | Tidak | Kosong selama masih `NEW` atau `ERROR` |
| `ConsumedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu fakta berhasil diolah |
| `AcknowledgedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu ACK dikirim balik ke Billing |
| `RetryCount` | `int` | Ya | `0` | — | — | — | Tidak | Berapa kali pengolahan diulang |
| `ErrorMessage` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Terisi hanya saat `ERROR` |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Diwarisi dari handoff Billing |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency (`FIN-DES-005`) |

---

## 2. Piutang

### 2.1 `FinReceivable` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceivableNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor piutang, contoh `AR-2026-09-00871` |
| `SourceHandoffKey` | `Guid` | Ya | — | UK | — | — | Tidak | Idempotensi terhadap `BilArHandoff.HandoffKey` |
| `SourceHandoffId` | `Guid` | Ya | — | Index | Id `BilArHandoff` | — | Tidak | Rujukan, bukan FK |
| `InvoiceId` | `Guid` | Ya | — | Index | Id `BilInvoice` | — | Tidak | Rujukan tagihan asal |
| `DebtorType` | `string(30)` | Ya | — | Index | — | — | Tidak | `PAYER`, `PATIENT_GUARANTOR`, `EMPLOYEE_BENEFIT` |
| `DebtorReferenceId` | `Guid?` | Tidak | — | Index | Penjamin atau penanggung | — | Tidak | Kosong sah untuk sebagian kasus |
| `BenefitOwnerId` | `Guid?` | Tidak | — | Index | Pegawai pemilik manfaat | — | Ya | Terisi hanya untuk `EMPLOYEE_BENEFIT` (`FIN-DES-024`) |
| `BenefitRelationship` | `string(30)?` | Tidak | — | — | — | — | Ya | `SELF`, `SPOUSE`, `CHILD`, dan seterusnya |
| `OriginalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Disalin dari handoff, tidak pernah dihitung ulang |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | MUST NOT negatif; hanya ditulis `FinanceReceivableService` |
| `AllocatedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah yang sudah dilunasi penerimaan |
| `AdjustedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Bersih dari koreksi yang sudah disetujui |
| `WrittenOffAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Nilai yang dihapusbukukan |
| `DueDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Dasar perhitungan aging |
| `Status` | `string(30)` | Ya | `OUTSTANDING` | Index | — | — | Tidak | `OUTSTANDING`, `PARTIAL`, `SETTLED`, `WRITTEN_OFF`, `CANCELLED` |
| `ClaimStatus` | `string(30)` | Ya | `NOT_REQUIRED` | — | — | — | Tidak | `NOT_REQUIRED`, `INCOMPLETE`, `COMPLETE`, `SUBMITTED` — tidak mengubah nilai piutang |
| `RecognizedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu piutang diakui Finance |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Diwarisi dari Billing |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | Tindakan penyebab |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 2.2 `FinReceivableItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceivableId` | `Guid` | Ya | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | Induk piutang |
| `PatientId` | `Guid?` | Tidak | — | Index | Pasien | — | **Ya** | MUST NOT ikut ke payload Accounting |
| `EncounterId` | `Guid?` | Tidak | — | Index | Kunjungan | — | **Ya** | Rujukan kunjungan |
| `InvoiceId` | `Guid?` | Tidak | — | Index | Tagihan | — | Tidak | Rujukan tagihan per baris |
| `Description` | `string(300)` | Ya | — | — | — | — | Tidak | Keterangan yang terbaca petugas |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jumlah seluruh baris = `OriginalAmount` induk |

### 2.3 `FinReceivableDocument` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceivableId` | `Guid` | Ya | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | Induk piutang |
| `DocumentType` | `string(50)` | Ya | — | — | — | — | Tidak | Contoh `SEP`, `RESUME_MEDIS`, `SURAT_JAMINAN` |
| `DocumentNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor berkas |
| `IsReceived` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah diterima atau belum |
| `ReceivedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu berkas diterima |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan petugas |

### 2.4 `FinReceivableAdjustment` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AdjustmentNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor koreksi |
| `ReceivableId` | `Guid` | Ya | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | Induk piutang |
| `SourceHandoffAdjustmentId` | `Guid?` | Tidak | — | Index | Id `BilHandoffAdjustment` | — | Tidak | Terisi bila koreksi berasal dari Billing |
| `Direction` | `string(10)` | Ya | — | — | — | — | Tidak | `DEBIT` menambah, `CREDIT` mengurangi |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif; arah ditentukan `Direction` |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan koreksi, wajib |
| `Status` | `string(30)` | Ya | `REQUESTED` | Index | — | — | Tidak | `REQUESTED`, `APPROVED`, `REJECTED` |
| `RequestedBy` | `Guid` | Ya | — | Index | Pengguna pengaju | — | Tidak | MUST NOT sama dengan `ApprovedBy` |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu pengajuan |
| `ApprovedBy` | `Guid?` | Tidak | — | — | Pengguna penyetuju | — | Tidak | Kosong selama belum diputus |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `RejectionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila `REJECTED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 2.5 `FinReceivableWriteOff` — status `Baru`

Bentuknya sama persis dengan `FinReceivableAdjustment` kecuali tidak punya `Direction` dan
`SourceHandoffAdjustmentId`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `WriteOffNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor penghapusan |
| `ReceivableId` | `Guid` | Ya | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | Induk piutang |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nilai yang dihapusbukukan |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan, wajib |
| `Status` | `string(30)` | Ya | `REQUESTED` | Index | — | — | Tidak | `REQUESTED`, `APPROVED`, `REJECTED` |
| `RequestedBy` | `Guid` | Ya | — | Index | — | — | Tidak | MUST NOT sama dengan `ApprovedBy` |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu pengajuan |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong selama belum diputus |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `RejectionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila `REJECTED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

---

## 3. Penerimaan

### 3.1 `FinReceipt` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceiptNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor penerimaan |
| `SourceType` | `string(30)` | Ya | — | Index | — | — | Tidak | `BILLING_TENDER`, `AR_COLLECTION`, `MANUAL` |
| `SourceTenderId` | `Guid?` | Tidak | — | UK parsial | Id `BilTender` | — | Tidak | Kunci idempotensi; kosong untuk penerimaan manual (`FIN-DES-010`) |
| `SourceCollectionHandoffId` | `Guid?` | Tidak | — | Index | Id `BilCollectionHandoff` | — | Tidak | Rujukan handoff asal |
| `SettlementId` | `Guid?` | Tidak | — | Index | Id `BilSettlement` | — | Tidak | Rujukan penyelesaian Billing |
| `InvoiceId` | `Guid?` | Tidak | — | Index | Id `BilInvoice` | — | Tidak | Rujukan tagihan |
| `PaymentMethodId` | `Guid?` | Tidak | — | Index | Metode pembayaran | — | Tidak | Disalin dari tender |
| `PaymentMethodAccountId` | `Guid?` | Tidak | — | — | Rekening metode | — | Tidak | Untuk non-tunai |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Disalin apa adanya dari `BilTender.Amount` |
| `AllocatedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Yang sudah dialokasikan |
| `UnallocatedAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | `Amount − AllocatedAmount`; MUST NOT negatif |
| `KwitansiNumber` | `string(50)?` | Tidak | — | Index | — | — | Tidak | Nomor kwitansi dari Billing |
| `CashierShiftId` | `Guid?` | Tidak | — | Index | Id `BilCashierShift` | — | Tidak | Wajib untuk tunai; dasar rekonsiliasi shift |
| `ProviderReference` | `string(150)?` | Tidak | — | Index | — | — | Tidak | Nomor rujukan penyedia non-tunai |
| `ProviderEventId` | `string(100)?` | Tidak | — | — | — | — | Tidak | Identitas peristiwa penyedia |
| `OccurredAt` | `DateTimeOffset` | Ya | — | Index | — | — | Tidak | Waktu uang benar-benar diterima |
| `SourceInvoiceStatus` | `string(30)?` | Tidak | — | — | — | — | Tidak | Status tagihan saat penerimaan; penentu tahan atau kirim |
| `Status` | `string(30)` | Ya | `RECEIVED` | Index | — | — | Tidak | `RECEIVED`, `ALLOCATED`, `RECONCILED`, `REVERSED` |
| `ReversalOfReceiptId` | `Guid?` | Tidak | — | Index | FK ke `FinReceipt` | `Restrict` | Tidak | Terisi pada baris pembalik |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Diwarisi dari Billing |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | Tindakan penyebab |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 3.2 `FinReceiptAllocation` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceiptId` | `Guid` | Ya | — | Index | FK ke `FinReceipt` | `Restrict` | Tidak | Induk penerimaan |
| `ReceivableId` | `Guid?` | Tidak | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | **Kosong sah** bila uang langsung melunasi tagihan |
| `SourceAllocationId` | `Guid?` | Tidak | — | Index | Id `BilPaymentAllocation` | — | Tidak | Rujukan alokasi Billing |
| `TargetType` | `string(30)` | Ya | — | — | — | — | Tidak | `RECEIVABLE`, `INVOICE_DIRECT` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif |
| `IsReversal` | `bool` | Ya | `false` | Index | — | — | Tidak | Menandai baris pembalik |
| `ReversalOfAllocationId` | `Guid?` | Tidak | — | Index | FK ke `FinReceiptAllocation` | `Restrict` | Tidak | Terisi bila `IsReversal = true` |
| `AllocatedBy` | `Guid` | Ya | — | — | Pengguna | — | Tidak | Staf yang mengalokasikan (`FIN-DES-013`) |
| `AllocatedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu alokasi |

---

## 4. Utang dan pembayaran

### 4.1 `FinSupplierPayable` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor utang |
| `SupplierId` | `Guid` | Ya | — | Index, UK gabungan | FK ke `MstSupplier` | `Restrict` | Tidak | Milik Administrator (`FIN-DEC-014`) |
| `SupplierInvoiceNumber` | `string(100)` | Ya | — | UK bersama `SupplierId` | — | — | Tidak | Penjaga tunggal terhadap invoice dobel |
| `SupplierInvoiceDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal faktur supplier |
| `Description` | `string(300)?` | Tidak | — | — | — | — | Tidak | Keterangan |
| `OriginalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nilai faktur |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | MUST NOT negatif |
| `PaidAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Yang sudah dibayar |
| `AdjustedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Bersih dari koreksi disetujui |
| `DueDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Dasar aging utang |
| `PaymentTermDays` | `int` | Ya | `0` | — | — | — | Tidak | Disalin dari `MstSupplier` saat dibuat |
| `Status` | `string(30)` | Ya | `OUTSTANDING` | Index | — | — | Tidak | `OUTSTANDING`, `PARTIAL`, `PAID`, `CANCELLED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 4.2 `FinSupplierPayableItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableId` | `Guid` | Ya | — | Index | FK ke `FinSupplierPayable` | `Restrict` | Tidak | Induk utang |
| `Description` | `string(300)` | Ya | — | — | — | — | Tidak | Nama barang atau jasa |
| `Quantity` | `decimal(18,2)` | Ya | `1` | — | — | — | Tidak | Jumlah |
| `UnitPrice` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Harga satuan |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `Quantity × UnitPrice` |

### 4.3 `FinDoctorPayable` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor utang dokter |
| `DoctorId` | `Guid` | Ya | — | Index | Dokter | — | **Ya** | MUST NOT ikut ke payload Accounting |
| `SourceDoctorServiceFeeId` | `Guid` | Ya | — | UK | Id `DoctorServiceFee` | — | Tidak | Satu fee disetujui = satu utang |
| `SourceApHandoffId` | `Guid?` | Tidak | — | Index | Id `BilApHandoff` | — | Tidak | Rujukan kesiapan, bukan sumber nilai |
| `PeriodCode` | `string(20)` | Ya | — | Index | — | — | Tidak | Contoh `2026-08` |
| `OriginalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Disalin dari fee yang disetujui |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | MUST NOT negatif |
| `PaidAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Yang sudah dibayar |
| `AdjustedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Bersih dari koreksi disetujui |
| `Status` | `string(30)` | Ya | `OUTSTANDING` | Index | — | — | Tidak | `OUTSTANDING`, `PARTIAL`, `PAID`, `CANCELLED` |
| `RecognizedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu utang diakui |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 4.4 `FinDoctorPayableItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableId` | `Guid` | Ya | — | Index | FK ke `FinDoctorPayable` | `Restrict` | Tidak | Induk utang |
| `SourceServiceFeeDetailId` | `Guid?` | Tidak | — | Index | Rincian fee | — | Tidak | Rujukan ke Medical Fee |
| `Description` | `string(300)` | Ya | — | — | — | — | Tidak | Layanan yang menghasilkan fee |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Disalin, tidak dihitung ulang |

### 4.5 `FinPayment` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PaymentNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor pembayaran |
| `PaymentType` | `string(30)` | Ya | — | Index | — | — | Tidak | `SUPPLIER`, `DOCTOR` |
| `PayeeReferenceId` | `Guid` | Ya | — | Index | Supplier atau dokter | — | **Ya** bila dokter | Identitas penerima |
| `BankAccountId` | `Guid` | Ya | — | Index | FK ke `MstBankAccount` | `Restrict` | Tidak | Rekening sumber dana |
| `PaymentMethod` | `string(30)` | Ya | — | — | — | — | Tidak | `TRANSFER`, `CASH`, `CHEQUE` |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | MUST sama dengan jumlah alokasi sebelum `PAID` |
| `AllocatedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah seluruh alokasi |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `SUBMITTED`, `APPROVED`, `PAID`, `REJECTED`, `CANCELLED` |
| `ApprovalTier` | `string(30)?` | Tidak | — | — | — | — | Tidak | Diisi service dari total nominal (`FIN-DEC-022`); ambang masih `FIN-OQ-010` |
| `RequestedBy` | `Guid` | Ya | — | Index | — | — | Tidak | MUST NOT sama dengan `ApprovedBy` |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu pengajuan |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong selama belum disetujui |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu persetujuan |
| `PaidAt` | `DateTimeOffset?` | Tidak | — | Index | — | — | Tidak | Waktu uang benar-benar keluar |
| `ReferenceNumber` | `string(150)?` | Tidak | — | — | — | — | Tidak | Nomor bukti transfer |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 4.6 `FinPaymentAllocation` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PaymentId` | `Guid` | Ya | — | Index | FK ke `FinPayment` | `Restrict` | Tidak | Induk pembayaran |
| `PayableType` | `string(30)` | Ya | — | Index | — | — | Tidak | `SUPPLIER`, `DOCTOR` |
| `SupplierPayableId` | `Guid?` | Tidak | — | Index | FK ke `FinSupplierPayable` | `Restrict` | Tidak | Tepat satu FK terisi (`FIN-DES-016`) |
| `DoctorPayableId` | `Guid?` | Tidak | — | Index | FK ke `FinDoctorPayable` | `Restrict` | Tidak | Tepat satu FK terisi |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif |
| `IsReversal` | `bool` | Ya | `false` | Index | — | — | Tidak | Menandai baris pembalik |
| `ReversalOfAllocationId` | `Guid?` | Tidak | — | Index | FK ke dirinya sendiri | `Restrict` | Tidak | Terisi bila `IsReversal = true` |

### 4.7 `FinPayableAdjustment` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AdjustmentNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor koreksi |
| `PayableType` | `string(30)` | Ya | — | Index | — | — | Tidak | `SUPPLIER`, `DOCTOR` |
| `SupplierPayableId` | `Guid?` | Tidak | — | Index | FK ke `FinSupplierPayable` | `Restrict` | Tidak | Tepat satu FK terisi |
| `DoctorPayableId` | `Guid?` | Tidak | — | Index | FK ke `FinDoctorPayable` | `Restrict` | Tidak | Tepat satu FK terisi |
| `Direction` | `string(10)` | Ya | — | — | — | — | Tidak | `DEBIT` mengurangi utang, `CREDIT` menambah |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan, wajib |
| `Status` | `string(30)` | Ya | `REQUESTED` | Index | — | — | Tidak | `REQUESTED`, `APPROVED`, `REJECTED` |
| `RequestedBy` | `Guid` | Ya | — | Index | — | — | Tidak | MUST NOT sama dengan `ApprovedBy` |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu pengajuan |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong selama belum diputus |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

---

## 5. Kas

### 5.1 `FinBankDeposit` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DepositNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor setoran |
| `DepositDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal setoran |
| `BankAccountId` | `Guid` | Ya | — | Index | FK ke `MstBankAccount` | `Restrict` | Tidak | Rekening tujuan |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | MUST NOT melebihi saldo tersedia saat posting |
| `CashierShiftId` | `Guid?` | Tidak | — | Index | Id `BilCashierShift` | — | Tidak | Rujukan; Finance MUST NOT menulis shift |
| `DepositSlipNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor bukti setor bank |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `POSTED`, `VERIFIED`, `CANCELLED` |
| `PostedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Terisi saat `POSTED` |
| `PostedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu posting |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 5.2 `FinDailyCashSnapshot` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CashDate` | `DateOnly` | Ya | — | UK | — | — | Tidak | Satu baris per tanggal |
| `OpeningBalance` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Saldo awal = `ClosingBalance` hari sebelumnya |
| `CashReceiptAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Ringkasan `FinReceipt` tunai hari itu |
| `OtherReceiptAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Penerimaan lain yang disetujui |
| `DisbursementAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Pengeluaran kas hari itu |
| `BankDepositAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Ringkasan `FinBankDeposit` berstatus `POSTED` |
| `ClosingBalance` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Hasil formula `FIN-DEC-020` |
| `Status` | `string(30)` | Ya | `OPEN` | Index | — | — | Tidak | `OPEN`, `CLOSED` |
| `ClosedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Terisi saat ditutup |
| `ClosedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu penutupan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

---

## 6. Data induk Finance

### 6.1 `MstBank` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BankCode` | `string(20)` | Ya | — | UK | — | — | Tidak | Kode bank, contoh `BCA` |
| `BankName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama bank |
| `SwiftCode` | `string(20)?` | Tidak | — | — | — | — | Tidak | Untuk transfer luar negeri |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Bank yang sudah dipakai dinonaktifkan, bukan dihapus |

### 6.2 `MstBankAccount` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BankId` | `Guid` | Ya | — | Index, UK gabungan | FK ke `MstBank` | `Restrict` | Tidak | Bank pemilik rekening |
| `AccountNumber` | `string(50)` | Ya | — | UK bersama `BankId` | — | — | **Ya** | Nomor rekening rumah sakit |
| `AccountName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama pemilik rekening |
| `AccountType` | `string(30)` | Ya | — | Index | — | — | Tidak | `OPERATIONAL`, `COLLECTION`, `PAYMENT` |
| `CurrencyCode` | `string(3)` | Ya | `IDR` | — | — | — | Tidak | Mengacu `MstCurrency.CurrencyCode` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Nonaktifkan, jangan hapus |

### 6.3 `MstCurrency` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CurrencyCode` | `string(3)` | Ya | — | UK | — | — | Tidak | ISO 4217, contoh `IDR` |
| `CurrencyName` | `string(100)` | Ya | — | — | — | — | Tidak | Nama mata uang |
| `Symbol` | `string(10)?` | Tidak | — | — | — | — | Tidak | Contoh `Rp` |
| `DecimalPlaces` | `int` | Ya | `2` | — | — | — | Tidak | Jumlah angka di belakang koma |
| `IsBaseCurrency` | `bool` | Ya | `false` | UK parsial | — | — | Tidak | Hanya satu baris boleh `true` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Nonaktifkan, jangan hapus |

### 6.4 `MstExchangeRate` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CurrencyId` | `Guid` | Ya | — | Index, UK gabungan | FK ke `MstCurrency` | `Restrict` | Tidak | Mata uang yang dikurskan |
| `RateDate` | `DateOnly` | Ya | — | UK bersama `CurrencyId` | — | — | Tidak | Tanggal berlaku kurs |
| `BuyRate` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Kurs beli |
| `SellRate` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Kurs jual |
| `MiddleRate` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Kurs tengah |
| `Source` | `string(100)?` | Tidak | — | — | — | — | Tidak | Sumber kurs, contoh `BI` |

---

## 7. Integrasi Accounting

### 7.1 `FinAccountingEventOutbox` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EventNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Lapis anti-dobel pertama |
| `EventTypeCode` | `string(50)` | Ya | — | Index, UK gabungan | — | — | Tidak | Salah satu dari **24** kode: 17 dari `FIN-DEC-002` ditambah tujuh dari AMENDMENT REVISI 3 (`FIN-DEC-031`, `034`, `035`, `040`..`044`). **Sengaja tanpa check constraint** (`FIN-DES-030`) supaya penyesuaian nama kode hasil ratifikasi Accounting tidak menuntut migration |
| `SourceModule` | `string(30)` | Ya | `Finance` | UK gabungan | — | — | Tidak | Selalu `Finance` |
| `SourceTransactionId` | `string(50)` | Ya | — | Index, UK gabungan | — | — | Tidak | Nomor transaksi Finance |
| `SourceVersion` | `string(20)` | Ya | `1` | UK gabungan | — | — | Tidak | Dinaikkan saat koreksi, bukan dipakai ulang |
| `EventOccurredAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu kejadian bisnis sebenarnya |
| `AccountingDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal pembukuan |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nilai kejadian |
| `CurrencyCode` | `string(3)` | Ya | `IDR` | — | — | — | Tidak | Check constraint hanya menerima `IDR` |
| `LegalEntityId` | `Guid` | Ya | — | Index | Badan hukum | — | Tidak | Rujukan shared |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Rantai sampai ke tagihan Billing |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | Tindakan penyebab |
| `ComponentsJson` | `text?` | Tidak | — | — | — | — | Tidak | Daftar `ComponentCode` dan `Amount` |
| `PayloadJson` | `text` | Ya | — | — | — | — | Tidak | Salinan persis pesan; MUST NOT memuat data pasien |
| `DeliveryStatus` | `string(30)` | Ya | `PENDING` | Index | — | — | Tidak | `PENDING`, `HELD_FOR_FINALIZATION`, `SENT`, `ACKNOWLEDGED`, `HELD`, `FAILED`. **`HELD_FOR_FINALIZATION` tetap sah di database tetapi TIDAK lagi dihasilkan kode baru** sejak AMENDMENT REVISI 3 (`FIN-DEC-030`); nilainya dipertahankan agar baris warisan tidak menjadi tidak valid — **nol migration** |
| `HoldReason` | `string(300)?` | Tidak | — | — | — | — | Tidak | Alasan tertahan |
| `AttemptCount` | `int` | Ya | `0` | — | — | — | Tidak | Jumlah percobaan kirim |
| `LastAttemptAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu percobaan terakhir |
| `LastResponseCode` | `int?` | Tidak | — | — | — | — | Tidak | Kode HTTP balasan terakhir |
| `AccountingReceiptNumber` | `string(50)?` | Tidak | — | — | — | — | Tidak | Dikembalikan Accounting |
| `AccountingJournalNumber` | `string(50)?` | Tidak | — | Index | — | — | Tidak | Dikembalikan Accounting |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

### 7.2 `FinAccountingEventAttempt` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OutboxId` | `Guid` | Ya | — | Index | FK ke `FinAccountingEventOutbox` | `Restrict` | Tidak | Baris kejadian yang dicoba |
| `AttemptNumber` | `int` | Ya | — | UK bersama `OutboxId` | — | — | Tidak | Urutan percobaan |
| `AttemptedAt` | `DateTimeOffset` | Ya | — | Index | — | — | Tidak | Waktu percobaan |
| `ResponseCode` | `int?` | Tidak | — | — | — | — | Tidak | Kode HTTP; kosong bila gagal sebelum sempat terkirim |
| `ResponseBody` | `string(2000)?` | Tidak | — | — | — | — | Tidak | Dipotong agar tabel tidak menggelembung |
| `DurationMs` | `int?` | Tidak | — | — | — | — | Tidak | Lama panggilan |
| `ErrorMessage` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Pesan galat teknis |

### 7.3 `FinSubledgerPeriodBalance` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | UK gabungan | Badan hukum | — | Tidak | Nama field masih draf (`FIN-OQ-011`) |
| `AccountingPeriodCode` | `string(20)` | Ya | — | UK gabungan | — | — | Tidak | Contoh `2026-09` |
| `ControlAccountCode` | `string(50)` | Ya | — | UK gabungan | Kode COA milik Accounting | — | Tidak | Finance MUST NOT menyimpan nomor akun lengkap |
| `SubledgerBalance` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Saldo akhir subledger |
| `AsOfDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal cut-off |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `SUBMITTED`, `ACKNOWLEDGED` |
| `SubmittedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Waktu pengiriman |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

---

## 8. Tabel milik modul lain

### 8.1 `BilArHandoff` — status `Diperbarui` (milik Billing)

Kolom yang **sudah ada** terbaca langsung di
`Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs@09101d05`: `Id`,
`InvoiceId`, `FinalizationRecordId`, `DebtorType`, `DebtorReferenceId`, `Amount`, `DueDate`,
`Status`, `HandoffKey`, `CorrelationId`, `CausationId`, `CreatedAt`, `AcknowledgedAt`,
`RowVersion`.

Kolom yang **ditambahkan** desain ini:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `BenefitOwnerId` | `Guid?` | Tidak | — | Index | Pegawai pemilik manfaat | — | **Ya** | Terisi hanya untuk `EMPLOYEE_BENEFIT` |
| `BenefitRelationship` | `string(30)?` | Tidak | — | — | — | — | **Ya** | `SELF`, `SPOUSE`, `CHILD`, dan seterusnya |

Perubahan nilai enum: `BillingArDebtorTypes` bertambah `EMPLOYEE_BENEFIT`. Saat ini hanya berisi
`PATIENT_GUARANTOR` dan `PAYER` — diverifikasi langsung dari source.

**Migration ini milik tim Billing**, bukan Finance, dan menunggu konfirmasi `FIN-DEC-006`.

### 8.2 `BilCollectionHandoff` — status `Baru` (milik Billing)

Bentuk kontraknya dikunci pada `contracts/integration-contract.md` bagian 2. Kolom lengkapnya
dirancang tim Billing; Finance hanya menetapkan field apa saja yang **MUST** ada agar dapat
dikonsumsi.

### 8.3 Tabel `Sudah ada` yang hanya dirujuk

Untuk tabel berikut hanya kolom kunci yang dicatat, sesuai aturan kedalaman bertingkat. Sumber
lengkapnya ada di file model masing-masing.

| Tabel | Kolom kunci yang dipakai modul ini | File model |
|---|---|---|
| `MstSupplier` | `Id`, `SupplierCode`, `SupplierName`, `PaymentTermDays`, `CreditLimitAmount`, `BankAccountNumber`, `IsActive` | `Areas/Administrator/MasterData/Models/MstSupplier.cs` |
| `BilTender` | `Id`, `SettlementId`, `PaymentMethodId`, `PaymentMethodAccountId`, `Amount`, `Status`, `KwitansiNumber`, `CashierShiftId`, `ProviderReference`, `SettledAt`, `CorrelationId`, `CausationId` | `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs` |
| `BilSettlement` | `Id`, `InvoiceId` | `.../Billing/Models/BilSettlement.cs` |
| `BilPaymentAllocation` | `Id`, `Amount` | `.../Billing/Models/BilPaymentAllocation.cs` |
| `BilApHandoff` | `Id`, `DoctorId`, `Amount`, `ReadinessStatus`, `HandoffKey`, `Status` | `.../Billing/Models/BilApHandoff.cs` |
| `BilHandoffAdjustment` | `Id`, `ArHandoffId`, `ApHandoffId`, `Direction`, `Amount`, `Reason` | `.../Billing/Models/BilHandoffAdjustment.cs` |
| `BilCashierShift` | `Id`, `SystemCash`, `PhysicalCash`, `Variance` — **baca saja** | `.../Cashier/Models/BilCashierShift.cs` |
| `MstPettyCashCategory` | `Id`, `CategoryCode`, `IsActive` | `Areas/Corporate/FinanceManagement/MasterData/Models/MstPettyCashCategory.cs` |
| `FinPettyCashBudget` | `Id`, `PoolCode`, `CurrentBalance`, `Status` | `.../PettyCash/Models/FinPettyCashBudget.cs` |
| `FinPettyCashBudgetMovement` | `Id`, `MovementType`, `Amount`, `VoucherId` | `.../PettyCash/Models/FinPettyCashBudgetMovement.cs` |

---

## 9. Bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya
> langsung akan berbenturan dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang
> di sini.

Seluruh DDL diturunkan dari rencana file configuration pada
`Repositories/Configurations/Corporate/FinanceManagement/`, mengikuti pola nyata
`FinPettyCashBudgetMovementConfiguration.cs`.

### 9.1 Piutang

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."FinReceivable" (
    "Id"                  uuid          NOT NULL,
    "ReceivableNumber"    varchar(50)   NOT NULL,
    "SourceHandoffKey"    uuid          NOT NULL,
    "SourceHandoffId"     uuid          NOT NULL,
    "InvoiceId"           uuid          NOT NULL,
    "DebtorType"          varchar(30)   NOT NULL,
    "DebtorReferenceId"   uuid,
    "BenefitOwnerId"      uuid,                    -- SENSITIF
    "BenefitRelationship" varchar(30),             -- SENSITIF
    "OriginalAmount"      numeric(18,2) NOT NULL,
    "OutstandingAmount"   numeric(18,2) NOT NULL,
    "AllocatedAmount"     numeric(18,2) NOT NULL DEFAULT 0,
    "AdjustedAmount"      numeric(18,2) NOT NULL DEFAULT 0,
    "WrittenOffAmount"    numeric(18,2) NOT NULL DEFAULT 0,
    "DueDate"             date          NOT NULL,
    "Status"              varchar(30)   NOT NULL DEFAULT 'OUTSTANDING',
    "ClaimStatus"         varchar(30)   NOT NULL DEFAULT 'NOT_REQUIRED',
    "RecognizedAt"        timestamptz   NOT NULL,
    "CorrelationId"       uuid          NOT NULL,
    "CausationId"         uuid          NOT NULL,
    "RowVersion"          uuid          NOT NULL,

    CONSTRAINT "PK_FinReceivable" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinReceivable_DebtorType"
        CHECK ("DebtorType" IN ('PAYER','PATIENT_GUARANTOR','EMPLOYEE_BENEFIT')),
    CONSTRAINT "CK_FinReceivable_Status"
        CHECK ("Status" IN ('OUTSTANDING','PARTIAL','SETTLED','WRITTEN_OFF','CANCELLED')),
    CONSTRAINT "CK_FinReceivable_Outstanding" CHECK ("OutstandingAmount" >= 0),
    CONSTRAINT "CK_FinReceivable_Balance"
        CHECK ("OriginalAmount" = "OutstandingAmount" + "AllocatedAmount"
                                + "AdjustedAmount" + "WrittenOffAmount"),
    CONSTRAINT "CK_FinReceivable_BenefitOwner"
        CHECK (("DebtorType" = 'EMPLOYEE_BENEFIT' AND "BenefitOwnerId" IS NOT NULL)
            OR ("DebtorType" <> 'EMPLOYEE_BENEFIT' AND "BenefitOwnerId" IS NULL))
);

CREATE UNIQUE INDEX "IX_FinReceivable_ReceivableNumber"
    ON public."FinReceivable" ("ReceivableNumber") WHERE "IsDelete" = false;
CREATE UNIQUE INDEX "IX_FinReceivable_SourceHandoffKey"
    ON public."FinReceivable" ("SourceHandoffKey") WHERE "IsDelete" = false;
CREATE INDEX "IX_FinReceivable_Status_DueDate"
    ON public."FinReceivable" ("Status", "DueDate");
```

```sql
CREATE TABLE public."FinReceivableItem" (
    "Id"            uuid          NOT NULL,
    "ReceivableId"  uuid          NOT NULL,
    "PatientId"     uuid,                       -- SENSITIF
    "EncounterId"   uuid,                       -- SENSITIF
    "InvoiceId"     uuid,
    "Description"   varchar(300)  NOT NULL,
    "Amount"        numeric(18,2) NOT NULL,

    CONSTRAINT "PK_FinReceivableItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceivableItem_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."FinReceivableDocument" (
    "Id"             uuid         NOT NULL,
    "ReceivableId"   uuid         NOT NULL,
    "DocumentType"   varchar(50)  NOT NULL,
    "DocumentNumber" varchar(100),
    "IsReceived"     boolean      NOT NULL DEFAULT false,
    "ReceivedAt"     timestamptz,
    "Notes"          varchar(500),

    CONSTRAINT "PK_FinReceivableDocument" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceivableDocument_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."FinReceivableAdjustment" (
    "Id"                       uuid          NOT NULL,
    "AdjustmentNumber"         varchar(50)   NOT NULL,
    "ReceivableId"             uuid          NOT NULL,
    "SourceHandoffAdjustmentId" uuid,
    "Direction"                varchar(10)   NOT NULL,
    "Amount"                   numeric(18,2) NOT NULL,
    "Reason"                   varchar(500)  NOT NULL,
    "Status"                   varchar(30)   NOT NULL DEFAULT 'REQUESTED',
    "RequestedBy"              uuid          NOT NULL,
    "RequestedAt"              timestamptz   NOT NULL,
    "ApprovedBy"               uuid,
    "ApprovedAt"               timestamptz,
    "RejectionReason"          varchar(500),
    "RowVersion"               uuid          NOT NULL,

    CONSTRAINT "PK_FinReceivableAdjustment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceivableAdjustment_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceivableAdjustment_Direction" CHECK ("Direction" IN ('DEBIT','CREDIT')),
    CONSTRAINT "CK_FinReceivableAdjustment_Status"
        CHECK ("Status" IN ('REQUESTED','APPROVED','REJECTED')),
    CONSTRAINT "CK_FinReceivableAdjustment_Amount" CHECK ("Amount" > 0),
    -- maker-checker: pengaju tidak boleh menyetujui permohonannya sendiri (FIN-DEC-012)
    CONSTRAINT "CK_FinReceivableAdjustment_MakerChecker"
        CHECK ("ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy")
);

CREATE UNIQUE INDEX "IX_FinReceivableAdjustment_Number"
    ON public."FinReceivableAdjustment" ("AdjustmentNumber") WHERE "IsDelete" = false;
```

`FinReceivableWriteOff` memakai bentuk yang sama dengan `FinReceivableAdjustment`, tanpa kolom
`Direction` dan `SourceHandoffAdjustmentId`, dengan check constraint maker-checker yang identik.

### 9.2 Penerimaan

```sql
CREATE TABLE public."FinReceipt" (
    "Id"                       uuid          NOT NULL,
    "ReceiptNumber"            varchar(50)   NOT NULL,
    "SourceType"               varchar(30)   NOT NULL,
    "SourceTenderId"           uuid,
    "SourceCollectionHandoffId" uuid,
    "SettlementId"             uuid,
    "InvoiceId"                uuid,
    "PaymentMethodId"          uuid,
    "PaymentMethodAccountId"   uuid,
    "Amount"                   numeric(18,2) NOT NULL,
    "AllocatedAmount"          numeric(18,2) NOT NULL DEFAULT 0,
    "UnallocatedAmount"        numeric(18,2) NOT NULL,
    "KwitansiNumber"           varchar(50),
    "CashierShiftId"           uuid,
    "ProviderReference"        varchar(150),
    "ProviderEventId"          varchar(100),
    "OccurredAt"               timestamptz   NOT NULL,
    "SourceInvoiceStatus"      varchar(30),
    "Status"                   varchar(30)   NOT NULL DEFAULT 'RECEIVED',
    "ReversalOfReceiptId"      uuid,
    "CorrelationId"            uuid          NOT NULL,
    "CausationId"              uuid          NOT NULL,
    "RowVersion"               uuid          NOT NULL,

    CONSTRAINT "PK_FinReceipt" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceipt_FinReceipt_ReversalOfReceiptId"
        FOREIGN KEY ("ReversalOfReceiptId") REFERENCES public."FinReceipt" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceipt_SourceType"
        CHECK ("SourceType" IN ('BILLING_TENDER','AR_COLLECTION','MANUAL')),
    CONSTRAINT "CK_FinReceipt_Status"
        CHECK ("Status" IN ('RECEIVED','ALLOCATED','RECONCILED','REVERSED')),
    CONSTRAINT "CK_FinReceipt_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_FinReceipt_Unallocated" CHECK ("UnallocatedAmount" >= 0),
    CONSTRAINT "CK_FinReceipt_AllocationBalance"
        CHECK ("Amount" = "AllocatedAmount" + "UnallocatedAmount"),
    -- tender wajib ada untuk penerimaan yang berasal dari Billing
    CONSTRAINT "CK_FinReceipt_TenderRequired"
        CHECK ("SourceType" <> 'BILLING_TENDER' OR "SourceTenderId" IS NOT NULL)
);

-- satu tender berhasil = paling banyak satu penerimaan (FIN-DES-010)
CREATE UNIQUE INDEX "IX_FinReceipt_SourceTenderId"
    ON public."FinReceipt" ("SourceTenderId")
    WHERE "SourceTenderId" IS NOT NULL AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_FinReceipt_ReceiptNumber"
    ON public."FinReceipt" ("ReceiptNumber") WHERE "IsDelete" = false;
CREATE INDEX "IX_FinReceipt_CashierShift_OccurredAt"
    ON public."FinReceipt" ("CashierShiftId", "OccurredAt");

CREATE TABLE public."FinReceiptAllocation" (
    "Id"                     uuid          NOT NULL,
    "ReceiptId"              uuid          NOT NULL,
    "ReceivableId"           uuid,
    "SourceAllocationId"     uuid,
    "TargetType"             varchar(30)   NOT NULL,
    "Amount"                 numeric(18,2) NOT NULL,
    "IsReversal"             boolean       NOT NULL DEFAULT false,
    "ReversalOfAllocationId" uuid,
    "AllocatedBy"            uuid          NOT NULL,
    "AllocatedAt"            timestamptz   NOT NULL,

    CONSTRAINT "PK_FinReceiptAllocation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceiptAllocation_FinReceipt_ReceiptId"
        FOREIGN KEY ("ReceiptId") REFERENCES public."FinReceipt" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceiptAllocation_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceiptAllocation_Self_ReversalOfAllocationId"
        FOREIGN KEY ("ReversalOfAllocationId")
        REFERENCES public."FinReceiptAllocation" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceiptAllocation_TargetType"
        CHECK ("TargetType" IN ('RECEIVABLE','INVOICE_DIRECT')),
    CONSTRAINT "CK_FinReceiptAllocation_Amount" CHECK ("Amount" > 0),
    -- alokasi ke piutang wajib menyebut piutangnya
    CONSTRAINT "CK_FinReceiptAllocation_ReceivableRequired"
        CHECK ("TargetType" <> 'RECEIVABLE' OR "ReceivableId" IS NOT NULL),
    CONSTRAINT "CK_FinReceiptAllocation_Reversal"
        CHECK (("IsReversal" = true  AND "ReversalOfAllocationId" IS NOT NULL)
            OR ("IsReversal" = false AND "ReversalOfAllocationId" IS NULL))
);
```

### 9.3 Utang dan pembayaran

```sql
CREATE TABLE public."FinSupplierPayable" (
    "Id"                    uuid          NOT NULL,
    "PayableNumber"         varchar(50)   NOT NULL,
    "SupplierId"            uuid          NOT NULL,
    "SupplierInvoiceNumber" varchar(100)  NOT NULL,
    "SupplierInvoiceDate"   date          NOT NULL,
    "Description"           varchar(300),
    "OriginalAmount"        numeric(18,2) NOT NULL,
    "OutstandingAmount"     numeric(18,2) NOT NULL,
    "PaidAmount"            numeric(18,2) NOT NULL DEFAULT 0,
    "AdjustedAmount"        numeric(18,2) NOT NULL DEFAULT 0,
    "DueDate"               date          NOT NULL,
    "PaymentTermDays"       integer       NOT NULL DEFAULT 0,
    "Status"                varchar(30)   NOT NULL DEFAULT 'OUTSTANDING',
    "RowVersion"            uuid          NOT NULL,

    CONSTRAINT "PK_FinSupplierPayable" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinSupplierPayable_MstSupplier_SupplierId"
        FOREIGN KEY ("SupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierPayable_Status"
        CHECK ("Status" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')),
    CONSTRAINT "CK_FinSupplierPayable_Outstanding" CHECK ("OutstandingAmount" >= 0)
);

-- penjaga tunggal terhadap invoice supplier yang terinput dua kali (FIN-DEC-015)
CREATE UNIQUE INDEX "IX_FinSupplierPayable_Supplier_InvoiceNumber"
    ON public."FinSupplierPayable" ("SupplierId", "SupplierInvoiceNumber")
    WHERE "IsDelete" = false;

CREATE TABLE public."FinDoctorPayable" (
    "Id"                       uuid          NOT NULL,
    "PayableNumber"            varchar(50)   NOT NULL,
    "DoctorId"                 uuid          NOT NULL,   -- SENSITIF
    "SourceDoctorServiceFeeId" uuid          NOT NULL,
    "SourceApHandoffId"        uuid,
    "PeriodCode"               varchar(20)   NOT NULL,
    "OriginalAmount"           numeric(18,2) NOT NULL,
    "OutstandingAmount"        numeric(18,2) NOT NULL,
    "PaidAmount"               numeric(18,2) NOT NULL DEFAULT 0,
    "AdjustedAmount"           numeric(18,2) NOT NULL DEFAULT 0,
    "Status"                   varchar(30)   NOT NULL DEFAULT 'OUTSTANDING',
    "RecognizedAt"             timestamptz   NOT NULL,
    "RowVersion"               uuid          NOT NULL,

    CONSTRAINT "PK_FinDoctorPayable" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinDoctorPayable_Status"
        CHECK ("Status" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')),
    CONSTRAINT "CK_FinDoctorPayable_Outstanding" CHECK ("OutstandingAmount" >= 0)
);

-- satu fee yang sudah disetujui = paling banyak satu utang dokter
CREATE UNIQUE INDEX "IX_FinDoctorPayable_SourceFee"
    ON public."FinDoctorPayable" ("SourceDoctorServiceFeeId") WHERE "IsDelete" = false;

CREATE TABLE public."FinPayment" (
    "Id"               uuid          NOT NULL,
    "PaymentNumber"    varchar(50)   NOT NULL,
    "PaymentType"      varchar(30)   NOT NULL,
    "PayeeReferenceId" uuid          NOT NULL,   -- SENSITIF bila dokter
    "BankAccountId"    uuid          NOT NULL,
    "PaymentMethod"    varchar(30)   NOT NULL,
    "TotalAmount"      numeric(18,2) NOT NULL,
    "AllocatedAmount"  numeric(18,2) NOT NULL DEFAULT 0,
    "Status"           varchar(30)   NOT NULL DEFAULT 'DRAFT',
    "ApprovalTier"     varchar(30),
    "RequestedBy"      uuid          NOT NULL,
    "RequestedAt"      timestamptz   NOT NULL,
    "ApprovedBy"       uuid,
    "ApprovedAt"       timestamptz,
    "PaidAt"           timestamptz,
    "ReferenceNumber"  varchar(150),
    "Notes"            varchar(500),
    "RowVersion"       uuid          NOT NULL,

    CONSTRAINT "PK_FinPayment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinPayment_MstBankAccount_BankAccountId"
        FOREIGN KEY ("BankAccountId") REFERENCES public."MstBankAccount" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinPayment_PaymentType" CHECK ("PaymentType" IN ('SUPPLIER','DOCTOR')),
    CONSTRAINT "CK_FinPayment_Status"
        CHECK ("Status" IN ('DRAFT','SUBMITTED','APPROVED','PAID','REJECTED','CANCELLED')),
    CONSTRAINT "CK_FinPayment_Total" CHECK ("TotalAmount" > 0),
    CONSTRAINT "CK_FinPayment_MakerChecker"
        CHECK ("ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy"),
    -- pembayaran yang sudah dibayar wajib teralokasi penuh
    CONSTRAINT "CK_FinPayment_FullyAllocatedWhenPaid"
        CHECK ("Status" <> 'PAID' OR "AllocatedAmount" = "TotalAmount")
);

CREATE TABLE public."FinPaymentAllocation" (
    "Id"                     uuid          NOT NULL,
    "PaymentId"              uuid          NOT NULL,
    "PayableType"            varchar(30)   NOT NULL,
    "SupplierPayableId"      uuid,
    "DoctorPayableId"        uuid,
    "Amount"                 numeric(18,2) NOT NULL,
    "IsReversal"             boolean       NOT NULL DEFAULT false,
    "ReversalOfAllocationId" uuid,

    CONSTRAINT "PK_FinPaymentAllocation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinPaymentAllocation_FinPayment_PaymentId"
        FOREIGN KEY ("PaymentId") REFERENCES public."FinPayment" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinPaymentAllocation_FinSupplierPayable_SupplierPayableId"
        FOREIGN KEY ("SupplierPayableId")
        REFERENCES public."FinSupplierPayable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinPaymentAllocation_FinDoctorPayable_DoctorPayableId"
        FOREIGN KEY ("DoctorPayableId")
        REFERENCES public."FinDoctorPayable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinPaymentAllocation_Amount" CHECK ("Amount" > 0),
    -- tepat satu FK terisi dan cocok dengan PayableType (FIN-DES-016)
    CONSTRAINT "CK_FinPaymentAllocation_ExactlyOneTarget"
        CHECK (("PayableType" = 'SUPPLIER' AND "SupplierPayableId" IS NOT NULL
                                          AND "DoctorPayableId"   IS NULL)
            OR ("PayableType" = 'DOCTOR'   AND "DoctorPayableId"   IS NOT NULL
                                          AND "SupplierPayableId" IS NULL))
);
```

`FinPayableAdjustment` memakai check constraint tepat-satu-FK yang identik dengan
`FinPaymentAllocation`, ditambah constraint maker-checker seperti pada
`FinReceivableAdjustment`.

### 9.4 Kas dan data induk

```sql
CREATE TABLE public."FinBankDeposit" (
    "Id"                uuid          NOT NULL,
    "DepositNumber"     varchar(50)   NOT NULL,
    "DepositDate"       date          NOT NULL,
    "BankAccountId"     uuid          NOT NULL,
    "Amount"            numeric(18,2) NOT NULL,
    "CashierShiftId"    uuid,
    "DepositSlipNumber" varchar(100),
    "Status"            varchar(30)   NOT NULL DEFAULT 'DRAFT',
    "PostedBy"          uuid,
    "PostedAt"          timestamptz,
    "Notes"             varchar(500),
    "RowVersion"        uuid          NOT NULL,

    CONSTRAINT "PK_FinBankDeposit" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinBankDeposit_MstBankAccount_BankAccountId"
        FOREIGN KEY ("BankAccountId") REFERENCES public."MstBankAccount" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinBankDeposit_Status"
        CHECK ("Status" IN ('DRAFT','POSTED','VERIFIED','CANCELLED')),
    CONSTRAINT "CK_FinBankDeposit_Amount" CHECK ("Amount" > 0)
);

CREATE TABLE public."FinDailyCashSnapshot" (
    "Id"                 uuid          NOT NULL,
    "CashDate"           date          NOT NULL,
    "OpeningBalance"     numeric(18,2) NOT NULL DEFAULT 0,
    "CashReceiptAmount"  numeric(18,2) NOT NULL DEFAULT 0,
    "OtherReceiptAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "DisbursementAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "BankDepositAmount"  numeric(18,2) NOT NULL DEFAULT 0,
    "ClosingBalance"     numeric(18,2) NOT NULL DEFAULT 0,
    "Status"             varchar(30)   NOT NULL DEFAULT 'OPEN',
    "ClosedBy"           uuid,
    "ClosedAt"           timestamptz,
    "RowVersion"         uuid          NOT NULL,

    CONSTRAINT "PK_FinDailyCashSnapshot" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinDailyCashSnapshot_Status" CHECK ("Status" IN ('OPEN','CLOSED')),
    -- formula kas harian FIN-DEC-020; kas kecil TIDAK ikut
    CONSTRAINT "CK_FinDailyCashSnapshot_Formula"
        CHECK ("ClosingBalance" = "OpeningBalance" + "CashReceiptAmount" + "OtherReceiptAmount"
                                - "DisbursementAmount" - "BankDepositAmount")
);

CREATE UNIQUE INDEX "IX_FinDailyCashSnapshot_CashDate"
    ON public."FinDailyCashSnapshot" ("CashDate") WHERE "IsDelete" = false;

CREATE TABLE public."MstBank" (
    "Id"        uuid         NOT NULL,
    "BankCode"  varchar(20)  NOT NULL,
    "BankName"  varchar(150) NOT NULL,
    "SwiftCode" varchar(20),
    "IsActive"  boolean      NOT NULL DEFAULT true,

    CONSTRAINT "PK_MstBank" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_MstBank_BankCode"
    ON public."MstBank" ("BankCode") WHERE "IsDelete" = false;

CREATE TABLE public."MstBankAccount" (
    "Id"            uuid         NOT NULL,
    "BankId"        uuid         NOT NULL,
    "AccountNumber" varchar(50)  NOT NULL,   -- SENSITIF
    "AccountName"   varchar(150) NOT NULL,
    "AccountType"   varchar(30)  NOT NULL,
    "CurrencyCode"  varchar(3)   NOT NULL DEFAULT 'IDR',
    "IsActive"      boolean      NOT NULL DEFAULT true,

    CONSTRAINT "PK_MstBankAccount" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstBankAccount_MstBank_BankId"
        FOREIGN KEY ("BankId") REFERENCES public."MstBank" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_MstBankAccount_AccountType"
        CHECK ("AccountType" IN ('OPERATIONAL','COLLECTION','PAYMENT'))
);

CREATE UNIQUE INDEX "IX_MstBankAccount_Bank_AccountNumber"
    ON public."MstBankAccount" ("BankId", "AccountNumber") WHERE "IsDelete" = false;

CREATE TABLE public."MstCurrency" (
    "Id"             uuid         NOT NULL,
    "CurrencyCode"   varchar(3)   NOT NULL,
    "CurrencyName"   varchar(100) NOT NULL,
    "Symbol"         varchar(10),
    "DecimalPlaces"  integer      NOT NULL DEFAULT 2,
    "IsBaseCurrency" boolean      NOT NULL DEFAULT false,
    "IsActive"       boolean      NOT NULL DEFAULT true,

    CONSTRAINT "PK_MstCurrency" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_MstCurrency_CurrencyCode"
    ON public."MstCurrency" ("CurrencyCode") WHERE "IsDelete" = false;
-- hanya satu mata uang dasar
CREATE UNIQUE INDEX "IX_MstCurrency_BaseCurrency"
    ON public."MstCurrency" ("IsBaseCurrency")
    WHERE "IsBaseCurrency" = true AND "IsDelete" = false;

CREATE TABLE public."MstExchangeRate" (
    "Id"         uuid          NOT NULL,
    "CurrencyId" uuid          NOT NULL,
    "RateDate"   date          NOT NULL,
    "BuyRate"    numeric(18,2) NOT NULL,
    "SellRate"   numeric(18,2) NOT NULL,
    "MiddleRate" numeric(18,2) NOT NULL,
    "Source"     varchar(100),

    CONSTRAINT "PK_MstExchangeRate" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstExchangeRate_MstCurrency_CurrencyId"
        FOREIGN KEY ("CurrencyId") REFERENCES public."MstCurrency" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_MstExchangeRate_Currency_RateDate"
    ON public."MstExchangeRate" ("CurrencyId", "RateDate") WHERE "IsDelete" = false;
```

### 9.5 Integrasi Accounting

```sql
CREATE TABLE public."FinAccountingEventOutbox" (
    "Id"                      uuid          NOT NULL,
    "EventNumber"             varchar(50)   NOT NULL,
    "EventTypeCode"           varchar(50)   NOT NULL,
    "SourceModule"            varchar(30)   NOT NULL DEFAULT 'Finance',
    "SourceTransactionId"     varchar(50)   NOT NULL,
    "SourceVersion"           varchar(20)   NOT NULL DEFAULT '1',
    "EventOccurredAt"         timestamptz   NOT NULL,
    "AccountingDate"          date          NOT NULL,
    "Amount"                  numeric(18,2) NOT NULL,
    "CurrencyCode"            varchar(3)    NOT NULL DEFAULT 'IDR',
    "LegalEntityId"           uuid          NOT NULL,
    "CorrelationId"           uuid          NOT NULL,
    "CausationId"             uuid          NOT NULL,
    "ComponentsJson"          text,
    "PayloadJson"             text          NOT NULL,
    "DeliveryStatus"          varchar(30)   NOT NULL DEFAULT 'PENDING',
    "HoldReason"              varchar(300),
    "AttemptCount"            integer       NOT NULL DEFAULT 0,
    "LastAttemptAt"           timestamptz,
    "LastResponseCode"        integer,
    "AccountingReceiptNumber" varchar(50),
    "AccountingJournalNumber" varchar(50),
    "RowVersion"              uuid          NOT NULL,

    CONSTRAINT "PK_FinAccountingEventOutbox" PRIMARY KEY ("Id"),
    -- kontrak Accounting hanya menerima rupiah (aturan bisnis #7)
    CONSTRAINT "CK_FinAccountingEventOutbox_Currency" CHECK ("CurrencyCode" = 'IDR'),
    CONSTRAINT "CK_FinAccountingEventOutbox_DeliveryStatus"
        CHECK ("DeliveryStatus" IN ('PENDING','HELD_FOR_FINALIZATION','SENT',
                                    'ACKNOWLEDGED','HELD','FAILED'))
);

-- lapis anti-dobel pertama
CREATE UNIQUE INDEX "IX_FinAccountingEventOutbox_EventNumber"
    ON public."FinAccountingEventOutbox" ("EventNumber") WHERE "IsDelete" = false;
-- lapis anti-dobel kedua (paket kontrak Accounting bagian 5)
CREATE UNIQUE INDEX "IX_FinAccountingEventOutbox_SourceIdentity"
    ON public."FinAccountingEventOutbox"
    ("SourceModule", "SourceTransactionId", "EventTypeCode", "SourceVersion")
    WHERE "IsDelete" = false;
CREATE INDEX "IX_FinAccountingEventOutbox_DeliveryStatus"
    ON public."FinAccountingEventOutbox" ("DeliveryStatus", "AccountingDate");

CREATE TABLE public."FinAccountingEventAttempt" (
    "Id"            uuid         NOT NULL,
    "OutboxId"      uuid         NOT NULL,
    "AttemptNumber" integer      NOT NULL,
    "AttemptedAt"   timestamptz  NOT NULL,
    "ResponseCode"  integer,
    "ResponseBody"  varchar(2000),
    "DurationMs"    integer,
    "ErrorMessage"  varchar(1000),

    CONSTRAINT "PK_FinAccountingEventAttempt" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinAccountingEventAttempt_Outbox_OutboxId"
        FOREIGN KEY ("OutboxId")
        REFERENCES public."FinAccountingEventOutbox" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_FinAccountingEventAttempt_Outbox_Number"
    ON public."FinAccountingEventAttempt" ("OutboxId", "AttemptNumber");

CREATE TABLE public."FinSubledgerPeriodBalance" (
    "Id"                   uuid          NOT NULL,
    "LegalEntityId"        uuid          NOT NULL,
    "AccountingPeriodCode" varchar(20)   NOT NULL,
    "ControlAccountCode"   varchar(50)   NOT NULL,
    "SubledgerBalance"     numeric(18,2) NOT NULL,
    "AsOfDate"             date          NOT NULL,
    "Status"               varchar(30)   NOT NULL DEFAULT 'DRAFT',
    "SubmittedAt"          timestamptz,
    "RowVersion"           uuid          NOT NULL,

    CONSTRAINT "PK_FinSubledgerPeriodBalance" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinSubledgerPeriodBalance_Status"
        CHECK ("Status" IN ('DRAFT','SUBMITTED','ACKNOWLEDGED'))
);

CREATE UNIQUE INDEX "IX_FinSubledgerPeriodBalance_Identity"
    ON public."FinSubledgerPeriodBalance"
    ("LegalEntityId", "AccountingPeriodCode", "ControlAccountCode")
    WHERE "IsDelete" = false;
```

### 9.6 Billing Intake

```sql
CREATE TABLE public."FinBillingHandoffIntake" (
    "Id"               uuid          NOT NULL,
    "HandoffType"      varchar(30)   NOT NULL,
    "SourceHandoffId"  uuid          NOT NULL,
    "SourceHandoffKey" uuid          NOT NULL,
    "Status"           varchar(30)   NOT NULL DEFAULT 'NEW',
    "TargetEntityId"   uuid,
    "ConsumedAt"       timestamptz,
    "AcknowledgedAt"   timestamptz,
    "RetryCount"       integer       NOT NULL DEFAULT 0,
    "ErrorMessage"     varchar(1000),
    "CorrelationId"    uuid          NOT NULL,
    "RowVersion"       uuid          NOT NULL,

    CONSTRAINT "PK_FinBillingHandoffIntake" PRIMARY KEY ("Id"),
    -- AMENDMENT REVISI 3 (FIN-DES-029): empat nilai terakhir ditambahkan 25 September 2026
    CONSTRAINT "CK_FinBillingHandoffIntake_HandoffType"
        CHECK ("HandoffType" IN ('AR','AP','COLLECTION','ADJUSTMENT',
                                 'DEPOSIT_MOVEMENT','REFUNDABLE_CREDIT',
                                 'REFUND_CASE','CASH_VARIANCE_REVIEW')),
    CONSTRAINT "CK_FinBillingHandoffIntake_Status"
        CHECK ("Status" IN ('NEW','CONSUMED','ACKNOWLEDGED','ERROR'))
);

-- satu fakta Billing hanya boleh diolah satu kali (FIN-DES-008, FIN-DES-009)
CREATE UNIQUE INDEX "IX_FinBillingHandoffIntake_Identity"
    ON public."FinBillingHandoffIntake" ("HandoffType", "SourceHandoffKey")
    WHERE "IsDelete" = false;
CREATE INDEX "IX_FinBillingHandoffIntake_Status"
    ON public."FinBillingHandoffIntake" ("Status", "HandoffType");
```

---

# AMENDMENT REVISI 2 — Rumpun Payable

| Field | Nilai |
|---|---|
| Revisi | `2`, 20 September 2026 — status `locked` 20 September 2026 |
| Keputusan | `FIN-DES-025`..`028` |
| Menggantikan | Bagian 4.3 dan 4.4 di atas (`FinDoctorPayable`, `FinDoctorPayableItem`) — keduanya **dibatalkan** dan tidak pernah dibuat |

## A.1 `FinMedicalServicePayable` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor utang jasa |
| `PayeeType` | `string(30)` | Ya | — | Index | — | — | Tidak | `DOCTOR`, `NURSE`, `OTHER_PRACTITIONER` |
| `PayeeReferenceId` | `Guid` | Ya | — | Index | Tenaga medis penerima | — | **Ya** | Maknanya ditentukan `PayeeType` |
| `SourceMedicalServiceFeeId` | `Guid` | Ya | — | UK | Hasil jasa dari Medical Fee | — | Tidak | Satu hasil jasa disetujui = satu utang |
| `SourceApHandoffId` | `Guid?` | Tidak | — | Index | Id `BilApHandoff` | — | Tidak | Rujukan kesiapan, bukan sumber nilai |
| `PeriodCode` | `string(20)` | Ya | — | Index | — | — | Tidak | Contoh `2026-08` |
| `OriginalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jasa **kotor**, disalin dari Medical Fee (`MF-DEC-005`) |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | MUST NOT negatif |
| `PaidAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Berkurang sebesar **alokasi**, bukan sebesar transfer |
| `AdjustedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Bersih dari koreksi disetujui |
| `Status` | `string(30)` | Ya | `OUTSTANDING` | Index | — | — | Tidak | `OUTSTANDING`, `PARTIAL`, `PAID`, `CANCELLED` |
| `RecognizedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu utang diakui |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## A.2 `FinMedicalServicePayableItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayableId` | `Guid` | Ya | — | Index | FK ke `FinMedicalServicePayable` | `Restrict` | Tidak | Induk utang |
| `SourceServiceFeeDetailId` | `Guid?` | Tidak | — | Index | Rincian hasil jasa | — | Tidak | Rujukan ke Medical Fee |
| `Description` | `string(300)` | Ya | — | — | — | — | Tidak | Layanan yang menghasilkan jasa |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Disalin, tidak dihitung ulang |

## A.3 `FinPaymentDeduction` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PaymentId` | `Guid` | Ya | — | Index | FK ke `FinPayment` | `Restrict` | Tidak | Induk pembayaran |
| `DeductionType` | `string(30)` | Ya | — | Index | — | — | Tidak | `PPH21`, `KASBON`, `PATIENT_DEBT`, `SITTING_FEE`, `KSO`, `IURAN`, `OTHER` |
| `Direction` | `string(10)` | Ya | — | — | — | — | Tidak | `DEDUCTION` mengurangi, `ADDITION` menambah |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif; arah ditentukan `Direction` |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | **Wajib** bila `DeductionType = OTHER` |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor bukti, misalnya nomor kasbon |

## A.4 `FinPayment` — status `Diperbarui`

Seluruh kolom pada bagian 4.5 di atas tetap berlaku. Yang **ditambahkan**:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `DeductionAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah baris berarah `DEDUCTION` |
| `AdditionAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah baris berarah `ADDITION` |
| `NetTransferAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `TotalAmount` − `DeductionAmount` + `AdditionAmount`; MUST NOT negatif |

Nilai `PaymentType` berubah dari `SUPPLIER`/`DOCTOR` menjadi `SUPPLIER`/`MEDICAL_SERVICE`.

## A.5 `FinPaymentAllocation` dan `FinPayableAdjustment` — status `Diperbarui`

Kolom `DoctorPayableId` diganti `MedicalServicePayableId` yang menunjuk
`FinMedicalServicePayable`. Nilai `PayableType` menjadi `SUPPLIER`/`MEDICAL_SERVICE`. Aturan
tepat-satu-FK tidak berubah.

## A.6 Bentuk DDL amendment

> Peringatan yang sama dengan bagian 9 berlaku: DDL ini dokumentasi bentuk, bukan skrip yang
> dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
CREATE TABLE public."FinMedicalServicePayable" (
    "Id"                        uuid          NOT NULL,
    "PayableNumber"             varchar(50)   NOT NULL,
    "PayeeType"                 varchar(30)   NOT NULL,
    "PayeeReferenceId"          uuid          NOT NULL,   -- SENSITIF
    "SourceMedicalServiceFeeId" uuid          NOT NULL,
    "SourceApHandoffId"         uuid,
    "PeriodCode"                varchar(20)   NOT NULL,
    "OriginalAmount"            numeric(18,2) NOT NULL,
    "OutstandingAmount"         numeric(18,2) NOT NULL,
    "PaidAmount"                numeric(18,2) NOT NULL DEFAULT 0,
    "AdjustedAmount"            numeric(18,2) NOT NULL DEFAULT 0,
    "Status"                    varchar(30)   NOT NULL DEFAULT 'OUTSTANDING',
    "RecognizedAt"              timestamptz   NOT NULL,
    "RowVersion"                uuid          NOT NULL,

    CONSTRAINT "PK_FinMedicalServicePayable" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinMedicalServicePayable_PayeeType"
        CHECK ("PayeeType" IN ('DOCTOR','NURSE','OTHER_PRACTITIONER')),
    CONSTRAINT "CK_FinMedicalServicePayable_Status"
        CHECK ("Status" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')),
    CONSTRAINT "CK_FinMedicalServicePayable_Outstanding" CHECK ("OutstandingAmount" >= 0)
);

-- satu hasil jasa yang sudah disetujui = paling banyak satu utang
CREATE UNIQUE INDEX "IX_FinMedicalServicePayable_SourceFee"
    ON public."FinMedicalServicePayable" ("SourceMedicalServiceFeeId") WHERE "IsDelete" = false;
CREATE INDEX "IX_FinMedicalServicePayable_Payee_Period"
    ON public."FinMedicalServicePayable" ("PayeeType", "PayeeReferenceId", "PeriodCode");

CREATE TABLE public."FinPaymentDeduction" (
    "Id"              uuid          NOT NULL,
    "PaymentId"       uuid          NOT NULL,
    "DeductionType"   varchar(30)   NOT NULL,
    "Direction"       varchar(10)   NOT NULL,
    "Amount"          numeric(18,2) NOT NULL,
    "Reason"          varchar(500),
    "ReferenceNumber" varchar(100),

    CONSTRAINT "PK_FinPaymentDeduction" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinPaymentDeduction_FinPayment_PaymentId"
        FOREIGN KEY ("PaymentId") REFERENCES public."FinPayment" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinPaymentDeduction_Type"
        CHECK ("DeductionType" IN ('PPH21','KASBON','PATIENT_DEBT','SITTING_FEE',
                                   'KSO','IURAN','OTHER')),
    CONSTRAINT "CK_FinPaymentDeduction_Direction"
        CHECK ("Direction" IN ('DEDUCTION','ADDITION')),
    CONSTRAINT "CK_FinPaymentDeduction_Amount" CHECK ("Amount" > 0),
    -- pos lain-lain wajib menyebut alasannya
    CONSTRAINT "CK_FinPaymentDeduction_OtherReason"
        CHECK ("DeductionType" <> 'OTHER' OR "Reason" IS NOT NULL)
);

CREATE INDEX "IX_FinPaymentDeduction_Payment"
    ON public."FinPaymentDeduction" ("PaymentId", "DeductionType");

-- kolom tambahan pada FinPayment
ALTER TABLE public."FinPayment"
    ADD COLUMN "DeductionAmount"   numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN "AdditionAmount"    numeric(18,2) NOT NULL DEFAULT 0,
    ADD COLUMN "NetTransferAmount" numeric(18,2) NOT NULL DEFAULT 0;

ALTER TABLE public."FinPayment"
    ADD CONSTRAINT "CK_FinPayment_NetTransfer"
        CHECK ("NetTransferAmount" = "TotalAmount" - "DeductionAmount" + "AdditionAmount"),
    ADD CONSTRAINT "CK_FinPayment_NetTransferNonNegative"
        CHECK ("NetTransferAmount" >= 0);
```

`ALTER TABLE` di atas ditulis demikian hanya untuk memperjelas apa yang berubah. Karena
migration `AddFinancePayable` **belum pernah dijalankan**, ketiga kolom itu sebenarnya masuk
sebagai bagian dari `CREATE TABLE "FinPayment"` yang asli — bukan sebagai migration tambahan.
