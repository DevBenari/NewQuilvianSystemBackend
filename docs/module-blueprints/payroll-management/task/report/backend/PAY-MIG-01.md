# Laporan Perubahan Backend — `PAY-MIG-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-MIG-01` |
| Judul | Migration, designer, snapshot manual bila dibutuhkan |
| Slice | Payroll mandiri — schema |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | Prompt Tahap F; `PAYROLL_V2_05` |
| Contract version | Kontrak target pada prompt (`CT-PAY-01..15`, status PROPOSAL); kontrak nyata di `PAYROLL_V2_04` bagian 4 |
| Dependency | PAY-BE-07, PAY-BE-08 (kebutuhan indeks) |
| Klasifikasi | `MEDIUM` — tiga indeks, tetapi berkas Designer sangat besar |
| Task mode | `BACKEND` (lintas repository; FE dikerjakan pada task pasangan `PAY-FE-*`) |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — tiga file konsisten ditulis; belum dijalankan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` (belum terdaftar di registry — lihat `PAYROLL_V2_01` bagian 7) |
| Pemilik/prefix registry | Tidak ada baris; **tidak ada entity operasional baru** pada task ini sehingga `QBE-MOD-002` tidak memblokir |
| Keberlakuan | `NEW CODE` untuk berkas baru; `TOUCHED LEGACY` untuk controller HRD yang diberi rute alias |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-MOD-003`, `QBE-CODE-002/003` |
| Migration / database | Tiga file migration dibuat **manual** atas instruksi eksplisit task; **tidak** dijalankan (`dotnet ef`/update database `NOT RUN`) |

## 1. Masalah yang diperbaiki

Pencegahan duplikasi (satu run Regular per periode, satu instruksi hidup per penerima, satu klaim jasa medis) hanya di kode; dua permintaan bersamaan masih bisa lolos.

## 2. Proses bisnis

Tiga indeks unik parsial ditambahkan di konfigurasi EF dan ditulis sebagai migration manual (`Up` membuat, `Down` menghapus). Migration tidak dijalankan; pihak berwenang menerapkannya setelah menjalankan SQL pra-cek.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` backend, `DATABASE_RULES.md`, `API_RULES.md`, `role-access-rules.md`, kontrak QBE, controller/service/DTO/model Payroll lama, `ApplicationDbContext`, `Program.cs`, tiga layanan handoff HRD, `FinanceReceivablePayrollSyncService`, `MedicalFee*`, serta `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261104071700_AddPayrollIntegrityConstraints.cs` | `Up`/`Down` tiga indeks |
| `Migrations/20261104071700_AddPayrollIntegrityConstraints.Designer.cs` | Atribut `DbContext`/`Migration` + `BuildTargetModel` penuh |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +15 baris (tiga blok `HasIndex`) |
| `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/TrxPayrollRunConfiguration.cs, TrxPayrollPaymentConfiguration.cs, TrxPayrollVariableInputConfiguration.cs` | `HasIndex(...).IsUnique().HasFilter(...).HasDatabaseName(...)` |

### 3.3 Dampak kontrak API, database, dan keamanan

**Database:** tiga indeks unik parsial saja; tidak ada tabel/kolom/FK baru. Risiko data lama dan SQL pra-cek di `PAYROLL_V2_05` bagian 5. **API/keamanan:** tidak berubah.

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
| Konsistensi EF diverifikasi statis | Terpenuhi | `diff` Designer vs Designer lama dan vs snapshot; filter identik di konfigurasi/snapshot/migration (`PAYROLL_V2_05` bagian 4) |
| Tidak ada migration placeholder kosong | Terpenuhi | Designer berisi model penuh |
| Dijalankan terhadap database | Tidak dilakukan | Dilarang pemilik |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `CLAUDE.md` backend mensyaratkan instruksi eksplisit untuk pembuatan migration; dasar di sini adalah task `PAY-MIG-01` pada prompt pemilik. Pemilik dapat membatalkan lewat `PAYROLL_V2_05` bagian 3.4. |
| Masalah yang diketahui | Kesesuaian dengan keluaran `dotnet ef migrations add` sungguhan belum dibuktikan. |
| Perubahan sampingan | `NONE` — tidak ada berkas di luar lingkup yang disentuh |
| Interupsi | `NONE` |
| Status Git | 24 file `M`, 9 file `D` (dipindah keluar dari lokasi lama), 45 file `??` (40 kode/migration + 5 dokumen) — tidak ada stage/commit |
| Langkah berikutnya | Jalankan pra-cek SQL, `has-pending-model-changes`, lalu terapkan di lingkungan pengembangan. |
