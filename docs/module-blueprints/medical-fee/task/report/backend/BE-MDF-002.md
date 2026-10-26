# Laporan Perubahan Backend — `BE-MDF-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-002` |
| Judul | Entity dan configuration daftar peran |
| Slice | `MVP-0` (`EPIC MDF-01`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-003`, `MDF-DES-011`, `MF-DEC-016`; `MDF-VAL-1.0` §1; `erd/data-dictionary.md` bagian 1; `erd/sharing-rule.md` bagian 5 |
| Contract version | `MDF-VAL-1.0`, terkunci 20 September 2026. `MDF-API-1.0` tidak berlaku: task ini tidak membuat endpoint |
| Dependency | `BE-MDF-001` — 🟡 sebagian: baris registry sudah tertulis, commit terpisah oleh pemilik belum dilakukan |
| Klasifikasi | `LIGHT` — satu entity, satu configuration, satu `DbSet`, nol endpoint, nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5.5 (`claude-sonnet-5-5`) |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — kedua acceptance criteria terpetakan ke source, tetapi `dotnet build` dan uji `T-01`..`T-06` belum dijalankan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `MasterData` |
| Pemilik/prefix pada registry | Baris `Mdf` (tertulis di `BE-MDF-001`) untuk modul; entity ini memakai `Mst` karena data induk (`MDF-DES-003`). Sub-folder `MasterData` dipetakan checker ke baris `Mst` — dan prefix `Mst` cocok dengan nama entity |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` (baris `Mdf` ditulis 26 Oktober 2026, belum di-commit) |
| QBE ID yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` |
| QBE ID yang tidak berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada service, endpoint, kode bisnis, maupun migration |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Master data |

---

## 1. Masalah yang diperbaiki

Modul Medical Fee belum punya tempat menyimpan daftar peran pelaksana. Akibatnya "operator" di kamar operasi dan "operator" di tindakan poli tidak punya definisi tunggal, dan baris tarif sharing (`BE-MDF-005`) belum punya tabel peran untuk ditunjuk. Contohnya: peran `OPERATOR` yang dipetakan ke `PrimarySurgeon` milik Operating Room harus bisa dirujuk satu kali oleh seluruh baris tarif, bukan diketik ulang per baris.

---

## 2. Proses bisnis

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan bentuk data daftar peran jasa medis |
| Pelaku | Belum ada pelaku — task ini hanya membentuk entity. Pengelolaan oleh pengguna datang pada `BE-MDF-004` |
| Pemicu | Roadmap `MVP-0`, sesudah `BE-MDF-001` |

Aturan yang kini tertanam pada bentuk data:

1. **Kode peran unik.** Dua peran aktif tidak boleh sama-sama berkode `OPERATOR`. Peran yang sudah dihapus tidak ikut dihitung, jadi kode lama boleh dipakai lagi.
2. **Satu peran Operating Room hanya dipetakan sekali.** Contoh: bila `OPERATOR` sudah memetakan `PrimarySurgeon`, peran kedua tidak boleh memetakan `PrimarySurgeon` juga. Peran tanpa padanan di kamar operasi (kolom pemetaan kosong) boleh banyak.
3. **Peran dinonaktifkan, tidak dihapus.** Kolom `IsActive` ada untuk itu; penghapusan permanen tidak pernah terjadi karena entity mewarisi `IdentityModel` (soft delete).

Jalur tidak normal: kedua aturan keunikan baru ditegakkan basis data **setelah** migration `BE-MDF-003` dibuat dan dijalankan. Sampai saat itu keduanya hanya definisi pada configuration.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/erd/data-dictionary.md` bagian 1 | Kolom, tipe, dan index |
| `docs/module-blueprints/medical-fee/contracts/validation-matrix.md` bagian 1 | Aturan 1.1–1.7 |
| `docs/module-blueprints/medical-fee/02-backend-architecture.md` bagian 4.1 dan 5 | Lokasi berkas, catatan desain |
| `docs/module-blueprints/medical-fee/erd/sharing-rule.md` bagian 5 | Invariant pemetaan |
| `Models/IdentityModel.cs` | Kolom audit yang diwarisi |
| `Areas/HealthServices/OperatingRoomManagement/Enums/OperatingRoomEnums.cs` | Nilai `OprTeamRole` yang dipetakan |
| `McuExaminationType.cs` dan `McuExaminationTypeConfiguration.cs` | Pembanding terdekat (master, partial unique index, `IsActive`) |
| `Repositories/ApplicationDbContext.cs` | Pola `DbSet` dan `#region` per modul |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` baris 340–426 | Cara checker memutuskan pemilik |
| `Migrations/20260930120000_AddMasterDailyNursingAction.cs` | Preseden risiko `PendingModelChangesWarning` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MstMedicalFeeRole.cs` | **Baru.** Entity `MstMedicalFeeRole : IdentityModel` dengan `Id`, `RoleCode` (30), `RoleName` (100), `OprTeamRoleMapping` (30, boleh kosong), `IsPrimaryRole` (bawaan `false`), `Description` (500, boleh kosong), `IsActive` (bawaan `true`) |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MstMedicalFeeRoleConfiguration.cs` | **Baru.** Tabel `public."MstMedicalFeeRole"`; unique index `RoleCode` dan `OprTeamRoleMapping`, keduanya parsial `WHERE "IsDelete" = false` (yang kedua juga `AND "OprTeamRoleMapping" IS NOT NULL`); index `IsActive` |
| `Repositories/ApplicationDbContext.cs` | Satu `using`, satu `#region MEDICAL FEE MANAGEMENT`, dan `DbSet<MstMedicalFeeRole> MstMedicalFeeRoles` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint. Sembilan endpoint baseline master data dikerjakan `BE-MDF-004` sesuai roadmap, bukan di slice ini |
| Database | Model EF bertambah satu tabel, tetapi **migration belum dibuat** (`BE-MDF-003`, butuh otorisasi terpisah) dan nol operasi database. Lihat peringatan di bagian 7 |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint, tidak ada hardcode role |

### 3.4 Selisih terhadap blueprint

| Butir | Blueprint | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Navigation `MdfSharingRule` | `02-backend-architecture.md` 4.1: "Punya banyak `MdfSharingRule`" | Belum ada | Entity `MdfSharingRule` baru lahir pada `BE-MDF-005`; navigation ditambahkan di sana |
| `DbSet` | Roadmap menyebut sembilan `DbSet` di `MVP-0` tanpa menunjuk task | Satu `DbSet` ditambahkan di task ini | Kontrak engineering menyebut entity, configuration, dan `DbSet` satu paket; delapan sisanya mengikuti task entity masing-masing |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Pemilik meminta **tanpa `dotnet build` otomatis** pada task ini |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang di mesin ini |
| Review diff dan cakupan | 3 berkas kode (2 baru, 1 diubah), nol migration, nol berkas di luar scope | `PASS` | `git status --short`, `git diff -- Repositories/ApplicationDbContext.cs` |
| Pencarian `Trx`, `SortOrder`, `IsInRole` pada folder modul | Nol hasil | `PASS` | `grep` pada `Areas/HealthServices/MedicalFeeManagement` |
| Bentrok nama `MstMedicalFeeRole` / `MedicalFeeManagement` di source lain | Nol hasil sebelum penulisan | `PASS` | `grep` pada seluruh `*.cs` kecuali `bin`/`obj` |
| Akhir baris berkas baru mengikuti repository | LF, sama dengan `ApplicationDbContext.cs` | `PASS` | `grep -c $'\r'` bernilai 0 |
| Pemilik hasil checker atas jalur berkas | `MasterData` cocok baris `Mst` pada kedalaman 1 (mengalahkan `Mdf` kedalaman 0); `MstMedicalFeeRole` berawalan `Mst` sehingga lolos | `PASS` (dibaca dari kode, belum dijalankan) | `Invoke-QbeConformanceCheck.ps1` baris 382–426 |
| `T-01`..`T-06` | Tidak dijalankan | `NOT RUN` | Menuntut layanan/API (`BE-MDF-004`) dan tabel hasil migration (`BE-MDF-003`); keduanya belum ada |
| Migration dibuat | Tidak | `NOT APPLICABLE` | `ls Migrations` tidak memuat `MedicalFee`/`Mdf`; wewenang migration terpisah |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:** `dotnet build` (permintaan pemilik), checker QBE (tidak ada `pwsh`), dan `T-01`..`T-06` (prasyarat belum ada). **Kode ini belum pernah dikompilasi.** Saya menulisnya mengikuti `McuExaminationType` dan `McuExaminationTypeConfiguration` baris per baris, tetapi itu bukan pengganti build.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "`RoleCode` unik" | Terpetakan ke source; **belum terbukti pada basis data** | `MstMedicalFeeRoleConfiguration.cs` — unique index `RoleCode` parsial `WHERE "IsDelete" = false`. Penegakan nyata menunggu `BE-MDF-003` |
| Acceptance: "satu `OprTeamRole` dipetakan paling banyak satu peran" | Terpetakan ke source; **belum terbukti pada basis data** | Unique index `OprTeamRoleMapping` parsial `WHERE "IsDelete" = false AND "OprTeamRoleMapping" IS NOT NULL`. Penegakan nyata menunggu `BE-MDF-003` |
| Verifikasi: `T-01`..`T-06` | **Belum terpenuhi** | `NOT RUN` — lihat bagian 5 |
| DoD: "Prefix `Mst` untuk data induk" | Terpenuhi | `MstMedicalFeeRole` berawalan `Mst`; tabel `public."MstMedicalFeeRole"` |
| DoD umum #2: entity mewarisi `IdentityModel`; nol hard delete | Terpenuhi untuk entity ini | `MstMedicalFeeRole : IdentityModel`; tidak ada endpoint hapus |
| DoD umum #9: partial unique index memakai `WHERE "IsDelete" = false` | Terpenuhi untuk entity ini | Kedua index unique memuat filter itu |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas permintaan pemilik |

Butir yang belum terpenuhi disebut apa adanya; karena itu status `🟡`, bukan `✅`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **`PendingModelChangesWarning`.** Model EF kini memuat `MstMedicalFeeRole` tetapi snapshot migration belum. Preseden di repo: commit `06678c61` (`MstDailyNursingAction`) menambah entity tanpa migration sehingga EF 9 menolak `dotnet ef database update`. Selama `BE-MDF-003` belum dibuat, `database update` pada branch ini akan menolak jalan. Aplikasi sendiri tidak memanggil `Migrate()` saat startup dan belum ada seeder yang menyentuh tabel ini. **Jangan menggabungkan task ini ke branch bersama tanpa `BE-MDF-003`** |
| Peringatan | **Bentrok sub-folder `MasterData` untuk `BE-MDF-005`** (dari laporan `BE-MDF-001`) tetap terbuka. `BE-MDF-002` tidak terdampak karena awalan entity-nya `Mst` |
| Masalah yang diketahui | Kode belum dikompilasi. Pemilik disarankan menjalankan `dotnet build` sendiri sebelum commit |
| Risiko tersisa | Pengujian `T-01`..`T-06` baru bisa dilakukan sesudah `BE-MDF-003` dan `BE-MDF-004` |
| Perubahan sampingan | `NONE` dari task ini. Catatan: berkas `docs/module-blueprints/medical-fee/evidence/04-balasan-owner-billing.md` berstatus untracked dan **bukan** dari task ini; tidak disentuh |
| Interupsi | `NONE` |
| Status Git | `M Repositories/ApplicationDbContext.cs`, `?? Areas/HealthServices/MedicalFeeManagement/`, `?? Repositories/Configurations/HealthServices/MedicalFeeManagement/`, serta perubahan dokumen `BE-MDF-001` yang belum di-commit |
| Traceability | `requirement-traceability.md` tidak ada pada blueprint ini; tidak ada baris yang bisa diperbarui |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build`. (2) Putuskan penanganan bentrok `MasterData` sebelum `BE-MDF-005`. (3) `BE-MDF-005`, lalu `BE-MDF-007`, `BE-MDF-018`, dan migration `BE-MDF-003` dengan otorisasi terpisah |
