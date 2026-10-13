# CRM Membership — ERD Context Patient Membership

| Field | Nilai |
|---|---|
| Revision | `1`, status `draft` |
| Backend SHA | `1aecf642` |

Kolom audit `IdentityModel` tidak digambar.

```mermaid
erDiagram
    MstPatientMembership {
        uuid Id PK
        uuid PatientId FK "Restrict"
        uuid MembershipTierId FK "Restrict"
        varchar MemberNumber UK "50, dibuat sistem"
        int MembershipStatus "enum, bawaan PendingPayment"
        int RegistrationType "baru, enum"
        date JoinDate "tanggal daftar WIB"
        date PeriodStartDate "baru, terisi saat lunas"
        date ExpiredDate "tanggal akhir periode, inklusif"
        timestamptz PaymentDueAt "baru, daftar + 7 hari"
        timestamptz ActivatedAt "baru"
        numeric FeeAmountSnapshot "baru, 18,2"
        uuid PreviousPatientMembershipId FK "baru, self, Restrict"
        int CancellationCategory "baru, enum"
        varchar CancellationReason "baru, 500"
        uuid FeeChargeReferenceId "baru, tanpa FK (DEC-CRM-001)"
        boolean IsActive "proyeksi kompatibilitas"
        boolean IsPrimary "proyeksi kompatibilitas"
        uuid RowVersion "baru, concurrency"
    }
    PatMembershipStatusHistory {
        uuid Id PK
        uuid PatientMembershipId FK "Restrict"
        int FromStatus "enum, kosong untuk baris pertama"
        int ToStatus "enum"
        varchar ActionCode "50"
        int Source "enum Staff/System/Billing"
        uuid ActorUserId "kosong bila sistem"
        timestamptz OccurredAt
        varchar Reason "500"
        uuid ReferenceId "tagihan/refund"
        varchar OldNotes "250"
        varchar NewNotes "250"
    }
    MstMembershipTier {
        uuid Id PK
        int TierType "enum"
        int PriorityLevel "urutan upgrade"
        int ValidityMonths
        numeric AnnualFeeAmount "baru"
    }
    MstPatient {
        uuid Id PK
        varchar MedicalRecordNumber UK
        boolean IsMember "proyeksi"
        uuid DefaultMembershipTierId "proyeksi"
        uuid ActivePatientMembershipId "proyeksi"
    }
    MstPatient ||--o{ MstPatientMembership : "1:N — Sudah ada"
    MstMembershipTier ||--o{ MstPatientMembership : "1:N — Sudah ada"
    MstPatientMembership ||--|{ PatMembershipStatusHistory : "1:N — Baru"
    MstPatientMembership |o--o| MstPatientMembership : "0:1 — Baru, menggantikan"
```

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstPatientMembership` | Diperbarui | Patient Management | 10 kolom baru, 1 bawaan berubah, index unik bersyarat diganti |
| `PatMembershipStatusHistory` | Baru | Patient Management | Butuh baris registry folder `PatientManagement/Membership` lebih dulu |
| `MstMembershipTier` | Diperbarui | Administrator / Master Data | 1 kolom baru |
| `MstPatient` | Sudah ada | Patient Management | Direferensikan, **MUST NOT** disalin |

## Index unik bersyarat (penjaga `INV-01` s.d. `INV-03`)

| Index | Kolom | Filter | Arti bagi pengguna |
|---|---|---|---|
| `UX_MstPatientMembership_Patient_Active` | `PatientId` | `MembershipStatus = 1 AND IsDelete = false` | Satu pasien tidak bisa punya dua membership Aktif |
| `UX_MstPatientMembership_Patient_Scheduled` | `PatientId` | `MembershipStatus = 8 AND IsDelete = false` | Paling banyak satu perpanjangan Terjadwal |
| `UX_MstPatientMembership_Patient_Pending` | `PatientId` | `MembershipStatus = 7 AND IsDelete = false` | Paling banyak satu pendaftaran Menunggu Pembayaran |

Contoh: petugas A dan petugas B mendaftarkan Gold untuk pasien yang sama pada detik yang sama.
Kunci per pasien membuat salah satunya menunggu. Bila kunci terlewati karena kesalahan, index
`UX_..._Pending` tetap menolak baris kedua, dan petugas B menerima pesan "Pasien ini sudah
punya pendaftaran yang menunggu pembayaran" (kode 409).
