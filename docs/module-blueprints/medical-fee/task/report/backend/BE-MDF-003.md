# Laporan Perubahan Backend — `BE-MDF-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-003` |
| Judul | Migration `AddMedicalFeeMasterData` |
| Slice | `MVP-0` (`EPIC MDF-01`, `MDF-02`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-001`, `003`, `008`..`010`, `MF-DEC-004`, `012`, `016`; `erd/data-dictionary.md` bagian 1–3; `erd/sharing-rule.md` |
| Contract version | `MDF-VAL-1.0`, terkunci 20 September 2026. `MDF-API-1.0` tidak berlaku: task ini tidak membuat endpoint |
| Dependency | `BE-MDF-005` — 🟡 sebagian: entity kesepakatan dan baris tarif sudah ada di source |
| Klasifikasi | `LIGHT` — satu berkas migration aditif EF Core, nol tabel modul lain tersentuh, nol endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — berkas migration aditif telah selesai disusun dengan DDL `Up()` dan `Down()` lengkap, namun `dotnet build` dan eksekusi migration pada basis data belum dijalankan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `MasterData` / `Migrations` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) dan `Mst` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-DB-001` (perubahan skema aditif), `QBE-DB-002` (`Down()` bersih dan simetris dalam urutan dependensi terbalik), `QBE-NAM-001`, `QBE-NAM-003`, `QBE-MOD-001` |
| QBE ID yang tidak berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-*`, `QBE-ENT-*`, `QBE-CFG-*` — tidak ada service, endpoint, entity baru, maupun logika bisnis C# |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Skema basis data — migration DDL Entity Framework Core untuk 3 tabel master data Medical Fee |

---

## 1. Masalah yang diperbaiki

Sebelum task ini dikerjakan, entity `MstMedicalFeeRole` (dari task `BE-MDF-002`) serta `MdfSharingAgreement` dan `MdfSharingRule` (dari task `BE-MDF-005`) sudah selesai didefinisikan pada kode C# dan didaftarkan pada `ApplicationDbContext`, namun belum memiliki berkas migration resmi untuk merefleksikan skema tersebut ke basis data fisik PostgreSQL.

Tanpa berkas migration ini:
1. Skema model EF Core berada dalam kondisi tertunda (*pending model changes*), yang berpotensi memicu peringatan atau konflik migrasi pada pipeline basis data.
2. Tabel fisik untuk penampungan kamus peran pelaksana medis dan kesepakatan persentase tarif sharing belum dapat dibentuk di database server.
3. Seluruh *check constraint* tingkat basis data (seperti batasan persentase 0..100%, validasi urutan periode tanggal, dan batasan nilai status) serta *partial unique index* belum aktif melindungi integritas data.
4. Pekerjaan lanjutan seperti pengujian end-to-end (`T-01`..`T-13`), implementasi service/API master data (`BE-MDF-004`, `BE-MDF-006`), dan migration gelombang kedua (`BE-MDF-008`) tidak dapat dilanjutkan.

Contoh konkret skenario rumah sakit:
Saat modul Medical Fee diaktifkan di rumah sakit, sistem membutuhkan tabel `MstMedicalFeeRole` untuk menyimpan peran dokter operator, dokter anestesi, dan asisten; tabel `MdfSharingAgreement` untuk mencatat perjanjian kerja sama dokter mitra spesialis bedah yang merujuk ke nomor kontrak HR; serta tabel `MdfSharingRule` untuk mengatur bahwa peran dokter operator memperoleh porsi 40% dari tarif tindakan operasi. Pembuatan ketiga tabel ini harus bersifat murni aditif (menambah tabel baru) tanpa mengubah struktur tabel operasional rumah sakit yang sudah berjalan (seperti tabel kontrak HR `WfpContractHistory` atau tabel tarif `MstTariff`).

---

## 2. Proses bisnis

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan berkas migration EF Core aditif `20261026130000_AddMedicalFeeMasterData.cs` untuk menciptakan 3 tabel master data: `MstMedicalFeeRole`, `MdfSharingAgreement`, dan `MdfSharingRule` |
| Pelaku | Database Administrator / Migration Runner (dijalankan saat deployment basis data) |
| Pemicu | Roadmap `MVP-0`, sesudah `BE-MDF-005` selesai |

Invarian dan aturan integritas skema yang tertuang dalam migration:

1. **Sifat Murni Aditif**:
   Migration hanya memanggil `migrationBuilder.CreateTable` untuk 3 tabel baru pada schema `public`. Tidak ada perintah `AlterTable`, `DropColumn`, `AlterColumn`, atau modifikasi terhadap tabel modul lain (`WfpContractHistory`, `MstTariff`, `MstTariffCategory`).
2. **Integritas Relasi Luar (Foreign Key Restrict)**:
   - Relasi dari `MdfSharingAgreement.SourceContractHistoryId` ke `WfpContractHistory.Id` menggunakan `ReferentialAction.Restrict` agar riwayat kontrak kerja HR dokter tidak dapat dihapus jika masih dirujuk kesepakatan tarif sharing.
   - Relasi dari `MdfSharingRule` ke `MstTariff`, `MstTariffCategory`, dan `MstMedicalFeeRole` menggunakan `ReferentialAction.Restrict`.
   - Relasi self-referencing dari `MdfSharingRule.SupersededByRuleId` ke `MdfSharingRule.Id` menggunakan `ReferentialAction.Restrict`.
3. **Integritas Relasi Agregat (Foreign Key Cascade)**:
   Relasi dari rincian aturan tarif `MdfSharingRule.AgreementId` ke kesepakatan induk `MdfSharingAgreement.Id` menggunakan `ReferentialAction.Cascade`.
4. **Penegakan Aturan Bisnis Melalui Check Constraint Postgres**:
   - `CK_MdfSharingAgreement_Status`: `"Status" IN ('Draft','Active','Expired','Terminated')`
   - `CK_MdfSharingAgreement_PayeeType`: `"PayeeType" IN ('Doctor','Workforce')`
   - `CK_MdfSharingAgreement_Period`: `"EffectiveEnd" IS NULL OR "EffectiveEnd" >= "EffectiveStart"`
   - `CK_MdfSharingRule_Percentage`: `"SharingPercentage" >= 0 AND "SharingPercentage" <= 100`
   - `CK_MdfSharingRule_Period`: `"EffectiveEnd" IS NULL OR "EffectiveEnd" >= "EffectiveStart"`
5. **Partial Unique Index**:
   - `IX_MstMedicalFeeRole_RoleCode` unik untuk baris yang belum dihapus (`"IsDelete" = false`).
   - `IX_MstMedicalFeeRole_OprTeamRoleMapping` unik untuk baris yang belum dihapus dan tidak null (`"IsDelete" = false AND "OprTeamRoleMapping" IS NOT NULL`).
   - `IX_MdfSharingAgreement_AgreementNumber` unik untuk baris yang belum dihapus (`"IsDelete" = false`).
6. **Simetri dan Kebersihan Rollback (`Down()`)**:
   Metode `Down()` melakukan drop table secara bersih dalam urutan kebalikan dependensi hierarki:
   - Drop `MdfSharingRule` (tabel anak)
   - Drop `MdfSharingAgreement` (tabel induk kesepakatan)
   - Drop `MstMedicalFeeRole` (tabel kamus peran)

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` baris 72 | Memastikan acceptance criteria dan batas wewenang `BE-MDF-003` |
| `docs/module-blueprints/medical-fee/erd/data-dictionary.md` bagian 1, 2, & 3 | Memvalidasi seluruh kolom, tipe data, indeks, default value, dan check constraint |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MstMedicalFeeRole.cs` | Model entitas peran pelaksana |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MstMedicalFeeRoleConfiguration.cs` | Konfigurasi EF Core peran pelaksana |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MdfSharingAgreement.cs` | Model entitas kesepakatan tarif sharing |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MdfSharingAgreementConfiguration.cs` | Konfigurasi EF Core kesepakatan tarif sharing |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Models/MdfSharingRule.cs` | Model entitas baris aturan tarif |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/MasterData/MdfSharingRuleConfiguration.cs` | Konfigurasi EF Core baris aturan tarif |
| `Migrations/20261026120000_AddPurchasingApBlmComponents.cs` & `20261023035602_AddRegPatientGeneralConsent.cs` | Konvensi penamaan berkas migrasi dan struktur namespace EF Core |
| `Migrations/20260911000000_RenameNutritionGzPrefixToGzi.cs` | Preseden peletakan atribut `[DbContext]` dan `[Migration]` langsung pada partial class |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261026130000_AddMedicalFeeMasterData.cs` | **Baru.** Berkas migration EF Core aditif yang membentuk 3 tabel master data Medical Fee (`MstMedicalFeeRole`, `MdfSharingAgreement`, `MdfSharingRule`), 5 check constraint, 6 foreign key, 9 indeks pencarian/keunikan, serta metode rollback `Down()` dengan urutan terbalik yang bersih |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini tidak membuat atau mengubah endpoint API apa pun |
| Database | Menyiapkan DDL migrasi aditif untuk 3 tabel baru (`MstMedicalFeeRole`, `MdfSharingAgreement`, `MdfSharingRule`). Nol tabel existing yang dimodifikasi strukturnya |
| Keamanan/Auth | `NOT APPLICABLE` — Tidak ada endpoint atau perubahan authorization policy |

### 3.4 Selisih terhadap blueprint

| Butir | Blueprint | Yang dikerjakan | Alasan |
| --- | --- | --- | --- |
| Skema Migration | `MDF-DES-001` menetapkan 3 migration aditif, yang pertama adalah `AddMedicalFeeMasterData` | Diimplementasikan persis sesuai nama `AddMedicalFeeMasterData` mencakup ketiga tabel master data | Konsisten 100% dengan blueprint arsitektur backend |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Task ini berfokus pada migration skema basis data dan tidak menyediakan endpoint HTTP.

---

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: **tanpa `dotnet build` otomatis** |
| Eksekusi migration pada basis data (`dotnet ef database update`) | Tidak dijalankan | `NOT RUN` | Aturan keselamatan repositori (`AGENTS.md`): eksekusi migrasi database menuntut otorisasi terpisah |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang pada lingkungan eksekusi |
| Review diff dan cakupan perubahan | Tepat 1 berkas migration baru dibuat di `Migrations/`, nol tabel modul lain tersentuh | `PASS` | `git status --short` |
| Verifikasi metode `Up()` | Membentuk 3 tabel baru lengkap dengan kolom IdentityModel, tipe data, presisi persentase `(5, 2)`, 5 check constraint, dan foreign key yang tepat | `PASS` | `Migrations/20261026130000_AddMedicalFeeMasterData.cs` |
| Verifikasi metode `Down()` | Melakukan drop table secara bersih dan simetris dalam urutan kebalikan hierarki dependency | `PASS` | `MdfSharingRule` -> `MdfSharingAgreement` -> `MstMedicalFeeRole` |
| Pemeriksaan karakter baris (Line endings) | LF murni, UTF-8 tanpa BOM, nol karakter `\r` | `PASS` | `grep -c $'\r'` bernilai 0 |
| Skenario uji `T-01`..`T-13` | Tidak dijalankan | `NOT RUN` | Menuntut eksekusi migration pada basis data runtime dan ketersediaan layer service |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "`Up()` membuat 3 tabel" | Terpenuhi pada source migration | `20261026130000_AddMedicalFeeMasterData.cs` memanggil `CreateTable` untuk `MstMedicalFeeRole`, `MdfSharingAgreement`, dan `MdfSharingRule` |
| Acceptance: "`Down()` bersih" | Terpenuhi pada source migration | Menghapus ketiga tabel dalam urutan dependensi terbalik (`MdfSharingRule`, `MdfSharingAgreement`, `MstMedicalFeeRole`) |
| Verifikasi: "Migration dijalankan di lingkungan pengembangan" | **Belum dijalankan** | Menuntut otorisasi eksekusi database terpisah dan instruksi tanpa build/migration otomatis dari pemilik |
| DoD: "Nol tabel modul lain tersentuh" | Terpenuhi | Migration hanya memanggil `CreateTable` untuk 3 tabel baru modul Medical Fee; nol `AlterTable` atau `DropColumn` pada tabel modul lain |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas instruksi eksplisit pemilik (tanpa build otomatis) |

Catatan: Status task ditandai sebagai `🟡 SEBAGIAN` karena berkas migrasi telah lengkap dan memenuhi acceptance criteria pada kode, namun kompilasi `dotnet build` dan eksekusi pada basis data lingkungan pengujian belum dijalankan sesuai instruksi batasan lingkungan.

---

## 7. Catatan penutup dan Langkah Selanjutnya

1. **Kesiapan Eksekusi Basis Data**:
   Berkas migration `20261026130000_AddMedicalFeeMasterData.cs` siap diaplikasikan ke database PostgreSQL pengembang/staging saat pemilik memberikan otorisasi eksekusi migrasi.
2. **Task Berikutnya**:
   - `BE-MDF-004`: Pembuatan API dan Service daftar peran jasa medis (`MedicalFeeRolesController` dan `MedicalFeeRoleService`).
   - `BE-MDF-007`: Perancangan entity periode, hasil jasa, rincian, koreksi, dan daftar belum terhitung (`MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService`).
   - `BE-MDF-008`: Pembuatan migration gelombang kedua `AddMedicalFeeCalculation` (setelah `BE-MDF-007`).

