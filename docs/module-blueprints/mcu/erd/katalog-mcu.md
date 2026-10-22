# MCU — ERD Katalog MCU

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21 |
| Context | Katalog MCU (`BC-MCU-01`) |
| Keputusan | `MCU-DEC-005`, `007`, `013`, `015`, `020`, `025`, `034` |

```mermaid
erDiagram
    McuPackage {
        uuid Id PK
        varchar PackageCode UK "30, unik selama tidak terhapus"
        varchar PackageName "150"
        uuid GeneralTariffId FK "MstTariff ber-IsPackageTariff"
        boolean IsActive
    }
    McuPackageComponent {
        uuid Id PK
        uuid PackageId FK
        int ComponentType "1 Lab, 2 Rad, 3 Pemeriksaan MCU"
        uuid ProcedureId FK "wajib Lab/Rad"
        uuid RadModalityId FK "wajib Rad"
        uuid ExaminationTypeId FK "wajib Pemeriksaan MCU"
        numeric RefundAmount "18,2"
        numeric MedicalFeeAmount "18,2"
    }
    McuPackageComponentRequirement {
        uuid Id PK
        uuid PackageComponentId FK
        varchar RequirementText "300"
        boolean IsConfirmationRequired
    }
    McuPackageCompanyPrice {
        uuid Id PK
        uuid PackageId FK
        uuid CompanyGuarantorId FK
        uuid TariffId FK "MstTariff ber-IsPackageTariff"
        date EffectiveStartDate
        date EffectiveEndDate "kosong = terbuka"
    }
    McuExaminationType {
        uuid Id PK
        varchar ExaminationTypeCode UK "30"
        varchar ExaminationTypeName "150"
        boolean IsActive
    }
    McuExaminationField {
        uuid Id PK
        uuid ExaminationTypeId FK
        varchar FieldCode "50, unik per jenis"
        int FieldType "1 Teks, 2 Angka, 3 Pilihan, 4 Ya/Tidak"
        int FieldNumber "nomor butir formulir"
        boolean IsRequired
    }
    McuFitnessCategory {
        uuid Id PK
        varchar FitnessCategoryCode UK "30"
        varchar FitnessCategoryName "100"
        boolean IsActive
    }
    MstTariff {
        uuid Id PK
        boolean IsPackageTariff
        numeric NormalPrice
    }
    MstProcedure {
        uuid Id PK
        boolean IsLaboratory
        boolean IsRadiology
    }
    MstCompanyGuarantor {
        uuid Id PK
        date ContractEndDate
    }
    McuPackage ||--|{ McuPackageComponent : "1:N — Baru"
    McuPackageComponent ||--o{ McuPackageComponentRequirement : "1:N — Baru"
    McuPackage ||--o{ McuPackageCompanyPrice : "1:N — Baru"
    McuExaminationType ||--|{ McuExaminationField : "1:N — Baru"
    McuExaminationType |o--o{ McuPackageComponent : "0..1:N — Baru"
    MstProcedure |o--o{ McuPackageComponent : "0..1:N — Sudah ada"
    MstTariff ||--o{ McuPackage : "1:N — Sudah ada"
    MstTariff ||--o{ McuPackageCompanyPrice : "1:N — Sudah ada"
    MstCompanyGuarantor ||--o{ McuPackageCompanyPrice : "1:N — Sudah ada"
```

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `McuPackage` | Baru | MCU | Dinonaktifkan, tidak dihapus |
| `McuPackageComponent` | Baru | MCU | Potretnya disalin ke order |
| `McuPackageComponentRequirement` | Baru | MCU | — |
| `McuPackageCompanyPrice` | Baru | MCU | Tanpa nominal; menunjuk tarif |
| `McuExaminationType`, `McuExaminationField` | Baru | MCU | Isi menunggu `MCU-OQ-002` |
| `McuFitnessCategory` | Baru | MCU | Isi menunggu `MCU-OQ-003` |
| `MstTariff`, `MstProcedure`, `MstCompanyGuarantor`, `MstRadModality` | Sudah ada | Master Data / Radiologi | Dirujuk, **tidak** disalin |
