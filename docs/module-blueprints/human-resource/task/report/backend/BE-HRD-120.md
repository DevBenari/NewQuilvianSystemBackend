# Laporan Perubahan Backend — `BE-HRD-120`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-120` |
| Judul | Controller & DTO Mutasi Saldo Ledger dan Kuota Carry-Forward (`LeaveBalanceLedgerController`) |
| Slice | S-B2 (LeaveManagement - Administrasi Cuti & Izin Pegawai Rumah Sakit) |
| Milestone | MVP-24 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-110, API Contract v12 Bagian 18.1, 02-backend-architecture.md Bagian 21, AT-HRD-LEV-01 s.d. AT-HRD-LEV-04 |
| Dependency | `BE-HRD-118`, `BE-HRD-119` ✅ |
| Klasifikasi | `CONTROLLER & RESTFUL API SPECIFICATION (SWAGGER / API CONTRACT V12)` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Di modul Administrasi Cuti & Izin Rumah Sakit, transparansi saldo cuti bagi pegawai mandiri (Employee Self-Service / ESS) dan kemudahan pengawasan bagi manajemen SDM rumah sakit sangat menentukan tingkat kepuasan kerja staf medis. Staf medis membutuhkan visibilitas instan atas:
1. Berapa sisa saldo cuti tahun berjalan yang dapat diambil.
2. Berapa sisa kuota bawaan tahun lalu (*Carry-Forward*) yang akan segera hangus pada 30 Juni agar tidak terlewat.
3. Berapa hari cuti yang sedang terkunci dalam proses persetujuan Kepala Ruangan (*Reserved Balance*).

Melalui task `BE-HRD-120`, antarmuka API buku besar mutasi cuti distandarkan secara menyeluruh:
1. **Tag Swagger Resmi**: Menggunakan tag standar `[Tags("HR Leave - Balances & Ledgers")]`.
2. **Dukungan Rute Ganda RESTful**: Mendukung endpoint standar `/api/corporate/human-resource/leave/balances` dan versi kompatibilitas `/api/v1/corporate/human-resource/leave/balances`.
3. **Penyediaan Endpoint Lengkap**:
   - `GET /my`: Query saldo mandiri ESS dengan deteksi otomatis user login.
   - `GET /workforce/{id}/summary`: Query saldo pegawai tertentu oleh atasan / HR Admin.
   - `GET /ledger`: Riwayat transaksi buku besar mutasi dengan filter pegawai, jenis cuti, tahun, dan tipe transaksi.
   - `POST /entitlement`: Alokasi manual hak cuti tahunan (12 hari).
   - `POST /adjustment`: Penyesuaian saldo resmi HR berlandaskan SK Direksi.
   - `POST /carry-forward/run-year-end`: Eksekusi manual / runner carry-forward akhir tahun max 6 hari.
   - `POST /carry-forward/run-mid-year-expiry`: Eksekusi manual / runner kedaluwarsa kuota bawaan 30 Juni.

---

## 2. Tabel Spesifikasi Endpoint Bergaya Swagger

### Tag: `[Tags("HR Leave - Balances & Ledgers")]`

| Method | Path | Deskripsi | Otorisasi / Permission | Request Body / Query | Response Model | Status Codes |
| :---: | :--- | :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/corporate/human-resource/leave/balances/my` | Melihat ringkasan saldo cuti mandiri ESS staf login beserta sisa kuota bawaan dan peringatan 30 Juni | `[AccessPermission("LeaveBalanceLedger", "Read")]` | Query: `leaveTypeId`, `year` | `ApiResponse<LeaveBalanceLedgerSummaryDto>` | `200`, `401`, `404` |
| `GET` | `/api/corporate/human-resource/leave/balances/workforce/{id}/summary` | Melihat ringkasan saldo cuti pegawai tertentu oleh HRD / Atasan | `[AccessPermission("LeaveBalanceLedger", "Read")]` | Query: `leaveTypeId`, `year` | `ApiResponse<LeaveBalanceLedgerSummaryDto>` | `200`, `401`, `403`, `404` |
| `GET` | `/api/corporate/human-resource/leave/balances/ledger` | Riwayat transaksi buku besar mutasi saldo cuti terpaginasi | `[AccessPermission("LeaveBalanceLedger", "Read")]` | Query: `TrxLeaveBalanceLedgerQueryRequest` | `ApiResponse<LeaveBalanceLedgerPagedResponse>` | `200`, `401`, `403` |
| `POST` | `/api/corporate/human-resource/leave/balances/entitlement` | Alokasi hak cuti tahunan (12 hari kerja) kepada pegawai | `[AccessPermission("LeaveBalanceLedger", "Create")]` | `PostLeaveEntitlementRequest` | `ApiResponse<LeaveBalanceLedgerItemDto>` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/corporate/human-resource/leave/balances/adjustment` | Penyesuaian saldo resmi HR Manager dengan nomor SK Direksi | `[AccessPermission("LeaveBalanceLedger", "Update")]` | `ManualLeaveAdjustmentRequest` | `ApiResponse<LeaveBalanceLedgerItemDto>` | `200`, `400`, `401`, `403`, `409` |
| `POST` | `/api/corporate/human-resource/leave/balances/carry-forward/run-year-end` | Runner carry-forward akhir tahun 31 Desember max 6 hari kerja | `[AccessPermission("LeaveBalanceLedger", "Update")]` | Query: `sourceYear` | `ApiResponse<CarryForwardRunResultDto>` | `200`, `401`, `403` |
| `POST` | `/api/corporate/human-resource/leave/balances/carry-forward/run-mid-year-expiry` | Runner penghangusan otomatis sisa kuota bawaan per 30 Juni | `[AccessPermission("LeaveBalanceLedger", "Update")]` | Query: `currentYear` | `ApiResponse<CarryForwardRunResultDto>` | `200`, `401`, `403` |

---

## 3. Spesifikasi DTO Response Saldo ESS (`LeaveBalanceLedgerSummaryDto`)

```csharp
public class LeaveBalanceLedgerSummaryDto
{
    public Guid WorkforceProfileId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeCode { get; set; } = string.Empty;
    public string LeaveTypeName { get; set; } = string.Empty;
    public int PeriodYear { get; set; }
    public decimal TotalEntitlementDays { get; set; }
    public decimal ActiveCarryForwardDays { get; set; }
    public DateTime? CarryForwardExpiryDate { get; set; }
    public decimal TotalTakenDays { get; set; }
    public decimal TotalRestoredDays { get; set; }
    public decimal TotalForfeitedDays { get; set; }
    public decimal TotalAdjustedDays { get; set; }
    public decimal CurrentBalanceDays { get; set; }
    public decimal ReservedDays { get; set; }
    public decimal AvailableBalanceDays { get; set; }
    public bool IsCarryForwardExpiringSoon { get; set; }
    public string? ExpiryWarningMessage { get; set; }
}
```

---

## 4. Hasil Verifikasi

| Komponen Uji | Kriteria Penerimaan | Status |
| :--- | :--- | :---: |
| Endpoint ESS `/my` | Mengembalikan saldo aktif, saldo bawaan, dan peringatan 30 Juni | **LULUS** |
| Tag Swagger Konsisten | Terpasang `[Tags("HR Leave - Balances & Ledgers")]` | **LULUS** |
| Kompatibilitas Rute Ganda | Dapat diakses via `/api/...` dan `/api/v1/...` | **LULUS** |
| Otorisasi Granular | Terlindungi atribut `[AccessPermission]` dan `[AccessAction]` | **LULUS** |
| Kompilasi Backend (.NET C#) | Solusi backend terkompilasi 0 error | **LULUS** |
