# ERD — Utang Supplier, Utang Dokter, dan Pembayaran

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Backend SHA | `09101d05` |

Kolom audit warisan `IdentityModel` tidak digambar.

## 1. Utang

```mermaid
erDiagram
    MstSupplier {
        uuid Id PK
        varchar SupplierCode UK
        varchar SupplierName
        int PaymentTermDays
        numeric CreditLimitAmount
        varchar BankAccountNumber
        boolean IsActive
    }
    FinSupplierPayable {
        uuid Id PK
        varchar PayableNumber UK
        uuid SupplierId FK
        varchar SupplierInvoiceNumber UK "unik bersama SupplierId"
        date SupplierInvoiceDate
        numeric OriginalAmount
        numeric OutstandingAmount "tidak boleh negatif"
        numeric PaidAmount
        numeric AdjustedAmount
        date DueDate
        int PaymentTermDays "disalin dari MstSupplier saat dibuat"
        varchar Status "OUTSTANDING, PARTIAL, PAID, CANCELLED"
        uuid RowVersion
    }
    FinSupplierPayableItem {
        uuid Id PK
        uuid PayableId FK
        varchar Description
        numeric Quantity
        numeric UnitPrice
        numeric Amount
    }
    FinDoctorPayable {
        uuid Id PK
        varchar PayableNumber UK
        uuid DoctorId FK
        uuid SourceDoctorServiceFeeId UK "satu fee disetujui = satu utang"
        uuid SourceApHandoffId "rujukan kesiapan, bukan sumber nilai"
        varchar PeriodCode
        numeric OriginalAmount
        numeric OutstandingAmount
        numeric PaidAmount
        numeric AdjustedAmount
        varchar Status "OUTSTANDING, PARTIAL, PAID, CANCELLED"
        timestamp RecognizedAt
        uuid RowVersion
    }
    FinDoctorPayableItem {
        uuid Id PK
        uuid PayableId FK
        uuid SourceServiceFeeDetailId
        varchar Description
        numeric Amount
    }
    MstSupplier ||--o{ FinSupplierPayable : "1:N — Sudah ada, milik Administrator"
    FinSupplierPayable ||--o{ FinSupplierPayableItem : "1:N — Baru"
    FinDoctorPayable ||--o{ FinDoctorPayableItem : "1:N — Baru"
```

## 2. Pembayaran dan alokasinya

```mermaid
erDiagram
    MstBankAccount {
        uuid Id PK
        uuid BankId FK
        varchar AccountNumber
        varchar AccountType "OPERATIONAL, COLLECTION, PAYMENT"
        boolean IsActive
    }
    FinPayment {
        uuid Id PK
        varchar PaymentNumber UK
        varchar PaymentType "SUPPLIER, DOCTOR"
        uuid PayeeReferenceId "SupplierId atau DoctorId"
        uuid BankAccountId FK
        varchar PaymentMethod "TRANSFER, CASH, CHEQUE"
        numeric TotalAmount
        numeric AllocatedAmount "wajib sama dengan TotalAmount sebelum PAID"
        varchar Status "DRAFT, SUBMITTED, APPROVED, PAID, REJECTED, CANCELLED"
        varchar ApprovalTier "diisi service dari total nominal"
        uuid RequestedBy
        uuid ApprovedBy "wajib beda dari RequestedBy"
        timestamp PaidAt
        varchar ReferenceNumber
        uuid RowVersion
    }
    FinPaymentAllocation {
        uuid Id PK
        uuid PaymentId FK
        varchar PayableType "SUPPLIER, DOCTOR"
        uuid SupplierPayableId FK "tepat satu terisi"
        uuid DoctorPayableId FK "tepat satu terisi"
        numeric Amount
        boolean IsReversal
        uuid ReversalOfAllocationId FK
    }
    FinPayableAdjustment {
        uuid Id PK
        varchar AdjustmentNumber UK
        varchar PayableType "SUPPLIER, DOCTOR"
        uuid SupplierPayableId FK
        uuid DoctorPayableId FK
        varchar Direction "DEBIT, CREDIT"
        numeric Amount
        varchar Status "REQUESTED, APPROVED, REJECTED"
        uuid RequestedBy
        uuid ApprovedBy
        uuid RowVersion
    }
    FinSupplierPayable {
        uuid Id PK
        numeric OutstandingAmount
    }
    FinDoctorPayable {
        uuid Id PK
        numeric OutstandingAmount
    }
    MstBankAccount ||--o{ FinPayment : "1:N — Baru"
    FinPayment ||--o{ FinPaymentAllocation : "1:N — Baru"
    FinSupplierPayable ||--o{ FinPaymentAllocation : "1:N — Baru"
    FinDoctorPayable ||--o{ FinPaymentAllocation : "1:N — Baru"
    FinSupplierPayable ||--o{ FinPayableAdjustment : "1:N — Baru"
    FinDoctorPayable ||--o{ FinPayableAdjustment : "1:N — Baru"
    FinPaymentAllocation |o--o| FinPaymentAllocation : "0:1 — Baru, pembalikan"
```

## 3. Status entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstSupplier` | Sudah ada | Administrator | Dipakai apa adanya (`FIN-DEC-014`); **MUST NOT** disalin ke Finance |
| `FinSupplierPayable` | Baru | Finance | Diinput manual staf AP (`FIN-DEC-015`) |
| `FinSupplierPayableItem` | Baru | Finance | Jumlah barisnya wajib sama dengan `OriginalAmount` induk |
| `FinDoctorPayable` | Baru | Finance | Lahir dari fee yang sudah disetujui Medical Fee, bukan dari `BilApHandoff` |
| `FinDoctorPayableItem` | Baru | Finance | Nilainya disalin, tidak dihitung ulang |
| `FinPayment` | Baru | Finance | Satu entity untuk supplier dan dokter (`FIN-DES-015`) |
| `FinPaymentAllocation` | Baru | Finance | Dua FK nullable + check constraint (`FIN-DES-016`) |
| `FinPayableAdjustment` | Baru | Finance | Memakai aturan tepat-satu-FK yang sama |
| `MstBankAccount` | Baru | Finance | Rekening sumber pembayaran |
| `DoctorServiceFee` | Sudah ada di luar | Medical Fee | Hanya dirujuk lewat `SourceDoctorServiceFeeId` |

## 4. Mengapa satu pembayaran boleh melunasi banyak utang

Ini jawaban langsung atas `FIN-DEC-019`. Owner menyebut masalah nyata pada sistem lama: dokter
dobel dibayar atau justru terlewat, karena rekap dikerjakan di Excel terpisah dari sistem.

Bentuk `FinPayment` satu-ke-banyak `FinPaymentAllocation` membuat rekap itu **hidup di dalam
sistem**, bukan di luar. Contoh konkret:

> Rumah sakit membayar dr. Andi (nama samaran) untuk bulan Agustus 2026 dengan satu transfer
> sebesar Rp 24.500.000. Transfer itu sebenarnya melunasi tiga utang: fee rawat jalan
> Rp 12.000.000, fee tindakan Rp 9.500.000, dan fee visite Rp 3.000.000.
>
> Yang tersimpan: satu baris `FinPayment` bernilai Rp 24.500.000, dan **tiga** baris
> `FinPaymentAllocation` yang masing-masing menunjuk satu `FinDoctorPayable`. Setiap utang itu
> `OutstandingAmount`-nya menjadi nol.
>
> Kalau kemudian ada pertanyaan "fee tindakan Agustus dr. Andi sudah dibayar belum?", jawabannya
> dapat ditelusuri satu query — bukan dengan membuka Excel.

Aturan yang menjaganya: `FinPayment.TotalAmount` **MUST** sama dengan jumlah seluruh alokasinya
sebelum statusnya boleh menjadi `PAID`. Dalam contoh di atas,
12.000.000 + 9.500.000 + 3.000.000 = 24.500.000. Bila staf keliru mengalokasikan hanya dua dari
tiga utang, totalnya menjadi Rp 21.500.000, tidak sama dengan Rp 24.500.000, dan sistem menolak
pembayaran itu diselesaikan.

## 5. Aturan tepat-satu-FK

`FinPaymentAllocation` dan `FinPayableAdjustment` sama-sama punya dua kolom FK yang keduanya
boleh kosong. Aturannya ditegakkan database, bukan hanya kode:

| `PayableType` | `SupplierPayableId` | `DoctorPayableId` |
|---|---|---|
| `SUPPLIER` | Wajib terisi | Wajib kosong |
| `DOCTOR` | Wajib kosong | Wajib terisi |

Kombinasi lain ditolak `CHECK` constraint. Ini mencegah baris yatim yang menunjuk dua utang
sekaligus atau tidak menunjuk apa pun — kesalahan yang baru ketahuan saat rekonsiliasi kalau
hanya dijaga di kode.

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Revisi | `2`, 20 September 2026 — status `locked` 20 September 2026 |
| Dipicu | `MF-DEC-002`, `MF-DEC-005`, `MF-DEC-008` |
| Keputusan | `FIN-DES-025`..`028` |

## A.1 Bentuk sesudah amendment

```mermaid
erDiagram
    FinMedicalServicePayable {
        uuid Id PK
        varchar PayableNumber UK
        varchar PayeeType "DOCTOR, NURSE, OTHER_PRACTITIONER"
        uuid PayeeReferenceId "SENSITIF"
        uuid SourceMedicalServiceFeeId UK "satu hasil jasa = satu utang"
        varchar PeriodCode
        numeric OriginalAmount
        numeric OutstandingAmount "tidak boleh negatif"
        numeric PaidAmount
        numeric AdjustedAmount
        varchar Status "OUTSTANDING, PARTIAL, PAID, CANCELLED"
        timestamp RecognizedAt
        uuid RowVersion
    }
    FinPayment {
        uuid Id PK
        varchar PaymentNumber UK
        varchar PaymentType "SUPPLIER, MEDICAL_SERVICE"
        numeric TotalAmount "jumlah utang yang dilunasi"
        numeric AllocatedAmount
        numeric DeductionAmount "BARU"
        numeric AdditionAmount "BARU"
        numeric NetTransferAmount "BARU - uang yang benar-benar keluar"
        varchar Status
        uuid RowVersion
    }
    FinPaymentDeduction {
        uuid Id PK
        uuid PaymentId FK
        varchar DeductionType "PPH21, KASBON, PATIENT_DEBT, SITTING_FEE, KSO, IURAN, OTHER"
        varchar Direction "DEDUCTION, ADDITION"
        numeric Amount "selalu positif"
        varchar Reason "wajib bila OTHER"
        varchar ReferenceNumber
    }
    FinPaymentAllocation {
        uuid Id PK
        uuid PaymentId FK
        varchar PayableType "SUPPLIER, MEDICAL_SERVICE"
        uuid SupplierPayableId FK "tepat satu terisi"
        uuid MedicalServicePayableId FK "tepat satu terisi"
        numeric Amount
    }
    FinPayment ||--o{ FinPaymentAllocation : "1:N — Diperbarui"
    FinPayment ||--o{ FinPaymentDeduction : "1:N — Baru"
    FinMedicalServicePayable ||--o{ FinPaymentAllocation : "1:N — Baru"
```

## A.2 Status entity sesudah amendment

| Entity | Status | Catatan |
|---|---|---|
| `FinMedicalServicePayable` | Baru | Menggantikan `FinDoctorPayable`; melayani dokter dan tenaga kesehatan lain |
| `FinMedicalServicePayableItem` | Baru | Menggantikan `FinDoctorPayableItem` |
| `FinPaymentDeduction` | Baru | Potongan dan tambahan per pembayaran |
| `FinPayment` | Diperbarui | Tambah tiga kolom uang |
| `FinPaymentAllocation`, `FinPayableAdjustment` | Diperbarui | `DoctorPayableId` → `MedicalServicePayableId` |
| `FinDoctorPayable`, `FinDoctorPayableItem` | **Dibatalkan** | Tidak pernah dibuat |

## A.3 Tiga angka pada satu pembayaran

Ini bagian yang paling mudah salah dibaca dari ERD di atas.

| Angka | Menjawab pertanyaan | Dihitung dari |
|---|---|---|
| `TotalAmount` | "Berapa utang jasa yang lunas?" | Jumlah seluruh `FinPaymentAllocation` |
| `NetTransferAmount` | "Berapa uang yang benar-benar keluar dari rekening?" | `TotalAmount` − potongan + tambahan |
| `OutstandingAmount` pada utang | "Masih ada sisa kewajiban?" | Berkurang sebesar **alokasinya**, bukan sebesar transfer |

Ketiganya sengaja dipisah. Bila `NetTransferAmount` dipakai mengurangi utang, utang jasa tidak
akan pernah lunas karena potongan pajak dan kasbon akan selamanya tersisa sebagai "kekurangan
bayar" — padahal kewajiban rumah sakit kepada penerima memang sudah selesai (`FIN-DES-028`).
