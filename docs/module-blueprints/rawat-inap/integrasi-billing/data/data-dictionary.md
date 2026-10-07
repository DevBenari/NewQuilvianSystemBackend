# Kamus Data (Data Dictionary) — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| Versi Dokumen | `1.0.0` (Draft) — 17 September 2026 |
| Target Database | PostgreSQL / Microsoft SQL Server |
| ORM Framework | Entity Framework Core 8 |

---

## 1. Konvensi Kolom Standar Entity Framework (Base Entity)

Seluruh entitas yang diturunkan dari `IdentityModel` atau `BaseAuditableEntity` pada sistem Quilvian mewarisi 10 kolom standar berikut. Kolom-kolom ini **tidak diulang** di setiap tabel di bawah ini:

| Nama Kolom | Tipe Data | Nullable | Sensitif | Keterangan |
|---|---|:---:|:---:|---|
| `Id` | `uuid` | Tidak | Tidak | Primary Key unik entitas (Clustered Index). |
| `CreatedBy` | `varchar(100)` | Tidak | Tidak | Nama akun pengguna yang membuat data pertama kali. |
| `CreatedAt` | `timestamp with time zone` | Tidak | Tidak | Waktu pembuatan data pertama kali (UTC). |
| `LastModifiedBy` | `varchar(100)` | Ya | Tidak | Nama akun pengguna yang terakhir mengubah data. |
| `LastModifiedAt` | `timestamp with time zone` | Ya | Tidak | Waktu perubahan terakhir data (UTC). |
| `IsDeleted` | `boolean` | Tidak | Tidak | Penanda soft-delete (default: `false`). |
| `DeletedBy` | `varchar(100)` | Ya | Tidak | Nama akun pengguna yang melakukan soft-delete. |
| `DeletedAt` | `timestamp with time zone` | Ya | Tidak | Waktu soft-delete data (UTC). |
| `RowVersion` | `bytea` / `rowversion` | Ya | Tidak | Penanda kontrol konkurensi optimistik (*Optimistic Concurrency*). |

> **Penanda Kolom Sensitif:** Kolom yang ditandai **Sensitif = Ya** tidak boleh dicatat dalam plain-text log, custom audit logger, atau dijadikan payload contoh publik untuk mencegah kebocoran data privasi medis atau kredensial.

---

## 2. Tabel Baru: `InpIntegrationOutboxes`

- **Status:** `Baru`
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Menampung seluruh pesan integrasi asinkronus ke modul Billing secara transaksional sebelum dikirimkan oleh background worker, menjamin prinsip *at-least-once delivery* dan *idempotency*.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `IdempotencyKey` | `varchar(255)` | Tidak | Tidak | **Unique Index:** `UQ_InpIntegrationOutbox_IdempotencyKey` | Kunci keunikan gabungan pesan. Format baku: `SourceDomain:SourceType:SourceDetailId:Version`. Mencegah duplikasi charge di kasir. |
| `SourceDomain` | `varchar(50)` | Tidak | Tidak | Index biasa | Domain pembuat event, e.g. `'INPATIENT'`. |
| `SourceType` | `varchar(50)` | Tidak | Tidak | Index biasa | Tipe entitas pelayanan: `'ADMISSION'`, `'ROOM_STAY'`, `'DISCHARGE'`. |
| `SourceDetailId` | `varchar(100)` | Tidak | Tidak | Index biasa | ID unik baris entitas pelayanan terkait (e.g. `AdmissionId` atau `PlacementId`). |
| `EventType` | `varchar(100)` | Tidak | Tidak | Index biasa | Jenis event bisnis: `ADMISSION_CONFIRMED`, `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED`. |
| `PayloadJson` | `text` / `jsonb` | Tidak | **Ya** | — | Konten data payload event terstruktur dalam format JSON. |
| `Status` | `integer` | Tidak | Tidak | **Filtered Index:** `IX_InpOutbox_Status_Pending` (`WHERE Status IN (0, 3)`) | Nilai enum: `0` = Pending, `1` = Processing, `2` = Published, `3` = Failed, `4` = DeadLetter. |
| `RetryCount` | `integer` | Tidak | Tidak | — | Jumlah percobaan pengiriman yang sudah dilakukan (default: `0`). |
| `NextRetryAtUtc` | `timestamp with time zone` | Ya | Tidak | Index biasa | Jadwal percobaan pengiriman berikutnya dihitung via formula exponential backoff. |
| `PublishedAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu saat pesan berhasil dikonfirmasi (*ACK*) oleh penerima. |
| `LastError` | `text` | Ya | Tidak | — | Pesan galat teknis terakhir bila pengiriman gagal. |
| `CreatedAtUtc` | `timestamp with time zone` | Tidak | Tidak | Index biasa | Waktu pencatatan pesan ke antrean outbox (UTC). |

---

## 3. Tabel Diperbarui: `InpBedPlacements`

- **Status:** `Diperbarui` (penambahan kolom penunjang standardisasi occupancy)
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Mencatat riwayat hunian tempat tidur pasien secara presisi, membedakan jam penempatan awal dan jam kepergian fisik.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `PhysicallyLeftAt` | `timestamp with time zone` | Ya | Tidak | — | Waktu pasien meninggalkan ruangan kamar tidur secara fisik nyata. Menjadi dasar pematokan `OccupancyEndAt`. |
| `Version` | `integer` | Tidak | Tidak | — | Nomor versi hunian tempat tidur (default: `1`). Naik setiap terjadi koreksi data kamar. |
| `ChangeReason` | `text` | Ya | Tidak | — | Alasan mutasi kamar atau koreksi admisi. Wajib diisi jika terjadi koreksi data. |
| `IsSuperseded` | `boolean` | Tidak | Tidak | Index biasa | Bernilai `true` jika baris ini telah digantikan oleh versi koreksi yang lebih baru (tidak pernah dihapus fisik). |
| `SupersededAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu saat baris data ini digantikan oleh versi baru. |

---

## 4. Tabel Diperbarui: `InpDischargeRequests`

- **Status:** `Diperbarui` (penambahan kolom pelacakan clearance & override)
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Mengelola status izin pulang medis dokter DPJP, persetujuan kasir, dan wewenang pelepasan fisik pasien.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `ClearanceStatus` | `integer` | Tidak | Tidak | Index biasa | Nilai enum: `0` = None, `1` = Pending, `2` = Cleared, `3` = Revoked, `4` = Overridden. |
| `ClearanceRevokedReason` | `text` | Ya | Tidak | — | Alasan kasir mencabut persetujuan clearance (misal tagihan susulan). |
| `IsSupervisorOverridden` | `boolean` | Tidak | Tidak | Index biasa | Bernilai `true` jika pemulangan pasien disahkan melalui jalur *Supervisor Override*. |
| `SupervisorOverrideReason` | `text` | Ya | Tidak | — | Alasan wajib kedaruratan klinis / evakuasi ambulans rujukan saat melakukan override. |
| `SupervisorOverriddenByUserId`| `uuid` | Ya | Tidak | FK ke User | ID akun supervisor bangsal yang mengeksekusi override darurat. |
| `SupervisorOverriddenAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu presisi pelaksanaan supervisor override (UTC). |

---

## 5. Tabel Referensi: Modul `BillingManagement` (Hanya Dibaca)

- **Status:** `Sudah ada` (milik modul tetangga)
- **Modul Pemilik:** `BillingManagement`
- **Catatan:** Modul Rawat Inap **TIDAK MEMILIKI** hak tulis atau wewenang skema atas tabel ini. Hanya dibaca melalui kontrak DTO REST.

| Nama Tabel / Entitas | Modul Asal | Kolom Kunci yang Dirujuk | Keterangan Pemakaian |
|---|---|---|---|
| `BillingFolio` / `BilInvoice` | `BillingManagement` | `FolioId`, `EncounterId`, `FolioStatus` (`OPEN`, `CLOSED`) | Memvalidasi apakah tagihan pasien masih berstatus `OPEN` sebelum mengizinkan mutasi kamar. |
| `BilFinancialClearance` | `BillingManagement` | `ClearanceId`, `EncounterId`, `Status`, `BlockerReasons` | Membaca status persetujuan kasir dan daftar kendala tanpa membaca nominal uang. |

---

## 6. Amandemen kontrak `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

### 6.1 Kepala

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan melalui `IsDelete`.

Bagian 1 s.d. 5 di atas tetap sebagai jejak. Bila berbeda, bagian ini yang berlaku: bagian 1 s.d. 5 memakai nama tabel dan schema yang tidak sesuai source (misalnya `dbo`), sedangkan bagian ini diambil dari model dan configuration pada backend `c8e99ce5`.

### 6.2 Status dan kepemilikan tabel

| Entity | Status | Pemilik | Catatan |
|---|---|---|---|
| `InpEpisode` | **Diperbarui** | `InPatientManagement` (`episode-rawat-inap`) | Empat kolom pengamatan kasir; enam kolom dipensiunkan |
| `InpBedPlacement` | **Diperbarui** | `InPatientManagement` (`episode-rawat-inap`) | Dua kolom rantai koreksi |
| `InpIntegrationOutboxes` | **Diperbarui** | `InPatientManagement` (`integrasi-billing`) | Empat kolom pemrosesan dan putar ulang |
| `BilInvoice` | **Diperbarui** | `BillingManagement` | Enam kolom "perlu diperiksa" |
| `BilInpatientEventReceipt` | **Baru** | `BillingManagement` | Tanda terima event Rawat Inap |
| `BilInvoiceEncounterLink` | **Baru** | `BillingManagement` | Tautan kunjungan asal ke invoice `RANAP` (`RWI-DEC-207`) — 6.9 |
| `InpAdmissionReferral` | Sudah ada (dirancang `episode-rawat-inap` `0.10.0` kamus data 19.8) | `InPatientManagement` | Dibaca Billing: `Id`, `SourceEncounterId`, `CompletedEpisodeId`, `Status` |
| `BilInpatientClearanceHandoff` | Sudah ada | `BillingManagement` | Satu-satunya sumber status kasir. Kolom kunci: `EncounterId`, status, `EvaluatedAt`. Model: `Areas/HealthServices/BillingManagement/Billing/Models/BilInpatientClearanceHandoff.cs` |
| `InpFinancialClearance` | Sudah ada | `InPatientManagement` | **Baca saja** sejak kontrak ini. Kolom kunci: `EpisodeId`, `SequenceNumber`, `ClearanceStatus`. Model: `.../InPatientManagement/Models/InpFinancialClearance.cs` |
| `MstRoomChargePolicy`, `MstTariff` | Sudah ada | `MasterData` | Dibaca Billing untuk tarif kamar |

### 6.3 `InpEpisode` — `Diperbarui`

`[Table("InpEpisode", Schema = "public")]`. Model `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | Nomor episode |
| `EncounterId` | `Guid` | Ya | — | Unique | FK `RegPatientEncounter` | `Restrict` | Tidak | Satu episode per kunjungan; kunci baca status kasir |
| `PatientId` | `Guid` | Ya | — | Index; unique parsial `IX_InpEpisode_PatientId_Present` | FK `MstPatient` | `Restrict` | Tidak | Satu episode hadir per pasien |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | — |
| `PatientClassId` | `Guid` | Ya | — | Index | FK `MstPatientClass` | `Restrict` | Tidak | — |
| `EpisodeStatus` | `InpEpisodeStatus` (`int`) | Ya | `Draft` | Index | — | — | Tidak | Lima nilai; tidak berubah |
| `AdmittedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | — |
| `DischargeDecidedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | — |
| `PhysicallyLeftAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Waktu keluar ruangan; akhir masa hunian (`RWI-DEC-159`) |
| `PhysicallyLeftByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Pencatat keluar ruangan |
| **`DepartureClearanceObserved`** | `InpClearanceObservation?` (`int`) | Tidak | — | **Index baru** | — | — | Tidak | **Baru.** Status kasir yang dibaca saat keluar ruangan, atau `Unreadable` |
| **`DepartureClearanceObservedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu bacaan |
| **`DepartureClearanceWarningAcknowledged`** | `bool` | Ya | `false` | — | — | — | Tidak | **Baru.** `true` bila pencatat mengakui peringatan kasir |
| **`ClosureClearanceObserved`** | `InpClearanceObservation?` (`int`) | Tidak | — | — | — | — | Tidak | **Baru.** Status kasir saat penutupan; `Cleared` pada penutupan normal |
| `MotherEpisodeId` | `Guid?` | Tidak | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `RequiresIsolation` | `bool` | Ya | `false` | Index | — | — | Tidak | — |
| `IsolationSource` | `InpIsolationSource?` (`int`) | Tidak | — | — | — | — | Tidak | — |
| `IsolationSetByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `IsolationSetByDoctorId` | `Guid?` | Tidak | — | Index | FK `MstDoctor` | `Restrict` | Tidak | — |
| `IsolationSetAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `IsolationNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Catatan klinis |
| `ClosedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | — |
| `DischargeType` | `InpDischargeType` (`int`) | Ya | `Unknown` | — | — | — | Tidak | — |
| `IsClosedWithoutFinancialClearance` | `bool` | Ya | `false` | Index | — | — | Tidak | Ditulis penutupan override |
| `ClosedWithoutClearanceReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan override |
| `ClearanceStatus` | `BillingClearanceStatus` (`int`) | Ya | `None` | — | — | — | Tidak | **Dipensiunkan** — tidak ditulis, tidak dibaca |
| `ClearanceRevokedReason` | `string(1000)?` | Tidak | — | — | — | — | Tidak | **Dipensiunkan** |
| `IsSupervisorOverridden` | `bool` | Ya | `false` | — | — | — | Tidak | **Dipensiunkan** |
| `SupervisorOverrideReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | **Dipensiunkan** |
| `SupervisorOverriddenByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **Dipensiunkan** |
| `SupervisorOverriddenAtUtc` | `DateTime?` | Tidak | — | — | — | — | Tidak | **Dipensiunkan** |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `Notes` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

Konkurensi: penutupan dan keluar ruangan memakai pemeriksaan status di dalam transaksi; tidak ada kolom versi baru.

### 6.4 `InpBedPlacement` — `Diperbarui`

`[Table("InpBedPlacement", Schema = "public")]`. Model `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index; unique `(EpisodeId, SequenceNumber)` | FK `InpEpisode` | `Restrict` | Tidak | — |
| `BedId` | `Guid` | Ya | — | Index; unique parsial aktif `IX_InpBedPlacement_BedId_Active` (`EndDateTime IS NULL`) | FK `MstBed` | `Restrict` | Tidak | — |
| `RoomId` | `Guid` | Ya | — | Index | FK `MstRoom` | `Restrict` | Tidak | — |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK `MstServiceUnit` | `Restrict` | Tidak | — |
| `PatientClassId` | `Guid` | Ya | — | Index | FK `MstPatientClass` | `Restrict` | Tidak | — |
| `SequenceNumber` | `int` | Ya | — | Bagian unique | — | — | Tidak | Baris koreksi mendapat nomor urut berikutnya |
| `StartDateTime` | `DateTime` | Ya | `DateTime.UtcNow` | Index | — | — | Tidak | — |
| `EndDateTime` | `DateTime?` | Tidak | — | Index | — | — | Tidak | — |
| `EndReason` | `InpBedPlacementEndReason?` (`int`) | Tidak | — | — | — | — | Tidak | — |
| `TransferReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Dibaca laporan transfer |
| `PhysicallyLeftAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | Dipakai pemeriksaan versi koreksi |
| `ChangeReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan koreksi |
| `IsSuperseded` | `bool` | Ya | `false` | Index | — | — | Tidak | Tetap berarti "diakhiri transfer". **Bukan** saringan tarif |
| `SupersededAtUtc` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| **`CorrectsPlacementId`** | `Guid?` | Tidak | — | Index | FK `InpBedPlacement` (diri sendiri) | `Restrict` | Tidak | **Baru.** Pada baris hasil koreksi: baris yang dikoreksinya |
| **`SupersededByCorrectionId`** | `Guid?` | Tidak | — | **Index** | FK `InpBedPlacement` (diri sendiri) | `Restrict` | Tidak | **Baru.** Pada baris yang dikoreksi: baris penggantinya. Billing **mengecualikan** baris yang kolom ini terisi (`INV-RWF-07`) |
| `PlacedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `EndedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

**Catatan unique aktif per bed.** Bila koreksi mengganti bed pada penempatan yang masih berjalan, baris lama diberi `EndDateTime` = waktu mulai yang dikoreksi dan `SupersededByCorrectionId`; baris baru berjalan pada bed yang benar. Unique parsial per bed tetap terjaga.

### 6.5 `InpIntegrationOutboxes` — `Diperbarui`

`[Table("InpIntegrationOutboxes", Schema = "public")]` — nama jamak adalah utang teknis yang dicatat, tidak dirapikan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `IdempotencyKey` | `string(255)` | Ya | — | Unique `UQ_InpIntegrationOutbox_IdempotencyKey` | — | — | Tidak | `INPATIENT:<SourceType>:<SourceId>:<Version>` |
| `SourceDomain` | `string(50)` | Ya | `"INPATIENT"` | — | — | — | Tidak | — |
| `SourceType` | `string(50)` | Ya | — | — | — | — | Tidak | `ADMISSION`, `ROOM_STAY`, `DISCHARGE` |
| `SourceDetailId` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `EventType` | `string(100)` | Ya | — | — | — | — | Tidak | Empat event pada `integration-contract.md` 4.2 |
| `PayloadJson` | `text` | Ya | — | — | — | — | Tidak | Hanya daftar putih `INV-RWF-05` |
| `Status` | `OutboxStatus` (`int`) | Ya | `Pending` | Index gabungan baru `(Status, ProcessingStartedAtUtc)` | — | — | Tidak | — |
| `RetryCount` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `NextRetryAtUtc` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `PublishedAtUtc` | `DateTime?` | Tidak | — | — | — | — | Tidak | Diisi hanya setelah tanda terima |
| `LastError` | `text?` | Tidak | — | — | — | — | Tidak | Pesan teknis; tanpa data klinis |
| `CreatedAtUtc` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | — |
| **`ProcessingStartedAtUtc`** | `DateTime?` | Tidak | — | Bagian index baru | — | — | Tidak | **Baru.** Awal masa sewa pemrosesan |
| **`AcknowledgedReceiptId`** | `Guid?` | Tidak | — | — | Rujukan logis ke `BilInpatientEventReceipt.Id` (tanpa FK lintas modul) | — | Tidak | **Baru** |
| **`ReplayBatchId`** | `Guid?` | Tidak | — | Index | — | — | Tidak | **Baru** |
| **`ReplayedAtUtc`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru** |

### 6.6 `BilInvoice` — `Diperbarui` (milik Billing)

`[Table("BilInvoice", Schema = "public")]`. Model `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | **Unique** | — | — | Tidak | Satu invoice per kunjungan (`INV-RWF-06`) |
| `InvoiceNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | — |
| `ServiceType` | `string(30)` | Ya | — | — | — | — | Tidak | Rawat inap selalu `"RANAP"` (`FIN-CON-01`) |
| `Status` | `string(30)` | Ya | `OPEN` | — | — | — | Tidak | `OPEN`, `FINAL`, `CLOSED`, `SETTLED_BY_WRITE_OFF` |
| `CurrentCalculationVersion` | `int` | Ya | — | — | — | — | Tidak | — |
| `InvoiceDate` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `ClosedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Konkurensi optimistis |
| **`RequiresReview`** | `bool` | Ya | `false` | **Index parsial** `WHERE "RequiresReview"` | — | — | Tidak | **Baru** |
| **`ReviewReasonCode`** | `string(50)?` | Tidak | — | — | — | — | Tidak | **Baru.** `MANUAL_AND_AUTOMATIC_ROOM_CHARGE` |
| **`ReviewFlaggedAt`** | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`ReviewResolvedAt`** | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`ReviewResolvedByUserId`** | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | **Baru** |
| **`ReviewResolutionNote`** | `string(500)?` | Tidak | — | — | — | — | **Ya** | **Baru** |

### 6.7 `BilInpatientEventReceipt` — `Baru` (milik Billing)

`[Table("BilInpatientEventReceipt", Schema = "public")]`. Lokasi `Areas/HealthServices/BillingManagement/Billing/Models/BilInpatientEventReceipt.cs`; configuration `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInpatientEventReceiptConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Dikembalikan sebagai `ReceiptId` |
| `IdempotencyKey` | `string(255)` | Ya | — | **Unique** | — | — | Tidak | Sama dengan kunci outbox |
| `EventType` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `EpisodeId` | `Guid` | Ya | — | Index | Rujukan logis, tanpa FK lintas modul | — | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | Index | — | — | Tidak | — |
| `SourceId` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `SourceVersion` | `int` | Ya | — | — | — | — | Tidak | — |
| `OccurredAtUtc` | `DateTime` | Ya | — | — | — | — | Tidak | Waktu kejadian di Rawat Inap |
| `ReceivedAtUtc` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | — |
| `Outcome` | `string(50)` | Ya | — | — | — | — | Tidak | Kosakata pada `02-backend-architecture.md` 9.7 |
| `InvoiceId` | `Guid?` | Tidak | — | — | FK `BilInvoice` | `Restrict` | Tidak | — |
| `CalculationVersionNo` | `int?` | Tidak | — | — | — | — | Tidak | Versi hitungan yang dibuat |
| `Message` | `string(500)?` | Tidak | — | — | — | — | Tidak | Tanpa data klinis |

### 6.8 Skema DDL

> **Peringatan.** Basis data dibentuk EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang. Untuk tabel `Diperbarui`, hanya kolom baru beserta index-nya yang ditulis; bentuk kolom lama mengikuti configuration yang sudah ada.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
ALTER TABLE public."InpEpisode"
    ADD "DepartureClearanceObserved"            integer,
    ADD "DepartureClearanceObservedAt"          timestamp,
    ADD "DepartureClearanceWarningAcknowledged" boolean NOT NULL DEFAULT false,
    ADD "ClosureClearanceObserved"              integer;
CREATE INDEX "IX_InpEpisode_DepartureClearanceObserved"
    ON public."InpEpisode" ("DepartureClearanceObserved");

ALTER TABLE public."InpBedPlacement"
    ADD "CorrectsPlacementId"      uuid,
    ADD "SupersededByCorrectionId" uuid,
    ADD CONSTRAINT "FK_InpBedPlacement_InpBedPlacement_CorrectsPlacementId"
        FOREIGN KEY ("CorrectsPlacementId") REFERENCES public."InpBedPlacement" ("Id") ON DELETE RESTRICT,
    ADD CONSTRAINT "FK_InpBedPlacement_InpBedPlacement_SupersededByCorrectionId"
        FOREIGN KEY ("SupersededByCorrectionId") REFERENCES public."InpBedPlacement" ("Id") ON DELETE RESTRICT;
CREATE INDEX "IX_InpBedPlacement_CorrectsPlacementId" ON public."InpBedPlacement" ("CorrectsPlacementId");
CREATE INDEX "IX_InpBedPlacement_SupersededByCorrectionId" ON public."InpBedPlacement" ("SupersededByCorrectionId");

ALTER TABLE public."InpIntegrationOutboxes"
    ADD "ProcessingStartedAtUtc" timestamp,
    ADD "AcknowledgedReceiptId"  uuid,
    ADD "ReplayBatchId"          uuid,
    ADD "ReplayedAtUtc"          timestamp;
CREATE INDEX "IX_InpIntegrationOutbox_Status_ProcessingStartedAtUtc"
    ON public."InpIntegrationOutboxes" ("Status", "ProcessingStartedAtUtc");
CREATE INDEX "IX_InpIntegrationOutbox_ReplayBatchId" ON public."InpIntegrationOutboxes" ("ReplayBatchId");

ALTER TABLE public."BilInvoice"
    ADD "RequiresReview"         boolean NOT NULL DEFAULT false,
    ADD "ReviewReasonCode"       varchar(50),
    ADD "ReviewFlaggedAt"        timestamptz,
    ADD "ReviewResolvedAt"       timestamptz,
    ADD "ReviewResolvedByUserId" uuid,
    ADD "ReviewResolutionNote"   varchar(500);   -- SENSITIF
CREATE INDEX "IX_BilInvoice_RequiresReview" ON public."BilInvoice" ("RequiresReview") WHERE "RequiresReview";

CREATE TABLE public."BilInpatientEventReceipt" (
    "Id"                   uuid         NOT NULL,
    "IdempotencyKey"       varchar(255) NOT NULL,
    "EventType"            varchar(100) NOT NULL,
    "EpisodeId"            uuid         NOT NULL,
    "EncounterId"          uuid         NOT NULL,
    "SourceId"             varchar(100) NOT NULL,
    "SourceVersion"        integer      NOT NULL,
    "OccurredAtUtc"        timestamp    NOT NULL,
    "ReceivedAtUtc"        timestamp    NOT NULL,
    "Outcome"              varchar(50)  NOT NULL,
    "InvoiceId"            uuid,
    "CalculationVersionNo" integer,
    "Message"              varchar(500),
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_BilInpatientEventReceipt" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilInpatientEventReceipt_BilInvoice_InvoiceId"
        FOREIGN KEY ("InvoiceId") REFERENCES public."BilInvoice" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BilInpatientEventReceipt_IdempotencyKey" ON public."BilInpatientEventReceipt" ("IdempotencyKey");
CREATE INDEX "IX_BilInpatientEventReceipt_EncounterId" ON public."BilInpatientEventReceipt" ("EncounterId");
CREATE INDEX "IX_BilInpatientEventReceipt_EpisodeId" ON public."BilInpatientEventReceipt" ("EpisodeId");
```

### 6.9 `BilInvoiceEncounterLink` — `Baru` (milik Billing) ★ 2 Oktober 2026

Penyelarasan decision log revision `31` (`RWI-DEC-207`). Kolom warisan `IdentityModel` tidak diulang.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `RanapInvoiceId` | `uuid` | Ya | — | Unique (`RanapInvoiceId`, `LinkedEncounterId`) | FK `BilInvoice` | `Restrict` | Tidak | Invoice `RANAP` episode |
| `LinkedEncounterId` | `uuid` | Ya | — | Index; bagian unique | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunjungan asal (poli/ODC) = `InpAdmissionReferral.SourceEncounterId` |
| `LinkReason` | `varchar(40)` | Ya | — | — | — | — | Tidak | `SURGERY_ORIGIN`. Nilai `EMERGENCY_TRANSFER` dicadangkan untuk `BKC-DEC-118`, tidak dipakai kontrak ini |
| `SourceReferralId` | `uuid` | Tidak | `null` | — | FK `InpAdmissionReferral` | `Restrict` | Tidak | Asal tautan untuk `SURGERY_ORIGIN` |
| `ReceiptId` | `uuid` | Ya | — | — | FK `BilInpatientEventReceipt` | `Restrict` | Tidak | Tanda terima `ADMISSION_CONFIRMED` yang membuat tautan |
| `LinkedAt` | `timestamp with time zone` | Ya | — | — | — | — | Tidak | — |

Tautan tidak pernah dihapus oleh alur normal; koreksi tautan yang salah adalah aturan internal Billing.

```sql
-- I6 — BillingManagement (sesudah E6 episode-rawat-inap)
CREATE TABLE public."BilInvoiceEncounterLink" (
    "Id" uuid PRIMARY KEY,
    "RanapInvoiceId" uuid NOT NULL REFERENCES public."BilInvoice" ("Id") ON DELETE RESTRICT,
    "LinkedEncounterId" uuid NOT NULL REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    "LinkReason" varchar(40) NOT NULL,
    "SourceReferralId" uuid NULL REFERENCES public."InpAdmissionReferral" ("Id") ON DELETE RESTRICT,
    "ReceiptId" uuid NOT NULL REFERENCES public."BilInpatientEventReceipt" ("Id") ON DELETE RESTRICT,
    "LinkedAt" timestamp with time zone NOT NULL);
CREATE UNIQUE INDEX "UX_BilInvoiceEncounterLink_Invoice_Encounter" ON public."BilInvoiceEncounterLink" ("RanapInvoiceId", "LinkedEncounterId") WHERE NOT "IsDelete";
CREATE INDEX "IX_BilInvoiceEncounterLink_LinkedEncounterId" ON public."BilInvoiceEncounterLink" ("LinkedEncounterId");
```
