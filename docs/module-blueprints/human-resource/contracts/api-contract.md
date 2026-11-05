# Human Resource — Kontrak API

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Dokumen | `contracts/api-contract.md` |
| `contract_version` | `v6` — angka set kontrak disimpan di `blueprint-manifest.md` field `contract_versions` |
| `last_changed_in` | `v7` — bagian 13 (`S-D1`) ditambahkan 3 November 2026; bagian lain tidak berubah sejak `v6` |
| Status | `draft` — **belum** `approved` |
| Owner | Backend, mengikuti `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` |
| `approved_by` / `approved_at` | **Hanya bagian 13** (`S-D1` Perencanaan Tenaga Kerja): `approved` oleh Pengguna — pemilik teknis dan produk (`HRD-DEC-015`) — pada 2026-11-03. Bagian lain dokumen ini **belum** disetujui, sehingga status dokumen tetap `draft` |
| `input_revision` | `02-backend-architecture.md` revision `2`; `00-interview-decisions.md` revision `16` (`HRD-DEC-046` s.d. `HRD-DEC-057`) |
| `input_hash` — decision log | `26da64e129b0c34a17b67b0a41f47986b1aedae92a600077d936b0741eb574ee` |
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

#### Keamanan yang mengikat seluruh endpoint gaji dan slip gaji

`[DECISION]` `HRD-DEC-038`, `HRD-DEC-039`, `HRD-DEC-040`. Seluruhnya **kontrak sasaran** —
belum ada satu pun yang berjalan hari ini.

| Aturan | Bunyi |
| --- | --- |
| Otentikasi bertingkat | Endpoint yang mengembalikan nominal gaji atau slip gaji **MUST** menuntut `SALARY_SENSITIVE_SESSION` yang masih berlaku. Tanpa itu: `401` |
| Kepemilikan diturunkan backend | Untuk endpoint layanan mandiri, kepemilikan diturunkan dari `pengguna terautentikasi → profil workforce → pemilik slip gaji`. Pengenal dari layar **MUST NOT** dipercaya |
| Audit `GET` sensitif | **Pengecualian** terhadap konvensi project. Pembacaan nominal dan slip gaji **MUST** dicatat; nominalnya sendiri **MUST NOT** masuk catatan |
| Cache | Response **MUST** memakai `Cache-Control: no-store` atau padanan canonical |
| Unduhan berkas | **MUST NOT** memakai URL statis publik yang dapat ditebak |

**Endpoint sasaran yang dibutuhkan keputusan ini:**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `api/v1/corporate/human-resource/salary-access/verify` | Konfirmasi kata sandi untuk membuka data gaji | Pengguna terautentikasi | `VerifySalaryAccessRequest` | `ApiResponse<SalarySensitiveSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `api/v1/self-services/human-resource/my-payslips` | Daftar slip gaji milik sendiri | `MyPayslip : Read` **ditambah** sesi gaji sensitif | Query | `ApiResponse<PagedResult<PayslipResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `api/v1/self-services/human-resource/my-payslips/{id:guid}` | Detail slip gaji milik sendiri | Sama | — | `ApiResponse<PayslipDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `api/v1/self-services/human-resource/my-payslips/{id:guid}/download` | Mengunduh berkas slip gaji | Sama | — | Berkas, `no-store` | **Rencana (belum tersedia)** |

**Kode status khas kelompok ini.** `401` berarti sesi gaji sensitif belum ada atau sudah
kedaluwarsa — petugas diminta memasukkan kata sandi lagi, **bukan** diminta masuk ulang. `403`
pada slip gaji berarti sumber daya itu bukan milik pemohon, dan jawabannya **MUST NOT**
membocorkan apakah slip gaji dengan pengenal itu ada atau tidak.

**Larangan implementasi yang mudah dilanggar:** jangan membuat pemverifikasi kata sandi baru
untuk HR. Endpoint `salary-access/verify` **MUST** memakai infrastruktur verifikasi kata sandi
Identity yang canonical.

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
| `POST` | `/on-behalf` | HR mengajukan permohonan atas nama pegawai yang kesulitan akses | `EmployeeProfileChange : CreateOnBehalf` | `CreateProfileChangeOnBehalfRequest` | `ApiResponse<ProfileChangeDetailResponse>` | Tersedia (multipart/form-data) |
| `GET` | `/{id:guid}/evidence/download` | Mengunduh berkas bukti pendukung permohonan | `EmployeeProfileChange : Read` | — | Berkas stream PDF/JPG/PNG | Tersedia (`HRD-DEC-052`) |

### Corporate / Human Resource / Workforce Core / Personnel Action Order

Base URL: `api/v1/corporate/human-resource/personnel-action-orders`

Mengelola seluruh peristiwa kepegawaian (Mutasi, Promosi, Rotasi, Demosi, Penugasan Sementara) sebagai satu kesatuan dokumen utuh (`HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan kategori, status, dan saringan SK | `PersonnelActionOrder : Read` | — | `ApiResponse<FilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah SK per status | `PersonnelActionOrder : Read` | — | `ApiResponse<SummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar dokumen peristiwa seluruh pegawai | `PersonnelActionOrder : Read` | Query | `ApiResponse<PagedResult<PersonnelActionOrderListResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Detail lengkap satu SK dan penempatan | `PersonnelActionOrder : Read` | — | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf dokumen peristiwa baru | `PersonnelActionOrder : Create` | `CreatePersonnelActionOrderRequest` | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id:guid}` | Mengubah isi draf dokumen peristiwa | `PersonnelActionOrder : Update` | `UpdatePersonnelActionOrderRequest` | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/attachments` | Mengunggah berkas scan SK resmi bertandatangan | `PersonnelActionOrder : Update` | Multipart Form Data | `ApiResponse<AttachmentResponse>` | **Rencana (belum tersedia)** — Wajib sebelum `/submit` (`HRD-DEC-056`) |
| `GET` | `/{id:guid}/attachments/{attachmentId:guid}` | Mengunduh berkas scan SK bertandatangan | `PersonnelActionOrder : Read` | — | Berkas PDF/Gambar | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan SK untuk persetujuan (pola T8) | `PersonnelActionOrder : Update` | — | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui SK (`APPROVER_MUST_DIFFER_FROM_CREATOR`) | `PersonnelActionOrder : Approve` | — | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reject` | Menolak SK | `PersonnelActionOrder : Approve` | `RejectOrderRequest` | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** — Alasan wajib diisi |
| `POST` | `/{id:guid}/cancel-request` | Mengajukan pembatalan SK terjadwal | `PersonnelActionOrder : Cancel` | `CancelOrderRequest` | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** — Hanya sebelum tgl berlaku (`HRD-DEC-057`) |
| `POST` | `/{id:guid}/cancel-approve` | Menyetujui pembatalan SK | `PersonnelActionOrder : Approve` | — | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/apply` | Menerapkan penempatan baru dan menutup penempatan lama pada tanggal berlaku | `PersonnelActionOrder : Process` | — | `ApiResponse<PersonnelActionOrderDetailResponse>` | **Rencana (belum tersedia)** |

### Corporate / Human Resource / Workforce Core / Employee 360

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/employee-360`

Menyajikan profil komprehensif pegawai dengan kontrol akses berbasis relasi (`HR` vs `Atasan Langsung`, `HRD-DEC-053`).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Mengambil profil Employee 360 | `Employee360 : Read` | — | `ApiResponse<Employee360Response>` | **Rencana (belum tersedia)** |

**Aturan Otorisasi Employee 360 (`HRD-DEC-053`):**
1. Petugas HR: melihat seluruh profil kerja, kontrak, riwayat karir, sertifikat, kehadiran, cuti. Nominal gaji hanya tampil jika memegang `WfpSalaryAssignment : ViewAmount`.
2. Atasan Langsung: hanya dapat membuka Employee 360 pegawai yang **atasan utamanya pada tanggal hari ini** adalah pengguna tersebut (`ONLY_PRIMARY_MANAGER_ROUTES_APPROVAL`). Melihat profil kerja, kontrak, riwayat mutasi/promosi, kompetensi, kontak darurat, ringkasan cuti/kehadiran. **Dilarang keras menampilkan nominal gaji, rekening bank, NIK, NPWP, keluarga/tanggungan, dan catatan disiplin.**
3. Pengguna yang bukan HR dan bukan atasan langsung bawahan: **Ditolak HTTP 403 Forbidden**.

### Corporate / Human Resource / Workforce Core / Initial Placement Batch

Base URL: `api/v1/corporate/human-resource/workforce-core/initial-batches`

Mengelola batch pengisian data awal penempatan bagi 55 pegawai aktif (`HRD-DEC-054`).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/template` | Mengunduh template batch dengan prefill 55 pegawai aktif | `InitialPlacementBatch : Read` | — | Berkas XLSX/CSV | **Rencana (belum tersedia)** |
| `POST` | `/validate` | Memvalidasi integritas baris penempatan awal | `InitialPlacementBatch : Create` | Multipart File | `ApiResponse<BatchValidationResultResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengunggah dan menyimpan batch data awal (Draft) | `InitialPlacementBatch : Create` | `CreateInitialBatchRequest` | `ApiResponse<InitialBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar riwayat batch data awal | `InitialPlacementBatch : Read` | Query | `ApiResponse<PagedResult<InitialBatchListResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id:guid}` | Detail satu batch dan seluruh barisnya | `InitialPlacementBatch : Read` | — | `ApiResponse<InitialBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/submit` | Mengajukan batch data awal untuk persetujuan | `InitialPlacementBatch : Update` | — | `ApiResponse<InitialBatchDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/approve` | Menyetujui batch sekali dan menerapkan penempatan awal | `InitialPlacementBatch : Approve` | — | `ApiResponse<InitialBatchDetailResponse>` | **Rencana (belum tersedia)** — Disetujui sekali (`HRD-DEC-054`) |

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
| `Self Services / Human Resource / Profile Change` | `.../profile-changes` | Mengajukan perubahan data pribadi mandiri | `MyProfileChange : Read`, `: Create`, `: Update`, `: Submit`, `: Cancel`, `: Delete` |

### Self Services / Human Resource / Profile Change

Base URL: `api/v1/self-services/human-resource/profile-changes`

Mengelola pengajuan perubahan data pribadi profil mandiri karyawan login (`HRD-DEC-091`..`094`).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/current-profile` | Mengambil snapshot profil lengkap pengguna login | `MyProfileChange : Read` | — | `ApiResponse<EmployeeCurrentProfileResponse>` | Tersedia |
| `GET` | `/summary` | Ringkasan statistik pengajuan mandiri | `MyProfileChange : Read` | — | `ApiResponse<EmployeeProfileChangeSummaryResponse>` | Tersedia |
| `GET` | `/` | Daftar riwayat pengajuan mandiri | `MyProfileChange : Read` | Query (paging) | `ApiResponse<PagedResult<EmployeeProfileChangeListItemResponse>>` | Tersedia |
| `GET` | `/{id:guid}` | Detail satu pengajuan mandiri | `MyProfileChange : Read` | — | `ApiResponse<EmployeeProfileChangeDetailResponse>` | Tersedia |
| `POST` | `/` | Membuat draf permohonan perubahan profil | `MyProfileChange : Create` | `CreateEmployeeProfileChangeDraftRequest` | `ApiResponse<EmployeeProfileChangeDetailResponse>` | Tersedia |
| `PUT` | `/{id:guid}` | Memperbarui draf permohonan | `MyProfileChange : Update` | `UpdateEmployeeProfileChangeDraftRequest` | `ApiResponse<EmployeeProfileChangeDetailResponse>` | Tersedia |
| `POST` | `/{id:guid}/evidence` | Mengunggah berkas bukti pendukung | `MyProfileChange : Update` | Multipart Form Data (`file`, `remarks`) | `ApiResponse<EmployeeProfileChangeEvidenceFileResponse>` | Tersedia (`HRD-DEC-052`) |
| `GET` | `/{id:guid}/evidence/download` | Mengunduh berkas bukti pendukung | `MyProfileChange : Read` | — | Berkas stream PDF/JPG/PNG | Tersedia |
| `DELETE` | `/{id:guid}/evidence` | Menghapus berkas bukti draf | `MyProfileChange : Update` | — | `ApiResponse<bool>` | Tersedia |
| `POST` | `/{id:guid}/submit` | Mengajukan draf ke Workflow Engine | `MyProfileChange : Submit` | `SubmitEmployeeProfileChangeRequest` | `ApiResponse<EmployeeProfileChangeDetailResponse>` | Tersedia |
| `POST` | `/{id:guid}/cancel` | Membatalkan permohonan mandiri | `MyProfileChange : Cancel` | `CancelEmployeeProfileChangeRequest` | `ApiResponse<EmployeeProfileChangeDetailResponse>` | Tersedia |
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
| Rekrutmen, benefit, tiket HR | `S-D2` s.d. `S-D4` `BLOCKED` oleh `HRD-Q-05`. Perencanaan tenaga kerja (`S-D1`) dirancang pada bagian 13 |
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

---

## 13. Perencanaan Tenaga Kerja — `S-D1` (`last_changed_in` `v7`, 3 November 2026)

Sumber desain: `02-backend-architecture.md` bagian 16. **Seluruh endpoint pada bagian ini belum ada
di kode** dan berlabel **Rencana (belum tersedia)**. Pola bagian 0.1 dan kode status bagian 0.3
berlaku. Base URL bersama: `api/v1/corporate/human-resource/workforce-planning`.

Aturan yang berlaku untuk seluruh grup:

| Aturan | Isi |
| --- | --- |
| Pembaruan dokumen | Request ubah, ajukan, batal, dan tarik kembali **MUST** membawa `RowVersion`; versi usang → `409` |
| Persetujuan | **Tidak ada** endpoint setuju/tolak/kembalikan di bagian ini. Keputusan `T9` s.d. `T13` lewat kotak masuk persetujuan, bagian 6 |
| Cakupan unit | Kepala Unit hanya melihat dan mengajukan untuk unit di dalam cakupan organisasinya; di luar itu `GET` daftar tidak memuatnya dan `GET {id}` menjawab `404` |
| Kolom biaya | Hanya terisi bila pemanggil memegang `WorkforcePlanningCost : Read`; selain itu kolom biaya bernilai `null` dan `costVisible = false` |

### Corporate / Human Resource / Workforce Planning / Staffing Standard

Base URL: `api/v1/corporate/human-resource/workforce-planning/staffing-standards`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar standar per unit, versi, dan status | `WorkforceStaffingStandard : Read` | `StaffingStandardPagedQuery` | `ApiResponse<PagedResult<StaffingStandardListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu versi beserta baris dan skill mix | `WorkforceStaffingStandard : Read` | — | `ApiResponse<StaffingStandardResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/effective` | Standar yang berlaku untuk satu unit pada satu tanggal — **kontrak baca untuk Penjadwalan** | `WorkforceStaffingStandard : Read` | `organizationUnitId`, `date` | `ApiResponse<StaffingStandardResponse>`; `404` bila belum ada standar berlaku | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf versi baru untuk satu unit; bila sudah ada versi aktif, isinya disalin sebagai titik awal | `WorkforceStaffingStandard : Create` | `CreateStaffingStandardRequest` | `ApiResponse<StaffingStandardResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf atau versi yang dikembalikan, termasuk baris dan skill mix | `WorkforceStaffingStandard : Update` | `UpdateStaffingStandardRequest` | `ApiResponse<StaffingStandardResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/recommendation` | Angka rekomendasi per baris beserta rincian hitung, untuk periode tertentu | `WorkforceStaffingStandard : Read` | `periodStartDate` | `ApiResponse<StaffingRecommendationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan ke `T9` | `WorkforceStaffingStandard : Submit` | `SubmitDocumentRequest` | `ApiResponse<StaffingStandardResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan draf atau versi yang dikembalikan | `WorkforceStaffingStandard : Cancel` | `CancelDocumentRequest` | `ApiResponse<StaffingStandardResponse>` | **Rencana (belum tersedia)** |

`CreateStaffingStandardRequest`: `organizationUnitId`, `effectiveStartDate`. `UpdateStaffingStandardRequest`:
`effectiveStartDate`, `justification`, `lines[]` (`positionId`, `professionId`, `calculationMethod`,
`minimumHeadcount`, `ratioStaffQuantity`, `ratioWorkloadQuantity`, `workloadUnit`, `shiftsPerDay`,
`staffPerShift`, `serviceHoursPerDay`, `reliefFactor`, `roundingMethod`, `countsOutsourced`, `notes`),
`skillMixRules[]` (`shiftId`, `professionId`, `competencyId`, `minimumCompetencyLevel`,
`minimumQualifiedHeadcount`, `isMandatory`), `rowVersion`.

`StaffingRecommendationResponse` per baris: `positionId`, `calculationMethod`, `recommendedQuantity`,
`breakdown` (contoh "7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32"), `missingInput`
(contoh "angka beban belum diisi").

### Corporate / Human Resource / Workforce Planning / Staffing Workload

Base URL: `api/v1/corporate/human-resource/workforce-planning/staffing-workloads`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar angka beban per unit, jabatan, periode | `WorkforceStaffingWorkload : Read` | `StaffingWorkloadPagedQuery` | `ApiResponse<PagedResult<StaffingWorkloadResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengisi angka beban satu periode | `WorkforceStaffingWorkload : Create` | `UpsertStaffingWorkloadRequest` | `ApiResponse<StaffingWorkloadResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Memperbaiki angka beban | `WorkforceStaffingWorkload : Update` | `UpsertStaffingWorkloadRequest` | `ApiResponse<StaffingWorkloadResponse>` | **Rencana (belum tersedia)** |

`UpsertStaffingWorkloadRequest`: `organizationUnitId`, `positionId`, `periodStartDate`, `periodEndDate`,
`workloadValue`, `sourceNote`.

### Corporate / Human Resource / Workforce Planning / Position Formation

Base URL: `api/v1/corporate/human-resource/workforce-planning/position-formations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Formasi yang berlaku per unit × jabatan pada satu tanggal | `WorkforcePositionFormation : Read` | `PositionFormationPagedQuery` (`hospitalSiteId`, `organizationUnitId`, `positionId`, `date`) | `ApiResponse<PagedResult<PositionFormationResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/history` | Seluruh versi satu unit × jabatan beserta dokumen sumbernya | `WorkforcePositionFormation : Read` | `organizationUnitId`, `positionId` | `ApiResponse<List<PositionFormationResponse>>` | **Rencana (belum tersedia)** |

**Tidak ada** `POST`, `PUT`, `PATCH`, maupun `DELETE` (`NO_DIRECT_FORMATION_EDIT`). Permintaan
dengan method lain ke base URL ini menjawab `405`.

### Corporate / Human Resource / Workforce Planning / Initial Formation Batch

Base URL: `api/v1/corporate/human-resource/workforce-planning/initial-formation-batches`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/template` | Mengunduh templat isian | `WorkforceInitialFormationBatch : Read` | — | Berkas `xlsx` | **Rencana (belum tersedia)** |
| `POST` | `/validate` | Memeriksa baris sebelum disimpan | `WorkforceInitialFormationBatch : Read` | `List<InitialFormationBatchRowRequest>` | `ApiResponse<List<InitialFormationBatchRowValidationResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar batch | `WorkforceInitialFormationBatch : Read` | `PagedQuery` | `ApiResponse<PagedResult<InitialFormationBatchListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail batch beserta baris | `WorkforceInitialFormationBatch : Read` | — | `ApiResponse<InitialFormationBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf batch | `WorkforceInitialFormationBatch : Create` | `CreateInitialFormationBatchRequest` | `ApiResponse<InitialFormationBatchResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf | `WorkforceInitialFormationBatch : Update` | `UpdateInitialFormationBatchRequest` | `ApiResponse<InitialFormationBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/source-document` | Mengunggah scan dokumen formasi resmi | `WorkforceInitialFormationBatch : Update` | `multipart/form-data` berkas PDF/JPG/PNG | `ApiResponse<InitialFormationBatchResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/source-document` | Mengunduh scan dokumen | `WorkforceInitialFormationBatch : Read` | — | Berkas | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan ke `T10` | `WorkforceInitialFormationBatch : Submit` | `SubmitDocumentRequest` | `ApiResponse<InitialFormationBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan draf atau batch yang dikembalikan | `WorkforceInitialFormationBatch : Cancel` | `CancelDocumentRequest` | `ApiResponse<InitialFormationBatchResponse>` | **Rencana (belum tersedia)** |

`CreateInitialFormationBatchRequest`: `hospitalSiteId`, `sourceDocumentReference`, `notes`,
`rows[]` (`organizationUnitId`, `positionId`, `formationQuantity`, `effectiveStartDate`).

### Corporate / Human Resource / Workforce Planning / Manpower Plan

Base URL: `api/v1/corporate/human-resource/workforce-planning/manpower-plans`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar rencana dan addendum | `WorkforceManpowerPlan : Read` | `ManpowerPlanPagedQuery` | `ApiResponse<PagedResult<ManpowerPlanListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta baris; kolom biaya mengikuti aturan di atas | `WorkforceManpowerPlan : Read` | — | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf rencana tahunan | `WorkforceManpowerPlan : Create` | `CreateManpowerPlanRequest` | `ApiResponse<ManpowerPlanResponse>` `201` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/addenda` | Membuat draf addendum dari rencana `Approved` | `WorkforceManpowerPlan : Create` | `CreateManpowerPlanAddendumRequest` | `ApiResponse<ManpowerPlanResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf atau dokumen yang dikembalikan | `WorkforceManpowerPlan : Update` | `UpdateManpowerPlanRequest` | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/recalculate` | Menghitung ulang formasi saat ini, rekomendasi, dan biaya pada draf | `WorkforceManpowerPlan : Update` | `rowVersion` | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan ke `T10`; potret gap tersimpan | `WorkforceManpowerPlan : Submit` | `SubmitDocumentRequest` | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan draf atau dokumen yang dikembalikan | `WorkforceManpowerPlan : Cancel` | `CancelDocumentRequest` | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finance-adjustment` | Koreksi angka total oleh Finance pada langkah Finance `T10` | `WorkforcePlanningCost : Adjust` | `FinanceAdjustmentRequest` (`adjustedTotal`, `reason`, `rowVersion`) | `ApiResponse<ManpowerPlanResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/gap-snapshots` | Potret gap yang menempel pada dokumen | `WorkforceStaffingGap : Read` | — | `ApiResponse<List<StaffingGapSnapshotResponse>>` | **Rencana (belum tersedia)** |

`UpdateManpowerPlanRequest`: `referenceDate`, `justification`, `lines[]` (`organizationUnitId`,
`positionId`, `salaryGradeId`, `proposedFormationQuantity`, `deviationReason`, `effectiveStartDate`),
`rowVersion`. Kolom potret (`currentFormationQuantity`, `recommendedQuantity`, biaya) diisi sistem
lewat `/recalculate` dan tidak diterima dari request.

### Corporate / Human Resource / Workforce Planning / Headcount Request

Base URL: `api/v1/corporate/human-resource/workforce-planning/headcount-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permintaan | `WorkforceHeadcountRequest : Read` | `HeadcountRequestPagedQuery` (status, unit, jabatan, `isOverdue`, `isOverFormationAfterAddendum`) | `ApiResponse<PagedResult<HeadcountRequestListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta kursi, potret, pembatalan, dan riwayat rencana pemenuhan | `WorkforceHeadcountRequest : Read` | — | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/remaining-seats` | Sisa kursi langsung satu unit × jabatan | `WorkforceHeadcountRequest : Read` | `organizationUnitId`, `positionId`, `date` | `ApiResponse<RemainingSeatsResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf | `WorkforceHeadcountRequest : Create` | `CreateHeadcountRequestRequest` | `ApiResponse<HeadcountRequestResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf atau dokumen yang dikembalikan | `WorkforceHeadcountRequest : Update` | `UpdateHeadcountRequestRequest` | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan ke `T11` atau `T12` menurut jenisnya; potret gap tersimpan | `WorkforceHeadcountRequest : Submit` | `SubmitDocumentRequest` | `ApiResponse<HeadcountRequestResponse>`; `422` bila sisa kursi tidak cukup | **Rencana (belum tersedia)** |
| `POST` | `/{id}/withdraw` | Menarik kembali sebelum persetujuan terakhir | `WorkforceHeadcountRequest : Withdraw` | `WithdrawRequest` (`reason`, `rowVersion`) | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finance-adjustment` | Koreksi biaya oleh Finance pada langkah Finance `T12` | `WorkforcePlanningCost : Adjust` | `FinanceAdjustmentRequest` | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/fulfilment-plan` | Mengubah sumber pemenuhan kursi yang belum terpenuhi | `WorkforceHeadcountRequest : ChangeFulfilmentPlan` | `ChangeFulfilmentPlanRequest` (`seats[]` `{seatId, toSource}`, `reason`, `rowVersion`) | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancellations` | Mengajukan pembatalan kursi yang belum terpenuhi ke `T13` | `WorkforceHeadcountRequest : RequestCancellation` | `CreateHeadcountCancellationRequest` (`seatIds[]`, `reason`) | `ApiResponse<HeadcountCancellationResponse>` `201`; `409` bila lowongan kursi itu masih terbuka | **Rencana (belum tersedia)** |
| `POST` | `/{id}/seats/{seatId}/fulfilment-link` | Menautkan penempatan atau SK ke kursi | `WorkforceHeadcountRequest : LinkFulfilment` | `LinkFulfilmentRequest` (`positionAssignmentId` **atau** `personnelActionOrderId`) | `ApiResponse<HeadcountRequestResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}/seats/{seatId}/fulfilment-link` | Melepas tautan selama penempatan belum berlaku | `WorkforceHeadcountRequest : LinkFulfilment` | `reason` | `ApiResponse<HeadcountRequestResponse>`; `409` bila kursi sudah `Fulfilled` | **Rencana (belum tersedia)** |

`CreateHeadcountRequestRequest`: `organizationUnitId`, `positionId`, `salaryGradeId`, `requestType`,
`requiredStartDate`, `temporaryEndDate`, `temporaryReasonWorkforceProfileId`, `justification`,
`seats[]` (`seatFte`, `fulfilmentSource`, `replacedWorkforceProfileId`).

### Corporate / Human Resource / Workforce Planning / Recruitment Queue

Base URL: `api/v1/corporate/human-resource/workforce-planning/recruitment-queue`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Kursi luar berstatus `Open` dari permintaan `ReadyForRecruitment` atau `PartiallyFulfilled`; kursi `Frozen` tidak muncul | `WorkforceRecruitmentQueue : Read` | `PagedQuery` | `ApiResponse<PagedResult<RecruitmentQueueItemResponse>>` — **tanpa** kolom biaya | **Rencana (belum tersedia)** |

Antrean ini dilayani `HrdRecruitmentQueueController` tersendiri — `[Tags]` di repository ini dipasang per
controller — agar Recruiter tidak perlu hak baca permintaan secara utuh.

### Corporate / Human Resource / Workforce Planning / Staffing Gap

Base URL: `api/v1/corporate/human-resource/workforce-planning/staffing-gaps`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Gap **langsung** per unit × jabatan pada tanggal acuan, beserta lapisan dan penanda kelengkapan | `WorkforceStaffingGap : Read` | `hospitalSiteId`, `organizationUnitId`, `referenceDate` | `ApiResponse<List<StaffingGapLineResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/snapshots` | Daftar potret pengajuan, bulanan, dan rekonstruksi | `WorkforceStaffingGap : Read` | `StaffingGapSnapshotPagedQuery` | `ApiResponse<PagedResult<StaffingGapSnapshotListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/snapshots/{id}` | Detail satu potret | `WorkforceStaffingGap : Read` | — | `ApiResponse<StaffingGapSnapshotResponse>` | **Rencana (belum tersedia)** |

`StaffingGapLineResponse`: `organizationUnitId`, `positionId`, `formationQuantity`, `filledFte`,
`toBeVacatedFte`, `temporarilyUnavailableFte`, `notYetCompetentFte`, `concurrentAssignmentCount`,
`outsourcedFte`, `permanentGap`, `temporaryGap`, `competencyGap`, dan empat penanda kelengkapan. Nilai
`null` berarti **data belum tersedia**, bukan nol. Tidak ada endpoint ubah atau hapus potret.

### Corporate / Human Resource / Workforce Planning / Setting

Base URL: `api/v1/corporate/human-resource/workforce-planning/settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Pengaturan satu rumah sakit | `WorkforcePlanningSetting : Read` | `hospitalSiteId` | `ApiResponse<WorkforcePlanningSettingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengubah pengaturan | `WorkforcePlanningSetting : Update` | `UpdateWorkforcePlanningSettingRequest` (`hospitalSiteId`, `longLeaveThresholdDays`, `overdueRequestThresholdDays`, `changeReason`, `rowVersion`) | `ApiResponse<WorkforcePlanningSettingResponse>` | **Rencana (belum tersedia)** |

### 13.1 Persetujuan `T9` s.d. `T13` lewat kotak masuk

Kelima rantai memakai endpoint kotak masuk persetujuan bagian 6 tanpa endpoint baru. Jenis rujukan
yang muncul di kotak masuk:

| Rantai | Jenis rujukan | Dokumen |
| --- | --- | --- |
| `T9` | `WorkforceStaffingStandard` | Standar kebutuhan tenaga |
| `T10` | `WorkforceManpowerPlan`, `WorkforceInitialFormationBatch` | Rencana tahunan, addendum, batch formasi awal |
| `T11`, `T12` | `WorkforceHeadcountRequest` | Permintaan headcount |
| `T13` | `WorkforceHeadcountCancellation` | Pembatalan kursi permintaan yang sudah disetujui (`HRD-DEC-071`); satu langkah HR Manager |

### 13.2 Endpoint `Sudah ada` yang dibaca `S-D1`

| Grup | Endpoint | Dipakai untuk |
| --- | --- | --- |
| `Corporate / Human Resource / Master Data / Salary Grade` | `GET .../master-data/salary-grades/options` | Pilihan grade pada rencana dan permintaan |
| `Corporate / Human Resource / Workforce Core / Personnel Action Order` | `POST .../personnel-action-orders` dan alurnya | Kursi internal dipenuhi lewat SK |
| `Corporate / Human Resource / Workflow Management / Approval Inbox` | Bagian 6 | Seluruh persetujuan `T9` s.d. `T13` |


---

## 14. Rekrutmen dan Hiring — `S-D2` (`last_changed_in` `v8`, 3 November 2026)

Sumber desain: `02-backend-architecture.md` bagian 17. **Seluruh endpoint pada bagian ini belum ada di kode** dan berlabel
**Rencana (belum tersedia)**. Pola bagian 0.1 dan kode status bagian 0.3 berlaku.
Base URL bersama: `api/v1/corporate/human-resource/recruitment`.

### 14.1 Grup `[Tags("RecruitmentManagement - Requisitions")]`
Base path: `/requisitions`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar requisition terpaginasi | `Recruitment : Read` | Query (`page`, `pageSize`, `status`, `unitId`) | `ApiResponse<PagedResult<JobRequisitionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail requisition beserta kuota kursi | `Recruitment : Read` | — | `ApiResponse<JobRequisitionDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat requisition dari Headcount Request | `Recruitment : Create` | `CreateJobRequisitionRequest` (`headcountRequestId`, `positionId`, `requestedSeats`, `targetDate`) | `ApiResponse<JobRequisitionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf requisition | `Recruitment : Update` | `UpdateJobRequisitionRequest` (`requestedSeats`, `targetDate`, `rowVersion`) | `ApiResponse<JobRequisitionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan requisition untuk disahkan | `Recruitment : Update` | `SubmitJobRequisitionRequest` (`rowVersion`) | `ApiResponse<JobRequisitionResponse>` | **Rencana (belum tersedia)** |

### 14.2 Grup `[Tags("RecruitmentManagement - Vacancies")]`
Base path: `/vacancies`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar lowongan kerja terpaginasi | `Recruitment : Read` | Query (`page`, `pageSize`, `status`, `isClinical`) | `ApiResponse<PagedResult<JobVacancyResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail lowongan dan syarat kualifikasi | `Recruitment : Read` | — | `ApiResponse<JobVacancyDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat lowongan kerja baru dari Requisition | `Recruitment : Create` | `CreateJobVacancyRequest` (`jobRequisitionId`, `title`, `targetSeats`, `publishDate`, `closingDate`, `isClinical`) | `ApiResponse<JobVacancyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/publish` | Mempublikasikan lowongan kerja | `Recruitment : Update` | `PublishJobVacancyRequest` (`rowVersion`) | `ApiResponse<JobVacancyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close` | Menutup lowongan kerja | `Recruitment : Update` | `CloseJobVacancyRequest` (`rowVersion`) | `ApiResponse<JobVacancyResponse>` | **Rencana (belum tersedia)** |

### 14.3 Grup `[Tags("RecruitmentManagement - Candidates")]`
Base path: `/candidates`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pelamar terpaginasi | `Candidate : Read` | Query (`page`, `pageSize`, `searchNik`, `searchName`) | `ApiResponse<PagedResult<CandidateSummaryResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail profil pelamar dan dokumen | `Candidate : Read` | — | `ApiResponse<CandidateDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mendaftarkan pelamar baru (validasi NIK KTP) | `Candidate : Create` | `RegisterCandidateRequest` (`nationalIdNumber`, `fullName`, `email`, `phone`, `birthDate`, `gender`) | `ApiResponse<CandidateResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/documents` | Mengunggah dokumen pelamar (STR, Ijazah, dll) | `Candidate : Update` | `UploadCandidateDocumentRequest` (`documentType`, `documentNumber`, `expiryDate`, `fileUrl`) | `ApiResponse<CandidateDocumentResponse>` | **Rencana (belum tersedia)** |

### 14.4 Grup `[Tags("RecruitmentManagement - Screenings")]`
Base path: `/screenings`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/evaluate` | Penapisan berkas otomatis (Hard gate STR) | `Recruitment : Update` | `EvaluateScreeningRequest` (`applicationId`) | `ApiResponse<ScreeningResultResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/application/{applicationId}` | Melihat hasil penapisan berkas | `Recruitment : Read` | — | `ApiResponse<ScreeningResultResponse>` | **Rencana (belum tersedia)** |

### 14.5 Grup `[Tags("RecruitmentManagement - Evaluations")]`
Base path: `/evaluations`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/hr` | Submit formulir evaluasi wawancara HR | `Recruitment : Evaluate` | `SubmitHrEvaluationRequest` (`applicationId`, `behavioralScore`, `recommendation`, `notes`) | `ApiResponse<CandidateEvaluationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/clinical` | Submit uji klinis unit (dengan hak veto mutlak) | `ClinicalAssessment : Evaluate` | `SubmitClinicalEvaluationRequest` (`applicationId`, `competencyScore`, `recommendation`, `notes`) | `ApiResponse<CandidateEvaluationResponse>` | **Rencana (belum tersedia)** |

### 14.6 Grup `[Tags("RecruitmentManagement - Offers")]`
Base path: `/offers`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar surat penawaran terpaginasi | `JobOffer : Read` | Query (`page`, `pageSize`, `status`) | `ApiResponse<PagedResult<JobOfferSummaryResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail surat penawaran kerja | `JobOffer : Read` | — | `ApiResponse<JobOfferDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf surat penawaran bersyarat | `JobOffer : Create` | `CreateJobOfferRequest` (`applicationId`, `offeredBaseSalary`, `offeredFixedAllowance`, `targetJoinDate`) | `ApiResponse<JobOfferResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Persetujuan bertingkat (T13 standar / T14 eskalasi) | `JobOffer : Approve` | `ApproveJobOfferRequest` (`decision`, `comments`, `rowVersion`) | `ApiResponse<JobOfferResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/respond` | Mencatat konfirmasi kandidat (Accepted/Rejected) | `JobOffer : Update` | `CandidateOfferResponseRequest` (`responseStatus`, `rejectionReason`) | `ApiResponse<JobOfferResponse>` | **Rencana (belum tersedia)** |

### 14.7 Grup `[Tags("RecruitmentManagement - MedicalClearance")]`
Base path: `/medical-clearance`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/result` | Input status kelaikan fisik MCU (oleh dokter K3RS) | `OccupationalHealth : Evaluate` | `SubmitMcuClearanceRequest` (`jobOfferId`, `fitnessStatus`, `restrictionsNotes`) | `ApiResponse<McuClearanceResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{jobOfferId}` | Melihat status kelaikan MCU (masking diagnosa) | `Recruitment : Read` | — | `ApiResponse<McuClearanceSummaryResponse>` | **Rencana (belum tersedia)** |

### 14.8 Grup `[Tags("RecruitmentManagement - TalentPool")]`
Base path: `/talent-pool`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kandidat cadangan aktif berperingkat | `Recruitment : Read` | Query (`positionId`, `minScore`) | `ApiResponse<List<TalentPoolCandidateResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/promote` | Promosi cepat kandidat cadangan ke offer baru | `Recruitment : Update` | `PromoteTalentPoolRequest` (`jobRequisitionId`, `targetJoinDate`) | `ApiResponse<JobOfferResponse>` | **Rencana (belum tersedia)** |

### 14.9 Grup `[Tags("RecruitmentManagement - HiringHandoffs")]`
Base path: `/handoffs`

| Method | Path | Deskripsi | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/execute` | Serah terima kandidat final ke Onboarding | `Recruitment : Execute` | `ExecuteHiringHandoffRequest` (`jobOfferId`, `scheduledOrientationDate`) | `ApiResponse<HiringHandoffResponse>` | **Rencana (belum tersedia)** |


---

## 15. Onboarding dan Kontrak Kerja — `S-C4-ONB` (`last_changed_in` `v9`, 4 November 2026)

| Field | Isi |
| --- | --- |
| Status | `approved` — Pengguna (pemilik teknis dan produk, `HRD-DEC-015`), 2026-11-04, beserta kelima pilihan desain (`HRD-DEC-096`). Jawaban pemilik: "setujui semua ya dan kerjakan" |
| Sumber desain | `02-backend-architecture.md` bagian 18; decision log bagian 34 |
| Kompatibilitas | Endpoint baru seluruhnya; **satu perubahan memutus** pada grup `Contract History` 02 (15.11); penambahan isian pada grup tawaran `S-D2` (15.12) |

**Seluruh endpoint pada bagian ini belum ada di kode** dan berlabel **Rencana (belum tersedia)**.
Pola bagian 0.1 dan kode status bagian 0.3 berlaku. Base URL bersama:
`api/v1/corporate/human-resource/lifecycle-management`.

Aturan yang berlaku untuk seluruh grup:

| Aturan | Isi |
| --- | --- |
| Pembaruan dokumen | Setiap request ubah, ajukan, tarik, tandai, dan nyatakan **MUST** membawa `rowVersion`; versi usang → `409` (`VR-ONB-15`) |
| Persetujuan | **Tidak ada** endpoint setuju/tolak/minta revisi di bagian ini. Keputusan `T15` s.d. `T18` lewat kotak masuk persetujuan, bagian 6 |
| Cakupan unit | Kepala Unit hanya melihat dan bertindak atas pegawai di unit dalam cakupan organisasinya; di luar itu `GET` daftar tidak memuatnya dan `GET {id}` menjawab `404` |
| Berkas | Unggahan memakai `multipart/form-data`; PDF, JPG, atau PNG paling besar 10 MB (`VR-ONB-09`). Unduhan lewat endpoint terautentikasi, bukan path publik |
| Gaji | Nominal gaji hanya terisi bila pemanggil memegang `WfpSalaryAssignment : ViewAmount`; selain itu `null` dan `amountVisible = false` (`HRD-DEC-033`). Pembacaan nominal dicatat (`HRD-DEC-039`) |

### 15.1 Corporate / Human Resource / Lifecycle Management / Onboarding Case

Base URL: `api/v1/corporate/human-resource/lifecycle-management/onboarding-cases`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kasus onboarding | `WorkforceOnboardingCase : Read` | `OnboardingCasePagedQuery` (`hospitalSiteId`, `organizationUnitId`, `caseStatus`, `plannedStartFrom`, `plannedStartTo`, `coordinatorUserId`, `search`) | `ApiResponse<PagedResult<OnboardingCaseListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Hitungan per status, terlambat mulai, dan kontrak belum ditandatangani | `WorkforceOnboardingCase : Read` | `hospitalSiteId` | `ApiResponse<OnboardingCaseSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail kasus: butir, kontrak pertama, akun, pendampingan, peringatan | `WorkforceOnboardingCase : Read` | — | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/first-day-attendance` | Mencatat hadir hari pertama; mengaktifkan akun layanan mandiri | `WorkforceOnboardingCase : CompleteItem` | `RecordFirstDayRequest` (`attendedOn`, `rowVersion`) | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/first-day-attendance/undo` | Membatalkan catatan hadir yang salah, selama kontrak pertama belum ditandatangani | `WorkforceOnboardingCase : CompleteItem` | `UndoItemRequest` (`reason`, `rowVersion`) | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/items/{itemId}/complete` | Menandai butir milik HRD selesai | `WorkforceOnboardingCase : CompleteItem` | `multipart`: `completionNote`, `file` (wajib untuk butir wajib bukti), `rowVersion` | `ApiResponse<OnboardingCaseItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/items/{itemId}/undo` | Membatalkan centang butir milik HRD | `WorkforceOnboardingCase : CompleteItem` | `UndoItemRequest` | `ApiResponse<OnboardingCaseItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/unit-items/{itemId}/complete` | Menandai butir milik Kepala Unit selesai, termasuk memilih atasan langsung atau preceptor | `WorkforceOnboardingCase : CompleteUnitItem` | `multipart`: `completionNote`, `file`, `selectedWorkforceProfileId` (butir pemilihan orang), `rowVersion` | `ApiResponse<OnboardingCaseItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/unit-items/{itemId}/undo` | Membatalkan centang butir milik Kepala Unit | `WorkforceOnboardingCase : CompleteUnitItem` | `UndoItemRequest` | `ApiResponse<OnboardingCaseItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/automatic-items/refresh` | Memeriksa ulang butir otomatis sekarang juga | `WorkforceOnboardingCase : CompleteItem` | — | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/coordinator` | Mengalihkan koordinator HRD | `WorkforceOnboardingCase : Reassign` | `ReassignCoordinatorRequest` (`coordinatorUserId`, `rowVersion`) | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/declare-ready` | Menyatakan siap kerja | `WorkforceOnboardingCase : DeclareReady` | `DeclareReadyRequest` (`rowVersion`) | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel-join` | Batal bergabung | `WorkforceOnboardingCase : CancelJoin` | `CancelJoinRequest` (`cancelReason`, `cancelNote`, `rowVersion`) | `ApiResponse<OnboardingCaseResponse>` | **Rencana (belum tersedia)** |

`OnboardingCaseResponse`: `id`, `caseNumber`, `employeeName`, `positionName`, `organizationUnitName`,
`isClinical`, `plannedStartDate`, `caseStatus`, `coordinator`, `firstDayAttendedOn`,
`hasGeneralTemplate`, `items[]` (`id`, `itemCode`, `itemName`, `templateLayer`, `responsibleRole`,
`itemKind`, `isRequired`, `dueDate`, `itemStatus`, `completedAt`, `completedByName`, `selectedPersonName`,
`hasEvidence`, `automaticResultNote`, `canComplete`, `canUndo`), `requiredRemaining`,
`firstContract` (`contractHistoryId`, `contractStatus`, `contractTypeName`, `startDate`, `endDate`),
`accessRequests[]` (`requestType`, `requestStatus`, `failureMessage`), `warnings[]` (contoh
"Kewenangan klinis (SPK/RKK) belum tercatat"), `availableActions[]`, `rowVersion`.

`availableActions` dihitung backend (`AGENTS.md` bagian Alur Kerja): layar tidak menebak tombol sendiri.

### 15.2 Corporate / Human Resource / Lifecycle Management / Onboarding Template

Base URL: `api/v1/corporate/human-resource/lifecycle-management/onboarding-templates`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar template per lapis | `WorkforceOnboardingTemplate : Read` | `templateLayer`, `organizationUnitId`, `isActive` | `ApiResponse<List<OnboardingTemplateListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail template beserta butirnya | `WorkforceOnboardingTemplate : Read` | — | `ApiResponse<OnboardingTemplateResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat template; butir sistem lapis itu disisipkan otomatis | `WorkforceOnboardingTemplate : Create` | `UpsertOnboardingTemplateRequest` | `ApiResponse<OnboardingTemplateResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah template dan butirnya | `WorkforceOnboardingTemplate : Update` | `UpsertOnboardingTemplateRequest` | `ApiResponse<OnboardingTemplateResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `WorkforceOnboardingTemplate : Update` | `ToggleActiveRequest` (`isActive`, `rowVersion`) | `ApiResponse<OnboardingTemplateResponse>` | **Rencana (belum tersedia)** |

`UpsertOnboardingTemplateRequest`: `templateCode`, `templateName`, `templateLayer`, `organizationUnitId`,
`targetCompletionDays`, `description`, `tasks[]` (`id`, `taskCode`, `taskName`, `taskCategory`,
`description`, `responsibleRole`, `itemKind`, `automaticSource`, `certificationTypeId`, `isRequired`,
`dueDayOffset`), `rowVersion`. Butir sistem dikirim balik apa adanya; perubahan jenis, penanggung jawab,
atau tanda wajibnya ditolak (`VR-ONB-22`).

### 15.3 Corporate / Human Resource / Lifecycle Management / Employment Contract

Base URL: `api/v1/corporate/human-resource/lifecycle-management/employment-contracts`

`{id}` adalah `Id` baris `WfpContractHistory`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kontrak | `WorkforceEmploymentContract : Read` | `EmploymentContractPagedQuery` (`organizationUnitId`, `contractStatus`, `contractTypeId`, `expiringWithinDays`, `historyType`, `missingDocument`, `search`) | `ApiResponse<PagedResult<EmploymentContractListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail kontrak: rantai kontrak, masa percobaan, keputusan, koreksi | `WorkforceEmploymentContract : Read` | — | `ApiResponse<EmploymentContractResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/print-data` | Data cetak draf kontrak, dari tawaran yang disetujui | `WorkforceEmploymentContract : Read` | — | `ApiResponse<EmploymentContractPrintResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/mark-signed` | Menandai kontrak sudah ditandatangani | `WorkforceEmploymentContract : MarkSigned` | `multipart`: `file` (wajib), `signedDate`, `rowVersion` | `ApiResponse<EmploymentContractResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/document` | Mengunggah scan kontrak data awal yang menyusul | `WorkforceEmploymentContract : UploadDocument` | `multipart`: `file`, `rowVersion` | `ApiResponse<EmploymentContractResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | Mengunduh scan kontrak | `WorkforceEmploymentContract : Read` | — | Berkas | **Rencana (belum tersedia)** |
| `GET` | `/{id}/fixed-term-check` | Pratinjau total PKWT bila diperpanjang sekian bulan | `WorkforceEmploymentContract : Read` | `extensionMonths` | `ApiResponse<FixedTermCheckResponse>` (`totalYears`, `totalMonths`, `allowed`, `maximumExtensionMonths`) | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decisions` | Mengajukan keputusan Perpanjang, Angkat jadi Pegawai Tetap, atau Tidak Diperpanjang; memulai `T16` | `WorkforceContractDecision : Submit` | `SubmitContractDecisionRequest` (`decisionType`, `extensionMonths`, `reasonText`, `contractRowVersion`) | `ApiResponse<ContractDecisionResponse>` `201` | **Rencana (belum tersedia)** |
| `GET` | `/decisions/{decisionId}` | Detail keputusan | `WorkforceContractDecision : Read` | — | `ApiResponse<ContractDecisionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/decisions/{decisionId}` | Mengubah keputusan yang dikembalikan, lalu mengajukan ulang | `WorkforceContractDecision : Submit` | `SubmitContractDecisionRequest` + `rowVersion` | `ApiResponse<ContractDecisionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/decisions/{decisionId}/withdraw` | Menarik keputusan sebelum diputus | `WorkforceContractDecision : Submit` | `WithdrawRequest` (`reason`, `rowVersion`) | `ApiResponse<ContractDecisionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/decisions/{decisionId}/notification` | Mencatat tanggal pemberitahuan Tidak Diperpanjang kepada pegawai | `WorkforceContractDecision : RecordNotification` | `RecordNotificationRequest` (`employeeNotifiedOn`, `rowVersion`) | `ApiResponse<ContractDecisionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/corrections` | Mengajukan koreksi kontrak; memulai `T17` | `WorkforceEmploymentContract : RequestCorrection` | `multipart`: `afterContractNumber`, `afterContractTypeId`, `afterStartDate`, `afterEndDate`, `reasonText`, `file`, `contractRowVersion` | `ApiResponse<ContractCorrectionResponse>` `201` | **Rencana (belum tersedia)** |
| `GET` | `/corrections/{correctionId}` | Detail koreksi beserta nilai sebelum dan sesudah | `WorkforceEmploymentContract : Read` | — | `ApiResponse<ContractCorrectionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/corrections/{correctionId}` | Mengubah koreksi yang dikembalikan, lalu mengajukan ulang | `WorkforceEmploymentContract : RequestCorrection` | sama dengan pengajuan + `rowVersion` | `ApiResponse<ContractCorrectionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/corrections/{correctionId}/withdraw` | Menarik koreksi sebelum diputus | `WorkforceEmploymentContract : RequestCorrection` | `WithdrawRequest` | `ApiResponse<ContractCorrectionResponse>` | **Rencana (belum tersedia)** |

**Tidak ada** `PUT` atas isi kontrak. Isi draf kontrak pertama berubah hanya lewat revisi tawaran di
04 (`HRD-DEC-084` butir 6); isi kontrak yang sudah ditandatangani berubah hanya lewat koreksi `T17`.

`EmploymentContractPrintResponse`: `employeeName`, `positionName`, `organizationUnitName`,
`contractTypeName`, `contractNumber`, `startDate`, `endDate`, `probationEndDate`, `basicSalary`
(`null` bila tidak berhak), `amountVisible`, `offerNumber`.

### 15.4 Corporate / Human Resource / Lifecycle Management / Probation

Base URL: `api/v1/corporate/human-resource/lifecycle-management/probations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar masa percobaan | `WorkforceProbation : Read` | `ProbationPagedQuery` (`organizationUnitId`, `probationStatus`, `dueOnly`, `search`) | `ApiResponse<PagedResult<ProbationListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail masa percobaan beserta seluruh penilaiannya | `WorkforceProbation : Read` | — | `ApiResponse<ProbationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/assessments` | Mengajukan hasil Lulus atau Tidak Lulus; memulai `T15` | `WorkforceProbation : Submit` | `multipart`: `result`, `reasonText`, `file` (wajib bila `Fail`), `probationRowVersion` | `ApiResponse<ProbationAssessmentResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/assessments/{assessmentId}` | Mengubah penilaian yang dikembalikan, lalu mengajukan ulang | `WorkforceProbation : Submit` | sama dengan pengajuan + `rowVersion` | `ApiResponse<ProbationAssessmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/assessments/{assessmentId}/withdraw` | Menarik penilaian sebelum diputus | `WorkforceProbation : Submit` | `WithdrawRequest` | `ApiResponse<ProbationAssessmentResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/assessments/{assessmentId}/evidence` | Mengunduh lampiran penilaian | `WorkforceProbation : Read` | — | Berkas | **Rencana (belum tersedia)** |

**Tidak ada** endpoint perpanjangan masa percobaan (`VR-ONB-59`).

### 15.5 Corporate / Human Resource / Lifecycle Management / Clinical Preceptorship

Base URL: `api/v1/corporate/human-resource/lifecycle-management/clinical-preceptorships`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pendampingan | `WorkforcePreceptorship : Read` | `PreceptorshipPagedQuery` (`organizationUnitId`, `preceptorshipStatus`, `preceptorWorkforceProfileId`) | `ApiResponse<PagedResult<PreceptorshipListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail pendampingan beserta riwayat preceptor | `WorkforcePreceptorship : Read` | — | `ApiResponse<PreceptorshipResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/preceptor` | Mengganti preceptor | `WorkforcePreceptorship : Manage` | `ChangePreceptorRequest` (`preceptorWorkforceProfileId`, `changeReason`, `rowVersion`) | `ApiResponse<PreceptorshipResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/declare-independent` | Menyatakan Siap Mandiri | `WorkforcePreceptorship : Manage` | `multipart`: `independentOn`, `assessmentNote`, `file` (wajib), `rowVersion` | `ApiResponse<PreceptorshipResponse>` | **Rencana (belum tersedia)** |

### 15.6 Corporate / Human Resource / Lifecycle Management / Initial Contract Batch

Base URL: `api/v1/corporate/human-resource/lifecycle-management/initial-contract-batches`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/template` | Mengunduh templat berisi pegawai aktif yang belum punya kontrak | `WorkforceInitialContractBatch : Read` | `hospitalSiteId` | Berkas `xlsx` | **Rencana (belum tersedia)** |
| `POST` | `/validate` | Memeriksa baris sebelum disimpan | `WorkforceInitialContractBatch : Read` | `List<InitialContractBatchRowRequest>` | `ApiResponse<List<InitialContractBatchRowValidationResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar batch | `WorkforceInitialContractBatch : Read` | `PagedQuery` | `ApiResponse<PagedResult<InitialContractBatchListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail batch beserta baris dan hasil pemeriksaannya | `WorkforceInitialContractBatch : Read` | — | `ApiResponse<InitialContractBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menyimpan batch baru | `WorkforceInitialContractBatch : Create` | `UpsertInitialContractBatchRequest` (`hospitalSiteId`, `rows[]`) | `ApiResponse<InitialContractBatchResponse>` `201` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah batch `Draft` atau `Returned` | `WorkforceInitialContractBatch : Update` | `UpsertInitialContractBatchRequest` + `rowVersion` | `ApiResponse<InitialContractBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan ke `T18` | `WorkforceInitialContractBatch : Submit` | `SubmitDocumentRequest` | `ApiResponse<InitialContractBatchResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan batch `Draft` atau `Returned` | `WorkforceInitialContractBatch : Cancel` | `CancelDocumentRequest` | `ApiResponse<InitialContractBatchResponse>` | **Rencana (belum tersedia)** |

`InitialContractBatchRowRequest`: `workforceProfileId`, `contractTypeId`, `contractNumber`, `startDate`,
`endDate`, `fixedTermSeriesStartDate`.

### 15.7 Corporate / Human Resource / Lifecycle Management / Onboarding Access Request

Base URL: `api/v1/corporate/human-resource/lifecycle-management/onboarding-access-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permintaan akun dan akses kerja | `WorkforceOnboardingAccessRequest : Read` | `requestType`, `requestStatus`, `hospitalSiteId` | `ApiResponse<PagedResult<OnboardingAccessRequestResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/complete` | Menandai akses kerja sudah diberikan | `WorkforceOnboardingAccessRequest : Complete` | `CompleteAccessRequest` (`completionNote`) | `ApiResponse<OnboardingAccessRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/retry` | Mengulang pembuatan akun yang gagal | `WorkforceOnboardingAccessRequest : Retry` | — | `ApiResponse<OnboardingAccessRequestResponse>` | **Rencana (belum tersedia)** |

### 15.8 Corporate / Human Resource / Lifecycle Management / Lifecycle Worklist

Base URL: `api/v1/corporate/human-resource/lifecycle-management/worklist`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar Perlu Tindakan bagi pemanggil, sesuai perannya dan cakupan unitnya | `WorkforceLifecycleWorklist : Read` | `WorklistQuery` (`category`, `organizationUnitId`, `page`, `pageSize`) | `ApiResponse<PagedResult<LifecycleWorklistItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/counts` | Hitungan per kategori, untuk penanda di menu | `WorkforceLifecycleWorklist : Read` | — | `ApiResponse<LifecycleWorklistCountResponse>` | **Rencana (belum tersedia)** |

`category`: `ContractExpiring`, `ContractEscalated`, `ContractLapsed`, `ProbationDue`,
`NonRenewalNotificationPending`, `FirstContractUnsigned`, `LateStart`, `LegacyContractDocumentMissing`,
`ContractNotRecorded` (`02-backend-architecture.md` 18.9.5). Setiap butir membawa `targetType` dan
`targetId` agar layar dapat membuka dokumen yang tepat.

### 15.9 Corporate / Human Resource / Lifecycle Management / Lifecycle Setting

Base URL: `api/v1/corporate/human-resource/lifecycle-management/lifecycle-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca pengaturan satu rumah sakit | `WorkforceLifecycleSetting : Read` | `hospitalSiteId` | `ApiResponse<LifecycleSettingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengubah pengaturan | `WorkforceLifecycleSetting : Update` | `UpdateLifecycleSettingRequest` (`hospitalSiteId`, `contractReminderDays`, `contractEscalationDays`, `probationReminderDays`, `lateStartFlagDays`, `rowVersion`) | `ApiResponse<LifecycleSettingResponse>` | **Rencana (belum tersedia)** |

### 15.10 Kontrak baca internal untuk sub-modul lain

Bukan endpoint HTTP. `HrdEmploymentReadinessReadService` dipanggil langsung oleh service 06 di backend
yang sama (`INT-HR-31`):

| Metode | Masukan | Keluaran |
| --- | --- | --- |
| `GetSchedulingEligibilityAsync` | `workforceProfileIds[]`, `date` | Per pegawai: `eligibility` (`Eligible`, `NotReady`, `ContractHold`), `isAccompanied`, `preceptorName`, `hasCredentialWarning`, `contractNotRecorded`, `reasonText` |

### 15.11 Perubahan pada grup yang sudah ada — Corporate / Human Resource / Workforce Core / Contract History

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/contract-histories`

| Method | Path | Keadaan hari ini | Sesudah `S-C4-ONB` |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata`, `/summary`, `/`, `/{id}` | Tersedia | **Tidak berubah** |
| `POST` | `/` | Tersedia | Menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE` (`VR-ONB-32`) |
| `PUT` | `/{id}` | Tersedia | Menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE` |
| `PATCH` | `/{id}/status` | Tersedia | Menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE` |
| `PATCH` | `/{id}/current` | Tersedia | Menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE` |
| `DELETE` | `/{id}` | Tersedia | Menjawab `409` `CONTRACT_MANAGED_BY_LIFECYCLE` |

Dampak kompatibilitas: **memutus** bagi satu consumer, tab riwayat kontrak editor workforce
(`HRD-TF-043`). Tab itu diubah menjadi baca saja dengan tautan ke layar Kontrak Kerja.

**Grup master pegawai (`EmployeeController`)** — `PUT` menjawab `409` `EMPLOYMENT_FIELDS_MANAGED_BY_LIFECYCLE`
bila request mengubah `employmentTypeId`, `employmentStatusId`, `joinDate`, `contractEndDate`, atau
`probationEndDate` untuk pegawai yang sudah punya baris kontrak (`VR-ONB-33`). Isian lain tidak
berubah.

### 15.12 Perubahan pada grup `S-D2` — `[Tags("RecruitmentManagement - Offers")]` (`HRD-DEC-095`)

| Method | Path | Perubahan |
| --- | --- | --- |
| `POST` | `/` | `CreateJobOfferRequest` mendapat `contractTypeId` (wajib) dan `contractDurationMonths` (wajib bila jenis kontrak mensyaratkan tanggal berakhir) |
| `GET` | `/`, `/{id}` | Response memuat `contractTypeId`, `contractTypeName`, `contractDurationMonths`, `contractEndDate` (dihitung) |
| `POST` | `/{id}/approve` | Tidak berubah; penyetuju melihat jenis dan lama kontrak pada detail |

Endpoint revisi tawaran yang sudah dirancang `S-D2` dipakai juga untuk mengubah jenis atau lama
kontrak sebelum kontrak pertama ditandatangani.

### 15.13 Kode status khusus bagian ini

| Kode | Arti bagi pengguna | Contoh |
| --- | --- | --- |
| `200` | Berhasil | Butir dicentang |
| `201` | Dokumen baru tersimpan | Keputusan kontrak diajukan |
| `400` | Isian tidak lengkap atau formatnya salah | Alasan batal bergabung kosong; berkas bukan PDF/JPG/PNG |
| `403` | Tidak punya hak akses untuk tindakan ini | Kepala Unit mencoba menyatakan siap kerja |
| `404` | Data tidak ditemukan atau di luar cakupan unit | Kepala Farmasi membuka kasus pegawai ICU |
| `405` | Tindakan memang tidak tersedia | Memperpanjang masa percobaan |
| `409` | Bertabrakan dengan data lain atau versi usang | Keputusan kontrak kedua untuk kontrak yang sama; riwayat kontrak 02 diubah langsung |
| `422` | Ditolak aturan bisnis | Siap kerja saat masih ada butir wajib; perpanjangan melewati 5 tahun |


---

## 16. Slice `S-B4` — Penjadwalan, Roster, dan On-Call, sub-modul 06 (`last_changed_in` `v10`, 4 November 2026)

| Field | Value |
| --- | --- |
| Status | `draft` — Siap diajukan untuk approval pengguna |
| `contract_version` | `v10` |
| `input_revision` — decision log | `00-interview-decisions.md` revision `20`, bagian 36 (`HRD-DEC-097` s.d. `HRD-DEC-103`) |
| Tanggal | 2026-11-04 |

### 16.1 Ringkasan endpoint `S-B4`

| Tag grup | Method | Path | Status implementasi | Kegunaan utama |
| --- | :---: | --- | :---: | --- |
| `[Tags("HR Scheduling - Roster Period")]` | `GET` | `/api/corporate/human-resource/scheduling/roster-periods` | `Tersedia` | Mendapatkan daftar periode roster unit |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods` | `Tersedia (diperbarui)` | Membuat draft periode roster baru per unit & auto populate staf |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/copy-previous` | `Rencana (belum tersedia)` | Menyalin seluruh jadwal shift dari periode bulan sebelumnya (`HRD-DEC-098`) |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/apply-pattern` | `Rencana (belum tersedia)` | Menerapkan pola shift berulang ke pegawai / kelompok |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/validate` | `Tersedia (diperbarui)` | Memvalidasi jadwal: evaluasi `B1`–`B7` dan `W1`–`W7` |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/publish` | `Tersedia (diperbarui)` | Menerbitkan roster bulanan dengan validasi ulang otomatis (`HRD-DEC-098`) |
| `[Tags("HR Scheduling - Roster Period")]` | `POST` | `/api/corporate/human-resource/scheduling/roster-periods/{id}/close` | `Tersedia (diperbarui)` | Mengunci mutlak periode roster oleh HRD Payroll (`HRD-DEC-102`) |
| `[Tags("HR Scheduling - Shift Assignment & Replacement")]` | `POST` | `/api/corporate/human-resource/scheduling/shift-assignments/{id}/replace` | `Tersedia (diperbarui)` | Eksekusi Ganti Petugas langsung pasca-terbit dengan alasan wajib (`HRD-DEC-101`) |
| `[Tags("HR Scheduling - Shift Assignment & Replacement")]` | `PUT` | `/api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct` | `Rencana (belum tersedia)` | Koreksi mundur jadwal maksimal 3 hari ke belakang (`HRD-DEC-102`) |
| `[Tags("HR Scheduling - On-Call Management")]` | `GET` | `/api/corporate/human-resource/scheduling/on-call-assignments` | `Tersedia` | Daftar penugasan siaga dokter & staf |
| `[Tags("HR Scheduling - On-Call Management")]` | `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments` | `Tersedia` | Membuat penugasan siaga baru |
| `[Tags("HR Scheduling - On-Call Management")]` | `POST` | `/api/corporate/human-resource/scheduling/on-call-assignments/{id}/call-in` | `Rencana (belum tersedia)` | Mencatat pemanggilan darurat cito (*call-in*) & waktu tiba SLA (`HRD-DEC-103`) |
| `[Tags("HR Scheduling - Settings")]` | `GET` | `/api/corporate/human-resource/scheduling/settings` | `Rencana (belum tersedia)` | Membaca konfigurasi aturan waktu kerja korporat |
| `[Tags("HR Scheduling - Settings")]` | `PUT` | `/api/corporate/human-resource/scheduling/settings` | `Rencana (belum tersedia)` | Memperbarui parameter aturan jeda 11 jam, toleransi 3 hari, dll. |

---

### 16.2 Spesifikasi Detail Endpoint

#### `[Tags("HR Scheduling - Roster Period")]`

##### 1. Salin Periode Lalu
- **Path:** `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/copy-previous`
- **Deskripsi:** Menyalin seluruh matriks shift dari periode roster bulan sebelumnya ke periode target draft.
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Create")]`
- **Request Body:**
  ```json
  {
    "sourceRosterPeriodId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "success": true,
    "copiedAssignmentsCount": 504,
    "message": "Berhasil menyalin 504 jadwal dinas dari periode Januari 2027"
  }
  ```

##### 2. Validasi Jadwal Roster
- **Path:** `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/validate`
- **Deskripsi:** Mengevaluasi seluruh penugasan terhadap aturan memblokir (`B1`–`B7`) dan peringatan (`W1`–`W7`).
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Read")]`
- **Response 200 OK:**
  ```json
  {
    "rosterPeriodId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "isPublishable": false,
    "blockingConflicts": [
      {
        "code": "B1",
        "employeeId": "d3b07384-d113-40bf-9c74-2977508ffb8c",
        "employeeName": "Budi Santoso",
        "shiftDate": "2027-02-08",
        "message": "Budi dijadwalkan dua shift yang bertabrakan pada 8 Februari 2027"
      },
      {
        "code": "B7",
        "employeeId": "e2c07384-d113-40bf-9c74-2977508ffb8d",
        "employeeName": "Rina Wulandari",
        "shiftDate": "2027-02-18",
        "message": "Rina berstatus Didampingi dan tidak boleh berdinas pada shift tanpa perawat Mandiri"
      }
    ],
    "warnings": [
      {
        "code": "W1",
        "employeeId": "a1b07384-d113-40bf-9c74-2977508ffb8a",
        "employeeName": "Tono Haryanto",
        "shiftDate": "2027-02-06",
        "message": "Jeda istirahat Tono antara Siang 5 Feb dan Pagi 6 Feb hanya 10 jam (setelan: 11 jam)",
        "selectedReason": "Permintaan Pegawai"
      },
      {
        "code": "W6",
        "shiftDate": "2027-02-10",
        "shiftCode": "Malam",
        "message": "Jumlah perawat pada Shift Malam 10 Feb hanya 2 orang (target kuota: 3 orang)",
        "selectedReason": null
      }
    ]
  }
  ```

##### 3. Terbitkan Roster Bulanan
- **Path:** `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/publish`
- **Deskripsi:** Menerbitkan roster secara mandiri oleh Kepala Unit setelah validasi ulang otomatis lolos (`HRD-DEC-098`).
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Publish")]`
- **Request Body:**
  ```json
  {
    "warningReasons": [
      {
        "warningCode": "W1",
        "employeeId": "a1b07384-d113-40bf-9c74-2977508ffb8a",
        "shiftDate": "2027-02-06",
        "reasonCategory": "Permintaan Pegawai",
        "notes": "Tono mengajukan dinas maju untuk keperluan keluarga"
      },
      {
        "warningCode": "W6",
        "shiftDate": "2027-02-10",
        "shiftCode": "Malam",
        "reasonCategory": "Kekurangan Tenaga",
        "notes": "BOR ICU diperkirakan < 40%"
      }
    ]
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "rosterPeriodId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "Published",
    "publishedAt": "2027-01-25T09:30:00Z",
    "publishedBy": "Sari (Kepala Ruangan ICU)",
    "message": "Roster ICU Februari 2027 berhasil diterbitkan."
  }
  ```
- **Response 422 Unprocessable Entity:** Dikembalikan jika masih ada pelanggaran `B1`–`B7` atau masih ada peringatan `W` yang belum memiliki alasan.

##### 4. Kunci / Tutup Periode Roster
- **Path:** `POST /api/corporate/human-resource/scheduling/roster-periods/{id}/close`
- **Deskripsi:** Menutup periode roster secara mutlak oleh tim Payroll saat cut-off penggajian (`HRD-DEC-102`).
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Close")]`
- **Response 200 OK:** Status berubah menjadi `Closed`, seluruh jadwal terkunci *read-only*.

---

#### `[Tags("HR Scheduling - Shift Assignment & Replacement")]`

##### 5. Ganti Petugas Langsung Pasca-Terbit
- **Path:** `POST /api/corporate/human-resource/scheduling/shift-assignments/{id}/replace`
- **Deskripsi:** Kepala Unit menunjuk pengganti darurat pada shift terbit dengan alasan wajib, langsung efektif (`HRD-DEC-101`).
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Update")]`
- **Request Body:**
  ```json
  {
    "replacementEmployeeId": "a1b07384-d113-40bf-9c74-2977508ffb8a",
    "reasonCategory": "Sakit Mendadak",
    "notes": "Budi sakit demam tinggi 2 jam sebelum dinas malam"
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "shiftAssignmentId": "8f3b7384-d113-40bf-9c74-2977508ffb8f",
    "rosterPeriodStatus": "Published",
    "isManualOverride": true,
    "assignedToEmployeeId": "a1b07384-d113-40bf-9c74-2977508ffb8a",
    "assignedToEmployeeName": "Tono Haryanto",
    "originalEmployeeId": "d3b07384-d113-40bf-9c74-2977508ffb8c",
    "originalEmployeeName": "Budi Santoso",
    "effectiveImmediately": true,
    "notificationsDispatched": ["Budi Santoso", "Tono Haryanto", "Pak Dodi (Kepala Bidang)"]
  }
  ```

##### 6. Koreksi Mundur Jadwal
- **Path:** `PUT /api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct`
- **Deskripsi:** Mengoreksi jadwal tanggal yang sudah lewat maksimal 3 hari ke belakang (`HRD-DEC-102`).
- **Hak Akses:** `[AccessPermission("HrdScheduling", "Update")]`
- **Request Body:**
  ```json
  {
    "newShiftId": "4c2b7384-d113-40bf-9c74-2977508ffb8e",
    "reasonCategory": "Koreksi Administratif Terlambat",
    "notes": "Lupa input penggantian dinas tanggal 10 Feb"
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "shiftAssignmentId": "8f3b7384-d113-40bf-9c74-2977508ffb8f",
    "isRetroactive": true,
    "daysBack": 2,
    "attendanceReevaluationTriggered": true,
    "message": "Jadwal berhasil dikoreksi dan evaluasi ulang absensi telah dijalankan."
  }
  ```

---

#### `[Tags("HR Scheduling - On-Call Management")]`

##### 7. Pencatatan Pemanggilan Siaga (*Call-In*) & Evaluasi SLA
- **Path:** `POST /api/corporate/human-resource/scheduling/on-call-assignments/{id}/call-in`
- **Deskripsi:** Mencatat pemanggilan darurat cito dokter/staf siaga aktif dan waktu kedatangannya (`HRD-DEC-103`).
- **Hak Akses:** `[AccessPermission("HrdOnCall", "Update")]`
- **Request Body:**
  ```json
  {
    "callInTime": "2027-02-16T02:00:00Z",
    "arrivalTime": "2027-02-16T02:25:00Z",
    "calledByUnitId": "7e1b7384-d113-40bf-9c74-2977508ffb8b",
    "callNotes": "Konfirmasi hasil kritis tes darah troponin I"
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "onCallAssignmentId": "5a2b7384-d113-40bf-9c74-2977508ffb85",
    "employeeName": "dr. Dodi, Sp.PK",
    "status": "Active",
    "expectedResponseMinutes": 30,
    "actualResponseMinutes": 25,
    "isSlaMet": true,
    "overtimeHandoffDispatched": true,
    "payrollAllowanceCalculated": true,
    "message": "Pemanggilan berhasil dicatat. Respons 25 menit (Memenuhi SLA 30 menit)."
  }
  ```

---

#### `[Tags("HR Scheduling - Settings")]`

##### 8. Pengaturan Parameter Waktu Kerja
- **Path:** `PUT /api/corporate/human-resource/scheduling/settings`
- **Deskripsi:** HR Admin memperbarui setelan aturan kerja korporat (`HRD-DEC-099`, `HRD-DEC-102`).
- **Hak Akses:** `[AccessPermission("HrdSchedulingSetting", "Manage")]`
- **Request Body:**
  ```json
  {
    "minRestHoursBetweenShifts": 11,
    "maxConsecutiveWorkDays": 6,
    "maxConsecutiveNightShifts": 2,
    "retroactiveCorrectionWindowDays": 3,
    "restHoursEscalatedToBlock": false
  }
  ```
- **Response 200 OK:**
  ```json
  {
    "success": true,
    "updatedAt": "2026-11-04T13:50:00Z",
    "message": "Pengaturan kebijakan penjadwalan berhasil diperbarui."
  }
  ```


---

## 17. Bagian `S-B1` — Kontrak API Administrasi Kehadiran dan Presensi Staf Rumah Sakit (Sub-modul 07, `last_changed_in` `v11`, 4 November 2026)

### 17.1 Spesifikasi Endpoint RESTful Bergaya Swagger

#### `[Tags("HR Attendance - Daily Monitoring")]`
*Kelompok endpoint untuk pemantauan presensi harian staf rumah sakit, penelusuran log mentah biometrik, dan kalkulasi kehadiran.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/daily` | Query daftar presensi harian per unit dan tanggal | `[AccessPermission("AttendanceDaily", "View")]` | Query: `date`, `unitId`, `status`, `page`, `pageSize` | `PagedResult<AttendanceDailySummaryDto>` | `200`, `400`, `401`, `403` |
| `GET` | `/api/v1/corporate/human-resource/attendance/daily/{id}` | Detail rekaman presensi harian dan pengecualian | `[AccessPermission("AttendanceDaily", "View")]` | Path: `id` (GUID) | `AttendanceDailyDetailDto` | `200`, `401`, `403`, `404` |
| `GET` | `/api/v1/corporate/human-resource/attendance/raw-logs` | Penelusuran riwayat mentah tap biometrik (*immutable drawer*) | `[AccessPermission("AttendanceDaily", "ViewRawLog")]` | Query: `workforceId`, `date` | `List<AttendanceRawLogDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/attendance/daily/recalculate` | Memicu hitung ulang kehadiran harian secara massal/manual | `[AccessPermission("AttendanceDaily", "Manage")]` | Body: `{ date, unitId? }` | `BatchRecalculationResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Attendance - Corrections")]`
*Kelompok endpoint untuk pengajuan koreksi presensi mandiri ESS dan kuasa HR on-behalf per `HRD-DEC-028`.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/corrections` | Daftar permohonan koreksi presensi | `[AccessPermission("AttendanceCorrection", "View")]` | Query: `status`, `unitId`, `page`, `pageSize` | `PagedResult<AttendanceCorrectionDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/attendance/corrections` | Mengajukan koreksi presensi (mandiri atau on-behalf) | `[AccessPermission("AttendanceCorrection", "Create")]` | Body: `CreateAttendanceCorrectionCommand` | `AttendanceCorrectionResultDto` | `201`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/corrections/{id}/approve` | Menyetujui koreksi presensi oleh atasan langsung | `[AccessPermission("AttendanceCorrection", "Approve")]` | Body: `{ notes? }` | `AttendanceCorrectionResultDto` | `200`, `400`, `401`, `403`, `404`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/corrections/{id}/reject` | Menolak permohonan koreksi presensi | `[AccessPermission("AttendanceCorrection", "Approve")]` | Body: `{ reason: string }` | `AttendanceCorrectionResultDto` | `200`, `400`, `401`, `403`, `404`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/corrections/{id}/apply` | Menerapkan koreksi dan memicu hitung ulang jam kerja | `[AccessPermission("AttendanceCorrection", "Apply")]` | Path: `id` | `AttendanceDailyDetailDto` | `200`, `401`, `403`, `404`, `409` |

#### `[Tags("HR Attendance - Early Leave Permission")]`
*Kelompok endpoint perizinan pulang cepat administratif non-deductible per `HRD-DEC-029` dan `HRD-DEC-107`.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/early-leave` | Daftar permohonan izin pulang cepat | `[AccessPermission("EarlyLeavePermission", "View")]` | Query: `date`, `status`, `unitId` | `List<EarlyLeavePermissionDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/attendance/early-leave` | Mengajukan izin pulang cepat darurat keluarga/dinas | `[AccessPermission("EarlyLeavePermission", "Create")]` | Body: `CreateEarlyLeavePermissionCommand` | `EarlyLeavePermissionDto` | `201`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/early-leave/{id}/approve` | Menyetujui izin pulang cepat (menandai Waived) | `[AccessPermission("EarlyLeavePermission", "Approve")]` | Body: `{ notes? }` | `EarlyLeavePermissionDto` | `200`, `401`, `403`, `404`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/early-leave/{id}/reject` | Menolak izin pulang cepat | `[AccessPermission("EarlyLeavePermission", "Approve")]` | Body: `{ reason: string }` | `EarlyLeavePermissionDto` | `200`, `401`, `403`, `404`, `409` |

#### `[Tags("HR Attendance - Official Duty")]`
*Kelompok endpoint surat tugas dinas luar, ambulans rujukan, kurir lab, dan home care per `HRD-DEC-109`.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/official-duty` | Daftar penugasan dinas luar rumah sakit | `[AccessPermission("OfficialDuty", "View")]` | Query: `startDate`, `endDate`, `status` | `PagedResult<OfficialDutyDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/attendance/official-duty` | Membuat permohonan dinas luar (pre/post-approved) | `[AccessPermission("OfficialDuty", "Create")]` | Body: `CreateOfficialDutyCommand` | `OfficialDutyDto` | `201`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/official-duty/{id}/approve` | Menyetujui surat tugas dinas luar | `[AccessPermission("OfficialDuty", "Approve")]` | Body: `{ notes? }` | `OfficialDutyDto` | `200`, `401`, `403`, `404`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/official-duty/{id}/check-in` | Rekam tap dinas luar via mobile (GPS & foto) | `[AccessPermission("OfficialDuty", "CheckIn")]` | Body: `{ latitude, longitude, photoUrl }` | `OfficialDutyCheckInResultDto` | `200`, `400`, `401`, `403` |

#### `[Tags("HR Attendance - Period Management")]`
*Kelompok endpoint penutupan periode presensi bulanan dan buka kembali darurat per `HRD-DEC-108`.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/periods` | Daftar periode presensi kehadiran bulanan | `[AccessPermission("AttendancePeriod", "View")]` | Query: `year`, `status` | `List<AttendancePeriodDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/close` | Menutup periode kehadiran (guard penolakan pending) | `[AccessPermission("AttendancePeriod", "Close")]` | Path: `id` | `CloseAttendancePeriodResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/attendance/periods/{id}/reopen` | Buka kembali darurat oleh HR Manager & rollback payroll | `[AccessPermission("AttendancePeriod", "Reopen")]` | Body: `{ reason: string }` | `ReopenPeriodResultDto` | `200`, `400`, `401`, `403`, `404`, `409` |

#### `[Tags("HR Attendance - Settings")]`
*Kelompok endpoint konfigurasi parameter waktu kerja presensi per `HRD-DEC-105`.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/attendance/settings` | Mengambil konfigurasi aktif presensi RS | `[AccessPermission("AttendanceSetting", "View")]` | None | `AttendanceSettingDto` | `200`, `401`, `403` |
| `PUT` | `/api/v1/corporate/human-resource/attendance/settings` | Memperbarui parameter toleransi keterlambatan/koreksi | `[AccessPermission("AttendanceSetting", "Manage")]` | Body: `UpdateAttendanceSettingCommand` | `AttendanceSettingDto` | `200`, `400`, `401`, `403` |


---

## 18. Bagian `S-B2` — Kontrak API Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08, `last_changed_in` `v12`, 4 November 2026)

### 18.1 Spesifikasi Endpoint RESTful Bergaya Swagger

#### `[Tags("HR Leave - Balances & Ledgers")]`
*Kelompok endpoint untuk informasi hak cuti tahunan (12 hari), saldo carry-forward (max 6 hari s.d. 30 Juni), riwayat buku besar mutasi, dan penyesuaian saldo resmi HR.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/leave/balances/my` | Mengambil rincian saldo cuti mandiri ESS pegawai (hak 12 hari & carry-forward) | `[AccessPermission("LeaveBalance", "ViewSelf")]` | None | `LeaveBalanceSummaryDto` | `200`, `401` |
| `GET` | `/api/v1/corporate/human-resource/leave/balances/ledger` | Query riwayat mutasi buku besar saldo cuti (*balance ledger*) | `[AccessPermission("LeaveBalance", "ViewSelf")]` | Query: `year`, `leaveTypeId`, `page`, `pageSize` | `PagedResult<LeaveBalanceLedgerDto>` | `200`, `401`, `403` |
| `GET` | `/api/v1/corporate/human-resource/leave/balances/admin` | Monitoring saldo cuti seluruh pegawai rumah sakit oleh HR | `[AccessPermission("LeaveBalance", "Manage")]` | Query: `unitId`, `year`, `search`, `page`, `pageSize` | `PagedResult<EmployeeLeaveBalanceAdminDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/leave/balances/carry-forward/run` | Memicu eksekusi scheduler carry-forward (31 Des) atau kedaluwarsa (30 Jun) | `[AccessPermission("LeaveBalance", "Manage")]` | Body: `{ executionType: "YearEndCarryForward" \| "MidYearForfeiture" }` | `CarryForwardRunResultDto` | `200`, `400`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/leave/balances/adjustment` | Penyesuaian saldo resmi HR berdokumen audit (`HRD-DEC-115`) | `[AccessPermission("LeaveAdjustment", "Execute")]` | Body: `CreateLeaveAdjustmentCommand` | `LeaveAdjustmentResultDto` | `201`, `400`, `401`, `403` |

#### `[Tags("HR Leave - Requests & ESS")]`
*Kelompok endpoint pengajuan cuti mandiri ESS, kalkulasi hari kerja efektif, opsi cuti setengah hari (0.5 hari), dan pembatalan sebelum cuti.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/leave/requests/my` | Daftar permohonan cuti aktif dan historis pegawai | `[AccessPermission("LeaveRequest", "ViewSelf")]` | Query: `status`, `year`, `page`, `pageSize` | `PagedResult<LeaveRequestSummaryDto>` | `200`, `401` |
| `GET` | `/api/v1/corporate/human-resource/leave/requests/{id}` | Detail permohonan cuti, status verifikasi surat sakit, dan timeline | `[AccessPermission("LeaveRequest", "View")]` | Path: `id` (GUID) | `LeaveRequestDetailDto` | `200`, `401`, `403`, `404` |
| `POST` | `/api/v1/corporate/human-resource/leave/requests` | Mengajukan permohonan cuti (mendukung half-day 0.5 hari & upload surat dokter) | `[AccessPermission("LeaveRequest", "Create")]` | Body: `CreateLeaveRequestCommand` | `LeaveRequestResultDto` | `201`, `400`, `401`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/requests/{id}/cancel` | Pembatalan cuti disetujui sebelum hari-H (*Pre-Leave Cancel* pulih 100%) | `[AccessPermission("LeaveRequest", "CancelSelf")]` | Body: `{ reason: string }` | `LeaveRequestResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Leave - Approvals & Coverage")]`
*Kelompok endpoint kotak masuk persetujuan atasan dengan kalkulasi peringatan kuota pelayanan (Safe Staffing Coverage Warning).*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/leave/approvals/inbox` | Daftar permohonan bawahan disertai status *Coverage Warning* | `[AccessPermission("LeaveApproval", "View")]` | Query: `unitId`, `status`, `page`, `pageSize` | `PagedResult<LeaveApprovalInboxItemDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/leave/approvals/{id}/approve` | Menyetujui cuti (atau menyetujui dengan mitigasi jika kuota warning) | `[AccessPermission("LeaveApproval", "Approve")]` | Body: `ApproveLeaveRequestCommand` `{ mitigationNotes? }` | `LeaveApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/approvals/{id}/reject` | Menolak permohonan cuti bawahan dengan alasan operasional | `[AccessPermission("LeaveApproval", "Approve")]` | Body: `{ rejectionReason: string }` | `LeaveApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/approvals/{id}/revise` | Meminta revisi tanggal permohonan cuti kepada pemohon | `[AccessPermission("LeaveApproval", "Approve")]` | Body: `{ revisionNotes: string }` | `LeaveApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Leave - Recall Management")]`
*Kelompok endpoint pemanggilan kembali cuti darurat (Leave Recall) saat situasi genting rumah sakit.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/leave/recalls` | Monitoring daftar kasus pemanggilan darurat pegawai cuti | `[AccessPermission("LeaveRecall", "View")]` | Query: `unitId`, `status`, `year` | `PagedResult<LeaveRecallDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/leave/recalls` | Inisiasi pemanggilan darurat oleh Kepala Ruangan / Kepala Unit | `[AccessPermission("LeaveRecall", "Create")]` | Body: `InitiateLeaveRecallCommand` | `LeaveRecallDto` | `201`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/recalls/{id}/approve` | Persetujuan resmi recall oleh HR Manager & pemulihan saldo otomatis | `[AccessPermission("LeaveRecall", "Approve")]` | Body: `{ notes? }` | `LeaveRecallExecutionResultDto` | `200`, `400`, `401`, `403`, `404`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/recalls/{id}/reject` | Menolak usulan pemanggilan darurat oleh HR Manager | `[AccessPermission("LeaveRecall", "Approve")]` | Body: `{ reason: string }` | `LeaveRecallDto` | `200`, `400`, `401`, `403`, `404`, `409` |

#### `[Tags("HR Leave - Reconciliations & Close")]`
*Kelompok endpoint penutupan periode cuti sebelum payroll dan pengamanan immutabilitas status Completed.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/leave/reconciliations/pending` | Daftar cuti pending yang akan direkonsiliasi retrospektif | `[AccessPermission("LeaveReconciliation", "View")]` | Query: `periodYear`, `periodMonth` | `List<PendingLeavePayrollDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/leave/reconciliations/close-period` | Mengunci periode cuti bulanan dan serahkan snapshot ke Payroll | `[AccessPermission("LeaveReconciliation", "Close")]` | Body: `{ periodYear: int, periodMonth: int }` | `CloseLeavePeriodResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/leave/execution/{id}/reverse` | Percobaan pembalikan cuti — **Ditolak HTTP 409 bila status Completed / PayrollClosed** (`HRD-DEC-115`) | `[AccessPermission("LeaveExecution", "Reverse")]` | Body: `{ reason: string }` | `LeaveExecutionResultDto` | `400`, `401`, `403`, `409 Conflict` |


---

## 19. Bagian `S-B3` — Kontrak API Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09, `last_changed_in` `v13`, 4 November 2026)

Bagian ini mendefinisikan spesifikasi antarmuka pemrograman aplikasi (API Endpoint Contract) bergaya Swagger untuk **Sub-modul 09. Overtime Management (Slice `S-B3`)**, menyerap kapabilitas backend `HRD-CAP-08` dan keputusan bisnis `HRD-DEC-116` s.d. `HRD-DEC-121`.

### 19.1 Ikhtisar Grup Endpoint Swagger

```csharp
[Tags("HR Overtime - Plans & ESS Requests")]
[Tags("HR Overtime - Realizations & Cito Claims")]
[Tags("HR Overtime - Biometric Verification & Discrepancies")]
[Tags("HR Overtime - Approvals & Escalations")]
[Tags("HR Overtime - Compensatory Time Off (CTO)")]
[Tags("HR Overtime - Periods, Reconciliation & Payroll Handoff")]
```

---

### 19.2 Spesifikasi Endpoint Detail

#### `[Tags("HR Overtime - Plans & ESS Requests")]`
*Kelompok endpoint pengajuan dan pengelolaan Surat Perintah Kerja Lembur (SPKL) terencana pra-dinas (H-1).*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/plans/my` | Daftar pengajuan SPKL terencana staf aktif | `[AccessPermission("OvertimePlan", "ViewSelf")]` | Query: `status`, `year`, `month`, `page`, `pageSize` | `PagedResult<OvertimePlanSummaryDto>` | `200`, `401` |
| `GET` | `/api/v1/corporate/human-resource/overtime/plans/{id}` | Detail rincian SPKL, daftar nakes pelaksana, dan status persetujuan | `[AccessPermission("OvertimePlan", "View")]` | Path: `id` (GUID) | `OvertimePlanDetailDto` | `200`, `401`, `403`, `404` |
| `POST` | `/api/v1/corporate/human-resource/overtime/plans` | Mengajukan SPKL pra-dinas terencana (H-1, individu atau kelompok tim) | `[AccessPermission("OvertimePlan", "Create")]` | Body: `CreateOvertimePlanCommand` | `OvertimePlanResultDto` | `201`, `400`, `401`, `409` |
| `POST` | `/api/v1/corporate/human-resource/overtime/plans/{id}/cancel` | Membatalkan rencana SPKL sebelum jam dinas lembur terlaksana | `[AccessPermission("OvertimePlan", "CancelSelf")]` | Body: `{ reason: string }` | `OvertimePlanResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Overtime - Realizations & Cito Claims")]`
*Kelompok endpoint pelaporan jam lembur aktual dan klaim darurat medis cito pasca-dinas (H+1).*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/realizations/my` | Daftar riwayat klaim realisasi lembur pribadi staf | `[AccessPermission("OvertimeRealization", "ViewSelf")]` | Query: `category`, `status`, `page`, `pageSize` | `PagedResult<OvertimeRealizationSummaryDto>` | `200`, `401` |
| `GET` | `/api/v1/corporate/human-resource/overtime/realizations/{id}` | Detail klaim lembur aktual, bukti nomor rekam medis cito, dan kalkulasi | `[AccessPermission("OvertimeRealization", "View")]` | Path: `id` (GUID) | `OvertimeRealizationDetailDto` | `200`, `401`, `403`, `404` |
| `POST` | `/api/v1/corporate/human-resource/overtime/realizations` | Pelaporan realisasi dari SPKL atau klaim mandiri darurat cito medis H+1 | `[AccessPermission("OvertimeRealization", "Create")]` | Body: `SubmitOvertimeRealizationCommand` | `OvertimeRealizationResultDto` | `201`, `400`, `401`, `409` |

#### `[Tags("HR Overtime - Biometric Verification & Discrepancies")]`
*Kelompok endpoint verifikasi silang jam lembur terhadap mesin absensi biometrik dan penyelesaian anomali.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/verifications` | Daftar komparasi klaim lembur vs catatan kehadiran biometrik | `[AccessPermission("OvertimeVerification", "View")]` | Query: `date`, `status`, `unitId`, `page`, `pageSize` | `PagedResult<OvertimeVerificationSummaryDto>` | `200`, `401`, `403` |
| `GET` | `/api/v1/corporate/human-resource/overtime/verifications/discrepancies` | Daftar antrean anomali selisih waktu > 30 menit atau lupa tap mesin | `[AccessPermission("OvertimeVerification", "Manage")]` | Query: `discrepancyType`, `unitId` | `List<OvertimeDiscrepancyItemDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/overtime/verifications/{id}/manual-adjust` | Penyesuaian manual durasi sah oleh HR Admin berdasar bukti logbook dinas | `[AccessPermission("OvertimeVerification", "Manage")]` | Body: `AdjustOvertimeVerificationCommand` | `OvertimeVerificationResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/overtime/verifications/{id}/reject-discrepancy` | Menolak klaim lembur anomali yang dinilai fiktif / tidak sah | `[AccessPermission("OvertimeVerification", "Manage")]` | Body: `{ rejectionReason: string }` | `OvertimeVerificationResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Overtime - Approvals & Escalations")]`
*Kelompok endpoint persetujuan operasional ruangan, pencegahan self-approval, dan eskalasi Direktur Pelayanan Medik.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/approvals/inbox` | Kotak masuk persetujuan SPKL bawahan bagi Kepala Ruangan | `[AccessPermission("OvertimeApproval", "View")]` | Query: `status`, `unitId`, `page`, `pageSize` | `PagedResult<OvertimeApprovalInboxItemDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/overtime/approvals/{id}/approve` | Menyetujui SPKL staf (Ditolak HTTP 403 bila menyetujui diri sendiri per `VR-OT-03`) | `[AccessPermission("OvertimeApproval", "Approve")]` | Body: `{ notes?: string }` | `OvertimeApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/overtime/approvals/{id}/reject` | Menolak SPKL bawahan dengan alasan operasional sah | `[AccessPermission("OvertimeApproval", "Approve")]` | Body: `{ rejectionReason: string }` | `OvertimeApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |
| `GET` | `/api/v1/corporate/human-resource/overtime/approvals/medical-director-inbox` | Kotak masuk eskalasi klaim cito darurat yang melampaui kuota PP 35/2021 | `[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]` | Query: `urgency`, `page`, `pageSize` | `PagedResult<MedicalDirectorOvertimeEscalationDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-approve` | Otorisasi khusus Direktur Pelayanan Medik untuk tindakan penyelamatan nyawa | `[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]` | Body: `ApproveMedicalDirectorEscalationCommand` | `OvertimeApprovalResultDto` | `200`, `400`, `401`, `403`, `409` |

#### `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`
*Kelompok endpoint tata kelola hak libur pengganti (CTO) berkedaluwarsa 3 bulan.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/compensatory-leave/my` | Saldo tiket libur pengganti staf aktif dan tanggal kedaluwarsanya | `[AccessPermission("OvertimeCompensatoryLeave", "ViewSelf")]` | Query: `status` | `List<CompensatoryTimeOffItemDto>` | `200`, `401` |
| `GET` | `/api/v1/corporate/human-resource/overtime/compensatory-leave` | Monitoring agregat seluruh tiket libur pengganti staf RS bagi HRD | `[AccessPermission("OvertimeCompensatoryLeave", "Manage")]` | Query: `unitId`, `isExpired`, `page`, `pageSize` | `PagedResult<CompensatoryTimeOffAdminDto>` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/overtime/compensatory-leave/expire-runner` | Menjalankan scheduler eksekusi hangus saldo CTO yang telah melewati 90 hari | `[AccessPermission("OvertimeCompensatoryLeave", "Manage")]` | Body: `{ asOfDate?: string }` | `CtoForfeitureExecutionResultDto` | `200`, `401`, `403` |

#### `[Tags("HR Overtime - Periods, Reconciliation & Payroll Handoff")]`
*Kelompok endpoint cut-off penutupan lembur bulanan, snapshot immutable, dan serah terima ke Payroll.*

| Method | Path | Deskripsi | Otorisasi / Permission | Request Payload | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/corporate/human-resource/overtime/periods/current` | Informasi status periode lembur berjalan dan tanggal batas cut-off | `[AccessPermission("OvertimePeriod", "View")]` | — | `OvertimePeriodStatusDto` | `200`, `401`, `403` |
| `GET` | `/api/v1/corporate/human-resource/overtime/reconciliations/summary` | Rekapitulasi lembur siap bayar dan daftar rapel retrospektif periode lalu | `[AccessPermission("OvertimePeriod", "Manage")]` | Query: `year`, `month` | `OvertimeReconciliationSummaryDto` | `200`, `401`, `403` |
| `POST` | `/api/v1/corporate/human-resource/overtime/periods/{id}/lock-cutoff` | Mengunci snapshot transaksi lembur pada cut-off bulanan (tgl 20) | `[AccessPermission("OvertimePeriod", "Manage")]` | Body: `{ cutoffDate: string }` | `LockOvertimePeriodResultDto` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/v1/corporate/human-resource/overtime/periods/{id}/handoff-payroll` | Menyerahkan paket data rekonsiliasi lembur terkunci ke Sub-modul 10 Payroll | `[AccessPermission("OvertimePeriod", "HandoffPayroll")]` | Body: `{ targetPayrollBatchId: string }` | `OvertimePayrollHandoffResultDto` | `200`, `400`, `401`, `403`, `409` |
