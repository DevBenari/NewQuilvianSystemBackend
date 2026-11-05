# Laporan Perubahan Backend — `PAY-BE-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-02` |
| Judul | Pisahkan master penggajian dari master HRD |
| Slice | Payroll mandiri — master data dan profil |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | Prompt tabel ownership; `PAYROLL_V2_01` bagian 5 |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-01 |
| Klasifikasi | `MEDIUM` — 17 controller disentuh dengan pola sama, tanpa logika baru |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | 🟡 Sebagian — rute dan tag Payroll tersedia; modul akses dan folder fisik belum dipindah |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Sepuluh master penggajian, enam controller profil `Wfp*`, dan penetapan gaji hanya bisa dipanggil lewat rute HRD sehingga frontend Payroll tidak punya alamat Payroll sendiri.

## 2. Proses bisnis

Setiap controller mendapat `[Route("api/v1/corporate/payroll-management/...")]` di atas rute lama dan `[Tags("Corporate / Payroll Management / ...")]`. Pengguna Payroll membuka master data di menu Payroll; pemanggil lama tetap bekerja.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/{SalaryGrade,SalaryStructure,PayrollPeriod,PayrollComponent,PayrollComponentCategory,AllowanceType,DeductionType,ShiftAllowancePolicy,OnCallAllowancePolicy,HazardAllowancePolicy}Controller.cs` | Rute alias `payroll-management/master-data/...` + tag |
| `Areas/Corporate/HumanResource/PayrollManagement/Controllers/Wfp{Payroll,Tax,Insurance,TransportAllowance,TransportAllowancePolicy,TransportAllowanceTransaction}Controller.cs` | Rute alias `payroll-management/...` + tag |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpSalaryAssignmentController.cs` | Rute alias salary-assignments + tag |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** penambahan rute, tidak ada pembuangan. **Database:** tidak ada perubahan; FK ke Workforce dan Benefit tetap. **Keamanan:** `moduleCode` Akses Role (`HUMAN_RESOURCE_MASTER_DATA`, `HUMAN_RESOURCE_PAYROLL_MANAGEMENT`, `HUMAN_RESOURCE_WORKFORCE_CORE`) sengaja **tidak** diubah karena seeder memakai kunci `(ModuleCode)` dan `(ModuleId, ControllerName)`; mengubahnya akan memutus grant peran yang ada.

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
| FK Workforce/Benefit tetap valid | Terpenuhi | Tidak ada perubahan entity/konfigurasi relasi |
| Role Payroll dapat mengakses | Terpenuhi (statis) | Izin diatur admin di Akses Role; nama resource/aksi tidak berubah |
| Pindah ownership master | Sebagian | Ownership di tingkat API/UI; folder dan modul akses fase 2 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Modul akses tetap `HUMAN_RESOURCE_*` sampai ada keputusan migrasi grant. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Rencanakan pemindahan modul akses bersama fase 2 dengan skrip migrasi grant yang disetujui. |
