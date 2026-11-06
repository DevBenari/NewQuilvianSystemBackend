# Laporan Perubahan Backend — `PAY-BE-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-05` |
| Judul | Approval, lock, reversal dan audit run |
| Slice | Payroll mandiri — persetujuan penggajian |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-06/07; Prompt Tahap B |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-04 |
| Klasifikasi | `HEAVY` — banyak berkas, lintas modul (HRD, Finance), menyentuh nominal keuangan |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — transisi eksplisit, kunci, versi, pembalikan, audit; approval berjenjang `NEEDS CONFIRMATION` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Perpindahan status run tidak punya penjaga versi, kunci konkurensi, pemisahan tugas opsional, penutupan, atau jejak alasan yang bisa ditampilkan.

## 2. Proses bisnis

Run berpindah `Draft → CollectingInput → Calculated → Submitted → Approved (terkunci) → SentToFinance → PartiallyPaid/Paid → Closed`; `Cancelled`/`Reversed` untuk jalur lain. Setiap aksi memakai transaksi serializable dengan kunci per run dan token `expectedRowVersion` (409 bila basi). Pengesahan idempoten; pengesahan oleh pengaju ditolak bila `RequireDifferentApprover` aktif. Pembalikan potongan: ajukan → setujui/tolak.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollRunService.cs` | Aksi Submit/Return/Approve/Cancel/Close/Reversal dengan `PayrollVersionToken` |
| `Areas/Corporate/PayrollManagement/Constants/PayrollRunStatuses.cs` | Helper transisi (`AcceptsInputs`, `IsFinalized`, `AllowsFinanceHandoff`, ...) |
| `Areas/Corporate/PayrollManagement/Services/PayrollExceptions.cs, PayrollVersionToken.cs, PayrollTransactionRunner.cs` | Eksepsi domain → 400/403/409; token versi; transaksi serializable + kunci penasihat |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollControllerSupport.cs` | Pemetaan eksepsi ke kode status baku |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollRunsController.cs` | Aksi `Close` dan penerimaan `expectedRowVersion` |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** `POST {id}/close`; badan aksi menerima `expectedRowVersion`. **Database:** tidak ada. **Keamanan:** izin `PayrollRun : Close`; keputusan kewenangan dan kepemilikan tetap di backend.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Corporate / Payroll Management / Payroll Run** (aksi `submit`, `return`, `approve`, `cancel`, `close`, pembalikan).

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
| Transisi status eksplisit | Terpenuhi | Tabel di `PAYROLL_V2_02` bagian 6; uji statis `payroll-installment-deduction-guards` |
| Data frozen tidak dapat diedit langsung | Terpenuhi | `IsLocked`, `AcceptsInputs`; hitung ulang ditolak |
| Approval berjenjang | Belum terpenuhi | Satu langkah + opsi pemisahan tugas; berjenjang `NEEDS CONFIRMATION` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Approval berjenjang belum ada (kebijakan belum disahkan). |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Sahkan jenjang persetujuan. |
