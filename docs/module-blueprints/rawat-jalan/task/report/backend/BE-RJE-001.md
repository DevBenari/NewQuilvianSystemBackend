# Laporan Perubahan Backend — `BE-RJE-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-001` |
| Judul | Fondasi data sinkron dan rekonsiliasi |
| Slice | `MVP-0` — `EPIC RJE-01` Fondasi data dan kontrak |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-001` |
| Trace | `FR-RJE-001`, `FR-RJE-002`, `FR-RJE-004`; `RJ-E2E-DEC-003`, `009`, `014`, `016`, `017`; `data/data-dictionary.md` bagian 2 dan 4; `contracts/state-transition-matrix.md` V2-A |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` (`RJ-E2E-DEC-016`, Sukma Giri, 28 September 2026); `1.0.0` dan `1.0.1` `approved` |
| Dependency | — (gelombang 1) |
| Klasifikasi | `MEDIUM` — 3 model (1 baru, 2 diperluas), 1 enum, 3 configuration, 1 migration dengan backfill, eksekusi database dev pemilik; tanpa endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: model, enum, configuration, `ApplicationDbContext`, migration, dan dokumen blueprint `rawat-jalan`. Database: **`QuilvianNewDevSukma` saja** (`RJ-E2E-DEC-017`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `063d38bc306bb6b46bdf088513fa6cdcc80399d8` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kelima acceptance criteria terbukti (bagian 6) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` (Operational, MasterData) dan `ClinicalManagement` |
| Registry | `Bil` — `BillingManagement / Billing` `ACTIVE`; `Cli` — `ClinicalManagement` `ACTIVE / LEGACY`; `Mst` — `Master / Reference / MasterData` `ACTIVE` |
| Keberlakuan | `MstBillingSyncPolicy`: `NEW CODE`. `BilProcessingEffect`, `CliClinicalMilestoneFact`: perluasan entity yang sudah patuh konvensi (bukan `Trx*`, bukan `LEGACY MIGRATION`) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-CODE-004` (unique `PolicyCode`), `QBE-ENUM-001`, `QBE-AUD-001` |
| Tidak berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001` — task tanpa controller/endpoint. `QBE-NAM-003`, `QBE-DB-001/002` — bukan rename legacy |
| Wewenang | Source, pembuatan migration, dan penerapan ke `QuilvianNewDevSukma` — `RJ-E2E-DEC-017`. Tanpa commit, push, merge, deployment, atau database lain |
| Governance terbaca | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md`, `REPORT_TEMPLATE.md`, `REVIEW_RULES.md` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, tidak ada tempat untuk mencatat apakah sebuah pelayanan Rawat Jalan sudah masuk ke
tagihan kasir, berapa kali pengirimannya dicoba, dan mengapa gagal. Tidak ada pula batas kirim ulang
yang dapat diatur admin. Akibatnya jembatan folio → invoice (`BE-RJE-003`) dan pekerja kirim ulang
(`BE-RJE-010`, `BE-RJE-011`) tidak punya pijakan data.

**Temuan yang mengubah desain.** Saat inspeksi, ternyata folio **tidak** membuat baris
`BilChargeLine` baru ketika fakta klinis berganti versi. Contoh: Lab *Darah Lengkap* diterima (versi 1)
lalu dibatalkan (versi 2). Folio hanya mencatat versi 2 sebagai satu `BilProcessingEffect` yang
menunjuk charge line versi 1 (`BillingFolioService.cs:183-266`). Kalau kolom sinkron ditaruh di
`BilChargeLine` seperti desain `1.0.0`, pembatalan dan obat tahap 2 tidak akan pernah sampai ke
invoice. Pemilik memutuskan kolom sinkron pindah ke `BilProcessingEffect` (`RJ-E2E-DEC-016`,
kontrak `1.0.2`) sebelum satu baris kode ditulis.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Setiap efek folio (satu fakta pelayanan + versinya) punya status penerusan ke tagihan yang dapat dibaca mesin dan petugas |
| Pelaku | Sistem (migration); admin Billing (kebijakan); petugas Billing (rekonsiliasi, pada task berikutnya) |
| Pemicu | Migration `AddClinicalChargeInvoiceSync` diterapkan |
| Langkah | 1. Kolom sinkron ditambahkan ke `BilProcessingEffect`, bernilai bawaan `NotApplicable`. 2. Kolom rekonsiliasi ditambahkan ke `CliClinicalMilestoneFact`. 3. Tabel `MstBillingSyncPolicy` dibuat berisi dua kebijakan. 4. Efek lama Rawat Jalan ditandai perlu rekonsiliasi |
| Aturan | Efek lama **tidak** pernah diteruskan otomatis (`RJ-E2E-DEC-014`); tanpa kebijakan aktif tidak ada kirim ulang otomatis (fail-closed) |
| Status yang dihasilkan | `BillingInvoiceSyncStatus`: `NotApplicable (0)`, `Pending (1)`, `Synced (2)`, `Failed (3)`, `ReconciliationRequired (4)`, `Resolved (5)` |
| Jalur tidak normal | Migration gagal di tengah → transaksi migration EF dibatalkan. Perlu mundur → `Down()` menghapus seluruh tambahan tanpa menyentuh data efek (terbukti, bagian 5 R10) |
| Hasil akhir | Efek lama Rawat Jalan menunggu keputusan petugas Billing dengan kode `LEGACY_PRE_BRIDGE`; efek lain `NotApplicable`; nol item invoice baru |

**Contoh backfill.** Efek *Procedure* `Succeeded` pada kunjungan Rawat Jalan → `4` /
`LEGACY_PRE_BRIDGE`. Efek *BloodBank* pada kunjungan yang sama → tetap `0` (di luar cakupan). Efek
*Laboratory* yang gagal sebelum membentuk folio → tetap `0`. Efek *Procedure* pada kunjungan IGD →
tetap `0`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingFolioService.cs` (`RecognizeMilestoneAsync`, `CreateProcessingEffect`),
`BillingOperationalConfigurations.cs`, `BilChargeLine.cs`, `BilProcessingEffect.cs`, `BilFolio.cs`,
`CliClinicalMilestoneFact.cs` beserta configuration-nya, `ClinicalMilestoneFactProducer.cs`,
`MstAdministrationFeePolicy.cs` beserta configuration-nya (pola master dan seed Billing),
`ApplicationDbContext.cs`, `ApplicationDbContextFactory.cs`, `ApplicationDbContextModelSnapshot.cs`,
`tooling/migrations/Update-MigrationHistory.ps1`, `RegPatientEncounter` di snapshot (tipe
`EncounterType`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Operational/Enums/BillingOperationalEnums.cs` | Enum baru `BillingInvoiceSyncStatus` (6 nilai, bawaan `NotApplicable = 0`) |
| `Areas/HealthServices/BillingManagement/Operational/Models/BilProcessingEffect.cs` | 16 properti baru: `IsClinicalCancellation`, `InvoiceSyncStatus`, `InvoiceSyncVersion`, `InvoiceSourceDomain`, `InvoiceSourceDetailId`, `InvoiceId`, `InvoiceItemId`, `InvoiceAdjustmentId`, `InvoiceSyncAttemptCount`, `InvoiceSyncNextAttemptAt`, `InvoiceSyncedAt`, `InvoiceSyncErrorCode`, `InvoiceSyncErrorMessage`, `ReconciliationResolvedAt`, `ReconciliationResolvedByUserId`, `ReconciliationResolutionNote` |
| `Areas/HealthServices/ClinicalManagement/Models/CliClinicalMilestoneFact.cs` | 5 properti baru: `NextDispatchAttemptAt`, `ReconciliationRequiredAt`, `ReconciliationResolvedAt`, `ReconciliationResolvedByUserId`, `ReconciliationResolutionNote` |
| `Areas/HealthServices/BillingManagement/MasterData/Models/MstBillingSyncPolicy.cs` | **Baru.** Master kebijakan kirim ulang + konstanta `BillingSyncPolicyCodes` |
| `Repositories/Configurations/HealthServices/BillingManagement/Operational/BillingOperationalConfigurations.cs` | Konfigurasi 16 kolom (panjang, bawaan, konversi enum, token konkurensi `InvoiceSyncVersion`), 2 index, 3 FK `Restrict` ke `BilInvoice`, `BilInvoiceItem`, `BilAdjustment` |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliClinicalMilestoneFactConfiguration.cs` | Panjang `ReconciliationResolutionNote` (500) dan index (`DispatchStatus`, `NextDispatchAttemptAt`) |
| `Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstBillingSyncPolicyConfiguration.cs` | **Baru.** Tabel, 3 check constraint (batas `RJE-VAL-030`), unique `PolicyCode` (baris hidup), token `RowVersion`, seed `FACT_DISPATCH` dan `INVOICE_SYNC` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<MstBillingSyncPolicy> MstBillingSyncPolicies` |
| `Migrations/20260928034143_AddClinicalChargeInvoiceSync.cs` + `.Designer.cs` | **Baru.** Hasil `dotnet ef migrations add`, ditambah SQL backfill `LEGACY_PRE_BRIDGE` di akhir `Up()` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui oleh EF. Selain tiga entitas task, lihat bagian 7 *Perubahan sampingan* |
| Dokumen blueprint | `00-interview-decisions.md` (`RJ-E2E-DEC-016`, `017`), `data/data-dictionary.md`, `02-backend-architecture.md`, `contracts/state-transition-matrix.md` (koreksi `1.0.2`), roadmap dan traceability (status) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint yang berubah |
| Database | 21 kolom baru (16 `BilProcessingEffect` + 5 `CliClinicalMilestoneFact`), 1 tabel baru, 6 index, 3 FK, 3 check constraint, 2 baris seed. Seluruhnya aditif. **Diterapkan ke `QuilvianNewDevSukma`** 28 September 2026; **belum** ke `QuilvianNewDevTim01`, staging, maupun production |
| Keamanan/Auth | `NOT APPLICABLE` — tanpa endpoint. Tidak ada kolom sensitif; `ReconciliationResolutionNote` diberi komentar larangan isi klinis |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Baseline `dotnet build … -o <scratchpad>/out-base` (sebelum perubahan) | `0 Error(s)`, `230 Warning(s)`, 2 menit 12 detik | `PASS` | Pembanding |
| Baseline `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Snapshot bersih sebelum task, sehingga `migrations add` aman dipakai |
| `dotnet ef migrations add AddClinicalChargeInvoiceSync` | 21 `AddColumn`, 3 `AddForeignKey`, 6 `CreateIndex`, 1 `CreateTable`, 1 `InsertData`; hanya tabel `BilProcessingEffect`, `CliClinicalMilestoneFact`, `MstBillingSyncPolicy` | `PASS` | Tidak ada operasi milik modul lain |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false --no-incremental -o <scratchpad>/out-rje001-full` | `0 Error(s)`, `230 Warning(s)`, 2 menit 22 detik; **nol** warning dari berkas task | `PASS` | Sama persis dengan baseline. Build inkremental sebelumnya (3,6 detik, `0 Warning(s)`) tidak dipakai sebagai bukti karena up-to-date, bukan kompilasi penuh |
| `dotnet ef migrations has-pending-model-changes` (sesudah) | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 11 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `Final result: PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| `dotnet ef database update 20260928034143_AddClinicalChargeInvoiceSync` | "Applying migration … Done." ke `QuilvianNewDevSukma` | `PASS` | Target dipastikan lewat konfigurasi `Development`; kredensial tidak dicetak |

### 5.1 Validasi database — 28 September 2026, `QuilvianNewDevSukma`

Keadaan sebelum migration: migration terakhir `20260925090000_AlterFinBillingHandoffIntakeHandoffTypeCheck`
(tidak ada migration lain tertunda); `BilProcessingEffect` 1 baris (*BloodBank*, kunjungan Rawat
Jalan, `Succeeded`); `CliClinicalMilestoneFact` 1 baris; `MstBillingSyncPolicy` belum ada.

Skenario R1–R9 dijalankan di dalam **satu transaksi yang di-rollback** dengan data sintetis
berpenanda `TEST-RJE001`, memakai teks SQL backfill yang dibaca langsung dari berkas migration.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Keadaan sesudah migration | Migration terakhir `20260928034143_AddClinicalChargeInvoiceSync`; efek lama *BloodBank* → `0`, tanpa `NULL`; `MstBillingSyncPolicy` 2 baris (5/60/3600, aktif); `BilInvoiceItem` 0 baris; 5 kolom `CliClinicalMilestoneFact` bertipe `timestamp with time zone`/`uuid`/`character varying`; 6 index dan 3 FK + 3 check constraint ada | 1, 2, 4 | `PASS` |
| R1 | Kunjungan Rawat Jalan tanpa folio tersedia sebagai data uji | Ya | — | `PASS` |
| R2 | Backfill atas 4 efek sintetis Rawat Jalan | 2 baris terbarui: *Procedure* `Succeeded` → `4`/`LEGACY_PRE_BRIDGE`; *Prescription* `PartialOutcome` → `4`/`LEGACY_PRE_BRIDGE`; *BloodBank* → `0`; *Laboratory* `FailedBeforeEffect` → `0` | 3 | `PASS` |
| R3 | Item invoice sesudah backfill | 0 | 3 | `PASS` |
| R4 | `PolicyCode` ganda (`INVOICE_SYNC`) | Ditolak `23505` `IX_MstBillingSyncPolicy_PolicyCode` | 4 | `PASS` |
| R5 | `MaxAttemptCount = 25` | Ditolak `23514` `CK_MstBillingSyncPolicy_MaxAttemptCount` | — | `PASS` |
| R6 | `MaxDelaySeconds` < `BaseDelaySeconds` | Ditolak `23514` `CK_MstBillingSyncPolicy_MaxDelaySeconds` | — | `PASS` |
| R7 | `MaxAttemptCount = 0` (sah, tanpa kirim ulang otomatis) | Diterima | — | `PASS` |
| R8 | Sisa data uji sesudah rollback | 0 | — | `PASS` |
| R9 | Efek *Procedure* pada kunjungan IGD (`EncounterType 2`) | Tidak terbackfill (0 baris), tetap `0` | 3 | `PASS` |
| R10 | `Down()` ke `20260925090000` lalu `Up()` kembali | Sesudah `Down()`: tabel `MstBillingSyncPolicy` hilang, 0 kolom baru tersisa di kedua tabel, efek lama tetap 1 baris. Sesudah `Up()`: migration terbaru kembali, seed 2 baris, efek lama `0` | 5 | `PASS` |

Uji manual: `NOT APPLICABLE` — tanpa endpoint; validasi dijalankan langsung terhadap database.

**Tidak dijalankan:** pembuktian backfill dengan data folio lama **nyata** — database dev pemilik
tidak memiliki efek Rawat Jalan dari lima konteks cakupan, sehingga dibuktikan dengan data sintetis
(R2, R9). `Down()` dibuktikan di `QuilvianNewDevSukma` sendiri, bukan salinan, karena membuat
database baru tidak diizinkan (memori pemilik); datanya aman karena `Down()` hanya menghapus
tambahan task ini.

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| `__EFMigrationsHistory` | `20260928034143_AddClinicalChargeInvoiceSync` terpasang (sekali dimundurkan dan dipasang ulang pada R10) |
| `MstBillingSyncPolicy` | 2 baris seed |
| `BilProcessingEffect` | 1 baris lama, `InvoiceSyncStatus = 0` |
| Data uji `TEST-RJE001` | Tidak ada — seluruhnya di-rollback |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Migration diterapkan ke `QuilvianNewDevSukma` tanpa error dan `has-pending-model-changes` bersih | Terpenuhi | Bagian 5; R0 |
| 2. Seluruh baris efek lama bernilai `0` atau `4`, tanpa `NULL` (kontrak `1.0.2`: `BilProcessingEffect`) | Terpenuhi | R0 (`0`: 1 baris, `NULL`: 0) |
| 3. Efek lama Rawat Jalan dari lima konteks yang menghasilkan folio bernilai `4` `LEGACY_PRE_BRIDGE`; nol item invoice baru | Terpenuhi (data sintetis) | R2, R3, R9 |
| 4. `MstBillingSyncPolicy` berisi `FACT_DISPATCH` dan `INVOICE_SYNC` (5/60/3600, aktif); `PolicyCode` unik | Terpenuhi | R0, R4 |
| 5. `Down()` membalik seluruh perubahan | Terpenuhi (pada database dev pemilik, bukan salinan) | R10 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `LF will be replaced by CRLF` dari Git pada beberapa berkas — hanya akhir baris, diff isi tetap sesuai |
| Masalah yang diketahui | Kontrak desain `1.0.0` keliru menempatkan kolom sinkron di `BilChargeLine`; dikoreksi `1.0.2` dan dokumen desain diperbarui dengan blok koreksi (bukan ditulis ulang). Kartu task lain di roadmap yang menyebut `BilChargeLine` (misalnya `BE-RJE-003`) **dibaca** sebagai `BilProcessingEffect` sesuai blok koreksi di `02` |
| Risiko tersisa | Designer migration baru (~4,5 MB) ikut dikompilasi; `tooling/migrations/Update-MigrationHistory.ps1` dapat dijalankan pemilik untuk memangkas, **tidak** dijalankan di task ini karena mengubah berkas migration lain. Migration belum diterapkan ke database selain `QuilvianNewDevSukma` |
| Perubahan sampingan | `ApplicationDbContextModelSnapshot.cs` ditulis ulang EF. Di luar tiga entitas task: (a) blok entity **ganda** di snapshot HEAD untuk `PhmMedicationAdministration`, `PhmMedicationAdministrationRevision`, `PhmMedicationAdministrationSetting`, `PhmMedicationScheduleTime` dihapus salinan keduanya (jumlah kemunculan turun tepat 1); (b) `PhmPrescriptionFinancialProjection` berpindah posisi; (c) `.ValueGeneratedOnAdd()` pada `RefundCategory`; (d) satu baris kosong di `LabOrder`; (e) BOM di baris pertama. Semuanya **setara model** — migration tidak memuat satu operasi pun untuk tabel-tabel itu dan `has-pending-model-changes` bersih. **Dibiarkan** karena merupakan keluaran canonical EF; memulihkannya berarti menyunting tangan berkas 112 ribu baris |
| Interupsi | `NONE` — satu penghentian yang disengaja untuk keputusan pemilik (`RJ-E2E-DEC-016`) |
| Status Git | Lihat bagian 7.1 |
| Langkah berikutnya | Gelombang 2: `BE-RJE-002` (kontrak adapter `1.3`) dan `BE-RJE-011` (kirim ulang fakta) — keduanya memerlukan wewenang tulis tersendiri. `BE-RJE-004` dan `BE-RJE-006` (gelombang 1) juga dapat dimulai |

### 7.1 Status Git akhir

```text
 M Areas/HealthServices/BillingManagement/Operational/Enums/BillingOperationalEnums.cs
 M Areas/HealthServices/BillingManagement/Operational/Models/BilProcessingEffect.cs
 M Areas/HealthServices/ClinicalManagement/Models/CliClinicalMilestoneFact.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
 M Repositories/Configurations/HealthServices/BillingManagement/Operational/BillingOperationalConfigurations.cs
 M Repositories/Configurations/HealthServices/ClinicalManagement/CliClinicalMilestoneFactConfiguration.cs
?? Areas/HealthServices/BillingManagement/MasterData/Models/MstBillingSyncPolicy.cs
?? Migrations/20260928034143_AddClinicalChargeInvoiceSync.Designer.cs
?? Migrations/20260928034143_AddClinicalChargeInvoiceSync.cs
?? Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstBillingSyncPolicyConfiguration.cs
```

Ditambah dokumen blueprint `docs/module-blueprints/rawat-jalan/**` dari sesi desain dan perencanaan
sebelumnya (belum di-commit). Tidak ada berkas yang di-stage, di-commit, atau di-push.
