# Finance Management — Arsitektur Backend

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` |
| Revision | `1` |
| Status | `approved` untuk **`FIN-DES-001`..`036`** — `FIN-DES-029`..`036` (AMENDMENT REVISI 3) disetujui Yasmin 25 September 2026 lewat pernyataan "Saya approve semua", dicatat apa adanya. `FIN-DES-001`..`024` disetujui Yasmin pada 20 September 2026, `FIN-DES-025`..`028` pada hari yang sama. Approval ini **bukan** otorisasi migration, perubahan source, maupun pendaftaran registry |
| Masukan | `00-interview-decisions.md` — `FIN-DEC-001`..`044` (`030`..`044` ditambahkan 25 September 2026), `01-existing-capability-map.md` beserta impact scan bagian 9.3 |
| Backend SHA | `09101d05` untuk bagian 1-10 dan AMENDMENT REVISI 2; **`d6cdfaf9`** untuk AMENDMENT REVISI 3 (branch `Yasmina`) |
| Frontend SHA | `abed49b03` |
| Tanggal | 20 September 2026; AMENDMENT REVISI 2 pada tanggal yang sama; **AMENDMENT REVISI 3 pada 25 September 2026** |
| Amendment yang berlaku | REVISI 2 (rumpun Payable, `FIN-DES-025`..`028`) dan **REVISI 3 (uang muka/deposit/selisih kas, `FIN-DES-029`..`036`)**. Keduanya di akhir dokumen; bagian 1-10 tidak ditulis ulang |

Dokumen ini menetapkan bentuk backend modul Finance Management. Ia **menurunkan** dari 23
keputusan bisnis yang sudah disetujui; ia tidak membuat keputusan bisnis baru. Setiap kali
dokumen ini mengambil keputusan teknis, keputusan itu diberi nomor `FIN-DES-nnn` dan disertai
keputusan bisnis yang menjadi dasarnya.

---

## 1. Bounded context dan kepemilikan

Finance Management adalah **subledger operasional keuangan**. Ia duduk di tengah: menerima
fakta dari Billing dan Medical Fee, mengelola piutang/utang/kas, lalu menerbitkan kejadian ke
Accounting. Ia tidak pernah menulis balik ke modul sumber.

### 1.1 Tujuh konteks di dalam modul

| Konteks | Folder submodul | Aggregate root | Tanggung jawab |
|---|---|---|---|
| Billing Intake | `BillingIntake/` | `FinBillingHandoffIntake` | Satu pintu masuk seluruh fakta dari Billing, beserta jejak ACK dan penanganan gagal |
| Receivable (Piutang) | `Receivable/` | `FinReceivable` | Piutang, rincian, dokumen klaim, koreksi, dan penghapusan piutang |
| Collection (Penerimaan) | `Collection/` | `FinReceipt` | Uang yang benar-benar diterima, beserta alokasinya ke piutang atau langsung ke tagihan |
| Payable (Utang) | `Payable/` | `FinSupplierPayable`, `FinDoctorPayable`, `FinPayment` | Utang supplier, utang dokter, dan pembayaran keluar |
| Cash Management | `CashManagement/` | `FinBankDeposit`, `FinDailyCashSnapshot` | Setoran bank dan posisi kas harian |
| Master Data | `MasterData/` | `MstBank`, `MstBankAccount`, `MstCurrency`, `MstPettyCashCategory` | Data induk milik Finance |
| Accounting Integration | `AccountingIntegration/` | `FinAccountingEventOutbox`, `FinSubledgerPeriodBalance` | Kotak keluar kejadian ke Accounting dan saldo subledger periode |

`Petty Cash/` yang sudah ada **tidak** disentuh pass ini. Kepemilikan keputusannya ada di
blueprint `billing-kasir` (`PC-DEC-*`/`PC-DES-*`) sesuai `FIN-DEC-009`.

### 1.2 Tabel kepemilikan data

Ini pertahanan paling langsung terhadap duplikasi entity. Kolom terakhir adalah yang paling
penting dibaca.

| Kelompok data | Modul pemilik | Dipakai modul ini | Dibuat ulang di modul ini |
|---|---|:---:|---|
| Pasien, kunjungan, rekam medis | Registration / Clinical | Ya, sebagai rujukan Id saja | **Tidak.** Finance hanya menyimpan `PatientId`/`EncounterId` sebagai rujukan pada rincian piutang |
| Tagihan, perhitungan, finalisasi (`BilInvoice`) | Billing | Ya, rujukan Id | **Tidak.** Finance tidak pernah menghitung ulang nilai tagihan |
| Penyelesaian pembayaran kasir (`BilSettlement`, `BilTender`, `BilPaymentAllocation`) | Billing | Ya, sebagai sumber fakta penerimaan | **Tidak.** Finance menyalin nilainya ke `FinReceipt`, tidak menghitung ulang |
| Shift kasir (`BilCashierShift`) | Billing | Ya, rujukan `CashierShiftId` untuk rekonsiliasi | **Tidak.** Finance tidak pernah menulis `SystemCash`/`PhysicalCash` |
| Handoff AR/AP (`BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment`) | Billing | Ya, dikonsumsi lewat `FinBillingHandoffIntake` | **Tidak.** Finance membaca dan memberi ACK, tidak memiliki |
| Handoff penerimaan (`BilCollectionHandoff`) | Billing | Ya | **Tidak.** Tabelnya dibangun tim Billing; blueprint ini hanya mengunci bentuk kontraknya (`FIN-DES-023`) |
| Voucher kas kecil (`BilPettyCashVoucher`) | Billing / HealthServices | Ya, ditaut `VoucherId` dari movement | **Tidak** |
| Anggaran & mutasi kas kecil (`FinPettyCashBudget`, `FinPettyCashBudgetMovement`) | **Finance** (sudah ada) | Ya | **Tidak.** Sudah ada dan dipakai apa adanya |
| Kategori kas kecil (`MstPettyCashCategory`) | **Finance** (sudah ada) | Ya | **Tidak** |
| Supplier (`MstSupplier`) | Administrator / MasterData | Ya, lewat `SupplierId` | **Tidak** — `FIN-DEC-014` menutup ini secara eksplisit |
| Aturan fee dokter & hasil fee (`MstDoctorServiceRule`, `DoctorServiceFee`) | Medical Fee | Ya, rujukan `SourceDoctorServiceFeeId` | **Tidak.** Finance hanya menerima fee yang sudah disetujui |
| Dokter (identitas) | HR / Master Data | Ya, rujukan `DoctorId` | **Tidak** |
| Pegawai & eligibilitas manfaat | HR | Ya, rujukan `BenefitOwnerId` | **Tidak.** `FIN-DEC-016` menegaskan Finance tidak menentukan ulang identitas ini |
| COA, jenis kejadian, aturan posting, periode, jurnal | Accounting | Ya, rujukan kode saja | **Tidak.** Finance tidak pernah menyimpan nomor akun |
| Badan hukum, cost center | Corporate / HR | Ya, rujukan `LegalEntityId` | **Tidak** |
| **Piutang, rincian, dokumen klaim, koreksi, write-off** | **Finance** | Ya | **Ya — baru.** Tidak ada pemilik lain |
| **Penerimaan dan alokasinya** | **Finance** | Ya | **Ya — baru.** Tidak ada pemilik lain |
| **Utang supplier, utang dokter, pembayaran keluar** | **Finance** | Ya | **Ya — baru.** Tidak ada pemilik lain |
| **Setoran bank dan kas harian** | **Finance** | Ya | **Ya — baru.** Tidak ada pemilik lain |
| **Bank, rekening bank, mata uang, kurs** | **Finance** | Ya | **Ya — baru.** `FIN-DEC-018` |
| **Kotak keluar kejadian Accounting** | **Finance** | Ya | **Ya — baru.** Finance adalah satu-satunya penerbit (`FIN-DEC-001`) |

---

## 2. Keputusan arsitektur

Seluruhnya `approved` 20 September 2026 oleh Yasmin (Product/Domain Owner Finance).

`FIN-DES-003` mendapat penegasan terpisah dari owner pada hari yang sama dan karena itu berlaku
sebagai **ketentuan modul**, bukan sekadar keputusan satu pass:

> Bila sebuah entity di dalam `FinanceManagement` berperan sebagai data induk, prefiksnya `Mst`.
> Entity selain itu berprefiks `Fin`.

Aturan itu mengikat entity Finance berikutnya, bukan hanya yang dirancang pass ini.

### 2.1 Penempatan dan penamaan

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-001` | Seluruh model, DTO, service, dan controller Finance ditempatkan di `Areas/Corporate/FinanceManagement/<Submodul>/`, sedangkan EF configuration terpisah di `Repositories/Configurations/Corporate/FinanceManagement/<Submodul>/` | Pola nyata `MstPettyCashCategoryConfiguration.cs` dan `FinPettyCashBudgetConfiguration.cs` | Implementer MUST NOT menaruh configuration di dalam `Areas/` |
| `FIN-DES-002` | Enam folder submodul baru (`BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration`) MUST didaftarkan pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` **sebelum** file model pertama ditulis | `QBE-MOD-003`, `QBE-NAM-004`; preseden `PC-OQ-003` pada billing-kasir | Tanpa pendaftaran, QBE menolak seluruh entity `Fin*` baru. Ini prasyarat implementasi, bukan blocker perencanaan |
| `FIN-DES-003` | **Ketentuan modul.** Entity yang berperan sebagai data induk di dalam `FinanceManagement` memakai prefix `Mst`; entity lainnya memakai prefix `Fin` | Preseden nyata `MstPettyCashCategory`; registry mencatat `Mst` = MasterData. **Ditegaskan langsung owner 20 September 2026** sebagai aturan modul, bukan sekadar usulan desain | Menggantikan penamaan `FinBank`/`FinBankAccount`/`FinCurrency` pada `FIN-PRD-V2-0.3` bagian 11 dengan `MstBank`/`MstBankAccount`/`MstCurrency`/`MstExchangeRate`. Mengikat entity Finance berikutnya, bukan hanya yang dirancang pass ini |

### 2.2 Pola teknis yang diwarisi dari Petty Cash

Keempat keputusan berikut tidak menciptakan pola baru. Semuanya menyalin pola yang sudah
berjalan, supaya modul ini tidak menjadi pulau tersendiri.

| ID | Keputusan | Bukti pola | Konsekuensi |
|---|---|---|---|
| `FIN-DES-004` | Status disimpan sebagai `string` dengan `[MaxLength(30)]`, daftar nilainya di `public static class ...Statuses`, dan ditegakkan `HasCheckConstraint` di configuration. **Bukan** enum C# yang dikonversi ke `int` | `FinPettyCashBudget.Status` + `PettyCashBudgetStatuses` + `CK_FinPettyCashBudgetMovement_MovementType` | Nilai status terbaca langsung di database tanpa kamus. Template ERD yang menyarankan `int` **tidak** dipakai di sini karena bertentangan dengan pola modul |
| `FIN-DES-005` | Optimistic concurrency memakai `Guid RowVersion` yang di-set ulang `Guid.NewGuid()` setiap perubahan, dicek manual terhadap `ExpectedRowVersion` dari request | `EnsureCurrentRowVersion` pada `PettyCashBudgetService` | Request pengubah state MUST membawa `ExpectedRowVersion`. Bila kosong → `400`; bila tidak cocok → `409` |
| `FIN-DES-006` | Setiap command yang memindahkan uang MUST membawa header `Idempotency-Key` bertipe `Guid`, dan hasilnya disimpan pada kolom `IdempotencyKey` dengan partial unique index | `PettyCashBudgetController.TopUp`, `IX_FinPettyCashBudgetMovement_IdempotencyKey` | Kiriman ulang mengembalikan hasil yang sama, tidak membuat baris kedua |
| `FIN-DES-007` | Command yang mengubah saldo atau `OutstandingAmount` dijalankan di dalam `BeginTransactionAsync(IsolationLevel.Serializable)` | `PettyCashBudgetService.TopUpAsync` baris 152 | Query baca murni tetap `AsNoTracking` tanpa transaksi |

### 2.3 Intake dari Billing

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-008` | Seluruh fakta dari Billing masuk lewat **satu tabel pintu masuk** `FinBillingHandoffIntake`, bukan lewat kolom unik yang ditempel di masing-masing subledger | Status model "Billing handoff intake `NEW → CONSUMED → ACKNOWLEDGED \| ERROR`" pada decision log; `FIN-BIL-005` mewajibkan ACK | Satu tempat untuk memantau dan mengulang intake yang gagal. Tanpa ini, kegagalan intake tersebar di empat subledger |
| `FIN-DES-009` | Kunci idempotensi piutang adalah `SourceHandoffKey`, disalin dari `BilArHandoff.HandoffKey` — **bukan** `BilArHandoff.Id` | `BilArHandoff.HandoffKey` adalah kunci idempotensi milik Billing (terbaca langsung di model, `09101d05`) | Bila Billing membuat ulang baris handoff dengan `HandoffKey` sama, Finance tetap tidak membuat piutang kedua |
| `FIN-DES-010` | Kunci idempotensi penerimaan adalah `SourceTenderId`, dengan partial unique index `WHERE "SourceTenderId" IS NOT NULL AND "IsDelete" = false` | `FIN-BIL-006`; pola partial index `IX_FinPettyCashBudgetMovement_Voucher_Disbursement` | Penerimaan manual (tanpa tender) tetap mungkin karena kolomnya nullable dan index-nya berfilter |
| `FIN-DES-011` | `FinReceipt` **tidak pernah** menciptakan `FinReceivable`. Keduanya fakta terpisah yang hanya dipertemukan oleh `FinReceiptAllocation` | `FIN-DEC` aturan bisnis #11 dan #14; `FIN-BIL-008` | Pasien yang bayar lunas menghasilkan receipt tanpa piutang sama sekali. Inilah pencegahan dobel-hitung yang diminta BRD |
| `FIN-DES-023` | Bentuk `BilCollectionHandoff` dikunci di `contracts/integration-contract.md` sebagai kontrak, tetapi **tabelnya milik Billing dan dibangun tim Billing**. Blueprint ini tidak merancang internalnya | `FIN-DEC-005`; `FIN-OOS-001` | Task pembuatan tabel itu masuk roadmap Billing, bukan roadmap Finance. Finance hanya membangun konsumennya |

### 2.4 Piutang, penerimaan, dan koreksi

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-012` | Setiap pembalikan — penerimaan maupun alokasi — dicatat sebagai **baris baru** yang menetralkan, tidak pernah dengan `UPDATE` atau `DELETE` baris lama | `FIN-DEC-021`; aturan bisnis #8 | Riwayat tetap utuh. Kolom `IsReversal` dan `ReversalOfAllocationId` yang membawa maknanya |
| `FIN-DES-013` | Alokasi penerimaan ke piutang **tidak** memiliki pencocokan otomatis apa pun di service. Staf mengirim daftar alokasi eksplisit | `FIN-DEC-011` memilih manual penuh | Service MUST NOT diam-diam menambahkan FIFO "supaya praktis". Bila kelak diinginkan, itu keputusan bisnis baru |
| `FIN-DES-014` | Maker-checker ditegakkan dua lapis: `HasCheckConstraint` pada database (`"RequestedBy" <> "ApprovedBy"`) dan pemeriksaan di service | `FIN-DEC-012` | Lapis database membuat aturan ini tidak bisa dilanggar walau ada jalur kode lain di kemudian hari |

### 2.5 Utang dan pembayaran

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-015` | Satu entity `FinPayment` melayani pembayaran supplier **dan** dokter, dibedakan kolom `PaymentType` | `FIN-DEC-019` membolehkan rekap; keduanya punya siklus approval yang sama bentuknya | Menghindari dua tabel kembar yang 90% kolomnya identik. Perbedaan perilaku ada di service, bukan di skema |
| `FIN-DES-016` | `FinPaymentAllocation` memakai dua FK nullable (`SupplierPayableId`, `DoctorPayableId`) dengan check constraint "tepat satu terisi dan cocok dengan `PayableType`" | Alternatifnya tabel alokasi terpisah per jenis, yang memecah query rekap | Rekap satu pembayaran ke banyak utang tetap satu query. Check constraint mencegah baris yatim |
| `FIN-DES-024` | Kolom `BenefitOwnerId` dan `BenefitRelationship` **disiapkan** pada `FinReceivable`, tetapi jalur intake-nya berstatus `OPEN DECISION` sampai konfirmasi Billing + HR turun | `FIN-DEC-006`, `FIN-DEC-016`; risiko yang dicatat manifest | Skema tidak perlu diubah lagi saat konfirmasi turun, tetapi rumpun employee benefit MUST NOT masuk gelombang pengiriman mana pun sebelum itu |

### 2.6 Kas

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-021` | Saldo kas yang tersedia untuk disetor **dihitung backend saat posting**, di dalam transaksi `Serializable`, bukan dikirim frontend | `FIN-DEC-017` menyebut eksplisit "validasi wajib dilakukan di backend saat posting agar tidak terjadi double allocation atau saldo minus" | Dua petugas yang menyetor bersamaan tidak bisa menghabiskan saldo yang sama dua kali |
| `FIN-DES-022` | Kas harian disimpan sebagai **baris nyata per tanggal** (`FinDailyCashSnapshot`), bukan view yang dihitung ulang setiap kali dibuka | Posisi kas yang sudah ditutup MUST tidak berubah walau ada transaksi terlambat; view akan diam-diam berubah | Transaksi yang datang setelah penutupan masuk ke tanggal berikutnya dan terlihat sebagai selisih, bukan menimpa angka kemarin |

### 2.7 Integrasi Accounting

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-017` | Baris outbox ditulis **di dalam transaksi bisnis yang sama** dengan fakta yang melahirkannya (transactional outbox) | `FIN-ACC-XMOD-V2-0.3` bagian 10 | Tidak mungkin ada piutang tanpa kejadian, atau kejadian tanpa piutang |
| `FIN-DES-018` | `DeliveryStatus` memiliki nilai khusus `HELD_FOR_FINALIZATION` sebagai status awal untuk kejadian penerimaan yang tagihannya belum `FINAL` | `FIN-DEC-004` | Worker pengirim MUST melewati baris berstatus ini. Pelepasannya dipicu peristiwa finalisasi tagihan, bukan oleh timer |
| `FIN-DES-019` | Dua lapis anti-dobel: unique `EventNumber`, dan unique gabungan (`SourceModule`, `SourceTransactionId`, `EventTypeCode`, `SourceVersion`) | Paket kontrak Accounting bagian 5 menyebut dua lapis itu persis | Koreksi atas transaksi yang sama MUST menaikkan `SourceVersion`, bukan memakai nomor yang sama |
| `FIN-DES-020` | Pengiriman dijalankan `IHostedService` terpisah yang **dimatikan lewat konfigurasi** sampai endpoint Accounting benar-benar ada | `FIN-CAP-018` masih `Missing`; `FIN-MVP-PRD-V2-0.3` menyebut "release blocker: jangan implementasi POST sebelum endpoint ada" | Outbox boleh dibangun dan diisi sekarang. Yang ditahan hanya pengirimannya |

---

## 3. Class diagram per konteks

### 3.1 Billing Intake

```mermaid
classDiagram
    class FinBillingHandoffIntake {
        +Guid Id
        +string HandoffType
        +Guid SourceHandoffId
        +Guid SourceHandoffKey
        +string Status
        +Guid? TargetEntityId
        +int RetryCount
        +string? ErrorMessage
        +Guid RowVersion
    }
    class BilArHandoff {
        +Guid Id
        +string DebtorType
        +Guid HandoffKey
        +string Status
    }
    class BilApHandoff {
        +Guid Id
        +Guid DoctorId
        +string ReadinessStatus
        +Guid HandoffKey
    }
    class BilCollectionHandoff {
        +Guid Id
        +Guid TenderId
        +Guid HandoffKey
    }
    BilArHandoff "1" --> "0..1" FinBillingHandoffIntake : dikonsumsi sebagai
    BilApHandoff "1" --> "0..1" FinBillingHandoffIntake : dikonsumsi sebagai
    BilCollectionHandoff "1" --> "0..1" FinBillingHandoffIntake : dikonsumsi sebagai
```

### 3.2 Receivable

```mermaid
classDiagram
    class FinReceivable {
        +Guid Id
        +string ReceivableNumber
        +Guid SourceHandoffKey
        +string DebtorType
        +Guid? DebtorReferenceId
        +Guid? BenefitOwnerId
        +decimal OriginalAmount
        +decimal OutstandingAmount
        +string Status
        +string ClaimStatus
        +Guid RowVersion
    }
    class FinReceivableItem {
        +Guid Id
        +Guid ReceivableId
        +Guid? PatientId
        +Guid? EncounterId
        +decimal Amount
    }
    class FinReceivableDocument {
        +Guid Id
        +Guid ReceivableId
        +string DocumentType
        +bool IsReceived
    }
    class FinReceivableAdjustment {
        +Guid Id
        +Guid ReceivableId
        +string Direction
        +decimal Amount
        +string Status
        +Guid RequestedBy
        +Guid? ApprovedBy
    }
    class FinReceivableWriteOff {
        +Guid Id
        +Guid ReceivableId
        +decimal Amount
        +string Status
        +Guid RequestedBy
        +Guid? ApprovedBy
    }
    FinReceivable "1" --> "1..*" FinReceivableItem : merinci
    FinReceivable "1" --> "0..*" FinReceivableDocument : dilengkapi
    FinReceivable "1" --> "0..*" FinReceivableAdjustment : dikoreksi
    FinReceivable "1" --> "0..*" FinReceivableWriteOff : dihapusbukukan
```

### 3.3 Collection

```mermaid
classDiagram
    class FinReceipt {
        +Guid Id
        +string ReceiptNumber
        +string SourceType
        +Guid? SourceTenderId
        +Guid? CashierShiftId
        +decimal Amount
        +decimal UnallocatedAmount
        +string Status
        +Guid? ReversalOfReceiptId
        +Guid RowVersion
    }
    class FinReceiptAllocation {
        +Guid Id
        +Guid ReceiptId
        +Guid? ReceivableId
        +string TargetType
        +decimal Amount
        +bool IsReversal
        +Guid? ReversalOfAllocationId
    }
    class FinReceivable {
        +Guid Id
        +decimal OutstandingAmount
        +string Status
    }
    FinReceipt "1" --> "0..*" FinReceiptAllocation : dialokasikan lewat
    FinReceivable "1" --> "0..*" FinReceiptAllocation : dilunasi sebagian oleh
    FinReceipt "1" --> "0..1" FinReceipt : dibalik oleh
```

### 3.4 Payable

```mermaid
classDiagram
    class FinSupplierPayable {
        +Guid Id
        +string PayableNumber
        +Guid SupplierId
        +string SupplierInvoiceNumber
        +decimal OutstandingAmount
        +string Status
        +Guid RowVersion
    }
    class FinDoctorPayable {
        +Guid Id
        +string PayableNumber
        +Guid DoctorId
        +Guid SourceDoctorServiceFeeId
        +decimal OutstandingAmount
        +string Status
        +Guid RowVersion
    }
    class FinPayment {
        +Guid Id
        +string PaymentNumber
        +string PaymentType
        +Guid BankAccountId
        +decimal TotalAmount
        +string Status
        +string ApprovalTier
        +Guid RequestedBy
        +Guid? ApprovedBy
        +Guid RowVersion
    }
    class FinPaymentAllocation {
        +Guid Id
        +Guid PaymentId
        +string PayableType
        +Guid? SupplierPayableId
        +Guid? DoctorPayableId
        +decimal Amount
    }
    class FinPayableAdjustment {
        +Guid Id
        +string PayableType
        +decimal Amount
        +string Status
    }
    FinPayment "1" --> "1..*" FinPaymentAllocation : melunasi lewat
    FinSupplierPayable "1" --> "0..*" FinPaymentAllocation : dilunasi oleh
    FinDoctorPayable "1" --> "0..*" FinPaymentAllocation : dilunasi oleh
    FinSupplierPayable "1" --> "0..*" FinPayableAdjustment : dikoreksi
    FinDoctorPayable "1" --> "0..*" FinPayableAdjustment : dikoreksi
```

### 3.5 Cash Management dan Master Data

```mermaid
classDiagram
    class FinBankDeposit {
        +Guid Id
        +string DepositNumber
        +DateOnly DepositDate
        +Guid BankAccountId
        +decimal Amount
        +Guid? CashierShiftId
        +string Status
        +Guid RowVersion
    }
    class FinDailyCashSnapshot {
        +Guid Id
        +DateOnly CashDate
        +decimal OpeningBalance
        +decimal CashReceiptAmount
        +decimal BankDepositAmount
        +decimal ClosingBalance
        +string Status
        +Guid RowVersion
    }
    class MstBank {
        +Guid Id
        +string BankCode
        +string BankName
        +bool IsActive
    }
    class MstBankAccount {
        +Guid Id
        +Guid BankId
        +string AccountNumber
        +string AccountType
        +bool IsActive
    }
    class MstCurrency {
        +Guid Id
        +string CurrencyCode
        +bool IsBaseCurrency
    }
    class MstExchangeRate {
        +Guid Id
        +Guid CurrencyId
        +DateOnly RateDate
        +decimal MiddleRate
    }
    MstBank "1" --> "0..*" MstBankAccount : memiliki
    MstBankAccount "1" --> "0..*" FinBankDeposit : tujuan setoran
    MstCurrency "1" --> "0..*" MstExchangeRate : dikurskan
```

### 3.6 Accounting Integration

```mermaid
classDiagram
    class FinAccountingEventOutbox {
        +Guid Id
        +string EventNumber
        +string EventTypeCode
        +string SourceTransactionId
        +string SourceVersion
        +decimal Amount
        +DateOnly AccountingDate
        +string DeliveryStatus
        +int AttemptCount
        +string? AccountingJournalNumber
        +Guid RowVersion
    }
    class FinAccountingEventAttempt {
        +Guid Id
        +Guid OutboxId
        +int AttemptNumber
        +int? ResponseCode
        +DateTimeOffset AttemptedAt
    }
    class FinSubledgerPeriodBalance {
        +Guid Id
        +Guid LegalEntityId
        +string AccountingPeriodCode
        +string ControlAccountCode
        +decimal SubledgerBalance
        +string Status
    }
    FinAccountingEventOutbox "1" --> "0..*" FinAccountingEventAttempt : dicoba kirim
```

---

## 4. Penjelasan setiap class

### 4.1 `FinBillingHandoffIntake`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` |
| Kategori | Transaksi — pintu masuk integrasi |
| Tanggung jawab utama | Mencatat satu baris untuk setiap fakta yang dikirim Billing, beserta apakah fakta itu sudah berhasil diolah Finance. Kalau pengolahan gagal, barisnya tetap ada dengan pesan galat, sehingga petugas tahu ada yang tertinggal — bukan hilang diam-diam |
| Field penting | `HandoffType`, `SourceHandoffId`, `SourceHandoffKey`, `Status`, `TargetEntityId`, `RetryCount`, `ErrorMessage`, `CorrelationId` |
| Navigation property dan relasi | Tidak punya navigation ke tabel Billing — hanya menyimpan Id-nya, agar tidak mengunci tabel milik modul lain |
| Pemakaian dalam alur bisnis | Aktif setiap kali Billing memfinalisasi tagihan atau menyelesaikan pembayaran |
| Catatan desain | `SourceHandoffKey` yang jadi kunci idempotensi, bukan `SourceHandoffId` (`FIN-DES-009`). Baris berstatus `ERROR` MUST dapat diulang tanpa membuat baris kedua |
| Ekuivalen model lama | — |

### 4.2 `FinReceivable`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` |
| Kategori | Transaksi — aggregate root piutang |
| Tanggung jawab utama | Menyimpan satu piutang beserta sisa yang belum tertagih. Nilainya disalin dari handoff Billing dan tidak pernah dihitung ulang Finance |
| Field penting | `ReceivableNumber`, `SourceHandoffKey`, `DebtorType`, `DebtorReferenceId`, `BenefitOwnerId`, `OriginalAmount`, `OutstandingAmount`, `AllocatedAmount`, `AdjustedAmount`, `WrittenOffAmount`, `DueDate`, `Status`, `ClaimStatus` |
| Navigation property dan relasi | Punya banyak `FinReceivableItem`, `FinReceivableDocument`, `FinReceivableAdjustment`, `FinReceivableWriteOff`, dan `FinReceiptAllocation` |
| Pemakaian dalam alur bisnis | Dibuat otomatis saat handoff AR dikonsumsi; hidup sampai lunas, dihapusbukukan, atau dibatalkan |
| Catatan desain | Invariant: `OriginalAmount = OutstandingAmount + AllocatedAmount + AdjustedAmount + WrittenOffAmount`. `OutstandingAmount` MUST NOT negatif dan MUST NOT ditulis di luar `FinanceReceivableService`. Kolom benefit disiapkan tapi jalurnya `OPEN DECISION` (`FIN-DES-024`) |
| Ekuivalen model lama | `Fin_ARHeader` pada V1 QuilvianSta |

### 4.3 `FinReceivableItem`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableItem.cs` |
| Kategori | Transaksi — rincian |
| Tanggung jawab utama | Menyimpan rincian pasien dan kunjungan di balik satu piutang, supaya piutang penjamin yang berisi banyak pasien tetap bisa ditelusuri satu per satu |
| Field penting | `ReceivableId`, `PatientId` (**sensitif**), `EncounterId`, `InvoiceId`, `Description`, `Amount` |
| Navigation property dan relasi | Milik `FinReceivable` |
| Pemakaian dalam alur bisnis | Dibuat bersamaan dengan piutangnya |
| Catatan desain | `PatientId` MUST NOT ikut ke payload Accounting (`FIN-DEC` aturan #6). Ia hanya hidup di dalam domain Finance |
| Ekuivalen model lama | `Fin_ARDetail` pada V1 |

### 4.4 `FinReceivableDocument`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableDocument.cs` |
| Kategori | Transaksi — daftar periksa dokumen |
| Tanggung jawab utama | Melacak kelengkapan berkas klaim tanpa pernah mengubah nilai piutang |
| Field penting | `ReceivableId`, `DocumentType`, `DocumentNumber`, `IsReceived`, `ReceivedAt`, `Notes` |
| Navigation property dan relasi | Milik `FinReceivable` |
| Pemakaian dalam alur bisnis | Diisi petugas AR saat menyiapkan penagihan ke penjamin |
| Catatan desain | Kelengkapan dokumen MUST NOT menjadi syarat piutang diakui (`FIN-DEC-013`). Ia hanya memengaruhi `ClaimStatus` |
| Ekuivalen model lama | — |

### 4.5 `FinReceivableAdjustment`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableAdjustment.cs` |
| Kategori | Transaksi — koreksi berjenjang |
| Tanggung jawab utama | Mencatat penambahan atau pengurangan nilai piutang yang diajukan satu petugas dan disetujui petugas lain |
| Field penting | `AdjustmentNumber`, `ReceivableId`, `SourceHandoffAdjustmentId`, `Direction`, `Amount`, `Reason`, `Status`, `RequestedBy`, `RequestedAt`, `ApprovedBy`, `ApprovedAt`, `RejectionReason` |
| Navigation property dan relasi | Milik `FinReceivable`; boleh menunjuk `BilHandoffAdjustment` sebagai sumber |
| Pemakaian dalam alur bisnis | Dipakai saat penjamin menyetujui nilai berbeda dari tagihan awal |
| Catatan desain | `RequestedBy` MUST NOT sama dengan `ApprovedBy`, ditegakkan check constraint **dan** service (`FIN-DES-014`). `OutstandingAmount` piutang hanya berubah saat status menjadi `APPROVED` |
| Ekuivalen model lama | — |

### 4.6 `FinReceivableWriteOff`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableWriteOff.cs` |
| Kategori | Transaksi — penghapusan berjenjang |
| Tanggung jawab utama | Menutup piutang yang sudah dinyatakan tidak tertagih, dengan persetujuan orang kedua |
| Field penting | `WriteOffNumber`, `ReceivableId`, `Amount`, `Reason`, `Status`, `RequestedBy`, `ApprovedBy`, `RejectionReason` |
| Navigation property dan relasi | Milik `FinReceivable` |
| Pemakaian dalam alur bisnis | Akhir hidup piutang yang gagal ditagih |
| Catatan desain | Penghapusan buku MUST NOT menghapus baris piutang. Ia hanya memindahkan nilai dari `OutstandingAmount` ke `WrittenOffAmount` |
| Ekuivalen model lama | Pemutihan Piutang pada V1 |

### 4.7 `FinReceipt`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Collection/Models/FinReceipt.cs` |
| Kategori | Transaksi — aggregate root penerimaan |
| Tanggung jawab utama | Mencatat uang yang benar-benar sudah diterima rumah sakit, baik dari kasir maupun dari penagihan piutang |
| Field penting | `ReceiptNumber`, `SourceType`, `SourceTenderId`, `SettlementId`, `InvoiceId`, `PaymentMethodId`, `PaymentMethodAccountId`, `Amount`, `AllocatedAmount`, `UnallocatedAmount`, `KwitansiNumber`, `CashierShiftId`, `ProviderReference`, `OccurredAt`, `SourceInvoiceStatus`, `Status`, `ReversalOfReceiptId` |
| Navigation property dan relasi | Punya banyak `FinReceiptAllocation`; boleh menunjuk dirinya sendiri lewat `ReversalOfReceiptId` |
| Pemakaian dalam alur bisnis | Dibuat saat tender Billing berhasil, atau saat penjamin membayar piutang |
| Catatan desain | Nilai `Amount` MUST disalin apa adanya dari `BilTender.Amount` — **jangan** dihitung ulang. Satu `SourceTenderId` hanya boleh melahirkan satu receipt (`FIN-DES-010`). Receipt MUST NOT menciptakan piutang (`FIN-DES-011`) |
| Ekuivalen model lama | `ReceivedPayment` pada V1 |

### 4.8 `FinReceiptAllocation`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptAllocation.cs` |
| Kategori | Transaksi — baris alokasi |
| Tanggung jawab utama | Menjelaskan uang pada satu penerimaan dipakai untuk melunasi apa. Inilah yang mencegah satu rupiah dihitung dua kali |
| Field penting | `ReceiptId`, `ReceivableId`, `SourceAllocationId`, `TargetType`, `Amount`, `IsReversal`, `ReversalOfAllocationId`, `AllocatedBy`, `AllocatedAt` |
| Navigation property dan relasi | Milik `FinReceipt`; boleh menunjuk `FinReceivable` |
| Pemakaian dalam alur bisnis | Dibuat otomatis saat receipt berasal dari tender yang sudah punya alokasi Billing, atau dibuat manual oleh staf AR (`FIN-DES-013`) |
| Catatan desain | `ReceivableId` boleh kosong — itu keadaan sah untuk pasien yang bayar lunas tanpa pernah punya piutang. Pembalikan membuat baris baru `IsReversal = true` (`FIN-DES-012`) |
| Ekuivalen model lama | `DetailInvoiceReceived` pada V1 |

### 4.9 `FinSupplierPayable`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` |
| Kategori | Transaksi — aggregate root utang supplier |
| Tanggung jawab utama | Mencatat kewajiban membayar supplier beserta sisa yang belum dibayar |
| Field penting | `PayableNumber`, `SupplierId`, `SupplierInvoiceNumber`, `SupplierInvoiceDate`, `OriginalAmount`, `OutstandingAmount`, `PaidAmount`, `AdjustedAmount`, `DueDate`, `PaymentTermDays`, `Status` |
| Navigation property dan relasi | Menunjuk `MstSupplier` milik Administrator; punya banyak `FinSupplierPayableItem` dan `FinPaymentAllocation` |
| Pemakaian dalam alur bisnis | Diinput manual staf AP (`FIN-DEC-015`) karena modul Purchasing belum ada |
| Catatan desain | Pasangan (`SupplierId`, `SupplierInvoiceNumber`) MUST unik di antara baris yang belum dihapus — inilah satu-satunya penjaga terhadap invoice supplier yang terinput dua kali, karena tidak ada PO yang bisa dicocokkan |
| Ekuivalen model lama | `PembayaranAP` dan `RekapAP` pada V1 |

### 4.10 `FinSupplierPayableItem`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableItem.cs` |
| Kategori | Transaksi — rincian |
| Tanggung jawab utama | Merinci isi satu invoice supplier |
| Field penting | `PayableId`, `Description`, `Quantity`, `UnitPrice`, `Amount` |
| Navigation property dan relasi | Milik `FinSupplierPayable` |
| Pemakaian dalam alur bisnis | Diisi bersamaan saat utang diinput |
| Catatan desain | Jumlah seluruh `Amount` baris MUST sama dengan `OriginalAmount` induknya |
| Ekuivalen model lama | `DetailPembayaranAP` pada V1 |

### 4.11 `FinDoctorPayable`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinDoctorPayable.cs` |
| Kategori | Transaksi — aggregate root utang dokter |
| Tanggung jawab utama | Mencatat kewajiban membayar jasa medis dokter yang fee-nya sudah disetujui Medical Fee |
| Field penting | `PayableNumber`, `DoctorId`, `SourceDoctorServiceFeeId`, `SourceApHandoffId`, `PeriodCode`, `OriginalAmount`, `OutstandingAmount`, `PaidAmount`, `Status`, `RecognizedAt` |
| Navigation property dan relasi | Punya banyak `FinDoctorPayableItem` dan `FinPaymentAllocation` |
| Pemakaian dalam alur bisnis | Dibuat setelah `DoctorServiceFee` berstatus disetujui — **bukan** saat `BilApHandoff` datang |
| Catatan desain | `SourceDoctorServiceFeeId` MUST unik: satu fee disetujui menghasilkan paling banyak satu utang. `BilApHandoff` hanya rujukan kesiapan, bukan sumber nilai (`FIN-DEC-003`, Opsi B) |
| Ekuivalen model lama | AP Dokter / Jasa Medis pada V1 |

### 4.12 `FinDoctorPayableItem`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinDoctorPayableItem.cs` |
| Kategori | Transaksi — rincian |
| Tanggung jawab utama | Merinci layanan apa saja yang membentuk satu utang dokter |
| Field penting | `PayableId`, `SourceServiceFeeDetailId`, `Description`, `Amount` |
| Navigation property dan relasi | Milik `FinDoctorPayable` |
| Pemakaian dalam alur bisnis | Dibuat bersamaan dengan utangnya, disalin dari rincian fee |
| Catatan desain | Finance MUST NOT menghitung ulang nilai per layanan; ia menyalin |
| Ekuivalen model lama | — |

### 4.13 `FinPayment`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` |
| Kategori | Transaksi — aggregate root pembayaran keluar |
| Tanggung jawab utama | Satu perintah bayar, yang boleh melunasi banyak utang sekaligus dalam bentuk rekap |
| Field penting | `PaymentNumber`, `PaymentType`, `PayeeReferenceId`, `BankAccountId`, `PaymentMethod`, `TotalAmount`, `AllocatedAmount`, `Status`, `ApprovalTier`, `RequestedBy`, `RequestedAt`, `ApprovedBy`, `ApprovedAt`, `PaidAt`, `ReferenceNumber` |
| Navigation property dan relasi | Menunjuk `MstBankAccount`; punya banyak `FinPaymentAllocation` |
| Pemakaian dalam alur bisnis | Dipakai staf AP untuk membayar supplier, dan untuk membayar rekap fee banyak dokter sekaligus |
| Catatan desain | `TotalAmount` MUST sama dengan jumlah seluruh alokasinya sebelum status boleh menjadi `PAID`. `ApprovalTier` diisi service dari total nominal (`FIN-DEC-022`) — ambangnya masih `FIN-OQ-010` |
| Ekuivalen model lama | `PembayaranAP` pada V1 |

### 4.14 `FinPaymentAllocation`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentAllocation.cs` |
| Kategori | Transaksi — baris alokasi |
| Tanggung jawab utama | Menjelaskan satu pembayaran rekap dipakai melunasi utang yang mana saja, sehingga tiap rupiah tetap tertelusur ke fee atau invoice aslinya |
| Field penting | `PaymentId`, `PayableType`, `SupplierPayableId`, `DoctorPayableId`, `Amount`, `IsReversal`, `ReversalOfAllocationId` |
| Navigation property dan relasi | Milik `FinPayment`; menunjuk salah satu dari dua jenis utang |
| Pemakaian dalam alur bisnis | Inilah yang menjawab keluhan sistem lama: dokter dobel atau lupa dibayar karena rekap dikerjakan di Excel terpisah (`FIN-DEC-019`) |
| Catatan desain | Tepat satu dari `SupplierPayableId`/`DoctorPayableId` terisi, dan MUST cocok dengan `PayableType` — ditegakkan check constraint (`FIN-DES-016`) |
| Ekuivalen model lama | `DetailPembayaranAP` pada V1 |

### 4.15 `FinPayableAdjustment`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs` |
| Kategori | Transaksi — koreksi utang |
| Tanggung jawab utama | Mencatat retur atau koreksi nilai utang tanpa mengubah catatan aslinya |
| Field penting | `AdjustmentNumber`, `PayableType`, `SupplierPayableId`, `DoctorPayableId`, `Direction`, `Amount`, `Reason`, `Status`, `RequestedBy`, `ApprovedBy` |
| Navigation property dan relasi | Menunjuk salah satu dari dua jenis utang |
| Pemakaian dalam alur bisnis | Dipakai saat barang diretur atau nilai fee dikoreksi setelah utang terbentuk |
| Catatan desain | Memakai aturan tepat-satu-FK yang sama dengan `FinPaymentAllocation` |
| Ekuivalen model lama | — |

### 4.16 `FinBankDeposit`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/CashManagement/Models/FinBankDeposit.cs` |
| Kategori | Transaksi — setoran kas ke bank |
| Tanggung jawab utama | Mencatat perpindahan uang tunai dari brankas rumah sakit ke rekening bank |
| Field penting | `DepositNumber`, `DepositDate`, `BankAccountId`, `Amount`, `CashierShiftId`, `DepositSlipNumber`, `Status`, `PostedBy`, `PostedAt` |
| Navigation property dan relasi | Menunjuk `MstBankAccount`; boleh menunjuk shift kasir sebagai rujukan |
| Pemakaian dalam alur bisnis | Dibuat Treasury setelah menghitung uang fisik yang akan disetor |
| Catatan desain | Saat status menjadi `POSTED`, service MUST menghitung ulang saldo tersedia di dalam transaksi `Serializable` dan menolak bila nominal melebihi saldo (`FIN-DES-021`). Setoran sebagian diperbolehkan |
| Ekuivalen model lama | Setoran Bank pada V1 |

### 4.17 `FinDailyCashSnapshot`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/CashManagement/Models/FinDailyCashSnapshot.cs` |
| Kategori | Transaksi — posisi kas harian |
| Tanggung jawab utama | Menyimpan posisi kas satu tanggal: saldo awal, uang masuk, uang keluar, setoran, dan saldo akhir |
| Field penting | `CashDate`, `OpeningBalance`, `CashReceiptAmount`, `OtherReceiptAmount`, `DisbursementAmount`, `BankDepositAmount`, `ClosingBalance`, `Status`, `ClosedBy`, `ClosedAt` |
| Navigation property dan relasi | Tidak punya FK — angkanya diringkas dari `FinReceipt` dan `FinBankDeposit` saat penutupan |
| Pemakaian dalam alur bisnis | Ditutup Treasury setiap akhir hari |
| Catatan desain | `CashDate` MUST unik. Kas kecil **tidak** masuk perhitungan ini karena kolamnya terpisah (`FIN-DEC-020`, aturan bisnis #15). Baris yang sudah `CLOSED` MUST NOT berubah; transaksi terlambat masuk ke tanggal berikutnya (`FIN-DES-022`) |
| Ekuivalen model lama | Kas Harian pada V1 |

### 4.18 `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` (keempatnya) |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/MasterData/Models/MstBank.cs`, `MstBankAccount.cs`, `MstCurrency.cs`, `MstExchangeRate.cs` |
| Kategori | Data induk milik Finance |
| Tanggung jawab utama | Menyediakan daftar bank, rekening rumah sakit, mata uang, dan kurs yang dipakai pembayaran dan setoran |
| Field penting | `MstBank`: `BankCode`, `BankName`, `SwiftCode`, `IsActive`. `MstBankAccount`: `BankId`, `AccountNumber`, `AccountName`, `AccountType`, `CurrencyCode`, `IsActive`. `MstCurrency`: `CurrencyCode`, `CurrencyName`, `Symbol`, `DecimalPlaces`, `IsBaseCurrency`, `IsActive`. `MstExchangeRate`: `CurrencyId`, `RateDate`, `BuyRate`, `SellRate`, `MiddleRate`, `Source` |
| Navigation property dan relasi | `MstBank` punya banyak `MstBankAccount`; `MstCurrency` punya banyak `MstExchangeRate` |
| Pemakaian dalam alur bisnis | Dipilih saat membuat pembayaran keluar dan saat mencatat setoran bank |
| Catatan desain | Memakai prefix `Mst`, bukan `Fin` (`FIN-DES-003`). Kurs hanya dipakai operasional; kejadian ke Accounting tetap IDR saja (`FIN-DEC-018`, aturan bisnis #7). Rekening yang sudah dipakai MUST dinonaktifkan, bukan dihapus — mengikuti pola `MstPettyCashCategory` |
| Ekuivalen model lama | Master Bank / Bank Account pada V1 |

### 4.19 `FinAccountingEventOutbox`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` |
| Kategori | Transaksi — kotak keluar integrasi |
| Tanggung jawab utama | Menyimpan salinan persis pesan yang akan dikirim ke Accounting, beserta status pengirimannya |
| Field penting | `EventNumber`, `EventTypeCode`, `SourceModule`, `SourceTransactionId`, `SourceVersion`, `EventOccurredAt`, `AccountingDate`, `Amount`, `CurrencyCode`, `LegalEntityId`, `CorrelationId`, `CausationId`, `ComponentsJson`, `PayloadJson`, `DeliveryStatus`, `HoldReason`, `AttemptCount`, `LastAttemptAt`, `LastResponseCode`, `AccountingReceiptNumber`, `AccountingJournalNumber` |
| Navigation property dan relasi | Punya banyak `FinAccountingEventAttempt` |
| Pemakaian dalam alur bisnis | Ditulis otomatis setiap kali Finance mengakui piutang, menerima uang, mengakui utang, atau membayar |
| Catatan desain | Ditulis di transaksi yang sama dengan fakta bisnisnya (`FIN-DES-017`). Dua lapis unique melindungi dari jurnal ganda (`FIN-DES-019`). `PayloadJson` MUST NOT memuat `PatientId`, nomor rekam medis, maupun `DoctorId` (aturan bisnis #6) |
| Ekuivalen model lama | — |

### 4.20 `FinAccountingEventAttempt`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventAttempt.cs` |
| Kategori | Transaksi — jejak percobaan kirim |
| Tanggung jawab utama | Menyimpan riwayat setiap percobaan pengiriman, supaya kegagalan berulang bisa ditelusuri |
| Field penting | `OutboxId`, `AttemptNumber`, `AttemptedAt`, `ResponseCode`, `ResponseBody`, `DurationMs`, `ErrorMessage` |
| Navigation property dan relasi | Milik `FinAccountingEventOutbox` |
| Pemakaian dalam alur bisnis | Diisi worker pengiriman |
| Catatan desain | Baris ini append-only. `ResponseBody` MUST dipotong panjangnya agar tidak menggelembungkan tabel |
| Ekuivalen model lama | — |

### 4.21 `FinSubledgerPeriodBalance`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinSubledgerPeriodBalance.cs` |
| Kategori | Transaksi — saldo tutup periode |
| Tanggung jawab utama | Menyimpan saldo akhir subledger per akun kontrol yang dikirim ke Accounting saat tutup periode |
| Field penting | `LegalEntityId`, `AccountingPeriodCode`, `ControlAccountCode`, `SubledgerBalance`, `AsOfDate`, `Status`, `SubmittedAt` |
| Navigation property dan relasi | — |
| Pemakaian dalam alur bisnis | Dibuat saat Finance menutup periode, sebelum Accounting menutup bukunya |
| Catatan desain | Nama field mengikuti draf `FIN-DEC-023` yang **belum dikonfirmasi Accounting** (`FIN-OQ-011`). Karena itu rumpun ini `POST-MVP` |
| Ekuivalen model lama | — |

### 4.22 Service

| Service | Status | Lokasi file | Fungsi utama | Dipanggil oleh | Membuka transaksi |
|---|---|---|---|---|:---:|
| `FinanceBillingIntakeService` | `Baru` | `BillingIntake/Services/FinanceBillingIntakeService.cs` | Membaca handoff Billing, membuat piutang/utang/penerimaan yang sesuai, lalu menandai ACK | `FinanceBillingIntakeController`, hosted job | Ya — `Serializable` |
| `FinanceReceivableService` | `Baru` | `Receivable/Services/FinanceReceivableService.cs` | Satu-satunya penulis `OutstandingAmount` piutang; mengurus aging, koreksi, write-off | `FinanceReceivablesController` | Ya — `Serializable` |
| `FinanceReceiptService` | `Baru` | `Collection/Services/FinanceReceiptService.cs` | Membuat penerimaan dari tender, mengalokasikan manual, dan membalik penerimaan | `FinanceReceiptsController`, `FinanceBillingIntakeService` | Ya — `Serializable` |
| `FinanceSupplierPayableService` | `Baru` | `Payable/Services/FinanceSupplierPayableService.cs` | Input manual utang supplier dan koreksinya | `FinanceSupplierPayablesController` | Ya |
| `FinanceDoctorPayableService` | `Baru` | `Payable/Services/FinanceDoctorPayableService.cs` | Membentuk utang dokter dari fee yang sudah disetujui | `FinanceDoctorPayablesController` | Ya |
| `FinancePaymentService` | `Baru` | `Payable/Services/FinancePaymentService.cs` | Menyusun pembayaran rekap, menegakkan approval berjenjang, mengalokasikan ke banyak utang | `FinancePaymentsController` | Ya — `Serializable` |
| `FinanceCashManagementService` | `Baru` | `CashManagement/Services/FinanceCashManagementService.cs` | Menghitung saldo kas tersedia, memposting setoran, menutup kas harian | `FinanceBankDepositsController`, `FinanceDailyCashController` | Ya — `Serializable` |
| `FinanceMasterDataService` | `Baru` | `MasterData/Services/FinanceMasterDataService.cs` | CRUD bank, rekening, mata uang, kurs | `FinanceBanksController` dan tiga controller master lain | Tidak — CRUD sederhana |
| `FinanceAccountingOutboxService` | `Baru` | `AccountingIntegration/Services/FinanceAccountingOutboxService.cs` | Menulis baris outbox di dalam transaksi pemanggil; melepas status tertahan saat tagihan final | Seluruh service Finance lain | Tidak membuka sendiri — **ikut** transaksi pemanggil (`FIN-DES-017`) |
| `FinanceAccountingDeliveryWorker` | `Baru` | `AccountingIntegration/Services/FinanceAccountingDeliveryWorker.cs` | `IHostedService` yang mengirim outbox ke Accounting dan mencatat percobaannya | Runtime | Ya, per baris |

`FinanceAccountingOutboxService` sengaja **tidak** membuka transaksi sendiri. Kalau ia membuka
transaksi terpisah, akan ada celah waktu ketika piutang sudah tersimpan tetapi kejadiannya
belum — persis masalah yang hendak dicegah pola outbox.

### 4.23 Controller

| Controller | Status | Lokasi file | Service yang dipakai | Grup Swagger |
|---|---|---|---|---|
| `FinanceBillingIntakeController` | `Baru` | `BillingIntake/Controllers/FinanceBillingIntakeController.cs` | `FinanceBillingIntakeService` | `Corporate / Finance Management / Billing Intake` |
| `FinanceReceivablesController` | `Baru` | `Receivable/Controllers/FinanceReceivablesController.cs` | `FinanceReceivableService` | `Corporate / Finance Management / Receivable` |
| `FinanceReceiptsController` | `Baru` | `Collection/Controllers/FinanceReceiptsController.cs` | `FinanceReceiptService` | `Corporate / Finance Management / Receipt` |
| `FinanceSupplierPayablesController` | `Baru` | `Payable/Controllers/FinanceSupplierPayablesController.cs` | `FinanceSupplierPayableService` | `Corporate / Finance Management / Supplier Payable` |
| `FinanceDoctorPayablesController` | `Baru` | `Payable/Controllers/FinanceDoctorPayablesController.cs` | `FinanceDoctorPayableService` | `Corporate / Finance Management / Doctor Payable` |
| `FinancePaymentsController` | `Baru` | `Payable/Controllers/FinancePaymentsController.cs` | `FinancePaymentService` | `Corporate / Finance Management / Payment` |
| `FinanceBankDepositsController` | `Baru` | `CashManagement/Controllers/FinanceBankDepositsController.cs` | `FinanceCashManagementService` | `Corporate / Finance Management / Bank Deposit` |
| `FinanceDailyCashController` | `Baru` | `CashManagement/Controllers/FinanceDailyCashController.cs` | `FinanceCashManagementService` | `Corporate / Finance Management / Daily Cash` |
| `FinanceBanksController` | `Baru` | `MasterData/Controllers/FinanceBanksController.cs` | `FinanceMasterDataService` | `Corporate / Finance Management / Master Data / Bank` |
| `FinanceBankAccountsController` | `Baru` | `MasterData/Controllers/FinanceBankAccountsController.cs` | `FinanceMasterDataService` | `Corporate / Finance Management / Master Data / Bank Account` |
| `FinanceCurrenciesController` | `Baru` | `MasterData/Controllers/FinanceCurrenciesController.cs` | `FinanceMasterDataService` | `Corporate / Finance Management / Master Data / Currency` |
| `FinanceAccountingEventsController` | `Baru` | `AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | `FinanceAccountingOutboxService` | `Corporate / Finance Management / Accounting Event Monitor` |
| `PettyCashBudgetController` | `Sudah ada` | `PettyCash/Controllers/PettyCashBudgetController.cs` | `PettyCashBudgetService` | `Corporate / Finance Management / Petty Cash / Budget` |
| `PettyCashCategoriesController` | `Sudah ada` | `MasterData/Controllers/PettyCashCategoriesController.cs` | `PettyCashCategoryService` | `Corporate / Finance Management / Master Data / Petty Cash Category` |

Seluruh controller baru memakai **satu** `[Route]` kanonik `api/v1/corporate/finance-management/...`.
Route alias `health-services/billing-management/...` yang ada pada dua controller Petty Cash
**MUST NOT** ditiru — alias itu warisan sejarah perpindahan folder, bukan pola yang berlaku.

---

## 5. Arsitektur folder

```text
Areas/Corporate/FinanceManagement/
├── BillingIntake/                      # BARU — wajib didaftarkan registry (FIN-DES-002)
│   ├── Controllers/FinanceBillingIntakeController.cs        # Baru
│   ├── Dtos/FinanceBillingIntakeDtos.cs                     # Baru
│   ├── Models/FinBillingHandoffIntake.cs                    # Baru
│   └── Services/FinanceBillingIntakeService.cs              # Baru
├── Receivable/                         # BARU
│   ├── Controllers/FinanceReceivablesController.cs          # Baru
│   ├── Dtos/FinanceReceivableDtos.cs                        # Baru
│   ├── Models/FinReceivable.cs                              # Baru
│   ├── Models/FinReceivableItem.cs                          # Baru
│   ├── Models/FinReceivableDocument.cs                      # Baru
│   ├── Models/FinReceivableAdjustment.cs                    # Baru
│   ├── Models/FinReceivableWriteOff.cs                      # Baru
│   └── Services/FinanceReceivableService.cs                 # Baru
├── Collection/                         # BARU
│   ├── Controllers/FinanceReceiptsController.cs             # Baru
│   ├── Dtos/FinanceReceiptDtos.cs                           # Baru
│   ├── Models/FinReceipt.cs                                 # Baru
│   ├── Models/FinReceiptAllocation.cs                       # Baru
│   └── Services/FinanceReceiptService.cs                    # Baru
├── Payable/                            # BARU
│   ├── Controllers/FinanceSupplierPayablesController.cs     # Baru
│   ├── Controllers/FinanceDoctorPayablesController.cs       # Baru
│   ├── Controllers/FinancePaymentsController.cs             # Baru
│   ├── Dtos/FinancePayableDtos.cs                           # Baru
│   ├── Dtos/FinancePaymentDtos.cs                           # Baru
│   ├── Models/FinSupplierPayable.cs                         # Baru
│   ├── Models/FinSupplierPayableItem.cs                     # Baru
│   ├── Models/FinDoctorPayable.cs                           # Baru
│   ├── Models/FinDoctorPayableItem.cs                       # Baru
│   ├── Models/FinPayment.cs                                 # Baru
│   ├── Models/FinPaymentAllocation.cs                       # Baru
│   ├── Models/FinPayableAdjustment.cs                       # Baru
│   ├── Services/FinanceSupplierPayableService.cs            # Baru
│   ├── Services/FinanceDoctorPayableService.cs              # Baru
│   └── Services/FinancePaymentService.cs                    # Baru
├── CashManagement/                     # BARU
│   ├── Controllers/FinanceBankDepositsController.cs         # Baru
│   ├── Controllers/FinanceDailyCashController.cs            # Baru
│   ├── Dtos/FinanceCashManagementDtos.cs                    # Baru
│   ├── Models/FinBankDeposit.cs                             # Baru
│   ├── Models/FinDailyCashSnapshot.cs                       # Baru
│   └── Services/FinanceCashManagementService.cs             # Baru
├── AccountingIntegration/              # BARU
│   ├── Controllers/FinanceAccountingEventsController.cs     # Baru
│   ├── Dtos/FinanceAccountingEventDtos.cs                   # Baru
│   ├── Models/FinAccountingEventOutbox.cs                   # Baru
│   ├── Models/FinAccountingEventAttempt.cs                  # Baru
│   ├── Models/FinSubledgerPeriodBalance.cs                  # Baru
│   ├── Services/FinanceAccountingOutboxService.cs           # Baru
│   └── Services/FinanceAccountingDeliveryWorker.cs          # Baru
├── MasterData/                         # SUDAH ADA — bertambah isi
│   ├── Controllers/PettyCashCategoriesController.cs         # Sudah ada
│   ├── Controllers/FinanceBanksController.cs                # Baru
│   ├── Controllers/FinanceBankAccountsController.cs         # Baru
│   ├── Controllers/FinanceCurrenciesController.cs           # Baru
│   ├── DTOs/PettyCashCategoryDtos.cs                        # Sudah ada
│   ├── DTOs/FinanceMasterDataDtos.cs                        # Baru
│   ├── Models/MstPettyCashCategory.cs                       # Sudah ada
│   ├── Models/MstBank.cs                                    # Baru
│   ├── Models/MstBankAccount.cs                             # Baru
│   ├── Models/MstCurrency.cs                                # Baru
│   ├── Models/MstExchangeRate.cs                            # Baru
│   ├── Services/PettyCashCategoryService.cs                 # Sudah ada
│   └── Services/FinanceMasterDataService.cs                 # Baru
└── PettyCash/                          # SUDAH ADA — TIDAK disentuh pass ini
    ├── Controllers/PettyCashBudgetController.cs             # Sudah ada
    ├── Dtos/PettyCashBudgetDtos.cs                          # Sudah ada
    ├── Models/FinPettyCashBudget.cs                         # Sudah ada
    ├── Models/FinPettyCashBudgetMovement.cs                 # Sudah ada
    └── Services/PettyCashBudgetService.cs                   # Sudah ada

Repositories/Configurations/Corporate/FinanceManagement/
├── BillingIntake/FinBillingHandoffIntakeConfiguration.cs    # Baru
├── Receivable/FinReceivableConfiguration.cs                 # Baru
├── Receivable/FinReceivableItemConfiguration.cs             # Baru
├── Receivable/FinReceivableDocumentConfiguration.cs         # Baru
├── Receivable/FinReceivableAdjustmentConfiguration.cs       # Baru
├── Receivable/FinReceivableWriteOffConfiguration.cs         # Baru
├── Collection/FinReceiptConfiguration.cs                    # Baru
├── Collection/FinReceiptAllocationConfiguration.cs          # Baru
├── Payable/FinSupplierPayableConfiguration.cs               # Baru
├── Payable/FinSupplierPayableItemConfiguration.cs           # Baru
├── Payable/FinDoctorPayableConfiguration.cs                 # Baru
├── Payable/FinDoctorPayableItemConfiguration.cs             # Baru
├── Payable/FinPaymentConfiguration.cs                       # Baru
├── Payable/FinPaymentAllocationConfiguration.cs             # Baru
├── Payable/FinPayableAdjustmentConfiguration.cs             # Baru
├── CashManagement/FinBankDepositConfiguration.cs            # Baru
├── CashManagement/FinDailyCashSnapshotConfiguration.cs      # Baru
├── AccountingIntegration/FinAccountingEventOutboxConfiguration.cs   # Baru
├── AccountingIntegration/FinAccountingEventAttemptConfiguration.cs  # Baru
├── AccountingIntegration/FinSubledgerPeriodBalanceConfiguration.cs  # Baru
├── MasterData/MstPettyCashCategoryConfiguration.cs          # Sudah ada
├── MasterData/MstBankConfiguration.cs                       # Baru
├── MasterData/MstBankAccountConfiguration.cs                # Baru
├── MasterData/MstCurrencyConfiguration.cs                   # Baru
├── MasterData/MstExchangeRateConfiguration.cs               # Baru
└── PettyCash/                                               # Sudah ada, tidak disentuh
```

**Catatan utang teknis yang MUST NOT ditiru:** dua controller Petty Cash memiliki `[Route]`
ganda (kanonik `corporate/finance-management` dan alias `health-services/billing-management`).
Alias itu peninggalan perpindahan folder 17 September 2026 dan hanya sah untuk kedua controller
tersebut. Controller baru memakai satu route kanonik saja. Perapian alias itu, bila kelak
diinginkan, MUST menjadi task tersendiri dengan approval pemilik modul — bukan dirapikan
diam-diam di tengah task lain.

### 5.1 Pendaftaran di luar folder modul

| Berkas | Perubahan | Alasan |
|---|---|---|
| `Repositories/ApplicationDbContext.cs` | Tambah 21 `DbSet<T>` baru | Pola existing: `DbSet<FinPettyCashBudget>` baris 619 |
| `Program.cs` | Tambah 9 `builder.Services.AddScoped<T>()` dan 1 `AddHostedService` | Pola existing: `AddScoped<PettyCashBudgetService>()` baris 732 |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Tambah baris untuk enam submodul baru | `QBE-MOD-003` — **MUST** dilakukan sebelum file model pertama ditulis |

---

## 6. Status model dan dampak migration

| Model | Status | Kolom yang berubah | Dampak migration |
|---|---|---|---|
| `FinBillingHandoffIntake` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceivable` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceivableItem` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceivableDocument` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceivableAdjustment` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceivableWriteOff` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceipt` | `Baru` | Seluruh kolom | Tabel baru |
| `FinReceiptAllocation` | `Baru` | Seluruh kolom | Tabel baru |
| `FinSupplierPayable` | `Baru` | Seluruh kolom | Tabel baru |
| `FinSupplierPayableItem` | `Baru` | Seluruh kolom | Tabel baru |
| `FinDoctorPayable` | `Baru` | Seluruh kolom | Tabel baru |
| `FinDoctorPayableItem` | `Baru` | Seluruh kolom | Tabel baru |
| `FinPayment` | `Baru` | Seluruh kolom | Tabel baru |
| `FinPaymentAllocation` | `Baru` | Seluruh kolom | Tabel baru |
| `FinPayableAdjustment` | `Baru` | Seluruh kolom | Tabel baru |
| `FinBankDeposit` | `Baru` | Seluruh kolom | Tabel baru |
| `FinDailyCashSnapshot` | `Baru` | Seluruh kolom | Tabel baru |
| `MstBank` | `Baru` | Seluruh kolom | Tabel baru |
| `MstBankAccount` | `Baru` | Seluruh kolom | Tabel baru |
| `MstCurrency` | `Baru` | Seluruh kolom | Tabel baru |
| `MstExchangeRate` | `Baru` | Seluruh kolom | Tabel baru |
| `FinAccountingEventOutbox` | `Baru` | Seluruh kolom | Tabel baru |
| `FinAccountingEventAttempt` | `Baru` | Seluruh kolom | Tabel baru |
| `FinSubledgerPeriodBalance` | `Baru` | Seluruh kolom | Tabel baru |
| `BilArHandoff` | `Diperbarui` — **milik Billing** | Tambah `BenefitOwnerId uuid NULL`, tambah `BenefitRelationship varchar(30) NULL`, tambah nilai `EMPLOYEE_BENEFIT` pada check constraint `DebtorType` | Dua kolom nullable, aman tanpa pengisian data lama. **Migration ini milik tim Billing**, bukan Finance |
| `BilCollectionHandoff` | `Baru` — **milik Billing** | Seluruh kolom | Tabel baru, dibangun tim Billing (`FIN-DES-023`) |
| `FinPettyCashBudget` | `Sudah ada` | Tidak ada | Tidak disentuh |
| `FinPettyCashBudgetMovement` | `Sudah ada` | Tidak ada | Tidak disentuh |
| `MstPettyCashCategory` | `Sudah ada` | Tidak ada | Tidak disentuh |
| `MstSupplier` | `Sudah ada` — milik Administrator | Tidak ada | Dipakai apa adanya (`FIN-DEC-014`) |

## 7. Rencana migration

Seluruh migration di bawah **memerlukan otorisasi terpisah** untuk dibuat maupun dijalankan
(`AGENTS.md`, bagian Keselamatan Database). Desain ini tidak memberi wewenang itu.

| Urut | Nama migration | Isi | Tanpa downtime | Pengisian data lama | Langkah mundur |
|---:|---|---|:---:|---|---|
| 1 | `AddFinanceMasterData` | `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate` | Ya | Tidak ada — tabel baru kosong | `Down()` menghapus empat tabel; belum ada yang menunjuknya |
| 2 | `AddFinanceReceivableAndCollection` | 5 tabel piutang + 2 tabel penerimaan | Ya | Tidak ada | `Down()` menghapus tujuh tabel |
| 3 | `AddFinanceBillingIntake` | `FinBillingHandoffIntake` | Ya | Tidak ada | `Down()` menghapus satu tabel |
| 4 | `AddFinanceAccountingOutbox` | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` | Ya | Tidak ada | `Down()` menghapus dua tabel |
| 5 | `AddFinancePayable` | 7 tabel utang dan pembayaran | Ya | Tidak ada | `Down()` menghapus tujuh tabel |
| 6 | `AddFinanceCashManagement` | `FinBankDeposit`, `FinDailyCashSnapshot` | Ya | Tidak ada | `Down()` menghapus dua tabel |
| 7 | `AddFinanceSubledgerPeriodBalance` | `FinSubledgerPeriodBalance` | Ya | Tidak ada | `Down()` menghapus satu tabel |
| — | `ExtendBilArHandoffWithEmployeeBenefit` | Dua kolom nullable pada `BilArHandoff` + perluasan check constraint | Ya | Baris lama tetap sah dengan kedua kolom `NULL` | `Down()` menghapus dua kolom dan mengembalikan check constraint |

Migration nomor 1 sampai 7 seluruhnya **aditif**: tidak ada tabel existing yang diubah, tidak
ada kolom yang dihapus, dan tidak ada data yang dipindahkan. Karena itu urutannya boleh
dipecah menjadi beberapa rilis tanpa saling mengunci, asalkan nomor 1 mendahului nomor 5 dan 6
(keduanya menunjuk `MstBankAccount`).

Migration terakhir **bukan milik Finance**. Ia menyentuh tabel Billing dan MUST dikerjakan tim
Billing setelah `FIN-DEC-006` dikonfirmasi.

## 8. Rencana data master awal

Modul dengan master kosong tidak dapat dipakai sama sekali. Isi minimum berikut MUST ada
sebelum fitur diaktifkan.

| Master | Isi minimum | Sumber nilai |
|---|---|---|
| `MstCurrency` | Sekurang-kurangnya satu baris `IDR` dengan `IsBaseCurrency = true`, `DecimalPlaces = 2` | ISO 4217; kontrak Accounting hanya menerima IDR (`FIN-DEC-018`) |
| `MstBank` | Bank tempat rumah sakit membuka rekening operasional | Daftar rekening rumah sakit dari Treasury |
| `MstBankAccount` | Sekurang-kurangnya satu rekening bertipe `COLLECTION` (tujuan setoran kasir) dan satu bertipe `PAYMENT` (sumber pembayaran keluar) | Treasury. Tanpa keduanya, setoran bank dan pembayaran AP tidak bisa dijalankan |
| `MstExchangeRate` | Boleh kosong saat mulai | Hanya dipakai bila ada transaksi non-IDR |
| `MstPettyCashCategory` | Sudah terisi `TRANSPORT`, `OPERASIONAL`, `KONSUMSI`, `MAINTENANCE`, `ATK` | Sudah ada di source, tidak perlu tindakan |
| `MstSupplier` | Supplier yang invoice-nya akan diinput | Milik Administrator; Finance hanya memakai |
| `AccEventType` di Accounting | **24 kode kejadian** — 17 dari `FIN-DEC-002` (sudah diratifikasi Accounting 24 September 2026) ditambah tujuh dari AMENDMENT REVISI 3 yang masih menunggu ratifikasi (`FIN-OQ-017`) | **Milik Accounting.** Kejadian berjenis yang belum terdaftar akan dijawab `422` `EVENT_TYPE_NOT_REGISTERED` dan berstatus Tertahan di sisi Accounting |

Nilai seperti tipe rekening dan kode mata uang **MUST** berasal dari master, **MUST NOT**
di-hardcode di controller maupun frontend.

## 9. Invariant dan batas transaksi

| Invariant | Ditegakkan di mana | Bila dilanggar |
|---|---|---|
| `FinReceivable.OriginalAmount = OutstandingAmount + AllocatedAmount + AdjustedAmount + WrittenOffAmount` | Service, di dalam transaksi `Serializable` | Transaksi dibatalkan; tidak ada baris tersimpan sebagian |
| `FinReceivable.OutstandingAmount >= 0` | Check constraint database + service | `422` |
| Satu `SourceTenderId` → paling banyak satu `FinReceipt` | Partial unique index | `409` |
| Satu `SourceHandoffKey` → paling banyak satu `FinReceivable` | Unique index | Intake ditandai sudah dikonsumsi, tidak membuat baris kedua |
| Satu `SourceDoctorServiceFeeId` → paling banyak satu `FinDoctorPayable` | Unique index | `409` |
| `FinReceipt.Amount = AllocatedAmount + UnallocatedAmount` | Service | Transaksi dibatalkan |
| `RequestedBy <> ApprovedBy` pada adjustment, write-off, dan payment | Check constraint + service | `422` |
| `FinPayment.TotalAmount = Σ FinPaymentAllocation.Amount` sebelum status `PAID` | Service | `422` |
| `FinPaymentAllocation`: tepat satu FK utang terisi sesuai `PayableType` | Check constraint | Baris ditolak database |
| `FinBankDeposit.Amount <= saldo kas tersedia` saat posting | Service, dihitung ulang di dalam transaksi `Serializable` | `422` |
| `FinDailyCashSnapshot.CashDate` unik | Unique index | `409` |
| `FinAccountingEventOutbox.EventNumber` unik | Unique index | Pengiriman ulang dianggap duplikat, aman |
| (`SourceModule`, `SourceTransactionId`, `EventTypeCode`, `SourceVersion`) unik | Unique index | Melindungi dari jurnal ganda walau nomor kejadian berbeda |
| `CurrencyCode = 'IDR'` pada seluruh baris outbox | Check constraint | Kontrak Accounting hanya menerima IDR (aturan bisnis #7) |

**Batas transaksi.** Satu transaksi database mencakup: fakta bisnisnya, pembaruan saldo
induknya, dan baris outbox-nya. Ketiganya berhasil bersama atau gagal bersama. Pengiriman ke
Accounting berada **di luar** transaksi itu — ia pekerjaan worker terpisah yang membaca outbox.

## 10. Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| `FinPractitionerPayable` terpisah dari `FinDoctorPayable` | Dua tabel yang hampir identik beserta dua jalur pembayaran yang harus dijaga konsisten. Diganti satu entity dengan kolom jenis penerima — lihat amendment revisi 2, `FIN-DES-025` |
| `FinSupplier` | Supplier sudah dimiliki Administrator (`MstSupplier`) dan field-nya sudah memuat termin pembayaran, limit kredit, dan rekening bank. `FIN-DEC-014` menutup ini |
| `FinPatient`, `FinDoctor` | Identitas pasien dan dokter milik modul lain. Finance cukup menyimpan Id-nya |
| `FinChartOfAccount`, `FinJournal` | COA dan jurnal milik Accounting. Menyalinnya akan melahirkan dua buku besar yang pasti berbeda setelah revisi pertama |
| `FinCashierShift` | Shift kasir milik Billing. Finance hanya menyimpan `CashierShiftId` untuk rekonsiliasi (aturan bisnis #12) |
| Tabel alokasi terpisah `FinSupplierPaymentAllocation` dan `FinDoctorPaymentAllocation` | Memecah query rekap menjadi dua dan menduplikasi logika pembalikan. Diganti satu tabel dengan check constraint (`FIN-DES-016`) |
| `FinDoctorServiceFee` | Perhitungan fee milik Medical Fee. Finance hanya menerima hasil yang sudah disetujui (`FIN-DEC-003`, Opsi B) |
| View SQL untuk kas harian | Angka yang sudah ditutup akan diam-diam berubah bila ada transaksi terlambat. Diganti tabel snapshot (`FIN-DES-022`) |
| Pencocokan otomatis FIFO pada alokasi piutang | `FIN-DEC-011` memilih manual penuh. Menambahkannya "supaya praktis" adalah keputusan bisnis baru, bukan keputusan teknis |
| Tabel `FinAgingBucket` | Aging diturunkan dari `DueDate` dan `OutstandingAmount` saat query. Menyimpannya berarti harus menghitung ulang setiap hari dan berisiko basi |
| Endpoint Finance yang menerima kejadian dari Accounting | Kontrak `ACC-XMOD-0.2` satu arah. Accounting adalah muara dan tidak menerbitkan kejadian balik |

---

# AMENDMENT REVISI 2 — Rumpun Payable

| Field | Nilai |
|---|---|
| Revisi | `2` |
| Tanggal | 20 September 2026 |
| Status | `approved` — `FIN-DES-025`..`028` disetujui Yasmin 20 September 2026 |
| Dipicu oleh | `MF-DEC-002`, `MF-DEC-005`, `MF-DEC-008` pada blueprint `medical-fee` |
| Menutup | `FIN-OQ-013`, `FIN-OQ-014` |
| Yang TIDAK disentuh | Seluruh rumpun MVP: data induk, intake Billing, piutang, penerimaan, alokasi, koreksi, write-off, kotak keluar kejadian. Bagian 1 sampai 10 di atas tetap berlaku apa adanya kecuali yang disebut eksplisit di bawah |

Amendment ini menyentuh `EPIC FIN-08` dan `EPIC FIN-09` yang keduanya `POST-MVP` dan **nol baris
kode**, sehingga tidak ada yang perlu dibongkar.

## A.1 Mengapa amendment ini ada

Dua keputusan modul Medical Fee membuat rancangan revisi 1 tidak lagi cukup:

| Keputusan Medical Fee | Akibatnya pada Finance |
|---|---|
| `MF-DEC-002` — penerima jasa bukan hanya dokter, tetapi juga tenaga kesehatan lain | `FinDoctorPayable` yang namanya dan bentuknya khusus dokter tidak dapat menampungnya |
| `MF-DEC-005` — Medical Fee menyerahkan jasa **kotor**; seluruh potongan diterapkan Finance | `FinPayment` revisi 1 **tidak punya tempat sama sekali** untuk PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, dan iuran |

Praktik yang harus ditampung terdokumentasi pada `evidence/03-referensi-meeting-rs-mmc.md`:
jasa kotor per dokter direkap sebulan, lalu dikurangi dan ditambah beberapa pos sebelum
ditransfer sekali sebagai satu angka bersih.

## A.2 Keputusan arsitektur baru

| ID | Keputusan | Dasar | Konsekuensi |
|---|---|---|---|
| `FIN-DES-025` | `FinDoctorPayable` **digantikan** `FinMedicalServicePayable`. Jenis penerima dibedakan kolom `PayeeType` (`DOCTOR`, `NURSE`, `OTHER_PRACTITIONER`), bukan tabel terpisah per jenis | `MF-DEC-002`, `MF-DEC-008`; mengikuti pola `FIN-DES-015` yang sudah memakai satu `FinPayment` untuk supplier dan dokter | Menghindari tabel kembar. `PayeeReferenceId` menunjuk tenaga medis apa pun, bukan khusus dokter |
| `FIN-DES-026` | Entity baru `FinPaymentDeduction` menyimpan potongan dan tambahan pada satu pembayaran, satu baris per pos | `MF-DEC-005`; pos-pos pada `evidence/03-referensi-meeting-rs-mmc.md` | Potongan bersifat per-penerima-per-periode, jadi melekat pada **pembayaran**, bukan pada utang. Kasbon tidak perlu dibagi ke puluhan baris jasa |
| `FIN-DES-027` | `FinPayment` bertambah `NetTransferAmount`. **Invariant lama tetap berlaku**: `TotalAmount` = jumlah alokasi ke utang. Invariant baru mendampinginya: `NetTransferAmount` = `TotalAmount` − potongan + tambahan | Menjaga `FIN-DES-015` tetap benar alih-alih mengubah arti `TotalAmount` | Dua angka punya makna berbeda dan keduanya dibutuhkan: `TotalAmount` menjawab "berapa utang yang lunas", `NetTransferAmount` menjawab "berapa uang yang keluar" |
| `FIN-DES-028` | Utang jasa tetap lunas sebesar **alokasinya**, bukan sebesar uang yang ditransfer | Potongan adalah urusan antara rumah sakit dan penerima, bukan pengurang kewajiban | Dokter dengan jasa Rp 24.500.000 dan potongan Rp 4.500.000 menerima transfer Rp 20.000.000, tetapi utang jasanya **lunas penuh** Rp 24.500.000 |

`FIN-DES-028` adalah bagian yang paling mudah salah dirancang. Bila potongan diperlakukan
sebagai pengurang utang, utang jasa tidak akan pernah lunas dan sisanya menumpuk selamanya.

## A.3 Contoh berangka

> Dr. Andi (nama samaran) punya tiga utang jasa bulan Agustus 2026: Rp 12.000.000,
> Rp 9.500.000, dan Rp 3.000.000. Totalnya Rp 24.500.000.
>
> Potongan bulan itu: PPh 21 Rp 1.225.000, kasbon Rp 3.000.000, iuran kerohanian Rp 100.000.
> Ada tambahan sitting fee Rp 500.000.
>
> Yang tersimpan:
>
> | Angka | Nilai | Artinya |
> |---|---|---|
> | `TotalAmount` | Rp 24.500.000 | Jumlah utang jasa yang dilunasi |
> | Tiga baris `FinPaymentAllocation` | 12.000.000 + 9.500.000 + 3.000.000 | Utang mana saja yang lunas |
> | Empat baris `FinPaymentDeduction` | −1.225.000, −3.000.000, −100.000, +500.000 | Rincian potongan dan tambahan |
> | `NetTransferAmount` | Rp 20.675.000 | Uang yang benar-benar ditransfer |
>
> Ketiga utang jasa berstatus **lunas penuh**, walaupun yang ditransfer lebih kecil.

## A.4 Class diagram — rumpun Payable sesudah amendment

```mermaid
classDiagram
    class FinMedicalServicePayable {
        +Guid Id
        +string PayableNumber
        +string PayeeType
        +Guid PayeeReferenceId
        +Guid SourceMedicalServiceFeeId
        +string PeriodCode
        +decimal OutstandingAmount
        +string Status
        +Guid RowVersion
    }
    class FinPayment {
        +Guid Id
        +string PaymentType
        +decimal TotalAmount
        +decimal AllocatedAmount
        +decimal DeductionAmount
        +decimal AdditionAmount
        +decimal NetTransferAmount
        +string Status
        +Guid RowVersion
    }
    class FinPaymentAllocation {
        +Guid Id
        +Guid PaymentId
        +string PayableType
        +Guid SupplierPayableId
        +Guid MedicalServicePayableId
        +decimal Amount
    }
    class FinPaymentDeduction {
        +Guid Id
        +Guid PaymentId
        +string DeductionType
        +string Direction
        +decimal Amount
        +string Reason
    }
    FinPayment "1" --> "1..*" FinPaymentAllocation : melunasi lewat
    FinPayment "1" --> "0..*" FinPaymentDeduction : dipotong atau ditambah
    FinMedicalServicePayable "1" --> "0..*" FinPaymentAllocation : dilunasi oleh
```

## A.5 Penjelasan class baru dan berubah

### `FinMedicalServicePayable` — menggantikan `FinDoctorPayable`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` — menggantikan `FinDoctorPayable` yang belum pernah ditulis kodenya |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinMedicalServicePayable.cs` |
| Kategori | Transaksi — aggregate root utang jasa tenaga medis |
| Tanggung jawab utama | Mencatat kewajiban membayar jasa tenaga medis yang hasilnya sudah disetujui modul Medical Fee |
| Field penting | `PayableNumber`, `PayeeType`, `PayeeReferenceId`, `SourceMedicalServiceFeeId`, `PeriodCode`, `OriginalAmount`, `OutstandingAmount`, `PaidAmount`, `AdjustedAmount`, `Status`, `RecognizedAt`, `RowVersion` |
| Navigation property dan relasi | Punya banyak `FinMedicalServicePayableItem` dan `FinPaymentAllocation` |
| Pemakaian dalam alur bisnis | Dibuat setelah hasil jasa disetujui Medical Fee — bukan saat `BilApHandoff` datang |
| Catatan desain | `SourceMedicalServiceFeeId` MUST unik: satu hasil jasa yang disetujui menghasilkan paling banyak satu utang. `PayeeType` menentukan makna `PayeeReferenceId`, dan keduanya **sensitif** |
| Ekuivalen model lama | `FinDoctorPayable` pada revisi 1 — digantikan, belum pernah diimplementasikan |

### `FinPaymentDeduction` — baru

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentDeduction.cs` |
| Kategori | Transaksi — baris potongan dan tambahan |
| Tanggung jawab utama | Merinci mengapa uang yang ditransfer berbeda dari jumlah utang yang dilunasi |
| Field penting | `PaymentId`, `DeductionType`, `Direction`, `Amount`, `Reason`, `ReferenceNumber` |
| Navigation property dan relasi | Milik `FinPayment` |
| Pemakaian dalam alur bisnis | Diisi staf AP saat menyusun rekap pembayaran, sebelum diajukan untuk disetujui |
| Catatan desain | `Direction` membedakan potongan dari tambahan, karena "Atless" di praktik memuat keduanya. Nilai `Amount` selalu positif; arahnya ditentukan `Direction`. Baris ini **tidak pernah** mengurangi `OutstandingAmount` utang (`FIN-DES-028`) |
| Ekuivalen model lama | — |

### `FinPayment` — diperbarui

| Kolom yang ditambahkan | Tipe | Keterangan |
|---|---|---|
| `DeductionAmount` | `decimal(18,2)` | Jumlah seluruh baris berarah potongan |
| `AdditionAmount` | `decimal(18,2)` | Jumlah seluruh baris berarah tambahan |
| `NetTransferAmount` | `decimal(18,2)` | `TotalAmount` − `DeductionAmount` + `AdditionAmount` |

Kolom lain pada `FinPayment` **tidak berubah**.

### `FinPaymentAllocation` dan `FinPayableAdjustment` — diperbarui

Kolom `DoctorPayableId` diganti namanya menjadi `MedicalServicePayableId`, menunjuk
`FinMedicalServicePayable`. Nilai `PayableType` berubah dari `DOCTOR` menjadi
`MEDICAL_SERVICE`. Aturan tepat-satu-FK (`FIN-DES-016`) **tetap berlaku apa adanya**.

## A.6 Jenis potongan yang dikenal

Diturunkan dari praktik pada `evidence/03-referensi-meeting-rs-mmc.md`. Disimpan sebagai
`string` dengan check constraint, mengikuti `FIN-DES-004`.

| `DeductionType` | Arah lazim | Keterangan |
|---|---|---|
| `PPH21` | Potongan | Pajak penghasilan atas jasa |
| `KASBON` | Potongan | Pinjaman yang sudah diambil penerima |
| `PATIENT_DEBT` | Potongan | Hutang pasien yang dijamin dipotong dari honor |
| `SITTING_FEE` | Tambahan | Honor kehadiran |
| `KSO` | Potongan | Kerja sama operasional |
| `IURAN` | Potongan | Iuran kerohanian dan sejenisnya |
| `OTHER` | Keduanya | Wajib mengisi `Reason` |

Daftar ini **MUST** dapat bertambah tanpa perubahan skema — hanya check constraint yang
diperbarui.

## A.7 Status model sesudah amendment

| Model | Status | Kolom yang berubah | Dampak migration |
|---|---|---|---|
| `FinMedicalServicePayable` | `Baru` | Seluruh kolom | Menggantikan rencana tabel `FinDoctorPayable` |
| `FinMedicalServicePayableItem` | `Baru` | Seluruh kolom | Menggantikan rencana `FinDoctorPayableItem` |
| `FinPaymentDeduction` | `Baru` | Seluruh kolom | Tabel baru |
| `FinPayment` | `Diperbarui` | Tambah `DeductionAmount`, `AdditionAmount`, `NetTransferAmount` | Tiga kolom uang, bawaan nol |
| `FinPaymentAllocation` | `Diperbarui` | `DoctorPayableId` → `MedicalServicePayableId`; nilai `PayableType` `DOCTOR` → `MEDICAL_SERVICE` | Belum ada data, jadi bukan rename melainkan definisi awal |
| `FinPayableAdjustment` | `Diperbarui` | Sama seperti di atas | Sama |
| `FinDoctorPayable`, `FinDoctorPayableItem` | **Dibatalkan** | — | Tidak pernah dibuat; dihapus dari rencana migration `AddFinancePayable` |

Rencana migration pada bagian 7 nomor 5 (`AddFinancePayable`) **belum dijalankan**, sehingga
seluruh perubahan di atas masuk ke migration yang sama — bukan menjadi migration tambahan.

## A.8 Invariant yang ditambahkan

| Invariant | Ditegakkan di mana | Bila dilanggar |
|---|---|---|
| `FinPayment.NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount` | Check constraint + service | Transaksi dibatalkan |
| `NetTransferAmount >= 0` | Check constraint | `422` — potongan tidak boleh melebihi jasa yang dibayarkan |
| `FinPaymentDeduction.Amount > 0` | Check constraint | Ditolak database |
| `DeductionType = 'OTHER'` mewajibkan `Reason` terisi | Check constraint | Ditolak database |
| Satu `SourceMedicalServiceFeeId` → paling banyak satu utang jasa | Unique index | `409` |
| Potongan **tidak pernah** mengurangi `OutstandingAmount` utang | Service | Utang lunas sebesar alokasinya (`FIN-DES-028`) |

## A.9 Yang tetap tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| Tabel utang terpisah per jenis tenaga medis | Melahirkan tabel kembar beserta dua jalur pembayaran; diganti kolom `PayeeType` (`FIN-DES-025`) |
| Perhitungan PPh 21 di dalam Finance | Finance hanya **mencatat** nilainya. Cara menghitungnya mengikuti ketentuan pajak, dan bila kelak perlu otomatis, itu keputusan tersendiri |
| Master jenis potongan sebagai tabel | Daftarnya pendek dan jarang berubah; check constraint sudah cukup, mengikuti pola `FIN-DES-004` |
| Pengurangan utang oleh potongan | Akan membuat utang jasa tidak pernah lunas (`FIN-DES-028`) |

---

# AMENDMENT REVISI 3 — Uang Muka, Deposit, dan Selisih Kas

| Field | Nilai |
|---|---|
| Tanggal | 25 September 2026 |
| Masukan | `00-interview-decisions.md` — `FIN-DEC-030`..`044`, seluruhnya `approved` 25 September 2026 |
| Backend SHA | `d6cdfaf9` (branch `Yasmina`) — bergerak dari `09101d05`, impact scan tercatat di `01-existing-capability-map.md` bagian 9.3 |
| Kontrak yang ikut naik | `FIN-INTEGRATION-1.1`, `FIN-STATE-1.1` |
| Status keputusan | `FIN-DES-029`..`036` **`approved`** — Yasmin, 25 September 2026. Approval ini BUKAN otorisasi migration, perubahan source, maupun pengaktifan pengiriman |
| Yang TIDAK berubah | Seluruh `FIN-DES-001`..`028` tetap berlaku apa adanya. Tidak ada keputusan lama yang dicabut |

## B.1 Mengapa amendment ini ada

Dua sebab, dan keduanya berasal dari luar dokumen ini:

1. **Accounting meminta satu perubahan perilaku** (`ACC-DEC-091`): penerimaan sebelum tagihan
   final tidak lagi ditahan, melainkan terbit segera sebagai Uang Muka Pasien. Owner Finance
   menyetujuinya (`FIN-DEC-030`). Ini membatalkan wujud teknis `FIN-DEC-004` yang **sudah
   terkode** — satu-satunya amendment pada blueprint ini yang menyentuh source berjalan.
2. **Gerbang cutover `G6` menuntut kejelasan** deposit pasien, kelebihan bayar, dan selisih kas
   shift kasir. Jawabannya melahirkan tujuh kode kejadian baru (`FIN-DEC-031`, `034`, `035`,
   `040`..`044`).

**Temuan yang paling mengubah rencana, dan arahnya menguntungkan:** ketiga rumpun itu ternyata
**sudah dimiliki Billing sepenuhnya** dan Finance sudah punya akses bacanya. Tidak ada satu pun
tabel baru yang perlu dibuat Finance, dan tidak ada kontrak baru yang perlu diminta dari Billing.
Bukti lengkap di `01-existing-capability-map.md` `FIN-CAP-022`..`025`.

## B.2 Keputusan arsitektur baru

| ID | Keputusan | Dasar | Kenapa begitu |
|---|---|---|---|
| `FIN-DES-029` | Empat jenis fakta baru masuk lewat `FinBillingHandoffIntake` yang **sudah ada**, dengan menambah nilai `HandoffType`: `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW`. **Bukan** tabel intake baru | `FIN-DEC-040`..`044`; `FIN-DES-008` sudah menetapkan tabel itu sebagai "satu pintu masuk seluruh fakta dari Billing" | Tabelnya memang dirancang untuk ini: `HandoffType` sudah menjadi kolom pembeda, dan unique index `(HandoffType, SourceHandoffKey)` sudah menjaga satu fakta hanya diolah satu kali. Membuat tabel intake kedua akan melahirkan dua jalur idempotensi yang bisa saling menyimpang |
| `FIN-DES-030` | `FinAccountingEventOutbox` **tidak bertambah satu kolom pun**. Tujuh kode baru cukup memakai nilai `EventTypeCode` baru, dan idempotensinya memakai unique index yang sudah ada `(SourceModule, SourceTransactionId, EventTypeCode, SourceVersion)` | `FIN-DEC-031`..`044`; konfigurasi `FinAccountingEventOutboxConfiguration` baris 53-55 | `EventTypeCode` sengaja tidak dibatasi check constraint (komentar pada model menyebutnya eksplisit), sehingga kode baru tidak menuntut migration. `SourceTransactionId` diisi kunci fakta Billing, sehingga satu mutasi deposit tidak mungkin melahirkan dua kejadian sejenis |
| `FIN-DES-031` | Batas `Amount > 0` pada `FinanceAccountingOutboxService.ValidateRequest` **dipersempit**: hanya berlaku untuk kejadian bernilai moneter searah. `SELISIH-KAS-SHIFT` boleh **negatif** (kekurangan kas), `SALDO-SUBLEDGER` boleh **nol atau negatif** | `FIN-DEC-034`, `043`, `035`; `integration-contract.md` bagian 5.6 | Diverifikasi langsung: pembatasan itu **hanya ada di kode**, bukan di database — konfigurasi hanya menetapkan `HasPrecision(18,2)` tanpa check constraint nilai. Jadi relaksasinya **nol migration** |
| `FIN-DES-032` | `AccountingOutboxEventRequest` bertambah satu properti opsional `SubledgerBalance` (`AccountingPeriodCode`, `ControlAccountCode`). `BuildPayloadJson` menyertakannya **hanya** bila terisi | `FIN-DEC-035`; `integration-contract.md` bagian 5.2 | Payload tetap dibangun di dalam service, bukan diterima mentah dari pemanggil — penjagaan `FR-FIN-073` (data pasien tidak pernah ikut) tetap terkunci di level tipe. Menambah satu objek bertipe tetap tidak membuka jalan field bebas |
| `FIN-DES-033` | Properti `RequiresFinalization` pada `AccountingOutboxEventRequest` **dicabut**. Penentuan perlakuan pra-finalisasi berpindah ke pemanggil sebagai **pemilihan `EventTypeCode`**, bukan sebagai penahanan status | `FIN-DEC-030` | Satu fakta hanya boleh punya satu representasi. Membiarkan `RequiresFinalization` tetap ada sementara tidak lagi berpengaruh akan menjadi properti yang menipu pembaca kode berikutnya |
| `FIN-DES-034` | Kode pembalikan penerimaan diturunkan dari **`FinReceipt.SourceInvoiceStatus` milik baris penerimaan ASLI** (yang ditunjuk `ReversalOfReceiptId`), bukan dari status tagihan saat pembalikan terjadi, dan bukan pula dengan menambah kolom baru | `FIN-DEC-044` | Kolomnya sudah ada dan sudah menyimpan status historis yang tepat. `FinanceReceiptService` juga sudah membaca baris asli untuk membuat pembalikan, sehingga tidak ada query tambahan sama sekali — **nol migration, nol biaya baca** |
| `FIN-DES-035` | Pemicu sinkronisasi empat fakta baru memakai **anti-join ke `FinBillingHandoffIntake`**, bukan ke kotak keluar, dengan penyempitan waktu (`OccurredAt`/`RecognizedAt`/`ReviewedAt`) | `FIN-DES-008`, `FIN-DES-029` | Tabel intake adalah catatan resmi "fakta ini sudah diolah". Memakai kotak keluar sebagai penanda akan mencampur dua urusan: apa yang sudah **diolah** dan apa yang sudah **dikirim** |
| `FIN-DES-036` | `SELISIH-KAS-SHIFT` memakai `BilCashVarianceReview.Id` sebagai `SourceTransactionId`, dan `AccountingDate` diambil dari **tanggal shift** (`BilCashierShift.OpenedAt`), bukan tanggal pengesahan | `FIN-DEC-043`; `FIN-CAP-024` | `BilCashVarianceReview` adalah baris yang benar-benar berarti "selisih disahkan", lengkap dengan `ReviewerId`, `Reason`, dan `Resolution`. Memakai `BilCashierShift.Id` akan gagal membedakan shift yang selisihnya ditinjau lebih dari sekali |

## B.3 Contoh berangka — tiga kejadian dari satu rawat inap

Pasien rawat inap menitipkan deposit, dipakai sebagian, sisanya dikembalikan.

| Tanggal | Fakta di Billing | Kejadian Finance | Nilai | Akibat di buku besar |
|---|---|---|---|---|
| 5 November | `BilDepositMovement` `TOP_UP` | `PENERIMAAN-UANG-MUKA` | Rp 10.000.000 | Debit Kas, Kredit Uang Muka Pasien |
| 12 November | `BilDepositMovement` `ALLOCATION` | `PEMAKAIAN-UANG-MUKA-DEPOSIT` | Rp 7.500.000 | Debit Uang Muka Pasien, Kredit Piutang — **kas tidak bergerak** |
| 13 November | `BilDepositMovement` `RELEASE` | `PENGEMBALIAN-UANG-MUKA` | Rp 2.500.000 | Debit Uang Muka Pasien, **Kredit Kas** |

Setelah ketiganya, saldo Uang Muka Pasien untuk pasien itu nol: Rp 10.000.000 masuk,
Rp 7.500.000 dipakai, Rp 2.500.000 dikembalikan. **Bila baris kedua dan ketiga memakai satu kode
yang sama** — sebagaimana `FIN-DEC-032` versi sebelum dikoreksi — Accounting tidak punya cara
membedakan mana yang mengurangi piutang dan mana yang mengeluarkan kas, sehingga salah satunya
pasti salah jurnal sementara buku besar tetap seimbang.

## B.4 Contoh berangka — kelebihan bayar

Pasien rawat jalan membayar tunai Rp 500.000 untuk tagihan Rp 450.000.

| Urutan | Fakta | Kejadian Finance | Nilai | Akibat |
|---:|---|---|---|---|
| 1 | Tender berhasil, tagihan sudah `FINAL` | `PENERIMAAN-KASIR` | Rp 500.000 | Debit Kas Rp 500.000 |
| 2 | Alokasi ke tagihan | *(tidak ada kejadian baru)* | Rp 450.000 | — |
| 3 | `BilRefundableCredit` `ALLOCATION_EXCESS` diakui | `PENGAKUAN-KELEBIHAN-BAYAR` | Rp 50.000 | Debit lawan jurnal penerimaan asli, Kredit Uang Muka Pasien — **kas tidak disentuh** |
| 4 | `BilRefundCase` `EXECUTED` | `PENGEMBALIAN-UANG-MUKA` | Rp 50.000 | Debit Uang Muka Pasien, Kredit Kas |

**Kenapa langkah 3 tidak digabung ke langkah 1.** Pada langkah 1 Finance **belum tahu** ada
kelebihan — Billing baru mengakuinya setelah alokasi. Menunggu sampai langkah 4 juga tidak bisa:
bila pengakuan terjadi November dan pengembaliannya Desember, jurnal koreksinya akan menabrak
periode November yang mungkin sudah ditutup Accounting, dan toleransi selisih mereka nol.

## B.5 Status model dan dampak migration

| Model | Status | Yang berubah | Dampak migration |
|---|---|---|---|
| `FinBillingHandoffIntake` | **Diperbarui** | **Tidak ada kolom yang berubah.** Yang berubah hanya himpunan nilai sah `HandoffType`: dari `AR`, `AP`, `COLLECTION`, `ADJUSTMENT` menjadi delapan nilai dengan tambahan `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW` | **Satu migration** — ubah check constraint `CK_FinBillingHandoffIntake_HandoffType` |
| `FinAccountingEventOutbox` | **Sudah ada, tidak disentuh skemanya** | Nol kolom. `EventTypeCode` menerima tujuh nilai baru tanpa check constraint; `Amount` sudah menerima nol/negatif di level database | **Nol migration** |
| `FinReceipt` | **Sudah ada, tidak disentuh** | Nol kolom. `SourceInvoiceStatus` yang sudah ada dipakai untuk memilih kode (`FIN-DES-034`) | **Nol migration** |
| `FinanceAccountingOutboxService` | **Diperbarui** (kode) | `ValidateRequest` dipersempit (`FIN-DES-031`); `AccountingOutboxEventRequest` bertambah `SubledgerBalance` dan kehilangan `RequiresFinalization` (`FIN-DES-032`, `033`); `BuildPayloadJson` menyertakan objek saldo bila ada | — |
| `FinanceReceiptService` | **Diperbarui** (kode) | Pemilihan `EventTypeCode` berdasarkan `SourceInvoiceStatus`; pembalikan memilih kode dari baris asli (`FIN-DES-034`) | — |
| `FinanceBillingIntakeService` | **Diperbarui** (kode) | Empat jalur sinkronisasi baru (`FIN-DES-035`) | — |
| `FinAccountingEventDeliveryStatuses` | **Sudah ada, tidak disentuh** | `HELD_FOR_FINALIZATION` **tetap** sebagai konstanta dan tetap sah pada check constraint, tetapi **tidak lagi dihasilkan kode baru** | **Nol migration** — lihat B.6 |

**Ringkasnya: satu migration untuk seluruh amendment ini.** Sisanya perubahan kode. Ini akibat
langsung dari keputusan memakai tabel dan kolom yang sudah ada (`FIN-DES-029`, `030`, `034`).

## B.6 Rencana migration

| Urutan | Migration | Tanpa downtime | Pengisian data lama | Cara mundur |
|---:|---|:---:|---|---|
| 1 | `AlterFinBillingHandoffIntakeHandoffTypeCheck` — `DROP CONSTRAINT` lalu `ADD CONSTRAINT` dengan delapan nilai | **Ya** | Tidak ada. Baris existing seluruhnya memakai empat nilai lama yang tetap sah | Kembalikan constraint ke empat nilai. **Hanya aman selama belum ada baris memakai nilai baru** — bila sudah ada, baris itu harus dihapus atau dipindah lebih dulu |

**Penanganan baris warisan `HELD_FOR_FINALIZATION`.** Nilai itu **tidak dihapus** dari check
constraint `CK_FinAccountingEventOutbox_DeliveryStatus`, supaya baris yang tersimpan sebelum
`FIN-DEC-030` berlaku tidak menjadi tidak valid. Tetapi baris seperti itu **tidak akan pernah
terkirim**, karena pemicu pelepasannya dicabut bersama `FIN-DES-033`. Karena itu:

1. Sebelum pengiriman diaktifkan (gerbang `G4`), hitung baris berstatus `HELD_FOR_FINALIZATION`.
2. Bila nol — kasus yang paling mungkin, karena worker pengiriman belum pernah hidup dan endpoint
   Accounting belum ada — tidak ada pekerjaan data sama sekali.
3. Bila ada, tiap baris diperiksa: `EventTypeCode`-nya dibetulkan menjadi `PENERIMAAN-UANG-MUKA`
   bila `SourceInvoiceStatus` penerimaannya `OPEN`, lalu statusnya dipindah ke `PENDING`
   (`state-transition-matrix.md` bagian 9, transisi migrasi satu kali).

Langkah 1 adalah **pembacaan database**, dan seperti seluruh dokumen ini ia **tidak** memberi
wewenang menjalankan apa pun terhadap database. Wewenang migration dan eksekusi tetap terpisah.

## B.7 Yang sengaja tidak dibuat pada amendment ini

| Yang ditolak | Alasan |
|---|---|
| Tabel `FinPatientAdvance` / `FinDepositLedger` milik Finance | Deposit pasien dimiliki Billing (`BilDepositAccount`/`BilDepositMovement`, `FIN-CAP-022`). Membuat salinannya di Finance melahirkan dua saldo yang bisa berselisih, dan melanggar aturan bisnis #1 (Finance menyalin, tidak menghitung ulang) |
| Tabel `FinRefundRequest` milik Finance | Kasus pengembalian beserta approval-nya dimiliki Billing (`BilRefundCase`, `FIN-CAP-023`) dan sudah punya siklus `SUBMITTED→APPROVED→EXECUTED` sendiri. Finance hanya menerbitkan kejadian saat `EXECUTED` |
| Tabel intake kedua khusus deposit/refund | Melahirkan dua jalur idempotensi yang bisa saling menyimpang; `FinBillingHandoffIntake` memang dirancang untuk diperluas lewat `HandoffType` (`FIN-DES-029`) |
| Kolom `AccountingEventTypeCode` pada `FinReceipt` | `SourceInvoiceStatus` yang sudah ada cukup untuk menurunkan kodenya (`FIN-DES-034`), sehingga kolom itu hanya akan menduplikasi informasi yang sama dan berisiko menyimpang |
| Check constraint pada `FinAccountingEventOutbox.EventTypeCode` | Katalog kode adalah kesepakatan dua pihak yang masih menunggu ratifikasi Accounting (`FIN-OQ-017`). Mengunci nilainya di database sekarang berarti setiap penyesuaian nama kode menuntut migration |
| Satu kode gabungan untuk pemakaian dan pengembalian uang muka | Lawan jurnalnya berbeda — lihat B.3. Ini keputusan yang **sudah pernah diambil lalu dikoreksi** (`FIN-DEC-032` `superseded`), dan dicatat di sini supaya tidak diusulkan ulang |
| Kejadian untuk `BilRefundCase` bersumber `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` | Lawan jurnalnya belum digali (`FIN-OQ-018`); di luar permintaan gerbang `G6` |


# AMENDMENT REVISI 4 — Purchasing/AP, AR Invoice Agregat, dan Potongan AR

## C.1 Mengapa amendment ini ada

Amendment ini menutup tiga rumpun kapabilitas baru (`FIN-SC-008`, `FIN-SC-009`, `FIN-SC-010`)
yang ditemukan lewat audit `Keuangan.md` — dokumen evidence hasil analisis video sistem rujukan
eksternal, bukan sumber otoritatif, dipakai murni sebagai peta area yang perlu diperiksa.
Ketiga rumpun disetujui lewat `FIN-DEC-045` s.d. `FIN-DEC-055` (`00-interview-decisions.md`),
seluruhnya `approved` 25 September 2026, dan ditelusuri lewat `/trace-existing-capabilities`
menghasilkan `FIN-CAP-026` s.d. `FIN-CAP-036` (`01-existing-capability-map.md` bagian 12).

Berbeda dari REVISI 2 (Payable) dan REVISI 3 (Accounting Integration/Collection) yang masing-
masing memperluas rumpun yang **sudah ada**, amendment ini yang pertama kali membangun rumpun
yang **sama sekali belum ada satu barisnya pun** di backend (Purchasing/AP) — dikonfirmasi nol
lewat pencarian `rg` menyeluruh (`FIN-CAP-028`). Ini juga pertama kalinya Finance Management
menjadi **pemilik proses hulu** (procurement), bukan sekadar konsumen hilir dari modul lain —
konsekuensi eksplisit dari `FIN-DEC-045` yang sudah dicatat sebagai penyimpangan pola oleh owner
sendiri.

Tiga rumpun ini dirancang dalam satu amendment karena diputuskan dalam satu sesi keputusan yang
sama dan saling beririsan: rumpun Purchasing/AP memperbarui `FinSupplierPayable` (sisi Payable
yang sudah ada), rumpun AR Invoice Agregat membangun lapisan baru di atas `FinReceivable` (sisi
Receivable yang sudah ada), dan rumpun Potongan AR memperbarui invariant `FinReceivable` yang
sama. Merancangnya terpisah berisiko melahirkan keputusan yang saling bertentangan pada entity
yang sama.

## C.2 Keputusan arsitektur baru

### Tabel kepemilikan data

| Kelompok data | Modul pemilik | Dipakai amendment ini | Dibuat ulang? |
|---|---|---|---|
| Data supplier (kode, NPWP, bank, TOP, lead time, PPN, diskon, limit kredit) | Finance Management (`MstSupplier`, `FIN-CAP-027`) | Ya — direferensikan seluruh entity Purchasing/AP baru | Tidak — `MstSupplier` sudah lengkap, tidak ada kolom baru |
| Utang ke supplier (ledger) | Finance Management (`FinSupplierPayable`) | Ya — jadi target penciptaan dari `FinPurchasingInvoice` | Tidak — diperluas (`Diperbarui`), bukan diganti |
| Piutang per invoice pasien/penjamin | Finance Management (`FinReceivable`, `FIN-CAP-031`) | Ya — jadi unit dasar yang dikelompokkan `FinReceivableInvoiceBatch`, dan target pengurangan `FinReceiptDeduction` | Tidak — dipakai apa adanya, nol kolom baru |
| Dokumen tagihan per invoice untuk penjamin perusahaan | Billing (`BillingCompanyGuarantorInvoiceDocumentService`, `FIN-CAP-030`) | Ya — dirujuk sebagai rincian baris dalam `FinReceivableInvoiceBatch` | Tidak — dipakai lewat pemanggilan service yang sudah ada (`Reuse with adapter`), bukan disalin |
| Potongan sisi Payable (pola struktur) | Finance Management (`FinPaymentDeduction`, `FIN-CAP-033`) | Ya — jadi pola struktural untuk `FinReceiptDeduction` (arah aliran dibalik, bukan tabel yang sama) | Tidak — entity baru terpisah, `FinPaymentDeduction` tidak disentuh |
| Ambang dan jenjang approval nominal (pola) | Finance Management (`FinancePaymentService.ResolveApprovalTier`, `FIN-CAP-035`) | Ya — logikanya diekstrak jadi helper bersama, dipakai ulang oleh PO dan Purchasing Invoice | Tidak — diekstrak (`Diperbarui`), logika nominal (Rp 50.000.000) tidak berubah |
| Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur | **Baru — Finance Management** | — | Ya — dikonfirmasi nol existing (`FIN-CAP-028`, `FIN-CAP-029`) |
| AR Invoice Agregat (batch tagihan gabungan) | **Baru — Finance Management** | — | Ya — dikonfirmasi nol existing (`FIN-CAP-032`) |
| Potongan sisi penerimaan piutang (PPh 23, biaya admin bank) | **Baru — Finance Management** | — | Ya — dikonfirmasi nol existing (`FIN-CAP-034`) |
| Kejadian akuntansi PPN Masukan Pembelian | Accounting (kode `PPN-MASUKAN-PEMBELIAN`, diusulkan `evidence/06`) | Ya — dipicu dari `FinPurchasingInvoice` disetujui | Tidak — Finance hanya mengirim envelope `ACC-XMOD-0.3` yang sudah ada, tidak membuat tabel Accounting |

### FIN-DES-037 — Rumpun Purchasing/AP: entity inti

Tujuh entity baru dibangun mengikuti alur `PO → Tanda Terima Barang → Tukar Faktur →
Purchasing Invoice`, sebagaimana ditegaskan `FIN-DEC-051`:

- `FinPurchaseOrder` + `FinPurchaseOrderItem` — permintaan pembelian ke supplier, menunggu
  approval bila di atas ambang (`FIN-DES-039`).
- `FinGoodsReceipt` + `FinGoodsReceiptItem` — pencatatan barang fisik diterima terhadap satu PO.
  **DEV_DISCRETION**: dirancang selalu terhadap satu `PurchaseOrderId` (tidak nullable) — jalur
  "tanpa PO" yang disebut `Keuangan.md` hanya berlaku pada Tukar Faktur/Purchasing Invoice
  (`FIN-DEC-051` eksplisit menyebut Tukar Faktur boleh tanpa PO), bukan pada penerimaan barang,
  karena menerima barang tanpa order yang mendahuluinya bukan pola yang diminta owner. Keputusan
  ini didelegasikan ke pelaksana desain karena tidak ada keputusan eksplisit soal GR tanpa PO
  pada decision log manapun; bila keliru, dapat diamandemen terpisah tanpa mengubah entity lain.
- `FinInvoiceExchange` ("Tukar Faktur") — checkpoint serah terima dokumen dari supplier,
  mencatat tanggal terima dan estimasi jatuh tempo dari TOP supplier (`MstSupplier.PaymentTermDays`,
  sudah ada — `FIN-CAP-027`). `PurchaseOrderId` dan `GoodsReceiptId` nullable sesuai `FIN-DEC-051`.
- `FinPurchasingInvoice` + `FinPurchasingInvoiceItem` — nilai final pembelian (PPN, diskon,
  potongan, DP/termin). Tepat satu `FinInvoiceExchange` menghasilkan tepat satu
  `FinPurchasingInvoice` (`FIN-DEC-051`, unique constraint pada `InvoiceExchangeId`).

### FIN-DES-038 — Retur Pembelian dan Deposit Retur

`FinSupplierReturn` + `FinSupplierReturnItem` mencatat pengembalian barang ke supplier.
`FinSupplierReturnDeposit` adalah entity kredit terpisah yang dapat dipakai lintas invoice/
pembelian berikutnya (`FIN-DEC-047`), secara struktural meniru pola `BilRefundableCredit`
(Billing) namun **bukan tabel yang sama** — arah aliran uang berlawanan (Billing: kredit ke
pasien; Finance: kredit dari supplier ke RS). `FinSupplierReturnDepositUsage` mencatat setiap
pemakaian deposit terhadap `FinPurchasingInvoice`/`FinSupplierPayable` tertentu, menjaga
`FinSupplierReturnDeposit.AvailableAmount` tetap konsisten (pola yang sama dengan bagaimana
`FinPaymentDeduction` diaudit — satu baris pemakaian, bukan pengurangan langsung tanpa jejak).

### FIN-DES-039 — Approval dua jenjang untuk PO dan Purchasing Invoice

`FIN-DEC-050` meminta jenjang approval bertingkat berdasarkan nominal sejak awal desain;
`FIN-DEC-052` menutup ambangnya di **Rp 50.000.000**, angka yang persis sama dengan placeholder
yang sudah ditemukan di `FinancePaymentService.ResolveApprovalTier` (`FIN-CAP-035`).

Alih-alih menulis ulang logika ambang tiga kali (Payment, PO, Purchasing Invoice), logika
`ResolveApprovalTier(decimal nominal)` dan `ApprovalTiers.Tier1`/`Tier2` **diekstrak** dari
`FinancePaymentService` menjadi helper bersama `FinanceApprovalTierResolver` (baru, di folder
`Payable/Services` karena dipakai lintas Payable — lihat C.7). `FinancePaymentService` diperbarui
untuk memanggil helper ini alih-alih menyimpan salinan logikanya sendiri, supaya tidak ada tiga
salinan ambang yang bisa menyimpang.

> **KOREKSI 25 September 2026 (ditemukan saat `/plan-module-delivery` membaca source).** Versi
> awal paragraf ini menyatakan "logika nominal tidak berubah, murni ekstraksi". **Itu keliru
> tepat pada satu nilai.** Kode yang berjalan (`FinancePaymentService.cs` baris 654-658) memakai
> `<= 50_000_000m → TIER_1`, sedangkan `FIN-DEC-052` yang sudah `approved` menetapkan
> `< Rp 50.000.000` → Supervisor Finance dan **`>= Rp 50.000.000` → Manajer Finance**.
>
> | Nominal | Kode hari ini | `FIN-DEC-052` |
> |---|---|---|
> | Rp 49.999.999 | `TIER_1` | `TIER_1` |
> | **Rp 50.000.000 tepat** | **`TIER_1`** | **`TIER_2`** |
> | Rp 50.000.001 | `TIER_2` | `TIER_2` |
>
> Resolver bersama **MUST** mengikuti `FIN-DEC-052` (`>= 50.000.000 → TIER_2`). Akibatnya ekstraksi
> ini **mengubah perilaku pembayaran yang sudah berjalan** untuk satu nilai batas itu:
> pembayaran tepat Rp 50.000.000 yang hari ini cukup disetujui Supervisor akan menuntut Manajer.
> Pembayaran yang sudah tersimpan tidak dihitung ulang — `ApprovalTier` dibekukan saat diajukan
> (`FIN-DEC-052`: jejak audit mencatat jenjang yang berlaku saat approval).

Komentar kode yang menyebut ambang ini "provisional"/"belum diratifikasi" (`FinPayment.cs` baris
18, `FinancePaymentsController.cs` baris 24-28, `FinancePaymentService.cs` baris 646-651)
diperbarui karena `FIN-DEC-052` sudah meratifikasi nilainya secara eksplisit.

`FinPurchaseOrder` dan `FinPurchasingInvoice` masing-masing mendapat kolom `ApprovalTier`,
`RequestedByUserId`, `RequestedAt`, `ApprovedByUserId`, `ApprovedAt`, mengikuti pola persis yang
sudah ada pada `FinPayment`. Checkpoint ini **berbeda** dari checkpoint approval pembayaran
(`FIN-DEC-022`) — PO/Purchasing Invoice disetujui sebelum utang tercatat; pembayaran disetujui
sebelum kas keluar. Keduanya kini memakai ambang yang sama (`FIN-DEC-052`) tapi tetap dua
checkpoint independen dengan actor dan record persetujuan masing-masing.

### FIN-DES-040 — Perluasan FinSupplierPayable

`FinSupplierPayable` (`Diperbarui`) mendapat kolom `SourcePurchasingInvoiceId` (nullable, FK ke
`FinPurchasingInvoice`). **Jalur input manual yang sudah ada tetap dipertahankan** — amendment
ini tidak menghapus kapasitas mencatat utang supplier secara manual, karena beberapa supplier
lama mungkin belum onboarding ke alur PO penuh dan tidak ada keputusan eksplisit untuk
mendeprekasinya. `SourcePurchasingInvoiceId` bernilai `NULL` untuk baris yang dibuat manual
(perilaku lama, tidak berubah) dan terisi otomatis untuk baris yang dibuat sistem saat
`FinPurchasingInvoice` berstatus `Approved` (`FIN-DEC-045`).

### FIN-DES-041 — AR Invoice Agregat

`FinReceivableInvoiceBatch` (baru) adalah aggregate root yang mengelompokkan baris `FinReceivable`
yang berbagi `DebtorType="PAYER"` dan `DebtorReferenceId` yang sama (kunci pengelompokan yang
sudah tersedia di `FinReceivable`, nol perubahan skema dibutuhkan — `FIN-CAP-031`) dalam satu
periode penagihan. Batch ini menjadi **dokumen resmi yang dikirim ke penjamin** (`FIN-DEC-048`),
menggantikan `BilInvoice` individual sebagai dokumen yang dikirim — namun `BilInvoice` dan
`FinReceivable` **tetap ada dan tidak diubah perannya** sebagai satuan internal; batch adalah
lapisan baru di atasnya, bukan pengganti. `FinReceivableInvoiceBatchItem` (baru) adalah tabel
penghubung `BatchId` ↔ `ReceivableId`; satu `FinReceivable` hanya boleh tergabung dalam satu
batch aktif pada satu waktu (unique constraint pada `ReceivableId` untuk baris yang batch-nya
belum `Cancelled`).

Saat batch diterbitkan (`Issued`), setiap baris `FinReceivable` anggotanya dapat merujuk balik
ke dokumen per-invoice yang sudah ada via `BillingCompanyGuarantorInvoiceDocumentService`
(`FIN-CAP-030`, `Reuse with adapter`) sebagai rincian baris pada dokumen batch gabungan —
service tersebut **dipanggil**, bukan disalin ulang logikanya.

### FIN-DES-042 — Potongan sisi penerimaan piutang

`FinReceiptDeduction` (baru) meniru struktur `FinPaymentDeduction` (`DeductionType`, `Amount`,
`Reason`, `ReferenceNumber`) namun melekat pada `FinReceipt`/alokasinya, bukan `FinPayment`, dan
`DeductionType` memakai daftar nilai berbeda: `PPH23`, `BANK_ADMIN_FEE`, `OTHER` (bukan daftar
`FinPaymentDeduction` yang memuat `PPH21`/`KASBON`/dst — konteksnya berbeda, sisi penerimaan
bukan sisi pembayaran ke pihak ketiga).

Per `FIN-DEC-055`, potongan ini **mengurangi `FinReceivable.OutstandingAmount`** — berfungsi
sebagai "pembayaran non-tunai", kebalikan tepat dari `FIN-DES-028` (potongan sisi Payable TIDAK
mengurangi `FinSupplierPayable` yang dibayarkan, karena di sana potongan adalah pengurang nilai
yang dibayar tunai, bukan pelunasan utang). Baris `FinReceiptDeduction` diperlakukan sama seperti
baris alokasi pembayaran biasa untuk keperluan invariant `FinReceivable`
(`OriginalAmount = OutstandingAmount + AllocatedAmount + AdjustedAmount + WrittenOffAmount`,
`FIN-CAP-031`) — nilainya masuk ke `AllocatedAmount`, bukan kategori baru, supaya invariant yang
sudah ada tidak perlu diubah bentuknya.

### FIN-DES-043 — PPN Masukan Pembelian

`FinPurchasingInvoice` menyimpan `PPNAmount` (nilai Pajak Masukan). Saat status berubah menjadi
`Approved`, sistem **menyiapkan** kejadian akuntansi kode `PPN-MASUKAN-PEMBELIAN` (diusulkan
lewat `evidence/06`, mengikuti kontrak amplop 12 field `ACC-XMOD-0.3` yang sudah diratifikasi
Accounting) ke `FinAccountingEventOutbox` yang sudah ada — **bukan mekanisme baru**, memakai
pola outbox transaksional yang sama dengan seluruh kejadian Finance lain.

**Gerbang keras**: sesuai `FIN-DEC-046`, worker pengiriman kejadian ini **tidak boleh diaktifkan**
sampai Accounting meratifikasi kode `PPN-MASUKAN-PEMBELIAN` (`FIN-OQ-020`, sisi Finance sudah
ditutup lewat `evidence/06`, ratifikasi masih ditunggu). Baris outbox tetap dibuat dan disimpan
berstatus `PENDING` sejak awal (tidak diblokir di level penulisan data), tapi worker pengiriman
untuk `EventTypeCode = PPN-MASUKAN-PEMBELIAN` secara eksplisit di-*feature-gate* menunggu
ratifikasi — pola yang sama persis dengan bagaimana kode lain di blueprint ini pernah menunggu
ratifikasi sebelum dikirim (`FIN-DES-029` dst).

Nilai pokok barang/jasa (bukan PPN-nya) dicatat lewat kejadian pengakuan utang supplier — kode
kejadian ini **belum diusulkan** pada amendment ini; menyusul sebagai keputusan operasional
terpisah sebelum `/plan-module-delivery` mengunci Purchasing/AP untuk implementasi
(dicatat sebagai open item baru di bagian C.11).

### FIN-DES-044 — Endpoint laporan (tanpa tabel baru)

Delapan kapabilitas pelaporan dari `Keuangan.md` (Aging AP, Rekap Purchasing AP, Laporan Tukar
Faktur, Laporan Jatuh Tempo, Rekonsiliasi Tagihan) dirancang sebagai **query service read-only**
di atas entity `FIN-DES-037`..`042` — tidak melahirkan tabel baru. Aging AP dan Laporan Jatuh
Tempo dihitung dari `FinSupplierPayable`/`FinPurchasingInvoice` beserta `EstimatedDueDate` pada
`FinInvoiceExchange`; Rekap Purchasing AP adalah agregasi periodik; Rekonsiliasi Tagihan
membandingkan `FinInvoiceExchange` terhadap `FinPurchasingInvoice` untuk menemukan Tukar Faktur
yang belum menghasilkan invoice. Ini konsisten dengan pola laporan Finance yang sudah ada
(tidak ada laporan Finance manapun yang punya tabel persisten tersendiri).

## C.3 Class diagram — rumpun Purchasing/AP

```mermaid
classDiagram
    class MstSupplier {
        +Guid Id
        +string Code
        +string Name
        +int PaymentTermDays
        +int LeadTimeDays
        +decimal TaxPercent
        +bool IsTaxable
        +decimal CreditLimitAmount
    }
    class FinPurchaseOrder {
        +Guid Id
        +string PONumber
        +Guid SupplierId
        +string Status
        +decimal TotalAmount
        +string ApprovalTier
        +Guid RequestedByUserId
        +DateTime RequestedAt
        +Guid? ApprovedByUserId
        +DateTime? ApprovedAt
        +Guid RowVersion
    }
    class FinPurchaseOrderItem {
        +Guid Id
        +Guid PurchaseOrderId
        +string ProductCategory
        +string ProductName
        +string Unit
        +decimal Quantity
        +decimal UnitPrice
        +decimal LineTotal
    }
    class FinGoodsReceipt {
        +Guid Id
        +string GRNumber
        +Guid PurchaseOrderId
        +DateTime ReceivedDate
        +string Status
        +Guid RowVersion
    }
    class FinGoodsReceiptItem {
        +Guid Id
        +Guid GoodsReceiptId
        +Guid PurchaseOrderItemId
        +decimal ReceivedQuantity
        +string Notes
    }
    class FinInvoiceExchange {
        +Guid Id
        +string ExchangeNumber
        +Guid SupplierId
        +Guid? PurchaseOrderId
        +Guid? GoodsReceiptId
        +string SupplierInvoiceNumber
        +DateTime SupplierInvoiceDate
        +DateTime ReceivedDate
        +DateTime EstimatedDueDate
        +string Status
        +Guid RowVersion
    }
    class FinPurchasingInvoice {
        +Guid Id
        +string InvoiceNumber
        +Guid InvoiceExchangeId
        +Guid SupplierId
        +decimal SubtotalAmount
        +decimal DiscountAmount
        +decimal PPNAmount
        +decimal DownPaymentAmount
        +decimal OtherDeductionAmount
        +decimal TotalAmount
        +string Status
        +string ApprovalTier
        +Guid RequestedByUserId
        +DateTime RequestedAt
        +Guid? ApprovedByUserId
        +DateTime? ApprovedAt
        +Guid RowVersion
    }
    class FinPurchasingInvoiceItem {
        +Guid Id
        +Guid PurchasingInvoiceId
        +string ProductName
        +decimal Quantity
        +decimal UnitPrice
        +decimal LineTotal
    }
    class FinSupplierPayable {
        +Guid Id
        +Guid SupplierId
        +Guid? SourcePurchasingInvoiceId
        +decimal OutstandingAmount
    }
    class FinSupplierReturn {
        +Guid Id
        +string ReturnNumber
        +Guid PurchasingInvoiceId
        +string Reason
        +decimal TotalAmount
        +string Status
    }
    class FinSupplierReturnItem {
        +Guid Id
        +Guid SupplierReturnId
        +decimal Quantity
        +decimal LineTotal
    }
    class FinSupplierReturnDeposit {
        +Guid Id
        +Guid SupplierId
        +Guid SourceReturnId
        +decimal OriginalAmount
        +decimal AvailableAmount
        +string Status
    }
    class FinSupplierReturnDepositUsage {
        +Guid Id
        +Guid SupplierReturnDepositId
        +Guid PurchasingInvoiceId
        +decimal UsedAmount
    }
    class FinanceApprovalTierResolver {
        +ResolveApprovalTier(decimal nominal) string
    }

    FinPurchaseOrder "1" --> "*" FinPurchaseOrderItem
    FinPurchaseOrder "1" --> "0..*" FinGoodsReceipt
    FinGoodsReceipt "1" --> "*" FinGoodsReceiptItem
    FinGoodsReceiptItem --> FinPurchaseOrderItem
    FinPurchaseOrder "0..1" --> "0..*" FinInvoiceExchange
    FinGoodsReceipt "0..1" --> "0..*" FinInvoiceExchange
    FinInvoiceExchange "1" --> "1" FinPurchasingInvoice
    FinPurchasingInvoice "1" --> "*" FinPurchasingInvoiceItem
    FinPurchasingInvoice "1" --> "0..1" FinSupplierPayable
    FinPurchasingInvoice "1" --> "0..*" FinSupplierReturn
    FinSupplierReturn "1" --> "*" FinSupplierReturnItem
    FinSupplierReturn "1" --> "0..1" FinSupplierReturnDeposit
    FinSupplierReturnDeposit "1" --> "*" FinSupplierReturnDepositUsage
    FinSupplierReturnDepositUsage --> FinPurchasingInvoice
    FinPurchaseOrder --> MstSupplier
    FinInvoiceExchange --> MstSupplier
    FinPurchasingInvoice --> MstSupplier
    FinPurchaseOrder ..> FinanceApprovalTierResolver
    FinPurchasingInvoice ..> FinanceApprovalTierResolver
```

## C.4 Class diagram — rumpun AR Invoice Agregat

```mermaid
classDiagram
    class FinReceivable {
        +Guid Id
        +string DebtorType
        +Guid DebtorReferenceId
        +decimal OriginalAmount
        +decimal OutstandingAmount
        +decimal AllocatedAmount
    }
    class FinReceivableInvoiceBatch {
        +Guid Id
        +string BatchNumber
        +string DebtorType
        +Guid DebtorReferenceId
        +DateTime PeriodStart
        +DateTime PeriodEnd
        +decimal TotalAmount
        +string Status
        +DateTime? IssuedAt
        +Guid RowVersion
    }
    class FinReceivableInvoiceBatchItem {
        +Guid Id
        +Guid BatchId
        +Guid ReceivableId
    }
    class BillingCompanyGuarantorInvoiceDocumentService {
        +GetDocument(Guid invoiceId) CompanyGuarantorInvoiceDocumentResponse
    }

    FinReceivableInvoiceBatch "1" --> "*" FinReceivableInvoiceBatchItem
    FinReceivableInvoiceBatchItem "1" --> "1" FinReceivable
    FinReceivableInvoiceBatch ..> BillingCompanyGuarantorInvoiceDocumentService : rincian per baris
```

## C.5 Class diagram — rumpun Potongan AR

```mermaid
classDiagram
    class FinReceivable {
        +Guid Id
        +decimal OutstandingAmount
        +decimal AllocatedAmount
    }
    class FinReceipt {
        +Guid Id
        +decimal Amount
    }
    class FinReceiptDeduction {
        +Guid Id
        +Guid ReceiptId
        +string DeductionType
        +decimal Amount
        +string Reason
        +string ReferenceNumber
    }

    FinReceipt "1" --> "*" FinReceiptDeduction
    FinReceiptDeduction ..> FinReceivable : mengurangi OutstandingAmount
```

## C.6 Penjelasan class

> **KOREKSI 25 September 2026 (ditemukan saat `/plan-module-delivery` membaca source).** Versi
> awal tabel ini dan pohon C.7 memuat tiga kekeliruan terhadap pola yang **sudah berjalan** di
> repository: (1) service dan controller memakai prefix `Fin…` padahal seluruh service/controller
> Finance memakai `Finance…` (`FinancePaymentService`, `FinanceReceiptsController`); (2) EF
> configuration ditaruh di `Areas/…/Configurations/` padahal seluruhnya ada di
> `Repositories/Configurations/Corporate/FinanceManagement/<Submodul>/` (DoD backend #10);
> (3) `FinReceipt`, `FinanceReceiptService`, dan `FinanceReceiptsController` berada di submodul
> **`Collection`**, bukan `Receivable` — sehingga `FinReceiptDeduction` ikut ke `Collection`.
> Nama **entity** (`Fin…`) dan nama tabel **tidak berubah**; yang dikoreksi hanya nama
> service/controller dan lokasi berkas. Tabel di bawah adalah versi yang berlaku.

| Class | Status | Lokasi file |
|---|---|---|
| `FinPurchaseOrder`, `FinPurchaseOrderItem` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| `FinGoodsReceipt`, `FinGoodsReceiptItem` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| `FinInvoiceExchange` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| `FinPurchasingInvoice`, `FinPurchasingInvoiceItem` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| `FinSupplierReturn`, `FinSupplierReturnItem` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| `FinSupplierReturnDeposit`, `FinSupplierReturnDepositUsage` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Models/` |
| Sebelas configuration entity Purchasing | Baru | `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/Fin…Configuration.cs` |
| `FinSupplierPayable` | Diperbarui — kolom `SourcePurchasingInvoiceId` (nullable, FK `SetNull`) | `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs`; configuration `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` |
| `FinanceApprovalTierResolver` (+ `ApprovalTiers` dipindah ke sini) | Baru — ekstraksi `FinancePaymentService.ResolveApprovalTier`, batas `>= 50.000.000 → TIER_2` (lihat koreksi `FIN-DES-039`) | `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalTierResolver.cs` |
| `FinancePaymentService` | Diperbarui — memanggil resolver; komentar "provisional" dicabut | `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` |
| `FinancePurchaseOrderService` | Baru — buat/ubah/ajukan/setujui/tolak/batal PO | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinanceGoodsReceiptService` | Baru — catat/batalkan penerimaan barang, perbarui status PO | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinanceInvoiceExchangeService` | Baru — catat/batalkan Tukar Faktur, hitung `EstimatedDueDate` | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinancePurchasingInvoiceService` | Baru — siklus Purchasing Invoice; saat `APPROVED` membuat `FinSupplierPayable` dan menulis outbox `PPN-MASUKAN-PEMBELIAN` dalam satu transaksi | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinanceSupplierReturnService` | Baru — retur, terbitkan dan pakai Deposit Retur | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinancePurchasingReportService` | Baru — lima laporan read-only | `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| `FinancePurchaseOrdersController`, `FinanceGoodsReceiptsController`, `FinanceInvoiceExchangesController`, `FinancePurchasingInvoicesController`, `FinanceSupplierReturnsController`, `FinancePurchasingReportsController` | Baru | `Areas/Corporate/FinanceManagement/Purchasing/Controllers/` |
| `FinReceivableInvoiceBatch`, `FinReceivableInvoiceBatchItem` | Baru | `Areas/Corporate/FinanceManagement/Receivable/Models/`; configuration di `Repositories/Configurations/Corporate/FinanceManagement/Receivable/` |
| `FinanceReceivableInvoiceBatchService` | Baru — kelompokkan `FinReceivable`, terbitkan batch, rujuk dokumen per-invoice Billing. **MUST NOT** menulis `FinReceivable.OutstandingAmount` (`FinanceReceivableService` tetap satu-satunya penulis, DoD backend #13) | `Areas/Corporate/FinanceManagement/Receivable/Services/` |
| `FinanceReceivableInvoiceBatchesController` | Baru | `Areas/Corporate/FinanceManagement/Receivable/Controllers/` |
| `FinReceiptDeduction` | Baru | `Areas/Corporate/FinanceManagement/Collection/Models/`; configuration di `Repositories/Configurations/Corporate/FinanceManagement/Collection/` |
| `FinanceReceiptService` | Diperbarui — catat potongan; efeknya ke piutang **disalurkan lewat** `FinanceReceivableService.ApplyAllocationAsync` yang sudah ada, bukan ditulis langsung | `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` |
| `FinanceReceiptsController` | Diperbarui — `GET`/`POST /receipts/{id}/deductions` | `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` |
| `BillingManagementServiceCollectionExtensions` | Diperbarui — registrasi DI tujuh service baru + resolver | `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` |
| `ApplicationDbContext` | Diperbarui — empat belas `DbSet` baru | `Repositories/ApplicationDbContext.cs` |
| `BillingCompanyGuarantorInvoiceDocumentService` | Sudah ada — dipanggil, tidak diubah | `Areas/HealthServices/BillingManagement/Billing/Services/BillingCompanyGuarantorInvoiceDocumentService.cs` |
| `MstSupplier` | Sudah ada — tidak diubah | `Areas/Administrator/MasterData/Models/MstSupplier.cs` |

**Utang teknis yang diikuti, bukan ditiru diam-diam.** Seluruh service Finance hari ini
didaftarkan DI di `BillingManagementServiceCollectionExtensions.cs` (milik Billing) — preseden
`BE-FIN-017`/`019`/`020`. Revisi ini **mengikuti** preseden itu agar tidak membuat dua titik
registrasi, dan **mencatatnya** sebagai utang teknis. Memindahkannya ke berkas registrasi milik
Finance adalah pekerjaan terpisah yang tidak dicakup revisi ini.

## C.7 Arsitektur folder

```text
Areas/Corporate/FinanceManagement/
├── Purchasing/                                   (Baru — submodul ketujuh, WAJIB didaftarkan
│   │                                              di registry sebelum model pertama)
│   ├── Models/          11 entity                 (Baru)
│   ├── Dtos/            per entity: Create/Update/Response/PagedQuery + Approve/Reject/Cancel
│   ├── Services/        FinancePurchaseOrderService, FinanceGoodsReceiptService,
│   │                    FinanceInvoiceExchangeService, FinancePurchasingInvoiceService,
│   │                    FinanceSupplierReturnService, FinancePurchasingReportService   (Baru)
│   └── Controllers/     enam Finance…Controller   (Baru)
├── Payable/
│   ├── Models/FinSupplierPayable.cs                    (Diperbarui)
│   └── Services/FinanceApprovalTierResolver.cs         (Baru)
│              FinancePaymentService.cs                 (Diperbarui)
├── Receivable/
│   ├── Models/FinReceivableInvoiceBatch.cs, …Item.cs   (Baru)
│   ├── Dtos/                                           (Baru)
│   ├── Services/FinanceReceivableInvoiceBatchService.cs        (Baru)
│   └── Controllers/FinanceReceivableInvoiceBatchesController.cs (Baru)
└── Collection/
    ├── Models/FinReceiptDeduction.cs                   (Baru)
    ├── Services/FinanceReceiptService.cs               (Diperbarui)
    └── Controllers/FinanceReceiptsController.cs        (Diperbarui)

Repositories/Configurations/Corporate/FinanceManagement/
├── Purchasing/       11 configuration              (Baru)
├── Payable/FinSupplierPayableConfiguration.cs      (Diperbarui)
├── Receivable/       2 configuration               (Baru)
└── Collection/FinReceiptDeductionConfiguration.cs  (Baru)
```

**Satu prasyarat struktur baru:** folder `Purchasing/` belum terdaftar di
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Enam submodul yang ada didaftarkan
eksplisit lewat `BE-FIN-001` (`FIN-DES-002`); `Purchasing` MUST mengikuti prosedur yang sama —
baris `Corporate / Finance | FinanceManagement / Purchasing / Pembelian | … | Fin | ACTIVE` —
sebelum file model pertama ditulis (`QBE-MOD-003`). Prefix tetap `Fin`, tidak ada prefix baru.

## C.8 Status model dan dampak migration

| Entity | Status | Kolom yang berubah |
|---|---|---|
| `FinPurchaseOrder` | Baru | Seluruh kolom baru (lihat C.3) |
| `FinPurchaseOrderItem` | Baru | Seluruh kolom baru |
| `FinGoodsReceipt` | Baru | Seluruh kolom baru |
| `FinGoodsReceiptItem` | Baru | Seluruh kolom baru |
| `FinInvoiceExchange` | Baru | Seluruh kolom baru |
| `FinPurchasingInvoice` | Baru | Seluruh kolom baru |
| `FinPurchasingInvoiceItem` | Baru | Seluruh kolom baru |
| `FinSupplierReturn` | Baru | Seluruh kolom baru |
| `FinSupplierReturnItem` | Baru | Seluruh kolom baru |
| `FinSupplierReturnDeposit` | Baru | Seluruh kolom baru |
| `FinSupplierReturnDepositUsage` | Baru | Seluruh kolom baru |
| `FinReceivableInvoiceBatch` | Baru | Seluruh kolom baru |
| `FinReceivableInvoiceBatchItem` | Baru | Seluruh kolom baru |
| `FinReceiptDeduction` | Baru | Seluruh kolom baru |
| `FinSupplierPayable` | Diperbarui | Tambah `SourcePurchasingInvoiceId Guid?` (FK ke `FinPurchasingInvoice`, `ON DELETE SET NULL`, nullable — baris lama tetap `NULL`) |
| `FinPayment` | Diperbarui (non-skema) | Tidak ada kolom baru — hanya komentar kode yang diperbarui (C.2, `FIN-DES-039`) |

Status model tiap entity transaksional baru mengikuti pola `Draft/PendingApproval/Approved/
Rejected/Cancelled` yang sudah dipakai `FinPayment` (kecuali `FinGoodsReceipt`/`FinInvoiceExchange`
yang lebih sederhana: `Received/Cancelled` dan `Received/LinkedToInvoice/Cancelled`), dijabarkan
penuh di `contracts/state-transition-matrix.md`.

## C.9 Rencana migration

1. Migration tunggal `AddPurchasingApRumpun` mencakup 11 tabel baru rumpun Purchasing/AP —
   seluruhnya `CREATE TABLE`, tidak mengganggu tabel yang sudah ada, dapat dijalankan tanpa
   mematikan layanan.
2. Migration kedua `AddArInvoiceBatchAndReceiptDeduction` mencakup `FinReceivableInvoiceBatch`,
   `FinReceivableInvoiceBatchItem`, `FinReceiptDeduction` — juga murni `CREATE TABLE`.
3. Migration ketiga `AddSourcePurchasingInvoiceIdToSupplierPayable` menambah satu kolom nullable
   ke `FinSupplierPayable` — aman dijalankan tanpa downtime, tidak butuh pengisian data lama
   (baris lama valid dengan `NULL`).
4. Ketiganya dipisah (bukan satu migration raksasa) supaya urutan dependency FK jelas dan mudah
   di-rollback satu-satu bila salah satu gagal — migration 2 dan 3 tidak bergantung pada isi data
   migration 1, hanya pada skema-nya.
5. Tidak ada langkah pengisian data lama (backfill) yang dibutuhkan — seluruh tabel baru kosong
   di awal, dan kolom baru pada `FinSupplierPayable` valid `NULL` untuk baris lama.
6. Langkah mundur bila gagal: `DROP TABLE` sesuai urutan FK terbalik untuk migration 1 dan 2;
   `DROP COLUMN SourcePurchasingInvoiceId` untuk migration 3. Seperti seluruh dokumen ini, bagian
   ini **hanya rencana** — wewenang membuat dan menjalankan migration tetap terpisah dan
   membutuhkan otorisasi eksplisit sendiri.

## C.10 Rencana data master awal

`MstSupplier` sudah punya data (tidak kosong) — tidak ada kebutuhan seed baru untuk rumpun ini.
Katalog `ApprovalTiers.Tier1`/`Tier2` sudah berupa konstanta kode (bukan tabel), dipakai ulang
apa adanya. Tidak ada tabel master baru yang lahir dari amendment ini — seluruh entity baru
bersifat transaksional (PO, Tukar Faktur, Purchasing Invoice, dst.), bukan master data, sehingga
bagian ini tidak berlaku secara substansial (mengikuti aturan skill: file/bagian tetap ditulis
dengan satu baris alasan, bukan dikosongkan).

## C.11 Yang sengaja tidak dibuat pada amendment ini

| Yang ditolak | Alasan |
|---|---|
| `MstProduct`/`MstItem` sebagai katalog produk pembelian | Tidak ada permintaan eksplisit untuk katalog produk terstruktur; `Keuangan.md` menyebut "Kategori Produk, Nama Produk, Satuan" sebagai field deskriptif, bukan referensi ke master. `FinPurchaseOrderItem`/`FinPurchasingInvoiceItem` memakai kolom teks bebas. Bila ke depan dibutuhkan katalog terstruktur, ini keputusan terpisah yang butuh scope baru |
| `FinGoodsReceipt` tanpa `PurchaseOrderId` (nullable) | Tidak ada keputusan eksplisit yang meminta penerimaan barang tanpa PO mendahului; berbeda dari Tukar Faktur yang eksplisit diizinkan tanpa PO (`FIN-DEC-051`). Menjaga `PurchaseOrderId` wajib mencegah kapabilitas yang tidak diminta ikut terbangun diam-diam |
| Menghapus jalur input manual `FinSupplierPayable` | Tidak ada keputusan yang meminta deprekasi; beberapa supplier mungkin belum onboarding ke alur PO. Menghapusnya adalah keputusan produk terpisah di luar scope amendment ini |
| Kode kejadian akuntansi baru untuk pengakuan utang supplier (nilai pokok, bukan PPN) | Belum diusulkan ke Accounting — `evidence/06` hanya mengusulkan kode PPN Masukan. Mengusulkan keduanya sekaligus tanpa keputusan terpisah berisiko keliru menggabungkan dua kejadian yang lawan jurnalnya berbeda (lihat `evidence/06` bagian 2) |
| Tabel Accounting/COA untuk PPN Masukan di sisi Finance | Akun dan waktu pengakuan adalah wewenang Accounting (`evidence/06` bagian 3), bukan sesuatu yang diusulkan/dibangun Finance |
| Satu tabel gabungan untuk `FinPaymentDeduction` dan `FinReceiptDeduction` | Arah aliran dan invariant-nya berlawanan (`FIN-DEC-049`, `FIN-DEC-055`) — menggabungkannya memaksa satu tabel menampung dua semantik berbeda, meniru kesalahan yang sudah pernah dikoreksi pada `FIN-DEC-032` (kode gabungan uang muka) |
| `FinReceivableInvoiceBatch` menggantikan `FinReceivable`/`BilInvoice` sebagai unit internal | `FIN-DEC-048` eksplisit menyatakan batch adalah lapisan baru DI ATAS unit yang sudah ada, bukan pengganti — mengubah `FinReceivable` jadi unit sekunder akan memaksa migrasi ulang seluruh rumpun AR yang sudah berjalan (`FIN-DES-010`..`013`) tanpa keputusan yang memintanya |
| Endpoint kirim otomatis kejadian `PPN-MASUKAN-PEMBELIAN` tanpa feature gate | `FIN-DEC-046` menetapkan ratifikasi Accounting sebagai syarat keras sebelum rumpun ini masuk `/plan-module-delivery` — mengaktifkan pengiriman tanpa gate melanggar keputusan itu langsung |


# AMENDMENT REVISI 5 — Sumber Dana Deposit Retur dan Jalur Potongan AR

## D.1 Mengapa amendment ini ada

Amendment ini menggambar skema yang dituntut empat keputusan yang sudah `approved`:
`FIN-DEC-057` (Deposit Retur dipakai di dalam `FinPayment`), `FIN-DEC-058` (kode kejadian
potongan AR), `FIN-DEC-061` (kode kejadian retur dan pemakaian deposit), dan `FIN-DEC-062`
(kode pembalikan potongan AR). Keputusan bisnisnya sudah turun; yang belum ada adalah
bentuk tabel dan urutan transaksi yang mewujudkannya.

Pembacaan source pada `96bf9746` menemukan **dua kekurangan** pada rancangan REVISI 4 yang
ikut diperbaiki di sini, karena keduanya membuat `BE-FIN-036` dan `BE-FIN-040` tidak dapat
dibangun apa adanya:

| # | Kekurangan pada REVISI 4 | Akibat bila dibangun apa adanya |
|---|---|---|
| 1 | `FinSupplierReturnDepositUsage.PurchasingInvoiceId` menunjuk invoice, bukan pembayaran | Pemakaian deposit tidak punya tempat di siklus `FinPayment`, padahal `FIN-DEC-057` menaruhnya di sana. Invoice `APPROVED` juga sudah beku (`FIN-STATE-1.2` B.4) |
| 2 | `FinReceiptDeduction` hanya punya `ReceiptId` | Satu penerimaan dapat dialokasikan ke banyak piutang — tidak ada cara tahu piutang **mana** yang dikurangi PPh 23-nya |

Keduanya menyentuh tabel yang **belum dibangun** (`BE-FIN-030`, `BE-FIN-038` belum
dikerjakan), sehingga koreksinya tidak menuntut pembetulan data. Satu-satunya tabel **yang
sudah berjalan** yang berubah adalah `FinPayment` (satu kolom baru, satu check constraint
diganti) — lihat D.6.

## D.2 Keputusan arsitektur baru

### FIN-DES-045 — Deposit Retur sebagai baris sumber dana di tingkat pembayaran

`FIN-DEC-057` menyebut deposit dipakai sebagai "baris alokasi non-tunai di dalam `FinPayment`".
Desain ini mewujudkannya sebagai **baris sumber dana di tingkat pembayaran**, bukan penanda
pada tiap `FinPaymentAllocation`. Alasannya satu dan bersifat teknis, bukan keputusan bisnis
baru:

> `FIN-VAL-057` melarang satu utang muncul dua kali dalam satu pembayaran. Bila sumber dana
> ditandai per baris alokasi, satu faktur yang dilunasi "Rp 3.000.000 transfer + Rp 2.000.000
> deposit" butuh dua baris alokasi ke utang yang sama — langsung ditolak aturan itu.

Pola baris sumber dana di tingkat pembayaran **sudah ada** dan sudah berjalan:
`FinPaymentDeduction` (`FIN-DES-026`). Deposit mengikuti bentuk yang sama.

| Hal | Ketentuan |
|---|---|
| Alokasi ke utang | **Tidak berubah.** `FinPaymentAllocation` tetap melunasi utang sebesar nilai penuhnya; `OutstandingAmount` berkurang lewat `FinancePaymentService.MarkPaidAsync` yang sudah ada — nol penulis baru |
| Sumber dana deposit | `FinSupplierReturnDepositUsage` kini menunjuk `PaymentId` (bukan lagi `PurchasingInvoiceId`) |
| Uang yang benar-benar keluar | `NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount` |
| Kolom baru pada `FinPayment` | `DepositAppliedAmount` — jumlah baris pemakaian deposit yang belum dilepas |
| Siapa boleh memakai deposit | Hanya `PaymentType = SUPPLIER`, dan hanya deposit milik supplier yang sama dengan `PayeeReferenceId` pembayaran |
| Kapan boleh ditambah/dilepas | Hanya selama pembayaran `DRAFT` — sama persis dengan aturan potongan (`FIN-STATE-0.2` A.2) |

**Contoh berangka.** Utang ke PT Contoh Farma dua faktur: Rp 6.000.000 dan Rp 4.000.000. Ada
Deposit Retur Rp 2.500.000 dari retur bulan lalu.

| Komponen | Nilai |
|---|---|
| `TotalAmount` (dua alokasi) | Rp 10.000.000 |
| `DepositAppliedAmount` | Rp 2.500.000 |
| `NetTransferAmount` | Rp 7.500.000 — yang benar-benar ditransfer |
| Sisa kedua utang sesudah `PAID` | Rp 0 dan Rp 0 |
| Sisa deposit | Rp 0 → deposit `EXHAUSTED` |

### FIN-DES-046 — Reservasi deposit dan pelepasannya

Deposit **dicadangkan** saat baris pemakaian ditambahkan ke pembayaran `DRAFT`, bukan saat
pembayaran `PAID`. Alasan: bila baru dikurangi saat `PAID`, dua pembayaran `DRAFT` dapat
sama-sama memakai deposit yang sama dan baru bertabrakan di ujung — setelah keduanya disetujui.

| Kejadian pada pembayaran | Status baris pemakaian | `AvailableAmount` deposit |
|---|---|---|
| Baris ditambahkan (pembayaran `DRAFT`) | `RESERVED` | Berkurang seketika |
| Baris dilepas oleh petugas (masih `DRAFT`) | `RELEASED` | Kembali |
| Pembayaran `REJECTED` atau `CANCELLED` | Seluruh baris `RESERVED` → `RELEASED` | Kembali |
| Pembayaran `PAID` | Seluruh baris `RESERVED` → `APPLIED` | Tidak bergerak lagi (sudah dikurangi saat reservasi) |

Baris **tidak pernah dihapus** — pelepasan adalah perubahan status, sehingga jejak "deposit ini
pernah dicadangkan lalu dilepas" tetap terbaca. Penulisan `AvailableAmount` hanya oleh
`FinanceSupplierReturnService`; `FinancePaymentService` memanggilnya, tidak menulis kolom deposit
sendiri. Seluruhnya `Serializable` dengan kunci `FIN_RETURN_DEPOSIT_{id}` dan `RowVersion`
deposit.

### FIN-DES-047 — Kejadian Accounting untuk retur dan pemakaian deposit (`FIN-DEC-061`)

| Kapan | Kode | `SourceTransactionId` | `Amount` |
|---|---|---|---|
| Retur `CONFIRMED` | `RETUR-PEMBELIAN` (ke-28) | `ReturnNumber` | `FinSupplierReturn.TotalAmount` |
| Pembayaran `PAID` dengan `DepositAppliedAmount > 0` | `PEMAKAIAN-DEPOSIT-RETUR` (ke-29) | `PaymentNumber` | `DepositAppliedAmount` |
| Pembayaran `PAID` (yang sudah ada) | `AP_PAYMENT` | `PaymentNumber` | **`TotalAmount − DepositAppliedAmount`** — sebelumnya `TotalAmount` |

**Satu perubahan pada kode yang sudah berjalan:** `FinancePaymentService.MarkPaidAsync`
(baris 553-563) menulis `Amount = payment.TotalAmount`. Sesudah amendment ini nilainya
dikurangi `DepositAppliedAmount`. Untuk seluruh pembayaran yang **tidak** memakai deposit,
`DepositAppliedAmount = 0` sehingga nilainya **identik** dengan hari ini — nol perubahan
perilaku bagi pembayaran lama.

**Pembayaran yang seluruhnya dilunasi deposit** (`TotalAmount − DepositAppliedAmount = 0`) **tidak**
menulis `AP_PAYMENT` sama sekali — `FIN-VAL-079` menolak kejadian bernilai nol, dan memang tidak
ada uang yang bergerak. Hanya `PEMAKAIAN-DEPOSIT-RETUR` yang terbit.

**Keterbatasan yang sudah ada dan tidak diubah amendment ini:** `AP_PAYMENT` memakai
`Components = TOTAL` (`FIN-DEC-038`), sehingga potongan (`DeductionAmount`) tetap tidak terpisah
di kejadian. Amendment ini hanya memisahkan porsi deposit, karena porsi itulah yang tanpa
pemisahan membukukan kas keluar untuk uang yang tidak pernah bergerak.

### FIN-DES-048 — Potongan AR melekat pada baris alokasi, dicatat bersama alokasinya

Menggantikan bentuk `FinReceiptDeduction` pada `FIN-DES-042`/`data-dictionary.md` C.15.

| Hal | Ketentuan |
|---|---|
| Melekat pada | `FinReceiptAllocation` (kolom baru `ReceiptAllocationId`) — alokasi itulah yang menyebut piutang mana (`ReceivableId`) |
| Syarat alokasi | Hanya alokasi `TargetType = RECEIVABLE` yang bukan baris pembalik. Alokasi `INVOICE_DIRECT` tidak punya piutang untuk dikurangi |
| Kapan dicatat | **Dalam permintaan yang sama** dengan alokasinya: `AllocateReceiptRequest` diperluas, tiap baris alokasi boleh membawa daftar potongan. Endpoint `POST /receipts/{id}/deductions` terpisah **dicabut** — setelah uang penerimaan habis teralokasi, `FIN-VAL-120` akan menolaknya, sehingga petugas tidak pernah bisa memakainya pada kasus PPh 23 biasa |
| Efek ke piutang | `FinanceReceiptService` memanggil `FinanceReceivableService.ApplyAllocationAsync(receivableId, deduction.Amount)` yang **sudah ada** — nilai masuk `AllocatedAmount`, sesuai `FIN-DES-042`/`FIN-DEC-055`. Nol penulis `OutstandingAmount` baru |
| Batas nilai | Uang alokasi + seluruh potongan pada baris itu ≤ sisa piutang (perluasan `FIN-VAL-033`). Potongan **tidak** memakai uang penerimaan, sehingga `UnallocatedAmount` tidak bergerak karenanya |
| Nomor | Kolom baru `DeductionNumber` (unik) — dipakai sebagai `SourceTransactionId` kejadian. Tanpa nomor sendiri, dua potongan pada satu penerimaan (PPh 23 **dan** biaya bank) akan berbagi kunci kejadian dan yang kedua terbaca sebagai koreksi yang pertama |

**Contoh berangka.** Piutang PT Asuransi Contoh Rp 10.000.000. Transfer masuk Rp 9.745.000.

| Baris dalam satu permintaan alokasi | Nilai | Efek |
|---|---|---|
| Alokasi uang | Rp 9.745.000 | `UnallocatedAmount` penerimaan → Rp 0 |
| Potongan `PPH23` | Rp 230.000 | `AllocatedAmount` piutang +230.000 |
| Potongan `BANK_ADMIN_FEE` | Rp 25.000 | `AllocatedAmount` piutang +25.000 |
| Sisa piutang | **Rp 0** — `SETTLED` | |

Kejadian yang terbit: satu kejadian penerimaan untuk Rp 9.745.000 (jalur yang sudah ada), dan
**dua** `POTONGAN-PIUTANG-NON-TUNAI` — Rp 230.000 dan Rp 25.000, masing-masing dengan
`DeductionNumber` sendiri.

### FIN-DES-049 — Pembalikan potongan AR (`FIN-DEC-062`)

Saat sebuah alokasi dibalik — manual oleh petugas, atau otomatis karena tender Billing
dibatalkan (`FIN-DEC-021`) — **seluruh potongan pada alokasi itu ikut dibalik dalam transaksi
yang sama**:

1. Untuk tiap `FinReceiptDeduction` pada alokasi itu, dibuat baris pembalik (`IsReversal = true`,
   `ReversalOfDeductionId` menunjuk baris asli, `DeductionNumber` baru).
2. `FinanceReceivableService.ReverseAllocationAsync(receivableId, deduction.Amount)` yang
   **sudah ada** dipanggil untuk tiap baris — piutang terbuka kembali.
3. Satu kejadian `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` per baris pembalik, `Amount` positif.

Potongan **tidak dapat dibalik sendirian** tanpa membalik alokasinya. Potongan yang keliru
dibetulkan dengan membalik alokasinya lalu mencatat ulang — sama seperti `FIN-STATE-1.2` B.8.

### FIN-DES-050 — Kejadian potongan AR (`FIN-DEC-058`)

| Kapan | Kode | `SourceTransactionId` | `Amount` |
|---|---|---|---|
| Potongan dicatat | `POTONGAN-PIUTANG-NON-TUNAI` (ke-26) | `DeductionNumber` | `FinReceiptDeduction.Amount` |
| Potongan dibalik | `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` (ke-27) | `DeductionNumber` baris pembalik | `Amount` baris asli, positif |

Kode **tidak pernah** `AR_PAYMENT`, `PENERIMAAN-PIUTANG`, maupun `PENYESUAIAN-PIUTANG`.

## D.3 Tabel kepemilikan data — perubahan

Tidak ada kelompok data baru dan tidak ada pemilik yang berubah. Yang berubah hanya bentuk dua
tabel Finance yang belum dibangun, dan satu kolom pada tabel Finance yang sudah berjalan.

| Kelompok data | Pemilik | Perubahan |
|---|---|---|
| Pembayaran keluar | Finance (`FinPayment`) | Tambah `DepositAppliedAmount`; ganti `CK_FinPayment_NetTransfer` |
| Pemakaian Deposit Retur | Finance (`FinSupplierReturnDepositUsage`) | `PurchasingInvoiceId` → `PaymentId`; tambah `Status`, `ReleasedAt`, `RowVersion` |
| Potongan AR | Finance (`FinReceiptDeduction`) | Tambah `ReceiptAllocationId`, `DeductionNumber`, `IsReversal`, `ReversalOfDeductionId` |

## D.4 Class diagram — sesudah amendment

```mermaid
classDiagram
    class FinPayment {
        +Guid Id
        +decimal TotalAmount
        +decimal DeductionAmount
        +decimal AdditionAmount
        +decimal DepositAppliedAmount  «Baru»
        +decimal NetTransferAmount
        +string Status
    }
    class FinPaymentAllocation {
        +Guid PaymentId
        +Guid? SupplierPayableId
        +decimal Amount
    }
    class FinPaymentDeduction {
        +Guid PaymentId
        +decimal Amount
    }
    class FinSupplierReturnDepositUsage {
        +Guid Id
        +Guid SupplierReturnDepositId
        +Guid PaymentId  «Diganti dari PurchasingInvoiceId»
        +decimal UsedAmount
        +string Status  «Baru: RESERVED/APPLIED/RELEASED»
        +DateTimeOffset UsedAt
        +DateTimeOffset? ReleasedAt  «Baru»
        +Guid RowVersion  «Baru»
    }
    class FinSupplierReturnDeposit {
        +Guid SupplierId
        +decimal AvailableAmount
        +string Status
    }
    FinPayment "1" --> "*" FinPaymentAllocation
    FinPayment "1" --> "*" FinPaymentDeduction
    FinPayment "1" --> "*" FinSupplierReturnDepositUsage : sumber dana
    FinSupplierReturnDeposit "1" --> "*" FinSupplierReturnDepositUsage
```

```mermaid
classDiagram
    class FinReceipt {
        +Guid Id
        +string ReceiptNumber
        +decimal UnallocatedAmount
    }
    class FinReceiptAllocation {
        +Guid Id
        +Guid ReceiptId
        +Guid? ReceivableId
        +string TargetType
        +decimal Amount
        +bool IsReversal
    }
    class FinReceiptDeduction {
        +Guid Id
        +string DeductionNumber  «Baru»
        +Guid ReceiptId
        +Guid ReceiptAllocationId  «Baru»
        +string DeductionType
        +decimal Amount
        +bool IsReversal  «Baru»
        +Guid? ReversalOfDeductionId  «Baru»
    }
    class FinanceReceivableService {
        +ApplyAllocationAsync(receivableId, amount)
        +ReverseAllocationAsync(receivableId, amount)
    }
    FinReceipt "1" --> "*" FinReceiptAllocation
    FinReceiptAllocation "1" --> "*" FinReceiptDeduction
    FinReceiptDeduction ..> FinanceReceivableService : satu-satunya jalur ke OutstandingAmount
```

## D.5 Penjelasan class — perubahan

| Class | Status | Perubahan | Lokasi file |
|---|---|---|---|
| `FinPayment` | Diperbarui | Tambah `DepositAppliedAmount` | `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs`; configuration `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs` |
| `FinSupplierReturnDepositUsage` | Baru (bentuk dikoreksi dari REVISI 4) | Lihat D.3 | `Areas/Corporate/FinanceManagement/Purchasing/Models/`; configuration `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/` |
| `FinSupplierReturnDepositUsageStatuses` | Baru | `RESERVED`, `APPLIED`, `RELEASED` — `static class`, bukan enum `int` | Idem, satu berkas dengan model |
| `FinReceiptDeduction` | Baru (bentuk dikoreksi dari REVISI 4) | Lihat D.3 | `Areas/Corporate/FinanceManagement/Collection/Models/`; configuration `Repositories/Configurations/Corporate/FinanceManagement/Collection/` |
| `FinancePaymentService` | Diperbarui | (a) `AddReturnDepositAsync`/`ReleaseReturnDepositAsync` — hanya `DRAFT`, membuka transaksi `Serializable`; (b) hitung `NetTransferAmount` dengan `DepositAppliedAmount`; (c) `SubmitAsync`: `FIN-VAL-091` hanya berlaku bila `DepositAppliedAmount = 0`; (d) `RejectAsync`/`CancelAsync`: lepas seluruh baris `RESERVED`; (e) `MarkPaidAsync`: baris → `APPLIED`, `AP_PAYMENT` bernilai `TotalAmount − DepositAppliedAmount` (dilewati bila nol), tulis `PEMAKAIAN-DEPOSIT-RETUR`; nomor bukti transfer wajib hanya bila `NetTransferAmount > 0` | `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` |
| `FinanceSupplierReturnService` | Baru (REVISI 4) + tanggung jawab tambahan | Satu-satunya penulis `AvailableAmount`: `ReserveAsync`, `ReleaseAsync`, `MarkAppliedAsync` — dipanggil `FinancePaymentService`, **ikut** transaksinya (tidak membuka transaksi sendiri); tulis `RETUR-PEMBELIAN` saat retur `CONFIRMED` | `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` |
| `FinancePaymentsController` | Diperbarui | `GET`/`POST /payments/{id}/return-deposits`, `DELETE /payments/{id}/return-deposits/{usageId}` | `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` |
| `FinanceSupplierReturnsController` | Baru (REVISI 4), cakupan menyempit | Endpoint `POST /deposits/{id}/apply` **dicabut** | `Areas/Corporate/FinanceManagement/Purchasing/Controllers/` |
| `FinanceReceiptService` | Diperbarui | `AllocateAsync` menerima potongan per baris alokasi; `ReverseAllocationAsync` ikut membalik potongannya; tulis `POTONGAN-…`/`PEMBALIKAN-POTONGAN-…` | `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` |
| `FinanceReceiptsController` | Diperbarui | `GET /receipts/{id}/deductions` saja (baca); `POST` dicabut | `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` |
| `FinanceReceivableService` | **Sudah ada — tidak diubah** | `ApplyAllocationAsync`/`ReverseAllocationAsync` dipakai apa adanya | `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` |
| `FinAccountingEventTypeCodes` | Diperbarui | Empat konstanta: `PotonganPiutangNonTunai`, `PembalikanPotonganPiutangNonTunai`, `ReturPembelian`, `PemakaianDepositRetur` (bersama `PpnMasukanPembelian` dari REVISI 4) | `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` |

**DTO yang berubah.**

| DTO | Jenis | Field |
|---|---|---|
| `AddPaymentReturnDepositRequest` | Create | `SupplierReturnDepositId` (Guid, wajib), `UsedAmount` (decimal > 0, wajib), `ExpectedRowVersion` (Guid, wajib — milik pembayaran) |
| `PaymentReturnDepositResponse` | Response | `Id`, `SupplierReturnDepositId`, `ReturnNumber`, `UsedAmount`, `Status`, `UsedAt`, `ReleasedAt` |
| `PaymentDetailResponse` | Response, diperluas | Tambah `depositAppliedAmount`, `returnDeposits[]` |
| `AllocateReceiptRequest.Lines[]` | Create, diperluas | Tiap baris tambah `Deductions[]`: `DeductionType` (`PPH23`/`BANK_ADMIN_FEE`/`OTHER`), `Amount`, `Reason` (wajib bila `OTHER`), `ReferenceNumber` |
| `ReceiptDeductionResponse` | Response | `Id`, `DeductionNumber`, `ReceiptAllocationId`, `ReceivableNumber`, `DeductionType`, `Amount`, `IsReversal`, `ReversalOfDeductionId` |

## D.6 Status model dan dampak migration

| Tabel | Status | Kolom yang berubah |
|---|---|---|
| `FinPayment` | **Diperbarui — tabel sudah berjalan** | Tambah `DepositAppliedAmount numeric(18,2) NOT NULL DEFAULT 0`. Ganti `CK_FinPayment_NetTransfer` menjadi `NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount`. Tambah `CK_FinPayment_DepositApplied` (`DepositAppliedAmount >= 0`) |
| `FinSupplierReturnDepositUsage` | Baru — **menggantikan** bentuk `data-dictionary.md` C.11 | `PurchasingInvoiceId` diganti `PaymentId`; tambah `Status`, `ReleasedAt`, `RowVersion` |
| `FinReceiptDeduction` | Baru — **menggantikan** bentuk `data-dictionary.md` C.15 | Tambah `DeductionNumber`, `ReceiptAllocationId`, `IsReversal`, `ReversalOfDeductionId` |

## D.7 Rencana migration

| Urutan | Migration | Isi | Tanpa downtime? | Cara mundur |
|---:|---|---|---|---|
| 1 | `AddPurchasingApRumpun` (REVISI 4, `BE-FIN-031`) | **Dibuat dengan bentuk D.6** untuk `FinSupplierReturnDepositUsage` — belum pernah ditulis, jadi tidak ada migration koreksi | Ya | Sama seperti REVISI 4 |
| 2 | `AddArInvoiceBatchAndReceiptDeduction` (REVISI 4, `BE-FIN-038`) | **Dibuat dengan bentuk D.6** untuk `FinReceiptDeduction` | Ya | Sama seperti REVISI 4 |
| 3 | `AddDepositAppliedAmountToFinPayment` (**baru**) | `ADD COLUMN ... DEFAULT 0`; `DROP CONSTRAINT CK_FinPayment_NetTransfer`; `ADD CONSTRAINT` bentuk baru; `ADD CONSTRAINT CK_FinPayment_DepositApplied` | Ya — seluruh baris lama bernilai `0` sehingga constraint baru langsung terpenuhi; tidak ada backfill | `DROP CONSTRAINT` baru, `ADD CONSTRAINT` bentuk lama, `DROP COLUMN`. **Aman hanya** bila belum ada baris dengan `DepositAppliedAmount > 0` |

Migration 3 bergantung pada tabel `FinPayment` yang sudah ada (`AddFinancePayment`, `BE-FIN-020` ✅)
dan harus dijalankan **sebelum** kode `BE-FIN-036` membaca kolomnya. Seperti seluruh dokumen ini,
bagian ini hanya rencana — wewenang membuat dan menjalankan migration tetap terpisah.

## D.8 Rencana data master awal

Tidak berlaku — amendment ini tidak menambah tabel master. Empat konstanta kode kejadian
adalah konstanta kode, bukan data tabel.

## D.9 Invariant yang ditambahkan

| Invariant | Ditegakkan di |
|---|---|
| `NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount` | Check constraint + service |
| `DepositAppliedAmount` = jumlah `UsedAmount` baris pemakaian berstatus `RESERVED` atau `APPLIED` | Service (`FinancePaymentService`) |
| Deposit hanya dipakai untuk pembayaran `SUPPLIER` kepada supplier pemilik deposit | Service |
| `FinSupplierReturnDeposit.AvailableAmount >= 0` | Check constraint (REVISI 4) + kunci `Serializable` |
| Satu baris potongan hanya dibalik sekali | Unique index parsial `ReversalOfDeductionId` |
| Uang alokasi + potongan pada satu baris alokasi ≤ sisa piutang | Service + `FinanceReceivableService` (sudah menolak alokasi melebihi sisa) |

## D.10 Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| Kolom `FundingSource` pada `FinPaymentAllocation` | Bertabrakan dengan `FIN-VAL-057` — lihat `FIN-DES-045` |
| Baris `FinPaymentAllocation` berjenis "deposit" | Sama seperti di atas, dan membuat `AllocatedAmount` tidak lagi berarti "utang yang dilunasi" |
| Mengurangi `AvailableAmount` baru saat `PAID` | Dua pembayaran `DRAFT` dapat memakai deposit yang sama tanpa ketahuan sampai ujung — lihat `FIN-DES-046` |
| `BankAccountId` nullable untuk pembayaran yang seluruhnya dari deposit | Mengubah kolom wajib pada tabel yang sudah berjalan demi kasus tepi. Rekening tetap wajib dipilih walau nilai transfernya nol; bila owner ingin menghapus kewajiban ini, itu keputusan terpisah |
| Endpoint `POST /receipts/{id}/deductions` terpisah | Tidak dapat dipakai pada kasus PPh 23 biasa — lihat `FIN-DES-048` |
| Membalik potongan tanpa membalik alokasinya | Potongan tanpa alokasi tidak punya arti; koreksi lewat pembalikan alokasi |
| Memisahkan potongan pembayaran (`DeductionAmount`) dari `AP_PAYMENT` | Di luar cakupan — keterbatasan `FIN-DEC-038` yang berlaku sejak REVISI 2, tidak dibuka ulang di sini |
