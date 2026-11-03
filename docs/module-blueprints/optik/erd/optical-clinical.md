# Optik — ERD Klinis (Pemeriksaan dan Resep)

| Field | Nilai |
| --- | --- |
| Konteks | `BC-OPT-01` Optik — Klinis |
| Revision | `1` — `draft` |
| Keputusan | `OPT-DEC-005`, `007`, `011`, `012`, `017`, `022`, `023`, `039`, `042`, `045`, `046`, `047` |

```mermaid
erDiagram
    OptRefractionExam {
        uuid Id PK
        varchar ExamNumber UK "OPT-EXM-..."
        uuid EncounterId FK "milik Registrasi"
        uuid PatientId FK "milik Patient Management"
        uuid ExaminerWorkforceProfileId FK "milik HR"
        varchar ExaminerLicenseNumberSnapshot "izin saat periksa"
        int Status "enum OptRefractionExamStatus"
        int CurrentRevisionNo
        timestamp FinalizedAt
        timestamp BillingFactEmittedAt "cegah tagih ulang"
        int Version "concurrency"
    }
    OptRefractionExamRevision {
        uuid Id PK
        uuid RefractionExamId FK
        int RevisionNo "UK bersama RefractionExamId"
        varchar VisusMeasurements "8 kolom visus, SENSITIF"
        numeric AutorefMeasurements "6 kolom, SENSITIF"
        numeric SubjectiveMeasurements "8 kolom, SENSITIF"
        numeric PdDistance
        numeric PdNear
        uuid RevisedByWorkforceProfileId FK
        varchar RevisionReason
    }
    OptPrescription {
        uuid Id PK
        varchar PrescriptionNumber UK "OPT-RX-..."
        uuid PatientId FK
        uuid EncounterId FK
        uuid RefractionExamId FK "kosong untuk resep luar"
        int PrescriptionType "Spectacle/ContactLens"
        int Origin "Internal/External"
        date IssuedDate
        int ValidityMonthsSnapshot
        date ValidUntil
        int CurrentVersionNo
        timestamp LockedAt "terisi = terkunci"
        uuid LockedByOrderId FK
        int Version
    }
    OptPrescriptionVersion {
        uuid Id PK
        uuid PrescriptionId FK
        int VersionNo "UK bersama PrescriptionId"
        numeric SpectaclePowers "OD/OS SPH CYL AXIS ADD, SENSITIF"
        numeric ContactLensPowers "OD/OS power BC DIA, SENSITIF"
        boolean IsTransposed
        varchar OriginalNotationText
        varchar RangeOverrideReason "hanya Sp.M"
        uuid AuthorWorkforceProfileId FK
        varchar AuthorLicenseNumberSnapshot
        varchar RevisionReason
    }
    RegPatientEncounter ||--o{ OptRefractionExam : "1:N — Sudah ada → Baru"
    OptRefractionExam ||--|{ OptRefractionExamRevision : "1:N — Baru"
    OptRefractionExam |o--o{ OptPrescription : "0..1:N — Baru"
    OptPrescription ||--|{ OptPrescriptionVersion : "1:N — Baru"
    RegPatientEncounter ||--o{ OptPrescription : "1:N — Sudah ada → Baru"
```

Kolom ukur diringkas pada diagram. Daftar kolom lengkap ada di
[data-dictionary.md](data-dictionary.md).

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `RegPatientEncounter` | Diperbarui | Registrasi | Dirujuk, **tidak** disalin; tambah `ReferralSourceEncounterId` |
| `MstWorkforceProfile` | Sudah ada | Human Resource | Pemeriksa, penulis resep, perevisi |
| `OptRefractionExam` | Baru | Optik | Root pemeriksaan |
| `OptRefractionExamRevision` | Baru | Optik | Versi hasil ukur (`OPT-DEC-047`) |
| `OptPrescription` | Baru | Optik | Root resep |
| `OptPrescriptionVersion` | Baru | Optik | Versi isi resep |
