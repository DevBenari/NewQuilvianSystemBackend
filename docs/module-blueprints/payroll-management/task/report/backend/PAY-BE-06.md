# Laporan Perubahan Backend — `PAY-BE-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-06` |
| Judul | Integrasi piutang karyawan: jadwal dan realisasi |
| Slice | Payroll mandiri — potongan dan penyesuaian |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-12/13; Prompt Tahap C |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-04, PAY-BE-05 |
| Klasifikasi | `HEAVY` — banyak berkas, lintas modul (HRD, Finance), menyentuh nominal keuangan |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | 🟡 Sebagian — jadwal, batas potong, konfirmasi, rekonsiliasi, restitusi ada; satu angsuran per pegawai per run dan titik pengakuan belum disahkan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |

## 1. Masalah yang diperbaiki

Tidak ada tampilan riwayat potongan dan rekonsiliasi per periode; batas 50% tertanam; restitusi dapat menggandakan nilai; pelunasan tidak boleh diakui dari draf atau preview.

## 2. Proses bisnis

Angsuran jatuh tempo ditarik ke run; potongan dihitung setelah gaji tersedia dengan batas persentase dari master; hasil dikonfirmasi ke Finance hanya saat run disahkan (idempoten). Rekonsiliasi per periode membandingkan jadwal, rencana, terpotong, dibalik, dan sisa di Finance tanpa membayar ulang. Restitusi disematkan sekali pada run tujuan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollInstallmentDeductionService.cs` | Batas potong dari `PayrollPolicyOptions`; baris potongan `AvailableNetSalary` |
| `Areas/Corporate/PayrollManagement/Services/PayrollEmployeeDeductionService.cs` | Riwayat, ringkasan, rekonsiliasi (kode `NOT_IN_RUN`..`APPLIED_WITHOUT_PLAN`), daftar restitusi |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollEmployeeDeductionsController.cs` | 3 endpoint |
| `Areas/Corporate/PayrollManagement/Services/PayrollRestitutionService.cs` | Restitusi sekali; `TIDAK_ADA_PAYROLL`; tanpa mutasi total ganda |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollRestitutionController.cs` | `GET /` daftar restitusi (baru) |

### 3.3 Dampak kontrak API, database, dan keamanan

**API:** `employee-receivable-deductions` (daftar, ringkasan, rekonsiliasi), `GET payroll-restitutions`. **Database:** tidak ada. **Keamanan:** resource `PayrollEmployeeDeduction`, `PayrollRestitution : Read`; rekonsiliasi dicatat sebagai audit siapa yang menjalankan.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Employee Receivable Deduction** dan **Restitution**.

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
| Idempotent | Terpenuhi (statis) | Kunci `FIN_INSTALLMENT_PAYROLL_RESULT_{id}`; uji `payroll-installment-deduction-guards` |
| Batas potong dan outstanding konsisten | Terpenuhi (statis) | `maxAllowed` = persen × gaji bersih tersedia, dibatasi nominal maksimum; sisa dari Finance |
| Tidak premature lunas | Terpenuhi | Hitung/ajukan tidak memanggil Finance; konfirmasi hanya saat disahkan |
| Semua angsuran pegawai pada periode terpotong | Belum terpenuhi | Hanya satu angsuran per pegawai per run (R1) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru selain batas pembuktian di atas |
| Masalah yang diketahui | R1 (`FirstOrDefault`); titik pengakuan pelunasan `NEEDS CONFIRMATION`. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Perbaiki dukungan banyak angsuran per penerima bersama pemilik Finance; sahkan titik pengakuan. |
