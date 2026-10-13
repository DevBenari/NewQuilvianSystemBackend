# Laporan Perubahan Backend — `BE-FIN-110`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-110` |
| Judul | Potongan cicilan berjalan di dalam mesin payroll run HR dan hasilnya terkirim otomatis ke Finance |
| Slice | `REV-21` (Dirumuskan Ulang dari `REV-19` Slice C) — Payroll Run Integration & Hospital AR Loan Deduction (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` (dirumuskan ulang `REV-21`) |
| Trace | `FIN-DEC-163`, `167`, `168`, `179`, `190`..`192`, `214`, `216`, `217`, `222`; `FR-FIN-224`..`227` |
| Contract version | `FIN-INTEGRATION-1.8` §`P.2`, §`P.3` (dikoreksi `FIN-INTEGRATION-1.10` §`S.5`); `FIN-INTEGRATION-1.10` §`S.4`; `FIN-VAL-1.13` `FIN-VAL-287`, `288` |
| Dependency | `BE-FIN-094` 🟡, `BE-FIN-095` 🟡, `{FIN-OQ-104}` ✅, `{FIN-OQ-106}` ✅ sebagian, `{GATE-01}` ✅, `{GATE-02}` ✅, `{GATE-03}`, `{FIN-OQ-111}` |
| Klasifikasi | `HEAVY` — integrasi lintas modul HR Payroll dan Finance Receivable: mesin potongan cicilan di HR (`PayrollInstallmentDeductionService`), master batas potongan gaji bersih (`MaxNetSalaryPercentage`), master cutover peralihan (`MstPayrollDeductionCutover`), pemisahan inti `ApplyResultCoreAsync`, validasi panggilan manual (`FIN-VAL-287`) dan peralihan (`FIN-VAL-288`) |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("kerjakan BE-FIN-110 tanpa build dotnet automatis /build-module-backend", 13 Oktober 2026) |
| Target tulis | Lihat §2.2 |
| Model | Claude Sonnet 3.7 / Antigravity |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh implementasi logika mesin potongan cicilan HR, batas persentase gaji bersih pada `MstDeductionType`, model cutover `MstPayrollDeductionCutover`, pemisahan inti pencatatan hasil `ApplyResultCoreAsync`, penegakan validasi `FIN-VAL-287` dan `FIN-VAL-288`, serta endpoint eksekusi potongan telah selesai diimplementasikan di source (10 berkas); `dotnet build` **NOT RUN** oleh agent (instruksi pemilik: "tanpa build dotnet automatis"); file migration disiapkan menunggu eksekusi bersama pemilik |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | Corporate / HumanResource / PayrollManagement (layanan pemotongan cicilan di dalam payroll run); Corporate / HumanResource / MasterData / PayrollAndBenefit (master batas potongan gaji bersih `MstDeductionType`); Corporate / FinanceManagement / Receivable (layanan inti penerapan hasil potongan `FinanceReceivablePayrollSyncService`); Corporate / FinanceManagement / MasterData (master peralihan `MstPayrollDeductionCutover`) |
| Registry | `HumanResource — Hrd — ACTIVE`; `FinanceManagement — Fin — ACTIVE` |
| Keberlakuan | `TOUCHED CODE` (`MstDeductionType.cs`, `MstDeductionTypeConfiguration.cs`, `DeductionTypeDtos.cs`, `DeductionTypeController.cs`, `FinanceReceivablePayrollSyncService.cs`, `ApplicationDbContext.cs`, `Program.cs`); `NEW CODE` (`PayrollInstallmentDeductionService.cs`, `PayrollRunInstallmentDeductionController.cs`, `PayrollInstallmentDeductionDtos.cs`, `MstPayrollDeductionCutover.cs`, `MstPayrollDeductionCutoverConfiguration.cs`) |
| Entity operasional baru | `MstPayrollDeductionCutover` (master data Finance dengan prefix `Mst`, pola `MstDirectPaymentThreshold`); tidak ada duplikasi entitas operasional bersama |
| QBE yang berlaku | `QBE-SVC-001` (authoritative service logic, zero calculation di klien), `QBE-API-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-CFG-001`, `QBE-NAM-001` |

---

## 1. Masalah yang Diperbaiki

Pada perancangan awal (`REV-19`), task `BE-FIN-110` mencakup eksekusi potongan cicilan di dalam mesin payroll run HR. Namun, audit kapabilitas pada `FIN-DEC-214` membuktikan bahwa:
1. **Mesin Payroll Run Belum Dibangun di HR**: Folder `Corporate/HumanResource/PayrollManagement` hanya memuat tabel model EF Core dan controller profil workforce murni; tidak ada service yang menjalankan kalkulasi gaji, pajak, maupun potongan secara dinamis.
2. **Kebutuhan Pemisahan Scope (`REV-21`)**: Sesuai amendment `REV-21`, task `BE-FIN-110` dirumuskan ulang secara tajam: **hanya bagian potongan cicilan di dalam mesin**, bukan membangun seluruh mesin payroll run (gaji pokok, pajak, netto) yang merupakan domain pekerjaan HR `S-B5` (`GATE-03`).
3. **Ketiadaan Batas Persentase Gaji Bersih**: Entitas `MstDeductionType` sebelumnya hanya memiliki `MaximumAmount` (rupiah) dan `DefaultPercentage`; tidak ada kolom batas persentase dari gaji bersih (`FIN-DEC-217`).
4. **Pemisahan Jalur Mesin dan Jalur Manual**: Sesuai `FIN-DEC-222` dan `FIN-INTEGRATION-1.10` §`S.4` & §`S.5`, pemanggilan langsung ke penerima hasil potongan tanpa `PayrollRunId` wajib ditolak (`FIN-VAL-287`), dan hasil mesin untuk periode sebelum tanggal peralihan wajib ditolak (`FIN-VAL-288`).

Task `BE-FIN-110` menuntaskan seluruh kebutuhan di atas secara terstruktur.

---

## 2. Proses Bisnis & Integrasi Pemotongan Cicilan Mesin HR

### 2.1 Pemenuhan Acceptance Criteria

1. **AC 1 — Pemotongan Sebagian & Pelimpahan Tunggakan Terbawa**:
   - Contoh kasus: Cicilan Rp 1.000.000 gagal selama 2 bulan (tunggakan Rp 2.000.000). Pada bulan ke-3, total terutang adalah Rp 3.000.000 (tercatat di `TrxPayrollVariableInput.Amount`).
   - Jika batas persentase gaji bersih hanya mengizinkan potongan sebesar Rp 1.500.000, mesin memotong `SEBAGIAN` sebesar Rp 1.500.000.
   - Hasil dikirim ke Finance via `ApplyResultCoreAsync`: saldo piutang berkurang Rp 1.500.000, mutasi subledger `POTONGAN-GAJI` diterbitkan, dan sisa tunggakan Rp 1.500.000 secara otomatis dilimpahkan ke `CarriedOverAmount` baris angsuran berikutnya.
2. **AC 2 — Urutan Prioritas Potongan (`FIN-DEC-216`)**:
   - Potongan wajib (`IsStatutory = true`, seperti BPJS Ketenagakerjaan/Kesehatan dan Pajak PPh 21) serta potongan non-loan lain yang memiliki prioritas lebih tinggi dikurangkan terlebih dahulu dari `GrossPay` untuk mendapatkan `AvailableNetSalary`.
   - Potongan cicilan piutang RS (kategori `Loan`, prioritas terendah) hanya mengambil porsi sisa gaji bersih yang diizinkan sesuai `MaxNetSalaryPercentage`.
3. **AC 3 — Idempotensi Kiriman Ulang (`FIN-VAL-240`)**:
   - Pemeriksaan `installment.LastPayrollPeriodId == request.PayrollPeriodId` memastikan bahwa kiriman ulang untuk periode yang sama menghasilkan respons `200 OK` dengan `IsIdempotentReplay = true` tanpa memotong saldo piutang atau mutasi dua kali.
4. **AC 4 — Penolakan Periode Sebelum Peralihan (`FIN-VAL-288`)**:
   - Bila `MstPayrollDeductionCutover` aktif (misal `AutomaticFromPeriod = "2027-01"`), pengiriman hasil mesin yang membawa `PayrollRunId` untuk periode sebelum peralihan (misal `2026-10`) ditolak dengan pesan `422 Unprocessable Entity`: *"Periode 2026-10 masih memakai pencatatan manual."*

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (`BE-FIN-110`, Amendment Revisi 21)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§`P.2`, §`P.3`, §`S.4`, §`S.5`)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-239`, `240`, `248`, `287`, `288`)
- `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstDeductionType.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesPayrollController.cs`

### 3.2 Berkas yang Berubah / Dibuat

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstDeductionType.cs` | Diperbarui | Menambahkan kolom `MaxNetSalaryPercentage` (`decimal?`) untuk batas persentase gaji bersih (`FIN-DEC-217`) |
| `Repositories/Configurations/Corporate/HumanResource/MasterData/PayrollAndBenefit/MstDeductionTypeConfiguration.cs` | Diperbarui | Konfigurasi presisi `(9, 4)` dan check constraint `CK_MstDeductionType_MaxNetSalaryPercentage` (0..100%) |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/DTOs/DeductionTypeDtos.cs` | Diperbarui | Expose `MaxNetSalaryPercentage` pada `DeductionTypeResponse` dan `CreateDeductionTypeRequest` |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/DeductionTypeController.cs` | Diperbarui | Mapping `MaxNetSalaryPercentage` pada query paged, detail, create, dan update |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstPayrollDeductionCutover.cs` | **Baru** | Model master peralihan `AutomaticFromPeriod` (format `YYYY-MM`), alasan, versi konkurensi |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstPayrollDeductionCutoverConfiguration.cs` | **Baru** | Konfigurasi EF Core, check constraint regex `^\d{4}-\d{2}$`, unique index parsial baris aktif |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | Pendaftaran `DbSet<MstPayrollDeductionCutover>` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs` | Diperbarui | Pemisahan `ApplyResultCoreAsync`, validasi peralihan `FIN-VAL-288`, penolakan manual tanpa run id `FIN-VAL-287` |
| `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollInstallmentDeductionDtos.cs` | **Baru** | DTO respons eksekusi pemotongan angsuran per pegawai dan ringkasan per payroll run |
| `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollInstallmentDeductionService.cs` | **Baru** | Engine pemrosesan pemotongan cicilan di mesin payroll HR, urutan prioritas, kalkulasi batas, komponen slip, dan pelaporan otomatis ke Finance |
| `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunInstallmentDeductionController.cs` | **Baru** | Endpoint `POST {runId}/installment-deductions` dan `POST employees/{runEmployeeId}/installment-deductions` |
| `Program.cs` | Diperbarui | Pendaftaran `AddScoped<PayrollInstallmentDeductionService>()` |
| `Migrations/20261013123000_AddPayrollInstallmentDeductionAndCutover.cs` | **Baru** | Skrip migration EF Core untuk penambahan kolom dan tabel baru |

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pemilik ("tanpa build dotnet automatis") |
| Review Logika Aljabar: Pemotongan Sebagian & Sisa Terbawa (AC 1) | `PASS` | Review manual | Terbukti di `PayrollInstallmentDeductionService.cs` dan `ApplyResultCoreAsync`: saat `dueAmount = 3.000.000` dan limit `1.500.000`, dipotong `1.500.000` status `SEBAGIAN`, sisa `1.500.000` dilimpahkan ke baris berikutnya |
| Review Urutan Prioritas Potongan (AC 2) | `PASS` | Review manual | `AvailableNetSalary = GrossPay - statutoryDeductions - otherPriorDeductions`. Potongan wajib dan non-loan dipotong sebelum loan |
| Review Idempotensi Kiriman Ulang (AC 3) | `PASS` | Review manual | `installment.LastPayrollPeriodId == request.PayrollPeriodId` mengembalikan `200 OK` dengan `IsIdempotentReplay = true` |
| Review Validasi Masa Peralihan Cutover (AC 4) | `PASS` | Review manual | `string.CompareOrdinal(installment.DeductionPeriod, cutover.AutomaticFromPeriod) < 0` melempar `PayrollResultValidationException` (`422`) |
| Review Validasi Panggilan Manual (FIN-VAL-287) | `PASS` | Review manual | Panggilan `ProcessResultAsync` tanpa `PayrollRunId` melempar `PayrollResultValidationException` (`422`) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

---

## 5. Risiko dan Rekomendasi Selanjutnya

1. **Eksekusi Build & Migration oleh Pemilik**: Pemilik perlu menjalankan `dotnet build` dan menerapkan migrasi `20261013123000_AddPayrollInstallmentDeductionAndCutover` ke database target.
2. **Task Penerus**:
   - `BE-FIN-111`: Restitusi salah orang melalui slip gaji periode berikutnya (menunggu `BE-FIN-110` dan `BE-FIN-109`).
   - `BE-FIN-115`: Master penetapan periode pertama peralihan ke mesin payroll (`MstPayrollDeductionCutover`).

