# Human Resource — Kamus Data

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `data/data-dictionary.md` |
| `contract_version` | `v2` — angka set kontrak disimpan di `blueprint-manifest.md` field `contract_versions` |
| `last_changed_in` | `v2` |
| Status | `draft` — **belum** `approved`. Approval adalah tindakan manusia, bukan keluaran skill |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik basis data untuk keputusan yang merusak data |
| `approved_by` / `approved_at` | **Belum ada** |
| `input_revision` | `02-backend-architecture.md` revision `1`; `00-interview-decisions.md` revision `12` |
| `input_hash` — decision log | `0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` (branch kerja `AndryZain`) |
| Backend baseline canonical | `origin/QuilvianIntegrationBackend` (`HRD-DEC-021`), diverifikasi pada `16b8b71` |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` — seluruh tabel dalam cakupan bersifat administratif ketenagakerjaan |
| Dampak kompatibilitas | **Tidak ada kolom yang dihapus, diganti nama, atau berubah tipe.** Seluruh perubahan bersifat penambahan kolom yang boleh kosong atau berisi nilai bawaan |

---

## 0. Cara membaca dokumen ini

Dokumen ini adalah **kontrak kolom** modul HR. Ia menjawab satu pertanyaan: kolom apa saja yang
ada di setiap tabel yang disentuh rancangan ini, dan kolom mana yang boleh kosong.

Dokumen ini **bukan** skrip basis data. Tidak ada satu baris pun di sini yang boleh dijalankan.

### 0.1 Kolom audit yang tidak diulang

Seluruh tabel HR mewarisi `IdentityModel` (`Models/IdentityModel.cs`), sehingga **setiap** tabel
di bawah memiliki sepuluh kolom audit berikut. Kolom-kolom ini **tidak diulang** pada tabel mana
pun di dokumen ini dan **tidak ditulis ulang** pada bagian DDL.

| Kolom | Tipe | Wajib | Bawaan | Keterangan |
| --- | --- | :---: | --- | --- |
| `CreateDateTime` | `timestamp with time zone` | Ya | `CURRENT_TIMESTAMP` | Waktu baris dibuat |
| `CreateBy` | `uuid` | Ya | `Guid.Empty` | Pengguna yang membuat |
| `UpdateDateTime` | `timestamp with time zone` | Tidak | — | Waktu perubahan terakhir |
| `UpdateBy` | `uuid` | Ya | `Guid.Empty` | Pengguna yang mengubah terakhir |
| `DeleteDateTime` | `timestamp with time zone` | Tidak | — | Waktu penandaan hapus |
| `DeleteBy` | `uuid` | Ya | `Guid.Empty` | Pengguna yang menandai hapus |
| `CancelDateTime` | `timestamp with time zone` | Tidak | — | Waktu pembatalan |
| `CancelBy` | `uuid` | Ya | `Guid.Empty` | Pengguna yang membatalkan |
| `IsCancel` | `boolean` | Ya | `false` | Penanda batal |
| `IsDelete` | `boolean` | Ya | `false` | Penanda hapus |

**Penghapusan di modul ini bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.**
Akibat langsungnya: hampir seluruh index unique memakai filter `"IsDelete" = false`, supaya nomor
dokumen yang sudah ditandai hapus tidak menghalangi penomoran baru.

### 0.2 Kedalaman mengikuti status tabel

| Status tabel | Yang ditulis di sini |
| --- | --- |
| `Baru` | Seluruh kolom |
| `Diperbarui` | Seluruh kolom, termasuk kolom yang sudah ada dan tidak berubah |
| `Sudah ada` | Kolom kunci saja — PK, FK, kolom status, dan kolom yang dipakai aturan bisnis modul ini — ditambah rujukan ke file model sebagai sumber lengkap |

**Tidak ada tabel berstatus `Baru` pada rancangan ini.** `HRD-DEC-031` menambah kolom pada empat entity penempatan dan remunerasi — lihat bagian 2.7 — **tanpa** membuat tabel baru. Seluruh kemampuan target dipenuhi dengan
`EXTEND` terhadap tabel yang sudah ada. Ini bukan kebetulan: `02-backend-architecture.md` bagian
7.4 mencatatnya sebagai keputusan, dan bagian 9 mencatat kemampuan yang sengaja **tidak**
mendapat tabel sendiri.

### 0.3 Penanda kolom rencana

Kolom yang **belum ada di source hari ini** dan hanya hidup sebagai rancangan ditandai
**`RENCANA`** pada kolom Keterangan. Kolom bertanda itu **MUST NOT** dianggap sudah dapat
dipakai, dan migration-nya **MUST NOT** dibuat sebelum task-nya diberi wewenang terpisah.

### 0.4 Kolom Sensitif

Kolom bertanda **Ya** pada kolom Sensitif:

- **MUST NOT** masuk ke custom logger dalam bentuk apa pun, termasuk sebagai bagian payload;
- **MUST NOT** dipakai sebagai contoh berisi data asli di dokumentasi mana pun;
- **SHOULD** ditinjau kebutuhan maskingnya pada response DTO sebelum dipakai di layar yang
  jangkauan pembacanya lebih luas daripada pemilik datanya.

Penandaan ini menjadi masukan langsung bagi
[`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md).

---

## 1. Tabel status dan kepemilikan

Inilah yang menahan modul HR membuat salinan data milik modul lain. Setiap tabel yang disentuh
rancangan ini punya barisnya di sini.

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `MstWorkforceProfile` | `Sudah ada` | Human Resource | Akar identitas pegawai. Dirujuk hampir seluruh tabel transaksi HR |
| `WfpSalaryAssignment` | **`Diperbarui`** | Human Resource | `HRD-DEC-031`: tambah kolom persetujuan dan wiring workflow. Kolom nominal bersifat sensitif |
| `WfpOrganizationAssignment` | **`Diperbarui`** | Human Resource | `HRD-DEC-031`: tambah seluruh kolom persetujuan dan wiring workflow |
| `WfpPositionAssignment` | **`Diperbarui`** | Human Resource | `HRD-DEC-031`: sama |
| `WfpManagerAssignment` | **`Diperbarui`** | Human Resource | `HRD-DEC-031`: sama. `CanApproveRequests` yang sudah ada **tidak** berubah artinya |
| `TrxEmployeeProfileChangeRequest` | `Sudah ada` | Human Resource | Permohonan ubah data diri |
| `HrdAttendanceRawLog` | `Sudah ada` | Human Resource | Rekaman mentah dari mesin absensi |
| `HrdAttendanceDaily` | `Sudah ada` | Human Resource | Hasil olahan kehadiran per hari |
| `HrdAttendancePeriod` | `Sudah ada` | Human Resource | Periode kehadiran, tutup dan buka |
| `HrdAttendanceException` | **`Diperbarui`** | Human Resource | Tambah tiga kolom klasifikasi; kosakata `ExceptionType` bertambah satu nilai |
| `HrdAttendanceCorrectionRequest` | **`Diperbarui`** | Human Resource | Tambah empat kolom pengajuan atas nama pegawai (`HRD-DEC-028`) |
| `WfpLeaveBalance` | `Sudah ada` | Human Resource | Saldo cuti per jenis |
| `WfpLeaveRequest` | `Sudah ada` | Human Resource | Permohonan cuti |
| `TrxLeaveBalanceTransaction` | `Sudah ada` | Human Resource | Buku besar saldo cuti |
| `TrxLeaveExecution` | **`Diperbarui`** | Human Resource | Tambah dua kolom; dua kolom pembalikan **ternyata sudah ada** — lihat bagian 2.3 |
| `TrxLeaveRecall` | **`Diperbarui`** | Human Resource | Tambah tiga kolom penggantian pengakuan pegawai |
| `WfpOvertimeRequest` | `Sudah ada` | Human Resource | Permohonan lembur |
| `TrxOvertimeRealization` | `Sudah ada` | Human Resource | Realisasi lembur |
| `TrxShiftAssignment` | **`Diperbarui`** | Human Resource | **Tidak ada kolom baru.** Yang bertambah hanya index; lihat bagian 2.6 |
| `TrxRosterPeriod` | `Sudah ada` | Human Resource | Periode roster. Skema lengkap, API belum ada (`HRD-DEC-026`) |
| `TrxRosterAssignment` | `Sudah ada` | Human Resource | Sama; kemungkinan tambah index |
| `TrxRosterPublication` | `Sudah ada` | Human Resource | Sama |
| `TrxRosterApproval` | `Sudah ada` | Human Resource | Sama |
| `TrxShiftReplacement` | `Sudah ada` | Human Resource | Sama |
| `TrxEmergencyStaffingRequest` | `Sudah ada` | Human Resource | Sama |
| `TrxOnCallAssignment` | `Sudah ada` | Human Resource | Sama |
| `TrxWorkflowInstance` | `Sudah ada` | Human Resource | Mesin persetujuan bersama |
| `TrxWorkflowApproverAssignment` | **`Diperbarui`** | Human Resource | Tambah empat kolom pengingat dan eskalasi (`HRD-DEC-030`) |
| `WfpPerformanceReview` | `Sudah ada` | Human Resource | Penilaian kinerja |
| `WfpDisciplinaryAction` | `Sudah ada` | Human Resource | Tindakan kedisiplinan |
| `TrxResignationRequest` | `Sudah ada` | Human Resource | Pengunduran diri |
| `TrxPayrollRun` | `Sudah ada` | Human Resource | Proses payroll sisi HR sampai `execute` (`HRD-DEC-009`) |
| `ApplicationUser` | `Sudah ada` | **Administrator / Identity** | Direferensikan lewat kolom `*UserId`. HR **MUST NOT** membuat tabel akun sendiri |
| Tabel pembayaran, jurnal, dan pajak | — | **Finance** | **Tidak dirujuk dan tidak dibuat.** `HRD-DEC-009` menghentikan tanggung jawab HR setelah `execute` |
| Tabel jadwal praktik dokter | — | **Health Services** | **Tidak dirujuk sebagai sumber kebenaran.** `HRD-DEC-006` |
| Tabel penyimpanan berkas fisik | — | **Shared platform** | HR hanya menyimpan metadata dan path. Kontrak `[OPEN]` — `HRD-DEP-006` |
| `CredentialingManagement` (18 tabel) | `Sudah ada` | Human Resource + Komite Medik | **Tidak disentuh dan tidak dirancang.** `S-C1` `BLOCKED` |
| `OccupationalHealthManagement` (10 tabel) | `Sudah ada` | K3RS | **Tidak disentuh dan tidak dirancang.** `S-C6` `BLOCKED` |
| Enam domain tanpa controller (68 tabel) | `Sudah ada` | Human Resource | **Tidak disentuh.** `S-D1` s.d. `S-D5` `BLOCKED`/`DEFERRED` oleh `HRD-Q-05` |

**Aturan yang mengikat tabel ini:** bila sebuah task kelak merasa perlu membuat tabel yang
menduplikasi baris mana pun di atas, task itu **berhenti** dan pertanyaannya dibawa ke pemilik
data yang tercatat. Ia tidak diselesaikan dengan membuat salinan.

---

## 2. Tabel berstatus `Diperbarui` — seluruh kolom

Enam tabel berikut adalah satu-satunya tabel yang skemanya berubah pada rancangan ini. Seluruh
kolomnya ditulis, termasuk kolom yang tidak berubah, supaya implementer tidak perlu membuka file
model untuk mengetahui bentuk lengkapnya.

### 2.1 `HrdAttendanceException`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."HrdAttendanceException"` |
| File model | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceException.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdAttendanceExceptionConfiguration.cs` |
| Alasan berubah | Pengecualian kehadiran perlu jejak siapa yang mengklasifikasikannya dan kapan (`HRD-DEC-022` s.d. `HRD-DEC-025`) |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AttendanceDailyId` | `uuid` | Ya | — | Unique bersama `ExceptionCode`, terfilter | FK ke `HrdAttendanceDaily` | `Restrict` | Tidak | Hari kehadiran yang menimbulkan pengecualian |
| `WorkforceProfileId` | `uuid` | Tidak | — | Index bersama `DetectedAt` | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai yang bersangkutan |
| `CorrectionRequestId` | `uuid` | Tidak | — | — | FK ke `HrdAttendanceCorrectionRequest` | `Restrict` | Tidak | Permohonan koreksi yang menutup pengecualian ini |
| `ExceptionCode` | `varchar(50)` | Ya | — | Unique bersama `AttendanceDailyId`, terfilter | — | — | Tidak | Kode pengecualian, unik per hari selama belum `Closed` |
| `ExceptionType` | `varchar(50)` | Ya | `"Unknown"` | **Index baru bersama `ExceptionStatus`** | — | — | Tidak | Kosakata: `Late`, `EarlyLeave`, `MissingCheckIn`, `MissingCheckOut`, `Absent`, `OutsideGeofence`, `DuplicatePunch`, `ScheduleMismatch`, dan **`OutOfScheduleWork`** yang ditambahkan rancangan ini |
| `Severity` | `varchar(20)` | Ya | `"Warning"` | Index bersama `ExceptionStatus`, `IsPayrollBlocking` | — | — | Tidak | `Info`, `Warning`, `High`, `Critical` |
| `ExceptionStatus` | `varchar(30)` | Ya | `"Open"` | Index | — | — | Tidak | `Open`, `UnderReview`, `Corrected`, `Waived`, `Rejected`, `Closed` |
| `DetectedAt` | `timestamp with time zone` | Ya | `CURRENT_TIMESTAMP` | Index bersama `WorkforceProfileId` | — | — | Tidak | Waktu sistem menemukan pengecualian |
| `ExpectedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu yang seharusnya menurut jadwal |
| `ActualAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu yang benar-benar terjadi |
| `DifferenceMinutes` | `integer` | Tidak | — | — | — | — | Tidak | Selisih menit antara seharusnya dan kenyataan |
| `IsAutoDetected` | `boolean` | Ya | `true` | — | — | — | Tidak | Ditemukan mesin, bukan diketik petugas |
| `IsPayrollBlocking` | `boolean` | Ya | `false` | Index bersama `ExceptionStatus`, `Severity` | — | — | Tidak | Menahan periode ditutup selama masih `Open` |
| `DetectionRule` | `varchar(100)` | Tidak | — | — | — | — | Tidak | Nama aturan pendeteksi |
| `Message` | `varchar(1000)` | Tidak | — | — | — | — | **Ya** | Keterangan pengecualian; dapat memuat keadaan pribadi pegawai |
| `ResolvedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Petugas yang menyelesaikan |
| `ResolvedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu penyelesaian |
| `ResolutionNote` | `varchar(1000)` | Tidak | — | — | — | — | **Ya** | Catatan penyelesaian; dapat memuat alasan pribadi |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Penanda aktif |
| `ClassificationDecision` | `varchar(40)` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Keputusan klasifikasi pengecualian oleh petugas HR |
| `ClassifiedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Siapa yang mengklasifikasikan |
| `ClassifiedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan diklasifikasikan |

**Index yang sudah ada dan tidak berubah**

| Index | Kolom | Unique | Filter |
| --- | --- | :---: | --- |
| Pengecualian unik per hari | `(AttendanceDailyId, ExceptionCode)` | Ya | `"IsDelete" = false AND "ExceptionStatus" <> 'Closed'` |
| Daftar prioritas | `(ExceptionStatus, Severity, IsPayrollBlocking)` | Tidak | — |
| Riwayat per pegawai | `(WorkforceProfileId, DetectedAt)` | Tidak | — |

**Index yang ditambahkan rancangan ini**

| Index | Kolom | Unique | Alasan |
| --- | --- | :---: | --- |
| Antrean klasifikasi | `(ExceptionType, ExceptionStatus)` | Tidak | Layar daftar pengecualian yang menunggu klasifikasi menyaring dengan dua kolom itu |

**Catatan penting tentang `OutOfScheduleWork`.** Nilai ini masuk ke kosakata `ExceptionType`,
yang bertipe `varchar(50)`, **bukan** enum basis data. Menambah nilai karena itu **tidak**
mengubah tipe kolom dan **tidak** memerlukan migration untuk kolomnya. Yang perlu diperbarui
adalah konstanta di source dan daftar nilai pada
[`../contracts/validation-matrix.md`](../contracts/validation-matrix.md).

### 2.2 `HrdAttendanceCorrectionRequest`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."HrdAttendanceCorrectionRequest"` |
| File model | `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionRequest.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdAttendanceCorrectionRequestConfiguration.cs` |
| Alasan berubah | `HRD-DEC-028` mengizinkan HR mengajukan koreksi **atas nama** pegawai. Tanpa kolom baru, jejak siapa yang benar-benar mengetik permohonan itu hilang |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RequestNumber` | `varchar(50)` | Ya | — | Unique, terfilter `"IsDelete" = false` | — | — | Tidak | Nomor permohonan yang dilihat pengguna |
| `WorkforceProfileId` | `uuid` | Ya | — | Index bersama `AttendanceDate` | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | **Pemilik data kehadiran** yang dikoreksi |
| `AttendanceDailyId` | `uuid` | Tidak | — | — | FK ke `HrdAttendanceDaily` | `Restrict` | Tidak | Hari kehadiran yang dikoreksi |
| `AttendanceId` | `uuid` | Tidak | — | — | FK ke `HrdAttendance` | `Restrict` | Tidak | Rekaman kehadiran tunggal |
| `RequestReasonId` | `uuid` | Tidak | — | — | FK ke `MstRequestReason` | `Restrict` | Tidak | Alasan baku dari master |
| `RejectionReasonId` | `uuid` | Tidak | — | — | FK ke `MstRejectionReason` | `Restrict` | Tidak | Alasan penolakan baku dari master |
| `WorkflowDefinitionId` | `uuid` | Tidak | — | Index bersama `RequestStatus` | FK ke `MstWorkflowDefinition` | `Restrict` | Tidak | Definisi alur persetujuan yang dipakai |
| `WorkflowInstanceId` | `uuid` | Tidak | — | Unique terfilter bukan-null | FK ke `TrxWorkflowInstance` | `Restrict` | Tidak | Satu permohonan paling banyak satu instance workflow |
| `RequestedByWorkforceProfileId` | `uuid` | Tidak | — | — | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Profil yang mengajukan, bisa berbeda dari pemilik data |
| `RequestedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun yang mengajukan |
| `AttendanceDate` | `date` | Ya | — | Index bersama `WorkforceProfileId` | — | — | Tidak | Tanggal kehadiran yang dikoreksi |
| `CorrectionType` | `varchar(50)` | Ya | `AttendanceTime` | — | — | — | Tidak | Jenis koreksi menurut `AttendanceValueConstants.CorrectionType` |
| `RequestStatus` | `varchar(30)` | Ya | `Draft` | Index bersama `SubmittedAt` | — | — | Tidak | Status permohonan menurut `AttendanceValueConstants.CorrectionRequestStatus` |
| `Reason` | `varchar(1500)` | Ya | — | — | — | — | **Ya** | Alasan pegawai; kerap memuat keadaan pribadi atau kesehatan |
| `EvidenceFilePath` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | Path berkas bukti pada penyimpanan bersama |
| `EvidenceFileName` | `varchar(255)` | Tidak | — | — | — | — | **Ya** | Nama berkas asli |
| `EvidenceContentType` | `varchar(100)` | Tidak | — | — | — | — | Tidak | Jenis MIME berkas |
| `OriginalSummaryJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Rekaman keadaan sebelum koreksi |
| `RequestedSummaryJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Keadaan yang diminta pemohon |
| `ApprovedSummaryJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Keadaan yang akhirnya disetujui |
| `SubmittedAt` | `timestamp with time zone` | Tidak | — | Index bersama `RequestStatus` | — | — | Tidak | Waktu diajukan |
| `ApprovedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu disetujui |
| `RejectedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu ditolak |
| `AppliedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu koreksi benar-benar diterapkan ke data kehadiran |
| `AppliedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menerapkan |
| `FinalNote` | `varchar(1000)` | Tidak | — | — | — | — | **Ya** | Catatan penutup |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Penanda aktif |
| `InitiatedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Akun yang benar-benar mengetik permohonan. Berbeda dari `RequestedByUserId` ketika HR mengajukan atas nama pegawai |
| `IsOnBehalf` | `boolean` | Ya | `false` | **Index baru bersama `RequestStatus`** | — | — | Tidak | **`RENCANA`.** Menandai permohonan yang dibuat HR atas nama pegawai |
| `OnBehalfReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | **`RENCANA`.** Mengapa HR yang mengajukan, bukan pegawainya |
| `OnBehalfNotifiedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan pegawai diberi tahu bahwa ada permohonan atas namanya |

**Index yang sudah ada dan tidak berubah**

| Index | Kolom | Unique | Filter |
| --- | --- | :---: | --- |
| Nomor permohonan | `(RequestNumber)` | Ya | `"IsDelete" = false` |
| Satu instance per permohonan | `(WorkflowInstanceId)` | Ya | `"WorkflowInstanceId" IS NOT NULL AND "IsDelete" = false` |
| Riwayat per pegawai | `(WorkforceProfileId, AttendanceDate)` | Tidak | — |
| Antrean per status | `(RequestStatus, SubmittedAt)` | Tidak | — |
| Antrean per definisi workflow | `(WorkflowDefinitionId, RequestStatus)` | Tidak | — |

**Index yang ditambahkan rancangan ini**

| Index | Kolom | Unique | Alasan |
| --- | --- | :---: | --- |
| Pengawasan pengajuan atas nama | `(IsOnBehalf, RequestStatus)` | Tidak | Layar pengawasan HR menyaring permohonan atas nama yang masih menunggu |

**Alasan `IsOnBehalf` diberi nilai bawaan, bukan dibiarkan kosong.** Kolom `boolean` yang boleh
kosong memaksa setiap pembacaan memikirkan tiga kemungkinan: benar, salah, dan tidak diketahui.
Baris lama yang sudah ada seluruhnya bukan pengajuan atas nama, sehingga `false` adalah jawaban
yang benar untuk semuanya. Karena itu migration-nya **tidak** perlu mengisi ulang baris lama.

### 2.3 `TrxLeaveExecution`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."TrxLeaveExecution"` |
| File model | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveExecution.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/LeaveManagement/TrxLeaveExecutionConfiguration.cs` |
| Alasan berubah | Pembalikan pelaksanaan cuti perlu alasan yang tercatat, dan pelaksanaan perlu tahu apakah periode payroll sudah dikunci saat ia berjalan |

> **Koreksi terhadap `02-backend-architecture.md` bagian 7.1.** Dokumen itu mendaftar empat
> kolom sebagai penambahan: `ReversalReason`, `ReversedByUserId`, `ReversedAt`, dan
> `PayrollLockCheckedAt`. Pembacaan source pada `e0ee42c` menunjukkan **`ReversedAt` dan
> `ReversedByUserId` sudah ada** di model maupun configuration hari ini. Yang benar-benar baru
> hanya **dua** kolom: `ReversalReason` dan `PayrollLockCheckedAt`. Kamus data ini memakai
> pembacaan source sebagai yang berlaku; `02-backend-architecture.md` bagian 7.1 disinkronkan
> mengikuti temuan ini.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ExecutionNumber` | `varchar(60)` | Ya | — | Unique, terfilter | — | — | Tidak | Nomor pelaksanaan |
| `LeaveRequestId` | `uuid` | Ya | — | **Unique**, terfilter | FK ke `WfpLeaveRequest` | `Restrict` | Tidak | Satu permohonan cuti paling banyak satu pelaksanaan |
| `WorkforceProfileId` | `uuid` | Ya | — | Index bersama tanggal | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai yang cuti |
| `LeaveTypeId` | `uuid` | Ya | — | — | FK ke `MstLeaveType` | `Restrict` | Tidak | Jenis cuti |
| `LeaveBalanceId` | `uuid` | Tidak | — | — | FK ke `WfpLeaveBalance` | `Restrict` | Tidak | Saldo yang dipotong |
| `StartDate` | `date` | Ya | — | Index bersama `ExecutionStatus` | — | — | Tidak | Tanggal mulai cuti |
| `EndDate` | `date` | Ya | — | Index bersama `ExecutionStatus` | — | — | Tidak | Tanggal selesai cuti |
| `RequestedDays` | `numeric(18,4)` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang diminta |
| `ExecutedDays` | `numeric(18,4)` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang benar-benar terlaksana |
| `ExecutionStatus` | `varchar(30)` | Ya | `"Scheduled"` | Index | — | — | Tidak | Status pelaksanaan |
| `AttendanceIntegrationStatus` | `varchar(30)` | Ya | `"Pending"` | — | — | — | Tidak | Keadaan penerapan ke data kehadiran |
| `BalanceExecutionStatus` | `varchar(30)` | Ya | `"Pending"` | — | — | — | Tidak | Keadaan pemotongan saldo |
| `ExpectedAttendanceDayCount` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah hari kehadiran yang seharusnya terpengaruh |
| `AppliedAttendanceDayCount` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang berhasil diterapkan |
| `ConflictAttendanceDayCount` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang bentrok |
| `FailedAttendanceDayCount` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang gagal |
| `TotalScheduledMinutes` | `integer` | Ya | `0` | — | — | — | Tidak | Total menit terjadwal |
| `TotalPayableLeaveMinutes` | `integer` | Ya | `0` | — | — | — | Tidak | Total menit cuti yang dibayar |
| `StartedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu pelaksanaan dimulai |
| `StartedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang memulai |
| `CompletedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu selesai |
| `CompletedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang menyelesaikan |
| `ReversedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu pembalikan. **Sudah ada di source** |
| `ReversedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang membalik. **Sudah ada di source** |
| `LastAttemptAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Percobaan terakhir |
| `RetryCount` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah percobaan ulang |
| `CorrelationId` | `varchar(120)` | Tidak | — | — | — | — | Tidak | Penanda korelasi lintas proses |
| `IdempotencyKey` | `varchar(160)` | Tidak | — | Unique terfilter bukan-null | — | — | Tidak | Penjaga pengiriman ganda |
| `ExecutionSnapshotJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Rekaman masukan pelaksanaan |
| `ResultSnapshotJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Rekaman hasil pelaksanaan |
| `ErrorSummary` | `varchar(4000)` | Tidak | — | — | — | — | **Ya** | Ringkasan kegagalan; dapat memuat potongan data pegawai |
| `Notes` | `varchar(2000)` | Tidak | — | — | — | — | **Ya** | Catatan bebas |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Penanda aktif |
| `ReversalReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | **`RENCANA`.** Alasan pembalikan pelaksanaan cuti |
| `PayrollLockCheckedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan pelaksanaan memeriksa apakah periode payroll sudah dikunci |

**Index yang sudah ada dan tidak berubah**

| Index | Kolom | Unique | Filter |
| --- | --- | :---: | --- |
| Nomor pelaksanaan | `(ExecutionNumber)` | Ya | `"IsDelete" = false` |
| Satu pelaksanaan per permohonan | `(LeaveRequestId)` | Ya | `"IsDelete" = false` |
| Penjaga pengiriman ganda | `(IdempotencyKey)` | Ya | `"IdempotencyKey" IS NOT NULL AND "IsDelete" = false` |
| Antrean per status dan rentang | `(ExecutionStatus, StartDate, EndDate)` | Tidak | — |
| Riwayat per pegawai | `(WorkforceProfileId, StartDate, EndDate)` | Tidak | — |

**Tidak ada index baru pada tabel ini.**

### 2.4 `TrxLeaveRecall`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."TrxLeaveRecall"` |
| File model | `Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveRecall.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/LeaveManagement/TrxLeaveRecallConfiguration.cs` |
| Alasan berubah | Penarikan pegawai dari cuti kadang harus berjalan tanpa menunggu pegawainya membaca pemberitahuan. Penggantian pengakuan itu **wajib** meninggalkan jejak siapa dan mengapa |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RecallNumber` | `varchar(50)` | Ya | — | Unique, terfilter | — | — | Tidak | Nomor penarikan |
| `LeaveRequestId` | `uuid` | Ya | — | Index bersama `RecallStatus` | FK ke `WfpLeaveRequest` | `Restrict` | Tidak | Cuti yang ditarik |
| `WorkforceProfileId` | `uuid` | Ya | — | — | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai yang ditarik |
| `ReplacementWorkforceProfileId` | `uuid` | Tidak | — | — | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai pengganti bila ada |
| `WorkflowDefinitionId` | `uuid` | Tidak | — | — | FK ke `MstWorkflowDefinition` | `Restrict` | Tidak | Definisi alur persetujuan |
| `WorkflowInstanceId` | `uuid` | Tidak | — | — | — | — | Tidak | Instance workflow; **belum** dikonfigurasi sebagai FK di configuration hari ini |
| `BalanceTransactionId` | `uuid` | Tidak | — | — | FK ke `TrxLeaveBalanceTransaction` | `Restrict` | Tidak | Transaksi pengembalian saldo |
| `OriginalLeaveEndDate` | `date` | Ya | — | — | — | — | Tidak | Tanggal selesai cuti sebelum ditarik |
| `RecallEffectiveDate` | `date` | Ya | — | Index bersama `RecallStatus` | — | — | Tidak | Sejak kapan penarikan berlaku |
| `ActualReturnToWorkDate` | `date` | Tidak | — | — | — | — | Tidak | Tanggal pegawai benar-benar kembali bekerja |
| `RecalledLeaveDays` | `numeric(10,2)` | Ya | `0` | — | — | — | Tidak | Jumlah hari cuti yang ditarik |
| `RestoredBalanceDays` | `numeric(10,2)` | Ya | `0` | — | — | — | Tidak | Jumlah hari yang dikembalikan ke saldo |
| `RecallReason` | `varchar(2000)` | Ya | — | — | — | — | **Ya** | Alasan penarikan; kerap menyebut keadaan unit dan nama orang |
| `RecallStatus` | `varchar(30)` | Ya | `"Draft"` | Index | — | — | Tidak | `Draft`, `Submitted`, `Acknowledged`, `Approved`, `Rejected`, `Applied`, `Cancelled` |
| `InitiatedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Yang memulai penarikan |
| `AcknowledgedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Pegawai yang mengakui penarikan |
| `AcknowledgedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan diakui |
| `ApprovedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Yang menyetujui |
| `ApprovedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan disetujui |
| `AppliedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan diterapkan |
| `Notes` | `varchar(2000)` | Tidak | — | — | — | — | **Ya** | Catatan bebas |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Penanda aktif |
| `AcknowledgementOverrideReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | **`RENCANA`.** Mengapa pengakuan pegawai dilewati |
| `AcknowledgementOverrideByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Siapa yang melewati |
| `AcknowledgementOverrideAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan dilewati |

**Index yang sudah ada dan tidak berubah**

| Index | Kolom | Unique | Filter |
| --- | --- | :---: | --- |
| Nomor penarikan | `(RecallNumber)` | Ya | `"IsDelete" = false` |
| Penarikan per permohonan cuti | `(LeaveRequestId, RecallStatus, IsDelete)` | Tidak | — |
| Antrean per tanggal berlaku | `(RecallEffectiveDate, RecallStatus, IsDelete)` | Tidak | — |

**Tidak ada index baru pada tabel ini.**

**Satu catatan yang perlu ditindaklanjuti implementer.** Kolom `WorkflowInstanceId` ada di model
tetapi **tidak** dikonfigurasi sebagai foreign key di
`TrxLeaveRecallConfiguration.cs`, berbeda dengan `HrdAttendanceCorrectionRequest` yang
mengonfigurasinya. Ini ketidakseragaman yang tercatat, bukan temuan baru yang menuntut migration
sekarang. Menutupnya adalah pekerjaan `REPAIR` yang **MUST** punya task tersendiri.

### 2.5 `TrxWorkflowApproverAssignment`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."TrxWorkflowApproverAssignment"` |
| File model | `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowApproverAssignment.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/WorkflowManagement/TrxWorkflowApproverAssignmentConfiguration.cs` |
| Alasan berubah | `HRD-DEC-030` menetapkan mesin SLA, pengingat, dan eskalasi untuk kotak masuk persetujuan terpadu. Tanpa kolom ini, pengingat tidak punya tempat menyimpan sudah berapa kali ia dikirim |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `WorkflowInstanceId` | `uuid` | Ya | — | Index bersama `AssignmentStatus`, `IsActive`, `IsDelete` | FK ke `TrxWorkflowInstance` | `Restrict` | Tidak | Instance workflow induk |
| `WorkflowStepInstanceId` | `uuid` | Ya | — | **Unique** bersama `AssignmentOrder`; **unique** bersama `AssignedApproverUserId` | FK ke `TrxWorkflowStepInstance` | `Restrict` | Tidak | Langkah yang sedang berjalan |
| `ApprovalMatrixId` | `uuid` | Tidak | — | — | FK ke `MstApprovalMatrix` | `Restrict` | Tidak | Matriks yang menurunkan penunjukan ini |
| `ApprovalDelegationId` | `uuid` | Tidak | — | Index bersama `IsDelegated` | FK ke `TrxApprovalDelegation` | `Restrict` | Tidak | Pendelegasian yang berlaku |
| `AssignedApproverUserId` | `uuid` | Ya | — | Index bersama status dan tenggat | FK ke `ApplicationUser` | `Restrict` | Tidak | Penyetuju yang ditunjuk |
| `AssignedApproverWorkforceProfileId` | `uuid` | Tidak | — | Index bersama `AssignmentStatus` | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Profil penyetuju |
| `OriginalApproverUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Penyetuju asal sebelum didelegasikan |
| `OriginalApproverWorkforceProfileId` | `uuid` | Tidak | — | — | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Profil penyetuju asal |
| `AssignedApproverRoleCode` | `varchar(100)` | Tidak | — | — | — | — | Tidak | Kode peran penyetuju |
| `ApproverSourceSnapshot` | `varchar(50)` | Ya | `RequesterManager` | — | — | — | Tidak | Dari mana penyetuju diturunkan, direkam saat penunjukan |
| `AssignmentOrder` | `integer` | Ya | `1` | Unique bersama `WorkflowStepInstanceId` | — | — | Tidak | Urutan penyetuju. Dijaga check constraint `> 0` |
| `AssignmentStatus` | `varchar(40)` | Ya | `Pending` | Beberapa index | — | — | Tidak | Status penunjukan |
| `AssignedAt` | `timestamp with time zone` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Waktu penunjukan |
| `AvailableAt` | `timestamp with time zone` | Tidak | — | Index bersama status | — | — | Tidak | Sejak kapan penyetuju boleh bertindak |
| `StartedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan penyetuju mulai membaca |
| `DueAt` | `timestamp with time zone` | Tidak | — | Index bersama status; **index baru bersama `AssignmentStatus`** | — | — | Tidak | Tenggat SLA |
| `CompletedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan selesai. Dijaga check constraint `>= AssignedAt` |
| `DelegatedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Kapan didelegasikan |
| `IsRequired` | `boolean` | Ya | `true` | — | — | — | Tidak | Penyetuju wajib atau opsional |
| `IsCurrentAssignment` | `boolean` | Ya | `false` | — | — | — | Tidak | Menandai penunjukan yang sedang berjalan |
| `IsDelegated` | `boolean` | Ya | `false` | Index bersama `ApprovalDelegationId` | — | — | Tidak | Menandai hasil pendelegasian |
| `ResolutionSnapshotJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Rekaman keputusan penyetuju |
| `IsActive` | `boolean` | Ya | `true` | Index bersama `WorkflowInstanceId` | — | — | Tidak | Penanda aktif |
| `LastReminderSentAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan pengingat terakhir dikirim |
| `ReminderCount` | `integer` | Ya | `0` | — | — | — | Tidak | **`RENCANA`.** Sudah berapa kali diingatkan |
| `EscalatedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Kapan dieskalasi |
| `EscalatedToUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Dieskalasi kepada siapa |

**Check constraint yang sudah ada**

| Nama | Aturan |
| --- | --- |
| `CK_TrxWorkflowApproverAssignment_AssignmentOrder` | `"AssignmentOrder" > 0` |
| `CK_TrxWorkflowApproverAssignment_CompletedAt` | `"CompletedAt" IS NULL OR "CompletedAt" >= "AssignedAt"` |

**Index yang ditambahkan rancangan ini**

| Index | Kolom | Unique | Alasan |
| --- | --- | :---: | --- |
| Pemindaian tenggat | `(AssignmentStatus, DueAt)` | Tidak | Pemroses pengingat membaca kombinasi ini setiap putaran. Tanpa index, biayanya tumbuh sebanding jumlah seluruh penunjukan |

**Peringatan operasional.** Pada basis data yang sudah besar, index ini **SHOULD** dibuat
*concurrently* supaya tabel tidak terkunci selama pembuatannya. Ini dicatat di
`02-backend-architecture.md` bagian 8 sebagai bagian rencana migration.

### 2.6 `TrxShiftAssignment`

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."TrxShiftAssignment"` |
| File model | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxShiftAssignment.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/TrxShiftAssignmentConfiguration.cs` |
| Alasan berubah | **Tidak ada kolom yang berubah.** Yang berubah adalah keberadaan API-nya (`HRD-DEC-026`), ditambah satu index untuk resolusi jadwal |

Tabel ini diberi status `Diperbarui` **hanya** karena index-nya bertambah. Kolomnya ditulis
lengkap agar implementer tidak menyimpulkan bahwa "tidak ada kolom baru" berarti "tidak perlu
dibaca".

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RosterAssignmentId` | `uuid` | Ya | — | Index bersama `ShiftDate`, `AssignmentStatus` | FK ke `TrxRosterAssignment` | `Restrict` | Tidak | Penugasan roster induk |
| `WorkforceProfileId` | `uuid` | Ya | — | Index bersama jadwal; **index baru** | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai yang dijadwalkan |
| `WorkScheduleId` | `uuid` | Tidak | — | — | FK ke `MstWorkSchedule` | `Restrict` | Tidak | Jadwal kerja acuan |
| `ShiftId` | `uuid` | Tidak | — | Index bersama lokasi dan tanggal | FK ke `MstShift` | `Restrict` | Tidak | Shift yang dipakai |
| `HospitalSiteId` | `uuid` | Tidak | — | Index bersama lokasi | FK ke `MstHospitalSite` | `Restrict` | Tidak | Lokasi rumah sakit |
| `OrganizationUnitId` | `uuid` | Tidak | — | Index bersama lokasi | FK ke `MstOrganizationUnit` | `Restrict` | Tidak | Unit organisasi |
| `DepartmentId` | `uuid` | Tidak | — | Index bersama lokasi | FK ke `MstDepartment` | `Restrict` | Tidak | Departemen |
| `WorkLocationId` | `uuid` | Tidak | — | — | FK ke `MstWorkLocation` | `Restrict` | Tidak | Lokasi kerja |
| `DailyStaffingRequirementId` | `uuid` | Tidak | — | — | FK ke `TrxDailyStaffingRequirement` | `Restrict` | Tidak | Kebutuhan tenaga harian |
| `ShiftSkillRequirementId` | `uuid` | Tidak | — | — | FK ke `MstShiftSkillRequirement` | `Restrict` | Tidak | Kebutuhan keahlian |
| `ScheduleChangeRequestId` | `uuid` | Tidak | — | — | FK ke `WfpScheduleChangeRequest` | `Restrict` | Tidak | Permohonan ubah jadwal yang melahirkan baris ini |
| `ShiftSwapRequestId` | `uuid` | Tidak | — | — | FK ke `WfpShiftSwapRequest` | `Restrict` | Tidak | Permohonan tukar shift |
| `ShiftDate` | `date` | Ya | — | Index bersama roster; **index baru** | — | — | Tidak | Tanggal shift |
| `ScheduledStartAt` | `timestamp with time zone` | Ya | — | Index bersama pegawai | — | — | Tidak | Waktu mulai terjadwal |
| `ScheduledEndAt` | `timestamp with time zone` | Ya | — | Index bersama pegawai | — | — | Tidak | Waktu selesai terjadwal |
| `BreakDurationMinutes` | `integer` | Ya | `0` | — | — | — | Tidak | Lama istirahat |
| `PlannedWorkMinutes` | `integer` | Ya | `0` | — | — | — | Tidak | Menit kerja terencana |
| `AssignmentType` | `varchar(30)` | Ya | `"Regular"` | — | — | — | Tidak | `Regular`, `Overtime`, `OnCall`, `Training`, `Remote`, `BusinessTrip`, `DayOff` |
| `AssignmentStatus` | `varchar(30)` | Ya | `"Draft"` | Index bersama roster | — | — | Tidak | `Draft`, `Validated`, `Published`, `Confirmed`, `Completed`, `Cancelled`, `Replaced` |
| `AssignmentSource` | `varchar(30)` | Tidak | `"Roster"` | — | — | — | Tidak | `Roster`, `Manual`, `ScheduleChange`, `ShiftSwap`, `Emergency`, `Import` |
| `IsNightShift` | `boolean` | Ya | `false` | — | — | — | Tidak | Penanda shift malam |
| `IsOnCall` | `boolean` | Ya | `false` | — | — | — | Tidak | Penanda siaga |
| `IsDayOff` | `boolean` | Ya | `false` | — | — | — | Tidak | Penanda hari libur |
| `IsManualOverride` | `boolean` | Ya | `false` | — | — | — | Tidak | Ditetapkan manual, melewati penjadwal |
| `HasDoubleShiftConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Bentrok shift ganda |
| `HasLeaveConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Bentrok dengan cuti |
| `HasTrainingConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Bentrok dengan pelatihan |
| `HasMinimumRestConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Melanggar istirahat minimum |
| `HasWorkHourLimitConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Melanggar batas jam kerja |
| `HasLicenseConflict` | `boolean` | Ya | `false` | — | — | — | **Ya** | Menyiratkan keadaan lisensi profesi seseorang |
| `HasClinicalPrivilegeConflict` | `boolean` | Ya | `false` | — | — | — | **Ya** | Menyiratkan keadaan kewenangan klinis seseorang. **Kolom ini tidak menjadikan `S-C1` boleh dirancang** |
| `HasMinimumStaffingConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Tenaga di bawah minimum |
| `HasSkillMixConflict` | `boolean` | Ya | `false` | — | — | — | Tidak | Bauran keahlian tidak terpenuhi |
| `HasBlockingConflict` | `boolean` | Ya | `false` | Index bersama `IsValidationPassed` | — | — | Tidak | Ada bentrok yang menahan publikasi |
| `IsValidationPassed` | `boolean` | Ya | `false` | Index bersama `HasBlockingConflict` | — | — | Tidak | Lolos pemeriksaan |
| `ValidatedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | Waktu pemeriksaan |
| `ValidatedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Siapa yang memeriksa |
| `ValidationResultJson` | `jsonb` | Tidak | — | — | — | — | **Ya** | Rincian hasil pemeriksaan; memuat sebab bentrok lisensi dan kewenangan klinis |
| `OverrideReason` | `varchar(1000)` | Tidak | — | — | — | — | **Ya** | Alasan penetapan manual |
| `Notes` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | Catatan bebas |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Penanda aktif |

**Index yang sudah ada dan tidak berubah**

| Index | Kolom | Unique |
| --- | --- | :---: |
| Bentang jadwal per pegawai | `(WorkforceProfileId, ScheduledStartAt, ScheduledEndAt, IsDelete)` | Tidak |
| Roster per tanggal | `(RosterAssignmentId, ShiftDate, AssignmentStatus)` | Tidak |
| Papan jadwal per lokasi | `(HospitalSiteId, OrganizationUnitId, DepartmentId, ShiftDate, ShiftId)` | Tidak |
| Antrean bentrok | `(HasBlockingConflict, IsValidationPassed, IsDelete)` | Tidak |

**Index yang ditambahkan rancangan ini**

| Index | Kolom | Unique | Alasan |
| --- | --- | :---: | --- |
| Resolusi jadwal harian | `(WorkforceProfileId, ShiftDate, IsActive)` | Tidak | Pengolah kehadiran mencari "shift pegawai ini pada tanggal ini" untuk setiap baris kehadiran yang diproses |

**Unique constraint yang sengaja DITAHAN.** Secara aturan bisnis, seorang pegawai seharusnya
punya paling banyak satu penugasan shift aktif per tanggal, yang berarti
`(WorkforceProfileId, ShiftDate, IsActive)` layak menjadi unique. Constraint itu **tidak**
dipasang pada rancangan ini.

Alasannya bukan keraguan desain, melainkan keselamatan data: memasang unique constraint pada
tabel yang **sudah berisi** baris ganda akan **menggagalkan migration** dan menghentikan proses
pemasangan di tengah jalan. Apakah baris ganda itu ada hari ini belum diketahui, dan itulah isi
`HRD-Q-05`. Sampai audit data menjawabnya, statusnya **`BLOCKED`**, bukan "nanti dikerjakan".

---

### 2.7 Empat entity penempatan dan remunerasi — `HRD-DEC-031`

Keempat entity di bawah naik menjadi `Diperbarui` karena `HRD-DEC-031` menetapkan persetujuan
wajib beserta pemisahan peran. Sebelum keputusan itu, keempatnya `Sudah ada` dan tidak disentuh.

| Aspek | Isi |
| --- | --- |
| File model | `Areas/Corporate/HumanResource/WorkforceCore/Models/Wfp{Salary,Organization,Position,Manager}Assignment.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/WorkforceCore/` |
| Alasan berubah | Perubahan yang mengubah posisi efektif dan remunerasi pegawai wajib disetujui pihak yang berbeda dari pembuatnya |

#### 2.7.1 Kolom persetujuan yang ditambahkan pada keempatnya

Kolom berikut ditambahkan **sama bentuknya** pada `WfpSalaryAssignment`,
`WfpOrganizationAssignment`, `WfpPositionAssignment`, dan `WfpManagerAssignment`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `WorkflowDefinitionId` | `uuid` | Tidak | — | — | FK ke `MstWorkflowDefinition` | `Restrict` | Tidak | **`RENCANA`.** Definisi alur `T8` yang dipakai |
| `WorkflowInstanceId` | `uuid` | Tidak | — | Unique terfilter bukan-null | FK ke `TrxWorkflowInstance` | `Restrict` | Tidak | **`RENCANA`.** Satu perubahan paling banyak satu instance workflow |
| `ApprovalStatus` | `varchar(30)` | Ya | `"Draft"` | **Index baru** bersama `EffectiveStartDate` | — | — | Tidak | **`RENCANA`.** `Draft`, `Submitted`, `UnderReview`, `NeedRevision`, `Approved`, `Rejected`, `Cancelled` |
| `SubmittedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Pembuat yang mengajukan |
| `SubmittedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Waktu diajukan |
| `RejectedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Penolak |
| `RejectedAt` | `timestamp with time zone` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Waktu ditolak |
| `RejectionReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | **`RENCANA`.** Alasan penolakan; dapat menyebut keadaan pribadi |

#### 2.7.2 Kolom persetujuan yang sudah ada, dan yang belum

| Entity | `ApprovedByUserId` / `ApprovedAt` | Endpoint persetujuan | Yang perlu ditambahkan |
| --- | --- | --- | --- |
| `WfpSalaryAssignment` | **Sudah ada** | **Sudah ada** — `PATCH /{id}/approval` | Delapan kolom pada 2.7.1, **kecuali** `ApprovedByUserId`/`ApprovedAt` |
| `WfpOrganizationAssignment` | Tidak ada | Tidak ada | Seluruh kolom 2.7.1 **ditambah** `ApprovedByUserId`, `ApprovedAt` |
| `WfpPositionAssignment` | Tidak ada | Tidak ada | Sama |
| `WfpManagerAssignment` | Tidak ada | Tidak ada | Sama |

#### 2.7.3 Dua penjaga yang tidak berupa kolom

**Menambahkan kolom saja tidak memenuhi `HRD-DEC-031`.** Kamus data hanya dapat menyiapkan
tempatnya; yang membuat keputusan itu berlaku adalah dua penjaga di tingkat aturan bisnis:

| Penjaga | Aturan | Keadaan hari ini |
| --- | --- | --- |
| **Gerbang efektivitas** | Penempatan **MUST NOT** berlaku sebelum `ApprovalStatus` bernilai `Approved` | **Belum ada.** Tidak ada satu pun pemeriksaan sebelum penempatan berlaku |
| **Pemisahan peran** | `SubmittedByUserId` **MUST NOT** sama dengan `ApprovedByUserId` | **Belum ada.** Endpoint persetujuan gaji memakai butir hak akses yang sama dengan buat dan ubah |

Keduanya adalah pekerjaan implementasi yang **wajib** direncanakan bersama penambahan kolomnya.
Merencanakan kolom tanpa penjaganya menghasilkan tabel yang terlihat aman tetapi tidak menjaga
apa pun.

#### 2.7.4 Bentuk DDL

```sql
-- Bentuk perubahan sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Berlaku sama untuk keempat tabel; contoh memakai WfpOrganizationAssignment.
ALTER TABLE public."WfpOrganizationAssignment"
    ADD COLUMN "WorkflowDefinitionId" uuid,                              -- RENCANA
    ADD COLUMN "WorkflowInstanceId"   uuid,                              -- RENCANA
    ADD COLUMN "ApprovalStatus"       varchar(30) NOT NULL DEFAULT 'Draft', -- RENCANA
    ADD COLUMN "SubmittedByUserId"    uuid,                              -- RENCANA
    ADD COLUMN "SubmittedAt"          timestamptz,                       -- RENCANA
    ADD COLUMN "ApprovedByUserId"     uuid,                              -- RENCANA
    ADD COLUMN "ApprovedAt"           timestamptz,                       -- RENCANA
    ADD COLUMN "RejectedByUserId"     uuid,                              -- RENCANA
    ADD COLUMN "RejectedAt"           timestamptz,                       -- RENCANA
    ADD COLUMN "RejectionReason"      varchar(500);                      -- RENCANA, SENSITIF

CREATE INDEX "IX_WfpOrganizationAssignment_ApprovalStatus_EffectiveStartDate"
    ON public."WfpOrganizationAssignment" ("ApprovalStatus", "EffectiveStartDate");

CREATE UNIQUE INDEX "IX_WfpOrganizationAssignment_WorkflowInstanceId"
    ON public."WfpOrganizationAssignment" ("WorkflowInstanceId")
    WHERE "WorkflowInstanceId" IS NOT NULL AND "IsDelete" = false;

-- WfpSalaryAssignment TIDAK menambahkan "ApprovedByUserId" dan "ApprovedAt":
-- keduanya sudah ada di tabel itu.
```

---

## 3. Tabel berstatus `Sudah ada` — kolom kunci

Untuk tabel di bawah, hanya kolom kunci yang ditulis: primary key, foreign key, kolom status, dan
kolom yang dipakai aturan bisnis modul ini. **Sumber lengkapnya adalah file model yang dirujuk**,
bukan dokumen ini. Menyalin ratusan kolom yang tidak berubah hanya menciptakan salinan kedua yang
pasti menyimpang setelah revisi pertama.

### 3.1 Profil dan administrasi workforce

#### `MstWorkforceProfile`

`Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstWorkforceProfile.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK. Dirujuk hampir seluruh tabel transaksi HR | Tidak |
| `ProfileCode` | `varchar(50)` | Ya | Kode pegawai yang dilihat pengguna | Tidak |
| `UserType` | `enum` | Ya | Membedakan pegawai, dokter, dan pengguna luar | Tidak |
| `DisplayName` | `varchar(200)` | Ya | Nama yang ditampilkan | **Ya** |
| `Email` | `varchar(200)` | Tidak | Kontak | **Ya** |
| `PhoneNumber` | `varchar(30)` | Tidak | Kontak | **Ya** |
| `WhatsAppNumber` | `varchar(30)` | Tidak | Kontak | **Ya** |
| `PrimaryDepartmentId` | `uuid` | Tidak | FK ke `MstDepartment` | Tidak |
| `PrimaryPositionId` | `uuid` | Tidak | FK ke `MstPosition` | Tidak |
| `IsActive` | `boolean` | Ya | Penanda aktif | Tidak |

#### `WfpSalaryAssignment`

`Areas/Corporate/HumanResource/WorkforceCore/Models/WfpSalaryAssignment.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `WorkforceProfileId` | `uuid` | Ya | FK ke `MstWorkforceProfile` | Tidak |
| `SalaryStructureId` | `uuid` | Ya | FK struktur gaji | Tidak |
| `SalaryGradeId` | `uuid` | Ya | FK golongan gaji | Tidak |
| `EmployeeGradeId` | `uuid` | Tidak | FK kelas jabatan | Tidak |
| `PayrollPeriodId` | `uuid` | Tidak | FK periode payroll | Tidak |
| `BaseSalary` | `numeric` | Ya | Nominal gaji pokok | **Ya** |
| `CurrencyCode` | `varchar(3)` | Ya | Mata uang, bawaan `IDR` | Tidak |
| `IsPrimary` | `boolean` | Ya | Penempatan gaji utama | Tidak |
| `ApprovedByUserId` | `uuid` | Tidak | FK ke `ApplicationUser` | Tidak |

**Aturan yang mengikat tabel ini:** kolom nominal **MUST NOT** masuk custom logger, dan
**MUST NOT** tampil pada layar mana pun yang jangkauan pembacanya lebih luas daripada pemilik
data dan HR yang berwenang. Pengaturannya ada di
[`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md).

#### `TrxEmployeeProfileChangeRequest`

`Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeProfileChangeRequest.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RequestNumber` | `varchar(50)` | Ya | Nomor permohonan | Tidak |
| `WorkforceProfileId` | `uuid` | Ya | FK pemilik data yang diubah | Tidak |
| `WorkflowDefinitionId` | `uuid` | Tidak | FK definisi alur persetujuan | Tidak |
| `RequestReasonId` | `uuid` | Tidak | FK alasan baku | Tidak |
| `RequestStatus` | `varchar(50)` | Ya | Status permohonan | Tidak |
| `RequestedByUserId` | `uuid` | Ya | Pengaju | Tidak |
| `ApprovedByUserId` | `uuid` | Tidak | Penyetuju | Tidak |
| `RejectedByUserId` | `uuid` | Tidak | Penolak | Tidak |
| `AppliedByUserId` | `uuid` | Tidak | Yang menerapkan perubahan | Tidak |

### 3.2 Kehadiran

#### `HrdAttendanceRawLog`

`Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceRawLog.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `UserId` | `uuid` | Tidak | FK akun; boleh kosong karena mesin absensi kadang mengirim identitas mentah | Tidak |
| `WorkforceProfileId` | `uuid` | Tidak | FK profil pegawai setelah dicocokkan | Tidak |
| `EmployeeId`, `DoctorId` | `uuid` | Tidak | Jalur identitas warisan | Tidak |
| `AttendanceDeviceId` | `uuid` | Tidak | FK mesin absensi | Tidak |
| `AttendanceLocationId` | `uuid` | Tidak | FK lokasi absensi | Tidak |
| `HospitalSiteId` | `uuid` | Tidak | FK lokasi rumah sakit | Tidak |
| `ProcessingStatus` | `varchar(30)` | Ya | Keadaan pengolahan rekaman mentah | Tidak |
| `ProcessedAttendanceId` | `uuid` | Tidak | Hasil olahan tingkat rekaman | Tidak |
| `ProcessedAttendanceDailyId` | `uuid` | Tidak | Hasil olahan tingkat hari | Tidak |

**Mengapa hampir seluruh FK boleh kosong.** Tabel ini menerima apa adanya dari mesin absensi.
Rekaman yang identitasnya belum bisa dicocokkan **tetap disimpan**, lalu diolah kemudian. Kalau
kolomnya dibuat wajib, rekaman yang tidak cocok akan hilang — dan justru rekaman itulah yang
paling perlu ditelusuri.

#### `HrdAttendanceDaily`

`Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceDaily.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK. Induk pengecualian dan permohonan koreksi | Tidak |
| `UserId` | `uuid` | Ya | FK akun pemilik hari kehadiran | Tidak |
| `WorkforceProfileId` | `uuid` | Tidak | FK profil pegawai | Tidak |
| `OrganizationAssignmentId` | `uuid` | Tidak | FK penempatan organisasi saat itu | Tidak |
| `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId`, `PositionId` | `uuid` | Tidak | Rekaman struktur organisasi pada hari itu | Tidak |
| `WorkLocationId` | `uuid` | Tidak | FK lokasi kerja | Tidak |
| `WorkScheduleId`, `WorkScheduleAssignmentId` | `uuid` | Tidak | FK jadwal yang berlaku | Tidak |
| `PrimaryShiftAssignmentId` | `uuid` | Tidak | FK ke `TrxShiftAssignment` yang dipakai | Tidak |
| `ShiftId` | `uuid` | Tidak | FK shift | Tidak |
| `AttendancePolicyId` | `uuid` | Tidak | FK kebijakan kehadiran | Tidak |

**Mengapa struktur organisasi ikut disalin ke sini.** Ini **bukan** duplikasi master. Kolom-kolom
itu merekam **keadaan pada hari itu**. Kalau seorang pegawai pindah departemen bulan depan,
kehadirannya bulan ini tetap harus terbaca sebagai kehadiran di departemen lamanya. Menghitung
ulang dari master akan mengubah masa lalu.

#### `HrdAttendancePeriod`

`Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendancePeriod.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `PeriodCode` | `varchar(50)` | Ya | Kode periode | Tidak |
| `LegalEntityId`, `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId` | `uuid` | Tidak | Cakupan periode | Tidak |
| `PeriodStatus` | `varchar(30)` | Ya | **Kunci penutupan.** Menahan koreksi setelah periode ditutup | Tidak |
| `LastProcessingRunId` | `uuid` | Tidak | Proses pengolahan terakhir | Tidak |
| `ClosedByUserId` | `uuid` | Tidak | Siapa yang menutup | Tidak |
| `ReopenedByUserId` | `uuid` | Tidak | Siapa yang membuka kembali | Tidak |

### 3.3 Cuti dan saldo

#### `WfpLeaveBalance`

`Areas/Corporate/HumanResource/LeaveManagement/Models/WfpLeaveBalance.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `WorkforceProfileId` | `uuid` | Ya | FK pemilik saldo |
| `LeaveTypeId` | `uuid` | Ya | FK jenis cuti |
| `LeavePolicyId`, `LeaveEntitlementPolicyId`, `LeaveEntitlementPeriodId` | `uuid` | Tidak | FK kebijakan dan periode hak cuti |
| `BalanceStatus` | `varchar(30)` | Ya | Status saldo |
| `LastTransactionId` | `uuid` | Tidak | Transaksi terakhir yang mengubah saldo |
| `LockedByUserId` | `uuid` | Tidak | Penanda penguncian saldo |

#### `WfpLeaveRequest`

`Areas/Corporate/HumanResource/LeaveManagement/Models/WfpLeaveRequest.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `RequestNumber` | `varchar(50)` | Ya | Nomor permohonan |
| `WorkforceProfileId` | `uuid` | Ya | FK pemohon |
| `LeaveTypeId` | `uuid` | Ya | FK jenis cuti |
| `LeavePolicyId`, `LeaveBalanceId` | `uuid` | Tidak | FK kebijakan dan saldo yang dipotong |
| `OrganizationAssignmentId`, `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId`, `PositionId` | `uuid` | Tidak | Rekaman struktur organisasi saat permohonan |
| `ReplacementWorkforceProfileId` | `uuid` | Tidak | FK pegawai pengganti |
| `RequestReasonId` | `uuid` | Tidak | FK alasan baku |

Kolom alasan bebas pada tabel ini bersifat **sensitif**: permohonan cuti kerap menyebut alasan
kesehatan atau keluarga.

#### `TrxLeaveBalanceTransaction`

`Areas/Corporate/HumanResource/LeaveManagement/Models/TrxLeaveBalanceTransaction.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `TransactionNumber` | `varchar(50)` | Ya | Nomor transaksi |
| `LeaveBalanceId` | `uuid` | Ya | FK saldo yang bergerak |
| `WorkforceProfileId` | `uuid` | Ya | FK pemilik saldo |
| `LeaveTypeId` | `uuid` | Ya | FK jenis cuti |
| `LeaveRequestId` | `uuid` | Tidak | FK permohonan yang memicu |
| `LeaveEntitlementId`, `LeaveAccrualId`, `LeaveCarryForwardId`, `LeaveAdjustmentId` | `uuid` | Tidak | Sumber pergerakan saldo |
| `ReversedTransactionId`, `OriginalTransactionId` | `uuid` | Tidak | **Rantai pembalikan.** Transaksi tidak dihapus, tetapi dibalik dengan transaksi baru |
| `PostingBatchId` | `uuid` | Tidak | Kelompok pembukuan |

**Aturan buku besar yang mengikat tabel ini:** saldo cuti **MUST NOT** diperbaiki dengan mengubah
angka pada `WfpLeaveBalance` secara langsung. Setiap perubahan saldo **MUST** meninggalkan baris
di tabel ini. Itulah sebabnya kolom `ReversedTransactionId` ada.

### 3.4 Lembur

#### `WfpOvertimeRequest`

`Areas/Corporate/HumanResource/OvertimeManagement/Models/WfpOvertimeRequest.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `RequestNumber` | `varchar(50)` | Ya | Nomor permohonan |
| `WorkforceProfileId` | `uuid` | Ya | FK pemohon |
| `OrganizationAssignmentId`, `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId`, `PositionId`, `CostCenterId` | `uuid` | Tidak | Rekaman struktur organisasi dan pusat biaya |
| `OvertimePolicyId` | `uuid` | Tidak | FK kebijakan lembur |
| `SourceOvertimePlanDetailId` | `uuid` | Tidak | FK rencana lembur asal |
| `WorkScheduleAssignmentId`, `RosterPeriodId` | `uuid` | Tidak | FK jadwal dan periode roster |

#### `TrxOvertimeRealization`

`Areas/Corporate/HumanResource/OvertimeManagement/Models/TrxOvertimeRealization.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RealizationNumber` | `varchar(50)` | Ya | Nomor realisasi | Tidak |
| `OvertimeRequestId` | `uuid` | Ya | FK permohonan lembur | Tidak |
| `WorkforceProfileId` | `uuid` | Ya | FK pegawai | Tidak |
| `AttendanceDailyId` | `uuid` | Tidak | **Jembatan ke kehadiran.** Realisasi dibuktikan data kehadiran, bukan pengakuan | Tidak |
| `CostCenterId` | `uuid` | Tidak | FK pusat biaya | Tidak |
| `CurrencyCode` | `varchar(10)` | Ya | Mata uang | Tidak |
| `RealizationStatus` | `varchar(40)` | Ya | Status realisasi | Tidak |
| Kolom nominal pembayaran lembur | `numeric` | — | Nilai yang diteruskan ke payroll | **Ya** |

### 3.5 Penjadwalan kerja

#### `TrxRosterPeriod`

`Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxRosterPeriod.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `RosterPeriodCode` | `varchar(50)` | Ya | Kode periode roster |
| `LegalEntityId`, `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId` | `uuid` | Tidak | Cakupan roster |
| `RosterPolicyId`, `MinimumRestPolicyId` | `uuid` | Tidak | FK kebijakan roster dan istirahat minimum |
| `WorkflowDefinitionId`, `WorkflowInstanceId` | `uuid` | Tidak | FK alur persetujuan roster |
| `RosterStatus` | `varchar(30)` | Ya | Status roster |
| `ValidatedByUserId`, `SubmittedByUserId`, `ApprovedByUserId` | `uuid` | Tidak | Jejak pemeriksa dan penyetuju |

#### Enam tabel roster lain

`TrxRosterAssignment`, `TrxRosterPublication`, `TrxRosterApproval`, `TrxShiftReplacement`,
`TrxEmergencyStaffingRequest`, dan `TrxOnCallAssignment` seluruhnya berstatus `Sudah ada` dengan
skema lengkap yang dibuat migration `20260726161839_initializeBigModulHRD2`.

**Yang belum ada pada ketujuh tabel roster adalah perilakunya, bukan tabelnya.** `HRD-DEC-026`
menetapkan arah `EXTEND` berupa penambahan controller, service, dan DTO — **bukan** perancangan
skema baru. Larangan yang menyertainya mengikat: jangan membuat skema baru sebelum model existing
diaudit satu per satu, dan `HRD-Q-05` **wajib** terjawab lebih dulu bila perubahan yang merusak
data ternyata diperlukan.

Kemungkinan index tambahan yang tercatat di `02-backend-architecture.md` bagian 7.2:

| Tabel | Index yang mungkin diperlukan | Status |
| --- | --- | --- |
| `TrxRosterAssignment` | `(RosterPeriodId, WorkforceProfileId)` | **Rencana**, menunggu bukti pola query dari implementasi |
| `TrxOnCallAssignment` | `(WorkforceProfileId, OnCallDate)` | **Rencana**, alasan yang sama |

### 3.6 Persetujuan bersama

#### `TrxWorkflowInstance`

`Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `WorkflowDefinitionId` | `uuid` | Ya | FK definisi alur |
| `RequestedByUserId` | `uuid` | Ya | Akun pengaju |
| `RequestedByWorkforceProfileId` | `uuid` | Tidak | Profil pengaju |
| `OrganizationAssignmentId`, `LegalEntityId`, `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId`, `CostCenterId` | `uuid` | Tidak | Rekaman struktur organisasi saat pengajuan |
| `ReferenceId` | `uuid` | Ya | **Penunjuk ke dokumen yang disetujui.** Inilah yang membuat satu mesin melayani banyak jenis transaksi |
| `RequestNumber` | `varchar(60)` | Ya | Nomor permohonan |
| `CurrentStepCode` | `varchar(50)` | Tidak | Langkah yang sedang berjalan |

**Mengapa `ReferenceId` tidak berupa foreign key.** Satu instance workflow bisa menunjuk
permohonan cuti, permohonan lembur, koreksi kehadiran, atau pengunduran diri. Foreign key hanya
bisa menunjuk satu tabel. Konsekuensinya dipikul secara sadar: keutuhan rujukan **MUST** dijaga
di lapisan aturan bisnis, bukan oleh basis data. Hal ini dicatat sebagai kewenangan yang tidak
dapat dijaga mesin pada
[`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md).

### 3.7 Lifecycle dan pengembangan orang

#### `WfpPerformanceReview`

`Areas/Corporate/HumanResource/PerformanceManagement/Models/WfpPerformanceReview.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `ReviewNumber` | `varchar(60)` | Ya | Nomor penilaian | Tidak |
| `WorkforceProfileId` | `uuid` | Ya | FK pegawai yang dinilai | Tidak |
| `PerformanceCycleId`, `MasterPerformanceCycleId` | `uuid` | Tidak | FK siklus penilaian | Tidak |
| `PerformanceTemplateId`, `RatingScaleId` | `uuid` | Tidak | FK template dan skala nilai | Tidak |
| `ReviewerUserId`, `ManagerUserId` | `uuid` | Tidak | Penilai dan atasan | Tidak |
| `ReviewStatus` | `varchar(50)` | Ya | Status penilaian | Tidak |
| `FinalizedByUserId` | `uuid` | Tidak | Yang memfinalkan | Tidak |
| Kolom nilai dan catatan penilaian | `numeric`, `text` | — | Isi penilaian | **Ya** |

#### `WfpDisciplinaryAction`

`Areas/Corporate/HumanResource/EmployeeRelationManagement/Models/WfpDisciplinaryAction.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `ActionCode` | `varchar(60)` | Ya | Kode tindakan | Tidak |
| `WorkforceProfileId` | `uuid` | Ya | FK pegawai yang dikenai | Tidak |
| `DisciplinaryCaseId`, `DisciplinaryDecisionId`, `IncidentReportId` | `uuid` | Tidak | FK kasus, keputusan, dan laporan insiden | Tidak |
| `DisciplinaryActionTypeId` | `uuid` | Ya | FK jenis tindakan | Tidak |
| `ViolationTypeId`, `SanctionTypeId`, `EmployeeRelationCaseTypeId` | `uuid` | Tidak | FK jenis pelanggaran dan sanksi | Tidak |
| `WorkflowDefinitionId` | `uuid` | Tidak | FK alur persetujuan | Tidak |
| Kolom uraian kasus dan sanksi | `text` | — | Isi tindakan | **Ya** |

**Seluruh isi tabel ini bersifat sensitif secara keseluruhan.** Ia memuat catatan kedisiplinan
yang jangkauan pembacanya paling sempit di seluruh modul HR.

#### `TrxResignationRequest`

`Areas/Corporate/HumanResource/LifecycleManagement/Models/TrxResignationRequest.cs`

| Kolom kunci | Tipe | Wajib | Peran |
| --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | PK |
| `RequestNumber` | `varchar(50)` | Ya | Nomor permohonan |
| `WorkforceProfileId` | `uuid` | Ya | FK pegawai yang mengundurkan diri |
| `EmployeeSeparationId` | `uuid` | Tidak | FK proses pemisahan yang menyusul |
| `RequestReasonId`, `RejectionReasonId` | `uuid` | Tidak | FK alasan baku |
| `WorkflowDefinitionId`, `WorkflowInstanceId` | `uuid` | Tidak | FK alur persetujuan |
| `RequestStatus` | `varchar(30)` | Tidak | Status permohonan |
| `SubmittedByUserId`, `ApprovedByUserId`, `RejectedByUserId` | `uuid` | Tidak | Jejak pengaju dan pemutus |

### 3.8 Payroll sisi HR

#### `TrxPayrollRun`

`Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRun.cs`

| Kolom kunci | Tipe | Wajib | Peran | Sensitif |
| --- | --- | :---: | --- | :---: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `PayrollPeriodId` | `uuid` | Ya | FK periode payroll | Tidak |
| `LegalEntityId`, `HospitalSiteId` | `uuid` | Tidak | Cakupan proses | Tidak |
| `WorkflowDefinitionId`, `WorkflowInstanceId` | `uuid` | Tidak | FK alur persetujuan | Tidak |
| `RunNumber` | `varchar(50)` | Ya | Nomor proses | Tidak |
| `RunStatus` | `varchar(30)` | Ya | **Batas tanggung jawab HR.** `HRD-DEC-009` menghentikan HR setelah `execute` | Tidak |
| `CurrencyCode` | `varchar(3)` | Ya | Mata uang | Tidak |
| `LockedByUserId`, `CalculatedByUserId`, `SubmittedByUserId`, `ApprovedByUserId`, `PostedByUserId` | `uuid` | Tidak | Jejak setiap tahap | Tidak |
| Kolom nominal total payroll | `numeric` | — | Agregat nilai | **Ya** |

**Batas yang mengikat tabel ini.** Modul HR **MUST NOT** menambah kolom yang menyimpan hasil
pembayaran, jurnal akuntansi, atau perhitungan pajak pada tabel ini. Bentuk serah terima ke
Finance masih `[OPEN]` — `HRD-Q-10` dan `HRD-Q-11`. Merancangnya sekarang berarti mengarang
kontrak milik modul lain.

---

## 4. Skema tabel dalam bentuk DDL

### 4.1 Peringatan yang berlaku untuk seluruh bagian ini

> **Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual.**
>
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya
> akan berbenturan dengan migration dan merusak riwayat skema. Sumber kebenaran bentuk tabel
> tetap file configuration EF Core yang dirujuk pada setiap bagian.
>
> Kolom audit `IdentityModel` **tidak** ditulis ulang di sini; daftarnya ada pada bagian 0.1.

Hanya tabel berstatus `Diperbarui` yang mendapat DDL. Tidak ada tabel `Baru` pada rancangan ini.
Untuk tabel `Sudah ada` yang tidak berubah, rujukan file configuration-nya sudah cukup.

### 4.2 `HrdAttendanceException`

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Sumber: Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/
--         HrdAttendanceExceptionConfiguration.cs
CREATE TABLE public."HrdAttendanceException" (
    "Id"                     uuid          NOT NULL,
    "AttendanceDailyId"      uuid          NOT NULL,
    "WorkforceProfileId"     uuid,
    "CorrectionRequestId"    uuid,
    "ExceptionCode"          varchar(50)   NOT NULL,
    "ExceptionType"          varchar(50)   NOT NULL,
    "Severity"               varchar(20)   NOT NULL,
    "ExceptionStatus"        varchar(30)   NOT NULL,
    "DetectedAt"             timestamptz   NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpectedAt"             timestamptz,
    "ActualAt"               timestamptz,
    "DifferenceMinutes"      integer,
    "IsAutoDetected"         boolean       NOT NULL DEFAULT true,
    "IsPayrollBlocking"      boolean       NOT NULL DEFAULT false,
    "DetectionRule"          varchar(100),
    "Message"                varchar(1000),          -- SENSITIF
    "ResolvedByUserId"       uuid,
    "ResolvedAt"             timestamptz,
    "ResolutionNote"         varchar(1000),          -- SENSITIF
    "IsActive"               boolean       NOT NULL DEFAULT true,
    "ClassificationDecision" varchar(40),            -- RENCANA
    "ClassifiedByUserId"     uuid,                   -- RENCANA
    "ClassifiedAt"           timestamptz,            -- RENCANA
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_HrdAttendanceException" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdAttendanceException_HrdAttendanceDaily_AttendanceDailyId"
        FOREIGN KEY ("AttendanceDailyId")
        REFERENCES public."HrdAttendanceDaily" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdAttendanceException_MstWorkforceProfile_WorkforceProfileId"
        FOREIGN KEY ("WorkforceProfileId")
        REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdAttendanceException_HrdAttendanceCorrectionRequest_CorrectionRequestId"
        FOREIGN KEY ("CorrectionRequestId")
        REFERENCES public."HrdAttendanceCorrectionRequest" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_HrdAttendanceException_AttendanceDailyId_ExceptionCode"
    ON public."HrdAttendanceException" ("AttendanceDailyId", "ExceptionCode")
    WHERE "IsDelete" = false AND "ExceptionStatus" <> 'Closed';

CREATE INDEX "IX_HrdAttendanceException_ExceptionStatus_Severity_IsPayrollBlocking"
    ON public."HrdAttendanceException" ("ExceptionStatus", "Severity", "IsPayrollBlocking");

CREATE INDEX "IX_HrdAttendanceException_WorkforceProfileId_DetectedAt"
    ON public."HrdAttendanceException" ("WorkforceProfileId", "DetectedAt");

-- RENCANA
CREATE INDEX "IX_HrdAttendanceException_ExceptionType_ExceptionStatus"
    ON public."HrdAttendanceException" ("ExceptionType", "ExceptionStatus");
```

### 4.3 `HrdAttendanceCorrectionRequest`

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Sumber: Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/
--         HrdAttendanceCorrectionRequestConfiguration.cs
CREATE TABLE public."HrdAttendanceCorrectionRequest" (
    "Id"                              uuid          NOT NULL,
    "RequestNumber"                   varchar(50)   NOT NULL,
    "WorkforceProfileId"              uuid          NOT NULL,
    "AttendanceDailyId"               uuid,
    "AttendanceId"                    uuid,
    "RequestReasonId"                 uuid,
    "RejectionReasonId"               uuid,
    "WorkflowDefinitionId"            uuid,
    "WorkflowInstanceId"              uuid,
    "RequestedByWorkforceProfileId"   uuid,
    "RequestedByUserId"               uuid,
    "AttendanceDate"                  date          NOT NULL,
    "CorrectionType"                  varchar(50)   NOT NULL DEFAULT 'AttendanceTime',
    "RequestStatus"                   varchar(30)   NOT NULL DEFAULT 'Draft',
    "Reason"                          varchar(1500) NOT NULL,   -- SENSITIF
    "EvidenceFilePath"                varchar(500),             -- SENSITIF
    "EvidenceFileName"                varchar(255),             -- SENSITIF
    "EvidenceContentType"             varchar(100),
    "OriginalSummaryJson"             jsonb,                    -- SENSITIF
    "RequestedSummaryJson"            jsonb,                    -- SENSITIF
    "ApprovedSummaryJson"             jsonb,                    -- SENSITIF
    "SubmittedAt"                     timestamptz,
    "ApprovedAt"                      timestamptz,
    "RejectedAt"                      timestamptz,
    "AppliedAt"                       timestamptz,
    "AppliedByUserId"                 uuid,
    "FinalNote"                       varchar(1000),            -- SENSITIF
    "IsActive"                        boolean       NOT NULL DEFAULT true,
    "InitiatedByUserId"               uuid,                     -- RENCANA
    "IsOnBehalf"                      boolean       NOT NULL DEFAULT false,  -- RENCANA
    "OnBehalfReason"                  varchar(500),             -- RENCANA, SENSITIF
    "OnBehalfNotifiedAt"              timestamptz,              -- RENCANA
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_HrdAttendanceCorrectionRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdAttendanceCorrectionRequest_MstWorkforceProfile_WorkforceProfileId"
        FOREIGN KEY ("WorkforceProfileId")
        REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdAttendanceCorrectionRequest_HrdAttendanceDaily_AttendanceDailyId"
        FOREIGN KEY ("AttendanceDailyId")
        REFERENCES public."HrdAttendanceDaily" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdAttendanceCorrectionRequest_TrxWorkflowInstance_WorkflowInstanceId"
        FOREIGN KEY ("WorkflowInstanceId")
        REFERENCES public."TrxWorkflowInstance" ("Id") ON DELETE RESTRICT
    -- FK lain mengikuti configuration: AttendanceId, RequestReasonId, RejectionReasonId,
    -- WorkflowDefinitionId, RequestedByWorkforceProfileId, RequestedByUserId, AppliedByUserId
);

CREATE UNIQUE INDEX "IX_HrdAttendanceCorrectionRequest_RequestNumber"
    ON public."HrdAttendanceCorrectionRequest" ("RequestNumber")
    WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_HrdAttendanceCorrectionRequest_WorkflowInstanceId"
    ON public."HrdAttendanceCorrectionRequest" ("WorkflowInstanceId")
    WHERE "WorkflowInstanceId" IS NOT NULL AND "IsDelete" = false;

CREATE INDEX "IX_HrdAttendanceCorrectionRequest_WorkforceProfileId_AttendanceDate"
    ON public."HrdAttendanceCorrectionRequest" ("WorkforceProfileId", "AttendanceDate");

CREATE INDEX "IX_HrdAttendanceCorrectionRequest_RequestStatus_SubmittedAt"
    ON public."HrdAttendanceCorrectionRequest" ("RequestStatus", "SubmittedAt");

CREATE INDEX "IX_HrdAttendanceCorrectionRequest_WorkflowDefinitionId_RequestStatus"
    ON public."HrdAttendanceCorrectionRequest" ("WorkflowDefinitionId", "RequestStatus");

-- RENCANA
CREATE INDEX "IX_HrdAttendanceCorrectionRequest_IsOnBehalf_RequestStatus"
    ON public."HrdAttendanceCorrectionRequest" ("IsOnBehalf", "RequestStatus");
```

### 4.4 `TrxLeaveExecution` — hanya bagian yang berubah

Tabel ini sudah ada dengan bentuk lengkap. Yang berubah hanya dua kolom, sehingga DDL di bawah
ditulis sebagai penambahan, bukan pembuatan tabel.

```sql
-- Bentuk perubahan sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Sumber: Repositories/Configurations/Corporate/HumanResource/LeaveManagement/
--         TrxLeaveExecutionConfiguration.cs
ALTER TABLE public."TrxLeaveExecution"
    ADD COLUMN "ReversalReason"       varchar(500),   -- RENCANA, SENSITIF
    ADD COLUMN "PayrollLockCheckedAt" timestamptz;    -- RENCANA

-- "ReversedAt" dan "ReversedByUserId" TIDAK ditambahkan: keduanya sudah ada di tabel ini.
```

### 4.5 `TrxLeaveRecall` — hanya bagian yang berubah

```sql
-- Bentuk perubahan sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Sumber: Repositories/Configurations/Corporate/HumanResource/LeaveManagement/
--         TrxLeaveRecallConfiguration.cs
ALTER TABLE public."TrxLeaveRecall"
    ADD COLUMN "AcknowledgementOverrideReason"   varchar(500),  -- RENCANA, SENSITIF
    ADD COLUMN "AcknowledgementOverrideByUserId" uuid,          -- RENCANA
    ADD COLUMN "AcknowledgementOverrideAt"       timestamptz;   -- RENCANA

ALTER TABLE public."TrxLeaveRecall"
    ADD CONSTRAINT "FK_TrxLeaveRecall_ApplicationUser_AcknowledgementOverrideByUserId"
        FOREIGN KEY ("AcknowledgementOverrideByUserId")
        REFERENCES public."AspNetUsers" ("Id") ON DELETE RESTRICT;
```

> Nama tabel akun aplikasi di atas mengikuti bentuk ASP.NET Core Identity yang dipakai project
> ini. Implementer **MUST** memastikannya dari configuration `ApplicationUser` sebelum membuat
> migration, bukan menyalin nama dari dokumen ini.

### 4.6 `TrxWorkflowApproverAssignment` — hanya bagian yang berubah

```sql
-- Bentuk perubahan sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Sumber: Repositories/Configurations/Corporate/HumanResource/WorkflowManagement/
--         TrxWorkflowApproverAssignmentConfiguration.cs
ALTER TABLE public."TrxWorkflowApproverAssignment"
    ADD COLUMN "LastReminderSentAt" timestamptz,                -- RENCANA
    ADD COLUMN "ReminderCount"      integer NOT NULL DEFAULT 0, -- RENCANA
    ADD COLUMN "EscalatedAt"        timestamptz,                -- RENCANA
    ADD COLUMN "EscalatedToUserId"  uuid;                       -- RENCANA

-- Pada basis data besar, index berikut SEBAIKNYA dibuat concurrently agar tabel tidak terkunci.
CREATE INDEX CONCURRENTLY "IX_TrxWorkflowApproverAssignment_AssignmentStatus_DueAt"
    ON public."TrxWorkflowApproverAssignment" ("AssignmentStatus", "DueAt");
```

### 4.7 `TrxShiftAssignment` — hanya index

```sql
-- Tidak ada kolom yang berubah pada tabel ini.
CREATE INDEX "IX_TrxShiftAssignment_WorkforceProfileId_ShiftDate_IsActive"
    ON public."TrxShiftAssignment" ("WorkforceProfileId", "ShiftDate", "IsActive");

-- Unique constraint berikut DITAHAN, bukan ditunda tanpa alasan.
-- Memasangnya pada tabel yang sudah berisi baris ganda akan menggagalkan migration.
-- Status: BLOCKED sampai HRD-Q-05 dijawab dengan audit data yang sebenarnya.
--
-- CREATE UNIQUE INDEX "UX_TrxShiftAssignment_WorkforceProfileId_ShiftDate"
--     ON public."TrxShiftAssignment" ("WorkforceProfileId", "ShiftDate")
--     WHERE "IsActive" = true AND "IsDelete" = false;
```

---

## 5. Rekapitulasi kolom sensitif

Daftar ini adalah masukan langsung bagi aturan logging dan bagi
[`../contracts/permission-audit-matrix.md`](../contracts/permission-audit-matrix.md). Kolom di
bawah **MUST NOT** masuk custom logger dalam bentuk apa pun.

| Tabel | Kolom sensitif | Sebab |
| --- | --- | --- |
| `MstWorkforceProfile` | `DisplayName`, `Email`, `PhoneNumber`, `WhatsAppNumber` | Identitas dan kontak pribadi |
| `WfpSalaryAssignment` | `BaseSalary` dan seluruh kolom nominal | Nilai gaji perorangan |
| `HrdAttendanceException` | `Message`, `ResolutionNote` | Kerap memuat keadaan pribadi pegawai |
| `HrdAttendanceCorrectionRequest` | `Reason`, `EvidenceFilePath`, `EvidenceFileName`, ketiga kolom `*SummaryJson`, `FinalNote`, `OnBehalfReason` | Alasan pribadi dan berkas bukti |
| `WfpLeaveRequest` | Kolom alasan bebas | Alasan cuti kerap menyebut kesehatan atau keluarga |
| `TrxLeaveExecution` | `ExecutionSnapshotJson`, `ResultSnapshotJson`, `ErrorSummary`, `Notes`, `ReversalReason` | Memuat potongan data pegawai |
| `TrxLeaveRecall` | `RecallReason`, `Notes`, `AcknowledgementOverrideReason` | Menyebut keadaan unit dan nama orang |
| `TrxOvertimeRealization` | Kolom nominal pembayaran | Nilai pembayaran perorangan |
| `TrxShiftAssignment` | `HasLicenseConflict`, `HasClinicalPrivilegeConflict`, `ValidationResultJson`, `OverrideReason`, `Notes` | Menyiratkan keadaan lisensi dan kewenangan klinis seseorang |
| `TrxWorkflowApproverAssignment` | `ResolutionSnapshotJson` | Memuat isi keputusan penyetuju |
| `WfpPerformanceReview` | Kolom nilai dan catatan penilaian | Penilaian kinerja perorangan |
| `WfpDisciplinaryAction` | **Seluruh isi tabel** | Catatan kedisiplinan; jangkauan pembaca paling sempit di modul ini |
| `TrxPayrollRun` | Kolom nominal total | Agregat nilai payroll |
| `WfpSalaryAssignment`, `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment` | `RejectionReason` | Alasan penolakan perubahan penempatan; dapat menyebut keadaan pribadi |

---

## 6. Traceability

| Isi kamus data | Requirement / Decision | Bukti | Acceptance test |
| --- | --- | --- | --- |
| Tiga kolom klasifikasi pada `HrdAttendanceException` | `HRD-DEC-022` s.d. `HRD-DEC-025` | `02-backend-architecture.md` §7.1 | `AT-HRD-B1-*` pada `../testing/acceptance-test-matrix.md` |
| Nilai `OutOfScheduleWork` pada `ExceptionType` | `HRD-DEC-023` | `../contracts/state-transition-matrix.md` | `AT-HRD-B1-*` |
| Empat kolom pengajuan atas nama | `HRD-DEC-028` | `flowcharts/koreksi-kehadiran.md` | `AT-HRD-B1-*` |
| Dua kolom pembalikan pelaksanaan cuti | `HRD-DEC-027` | `../contracts/state-transition-matrix.md` | `AT-HRD-B2-*` |
| Tiga kolom penggantian pengakuan penarikan cuti | `HRD-DEC-029` | `flowcharts/cuti.md` | `AT-HRD-B2-*` |
| Empat kolom pengingat dan eskalasi | `HRD-DEC-030` | `flowcharts/kotak-masuk-persetujuan.md` | `AT-HRD-A7-*` |
| Index resolusi jadwal harian | `HRD-DEC-026` | `02-backend-architecture.md` §7.2 | `AT-HRD-B4-*` |
| Unique constraint `TrxShiftAssignment` yang ditahan | `HRD-Q-05` | `01-prerequisite-readiness.md` `HRD-PRE-003` | **Belum dapat diuji.** `BLOCKED` |
| Tabel `CredentialingManagement` tidak disentuh | `S-C1` `BLOCKED`, `HRD-Q-08` | `MODULE-STATUS.md` §3 `HRD-BLK-001` | Tidak berlaku |
| Tabel `OccupationalHealthManagement` tidak disentuh | `S-C6` `BLOCKED`, `HRD-DEC-010` `draft` | `MODULE-STATUS.md` §3 `HRD-BLK-002` | Tidak berlaku |
| Enam domain tanpa controller tidak disentuh | `HRD-Q-05`, `HRD-DEC-004` | `MODULE-STATUS.md` §3 `HRD-BLK-004` | Tidak berlaku |
| Batas kolom payroll | `HRD-DEC-009`, `HRD-Q-10`, `HRD-Q-11` | `../contracts/integration-contract.md` | `AT-HRD-B5-*` |

---

## 7. Yang sengaja tidak dikerjakan pada dokumen ini

- **Tidak ada migration yang dibuat maupun dijalankan.** Seluruh DDL di sini adalah dokumentasi
  bentuk.
- **Tidak ada perubahan pada file model, configuration, maupun `ApplicationDbContext`.** Source
  aplikasi tetap read-only sepanjang fase desain.
- **Tidak ada tabel untuk kemampuan yang berstatus `BLOCKED`.** Kredensial, kewenangan klinis,
  OPPE, FPPE, kesehatan kerja staf, perencanaan tenaga kerja, rekrutmen, benefit, layanan HR, dan
  perjalanan dinas **tidak** dirancang kolomnya.
- **Tidak ada kolom yang diturunkan dari nama layar atau nama menu.** Setiap kolom di dokumen ini
  berasal dari file model yang sudah ada, atau dari keputusan `HRD-DEC-*` yang tercatat.
- **Tidak ada dokumen yang ditandai `approved`.** Approval tetap tindakan manusia.
