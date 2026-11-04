# Optik — Kamus Data

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Revision | `1` — `draft` |
| Sumber | `02-backend-architecture.md` (`OPT-BE-ARCH` r1) |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Dokumen klinis dan
pesanan Optik **tidak** memakai `IsDelete`; pembatalan memakai status.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk custom logger, tidak boleh dipakai sebagai
contoh berisi data asli, dan perlu ditinjau masking-nya pada response DTO. Seluruh enum disimpan
sebagai `integer` (`HasConversion<int>()`). Seluruh waktu disimpan UTC; perbandingan tanggal
bisnis (masa berlaku resep, izin, garansi) memakai tanggal kalender WIB.

## Daftar tabel

| Tabel | Status | Konteks | Lokasi model |
| --- | --- | --- | --- |
| `OptRefractionExam` | Baru | Klinis | `Areas/HealthServices/OpticalManagement/Models/OptRefractionExam.cs` |
| `OptRefractionExamRevision` | Baru | Klinis | `Areas/HealthServices/OpticalManagement/Models/OptRefractionExamRevision.cs` |
| `OptPrescription` | Baru | Klinis | `Areas/HealthServices/OpticalManagement/Models/OptPrescription.cs` |
| `OptPrescriptionVersion` | Baru | Klinis | `Areas/HealthServices/OpticalManagement/Models/OptPrescriptionVersion.cs` |
| `OptOrder` | Baru | Pesanan | `Areas/HealthServices/OpticalManagement/Models/OptOrder.cs` |
| `OptOrderItem` | Baru | Pesanan | `Areas/HealthServices/OpticalManagement/Models/OptOrderItem.cs` |
| `OptOrderCancellation` | Baru | Pesanan | `Areas/HealthServices/OpticalManagement/Models/OptOrderCancellation.cs` |
| `OptOrderStatusHistory` | Baru | Pesanan | `Areas/HealthServices/OpticalManagement/Models/OptOrderStatusHistory.cs` |
| `OptWarrantyClaim` | Baru | Pesanan | `Areas/HealthServices/OpticalManagement/Models/OptWarrantyClaim.cs` |
| `OptItemProfile` | Baru | Profil & Pengaturan | `Areas/HealthServices/OpticalManagement/Models/OptItemProfile.cs` |
| `OptSetting` | Baru | Profil & Pengaturan | `Areas/HealthServices/OpticalManagement/Models/OptSetting.cs` |
| `OptSettingRevision` | Baru | Profil & Pengaturan | `Areas/HealthServices/OpticalManagement/Models/OptSettingRevision.cs` |
| `RegPatientEncounter` | Diperbarui | Registrasi | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` |
| `MstPatient` | Sudah ada | Patient Management | dirujuk `PatientId` |
| `MstWorkforceProfile` | Sudah ada | Human Resource | dirujuk pemeriksa/penulis |
| `WfpCredentialLicense` | Sudah ada | Human Resource | `Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpCredentialLicense.cs` |
| `MstDrug` | Sudah ada | Master Data | `Areas/HealthServices/MasterData/Models/MstDrug.cs` |
| `MstTariff` | Sudah ada | Master Data | `Areas/HealthServices/MasterData/Models/MstTariff.cs` |
| `MstSupplier` | Sudah ada | Administrator Master Data | `Areas/Administrator/MasterData/Models/MstSupplier.cs` |
| `MstServiceUnit` | Sudah ada (enum diperbarui) | Master Data | `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` |
| `MstDrugStorageLocation` | Sudah ada | Master Data / Farmasi | `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs` |


## `OptRefractionExam` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptRefractionExam.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptRefractionExamConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ExamNumber` | `string(30)` | Ya | — | UK | — | — | Tidak | Nomor pemeriksaan dari `OptNumberService`, contoh `OPT-EXM-RSMMC-000001` |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `RegPatientEncounter` | Restrict | Tidak | Kunjungan Optik; wajib belum ditutup saat mulai |
| `PatientId` | `Guid` | Ya | — | Index | FK ke `MstPatient` | Restrict | Tidak | Disalin dari kunjungan; dipakai pencarian riwayat |
| `ExaminerWorkforceProfileId` | `Guid` | Ya | — | Index | FK ke `MstWorkforceProfile` | Restrict | Tidak | Pemeriksa RO/Sp.M (`OPT-DEC-005`) |
| `ExaminerLicenseNumberSnapshot` | `string(100)` | Ya | — | — | — | — | Tidak | Nomor SIP yang berlaku saat pemeriksaan (`OPT-DEC-012`) |
| `Status` | `OptRefractionExamStatus` | Ya | `Draft (1)` | Index bersama `EncounterId` | — | — | Tidak | Draft/Final/Cancelled |
| `CurrentRevisionNo` | `int` | Ya | `1` | — | — | — | Tidak | Nomor revisi hasil ukur yang berlaku |
| `StartedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu mulai |
| `FinalizedAt` | `DateTime?` | Tidak | — | Index bersama `PatientId` | — | — | Tidak | Waktu disimpan final |
| `FinalizedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Akun yang menyimpan final |
| `BillingFactEmittedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Terisi sekali saat fakta tarif pemeriksaan diterima Billing; mencegah tagih ulang (`OPT-DEC-042`, `047`) |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila `Cancelled` |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Penanda concurrency (`IsConcurrencyToken`); naik setiap perubahan |

## `OptRefractionExamRevision` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptRefractionExamRevision.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptRefractionExamRevisionConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RefractionExamId` | `Guid` | Ya | — | UK bersama `RevisionNo` | FK ke `OptRefractionExam` | Restrict | Tidak | Induk pemeriksaan |
| `RevisionNo` | `int` | Ya | — | UK bersama `RefractionExamId` | — | — | Tidak | 1, 2, 3 … |
| `VisusDistanceUncorrectedOd` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus jauh tanpa koreksi, mata kanan; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusDistanceUncorrectedOs` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus jauh tanpa koreksi, mata kiri; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusDistanceCorrectedOd` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus jauh dengan koreksi, mata kanan; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusDistanceCorrectedOs` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus jauh dengan koreksi, mata kiri; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusNearUncorrectedOd` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus dekat tanpa koreksi, mata kanan; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusNearUncorrectedOs` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus dekat tanpa koreksi, mata kiri; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusNearCorrectedOd` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus dekat dengan koreksi, mata kanan; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `VisusNearCorrectedOs` | `string(10)?` | Tidak | — | — | — | — | **Ya** | Visus dekat dengan koreksi, mata kiri; notasi Snellen, contoh `6/12` (`PROPOSED` RCG `n1`) |
| `AutorefOdSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer SPH mata kanan |
| `AutorefOdCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer CYL, notasi minus |
| `AutorefOdAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer AXIS 0..180 |
| `AutorefOsSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer SPH mata kiri |
| `AutorefOsCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer CYL, notasi minus |
| `AutorefOsAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Autorefraktometer AXIS 0..180 |
| `SubjectiveOdSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif SPH mata kanan |
| `SubjectiveOdCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif CYL, notasi minus |
| `SubjectiveOdAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif AXIS |
| `SubjectiveOdAdd` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | Tambahan baca (ADD) |
| `SubjectiveOsSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif SPH mata kiri |
| `SubjectiveOsCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif CYL, notasi minus |
| `SubjectiveOsAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Koreksi subjektif AXIS |
| `SubjectiveOsAdd` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | Tambahan baca (ADD) |
| `PdDistance` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | Jarak pupil jauh (mm) |
| `PdNear` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | Jarak pupil dekat (mm) |
| `KeratometryNote` | `string(200)?` | Tidak | — | — | — | — | **Ya** | Keratometri, opsional |
| `ClinicalNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan klinis pemeriksa |
| `RevisedByWorkforceProfileId` | `Guid` | Ya | — | — | FK ke `MstWorkforceProfile` | Restrict | Tidak | Pembuat revisi ini |
| `RevisedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu revisi |
| `RevisionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib untuk revisi sesudah `Final` (`OPT-DEC-047`) |

## `OptPrescription` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptPrescription.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptPrescriptionConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PrescriptionNumber` | `string(30)` | Ya | — | UK | — | — | Tidak | Contoh `OPT-RX-RSMMC-000001` |
| `PatientId` | `Guid` | Ya | — | Index bersama `ValidUntil` | FK ke `MstPatient` | Restrict | Tidak | Pemilik resep |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `RegPatientEncounter` | Restrict | Tidak | Kunjungan saat dibuat/disalin |
| `RefractionExamId` | `Guid?` | Tidak | — | Index | FK ke `OptRefractionExam` | Restrict | Tidak | Kosong untuk resep luar |
| `PrescriptionType` | `OptPrescriptionType` | Ya | — | — | — | — | Tidak | Spectacle/ContactLens |
| `Origin` | `OptPrescriptionOrigin` | Ya | — | — | — | — | Tidak | Internal/External |
| `ExternalAuthorName` | `string(150)?` | Tidak | — | — | — | — | Tidak | Wajib bila External (`OPT-DEC-017`) |
| `ExternalFacilityName` | `string(150)?` | Tidak | — | — | — | — | Tidak | Wajib bila External |
| `IssuedDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal terbit resep (resep luar: tanggal di kertas resep) |
| `ValidityMonthsSnapshot` | `int` | Ya | dari `OptSetting` | — | — | — | Tidak | Usia resep yang dibekukan (`OPT-DEC-023`, `040`) |
| `ValidUntil` | `DateOnly` | Ya | `IssuedDate` + snapshot | Index bersama `PatientId` | — | — | Tidak | Batas berlaku, dibandingkan dengan tanggal WIB |
| `CurrentVersionNo` | `int` | Ya | `1` | — | — | — | Tidak | Versi terakhir |
| `LockedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Terisi saat pesanan yang memakai resep masuk pengerjaan (`OPT-DEC-007`) |
| `LockedByOrderId` | `Guid?` | Tidak | — | — | rujukan logis ke `OptOrder` (tanpa FK fisik agar tak berputar) | — | Tidak | Pesanan pertama yang mengunci |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Penanda concurrency (`IsConcurrencyToken`); naik setiap perubahan |

## `OptPrescriptionVersion` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptPrescriptionVersion.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptPrescriptionVersionConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PrescriptionId` | `Guid` | Ya | — | UK bersama `VersionNo` | FK ke `OptPrescription` | Restrict | Tidak | Induk resep |
| `VersionNo` | `int` | Ya | — | UK bersama `PrescriptionId` | — | — | Tidak | 1, 2, 3 … |
| `OdSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Kacamata SPH mata kanan; −20,00..+20,00 kelipatan 0,25 |
| `OdCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Kacamata CYL; −6,00..0,00 kelipatan 0,25 (notasi minus) |
| `OdAxis` | `int?` | Tidak | — | — | — | — | **Ya** | 0..180; wajib bila CYL ≠ 0, kosong bila CYL = 0 |
| `OdAdd` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | +0,75..+3,50 kelipatan 0,25 |
| `OsSph` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Kacamata SPH mata kiri; −20,00..+20,00 kelipatan 0,25 |
| `OsCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Kacamata CYL; −6,00..0,00 kelipatan 0,25 (notasi minus) |
| `OsAxis` | `int?` | Tidak | — | — | — | — | **Ya** | 0..180; wajib bila CYL ≠ 0, kosong bila CYL = 0 |
| `OsAdd` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | +0,75..+3,50 kelipatan 0,25 |
| `PdDistance` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | 50..80 mm |
| `PdNear` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | 50..80 mm |
| `OdClPower` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Lensa kontak power mata kanan; kosmetik tanpa ukuran = 0 |
| `OdClCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Lensa kontak torik CYL, opsional |
| `OdClAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Lensa kontak torik AXIS, wajib bila CYL ≠ 0 |
| `OdClBaseCurve` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | BC, rentang dari `OptSetting` |
| `OdClDiameter` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | DIA, rentang dari `OptSetting` |
| `OsClPower` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Lensa kontak power mata kiri; kosmetik tanpa ukuran = 0 |
| `OsClCyl` | `decimal(5,2)?` | Tidak | — | — | — | — | **Ya** | Lensa kontak torik CYL, opsional |
| `OsClAxis` | `int?` | Tidak | — | — | — | — | **Ya** | Lensa kontak torik AXIS, wajib bila CYL ≠ 0 |
| `OsClBaseCurve` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | BC, rentang dari `OptSetting` |
| `OsClDiameter` | `decimal(4,2)?` | Tidak | — | — | — | — | **Ya** | DIA, rentang dari `OptSetting` |
| `ContactLensBrandOrType` | `string(150)?` | Tidak | — | — | — | — | Tidak | Wajib untuk resep lensa kontak (`OPT-DEC-022`) |
| `ContactLensReplacement` | `string(50)?` | Tidak | — | — | — | — | Tidak | Harian/bulanan/… |
| `IsTransposed` | `bool` | Ya | `false` | — | — | — | Tidak | Benar bila resep luar plus diubah ke notasi minus (`OPT-DEC-045`) |
| `OriginalNotationText` | `string(200)?` | Tidak | — | — | — | — | **Ya** | Teks asli resep sebelum transposisi, wajib bila `IsTransposed` |
| `RangeOverrideReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan Sp.M melewati rentang; kosong untuk RO |
| `AuthorWorkforceProfileId` | `Guid` | Ya | — | Index | FK ke `MstWorkforceProfile` | Restrict | Tidak | Penulis/penyalin/perevisi versi ini |
| `AuthorLicenseNumberSnapshot` | `string(100)` | Ya | — | — | — | — | Tidak | SIP yang berlaku saat menulis |
| `AuthoredAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu versi dibuat |
| `RevisionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib untuk versi ≥ 2 |

## `OptOrder` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptOrder.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptOrderConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderNumber` | `string(30)` | Ya | — | UK | — | — | Tidak | Contoh `OPT-ORD-RSMMC-000001` |
| `EncounterId` | `Guid` | Ya | — | Index bersama `Status` | FK ke `RegPatientEncounter` | Restrict | Tidak | Kunjungan pemesanan; tagihan menempel di sini (`OPT-DEC-043`) |
| `PatientId` | `Guid` | Ya | — | Index | FK ke `MstPatient` | Restrict | Tidak | Pemilik pesanan |
| `OrderKind` | `OptOrderKind` | Ya | — | — | — | — | Tidak | Fabrication/DirectSale/Remake |
| `PrescriptionId` | `Guid?` | Tidak | — | Index | FK ke `OptPrescription` | Restrict | Tidak | Wajib untuk pesanan berukuran dan lensa kontak |
| `PrescriptionVersionId` | `Guid?` | Tidak | — | — | FK ke `OptPrescriptionVersion` | Restrict | Tidak | Versi yang dikerjakan; tidak berubah setelah `InFabrication` |
| `Status` | `OptOrderStatus` | Ya | `Draft (1)` | Index bersama `EncounterId`; index bersama `ReadyAt` | — | — | Tidak | Bagian 3 `contracts/state-transition-matrix.md` |
| `DownPaymentMinPercentSnapshot` | `decimal(5,2)` | Ya | dari `OptSetting` | — | — | — | Tidak | Dibekukan saat dibuat (`OPT-DEC-009`) |
| `WorkshopType` | `OptWorkshopType?` | Tidak | — | — | — | — | Tidak | Wajib saat kirim ke pengerjaan |
| `ExternalLabSupplierId` | `Guid?` | Tidak | — | Index | FK ke `MstSupplier` | Restrict | Tidak | Wajib bila `ExternalLab` |
| `EstimatedReadyDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Perkiraan jadi (`OPT-DEC-013`) |
| `BilledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat fakta tagihan item diterbitkan |
| `SentToWorkAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat `InFabrication` |
| `ReadyAt` | `DateTime?` | Tidak | — | Index bersama `Status` | — | — | Tidak | Dasar daftar "perlu dihubungi" (`OPT-DEC-018`) |
| `HandedOverAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Awal masa garansi |
| `HandedOverBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas yang menyerahkan |
| `HandoverIdentityVerified` | `bool` | Ya | `false` | — | — | — | Tidak | Wajib `true` untuk menyerahkan (`OPT-DEC-048`) |
| `HandoverReceiverName` | `string(150)?` | Tidak | — | — | — | — | **Ya** | Diisi bila diambil wakil |
| `HandoverReceiverRelationship` | `string(50)?` | Tidak | — | — | — | — | Tidak | Hubungan wakil dengan pasien |
| `FittingNote` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan penyetelan |
| `WarrantyFitDaysSnapshot` | `int?` | Tidak | — | — | — | — | Tidak | Dibekukan saat `HandedOver` (`OPT-DEC-041`) |
| `WarrantyLensDefectMonthsSnapshot` | `int?` | Tidak | — | — | — | — | Tidak | Dibekukan saat `HandedOver` |
| `WarrantyFrameDefectMonthsSnapshot` | `int?` | Tidak | — | — | — | — | Tidak | Dibekukan saat `HandedOver` |
| `NotCollectedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat ditutup `NotCollected` |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat `Cancelled` |
| `IsRemake` | `bool` | Ya | `false` | — | — | — | Tidak | Pesanan remake Rp 0 (`OPT-DEC-015`) |
| `ParentOrderId` | `Guid?` | Tidak | — | Index | FK ke `OptOrder` | Restrict | Tidak | Pesanan asal, wajib untuk remake |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Penanda concurrency (`IsConcurrencyToken`); naik setiap perubahan |

## `OptOrderItem` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptOrderItem.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptOrderItemConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderId` | `Guid` | Ya | — | UK bersama `LineNo` | FK ke `OptOrder` | Restrict | Tidak | Induk pesanan |
| `LineNo` | `int` | Ya | — | UK bersama `OrderId` | — | — | Tidak | Nomor baris 1, 2, 3 … |
| `DrugId` | `Guid` | Ya | — | Index | FK ke `MstDrug` | Restrict | Tidak | Barang; wajib punya `OptItemProfile` aktif |
| `ItemCategory` | `OptItemCategory` | Ya | dari `OptItemProfile` | — | — | — | Tidak | Disalin saat baris dibuat |
| `LensSupply` | `OptLensSupply?` | Tidak | — | — | — | — | Tidak | Wajib untuk lensa kacamata: Stock/Custom |
| `Quantity` | `decimal(10,3)` | Ya | — | — | — | — | Tidak | > 0 |
| `Status` | `OptOrderItemStatus` | Ya | `Active (1)` | — | — | — | Tidak | Active/Cancelled |
| `IsLensCut` | `bool?` | Tidak | — | — | — | — | Tidak | Diisi saat pengajuan batal sesudah pengerjaan (`OPT-DEC-038`) |
| `StockIssuedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat stok dipotong (`OPT-DEC-019`) |
| `StockReturnedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat stok dikembalikan |
| `BillingFactVersion` | `int` | Ya | `0` | — | — | — | Tidak | Versi fakta tagihan terakhir yang dikirim; 0 = belum (remake selalu 0) |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat baris dibatalkan |

## `OptOrderCancellation` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptOrderCancellation.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptOrderCancellationConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderId` | `Guid` | Ya | — | Index; UK terfilter `Status = 1` | FK ke `OptOrder` | Restrict | Tidak | Pesanan yang diajukan batal |
| `Status` | `OptCancellationStatus` | Ya | `Requested (1)` | — | — | — | Tidak | Requested/Approved/Rejected |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan wajib |
| `RequestedBy` | `Guid` | Ya | — | — | — | — | Tidak | Pengaju |
| `RequestedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu pengajuan |
| `DecidedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Kepala Optik |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `DecisionNote` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila ditolak |

## `OptOrderStatusHistory` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptOrderStatusHistory.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptOrderStatusHistoryConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderId` | `Guid` | Ya | — | Index bersama `OccurredAt` | FK ke `OptOrder` | Restrict | Tidak | Pesanan |
| `FromStatus` | `OptOrderStatus?` | Tidak | — | — | — | — | Tidak | Kosong untuk baris pertama |
| `ToStatus` | `OptOrderStatus` | Ya | — | — | — | — | Tidak | Status tujuan |
| `ActorUserId` | `Guid` | Ya | — | — | — | — | Tidak | Pelaku |
| `OccurredAt` | `DateTime` | Ya | waktu server | Index bersama `OrderId` | — | — | Tidak | Waktu kejadian |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib untuk batal dan tidak diambil |

## `OptWarrantyClaim` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptWarrantyClaim.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptWarrantyClaimConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ClaimNumber` | `string(30)` | Ya | — | UK | — | — | Tidak | Contoh `OPT-CLM-RSMMC-000001` |
| `OrderId` | `Guid` | Ya | — | Index | FK ke `OptOrder` | Restrict | Tidak | Pesanan asal; wajib `HandedOver` |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `RegPatientEncounter` | Restrict | Tidak | Kunjungan keluhan baru (`OPT-DEC-043`) |
| `ComplaintType` | `OptComplaintType` | Ya | — | — | — | — | Tidak | FitDiscomfort/LensDefect/FrameDefect/Damage |
| `ComplaintNote` | `string(1000)` | Ya | — | — | — | — | **Ya** | Keluhan pasien |
| `IsWithinWarranty` | `bool` | Ya | dihitung | — | — | — | Tidak | Dihitung saat dicatat dari snapshot garansi pesanan; `Damage` selalu `false` |
| `WarrantyDeadlineSnapshot` | `DateOnly?` | Tidak | dihitung | — | — | — | Tidak | Batas garansi untuk jenis keluhan ini |
| `Status` | `OptWarrantyClaimStatus` | Ya | `Submitted (1)` | — | — | — | Tidak | Submitted/Examined/Approved/Rejected |
| `ExaminedByWorkforceProfileId` | `Guid?` | Tidak | — | — | FK ke `MstWorkforceProfile` | Restrict | Tidak | RO/Sp.M pemeriksa ulang |
| `ExaminedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemeriksaan ulang |
| `ExaminationFinding` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Temuan pemeriksaan ulang |
| `ReexamRefractionExamId` | `Guid?` | Tidak | — | — | FK ke `OptRefractionExam` | Restrict | Tidak | Pemeriksaan refraksi baru, bila dilakukan |
| `DecidedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Kepala Optik |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `DecisionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila ditolak |
| `RemakeOrderId` | `Guid?` | Tidak | — | UK terfilter (tidak kosong) | FK ke `OptOrder` | Restrict | Tidak | Pesanan remake yang lahir; paling banyak satu |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Penanda concurrency (`IsConcurrencyToken`); naik setiap perubahan |

## `OptItemProfile` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptItemProfile.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptItemProfileConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DrugId` | `Guid` | Ya | — | UK | FK ke `MstDrug` | Restrict | Tidak | Barang; satu profil per barang |
| `ItemCategory` | `OptItemCategory` | Ya | — | Index | — | — | Tidak | Frame/SpectacleLens/ContactLens/Accessory/ReadyMadeGlasses |
| `Color` | `string(50)?` | Tidak | — | — | — | — | Tidak | Frame |
| `FrameSize` | `string(20)?` | Tidak | — | — | — | — | Tidak | Frame, contoh `52-18-140` |
| `FrameMaterial` | `string(50)?` | Tidak | — | — | — | — | Tidak | Frame |
| `LensIndex` | `decimal(4,2)?` | Tidak | — | — | — | — | Tidak | Lensa, contoh 1,56 |
| `LensDesign` | `OptLensDesign?` | Tidak | — | — | — | — | Tidak | Lensa |
| `LensCoating` | `string(100)?` | Tidak | — | — | — | — | Tidak | Lensa, contoh antiradiasi |
| `ContactLensBaseCurve` | `decimal(4,2)?` | Tidak | — | — | — | — | Tidak | Lensa kontak |
| `ContactLensDiameter` | `decimal(4,2)?` | Tidak | — | — | — | — | Tidak | Lensa kontak |
| `ContactLensReplacement` | `string(50)?` | Tidak | — | — | — | — | Tidak | Lensa kontak, contoh bulanan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Profil nonaktif tidak bisa dipilih di pesanan baru |

## `OptSetting` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptSetting.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptSettingConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ServiceUnitId` | `Guid` | Ya | — | UK | FK ke `MstServiceUnit` | Restrict | Tidak | Unit Optik |
| `DownPaymentMinPercent` | `decimal(5,2)` | Ya | `50` | — | — | — | Tidak | 0..100 (`OPT-DEC-009`) |
| `PickupReminderDays` | `int` | Ya | `30` | — | — | — | Tidak | 1..365 (`OPT-DEC-040`) |
| `NotCollectedCloseDays` | `int` | Ya | `90` | — | — | — | Tidak | > `PickupReminderDays` |
| `PrescriptionValidityMonths` | `int` | Ya | `12` | — | — | — | Tidak | 1..60 |
| `WarrantyFitDays` | `int` | Ya | `30` | — | — | — | Tidak | Garansi ukuran (`OPT-DEC-041`) |
| `WarrantyLensDefectMonths` | `int` | Ya | `6` | — | — | — | Tidak | Garansi cacat lensa |
| `WarrantyFrameDefectMonths` | `int` | Ya | `6` | — | — | — | Tidak | Garansi cacat frame |
| `ContactLensBaseCurveMin` | `decimal(4,2)` | Ya | `7.50` | — | — | — | Tidak | `PROPOSED`, dikonfirmasi RO/Sp.M |
| `ContactLensBaseCurveMax` | `decimal(4,2)` | Ya | `9.50` | — | — | — | Tidak | `PROPOSED` |
| `ContactLensDiameterMin` | `decimal(4,2)` | Ya | `13.00` | — | — | — | Tidak | `PROPOSED` |
| `ContactLensDiameterMax` | `decimal(4,2)` | Ya | `15.00` | — | — | — | Tidak | `PROPOSED` |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Penanda concurrency (`IsConcurrencyToken`); naik setiap perubahan |

## `OptSettingRevision` — Baru

Lokasi model: `Areas/HealthServices/OpticalManagement/Models/OptSettingRevision.cs`. Configuration: `Repositories/Configurations/HealthServices/OpticalManagement/OptSettingRevisionConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SettingId` | `Guid` | Ya | — | Index bersama `ChangedAt` | FK ke `OptSetting` | Restrict | Tidak | Pengaturan yang berubah |
| `ChangedFieldsSummary` | `string(2000)` | Ya | — | — | — | — | Tidak | Nilai lama → baru per kolom, contoh `PickupReminderDays: 30 → 21` |
| `ChangedBy` | `Guid` | Ya | — | — | — | — | Tidak | Admin Optik |
| `ChangedAt` | `DateTime` | Ya | waktu server | Index bersama `SettingId` | — | — | Tidak | Waktu perubahan |

## `RegPatientEncounter` — Diperbarui

Lokasi model: `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`. Satu kolom baru (baris terakhir); seluruh kolom lain sudah ada dan tidak berubah. Panjang dan wajib dibaca dari atribut model; tipe database dan index lengkap mengikuti configuration Registrasi yang ada.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada, tidak berubah |
| `EncounterNumber` | `string(50)` | Ya | `string.Empty` | UK | — | — | Tidak | Sudah ada, tidak berubah |
| `PatientId` | `Guid` | Ya | — | — | FK ke `MstPatient` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `ServiceUnitId` | `Guid` | Ya | — | — | FK ke `MstServiceUnit` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `ClinicId` | `Guid?` | Tidak | — | — | FK ke `MstClinic` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `RoomId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `DoctorId` | `Guid?` | Tidak | — | — | FK ke `MstDoctor` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `DoctorScheduleId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `DoctorServiceRuleId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `PatientClassId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `KioskScanSessionId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeCategoryId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeYearAtEncounter` | `int?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeMonthAtEncounter` | `int?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeDayAtEncounter` | `int?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `TotalAgeDaysAtEncounter` | `int?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeTextAtEncounter` | `string?(100)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeCategoryCodeSnapshot` | `string?(50)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeCategoryNameSnapshot` | `string?(150)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeReferenceDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AgeCalculatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `EncounterDate` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `EncounterType` | `EncounterType` | Ya | `EncounterType.Outpatient` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `VisitType` | `VisitType` | Ya | `VisitType.NewVisit` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `RegistrationSource` | `EncounterRegistrationSource` | Ya | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `EncounterStatus` | `EncounterStatus` | Ya | `EncounterStatus.Registered` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `ChiefComplaint` | `string?(500)` | Tidak | — | — | — | — | **Ya** | Sudah ada, tidak berubah |
| `PaymentType` | `EncounterPaymentType` | Ya | `EncounterPaymentType.Cash` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `PaymentMethodId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsReferral` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `ReferralNumber` | `string?(250)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsReferralRequired` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsReferralVerified` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `ReferralInstitutionId` | `Guid?` | Tidak | — | — | FK ke `MstReferralInstitution` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `ReferralDoctorId` | `Guid?` | Tidak | — | — | FK ke `MstReferralDoctor` | lihat configuration | Tidak | Sudah ada, tidak berubah |
| `IsNewPatient` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsFromKiosk` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsWalkIn` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsAppointment` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsScreeningRequired` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsQueueRequired` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `IsDoctorRequired` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `RegisteredAt` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `RegisteredByUserId` | `Guid` | Ya | `Guid.Empty` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `CheckedInAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `NoShowAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `NoShowByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `NoShowReason` | `string?(250)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `CancelledByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `CancelReason` | `string?(250)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `Notes` | `string?(500)` | Tidak | — | — | — | — | **Ya** | Sudah ada, tidak berubah |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada, tidak berubah |
| `RegistrationIdempotencyKey` | `string?(100)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `SourceEncounterId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AdmissionTransferId` | `Guid?` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `AdmissionTransferSourceType` | `string?(20)` | Tidak | — | — | — | — | Tidak | Sudah ada, tidak berubah |
| `ReferralSourceEncounterId` | `Guid?` | Tidak | — | Index | FK ke `RegPatientEncounter` | Restrict | Tidak | **Baru.** Kunjungan Poli Mata asal rujukan (`OPT-DEC-037`). Bukan `SourceEncounterId` (transfer) |

## Tabel `Sudah ada` — kolom kunci yang dipakai Optik

| Tabel | Kolom | Dipakai untuk |
| --- | --- | --- |
| `MstPatient` | `Id` | Rujukan pasien |
| `MstWorkforceProfile` | `Id`, `DisplayName`, profesi | Pemeriksa dan penulis resep |
| `ApplicationUser` | `WorkforceProfileId` | Menemukan pegawai dari akun yang masuk |
| `WfpCredentialLicense` | `WorkforceProfileId`, `LicenseType`, `LicenseNumber`, `ExpiredDate`, `IsVerified`, `IsRevoked`, `IsActive` | Gerbang izin praktik (`OPT-DEC-012`) |
| `MstDrug` | `Id`, `DrugCode`, `DrugName`, `BrandName`, `DrugCategoryId`, `IsPrescribable`, `IsStockManaged` | Barang Optik |
| `MstTariff` | `Id`, `DrugId`, `ProcedureId`, `ClinicId`, `PatientClassId`, `NormalPrice`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | Harga (dibaca Billing) |
| `MstSupplier` | `Id`, nama | Lab lensa luar |
| `MstServiceUnit` | `Id`, `ServiceUnitType` | Unit Optik |
| `MstDrugStorageLocation` | `Id`, `ServiceUnitId`, `IsAllowDispensing`, `IsAllowReceiving` | Depo Optik |
| `PhmDrugStockMutation` | `SourceDocumentType`, `SourceDocumentId` | Jejak mutasi Optik (`OpticalOrder`) |

## Skema DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel** sebagaimana nanti dihasilkan EF Core, **bukan
> skrip untuk dijalankan**. Menjalankannya akan berbenturan dengan migration. Kolom audit
> `IdentityModel` tidak ditulis ulang di setiap tabel.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptRefractionExam" (
    "Id"                             uuid          NOT NULL,
    "ExamNumber"                     varchar(30)   NOT NULL,
    "EncounterId"                    uuid          NOT NULL,
    "PatientId"                      uuid          NOT NULL,
    "ExaminerWorkforceProfileId"     uuid          NOT NULL,
    "ExaminerLicenseNumberSnapshot"  varchar(100)  NOT NULL,
    "Status"                         integer       NOT NULL,  -- enum, HasConversion<int>
    "CurrentRevisionNo"              integer       NOT NULL,
    "StartedAt"                      timestamp     NOT NULL,
    "FinalizedAt"                    timestamp    ,
    "FinalizedBy"                    uuid         ,
    "BillingFactEmittedAt"           timestamp    ,
    "CancelReason"                   varchar(500) ,
    "Version"                        integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptRefractionExam" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptRefractionExam_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptRefractionExam_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptRefractionExam_MstWorkforceProfile_ExaminerWorkforceProfileId"
        FOREIGN KEY ("ExaminerWorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptRefractionExam_ExamNumber" ON public."OptRefractionExam" ("ExamNumber");
CREATE INDEX "IX_OptRefractionExam_EncounterId_Status" ON public."OptRefractionExam" ("EncounterId", "Status");
CREATE INDEX "IX_OptRefractionExam_PatientId_FinalizedAt" ON public."OptRefractionExam" ("PatientId", "FinalizedAt");
CREATE INDEX "IX_OptRefractionExam_ExaminerWorkforceProfileId" ON public."OptRefractionExam" ("ExaminerWorkforceProfileId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptRefractionExamRevision" (
    "Id"                           uuid          NOT NULL,
    "RefractionExamId"             uuid          NOT NULL,
    "RevisionNo"                   integer       NOT NULL,
    "VisusDistanceUncorrectedOd"   varchar(10)  ,  -- SENSITIF
    "VisusDistanceUncorrectedOs"   varchar(10)  ,  -- SENSITIF
    "VisusDistanceCorrectedOd"     varchar(10)  ,  -- SENSITIF
    "VisusDistanceCorrectedOs"     varchar(10)  ,  -- SENSITIF
    "VisusNearUncorrectedOd"       varchar(10)  ,  -- SENSITIF
    "VisusNearUncorrectedOs"       varchar(10)  ,  -- SENSITIF
    "VisusNearCorrectedOd"         varchar(10)  ,  -- SENSITIF
    "VisusNearCorrectedOs"         varchar(10)  ,  -- SENSITIF
    "AutorefOdSph"                 numeric(5,2) ,  -- SENSITIF
    "AutorefOdCyl"                 numeric(5,2) ,  -- SENSITIF
    "AutorefOdAxis"                integer      ,  -- SENSITIF
    "AutorefOsSph"                 numeric(5,2) ,  -- SENSITIF
    "AutorefOsCyl"                 numeric(5,2) ,  -- SENSITIF
    "AutorefOsAxis"                integer      ,  -- SENSITIF
    "SubjectiveOdSph"              numeric(5,2) ,  -- SENSITIF
    "SubjectiveOdCyl"              numeric(5,2) ,  -- SENSITIF
    "SubjectiveOdAxis"             integer      ,  -- SENSITIF
    "SubjectiveOdAdd"              numeric(4,2) ,  -- SENSITIF
    "SubjectiveOsSph"              numeric(5,2) ,  -- SENSITIF
    "SubjectiveOsCyl"              numeric(5,2) ,  -- SENSITIF
    "SubjectiveOsAxis"             integer      ,  -- SENSITIF
    "SubjectiveOsAdd"              numeric(4,2) ,  -- SENSITIF
    "PdDistance"                   numeric(4,1) ,  -- SENSITIF
    "PdNear"                       numeric(4,1) ,  -- SENSITIF
    "KeratometryNote"              varchar(200) ,  -- SENSITIF
    "ClinicalNote"                 varchar(1000),  -- SENSITIF
    "RevisedByWorkforceProfileId"  uuid          NOT NULL,
    "RevisedAt"                    timestamp     NOT NULL,
    "RevisionReason"               varchar(500) ,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptRefractionExamRevision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptRefractionExamRevision_OptRefractionExam_RefractionExamId"
        FOREIGN KEY ("RefractionExamId") REFERENCES public."OptRefractionExam" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptRefractionExamRevision_MstWorkforceProfile_RevisedByWorkforceProfileId"
        FOREIGN KEY ("RevisedByWorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptRefractionExamRevision_RefractionExamId_RevisionNo" ON public."OptRefractionExamRevision" ("RefractionExamId", "RevisionNo");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptPrescription" (
    "Id"                      uuid          NOT NULL,
    "PrescriptionNumber"      varchar(30)   NOT NULL,
    "PatientId"               uuid          NOT NULL,
    "EncounterId"             uuid          NOT NULL,
    "RefractionExamId"        uuid         ,
    "PrescriptionType"        integer       NOT NULL,  -- enum, HasConversion<int>
    "Origin"                  integer       NOT NULL,  -- enum, HasConversion<int>
    "ExternalAuthorName"      varchar(150) ,
    "ExternalFacilityName"    varchar(150) ,
    "IssuedDate"              date          NOT NULL,
    "ValidityMonthsSnapshot"  integer       NOT NULL,
    "ValidUntil"              date          NOT NULL,
    "CurrentVersionNo"        integer       NOT NULL,
    "LockedAt"                timestamp    ,
    "LockedByOrderId"         uuid         ,
    "Version"                 integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptPrescription" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptPrescription_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptPrescription_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptPrescription_OptRefractionExam_RefractionExamId"
        FOREIGN KEY ("RefractionExamId") REFERENCES public."OptRefractionExam" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptPrescription_PrescriptionNumber" ON public."OptPrescription" ("PrescriptionNumber");
CREATE INDEX "IX_OptPrescription_PatientId_ValidUntil" ON public."OptPrescription" ("PatientId", "ValidUntil");
CREATE INDEX "IX_OptPrescription_EncounterId" ON public."OptPrescription" ("EncounterId");
CREATE INDEX "IX_OptPrescription_RefractionExamId" ON public."OptPrescription" ("RefractionExamId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptPrescriptionVersion" (
    "Id"                           uuid          NOT NULL,
    "PrescriptionId"               uuid          NOT NULL,
    "VersionNo"                    integer       NOT NULL,
    "OdSph"                        numeric(5,2) ,  -- SENSITIF
    "OdCyl"                        numeric(5,2) ,  -- SENSITIF
    "OdAxis"                       integer      ,  -- SENSITIF
    "OdAdd"                        numeric(4,2) ,  -- SENSITIF
    "OsSph"                        numeric(5,2) ,  -- SENSITIF
    "OsCyl"                        numeric(5,2) ,  -- SENSITIF
    "OsAxis"                       integer      ,  -- SENSITIF
    "OsAdd"                        numeric(4,2) ,  -- SENSITIF
    "PdDistance"                   numeric(4,1) ,  -- SENSITIF
    "PdNear"                       numeric(4,1) ,  -- SENSITIF
    "OdClPower"                    numeric(5,2) ,  -- SENSITIF
    "OdClCyl"                      numeric(5,2) ,  -- SENSITIF
    "OdClAxis"                     integer      ,  -- SENSITIF
    "OdClBaseCurve"                numeric(4,2) ,  -- SENSITIF
    "OdClDiameter"                 numeric(4,2) ,  -- SENSITIF
    "OsClPower"                    numeric(5,2) ,  -- SENSITIF
    "OsClCyl"                      numeric(5,2) ,  -- SENSITIF
    "OsClAxis"                     integer      ,  -- SENSITIF
    "OsClBaseCurve"                numeric(4,2) ,  -- SENSITIF
    "OsClDiameter"                 numeric(4,2) ,  -- SENSITIF
    "ContactLensBrandOrType"       varchar(150) ,
    "ContactLensReplacement"       varchar(50)  ,
    "IsTransposed"                 boolean       NOT NULL,
    "OriginalNotationText"         varchar(200) ,  -- SENSITIF
    "RangeOverrideReason"          varchar(500) ,
    "AuthorWorkforceProfileId"     uuid          NOT NULL,
    "AuthorLicenseNumberSnapshot"  varchar(100)  NOT NULL,
    "AuthoredAt"                   timestamp     NOT NULL,
    "RevisionReason"               varchar(500) ,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptPrescriptionVersion" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptPrescriptionVersion_OptPrescription_PrescriptionId"
        FOREIGN KEY ("PrescriptionId") REFERENCES public."OptPrescription" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptPrescriptionVersion_MstWorkforceProfile_AuthorWorkforceProfileId"
        FOREIGN KEY ("AuthorWorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptPrescriptionVersion_PrescriptionId_VersionNo" ON public."OptPrescriptionVersion" ("PrescriptionId", "VersionNo");
CREATE INDEX "IX_OptPrescriptionVersion_AuthorWorkforceProfileId" ON public."OptPrescriptionVersion" ("AuthorWorkforceProfileId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptOrder" (
    "Id"                                 uuid          NOT NULL,
    "OrderNumber"                        varchar(30)   NOT NULL,
    "EncounterId"                        uuid          NOT NULL,
    "PatientId"                          uuid          NOT NULL,
    "OrderKind"                          integer       NOT NULL,  -- enum, HasConversion<int>
    "PrescriptionId"                     uuid         ,
    "PrescriptionVersionId"              uuid         ,
    "Status"                             integer       NOT NULL,  -- enum, HasConversion<int>
    "DownPaymentMinPercentSnapshot"      numeric(5,2)  NOT NULL,
    "WorkshopType"                       integer      ,  -- enum, HasConversion<int>
    "ExternalLabSupplierId"              uuid         ,
    "EstimatedReadyDate"                 date         ,
    "BilledAt"                           timestamp    ,
    "SentToWorkAt"                       timestamp    ,
    "ReadyAt"                            timestamp    ,
    "HandedOverAt"                       timestamp    ,
    "HandedOverBy"                       uuid         ,
    "HandoverIdentityVerified"           boolean       NOT NULL,
    "HandoverReceiverName"               varchar(150) ,  -- SENSITIF
    "HandoverReceiverRelationship"       varchar(50)  ,
    "FittingNote"                        varchar(500) ,
    "WarrantyFitDaysSnapshot"            integer      ,
    "WarrantyLensDefectMonthsSnapshot"   integer      ,
    "WarrantyFrameDefectMonthsSnapshot"  integer      ,
    "NotCollectedAt"                     timestamp    ,
    "CancelledAt"                        timestamp    ,
    "IsRemake"                           boolean       NOT NULL,
    "ParentOrderId"                      uuid         ,
    "Version"                            integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptOrder_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrder_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrder_OptPrescription_PrescriptionId"
        FOREIGN KEY ("PrescriptionId") REFERENCES public."OptPrescription" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrder_OptPrescriptionVersion_PrescriptionVersionId"
        FOREIGN KEY ("PrescriptionVersionId") REFERENCES public."OptPrescriptionVersion" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrder_MstSupplier_ExternalLabSupplierId"
        FOREIGN KEY ("ExternalLabSupplierId") REFERENCES public."MstSupplier" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrder_OptOrder_ParentOrderId"
        FOREIGN KEY ("ParentOrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptOrder_OrderNumber" ON public."OptOrder" ("OrderNumber");
CREATE INDEX "IX_OptOrder_EncounterId_Status" ON public."OptOrder" ("EncounterId", "Status");
CREATE INDEX "IX_OptOrder_Status_ReadyAt" ON public."OptOrder" ("Status", "ReadyAt");
CREATE INDEX "IX_OptOrder_PatientId" ON public."OptOrder" ("PatientId");
CREATE INDEX "IX_OptOrder_PrescriptionId" ON public."OptOrder" ("PrescriptionId");
CREATE INDEX "IX_OptOrder_ParentOrderId" ON public."OptOrder" ("ParentOrderId");
CREATE INDEX "IX_OptOrder_ExternalLabSupplierId" ON public."OptOrder" ("ExternalLabSupplierId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptOrderItem" (
    "Id"                  uuid          NOT NULL,
    "OrderId"             uuid          NOT NULL,
    "LineNo"              integer       NOT NULL,
    "DrugId"              uuid          NOT NULL,
    "ItemCategory"        integer       NOT NULL,  -- enum, HasConversion<int>
    "LensSupply"          integer      ,  -- enum, HasConversion<int>
    "Quantity"            numeric(10,3) NOT NULL,
    "Status"              integer       NOT NULL,  -- enum, HasConversion<int>
    "IsLensCut"           boolean      ,
    "StockIssuedAt"       timestamp    ,
    "StockReturnedAt"     timestamp    ,
    "BillingFactVersion"  integer       NOT NULL,
    "CancelledAt"         timestamp    ,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptOrderItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptOrderItem_OptOrder_OrderId"
        FOREIGN KEY ("OrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptOrderItem_MstDrug_DrugId"
        FOREIGN KEY ("DrugId") REFERENCES public."MstDrug" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptOrderItem_OrderId_LineNo" ON public."OptOrderItem" ("OrderId", "LineNo");
CREATE INDEX "IX_OptOrderItem_DrugId" ON public."OptOrderItem" ("DrugId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptOrderCancellation" (
    "Id"            uuid          NOT NULL,
    "OrderId"       uuid          NOT NULL,
    "Status"        integer       NOT NULL,  -- enum, HasConversion<int>
    "Reason"        varchar(500)  NOT NULL,
    "RequestedBy"   uuid          NOT NULL,
    "RequestedAt"   timestamp     NOT NULL,
    "DecidedBy"     uuid         ,
    "DecidedAt"     timestamp    ,
    "DecisionNote"  varchar(500) ,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptOrderCancellation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptOrderCancellation_OptOrder_OrderId"
        FOREIGN KEY ("OrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptOrderCancellation_OrderId_Requested" ON public."OptOrderCancellation" ("OrderId") WHERE "Status" = 1 AND "IsDelete" = false;
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptOrderStatusHistory" (
    "Id"           uuid          NOT NULL,
    "OrderId"      uuid          NOT NULL,
    "FromStatus"   integer      ,  -- enum, HasConversion<int>
    "ToStatus"     integer       NOT NULL,  -- enum, HasConversion<int>
    "ActorUserId"  uuid          NOT NULL,
    "OccurredAt"   timestamp     NOT NULL,
    "Reason"       varchar(500) ,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptOrderStatusHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptOrderStatusHistory_OptOrder_OrderId"
        FOREIGN KEY ("OrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_OptOrderStatusHistory_OrderId_OccurredAt" ON public."OptOrderStatusHistory" ("OrderId", "OccurredAt");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptWarrantyClaim" (
    "Id"                            uuid          NOT NULL,
    "ClaimNumber"                   varchar(30)   NOT NULL,
    "OrderId"                       uuid          NOT NULL,
    "EncounterId"                   uuid          NOT NULL,
    "ComplaintType"                 integer       NOT NULL,  -- enum, HasConversion<int>
    "ComplaintNote"                 varchar(1000) NOT NULL,  -- SENSITIF
    "IsWithinWarranty"              boolean       NOT NULL,
    "WarrantyDeadlineSnapshot"      date         ,
    "Status"                        integer       NOT NULL,  -- enum, HasConversion<int>
    "ExaminedByWorkforceProfileId"  uuid         ,
    "ExaminedAt"                    timestamp    ,
    "ExaminationFinding"            varchar(1000),  -- SENSITIF
    "ReexamRefractionExamId"        uuid         ,
    "DecidedBy"                     uuid         ,
    "DecidedAt"                     timestamp    ,
    "DecisionReason"                varchar(500) ,
    "RemakeOrderId"                 uuid         ,
    "Version"                       integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptWarrantyClaim" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptWarrantyClaim_OptOrder_OrderId"
        FOREIGN KEY ("OrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptWarrantyClaim_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptWarrantyClaim_MstWorkforceProfile_ExaminedByWorkforceProfileId"
        FOREIGN KEY ("ExaminedByWorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptWarrantyClaim_OptRefractionExam_ReexamRefractionExamId"
        FOREIGN KEY ("ReexamRefractionExamId") REFERENCES public."OptRefractionExam" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_OptWarrantyClaim_OptOrder_RemakeOrderId"
        FOREIGN KEY ("RemakeOrderId") REFERENCES public."OptOrder" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptWarrantyClaim_ClaimNumber" ON public."OptWarrantyClaim" ("ClaimNumber");
CREATE INDEX "IX_OptWarrantyClaim_OrderId" ON public."OptWarrantyClaim" ("OrderId");
CREATE INDEX "IX_OptWarrantyClaim_EncounterId" ON public."OptWarrantyClaim" ("EncounterId");
CREATE UNIQUE INDEX "UX_OptWarrantyClaim_RemakeOrderId" ON public."OptWarrantyClaim" ("RemakeOrderId") WHERE "RemakeOrderId" IS NOT NULL;
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptItemProfile" (
    "Id"                      uuid          NOT NULL,
    "DrugId"                  uuid          NOT NULL,
    "ItemCategory"            integer       NOT NULL,  -- enum, HasConversion<int>
    "Color"                   varchar(50)  ,
    "FrameSize"               varchar(20)  ,
    "FrameMaterial"           varchar(50)  ,
    "LensIndex"               numeric(4,2) ,
    "LensDesign"              integer      ,  -- enum, HasConversion<int>
    "LensCoating"             varchar(100) ,
    "ContactLensBaseCurve"    numeric(4,2) ,
    "ContactLensDiameter"     numeric(4,2) ,
    "ContactLensReplacement"  varchar(50)  ,
    "IsActive"                boolean       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptItemProfile" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptItemProfile_MstDrug_DrugId"
        FOREIGN KEY ("DrugId") REFERENCES public."MstDrug" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptItemProfile_DrugId" ON public."OptItemProfile" ("DrugId");
CREATE INDEX "IX_OptItemProfile_ItemCategory" ON public."OptItemProfile" ("ItemCategory");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptSetting" (
    "Id"                          uuid          NOT NULL,
    "ServiceUnitId"               uuid          NOT NULL,
    "DownPaymentMinPercent"       numeric(5,2)  NOT NULL,
    "PickupReminderDays"          integer       NOT NULL,
    "NotCollectedCloseDays"       integer       NOT NULL,
    "PrescriptionValidityMonths"  integer       NOT NULL,
    "WarrantyFitDays"             integer       NOT NULL,
    "WarrantyLensDefectMonths"    integer       NOT NULL,
    "WarrantyFrameDefectMonths"   integer       NOT NULL,
    "ContactLensBaseCurveMin"     numeric(4,2)  NOT NULL,
    "ContactLensBaseCurveMax"     numeric(4,2)  NOT NULL,
    "ContactLensDiameterMin"      numeric(4,2)  NOT NULL,
    "ContactLensDiameterMax"      numeric(4,2)  NOT NULL,
    "Version"                     integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptSetting" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptSetting_MstServiceUnit_ServiceUnitId"
        FOREIGN KEY ("ServiceUnitId") REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_OptSetting_ServiceUnitId" ON public."OptSetting" ("ServiceUnitId");
```

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."OptSettingRevision" (
    "Id"                    uuid          NOT NULL,
    "SettingId"             uuid          NOT NULL,
    "ChangedFieldsSummary"  varchar(2000) NOT NULL,
    "ChangedBy"             uuid          NOT NULL,
    "ChangedAt"             timestamp     NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_OptSettingRevision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OptSettingRevision_OptSetting_SettingId"
        FOREIGN KEY ("SettingId") REFERENCES public."OptSetting" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_OptSettingRevision_SettingId_ChangedAt" ON public."OptSettingRevision" ("SettingId", "ChangedAt");
```

```sql
-- RegPatientEncounter: hanya perubahan. Bukan skrip untuk dijalankan.
ALTER TABLE public."RegPatientEncounter" ADD "ReferralSourceEncounterId" uuid NULL;
CREATE INDEX "IX_RegPatientEncounter_ReferralSourceEncounterId" ON public."RegPatientEncounter" ("ReferralSourceEncounterId");
ALTER TABLE public."RegPatientEncounter" ADD CONSTRAINT "FK_RegPatientEncounter_RegPatientEncounter_ReferralSourceEncounterId"
    FOREIGN KEY ("ReferralSourceEncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT;
```
