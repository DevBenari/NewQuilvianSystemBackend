# CRM Membership — ERD Konteks

| Field | Nilai |
|---|---|
| Revision | `1`, status `draft` |
| Masukan | `03-domain-architecture.md` revision 1, `02-backend-architecture.md` revision 1 |
| Backend SHA | `1aecf642` |

Peta ini menunjukkan arah ketergantungan antar context. Panah dari A ke B berarti A
menyimpan rujukan ke data milik B. Detail kolom ada di `patient-membership.md`.

```mermaid
erDiagram
    MstPatient {
        uuid Id PK
        varchar MedicalRecordNumber UK
        boolean IsMember "proyeksi, ditulis BC-MBR"
        uuid ActivePatientMembershipId "proyeksi"
    }
    MstMembershipTier {
        uuid Id PK
        varchar TierCode UK
        int TierType "enum"
        numeric AnnualFeeAmount "baru"
    }
    MstPatientMembership {
        uuid Id PK
        uuid PatientId FK
        uuid MembershipTierId FK
        varchar MemberNumber UK
        int MembershipStatus "enum"
    }
    RegPatientEncounter {
        uuid Id PK
        uuid PatientMembershipIdSnapshot "usulan AG-01"
        uuid MembershipTierIdSnapshot "usulan AG-01"
    }
    RegQueue {
        uuid Id PK
        uuid PatientMembershipIdSnapshot "sudah ada"
        uuid MembershipTierIdSnapshot "sudah ada"
    }
    BilInvoice {
        uuid Id PK
        uuid EncounterId FK
    }
    MstPatient ||--o{ MstPatientMembership : "1:N — Diperbarui, BC-PAT ke BC-MBR"
    MstMembershipTier ||--o{ MstPatientMembership : "1:N — Diperbarui, BC-TIER ke BC-MBR"
    MstPatientMembership |o--o{ RegPatientEncounter : "snapshot — usulan AG-01"
    MstPatientMembership |o--o{ RegQueue : "snapshot — Sudah ada"
    RegPatientEncounter ||--o{ BilInvoice : "1:N — Sudah ada, billing membaca snapshot"
```

| Context | Entity | Status | Owner |
|---|---|---|---|
| `BC-PAT` | `MstPatient` | Sudah ada (proyeksi status ditulis `BC-MBR`) | Patient Management |
| `BC-TIER` | `MstMembershipTier` | Diperbarui | Administrator / Master Data |
| `BC-MBR` | `MstPatientMembership`, `PatMembershipStatusHistory` | Diperbarui / Baru | Patient Management |
| `BC-REG` | `RegPatientEncounter` | Diperbarui (usulan `AG-01`) | Registration |
| `BC-REG` | `RegQueue` | Sudah ada | Registration |
| `BC-BIL` | `BilInvoice` | Sudah ada. Tagihan iuran menunggu `DEC-CRM-001` | Billing |

Rujukan snapshot dari kunjungan ke membership adalah rujukan historis. Snapshot tidak
berubah walaupun status membership berubah sesudahnya (`CRM-DEC-005`).
