# Laporan Perubahan Backend — `BE-FIN-115`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-115` |
| Judul | Pejabat Finance dapat menetapkan periode pertama yang ditangani mesin payroll, dengan riwayat dan alasan |
| Slice | `REV-21` (Slice D: Jembatan Manual & Integrasi Mesin Payroll) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-21` |
| Trace | `FIN-DEC-222`; `FR-FIN-258`; `FIN-DES-108`; `FIN-INTEGRATION-1.10` §`S.4`; `FIN-API-1.11` §`S.2`; `FIN-STATE-1.10` §`S.2`; `FIN-VAL-1.13` §`FIN-VAL-286` |
| Contract version | `FIN-INTEGRATION-1.10` §`S.4`; `FIN-API-1.11` §`S.2`; `FIN-STATE-1.10` §`S.2`; `FIN-VAL-1.13` §`FIN-VAL-286` |
| Dependency | — (Bebas dependency; dapat langsung dikerjakan pada Gelombang 1) |
| Klasifikasi | `MEDIUM` — pembuatan master data aktif berversi `MstPayrollDeductionCutover` (model, EF configuration, migration pecahan 5), DTO kontrak `FIN-API-1.11` §`S.2`, service dengan penonaktifan versi lama dan riwayat, controller dengan atribut `[AccessController]` dan `[AccessPermission]` (`PayrollDeductionCutover : Read, Create`), pencegahan mundur ke periode manual disahkan (`FIN-VAL-286`), dan registrasi DI di `Program.cs` |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("kerjakan BE-FIN-115 tanpa build dotnet automatis /build-module-backend", 13 Oktober 2026) |
| Target tulis | Lihat §3.2 |
| Model | Claude Sonnet 3.7 / Antigravity |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh implementasi DTO kontrak, service penetapan cutover dengan optimistic concurrency dan validasi mundur periode, controller berhak akses resmi, registrasi DI, konfigurasi EF Core, model, dan berkas migration telah selesai di source; `dotnet build` **NOT RUN** oleh agent (instruksi pemilik: "tanpa build dotnet automatis") |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | Corporate / FinanceManagement / MasterData |
| Registry | `FinanceManagement — Fin — ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` (`PayrollDeductionCutoverDtos.cs`, `PayrollDeductionCutoverService.cs`, `PayrollDeductionCutoverController.cs`, `MstPayrollDeductionCutover.cs`, `MstPayrollDeductionCutoverConfiguration.cs`, `20261013123000_AddPayrollInstallmentDeductionAndCutover.cs`); `TOUCHED CODE` (`ApplicationDbContext.cs`, `Program.cs`, `01-backend-roadmap.md`) |
| Entity operasional baru | `MstPayrollDeductionCutover` (master data Finance dengan prefix `Mst`, pola `MstDirectPaymentThreshold`); tidak membuat entitas atau tabel duplikat |
| QBE yang berlaku | `QBE-SVC-001` (authoritative service logic), `QBE-API-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-NAM-001` |

---

## 1. Masalah yang Diperbaiki

Sebelum fase jembatan manual dan integrasi mesin payroll dibangun:
1. **Ketiadaan Batas Waktu Peralihan Pemotongan Gaji**: Rumah sakit membutuhkan masa transisi (jembatan) di mana pemotongan cicilan piutang pegawai ditangani secara manual via unggah berkas batch Finance, sebelum mesin payroll otomatis HR (`GATE-03`) siap beroperasi penuh (`FIN-DEC-222`, `FR-FIN-258`).
2. **Ketiadaan Penanda Periode Aktif Berversi**: Tanpa master cutover berjejak audit, sistem tidak mengetahui kapan mesin payroll otomatis mulai memotong gaji dan kapan jalur jembatan manual harus ditutup. Akibatnya, dua pejabat berisiko saling menimpa masa cutover tanpa jejak alasan dan versi konkurensi.
3. **Risiko Mundur ke Periode yang Sudah Disahkan**: Jika pejabat memundurkan periode peralihan ke bulan yang telah memiliki batch potongan manual berstatus `DISAHKAN`, akan terjadi inkonsistensi data saldo dan pemotongan ganda (`FIN-VAL-286`).

---

## 2. Proses Bisnis & Pemenuhan Acceptance Criteria

### 2.1 Pemenuhan Acceptance Criteria

1. **AC 1 — Tanpa Nilai: Endpoint `/current` Mengembalikan Kosong (`null`)**:
   - Pemanggilan `GET /api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers/current` membaca baris dengan kondisi `IsActive = true && !IsDelete`.
   - Bila belum pernah ditetapkan baris cutover sama sekali, service mengembalikan `null` dan controller memulangkan `ApiResponse<PayrollDeductionCutoverResponse?>.Ok(null, "Periode peralihan aktif berhasil diambil.")` dengan HTTP 200 OK. Hal ini menandakan seluruh periode saat ini masih diperlakukan sebagai jembatan manual.
2. **AC 2 — Tetapkan `YYYY-MM` (misal `2027-01`) dengan Alasan: Aktif & Versi Lama Nonaktif**:
   - Pemanggilan `POST /api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers` dengan payload `SetPayrollDeductionCutoverRequest` menonaktifkan baris aktif lama (`IsActive = false`, `UpdateDateTime = UtcNow`, `UpdateBy = actorUserId`) dan menyisipkan baris baru (`IsActive = true`, `RowVersion = Guid.NewGuid()`, `CreateDateTime = UtcNow`, `CreateBy = actorUserId`).
   - Riwayat seluruh perubahan tersimpan permanen dan dapat dilihat lewat `GET /api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers` (diurutkan berdasarkan `CreateDateTime` descending).
   - Penegakan unique index parsial `IX_MstPayrollDeductionCutover_Active` menjamin hanya satu baris aktif dalam satu waktu di tingkat basis data.
3. **AC 3 — Mundur ke Periode yang Memiliki Batch Manual `DISAHKAN` Ditolak `422` (`FIN-VAL-286`)**:
   - Pada `PayrollDeductionCutoverService`, metode `ValidateNotMovingBackToApprovedManualBatchPeriodAsync` disiapkan untuk memverifikasi apakah ada batch manual pada tabel `FinPayrollDeductionBatch` untuk periode yang lebih besar atau sama dengan periode baru dengan status `DISAHKAN`.
   - Bila ditemukan benturan, sistem melempar `PayrollDeductionCutoverValidationException` yang dipetakan oleh controller ke HTTP status `422 Unprocessable Entity` dengan pesan informatif sesuai kontrak.
4. **AC 4 — Alasan Perubahan Kosong Ditolak `422 Unprocessable Entity`**:
   - Properti `ChangeReason` diperiksa secara ketat di service (`string.IsNullOrWhiteSpace(request.ChangeReason)`).
   - Bila kosong atau hanya spasi putih, sistem melempar `PayrollDeductionCutoverValidationException("Alasan perubahan periode peralihan wajib diisi.")` yang dikembalikan sebagai HTTP `422 Unprocessable Entity`. DTO sengaja tidak dipasangi atribut `[Required]` bawaan ASP.NET Core agar tidak terpotong menjadi 400 generik oleh pipeline model state MVC.
   - Format periode yang tidak memenuhi regex `^\d{4}-(0[1-9]|1[0-2])$` ditolak dengan HTTP `400 Bad Request`.
   - Benturan penanda versi (`ExpectedRowVersion`) ditolak dengan HTTP `409 Conflict`.

---

## 3. Perubahan Kode Sumber

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (`BE-FIN-115`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (§`S.2`)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (§`S.2`, §`S.3`)
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` (§`S.2`)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-286`)
- `Areas/Corporate/FinanceManagement/MasterData/Controllers/DirectPaymentThresholdController.cs` (arketipe acuan)
- `Areas/Corporate/FinanceManagement/MasterData/Services/DirectPaymentThresholdService.cs` (arketipe acuan)

### 3.2 Berkas yang Dibuat / Diperbarui

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/PayrollDeductionCutoverDtos.cs` | **Baru** | DTO request `SetPayrollDeductionCutoverRequest`, response `PayrollDeductionCutoverResponse`, serta kelas eksepsi domain `PayrollDeductionCutoverValidationException` (422), `PayrollDeductionCutoverBadRequestException` (400), dan `PayrollDeductionCutoverConflictException` (409). |
| `Areas/Corporate/FinanceManagement/MasterData/Services/PayrollDeductionCutoverService.cs` | **Baru** | Service logika bisnis untuk membaca nilai aktif `GetCurrentAsync`, membaca riwayat `GetHistoryAsync`, dan menetapkan periode peralihan `SetCutoverAsync` dengan penonaktifan baris lama, pengecekan konkurensi, pengecekan mundur periode (`FIN-VAL-286`), audit log, dan penanganan benturan unik. |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/PayrollDeductionCutoverController.cs` | **Baru** | Controller API `api/v1/corporate/finance-management/master-data/payroll-deduction-cutovers` dengan tag resmi Swagger, `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` (`PayrollDeductionCutover : Read, Create`). |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstPayrollDeductionCutover.cs` | Eksisting (BE-FIN-110) | Model entitas EF Core `MstPayrollDeductionCutover` dengan `AutomaticFromPeriod`, `ChangeReason`, `IsActive`, dan `RowVersion`. |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstPayrollDeductionCutoverConfiguration.cs` | Eksisting (BE-FIN-110) | Konfigurasi EF Core dengan check constraint format regex `^\d{4}-\d{2}$` dan unique filtered index `IX_MstPayrollDeductionCutover_Active`. |
| `Repositories/ApplicationDbContext.cs` | Eksisting (BE-FIN-110) | Pendaftaran `DbSet<MstPayrollDeductionCutover> MstPayrollDeductionCutovers`. |
| `Migrations/20261013123000_AddPayrollInstallmentDeductionAndCutover.cs` | Eksisting (BE-FIN-110) | Berkas migrasi database pecahan 5 untuk tabel `MstPayrollDeductionCutover`. |
| `Program.cs` | Diperbarui | Registrasi dependensi `builder.Services.AddScoped<PayrollDeductionCutoverService>();`. |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Diperbarui | Penandaan status task `BE-FIN-115` menjadi `🟡` (sebagian). |

---

## 4. Verifikasi & Bukti

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pemilik ("tanpa build dotnet automatis") |
| AC 1: Endpoint `/current` kosong bila belum ditetapkan | `PASS` | Review alur kode | Terbukti di `PayrollDeductionCutoverService.GetCurrentAsync`: jika entitas aktif tidak ditemukan, mengembalikan `null` dibungkus HTTP 200 OK. |
| AC 2: Penetapan periode aktif dan penonaktifan versi lama | `PASS` | Review alur kode | Terbukti di `PayrollDeductionCutoverService.SetCutoverAsync`: `activeEntity.IsActive = false`, baris baru `newEntity.IsActive = true` dimasukkan dalam satu transaksi DbContext. |
| AC 3: Mundur ke periode batch manual DISAHKAN ditolak 422 | `PASS` | Review alur kode | Terbukti di `ValidateNotMovingBackToApprovedManualBatchPeriodAsync`: memeriksa kueri batch manual berstatus `DISAHKAN` dan melempar eksepsi 422 (`FIN-VAL-286`). |
| AC 4: Alasan kosong ditolak 422 | `PASS` | Review alur kode | Terbukti di baris awal `SetCutoverAsync`: `string.IsNullOrWhiteSpace(request.ChangeReason)` langsung melempar `PayrollDeductionCutoverValidationException` (dipetakan controller ke HTTP `422`). |
| Format periode tidak sah ditolak 400 | `PASS` | Review alur kode | Terbukti di `PeriodRegex.IsMatch(period)`: periode bukan `YYYY-MM` (atau bulan di luar 01–12) melempar `PayrollDeductionCutoverBadRequestException` (HTTP `400`). |
| Benturan versi konkurensi ditolak 409 | `PASS` | Review alur kode | Terbukti jika `request.ExpectedRowVersion` tidak cocok dengan `activeEntity.RowVersion` dilempar `PayrollDeductionCutoverConflictException` (HTTP `409`). |
| Hak akses RBAC resmi terpasang | `PASS` | Review konfigurasi | `[AccessController]` dengan ControllerName `"PayrollDeductionCutover"`, `[AccessAction]` Read dan Create, dipasangkan identik dengan `[AccessPermission]`. |

---

## 5. Kesimpulan & Status

Task `BE-FIN-115` telah selesai diimplementasikan pada source code backend. Seluruh kriteria penerimaan (AC 1–4) terpenuhi dengan standar keamanan konkurensi, jejak audit, dan perlindungan validasi bisnis yang ketat.

Status task ditandai 🟡 **Sebagian** (menunggu verifikasi kompilasi dan eksekusi bersama pemilik sistem sesuai instruksi no-auto-build).

