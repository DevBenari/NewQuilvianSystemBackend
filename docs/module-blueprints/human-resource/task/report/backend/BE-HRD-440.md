# Laporan Perubahan Backend — `BE-HRD-440`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-440` |
| Judul | Kolom `AllowBulkApproval` dan `EscalatedFromAssignmentId`, migration, konstanta `Escalate`, ruas definisi |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 1 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-440` |
| Trace | `FR-WF-29`, `FR-WF-30`; `HRD-DEC-232`, `HRD-DEC-233` |
| Contract version | `v29` — kamus data 30.1, 30.2, 30.4; desain backend 37.6–37.7 — approved 20 November 2026 |
| Dependency | Gerbang G1 ✅; **G2 ✅** — izin pengguna 20 November 2026: "lanjut BE-HRD-440, izin migration diberikan" (membuat dan menerapkan migration ini ke devYoga) |
| Klasifikasi | `STANDARD` — `TOUCHED LEGACY` pada entity dan konfigurasi yang sudah ada; satu migration aditif |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `815ce085` (branch `yoga`) + working tree task ini |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi; migration diterapkan ke devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) dan `MasterData/Workflow` |
| Keberlakuan | `TOUCHED LEGACY` — kolom ditambahkan ke `MstWorkflowDefinition` dan `TrxWorkflowApproverAssignment` yang sudah ada; tanpa rename (bukan `LEGACY MIGRATION`), tanpa entity baru |
| QBE yang berlaku | `QBE-ENT-002` (Guid nullable + navigasi sesuai semantik), `QBE-CFG-002` (konfigurasi diperbaiki dalam cakupan: default, FK `Restrict`, index), `QBE-API-001` (DTO dan respons pola yang ada). `QBE-NAM-001`/`002` tidak berlaku: nama `Trx*`/`Mst*` legacy tidak diubah |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md` (suite Skill) |
| Wewenang terpisah | Implementasi source ✅; pembuatan migration ✅ (G2); eksekusi ke devYoga ✅ (G2, hanya migration ini); deployment ✖ |

---

## 1. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowDefinition.cs` | `public bool AllowBulkApproval { get; set; } = false;` |
| `Repositories/Configurations/Corporate/HumanResource/MasterData/Workflow/MstWorkflowDefinitionConfiguration.cs` | `HasDefaultValue(false)` untuk `AllowBulkApproval` |
| `Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowApproverAssignment.cs` | `Guid? EscalatedFromAssignmentId` + navigasi `EscalatedFromAssignment` |
| `Repositories/Configurations/Corporate/HumanResource/WorkflowManagement/TrxWorkflowApproverAssignmentConfiguration.cs` | FK diri sendiri `DeleteBehavior.Restrict` + index `EscalatedFromAssignmentId` |
| `Areas/Corporate/HumanResource/WorkflowManagement/Models/WorkflowValueConstants.cs` | `ActionType.Escalate = "Escalate"` |
| `Areas/Corporate/HumanResource/MasterData/Workflow/DTOs/WorkflowDefinitionDtos.cs` | `AllowBulkApproval` pada `WorkflowDefinitionResponse` dan `CreateWorkflowDefinitionRequest` (diwarisi `UpdateWorkflowDefinitionRequest`) |
| `Areas/Corporate/HumanResource/MasterData/Workflow/Controllers/WorkflowDefinitionController.cs` | Pemetaan pada buat, ubah, daftar, dan detail (4 baris; tanpa `<summary>` XML baru) |
| `Migrations/20261120035346_AddWorkflowBulkFlagAndEscalationLink.cs` (+ `.Designer.cs`) | **Baru**. `Up`: dua `AddColumn`, `CreateIndex`, `AddForeignKey` (`Restrict`). `Down`: kebalikannya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui generator EF: dua kolom, FK, index **dan** entity `HrdOvertimeSetting` (lihat 4) |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowEscalationAndBulkFlagSchemaTests.cs` (baru), `AcceptanceTestIndex.md` | 7 uji harness dan satu baris indeks |

---

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowEscalationAndBulkFlagSchemaTests` | 7/7 lulus | `PASS` (AC1–AC3) | Keluaran `dotnet test` |
| Kontrol negatif: 7 mutasi (FK `Cascade`; pemetaan buat, ubah, daftar, detail dihapus; migration membawa operasi lain; bawaan `HasDefaultValue` dihapus) | 6 tertangkap; **M1 setara**: tanpa `HasDefaultValue`, EF tetap memberi `defaultValue: false` pada kolom `bool NOT NULL` baru, sehingga skema tidak berubah | `PASS` | `mut440.js` |
| Regresi penuh proyek uji HR | 360/360 lulus (10 menit 20 detik) | `PASS` | Lihat 4: dijalankan dari salinan tanpa empat berkas uji payroll |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | 0 galat | `PASS` | Log build |
| QBE `Strict` atas 8 berkas task | 0 violation, 0 review; probe kontrol `TrxZzzQbeProbe440` terdeteksi 4 violation (`QBE-NAM-001`, `ENT-001`, `CFG-001`, `MOD-002`) lalu dihapus | `PASS` | Keluaran checker |
| Migration ke devYoga | Skrip `--idempotent` khusus migration ini; preflight tanpa DROP/UPDATE/DELETE/TRUNCATE; uji kering `ROLLBACK` (riwayat tetap 428); diterapkan `COMMIT`: dua kolom, index, FK, riwayat 429, seluruh definisi `AllowBulkApproval = false` | `PASS` | `apply440.mjs`, `dotnet ef migrations list` |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Migration menambah kedua kolom, FK, index; `Down` menghapusnya; definisi lama `false` | Terpenuhi | Uji isi migration; devYoga sesudah `COMMIT` |
| 2. Master Workflow menyimpan dan mengembalikan `allowBulkApproval` | Terpenuhi | Uji buat/ubah/detail/daftar lewat controller |
| 3. Penugasan dapat menyimpan `EscalatedFromAssignmentId` pada tahap yang sama | Terpenuhi | Uji round-trip + metadata FK |

---

## 3. Status migration dan database

| Hal | Isi |
| --- | --- |
| Migration | `20261120035346_AddWorkflowBulkFlagAndEscalationLink` — aditif, tanpa downtime |
| devYoga | Diterapkan 20 November 2026; `__EFMigrationsHistory` 428 → 429 |
| Migration lain yang tertunda di devYoga (tidak disentuh) | `20261118035706_AddPayrollStatutoryPolicyTaxInsuranceAndWorkforceAllowance`, `20261118100000_AddHrdOvertimeSetting`, `20261119042534_AddAssetMovement` — milik pihak lain, di luar izin. `dotnet ef database update` berikutnya tetap menerapkannya (EF menerapkan setiap migration yang belum ada di riwayat); skrip ber-rentang wajib `--idempotent` |
| Data | Tidak ada perubahan data; penanda massal awal (`HRD-ASM-90`) menunggu `BE-HRD-448` (G3, G4) |

---

## 4. Temuan dan delta

| Temuan | Dampak | Tindak lanjut |
| --- | --- | --- |
| Snapshot model tidak memuat `HrdOvertimeSetting` (migration manual idempoten `20261118100000_AddHrdOvertimeSetting`), sehingga generator EF ikut membuat tabel itu di migration ini | Bila dibiarkan, migration gagal/ganda saat `AddHrdOvertimeSetting` sudah diterapkan | Operasi `HrdOvertimeSetting` dikeluarkan dari `Up`/`Down` (dijaga uji "hanya dua kolom, FK, index"); snapshot dibiarkan memuat entity itu karena memang bagian model |
| Proyek uji HR gagal kompilasi sejak merge `815ce085`: `WorkforcePayrollDataContractTests` dan `WorkforcePayrollDataSaveTests` memakai `WorkforcePayrollDataController.Save`, `UpdateWorkforcePayrollDataRequest`, dan seterusnya yang sudah tidak ada | Uji HR tidak dapat dijalankan dari repository apa adanya | Uji dijalankan dari salinan scratchpad tanpa empat berkas `WorkforcePayrollData*Tests` (milik slice `S-A8`, tidak diubah). Perlu penyesuaian oleh pemilik `S-A8` |
| Indeks unik per tahap `(WorkflowStepInstanceId, AssignmentOrder)` dan `(WorkflowStepInstanceId, AssignedApproverUserId)` | Penugasan eskalasi wajib memakai urutan berikutnya dan tidak boleh dibuat bila atasan sudah penyetuju di tahap itu | Dicatat untuk `BE-HRD-442` |
| Index FK `EscalatedFromAssignmentId` juga dibuat konvensi EF | `HasIndex` eksplisit tidak mengubah hasil, tetap dipertahankan sesuai desain 37.6 | Tidak perlu tindakan |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Migration lain yang tertunda di devYoga milik pihak lain |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `BE-HRD-441` (Plh aktif di engine), `BE-HRD-442` (eskalasi di engine), `BE-HRD-447` (endpoint generik) — gelombang 2, dapat paralel |
