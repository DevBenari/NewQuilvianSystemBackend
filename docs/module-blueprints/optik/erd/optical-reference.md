# Optik — ERD Profil Barang dan Pengaturan

| Field | Nilai |
| --- | --- |
| Konteks | `BC-OPT-03` Optik — Profil Barang & Pengaturan |
| Revision | `1` — `draft` |
| Keputusan | `OPT-DEC-009`, `031`, `032`, `040`, `041`, `045` |

```mermaid
erDiagram
    MstDrug {
        uuid Id PK
        varchar DrugCode UK
        varchar DrugName
        varchar BrandName "dipakai sebagai merk"
        uuid DrugCategoryId FK
        boolean IsPrescribable "false untuk barang Optik"
        boolean IsStockManaged
    }
    OptItemProfile {
        uuid Id PK
        uuid DrugId FK "UK"
        int ItemCategory
        varchar Color
        varchar FrameSize "contoh 52-18-140"
        varchar FrameMaterial
        numeric LensIndex
        int LensDesign
        varchar LensCoating
        numeric ContactLensBaseCurve
        numeric ContactLensDiameter
        varchar ContactLensReplacement
        boolean IsActive
    }
    MstServiceUnit {
        uuid Id PK
        int ServiceUnitType "Optical = 11"
    }
    OptSetting {
        uuid Id PK
        uuid ServiceUnitId FK "UK"
        numeric DownPaymentMinPercent
        int PickupReminderDays
        int NotCollectedCloseDays
        int PrescriptionValidityMonths
        int WarrantyFitDays
        int WarrantyLensDefectMonths
        int WarrantyFrameDefectMonths
        numeric ContactLensRanges "BC/DIA min-max"
        int Version
    }
    OptSettingRevision {
        uuid Id PK
        uuid SettingId FK
        varchar ChangedFieldsSummary
        uuid ChangedBy
        timestamp ChangedAt
    }
    MstDrug ||--o| OptItemProfile : "1:0..1 — Sudah ada → Baru"
    MstServiceUnit ||--o| OptSetting : "1:0..1 — Diperbarui → Baru"
    OptSetting ||--o{ OptSettingRevision : "1:N — Baru"
```

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `MstDrug` | Sudah ada | Master Data | Tidak diubah; hanya data barang baru |
| `MstServiceUnit` | Diperbarui (enum saja) | Master Data | Nilai `ServiceUnitType.Optical = 11`; tanpa migration |
| `OptItemProfile` | Baru | Optik | Atribut terstruktur (`OPT-DEC-031`); daftar atribut `CONFIGURABLE_DEFAULT` |
| `OptSetting` | Baru | Optik | Angka bawaan |
| `OptSettingRevision` | Baru | Optik | Audit perubahan pengaturan |
