# Kamus Data — Modul MCU

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21 |
| Sumber | `02-backend-architecture.md` r1 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan
`IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Seluruh unique index pada
tabel MCU **difilter** `"IsDelete" = false`, kecuali disebut lain. Seluruh tabel di schema `public`, nama
tabel = nama entity. Seluruh nilai uang `numeric(18,2)`.

**Kolom Sensitif = Ya** tidak boleh masuk log aplikasi, tidak boleh dipakai sebagai contoh berisi data
asli, dan ditinjau masking-nya pada respons.

---

## 1. Katalog MCU

### `McuPackage` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PackageCode` | `string(30)` | Ya | — | UK | — | — | Tidak | Kode paket, huruf besar, tanpa spasi. Contoh `MCU-BASIC` |
| `PackageName` | `string(150)` | Ya | — | — | — | — | Tidak | Contoh *Paket Basic* |
| `Description` | `string(1000)` | Tidak | — | — | — | — | Tidak | — |
| `GeneralTariffId` | `Guid` | Ya | — | Index | FK `MstTariff` | `Restrict` | Tidak | Harga umum; tarif wajib `IsPackageTariff = true` |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Nonaktif = tidak bisa dipilih untuk order baru |

### `McuPackageComponent` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PackageId` | `Guid` | Ya | — | Index | FK `McuPackage` | `Restrict` | Tidak | — |
| `ComponentType` | `McuComponentType` (int) | Ya | — | — | — | — | Tidak | 1 Lab, 2 Rad, 3 Pemeriksaan MCU |
| `ProcedureId` | `Guid?` | Lab/Rad | — | UK bersama `PackageId` (filter bukan NULL) | FK `MstProcedure` | `Restrict` | Tidak | Prosedur wajib ber-`IsLaboratory` (Lab) atau `IsRadiology` (Rad) |
| `RadModalityId` | `Guid?` | Rad | — | Index | FK `MstRadModality` | `Restrict` | Tidak | Modalitas untuk pesanan Rad |
| `ExaminationTypeId` | `Guid?` | Pemeriksaan MCU | — | UK bersama `PackageId` (filter bukan NULL) | FK `McuExaminationType` | `Restrict` | Tidak | — |
| `RefundAmount` | `decimal` | Ya | `0` | — | — | — | Tidak | Nilai pengembalian bila komponen dibatalkan (`MCU-DEC-015`); ≥ 0 |
| `MedicalFeeAmount` | `decimal` | Ya | `0` | — | — | — | Tidak | Nilai jasa medis (`MCU-DEC-034`); ≥ 0 |

### `McuPackageComponentRequirement` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PackageComponentId` | `Guid` | Ya | — | Index | FK `McuPackageComponent` | `Restrict` | Tidak | — |
| `RequirementText` | `string(300)` | Ya | — | — | — | — | Tidak | Contoh *Puasa 10–12 jam sebelum pengambilan darah* |
| `IsConfirmationRequired` | `bool` | Ya | `true` | — | — | — | Tidak | Wajib ditandai saat check-in |

### `McuPackageCompanyPrice` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PackageId` | `Guid` | Ya | — | Index (`PackageId`, `CompanyGuarantorId`) | FK `McuPackage` | `Restrict` | Tidak | — |
| `CompanyGuarantorId` | `Guid` | Ya | — | ↑ | FK `MstCompanyGuarantor` | `Restrict` | Tidak | — |
| `TariffId` | `Guid` | Ya | — | Index | FK `MstTariff` | `Restrict` | Tidak | Tarif khusus ber-`IsPackageTariff` |
| `EffectiveStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal kalender WIB |
| `EffectiveEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Kosong = tanpa batas; tidak boleh tumpang tindih (`VAL-MCU-05`) |

### `McuExaminationType` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ExaminationTypeCode` | `string(30)` | Ya | — | UK | — | — | Tidak | Contoh `FISIK`, `VISUS` |
| `ExaminationTypeName` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### `McuExaminationField` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ExaminationTypeId` | `Guid` | Ya | — | UK bersama `FieldCode`; UK bersama `FieldNumber` | FK `McuExaminationType` | `Restrict` | Tidak | — |
| `FieldCode` | `string(50)` | Ya | — | ↑ | — | — | Tidak | Contoh `TEKANAN_SISTOLIK` |
| `FieldLabel` | `string(150)` | Ya | — | — | — | — | Tidak | Contoh *Tekanan darah sistolik* |
| `FieldType` | `McuExaminationFieldType` (int) | Ya | — | — | — | — | Tidak | 1 Teks, 2 Angka, 3 Pilihan, 4 Ya/Tidak |
| `UnitName` | `string(30)` | Tidak | — | — | — | — | Tidak | Contoh `mmHg` |
| `OptionValues` | `string(1000)` | Pilihan | — | — | — | — | Tidak | Pilihan dipisah `|`, contoh `Normal|Parsial|Total` |
| `IsRequired` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `FieldNumber` | `int` | Ya | — | ↑ | — | — | Tidak | Nomor butir pada formulir; ≥ 1 |

### `McuFitnessCategory` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `FitnessCategoryCode` | `string(30)` | Ya | — | UK | — | — | Tidak | Contoh `FIT`, `UNFIT_SEMENTARA` |
| `FitnessCategoryName` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `Description` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

---

## 2. Jadwal MCU

### `McuDailySchedule` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ScheduleDate` | `DateOnly` | Ya | — | UK | — | — | Tidak | Tanggal kalender WIB |
| `IsOpen` | `bool` | Ya | `true` | — | — | — | Tidak | — |
| `Quota` | `int` | Ya | — | — | — | — | Tidak | ≥ 0 |
| `UsedQuota` | `int` | Ya | `0` | — | — | — | Tidak | Dijaga service; tidak boleh diubah lewat API |
| `ApprovedOverrideQuota` | `int` | Ya | `0` | — | — | — | Tidak | Jumlah tambahan dari `McuQuotaOverride` |
| `DutyMcuDoctorId` | `Guid?` | Tidak | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter MCU jaga tanggal itu (`MCU-DEC-042`): dipakai otomatis oleh check-in kiosk dan menjadi isian bawaan check-in admisi. Kosong → kiosk menolak (`VAL-MCU-47`) |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi (`IsConcurrencyToken`) |

### `McuQuotaOverride` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `DailyScheduleId` | `Guid` | Ya | — | Index | FK `McuDailySchedule` | `Restrict` | Tidak | — |
| `AdditionalQuota` | `int` | Ya | — | — | — | — | Tidak | ≥ 1 |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Contoh *Kontrak tahunan PT Contoh* |
| `ApprovedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Kepala unit MCU |
| `ApprovedAt` | `DateTime` | Ya | waktu server UTC | — | — | — | Tidak | — |

---

## 3. Layanan MCU

### `McuOrder` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OrderNumber` | `string(30)` | Ya | dari seri nomor | UK (tanpa filter) | — | — | Tidak | `MCU-RSMMC-000001`; dialokasikan service (QBE-CODE-001..004) |
| `PackageId` | `Guid` | Ya | — | Index | FK `McuPackage` | `Restrict` | Tidak | — |
| `PackageCodeSnapshot` | `string(30)` | Ya | — | — | — | — | Tidak | Potret saat order dibuat |
| `PackageNameSnapshot` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `PatientId` | `Guid?` | Sejak *Hadir* | — | Index | FK `MstPatient` | `Restrict` | Tidak | Kosong selama identitas sementara |
| `ParticipantName` | `string(150)` | Ya | — | — | — | — | **Ya** | Diisi dari pasien bila sudah pasien |
| `ParticipantNik` | `string(16)` | Bila belum pasien | — | Index | — | — | **Ya** | 16 digit |
| `ParticipantBirthDate` | `DateOnly?` | Bila belum pasien | — | — | — | — | **Ya** | — |
| `ParticipantPhone` | `string(20)` | Tidak | — | — | — | — | **Ya** | — |
| `PatientMatchMethod` | `McuPatientMatchMethod?` (int) | Sejak *Hadir* | — | — | — | — | Tidak | Cara tertaut ke pasien |
| `PatientMatchConfirmedByUserId` | `Guid?` | Bila dikonfirmasi petugas | — | — | FK `ApplicationUser` | `Restrict` | Tidak | `MCU-DEC-039` |
| `Source` | `McuOrderSource` (int) | Ya | — | — | — | — | Tidak | Pintu asal |
| `SourceEncounterId` | `Guid?` | Rekomendasi | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunjungan RJ/RI/IGD asal rekomendasi |
| `RecommendingDoctorId` | `Guid?` | Rekomendasi | — | — | FK `MstDoctor` | `Restrict` | Tidak | — |
| `CompanyGuarantorId` | `Guid?` | Tidak | — | Index | FK `MstCompanyGuarantor` | `Restrict` | Tidak | Peserta perusahaan |
| `BookingImportId` | `Guid?` | Tidak | — | Index | FK `McuBookingImport` | `Restrict` | Tidak | — |
| `ExaminationDate` | `DateOnly` | Ya | — | Index (`ExaminationDate`, `OrderStatus`) | — | — | Tidak | Tanggal kalender WIB |
| `DailyScheduleId` | `Guid` | Ya | — | Index | FK `McuDailySchedule` | `Restrict` | Tidak | — |
| `OrderStatus` | `McuOrderStatus` (int) | Ya | `Scheduled` atau `CheckedIn` | ↑ | — | — | Tidak | Sembilan status |
| `ArrivalConfirmedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Konfirmasi WhatsApp (`MCU-DEC-030`); kelak |
| `EncounterId` | `Guid?` | Sejak *Hadir* | — | UK (filter bukan NULL) | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `CheckInChannel` | `McuCheckInChannel?` (int) | Sejak *Hadir* | — | — | — | — | Tidak | 1 Admisi, 2 Kiosk |
| `CheckedInAt` | `DateTime?` | Sejak *Hadir* | — | — | — | — | Tidak | — |
| `CheckedInByUserId` | `Guid?` | Sejak *Hadir* | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Akun kiosk untuk jalur kiosk |
| `McuDoctorId` | `Guid?` | Sejak *Hadir* | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Salinan dokter berlaku dari `McuOrderDoctorAssignment` |
| `AppliedTariffId` | `Guid?` | Sejak *Hadir* | — | — | FK `MstTariff` | `Restrict` | Tidak | Tarif umum atau tarif perusahaan yang dipakai |
| `PackageInvoiceItemId` | `Guid?` | Sejak *Hadir* | — | — | FK `BilInvoiceItem` | `Restrict` | Tidak | Baris paket |
| `ClearanceBasis` | `McuClearanceBasis?` (int) | Sejak *Dalam Pemeriksaan* | — | — | — | — | Tidak | 1 Lunas, 2 Terjamin |
| `ExaminationStartedAt` | `DateTime?` | Sejak *Dalam Pemeriksaan* | — | — | — | — | Tidak | — |
| `NoShowAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CancelReason` | `string(500)` | Bila batal | — | — | — | — | Tidak | Kolom `IsCancel`/`CancelDateTime`/`CancelBy` dari `IdentityModel` ikut diisi |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

### `McuOrderComponent` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | Index | FK `McuOrder` | `Restrict` | Tidak | — |
| `PackageComponentId` | `Guid?` | Komponen paket | — | — | FK `McuPackageComponent` | `Restrict` | Tidak | Kosong = di luar paket |
| `ComponentType` | `McuComponentType` (int) | Ya | — | — | — | — | Tidak | — |
| `ProcedureId` | `Guid?` | Lab/Rad | — | UK bersama `McuOrderId` (filter bukan NULL, `ComponentStatus` ≠ Dibatalkan) | FK `MstProcedure` | `Restrict` | Tidak | — |
| `RadModalityId` | `Guid?` | Rad | — | — | FK `MstRadModality` | `Restrict` | Tidak | — |
| `ExaminationTypeId` | `Guid?` | Pemeriksaan MCU | — | UK bersama `McuOrderId` (sama) | FK `McuExaminationType` | `Restrict` | Tidak | — |
| `ComponentNameSnapshot` | `string(200)` | Ya | — | — | — | — | Tidak | — |
| `IsPackageCovered` | `bool` | Ya | `true` | — | — | — | Tidak | `false` = di luar paket, ditagih biasa (`MCU-DEC-036`) |
| `RefundAmountSnapshot` | `decimal` | Ya | `0` | — | — | — | Tidak | — |
| `MedicalFeeAmountSnapshot` | `decimal` | Ya | `0` | — | — | — | Tidak | — |
| `ComponentStatus` | `McuOrderComponentStatus` (int) | Ya | `Pending` | Index | — | — | Tidak | — |
| `ScheduledDate` | `DateOnly` | Ya | = `ExaminationDate` | — | — | — | Tidak | Berubah bila dijadwal ulang |
| `LabOrderId` | `Guid?` | Lab sejak *Dipesan* | — | Index | FK `LabOrder` | `Restrict` | Tidak | Boleh sama untuk beberapa komponen satu disiplin |
| `RadOrderId` | `Guid?` | Rad sejak *Dipesan* | — | Index | FK `RadOrder` | `Restrict` | Tidak | — |
| `PerformingDoctorId` | `Guid?` | Saat selesai bila ada jasa | — | — | FK `MstDoctor` | `Restrict` | Tidak | Dokter pelaksana/pembaca |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CancelReason` | `string(500)` | Bila dibatalkan | — | — | — | — | Tidak | Contoh *Kehamilan* |
| `CancelledByDoctorId` | `Guid?` | Bila dibatalkan | — | — | FK `MstDoctor` | `Restrict` | Tidak | Dokter MCU |
| `RefundCaseId` | `Guid?` | Bila ada pengembalian | — | — | FK `BilRefundCase` | `Restrict` | Tidak | — |
| `MedicalFeeReleasedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Waktu komponen tersedia sebagai sumber jasa medis |
| `AddedByDoctorId` | `Guid?` | Di luar paket | — | — | FK `MstDoctor` | `Restrict` | Tidak | — |

### `McuOrderRequirementCheck` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | Index | FK `McuOrder` | `Restrict` | Tidak | — |
| `McuOrderComponentId` | `Guid` | Ya | — | — | FK `McuOrderComponent` | `Restrict` | Tidak | — |
| `RequirementTextSnapshot` | `string(300)` | Ya | — | — | — | — | Tidak | — |
| `Outcome` | `McuRequirementOutcome` (int) | Ya | — | — | — | — | Tidak | 1 Terpenuhi, 2 Lanjut dengan catatan, 3 Komponen dijadwal ulang |
| `Note` | `string(500)` | Bila 2 | — | — | — | — | **Ya** | Contoh *Mengaku sudah sarapan pukul 06.00* |
| `CheckedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `CheckedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | — |

### `McuOrderStatusHistory` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | Index (`McuOrderId`, `ChangedAt`) | FK `McuOrder` | `Restrict` | Tidak | — |
| `FromStatus` | `McuOrderStatus?` (int) | Tidak | — | — | — | — | Tidak | Kosong untuk status awal |
| `ToStatus` | `McuOrderStatus` (int) | Ya | — | — | — | — | Tidak | — |
| `ChangedAt` | `DateTime` | Ya | waktu server | ↑ | — | — | Tidak | Dipakai laporan kelak |
| `ChangedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Kosong bila sistem (tutup hari) |
| `Reason` | `string(500)` | Tidak | — | — | — | — | Tidak | — |

### `McuOrderDoctorAssignment` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | UK filtered (`McuOrderId`) bila `EndedAt` NULL | FK `McuOrder` | `Restrict` | Tidak | Hanya satu yang berlaku |
| `DoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | — |
| `AssignedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | — |
| `AssignedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Admisi (check-in) atau kepala unit (ganti) |
| `Reason` | `string(500)` | Bila ganti | — | — | — | — | Tidak | `MCU-DEC-040` |
| `EndedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |

### `McuBookingImport` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ImportNumber` | `string(30)` | Ya | dari seri nomor | UK | — | — | Tidak | `MCU-IMP-000001` |
| `CompanyGuarantorId` | `Guid` | Ya | — | Index | FK `MstCompanyGuarantor` | `Restrict` | Tidak | — |
| `PackageId` | `Guid` | Ya | — | — | FK `McuPackage` | `Restrict` | Tidak | — |
| `ExaminationDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `FileName` | `string(255)` | Ya | — | — | — | — | Tidak | Nama berkas saja; isi tidak disimpan |
| `TotalRows` | `int` | Ya | — | — | — | — | Tidak | — |
| `CreatedRows` | `int` | Ya | — | — | — | — | Tidak | — |
| `FailedRows` | `int` | Ya | — | — | — | — | Tidak | — |
| `FailureSummary` | `string(4000)` | Tidak | — | — | — | — | **Ya** | Nomor baris + sebab, dapat memuat nama |

### `McuExaminationResult` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderComponentId` | `Guid` | Ya | — | UK | FK `McuOrderComponent` | `Restrict` | Tidak | Komponen ber-`ComponentType` 3 |
| `ConclusionText` | `string(2000)` | Tidak | — | — | — | — | **Ya** | Contoh *EKG: irama sinus, kesan normal* |
| `FilledByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `FilledAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | — |

### `McuExaminationResultValue` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ExaminationResultId` | `Guid` | Ya | — | UK bersama `ExaminationFieldId` | FK `McuExaminationResult` | `Restrict` | Tidak | — |
| `ExaminationFieldId` | `Guid` | Ya | — | ↑ | FK `McuExaminationField` | `Restrict` | Tidak | — |
| `ValueText` | `string(500)` | Bila isian wajib | — | — | — | — | **Ya** | Angka disimpan sebagai teks berformat titik desimal (`6.4`), divalidasi menurut `FieldType` |

### `McuExaminationResultValueRevision` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ResultValueId` | `Guid` | Ya | — | Index | FK `McuExaminationResultValue` | `Restrict` | Tidak | — |
| `PreviousValueText` | `string(500)` | Tidak | — | — | — | — | **Ya** | — |
| `ResumeVersionNumber` | `int` | Ya | — | — | — | — | Tidak | Versi resume yang disetujui dengan nilai lama |
| `ChangedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `ChangedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | — |

### `McuResume` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | UK bersama `VersionNumber`; UK filtered `IsCurrent = true` | FK `McuOrder` | `Restrict` | Tidak | — |
| `VersionNumber` | `int` | Ya | `1` | ↑ | — | — | Tidak | — |
| `ResumeStatus` | `McuResumeStatus` (int) | Ya | `Draft` | — | — | — | Tidak | — |
| `IsCurrent` | `bool` | Ya | `true` | ↑ | — | — | Tidak | — |
| `GeneralConditionText` | `string(4000)` | Saat approve | — | — | — | — | **Ya** | Kondisi kesehatan keseluruhan |
| `ImportantFindingsText` | `string(4000)` | Tidak | — | — | — | — | **Ya** | Temuan penting |
| `RecommendationText` | `string(4000)` | Saat approve | — | — | — | — | **Ya** | Rekomendasi medis berikutnya |
| `FitnessCategoryId` | `Guid?` | Saat approve | — | — | FK `McuFitnessCategory` | `Restrict` | Tidak | — |
| `WorkRecommendationText` | `string(1000)` | Tidak | — | — | — | — | **Ya** | Satu-satunya teks yang boleh ke perusahaan tanpa persetujuan |
| `AuthorDoctorId` | `Guid?` | Saat simpan dokter | — | — | FK `MstDoctor` | `Restrict` | Tidak | — |
| `ApprovedByDoctorId` | `Guid?` | Sejak disetujui | — | — | FK `MstDoctor` | `Restrict` | Tidak | — |
| `ApprovedAt` | `DateTime?` | ↑ | — | — | — | — | Tidak | — |
| `ReturnReason` | `string(500)` | Bila dikembalikan | — | — | — | — | Tidak | — |
| `VerifiedByUserId` | `Guid?` | Sejak terverifikasi | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Bukan akun dokter pemberi approve |
| `VerifiedAt` | `DateTime?` | ↑ | — | — | — | — | Tidak | — |
| `ReopenReason` | `string(500)` | Bila dikoreksi | — | — | — | — | Tidak | Ditulis pada versi lama |
| `ReopenedByDoctorId` | `Guid?` | ↑ | — | — | FK `MstDoctor` | `Restrict` | Tidak | — |
| `ReopenedAt` | `DateTime?` | ↑ | — | — | — | — | Tidak | — |

### `McuReleaseConsent` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | UK filtered bila `RevokedAt` NULL | FK `McuOrder` | `Restrict` | Tidak | Satu persetujuan berlaku per order |
| `CompanyGuarantorId` | `Guid` | Ya | — | — | FK `MstCompanyGuarantor` | `Restrict` | Tidak | Wajib sama dengan perusahaan order |
| `ConsentGivenAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `RecordedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `EvidenceType` | `McuConsentEvidenceType` (int) | Ya | — | — | — | — | Tidak | 1 Formulir kertas, 2 Pernyataan elektronik — menunggu `MCU-OQ-003` |
| `EvidenceReference` | `string(200)` | Tidak | — | — | — | — | Tidak | Nomor formulir kertas |
| `RevokedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RevokedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `RevokeReason` | `string(500)` | Bila dicabut | — | — | — | — | Tidak | — |

### `McuResultDelivery` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `McuOrderId` | `Guid` | Ya | — | Index | FK `McuOrder` | `Restrict` | Tidak | — |
| `McuResumeId` | `Guid` | Ya | — | Index | FK `McuResume` | `Restrict` | Tidak | Versi terverifikasi yang diserahkan |
| `RecipientType` | `McuDeliveryRecipient` (int) | Ya | — | — | — | — | Tidak | 1 Peserta, 2 Perusahaan |
| `ContentType` | `McuDeliveryContent` (int) | Ya | — | — | — | — | Tidak | 1 Lengkap, 2 Ringkasan kelayakan |
| `Channel` | `McuDeliveryChannel` (int) | Ya | — | — | — | — | Tidak | 1 Ambil langsung, 2 WhatsApp, 3 Email, 4 Telepon |
| `DestinationText` | `string(200)` | WhatsApp/Email/Telepon | — | — | — | — | **Ya** | Nomor atau alamat tujuan |
| `DeliveredAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `DeliveredByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `Note` | `string(500)` | Tidak | — | — | — | — | Tidak | Contoh *Telepon diterima peserta* |
| `RequiresCorrectionNotice` | `bool` | Ya | `false` | Index | — | — | Tidak | Diset `true` saat resume dibuka ulang |
| `CorrectionNoticeRecordedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |

---

## 4. Tabel modul lain

### `LabOrder` — Diperbarui (pemilik Laboratorium)

Kolom lengkap: `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`. Yang dipakai MCU:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | — | PK | — | — | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | — | FK `RegPatientEncounter` | — | Tidak | Kunjungan MCU |
| `OrderStatus` | `LabOrderStatus` | Ya | — | — | — | — | Tidak | `Completed` = komponen tuntas |
| `InstructingDoctorId` | `Guid?` | — | — | — | FK `MstDoctor` | — | Tidak | Diisi dokter MCU (`MCU-DEC-032`) |
| **`CoveringMcuOrderId`** | `Guid?` | Tidak | `NULL` | Index | FK `McuOrder` | `Restrict` | Tidak | **Baru.** Terisi = dicakup paket, tidak menerbitkan `LaboratoryCharge` |

### `RadOrder` — Diperbarui (pemilik Radiologi)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | — | PK | — | — | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | — | FK `RegPatientEncounter` | — | Tidak | — |
| `OrderStatus` | `RadOrderStatus` | Ya | — | — | — | — | Tidak | Selesai = komponen tuntas |
| `InstructingDoctorId` | `Guid?` | — | — | — | FK `MstDoctor` | — | Tidak | Diisi dokter MCU |
| `ExaminingDoctorId` | `Guid?` | — | — | — | FK `MstDoctor` | — | Tidak | Dokter pembaca → `PerformingDoctorId` komponen |
| **`CoveringMcuOrderId`** | `Guid?` | Tidak | `NULL` | Index | FK `McuOrder` | `Restrict` | Tidak | **Baru.** Terisi = tidak menerbitkan `RadiologyCharge` |

### Sudah ada — kolom kunci saja

| Tabel | Kolom yang dipakai MCU | File model |
| --- | --- | --- |
| `MstPatient` | `Id`, `MedicalRecordNumber`, `FullName`, `IdentityNumber` (Sensitif), `BirthDate` (Sensitif) | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` |
| `RegPatientEncounter` | `Id`, `PatientId`, `EncounterType` (`MedicalCheckup = 4`), `EncounterStatus`, `PaymentType`, `ClinicId` | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` |
| `MstTariff` | `Id`, `IsPackageTariff`, `NormalPrice`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | `Areas/HealthServices/MasterData/Models/MstTariff.cs` |
| `MstProcedure` | `Id`, `IsLaboratory`, `LabDiscipline`, `IsRadiology` | `Areas/HealthServices/MasterData/Models/MstProcedure.cs` |
| `MstCompanyGuarantor` | `Id`, `ContractStartDate`, `ContractEndDate`, `IsNeedGuaranteeLetter`, `IsActive` | `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` |
| `BilInvoiceItem` | `Id`, `InvoiceId`, `Status` | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs` |
| `BilRefundCase` | `Id`, status | `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundCase.cs` |

---

## 5. Enum

Seluruhnya di `Areas/HealthServices/MedicalCheckupManagement/Enums/MedicalCheckupEnums.cs`, disimpan
`HasConversion<int>()`. Nilai `0` sengaja tidak dipakai agar nilai kosong tidak tersimpan diam-diam.

| Enum | Nilai | Bawaan |
| --- | --- | --- |
| `McuComponentType` | `Laboratory = 1`, `Radiology = 2`, `McuExamination = 3` | — |
| `McuExaminationFieldType` | `Text = 1`, `Number = 2`, `Option = 3`, `YesNo = 4` | — |
| `McuOrderSource` | `DirectBooking = 1`, `OutpatientRecommendation = 2`, `InpatientRecommendation = 3`, `EmergencyRecommendation = 4`, `WalkInAdmission = 5`, `WalkInKiosk = 6`, `CompanyImport = 7` | — |
| `McuOrderStatus` | `Scheduled = 1` (*Terjadwal*), `NoShow = 2` (*Tidak Hadir*), `CheckedIn = 3` (*Hadir*), `InExamination = 4` (*Dalam Pemeriksaan*), `AwaitingResume = 5` (*Menunggu Resume*), `DoctorApproved = 6` (*Disetujui Dokter*), `Verified = 7` (*Terverifikasi*), `Delivered = 8` (*Diserahkan*), `Cancelled = 9` (*Dibatalkan*) | `Scheduled`; *walk-in* `CheckedIn` |
| `McuOrderComponentStatus` | `Pending = 1`, `Ordered = 2`, `ReadyToFill = 3`, `Rescheduled = 4`, `Completed = 5`, `Cancelled = 6` | `Pending` |
| `McuRequirementOutcome` | `Fulfilled = 1`, `ProceedWithNote = 2`, `ComponentRescheduled = 3` | — |
| `McuPatientMatchMethod` | `ExistingAtBooking = 1`, `NikAndBirthDateMatch = 2`, `ConfirmedByStaff = 3`, `NewPatientRegistered = 4` | — |
| `McuCheckInChannel` | `Admission = 1`, `Kiosk = 2` | — |
| `McuClearanceBasis` | `Paid = 1`, `Guaranteed = 2` | — |
| `McuResumeStatus` | `Draft = 1`, `Approved = 2`, `Verified = 3`, `Corrected = 4` | `Draft` |
| `McuConsentEvidenceType` | `PaperForm = 1`, `ElectronicAcknowledgement = 2` | — |
| `McuDeliveryRecipient` | `Participant = 1`, `Company = 2` | — |
| `McuDeliveryContent` | `Full = 1`, `FitnessSummary = 2` | — |
| `McuDeliveryChannel` | `Pickup = 1`, `WhatsApp = 2`, `Email = 3`, `Phone = 4` | — |

---

## 6. Bentuk tabel (DDL)

> **Peringatan.** Basis data proyek ini dibentuk EF Core Migrations. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan dengan
> migration. Kolom audit `IdentityModel` tidak ditulis ulang. Bentuk akhir mengikuti file configuration
> di `Repositories/Configurations/HealthServices/MedicalCheckupManagement/`.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."McuPackage" (
    "Id" uuid NOT NULL, "PackageCode" varchar(30) NOT NULL, "PackageName" varchar(150) NOT NULL,
    "Description" varchar(1000), "GeneralTariffId" uuid NOT NULL, "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_McuPackage" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackage_MstTariff_GeneralTariffId" FOREIGN KEY ("GeneralTariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_McuPackage_PackageCode" ON public."McuPackage" ("PackageCode") WHERE "IsDelete" = false;

CREATE TABLE public."McuPackageComponent" (
    "Id" uuid NOT NULL, "PackageId" uuid NOT NULL, "ComponentType" integer NOT NULL,
    "ProcedureId" uuid, "RadModalityId" uuid, "ExaminationTypeId" uuid,
    "RefundAmount" numeric(18,2) NOT NULL DEFAULT 0, "MedicalFeeAmount" numeric(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT "PK_McuPackageComponent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageComponent_McuPackage_PackageId" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_MstProcedure_ProcedureId" FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_MstRadModality_RadModalityId" FOREIGN KEY ("RadModalityId") REFERENCES public."MstRadModality" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_McuExaminationType_ExaminationTypeId" FOREIGN KEY ("ExaminationTypeId") REFERENCES public."McuExaminationType" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_McuPackageComponent_PackageId_ProcedureId" ON public."McuPackageComponent" ("PackageId", "ProcedureId") WHERE "ProcedureId" IS NOT NULL AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_McuPackageComponent_PackageId_ExaminationTypeId" ON public."McuPackageComponent" ("PackageId", "ExaminationTypeId") WHERE "ExaminationTypeId" IS NOT NULL AND "IsDelete" = false;

CREATE TABLE public."McuPackageComponentRequirement" (
    "Id" uuid NOT NULL, "PackageComponentId" uuid NOT NULL, "RequirementText" varchar(300) NOT NULL,
    "IsConfirmationRequired" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_McuPackageComponentRequirement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageComponentRequirement_McuPackageComponent" FOREIGN KEY ("PackageComponentId") REFERENCES public."McuPackageComponent" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuPackageCompanyPrice" (
    "Id" uuid NOT NULL, "PackageId" uuid NOT NULL, "CompanyGuarantorId" uuid NOT NULL, "TariffId" uuid NOT NULL,
    "EffectiveStartDate" date NOT NULL, "EffectiveEndDate" date,
    CONSTRAINT "PK_McuPackageCompanyPrice" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageCompanyPrice_McuPackage" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageCompanyPrice_MstCompanyGuarantor" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageCompanyPrice_MstTariff" FOREIGN KEY ("TariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_McuPackageCompanyPrice_PackageId_CompanyGuarantorId" ON public."McuPackageCompanyPrice" ("PackageId", "CompanyGuarantorId");

CREATE TABLE public."McuExaminationType" (
    "Id" uuid NOT NULL, "ExaminationTypeCode" varchar(30) NOT NULL, "ExaminationTypeName" varchar(150) NOT NULL,
    "Description" varchar(500), "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_McuExaminationType" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuExaminationType_ExaminationTypeCode" ON public."McuExaminationType" ("ExaminationTypeCode") WHERE "IsDelete" = false;

CREATE TABLE public."McuExaminationField" (
    "Id" uuid NOT NULL, "ExaminationTypeId" uuid NOT NULL, "FieldCode" varchar(50) NOT NULL, "FieldLabel" varchar(150) NOT NULL,
    "FieldType" integer NOT NULL, "UnitName" varchar(30), "OptionValues" varchar(1000),
    "IsRequired" boolean NOT NULL DEFAULT false, "FieldNumber" integer NOT NULL,
    CONSTRAINT "PK_McuExaminationField" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuExaminationField_McuExaminationType" FOREIGN KEY ("ExaminationTypeId") REFERENCES public."McuExaminationType" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_McuExaminationField_Type_FieldCode" ON public."McuExaminationField" ("ExaminationTypeId", "FieldCode") WHERE "IsDelete" = false;
CREATE UNIQUE INDEX "IX_McuExaminationField_Type_FieldNumber" ON public."McuExaminationField" ("ExaminationTypeId", "FieldNumber") WHERE "IsDelete" = false;

CREATE TABLE public."McuFitnessCategory" (
    "Id" uuid NOT NULL, "FitnessCategoryCode" varchar(30) NOT NULL, "FitnessCategoryName" varchar(100) NOT NULL,
    "Description" varchar(500), "IsActive" boolean NOT NULL DEFAULT true,
    CONSTRAINT "PK_McuFitnessCategory" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuFitnessCategory_Code" ON public."McuFitnessCategory" ("FitnessCategoryCode") WHERE "IsDelete" = false;

CREATE TABLE public."McuDailySchedule" (
    "Id" uuid NOT NULL, "ScheduleDate" date NOT NULL, "IsOpen" boolean NOT NULL DEFAULT true, "Quota" integer NOT NULL,
    "UsedQuota" integer NOT NULL DEFAULT 0, "ApprovedOverrideQuota" integer NOT NULL DEFAULT 0, "DutyMcuDoctorId" uuid,
    "Version" integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_McuDailySchedule" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuDailySchedule_MstDoctor_DutyMcuDoctorId" FOREIGN KEY ("DutyMcuDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_McuDailySchedule_ScheduleDate" ON public."McuDailySchedule" ("ScheduleDate") WHERE "IsDelete" = false;
CREATE INDEX "IX_McuDailySchedule_DutyMcuDoctorId" ON public."McuDailySchedule" ("DutyMcuDoctorId");

CREATE TABLE public."McuQuotaOverride" (
    "Id" uuid NOT NULL, "DailyScheduleId" uuid NOT NULL, "AdditionalQuota" integer NOT NULL, "Reason" varchar(500) NOT NULL,
    "ApprovedByUserId" uuid NOT NULL, "ApprovedAt" timestamp NOT NULL,
    CONSTRAINT "PK_McuQuotaOverride" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuQuotaOverride_McuDailySchedule" FOREIGN KEY ("DailyScheduleId") REFERENCES public."McuDailySchedule" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuBookingImport" (
    "Id" uuid NOT NULL, "ImportNumber" varchar(30) NOT NULL, "CompanyGuarantorId" uuid NOT NULL, "PackageId" uuid NOT NULL,
    "ExaminationDate" date NOT NULL, "FileName" varchar(255) NOT NULL, "TotalRows" integer NOT NULL, "CreatedRows" integer NOT NULL,
    "FailedRows" integer NOT NULL, "FailureSummary" varchar(4000), -- SENSITIF
    CONSTRAINT "PK_McuBookingImport" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuBookingImport_ImportNumber" ON public."McuBookingImport" ("ImportNumber");

CREATE TABLE public."McuOrder" (
    "Id" uuid NOT NULL, "OrderNumber" varchar(30) NOT NULL, "PackageId" uuid NOT NULL,
    "PackageCodeSnapshot" varchar(30) NOT NULL, "PackageNameSnapshot" varchar(150) NOT NULL,
    "PatientId" uuid,
    "ParticipantName" varchar(150) NOT NULL,     -- SENSITIF
    "ParticipantNik" varchar(16),                -- SENSITIF
    "ParticipantBirthDate" date,                 -- SENSITIF
    "ParticipantPhone" varchar(20),              -- SENSITIF
    "PatientMatchMethod" integer, "PatientMatchConfirmedByUserId" uuid,
    "Source" integer NOT NULL, "SourceEncounterId" uuid, "RecommendingDoctorId" uuid,
    "CompanyGuarantorId" uuid, "BookingImportId" uuid,
    "ExaminationDate" date NOT NULL, "DailyScheduleId" uuid NOT NULL, "OrderStatus" integer NOT NULL,
    "ArrivalConfirmedAt" timestamp, "EncounterId" uuid, "CheckInChannel" integer, "CheckedInAt" timestamp,
    "CheckedInByUserId" uuid, "McuDoctorId" uuid, "AppliedTariffId" uuid, "PackageInvoiceItemId" uuid,
    "ClearanceBasis" integer, "ExaminationStartedAt" timestamp, "NoShowAt" timestamp, "CancelReason" varchar(500),
    "Version" integer NOT NULL DEFAULT 0,
    CONSTRAINT "PK_McuOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrder_McuPackage" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstPatient" FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_RegPatientEncounter_EncounterId" FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_RegPatientEncounter_SourceEncounterId" FOREIGN KEY ("SourceEncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_McuDailySchedule" FOREIGN KEY ("DailyScheduleId") REFERENCES public."McuDailySchedule" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstCompanyGuarantor" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_McuBookingImport" FOREIGN KEY ("BookingImportId") REFERENCES public."McuBookingImport" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstDoctor_McuDoctorId" FOREIGN KEY ("McuDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstTariff_AppliedTariffId" FOREIGN KEY ("AppliedTariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_BilInvoiceItem" FOREIGN KEY ("PackageInvoiceItemId") REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT
    -- FK RecommendingDoctorId, CheckedInByUserId, PatientMatchConfirmedByUserId mengikuti pola yang sama
);
CREATE UNIQUE INDEX "IX_McuOrder_OrderNumber" ON public."McuOrder" ("OrderNumber");
CREATE UNIQUE INDEX "IX_McuOrder_EncounterId" ON public."McuOrder" ("EncounterId") WHERE "EncounterId" IS NOT NULL;
CREATE INDEX "IX_McuOrder_ExaminationDate_OrderStatus" ON public."McuOrder" ("ExaminationDate", "OrderStatus");
CREATE INDEX "IX_McuOrder_ParticipantNik" ON public."McuOrder" ("ParticipantNik");

CREATE TABLE public."McuOrderComponent" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "PackageComponentId" uuid, "ComponentType" integer NOT NULL,
    "ProcedureId" uuid, "RadModalityId" uuid, "ExaminationTypeId" uuid, "ComponentNameSnapshot" varchar(200) NOT NULL,
    "IsPackageCovered" boolean NOT NULL DEFAULT true, "RefundAmountSnapshot" numeric(18,2) NOT NULL DEFAULT 0,
    "MedicalFeeAmountSnapshot" numeric(18,2) NOT NULL DEFAULT 0, "ComponentStatus" integer NOT NULL, "ScheduledDate" date NOT NULL,
    "LabOrderId" uuid, "RadOrderId" uuid, "PerformingDoctorId" uuid, "CompletedAt" timestamp, "CancelReason" varchar(500),
    "CancelledByDoctorId" uuid, "RefundCaseId" uuid, "MedicalFeeReleasedAt" timestamp, "AddedByDoctorId" uuid,
    CONSTRAINT "PK_McuOrderComponent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrderComponent_McuOrder" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_LabOrder" FOREIGN KEY ("LabOrderId") REFERENCES public."LabOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_RadOrder" FOREIGN KEY ("RadOrderId") REFERENCES public."RadOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_BilRefundCase" FOREIGN KEY ("RefundCaseId") REFERENCES public."BilRefundCase" ("Id") ON DELETE RESTRICT
    -- FK PackageComponentId, ProcedureId, RadModalityId, ExaminationTypeId, dokter: pola yang sama, RESTRICT
);
CREATE UNIQUE INDEX "IX_McuOrderComponent_Order_Procedure" ON public."McuOrderComponent" ("McuOrderId", "ProcedureId") WHERE "ProcedureId" IS NOT NULL AND "ComponentStatus" <> 6 AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_McuOrderComponent_Order_ExaminationType" ON public."McuOrderComponent" ("McuOrderId", "ExaminationTypeId") WHERE "ExaminationTypeId" IS NOT NULL AND "ComponentStatus" <> 6 AND "IsDelete" = false;
CREATE INDEX "IX_McuOrderComponent_LabOrderId" ON public."McuOrderComponent" ("LabOrderId");
CREATE INDEX "IX_McuOrderComponent_RadOrderId" ON public."McuOrderComponent" ("RadOrderId");

CREATE TABLE public."McuOrderRequirementCheck" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "McuOrderComponentId" uuid NOT NULL,
    "RequirementTextSnapshot" varchar(300) NOT NULL, "Outcome" integer NOT NULL,
    "Note" varchar(500), -- SENSITIF
    "CheckedByUserId" uuid NOT NULL, "CheckedAt" timestamp NOT NULL,
    CONSTRAINT "PK_McuOrderRequirementCheck" PRIMARY KEY ("Id")
);

CREATE TABLE public."McuOrderStatusHistory" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "FromStatus" integer, "ToStatus" integer NOT NULL,
    "ChangedAt" timestamp NOT NULL, "ChangedByUserId" uuid, "Reason" varchar(500),
    CONSTRAINT "PK_McuOrderStatusHistory" PRIMARY KEY ("Id")
);
CREATE INDEX "IX_McuOrderStatusHistory_Order_ChangedAt" ON public."McuOrderStatusHistory" ("McuOrderId", "ChangedAt");

CREATE TABLE public."McuOrderDoctorAssignment" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "DoctorId" uuid NOT NULL, "AssignedAt" timestamp NOT NULL,
    "AssignedByUserId" uuid NOT NULL, "Reason" varchar(500), "EndedAt" timestamp,
    CONSTRAINT "PK_McuOrderDoctorAssignment" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuOrderDoctorAssignment_Active" ON public."McuOrderDoctorAssignment" ("McuOrderId") WHERE "EndedAt" IS NULL AND "IsDelete" = false;

CREATE TABLE public."McuExaminationResult" (
    "Id" uuid NOT NULL, "McuOrderComponentId" uuid NOT NULL,
    "ConclusionText" varchar(2000), -- SENSITIF
    "FilledByUserId" uuid NOT NULL, "FilledAt" timestamp NOT NULL,
    CONSTRAINT "PK_McuExaminationResult" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuExaminationResult_Component" ON public."McuExaminationResult" ("McuOrderComponentId") WHERE "IsDelete" = false;

CREATE TABLE public."McuExaminationResultValue" (
    "Id" uuid NOT NULL, "ExaminationResultId" uuid NOT NULL, "ExaminationFieldId" uuid NOT NULL,
    "ValueText" varchar(500), -- SENSITIF
    CONSTRAINT "PK_McuExaminationResultValue" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuExaminationResultValue_Result_Field" ON public."McuExaminationResultValue" ("ExaminationResultId", "ExaminationFieldId") WHERE "IsDelete" = false;

CREATE TABLE public."McuExaminationResultValueRevision" (
    "Id" uuid NOT NULL, "ResultValueId" uuid NOT NULL,
    "PreviousValueText" varchar(500), -- SENSITIF
    "ResumeVersionNumber" integer NOT NULL, "ChangedByUserId" uuid NOT NULL, "ChangedAt" timestamp NOT NULL,
    CONSTRAINT "PK_McuExaminationResultValueRevision" PRIMARY KEY ("Id")
);

CREATE TABLE public."McuResume" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "VersionNumber" integer NOT NULL, "ResumeStatus" integer NOT NULL,
    "IsCurrent" boolean NOT NULL DEFAULT true,
    "GeneralConditionText" varchar(4000),   -- SENSITIF
    "ImportantFindingsText" varchar(4000),  -- SENSITIF
    "RecommendationText" varchar(4000),     -- SENSITIF
    "FitnessCategoryId" uuid,
    "WorkRecommendationText" varchar(1000), -- SENSITIF
    "AuthorDoctorId" uuid, "ApprovedByDoctorId" uuid, "ApprovedAt" timestamp, "ReturnReason" varchar(500),
    "VerifiedByUserId" uuid, "VerifiedAt" timestamp, "ReopenReason" varchar(500), "ReopenedByDoctorId" uuid, "ReopenedAt" timestamp,
    CONSTRAINT "PK_McuResume" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuResume_Order_Version" ON public."McuResume" ("McuOrderId", "VersionNumber");
CREATE UNIQUE INDEX "IX_McuResume_Order_Current" ON public."McuResume" ("McuOrderId") WHERE "IsCurrent" = true AND "IsDelete" = false;

CREATE TABLE public."McuReleaseConsent" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "CompanyGuarantorId" uuid NOT NULL, "ConsentGivenAt" timestamp NOT NULL,
    "RecordedByUserId" uuid NOT NULL, "EvidenceType" integer NOT NULL, "EvidenceReference" varchar(200),
    "RevokedAt" timestamp, "RevokedByUserId" uuid, "RevokeReason" varchar(500),
    CONSTRAINT "PK_McuReleaseConsent" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_McuReleaseConsent_Active" ON public."McuReleaseConsent" ("McuOrderId") WHERE "RevokedAt" IS NULL AND "IsDelete" = false;

CREATE TABLE public."McuResultDelivery" (
    "Id" uuid NOT NULL, "McuOrderId" uuid NOT NULL, "McuResumeId" uuid NOT NULL, "RecipientType" integer NOT NULL,
    "ContentType" integer NOT NULL, "Channel" integer NOT NULL,
    "DestinationText" varchar(200), -- SENSITIF
    "DeliveredAt" timestamp NOT NULL, "DeliveredByUserId" uuid NOT NULL, "Note" varchar(500),
    "RequiresCorrectionNotice" boolean NOT NULL DEFAULT false, "CorrectionNoticeRecordedAt" timestamp,
    CONSTRAINT "PK_McuResultDelivery" PRIMARY KEY ("Id")
);
CREATE INDEX "IX_McuResultDelivery_Order" ON public."McuResultDelivery" ("McuOrderId");

-- Diperbarui (milik Laboratorium dan Radiologi)
ALTER TABLE public."LabOrder" ADD "CoveringMcuOrderId" uuid;
CREATE INDEX "IX_LabOrder_CoveringMcuOrderId" ON public."LabOrder" ("CoveringMcuOrderId");
ALTER TABLE public."LabOrder" ADD CONSTRAINT "FK_LabOrder_McuOrder_CoveringMcuOrderId"
    FOREIGN KEY ("CoveringMcuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT;
ALTER TABLE public."RadOrder" ADD "CoveringMcuOrderId" uuid;
CREATE INDEX "IX_RadOrder_CoveringMcuOrderId" ON public."RadOrder" ("CoveringMcuOrderId");
ALTER TABLE public."RadOrder" ADD CONSTRAINT "FK_RadOrder_McuOrder_CoveringMcuOrderId"
    FOREIGN KEY ("CoveringMcuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT;
```
