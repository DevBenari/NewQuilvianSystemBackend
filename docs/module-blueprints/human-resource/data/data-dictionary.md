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

## 16. Bagian `S-B6` — Kamus Data Restitusi & Pemotongan Penggajian Pegawai Rumah Sakit (Sub-modul 11, `last_changed_in` `v15`, 5 November 2026)

Bagian ini mendefinisikan kamus data terperinci untuk 6 entitas tabel baru yang mengelola **Sub-modul 11 (Restitution & Payroll Deduction Integration — Slice `S-B6`)**, mencakup penegakan integritas data, relasi kunci, validasi bisnis batas 50% PP 36/2021, serta penandaan kolom sensitif.

---

### 16.1 Tabel `public.hrd_restitution_settings` (Status: BARU)

Tabel master pengaturan kebijakan plafon tahunan penggantian biaya (kacamata kerja, faskes luar, dinas darurat) per golongan serta batas maksimal pemotongan upah tenaga kerja.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik konfigurasi pengaturan kebijakan |
| `grade_level_code` | `VARCHAR(50)` | No | - | FK / UQ1 | Tidak | Kode golongan/tingkat pegawai (`GRADE-I`, `GRADE-II`, dll.) atau `GRADE-ALL` |
| `benefit_category` | `VARCHAR(100)` | No | - | UQ1 | Tidak | Kategori manfaat: `KacamataKerja`, `FaskesLuarDarurat`, `DinasCitoMedis`, `KebijakanUmumPemotongan` |
| `annual_limit_amount` | `NUMERIC(15,2)` | No | `0.00` | - | Tidak | Batas maksimal rupiah yang dapat diklaim pegawai dalam satu tahun takwim |
| `minimum_service_months_required`| `INT` | No | `0` | - | Tidak | Syarat masa kerja minimal (dalam bulan) agar pegawai berhak atas manfaat |
| `max_deduction_percentage_of_gross`| `NUMERIC(5,2)` | No | `50.00` | - | Tidak | Batas maksimal persentase potongan upah kotor non-regulasi (Hard-Cap 50% PP 36/2021) |
| `allow_emergency_cito_cashier` | `BOOLEAN` | No | `FALSE` | - | Tidak | Penanda apakah kategori ini boleh dicairkan darurat via loket Kasir RS 24/7 |
| `is_active` | `BOOLEAN` | No | `TRUE` | - | Tidak | Status keaktifan kebijakan konfigurasi |

---

### 16.2 Tabel `public.trx_employee_restitution_claims` (Status: BARU)

Tabel transaksi pengajuan klaim restitusi mandiri staf rumah sakit melalui portal ESS berlampiran dokumen struk kuitansi asli dan riwayat audit persetujuan bertingkat.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik klaim restitusi |
| `claim_number` | `VARCHAR(100)` | No | - | UQ | Tidak | Nomor resmi berkas klaim (Format: `CLM-REST/YYYYMM/NNNN`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID pegawai pemohon klaim (merujuk ke `public.m_employees(id)`) |
| `claim_category` | `VARCHAR(100)` | No | - | - | Tidak | Kategori klaim: `KacamataKerja`, `FaskesLuarDarurat`, `DinasCitoMedis` |
| `expense_date` | `DATE` | No | - | IDX1 | Tidak | Tanggal pengeluaran pada kuitansi asli (maksimal 60 hari kalender lampau) |
| `claim_amount` | `NUMERIC(15,2)` | No | - | - | Ya | Nominal total biaya yang diajukan oleh pegawai |
| `approved_amount` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Nominal yang disetujui HR C&B setelah dipotong sisa kuota plafon |
| `receipt_attachment_url` | `TEXT` | No | - | - | Ya | Tautan URL penyimpanan berkas pindaian/foto kuitansi asli |
| `medical_resume_attachment_url` | `TEXT` | Yes | `NULL` | - | Ya (Medis) | Tautan URL resume medis dokter / resep kacamata (Data Medis Sensitif) |
| `claim_status` | `VARCHAR(50)` | No | `'Draft'` | IDX2 | Tidak | Status siklus klaim: `Draft`, `Submitted`, `SupervisorApproved`, `HrReviewed`, `Approved`, `Rejected`, `QueuedForPayroll`, `ProcessedInPayroll`, `VoucherIssued`, `DisbursedViaCashier`, `Closed` |
| `reason_notes` | `TEXT` | No | - | - | Tidak | Uraian keperluan pengeluaran klaim |
| `rejection_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Alasan tertulis apabila klaim ditolak pada salah satu tahap verifikasi |
| `supervisor_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID atasan langsung pemohon yang menyetujui tahap 1 (Anti-Self-Approval) |
| `supervisor_approved_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu stempel persetujuan atasan langsung |
| `hr_reviewer_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID petugas HR C&B yang memverifikasi keabsahan kuitansi fisik |
| `hr_reviewed_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu stempel verifikasi HR C&B |
| `final_approver_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID Manajer SDM / Direktur yang memberikan otorisasi akhir |
| `final_approved_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu stempel otorisasi akhir |
| `is_cito_emergency` | `BOOLEAN` | No | `FALSE` | - | Tidak | Penanda darurat medis nakes (jalur cepat pencairan kasir 24/7) |

---

### 16.3 Tabel `public.trx_payroll_restitutions` (Status: BARU)

Tabel penampung komponen penyesuaian restitusi yang diinjeksikan ke siklus penggajian bulanan, mencakup pencairan klaim yang disetujui maupun kompensasi koreksi salah potong periode lampau (`PAY-B6`).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik catatan restitusi payroll |
| `restitution_number` | `VARCHAR(100)` | No | - | UQ | Tidak | Nomor registrasi restitusi payroll (Format: `RST-PAY/YYYYMM/NNNN`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID pegawai penerima restitusi (merujuk ke `public.m_employees(id)`) |
| `claim_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID klaim asal pada `trx_employee_restitution_claims` (bila dari klaim ESS) |
| `restitution_type` | `VARCHAR(50)` | No | - | - | Tidak | Jenis restitusi: `BenefitReimbursement` atau `SalaryAdjustmentCorrection` (`HRD-DEC-131`) |
| `amount` | `NUMERIC(15,2)` | No | - | - | Ya | Nominal uang yang dikembalikan ke pegawai pada slip gaji |
| `affected_payroll_run_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID siklus penggajian masa lalu yang mengalami kesalahan pemotongan (Immutable Audit Link) |
| `affected_payslip_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID slip gaji masa lalu yang salah dipotong |
| `target_payroll_run_id` | `UUID` | Yes | `NULL` | FK, IDX2 | Tidak | ID siklus penggajian aktif berjalan tempat dana dikembalikan |
| `status` | `VARCHAR(50)` | No | `'PendingApproval'`| - | Tidak | Status komponen: `PendingApproval`, `Approved`, `IncludedInPayroll`, `Cancelled` |
| `justification` | `TEXT` | No | - | - | Tidak | Penjelasan audit dan dasar hukum pengembalian uang |
| `approved_by_user_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID pejabat otorisasi pengembalian dana |
| `approved_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu stempel persetujuan pengembalian dana |

---

### 16.4 Tabel `public.trx_payroll_deduction_schedules` (Status: BARU)

Tabel master jadwal pemotongan angsuran cicilan pinjaman/kasbon dan piutang staf yang disinkronisasikan dari Departemen Keuangan (*Finance Handoff*).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik jadwal angsuran |
| `schedule_number` | `VARCHAR(100)` | No | - | UQ | Tidak | Nomor registrasi jadwal cicilan (Format: `DED-SCH/YYYYMM/NNNN`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID pegawai yang memiliki kewajiban cicilan |
| `deduction_category` | `VARCHAR(100)` | No | - | - | Tidak | Kategori potongan: `GantiRugiAsetKasir`, `KasbonPinjamanRS`, `CicilanKoperasi`, `IuranPaguyuban` |
| `priority_level` | `INT` | No | - | IDX1 | Tidak | Tingkat prioritas eksekusi berjenjang: 1 (Tertinggi) s.d. 4 (Terendah) (`HRD-DEC-128`) |
| `external_reference_id` | `VARCHAR(100)` | Yes | `NULL` | IDX2 | Tidak | Nomor registrasi piutang/kontrak pinjaman pada modul Finance (Finance AR Code) |
| `total_loan_principal` | `NUMERIC(15,2)` | No | - | - | Ya | Total pokok pinjaman/ganti rugi yang wajib dilunasi |
| `monthly_installment_amount`| `NUMERIC(15,2)` | No | - | - | Ya | Besaran nilai angsuran normal per bulan |
| `total_installment_months` | `INT` | No | `1` | - | Tidak | Total jangka waktu cicilan (tenor dalam satuan bulan) |
| `paid_installment_count` | `INT` | No | `0` | - | Tidak | Jumlah bulan angsuran yang telah berhasil dipotong |
| `remaining_principal` | `NUMERIC(15,2)` | No | - | - | Ya | Sisa pokok pinjaman yang belum lunas |
| `start_date` | `DATE` | No | - | - | Tidak | Tanggal mulai diberlakukannya pemotongan cicilan |
| `end_date` | `DATE` | Yes | `NULL` | - | Tidak | Perkiraan tanggal selesai/lunas |
| `schedule_status` | `VARCHAR(50)` | No | `'Active'` | - | Tidak | Status jadwal: `Active`, `Paused`, `Completed`, `Cancelled`, `ClearedAtSeparation` |
| `auto_carry_over_on_cap_exceeded`| `BOOLEAN` | No | `TRUE` | - | Tidak | Penanda apakah sisa tagihan otomatis ditunda ke bulan depan jika mentok batas 50% |

---

### 16.5 Tabel `public.trx_payroll_deduction_items` (Status: BARU)

Tabel rincian eksekusi pemotongan gaji per siklus penggajian bulanan yang mencatat nilai terjadwal, nilai terpotong aktual, dan nilai yang ditunda akibat pembatasan regulasi.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik baris eksekusi pemotongan |
| `payroll_run_id` | `UUID` | No | - | FK, UQ1 | Tidak | ID siklus penggajian bulanan terkait (merujuk ke `trx_payroll_runs(id)`) |
| `deduction_schedule_id`| `UUID` | No | - | FK, UQ1 | Tidak | ID jadwal cicilan terkait (merujuk ke `trx_payroll_deduction_schedules(id)`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID pegawai yang dipotong gajinya |
| `deduction_category` | `VARCHAR(100)` | No | - | - | Tidak | Salinan kategori pemotongan untuk keperluan reporting cepat |
| `priority_level` | `INT` | No | - | - | Tidak | Salinan tingkat prioritas eksekusi (1 s.d. 4) |
| `scheduled_amount` | `NUMERIC(15,2)` | No | - | - | Ya | Nominal angsuran yang seharusnya ditagihkan pada periode ini |
| `actual_deducted_amount`| `NUMERIC(15,2)`| No | `0.00` | - | Ya | Nominal yang berhasil dipotong secara sah pada slip gaji berjalan |
| `deferred_amount` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Nominal yang ditunda (*carry-over*) karena membentur batas 50% upah kotor |
| `execution_status` | `VARCHAR(50)` | No | `'Pending'`| - | Tidak | Status eksekusi: `Pending`, `DeductedInFull`, `PartiallyDeducted`, `DeferredDueToSalaryCap`, `Cancelled` |
| `deferral_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Keterangan sistem mengapa terjadi pemotongan parsial atau penundaan penuh |
| `executed_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu stempel eksekusi perhitungan oleh mesin penggajian |

---

### 16.6 Tabel `public.trx_cashier_disbursement_vouchers` (Status: BARU)

Tabel voucher pengeluaran kas elektronik untuk pencairan tunai di loket Kasir RS 24/7 bagi staf medis yang mengalami kondisi darurat kecelakaan kerja okupasional (*Cito Emergency*).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik voucher kasir |
| `voucher_number` | `VARCHAR(100)` | No | - | UQ | Tidak | Nomor seri voucher elektronik (Format: `VCH-CSH/YYYYMM/NNNN`) |
| `claim_id` | `UUID` | No | - | FK | Tidak | ID klaim restitusi terkait (merujuk ke `trx_employee_restitution_claims(id)`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID staf nakes penerima dana tunai |
| `voucher_amount` | `NUMERIC(15,2)` | No | - | - | Ya | Besaran nominal uang tunai yang wajib diserahkan kasir |
| `security_token_hash`| `VARCHAR(256)` | No | - | - | Ya (Token) | Nilai hash SHA-256 dari token otorisasi satu kali pakai (OTP) |
| `issued_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penerbitan voucher elektronik |
| `expired_at` | `TIMESTAMPTZ` | No | - | IDX2 | Tidak | Waktu kedaluwarsa voucher (tepat 48 jam sejak `issued_at`) |
| `voucher_status` | `VARCHAR(50)` | No | `'Issued'` | IDX2 | Tidak | Status voucher: `Issued`, `ClaimedAtCashier`, `DisbursedViaCashier`, `Expired`, `Cancelled` |
| `cashed_by_user_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID user petugas kasir yang menyerahkan uang tunai |
| `cashed_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu kasir memvalidasi sistem dan menyerahkan uang |
| `cashier_receipt_number`| `VARCHAR(100)`| Yes | `NULL` | - | Tidak | Nomor bukti tanda terima kasir cetak fisik |
| `notes` | `TEXT` | Yes | `NULL` | - | Tidak | Catatan operasional kasir atau keterangan insiden |


---

## 17. Kamus Data Sub-Modul 12 (Slice S-D3 Benefit Management — Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)

Bagian ini mendokumentasikan spesifikasi kamus data teknis untuk 7 entitas baru pada **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, mencakup struktur kolom, tipe data, relasi kunci, indeks performa, penanda sensitivitas privasi, serta aturan validasi database. Sepuluh kolom standar warisan `IdentityModel` (`id`, `created_by`, `created_at`, `updated_by`, `updated_at`, `is_deleted`, `deleted_by`, `deleted_at`, `tenant_id`, `version`) berlaku untuk seluruh tabel berstatus `Baru` di bawah ini.

---

### 17.1 Tabel `mst_benefit_plans` (Master Paket Manfaat Rumah Sakit)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Master katalog paket manfaat berjenjang 3-lapisan (Regulasi BPJS wajib, Asuransi Swasta Tambahan per grade, dan Subsidi Berobat Internal di RS Quilvian sendiri sesuai `HRD-DEC-134`).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik paket manfaat |
| `plan_code` | `VARCHAR(50)` | No | - | UQ | Tidak | Kode unik paket manfaat (contoh: `BEN-BPJS-STD`, `BEN-PVT-EXEC`) |
| `plan_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama deskriptif paket manfaat |
| `benefit_category` | `VARCHAR(50)` | No | - | IDX1 | Tidak | Kategori: `BPJS_Mandatory`, `Private_Insurance`, `InHouse_HospitalCare` |
| `coverage_tier` | `VARCHAR(50)` | No | - | - | Tidak | Tier perlindungan: `Basic`, `Silver`, `Gold`, `Platinum`, `Executive` |
| `annual_limit_amount` | `NUMERIC(15,2)` | No | `0.00` | - | Tidak | Pagu plafon anggaran tahunan per orang (Rupiah); 0 jika tak terbatas |
| `hospital_discount_percentage`| `NUMERIC(5,2)` | No | `0.00` | - | Tidak | Persentase diskon pelayanan di RS Quilvian sendiri (0.00% s.d. 100.00%) |
| `requires_bpjs_primary_cob` | `BOOLEAN` | No | `true` | - | Tidak | Wajib COB primer BPJS Kesehatan ber-SEP sebelum klaim sekunder |
| `minimum_service_months` | `INT` | No | `3` | - | Tidak | Masa tunggu aktivasi (bulan); 0 untuk BPJS TK, 3 untuk asuransi swasta/RS |
| `is_active` | `BOOLEAN` | No | `true` | IDX2 | Tidak | Penanda keaktifan paket manfaat |

---

### 17.2 Tabel `hrd_benefit_grade_eligibilities` (Matriks Kelayakan Paket per Grade)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Matriks pemetaan kelayakan paket manfaat berdasarkan golongan/grade kepegawaian dan tipe ikatan kerja (PKWT/PKWTT).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik aturan kelayakan |
| `benefit_plan_id` | `UUID` | No | - | FK, UQ1 | Tidak | ID paket manfaat terkait (merujuk ke `mst_benefit_plans(id)`) |
| `grade_level_code` | `VARCHAR(50)` | No | - | UQ1, IDX1 | Tidak | Kode golongan/grade staf (contoh: `GR-01`, `GR-MED-SP`, `GR-DIR`) |
| `employment_type_code` | `VARCHAR(50)`| No | - | UQ1 | Tidak | Status ikatan kerja: `Permanent` (PKWTT), `Contract` (PKWT) |
| `specific_annual_limit`| `NUMERIC(15,2)`| Yes | `NULL` | - | Tidak | Nominal plafon khusus per grade (override limit standar paket jika terisi) |
| `inpatient_room_class` | `VARCHAR(50)` | No | - | - | Tidak | Hak kelas kamar rawat inap: `Kelas 2`, `Kelas 1`, `VIP`, `VVIP` |
| `is_active` | `BOOLEAN` | No | `true` | - | Tidak | Penanda keaktifan aturan eligibilitas |

---

### 17.3 Tabel `trx_employee_insurance_enrollments` (Kepesertaan Asuransi Pegawai)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Pendaftaran kepesertaan asuransi pegawai rumah sakit, menyimpan nomor kartu BPJS dan polis swasta serta siklus aktivasi bertahap.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik kepesertaan asuransi |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID staf nakes terkait (merujuk ke `mst_workforce_profiles(id)`) |
| `benefit_plan_id` | `UUID` | No | - | FK | Tidak | ID paket manfaat yang diikuti (merujuk ke `mst_benefit_plans(id)`) |
| `bpjs_kesehatan_number` | `VARCHAR(50)` | Yes | `NULL` | IDX2 | Ya (BPJS) | Nomor kartu BPJS Kesehatan (13 digit angka) |
| `bpjs_ketenagakerjaan_number`| `VARCHAR(50)`| Yes | `NULL` | IDX3 | Ya (BPJS) | Nomor kartu BPJS Ketenagakerjaan (11 digit angka) |
| `private_insurance_policy_number`|`VARCHAR(100)`| Yes | `NULL` | - | Ya | Nomor polis asuransi komersial tambahan perorangan |
| `private_insurance_provider_name`|`VARCHAR(150)`| Yes | `NULL` | - | Tidak | Nama perusahaan asuransi mitra (misal: Mandiri Inhealth, Prudential) |
| `enrollment_status` | `VARCHAR(50)` | No | `'Draft'` | IDX1 | Tidak | Status: `Draft`, `PendingActivation`, `Active`, `Suspended`, `Terminated` |
| `effective_date` | `TIMESTAMPTZ` | No | - | - | Tidak | Tanggal mulai berlakunya hak benefit |
| `expiry_date` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Tanggal akhir polis asuransi tahunan |
| `termination_date` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Tanggal efektif penonaktifan benefit akibat resign/PHK |
| `termination_reason` | `VARCHAR(255)` | Yes | `NULL` | - | Tidak | Alasan penonaktifan (Resigned, ContractEnd, Dismissed, Deceased) |

---

### 17.4 Tabel `trx_employee_benefit_dependents` (Tanggungan Keluarga Inti Sah)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Menyimpan data anggota keluarga inti sah pegawai (maksimal 1 pasangan + 3 anak) beserta riwayat verifikasi dokumen kependudukan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik tanggungan keluarga |
| `employee_enrollment_id` | `UUID` | No | - | FK, IDX1 | Tidak | ID enrollment asuransi staf terkait |
| `dependent_name` | `VARCHAR(150)` | No | - | - | Ya | Nama lengkap tanggungan sesuai Kartu Keluarga |
| `national_id_number` | `VARCHAR(16)` | No | - | IDX2 | Ya (NIK) | NIK KTP/KIA 16 digit angka valid |
| `relationship_type` | `VARCHAR(50)` | No | - | - | Tidak | Jenis hubungan: `Spouse` (Pasangan Sah), `Child` (Anak Kandung/Adopsi) |
| `birth_date` | `DATE` | No | - | - | Ya | Tanggal lahir tanggungan |
| `gender` | `VARCHAR(10)` | No | - | - | Tidak | Jenis kelamin: `Male`, `Female` |
| `family_card_number` | `VARCHAR(16)` | No | - | - | Ya (KK) | Nomor Kartu Keluarga (16 digit angka) |
| `is_active_student` | `BOOLEAN` | No | `false` | - | Tidak | Penanda anak kuliah aktif usia 21-25 tahun |
| `student_verification_letter_path`| `VARCHAR(255)`| Yes | `NULL` | - | Tidak | Lokasi berkas surat keterangan aktif kuliah dekanat kampus |
| `student_verification_expiry_date`| `DATE` | Yes | `NULL` | IDX3 | Tidak | Tanggal kedaluwarsa surat kuliah (default 31 Agustus tahun ajaran) |
| `verification_status` | `VARCHAR(50)` | No | `'Draft'` | - | Tidak | Status: `Draft`, `Submitted`, `Verified`, `Rejected`, `ExpiredAgeLimit` |
| `verified_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu persetujuan verifikasi oleh HR Benefit Officer |
| `verified_by_user_id` | `UUID` | Yes | `NULL` | FK | Tidak | ID user petugas HR yang memverifikasi dokumen fisik |
| `rejection_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Alasan penolakan berkas (jika status `Rejected`) |
| `is_active` | `BOOLEAN` | No | `true` | - | Tidak | Penanda status aktif tanggungan |

---

### 17.5 Tabel `hrd_benefit_balance_movements` (Buku Besar Mutasi Saldo Subsidi Berobat RS)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Buku besar double-entry pencatatan mutasi saldo subsidi plafon berobat di RS Quilvian sendiri, menjamin integritas saldo tidak pernah negatif.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik mutasi buku besar |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID staf nakes pemilik plafon subsidi |
| `benefit_plan_id` | `UUID` | No | - | FK | Tidak | ID paket subsidi berobat RS |
| `movement_type` | `VARCHAR(50)` | No | - | IDX2 | Tidak | Tipe: `AnnualAllocation`, `HospitalCareUsage`, `ManualAdjustment`, `YearEndExpiryReset` |
| `movement_amount` | `NUMERIC(15,2)` | No | - | - | Ya | Besaran nominal mutasi (Positif = Penambahan, Negatif = Pemakaian) |
| `balance_before` | `NUMERIC(15,2)` | No | - | - | Ya | Saldo plafon berjalan sebelum transaksi |
| `balance_after` | `NUMERIC(15,2)` | No | - | - | Ya | Saldo plafon berjalan setelah transaksi (Wajib `>= 0.00`) |
| `reference_number` | `VARCHAR(100)` | No | - | IDX3 | Tidak | Nomor referensi transaksi (No. Registrasi SIMRS, No. SK Plafon Tahunan) |
| `description` | `TEXT` | No | - | - | Tidak | Narasi keterangan mutasi dan rincian tindakan medis/poli |
| `movement_date` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | IDX1 | Tidak | Waktu transaksi pemotongan atau alokasi saldo |

---

### 17.6 Tabel `trx_benefit_claims` (Transaksi Klaim Koordinasi Manfaat COB)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Transaksi pengajuan dan penyelesaian klaim biaya perawatan kesehatan dengan mekanisme COB BPJS Kesehatan sebagai Primary Payer.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik klaim manfaat |
| `claim_number` | `VARCHAR(100)` | No | - | UQ | Tidak | Nomor seri klaim manfaat (Format: `CLM-BEN/YYYYMM/NNNN`) |
| `employee_id` | `UUID` | No | - | FK, IDX1 | Ya | ID staf nakes yang mengajukan klaim |
| `dependent_id` | `UUID` | Yes | `NULL` | FK | Ya | ID tanggungan keluarga pasien (jika klaim atas nama keluarga) |
| `benefit_plan_id` | `UUID` | No | - | FK | Tidak | ID paket asuransi/benefit yang diklaim |
| `service_type` | `VARCHAR(50)` | No | - | - | Tidak | Kategori: `Inpatient` (Rawat Inap), `Outpatient` (Rawat Jalan), `Emergency` |
| `hospital_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama rumah sakit tempat nakes/keluarga dirawat |
| `sep_bpjs_number` | `VARCHAR(100)` | Yes | `NULL` | IDX2 | Tidak | Nomor Surat Eligibilitas Peserta BPJS Kesehatan (Wajib untuk klaim COB) |
| `total_hospital_bill`| `NUMERIC(15,2)`| No | - | - | Ya | Total tagihan kuitansi asli rumah sakit (Rupiah) |
| `primary_payer_covered_amount`|`NUMERIC(15,2)`| No | `0.00` | - | Ya | Porsi biaya yang ditanggung BPJS Kesehatan (Tarif INA-CBGs) |
| `secondary_claim_amount`|`NUMERIC(15,2)`| No | `0.00` | - | Ya | Porsi santunan yang diklaimkan ke asuransi swasta tambahan / RS |
| `out_of_pocket_excess_amount`|`NUMERIC(15,2)`| No | `0.00` | - | Ya | Sisa tagihan ekses yang dibayar mandiri atau dipotong slip gaji |
| `claim_status` | `VARCHAR(50)` | No | `'Draft'` | IDX1 | Tidak | Status: `Draft`, `Submitted`, `UnderVerification`, `Approved`, `Rejected`, `Settled`, `Cancelled` |
| `service_date` | `DATE` | No | - | - | Tidak | Tanggal pasien menerima perawatan medis |
| `submitted_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengajuan klaim oleh pegawai di ESS |
| `settled_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pencairan/pelunasan santunan klaim sekunder |
| `settlement_reference`| `VARCHAR(100)`| Yes | `NULL` | - | Tidak | Nomor referensi transfer bank / pembayaran kasir |

---

### 17.7 Tabel `trx_benefit_claim_documents` (Lampiran Berkas Bukti Klaim Manfaat)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `BenefitManagement` (HRD)
- **Deskripsi:** Berkas digital lampiran pendukung klaim manfaat (scan SEP BPJS, kuitansi bermaterai, resume medis, perincian obat).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas lampiran |
| `claim_id` | `UUID` | No | - | FK, IDX1 | Tidak | ID klaim terkait (merujuk ke `trx_benefit_claims(id)`) |
| `document_type` | `VARCHAR(50)` | No | - | - | Tidak | Tipe: `BPJS_SEP`, `Hospital_Invoice`, `Receipt_Original`, `Prescription_Details`, `Medical_Summary` |
| `document_number` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Nomor bukti yang tertera pada lembar dokumen fisik |
| `file_storage_path` | `VARCHAR(255)` | No | - | - | Tidak | Path lokasi penyimpanan berkas digital terenkripsi |
| `file_size_bytes` | `BIGINT` | No | - | - | Tidak | Ukuran berkas digital dalam satuan byte (Maks 5 MB) |
| `mime_type` | `VARCHAR(100)` | No | - | - | Tidak | Format MIME: `application/pdf`, `image/jpeg`, `image/png` |
| `uploaded_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengunggahan dokumen ke server |
| `uploaded_by` | `VARCHAR(100)` | No | - | - | Tidak | NIP / username pengunggah dokumen |


---

## 18. Kamus Data Sub-Modul 13: HR Service Management & Request (Layanan Administrasi, Helpdesk SDM & Penerbitan Dokumen Kepegawaian RS)

Bagian ini mendefinisikan kamus data lengkap untuk entitas transaksi dan master pendukung **Sub-Modul 13 (Slice `S-D4`)**, mencakup katalog layanan SDM, penomoran terpusat naskah dinas, tiket helpdesk dengan SLA berjenjang, penarikan gaji otomatis berproteksi PIN 4-digit, serta log pemutakhiran profil kepegawaian.

Setiap entitas di bawah ini mewarisi 10 kolom audit standar `IdentityModel` (`id`, `created_at`, `created_by`, `updated_at`, `updated_by`, `is_deleted`, `deleted_at`, `deleted_by`, `row_version`, `tenant_id`) kecuali dinyatakan secara eksplisit.

---

### 18.1 Tabel `mst_hr_service_categories` (Kategori Layanan SDM)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Master pengelompokan layanan bantuan SDM rumah sakit (Administrasi Umum, Persuratan Resmi, Pemutakhiran Profil, Perjalanan Dinas Klinis).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik kategori layanan |
| `category_code` | `VARCHAR(30)` | No | - | UQ, IDX1 | Tidak | Kode unik kategori (contoh: `ADM-GEN`, `DOC-OFF`, `DATA-UPD`) |
| `category_name` | `VARCHAR(100)` | No | - | - | Tidak | Nama resmi kategori layanan |
| `description` | `TEXT` | Yes | `NULL` | - | Tidak | Uraian cakupan permohonan dalam kategori ini |
| `icon_name` | `VARCHAR(50)` | Yes | `NULL` | - | Tidak | Nama identifier icon UI untuk portal ESS |
| `display_order` | `INT` | No | `0` | - | Tidak | Urutan penayangan pada katalog layanan ESS |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX2 | Tidak | Penanda keaktifan kategori |

---

### 18.2 Tabel `mst_hr_service_types` (Tipe / Jenis Layanan Spesifik)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Detail jenis layanan spesifik, batas target durasi jam SLA, konfigurasi berkas wajib, dan kewenangan penugasan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik jenis layanan |
| `category_id` | `UUID` | No | - | FK, IDX1 | Tidak | Merujuk ke `mst_hr_service_categories(id)` |
| `type_code` | `VARCHAR(30)` | No | - | UQ, IDX2 | Tidak | Kode unik tipe layanan (contoh: `SRV-SKK`, `SRV-SAL`, `SRV-CITO`) |
| `type_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama jenis layanan yang tampil di menu staf |
| `priority_tier` | `VARCHAR(20)` | No | `'Standar'` | IDX3 | Tidak | Tingkatan prioritas: `'Cito'`, `'Standar'`, `'Khusus'` |
| `sla_target_hours` | `INT` | No | `24` | - | Tidak | Batas waktu target penyelesaian dalam jam kerja (Cito: 4, Standar: 24, Khusus: 72) |
| `requires_attachment` | `BOOLEAN` | No | `FALSE` | - | Tidak | Flag apakah pemohon wajib melampirkan berkas bukti pendukung |
| `required_attachment_description`| `TEXT` | Yes | `NULL` | - | Tidak | Petunjuk berkas yang wajib diunggah (misal: scan buku tabungan asli) |
| `requires_supervisor_approval` | `BOOLEAN` | No | `FALSE` | - | Tidak | Flag apakah butuh disposisi Kepala Ruangan sebelum diproses HR |
| `auto_assign_enabled` | `BOOLEAN` | No | `FALSE` | - | Tidak | Flag otomatis penugasan ke analis HR tertentu |
| `default_assignee_role` | `VARCHAR(50)` | Yes | `NULL` | - | Tidak | Role analis penanggung jawab default (`HR_Specialist`, `HR_Officer_CB`) |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX4 | Tidak | Penanda keaktifan jenis layanan |

---

### 18.3 Tabel `mst_employee_document_types` (Master Template Dokumen Resmi Kepegawaian)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Template naskah dinas resmi rumah sakit, konfigurasi nomor surat gapless, token variabel dinamis, dan perlindungan kerahasiaan nominal gaji.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik jenis template surat |
| `document_code` | `VARCHAR(30)` | No | - | UQ, IDX1 | Tidak | Kode format dokumen (contoh: `DOC-SKK`, `DOC-SAL`, `DOC-SPK`) |
| `document_name` | `VARCHAR(150)` | No | - | - | Tidak | Judul resmi dokumen (contoh: Surat Keterangan Kerja Aktif) |
| `numbering_pattern` | `VARCHAR(100)` | No | `'[KODE]/...'` | - | Tidak | Pola format nomor surat standar RS Quilvian |
| `last_sequence_number` | `INT` | No | `0` | - | Tidak | Counter urutan nomor surat terakhir tahun berjalan (*gapless*) |
| `current_year` | `INT` | No | `2026` | - | Tidak | Tahun penomoran aktif untuk peresetan tahunan |
| `template_body_html` | `TEXT` | No | - | - | Tidak | Isi naskah dinas berformat HTML/Liquid dengan variabel token |
| `supported_tokens_json` | `JSONB` | No | `'[]'::jsonb` | - | Tidak | Array string daftar token variabel yang diizinkan |
| `validity_period_days` | `INT` | No | `90` | - | Tidak | Masa berlaku dokumen dalam hari kalender (default 90 hari / 3 bulan) |
| `is_financial_sensitive` | `BOOLEAN` | No | `FALSE` | IDX2 | Tidak | Flag dokumen finansial yang memuat gaji (memerlukan PIN 4-digit & approval C&B) |
| `requires_cb_approval` | `BOOLEAN` | No | `FALSE` | - | Tidak | Flag kewajiban persetujuan Supervisor Kompensasi & Benefit |
| `authorized_signer_title` | `VARCHAR(100)`| No | `'Manajer SDM'`| - | Tidak | Jabatan pejabat berwenang penandatangan dokumen |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX3 | Tidak | Penanda keaktifan template dokumen |

---

### 18.4 Tabel `trx_hr_service_requests` (Tiket Permohonan Layanan SDM)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Agregat utama tiket permohonan staf rumah sakit via ESS, tracking SLA 3-tier secara real-time, dan antrean analis SDM.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik tiket layanan |
| `ticket_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor tiket unik sistem (contoh: `TKT-202611-00128`) |
| `employee_id` | `UUID` | No | - | FK, IDX2 | Tidak | ID staf pemohon (merujuk ke `mst_workforce_profiles(id)`) |
| `service_type_id` | `UUID` | No | - | FK, IDX3 | Tidak | ID jenis layanan (merujuk ke `mst_hr_service_types(id)`) |
| `title` | `VARCHAR(200)` | No | - | - | Tidak | Ringkasan pokok permohonan staf |
| `description` | `TEXT` | No | - | - | Tidak | Uraian lengkap rincian permohonan |
| `priority_tier` | `VARCHAR(20)` | No | `'Standar'` | IDX4 | Tidak | Nilai: `'Cito'`, `'Standar'`, `'Khusus'` |
| `sla_target_hours` | `INT` | No | - | - | Tidak | Target waktu SLA yang dikunci saat tiket disubmit |
| `sla_deadline_utc` | `TIMESTAMPTZ` | No | - | IDX5 | Tidak | Batas waktu penyelesaian absolut (dihitung berbasis jam kerja RS) |
| `sla_status` | `VARCHAR(20)` | No | `'WithinSla'` | IDX6 | Tidak | Status SLA: `'WithinSla'`, `'Warning75'`, `'Overdue'`, `'Escalated'` |
| `assigned_to_user_id` | `UUID` | Yes | `NULL` | IDX7 | Tidak | ID user analis HR penanggung jawab penanganan tiket |
| `request_status` | `VARCHAR(30)` | No | `'Submitted'` | IDX8 | Tidak | Status: `'Draft'`, `'Submitted'`, `'InReview'`, `'WaitingStaffResponse'`, `'Approved'`, `'Rejected'`, `'Resolved'`, `'Closed'`, `'Cancelled'` |
| `first_response_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu respon pertama kali oleh analis HR |
| `resolved_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu penyelesaian tiket secara tuntas |
| `closed_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu penutupan tiket oleh pemohon atau auto-close 7 hari |
| `resolution_summary` | `TEXT` | Yes | `NULL` | - | Tidak | Rangkuman hasil pemrosesan oleh tim HR |

---

### 18.5 Tabel `trx_hr_service_request_attachments` (Lampiran Berkas Pendukung Tiket)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Berkas dokumen bukti persyaratan permohonan (scan KTP, buku tabungan, buku nikah, disposisi cito).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas lampiran |
| `request_id` | `UUID` | No | - | FK, IDX1 | Tidak | Merujuk ke `trx_hr_service_requests(id)` |
| `file_path` | `VARCHAR(500)` | No | - | - | Ya | Path penyimpanan berkas terenkripsi di server |
| `original_file_name` | `VARCHAR(255)` | No | - | - | Tidak | Nama asli file saat diunggah oleh staf |
| `content_type` | `VARCHAR(100)` | No | - | - | Tidak | Format MIME: `application/pdf`, `image/jpeg`, `image/png` |
| `file_size_bytes` | `BIGINT` | No | - | - | Tidak | Ukuran file dalam bytes (Maksimal 5.242.880 bytes / 5 MB) |
| `attachment_type` | `VARCHAR(50)` | No | - | - | Tidak | Tipe: `DisposisiCito`, `BukuTabungan`, `BukuNikah`, `KTP`, `Lainnya` |
| `uploaded_at_utc` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengunggahan berkas |

---

### 18.6 Tabel `trx_hr_service_request_comments` (Utas Percakapan Klarifikasi)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Utas obrolan klarifikasi 2-arah antara staf pemohon dan tim HR Service Desk, termasuk catatan internal privat.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik komentar |
| `request_id` | `UUID` | No | - | FK, IDX1 | Tidak | Merujuk ke `trx_hr_service_requests(id)` |
| `author_user_id` | `UUID` | No | - | IDX2 | Tidak | ID user pengirim pesan |
| `author_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama pengirim komentar yang ditampilkan |
| `comment_text` | `TEXT` | No | - | - | Tidak | Isi teks pesan klarifikasi |
| `is_internal_note` | `BOOLEAN` | No | `FALSE` | IDX3 | Tidak | Flag jika `TRUE`, pesan hanya bisa dibaca sesama tim HR (tidak tampak di ESS) |
| `created_at_utc` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengiriman pesan |

---

### 18.7 Tabel `trx_employee_document_requests` (Permohonan Surat Resmi Kepegawaian)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Detail transaksi permohonan surat naskah dinas resmi dan alur persetujuan pejabat berwenang sebelum penerbitan nomor.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik transaksi permohonan dokumen |
| `hr_service_request_id` | `UUID`| No | - | UQ, FK, IDX1| Tidak | ID tiket induk (merujuk ke `trx_hr_service_requests(id)`) |
| `document_type_id` | `UUID` | No | - | FK, IDX2 | Tidak | Merujuk ke `mst_employee_document_types(id)` |
| `requested_by_employee_id`| `UUID` | No | - | FK, IDX3 | Tidak | ID pegawai pemohon |
| `purpose_of_letter` | `VARCHAR(255)`| No | - | - | Tidak | Tujuan pembuatan surat (misal: Pengajuan KPR Bank BTN) |
| `recipient_entity_name` | `VARCHAR(200)`| No | - | - | Tidak | Pihak yang dituju (misal: Pimpinan Bank BTN Cabang Medika) |
| `specific_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Keterangan khusus yang perlu dicantumkan |
| `draft_document_body_html`| `TEXT` | No | - | - | Ya | Draf naskah dinas hasil rendering token dengan data profil staf |
| `approval_status` | `VARCHAR(30)` | No | `'Submitted'` | IDX4 | Tidak | Status: `'Submitted'`, `'CbApproved'`, `'Authorized'`, `'Rejected'` |
| `cb_approver_user_id` | `UUID` | Yes | `NULL` | - | Tidak | ID supervisor C&B yang memverifikasi data gaji |
| `cb_approved_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu persetujuan oleh C&B Supervisor |
| `authorized_signer_user_id`| `UUID`| Yes | `NULL` | - | Tidak | ID pejabat penandatangan dokumen (Manajer SDM) |
| `authorized_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu otorisasi pembubuhan tanda tangan |
| `rejection_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Alasan penolakan permohonan surat |

---

### 18.8 Tabel `trx_employee_document_issuances` (Dokumen Resmi Terbit & Portal Verifikasi QR)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` (HRD)
- **Deskripsi:** Rekam dokumen resmi kepegawaian yang telah sah, ber-QR Code publik, berproteksi 4-digit PIN finansial, dan pengarsipan e-Dossier.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik penerbitan dokumen |
| `document_request_id` | `UUID` | No | - | UQ, FK, IDX1| Tidak | Merujuk ke `trx_employee_document_requests(id)` |
| `document_type_id` | `UUID` | No | - | FK, IDX2 | Tidak | Merujuk ke `mst_employee_document_types(id)` |
| `employee_id` | `UUID` | No | - | FK, IDX3 | Tidak | ID pegawai pemilik dokumen |
| `official_document_number`| `VARCHAR(100)`| No | - | UQ, IDX4 | Tidak | Nomor seri resmi (contoh: `042/RSQ-SDM/SKG/XI/2026/0128`) |
| `issued_date` | `DATE` | No | - | - | Tidak | Tanggal penerbitan resmi dokumen |
| `valid_until_date` | `DATE` | No | - | IDX5 | Tidak | Batas tanggal masa berlaku dokumen (default H+90 hari) |
| `signer_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama lengkap pejabat penandatangan sah |
| `signer_title` | `VARCHAR(150)` | No | - | - | Tidak | Jabatan pejabat penandatangan |
| `signer_signature_hash` | `VARCHAR(256)`| No | - | - | Tidak | Hash integritas digital signature penandatangan |
| `verification_hash` | `VARCHAR(64)` | No | - | UQ, IDX6 | Tidak | Hash unik SHA-256 untuk URL QR Code publik (`/verify/document/{hash}`) |
| `qr_code_image_path` | `VARCHAR(500)` | No | - | - | Tidak | Path file gambar kode QR yang ditempel di kaki PDF |
| `pdf_file_path` | `VARCHAR(500)` | No | - | - | Ya | Path file PDF naskah dinas resmi siap unduh |
| `is_financial_sensitive` | `BOOLEAN` | No | `FALSE` | IDX7 | Tidak | Flag dokumen finansial yang memuat gaji |
| `security_pin_hash` | `VARCHAR(255)` | Yes | `NULL` | - | Ya | Hash BCrypt dari 4-digit PIN rahasia untuk membuka rincian gaji |
| `salary_bruto_amount` | `NUMERIC(18,2)`| Yes | `NULL` | - | Ya | Nominal gaji bruto yang disahkan (ditarik dari Sub-10) |
| `salary_netto_amount` | `NUMERIC(18,2)`| Yes | `NULL` | - | Ya | Nominal penerimaan gaji bersih yang disahkan |
| `payroll_payslip_id` | `UUID` | Yes | `NULL` | FK | Ya | ID slip gaji Sub-10 sumber penarikan data |
| `issuance_status` | `VARCHAR(20)` | No | `'Active'` | IDX8 | Tidak | Status: `'Active'`, `'Expired'`, `'Revoked'` |
| `revoked_at_utc` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pencabutan dokumen bila dibatalkan |
| `revoked_by_user_id` | `UUID` | Yes | `NULL` | - | Tidak | ID user pejabat yang mengeksekusi revokasi |
| `revocation_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Alasan tertulis pembatalan keabsahan dokumen |
| `revocation_bap_number` | `VARCHAR(100)`| Yes | `NULL` | - | Tidak | Nomor Berita Acara Pemeriksaan (BAP) pembatalan |
| `e_dossier_document_id` | `UUID` | Yes | `NULL` | - | Tidak | ID berkas pada repositori E-Dossier Sub-02 (`INT-HR-66`) |

---

### 18.9 Tabel `hrd_profile_change_histories` (Log Audit Pemutakhiran Profil Otomatis)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `HrServiceManagement` / `WorkforceCore` (HRD)
- **Deskripsi:** Log forensik perbandingan snapshot data lama dan data baru hasil permohonan staf yang disetujui petugas HR Service.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik log histori perubahan profil |
| `hr_service_request_id` | `UUID`| No | - | FK, IDX1 | Tidak | Merujuk ke tiket layanan sumber perubahan |
| `employee_id` | `UUID` | No | - | FK, IDX2 | Tidak | ID pegawai yang datanya dimutakhirkan |
| `component_modified` | `VARCHAR(50)` | No | - | - | Tidak | Komponen yang diubah: `'Bank_Account'`, `'Marital_Status'`, `'Address'` |
| `old_values_json` | `JSONB` | No | - | - | Ya | Snapshot data lama sebelum diubah |
| `new_values_json` | `JSONB` | No | - | - | Ya | Nilai data baru hasil pengajuan staf |
| `approved_by_user_id` | `UUID` | No | - | - | Tidak | ID petugas analis HR yang menyetujui perubahan |
| `applied_at_utc` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu mutasi data master berhasil dieksekusi |


---

## 19. Kamus Data Sub-Modul 14: Credentialing, License & Clinical Privilege (Slice `S-C1`)

Bagian ini mendefinisikan kamus data lengkap untuk tabel baru dan pembaruan tabel yang digunakan oleh **Sub-Modul 14 (Slice `S-C1`)** dalam rangka tata kelola kredensialing 3 komite mandiri (Medik, Keperawatan, Nakes Lain), verifikasi sumber primer (PSV) STR seumur hidup & SIP 5 tahun, pengesahan SPK & RKK oleh Direktur Utama, registri kewenangan klinis aktif, dan log audit bypass darurat di SIMRS/EMR.

---

### 19.1 Tabel `mst_credentialing_committees` (Master Komite Kredensial Rumah Sakit)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Master 3 pilar komite tata kelola profesi tenaga kesehatan rumah sakit berdasarkan amanat Permenkes 6/2026.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik komite kredensial |
| `committee_code` | `VARCHAR(30)` | No | - | UQ, IDX1 | Tidak | Kode komite unik: `MEDIK`, `KEPERAWATAN`, `NAKES_LAIN` (`VR-CRD-10`) |
| `committee_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama resmi komite (contoh: "Komite Medik RS Quilvian") |
| `profession_category` | `VARCHAR(100)` | No | - | - | Tidak | Kategori rumpun profesi yang dinaungi |
| `chairperson_workforce_id` | `UUID` | No | - | FK (`mst_workforce_profiles`) | Tidak | ID dokter/perawat/nakes yang menjabat Ketua Komite |
| `secretary_workforce_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID sekretaris komite yang mengelola administrasi berkas |
| `is_active` | `BOOLEAN` | No | `TRUE` | - | Tidak | Status keaktifan komite |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan baris master |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Username pembuat |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaruan terakhir |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Username pembaru terakhir |

---

### 19.2 Tabel `wfp_credential_licenses` (Lisensi Profesional STR & SIP Tenaga Medis)
- **Status Tabel:** `Diperbarui` (Kolom Baru: `is_lifetime`, `is_psv_verified`, `psv_source_url`, `psv_verified_at`, `psv_verified_by`, `satusehat_practitioner_id`)
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Registri dokumen legalitas izin praktik dokter dan tenaga kesehatan di rumah sakit (UU 17/2023).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik lisensi nakes |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX1 | Tidak | ID nakes pemilik lisensi |
| `license_type` | `VARCHAR(30)` | No | - | IDX2 | Tidak | Tipe dokumen lisensi: `STR`, `SIP`, `SURAT_TUGAS` |
| `license_number` | `VARCHAR(100)` | No | - | IDX3 | Ya | Nomor registrasi resmi STR KKI atau nomor SIP Dinkes |
| `issuing_authority` | `VARCHAR(150)` | No | - | - | Tidak | Instansi penerbit (contoh: KKI, Dinkes Kota Surabaya, DPMPTSP) |
| `issue_date` | `DATE` | No | - | - | Tidak | Tanggal penerbitan dokumen lisensi |
| `expiry_date` | `DATE` | Yes | `NULL` | IDX4 | Tidak | Tanggal kedaluwarsa dokumen (wajib 5 tahun untuk SIP; NULL untuk STR Seumur Hidup) |
| `is_lifetime` | `BOOLEAN` | No | `FALSE` | - | Tidak | `TRUE` jika STR berlaku seumur hidup sesuai UU 17/2023 (`HRD-DEC-146`) |
| `is_psv_verified` | `BOOLEAN` | No | `FALSE` | IDX5 | Tidak | Status verifikasi sumber primer ke portal KKI / SatuSehat SDMK |
| `psv_source_url` | `VARCHAR(500)` | Yes | `NULL` | - | Ya | URL rujukan verifikasi keabsahan portal Kemenkes |
| `psv_verified_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pelaksanaan audit PSV oleh staf kredensialing |
| `psv_verified_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID staf sekretariat kredensialing yang melakukan PSV |
| `satusehat_practitioner_id` | `VARCHAR(100)` | Yes | `NULL` | IDX6 | Ya | ID Praktisi nakes di platform SatuSehat Kemenkes |
| `status` | `VARCHAR(30)` | No | `'ACTIVE'` | IDX7 | Tidak | Status lisensi: `PENDING_PSV`, `ACTIVE`, `EXPIRING_SOON`, `EXPIRED`, `REVOKED` |

---

### 19.3 Tabel `trx_credentialing_applications` (Berkas Permohonan Kredensial Nakes)
- **Status Tabel:** `Diperbarui` (Kolom Baru: `committee_id`, `application_type`, `current_clinical_level`, `target_clinical_level`)
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Berkas permohonan kredensialing baru atau kenaikan jenjang klinis nakes ke komite terkait.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas permohonan |
| `application_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi berkas gapless (contoh: `CRD/2026/XI/0001`) |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID nakes pemohon kredensial |
| `committee_id` | `UUID` | No | - | FK (`mst_credentialing_committees`) | Tidak | Komite tujuan penelaahan (Medik / Keperawatan / Nakes Lain) |
| `application_type` | `VARCHAR(30)` | No | `'INITIAL'` | - | Tidak | Tipe permohonan: `INITIAL`, `RECREDENTIALING`, `LEVEL_ADVANCEMENT` |
| `current_clinical_level`| `VARCHAR(30)` | Yes | `NULL` | - | Tidak | Jenjang klinis saat ini (contoh: PK II, Dokter Spesialis Pratama) |
| `target_clinical_level` | `VARCHAR(30)` | No | - | - | Tidak | Jenjang klinis yang dimohonkan (contoh: PK III, Konsultan) |
| `status` | `VARCHAR(30)` | No | `'DRAFT'` | IDX3 | Tidak | Status: `DRAFT`, `SUBMITTED`, `UNDER_PSV`, `PEER_REVIEW`, `PLENARY`, `APPROVED`, `REJECTED` |
| `submission_date` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu berkas disubmit resmi oleh pemohon |
| `reviewer_chairman_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Ketua Subkomite penanggung jawab berkas |

---

### 19.4 Tabel `trx_clinical_privilege_assessments` (Asesmen Panel Mitra Bestari / Peer Review)
- **Status Tabel:** `Diperbarui` (Kolom Baru: `peer_reviewer_workforce_id`, `requested_level`, `recommended_level`, `signature_hash`)
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Hasil evaluasi teknis portofolio dan rekomendasi derajat wewenang oleh panel Mitra Bestari (minimal 2 reviewer).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik asesmen peer review |
| `application_id` | `UUID` | No | - | FK (`trx_credentialing_applications`), IDX1 | Tidak | ID berkas permohonan kredensial |
| `peer_reviewer_workforce_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID nakes reviewer Mitra Bestari yang ditunjuk |
| `privilege_catalog_id` | `UUID` | No | - | FK (`mst_clinical_privilege_catalogs`) | Tidak | ID butir tindakan medis/keperawatan yang dinilai |
| `requested_level` | `INT` | No | 4 | - | Tidak | Derajat wewenang yang diminta (4: Mandiri, 3: Supervisi) |
| `recommended_level` | `INT` | No | - | - | Tidak | Derajat wewenang hasil asesmen reviewer (4: Mandiri, 3: Supervisi, 1-2: Ditolak) |
| `reviewer_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Catatan pertimbangan klinis atau alasan penolakan/supervisi |
| `assessment_date` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu asesmen diselesaikan reviewer |
| `signature_hash` | `VARCHAR(256)` | No | - | - | Ya | Hash integritas tanda tangan elektronik reviewer |

---

### 19.5 Tabel `trx_spk_rkk_issuances` (Dokumen Sah Penerbitan SPK & RKK oleh Direktur Utama)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Dokumen Surat Penugasan Klinis (SPK) dan lampiran Rincian Kewenangan Klinis (RKK) yang disahkan secara tunggal oleh Direktur Utama RS.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik dokumen SPK/RKK |
| `spk_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor resmi SPK gapless (contoh: `SPK/RSQ-DIR/XI/2026/0045`) |
| `rkk_number` | `VARCHAR(100)` | No | - | UQ, IDX2 | Tidak | Nomor resmi lampiran RKK gapless |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX3 | Tidak | ID nakes yang menerima penugasan klinis |
| `committee_id` | `UUID` | No | - | FK (`mst_credentialing_committees`) | Tidak | ID komite yang merekomendasikan |
| `effective_start_date` | `DATE` | No | - | IDX4 | Tidak | Tanggal mulai berlaku penugasan klinis |
| `effective_end_date` | `DATE` | No | - | IDX4 | Tidak | Tanggal akhir berlaku (standar 3 tahun sejak tanggal pengesahan Dirut) |
| `privilege_nature` | `VARCHAR(30)` | No | `'REGULAR'` | - | Tidak | Sifat penugasan: `REGULAR`, `TEMPORARY` (maks 3 bln), `EMERGENCY` (maks 72 jam) |
| `approved_by_director_id`| `UUID` | No | - | FK (`mst_workforce_profiles`) | Tidak | ID Direktur Utama penandatangan dokumen sah |
| `approved_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu stempel pengesahan Direktur Utama |
| `digital_signature_hash` | `VARCHAR(256)` | No | - | - | Ya | Hash integritas SHA-256 tanda tangan elektronik Direktur Utama |
| `document_pdf_path` | `VARCHAR(500)` | No | - | - | Tidak | Lokasi file PDF resmi di repositori dokumen rumah sakit |
| `qr_verification_token` | `VARCHAR(100)` | No | - | UQ, IDX5 | Ya | Token unik QR Code untuk verifikasi keabsahan publik |
| `status` | `VARCHAR(30)` | No | `'ACTIVE'` | IDX6 | Tidak | Status dokumen: `ACTIVE`, `EXPIRING_SOON`, `EXPIRED`, `SUSPENDED`, `REVOKED` |

---

### 19.6 Tabel `wfp_clinical_privileges` (Registri Kewenangan Klinis Individual Aktif)
- **Status Tabel:** `Diperbarui` (Kolom Baru: `spk_issuance_id`, `supervision_level`, `supervisor_workforce_id`, `privilege_nature`, `is_suspended`, `suspension_reason`)
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Daftar butir tindakan klinis aktif per nakes yang menjadi acuan gerbang SIMRS/EMR.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik baris kewenangan klinis |
| `spk_issuance_id` | `UUID` | Yes | `NULL` | FK (`trx_spk_rkk_issuances`), IDX1 | Tidak | ID dokumen induk pengesahan SPK/RKK Dirut |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID nakes pemilik wewenang |
| `privilege_catalog_id` | `UUID` | No | - | FK (`mst_clinical_privilege_catalogs`), IDX3 | Tidak | ID butir tindakan medis/keperawatan |
| `supervision_level` | `INT` | No | 4 | IDX4 | Tidak | Derajat wewenang: 4: Mandiri, 3: Supervisi, 1-2: Ditolak (`HRD-DEC-148`) |
| `supervisor_workforce_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID dokter supervisor penanggung jawab jika Level 3 |
| `privilege_nature` | `VARCHAR(30)` | No | `'REGULAR'` | - | Tidak | `REGULAR`, `TEMPORARY`, `EMERGENCY` |
| `valid_from` | `DATE` | No | - | - | Tidak | Tanggal awal aktif kewenangan |
| `valid_until` | `DATE` | No | - | IDX5 | Tidak | Tanggal kedaluwarsa kewenangan |
| `is_suspended` | `BOOLEAN` | No | `FALSE` | IDX6 | Tidak | Status penangguhan wewenang (`TRUE` jika SIP mati atau dalam evaluasi etik) |
| `suspension_reason` | `VARCHAR(500)` | Yes | `NULL` | - | Tidak | Alasan penangguhan wewenang klinis |

---

### 19.7 Tabel `trx_clinical_privilege_emergency_override_logs` (Log Audit Bypass Darurat SIMRS)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Rekam jejak audit saat dokter mengaktifkan bypass darurat penyelamatan nyawa (*Life-Saving Emergency Override*) untuk tindakan di luar RKK aktifnya.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik log override darurat |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX1 | Tidak | ID dokter yang mengeksekusi bypass darurat |
| `privilege_catalog_id` | `UUID` | No | - | FK (`mst_clinical_privilege_catalogs`) | Tidak | ID tindakan medis yang di-override |
| `simrs_patient_episode_id` | `VARCHAR(100)` | No | - | IDX2 | Ya | ID registrasi pasien gawat darurat di SIMRS |
| `action_attempted` | `VARCHAR(200)` | No | - | - | Tidak | Nama prosedur tindakan yang dijalankan |
| `emergency_reason_justification` | `TEXT` | No | - | - | Ya | Penjelasan justifikasi medis cito darurat nyawa oleh dokter |
| `override_triggered_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | IDX3 | Tidak | Waktu tombol bypass ditekan di EMR |
| `ip_address` | `VARCHAR(50)` | No | - | - | Ya | Alamat IP komputer workstation IGD/ICU/OK |
| `is_audited_by_kmkp` | `BOOLEAN` | No | `FALSE` | IDX4 | Tidak | Status konfirmasi audit oleh Komite Mutu & Keselamatan Pasien |
| `kmkp_audit_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu sidang telaah audit KMKP |
| `kmkp_audit_verdict` | `VARCHAR(50)` | Yes | `NULL` | - | Tidak | Putusan KMKP: `JUSTIFIED_CLINICAL_EMERGENCY`, `ETHICAL_VIOLATION` |

---

### 19.8 Tabel `trx_recredentialing_applications` (Siklus Evaluasi Rekredensialing 3 Tahunan)
- **Status Tabel:** `Diperbarui` (Kolom Baru: `oppe_performance_score`, `oppe_evaluation_summary`, `current_spk_issuance_id`)
- **Modul Pemilik:** `CredentialingManagement` (HRD)
- **Deskripsi:** Berkas evaluasi berkala 3 tahunan nakes terhubung dengan nilai kinerja klinis OPPE Sub-15.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas rekredensialing |
| `recredential_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi berkas rekredensialing gapless |
| `current_spk_issuance_id` | `UUID` | No | - | FK (`trx_spk_rkk_issuances`) | Tidak | ID dokumen SPK/RKK aktif yang sedang dievaluasi |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID nakes yang dievaluasi berkala |
| `committee_id` | `UUID` | No | - | FK (`mst_credentialing_committees`) | Tidak | ID komite pemeriksa |
| `oppe_performance_score` | `DECIMAL(5,2)` | Yes | `NULL` | - | Tidak | Skor agregasi evaluasi kinerja klinis dari Sub-15 (0-100) |
| `oppe_evaluation_summary` | `TEXT` | Yes | `NULL` | - | Tidak | Ringkasan rekam jejak mutu asuhan klinis dokter |
| `requested_date` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengajuan atau inisiasi otomatis draf pada H-60 |
| `status` | `VARCHAR(30)` | No | `'DRAFT'` | IDX3 | Tidak | Status: `DRAFT`, `SUBMITTED`, `UNDER_EVALUATION`, `APPROVED`, `SUSPENDED` |

---

## 20. Kamus Data Sub-Modul 15: OPPE, FPPE & Recredentialing (`S-C1-OPPE`)

### 20.1 Tabel `mst_oppe_metric_catalogs` (Katalog Indikator Kinerja Klinis OPPE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Master katalog indikator evaluasi praktik profesional berkelanjutan berbasis 6 pilar standar ACGME/KARS.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik indikator OPPE |
| `metric_code` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Kode unik indikator (cth: `OPP-SURG-IDO`, `OPP-EMR-TIME`) |
| `metric_name` | `VARCHAR(250)` | No | - | - | Tidak | Nama deskriptif indikator performa klinis |
| `acgme_domain` | `VARCHAR(50)` | No | - | IDX2 | Tidak | 6 Pilar: `PatientCare`, `MedicalKnowledge`, `PracticeBasedLearning`, `InterpersonalCommunication`, `Professionalism`, `SystemsBasedPractice` |
| `target_profession_type` | `VARCHAR(50)` | No | - | - | Tidak | Profesi target: `DOKTER_SPESIALIS`, `DOKTER_UMUM`, `PERAWAT_KLINIS` |
| `ksm_department_id` | `UUID` | Yes | `NULL` | FK (`mst_organization_units`) | Tidak | Spesifik KSM tertentu (NULL = berlaku umum seluruh dokter) |
| `data_source_type` | `VARCHAR(50)` | No | - | - | Tidak | Sumber data: `SIMRS_AUTOMATED`, `PEER_REVIEW_QUALITATIVE`, `PATIENT_SAFETY_INCIDENT`, `CLINICAL_AUDIT_MANUAL` |
| `target_threshold_operator` | `VARCHAR(10)` | No | - | - | Tidak | Operator ambang target: `GTE` (>=), `LTE` (<=), `EQ` (==), `BETWEEN` |
| `target_threshold_value` | `DECIMAL(10,2)` | No | - | - | Tidak | Nilai numerik target kepatuhan atau batas maksimal toleransi |
| `default_weight_percentage` | `DECIMAL(5,2)` | No | `0.00` | - | Tidak | Bobot kontribusi indikator dalam persentase |
| `simrs_metric_extractor_key` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Kunci integrasi job ekstraktor data otomatis modul SIMRS/KMKP |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX3 | Tidak | Status keaktifan indikator dalam evaluasi |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan master |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu modifikasi terakhir |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah data |

---

### 20.2 Tabel `trx_oppe_evaluation_periods` (Periode Siklus Evaluasi OPPE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Periode siklus evaluasi berkala OPPE tahunan atau semesteran per KSM atau rumah sakit.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik periode evaluasi OPPE |
| `period_code` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Kode registrasi periode tahunan/semesteran |
| `period_name` | `VARCHAR(250)` | No | - | - | Tidak | Nama periode (misal: "OPPE Semester 1 2026 - KSM Bedah") |
| `evaluation_year` | `INT` | No | - | IDX2 | Tidak | Tahun kalender pelaksanaan evaluasi |
| `evaluation_cycle` | `VARCHAR(20)` | No | - | - | Tidak | Siklus: `SEMESTER_1`, `SEMESTER_2`, `ANNUAL` |
| `start_date` | `DATE` | No | - | - | Tidak | Tanggal awal rentang data klinis yang dievaluasi |
| `end_date` | `DATE` | No | - | - | Tidak | Tanggal akhir rentang data klinis yang dievaluasi |
| `ksm_department_id` | `UUID` | Yes | `NULL` | FK (`mst_organization_units`) | Tidak | Lingkup KSM tertentu (NULL jika serentak seluruh RS) |
| `status` | `VARCHAR(30)` | No | `'DRAFT'` | IDX3 | Tidak | Siklus: `DRAFT`, `ACTIVE`, `EVALUATION_CLOSED`, `FINALIZED`, `CANCELLED` |
| `quantitative_weight_percentage` | `DECIMAL(5,2)` | No | `60.00` | - | Tidak | Bobot porsi nilai data objektif SIMRS (standar: 60%) |
| `qualitative_weight_percentage` | `DECIMAL(5,2)` | No | `40.00` | - | Tidak | Bobot porsi telaah sejawat / peer review (standar: 40%) |
| `is_locked` | `BOOLEAN` | No | `FALSE` | - | Tidak | Status penguncian data (tidak dapat diedit lagi jika TRUE) |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan periode |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu perubahan terakhir |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah data |

---

### 20.3 Tabel `trx_oppe_staff_evaluations` (Rapor Evaluasi OPPE Individu Dokter)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Berkas rapor evaluasi kinerja klinis individu tenaga medis dalam satu periode OPPE tertentu.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik rapor individu OPPE |
| `evaluation_period_id` | `UUID` | No | - | FK (`trx_oppe_evaluation_periods`), IDX1 | Tidak | Relasi ke siklus periode evaluasi |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID dokter spesialis/nakes yang dievaluasi |
| `evaluation_number` | `VARCHAR(100)` | No | - | UQ, IDX3 | Tidak | Nomor rapor resmi registrasi OPPE |
| `quantitative_score` | `DECIMAL(5,2)` | Yes | `NULL` | - | Tidak | Skor agregasi nilai objektif SIMRS/KMKP (0-100) |
| `qualitative_score` | `DECIMAL(5,2)` | Yes | `NULL` | - | Tidak | Skor rata-rata telaah sejawat peer review KSM (0-100) |
| `composite_score` | `DECIMAL(5,2)` | Yes | `NULL` | - | Tidak | Nilai komposit akhir (60% kuantitatif + 40% kualitatif) |
| `passed_evaluation` | `BOOLEAN` | No | `FALSE` | - | Tidak | Status kelulusan ambang batas standar OPPE (>= 75.0) |
| `early_warning_triggered` | `BOOLEAN` | No | `FALSE` | IDX4 | Tidak | True jika terdapat indikator jatuh di bawah batas bahaya |
| `status` | `VARCHAR(30)` | No | `'DRAFT'` | IDX5 | Tidak | Status: `DRAFT`, `IN_REVIEW`, `COMPLETED`, `ACKNOWLEDGED`, `DISPUTED` |
| `committee_notes` | `TEXT` | Yes | `NULL` | - | Ya | Kesimpulan & rekomendasi tertulis Subkomite Mutu Profesi |
| `acknowledged_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu konfirmasi persetujuan/baca oleh dokter yang bersangkutan |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembentukan berkas |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu update nilai |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah nilai |

---

### 20.4 Tabel `trx_oppe_staff_evaluation_details` (Rincian Butir Penilaian Indikator OPPE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Detail skor per butir indikator dan catatan peer review yang dilindungi kerahasiaan (*Peer Review Privilege*).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik detail penilaian indikator |
| `staff_evaluation_id` | `UUID` | No | - | FK (`trx_oppe_staff_evaluations`), IDX1 | Tidak | Relasi ke berkas rapor induk |
| `metric_catalog_id` | `UUID` | No | - | FK (`mst_oppe_metric_catalogs`), IDX2 | Tidak | Relasi ke master indikator |
| `target_value` | `DECIMAL(10,2)` | No | - | - | Tidak | Ambang target yang ditetapkan KSM/RS |
| `achieved_value` | `DECIMAL(10,2)` | No | - | - | Tidak | Nilai riil pencapaian dokter dari data SIMRS/Review |
| `metric_score` | `DECIMAL(5,2)` | No | - | - | Tidak | Skor butir terkonversi skala 0 s.d. 100 |
| `weight_percentage` | `DECIMAL(5,2)` | No | - | - | Tidak | Bobot kontribusi butir terhadap sub-komponen |
| `weighted_score` | `DECIMAL(5,2)` | No | - | - | Tidak | Hasil kali skor butir dengan persentase bobot |
| `simrs_sync_timestamp` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu data otomatis ditarik dari SIMRS |
| `raw_data_payload_json` | `JSONB` | Yes | `NULL` | - | Ya | Payload bukti agregasi kasus dari database klinis |
| `reviewer_workforce_profile_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Ya | ID dokter sejawat penilai (dirahasiakan) |
| `peer_review_notes` | `TEXT` | Yes | `NULL` | - | Ya | Catatan kualitatif konfidensial peer review |
| `is_confidential_peer_review` | `BOOLEAN` | No | `FALSE` | IDX3 | Tidak | True jika butir tunduk pada hak istimewa *Peer Review Privilege* |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu entri |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu edit |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah data |

---

### 20.5 Tabel `trx_fppe_engagements` (Penugasan & Kasus FPPE Terarah)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Berkas penugasan evaluasi praktik klinis terarah (FPPE) baik untuk staf baru, prosedur baru, maupun audit pemicu insiden keselamatan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik penugasan FPPE |
| `fppe_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi penugasan FPPE terstruktur |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID dokter spesialis yang menjalani FPPE |
| `trigger_type` | `VARCHAR(50)` | No | - | IDX3 | Tidak | Jenis pemicu: `NEW_STAFF_ONBOARDING`, `NEW_CLINICAL_PROCEDURE`, `SENTINEL_EVENT`, `SURGICAL_COMPLICATION_HIGH`, `EMR_TIMELINESS_BREACH`, `ETHICAL_COMMITTEE_REFERRAL` |
| `privilege_catalog_id` | `UUID` | Yes | `NULL` | FK (`mst_clinical_privilege_catalogs`) | Tidak | Spesifik tindakan/prosedur baru yang dievaluasi (opsional) |
| `supervisor_workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`) | Tidak | ID dokter proctor/supervisor pendamping yang ditunjuk |
| `start_date` | `DATE` | No | - | - | Tidak | Tanggal dimulainya masa pemantauan proctoring |
| `end_date` | `DATE` | No | - | - | Tidak | Batas akhir masa pemantauan proctoring |
| `target_case_count` | `INT` | No | `10` | - | Tidak | Target jumlah kasus klinis terproctoring (staf baru: 10, prosedur baru: 5) |
| `completed_case_count` | `INT` | No | `0` | - | Tidak | Jumlah kasus yang telah selesai diverifikasi proctor |
| `status` | `VARCHAR(30)` | No | `'INITIATED'` | IDX4 | Tidak | Status: `INITIATED`, `IN_PROGRESS`, `EXTENDED`, `UNDER_REVIEW`, `CONCLUDED`, `ABORTED` |
| `trigger_justification_notes` | `TEXT` | No | - | - | Ya | Latar belakang pemicu insiden atau rujukan komite etik |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu inisiasi FPPE |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu perubahan status |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah status |

---

### 20.6 Tabel `trx_fppe_case_logs` (Logbook Kasus Terproctoring FPPE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Buku catatan logbook pelaksanaan tindakan medis dalam masa proctoring FPPE.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik catatan kasus terproctoring |
| `fppe_engagement_id` | `UUID` | No | - | FK (`trx_fppe_engagements`), IDX1 | Tidak | Relasi ke berkas induk penugasan FPPE |
| `case_number` | `VARCHAR(100)` | No | - | IDX2 | Tidak | Nomor urut kasus dalam masa proctoring |
| `simrs_patient_pseudonym_id` | `VARCHAR(100)` | No | - | - | Ya | ID samaran rekam medis pasien untuk audit klinis terproteksi |
| `procedure_catalog_id` | `UUID` | No | - | FK (`mst_clinical_privilege_catalogs`) | Tidak | Jenis tindakan medis yang dilaksanakan |
| `procedure_date` | `DATE` | No | - | - | Tidak | Tanggal pelaksanaan tindakan operasi/tindakan medis |
| `clinical_independence_level` | `INT` | No | - | - | Tidak | Tingkat kemandirian: 1 (Observasi), 2 (Asistensi), 3 (Supervisi Langsung), 4 (Mandiri Terpantau) |
| `proctor_evaluation_verdict` | `VARCHAR(50)` | No | - | - | Tidak | Putusan proctor: `SATISFACTORY`, `ACCEPTABLE_WITH_COACHING`, `UNSATISFACTORY`, `COMPLICATION_OCCURRED` |
| `complication_observed` | `BOOLEAN` | No | `FALSE` | IDX3 | Tidak | True jika terjadi komplikasi intra/pasca-prosedur |
| `proctor_clinical_notes` | `TEXT` | No | - | - | Ya | Evaluasi teknik klinis dan keselamatan pasien oleh proctor |
| `is_verified_by_proctor` | `BOOLEAN` | No | `FALSE` | IDX4 | Tidak | Status persetujuan tanda tangan digital proctor |
| `verified_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu persetujuan verifikasi proctor |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu entri logbook |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu revisi entri |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah entri |

---

### 20.7 Tabel `trx_fppe_evaluation_conclusions` (Kesimpulan & Rekomendasi Sidang Pleno FPPE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Berita acara putusan sidang pleno Subkomite Mutu Profesi atas hasil evaluasi FPPE.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik kesimpulan FPPE |
| `fppe_engagement_id` | `UUID` | No | - | UQ, FK (`trx_fppe_engagements`), IDX1 | Tidak | Relasi 1-to-1 dengan berkas penugasan FPPE |
| `conclusion_number` | `VARCHAR(100)` | No | - | UQ, IDX2 | Tidak | Nomor registrasi berita acara putusan FPPE |
| `plenary_date` | `DATE` | No | - | - | Tidak | Tanggal pelaksanaan rapat pleno komite medik |
| `final_verdict` | `VARCHAR(50)` | No | - | IDX3 | Tidak | Putusan: `FULL_PRIVILEGE_CONFIRMED`, `PROCTORING_EXTENDED`, `SUPERVISED_PRACTICE`, `PRIVILEGE_REVOKED` |
| `remediation_plan_details` | `TEXT` | Yes | `NULL` | - | Ya | Program pembinaan klinis dan pendampingan wajib |
| `extension_duration_months` | `INT` | Yes | `NULL` | - | Tidak | Tambahan bulan pemantauan jika perpanjangan proctoring |
| `committee_chair_workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`) | Tidak | ID Ketua Komite Medik / Subkomite Mutu penandatangan |
| `simrs_privilege_sync_status` | `VARCHAR(30)` | No | `'PENDING'` | IDX4 | Tidak | Status sinkronisasi ke EMR SIMRS: `PENDING`, `SYNCED`, `FAILED`, `BYPASSED` |
| `simrs_sync_timestamp` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaharuan hak akses ke modul SIMRS |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penerbitan putusan |
| `created_by` | `VARCHAR(100)` | No | - | - | Tidak | Pembuat data |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaruan |
| `updated_by` | `VARCHAR(100)` | Yes | `NULL` | - | Tidak | Pengubah data |

---

### 20.8 Tabel `hrd_clinical_peer_review_audit_logs` (Log Audit Kerahasiaan Peer Review Privilege)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `ClinicalPerformanceQuality` (HRD & Komite Medik)
- **Deskripsi:** Log keamanan permanen yang mencatat seluruh aksi pembacaan atau pembukaan dokumen telaah sejawat (*tamper-proof*).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik log audit akses kerahasiaan |
| `peer_review_detail_id` | `UUID` | No | - | FK (`trx_oppe_staff_evaluation_details`), IDX1 | Tidak | Relasi ke detail butir evaluasi peer review yang dibaca |
| `accessor_workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID personel yang membuka/membaca dokumen konfidensial |
| `accessed_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | IDX3 | Tidak | Waktu presisi pembacaan dokumen |
| `access_channel` | `VARCHAR(50)` | No | - | - | Tidak | Kanal akses: `WEB_PORTAL`, `API_DIRECT`, `EXPORT_REPORT` |
| `ip_address` | `VARCHAR(50)` | No | - | - | Ya | Alamat IP pemohon akses |
| `user_agent` | `VARCHAR(250)` | Yes | `NULL` | - | Tidak | Data peramban/perangkat yang dipakai |
| `confidentiality_notice_acknowledged` | `BOOLEAN` | No | `TRUE` | - | Tidak | Penegasan bahwa pengakses telah menyetujui peringatan hukum kerahasiaan |

## 21. Sub-Modul 16: Learning, Training & Competency (Slice S-C2)

Kamus data ini memuat spesifikasi skema relasional modul pembelajaran, pelatihan, dan kompetensi tenaga kesehatan serta seluruh pegawai rumah sakit (*LearningAndDevelopment* dan *hrd_training_bond_contracts*), mencakup pemenuhan 5 pelatihan wajib akreditasi STARKES KPS 8, evaluasi OSCE klinis, radar kedaluwarsa berkala, kontrol anggaran diklat, ikatan dinas retensi nakes, dan pencatatan SKP Plataran Sehat Kemenkes.

### 21.1 Tabel `mst_mandatory_training_rules` (Master Aturan Pelatihan Wajib Akreditasi RS)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Diklat & SDM RS)
- **Deskripsi:** Master katalog aturan pelatihan wajib berkala rumah sakit (BHD/BLS, PPI, Keselamatan Pasien/SKP, K3RS & Code Red, Komunikasi Efektif SBAR) dan pelatihan klinis khusus unit intensif (ACLS, ICU Ventilator, dsb) sesuai standar STARKES 2024 (KPS 8 & 8.1).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik aturan pelatihan wajib |
| `training_code` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Kode unik pelatihan (misal `TRN-MND-BHD`, `TRN-MND-PPI`, `TRN-MND-ACLS`) |
| `training_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama resmi pelatihan wajib rumah sakit |
| `training_category` | `VARCHAR(50)` | No | - | IDX2 | Tidak | Kategori: `ACCREDITATION_MANDATORY_HOSPITAL_WIDE`, `CLINICAL_INTENSIVE_SPECIALTY`, `DEPARTMENTAL_SPECIFIC` |
| `target_scope` | `VARCHAR(50)` | No | - | - | Tidak | Lingkup target: `ALL_EMPLOYEES_100_PERCENT`, `ALL_CLINICAL_STAFF`, `UNIT_SPECIFIC` |
| `target_unit_id` | `UUID` | Yes | `NULL` | FK (`mst_departments`), IDX3 | Tidak | ID unit kerja spesifik jika berlaku untuk unit tertentu (ICU, OK, IGD, Dialisis) |
| `target_profession_group` | `VARCHAR(50)` | Yes | `NULL` | - | Tidak | Kelompok profesi target: `MEDIS`, `KEPERAWATAN`, `NAKES_LAIN`, `NON_MEDIS`, `ALL` |
| `validity_period_months` | `INTEGER` | No | `24` | - | Tidak | Masa berlaku sertifikat dalam satuan bulan (default: 24 bulan / 2 tahun sesuai standar akreditasi) |
| `grace_period_days` | `INTEGER` | No | `30` | - | Tidak | Masa tenggang pendaftaran perpanjangan sebelum radar pemblokiran shift (default: 30 hari) |
| `min_attendance_percentage` | `NUMERIC(5,2)` | No | `80.00` | - | Tidak | Ambang minimal kehadiran presensi (80.00% umum, 100.00% nakes klinis) |
| `min_theory_passing_score` | `NUMERIC(5,2)` | No | `75.00` | - | Tidak | Nilai kelulusan minimal ujian teori post-test (default: 75.00) |
| `is_osce_required` | `BOOLEAN` | No | `FALSE` | - | Tidak | Wajib ujian simulasi keterampilan klinis langsung (*OSCE*) bertanda tangan assessor |
| `min_osce_passing_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Nilai kelulusan minimal ujian simulasi OSCE jika diwajibkan (misal 80.00) |
| `impact_on_clinical_privilege`| `BOOLEAN` | No | `FALSE` | - | Tidak | Apakah memicu pembekuan RKK Level 4 jika kedaluwarsa (`INT-HR-77`) |
| `impact_on_shift_scheduling` | `BOOLEAN` | No | `TRUE` | - | Tidak | Apakah memicu penugasan *buddy system* / pendampingan di jadwal shift Sub-06 (`INT-HR-79`) |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX4 | Tidak | Status keaktifan aturan pelatihan wajib |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan data aturan |
| `created_by` | `UUID` | No | - | - | Tidak | ID user pembuat aturan |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengubahan data aturan |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah aturan |

### 21.2 Tabel `trx_training_plans` (Rencana Diklat Tahunan & Analisis Kebutuhan Pelatihan - TNA)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Diklat & Manajemen Keuangan RS)
- **Deskripsi:** Rekapitulasi Rencana Program Pembelajaran dan Pelatihan Tahunan RS (*Annual Training Master Plan*) berbasis *Training Needs Analysis* (TNA) dan kepatuhan akreditasi.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik rencana diklat tahunan |
| `plan_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi program diklat tahunan (misal `PLN-TRN-2026-001`) |
| `budget_year` | `INTEGER` | No | - | IDX2 | Tidak | Tahun anggaran pelaksanaan diklat (misal `2026`) |
| `plan_title` | `VARCHAR(200)` | No | - | - | Tidak | Judul rencana kerja program diklat |
| `department_id` | `UUID` | Yes | `NULL` | FK (`mst_departments`), IDX3 | Tidak | ID departemen pengusul (NULL jika rencana diklat terpadu tingkat rumah sakit) |
| `total_estimated_budget` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Total estimasi pagu biaya pelaksanaan program |
| `currency` | `VARCHAR(3)` | No | `'IDR'` | - | Tidak | Mata uang anggaran (default: IDR) |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX4 | Tidak | Status: `Draft`, `Submitted`, `Approved`, `Active`, `Closed`, `Cancelled` |
| `approved_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Direktur / Manajer SDM yang menyetujui program tahunan |
| `approved_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengesahan rencana tahunan |
| `approval_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Catatan pertimbangan pengesahan pimpinan |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan rencana kerja |
| `created_by` | `UUID` | No | - | - | Tidak | ID user pembuat |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaruan data |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah |

### 21.3 Tabel `trx_training_sessions` (Sesi Penyelenggaraan Kelas & Workshop Pelatihan)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Bagian Diklat RS)
- **Deskripsi:** Pelaksanaan batch atau sesi kelas/workshop/simulasi pelatihan yang dijadwalkan, mencakup narasumber, instruktur klinis, kuota, metode, dan tempat pelaksanaan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik sesi kelas pelatihan |
| `training_rule_id` | `UUID` | Yes | `NULL` | FK (`mst_mandatory_training_rules`), IDX1 | Tidak | Relasi ke master aturan jika sesi pelatihan wajib akreditasi |
| `session_code` | `VARCHAR(50)` | No | - | UQ, IDX2 | Tidak | Kode unik sesi kelas (misal `SES-BHD-2026-B1`) |
| `session_title` | `VARCHAR(200)` | No | - | - | Tidak | Judul nama kelas/sesi pelatihan |
| `training_type` | `VARCHAR(50)` | No | - | IDX3 | Tidak | Tipe: `INTERNAL_IN_HOUSE`, `EXTERNAL_OFF_SITE`, `HYBRID_SIMULATION`, `ONLINE_WEBINAR` |
| `start_time` | `TIMESTAMPTZ` | No | - | IDX4 | Tidak | Tanggal dan waktu pembukaan sesi pelatihan |
| `end_time` | `TIMESTAMPTZ` | No | - | - | Tidak | Tanggal dan waktu penutupan sesi pelatihan |
| `location_room` | `VARCHAR(150)` | Yes | `NULL` | - | Tidak | Ruang kelas, aula, atau laboratorium simulasi RS |
| `virtual_meeting_url` | `VARCHAR(250)` | Yes | `NULL` | - | Tidak | URL tautan webinar/zoom jika hybrid atau daring |
| `max_capacity` | `INTEGER` | No | `30` | - | Tidak | Kuota kapasitas maksimal peserta |
| `current_enrolled_count` | `INTEGER` | No | `0` | - | Tidak | Jumlah peserta yang telah terdaftar resmi |
| `primary_instructor_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama narasumber utama / instruktur pelatihan |
| `primary_assessor_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`), IDX5 | Tidak | ID dokter/perawat assesor penguji simulasi OSCE |
| `total_duration_hours` | `NUMERIC(5,2)` | No | `8.00` | - | Tidak | Total jam pelajaran efektif diklat (JPL) |
| `skp_points` | `NUMERIC(5,2)` | No | `0.00` | - | Tidak | Bobot poin SKP Kemenkes yang diakui |
| `status` | `VARCHAR(30)` | No | `'Scheduled'` | IDX6 | Tidak | Status: `Scheduled`, `RegistrationOpen`, `InProgress`, `AssessmentPhase`, `Completed`, `Cancelled` |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan sesi pelatihan |
| `created_by` | `UUID` | No | - | - | Tidak | ID user perancang sesi |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengubahan data sesi |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah sesi |

### 21.4 Tabel `trx_training_enrollment_requests` (Pengajuan Pendaftaran Diklat & Penugasan Staf)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (ESS Staf & Diklat RS)
- **Deskripsi:** Pendaftaran peserta ke sesi diklat, baik melalui inisiatif pegawai (*Employee Self-Service* - ESS), penugasan otomatis sistem radar kedaluwarsa, maupun penugasan resmi atasan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas pendaftaran diklat |
| `enrollment_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi pendaftaran (misal `ENR-TRN-2026-0045`) |
| `training_session_id` | `UUID` | No | - | FK (`trx_training_sessions`), IDX2 | Tidak | Relasi ke sesi kelas yang diikuti |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX3 | Tidak | ID pegawai / nakes yang mendaftar |
| `enrollment_source` | `VARCHAR(50)` | No | - | - | Tidak | Sumber pendaftaran: `ESS_SELF_SUBMISSION`, `RADAR_EXPIRY_AUTO_ASSIGNED`, `SUPERVISOR_MANDATED` |
| `cost_amount` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Biaya keikutsertaan pelatihan |
| `funding_source` | `VARCHAR(50)` | No | `'HOSPITAL_BUDGET'` | - | Tidak | Sumber dana: `HOSPITAL_BUDGET`, `PERSONAL_STAFF`, `SPONSOR_THIRD_PARTY` |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX4 | Tidak | Status: `Draft`, `PendingSupervisor`, `PendingDiklat`, `Approved`, `InTraining`, `Completed`, `Failed`, `Cancelled`, `Rejected` |
| `supervisor_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Kepala Ruangan / Supervisor yang menyetujui dispensasi dinas |
| `supervisor_action_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu persetujuan supervisor |
| `diklat_approver_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID staf Diklat yang memvalidasi anggaran dan kuota |
| `diklat_action_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu validasi Diklat |
| `rejection_reason` | `TEXT` | Yes | `NULL` | - | Tidak | Alasan penolakan jika pendaftaran ditolak |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengajuan pendaftaran |
| `created_by` | `UUID` | No | - | - | Tidak | ID pembuat berkas |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu modifikasi berkas |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID pengubah berkas |

### 21.5 Tabel `trx_training_attendances` (Presensi Kehadiran Peserta Sesi Pelatihan)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Bagian Diklat RS)
- **Deskripsi:** Log rekaman absensi dan durasi hadir peserta pada setiap sesi pembelajaran per hari/sesi materi sebagai prasyarat utama keikutsertaan ujian kelulusan.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik log presensi diklat |
| `enrollment_id` | `UUID` | No | - | FK (`trx_training_enrollment_requests`), IDX1 | Tidak | Relasi ke pendaftaran peserta |
| `training_session_id` | `UUID` | No | - | FK (`trx_training_sessions`), IDX2 | Tidak | Relasi ke sesi pelatihan |
| `session_date` | `DATE` | No | - | IDX3 | Tidak | Tanggal kehadiran kelas |
| `check_in_time` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu presisi check-in hadir |
| `check_out_time` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu presisi check-out pulang/selesai sesi |
| `actual_duration_minutes` | `INTEGER` | No | `0` | - | Tidak | Durasi aktual menghadiri materi dalam menit |
| `attendance_status` | `VARCHAR(30)` | No | `'PRESENT'` | - | Tidak | Status: `PRESENT`, `LATE`, `PERMIT`, `ABSENT` |
| `verified_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID instruktur/staf Diklat pemverifikasi daftar hadir |
| `notes` | `VARCHAR(250)` | Yes | `NULL` | - | Tidak | Catatan kehadiran jika terlambat atau izin |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pencatatan presensi |

### 21.6 Tabel `trx_training_assessments` (Hasil Ujian Evaluasi Teori Post-Test & Simulasi Praktik OSCE)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Assessor Klinis & Bagian Diklat RS)
- **Deskripsi:** Hasil evaluasi objektif kelulusan kompetensi yang memisahkan skor teori post-test dari skor ujian simulasi keterampilan klinis langsung (*Objective Structured Clinical Examination* - OSCE).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik evaluasi kompetensi |
| `enrollment_id` | `UUID` | No | - | UQ, FK (`trx_training_enrollment_requests`), IDX1 | Tidak | Relasi ke pendaftaran peserta (satu evaluasi per peserta) |
| `post_test_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Nilai ujian teori post-test tertulis (skala 0 - 100) |
| `post_test_passed` | `BOOLEAN` | No | `FALSE` | - | Tidak | Status kelulusan teori (skor >= `min_theory_passing_score`) |
| `osce_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Nilai ujian simulasi keterampilan klinis langsung OSCE |
| `osce_checklist_data` | `JSONB` | Yes | `NULL` | - | Tidak | Rincian skor butir rubrik penilaian simulasi keterampilan |
| `osce_passed` | `BOOLEAN` | No | `FALSE` | - | Tidak | Status kelulusan ujian praktik simulasi OSCE |
| `assessor_id` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID dokter/perawat instruktur penilai ujian OSCE |
| `assessor_digital_signature`| `TEXT`| Yes | `NULL` | - | Ya | Tanda tangan digital / hash kriptografis pengesahan assessor |
| `assessment_date` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pelaksanaan ujian penilaian |
| `final_outcome` | `VARCHAR(30)` | No | `'PENDING'` | IDX3 | Tidak | Hasil akhir: `PASSED`, `FAILED_THEORY_REMEDIATION`, `FAILED_OSCE_REMEDIATION`, `FAILED_UNQUALIFIED` |
| `remediation_deadline` | `DATE` | Yes | `NULL` | - | Tidak | Batas waktu remedial ujian jika diberi kesempatan perbaikan |
| `feedback_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Catatan umpan balik pembinaan dari penguji klinis |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pencatatan hasil penilaian |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu perbaikan nilai jika remedial |

### 21.7 Tabel `trx_training_certificates` (Sertifikat Pelatihan, Portofolio & Radar Kedaluwarsa)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Bagian Diklat & SDM RS)
- **Deskripsi:** Registri sertifikat pelatihan dan kompetensi staf rumah sakit, terhubung ke radar peringatan kedaluwarsa (H-90, H-60, H-30) dan arsip dokumen digital e-Dossier pegawai Sub-02.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik sertifikat kompetensi |
| `certificate_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor seri resmi sertifikat (internal RS atau lembaga eksternal) |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID pegawai pemilik sertifikat |
| `training_rule_id` | `UUID` | Yes | `NULL` | FK (`mst_mandatory_training_rules`), IDX3 | Tidak | Relasi ke master aturan jika sertifikat pelatihan wajib |
| `enrollment_id` | `UUID` | Yes | `NULL` | FK (`trx_training_enrollment_requests`) | Tidak | Relasi ke berkas diklat internal jika diterbitkan oleh RS |
| `training_title` | `VARCHAR(200)` | No | - | - | Tidak | Nama pelatihan yang tercantum pada sertifikat |
| `issuing_institution` | `VARCHAR(150)` | No | - | - | Tidak | Lembaga penerbit (misal `Bagian Diklat RS Quilvian`, `PERKI`, `Kemenkes RI`) |
| `issue_date` | `DATE` | No | - | IDX4 | Tidak | Tanggal penerbitan sertifikat |
| `expiry_date` | `DATE` | Yes | `NULL` | IDX5 | Tidak | Tanggal kedaluwarsa masa berlaku (NULL jika seumur hidup) |
| `status` | `VARCHAR(30)` | No | `'Active'` | IDX6 | Tidak | Status: `Draft`, `Active`, `ExpiringSoon`, `Expired`, `Revoked` |
| `certificate_document_url`| `VARCHAR(250)`| No | - | - | Ya | Tautan berkas dokumen digital sertifikat (e-Dossier Sub-02) |
| `plataran_sehat_cert_id` | `VARCHAR(100)` | Yes | `NULL` | IDX7 | Tidak | ID sertifikat resmi pada platform Plataran Sehat Kemenkes |
| `skp_points_earned` | `NUMERIC(5,2)` | No | `0.00` | - | Tidak | Poin SKP resmi yang diperoleh |
| `is_verified_by_diklat` | `BOOLEAN` | No | `FALSE` | - | Tidak | Status verifikasi keabsahan oleh staf Diklat jika unggahan eksternal |
| `verified_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID staf Diklat pemeriksa dokumen eksternal |
| `verified_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu verifikasi keabsahan berkas |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pencatatan sertifikat |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaruan data sertifikat |

### 21.8 Tabel `trx_training_budgets` (Pagu Anggaran & Realisasi Biaya Pelatihan per Unit Kerja)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (Manajemen Keuangan & Diklat RS)
- **Deskripsi:** Pengendalian alokasi pagu anggaran diklat tahunan per departemen/unit kerja, memastikan pengajuan pelatihan tidak melampaui sisa pagu biaya yang tersedia.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik pagu anggaran unit |
| `department_id` | `UUID` | No | - | FK (`mst_departments`), IDX1 | Tidak | ID unit kerja / departemen pemilik alokasi pagu |
| `fiscal_year` | `INTEGER` | No | - | IDX2 | Tidak | Tahun anggaran fiskal berjalan (misal `2026`) |
| `allocated_budget` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Pagu anggaran pelatihan yang disetujui untuk unit |
| `committed_budget` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Anggaran yang telah terikat oleh pendaftaran dalam proses approval |
| `realized_budget` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Anggaran yang telah direalisasikan dan dibayarkan |
| `remaining_budget` | `NUMERIC(15,2)` | No | `0.00` | - | Ya | Sisa alokasi pagu anggaran yang masih dapat digunakan |
| `is_frozen` | `BOOLEAN` | No | `FALSE` | - | Tidak | Flag pembekuan anggaran jika terjadi audit atau penyesuaian kas RS |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penetapan pagu anggaran |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu mutasi perhitungan pagu |

### 21.9 Tabel `hrd_training_bond_contracts` (Perjanjian Ikatan Dinas & Komitmen Retensi Staf Diklat)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` & `WorkforceCore` (Legal SDM RS)
- **Deskripsi:** Kontrak ikatan dinas dan retensi kerja staf nakes yang dibiayai rumah sakit untuk pelatihan eksternal/spesialisasi dengan biaya > Rp 10 Juta atau durasi > 1 bulan (`HRD-DEC-162`).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik perjanjian ikatan dinas |
| `contract_number` | `VARCHAR(100)` | No | - | UQ, IDX1 | Tidak | Nomor register akta ikatan dinas (misal `BND-HRD-2026-0012`) |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID staf nakes yang menandatangani ikatan dinas |
| `enrollment_id` | `UUID` | No | - | FK (`trx_training_enrollment_requests`), IDX3 | Tidak | Relasi ke pendaftaran diklat yang dibiayai RS |
| `training_title` | `VARCHAR(200)` | No | - | - | Tidak | Nama pelatihan/studi spesialisasi yang ditempuh |
| `total_sponsorship_amount`| `NUMERIC(15,2)`| No | `0.00` | - | Ya | Total biaya pembiayaan oleh rumah sakit (pelatihan, transport, saku) |
| `retention_period_months`| `INTEGER` | No | `24` | - | Tidak | Masa wajib retensi kerja dalam bulan (misal 12 s.d. 36 bulan) |
| `start_date` | `DATE` | No | - | IDX4 | Tidak | Tanggal mulai berlakunya ikatan dinas (pasca pelatihan selesai) |
| `end_date` | `DATE` | No | - | IDX5 | Tidak | Tanggal berakhirnya ikatan dinas secara normal |
| `penalty_formula_type` | `VARCHAR(50)` | No | `'PRORATED_REMAINING_MONTHS'`| - | Tidak | Metode denda wanprestasi: `FULL_COST_PLUS_FINE_50`, `PRORATED_REMAINING_MONTHS` |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX6 | Tidak | Status: `Draft`, `PendingSigning`, `Active`, `Completed`, `BreachPenalized`, `Discharged` |
| `signed_document_url` | `VARCHAR(250)` | Yes | `NULL` | - | Ya | Berkas PDF akta ikatan dinas bertanda tangan basah / meterai / e-Sign |
| `breach_recorded_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Tanggal pencatatan wanprestasi jika staf mengundurkan diri sepihak |
| `penalty_invoiced_amount`| `NUMERIC(15,2)`| Yes | `NULL` | - | Ya | Nominal tagihan penalti ganti rugi yang dibebankan kepada staf |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penerbitan draft kontrak ikatan dinas |
| `created_by` | `UUID` | No | - | - | Tidak | ID staf HR Legal pembuat kontrak |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu modifikasi data ikatan dinas |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah |

### 21.10 Tabel `trx_plataran_sehat_skp_records` (Pencatatan Portofolio Satuan Kredit Profesi - SKP Kemenkes)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `LearningAndDevelopment` (SDM Nakes & Plataran Sehat Kemenkes)
- **Deskripsi:** Portofolio perolehan Satuan Kredit Profesi (SKP) tenaga medis dan tenaga kesehatan yang dipetakan ke 3 kategori resmi Kemenkes (Pembelajaran, Pelayanan, Pengabdian) untuk perpanjangan SIP 5 tahunan (`INT-HR-80`).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik log portofolio SKP |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX1 | Tidak | ID nakes pemilik perolehan SKP |
| `skp_category` | `VARCHAR(50)` | No | - | IDX2 | Tidak | Kategori: `PEMBELAJARAN_DIKLAT`, `PELAYANAN_KLINIS`, `PENGABDIAN_MASYARAKAT` |
| `activity_title` | `VARCHAR(250)` | No | - | - | Tidak | Nama kegiatan/seminar/pelayanan penghasil SKP |
| `skp_earned` | `NUMERIC(5,2)` | No | `0.00` | - | Tidak | Jumlah poin SKP yang diperoleh |
| `activity_date` | `DATE` | No | - | IDX3 | Tidak | Tanggal perolehan atau pelaksanaan kegiatan |
| `plataran_sehat_id` | `VARCHAR(100)` | Yes | `NULL` | IDX4 | Tidak | ID referensi kegiatan / sertifikat dari Plataran Sehat Kemenkes |
| `sync_status` | `VARCHAR(30)` | No | `'LOCAL_RECORDED'` | - | Tidak | Status: `LOCAL_RECORDED`, `VERIFIED_PLATARAN_SEHAT`, `SYNCED_SATUSEHAT_SDMK`, `REJECTED` |
| `synced_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu sukses sinkronisasi dengan API Kemenkes |
| `supporting_document_url`| `VARCHAR(250)`| Yes | `NULL` | - | Ya | Tautan bukti pendukung sertifikat/surat tugas di e-Dossier |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengentrian data perolehan SKP |


## 22. Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)

Kamus data ini memuat spesifikasi skema relasional modul manajemen kinerja pegawai rumah sakit dan evaluasi jenjang karier klinis nakes (*PerformanceManagement*), mencakup 10 tabel yang mengatur katalog KPI, siklus penilaian tahunan, cascading sasaran, lembar penilaian komposit 60:40, sidang kalibrasi, program PIP 60 hari, review clinical ladder PK I-V, serta penanganan sanggah banding nilai kinerja.

### 22.1 Tabel `mst_kpi_metric_catalogs` (Master Katalog Indikator Kinerja & Perilaku)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (HR Performance & Manajemen Mutu)
- **Deskripsi:** Master katalog metrik target kinerja kuantitatif dan rubrik deskriptor Core Values BerAKHLAK skala likert 1 s.d. 5.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik indikator metrik kinerja |
| `metric_code` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Kode unik metrik (misal `KPI-FIN-01`, `KPI-MED-04`, `BEH-AKH-01`) |
| `metric_name` | `VARCHAR(150)` | No | - | - | Tidak | Nama resmi indikator kinerja atau butir Core Values |
| `component_type` | `VARCHAR(30)` | No | - | IDX2 | Tidak | Tipe komponen: `QUANTITATIVE_KPI`, `CORE_VALUES_BEHAVIOR` |
| `category` | `VARCHAR(50)` | No | - | - | Tidak | Kategori: `FINANCIAL`, `SERVICE_QUALITY`, `PATIENT_SAFETY`, `PEOPLE_CULTURE` |
| `target_profession_group` | `VARCHAR(50)` | No | `'ALL'` | - | Tidak | Kelompok profesi target: `MEDIS`, `KEPERAWATAN`, `PENUNJANG`, `UMUM`, `ALL` |
| `unit_of_measure` | `VARCHAR(30)` | Yes | `NULL` | - | Tidak | Satuan ukur: `PERSEN`, `MENIT`, `RUPIAH`, `JUMLAH_KASUS`, `SKALA_1_5` |
| `target_polarity` | `VARCHAR(20)` | No | `'POSITIVE'` | - | Tidak | Polaritas capaian: `POSITIVE` (makin tinggi makin baik), `NEGATIVE` (makin rendah makin baik) |
| `default_target_value` | `NUMERIC(15,2)` | Yes | `NULL` | - | Tidak | Nilai target patokan standar rumah sakit |
| `scoring_rubric_json` | `JSONB` | Yes | `NULL` | - | Tidak | Panduan rubrik deskriptor operasional skor 1 s.d. 5 |
| `is_active` | `BOOLEAN` | No | `TRUE` | IDX3 | Tidak | Status keaktifan indikator metrik |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan data master |
| `created_by` | `UUID` | No | - | - | Tidak | ID user pembuat |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengubahan data |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah |

### 22.2 Tabel `trx_performance_cycles` (Siklus Periode Evaluasi Kinerja Rumah Sakit)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (HR Performance RS)
- **Deskripsi:** Pengaturan periode siklus penilaian kinerja tahunan atau semesteran rumah sakit, mencakup jadwal tenggat waktu penilaian berjenjang dan sidang kalibrasi.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik siklus penilaian |
| `cycle_code` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Kode unik siklus (misal `CYC-2026-ANNUAL`, `CYC-2026-H1`) |
| `cycle_title` | `VARCHAR(150)` | No | - | - | Tidak | Judul nama siklus evaluasi kinerja |
| `evaluation_year` | `INTEGER` | No | - | IDX2 | Tidak | Tahun evaluasi pelaksanaan kinerja (misal `2026`) |
| `cycle_type` | `VARCHAR(30)` | No | `'ANNUAL'` | - | Tidak | Tipe siklus: `ANNUAL`, `SEMESTER_1`, `SEMESTER_2`, `SPECIAL_PROBATION` |
| `start_date` | `DATE` | No | - | - | Tidak | Tanggal awal periode kerja yang dinilai |
| `end_date` | `DATE` | No | - | - | Tidak | Tanggal akhir periode kerja yang dinilai |
| `self_appraisal_deadline` | `TIMESTAMPTZ` | No | - | - | Tidak | Batas waktu pengisian evaluasi mandiri oleh staf di ESS |
| `manager_appraisal_deadline`| `TIMESTAMPTZ`| No | - | - | Tidak | Batas waktu pengisian nilai oleh Kepala Ruangan / Karu |
| `calibration_deadline` | `TIMESTAMPTZ` | No | - | - | Tidak | Batas waktu pelaksanaan sidang pleno kalibrasi manajemen |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX3 | Tidak | Status: `Draft`, `Active`, `InReview`, `Calibrating`, `Closed`, `Cancelled` |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan siklus |
| `created_by` | `UUID` | No | - | - | Tidak | ID user perancang siklus |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengubahan data siklus |
| `updated_by` | `UUID` | Yes | `NULL` | - | Tidak | ID user pengubah |

### 22.3 Tabel `trx_performance_goals` (Sasaran Kerja Target Individu Pegawai)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (Pegawai & Kepala Ruangan)
- **Deskripsi:** Butir target sasaran kerja kuantitatif individu pegawai yang diturunkan (*cascading*) dari sasaran mutu instalasi/unit kerja, lengkap dengan bobot dan target capaian.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik butir sasaran kerja individu |
| `performance_cycle_id` | `UUID` | No | - | FK (`trx_performance_cycles`), IDX1 | Tidak | Relasi ke siklus kinerja yang berjalan |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID pegawai pemilik sasaran kerja |
| `kpi_metric_catalog_id` | `UUID` | No | - | FK (`mst_kpi_metric_catalogs`), IDX3 | Tidak | Relasi ke master indikator metrik yang diukur |
| `goal_description` | `VARCHAR(250)` | No | - | - | Tidak | Rincian narasi target kerja spesifik individu |
| `weight_percentage` | `NUMERIC(5,2)` | No | - | - | Tidak | Bobot kontribusi target dalam komponen KPI (total 100%) |
| `target_value` | `NUMERIC(15,2)` | No | - | - | Tidak | Nilai target capaian yang disepakati bersama atasan |
| `actual_value` | `NUMERIC(15,2)` | Yes | `NULL` | - | Tidak | Nilai aktual realisasi capaian di akhir periode |
| `achievement_percentage`| `NUMERIC(5,2)`| Yes | `NULL` | - | Tidak | Persentase ketercapaian target individu (`actual / target`) |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX4 | Tidak | Status: `Draft`, `Submitted`, `Approved_Locked`, `Cancelled` |
| `approved_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Kepala Ruangan yang menyetujui target |
| `approved_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu persetujuan target sasaran |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan butir sasaran |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pembaruan data |

### 22.4 Tabel `trx_performance_appraisals` (Lembar Rapor Evaluasi Kinerja Pegawai)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (HRD & Penilai Kinerja)
- **Deskripsi:** Rapor komposit evaluasi kinerja pegawai rumah sakit, menggabungkan 60% KPI hasil kerja dan 40% perilaku Core Values BerAKHLAK, lengkap dengan riwayat skor pra dan pasca kalibrasi.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik lembar rapor evaluasi |
| `performance_cycle_id` | `UUID` | No | - | FK (`trx_performance_cycles`), IDX1 | Tidak | Relasi ke siklus evaluasi |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID pegawai yang dievaluasi (UQ komposit bersama cycle_id) |
| `first_line_manager_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX3 | Tidak | ID Kepala Ruangan / Atasan langsung penilai |
| `second_line_manager_id`| `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`), IDX4 | Tidak | ID Kepala Bidang / Atasan kedua penelaah |
| `kpi_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Skor terbobot komponen hasil kerja KPI kuantitatif (60%) |
| `behavior_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Skor terbobot komponen perilaku Core Values BerAKHLAK (40%) |
| `pre_calibration_score`| `NUMERIC(5,2)` | Yes | `NULL` | - | Ya | Skor komposit awal sebelum sidang kalibrasi |
| `final_composite_score`| `NUMERIC(5,2)` | Yes | `NULL` | IDX5 | Ya | Skor komposit resmi akhir pasca sidang kalibrasi |
| `performance_band` | `VARCHAR(20)` | Yes | `NULL` | IDX6 | Tidak | Predikat: `SANGAT_BAIK_A`, `BAIK_B`, `CUKUP_C`, `KURANG_D`, `BURUK_E` |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX7 | Tidak | Status: `Draft`, `SelfSubmitted`, `ManagerAssessed`, `SecondLineReviewed`, `InCalibration`, `FinalApproved`, `Disputed` |
| `calibration_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Risalah justifikasi komite jika terjadi perubahan nilai |
| `manager_feedback_notes`| `TEXT`| Yes | `NULL` | - | Tidak | Catatan umpan balik pembinaan dari atasan langsung |
| `employee_acknowledged_at`| `TIMESTAMPTZ`| Yes | `NULL` | - | Tidak | Waktu penandatanganan/konfirmasi nilai oleh pegawai di ESS |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu inisiasi lembar evaluasi |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu mutasi data evaluasi |

### 22.5 Tabel `trx_performance_appraisal_details` (Rincian Butir Penilaian KPI & Perilaku)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (Penilai Kinerja)
- **Deskripsi:** Detail butir penilaian individu per metrik KPI hasil kerja dan per butir perilaku Core Values BerAKHLAK, memuat skor evaluasi mandiri dan skor penilai.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik butir rincian penilaian |
| `performance_appraisal_id`| `UUID`| No | - | FK (`trx_performance_appraisals`), IDX1 | Tidak | Relasi ke induk lembar evaluasi kinerja |
| `component_type` | `VARCHAR(30)` | No | - | IDX2 | Tidak | Tipe: `QUANTITATIVE_KPI`, `CORE_VALUES_BEHAVIOR` |
| `metric_or_code` | `VARCHAR(50)` | No | - | - | Tidak | Kode metrik KPI atau butir nilai BerAKHLAK |
| `item_title` | `VARCHAR(200)` | No | - | - | Tidak | Judul nama butir penilaian |
| `weight_percentage` | `NUMERIC(5,2)` | No | - | - | Tidak | Bobot butir dalam komponen bersangkutan |
| `self_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Skor penilaian refleksi mandiri oleh staf |
| `manager_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Skor resmi yang diberikan atasan langsung |
| `final_score` | `NUMERIC(5,2)` | Yes | `NULL` | - | Tidak | Skor akhir butir pasca review dan kalibrasi |
| `evidence_document_url`| `VARCHAR(250)`| Yes | `NULL` | - | Ya | Tautan berkas bukti dukung capaian kinerja di e-Dossier |
| `evaluator_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Catatan deskripsi pengamatan perilaku kerja pegawai |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pembuatan butir rincian |

### 22.6 Tabel `trx_performance_calibration_sessions` (Sidang Pleno Kalibrasi Manajemen RS)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (Komite Kalibrasi & Direksi RS)
- **Deskripsi:** Registri pelaksanaan sidang pleno kalibrasi manajemen rumah sakit, memuat sebaran pedoman fleksibel, sebaran aktual, dan penetapan skor akhir RS.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik sidang kalibrasi |
| `performance_cycle_id` | `UUID` | No | - | FK (`trx_performance_cycles`), IDX1 | Tidak | Relasi ke siklus evaluasi yang dikalibrasi |
| `session_number` | `VARCHAR(50)` | No | - | UQ, IDX2 | Tidak | Nomor registrasi sidang pleno (misal `CAL-2026-PLENO-01`) |
| `session_date` | `TIMESTAMPTZ` | No | - | - | Tidak | Tanggal dan waktu pelaksanaan sidang |
| `guiding_distribution_json`| `JSONB` | No | - | - | Tidak | Parameter pedoman sebaran nilai (10-15% A, 70-80% B, 5-10% C/D) |
| `actual_distribution_json` | `JSONB` | Yes | `NULL` | - | Tidak | Realisasi sebaran nilai komposit RS pasca kalibrasi |
| `total_employees_calibrated`| `INTEGER`| No | `0` | - | Tidak | Jumlah total pegawai yang dievaluasi dalam sidang |
| `total_overrides_applied`| `INTEGER` | No | `0` | - | Tidak | Jumlah pegawai yang nilainya disesuaikan oleh komite |
| `minutes_of_meeting_url`| `VARCHAR(250)`| Yes | `NULL` | - | Ya | Berkas PDF risalah berita acara sidang pleno bertanda tangan Direksi |
| `status` | `VARCHAR(30)` | No | `'Scheduled'` | IDX3 | Tidak | Status: `Scheduled`, `InProgress`, `Completed`, `Cancelled` |
| `completed_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Direktur SDM yang menutup sidang kalibrasi |
| `completed_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengesahan hasil kalibrasi |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penjadwalan sidang |

### 22.7 Tabel `trx_performance_improvement_plans` (Program Peningkatan Kinerja - PIP 60 Hari)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (HR Performance & Atasan Langsung)
- **Deskripsi:** Kontrak formal pelaksanaan program pembinaan kinerja 60 hari kalender bagi pegawai yang memperoleh nilai evaluasi di bawah standar (skor < 65).

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik kontrak pembinaan PIP |
| `pip_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi kontrak PIP (misal `PIP-2026-0034`) |
| `performance_appraisal_id`| `UUID`| No | - | FK (`trx_performance_appraisals`), IDX2 | Tidak | Relasi ke rapor kinerja pemicu pembinaan |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX3 | Tidak | ID pegawai yang menjalani program pembinaan |
| `mentor_workforce_profile_id`| `UUID`| No | - | FK (`mst_workforce_profiles`), IDX4 | Tidak | ID staf senior / mentor pendamping yang ditunjuk |
| `start_date` | `DATE` | No | - | IDX5 | Tidak | Tanggal awal berlakunya kontrak PIP (Hari ke-1) |
| `end_date` | `DATE` | No | - | IDX6 | Tidak | Tanggal batas akhir periode 60 hari kalender (Hari ke-60) |
| `action_plan_summary` | `TEXT` | No | - | - | Tidak | Rangkuman sasaran target perbaikan konkret |
| `status` | `VARCHAR(30)` | No | `'Draft'` | IDX7 | Tidak | Status: `Draft`, `Active`, `Extended`, `Completed`, `Failed`, `Terminated` |
| `final_verdict` | `VARCHAR(30)` | Yes | `NULL` | - | Tidak | Putusan sidang: `PASSED_NORMAL`, `EXTENDED_30_DAYS`, `ESCALATED_HR_LEGAL` |
| `verdict_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Pertimbangan komite atas hasil akhir evaluasi PIP |
| `signed_contract_url` | `VARCHAR(250)` | Yes | `NULL` | - | Ya | Berkas PDF kontrak komitmen PIP bertanda tangan pegawai dan atasan |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu penerbitan kontrak pembinaan |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu mutasi status pembinaan |

### 22.8 Tabel `trx_performance_pip_action_logs` (Logbook Mingguan Monitoring Pembinaan PIP)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (Mentor & Atasan Langsung)
- **Deskripsi:** Catatan berkala monitoring mingguan (Minggu 1 s.d. 8) pelaksanaan rencana aksi perbaikan kinerja oleh mentor dan atasan langsung.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik logbook monitoring mingguan |
| `pip_id` | `UUID` | No | - | FK (`trx_performance_improvement_plans`), IDX1 | Tidak | Relasi ke kontrak PIP induk |
| `week_number` | `INTEGER` | No | - | IDX2 | Tidak | Minggu ke pembinaan (1 s.d. 8, atau 9 s.d. 12 jika perpanjangan) |
| `log_date` | `DATE` | No | - | - | Tidak | Tanggal pelaksanaan evaluasi mingguan |
| `progress_summary` | `TEXT` | No | - | - | Tidak | Ringkasan pencapaian dan kemajuan kinerja pegawai |
| `deficiencies_observed`| `TEXT` | Yes | `NULL` | - | Tidak | Catatan kekurangan atau kendala yang masih ditemukan |
| `mentor_feedback` | `TEXT` | No | - | - | Tidak | Arahan pembinaan dan rekomendasi dari mentor pendamping |
| `is_target_on_track` | `BOOLEAN` | No | `TRUE` | - | Tidak | Status kepatuhan progres: TRUE (sesuai target), FALSE (berisiko gagal) |
| `logged_by` | `UUID` | No | - | FK (`mst_workforce_profiles`) | Tidak | ID user pencatat logbook (Mentor / Karu) |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengentrian data logbook |

### 22.9 Tabel `trx_clinical_career_ladder_reviews` (Evaluasi Jenjang Karier Klinis Nakes - PK I s.d. PK V)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` & Komite Keperawatan / Nakes
- **Deskripsi:** Berkas permohonan dan evaluasi kenaikan jenjang karier profesional perawat klinis/nakes, memvalidasi prasyarat skor KPI minimal >= 75 dan audit logbook komite.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas jenjang karier klinis |
| `review_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi berkas review (misal `LAD-KEP-2026-0089`) |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX2 | Tidak | ID nakes yang mengajukan kenaikan jenjang |
| `performance_appraisal_id`| `UUID`| No | - | FK (`trx_performance_appraisals`), IDX3 | Tidak | Relasi ke rapor kinerja pembukti prasyarat administratif |
| `current_clinical_level`| `VARCHAR(20)` | No | - | - | Tidak | Jenjang klinis aktif saat ini (misal `PK_I`, `PK_II`, `PK_III`, `PK_IV`) |
| `target_clinical_level` | `VARCHAR(20)` | No | - | - | Tidak | Jenjang klinis yang diajukan (misal `PK_II`, `PK_III`, `PK_IV`, `PK_V`) |
| `clinical_experience_years`| `NUMERIC(4,1)`| No | - | - | Tidak | Total masa kerja klinis relevan dalam satuan tahun |
| `meets_performance_gate`| `BOOLEAN` | No | `FALSE` | - | Tidak | Lulus gate skor kinerja tahunan (skor >= 75.00 / predikat "Baik") |
| `meets_competency_gate` | `BOOLEAN` | No | `FALSE` | - | Tidak | Lulus gate sertifikasi kompetensi aktif Sub-16 (`INT-HR-83`) |
| `free_from_sentinel` | `BOOLEAN` | No | `TRUE` | - | Tidak | Bebas dari catatan insiden keselamatan pasien sentinel Sub-15 |
| `portfolio_logbook_score`| `NUMERIC(5,2)`| Yes | `NULL` | - | Tidak | Nilai kelayakan audit portofolio logbook oleh asesor komite |
| `committee_verdict` | `VARCHAR(30)` | No | `'PENDING'` | IDX4 | Tidak | Rekomendasi: `RECOMMENDED`, `DEFERRED_6_MONTHS`, `NOT_RECOMMENDED` |
| `appointment_letter_url`| `VARCHAR(250)`| Yes | `NULL` | - | Ya | Berkas PDF Surat Keputusan (SK) Direktur Utama penetapan jenjang |
| `final_status` | `VARCHAR(30)` | No | `'Draft'` | IDX5 | Tidak | Status: `Draft`, `Submitted`, `Audited`, `CommitteePlenary`, `Approved`, `Rejected` |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu permohonan diajukan |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu pengesahan keputusan |

### 22.10 Tabel `trx_performance_appraisal_disputes` (Sengketa Sanggah Banding Nilai Kinerja)
- **Status Tabel:** `Baru`
- **Modul Pemilik:** `PerformanceManagement` (HR Performance & Komite Banding)
- **Deskripsi:** Registri penanganan sanggah banding resmi pegawai atas hasil evaluasi kinerja pasca kalibrasi, mencakup mediasi HR dalam 7 hari dan putusan final komite dalam 14 hari.

| Nama Kolom | Tipe Data | Nullable? | Default | PK/FK/Index | Sensitif? | Keterangan & Validasi Bisnis |
| :--- | :--- | :---: | :--- | :--- | :---: | :--- |
| `id` | `UUID` | No | `gen_random_uuid()` | PK | Tidak | Pengenal unik berkas sanggah banding |
| `dispute_number` | `VARCHAR(50)` | No | - | UQ, IDX1 | Tidak | Nomor registrasi sanggahan (misal `DSP-2026-0012`) |
| `performance_appraisal_id`| `UUID`| No | - | FK (`trx_performance_appraisals`), IDX2 | Tidak | Relasi ke lembar evaluasi kinerja yang disanggah |
| `workforce_profile_id` | `UUID` | No | - | FK (`mst_workforce_profiles`), IDX3 | Tidak | ID pegawai pemohon banding |
| `dispute_date` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | IDX4 | Tidak | Tanggal presisi pengajuan sanggahan (maks H+7 sejak rilis) |
| `disputed_component` | `VARCHAR(30)` | No | - | - | Tidak | Komponen yang disanggah: `QUANTITATIVE_KPI`, `CORE_VALUES`, `BOTH` |
| `grievance_reason` | `TEXT` | No | - | - | Tidak | Uraian argumentasi keberatan dan kronologi penolakan skor |
| `evidence_document_url`| `VARCHAR(250)`| No | - | - | Ya | Tautan berkas bukti pembanding konkret dari pemohon |
| `mediation_notes` | `TEXT` | Yes | `NULL` | - | Tidak | Notulen mediasi klarifikasi oleh tim HR Performance |
| `final_verdict` | `VARCHAR(30)` | Yes | `NULL` | IDX5 | Tidak | Putusan: `SCORE_UPHELD_ORIGINAL`, `SCORE_REVISED_ADJUSTED`, `REASSIGN_EVALUATOR` |
| `adjusted_composite_score`| `NUMERIC(5,2)`| Yes | `NULL` | - | Ya | Nilai komposit baru jika putusan banding mengabulkan penyesuaian |
| `verdict_delivered_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu penetapan putusan final majelis banding (maks H+14) |
| `decided_by` | `UUID` | Yes | `NULL` | FK (`mst_workforce_profiles`) | Tidak | ID Direktur SDM pemutus perkara sengketa |
| `status` | `VARCHAR(30)` | No | `'Submitted'` | IDX6 | Tidak | Status: `Submitted`, `MediationPhase`, `PlenaryReview`, `Resolved`, `Rejected` |
| `created_at` | `TIMESTAMPTZ` | No | `CURRENT_TIMESTAMP` | - | Tidak | Waktu pengajuan sanggahan |
| `updated_at` | `TIMESTAMPTZ` | Yes | `NULL` | - | Tidak | Waktu penutupan perkara |


---

## 23. Kamus Data Sub-Modul 18: Hubungan Industrial & Terminasi (Slice `S-C5`)

### 23.1 Tabel `trx_disciplinary_incident_reports` (Laporan Insiden Pelanggaran Disiplin & Just Culture)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `incident_report_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik laporan insiden |
| `incident_number` | `VARCHAR(64)` | NOT NULL | - | Nomor dokumen insiden (mis. `INC-2026-11-001`) |
| `employee_id` | `UUID` | NOT NULL | - | ID pegawai terlapor (FK ke `mst_workforce_profiles`) |
| `incident_date` | `TIMESTAMPTZ` | NOT NULL | - | Tanggal dan waktu kejadian insiden |
| `incident_location` | `VARCHAR(255)` | NOT NULL | - | Tempat kejadian (mis. Ruang Rawat Inap Melati) |
| `category` | `VARCHAR(64)` | NOT NULL | - | Kategori (Attendance, Behavioral, ClinicalSafety, Ethics, SOPViolation) |
| `just_culture_classification` | `VARCHAR(32)` | NOT NULL | `'HumanError'` | Klasifikasi Just Culture: `HumanError`, `AtRiskBehavior`, `RecklessMisconduct` |
| `description` | `TEXT` | NOT NULL | - | Kronologi rinci kejadian insiden |
| `reporter_type` | `VARCHAR(32)` | NOT NULL | - | Jenis pelapor: `Self`, `Superior`, `Peer`, `Committee`, `PatientGrievance` |
| `reporter_employee_id` | `UUID` | NULL | - | ID pegawai pelapor jika bukan anonim |
| `kmkp_audit_reference_number`| `VARCHAR(64)` | NULL | - | Nomor referensi investigasi KMKP untuk insiden klinis (`INT-HR-86`) |
| `status` | `VARCHAR(32)` | NOT NULL | `'Reported'` | Status: `Reported`, `Investigating`, `ResolvedNoSanction`, `SanctionRecommended` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pencatatan laporan |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data terakhir |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pembuat data |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data terakhir |

---

### 23.2 Tabel `trx_disciplinary_investigation_records` (Berita Acara Pemeriksaan - BAP Disiplin)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `investigation_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik berkas pemeriksaan BAP |
| `incident_report_id` | `UUID` | NOT NULL | - | FK ke `trx_disciplinary_incident_reports` |
| `bap_number` | `VARCHAR(64)` | NOT NULL | - | Nomor resmi dokumen BAP (mis. `BAP-HRD-2026-089`) |
| `hearing_date` | `TIMESTAMPTZ` | NOT NULL | - | Tanggal dan jam pelaksanaan sidang klarifikasi/BAP |
| `investigator_names` | `TEXT` | NOT NULL | - | Daftar nama tim pemeriksa HR Legal / Komite Disiplin |
| `witness_statements` | `TEXT` | NULL | - | Rangkuman kesaksian saksi-saksi |
| `evidence_document_urls` | `TEXT` | NULL | - | URL berkas dokumen bukti fisik/rekaman CCTV/log SIMRS |
| `employee_defense_statement`| `TEXT` | NULL | - | Pernyataan pembelaan tertulis dari pegawai terperiksa |
| `investigation_findings` | `TEXT` | NOT NULL | - | Fakta hukum dan temuan kesimpulan tim pemeriksa |
| `recommended_action` | `VARCHAR(64)` | NOT NULL | - | Rekomendasi: `Coaching`, `VerbalWarning`, `SP1`, `SP2`, `SP3`, `Suspension`, `Termination` |
| `status` | `VARCHAR(32)` | NOT NULL | `'Draft'` | Status BAP: `Draft`, `Submitted`, `ApprovedByHrDirector`, `Rejected` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pembuatan dokumen BAP |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data BAP |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas penyusun BAP |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data BAP |

---

### 23.3 Tabel `trx_disciplinary_sanctions` (Surat Peringatan & Sanksi Disiplin PP 35/2021)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `sanction_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik surat sanksi disiplin |
| `investigation_id` | `UUID` | NULL | - | FK ke berkas BAP jika diterbitkan melalui sidang formal |
| `employee_id` | `UUID` | NOT NULL | - | ID pegawai penerima sanksi (FK ke `mst_workforce_profiles`) |
| `sanction_number` | `VARCHAR(64)` | NOT NULL | - | Nomor registrasi surat sanksi (mis. `SP-02/HRD/XI/2026`) |
| `sanction_level` | `VARCHAR(32)` | NOT NULL | - | Tingkat: `VerbalWarning`, `SP1`, `SP2`, `SP3` |
| `issued_date` | `DATE` | NOT NULL | - | Tanggal resmi surat sanksi diterbitkan |
| `valid_until` | `DATE` | NOT NULL | - | Tanggal kedaluwarsa sanksi (`IssuedDate + 6 Months` berdasar PP 35/2021) |
| `violation_description` | `TEXT` | NOT NULL | - | Uraian pasal tata tertib yang dilanggar |
| `status` | `VARCHAR(32)` | NOT NULL | `'Active'` | Status sanksi: `Active`, `Expired`, `RevokedOnAppeal` |
| `is_appealed` | `BOOLEAN` | NOT NULL | `FALSE` | Indikator apakah staf mengajukan sanggah banding |
| `appeal_resolution` | `TEXT` | NULL | - | Putusan akhir sidang banding Direksi jika ada |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pencatatan data sanksi |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data sanksi |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pejabat penerbit |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data terakhir |

---

### 23.4 Tabel `trx_disciplinary_suspensions` (Skorsing Investigasi - Paid Suspension)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `suspension_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik berkas skorsing investigasi |
| `investigation_id` | `UUID` | NOT NULL | - | FK ke berkas BAP dasar penjatuhan skorsing |
| `employee_id` | `UUID` | NOT NULL | - | ID pegawai yang diskorsing (FK ke `mst_workforce_profiles`) |
| `suspension_number` | `VARCHAR(64)` | NOT NULL | - | Nomor surat penetapan skorsing |
| `start_date` | `DATE` | NOT NULL | - | Tanggal efektif mulai berlakunya skorsing |
| `expected_end_date` | `DATE` | NOT NULL | - | Batas akhir skorsing (Maksimal 6 bulan sesuai regulasi) |
| `actual_end_date` | `DATE` | NULL | - | Tanggal aktual berakhirnya masa skorsing |
| `wage_payment_status` | `VARCHAR(32)` | NOT NULL | `'PaidFull'` | Status hak upah: `PaidFull` (100% upah pokok & tunjangan tetap Pasal 157A) |
| `is_system_access_revoked`| `BOOLEAN` | NOT NULL | `TRUE` | Status pemblokiran akun login SSO SIMRS, EMR, PACS (`INT-HR-88`) |
| `is_physical_access_revoked`| `BOOLEAN` | NOT NULL | `TRUE` | Status penonaktifan kartu akses fisik pintu rumah sakit |
| `status` | `VARCHAR(32)` | NOT NULL | `'Active'` | Status: `Active`, `CompletedReinstated`, `EscalatedToTermination` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pencatatan penetapan skorsing |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data skorsing |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pejabat pembuat |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.5 Tabel `trx_employee_grievances` (Kanal Pengaduan Keluhan Staf & Whistleblowing)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `grievance_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik tiket pengaduan |
| `ticket_number` | `VARCHAR(64)` | NOT NULL | - | Nomor tiket (mis. `GRV-2026-11-0042`) |
| `complainant_employee_id`| `UUID` | NULL | - | ID staf pengadu jika bukan mode anonim total |
| `is_whistleblower_case` | `BOOLEAN` | NOT NULL | `FALSE` | Penanda kasus whistleblower kritis (fraud/pelecehan) |
| `is_anonymous` | `BOOLEAN` | NOT NULL | `FALSE` | Permintaan identitas anonim dari pengadu |
| `encrypted_report_payload` | `TEXT` | NOT NULL | - | Uraian laporan terenkripsi asimetris level aplikasi |
| `category` | `VARCHAR(64)` | NOT NULL | - | Kategori (InterpersonalConflict, ShiftDispute, Workload, Fraud, Harassment) |
| `target_unit` | `VARCHAR(128)` | NULL | - | Unit kerja lokasi dugaan terjadinya masalah |
| `severity_level` | `VARCHAR(32)` | NOT NULL | `'Medium'` | Tingkat urgensi: `Low`, `Medium`, `High`, `Critical` |
| `target_resolution_date` | `DATE` | NOT NULL | - | Batas SLA penyelesaian (maksimal 14 hari kerja) |
| `status` | `VARCHAR(32)` | NOT NULL | `'Submitted'` | Status tiket: `Submitted`, `UnderReview`, `MediationScheduled`, `Resolved`, `Closed` |
| `resolution_notes` | `TEXT` | NULL | - | Notulen kesepakatan damai atau putusan mediasi HR |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pengiriman tiket pengaduan |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran status tiket |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengirim |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.6 Tabel `trx_bipartite_negotiation_forums` (Forum Perundingan Bipartit RS)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `forum_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik agenda sidang Bipartit |
| `register_number` | `VARCHAR(64)` | NOT NULL | - | Nomor registrasi sidang Bipartit (mis. `BIP-2026-003`) |
| `meeting_date` | `TIMESTAMPTZ` | NOT NULL | - | Tanggal dan jam pelaksanaan perundingan |
| `dispute_type` | `VARCHAR(64)` | NOT NULL | - | Jenis: `RightsDispute`, `InterestDispute`, `TerminationDispute`, `UnionDispute` |
| `management_representatives`| `TEXT` | NOT NULL | - | Daftar delegasi manajemen rumah sakit |
| `union_representatives` | `TEXT` | NOT NULL | - | Daftar delegasi serikat pekerja / perwakilan buruh |
| `agenda_description` | `TEXT` | NOT NULL | - | Pokok permasalahan yang dirundingkan |
| `minutes_of_meeting` | `TEXT` | NOT NULL | - | Risalah jalannya perundingan Bipartit |
| `joint_agreement_document_url`| `VARCHAR(512)`| NULL | - | URL berkas naskah Perjanjian Bersama (PB) yang ditandatangani |
| `status` | `VARCHAR(32)` | NOT NULL | `'Scheduled'` | Status: `Scheduled`, `CompletedAgreementReached`, `Deadlocked`, `Cancelled` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pencatatan agenda |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pencatat |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.7 Tabel `trx_employee_separations` (Pemutusan Hubungan Kerja - PHK)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `separation_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik proses pemutusan hubungan kerja |
| `separation_number` | `VARCHAR(64)` | NOT NULL | - | Nomor registrasi PHK (mis. `SEP-2026-11-015`) |
| `employee_id` | `UUID` | NOT NULL | - | ID staf yang putus kerja (FK ke `mst_workforce_profiles`) |
| `separation_reason` | `VARCHAR(64)` | NOT NULL | - | Alasan: `VoluntaryResign`, `ContractEnd`, `Retirement`, `DisciplinarySP3`, `SevereMisconduct`, `Deceased` |
| `notice_date` | `DATE` | NOT NULL | - | Tanggal penyampaian surat pemberitahuan/pengunduran diri |
| `effective_date` | `DATE` | NOT NULL | - | Tanggal resmi pengakhiran ikatan kerja |
| `resignation_letter_url` | `VARCHAR(512)` | NULL | - | URL dokumen surat pengunduran diri / penetapan PHK |
| `termination_decision_number`| `VARCHAR(128)`| NULL | - | Nomor SK Direksi pengesahan PHK |
| `approval_status` | `VARCHAR(32)` | NOT NULL | `'Pending'` | Status persetujuan: `Pending`, `ApprovedByDirector`, `Rejected` |
| `status` | `VARCHAR(32)` | NOT NULL | `'Initiated'` | Status alur: `Initiated`, `ClearanceInProgress`, `FinalizedCompleted`, `Cancelled` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu inisiasi pengajuan terminasi |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran data terminasi |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas inisiator |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.8 Tabel `trx_severance_calculations` (Kalkulasi Pesangon & Kompensasi PP 35/2021)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `calculation_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik lembar hitung pesangon |
| `separation_id` | `UUID` | NOT NULL | - | FK ke `trx_employee_separations` |
| `base_monthly_wage` | `NUMERIC(14,2)`| NOT NULL | - | Upah dasar acuan (Upah Pokok + Tunjangan Tetap) |
| `service_years` | `INT` | NOT NULL | - | Masa kerja bulat dalam tahun |
| `service_months` | `INT` | NOT NULL | - | Sisa bulan masa kerja |
| `severance_pay_amount` | `NUMERIC(14,2)`| NOT NULL | `0` | Nominal Uang Pesangon (UP) |
| `service_award_amount` | `NUMERIC(14,2)`| NOT NULL | `0` | Nominal Uang Penghargaan Masa Kerja (UPMK) |
| `rights_compensation_amount`| `NUMERIC(14,2)`| NOT NULL | `0` | Nominal Uang Penggantian Hak (UPH: cuti tahunan, ongkos) |
| `separation_allowance_amount`| `NUMERIC(14,2)`| NOT NULL | `0` | Nominal Uang Pisah (sesuai PP/PKB) |
| `total_severance_gross` | `NUMERIC(14,2)`| NOT NULL | `0` | Total bruto seluruh komponen pesangon |
| `tax_deduction` | `NUMERIC(14,2)`| NOT NULL | `0` | Potongan Pajak Penghasilan Pasal 21 Final atas Pesangon |
| `total_severance_net` | `NUMERIC(14,2)`| NOT NULL | `0` | Total bersih yang disetor ke pegawai |
| `status` | `VARCHAR(32)` | NOT NULL | `'Draft'` | Status lembar hitung: `Draft`, `VerifiedByHr`, `ApprovedByFinanceDirector` |
| `is_approved_by_finance_director`| `BOOLEAN`| NOT NULL | `FALSE` | Kunci otorisasi Direktur Keuangan sebelum disinkronkan ke payroll |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu kalkulasi pertama kali |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran nilai |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas kalkulator |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.9 Tabel `trx_exit_clearance_processes` (Induk Proses Exit Clearance Pegawai)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `clearance_process_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik berkas kliring keluar |
| `separation_id` | `UUID` | NOT NULL | - | FK ke `trx_employee_separations` |
| `employee_id` | `UUID` | NOT NULL | - | ID pegawai offboarding (FK ke `mst_workforce_profiles`) |
| `initiated_date` | `DATE` | NOT NULL | - | Tanggal dimulainya proses exit clearance |
| `target_completion_date`| `DATE` | NOT NULL | - | Batas tanggal seluruh unit harus selesai memverifikasi |
| `actual_completion_date`| `DATE` | NULL | - | Tanggal aktual seluruh 5 unit berstatus `Approved` |
| `overall_clearance_status`| `VARCHAR(32)` | NOT NULL | `'InProgress'` | Status: `InProgress`, `Completed`, `BlockedOutstanding` |
| `is_final_payroll_unlocked`| `BOOLEAN` | NOT NULL | `FALSE` | Kunci gembok pembuka rilis gaji akhir di Sub-10 (`INT-HR-87`) |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pembukaan berkas kliring |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu pemutakhiran berkas |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas inisiator |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pengubah data |

---

### 23.10 Tabel `trx_exit_clearance_checklists` (Checklist Serah Terima Multi-Departemen)

| Kolom | Tipe Data | Nullable | Default | Deskripsi / Domain |
|---|---|---|---|---|
| `checklist_id` | `UUID` | NOT NULL | `gen_random_uuid()` | Kunci primer unik butir checklist kliring |
| `clearance_process_id` | `UUID` | NOT NULL | - | FK ke `trx_exit_clearance_processes` |
| `department_type` | `VARCHAR(32)` | NOT NULL | - | Departemen: `WorkUnitHandover`, `GeneralAffairsAsset`, `PharmacyMedicalDevice`, `FinanceCooperative`, `InformationTechnology` |
| `item_name` | `VARCHAR(255)` | NOT NULL | - | Nama barang/kewajiban (mis. Kunci Ruangan, ID Card, Stetoskop, Pinjaman, Akun SSO) |
| `is_returned` | `BOOLEAN` | NOT NULL | `FALSE` | Konfirmasi fisik barang telah diserahkan kembali |
| `outstanding_liability_amount`| `NUMERIC(14,2)`| NOT NULL | `0` | Nominal tanggungan ganti rugi/sisa utang jika ada |
| `approver_notes` | `TEXT` | NULL | - | Catatan verifikator departemen terkait |
| `approved_by_user_id` | `UUID` | NULL | - | ID user penandatangan digital departemen |
| `approved_at` | `TIMESTAMPTZ` | NULL | - | Waktu persetujuan digital diberikan |
| `status` | `VARCHAR(32)` | NOT NULL | `'Pending'` | Status butir: `Pending`, `Approved`, `RejectedOutstanding` |
| `created_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu inisiasi item checklist |
| `updated_at` | `TIMESTAMPTZ` | NOT NULL | `NOW()` | Waktu verifikasi item |
| `created_by` | `VARCHAR(128)` | NOT NULL | - | Identitas pembuat item |
| `updated_by` | `VARCHAR(128)` | NOT NULL | - | Identitas verifikator |
