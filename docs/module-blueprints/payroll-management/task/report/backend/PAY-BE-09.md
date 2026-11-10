# Laporan Perubahan Backend — `PAY-BE-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-09` |
| Judul | Pembuatan, versi, publikasi slip gaji |
| Slice | Payroll mandiri — slip gaji |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-10/11 |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-08 |
| Klasifikasi | `HEAVY` — banyak berkas, lintas modul (HRD, Finance), menyentuh nominal keuangan |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — snapshot beku, versi, penerbitan, akses per penerima, unduh beraudit; aturan akses `NEEDS CONFIRMATION` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Slip belum punya snapshot immutable, versi, penerbitan terkontrol, akses per penerima, atau jejak unduhan.

## 2. Proses bisnis

Admin membuat slip dari snapshot run (checksum SHA-256; versi naik hanya bila isi berubah dan belum terbit), lalu menerbitkan sesuai pemicu (bawaan: setelah pembayaran berhasil). Pemilik melihat dan mengunduh slipnya; slip orang lain dijawab 404. Setiap unduhan menaikkan penghitung dan mencatat audit.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollPayslipService.cs` | Generate, publish, daftar, detail, unduh |
| `Areas/Corporate/PayrollManagement/Services/PayrollPayslipDocuments.cs` | Dokumen PDF (QuestPDF) dari snapshot |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollPayslipsController.cs, PayrollMyPayslipsController.cs` | Admin dan pemilik (rute self-service tambahan) |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollPayslipDtos.cs` | Snapshot dan respons |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** 5 endpoint admin + 3 endpoint pemilik. **Database:** tidak ada tabel baru (`TrxPayrollPayslip` 1:1 dengan penerima run). **Keamanan:** `PayrollPayslip`, `MyPayslip`; PDF admin dijaga `ViewAmount`.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Payslip** dan **Self Services / Payroll / Payslip**.

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
| Snapshot immutable | Terpenuhi | Isi dibekukan + checksum; slip terbit tidak dibuat ulang |
| Akses per penerima | Terpenuhi | Pemilik dari `ApplicationUser.WorkforceProfileId`; bukan pemilik → 404 |
| Audit unduh | Terpenuhi | `DownloadCount`, waktu pertama/terakhir, log audit |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | Aturan akses slip (kapan terbit) `NEEDS CONFIRMATION`. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Sahkan kebijakan penerbitan slip. |
