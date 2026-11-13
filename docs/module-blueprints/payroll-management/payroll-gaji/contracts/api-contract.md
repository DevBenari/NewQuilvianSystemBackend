# API Contract — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status | `approved` |
| `last_changed_in` | `PGJ-C1` |
| Owner | Yasmin; API owner: Backend/API Owner; `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `input_revision` | `02-backend-architecture.md` r1, `contracts/validation-matrix.md` r1 |
| Dampak kompatibilitas | Penambahan saja. Endpoint lama tidak berubah bentuk; dua endpoint simpan master (Struktur, Grade) mendapat validasi tambahan |

Seluruh respons dibungkus `ApiResponse<T>`. Kolom **Hak akses** di bawah adalah satu-satunya
pemetaan endpoint ke hak akses di blueprint ini; atributnya ditulis
`[AccessPermission("<Resource>", "<Action>")]`. Arti kode status ada di
`contracts/validation-matrix.md` bagian 6.

---

## 1. Endpoint baru

### Corporate / Payroll Management / Master Data / Salary Structure Allowance

Base URL: `api/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId}/allowances`
Controller: `PayrollSalaryStructureAllowancesController` (`ControllerName = "SalaryStructureAllowance"`)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat tunjangan tetap satu struktur | `SalaryStructureAllowance : Read` | — | `List<SalaryStructureAllowanceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah satu jenis tunjangan ke struktur | `SalaryStructureAllowance : Create` | `CreateSalaryStructureAllowanceRequest` | `SalaryStructureAllowanceResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah cara hitung, nominal, persen, atau status aktif | `SalaryStructureAllowance : Update` | `UpdateSalaryStructureAllowanceRequest` | `SalaryStructureAllowanceResponse` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menghapus baris (ditandai terhapus) | `SalaryStructureAllowance : Delete` | query `expectedRowVersion` | `object` | **Rencana (belum tersedia)** |

Kode: 200, 400 (`SA-01`–`SA-03`, `SA-05`, `SA-06`), 403, 404, 409 (`SA-04`, `SA-07`).

Delta kontrak: baris tunjangan adalah sub-resource master. Sembilan endpoint baseline master data
tidak dipakai di sini karena baris selalu dikelola dari dalam struktur induknya; ringkasan, opsi,
dan saringan dipegang endpoint struktur.

### Corporate / Payroll Management / Payroll Variable Input

Base URL: `api/v1/corporate/payroll-management/payroll-runs/{runId}/variable-inputs`
Controller: `PayrollVariableInputsController` (`ControllerName = "PayrollVariableInput"`)
Arketipe: sub-proses transaksi yang ter-scope ke run induk. Tidak ada `DELETE`; pembatalan
lewat aksi `cancel`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar penghasilan variabel manual di run | `PayrollVariableInput : Read` | query `PayrollVariableInputQuery` | `PagedResult<PayrollVariableInputResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu input | `PayrollVariableInput : Read` | — | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat satu input `Draft` | `PayrollVariableInput : Create` | `CreatePayrollVariableInputRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/import` | Mengimpor banyak baris sekaligus, semua atau tidak sama sekali | `PayrollVariableInput : Import` | `ImportPayrollVariableInputRequest` | `PayrollVariableInputImportResult` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/content` | Mengubah komponen, jenis, nominal, atau catatan input `Draft` | `PayrollVariableInput : Update` | `UpdatePayrollVariableInputContentRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan input | `PayrollVariableInput : Submit` | `PayrollVariableInputActionRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Menyetujui input | `PayrollVariableInput : Approve` | `PayrollVariableInputActionRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak input dengan alasan | `PayrollVariableInput : Reject` | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan input dengan alasan | `PayrollVariableInput : Cancel` | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | **Rencana (belum tersedia)** |
| `POST` | `/bulk-submit` | Mengajukan banyak input `Draft` | `PayrollVariableInput : Submit` | `PayrollVariableInputBulkRequest` | `PayrollVariableInputBulkResult` | **Rencana (belum tersedia)** |
| `POST` | `/bulk-approve` | Menyetujui banyak input `Submitted` | `PayrollVariableInput : Approve` | `PayrollVariableInputBulkRequest` | `PayrollVariableInputBulkResult` | **Rencana (belum tersedia)** |

Kode: 200, 400 (`VI-01`–`VI-08`), 403, 404, 409 (`VI-09`, `VI-12`). Aksi massal memproses tiap
input sendiri-sendiri dan melaporkan hasil per input; satu input gagal tidak menggagalkan input
lain. Impor berbeda: satu baris gagal membatalkan seluruh impor (`VI-08`).

### Corporate / Payroll Management / Payroll Run — tambahan

Base URL: `api/v1/corporate/payroll-management/payroll-runs`
Controller: `PayrollRunsController` (`ControllerName = "PayrollRun"`, tidak berubah)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/simulations` | Menghitung percobaan satu periode tanpa menyimpan apa pun | `PayrollRun : Simulate` | `PayrollSimulationRequest` | `PayrollSimulationResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/overtime-inputs` | Rekap lembur terverifikasi yang diimpor ke run | `PayrollRun : Read` | query `PayrollRunOvertimeInputQuery` | `PagedResult<PayrollRunOvertimeInputResponse>` | **Rencana (belum tersedia)** |

Kode simulasi: 200, 400 (`SIM-02`, `SIM-03`), 403, 404 (`SIM-01`).

Perilaku baru pada endpoint yang sudah ada:

| Method | Path | Hak akses | Perubahan |
|---|---|---|---|
| `POST` | `/{id}/calculate` | `PayrollRun : Calculate` | Menghitung tunjangan dari struktur, transport bulanan, dan aturan input manual |
| `POST` | `/{id}/submit` | `PayrollRun : Submit` | Tambahan penolakan `VI-10` dan `VI-11` |
| `POST` | `/{id}/approve` | `PayrollRun : Approve` | Input manual `Approved` menjadi `Applied` |
| `POST` | `/{id}/cancel` | `PayrollRun : Cancel` | Input manual yang belum `Applied` menjadi `Cancelled` |

String action di atas sudah dicocokkan dengan `PayrollRunsController.cs` baris 128–157 @
`89b9fd69`.

### Corporate / Payroll Management / Report — tambahan

Base URL: `api/v1/corporate/payroll-management/reports`
Controller: `PayrollReportsController` (`ControllerName = "PayrollReport"`, tidak berubah)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/expense-summary` | Beban gaji per Cost Center per komponen, dari run yang sudah disahkan (pengganti menu Jurnal) | `PayrollReport : ViewAmount` | query `PayrollExpenseSummaryQuery` | `PayrollExpenseSummaryResponse` | **Rencana (belum tersedia)** |
| `GET` | `/tax-contribution` | PPh 21 dan iuran BPJS pegawai yang dipakai run, beserta sumbernya (tetapan profil) | `PayrollReport : ViewAmount` | query `PayrollTaxContributionQuery` | `PagedResult<PayrollTaxContributionRowResponse>` | **Rencana (belum tersedia)** |

Kode: 200 (termasuk hasil kosong `RPT-01`), 403.

## 2. Endpoint yang sudah ada dan dipakai apa adanya oleh layar baru

### Corporate / Payroll Management / Transport Allowance Policy

Base URL: `api/v1/corporate/payroll-management/transport-allowance-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan saringan | `TransportAllowancePolicy : Read` | — | metadata saringan | Sudah ada |
| `GET` | `/summary` | Ringkasan jumlah kebijakan | `TransportAllowancePolicy : Read` | — | ringkasan | Sudah ada |
| `GET` | `/` | Daftar kebijakan | `TransportAllowancePolicy : Read` | query | `PagedResult<...>` | Sudah ada |
| `GET` | `/options` | Pilihan kebijakan untuk form | `TransportAllowancePolicy : Read` | — | daftar opsi | Sudah ada |
| `GET` | `/{id}` | Detail kebijakan | `TransportAllowancePolicy : Read` | — | detail | Sudah ada |
| `POST` | `/` | Membuat kebijakan | `TransportAllowancePolicy : Create` | body | detail | Sudah ada |
| `PUT` | `/{id}` | Mengubah kebijakan | `TransportAllowancePolicy : Update` | body | detail | Sudah ada |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `TransportAllowancePolicy : Update` | body | detail | Sudah ada |
| `DELETE` | `/{id}` | Menghapus (ditandai) | `TransportAllowancePolicy : Delete` | — | — | Sudah ada |

### Corporate / Payroll Management / Transport Allowance

Base URL: `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/transport-allowance`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata`, `/summary`, `/`, `/{id}` | Melihat tunjangan transport seorang pegawai | `WorkforceTransportAllowance : Read` | query | daftar dan detail | Sudah ada |
| `POST` | `/` | Menetapkan tunjangan transport pegawai | `WorkforceTransportAllowance : Create` | body | detail | Sudah ada |
| `PUT` | `/{id}` | Mengubah | `WorkforceTransportAllowance : Update` | body | detail | Sudah ada |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan | `WorkforceTransportAllowance : Update` | body | detail | Sudah ada |
| `DELETE` | `/{id}` | Menghapus (ditandai) | `WorkforceTransportAllowance : Delete` | — | — | Sudah ada |

### Corporate / Human Resource / Master Data / Leave and Overtime / Overtime Rate

Base URL: `api/v1/corporate/human-resource/master-data/overtime-rates` — milik HR, dibaca-saja
oleh layar Tarif Lembur Payroll (PRL-DEC-42).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` dan `/{id}` | Melihat tarif lembur | `OvertimeRate : Read` | query | daftar dan detail | Sudah ada |

### Master struktur dan grade — validasi tambahan

| Grup | Method | Path | Hak akses | Perubahan |
|---|---|---|---|---|
| `Corporate / Payroll Management / Master Data / Salary Structure` | `POST`, `PUT` | `/`, `/{id}` | `SalaryStructure : Create`, `SalaryStructure : Update` (sudah ada) | Penolakan `SR-01`–`SR-03` |
| `Corporate / Payroll Management / Master Data / Salary Grade` | `PUT` | `/{id}` | `SalaryGrade : Update` (sudah ada) | Penolakan `SR-04` |

## 3. DTO

| DTO | Jenis | Field |
|---|---|---|
| `CreateSalaryStructureAllowanceRequest` | Create | `allowanceTypeId`, `calculationMethod`, `amount`, `percentage`, `notes`, `isActive` |
| `UpdateSalaryStructureAllowanceRequest` | Update | sama dengan Create, ditambah `expectedRowVersion` |
| `SalaryStructureAllowanceResponse` | Response | `id`, `salaryStructureId`, `allowanceTypeId`, `allowanceTypeCode`, `allowanceTypeName`, `calculationMethod`, `amount`, `percentage`, `maximumAmount` (dari jenis tunjangan), `isTaxable`, `isProrated`, `notes`, `isActive`, `rowVersion` |
| `PayrollVariableInputQuery` | PagedQuery | `inputStatus`, `inputType`, `search` (nama atau nomor pegawai), `pageNumber`, `pageSize` |
| `CreatePayrollVariableInputRequest` | Create | `payrollRunEmployeeId`, `payrollComponentId`, `inputType`, `amount`, `notes`, `attachmentPath` |
| `ImportPayrollVariableInputRequest` | Create | `rows[]`: `employeeNumber`, `payrollComponentCode`, `inputType`, `amount`, `notes` |
| `PayrollVariableInputImportResult` | Response | `createdCount`, `failedRows[]`: `rowNumber`, `reason` |
| `UpdatePayrollVariableInputContentRequest` | Update | `payrollComponentId`, `inputType`, `amount`, `notes`, `attachmentPath`, `expectedRowVersion` |
| `PayrollVariableInputActionRequest` | Status | `expectedRowVersion` |
| `PayrollVariableInputDecisionRequest` | Status | `reason`, `expectedRowVersion` |
| `PayrollVariableInputBulkRequest` | Status | `inputIds[]` |
| `PayrollVariableInputBulkResult` | Response | `succeeded[]`, `failed[]`: `inputId`, `reason` |
| `PayrollVariableInputResponse` | Response | `id`, `inputNumber`, `payrollRunEmployeeId`, `employeeNumber`, `employeeName`, `payrollComponentCode`, `payrollComponentName`, `inputType`, `inputStatus`, `amount`, `notes`, `submittedAt`, `submittedByName`, `approvedAt`, `approvedByName`, `rejectedAt`, `rejectedByName`, `decisionReason`, `availableActions[]`, `rowVersion` |
| `PayrollSimulationRequest` | Create | `payrollPeriodId`, `workforceProfileIds[]` (opsional; kosong = seluruh pegawai sampai batas) |
| `PayrollSimulationResponse` | Response | `periodCode`, `policyVersion`, `employeeCount`, `totalGrossPay`, `totalDeduction`, `totalNetPay`, `employees[]`: `workforceProfileId`, `employeeName`, `grossPay`, `totalDeduction`, `netPay`, `components[]` (`code`, `name`, `type`, `amount`, `isTaxable`), `warnings[]`, `errors[]` |
| `PayrollRunOvertimeInputQuery` | PagedQuery | `search`, `pageNumber`, `pageSize` |
| `PayrollRunOvertimeInputResponse` | Response | `employeeNumber`, `employeeName`, `overtimeDate`, `verifiedMinutes`, `rateMultiplier`, `hourlyRate`, `overtimeAmount` |
| `PayrollExpenseSummaryQuery` | PagedQuery | `payrollPeriodId` atau `payrollRunId` (salah satu wajib), `costCenterId` (opsional) |
| `PayrollExpenseSummaryResponse` | Response | `periodCode`, `runNumbers[]`, `rows[]`: `costCenterCode`, `costCenterName`, `componentCode`, `componentName`, `componentType`, `employeeCount`, `amount`; `totalEarning`, `totalDeduction`, `note` |
| `PayrollTaxContributionQuery` | PagedQuery | `payrollPeriodId` atau `payrollRunId`, `search`, `pageNumber`, `pageSize` |
| `PayrollTaxContributionRowResponse` | Response | `employeeNumber`, `employeeName`, `costCenterName`, `taxAmount`, `insuranceEmployeeAmount`, `source` (`TETAPAN_PROFIL`), `policyVersion` |

Seluruh nominal pada respons di atas sensitif (lihat `data/data-dictionary.md`).
