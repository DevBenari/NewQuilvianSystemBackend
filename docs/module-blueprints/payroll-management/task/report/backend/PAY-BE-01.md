# Laporan Perubahan Backend — `PAY-BE-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-01` |
| Judul | Audit dependency dan pindah bounded context Payroll |
| Slice | Payroll mandiri — pemindahan bertahap |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | Prompt Tahap A; `PAYROLL_V2_01`; keputusan pemilik: "Bertahap, aman QBE" |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | Tidak ada (task pertama) |
| Klasifikasi | `HEAVY` — banyak berkas, lintas modul (HRD, Finance), menyentuh nominal keuangan |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | 🟡 Sebagian — kode non-persisted dan seluruh kode baru sudah di modul Payroll; entity dan konfigurasi EF menunggu fase 2 |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Payroll hidup di folder HRD (`Areas/Corporate/HumanResource/PayrollManagement`) sehingga sulit dikelola mandiri. Memindahkan semua file sekaligus akan memicu kegagalan CI QBE karena file yang dipindah diperlakukan sebagai kode baru (`QBE-NAM-001`, `QBE-MOD-002`).

## 2. Proses bisnis

Pemilik repository memilih pemindahan bertahap. **Sekarang:** controller, service, DTO, dan konstanta Payroll Run, Restitusi, serta Potongan Cicilan dipindah dengan `mv` ke `Areas/Corporate/PayrollManagement`; namespace, `using`, dan DI disesuaikan. **Fase 2 (usulan):** entity `Trx*`/`Wfp*` dan konfigurasi EF dipindah setelah registry dan pengecualian disetujui manusia. Tidak ada dua sumber kebenaran: tidak ada file yang disalin.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollRunsController.cs, PayrollRestitutionController.cs` | Dipindah dari HRD; rute baru + alias lama |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs, PayrollRestitutionService.cs, PayrollInstallmentDeductionService.cs` | Dipindah; namespace baru |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollRunDtos.cs, PayrollRestitutionDtos.cs, PayrollInstallmentDeductionDtos.cs` | Dipindah; namespace baru |
| `Areas/Corporate/PayrollManagement/Constants/PayrollRunStatuses.cs` | Dipindah dari `Models/` |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | `using` ke namespace baru |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivablePayrollSyncService.cs` | `using ...PayrollManagement.Constants` |
| `Program.cs` | `using` baru, opsi kebijakan, dan registrasi 11 layanan Payroll |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** rute lama tetap hidup (alias) sehingga pemanggil lama tidak putus. **Database:** nama tabel/kolom tidak berubah → tidak memerlukan migration. **Keamanan:** kunci `AccessController` (`PayrollRun`, `PayrollRestitution`) tidak berubah sehingga hak akses yang sudah diberikan tetap berlaku.

## 4. Dokumentasi endpoint

Tidak menambah atau mengubah endpoint (`NOT APPLICABLE`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `verify_be.py` (analisis statis kustom: using, arity, atribut akses, nama tipe ganda, bentrok rute, namespace kosong) | 0 kesalahan, 0 peringatan (38 file Payroll, 16 controller) | `PASS` | keluaran skrip, dijalankan ulang setelah perubahan terakhir |
| `dotnet build` pada project aplikasi | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | - |
| Tes otomatis backend | Tidak ada project tes otomatis di backend (kebijakan repository); tidak dibuat | `NOT APPLICABLE` | `rules/backend/TEST_POLICY.md` |
| Penjaga statis dari sisi FE membaca source BE (`payroll-installment-deduction-guards`, `payroll-run-screens`, `payroll-module-contract`) | 6 + 11 + 12 uji lulus | `PASS` | `node --test` pada berkas tes |

**Tidak dijalankan:** `dotnet restore/build/test`, `dotnet ef ...`, eksekusi database, server/API — dilarang pemilik. Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada duplikasi controller/DbSet/tabel | Terpenuhi | Satu `PayrollRunsController` dengan `[Route]` ganda; tidak ada DbSet/tabel baru; `verify_be.py` (bentrok rute 0, nama tipe ganda 0) |
| Namespace dan DI konsisten | Terpenuhi (statis) | `Program.cs`; `verify_be.py` 0 kesalahan |
| Pindahkan model yang dimiliki Payroll | Belum terpenuhi | 19 entity `Trx*`/`Wfp*` belum dipindah — menunggu registry (`PAYROLL_V2_01` bagian 7) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Fase 2 belum dikerjakan; draf registry, pengecualian, dan skrip ada di `PAYROLL_V2_01` bagian 7 (tidak diterapkan). |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Pemilik repository memutuskan registry/pengecualian QBE, lalu fase 2 dijalankan oleh pihak yang boleh membangun. |
