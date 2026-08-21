# Context ERD — Rekam Medis Existing-First

Status: `draft`. Diagram menunjukkan ownership dan arah konsumsi, bukan foreign key atau database
baru.

```mermaid
erDiagram
    PATIENT_MANAGEMENT ||--o{ REGISTRATION : "patient reference"
    REGISTRATION ||--o{ CLINICAL_MANAGEMENT : "encounter dan queue"
    WORKFORCE_IDENTITY ||--o{ CLINICAL_MANAGEMENT : "doctor/provider reference"
    HEALTH_SERVICES_MASTER ||--o{ CLINICAL_MANAGEMENT : "service location"
    CLINICAL_MANAGEMENT ||--o{ PHARMACY_MANAGEMENT : "consultation/prescription"
    CLINICAL_MANAGEMENT ||--o{ MEDICAL_RECORD_EXISTING_VIEW : "clinical fact reference"
    PATIENT_MANAGEMENT ||--o{ MEDICAL_RECORD_EXISTING_VIEW : "patient reference"
    REGISTRATION ||--o{ MEDICAL_RECORD_EXISTING_VIEW : "encounter reference"
    LAB_RADIOLOGY ||--o{ MEDICAL_RECORD_EXISTING_VIEW : "released result reference"
    MEDICAL_RECORD_EXISTING_VIEW }o--o{ FINANCE_CASEMIX : "completeness future; readiness independent"
```

| Context | Status pada revision ini | Batas |
| --- | --- | --- |
| Patient/Registration/Workforce/Master Data | Existing owner | Direferensikan, tidak disalin. |
| Clinical Management | Existing owner | Assessment, SOAP, diagnosis, tindakan, alergi, vital, CPPT, dokumen, consent. |
| Pharmacy | Existing owner | Resep/dispensing; terlibat dalam finalisasi konsultasi existing. |
| Medical Record existing view | Adapter/view | Belum menjadi aggregate episode RM. |
| Lab/Radiology | Existing owner eksternal slice | Hanya hasil released/versioned kelak. |
| Finance/Casemix | Existing owner | Menentukan readiness sendiri; tidak menahan signature/closure RM. |

Detail entity Clinical Management ada di `erd/existing-clinical-foundation.md`. Aggregate baru dari
arsitektur domain penuh belum dimaterialisasi pada revision existing-first.
