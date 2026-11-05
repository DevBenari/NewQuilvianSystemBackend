# Laporan Perubahan Backend — `PAY-BE-07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-07` |
| Judul | Integrasi hasil jasa medis yang sudah approved |
| Slice | Payroll mandiri — remunerasi dokter |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-14; Prompt Tahap D (jalur dokter) |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-04 |
| Klasifikasi | `HEAVY` — banyak berkas, lintas modul (HRD, Finance), menyentuh nominal keuangan |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — jalur tunggal terjaga; jalur Payroll nonaktif bawaan (`NEEDS CONFIRMATION`) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Persetujuan periode Medical Fee otomatis membuat handoff Finance AP untuk setiap hak jasa; menambah jalur Payroll tanpa pagar membayar jasa dua kali.

## 2. Proses bisnis

Staf melihat posisi tiap hak jasa (Payroll, AP, belum punya jalur, bukan penerima payroll). Bila kebijakan jalur Payroll diaktifkan, hanya hak berstatus `Approved` tanpa catatan handoff AP yang dapat diklaim ke run terbuka sebagai input variabel `MFI-<NomorFee>`. Klaim ganda ditolak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollMedicalFeeIntakeService.cs` | Posisi jalur, klaim satu kali |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollMedicalFeeIntakeController.cs` | `GET /`, `POST /` |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollMedicalFeeDtos.cs` | Kontrak posisi jalur dan klaim |
| `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/TrxPayrollVariableInputConfiguration.cs` | Indeks unik `UX_TrxPayrollVariableInput_MedicalFeeClaim` |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** 2 endpoint. **Database:** 1 indeks unik parsial (lihat `PAY-MIG-01`). **Keamanan:** resource `PayrollMedicalFeeIntake`. Modul Medical Fee **tidak diubah**.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Medical Fee Intake**.

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
| Satu SourceId tidak dibayar ganda | Terpenuhi (statis) | Pagar berlapis + indeks unik |
| Pembeda jalur Payroll vs AP | Terpenuhi | Enum posisi `PAYROLL_PATH_CLAIMED`/`AP_PATH_ACTIVE`/... |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Pajak jasa medis dan aktivasi jalur Payroll `NEEDS CONFIRMATION`. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Putuskan jalur honor dokter (kontrak/nonpegawai) dan perlakuan pajak. |
