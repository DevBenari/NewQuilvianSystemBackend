# Laporan Perubahan Backend — `BE-HRD-115`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-115` |
| Judul | Controller & Endpoint Manajemen Siklus Periode dan Serah Terima Payroll (`AttendancePeriodController`, `AttendancePayrollHandoffController`) |
| Slice | S-B1 (AttendanceManagement - Presensi dan Kehadiran) |
| Milestone | MVP-23 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-108, API Contract v11 Bagian 17.2, 02-backend-architecture.md Bagian 20, AT-HRD-ATT-16 s.d. AT-HRD-ATT-18 |
| Dependency | `BE-HRD-113`, `BE-HRD-114` ✅ |
| Klasifikasi | `CONTROLLER & ENDPOINT SPECIFICATION (SWAGGER / API CONTRACT V11)` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Dalam integrasi sistem ERP Rumah Sakit, siklus hidup data presensi bulanan harus terhubung secara mulus dan berintegritas tinggi antara modul Presensi (Sub-Modul 07) dan modul Penggajian / Payroll (Sub-Modul 10). Ketidaksesuaian endpoint, tag dokumentasi Swagger yang tidak terstandar, atau tidak adanya endpoint peninjauan riwayat audit pembukaan periode dapat membingungkan pengembang frontend dan tim audit kepatuhan rumah sakit.

Melalui task `BE-HRD-115`, seluruh antarmuka API siklus hidup periode dan integrasi handoff payroll distandarkan secara penuh:
1. **Penataan Tag Swagger Resmi**:
   - `[Tags("HR Attendance - Period Management")]` untuk controller periode.
   - `[Tags("HR Attendance - Payroll Handoff")]` untuk controller serah terima payroll.
2. **Dukungan Rute Ganda RESTful**: Menjamin kompatibilitas mundur (*backward compatibility*) dengan mendukung rute standar modular dan rute versi v1:
   - `/api/corporate/human-resource/attendance/periods` & `/api/v1/corporate/human-resource/attendance/periods`
   - `/api/corporate/human-resource/attendance/payroll-handoff` & `/api/v1/corporate/human-resource/attendance/payroll-handoff`
3. **Penyediaan Endpoint Lengkap**:
   - `POST /close`: Menutup periode ber-guard, mengonversi sisa `IncompletePunch` menjadi `Absent`, dan mengembalikan jumlah rekaman terdampak.
   - `POST /reopen`: Membuka kembali periode oleh HR Manager, memvalidasi alasan min 15 karakter, dan otomatis membatalkan serah terima payroll (`INT-HR-43`).
   - `GET /{id}/reopen-logs`: Mengambil seluruh riwayat log buka kembali periode beserta catatan audit rollback payroll.
   - `POST /execute`: Menghasilkan snapshot rekapitulasi jam kerja riil, jam lembur, dan potongan presensi untuk modul Payroll.

---

## 2. Tabel Spesifikasi Endpoint Bergaya Swagger

### Tag: `[Tags("HR Attendance - Period Management")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/corporate/human-resource/attendance/periods` | Mendapatkan daftar periode presensi terfilter dan terpaginasi | `[AccessPermission("AttendancePeriod", "Read")]` | Query: `AttendancePeriodQueryRequest` | `ApiResponse<AttendancePeriodPagedResponse>` | `200`, `401`, `403` |
| `GET` | `/api/corporate/human-resource/attendance/periods/{id}` | Mengambil detail periode beserta snapshot validasi terakhir | `[AccessPermission("AttendancePeriod", "Read")]` | None | `ApiResponse<AttendancePeriodDetailResponse>` | `200`, `401`, `403`, `404` |
| `GET` | `/api/corporate/human-resource/attendance/periods/{id}/close-preview` | Melihat pratinjau validasi dan daftar issue sebelum penutupan periode | `[AccessPermission("AttendancePeriod", "Read")]` | None | `ApiResponse<AttendancePeriodClosePreviewResponse>` | `200`, `401`, `403`, `404` |
| `POST` | `/api/corporate/human-resource/attendance/periods/{id}/close` | Menutup periode kehadiran, mengeksekusi konversi IncompletePunch ke Absent, dan mengunci presensi | `[AccessPermission("AttendancePeriod", "Close")]` | `CloseAttendancePeriodRequest` (`Reason`) | `ApiResponse<AttendancePeriodActionResponse>` | `200`, `400`, `401`, `403`, `404`, `409` |
| `POST` | `/api/corporate/human-resource/attendance/periods/{id}/reopen` | Buka kembali darurat oleh HR Manager dengan rollback serah terima payroll otomatis | `[AccessPermission("AttendancePeriod", "Reopen")]` | `ReopenAttendancePeriodRequest` (`Reason` min 15 char) | `ApiResponse<AttendancePeriodActionResponse>` | `200`, `400`, `401`, `403`, `404`, `409` |
| `GET` | `/api/corporate/human-resource/attendance/periods/{id}/reopen-logs` | Mengambil riwayat log pembukaan kembali periode darurat dan jejak audit | `[AccessPermission("AttendancePeriod", "Read")]` | None | `ApiResponse<List<AttendancePeriodReopenLogResponse>>` | `200`, `401`, `403` |
| `POST` | `/api/corporate/human-resource/attendance/periods/{id}/cancel` | Membatalkan periode presensi sebelum finalisasi | `[AccessPermission("AttendancePeriod", "Cancel")]` | `CancelAttendancePeriodRequest` (`Reason`) | `ApiResponse<AttendancePeriodActionResponse>` | `200`, `400`, `401`, `403`, `404`, `409` |

### Tag: `[Tags("HR Attendance - Payroll Handoff")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/corporate/human-resource/attendance/payroll-handoff/payroll-runs/{payrollRunId}/summary` | Melihat ringkasan kelayakan data presensi untuk payroll run | `[AccessPermission("AttendancePayrollHandoff", "Read")]` | None | `ApiResponse<AttendancePayrollHandoffSummaryResponse>` | `200`, `401`, `403`, `404` |
| `GET` | `/api/corporate/human-resource/attendance/payroll-handoff/payroll-runs/{payrollRunId}/preview` | Melihat pratinjau snapshot rekapitulasi presensi sebelum dieksekusi | `[AccessPermission("AttendancePayrollHandoff", "Read")]` | Query: `AttendancePayrollHandoffQueryRequest` | `ApiResponse<AttendancePayrollHandoffPreviewPagedResponse>` | `200`, `401`, `403`, `404` |
| `POST` | `/api/corporate/human-resource/attendance/payroll-handoff/payroll-runs/{payrollRunId}/execute` | Mengeksekusi pembentukan snapshot rekap presensi ke modul payroll | `[AccessPermission("AttendancePayrollHandoff", "Execute")]` | `ExecuteAttendancePayrollHandoffRequest` | `ApiResponse<AttendancePayrollHandoffExecutionResponse>` | `200`, `400`, `401`, `403`, `404`, `409` |
| `POST` | `/api/corporate/human-resource/attendance/payroll-handoff/payroll-runs/{payrollRunId}/rollback` | Membatalkan penyerahan presensi sebelum payroll run diproses final | `[AccessPermission("AttendancePayrollHandoff", "Rollback")]` | `RollbackAttendancePayrollHandoffRequest` (`Reason`) | `ApiResponse<AttendancePayrollHandoffRollbackResponse>` | `200`, `400`, `401`, `403`, `404`, `409` |

---

## 3. Hasil Verifikasi

| Komponen Uji | Kriteria Penerimaan | Status |
| :--- | :--- | :---: |
| Tag Swagger Konsisten | Tag Swagger `HR Attendance - Period Management` & `HR Attendance - Payroll Handoff` terpasang | **LULUS** |
| Kompatibilitas Rute Ganda | Endpoint dapat diakses via `/api/...` maupun `/api/v1/...` | **LULUS** |
| Endpoint Reopen Logs | Endpoint `/{id:guid}/reopen-logs` berfungsi mengembalikan riwayat audit | **LULUS** |
| Kompilasi Backend (.NET C#) | Solusi backend terkompilasi 0 error | **LULUS** |
