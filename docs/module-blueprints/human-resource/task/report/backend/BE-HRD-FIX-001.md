# Laporan Perubahan Backend — `BE-HRD-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-FIX-001` |
| Judul | Migration `AddMissingHrdColumnDefaults`: DEFAULT kolom yang hilang di database |
| Slice | Perbaikan cacat di luar roadmap; penghadang uji API `A52` (`BE-HRD-337`, `BE-HRD-338`) |
| Roadmap | Tidak ada kartu. Dicatat pada `roadmap/backend-roadmap.md` bagian 24 di bawah `BE-HRD-338` |
| Trace | `AT-HRD-PK-01`, `-04`, `-11`–`-14` (terhadang); `POST roster-periods/{id}/assignments` |
| Contract version | `v26` tidak berubah. Perbaikan menyelaraskan database dengan model EF yang sudah ada |
| Dependency | Migration `20261112043425_AddSchedulingAttendanceAmendmentA52` (`BE-HRD-334`) — diterapkan lebih dulu pada hari yang sama |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), sekitar 10 berkas diperiksa (1), satu berkas baru (0), tanpa logika (0), tanpa perubahan API (0), dampak migration (2), tanpa keamanan (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Migrations/` (satu berkas baru); dokumen roadmap/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `400a9678` (branch `yoga`) |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — migration dibuat, diuji kering, dan diterapkan ke devYoga atas pilihan pengguna ("Migration perbaikan + uji"); tambah shift roster terbukti berhasil lewat API |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement`, `LifecycleManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — penyelarasan skema tabel yang sudah ada, tanpa rename |
| QBE yang berlaku | Tidak ada QBE yang terpicu. `QBE-DB-001`/`002` khusus rename `LEGACY MIGRATION`; migration ini tidak me-rename dan tidak memakai DROP+CREATE |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/DATABASE_RULES.md` |
| Wewenang | Pembuatan migration, uji kering, dan penerapan ke devYoga diberikan pengguna secara eksplisit pada 13 November 2026 |

---

## 1. Masalah yang diperbaiki

| Cacat | Akibat |
| --- | --- |
| Model EF memberi `TrxShiftAssignment.IsManualOverride` nilai `HasDefaultValue(false)` dan snapshot memuatnya, tetapi tidak ada migration yang memasang DEFAULT ke tabel | EF tidak mengirim kolom ber-default bila nilainya sama dengan nilai bawaan CLR. PostgreSQL lalu mengisi NULL dan menolak dengan `23502`. **Setiap** tambah shift roster gagal 500. Di devYoga belum pernah ada satu baris pun `TrxShiftAssignment` |
| 15 kolom lain mengalami drift yang sama, kebanyakan `CreateDateTime` di tabel onboarding, kontrak, dan probation | INSERT gagal 500 bila kode lupa mengisi `CreateDateTime` |

Sumber drift: konfigurasi default `TrxShiftAssignment` masuk lewat commit `5d787bc13` (branch SDM, 4 November 2026). Snapshot ikut diperbarui tanpa migration pasangan.

Cara menemukannya: DDL model (`dotnet ef dbcontext script`) dibandingkan dengan `information_schema.columns` devYoga secara baca-saja. Dari 4.361 kolom `NOT NULL DEFAULT` di model, 18 kolom pada 15 tabel tidak punya DEFAULT di database.

---

## 2. Proses bisnis

Kepala Unit menyusun roster ICU dengan menambah shift Pagi dan Malam untuk tiap perawat. Sebelum
perbaikan, langkah pertama ini sudah gagal dengan pesan "Terjadi kesalahan pada server", sehingga
roster tidak pernah bisa disusun, divalidasi, maupun diterbitkan di database yang dibangun dari
migration.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Repositories/Configurations/Corporate/HumanResource/SchedulingManagement/TrxShiftAssignmentConfiguration.cs`
- `Repositories/Configurations/Corporate/HumanResource/LifecycleManagement/MstOnboardingTemplateConfiguration.cs`, `MstOnboardingTemplateTaskConfiguration.cs`, `HrdOnboardingCaseItemConfiguration.cs`
- `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdOnboardingTemplateService.cs` (jalur tulis `IsRequired`, `TargetCompletionDays`, `Version`)
- `Migrations/20260726161839_initializeBigModulHRD2.cs` (kolom dibuat tanpa DEFAULT); `ApplicationDbContextModelSnapshot.cs`
- `Migrations/20260911000000_RenameNutritionGzPrefixToGzi.cs` (pola migration tulisan tangan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261113050000_AddMissingHrdColumnDefaults.cs` | Baru. Migration tulisan tangan dengan atribut `[DbContext]`/`[Migration]`, tanpa Designer. `Up` menjalankan 16 `ALTER COLUMN … SET DEFAULT`; `Down` menjalankan 16 `DROP DEFAULT`. Snapshot tidak diubah karena sudah benar |

Kolom yang diberi DEFAULT:

| Tabel | Kolom | DEFAULT |
| --- | --- | --- |
| `TrxShiftAssignment` | `IsManualOverride` | `false` |
| `HrdClinicalPreceptorship`, `HrdClinicalPreceptorshipPreceptor`, `HrdContractCorrection`, `HrdContractDecision`, `HrdEmploymentLifecycleSetting`, `HrdInitialContractBatch`, `HrdInitialContractBatchRow`, `HrdOnboardingAccessRequest`, `HrdOnboardingCase`, `HrdOnboardingCaseItem`, `HrdProbation`, `HrdProbationAssessment` | `CreateDateTime` | `CURRENT_TIMESTAMP` |
| `MstOnboardingTemplate` | `Version` | `1` |
| `MstOnboardingTemplateTask` | `TaskCategory`, `DueDayOffset` | `'General'`, `0` |

**Sengaja tidak disertakan** (2 dari 18):

| Kolom | Alasan |
| --- | --- |
| `MstOnboardingTemplateTask.IsRequired` (model: `TRUE`) | Diisi dari permintaan pengguna. Dengan DEFAULT `TRUE`, EF tidak mengirim `false`, sehingga butir yang dipilih "tidak wajib" diam-diam tersimpan wajib. Tanpa DEFAULT, kasus itu gagal terang-terangan (`23502`) |
| `MstOnboardingTemplate.TargetCompletionDays` (model: `30`) | Diisi langsung dari permintaan tanpa validasi. Nilai 0 akan diam-diam menjadi 30 |

Perbaikan yang benar untuk keduanya ada di model (sentinel atau tanpa `HasDefaultValue`), bukan di
database. Itu wewenang pemilik LifecycleManagement.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah. `POST roster-periods/{id}/assignments` kembali berfungsi |
| Database | 16 DEFAULT kolom ditambahkan. Tanpa perubahan tipe, nullability, maupun data. **Diterapkan ke devYoga** (riwayat 413 → 414). Server dev bersama dan database lain belum |
| Keamanan/Auth | Tidak berubah |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau yang berubah.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap); 1 menit 26 detik | `PASS` | Keluaran build |
| `dotnet ef migrations list --no-build` | Hanya `20261113050000_AddMissingHrdColumnDefaults` yang `(Pending)` | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Snapshot tetap selaras |
| `migrations script` naik/turun | Naik: 16 `SET DEFAULT` + 1 baris riwayat. Turun: 16 `DROP DEFAULT` + hapus baris riwayat. Tanpa DROP tabel/kolom maupun UPDATE data | `PASS` | `up2.sql`, `down2.sql` di scratchpad sesi |
| Uji kering devYoga: naik → turun → `ROLLBACK` | Riwayat 413 → 414 → 413; kolom ber-DEFAULT 0 → 16 → 0 | `PASS` | Keluaran `defaults-mig.js dry` |
| Penerapan devYoga (`COMMIT`) | Riwayat 414; 16/16 kolom ber-DEFAULT | `PASS` | Keluaran `defaults-mig.js apply` |
| Pembanding drift diulang | Sisa 2 kolom, tepat dua kolom yang sengaja dikecualikan | `PASS` | Keluaran `defaults-drift.js` |
| Kontrol: `POST roster-periods/{id}/assignments` sebelum dan sesudah | Sebelum: 500 (`23502 IsManualOverride`). Sesudah: 200, dua perawat dengan 2 dan 6 shift tersimpan | `PASS` | Log backend; keluaran `at-setup.mjs roster` |
| Startup backend sesudah migration | `/health` 200. Satu-satunya galat adalah `AttendanceScheduler:SystemActorUserId belum dikonfigurasi` yang sudah ada sebelumnya | `PASS` | `be.log` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tambah shift roster tidak lagi gagal 500 | Terpenuhi | Bagian 5, baris kontrol |
| Migration hanya menambah DEFAULT dan dapat dibalik | Terpenuhi | Uji kering naik → turun |
| Tidak ada nilai pilihan pengguna yang dapat tertimpa DEFAULT baru | Terpenuhi | Dua kolom berisiko dikecualikan (bagian 3.2) |
| DoD umum: build lulus, snapshot tidak bergeser | Terpenuhi | Bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) **`HrdOnboardingCaseItem.IsRequired`** sudah ber-DEFAULT `TRUE` di database dan model, sehingga butir template "tidak wajib" yang disalin ke kasus onboarding diam-diam menjadi wajib. Cacat ini sudah ada, tidak disentuh migration ini, dan perlu dilaporkan ke pemilik LifecycleManagement. (2) Dua kolom yang dikecualikan (bagian 3.2) tetap gagal 23502 bila nilainya `false`/`0` |
| Risiko tersisa | Server dev bersama dan produksi belum menerima migration ini. Bila skemanya dibangun dari migration yang sama, tambah shift roster di sana juga gagal 500. `SET DEFAULT` aman diulang pada database yang sudah memiliki DEFAULT |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas migration baru (untracked); laporan ini; dua dokumen roadmap/traceability. Tidak ada stage atau commit |
| Langkah berikutnya | (a) Pemilik LifecycleManagement memperbaiki model `IsRequired`/`TargetCompletionDays` (sentinel). (b) Terapkan migration ini ke server dev bersama bersama migration `A52` |
