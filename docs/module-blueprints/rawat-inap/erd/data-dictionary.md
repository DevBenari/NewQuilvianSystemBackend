# Kamus Data — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.3` |
| Status | `draft` |
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

## 2. `InpDoctorAssignment` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EpisodeId` | `Guid` | Ya | — | Index | FK ke `InpEpisode` | `Restrict` | Tidak | Episode pemilik |
| `DoctorId` | `Guid` | Ya | — | Index | FK ke `MstDoctor` | `Restrict` | Tidak | Dokter yang ditunjuk sebagai DPJP |
| `SequenceNumber` | `int` | Ya | — | Unique bersama `EpisodeId` | — | — | Tidak | Urutan penugasan, dimulai dari 1 |
| `StartDateTime` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Mulai berlakunya tanggung jawab |
| `EndDateTime` | `DateTime?` | Tidak | — | Index parsial | — | — | Tidak | Kosong berarti masih aktif. Unique atas `EpisodeId` bila kosong, menjaga `INV-INP-03` |
| `AssignedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menugaskan atau mengalihkan |
| `HandoverReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib diisi bila baris ini lahir dari pengalihan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

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
| `StartDateTime` | `DateTime` | Ya | `UtcNow` | Index | — | — | Tidak | Mulai ditempati |
| `EndDateTime` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Kosong berarti masih ditempati |
| `EndReason` | `InpBedPlacementEndReason?` | Tidak | — | — | — | — | Tidak | Kenapa penempatan berakhir: perpindahan, penutupan episode, pembatalan admisi, atau **kepergian fisik pasien**. Disimpan sebagai `int` |
| `TransferReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan medis perpindahan. Wajib bila baris ini lahir dari perpindahan |
| `PlacedByUserId` | `Guid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menempatkan |
| `EndedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang mengakhiri penempatan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

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

-- Menjaga INV-INP-03: satu episode hanya boleh punya satu DPJP aktif
CREATE UNIQUE INDEX "IX_InpDoctorAssignment_EpisodeId_Active"
    ON public."InpDoctorAssignment" ("EpisodeId") WHERE "EndDateTime" IS NULL;


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

Tidak ada kolom yang dihapus dan tidak ada tipe yang berubah pada revision ini.
