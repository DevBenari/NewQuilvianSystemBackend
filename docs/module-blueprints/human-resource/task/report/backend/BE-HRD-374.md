# Laporan Perubahan Backend — `BE-HRD-374`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-374` |
| Judul | Definisi workflow `BUSINESS_TRAVEL_REQUEST`: konstanta, seeder, dan pendaftaran startup |
| Slice | `S-D5` amandemen / `MVP-70` — gelombang 1 · `EPIC HRD-121` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-374` |
| Trace | `HRD-DEC-224`; desain `02-backend-architecture.md` 36.3–36.7; Integration 25.1 (definisi); Permission 32.1 (`AllowSelfApproval = false`); `HRD-TF-075` (SLA 48 jam); `HRD-Q-222` (bawaan aktif) |
| Contract version | `v28` — approved 19 November 2026 (`HRD-DEC-015`); tidak diubah |
| Dependency | Gerbang G1 (kontrak `v28` approved) ✅ |
| Klasifikasi | `LOW–MEDIUM` — satu repository, 3 berkas source (2 baru, 1 diubah satu blok), tanpa perubahan API/schema; menulis satu baris data master saat startup di basis data bersama |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjut BE-HRD-374") |
| Target tulis | `Constants/BusinessTravelWorkflowCodes.cs` (baru), `Seeders/BusinessTravelWorkflowDefinitionSeeder.cs` (baru), `Program.cs`; uji baru `Tests/QuilvianSystemBackend.HumanResourceTests/TravelWorkflowDefinitionSeederTests.cs`; dokumen roadmap/traceability/indeks uji |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d7d4eb3b` (branch `yoga`); perubahan sebelumnya (`BE-HRD-298`, dokumen desain/rencana `v28`) belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terbukti: uji baru 8/8, kontrol negatif lima mutasi tertangkap, proyek uji HR 327/327, QBE Strict tanpa temuan; startup Development menanam definisi di devYoga dan startup kedua tidak menambah apa pun |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Data yang ditulis milik `WorkflowManagement / Workflow` (`Wfl`, `SHARED PLATFORM CAPABILITY`, `ACTIVE / LEGACY`) lewat entity `MstWorkflowDefinition`/`MstWorkflowStep` yang sudah ada; tanpa entity baru, sehingga `QBE-MOD-002` tidak berlaku |
| Keberlakuan | `NEW CODE` (konstanta dan seeder baru); `TOUCHED LEGACY` (`Program.cs`, satu panggilan `RunStartupSeederAsync`) |
| QBE yang berlaku | `QBE-MOD-001` (seeder dan konstanta di bawah submodule pemilik `BusinessTravelManagement`), `QBE-ENUM-001` (kode referensi, definisi, tahap, dan status aktif dimiliki modul), `QBE-TXN-001` (satu `SaveChangesAsync` untuk definisi dan tahap), `QBE-AUD-001` (log startup terpisah dari audit basis data) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*` (tanpa entity/konfigurasi baru); `QBE-API-*`, `QBE-PERM-001`, `QBE-DTO-001` (tanpa endpoint); `QBE-CODE-*` (kode definisi tetap, bukan nomor bisnis yang dialokasikan); `QBE-DB-*` (tanpa schema/migration) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill (folder `.codex` sudah dicabut, `AGENTS.md` baris 45) |

---

## 1. Kondisi as-is terhadap kontrak

| Hal | As-is (`d7d4eb3b`) | Kontrak `v28` / desain 36.7 |
| --- | --- | --- |
| Definisi workflow perjalanan dinas | Tidak ada di source maupun di devYoga (kueri baca-saja sebelum startup: 0 baris dengan kode `BUSINESS_TRAVEL_REQUEST` atau `RequestType` berisi "trav") | Satu definisi "Persetujuan Perjalanan Dinas" v1, satu tahap atasan langsung, aktif |
| Syarat definisi terpakai | `WorkflowService.ResolveWorkflowDefinitionAsync` hanya memilih definisi `IsActive` **dan** `WorkflowStatus == "Active"`, dalam rentang tanggal dan lingkup pemohon | Seeder mengisi keduanya; tanpa batas tanggal dan tanpa lingkup sehingga berlaku untuk semua pemohon |
| SLA tahap | `MstWorkflowStep` tidak punya kolom SLA; tenggat tugas (`DueAt`) dihitung dari `EscalationAfterHours ?? ReminderAfterHours` (`WorkflowService.cs` baris 1911) dan kolom itu tidak dipakai di tempat lain | 48 jam (`HRD-TF-075`) → `EscalationAfterHours = 48` |
| Konstanta status definisi | Workflow V2 memakai literal `"Active"`; Lembur menyimpan konstantanya sendiri (`OvertimeValueConstants.Workflow.ActiveStatus`) | Konstanta milik modul `BusinessTravelWorkflowCodes.DefinitionActiveStatus` |

**Delta yang dicatat (tidak mengubah kontrak):** konvensi HR yang ada memakai `RequestType` deskriptif (`OvertimeRequest`; komentar model menyebut `TravelRequest`) dan `ReferenceType` kapital (`OVERTIME_REQUEST`). Kontrak `v28` yang disetujui (desain 36.7, integration 25.1) menetapkan `HRD_TRV_REQUEST` untuk keduanya, dan rencana `FE-HRD-339` memetakan nilai itu di kotak masuk. Task ini mengikuti kontrak; `RequestType` tampil sebagai jenis dokumen di kotak masuk persetujuan dan dipakai untuk menyaring alasan penolakan.

---

## 2. Perubahan yang dikerjakan

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/HumanResource/BusinessTravelManagement/Constants/BusinessTravelWorkflowCodes.cs` | Baru | `ReferenceType = "HRD_TRV_REQUEST"`, `RequestType` (sama), `DefinitionCode = "BUSINESS_TRAVEL_REQUEST"`, `ManagerStepCode = "HRD-TRV-01"`, `WorkflowCategory = "HumanResource"`, `DefinitionActiveStatus = "Active"`, `ManagerStepSlaHours = 48` |
| `Areas/Corporate/HumanResource/BusinessTravelManagement/Seeders/BusinessTravelWorkflowDefinitionSeeder.cs` | Baru | Pola `ProcurementWorkflowDefinitionSeeder`: menghormati `SeedDefaultData:Enabled`; definisi dicari menurut kode + versi 1 termasuk yang dihapus; yang dihapus admin tidak dihidupkan; yang sudah ada tidak pernah ditimpa; tahap `HRD-TRV-01` ditambahkan hanya bila belum ada (termasuk yang dihapus). Definisi: `IsActive = true`, `WorkflowStatus = "Active"`, `IsDefault`, `AllowRequesterCancel`/`AllowRequesterWithdraw`, `StopOnRejection`, tanpa lingkup. Tahap: `Approval`, `Any`, 1 penyetuju, `RequesterManager`, `AllowSelfApproval = false`, `AllowDelegation`, `EscalationAfterHours = 48`. Satu `SaveChangesAsync` |
| `Program.cs` | Diubah (1 blok) | `RunStartupSeederAsync("BusinessTravelWorkflowDefinitionSeeder", …)` tepat sesudah seeder workflow Pengadaan, dengan komentar asal-usul |
| `Tests/QuilvianSystemBackend.HumanResourceTests/TravelWorkflowDefinitionSeederTests.cs` | Baru | 8 uji (lihat bagian 3) |

Berkas baru ditulis CRLF seperti berkas sekitarnya. Tanpa `<summary>` XML pada action controller (tidak ada controller yang disentuh).

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter TravelWorkflowDefinitionSeederTests` (SQLite harness) | 8/8 lulus | `PASS` | Keluaran `dotnet test` |
| — Tanam pertama | 1 definisi + 1 tahap; seluruh nilai sesuai desain 36.7 | `PASS` (AC1) | idem |
| — Definisi memenuhi syarat pilih Workflow V2 (salinan kriteria `ResolveWorkflowDefinitionAsync`: aktif, "Active", kode, tanggal, tanpa lingkup) dengan satu tahap aktif | Tepat satu kandidat | `PASS` (AC1) | idem |
| — Tanam ulang | 0/0; tetap satu definisi, satu tahap | `PASS` (AC1) | idem |
| — Admin menonaktifkan, mengganti nama, mengubah SLA tahap jadi 24 jam, menambah tahap `HRD-TRV-02`, lalu tanam ulang | 0/0; seluruh perubahan admin tetap | `PASS` (AC2) | idem |
| — Definisi dihapus admin, lalu tanam ulang | 0/0; tidak ada definisi baru | `PASS` (AC2) | idem |
| — Tahap bawaan dihapus admin → tidak ditambahkan lagi; tahap dihapus fisik → dilengkapi | 0 lalu 1 tahap | `PASS` (AC2) | idem |
| — `SeedDefaultData:Enabled = false` lewat `SeedAsync(IServiceProvider)` | Dilewati, 0 definisi | `PASS` (AC3 sebagian) | idem |
| — Konfigurasi bawaan lewat `SeedAsync(IServiceProvider)` | Menanam 1/1 | `PASS` | idem |
| Kontrol negatif (lima putaran; berkas dipulihkan dari salinan, `cmp` identik di akhir) | K1 `IsActive = false` → 2 uji gagal; K2 pencarian mengabaikan yang dihapus → 1 gagal; K3 tahap selalu ditambah → 3 gagal; K4 SLA dihapus → 1 gagal; K5 pemeriksaan `SeedDefaultData` dimatikan → 1 gagal | `PASS` | Keluaran `dotnet test` per putaran |
| Proyek uji HR penuh (`dotnet test -p:RunAnalyzers=False`, termasuk build project aplikasi) | 327/327 lulus (319 + 8 baru); build 0 galat | `PASS` | Keluaran `dotnet test` |
| QBE `ReportOnly` dengan probe `ZzzQbeProbeController.cs` (controller memakai `ApplicationDbContext` dan `Count() + 1`) | Probe: 2 `REVIEW` `QBE-SVC-001` + 1 `VIOLATION` `QBE-CODE-003`; tiga berkas asli tanpa temuan | `PASS` (kontrol negatif) | Keluaran checker |
| QBE `Strict` atas tiga berkas source (probe sudah dihapus) | 0 violation, 0 review; `Final result: PASS` | `PASS` | Keluaran checker |
| Kueri baca-saja devYoga sebelum startup (`BEGIN READ ONLY` … `ROLLBACK`) | 0 definisi perjalanan dinas | Baseline | `q374.mjs` |
| Startup Development pertama (`dotnet bin/Debug/net9.0/QuilvianSystemBackend.dll`) | "Seeder definisi workflow Perjalanan Dinas: 1 definisi, 1 tahap ditambahkan."; "Permission registry valid. 2563 identitas kanonik dari 2563 kemampuan terdaftar." Satu-satunya `ERR` = penjadwal kehadiran tanpa `SystemActorUserId` (kondisi devYoga yang sudah ada, tidak terkait) | `PASS` (AC1, AC3) | `be374.log` |
| Kueri baca-saja devYoga sesudah startup | Definisi `5b1d7e30-0001-4c2a-9f10-7a3e2d4c6b01` `BUSINESS_TRAVEL_REQUEST`, "Persetujuan Perjalanan Dinas", `HRD_TRV_REQUEST`, `HumanResource`, v1, `Active`, aktif, default, tanpa lingkup; tahap `HRD-TRV-01` "Atasan langsung pemohon", `RequesterManager`, `Any`, 1, tanpa self-approval, 48 jam | `PASS` (AC1) | `q374.mjs` |
| `GET api/v1/corporate/human-resource/master-data/workflow-definitions?search=BUSINESS_TRAVEL` (superadmin) | `200`; satu baris "Persetujuan Perjalanan Dinas", `Active`, aktif — tampil di Master Workflow | `PASS` (AC1) | `a374.mjs` |
| Startup Development kedua | "… 0 definisi, 0 tahap ditambahkan." | `PASS` (AC1) | `be374b.log` |

Backend yang dinyalakan untuk verifikasi sudah dimatikan kembali. Tidak ada server lain yang berjalan saat build.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Startup pertama menanam satu definisi `BUSINESS_TRAVEL_REQUEST` dengan satu tahap sesuai outcome; startup kedua tidak menambah apa pun | Terpenuhi | Harness + dua startup devYoga + kueri baca-saja + API Master Workflow |
| 2. Definisi yang sudah diubah admin (tahap ditambah, dinonaktifkan) tidak ditimpa seeder | Terpenuhi (harness) | Tiga uji + K2/K3; tidak diuji dengan mengubah data devYoga (tanpa wewenang tulis DB) |
| 3. Kegagalan seeder tercatat di log startup dan tidak menghentikan aplikasi | Terpenuhi lewat mekanisme yang ada | Seeder dipanggil melalui `RunStartupSeederAsync` (log `[StartupSeed]` per seeder, `Seeders:ContinueOnFailure` untuk pengembangan) — perilaku bersama yang tidak diubah; jalur gagal tidak dipicu sengaja di devYoga |
| Migration | Tidak ada migration. Satu definisi dan satu tahap ditulis ke devYoga **hanya** lewat startup aplikasi (bukan eksekusi DB langsung) | — |
| Laporan | Berkas ini | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data yang tertinggal di devYoga | Definisi `BUSINESS_TRAVEL_REQUEST` + tahap `HRD-TRV-01` (data master yang memang diperlukan `BE-HRD-375` dst.); akan tertanam juga di lingkungan lain saat backend start |
| Pertanyaan terbuka | `HRD-Q-222` — bila pemilik memilih definisi ditanam nonaktif, ubah `IsActive`/`WorkflowStatus` pada seeder; untuk lingkungan yang sudah tertanam, admin menonaktifkannya di Master Workflow (seeder tidak menimpa) |
| Risiko tersisa | Pemohon tanpa atasan langsung ditolak engine saat Ajukan (`VR-TRV-29`) — ditangani `BE-HRD-375` |
| Skrip bukti (scratchpad, tidak di-commit) | `mut374.js` (kontrol negatif), `q374.mjs` (kueri baca-saja), `a374.mjs` (GET Master Workflow) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `BE-HRD-375` — SPPD dibuat sebagai Draf, isi terkunci, endpoint Ajukan (boleh paralel: `BE-HRD-379`) |
