# ERD — Ownership/Reference Existing

Status: `draft`. Semua entity berstatus `Sudah ada`; tidak ada tabel baru atau diperbarui.

```mermaid
erDiagram
    MstPatient {
        uuid Id PK
        varchar MedicalRecordNumber UK "SENSITIF"
    }
    TrxPatientEncounter {
        uuid Id PK
        uuid PatientId FK
        uuid ServiceUnitId FK
        uuid ClinicId FK "nullable"
        uuid RoomId FK "nullable"
        int EncounterStatus "enum"
    }
    MstServiceUnit {
        uuid Id PK
        varchar ServiceUnitCode UK
        varchar ServiceUnitName
    }
    MstDoctor {
        uuid Id PK
        uuid ProfessionId FK
        uuid SpecializationId FK "nullable"
        varchar FullName "SENSITIF"
    }
    TrxPatientIntegratedProgressNote {
        uuid Id PK
        uuid PatientId FK
        uuid EncounterId FK "nullable"
        uuid DoctorId FK "nullable"
        uuid ProviderUserId FK "nullable"
        uuid ServiceUnitId FK "nullable"
        uuid ClinicId FK "nullable"
        timestamp NoteDateTime
    }
    MstPatient ||--o{ TrxPatientEncounter : "1:N — Existing owner reference"
    MstServiceUnit ||--o{ TrxPatientEncounter : "1:N — Existing owner reference"
    MstPatient ||--o{ TrxPatientIntegratedProgressNote : "1:N — Existing owner reference"
    TrxPatientEncounter |o--o{ TrxPatientIntegratedProgressNote : "0..1:N — Existing"
    MstDoctor |o--o{ TrxPatientIntegratedProgressNote : "0..1:N — Existing"
    MstServiceUnit |o--o{ TrxPatientIntegratedProgressNote : "0..1:N — Existing"
```

## Status Entity

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `MstPatient` | Sudah ada | Patient Management | Direferensikan; tidak disalin. |
| `TrxPatientEncounter` | Sudah ada | Registration Management | Anchor pelayanan; bukan lifecycle RM. |
| `MstServiceUnit` | Sudah ada | Health Services Master Data | Direferensikan. |
| `MstDoctor` | Sudah ada | Workforce/Identity | Identity/profession reference. |
| `TrxPatientIntegratedProgressNote` | Sudah ada | Clinical Management | Read reference; mutation existing tetap conflict. |

Tidak ada DDL karena tidak ada tabel `Baru` atau `Diperbarui`.
