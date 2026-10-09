# Laporan Perubahan Backend — `BE-PAT-MIG-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PAT-MIG-001` |
| Judul | RSMMC Pilot MRN Reconciliation |
| Slice | S1 — Mekanisme rekonsiliasi MRN Pilot |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md), kartu `BE-PAT-MIG-001` |
| Trace | `PAT-DEC-001`–`PAT-DEC-016`, `PAT-OQ-001`–`PAT-OQ-007`, requirement `PAT-MIG-AC-01`–`PAT-MIG-AC-28` |
| Contract version | `contracts/api-contract.md` `1.1.0` — `approved` final 8 Oktober 2026 |
| Dependency | Tidak ada task prasyarat. Prasyarat data `migration.rsmmc_*` dilaporkan pemilik, **tidak** diverifikasi pada task ini |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika bisnis kompleks (2), kontrak API yang sudah disetujui (1), perilaku persistence yang ada (1), berkaitan dengan keamanan (1), satu repository (0), tanpa UI (0) |
| Task mode | `BACKEND` |
| Target tulis | **Saat ini:** worktree `NewQuilvianSystemBackendPatMigration`, branch `BE-PAT-MIG-001`, dibuat dari `origin/QuilvianIntegrationBackend` (bagian 9). Riwayat: pengerjaan pertama di branch `QuilvianStaDeploy` (worktree `NewQuilvianSystemBackendDeployStaging`), kini hanya rujukan baca |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | **Saat ini:** `179aea3fe3912832f1230b0b7d19a132dd651839` (`QuilvianIntegrationBackend`) — perubahan belum di-commit. Riwayat: `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` (`QuilvianStaDeploy`) |
| Tanggal | 8 Oktober 2026 (pengerjaan pertama dan koreksi owner review); 9 Oktober 2026 (port semantik ke Integration, bagian 9) |
| Status | ✅ **SELESAI 9 Oktober 2026 untuk cakupan task ini — implementasi source.** **SOURCE IMPLEMENTATION: APPROVED** — review source leader `APPROVED` 9 Oktober 2026 (bagian 10), di atas port ke baseline Integration yang divalidasi ulang (bagian 9: test `73/73`, build `0` error, QBE `Strict` `PASS`). **RUNTIME: PENDING** — rekonsiliasi **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION**; `PAT-GATE-001` tetap **OPEN**. Migrasi RSMMC secara keseluruhan **belum** selesai |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `PatientManagement` |
| Submodule | `MasterData` (folder source pemilik `PatientController`) |
| Pemilik/prefix registry | `PatientManagement operational` / `Pat` — `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 24) |
| Klasifikasi bisnis | `LEGACY MIGRATION` — migrasi data pasien lama |
| Keberlakuan QBE | Per berkas, lihat tabel di bawah. Tidak ada *rename* source maupun tabel, sehingga `QBE-NAM-003`, `QBE-DB-001`, dan `QBE-DB-002` tidak berlaku |
| Entity persisted baru | Tidak ada. `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001`–`004`, `QBE-MOD-002`–`003` tidak berlaku |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, `rules/README.md`, `rules/GLOBAL_RULES.md` (indeks), `TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`, `TEST_POLICY`, `role-access-rules`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry |

### Keberlakuan per berkas

| Berkas | Keberlakuan | QBE yang diterapkan |
| --- | --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | `TOUCHED LEGACY` (dibatasi) | `QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001` (aksi baru tidak menyentuh `DbContext`) |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcPilotMrnReconciliationDtos.cs` | `NEW CODE` | `QBE-DTO-001`, `QBE-VAL-001`, `QBE-API-001` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMrnReconciliationService.cs` | `NEW CODE` | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMigrationSource.cs` | `NEW CODE` — membaca infrastruktur migrasi yang sudah ada, tanpa mutasi skema | `QBE-SVC-001`; `PAT-OQ-007` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotQrArtifactStore.cs` | `NEW CODE` | `PAT-OQ-005` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotEnvironmentGate.cs` | `NEW CODE` | `PAT-OQ-002`, `PAT-OQ-006` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotApprovedBatchProfile.cs` | `NEW CODE` | Gerbang batch kanonik |
| `Program.cs` | `TOUCHED LEGACY` — registrasi DI saja | — |
| `.gitignore` | Konfigurasi repository | Mengikuti konvensi tiga project test yang sudah ada |
| `Tests/QuilvianSystemBackend.PatientManagementTests/**` | `NEW CODE` lingkup test | `TEST_POLICY` bagian 3 — diminta eksplisit pemilik |

`QBE-CODE-001` sampai `QBE-CODE-006` tidak berlaku: task ini **tidak** membangkitkan nomor bisnis. MRN tujuan dibaca dari tabel perencana, dan pembangkit MRN maupun `PatientCode` tidak dipanggil.

---

## 1. Masalah yang diperbaiki

Uji coba pertama migrasi RSMMC (Pilot V1) memasukkan 715 pasien lewat API pembuatan pasien biasa. API itu membuat MRN sendiri, sehingga 715 pasien mendapat nomor buatan Quilvian, bukan nomor yang sudah direncanakan pada `migration.rsmmc_patient_mrn_plan`. Nomor-nomor buatan itu ternyata dibutuhkan pasien RSMMC lain, sehingga sisa 704.508 pasien tidak dapat dimigrasikan sebelum 715 pasien Pilot pindah ke nomor yang benar.

Sebelum task ini, tidak ada mekanisme yang aman untuk memindahkan MRN tersebut. Mengubahnya lewat endpoint `PUT` pasien tidak mungkin karena endpoint itu tidak menerima MRN, dan mengubahnya lewat SQL langsung akan meninggalkan QR yang masih bertuliskan nomor lama.

**Contoh dengan data samaran.** Pasien "Budi Contoh" (`legacy_pid` 1001) bernomor `00-00-07-01` buatan Quilvian. Rencana kanonik menetapkan `00-79-70-15`. Sesudah rekonsiliasi, nomornya `00-79-70-15`, QR baru ada di `patient-qrcodes/00-79-70-15/qrcode.png`, dan QR lama di `patient-qrcodes/00-00-07-01/` tetap ada.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| **Tujuan** | 715 pasien Pilot berpindah ke MRN kanonik tanpa kehilangan identitas, relasi, atau QR lama |
| **Pelaku** | Admin migrasi yang sudah login dan punya izin `Patient : Update`. Eksekusi sungguhan butuh otorisasi terpisah pemilik (`PAT-GATE-001`) |
| **Pemicu** | Admin memanggil `POST …/admin/migration/rsmmc-pilot-mrn/reconcile` — **belum** dilakukan pada task ini |
| **Prasyarat** | Environment bernama persis `Staging`; tabel `migration.rsmmc_*` tersedia; keadaan batch sesuai yang disetujui |

### 2.1 Langkah

1. Sistem memvalidasi masukan: `batchId` wajib, `limit` 1–100 dengan bawaan 25, `dryRun` bawaan `true`.
2. Sistem memastikan environment bernama persis `Staging`. Bila tidak, permintaan ditolak **sebelum** satu tabel pun dibaca.
3. Untuk eksekusi, sistem memastikan aktor pengguna dikenali, supaya `UpdateBy` dan log mencatat siapa yang mengubah.
4. Sistem membaca baris perencana dan crosswalk untuk batch itu, lalu memuat `MstPatient` yang cocok. Hasilnya **set Pilot**, diurutkan menurut `legacy_pid` naik.
5. Sistem menghitung sebelas gerbang batch: batch kanonik, 705.223 baris perencana `FINALIZED`, 715 baris cadangan, 715 pasien Pilot, keunikan `legacy_pid`/`Id`/MRN tujuan, serta MRN tujuan terisi dan berformat `00-00-00-00`. Pada eksekusi, satu gerbang gagal saja sudah menolak seluruh permintaan tanpa perubahan apa pun.
6. Sistem mengklasifikasikan **seluruh** set Pilot: `ALREADY_RECONCILED`, `QR_INCONSISTENT`, atau layak direkonsiliasi.
7. Sistem memilih paling banyak `limit` pasien layak. Pasien kanonik tidak menghabiskan jatah ini.
8. Pada simulasi, sistem melaporkan `READY` atau `CONFLICT` untuk setiap pasien terpilih, lalu selesai **tanpa** mengubah database dan tanpa menyentuh filesystem.
9. Pada eksekusi, untuk setiap pasien terpilih secara berurutan:
   1. membuka transaksi untuk pasien ini saja;
   2. membaca ulang baris perencana dan crosswalk, lalu memastikan status masih `FINALIZED`, MRN tujuan sama, dan pemetaan pasien sama;
   3. membaca ulang pasien, lalu memastikan MRN, `PatientCode`, dan path QR-nya masih sama dengan saat dipilih;
   4. memastikan MRN tujuan tidak dimiliki pasien lain, termasuk pasien yang sudah ditandai terhapus;
   5. memeriksa path QR tujuan, membentuk PNG, lalu menulisnya dengan `FileMode.CreateNew`;
   6. mengubah empat kolom, menyimpan, lalu *commit*.
10. Proses berhenti pada `CONFLICT` atau `FAILED` pertama. Pasien yang sudah *commit* tetap tersimpan.

### 2.2 Perubahan status pasien

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| MRN buatan Quilvian | Simulasi | `READY` (laporan saja) | Pemegang `Patient : Update` | Environment `Staging` |
| MRN buatan Quilvian | Eksekusi | `RECONCILED` | Pemegang `Patient : Update` | Environment `Staging`, seluruh gerbang lolos, aktor dikenali |
| MRN kanonik, QR sesuai | Panggilan ulang | `ALREADY_RECONCILED` (tidak berubah) | — | — |
| MRN kanonik, QR tidak sesuai | Panggilan apa pun | `QR_INCONSISTENT` (dilaporkan, tidak diperbaiki) | — | — |

### 2.3 Jalur tidak normal

| Kejadian | Status | Data pasien | QR lama | QR baru |
| --- | --- | --- | --- | --- |
| MRN tujuan dimiliki pasien lain | `CONFLICT` / `MRN_OWNED_BY_ANOTHER_PATIENT` | Tetap | Tetap | Tidak dibuat |
| Artefak QR tujuan sudah ada | `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS` | Tetap | Tetap | Tidak dibuat; file yang ada tidak ditimpa |
| Artefak QR tujuan muncul di antara pemeriksaan dan penulisan | `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS` | Tetap | Tetap | `CreateNew` ditolak; file proses lain tidak ditimpa |
| PNG QR gagal dibentuk | `FAILED` / tahap `CREATE_QR` | Tetap | Tetap | Tidak ada berkas |
| `SaveChanges` gagal sesudah QR dibuat | `FAILED` / tahap `SAVE_CHANGES` | Tetap — transaksi dibatalkan | Tetap | Dihapus *best-effort* **hanya bila** kepemilikan terbukti: `CreatedByThisInvocation = true`, berkas masih ada, panjang sama, **dan** SHA-256 isinya sama dengan byte yang ditulis pemanggilan ini. Bila tidak terbukti, tidak dihapus dan dicatat sebagai ketidakpastian pembersihan |
| *Commit* gagal | `FAILED` / tahap `COMMIT_OUTCOME_UNCERTAIN` | Tidak dapat dipastikan | Tetap | **Dipertahankan.** Tidak dihapus otomatis, tidak dicoba ulang, proses berhenti. Operator merekonsiliasi kedua QR |
| Perencana atau crosswalk berubah sesudah pemilihan | `FAILED` / tahap `REVALIDATE_PLAN` | Tetap | Tetap | Tidak dibuat |
| Pasien ke-2 gagal dari 3 terpilih | Pasien 1 `RECONCILED`, pasien 2 `FAILED`, pasien 3 tidak diproses | Pasien 1 berubah, 2 dan 3 tetap | Tetap | Ada untuk pasien 1 saja |

### 2.4 Hasil akhir

Untuk setiap pasien yang berhasil, `MedicalRecordNumber` dan `QrCodePath` menunjuk nomor kanonik, `UpdateDateTime` dan `UpdateBy` terisi aktor, QR baru tersedia, dan QR lama tetap ada untuk pemulihan. Tabel perencana, crosswalk, dan cadangan tidak berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: daftar pada Backend Governance Preflight, ditambah paket `docs/module-blueprints/patient-management/` (manifest revision 2, keputusan revision 2, kontrak `1.1.0`, roadmap revision 2, traceability).
- Source Patient: `PatientController.cs` (constructor, `CreatePatient`, `CreatePatientForAdmin`, `UpdatePatient`, `DeletePatient`, `SavePatientQrCodeFile`, `BuildPatientQrPayload`, `GenerateMedicalRecordNumberAsync`, `GenerateQrCodePngBytes` — kini `RenderQrCodePngBytes`, `ResolveQrLogoPath`, `GetFileStoragePaths`, `CombineUrlPath`, `SanitizePathSegment`, `DeletePhysicalFileIfExists`, `GeneratePatientCodeAsync`, `GetCurrentUserId`), `MstPatient.cs`, `Models/IdentityModel.cs`, `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs`, `PatientDtos.cs`.
- Infrastruktur: `Services/Logging/LoggerService.cs`, `Responses/ApiResponse.cs`, `Attributes/AccessPermissionAttribute.cs`, `Attributes/AccessActionAttribute.cs`, `Seeders/AccessMenuSeeder.cs`, `Services/Security/PermissionRegistryDescriptor.cs`, `Program.cs`, `QuilvianSystemBackend.csproj`, `.gitignore`, `tooling/qbe/Invoke-QbeConformanceCheck.ps1`.
- Pola terdekat: `Areas/HealthServices/MedicalRecordManagement/Controllers/MedicalRecordBackfillController.cs` (endpoint perawatan admin dengan `isDryRun` bawaan `true`), `LabOrderNumberService.cs` (`Database.SqlQuery<T>`), `KioskPatientLookupService.cs` (`FromSqlInterpolated`), `NutritionMasterController.cs` (`[FromServices]`).
- Test: `Tests/QuilvianSystemBackend.PharmacyTests/` (csproj, `TestDatabase`, `PermissionMatrixTests`, `PharmacyHarness`).
- Pencarian: `rsmmc_patient`, `crosswalk`, `mrn_plan`, `migration.` pada seluruh source — **tidak ditemukan** utilitas rekonsiliasi atau pemetaan tabel migrasi yang sudah ada.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Aksi baru `ReconcileRsmmcPilotMrn` diletakkan sesudah `DeletePatient`; factory `CreateRsmmcPilotQrArtifactStore`; konstanta `PatientQrCodeFileName` dipakai juga oleh `SavePatientQrCodeFile` (nilai sama, `qrcode.png`); `GetPublicRequestPath()` diekstrak dari `GetFileStoragePaths()` dengan perilaku identik, supaya simulasi dapat menghitung path publik tanpa membuat folder; method `private static` `GenerateQrCodePngBytes` di-rename menjadi `RenderQrCodePngBytes` (koreksi owner review, lihat bagian 8) — isi method dan label log `step = "GenerateQrCodePngBytes"` tidak berubah |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcPilotMrnReconciliationDtos.cs` | **Baru.** Request, response, item, gerbang, konstanta status dan jenis konflik |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMrnReconciliationService.cs` | **Baru.** Orkestrasi validasi, gerbang, klasifikasi, satu transaksi per pasien, pembersihan, log |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMigrationSource.cs` | **Baru.** Pembaca `migration.rsmmc_*` lewat `Database.SqlQuery<T>` berparameter; hanya `SELECT` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotQrArtifactStore.cs` | **Baru.** Pembuatan QR tanpa penimpaan (`FileMode.CreateNew`) dan pembersihan yang dibatasi kepemilikan |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotEnvironmentGate.cs` | **Baru.** Daftar izin positif: `Staging` persis, ordinal |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotApprovedBatchProfile.cs` | **Baru.** Ekspektasi batch kanonik `45eeefba-…-d986851f7c1f`: 705.223 / 715 / 715 |
| `Program.cs` | Satu `using` dan tiga registrasi DI: profil batch (singleton), pembaca migrasi, service (scoped) |
| `.gitignore` | Whitelist `Tests/QuilvianSystemBackend.PatientManagementTests/` beserta `bin/` dan `obj/`, mengikuti tiga blok project test yang ada. Tanpanya project test diabaikan Git oleh aturan `/Tests/*` |
| `Tests/QuilvianSystemBackend.PatientManagementTests/` | **Baru.** Project xUnit khusus (8 berkas): csproj, `Infrastructure/TestDatabase.cs`, `Infrastructure/TiruanSumberMigrasiRsmmc.cs`, `Infrastructure/RekonsiliasiHarness.cs`, `Infrastructure/UjiPasien.cs`, dan empat kelas test |
| `docs/module-blueprints/patient-management/task/report/backend/BE-PAT-MIG-001.md` | **Baru.** Laporan ini |
| `docs/module-blueprints/patient-management/roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md` | Baris status dan tautan bukti saja |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru sesuai kontrak `1.1.0`, rute sama persis dengan rencana. Endpoint lain tidak berubah. Nama status `READY`, `RECONCILED`, `FAILED` mengisi butir `DEV_DISCRETION` kontrak bagian 4.3, sesuai daftar status pada instruksi task |
| Database | **Tidak ada** perubahan skema, entity, `DbSet`, konfigurasi EF, maupun EF migration. Tabel `migration.rsmmc_*` hanya dibaca lewat `Database.SqlQuery<T>`, yang memetakan hasil ke tipe di luar model. Belum ada eksekusi apa pun terhadap database mana pun |
| Keamanan/Auth | Login wajib (`[Authorize]` controller). Izin `Patient : Update` yang sudah ada. Gerbang environment `Staging` persis untuk kedua mode. Eksekusi menolak aktor yang tidak dikenali. Tidak ada `[AllowAnonymous]`, `IsInRole`, atau nama peran |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin/migration/rsmmc-pilot-mrn/reconcile` | Mensimulasikan atau menjalankan pemindahan MRN pasien Pilot RSMMC ke nomor kanonik, paling banyak `limit` pasien per panggilan | `Patient : Update` |

**Contoh body.**

```json
{ "batchId": "45eeefba-dd39-6831-1bd2-d986851f7c1f", "dryRun": true, "limit": 25 }
```

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Simulasi atau eksekusi selesai, termasuk bila berhenti di tengah karena konflik atau kegagalan. Isinya ringkasan, sebelas gerbang, dan daftar pasien |
| `400` | Isian tidak sah (`batchId` kosong, `limit` di luar 1–100), atau aktor tidak dikenali pada eksekusi |
| `401` | Belum login |
| `403` | Tidak punya izin `Patient : Update`, **atau** environment bukan `Staging` persis (kode `RSMMC_PILOT_ENVIRONMENT_NOT_ALLOWED`) |
| `409` | Eksekusi ditolak karena gerbang batch gagal (kode `RSMMC_PILOT_BATCH_GATE_FAILED`). Tidak ada data yang diubah; daftar gerbang ada di `errors.gates` |

Endpoint ini **belum pernah dipanggil**.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Preflight Git | Branch `QuilvianStaDeploy`, HEAD `103b45cc`, remote `DevBenari/NewQuilvianSystemBackend`, hanya dua path untracked yang diketahui | `PASS` | `git status --short`, `.git/HEAD`, `git config` remote |
| `dotnet build -p:RunAnalyzers=false` (build pertama sesudah source) | `0 Error(s)`, `242 Warning(s)`, 3 menit 3 detik | `PASS` | Warning yang terlihat berasal dari berkas lama (`CS1573`/`CS1574`/`CS1734` komentar XML) |
| `dotnet build -p:RunAnalyzers=false --no-incremental` (build penuh akhir, sesudah project test ditambahkan) | `Build succeeded.` `0 Error(s)`, `242 Warning(s)`, 1 menit 57 detik — jumlah warning sama dengan build pertama | `PASS` | Log lengkap dianalisis per berkas: **nol** warning dari tujuh berkas baru. Satu-satunya warning pada berkas yang disentuh, `PatientController.cs(2611) CS8619`, ada di `GetActorNameMapAsync` dari commit `722a2dd92` (10 Juni 2026) dan tidak termasuk diff — `EXISTING / ENVIRONMENT ISSUE` |
| `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj -p:RunAnalyzers=false` | `Total tests: 66`, `Passed: 66`, `Failed: 0`, `Skipped: 0`, 1,45 menit | `PASS` | Keluaran VSTest |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly -Path` (8 berkas source) | `VIOLATION: 1`, `REVIEW: 0`, `Final result: PASS` (ReportOnly) | `PASS` dengan satu temuan positif palsu | Lihat bagian 7 — `QBE-CODE-002` |
| Pemeriksaan artefak QR | Tidak ada perubahan di `Storage/`, tidak ada `.png` baru di worktree, folder uji sementara terhapus | `PASS` | `git status --short --untracked-files=all` |
| Verifikasi kontrak izin | `[Authorize]`, `ControllerName = "Patient"`, `[AccessPermission("Patient","Update")]`, `[AccessAction("Update", …, AccessType = AccessTypes.Update)]`, `VisibleInRoleAccess = true`, `IsSystemOnly = false` | `PASS` | Test `Endpoint_MenuntutLoginDanIzinPatientUpdateYangSudahAda` |

AUTOMATED TEST: `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj` — `PASS` (66/66), dibuat atas permintaan eksplisit pemilik (`AC-27`, `PAT-OQ-001`).

**`TEST_POLICY` guard (`PAT-OQ-001`).** `rules/backend/TEST_POLICY.md` dibaca ulang sebelum project test dibuat. Bagian 3 terpenuhi: ada instruksi langsung pada task aktif, dan `AC-27` disetujui pemilik pada kartu task. Project tidak didaftarkan di `QuilvianSystemBackend.sln` (dilarang bagian 2, dan sama dengan tiga project test yang ada), dan tidak ada workflow CI baru. Versi paket test sama persis dengan `PharmacyTests`.

Uji manual: `NOT APPLICABLE` — pemanggilan endpoint dan eksekusi database tidak diberi wewenang pada task ini.

**Tidak dijalankan:** pemanggilan endpoint; koneksi ke database staging atau mana pun; `dotnet ef` dalam bentuk apa pun; deployment. Teks SQL `RsmmcPilotMigrationSource` **belum** pernah dijalankan terhadap PostgreSQL — lihat risiko pada bagian 7.

### 5.1 Pemetaan 37 topik test yang diminta

| No | Topik | Test |
| ---: | --- | --- |
| 1 | Simulasi tidak mengubah `MstPatient` | `Simulasi_TidakMengubahSatuPunMstPatient` |
| 2 | Simulasi tidak menulis filesystem | `Simulasi_TidakMenulisFolderMaupunFileQr` |
| 3 | `dryRun` tidak dikirim berarti `true` | `DryRunTidakDikirim_DianggapSimulasi` |
| 4 | `limit` < 1 ditolak | `LimitDiBawahSatu_Ditolak` (0, −1), `AtributValidasiDto_MembatasiLimit` |
| 5 | `limit` > 100 ditolak | `LimitDiAtasSeratus_Ditolak` (101, 1000), `AtributValidasiDto_MembatasiLimit` |
| 6 | `Staging` persis diterima | `LingkunganStagingPersis_Diterima`, `GerbangLingkungan_HanyaStagingPersis` |
| 7 | `staging` ditolak | `LingkunganSelainStagingPersis_DitolakSebelumMembacaApaPun` (`staging`, `STAGING`) |
| 8 | `Production` ditolak | idem (`Production`, kedua mode) |
| 9 | `Development` ditolak | idem (`Development`, kedua mode) |
| 10 | Kosong/tidak dikenal ditolak | idem (`""`, `Stagging`, `Staging-Pilot`) dan `GerbangLingkungan_HanyaStagingPersis` (`null`, spasi) |
| 11 | Login dan izin mengikuti konvensi | `Endpoint_MenuntutLoginDanIzinPatientUpdateYangSudahAda`, `EksekusiTanpaAktorTerkenali_Ditolak` |
| 12 | Tabrakan MRN tujuan ditolak | `MrnKanonikDipakaiPasienLain_Conflict_TanpaPerubahanDanBerhenti`, `Simulasi_MelaporkanKonflikMrnTanpaMengubahData` |
| 13 | `MstPatient.Id` tetap | `Berhasil_MrnDanQrPindahKeKanonik_IdentitasTetap` |
| 14 | `PatientCode` tetap | idem, `TidakMemakaiPembangkitMrnMaupunPatientCodeNormal_TidakMenambahPasien` |
| 15 | MRN berubah lama → kanonik | `Berhasil_MrnDanQrPindahKeKanonik_IdentitasTetap` |
| 16 | Hanya kolom yang diizinkan berubah | idem — seluruh kolom skalar dibandingkan lewat `UjiPasien.KolomBerubah` |
| 17 | Path QR menjadi path MRN kanonik | idem, `PathPublik_SamaDenganPolaPembuatanPasien` |
| 18 | Isi QR memakai MRN kanonik | `QrBaru_IsiDanFolderMemakaiMrnKanonik`, `PayloadProduksi_AdalahMrnKanonik`, `PembuatPngProduksi_MenghasilkanPngUntukMrnKanonik` |
| 19 | QR tujuan sudah ada → `CONFLICT` tanpa menimpa | `ArtefakQrTujuanSudahAda_Conflict_TanpaMenimpaDanTanpaMemanggilPembuatQr`, `CreateNew_ArtefakSudahAda_KonflikDanIsiTidakBerubah` |
| 20 | Race pada batas pembuatan akhir tidak menimpa | `ArtefakQrMunculDiAntaraPemeriksaanDanPenulisan_Conflict_TanpaMenimpa`, `CreateNew_ArtefakMunculSaatPembentukan_KonflikRaceDanIsiTidakBerubah` |
| 21 | Gagal membuat QR → database tetap | `GagalMembuatQr_Failed_DataPasienTetapDanTidakAdaBerkasBaru` |
| 22 | Database gagal sesudah QR → hanya QR baru dibersihkan | `GagalSimpanSesudahQrDibuat_HanyaQrBaruDibersihkan_QrLamaTetap`, `GagalSimpan_QrBaruDigantiIsiSamaPanjang_TidakDihapusDanDilaporkan`, `GagalCommit_HasilTidakPasti_QrBaruDanLamaDipertahankan_ProsesBerhenti`, dan enam test `TryDeleteCreated_*`/`CreateNew_*` pada bagian 8.2 (dua test lama di-rename: `…HanyaMenghapusArtefakMiliknya…` → `…ArtefakPersisMiliknya…`, `…BerkasSudahDigantiPihakLain…` → `…PanjangBerbeda…`) |
| 23 | QR lama tidak tersentuh | `QrLama_TetapAdaDanTidakBerubah`, dan seluruh test konflik/gagal |
| 24 | `normalized_mrn` tidak dipakai | `MrnTujuan_DiambilDariPerencana_BukanNormalizedMrnCrosswalk`, `SqlPembacaMigrasi_TidakMemakaiNormalizedMrn_DanNilaiMasukanBerparameter` |
| 25 | Crosswalk tidak berubah | `CrosswalkPerencanaDanCadangan_TidakBerubah`, `SqlPembacaMigrasi_HanyaMembaca`, `AntarmukaSumberMigrasi_TidakPunyaOperasiTulis` |
| 26 | Perencana tidak berubah | idem |
| 27 | Cadangan tidak berubah | idem |
| 28 | Kanonik + QR benar → `ALREADY_RECONCILED` | `MrnSudahKanonikDanQrBenar_AlreadyReconciled_TanpaPerubahan`, `PanggilanUlang_Idempoten_TidakAdaPerubahanKedua` |
| 29 | Kanonik + QR salah → `QR_INCONSISTENT` | `MrnKanonikTetapiQrSalah_QrInconsistent_DilaporkanTanpaDiperbaiki` |
| 30 | `ALREADY_RECONCILED` tidak menghabiskan `limit` | `PasienKanonikDanQrInconsistent_TidakMenghabiskanLimit` |
| 31 | `QR_INCONSISTENT` tidak menghabiskan `limit` | idem |
| 32 | Urutan `legacy_pid` naik deterministik | `Simulasi_MenampilkanIsiWajibDanUrutLegacyPidNaik` (tiruan sumber sengaja mengembalikan urutan terbalik; `2 < 101 < 1010` secara angka) |
| 33 | Paling banyak `limit` diproses | `PalingBanyakLimitPasienDiproses_SisanyaTidakTersentuh` |
| 34 | Kegagalan pertama menghentikan proses | `KegagalanPertama_MenghentikanProses_PasienSebelumnyaTetapTersimpan` |
| 35 | Pasien yang sudah *commit* tidak ikut dibatalkan | idem |
| 36 | Pembangkit MRN normal tidak dipakai | `TidakMemakaiPembangkitMrnMaupunPatientCodeNormal_TidakMenambahPasien` |
| 37 | Pembangkit `PatientCode` tidak dipakai | idem — daftar seluruh `PatientCode` sama sebelum dan sesudah |

Test tambahan di luar daftar: gerbang batch menolak eksekusi (`GerbangBatchGagal_EksekusiDitolakTanpaPerubahan`, `BatchSelainBatchKanonik_EksekusiDitolak`), revalidasi perencana dan crosswalk (`PerencanaBerubahSesudahPemilihan_Failed_TanpaPerubahan`, `PemetaanCrosswalkBerubahSesudahPemilihan_Failed_TanpaPerubahan`), letak aksi terhadap label Akses Role (`Endpoint_DiletakkanSesudahAksiUpdateLama_SupayaLabelAksesRoleTidakBerubah`), `batchId` kosong, dan simulasi pada batch lain.

**Cara test bekerja.** Database memakai SQLite in-memory dari konfigurasi EF aplikasi, sama dengan uji Farmasi. Tabel `migration.rsmmc_*` diganti tiruan di memori (`TiruanSumberMigrasiRsmmc`), karena tabel itu tidak ada di model EF dan task ini tidak boleh mengakses database staging. Folder QR memakai direktori sementara sistem operasi yang dihapus sesudah tiap test. Fungsi format QR diambil langsung dari `PatientController` lewat refleksi, jadi test memakai format bisnis yang sebenarnya, bukan salinannya. Tidak ada data migrasi sungguhan yang dipakai.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-01` Aturan memilih baris | Terpenuhi (source + test) | `RsmmcPilotMigrationSource.MappedFinalizedRowsSql` (syarat 1–4), `LoadPilotSnapshotAsync` (syarat 5), klasifikasi pada `ReconcileAsync` (syarat 6); topik test 32, 33 |
| `AC-02` MRN tujuan hanya dari perencana | Terpenuhi | `RsmmcPilotPlanRow` tidak punya `normalized_mrn`; topik test 24 |
| `AC-03` `dryRun=true` tanpa mutasi | Terpenuhi | Cabang simulasi kembali sebelum transaksi; `resolveStorage` tidak pernah dipanggil; topik 1–3 |
| `AC-04` Baris deterministik dengan isi minimal | Terpenuhi | `RsmmcPilotMrnReconcileItemResponse`; `SortItems`; topik 32 |
| `AC-05` Hanya non-produksi terbukti, gagal tertutup | Terpenuhi | `RsmmcPilotEnvironmentGate` (ordinal `Staging`); topik 6–10 |
| `AC-06` Login + `Patient : Update` | Terpenuhi | Atribut pada `ReconcileRsmmcPilotMrn`; topik 11 |
| `AC-07` Identitas, demografi, relasi, `CreateDateTime`, `CreateBy` tetap | Terpenuhi | Hanya empat kolom yang diisi pada tahap `UPDATE_PATIENT`; topik 13, 14, 16 |
| `AC-08` Hanya empat kolom berubah | Terpenuhi | idem; topik 16 |
| `AC-09` QR baru dari MRN kanonik | Terpenuhi | `RsmmcPilotQrArtifactStore.CreateNew` memakai `BuildPatientQrPayload`, `SanitizePathSegment`, `CombineUrlPath`, `RenderQrCodePngBytes` (dahulu `GenerateQrCodePngBytes`); topik 17, 18 |
| `AC-10` QR lama tidak tersentuh | Terpenuhi | Store tidak pernah menerima path lama; pembersihan hanya artefak bukti kepemilikan; topik 23 |
| `AC-11` QR gagal → pasien tetap | Terpenuhi | PNG dibentuk sebelum perubahan entity; topik 21 |
| `AC-12` Database gagal → hanya QR baru dibersihkan | Terpenuhi | `CleanupCreatedQrAsync` + `TryDeleteCreated` dengan bukti kepemilikan penanda + panjang + SHA-256; *commit* tidak pasti tidak pernah menghapus; topik 22 |
| `AC-13` Artefak QR tujuan tidak ditimpa diam-diam | Terpenuhi | Pemeriksaan awal + `FileMode.CreateNew`; topik 19, 20 |
| `AC-14` Dapat dilanjutkan/idempoten, `ALREADY_RECONCILED` | Terpenuhi | Klasifikasi seluruh set Pilot; topik 28 |
| `AC-15` `QR_INCONSISTENT` dilaporkan | Terpenuhi | topik 29 |
| `AC-16` Pembangkit MRN normal tidak dipanggil | Terpenuhi | Service tidak merujuk `PatientController.GenerateMedicalRecordNumberAsync` (private, tidak terjangkau); topik 36 |
| `AC-17` Tidak ada `PatientCode` baru | Terpenuhi | topik 37 |
| `AC-18` Tidak ada `MstPatient` baru | Terpenuhi | Service hanya memuat dan mengubah entity yang ada; jumlah pasien sama (topik 36) |
| `AC-19` Tidak ada hapus/buat ulang `MstPatient` | Terpenuhi | Tidak ada `Add`/`Remove` pada service; `Id` sama (topik 13) |
| `AC-20` Crosswalk tidak diubah | Terpenuhi | Hanya `SELECT`; topik 25 |
| `AC-21` Perencana tidak diubah | Terpenuhi | idem; topik 26 |
| `AC-22` Cadangan tidak diubah | Terpenuhi | Hanya `count(*)`; topik 27 |
| `AC-23` 704.508 pasien lain di luar cakupan | Terpenuhi | Review diff: tidak ada pembuatan pasien atau alokasi `PatientCode` |
| `AC-24` Paling banyak `limit`, urutan deterministik | Terpenuhi | topik 30–33 |
| `AC-25` Satu transaksi per pasien | Terpenuhi | `ReconcileOneAsync` membuka transaksi sendiri; topik 35 |
| `AC-26` Berhenti pada kegagalan tak terduga pertama | Terpenuhi | `break` sesudah status bukan `RECONCILED`; topik 34 |
| `AC-27` Test terfokus | Terpenuhi | 72 test sesudah koreksi (66 pada pengerjaan pertama), 37 topik pada bagian 5.1 ditambah test kepemilikan bagian 8.2 |
| `AC-28` `dotnet build -p:RunAnalyzers=false` + test | Terpenuhi | Bagian 5 |

| Definition of Done — fase implementasi | Status |
| --- | --- |
| Endpoint diimplementasikan sesuai kontrak `1.1.0` | Terpenuhi |
| Test terfokus lulus | Terpenuhi — 72/72 sesudah koreksi (66/66 pada pengerjaan pertama) |
| Build backend lulus | Terpenuhi — 0 error |
| Tidak ada perubahan berkas yang tidak dimaksud | Terpenuhi — lihat bagian 3.2 dan status Git |
| Laporan task implementasi lengkap | Terpenuhi — berkas ini |
| Source sudah direview pengguna/leader | **Terpenuhi 9 Oktober 2026** — review source leader `APPROVED` atas port di branch `BE-PAT-MIG-001` (bagian 10). Sebelumnya: belum terpenuhi, menahan status di 🟡 |
| Belum ada rekonsiliasi runtime | Terpenuhi — endpoint tidak pernah dipanggil |

**Runtime.** 15 requirement punya bukti tahap runtime (`requirement-traceability.md` bagian 2). Seluruhnya **NOT EXECUTED / PENDING SEPARATE AUTHORIZATION** — menunggu `PAT-GATE-001`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Ditutup 8 Oktober 2026.** Pengerjaan pertama menghasilkan `VIOLATION QBE-CODE-002` positif palsu: pola checker `Generate\w*(Code\|Number)` cocok dengan nama `GenerateQrCodePngBytes` karena potongan "QrCode", padahal method itu membentuk **gambar** PNG, bukan mengalokasikan nomor bisnis. Atas keputusan pemilik, method `private static` itu di-rename menjadi `RenderQrCodePngBytes`. Checker global dan `QBE_EXCEPTIONS.json` **tidak** diubah. Hasil `Strict` sesudah rename ada di bagian 8 |
| Masalah yang diketahui | Teks SQL `RsmmcPilotMigrationSource` belum pernah dijalankan terhadap PostgreSQL. Tipe fisik kolom `migration.rsmmc_*` tidak tercatat di repository, sehingga kueri meng-cast seluruh kolom ke `text` dan membandingkan `lower(batch_id::text)`. Simulasi pertama di staging sesudah `PAT-GATE-001` adalah pembuktian pertamanya; kegagalannya aman karena terjadi sebelum mutasi apa pun |
| Risiko tersisa | (1) `PAT-RSK-005`: izin `Patient : Update` dipakai apa adanya, sesuai `AC-06` yang menyebut izin yang **sudah ada**; setiap pemegang izin itu di staging dapat menjalankan endpoint. (2) `PAT-RSK-008`: ejaan environment server staging belum diverifikasi; bila bukan `Staging` persis, endpoint menolak (gagal tertutup). (3) Kolom `legacy_pid` diurutkan sebagai angka bila keduanya angka, selain itu ordinal; bila tipe fisiknya teks non-angka, urutan bisa berbeda dari `ORDER BY` PostgreSQL, tetapi tetap deterministik. (4) Pemindaian ulang perencana per pasien memakai cast `::text`, sehingga indeks tidak terpakai; untuk paling banyak 100 pasien per panggilan, ini dinilai dapat diterima. (5) Kegagalan *commit* selalu dilaporkan `COMMIT_OUTCOME_UNCERTAIN`: QR baru dan QR lama sama-sama dipertahankan, proses berhenti, tanpa retry otomatis. Operator perlu memeriksa apakah data pasien menunjuk QR baru atau QR lama. (6) Bila penulisan QR gagal di tengah sesudah berkas tujuan dibuat, berkas sebagian tidak cocok dengan digest-nya sehingga sengaja tidak dihapus; kegagalan dilaporkan pada tahap `CREATE_QR` dengan path-nya. (7) Antara pemeriksaan digest dan penghapusan masih ada jeda singkat; selama jeda itu proses lain secara teori dapat mengganti berkas. Untuk endpoint admin khusus Staging dengan `CreateNew` sebagai penjaga utama, ini dinilai dapat diterima |
| Perubahan sampingan | `NONE`. Build dan test hanya menghasilkan `bin/`/`obj/` yang diabaikan Git |
| Interupsi | `NONE` |
| Status Git | HEAD `103b45cc`, belum di-*stage* dan belum di-commit. `git status --short`: ` M .gitignore`, ` M Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs`, ` M Program.cs`, `?? Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcPilotMrnReconciliationDtos.cs`, `?? Areas/HealthServices/PatientManagement/MasterData/Services/`, `?? Tests/QuilvianSystemBackend.PatientManagementTests/`, `?? appsettings.Staging.json` (sudah ada sebelum task, tidak disentuh), `?? docs/module-blueprints/patient-management/` (paket governance yang sudah ada sebelum task, ditambah laporan ini). `git diff --stat` sesudah koreksi owner review: 3 berkas, 117 baris ditambah, 11 dihapus (pengerjaan pertama: 115/9). Tidak ada perubahan di `Storage/`, `Migrations/`, maupun `appsettings*` |
| Langkah berikutnya | 1) Review source oleh leader memakai *source review pack*. 2) Commit/push/PR dengan otorisasi terpisah. 3) `PAT-GATE-001`: deployment staging, simulasi pertama, lalu eksekusi bertahap `limit` 25 |

---

## 8. Pengerjaan ulang — koreksi owner review 8 Oktober 2026

Status tetap 🟡. Pemilik menyetujui arsitektur dan hasil build/test secara prinsip, lalu meminta dua koreksi sebelum review source oleh leader.

### 8.1 Koreksi yang dikerjakan

| No | Permintaan | Yang dikerjakan |
| ---: | --- | --- |
| 1 | Ukuran berkas saja bukan bukti kepemilikan QR | `RsmmcPilotCreatedQrArtifact` kini membawa `CreatedByThisInvocation`, `Length`, dan `Sha256` (digest SHA-256 dari byte PNG yang ditulis, dihitung sebelum penulisan). `TryDeleteCreated` menghapus **hanya bila** keempat syarat terbukti: penanda `true`, berkas masih ada, panjang sama, digest isi sama. Selain itu tidak dihapus, dan hasilnya (`NotCreatedByThisInvocation`, `AlreadyMissing`, `OwnershipUnproven`, `Failed`) dicatat sebagai ketidakpastian pembersihan, terpisah dari galat database aslinya |
| 1 | *Commit* yang hasilnya tidak pasti | Galat apa pun dari `CommitAsync` kini dilaporkan `FAILED` dengan tahap `COMMIT_OUTCOME_UNCERTAIN`. QR baru **tidak** dihapus, QR lama tetap, proses berhenti, dan tidak ada retry otomatis. Pembacaan ulang database yang sebelumnya dipakai untuk "menebak" hasil commit dihapus |
| 1 | Penulisan QR gagal di tengah | Berkas sebagian tidak cocok dengan digest-nya, sehingga sengaja tidak dihapus. Galatnya menyebut path-nya dan dilaporkan pada tahap `CREATE_QR` |
| 2 | Positif palsu `QBE-CODE-002` | Penilaian: `GenerateQrCodePngBytes` **bukan** dibuat task ini (sudah ada sejak commit `0b45a440`), berstatus `private static`, dan hanya dirujuk di `PatientController` (deklarasi, satu pemanggilan di `SavePatientQrCodeFile`, dan factory baru) serta test refleksi. Rename tidak mengubah API publik maupun perilaku. Karena itu method di-rename menjadi `RenderQrCodePngBytes`. Isi method tidak berubah; teks label log `step = "GenerateQrCodePngBytes"` sengaja dipertahankan supaya keluaran log operasional tidak berubah. Checker global dan `QBE_EXCEPTIONS.json` tidak disentuh |

### 8.2 Test baru dan yang diperbarui

| Test | Membuktikan |
| --- | --- |
| `CreateNew_MencatatBuktiKepemilikan_PenandaPanjangDanSha256` | Artefak membawa penanda `true`, panjang, dan SHA-256 yang sama dengan isi berkas |
| `TryDeleteCreated_PanjangSamaIsiBerbeda_TidakDihapus` | Isi diganti dengan panjang persis sama → `OwnershipUnproven`, tidak dihapus |
| `TryDeleteCreated_ArtefakPersisMiliknya_Dihapus_BerkasLainDiFolderTetap` | Artefak persis buatan pemanggilan ini → dihapus; berkas lain di folder tetap |
| `TryDeleteCreated_PanjangBerbeda_TidakDihapus` | Isi diganti dengan panjang berbeda → tidak dihapus |
| `TryDeleteCreated_BukanBuatanPemanggilanIni_TidakDihapusWalauIsinyaCocok` | Penanda `false` → tidak dihapus walau digest cocok |
| `TryDeleteCreated_TidakPernahMenyentuhQrLama` | Artefak yang diarahkan ke path QR lama (panjangnya kebetulan sama) → tidak dihapus |
| `GagalSimpan_QrBaruDigantiIsiSamaPanjang_TidakDihapusDanDilaporkan` | Tingkat service: database gagal sesudah QR baru diganti isi sama panjang → QR tidak dihapus, pesan menyebut `OwnershipUnproven`, QR lama tetap |
| `GagalCommit_HasilTidakPasti_QrBaruDanLamaDipertahankan_ProsesBerhenti` | `CommitAsync` gagal → `COMMIT_OUTCOME_UNCERTAIN`, QR baru dan lama tetap, satu kali commit saja, pasien berikutnya tidak diproses |
| `GagalSimpanSesudahQrDibuat_HanyaQrBaruDibersihkan_QrLamaTetap` (tetap) | Kegagalan database terkonfirmasi dengan artefak persis → QR baru dihapus, QR lama tetap |

### 8.3 Validasi sesudah koreksi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj -p:RunAnalyzers=false` | `Test Run Successful.` `Total tests: 72`, `Passed: 72` — gagal 0, dilewati 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false --no-incremental` | `Build succeeded.` `0 Error(s)`, `242 Warning(s)`, 1 menit 39 detik. Nol warning dari berkas baru; satu-satunya warning pada berkas yang disentuh tetap `PatientController.cs(2611) CS8619` milik `GetActorNameMapAsync` (commit `722a2dd92`) | `PASS`; warning itu `EXISTING / ENVIRONMENT ISSUE` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (cakupan *working tree*: baris tambahan berkas tracked + seluruh berkas `.cs` baru) | `Files evaluated: 16`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS`, exit `0` | `PASS` |

Pemeriksaan `Strict` GitRange milik CI membandingkan SHA base dan head pull request, sehingga baru dapat dijalankan sesudah ada commit — dan commit belum diberi wewenang. Cakupan *working tree* di atas mengevaluasi delta yang sama dengan isi PR nantinya.

AUTOMATED TEST: `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj` — `PASS` (72/72).

---

## 9. Port semantik ke baseline Integration terbaru — 9 Oktober 2026

Status tetap 🟡. Bagian 1 sampai 8 di atas adalah **bukti sejarah** pengerjaan di `QuilvianStaDeploy`
@ `103b45cc` dan tidak diubah. Bagian ini mencatat fakta yang terbukti pada branch baru.

### 9.1 Mengapa port dilakukan

Implementasi yang sudah direview pemilik ada sebagai perubahan **belum di-commit** di worktree
`QuilvianStaDeploy`. Sementara itu `QuilvianIntegrationBackend` sudah maju 52 commit. Supaya pull
request nanti hanya berisi perubahan `BE-PAT-MIG-001` di atas source terbaru, perubahan itu
dipindahkan **secara makna** ke worktree baru yang dibuat langsung dari Integration — bukan dengan
menimpa berkas lama di atas berkas baru.

Aturannya sederhana: untuk perilaku yang tidak berkaitan, source Integration selalu menang. Source
lama hanya menjadi rujukan maksud `BE-PAT-MIG-001`.

### 9.2 Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Worktree tulis | `C:\Users\User\source\repos\NewQuilvianSystemBackendPatMigration` |
| Branch | `BE-PAT-MIG-001` |
| HEAD | `179aea3fe3912832f1230b0b7d19a132dd651839` |
| `origin/QuilvianIntegrationBackend` | `179aea3fe3912832f1230b0b7d19a132dd651839` — sama dengan HEAD |
| Remote | `https://github.com/DevBenari/NewQuilvianSystemBackend.git` |
| Status awal | Hanya `?? docs/module-blueprints/patient-management/` (paket governance yang sengaja disalin) |
| `appsettings.Staging.json` | **Tidak ada** di worktree ini; tidak disalin, tidak dibuka |
| Hash keputusan dan kontrak | `00-interview-decisions.md` `fa0138e5…2885` dan `api-contract.md` `cb41e4d5…ad7e` — sama dengan manifest, sehingga roadmap dan traceability tidak stale |
| Worktree rujukan | `QuilvianStaDeploy` @ `103b45cc`, dibaca **hanya-baca** lewat path absolut dan `git -C … status/diff` |

### 9.3 Selisih baseline `103b45cc` → `179aea3f` pada area yang sensitif bagi task ini

Merge base keduanya `fb844965`; Integration 52 commit di depan, rujukan 44 commit di depan.

| Area | Perubahan di Integration | Akibat bagi port |
| --- | --- | --- |
| `PatientController.cs` | `BuildPatientQrPayload`, `NormalizeMedicalRecordNumberToRawDigits`, dan `FormatMedicalRecordNumber` kini mendelegasikan ke kelas baru `PatientQrPayloadBuilder` (`BE-RWI-187`), perilakunya identik. `using …PatientManagement.MasterData.Services;` sudah ada | Factory QR rekonsiliasi tetap memberikan `BuildPatientQrPayload`, sehingga otomatis memakai pembentuk kanonik baru. `using` tidak ditambahkan dua kali |
| `PatientQrPayloadBuilder.cs` | **Baru** — pembentuk isi QR dan format MRN bersama | Dipakai apa adanya; satu test baru membuktikan kesamaannya (bagian 9.10) |
| `Program.cs` | +188 baris registrasi modul lain; `using …PatientManagement.MasterData.Services;` sudah ada di baris 63 | Hanya tiga registrasi DI yang ditambahkan, di titik yang sama dengan rujukan |
| `ApplicationDbContext.cs` | +77 baris: `DbSet` Rawat Inap/Klinis/Billing dan penomoran versi `InpEpisode` saat `SaveChanges` | Tidak menyentuh `MstPatient` maupun `migration.rsmmc_*`. Tidak ada perubahan model oleh task ini |
| `PatientDtos.cs` | `BirthTime` memakai `FlexibleNullableTimeSpanConverter` | Tidak berkaitan |
| `CreatePatient`, `UpdatePatient`, `GenerateMedicalRecordNumberAsync`, `GeneratePatientCodeAsync`, `SavePatientQrCodeFile`, `GenerateQrCodePngBytes`, `GetFileStoragePaths`, `CombineUrlPath`, `SanitizePathSegment`, `ResolveQrLogoPath`, `GetCurrentUserId` | Tidak berubah selain pergeseran nomor baris | Diff rujukan dapat diterapkan maknanya apa adanya |
| `LoggerService`, `ApiResponse`, atribut `Access*`, `IdentityModel`, `MstPatient`, `AccessPermissionService` | Tidak berubah | Source baru dikompilasi tanpa adaptasi |
| `.gitignore`, `QuilvianSystemBackend.csproj`, `QuilvianSystemBackend.sln`, `tooling/qbe/` | Tidak berubah | Whitelist test tetap diperlukan karena aturan `/Tests/*`; `Tests\**` tetap dikeluarkan dari kompilasi project utama |
| `Tests/QuilvianSystemBackend.PharmacyTests/` | Tiga berkas test Billing/Finance baru | Tidak berkaitan; project test PatientManagement tetap terpisah |

### 9.4 Berkas hasil port

| Berkas | Cara port | Keterangan |
| --- | --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcPilotMrnReconciliationDtos.cs` | Disalin | **Identik byte demi byte** dengan rujukan (`cmp`) |
| `…/Services/RsmmcPilotMrnReconciliationService.cs` | Disalin | Identik |
| `…/Services/RsmmcPilotMigrationSource.cs` | Disalin | Identik — teks SQL tidak berubah |
| `…/Services/RsmmcPilotQrArtifactStore.cs` | Disalin | Identik |
| `…/Services/RsmmcPilotEnvironmentGate.cs` | Disalin | Identik |
| `…/Services/RsmmcPilotApprovedBatchProfile.cs` | Disalin | Identik |
| `…/Controllers/PatientController.cs` | Maksud diterapkan pada berkas Integration | Baris tambah/hapus sama dengan diff rujukan **kecuali** `using …Services;` yang sudah ada di Integration. Isi: konstanta `PatientQrCodeFileName`, aksi `ReconcileRsmmcPilotMrn` sesudah `DeletePatient`, factory `CreateRsmmcPilotQrArtifactStore`, `GetPublicRequestPath()` diekstrak dari `GetFileStoragePaths()`, rename `GenerateQrCodePngBytes` → `RenderQrCodePngBytes` (label log `step` tetap) |
| `Program.cs` | Maksud diterapkan | Tiga registrasi DI saja; `using` sudah ada |
| `.gitignore` | Maksud diterapkan | Blok whitelist yang sama persis dengan rujukan; `bin/` dan `obj/` tetap diabaikan |
| `Tests/QuilvianSystemBackend.PatientManagementTests/` (9 berkas) | Disalin | Delapan berkas identik. `PenyimpanQrDanSumberMigrasiTests.cs` mendapat **satu test baru** `PayloadProduksi_SamaDenganPembentukQrKanonikIntegrasi` untuk delta `PatientQrPayloadBuilder` |

### 9.5 Selisih kutipan baris kontrak `1.1.0`

Manifest mewajibkan kutipan baris pada `api-contract.md` diperiksa ulang bila snapshot bergeser.
Kontrak **tidak diubah**; selisihnya dicatat di sini. Nomor di kolom tengah adalah HEAD
`179aea3f` sebelum port.

| Kutipan kontrak (`103b45cc`) | Integration `179aea3f` | Catatan |
| --- | --- | --- |
| `[Authorize]` 41, `[Route]` 42, `[AccessController]` 43–51, `[Tags]` 52 | 42, 43, 44–52, 53 | Isi sama |
| Izin ubah 771–778 dan 912–919 | 772–779 dan 913–920 | Isi sama |
| `BuildPatientQrPayload` 1386–1401 | 1389–1390 → `PatientQrPayloadBuilder.Build` | Didelegasikan, perilaku sama |
| Format MRN 1606–1636 | 1598–1599 → `PatientQrPayloadBuilder.FormatMedicalRecordNumber` | Didelegasikan, perilaku sama |
| Folder 59, 1076–1077; nama berkas 1103; path publik 60, 1153–1157 | 60, 1077–1078; 1104; 61, 1154–1158 | Isi sama |
| `File.WriteAllBytes` 1144 | 1145 | Masih menimpa diam-diam; tetap tidak dipakai rekonsiliasi |
| `SavePatientQrCodeFile` 1046–1172 | mulai 1047 | Isi sama |
| `GenerateMedicalRecordNumberAsync` 1403, `GeneratePatientCodeAsync` 2468 | 1392, 2427 | Isi sama; tetap tidak dipanggil rekonsiliasi |
| `AccessPermissionService.cs` 38, `OperatingRoomRuleRelaxation.cs` 41, workflow baris 105 `Staging`, `LabOrderNumberService.cs` 152, `KioskPatientLookupService.cs` 179 | Sama | — |

### 9.6 Alur pasien normal — tidak ada perubahan perilaku

| Alur | Dampak | Bukti |
| --- | --- | --- |
| `CreatePatient` / `CreatePatientForAdmin` | Tidak berubah | Tidak ada hunk diff di baris 464–766. QR tetap dibuat lewat `SavePatientQrCodeFile` |
| `UpdatePatient` / `UpdatePatientStatus` | Tidak berubah | Tidak ada hunk diff di baris 767–950 |
| `GenerateMedicalRecordNumberAsync` | Tidak berubah | Badan method tidak tersentuh; tetap dipanggil hanya oleh `CreatePatient` |
| `GeneratePatientCodeAsync` | Tidak berubah | Badan method tidak tersentuh |
| Pembuatan QR normal | Tidak berubah | `"qrcode.png"` diganti konstanta bernilai sama; `RenderQrCodePngBytes` adalah rename `private static` tanpa perubahan isi; `GetPublicRequestPath()` menghitung nilai yang persis sama dengan kode lama di `GetFileStoragePaths()` (bawaan `/uploads`, awalan `/`, tanpa `/` di akhir). Pemakai `GetFileStoragePaths()` lain — foto pasien dan logo QR — menerima nilai yang sama |
| Alokasi `PatientCode` dan MRN normal | Tidak berubah | Service rekonsiliasi tidak merujuk kedua pembangkit (keduanya `private` di controller) |

### 9.7 Kontrak SQL — `IDENTICAL`

`RsmmcPilotMigrationSource.cs` identik byte demi byte dengan rujukan, sehingga keempat teks SQL,
cast, JOIN, saringan, pengikatan parameter, dan proyeksi hasilnya **tidak berubah**.

| Kueri | Tujuan | Parameter | Cast | JOIN | Proyeksi |
| --- | --- | --- | --- | --- | --- |
| `MappedFinalizedRowsSql` | Set Pilot: perencana `FINALIZED` batch ini yang punya `quilvian_patient_id` di crosswalk, `ORDER BY p.legacy_pid` | `{0}` = batch (`Guid` format `D`) | `legacy_pid`, `plan_status`, `final_medical_record_number`, `quilvian_patient_id` ke `text`; `lower(p.batch_id::text)` | `JOIN crosswalk ON c.legacy_pid = p.legacy_pid` | `LegacyPid`, `PlanStatus`, `FinalMedicalRecordNumber`, `QuilvianPatientId` |
| `RevalidationRowsSql` | Baca ulang satu `legacy_pid` di dalam transaksi pasien, tanpa saringan status | `{0}` batch, `{1}` `legacy_pid` | sama, ditambah `p.legacy_pid::text = {1}` | `LEFT JOIN crosswalk` | sama |
| `CountFinalizedPlanRowsSql` | Gerbang 705.223 baris `FINALIZED` | `{0}` batch | `count(*)::int` | — | `Value` |
| `CountBackupRowsSql` | Gerbang 715 baris cadangan | `{0}` batch | `count(*)::int` | — | `Value` |

Seluruhnya hanya `SELECT`, dijalankan lewat `Database.SqlQuery<T>` dengan `FormattableString`,
sehingga nilai masukan menjadi parameter, bukan teks. Kolom `normalized_mrn` tidak dibaca.

Karena SQL `IDENTICAL`, bukti kompatibilitas PostgreSQL hanya-baca yang **dilaporkan pemilik pada
instruksi task** — `QuilvianNewSta`, `pg_is_in_recovery()` `false`, kueri Pilot 715, revalidasi
`legacy_pid` 797015 tepat satu baris dengan MRN akhir `00-79-70-15`, perencana `FINALIZED`
705.223, cadangan 715, diakhiri `ROLLBACK` — **tetap berlaku**. Agent tidak mengakses database
mana pun pada sesi ini dan tidak memverifikasi ulang angka tersebut.

### 9.8 Keselamatan QR — tidak berubah dari rujukan

| Hal | Mekanisme |
| --- | --- |
| Tanpa penimpaan | Pemeriksaan awal `File.Exists`, lalu penulisan akhir `FileMode.CreateNew` + `FileShare.None`; bila berkas sudah ada → `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS`, `MstPatient` tidak diubah, berkas yang ada tidak dihapus |
| Race | Berkas yang muncul di antara pemeriksaan dan penulisan ditolak sistem operasi oleh `CreateNew` → `CONFLICT` (`IsRaceConflict = true`) |
| Bukti kepemilikan | `CreatedByThisInvocation = true` + panjang sama + SHA-256 isi sama dengan byte yang ditulis |
| Panjang sama, isi berbeda | `OwnershipUnproven` → **tidak dihapus**, dicatat sebagai ketidakpastian pembersihan |
| Kegagalan database terkonfirmasi (`SAVE_CHANGES`) | Hanya artefak yang terbukti milik pemanggilan ini yang dihapus; folder hanya dihapus bila dibuat pemanggilan yang sama dan sudah kosong; tidak pernah rekursif |
| *Commit* tidak pasti | `FAILED` / `COMMIT_OUTCOME_UNCERTAIN`: QR baru **dan** QR lama dipertahankan, tanpa retry otomatis, proses berhenti |
| QR lama | Tidak pernah dikirim ke jalur pembersihan; tidak pernah dihapus, dipindah, atau ditimpa |

### 9.9 Keselamatan database

Tidak ada EF migration yang dibuat atau dijalankan, tidak ada perintah `dotnet ef`, tidak ada
perubahan skema, entity, `DbSet`, maupun konfigurasi EF (diff tidak menyentuh `Migrations/`,
`Repositories/`, atau `Models/`). Tidak ada koneksi ke database nyata mana pun. Tidak ada
`MstPatient`, crosswalk, perencana, maupun tabel cadangan yang diubah. Test memakai SQLite di
memori dan tiruan tabel `migration.rsmmc_*`.

### 9.10 Validasi pada branch `BE-PAT-MIG-001`

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj -p:RunAnalyzers=false` | `Passed!  - Failed: 0, Passed: 73, Skipped: 0, Total: 73, Duration: 1 m 35 s` | `PASS` |
| `dotnet build -p:RunAnalyzers=false --no-incremental` | `Build succeeded.` `256 Warning(s)`, `0 Error(s)`, `00:02:09.13`. **Nol** warning dari berkas baru. Satu-satunya warning pada berkas yang disentuh adalah `PatientController.cs(2569,20) CS8619` di `GetActorNameMapAsync` — baris `return await _dbContext.Users` dari commit `722a2dd92` (10 Juni 2026), di luar diff | `PASS`; warning itu `EXISTING / ENVIRONMENT ISSUE` |
| `powershell.exe -NoProfile -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (scope `WorkingTree`) | `Files evaluated: 16`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none`, `Final result: PASS`, exit `0`. 8 berkas test dikeluarkan dari `QBE-ENT-001`/`QBE-CFG-001`/`QBE-MOD-002` sesuai aturan checker | `PASS` |

Selisih dari rujukan `72/72`: **satu test tambahan**, `PayloadProduksi_SamaDenganPembentukQrKanonikIntegrasi`,
yang membuktikan isi QR rekonsiliasi sama dengan `PatientQrPayloadBuilder.Build` — pembentuk yang
baru diperkenalkan Integration — untuk MRN berformat maupun angka mentah (`00797015` →
`00-79-70-15`). Ke-72 test rujukan lulus tanpa perubahan. Jumlah warning build (`256`) tidak dapat
dibandingkan langsung dengan `242` pada rujukan, karena baseline source-nya berbeda.

AUTOMATED TEST: `dotnet test Tests/QuilvianSystemBackend.PatientManagementTests/QuilvianSystemBackend.PatientManagementTests.csproj` — `PASS` (73/73), dibuat atas permintaan eksplisit pemilik (`PAT-DEC-014`, `AC-27`, `PAT-OQ-001`).

**`TEST_POLICY` guard dibaca ulang.** `rules/backend/TEST_POLICY.md` bagian 3 tetap terpenuhi:
`AC-27` disetujui pemilik pada kartu task (`PAT-DEC-014`), dan instruksi task aktif meminta port
seluruh test keselamatan secara eksplisit. Project test tidak didaftarkan di
`QuilvianSystemBackend.sln` dan tidak ada workflow CI baru. Selisih governance `PAT-RSK-007` tetap
ada: `TEST_POLICY` menyebut `Tests/` sudah dihapus, padahal Integration masih men-*track* tiga
project test.

Uji manual: `NOT APPLICABLE` — pemanggilan endpoint tidak diberi wewenang.

**Tidak dijalankan:** pemanggilan endpoint; koneksi ke database mana pun; `dotnet ef`; deployment;
QBE `Strict` GitRange (butuh commit, dan commit belum diberi wewenang).

### 9.11 Status acceptance criteria dan gerbang

| Hal | Status |
| --- | --- |
| Implementasi `AC-01`–`AC-28` | Terpetakan ke source yang ada di branch ini — pemetaan bagian 6 tetap berlaku karena source baru identik dan perubahan controller sama maknanya |
| Verifikasi `AC-27`, `AC-28` | Terpenuhi ulang di Integration: test `73/73`, build `0` error |
| Runtime (15 requirement bertahap runtime) | **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION** |
| `PAT-GATE-001` | **Terbuka** |
| DoD "source sudah direview pengguna/leader" | Belum terpenuhi saat bagian ini ditulis → **terpenuhi 9 Oktober 2026**, lihat bagian 10 |
| Migrasi RSMMC secara keseluruhan | **Belum selesai**; 704.508 pasien sisa di luar cakupan (`AC-23`) |

### 9.12 Status Git dan keselamatan worktree rujukan

`git status --short` sesudah port dan validasi:

```text
 M .gitignore
 M Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs
 M Program.cs
?? Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcPilotMrnReconciliationDtos.cs
?? Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotApprovedBatchProfile.cs
?? Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotEnvironmentGate.cs
?? Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMigrationSource.cs
?? Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotMrnReconciliationService.cs
?? Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilotQrArtifactStore.cs
?? Tests/QuilvianSystemBackend.PatientManagementTests/
?? docs/module-blueprints/patient-management/
```

`git diff --stat`: 3 berkas, 115 baris ditambah, 11 dihapus. Rujukan 117/11; selisih dua baris
adalah dua `using …PatientManagement.MasterData.Services;` yang sudah ada di Integration.

Tidak ada perubahan di `Migrations/`, `Repositories/`, `Storage/`, maupun `appsettings*`, dan tidak
ada berkas `.png` atau folder `patient-qrcodes/` hasil runtime. Hasil build dan test hanya berada
di `bin/`/`obj/` yang diabaikan Git. Tidak ada stage, commit, push, PR, maupun deploy.

Worktree rujukan `NewQuilvianSystemBackendDeployStaging` diperiksa ulang secara hanya-baca di akhir
sesi: branch `QuilvianStaDeploy`, HEAD `103b45cc`, status dan `git diff --stat` (3 berkas, 117
tambah, 11 hapus) sama persis dengan awal sesi, dan tidak ada berkas task di sana yang berubah
pada 9 Oktober 2026. `appsettings.Staging.json` di sana tidak dibuka.

| Hal | Isi |
| --- | --- |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | 1) Review source oleh leader di branch `BE-PAT-MIG-001`. 2) Commit/push/PR dengan otorisasi terpisah, lalu QBE `Strict` GitRange di CI. 3) `PAT-GATE-001`: deployment staging, simulasi pertama, lalu eksekusi bertahap `limit` 25 |

---

## 10. Review source leader — `APPROVED`, 9 Oktober 2026

### 10.1 Keputusan

| Hal | Isi |
| --- | --- |
| Hasil review source | **`APPROVED`** |
| Tanggal | 9 Oktober 2026 |
| Yang direview | Port semantik `BE-PAT-MIG-001` di branch `BE-PAT-MIG-001` di atas `QuilvianIntegrationBackend` @ `179aea3f`, berdasarkan laporan bagian 9 |
| Oleh | Leader work item, lewat keputusan tertulis pada sesi `TASK MODE: BACKEND` 9 Oktober 2026 |
| Wewenang yang diberikan bersamanya | Mencatat persetujuan ini; inspeksi Git sebelum commit; stage **hanya** berkas `BE-PAT-MIG-001`; **satu** commit lokal; QBE `Strict` GitRange sesudah commit |
| Yang **tidak** diberikan | Push, pull request, merge, rebase, pull, ganti branch, deployment, promosi Integration → DEV atau DEV → STAGING, pemanggilan endpoint, koneksi atau mutasi database, pembuatan QR sungguhan, EF migration |

Keputusan bisnis, kontrak API `1.1.0`, acceptance criteria, kebijakan MRN kanonik, kontrak SQL,
dan cakupan task **tidak berubah** karena review ini.

### 10.2 Status sesudah review

| Bagian | Status |
| --- | --- |
| **SOURCE IMPLEMENTATION** | **`APPROVED`** — 28 dari 28 acceptance criteria terpetakan ke source dan terverifikasi; seluruh butir Definition of Done fase implementasi terpenuhi |
| **RUNTIME** | **`PENDING`** — rekonsiliasi **NOT EXECUTED — PENDING SEPARATE AUTHORIZATION** |
| `PAT-GATE-001` | **OPEN.** Prasyaratnya — review source, build, dan test — kini terpenuhi, tetapi otorisasi deployment staging, pemanggilan endpoint, dan eksekusi 715 pasien **belum** diberikan |
| Migrasi RSMMC secara keseluruhan | **Belum selesai.** 704.508 pasien sisa di luar cakupan (`AC-23`) dan menunggu rekonsiliasi Pilot lebih dulu (`PAT-DEC-010`) |

**Mengapa task ditandai `✅` sementara runtime belum.** Menurut aturan status roadmap canonical,
`✅` sah bila setiap acceptance criteria terpetakan ke source yang ada, validasi yang diminta task
sudah dijalankan, dan laporan tracked-nya ada. Ketiganya terpenuhi, dan butir DoD terakhir yang
menahan 🟡 — review source leader — kini terpenuhi. Tahap runtime **bukan** bagian `BE-PAT-MIG-001`:
`PAT-DEC-012` membatasi task ini pada implementasi source, dan `requirement-traceability.md`
bagian 1 menetapkan tahap runtime sebagai wewenang `PAT-GATE-001`. Karena itu tanda `✅` berarti
"implementasi source selesai dan disetujui", **bukan** "rekonsiliasi sudah dijalankan".

Contoh: sesudah tanda ini, 715 pasien Pilot di staging **masih** memakai MRN buatan Quilvian.
Nomor mereka baru berpindah ke nomor kanonik bila `PAT-GATE-001` dibuka dan endpoint dijalankan.

### 10.3 Commit lokal

Berkas ini ikut di dalam commit lokal `BE-PAT-MIG-001`, sehingga SHA commit itu dan hasil QBE
`Strict` GitRange — yang baru ada **sesudah** commit — tidak dapat ditulis di sini tanpa membuat
perubahan baru sesudah commit. Keduanya dilaporkan pada handoff sesi 9 Oktober 2026 dan dicatat
ke berkas ini pada sesi berikutnya yang diberi wewenang menulis.
