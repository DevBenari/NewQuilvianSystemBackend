# Hemodialisa — Kamus Data

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `draft` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | — / — |
| `input_revision` | `02-backend-architecture.md` r1 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah
maupun pada bentuk DDL.

Penghapusan bersifat **penandaan** melalui `IsDelete`, bukan penghapusan baris. Desain mana pun
tidak boleh mengandalkan baris benar-benar hilang dari tabel.

Seluruh nama tabel tunggal dan PascalCase, pada schema `public`. Seluruh enum disimpan sebagai
integer.

---

## 1. Status dan Kepemilikan Tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstPatient` | Sudah ada | Patient Management | Dirujuk, **tidak boleh** disalin |
| `RegPatientEncounter` | Sudah ada | Registration Management | Dirujuk, **tidak boleh** disalin |
| `InpEpisode` | Sudah ada | Inpatient Management | Dirujuk, boleh kosong |
| `MstDoctor` | Sudah ada | Human Resource | Dirujuk |
| `MstWorkforceProfile` | Sudah ada | Human Resource | Dirujuk |
| `MstServiceUnit` | Sudah ada | Master Data | Dirujuk; enum jenisnya bertambah satu nilai |
| `MstRoom` | Sudah ada | Master Data | Dirujuk |
| `MstProcedure` | Sudah ada | Master Data | Dirujuk; tindakan HD ditambahkan sebagai **data** |
| `MstDrug` | Sudah ada | Farmasi | Dirujuk |
| `TrxPatientProcedure` | Sudah ada | Clinical Management | Ditulis satu baris per sesi |
| `TrxPatientVitalSign` | Sudah ada | Clinical Management | Ditulis untuk tanda vital klinis |
| `TrxPatientConsent` | Sudah ada | Clinical Management | Hanya dibaca |
| `LabExamination` | Sudah ada | Laboratorium | Hanya dirujuk |
| `PhmDrugUsage` | Sudah ada | Farmasi | Ditulis lewat penerusan |
| `MrcClinicalDocumentIntegrity` | Sudah ada | Rekam Medis | Ditulis lewat layanan Rekam Medis |
| `MrcClinicalNoteAddendum` | Sudah ada | Rekam Medis | Ditulis lewat layanan Rekam Medis |
| `HmdOrder` | **Baru** | Hemodialisa | — |
| `HmdEpisode` | **Baru** | Hemodialisa | — |
| `HmdEligibilityAssessment` | **Baru** | Hemodialisa | — |
| `HmdVascularAccess` | **Baru** | Hemodialisa | — |
| `HmdSerologyReview` | **Baru** | Hemodialisa | — |
| `HmdIsolationDecision` | **Baru** | Hemodialisa | — |
| `HmdPrescription` | **Baru** | Hemodialisa | — |
| `HmdSession` | **Baru** | Hemodialisa | — |
| `HmdSessionChecklist` | **Baru** | Hemodialisa | — |
| `HmdSessionAssessment` | **Baru** | Hemodialisa | — |
| `HmdSessionObservation` | **Baru** | Hemodialisa | — |
| `HmdSessionMedication` | **Baru** | Hemodialisa | — |
| `HmdSessionComplication` | **Baru** | Hemodialisa | — |
| `HmdSessionStaffAssignment` | **Baru** | Hemodialisa | — |
| `HmdMachine` | **Baru** | Hemodialisa | Master milik modul, prefix `Hmd` bukan `Mst` |
| `HmdMachineStatusHistory` | **Baru** | Hemodialisa | — |
| `HmdStation` | **Baru** | Hemodialisa | Master milik modul |
| `HmdChecklistItem` | **Baru** | Hemodialisa | Master milik modul |
| `HmdReadinessItem` | **Baru** | Hemodialisa | Master milik modul |
| `HmdUnitReadiness` | **Baru** | Hemodialisa | — |
| `HmdUnitReadinessDetail` | **Baru** | Hemodialisa | — |
| `HmdSetting` | **Baru** | Hemodialisa | Master milik modul |

---

## 2. Kolom Kunci Tabel `Sudah Ada`

Hanya kolom yang dipakai aturan bisnis Hemodialisa. Sumber lengkapnya ada di file model.

### `RegPatientEncounter`

Model: `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`

| Kolom | Dipakai Hemodialisa untuk |
|---|---|
| `Id` | Dirujuk `HmdOrder.EncounterId` dan `HmdSession.EncounterId` |
| `PatientId` | Memastikan kunjungan memang milik pasien yang bersangkutan |
| `EncounterType` | Menentukan konteks: rawat jalan, rawat inap, atau gawat darurat |
| `EncounterStatus` | Memeriksa kunjungan masih sah sebelum sesi dimulai |

### `TrxPatientProcedure`

Model: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs`

| Kolom | Dipakai Hemodialisa untuk |
|---|---|
| `Id` | Dirujuk `HmdSession.PatientProcedureId` |
| `EncounterId` | Wajib; diisi dari sesi |
| `DoctorId` | **Wajib**; diisi dokter penanggung jawab sesi (`HMD-DEC-009`) |
| `InstructingDoctorId` | Diisi dokter pembuat resep (`HMD-DEC-009`) |
| `ProcedureStatus` | `InProgress` saat sesi dimulai, `Completed` saat difinalisasi |
| `IsBillable` | `false` bila sesi berakhir dihentikan (`HMD-DEC-012`) |
| `Quantity`, `UnitNameSnapshot` | Dibawa ke fakta tagihan |

### `MstServiceUnit`

Model: `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs`

| Kolom | Dipakai Hemodialisa untuk |
|---|---|
| `Id` | Dirujuk seluruh sumber daya dan pengaturan HD |
| `ServiceUnitType` | Bertambah nilai `Hemodialysis = 10` |

---

## 3. Tabel Baru

### 3.1 `HmdOrder` — permintaan HD masuk

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderNumber` | `string(30)` | Ya | — | Unique | — | — | Tidak | Nomor permintaan dari penyedia nomor seri |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | Pasien yang diminta dicuci darah |
| `EncounterId` | `Guid` | Ya | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | Konteks kunjungan; mengikuti bentuk `LabOrder` |
| `InpEpisodeId` | `Guid?` | Tidak | — | Index | FK `InpEpisode` | `Restrict` | Tidak | Terisi bila peminta dari bangsal |
| `EpisodeId` | `Guid?` | Tidak | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Terisi bila pasien sudah punya program HD |
| `RequestedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Pengguna yang membuat permintaan |
| `RequestingDoctorId` | `Guid?` | Tidak | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter yang meminta, bila pembuatnya perawat |
| `RequestedAt` | `DateTime` | Ya | waktu server | Index | — | — | Tidak | Waktu permintaan dibuat |
| `Priority` | `int` | Ya | `Routine` | Index | — | — | Tidak | `Routine` atau `Cito` |
| `ClinicalReason` | `string(1000)` | Ya | — | — | — | — | **Ya** | Indikasi klinis permintaan |
| `RequestedDate` | `DateOnly?` | Tidak | — | Index | — | — | Tidak | Tanggal yang diharapkan peminta |
| `OrderStatus` | `int` | Ya | `Requested` | Index | — | — | Tidak | Lihat matriks status |
| `StatusBeforeHold` | `int?` | Tidak | — | — | — | — | Tidak | Status sebelum ditahan; mengikuti pola `RadOrder` |
| `DecisionByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengguna yang menerima, menahan, menolak, atau membatalkan |
| `DecisionAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `DecisionReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Alasan penahanan, penolakan, atau pembatalan |

Index gabungan: `(PatientId, OrderStatus)` untuk daftar kerja, dan `(RequestedDate, Priority)`
untuk antrean harian.

### 3.2 `HmdEpisode` — program HD

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeNumber` | `string(30)` | Ya | — | Unique | — | — | Tidak | Nomor episode dari penyedia nomor seri |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | Pasien pemilik program |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | Unit HD yang melayani |
| `DpjpDoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter penanggung jawab program |
| `StartDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal program dimulai |
| `EndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal program berakhir |
| `EpisodeStatus` | `int` | Ya | `Draft` | Index | — | — | Tidak | Lihat matriks status |
| `SuspendReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan penangguhan |
| `ClosureReason` | `int?` | Tidak | — | — | — | — | Tidak | Selesai, pindah, berhenti, meninggal, atau lainnya |
| `ClosureNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Keterangan penutupan |

**Unique index bersyarat** `(PatientId)` untuk baris berstatus `Active` dan `IsDelete = false`.
Inilah yang menegakkan aturan satu episode aktif per pasien di tingkat basis data, bukan hanya di
service. Ketika `HmdSetting.AllowMultipleActiveEpisodePerPatient` dinyalakan, index ini tetap
ada; yang berubah adalah service berhenti memaksakannya — sehingga pelonggaran memerlukan satu
migration yang menghapus index itu. Keadaan ini dicatat sebagai konsekuensi yang diketahui dari
`RCG-M-05`.

### 3.3 `HmdEligibilityAssessment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `AssessedByDoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter penilai |
| `AssessedAt` | `DateTime` | Ya | waktu server | Index | — | — | Tidak | Waktu penilaian |
| `Outcome` | `int` | Ya | — | Index | — | — | Tidak | `Eligible`, `Deferred`, `Modified`, `Referred` |
| `IndicationSummary` | `string(1000)` | Ya | — | — | — | — | **Ya** | Indikasi klinis |
| `DecisionReason` | `string(1000)` | Ya | — | — | — | — | **Ya** | Alasan keputusan |
| `FollowUpInstruction` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Tindak lanjut bila bukan layak |

Sistem **tidak pernah** menentukan kelayakan sendiri. Baris ini selalu lahir dari keputusan
manusia.

### 3.4 `HmdVascularAccess`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `AccessType` | `int` | Ya | — | Index | — | — | Tidak | Fistula, graft, kateter, atau lainnya |
| `AccessSite` | `string(200)` | Ya | — | — | — | — | Tidak | Lokasi akses, misalnya lengan kiri |
| `AccessStatus` | `int` | Ya | `Usable` | Index | — | — | Tidak | `Usable`, `NeedsAttention`, `NotUsable` |
| `IsPrimary` | `bool` | Ya | `false` | Index | — | — | Tidak | Akses utama yang dipakai sehari-hari |
| `EstablishedDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal akses dibuat |
| `LastAssessedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Terakhir dinilai |
| `ConditionNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Kondisi akses |

### 3.5 `HmdSerologyReview`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `TestType` | `int` | Ya | — | Index | — | — | Tidak | HBsAg, anti-HBs, anti-HCV, anti-HIV |
| `LabExaminationId` | `Guid?` | Tidak | — | Index | — | — | Tidak | **Rujukan**, bukan salinan hasil. Tanpa FK karena milik modul lain |
| `ResultDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal hasil keluar |
| `ResultFlag` | `int` | Ya | — | Index | — | — | **Ya** | `Reactive`, `NonReactive`, `Indeterminate` |
| `ResultSummary` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Ringkasan hasil sebagaimana dibaca petugas |
| `ReviewStatus` | `int` | Ya | `PendingReview` | Index | — | — | Tidak | `PendingReview`, `Reviewed` |
| `ReviewedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Peninjau |
| `ReviewedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu tinjauan |
| `ReviewNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan tinjauan |

`LabExaminationId` sengaja **tanpa** foreign key. Hasil laboratorium milik modul lain, dan
memasang FK lintas modul membuat Hemodialisa ikut terkunci pada perubahan skema Laboratorium.

### 3.6 `HmdIsolationDecision`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `Requirement` | `int` | Ya | `None` | Index | — | — | **Ya** | `None`, `HepatitisB`, `Other` |
| `EffectiveFrom` | `DateOnly` | Ya | — | Index | — | — | Tidak | Berlaku sejak |
| `EffectiveTo` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Berlaku sampai |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Keputusan yang sedang berlaku |
| `DecidedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Pengambil keputusan |
| `DecidedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu keputusan |
| `Reason` | `string(1000)` | Ya | — | — | — | — | **Ya** | Dasar keputusan |
| `SerologyReviewId` | `Guid?` | Tidak | — | Index | FK `HmdSerologyReview` | `Restrict` | Tidak | Hasil yang mendasari, bila ada |

Aturan Hepatitis B **tidak** otomatis diberlakukan untuk Hepatitis C dan HIV. Ketiganya
keputusan terpisah, dan sistem tidak pernah menyimpulkannya sendiri dari hasil laboratorium.

### 3.7 `HmdPrescription`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `PrescribingDoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter pemberi instruksi |
| `EffectiveDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Berlaku sejak |
| `FrequencyPerWeek` | `int` | Ya | — | — | — | — | Tidak | Berapa kali seminggu |
| `TargetDurationMinutes` | `int` | Ya | — | — | — | — | Tidak | Target durasi satu sesi |
| `TargetUltrafiltrationMl` | `int?` | Tidak | — | — | — | — | Tidak | Target penarikan cairan |
| `BloodFlowRate` | `int?` | Tidak | — | — | — | — | Tidak | Target kecepatan aliran darah, ml/menit |
| `DialysateFlowRate` | `int?` | Tidak | — | — | — | — | Tidak | Target kecepatan dialisat, ml/menit |
| `DialyzerType` | `string(100)?` | Tidak | — | — | — | — | Tidak | Jenis dializer yang diinstruksikan |
| `AnticoagulantPlan` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Rencana antikoagulasi |
| `VascularAccessId` | `Guid?` | Tidak | — | Index | FK `HmdVascularAccess` | `Restrict` | Tidak | Akses yang diinstruksikan |
| `ClinicalNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan klinis |
| `PrescriptionStatus` | `int` | Ya | `Draft` | Index | — | — | Tidak | Lihat matriks status |
| `SupersededByPrescriptionId` | `Guid?` | Tidak | — | Index | FK `HmdPrescription` | `Restrict` | Tidak | Resep pengganti |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan pembatalan |

**Unique index bersyarat** `(EpisodeId)` untuk baris berstatus `Active` dan `IsDelete = false`.

### 3.8 `HmdSession`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionNumber` | `string(30)` | Ya | — | Unique | — | — | Tidak | Nomor sesi dari penyedia nomor seri |
| `EpisodeId` | `Guid` | Ya | — | Index | FK `HmdEpisode` | `Restrict` | Tidak | Induk |
| `PrescriptionId` | `Guid` | Ya | — | Index | FK `HmdPrescription` | `Restrict` | Tidak | Resep yang dijalankan |
| `EncounterId` | `Guid?` | Tidak | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | Boleh kosong saat baru dijadwalkan; **wajib** sebelum dimulai |
| `InpEpisodeId` | `Guid?` | Tidak | — | Index | FK `InpEpisode` | `Restrict` | Tidak | Bila pasien sedang dirawat inap |
| `OrderId` | `Guid?` | Tidak | — | Index | FK `HmdOrder` | `Restrict` | Tidak | Permintaan yang melahirkan sesi ini |
| `MachineId` | `Guid` | Ya | — | Index | FK `HmdMachine` | `Restrict` | Tidak | Mesin yang dipakai |
| `StationId` | `Guid` | Ya | — | Index | FK `HmdStation` | `Restrict` | Tidak | Station yang dipakai |
| `ResponsibleDoctorId` | `Guid?` | Tidak | — | Index | FK `MstDoctor` | `Restrict` | Tidak | **Wajib terisi sebelum sesi dimulai** (`HMD-DEC-009`) |
| `ScheduledDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Tanggal jadwal |
| `Shift` | `int` | Ya | — | Index | — | — | Tidak | Shift pelaksanaan |
| `ScheduledStartAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Jam mulai terjadwal |
| `ScheduledEndAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Jam selesai terjadwal |
| `CheckedInAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pasien datang |
| `StartedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | **Waktu server**, bukan waktu perangkat pengguna |
| `StartedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengguna yang memulai |
| `EndedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu cuci darah berakhir |
| `ActualDurationMinutes` | `int?` | Tidak | — | — | — | — | Tidak | Durasi sebenarnya |
| `ActualUltrafiltrationMl` | `int?` | Tidak | — | — | — | — | Tidak | Cairan yang benar-benar ditarik |
| `SessionStatus` | `int` | Ya | `Planned` | Index | — | — | Tidak | Lihat matriks status |
| `StopReason` | `int?` | Tidak | — | Index | — | — | Tidak | Sebab penghentian |
| `StopNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Keterangan penghentian |
| `DeviationNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Penyimpangan dari resep beserta alasannya |
| `Disposition` | `int?` | Tidak | — | — | — | — | Tidak | Tujuan pasien setelah sesi |
| `DispositionNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Instruksi tindak lanjut |
| `DocumentedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **Syarat 2** — penyelesai dokumentasi |
| `DocumentedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu dokumentasi diselesaikan |
| `SignedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **Syarat 2** — pengesah |
| `SignedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pengesahan |
| `PatientProcedureId` | `Guid?` | Tidak | — | Unique | FK `TrxPatientProcedure` | `Restrict` | Tidak | Tindakan yang dibuat; unik agar satu sesi tidak pernah punya dua tindakan |
| `IdempotencyKey` | `string(100)?` | Tidak | — | Unique | — | — | Tidak | Mencegah tombol Mulai yang ditekan dua kali |
| `BillingHandoffStatus` | `int` | Ya | `NotRequired` | Index | — | — | Tidak | `NotRequired`, `Pending`, `Succeeded`, `Failed` |
| `BillingHandoffAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu penyerahan berhasil |
| `BillingHandoffError` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Sebab kegagalan terakhir |

Index gabungan untuk pencegahan tabrakan: `(MachineId, ScheduledStartAt, ScheduledEndAt)`,
`(StationId, ScheduledStartAt, ScheduledEndAt)`, dan `(EpisodeId, ScheduledStartAt)`.

Ketiganya **index biasa, bukan unique**, karena tumpang tindih waktu tidak dapat dinyatakan
sebagai keunikan sederhana. Pemeriksaannya dijalankan service di dalam transaksi, dan index ini
membuat pemeriksaan itu cepat.

### 3.9 `HmdSessionChecklist`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `ChecklistItemId` | `Guid` | Ya | — | Index | FK `HmdChecklistItem` | `Restrict` | Tidak | Butir yang diperiksa |
| `Result` | `int` | Ya | `NotChecked` | Index | — | — | Tidak | `NotChecked`, `Met`, `NotMet`, `NotApplicable` |
| `VerifiedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pemeriksa |
| `VerifiedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemeriksaan |
| `Note` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan |
| `IsOverridden` | `bool` | Ya | `false` | Index | — | — | Tidak | Butir dilewati |
| `OverrideReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Alasan pelewatan; wajib bila dilewati |
| `OverriddenByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Dokter yang melewati |
| `OverriddenAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pelewatan |

**Unique index** `(SessionId, ChecklistItemId)` — satu butir hanya punya satu hasil per sesi.

### 3.10 `HmdSessionAssessment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `Phase` | `int` | Ya | — | Index | — | — | Tidak | `Pre` atau `Post` |
| `AssessedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu penilaian |
| `AssessedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Penilai |
| `BodyWeightKg` | `decimal(5,2)?` | Tidak | — | — | — | — | Tidak | Berat badan pada fase ini |
| `PatientVitalSignId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Rujukan ke tanda vital milik Clinical Management |
| `Complaint` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Keluhan pasien |
| `AccessConditionNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Kondisi akses vaskular |
| `PatientCondition` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Kondisi umum pasien |
| `TargetAchieved` | `bool?` | Tidak | — | — | — | — | Tidak | Hanya pada fase pasca |

**Unique index** `(SessionId, Phase)` — satu sesi punya satu penilaian pra dan satu pasca.

Nilai pasca-HD **tidak boleh** disalin otomatis dari pra-HD. Berat badan setelah tindakan adalah
angka yang menentukan apakah target penarikan cairan tercapai; menyalinnya membuat angka itu
kehilangan arti.

### 3.11 `HmdSessionObservation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `SequenceNumber` | `int` | Ya | — | Index | — | — | Tidak | Urutan pengamatan dalam sesi |
| `ObservedAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Waktu pengamatan |
| `RecordedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Pencatat |
| `SystolicBp` | `int?` | Tidak | — | — | — | — | Tidak | Tekanan darah sistolik |
| `DiastolicBp` | `int?` | Tidak | — | — | — | — | Tidak | Tekanan darah diastolik |
| `PulseRate` | `int?` | Tidak | — | — | — | — | Tidak | Nadi per menit |
| `RespiratoryRate` | `int?` | Tidak | — | — | — | — | Tidak | Napas per menit |
| `TemperatureC` | `decimal(4,1)?` | Tidak | — | — | — | — | Tidak | Suhu |
| `OxygenSaturation` | `int?` | Tidak | — | — | — | — | Tidak | Saturasi oksigen |
| `BloodFlowRate` | `int?` | Tidak | — | — | — | — | Tidak | Parameter mesin, ml/menit |
| `DialysateFlowRate` | `int?` | Tidak | — | — | — | — | Tidak | Parameter mesin, ml/menit |
| `TransmembranePressure` | `int?` | Tidak | — | — | — | — | Tidak | Parameter mesin |
| `VenousPressure` | `int?` | Tidak | — | — | — | — | Tidak | Parameter mesin |
| `ArterialPressure` | `int?` | Tidak | — | — | — | — | Tidak | Parameter mesin |
| `UltrafiltrationVolumeMl` | `int?` | Tidak | — | — | — | — | Tidak | Cairan tertarik sampai saat itu |
| `Note` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan pengamatan |

**Unique index** `(SessionId, SequenceNumber)` — mencegah dua pengamatan merebut nomor urut yang
sama, dan sekaligus memastikan riwayat tidak saling menimpa.

Tabel ini **tidak pernah** menyimpan hanya nilai terakhir. Setiap pengamatan adalah baris baru.

### 3.12 `HmdSessionMedication`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `DrugId` | `Guid` | Ya | — | Index | FK `MstDrug` | `Restrict` | Tidak | Obat yang diberikan |
| `Dose` | `decimal(10,3)` | Ya | — | — | — | — | Tidak | Jumlah |
| `DoseUnit` | `string(50)` | Ya | — | — | — | — | Tidak | Satuan |
| `Route` | `int` | Ya | — | — | — | — | Tidak | Jalur pemberian |
| `InstructedByDoctorId` | `Guid?` | Tidak | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter pemberi instruksi |
| `AdministeredByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Petugas pemberi |
| `AdministeredAt` | `DateTime` | Ya | waktu server | Index | — | — | Tidak | Waktu pemberian |
| `DrugUsageId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Rujukan ke pemakaian obat milik Farmasi |
| `HandoffStatus` | `int` | Ya | `Pending` | Index | — | — | Tidak | Status penerusan ke Farmasi |
| `Note` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Catatan |

Bila penerusan ke Farmasi gagal, baris ini **tetap tersimpan**. Obatnya memang sudah masuk ke
tubuh pasien.

### 3.13 `HmdSessionComplication`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `ComplicationType` | `int` | Ya | — | Index | — | — | Tidak | Jenis kejadian |
| `DetectedAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Waktu ditemukan |
| `DetectedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Penemu |
| `SignsAndSymptoms` | `string(1000)` | Ya | — | — | — | — | **Ya** | Tanda dan gejala |
| `Intervention` | `string(1000)` | Ya | — | — | — | — | **Ya** | Tindakan yang dilakukan |
| `ClinicianInstruction` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Instruksi dokter |
| `Outcome` | `int` | Ya | — | Index | — | — | Tidak | `Resolved`, `Ongoing`, `Escalated` |
| `SessionImpact` | `int` | Ya | — | — | — | — | Tidak | `Continued`, `Modified`, `Stopped` |
| `TransferDestination` | `string(200)?` | Tidak | — | — | — | — | Tidak | Tujuan pemindahan bila ada |

Sistem **tidak** menyimpulkan diagnosis dari nilai tanda vital. Baris ini selalu lahir dari
penilaian manusia.

### 3.14 `HmdSessionStaffAssignment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SessionId` | `Guid` | Ya | — | Index | FK `HmdSession` | `Restrict` | Tidak | Induk |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | Petugas |
| `StaffRole` | `int` | Ya | — | Index | — | — | Tidak | `PrimaryNurse`, `AssistantNurse`, `Technician` |
| `AssignedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | Penugas |
| `AssignedAt` | `DateTime` | Ya | waktu server | — | — | — | Tidak | Waktu penugasan |
| `CompetencyVerificationStatus` | `int` | Ya | `NotVerifiable` | Index | — | — | Tidak | **Syarat 5** — `Verified`, `NotAuthorized`, `NotVerifiable` |
| `CompetencyCheckedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemeriksaan kewenangan |
| `CompetencySourceReference` | `string(200)?` | Tidak | — | — | — | — | Tidak | Rujukan sumber pemeriksaan |

Nilai bawaan `NotVerifiable` disengaja. Selama pembacaan kewenangan dari Human Resource belum
tersedia, itulah keadaan yang jujur.

**Unique index** `(SessionId, WorkforceProfileId)`.

### 3.15 `HmdMachine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `MachineCode` | `string(30)` | Ya | — | Unique gabungan | — | — | Tidak | Kode mesin, unik per unit |
| `MachineName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama mesin |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | Unit pemilik |
| `Manufacturer` | `string(150)?` | Tidak | — | — | — | — | Tidak | Pabrikan |
| `SerialNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Nomor seri |
| `MachineStatus` | `int` | Ya | `Ready` | Index | — | — | Tidak | `Ready`, `Blocked`, `Maintenance`, `NotEligible` |
| `DedicatedFor` | `int` | Ya | `None` | Index | — | — | Tidak | Mesin khusus, misalnya untuk pasien Hepatitis B |
| `IsSchedulable` | `bool` | Ya | `true` | Index | — | — | Tidak | Boleh masuk penjadwalan |
| `LastStatusChangedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Perubahan status terakhir |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Masih dipakai unit |

**Unique index** `(ServiceUnitId, MachineCode)`.

### 3.16 `HmdMachineStatusHistory`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `MachineId` | `Guid` | Ya | — | Index | FK `HmdMachine` | `Restrict` | Tidak | Mesin |
| `FromStatus` | `int?` | Tidak | — | — | — | — | Tidak | Kosong pada baris pertama |
| `ToStatus` | `int` | Ya | — | Index | — | — | Tidak | Status baru |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan perubahan |
| `ChangedByUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Pengubah |
| `ChangedAt` | `DateTime` | Ya | waktu server | Index | — | — | Tidak | Waktu perubahan |

### 3.17 `HmdStation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `StationCode` | `string(30)` | Ya | — | Unique gabungan | — | — | Tidak | Kode station |
| `StationName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama station |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | Unit pemilik |
| `RoomId` | `Guid?` | Tidak | — | Index | FK `MstRoom` | `Restrict` | Tidak | Ruang tempat station berada |
| `StationStatus` | `int` | Ya | `Available` | Index | — | — | Tidak | `Available`, `Blocked`, `Maintenance` |
| `IsIsolationStation` | `bool` | Ya | `false` | Index | — | — | Tidak | Station untuk pasien yang perlu isolasi |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Masih dipakai |

**Unique index** `(ServiceUnitId, StationCode)`.

### 3.18 `HmdChecklistItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ItemCode` | `string(50)` | Ya | — | Unique | — | — | Tidak | Kode butir |
| `ItemName` | `string(200)` | Ya | — | — | — | — | Tidak | Nama butir sebagaimana dibaca perawat |
| `Category` | `int` | Ya | — | Index | — | — | Tidak | Identitas, konteks klinis, atau sumber daya |
| `IsMandatory` | `bool` | Ya | `true` | Index | — | — | Tidak | Butir wajib |
| `IsOverridable` | `bool` | Ya | **`false`** | Index | — | — | Tidak | **Syarat 1** — apakah boleh dilewati dokter |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Masih dipakai |

Kolom `IsOverridable` adalah wujud `HMD-ASM-001`. Nilai bawaannya `false` untuk seluruh butir,
dan hanya pemegang hak akses tata kelola klinis yang dapat mengubahnya.

### 3.19 `HmdReadinessItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ItemCode` | `string(50)` | Ya | — | Unique | — | — | Tidak | Kode butir |
| `ItemName` | `string(200)` | Ya | — | — | — | — | Tidak | Nama butir |
| `Category` | `int` | Ya | — | Index | — | — | Tidak | Mesin, station, air, bahan, atau staf |
| `IsMandatory` | `bool` | Ya | `true` | Index | — | — | Tidak | Butir wajib untuk menyatakan unit siap |
| `RequiresResultDate` | `bool` | Ya | `false` | — | — | — | Tidak | Butir yang hasilnya punya masa berlaku, misalnya pemeriksaan air |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Masih dipakai |

### 3.20 `HmdUnitReadiness`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | Unit HD |
| `ReadinessDate` | `DateOnly` | Ya | — | Unique gabungan | — | — | Tidak | Tanggal |
| `Shift` | `int` | Ya | — | Unique gabungan | — | — | Tidak | Shift |
| `ReadinessStatus` | `int` | Ya | `Draft` | Index | — | — | Tidak | `Draft`, `Ready`, `NotReady` |
| `DeclaredByUserId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Penyata |
| `DeclaredAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pernyataan |
| `NotReadyReason` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Alasan unit tidak siap |

**Unique index** `(ServiceUnitId, ReadinessDate, Shift)` untuk baris `IsDelete = false`.

### 3.21 `HmdUnitReadinessDetail`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `UnitReadinessId` | `Guid` | Ya | — | Index | FK `HmdUnitReadiness` | `Restrict` | Tidak | Induk |
| `ReadinessItemId` | `Guid` | Ya | — | Index | FK `HmdReadinessItem` | `Restrict` | Tidak | Butir |
| `Result` | `int` | Ya | `NotChecked` | Index | — | — | Tidak | `NotChecked`, `Met`, `NotMet`, `NotApplicable` |
| `ResultDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal hasil, untuk butir yang punya masa berlaku |
| `ReferenceNumber` | `string(100)?` | Tidak | — | — | — | — | Tidak | Rujukan hasil pemeriksaan luar |
| `VerifiedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pemeriksa |
| `VerifiedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemeriksaan |
| `Note` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan |

**Unique index** `(UnitReadinessId, ReadinessItemId)`.

### 3.22 `HmdSetting`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ServiceUnitId` | `Guid` | Ya | — | Unique | FK `MstServiceUnit` | `Restrict` | Tidak | Satu baris per unit HD |
| `MaxPatientsPerNurse` | `int` | Ya | `3` | — | — | — | Tidak | **Syarat 3** — batas pasien per perawat |
| `EnforceNurseRatio` | `bool` | Ya | `false` | — | — | — | Tidak | Menolak penjadwalan bila batas terlampaui |
| `WaterResultValidityHours` | `int` | Ya | `720` | — | — | — | Tidak | **Syarat 3** — masa berlaku hasil pemeriksaan air |
| `EnforceCompetencyCheck` | `bool` | Ya | `false` | — | — | — | Tidak | **Syarat 5** — menolak penugasan petugas tanpa kewenangan |
| `AllowMultipleActiveEpisodePerPatient` | `bool` | Ya | `false` | — | — | — | Tidak | Melonggarkan aturan satu episode aktif |
| `SessionStartGraceMinutes` | `int` | Ya | `60` | — | — | — | Tidak | Toleransi mulai sesi terhadap jadwal |
| `RequireDifferentSigner` | `bool` | Ya | `true` | — | — | — | Tidak | Pengesah harus berbeda dari penyelesai dokumentasi |

---

## 4. Bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, **bukan** skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya
> akan berbenturan dengan migration.
>
> Kolom audit warisan `IdentityModel` **tidak ditulis ulang** di sini.

Empat tabel paling menentukan ditampilkan bentuknya. Delapan belas tabel lain mengikuti pola
yang sama dan bentuknya dapat dibaca dari tabel kolom pada bagian 3.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."HmdOrder" (
    "Id"                   uuid          NOT NULL,
    "OrderNumber"          varchar(30)   NOT NULL,
    "PatientId"            uuid          NOT NULL,
    "EncounterId"          uuid          NOT NULL,
    "InpEpisodeId"         uuid,
    "EpisodeId"            uuid,
    "RequestedByUserId"    uuid          NOT NULL,
    "RequestingDoctorId"   uuid,
    "RequestedAt"          timestamp     NOT NULL,
    "Priority"             integer       NOT NULL,   -- enum, HasConversion<int>
    "ClinicalReason"       varchar(1000) NOT NULL,   -- SENSITIF
    "RequestedDate"        date,
    "OrderStatus"          integer       NOT NULL,   -- enum, HasConversion<int>
    "StatusBeforeHold"     integer,
    "DecisionByUserId"     uuid,
    "DecisionAt"           timestamp,
    "DecisionReason"       varchar(1000),            -- SENSITIF

    CONSTRAINT "PK_HmdOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HmdOrder_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdOrder_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_HmdOrder_OrderNumber" ON public."HmdOrder" ("OrderNumber");
CREATE INDEX "IX_HmdOrder_PatientId_OrderStatus" ON public."HmdOrder" ("PatientId", "OrderStatus");
CREATE INDEX "IX_HmdOrder_RequestedDate_Priority" ON public."HmdOrder" ("RequestedDate", "Priority");


CREATE TABLE public."HmdEpisode" (
    "Id"              uuid          NOT NULL,
    "EpisodeNumber"   varchar(30)   NOT NULL,
    "PatientId"       uuid          NOT NULL,
    "ServiceUnitId"   uuid          NOT NULL,
    "DpjpDoctorId"    uuid          NOT NULL,
    "StartDate"       date          NOT NULL,
    "EndDate"         date,
    "EpisodeStatus"   integer       NOT NULL,   -- enum, HasConversion<int>
    "SuspendReason"   varchar(500),
    "ClosureReason"   integer,
    "ClosureNote"     varchar(1000),            -- SENSITIF

    CONSTRAINT "PK_HmdEpisode" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HmdEpisode_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdEpisode_MstDoctor_DpjpDoctorId"
        FOREIGN KEY ("DpjpDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_HmdEpisode_EpisodeNumber" ON public."HmdEpisode" ("EpisodeNumber");

-- Satu episode aktif per pasien. Index bersyarat, bukan unique biasa.
CREATE UNIQUE INDEX "IX_HmdEpisode_PatientId_Active"
    ON public."HmdEpisode" ("PatientId")
    WHERE "EpisodeStatus" = 2 AND "IsDelete" = false;   -- 2 = Active


CREATE TABLE public."HmdSession" (
    "Id"                      uuid          NOT NULL,
    "SessionNumber"           varchar(30)   NOT NULL,
    "EpisodeId"               uuid          NOT NULL,
    "PrescriptionId"          uuid          NOT NULL,
    "EncounterId"             uuid,
    "InpEpisodeId"            uuid,
    "OrderId"                 uuid,
    "MachineId"               uuid          NOT NULL,
    "StationId"               uuid          NOT NULL,
    "ResponsibleDoctorId"     uuid,
    "ScheduledDate"           date          NOT NULL,
    "Shift"                   integer       NOT NULL,
    "ScheduledStartAt"        timestamp     NOT NULL,
    "ScheduledEndAt"          timestamp     NOT NULL,
    "CheckedInAt"             timestamp,
    "StartedAt"               timestamp,
    "StartedByUserId"         uuid,
    "EndedAt"                 timestamp,
    "ActualDurationMinutes"   integer,
    "ActualUltrafiltrationMl" integer,
    "SessionStatus"           integer       NOT NULL,   -- enum, HasConversion<int>
    "StopReason"              integer,
    "StopNote"                varchar(1000),            -- SENSITIF
    "DeviationNote"           varchar(1000),            -- SENSITIF
    "Disposition"             integer,
    "DispositionNote"         varchar(1000),            -- SENSITIF
    "DocumentedByUserId"      uuid,
    "DocumentedAt"            timestamp,
    "SignedByUserId"          uuid,
    "SignedAt"                timestamp,
    "PatientProcedureId"      uuid,
    "IdempotencyKey"          varchar(100),
    "BillingHandoffStatus"    integer       NOT NULL,
    "BillingHandoffAt"        timestamp,
    "BillingHandoffError"     varchar(1000),

    CONSTRAINT "PK_HmdSession" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HmdSession_HmdEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."HmdEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdSession_HmdPrescription_PrescriptionId"
        FOREIGN KEY ("PrescriptionId") REFERENCES public."HmdPrescription" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdSession_HmdMachine_MachineId"
        FOREIGN KEY ("MachineId") REFERENCES public."HmdMachine" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdSession_HmdStation_StationId"
        FOREIGN KEY ("StationId") REFERENCES public."HmdStation" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HmdSession_TrxPatientProcedure_PatientProcedureId"
        FOREIGN KEY ("PatientProcedureId") REFERENCES public."TrxPatientProcedure" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_HmdSession_SessionNumber" ON public."HmdSession" ("SessionNumber");

-- Satu sesi tidak pernah punya dua tindakan
CREATE UNIQUE INDEX "IX_HmdSession_PatientProcedureId"
    ON public."HmdSession" ("PatientProcedureId") WHERE "PatientProcedureId" IS NOT NULL;

-- Tombol Mulai yang ditekan dua kali tetap menghasilkan satu sesi
CREATE UNIQUE INDEX "IX_HmdSession_IdempotencyKey"
    ON public."HmdSession" ("IdempotencyKey") WHERE "IdempotencyKey" IS NOT NULL;

-- Tiga index pencegahan tabrakan. Bukan unique: tumpang tindih waktu diperiksa service.
CREATE INDEX "IX_HmdSession_Machine_Window"
    ON public."HmdSession" ("MachineId", "ScheduledStartAt", "ScheduledEndAt");
CREATE INDEX "IX_HmdSession_Station_Window"
    ON public."HmdSession" ("StationId", "ScheduledStartAt", "ScheduledEndAt");
CREATE INDEX "IX_HmdSession_Episode_Window"
    ON public."HmdSession" ("EpisodeId", "ScheduledStartAt");


CREATE TABLE public."HmdSessionObservation" (
    "Id"                      uuid          NOT NULL,
    "SessionId"               uuid          NOT NULL,
    "SequenceNumber"          integer       NOT NULL,
    "ObservedAt"              timestamp     NOT NULL,
    "RecordedByUserId"        uuid          NOT NULL,
    "SystolicBp"              integer,
    "DiastolicBp"             integer,
    "PulseRate"               integer,
    "RespiratoryRate"         integer,
    "TemperatureC"            numeric(4,1),
    "OxygenSaturation"        integer,
    "BloodFlowRate"           integer,
    "DialysateFlowRate"       integer,
    "TransmembranePressure"   integer,
    "VenousPressure"          integer,
    "ArterialPressure"        integer,
    "UltrafiltrationVolumeMl" integer,
    "Note"                    varchar(1000),            -- SENSITIF

    CONSTRAINT "PK_HmdSessionObservation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HmdSessionObservation_HmdSession_SessionId"
        FOREIGN KEY ("SessionId") REFERENCES public."HmdSession" ("Id") ON DELETE RESTRICT
);

-- Riwayat pemantauan tidak boleh saling menimpa
CREATE UNIQUE INDEX "IX_HmdSessionObservation_SessionId_SequenceNumber"
    ON public."HmdSessionObservation" ("SessionId", "SequenceNumber");
CREATE INDEX "IX_HmdSessionObservation_SessionId_ObservedAt"
    ON public."HmdSessionObservation" ("SessionId", "ObservedAt");
```

Seluruh relasi klinis memakai `DeleteBehavior.Restrict`, supaya riwayat transaksi tidak ikut
terhapus berantai ketika satu baris induk ditandai terhapus.
