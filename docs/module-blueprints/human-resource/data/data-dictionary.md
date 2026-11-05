# Human Resource — Kamus Data

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `data/data-dictionary.md` |
| `contract_version` | `v6` — angka set kontrak disimpan di `blueprint-manifest.md` field `contract_versions` |
| `last_changed_in` | `v7` — bagian 8 (`S-D1`) ditambahkan 3 November 2026; bagian 1–7 tidak berubah sejak `v6` |
| Status | `draft` — **belum** `approved`. Approval adalah tindakan manusia, bukan keluaran skill |
| Owner | Technical owner (`HRD-DEC-015`), bersama pemilik basis data untuk keputusan yang merusak data |
| `approved_by` / `approved_at` | **Hanya bagian 8** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `02-backend-architecture.md` revision `2`; `00-interview-decisions.md` revision `16` (`HRD-DEC-046` s.d. `HRD-DEC-057`) |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
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

Tabel berstatus **`Baru`** pada rancangan ini adalah `HrdPersonnelActionOrder`, `HrdInitialPlacementBatch`, dan `HrdInitialPlacementBatchRow` yang lahir dari keputusan `HRD-DEC-048` dan `HRD-DEC-054`. Tabel lainnya berstatus `Diperbarui` (`EXTEND`) melalui penambahan kolom persetujuan, tautan SK, dan pengajuan atas nama pada entity yang sudah ada.

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
| `HrdPersonnelActionOrder` | **`Baru`** | Human Resource | `HRD-DEC-048`: Dokumen Peristiwa Kepegawaian (SK Mutasi/Promosi/Rotasi/Demosi/Penugasan Sementara) |
| `HrdInitialPlacementBatch` | **`Baru`** | Human Resource | `HRD-DEC-054`: Batch data awal penempatan kepegawaian 55 pegawai existing |
| `HrdInitialPlacementBatchRow` | **`Baru`** | Human Resource | `HRD-DEC-054`: Rincian baris penempatan per pegawai dalam batch data awal |
| `TrxEmployeeProfileChangeRequest` | **`Diperbarui`** | Human Resource | `HRD-DEC-052`: tambah kolom pengajuan atas nama dan bukti berkas sensitif |
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
| `PersonnelActionOrderId` | `uuid` | Tidak | — | Index | FK ke `HrdPersonnelActionOrder` | `Restrict` | Tidak | **`RENCANA`.** Tautan ke Dokumen Peristiwa SK yang melahirkan penempatan ini (`HRD-DEC-048`) |

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

### 2.8 `HrdPersonnelActionOrder` (`Baru`)

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."HrdPersonnelActionOrder"` |
| File model | `Areas/Corporate/HumanResource/WorkforceCore/Models/HrdPersonnelActionOrder.cs` |
| File configuration | `Repositories/Configurations/Corporate/HumanResource/WorkforceCore/HrdPersonnelActionOrderConfiguration.cs` |
| Alasan dibuat | `HRD-DEC-048` menetapkan bahwa setiap peristiwa kepegawaian (Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara) dicatat sebagai satu dokumen peristiwa (SK) utuh yang mengikat perubahan unit, jabatan, dan atasan |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ActionOrderNumber` | `varchar(60)` | Ya | — | Unique, terfilter `"IsDelete" = false` | — | — | Tidak | Nomor SK resmi yang dilihat pengguna |
| `WorkforceProfileId` | `uuid` | Ya | — | Index bersama `EffectiveDate` | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai yang mengalami peristiwa kepegawaian |
| `ActionCategory` | `varchar(40)` | Ya | — | Index | — | — | Tidak | Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara |
| `EffectiveDate` | `date` | Ya | — | Index bersama `OrderStatus` | — | — | Tidak | Tanggal mulai berlakunya SK (`HRD-DEC-048`) |
| `OrderStatus` | `varchar(30)` | Ya | `"Draft"` | Index | — | — | Tidak | `Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled` |
| `ReasonText` | `varchar(1500)` | Ya | — | — | — | — | **Ya** | Alasan diterbitkannya SK kepegawaian |
| `SignedDocumentPath` | `varchar(500)` | Ya | — | — | — | — | Tidak | **Wajib diisi sebelum diajukan** (`HRD-DEC-056`). Path berkas scan SK resmi bertandatangan |
| `SignedDocumentFileName` | `varchar(255)` | Ya | — | — | — | — | Tidak | Nama berkas scan SK asli |
| `TargetOrganizationUnitId` | `uuid` | Tidak | — | — | FK ke `MstOrganizationUnit` | `Restrict` | Tidak | Unit organisasi baru (bila ada mutasi/rotasi) |
| `TargetPositionId` | `uuid` | Tidak | — | — | FK ke `MstPosition` | `Restrict` | Tidak | Jabatan baru (bila ada promosi/demosi/rotasi) |
| `TargetManagerProfileId` | `uuid` | Tidak | — | — | FK ke `MstWorkforceProfile` | `Restrict` | Tidak | Atasan langsung baru |
| `LinkedSalaryAssignmentId` | `uuid` | Tidak | — | — | FK ke `WfpSalaryAssignment` | `Restrict` | Tidak | Tautan ke penetapan gaji penyesuaian (jika ada) |
| `WorkflowDefinitionId` | `uuid` | Tidak | — | — | FK ke `MstWorkflowDefinition` | `Restrict` | Tidak | Definisi alur persetujuan T8 |
| `WorkflowInstanceId` | `uuid` | Tidak | — | Unique terfilter bukan-null | FK ke `TrxWorkflowInstance` | `Restrict` | Tidak | Satu SK paling banyak satu instance workflow |
| `SubmittedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun HR Admin pembuat SK |
| `SubmittedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu SK diajukan |
| `ApprovedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Penyetuju (`APPROVER_MUST_DIFFER_FROM_CREATOR`) |
| `ApprovedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu SK disetujui |
| `RejectedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun penolak |
| `RejectedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu SK ditolak |
| `RejectionReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | Alasan penolakan SK wajib diisi |
| `CancellationRequestedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Akun pengaju pembatalan SK (`HRD-DEC-057`) |
| `CancellationRequestedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu pengajuan batal diajukan |
| `CancellationReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | Alasan pembatalan SK terjadwal |
| `ExecutedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu penempatan diterapkan ke sistem |

### 2.9 `TrxEmployeeProfileChangeRequest` (`Diperbarui`)

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."TrxEmployeeProfileChangeRequest"` |
| File model | `Areas/Corporate/HumanResource/WorkforceCore/Models/TrxEmployeeProfileChangeRequest.cs` |
| Alasan berubah | `HRD-DEC-052` mengizinkan pengajuan permohonan atas nama pegawai oleh HR dan mewajibkan berkas bukti untuk perubahan data sensitif |

| Kolom Tambahan | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `IsOnBehalf` | `boolean` | Ya | `false` | Index bersama `RequestStatus` | — | — | Tidak | **`RENCANA`.** Menandai pengajuan yang dibuat HR atas nama pegawai |
| `InitiatedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **`RENCANA`.** Akun HR pembuat pengajuan atas nama |
| `OnBehalfReason` | `varchar(500)` | Tidak | — | — | — | — | **Ya** | **`RENCANA`.** Alasan mengapa HR yang mengajukan |
| `OnBehalfNotifiedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Waktu pegawai diberi notifikasi permohonan |
| `EvidenceFilePath` | `varchar(500)` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Path berkas bukti resmi (wajib untuk data sensitif) |
| `EvidenceFileName` | `varchar(255)` | Tidak | — | — | — | — | Tidak | **`RENCANA`.** Nama berkas bukti asli |

### 2.10 `HrdInitialPlacementBatch` dan `HrdInitialPlacementBatchRow` (`Baru`)

| Aspek | Isi |
| --- | --- |
| Tabel dan schema | `public."HrdInitialPlacementBatch"` dan `public."HrdInitialPlacementBatchRow"` |
| File model | `Areas/Corporate/HumanResource/WorkforceCore/Models/HrdInitialPlacementBatch.cs` |
| Alasan dibuat | `HRD-DEC-054` mengatur pengisian data awal penempatan bagi 55 pegawai existing yang belum memiliki jabatan, atasan, dan gaji melalui satu batch tervalidasi yang disetujui sekali |

| Kolom Header | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama batch |
| `BatchNumber` | `varchar(50)` | Ya | — | Unique, terfilter | — | — | Tidak | Nomor batch data awal |
| `BatchStatus` | `varchar(30)` | Ya | `"Draft"` | Index | — | — | Tidak | `Draft`, `Validated`, `Submitted`, `Approved`, `Rejected` |
| `TotalRows` | `integer` | Ya | `0` | — | — | — | Tidak | Jumlah pegawai dalam batch |
| `ValidatedRows` | `integer` | Ya | `0` | — | — | — | Tidak | Baris lolos validasi |
| `FailedRows` | `integer` | Ya | `0` | — | — | — | Tidak | Baris gagal validasi |
| `SubmittedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | HR Admin pengunggah |
| `ApprovedByUserId` | `uuid` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | HR Manager penyetuju (`APPROVER_MUST_DIFFER_FROM_CREATOR`) |
| `ApprovedAt` | `timestamptz` | Tidak | — | — | — | — | Tidak | Waktu batch disetujui dan diterapkan |

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
| `MstSalaryGrade`, `MstSalaryStructure` | `MinimumSalary`, `MidpointSalary`, `MaximumSalary`, `DefaultBaseSalary`, `MinimumBaseSalary`, `MaximumBaseSalary` | Nilai kebijakan gaji. `HRD-DEC-037` membatasi kewenangannya hanya pada `HR Manager` |

---

## 5.1 Master gaji — versi, tanggal berlaku, dan riwayat `HRD-DEC-037`

| Aspek | Keadaan di source | Sasaran |
| --- | --- | --- |
| Tanggal berlaku | **Sudah ada** — `MstSalaryGrade.EffectiveStartDate` dan `EffectiveEndDate`; `MstSalaryStructure` serupa | Dipertahankan |
| Versi kebijakan | Belum ada penanda versi tersendiri | Setiap perubahan menghasilkan versi baru yang dapat dirujuk |
| Audit perubahan | Hanya kolom audit `IdentityModel` bawaan | Ditambah: rujukan versi sebelumnya, rujukan versi baru, dan tanggal berlaku |
| Riwayat | Penghapusan sudah berupa penandaan `IsDelete`, bukan penghapusan baris | **Aturan lama MUST NOT dihapus** saat kebijakan berubah — termasuk tidak ditandai hapus |

**Empat faktor penentu gaji** setelah `HRD-DEC-041` dan `HRD-DEC-045`:

| # | Faktor | Padanan di source | Klasifikasi |
| ---: | --- | --- | --- |
| 1 | Golongan | `MstSalaryGrade.EmployeeGradeId`, `MstSalaryStructure.EmployeeCategoryId` | **`REUSE`** |
| 2 | Level | `MstSalaryGrade.GradeLevel` | **`REUSE`** |
| 3 | Status kerja | `MstSalaryStructure.EmploymentTypeId`, `EmployeeCategoryId` | **`REUSE`** |
| 4 | **Jenjang Pendidikan** | `WfpEducation.EducationLevel` + `IsVerified` + `IsHighestEducation` | **`REUSE`** dengan dua celah: butir hak akses verifikasi terpisah, dan kosakata jenjang terkendali |
| — | ~~**Masa Kerja**~~ | — | **Dikeluarkan dari cakupan MVP saat ini** `HRD-DEC-045`. Bukan faktor kebijakan gaji; `HRD-Q-56` `DEFERRED / NOT_APPLICABLE_TO_CURRENT_MVP` |

Istilah "masa studi" **ditarik** — `HRD-DEC-041` menggantinya dengan Jenjang Pendidikan.
`HRD-Q-55` **tertutup**. `HRD-DEC-042` yang sempat menambahkan Masa Kerja sebagai dimensi
**digantikan** `HRD-DEC-045` untuk MVP saat ini; masa kerja **bukan** faktor kebijakan gaji.

**Entity kebijakan gaji berversi belum ada.** Tidak ada tabel yang menyimpan versi kebijakan gaji
beserta kriteria per dimensi. `MstSalaryGrade` menyimpan **rentang gaji**, bukan aturan
kelayakan. Preseden bentuknya sudah ada pada `MstBenefitEligibilityRule` — lihat
`../02-backend-architecture.md` bagian 11.2.4. **Tidak ada tabel yang dibuat pada pass ini.**

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

---

## 8. Slice `S-D1` — Perencanaan Tenaga Kerja (`last_changed_in` `v7`, 3 November 2026)

Sumber desain: `02-backend-architecture.md` bagian 16. Keputusan: `HRD-DEC-058` s.d. `HRD-DEC-072`.
Seluruh tabel mewarisi `IdentityModel` (bagian 0.1); kolom auditnya tidak diulang.

Konvensi yang berlaku untuk seluruh tabel bagian ini:

| Konvensi | Isi |
| --- | --- |
| Schema dan nama | `public."Hrd<Nama>"`, tunggal PascalCase; `DbSet` jamak |
| Tanggal tanpa jam | `DateOnly` → `date` |
| Waktu kejadian | `DateTime` UTC → `timestamp with time zone` |
| Enum | `HasConversion<int>()` → `integer` |
| Uang | `decimal(18,2)` → `numeric(18,2)` |
| Jumlah orang dan porsi kerja | `decimal(9,2)` → `numeric(9,2)` |
| Token konkurensi | `RowVersion` `Guid`, diganti setiap pembaruan, `IsConcurrencyToken()` — mengikuti `TrxEmployeeBenefitEnrollment` |
| Perilaku hapus | `Restrict` untuk seluruh FK ke master dan ke aggregate lain; `Cascade` hanya dari induk aggregate ke barisnya sendiri |
| Indeks unik terfilter | Selalu menyertakan `"IsDelete" = false` |

### 8.1 Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `HrdStaffingStandard` | Baru | HR — `S-D1` | Aggregate root standar |
| `HrdStaffingStandardLine` | Baru | HR — `S-D1` | Baris jabatan standar |
| `HrdStaffingSkillMixRule` | Baru | HR — `S-D1` | Dibaca Penjadwalan (`HRD-DEC-058`) |
| `HrdStaffingWorkloadInput` | Baru | HR — `S-D1` | Angka beban manual |
| `HrdWorkforcePlanningSetting` | Baru | HR — `S-D1` | Satu baris per rumah sakit |
| `HrdPositionFormation` | Baru | HR — `S-D1` | Ditulis sistem saja |
| `HrdInitialFormationBatch` | Baru | HR — `S-D1` | — |
| `HrdInitialFormationBatchRow` | Baru | HR — `S-D1` | — |
| `HrdManpowerPlan` | Baru | HR — `S-D1` | Rencana dan addendum |
| `HrdManpowerPlanLine` | Baru | HR — `S-D1` | — |
| `HrdHeadcountRequest` | Baru | HR — `S-D1` | — |
| `HrdHeadcountRequestSeat` | Baru | HR — `S-D1` | — |
| `HrdHeadcountRequestCancellation` | Baru | HR — `S-D1` | — |
| `HrdFulfilmentPlanChange` | Baru | HR — `S-D1` | — |
| `HrdStaffingGapSnapshot` | Baru | HR — `S-D1` | Tidak dapat diubah |
| `HrdStaffingGapSnapshotLine` | Baru | HR — `S-D1` | Tidak dapat diubah |
| `WfpOrganizationAssignment`, `WfpPositionAssignment` | Sudah ada | 02 Workforce Core | Dibaca; **MUST NOT** disalin |
| `HrdPersonnelActionOrder` | Sudah ada | 02 Workforce Core | Dirujuk kursi internal |
| `WfpLeaveRequest` | Sudah ada | 08 Cuti | Dibaca |
| `WfpCompetencyAssessment` | Sudah ada | 16 Learning & Competency | Dibaca |
| `TrxResignationRequest` | Sudah ada | 20 Lifecycle | Dibaca |
| `MstSalaryGrade`, `MstPosition`, `MstOrganizationUnit`, `MstShift`, `MstCompetency`, `MstProfession`, `MstHospitalSite`, `MstApprovalMatrix` | Sudah ada | 01 Master Data HR / `BC-HR-07` | Dirujuk FK atau dibaca |
| Sepuluh kandidat `WorkforcePlanning` (`MstPositionHeadcountPlan` … `TrxWorkforceAllocation`) | Sudah ada | HR | **Tidak dipakai** `S-D1`; lihat `02-backend-architecture.md` 16.10 |

### 8.2 Tabel baru — seluruh kolom

#### `HrdStaffingStandard`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `StandardNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | Dari number series |
| `HospitalSiteId` | `Guid` | Ya | — | Index | FK `MstHospitalSite` | `Restrict` | Tidak | Rumah sakit |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | `Restrict` | Tidak | Unit pemilik standar |
| `VersionNumber` | `int` | Ya | — | Unique bersama `OrganizationUnitId` | — | — | Tidak | Naik 1 setiap versi |
| `EffectiveStartDate` | `DateOnly` | Ya | — | Index bersama `OrganizationUnitId` | — | — | Tidak | Tanggal mulai berlaku |
| `EffectiveEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Diisi sistem saat digantikan |
| `StandardStatus` | `StaffingStandardStatus` | Ya | `Draft` | Index | — | — | Tidak | Lihat state matrix bagian 11.1 |
| `Justification` | `string(1000)?` | Saat diajukan | — | — | — | — | **Ya** | Alasan perubahan; teks bebas |
| `SupersedesStandardId` | `Guid?` | Tidak | — | Index | FK `HrdStaffingStandard` | `Restrict` | Tidak | Versi yang digantikan |
| `WorkflowDefinitionId` | `Guid?` | Tidak | — | — | FK `MstWorkflowDefinition` | `Restrict` | Tidak | Definisi `T9` |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | — | — | Tidak | Instance `T9` |
| `SubmittedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengaju |
| `SubmittedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ApprovedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Penyetuju langkah terakhir |
| `ApprovedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ActivatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Saat status menjadi `Active` |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token konkurensi |

#### `HrdStaffingStandardLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `StaffingStandardId` | `Guid` | Ya | — | Unique bersama `PositionId` | FK `HrdStaffingStandard` | `Cascade` | Tidak | Induk |
| `PositionId` | `Guid` | Ya | — | Index | FK `MstPosition` | `Restrict` | Tidak | Jabatan |
| `ProfessionId` | `Guid?` | Tidak | — | — | FK `MstProfession` | `Restrict` | Tidak | Profesi bila relevan |
| `CalculationMethod` | `StaffingCalculationMethod` | Ya | — | — | — | — | Tidak | `WorkloadAnalysis` ditolak validasi |
| `MinimumHeadcount` | `decimal(9,2)?` | Untuk `FixedMinimum` | — | — | — | — | Tidak | ≥ 0 |
| `RatioStaffQuantity` | `decimal(9,2)?` | Untuk `WorkloadRatio` | — | — | — | — | Tidak | > 0; contoh 1 perawat |
| `RatioWorkloadQuantity` | `decimal(9,2)?` | Untuk `WorkloadRatio` | — | — | — | — | Tidak | > 0; contoh 1 pasien |
| `WorkloadUnit` | `string(50)?` | Untuk `WorkloadRatio` | — | — | — | — | Tidak | Contoh "pasien terisi per hari" |
| `ShiftsPerDay` | `int?` | Untuk `WorkloadRatio` dan `ServiceCoverage` | — | — | — | — | Tidak | 1–6 |
| `StaffPerShift` | `decimal(9,2)?` | Untuk `ServiceCoverage` | — | — | — | — | Tidak | > 0 |
| `ServiceHoursPerDay` | `decimal(5,2)?` | Untuk `ServiceCoverage` | — | — | — | — | Tidak | 0 < x ≤ 24; informasi jam layanan |
| `ReliefFactor` | `decimal(5,2)?` | Untuk `WorkloadRatio` dan `ServiceCoverage` | — | — | — | — | Tidak | ≥ 1,00; **tanpa** nilai bawaan sistem |
| `RoundingMethod` | `StaffingRoundingMethod` | Ya | — | — | — | — | Tidak | **Tanpa** nilai bawaan sistem |
| `CountsOutsourced` | `bool` | Ya | `false` | — | — | — | Tidak | Alih daya ikut menutup kebutuhan |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |

#### `HrdStaffingSkillMixRule`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `StaffingStandardId` | `Guid` | Ya | — | Unique bersama `ShiftId`, `CompetencyId`, `ProfessionId` | FK `HrdStaffingStandard` | `Cascade` | Tidak | Induk |
| `ShiftId` | `Guid?` | Tidak | — | — | FK `MstShift` | `Restrict` | Tidak | Kosong = seluruh shift |
| `ProfessionId` | `Guid?` | Tidak | — | — | FK `MstProfession` | `Restrict` | Tidak | — |
| `CompetencyId` | `Guid` | Ya | — | Index | FK `MstCompetency` | `Restrict` | Tidak | Kompetensi yang diwajibkan |
| `MinimumCompetencyLevel` | `string(50)?` | Tidak | — | — | — | — | Tidak | — |
| `MinimumQualifiedHeadcount` | `decimal(9,2)` | Ya | — | — | — | — | Tidak | > 0 |
| `IsMandatory` | `bool` | Ya | `true` | — | — | — | Tidak | Wajib dipenuhi setiap shift |
| `Notes` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |

#### `HrdStaffingWorkloadInput`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Unique terfilter bersama `PositionId`, `PeriodStartDate` | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `PeriodStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `PeriodEndDate` | `DateOnly` | Ya | — | — | — | — | Tidak | ≥ `PeriodStartDate` |
| `WorkloadValue` | `decimal(12,2)` | Ya | — | — | — | — | Tidak | > 0 |
| `SourceNote` | `string(500)` | Ya | — | — | — | — | Tidak | Contoh "rekap sensus harian ICU Q4 2026" |

#### `HrdWorkforcePlanningSetting`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Unique terfilter | FK `MstHospitalSite` | `Restrict` | Tidak | Satu baris per rumah sakit |
| `LongLeaveThresholdDays` | `int?` | Tidak | — | — | — | — | Tidak | Kosong = lapisan "tidak tersedia sementara" belum dapat dihitung |
| `OverdueRequestThresholdDays` | `int?` | Tidak | — | — | — | — | Tidak | Kosong = tanda "lewat batas" tidak pernah menyala |
| `ChangeReason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan perubahan terakhir |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

#### `HrdPositionFormation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Index | FK `MstHospitalSite` | `Restrict` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Unique bersama `PositionId`, `VersionNumber`; unique bersama `PositionId`, `EffectiveStartDate` | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `VersionNumber` | `int` | Ya | — | — | — | — | Tidak | 1, 2, 3, … per unit × jabatan |
| `FormationQuantity` | `decimal(9,2)` | Ya | — | — | — | — | Tidak | ≥ 0 |
| `EffectiveStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `EffectiveEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Satu-satunya kolom yang boleh diubah sesudah ditulis |
| `SourceType` | `PositionFormationSourceType` | Ya | — | — | — | — | Tidak | — |
| `SourceDocumentId` | `Guid` | Ya | — | Index | — | — | Tidak | Id dokumen sumber; tanpa FK karena sumbernya lima tabel berbeda |
| `SourceDocumentNumber` | `string(50)` | Ya | — | — | — | — | Tidak | Nomor dokumen sumber untuk tampilan riwayat |
| `ApprovedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | Penyetuju terakhir dokumen sumber |
| `ApprovedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `PreviousFormationId` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | — |

#### `HrdInitialFormationBatch`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BatchNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Index | FK `MstHospitalSite` | `Restrict` | Tidak | — |
| `SourceDocumentReference` | `string(200)` | Ya | — | — | — | — | Tidak | Contoh "SK Direktur No. 12/2026" |
| `SourceDocumentPath` | `string(500)?` | Saat diajukan | — | — | — | — | Tidak | Lokasi scan; disimpan lewat pola `WorkflowFileStorageService` |
| `SourceDocumentFileName` | `string(255)?` | Saat diajukan | — | — | — | — | Tidak | — |
| `BatchStatus` | `InitialFormationBatchStatus` | Ya | `Draft` | Index | — | — | Tidak | — |
| `TotalRows` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `ValidRows` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `InvalidRows` | `int` | Ya | `0` | — | — | — | Tidak | Harus 0 sebelum diajukan |
| `Notes` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `WorkflowDefinitionId` | `Guid?` | Tidak | — | — | FK `MstWorkflowDefinition` | `Restrict` | Tidak | `T10` |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | — | — | Tidak | — |
| `SubmittedByUserId`, `SubmittedAt`, `ApprovedByUserId`, `ApprovedAt`, `AppliedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | Jejak siklus |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

#### `HrdInitialFormationBatchRow`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BatchId` | `Guid` | Ya | — | Unique bersama `OrganizationUnitId`, `PositionId` | FK `HrdInitialFormationBatch` | `Cascade` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Index bersama `PositionId` | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `FormationQuantity` | `decimal(9,2)` | Ya | — | — | — | — | Tidak | ≥ 0 |
| `EffectiveStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Sesuai dokumen sumber |
| `IsValid` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `ValidationMessage` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `CreatedFormationId` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | Diisi saat `Applied` |

#### `HrdManpowerPlan`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PlanNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | — |
| `PlanKind` | `ManpowerPlanKind` | Ya | — | — | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Unique terfilter bersama `PlanYear` untuk `AnnualPlan` yang tidak `Rejected`/`Cancelled` | FK `MstHospitalSite` | `Restrict` | Tidak | Satu rencana tahunan per rumah sakit per tahun |
| `PlanYear` | `int` | Ya | — | — | — | — | Tidak | — |
| `PeriodStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `PeriodEndDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `ParentPlanId` | `Guid?` | Untuk `Addendum` | — | Unique bersama `AddendumSequence` | FK `HrdManpowerPlan` | `Restrict` | Tidak | Rencana induk |
| `AddendumSequence` | `int?` | Untuk `Addendum` | — | — | — | — | Tidak | 01, 02, … |
| `ReferenceDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal acuan gap |
| `PlanStatus` | `ManpowerPlanStatus` | Ya | `Draft` | Index | — | — | Tidak | — |
| `Justification` | `string(2000)?` | Saat diajukan | — | — | — | — | **Ya** | Teks bebas |
| `CurrencyCode` | `string(3)` | Ya | `IDR` | — | — | — | Tidak | — |
| `TotalEstimatedCost` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | Biaya; hanya untuk `WorkforcePlanningCost : Read` |
| `FinanceAdjustedTotal` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | Angka koreksi Finance |
| `FinanceAdjustmentReason` | `string(1000)?` | Bila koreksi | — | — | — | — | **Ya** | — |
| `FinanceAdjustedByUserId`, `FinanceAdjustedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `WorkflowDefinitionId`, `WorkflowInstanceId` | `Guid?` | Tidak | — | Index pada instance | FK definisi `Restrict` | — | Tidak | `T10` |
| `SubmittedByUserId`, `SubmittedAt`, `ApprovedByUserId`, `ApprovedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

#### `HrdManpowerPlanLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PlanId` | `Guid` | Ya | — | Unique bersama `OrganizationUnitId`, `PositionId` | FK `HrdManpowerPlan` | `Cascade` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `SalaryGradeId` | `Guid` | Ya | — | — | FK `MstSalaryGrade` | `Restrict` | Tidak | Dasar biaya (`GRADE_REQUIRED_PER_LINE`) |
| `CurrentFormationQuantity` | `decimal(9,2)` | Ya | `0` | — | — | — | Tidak | Potret saat hitung ulang |
| `RecommendedQuantity` | `decimal(9,2)?` | Tidak | — | — | — | — | Tidak | Kosong bila standar atau angka beban belum ada |
| `RecommendationBreakdown` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Rincian hitung |
| `ProposedFormationQuantity` | `decimal(9,2)` | Ya | — | — | — | — | Tidak | ≥ 0 |
| `ChangeQuantity` | `decimal(9,2)` | Ya | `0` | — | — | — | Tidak | Usulan − saat ini; dihitung sistem |
| `DeviationReason` | `string(1000)?` | Bila usulan ≠ rekomendasi | — | — | — | — | **Ya** | Teks bebas |
| `EffectiveStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Rencana: awal periode; addendum: per baris |
| `MonthlyRateSnapshot` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | Gaji tengah grade pada tanggal berlaku |
| `MonthsInPeriod` | `int` | Ya | — | — | — | — | Tidak | Bulan dari tanggal berlaku sampai akhir periode |
| `EstimatedCost` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | Usulan × tarif × bulan |
| `IncrementalCost` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | Perubahan × tarif × bulan |
| `CostStatus` | `CostCalculationStatus` | Ya | — | — | — | — | Tidak | — |
| `CreatedFormationId` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | Diisi saat disetujui |

#### `HrdHeadcountRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `RequestNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | — | FK `MstHospitalSite` | `Restrict` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | Index bersama `PositionId`, `RequestStatus` | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `SalaryGradeId` | `Guid` | Ya | — | — | FK `MstSalaryGrade` | `Restrict` | Tidak | Dasar biaya |
| `RequestType` | `HeadcountRequestType` | Ya | — | — | — | — | Tidak | — |
| `RequestStatus` | `HeadcountRequestStatus` | Ya | `Draft` | — | — | — | Tidak | — |
| `RequiredStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `TemporaryEndDate` | `DateOnly?` | Untuk `Temporary` | — | — | — | — | Tidak | — |
| `TemporaryReasonWorkforceProfileId` | `Guid?` | Untuk `Temporary` | — | — | FK `MstWorkforceProfile` | `Restrict` | Tidak | Orang yang tidak tersedia |
| `Justification` | `string(2000)?` | Untuk `AdditionalBeyondFormation` | — | — | — | — | **Ya** | Teks bebas |
| `RequestedQuantity` | `decimal(9,2)` | Ya | `0` | — | — | — | Tidak | Jumlah `SeatFte`; dihitung sistem |
| `ExternalSeatCount` | `int` | Ya | `0` | — | — | — | Tidak | Rencana pemenuhan luar |
| `InternalSeatCount` | `int` | Ya | `0` | — | — | — | Tidak | Rencana pemenuhan internal |
| `FormationVersionIdAtSubmission` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | Formasi saat diajukan |
| `RemainingSeatsAtSubmission` | `decimal(9,2)?` | Tidak | — | — | — | — | Tidak | — |
| `EstimatedMonthlyRate` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | — |
| `EstimatedCost` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | — |
| `CostStatus` | `CostCalculationStatus` | Ya | — | — | — | — | Tidak | — |
| `FinanceAdjustedCost` | `decimal(18,2)?` | Tidak | — | — | — | — | **Ya** | `T12` saja |
| `FinanceAdjustmentReason` | `string(1000)?` | Bila koreksi | — | — | — | — | **Ya** | — |
| `FinanceAdjustedByUserId`, `FinanceAdjustedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `IsOverFormationAfterAddendum` | `bool` | Ya | `false` | Index | — | — | Tidak | `PENDING_REQUESTS_FLAGGED` |
| `IsOverdue` | `bool` | Ya | `false` | Index | — | — | Tidak | Tanda "lewat batas" |
| `CreatedFormationId` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | Untuk `T12` |
| `WorkflowDefinitionId`, `WorkflowInstanceId` | `Guid?` | Tidak | — | Index pada instance | FK definisi `Restrict` | — | Tidak | `T11` atau `T12` |
| `SubmittedByUserId`, `SubmittedAt`, `ApprovedByUserId`, `ApprovedAt`, `ReadyForRecruitmentAt`, `WithdrawnAt`, `ClosedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

#### `HrdHeadcountRequestSeat`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HeadcountRequestId` | `Guid` | Ya | — | Unique bersama `SeatNumber` | FK `HrdHeadcountRequest` | `Cascade` | Tidak | — |
| `SeatNumber` | `int` | Ya | — | — | — | — | Tidak | 1, 2, … |
| `SeatFte` | `decimal(4,2)` | Ya | `1.00` | — | — | — | Tidak | 0,10–1,00 |
| `FulfilmentSource` | `HeadcountFulfilmentSource` | Ya | — | — | — | — | Tidak | — |
| `SeatStatus` | `HeadcountSeatStatus` | Ya | `Open` | Index | — | — | Tidak | — |
| `ReplacedWorkforceProfileId` | `Guid?` | Untuk `Replacement` | — | — | FK `MstWorkforceProfile` | `Restrict` | Tidak | Orang yang digantikan |
| `LinkedPositionAssignmentId` | `Guid?` | Tidak | — | **Unique terfilter**: tidak kosong dan `SeatStatus` ≠ `Cancelled` | FK `WfpPositionAssignment` | `Restrict` | Tidak | `ONE_PLACEMENT_ONE_SEAT` |
| `LinkedPersonnelActionOrderId` | `Guid?` | Tidak | — | **Unique terfilter** seperti di atas | FK `HrdPersonnelActionOrder` | `Restrict` | Tidak | Kursi internal sebelum penempatan berlaku |
| `LinkedByUserId`, `LinkedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `FulfilledAt` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal mulai penempatan |
| `CancellationId` | `Guid?` | Tidak | — | Index | FK `HrdHeadcountRequestCancellation` | `Restrict` | Tidak | — |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |

#### `HrdHeadcountRequestCancellation`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `CancellationNumber` | `string(50)` | Ya | — | Unique | — | — | Tidak | — |
| `HeadcountRequestId` | `Guid` | Ya | — | Index | FK `HrdHeadcountRequest` | `Restrict` | Tidak | — |
| `CancellationStatus` | `HeadcountCancellationStatus` | Ya | `Submitted` | — | — | — | Tidak | — |
| `Reason` | `string(1000)` | Ya | — | — | — | — | **Ya** | Teks bebas |
| `RequestedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | — |
| `DecidedByUserId`, `DecidedAt` | `Guid?` / `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CreatedFormationId` | `Guid?` | Tidak | — | — | FK `HrdPositionFormation` | `Restrict` | Tidak | Bila kursi berasal dari `T12` |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | — | — | — | Tidak | `T13` |

#### `HrdFulfilmentPlanChange`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HeadcountRequestId` | `Guid` | Ya | — | Index | FK `HrdHeadcountRequest` | `Restrict` | Tidak | — |
| `SeatId` | `Guid` | Ya | — | — | FK `HrdHeadcountRequestSeat` | `Restrict` | Tidak | — |
| `FromSource` | `HeadcountFulfilmentSource` | Ya | — | — | — | — | Tidak | — |
| `ToSource` | `HeadcountFulfilmentSource` | Ya | — | — | — | — | Tidak | ≠ `FromSource` |
| `Reason` | `string(1000)` | Ya | — | — | — | — | **Ya** | Teks bebas |
| `ChangedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | HR Manager |
| `ChangedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |

#### `HrdStaffingGapSnapshot`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SnapshotType` | `StaffingGapSnapshotType` | Ya | — | — | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Unique terfilter bersama `PeriodMonth` untuk `Monthly` | FK `MstHospitalSite` | `Restrict` | Tidak | Satu potret bulanan per bulan |
| `ReferenceDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `SourceDocumentType` | `PositionFormationSourceType?` | Untuk `Submission` | — | Index bersama `SourceDocumentId` | — | — | Tidak | Dipakai `ManpowerPlan`, `PlanAddendum`, `HeadcountRequest` |
| `SourceDocumentId` | `Guid?` | Untuk `Submission` | — | — | — | — | Tidak | — |
| `PeriodMonth` | `DateOnly?` | Untuk `Monthly` dan `Reconstructed` | — | — | — | — | Tidak | Hari pertama bulan |
| `TakenAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `ReconstructedAt` | `DateTime?` | Untuk `Reconstructed` | — | — | — | — | Tidak | `MISSED_SNAPSHOT_NOT_FAKED` |

#### `HrdStaffingGapSnapshotLine`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SnapshotId` | `Guid` | Ya | — | Unique bersama `OrganizationUnitId`, `PositionId` | FK `HrdStaffingGapSnapshot` | `Cascade` | Tidak | — |
| `OrganizationUnitId` | `Guid` | Ya | — | — | FK `MstOrganizationUnit` | `Restrict` | Tidak | — |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | — |
| `FormationQuantity` | `decimal(9,2)?` | Tidak | — | — | — | — | Tidak | Kosong bila belum ada formasi |
| `FilledFte`, `ToBeVacatedFte`, `TemporarilyUnavailableFte`, `NotYetCompetentFte`, `OutsourcedFte` | `decimal(9,2)?` | Tidak | — | — | — | — | Tidak | Kosong = data belum tersedia, **bukan** 0 |
| `ConcurrentAssignmentCount` | `int?` | Tidak | — | — | — | — | Tidak | Rangkap/Plt sebagai informasi |
| `PermanentGap`, `TemporaryGap`, `CompetencyGap` | `decimal(9,2)?` | Tidak | — | — | — | — | Tidak | — |
| `PlacementDataComplete`, `LeaveDataComplete`, `CompetencyDataComplete`, `WorkforceTypeDataComplete` | `bool` | Ya | `false` | — | — | — | Tidak | `SNAPSHOT_RECORDS_COMPLETENESS` |
| `ShortageSummaryCount` | `int?` | Tidak | — | — | — | — | Tidak | Dari Penjadwalan; kosong sampai tersedia |

### 8.3 Tabel `Sudah ada` — kolom kunci yang dibaca `S-D1`

| Tabel | Kolom yang dibaca | Aturan yang memakainya | Model |
| --- | --- | --- | --- |
| `WfpOrganizationAssignment` | `WorkforceProfileId`, `OrganizationUnitId`, `IsPrimary`, `AssignmentType`, `EffectiveStartDate`, `EffectiveEndDate` | Lapisan terisi, rangkap/Plt | `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpOrganizationAssignment.cs` |
| `WfpPositionAssignment` | `Id`, `WorkforceProfileId`, `OrganizationUnitId`, `PositionId`, `IsPrimary`, `AssignmentType`, `EffectiveStartDate`, `EffectiveEndDate`, `PersonnelActionOrderId` | Terisi; tautan kursi; kursi internal terpenuhi | `.../WorkforceCore/Models/WfpPositionAssignment.cs` |
| `HrdPersonnelActionOrder` | `Id`, `OrderStatus`, `EffectiveDate`, `TargetOrganizationUnitId`, `TargetPositionId` | Kursi internal; akan kosong (SK pindah keluar) | `.../WorkforceCore/Models/HrdPersonnelActionOrder.cs` |
| `WfpLeaveRequest` | `WorkforceProfileId`, `StartDate`, `EndDate`, `LeaveRequestStatus` | Tidak tersedia sementara | `.../LeaveManagement/Models/WfpLeaveRequest.cs` |
| `WfpCompetencyAssessment` | `WorkforceProfileId`, `CompetencyId`, `AssessmentDate`, `ExpiredDate`, `VerifiedAt` | Belum memenuhi kompetensi | `.../LearningAndDevelopment/Models/WfpCompetencyAssessment.cs` |
| `TrxResignationRequest` | `WorkforceProfileId`, `ProposedLastWorkingDate`, `RequestStatus` | Akan kosong | `.../LifecycleManagement/Models/TrxResignationRequest.cs` |
| `MstSalaryGrade` | `Id`, `MidpointSalary`, `EffectiveStartDate`, `EffectiveEndDate`, `IsActive` | Estimasi biaya | `.../MasterData/PayrollAndBenefit/Models/MstSalaryGrade.cs` |
| `MstOrganizationUnit` | `Id`, `ParentOrganizationUnitId`, `UnitType` | Penelusuran direktorat (`HRD-DEC-072`) | `.../MasterData/Organization/Models/MstOrganizationUnit.cs` |
| `MstApprovalMatrix` | `WorkflowStepId`, `OrganizationUnitId`, `ApproverSourceType`, `ApproverPositionId`, `IsFallback` | Langkah Direktur | `.../MasterData/Workflow/Models/MstApprovalMatrix.cs` |

### 8.4 Skema tabel dalam bentuk DDL

> **Peringatan.** Sama seperti bagian 4.1: DDL di bawah adalah **dokumentasi bentuk tabel**, bukan
> skrip yang dijalankan. Configuration EF Core untuk tabel ini **belum ada**; bentuk ini menjadi acuan
> saat configuration ditulis, dan configuration itulah yang kemudian menjadi sumber kebenaran.
> Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."HrdStaffingStandard" (
    "Id" uuid NOT NULL, "StandardNumber" varchar(50) NOT NULL,
    "HospitalSiteId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL,
    "VersionNumber" integer NOT NULL, "EffectiveStartDate" date NOT NULL, "EffectiveEndDate" date,
    "StandardStatus" integer NOT NULL,
    "Justification" varchar(1000),                       -- SENSITIF
    "SupersedesStandardId" uuid, "WorkflowDefinitionId" uuid, "WorkflowInstanceId" uuid,
    "SubmittedByUserId" uuid, "SubmittedAt" timestamptz, "ApprovedByUserId" uuid,
    "ApprovedAt" timestamptz, "ActivatedAt" timestamptz, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdStaffingStandard" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdStaffingStandard_MstOrganizationUnit" FOREIGN KEY ("OrganizationUnitId")
        REFERENCES public."MstOrganizationUnit" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdStaffingStandard_Supersedes" FOREIGN KEY ("SupersedesStandardId")
        REFERENCES public."HrdStaffingStandard" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdStaffingStandard_StandardNumber" ON public."HrdStaffingStandard" ("StandardNumber");
CREATE UNIQUE INDEX "IX_HrdStaffingStandard_Unit_Version" ON public."HrdStaffingStandard" ("OrganizationUnitId", "VersionNumber");

CREATE TABLE public."HrdStaffingStandardLine" (
    "Id" uuid NOT NULL, "StaffingStandardId" uuid NOT NULL, "PositionId" uuid NOT NULL,
    "ProfessionId" uuid, "CalculationMethod" integer NOT NULL,
    "MinimumHeadcount" numeric(9,2), "RatioStaffQuantity" numeric(9,2), "RatioWorkloadQuantity" numeric(9,2),
    "WorkloadUnit" varchar(50), "ShiftsPerDay" integer, "StaffPerShift" numeric(9,2),
    "ServiceHoursPerDay" numeric(5,2), "ReliefFactor" numeric(5,2), "RoundingMethod" integer NOT NULL,
    "CountsOutsourced" boolean NOT NULL DEFAULT false, "Notes" varchar(500),
    CONSTRAINT "PK_HrdStaffingStandardLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdStaffingStandardLine_Standard" FOREIGN KEY ("StaffingStandardId")
        REFERENCES public."HrdStaffingStandard" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_HrdStaffingStandardLine_MstPosition" FOREIGN KEY ("PositionId")
        REFERENCES public."MstPosition" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdStaffingStandardLine_Standard_Position" ON public."HrdStaffingStandardLine" ("StaffingStandardId", "PositionId");

CREATE TABLE public."HrdStaffingSkillMixRule" (
    "Id" uuid NOT NULL, "StaffingStandardId" uuid NOT NULL, "ShiftId" uuid, "ProfessionId" uuid,
    "CompetencyId" uuid NOT NULL, "MinimumCompetencyLevel" varchar(50),
    "MinimumQualifiedHeadcount" numeric(9,2) NOT NULL, "IsMandatory" boolean NOT NULL DEFAULT true,
    "Notes" varchar(500),
    CONSTRAINT "PK_HrdStaffingSkillMixRule" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdStaffingSkillMixRule_Standard" FOREIGN KEY ("StaffingStandardId")
        REFERENCES public."HrdStaffingStandard" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_HrdStaffingSkillMixRule_MstCompetency" FOREIGN KEY ("CompetencyId")
        REFERENCES public."MstCompetency" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."HrdStaffingWorkloadInput" (
    "Id" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL, "PositionId" uuid NOT NULL,
    "PeriodStartDate" date NOT NULL, "PeriodEndDate" date NOT NULL,
    "WorkloadValue" numeric(12,2) NOT NULL, "SourceNote" varchar(500) NOT NULL,
    CONSTRAINT "PK_HrdStaffingWorkloadInput" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdStaffingWorkloadInput_Unit_Position_Period" ON public."HrdStaffingWorkloadInput"
    ("OrganizationUnitId", "PositionId", "PeriodStartDate") WHERE "IsDelete" = false;

CREATE TABLE public."HrdWorkforcePlanningSetting" (
    "Id" uuid NOT NULL, "HospitalSiteId" uuid NOT NULL, "LongLeaveThresholdDays" integer,
    "OverdueRequestThresholdDays" integer, "ChangeReason" varchar(500) NOT NULL, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdWorkforcePlanningSetting" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdWorkforcePlanningSetting_Site" ON public."HrdWorkforcePlanningSetting" ("HospitalSiteId") WHERE "IsDelete" = false;

CREATE TABLE public."HrdPositionFormation" (
    "Id" uuid NOT NULL, "HospitalSiteId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL,
    "PositionId" uuid NOT NULL, "VersionNumber" integer NOT NULL, "FormationQuantity" numeric(9,2) NOT NULL,
    "EffectiveStartDate" date NOT NULL, "EffectiveEndDate" date, "SourceType" integer NOT NULL,
    "SourceDocumentId" uuid NOT NULL, "SourceDocumentNumber" varchar(50) NOT NULL,
    "ApprovedByUserId" uuid NOT NULL, "ApprovedAt" timestamptz NOT NULL, "PreviousFormationId" uuid,
    CONSTRAINT "PK_HrdPositionFormation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdPositionFormation_Previous" FOREIGN KEY ("PreviousFormationId")
        REFERENCES public."HrdPositionFormation" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdPositionFormation_Unit_Position_Version" ON public."HrdPositionFormation" ("OrganizationUnitId", "PositionId", "VersionNumber");
CREATE UNIQUE INDEX "IX_HrdPositionFormation_Unit_Position_Start" ON public."HrdPositionFormation" ("OrganizationUnitId", "PositionId", "EffectiveStartDate");

CREATE TABLE public."HrdInitialFormationBatch" (
    "Id" uuid NOT NULL, "BatchNumber" varchar(50) NOT NULL, "HospitalSiteId" uuid NOT NULL,
    "SourceDocumentReference" varchar(200) NOT NULL, "SourceDocumentPath" varchar(500),
    "SourceDocumentFileName" varchar(255), "BatchStatus" integer NOT NULL,
    "TotalRows" integer NOT NULL DEFAULT 0, "ValidRows" integer NOT NULL DEFAULT 0,
    "InvalidRows" integer NOT NULL DEFAULT 0, "Notes" varchar(1000),
    "WorkflowDefinitionId" uuid, "WorkflowInstanceId" uuid, "SubmittedByUserId" uuid,
    "SubmittedAt" timestamptz, "ApprovedByUserId" uuid, "ApprovedAt" timestamptz,
    "AppliedAt" timestamptz, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdInitialFormationBatch" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdInitialFormationBatch_BatchNumber" ON public."HrdInitialFormationBatch" ("BatchNumber");

CREATE TABLE public."HrdInitialFormationBatchRow" (
    "Id" uuid NOT NULL, "BatchId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL,
    "PositionId" uuid NOT NULL, "FormationQuantity" numeric(9,2) NOT NULL, "EffectiveStartDate" date NOT NULL,
    "IsValid" boolean NOT NULL DEFAULT false, "ValidationMessage" varchar(500), "CreatedFormationId" uuid,
    CONSTRAINT "PK_HrdInitialFormationBatchRow" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdInitialFormationBatchRow_Batch" FOREIGN KEY ("BatchId")
        REFERENCES public."HrdInitialFormationBatch" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_HrdInitialFormationBatchRow_Batch_Unit_Position" ON public."HrdInitialFormationBatchRow" ("BatchId", "OrganizationUnitId", "PositionId");

CREATE TABLE public."HrdManpowerPlan" (
    "Id" uuid NOT NULL, "PlanNumber" varchar(50) NOT NULL, "PlanKind" integer NOT NULL,
    "HospitalSiteId" uuid NOT NULL, "PlanYear" integer NOT NULL, "PeriodStartDate" date NOT NULL,
    "PeriodEndDate" date NOT NULL, "ParentPlanId" uuid, "AddendumSequence" integer,
    "ReferenceDate" date NOT NULL, "PlanStatus" integer NOT NULL,
    "Justification" varchar(2000),                        -- SENSITIF
    "CurrencyCode" varchar(3) NOT NULL DEFAULT 'IDR',
    "TotalEstimatedCost" numeric(18,2),                   -- SENSITIF
    "FinanceAdjustedTotal" numeric(18,2),                 -- SENSITIF
    "FinanceAdjustmentReason" varchar(1000),              -- SENSITIF
    "FinanceAdjustedByUserId" uuid, "FinanceAdjustedAt" timestamptz,
    "WorkflowDefinitionId" uuid, "WorkflowInstanceId" uuid, "SubmittedByUserId" uuid,
    "SubmittedAt" timestamptz, "ApprovedByUserId" uuid, "ApprovedAt" timestamptz, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdManpowerPlan" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdManpowerPlan_Parent" FOREIGN KEY ("ParentPlanId")
        REFERENCES public."HrdManpowerPlan" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdManpowerPlan_PlanNumber" ON public."HrdManpowerPlan" ("PlanNumber");
CREATE UNIQUE INDEX "IX_HrdManpowerPlan_Site_Year_Annual" ON public."HrdManpowerPlan" ("HospitalSiteId", "PlanYear")
    WHERE "PlanKind" = 1 AND "PlanStatus" NOT IN (4, 6) AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_HrdManpowerPlan_Parent_Sequence" ON public."HrdManpowerPlan" ("ParentPlanId", "AddendumSequence")
    WHERE "ParentPlanId" IS NOT NULL;

CREATE TABLE public."HrdManpowerPlanLine" (
    "Id" uuid NOT NULL, "PlanId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL, "PositionId" uuid NOT NULL,
    "SalaryGradeId" uuid NOT NULL, "CurrentFormationQuantity" numeric(9,2) NOT NULL DEFAULT 0,
    "RecommendedQuantity" numeric(9,2), "RecommendationBreakdown" varchar(1000),
    "ProposedFormationQuantity" numeric(9,2) NOT NULL, "ChangeQuantity" numeric(9,2) NOT NULL DEFAULT 0,
    "DeviationReason" varchar(1000),                      -- SENSITIF
    "EffectiveStartDate" date NOT NULL,
    "MonthlyRateSnapshot" numeric(18,2),                  -- SENSITIF
    "MonthsInPeriod" integer NOT NULL,
    "EstimatedCost" numeric(18,2),                        -- SENSITIF
    "IncrementalCost" numeric(18,2),                      -- SENSITIF
    "CostStatus" integer NOT NULL, "CreatedFormationId" uuid,
    CONSTRAINT "PK_HrdManpowerPlanLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdManpowerPlanLine_Plan" FOREIGN KEY ("PlanId")
        REFERENCES public."HrdManpowerPlan" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_HrdManpowerPlanLine_MstSalaryGrade" FOREIGN KEY ("SalaryGradeId")
        REFERENCES public."MstSalaryGrade" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdManpowerPlanLine_Plan_Unit_Position" ON public."HrdManpowerPlanLine" ("PlanId", "OrganizationUnitId", "PositionId");

CREATE TABLE public."HrdHeadcountRequest" (
    "Id" uuid NOT NULL, "RequestNumber" varchar(50) NOT NULL, "HospitalSiteId" uuid NOT NULL,
    "OrganizationUnitId" uuid NOT NULL, "PositionId" uuid NOT NULL, "SalaryGradeId" uuid NOT NULL,
    "RequestType" integer NOT NULL, "RequestStatus" integer NOT NULL, "RequiredStartDate" date NOT NULL,
    "TemporaryEndDate" date, "TemporaryReasonWorkforceProfileId" uuid,
    "Justification" varchar(2000),                        -- SENSITIF
    "RequestedQuantity" numeric(9,2) NOT NULL DEFAULT 0,
    "ExternalSeatCount" integer NOT NULL DEFAULT 0, "InternalSeatCount" integer NOT NULL DEFAULT 0,
    "FormationVersionIdAtSubmission" uuid, "RemainingSeatsAtSubmission" numeric(9,2),
    "EstimatedMonthlyRate" numeric(18,2),                 -- SENSITIF
    "EstimatedCost" numeric(18,2),                        -- SENSITIF
    "CostStatus" integer NOT NULL,
    "FinanceAdjustedCost" numeric(18,2),                  -- SENSITIF
    "FinanceAdjustmentReason" varchar(1000),              -- SENSITIF
    "FinanceAdjustedByUserId" uuid, "FinanceAdjustedAt" timestamptz,
    "IsOverFormationAfterAddendum" boolean NOT NULL DEFAULT false, "IsOverdue" boolean NOT NULL DEFAULT false,
    "CreatedFormationId" uuid, "WorkflowDefinitionId" uuid, "WorkflowInstanceId" uuid,
    "SubmittedByUserId" uuid, "SubmittedAt" timestamptz, "ApprovedByUserId" uuid, "ApprovedAt" timestamptz,
    "ReadyForRecruitmentAt" timestamptz, "WithdrawnAt" timestamptz, "ClosedAt" timestamptz,
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdHeadcountRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdHeadcountRequest_MstPosition" FOREIGN KEY ("PositionId")
        REFERENCES public."MstPosition" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdHeadcountRequest_RequestNumber" ON public."HrdHeadcountRequest" ("RequestNumber");
CREATE INDEX "IX_HrdHeadcountRequest_Unit_Position_Status" ON public."HrdHeadcountRequest" ("OrganizationUnitId", "PositionId", "RequestStatus");

CREATE TABLE public."HrdHeadcountRequestSeat" (
    "Id" uuid NOT NULL, "HeadcountRequestId" uuid NOT NULL, "SeatNumber" integer NOT NULL,
    "SeatFte" numeric(4,2) NOT NULL DEFAULT 1.00, "FulfilmentSource" integer NOT NULL,
    "SeatStatus" integer NOT NULL, "ReplacedWorkforceProfileId" uuid,
    "LinkedPositionAssignmentId" uuid, "LinkedPersonnelActionOrderId" uuid,
    "LinkedByUserId" uuid, "LinkedAt" timestamptz, "FulfilledAt" date,
    "CancellationId" uuid, "CancelledAt" timestamptz,
    CONSTRAINT "PK_HrdHeadcountRequestSeat" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdHeadcountRequestSeat_Request" FOREIGN KEY ("HeadcountRequestId")
        REFERENCES public."HrdHeadcountRequest" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_HrdHeadcountRequestSeat_WfpPositionAssignment" FOREIGN KEY ("LinkedPositionAssignmentId")
        REFERENCES public."WfpPositionAssignment" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdHeadcountRequestSeat_HrdPersonnelActionOrder" FOREIGN KEY ("LinkedPersonnelActionOrderId")
        REFERENCES public."HrdPersonnelActionOrder" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdHeadcountRequestSeat_Request_Seat" ON public."HrdHeadcountRequestSeat" ("HeadcountRequestId", "SeatNumber");
CREATE UNIQUE INDEX "IX_HrdHeadcountRequestSeat_LinkedPositionAssignment" ON public."HrdHeadcountRequestSeat" ("LinkedPositionAssignmentId")
    WHERE "LinkedPositionAssignmentId" IS NOT NULL AND "SeatStatus" <> 5 AND "IsDelete" = false;
CREATE UNIQUE INDEX "IX_HrdHeadcountRequestSeat_LinkedPersonnelActionOrder" ON public."HrdHeadcountRequestSeat" ("LinkedPersonnelActionOrderId")
    WHERE "LinkedPersonnelActionOrderId" IS NOT NULL AND "SeatStatus" <> 5 AND "IsDelete" = false;

CREATE TABLE public."HrdHeadcountRequestCancellation" (
    "Id" uuid NOT NULL, "CancellationNumber" varchar(50) NOT NULL, "HeadcountRequestId" uuid NOT NULL,
    "CancellationStatus" integer NOT NULL,
    "Reason" varchar(1000) NOT NULL,                      -- SENSITIF
    "RequestedByUserId" uuid NOT NULL, "DecidedByUserId" uuid, "DecidedAt" timestamptz,
    "CreatedFormationId" uuid, "WorkflowInstanceId" uuid,
    CONSTRAINT "PK_HrdHeadcountRequestCancellation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdHeadcountRequestCancellation_Request" FOREIGN KEY ("HeadcountRequestId")
        REFERENCES public."HrdHeadcountRequest" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdHeadcountRequestCancellation_Number" ON public."HrdHeadcountRequestCancellation" ("CancellationNumber");

CREATE TABLE public."HrdFulfilmentPlanChange" (
    "Id" uuid NOT NULL, "HeadcountRequestId" uuid NOT NULL, "SeatId" uuid NOT NULL,
    "FromSource" integer NOT NULL, "ToSource" integer NOT NULL,
    "Reason" varchar(1000) NOT NULL,                      -- SENSITIF
    "ChangedByUserId" uuid NOT NULL, "ChangedAt" timestamptz NOT NULL,
    CONSTRAINT "PK_HrdFulfilmentPlanChange" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdFulfilmentPlanChange_Seat" FOREIGN KEY ("SeatId")
        REFERENCES public."HrdHeadcountRequestSeat" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."HrdStaffingGapSnapshot" (
    "Id" uuid NOT NULL, "SnapshotType" integer NOT NULL, "HospitalSiteId" uuid NOT NULL,
    "ReferenceDate" date NOT NULL, "SourceDocumentType" integer, "SourceDocumentId" uuid,
    "PeriodMonth" date, "TakenAt" timestamptz NOT NULL, "ReconstructedAt" timestamptz,
    CONSTRAINT "PK_HrdStaffingGapSnapshot" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdStaffingGapSnapshot_Site_Month" ON public."HrdStaffingGapSnapshot" ("HospitalSiteId", "PeriodMonth")
    WHERE "SnapshotType" = 2 AND "IsDelete" = false;

CREATE TABLE public."HrdStaffingGapSnapshotLine" (
    "Id" uuid NOT NULL, "SnapshotId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL, "PositionId" uuid NOT NULL,
    "FormationQuantity" numeric(9,2), "FilledFte" numeric(9,2), "ToBeVacatedFte" numeric(9,2),
    "TemporarilyUnavailableFte" numeric(9,2), "NotYetCompetentFte" numeric(9,2), "OutsourcedFte" numeric(9,2),
    "ConcurrentAssignmentCount" integer, "PermanentGap" numeric(9,2), "TemporaryGap" numeric(9,2),
    "CompetencyGap" numeric(9,2), "PlacementDataComplete" boolean NOT NULL DEFAULT false,
    "LeaveDataComplete" boolean NOT NULL DEFAULT false, "CompetencyDataComplete" boolean NOT NULL DEFAULT false,
    "WorkforceTypeDataComplete" boolean NOT NULL DEFAULT false, "ShortageSummaryCount" integer,
    CONSTRAINT "PK_HrdStaffingGapSnapshotLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdStaffingGapSnapshotLine_Snapshot" FOREIGN KEY ("SnapshotId")
        REFERENCES public."HrdStaffingGapSnapshot" ("Id") ON DELETE CASCADE
);
CREATE UNIQUE INDEX "IX_HrdStaffingGapSnapshotLine_Snapshot_Unit_Position" ON public."HrdStaffingGapSnapshotLine" ("SnapshotId", "OrganizationUnitId", "PositionId");
```

FK ke master yang tidak ditulis di atas — `HospitalSiteId`, `ProfessionId`, `ShiftId`,
`WorkflowDefinitionId`, `TemporaryReasonWorkforceProfileId`, `ReplacedWorkforceProfileId`,
`CreatedFormationId`, `FormationVersionIdAtSubmission`, `CancellationId` — seluruhnya `ON DELETE
RESTRICT` sesuai tabel bagian 8.2. Disingkat agar blok tetap terbaca.

### 8.5 Kolom sensitif `S-D1`

| Tabel | Kolom sensitif | Sebab |
| --- | --- | --- |
| `HrdManpowerPlan`, `HrdManpowerPlanLine`, `HrdHeadcountRequest` | Seluruh kolom biaya: `TotalEstimatedCost`, `FinanceAdjustedTotal`, `MonthlyRateSnapshot`, `EstimatedCost`, `IncrementalCost`, `EstimatedMonthlyRate`, `FinanceAdjustedCost` | Anggaran; hanya untuk pemegang `WorkforcePlanningCost : Read` (`HRD-DEC-065`). Bukan gaji perorangan, tetapi tetap tidak masuk logger |
| Seluruh tabel `S-D1` dengan teks bebas | `Justification`, `DeviationReason`, `FinanceAdjustmentReason`, `Reason` | Teks bebas kerap memuat keadaan pribadi, misalnya "Maya batal dimutasi karena sakit" |


---

## 9. Slice `S-D2` — Rekrutmen dan Hiring (`last_changed_in` `v8`, 3 November 2026)

Sumber desain: `02-backend-architecture.md` bagian 17. Keputusan: `HRD-DEC-073` s.d. `HRD-DEC-080`.
Seluruh tabel mewarisi `IdentityModel` (bagian 0.1); kolom auditnya (`Id`, `CreateDateTime`, `UpdateDateTime`, `DeleteDateTime`, `IsDelete`, `IsCancel`, `CancelDateTime`) tidak diulang.

Konvensi yang berlaku untuk seluruh tabel bagian ini:
- Schema dan nama: `public."Hrd<Nama>"`, tunggal PascalCase; `DbSet` jamak.
- Tanggal tanpa jam: `DateOnly` -> `date`.
- Tanggal dengan jam: `DateTime` -> `timestamp with time zone`.
- Uang: `decimal(18,2)`.
- Status: `varchar(30)` dengan penegakan enum pada lapisan aplikasi dan DTO.
- Indeks unik terfilter menyertakan `"IsDelete" = false`.

### 9.1 Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `HrdJobRequisition` | Baru | HR - `S-D2` | Aggregate root permintaan pengadaan tenaga (`HRD-DEC-073`) |
| `HrdJobVacancy` | Baru | HR - `S-D2` | Lowongan publikasi dengan penanda klinis `IsClinical` |
| `HrdCandidate` | Baru | HR - `S-D2` | Identitas pelamar dengan validasi unik NIK (`HRD-DEC-075`) |
| `HrdCandidateApplication` | Baru | HR - `S-D2` | Rekam berkas lamaran dan pelacakan tahapan seleksi |
| `HrdCandidateDocument` | Baru | HR - `S-D2` | Penyimpanan dokumen legalitas & STR klinis (`HRD-DEC-076`) |
| `HrdCandidateScreening` | Baru | HR - `S-D2` | Gerbang penapisan otomatis dokumen STR aktif (`HRD-DEC-076`) |
| `HrdCandidateEvaluation` | Baru | HR - `S-D2` | Evaluasi wawancara HR dan uji klinis unit dengan hak veto (`HRD-DEC-077`) |
| `HrdJobOffer` | Baru | HR - `S-D2` | Penawaran bersyarat dan integrasi kesimpulan MCU (`HRD-DEC-078`) |
| `HrdJobOfferApproval` | Baru | HR - `S-D2` | Log persetujuan bertingkat plafon upah `T13`/`T14` (`HRD-DEC-079`) |
| `HrdPreEmploymentMedicalCheck` | Baru | HR - `S-D2` | Kesimpulan kelaikan fisik MCU dari tim K3RS (`HRD-DEC-078`) |
| `HrdTalentPool` | Baru | HR - `S-D2` | Bank kandidat cadangan berperingkat aktif 6 bulan (`HRD-DEC-080`) |
| `HrdCandidateHiringHandoff` | Baru | HR - `S-D2` | Serah terima ke Sub-modul 05 Onboarding (`HRD-DEC-075`) |
| 20 tabel kandidat rekrutmen lama | Dibuang | HR | Dibersihkan oleh migration dengan `EMPTY_TABLE_GUARD` (`HRD-DEC-074`) |

### 9.2 Tabel baru — rincian kolom material

#### `HrdJobRequisition`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `RequisitionNumber` | varchar(50) | Ya | — | Unik | — | — | Tidak | Nomor seri unik requisition |
| `HeadcountRequestId` | uuid | Ya | — | Ya | `HrdHeadcountRequest` | Restrict | Tidak | Rujukan permintaan headcount yang disetujui |
| `OrganizationUnitId` | uuid | Ya | — | Ya | `MstOrganizationUnit` | Restrict | Tidak | Unit rumah sakit pemohon |
| `PositionId` | uuid | Ya | — | Ya | `MstPosition` | Restrict | Tidak | Jabatan yang dibuka |
| `RequestedSeats` | int | Ya | 1 | — | — | — | Tidak | Jumlah kursi yang diminta |
| `FulfilledSeats` | int | Ya | 0 | — | — | — | Tidak | Jumlah kursi yang sudah terisi |
| `SalaryGradeId` | uuid | Ya | — | Ya | `MstSalaryGrade` | Restrict | Tidak | Plafon grade upah |
| `Status` | varchar(30) | Ya | 'DRAFT' | Ya | — | — | Tidak | DRAFT, SUBMITTED, APPROVED, FULFILLED, CANCELLED |

#### `HrdJobVacancy`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `VacancyNumber` | varchar(50) | Ya | — | Unik | — | — | Tidak | Nomor lowongan |
| `JobRequisitionId` | uuid | Ya | — | Ya | `HrdJobRequisition` | Restrict | Tidak | Requisition induk |
| `Title` | varchar(200) | Ya | — | — | — | — | Tidak | Judul lowongan kerja |
| `TargetSeats` | int | Ya | 1 | — | — | — | Tidak | Alokasi kursi lowongan |
| `IsClinical` | boolean | Ya | false | Ya | — | — | Tidak | Penanda peran klinis (wajib STR) |
| `PublishDate` | date | Ya | — | — | — | — | Tidak | Tanggal tayang |
| `ClosingDate` | date | Ya | — | — | — | — | Tidak | Batas akhir lamaran |
| `Status` | varchar(30) | Ya | 'DRAFT' | Ya | — | — | Tidak | DRAFT, PUBLISHED, CLOSED, CANCELLED |

#### `HrdCandidate`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `CandidateNumber` | varchar(50) | Ya | — | Unik | — | — | Tidak | Nomor kandidat |
| `NationalIdNumber` | varchar(20) | Ya | — | Unik | — | — | Ya | NIK KTP — divalidasi anti-duplikasi |
| `FullName` | varchar(150) | Ya | — | — | — | — | Ya | Nama lengkap |
| `Email` | varchar(100) | Ya | — | Ya | — | — | Ya | Email korespondensi |
| `PhoneNumber` | varchar(30) | Ya | — | — | — | — | Ya | Nomor kontak telepon/WhatsApp |
| `WorkforceProfileId` | uuid | Tidak | null | Ya | `WfpProfile` | Restrict | Tidak | Diisi jika rehire mantan karyawan |

#### `HrdCandidateDocument`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `CandidateId` | uuid | Ya | — | Ya | `HrdCandidate` | Cascade | Tidak | Kandidat pemilik dokumen |
| `ApplicationId` | uuid | Ya | — | Ya | `HrdCandidateApplication` | Cascade | Tidak | Lamaran terkait |
| `DocumentType` | varchar(50) | Ya | — | Ya | — | — | Tidak | License_STR, Diploma, BTCLS, ACLS, CV |
| `DocumentNumber` | varchar(100) | Tidak | null | — | — | — | Tidak | Nomor registrasi STR / Ijazah |
| `ExpiryDate` | date | Tidak | null | Ya | — | — | Tidak | Tanggal masa berlaku STR |
| `FileUrl` | varchar(500) | Ya | — | — | — | — | Tidak | Lokasi berkas tersimpan aman |
| `VerificationStatus` | varchar(30) | Ya | 'PENDING' | Ya | — | — | Tidak | PENDING, VERIFIED, REJECTED |

#### `HrdCandidateScreening`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ApplicationId` | uuid | Ya | — | Unik | `HrdCandidateApplication` | Cascade | Tidak | Satu hasil penapisan per lamaran |
| `IsClinicalPosition` | boolean | Ya | false | — | — | — | Tidak | Mewarisi dari lowongan |
| `HasValidStr` | boolean | Ya | false | — | — | — | Tidak | True jika STR aktif dan tidak kadaluwarsa |
| `Result` | varchar(30) | Ya | 'PENDING' | Ya | — | — | Tidak | PASSED, FAILED (Auto-reject bila STR mati) |

#### `HrdCandidateEvaluation`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ApplicationId` | uuid | Ya | — | Ya | `HrdCandidateApplication` | Cascade | Tidak | Lamaran terkait |
| `EvaluationType` | varchar(30) | Ya | — | Ya | — | — | Tidak | HR_INTERVIEW, CLINICAL_ASSESSMENT |
| `EvaluatorWorkforceProfileId` | uuid | Ya | — | Ya | `WfpProfile` | Restrict | Tidak | Penilai (HR / Kepala Ruang) |
| `CompetencyScore` | decimal(5,2) | Ya | 0 | — | — | — | Tidak | Skor uji klinis / teknis |
| `Recommendation` | varchar(30) | Ya | — | Ya | — | — | Tidak | RECOMMENDED, CONSIDERED, NOT_RECOMMENDED |
| `IsClinicalVeto` | boolean | Ya | false | Ya | — | — | Tidak | True jika unit memberi NOT_RECOMMENDED |
| `FeedbackNotes` | varchar(2000) | Tidak | null | — | — | — | Ya | Catatan kualitatif asesmen |

#### `HrdJobOffer`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `OfferNumber` | varchar(50) | Ya | — | Unik | — | — | Tidak | Nomor surat penawaran |
| `ApplicationId` | uuid | Ya | — | Unik | `HrdCandidateApplication` | Restrict | Tidak | Lamaran terpilih |
| `OfferedBaseSalary` | decimal(18,2) | Ya | — | — | — | — | Ya | Gaji pokok yang ditawarkan |
| `OfferedFixedAllowance` | decimal(18,2) | Ya | 0 | — | — | — | Ya | Tunjangan tetap |
| `IsAboveGradeMax` | boolean | Ya | false | Ya | — | — | Tidak | True jika > batas maksimum skala upah |
| `OfferStatus` | varchar(30) | Ya | 'DRAFT' | Ya | — | — | Tidak | DRAFT, CONDITIONAL_ACCEPTED, FINAL_ACCEPTED, REJECTED |
| `RequiredMcuStatus` | varchar(30) | Ya | 'PENDING' | Ya | — | — | Tidak | PENDING, FIT, FIT_WITH_RESTRICTIONS, UNFIT |

#### `HrdPreEmploymentMedicalCheck`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `JobOfferId` | uuid | Ya | — | Unik | `HrdJobOffer` | Restrict | Tidak | Penawaran bersyarat terkait |
| `CandidateId` | uuid | Ya | — | Ya | `HrdCandidate` | Restrict | Tidak | Kandidat yang diperiksa |
| `FitnessStatus` | varchar(30) | Ya | 'PENDING' | Ya | — | — | Tidak | FIT, FIT_WITH_RESTRICTIONS, UNFIT |
| `RestrictionsNotes` | varchar(1000) | Tidak | null | — | — | — | Ya | Catatan pembatasan beban dinas dari K3RS |

#### `HrdTalentPool`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `CandidateId` | uuid | Ya | — | Ya | `HrdCandidate` | Restrict | Tidak | Kandidat cadangan |
| `PositionId` | uuid | Ya | — | Ya | `MstPosition` | Restrict | Tidak | Jabatan kompeten |
| `RankScore` | decimal(5,2) | Ya | — | Ya | — | — | Tidak | Peringkat skor kelulusan klinis |
| `Status` | varchar(30) | Ya | 'ACTIVE' | Ya | — | — | Tidak | ACTIVE, PROMOTED, EXPIRED |
| `ExpiryDate` | date | Ya | — | Ya | — | — | Tidak | Batas aktif 6 bulan sejak masuk pool |

#### `HrdCandidateHiringHandoff`
| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku Hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `JobOfferId` | uuid | Ya | — | Unik | `HrdJobOffer` | Restrict | Tidak | Penawaran final yang diterima |
| `WorkforceProfileId` | uuid | Ya | — | Ya | `WfpProfile` | Restrict | Tidak | Profil pegawai yang disiapkan (Pending Onboarding) |
| `HandoffStatus` | varchar(30) | Ya | 'PENDING' | Ya | — | — | Tidak | PENDING, IN_ONBOARDING, COMPLETED |

### 9.3 Kolom Sensitif S-D2

1. **Privasi Pelamar (Data Pribadi / UU PDP)**: `NationalIdNumber` (NIK), `FullName`, `Email`, `PhoneNumber`, `Address` pada `HrdCandidate`.
2. **Kompensasi Perorangan**: `OfferedBaseSalary`, `OfferedFixedAllowance` pada `HrdJobOffer`.
3. **Kesehatan Kerja**: `RestrictionsNotes` pada `HrdPreEmploymentMedicalCheck`. Catatan: diagnosis penyakit detail DILARANG masuk tabel ini dan tetap di modul Occupational Health Sub-modul 18.
4. **Catatan Evaluasi Personal**: `FeedbackNotes` pada `HrdCandidateEvaluation`.


---

## 10. Slice `S-C4-ONB` — Onboarding dan Kontrak Kerja, sub-modul 05 (`last_changed_in` `v9`, 4 November 2026)

| Field | Isi |
| --- | --- |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan" |
| Sumber desain | `02-backend-architecture.md` bagian 18; decision log revision `19` bagian 34 |
| Backend SHA | `8b17b75a` |

Seluruh tabel mewarisi `IdentityModel`; sepuluh kolom auditnya tidak diulang (kepala dokumen ini).
Penghapusan bersifat penandaan `IsDelete`. Tipe tanggal tanpa jam memakai `DateOnly` (`date`), mengikuti
`HrdJobOffer.ProposedJoinDate`; kolom `WfpContractHistory` yang sudah ada tetap `DateTime`.

### 10.1 Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `HrdOnboardingCase` | Baru | HR — `S-C4-ONB` | Menggantikan `TrxEmployeeOnboarding` dan `WfpOnboardingChecklist` |
| `HrdOnboardingCaseItem` | Baru | HR — `S-C4-ONB` | Menggantikan `TrxEmployeeOnboardingTask` dan `WfpOnboardingTask` |
| `MstOnboardingTemplate` | Diperbarui | HR — `S-C4-ONB` | Master; lokasi folder utang teknis |
| `MstOnboardingTemplateTask` | Diperbarui | HR — `S-C4-ONB` | Master |
| `HrdOnboardingAccessRequest` | Baru | HR — `S-C4-ONB` | Akun dan akses kerja |
| `WfpContractHistory` | Diperbarui | 02 Workforce Core | Penambahan kolom saja; ditulis lewat `WfpContractHistoryWriteService` |
| `HrdContractDecision` | Baru | HR — `S-C4-ONB` | Menggantikan `TrxContractNonRenewal` |
| `HrdContractCorrection` | Baru | HR — `S-C4-ONB` | — |
| `HrdInitialContractBatch`, `HrdInitialContractBatchRow` | Baru | HR — `S-C4-ONB` | — |
| `HrdProbation`, `HrdProbationAssessment` | Baru | HR — `S-C4-ONB` | Menggantikan `TrxProbationReview` |
| `HrdClinicalPreceptorship`, `HrdClinicalPreceptorshipPreceptor` | Baru | HR — `S-C4-ONB` | — |
| `HrdEmploymentLifecycleSetting` | Baru | HR — `S-C4-ONB` | Satu baris per rumah sakit |
| `MstContractType` | Diperbarui | 01 Master Data HR | Satu kolom baru |
| `HrdJobOffer` | Diperbarui | 04 Rekrutmen `S-D2` | Dua kolom baru (`HRD-DEC-095`) |
| `MstEmployee`, `MstDoctor` | Sudah ada | 01 Master Data HR | Lima isian ditulis sebagai salinan |
| `TrxEmployeeSeparation` | Sudah ada | 20 | Dua nilai `SeparationType` baru |
| `HrdCandidateHiringHandoff`, `HrdPreEmploymentMedicalCheck` | Sudah ada | 04 Rekrutmen `S-D2` | Dibaca |
| `WfpCertification`, `WfpClinicalPrivilege` | Sudah ada | `CredentialingManagement` | Dibaca |
| `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpSalaryAssignment`, `WfpManagerAssignment` | Sudah ada | 02 Workforce Core | Ditulis lewat `WfpNewHireAssignmentService` |
| `TrxEmployeeOnboarding`, `TrxEmployeeOnboardingTask`, `WfpOnboardingChecklist`, `WfpOnboardingTask`, `TrxProbationReview`, `TrxContractNonRenewal` | Dibuang | — | Kosong (`HRD-TF-047`); pengaman tabel kosong |

### 10.2 `HrdOnboardingCase`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CaseNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | Dari number series, contoh `ONB-2027-000015` |
| `HiringHandoffId` | `Guid` | Ya | — | Unik | FK `HrdCandidateHiringHandoff` | `Restrict` | Tidak | Satu serah terima satu kasus (`HRD-DEC-083`) |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | Pegawai baru |
| `HospitalSiteId` | `Guid` | Ya | — | Index | FK `MstHospitalSite` | `Restrict` | Tidak | Disalin dari requisition |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | `Restrict` | Tidak | Unit tujuan; dasar cakupan Kepala Unit |
| `PositionId` | `Guid` | Ya | — | — | FK `MstPosition` | `Restrict` | Tidak | Jabatan tujuan |
| `IsClinical` | `bool` | Ya | `false` | — | — | — | Tidak | Dari lowongan; menentukan lapis Klinis |
| `PlannedStartDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | Dari tawaran `ProposedJoinDate` |
| `CoordinatorUserId` | `Guid` | Ya | — | Index | FK `ApplicationUser` | `Restrict` | Tidak | Bawaan: petugas yang menjalankan serah terima |
| `CaseStatus` | `int` (`OnboardingCaseStatus`) | Ya | `InProgress` | Index | — | — | Tidak | — |
| `FirstDayAttendedOn` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Terisi bersama butir `FIRST_DAY_ATTENDANCE` |
| `FirstDayRecordedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `ReadyForWorkAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ReadyForWorkByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Petugas HRD yang menyatakan siap kerja |
| `CancelReason` | `int?` (`OnboardingCancelReason`) | Tidak | — | — | — | — | Tidak | Wajib saat batal bergabung |
| `CancelNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Dapat memuat alasan pribadi |
| `CancelledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CancelledByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `HasGeneralTemplate` | `bool` | Ya | `true` | — | — | — | Tidak | `false` bila saat dibuat belum ada Template Umum aktif |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Concurrency token |

### 10.3 `HrdOnboardingCaseItem`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OnboardingCaseId` | `Guid` | Ya | — | Unik bersama `ItemCode` | FK `HrdOnboardingCase` | `Restrict` | Tidak | — |
| `TemplateLayer` | `int` (`OnboardingTemplateLayer`) | Ya | — | — | — | — | Tidak | Asal lapis butir |
| `SourceTemplateTaskId` | `Guid?` | Tidak | — | — | FK `MstOnboardingTemplateTask` | `Restrict` | Tidak | Kosong untuk butir sistem tanpa template |
| `ItemCode` | `string(50)` | Ya | — | Unik bersama kasus | — | — | Tidak | Disalin dari `TaskCode`, atau `SystemItemCode` |
| `ItemName` | `string(250)` | Ya | — | — | — | — | Tidak | — |
| `TaskCategory` | `string(50)?` | Tidak | — | — | — | — | Tidak | Kelompok tampilan, misalnya "Orientasi" |
| `Description` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `SystemItemCode` | `string(40)?` | Tidak | — | Index | — | — | Tidak | Lima kode pada `02-backend-architecture.md` 18.4.2 |
| `ResponsibleRole` | `int` (`OnboardingResponsibleRole`) | Ya | — | — | — | — | Tidak | `Hrd`, `UnitHead`, `System` |
| `ItemKind` | `int` (`OnboardingItemKind`) | Ya | `Manual` | — | — | — | Tidak | — |
| `AutomaticSource` | `int?` (`OnboardingAutomaticSource`) | Tidak | — | — | — | — | Tidak | Wajib bila `ItemKind = Automatic` |
| `CertificationTypeId` | `Guid?` | Tidak | — | — | FK `MstCertificationType` | `Restrict` | Tidak | Wajib bila sumber `MandatoryCertification` |
| `IsRequired` | `bool` | Ya | `true` | — | — | — | Tidak | Butir sistem selalu `true` |
| `DueDayOffset` | `int` | Ya | `0` | — | — | — | Tidak | Hari sesudah tanggal mulai rencana; urutan tampil |
| `DueDate` | `DateOnly` | Ya | — | — | — | — | Tidak | `PlannedStartDate` + `DueDayOffset` |
| `ItemStatus` | `int` (`OnboardingItemStatus`) | Ya | `Pending` | — | — | — | Tidak | — |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CompletedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Kosong bila diisi sistem |
| `CompletionNote` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `SelectedWorkforceProfileId` | `Guid?` | Tidak | — | — | FK `MstWorkforceProfile` | `Restrict` | Tidak | Atasan langsung atau preceptor yang dipilih |
| `EvidenceStoragePath` | `string(500)?` | Tidak | — | — | — | — | Tidak | Path relatif `WorkflowFileStorageService` |
| `EvidenceFileName` | `string(250)?` | Tidak | — | — | — | — | Tidak | Nama berkas asli |
| `EvidenceContentType` | `string(150)?` | Tidak | — | — | — | — | Tidak | — |
| `EvidenceFileSize` | `long?` | Tidak | — | — | — | — | Tidak | Byte |
| `AutomaticCheckedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Pemeriksaan otomatis terakhir |
| `AutomaticResultNote` | `string(250)?` | Tidak | — | — | — | — | Tidak | Contoh "BTCLS berlaku s.d. 12-03-2028" |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 10.4 `MstOnboardingTemplate` — Diperbarui

Kolom yang **dibuang**: `TemplateType`, `LegalEntityId`, `HospitalSiteId`, `DepartmentId`, `PositionId`,
`WorkforceTypeId`, `EmployeeCategoryId`, `EmploymentTypeId`, `WorkflowDefinitionId`,
`AutoCreateChecklist`, `RequiresProbationReview`, `ProbationReviewAfterDays`, `EffectiveStartDate`,
`EffectiveEndDate`, `IsDefault`. Bentuk akhir:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada |
| `TemplateCode` | `string(50)` | Ya | — | Unik | — | — | Tidak | Sudah ada |
| `TemplateName` | `string(200)` | Ya | — | — | — | — | Tidak | Sudah ada |
| `TemplateLayer` | `int` (`OnboardingTemplateLayer`) | Ya | — | Unik bersyarat | — | — | Tidak | **Baru.** Satu aktif per lapis `General` dan `Clinical` |
| `OrganizationUnitId` | `Guid?` | Tidak | — | Unik bersyarat | FK `MstOrganizationUnit` | `Restrict` | Tidak | Sudah ada. Wajib bila lapis `Unit`; satu aktif per unit |
| `TargetCompletionDays` | `int` | Ya | `30` | — | — | — | Tidak | Sudah ada |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | Sudah ada; naik setiap disimpan |
| `Description` | `string(1500)?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |

### 10.5 `MstOnboardingTemplateTask` — Diperbarui

Kolom yang **dibuang**: `ResponsiblePartyType`, `ResponsibleOrganizationUnitId`, `ResponsiblePositionId`,
`RequiresDocument`, `RequiresVerification`, `CompletionSource`, `SortOrder`. Bentuk akhir:

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada |
| `OnboardingTemplateId` | `Guid` | Ya | — | Unik bersama `TaskCode` | FK `MstOnboardingTemplate` | `Restrict` | Tidak | Sudah ada |
| `TaskCode` | `string(50)` | Ya | — | Unik bersama template | — | — | Tidak | Sudah ada |
| `TaskName` | `string(250)` | Ya | — | — | — | — | Tidak | Sudah ada |
| `TaskCategory` | `string(50)` | Ya | `"General"` | — | — | — | Tidak | Sudah ada; kelompok tampilan |
| `Description` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `ResponsibleRole` | `int` (`OnboardingResponsibleRole`) | Ya | `Hrd` | — | — | — | Tidak | **Baru** |
| `ItemKind` | `int` (`OnboardingItemKind`) | Ya | `Manual` | — | — | — | Tidak | **Baru** |
| `AutomaticSource` | `int?` | Tidak | — | — | — | — | Tidak | **Baru** |
| `CertificationTypeId` | `Guid?` | Tidak | — | — | FK `MstCertificationType` | `Restrict` | Tidak | **Baru** |
| `SystemItemCode` | `string(40)?` | Tidak | — | — | — | — | Tidak | **Baru.** Butir sistem terkunci |
| `IsRequired` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |
| `DueDayOffset` | `int` | Ya | `0` | — | — | — | Tidak | Sudah ada; urutan tampil semantik |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |

### 10.6 `HrdOnboardingAccessRequest`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OnboardingCaseId` | `Guid` | Ya | — | Unik bersama `RequestType` | FK `HrdOnboardingCase` | `Restrict` | Tidak | — |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | — |
| `RequestType` | `int` (`OnboardingAccessRequestType`) | Ya | — | Unik bersama kasus | — | — | Tidak | `AccountCreation` atau `WorkAccess` |
| `RequestStatus` | `int` (`OnboardingAccessRequestStatus`) | Ya | `Requested` | Index | — | — | Tidak | — |
| `ApplicationUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Akun yang dibuat |
| `FailureMessage` | `string(500)?` | Tidak | — | — | — | — | Tidak | Contoh "Email pegawai belum diisi" |
| `SelfServiceActivatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Akun aktif untuk layanan mandiri |
| `RequestedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CompletedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Administrator/IT untuk `WorkAccess` |
| `CompletionNote` | `string(500)?` | Tidak | — | — | — | — | Tidak | Contoh peran yang diberikan |

### 10.7 `WfpContractHistory` — Diperbarui (penambahan kolom saja)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada |
| `WorkforceProfileId` | `Guid` | Ya | — | Unik bersyarat (`IsCurrent`) | FK `MstWorkforceProfile` | `Restrict` | Tidak | Sudah ada |
| `PreviousContractHistoryId` | `Guid?` | Tidak | — | — | FK `WfpContractHistory` | `Restrict` | Tidak | Sudah ada; rantai perpanjangan |
| `ContractTypeId` | `Guid?` | Tidak | — | — | FK `MstContractType` | `Restrict` | Tidak | Sudah ada; **wajib** untuk setiap baris yang ditulis 05 |
| `EmploymentTypeId` | `Guid?` | Tidak | — | — | FK `MstEmploymentType` | `Restrict` | Tidak | Sudah ada; diisi dari `MstContractType.EmploymentTypeId` |
| `WorkerSourceId` | `Guid?` | Tidak | — | — | FK `MstWorkerSource` | `Restrict` | Tidak | Sudah ada; tidak diisi 05 |
| `TerminationReasonId` | `Guid?` | Tidak | — | — | FK | `Restrict` | Tidak | Sudah ada; tidak diisi 05 |
| `ContractNumber` | `string(100)` | Ya | — | Unik bersama profil | — | — | Tidak | Sudah ada; dari number series atau batch |
| `HistoryType` | `string(50)` | Ya | `"Initial"` | — | — | — | Tidak | Sudah ada. Nilai 05: `Initial`, `InitialData`, `Renewal`, `Conversion` |
| `ContractStatus` | `string(50)` | Ya | `"Draft"` | — | — | — | Tidak | Sudah ada. Nilai 05: `Draft`, `Active`, `Completed`, `Ended`, `LapsedWithoutDecision`, `Cancelled` |
| `StartDate` | `DateTime` | Ya | — | — | — | — | Tidak | Sudah ada |
| `EndDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada; wajib bila jenis kontrak `RequiresEndDate` |
| `SignedDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada; tanggal tanda tangan |
| `ProbationEndDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada; **diisi sistem saja** dari `HrdProbation` |
| `TerminatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada; tidak diisi 05 |
| `RenewalSequence` | `int` | Ya | `0` | — | — | — | Tidak | Sudah ada |
| `IsCurrent` | `bool` | Ya | `false` | Unik bersyarat | — | — | Tidak | Sudah ada; tepat satu per pegawai |
| `DocumentPath` | `string(500)?` | Tidak | — | — | — | — | Tidak | Sudah ada; scan bertanda tangan |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |
| `SourceDocumentType` | `string(50)?` | Tidak | — | Index bersama `SourceDocumentId` | — | — | Tidak | **Baru.** `HiringHandoff`, `ContractDecision`, `InitialContractBatch` |
| `SourceDocumentId` | `Guid?` | Tidak | — | Index | — | — | Tidak | **Baru.** Tanpa FK karena sumbernya tiga tabel |
| `FixedTermSeriesStartDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru.** Awal rangkaian PKWT untuk batas 5 tahun |
| `DocumentFileName` | `string(250)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| `DocumentContentType` | `string(150)?` | Tidak | — | — | — | — | Tidak | **Baru** |
| `SignedRecordedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru.** Saat HRD menandai sudah ditandatangani |
| `SignedRecordedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | **Baru** |
| `RowVersion` | `Guid` | Ya | `gen_random_uuid()` | — | — | — | Tidak | **Baru.** Concurrency token |

### 10.8 `HrdContractDecision`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `DecisionNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | Number series |
| `ContractHistoryId` | `Guid` | Ya | — | Unik bersyarat (status terbuka atau disetujui) | FK `WfpContractHistory` | `Restrict` | Tidak | Kontrak yang akan berakhir |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | — |
| `DecisionType` | `int` (`ContractDecisionType`) | Ya | — | — | — | — | Tidak | — |
| `ExtensionMonths` | `int?` | Tidak | — | — | — | — | Tidak | Wajib bila `Extend`; 1–60 |
| `ProposedStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal berakhir lama + 1 hari |
| `ProposedEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Terisi bila `Extend` |
| `ReasonText` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Wajib bila `NotRenew` |
| `DecisionStatus` | `int` (`ContractDecisionStatus`) | Ya | `Submitted` | Index | — | — | Tidak | — |
| `SubmittedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Atasan langsung atau Kepala Unit |
| `SubmittedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | FK instance workflow | `Restrict` | Tidak | `T16` |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan penyetuju |
| `FollowUpContractHistoryId` | `Guid?` | Tidak | — | — | FK `WfpContractHistory` | `Restrict` | Tidak | Kontrak lanjutan |
| `EmployeeNotifiedOn` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal pemberitahuan tidak diperpanjang |
| `NotificationRecordedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `EmployeeSeparationId` | `Guid?` | Tidak | — | — | FK `TrxEmployeeSeparation` | `Restrict` | Tidak | Serah terima ke 20 |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 10.9 `HrdContractCorrection`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `CorrectionNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | Number series |
| `ContractHistoryId` | `Guid` | Ya | — | Unik bersyarat (status terbuka) | FK `WfpContractHistory` | `Restrict` | Tidak | — |
| `BeforeContractNumber` | `string(100)` | Ya | — | — | — | — | Tidak | Salinan saat diajukan |
| `BeforeContractTypeId` | `Guid` | Ya | — | — | FK `MstContractType` | `Restrict` | Tidak | — |
| `BeforeStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `BeforeEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | — |
| `BeforeDocumentPath` | `string(500)?` | Tidak | — | — | — | — | Tidak | Scan lama tetap tersimpan |
| `AfterContractNumber` | `string(100)` | Ya | — | — | — | — | Tidak | — |
| `AfterContractTypeId` | `Guid` | Ya | — | — | FK `MstContractType` | `Restrict` | Tidak | — |
| `AfterStartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `AfterEndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | — |
| `ReasonText` | `string(2000)` | Ya | — | — | — | — | **Ya** | — |
| `EvidenceStoragePath` | `string(500)` | Ya | — | — | — | — | Tidak | Scan dokumen perbaikan bertanda tangan |
| `EvidenceFileName` | `string(250)` | Ya | — | — | — | — | Tidak | — |
| `EvidenceContentType` | `string(150)` | Ya | — | — | — | — | Tidak | — |
| `CorrectionStatus` | `int` (`ContractCorrectionStatus`) | Ya | `Submitted` | Index | — | — | Tidak | — |
| `SubmittedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `SubmittedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | FK instance workflow | `Restrict` | Tidak | `T17` |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionNote` | `string(1000)?` | Tidak | — | — | — | — | Tidak | — |
| `AppliedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 10.10 `HrdInitialContractBatch` dan `HrdInitialContractBatchRow`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Batch |
| `BatchNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | Number series |
| `HospitalSiteId` | `Guid` | Ya | — | Index | FK `MstHospitalSite` | `Restrict` | Tidak | — |
| `BatchStatus` | `int` (`InitialContractBatchStatus`) | Ya | `Draft` | Index | — | — | Tidak | — |
| `TotalRows`, `ValidRows`, `InvalidRows` | `int` | Ya | `0` | — | — | — | Tidak | Dihitung ulang setiap disimpan |
| `SubmittedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `SubmittedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | — | FK instance workflow | `Restrict` | Tidak | `T18` |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `DecidedAt`, `AppliedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

| Kolom baris | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `BatchId` | `Guid` | Ya | — | Unik bersama `WorkforceProfileId` | FK `HrdInitialContractBatch` | `Restrict` | Tidak | — |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | — |
| `ContractTypeId` | `Guid` | Ya | — | — | FK `MstContractType` | `Restrict` | Tidak | — |
| `ContractNumber` | `string(100)` | Ya | — | — | — | — | Tidak | Nomor pada dokumen lama |
| `StartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `EndDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Wajib untuk PKWT |
| `FixedTermSeriesStartDate` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Wajib untuk PKWT |
| `IsValid` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `ValidationMessage` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `CreatedContractHistoryId` | `Guid?` | Tidak | — | — | FK `WfpContractHistory` | `Restrict` | Tidak | Terisi saat batch diterapkan |

### 10.11 `HrdProbation` dan `HrdProbationAssessment`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Masa percobaan |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | — |
| `ContractHistoryId` | `Guid` | Ya | — | Unik | FK `WfpContractHistory` | `Restrict` | Tidak | Kontrak PKWTT pertama |
| `StartDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal mulai kontrak |
| `EndDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | `StartDate` + 3 bulan − 1 hari |
| `ReminderFromDate` | `DateOnly` | Ya | — | Index | — | — | Tidak | `EndDate` − `ProbationReminderDays` saat dibuat |
| `ProbationStatus` | `int` (`ProbationStatus`) | Ya | `Running` | Index | — | — | Tidak | — |
| `ClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ClosedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Kosong bila lulus otomatis |
| `EmployeeSeparationId` | `Guid?` | Tidak | — | — | FK `TrxEmployeeSeparation` | `Restrict` | Tidak | Bila tidak lulus |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

| Kolom penilaian | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `AssessmentNumber` | `string(30)` | Ya | — | Unik | — | — | Tidak | Number series |
| `ProbationId` | `Guid` | Ya | — | Index | FK `HrdProbation` | `Restrict` | Tidak | — |
| `Result` | `int` (`ProbationResult`) | Ya | — | — | — | — | Tidak | `Pass` atau `Fail` |
| `ReasonText` | `string(2000)` | Ya | — | — | — | — | **Ya** | Penilaian atasan |
| `EvidenceStoragePath` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib bila `Fail` |
| `EvidenceFileName` | `string(250)?` | Tidak | — | — | — | — | Tidak | — |
| `EvidenceContentType` | `string(150)?` | Tidak | — | — | — | — | Tidak | — |
| `AssessmentStatus` | `int` (`ProbationAssessmentStatus`) | Ya | `Submitted` | Index | — | — | Tidak | — |
| `SubmittedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `SubmittedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `WorkflowInstanceId` | `Guid?` | Tidak | — | Index | FK instance workflow | `Restrict` | Tidak | `T15` |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `DecisionNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 10.12 `HrdClinicalPreceptorship` dan `HrdClinicalPreceptorshipPreceptor`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OnboardingCaseId` | `Guid` | Ya | — | Unik | FK `HrdOnboardingCase` | `Restrict` | Tidak | — |
| `WorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | Tenaga klinis yang didampingi |
| `OrganizationUnitId` | `Guid` | Ya | — | Index | FK `MstOrganizationUnit` | `Restrict` | Tidak | Cakupan Kepala Unit |
| `PreceptorshipStatus` | `int` (`PreceptorshipStatus`) | Ya | `PreceptorAssigned` | Index | — | — | Tidak | — |
| `AccompaniedSince` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Tanggal siap kerja |
| `IndependentOn` | `DateOnly?` | Tidak | — | — | — | — | Tidak | — |
| `DeclaredByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Kepala Unit |
| `EvidenceStoragePath` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib saat siap mandiri |
| `EvidenceFileName` | `string(250)?` | Tidak | — | — | — | — | Tidak | — |
| `EvidenceContentType` | `string(150)?` | Tidak | — | — | — | — | Tidak | — |
| `AssessmentNote` | `string(2000)?` | Tidak | — | — | — | — | **Ya** | Catatan kompetensi |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

| Kolom preceptor | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PreceptorshipId` | `Guid` | Ya | — | Unik bersyarat (`AssignedUntil IS NULL`) | FK `HrdClinicalPreceptorship` | `Restrict` | Tidak | Satu preceptor berjalan |
| `PreceptorWorkforceProfileId` | `Guid` | Ya | — | Index | FK `MstWorkforceProfile` | `Restrict` | Tidak | — |
| `AssignedFrom` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `AssignedUntil` | `DateOnly?` | Tidak | — | — | — | — | Tidak | Terisi saat diganti atau siap mandiri |
| `AssignedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `ChangeReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib saat mengganti |

### 10.13 `HrdEmploymentLifecycleSetting`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `HospitalSiteId` | `Guid` | Ya | — | Unik | FK `MstHospitalSite` | `Restrict` | Tidak | — |
| `ContractReminderDays` | `int` | Ya | `60` | — | — | — | Tidak | 14–180 |
| `ContractEscalationDays` | `int` | Ya | `14` | — | — | — | Tidak | 1 sampai kurang dari `ContractReminderDays` |
| `ProbationReminderDays` | `int` | Ya | `14` | — | — | — | Tidak | 1–60 |
| `LateStartFlagDays` | `int` | Ya | `3` | — | — | — | Tidak | 1–30 |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 10.14 `MstContractType` dan `HrdJobOffer` — Diperbarui

| Kolom `MstContractType` | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada |
| `ContractTypeCode` | `string(50)` | Ya | — | Unik | — | — | Tidak | Sudah ada; `PKWT`, `PKWTT` |
| `ContractTypeName` | `string(150)` | Ya | — | — | — | — | Tidak | Sudah ada |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `DefaultDurationMonths` | `int?` | Tidak | — | — | — | — | Tidak | Sudah ada; usulan awal pada layar tawaran |
| `IsRenewable` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |
| `RequiresEndDate` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada; menentukan wajibnya lama kontrak |
| `IsProbationApplicable` | `bool` | Ya | `false` | — | — | — | Tidak | Sudah ada; **satu-satunya penentu masa percobaan** (`HRD-DEC-085`) |
| `EmploymentTypeId` | `Guid?` | Tidak | — | — | FK `MstEmploymentType` | `Restrict` | Tidak | **Baru.** Jenis kepegawaian yang dipakai kontrak jenis ini |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Sudah ada; kolom warisan master, tidak disentuh |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Sudah ada |

| Kolom `HrdJobOffer` | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Sudah ada |
| `OfferNumber` | `string(50)` | Ya | — | Unik | — | — | Tidak | Sudah ada |
| `ApplicationId` | `Guid` | Ya | — | Index | FK `HrdCandidateApplication` | `Restrict` | Tidak | Sudah ada |
| `SalaryGradeId` | `Guid` | Ya | — | — | FK `MstSalaryGrade` | `Restrict` | Tidak | Sudah ada |
| `OfferedBasicSalary` | `decimal` | Ya | — | — | — | — | **Ya** | Sudah ada |
| `MaximumGradeSalary` | `decimal` | Ya | — | — | — | — | Tidak | Sudah ada; plafon grade |
| `ApprovalTier` | `int` (`JobOfferApprovalTier`) | Ya | `Tier13Standard` | — | — | — | Tidak | Sudah ada |
| `OfferedPositionTitle` | `string(150)` | Ya | `""` | — | — | — | Tidak | Sudah ada |
| `ProposedJoinDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Sudah ada; tanggal mulai kontrak |
| `OfferDate` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Sudah ada |
| `ExpiryDate` | `DateTime` | Ya | — | — | — | — | Tidak | Sudah ada |
| `Status` | `int` (`JobOfferStatus`) | Ya | `Draft` | Index | — | — | Tidak | Sudah ada |
| `CandidateResponseDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `CandidateResponseNotes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Sudah ada |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Sudah ada |
| `ContractTypeId` | `Guid?` | Tidak di database, **wajib** di service | — | — | FK `MstContractType` | `Restrict` | Tidak | **Baru** (`HRD-DEC-095`) |
| `ContractDurationMonths` | `int?` | Tidak | — | — | — | — | Tidak | **Baru.** Wajib 1–60 bila jenis kontrak `RequiresEndDate` |

### 10.15 Tabel `Sudah ada` — kolom kunci yang dipakai

| Tabel | Kolom kunci | Dipakai untuk | File model |
| --- | --- | --- | --- |
| `MstEmployee` | `WorkforceProfileId`, `EmploymentTypeId`, `EmploymentStatusId`, `JoinDate`, `ProbationEndDate`, `ContractEndDate`, `Email`, `BirthDate` | Salinan kepegawaian; syarat pembuatan akun | `MasterData/Workforce/Models/MstEmployee.cs` |
| `MstEmploymentStatus` | `EmploymentStatusCode`, `IsActiveEmployment`, `IsSchedulable`, `IsPayrollEligible`, `IsTerminalStatus` | Tiga status baru (`02-backend-architecture.md` 18.8) | `MasterData/Workforce/Models/MstEmploymentStatus.cs` |
| `HrdCandidateHiringHandoff` | `Id`, `OfferId`, `WorkforceProfileId`, `Status` | Pembuka kasus | `RecruitmentManagement/Models/HrdCandidateHiringHandoff.cs` |
| `HrdPreEmploymentMedicalCheck` | `OfferId`, `ClearanceResult` | Butir otomatis kelaikan | `RecruitmentManagement/Models/HrdPreEmploymentMedicalCheck.cs` |
| `WfpCertification` | `WorkforceProfileId`, `CertificationTypeId`, `IsVerified`, `ExpiredDate`, `IsLifetime` | Butir otomatis sertifikat | `CredentialingManagement/Models/WfpCertification.cs` |
| `WfpClinicalPrivilege` | `WorkforceProfileId`, status aktif | Peringatan kredensial | `CredentialingManagement/Models/` |
| `WfpManagerAssignment` | `WorkforceProfileId`, `ManagerWorkforceProfileId`, tanggal berlaku | Relasi atasan saat siap kerja; penentu atasan langsung | `WorkforceCore/Models/WfpManagerAssignment.cs` |
| `TrxEmployeeSeparation` | `WorkforceProfileId`, `SeparationType`, `SeparationStatus`, `LastWorkingDate` | Serah terima ke 20 | `LifecycleManagement/Models/TrxEmployeeSeparation.cs` |
| `ApplicationUser` | `WorkforceProfileId`, `EmployeeId`, `IsActive` | Akun bertahap | `Models/ApplicationUser.cs` |

### 10.16 Kolom sensitif `S-C4-ONB`

`CancelNote`, seluruh `ReasonText`, `DecisionNote` pada keputusan kontrak dan penilaian masa
percobaan, `AssessmentNote` pendampingan, dan `HrdJobOffer.OfferedBasicSalary`. Kolom ini **MUST NOT**
masuk custom logger. Isi berkas bukti — form penilaian masa percobaan dan logbook pendampingan —
diperlakukan sama: diunduh lewat endpoint terautentikasi, tidak pernah lewat path publik.

### 10.17 Bentuk DDL

> **Peringatan.** Basis data dibentuk EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk
> tabel**, bukan skrip yang dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."HrdOnboardingCase" (
    "Id" uuid NOT NULL, "CaseNumber" varchar(30) NOT NULL, "HiringHandoffId" uuid NOT NULL,
    "WorkforceProfileId" uuid NOT NULL, "HospitalSiteId" uuid NOT NULL, "OrganizationUnitId" uuid NOT NULL,
    "PositionId" uuid NOT NULL, "IsClinical" boolean NOT NULL DEFAULT false, "PlannedStartDate" date NOT NULL,
    "CoordinatorUserId" uuid NOT NULL, "CaseStatus" integer NOT NULL,
    "FirstDayAttendedOn" date, "FirstDayRecordedByUserId" uuid, "ReadyForWorkAt" timestamp,
    "ReadyForWorkByUserId" uuid, "CancelReason" integer, "CancelNote" varchar(1000), -- SENSITIF
    "CancelledAt" timestamp, "CancelledByUserId" uuid, "HasGeneralTemplate" boolean NOT NULL DEFAULT true,
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdOnboardingCase" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdOnboardingCase_HrdCandidateHiringHandoff_HiringHandoffId"
        FOREIGN KEY ("HiringHandoffId") REFERENCES public."HrdCandidateHiringHandoff" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_HrdOnboardingCase_MstWorkforceProfile_WorkforceProfileId"
        FOREIGN KEY ("WorkforceProfileId") REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT
    -- FK HospitalSiteId, OrganizationUnitId, PositionId, CoordinatorUserId: pola sama, ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdOnboardingCase_CaseNumber" ON public."HrdOnboardingCase" ("CaseNumber");
CREATE UNIQUE INDEX "IX_HrdOnboardingCase_HiringHandoffId" ON public."HrdOnboardingCase" ("HiringHandoffId") WHERE "IsDelete" = false;
CREATE INDEX "IX_HrdOnboardingCase_OrganizationUnitId_CaseStatus" ON public."HrdOnboardingCase" ("OrganizationUnitId", "CaseStatus");

CREATE TABLE public."HrdOnboardingCaseItem" (
    "Id" uuid NOT NULL, "OnboardingCaseId" uuid NOT NULL, "TemplateLayer" integer NOT NULL,
    "SourceTemplateTaskId" uuid, "ItemCode" varchar(50) NOT NULL, "ItemName" varchar(250) NOT NULL,
    "TaskCategory" varchar(50), "Description" varchar(1000), "SystemItemCode" varchar(40),
    "ResponsibleRole" integer NOT NULL, "ItemKind" integer NOT NULL, "AutomaticSource" integer,
    "CertificationTypeId" uuid, "IsRequired" boolean NOT NULL DEFAULT true, "DueDayOffset" integer NOT NULL DEFAULT 0,
    "DueDate" date NOT NULL, "ItemStatus" integer NOT NULL, "CompletedAt" timestamp, "CompletedByUserId" uuid,
    "CompletionNote" varchar(1000), "SelectedWorkforceProfileId" uuid, "EvidenceStoragePath" varchar(500),
    "EvidenceFileName" varchar(250), "EvidenceContentType" varchar(150), "EvidenceFileSize" bigint,
    "AutomaticCheckedAt" timestamp, "AutomaticResultNote" varchar(250), "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdOnboardingCaseItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdOnboardingCaseItem_HrdOnboardingCase_OnboardingCaseId"
        FOREIGN KEY ("OnboardingCaseId") REFERENCES public."HrdOnboardingCase" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdOnboardingCaseItem_OnboardingCaseId_ItemCode" ON public."HrdOnboardingCaseItem" ("OnboardingCaseId", "ItemCode") WHERE "IsDelete" = false;

-- MstOnboardingTemplate dan MstOnboardingTemplateTask: kolom pada 10.4 dan 10.5 dibuang
-- (dengan pengaman tabel kosong), lalu:
ALTER TABLE public."MstOnboardingTemplate" ADD "TemplateLayer" integer NOT NULL;
CREATE UNIQUE INDEX "IX_MstOnboardingTemplate_TemplateLayer_Active" ON public."MstOnboardingTemplate" ("TemplateLayer")
    WHERE "IsActive" = true AND "IsDelete" = false AND "TemplateLayer" IN (1, 2);
CREATE UNIQUE INDEX "IX_MstOnboardingTemplate_OrganizationUnitId_Active" ON public."MstOnboardingTemplate" ("OrganizationUnitId")
    WHERE "IsActive" = true AND "IsDelete" = false AND "TemplateLayer" = 3;
ALTER TABLE public."MstOnboardingTemplateTask"
    ADD "ResponsibleRole" integer NOT NULL, ADD "ItemKind" integer NOT NULL, ADD "AutomaticSource" integer,
    ADD "CertificationTypeId" uuid REFERENCES public."MstCertificationType" ("Id") ON DELETE RESTRICT,
    ADD "SystemItemCode" varchar(40);
CREATE UNIQUE INDEX "IX_MstOnboardingTemplateTask_TemplateId_TaskCode" ON public."MstOnboardingTemplateTask" ("OnboardingTemplateId", "TaskCode") WHERE "IsDelete" = false;

CREATE TABLE public."HrdOnboardingAccessRequest" (
    "Id" uuid NOT NULL, "OnboardingCaseId" uuid NOT NULL, "WorkforceProfileId" uuid NOT NULL,
    "RequestType" integer NOT NULL, "RequestStatus" integer NOT NULL, "ApplicationUserId" uuid,
    "FailureMessage" varchar(500), "SelfServiceActivatedAt" timestamp, "RequestedAt" timestamp NOT NULL,
    "CompletedAt" timestamp, "CompletedByUserId" uuid, "CompletionNote" varchar(500),
    CONSTRAINT "PK_HrdOnboardingAccessRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdOnboardingAccessRequest_HrdOnboardingCase_OnboardingCaseId"
        FOREIGN KEY ("OnboardingCaseId") REFERENCES public."HrdOnboardingCase" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdOnboardingAccessRequest_Case_Type" ON public."HrdOnboardingAccessRequest" ("OnboardingCaseId", "RequestType") WHERE "IsDelete" = false;

ALTER TABLE public."WfpContractHistory"
    ADD "SourceDocumentType" varchar(50), ADD "SourceDocumentId" uuid, ADD "FixedTermSeriesStartDate" timestamp,
    ADD "DocumentFileName" varchar(250), ADD "DocumentContentType" varchar(150), ADD "SignedRecordedAt" timestamp,
    ADD "SignedRecordedByUserId" uuid, ADD "RowVersion" uuid NOT NULL DEFAULT gen_random_uuid();
CREATE INDEX "IX_WfpContractHistory_SourceDocument" ON public."WfpContractHistory" ("SourceDocumentType", "SourceDocumentId");

CREATE TABLE public."HrdContractDecision" (
    "Id" uuid NOT NULL, "DecisionNumber" varchar(30) NOT NULL, "ContractHistoryId" uuid NOT NULL,
    "WorkforceProfileId" uuid NOT NULL, "DecisionType" integer NOT NULL, "ExtensionMonths" integer,
    "ProposedStartDate" date NOT NULL, "ProposedEndDate" date, "ReasonText" varchar(2000), -- SENSITIF
    "DecisionStatus" integer NOT NULL, "SubmittedByUserId" uuid NOT NULL, "SubmittedAt" timestamp NOT NULL,
    "WorkflowInstanceId" uuid, "DecidedByUserId" uuid, "DecidedAt" timestamp, "DecisionNote" varchar(1000), -- SENSITIF
    "FollowUpContractHistoryId" uuid, "EmployeeNotifiedOn" date, "NotificationRecordedByUserId" uuid,
    "EmployeeSeparationId" uuid, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdContractDecision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdContractDecision_WfpContractHistory_ContractHistoryId"
        FOREIGN KEY ("ContractHistoryId") REFERENCES public."WfpContractHistory" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdContractDecision_DecisionNumber" ON public."HrdContractDecision" ("DecisionNumber");
CREATE UNIQUE INDEX "IX_HrdContractDecision_ContractHistoryId_Open" ON public."HrdContractDecision" ("ContractHistoryId")
    WHERE "IsDelete" = false AND "DecisionStatus" IN (1, 2, 4);

CREATE TABLE public."HrdContractCorrection" (
    "Id" uuid NOT NULL, "CorrectionNumber" varchar(30) NOT NULL, "ContractHistoryId" uuid NOT NULL,
    "BeforeContractNumber" varchar(100) NOT NULL, "BeforeContractTypeId" uuid NOT NULL, "BeforeStartDate" date NOT NULL,
    "BeforeEndDate" date, "BeforeDocumentPath" varchar(500), "AfterContractNumber" varchar(100) NOT NULL,
    "AfterContractTypeId" uuid NOT NULL, "AfterStartDate" date NOT NULL, "AfterEndDate" date,
    "ReasonText" varchar(2000) NOT NULL, -- SENSITIF
    "EvidenceStoragePath" varchar(500) NOT NULL, "EvidenceFileName" varchar(250) NOT NULL,
    "EvidenceContentType" varchar(150) NOT NULL, "CorrectionStatus" integer NOT NULL,
    "SubmittedByUserId" uuid NOT NULL, "SubmittedAt" timestamp NOT NULL, "WorkflowInstanceId" uuid,
    "DecidedByUserId" uuid, "DecidedAt" timestamp, "DecisionNote" varchar(1000), "AppliedAt" timestamp,
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdContractCorrection" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdContractCorrection_WfpContractHistory_ContractHistoryId"
        FOREIGN KEY ("ContractHistoryId") REFERENCES public."WfpContractHistory" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdContractCorrection_ContractHistoryId_Open" ON public."HrdContractCorrection" ("ContractHistoryId")
    WHERE "IsDelete" = false AND "CorrectionStatus" IN (1, 2);

CREATE TABLE public."HrdInitialContractBatch" (
    "Id" uuid NOT NULL, "BatchNumber" varchar(30) NOT NULL, "HospitalSiteId" uuid NOT NULL,
    "BatchStatus" integer NOT NULL, "TotalRows" integer NOT NULL DEFAULT 0, "ValidRows" integer NOT NULL DEFAULT 0,
    "InvalidRows" integer NOT NULL DEFAULT 0, "SubmittedByUserId" uuid, "SubmittedAt" timestamp,
    "WorkflowInstanceId" uuid, "DecidedByUserId" uuid, "DecidedAt" timestamp, "AppliedAt" timestamp,
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdInitialContractBatch" PRIMARY KEY ("Id")
);
CREATE TABLE public."HrdInitialContractBatchRow" (
    "Id" uuid NOT NULL, "BatchId" uuid NOT NULL, "WorkforceProfileId" uuid NOT NULL, "ContractTypeId" uuid NOT NULL,
    "ContractNumber" varchar(100) NOT NULL, "StartDate" date NOT NULL, "EndDate" date,
    "FixedTermSeriesStartDate" date, "IsValid" boolean NOT NULL DEFAULT false, "ValidationMessage" varchar(500),
    "CreatedContractHistoryId" uuid,
    CONSTRAINT "PK_HrdInitialContractBatchRow" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdInitialContractBatchRow_HrdInitialContractBatch_BatchId"
        FOREIGN KEY ("BatchId") REFERENCES public."HrdInitialContractBatch" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdInitialContractBatchRow_BatchId_WorkforceProfileId" ON public."HrdInitialContractBatchRow" ("BatchId", "WorkforceProfileId") WHERE "IsDelete" = false;

CREATE TABLE public."HrdProbation" (
    "Id" uuid NOT NULL, "WorkforceProfileId" uuid NOT NULL, "ContractHistoryId" uuid NOT NULL,
    "StartDate" date NOT NULL, "EndDate" date NOT NULL, "ReminderFromDate" date NOT NULL,
    "ProbationStatus" integer NOT NULL, "ClosedAt" timestamp, "ClosedByUserId" uuid, "EmployeeSeparationId" uuid,
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdProbation" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdProbation_WfpContractHistory_ContractHistoryId"
        FOREIGN KEY ("ContractHistoryId") REFERENCES public."WfpContractHistory" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdProbation_ContractHistoryId" ON public."HrdProbation" ("ContractHistoryId") WHERE "IsDelete" = false;
CREATE INDEX "IX_HrdProbation_Status_EndDate" ON public."HrdProbation" ("ProbationStatus", "EndDate");

CREATE TABLE public."HrdProbationAssessment" (
    "Id" uuid NOT NULL, "AssessmentNumber" varchar(30) NOT NULL, "ProbationId" uuid NOT NULL,
    "Result" integer NOT NULL, "ReasonText" varchar(2000) NOT NULL, -- SENSITIF
    "EvidenceStoragePath" varchar(500), "EvidenceFileName" varchar(250), "EvidenceContentType" varchar(150),
    "AssessmentStatus" integer NOT NULL, "SubmittedByUserId" uuid NOT NULL, "SubmittedAt" timestamp NOT NULL,
    "WorkflowInstanceId" uuid, "DecidedByUserId" uuid, "DecidedAt" timestamp, "DecisionNote" varchar(1000), -- SENSITIF
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdProbationAssessment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdProbationAssessment_HrdProbation_ProbationId"
        FOREIGN KEY ("ProbationId") REFERENCES public."HrdProbation" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdProbationAssessment_ProbationId_Open" ON public."HrdProbationAssessment" ("ProbationId")
    WHERE "IsDelete" = false AND "AssessmentStatus" IN (1, 2);

CREATE TABLE public."HrdClinicalPreceptorship" (
    "Id" uuid NOT NULL, "OnboardingCaseId" uuid NOT NULL, "WorkforceProfileId" uuid NOT NULL,
    "OrganizationUnitId" uuid NOT NULL, "PreceptorshipStatus" integer NOT NULL, "AccompaniedSince" date,
    "IndependentOn" date, "DeclaredByUserId" uuid, "EvidenceStoragePath" varchar(500),
    "EvidenceFileName" varchar(250), "EvidenceContentType" varchar(150), "AssessmentNote" varchar(2000), -- SENSITIF
    "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdClinicalPreceptorship" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdClinicalPreceptorship_HrdOnboardingCase_OnboardingCaseId"
        FOREIGN KEY ("OnboardingCaseId") REFERENCES public."HrdOnboardingCase" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdClinicalPreceptorship_OnboardingCaseId" ON public."HrdClinicalPreceptorship" ("OnboardingCaseId") WHERE "IsDelete" = false;

CREATE TABLE public."HrdClinicalPreceptorshipPreceptor" (
    "Id" uuid NOT NULL, "PreceptorshipId" uuid NOT NULL, "PreceptorWorkforceProfileId" uuid NOT NULL,
    "AssignedFrom" date NOT NULL, "AssignedUntil" date, "AssignedByUserId" uuid NOT NULL, "ChangeReason" varchar(500),
    CONSTRAINT "PK_HrdClinicalPreceptorshipPreceptor" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_HrdClinicalPreceptorshipPreceptor_Preceptorship"
        FOREIGN KEY ("PreceptorshipId") REFERENCES public."HrdClinicalPreceptorship" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_HrdClinicalPreceptorshipPreceptor_Open" ON public."HrdClinicalPreceptorshipPreceptor" ("PreceptorshipId")
    WHERE "IsDelete" = false AND "AssignedUntil" IS NULL;

CREATE TABLE public."HrdEmploymentLifecycleSetting" (
    "Id" uuid NOT NULL, "HospitalSiteId" uuid NOT NULL, "ContractReminderDays" integer NOT NULL DEFAULT 60,
    "ContractEscalationDays" integer NOT NULL DEFAULT 14, "ProbationReminderDays" integer NOT NULL DEFAULT 14,
    "LateStartFlagDays" integer NOT NULL DEFAULT 3, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_HrdEmploymentLifecycleSetting" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdEmploymentLifecycleSetting_HospitalSiteId" ON public."HrdEmploymentLifecycleSetting" ("HospitalSiteId") WHERE "IsDelete" = false;

ALTER TABLE public."MstContractType" ADD "EmploymentTypeId" uuid REFERENCES public."MstEmploymentType" ("Id") ON DELETE RESTRICT;
ALTER TABLE public."HrdJobOffer" ADD "ContractTypeId" uuid REFERENCES public."MstContractType" ("Id") ON DELETE RESTRICT,
    ADD "ContractDurationMonths" integer;
```


---

## 11. Slice `S-B4` — Penjadwalan, Roster, dan On-Call, sub-modul 06 (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |

### 11.1 Ringkasan status tabel `S-B4`

| Nama tabel | Schema | Status | Modul pemilik | Tanggung jawab |
| --- | --- | --- | --- | --- |
| `HrdSchedulingSetting` | `public` | `Baru` | HumanResource (06) | Parameter korporat penjadwalan & batas toleransi |
| `TrxRosterPeriod` | `public` | `Diperbarui` | HumanResource (06) | Periode roster unit bulanan |
| `TrxShiftAssignment` | `public` | `Diperbarui` | HumanResource (06) | Penugasan shift harian per pegawai |
| `TrxShiftReplacement` | `public` | `Diperbarui` | HumanResource (06) | Penggantian shift darurat langsung |
| `TrxOnCallAssignment` | `public` | `Diperbarui` | HumanResource (06) | Penugasan siaga & pemanggilan cito |
| `TrxRosterAssignment` | `public` | `Sudah ada` | HumanResource (06) | Daftar staf eligible per periode roster |
| `MstShift` | `public` | `Sudah ada` | HumanResource (01) | Definisi jam shift |
| `MstShiftPattern` | `public` | `Sudah ada` | HumanResource (01) | Pola shift berulang |
| `MstOnCallType` | `public` | `Sudah ada` | HumanResource (01) | Jenis siaga & target SLA |

### 11.2 Tabel `Baru` — seluruh kolom

#### 11.2.1 `HrdSchedulingSetting`
Menyimpan parameter konfigurasi aturan waktu kerja rumah sakit (`HRD-DEC-099`, `HRD-DEC-102`).

| Nama kolom | Tipe | Wajib | Nilai bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `gen_random_uuid()` | PK | Tidak |
| `HospitalSiteId` | `uuid` | Ya | — | FK ke HospitalSite, unique constraint | Tidak |
| `MinRestHoursBetweenShifts` | `integer` | Ya | `11` | Jeda istirahat minimal antar shift (jam) (`W1`) | Tidak |
| `MaxConsecutiveWorkDays` | `integer` | Ya | `6` | Batas hari kerja berturut-turut tanpa libur (`W2`) | Tidak |
| `MaxConsecutiveNightShifts` | `integer` | Ya | `2` | Batas shift malam berurutan maksimal (`W3`) | Tidak |
| `RetroactiveCorrectionWindowDays` | `integer` | Ya | `3` | Batas maksimal hari koreksi mundur mandiri unit (`HRD-DEC-102`) | Tidak |
| `RestHoursEscalatedToBlock` | `boolean` | Ya | `false` | True jika aturan jeda dinaikkan menjadi memblokir (`B`) | Tidak |
| `ConsecutiveDaysEscalatedToBlock` | `boolean` | Ya | `false` | True jika aturan hari berurutan dinaikkan memblokir | Tidak |
| `NightShiftsEscalatedToBlock` | `boolean` | Ya | `false` | True jika aturan malam berurutan dinaikkan memblokir | Tidak |
| `RowVersion` | `uuid` | Ya | `gen_random_uuid()` | Optimistic concurrency token | Tidak |
| *(Kolom IdentityModel)* | — | — | — | 10 kolom audit standar (`CreatedBy`, `CreatedAt`, dll.) | Tidak |

### 11.3 Tabel `Diperbarui` — seluruh kolom

#### 11.3.1 `TrxRosterPeriod`

| Nama kolom | Tipe | Wajib | Nilai bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `gen_random_uuid()` | PK | Tidak |
| `PeriodCode` | `varchar(50)` | Ya | — | Kode unik periode (format `RST-YYYYMM-UNITCODE`), aman konkurensi | Tidak |
| `PeriodName` | `varchar(150)` | Ya | — | Nama deskriptif periode (misal "Roster ICU Februari 2027") | Tidak |
| `UnitId` | `uuid` | Ya | — | FK ke Master Organization/Unit kerja | Tidak |
| `StartDate` | `date` | Ya | — | Tanggal awal periode (biasanya tanggal 1) | Tidak |
| `EndDate` | `date` | Ya | — | Tanggal akhir periode (tanggal terakhir bulan) | Tidak |
| `Status` | `varchar(30)` | Ya | `'Draft'` | Kosakata: `Draft`, `Validation`, `Published`, `Closed` | Tidak |
| `TotalStaffCount` | `integer` | Ya | `0` | Jumlah staf yang dijadwalkan | Tidak |
| `BlockingConflictCount` | `integer` | Ya | `0` | Jumlah pelanggaran memblokir (`B1`–`B7`) | Tidak |
| `WarningCount` | `integer` | Ya | `0` | Jumlah peringatan terdeteksi (`W1`–`W7`) | Tidak |
| `IsRecheckPassed` | `boolean` | Ya | `false` | Status lolos pemeriksaan ulang sebelum terbit (`HRD-DEC-098`) | Tidak |
| `PublishedByUserId` | `varchar(450)` | Tidak | `null` | User ID Kepala Unit yang menerbitkan | Tidak |
| `PublishedAt` | `timestamptz` | Tidak | `null` | Waktu penerbitan roster | Tidak |
| `ClosedByUserId` | `varchar(450)` | Tidak | `null` | User ID HRD Payroll yang menutup periode (`HRD-DEC-102`) | Tidak |
| `ClosedAt` | `timestamptz` | Tidak | `null` | Waktu penguncian mutlak periode | Tidak |
| *(Kolom IdentityModel)* | — | — | — | 10 kolom audit standar | Tidak |

#### 11.3.2 `TrxShiftAssignment`

| Nama kolom | Tipe | Wajib | Nilai bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `gen_random_uuid()` | PK | Tidak |
| `RosterPeriodId` | `uuid` | Ya | — | FK ke `TrxRosterPeriod` (ON DELETE CASCADE) | Tidak |
| `EmployeeId` | `uuid` | Ya | — | FK ke `MstWorkforceProfile` | Tidak |
| `ShiftDate` | `date` | Ya | — | Tanggal dinas shift | Tidak |
| `ShiftId` | `uuid` | Ya | — | FK ke `MstShift` | Tidak |
| `ShiftCode` | `varchar(30)` | Ya | — | Kode shift (P, S, M, L) | Tidak |
| `StartTime` | `time` | Ya | — | Jam mulai shift | Tidak |
| `EndTime` | `time` | Ya | — | Jam selesai shift | Tidak |
| `Status` | `varchar(30)` | Ya | `'Scheduled'` | Kosakata: `Draft`, `Scheduled`, `Published`, `Completed`, `Cancelled` | Tidak |
| `IsManualOverride` | `boolean` | Ya | `false` | True bila shift ini hasil Ganti Petugas / penyesuaian | Tidak |
| `OverrideReason` | `varchar(500)` | Tidak | `null` | Alasan pergantian staf wajib terisi jika override (`HRD-DEC-101`) | Tidak |
| `OriginalEmployeeId` | `uuid` | Tidak | `null` | FK ke pegawai asal yang digantikan bila mutasi | Tidak |
| `IsAccompanied` | `boolean` | Ya | `false` | True bila staf berstatus Didampingi (`HRD-DEC-087`, `HRD-DEC-100`) | Tidak |
| `PreceptorEmployeeId` | `uuid` | Tidak | `null` | FK ke pegawai preceptor pendamping | Tidak |
| `Notes` | `varchar(500)` | Tidak | `null` | Catatan penugasan | Tidak |
| *(Kolom IdentityModel)* | — | — | — | 10 kolom audit standar | Tidak |

#### 11.3.3 `TrxShiftReplacement`

| Nama kolom | Tipe | Wajib | Nilai bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `gen_random_uuid()` | PK | Tidak |
| `ShiftAssignmentId` | `uuid` | Ya | — | FK ke `TrxShiftAssignment` | Tidak |
| `RosterPeriodId` | `uuid` | Ya | — | FK ke `TrxRosterPeriod` | Tidak |
| `OriginalEmployeeId` | `uuid` | Ya | — | FK ke staf yang berhalangan hadir | Tidak |
| `ReplacementEmployeeId` | `uuid` | Ya | — | FK ke staf pengganti yang ditugaskan | Tidak |
| `ShiftDate` | `date` | Ya | — | Tanggal dinas penggantian | Tidak |
| `ShiftId` | `uuid` | Ya | — | FK ke `MstShift` | Tidak |
| `ReasonCategory` | `varchar(100)` | Ya | — | Kategori alasan: `Sakit Mendadak`, `Izin/Keluarga`, `Lonjakan Pasien`, dll. | Tidak |
| `Notes` | `varchar(500)` | Tidak | `null` | Catatan detail alasan penggantian | Tidak |
| `Status` | `varchar(30)` | Ya | `'Applied'` | Status mutasi (`Applied`, `Reverted`) | Tidak |
| `ExecutedByUserId` | `varchar(450)` | Ya | — | User ID Kepala Unit yang mengeksekusi langsung | Tidak |
| `ExecutedAt` | `timestamptz` | Ya | `now()` | Waktu mutasi dieksekusi | Tidak |
| *(Kolom IdentityModel)* | — | — | — | 10 kolom audit standar | Tidak |

#### 11.3.4 `TrxOnCallAssignment`

| Nama kolom | Tipe | Wajib | Nilai bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `gen_random_uuid()` | PK | Tidak |
| `EmployeeId` | `uuid` | Ya | — | FK ke `MstWorkforceProfile` (Dokter / Staf) | Tidak |
| `EmployeeName` | `varchar(200)` | Ya | — | Nama tenaga medis siaga | Tidak |
| `ProfessionGroup` | `varchar(50)` | Ya | — | Dokter Spesialis / Perawat / Penunjang Medis | Tidak |
| `UnitId` | `uuid` | Ya | — | FK ke Unit kerja cakupan siaga | Tidak |
| `OnCallTypeId` | `uuid` | Ya | — | FK ke `MstOnCallType` | Tidak |
| `OnCallTypeCode` | `varchar(50)` | Ya | — | Kode jenis siaga | Tidak |
| `StartTime` | `timestamptz` | Ya | — | Waktu mulai periode siaga standby | Tidak |
| `EndTime` | `timestamptz` | Ya | — | Waktu selesai periode siaga standby | Tidak |
| `Status` | `varchar(30)` | Ya | `'Scheduled'` | Kosakata: `Scheduled`, `Confirmed`, `Active`, `Completed` (`HRD-DEC-103`) | Tidak |
| `ExpectedResponseMinutes` | `integer` | Ya | `30` | Target SLA waktu respon (menit) dari master tipe siaga | Tidak |
| `IsCalledIn` | `boolean` | Ya | `false` | True bila terjadi pemanggilan cito darurat | Tidak |
| `CallInTime` | `timestamptz` | Tidak | `null` | Waktu panggilan cito dilakukan | Tidak |
| `ArrivalTime` | `timestamptz` | Tidak | `null` | Waktu dokter/staf tiba di rumah sakit | Tidak |
| `ResponseMinutes` | `integer` | Tidak | `null` | Selisih menit antara CallInTime dan ArrivalTime | Tidak |
| `IsSlaMet` | `boolean` | Tidak | `null` | True bila ResponseMinutes <= ExpectedResponseMinutes | Tidak |
| `CalledByUnitId` | `uuid` | Tidak | `null` | FK ke Unit yang memanggil (IGD, ICU, Lab, OK) | Tidak |
| `CallNotes` | `varchar(500)` | Tidak | `null` | Catatan indikasi medis / kasus pemanggilan | Tidak |
| *(Kolom IdentityModel)* | — | — | — | 10 kolom audit standar | Tidak |

### 11.4 Skema DDL tabel `S-B4`

```sql
-- 1. Tabel Konfigurasi Penjadwalan
CREATE TABLE public."HrdSchedulingSettings" (
    "Id" uuid NOT NULL DEFAULT gen_random_uuid(),
    "HospitalSiteId" uuid NOT NULL,
    "MinRestHoursBetweenShifts" integer NOT NULL DEFAULT 11,
    "MaxConsecutiveWorkDays" integer NOT NULL DEFAULT 6,
    "MaxConsecutiveNightShifts" integer NOT NULL DEFAULT 2,
    "RetroactiveCorrectionWindowDays" integer NOT NULL DEFAULT 3,
    "RestHoursEscalatedToBlock" boolean NOT NULL DEFAULT false,
    "ConsecutiveDaysEscalatedToBlock" boolean NOT NULL DEFAULT false,
    "NightShiftsEscalatedToBlock" boolean NOT NULL DEFAULT false,
    "RowVersion" uuid NOT NULL DEFAULT gen_random_uuid(),
    "CreatedBy" varchar(450) NOT NULL,
    "CreatedAt" timestamptz NOT NULL DEFAULT now(),
    "UpdatedBy" varchar(450),
    "UpdatedAt" timestamptz,
    "IsDelete" boolean NOT NULL DEFAULT false,
    "DeletedBy" varchar(450),
    "DeletedAt" timestamptz,
    CONSTRAINT "PK_HrdSchedulingSettings" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_HrdSchedulingSettings_HospitalSiteId" ON public."HrdSchedulingSettings" ("HospitalSiteId") WHERE "IsDelete" = false;

-- 2. Alter Table Penambahan Kolom pada TrxRosterPeriod
ALTER TABLE public."TrxRosterPeriod" 
    ADD COLUMN IF NOT EXISTS "IsRecheckPassed" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "PublishedByUserId" varchar(450),
    ADD COLUMN IF NOT EXISTS "PublishedAt" timestamptz,
    ADD COLUMN IF NOT EXISTS "ClosedByUserId" varchar(450),
    ADD COLUMN IF NOT EXISTS "ClosedAt" timestamptz;

-- 3. Alter Table Penambahan Kolom pada TrxShiftAssignment
ALTER TABLE public."TrxShiftAssignment" 
    ADD COLUMN IF NOT EXISTS "IsManualOverride" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "OverrideReason" varchar(500),
    ADD COLUMN IF NOT EXISTS "OriginalEmployeeId" uuid REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT,
    ADD COLUMN IF NOT EXISTS "IsAccompanied" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "PreceptorEmployeeId" uuid REFERENCES public."MstWorkforceProfile" ("Id") ON DELETE RESTRICT;

-- 4. Alter Table Penambahan Kolom pada TrxShiftReplacement
ALTER TABLE public."TrxShiftReplacement" 
    ADD COLUMN IF NOT EXISTS "ReasonCategory" varchar(100) NOT NULL DEFAULT 'Sakit Mendadak',
    ADD COLUMN IF NOT EXISTS "Notes" varchar(500),
    ADD COLUMN IF NOT EXISTS "ExecutedByUserId" varchar(450) NOT NULL DEFAULT 'SYSTEM',
    ADD COLUMN IF NOT EXISTS "ExecutedAt" timestamptz NOT NULL DEFAULT now();

-- 5. Alter Table Penambahan Kolom pada TrxOnCallAssignment
ALTER TABLE public."TrxOnCallAssignment" 
    ADD COLUMN IF NOT EXISTS "ExpectedResponseMinutes" integer NOT NULL DEFAULT 30,
    ADD COLUMN IF NOT EXISTS "IsCalledIn" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "CallInTime" timestamptz,
    ADD COLUMN IF NOT EXISTS "ArrivalTime" timestamptz,
    ADD COLUMN IF NOT EXISTS "ResponseMinutes" integer,
    ADD COLUMN IF NOT EXISTS "IsSlaMet" boolean,
    ADD COLUMN IF NOT EXISTS "CalledByUnitId" uuid,
    ADD COLUMN IF NOT EXISTS "CallNotes" varchar(500);

CREATE INDEX "IX_TrxOnCallAssignment_ActiveStatus" ON public."TrxOnCallAssignment" ("UnitId", "Status", "StartTime", "EndTime") WHERE "IsDelete" = false;
CREATE INDEX "IX_TrxShiftAssignment_DateUnit" ON public."TrxShiftAssignment" ("ShiftDate", "Status") WHERE "IsDelete" = false;
```

### 11.5 Traceability keputusan ke Kamus Data

| Keputusan | Tabel & Kolom yang Menampung |
| --- | --- |
| `HRD-DEC-098` | `TrxRosterPeriod.PublishedByUserId`, `PublishedAt`, `IsRecheckPassed` |
| `HRD-DEC-099` | `HrdSchedulingSetting` (seluruh kolom konfigurasi aturan jeda dan hari berurutan) |
| `HRD-DEC-100` | `TrxShiftAssignment.IsAccompanied`, `PreceptorEmployeeId` |
| `HRD-DEC-101` | `TrxShiftAssignment.IsManualOverride`, `OverrideReason`, `TrxShiftReplacement` (seluruh kolom) |
| `HRD-DEC-102` | `HrdSchedulingSetting.RetroactiveCorrectionWindowDays`, `TrxRosterPeriod.ClosedByUserId`, `ClosedAt` |
| `HRD-DEC-103` | `TrxOnCallAssignment` (`ExpectedResponseMinutes`, `IsCalledIn`, `CallInTime`, `ArrivalTime`, `ResponseMinutes`, `IsSlaMet`) |


---

## 12. Bagian `S-B1` — Kamus Data Administrasi Kehadiran dan Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

Bagian ini mendefinisikan kamus data untuk entitas baru dan perubahan skema pada **Slice `S-B1` — Administrasi Kehadiran dan Presensi Staf Rumah Sakit 24/7**.

### 12.1 Entitas Baru: `HrdEarlyLeavePermission` (Tabel `hrd_early_leave_permissions`)
* **Status:** Baru (`NEW`).
* **Fungsi:** Menyimpan dokumen permohonan izin pulang cepat administratif yang bersifat *non-deductible* (tidak memotong kuota saldo cuti tahunan per `HRD-DEC-107`).

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key unik permohonan izin pulang cepat | Tidak |
| `attendance_daily_id` | `UUID` | Ya | — | FK ke `hrd_attendance_dailies.id` (sesi kehadiran yang diizinkan) | Tidak |
| `workforce_profile_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (pegawai yang mengajukan) | Tidak |
| `requested_early_leave_at` | `TIMESTAMPTZ` | Ya | — | Waktu rencana keluar lebih awal yang diajukan pegawai | Tidak |
| `reason` | `VARCHAR(500)` | Ya | — | Alasan tertulis darurat keluarga atau dinas mendadak | Tidak |
| `status` | `VARCHAR(50)` | Ya | `'Draft'` | Status izin: `Draft`, `Submitted`, `Approved`, `Rejected`, `Waived` | Tidak |
| `approved_by_workforce_id` | `UUID` | Tidak | `NULL` | FK ke `mst_workforce_profiles.id` (atasan langsung penyetuju) | Tidak |
| `approved_at` | `TIMESTAMPTZ` | Tidak | `NULL` | Waktu persetujuan atasan langsung diberikan | Tidak |
| `rejection_reason` | `VARCHAR(500)` | Tidak | `NULL` | Catatan alasan penolakan jika izin ditolak | Tidak |
| `is_deductible` | `BOOLEAN` | Ya | `false` | Menegaskan izin tidak memotong kuota saldo cuti (`HRD-DEC-107`) | Tidak |

### 12.2 Entitas Baru: `HrdOfficialDutyRequest` (Tabel `hrd_official_duty_requests`)
* **Status:** Baru (`NEW`).
* **Fungsi:** Menyimpan data penugasan staf berdinas di luar area fisik gedung rumah sakit (ambulans rujukan, kurir lab/BBLK, pelatihan wajib RS, home care per `HRD-DEC-109`).

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key unik surat tugas dinas luar | Tidak |
| `workforce_profile_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (staf yang ditugaskan) | Tidak |
| `duty_category` | `VARCHAR(50)` | Ya | — | Kategori: `AmbulanceReferral`, `LabCourier`, `TrainingSeminar`, `HomeCare` | Tidak |
| `assignment_order_number` | `VARCHAR(100)` | Ya | — | Nomor surat tugas / nomor referensi perintah dinas | Tidak |
| `start_at` | `TIMESTAMPTZ` | Ya | — | Waktu mulai dinas luar | Tidak |
| `end_at` | `TIMESTAMPTZ` | Ya | — | Waktu berakhir dinas luar (`end_at > start_at`) | Tidak |
| `destination` | `VARCHAR(255)` | Ya | — | Lokasi/instansi tujuan pelaksanaan dinas luar | Tidak |
| `purpose` | `TEXT` | Ya | — | Uraian tugas/tindakan medis/pelatihan yang dilaksanakan | Tidak |
| `latitude` | `DECIMAL(10,8)` | Tidak | `NULL` | Koordinat latitude lokasi tugas saat tap mobile | Ya |
| `longitude` | `DECIMAL(11,8)` | Tidak | `NULL` | Koordinat longitude lokasi tugas saat tap mobile | Ya |
| `attachment_url` | `VARCHAR(500)` | Tidak | `NULL` | Tautan foto selfie/bukti pelaksanaan tugas di lokasi | Tidak |
| `status` | `VARCHAR(50)` | Ya | `'Draft'` | Status tugas: `Draft`, `Submitted`, `Approved`, `Rejected`, `Completed` | Tidak |
| `approved_by_workforce_id` | `UUID` | Tidak | `NULL` | FK ke `mst_workforce_profiles.id` (atasan langsung pemberi tugas) | Tidak |
| `approved_at` | `TIMESTAMPTZ` | Tidak | `NULL` | Waktu persetujuan surat tugas | Tidak |

### 12.3 Entitas Baru: `HrdAttendanceSetting` (Tabel `hrd_attendance_settings`)
* **Status:** Baru (`NEW`).
* **Fungsi:** Menyimpan konfigurasi parameter waktu kerja presensi dan kebijakan penutupan periode per `HRD-DEC-105` dan `HRD-DEC-108`.

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key konfigurasi presensi | Tidak |
| `hospital_code` | `VARCHAR(50)` | Ya | `'RSMMC'` | Kode rumah sakit / unit bisnis | Tidak |
| `grace_period_minutes` | `INT` | Ya | `10` | Toleransi keterlambatan tanpa denda/late (default 10 menit) | Tidak |
| `max_backdated_correction_days`| `INT` | Ya | `3` | Batas hari toleransi koreksi tanggal mundur (default 3 hari) | Tidak |
| `require_attachment_for_correction`| `BOOLEAN` | Ya | `true` | Kewajiban melampirkan berkas bukti saat koreksi | Tidak |
| `auto_convert_incomplete_to_absent`| `BOOLEAN` | Ya | `true` | Konversi otomatis sisa 1 tap menjadi Absent saat tutup periode | Tidak |
| `is_active` | `BOOLEAN` | Ya | `true` | Penanda konfigurasi aktif yang berlaku | Tidak |

### 12.4 Perubahan Tabel Eksisting: `HrdAttendanceDaily` (Tabel `hrd_attendance_dailies`)
* **Status:** Diperbarui (`EXTEND`).
* **Kolom Tambahan:**
  * `early_leave_permission_id` (`UUID`, nullable, FK ke `hrd_early_leave_permissions.id`).
  * `official_duty_request_id` (`UUID`, nullable, FK ke `hrd_official_duty_requests.id`).
  * `incomplete_punch_reason` (`VARCHAR(500)`, nullable, keterangan jika terjadi 1 tap).
  * Penambahan nilai enum status: `IncompletePunch`, `OnDuty`.

### 12.5 Skrip DDL PostgreSQL (Slice `S-B1`)

```sql
-- 1. Tabel Konfigurasi Presensi Rumah Sakit
CREATE TABLE IF NOT EXISTS hrd_attendance_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    hospital_code VARCHAR(50) NOT NULL UNIQUE,
    grace_period_minutes INT NOT NULL DEFAULT 10,
    max_backdated_correction_days INT NOT NULL DEFAULT 3,
    require_attachment_for_correction BOOLEAN NOT NULL DEFAULT true,
    auto_convert_incomplete_to_absent BOOLEAN NOT NULL DEFAULT true,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false
);

-- 2. Tabel Izin Pulang Cepat Non-Deductible
CREATE TABLE IF NOT EXISTS hrd_early_leave_permissions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    attendance_daily_id UUID NOT NULL,
    workforce_profile_id UUID NOT NULL,
    requested_early_leave_at TIMESTAMPTZ NOT NULL,
    reason VARCHAR(500) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    approved_by_workforce_id UUID,
    approved_at TIMESTAMPTZ,
    rejection_reason VARCHAR(500),
    is_deductible BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_early_leave_daily FOREIGN KEY (attendance_daily_id) REFERENCES hrd_attendance_dailies(id),
    CONSTRAINT fk_early_leave_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_early_leave_approver FOREIGN KEY (approved_by_workforce_id) REFERENCES mst_workforce_profiles(id)
);

-- 3. Tabel Surat Tugas Dinas Luar / Ambulans / Home Care
CREATE TABLE IF NOT EXISTS hrd_official_duty_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workforce_profile_id UUID NOT NULL,
    duty_category VARCHAR(50) NOT NULL,
    assignment_order_number VARCHAR(100) NOT NULL,
    start_at TIMESTAMPTZ NOT NULL,
    end_at TIMESTAMPTZ NOT NULL,
    destination VARCHAR(255) NOT NULL,
    purpose TEXT NOT NULL,
    latitude DECIMAL(10,8),
    longitude DECIMAL(11,8),
    attachment_url VARCHAR(500),
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    approved_by_workforce_id UUID,
    approved_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_official_duty_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_official_duty_approver FOREIGN KEY (approved_by_workforce_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT chk_official_duty_time CHECK (end_at > start_at)
);

-- 4. Indeks Kinerja Presensi
CREATE INDEX IF NOT EXISTS idx_early_leave_daily ON hrd_early_leave_permissions(attendance_daily_id);
CREATE INDEX IF NOT EXISTS idx_official_duty_workforce_date ON hrd_official_duty_requests(workforce_profile_id, start_at, end_at);
```


---

## 13. Bagian `S-B2` — Kamus Data Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

Bagian ini mendokumentasikan spesifikasi struktur data persistensi untuk **Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit**, mencakup entitas buku besar saldo atomik `TrxLeaveBalanceLedger`, transaksi pemanggilan dinas darurat `TrxLeaveRecallRequest`, penyesuaian saldo audit `TrxLeaveAdjustment`, serta perluasan kolom `WfpLeaveRequest`.

### 13.1 Tabel Entitas Baru: `TrxLeaveBalanceLedger` (Tabel `trx_leave_balance_ledgers`)
* **Modul Pemilik:** `LeaveManagement` (Sub-Modul 08)
* **Status:** Baru (`NEW`)
* **Kegunaan:** Mencatat setiap peristiwa mutasi saldo cuti secara atomik, transparan, dan tidak dapat dimanipulasi tanpa jejak audit rujukan (`HRD-DEC-110`).

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | :---: | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key unik baris mutasi | Tidak |
| `workforce_profile_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (pegawai pemilik saldo) | Ya |
| `leave_type_id` | `UUID` | Ya | — | FK ke `mst_leave_types.id` (jenis cuti, misal Cuti Tahunan) | Tidak |
| `period_year` | `INT` | Ya | — | Tahun kalender hak cuti (misal 2026) | Tidak |
| `transaction_type` | `VARCHAR(50)` | Ya | — | Jenis transaksi: `Entitlement`, `CarryForward`, `Taken`, `CancellationRestore`, `RecallRestore`, `Forfeiture`, `Adjustment` | Tidak |
| `amount_days` | `DECIMAL(4,1)` | Ya | — | Nominal hari mutasi (+ untuk penambahan, - untuk pemotongan, misal 0.5, 1.0, 12.0) | Tidak |
| `balance_before` | `DECIMAL(4,1)` | Ya | — | Saldo sebelum transaksi dibukukan | Tidak |
| `balance_after` | `DECIMAL(4,1)` | Ya | — | Saldo sesudah transaksi dibukukan | Tidak |
| `transaction_date` | `TIMESTAMPTZ` | Ya | `CURRENT_TIMESTAMP` | Waktu mutasi terjadi secara nyata | Tidak |
| `reference_request_id` | `UUID` | Tidak | `NULL` | FK ke `wfp_leave_requests.id` (bila mutasi berasal dari permohonan/pembatalan cuti) | Tidak |
| `reference_recall_id` | `UUID` | Tidak | `NULL` | FK ke `trx_leave_recall_requests.id` (bila mutasi berasal dari pemulihan recall) | Tidak |
| `reference_adjustment_id` | `UUID` | Tidak | `NULL` | FK ke `trx_leave_adjustments.id` (bila mutasi berasal dari penyesuaian HR) | Tidak |
| `expiry_date` | `TIMESTAMPTZ` | Tidak | `NULL` | Batas kedaluwarsa saldo (khusus carry-forward: 30 Juni tahun berjalan per `HRD-DEC-110`) | Tidak |
| `is_forfeited` | `BOOLEAN` | Ya | `false` | Penanda apakah saldo ini telah hangus akibat kedaluwarsa | Tidak |
| `notes` | `VARCHAR(500)` | Tidak | `NULL` | Keterangan pelengkap mutasi transaksi | Tidak |

### 13.2 Tabel Entitas Baru: `TrxLeaveRecallRequest` (Tabel `trx_leave_recall_requests`)
* **Modul Pemilik:** `LeaveManagement` (Sub-Modul 08)
* **Status:** Baru (`NEW`)
* **Kegunaan:** Mengakomodasi prosedur pemanggilan kembali pegawai cuti saat darurat rumah sakit dengan otorisasi HR Manager dan pemulihan saldo otomatis (`HRD-DEC-113`).

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | :---: | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key permohonan recall | Tidak |
| `recall_number` | `VARCHAR(100)` | Ya | — | Nomor unik surat pemanggilan dinas darurat | Tidak |
| `leave_request_id` | `UUID` | Ya | — | FK ke `wfp_leave_requests.id` (permohonan cuti aktif yang dipanggil) | Tidak |
| `workforce_profile_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (pegawai yang dipanggil kembali) | Ya |
| `effective_recall_date` | `DATE` | Ya | — | Tanggal efektif pegawai wajib mulai bertugas kembali | Tidak |
| `restored_days` | `DECIMAL(4,1)` | Ya | — | Jumlah hari cuti yang belum dinikmati dan dipulihkan ke saldo pegawai | Tidak |
| `emergency_category` | `VARCHAR(50)` | Ya | — | Kategori darurat: `DisasterMascualty`, `EpidemicOutbreak`, `CriticalSurgeIcu`, `Other` | Tidak |
| `recall_reason` | `TEXT` | Ya | — | Alasan urgensi pemanggilan kembali oleh Kepala Unit | Tidak |
| `status` | `VARCHAR(50)` | Ya | `'RequestedByHeadUnit'` | Status alur: `RequestedByHeadUnit`, `ApprovedByHrManager`, `Executed`, `Rejected` | Tidak |
| `requested_by_head_unit_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (Kepala Ruangan penginisiasi) | Tidak |
| `requested_at` | `TIMESTAMPTZ` | Ya | `CURRENT_TIMESTAMP` | Waktu pengajuan recall diajukan | Tidak |
| `approved_by_hr_manager_id` | `UUID` | Tidak | `NULL` | FK ke `mst_workforce_profiles.id` (HR Manager yang menyetujui) | Tidak |
| `approved_at` | `TIMESTAMPTZ` | Tidak | `NULL` | Waktu persetujuan recall diberikan | Tidak |
| `rejection_reason` | `VARCHAR(500)` | Tidak | `NULL` | Alasan penolakan jika recall ditolak HR Manager | Tidak |

### 13.3 Tabel Entitas Baru: `TrxLeaveAdjustment` (Tabel `trx_leave_adjustments`)
* **Modul Pemilik:** `LeaveManagement` (Sub-Modul 08)
* **Status:** Diperbarui (`EXTEND`)
* **Kegunaan:** Satu-satunya jalur resmi koreksi saldo cuti historis pasca-tutup periode payroll dengan kewajiban nomor dokumen audit (`HRD-DEC-115`).

| Nama Kolom | Tipe Data | Wajib | Nilai Bawaan | Keterangan & Validasi | Sensitif |
| --- | --- | :---: | :---: | --- | :---: |
| `id` | `UUID` | Ya | `gen_random_uuid()` | Primary Key transaksi penyesuaian | Tidak |
| `adjustment_number` | `VARCHAR(100)` | Ya | — | Nomor unik transaksi penyesuaian HR | Tidak |
| `workforce_profile_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (pegawai yang disesuaikan saldonya) | Ya |
| `leave_type_id` | `UUID` | Ya | — | FK ke `mst_leave_types.id` (jenis cuti yang disesuaikan) | Tidak |
| `adjustment_days` | `DECIMAL(4,1)` | Ya | — | Besaran hari penyesuaian | Tidak |
| `direction` | `VARCHAR(20)` | Ya | — | Arah penyesuaian: `Increase` (+), `Decrease` (-) | Tidak |
| `audit_document_number` | `VARCHAR(100)` | Ya | — | Nomor dokumen audit resmi/SK Direksi (wajib per `HRD-DEC-115`) | Tidak |
| `reason` | `VARCHAR(500)` | Ya | — | Penjelasan audit mengapa penyesuaian dilakukan | Tidak |
| `adjusted_by_hr_manager_id` | `UUID` | Ya | — | FK ke `mst_workforce_profiles.id` (HR Manager pelaksana) | Tidak |
| `adjusted_at` | `TIMESTAMPTZ` | Ya | `CURRENT_TIMESTAMP` | Waktu eksekusi penyesuaian saldo | Tidak |

### 13.4 Perubahan Tabel Eksisting: `WfpLeaveRequest` (Tabel `wfp_leave_requests`)
* **Status:** Diperbarui (`EXTEND`)
* **Kolom Tambahan:**
  * `has_staffing_coverage_warning` (`BOOLEAN`, default `false`, flag jika cuti melanggar kuota minimal shift `HRD-DEC-111`).
  * `coverage_warning_details` (`TEXT`, nullable, rincian unit, tanggal, shift, dan kuota yang terancam).
  * `coverage_mitigation_notes` (`VARCHAR(500)`, nullable, catatan mitigasi perawat pengganti oleh Kepala Ruangan).
  * `is_doctor_note_verified` (`BOOLEAN`, default `false`, verifikasi keabsahan surat dokter K3RS `HRD-DEC-112`).
  * `doctor_note_faskes_name` (`VARCHAR(255)`, nullable, nama faskes/RS/klinik penerbit surat sakit).
  * `doctor_sip_number` (`VARCHAR(100)`, nullable, nomor Surat Izin Praktik dokter pemeriksa).
  * `is_pending_payroll_reconciliation` (`BOOLEAN`, default `false`, penanda cuti disetujui pasca cut-off payroll `HRD-DEC-114`).
  * `is_payroll_closed` (`BOOLEAN`, default `false`, pengunci mutlak immutabilitas cuti `HRD-DEC-115`).

### 13.5 Tabel Master Eksisting: `MstLeaveType` (Tabel `mst_leave_types`)
* **Status:** Sudah Ada (`REUSE`)
* **Kolom Kunci:** `id`, `type_code`, `type_name`, `is_deductible`, `default_quota_days`, `allow_carry_forward`, `max_carry_forward_days`, `require_medical_certificate`, `medical_certificate_min_days`, `is_active`.

### 13.6 Skrip DDL PostgreSQL (Slice `S-B2`)

```sql
-- 1. Tabel Buku Besar Saldo Cuti (Balance Ledger)
CREATE TABLE IF NOT EXISTS trx_leave_balance_ledgers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    workforce_profile_id UUID NOT NULL,
    leave_type_id UUID NOT NULL,
    period_year INT NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    amount_days DECIMAL(4,1) NOT NULL,
    balance_before DECIMAL(4,1) NOT NULL,
    balance_after DECIMAL(4,1) NOT NULL,
    transaction_date TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reference_request_id UUID,
    reference_recall_id UUID,
    reference_adjustment_id UUID,
    expiry_date TIMESTAMPTZ,
    is_forfeited BOOLEAN NOT NULL DEFAULT false,
    notes VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_ledger_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_ledger_leave_type FOREIGN KEY (leave_type_id) REFERENCES mst_leave_types(id)
);

-- 2. Tabel Pemanggilan Cuti Darurat Rumah Sakit (Leave Recall)
CREATE TABLE IF NOT EXISTS trx_leave_recall_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    recall_number VARCHAR(100) NOT NULL UNIQUE,
    leave_request_id UUID NOT NULL,
    workforce_profile_id UUID NOT NULL,
    effective_recall_date DATE NOT NULL,
    restored_days DECIMAL(4,1) NOT NULL,
    emergency_category VARCHAR(50) NOT NULL,
    recall_reason TEXT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'RequestedByHeadUnit',
    requested_by_head_unit_id UUID NOT NULL,
    requested_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    approved_by_hr_manager_id UUID,
    approved_at TIMESTAMPTZ,
    rejection_reason VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_recall_leave_request FOREIGN KEY (leave_request_id) REFERENCES wfp_leave_requests(id),
    CONSTRAINT fk_recall_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_recall_requester FOREIGN KEY (requested_by_head_unit_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_recall_approver FOREIGN KEY (approved_by_hr_manager_id) REFERENCES mst_workforce_profiles(id)
);

-- 3. Tabel Penyesuaian Saldo Resmi HR (Leave Adjustment)
CREATE TABLE IF NOT EXISTS trx_leave_adjustments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    adjustment_number VARCHAR(100) NOT NULL UNIQUE,
    workforce_profile_id UUID NOT NULL,
    leave_type_id UUID NOT NULL,
    adjustment_days DECIMAL(4,1) NOT NULL,
    direction VARCHAR(20) NOT NULL,
    audit_document_number VARCHAR(100) NOT NULL,
    reason VARCHAR(500) NOT NULL,
    adjusted_by_hr_manager_id UUID NOT NULL,
    adjusted_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_adjustment_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_adjustment_leave_type FOREIGN KEY (leave_type_id) REFERENCES mst_leave_types(id),
    CONSTRAINT fk_adjustment_approver FOREIGN KEY (adjusted_by_hr_manager_id) REFERENCES mst_workforce_profiles(id)
);

-- 4. Kolom Tambahan pada Tabel wfp_leave_requests
ALTER TABLE wfp_leave_requests 
ADD COLUMN IF NOT EXISTS has_staffing_coverage_warning BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN IF NOT EXISTS coverage_warning_details TEXT,
ADD COLUMN IF NOT EXISTS coverage_mitigation_notes VARCHAR(500),
ADD COLUMN IF NOT EXISTS is_doctor_note_verified BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN IF NOT EXISTS doctor_note_faskes_name VARCHAR(255),
ADD COLUMN IF NOT EXISTS doctor_sip_number VARCHAR(100),
ADD COLUMN IF NOT EXISTS is_pending_payroll_reconciliation BOOLEAN NOT NULL DEFAULT false,
ADD COLUMN IF NOT EXISTS is_payroll_closed BOOLEAN NOT NULL DEFAULT false;

-- 5. Indeks Kinerja Modul Cuti
CREATE INDEX IF NOT EXISTS idx_leave_ledger_workforce_year ON trx_leave_balance_ledgers(workforce_profile_id, leave_type_id, period_year);
CREATE INDEX IF NOT EXISTS idx_leave_ledger_expiry ON trx_leave_balance_ledgers(expiry_date, is_forfeited);
CREATE INDEX IF NOT EXISTS idx_leave_recall_leave_request ON trx_leave_recall_requests(leave_request_id);
CREATE INDEX IF NOT EXISTS idx_leave_request_dates ON wfp_leave_requests(workforce_profile_id, start_date, end_date, status);
```


---

## 14. Bagian `S-B3` — Kamus Data Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Kamus data ini mendokumentasikan skema tabel, kolom, tipe data, nullability, indeks, relasi integritas referensial, serta klasifikasi sensitivitas data untuk **Sub-modul 09. Overtime Management (Slice `S-B3`)**. Seluruh tabel mewarisi 10 kolom audit standar (`IdentityModel`).

### 14.1 Ringkasan Entitas & Tabel Database

| Nama Entitas (C# Model) | Nama Tabel (PostgreSQL) | Status | Modul Pemilik | Keterangan & Tujuan Fungsional |
| :--- | :--- | :---: | :---: | :--- |
| `TrxOvertimePlan` | `trx_overtime_plans` | Diperbarui | Overtime (09) | SPKL induk pra-dinas terencana H-1 unit kerja |
| `TrxOvertimePlanDetail` | `trx_overtime_plan_details` | Diperbarui | Overtime (09) | Rincian staf pelaksana dalam satu penugasan lembur kelompok |
| `TrxOvertimeRealization` | `trx_overtime_realizations` | Diperbarui | Overtime (09) | Realisasi jam lembur aktual & klaim darurat medis cito H+1 |
| `TrxOvertimeRealizationDetail`| `trx_overtime_realization_details` | Diperbarui | Overtime (09) | Rincian waktu fisik pelaksanaan lembur per tugas |
| `TrxOvertimeVerification` | `trx_overtime_verifications` | Diperbarui | Overtime (09) | Hasil verifikasi silang mesin presensi & discrepancy logging |
| `TrxCompensatoryTimeOff` | `trx_compensatory_time_offs` | Diperbarui | Overtime (09) | Hak kuota cuti pengganti (CTO) berkedaluwarsa 90 hari |
| `TrxOvertimePeriod` | `trx_overtime_periods` | Diperbarui | Overtime (09) | Periode cut-off lembur bulanan pra-payroll |
| `TrxOvertimeRequestApproval` | `trx_overtime_request_approvals` | Diperbarui | Overtime (09) | Riwayat persetujuan berjenjang & pencegahan self-approval |

---

### 14.2 Spesifikasi Kolom Tabel

#### 1. Tabel `trx_overtime_plans` (SPKL Pra-Dinas Terencana)
*Tabel induk pencatatan Surat Perintah Kerja Lembur terencana H-1.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | — | Tidak | Identifikator unik berkas SPKL |
| `plan_number` | `VARCHAR(100)` | Tidak | Unique | — | Tidak | Nomor registrasi SPKL (misal: `SPKL/2026/11/0089`) |
| `unit_id` | `UUID` | Tidak | Index | Unit Organisasi | Tidak | Unit kerja tempat pekerjaan lembur dilaksanakan |
| `plan_date` | `DATE` | Tidak | Index | — | Tidak | Tanggal rencana dinas lembur |
| `estimated_start_time` | `TIME` | Tidak | — | — | Tidak | Estimasi jam mulai dinas lembur |
| `estimated_end_time` | `TIME` | Tidak | — | — | Tidak | Estimasi jam selesai dinas lembur |
| `total_planned_minutes`| `INT` | Tidak | — | — | Tidak | Estimasi total durasi lembur terencana (menit) |
| `task_description` | `TEXT` | Tidak | — | — | Tidak | Uraian tugas operasional pekerjaan lembur |
| `compensation_type` | `VARCHAR(50)` | Tidak | — | — | Tidak | `FinancialPayout` atau `CompensatoryTimeOff` (`HRD-DEC-118`) |
| `status` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `Draft`, `WaitingApproval`, `Approved`, `Rejected`, `Cancelled` |
| `supervisor_approver_id`| `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Kepala Ruangan / atasan penyetuju operasional |
| `supervisor_approved_at`| `TIMESTAMPTZ` | Ya | — | — | Tidak | Waktu persetujuan Kepala Ruangan |
| `hr_approver_id` | `UUID` | Ya | Index | `mst_workforce_profiles(id)` | Tidak | HR Officer pemeriksa kepatuhan kuota & anggaran |
| `hr_approved_at` | `TIMESTAMPTZ` | Ya | — | — | Tidak | Waktu persetujuan administratif HR Officer |
| `is_emergency_escalation`| `BOOLEAN` | Tidak | — | — | Tidak | True jika membutuhkan otorisasi khusus kuota |
| `weekly_quota_hours_at_plan`| `DECIMAL(5,2)` | Ya | — | — | Tidak | Snapshot akumulasi jam lembur saat form diajukan |

#### 2. Tabel `trx_overtime_realizations` (Realisasi & Klaim Lembur Darurat Cito)
*Tabel pencatatan lembur aktual dan pelaporan darurat tindakan cito medis pasca-dinas.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | — | Tidak | Identifikator unik klaim realisasi lembur |
| `realization_number` | `VARCHAR(100)` | Tidak | Unique | — | Tidak | Nomor registrasi klaim (misal: `OTR/2026/11/0142`) |
| `overtime_plan_id` | `UUID` | Ya | Index | `trx_overtime_plans(id)` | Tidak | Tautan SPKL terencana (null jika klaim cito darurat) |
| `workforce_profile_id` | `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Pegawai/nakes yang melaksanakan dinas lembur |
| `overtime_category` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `Planned` atau `EmergencyCito` (`HRD-DEC-116`) |
| `work_date` | `DATE` | Tidak | Index | — | Tidak | Tanggal pelaksanaan dinas lembur |
| `actual_start_time` | `TIME` | Tidak | — | — | Tidak | Jam mulai dinas lembur aktual |
| `actual_end_time` | `TIME` | Tidak | — | — | Tidak | Jam selesai dinas lembur aktual |
| `actual_claimed_minutes`| `INT` | Tidak | — | — | Tidak | Total durasi lembur fisik yang diklaim (menit) |
| `emergency_clinical_reference_no` | `VARCHAR(100)` | Ya | Index | — | **Ya** | Nomor Rekam Medis / Insiden Darurat Cito (`HRD-DEC-116`) |
| `clinical_action_summary` | `TEXT` | Ya | — | — | **Ya** | Ringkasan tindakan klinis cito medis (dilindungi hak akses) |
| `compensation_type` | `VARCHAR(50)` | Tidak | — | — | Tidak | `FinancialPayout` atau `CompensatoryTimeOff` |
| `status` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `Draft`, `WaitingUnitApproval`, `PendingVerification`, `ApprovedForPayroll`, `Rejected` |
| `is_medical_director_escalation` | `BOOLEAN` | Tidak | — | — | Tidak | True jika melampaui kuota PP 35/2021 (`HRD-DEC-117`) |
| `medical_director_approved_by` | `UUID` | Ya | Index | `mst_workforce_profiles(id)` | Tidak | Direktur Pelayanan Medik pemberi dispensasi |
| `is_retrospective_adjustment` | `BOOLEAN` | Tidak | Index | — | Tidak | True jika disetujui terlambat pasca-cutoff (`HRD-DEC-121`) |

#### 3. Tabel `trx_overtime_verifications` (Verifikasi Silang Biometrik & Discrepancy)
*Tabel pencatatan perbandingan menit presensi biometrik aktual dan mitigasi anomali.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | — | Tidak | Identifikator unik verifikasi presensi lembur |
| `overtime_realization_id`| `UUID` | Tidak | Unique | `trx_overtime_realizations(id)` | Tidak | Tautan entitas klaim realisasi lembur |
| `biometric_check_in` | `TIMESTAMPTZ`| Ya | — | Sub-modul 07 Attendance | Tidak | Jam tap masuk aktual di mesin biometrik |
| `biometric_check_out`| `TIMESTAMPTZ`| Ya | — | Sub-modul 07 Attendance | Tidak | Jam tap keluar aktual di mesin biometrik |
| `biometric_excess_minutes`| `INT` | Tidak | — | — | Tidak | Total kelebihan waktu fisik tercatat mesin absensi |
| `verified_payable_minutes`| `INT` | Tidak | — | — | Tidak | Durasi sah akhir dibayarkan berdasar *min-duration rule* |
| `verification_status` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `AutoVerified`, `FlaggedDiscrepancy`, `ManuallyAdjusted`, `RejectedDiscrepancy` |
| `discrepancy_reason` | `VARCHAR(255)`| Ya | — | — | Tidak | Kategori selisih (`DurationMismatch`, `MissingBiometric`) |
| `manual_adjustment_notes`| `TEXT` | Ya | — | — | Tidak | Catatan evaluasi manual HR Admin atas logbook fisik |
| `verified_by_hr_admin_id`| `UUID` | Ya | Index | `mst_workforce_profiles(id)` | Tidak | HR Admin pemeriksa berkas bukti fisik anomali |
| `verified_at` | `TIMESTAMPTZ`| Ya | — | — | Tidak | Waktu penyelesaian verifikasi presensi |

#### 4. Tabel `trx_compensatory_time_offs` (Hak Cuti Pengganti / CTO)
*Tabel saldo kuota hak libur kompensasi lembur dengan masa kedaluwarsa 3 bulan.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | — | Tidak | Identifikator unik tiket saldo CTO |
| `cto_ticket_number` | `VARCHAR(100)` | Tidak | Unique | — | Tidak | Nomor registrasi CTO (misal: `CTO/2026/11/0034`) |
| `workforce_profile_id` | `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Pegawai penerima hak cuti pengganti |
| `overtime_realization_id`| `UUID` | Tidak | Index | `trx_overtime_realizations(id)` | Tidak | Sumber transaksi klaim lembur sah |
| `granted_hours` | `DECIMAL(5,2)` | Tidak | — | — | Tidak | Total jam kompensasi lembur yang diberikan |
| `granted_days` | `DECIMAL(4,1)` | Tidak | — | — | Tidak | Konversi hari cuti (1 hari = 8 jam kerja) |
| `effective_date` | `DATE` | Tidak | — | — | Tidak | Tanggal mulai hak libur dapat digunakan |
| `expiry_date` | `DATE` | Tidak | Index | — | Tidak | Tanggal kedaluwarsa (H+90 hari / 3 bulan per `HRD-DEC-118`) |
| `remaining_hours` | `DECIMAL(5,2)` | Tidak | — | — | Tidak | Sisa jam CTO yang belum terpakai |
| `status` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `Active`, `FullyUsed`, `Expired` |
| `is_expired` | `BOOLEAN` | Tidak | Index | — | Tidak | True jika sisa jam dihanguskan oleh scheduler |

#### 5. Tabel `trx_overtime_periods` (Periode & Cut-Off Lembur Bulanan)
*Tabel penutupan cut-off transaksi lembur bulanan pra-payroll.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | — | Tidak | Identifikator unik periode cut-off lembur |
| `period_year` | `INT` | Tidak | Index | — | Tidak | Tahun periode penggajian (misal: 2026) |
| `period_month` | `INT` | Tidak | Index | — | Tidak | Bulan periode penggajian (1 s.d. 12) |
| `cutoff_date` | `DATE` | Tidak | — | — | Tidak | Tanggal batas cut-off bulanan (misal: 20 November 2026) |
| `period_status` | `VARCHAR(50)` | Tidak | Index | — | Tidak | `Open`, `CutoffLocked`, `Reconciled`, `Closed` (`HRD-DEC-121`) |
| `total_verified_requests`| `INT` | Tidak | — | — | Tidak | Jumlah total SPKL terverifikasi siap dibayarkan |
| `total_payable_hours` | `DECIMAL(7,2)` | Tidak | — | — | Tidak | Akumulasi jam lembur yang diserahkan ke payroll |
| `total_cto_granted_hours`| `DECIMAL(6,2)` | Tidak | — | — | Tidak | Akumulasi jam yang dikonversi menjadi saldo CTO |
| `is_handoff_executed` | `BOOLEAN` | Tidak | — | — | Tidak | True jika serah terima data ke Payroll 10 telah berhasil |
| `handoff_executed_at` | `TIMESTAMPTZ`| Ya | — | — | Tidak | Waktu eksekusi serah terima data ke Payroll |

---

### 14.3 DDL PostgreSQL Lengkap untuk Sub-Modul 09

```sql
-- DDL PostgreSQL Sub-Modul 09 Overtime Management (Slice S-B3)

CREATE TABLE IF NOT EXISTS trx_overtime_plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_number VARCHAR(100) NOT NULL UNIQUE,
    unit_id UUID NOT NULL,
    plan_date DATE NOT NULL,
    estimated_start_time TIME NOT NULL,
    estimated_end_time TIME NOT NULL,
    total_planned_minutes INT NOT NULL,
    task_description TEXT NOT NULL,
    compensation_type VARCHAR(50) NOT NULL DEFAULT 'FinancialPayout',
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    supervisor_approver_id UUID NOT NULL,
    supervisor_approved_at TIMESTAMPTZ,
    hr_approver_id UUID,
    hr_approved_at TIMESTAMPTZ,
    is_emergency_escalation BOOLEAN NOT NULL DEFAULT false,
    weekly_quota_hours_at_plan DECIMAL(5,2),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_ot_plan_supervisor FOREIGN KEY (supervisor_approver_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_ot_plan_hr FOREIGN KEY (hr_approver_id) REFERENCES mst_workforce_profiles(id)
);

CREATE TABLE IF NOT EXISTS trx_overtime_plan_details (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    overtime_plan_id UUID NOT NULL,
    workforce_profile_id UUID NOT NULL,
    planned_start_time TIME NOT NULL,
    planned_end_time TIME NOT NULL,
    planned_minutes INT NOT NULL,
    individual_task_notes VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'Planned',
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_ot_plan_detail_parent FOREIGN KEY (overtime_plan_id) REFERENCES trx_overtime_plans(id) ON DELETE CASCADE,
    CONSTRAINT fk_ot_plan_detail_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id)
);

CREATE TABLE IF NOT EXISTS trx_overtime_realizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    realization_number VARCHAR(100) NOT NULL UNIQUE,
    overtime_plan_id UUID,
    workforce_profile_id UUID NOT NULL,
    overtime_category VARCHAR(50) NOT NULL DEFAULT 'Planned',
    work_date DATE NOT NULL,
    actual_start_time TIME NOT NULL,
    actual_end_time TIME NOT NULL,
    actual_claimed_minutes INT NOT NULL,
    emergency_clinical_reference_no VARCHAR(100),
    clinical_action_summary TEXT,
    compensation_type VARCHAR(50) NOT NULL DEFAULT 'FinancialPayout',
    status VARCHAR(50) NOT NULL DEFAULT 'Draft',
    is_medical_director_escalation BOOLEAN NOT NULL DEFAULT false,
    medical_director_approved_by UUID,
    is_retrospective_adjustment BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_ot_realization_plan FOREIGN KEY (overtime_plan_id) REFERENCES trx_overtime_plans(id),
    CONSTRAINT fk_ot_realization_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_ot_realization_med_dir FOREIGN KEY (medical_director_approved_by) REFERENCES mst_workforce_profiles(id)
);

CREATE TABLE IF NOT EXISTS trx_overtime_verifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    overtime_realization_id UUID NOT NULL UNIQUE,
    biometric_check_in TIMESTAMPTZ,
    biometric_check_out TIMESTAMPTZ,
    biometric_excess_minutes INT NOT NULL DEFAULT 0,
    verified_payable_minutes INT NOT NULL DEFAULT 0,
    verification_status VARCHAR(50) NOT NULL DEFAULT 'AutoVerified',
    discrepancy_reason VARCHAR(255),
    manual_adjustment_notes TEXT,
    verified_by_hr_admin_id UUID,
    verified_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_ot_verification_realization FOREIGN KEY (overtime_realization_id) REFERENCES trx_overtime_realizations(id) ON DELETE CASCADE,
    CONSTRAINT fk_ot_verification_hr_admin FOREIGN KEY (verified_by_hr_admin_id) REFERENCES mst_workforce_profiles(id)
);

CREATE TABLE IF NOT EXISTS trx_compensatory_time_offs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cto_ticket_number VARCHAR(100) NOT NULL UNIQUE,
    workforce_profile_id UUID NOT NULL,
    overtime_realization_id UUID NOT NULL,
    granted_hours DECIMAL(5,2) NOT NULL,
    granted_days DECIMAL(4,1) NOT NULL DEFAULT 0,
    effective_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    remaining_hours DECIMAL(5,2) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    is_expired BOOLEAN NOT NULL DEFAULT false,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT fk_cto_workforce FOREIGN KEY (workforce_profile_id) REFERENCES mst_workforce_profiles(id),
    CONSTRAINT fk_cto_realization FOREIGN KEY (overtime_realization_id) REFERENCES trx_overtime_realizations(id)
);

CREATE TABLE IF NOT EXISTS trx_overtime_periods (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    period_year INT NOT NULL,
    period_month INT NOT NULL,
    cutoff_date DATE NOT NULL,
    period_status VARCHAR(50) NOT NULL DEFAULT 'Open',
    total_verified_requests INT NOT NULL DEFAULT 0,
    total_payable_hours DECIMAL(7,2) NOT NULL DEFAULT 0,
    total_cto_granted_hours DECIMAL(6,2) NOT NULL DEFAULT 0,
    is_handoff_executed BOOLEAN NOT NULL DEFAULT false,
    handoff_executed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by VARCHAR(100) NOT NULL,
    updated_at TIMESTAMPTZ,
    updated_by VARCHAR(100),
    is_deleted BOOLEAN NOT NULL DEFAULT false,
    CONSTRAINT uq_ot_period_year_month UNIQUE (period_year, period_month)
);

-- Indeks Kinerja Kueri Overtime Management
CREATE INDEX IF NOT EXISTS idx_ot_plan_date_unit ON trx_overtime_plans(plan_date, unit_id, status);
CREATE INDEX IF NOT EXISTS idx_ot_realization_emp_date ON trx_overtime_realizations(workforce_profile_id, work_date, status);
CREATE INDEX IF NOT EXISTS idx_ot_verification_state ON trx_overtime_verifications(verification_status);
CREATE INDEX IF NOT EXISTS idx_cto_emp_expiry ON trx_compensatory_time_offs(workforce_profile_id, expiry_date, is_expired);
CREATE INDEX IF NOT EXISTS idx_ot_period_lookup ON trx_overtime_periods(period_year, period_month, period_status);
```

---

## 15. Bagian `S-B5` — Kamus Data Penggajian Pegawai Rumah Sakit (Sub-modul 10, `last_changed_in` `v14`, 5 November 2026)

Kamus data ini mendokumentasikan skema tabel, kolom, tipe data, nullability, indeks performa, relasi integritas referensial, serta klasifikasi sensitivitas data keuangan untuk **Sub-modul 10. Payroll Management (Slice `S-B5`)**. Seluruh data finansial nominal, potongan pajak, dan PIN sekunder bertanda **Sensitif (Ya)** dan dilarang dicatat pada log umum (*plain-text logger*).

### 15.1 Ringkasan Entitas & Tabel Database

| Nama Entitas (C# Model) | Nama Tabel (PostgreSQL) | Status | Modul Pemilik | Keterangan & Tujuan Fungsional |
| :--- | :--- | :---: | :---: | :--- |
| `TrxPayrollRun` | `trx_payroll_runs` | Diperbarui | Payroll (10) | Siklus agregat penggajian bulanan dan status penguncian data |
| `TrxPayrollItemDetail` | `trx_payroll_item_details` | **Baru** | Payroll (10) | Rincian baris pendapatan tetap/variabel, potongan TER PPh 21, dan BPJS |
| `TrxPayrollVarianceAlert` | `trx_payroll_variance_alerts` | **Baru** | Payroll (10) | Perekaman anomali lonjakan deviasi gaji $\ge 20\%$ dan justifikasi wajib |
| `TrxPayrollApproval` | `trx_payroll_approvals` | **Baru** | Payroll (10) | Jejak audit otorisasi bertingkat 3-jenjang (*Segregation of Duties*) |
| `TrxPayrollHandoffBatch` | `trx_payroll_handoff_batches` | **Baru** | Payroll (10) | Berkas serah terima ke Finance ber-checksum SHA-256 dan protokol rollback |
| `TrxPayrollPayslip` | `trx_payroll_payslips` | Diperbarui | Payroll (10) | Slip gaji digital pegawai ESS terlindungi PIN 6-digit sekunder |
| `HrdPayrollSetting` | `hrd_payroll_settings` | **Baru** | Payroll (10) | Konfigurasi cut-off, ambang deviasi, dan tarif regulasi rumah sakit |

---

### 15.2 Spesifikasi Kolom Tabel

#### 1. Tabel `trx_payroll_runs` (Siklus Penggajian Bulanan)
*Tabel utama aggregate root siklus penggajian bulanan rumah sakit.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik siklus payroll run |
| `run_number` | `VARCHAR(100)` | Tidak | Unique | - | Tidak | Nomor referensi resmi run (misal `PR-202611-001`) |
| `period_id` | `UUID` | Tidak | Index | - | Tidak | Rujukan ke periode kerja yang digaji |
| `period_start_date` | `DATE` | Tidak | - | - | Tidak | Tanggal awal rentang dinas penggajian |
| `period_end_date` | `DATE` | Tidak | - | - | Tidak | Tanggal akhir rentang dinas penggajian |
| `cutoff_date` | `DATE` | Tidak | Index | - | Tidak | Tanggal batas cut-off data (default tgl 20) |
| `run_status` | `VARCHAR(50)` | Tidak | Index | - | Tidak | `Draft`, `Calculating`, `Review`, `WaitingHrReview`, `WaitingFinalApproval`, `ApprovedFinal`, `HandoffReady`, `Closed`, `NeedCorrection` |
| `total_employees` | `INT` | Tidak | - | - | Tidak | Jumlah staf yang diproses gajinya |
| `total_gross_pay` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Total beban gaji kotor seluruh staf RS |
| `total_deductions` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Total potongan pajak, BPJS, dan pinjaman |
| `total_tax_withheld`| `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Akumulasi pemotongan PPh 21 TER |
| `total_bpjs_employer`| `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Beban iuran BPJS porsi pemberi kerja RS |
| `total_bpjs_employee`| `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Potongan iuran BPJS porsi staf |
| `total_net_pay` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Total dana bersih yang diserahkan ke Finance |
| `is_locked` | `BOOLEAN` | Tidak | Index | - | Tidak | True jika sudah otorisasi final (immutable) |
| `locked_at` | `TIMESTAMPTZ` | Ya | - | - | Tidak | Waktu penguncian mutlak oleh Direksi |
| `locked_by_user_id` | `UUID` | Ya | - | `users(id)` | Tidak | Pengguna Direktur yang mengunci run |
| `checksum_sha256` | `VARCHAR(64)` | Ya | - | - | Tidak | Hash integritas berkas serah terima |
| `finance_handoff_status`| `VARCHAR(50)`| Tidak | Index | - | Tidak | `PendingHandoff`, `Dispatched`, `AcknowledgedByFinance`, `PaymentConfirmed`, `RejectedByFinance` |

#### 2. Tabel `trx_payroll_item_details` (Rincian Komponen Gaji Atomik)
*Tabel baris individual transaksi komponen gaji per pegawai.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik item komponen |
| `payroll_run_id` | `UUID` | Tidak | Index | `trx_payroll_runs(id)` | Tidak | Rujukan ke siklus payroll induk |
| `employee_id` | `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Pegawai penerima komponen |
| `component_category`| `VARCHAR(50)`| Tidak | Index | - | Tidak | `FixedEarning`, `VariableEarning`, `StatutoryDeduction`, `VoluntaryDeduction` |
| `component_code` | `VARCHAR(50)` | Tidak | - | - | Tidak | `BASIC`, `POS_ALLOW`, `NIGHT_SHIFT`, `OVERTIME`, `PPH21_TER`, `BPJS_KES`, `BPJS_TK`, `LOAN_DEDUCTION` |
| `component_name` | `VARCHAR(150)`| Tidak | - | - | Tidak | Nama deskriptif komponen |
| `amount` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Nilai nominal rupiah komponen gaji |
| `rate_type` | `VARCHAR(50)` | Tidak | - | - | Tidak | `Fixed`, `Percentage`, `Formula`, `HoursMultiplied` |
| `multiplier` | `NUMERIC(8,2)` | Tidak | - | - | Tidak | Faktor pengali (misal 1.5x / 2.0x lembur) |
| `provenance_source_module`| `VARCHAR(50)`| Tidak| - | - | Tidak | Modul sumber asal (`Attendance07`, `Leave08`, `Overtime09`, `Restitution11`, `SalaryConfig`) |
| `provenance_reference_id`| `VARCHAR(100)`| Ya | - | - | Tidak | Nomor dokumen asal transaksi bukti |
| `tax_classification`| `VARCHAR(50)`| Tidak | - | - | Tidak | Objek pajak PPh 21 atau bukan |
| `is_subject_to_bpjs`| `BOOLEAN` | Tidak | - | - | Tidak | Menjadi dasar perhitungan BPJS atau tidak |

#### 3. Tabel `trx_payroll_variance_alerts` (Deteksi Variansi Anomali Gaji)
*Tabel perekaman deviasi gaji bersih $\ge 20\%$ beserta audit justifikasi formal.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik alert variansi |
| `payroll_run_id` | `UUID` | Tidak | Index | `trx_payroll_runs(id)` | Tidak | Rujukan ke siklus payroll |
| `employee_id` | `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Pegawai yang terdeteksi anomali gaji |
| `previous_net_pay` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Nilai gaji bersih periode bulan lalu |
| `current_net_pay` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Nilai gaji bersih periode bulan ini |
| `variance_percentage`| `NUMERIC(6,2)`| Tidak | - | - | Tidak | Persentase kenaikan/penurunan gaji |
| `variance_amount` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Selisih nominal rupiah absolut |
| `alert_severity` | `VARCHAR(20)` | Tidak | - | - | Tidak | `Warning` (20-49%), `Critical` ($\ge 50\%$) |
| `justification_reason`| `TEXT` | Ya | - | - | Tidak | Penjelasan tertulis wajib Payroll Officer |
| `justified_by_user_id`| `UUID` | Ya | - | `users(id)` | Tidak | Petugas yang mengisi justifikasi |
| `justified_at` | `TIMESTAMPTZ` | Ya | - | - | Tidak | Waktu pengisian justifikasi |
| `is_resolved` | `BOOLEAN` | Tidak | Index | - | Tidak | True jika justifikasi telah lengkap disetujui |

#### 4. Tabel `trx_payroll_approvals` (Audit Persetujuan 3-Jenjang)
*Tabel rekaman otorisasi pemisahan tugas (Segregation of Duties) berjenjang.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik log persetujuan |
| `payroll_run_id` | `UUID` | Tidak | Index | `trx_payroll_runs(id)` | Tidak | Rujukan ke siklus penggajian |
| `approval_tier_level`| `INT` | Tidak | - | - | Tidak | `1` (Officer Review), `2` (Manager Review), `3` (Director Final Authorization) |
| `tier_role_name` | `VARCHAR(50)` | Tidak | - | - | Tidak | Nama peran penilai |
| `approver_user_id` | `UUID` | Tidak | Index | `users(id)` | Tidak | ID pengguna yang melakukan aksi peninjauan |
| `approval_action` | `VARCHAR(50)` | Tidak | - | - | Tidak | `Submitted`, `Approved`, `Rejected`, `RequestedRevision` |
| `notes` | `TEXT` | Ya | - | - | Tidak | Catatan evaluasi penilai |
| `action_at` | `TIMESTAMPTZ` | Tidak | - | - | Tidak | Waktu penandaan persetujuan |
| `ip_address` | `VARCHAR(50)` | Ya | - | - | Tidak | Alamat IP jaringan penilai |

#### 5. Tabel `trx_payroll_handoff_batches` (Berkas Serah Terima Finance & Rollback)
*Tabel serah terima berkas final ke Keuangan dan penanganan penolakan.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik berkas serah terima |
| `payroll_run_id` | `UUID` | Tidak | Unique | `trx_payroll_runs(id)` | Tidak | Rujukan 1-to-1 ke siklus payroll |
| `batch_reference_number`| `VARCHAR(100)`| Tidak | Unique | - | Tidak | Nomor batch pengiriman ke Finance |
| `handoff_status` | `VARCHAR(50)` | Tidak | Index | - | Tidak | `Dispatched`, `AcknowledgedByFinance`, `RejectedByFinance`, `PaymentConfirmed` |
| `bank_disbursement_payload_json`| `JSONB`| Tidak | - | - | **Ya** | Rincian data transfer bank pegawai terenkripsi |
| `integrity_checksum`| `VARCHAR(64)` | Tidak | - | - | Tidak | Checksum SHA-256 untuk verifikasi data anti-tamper |
| `dispatched_at` | `TIMESTAMPTZ` | Tidak | - | - | Tidak | Waktu penyerahan berkas ke Finance |
| `dispatched_by_user_id`| `UUID` | Tidak | - | `users(id)` | Tidak | Petugas HR pengirim berkas |
| `finance_acknowledged_at`| `TIMESTAMPTZ`| Ya | - | - | Tidak | Waktu Finance menerima paket |
| `finance_rejection_reason`| `TEXT` | Ya | - | - | Tidak | Catatan tertulis kendala perbankan dari Finance |
| `finance_rejection_at`| `TIMESTAMPTZ`| Ya | - | - | Tidak | Waktu Finance melakukan penolakan batch |
| `payment_confirmed_at`| `TIMESTAMPTZ`| Ya | - | - | Tidak | Waktu konfirmasi transfer bank sukses |

#### 6. Tabel `trx_payroll_payslips` (Slip Gaji Mandiri ESS Ber-PIN)
*Tabel data rincian slip gaji digital pegawai mandiri.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik slip gaji |
| `payroll_run_id` | `UUID` | Tidak | Index | `trx_payroll_runs(id)` | Tidak | Rujukan ke run siklus payroll |
| `employee_id` | `UUID` | Tidak | Index | `mst_workforce_profiles(id)` | Tidak | Pemilik slip gaji |
| `payslip_number` | `VARCHAR(100)`| Tidak | Unique | - | Tidak | Nomor resmi slip gaji digital |
| `gross_amount` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Total pendapatan kotor pegawai |
| `total_deductions` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Total potongan pajak dan BPJS |
| `net_amount` | `NUMERIC(18,2)`| Tidak | - | - | **Ya** | Uang bersih yang diterima di rekening |
| `payslip_status` | `VARCHAR(50)` | Tidak | - | - | Tidak | `Draft`, `Published`, `Archived` |
| `is_released_to_ess`| `BOOLEAN` | Tidak | Index | - | Tidak | True jika sudah dapat diakses di portal ESS |
| `released_at` | `TIMESTAMPTZ` | Ya | - | - | Tidak | Waktu rilis resmi ke portal ESS |
| `secondary_pin_hash`| `VARCHAR(255)`| Ya | - | - | **Ya** | Hash sandi PIN 6-digit pegawai |
| `failed_pin_attempt_count`| `INT` | Tidak | - | - | Tidak | Penghitung kesalahan input PIN (maks 3 kali) |
| `pin_locked_until` | `TIMESTAMPTZ` | Ya | - | - | Tidak | Batas waktu pembekuan akses jika salah PIN |

#### 7. Tabel `hrd_payroll_settings` (Konfigurasi Parameter Penggajian RS)
*Tabel pengaturan operasional parameter penggajian rumah sakit.*

| Nama Kolom | Tipe Data | Nullable | Index / Unique | FK / Rujukan | Sensitif | Keterangan & Aturan |
| :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `id` | `UUID` | Tidak | PK | - | Tidak | Identifikator unik setting |
| `setting_key` | `VARCHAR(100)`| Tidak | Unique | - | Tidak | Kunci konfigurasi sistem |
| `monthly_cutoff_day`| `INT` | Tidak | - | - | Tidak | Tanggal batas cut-off bulanan (default tgl 20) |
| `official_disbursement_day`| `INT` | Tidak | - | - | Tidak | Tanggal transfer gaji resmi (default tgl 25) |
| `variance_threshold_percent`| `NUMERIC(5,2)`| Tidak | - | - | Tidak | Persentase deviasi variansi alert (default 20.00%) |
| `variance_threshold_nominal`| `NUMERIC(18,2)`| Tidak | - | - | Tidak | Ambang deviasi nominal alert (default Rp 2.000.000) |
| `bpjs_health_employer_rate`| `NUMERIC(5,4)`| Tidak | - | - | Tidak | Tarif BPJS Kes beban RS (default 0.0400) |
| `bpjs_health_employee_rate`| `NUMERIC(5,4)`| Tidak | - | - | Tidak | Tarif BPJS Kes beban staf (default 0.0100) |
| `bpjs_tk_jht_employer_rate`| `NUMERIC(5,4)`| Tidak | - | - | Tidak | Tarif JHT beban RS (default 0.0370) |
| `bpjs_tk_jht_employee_rate`| `NUMERIC(5,4)`| Tidak | - | - | Tidak | Tarif JHT beban staf (default 0.0200) |
| `bpjs_tk_jp_cap_amount`| `NUMERIC(18,2)`| Tidak | - | - | Tidak | Plafon maksimal dasar Jaminan Pensiun |
| `is_pin_secondary_auth_required`| `BOOLEAN`| Tidak | - | - | Tidak | Penegakan wajib PIN pada ESS (default true) |
| `is_active` | `BOOLEAN` | Tidak | - | - | Tidak | Status keaktifan parameter |
