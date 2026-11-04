# Optik — ERD Pesanan dan Layanan Purna

| Field | Nilai |
| --- | --- |
| Konteks | `BC-OPT-02` Optik — Pesanan & Layanan Purna |
| Revision | `1` — `draft` |
| Keputusan | `OPT-DEC-010`, `013`, `015`, `016`, `018`, `019`, `020`, `028`, `029`, `034`, `035`, `038`, `041`, `043`, `044`, `048`, `049` |

```mermaid
erDiagram
    OptOrder {
        uuid Id PK
        varchar OrderNumber UK "OPT-ORD-..."
        uuid EncounterId FK "milik Registrasi"
        uuid PatientId FK
        int OrderKind "Fabrication/DirectSale/Remake"
        uuid PrescriptionId FK
        uuid PrescriptionVersionId FK "versi yang dikerjakan"
        int Status "enum OptOrderStatus"
        numeric DownPaymentMinPercentSnapshot
        int WorkshopType "InHouse/ExternalLab"
        uuid ExternalLabSupplierId FK "milik Master Data"
        date EstimatedReadyDate
        timestamp HandedOverAt "awal garansi"
        boolean HandoverIdentityVerified
        uuid ParentOrderId FK "pesanan asal remake"
        int Version
    }
    OptOrderItem {
        uuid Id PK
        uuid OrderId FK
        int LineNo "UK bersama OrderId"
        uuid DrugId FK "milik Master Data"
        int ItemCategory
        int LensSupply "Stock/Custom"
        numeric Quantity
        int Status "Active/Cancelled"
        boolean IsLensCut
        timestamp StockIssuedAt
        timestamp StockReturnedAt
        int BillingFactVersion
    }
    OptOrderCancellation {
        uuid Id PK
        uuid OrderId FK
        int Status "Requested/Approved/Rejected"
        varchar Reason
        uuid RequestedBy
        uuid DecidedBy
    }
    OptOrderStatusHistory {
        uuid Id PK
        uuid OrderId FK
        int FromStatus
        int ToStatus
        uuid ActorUserId
        timestamp OccurredAt
        varchar Reason
    }
    OptWarrantyClaim {
        uuid Id PK
        varchar ClaimNumber UK "OPT-CLM-..."
        uuid OrderId FK "pesanan asal"
        uuid EncounterId FK "kunjungan keluhan"
        int ComplaintType
        boolean IsWithinWarranty
        date WarrantyDeadlineSnapshot
        int Status
        uuid ReexamRefractionExamId FK
        uuid RemakeOrderId FK
        int Version
    }
    OptOrder ||--|{ OptOrderItem : "1:N — Baru"
    OptOrder ||--o{ OptOrderCancellation : "1:N — Baru"
    OptOrder ||--|{ OptOrderStatusHistory : "1:N — Baru"
    OptOrder ||--o{ OptWarrantyClaim : "1:N — Baru"
    OptWarrantyClaim |o--o| OptOrder : "0..1:0..1 — remake"
    OptOrder |o--o{ OptOrder : "0..1:N — pesanan asal"
    OptPrescriptionVersion ||--o{ OptOrder : "1:N — Baru"
    MstDrug ||--o{ OptOrderItem : "1:N — Sudah ada → Baru"
    MstSupplier |o--o{ OptOrder : "0..1:N — Sudah ada → Baru"
```

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `OptOrder` | Baru | Optik | Root pesanan; tanpa harga dan diskon |
| `OptOrderItem` | Baru | Optik | Baris barang; satu fakta tagihan per item |
| `OptOrderCancellation` | Baru | Optik | Pengajuan + keputusan batal |
| `OptOrderStatusHistory` | Baru | Optik | Jejak transisi |
| `OptWarrantyClaim` | Baru | Optik | Keluhan garansi |
| `MstDrug` | Sudah ada | Master Data | Barang Optik |
| `MstSupplier` | Sudah ada | Administrator Master Data | Lab lensa luar |
