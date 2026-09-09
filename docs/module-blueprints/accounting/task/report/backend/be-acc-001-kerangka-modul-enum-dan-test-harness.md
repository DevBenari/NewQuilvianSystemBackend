# BE-ACC-001 — Kerangka modul, enum, dan test harness

- **TASK ID:** `BE-ACC-001` — Kerangka modul, enum, dan test harness
- **TASK TYPE:** Implementasi backend, fondasi modul
- **COMPLEXITY:** `LIGHT`
- **CLASSIFICATION SCORE:** rendah — tanpa entity persisted, tanpa endpoint, tanpa migration
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `Areas/Corporate/AccountingManagement/**`, `Tests/QuilvianSystemBackend.Tests/AccountingManagement/**`
- **Tanggal:** 1 September 2026
- **Baseline:** `ACC-BP-001` revisi 5 `APPROVED`, decision revision 1.2, roadmap revisi 2 `APPROVED`
- **HEAD saat mulai:** `ca6b7e0` pada branch `rizkiG`, working tree bersih

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `MasterData/ChartOfAccount`, `MasterData/JournalType`, `JournalManagement`, `AccountingPeriod`, `GeneralLedger` |
| Pemilik / prefix registry | Rizki / `Acc` — **terdaftar, lifecycle `ACTIVE`** |
| Applicability | `NEW CODE` |
| QBE ID yang berlaku | `QBE-MOD-002`, `QBE-MOD-003` — dievaluasi, tidak dilanggar |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca, lihat catatan lokasi di bawah |

### Catatan lokasi registry — empat salinan, isinya berbeda

Ini yang paling mudah menyesatkan pada modul ini, jadi dicatat lengkap.

| # | Lokasi | Ada baris `Acc`? | Keterangan |
|---:|---|:---:|---|
| 1 | `QuilvianEngineeringSkills/agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Ya, `ACTIVE`** | **Sumber kebenaran.** Baris 29, kepanjangan baris 53, catatan lifecycle baris 99–100 |
| 2 | `NewQuilvianSystemBackend:docs/engineering/...` pada `origin/QuilvianIntegrationBackend@9522caa` | **Tidak** | Salinan lama. Disebut `AGENTS.md`, tetapi belum memuat Accounting |
| 3 | Akar rules terpasang, `rules/backend/engineering/...` | **Folder tidak ada** | Plugin cache tertinggal dari repo sumbernya |
| 4 | `NewQuilvianSystemBackend:agents/rules/engineering/...` | **Folder tidak ada** | **Path yang dibaca checker QBE** — lihat temuan 2 |

Pendaftaran `Acc` tercatat dua tahap pada salinan nomor 1:

- 1 September 2026 — baris baru, lifecycle `PLANNED`, atas instruksi pemilik modul dan blueprint revisi 4;
- 1 September 2026 — `PLANNED` → `ACTIVE`, atas blueprint revisi 5 keputusan `ACC-DEC-038`, yang mencabut penghalang `QBE-MOD-002` untuk source model persisted `Acc*`.

Catatan lifecycle itu juga menegaskan `AccountingManagement`/`Acc` dan `Finance`/`Fin` adalah bounded context berbeda, dan entri `Fin` tidak diubah.

## FILES INSPECTED

`AGENTS.md`; `rules/backend/REPORT_TEMPLATE.md`; `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (empat salinan di atas); `tooling/qbe/Invoke-QbeConformanceCheck.ps1`; `Models/IdentityModel.cs`; `Areas/Corporate/HumanResource/MasterData/Workforce/Enums/ExternalUserStatus.cs`; `Areas/HealthServices/BillingManagement/Operational/Enums/BillingOperationalEnums.cs`; `Tests/QuilvianSystemBackend.Tests/MedicalRecordManagement/ClinicalDocumentIntegritySchemaTests.cs`; `QuilvianSystemBackend.sln`; blueprint `roadmap/backend-roadmap.md`, `contracts/state-transition-matrix.md`, `erd/01-chart-of-account.md`, `erd/02-journal.md`, `erd/03-accounting-period.md`.

## FILES CHANGED

Tujuh berkas dibuat, **nol berkas existing diubah**.

| Berkas | Baris |
|---|---:|
| `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Enums/AccountType.cs` | 28 |
| `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Enums/NormalBalance.cs` | 22 |
| `Areas/Corporate/AccountingManagement/JournalManagement/Enums/JournalStatus.cs` | 32 |
| `Areas/Corporate/AccountingManagement/JournalManagement/Enums/JournalApprovalAction.cs` | 33 |
| `Areas/Corporate/AccountingManagement/JournalManagement/Enums/JournalCorrectionType.cs` | 29 |
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/AccountingPeriodStatus.cs` | 30 |
| `Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingFoundationTests.cs` | 219 |

**7 files changed, 393 insertions(+), 0 deletions(-)**

Ditambah 18 folder kosong yang **tidak terlacak git** karena repo ini tidak memakai `.gitkeep`. Folder itu terisi sendiri pada `BE-ACC-003` ke atas.

## IMPLEMENTATION

Enam enum dibuat sesuai contract `ACC-STATE-0.1`, satu enum satu berkas, mengikuti pola `ExternalUserStatus.cs`: namespace berkurung, `using System.ComponentModel.DataAnnotations;`, dan `[Display(Name = ...)]` Bahasa Indonesia pada setiap anggota.

| Enum | Nilai |
|---|---|
| `AccountType` | `Asset=1`, `Liability=2`, `Equity=3`, `Revenue=4`, `Expense=5` |
| `NormalBalance` | `Debit=1`, `Credit=2` |
| `JournalStatus` | `Draft=1`, `PendingApproval=2`, `Approved=3`, `Posted=4`, `Rejected=5` |
| `JournalApprovalAction` | `Submitted=1`, `Approved=2`, `Rejected=3`, `Posted=4`, `Reversed=5` |
| `JournalCorrectionType` | `FullReversal=1`, `Adjustment=2` |
| `AccountingPeriodStatus` | `Open=1`, `SoftClosed=2`, `Closed=3` |

Test harness berisi sembilan test: enam menguji nilai enum, satu menguji kelengkapan `Display Name`, satu menjaga batas task lewat refleksi (tidak boleh ada tipe di namespace Accounting yang mewarisi `IdentityModel`), dan satu menjaga konvensi folder `Controllers/` bentuk jamak lewat pemeriksaan filesystem.

Test penjaga batas itu sengaja dibuat: ia menjadi pagar otomatis bila ada yang menambahkan entity Accounting di luar urutan task.

## BLUEPRINT STATUS/EVIDENCE

`NOT APPLICABLE` — task ini `BACKEND MODE`, bukan `MODULE BLUEPRINT MODE`.

## API CONTRACT IMPACT

`NONE`. Tidak ada controller, endpoint, maupun DTO yang dibuat.

## DATABASE IMPACT

`NONE`. Diverifikasi satu per satu:

| Pemeriksaan | Hasil |
|---|---|
| Berkas di `Models/` Accounting | Tidak ada |
| `Migrations/` berubah | Tidak |
| `Migrations/ApplicationDbContextModelSnapshot.cs` berubah | Tidak |
| `Repositories/ApplicationDbContext.cs` berubah | Tidak |
| `Program.cs` berubah | Tidak |
| `dotnet ef migrations add` dijalankan | Tidak |
| `dotnet ef database update` dijalankan | Tidak |

## SECURITY IMPACT

`NONE`. Tidak ada endpoint, atribut hak akses, maupun jalur autentikasi yang disentuh.

## VISUAL REFERENCE

`NOT REQUIRED`.

## VALIDATION

| Perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `dotnet build QuilvianSystemBackend.sln` | **0 Error**, 199 Warning | Lulus | Seluruh warning pre-existing; nol berasal dari berkas baru |
| `dotnet test Tests/QuilvianSystemBackend.Tests --filter AccountingManagement` | **9 lulus**, 0 gagal | Lulus | Durasi 408 ms |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **141 lulus**, 0 gagal | Lulus | Regresi penuh project |
| `dotnet test QuilvianSystemBackend.Tests` (akar) | **815 lulus**, 0 gagal | Lulus | Regresi project kedua |

Total **956 test lulus, nol regresi**.

## Acceptance criteria

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | Build lulus | ✅ | 0 Error |
| 2 | Enam enum bernilai persis contract state | ✅ | Enam test nilai, seluruhnya lulus |
| 3 | Folder `Controllers/` bentuk jamak | ✅ | `FolderModulAccounting_MemakaiKonvensiRepository` — menolak bentuk tunggal `Controller/` |
| 4 | Tidak ada berkas di `Models/` | ✅ | `ModulAccounting_BelumMemilikiEntityPersisted` |

## Definition of Done

| Butir | Hasil |
|---|:---:|
| Enum lengkap | ✅ |
| Build lulus | ✅ |
| Laporan task berisi daftar berkas | ✅ berkas ini |

## WARNINGS

199 warning build, seluruhnya pre-existing pada modul lain: `MSB3277` konflik versi `Microsoft.Extensions.DependencyModel` di `BillingTests`, sejumlah `xUnit2029`/`xUnit2031` di `InPatientManagement`, dan satu `CS8603` di `LaboratoryAuthorityTests`. Tidak satu pun berasal dari berkas Accounting, dan tidak ada yang diperbaiki karena berada di luar cakupan task.

## KNOWN ISSUES

Empat temuan, diurutkan dari yang paling berdampak.

### 1. Checker QBE membaca path registry yang tidak ada — akar `ACC-DEP-007`

`tooling/qbe/Invoke-QbeConformanceCheck.ps1` baris 29 dan 162 menyusun path registry sebagai:

```
<root backend>/agents/rules/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
```

Folder `agents/rules/engineering/` **tidak ada** di `NewQuilvianSystemBackend`. Yang ada hanya `agents/rules/` berisi tujuh berkas, tanpa subfolder `engineering/`.

Akibatnya checker tidak menemukan registry sama sekali saat dijalankan terhadap repo backend. Ini akar konkret `ACC-DEP-007`, dan sifatnya **mematikan gerbang CI saat merge, bukan kemampuan menulis kode**. Milik lead; jangan ditambal sebagai bagian task Accounting.

### 2. Plugin cache tertinggal dari repo sumber skill

`AGENTS.md` mewajibkan membaca `rules/GLOBAL_RULES.md` dan `rules/backend/engineering/*` dari akar rules terpasang. Keduanya **tidak ada** di `.claude/plugins/cache/quilvian/quilvian-engineering-skills/0.1.0/.claude/rules/`, padahal `rules/backend/engineering/` **ada** di repo sumber `QuilvianEngineeringSkills`.

Jadi masalahnya bukan dokumennya hilang, melainkan **plugin terpasang belum diperbarui**. Selama belum, agent yang hanya membaca plugin cache akan menyimpulkan registry belum memuat `Acc` — persis yang terjadi pada sesi ini sebelum dikoreksi.

Urutan presedensi tetap terbaca lewat `rules/README.md`, yang isinya sepadan dengan yang dijanjikan `GLOBAL_RULES.md`.

### 3. Sisa folder `agents/rules/` di backend

`AGENTS.md` menyatakan repo ini *"tidak lagi memiliki folder `agents/rules/`"* dan meminta sisanya dilaporkan. Folder itu masih ada dan **terlacak git** — tujuh berkas. Tidak dipakai sebagai sumber aturan pada task ini, dan tidak dihapus karena berada di luar cakupan.

Perlu diperhatikan: selama folder ini masih ada, mudah tertukar dengan akar rules yang benar.

### 4. Dua project bernama `QuilvianSystemBackend.Tests`

Solution memuat dua project dengan nama assembly sama:

```
QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj        (akar)  — 815 test
Tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj  (Tests/) — 141 test
```

Test Accounting ditempatkan di `Tests/`, mengikuti preseden terbaru Medical Record (31 Agustus 2026) dan sejalan dengan `Tests/QuilvianSystemBackend.BillingTests`. Utang teknis yang sudah ada sebelum task ini.

## MANUAL TEST

`NOT APPLICABLE` — tidak ada perilaku runtime yang dapat diamati pengguna pada task ini.

## INCIDENTAL CHANGES

`NONE`.

## INTERRUPTIONS

`NONE`.

## GIT STATUS

```
?? Areas/Corporate/AccountingManagement/
?? Tests/QuilvianSystemBackend.Tests/AccountingManagement/
```

Tidak ada berkas existing yang berubah. Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

`BE-ACC-002` — audit mekanisme hak akses badan hukum. Read-only, tanpa entity, dan menutup satu pertanyaan memblokir yang tersisa yaitu cara pemberian hak `LegalEntityId` kepada pengguna.

`BE-ACC-003` juga sudah `EXECUTION_READY` karena lifecycle `Acc` sudah `ACTIVE`. Namun mendahulukan `BE-ACC-002` lebih aman: hasilnya menentukan bentuk penyaringan yang dipakai seluruh endpoint sejak `BE-ACC-007`.

**Menunggu instruksi eksplisit owner.** Approval roadmap bukan perintah jalan.
