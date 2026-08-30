# Human Resource — Kontrak API

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/api-contract.md` |
| `contract_version` | `v2` — angka set kontrak disimpan di `blueprint-manifest.md` field `contract_versions` |
| `last_changed_in` | `v2` |
| Status | `draft` — **belum** `approved` |
| Owner | Backend, mengikuti `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` |
| `approved_by` / `approved_at` | **Belum ada** |
| `input_revision` | `02-backend-architecture.md` revision `1`; `00-interview-decisions.md` revision `12` |
| `input_hash` — decision log | `0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3` |
| Backend SHA | `e0ee42c752a5f92c5b1663ff88bef07a5859f79f` |
| Dampak kompatibilitas | **Tidak ada perubahan yang memutus kontrak berjalan.** Seluruh perubahan bersifat penambahan endpoint atau penambahan route template alias |

---

## 0. Cara membaca dokumen ini

Setiap grup endpoint memakai **judul persis nilai `[Tags(...)]`** pada controller, diikuti base
URL dan tabel API. Ini agar pembaca dapat mencocokkan dokumen dengan halaman Swagger tanpa
menebak.

| Penanda pada kolom Status | Artinya |
| --- | --- |
| *(kosong)* | Endpoint **sudah ada** di kode pada baseline saat ini dan dapat dipakai |
| **`Rencana (belum tersedia)`** | Endpoint **belum ada**. Ini target desain, bukan sesuatu yang bisa dipanggil hari ini |

Kolom **Hak akses** pada dokumen ini adalah **satu-satunya** tempat pemetaan endpoint ke hak
akses hidup di seluruh blueprint. `contracts/permission-audit-matrix.md` **MUST NOT** mendaftar
ulang endpoint yang sama.

### 0.1 Pola yang berlaku untuk seluruh endpoint HR

| Hal | Konvensi |
| --- | --- |
| Awalan route | `api/v1/` |
| Area korporat | `api/v1/corporate/human-resource/...` |
| Area layanan mandiri | `api/v1/self-services/human-resource/...` |
| Pembungkus respons | `ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)` |
| Daftar berhalaman | `PagedResult<T>` di dalam `ApiResponse<T>` |
| Constraint route | `{id:guid}` bila identifier-nya `Guid` |
| Autentikasi | `[Authorize]` pada kelas — **150 dari 150** controller HR memilikinya |
| Hak akses | `[AccessController]` di kelas; `[AccessAction]` dan `[AccessPermission("Resource","Action")]` di setiap endpoint |

### 0.2 Tiga endpoint pendukung yang seragam

Hampir seluruh controller HR menyediakan tiga endpoint yang bentuknya sama. Ketiganya tidak
diulang pada setiap tabel di bawah kecuali ada yang khas.

| Method | Path | Kegunaan bagi pengguna |
| --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil pilihan saringan, urutan, dan daftar status beserta labelnya, supaya halaman dapat dirender tanpa menebak |
| `GET` | `/summary` | Mengambil angka ringkasan untuk kartu statistik di kepala halaman |
| `GET` | `/options` | Mengambil data ringan untuk pilihan pada formulir lain |

**Aturan:** `GET /` dan `GET /options` adalah dua endpoint berbeda dengan tujuan berbeda. Jangan
memakai daftar utama sebagai sumber pilihan, dan jangan memakai `/options` sebagai tabel.

### 0.3 Kode status dan artinya bagi pengguna

Berlaku untuk seluruh endpoint pada dokumen ini kecuali disebut lain.

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Permintaan berhasil, data dikembalikan |
| `201` | Data baru berhasil dibuat |
| `204` | Berhasil, tidak ada data yang perlu ditampilkan |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `401` | Sesi Anda sudah berakhir. Silakan masuk kembali |
| `403` | Anda tidak punya hak akses untuk tindakan ini |
| `404` | Data yang dicari tidak ditemukan, atau sudah dihapus |
| `409` | Tindakan ini bertabrakan dengan keadaan data saat ini — misalnya periode sudah tertutup, atau dua petugas mengubah data yang sama pada waktu hampir bersamaan |
| `422` | Data yang dikirim benar bentuknya, tetapi melanggar aturan bisnis |
| `500` | Terjadi gangguan pada sistem. Laporkan kepada tim teknis beserta waktu kejadiannya |

---

## 1. Master Data

Enam puluh lima controller master data HR menyediakan **618 endpoint** dan seluruhnya mengikuti
bentuk baku sembilan endpoint. Karena bentuknya identik, dokumen ini menuliskannya **sekali**
sebagai pola, lalu mendaftar base URL-nya.

### Corporate / Human Resource / Master Data / *(pola baku)*

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi saringan dan form | `<Resource> : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Mengambil ringkasan jumlah data | `<Resource> : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Mengambil daftar dengan saringan, pencarian, urutan, dan halaman | `<Resource> : Read` | Query | `ApiResponse<PagedResult<ListResponse>>` | |
| `GET` | `/options` | Mengambil data ringan untuk pilihan | `<Resource> : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/{id:guid}` | Mengambil detail satu data | `<Resource> : Read` | — | `ApiResponse<DetailResponse>` | |
| `POST` | `/` | Membuat data baru | `<Resource> : Create` | `Create<Entity>Request` | `ApiResponse<DetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah seluruh field bisnis | `<Resource> : Update` | `Update<Entity>Request` | `ApiResponse<DetailResponse>` | |
| `PATCH` | `/{id:guid}/status` | Mengubah status aktif atau tidak aktif saja | `<Resource> : Update` | `Update<Entity>StatusRequest` | `ApiResponse<DetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai data terhapus tanpa menghapus barisnya | `<Resource> : Delete` | — | `ApiResponse<object>` | |

**Contoh terisi.** Untuk grup `Corporate / Human Resource / Master Data / Leave and Overtime /
Leave Type`, base URL-nya `api/v1/corporate/human-resource/master-data/leave-types` dan hak
aksesnya `LeaveType : Read`, `LeaveType : Create`, `LeaveType : Update`, `LeaveType : Delete`.

### 1.1 Delapan route yang mendapat alias kebab-case — `S0-B`

`[DECISION]` `HRD-DEC-016`: kebab-case adalah nama canonical; nama lama **tetap hidup** sebagai
alias yang dilayani **action yang sama**. Bukan hard breaking rename.

| Nama canonical | Alias lama yang tetap hidup | Controller | Status |
| --- | --- | --- | --- |
| `master-data/action-types` | `master-data/actiontypes` | `DisciplinaryActionTypeController` | **Rencana (belum tersedia)** |
| `master-data/case-types` | `master-data/casetypes` | `EmployeeRelationCaseTypeController` | **Rencana (belum tersedia)** |
| `master-data/sanction-types` | `master-data/sanctiontypes` | `SanctionTypeController` | **Rencana (belum tersedia)** |
| `master-data/violation-types` | `master-data/violationtypes` | `ViolationTypeController` | **Rencana (belum tersedia)** |
| `master-data/work-calendars` | `master-data/workcalendars` | `WorkCalendarController` | **Rencana (belum tersedia)** |
| `master-data/work-schedules` | `master-data/workschedules` | `WorkScheduleController` | **Rencana (belum tersedia)** |
| `master-data/shift-groups` | `master-data/shiftgroups` | `ShiftGroupController` | **Rencana (belum tersedia)** |
| `master-data/shift-patterns` | `master-data/shiftpatterns` | `ShiftPatternController` | **Rencana (belum tersedia)** |

**Aturan yang mengikat implementasinya: satu action, satu implementasi.** Alias hanya menambah
route template pada action yang sama. Dilarang menggandakan controller, service, validasi, atau
aturan bisnis hanya untuk melayani nama lama. Bila dua nama menghasilkan dua implementasi,
`HRD-DEC-016` dilanggar.

Enam route yang **tidak** ikut diseragamkan karena sudah benar sebagai kata tunggal berbentuk
jamak: `shifts`, `doctors`, `employees`, `competencies`, `professions`, `specializations`.
Route `organization` yang berbentuk tunggal dinilai terpisah — `[OPEN]` `HRD-Q-14`.

---

## 2. Kehadiran

### Corporate / Human Resource / Attendance Management / Attendance Period

Base URL: `api/v1/corporate/human-resource/attendance/periods`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan periode | `AttendancePeriod : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan jumlah periode per keadaan | `AttendancePeriod : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar periode kehadiran | `AttendancePeriod : Read` | Query | `ApiResponse<PagedResult<PeriodResponse>>` | |
| `GET` | `/options` | Pilihan periode untuk formulir lain | `AttendancePeriod : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu periode | `AttendancePeriod : Read` | — | `ApiResponse<PeriodDetailResponse>` | |
| `POST` | `/` | Membuka periode baru | `AttendancePeriod : Create` | `CreateAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah rentang tanggal atau keterangan periode | `AttendancePeriod : Update` | `UpdateAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | |
| `GET` | `/{id:guid}/close-preview` | Melihat apa saja yang masih menghalangi penutupan | `AttendancePeriod : Read` | — | `ApiResponse<ClosePreviewResponse>` | |
| `POST` | `/{id:guid}/enqueue-processing` | Mengantrikan pemrosesan kehadiran seluruh periode | `AttendancePeriod : Process` | `EnqueueProcessingRequest` | `ApiResponse<SchedulerJobResponse>` | |
| `POST` | `/{id:guid}/close` | Menutup periode | `AttendancePeriod : Close` | `CloseAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | |
| `POST` | `/{id:guid}/reopen` | Membuka kembali periode yang sudah ditutup | `AttendancePeriod : Reopen` | `ReopenAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan periode | `AttendancePeriod : Cancel` | `CancelAttendancePeriodRequest` | `ApiResponse<PeriodDetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai periode terhapus | `AttendancePeriod : Delete` | — | `ApiResponse<object>` | |

**Kode status khas grup ini.** `409` pada `close` berarti masih ada pengecualian pemblokir
berstatus `Open`/`UnderReview`, atau masih ada permohonan koreksi yang berjalan. `409` pada
`reopen` berarti periodenya bukan `Closed`, atau sudah ada hari yang tertaut payroll, atau masih
ada pekerjaan terjadwal yang berjalan.

### Corporate / Human Resource / Attendance Management / Attendance Daily

Base URL: `api/v1/corporate/human-resource/attendance/dailies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan kehadiran harian | `AttendanceDaily : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan hadir, terlambat, tidak hadir | `AttendanceDaily : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar kehadiran harian | `AttendanceDaily : Read` | Query | `ApiResponse<PagedResult<DailyResponse>>` | |
| `GET` | `/payroll-readiness` | Kesiapan data kehadiran untuk diserahkan ke payroll | `AttendanceDaily : Read` | Query | `ApiResponse<PayrollReadinessResponse>` | |
| `GET` | `/{id:guid}` | Detail kehadiran satu pegawai pada satu hari | `AttendanceDaily : Read` | — | `ApiResponse<DailyDetailResponse>` | |
| `GET` | `/{id:guid}/segments` | Rincian potongan waktu kerja, istirahat, dan lembur | `AttendanceDaily : Read` | — | `ApiResponse<List<SegmentResponse>>` | |
| `GET` | `/{id:guid}/exceptions` | Daftar pengecualian pada hari itu | `AttendanceDaily : Read` | — | `ApiResponse<List<ExceptionResponse>>` | |
| `GET` | `/{id:guid}/raw-logs` | Rekaman mentah yang menjadi dasar hasil olahan | `AttendanceDaily : Read` | — | `ApiResponse<List<RawLogResponse>>` | |

**Catatan penting.** Grup ini **tidak punya** endpoint `POST`, `PUT`, maupun `DELETE`, dan itu
disengaja. Kehadiran harian adalah **hasil olahan**; ia diubah lewat koreksi dan pemrosesan
ulang, bukan disunting langsung.

### Corporate / Human Resource / Attendance Management / Attendance Raw Log

Base URL: `api/v1/corporate/human-resource/attendance/raw-logs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan rekaman mentah | `AttendanceRawLog : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan rekaman per keadaan pemrosesan | `AttendanceRawLog : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar rekaman mentah | `AttendanceRawLog : Read` | Query | `ApiResponse<PagedResult<RawLogResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu rekaman | `AttendanceRawLog : Read` | — | `ApiResponse<RawLogDetailResponse>` | |
| `POST` | `/` | Memasukkan satu rekaman baru, misalnya dari integrasi mesin | `AttendanceRawLog : Create` | `CreateRawLogRequest` | `ApiResponse<RawLogDetailResponse>` | |
| `POST` | `/batch` | Memasukkan banyak rekaman sekaligus, misalnya impor harian | `AttendanceRawLog : Create` | `CreateRawLogBatchRequest` | `ApiResponse<BatchResultResponse>` | |
| `POST` | `/{id:guid}/retry` | Memproses ulang rekaman yang gagal dipetakan | `AttendanceRawLog : Update` | — | `ApiResponse<RawLogDetailResponse>` | |

**Invariant yang tidak boleh dilanggar.** Tidak ada endpoint `PUT` maupun `DELETE` pada grup ini,
dan itu **disengaja**. Rekaman mentah adalah fakta. `retry` hanya memproses ulang pemetaannya,
tidak mengubah isi rekamannya.

### Corporate / Human Resource / Attendance Management / Attendance Processing

Base URL: `api/v1/corporate/human-resource/attendance/processing`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan riwayat pemrosesan | `AttendanceProcessing : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan hasil pemrosesan | `AttendanceProcessing : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar riwayat pemrosesan | `AttendanceProcessing : Read` | Query | `ApiResponse<PagedResult<RunResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu pemrosesan beserta kesalahannya | `AttendanceProcessing : Read` | — | `ApiResponse<RunDetailResponse>` | |
| `POST` | `/process` | Memproses kehadiran satu pegawai pada satu tanggal | `AttendanceProcessing : Create` | `ProcessSingleRequest` | `ApiResponse<RunDetailResponse>` | |
| `POST` | `/process-range` | Memproses kehadiran satu rentang tanggal | `AttendanceProcessing : Create` | `ProcessRangeRequest` | `ApiResponse<RunDetailResponse>` | |
| `POST` | `/attendance-dailies/{attendanceDailyId:guid}/reprocess` | Memproses ulang satu hari saja | `AttendanceProcessing : Update` | `ReprocessRequest` | `ApiResponse<DailyDetailResponse>` | |

### Corporate / Human Resource / Attendance Management / Attendance Correction Administration

Base URL: `api/v1/corporate/human-resource/attendance/correction-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan permohonan koreksi | `AttendanceCorrection : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan permohonan per keadaan | `AttendanceCorrection : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar permohonan koreksi seluruh pegawai | `AttendanceCorrection : Read` | Query | `ApiResponse<PagedResult<CorrectionResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu permohonan | `AttendanceCorrection : Read` | — | `ApiResponse<CorrectionDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan permohonan | `AttendanceCorrection : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/workflow/synchronize` | Menyelaraskan status permohonan dengan mesin persetujuan | `AttendanceCorrection : Synchronize` | — | `ApiResponse<CorrectionDetailResponse>` | **Diperketat** — lihat catatan |
| `POST` | `/{id:guid}/apply` | Menerapkan koreksi yang sudah disetujui ke kehadiran harian | `AttendanceCorrection : Apply` | — | `ApiResponse<CorrectionDetailResponse>` | |
| `GET` | `/{id:guid}/evidence/download` | Mengunduh bukti yang dilampirkan | `AttendanceCorrection : Read` | — | Berkas | |
| `POST` | `/on-behalf` | Mengajukan koreksi atas nama pegawai yang tidak dapat mengakses layanan mandiri | `AttendanceCorrection : CreateOnBehalf` | `CreateCorrectionOnBehalfRequest` | `ApiResponse<CorrectionDetailResponse>` | **Rencana (belum tersedia)** |

**Catatan pada `synchronize`** `[DECISION]` `HRD-DEC-022`: endpoint ini **MUST NOT** menurunkan
permohonan berstatus `Applied` kembali ke `Approved` atau status sebelumnya mana pun. Keadaan
hari ini melanggar aturan itu — `MapRequestStatus` menulis status tanpa memeriksa status saat
ini, sehingga apply dapat berjalan dua kali dan memutasi ulang kehadiran harian. Ini tercatat
sebagai **`IMPLEMENTATION DEFECT / REPAIR`**, bukan perilaku target.

**Catatan pada `on-behalf`** `[DECISION]` `HRD-DEC-028`: permintaan **wajib** memuat pegawai yang
diwakili, alasan mengapa pegawai tidak dapat mengajukan sendiri, dan bukti bila kebijakan
menuntutnya. Sistem menyimpan initiator, waktu, dan jejak audit, lalu memberi tahu pegawai yang
bersangkutan. **Tidak ada jalur persetujuan baru** — persetujuannya tetap memakai workflow
koreksi yang berlaku.

### Corporate / Human Resource / Attendance Management / Attendance Correction Monitoring

Base URL: `api/v1/corporate/human-resource/attendance/correction-monitoring`

Menyediakan pemantauan koreksi lintas pegawai beserta perbaikan massal. Hak akses
`AttendanceCorrectionMonitoring : Read` dan `: Repair`.

> **Batas MVP — `HRD-DEC-035`.** Grup di bawah tetap **di dalam MVP** karena ia menghasilkan
> **masukan HR yang siap payroll**. Yang **keluar** dari MVP adalah orkestrasi putaran payroll:
> pembuatan `TrxPayrollRun`, pemajuan statusnya, perhitungan, persetujuan, dan serah terima final.
> Keduanya jangan tertukar — `Payroll Executed` **MUST NOT** dibaca sebagai `Employee Paid`.

### Corporate / Human Resource / Attendance Management / Attendance Payroll Handoff

Base URL: `api/v1/corporate/human-resource/attendance/payroll-handoff`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan serah terima | `AttendancePayrollHandoff : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/payroll-runs/options` | Pilihan putaran payroll yang tersedia | `AttendancePayrollHandoff : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/payroll-runs/{payrollRunId:guid}/summary` | Ringkasan data kehadiran yang akan diserahkan | `AttendancePayrollHandoff : Read` | — | `ApiResponse<HandoffSummaryResponse>` | |
| `GET` | `/payroll-runs/{payrollRunId:guid}/preview` | Pratinjau rinci sebelum serah terima dijalankan | `AttendancePayrollHandoff : Read` | Query | `ApiResponse<PagedResult<HandoffPreviewResponse>>` | |
| `GET` | `/payroll-runs/{payrollRunId:guid}/reconciliation` | Selisih antara data kehadiran dan snapshot payroll | `AttendancePayrollHandoff : Read` | Query | `ApiResponse<ReconciliationResponse>` | |
| `POST` | `/payroll-runs/{payrollRunId:guid}/execute` | Menjalankan serah terima kehadiran ke payroll | `AttendancePayrollHandoff : Execute` | `ExecuteHandoffRequest` | `ApiResponse<HandoffResultResponse>` | |
| `POST` | `/payroll-runs/{payrollRunId:guid}/repair` | Memperbaiki serah terima yang sebagian gagal | `AttendancePayrollHandoff : Repair` | `RepairHandoffRequest` | `ApiResponse<HandoffResultResponse>` | |
| `POST` | `/payroll-runs/{payrollRunId:guid}/rollback` | Membatalkan serah terima yang sudah dijalankan | `AttendancePayrollHandoff : Rollback` | `RollbackHandoffRequest` | `ApiResponse<HandoffResultResponse>` | |

**Batas yang mengikat grup ini** `[DECISION]` `HRD-DEC-009`: **rantai tanggung jawab HR berhenti
di sini.** Tidak ada satu pun endpoint HR yang mengubah status pembayaran. Bentuk data yang
diterima Finance dan perilaku bila Finance menolak batch adalah `[OPEN]` — `HRD-Q-10` dan
`HRD-Q-11` — dan **MUST NOT** dirancang sebelum keduanya dijawab.

**Idempotensi.** Menjalankan `execute` dua kali untuk data yang sama **tidak** menghasilkan dua
penyerahan. Endpoint memeriksa snapshot yang sudah ada lebih dulu `[EXISTING]`.

### Corporate / Human Resource / Attendance Management / Attendance Exception Classification

Base URL: `api/v1/corporate/human-resource/attendance/exception-classifications`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending` | Daftar pengecualian kerja di luar jadwal yang menunggu keputusan atasan | `AttendanceException : Read` | Query | `ApiResponse<PagedResult<ExceptionResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{exceptionId:guid}/classify` | Menetapkan klasifikasi akhir: lembur, koreksi jadwal, tercatat tanpa kompensasi, atau klasifikasi resmi lain | `AttendanceException : Classify` | `ClassifyExceptionRequest` | `ApiResponse<ExceptionResponse>` | **Rencana (belum tersedia)** |

`[DECISION]` `HRD-DEC-025` dan `HRD-DEC-013`: aktivitas kerja nyata di luar jadwal yang valid
memakai tipe pengecualian **baru dan terpisah**, bukan `ScheduleMismatch` yang bermakna *jadwal
tidak dapat diselesaikan*. Klasifikasinya **tidak pernah otomatis menjadi lembur** — atasan yang
memutuskan.

### Corporate / Human Resource / Attendance Management / Attendance Schedule Resolver

Base URL: `api/v1/corporate/human-resource/attendance/schedule-resolver`

Menyediakan pemeriksaan jadwal yang berlaku untuk satu pegawai pada satu tanggal, beserta
sumbernya. Hak akses `AttendanceScheduleResolver : Read`.

### Corporate / Human Resource / Attendance Management / Attendance Scheduler

Base URL: `api/v1/corporate/human-resource/attendance/scheduler-jobs`

Mengelola pekerjaan pemrosesan terjadwal. Hak akses `AttendanceScheduler : Read`, `: Enqueue`,
`: Cancel`, `: Retry`.

---

## 3. Cuti

### Corporate / Human Resource / Leave Management / Leave Balance

Base URL: `api/v1/corporate/human-resource/leave/balances`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan saldo | `LeaveBalance : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan saldo seluruh pegawai | `LeaveBalance : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar saldo cuti pegawai | `LeaveBalance : Read` | Query | `ApiResponse<PagedResult<BalanceResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu saldo | `LeaveBalance : Read` | — | `ApiResponse<BalanceDetailResponse>` | |
| `GET` | `/{id:guid}/ledger` | Buku besar: seluruh pergerakan saldo beserta sebabnya | `LeaveBalance : Read` | Query | `ApiResponse<PagedResult<LedgerResponse>>` | |
| `GET` | `/{id:guid}/entitlements` | Hak cuti yang menjadi dasar saldo | `LeaveBalance : Read` | — | `ApiResponse<List<EntitlementResponse>>` | |
| `GET` | `/{id:guid}/accruals` | Akrual yang sudah masuk ke saldo ini | `LeaveBalance : Read` | — | `ApiResponse<List<AccrualResponse>>` | |
| `GET` | `/{id:guid}/carry-forwards` | Sisa cuti yang dibawa dari periode sebelumnya | `LeaveBalance : Read` | — | `ApiResponse<List<CarryForwardResponse>>` | |
| `GET` | `/{id:guid}/adjustments` | Penyesuaian manual pada saldo ini | `LeaveBalance : Read` | — | `ApiResponse<List<AdjustmentResponse>>` | |
| `GET` | `/{id:guid}/reconciliation` | Perbandingan saldo tersimpan dengan hasil hitung buku besar | `LeaveBalance : Read` | — | `ApiResponse<ReconciliationResponse>` | |

**Mengapa grup ini hanya membaca.** Saldo **tidak pernah** diubah lewat endpoint saldo. Ia
berubah sebagai akibat dari akrual, sisa yang dibawa, pengajuan cuti, pembatalan, atau
penyesuaian yang punya jalurnya sendiri. Endpoint `reconciliation` adalah alat untuk membuktikan
bahwa saldo tersimpan masih sama dengan hasil hitung buku besar.

### Corporate / Human Resource / Leave Management / Leave Adjustment

Base URL: `api/v1/corporate/human-resource/leave/adjustments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan penyesuaian | `LeaveAdjustment : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/reasons/options` | Pilihan alasan penyesuaian yang sah | `LeaveAdjustment : Read` | — | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/summary` | Ringkasan penyesuaian | `LeaveAdjustment : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar penyesuaian | `LeaveAdjustment : Read` | Query | `ApiResponse<PagedResult<AdjustmentResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu penyesuaian | `LeaveAdjustment : Read` | — | `ApiResponse<AdjustmentDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan penyesuaian | `LeaveAdjustment : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/` | Membuat draft penyesuaian | `LeaveAdjustment : Create` | `CreateLeaveAdjustmentRequest` | `ApiResponse<AdjustmentDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah draft penyesuaian | `LeaveAdjustment : Update` | `UpdateLeaveAdjustmentRequest` | `ApiResponse<AdjustmentDetailResponse>` | |
| `POST` | `/{id:guid}/prepare-workflow` | Menyiapkan jalur persetujuan | `LeaveAdjustment : Update` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/submit` | Mengajukan penyesuaian untuk disetujui | `LeaveAdjustment : Submit` | `SubmitRequest` | `ApiResponse<AdjustmentDetailResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan penyesuaian | `LeaveAdjustment : Cancel` | `CancelRequest` | `ApiResponse<AdjustmentDetailResponse>` | |
| `POST` | `/{id:guid}/workflow/synchronize` | Menyelaraskan status dengan mesin persetujuan | `LeaveAdjustment : Update` | — | `ApiResponse<AdjustmentDetailResponse>` | |
| `POST` | `/{id:guid}/post` | Memasukkan penyesuaian ke buku besar saldo | `LeaveAdjustment : Post` | — | `ApiResponse<AdjustmentDetailResponse>` | |
| `POST` | `/{id:guid}/reverse` | Membalikkan penyesuaian yang sudah masuk | `LeaveAdjustment : Reverse` | `ReverseRequest` | `ApiResponse<AdjustmentDetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai penyesuaian terhapus | `LeaveAdjustment : Delete` | — | `ApiResponse<object>` | |

**Aturan yang mengikat.** Setiap penyesuaian **wajib** menyimpan alasan dan pelakunya. Saldo
tidak pernah berubah tanpa jejak. Apakah penyesuaian memerlukan persetujuan berjenjang adalah
`[OPEN]` — `HRD-Q-27`; jalur `submit` dan `prepare-workflow` sudah ada, tetapi kewajiban
memakainya belum diputuskan.

### Corporate / Human Resource / Leave Management / Leave Execution

Base URL: `api/v1/corporate/human-resource/leave/executions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan eksekusi | `LeaveExecution : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan eksekusi per keadaan | `LeaveExecution : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar eksekusi cuti | `LeaveExecution : Read` | Query | `ApiResponse<PagedResult<ExecutionResponse>>` | |
| `GET` | `/{leaveRequestId:guid}` | Detail eksekusi satu permohonan | `LeaveExecution : Read` | — | `ApiResponse<ExecutionDetailResponse>` | |
| `GET` | `/{leaveRequestId:guid}/reconciliation` | Selisih antara saldo, kehadiran, dan eksekusi | `LeaveExecution : Read` | — | `ApiResponse<ReconciliationResponse>` | |
| `POST` | `/process-due` | Menjalankan seluruh cuti yang jatuh tempo hari ini | `LeaveExecution : Execute` | `ProcessDueRequest` | `ApiResponse<BatchResultResponse>` | |
| `POST` | `/{leaveRequestId:guid}/execute` | Menjalankan satu cuti | `LeaveExecution : Execute` | — | `ApiResponse<ExecutionDetailResponse>` | |
| `POST` | `/{leaveRequestId:guid}/retry` | Mengulang eksekusi yang gagal | `LeaveExecution : Retry` | — | `ApiResponse<ExecutionDetailResponse>` | |
| `POST` | `/{leaveRequestId:guid}/reverse` | Membalikkan eksekusi cuti | `LeaveExecution : Reverse` | `ReverseExecutionRequest` | `ApiResponse<ExecutionDetailResponse>` | **Diperketat** — lihat catatan |
| `POST` | `/cancellations/{cancellationRequestId:guid}/apply` | Menerapkan pembatalan cuti yang sudah disetujui | `LeaveExecution : ApplyCancellation` | — | `ApiResponse<ExecutionDetailResponse>` | |

**Catatan pada `reverse`** `[DECISION]` `HRD-DEC-023`. Enam syarat yang **wajib** dipenuhi:
permission khusus, alasan wajib diisi, pelaku dan waktu tercatat, rekonsiliasi kehadiran
dijalankan, saldo dibalikkan atau dihitung ulang, dan periode payroll diperiksa lebih dulu.
**Bila payroll sudah terkunci, histori `Completed` MUST NOT dimutasi langsung** — pakai transaksi
penyesuaian terpisah. Keadaan hari ini **tidak** memenuhi keenamnya: endpoint tidak punya guard
status, tidak mewajibkan alasan, dan tidak memeriksa kunci payroll. Ini tercatat sebagai
**`IMPLEMENTATION DEFECT / REPAIR`**.

### Corporate / Human Resource / Leave Management / Leave Recall

Base URL: `api/v1/corporate/human-resource/leave/recalls`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemanggilan kembali | `LeaveRecall : Read` | Query | `ApiResponse<PagedResult<RecallResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu pemanggilan kembali | `LeaveRecall : Read` | — | `ApiResponse<RecallDetailResponse>` | |
| `POST` | `/` | Membuat pemanggilan kembali | `LeaveRecall : Create` | `CreateLeaveRecallRequest` | `ApiResponse<RecallDetailResponse>` | |
| `POST` | `/{id:guid}/prepare-workflow` | Menyiapkan jalur persetujuan | `LeaveRecall : Update` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/submit` | Mengajukan pemanggilan kembali | `LeaveRecall : Submit` | `SubmitRequest` | `ApiResponse<RecallDetailResponse>` | |
| `POST` | `/{id:guid}/synchronize` | Menyelaraskan status dengan mesin persetujuan | `LeaveRecall : Synchronize` | — | `ApiResponse<RecallDetailResponse>` | |
| `POST` | `/{id:guid}/apply` | Menerapkan pemanggilan kembali | `LeaveRecall : Apply` | — | `ApiResponse<RecallDetailResponse>` | |
| `POST` | `/{id:guid}/acknowledgement-override` | HR Manager menandai pemberitahuan sudah tersampaikan walau pegawai belum mengonfirmasi | `LeaveRecall : OverrideAcknowledgement` | `AcknowledgementOverrideRequest` | `ApiResponse<RecallDetailResponse>` | **Rencana (belum tersedia)** |

`[DECISION]` `HRD-DEC-024`: `Acknowledged` **bukan** prasyarat sebelum `Approved`. Persetujuan
pemanggilan kembali adalah keputusan organisasi, bukan keputusan pegawai. Override **wajib**
menyimpan alasan, pelaku, waktu, dan jejak audit. Pegawai **MUST NOT** dapat memblokir keputusan
organisasi selamanya hanya dengan tidak mengonfirmasi.

### Grup cuti lain yang sudah ada

| Grup `[Tags(...)]` | Base URL | Kegunaan | Hak akses utama |
| --- | --- | --- | --- |
| `Corporate / Human Resource / Leave Management / Entitlement Period` | `.../leave/entitlement-periods` | Periode hak cuti beserta saldo per periode | `LeaveEntitlementPeriod : Read` |
| `Corporate / Human Resource / Leave Management / Leave Accrual` | `.../leave/accrual-runs` | Proses penambahan hak cuti berkala | `LeaveAccrualRun : Read`, `: Create`, `: Execute`, `: Retry`, `: Cancel` |
| `Corporate / Human Resource / Leave Management / Leave Carry Forward` | `.../leave/carry-forward-runs` | Proses membawa sisa cuti ke periode berikutnya beserta kedaluwarsanya | `LeaveCarryForwardRun : Read`, `: Create`, `: Execute`, `: Reverse`, `: Expire` |
| `Corporate / Human Resource / Leave Management / Leave Calendar` | `.../leave/calendar` | Kalender cuti untuk administrasi dan untuk tim | `LeaveCalendar : Read`, `LeaveTeamCalendar : Read` |
| `Corporate / Human Resource / Leave Management / Leave Cancellation` | `.../leave/cancellations` | Administrasi pembatalan cuti | `LeaveCancellation : Read`, `: Synchronize`, `: Apply` |
| `Corporate / Human Resource / Leave Management / Leave Request Workflow` | `.../leave/request-workflow` | Keadaan persetujuan permohonan cuti dan pengulangan pemotongan saldo | `LeaveRequestWorkflow : Read`, `: Synchronize`, `: RetryBalance` |
| `Corporate / Human Resource / Leave Management / Final Reconciliation` | `.../leave/final-reconciliation` | Pemeriksaan akhir dan perbaikan satu permohonan | `LeaveFinalReconciliation : Read`, `: Repair` |
| `Corporate / Human Resource / Leave Management / Leave Payroll Integration` | `.../leave/payroll-integration` | Serah terima data cuti ke payroll | `LeavePayrollIntegration : Read`, `: Reconcile`, `: Execute`, `: Rollback` |

---

## 4. Lembur

Lembur berjalan **lima tahap terpisah**, dan kelimanya **MUST NOT** disatukan: rencana,
permohonan, realisasi, verifikasi, serah terima.

### Corporate / Human Resource / Overtime Management / Overtime Plan

Base URL: `api/v1/corporate/human-resource/overtime-management/plans`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar rencana lembur | `OvertimePlan : Read` | Query | `ApiResponse<PagedResult<PlanResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu rencana | `OvertimePlan : Read` | — | `ApiResponse<PlanDetailResponse>` | |
| `GET` | `/{id:guid}/details/{detailId:guid}` | Detail satu baris rencana | `OvertimePlan : Read` | — | `ApiResponse<PlanDetailItemResponse>` | |
| `POST` | `/` | Membuat rencana lembur | `OvertimePlan : Create` | `CreateOvertimePlanRequest` | `ApiResponse<PlanDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah rencana | `OvertimePlan : Update` | `UpdateOvertimePlanRequest` | `ApiResponse<PlanDetailResponse>` | |
| `PATCH` | `/{id:guid}/status` | Mengubah status rencana | `OvertimePlan : Update` | `UpdateStatusRequest` | `ApiResponse<PlanDetailResponse>` | |
| `POST` | `/{id:guid}/details` | Menambah baris pegawai pada rencana | `OvertimePlan : CreateDetail` | `CreatePlanDetailRequest` | `ApiResponse<PlanDetailItemResponse>` | |
| `PUT` | `/{id:guid}/details/{detailId:guid}` | Mengubah baris rencana | `OvertimePlan : UpdateDetail` | `UpdatePlanDetailRequest` | `ApiResponse<PlanDetailItemResponse>` | |
| `DELETE` | `/{id:guid}/details/{detailId:guid}` | Menghapus baris rencana | `OvertimePlan : DeleteDetail` | — | `ApiResponse<object>` | |
| `POST` | `/{id:guid}/details/validate-preview` | Memeriksa satu baris sebelum disimpan | `OvertimePlan : Validate` | `ValidateDetailRequest` | `ApiResponse<ValidationResponse>` | |
| `POST` | `/{id:guid}/validate` | Memeriksa seluruh rencana | `OvertimePlan : Validate` | — | `ApiResponse<ValidationResponse>` | |
| `POST` | `/{id:guid}/publish` | Menerbitkan rencana | `OvertimePlan : Publish` | — | `ApiResponse<PlanDetailResponse>` | |
| `POST` | `/{id:guid}/generate-requests` | Menurunkan rencana menjadi permohonan lembur per pegawai | `OvertimePlan : GenerateRequest` | `GenerateRequestsRequest` | `ApiResponse<BatchResultResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan rencana | `OvertimePlan : Cancel` | `CancelRequest` | `ApiResponse<PlanDetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai rencana terhapus | `OvertimePlan : Delete` | — | `ApiResponse<object>` | |

### Corporate / Human Resource / Overtime Management / Overtime Realization

Base URL: `api/v1/corporate/human-resource/overtime-management/realizations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar realisasi lembur | `OvertimeRealization : Read` | Query | `ApiResponse<PagedResult<RealizationResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu realisasi | `OvertimeRealization : Read` | — | `ApiResponse<RealizationDetailResponse>` | |
| `POST` | `/requests/{requestId:guid}/preview` | Pratinjau perhitungan sebelum disimpan | `OvertimeRealization : Preview` | `PreviewRequest` | `ApiResponse<RealizationPreviewResponse>` | |
| `POST` | `/requests/{requestId:guid}/calculate` | Menghitung realisasi dari kehadiran nyata | `OvertimeRealization : Calculate` | `CalculateRequest` | `ApiResponse<RealizationDetailResponse>` | |
| `POST` | `/requests/{requestId:guid}/recalculate` | Menghitung ulang setelah data kehadiran berubah | `OvertimeRealization : Recalculate` | `RecalculateRequest` | `ApiResponse<RealizationDetailResponse>` | |
| `POST` | `/{id:guid}/submit-verification` | Mengajukan realisasi untuk diverifikasi | `OvertimeRealization : SubmitVerification` | `SubmitRequest` | `ApiResponse<RealizationDetailResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan realisasi | `OvertimeRealization : Cancel` | `CancelRequest` | `ApiResponse<RealizationDetailResponse>` | |

**Aturan nominal.** Nominal lembur **selalu** dihitung backend memakai tarif dari master.
Frontend **MUST NOT** menghitung tarif sendiri.

### Grup lembur lain yang sudah ada

| Grup `[Tags(...)]` | Base URL | Kegunaan | Hak akses utama |
| --- | --- | --- | --- |
| `Corporate / Human Resource / Overtime Management / Overtime Workflow` | `.../overtime-management/requests` | Mengajukan dan menyelaraskan persetujuan permohonan lembur | `OvertimeWorkflow : Submit`, `: Synchronize` |
| `Corporate / Human Resource / Overtime Management / Overtime Verification` | `.../overtime-management/verifications` | Verifikasi berjenjang atas realisasi | `OvertimeVerification : Read`, `: Start`, `: Approve`, `: NeedRevision`, `: Reject` |
| `Corporate / Human Resource / Overtime Management / Period Closing` | `.../overtime-management/periods` | Periode lembur beserta tutup dan buka kembali | `OvertimePeriod : Read`, `: Create`, `: Validate`, `: Close`, `: Reopen`, `: Cancel` |
| `Corporate / Human Resource / Overtime Management / Payroll Handoff` | `.../overtime-management/payroll-handoffs` | Serah terima realisasi lembur ke payroll | `OvertimePayrollHandoff : Read`, `: Preview`, `: Post`, `: Reconcile`, `: Rollback` |
| `Corporate / Human Resource / Overtime Management / Compensatory Leave` | `.../overtime-management/compensatory-leaves` | Cuti pengganti dari lembur | `OvertimeCompensatoryLeave : Read`, `: Preview`, `: Post`, `: Reverse`, `: Reconcile` |
| `Corporate / Human Resource / Overtime Management / Final Reconciliation` | `.../overtime-management/reconciliation` | Pemeriksaan akhir lintas lembur | `OvertimeReconciliation : Reconcile` |
| `Corporate / Human Resource / Overtime Management / Scheduler` | `.../overtime-management/scheduler-jobs` | Pekerjaan terjadwal penutupan lembur | `OvertimeScheduler : Read`, `: Enqueue`, `: Cancel`, `: Retry` |

**Guard yang mengikat serah terima lembur.** Posting ditolak kecuali realisasi berstatus
`Verified` **dan** verifikasi aktif terbaru berstatus `Approved` `[EXISTING]`. Koreksi setelah
posting **tidak** selalu memerlukan pembukaan periode penuh — tersedia `rollback` per realisasi
yang lebih sempit cakupannya.

---

## 5. Penjadwalan Kerja

### Corporate / Human Resource / Scheduling Management / Work Schedule Assignment

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/work-schedule-assignments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan penempatan jadwal | **Tidak ada** — lihat catatan | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan penempatan | **Tidak ada** | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar penempatan jadwal pegawai ini | **Tidak ada** | Query | `ApiResponse<PagedResult<AssignmentResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu penempatan | **Tidak ada** | — | `ApiResponse<AssignmentDetailResponse>` | |
| `POST` | `/` | Menempatkan jadwal kerja | **Tidak ada** | `CreateWorkScheduleAssignmentRequest` | `ApiResponse<AssignmentDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah penempatan | **Tidak ada** | `UpdateWorkScheduleAssignmentRequest` | `ApiResponse<AssignmentDetailResponse>` | |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan atau menonaktifkan penempatan | **Tidak ada** | `UpdateStatusRequest` | `ApiResponse<AssignmentDetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai penempatan terhapus | **Tidak ada** | — | `ApiResponse<object>` | |

**Temuan yang harus dicatat, bukan diperbaiki dari alur blueprint.** Kedelapan endpoint di atas
**tidak memiliki `[AccessPermission]`** pada action-nya. Controller tetap memiliki `[Authorize]`,
sehingga tidak terbuka tanpa autentikasi, tetapi tidak dijaga hak akses per aksi seperti 148
controller HR lainnya. Butir hak akses target: `WorkScheduleAssignment : Read`, `: Create`,
`: Update`, `: Delete` — **Rencana (belum tersedia)**.

**Guard target yang belum ada** `[DECISION]` `HRD-DEC-027`: penempatan jadwal saat ini dan yang
akan datang oleh HR berwenang pada periode yang masih dapat disunting **tidak** memerlukan
persetujuan tambahan; audit trail tetap wajib. Perubahan yang berlaku surut, atau yang menyentuh
periode kehadiran maupun payroll yang sudah diproses atau terkunci, **wajib** lewat koreksi
terkendali. Guard itu **belum ada** — `MISSING`.

### Corporate / Human Resource / Scheduling Management / Schedule Change Administration

Base URL: `api/v1/corporate/human-resource/scheduling-management/schedule-change-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permohonan ubah jadwal seluruh pegawai | `ScheduleChangeRequest : Read` | Query | `ApiResponse<PagedResult<ScheduleChangeResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu permohonan | `ScheduleChangeRequest : Read` | — | `ApiResponse<ScheduleChangeDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan | `ScheduleChangeRequest : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/apply` | Menerapkan perubahan jadwal yang sudah disetujui | `ScheduleChangeRequest : Update` | — | `ApiResponse<ScheduleChangeDetailResponse>` | |
| `POST` | `/{id:guid}/workflow/synchronize` | Menyelaraskan status dengan mesin persetujuan | `ScheduleChangeRequest : Update` | — | `ApiResponse<ScheduleChangeDetailResponse>` | |

### Corporate / Human Resource / Scheduling Management / Shift Swap Administration

Base URL: `api/v1/corporate/human-resource/scheduling-management/shift-swap-requests`

Bentuknya sejajar dengan ubah jadwal, dengan hak akses `ShiftSwapRequest : Read` dan `: Update`.

**Perbedaan yang tidak boleh disamakan.** Tukar shift memakai `WorkflowDefinitionCode` yang
**berbeda** dari ubah jadwal, dan berjalan **dua tahap terpisah**: persetujuan rekan lebih dulu,
baru persetujuan atasan `[EXISTING]`.

### Corporate / Human Resource / Scheduling Management / Roster Period

Base URL: `api/v1/corporate/human-resource/scheduling-management/roster-periods`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan roster | `RosterPeriod : Read` | — | `ApiResponse<FilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan roster per keadaan | `RosterPeriod : Read` | — | `ApiResponse<SummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar periode roster | `RosterPeriod : Read` | Query | `ApiResponse<PagedResult<RosterPeriodResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Detail satu periode roster | `RosterPeriod : Read` | — | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat periode roster | `RosterPeriod : Create` | `CreateRosterPeriodRequest` | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Mengubah periode roster | `RosterPeriod : Update` | `UpdateRosterPeriodRequest` | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/validate` | Memeriksa kecukupan tenaga dan bentrok jadwal | `RosterPeriod : Validate` | — | `ApiResponse<ValidationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan roster untuk disetujui | `RosterPeriod : Submit` | `SubmitRequest` | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/publish` | Menerbitkan roster sehingga menjadi jadwal yang berlaku | `RosterPeriod : Publish` | `PublishRequest` | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/lock` | Mengunci roster agar tidak berubah lagi | `RosterPeriod : Lock` | — | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/close` | Menutup periode roster | `RosterPeriod : Close` | — | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/cancel` | Membatalkan periode roster | `RosterPeriod : Cancel` | `CancelRequest` | `ApiResponse<RosterPeriodDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}/assignments` | Kisi penugasan per pegawai per tanggal | `RosterAssignment : Read` | Query | `ApiResponse<RosterGridResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/assignments` | Menugaskan pegawai pada roster | `RosterAssignment : Create` | `CreateRosterAssignmentRequest` | `ApiResponse<RosterAssignmentResponse>` | **Rencana (belum tersedia)** |

### Grup penjadwalan lain yang direncanakan

`[DECISION]` `HRD-DEC-026` — keenam grup di bawah adalah target `EXTEND` terhadap skema yang
sudah ada, **bukan** `DEFERRED`. Model, konfigurasi EF, dan tabelnya sudah ada; yang belum ada
adalah perilakunya.

| Grup `[Tags(...)]` | Base URL | Kegunaan | Hak akses utama | Status |
| --- | --- | --- | --- | --- |
| `.../Scheduling Management / Shift Assignment` | `.../scheduling-management/shift-assignments` | Penugasan shift harian yang dibaca pemroses kehadiran | `ShiftAssignment : Read`, `: Create`, `: Update`, `: Cancel` | **Rencana (belum tersedia)** |
| `.../Scheduling Management / Shift Replacement` | `.../scheduling-management/shift-replacements` | Penggantian shift saat pegawai berhalangan | `ShiftReplacement : Read`, `: Create`, `: Approve`, `: Cancel` | **Rencana (belum tersedia)** |
| `.../Scheduling Management / Emergency Staffing` | `.../scheduling-management/emergency-staffing-requests` | Permintaan tenaga darurat | `EmergencyStaffing : Read`, `: Create`, `: Fulfill`, `: Cancel` | **Rencana (belum tersedia)** |
| `.../Scheduling Management / On Call Assignment` | `.../scheduling-management/on-call-assignments` | Penugasan siaga aktual, terpisah dari master jenis siaga | `OnCallAssignment : Read`, `: Create`, `: Confirm`, `: Activate`, `: Cancel` | **Rencana (belum tersedia)** |
| `.../Scheduling Management / Roster Publication` | bagian dari `roster-periods` | Riwayat penerbitan roster | `RosterPeriod : Read` | **Rencana (belum tersedia)** |
| `.../Scheduling Management / Roster Approval` | bagian dari `roster-periods` | Riwayat persetujuan roster | `RosterPeriod : Read` | **Rencana (belum tersedia)** |

---

## 6. Persetujuan Bersama

### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan kotak masuk | `ApprovalInbox : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Jumlah menunggu, terdelegasi, dan lewat batas waktu | `ApprovalInbox : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar pengajuan yang menunggu persetujuan saya, lintas jenis transaksi | `ApprovalInbox : Read` | Query `view=open\|completed\|all` | `ApiResponse<PagedResult<InboxItemResponse>>` | |
| `GET` | `/delegated-to-me` | Tugas yang dialihkan orang lain kepada saya | `ApprovalInbox : Read` | Query | `ApiResponse<PagedResult<InboxItemResponse>>` | |
| `GET` | `/{assignmentId:guid}` | Detail satu tugas persetujuan | `ApprovalInbox : Read` | — | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/approve` | Menyetujui | `ApprovalInbox : Approve` | `ApproveRequest` | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/reject` | Menolak, dengan alasan wajib | `ApprovalInbox : Reject` | `RejectRequest` | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/request-revision` | Meminta pemohon memperbaiki | `ApprovalInbox : RequestRevision` | `RequestRevisionRequest` | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/return` | Mengembalikan ke langkah sebelumnya | `ApprovalInbox : Return` | `ReturnRequest` | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/verify` | Memverifikasi, untuk langkah bertipe verifikasi | `ApprovalInbox : Verify` | `VerifyRequest` | `ApiResponse<InboxItemDetailResponse>` | |
| `POST` | `/{assignmentId:guid}/acknowledge` | Mengakui, untuk langkah bertipe pengakuan | `ApprovalInbox : Acknowledge` | `AcknowledgeRequest` | `ApiResponse<InboxItemDetailResponse>` | |

**Gate kewenangan yang benar-benar berlaku.** Setiap aksi memeriksa
`assignment.AssignedApproverUserId == actorContext.UserId` `[EXISTING]`. Memiliki
`ApprovalInbox : Approve` **tidak** membuat seseorang dapat menyetujui pengajuan yang tidak
ditugaskan kepadanya.

**Pagar `HRD-DEC-018`.** Kotak masuk **hanya** menyeragamkan bentuk baris ringkasan, cara
memfilter dan mengurutkan, penanda status, dan cara berpindah ke detail. Workflow, policy,
permission, validasi, batas waktu, dan eskalasi **tetap milik masing-masing jenis transaksi**.

### Corporate / Human Resource / Workflow Management / Workflow Instance

Base URL: `api/v1/corporate/human-resource/workflow-instances`

Menyediakan pembuatan, pengajuan, dan keputusan per assignment, ditambah pembatalan dan
penarikan. Hak akses `WorkflowInstance : Read`, `: Create`, `: Submit`, `: Approve`, `: Reject`,
`: RequestRevision`, `: Return`, `: Verify`, `: Acknowledge`, `: Cancel`, `: Withdraw`.

### Corporate / Human Resource / Workflow Management / Approval Delegation

Base URL: `api/v1/corporate/human-resource/approval-delegations`

Mengelola pelimpahan wewenang persetujuan. Hak akses `ApprovalDelegation : Read`, `: Create`,
`: Update`, `: Submit`, `: Approve`, `: Reject`, `: Activate`, `: Revoke`, `: Cancel`,
`: Delete`.

**Guard yang penting.** Delegasi **tidak dapat** disetujui oleh delegator maupun penerima
delegasi itu sendiri `[EXISTING]`. Ini pola pemisahan peran yang benar, dan **layak ditiru**
domain lain — bandingkan dengan tindakan disiplin yang justru memperbolehkan swa-setuju
(`HRD-Q-51`).

### Corporate / Human Resource / Workflow Management / Workflow Comment dan Workflow Attachment

Base URL keduanya: `api/v1/corporate/human-resource/workflow-instances`

Komentar dan lampiran pada satu instance persetujuan. Hak akses `WorkflowComment : Read`,
`: Create`, `: Update`, `: Delete`; `WorkflowAttachment : Read`, `: Create`, `: Delete`.

### Endpoint mesin pengingat dan eskalasi

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `api/v1/corporate/human-resource/workflow-reminders/pending` | Melihat tugas yang mendekati atau melewati batas waktu | `WorkflowReminder : Read` | Query | `ApiResponse<PagedResult<ReminderResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `api/v1/corporate/human-resource/workflow-reminders/run` | Menjalankan pemrosesan pengingat dan eskalasi secara manual | `WorkflowReminder : Run` | `RunReminderRequest` | `ApiResponse<BatchResultResponse>` | **Rencana (belum tersedia)** |

`[DECISION]` `HRD-DEC-030`: `DueAt`, `ReminderAfterHours`, dan `EscalationAfterHours` **harus
benar-benar dieksekusi** oleh pemrosesan terjadwal, bukan sekadar tersimpan sebagai konfigurasi
seperti hari ini. `AutoApproveAfterHours` dan `AutoRejectAfterHours` **default mati**, hanya
aktif bila definisi workflow transaksi itu secara eksplisit mengizinkan — **dilarang** berlaku
otomatis ke seluruh transaksi HR.

---

## 7. Profil dan Administrasi Workforce

### Corporate / Human Resource / Workforce Core / *(pola per profil)*

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/<sumber-daya>`

Empat belas controller mengikuti satu pola yang seragam, dengan `<sumber-daya>` berupa:
`addresses`, `bank-accounts`, `contract-histories`, `dependents`, `documents`, `educations`,
`emergency-contacts`, `employment-histories`, `family-members`, `manager-assignments`,
`organization-assignments`, `position-assignments`, `salary-assignments`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan | `<Wfp Resource> : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/summary` | Ringkasan | `<Wfp Resource> : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar milik satu pegawai | `<Wfp Resource> : Read` | Query | `ApiResponse<PagedResult<Response>>` | |
| `GET` | `/{id:guid}` | Detail satu baris | `<Wfp Resource> : Read` | — | `ApiResponse<DetailResponse>` | |
| `POST` | `/` | Menambah baris baru | `<Wfp Resource> : Create` | `Create<Entity>Request` | `ApiResponse<DetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah satu baris | `<Wfp Resource> : Update` | `Update<Entity>Request` | `ApiResponse<DetailResponse>` | |
| `PATCH` | `/{id:guid}/status` | Mengubah status aktif | `<Wfp Resource> : Update` | `UpdateStatusRequest` | `ApiResponse<DetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai terhapus | `<Wfp Resource> : Delete` | — | `ApiResponse<object>` | |

`WfpSalaryAssignment` menambah dua endpoint khas: `PATCH /{id:guid}/approval` dengan hak akses
`WfpSalaryAssignment : Update`, dan `PATCH /{id:guid}/primary` untuk menandai penetapan utama.

#### Perubahan yang dituntut `HRD-DEC-031`

Endpoint persetujuan yang ada hari ini **tidak memenuhi** `HRD-DEC-031` karena dua sebab: ia
memakai butir hak akses yang **sama** dengan buat dan ubah, sehingga pembuat dapat menyetujui
transaksinya sendiri; dan tidak ada pemeriksaan status persetujuan sebelum penempatan berlaku.

Bentuk target bagi keempat entity penempatan dan remunerasi — `WfpSalaryAssignment`,
`WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`:

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id:guid}/submit` | Mengajukan perubahan untuk disetujui | `<Wfp Resource> : Update` | `SubmitAssignmentRequest` | `ApiResponse<DetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui perubahan | `<Wfp Resource> : Approve` | `ApproveAssignmentRequest` | `ApiResponse<DetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak perubahan beserta alasannya | `<Wfp Resource> : Approve` | `RejectAssignmentRequest` | `ApiResponse<DetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/request-revision` | Meminta pembuat memperbaiki | `<Wfp Resource> : Approve` | `RequestRevisionRequest` | `ApiResponse<DetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}/amount` | Membaca nominal gaji satu penetapan | `WfpSalaryAssignment : ViewAmount` | — | `ApiResponse<SalaryAmountResponse>` | **Rencana (belum tersedia)** — hanya pada `WfpSalaryAssignment` |

**Butir `: Approve` adalah inti keputusan ini.** Selama menyetujui dan mengubah masih berbagi satu
butir `: Update`, pemisahan peran **tidak dapat** ditegakkan hanya dengan konfigurasi peran.

**Kode status khas grup ini.** `403` pada `approve` berarti penyetuju adalah pembuat transaksi itu
sendiri — dan **tidak ada pengecualian**, termasuk ketika unit hanya punya satu petugas. `409`
pada pemberlakuan penempatan berarti perubahan belum disetujui.

Endpoint `PATCH /{id:guid}/approval` yang ada sekarang tetap hidup sebagai alias selama masa
peralihan, mengikuti pola `HRD-DEC-016`, dan **MUST** ikut menegakkan pemisahan peran begitu
penjaganya dibangun.

### Daftar lintas-pegawai — `S-A1`

`[DECISION]` `HRD-DEC-012`: enam menu `Administrasi Kepegawaian` mendapat halaman daftar yang
menampilkan data **seluruh pegawai** pada satu periode, bukan data satu orang.

Base URL: `api/v1/corporate/human-resource/workforce-core`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/organization-assignments` | Seluruh penempatan organisasi yang berlaku pada satu periode | `WfpOrganizationAssignment : ReadAll` | Query | `ApiResponse<PagedResult<CrossEmployeeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/position-assignments` | Seluruh penempatan jabatan | `WfpPositionAssignment : ReadAll` | Query | `ApiResponse<PagedResult<CrossEmployeeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/manager-assignments` | Seluruh relasi atasan | `WfpManagerAssignment : ReadAll` | Query | `ApiResponse<PagedResult<CrossEmployeeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/employment-histories` | Seluruh riwayat kepegawaian | `WfpEmploymentHistory : ReadAll` | Query | `ApiResponse<PagedResult<CrossEmployeeResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/salary-assignments` | Seluruh penetapan gaji yang berlaku pada satu periode | `WfpSalaryAssignment : ReadAll` | Query | `ApiResponse<PagedResult<CrossEmployeeResponse>>` | **Rencana (belum tersedia)** |

**Pembatasan yang berlaku pada `salary-assignments`** `[DECISION]` `HRD-DEC-033`. Response
**MUST NOT** memuat nominal gaji. Ia memuat pegawai, unit, kelas gaji, tanggal berlaku, dan
status — cukup untuk pekerjaan administratif, tanpa membuka nominal banyak orang sekaligus.

Nominal dibaca terpisah lewat `GET /{id:guid}/amount` dengan butir sensitif
`WfpSalaryAssignment : ViewAmount`. Penyembunyian **MUST** dilakukan dengan **tidak menyertakan**
nilainya pada response, bukan dengan menyembunyikannya di layar — nilai yang tetap terkirim tetap
terbaca siapa pun yang membuka alat pengembang peramban. Keterlihatan massal
(`WfpSalaryAssignment : ViewAmountBulk`) **tidak diberikan pada MVP** dan memerlukan co-sign
keamanan.

Daftar lintas-pegawai untuk perubahan data pegawai **sudah ada**, yaitu
`api/v1/corporate/human-resource/employee-profile-changes`, dan menjadi contoh bentuk bagi kelima
endpoint di atas.

### Corporate / Human Resource / Workforce Core / Employee Profile Change Administration

Base URL: `api/v1/corporate/human-resource/employee-profile-changes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permohonan perubahan data seluruh pegawai | `EmployeeProfileChange : Read` | Query | `ApiResponse<PagedResult<ProfileChangeResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu permohonan | `EmployeeProfileChange : Read` | — | `ApiResponse<ProfileChangeDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan | `EmployeeProfileChange : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/start-verification` | Memulai verifikasi | `EmployeeProfileChange : Update` | `StartVerificationRequest` | `ApiResponse<ProfileChangeDetailResponse>` | |
| `POST` | `/{id:guid}/verifications/{verificationId:guid}/decision` | Memutuskan hasil satu verifikasi | `EmployeeProfileChange : Update` | `VerificationDecisionRequest` | `ApiResponse<VerificationResponse>` | |
| `GET` | `/{id:guid}/verifications/{verificationId:guid}/evidence` | Mengunduh bukti verifikasi | `EmployeeProfileChange : Read` | — | Berkas | |
| `POST` | `/{id:guid}/approve` | Menyetujui permohonan | `EmployeeProfileChange : Update` | `ApproveRequest` | `ApiResponse<ProfileChangeDetailResponse>` | |
| `POST` | `/{id:guid}/reject` | Menolak permohonan | `EmployeeProfileChange : Update` | `RejectRequest` | `ApiResponse<ProfileChangeDetailResponse>` | |
| `POST` | `/{id:guid}/request-revision` | Meminta pemohon memperbaiki | `EmployeeProfileChange : Update` | `RequestRevisionRequest` | `ApiResponse<ProfileChangeDetailResponse>` | |
| `POST` | `/{id:guid}/apply` | Menerapkan perubahan ke profil pegawai | `EmployeeProfileChange : Update` | — | `ApiResponse<ProfileChangeDetailResponse>` | |
| `POST` | `/{id:guid}/workflow/synchronize` | Menyelaraskan status dengan mesin persetujuan | `EmployeeProfileChange : Update` | — | `ApiResponse<ProfileChangeDetailResponse>` | |

### Corporate / Human Resource / Workforce Profile Management / Workforce Profile Overview

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/overview`

Satu endpoint `GET /` yang mengembalikan ringkasan seluruh berkas kepegawaian seorang pegawai.
Hak akses `WorkforceProfileOverview : Read`.

---

## 8. Layanan Mandiri Pegawai

**Aturan kepemilikan yang mengikat seluruh grup ini.** Identitas pegawai diturunkan dari pengguna
yang **terautentikasi** lewat `HumanResourceContextService`, **bukan** dari identifier yang
dikirim pemanggil. Ini yang mencegah pegawai membaca data pegawai lain dengan menukar `id` di
URL.

### Self Services / Human Resource / Context

Base URL: `api/v1/self-services/human-resource/context`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Konteks pegawai, organisasi, atasan, dan peran dari pengguna yang sedang masuk | **Tidak ada** — cukup terautentikasi | — | `ApiResponse<HumanResourceUserContextDto>` | |

### Self Services / Human Resource / Leave Request

Base URL: `api/v1/self-services/human-resource/leave/requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan pengajuan saya | `MyLeaveRequest : Read` | — | `ApiResponse<FilterMetadataResponse>` | |
| `GET` | `/balances/options` | Saldo cuti saya per jenis, untuk pilihan pada formulir | `MyLeaveRequest : Read` | — | `ApiResponse<List<BalanceOptionResponse>>` | |
| `GET` | `/reasons/options` | Pilihan alasan pengajuan | `MyLeaveRequest : Read` | — | `ApiResponse<List<OptionResponse>>` | |
| `POST` | `/calculate` | Menghitung berapa hari yang akan dipotong dari saldo | `MyLeaveRequest : Read` | `CalculateLeaveRequest` | `ApiResponse<CalculationResponse>` | |
| `GET` | `/summary` | Ringkasan pengajuan saya | `MyLeaveRequest : Read` | — | `ApiResponse<SummaryResponse>` | |
| `GET` | `/` | Daftar pengajuan cuti saya | `MyLeaveRequest : Read` | Query | `ApiResponse<PagedResult<LeaveRequestResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu pengajuan | `MyLeaveRequest : Read` | — | `ApiResponse<LeaveRequestDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan pengajuan saya | `MyLeaveRequest : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/` | Membuat draft pengajuan cuti | `MyLeaveRequest : Create` | `CreateLeaveRequestRequest` | `ApiResponse<LeaveRequestDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah draft | `MyLeaveRequest : Update` | `UpdateLeaveRequestRequest` | `ApiResponse<LeaveRequestDetailResponse>` | |
| `POST` | `/{id:guid}/prepare-workflow` | Menyiapkan jalur persetujuan | `MyLeaveRequest : Update` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/submit` | Mengajukan cuti | `MyLeaveRequest : Submit` | `SubmitRequest` | `ApiResponse<LeaveRequestDetailResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan pengajuan | `MyLeaveRequest : Cancel` | `CancelRequest` | `ApiResponse<LeaveRequestDetailResponse>` | |
| `POST` | `/{id:guid}/attachments` | Mengunggah lampiran pendukung | `MyLeaveRequest : Update` | `multipart/form-data` | `ApiResponse<AttachmentResponse>` | |
| `GET` | `/{id:guid}/attachments/{attachmentId:guid}/download` | Mengunduh lampiran | `MyLeaveRequest : Read` | — | Berkas | |
| `DELETE` | `/{id:guid}/attachments/{attachmentId:guid}` | Menghapus lampiran | `MyLeaveRequest : Update` | — | `ApiResponse<object>` | |
| `DELETE` | `/{id:guid}` | Menandai draft terhapus | `MyLeaveRequest : Delete` | — | `ApiResponse<object>` | |

**Aturan yang mengikat `calculate`.** Angka hari yang dipotong **selalu** berasal dari endpoint
ini. Frontend **MUST NOT** menghitung sendiri. Untuk cuti per jam, rumus yang berlaku adalah
jumlah menit yang diminta dibagi menit kerja terjadwal hari itu, dibulatkan empat angka di
belakang koma `[EXISTING]`.

**Contoh berangka.** Seorang pegawai dengan jadwal kerja 480 menit sehari mengajukan cuti per
jam selama 120 menit. Perhitungannya 120 ÷ 480 = 0,25 hari. Angka itulah yang dipotong dari
saldo. Bila jadwal hari itu ternyata 420 menit, hasilnya 120 ÷ 420 = 0,2857 hari — berbeda,
dan itu memang benar, karena porsi hari kerjanya berbeda.

**Catatan teknis yang perlu diketahui pemilik teknis.** Bila jadwal hari itu **tidak dapat
diselesaikan**, sistem hari ini memakai angka bawaan 480 menit yang **tertulis di dalam kode**,
bukan diambil dari master mana pun. Apakah angka itu boleh dipakai, atau perhitungan seharusnya
berhenti sampai jadwal yang sah tersedia, adalah `[OPEN]` — `HRD-Q-48`.

### Self Services / Human Resource / Overtime

Base URL: `api/v1/self-services/human-resource/overtime`

Bentuknya sejajar dengan pengajuan cuti: `validate-preview`, draft, submit, cancel. Hak akses
`MyOvertime : Read`, `: Validate`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete`.

**Pemeriksaan yang berlaku.** Pengajuan yang rentang waktunya bertumpuk dengan pengajuan lain
ditolak dengan kode `409` dan penanda `REQUEST_OVERLAP` `[EXISTING]`.

### Self Services / Human Resource / Shift Swap

Base URL: `api/v1/self-services/human-resource/shift-swap-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/target-options` | Rekan yang memenuhi syarat untuk ditukar | `MyShiftSwap : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/assignment-options` | Shift saya yang dapat ditukar | `MyShiftSwap : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `GET` | `/target-assignment-options` | Shift rekan yang dapat ditukar | `MyShiftSwap : Read` | Query | `ApiResponse<List<OptionResponse>>` | |
| `POST` | `/validate-preview` | Memeriksa apakah pertukaran melanggar aturan | `MyShiftSwap : Read` | `ValidateSwapRequest` | `ApiResponse<ValidationResponse>` | |
| `POST` | `/` | Membuat draft permohonan tukar shift | `MyShiftSwap : Create` | `CreateShiftSwapRequest` | `ApiResponse<ShiftSwapDetailResponse>` | |
| `POST` | `/{id:guid}/submit-to-target` | Mengirim permohonan kepada rekan yang dituju | `MyShiftSwap : Submit` | `SubmitToTargetRequest` | `ApiResponse<ShiftSwapDetailResponse>` | |
| `POST` | `/{id:guid}/target-response` | Rekan menerima atau menolak | `MyShiftSwap : Respond` | `TargetResponseRequest` | `ApiResponse<ShiftSwapDetailResponse>` | |
| `POST` | `/{id:guid}/submit-approval` | Meneruskan ke persetujuan atasan | `MyShiftSwap : Submit` | `SubmitApprovalRequest` | `ApiResponse<ShiftSwapDetailResponse>` | |
| `POST` | `/{id:guid}/cancel` | Membatalkan permohonan | `MyShiftSwap : Cancel` | `CancelRequest` | `ApiResponse<ShiftSwapDetailResponse>` | |

**Guard dua tahap yang tidak dapat dilewati.** `submit-approval` ditolak dengan `409` bila rekan
yang dituju **belum** menerima `[EXISTING]`. `TargetRejected` adalah keadaan akhir yang tidak
pernah mencapai persetujuan atasan.

### Grup layanan mandiri lain yang sudah ada

| Grup `[Tags(...)]` | Base URL | Kegunaan | Hak akses utama |
| --- | --- | --- | --- |
| `Self Services / Human Resource / Attendance` | `.../attendance` | Mencatat kehadiran masuk dan pulang, melihat riwayat | **Tidak ada `[AccessPermission]`** — temuan, lihat bagian 10 |
| `Self Services / Human Resource / Attendance Correction` | `.../attendance-corrections` | Mengajukan koreksi kehadiran beserta bukti | `MyAttendanceCorrection : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete` |
| `Self Services / Human Resource / Leave Balance` | `.../leave/balances` | Melihat saldo cuti saya beserta buku besarnya | `MyLeaveBalance : Read` |
| `Self Services / Human Resource / Leave Calendar` | `.../leave/calendar` | Melihat kalender cuti unit saya | `MyLeaveCalendar : Read` |
| `Self Services / Human Resource / Leave Cancellation` | `.../leave/cancellations` | Mengajukan pembatalan cuti | `MyLeaveCancellation : Read`, `: Create`, `: Update`, `: Submit` |
| `Self Services / Human Resource / Return To Work` | `.../leave/return-to-work` | Mengonfirmasi pemanggilan kembali | `MyReturnToWork : Read`, `: Acknowledge` |
| `Self Services / Human Resource / Schedule Change` | `.../schedule-change-requests` | Mengajukan perubahan jadwal | `MyScheduleChange : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete` |
| `Self Services / Human Resource / Profile Change` | `.../profile-changes` | Mengajukan perubahan data pribadi | `MyProfileChange : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete` |
| `Self Services / Human Resource / Resignation` | `.../resignation-requests` | Mengajukan pengunduran diri | `MyResignation : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete` |

---

## 9. Lifecycle dan Pengembangan Orang

### Corporate / Human Resource / Lifecycle Management / Resignation Administration

Base URL: `api/v1/corporate/human-resource/lifecycle-management/resignation-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pengunduran diri | `ResignationRequest : Read` | Query | `ApiResponse<PagedResult<ResignationResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu pengunduran diri | `ResignationRequest : Read` | — | `ApiResponse<ResignationDetailResponse>` | |
| `GET` | `/{id:guid}/workflow` | Keadaan persetujuan | `ResignationRequest : Read` | — | `ApiResponse<WorkflowStatusResponse>` | |
| `POST` | `/{id:guid}/workflow/synchronize` | Menyelaraskan status dengan mesin persetujuan | `ResignationRequest : Update` | — | `ApiResponse<ResignationDetailResponse>` | |
| `POST` | `/{id:guid}/handoff` | Menjalankan serah terima offboarding | `ResignationRequest : Update` | `HandoffRequest` | `ApiResponse<ResignationDetailResponse>` | |

**Batas yang harus diketahui.** `handoff` membuat daftar periksa offboarding **satu kali**.
Setelah itu **tidak ada kode yang memutakhirkan status tugas maupun daftar periksanya**
`[EXISTING]`. Pencabutan akun aplikasi **tidak otomatis** — kontraknya ke Identity `[OPEN]`,
`HRD-DEP-003`.

### Endpoint offboarding yang direncanakan

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/offboarding-checklists` | Daftar periksa offboarding seluruh pegawai yang keluar | `OffboardingChecklist : Read` | Query | `ApiResponse<PagedResult<ChecklistResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `.../offboarding-checklists/{id:guid}` | Detail satu daftar periksa beserta tugasnya | `OffboardingChecklist : Read` | — | `ApiResponse<ChecklistDetailResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `.../offboarding-checklists/{id:guid}/tasks/{taskId:guid}/status` | Menandai satu tugas selesai atau tidak berlaku | `OffboardingChecklist : Update` | `UpdateTaskStatusRequest` | `ApiResponse<TaskResponse>` | **Rencana (belum tersedia)** |
| `POST` | `.../offboarding-checklists/{id:guid}/close` | Menutup daftar periksa | `OffboardingChecklist : Close` | `CloseChecklistRequest` | `ApiResponse<ChecklistDetailResponse>` | **Rencana (belum tersedia)** |

### Corporate / Human Resource / Learning and Development / Training Record

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/training-records`

Mengikuti pola sembilan endpoint, ditambah `PATCH /{id:guid}/verify` untuk menandai rekaman sudah
diverifikasi. Hak akses `WorkforceTrainingRecord : Read`, `: Create`, `: Update`, `: Delete`.

**Batas yang harus diketahui.** Ini **pencatatan pasca-kejadian**, bukan siklus pendaftaran
sampai kelulusan. Sebelas entity rencana pelatihan formal — sesi, peserta, penilaian, sertifikat,
anggaran — **tidak punya controller sama sekali** `[EXISTING]`.

### Corporate / Human Resource / Learning and Development / Competency Assessment

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/competency-assessments`

Sama polanya. Hak akses `WorkforceCompetencyAssessment : Read`, `: Create`, `: Update`,
`: Delete`.

**Batas yang mengikat.** Keterkaitan kompetensi dan pelatihan dengan **kewenangan klinis**
`BLOCKED` — itu bagian `S-C1`.

### Corporate / Human Resource / Performance Management / Performance Review

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/performance-reviews`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar penilaian kinerja pegawai ini | `PerformanceReview : Read` | Query | `ApiResponse<PagedResult<ReviewResponse>>` | |
| `GET` | `/{id:guid}` | Detail satu penilaian | `PerformanceReview : Read` | — | `ApiResponse<ReviewDetailResponse>` | |
| `POST` | `/` | Membuat penilaian | `PerformanceReview : Create` | `CreateReviewRequest` | `ApiResponse<ReviewDetailResponse>` | |
| `PUT` | `/{id:guid}` | Mengubah penilaian | `PerformanceReview : Update` | `UpdateReviewRequest` | `ApiResponse<ReviewDetailResponse>` | |
| `PATCH` | `/{id:guid}/status` | Mengubah tahap penilaian | `PerformanceReview : Update` | `UpdateStatusRequest` | `ApiResponse<ReviewDetailResponse>` | |
| `PATCH` | `/{id:guid}/finalize` | Memfinalkan penilaian | `PerformanceReview : Update` | — | `ApiResponse<ReviewDetailResponse>` | |
| `PATCH` | `/{id:guid}/acknowledge` | Pegawai mengakui hasil penilaian | `PerformanceReview : Update` | — | `ApiResponse<ReviewDetailResponse>` | |
| `DELETE` | `/{id:guid}` | Menandai penilaian terhapus | `PerformanceReview : Delete` | — | `ApiResponse<object>` | |

**Guard yang benar-benar dijaga kode, dan layak ditiru.** `finalize` menolak bila masih ada
rincian yang belum berskor. `acknowledge` menolak bila belum difinalkan. Setelah difinalkan,
`PUT`, `PATCH status`, dan `DELETE` ditolak, termasuk pada rinciannya `[EXISTING]`.

### Corporate / Human Resource / Employee Relation Management / Disciplinary Action

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/disciplinary-actions`

Mengikuti pola sembilan endpoint, ditambah `PATCH /{id:guid}/acknowledge` dan
`PATCH /{id:guid}/appeal`. Hak akses `WorkforceDisciplinaryAction : Read`, `: Create`,
`: Update`, `: Delete`.

**Dua temuan yang harus dibaca sebelum kemampuan ini diperluas.** Pertama, `PATCH status` hanya
memeriksa **keanggotaan himpunan** nilai, bukan urutan transisi yang sah — status apa pun dapat
berpindah ke status lain. Kedua, **pembuat tindakan dapat menyetujui tindakannya sendiri**.
Keduanya `[OPEN]` — `HRD-Q-51` dan `HRD-Q-52`.

---

## 10. Endpoint yang tidak dijaga hak akses per aksi

Dua controller HR **tidak memiliki `[AccessPermission]`** pada action-nya, sementara 148 lainnya
memilikinya. Keduanya tetap memiliki `[Authorize]`, sehingga **tidak** terbuka tanpa
autentikasi — tetapi tidak dapat dibatasi per aksi.

| Controller | Jumlah endpoint | Lokasi file | Dampak |
| --- | ---: | --- | --- |
| `WfpWorkScheduleAssignmentController` | 8 | `Areas/Corporate/HumanResource/SchedulingManagement/Controllers/WfpWorkScheduleAssignmentController.cs` | Siapa pun yang masuk dapat menempatkan, mengubah, dan menghapus jadwal kerja pegawai mana pun |
| `AttendanceSelfServiceController` | 7 | `Areas/SelfServices/HumanResource/Controllers/AttendanceSelfServiceController.cs` | Kepemilikan tetap diturunkan dari pengguna terautentikasi, sehingga pegawai tetap hanya menyentuh datanya sendiri. Dampaknya lebih kecil, tetapi tetap menyimpang dari pola |

Ini **temuan yang dicatat**, bukan perbaikan yang dikerjakan dari alur blueprint. Perbaikannya
menjadi task implementasi tersendiri.

---

## 11. Endpoint yang **tidak** dirancang pada kontrak ini

| Kelompok | Alasan |
| --- | --- |
| Kredensial, lisensi, kewenangan klinis, SPK/RKK, OPPE, FPPE | `S-C1` `BLOCKED`. Lima controller yang ada hari ini **tidak** dituliskan bentuk targetnya |
| Rekam kesehatan kerja staf | `S-C6` `BLOCKED`. Satu controller yang ada hari ini **tidak** dituliskan bentuk targetnya |
| Perencanaan tenaga kerja, rekrutmen, benefit, tiket HR | `S-D1` s.d. `S-D4` `BLOCKED` oleh `HRD-Q-05` |
| Perjalanan dinas dan reimbursement | `S-D5` `DEFERRED` |
| Kalkulasi payroll lintas domain dan persetujuan tingkat run | `MISSING` di source, dan `HRD-Q-49` belum menjawab bagaimana putaran payroll dimulai |
| Bentuk data serah terima ke Finance | `[OPEN]` `HRD-Q-10`. Merancangnya berarti menetapkan kontrak lintas modul secara sepihak |
| Perilaku bila Finance menolak batch | `[OPEN]` `HRD-Q-11` |
| Izin pulang cepat sebagai kemampuan berdiri sendiri | `HRD-DEC-029` sudah menetapkan pemisahannya dari cuti per jam, tetapi nilai policy-nya `[OPEN]` `HRD-Q-47` |

---

## 12. Traceability

| Kelompok endpoint | Decision ID / Capability ID | Flow |
| --- | --- | --- |
| Alias route kebab-case | `HRD-DEC-016`; `HRD-TF-006` | — |
| Daftar lintas-pegawai | `HRD-DEC-012`; `HRD-CAP-03` | `flows/01-employee-administration.md` |
| Kotak masuk terpadu | `HRD-DEC-011`, `HRD-DEC-018`; `HRD-CAP-23`, `HRD-CAP-24` | `flows/09-unified-approval.md` |
| Mesin pengingat dan eskalasi | `HRD-DEC-030` | `flows/09-unified-approval.md` |
| Klasifikasi pengecualian kerja di luar jadwal | `HRD-DEC-013`, `HRD-DEC-025` | `flows/02-attendance.md` |
| Koreksi kehadiran atas nama pegawai | `HRD-DEC-028`; `HRD-CAP-04` | `flows/07-attendance-correction.md` |
| `synchronize` tidak menurunkan `Applied` | `HRD-DEC-022` | `flows/02-attendance.md` |
| `reverse` cuti terkendali | `HRD-DEC-023`; `HRD-CAP-07` | `flows/03-leave.md` |
| Override acknowledgement pemanggilan kembali | `HRD-DEC-024` | `flows/03-leave.md` |
| Roster dan penjadwalan operasional | `HRD-DEC-026`; `HRD-CAP-09` | `flows/05-work-scheduling.md` |
| Guard jadwal berlaku surut | `HRD-DEC-027` | `flows/05-work-scheduling.md` |
| Batas serah terima payroll | `HRD-DEC-009`; `HRD-CAP-10` | `flows/10-payroll-processing-handoff.md` |
| Daftar periksa offboarding | `HRD-CAP-17` | `flows/11-lifecycle-offboarding.md` |
| Layanan mandiri pegawai | `HRD-DEC-007`; `HRD-CAP-25` | `flows/01` s.d. `flows/08` |
