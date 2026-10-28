# Laporan Perubahan Backend — `BE-HRD-014`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-014` |
| Judul | Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding Karyawan |
| Slice | `S-C4` (Exit Clearance, Pengembalian Aset, & Offboarding) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-003`, `HRD-DEC-019`, `IN-05`, `BE-FIN-112`, `FIN-DEC-193` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target tulis | `ExitClearanceController.cs`, `ExitClearanceService.cs`, `ExitClearanceDtos.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

1. **Ketiadaan Validasi Komprehensif Antar-Unit pada Alur Berhenti Kerja Pegawai**:
   Sebelumnya, proses offboarding pegawai rumah sakit (pengunduran diri, pensiun, atau masa kontrak selesai) berisiko menimbulkan kerugian aset rumah sakit (misal laptop dinas, alat medis perorangan, ID card akses ruangan steril) serta kebocoran data rekam medis jika hak akses sistem/IT belum dicabut secara disiplin.
2. **Ketiadaan Penegakan Gerbang Bebas Tanggungan Keuangan (*Finance Clearance Gate*)**:
   Pegawai yang memiliki sisa pinjaman koperasi RS, kasbon dinas, atau tagihan piutang belum terselesaikan di bagian Keuangan/Finance sebelumnya dapat lolos tanpa pelunasan. Melalui integrasi `BE-FIN-112` (`FinanceReceivableClearanceService`), status bebas tanggungan dikunci secara sistem dan tidak dapat dimanipulasi secara manual oleh HR.
3. **Ketiadaan Laporan Perubahan Resmi Berbasis Bukti (*Tracked Report*)**:
   Meskipun controller dan service `ExitClearanceController` telah aktif di codebase, laporan task resmi dan sinkronisasi status roadmap modul Human Resource belum tercatat sesuai tata kelola Quilvian Engineering Skills.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Pegawai Berhenti / Resign / Pensiun]
                  │
                  ▼
         [Employee Separation]
                  │
                  ├──► 1. HR Menginisiasi Exit Clearance ──► POST /exit-clearances
                  │       └─► Status awal: Draft (Total Item: 6 Checklist Aspek)
                  │
                  ├──► 2. Verifikasi Aspek Multi-Unit: ──► PUT /exit-clearances/{id}/checklist
                  │       ├─► Departemen / Unit Kerja (IsDepartmentCleared)
                  │       ├─► Pengembalian Aset & Inventaris RS (IsAssetCleared)
                  │       ├─► Hak Akses SIMRS & Pintu Masuk Steril (IsItAccessCleared)
                  │       ├─► Hak Payroll & Pesangon (IsPayrollCleared)
                  │       └─► Dokumen Administrasi HR (IsHrCleared)
                  │
                  ├──► 3. Pemeriksaan Otomatis Tanggungan Finance ──► POST /exit-clearances/{id}/verify-finance
                  │       ├─► Sistem memanggil FinanceReceivableClearanceService
                  │       ├─► Jika ada sisa piutang > Rp 0 ──► IsFinanceCleared = false (TERKUNCI)
                  │       └─► Jika piutang lunas (Rp 0) ──► IsFinanceCleared = true (LULUS)
                  │
                  └──► 4. Pengesahan Akhir Exit Clearance ──► POST /exit-clearances/{id}/complete
                          ├─► Sistem memverifikasi ulang IsFinanceCleared == true
                          ├─► Jika belum lunas ──► DITOLAK KERAS (422 Unprocessable Entity)
                          └─► Jika seluruh aspek terpenuhi ──► Status: Completed
```

* **Tujuan**: Menjamin bahwa seluruh kewajiban administratif, fisik, aset rumah sakit, keamanan informasi medis, dan tanggungan finansial pegawai telah selesai 100% sebelum pegawai resmi dilepas dari rumah sakit.
* **Pelaku**: HR Administrator, Bagian Aset/Umum (GA), Tim IT SIMRS, Unit Kerja Terkait, Bagian Finance/Piutang.
* **Pemicu**: Pengajuan resign yang telah disetujui, penetapan masa pensiun, atau habisnya masa kontrak kerja.
* **Langkah & Aturan**:
  1. Exit clearance dibuat berdasar data `TrxEmployeeSeparation`.
  2. Checklist mencakup 6 aspek: Unit Kerja, Aset, IT Access, Payroll, HR, dan Finance.
  3. Status `IsFinanceCleared` tidak dapat diubah oleh form checklist HR; status ini authoritative diambil langsung dari modul Finance (`FinanceReceivableClearanceService`).
  4. Penyelesaian clearance (`CompleteAsync`) memvalidasi ulang kepatuhan keuangan; jika pegawai masih berhutang, permohonan penyelesaian ditolak.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Lifecycle Management / Exit Clearance`
Tag: `[Tags("Corporate / Human Resource / Lifecycle Management / Exit Clearance")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/filters/metadata` | Mengambil metadata opsi filter exit clearance | `ExitClearance:Read` | - | `ApiResponse<ExitClearanceFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/summary` | Mengambil statistik ringkasan exit clearance (Draft, InProgress, Completed, PendingFinance) | `ExitClearance:Read` | - | `ApiResponse<ExitClearanceSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances` | Mengambil daftar transaksi exit clearance berhalaman | `ExitClearance:Read` | `ExitClearanceQuery` | `ApiResponse<PagedResult<ExitClearanceListResponse>>` |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}` | Mengambil detail lengkap satu transaksi exit clearance | `ExitClearance:Read` | `id` (Guid) | `ApiResponse<ExitClearanceDetailResponse>` |
| `GET` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/by-separation/{separationId}` | Mengambil data exit clearance berdasarkan ID pemisahan pegawai | `ExitClearance:Read` | `separationId` (Guid) | `ApiResponse<ExitClearanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances` | Membuat transaksi exit clearance baru dari berkas pemisahan | `ExitClearance:Create` | `CreateExitClearanceRequest` | `ApiResponse<ExitClearanceDetailResponse>` |
| `PUT` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}/checklist` | Memperbarui verifikasi checklist aspek unit, aset, IT, dan HR | `ExitClearance:Update` | `UpdateExitClearanceChecklistRequest` | `ApiResponse<ExitClearanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}/verify-finance` | Memeriksa live status bebas tanggungan ke sistem Finance RS | `ExitClearance:Update` | `id` (Guid) | `ApiResponse<ExitClearanceFinanceCheckResponse>` |
| `POST` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}/complete` | Menyelesaikan dan mengesahkan exit clearance (Wajib bebas tanggungan Finance) | `ExitClearance:Complete` | `CompleteExitClearanceRequest` | `ApiResponse<ExitClearanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/lifecycle-management/exit-clearances/{id}/cancel` | Membatalkan transaksi exit clearance | `ExitClearance:Cancel` | `CancelExitClearanceRequest` | `ApiResponse<ExitClearanceDetailResponse>` |

---

## 4. Bukti implementasi kode

1. **Controller Lengkap**:
   - Berkas: [ExitClearanceController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ExitClearanceController.cs)
   - Mengimplementasikan 10 endpoint RESTful dengan penegakan permission `ExitClearance` dan penanganan status respons baku.

2. **Domain Service & Integrasi Finance**:
   - Berkas: [ExitClearanceService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LifecycleManagement/Services/ExitClearanceService.cs)
   - Mengintegrasikan pemeriksaan piutang via `FinanceReceivableClearanceService`, kalkulasi persentase kelulusan checklist 6 aspek, audit log komprehensif, dan penguncian gerbang penyelesaian.

3. **Registrasi Service**:
   - Terdaftar pada [Program.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Program.cs#L974) (`builder.Services.AddScoped<ExitClearanceService>()`).

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: `ExitClearanceController` memvalidasi status kelulusan setiap unit verifikator (IT, GA/Asset, Unit/Department, HR, dan Keuangan) -> **LULUS**.
* **Acceptance Criteria 2**: Penutupan exit clearance menandai status `Completed`, mencatat tanggal penyelesaian, dan memvalidasi `IsFinanceCleared` -> **LULUS**.
