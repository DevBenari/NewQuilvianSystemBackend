# Kamus Data — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | **`0.7`** — bagian 20 Workspace PPRI, kontrak `0.11.0` (7 Oktober 2026, `draft`). Sebelumnya `0.6` — bagian 19 Finishing (`approved`, `RWI-DEC-221`); `0.5` — bagian 18 penyelarasan `PRD-RWI-V2-001`, blueprint revision `7` |
| Status | **`approved`** untuk `0.7` (bagian 20) — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`). Status bagian sebelumnya mengikuti `blueprint-manifest.md` sub-modul |
| Backend SHA | `5afb54b` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah dan
**tidak ditulis ulang** pada bagian DDL.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk ke custom logger, tidak boleh dipakai sebagai
contoh berisi data asli, dan perlu ditinjau kebutuhan penyamarannya pada response.

---

## 1. `InpEpisode` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | Nomor episode terbaca manusia, awalan dari master pengaturan |
| `EncounterId` | `Guid` | Ya | — | **Unique** | FK ke `TrxPatientEncounter` | `Restrict` | Tidak | Jangkar episode. Unique menjaga `INV-INP-04` |
| `PatientId` | `Guid` | Ya | — | Index + **unique parsial** | FK ke `MstPatient` | `Restrict` | Tidak | Salinan dari kunjungan, hanya untuk mempercepat census. Unique parsial menjaga `INV-INP-10`, lihat bagian 16 |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK ke `MstServiceUnit` | `Restrict` | Tidak | Unit layanan tempat pasien dirawat saat admisi dibuka |
| `PatientClassId` | `Guid` | Ya | — | Index | FK ke `MstPatientClass` | `Restrict` | Tidak | Kelas saat admisi dibuka. Kelas yang ditagihkan dibaca dari penempatan |
| `EpisodeStatus` | `InpEpisodeStatus` | Ya | `Draft` | Index | — | — | Tidak | Disimpan sebagai `int` |
| `AdmittedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Diisi saat pasien menempati tempat tidur. Titik mulai lama dirawat |
| `DischargeDecidedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Diisi saat DPJP memutuskan pasien boleh pulang |
| `PhysicallyLeftAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Diisi saat kepergian fisik pasien dicatat. Kosong berarti pasien masih berada di ruangan. Dipakai `INV-INP-10` dan census |
| `PhysicallyLeftByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang mencatat kepergian |
| `MotherEpisodeId` | `Guid?` | Tidak | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode ibu, hanya untuk bayi rawat gabung. **Tidak boleh** menunjuk episode milik pasien yang sama |
| `RequiresIsolation` | `bool` | Ya | `false` | Index | — | — | Tidak | Penanda kebutuhan isolasi. Dipakai aturan 7 dan 8 pada Kelayakan Penempatan |
| `IsolationSource` | `InpIsolationSource?` | Tidak | — | — | — | — | Tidak | `AdmissionRecord` bila direkam petugas admisi dari keterangan dokter pengirim; `ClinicalDecision` bila ditetapkan DPJP |
| `IsolationSetByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang terakhir mengubah penanda isolasi |
| `IsolationSetByDoctorId` | `Guid?` | Tidak | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Diisi hanya bila `IsolationSource` bernilai `ClinicalDecision` |
| `IsolationSetAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan penanda isolasi terakhir diubah |
| `IsolationNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan klinis atau keterangan dokter pengirim |
| `ClosedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Diisi saat episode ditutup. Titik akhir lama dirawat |
| `DischargeType` | `InpDischargeType` | Ya | `Unknown` | — | — | — | Tidak | Diisi saat keputusan pulang. Disimpan sebagai `int` |
| `IsClosedWithoutFinancialClearance` | `bool` | Ya | `false` | Index | — | — | Tidak | Menandai penutupan yang menembus gerbang keuangan |
| `ClosedWithoutClearanceReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan supervisor menembus gerbang. Wajib bila kolom di atas `true` |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan pembatalan admisi |
| `Notes` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan bebas petugas admisi |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Mengikuti konvensi project |

## 2. `InpDoctorAssignment` — status `Diperbarui` sejak `0.8.0`

**Satu kolom ditambahkan** pada 11 September 2026 oleh `RWI-DEC-099`, dan **satu index unik
berubah filternya**. Sebelum itu status tabel ini `Baru`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `DoctorId` | `Guid` | Ya | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Dokter yang ditunjuk. ~~Selalu DPJP~~ — sejak `0.8.0` perannya ditentukan `AssignmentRole` |
| **`AssignmentRole`** | `int` enum | Ya | `1` `Dpjp` | Index gabungan, lihat di bawah | — | — | Tidak | **Kolom baru `0.8.0`.** `1` `Dpjp`, `2` `Consultant`, `3` `OnCallDoctor`. Menentukan kewenangan menulis dan kewenangan memutuskan pulang |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan penugasan, dimulai dari 1. Deretnya **satu per episode**, bukan satu per peran |
| `StartDateTime` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Mulai berlakunya tanggung jawab. Dipakai menilai kewenangan pada **waktu klinis** dokumen |
| `EndDateTime` | `DateTime?` | Tidak | — | Index parsial | — | — | Tidak | Kosong berarti masih aktif. **Filter unique berubah `0.8.0`**, lihat catatan index |
| `AssignedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menugaskan atau mengalihkan. Untuk konsulen dan dokter jaga, ini kepala ruangan atau supervisor |
| `HandoverReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib diisi bila baris ini lahir dari pengalihan. **Sejak `0.8.0` juga wajib** bila perannya `Consultant` atau `OnCallDoctor`, berisi alasan pelibatan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

### 2.1 Perubahan index unik — wajib dibaca sebelum migration

Index `IX_InpDoctorAssignment_EpisodeId_Active` hari ini berbunyi unik atas `EpisodeId` dengan
filter `"EndDateTime" IS NULL`. Artinya **satu episode hanya boleh punya satu penugasan terbuka**.

Selama tabel ini hanya menyimpan DPJP, index itu benar. Begitu konsulen dan dokter jaga masuk,
index itu **menolak baris kedua di tingkat database**, dan `RWI-DEC-099` tidak akan pernah dapat
dijalankan. Ini bukan pilihan gaya; tanpa perubahan index, keputusan itu gagal pada `INSERT`
kedua.

| Keadaan | Filter index | Akibatnya |
| --- | --- | --- |
| Sebelum `0.8.0` | `"EndDateTime" IS NULL` | Satu penugasan terbuka per episode. Konsulen kedua **ditolak database** |
| Sejak `0.8.0` | `"EndDateTime" IS NULL AND "AssignmentRole" = 1` | **Tepat satu DPJP aktif** per episode; konsulen dan dokter jaga boleh banyak dan boleh bersamaan |

`INV-INP-03` berbunyi "episode belum `Closed`/`Cancelled` punya tepat satu DPJP aktif". Filter
baru itu menegakkan bunyi invariant **apa adanya**, sedangkan filter lama menegakkan sesuatu yang
lebih ketat daripada yang diminta. Jadi perubahan ini **memulihkan** invariant, bukan
melonggarkannya.

**Pengisian data lama.** Seluruh baris yang sudah ada diisi `AssignmentRole = 1` `Dpjp`. Itu bukan
tebakan: sebelum `0.8.0` tabel ini memang hanya menyimpan DPJP, sebagaimana tertulis pada kolom
`DoctorId` versi sebelumnya. Tidak ada baris yang ambigu, sehingga tidak ada laporan
`unresolved` yang perlu dibuat.

**Urutan migration yang aman tanpa mematikan layanan.** Tambah kolom dengan nilai bawaan lebih
dulu, isi baris lama, baru ganti index. Membuang index lama sebelum kolomnya terisi membuka celah
waktu ketika dua DPJP aktif dapat tersimpan.

## 3. `InpNurseAssignment` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `EmployeeId` | `Guid` | Ya | — | Index | FK ke `MstEmployee` | `Restrict` | Tidak | Perawat yang ditugaskan |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan penugasan |
| `StartDateTime` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Mulai berlakunya tanggung jawab |
| `EndDateTime` | `DateTime?` | Tidak | — | Index parsial | — | — | Tidak | Kosong berarti masih aktif. Unique atas `EpisodeId` bila kosong |
| `AssignedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Kepala ruangan yang menugaskan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 4. `InpBedReservation` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode yang memesan |
| `BedId` | `Guid` | Ya | — | Index parsial unik | FK ke `MstBed` | `Restrict` | Tidak | Unik bila `ReservationStatus = Active`, menjaga `INV-INP-02` |
| `ReservedAt` | `DateTime` | Ya | `UtcNow` | — | — | — | Tidak | Waktu pemesanan dibuat |
| `ExpiresAt` | `DateTime` | Ya | — | Index | — | — | Tidak | Disalin dari `BedReservationMinutes` **saat pemesanan dibuat** |
| `ReservationStatus` | `InpBedReservationStatus` | Ya | `Active` | Index | — | — | Tidak | Disimpan sebagai `int` |
| `ReservedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Petugas admisi yang memesan |
| `ReleasedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pemesanan berhenti aktif, apa pun sebabnya |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 5. `InpBedPlacement` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode yang menempati |
| `BedId` | `Guid` | Ya | — | Index parsial unik | FK ke `MstBed` | `Restrict` | Tidak | Unik bila `EndDateTime` kosong, menjaga `INV-INP-02` |
| `RoomId` | `Guid` | Ya | — | Index | FK ke `MstRoom` | `Restrict` | Tidak | **Salinan saat penempatan dibuat**, bukan pembacaan langsung |
| `ServiceUnitId` | `Guid` | Ya | — | Index | FK ke `MstServiceUnit` | `Restrict` | Tidak | Salinan saat penempatan dibuat |
| `PatientClassId` | `Guid` | Ya | — | Index | FK ke `MstPatientClass` | `Restrict` | Tidak | Salinan saat penempatan dibuat. Inilah kelas yang ditagihkan |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan penempatan di dalam episode |
| `StartDateTime` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Mulai ditempati. Bawaan `UtcNow` hanya berlaku untuk jalur datang langsung dan poliklinik; untuk episode yang lahir dari serah terima IGD nilainya **dibaca dari event `Tiba`** pada catatan kepergian IGD dan tidak pernah dikoreksi setelah tersimpan — `RWI-DEC-072`. Bentuk kolomnya tidak berubah |
| `EndDateTime` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Kosong berarti masih ditempati |
| `EndReason` | `InpBedPlacementEndReason?` | Tidak | — | — | — | — | Tidak | Kenapa penempatan berakhir: perpindahan, penutupan episode, pembatalan admisi, atau **kepergian fisik pasien**. Disimpan sebagai `int` |
| `TransferReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan medis perpindahan. Wajib bila baris ini lahir dari perpindahan |
| `PlacedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menempatkan |
| `EndedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang mengakhiri penempatan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

**Satu kolom milik modul lain yang dibaca tabel ini secara tidak langsung.**
`TrxPatientEncounter.OriginEncounterId` — `Guid?`, boleh kosong, milik `RegistrationManagement`,
**dibuat dan diisi modul IGD** lewat `IGD-DEC-075`. Rawat Inap memakainya hanya untuk mengetahui
apakah sebuah episode lahir dari serah terima IGD. Kolom itu **tidak** dibuat, tidak diubah, dan
tidak masuk migration milik modul ini — `RWI-DEC-073`.

## 6. `InpDischargeSummary` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | **Unique** | FK ke `InpEpisode` | `Restrict` | Tidak | Unique menjaga `INV-INP-05` |
| `PrimaryDiagnosisText` | `string(1000)` | Ya | — | — | — | — | **Ya** | Diagnosis utama. Berbentuk teks pada MVP |
| `SecondaryDiagnosisText` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Diagnosis sekunder |
| `ProcedureSummary` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Tindakan selama dirawat |
| `DischargeMedicationNote` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Catatan obat pulang |
| `FollowUpInstruction` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Instruksi kontrol |
| `ReferralDestination` | `string(250)?` | Tidak | — | — | — | — | Tidak | Wajib bila cara pulang `Referred` |
| `ClinicalSummary` | `string(4000)?` | Tidak | — | — | — | — | **Ya** | Ringkasan perjalanan penyakit |
| `SignedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Kosong berarti belum ditandatangani, dan penutupan tertahan |
| `SignedByDoctorId` | `Guid?` | Tidak | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | DPJP yang menandatangani |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 7. `InpDischargeSummaryRevision` — status `Baru` pada revision `0.2`

Menyimpan salinan resume pulang **versi sebelumnya**, dibuat setiap kali resume yang sudah
ditandatangani diubah. Penyuntingan sebelum tanda tangan tidak membuat baris di sini.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DischargeSummaryId` | `Guid` | Ya | — | Index | FK ke `InpDischargeSummary` | `Restrict` | Tidak | Resume yang versinya disalin |
| `RevisionNumber` | `int` | Ya | — | Unique bersama `DischargeSummaryId` | — | — | Tidak | Urutan versi, dimulai dari 1 |
| `CorrectionSessionId` | `Guid?` | Tidak | — | Index | FK ke `InpCorrectionSession` | `Restrict` | Tidak | Sesi koreksi yang menyebabkan penggantian |
| `PrimaryDiagnosisText` | `string(1000)` | Ya | — | — | — | — | **Ya** | Salinan isi versi lama |
| `SecondaryDiagnosisText` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Salinan isi versi lama |
| `ProcedureSummary` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Salinan isi versi lama |
| `DischargeMedicationNote` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Salinan isi versi lama |
| `FollowUpInstruction` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Salinan isi versi lama |
| `ReferralDestination` | `string(250)?` | Tidak | — | — | — | — | Tidak | Salinan isi versi lama |
| `ClinicalSummary` | `string(4000)?` | Tidak | — | — | — | — | **Ya** | Salinan isi versi lama |
| `PreviousDischargeType` | `InpDischargeType` | Ya | — | — | — | — | Tidak | Cara pulang yang berlaku pada versi lama |
| `PreviousSignedAt` | `DateTime` | Ya | — | — | — | — | Tidak | Kapan versi lama ditandatangani |
| `PreviousSignedByDoctorId` | `Guid` | Ya | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Siapa yang menandatangani versi lama |
| `SupersededAt` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Kapan versi ini digantikan |
| `SupersededByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menggantikan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Baris ini tidak pernah dinonaktifkan |

**Aturan yang mengikat tabel ini.** Baris di sini **tidak dapat diubah dan tidak dapat dihapus**;
tidak disediakan endpoint update maupun delete. `InpDischargeSummary` tetap menyimpan versi yang
berlaku, sehingga `INV-INP-05` — satu episode paling banyak satu resume — tidak berubah.

## 8. `InpClearanceMark` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Unique bersama `ClearanceItemId` | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `ClearanceItemId` | `Guid` | Ya | — | Index | FK ke `MstInpatientClearanceItem` | `Restrict` | Tidak | Butir yang ditandai |
| `MarkedAt` | `DateTime` | Ya | `UtcNow` | — | — | — | Tidak | Waktu penandaan |
| `MarkedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Petugas admisi yang menandai |
| `Note` | `string(500)?` | Tidak | — | — | — | — | Tidak | Keterangan tambahan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 9. `InpFinancialClearance` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan penandaan |
| `ClearanceStatus` | `InpFinancialClearanceStatus` | Ya | `Pending` | Index | — | — | Tidak | Disimpan sebagai `int` |
| `MarkedAt` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Waktu penandaan |
| `MarkedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Petugas kasir atau billing |
| `Note` | `string(500)` | Ya | — | — | — | — | Tidak | **Wajib.** Penandaan tanpa catatan ditolak |
| `IsManualMarking` | `bool` | Ya | `true` | — | — | — | Tidak | Selalu `true` selama MVP. Wajib ditampilkan pada layar dan laporan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 10. `InpStatusHistory` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan perpindahan status |
| `FromStatus` | `InpEpisodeStatus?` | Tidak | — | — | — | — | Tidak | Kosong pada baris pertama |
| `ToStatus` | `InpEpisodeStatus` | Ya | — | Index | — | — | Tidak | Status baru |
| `ActionType` | `string(50)` | Ya | — | — | — | — | Tidak | Nama tindakan, misalnya `Admit`, `Transfer`, `Close` |
| `ActorType` | `InpStatusChangeActorType` | Ya | `User` | Index | — | — | Tidak | `User` atau `System` |
| `ChangedByUserId` | `Guid?` | Tidak | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | **Kosong bila dilakukan sistem** |
| `ChangedAt` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Waktu perpindahan |
| `Reason` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Alasan perpindahan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Baris ini tidak pernah dinonaktifkan |

## 11. `InpCorrectionSession` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode yang dikoreksi |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan sesi koreksi |
| `OpenedAt` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Waktu sesi dibuka |
| `OpenedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Supervisor yang membuka |
| `OpenReason` | `string(500)` | Ya | — | — | — | — | Tidak | **Wajib.** Reopen tanpa alasan ditolak |
| `ClosedAt` | `DateTime?` | Tidak | — | Index parsial | — | — | Tidak | Kosong berarti masih terbuka. Unique atas `EpisodeId` bila kosong |
| `ClosedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Supervisor yang menutup |
| `ChangedFieldSummary` | `string(4000)?` | Tidak | — | — | — | — | Tidak | **Wajib saat sesi ditutup.** Daftar apa saja yang berubah |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 12. `MstInpatientSetting` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `Code` | `string(50)` | Ya | `DEFAULT` | Unique | — | — | Tidak | Kode pengaturan |
| `Name` | `string(150)` | Ya | — | — | — | — | Tidak | Nama pengaturan |
| `BedReservationMinutes` | `int` | Ya | `120` | — | — | — | Tidak | Batas pemesanan tempat tidur, `RWI-RULE-002` |
| `DraftEpisodeExpiryHours` | `int` | Ya | `24` | — | — | — | Tidak | Batas episode `Draft` telantar, `RWI-RULE-022` |
| `InitialAssessmentTargetHours` | `int` | Ya | `24` | — | — | — | Tidak | Target pengkajian awal, `RWI-RULE-021` — **belum final secara klinis** |
| `ProgressNoteVerificationTargetHours` | `int` | Ya | `24` | — | — | — | Tidak | Target verifikasi CPPT, `RWI-RULE-021` — belum final |
| `PendingClosureThresholdHours` | `int` | Ya | `4` | — | — | — | Tidak | Ambang daftar pantau penutupan tertunda, `RWI-RULE-023` |
| `EpisodeNumberPrefix` | `string(20)` | Ya | `RI` | — | — | — | Tidak | Awalan nomor episode |
| `IsDefault` | `bool` | Ya | `true` | — | — | — | Tidak | Menandai baris yang dipakai |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |
| `Notes` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Keterangan admin |

## 13. `MstInpatientClearanceItem` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ItemCode` | `string(50)` | Ya | — | Unique | — | — | Tidak | Kode butir |
| `ItemName` | `string(200)` | Ya | — | — | — | — | Tidak | Nama butir yang dibaca petugas |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Penjelasan butir |
| `IsMandatory` | `bool` | Ya | `true` | Index | — | — | Tidak | Butir wajib menahan penutupan |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Butir nonaktif tidak lagi menahan penutupan |

---

## 14. Tabel milik modul lain — status `Sudah ada`

Hanya kolom kunci dan kolom yang dipakai aturan bisnis modul ini. Sumber lengkapnya ada pada file
model masing-masing.

### 14.1 `TrxPatientEncounter` — sumber `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`

| Kolom | Tipe | Dipakai modul ini untuk | Keterangan |
| --- | --- | --- | --- |
| `Id` | `Guid` | Jangkar episode | PK |
| `EncounterNumber` | `string(50)` | Ditampilkan pada census | Unique |
| `PatientId` | `Guid` | Sumber `InpEpisode.PatientId` | FK |
| `EncounterType` | `EncounterType` | Wajib bernilai `Inpatient` untuk episode rawat inap | Nilai `3` |
| `ServiceUnitId` | `Guid` | Konteks unit layanan | FK |
| `PatientClassId` | `Guid?` | Kelas saat kunjungan dibuat | FK |
| `EncounterStatus` | `EncounterStatus` | **Tidak dipakai** modul ini | Status poliklinik, bukan status episode |

**Yang tidak boleh dilakukan:** menyimpan status episode ke dalam `EncounterStatus`. Keduanya
lifecycle yang berbeda dan pemiliknya berbeda.

### 14.2 `MstBed` — sumber `Areas/HealthServices/MasterData/Models/MstBed.cs`

| Kolom | Tipe | Dipakai modul ini untuk | Keterangan |
| --- | --- | --- | --- |
| `Id` | `Guid` | Sasaran pemesanan dan penempatan | PK |
| `BedCode` | `string(50)` | Ditampilkan pada census dan pesan kesalahan | Unique |
| `RoomId` | `Guid` | Sumber salinan `InpBedPlacement.RoomId` | FK |
| `BedStatus` | `BedStatus` | **Ditulis** modul ini sebagai salinan | Hanya nilai `Available`, `Reserved`, `Occupied` |
| `IsReservable` | `bool` | Aturan Kelayakan Penempatan | — |
| `IsActive` | `bool` | Aturan Kelayakan Penempatan | — |
| `IsForNewborn` | `bool` | Menandai boks bayi | `RWI-RULE-014` |
| `IsForMale`, `IsForFemale` | `bool` | Aturan 4 dan 5 Kelayakan Penempatan — **menolak** penempatan | `RWI-RULE-012` B.1 dan B.2. Sejak revision `0.3` bukan lagi penyaring pencarian |
| `IsIsolationBed` | `bool` | Aturan 7 dan 8 Kelayakan Penempatan — **menolak** penempatan dari dua arah | `RWI-RULE-012` A.5 dan A.6 |

> **Kenaikan taruhan pada revision `0.3`.** Keempat penanda di atas sebelumnya hanya menyembunyikan
> tempat tidur dari hasil pencarian; salah setel berarti tempat tidur tidak muncul, dan petugas
> tetap dapat menempatkan pasien secara paksa. Sejak `RWI-DEC-064` keduanya **menolak**, sehingga
> penanda yang salah setel akan menolak penempatan yang sah. Karena itu `RWI-DEC-063` memberi
> penanggung jawab pengisian master data beserta target tanggalnya.

### 14.3 `MstRoom`, `MstServiceUnit`, `MstPatientClass`

| Tabel | Kolom kunci yang dipakai | Sumber |
| --- | --- | --- |
| `MstRoom` | `Id`, `ServiceUnitId`, `PatientClassId`, `RoomType`, `IsAvailableForAdmission` | `Areas/HealthServices/MasterData/Models/MstRoom.cs` |
| `MstServiceUnit` | `Id`, `ServiceUnitType`, `IsQueueRequired` | `Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` |
| `MstPatientClass` | `Id`, `PatientClassName`, `ClassLevel`, `IsForInpatient`, `DefaultDailyRoomRate` | `Areas/HealthServices/MasterData/Models/MstPatientClass.cs` |

### 14.4 `MstDoctor` dan `MstEmployee`

| Tabel | Kolom kunci yang dipakai | Sumber |
| --- | --- | --- |
| `MstDoctor` | `Id`, `FullName` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs` |
| `MstEmployee` | `Id`, `FullName` | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs` |

---

## 15. Enum

| Enum | Nilai | Bawaan | Lokasi file |
| --- | --- | --- | --- |
| `InpEpisodeStatus` | `Draft = 0`, `Admitted = 1`, `DischargePending = 2`, `Closed = 3`, `Cancelled = 4` | `Draft` | `Areas/HealthServices/InPatientManagement/Enums/InpEpisodeStatus.cs` |
| `InpDischargeType` | `Unknown = 0`, `DoctorApproved = 1`, `AgainstMedicalAdvice = 2`, `Referred = 3` | `Unknown` | `.../Enums/InpDischargeType.cs` |
| `InpBedReservationStatus` | `Active = 1`, `Consumed = 2`, `Expired = 3`, `Cancelled = 4` | `Active` | `.../Enums/InpBedReservationStatus.cs` |
| `InpBedPlacementEndReason` | `Transfer = 1`, `EpisodeClosed = 2`, `AdmissionCancelled = 3`, **`PatientDeparted = 4`** | — | `.../Enums/InpBedPlacementEndReason.cs` |
| `InpFinancialClearanceStatus` | `Pending = 0`, `Cleared = 1`, `Blocked = 2` | `Pending` | `.../Enums/InpFinancialClearanceStatus.cs` |
| `InpIsolationSource` | `AdmissionRecord = 1`, `ClinicalDecision = 2` | — | `.../Enums/InpIsolationSource.cs` |
| `InpStatusChangeActorType` | `User = 1`, `System = 2` | `User` | `.../Enums/InpStatusChangeActorType.cs` |

**Catatan tentang `InpDischargeType`.** Nilai `4` dan `5` **sengaja dikosongkan** untuk cara pulang
meninggal dan kabur. Keduanya di luar scope revisi ini dan menunggu `DEC-INP-007`. Mengosongkan
nomornya sekarang membuat penambahan kelak tidak mengubah angka yang sudah tersimpan.

---

## 16. Skema dalam bentuk DDL

> **Peringatan wajib dibaca lebih dulu.** Basis data project ini dibentuk EF Core Migrations,
> bukan skrip SQL manual. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang
> dijalankan. Menjalankannya akan berbenturan dengan migration. Sumber kebenarannya adalah file
> configuration pada `Repositories/Configurations/HealthServices/InPatientManagement/`.
>
> Kolom audit warisan `IdentityModel` tidak ditulis ulang di bawah.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."InpEpisode" (
    "Id"                                  uuid          NOT NULL,
    "EpisodeNumber"                       varchar(50)   NOT NULL,
    "EncounterId"                         uuid          NOT NULL,
    "PatientId"                           uuid          NOT NULL,
    "ServiceUnitId"                       uuid          NOT NULL,
    "PatientClassId"                      uuid          NOT NULL,
    "EpisodeStatus"                       integer       NOT NULL,  -- enum, HasConversion<int>
    "AdmittedAt"                          timestamp,
    "DischargeDecidedAt"                  timestamp,
    "PhysicallyLeftAt"                    timestamp,               -- kosong = pasien masih di ruangan
    "PhysicallyLeftByUserId"              uuid,
    "ClosedAt"                            timestamp,
    "DischargeType"                       integer       NOT NULL,  -- enum
    "MotherEpisodeId"                     uuid,                    -- episode ibu, hanya bayi rawat gabung
    "RequiresIsolation"                   boolean       NOT NULL,
    "IsolationSource"                     integer,                 -- enum, 1 catatan awal, 2 keputusan klinis
    "IsolationSetByUserId"                uuid,
    "IsolationSetByDoctorId"              uuid,
    "IsolationSetAt"                      timestamp,
    "IsolationNote"                       varchar(500),            -- SENSITIF
    "IsClosedWithoutFinancialClearance"   boolean       NOT NULL,
    "ClosedWithoutClearanceReason"        varchar(500),
    "CancelReason"                        varchar(500),
    "Notes"                               varchar(1000),           -- SENSITIF
    "IsActive"                            boolean       NOT NULL,

    CONSTRAINT "PK_InpEpisode" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpEpisode_TrxPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."TrxPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpEpisode_InpEpisode_MotherEpisodeId"
        FOREIGN KEY ("MotherEpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_InpEpisode_EpisodeNumber" ON public."InpEpisode" ("EpisodeNumber");
CREATE UNIQUE INDEX "IX_InpEpisode_EncounterId"   ON public."InpEpisode" ("EncounterId");
CREATE INDEX "IX_InpEpisode_EpisodeStatus"        ON public."InpEpisode" ("EpisodeStatus");
CREATE INDEX "IX_InpEpisode_PatientId"            ON public."InpEpisode" ("PatientId");
CREATE INDEX "IX_InpEpisode_MotherEpisodeId"      ON public."InpEpisode" ("MotherEpisodeId");
CREATE INDEX "IX_InpEpisode_RequiresIsolation"    ON public."InpEpisode" ("RequiresIsolation");

-- Menjaga INV-INP-10: satu pasien paling banyak satu episode yang benar-benar hadir.
-- 1 = Admitted, 2 = DischargePending.
CREATE UNIQUE INDEX "IX_InpEpisode_PatientId_Present"
    ON public."InpEpisode" ("PatientId")
    WHERE "EpisodeStatus" = 1
       OR ("EpisodeStatus" = 2 AND "PhysicallyLeftAt" IS NULL);


CREATE TABLE public."InpBedPlacement" (
    "Id"               uuid          NOT NULL,
    "EpisodeId"        uuid          NOT NULL,
    "BedId"            uuid          NOT NULL,
    "RoomId"           uuid          NOT NULL,
    "ServiceUnitId"    uuid          NOT NULL,
    "PatientClassId"   uuid          NOT NULL,
    "SequenceNumber"   integer       NOT NULL,
    "StartDateTime"    timestamp     NOT NULL,
    "EndDateTime"      timestamp,
    "EndReason"        integer,                 -- enum: 1 Transfer, 2 EpisodeClosed, 3 AdmissionCancelled, 4 PatientDeparted
    "TransferReason"   varchar(500),
    "PlacedByUserId"   uuid          NOT NULL,
    "EndedByUserId"    uuid,
    "IsActive"         boolean       NOT NULL,

    CONSTRAINT "PK_InpBedPlacement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpBedPlacement_InpEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpBedPlacement_MstBed_BedId"
        FOREIGN KEY ("BedId") REFERENCES public."MstBed" ("Id") ON DELETE RESTRICT
);

-- Menjaga INV-INP-02: satu tempat tidur hanya boleh punya satu penempatan aktif
CREATE UNIQUE INDEX "IX_InpBedPlacement_BedId_Active"
    ON public."InpBedPlacement" ("BedId") WHERE "EndDateTime" IS NULL;

CREATE UNIQUE INDEX "IX_InpBedPlacement_EpisodeId_SequenceNumber"
    ON public."InpBedPlacement" ("EpisodeId", "SequenceNumber");


CREATE TABLE public."InpBedReservation" (
    "Id"                  uuid        NOT NULL,
    "EpisodeId"           uuid        NOT NULL,
    "BedId"               uuid        NOT NULL,
    "ReservedAt"          timestamp   NOT NULL,
    "ExpiresAt"           timestamp   NOT NULL,
    "ReservationStatus"   integer     NOT NULL,  -- enum
    "ReservedByUserId"    uuid        NOT NULL,
    "ReleasedAt"          timestamp,
    "IsActive"            boolean     NOT NULL,

    CONSTRAINT "PK_InpBedReservation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpBedReservation_InpEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpBedReservation_MstBed_BedId"
        FOREIGN KEY ("BedId") REFERENCES public."MstBed" ("Id") ON DELETE RESTRICT
);

-- Menjaga INV-INP-02 pada sisi pemesanan; 1 berarti ReservationStatus = Active
CREATE UNIQUE INDEX "IX_InpBedReservation_BedId_Active"
    ON public."InpBedReservation" ("BedId") WHERE "ReservationStatus" = 1;


CREATE TABLE public."InpDoctorAssignment" (
    "Id"                 uuid          NOT NULL,
    "EpisodeId"          uuid          NOT NULL,
    "DoctorId"           uuid          NOT NULL,
    "AssignmentRole"     integer       NOT NULL DEFAULT 1,  -- kolom baru 0.8.0
    "SequenceNumber"     integer       NOT NULL,
    "StartDateTime"      timestamp     NOT NULL,
    "EndDateTime"        timestamp,
    "AssignedByUserId"   uuid          NOT NULL,
    "HandoverReason"     varchar(500),
    "IsActive"           boolean       NOT NULL,

    CONSTRAINT "PK_InpDoctorAssignment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpDoctorAssignment_InpEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpDoctorAssignment_MstDoctor_DoctorId"
        FOREIGN KEY ("DoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);

-- Menjaga INV-INP-03: satu episode hanya boleh punya satu DPJP aktif.
--
-- Bentuk sampai contract_version 0.7.0, DICABUT 11 September 2026:
--     CREATE UNIQUE INDEX "IX_InpDoctorAssignment_EpisodeId_Active"
--         ON public."InpDoctorAssignment" ("EpisodeId") WHERE "EndDateTime" IS NULL;
--
-- Filter lama menolak SETIAP penugasan terbuka kedua, termasuk konsulen dan dokter jaga
-- yang disahkan RWI-DEC-099. Filter baru menegakkan bunyi INV-INP-03 apa adanya, yaitu
-- tepat satu DPJP aktif, sambil mengizinkan konsulen dan dokter jaga berdampingan.
CREATE UNIQUE INDEX "IX_InpDoctorAssignment_EpisodeId_ActiveDpjp"
    ON public."InpDoctorAssignment" ("EpisodeId")
    WHERE "EndDateTime" IS NULL AND "AssignmentRole" = 1;

-- Mempercepat penilaian kewenangan menulis pada waktu klinis tertentu
CREATE INDEX "IX_InpDoctorAssignment_Episode_Doctor_Role_Period"
    ON public."InpDoctorAssignment" ("EpisodeId", "DoctorId", "AssignmentRole", "StartDateTime");


CREATE TABLE public."InpStatusHistory" (
    "Id"                uuid           NOT NULL,
    "EpisodeId"         uuid           NOT NULL,
    "SequenceNumber"    integer        NOT NULL,
    "FromStatus"        integer,                  -- enum, kosong pada baris pertama
    "ToStatus"          integer        NOT NULL,  -- enum
    "ActionType"        varchar(50)    NOT NULL,
    "ActorType"         integer        NOT NULL,  -- 1 orang, 2 sistem
    "ChangedByUserId"   uuid,                     -- kosong bila dilakukan sistem
    "ChangedAt"         timestamp      NOT NULL,
    "Reason"            varchar(1000),
    "IsActive"          boolean        NOT NULL,

    CONSTRAINT "PK_InpStatusHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpStatusHistory_InpEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_InpStatusHistory_EpisodeId_SequenceNumber"
    ON public."InpStatusHistory" ("EpisodeId", "SequenceNumber");


CREATE TABLE public."InpDischargeSummary" (
    "Id"                        uuid           NOT NULL,
    "EpisodeId"                 uuid           NOT NULL,
    "PrimaryDiagnosisText"      varchar(1000)  NOT NULL,  -- SENSITIF
    "SecondaryDiagnosisText"    varchar(2000),            -- SENSITIF
    "ProcedureSummary"          varchar(2000),            -- SENSITIF
    "DischargeMedicationNote"   varchar(2000),            -- SENSITIF
    "FollowUpInstruction"       varchar(2000),            -- SENSITIF
    "ReferralDestination"       varchar(250),
    "ClinicalSummary"           varchar(4000),            -- SENSITIF
    "SignedAt"                  timestamp,
    "SignedByDoctorId"          uuid,
    "IsActive"                  boolean        NOT NULL,

    CONSTRAINT "PK_InpDischargeSummary" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpDischargeSummary_InpEpisode_EpisodeId"
        FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT
);

-- Menjaga INV-INP-05: satu episode hanya boleh punya satu resume pulang
CREATE UNIQUE INDEX "IX_InpDischargeSummary_EpisodeId"
    ON public."InpDischargeSummary" ("EpisodeId");
```

```sql
-- Baru pada revision 0.2. Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."InpDischargeSummaryRevision" (
    "Id"                          uuid           NOT NULL,
    "DischargeSummaryId"          uuid           NOT NULL,
    "RevisionNumber"              integer        NOT NULL,
    "CorrectionSessionId"         uuid,
    "PrimaryDiagnosisText"        varchar(1000)  NOT NULL,  -- SENSITIF
    "SecondaryDiagnosisText"      varchar(2000),            -- SENSITIF
    "ProcedureSummary"            varchar(2000),            -- SENSITIF
    "DischargeMedicationNote"     varchar(2000),            -- SENSITIF
    "FollowUpInstruction"         varchar(2000),            -- SENSITIF
    "ReferralDestination"         varchar(250),
    "ClinicalSummary"             varchar(4000),            -- SENSITIF
    "PreviousDischargeType"       integer        NOT NULL,  -- enum
    "PreviousSignedAt"            timestamp      NOT NULL,
    "PreviousSignedByDoctorId"    uuid           NOT NULL,
    "SupersededAt"                timestamp      NOT NULL,
    "SupersededByUserId"          uuid           NOT NULL,
    "IsActive"                    boolean        NOT NULL,

    CONSTRAINT "PK_InpDischargeSummaryRevision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpDischargeSummaryRevision_InpDischargeSummary_DischargeSummaryId"
        FOREIGN KEY ("DischargeSummaryId")
        REFERENCES public."InpDischargeSummary" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpDischargeSummaryRevision_InpCorrectionSession_CorrectionSessionId"
        FOREIGN KEY ("CorrectionSessionId")
        REFERENCES public."InpCorrectionSession" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_InpDischargeSummaryRevision_SummaryId_RevisionNumber"
    ON public."InpDischargeSummaryRevision" ("DischargeSummaryId", "RevisionNumber");
```

Enam tabel sisanya — `InpNurseAssignment`, `InpClearanceMark`, `InpFinancialClearance`,
`InpCorrectionSession`, `MstInpatientSetting`, dan `MstInpatientClearanceItem` — mengikuti pola
yang sama persis: `Id` sebagai kunci utama, foreign key ke induknya dengan `ON DELETE RESTRICT`,
enum sebagai `integer`, dan unique index sesuai kolom Index pada tabel kamus data di atas. Bentuk
DDL-nya tidak ditulis ulang di sini karena tidak menambah informasi baru.

---

## 17. Perubahan pada revision `0.2`

| Yang berubah | Dasar |
| --- | --- |
| `InpEpisode` bertambah `PhysicallyLeftAt`, `PhysicallyLeftByUserId`, `MotherEpisodeId`, satu foreign key ke dirinya sendiri, dan satu unique index parsial | `RWI-DEC-054`, `RWI-DEC-055`, `RWI-DEC-056` |
| `InpBedPlacementEndReason` bertambah nilai `PatientDeparted` | `RWI-DEC-055` |
| Pada revision `0.3`: `InpEpisode` bertambah enam kolom kebutuhan isolasi, dan enum baru `InpIsolationSource` | `RWI-DEC-065` |
| Tabel baru `InpDischargeSummaryRevision` beserta DDL-nya | `RWI-DEC-057` |
| Penomoran bagian bergeser karena satu tabel baru disisipkan pada urutan 7 | — |
| Pada revision `0.4`: `InpBedPlacement.StartDateTime` berubah **asal nilainya** untuk jalur serah terima IGD. Tidak ada kolom, tipe, index, maupun DDL yang berubah | `RWI-DEC-072` |
| Pada revision `0.4`: satu kolom milik modul lain dicatat sebagai **dibaca**, yaitu `TrxPatientEncounter.OriginEncounterId`. Tidak dibuat modul ini | `RWI-DEC-073` |

Tidak ada kolom yang dihapus dan tidak ada tipe yang berubah pada revision ini.

---

## 18. Amandemen revision `0.5` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.8` bagian 11 |
| Status | **`draft`** |
| Keputusan | `RWI-DEC-112`, `RWI-DEC-130` |

**Nol tabel baru.** Tiga tabel `Diperbarui`, seluruhnya milik `InPatientManagement`.

### 18.1 `InpDoctorAssignment` — `Diperbarui` — sumber lengkap `Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `AssignmentPurpose` | `integer` | Ya | `0` `Regular` | `IX_InpDoctorAssignment_DoctorId_Active` (`DoctorId`, `EndDateTime`) — **baru**, untuk census `assignedToMe` | — | — | Tidak | `1` `LateDocumentation`: wajib `AssignmentRole = 3` `OnCallDoctor`, `EndDateTime` terisi dan lebih besar dari `StartDateTime`, `HandoverReason` terisi |

**Kolom lama yang dipakai aturan baru:** `AssignmentRole`, `StartDateTime`, `EndDateTime`, `HandoverReason` (alasan
pelibatan konsulen, pemanggilan dokter jaga, atau penulisan catatan terlambat), `AssignedByUserId`.

### 18.2 `InpDischargeSummary` — `Diperbarui` — sumber lengkap `Areas/HealthServices/InPatientManagement/Models/InpDischargeSummary.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ImportantFindingsSummary` | `varchar(4000)` | Tidak | `null` | — | — | — | **Ya** | Pemeriksaan Penting. Contoh "Hb 7,8 g/dL (12/09) → transfusi; Rontgen toraks: efusi pleura kanan" |
| `DischargeConditionNote` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Kondisi Saat Pulang. Contoh "Sadar penuh, TD 120/80, jalan sendiri, luka operasi kering" |
| `EducationSummary` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Edukasi. Contoh "Diet rendah garam; tanda bahaya sesak — segera ke IGD; kontrol poli jantung 22/09" |

`ClinicalSummary` yang sudah ada **tidak** berubah bentuk; labelnya di layar menjadi "Ringkasan Perawatan".

### 18.3 `InpDischargeSummaryRevision` — `Diperbarui`

Tiga kolom yang sama persis dengan 18.2, bertipe, panjang, dan sensitivitas sama. Diisi dari nilai versi bertanda tangan
sebelumnya ketika resume ditandatangani ulang lewat sesi koreksi.

### 18.4 Skema DDL revision `0.5`

> **Peringatan.** Dokumentasi bentuk, bukan skrip yang dijalankan. Skema sungguhan lahir dari EF Core migration
> `InPatientManagement`. Kolom warisan `IdentityModel` tidak ditulis ulang.

```sql
ALTER TABLE public."InpDoctorAssignment" ADD COLUMN "AssignmentPurpose" integer NOT NULL DEFAULT 0;
ALTER TABLE public."InpDoctorAssignment" ADD CONSTRAINT "CK_InpDoctorAssignment_LateDocumentation"
    CHECK ("AssignmentPurpose" <> 1
        OR ("AssignmentRole" = 3 AND "EndDateTime" IS NOT NULL AND "EndDateTime" > "StartDateTime"
            AND "HandoverReason" IS NOT NULL AND length(trim("HandoverReason")) > 0));
CREATE INDEX "IX_InpDoctorAssignment_DoctorId_Active"
    ON public."InpDoctorAssignment" ("DoctorId", "EndDateTime")
    WHERE "IsDelete" = false;

ALTER TABLE public."InpDischargeSummary" ADD COLUMN "ImportantFindingsSummary" varchar(4000) NULL;  -- SENSITIF
ALTER TABLE public."InpDischargeSummary" ADD COLUMN "DischargeConditionNote" varchar(2000) NULL;    -- SENSITIF
ALTER TABLE public."InpDischargeSummary" ADD COLUMN "EducationSummary" varchar(2000) NULL;          -- SENSITIF

ALTER TABLE public."InpDischargeSummaryRevision" ADD COLUMN "ImportantFindingsSummary" varchar(4000) NULL;
ALTER TABLE public."InpDischargeSummaryRevision" ADD COLUMN "DischargeConditionNote" varchar(2000) NULL;
ALTER TABLE public."InpDischargeSummaryRevision" ADD COLUMN "EducationSummary" varchar(2000) NULL;
```

### 18.5 Tabel milik modul lain yang **ditulis** lewat penutupan episode

| Tabel | Pemilik | Kolom yang berubah saat penutupan | Lewat service |
| --- | --- | --- | --- |
| `MrcClinicalDocumentIntegrity` | `MedicalRecordManagement` | Status `Draft` → `LockedUnsigned`, `LockTrigger` | `ClinicalDocumentIntegrityService` |
| `TrxPatientProcedure` | `ClinicalManagement` | Status → `Cancelled`, alasan, `CancelledByEpisodeClosure` | `PatientProcedureOrderService` — kolom dirancang `dokter-rawat-inap` data 13.10 |
| `PhmMedicationAdministration` | `PharmacyManagement` | `DoseStatus` → `Cancelled`, `StatusReason` | `MedicationAdministrationService` — dirancang `keperawatan` data 11.12 |

`InPatientManagement` **tidak** memetakan tabel-tabel itu pada configuration-nya dan tidak menulis kolomnya sendiri.

---

## 19. Amandemen revision `0.6` / kontrak `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
| --- | --- |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.9` bagian 12 |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Backend SHA | `c8e99ce5` (HEAD `425cfeae`) |
| Keputusan | `RWI-DEC-173` s.d. `177`, `182`, `189`, `196`, `199`, `201`, `204`, `205` |

Aturan kepala dokumen tetap berlaku: kolom warisan `IdentityModel` tidak diulang; hapus berarti `IsDelete`. Enum disimpan sebagai `integer`, mengikuti konfigurasi OK dan Rawat Inap yang sudah ada.

### 19.1 Ringkasan tabel

| Tabel | Status | Pemilik | Bagian |
|---|---|---|---|
| `OprCase` | `Diperbarui` | `OperatingRoomManagement` | 19.2 |
| `OprWardPreOpNote` | `Baru` | `OperatingRoomManagement` | 19.4 |
| `OprWardPreOpItem` | `Baru` | `OperatingRoomManagement` | 19.5 |
| `OprWardPreOpSiteMark` | `Baru` | `OperatingRoomManagement` | 19.6 |
| `MstSurgicalPreparationItem` | `Baru` | `MasterData` | 19.7 |
| `InpAdmissionReferral` | `Baru` | `InPatientManagement` | 19.8 |
| `CliTransferHandover` | `Baru` (`P2`) | `ClinicalManagement` | 19.9 |
| `MstInpatientSetting` | `Diperbarui` | `MasterData` | 19.10 |
| `MstTariff` | `Diperbarui` | `MasterData` | `keperawatan/data/data-dictionary.md` 12.14 — **satu-satunya daftar lengkap** |
| `OprHandover`, `OprCaseProcedure`, `OprExecutionRecord`, `OprRecovery`, `OprIntegrationDelivery`, `OprStatusHistory`, `InpBedPlacement`, `TrxPatientProcedure` | `Sudah ada` | Masing-masing | 19.11 |

### 19.2 `OprCase` — `Diperbarui` — sumber lengkap `Areas/HealthServices/OperatingRoomManagement/Models/OprCase.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `SurgicalServiceType` | `integer` | Ya | `1` `General` | — | — | — | Tidak | `2` `Obstetric` dari tab Bedah Obgyn. Kasus lama terisi `1` |
| `PlannedAnesthesiaType` | `integer` | Tidak | `null` | — | — | — | Tidak | Rencana saat dipesan. Teknik sesungguhnya tetap di catatan anestesi |
| `RejectedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Terisi bersama `Status = 8` |
| `RejectedByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | Penolak dari akun login |
| `RejectionReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Contoh "Hasil lab pra-operasi belum ada" |

**Constraint baru:** `CK_OprCase_Rejected` — `Status <> 8` atau ketiga kolom penolakan terisi. **Kolom lama yang dipakai aturan baru:** `Status` (bertambah nilai `8` `Rejected`), `Laterality` (`varchar(30)`, dibandingkan dengan sisi penandaan; pesanan bangsal mengisinya dengan `Left`, `Right`, `Bilateral`, atau `NotApplicable`. Kasus dari petugas OK yang berisi teks bebas di luar keempat kode itu tidak dibandingkan otomatis — perawat OK mencocokkan saat konfirmasi), `EncounterId`, `PreferredAt`, `Version`.

### 19.3 Enum baru dan berubah

| Enum | Nilai | Bawaan |
|---|---|---|
| `OprCaseStatus` | Tambah `Rejected = 8` | — |
| `OprSurgicalServiceType` | `General = 1`, `Obstetric = 2` | `General` |
| `OprPlannedAnesthesiaType` | `General = 1`, `Regional = 2`, `Local = 3`, `Sedation = 4` (usulan, disahkan pemilik OK) | — |
| `OprWardPreOpStatus` | `Draft = 1`, `Sent = 2`, `Confirmed = 3`, `NeedsUpdate = 4`, `Superseded = 5` | `Draft` |
| `OprBodyView` | `Front = 1`, `Back = 2`, `Left = 3`, `Right = 4` | — |
| `InpAdmissionReferralStatus` | `Pending = 1`, `Completed = 2`, `Cancelled = 3` | `Pending` |
| `InpRequestedCareLevel` | `Inpatient = 1`, `Icu = 2` | — |
| `CliTransferHandoverStatus` | `NotSent = 1`, `Sent = 2`, `Accepted = 3`, `Rejected = 4` | `NotSent` |

Sisi penandaan disimpan sebagai `varchar(30)` berisi `Left`, `Right`, `Bilateral`, `NotApplicable` agar sebanding langsung dengan `OprCase.Laterality`.

### 19.4 `OprWardPreOpNote` — `Baru` — satu baris per versi

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `OprCaseId` | `uuid` | Ya | — | Unique (`OprCaseId`, `VersionNumber`) | FK `OprCase` | `Restrict` | Tidak | — |
| `VersionNumber` | `integer` | Ya | `1` | Bagian unique | — | — | Tidak | Naik satu setiap versi baru setelah penundaan |
| `PreviousVersionId` | `uuid` | Tidak | `null` | Index | FK `OprWardPreOpNote` (diri sendiri) | `Restrict` | Tidak | Versi yang diperbarui |
| `Status` | `integer` | Ya | `1` `Draft` | Index (`OprCaseId`, `Status`) | — | — | Tidak | 19.3 |
| `VitalSnapshotJson` | `jsonb` | Tidak | `null` | — | — | — | **Ya** | Diisi saat dikirim dari `TrxPatientVitalSign` terakhir: TD, nadi, napas, suhu, SpO₂, waktu catat, `VitalSignId` |
| `PainSnapshotJson` | `jsonb` | Tidak | `null` | — | — | — | **Ya** | Skor, nama skala, waktu catat, rujukan respons instrumen; `null` bila belum pernah dinilai |
| `MarkingLaterality` | `varchar(30)` | Tidak | `null` | — | — | — | Tidak | Wajib saat kirim |
| `MarkingLocationNote` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Contoh "Kuadran kanan bawah abdomen" |
| `SiteMarkingConfirmed` | `boolean` | Ya | `false` | — | — | — | Tidak | Konfirmasi penerima atas penandaan |
| `SentByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | — |
| `SentAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `ConfirmedByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | `CK`: ≠ `SentByUserId` |
| `ConfirmedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `NeedsUpdateAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu kasus ditunda |
| `Version` | `integer` | Ya | `0` | — | — | — | Tidak | Konkurensi optimistis, pola OK |

**Unique parsial:** `UX_OprWardPreOpNote_OneOpen` pada `OprCaseId` dengan `Status IN (1, 2, 3)` — paling banyak satu versi yang masih berjalan per kasus.

### 19.5 `OprWardPreOpItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `NoteId` | `uuid` | Ya | — | Unique (`NoteId`, `PreparationItemId`) | FK `OprWardPreOpNote` | `Cascade` | Tidak | Butir ikut versinya |
| `PreparationItemId` | `uuid` | Ya | — | Index | FK `MstSurgicalPreparationItem` | `Restrict` | Tidak | — |
| `ItemNameSnapshot` | `varchar(200)` | Ya | — | — | — | — | Tidak | Nama saat versi dibuat, agar perubahan master tidak mengubah riwayat |
| `IsMandatorySnapshot` | `boolean` | Ya | — | — | — | — | Tidak | Sama |
| `SenderConfirmed` | `boolean` | Ya | `false` | — | — | — | Tidak | — |
| `ReceiverConfirmed` | `boolean` | Ya | `false` | — | — | — | Tidak | — |
| `ReceiverConfirmedByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | ≠ pengirim |
| `ReceiverConfirmedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `Note` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Contoh "Puasa sejak 22.00" |

### 19.6 `OprWardPreOpSiteMark` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `NoteId` | `uuid` | Ya | — | Index | FK `OprWardPreOpNote` | `Cascade` | Tidak | — |
| `BodyView` | `integer` | Ya | — | — | — | — | Tidak | 19.3 |
| `X` | `numeric(5,2)` | Ya | — | — | — | — | Tidak | 0–100, persen lebar gambar |
| `Y` | `numeric(5,2)` | Ya | — | — | — | — | Tidak | 0–100, persen tinggi gambar |
| `Label` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | — |

Foto tubuh pasien **tidak** disimpan (`RWI-DEC-174`).

### 19.7 `MstSurgicalPreparationItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `Code` | `varchar(30)` | Ya | — | Unique (bukan terhapus) | — | — | Tidak | Contoh `SPI-ID-01` |
| `GroupName` | `varchar(100)` | Ya | — | Index | — | — | Tidak | Verifikasi pasien, Persiapan fisik, Hasil pemeriksaan, Persiapan lain |
| `ItemName` | `varchar(200)` | Ya | — | — | — | — | Tidak | Contoh "Gelang identitas terpasang" |
| `IsMandatory` | `boolean` | Ya | `true` | — | — | — | Tidak | — |
| `SortOrder` | `integer` | Ya | `0` | — | — | — | Tidak | — |
| `Description` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | — |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | — |
| `RowVersion` | `xmin` | Ya | sistem | — | — | — | Tidak | Konkurensi, pola master |

### 19.8 `InpAdmissionReferral` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `PatientId` | `uuid` | Ya | — | Unique parsial `Status = 1` | FK `MstPatient` | `Restrict` | Tidak | Satu permintaan `Pending` per pasien |
| `SourceEncounterId` | `uuid` | Ya | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunjungan asal (poliklinik, IGD, ODC) = `OprCase.EncounterId` |
| `OprCaseId` | `uuid` | Ya | — | Unique parsial `Status = 1` | FK `OprCase` | `Restrict` | Tidak | — |
| `PrimarySurgeonId` | `uuid` | Ya | — | — | FK `MstDoctor` | `Restrict` | Tidak | Usulan DPJP awal; petugas admisi tetap memilih |
| `RequestedCareLevel` | `integer` | Ya | — | — | — | — | Tidak | 19.3 |
| `RecoveryDecisionNote` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Salinan `OprRecovery.DecisionNote` saat dibuat |
| `Status` | `integer` | Ya | `1` `Pending` | Index (`Status`, `RequestedAt`) | — | — | Tidak | — |
| `RequestedAt` | `timestamp with time zone` | Ya | — | Bagian index | — | — | Tidak | Dasar "lamanya menunggu" |
| `CancelledAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `CancelledByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | — |
| `CancelledReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Contoh "Pasien boleh pulang dari kamar pulih" |
| `CompletedEpisodeId` | `uuid` | Tidak | `null` | Unique (bila terisi) | FK `InpEpisode` | `Restrict` | Tidak | — |
| `CompletedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `RowVersion` | `xmin` | Ya | sistem | — | — | — | Tidak | — |

**Constraint:** `CK_InpAdmissionReferral_State` — `Status = 2` ⇒ `CompletedEpisodeId` terisi; `Status = 3` ⇒ `CancelledReason` terisi.

### 19.9 `CliTransferHandover` — `Baru` (`P2`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | Index (`InpEpisodeId`, `Status`) | FK `InpEpisode` | `Restrict` | Tidak | — |
| `FromPlacementId` | `uuid` | Ya | — | — | FK `InpBedPlacement` | `Restrict` | Tidak | — |
| `ToPlacementId` | `uuid` | Ya | — | **Unique** | FK `InpBedPlacement` | `Restrict` | Tidak | Satu dokumen per penempatan tujuan (idempotensi `INT-RWF-26`) |
| `FromServiceUnitId` | `uuid` | Ya | — | — | FK `MstServiceUnit` | `Restrict` | Tidak | — |
| `ToServiceUnitId` | `uuid` | Ya | — | Index (`ToServiceUnitId`, `Status`) | FK `MstServiceUnit` | `Restrict` | Tidak | — |
| `Status` | `integer` | Ya | `1` `NotSent` | Bagian index | — | — | Tidak | — |
| `SoapSummary` | `varchar(4000)` | Tidak | `null` | — | — | — | **Ya** | Kondisi pasien |
| `HandedItems` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | Barang yang diserahkan |
| `SpecialInstructions` | `varchar(2000)` | Tidak | `null` | — | — | — | **Ya** | — |
| `SnapshotJson` | `jsonb` | Tidak | `null` | — | — | — | **Ya** | Dibekukan saat dikirim: GCS, tanda vital, nyeri, risiko jatuh, balance cairan, beserta rujukan catatan sumber |
| `SentByUserId`, `SentAt` | `uuid`, `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Pelaksana |
| `ReceivedByUserId`, `ReceivedAt` | `uuid`, `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Penerima; `CK` ≠ pengirim |
| `RejectionReason` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** | — |
| `Version` | `integer` | Ya | `0` | — | — | — | Tidak | Konkurensi |

### 19.10 `MstInpatientSetting` — `Diperbarui` — sumber lengkap `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `PendingSurgicalHandoverAlertMinutes` | `integer` | Ya | `60` | — | — | — | Tidak | 1–1440. Serah terima pasca operasi yang belum diterima lebih dari ini tampil di daftar pantau |
| `PendingAdmissionReferralAlertMinutes` | `integer` | Ya | `30` | — | — | — | Tidak | 1–1440. Sama untuk permintaan admisi |

### 19.11 Tabel `Sudah ada` yang dipakai aturan baru — kolom kunci saja

| Tabel | File model | Kolom yang dipakai |
|---|---|---|
| `OprHandover` | `OperatingRoomManagement/Models/OprHandover.cs` | `Id`, `OprCaseId`, `DestinationUnitId`, `Status`, `SentBy`, `SentAt`, `ReceivedBy`, `AcceptedAt`, `RejectionReason`, `InstructionSummary` |
| `OprCaseProcedure` | `OperatingRoomManagement/Models/OprCaseProcedure.cs` | `OprCaseId`, `PatientProcedureId`, `IsPrimary` |
| `OprExecutionRecord` | `OperatingRoomManagement/Models/OprExecutionRecord.cs` | `Status`, `PostDiagnosis`, `Findings`, `Complications`, `BloodLossMl`, `ImplantDrainNote`, `PostPlan`, `FinishedAt` |
| `OprRecovery` | `OperatingRoomManagement/Models/OprRecovery.cs` | `ScoreSystem`, `ScoreValue`, `Decision`, `DecisionNote`, `ReleasedAt` |
| `OprIntegrationDelivery` | `OperatingRoomManagement/Models/OprIntegrationDelivery.cs` | Kunci `case:charge:component:revision`, `Status` |
| `OprStatusHistory` | `OperatingRoomManagement/Models/OprStatusHistory.cs` | `ToStatus`, `Action`, `Reason`, `OccurredAt` — sumber `LastStatusReason` |
| `InpBedPlacement` | `InPatientManagement/Models/InpBedPlacement.cs` | `InpEpisodeId`, `BedId`, `RoomId`, `ServiceUnitId`, `StartDateTime`, `EndDateTime`, `TransferReason`, `IsSuperseded`, `CorrectsPlacementId` (`integrasi-billing` `1.1.0`) |
| `TrxPatientProcedure` | `ClinicalManagement/Models/TrxPatientProcedure.cs` | `Id`, `EncounterId`, `ProcedureStatus`, pelaksana dan waktu pelaksanaan |

### 19.12 Skema DDL revision `0.6`

> **Peringatan.** Dokumentasi bentuk, bukan skrip yang dijalankan. Skema sungguhan lahir dari EF Core migration masing-masing modul pemilik. Kolom warisan `IdentityModel` tidak ditulis ulang.

```sql
-- E5 — OperatingRoomManagement (sesudah E4)
ALTER TABLE public."OprCase" ADD COLUMN "SurgicalServiceType" integer NOT NULL DEFAULT 1;
ALTER TABLE public."OprCase" ADD COLUMN "PlannedAnesthesiaType" integer NULL;
ALTER TABLE public."OprCase" ADD COLUMN "RejectedAt" timestamp with time zone NULL;
ALTER TABLE public."OprCase" ADD COLUMN "RejectedByUserId" uuid NULL;
ALTER TABLE public."OprCase" ADD COLUMN "RejectionReason" varchar(500) NULL;
ALTER TABLE public."OprCase" ADD CONSTRAINT "CK_OprCase_Rejected"
    CHECK ("Status" <> 8 OR ("RejectedAt" IS NOT NULL AND "RejectedByUserId" IS NOT NULL AND "RejectionReason" IS NOT NULL));

CREATE TABLE public."OprWardPreOpNote" (
    "Id" uuid PRIMARY KEY,
    "OprCaseId" uuid NOT NULL REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT,
    "VersionNumber" integer NOT NULL DEFAULT 1,
    "PreviousVersionId" uuid NULL REFERENCES public."OprWardPreOpNote" ("Id") ON DELETE RESTRICT,
    "Status" integer NOT NULL DEFAULT 1,
    "VitalSnapshotJson" jsonb NULL,
    "PainSnapshotJson" jsonb NULL,
    "MarkingLaterality" varchar(30) NULL,
    "MarkingLocationNote" varchar(500) NULL,
    "SiteMarkingConfirmed" boolean NOT NULL DEFAULT false,
    "SentByUserId" uuid NULL, "SentAt" timestamp with time zone NULL,
    "ConfirmedByUserId" uuid NULL, "ConfirmedAt" timestamp with time zone NULL,
    "NeedsUpdateAt" timestamp with time zone NULL,
    "Version" integer NOT NULL DEFAULT 0,
    CONSTRAINT "CK_OprWardPreOpNote_TwoAccounts" CHECK ("ConfirmedByUserId" IS NULL OR "ConfirmedByUserId" <> "SentByUserId"));
CREATE UNIQUE INDEX "UX_OprWardPreOpNote_Case_Version" ON public."OprWardPreOpNote" ("OprCaseId", "VersionNumber");
CREATE UNIQUE INDEX "UX_OprWardPreOpNote_OneOpen" ON public."OprWardPreOpNote" ("OprCaseId") WHERE "Status" IN (1, 2, 3) AND NOT "IsDelete";

CREATE TABLE public."OprWardPreOpItem" (
    "Id" uuid PRIMARY KEY,
    "NoteId" uuid NOT NULL REFERENCES public."OprWardPreOpNote" ("Id") ON DELETE CASCADE,
    "PreparationItemId" uuid NOT NULL REFERENCES public."MstSurgicalPreparationItem" ("Id") ON DELETE RESTRICT,
    "ItemNameSnapshot" varchar(200) NOT NULL,
    "IsMandatorySnapshot" boolean NOT NULL,
    "SenderConfirmed" boolean NOT NULL DEFAULT false,
    "ReceiverConfirmed" boolean NOT NULL DEFAULT false,
    "ReceiverConfirmedByUserId" uuid NULL, "ReceiverConfirmedAt" timestamp with time zone NULL,
    "Note" varchar(500) NULL);
CREATE UNIQUE INDEX "UX_OprWardPreOpItem_Note_Item" ON public."OprWardPreOpItem" ("NoteId", "PreparationItemId");

CREATE TABLE public."OprWardPreOpSiteMark" (
    "Id" uuid PRIMARY KEY,
    "NoteId" uuid NOT NULL REFERENCES public."OprWardPreOpNote" ("Id") ON DELETE CASCADE,
    "BodyView" integer NOT NULL,
    "X" numeric(5,2) NOT NULL CHECK ("X" BETWEEN 0 AND 100),
    "Y" numeric(5,2) NOT NULL CHECK ("Y" BETWEEN 0 AND 100),
    "Label" varchar(100) NULL);

-- E4 — MasterData (dijalankan lebih dulu)
CREATE TABLE public."MstSurgicalPreparationItem" (
    "Id" uuid PRIMARY KEY,
    "Code" varchar(30) NOT NULL,
    "GroupName" varchar(100) NOT NULL,
    "ItemName" varchar(200) NOT NULL,
    "IsMandatory" boolean NOT NULL DEFAULT true,
    "SortOrder" integer NOT NULL DEFAULT 0,
    "Description" varchar(500) NULL,
    "IsActive" boolean NOT NULL DEFAULT true);
CREATE UNIQUE INDEX "UX_MstSurgicalPreparationItem_Code" ON public."MstSurgicalPreparationItem" ("Code") WHERE NOT "IsDelete";
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "PendingSurgicalHandoverAlertMinutes" integer NOT NULL DEFAULT 60;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "PendingAdmissionReferralAlertMinutes" integer NOT NULL DEFAULT 30;

-- E6 — InPatientManagement
CREATE TABLE public."InpAdmissionReferral" (
    "Id" uuid PRIMARY KEY,
    "PatientId" uuid NOT NULL REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    "SourceEncounterId" uuid NOT NULL REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    "OprCaseId" uuid NOT NULL REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT,
    "PrimarySurgeonId" uuid NOT NULL REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    "RequestedCareLevel" integer NOT NULL,
    "RecoveryDecisionNote" varchar(2000) NULL,
    "Status" integer NOT NULL DEFAULT 1,
    "RequestedAt" timestamp with time zone NOT NULL,
    "CancelledAt" timestamp with time zone NULL, "CancelledByUserId" uuid NULL, "CancelledReason" varchar(500) NULL,
    "CompletedEpisodeId" uuid NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "CompletedAt" timestamp with time zone NULL,
    CONSTRAINT "CK_InpAdmissionReferral_State" CHECK (("Status" <> 2 OR "CompletedEpisodeId" IS NOT NULL) AND ("Status" <> 3 OR "CancelledReason" IS NOT NULL)));
CREATE UNIQUE INDEX "UX_InpAdmissionReferral_Patient_Pending" ON public."InpAdmissionReferral" ("PatientId") WHERE "Status" = 1 AND NOT "IsDelete";
CREATE UNIQUE INDEX "UX_InpAdmissionReferral_Case_Pending" ON public."InpAdmissionReferral" ("OprCaseId") WHERE "Status" = 1 AND NOT "IsDelete";
CREATE UNIQUE INDEX "UX_InpAdmissionReferral_CompletedEpisode" ON public."InpAdmissionReferral" ("CompletedEpisodeId") WHERE "CompletedEpisodeId" IS NOT NULL;
CREATE INDEX "IX_InpAdmissionReferral_Status_RequestedAt" ON public."InpAdmissionReferral" ("Status", "RequestedAt");

-- E7 — ClinicalManagement (P2)
CREATE TABLE public."CliTransferHandover" (
    "Id" uuid PRIMARY KEY,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "FromPlacementId" uuid NOT NULL REFERENCES public."InpBedPlacement" ("Id") ON DELETE RESTRICT,
    "ToPlacementId" uuid NOT NULL REFERENCES public."InpBedPlacement" ("Id") ON DELETE RESTRICT,
    "FromServiceUnitId" uuid NOT NULL REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT,
    "ToServiceUnitId" uuid NOT NULL REFERENCES public."MstServiceUnit" ("Id") ON DELETE RESTRICT,
    "Status" integer NOT NULL DEFAULT 1,
    "SoapSummary" varchar(4000) NULL, "HandedItems" varchar(2000) NULL, "SpecialInstructions" varchar(2000) NULL,
    "SnapshotJson" jsonb NULL,
    "SentByUserId" uuid NULL, "SentAt" timestamp with time zone NULL,
    "ReceivedByUserId" uuid NULL, "ReceivedAt" timestamp with time zone NULL,
    "RejectionReason" varchar(1000) NULL,
    "Version" integer NOT NULL DEFAULT 0,
    CONSTRAINT "CK_CliTransferHandover_TwoAccounts" CHECK ("ReceivedByUserId" IS NULL OR "ReceivedByUserId" <> "SentByUserId"));
CREATE UNIQUE INDEX "UX_CliTransferHandover_ToPlacement" ON public."CliTransferHandover" ("ToPlacementId");
CREATE INDEX "IX_CliTransferHandover_Episode_Status" ON public."CliTransferHandover" ("InpEpisodeId", "Status");
CREATE INDEX "IX_CliTransferHandover_ToUnit_Status" ON public."CliTransferHandover" ("ToServiceUnitId", "Status");
```

**Urutan:** `E4` (master) → `E5` (OK) → `E6` → `E7`, karena `OprWardPreOpItem` merujuk `MstSurgicalPreparationItem` dan `InpAdmissionReferral` merujuk `OprCase`. Blok DDL di atas ditulis per pemilik, bukan per urutan jalan. Urutan lintas sub-modul ada di `../02-module-map.md` bagian 7.4.

### 19.13 Penyelarasan decision log revision `31` ★ 2 Oktober 2026

Tidak ada kolom atau tabel baru milik sub-modul ini. `InpAdmissionReferral` (19.8) kini dirujuk tabel Billing `BilInvoiceEncounterLink.SourceReferralId` (`Restrict`; `integrasi-billing` kamus data 6.9, `RWI-DEC-207`), sehingga baris permintaan admisi yang sudah tertaut tidak dapat dihapus fisik — sejalan dengan aturan hapus `IsDelete` di kepala dokumen.

---

## 20. Amandemen revision `0.7` / kontrak `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
| --- | --- |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.10` bagian 13 |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`). Bagian 19 (`0.6`) tetap `approved` (`RWI-DEC-221`) |
| Backend SHA | `fdf85a07` (capability map bagian 20 diaudit pada `671191eb`, `RWI-FACT-065`) |
| Keputusan | `RWI-DEC-228` s.d. `264`; `RWI-FACT-067` |

Aturan kepala dokumen tetap berlaku: kolom warisan `IdentityModel` tidak diulang; hapus berarti `IsDelete`, dan alur bisnis Workspace PPRI **tidak pernah** memakai hapus. Enum disimpan sebagai `integer`. Waktu disimpan `timestamp with time zone` dalam UTC dan ditampilkan menurut zona waktu profil rumah sakit (bawaan `Asia/Jakarta`).

**Arti kolom Sensitif pada bagian ini.** Bertanda **Ya** berarti data pribadi pasien atau keluarga, isi pernyataan yang menyangkut keyakinan dan keuangan pribadi, atau salinan beku yang memuat keduanya. Kolom itu tidak masuk custom logger, tidak dipakai sebagai contoh berisi data asli, dan disamarkan di layar daftar bila berupa nomor identitas (`NFR-RWA-06`).

### 20.1 Ringkasan tabel

| Tabel | Status | Pemilik | Gelombang | Bagian |
|---|---|---|---|---|
| `InpAdmissionDocument` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.2 |
| `InpAdmissionDocumentSignature` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.3 |
| `InpAdmissionDocumentParty` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.4 |
| `InpAdmissionHandoverItem` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.5 |
| `InpAdmissionPrivacyRequest` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.6 |
| `InpAdmissionPrivacyEntry` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.7 |
| `InpAdmissionBeliefItem` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.8 |
| `InpAdmissionCostDifferenceStatement` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.9 |
| `InpAdmissionDepositStatement` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.10 |
| `InpAdmissionCostEstimate` | `Baru` | `InPatientManagement` | Di luar gelombang (`EPIC-RWA-09`) | 20.11 |
| `InpAdmissionCostEstimateLine` | `Baru` | `InPatientManagement` | Di luar gelombang | 20.12 |
| `InpAdmissionPrintLog` | `Baru` | `InPatientManagement` | `RWA-MVP-0` | 20.13 |
| `InpAdmissionProcedurePlanMark` | `Baru` | `InPatientManagement` | Di luar gelombang | 20.14 |
| `MstInpatientClearanceItem` | `Diperbarui` | `MasterData` | `RWA-MVP-0` | 20.15 |
| `MstInpatientSetting` | `Diperbarui` | `MasterData` | `RWA-MVP-0` | 20.16 |
| `InpEpisode`, `InpBedPlacement`, `InpStatusHistory`, `InpDoctorAssignment`, `InpNurseAssignment`, `MstPatient`, `MstPatientRelationship`, `MstPatientEmergencyContact`, `RegPatientEncounter`, `RegPatientEncounterGuarantor`, `CliDoctorCertificate`, `OprCase`, `MstHospitalSite`, `MstTariff`, `MstReferralDoctor` | `Sudah ada` — **dibaca lewat service pemilik, tidak diubah** | Masing-masing | — | 20.17 |

### 20.2 `InpAdmissionDocument` — `Baru` — satu baris per versi dokumen

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `EpisodeId` | `uuid` | Ya | — | Unique parsial dokumen aktif; index (`EpisodeId`, `DocumentType`, `CreateDateTime`) | FK `InpEpisode` | `Restrict` | Tidak | Episode pemilik dokumen |
| `PatientId` | `uuid` | Ya | — | Index (`PatientId`, `DocumentType`, `Status`) | FK `MstPatient` | `Restrict` | Tidak | Diisi server dari episode. Dipakai mencari Nilai Kepercayaan `Completed` terakhir pasien lintas episode (`RWI-DEC-242`) |
| `DocumentType` | `integer` | Ya | — | Bagian index | — | — | Tidak | `InpAdmissionDocumentType` |
| `Status` | `integer` | Ya | `1` `Draft` | Bagian filter unique parsial | — | — | Tidak | `InpAdmissionDocumentStatus` |
| `VersionNo` | `integer` | Ya | `1` | — | — | — | Tidak | Versi di dalam satu rantai koreksi: versi baru = versi sebelumnya + 1, dijaga `RowVersion` versi sebelumnya dan unique `PreviousVersionId`. Bukan nomor bisnis; rantai baru sesudah pembatalan mulai dari 1 |
| `PreviousVersionId` | `uuid` | Tidak | `null` | **Unique** bila terisi | FK `InpAdmissionDocument` (diri sendiri) | `Restrict` | Tidak | Versi yang dikoreksi; satu versi hanya punya satu pengganti |
| `CorrectionReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `VersionNo > 1`; 10–500 karakter |
| `SigningCity` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Bawaan dari `MstInpatientSetting.DocumentSigningCity` saat dibuat; boleh diubah selama `Draft` |
| `StatementDate` | `date` | Tidak | `null` | — | — | — | Tidak | "Tanggal" pada formulir; tanggal surat Pelunasan Deposit. Wajib saat kunci untuk Privasi, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit |
| `Note` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** | "Keterangan" atau "Catatan" formulir |
| `SnapshotFormatVersion` | `integer` | Tidak | `null` | — | — | — | Tidak | Bentuk `SnapshotJson`; `1` pada revision ini |
| `SnapshotJson` | `jsonb` | Tidak | `null` | — | — | — | **Ya** | Salinan beku saat dikunci (20.2.1); dikosongkan saat buka kunci |
| `LockedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu kunci terakhir |
| `LockedByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | — |
| `CompletedAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu slot wajib terakhir terisi |
| `SupersededAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu versi koreksi dibuat |
| `CancelledAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | — |
| `CancelledByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | — |
| `CancelledReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Buang konsep: 1–500 karakter; batal dokumen terkunci: 10–500 karakter |
| `IdempotencyKey` | `varchar(80)` | Tidak | `null` | Unique bila terisi dan belum dihapus | — | — | Tidak | Dari header `Idempotency-Key` simpan konsep atau versi koreksi |
| `RowVersion` | `uuid` | Ya | baru | — | — | — | Tidak | Token konkurensi; berganti setiap kali dokumen atau anaknya diubah |

**Constraint:**

- `UX_InpAdmissionDocument_Episode_Type_Active` — unique (`EpisodeId`, `DocumentType`) dengan filter `"Status" IN (1, 2, 3) AND NOT "IsDelete"` (`INV-RWA-01`).
- `CK_InpAdmissionDocument_State` — `Status = 2` ⇒ `LockedAt` dan `SnapshotJson` terisi; `Status = 3` ⇒ `CompletedAt` dan `SnapshotJson` terisi; `Status = 4` ⇒ `SupersededAt` terisi; `Status = 5` ⇒ `CancelledAt` dan `CancelledReason` terisi; `VersionNo > 1` ⇒ `PreviousVersionId` dan `CorrectionReason` terisi.

**Contoh.** Selisih Biaya Tn. Budi versi 1 `Completed` pukul 10.30. Pukul 13.00 Sari membuat versi koreksi beralasan "koreksi alamat deklarer". Satu transaksi mengubah versi 1 menjadi `Superseded` (`SupersededAt` 13.00), lalu membuat versi 2 `Draft` dengan `PreviousVersionId` = versi 1. Unique index dokumen aktif tetap berisi satu baris.

#### 20.2.1 Bentuk `SnapshotJson` — `SnapshotFormatVersion = 1`

Dibentuk `InpAdmissionSnapshotBuilder` saat dokumen dikunci. Isinya **hanya** yang dicetak jenis dokumen itu (`RWI-DEC-257` butir 2). Nomor identitas pasien **tidak** dibekukan, karena tidak dicetak dokumen admisi mana pun.

```json
{
  "formatVersion": 1,
  "frozenAt": "2026-10-09T03:00:00Z",
  "documentVersionNo": 1,
  "letterhead": { "siteName": "<nama rumah sakit>", "addressLines": ["<alamat>", "<kota, provinsi>"], "phoneNumber": "<telepon>", "email": "<email>" },
  "formCode": "005/NM/E/Rev01/XI/2016",
  "signingCity": "<kota>",
  "patient": { "fullName": "Budi Santoso", "salutation": "Tn.", "medicalRecordNumber": "00-12-34-56", "birthDate": "1981-03-12", "gender": "Male", "religion": "Islam", "address": "<alamat pasien>" },
  "episode": { "episodeNumber": "RI-261007-0001", "admittedAt": "2026-10-07T01:15:00Z", "patientClassName": "Kelas 2", "roomName": "Melati 03", "bedName": "B", "serviceUnitName": "Melati", "attendingDoctorName": "dr. Andika" },
  "guarantor": { "paymentType": "Insurance", "guarantorName": "PT Asuransi Sehat Sentosa", "policyNumber": "<polis>", "memberNumber": "<peserta>", "cardNumber": "7788-0012-3456" },
  "deposit": { "minimumPolicyAmount": 5000000, "receivedAmount": 2000000, "shortfallAmount": 3000000, "amountsReadAt": "2026-10-09T03:00:00Z" }
}
```

Bagian `guarantor` hanya untuk Selisih Biaya, Pelunasan Deposit, dan Estimasi Biaya; bagian `deposit` hanya untuk Pelunasan Deposit (angkanya juga disimpan bertipe di 20.10). Data samaran di atas mengikuti PRD bagian 18.

### 20.3 `InpAdmissionDocumentSignature` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | Unique (`DocumentId`, `Slot`) belum dihapus | FK `InpAdmissionDocument` | `Restrict` | Tidak | — |
| `Slot` | `integer` | Ya | — | Bagian unique | — | — | Tidak | `InpAdmissionSignatureSlot` |
| `Method` | `integer` | Ya | — | — | — | — | Tidak | `PaperRecorded` untuk slot pasien/keluarga; `ElectronicAttestation` untuk slot petugas |
| `SignerName` | `varchar(200)` | Ya | — | — | — | — | **Ya** | Kertas: nama yang menandatangani lembar. Atestasi: `DisplayName` akun saat menandatangani (dibekukan) |
| `SignerPositionName` | `varchar(150)` | Tidak | `null` | — | — | — | Tidak | Atestasi: jabatan utama akun (`PrimaryPosition.PositionName`) saat menandatangani |
| `SignerRelationship` | `integer` | Tidak | `null` | — | — | — | Tidak | Kertas: `InpAdmissionPartyRelationship` |
| `SignerRelationshipText` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Kertas: teks hubungan bila `Other`, misalnya "adik ipar" |
| `SignedAt` | `timestamp with time zone` | Ya | — | — | — | — | Tidak | Kertas: waktu tanda tangan di lembar, diisi petugas, tidak sebelum `LockedAt` dan tidak di masa depan. Atestasi: waktu server |
| `SignedByUserId` | `uuid` | Tidak | `null` | **Unique** (`DocumentId`, `SignedByUserId`) bila terisi dan belum dihapus | Akun pengguna | — | Tidak | Atestasi: akun penanda tangan (`INV-RWA-04`) |
| `VerifiedByUserId` | `uuid` | Tidak | `null` | — | Akun pengguna | — | Tidak | Kertas: petugas yang memeriksa lembar dan mencatatnya |
| `RecordedAt` | `timestamp with time zone` | Ya | — | — | — | — | Tidak | Waktu server saat baris dicatat |
| `IdempotencyKey` | `varchar(80)` | Tidak | `null` | Unique bila terisi dan belum dihapus | — | — | Tidak | — |

**Constraint:** `CK_InpAdmissionDocumentSignature_Method` — `Method = 1` ⇒ `Slot = 1`, `VerifiedByUserId` terisi, `SignedByUserId` kosong; `Method = 2` ⇒ `Slot <> 1`, `SignedByUserId` terisi.

**Contoh.** Pada Selisih Biaya Tn. Budi tercatat dua baris: (1) slot 1, kertas, "Rina Santoso", `Spouse`, ditandatangani 10.15, diverifikasi Sari; (2) slot 2, atestasi, "Sari Wulandari", "Petugas Admisi", 10.20, akun Sari. Dua baris itu sah walaupun keduanya melibatkan Sari, karena verifikasi kertas bukan slot petugas (`RWI-DEC-239`).

### 20.4 `InpAdmissionDocumentParty` — `Baru` — penanda tangan atau deklarer yang dinyatakan dokumen

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | **Unique** | FK `InpAdmissionDocument` | `Restrict` | Tidak | Satu pihak per dokumen |
| `SourceType` | `integer` | Ya | `4` `Manual` | — | — | — | Tidak | `InpAdmissionPartySource` |
| `SourceRecordId` | `uuid` | Tidak | `null` | — | **Tanpa FK** | — | Tidak | Id relasi atau kontak darurat asal. Jejak saja; tidak dipakai membaca ulang, karena isi pernyataan beku sejak disimpan |
| `FullName` | `varchar(150)` | Ya | — | — | — | — | **Ya** | Nama penanda tangan atau deklarer |
| `Relationship` | `integer` | Tidak | `null` | — | — | — | Tidak | `InpAdmissionPartyRelationship`. Wajib untuk Nilai Kepercayaan |
| `RelationshipText` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Teks hubungan bila `Other` atau berasal dari kontak darurat |
| `Address` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib untuk Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit |
| `BirthDate` | `date` | Tidak | `null` | — | — | — | **Ya** | Nilai Kepercayaan; umur dihitung, tidak disimpan |
| `Gender` | `integer` | Tidak | `null` | — | — | — | Tidak | Enum `Gender` yang sudah ada. Wajib untuk Nilai Kepercayaan |
| `Occupation` | `varchar(100)` | Tidak | `null` | — | — | — | **Ya** | Selisih Biaya |
| `IdentityType` | `integer` | Tidak | `null` | — | — | — | Tidak | `InpAdmissionPartyIdentityType`. Wajib untuk Selisih Biaya |
| `IdentityNumber` | `varchar(50)` | Tidak | `null` | — | — | — | **Ya** | Wajib untuk Selisih Biaya. Disamarkan di daftar, misalnya `3275•••••••••001` |
| `MobilePhone` | `varchar(13)` | Tidak | `null` | — | — | — | **Ya** | Angka saja, maksimal 13 digit; tanda hubung dan spasi dibuang server (`FR-RWA-082`). Wajib untuk Pelunasan Deposit |
| `OfficePhone` | `varchar(20)` | Tidak | `null` | — | — | — | **Ya** | Selisih Biaya |

**Pemakaian per jenis:** Privasi = `FullName` (Nama Penanda Tangan); Nilai Kepercayaan = nama, tanggal lahir, jenis kelamin, hubungan, alamat; Selisih Biaya = nama, alamat, pekerjaan, jenis dan nomor identitas, telepon; Pelunasan Deposit = nama, alamat, telepon, sumber data. Serah Terima dan Estimasi Biaya tidak punya pihak.

### 20.5 `InpAdmissionHandoverItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | Unique (`DocumentId`, `ClearanceItemId`); unique (`DocumentId`, `LineNo`) | FK `InpAdmissionDocument` | `Restrict` | Tidak | — |
| `ClearanceItemId` | `uuid` | Ya | — | Bagian unique | FK `MstInpatientClearanceItem` | `Restrict` | Tidak | Butir master asal, jenis `NewPatientHandover` |
| `LineNo` | `integer` | Ya | — | Bagian unique | — | — | Tidak | Urutan baris pada lembar (1..*n*), dibekukan saat dibuat — urutan bisnis cetakan, bukan `SortOrder` presentasi |
| `ItemNumberSnapshot` | `integer` | Tidak | `null` | — | — | — | Tidak | Nomor butir induk tercetak 1..15; kosong untuk sub-butir (dicetak berpoin) |
| `ParentItemNumberSnapshot` | `integer` | Tidak | `null` | — | — | — | Tidak | Nomor induk sub-butir, misalnya `2` |
| `ItemCodeSnapshot` | `varchar(50)` | Ya | — | — | — | — | Tidak | Kode butir saat dibuat, misalnya `STPB-13` |
| `ItemNameSnapshot` | `varchar(200)` | Ya | — | — | — | — | Tidak | Nama butir saat dibuat, misalnya "PASANG GELANG" |
| `Choice` | `integer` | Tidak | `null` | — | — | — | Tidak | `InpHandoverItemChoice`; kosong = belum dipilih. Wajib terisi saat kunci |
| `Note` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | Keterangan; wajib bila `NotDone` saat kunci |

Saran sistem **tidak disimpan**; ia dihitung saat dibaca dari sumber saran master dan hanya membantu petugas memilih (`RWI-DEC-241` butir 2).

### 20.6 `InpAdmissionPrivacyRequest` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | **Unique** | FK `InpAdmissionDocument` | `Restrict` | Tidak | Dokumen Permintaan Privasi |
| `IsTransportPrivacyRequested` | `boolean` | Ya | `false` | — | — | — | Tidak | "Privasi selama Transportasi" Ya/Tidak; bawaan Tidak (PRD Lampiran A.5) |

### 20.7 `InpAdmissionPrivacyEntry` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | Unique (`DocumentId`, `EntryType`, `LineNo`) | FK `InpAdmissionDocument` | `Restrict` | Tidak | — |
| `EntryType` | `integer` | Ya | — | Bagian unique | — | — | Tidak | `AllowedVisitor` atau `SpecialServiceRequest` |
| `LineNo` | `integer` | Ya | — | Bagian unique | — | — | Tidak | 1–3, mengikuti tiga baris V1 (G-40); `CHECK` 1–3 |
| `Text` | `varchar(200)` | Ya | — | — | — | — | **Ya** | Satu nama kerabat atau satu permintaan, utuh — "Sdr. Dimas, Jr." tetap satu baris |

### 20.8 `InpAdmissionBeliefItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | Unique (`DocumentId`, `ItemNo`) | FK `InpAdmissionDocument` | `Restrict` | Tidak | — |
| `ItemNo` | `integer` | Ya | — | Bagian unique | — | — | Tidak | Nomor butir tercetak 1–5; `CHECK` 1–5 |
| `Text` | `varchar(500)` | Ya | — | — | — | — | **Ya** | Contoh "Tidak menerima transfusi darah" |

### 20.9 `InpAdmissionCostDifferenceStatement` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | **Unique** | FK `InpAdmissionDocument` | `Restrict` | Tidak | Dokumen Selisih Biaya |
| `Subject` | `integer` | Ya | — | — | — | — | Tidak | `InpCostDifferenceSubject`: diri saya sendiri, istri saya, suami saya, anak saya, saudara kandung lainnya |
| `SubjectOtherText` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Wajib bila `OtherSibling`, misalnya "kakak kandung" |

### 20.10 `InpAdmissionDepositStatement` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | **Unique** | FK `InpAdmissionDocument` | `Restrict` | Tidak | Dokumen Pelunasan Deposit |
| `DueAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Jatuh tempo = tanggal pilihan petugas pukul 11.00 waktu rumah sakit, disimpan UTC. Wajib saat kunci |
| `PolicyFollowUpIntervalDays` | `integer` | Tidak | `null` | — | — | — | Tidak | Interval kebijakan deposit yang dipakai membatasi jatuh tempo; dibekukan saat kunci |
| `MinimumPolicyAmount` | `numeric(18,2)` | Tidak | `null` | — | — | — | **Ya** | Dari Billing, dibekukan saat kunci |
| `ReceivedAmount` | `numeric(18,2)` | Tidak | `null` | — | — | — | **Ya** | Sama |
| `ShortfallAmount` | `numeric(18,2)` | Tidak | `null` | — | — | — | **Ya** | Sama; harus > 0 saat kunci |
| `AmountsReadAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu angka dibaca dari Billing |

**Contoh perhitungan jatuh tempo.** Surat bertanggal Jumat 9 Oktober 2026, interval kebijakan 3 hari: batas = 12 Oktober; hari kerja berikutnya = Senin 12 Oktober; bawaan `DueAt` = 12 Oktober 11.00 WIB = `2026-10-12T04:00:00Z`. Dengan interval 1 hari, batas Sabtu 10 Oktober, sehingga bawaan dipotong menjadi Sabtu 10 Oktober 11.00 WIB (`RWI-DEC-248`).

### 20.11 `InpAdmissionCostEstimate` — `Baru` — di luar gelombang (`EPIC-RWA-09`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | **Unique** | FK `InpAdmissionDocument` | `Restrict` | Tidak | Dokumen Estimasi Biaya |
| `OprCaseId` | `uuid` | Tidak | `null` | Index | FK `OprCase` | `Restrict` | Tidak | Kasus OK asal isian kepala, bila ada (G-48) |
| `PlannedProcedureText` | `varchar(300)` | Tidak | `null` | — | — | — | **Ya** | "Jenis tindakan"; wajib saat kunci |
| `PlannedScheduleAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | "Jadwal tindakan" |
| `DoctorId` | `uuid` | Tidak | `null` | — | FK `MstDoctor` | `Restrict` | Tidak | "Dokter" |
| `PatientClassId` | `uuid` | Ya | — | — | FK `MstPatientClass` | `Restrict` | Tidak | Kelas acuan tarif; bawaan kelas episode |
| `EstimatedLengthOfStayDays` | `integer` | Ya | `1` | — | — | — | Tidak | 1–365 |
| `PricesReadAt` | `timestamp with time zone` | Tidak | `null` | — | — | — | Tidak | Waktu harga terakhir dibaca; dibekukan saat kunci |
| `NotesSnapshot` | `text` | Tidak | `null` | — | — | — | Tidak | Catatan aturan biaya yang dibentuk dari kebijakan Billing saat kunci (biaya admin). Catatan cito dan sejenisnya menunggu `DEC-INP-020` |

### 20.12 `InpAdmissionCostEstimateLine` — `Baru` — di luar gelombang

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `DocumentId` | `uuid` | Ya | — | Unique (`DocumentId`, `LineNo`) | FK `InpAdmissionDocument` | `Restrict` | Tidak | — |
| `LineNo` | `integer` | Ya | — | Bagian unique | — | — | Tidak | Urutan baris tercetak |
| `LineType` | `integer` | Ya | — | — | — | — | Tidak | `InpCostEstimateLineType` |
| `Description` | `varchar(300)` | Ya | — | — | — | — | Tidak | Uraian baris |
| `ProcedureId` | `uuid` | Tidak | `null` | — | FK `MstProcedure` | `Restrict` | Tidak | Baris tindakan |
| `TariffId` | `uuid` | Tidak | `null` | — | FK `MstTariff` | `Restrict` | Tidak | Baris kamar per hari |
| `DoctorId` | `uuid` | Tidak | `null` | — | FK `MstDoctor` | `Restrict` | Tidak | Baris visit dokter |
| `Quantity` | `numeric(10,2)` | Ya | `1` | — | — | — | Tidak | Hari atau jumlah; > 0 |
| `UnitPrice` | `numeric(18,2)` | Tidak | `null` | — | — | — | **Ya** | Kosong bila tarif tidak ditemukan |
| `PriceSource` | `integer` | Ya | — | — | — | — | Tidak | `Tariff`, `Manual`, `Unavailable`. Kunci ditolak selama ada `Unavailable` (`FR-RWA-092`) |
| `ManualReason` | `varchar(300)` | Tidak | `null` | — | — | — | Tidak | Wajib bila `Manual` |

**Constraint:** `CK_InpAdmissionCostEstimateLine_Price` — `PriceSource IN (1, 2)` ⇒ `UnitPrice` terisi; `PriceSource = 2` ⇒ `ManualReason` terisi.

### 20.13 `InpAdmissionPrintLog` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `EpisodeId` | `uuid` | Ya | — | Index (`EpisodeId`, `PrintKind`, `PrintedAt`) | FK `InpEpisode` | `Restrict` | Tidak | — |
| `DocumentId` | `uuid` | Tidak | `null` | Index (`DocumentId`, `PrintedAt`) | FK `InpAdmissionDocument` | `Restrict` | Tidak | Wajib bila `PrintKind = AdmissionDocument` |
| `PrintKind` | `integer` | Ya | — | Bagian index | — | — | Tidak | `InpAdmissionPrintKind` |
| `DocumentStatusAtPrint` | `integer` | Tidak | `null` | — | — | — | Tidak | Status dokumen saat dicetak; menentukan penanda cetakan |
| `Copies` | `integer` | Ya | `1` | — | — | — | Tidak | 1–10 |
| `IsReprint` | `boolean` | Ya | `false` | — | — | — | Tidak | Ditetapkan server: sudah ada cetakan sebelumnya untuk kunci yang sama (20.13.1), atau episode `Closed`/`Cancelled` |
| `ReprintReason` | `integer` | Tidak | `null` | — | — | — | Tidak | `InpReprintReason`; wajib bila `IsReprint` |
| `ReprintNote` | `varchar(200)` | Tidak | `null` | — | — | — | Tidak | Wajib bila alasan `Other` |
| `PrintedByUserId` | `uuid` | Ya | — | — | Akun pengguna | — | Tidak | — |
| `PrintedAt` | `timestamp with time zone` | Ya | — | Bagian index | — | — | Tidak | Waktu server |
| `IdempotencyKey` | `varchar(80)` | Tidak | `null` | Unique bila terisi dan belum dihapus | — | — | Tidak | Klik ganda tidak menambah baris |

**Constraint:** `CK_InpAdmissionPrintLog_Reprint` — `IsReprint` ⇒ `ReprintReason` terisi; `ReprintReason = 4` ⇒ `ReprintNote` terisi; `PrintKind = 5` ⇒ `DocumentId` terisi.

#### 20.13.1 Kunci "cetakan yang sama"

| `PrintKind` | Dianggap cetakan yang sama bila | "Cetakan ke-*n*" |
|---|---|---|
| `AdultWristband`, `InfantWristband` | `EpisodeId` sama dan jenis gelang sama | Urutan `PrintedAt` baris jenis itu pada episode |
| `PatientLabel`, `InpatientBaseData` | `EpisodeId` dan `PrintKind` sama | Sama |
| `AdmissionDocument` | `DocumentId` sama dan `DocumentStatusAtPrint` sama, untuk status `AwaitingSignature`, `Completed`, `Superseded`, `Cancelled`. Cetakan `Draft` tidak pernah dihitung cetak ulang | Sama |

**Contoh.** Gelang Budi dicetak Sari 09.50 (`IsReprint = false`). Pukul 15.10 Andi mencetak lagi dengan alasan "rusak": baris kedua `IsReprint = true`, `ReprintReason = Damaged`, dan layar menulis "Cetakan ke-2, rusak, oleh Andi, 15.10".

### 20.14 `InpAdmissionProcedurePlanMark` — `Baru` — di luar gelombang

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | — |
| `EpisodeId` | `uuid` | Ya | — | Unique bila `UnmarkedAt` kosong dan belum dihapus | FK `InpEpisode` | `Restrict` | Tidak | Satu penanda aktif per episode |
| `MarkedAt`, `MarkedByUserId` | `timestamp with time zone`, `uuid` | Ya | — | — | — | — | Tidak | — |
| `Note` | `varchar(300)` | Tidak | `null` | — | — | — | **Ya** | Contoh "rencana kemoterapi minggu depan" |
| `UnmarkedAt`, `UnmarkedByUserId` | `timestamp with time zone`, `uuid` | Tidak | `null` | — | — | — | Tidak | Diisi saat penanda dicabut; baris tidak dihapus |

### 20.15 `MstInpatientClearanceItem` — `Diperbarui` — seluruh kolom

Sumber lengkap `Areas/HealthServices/MasterData/Models/MstInpatientClearanceItem.cs`; configuration `Repositories/Configurations/HealthServices/MasterData/MstInpatientClearanceItemConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | Sudah ada |
| `ItemCode` | `varchar(50)` | Ya | — | **Unique** `IX_MstInpatientClearanceItem_ItemCode` | — | — | Tidak | Sudah ada; unik lintas jenis, sehingga kode serah terima memakai awalan `STPB-` |
| `ItemName` | `varchar(200)` | Ya | — | — | — | — | Tidak | Sudah ada |
| `Description` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | Sudah ada |
| `IsMandatory` | `boolean` | Ya | `true` | Index | — | — | Tidak | Sudah ada. Menahan penutupan hanya untuk jenis `EpisodeClosure`; untuk serah terima seluruh butir wajib dipilih (`RWI-DEC-241`) |
| `SortOrder` | `integer` | Ya | `0` | — | — | — | Tidak | Sudah ada — **legacy** (`TOUCHED LEGACY`); tetap dipakai sebagai urutan butir per jenis |
| `IsActive` | `boolean` | Ya | `true` | Index | — | — | Tidak | Sudah ada |
| `ChecklistType` | `integer` | Ya | **`1`** `EpisodeClosure` | **Baru** index (`ChecklistType`, `IsActive`) | — | — | Tidak | **Baru.** `MstClearanceChecklistType`; baris lama menjadi penutupan |
| `ParentItemId` | `uuid` | Tidak | `null` | **Baru** index | FK `MstInpatientClearanceItem` (diri sendiri) | `Restrict` | Tidak | **Baru.** Induk sub-butir; induk wajib satu jenis dan bukan sub-butir (kedalaman satu) |
| `HandoverSuggestionSource` | `integer` | Ya | **`0`** `None` | — | — | — | Tidak | **Baru.** `MstHandoverSuggestionSource`; hanya untuk jenis serah terima |

**Constraint:** `CK_MstInpatientClearanceItem_Type` — `ChecklistType = 1` ⇒ `ParentItemId` kosong dan `HandoverSuggestionSource = 0`.

### 20.16 `MstInpatientSetting` — `Diperbarui` — seluruh kolom

Sumber lengkap `Areas/HealthServices/MasterData/Models/MstInpatientSetting.cs`; configuration `Repositories/Configurations/HealthServices/MasterData/MstInpatientSettingConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | baru | PK | — | — | Tidak | Sudah ada |
| `Code` | `varchar(50)` | Ya | `DEFAULT` | **Unique** | — | — | Tidak | Sudah ada |
| `Name` | `varchar(150)` | Ya | — | — | — | — | Tidak | Sudah ada |
| `BedReservationMinutes` | `integer` | Ya | `120` | — | — | — | Tidak | Sudah ada |
| `DraftEpisodeExpiryHours` | `integer` | Ya | `24` | — | — | — | Tidak | Sudah ada |
| `InitialAssessmentTargetHours` | `integer` | Ya | `24` | — | — | — | Tidak | Sudah ada |
| `ProgressNoteVerificationTargetHours` | `integer` | Ya | `24` | — | — | — | Tidak | Sudah ada |
| `PendingClosureThresholdHours` | `integer` | Ya | `4` | — | — | — | Tidak | Sudah ada |
| `DepositFollowUpIntervalDays` | `integer` | Ya | `3` | — | — | — | Tidak | Sudah ada; ambang daftar pantau, **bukan** batas jatuh tempo surat (`RWI-DEC-260`) |
| `PendingSurgicalHandoverAlertMinutes` | `integer` | Ya | `60` | — | — | — | Tidak | Sudah ada (Finishing) |
| `PendingAdmissionReferralAlertMinutes` | `integer` | Ya | `30` | — | — | — | Tidak | Sudah ada (Finishing) |
| `EpisodeNumberPrefix` | `varchar(20)` | Ya | `RI` | — | — | — | Tidak | Sudah ada |
| `IsDefault` | `boolean` | Ya | `true` | Index (`IsActive`, `IsDefault`) | — | — | Tidak | Sudah ada |
| `IsActive` | `boolean` | Ya | `true` | Bagian index | — | — | Tidak | Sudah ada |
| `Notes` | `varchar(1000)` | Tidak | `null` | — | — | — | Tidak | Sudah ada |
| `GeneralConsentFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru.** Kode Formulir General Consent V1 |
| `NewPatientHandoverFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru** |
| `PrivacyRequestFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru** |
| `BeliefValuesFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru** |
| `CostDifferenceFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru** |
| `DepositSettlementFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru** |
| `CostEstimateFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru**; V1 tidak punya kode |
| `InpatientBaseDataFormCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | **Baru**; V1 tidak punya kode |
| `DocumentSigningCity` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | **Baru.** Kota penandatanganan bawaan dokumen |
| `InfantWristbandMaxAgeYears` | `integer` | Ya | **`5`** | — | — | — | Tidak | **Baru.** 0–16; umur tertinggi yang mendapat Gelang Bayi (`RWI-DEC-243`) |
| `PatientLabelHospitalCode` | `varchar(30)` | Tidak | `null` | — | — | — | Tidak | **Baru.** Kode singkat rumah sakit pada label; kosong = `MstHospitalSite.SiteCode` (G-36) |

Kode formulir yang kosong dicetak tanpa kode, tidak diganti nilai bawaan dari program (`RWI-DEC-247`).

### 20.17 Tabel `Sudah ada` yang dibaca — kolom kunci saja

Dibaca **lewat service pemilik** (`02-backend-architecture.md` 13.6), bukan query Rawat Inap ke tabelnya, kecuali tabel milik `InPatientManagement` sendiri.

| Tabel | File model | Kolom yang dipakai | Lewat |
|---|---|---|---|
| `InpEpisode` | `InPatientManagement/Models/InpEpisode.cs` | `Id`, `EpisodeNumber`, `EncounterId`, `PatientId`, `PatientClassId`, `EpisodeStatus`, `AdmittedAt`, `RequiresIsolation` | Langsung (modul sendiri) |
| `InpBedPlacement` | `InPatientManagement/Models/InpBedPlacement.cs` | `EpisodeId`, `ServiceUnitId`, `RoomId`, `BedId`, `PatientClassId`, `StartDateTime`, `EndDateTime`, `IsActive`, `IsSuperseded`, `SupersededByCorrectionId` | `InpPatientLocationQuery` |
| `InpStatusHistory` | `InPatientManagement/Models/InpStatusHistory.cs` | `ToStatus`, `ChangedByUserId`, `ChangedAt` — petugas yang mengonfirmasi admisi untuk IPD | Langsung |
| `InpDoctorAssignment`, `InpNurseAssignment` | `InPatientManagement/Models/` | DPJP dan perawat penanggung jawab aktif | Langsung |
| `MstRoom`, `MstBed` | `MasterData/Models/` | `IsIsolationRoom`, `IsIntensiveCare`, `IsIsolationBed`, `IsIntensiveCareBed` (`RWI-DEC-251`) | Navigasi penempatan, mengikuti pola census |
| `MstPatient` | `PatientManagement/MasterData/Models/MstPatient.cs` | `MedicalRecordNumber`, `FullName`, `NickName`, `BirthDate`, `Gender`, `Religion`, `MaritalStatus`, `IdentityType`, `IdentityNumber`, `PhoneNumber`, `Email`, `Address`, wilayah, `IsNewborn`, `MotherPatientId` | `PatientProfileQueryService` (`RWI-OQ-126`) |
| `MstPatientRelationship`, `MstPatientEmergencyContact` | `PatientManagement/MasterData/Models/` | Jenis atau teks hubungan, nama, alamat, telepon, `IsPrimary`, `IsResponsiblePerson`, `IsLegalGuardian`, `IsActive` | Sama |
| `RegPatientEncounterGuarantor` | `RegistrationManagement/Models/RegPatientEncounterGuarantor.cs` | `PaymentType`, `CardNumberSnapshot`, `MemberNumberSnapshot`, `PolicyNumberSnapshot`, `PaymentSourceNameSnapshot` | `EncounterInsuranceService` |
| `RegPatientEncounter` | `RegistrationManagement/Models/RegPatientEncounter.cs` | `ReferralDoctorId` | `EncounterReferralQueryService` (`RWI-OQ-128`) |
| `CliDoctorCertificate` | `ClinicalManagement/Models/CliDoctorCertificate.cs` | `CertificateType = InpatientReferral`, `CertificateStatus = Issued`, `EncounterId`, `DoctorId`, `IssuedDate`, `ReferralDiagnosis`, `ReferralReason` | `DoctorCertificateService` |
| `OprCase` | `OperatingRoomManagement/Models/OprCase.cs` | `EncounterId`, `Status` | `OperatingRoomCaseService` |
| `MstHospitalSite` | `Corporate/HumanResource/MasterData/Organization/Models/MstHospitalSite.cs` | `IsMainSite`, `IsActive`, `SiteCode`, `SiteName`, `Address`, wilayah, `PhoneNumber`, `Email`, `TimeZoneId` | `HospitalSiteProfileQueryService` (`RWI-OQ-127`) |
| `MstTariff` | `MasterData/Models/MstTariff.cs` | Tarif kamar per unit dan kelas | `BillingCalculationService` (`RWI-OQ-129`) |

### 20.18 Skema DDL revision `0.7`

> **Peringatan.** Dokumentasi bentuk tabel, **bukan** skrip yang dijalankan. Skema sungguhan lahir dari EF Core migration `E9`, `E10`, dan `E11` (`02-backend-architecture.md` 13.12). Kolom warisan `IdentityModel` tidak ditulis ulang.

```sql
-- E9 — MasterData
ALTER TABLE public."MstInpatientClearanceItem" ADD COLUMN "ChecklistType" integer NOT NULL DEFAULT 1;
ALTER TABLE public."MstInpatientClearanceItem" ADD COLUMN "ParentItemId" uuid NULL;
ALTER TABLE public."MstInpatientClearanceItem" ADD COLUMN "HandoverSuggestionSource" integer NOT NULL DEFAULT 0;
ALTER TABLE public."MstInpatientClearanceItem" ADD CONSTRAINT "FK_MstInpatientClearanceItem_MstInpatientClearanceItem_ParentItemId"
    FOREIGN KEY ("ParentItemId") REFERENCES public."MstInpatientClearanceItem" ("Id") ON DELETE RESTRICT;
ALTER TABLE public."MstInpatientClearanceItem" ADD CONSTRAINT "CK_MstInpatientClearanceItem_Type"
    CHECK ("ChecklistType" <> 1 OR ("ParentItemId" IS NULL AND "HandoverSuggestionSource" = 0));
CREATE INDEX "IX_MstInpatientClearanceItem_ChecklistType_IsActive" ON public."MstInpatientClearanceItem" ("ChecklistType", "IsActive");
CREATE INDEX "IX_MstInpatientClearanceItem_ParentItemId" ON public."MstInpatientClearanceItem" ("ParentItemId");

ALTER TABLE public."MstInpatientSetting" ADD COLUMN "GeneralConsentFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "NewPatientHandoverFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "PrivacyRequestFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "BeliefValuesFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "CostDifferenceFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "DepositSettlementFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "CostEstimateFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "InpatientBaseDataFormCode" varchar(50) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "DocumentSigningCity" varchar(100) NULL;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "InfantWristbandMaxAgeYears" integer NOT NULL DEFAULT 5;
ALTER TABLE public."MstInpatientSetting" ADD COLUMN "PatientLabelHospitalCode" varchar(30) NULL;

-- E10 — InPatientManagement (sesudah E9)
CREATE TABLE public."InpAdmissionDocument" (
    "Id"                    uuid         NOT NULL,
    "EpisodeId"             uuid         NOT NULL,
    "PatientId"             uuid         NOT NULL,
    "DocumentType"          integer      NOT NULL,
    "Status"                integer      NOT NULL DEFAULT 1,
    "VersionNo"             integer      NOT NULL DEFAULT 1,
    "PreviousVersionId"     uuid         NULL,
    "CorrectionReason"      varchar(500) NULL,   -- SENSITIF
    "SigningCity"           varchar(100) NULL,
    "StatementDate"         date         NULL,
    "Note"                  varchar(1000) NULL,  -- SENSITIF
    "SnapshotFormatVersion" integer      NULL,
    "SnapshotJson"          jsonb        NULL,   -- SENSITIF
    "LockedAt"              timestamp with time zone NULL,
    "LockedByUserId"        uuid         NULL,
    "CompletedAt"           timestamp with time zone NULL,
    "SupersededAt"          timestamp with time zone NULL,
    "CancelledAt"           timestamp with time zone NULL,
    "CancelledByUserId"     uuid         NULL,
    "CancelledReason"       varchar(500) NULL,   -- SENSITIF
    "IdempotencyKey"        varchar(80)  NULL,
    "RowVersion"            uuid         NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang
    CONSTRAINT "PK_InpAdmissionDocument" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionDocument_InpEpisode_EpisodeId" FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionDocument_MstPatient_PatientId" FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionDocument_InpAdmissionDocument_PreviousVersionId" FOREIGN KEY ("PreviousVersionId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionDocument_State" CHECK (
        ("Status" <> 2 OR ("LockedAt" IS NOT NULL AND "SnapshotJson" IS NOT NULL)) AND
        ("Status" <> 3 OR ("CompletedAt" IS NOT NULL AND "SnapshotJson" IS NOT NULL)) AND
        ("Status" <> 4 OR "SupersededAt" IS NOT NULL) AND
        ("Status" <> 5 OR ("CancelledAt" IS NOT NULL AND "CancelledReason" IS NOT NULL)) AND
        ("VersionNo" = 1 OR ("PreviousVersionId" IS NOT NULL AND "CorrectionReason" IS NOT NULL)))
);
CREATE UNIQUE INDEX "UX_InpAdmissionDocument_Episode_Type_Active" ON public."InpAdmissionDocument" ("EpisodeId", "DocumentType")
    WHERE "Status" IN (1, 2, 3) AND NOT "IsDelete";
CREATE UNIQUE INDEX "UX_InpAdmissionDocument_PreviousVersionId" ON public."InpAdmissionDocument" ("PreviousVersionId")
    WHERE "PreviousVersionId" IS NOT NULL;
CREATE UNIQUE INDEX "UX_InpAdmissionDocument_IdempotencyKey" ON public."InpAdmissionDocument" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND NOT "IsDelete";
CREATE INDEX "IX_InpAdmissionDocument_EpisodeId_DocumentType_CreateDateTime" ON public."InpAdmissionDocument" ("EpisodeId", "DocumentType", "CreateDateTime");
CREATE INDEX "IX_InpAdmissionDocument_PatientId_DocumentType_Status" ON public."InpAdmissionDocument" ("PatientId", "DocumentType", "Status");

CREATE TABLE public."InpAdmissionDocumentSignature" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "Slot" integer NOT NULL, "Method" integer NOT NULL,
    "SignerName"             varchar(200) NOT NULL,  -- SENSITIF
    "SignerPositionName"     varchar(150) NULL,
    "SignerRelationship"     integer NULL,
    "SignerRelationshipText" varchar(100) NULL,
    "SignedAt" timestamp with time zone NOT NULL, "SignedByUserId" uuid NULL, "VerifiedByUserId" uuid NULL,
    "RecordedAt" timestamp with time zone NOT NULL, "IdempotencyKey" varchar(80) NULL,
    CONSTRAINT "PK_InpAdmissionDocumentSignature" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionDocumentSignature_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionDocumentSignature_Method" CHECK (
        ("Method" <> 1 OR ("Slot" = 1 AND "VerifiedByUserId" IS NOT NULL AND "SignedByUserId" IS NULL)) AND
        ("Method" <> 2 OR ("Slot" <> 1 AND "SignedByUserId" IS NOT NULL)))
);
CREATE UNIQUE INDEX "UX_InpAdmissionDocumentSignature_Document_Slot" ON public."InpAdmissionDocumentSignature" ("DocumentId", "Slot") WHERE NOT "IsDelete";
CREATE UNIQUE INDEX "UX_InpAdmissionDocumentSignature_Document_Signer" ON public."InpAdmissionDocumentSignature" ("DocumentId", "SignedByUserId")
    WHERE "SignedByUserId" IS NOT NULL AND NOT "IsDelete";
CREATE UNIQUE INDEX "UX_InpAdmissionDocumentSignature_IdempotencyKey" ON public."InpAdmissionDocumentSignature" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND NOT "IsDelete";

CREATE TABLE public."InpAdmissionDocumentParty" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "SourceType" integer NOT NULL DEFAULT 4, "SourceRecordId" uuid NULL,
    "FullName" varchar(150) NOT NULL,       -- SENSITIF
    "Relationship" integer NULL, "RelationshipText" varchar(100) NULL,
    "Address" varchar(500) NULL,            -- SENSITIF
    "BirthDate" date NULL,                  -- SENSITIF
    "Gender" integer NULL,
    "Occupation" varchar(100) NULL,         -- SENSITIF
    "IdentityType" integer NULL,
    "IdentityNumber" varchar(50) NULL,      -- SENSITIF
    "MobilePhone" varchar(13) NULL,         -- SENSITIF
    "OfficePhone" varchar(20) NULL,         -- SENSITIF
    CONSTRAINT "PK_InpAdmissionDocumentParty" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionDocumentParty_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_InpAdmissionDocumentParty_DocumentId" ON public."InpAdmissionDocumentParty" ("DocumentId");

CREATE TABLE public."InpAdmissionHandoverItem" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "ClearanceItemId" uuid NOT NULL, "LineNo" integer NOT NULL,
    "ItemNumberSnapshot" integer NULL, "ParentItemNumberSnapshot" integer NULL,
    "ItemCodeSnapshot" varchar(50) NOT NULL, "ItemNameSnapshot" varchar(200) NOT NULL,
    "Choice" integer NULL, "Note" varchar(500) NULL,
    CONSTRAINT "PK_InpAdmissionHandoverItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionHandoverItem_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionHandoverItem_MstInpatientClearanceItem_ClearanceItemId" FOREIGN KEY ("ClearanceItemId") REFERENCES public."MstInpatientClearanceItem" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_InpAdmissionHandoverItem_DocumentId_ClearanceItemId" ON public."InpAdmissionHandoverItem" ("DocumentId", "ClearanceItemId");
CREATE UNIQUE INDEX "IX_InpAdmissionHandoverItem_DocumentId_LineNo" ON public."InpAdmissionHandoverItem" ("DocumentId", "LineNo");

CREATE TABLE public."InpAdmissionPrivacyRequest" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "IsTransportPrivacyRequested" boolean NOT NULL DEFAULT false,
    CONSTRAINT "PK_InpAdmissionPrivacyRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionPrivacyRequest_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_InpAdmissionPrivacyRequest_DocumentId" ON public."InpAdmissionPrivacyRequest" ("DocumentId");

CREATE TABLE public."InpAdmissionPrivacyEntry" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "EntryType" integer NOT NULL, "LineNo" integer NOT NULL,
    "Text" varchar(200) NOT NULL,           -- SENSITIF
    CONSTRAINT "PK_InpAdmissionPrivacyEntry" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionPrivacyEntry_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionPrivacyEntry_LineNo" CHECK ("LineNo" BETWEEN 1 AND 3)
);
CREATE UNIQUE INDEX "IX_InpAdmissionPrivacyEntry_DocumentId_EntryType_LineNo" ON public."InpAdmissionPrivacyEntry" ("DocumentId", "EntryType", "LineNo");

CREATE TABLE public."InpAdmissionBeliefItem" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "ItemNo" integer NOT NULL,
    "Text" varchar(500) NOT NULL,           -- SENSITIF
    CONSTRAINT "PK_InpAdmissionBeliefItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionBeliefItem_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionBeliefItem_ItemNo" CHECK ("ItemNo" BETWEEN 1 AND 5)
);
CREATE UNIQUE INDEX "IX_InpAdmissionBeliefItem_DocumentId_ItemNo" ON public."InpAdmissionBeliefItem" ("DocumentId", "ItemNo");

CREATE TABLE public."InpAdmissionCostDifferenceStatement" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "Subject" integer NOT NULL, "SubjectOtherText" varchar(100) NULL,
    CONSTRAINT "PK_InpAdmissionCostDifferenceStatement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionCostDifferenceStatement_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionCostDifferenceStatement_Other" CHECK ("Subject" <> 5 OR "SubjectOtherText" IS NOT NULL)
);
CREATE UNIQUE INDEX "IX_InpAdmissionCostDifferenceStatement_DocumentId" ON public."InpAdmissionCostDifferenceStatement" ("DocumentId");

CREATE TABLE public."InpAdmissionDepositStatement" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL,
    "DueAt" timestamp with time zone NULL, "PolicyFollowUpIntervalDays" integer NULL,
    "MinimumPolicyAmount" numeric(18,2) NULL,  -- SENSITIF
    "ReceivedAmount" numeric(18,2) NULL,       -- SENSITIF
    "ShortfallAmount" numeric(18,2) NULL,      -- SENSITIF
    "AmountsReadAt" timestamp with time zone NULL,
    CONSTRAINT "PK_InpAdmissionDepositStatement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionDepositStatement_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_InpAdmissionDepositStatement_DocumentId" ON public."InpAdmissionDepositStatement" ("DocumentId");

CREATE TABLE public."InpAdmissionPrintLog" (
    "Id" uuid NOT NULL, "EpisodeId" uuid NOT NULL, "DocumentId" uuid NULL, "PrintKind" integer NOT NULL,
    "DocumentStatusAtPrint" integer NULL, "Copies" integer NOT NULL DEFAULT 1, "IsReprint" boolean NOT NULL DEFAULT false,
    "ReprintReason" integer NULL, "ReprintNote" varchar(200) NULL,
    "PrintedByUserId" uuid NOT NULL, "PrintedAt" timestamp with time zone NOT NULL, "IdempotencyKey" varchar(80) NULL,
    CONSTRAINT "PK_InpAdmissionPrintLog" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionPrintLog_InpEpisode_EpisodeId" FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionPrintLog_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionPrintLog_Reprint" CHECK (
        ("IsReprint" = false OR "ReprintReason" IS NOT NULL) AND
        ("ReprintReason" IS DISTINCT FROM 4 OR "ReprintNote" IS NOT NULL) AND
        ("PrintKind" <> 5 OR "DocumentId" IS NOT NULL) AND
        ("Copies" BETWEEN 1 AND 10))
);
CREATE INDEX "IX_InpAdmissionPrintLog_EpisodeId_PrintKind_PrintedAt" ON public."InpAdmissionPrintLog" ("EpisodeId", "PrintKind", "PrintedAt");
CREATE INDEX "IX_InpAdmissionPrintLog_DocumentId_PrintedAt" ON public."InpAdmissionPrintLog" ("DocumentId", "PrintedAt");
CREATE UNIQUE INDEX "UX_InpAdmissionPrintLog_IdempotencyKey" ON public."InpAdmissionPrintLog" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND NOT "IsDelete";

-- E11 — InPatientManagement, di luar gelombang (EPIC-RWA-09)
CREATE TABLE public."InpAdmissionCostEstimate" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "OprCaseId" uuid NULL,
    "PlannedProcedureText" varchar(300) NULL,  -- SENSITIF
    "PlannedScheduleAt" timestamp with time zone NULL, "DoctorId" uuid NULL, "PatientClassId" uuid NOT NULL,
    "EstimatedLengthOfStayDays" integer NOT NULL DEFAULT 1, "PricesReadAt" timestamp with time zone NULL, "NotesSnapshot" text NULL,
    CONSTRAINT "PK_InpAdmissionCostEstimate" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionCostEstimate_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimate_OprCase_OprCaseId" FOREIGN KEY ("OprCaseId") REFERENCES public."OprCase" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimate_MstDoctor_DoctorId" FOREIGN KEY ("DoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimate_MstPatientClass_PatientClassId" FOREIGN KEY ("PatientClassId") REFERENCES public."MstPatientClass" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionCostEstimate_Los" CHECK ("EstimatedLengthOfStayDays" BETWEEN 1 AND 365)
);
CREATE UNIQUE INDEX "IX_InpAdmissionCostEstimate_DocumentId" ON public."InpAdmissionCostEstimate" ("DocumentId");
CREATE INDEX "IX_InpAdmissionCostEstimate_OprCaseId" ON public."InpAdmissionCostEstimate" ("OprCaseId");

CREATE TABLE public."InpAdmissionCostEstimateLine" (
    "Id" uuid NOT NULL, "DocumentId" uuid NOT NULL, "LineNo" integer NOT NULL, "LineType" integer NOT NULL,
    "Description" varchar(300) NOT NULL, "ProcedureId" uuid NULL, "TariffId" uuid NULL, "DoctorId" uuid NULL,
    "Quantity" numeric(10,2) NOT NULL DEFAULT 1,
    "UnitPrice" numeric(18,2) NULL,            -- SENSITIF
    "PriceSource" integer NOT NULL, "ManualReason" varchar(300) NULL,
    CONSTRAINT "PK_InpAdmissionCostEstimateLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionCostEstimateLine_InpAdmissionDocument_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES public."InpAdmissionDocument" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimateLine_MstProcedure_ProcedureId" FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimateLine_MstTariff_TariffId" FOREIGN KEY ("TariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_InpAdmissionCostEstimateLine_MstDoctor_DoctorId" FOREIGN KEY ("DoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "CK_InpAdmissionCostEstimateLine_Price" CHECK (
        ("PriceSource" NOT IN (1, 2) OR "UnitPrice" IS NOT NULL) AND ("PriceSource" <> 2 OR "ManualReason" IS NOT NULL) AND "Quantity" > 0)
);
CREATE UNIQUE INDEX "IX_InpAdmissionCostEstimateLine_DocumentId_LineNo" ON public."InpAdmissionCostEstimateLine" ("DocumentId", "LineNo");

CREATE TABLE public."InpAdmissionProcedurePlanMark" (
    "Id" uuid NOT NULL, "EpisodeId" uuid NOT NULL, "MarkedAt" timestamp with time zone NOT NULL, "MarkedByUserId" uuid NOT NULL,
    "Note" varchar(300) NULL,                  -- SENSITIF
    "UnmarkedAt" timestamp with time zone NULL, "UnmarkedByUserId" uuid NULL,
    CONSTRAINT "PK_InpAdmissionProcedurePlanMark" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_InpAdmissionProcedurePlanMark_InpEpisode_EpisodeId" FOREIGN KEY ("EpisodeId") REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "UX_InpAdmissionProcedurePlanMark_Episode_Active" ON public."InpAdmissionProcedurePlanMark" ("EpisodeId")
    WHERE "UnmarkedAt" IS NULL AND NOT "IsDelete";
```

Nama tabel FK modul lain (`MstPatient`, `MstDoctor`, `MstPatientClass`, `MstProcedure`, `MstTariff`, `OprCase`) mengikuti nama `[Table]` di source saat migration dibuat; DDL di atas hanya menunjukkan arah relasi dan perilaku hapusnya.
