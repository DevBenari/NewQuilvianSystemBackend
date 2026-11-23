# Laporan Perubahan Backend — `PAY-BE-FIX-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-007` (ad-hoc, bukan kartu roadmap; nomor dipilih agent melanjutkan seri `PAY-BE-FIX-001..006`) |
| Judul | Penghapusan fitur iuran BPJS Kesehatan dan BPJS Ketenagakerjaan dari jalur aktif Payroll, aman terhadap skema dan data historis |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas master prompt pemilik 2026-11-20; roadmap dan traceability tidak disentuh (preseden `PAY-BE-FIX-001..006`) |
| Dependency | Tidak ada DDL, migration, atau perubahan data. Frontend di `PAY-FE-FIX-007`. Membalik sebagian keputusan `PAY-BE-FIX-006` ("penghapusan butuh instruksi tertulis terpisah"): instruksi itu kini ada, dibatasi ke source tanpa perubahan skema |
| Klasifikasi | `HIGH` — jalur penggajian, kontrak API, pengaman Finance Handoff |
| Task mode | `BACKEND` |
| Branch / HEAD | `Yasmina` / `63ad7db73891be0cd39e7b8a8b78bb22637e3c11` (tidak berpindah branch; working tree bersih sebelum mulai) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan (static review). `dotnet build` NOT RUN, `dotnet restore` NOT RUN, migration/`dotnet ef` TIDAK dibuat dan TIDAK dijalankan, uji API/runtime NOT RUN, checker QBE NOT RUN.** Kode belum pernah dikompilasi |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement`; lintas: `HumanResource/PayrollManagement` (Modern), `HumanResource/MasterData/PayrollAndBenefit` (Jenis Tunjangan), `Repositories`, `Services/Features`, `Program.cs` |
| Prefix | Tidak ada entity baru, tidak ada tabel baru |
| Keberlakuan | `NEW CODE` (`PayrollFinanceHandoffGuard`); `TOUCHED LEGACY` (selebihnya) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (endpoint dihapus; tidak ada endpoint baru), `QBE-DTO-001`, `QBE-DB-001/002` (tidak ada perubahan skema) |
| Checker QBE | **NOT RUN** |
| AUTOMATED TEST | `NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)` |

## Prinsip

1. Hapus source operasional BPJS Payroll; **jangan** menyentuh skema, migration, ModelSnapshot, tabel, kolom, FK, indeks, atau data.
2. Entity, DbSet, dan mapping tetap (LEGACY_PERSISTED / BLOCKED_BY_SCHEMA); hanya komentar penanda ditambahkan.
3. Pengaman Finance Handoff **tidak** dilemahkan: dipisahkan dari BPJS dan dipertahankan sampai pemilik proses memutuskan.
4. PPh dan asuransi swasta tidak boleh rusak; snapshot historis tetap terbaca.

## Perubahan

### Dihapus
| Berkas | Alasan |
| --- | --- |
| `Services/PayrollBpjsCalculationService.cs`, `Services/IPayrollBpjsCalculationService.cs` | Mesin hitung BPJS jalur lama; satu-satunya consumer (`PayrollRunModernController`, 2 endpoint) sudah dihapus; endpoint itu sebelumnya selalu ditolak 422 oleh `LegacyModernPayrollGateFilter` |
| `Services/PayrollBpjsPolicyService.cs`, `Controllers/PayrollBpjsPoliciesController.cs` | CRUD/lifecycle kebijakan BPJS; tidak ada consumer tersisa |
| `HumanResource/PayrollManagement/DTOs/PayrollBpjsDtos.cs` | DTO kalkulasi BPJS jalur lama |
| `Program.cs` | 2 registrasi DI (`PayrollBpjsPolicyService`, `IPayrollBpjsCalculationService`) |

### Baru
`Services/PayrollFinanceHandoffGuard.cs` — penahan Finance Handoff produksi yang berdiri sendiri (`financeHandoffHeld`), tetap membaca kunci historis `testingModeWithoutBpjs` / `enableBpjsInPayroll=false` pada snapshot run lama agar run yang sudah ditahan tidak terbuka.

### Direfactor
| Berkas | Perubahan |
| --- | --- |
| `PayrollTaxInsuranceRules` | `AssessInsurance(cfg, start, end)` hanya asuransi swasta; hapus `PayrollInsurancePolicy`, `ParsePrograms`, `AssessWithoutBpjs`, `IsTestingModeWithoutBpjs`; `ValidateInsuranceConfiguration` tanpa BPJS; tambah `AssessInsuranceConfiguration`; `PayrollTaxInsuranceReadiness` → `PayrollTaxReadiness` (PPh saja); `PayrollPolicyAvailability(TaxApproved, TaxVersion)`; snapshot historis dipertahankan |
| `PayrollCalculationService` | Tanpa kebijakan/tarif BPJS, tanpa dasar upah, tanpa penahan klasifikasi; snapshot kebijakan run menulis `financeHandoffHeld`; komponen insurance hanya `PAY_INS_PRIVATE_*` |
| `PayrollRunService`, `PayrollPaymentService`, `PayrollRunDtos` | `IsFinanceHandoffHeld` + `FinanceHandoffHoldReason`; 3 jalur Finance Handoff memakai `PayrollFinanceHandoffGuard.IsHeld`; kelengkapan tidak lagi menilai insurance |
| `PayrollReportService`, `PayrollReportDtos`, `PayrollReportsController` | Tanpa flag mode pengujian; komponen BPJS historis tetap dijumlahkan |
| `PayrollDataReadinessService/Dtos`, `PayrollCompensationService/Dtos`, `PayrollReadinessEvaluator` | Hanya PPh; field `InsuranceConfiguration*`, `HasInsuranceProfile`, status BPJS dihapus; nama historis `TaxInsuranceStatus/NotSet` dipertahankan (= PPh) |
| `PayrollStatutoryPolicyResolver`, `PayrollStatutoryPolicyDtos`, `PayrollTaxPoliciesController` | Tanpa `GetBpjsPolicyAsync`, status/ketersediaan/menu BPJS, `JkkRiskClassOptions`; konstruktor resolver tanpa `IOptions` |
| `PayrollSettingService`, `PayrollSettingDtos`, `PayrollSettingModernController` | DTO/command tanpa 10 parameter BPJS; kolom `HrdPayrollSetting` BPJS tidak pernah ditulis |
| `PayrollSimulationService/Dtos` | Tanpa catatan/flag mode pengujian BPJS |
| `WfpInsuranceController`, `WfpInsuranceDtos` | Kontrak profil asuransi hanya asuransi swasta; kolom BPJS tidak dibaca/ditulis/dikembalikan; klien lama tetap diterima (isian BPJS diabaikan) |
| `AllowanceTypeController/Dtos`, `WfpTransportAllowancePolicyController/Dtos` | Hapus `IsFixedAllowance` dan `IncludeInBpjsWageBase` dari kontrak (kolom entity dipertahankan) |
| `PayrollRunModernController`, `LegacyModernPayrollGateFilter` | Hapus 2 endpoint BPJS + dependency; filter tetap memblokir PPh jalur lama |
| `PayrollPayslipDistributionService` | Label "Iuran Karyawan (asuransi)"; kode `INSURANCE_CONTRIBUTION` |
| `PayrollPolicyOptions`, `BpjsFeatureGate` | Komentar: saklar hanya untuk HRD Benefit dan K3; **tidak lagi memengaruhi Payroll** |

### Dipertahankan (tidak diubah perilakunya)
- `PrlBpjsPolicy`, `PrlInsurance`, `HrdPayrollSetting`, `PrlPayrollRun*`, `MstAllowanceType`, `PrlTransportAllowancePolicy`: entity, mapping, DbSet (komentar LEGACY_PERSISTED saja).
- `IBpjsFeatureGate`, `BpjsFeatureRequired`, `SystemFeaturesController`, `GlobalExceptionMiddleware`: dipakai HRD Benefit (klaim COB/SEP, nomor kepesertaan) dan K3 (JKK) — di luar Payroll.
- `LegacyTaxInsuranceComponentCodes` (termasuk `DED_BPJS_*`): sengaja dipertahankan; tanpanya komponen lama pada run belum final terhitung sebagai potongan manual.
- Daftar program pada `PrlTaxPolicy` (premi pemberi kerja / iuran pensiun) dan `ProgramOptions` metadata: bagian kebijakan PPh, dibiarkan.

## Pengaman Finance Handoff (kritis)

Sebelumnya: run yang dihitung saat saklar BPJS mati ditandai `testingModeWithoutBpjs` dan ditahan. Setelah penghapusan, setiap run baru tidak punya BPJS sama sekali, sehingga tanpa penggantian semua run akan lolos. Maka: snapshot kebijakan run baru menulis `financeHandoffHeld = true` (`PENDING_OWNER_DECISION`); `HandoffAsync`, hasil pembayaran, dan `RetryAsync` menolak dengan pesan netral; `BuildAvailableActions` tidak menawarkan `FinanceHandoff`. Status approval run tidak diubah. **NEEDS BUSINESS DECISION:** aturan pengganti yang melepas penahan.

## Dampak hasil payroll

Perilaku efektif run baru **sama** dengan sebelumnya saat saklar `false` (bawaan sejak PAY-BE-FIX-006): tanpa BPJS, asuransi swasta tetap dihitung, `PrlPayroll.InsuranceAmount` tidak dipakai, PPh tidak berubah. Perbedaan: penahan konfigurasi dan dasar upah BPJS (yang hanya berlaku saat saklar `true`) tidak ada lagi; lingkungan yang menyetel `EnableBpjsInPayroll=true` tidak lagi menghitung BPJS.

## Risiko dan hal terbuka

| # | Risiko / terbuka |
| --- | --- |
| 1 | Kode belum dikompilasi. Perubahan menyentuh ±45 berkas; wajib `dotnet build` oleh pemilik sebelum merge |
| 2 | Baris registry izin lama (`PayrollBpjsPolicy`, `PayrollRunModern.CalculateBpjs*`) mungkin tersisa di DB; tidak memberi akses, hanya yatim |
| 3 | Run belum final yang pernah dihitung dengan BPJS: komponen `PAY_INS_BPJS_*` baru hilang saat hitung ulang; blocker lama pada `ValidationSummaryJson` bertahan sampai dihitung ulang |
| 4 | `HrdPayrollSetting` BPJS: nilai awal entity (`12000000m` dst.) masih ada sebagai inisialisasi properti; menghapusnya butuh keputusan karena baris default baru memakainya |
| 5 | `TaxInsuranceStatus` / `TaxInsuranceNotSet` nama historis dipertahankan demi kontrak FE; ganti nama = pekerjaan terpisah |
| 6 | Alur BPJS di HRD Benefit dan K3 tetap digerbang saklar; hapus/pertahankan = NEEDS BUSINESS DECISION |
| 7 | `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePayrollDataListTests.cs` memanggil konstruktor `PayrollDataReadinessService` 2 argumen (sudah tidak cocok sebelum task ini) |

## BLOCKED_BY_SCHEMA (butuh pekerjaan database terpisah)

`DROP TABLE "PrlBpjsPolicy"`; kolom BPJS pada `PrlInsurance` (+ indeks filter nomor BPJS, indeks gabungan); 10 kolom BPJS `HrdPayrollSetting`; `IsFixedAllowance`, `IncludeInBpjsWageBase` pada `MstAllowanceType` dan `PrlTransportAllowancePolicy`; `InsuranceSnapshotJson`/kolom kontribusi pada `PrlPayrollRun*` (data historis, sebaiknya tidak dihapus).

## Validasi

| Item | Hasil |
| --- | --- |
| `dotnet build` / `restore` | **NOT RUN** |
| Migration / `dotnet ef` | **TIDAK dibuat, TIDAK dijalankan** |
| Uji API / runtime | **NOT RUN** |
| Checker QBE | **NOT RUN** |
| Static review | Dilakukan: sapuan grep simbol terhapus (0 rujukan aktif tersisa), pembacaan ulang area yang diedit, pemeriksaan konsumen DI |
