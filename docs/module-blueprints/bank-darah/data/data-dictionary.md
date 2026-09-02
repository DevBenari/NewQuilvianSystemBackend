# Kamus Data — Bank Darah

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — `draft` |
| `last_changed_in` | `v4` |
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
| `BbkBloodUnit`, `BbkBloodUnitAllocation`, `BbkCompatibilityEvidence`, `BbkEmergencyAuthorization`, `BbkIssuanceCorrection` | Baru | Bank Darah | `v2`: `BbkBloodUnit` +`CurrentPlacementId`, `BbkEmergencyAuthorization` +`BypassScope`. **`v3`**: `BbkEmergencyAuthorization` +`AuthorizerRole`/`EmergencyConditionNote`; `BbkIssuanceCorrection` memperoleh lifecycle dua tahap |
| `BbkBloodUnitPlacement` | **Baru pada `v2`** | Bank Darah | Riwayat penempatan kantong, append-only (`BD-DOM-25`, `DEC-BD-036`) |
| `BbkBloodGroupExam`, `BbkBloodGroupSample`, `BbkBloodGroupConflictResolution` | Baru | Bank Darah | — |
| `BbkBloodBankProcedure` | Baru | Bank Darah | Tanpa penyaluran charge (`DEC-BD-016`) |
| `BbkTransitionHistory` | Baru | Bank Darah | Append-only |
| `MstBloodComponent`, `MstBloodBankReason` | Baru | Bank Darah (master) | Setup MVP |
| `MstBloodStorageLocation` | **Baru pada `v2`** | Bank Darah (master) | Master ketiga Setup MVP (`BD-DOM-24`, `DEC-BD-035`). Prasyarat go-live |
| `MstDrugStorageLocation` | Sudah ada | HealthServices Master Data (Farmasi) | **Tidak dipakai dan tidak disentuh.** Ditolak sebagai kandidat pakai-ulang oleh `DEC-BD-035` |
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
| `ProviderRequestId` | `Guid` | Ya | — | Index | FK `BbkProviderRequest` | `Restrict` | Tidak | Asal — tak pernah putus |
| `ReceiptId` | `Guid` | Ya | — | Index | FK `BbkBloodUnitReceipt` | `Restrict` | Tidak | Penerimaan pelahir |
| `BloodComponentId` | `Guid` | Ya | — | Index | FK `MstBloodComponent` | `Restrict` | Tidak | Komponen |
| `IsExcess` | `bool` | Ya | `false` | — | — | — | Tidak | Kantong berlebih (`DEC-BD-025`) |
| `UnitStatus` | `int` (`BbkBloodUnitStatus`) | Ya | **`Received`** | Index | — | — | Tidak | Status kantong. Bawaan berubah pada `v2`: kantong lahir **belum tersimpan** (`DEC-BD-036`) |
| `CurrentPlacementId` | `Guid?` | Tidak | `null` | Index | FK `BbkBloodUnitPlacement` | `Restrict` | Tidak | **Kolom baru `v2`.** Penunjuk ke penempatan yang sedang berlaku. `NULL` selama `Received`. **Tidak pernah disunting sendiri** — hanya berpindah bersama penambahan penempatan, dalam satu transaksi (`ARCH-BD-POS-05`) |
| `IssuedToPatientId` | `Guid?` | Tidak | — | Index | FK `MstPatient` | `Restrict` | **Ya** | Terisi saat `Issued` |
| `IssuedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemberian (terminal) |
| `IssuedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pelaku pemberian |
| `IssuedViaEmergency` | `bool` | Ya | `false` | — | — | — | Tidak | Penanda jalur darurat |
| `CompatibilityEvidenceIdUsed` | `Guid?` | Tidak | — | — | FK `BbkCompatibilityEvidence` | `Restrict` | Tidak | Bukti yang dipakai saat pemberian |
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
| `ValidatedByUserId` | `Guid` | Ya | — | Index | — | **Ganti nama dari `CheckedByUserId` pada `v4`.** Petugas BDRS berwenang validasi yang **menyatakan** hasilnya. Pelaksana pemeriksaan boleh orang lain, dan identitasnya **tidak** disimpan di sini — yang direkam adalah yang menyatakan (`DEC-BD-042`) |
| `EvidenceResult` | `int` (`BbkCompatibilityResult`) | Ya | — | Index | — | **Kolom baru `v4`.** `Compatible` atau `Incompatible`. Bukti bernilai `Incompatible` **tetap tersimpan** dan **tidak** membuka gerbang pemberian |
| `CheckedAt` | `DateTime` | Ya | — | Index | — | Dasar hitung masa berlaku |
| `IsSuperseded` | `bool` | Ya | `false` | — | — | Gugur saat pengalihan (`DEC-BD-028`) |
| `SupersededReason` | `string(200)?` | Tidak | — | — | — | Mis. "kantong dialihkan" |

> Masa berlaku **tidak** disimpan; dihitung `CheckedAt + MstBloodComponent.CompatibilityEvidenceValidityHours` saat gerbang (`ARCH-BD-POS-01`).

> **Gerbang pemberian memeriksa `EvidenceResult`, bukan sekadar keberadaan baris.** Sejak `v4` bukti
> yang menyatakan tidak cocok juga tersimpan; meloloskannya berarti memberikan darah yang sudah
> dinyatakan tidak cocok oleh manusia. Pengetatan ini penurunan dari `DEC-BD-042` dan menunggu
> penegasan pemilik proses — `OQ-BD-018`.

> **Kenapa nama kolom pelakunya berganti.** `CheckedByUserId` terbaca sebagai "yang memeriksa".
> `DEC-BD-042` menetapkan pelaksana pemeriksaan **boleh** berbeda dari validator, dan yang disimpan
> adalah **validator** — orang yang menyatakan hasilnya sah. Nama lama akan menyesatkan pembaca yang
> menyangka kolom ini berisi analis pelaksananya.

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
| `BypassScope` | `int` (`BbkEmergencyBypassScope`) | Ya | — | — | **Kolom baru `v2`.** Gerbang yang dilewati: `CompatibilityEvidence`, `InactiveStorageLocation`, atau `Both` (`INV-BD-030`, `DEC-BD-038`) |
| `AuthorizerRole` | `int` (`BbkEmergencyAuthorizerRole`) | Ya | — | Index | **Kolom baru `v3`.** Peran yang dipakai penerbit: `BloodBankDoctor` atau `AttendingPhysician`. Menjawab *dengan wewenang apa*, bukan *siapa* (`DEC-BD-040`, `INV-BD-032`) |
| `EmergencyConditionNote` | `string(500)` | Ya | — | — | **Kolom baru `v3`.** Keterangan keadaan klinis saat itu. **Wajib**, tidak boleh kosong. Berbeda dari `ReasonCode` yang terkendali — ini uraian keadaan, bukan kategori |

> Enum tiga nilai dipilih supaya keadaan "darurat yang tidak melewati gerbang apa pun" **tidak dapat
> ditulis sama sekali**. Dua kolom bool akan memungkinkan `(false, false)` yang tak bermakna.

### `BbkBloodUnitPlacement`

Riwayat penempatan kantong. **Append-only** (`INV-BD-026`) — tidak ada jalur bisnis yang boleh mengubah
atau menghapus barisnya, termasuk saat salah taruh; salah taruh diperbaiki dengan menambah penempatan
baru.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | — | Tidak | — |
| `BloodUnitId` | `Guid` | Ya | — | Index | FK `BbkBloodUnit` | `Restrict` | Tidak | Kantong yang ditempatkan |
| `StorageLocationId` | `Guid` | Ya | — | Index | FK `MstBloodStorageLocation` | `Restrict` | Tidak | Wajib menunjuk lokasi **aktif saat penempatan dibuat** (`INV-BD-027`) |
| `PreviousPlacementId` | `Guid?` | Tidak | `null` | Index | FK `BbkBloodUnitPlacement` | `Restrict` | Tidak | `NULL` pada penempatan pertama; terisi pada perpindahan — inilah satu-satunya pembeda keduanya |
| `PlacedAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Sejak kapan kantong ada di lokasi itu |
| `PlacedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | **Selalu manusia**; sistem tidak pernah menjadi pelaku perpindahan (`DEC-BD-037`) |
| `IsCurrent` | `bool` | Ya | `true` | Filtered unique | — | — | Tidak | Maks. satu `true` per kantong (`INV-BD-026`) |
| `Note` | `string(500)?` | Tidak | `null` | — | — | — | Tidak | Keterangan bebas. **Bukan** alasan terkendali — bukti yang disetujui tidak menuntut alasan pada perpindahan |

> Keunikan "satu penempatan berlaku" dijaga **filtered unique index** `(BloodUnitId) WHERE IsCurrent = true`,
> pola yang sama dengan alokasi aktif. Penempatan lama tetap tersimpan dengan `IsCurrent = false`.

> **Kenapa tidak ada `RemovedAt`.** Bukti yang disetujui tidak menuntut pencatatan saat kantong keluar
> dari lokasi. Rentang "sejak kapan sampai kapan" terbaca dari `PlacedAt` penempatan berikutnya, dan
> status terminal kantong (`ReturnedToProvider`/`NotUsable`) menjelaskan mengapa ia tidak lagi di stok.
> Menambah kolom yang tidak diminta berarti menuntut petugas mengisi data yang tidak ada aturannya.

### `BbkIssuanceCorrection`

| Kolom | Tipe | Wajib | Index | Relasi | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | PK | — | Append-only |
| `BloodUnitId` | `Guid` | Ya | Index | FK `BbkBloodUnit` | Menunjuk pemberian pada kantong ini |
| `WhatWasWrong` | `string(500)` | Ya | — | — | Apa yang keliru dicatat |
| `WhatIsCorrect` | `string(500)` | Ya | — | — | Apa yang benar |
| `ReasonCode` | `string(30)` | Ya | — | FK `MstBloodBankReason.ReasonCode` | Alasan terkendali |
| `SupportingEvidenceNote` | `string(1000)` | Ya | — | — | **Kolom baru `v3`.** Bukti pendukung berupa **keterangan tertulis** (`DEC-BD-041`). Lampiran berkas belum diputuskan — `OQ-BD-016` |
| `CorrectionStatus` | `int` (`BbkCorrectionStatus`) | Ya | `Requested` | Index | **Kolom baru `v3`.** `Requested` → `Approved` / `Rejected`. **Selama `Requested`, koreksi belum berlaku** (`INV-BD-033`) |
| `RequestedByUserId` | `Guid` | Ya | — | Index | **Ganti nama dari `CorrectedByUserId` pada `v3`.** Petugas BDRS yang mengajukan |
| `RequestedAt` | `DateTime` | Ya | — | — | **Ganti nama dari `CorrectedAt` pada `v3`.** Waktu pengajuan |
| `DecidedByUserId` | `Guid?` | Tidak | `null` | — | **Kolom baru `v3`.** Dokter BDRS yang memutuskan. **Wajib berbeda dari `RequestedByUserId`** (`DEC-BD-041`). Kosong selama `Requested` |
| `DecidedAt` | `DateTime?` | Tidak | `null` | — | **Kolom baru `v3`.** Waktu keputusan. Kosong selama `Requested` |
| `DecisionNote` | `string(500)?` | Tidak | `null` | — | **Kolom baru `v3`.** Keterangan pemutus. **Wajib diisi bila `Rejected`** — penolakan tanpa alasan tidak dapat ditinjau |

> **Satu pasang kolom pemutus, bukan dua.** `DecidedBy`/`DecidedAt` dipakai untuk menyetujui maupun
> menolak. Dua pasang terpisah (`ApprovedBy` + `RejectedBy`) memungkinkan baris yang punya penyetuju
> sekaligus penolak — keadaan yang tak bermakna dan tak mungkin terjadi di dunia nyata.

> **Koreksi yang ditolak tetap tersimpan** dengan `CorrectionStatus = Rejected`. Ia tidak dihapus dan
> tidak dibiarkan menggantung di `Requested`; fakta bahwa seseorang pernah menyatakan catatan itu keliru
> dan pemutus tidak sependapat justru bagian riwayat yang berguna saat ditinjau kemudian.

> **Angka pemenuhan order menyaring `CorrectionStatus = Approved`.** Menyertakan yang `Requested` akan
> membuat angka bergerak sebelum keputusan turun (`INV-BD-033`).

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

### `MstBloodStorageLocation`

Master lokasi penyimpanan darah milik BDRS. **Bukan** cold storage farmasi — lihat `MstDrugStorageLocation`
pada tabel status, yang sengaja tidak dipakai (`DEC-BD-035`).

| Kolom | Tipe | Wajib | Bawaan | Index | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — |
| `StorageLocationCode` | `string(30)` | Ya | — | Unique | Kode lokasi, mis. `KLK-BSR` |
| `StorageLocationName` | `string(150)` | Ya | — | — | Nama yang dikenali petugas, mis. `Kulkas Besar` |
| `IsActive` | `bool` | Ya | `true` | Index | **Penanda yang menutup dua gerbang** saat bernilai `false` (`INV-BD-027`, `INV-BD-028`). Dibaca saat gerbang dinilai, **tidak pernah disalin** ke kantong |
| `SortOrder` | `int` | Ya | `0` | — | Urutan tampil pilihan |
| `Description` | `string(250)?` | Tidak | `null` | — | Keterangan bebas |

> **Yang sengaja tidak ada di sini:** rentang suhu, kelembapan, kapasitas, rak/shelf/bin, hierarki
> induk-anak, dan seluruh penanda farmasi. Semuanya di luar scope MVP (`DEC-BD-035`), dan ketiadaannya
> disengaja — bukan kolom yang lupa dirancang.

> **Lokasi nonaktif tidak pernah dihapus.** Penempatan lama menunjuk ke sini lewat `Restrict`, dan
> riwayat kantong wajib tetap terbaca. Penonaktifan hanya mengubah `IsActive`.

### `MstBloodBankReason`

| Kolom | Tipe | Wajib | Bawaan | Index | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — |
| `ReasonCode` | `string(30)` | Ya | — | Unique | Kode alasan |
| `ReasonText` | `string(200)` | Ya | — | — | Teks yang ditampilkan |
| `ReasonCategory` | `string(40)` | Ya | — | Index | **Diperbarui `v4`.** `OrderCancellationClinical`/`OrderCancellationOperational` (menggantikan `OrderCancellation` tunggal — `DEC-BD-044`) / `Emergency` / `PendingReviewResolution` / `Return` / `NotUsable` / `OverDelivery` / `AllocationCancellation` / `IssuanceCorrection` / `CorrectionRejection` |

> **Kenapa pembatalan order punya dua kategori, bukan satu.** `DEC-BD-044` mengizinkan dua peran
> membatalkan order dengan sebab yang berbeda: dokter mencabut kebutuhan klinis, petugas BDRS
> merapikan kekeliruan operasional. Keduanya memakai **satu** butir hak akses `BloodOrder : Cancel`;
> yang membedakannya pada rekam adalah kategori alasannya. Tanpa pemisahan kategori, peninjau tidak
> dapat membedakan order yang dicabut karena pasiennya tidak jadi ditransfusi dari order yang dihapus
> karena salah input.
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
    "UnitStatus"                  integer     NOT NULL,   -- enum; bawaan Received (v2)
    "CurrentPlacementId"          uuid,                   -- v2; NULL selama Received
    "IssuedToPatientId"           uuid,
    "IssuedAt"                    timestamp,
    "IssuedByUserId"              uuid,
    "IssuedViaEmergency"          boolean     NOT NULL,
    "CompatibilityEvidenceIdUsed" uuid,
    "Version"                     integer     NOT NULL,
    CONSTRAINT "PK_BbkBloodUnit" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BbkBloodUnit_BbkProviderRequest_ProviderRequestId"
        FOREIGN KEY ("ProviderRequestId") REFERENCES public."BbkProviderRequest" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BbkBloodUnit_BbkBloodUnitPlacement_CurrentPlacementId"
        FOREIGN KEY ("CurrentPlacementId") REFERENCES public."BbkBloodUnitPlacement" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BbkBloodUnit_PmiBagNumber" ON public."BbkBloodUnit" ("PmiBagNumber");
CREATE INDEX "IX_BbkBloodUnit_UnitStatus" ON public."BbkBloodUnit" ("UnitStatus");
CREATE INDEX "IX_BbkBloodUnit_CurrentPlacementId" ON public."BbkBloodUnit" ("CurrentPlacementId");

-- v2. FK melingkar dengan BbkBloodUnit: CurrentPlacementId nullable, diisi saat penempatan pertama.
CREATE TABLE public."BbkBloodUnitPlacement" (
    "Id"                  uuid        NOT NULL,
    "BloodUnitId"         uuid        NOT NULL,
    "StorageLocationId"   uuid        NOT NULL,
    "PreviousPlacementId" uuid,                   -- NULL pada penempatan pertama
    "PlacedAt"            timestamp   NOT NULL,
    "PlacedByUserId"      uuid        NOT NULL,   -- selalu manusia
    "IsCurrent"           boolean     NOT NULL,
    "Note"                varchar(500),
    CONSTRAINT "PK_BbkBloodUnitPlacement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BbkBloodUnitPlacement_BbkBloodUnit_BloodUnitId"
        FOREIGN KEY ("BloodUnitId") REFERENCES public."BbkBloodUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BbkBloodUnitPlacement_MstBloodStorageLocation_StorageLocationId"
        FOREIGN KEY ("StorageLocationId") REFERENCES public."MstBloodStorageLocation" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BbkBloodUnitPlacement_BbkBloodUnitPlacement_PreviousPlacementId"
        FOREIGN KEY ("PreviousPlacementId") REFERENCES public."BbkBloodUnitPlacement" ("Id") ON DELETE RESTRICT
);
-- Satu penempatan berlaku per kantong: unique parsial, pola sama dengan alokasi aktif
CREATE UNIQUE INDEX "IX_BbkBloodUnitPlacement_CurrentUnit"
    ON public."BbkBloodUnitPlacement" ("BloodUnitId") WHERE "IsCurrent" = true;
CREATE INDEX "IX_BbkBloodUnitPlacement_StorageLocationId" ON public."BbkBloodUnitPlacement" ("StorageLocationId");
CREATE INDEX "IX_BbkBloodUnitPlacement_PlacedAt" ON public."BbkBloodUnitPlacement" ("PlacedAt");

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

-- v2. Master lokasi penyimpanan darah. Bersih dari atribut farmasi — bandingkan MstDrugStorageLocation
-- yang sengaja TIDAK dipakai (DEC-BD-035).
CREATE TABLE public."MstBloodStorageLocation" (
    "Id"                  uuid         NOT NULL,
    "StorageLocationCode" varchar(30)  NOT NULL,
    "StorageLocationName" varchar(150) NOT NULL,
    "IsActive"            boolean      NOT NULL,
    "SortOrder"           integer      NOT NULL,
    "Description"         varchar(250),
    CONSTRAINT "PK_MstBloodStorageLocation" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_MstBloodStorageLocation_StorageLocationCode"
    ON public."MstBloodStorageLocation" ("StorageLocationCode");
CREATE INDEX "IX_MstBloodStorageLocation_IsActive" ON public."MstBloodStorageLocation" ("IsActive");

-- v2/v3. Kolom tambahan pada BbkEmergencyAuthorization (tabel belum pernah dibuat; masuk CREATE):
--   "BypassScope"            integer      NOT NULL  -- 0 CompatibilityEvidence, 1 InactiveStorageLocation, 2 Both
--   "AuthorizerRole"         integer      NOT NULL  -- v3; 0 BloodBankDoctor, 1 AttendingPhysician
--   "EmergencyConditionNote" varchar(500) NOT NULL  -- v3; keadaan klinis, wajib
--
-- v3. Kolom pada BbkIssuanceCorrection (tabel belum pernah dibuat; masuk CREATE):
--   "SupportingEvidenceNote" varchar(1000) NOT NULL -- bukti pendukung berupa teks
--   "CorrectionStatus"       integer       NOT NULL -- 0 Requested, 1 Approved, 2 Rejected
--   "RequestedByUserId"      uuid          NOT NULL -- ganti nama dari CorrectedByUserId
--   "RequestedAt"            timestamp     NOT NULL -- ganti nama dari CorrectedAt
--   "DecidedByUserId"        uuid                   -- NULL selama Requested; wajib != RequestedByUserId
--   "DecidedAt"              timestamp
--   "DecisionNote"           varchar(500)           -- wajib diisi bila Rejected

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
