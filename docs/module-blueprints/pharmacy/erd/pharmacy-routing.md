# Farmasi — ERD Routing Depo

```mermaid
erDiagram
    TrxPatientEncounter {
        uuid Id PK
        int EncounterType
        uuid ServiceUnitId FK
        uuid ClinicId FK "nullable"
    }
    MstDrugStorageLocation {
        uuid Id PK
        uuid ServiceUnitId FK "nullable"
        uuid ClinicId FK "nullable"
        varchar StorageLocationType
        boolean IsPharmacyLocation
        boolean IsAllowDispensing
        boolean IsMainWarehouse
        boolean IsQuarantineLocation
        boolean IsDelete
    }
    TrxPatientEncounter }o..o{ MstDrugStorageLocation : "logical match only — Existing"
```

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `TrxPatientEncounter` | Sudah ada | Registration Management | Direferensikan, tidak disalin |
| `MstDrugStorageLocation` | Sudah ada | Health Services Master Data | Direferensikan, tidak diubah |

Relasi adalah pencocokan logis, bukan foreign key baru.

