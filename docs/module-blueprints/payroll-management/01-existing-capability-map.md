# Payroll Management — Peta Kemampuan yang Sudah Ada

| Field | Nilai |
|---|---|
| Blueprint ID | `PRL-BP-001` |
| Revision | `1` |
| Diaudit | 12 November 2026 oleh `trace-existing-capabilities` (statis, read-only) |
| Backend | `NewQuilvianSystemBackend` @ `89b9fd69`, branch `Yasmina`, working tree bersih |
| Frontend | `QuilvianSystemFrontendDev` @ `bfa3bae69`, branch `yasmina`, working tree bersih |
| Keputusan sumber | `00-interview-decisions.md` revision 1 |
| Catatan penulisan | Audit berjalan saat repository read-only; berkas ini ditulis setelah `MODULE BLUEPRINT MODE` aktif. Dua temuan pass desain (F-12, F-13 di bagian 4) ditambahkan dan ditandai |

## 1. Batas audit

Yang diaudit: source Payroll V2 (`Areas/Corporate/PayrollManagement`), master data gaji dan
penetapan gaji HR, Medical Fee, serta menu dan layar frontend Payroll.

Yang **tidak** dilakukan: build, uji runtime, pemeriksaan isi database, penelusuran branch
`origin/MHamzah`.

Arti status "Ready to reuse" di peta ini: bukti statis lengkap untuk wiring, otorisasi, konsumen
frontend, dan data. Bukti build dan uji layar pada SHA ini belum ada; itu menunggu pemilik.

## 2. Peta kemampuan — Payroll Gaji

Semua path relatif terhadap akar repository backend kecuali disebut frontend.

| ID | Kebutuhan (menu) | Status | Bukti | Gap dan risiko |
|---|---|---|---|---|
| P-01 | Dashboard (1) | Missing | Frontend `src/app/payroll/page.jsx` hanya redirect ke Persiapan Penggajian | Tidak ada endpoint dashboard |
| P-02 | Komponen dan Kategori (2.1, 2.4) | Extend | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstPayrollComponent.cs`; hanya `ComponentType` dan `IsTaxable` dibaca di `PayrollCalculationService.cs` baris 312–317 | `CalculationMethod`, `FormulaExpression`, `DefaultAmount`, `DefaultPercentage` tidak dibaca |
| P-03 | Jenis Potongan (2.2) | Ready to reuse | `PayrollInstallmentDeductionService.cs` baris 77 dan 126 | — |
| P-04 | Jenis Tunjangan (2.1) | Extend | `MstAllowanceType.cs` | Tidak dibaca service mana pun |
| P-05 | Sumber Dana (2.3) | Missing | Tidak ada entity atau atribut | — |
| P-06 | Grade Gaji (3.1) | Ready to reuse | `SalaryGradeController.cs` baris 517 (min ≤ tengah ≤ maks) | Hanya Rekrutmen yang memakai `MaximumSalary` |
| P-07 | Struktur Gaji (3.1, 3.5) | Extend | `MstSalaryStructure.cs`; `PayrollRunService.cs` baris 570 dan 596 hanya menyalin `SalaryStructureId` | `ComponentConfigurationJson`, batas min/maks, penanda `Include*` tidak dibaca; controller memakai `ApplicationDbContext` langsung (legacy) |
| P-08 | Penetapan Gaji Pokok (3.2) | Extend | `WfpSalaryAssignmentController.cs` baris 379–396 | Tanpa cek rentang grade; satu penyetuju; layar Payroll baca-saja |
| P-09 | Tunjangan Tetap (3.5) | Conflict | `PayrollCalculationService.cs` baris 276 dan 344 | `TotalAllowance` profil, satu baris, selalu kena pajak — bertentangan dengan PRL-DEC-02 |
| P-10 | Tunjangan Jabatan dan Profesi (3.3, 3.4) | Conflict | `PayrollEarningsCalculationService.cs` baris 325–383 | Nominal di kode dari kata kunci nama — bertentangan dengan PRL-DEC-14, 15 |
| P-11 | Tunjangan Shift, Siaga, Risiko | Extend | Tiga master dan menu ada; `OnCallCompensationHandoffService.cs` hanya dipanggil controller Scheduling | Payroll tidak mengonsumsi satu pun |
| P-12 | Tunjangan Transportasi | Extend | `WfpTransportAllowancePolicyController.cs`, `WfpTransportAllowanceController.cs`, `WfpTransportAllowanceTransactionController.cs`; entity `PrlTransportAllowance*` | Tidak ada konsumen di perhitungan; **frontend tidak punya layar transport sama sekali** (F-13) |
| P-13 | Periode dan siklus run (4.1, 4.2, 4.4, 11.1–11.3) | Ready to reuse | `PayrollRunsController.cs`: 20 endpoint beratribut akses | Hanya `Regular` |
| P-14 | Run THR dan bonus tahunan | Missing | `PayrollRunService.cs` baris 304 | — |
| P-15 | Simulasi (4.3) | Missing | Hanya `SimulateEmployeeDeductionsAsync` di mesin Modern | Bukan simulasi run |
| P-16 | Koreksi (4.5, 11.2) | Reuse with adapter | Reversal per pegawai di `PayrollRunsController.cs`; Restitusi dan Koreksi | Belum ada jalur koreksi umum |
| P-17 | Input variabel: insentif, bonus, honor | Extend | Konsumsi di `PayrollCalculationService.cs` baris 302–321 | Tidak ada endpoint input atau persetujuan manual; status `Submitted` dan `Verified` ikut dihitung (F-12) |
| P-18 | Lembur dan Harian (5.1, 5.2) | Reuse with adapter | `PayrollHrdImportService` mengimpor input lembur dan kehadiran; `MstOvertimeRate` milik HR | Tidak ada layar rekap lembur di Payroll |
| P-19 | Potongan cicilan (6.4) | Ready to reuse | `PayrollEmployeeDeductionsController.cs`; `FinanceDeductionSyncService` | — |
| P-20 | PPh21 dan BPJS (6.1–6.3) | Conflict | V2: `PayrollCalculationService.cs` baris 323–325; Modern: `PayrollRunModernController.cs` baris 225 dan 268 | Dua mekanisme |
| P-21 | Persetujuan payroll (11.3) | Reuse with adapter | `PrlPayrollApproval`; `RequireDifferentApprover` bawaan `false` (`PayrollRunService.cs` baris 761) | Satu langkah; belum ada `ApproveException` |
| P-22 | Daftar pembayaran, Instruksi Transfer, Status (12.1–12.3) | Ready to reuse | `PayrollPaymentsController.cs` | — |
| P-23 | Slip (12.4) | Ready to reuse | `PayrollPayslipsController.cs`, `PayrollMyPayslipsController.cs` | — |
| P-24 | Rincian Remunerasi (12.5) | Missing | — | — |
| P-25 | Laporan Payroll dan Biaya Per-Unit (13.1, 13.3) | Reuse with adapter | `PayrollReportService.cs` baris 100; `PayrollReportDtos.cs` baris 63 (`ByCostCenter`) | Belum per komponen, belum memuat remunerasi atau Sumber Dana |
| P-26 | Jurnal (13.4) | Unknown | Accounting punya jurnal dan posting rule | Tidak diaudit apakah Accounting mengonsumsi peristiwa Payroll |
| P-27 | Histori pembayaran (13.5) | Ready to reuse | Frontend rute `payment-history` | — |

## 3. Peta kemampuan — Remunerasi dan lintas modul

| ID | Kebutuhan | Status | Bukti | Gap dan risiko |
|---|---|---|---|---|
| R-01 | Skema remunerasi (7.1–7.5) | Missing | Tidak ada entity indeks, bobot, atau pool | — |
| R-02 | Skor kinerja per pegawai | Reuse with adapter | `Areas/Corporate/HumanResource/PerformanceManagement/Models/WfpPerformanceReview.cs` | Penilaian tahunan, distribusi per periode |
| R-03 | Indikator kinerja (7.3, 10.1) | Ready to reuse | `KpiCatalogController`, `MstKpiCatalog`, `MstKpiMetricCatalog` | Hubungan dengan `PerformanceTemplate` tidak ditelusuri |
| R-04 | Jasa langsung (8.1–8.3) | Ready to reuse | `MdfServiceFee.cs`; `PayrollMedicalFeeIntakeService.cs` baris 61 | — |
| R-05 | Kontribusi per unit (8.4, 9.3) | Missing | `MdfServiceFeeDetail.cs` hanya `SourceDomain` | Butuh amandemen Medical Fee (PRL-DEC-47) |
| R-06 | Pool dan distribusi (9.1–9.4) | Missing | — | — |
| R-07 | Insentif Jabatan, Bonus Prestasi, Penyesuaian (10.2–10.4) | Missing | Kendaraan teknis: P-17 | — |
| R-08 | Persetujuan Remunerasi (11.4) | Missing | Infrastruktur: P-21 | — |
| R-09 | Cost Center | Ready to reuse | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs`; `PrlPayroll.CostCenterId`; snapshot pada `PrlPayrollRunEmployee` | Isi data tidak diperiksa |
| X-01 | Otorisasi Payroll V2 | Ready to reuse | Sembilan controller: setiap `[Http]` punya `AccessAction` dan `AccessPermission`; 15 resource | Hak `ApproveException` belum ada |
| X-02 | Satu mesin dan satu antarmuka | Conflict | `PayrollRunInitializationService.cs` baris 128 dan `PayrollRunService` sama-sama menulis `PrlPayrollRuns`; tiga layar HR Modern tidak diimpor berkas mana pun | Arah: PRL-DEC-44, menunggu MHamzah |

Ringkasan status: 11 Ready to reuse, 5 Reuse with adapter, 7 Extend, 10 Missing, 4 Conflict,
1 Unknown, 0 Repair.

## 4. Temuan tambahan pada pass desain (12 November 2026)

| ID | Temuan | Bukti @ SHA di atas | Dampak |
|---|---|---|---|
| F-12 | Perhitungan menghitung input variabel berstatus `Submitted`, `Verified`, `Approved`, `Applied`, `Processed`, `Posted`. Pembuat otomatis menulis status berbeda: jasa medis `Approved` (`PayrollMedicalFeeIntakeService.cs` baris 277), AR Finance `Draft` (`FinanceReceivablePayrollSyncService.cs` baris 493), cicilan `Applied` (`PayrollInstallmentDeductionService.cs` baris 230) | `PayrollCalculationService.cs` baris 43–46 | Input manual baru harus disetujui dulu (PRL-DEC-07) tanpa mengubah perilaku sumber otomatis |
| F-13 | Frontend tidak punya layar apa pun untuk Tunjangan Transportasi, padahal backend-nya punya tiga controller CRUD | Pencarian `transport-allowance` pada `src` frontend @ `bfa3bae69` | Transport tidak dapat dipakai tanpa layar baru |
| F-14 | Input variabel terikat ke baris pegawai di dalam run (`PayrollRunEmployeeId` wajib) dan belum punya kolom pengesahan | `PrlPayrollVariableInput.cs` baris 14 dan 47–50 | Input hanya dapat dibuat bila run periode itu sudah ada |
| F-15 | Run menyimpan waktu perhitungan terakhir (`CalculatedAt`) | `PrlPayrollRun.cs` baris 62 | Dapat dipakai menolak pengajuan run bila input berubah setelah dihitung |
| F-16 | Penyedia nomor atomik tersedia: `NumberSeriesAllocator.AllocateAsync` memakai `DbContext` terpisah | `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs` baris 84 | Nomor input tidak boleh memakai Count/Max+1 (`QBE-CODE-002`) |

## 5. Kontrak as-is yang dirujuk desain

| Grup Swagger | Base URL | Pemilik |
|---|---|---|
| `Corporate / Payroll Management / Payroll Run` | `api/v1/corporate/payroll-management/payroll-runs` | Payroll |
| `Corporate / Payroll Management / Report` | `api/v1/corporate/payroll-management/reports` | Payroll |
| `Corporate / Payroll Management / Master Data / Salary Structure` | `api/v1/corporate/payroll-management/master-data/salary-structures` | Payroll (berkas di folder HR) |
| `Corporate / Payroll Management / Master Data / Salary Grade` | `api/v1/corporate/payroll-management/master-data/salary-grades` | Payroll (berkas di folder HR) |
| `Corporate / Human Resource / Master Data / Leave and Overtime / Overtime Rate` | `api/v1/corporate/human-resource/master-data/overtime-rates` | HR |

## 6. Conflict dan Unknown

**Conflict:** X-02 (dua mesin), P-09 dan P-10 (tunjangan), P-20 (PPh/BPJS), serta dokumen
`PAYROLL_V2_02` bagian 2 lawan kode (F-11 di `00-interview-decisions.md`).

**Unknown:** build dan runtime pada SHA ini; pemakai endpoint Modern selain frontend; isi
database (Cost Center, penilaian kinerja final); isi `origin/MHamzah` `f751f8ca` (berbeda 10
baris di `PayrollEarningsCalculationService` dan 21 baris di `PayrollCalculationService`);
apakah layar 10 master berfungsi di browser.

## 7. Pemicu impact scan

Peta ini basi bila: SHA backend bukan `89b9fd69` atau frontend bukan `bfa3bae69`; ada merge dari
`origin/MHamzah`; `PayrollPolicyOptions` berubah; Medical Fee diamandemen; ada migration baru
pada tabel `Prl*` atau master payroll; menu Payroll berubah.
