# Kamus Data — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5` |
| Status | `draft` |
| Commit diaudit | backend `f69e9e48` |

## Kolom warisan yang tidak diulang

Seluruh tabel mewarisi `IdentityModel` dan karena itu memiliki sepuluh kolom berikut. Kolom
ini **tidak diulang** pada tabel mana pun di bawah:

| Kolom | Tipe | Kegunaan |
| --- | --- | --- |
| `CreateDateTime` | `timestamp` | Waktu baris dibuat |
| `CreateBy` | `uuid` | Pembuat |
| `UpdateDateTime` | `timestamp?` | Waktu perubahan terakhir |
| `UpdateBy` | `uuid` | Pengubah terakhir |
| `DeleteDateTime` | `timestamp?` | Waktu penandaan hapus |
| `DeleteBy` | `uuid` | Penanda hapus |
| `CancelDateTime` | `timestamp?` | Waktu pembatalan |
| `CancelBy` | `uuid` | Pembatal |
| `IsCancel` | `boolean` | Penanda batal |
| `IsDelete` | `boolean` | Penanda hapus; **penghapusan bersifat penandaan** |

> **Keterbatasan yang wajib diketahui.** Kolom di atas hanya menyimpan **pelaku terakhir**.
> Untuk catatan klinis, `IGD-DEC-080` mensyaratkan riwayat versi yang tidak dapat diwakili
> kolom-kolom ini. Karena itu tabel klinis memperoleh penanda versi tersendiri.

## Penanda sensitif

Kolom bertanda **Sensitif** memuat data pribadi atau klinis pasien. Kolom tersebut:

- **tidak boleh** masuk ke custom logger maupun berkas log;
- **tidak boleh** dipakai sebagai contoh berisi data asli pada dokumen mana pun;
- hanya boleh ditampilkan kepada pengguna yang berwenang atas unit dan kemampuannya.

---

## 1. `TrxEmergencyDeparture` — Diperbarui

| Field | Nilai |
| --- | --- |
| Nama tabel | `TrxEmergencyDeparture`, schema `public` |
| Status | **Diperbarui** — berganti nama dari `TrxEmergencyTransfer` |
| Perilaku hapus | Penandaan, bukan penghapusan baris |
| Pemilik | Emergency Installation |

| Kolom | Tipe | Wajib | Bawaan | Panjang | Validasi | Sensitif |
| --- | --- | :---: | --- | ---: | --- | :---: |
| `Id` | `uuid` PK | Ya | `NEWID()` | — | — | Tidak |
| `EmergencyVisitId` | `uuid` FK | Ya | — | — | Kunjungan harus ada dan belum `Completed` | Tidak |
| `DepartureNumber` | `varchar` UK | Ya | Dibentuk sistem | 50 | Unik di seluruh baris termasuk yang ditandai hapus | Tidak |
| `FromServiceUnitId` | `uuid` FK | Tidak | — | — | Harus ada bila diisi | Tidak |
| `ToServiceUnitId` | `uuid` FK | Ya | — | — | Harus ada dan berbeda dari `FromServiceUnitId` | Tidak |
| `PhysicalStatus` | `int` | Ya | `Prepared` | — | Nilai enum `EmergencyPhysicalStatus` | Tidak |
| `HandoverStatus` | `int` | Ya | `Submitted` | — | Nilai enum `EmergencyHandoverStatus` | Tidak |
| `SendingNurseUserId` | `uuid` FK | Tidak | — | — | — | Tidak |
| `ReceivingNurseUserId` | `uuid` FK | Tidak | — | — | Wajib berwenang atas `ToServiceUnitId` | Tidak |
| `SituationSummary` | `text` | Tidak | — | 2000 | Wajib terisi atau ditandai tidak dapat diisi sebelum dokumen diajukan | **Ya** |
| `BackgroundSummary` | `text` | Tidak | — | 2000 | Sama | **Ya** |
| `AssessmentSummary` | `text` | Tidak | — | 2000 | Sama | **Ya** |
| `RecommendationSummary` | `text` | Tidak | — | 2000 | Sama | **Ya** |
| `AllergySnapshot` | `text` | Tidak | — | 1000 | Diisi sistem saat dokumen diajukan | **Ya** |
| `LastVitalSignId` | `uuid` FK | Tidak | — | — | Diisi sistem saat dokumen diajukan | Tidak |
| `TriageLevelSnapshot` | `varchar` | Tidak | — | 150 | Diisi sistem dari penilaian triase terakhir yang `Completed` | Tidak |
| `TransferReason` | `varchar` | Tidak | — | 1000 | — | **Ya** |
| `Notes` | `varchar` | Tidak | — | 1000 | — | **Ya** |
| `IsActive` | `boolean` | Ya | `true` | — | — | Tidak |

**Kolom yang dihapus:** `FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId`, `TransferStatus`,
`AcceptedAt`, `AcceptedByUserId`, `RejectionReason`, `HandoverSummary`.

`RejectionReason` dan `HandoverSummary` pindah ke `TrxEmergencyDepartureEvent.Reason` dan ke
empat kolom SBAR, karena keduanya kini punya riwayat.

**Index:** `DepartureNumber` unik; `(EmergencyVisitId, PhysicalStatus)`;
`(ToServiceUnitId, HandoverStatus)`; `FromServiceUnitId`.

---

## 2. `TrxEmergencyDepartureEvent` — Baru

| Field | Nilai |
| --- | --- |
| Nama tabel | `TrxEmergencyDepartureEvent`, schema `public` |
| Status | **Baru** |
| Perilaku hapus | **Tidak ada endpoint hapus.** Relasi induk `DeleteBehavior.Restrict` |
| Pemilik | Emergency Installation |

| Kolom | Tipe | Wajib | Bawaan | Panjang | Validasi | Sensitif |
| --- | --- | :---: | --- | ---: | --- | :---: |
| `Id` | `uuid` PK | Ya | `NEWID()` | — | — | Tidak |
| `EmergencyDepartureId` | `uuid` FK | Ya | — | — | — | Tidak |
| `EventType` | `int` | Ya | — | — | Nilai enum `EmergencyDepartureEventType` | Tidak |
| `OccurredAt` | `timestamp` | Ya | — | — | **Waktu kejadian sebenarnya.** Tidak boleh di masa depan | Tidak |
| `RecordedAt` | `timestamp` | Ya | Waktu server | — | Diisi sistem, tidak dapat diisi pemanggil | Tidak |
| `RecordedByUserId` | `uuid` FK | Ya | Pengguna aktif | — | Diisi sistem | Tidak |
| `ServiceUnitIdOfActor` | `uuid` FK | Tidak | — | — | Unit tempat pelaku berwenang saat mencatat | Tidak |
| `IsEffective` | `boolean` | Ya | `true` | — | Kejadian yang dikoreksi menjadi `false` | Tidak |
| `SupersedesEventId` | `uuid` FK | Tidak | — | — | Wajib diisi untuk `Amended` dan `Reversed` | Tidak |
| `ApprovedByUserId` | `uuid` FK | Tidak | — | — | **Wajib** untuk `Reversed`, dan **wajib berbeda** dari `RecordedByUserId` | Tidak |
| `Reason` | `varchar` | Tidak | — | 1000 | Wajib untuk `HandoverRejected`, `Cancelled`, `Amended`, `Reversed` | **Ya** |
| `DowntimeReference` | `varchar` | Tidak | — | 250 | Wajib bila `RecordedAt` terpaut jauh dari `OccurredAt` | Tidak |

**Index:** `(EmergencyDepartureId, OccurredAt)`; `(EmergencyDepartureId, EventType, IsEffective)`.

**Concurrency:** penulisan kejadian dan pembaruan kolom status pada `TrxEmergencyDeparture`
terjadi dalam **satu transaksi**. Mustahil ada kejadian tanpa status yang mengikutinya.

---

## 3. `TrxEmergencyHandoverOrderItem` — Baru

| Field | Nilai |
| --- | --- |
| Nama tabel | `TrxEmergencyHandoverOrderItem`, schema `public` |
| Status | **Baru** |
| Perilaku hapus | Penandaan |
| Pemilik | Emergency Installation |

| Kolom | Tipe | Wajib | Bawaan | Panjang | Validasi | Sensitif |
| --- | --- | :---: | --- | ---: | --- | :---: |
| `Id` | `uuid` PK | Ya | `NEWID()` | — | — | Tidak |
| `EmergencyDepartureId` | `uuid` FK | Ya | — | — | — | Tidak |
| `OrderKind` | `int` | Ya | — | — | Nilai enum `EmergencyOrderKind` | Tidak |
| `OrderReferenceId` | `uuid` | Ya | — | — | **Tanpa FK** — menunjuk tabel berbeda menurut `OrderKind` | Tidak |
| `OrderLabelSnapshot` | `varchar` | Ya | — | 250 | Salinan nama pesanan saat sikap diambil | **Ya** |
| `Action` | `int` | Ya | — | — | Nilai enum `EmergencyOrderAction` | Tidak |
| `ActionReason` | `varchar` | Tidak | — | 1000 | **Wajib** bila `Action` = `Cancelled` | **Ya** |
| `ActionByUserId` | `uuid` FK | Ya | Pengguna aktif | — | Diisi sistem | Tidak |
| `ActionAt` | `timestamp` | Ya | Waktu server | — | Diisi sistem | Tidak |
| `IsActive` | `boolean` | Ya | `true` | — | — | Tidak |

**Index:** unique `(EmergencyDepartureId, OrderKind, OrderReferenceId)`.

`OrderLabelSnapshot` disalin supaya daftar serah terima tetap terbaca walaupun pesanan
aslinya kemudian diubah atau dibatalkan modul pemiliknya.

---

## 4. `TrxEmergencyDoctorAssignment` — Baru

| Field | Nilai |
| --- | --- |
| Nama tabel | `TrxEmergencyDoctorAssignment`, schema `public` |
| Status | **Baru** |
| Perilaku hapus | Penandaan; baris lama **tidak pernah ditimpa** |
| Pemilik | Emergency Installation |

| Kolom | Tipe | Wajib | Bawaan | Panjang | Validasi | Sensitif |
| --- | --- | :---: | --- | ---: | --- | :---: |
| `Id` | `uuid` PK | Ya | `NEWID()` | — | — | Tidak |
| `EmergencyVisitId` | `uuid` FK | Ya | — | — | — | Tidak |
| `DoctorId` | `uuid` FK | Ya | — | — | Dokter harus ada dan aktif | Tidak |
| `EffectiveFrom` | `timestamp` | Ya | Waktu server | — | Tidak boleh mendahului waktu kedatangan pasien | Tidak |
| `EffectiveTo` | `timestamp?` | Tidak | — | — | Kosong berarti sedang aktif | Tidak |
| `AssignedByUserId` | `uuid` FK | Ya | Pengguna aktif | — | Diisi sistem | Tidak |
| `AssignmentReason` | `varchar` | Tidak | — | 500 | **Wajib** saat pengalihan, bukan saat penetapan pertama | Tidak |
| `IsActive` | `boolean` | Ya | `true` | — | — | Tidak |

**Index:** `(EmergencyVisitId, EffectiveFrom)`; **unique bersyarat** pada `(EmergencyVisitId)`
untuk baris dengan `EffectiveTo IS NULL`.

---

## 5. Tabel yang sudah ada — kolom kunci saja

Untuk tabel berstatus `Sudah ada`, hanya kolom kunci yang didokumentasikan: primary key,
foreign key, kolom status, dan kolom yang dipakai aturan bisnis modul ini.

### 5.1 `TrxEmergencyVisit`

Berkas model: `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs`

| Kolom kunci | Kegunaan bagi aturan bisnis IGD | Sensitif |
| --- | --- | :---: |
| `Id` PK | — | Tidak |
| `EncounterId` FK, unik | Satu kunjungan pasien hanya boleh punya satu kunjungan IGD | Tidak |
| `PatientId` FK, boleh kosong | Kosong untuk pasien yang belum teridentifikasi | Tidak |
| `RegistrationStatus` | Gerbang pelaksanaan tindak lanjut | Tidak |
| `VisitStatus` | Lifecycle utama; **wajib** diubah lewat `CanTransition` | Tidak |
| `IsUnknownPatient`, `TemporaryPatientAlias` | Pasien tanpa identitas | **Ya** |
| `IsImmediateCareAllowed` | Boleh ditangani sebelum administrasi selesai | Tidak |
| `TreatmentStartedAt` | Dasar perhitungan pelampauan batas waktu triase | Tidak |
| `VisitCompletedAt` | **Hanya** diisi aksi selesaikan kunjungan | Tidak |
| `ChiefComplaint` | Keluhan utama | **Ya** |

### 5.2 `TrxEmergencyTriage`

Berkas model: `.../Models/TrxEmergencyTriage.cs`

| Kolom kunci | Kegunaan | Sensitif |
| --- | --- | :---: |
| `EmergencyVisitId` FK | Induk | Tidak |
| `TriageLevelId` FK | Tingkat kegawatan | Tidak |
| `Sequence` | Unik bersama `EmergencyVisitId` | Tidak |
| `TriageStatus` | `Completed` memicu perubahan status kunjungan | Tidak |
| `PreviousTriageId` FK | Rantai penilaian ulang | Tidak |
| `MaxWaitingMinutesSnapshot`, `ResponseDueAt` | Kosong berarti target belum ditetapkan | Tidak |
| `IsSlaBreached`, `SlaBreachedAt` | Penanda permanen, tidak dihitung ulang | Tidak |
| `AirwaySummary` … `RedFlagSummary` | Ringkasan primary survey | **Ya** |

### 5.3 `TrxPatientEncounter` — Diperbarui, milik Registration Management

Berkas model: `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`

| Kolom | Status | Kegunaan bagi IGD | Sensitif |
| --- | --- | --- | :---: |
| `EncounterType` | Sudah ada, **arti berubah untuk IGD** | Bernilai `Emergency` untuk kunjungan IGD | Tidak |
| `PatientClassId` | Sudah ada | Diisi dari master bertanda `IsForEmergency` | Tidak |
| `DoctorId` | Sudah ada | **Nilai efektif** dokter aktif; riwayatnya di `TrxEmergencyDoctorAssignment` | Tidak |
| `OriginEncounterId` | **Baru**, `uuid?` | Menunjuk kunjungan sebelumnya dalam satu rangkaian kedatangan | Tidak |

`OriginEncounterId`: boleh kosong, tanpa nilai bawaan, `DeleteBehavior.Restrict`, index
tunggal. Validasi: tidak boleh menunjuk dirinya sendiri; rangkaian tidak boleh membentuk
lingkaran.

### 5.4 `MstServiceUnit` — Diperbarui, milik Master Data

Berkas model: `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs`

| Kolom | Status | Kegunaan bagi IGD | Sensitif |
| --- | --- | --- | :---: |
| `OrganizationUnitId` | **Baru**, `uuid?` | Jembatan ke simpul organisasi; dasar penjagaan kewenangan unit | Tidak |

### 5.5 Tabel klinis yang memperoleh penanda versi — milik Clinical Management

Berlaku untuk `TrxPatientAssessment`, `TrxPatientVitalSign`, dan
`TrxPatientIntegratedProgressNote`.

| Kolom | Tipe | Wajib | Bawaan | Kegunaan |
| --- | --- | :---: | --- | --- |
| `IsEffective` | `boolean` | Ya | `true` | Menandai baris yang berlaku; tepat satu per rantai koreksi |
| `Amends…Id` | `uuid?` | Tidak | — | Menunjuk baris yang dikoreksi |
| `AmendmentReason` | `varchar(500)` | Tidak | — | **Wajib** bila `Amends…Id` terisi |

Ketiganya **milik Clinical Management** dan menunggu persetujuan pemiliknya.
