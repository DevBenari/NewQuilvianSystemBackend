# Kamus Data — Modul Kiosk

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` r1 |
| Status | `approved` — Sukma Giri Pratama, 30 Sep 2026 |
| `input_revision` | `02-backend-architecture.md` r1 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

> **Revisi Kiosk tidak menambah atau mengubah tabel apa pun.** Semua tabel di bawah berstatus `Sudah ada`, sehingga hanya kolom yang dipakai aturan bisnis Kiosk yang dicatat, ditambah rujukan ke file model. Karena itu **tidak ada bagian DDL**: DDL hanya ditulis untuk tabel `Baru` atau `Diperbarui`.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `MstPatient` | Sudah ada | PatientManagement | Dibaca Cek No. RM; **MUST NOT** disalin atau diubah |
| `MstPatientIdentityDocument` | Sudah ada | PatientManagement | Dibaca untuk pencocokan KTP cadangan |
| `MstPatientInsurance` | Sudah ada | PatientManagement | Dibaca: kartu asuransi (Step 1), kandidat penjamin |
| `MstPatientMembership` | Sudah ada | PatientManagement | Dibaca: nomor member (Step 1) |
| `MstPatientCompanyGuarantor` | Sudah ada | PatientManagement | Dibaca: kandidat penjamin perusahaan |
| `RegPatientEncounter` | Sudah ada | RegistrationManagement | Dibuat lewat route kiosk existing |
| `RegPatientEncounterGuarantor` | Sudah ada | RegistrationManagement | Satu per kunjungan; kini boleh berisi perusahaan dari Kiosk |
| `TrxKioskScanSession` | Sudah ada (legacy `Trx*`) | RegistrationManagement | Dibuat sekali di Step 3 |

## 2. Kolom yang dipakai aturan Kiosk

### `MstPatient` — `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs`

| Kolom | Tipe | Wajib | Index | Sensitif | Dipakai untuk |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | `patientId` di respons lookup |
| `MedicalRecordNumber` | `string(50)` | Ya | Unique | Tidak | Kartu Pasien |
| `PatientCode` | `string(50)` | Ya | Unique | Tidak | Kartu Pasien |
| `FullName` | `string(200)` | Ya | Index | **Ya** | Kartu Pasien (tidak masuk log) |
| `IdentityNumber` | `string(50)` | Tidak | Unique, filter `IdentityNumber IS NOT NULL AND IsDelete = false` | **Ya** | Pencocokan KTP |
| `PhoneNumber` | `string(30)` | Tidak | Index (kolom mentah) | **Ya** | Pencocokan HP setelah dinormalkan di query |
| `PatientStatus` | `PatientStatus` (int) | Ya | — | Tidak | Hanya `Active (1)` yang dianggap ditemukan |
| `IsActive` | `bool` | Ya | — | Tidak | Harus `true` |
| `IsDeceased` | `bool` | Ya | — | **Ya** | Harus `false`; **tidak pernah** disebut di layar |
| `MergedToPatientId` | `Guid?` | Tidak | FK ke `MstPatient`, `Restrict` | Tidak | Diikuti ke pasien tujuan (maks. 3 langkah) |
| `PatientType`, `Gender`, `BloodType` | enum | — | — | `BloodType`: **Ya** | Label Kartu Pasien |

### `MstPatientIdentityDocument` — `.../MasterData/Models/MstPatientIdentityDocument.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `IdentityNumber` (**Sensitif**), `IsActive` | Pencocokan KTP cadangan, sama dengan `FindPatientAsync` |

### `MstPatientInsurance` — `.../MasterData/Models/MstPatientInsurance.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `CardNumber` (**Sensitif**), `IsActive` | `searchType = 3` di Step 1; daftar kandidat penjamin |

### `MstPatientMembership` — `.../MasterData/Models/MstPatientMembership.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `MemberNumber` (**Sensitif**), `IsActive` | `searchType = 4` di Step 1 |

### `RegPatientEncounterGuarantor` — `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `EncounterId` (FK), `PaymentType`, `PatientInsuranceId`, `PatientCompanyGuarantorId`, `IsPrimary` | Satu penanggung per kunjungan (`KSK-FACT-004`). Diisi `CreateEncounterCoreAsync` existing |

### `TrxKioskScanSession` — `Areas/HealthServices/RegistrationManagement/Models/TrxKioskScanSession.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `Id`, `PatientId`, `TargetService` (null untuk poliklinik, `2` untuk Laboratorium), `HasPhysicianRequest`, `Status` | Dibentuk sekali di Step 3; ditempel ke kunjungan poliklinik lewat `KioskScanSessionId` |

## 3. Data yang tidak disimpan

| Data | Letak hidupnya | Dibuang kapan |
| --- | --- | --- |
| Nilai KTP/HP yang diketik di Cek No. RM | Memori layar + body request | Saat pindah halaman, `SESSION_CLEARED`, atau hasil baru |
| `patientId` untuk handoff Cek No. RM → Pasien Lama | Redux in-memory (tanpa persistence) | Begitu dibaca halaman Pasien Lama, atau saat `SESSION_CLEARED` |
| Hasil pindai kartu Step 1 | Memori layar | Setelah dikirim ke `scan-result` di Step 3, atau `SESSION_CLEARED` |
