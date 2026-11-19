# Laporan Perubahan Backend — `PAY-BE-FIX-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-006` (ad-hoc, bukan kartu roadmap) |
| Judul | Penonaktifan sementara, non-destruktif, seluruh konfigurasi dan proses khusus BPJS Kesehatan dan BPJS Ketenagakerjaan (Payroll, HRD/Benefit, K3) dengan satu saklar Backend |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas master prompt pemilik 2026-11-18; roadmap dan traceability tidak disentuh (preseden `PAY-BE-FIX-001..005`) |
| Dependency | Tidak ada DDL, migration, atau perubahan data. Frontend di `PAY-FE-FIX-006` |
| Klasifikasi | `HIGH` — jalur penggajian dan kepesertaan, kontrak API, kewajiban regulasi |
| Task mode | `BACKEND` |
| Branch / HEAD | `Yasmina` / `3492fba4415294140fecd12ab15dd60ddf37fdda` (tidak berpindah branch) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-18 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan. `dotnet build` NOT RUN, `dotnet restore` NOT RUN, migration/`dotnet ef` TIDAK dibuat dan TIDAK dijalankan, uji API/runtime NOT RUN, checker QBE NOT RUN.** Kode belum pernah dikompilasi |

---

## Prinsip

**Penonaktifan non-destruktif.** Tidak ada tabel, kolom, migration, data transaksi, riwayat payroll, audit log, atau snapshot keuangan yang dihapus atau diubah. Nilai BPJS yang tersimpan **dipertahankan** (tidak dinolkan, tidak ditimpa nilai bawaan DTO), tetap dapat dibaca, dan kembali berlaku bila saklar dinyalakan.

Usulan pemilik di tengah sesi ("hapus semua config BPJS") **tidak dilaksanakan**: penghapusan data/kolom tidak dapat dibatalkan, run lama menyimpan snapshot iuran, dan skema butuh migration yang dilarang task ini. Hasil yang terlihat sama sudah dicapai oleh saklar. Penghapusan butuh instruksi tertulis terpisah (lihat §6).

## Saklar tunggal

`Payroll:Policy:EnableBpjsInPayroll` (`PayrollPolicyOptions.EnableBpjsInPayroll`) dipakai ulang, bukan saklar baru. **Bawaan kode `false`** dan `appsettings.json` ditambah `"Payroll": { "Policy": { "EnableBpjsInPayroll": false } }` (Development sudah `false`). Dibaca lewat `IBpjsFeatureGate` (singleton, `IOptions`: nilai tetap selama proses hidup; ubah konfigurasi lalu **mulai ulang** aplikasi). Frontend membacanya lewat `GET /api/v1/system/features`, sehingga Payroll, HRD, K3, dan FE tidak pernah berbeda.

> **Pembalikan keputusan lama:** uji kontrak `payroll-policy.test.mjs` sebelumnya mengunci "default produksi harus true". Sekarang `false` atas keputusan pemilik 2026-11-18; uji diperbarui. Lingkungan yang tidak menyetel `Payroll__Policy__EnableBpjsInPayroll=true` secara eksplisit kini BPJS-nya mati.

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement`, `HumanResource/BenefitManagement`, `HumanResource/OccupationalHealthManagement`, `HumanResource/MasterData/PayrollAndBenefit`; lintas modul: `Services/Features`, `Attributes`, `Middlewares` |
| Prefix | Tidak ada entity baru, tidak ada tabel baru |
| Keberlakuan | `NEW CODE` (gerbang, filter, endpoint fitur); `TOUCHED LEGACY` (selebihnya) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (endpoint fitur hanya `[Authorize]`, mengikuti preseden `VersionController`: bukan data sensitif, dibutuhkan semua modul), `QBE-DTO-001`, `QBE-DB-001/002` (tidak ada perubahan skema) |
| Checker QBE | **NOT RUN** |

## 1. Berkas baru

| Berkas | Isi |
| --- | --- |
| `Services/Features/BpjsFeatureGate.cs` | `IBpjsFeatureGate` (`IsEnabled`, `DisabledMessage`), `BpjsFeatureGate`, `BpjsFeatureDisabledException : InvalidOperationException`, ekstensi `EnsureEnabled()` |
| `Attributes/BpjsFeatureRequiredAttribute.cs` | Filter aksi: saat nonaktif menolak **409** (`ApiResponse.Fail(409, ...)`); `AllowReads = true` meloloskan GET/HEAD/OPTIONS |
| `Controllers/SystemFeaturesController.cs`, `DTOs/System/SystemFeaturesResponse.cs` | `GET /api/v1/system/features` → `{ bpjsEnabled, bpjsDisabledMessage }` |

Perubahan lintas: `Program.cs` (registrasi singleton gerbang), `Middlewares/GlobalExceptionMiddleware.cs` (`BpjsFeatureDisabledException` → 409, bukan 500).

## 2. Audit Backend (klasifikasi → tindakan)

Klasifikasi: `ACTIVE_CONFIG` konfigurasi BPJS aktif · `ACTIVE_WORKFLOW` proses/transaksi BPJS aktif · `SHARED_DEPENDENCY` dipakai bersama jalur non-BPJS · `HISTORICAL_DATA` riwayat · `UNRELATED`.

| # | Berkas / simbol | Kelas | Tindakan | Risiko |
| --- | --- | --- | --- | --- |
| 1 | `PayrollPolicyOptions.EnableBpjsInPayroll`, `appsettings.json` | ACTIVE_CONFIG | Bawaan `false`; saklar eksplisit | Lingkungan tanpa override kini BPJS mati |
| 2 | `PayrollBpjsPoliciesController` (seluruh tulis: buat, ubah, revisi, ajukan, sahkan, tolak, pensiunkan, hapus, `from-setting`) | ACTIVE_CONFIG | `[BpjsFeatureRequired(AllowReads = true)]`: tulis 409, baca tetap | Kebijakan lama tak dapat disahkan sampai saklar nyala |
| 3 | `PayrollBpjsPolicyService` (9 jalur tulis) | ACTIVE_CONFIG | Pertahanan berlapis: `EnsureFeatureEnabled()` di awal setiap transaksi tulis | — |
| 4 | `PayrollSettingService.UpdateSettingAsync` (10 kolom `Bpjs*`) | ACTIVE_CONFIG | Ditulis hanya saat aktif; saat nonaktif nilai tersimpan dipertahankan (command membawa nol bawaan DTO bila FE berhenti mengirim) | Pengaturan non-BPJS tetap dapat disimpan tanpa menimpa BPJS |
| 5 | `WfpInsuranceController` (profil iuran) | ACTIVE_CONFIG | `PreserveBpjsFieldsWhenDisabled` (flag/nomor/tarif/program dari entity, bukan dari request); opsi program BPJS kosong; validasi mengabaikan flag BPJS tersimpan bila status bukan ACTIVE; status konfigurasi dan ringkasan memakai `enableBpjs: gate.IsEnabled` | Profil ACTIVE yang hanya berisi BPJS tetap dinilai sah dari nilai tersimpan |
| 6 | `PayrollTaxInsuranceRules.AssessInsurance` / `AssessWithoutBpjs` / `AssessReadiness` | SHARED_DEPENDENCY | BPJS tidak dihitung (`TESTING_MODE_DISABLED`, bukan `NOT_APPLICABLE`/`VALID_ZERO`/belum dikonfigurasi). **Asuransi swasta tetap dihitung** bila ACTIVE+aktif+berlaku pada periode (sebelumnya ikut hilang). `AssessReadiness` tanpa info ketersediaan kini gagal-tertutup (`?? false`) | Hasil run lama tak berubah (snapshot) |
| 7 | `AllowanceTypeController`, `WfpTransportAllowancePolicyController` (`IsFixedAllowance`, `IncludeInBpjsWageBase`) | ACTIVE_CONFIG | Buat: `null` saat nonaktif; ubah: tidak ditulis; validasi pasangan/aturan hanya saat aktif | Klasifikasi lama tidak berubah |
| 8 | `PayrollCalculationService`, `PayrollRunService`, `PayrollPaymentService`, `PayrollRunModernController` | ACTIVE_WORKFLOW/SHARED | **Tidak diubah.** Mesin sudah mengikuti saklar; aksi Finance Handoff run mode pengujian tetap dihapus/ditolak (pengaman dipertahankan) | Lihat §6 butir 1 |
| 9 | `BenefitClaimController.CreateEssClaim` (`POST api/v1/corporate/hrd/benefits/claims`) | ACTIVE_WORKFLOW | `[BpjsFeatureRequired]`: klaim COB baru (wajib SEP BPJS) 409. Baca, unggah dokumen, verifikasi, penyelesaian klaim lama tidak diubah | Klaim berjalan tak dapat diajukan baru; yang lama tetap diproses lewat API |
| 10 | `BenefitEnrollmentService.UpdatePolicyNumbersAsync` + controller | ACTIVE_CONFIG | Nomor BPJS Kes/TK ditolak 409 saat nonaktif; nomor polis swasta tetap | — |
| 11 | `BenefitPlanService` + `BenefitPlanController` | ACTIVE_CONFIG | Paket kategori `BPJS_Mandatory` tak dapat dibuat/diubah/dialihkan/diberi kelayakan (409); `RequiresBPJSPrimaryCOB` dipertahankan saat ubah; seed tidak membuat paket BPJS dan tidak menyentuh matriksnya; evaluasi kelayakan melewati paket BPJS dan pesan netral | Paket BPJS tetap terbaca di API |
| 12 | `OccupationalInjuryService.ReportInjuryAsync` | SHARED_DEPENDENCY | KAK/PAK tetap dicatat. `InsuranceClaimNumber`, `WorkCompensationClaimNumber`, tenggat 2x24 jam, dan status pelaporan `null` saat nonaktif. `EmployeeInjuryResponseDto.BpjsStage1ReportDeadline` dan `BpjsReportedWithin48Hours` kini **nullable** | Perubahan kontrak (tipe nullable); tidak ada konsumen FE |
| 13 | `GenerateBpjsFormAsync` + `GET injuries/{id}/bpjs-jkk-form` | ACTIVE_WORKFLOW | `[BpjsFeatureRequired]` + `EnsureEnabled()` di service | Lihat §6 butir 5 |
| 14 | `BPJSOccupationalInjuryFollowUpWorker` | ACTIVE_WORKFLOW | Tetap terdaftar; `ExecuteAsync` langsung selesai saat nonaktif (tanpa polling/notifikasi) | Perlu restart saat saklar diubah |
| 15 | `AdmissionQueueOptions` (`IsBpjsServiceAvailable=false`, `RequiresBpjsService=false`), `QueueVoiceService` | UNRELATED | Dipertahankan apa adanya | — |
| 16 | Billing `AdministrationFeeCalculationService` (penjamin BPJS/JKN), pendaftaran, kiosk, nomor "SEP-" pada lifecycle (= nomor separation kepegawaian, bukan SEP BPJS) | SHARED_DEPENDENCY / UNRELATED | **Tidak disentuh** | — |
| 17 | DTO respons `EnableBpjsInPayroll = true` (bawaan DTO) | HISTORICAL_DATA | Tidak diubah: diisi dari options oleh service; bawaan hanya relevan untuk snapshot lama tanpa field itu (BPJS memang ikut) | — |
| 18 | `PrlBpjsPolicy`, kolom BPJS profil iuran (`PrlInsurance`) dan Pengaturan Penggajian, `TrxBenefitClaim.SepBpjsNumber`, nomor BPJS enrollment, `MstBenefitPlan` BPJS, nomor klaim `TrxEmployeeInjury`, snapshot run | HISTORICAL_DATA | **Dipertahankan penuh**, tetap terbaca | — |

## 3. Kontrak API (additive kecuali butir 12)

- `GET /api/v1/system/features` (baru).
- Penolakan 409 `ApiResponse<object>` dengan pesan baku: `PayrollBpjsPoliciesController` (tulis), `POST .../benefits/claims`, `PUT .../enrollments/{id}/policy-numbers` (hanya bila nomor BPJS dikirim), `POST/PUT /api/hr-benefits/plans` untuk paket BPJS, kelayakan grade paket BPJS, `GET .../injuries/{id}/bpjs-jkk-form`.
- `EmployeeInjuryResponseDto`: dua kolom BPJS menjadi nullable.

## 4. Perlindungan data

Tidak ada `DELETE`, `UPDATE` massal, migration, atau DDL. Kolom BPJS tidak pernah ditulis nol/default saat tidak ditampilkan. Tidak ada nomor klaim, nomor kepesertaan, atau angka rekaan yang dibuat. Audit log dan snapshot tidak disentuh.

## 5. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build`, `dotnet restore`, `dotnet ef`, migration, DDL | **NOT RUN** (dilarang) |
| Automated test backend | `AUTOMATED TEST: NOT APPLICABLE` (TEST_POLICY). Kontrak dikunci dari Frontend (`bpjs-feature-deactivation.test.mjs`, membaca source BE) |
| Tinjauan kompilasi lewat pembacaan | Konstruktor/DI yang berubah (`PayrollBpjsPolicyService`, `WfpInsuranceController`, `AllowanceTypeController`, `WfpTransportAllowancePolicyController`, `BenefitEnrollmentService`, `BenefitPlanService`, `OccupationalInjuryService`, `WorkRestrictionsAndInjuryController`, `BPJSOccupationalInjuryFollowUpWorker`): tidak ada instansiasi manual `new` (pencarian), semuanya lewat DI. Posisional `PayrollInsuranceAssessment` (14 argumen), pola properti pada record, `?:` target-typed ke `DateTime?`/`bool?`, urutan `catch` pada middleware diperiksa. **Belum dikompilasi** |
| Uji API/runtime/UAT | **NOT RUN** |

`git status`: tidak ada yang di-stage atau di-commit. Catatan: `WfpTransportAllowancePolicyController.cs` di working tree juga memuat perubahan **bukan dari task ini** (pembangkitan kode berulang, normalisasi tanggal UTC, pemetaan galat DB) yang sudah ada sebelum penyuntingan saya; diff berkas itu lebih besar dari butir 7.

## 6. Risiko dan keputusan tertunda

| # | Hal | Status |
| --- | --- | --- |
| 1 | **Finance Handoff.** Run yang dihitung saat saklar mati ditandai mode pengujian dan **ditahan dari Finance Handoff produksi**. Pengaman tidak dilemahkan dan mode pengujian tidak dijadikan produksi. Bila BPJS memang tidak dibutuhkan untuk produksi, butuh keputusan bisnis yang sah (aturan baru yang membedakan "BPJS ditangguhkan sah" dari "mode pengujian") | NEEDS OWNER ACTION |
| 2 | **Kewajiban hukum.** BPJS adalah kewajiban pemberi kerja menurut regulasi. Menonaktifkan "sementara" tidak menghapus kewajiban; kepatuhan perlu dikonfirmasi pemilik HR/legal sebelum penggajian produksi | NEEDS OWNER ACTION |
| 3 | Bawaan saklar berubah `true → false` (lihat atas). Mengaktifkan kembali: setel `Payroll__Policy__EnableBpjsInPayroll=true` lalu mulai ulang | DIKETAHUI |
| 4 | Klaim COB berjalan: FE menyembunyikan meja kerja klaim (rute dijaga), API verifikasi/penyelesaian tetap hidup. Keputusan: apakah meja kerja perlu mode baca-saja saat nonaktif | NEEDS OWNER ACTION |
| 5 | `GenerateBpjsFormAsync` (aktif) masih berisi nilai contoh tertanam (`BpjsKetenagakerjaanNumber = "0001234567890"`, `ReimbursementAmountClaimed = 15000000m`) dari kode lama. Tidak diubah (di luar cakupan, butuh aturan bisnis), tetapi **harus diperbaiki sebelum saklar dinyalakan** | NEEDS OWNER ACTION |
| 6 | Penghapusan permanen konfigurasi/data BPJS: **tidak dilakukan**; butuh instruksi tertulis, backup, migration terpisah, dan keputusan atas snapshot run lama | NEEDS OWNER ACTION |
| 7 | Saklar dibaca sekali per proses (`IOptions`); perubahan nilai butuh restart | DIKETAHUI |
