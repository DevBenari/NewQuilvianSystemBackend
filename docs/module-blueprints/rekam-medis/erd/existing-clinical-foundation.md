# ERD — Fondasi Klinis Existing Rekam Medis

| Field | Nilai |
| --- | --- |
| Status | `draft` |
| Scope | Entity Clinical Management yang sudah tersedia dan dipakai ulang pada tahap existing-first |
| Owner | Clinical Management; Rekam Medis hanya consumer/reference |
| Snapshot source | Backend `5103e68eec5529540d369673c8a4e2651be0344b` |

Diagram ini menggambarkan relasi penting yang sudah ada. Ia tidak memerintahkan penambahan tabel,
kolom, atau foreign key. Semua entity berstatus `Sudah ada`.

## Assessment dan konsultasi

```mermaid
erDiagram
    TrxPatientAssessment {
        uuid Id PK
        uuid EncounterId FK
        uuid QueueId FK
        uuid PatientId FK
        uuid ServiceUnitId FK
        uuid ClinicId FK "nullable"
        uuid DoctorId FK "nullable"
        int AssessmentStatus "Draft/InProgress/Completed/Cancelled"
        timestamp AssessmentDateTime
    }
    TrxDoctorConsultation {
        uuid Id PK
        uuid EncounterId FK
        uuid QueueId FK
        uuid AssessmentId FK "nullable"
        uuid PatientId FK
        uuid DoctorId FK
        uuid ServiceUnitId FK
        uuid ClinicId FK "nullable"
        int ConsultationStatus "Draft/InProgress/Completed/Cancelled"
        timestamp ConsultationDateTime
    }
    TrxPatientAssessment |o--o{ TrxDoctorConsultation : "assessment opsional — Sudah ada"
```

## Diagnosis, tindakan, dan consent

```mermaid
erDiagram
    TrxDoctorConsultation {
        uuid Id PK
        uuid EncounterId FK
        uuid PatientId FK
        uuid DoctorId FK
        int ConsultationStatus
    }
    TrxPatientDiagnosis {
        uuid Id PK
        uuid EncounterId FK
        uuid ConsultationId FK
        uuid PatientId FK
        uuid DoctorId FK
        uuid DiagnosisId FK "nullable"
        int DiagnosisStatus "Active/Resolved/RuledOut/Cancelled"
        boolean IsPrimary
        timestamp DiagnosisDateTime
    }
    TrxPatientProcedure {
        uuid Id PK
        uuid EncounterId FK
        uuid ConsultationId FK
        uuid PatientId FK
        uuid DoctorId FK
        uuid ProcedureId FK
        int ProcedureStatus "Planned/Ordered/InProgress/Completed/Cancelled"
        timestamp ProcedureDateTime
    }
    TrxPatientConsent {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid ConsultationId FK "nullable"
        uuid PatientProcedureId FK "nullable"
        int ConsentStatus "Draft hingga EnteredInError"
        timestamp ConsentDateTime
        timestamp SignedAt "nullable"
    }
    TrxDoctorConsultation ||--o{ TrxPatientDiagnosis : "1:N — Sudah ada"
    TrxDoctorConsultation ||--o{ TrxPatientProcedure : "1:N — Sudah ada"
    TrxPatientProcedure |o--o{ TrxPatientConsent : "0..1:N — Sudah ada"
```

## Data keselamatan dan catatan lintas profesi

```mermaid
erDiagram
    TrxPatientAllergy {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid ConsultationId FK "nullable"
        uuid AssessmentId FK "nullable"
        int AllergyStatus "Active hingga Cancelled"
        timestamp ReportedDateTime
        boolean IsHighRisk
    }
    TrxPatientVitalSign {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid AssessmentId FK "nullable"
        uuid ConsultationId FK "nullable"
        int VitalSignStatus "Draft hingga EnteredInError"
        timestamp ObservationDateTime
    }
    TrxPatientIntegratedProgressNote {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid ConsultationId FK "nullable"
        uuid AssessmentId FK "nullable"
        uuid VitalSignId FK "nullable"
        timestamp NoteDateTime
        uuid ProviderUserId FK "nullable"
    }
    TrxPatientClinicalDocument {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid ConsultationId FK "nullable"
        uuid PatientDiagnosisId FK "nullable"
        uuid PatientProcedureId FK "nullable"
        int DocumentStatus "Draft hingga EnteredInError"
        varchar FileHash "nullable"
        timestamp DocumentDateTime
    }
    TrxPatientVitalSign |o--o{ TrxPatientIntegratedProgressNote : "0..1:N — Sudah ada"
    TrxPatientClinicalDocument }o--o| TrxPatientDiagnosis : "dokumen diagnosis opsional — Sudah ada"
    TrxPatientClinicalDocument }o--o| TrxPatientProcedure : "dokumen tindakan opsional — Sudah ada"
```

## Status entity dan keputusan reuse

| Entity | Status | Owner | Keputusan existing-first |
| --- | --- | --- | --- |
| `TrxPatientAssessment` | Sudah ada | Clinical Management | `Extend`; status complete bukan signature RM. |
| `TrxDoctorConsultation` | Sudah ada | Clinical Management | `Extend`; finalisasi konsultasi bukan penutupan episode RM. |
| `TrxPatientDiagnosis` | Sudah ada | Clinical Management | `Adapter`; sumber diagnosis tetap entity ini. |
| `TrxPatientProcedure` | Sudah ada | Clinical Management | `Adapter`; tindakan dan billing touchpoint tetap milik owner. |
| `TrxPatientAllergy` | Sudah ada | Clinical Management | `Adapter`; dipakai sebagai data keselamatan. |
| `TrxPatientVitalSign` | Sudah ada | Clinical Management | `Adapter`; nilai klinis tidak disalin ke tabel RM. |
| `TrxPatientIntegratedProgressNote` | Sudah ada | Clinical Management | `Repair/Extend`; belum memiliki lifecycle signature immutable. |
| `TrxPatientClinicalDocument` | Sudah ada | Clinical Management | `Repair/Extend`; update/delete existing tidak cukup untuk finality RM. |
| `TrxPatientConsent` | Sudah ada | Clinical Management | `Adapter`; signature existing belum otomatis menjadi signature evidence RM. |

## Batas interpretasi

- Status `Completed`, `Verified`, `Approved`, atau `Signed` pada provider existing tidak otomatis
  memenuhi signature evidence Rekam Medis yang memerlukan autentikasi ulang dan hash isi.
- Endpoint update/delete existing hanya boleh dipakai sesuai lifecycle owner. Record yang kelak
  dinyatakan final oleh RM harus read-only dan berubah melalui correction/addendum.
- Tidak ada DDL karena revision ini tidak menambah atau memperbarui tabel.

**Contoh:** konsultasi berstatus `Completed` boleh dipakai sebagai bukti layanan selesai, tetapi
episode Rekam Medis tetap `Belum Lengkap` bila diagnosis utama atau catatan wajib belum mempunyai
bukti tanda tangan yang sah.
