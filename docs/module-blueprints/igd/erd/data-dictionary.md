# Kamus Data — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5`; **bagian 6 ditambahkan 22 September 2026** (encounter-first) |
| Status | `draft` |
| Commit diaudit | backend `f69e9e48` |
| Diselaraskan | 15 September 2026 — nama tabel bagian 4 dan rujukan bagian 5.3 menjadi `EmgDoctorAssignment` (`IGD-DEC-116`); isi kolom tidak berubah |

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

## 4. `EmgDoctorAssignment` — Baru

| Field | Nilai |
| --- | --- |
| Nama tabel | `EmgDoctorAssignment`, schema `public`. Ditetapkan `IGD-DEC-116`, menggantikan nama rancangan lama `TrxEmergencyDoctorAssignment` |
| Status | **Baru** |
| Perilaku hapus | Penandaan; baris lama **tidak pernah ditimpa** |
| Pemilik | Emergency Installation |

| Kolom | Tipe | Wajib | Bawaan | Panjang | Validasi | Sensitif |
| --- | --- | :---: | --- | ---: | --- | :---: |
| `Id` | `uuid` PK | Ya | `NEWID()` | — | — | Tidak |
| `EmergencyVisitId` | `uuid` FK | Ya | — | — | — | Tidak |
| `DoctorId` | `uuid` FK | Ya | — | — | Dokter harus ada dan aktif | Tidak |
| `EffectiveFrom` | `timestamp with time zone` | Ya | Waktu server | — | Tidak boleh mendahului waktu kedatangan pasien | Tidak |
| `EffectiveTo` | `timestamp with time zone?` | Tidak | — | — | **Kosong berarti penugasan sedang berjalan** | Tidak |
| `AssignedByUserId` | `uuid?` FK | **Bersyarat** | Pengguna aktif | — | Diisi sistem. **Wajib** untuk setiap penetapan dan pengalihan baru. Boleh `NULL` **hanya** pada baris hasil pengisian data lama `BE-IGD-048` ketika pelaku historisnya tidak dapat dibuktikan (`IGD-DEC-136`) | Tidak |
| `AssignmentReason` | `varchar` | Tidak | — | 500 | **Wajib** saat pengalihan, bukan saat penetapan pertama | Tidak |

**Index sebagaimana benar-benar diterapkan** (migration `20260917072515`, 17 September 2026):

| Nama | Bentuk | Asal |
| --- | --- | --- |
| `IX_EmgDoctorAssignment_EmergencyVisitId_Active` | **unique bersyarat**, penyaring `EffectiveTo IS NULL` | Kamus data ini |
| `IX_EmgDoctorAssignment_EmergencyVisitId_EffectiveFrom` | komposit | Kamus data ini |
| `IX_EmgDoctorAssignment_DoctorId` | tunggal | Ditambahkan `BE-IGD-044` untuk pencarian per dokter |
| `IX_EmgDoctorAssignment_EffectiveTo` | tunggal | Ditambahkan `BE-IGD-044` untuk penyaringan penugasan berjalan |
| `IX_EmgDoctorAssignment_AssignedByUserId` | tunggal | Dibuat EF otomatis untuk foreign key |

Penyaring unique bersyarat **tidak** menyertakan `IsDelete`. Menyertakannya membuat baris
yang di-soft-delete melepaskan slot berjalannya, sehingga satu kunjungan dapat punya dua
baris `EffectiveTo IS NULL` — pelajaran `BE-IGD-047`.

**Foreign key:** `AspNetUsers.Id`, `EmgVisit.Id`, dan `MstDoctor.Id`, ketiganya `Restrict`. FK ke
`AspNetUsers.Id` (`AssignedByUserId`) bersifat opsional sejak `BE-IGD-048` — `NULL` tidak
melanggar foreign key di PostgreSQL.

> **`EffectiveFrom` pada baris hasil `BE-IGD-048` adalah *historical fallback*.** Diisi
> `EmgVisit.ArrivalDateTime` (awal episode IGD) karena waktu penetapan dokter sesungguhnya pada data
> lama **tidak tersimpan**. Nilainya **bukan** waktu penetapan yang terbukti; artinya "tercatat sebagai
> penanggung jawab sejak awal episode". `RegPatientEncounter.UpdateDateTime` sengaja **tidak** dipakai
> — ia waktu edit terakhir apa pun. Baris legacy dikenali dari `AssignedByUserId IS NULL` dan
> `AssignmentReason = 'Data historis - pengisian BE-IGD-048'`.

> **`AssignedByUserId` menjadi nullable — `BE-IGD-048`, `IGD-DEC-136`, 18 September 2026.**
> Migration `20260917072515` menerapkannya `NOT NULL`. Audit schema hari itu juga menemukan
> `Up()` migration tersebut nol `InsertData`/`Sql` — tabelnya applied kosong. `IGD-DEC-136`
> menutup `IGD-OQ-092` dengan menetapkan kolom ini boleh `NULL` khusus baris hasil pengisian
> data lama yang pelaku historisnya tak terbukti; migration korektif `BE-IGD-048` mengubah
> nullability sekaligus mengisi data lama. Baris riwayat sungguhan tetap wajib berpelaku.

> **`IsActive` sengaja tidak ada — `IGD-DEC-130`, 16 September 2026.** Rancangan sebelumnya
> memuat kolom `IsActive` **dan** `EffectiveTo`, padahal keduanya menyatakan fakta yang sama dan
> hanya `EffectiveTo` yang dijaga unique bersyarat. Dua penanda untuk satu fakta pasti berbeda
> suatu hari — satu diperbarui, satunya tertinggal. Kolom itu **dihapus sebelum model dan
> migration dibuat**, bukan ditambahkan lalu dicabut pada migration berikutnya.
>
> Penentu waktunya tinggal satu:
>
> | Pertanyaan | Aturan |
> | --- | --- |
> | Penugasan yang sedang berjalan | `EffectiveTo IS NULL` |
> | Penugasan pada waktu `T` | `EffectiveFrom <= T AND (EffectiveTo IS NULL OR T < EffectiveTo)` |
>
> `IsDelete` dan `IsCancel` bawaan `IdentityModel` tetap ada sebagai urusan **validitas baris dan
> audit**, dan **bukan** pengganti `EffectiveTo`.

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
| `DoctorId` | Sudah ada | **Nilai efektif** dokter aktif; riwayatnya di `EmgDoctorAssignment` | Tidak |
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

---

## 6. Encounter-first — 22 September 2026 (**Rencana (belum tersedia)**)

Kontrak kolom untuk tabel `Baru`/`Diperbarui` slice encounter-first (`IGD-DEC-139`, `142`…`148`, `150`…`154`).
Salinannya ada di `02-backend-architecture.md` §13.6; **berkas ini yang mengikat** bila keduanya berbeda.
Sepuluh kolom `IdentityModel` tidak diulang. Kolom bertanda **Sensitif** tidak boleh masuk custom logger
dan tidak boleh dipakai sebagai contoh berisi data asli.

| Tabel | Status | Pemilik | Schema |
| --- | --- | --- | --- |
| `EmgVisit` | **Diperbarui** — 3 kolom | IGD | `public` |
| `EmgDuplicateEpisodeOverride` | **Baru** | IGD | `public` |
| `EmgEncounterReconciliationRun` | **Baru** | IGD | `public` |
| `EmgEncounterReconciliationItem` | **Baru** | IGD | `public` |
| `RegPatientEncounter` | **Sudah ada** — nol kolom baru | Registration Management | `public` |

**`EmgVisit` — Diperbarui** (`public."EmgVisit"`)

| Kolom | Tipe | Wajib | Bawaan | Validasi | Sensitif |
| --- | --- | :-: | --- | --- | :-: |
| `ArrivalTimeSource` | `int` (`EmergencyArrivalTimeSource`) | Ya | `0` (`Unverified`) | Nilai enum | Tidak |
| `ArrivalConfirmedByUserId` | `uuid?` | Tidak | `null` | FK `AspNetUsers`, `Restrict`; diisi dari token | Tidak |
| `ArrivalConfirmedAt` | `timestamptz?` | Tidak | `null` | Waktu server | Tidak |

Index: tidak ada yang baru. Perilaku hapus: tidak berubah.

**`EmgDuplicateEpisodeOverride` — Baru** (`public."EmgDuplicateEpisodeOverride"`, mewarisi `IdentityModel`)

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `EncounterId` | `uuid` | Ya | FK `RegPatientEncounter`, `Restrict`; **unique** — satu catatan per encounter baru | Tidak |
| `PatientId` | `uuid` | Ya | FK `MstPatient`, `Restrict`; index | Tidak |
| `OverriddenEncounterId` | `uuid?` | Tidak | FK `RegPatientEncounter`, `Restrict` — episode lama bila berupa encounter | Tidak |
| `OverriddenVisitId` | `uuid?` | Tidak | FK `EmgVisit`, `Restrict` — episode lama bila berupa kunjungan | Tidak |
| `Reason` | `varchar(500)` | Ya | Di-*trim*, 1–500 | **Ya** — dapat memuat keterangan klinis |
| `OverriddenByUserId` | `uuid` | Ya | FK `AspNetUsers`, `Restrict`; dari token | Tidak |
| `OverriddenAt` | `timestamptz` | Ya | Waktu server | Tidak |

Check constraint: `OverriddenEncounterId IS NOT NULL OR OverriddenVisitId IS NOT NULL`. Tambah-saja: tidak
ada endpoint ubah/hapus.

**`EmgEncounterReconciliationRun` — Baru**

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RunNumber` | `varchar(50)` | Ya | Unique; dibentuk `DocumentNumberService` | Tidak |
| `Status` | `int` | Ya | `EmergencyReconciliationRunStatus` | Tidak |
| `Reason` | `varchar(500)` | Ya | 1–500 | Ya |
| `CountK1`, `CountK1Outpatient`, `CountK2`, `CountK3`, `CountK4` | `int` | Ya | Hasil hitung saat eksekusi | Tidak |
| `ExecutedByUserId` | `uuid` | Ya | FK `AspNetUsers`, `Restrict` | Tidak |
| `ExecutedAt` | `timestamptz` | Ya | Server | Tidak |
| `ReversedByUserId` | `uuid?` | Tidak | FK `AspNetUsers`, `Restrict` | Tidak |
| `ReversedAt` | `timestamptz?` | Tidak | Server | Tidak |
| `ReverseReason` | `varchar(500)?` | Tidak | Wajib saat dibalik | Ya |

**`EmgEncounterReconciliationItem` — Baru**

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RunId` | `uuid` | Ya | FK run, `Cascade` (baris ikut run; run sendiri tidak pernah dihapus) | Tidak |
| `EncounterId` | `uuid` | Ya | FK `RegPatientEncounter`, `Restrict`; index | Tidak |
| `EmergencyVisitId` | `uuid` | Ya | FK `EmgVisit`, `Restrict` | Tidak |
| `Class` | `int` | Ya | Hanya `K1` atau `K1Outpatient` yang pernah ditulis | Tidak |
| `StatusBefore`, `StatusAfter` | `int` | Ya | Nilai `EncounterStatus` | Tidak |
| `CompletedAtBefore`, `CompletedAtAfter` | `timestamptz?` | Tidak | — | Tidak |
| `IsReversed` | `bool` | Ya | Bawaan `false` | Tidak |
| `ReverseSkipReason` | `varchar(200)?` | Tidak | Diisi bila baris dilewati saat pembalikan | Tidak |

Unique `(RunId, EncounterId)`.

**`RegPatientEncounter` — Sudah ada, kolom kunci yang dipakai aturan slice ini** (model: `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`)

| Kolom | Dipakai untuk | Ditulis IGD? |
| --- | --- | :-: |
| `Id`, `PatientId`, `EncounterType` | Rumus episode terbuka klausa A | Tidak |
| `EncounterStatus` | Tanda berakhir; NoShow; penutupan ikut kunjungan; rekonsiliasi | **Ya** |
| `RegisteredAt` | Label "Terdaftar" di daftar triage; fallback waktu tiba | Tidak |
| `CompletedAt` | Tanda berakhir | **Ya** |
| `NoShowAt`, `NoShowByUserId`, `NoShowReason` | Tanda berakhir; NoShow IGD | **Ya** |
| `IsCancel`, `CancelledAt`, `CancelledByUserId`, `CancelReason`, `IsActive` | Tanda berakhir; pembatalan ikut kunjungan | **Ya** |
| `ChiefComplaint` | Nilai awal keluhan saat kunjungan lahir | Tidak |
| `IsQueueRequired` | **Tidak lagi menentukan** antrean untuk Emergency (`IGD-DEC-144`) | Tidak |

**Enum baru** — disimpan sebagai `int`: `EmergencyArrivalTimeSource` (`Unverified = 0` bawaan, `Fallback = 1`,
`Confirmed = 2`); `EmergencyReconciliationClass` (`K1 = 1`, `K1Outpatient = 2`, `K2 = 3`, `K3 = 4`, `K4 = 5`);
`EmergencyReconciliationRunStatus` (`Executed = 1`, `Reversed = 2`). `EmergencyVisitStartMode` hanya untuk request,
tidak disimpan.

## 7. Penutupan lewat disposisi — 23 September 2026

Slice `IGD-DEC-163`…`169` menambah **satu kolom** dan tidak membuat tabel baru. Sepuluh kolom warisan
`IdentityModel` tidak diulang di sini; lihat kepala dokumen.

### `EmgVisit` — Diperbarui (pemilik: IGD)

| Kolom | Tipe | Wajib | Bawaan | Aturan | Sensitif |
| --- | --- | :-: | --- | --- | :-: |
| `ClosedByDispositionId` | `uuid?` | Tidak | `null` | FK `EmgDisposition.Id`, `DeleteBehavior.Restrict`, index FK bawaan konvensi. Terisi hanya bila penutupan berasal dari disposisi yang dilaksanakan; kosong berarti kunjungan ditutup petugas lewat aksi selesaikan kunjungan, atau belum ditutup | Tidak |

**Cara membacanya.** `VisitCompletedAt` terisi **dan** `ClosedByDispositionId` kosong berarti penutupan manual.
Keduanya terisi berarti penutupan berasal dari disposisi, dan kolom itu menunjuk disposisi mana. `VisitCompletedAt`
kosong berarti kunjungan belum selesai — apa pun isi kolom lain.

### Tabel yang **tidak** berubah pada slice ini

`EmgDisposition`, `EmgObservation`, `EmgDeparture`, `EmgHandoverOrderItem`, `EmgDispositionType`, dan
`RegPatientEncounter` seluruhnya dibaca apa adanya. Nol kolom baru, nol index baru, nol perubahan perilaku hapus.
`RegPatientEncounter` tetap ditulis hanya lewat daftar kolom tertutup integration §5.2.
