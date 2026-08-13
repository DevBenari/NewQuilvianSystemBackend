# Kamus Data — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Commit diaudit | backend `e5331a0` |
| Aturan kedalaman | `DEC-RSK-002` — bertingkat mengikuti status tabel |

## Cara membaca dokumen ini

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

Kedalaman dokumentasi mengikuti status tabel:

| Status tabel | Yang didokumentasikan |
| --- | --- |
| `Baru` dan `Diperbarui` | Seluruh kolom |
| `Sudah ada` | Kolom kunci saja — PK, FK, status, dan kolom yang dipakai aturan bisnis modul ini — ditambah rujukan ke file model |

Kolom bertanda **Sensitif** tidak boleh masuk custom logger dan tidak boleh dipakai sebagai
contoh berisi data asli pasien.

Seluruh FK memakai `DeleteBehavior.Restrict`.

---

## 1. `TrxEmergencyTriage` — status `Diperbarui`, seluruh kolom

Sumber: `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | Kunci utama |
| `EmergencyVisitId` | `Guid` | Ya | — | Index | `TrxEmergencyVisit` | Tidak | Kunjungan IGD induk |
| `TriageLevelId` | `Guid` | Ya | — | Index | `MstEmergencyTriageLevel` | Tidak | Level triage yang ditetapkan |
| `PatientVitalSignId` | `Guid?` | Tidak | — | — | `TrxPatientVitalSign` | Tidak | Tanda vital saat triage, milik Clinical Management |
| `Sequence` | `int` | Ya | `1` | — | — | Tidak | Urutan penilaian dalam satu kunjungan |
| `IsRetriage` | `bool` | Ya | `false` | — | — | Tidak | Menandai penilaian ulang |
| `PreviousTriageId` | `Guid?` | Tidak | — | Index | `TrxEmergencyTriage` | Tidak | Penilaian yang digantikan |
| `TriageSystem` | `EmergencyTriageSystem` | Ya | `ATS` | — | — | Tidak | `ATS` atau `ESI`, keduanya skala lima level |
| `TriageStatus` | `EmergencyTriageStatus` | Ya | `Draft` | Index | — | Tidak | `Draft`, `InProgress`, `Completed`, `Superseded`, `Cancelled` |
| `StartedAt` | `DateTime` | Ya | `UtcNow` | Index | — | Tidak | Waktu penilaian dimulai |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Waktu penilaian selesai |
| `MaxWaitingMinutesSnapshot` | `int` | Ya | — | — | — | Tidak | Salinan target dari master saat penilaian dibuat |
| `ResponseDueAt` | `DateTime?` | Tidak | — | Index | — | Tidak | Batas waktu respons, dihitung server dari `StartedAt` + target master |
| `ImmediateCareAllowed` | `bool` | Ya | `false` | — | — | Tidak | Pelayanan boleh dimulai tanpa menunggu administrasi |
| `TriageReason` | `string(1000)` | Tidak | — | — | — | **Ya** | Alasan klinis penetapan level |
| `AirwaySummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan jalan napas |
| `BreathingSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan pernapasan |
| `CirculationSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan sirkulasi |
| `DisabilitySummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan kesadaran dan neurologis |
| `ExposureSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Ringkasan pemeriksaan paparan |
| `RedFlagSummary` | `string(1000)` | Tidak | — | — | — | **Ya** | Tanda bahaya yang ditemukan |
| `PerformedByUserId` | `Guid` | Ya | — | Index | `ApplicationUser` | Tidak | Petugas yang menilai |
| `ReviewedByUserId` | `Guid?` | Tidak | — | — | `ApplicationUser` | Tidak | Peninjau penilaian |
| `ReviewedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Waktu peninjauan |
| `Notes` | `string(1000)` | Tidak | — | — | — | **Ya** | Catatan tambahan |
| `IsActive` | `bool` | Ya | `true` | — | — | Tidak | Penanda aktif |
| **`IsSlaBreached`** | **`bool`** | **Ya** | **`false`** | **Index** | — | **Tidak** | **Baru — target respons terlampaui** |
| **`SlaBreachedAt`** | **`DateTime?`** | **Tidak** | — | — | — | **Tidak** | **Baru — waktu pelampauan tercatat** |

Index gabungan baru: `(EmergencyVisitId, ResponseDueAt, IsSlaBreached)` untuk pencarian
pasien yang melewati batas waktu tunggu.

Delapan kolom ringkasan klinis bertanda sensitif seluruhnya berukuran maksimal 1000 karakter.

---

## 2. Tabel berstatus `Sudah ada` — kolom kunci

Untuk kolom lengkap, buka file model yang dirujuk.

### `TrxEmergencyVisit`

Sumber: `.../EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs`

| Kolom | Tipe | Wajib | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | Kunci utama |
| `EmergencyVisitNumber` | `string` | Ya | Unique | Tidak | Nomor kunjungan, dibentuk `EmergencyDocumentNumberService` |
| `EncounterId` | `Guid?` | Tidak | `TrxPatientEncounter`, unique | Tidak | Penghubung ke seluruh data klinis |
| `PatientId` | `Guid?` | Tidak | Patient | Tidak | Kosong bila pasien belum teridentifikasi |
| `ServiceUnitId` | `Guid` | Ya | Master unit | Tidak | Menentukan konteks IGD |
| `ArrivalModeId` | `Guid?` | Tidak | `MstEmergencyArrivalMode` | Tidak | Cara pasien datang |
| `CaseTypeId` | `Guid?` | Tidak | `MstEmergencyCaseType` | Tidak | Klasifikasi kasus |
| `IsUnknownPatient` | `bool` | Ya | — | Tidak | Pasien tak dikenal, jalur keselamatan |
| `IsImmediateCareAllowed` | `bool` | Ya | — | Tidak | Pelayanan boleh mendahului administrasi |
| `RegistrationStatus` | `EmergencyRegistrationStatus` | Ya | Index | Tidak | `Pending`, `Provisional`, `Registered`, `Completed`, `Cancelled` |
| `VisitStatus` | `EmergencyVisitStatus` | Ya | Index | Tidak | Lihat catatan perubahan enum di bawah |
| `VisitCompletedAt` | `DateTime?` | Tidak | — | Tidak | Terisi saat seluruh kewajiban klinis tuntas |
| `ChiefComplaint` | `string` | Tidak | — | **Ya** | Keluhan utama pasien |

### `TrxEmergencyTriageDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyTriageId`, `TriageIndicatorId` | Kunci dan relasi |
| `IsMatched`, `Sequence` | Indikator cocok atau tidak, beserta urutannya |
| Kolom snapshot master | Menjaga histori tetap utuh saat master indikator berubah |

### `TrxEmergencyResuscitation`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `ResuscitationNumber` | Unique |
| `ResuscitationStatus` | `Planned`, `InProgress`, `Completed`, `Stopped`, `Cancelled` |
| `TeamLeaderDoctorId`, `RecordedByUserId` | Penanggung jawab |

### `TrxEmergencyObservation`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `ObservationNumber` | Unique |
| `ObservationStatus` | `Active`, `Completed`, `Escalated`, `Cancelled` |
| `ResponsibleDoctorId`, `ResponsibleNurseUserId` | Penanggung jawab |

### `TrxEmergencyObservationDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyObservationId` | Kunci dan induk |
| `PatientVitalSignId`, `ProgressNoteId` | Rujukan ke Clinical Management, bukan salinan |
| `RecordedByUserId` | Pencatat |
| Kolom kondisi klinis dan intervensi | **Sensitif** |

### `TrxEmergencyProcedureDetail`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId`, `PatientProcedureId` | `PatientProcedureId` unique, satu banding satu |
| `EmergencyResuscitationId`, `EmergencyObservationId` | Konteks opsional |
| `DetailType` | `General`, `SkinTest`, `TetanusToxoid`, `AntiTetanusSerum`, `EmergencyMedication`, `Resuscitation`, `Other` |

### `TrxEmergencyDisposition`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId`, `DispositionTypeId` | Kunci dan relasi |
| `DispositionStatus` | `Draft`, `Confirmed`, `Executed`, `Cancelled` |
| `DecidedByDoctorId`, `ConfirmedByUserId` | Pemutus dan pengesah |
| `DestinationServiceUnitId`, `ReferralNumber` | Tujuan internal atau rujukan keluar |
| `IsPatientDeceased`, `IsVisumRequested` | Penanda khusus |
| Kolom kondisi saat keluar dan instruksi | **Sensitif** |

### `TrxEmergencyTransfer`

| Kolom kunci | Keterangan |
| --- | --- |
| `Id`, `EmergencyVisitId` | Kunci dan induk |
| `TransferNumber` | Unique |
| `FromServiceUnitId`, `ToServiceUnitId`, `FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId` | Asal dan tujuan |
| `TransferStatus` | `Requested`, `Accepted`, `InTransit`, `Completed`, `Rejected`, `Cancelled` |
| `RequestedByUserId`, `AcceptedByUserId`, `SendingNurseUserId`, `ReceivingNurseUserId` | Pihak yang terlibat |

---

## 3. Master — kolom kunci

| Tabel | Kolom kunci | Catatan |
| --- | --- | --- |
| `MstEmergencyTriageLevel` | `Id`, `Level`, `Code`, `Name`, `ColorName`, `ColorHex`, `MaxWaitingMinutes`, `Sequence`, `IsActive` | Warna dan target waktu wajib dari sini, tidak boleh di-hardcode |
| `MstEmergencyTriageIndicator` | `Id`, `TriageLevelId`, `Code`, `Name`, `IndicatorGroup`, `Sequence`, `IsActive` | Checklist saat triage |
| `MstEmergencyArrivalMode` | `Id`, `Code`, `Name`, `IsAmbulance`, `IsReferral`, `Sequence`, `IsActive` | Dasar pelaporan rujukan dan ambulans |
| `MstEmergencyCaseType` | `Id`, `Code`, `Name`, `Sequence`, `IsActive` | Klasifikasi kasus |
| `MstEmergencyDispositionType` | `Id`, `Code`, `Name`, `Sequence`, `IsActive` | Menentukan kebutuhan unit tujuan atau rujukan |
| `MstEmergencySetting` | `Id`, `Code`, `Name`, `DefaultEmergencyServiceUnitId`, `IsDefault`, `IsActive` | Hanya satu baris boleh `IsDefault` |

Seluruh master memakai `Code` unique.

---

## 4. Perubahan enum

| Enum | Status | Perubahan |
| --- | --- | --- |
| `EmergencyVisitStatus` | **Diperbarui** | Tambah `Completed = 9` setelah `Disposed = 7` dan `Cancelled = 8`. Nilai lama tidak digeser agar data tersimpan tetap sahih |

Nilai lengkap setelah perubahan: `Arrived`, `WaitingForTriage`, `Triaged`, `InTreatment`,
`UnderObservation`, `AwaitingDisposition`, `Disposed`, `Cancelled`, `Completed`.

---

## 5. Ringkasan kolom sensitif

Kolom berikut memuat informasi klinis pasien. Seluruhnya dilarang masuk custom logger dan
dilarang dipakai sebagai contoh berisi data asli.

| Tabel | Kolom |
| --- | --- |
| `TrxEmergencyVisit` | `ChiefComplaint` |
| `TrxEmergencyTriage` | `TriageReason`, `AirwaySummary`, `BreathingSummary`, `CirculationSummary`, `DisabilitySummary`, `ExposureSummary`, `RedFlagSummary`, `Notes` |
| `TrxEmergencyObservationDetail` | Kondisi klinis, intervensi, dan respons pasien |
| `TrxEmergencyDisposition` | Kondisi saat keluar, instruksi tindak lanjut, alasan penolakan |
| `TrxEmergencyResuscitation` | Pemicu dan hasil resusitasi |

Payload log hanya boleh memuat `EntityId`, nama controller, nama action, dan status.
