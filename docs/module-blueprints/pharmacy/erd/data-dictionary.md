# Farmasi — Kamus Data Routing Depo

Seluruh entity existing mewarisi `IdentityModel`. Kolom audit tidak diulang. Tidak ada tabel baru atau diperbarui, sehingga tidak ada DDL dokumentasi maupun migration.

## `TrxPatientEncounter` — Sudah ada

Sumber lengkap: `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`.

| Kolom | Tipe | Wajib | Index | Sensitif | Kegunaan routing |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | Identitas encounter |
| `EncounterType` | `EncounterType` | Ya | — | Tidak | Memilih aturan layanan |
| `ServiceUnitId` | `Guid` | Ya | FK/index existing | Tidak | Scope unit |
| `ClinicId` | `Guid?` | Tidak | FK/index existing | Tidak | Prioritas Rawat Jalan |

## `MstDrugStorageLocation` — Sudah ada

Sumber lengkap: `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs` dan `Repositories/Configurations/HealthServices/MstDrugStorageLocationConfiguration.cs`.

| Kolom | Tipe | Wajib | Sensitif | Kegunaan routing |
| --- | --- | :---: | :---: | --- |
| `Id` | `Guid` | Ya | Tidak | Identitas Depo |
| `ServiceUnitId` | `Guid?` | Tidak | Tidak | Scope unit |
| `ClinicId` | `Guid?` | Tidak | Tidak | Scope klinik |
| `StorageLocationType` | `string` | Ya | Tidak | Tipe Emergency/Pharmacy |
| `IsPharmacyLocation` | `bool` | Ya | Tidak | Eligibility |
| `IsAllowDispensing` | `bool` | Ya | Tidak | Eligibility |
| `IsMainWarehouse` | `bool` | Ya | Tidak | Harus false |
| `IsQuarantineLocation` | `bool` | Ya | Tidak | Harus false |
| `IsDelete` | `bool` | Ya | Tidak | Harus false |

