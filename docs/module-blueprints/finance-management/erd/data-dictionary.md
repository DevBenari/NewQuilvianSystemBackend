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

# AMENDMENT REVISI 4 — Purchasing/AP, AR Invoice Agregat, Potongan AR

| Field | Nilai |
|---|---|
| Revisi | `4`, 25 September 2026 — status `draft` |
| Keputusan | `FIN-DEC-045`..`055`, `FIN-DES-037`..`044` |
| Rincian arsitektur | `02-backend-architecture.md` bagian AMENDMENT REVISI 4 |

## C.1 `FinPurchaseOrder` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PONumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Contoh `PO-2026-09-00142` |
| `SupplierId` | `Guid` | Ya | — | Index | FK ke `MstSupplier` | `Restrict` | Tidak | Supplier tujuan |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `PENDING_APPROVAL`, `APPROVED`, `REJECTED`, `CANCELLED`, `PARTIALLY_RECEIVED`, `FULLY_RECEIVED`, `CLOSED` |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jumlah seluruh baris |
| `ApprovalTier` | `string(10)` | Ya | — | — | — | — | Tidak | `TIER_1`, `TIER_2` — dihitung `FinanceApprovalTierResolver` (`FIN-DES-039`): `< 50.000.000 → TIER_1`, `>= 50.000.000 → TIER_2` |
| `RequestedByUserId` | `Guid` | Ya | — | Index | Pengaju | — | Tidak | — |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | — |
| `ApprovedByUserId` | `Guid?` | Tidak | — | — | Penyetuju | — | Tidak | Kosong sampai disetujui |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.2 `FinPurchaseOrderItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PurchaseOrderId` | `Guid` | Ya | — | Index | FK ke `FinPurchaseOrder` | `Restrict` | Tidak | Induk PO |
| `ProductCategory` | `string(100)` | Ya | — | — | — | — | Tidak | Teks bebas — tidak ada master produk (C.11) |
| `ProductName` | `string(300)` | Ya | — | — | — | — | Tidak | — |
| `Unit` | `string(30)` | Ya | — | — | — | — | Tidak | Contoh `PCS`, `BOX` |
| `Quantity` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `UnitPrice` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `LineTotal` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `Quantity * UnitPrice` |

## C.3 `FinGoodsReceipt` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `GRNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Contoh `GR-2026-09-00142` |
| `PurchaseOrderId` | `Guid` | Ya | — | Index | FK ke `FinPurchaseOrder` | `Restrict` | Tidak | Wajib terisi — tidak ada GR tanpa PO (C.11) |
| `ReceivedDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | — |
| `Status` | `string(20)` | Ya | `RECEIVED` | — | — | — | Tidak | `RECEIVED`, `CANCELLED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.4 `FinGoodsReceiptItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `GoodsReceiptId` | `Guid` | Ya | — | Index | FK ke `FinGoodsReceipt` | `Restrict` | Tidak | Induk GR |
| `PurchaseOrderItemId` | `Guid` | Ya | — | Index | FK ke `FinPurchaseOrderItem` | `Restrict` | Tidak | Baris PO yang diterima |
| `ReceivedQuantity` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Boleh kurang dari `Quantity` PO (penerimaan sebagian) |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Kondisi barang |

## C.5 `FinInvoiceExchange` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ExchangeNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Contoh `TF-2026-09-00142` |
| `SupplierId` | `Guid` | Ya | — | Index | FK ke `MstSupplier` | `Restrict` | Tidak | — |
| `PurchaseOrderId` | `Guid?` | Tidak | — | Index | FK ke `FinPurchaseOrder` | `SetNull` | Tidak | Boleh kosong (`FIN-DEC-051`) |
| `GoodsReceiptId` | `Guid?` | Tidak | — | Index | FK ke `FinGoodsReceipt` | `SetNull` | Tidak | Boleh kosong |
| `SupplierInvoiceNumber` | `string(100)` | Ya | — | — | — | — | Tidak | Nomor faktur fisik dari supplier |
| `SupplierInvoiceDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `ReceivedDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal dokumen diterima RS |
| `EstimatedDueDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | `ReceivedDate + MstSupplier.PaymentTermDays`, dihitung sistem |
| `Status` | `string(20)` | Ya | `RECEIVED` | Index | — | — | Tidak | `RECEIVED`, `LINKED_TO_INVOICE`, `CANCELLED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.6 `FinPurchasingInvoice` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `InvoiceNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor internal |
| `InvoiceExchangeId` | `Guid` | Ya | — | UK | FK ke `FinInvoiceExchange` | `Restrict` | Tidak | Tepat satu Tukar Faktur = tepat satu Purchasing Invoice (`FIN-DEC-051`) |
| `SupplierId` | `Guid` | Ya | — | Index | FK ke `MstSupplier` | `Restrict` | Tidak | — |
| `SubtotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nilai barang/jasa sebelum pajak dan potongan |
| `DiscountAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | — |
| `PPNAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Pajak Masukan (`FIN-DES-043`) |
| `DownPaymentAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | DP yang mengurangi nilai jatuh tempo |
| `OtherDeductionAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Potongan lain di luar PPN/DP |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `Subtotal - Discount + PPN - DownPayment - OtherDeduction` |
| `Status` | `string(30)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `PENDING_APPROVAL`, `APPROVED`, `REJECTED`, `CANCELLED` |
| `ApprovalTier` | `string(10)` | Ya | — | — | — | — | Tidak | `TIER_1`, `TIER_2` (`FIN-DES-039`) |
| `RequestedByUserId` | `Guid` | Ya | — | Index | Pengaju | — | Tidak | — |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | — |
| `ApprovedByUserId` | `Guid?` | Tidak | — | — | Penyetuju | — | Tidak | — |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.7 `FinPurchasingInvoiceItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PurchasingInvoiceId` | `Guid` | Ya | — | Index | FK ke `FinPurchasingInvoice` | `Restrict` | Tidak | Induk invoice |
| `ProductName` | `string(300)` | Ya | — | — | — | — | Tidak | — |
| `Quantity` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `UnitPrice` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `LineTotal` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |

## C.8 `FinSupplierReturn` — status `Diperbarui` (AMENDMENT REVISI 6)

Status naik dari `Baru` menjadi `Diperbarui`: tabelnya **sudah dibangun dan sudah berjalan** pada
`cba60cb0`, dan AMENDMENT REVISI 6 menambahkan satu kolom (`FIN-DES-055`).

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReturnNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | — |
| `PurchasingInvoiceId` | `Guid` | Ya | — | Index | FK ke `FinPurchasingInvoice` | `Restrict` | Tidak | Invoice sumber |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | — |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | **Pokok tanpa PPN** — jumlah `LineTotal` seluruh barisnya. Arti ini diperjelas REVISI 6; nilainya tidak berubah |
| `PPNAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | **Baru (REVISI 6).** Porsi PPN barang yang diretur. `>= 0` lewat `CK_FinSupplierReturn_PPNAmount`. Mengikuti nama `FinPurchasingInvoice.PPNAmount`. Kredit retur yang lahir = `TotalAmount + PPNAmount` |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `CONFIRMED`, `CANCELLED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

**Kenapa kolom ini ada.** Accounting meratifikasi `RETUR-PEMBELIAN` dengan syarat nilainya pokok
tanpa PPN dan porsi PPN dikirim lewat kode terpisah (`integration-contract.md` bagian 5.10).
Tanpa kolom ini, kredit retur tercatat hanya sebesar pokok padahal supplier mengakui pokok + PPN,
sehingga selisihnya tidak pernah dapat dipakai mengurangi utang dan terbaca sebagai utang yang
masih harus dibayar.

## C.9 `FinSupplierReturnItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SupplierReturnId` | `Guid` | Ya | — | Index | FK ke `FinSupplierReturn` | `Restrict` | Tidak | Induk retur |
| `Description` | `string(300)` | Ya | — | — | — | — | Tidak | — |
| `Quantity` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `LineTotal` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |

## C.10 `FinSupplierReturnDeposit` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SupplierId` | `Guid` | Ya | — | Index | FK ke `MstSupplier` | `Restrict` | Tidak | Dapat dipakai lintas invoice supplier yang sama (`FIN-DEC-047`) |
| `SourceReturnId` | `Guid` | Ya | — | UK | FK ke `FinSupplierReturn` | `Restrict` | Tidak | Satu retur = satu deposit |
| `OriginalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `AvailableAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | MUST NOT negatif; berkurang lewat `FinSupplierReturnDepositUsage` |
| `Status` | `string(20)` | Ya | `AVAILABLE` | Index | — | — | Tidak | `AVAILABLE`, `EXHAUSTED`, `CANCELLED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.11 `FinSupplierReturnDepositUsage` — status `Baru` — **DIGANTIKAN bagian D.2 (REVISI 5)**

> **Jangan dibangun dari bentuk ini.** `PurchasingInvoiceId` diganti `PaymentId`, ditambah
> `Status`, `ReleasedAt`, `RowVersion` (`FIN-DES-045`, `046`). Tabel di bawah dipertahankan
> sebagai jejak.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SupplierReturnDepositId` | `Guid` | Ya | — | Index | FK ke `FinSupplierReturnDeposit` | `Restrict` | Tidak | Deposit yang dipakai |
| `PurchasingInvoiceId` | `Guid` | Ya | — | Index | FK ke `FinPurchasingInvoice` | `Restrict` | Tidak | Invoice tujuan pemakaian |
| `UsedAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | MUST NOT melebihi `AvailableAmount` saat ditulis |
| `UsedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | — |

## C.12 `FinSupplierPayable` — status `Diperbarui`

Seluruh kolom yang sudah ada tetap berlaku. Yang **ditambahkan**:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `SourcePurchasingInvoiceId` | `Guid?` | Tidak | — | Index | FK ke `FinPurchasingInvoice` | `SetNull` | Tidak | `NULL` untuk baris lama/input manual (`FIN-DES-040`); terisi otomatis saat dibuat dari Purchasing Invoice `Approved` |

## C.13 `FinReceivableInvoiceBatch` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BatchNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Nomor seri resmi, terpisah dari `BilInvoice.InvoiceNumber` (`FIN-DES-041`) |
| `DebtorType` | `string(30)` | Ya | `PAYER` | Index | — | — | Tidak | Tetap `PAYER` pada rilis ini (`FIN-DEC-048`) |
| `DebtorReferenceId` | `Guid` | Ya | — | Index | Penjamin/perusahaan | — | Tidak | Kunci pengelompokan, sama dengan `FinReceivable.DebtorReferenceId` |
| `PeriodStart` | `DateOnly` | Ya | — | Index | — | — | Tidak | — |
| `PeriodEnd` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jumlah `OriginalAmount` seluruh `FinReceivable` anggota |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `ISSUED`, `PARTIALLY_PAID`, `PAID`, `CANCELLED` |
| `IssuedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Terisi saat `ISSUED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

## C.14 `FinReceivableInvoiceBatchItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BatchId` | `Guid` | Ya | — | Index | FK ke `FinReceivableInvoiceBatch` | `Restrict` | Tidak | Induk batch |
| `ReceivableId` | `Guid` | Ya | — | UK parsial | FK ke `FinReceivable` | `Restrict` | Tidak | Satu `FinReceivable` hanya boleh berada di satu batch yang belum `CANCELLED` (unique index parsial, lihat DDL) |

## C.15 `FinReceiptDeduction` — status `Baru` — **DIGANTIKAN bagian D.3 (REVISI 5)**

> **Jangan dibangun dari bentuk ini.** Ditambah `DeductionNumber`, `ReceiptAllocationId`,
> `IsReversal`, `ReversalOfDeductionId` (`FIN-DES-048`, `049`). Tabel di bawah dipertahankan
> sebagai jejak.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceiptId` | `Guid` | Ya | — | Index | FK ke `FinReceipt` | `Restrict` | Tidak | Induk penerimaan |
| `DeductionType` | `string(30)` | Ya | — | Index | — | — | Tidak | `PPH23`, `BANK_ADMIN_FEE`, `OTHER` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif; mengurangi `FinReceivable.OutstandingAmount` lewat `AllocatedAmount` (`FIN-DEC-055`) |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | **Wajib** bila `DeductionType = OTHER` |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor bukti potong/bukti setor bank |

## C.16 Bentuk DDL amendment

> Peringatan yang sama dengan bagian 9 berlaku: DDL ini dokumentasi bentuk, bukan skrip yang
> dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
CREATE TABLE public."FinPurchaseOrder" (
    "Id"                uuid          NOT NULL,
    "PONumber"          varchar(50)   NOT NULL,
    "SupplierId"        uuid          NOT NULL,
    "Status"            varchar(30)   NOT NULL DEFAULT 'DRAFT',
    "TotalAmount"       numeric(18,2) NOT NULL,
    "ApprovalTier"      varchar(10)   NOT NULL,
    "RequestedByUserId" uuid          NOT NULL,
    "RequestedAt"       timestamptz   NOT NULL,
    "ApprovedByUserId"  uuid,
    "ApprovedAt"        timestamptz,
    "RowVersion"        uuid          NOT NULL,

    CONSTRAINT "PK_FinPurchaseOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinPurchaseOrder_PONumber" UNIQUE ("PONumber"),
    CONSTRAINT "FK_FinPurchaseOrder_MstSupplier_SupplierId"
        FOREIGN KEY ("SupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinPurchaseOrder_Status"
        CHECK ("Status" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED',
                            'PARTIALLY_RECEIVED','FULLY_RECEIVED','CLOSED')),
    CONSTRAINT "CK_FinPurchaseOrder_ApprovalTier" CHECK ("ApprovalTier" IN ('TIER_1','TIER_2'))
);

CREATE TABLE public."FinPurchaseOrderItem" (
    "Id"              uuid          NOT NULL,
    "PurchaseOrderId" uuid          NOT NULL,
    "ProductCategory" varchar(100)  NOT NULL,
    "ProductName"     varchar(300)  NOT NULL,
    "Unit"            varchar(30)   NOT NULL,
    "Quantity"        numeric(18,2) NOT NULL,
    "UnitPrice"       numeric(18,2) NOT NULL,
    "LineTotal"       numeric(18,2) NOT NULL,

    CONSTRAINT "PK_FinPurchaseOrderItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinPurchaseOrderItem_FinPurchaseOrder_PurchaseOrderId"
        FOREIGN KEY ("PurchaseOrderId") REFERENCES public."FinPurchaseOrder" ("Id")
        ON DELETE RESTRICT
);

CREATE TABLE public."FinGoodsReceipt" (
    "Id"              uuid        NOT NULL,
    "GRNumber"        varchar(50) NOT NULL,
    "PurchaseOrderId" uuid        NOT NULL,
    "ReceivedDate"    date        NOT NULL,
    "Status"          varchar(20) NOT NULL DEFAULT 'RECEIVED',
    "RowVersion"      uuid        NOT NULL,

    CONSTRAINT "PK_FinGoodsReceipt" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinGoodsReceipt_GRNumber" UNIQUE ("GRNumber"),
    CONSTRAINT "FK_FinGoodsReceipt_FinPurchaseOrder_PurchaseOrderId"
        FOREIGN KEY ("PurchaseOrderId") REFERENCES public."FinPurchaseOrder" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "CK_FinGoodsReceipt_Status" CHECK ("Status" IN ('RECEIVED','CANCELLED'))
);

CREATE TABLE public."FinGoodsReceiptItem" (
    "Id"                  uuid          NOT NULL,
    "GoodsReceiptId"      uuid          NOT NULL,
    "PurchaseOrderItemId" uuid          NOT NULL,
    "ReceivedQuantity"    numeric(18,2) NOT NULL,
    "Notes"               varchar(500),

    CONSTRAINT "PK_FinGoodsReceiptItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinGoodsReceiptItem_FinGoodsReceipt_GoodsReceiptId"
        FOREIGN KEY ("GoodsReceiptId") REFERENCES public."FinGoodsReceipt" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinGoodsReceiptItem_FinPurchaseOrderItem_PurchaseOrderItemId"
        FOREIGN KEY ("PurchaseOrderItemId") REFERENCES public."FinPurchaseOrderItem" ("Id")
        ON DELETE RESTRICT
);

CREATE TABLE public."FinInvoiceExchange" (
    "Id"                    uuid        NOT NULL,
    "ExchangeNumber"        varchar(50) NOT NULL,
    "SupplierId"            uuid        NOT NULL,
    "PurchaseOrderId"       uuid,
    "GoodsReceiptId"        uuid,
    "SupplierInvoiceNumber" varchar(100) NOT NULL,
    "SupplierInvoiceDate"   date        NOT NULL,
    "ReceivedDate"          date        NOT NULL,
    "EstimatedDueDate"      date        NOT NULL,
    "Status"                varchar(20) NOT NULL DEFAULT 'RECEIVED',
    "RowVersion"            uuid        NOT NULL,

    CONSTRAINT "PK_FinInvoiceExchange" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinInvoiceExchange_ExchangeNumber" UNIQUE ("ExchangeNumber"),
    CONSTRAINT "FK_FinInvoiceExchange_MstSupplier_SupplierId"
        FOREIGN KEY ("SupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinInvoiceExchange_FinPurchaseOrder_PurchaseOrderId"
        FOREIGN KEY ("PurchaseOrderId") REFERENCES public."FinPurchaseOrder" ("Id")
        ON DELETE SET NULL,
    CONSTRAINT "FK_FinInvoiceExchange_FinGoodsReceipt_GoodsReceiptId"
        FOREIGN KEY ("GoodsReceiptId") REFERENCES public."FinGoodsReceipt" ("Id")
        ON DELETE SET NULL,
    CONSTRAINT "CK_FinInvoiceExchange_Status"
        CHECK ("Status" IN ('RECEIVED','LINKED_TO_INVOICE','CANCELLED'))
);

CREATE TABLE public."FinPurchasingInvoice" (
    "Id"                  uuid          NOT NULL,
    "InvoiceNumber"       varchar(50)   NOT NULL,
    "InvoiceExchangeId"   uuid          NOT NULL,
    "SupplierId"          uuid          NOT NULL,
    "SubtotalAmount"      numeric(18,2) NOT NULL,
    "DiscountAmount"      numeric(18,2) NOT NULL DEFAULT 0,
    "PPNAmount"           numeric(18,2) NOT NULL DEFAULT 0,
    "DownPaymentAmount"   numeric(18,2) NOT NULL DEFAULT 0,
    "OtherDeductionAmount" numeric(18,2) NOT NULL DEFAULT 0,
    "TotalAmount"         numeric(18,2) NOT NULL,
    "Status"              varchar(30)   NOT NULL DEFAULT 'DRAFT',
    "ApprovalTier"        varchar(10)   NOT NULL,
    "RequestedByUserId"   uuid          NOT NULL,
    "RequestedAt"         timestamptz   NOT NULL,
    "ApprovedByUserId"    uuid,
    "ApprovedAt"          timestamptz,
    "RowVersion"          uuid          NOT NULL,

    CONSTRAINT "PK_FinPurchasingInvoice" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinPurchasingInvoice_InvoiceNumber" UNIQUE ("InvoiceNumber"),
    CONSTRAINT "UQ_FinPurchasingInvoice_InvoiceExchangeId" UNIQUE ("InvoiceExchangeId"),
    CONSTRAINT "FK_FinPurchasingInvoice_FinInvoiceExchange_InvoiceExchangeId"
        FOREIGN KEY ("InvoiceExchangeId") REFERENCES public."FinInvoiceExchange" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinPurchasingInvoice_MstSupplier_SupplierId"
        FOREIGN KEY ("SupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinPurchasingInvoice_Status"
        CHECK ("Status" IN ('DRAFT','PENDING_APPROVAL','APPROVED','REJECTED','CANCELLED')),
    CONSTRAINT "CK_FinPurchasingInvoice_ApprovalTier" CHECK ("ApprovalTier" IN ('TIER_1','TIER_2'))
);

CREATE TABLE public."FinPurchasingInvoiceItem" (
    "Id"                  uuid          NOT NULL,
    "PurchasingInvoiceId" uuid          NOT NULL,
    "ProductName"         varchar(300)  NOT NULL,
    "Quantity"            numeric(18,2) NOT NULL,
    "UnitPrice"           numeric(18,2) NOT NULL,
    "LineTotal"           numeric(18,2) NOT NULL,

    CONSTRAINT "PK_FinPurchasingInvoiceItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinPurchasingInvoiceItem_FinPurchasingInvoice_PurchasingInvoiceId"
        FOREIGN KEY ("PurchasingInvoiceId") REFERENCES public."FinPurchasingInvoice" ("Id")
        ON DELETE RESTRICT
);

CREATE TABLE public."FinSupplierReturn" (
    "Id"                  uuid          NOT NULL,
    "ReturnNumber"        varchar(50)   NOT NULL,
    "PurchasingInvoiceId" uuid          NOT NULL,
    "Reason"              varchar(500)  NOT NULL,
    "TotalAmount"         numeric(18,2) NOT NULL,
    "PPNAmount"           numeric(18,2) NOT NULL DEFAULT 0,   -- Baru, AMENDMENT REVISI 6
    "Status"              varchar(20)   NOT NULL DEFAULT 'DRAFT',
    "RowVersion"          uuid          NOT NULL,

    CONSTRAINT "PK_FinSupplierReturn" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinSupplierReturn_ReturnNumber" UNIQUE ("ReturnNumber"),
    CONSTRAINT "CK_FinSupplierReturn_PPNAmount" CHECK ("PPNAmount" >= 0),
    CONSTRAINT "FK_FinSupplierReturn_FinPurchasingInvoice_PurchasingInvoiceId"
        FOREIGN KEY ("PurchasingInvoiceId") REFERENCES public."FinPurchasingInvoice" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierReturn_Status" CHECK ("Status" IN ('DRAFT','CONFIRMED','CANCELLED'))
);

CREATE TABLE public."FinSupplierReturnItem" (
    "Id"               uuid          NOT NULL,
    "SupplierReturnId" uuid          NOT NULL,
    "Description"      varchar(300)  NOT NULL,
    "Quantity"         numeric(18,2) NOT NULL,
    "LineTotal"        numeric(18,2) NOT NULL,

    CONSTRAINT "PK_FinSupplierReturnItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinSupplierReturnItem_FinSupplierReturn_SupplierReturnId"
        FOREIGN KEY ("SupplierReturnId") REFERENCES public."FinSupplierReturn" ("Id")
        ON DELETE RESTRICT
);

CREATE TABLE public."FinSupplierReturnDeposit" (
    "Id"              uuid          NOT NULL,
    "SupplierId"      uuid          NOT NULL,
    "SourceReturnId"  uuid          NOT NULL,
    "OriginalAmount"  numeric(18,2) NOT NULL,
    "AvailableAmount" numeric(18,2) NOT NULL,
    "Status"          varchar(20)   NOT NULL DEFAULT 'AVAILABLE',
    "RowVersion"      uuid          NOT NULL,

    CONSTRAINT "PK_FinSupplierReturnDeposit" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinSupplierReturnDeposit_SourceReturnId" UNIQUE ("SourceReturnId"),
    CONSTRAINT "FK_FinSupplierReturnDeposit_MstSupplier_SupplierId"
        FOREIGN KEY ("SupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinSupplierReturnDeposit_FinSupplierReturn_SourceReturnId"
        FOREIGN KEY ("SourceReturnId") REFERENCES public."FinSupplierReturn" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierReturnDeposit_Available" CHECK ("AvailableAmount" >= 0),
    CONSTRAINT "CK_FinSupplierReturnDeposit_Status"
        CHECK ("Status" IN ('AVAILABLE','EXHAUSTED','CANCELLED'))
);

CREATE TABLE public."FinSupplierReturnDepositUsage" (
    "Id"                       uuid          NOT NULL,
    "SupplierReturnDepositId"  uuid          NOT NULL,
    "PurchasingInvoiceId"      uuid          NOT NULL,
    "UsedAmount"                numeric(18,2) NOT NULL,
    "UsedAt"                    timestamptz   NOT NULL,

    CONSTRAINT "PK_FinSupplierReturnDepositUsage" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinSupplierReturnDepositUsage_FinSupplierReturnDeposit_DepositId"
        FOREIGN KEY ("SupplierReturnDepositId") REFERENCES public."FinSupplierReturnDeposit" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinSupplierReturnDepositUsage_FinPurchasingInvoice_InvoiceId"
        FOREIGN KEY ("PurchasingInvoiceId") REFERENCES public."FinPurchasingInvoice" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierReturnDepositUsage_Amount" CHECK ("UsedAmount" > 0)
);

-- kolom tambahan pada FinSupplierPayable
ALTER TABLE public."FinSupplierPayable"
    ADD COLUMN "SourcePurchasingInvoiceId" uuid;

ALTER TABLE public."FinSupplierPayable"
    ADD CONSTRAINT "FK_FinSupplierPayable_FinPurchasingInvoice_SourcePurchasingInvoiceId"
        FOREIGN KEY ("SourcePurchasingInvoiceId") REFERENCES public."FinPurchasingInvoice" ("Id")
        ON DELETE SET NULL;

CREATE TABLE public."FinReceivableInvoiceBatch" (
    "Id"              uuid          NOT NULL,
    "BatchNumber"     varchar(50)   NOT NULL,
    "DebtorType"      varchar(30)   NOT NULL DEFAULT 'PAYER',
    "DebtorReferenceId" uuid        NOT NULL,
    "PeriodStart"     date          NOT NULL,
    "PeriodEnd"       date          NOT NULL,
    "TotalAmount"     numeric(18,2) NOT NULL,
    "Status"          varchar(20)   NOT NULL DEFAULT 'DRAFT',
    "IssuedAt"        timestamptz,
    "RowVersion"      uuid          NOT NULL,

    CONSTRAINT "PK_FinReceivableInvoiceBatch" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinReceivableInvoiceBatch_BatchNumber" UNIQUE ("BatchNumber"),
    CONSTRAINT "CK_FinReceivableInvoiceBatch_DebtorType" CHECK ("DebtorType" = 'PAYER'),
    CONSTRAINT "CK_FinReceivableInvoiceBatch_Status"
        CHECK ("Status" IN ('DRAFT','ISSUED','PARTIALLY_PAID','PAID','CANCELLED'))
);

CREATE TABLE public."FinReceivableInvoiceBatchItem" (
    "Id"            uuid NOT NULL,
    "BatchId"       uuid NOT NULL,
    "ReceivableId"  uuid NOT NULL,

    CONSTRAINT "PK_FinReceivableInvoiceBatchItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceivableInvoiceBatchItem_FinReceivableInvoiceBatch_BatchId"
        FOREIGN KEY ("BatchId") REFERENCES public."FinReceivableInvoiceBatch" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceivableInvoiceBatchItem_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT
);

-- satu piutang hanya boleh tergabung dalam satu batch yang masih aktif
CREATE UNIQUE INDEX "IX_FinReceivableInvoiceBatchItem_ActiveReceivable"
    ON public."FinReceivableInvoiceBatchItem" ("ReceivableId")
    WHERE "IsDelete" = false;

CREATE TABLE public."FinReceiptDeduction" (
    "Id"              uuid          NOT NULL,
    "ReceiptId"       uuid          NOT NULL,
    "DeductionType"   varchar(30)   NOT NULL,
    "Amount"          numeric(18,2) NOT NULL,
    "Reason"          varchar(500),
    "ReferenceNumber" varchar(100),

    CONSTRAINT "PK_FinReceiptDeduction" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceiptDeduction_FinReceipt_ReceiptId"
        FOREIGN KEY ("ReceiptId") REFERENCES public."FinReceipt" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceiptDeduction_Type"
        CHECK ("DeductionType" IN ('PPH23','BANK_ADMIN_FEE','OTHER')),
    CONSTRAINT "CK_FinReceiptDeduction_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_FinReceiptDeduction_OtherReason"
        CHECK ("DeductionType" <> 'OTHER' OR "Reason" IS NOT NULL)
);

CREATE INDEX "IX_FinReceiptDeduction_Receipt"
    ON public."FinReceiptDeduction" ("ReceiptId", "DeductionType");
```

Seperti seluruh migration pada dokumen ini, DDL di atas **belum dijalankan** — wewenang membuat
dan menjalankan migration tetap terpisah dan membutuhkan otorisasi eksplisit tersendiri
(`02-backend-architecture.md` bagian C.9).


# AMENDMENT REVISI 5 — Sumber Dana Deposit Retur dan Jalur Potongan AR

| Field | Nilai |
|---|---|
| Revisi | `5`, 25 September 2026 — status `approved` 26 September 2026 |
| Keputusan | `FIN-DEC-057`, `058`, `061`, `062`; `FIN-DES-045`..`050` |
| Menggantikan | Bagian C.11 (`FinSupplierReturnDepositUsage`) dan C.15 (`FinReceiptDeduction`) pada AMENDMENT REVISI 4 — keduanya **belum pernah dibangun**, sehingga penggantian ini tidak menuntut pembetulan data |

## D.1 `FinPayment` — status `Diperbarui`

Seluruh kolom bagian 4.5 dan A.4 tetap berlaku. Yang **ditambahkan**:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `DepositAppliedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah `UsedAmount` baris pemakaian deposit berstatus `RESERVED`/`APPLIED`; MUST NOT negatif |

`NetTransferAmount` kini dihitung `TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount`.

## D.2 `FinSupplierReturnDepositUsage` — status `Baru` (menggantikan C.11)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SupplierReturnDepositId` | `Guid` | Ya | — | Index | FK ke `FinSupplierReturnDeposit` | `Restrict` | Tidak | Deposit yang dipakai |
| `PaymentId` | `Guid` | Ya | — | Index | FK ke `FinPayment` | `Restrict` | Tidak | **Menggantikan** `PurchasingInvoiceId` — pembayaran yang memakai deposit sebagai sumber dana (`FIN-DES-045`) |
| `UsedAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif |
| `Status` | `string(20)` | Ya | `RESERVED` | Index | — | — | Tidak | `RESERVED`, `APPLIED`, `RELEASED` (`FIN-DES-046`) |
| `UsedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu dicadangkan |
| `ReleasedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Terisi hanya saat `RELEASED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Optimistic concurrency |

Satu deposit tidak boleh dua kali dalam satu pembayaran selama barisnya belum dilepas — unique
index parsial `(PaymentId, SupplierReturnDepositId) WHERE Status <> 'RELEASED' AND IsDelete = false`.

## D.3 `FinReceiptDeduction` — status `Baru` (menggantikan C.15)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DeductionNumber` | `string(50)` | Ya | — | UK | — | — | Tidak | Contoh `DED-2026-09-00031`; dipakai sebagai `SourceTransactionId` kejadian (`FIN-DES-050`) |
| `ReceiptId` | `Guid` | Ya | — | Index | FK ke `FinReceipt` | `Restrict` | Tidak | Induk penerimaan — disimpan untuk penyaringan cepat |
| `ReceiptAllocationId` | `Guid` | Ya | — | Index | FK ke `FinReceiptAllocation` | `Restrict` | Tidak | **Baru.** Alokasi `TargetType = RECEIVABLE` tempat potongan melekat; menentukan piutang yang dikurangi (`FIN-DES-048`) |
| `DeductionType` | `string(30)` | Ya | — | Index | — | — | Tidak | `PPH23`, `BANK_ADMIN_FEE`, `OTHER` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu positif, termasuk pada baris pembalik |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | **Ya** bila memuat keterangan pihak ketiga | **Wajib** bila `DeductionType = OTHER` |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor bukti potong / bukti bank |
| `IsReversal` | `bool` | Ya | `false` | — | — | — | Tidak | **Baru.** Baris pembalik (`FIN-DES-049`) |
| `ReversalOfDeductionId` | `Guid?` | Tidak | — | UK parsial | FK ke `FinReceiptDeduction` (self) | `Restrict` | Tidak | **Baru.** Terisi hanya bila `IsReversal = true`; satu baris hanya dibalik sekali |

## D.4 Bentuk DDL amendment

> Peringatan yang sama dengan bagian 9 berlaku: dokumentasi bentuk, bukan skrip yang
> dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- (a) Tabel sudah berjalan — migration baru AddDepositAppliedAmountToFinPayment
ALTER TABLE public."FinPayment"
    ADD COLUMN "DepositAppliedAmount" numeric(18,2) NOT NULL DEFAULT 0;

ALTER TABLE public."FinPayment" DROP CONSTRAINT "CK_FinPayment_NetTransfer";
ALTER TABLE public."FinPayment"
    ADD CONSTRAINT "CK_FinPayment_NetTransfer"
        CHECK ("NetTransferAmount" = "TotalAmount" - "DeductionAmount"
                                     + "AdditionAmount" - "DepositAppliedAmount"),
    ADD CONSTRAINT "CK_FinPayment_DepositApplied" CHECK ("DepositAppliedAmount" >= 0);

-- (b) Menggantikan bentuk C.16 untuk tabel ini — ditulis di dalam AddPurchasingApRumpun
CREATE TABLE public."FinSupplierReturnDepositUsage" (
    "Id"                      uuid          NOT NULL,
    "SupplierReturnDepositId" uuid          NOT NULL,
    "PaymentId"               uuid          NOT NULL,
    "UsedAmount"              numeric(18,2) NOT NULL,
    "Status"                  varchar(20)   NOT NULL DEFAULT 'RESERVED',
    "UsedAt"                  timestamptz   NOT NULL,
    "ReleasedAt"              timestamptz,
    "RowVersion"              uuid          NOT NULL,

    CONSTRAINT "PK_FinSupplierReturnDepositUsage" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinSupplierReturnDepositUsage_FinSupplierReturnDeposit_DepositId"
        FOREIGN KEY ("SupplierReturnDepositId") REFERENCES public."FinSupplierReturnDeposit" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinSupplierReturnDepositUsage_FinPayment_PaymentId"
        FOREIGN KEY ("PaymentId") REFERENCES public."FinPayment" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierReturnDepositUsage_Amount" CHECK ("UsedAmount" > 0),
    CONSTRAINT "CK_FinSupplierReturnDepositUsage_Status"
        CHECK ("Status" IN ('RESERVED','APPLIED','RELEASED')),
    CONSTRAINT "CK_FinSupplierReturnDepositUsage_ReleasedAt"
        CHECK (("Status" = 'RELEASED') = ("ReleasedAt" IS NOT NULL))
);

CREATE UNIQUE INDEX "IX_FinSupplierReturnDepositUsage_ActivePerPayment"
    ON public."FinSupplierReturnDepositUsage" ("PaymentId", "SupplierReturnDepositId")
    WHERE "Status" <> 'RELEASED' AND "IsDelete" = false;

-- (c) Menggantikan bentuk C.16 untuk tabel ini — ditulis di dalam AddArInvoiceBatchAndReceiptDeduction
CREATE TABLE public."FinReceiptDeduction" (
    "Id"                    uuid          NOT NULL,
    "DeductionNumber"       varchar(50)   NOT NULL,
    "ReceiptId"             uuid          NOT NULL,
    "ReceiptAllocationId"   uuid          NOT NULL,
    "DeductionType"         varchar(30)   NOT NULL,
    "Amount"                numeric(18,2) NOT NULL,
    "Reason"                varchar(500),
    "ReferenceNumber"       varchar(100),
    "IsReversal"            boolean       NOT NULL DEFAULT false,
    "ReversalOfDeductionId" uuid,

    CONSTRAINT "PK_FinReceiptDeduction" PRIMARY KEY ("Id"),
    CONSTRAINT "UQ_FinReceiptDeduction_DeductionNumber" UNIQUE ("DeductionNumber"),
    CONSTRAINT "FK_FinReceiptDeduction_FinReceipt_ReceiptId"
        FOREIGN KEY ("ReceiptId") REFERENCES public."FinReceipt" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceiptDeduction_FinReceiptAllocation_ReceiptAllocationId"
        FOREIGN KEY ("ReceiptAllocationId") REFERENCES public."FinReceiptAllocation" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceiptDeduction_FinReceiptDeduction_ReversalOfDeductionId"
        FOREIGN KEY ("ReversalOfDeductionId") REFERENCES public."FinReceiptDeduction" ("Id")
        ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceiptDeduction_Type"
        CHECK ("DeductionType" IN ('PPH23','BANK_ADMIN_FEE','OTHER')),
    CONSTRAINT "CK_FinReceiptDeduction_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "CK_FinReceiptDeduction_OtherReason"
        CHECK ("DeductionType" <> 'OTHER' OR "Reason" IS NOT NULL),
    CONSTRAINT "CK_FinReceiptDeduction_Reversal"
        CHECK ("IsReversal" = ("ReversalOfDeductionId" IS NOT NULL))
);

CREATE INDEX "IX_FinReceiptDeduction_Allocation"
    ON public."FinReceiptDeduction" ("ReceiptAllocationId");
CREATE UNIQUE INDEX "IX_FinReceiptDeduction_ReversalOnce"
    ON public."FinReceiptDeduction" ("ReversalOfDeductionId")
    WHERE "ReversalOfDeductionId" IS NOT NULL AND "IsDelete" = false;
```

Syarat "alokasi harus `TargetType = RECEIVABLE` dan bukan pembalik" **tidak** dapat dijaga check
constraint karena menyangkut tabel lain; ia ditegakkan `FinanceReceiptService` (`FIN-VAL-129`).

# AMENDMENT REVISI 13 — `FinReceivableInvoiceBatch` menjadi `Diperbarui` (sumbu klaim penjamin)

Status: `draft`, 1 Oktober 2026. Diturunkan dari `FIN-DEC-097`; dirancang `FIN-DES-070`, `FIN-DES-071`.

Seluruh tabel pada kamus ini mewarisi `IdentityModel` (sepuluh kolom audit) — tidak diulang di bawah.
Penghapusan bersifat penandaan lewat `IsDelete`, bukan penghapusan baris.

## Tabel status dan kepemilikan — perubahan amendment ini

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `FinReceivableInvoiceBatch` | **Diperbarui** | Finance Management | Tujuh kolom sumbu klaim ditambahkan |
| `FinReceivableInvoiceBatchItem` | Sudah ada | Finance Management | Tidak berubah |
| `FinReceivable` | Sudah ada | Finance Management | Tidak berubah. Selisih klaim **MUST NOT** menyentuh `OutstandingAmount` |
| `FinReceivableWriteOff` | Sudah ada | Finance Management | Tidak berubah; hanya bertambah permukaan baca |
| `FinReceiptAllocation` | Sudah ada | Finance Management | Tidak berubah; hanya bertambah permukaan baca |

## `FinReceivableInvoiceBatch` — seluruh kolom (status `Diperbarui`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BatchNumber` | `string(50)` | Ya | — | Unique, tersaring `IsDelete = false` | — | — | Tidak | Nomor dokumen tagihan; dialokasikan service lewat number-series |
| `DebtorType` | `string(30)` | Ya | `PAYER` | Index | — | — | Tidak | Terkunci `PAYER` lewat check constraint (`FIN-DEC-048`) |
| `DebtorReferenceId` | `Guid` | Ya | — | Index | Rujukan penjamin milik modul lain | — | Tidak | **MUST NOT** disalin menjadi master penjamin milik Finance |
| `PeriodStart` | `DateOnly` | Ya | — | Index | — | — | Tidak | Awal periode penagihan |
| `PeriodEnd` | `DateOnly` | Ya | — | — | — | — | Tidak | Akhir periode penagihan |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jumlah `OriginalAmount` seluruh anggota |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | Sumbu dokumen dan pelunasan. **Tidak berubah** pada amendment ini |
| `IssuedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Terisi saat `ISSUED` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Concurrency token |
| **`ClaimStatus`** | **`string(30)?`** | **Tidak** | — | **Index** | — | — | Tidak | **Baru.** Sumbu jawaban penjamin: kosong, `SUBMITTED`, `PAYER_VERIFIED`, `APPROVED`, `CLOSED` |
| **`ApprovedAmount`** | **`decimal(18,2)?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Nominal yang disetujui penjamin. **MUST NOT** dipakai sebagai dasar pelunasan |
| **`PayerClaimReference`** | **`string(100)?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Nomor rujukan klaim milik penjamin. **MUST NOT** diisi keterangan klinis |
| **`ClaimNote`** | **`string(500)?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Keterangan petugas; wajib diisi saat nominal disetujui lebih kecil dari tagihan (`FIN-VAL-151`). **MUST NOT** diisi diagnosis |
| **`PayerVerifiedAt`** | **`DateTimeOffset?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Saat berkas dinyatakan diterima penjamin |
| **`ClaimApprovedAt`** | **`DateTimeOffset?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Saat nominal persetujuan dicatat |
| **`ClaimClosedAt`** | **`DateTimeOffset?`** | **Tidak** | — | — | — | — | Tidak | **Baru.** Saat klaim ditutup |

Selisih klaim (`TotalAmount` dikurangi `ApprovedAmount`) **tidak punya kolom** — ia dihitung pada
response sebagai `ClaimVarianceAmount` (`FIN-DES-071`). Menyimpannya membuat angka itu dapat basi
terhadap kedua sumbernya.

## Bentuk DDL

> Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan
> dengan migration.

```sql
-- Bentuk kolom yang DITAMBAHKAN pada public."FinReceivableInvoiceBatch".
-- Kolom audit IdentityModel dan kolom lama tidak ditulis ulang di sini.
ALTER TABLE public."FinReceivableInvoiceBatch"
    ADD COLUMN "ClaimStatus"          varchar(30),
    ADD COLUMN "ApprovedAmount"       numeric(18,2),
    ADD COLUMN "PayerClaimReference"  varchar(100),
    ADD COLUMN "ClaimNote"            varchar(500),
    ADD COLUMN "PayerVerifiedAt"      timestamp with time zone,
    ADD COLUMN "ClaimApprovedAt"      timestamp with time zone,
    ADD COLUMN "ClaimClosedAt"        timestamp with time zone;

ALTER TABLE public."FinReceivableInvoiceBatch"
    ADD CONSTRAINT "CK_FinReceivableInvoiceBatch_ClaimStatus"
    CHECK ("ClaimStatus" IS NULL OR "ClaimStatus" IN
        ('SUBMITTED', 'PAYER_VERIFIED', 'APPROVED', 'CLOSED'));

CREATE INDEX "IX_FinReceivableInvoiceBatch_ClaimStatus"
    ON public."FinReceivableInvoiceBatch" ("ClaimStatus");
```

Sumber kebenarannya tetap
`Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchConfiguration.cs`,
yang **MUST** diperbarui bersamaan dengan modelnya.

# AMENDMENT REVISI 13 (lanjutan) — Dua tabel baru: piutang sewa non-pasien

Status: `draft`, 1 Oktober 2026. Diturunkan dari `FIN-DEC-099`..`FIN-DEC-104`;
dirancang `FIN-DES-074`..`FIN-DES-077`.

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit) — tidak diulang di bawah.
Penghapusan bersifat penandaan lewat `IsDelete`, bukan penghapusan baris.

## Tabel status dan kepemilikan

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `FinNonPatientReceivable` | **Baru** | Finance Management | Tagihan sewa parkir dan tenant |
| `FinNonPatientReceivableSettlement` | **Baru** | Finance Management | Pelunasan atas tagihan sewa |
| `FinReceivable` | Sudah ada | Finance Management | **Tidak disentuh.** Invariant "wajib dari Billing" tetap utuh (`FIN-DEC-101`) |
| `FinReceipt` | Sudah ada | Finance Management | **Tidak disentuh.** Pelunasan sewa tidak melewatinya (`FIN-DES-075`) |

## `FinNonPatientReceivable` (status `Baru`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceivableNumber` | `string(50)` | Ya | — | Unique, tersaring `IsDelete = false` | — | — | Tidak | Nomor tagihan sewa. Dialokasikan service; mewarisi utang teknis `QBE-CODE-001`..`006` seperti seluruh rumpun ini |
| `Category` | `string(20)` | Ya | — | Index | — | — | Tidak | `PARKING` atau `TENANT`, dijaga check constraint (`FIN-DEC-104`) |
| `CounterpartyName` | `string(200)` | Ya | — | Index | — | — | Tidak | Nama penyewa. **Teks bebas** — tidak ada master penyewa (`FIN-DEC-100`) |
| `RentedObject` | `string(200)` | Ya | — | — | — | — | Tidak | Objek sewa, misalnya area parkir atau unit tenant. **Teks bebas** |
| `PeriodStart` | `DateOnly` | Ya | — | Index | — | — | Tidak | Awal periode yang ditagih |
| `PeriodEnd` | `DateOnly` | Ya | — | — | — | — | Tidak | Akhir periode yang ditagih |
| `DueDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Jatuh tempo; dasar perhitungan kelompok umur piutang |
| `BilledAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal sewa periode itu |
| `LateFeeAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Denda keterlambatan, **diketik petugas**, bukan dihitung sistem (`FIN-DEC-102`) |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Index | — | — | Tidak | Sisa yang belum dibayar. Service kapabilitas ini **satu-satunya** penulisnya |
| `Status` | `string(20)` | Ya | `OUTSTANDING` | Index | — | — | Tidak | `OUTSTANDING`, `PARTIALLY_SETTLED`, `SETTLED`, `WRITTEN_OFF`, `CANCELLED`, dijaga check constraint |
| `Note` | `string(500)` | Tidak | — | — | — | — | Tidak | Keterangan petugas, termasuk alasan penghapusan atau pembatalan |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Concurrency token |

**Nol kolom rujukan** ke `BilInvoice`, `FinReceivable`, `FinReceipt`, atau serah terima Billing —
ketiadaannya **disengaja** dan menjadi inti `FIN-DEC-101`.

## `FinNonPatientReceivableSettlement` (status `Baru`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `NonPatientReceivableId` | `Guid` | Ya | — | Index | FK ke `FinNonPatientReceivable` | `Restrict` | Tidak | Tagihan induk |
| `SettlementDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal uang diterima |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal diterima. **Boleh minus** untuk membatalkan pelunasan sebelumnya |
| `PaymentMethod` | `string(50)` | Ya | — | — | — | — | Tidak | Cara bayar, misalnya transfer atau tunai. Teks bebas pada rilis ini |
| `ReferenceNumber` | `string(100)` | Tidak | — | — | — | — | Tidak | Nomor bukti transfer atau kuitansi |
| `Note` | `string(500)` | Tidak | — | — | — | — | Tidak | Keterangan petugas |

Baris pelunasan **tidak pernah dihapus**; pembetulan dilakukan dengan menambah baris bernilai minus.

## Bentuk DDL

> Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan.

```sql
-- Kolom audit IdentityModel tidak ditulis ulang di sini.
CREATE TABLE public."FinNonPatientReceivable" (
    "Id"                 uuid           NOT NULL,
    "ReceivableNumber"   varchar(50)    NOT NULL,
    "Category"           varchar(20)    NOT NULL,
    "CounterpartyName"   varchar(200)   NOT NULL,
    "RentedObject"       varchar(200)   NOT NULL,
    "PeriodStart"        date           NOT NULL,
    "PeriodEnd"          date           NOT NULL,
    "DueDate"            date           NOT NULL,
    "BilledAmount"       numeric(18,2)  NOT NULL,
    "LateFeeAmount"      numeric(18,2)  NOT NULL DEFAULT 0,
    "OutstandingAmount"  numeric(18,2)  NOT NULL,
    "Status"             varchar(20)    NOT NULL DEFAULT 'OUTSTANDING',
    "Note"               varchar(500),
    "RowVersion"         uuid           NOT NULL,

    CONSTRAINT "PK_FinNonPatientReceivable" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinNonPatientReceivable_Category"
        CHECK ("Category" IN ('PARKING', 'TENANT')),
    CONSTRAINT "CK_FinNonPatientReceivable_Status"
        CHECK ("Status" IN ('OUTSTANDING', 'PARTIALLY_SETTLED', 'SETTLED', 'WRITTEN_OFF', 'CANCELLED'))
);

CREATE UNIQUE INDEX "IX_FinNonPatientReceivable_ReceivableNumber"
    ON public."FinNonPatientReceivable" ("ReceivableNumber") WHERE "IsDelete" = false;
CREATE INDEX "IX_FinNonPatientReceivable_Category"
    ON public."FinNonPatientReceivable" ("Category");
CREATE INDEX "IX_FinNonPatientReceivable_Status"
    ON public."FinNonPatientReceivable" ("Status");
CREATE INDEX "IX_FinNonPatientReceivable_DueDate"
    ON public."FinNonPatientReceivable" ("DueDate");

CREATE TABLE public."FinNonPatientReceivableSettlement" (
    "Id"                       uuid           NOT NULL,
    "NonPatientReceivableId"   uuid           NOT NULL,
    "SettlementDate"           date           NOT NULL,
    "Amount"                   numeric(18,2)  NOT NULL,
    "PaymentMethod"            varchar(50)    NOT NULL,
    "ReferenceNumber"          varchar(100),
    "Note"                     varchar(500),

    CONSTRAINT "PK_FinNonPatientReceivableSettlement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinNonPatientReceivableSettlement_FinNonPatientReceivable"
        FOREIGN KEY ("NonPatientReceivableId")
        REFERENCES public."FinNonPatientReceivable" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_FinNonPatientReceivableSettlement_NonPatientReceivableId"
    ON public."FinNonPatientReceivableSettlement" ("NonPatientReceivableId");
```

---

# Revisi 14 — Kamus data tabel baru dan tabel yang diperbarui

Seluruh tabel di bawah mewarisi `IdentityModel`; sepuluh kolom auditnya tidak diulang di sini
(lihat kepala dokumen). Penghapusan bersifat penandaan lewat `IsDelete`.

Turunan `FIN-DES-078`..`FIN-DES-091`. Delapan tabel `Baru`, dua tabel `Diperbarui`.

## R14.1 `FinReceivableMovement` — `Baru`

Buku mutasi piutang. Lokasi model:
`Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ReceivableId` | `Guid` | Ya | — | Index | FK ke `FinReceivable` | `Restrict` | Tidak | Induk piutang |
| `MovementType` | `string(30)` | Ya | — | Index | — | — | Tidak | Sembilan nilai; lihat `erd/receivable-collection.md` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | **Bertanda**: positif menaikkan sisa, negatif menurunkannya |
| `BalanceBefore` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Sisa piutang sebelum mutasi |
| `BalanceAfter` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Sisa sesudah mutasi; `MUST` sama dengan `BalanceBefore + Amount` |
| `BusinessDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | **Tanggal WIB** — dasar perhitungan posisi per tanggal |
| `OccurredAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Tanda waktu kejadian, tetap UTC |
| `SourceAllocationId` | `Guid?` | Tidak | — | Index | Rujukan `FinReceiptAllocation`, **bukan** FK | — | Tidak | Terisi untuk mutasi alokasi dan potongan |
| `PaymentMethodCode` | `string(30)?` | Tidak | — | Index | — | — | Tidak | Terisi hanya untuk `PEMBAYARAN-LANGSUNG`; `TRANSFER` atau `CASH` |
| `FundingSourceType` | `string(30)?` | Tidak | — | — | — | — | Tidak | `BANK_ACCOUNT` atau `CASH`; wajib bila `PaymentMethodCode` terisi |
| `FundingSourceId` | `Guid?` | Tidak | — | — | Rujukan `MstBankAccount`, **bukan** FK | — | Tidak | Kosong bila sumber dananya kas |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor rujukan transfer atau kuitansi |
| `ProofId` | `Guid?` | Tidak | — | **Unique** (parsial) | FK ke `FinTransactionProof` | `Restrict` | Tidak | Satu bukti **MUST NOT** dipakai dua mutasi |
| `OpeningItemBatchId` | `Guid?` | Tidak | — | Index | Rujukan `FinOpeningItemBatch`, **bukan** FK | — | Tidak | Terisi hanya untuk `PEMBUKAAN-MIGRASI` |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Catatan petugas; dapat memuat nama pihak ketiga |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Penelusuran lintas kejadian |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | Penyebab langsung |

Aturan tambahan: baris **tidak pernah diubah atau dihapus**; koreksi menambah baris.

## R14.2 `FinSupplierPayableMovement` — `Baru`

Lokasi model: `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableMovement.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SupplierPayableId` | `Guid` | Ya | — | Index | FK ke `FinSupplierPayable` | `Restrict` | Tidak | Induk utang |
| `MovementType` | `string(30)` | Ya | — | Index | — | — | Tidak | Lima nilai; lihat `erd/payable.md` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Bertanda, sama seperti mutasi piutang |
| `BalanceBefore` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | — |
| `BalanceAfter` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `MUST` sama dengan `BalanceBefore + Amount` |
| `BusinessDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | **Tanggal WIB**; untuk pembayaran dokumen disalin dari `FinPayment.ApprovedAt` |
| `OccurredAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | UTC |
| `PaymentId` | `Guid?` | Tidak | — | Index | Rujukan `FinPayment`, **bukan** FK | — | Tidak | Terisi untuk `PEMBAYARAN-DOKUMEN` |
| `PaymentAllocationId` | `Guid?` | Tidak | — | Index | Rujukan `FinPaymentAllocation`, **bukan** FK | — | Tidak | Satu baris per alokasi |
| `PaymentMethodCode` | `string(30)?` | Tidak | — | Index | — | — | Tidak | `TRANSFER` atau `CASH` |
| `FundingSourceType` | `string(30)?` | Tidak | — | — | — | — | Tidak | `BANK_ACCOUNT` atau `CASH` |
| `FundingSourceId` | `Guid?` | Tidak | — | — | Rujukan `MstBankAccount`, **bukan** FK | — | Tidak | — |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | — |
| `ProofId` | `Guid?` | Tidak | — | **Unique** (parsial) | FK ke `FinTransactionProof` | `Restrict` | Tidak | — |
| `OpeningItemBatchId` | `Guid?` | Tidak | — | Index | Rujukan `FinOpeningItemBatch`, **bukan** FK | — | Tidak | Terisi hanya untuk `PEMBUKAAN-MIGRASI` |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Dapat memuat keterangan supplier |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | — |

## R14.3 `FinCashMovement` — `Baru`

Lokasi model: `Areas/Corporate/FinanceManagement/CashManagement/Models/FinCashMovement.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `MovementType` | `string(40)` | Ya | — | **Unique** gabungan | — | — | Tidak | Tujuh nilai; lihat `erd/cash-and-master-data.md` |
| `Direction` | `string(3)` | Ya | — | Index | — | — | Tidak | `IN` atau `OUT` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | **Selalu positif**; arahnya dibawa `Direction` |
| `BusinessDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | **Tanggal WIB** — dasar posisi kas per tanggal |
| `OccurredAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | UTC |
| `SourceReferenceType` | `string(40)` | Ya | — | **Unique** gabungan | — | — | Tidak | `CASHIER_SHIFT`, `BANK_DEPOSIT`, `RECEIVABLE_MOVEMENT`, `PAYABLE_MOVEMENT`, `PAYMENT`, `OPENING_BALANCE` |
| `SourceReferenceId` | `string(100)` | Ya | — | **Unique** gabungan | — | — | Tidak | Id atau nomor sumbernya sebagai teks, karena sumbernya melintasi tabel dan modul |
| `CashierShiftId` | `Guid?` | Tidak | — | Index | Rujukan `BilCashierShift` milik **Billing**, **bukan** FK | — | Tidak | Terisi untuk `KAS-SHIFT` |
| `PaymentMethodCode` | `string(30)?` | Tidak | — | — | — | — | Tidak | Terisi untuk mutasi yang lahir dari pembayaran |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | — |

Unique index gabungan (`SourceReferenceType`, `SourceReferenceId`, `MovementType`) berfilter
`IsDelete = false` adalah **inti idempotensi** buku ini.

## R14.4 `FinSubledgerControlAccountMap` — `Baru`

Lokasi model:
`Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinSubledgerControlAccountMap.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BalanceGroup` | `string(30)` | Ya | — | **Unique** gabungan | — | — | Tidak | `KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`, `UTANG-JASA-MEDIS` |
| `SegmentKey` | `string(40)?` | Tidak | `NULL` | **Unique** gabungan | — | — | Tidak | `NULL` berarti satu akun menanggung seluruh kelompok |
| `ControlAccountCode` | `string(50)` | Ya | — | **Unique** (parsial, baris aktif) | — | — | Tidak | Kode akun milik Accounting, disimpan sebagai teks |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Baris tidak aktif disimpan sebagai riwayat pemetaan |
| `Notes` | `string(300)?` | Tidak | — | — | — | — | Tidak | Misalnya rujukan surat bagan akun Accounting |

## R14.5 `FinOpeningBalance` — `Baru`

Lokasi model:
`Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningBalance.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BalanceGroup` | `string(30)` | Ya | — | **Unique** (parsial, baris aktif) | — | — | Tidak | Lima nilai, sama dengan tabel pemetaan |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | `PIUTANG`, `UTANG-SUPPLIER`, dan `UTANG-JASA-MEDIS` **MUST** `0.00` |
| `CutoverDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal mulai perhitungan posisi; snapshot menolak periode yang berakhir sebelumnya |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `APPROVED`, `LOCKED` |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | **Wajib** — termasuk alasan nilai nol pada kelompok item migrasi |
| `AccountingReferenceDocument` | `string(200)` | Ya | — | — | — | — | Tidak | Rujukan dokumen saldo awal manual Accounting (G5) |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Terisi saat `APPROVED` |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `LockedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Sesudah terisi, baris **MUST NOT** berubah |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Concurrency token |

## R14.6 `FinOpeningItemBatch` — `Baru`

Lokasi model:
`Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningItemBatch.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BatchNumber` | `string(50)` | Ya | — | **Unique** | — | — | Tidak | Pola tanggal + GUID, mengikuti rumpun ini |
| `ItemKind` | `string(30)` | Ya | — | Index | — | — | Tidak | `RECEIVABLE` atau `SUPPLIER_PAYABLE`; **tidak** dicampur dalam satu batch |
| `Status` | `string(20)` | Ya | `DRAFT` | Index | — | — | Tidak | `DRAFT`, `VALIDATED`, `APPROVED`, `LOCKED`, `REJECTED` |
| `CutoverDate` | `DateOnly` | Ya | — | — | — | — | Tidak | `MUST` sama dengan `CutoverDate` pada `FinOpeningBalance` |
| `TotalItemCount` | `int` | Ya | `0` | — | — | — | Tidak | Hasil validasi |
| `TotalOutstandingAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Jumlah sisa seluruh baris yang lolos validasi |
| `DeclaredAccountingOpeningAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Angka yang **dinyatakan petugas** dari dokumen Accounting (`FIN-DES-090`) |
| `AccountingReferenceDocument` | `string(200)` | Ya | — | — | — | — | Tidak | Wajib sebelum `APPROVED` |
| `UploadedFileName` | `string(260)` | Ya | — | — | — | — | Tidak | Nama berkas asli yang diunggah |
| `SourceFormat` | `string(10)` | Ya | — | — | — | — | Tidak | **Kolom baru revisi 15** (`FIN-DES-093`). `CSV` atau `XLSX`, ditetapkan dari tipe media/ekstensi saat unggah — **bukan** dari ruas yang diisi pengguna. Dicatat supaya cacat paritas antar format dapat ditelusuri; ekstensi pada `UploadedFileName` dapat berbeda dari isi sebenarnya |
| `ValidationSummaryJson` | `text?` | Tidak | — | — | — | — | **Ya** | Hasil validasi per baris; dapat memuat nama debitur atau supplier |
| `RejectionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila `REJECTED` |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | — |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `LockedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

## R14.7 `FinTransactionProof` — `Baru`

Lokasi model: `Areas/Corporate/FinanceManagement/Collection/Models/FinTransactionProof.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Dipakai sebagai `ProofId` pada mutasi |
| `ProofType` | `string(30)` | Ya | — | Index | — | — | Tidak | Misalnya `BUKTI-TRANSFER`, `KUITANSI` |
| `OriginalFileName` | `string(260)` | Ya | — | — | — | — | Tidak | Nama berkas dari pengguna |
| `StoredFileName` | `string(260)` | Ya | — | **Unique** | — | — | Tidak | Nama hasil penormalan, mencegah tabrakan |
| `RelativePath` | `string(500)` | Ya | — | — | — | — | Tidak | Relatif terhadap `FileStorage:UploadRootPath`; **MUST** divalidasi berada di bawah akarnya |
| `MediaType` | `string(100)` | Ya | — | — | — | — | Tidak | **Diperbarui revisi 15** (`FIN-DEC-139`, `FIN-DES-092`): `application/pdf`, `image/jpeg`, `image/png`. Daftarnya dari `FinanceManagement:TransactionProof:AllowedExtensions`; tipe media **MUST** diperiksa, bukan hanya ekstensinya |
| `SizeBytes` | `long` | Ya | — | — | — | — | Tidak | **Diperbarui revisi 15** (`FIN-DEC-139`, `FIN-DES-092`): batas dari `FinanceManagement:TransactionProof:MaxFileSizeBytes`. Nilai awalnya **belum ditetapkan** (`FIN-OQ-082`); tanpa nilai itu unggah **ditolak** `503`, bukan dianggap tak terbatas |
| `UploadedBy` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `UploadedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | — |

Berkasnya **tidak** disimpan di database. Isi berkas bukti dapat memuat data pihak ketiga, sehingga
jalur unduhnya **MUST** dijaga hak akses dan **MUST NOT** dicatat logger beserta isinya.

## R14.8 `MstDirectPaymentThreshold` — `Baru`

Lokasi model: `Areas/Corporate/FinanceManagement/MasterData/Models/MstDirectPaymentThreshold.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Ambang rupiah; **nilai awalnya belum ditetapkan** (`FIN-OQ-074`) |
| `ChangeReason` | `string(500)` | Ya | — | — | — | — | Tidak | **Wajib** setiap kali diubah (`FIN-DEC-134`) |
| `IsActive` | `bool` | Ya | `true` | **Unique** (parsial, hanya `true`) | — | — | Tidak | Satu baris aktif saja |
| `EffectiveFrom` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal mulai berlaku |

Tanpa baris aktif, seluruh pembayaran langsung **ditolak** — perilaku yang disengaja.

## R14.9 `FinReceivable` — `Diperbarui`

Lokasi model: `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs`
Configuration: `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs`

**Hanya kolom yang berubah dan kolom kunci** yang ditulis di sini; kolom lainnya tidak berubah dan
sudah tercatat pada kamus data revisi sebelumnya.

| Kolom | Tipe sesudah | Wajib | Index | Perubahan | Sensitif |
|---|---|:---:|---|---|:---:|
| `SourceHandoffKey` | `Guid?` | **Tidak** (dulu Ya) | Unique, filter ditambah `IS NOT NULL` | **Menjadi nullable** | Tidak |
| `SourceHandoffId` | `Guid?` | **Tidak** (dulu Ya) | — | **Menjadi nullable** | Tidak |
| `InvoiceId` | `Guid?` | **Tidak** (dulu Ya) | Index | **Menjadi nullable** | Tidak |
| `OpeningItemBatchId` | `Guid?` | Tidak | Index | **Kolom baru**, FK ke `FinOpeningItemBatch`, `Restrict` | Tidak |
| `DebtorType` | `string(30)` | Ya | — | **Tidak berubah** — item migrasi tetap memakai tiga jenis yang sama | Tidak |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | Tidak berubah; `CK_FinReceivable_Outstanding >= 0` **tetap** | Tidak |

Check constraint **baru** `CK_FinReceivable_OpeningItem`:

```text
(  "SourceHandoffKey" IS NOT NULL AND "SourceHandoffId" IS NOT NULL
   AND "InvoiceId" IS NOT NULL   AND "OpeningItemBatchId" IS NULL )
OR
(  "SourceHandoffKey" IS NULL     AND "SourceHandoffId" IS NULL
   AND "InvoiceId" IS NULL        AND "OpeningItemBatchId" IS NOT NULL )
```

Artinya invariant "piutang pasien wajib berasal dari serah terima Billing" **tetap ditegakkan
database**, hanya kini bersyarat pada jenis barisnya.

## R14.10 `FinSupplierPayable` — `Diperbarui`

| Kolom | Tipe sesudah | Wajib | Index | Perubahan | Sensitif |
|---|---|:---:|---|---|:---:|
| `OpeningItemBatchId` | `Guid?` | Tidak | Index | **Kolom baru**, FK ke `FinOpeningItemBatch`, `Restrict` | Tidak |

Nol kolom lain berubah. `SupplierId` **tetap wajib**, sehingga setiap item migrasi utang **MUST**
menunjuk supplier yang sudah ada di master — dijaga FK yang sudah ada, dan menjadi salah satu
pemeriksaan validasi batch.

---

# Revisi 14 — Bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di
> bawah adalah **dokumentasi bentuk tabel**, **bukan** skrip untuk dijalankan. Menjalankannya akan
> berbenturan dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."FinReceivableMovement" (
    "Id"                   uuid           NOT NULL,
    "ReceivableId"         uuid           NOT NULL,
    "MovementType"         varchar(30)    NOT NULL,
    "Amount"               numeric(18,2)  NOT NULL,
    "BalanceBefore"        numeric(18,2)  NOT NULL,
    "BalanceAfter"         numeric(18,2)  NOT NULL,
    "BusinessDate"         date           NOT NULL,
    "OccurredAt"           timestamptz    NOT NULL,
    "SourceAllocationId"   uuid,
    "PaymentMethodCode"    varchar(30),
    "FundingSourceType"    varchar(30),
    "FundingSourceId"      uuid,
    "ReferenceNumber"      varchar(100),
    "ProofId"              uuid,
    "OpeningItemBatchId"   uuid,
    "Notes"                varchar(500),          -- SENSITIF
    "CorrelationId"        uuid           NOT NULL,
    "CausationId"          uuid           NOT NULL,
    CONSTRAINT "PK_FinReceivableMovement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinReceivableMovement_FinReceivable_ReceivableId"
        FOREIGN KEY ("ReceivableId") REFERENCES public."FinReceivable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinReceivableMovement_FinTransactionProof_ProofId"
        FOREIGN KEY ("ProofId") REFERENCES public."FinTransactionProof" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinReceivableMovement_Balance"
        CHECK ("BalanceAfter" = "BalanceBefore" + "Amount"),
    CONSTRAINT "CK_FinReceivableMovement_FundingSource"
        CHECK ("PaymentMethodCode" IS NULL OR "FundingSourceType" IS NOT NULL)
);

CREATE INDEX "IX_FinReceivableMovement_ReceivableId_BusinessDate"
    ON public."FinReceivableMovement" ("ReceivableId", "BusinessDate");
CREATE INDEX "IX_FinReceivableMovement_BusinessDate"
    ON public."FinReceivableMovement" ("BusinessDate");
CREATE UNIQUE INDEX "IX_FinReceivableMovement_ProofId"
    ON public."FinReceivableMovement" ("ProofId")
    WHERE "ProofId" IS NOT NULL AND "IsDelete" = false;

CREATE TABLE public."FinSupplierPayableMovement" (
    "Id"                   uuid           NOT NULL,
    "SupplierPayableId"    uuid           NOT NULL,
    "MovementType"         varchar(30)    NOT NULL,
    "Amount"               numeric(18,2)  NOT NULL,
    "BalanceBefore"        numeric(18,2)  NOT NULL,
    "BalanceAfter"         numeric(18,2)  NOT NULL,
    "BusinessDate"         date           NOT NULL,
    "OccurredAt"           timestamptz    NOT NULL,
    "PaymentId"            uuid,
    "PaymentAllocationId"  uuid,
    "PaymentMethodCode"    varchar(30),
    "FundingSourceType"    varchar(30),
    "FundingSourceId"      uuid,
    "ReferenceNumber"      varchar(100),
    "ProofId"              uuid,
    "OpeningItemBatchId"   uuid,
    "Notes"                varchar(500),          -- SENSITIF
    "CorrelationId"        uuid           NOT NULL,
    "CausationId"          uuid           NOT NULL,
    CONSTRAINT "PK_FinSupplierPayableMovement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayableId"
        FOREIGN KEY ("SupplierPayableId") REFERENCES public."FinSupplierPayable" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_FinSupplierPayableMovement_FinTransactionProof_ProofId"
        FOREIGN KEY ("ProofId") REFERENCES public."FinTransactionProof" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_FinSupplierPayableMovement_Balance"
        CHECK ("BalanceAfter" = "BalanceBefore" + "Amount")
);

CREATE INDEX "IX_FinSupplierPayableMovement_SupplierPayableId_BusinessDate"
    ON public."FinSupplierPayableMovement" ("SupplierPayableId", "BusinessDate");
CREATE UNIQUE INDEX "IX_FinSupplierPayableMovement_ProofId"
    ON public."FinSupplierPayableMovement" ("ProofId")
    WHERE "ProofId" IS NOT NULL AND "IsDelete" = false;

CREATE TABLE public."FinCashMovement" (
    "Id"                    uuid           NOT NULL,
    "MovementType"          varchar(40)    NOT NULL,
    "Direction"             varchar(3)     NOT NULL,
    "Amount"                numeric(18,2)  NOT NULL,
    "BusinessDate"          date           NOT NULL,
    "OccurredAt"            timestamptz    NOT NULL,
    "SourceReferenceType"   varchar(40)    NOT NULL,
    "SourceReferenceId"     varchar(100)   NOT NULL,
    "CashierShiftId"        uuid,
    "PaymentMethodCode"     varchar(30),
    "Notes"                 varchar(500),         -- SENSITIF
    "CorrelationId"         uuid           NOT NULL,
    "CausationId"           uuid           NOT NULL,
    CONSTRAINT "PK_FinCashMovement" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinCashMovement_Direction" CHECK ("Direction" IN ('IN','OUT')),
    CONSTRAINT "CK_FinCashMovement_Amount" CHECK ("Amount" > 0)
);

CREATE UNIQUE INDEX "IX_FinCashMovement_Source"
    ON public."FinCashMovement" ("SourceReferenceType", "SourceReferenceId", "MovementType")
    WHERE "IsDelete" = false;
CREATE INDEX "IX_FinCashMovement_BusinessDate_Direction"
    ON public."FinCashMovement" ("BusinessDate", "Direction");

CREATE TABLE public."FinSubledgerControlAccountMap" (
    "Id"                   uuid          NOT NULL,
    "BalanceGroup"         varchar(30)   NOT NULL,
    "SegmentKey"           varchar(40),
    "ControlAccountCode"   varchar(50)   NOT NULL,
    "IsActive"             boolean       NOT NULL DEFAULT true,
    "Notes"                varchar(300),
    CONSTRAINT "PK_FinSubledgerControlAccountMap" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinSubledgerControlAccountMap_BalanceGroup"
        CHECK ("BalanceGroup" IN ('KAS-KASIR','KAS-KECIL','PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS'))
);

CREATE UNIQUE INDEX "IX_FinSubledgerControlAccountMap_Group_Segment"
    ON public."FinSubledgerControlAccountMap" ("BalanceGroup", COALESCE("SegmentKey", ''))
    WHERE "IsActive" = true AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_FinSubledgerControlAccountMap_ControlAccountCode"
    ON public."FinSubledgerControlAccountMap" ("ControlAccountCode")
    WHERE "IsActive" = true AND "IsDelete" = false;

CREATE TABLE public."FinOpeningBalance" (
    "Id"                            uuid           NOT NULL,
    "BalanceGroup"                  varchar(30)    NOT NULL,
    "Amount"                        numeric(18,2)  NOT NULL,
    "CutoverDate"                   date           NOT NULL,
    "Status"                        varchar(20)    NOT NULL DEFAULT 'DRAFT',
    "Reason"                        varchar(500)   NOT NULL,
    "AccountingReferenceDocument"   varchar(200)   NOT NULL,
    "ApprovedBy"                    uuid,
    "ApprovedAt"                    timestamptz,
    "LockedAt"                      timestamptz,
    "RowVersion"                    uuid           NOT NULL,
    CONSTRAINT "PK_FinOpeningBalance" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinOpeningBalance_Status"
        CHECK ("Status" IN ('DRAFT','APPROVED','LOCKED')),
    CONSTRAINT "CK_FinOpeningBalance_ItemGroupZero"
        CHECK ("BalanceGroup" NOT IN ('PIUTANG','UTANG-SUPPLIER','UTANG-JASA-MEDIS') OR "Amount" = 0)
);

CREATE UNIQUE INDEX "IX_FinOpeningBalance_BalanceGroup"
    ON public."FinOpeningBalance" ("BalanceGroup")
    WHERE "IsDelete" = false;

CREATE TABLE public."FinOpeningItemBatch" (
    "Id"                                uuid           NOT NULL,
    "BatchNumber"                       varchar(50)    NOT NULL,
    "ItemKind"                          varchar(30)    NOT NULL,
    "Status"                            varchar(20)    NOT NULL DEFAULT 'DRAFT',
    "CutoverDate"                       date           NOT NULL,
    "TotalItemCount"                    integer        NOT NULL DEFAULT 0,
    "TotalOutstandingAmount"            numeric(18,2)  NOT NULL DEFAULT 0,
    "DeclaredAccountingOpeningAmount"   numeric(18,2)  NOT NULL,
    "AccountingReferenceDocument"       varchar(200)   NOT NULL,
    "UploadedFileName"                  varchar(260)   NOT NULL,
    "ValidationSummaryJson"             text,                      -- SENSITIF
    "RejectionReason"                   varchar(500),
    "ApprovedBy"                        uuid,
    "ApprovedAt"                        timestamptz,
    "LockedAt"                          timestamptz,
    "RowVersion"                        uuid           NOT NULL,
    CONSTRAINT "PK_FinOpeningItemBatch" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_FinOpeningItemBatch_ItemKind"
        CHECK ("ItemKind" IN ('RECEIVABLE','SUPPLIER_PAYABLE')),
    CONSTRAINT "CK_FinOpeningItemBatch_Status"
        CHECK ("Status" IN ('DRAFT','VALIDATED','APPROVED','LOCKED','REJECTED'))
);

CREATE UNIQUE INDEX "IX_FinOpeningItemBatch_BatchNumber"
    ON public."FinOpeningItemBatch" ("BatchNumber") WHERE "IsDelete" = false;

CREATE TABLE public."FinTransactionProof" (
    "Id"                 uuid          NOT NULL,
    "ProofType"          varchar(30)   NOT NULL,
    "OriginalFileName"   varchar(260)  NOT NULL,
    "StoredFileName"     varchar(260)  NOT NULL,
    "RelativePath"       varchar(500)  NOT NULL,
    "MediaType"          varchar(100)  NOT NULL,
    "SizeBytes"          bigint        NOT NULL,
    "UploadedBy"         uuid          NOT NULL,
    "UploadedAt"         timestamptz   NOT NULL,
    CONSTRAINT "PK_FinTransactionProof" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_FinTransactionProof_StoredFileName"
    ON public."FinTransactionProof" ("StoredFileName") WHERE "IsDelete" = false;

CREATE TABLE public."MstDirectPaymentThreshold" (
    "Id"              uuid           NOT NULL,
    "Amount"          numeric(18,2)  NOT NULL,
    "ChangeReason"    varchar(500)   NOT NULL,
    "IsActive"        boolean        NOT NULL DEFAULT true,
    "EffectiveFrom"   date           NOT NULL,
    CONSTRAINT "PK_MstDirectPaymentThreshold" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_MstDirectPaymentThreshold_Amount" CHECK ("Amount" > 0)
);

CREATE UNIQUE INDEX "IX_MstDirectPaymentThreshold_Active"
    ON public."MstDirectPaymentThreshold" (("IsActive"))
    WHERE "IsActive" = true AND "IsDelete" = false;

-- Perubahan pada tabel yang sudah berjalan (migration ke-4)

ALTER TABLE public."FinReceivable" ALTER COLUMN "SourceHandoffKey" DROP NOT NULL;
ALTER TABLE public."FinReceivable" ALTER COLUMN "SourceHandoffId"  DROP NOT NULL;
ALTER TABLE public."FinReceivable" ALTER COLUMN "InvoiceId"        DROP NOT NULL;
ALTER TABLE public."FinReceivable" ADD COLUMN "OpeningItemBatchId" uuid NULL;

ALTER TABLE public."FinReceivable"
    ADD CONSTRAINT "FK_FinReceivable_FinOpeningItemBatch_OpeningItemBatchId"
    FOREIGN KEY ("OpeningItemBatchId") REFERENCES public."FinOpeningItemBatch" ("Id") ON DELETE RESTRICT;

-- Dua langkah, sengaja dipisah supaya tidak memindai seluruh tabel di dalam kunci tulis
ALTER TABLE public."FinReceivable"
    ADD CONSTRAINT "CK_FinReceivable_OpeningItem" CHECK (
        (    "SourceHandoffKey" IS NOT NULL AND "SourceHandoffId" IS NOT NULL
         AND "InvoiceId" IS NOT NULL        AND "OpeningItemBatchId" IS NULL )
     OR (    "SourceHandoffKey" IS NULL     AND "SourceHandoffId" IS NULL
         AND "InvoiceId" IS NULL            AND "OpeningItemBatchId" IS NOT NULL )
    ) NOT VALID;
ALTER TABLE public."FinReceivable" VALIDATE CONSTRAINT "CK_FinReceivable_OpeningItem";

-- Index unik diganti filternya; CONCURRENTLY supaya tidak memblokir tulisan
DROP INDEX public."IX_FinReceivable_SourceHandoffKey";
CREATE UNIQUE INDEX CONCURRENTLY "IX_FinReceivable_SourceHandoffKey"
    ON public."FinReceivable" ("SourceHandoffKey")
    WHERE "IsDelete" = false AND "SourceHandoffKey" IS NOT NULL;

ALTER TABLE public."FinSupplierPayable" ADD COLUMN "OpeningItemBatchId" uuid NULL;
ALTER TABLE public."FinSupplierPayable"
    ADD CONSTRAINT "FK_FinSupplierPayable_FinOpeningItemBatch_OpeningItemBatchId"
    FOREIGN KEY ("OpeningItemBatchId") REFERENCES public."FinOpeningItemBatch" ("Id") ON DELETE RESTRICT;
```

Dua catatan jujur tentang DDL di atas:

1. `CREATE INDEX CONCURRENTLY` **tidak dapat** berjalan di dalam transaksi, sehingga migration
   ke-4 **MUST** menandai langkah itu tanpa transaksi, atau langkah index dijalankan sebagai
   langkah operasional terpisah. Mana yang dipakai adalah **keputusan pemilik repository**.
2. Jeda antara `DROP INDEX` dan selesainya `CREATE INDEX CONCURRENTLY` adalah jendela ketika
   idempotensi intake Billing **tidak** dijaga index. Urutan yang lebih aman — membuat index baru
   bernama lain lebih dulu, lalu menghapus yang lama — **SHOULD** dipakai, dan dicatat di sini
   supaya implementer tidak menyalin urutan di atas apa adanya.
