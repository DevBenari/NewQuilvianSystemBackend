# Kamus Data — Bank Darah

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v1` — `draft` |
| `last_changed_in` | `v1` |
| Sumber | `02-backend-architecture.md` (model) · `contracts/` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`,
`UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`,
dan `IsDelete`. Kolom-kolom itu **tidak** diulang pada tabel di bawah maupun pada DDL.

Penghapusan bersifat penandaan melalui `IsDelete`, **bukan** penghapusan baris (`BD-CAP-011`).

⚠️ **Nama tabel `Bbk*` memakai prefix placeholder** yang belum disahkan registry (`BD-DEP-008`). Bila
prefix final berbeda, seluruh nama tabel, kolom FK bernama `Bbk*`, dan Configuration ikut berganti.

---

## 1. Status dan kepemilikan tabel

| Entity / Tabel | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `BbkBloodOrder`, `BbkBloodOrderLine` | Baru | Bank Darah | — |
| `BbkProviderRequest`, `BbkBloodUnitReceipt` | Baru | Bank Darah | — |
| `BbkBloodUnit`, `BbkBloodUnitAllocation`, `BbkCompatibilityEvidence`, `BbkEmergencyAuthorization`, `BbkIssuanceCorrection` | Baru | Bank Darah | — |
| `BbkBloodGroupExam`, `BbkBloodGroupSample`, `BbkBloodGroupConflictResolution` | Baru | Bank Darah | — |
| `BbkBloodBankProcedure` | Baru | Bank Darah | Tanpa penyaluran charge (`DEC-BD-016`) |
| `BbkTransitionHistory` | Baru | Bank Darah | Append-only |
| `MstBloodComponent`, `MstBloodBankReason` | Baru | Bank Darah (master) | Setup MVP |
| `MstServiceUnit` | **Diperbarui** | HealthServices Master Data | +1 kolom `IsAvailableForBloodOrder` |
| `MstPatient`, `TrxPatientEncounter`, `InpEpisode`, `MstDoctor`, `MstClinic`, `MstRoom`, `MstPatientClass`, `MstProcedure`/tarif | Sudah ada | modul masing-masing | Direferensikan, **MUST NOT** disalin |

Enum disimpan sebagai `integer` (`HasConversion<int>`). `BloodType` dipakai ulang (`BD-CAP-016`).

---

## 2. Tabel Baru — kolom lengkap

### `BbkBloodOrder`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OrderNumber` | `string(30)` | Ya | — | Unique | — | — | Tidak | Dari number-series |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | Pasien |
| `EncounterId` | `Guid` | Ya | — | Index | FK `TrxPatientEncounter` | `Restrict` | Tidak | Kunjungan asal |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | Unit pemesan |
| `RequestingDoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Dokter peminta |
| `OrderSource` | `int` (`BbkOrderSource`) | Ya | `Electronic` | — | — | — | Tidak | Elektronik/manual |
| `InputByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Wajib bila `Manual` |
| `OrderStatus` | `int` (`BbkBloodOrderStatus`) | Ya | `Active` | Index | — | — | Tidak | Status order |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

### `BbkBloodOrderLine`

| Kolom | Tipe | Wajib | Index | Relasi | Hapus | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | — | — |
| `BloodOrderId` | `Guid` | Ya | Index | FK `BbkBloodOrder` | `Restrict` | Induk order |
| `BloodComponentId` | `Guid` | Ya | Index | FK `MstBloodComponent` | `Restrict` | Komponen diminta |
| `RequestedQuantity` | `int` | Ya | — | — | — | > 0 (`VAL-BD-002`) |
| `Sequence` | `int` | Ya | — | — | — | Nomor urut |

### `BbkProviderRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — | — |
| `RequestNumber` | `string(30)` | Ya | — | Unique | — | — | Dari number-series |
| `BloodOrderId` | `Guid` | Ya | — | Index | FK `BbkBloodOrder` | `Restrict` | Order asal |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Selalu satu pasien |
| `RequestStatus` | `int` (`BbkProviderRequestStatus`) | Ya | `Requested` | Index | — | — | Status |
| `Version` | `int` | Ya | `0` | — | — | — | Token; jaga sisa ≥ 0 |

### `BbkBloodUnitReceipt`

| Kolom | Tipe | Wajib | Index | Relasi | Hapus | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | — | — |
| `ProviderRequestId` | `Guid` | Ya | Index | FK `BbkProviderRequest` | `Restrict` | Permintaan asal |
| `ReceivedQuantity` | `int` | Ya | — | — | — | Jumlah kantong pada kedatangan ini |
| `ReceivedAt` | `DateTime` | Ya | Index | — | — | Waktu penerimaan fisik |
| `ReceivedByUserId` | `Guid` | Ya | — | — | — | Petugas penerima |
| `Sequence` | `int` | Ya | — | — | — | Urutan kedatangan |

### `BbkBloodUnit`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — | Tidak | — |
| `PmiBagNumber` | `string(50)` | Ya | — | Unique | — | — | **Ya** | Nomor kantong dari PMI |
| `ProviderRequestId` | `Guid` | Ya | — | Index | FK `BbkProviderRequest` | `Restrict` | Asal — tak pernah putus |
| `ReceiptId` | `Guid` | Ya | — | Index | FK `BbkBloodUnitReceipt` | `Restrict` | Penerimaan pelahir |
| `BloodComponentId` | `Guid` | Ya | — | Index | FK `MstBloodComponent` | `Restrict` | Komponen |
| `IsExcess` | `bool` | Ya | `false` | — | — | — | Tidak | Kantong berlebih (`DEC-BD-025`) |
| `UnitStatus` | `int` (`BbkBloodUnitStatus`) | Ya | `Available` | Index | — | — | Tidak | Status kantong |
| `IssuedToPatientId` | `Guid?` | Tidak | — | Index | FK `MstPatient` | `Restrict` | Terisi saat `Issued` |
| `IssuedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemberian (terminal) |
| `IssuedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pelaku pemberian |
| `IssuedViaEmergency` | `bool` | Ya | `false` | — | — | — | Tidak | Penanda jalur darurat |
| `CompatibilityEvidenceIdUsed` | `Guid?` | Tidak | — | — | FK `BbkCompatibilityEvidence` | `Restrict` | Bukti yang dipakai saat pemberian |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token — jaga alokasi tunggal |

### `BbkBloodUnitAllocation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — | — |
| `BloodUnitId` | `Guid` | Ya | — | Index | FK `BbkBloodUnit` | `Restrict` | Kantong |
| `BloodOrderLineId` | `Guid` | Ya | — | Index | FK `BbkBloodOrderLine` | `Restrict` | Baris kebutuhan |
| `AllocationStatus` | `int` (`BbkAllocationStatus`) | Ya | `Active` | Index | — | — | **Maks 1 `Active`/kantong** |
| `AllocatedByUserId` | `Guid` | Ya | — | — | — | — | Pelaku alokasi |
| `AllocatedAt` | `DateTime` | Ya | — | — | — | — | Waktu alokasi |
| `CancelReasonCode` | `string(30)?` | Tidak | — | — | FK `MstBloodBankReason.ReasonCode` | `Restrict` | Bila dibatalkan |
| `CancelReasonNote` | `string(500)?` | Tidak | — | — | — | — | Salinan teks alasan |
| `CancelledByUserId` | `Guid?` | Tidak | — | — | — | — | — |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | — |

> Keunikan "satu alokasi aktif" dijaga **filtered unique index** `(BloodUnitId) WHERE AllocationStatus = Active` + token `Version`, bukan unique polos (riwayat pembatalan tetap tersimpan — `ARCH-BD-POS-03`).

### `BbkCompatibilityEvidence`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — |
| `BloodUnitId` | `Guid` | Ya | — | Index | FK `BbkBloodUnit` | Kantong |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | **Terikat pasangan kantong+pasien** |
| `CheckedByUserId` | `Guid` | Ya | — | — | — | Petugas yang menyatakan selesai |
| `CheckedAt` | `DateTime` | Ya | — | Index | — | Dasar hitung masa berlaku |
| `IsSuperseded` | `bool` | Ya | `false` | — | — | Gugur saat pengalihan (`DEC-BD-028`) |
| `SupersededReason` | `string(200)?` | Tidak | — | — | — | Mis. "kantong dialihkan" |

> Masa berlaku **tidak** disimpan; dihitung `CheckedAt + MstBloodComponent.CompatibilityEvidenceValidityHours` saat gerbang (`ARCH-BD-POS-01`).

### `BbkEmergencyAuthorization`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | — |
| `BloodUnitId` | `Guid` | Ya | Index | FK `BbkBloodUnit` | Kantong |
| `PatientId` | `Guid` | Ya | Index | FK `MstPatient` | Pasien tujuan |
| `AuthorizedByUserId` | `Guid` | Ya | — | — | Peran berwenang (`DEF-BD-004`) |
| `AuthorizedAt` | `DateTime` | Ya | — | — | — |
| `ReasonCode` | `string(30)` | Ya | — | FK `MstBloodBankReason.ReasonCode` | Alasan wajib |
| `ReasonNote` | `string(500)?` | Tidak | — | — | Salinan teks |

### `BbkIssuanceCorrection`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | Append-only |
| `BloodUnitId` | `Guid` | Ya | Index | FK `BbkBloodUnit` | Menunjuk pemberian pada kantong ini |
| `WhatWasWrong` | `string(500)` | Ya | — | — | Apa yang keliru dicatat |
| `WhatIsCorrect` | `string(500)` | Ya | — | — | Apa yang benar |
| `ReasonCode` | `string(30)` | Ya | — | FK `MstBloodBankReason.ReasonCode` | Alasan terkendali |
| `CorrectedByUserId` | `Guid` | Ya | — | — | Peran berwenang (`DEF-BD-004`) |
| `CorrectedAt` | `DateTime` | Ya | — | — | — |

### `BbkBloodGroupExam`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | Tidak | — |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | Tidak | Pasien |
| `AboRhesusResult` | `int?` (`BloodType`) | Tidak | — | — | — | **Ya** | Hasil; kosong sebelum dicatat |
| `ExamStatus` | `int` (`BbkBloodGroupExamStatus`) | Ya | `SampleTaken` | Index | — | Tidak | Status |
| `ExaminedByUserId` | `Guid?` | Tidak | — | — | — | Tidak | Pemeriksa |
| `ExaminedAt` | `DateTime?` | Tidak | — | — | — | Tidak | Waktu pemeriksaan |
| `ValidatedByUserId` | `Guid?` | Tidak | — | — | — | Tidak | Validator (`DEF-BD-004`) |
| `ValidatedAt` | `DateTime?` | Tidak | — | — | — | Tidak | — |
| `IsValidResult` | `bool` | Ya | `false` | Index | — | Tidak | Hasil sah yang berlaku |
| `IsConflictHeld` | `bool` | Ya | `false` | Index | — | Tidak | Sedang bertentangan (`DEC-BD-026`) |
| `Version` | `int` | Ya | `0` | — | — | Tidak | Token |

### `BbkBloodGroupSample`

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | — | Tidak | — |
| `BloodGroupExamId` | `Guid` | Ya | Index | FK `BbkBloodGroupExam` | Tidak | Induk pemeriksaan |
| `SampleIdentifier` | `string(50)` | Ya | Unique | — | **Ya** | Identifier sampel internal |
| `TakenByUserId` | `Guid` | Ya | — | — | Tidak | Petugas pengambil |
| `TakenAt` | `DateTime` | Ya | — | — | Tidak | Waktu |

### `BbkBloodGroupConflictResolution`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | Append-only |
| `PatientId` | `Guid` | Ya | Index | FK `MstPatient` | Konflik milik pasien |
| `ResolvingExamId` | `Guid` | Ya | Index | FK `BbkBloodGroupExam` | **Wajib** — pemeriksaan ulang yang memutus (`DEC-BD-031`) |
| `ResolvedByUserId` | `Guid` | Ya | — | — | Validator |
| `ReasonCode` | `string(30)` | Ya | — | FK `MstBloodBankReason.ReasonCode` | Alasan |
| `ResolvedAt` | `DateTime` | Ya | — | — | — |

### `BbkBloodBankProcedure`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | — |
| `ProcedureNumber` | `string(30)` | Ya | Unique | — | Number-series |
| `BloodOrderId` | `Guid` | Ya | Index | FK `BbkBloodOrder` | Order |
| `ServiceUnitId` | `Guid` | Ya | Index | FK `MstServiceUnit` | Unit |
| `BdrsDoctorId` | `Guid` | Ya | Index | FK `MstDoctor` | Dokter BDRS |
| `PerformedByUserId` | `Guid` | Ya | — | — | Petugas |
| `PatientClassId` | `Guid` | Ya | Index | FK `MstPatientClass` | Kelas |
| `ProcedureRefId` | `Guid` | Ya | Index | FK `MstProcedure` | Tindakan bertarif |
| `TariffId` | `Guid` | Ya | — | FK tarif | Tarif dirujuk |
| `ProcedureCodeSnapshot` | `string(50)` | Ya | — | — | Salinan kode |
| `ProcedureNameSnapshot` | `string(200)` | Ya | — | — | Salinan nama |
| `TariffAmountSnapshot` | `decimal(18,2)` | Ya | — | — | Salinan tarif (pola `BD-CAP-008`) |
| `ProcedureStatus` | `int` (`BbkProcedureStatus`) | Ya | Index | — | `Recorded`/`Completed` |

### `BbkTransitionHistory`

| Kolom | Tipe | Wajib | Index | Keterangan |
| --- | --- | :---: | --- | --- |
| `Id` | `Guid` | Ya | PK | Append-only |
| `Scope` | `string(30)` | Ya | Index | `BloodOrder`/`ProviderRequest`/`BloodUnit`/`BloodGroupExam` |
| `EntityId` | `Guid` | Ya | Index | Id entity terkait |
| `Action` | `string(50)` | Ya | — | Nama tindakan |
| `FromStatus` | `string(30)?` | Tidak | — | — |
| `ToStatus` | `string(30)` | Ya | — | — |
| `ReasonCode` | `string(30)?` | Tidak | — | FK `MstBloodBankReason.ReasonCode` |
| `ReasonNote` | `string(500)?` | Tidak | — | **Salinan teks** saat kejadian |
| `ActorUserId` | `Guid` | Ya | — | Pelaku |
| `OccurredAt` | `DateTime` | Ya | Index | — |
| `CorrelationId` | `Guid?` | Tidak | Index | Korelasi antar-proses |

### `MstBloodComponent`

| Kolom | Tipe | Wajib | Bawaan | Index | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — |
| `ComponentCode` | `string(20)` | Ya | — | Unique | Mis. `PRC`, `TC`, `FFP` |
| `ComponentName` | `string(100)` | Ya | — | — | Nama komponen |
| `CompatibilityEvidenceValidityHours` | `int?` | Tidak | `null` | — | Masa berlaku per komponen (`DEC-BD-032`); kosong → gerbang fail-closed (`VAL-BD-020b`) |
| `IsActive` | `bool` | Ya | `true` | — | — |

### `MstBloodBankReason`

| Kolom | Tipe | Wajib | Bawaan | Index | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — |
| `ReasonCode` | `string(30)` | Ya | — | Unique | Kode alasan |
| `ReasonText` | `string(200)` | Ya | — | — | Teks yang ditampilkan |
| `ReasonCategory` | `string(40)` | Ya | — | Index | `OrderCancellation`/`Emergency`/`PendingReviewResolution`/`Return`/`NotUsable`/`OverDelivery`/`AllocationCancellation`/`IssuanceCorrection` |
| `IsActive` | `bool` | Ya | `true` | — | Nonaktif tak mengubah makna riwayat lama (teks disalin) |

---

## 3. Tabel Diperbarui — kolom yang berubah

### `MstServiceUnit` (owner: HealthServices Master Data)

| Kolom | Tipe | Wajib | Bawaan | Keterangan |
| --- | --- | :---: | --- | --- |
| `IsAvailableForBloodOrder` | `bool` | Ya | `false` | **Kolom baru.** Bergaya `IsAvailableFor*` (`BD-CAP-005`). Bawaan menolak (`DEC-BD-012`) |

Kolom kunci existing yang dipakai aturan modul ini: `Id` (PK), `IsAvailableForRegistration` dan
kerabatnya (pola). Sumber lengkap: `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs`.

---

## 4. Skema DDL — tabel Baru dan Diperbarui

> ⚠️ Basis data dibentuk **EF Core Migrations**, bukan SQL manual. DDL berikut adalah **dokumentasi
> bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan dengan migration.
> Kolom audit `IdentityModel` tidak ditulis ulang di sini. Nama `Bbk*` masih placeholder (`BD-DEP-008`).

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."BbkBloodOrder" (
    "Id"                  uuid        NOT NULL,
    "OrderNumber"         varchar(30) NOT NULL,
    "PatientId"           uuid        NOT NULL,
    "EncounterId"         uuid        NOT NULL,
    "ServiceUnitId"       uuid        NOT NULL,
    "RequestingDoctorId"  uuid        NOT NULL,
    "OrderSource"         integer     NOT NULL,   -- enum HasConversion<int>
    "InputByUserId"       uuid,
    "OrderStatus"         integer     NOT NULL,   -- enum
    "Version"             integer     NOT NULL,
    CONSTRAINT "PK_BbkBloodOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BbkBloodOrder_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BbkBloodOrder_OrderNumber" ON public."BbkBloodOrder" ("OrderNumber");
CREATE INDEX "IX_BbkBloodOrder_PatientId_OrderStatus" ON public."BbkBloodOrder" ("PatientId", "OrderStatus");

CREATE TABLE public."BbkBloodUnit" (
    "Id"                          uuid        NOT NULL,
    "PmiBagNumber"                varchar(50) NOT NULL,   -- SENSITIF
    "ProviderRequestId"           uuid        NOT NULL,
    "ReceiptId"                   uuid        NOT NULL,
    "BloodComponentId"            uuid        NOT NULL,
    "IsExcess"                    boolean     NOT NULL,
    "UnitStatus"                  integer     NOT NULL,   -- enum
    "IssuedToPatientId"           uuid,
    "IssuedAt"                    timestamp,
    "IssuedByUserId"              uuid,
    "IssuedViaEmergency"          boolean     NOT NULL,
    "CompatibilityEvidenceIdUsed" uuid,
    "Version"                     integer     NOT NULL,
    CONSTRAINT "PK_BbkBloodUnit" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BbkBloodUnit_BbkProviderRequest_ProviderRequestId"
        FOREIGN KEY ("ProviderRequestId") REFERENCES public."BbkProviderRequest" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BbkBloodUnit_PmiBagNumber" ON public."BbkBloodUnit" ("PmiBagNumber");
CREATE INDEX "IX_BbkBloodUnit_UnitStatus" ON public."BbkBloodUnit" ("UnitStatus");

CREATE TABLE public."BbkBloodUnitAllocation" (
    "Id"                uuid        NOT NULL,
    "BloodUnitId"       uuid        NOT NULL,
    "BloodOrderLineId"  uuid        NOT NULL,
    "AllocationStatus"  integer     NOT NULL,   -- enum: 0 Active, 1 Cancelled
    "AllocatedByUserId" uuid        NOT NULL,
    "AllocatedAt"       timestamp   NOT NULL,
    "CancelReasonCode"  varchar(30),
    "CancelReasonNote"  varchar(500),
    "CancelledByUserId" uuid,
    "CancelledAt"       timestamp,
    CONSTRAINT "PK_BbkBloodUnitAllocation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BbkBloodUnitAllocation_BbkBloodUnit_BloodUnitId"
        FOREIGN KEY ("BloodUnitId") REFERENCES public."BbkBloodUnit" ("Id") ON DELETE RESTRICT
);
-- Satu alokasi aktif per kantong: unique parsial atas status Active saja
CREATE UNIQUE INDEX "IX_BbkBloodUnitAllocation_ActiveUnit"
    ON public."BbkBloodUnitAllocation" ("BloodUnitId") WHERE "AllocationStatus" = 0;

CREATE TABLE public."MstBloodComponent" (
    "Id"                                  uuid        NOT NULL,
    "ComponentCode"                       varchar(20) NOT NULL,
    "ComponentName"                       varchar(100) NOT NULL,
    "CompatibilityEvidenceValidityHours"  integer,                 -- konfigurasi per komponen
    "IsActive"                            boolean     NOT NULL,
    CONSTRAINT "PK_MstBloodComponent" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_MstBloodComponent_ComponentCode" ON public."MstBloodComponent" ("ComponentCode");

-- Tabel Diperbarui: penambahan satu kolom, aman tanpa downtime
ALTER TABLE public."MstServiceUnit"
    ADD COLUMN "IsAvailableForBloodOrder" boolean NOT NULL DEFAULT false;
```

Tabel `Bbk*` lain (`BbkBloodOrderLine`, `BbkProviderRequest`, `BbkBloodUnitReceipt`,
`BbkCompatibilityEvidence`, `BbkEmergencyAuthorization`, `BbkIssuanceCorrection`, `BbkBloodGroupExam`,
`BbkBloodGroupSample`, `BbkBloodGroupConflictResolution`, `BbkBloodBankProcedure`, `BbkTransitionHistory`,
`MstBloodBankReason`) mengikuti bentuk yang sama: PK `Id`, FK `ON DELETE RESTRICT`, enum `integer`,
kolom sensitif diberi komentar `-- SENSITIF`. Bentuk final diambil dari file Configuration masing-masing
saat implementasi.
