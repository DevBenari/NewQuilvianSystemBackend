# Laporan Perubahan Backend — `BE-PAT-MIG-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PAT-MIG-002` |
| Judul | RSMMC Resumable Bulk Patient Migration Worker |
| Slice | S2 — Worker migrasi massal pasien RSMMC, Canary-A |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md), kartu `BE-PAT-MIG-002` |
| Trace | `PAT-DEC-017`–`PAT-DEC-077`; `PAT-FACT-079`–`083`, `092`–`097`; `PAT-MIG2-AC-01`–`PAT-MIG2-AC-51` (`AC-44`/`45` dipindah ke `BE-PAT-MIG-003`); kontrak Bagian B dan B15 |
| Contract version | `1.3.0` — `approved` 19 Oktober 2026 |
| Dependency | `BE-PAT-MIG-001` ✅, `BE-PAT-MIG-003` ✅, `BE-PAT-MIG-003A` ✅, `PAT-GATE-005` generasi 2 `PASS` (`PAT-FACT-093`), baseline tetap 741 (`PAT-FACT-095`) |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas diubah > 8 termasuk test (2), logika bisnis kompleks — transaksi per pasien, kunci, idempotensi (2), menambah kontrak API (2), penulisan persistence lewat SQL berparameter (1), berkaitan dengan keamanan (1), satu repository (0), tanpa UI (0) |
| Task mode | `BACKEND` — instruksi pemilik "TASK MODE: BACKEND — Implementasikan `BE-PAT-MIG-002`", lewat `quilvian-engineering-skills:build-module-backend`. Lingkup: implementasi source + verifikasi source saja |
| Target tulis | `NewQuilvianSystemBackend`: source PatientManagement `MasterData`, `Program.cs` (DI saja), project test `Tests/QuilvianSystemBackend.PatientManagementTests/`, `docs/module-blueprints/patient-management/**` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | **Kandidat akhir: commit lokal `46edc6afd946666178d9681f5db103310daa2b22`** di branch `BE-PAT-MIG-002`, induk langsung Integration `52d7e1175c439d4c36a4e53608f630c5823604d8` (bagian 11.1). Riwayat: dibangun di *working tree* di atas parent yang disetujui `c83bcdaa1e66a326ddb81dc9a431b70c76e40ed3` (22 Oktober 2026); di-*commit* dan di-*rebase* ke `35951dbb` sebagai `f0ee1c98` (bagian 10); lalu di-*rebase* ke `52d7e117` sebagai `46edc6af` — source dan test MIG-002 identik dengan `f0ee1c98`. **Belum** push, PR, merge, maupun deploy |
| Tanggal | 22 Oktober 2026 — implementasi dan fix pass review pemilik; 24 Oktober 2026 — finalisasi dokumentasi dengan bukti post-rebase pertama (`f0ee1c98`); 25 Oktober 2026 — finalisasi dokumentasi dengan bukti post-rebase kedua (`46edc6af`) yang dilaporkan pemilik |
| Status | 🟡 **IMPLEMENTED — VALIDATED LOCALLY** — bukan *deployed* dan bukan *production-ready*. Bukti post-rebase kedua pada `46edc6af` di atas `52d7e117` (bagian 11): build Release backend `PASS`; build Release project test `PASS`; PatientManagement non-provider `505`/`505`; provider-real MIG-002 `6`/`6`; provider-real penuh `29`/`29` (gagal `0`, dilewati `0`, ExitCode `0`, 1 jam 34 menit) pada PostgreSQL 15.19 sekali pakai; QBE `Strict` `GitRange` `PASS` (0 temuan). Satu percobaan provider-real sebelumnya gagal 25 kasus dengan `Connection refused` sesudah koneksi bukti terputus; run ulang `29`/`29` `PASS` (bagian 11.3). 47 dari 49 AC aktif terbukti; `AC-35`/`36` butuh eksekusi aktual di Staging — **`NOT RUN`**. **Belum:** review source leader, PR, merge, deploy; `PAT-GATE-002` `NOT RUN`; Canary-A 25 pasien `NOT RUN`; migrasi aktual 704.506 pasien `NOT RUN`; **tidak ada** otorisasi migrasi massal. *Keadaan 24 Oktober 2026 — jejak:* 🟡 IMPLEMENTED — VALIDATED LOCALLY. Bukti post-rebase pada `f0ee1c98` di atas `35951dbb` (bagian 10): build Release `0` error; build project test `PASS`; MIG-002 non-provider `202`/`202`; PatientManagement non-provider `505`/`505`; membership `97`/`97`; QBE `Strict` `GitRange` `PASS` (43 berkas, 0 temuan); **PostgreSQL 15.19 provider-real MIG-002 `6`/`6` dan penuh `29`/`29` `PASS`**. 47 dari 49 AC aktif terbukti; `AC-35`/`36` milik runtime. **Belum:** review source leader; push, PR, merge, deploy; `PAT-GATE-002` dan Canary-A Staging `NOT RUN`; belum ada migrasi pasien aktual lewat worker, dan **tidak ada** otorisasi migrasi pasien saat ini. *Keadaan 22 Oktober 2026 — jejak:* 🟡 source selesai di *working tree* di atas `c83bcdaa`; provider-real `PENDING OWNER-RUN` |

---

## 1. Masalah yang diselesaikan

704.506 pasien RSMMC kanonik sudah diaudit pada `migration.rsmmc_patient_target_preview`, lengkap dengan
`PatientCode` dan MRN akhirnya. Endpoint pendaftaran pasien biasa tidak bisa dipakai untuk memindahkan
mereka, karena endpoint itu membangkitkan kode dan MRN sendiri (`PAT-SRC-006`).

Task ini menambahkan **worker migrasi massal**: endpoint admin khusus Staging yang membuat pasien persis
dari preview — kode preview, MRN akhir perencana, dan QR baru di folder terpisah `rsmmc-bulk` — satu
pasien satu transaksi, aman diulang, dan dapat dilanjutkan. Build pertama hanya mengenal manifest eksekusi
`RSMMC-CANARY-A` v1: 25 pasien dalam urutan yang disetujui.

*Contoh.* Admin memanggil endpoint dengan `limit` 5. Lima pasien pertama manifest — `legacy_pid`
-667992, -667991, -667986, 797423, -667995 — dibuat beserta crosswalk dan QR-nya. Panggilan kedua melewati
kelimanya sebagai `ALREADY_MIGRATED` (tidak menghabiskan `limit`), lalu membuat lima berikutnya. Tidak ada
pasien ganda, walau panggilan diulang atau terputus di tengah.

---

## 2. Proses bisnis

**Tujuan.** Pasien RSMMC kanonik yang belum ada di Quilvian dibuat persis sesuai preview yang sudah
diaudit, tanpa nomor baru, tanpa pasien ganda, dan tanpa menyentuh Pilot 715 maupun QR historisnya.

**Pelaku.** Admin migrasi yang sudah login dan punya izin `Patient : Update`. Yang mengizinkan
simulasi dan eksekusi di Staging adalah pemilik/leader lewat `PAT-GATE-002` — **di luar** task ini.

**Pemicu.** `POST …/patients/admin/migration/rsmmc-bulk-patients/migrate`.

**Langkah berurutan** (`RsmmcBulkPatientMigrationService.MigrateAsync`):

1. Validasi isian tanpa membaca apa pun: `batchId` wajib, `selectionMode` harus `CANARY_A` persis,
   `limit` 1–25 (bawaan 25), `dryRun` bawaan `true`. Gagal → `400` dengan kode `RSMMC_BULK_*`.
2. Gerbang environment `RsmmcPilotEnvironmentGate` apa adanya: hanya `Staging` persis, ordinal. Gagal →
   `403` **sebelum** satu tabel pun dibaca. Eksekusi juga menolak aktor yang tidak dikenali (`400`).
3. 17 gerbang tingkat panggilan B4.2 dihitung ulang setiap panggilan, dimulai dari
   `PREVIEW_SCHEMA_SIGNATURE`. Pada eksekusi, satu gerbang gagal → `409` tanpa satu pasien pun diproses.
4. Snapshot baca-saja seluruh pasien manifest dan klasifikasi B3.2.
5. **Simulasi** (`dryRun=true`): seluruh 25 baris dikembalikan dalam urutan manifest, beserta bukti QR
   historis (ada/panjang/SHA-256) dan keberadaan path rencana `rsmmc-bulk`. Tidak ada transaksi, kunci,
   tulisan database, folder, atau berkas.
6. **Eksekusi** (`dryRun=false`): pasien manifest diproses berurutan. Per pasien — urutan B4.3:
   1. konversi `legacy_pid` → Int32 dengan pemeriksaan rentang (gagal → `FAILED`/`LOCK_KEY_OUT_OF_RANGE`);
   2. buka transaksi;
   3. `pg_try_advisory_xact_lock(1173286842, legacy_pid)` tanpa menunggu (dipegang → `CONFLICT`/`CONCURRENT_RUN`);
   4. baca ulang preview, perencana, crosswalk, dan kepemilikan kode/MRN/identitas/telepon **sesudah** kunci;
   5. klasifikasi ulang — selain `READY`, transaksi ditutup tanpa perubahan;
   6. `INSERT` `MstPatient` dari preview (`IsMember = false` terkirim eksplisit);
   7. `INSERT` crosswalk `TARGET_CREATED` / `MIGRATED_BULK_WORKER`;
   8. baca ulang pasien `AsNoTracking` dan crosswalk di transaksi yang sama, bandingkan dengan preview — selisih → rollback, `FAILED`/`VERIFY_PERSISTED`;
   9. buat QR di `patient-qrcodes/rsmmc-bulk/<MRN>/qrcode.png` dengan `FileMode.CreateNew`;
   10. *commit*, catat log.
7. Berhenti pada `CONFLICT` atau `FAILED` pertama; sisanya `NOT_PROCESSED`. `ALREADY_MIGRATED` dan
   `MIGRATED_QR_INCONSISTENT` tidak menghabiskan `limit` dan tidak menghentikan proses. Tidak ada pasien
   pengganti.

**Jalur tidak normal — kasus A sampai J.**

| Kasus | Keadaan | Hasil worker |
| --- | --- | --- |
| A | Crosswalk belum ada | `MIGRATED` — pasien, crosswalk, QR baru |
| B | Crosswalk tanpa target | `CONFLICT` / `CROSSWALK_EXISTS_WITHOUT_TARGET`; baris tidak disentuh; berhenti |
| D | Target ada dan cocok | `ALREADY_MIGRATED`; lanjut |
| — | Target cocok, `QrCodePath` atau berkas QR tidak sesuai | `MIGRATED_QR_INCONSISTENT`; dilaporkan, tidak diperbaiki; lanjut |
| E | Target tidak ada | `CONFLICT` / `ORPHANED_CROSSWALK`; pasien tidak dibuat ulang |
| F | Kode/MRN target berbeda; satu target untuk dua `legacy_pid` | `TARGET_MAPPING_MISMATCH` / `DUPLICATE_TARGET_MAPPING` |
| G | Berkas tak dikenal di path `rsmmc-bulk` | `CONFLICT` / `QR_ARTIFACT_ALREADY_EXISTS`; berkas tidak ditimpa/diadopsi; transaksi dibatalkan |
| H | Pembentukan/penulisan QR gagal | `FAILED` / `CREATE_QR`; tidak pernah *commit*; berkas sebagian tidak dihapus |
| I | Gagal sesudah QR dibuat, sebelum *commit* dicoba | Hapus QR hanya bila kepemilikan terbukti (penanda + panjang + SHA-256). Pada urutan B4.3 langkah sesudah QR hanya *commit*, sehingga jalur ini praktis menjadi J |
| J | `CommitAsync` melempar galat | `FAILED` / `COMMIT_OUTCOME_UNCERTAIN`; QR dipertahankan; tanpa *retry*; panggilan berikutnya → `ALREADY_MIGRATED` (bila *commit* sebenarnya berhasil) atau kasus G (bila gagal) — tidak pernah pasien kedua |

**Hasil akhir.** Untuk setiap pasien yang berhasil: satu `MstPatient` baru, satu baris crosswalk baru,
satu QR baru. Preview, perencana, cadangan, crosswalk historis, Pilot 715, dan QR historis tidak berubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/backend/` (`TASK_RULES`, `TEST_POLICY`, `REVIEW_RULES`, `DATABASE_RULES`,
`TASK_CLASSIFICATION`, `API_RULES`, `REPORT_TEMPLATE`); `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`
dan registry; `contracts/api-contract.md` `1.3.0` Bagian B dan B15; `00-interview-decisions.md`
(`PAT-DEC`, `PAT-FACT-079`–`083`); roadmap dan traceability; `RsmmcPilot*` (dibaca, tidak diubah);
`PatientIdentifierAllocationService.CheckAsync`; `RsmmcReservationGuardDtos.cs`; `PatientController.cs`
(route, izin, factory QR, `GetFileStoragePaths`); `Program.cs`; `MstPatient.cs` dan
`MstPatientConfiguration.cs` (dibaca: `IsMember` sentinel, `BirthDate` `date`, indeks unik); enum pasien;
`LoggerService.cs`; `ApiResponse.cs`; `ApplicationDbContextFactory.cs`; infrastruktur test
(`TestDatabase`, `RekonsiliasiHarness`, `ReservasiHarness`, `PostgresSekaliPakai`) dan test yang ada.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/RsmmcBulkPatientMigrationDtos.cs` | **Baru** — request, response ringkasan, gerbang, baris pasien; konstanta mode, status, jenis konflik, tahap gagal, keadaan crosswalk. `legacyPid` bertipe teks |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkApprovedBatchProfile.cs` | **Baru** — profil batch kanonik (704.506 / 705.223 / tertunda / baseline **741** / Pilot 715-710-5 / cadangan 715), namespace kunci `0x45EEEFBA`, status yang disetujui, konversi kunci Int32 |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkCanaryManifest.cs` | **Baru** — `RSMMC-CANARY-A` v1: 25 `legacy_pid` dalam urutan yang disetujui, sebaran 7/6/6/6, SHA-256 berurutan `c43207eb…79ad` dihitung ulang; sidik jari himpunan lama hanya jejak |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkMigrationStore.cs` | **Baru** — antarmuka dan implementasi PostgreSQL: 18 SQL berparameter **bertipe** (`uuid`, `bigint`, `integer`, `text`, larik), tanpa `::text`; kunci dua bagian `pg_try_advisory_xact_lock`; **satu-satunya** tulis: `INSERT` crosswalk. Kunci dan insert wajib di transaksi pasien |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkPatientMapper.cs` | **Baru** — validasi nilai preview (B10, `PREVIEW_VALUE_INVALID`), pemetaan preview → `MstPatient`, pembanding baca ulang `VERIFY_PERSISTED` (termasuk `IsMember = false`, `IsActive = true`, lima kolom baru `NULL`) |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcQrDestinationProbe.cs` | **Baru** — pemeriksa baca-saja path QR rencana dan historis (ada, panjang, SHA-256) tanpa membuat folder |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcBulkPatientMigrationService.cs` | **Baru** — orkestrasi: validasi, gerbang environment, 17 gerbang B4.2, klasifikasi B3.2, simulasi, eksekusi per pasien B4.3, kasus A–J, pembatalan, log B11 tanpa data pribadi |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | **Diubah terbatas** — aksi `MigrateRsmmcBulkPatients`; factory `CreateRsmmcBulkQrArtifactStore` (layout `patient-qrcodes/rsmmc-bulk`) dan `CreateRsmmcQrDestinationProbe`; `GetFileStoragePaths` dipecah menjadi `ResolveFileStorageRootPath` (tanpa membuat folder) + `Directory.CreateDirectory` — perilaku lama identik. Tanpa logika bisnis dan tanpa `DbContext` |
| `Program.cs` | **Diubah, DI saja** — profil (singleton), manifest (singleton), store dan service (scoped) |
| `Tests/QuilvianSystemBackend.PatientManagementTests/MigrasiMassal/Infrastruktur/TiruanStoreMigrasiMassal.cs` | **Baru** — store tiruan di atas skema SQLite `migration` yang di-ATTACH ke koneksi uji, memakai transaksi konteks yang sama (insert crosswalk ikut *rollback*); skema fisik `PAT-FACT-079`–`083` ditulis terpisah dari service |
| `Tests/…/MigrasiMassal/Infrastruktur/MigrasiMassalHarness.cs` | **Baru** — data baku (25 pasien manifest 7/6/6/6, 3 preview di luar manifest, Pilot + crosswalk + cadangan, 2 tertunda, crosswalk historis tanpa target, QR historis), profil uji, service produksi |
| `Tests/…/MigrasiMassal/Infrastruktur/AlatUjiMigrasiMassal.cs` | **Baru** — penangkap log `LoggerService`, penghitung perintah EF dan transaksi, interceptor kegagalan sesudah *commit* dan pengubah nilai tersimpan |
| `Tests/…/MigrasiMassal/*.cs` | **Baru** — 9 kelas uji: `ManifestDanProfilTests`, `ValidasiPermintaanDanIzinTests`, `GerbangPanggilanTests`, `SimulasiMigrasiMassalTests`, `EksekusiMigrasiMassalTests`, `KueriStoreMigrasiMassalTests`, `LogTanpaDataPribadiTests`, `QrNamespaceBulkTests`, dan provider-real `MigrasiMassalPostgresSungguhanTests` (`[Trait("Kategori", "ProviderReal")]`) |
| `Tests/…/ReservasiNomor/JalurPendaftaranReservasiTests.cs` | **Diubah** — inventaris tertutup rute POST non-pendaftaran ditambah satu anggota yang disetujui kontrak B2 (`MigrateRsmmcBulkPatients`). Assertion tetap persis; tidak dilemahkan (bagian 7, D-02) |
| Laporan ini | **Baru** |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru Bagian B: `POST /admin/migration/rsmmc-bulk-patients/migrate`. Endpoint lain tidak berubah. Respons publik hanya memuat field yang dikunci B2.2 — dijaga `Respons_FieldPersisKontrakB22_TanpaTambahan` (bagian 7, D-05; koreksi review pemilik bagian 9) |
| Database | Tidak ada perubahan entity, konfigurasi, `ApplicationDbContext`, maupun migration; `has-pending-model-changes` tanpa perubahan. Tabel `migration.rsmmc_*` tidak dipetakan ke EF. Saat runtime kelak: hanya `INSERT` `MstPatient` dan `INSERT` crosswalk. **Tidak ada** eksekusi database apa pun di sesi ini |
| Keamanan/Auth | `[Authorize]` controller, `[AccessAction("Update", "Migrate RSMMC Bulk Patients", AccessType = Update)]` + `[AccessPermission("Patient", "Update")]`. Tanpa `IsInRole`/`[AllowAnonymous]`. Hanya `Staging` persis; tidak ada konfigurasi yang melonggarkannya. Log tanpa nama, NIK, telepon, alamat, maupun tanggal lahir |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

Base URL: `api/v1/health-services/patient-management/master-data/patients`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/admin/migration/rsmmc-bulk-patients/migrate` | Mensimulasikan atau menjalankan pembuatan pasien RSMMC dari preview untuk manifest `CANARY_A`, paling banyak `limit` pasien per panggilan | `Patient : Update` (khusus `Staging` persis) | `RsmmcBulkPatientMigrationRequest` | `ApiResponse<RsmmcBulkPatientMigrationResponse>` |

Contoh request simulasi (bawaan):

```json
{ "batchId": "45eeefba-dd39-6831-1bd2-d986851f7c1f", "selectionMode": "CANARY_A" }
```

Contoh potongan respons simulasi (nilai samaran):

```json
{
  "batchId": "45eeefba-dd39-6831-1bd2-d986851f7c1f",
  "selectionMode": "CANARY_A",
  "dryRun": true,
  "limit": 25,
  "environmentName": "Staging",
  "manifestId": "RSMMC-CANARY-A",
  "manifestVersion": 1,
  "manifestSha256": "c43207eb73f1f244db13ff930bca1c22e2e4ae779018e5cd48e26f628a5a79ad",
  "manifestSize": 25,
  "totalInManifest": 25, "alreadyMigrated": 0,
  "eligibleBeforeRun": 25, "selected": 25,
  "ready": 25, "migrated": 0, "conflict": 0, "failed": 0, "notProcessed": 0, "remaining": 25,
  "stopped": false, "stopReason": null,
  "gates": [ { "code": "PREVIEW_SCHEMA_SIGNATURE", "expected": "Seluruh kolom sesuai", "actual": "Seluruh kolom sesuai", "passed": true } ],
  "items": [
    {
      "legacyPid": "-667992", "sourceGroup": "NOREG_WITHOUT_RM", "crosswalkStateBefore": "MISSING",
      "patientId": null, "patientCode": "PAT-RSMMC-12345", "medicalRecordNumber": "00-12-34-56",
      "plannedQrCodePath": "/uploads/patient-qrcodes/rsmmc-bulk/00-12-34-56/qrcode.png",
      "fullName": "Siti Contoh", "hasIdentityNumber": true, "hasPhoneNumber": false,
      "status": "READY", "historicalQrAtDefaultPath": true, "historicalQrLength": 4812,
      "historicalQrSha256": "9f2c…", "plannedQrExists": false,
      "conflictType": null, "failureStage": null, "message": "Pasien layak dibuat dari preview."
    }
  ]
}
```

| Kode | Arti | Kode galat |
| --- | --- | --- |
| `200` | Simulasi atau eksekusi selesai — termasuk bila berhenti di tengah karena konflik/gagal | — |
| `400` | Isian tidak sah atau aktor eksekusi tidak dikenali | `RSMMC_BULK_BATCH_ID_REQUIRED`, `RSMMC_BULK_SELECTION_MODE_NOT_SUPPORTED`, `RSMMC_BULK_LIMIT_OUT_OF_RANGE`, `RSMMC_BULK_ACTOR_UNKNOWN` |
| `401` | Belum login | — |
| `403` | Tanpa izin `Patient : Update`, atau environment bukan `Staging` persis — sebelum tabel apa pun dibaca | `RSMMC_BULK_ENVIRONMENT_NOT_ALLOWED` |
| `409` | Eksekusi ditolak karena gerbang gagal; daftar gerbang di `errors.gates` | `RSMMC_BULK_GATE_FAILED` |

---

## 5. Verifikasi

> **Bukti akhir ada di bagian 11** — validasi post-rebase kedua pada commit `46edc6af` di atas Integration
> `52d7e117`, termasuk provider-real PostgreSQL 15.19 `6`/`6` dan `29`/`29` `PASS`. Bagian 10 memuat bukti
> post-rebase pertama pada `f0ee1c98` di atas `35951dbb`. Tabel di bawah adalah bukti pra-rebase dan
> dipertahankan sebagai jejak.

Seluruh baris di bawah dijalankan ulang pada sesi lanjutan 22 Oktober 2026 di atas HEAD `c83bcdaa`
(*working tree* task ini), kecuali disebut lain.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `git diff --check` | Tanpa keluaran, exit `0`; berkas baru: 0 baris *trailing whitespace* | `PASS` | Keluaran perintah |
| Audit berkas terlarang: `git diff --stat c83bcdaa -- Models Repositories Migrations *.sln RsmmcPilot* RsmmcPilotMrnReconciliationDtos.cs appsettings* *.csproj .github Dockerfile` | Kosong — tidak ada perubahan | `PASS` | Keluaran perintah; `git status` tanpa `patient-qrcodes/` atau `.png` |
| `dotnet build QuilvianSystemBackend.csproj -c Release --no-incremental` | `0` error, `298` warning; **0 warning dari berkas yang diubah** | `PASS` (warning `EXISTING`, sama dengan baseline laporan `BE-PAT-MIG-003A`) | Log build disaring per berkas |
| `dotnet build … -c Release --no-incremental -p:RunAnalyzers=false` (`PAT-MIG2-AC-41`) | `0` error, `298` warning | `PASS` | Keluaran perintah |
| `dotnet build Tests/QuilvianSystemBackend.PatientManagementTests -c Release` | `0` error, `10` warning — 0 dari berkas task ini | `PASS` | Keluaran perintah |
| `dotnet test … --filter "FullyQualifiedName~MigrasiMassal&Kategori!=ProviderReal"` (fix pass review pemilik) | `202` lulus / `0` gagal / `0` dilewati (Total 202). Run sebelum fix pass tanpa pengecualian provider-real: `201` lulus / `0` gagal / `6` dilewati — keenam yang dilewati adalah uji provider-real | `PASS` | Keluaran perintah |
| `dotnet test … --filter "Kategori!=ProviderReal"` — regresi PatientManagement non-provider | `505` lulus / `0` gagal / `0` dilewati (Total 505) — termasuk test `BE-PAT-MIG-001`, `003`, `003A`, membership, dan `MigrasiMassal` | `PASS` | Keluaran perintah |
| `dotnet test … --filter "FullyQualifiedName~Membership"` — F-01 tetap hijau | `97` lulus / `0` gagal / `0` dilewati | `PASS` | Keluaran perintah |
| Provider-real PostgreSQL 15 (`Kategori=ProviderReal`, 6 kasus `MigrasiMassalPostgresSungguhanTests`) | **Tidak dijalankan** — `QUILVIAN_PG_PROOF_CONNECTION` dan `QUILVIAN_PG_PROOF_REQUIRED` tidak diisi; uji tercatat **dilewati**, bukan lulus | **`PENDING OWNER-RUN`** pada 22 Oktober 2026 — kemudian `PASS` `6`/`6` dan `29`/`29` post-rebase (bagian 10.2) | Pemeriksaan keberadaan variabel tanpa mencetak nilai |
| QBE `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (*working tree*) | 22 berkas `.cs` dievaluasi (13 berkas lingkup test); `VIOLATION 0`, `REVIEW 0`, `INFO 0`; `Final result: PASS` | `PASS` | Keluaran checker |
| `dotnet ef migrations has-pending-model-changes --context ApplicationDbContext --configuration Release --no-build` dengan `ConnectionStrings__DefaultConnection` diarahkan ke host `.invalid` | "No changes have been made to the model since the last migration." | `PASS` | Tidak ada koneksi ke DEV/STAGING/PRODUCTION |

`AUTOMATED TEST: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests (non-provider) — PASS` (post-rebase `505`/`505`, bagian 10 dan 11)
`AUTOMATED TEST PROVIDER-REAL: MigrasiMassalPostgresSungguhanTests — PASS 6/6; Kategori=ProviderReal penuh — PASS 29/29` (post-rebase, PostgreSQL 15.19 sekali pakai, dijalankan pemilik — bagian 10 dan 11). *Jejak 22 Oktober 2026:* `PENDING OWNER-RUN` — server bukti tidak tersedia pada sesi build

### 5.1 Cara menjalankan bukti provider-real (pemilik)

*Sudah dijalankan pemilik — hasil post-rebase `6`/`6` dan `29`/`29` `PASS` pada bagian 10.2 (`f0ee1c98`) dan 11.2 (`46edc6af`).*

Server PostgreSQL 15 sekali pakai milik pemilik — **bukan** DEV, STAGING, atau PRODUCTION; server bukti
mungkin membutuhkan `max_locks_per_transaction = 2048` (`F-14`):

```text
QUILVIAN_PG_PROOF_CONNECTION="Host=…;Username=…;Password=…;Database=postgres"
QUILVIAN_PG_PROOF_REQUIRED=1
dotnet test Tests/QuilvianSystemBackend.PatientManagementTests --filter "FullyQualifiedName~MigrasiMassalPostgresSungguhanTests" -- xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1
```

| Kasus | Yang dibuktikan (`PAT-DEC-065`, B15) |
| --- | --- |
| `A_Simulasi…` | 17 gerbang lulus pada skema PostgreSQL bertipe (`information_schema`, `uuid`/`bigint`/larik) tanpa penulisan |
| `B_Eksekusi…` | `IsMember = false` tersimpan **fisik** lewat INSERT (dibaca SQL mentah dari sesi lain); `IsActive = true`; baca ulang `AsNoTracking` cocok dengan preview; kolom crosswalk `PAT-DEC-047`; tanpa UPDATE; kunci lepas bersama transaksi |
| `C_D_Rollback…` | Kegagalan sesudah INSERT pasien + crosswalk → tidak ada pasien maupun crosswalk; percobaan ulang berhasil; panggilan ketiga `ALREADY_MIGRATED` tanpa target ganda |
| `E_GagalSebelumCommit…` | Kegagalan sebelum *commit* tidak meninggalkan pemetaan ter-*commit*; panggilan berikutnya tanpa pasien kedua |
| `F_KunciDipegangSesiLain…` | Kunci `(1173286842, -667992)` dari sesi lain → `CONCURRENT_RUN` tanpa menunggu; kunci allocator normal `(1346458697, 1313821261)` tidak menahan worker |
| `G_TipeFisikBerbeda…` | `birth_date` bertipe `timestamp` → `PREVIEW_SCHEMA_SIGNATURE` gagal, simulasi tidak mengklasifikasikan, eksekusi `409` |

### 5.2 Pemetaan test ke topik wajib

| Topik wajib prompt pemilik | Test |
| --- | --- |
| `Staging` persis; ditolak sebelum baca store/tabel | `LingkunganSelainStagingPersis_Ditolak403SebelumSatuTabelPunDibaca` (11 kasus: 0 pembacaan store, 0 perintah EF, 0 folder) |
| Bawaan `dryRun=true`, `limit=25`; mode/limit tidak sah | `TanpaDryRunDanLimit_BawaanSimulasiDanLimit25`, `SelectionModeSelainCanaryA_*` (6), `LimitDiLuar1Sampai25_*` (4), `LimitBatasSahDanBawaan_*` (3), `BatchIdTidakDikirimAtauKosong_*` |
| Batch kanonik tepat | `BatchSelainKanonik_EksekusiDitolak_SimulasiMelaporkanPreviewBerubah`, skenario `batch-dihapus` |
| Manifest 25 nilai, urutan, SHA-256 berurutan, sebaran kelompok | `Manifest_*` (7), `ManifestUrutanBerbedaDariSidikJariDisetujui_Ditolak`, `ManifestBerisiNilaiGanda_Ditolak`, skenario `kelompok-sumber-berubah`, `pasien-manifest-tanpa-preview` |
| Rumus baseline 741 + worker | `Profil_NilaiKanonikPersis`, `PatientBaseline_RumusTetapDitambahCrosswalkWorkerBatchKanonikSaja`, `PatientBaseline_NilaiTidakDiambilDariKonfigurasiAtauPermintaan`, skenario `pasien-tambahan`, `pasien-terhapus-tetap-dihitung`, `Limit5_ContohKontrak_*` (gerbang `5 + 5`) |
| Himpunan dan cadangan tertunda | Skenario `tertunda-tambahan`, `tertunda-hilang`, `kode-cadangan-dipakai`, `mrn-cadangan-dipakai`; `Simulasi_LegacyPidTertundaDiManifest_DeferredLegacyPid` |
| Gerbang preview/perencana | Skenario `preview-hilang`, `kode-ganda`, `mrn-ganda`, `perencana-bukan-finalized`; `DataBaku_SeluruhGerbangLulus_DalamUrutanKontrak` |
| Perilaku store bertipe | `KueriStoreMigrasiMassalTests` (18 SQL tanpa `::text`, satu tulis, kolom `PAT-DEC-047`, `NpgsqlDbType`, wajib transaksi) |
| `legacy_pid` Int64; konversi Int32 termasuk negatif | `KonversiKunci_*` (9 kasus + seluruh manifest), `Simulasi_LegacyPidDiLuarInt32_*`, `LegacyPidDiLuarInt32_FailedLockKeyOutOfRange_TanpaKunciDanBerhenti` |
| `CONCURRENT_RUN` | `KunciDipegangSesiLain_ConcurrentRunTanpaMenunggu_LaluBerhasilSesudahLepas` |
| Simulasi nol mutasi DB/filesystem | `Simulasi_TanpaMutasiDatabaseMaupunFilesystem` (snapshot DB dan berkas, 0 transaksi, 0 kunci, 0 folder) |
| Tanpa pengganti manifest | `KonflikTakTerduga_MenghentikanSisaTanpaPasienPengganti`, `Simulasi_SeluruhPasienManifestDalamUrutanManifest_*` |
| Klasifikasi B3.2 | `Simulasi_KlasifikasiB32_*` (33 kasus) |
| Ulang idempoten | `DiulangSesudahSelesai_Idempoten_TanpaPasienAtauQrKedua`, `Limit5_ContohKontrak_*` |
| Crosswalk tanpa target tidak disentuh | `KonflikTakTerduga_*` skenario `crosswalk-tanpa-target` (baris sebelum = sesudah) |
| Yatim/beda/ganda | Skenario `crosswalk-yatim`, `target-beda-kode`, `target-beda-mrn`, `target-ganda` |
| Tabrakan kode/MRN/NIK/telepon | Skenario `kode-dipakai*`, `mrn-dipakai*`, `nik-dipakai*`, `telepon-dipakai*`; `KeadaanDibacaUlangSesudahKunci_KodeDipakaiSetelahSnapshot_Konflik` |
| Path QR bulk; historis hanya informasi; `CreateNew` | `QrNamespaceBulkTests`, `Simulasi_BarisMemuatRencanaDanBuktiQrHistoris_*`, `Simulasi_BerkasDiPathBulk_*`, `KasusG_*`, `BerkasQrMunculSaatDibentuk_KonflikRace_*`, `Controller_PenulisQrBulk_*` |
| Pembersihan berbasis kepemilikan | `Pembersihan_*` (3) di `QrNamespaceBulkTests`; `KasusJ_*` (QR dipertahankan) |
| Verifikasi `IsMember = false` | `IsMemberFalse_DikirimEksplisitPadaInsert_TanpaUpdateSesudahnya`, `NilaiTersimpanBerbedaDariPreview_FailedVerifyPersisted_Rollback` (4 kasus), provider-real `B_*` |
| Pembatalan | `Pembatalan_*` (3) |
| Berhenti pada konflik/gagal pertama | `KasusG_*`, `KonflikTakTerduga_*` (6), `Gagal*` (3), `KasusJ_*` (2) |
| Jumlah/status/`remaining` | `Simulasi_RingkasanSidikJariDanJumlah`, `Limit5_*`, `SudahDimigrasiDanQrTidakKonsisten_*`, `Simulasi_PasienSudahDimigrasi_*` |
| Tanpa PII di log | `LogTanpaDataPribadiTests` (4) |
| Pilot tidak berubah | `Governance_SourcePilotTidakBerubahDariCommitInduk` (hash SHA-256 isi berkas, dinormalkan LF, terhadap `c83bcdaa`), `Endpoint_SesudahUpdatePatient_*`, test Pilot lama tetap lulus |

---

## 6. Acceptance criteria dan Definition of Done

| AC | Keadaan | Bukti |
| --- | --- | --- |
| `AC-01` | Terpenuhi | Gerbang environment pertama sesudah validasi isian; uji 11 nama environment × mode; `Controller_LingkunganDitolak_403DenganKode` |
| `AC-02` | Terpenuhi | `APPROVED_BATCH`, `BATCH_ROW_EXISTS`; eksekusi batch lain `409` |
| `AC-03` | Terpenuhi | `PREVIEW_ROWS`, `PREVIEW_UNIQUE`; profil 704.506 |
| `AC-04` | Terpenuhi | `PLANNER_FINALIZED_ROWS`; profil 705.223 |
| `AC-05` | Terpenuhi | `DEFERRED_SET`, `DEFERRED_RESERVATIONS_UNUSED` (kode **dan** MRN akhir tertunda) |
| `AC-06` | Terpenuhi | `PATIENT_BASELINE` = 741 + crosswalk `MIGRATED_BULK_WORKER` batch kanonik; seluruh baris tanpa filter `IsDelete`; Pilot dan crosswalk batch lain tidak terhitung |
| `AC-07`, `AC-08` | Terpenuhi | `PREVIEW_CODE_UNMAPPED`/`PREVIEW_MRN_UNMAPPED`; pemeriksaan per pasien sesudah kunci termasuk pasien terhapus; uji baca ulang sesudah kunci |
| `AC-09` | Terpenuhi | Manifest konstanta 25 nilai berurutan; SHA-256 dihitung ulang; sebaran 7/6/6/6; respons `manifestId`/`Version`/`Sha256` |
| `AC-10` | Terpenuhi (SQLite + PostgreSQL 15) | INSERT pasien dan crosswalk di transaksi yang sama; ulang → `ALREADY_MIGRATED`; provider-real `C_D_*`, `E_*` lulus (bagian 10.2) |
| `AC-11` | Terpenuhi (source + SQLite + PostgreSQL 15) | Kunci dua bagian `1173286842`, konversi Int32, `CONCURRENT_RUN`, baca ulang sesudah kunci; provider-real `F_*` lulus (bagian 10.2) |
| `AC-12`–`AC-16` | Terpenuhi | Kasus A, B, C+D, E, F — uji simulasi dan eksekusi |
| `AC-17` | Terpenuhi | Layout `rsmmc-bulk`, `CreateNew`, konflik membatalkan transaksi |
| `AC-18` | Terpenuhi | `TryDeleteCreated` apa adanya dengan bukti kepemilikan (`QrNamespaceBulkTests`); service hanya mengirim artefak buatannya sendiri |
| `AC-19` | Terpenuhi | Kasus J dua sisi: *commit* tidak terjadi → kasus G berikutnya; *commit* terjadi → `ALREADY_MIGRATED` berikutnya |
| `AC-20` | Terpenuhi | `FAILED`/`CREATE_QR`, tanpa *commit*, tanpa berkas; berkas sebagian tidak dihapus (penulis QR) |
| `AC-21`, `AC-22` | Terpenuhi | Simulasi nol mutasi; satu transaksi per pasien, pasien ter-*commit* tidak ikut batal |
| `AC-23` | Terpenuhi | Log B11 dengan `actorUserId`; uji tanpa PII |
| `AC-24`–`AC-28` | Terpenuhi | Kode/MRN dari preview/perencana; pemeriksa guard hanya `CheckAsync` baca-saja; NIK hanya untuk tabrakan; tanpa nama pengganti; dua tertunda ditolak |
| `AC-29`–`AC-32` | Terpenuhi | Hanya INSERT; baris lama tidak berubah; gerbang Pilot; QR historis utuh; tanpa pengaturan pelonggar |
| `AC-33` | Terpenuhi (SQLite + PostgreSQL 15) | `VERIFY_PERSISTED` membaca nilai fisik; selisih → rollback; bukti fisik PostgreSQL pada `AC-47` |
| `AC-34` | Terpenuhi | Tanpa migration/model; `has-pending-model-changes` bersih |
| `AC-35`, `AC-36` | `NANTI` — runtime | Milik `PAT-GATE-002` |
| `AC-37`–`AC-40` | Terpenuhi | Hanya `CANARY_A`, tanpa *hosted service*; respons B2.2; izin dan aktor; validasi |
| `AC-41` | Terpenuhi | Build `0` error (dengan dan tanpa analyzer), QBE `Strict` `PASS`, test terfokus lulus |
| `AC-42` | Terpenuhi | Berkas `RsmmcPilot*` tidak berubah (diff + pin hash); test Pilot lama lulus |
| `AC-43` | Terpenuhi (source/test); SHA-256 runtime di `PAT-GATE-002` | Path historis tidak pernah ditulis; QR historis utuh pada uji |
| `AC-44`, `AC-45` | Dipindah | `BE-PAT-MIG-003` |
| `AC-46` | Terpenuhi | `NORMAL_ALLOCATION_GUARD` memakai `CheckAsync` apa adanya; guard tidak aktif/tidak dikonfigurasi → `409` |
| `AC-47` | **Terpenuhi** — post-rebase | Uji SQLite lulus; **provider-real PostgreSQL 15.19 `MigrasiMassalPostgresSungguhanTests` `6`/`6` `PASS`** (bagian 10.2) atas tujuh hal `PAT-DEC-065`: `IsMember = false` tersimpan fisik lewat INSERT, `IsActive = true`, baca ulang `AsNoTracking` cocok dengan preview, rollback tanpa pasien maupun crosswalk, percobaan ulang idempoten, kegagalan sebelum *commit* tanpa pemetaan ter-*commit*. Diulang pada kandidat akhir `46edc6af`: `6`/`6`, penuh `29`/`29` `PASS` (bagian 11.2). *Jejak 22 Oktober 2026:* sebagian — `PENDING OWNER-RUN` |
| `AC-48` | Terpenuhi | Manifest = 25 kandidat dalam urutan yang sama, tanpa penggantian |
| `AC-49`–`AC-51` | Terpenuhi (SQLite + teks SQL + PostgreSQL 15) | Kolom `PAT-DEC-047`; `PHONE_ALREADY_USED`; SQL bertipe, `PREVIEW_SCHEMA_SIGNATURE`, `PREVIEW_VALUE_INVALID`; eksekusi SQL bertipe pada PostgreSQL 15 lewat provider-real `A_*`/`B_*`/`G_*` lulus (bagian 10.2) |

**Definition of Done — fase implementasi.**

- [x] Endpoint sesuai kontrak Bagian B `1.3.0`
- [x] Build backend lulus — Release `0` error
- [x] QBE `Strict` lulus
- [x] Test terfokus lulus, termasuk persistensi `IsMember = false` (SQLite)
- [x] Bukti provider-real `IsMember = false` pada PostgreSQL 15 sekali pakai — `6`/`6` `PASS` post-rebase, dijalankan pemilik (bagian 10.2). *Jejak 22 Oktober 2026: `PENDING OWNER-RUN`*
- [x] Tidak ada berkas di luar cakupan yang berubah; perilaku `BE-PAT-MIG-001` tetap
- [x] Laporan task lengkap — dokumen ini
- [ ] Source sudah direview leader
- [x] **Belum ada** eksekusi Canary-A

---

## 7. Arsitektur dan keputusan desain

| ID | Keputusan saat build | Alasan |
| --- | --- | --- |
| D-01 | Aksi baru diletakkan **sesudah** `UpdatePatient` dan **sebelum** `ReconcileRsmmcPilotMrn` | B8 hanya mensyaratkan "sesudah aksi `Update` yang sudah ada" supaya label Akses Role tetap milik `UpdatePatient`. Test lama `Endpoint_DiletakkanSesudahAksiUpdateLama_*` mengunci aksi Pilot sebagai aksi `Update` terakhir; letak ini memenuhi keduanya tanpa mengubah test itu |
| D-02 | Test inventaris `KetigaRutePendaftaran_BermuaraPadaDuaActionYangDiketahui` ditambah satu anggota | Assertion daftar tertutup rute POST non-pendaftaran; endpoint baru disetujui kontrak B2 dan tidak memakai allocator. Daftar tetap persis — tidak dilemahkan |
| D-03 | `BACKUP_ROWS` menghitung **seluruh** isi `migration.rsmmc_patient_pilot_reconcile_backup` dan membandingkannya dengan tepat **715** — **diterima pemilik** pada review 22 Oktober 2026 | (1) Kontrak B4.2 hanya menetapkan "Tabel cadangan Pilot = 715"; (2) tipe fisik `batch_id` tabel cadangan tidak dikunci pada `PAT-FACT-079`–`083`; (3) worker final tidak boleh memakai *fallback* `::text`; (4) hitungan seluruh tabel gagal tertutup — baris tambahan dari batch mana pun membuat gerbang gagal. Cadangan Pilot seluruhnya milik batch kanonik (`RsmmcPilotApprovedBatchProfile`). Tidak ada perubahan tabel atau basis data |
| D-04 | `PREVIEW_SCHEMA_SIGNATURE` juga mewajibkan `rsmmc_patient_batch.batch_id` `uuid` | Dibaca dengan parameter `uuid`; tipenya dibuktikan foreign key `last_batch_id` (`PAT-FACT-079`). Tambahan gagal tertutup |
| D-05 | Respons ringkasan **hanya** field B2.2. `alreadyMigrated` menghitung baris `ALREADY_MIGRATED` **dan** `MIGRATED_QR_INCONSISTENT`; `eligibleBeforeRun = totalInManifest − alreadyMigrated`; `ready` hanya terisi pada simulasi. Status per baris `MIGRATED_QR_INCONSISTENT` tetap | Keputusan pemilik pada review 22 Oktober 2026 (`F-01`): tidak ada field respons publik di luar kontrak. Pasien `MIGRATED_QR_INCONSISTENT` sudah punya pemetaan dan pasien target yang cocok (B3.2 "seperti baris di atas"), sehingga termasuk "sudah dimigrasi". *Versi awal — dicabut:* field aditif `migratedQrInconsistent` |
| D-06 | Validasi isian di service, bukan atribut DTO | `[ApiController]` akan mengembalikan `400` ModelState tanpa kode B2.3; validasi di service menjamin kode `RSMMC_BULK_*` |
| D-07 | Isi minimum log B11 ditulis ke teks pesan (`nama=nilai`) | `LoggerService` hanya merender pesan dan identitas pengguna, bukan objek `data`. Hanya nilai aman yang dikirim |
| D-08 | Tabrakan identitas dan telepon hanya terhadap pasien tidak dihapus; kode dan MRN termasuk pasien terhapus | B4.3 langkah 6; indeks unik `IdentityNumber` terfilter `IsDelete = false`; telepon tidak berindeks unik |
| D-09 | Preview yang bukan milik batch kanonik → `PREVIEW_STATE_CHANGED` | Baris B3.2 "bukan batch kanonik" |
| D-10 | Bila tanda tangan skema gagal, gerbang yang butuh kueri bertipe ditandai `NOT_EVALUATED` dan pasien simulasi `NOT_PROCESSED` | `PAT-GAP-009`: kueri bertipe tidak boleh dijalankan pada skema yang tidak cocok |
| D-11 | Sejak `INSERT_PATIENT` sampai `COMMIT` token pembatalan tidak dipakai | Pembatalan request tidak boleh memotong transaksi sesudah QR mungkin dibuat. Pembatalan sebelum mutasi → `NOT_PROCESSED` dan berhenti |
| D-12 | Store memakai `SqlQueryRaw` dengan `NpgsqlParameter` bertipe eksplisit | Menjamin `uuid`/`bigint`/`integer`/larik pada tiap parameter (`PAT-DEC-063`), alih-alih inferensi `FormattableString` |
| D-13 | Store tiruan di `MigrasiMassal/Infrastruktur/`, bukan `Infrastructure/TiruanStoreMigrasiMassal.cs` | Rencana berkas mengizinkan penyesuaian nama/letak; infrastruktur ini khusus task ini |
| D-14 | Store tiruan SQLite memakai skema `migration` yang di-ATTACH dan transaksi konteks yang sama | INSERT crosswalk ikut *commit*/*rollback* bersama `MstPatient`, sehingga rollback benar-benar teruji tanpa basis data environment mana pun |

---

## 8. Catatan penutup

**Risiko yang tersisa.**

| Risiko | Penanganan |
| --- | --- |
| ~~SQL bertipe, `information_schema`, dan kunci dua bagian belum dieksekusi pada PostgreSQL sungguhan~~ | **Tertutup** — provider-real PostgreSQL 15.19 `6`/`6` dan `29`/`29` `PASS` post-rebase (bagian 10.2). Skema Staging yang sebenarnya tetap dijaga `PREVIEW_SCHEMA_SIGNATURE` saat runtime: bila tipe fisiknya berbeda, gerbang menolak sebelum kueri bertipe |
| Tipe fisik `batch_id` tabel cadangan tidak diketahui | Tidak lagi dibutuhkan (D-03, diterima pemilik) |
| Setiap pemegang `Patient : Update` di Staging dapat memanggil endpoint (`PAT-RSK-005`) | Penjaga sesungguhnya `PAT-GATE-002`, manifest beku, dan gerbang B4.2 |
| Warning NuGet `NU1902`/`NU1903` untuk paket `SixLabors.ImageSharp`, tercatat pada validasi post-rebase | **Di luar cakupan MIG-002** — dicatat sebagai *follow-up* keamanan dependency terpisah (bagian 10.4); tidak diperbaiki otomatis dalam task ini |

**Tidak dilakukan** (sesuai larangan): pemanggilan endpoint, simulasi maupun eksekusi terhadap Staging,
Canary-A, akses basis data DEV/STAGING/PRODUCTION, pembuatan atau penerapan EF migration, eksekusi SQL ke
environment, pembuatan QR sungguhan, inventaris filesystem server, perubahan Docker/GitHub Actions,
deploy, commit, push, PR. `PAT-GATE-002` dan Canary-A tetap **`NOT RUN`**. Commit `f0ee1c98` (author
`devbenari`) dan *rebase* ke `35951dbb`, serta *rebase* berikutnya ke `52d7e117` yang menghasilkan
`46edc6af`, dibuat di luar sesi agent; agent tidak melakukan commit, push, *rebase*, maupun PR.

**Tindakan berikutnya** (diperbarui 25 Oktober 2026; daftar 22 Oktober 2026 ada sebagai jejak di bawah).

1. Review source oleh leader atas kandidat akhir `46edc6af`.
2. Push, PR, dan merge oleh pemilik — masing-masing atas keputusan pemilik.
3. Deploy artefak `BE-PAT-MIG-002` ke Staging — butuh otorisasi deployment terpisah.
4. Prasyarat `PAT-GATE-002`: bukti pembekuan keempat jalur pembuatan pasien, inventaris filesystem
   read-only, lalu otorisasi (a) simulasi dan (b) eksekusi. Sampai otorisasi itu ada, **tidak ada**
   migrasi pasien lewat worker — baik Canary-A 25 pasien maupun migrasi 704.506 pasien.
5. *Follow-up* terpisah: warning `NU1902`/`NU1903` `SixLabors.ImageSharp` (bagian 10.4).

*Jejak 22 Oktober 2026:* (1) pemilik menjalankan uji provider-real — **selesai**, bagian 10.2; (2) review
source oleh leader; commit dan PR oleh pemilik; (3) sesudah `BE-PAT-MIG-002` ✅: prasyarat `PAT-GATE-002`.

---

## 9. Koreksi review pemilik — 22 Oktober 2026

| Temuan | Keputusan pemilik | Yang dikerjakan |
| --- | --- | --- |
| `F-01` — *contract drift* respons | Tidak boleh ada field respons publik di luar kontrak `1.3.0` B2.2; kontrak tidak diubah | Field `migratedQrInconsistent` — **tidak** tercantum di B2.2 (kontrak baris 530 hanya `totalInManifest`, `alreadyMigrated`, `eligibleBeforeRun`, `selected`) — **dihapus** dari `RsmmcBulkPatientMigrationResponse` dan dari log simulasi. Status per baris `MIGRATED_QR_INCONSISTENT` tetap (B3.2). `alreadyMigrated` kini menghitung `ALREADY_MIGRATED` + `MIGRATED_QR_INCONSISTENT`; `eligibleBeforeRun = totalInManifest − alreadyMigrated` (nilainya sama dengan sebelum koreksi). Test baru `Respons_FieldPersisKontrakB22_TanpaTambahan` mengunci field ringkasan (23), gerbang (5), dan baris (18) persis daftar B2.2; tiga test lama disesuaikan |
| `F-02` — `BACKUP_ROWS` | **Diterima**: hitung seluruh isi tabel cadangan, bandingkan dengan tepat 715 | Rationale dicatat pada bagian 7, D-03. Tidak ada perubahan source, tabel, atau basis data |
| `F-03` — konsistensi dokumentasi | Audit *working tree* saat ini | Laporan ini **ada** sebagai berkas baru tak terlacak (`??`); `backend-roadmap.md` dan `requirement-traceability.md` **berubah** (`M`). Status Git yang dilihat pemilik sebelumnya adalah keluaran awal sesi lanjutan, sebelum dokumen ditulis |

**Revalidasi sesudah koreksi** (dijalankan pada fix pass ini):

| Perintah | Hasil |
| --- | --- |
| `git diff --check` | Bersih, exit `0` |
| Build Release `QuilvianSystemBackend.csproj --no-incremental` | `0` error, `298` warning — 0 dari berkas yang diubah |
| Build `QuilvianSystemBackend.PatientManagementTests` | `0` error, `10` warning — 0 dari berkas task ini |
| `--filter "FullyQualifiedName~MigrasiMassal&Kategori!=ProviderReal"` | `202` lulus / `0` gagal / `0` dilewati |
| `--filter "Kategori!=ProviderReal"` | `505` lulus / `0` gagal / `0` dilewati |
| `--filter "FullyQualifiedName~Membership"` | `97` lulus / `0` gagal |
| QBE `Strict` | 22 berkas, 0 temuan, `PASS` |
| Audit berkas terlarang terhadap `c83bcdaa` (termasuk `contracts/`) | Kosong |
| Provider-real | Tidak diminta pada fix pass ini — tetap **`PENDING OWNER-RUN`** (kemudian dijalankan pemilik post-rebase, bagian 10) |

`PAT-GATE-002` dan Canary-A tetap **`NOT RUN`**.

---

## 10. Bukti post-rebase pertama — `f0ee1c98` di atas `35951dbb`, dilaporkan pemilik

> **Digantikan bagian 11 sebagai bukti akhir** sesudah *rebase* ke Integration `52d7e117`. Isi bagian ini
> dipertahankan sebagai jejak.

Seluruh hasil pada bagian ini **dijalankan pemilik** dan dilaporkan untuk finalisasi dokumentasi
24 Oktober 2026; tanggal eksekusi run tidak dicantumkan dalam laporan pemilik. Agent tidak menjalankan
build, test, maupun akses basis data pada finalisasi ini; yang diperiksa agent hanya data Git baca-saja
pada bagian 10.1.

### 10.1 Baseline

| Field | Nilai |
| --- | --- |
| Baseline Integration yang diuji | `35951dbb6ba8ce8466a958d35a98d01f67c5efa2` — `Merge pull request #306 from DevBenari/yoga` |
| Commit `BE-PAT-MIG-002` yang diuji | `f0ee1c989b5cc7a9968d2bc11faeec40e3338860` — `feat(patient): implement RSMMC bulk migration worker`, author `devbenari`, 23 Oktober 2026; induk langsung `35951dbb` |
| Isi commit | 25 berkas: 7 source baru, `PatientController.cs`, `Program.cs`, 12 berkas test `MigrasiMassal/`, `JalurPendaftaranReservasiTests.cs`, laporan ini, roadmap, dan traceability. Tidak menyentuh `Models/`, `Repositories/`, `Migrations/`, `appsettings*`, `.sln`, `RsmmcPilot*`, maupun `contracts/` (`git show --stat f0ee1c98`) |
| Status Git | Hanya lokal: **belum** push, PR, merge, maupun deploy |

### 10.2 Hasil validasi post-rebase

| Pemeriksaan | Hasil |
| --- | --- |
| Build Release backend | `PASS` — `0` error |
| Build `QuilvianSystemBackend.PatientManagementTests` | `PASS` |
| MIG-002 terfokus non-provider | `202`/`202` `PASS` |
| Seluruh PatientManagement non-provider | `505`/`505` `PASS` |
| Regresi membership | `97`/`97` `PASS` |
| QBE `Strict` mode `GitRange` | `PASS` — 43 berkas dievaluasi; `VIOLATION 0`, `REVIEW 0`, `INFO 0` |
| PostgreSQL 15 provider-real terfokus MIG-002 (`MigrasiMassalPostgresSungguhanTests`) | `6`/`6` `PASS` |
| PostgreSQL 15 provider-real penuh (`Kategori=ProviderReal`) | `29`/`29` `PASS` — gagal `0`, dilewati `0` |

**Jangan menjumlahkan angka di atas.** Suite-suite itu saling tumpang tindih: `202` dan `97` termasuk di
dalam `505`, dan `6` termasuk di dalam `29`. Jumlahnya **bukan** jumlah test unik.

`has-pending-model-changes` tidak termasuk bukti post-rebase pemilik; bukti terakhirnya pada `c83bcdaa`
(bagian 5). Commit `f0ee1c98` tidak menyentuh `Models/`, `Repositories/`, maupun `Migrations/`
(bagian 10.1).

`AUTOMATED TEST: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests (non-provider) — PASS 505/505`
`AUTOMATED TEST PROVIDER-REAL: Kategori=ProviderReal — PASS 29/29 (MigrasiMassalPostgresSungguhanTests 6/6)`

### 10.3 Lingkungan bukti dan pembersihan

| Hal | Keadaan |
| --- | --- |
| Server | PostgreSQL **15.19** sekali pakai; `max_locks_per_transaction = 2048` — syarat server uji saja, bukan anjuran untuk DEV/STAGING/PRODUCTION |
| Akses | Lewat SSH tunnel ke `localhost`. **Tidak ada** basis data DEV, STAGING, maupun PRODUCTION yang dipakai |
| Basis data `pat_proof_*` tersisa | `0` |
| Kontainer PostgreSQL sekali pakai | Dihentikan dan dihapus |
| Port VM `55432` | Tidak lagi *listening* |
| SSH tunnel Windows | Ditutup; uji koneksi port `55432` di Windows = `False` |
| Variabel environment `QUILVIAN_PG_PROOF_*` | Dihapus |

### 10.4 Temuan lanjutan di luar cakupan — `SixLabors.ImageSharp` `NU1902`/`NU1903`

Validasi post-rebase menampilkan warning NuGet `NU1902` dan `NU1903` untuk paket `SixLabors.ImageSharp`
— peringatan audit NuGet bahwa versi paket yang dipakai memiliki kerentanan yang diketahui. Warning ini
**bukan** bagian `BE-PAT-MIG-002`: task ini tidak menambah atau mengubah dependency, dan perubahan paket
membutuhkan keputusan pemilik. Dicatat sebagai *follow-up* keamanan dependency yang terpisah dan **tidak**
diperbaiki otomatis dalam MIG-002.

### 10.5 Keadaan task dan gerbang

| Hal | Keadaan |
| --- | --- |
| Status task | 🟡 **IMPLEMENTED — VALIDATED LOCALLY** |
| Acceptance criteria | 47 dari 49 AC aktif terbukti — termasuk `AC-47` (provider-real); `AC-35`/`36` milik runtime `PAT-GATE-002`; `AC-44`/`45` dipindah ke `BE-PAT-MIG-003` |
| DoD fase implementasi | Seluruhnya terpenuhi **kecuali** review source leader |
| Git | Commit lokal `f0ee1c98`; **belum** push, PR, merge |
| Deploy | **Belum** |
| `PAT-GATE-002` | **`NOT RUN`** |
| Canary-A Staging | **`NOT RUN`** |
| Migrasi pasien aktual lewat worker | **Belum ada** |
| Otorisasi migrasi pasien | **Tidak ada** otorisasi untuk migrasi pasien saat ini |

Seluruh keputusan dan batasan migrasi yang disetujui tetap berlaku: batch kanonik
`45eeefba-dd39-6831-1bd2-d986851f7c1f`, baseline `741`, manifest `RSMMC-CANARY-A` v1 (25 `legacy_pid`,
urutan dan SHA-256 berurutan `c43207eb…79ad`), lantai reservasi, Pilot 715 dan 285 crosswalk historis
tidak disentuh, serta tanpa EF migration.

---

## 11. Bukti akhir post-rebase kedua — `46edc6af` di atas `52d7e117`, dilaporkan pemilik

Seluruh hasil pada bagian ini **dijalankan pemilik** dan dilaporkan untuk finalisasi dokumentasi
25 Oktober 2026; tanggal eksekusi run tidak dicantumkan dalam laporan pemilik. Agent tidak menjalankan
build, test, aplikasi, maupun akses basis data pada finalisasi ini; yang diperiksa agent hanya data Git
baca-saja pada bagian 11.1.

### 11.1 Baseline

| Field | Nilai |
| --- | --- |
| Baseline Integration yang dipakai untuk *rebase* | `52d7e1175c439d4c36a4e53608f630c5823604d8` — `Merge pull request #308 from DevBenari/sukmagp` |
| Commit `BE-PAT-MIG-002` yang diuji | `46edc6afd946666178d9681f5db103310daa2b22` — `feat(patient): implement RSMMC bulk migration worker`, author `devbenari`, tanggal author 23 Oktober 2026; induk langsung `52d7e117` |
| Isi commit | 25 berkas yang sama dengan `f0ee1c98`. Tidak menyentuh `Models/`, `Repositories/`, `Migrations/`, `appsettings*`, `.sln`, `RsmmcPilot*`, maupun `contracts/` (`git show --stat 46edc6af`) |
| Selisih terhadap `f0ee1c98` | Source dan test MIG-002 — tujuh berkas `RsmmcBulk*`/`RsmmcQrDestinationProbe`, `PatientController.cs`, dan seluruh `Tests/QuilvianSystemBackend.PatientManagementTests` — **identik** (`git diff --quiet f0ee1c98 46edc6af -- …`, exit `0`). Selisih lain berasal dari Integration `35951dbb` → `52d7e117` — antara lain `NewbornPatientRegistrationService.cs`, `PatientDtos.cs`, dan dua baris DI General Consent rawat jalan di `Program.cs`, di luar blok DI MIG-002 — serta dokumen finalisasi 24 Oktober 2026 |
| Status Git | Hanya lokal: **belum** push, PR, merge, maupun deploy |

### 11.2 Hasil validasi

| Pemeriksaan | Hasil |
| --- | --- |
| Build Release backend | `PASS` |
| Build Release `QuilvianSystemBackend.PatientManagementTests` | `PASS` |
| PatientManagement non-ProviderReal | `505`/`505` `PASS` |
| PostgreSQL 15 provider-real khusus MIG-002 (`MigrasiMassalPostgresSungguhanTests`) | `6`/`6` `PASS` |
| PostgreSQL 15 provider-real penuh (`Kategori=ProviderReal`) | `29`/`29` `PASS` — gagal `0`, dilewati `0`, ExitCode `0`, durasi 1 jam 34 menit |
| QBE `Strict` mode `GitRange` | `PASS` — `VIOLATION 0`, `REVIEW 0`, `INFO 0` |

**Jangan menjumlahkan angka di atas.** `6` termasuk di dalam `29`; suite non-provider dan provider-real
dihitung terpisah. Bukti pemilik untuk kandidat ini tidak mencantumkan run terfokus MIG-002 non-provider
maupun run membership tersendiri; keduanya termasuk dalam `505`/`505`. Jumlah berkas QBE tidak
dicantumkan untuk run ini.

`AUTOMATED TEST: dotnet test Tests/QuilvianSystemBackend.PatientManagementTests (non-ProviderReal) — PASS 505/505`
`AUTOMATED TEST PROVIDER-REAL: Kategori=ProviderReal — PASS 29/29 (MigrasiMassalPostgresSungguhanTests 6/6), ExitCode 0`

### 11.3 Percobaan provider-real yang gagal sebelum run lulus

Bukti pemilik mencatat satu percobaan provider-real sebelumnya yang mengalami **25 kegagalan** dengan galat
`Connection refused`, sesudah koneksi ke server bukti terputus. Pengujian ulang kemudian lulus **`29`/`29`**
(bagian 11.2).

Penyebab terputusnya koneksi **tidak** ditetapkan dalam laporan ini. Bukti yang tersedia hanya menunjukkan
bahwa koneksi ke server bukti tidak dapat dibuka selama percobaan itu; tidak ada diagnosis penyebab yang
terverifikasi, sehingga gangguan pada VM atau tunnel **tidak** dicatat sebagai fakta.

### 11.4 Lingkungan bukti dan pembersihan

| Hal | Keadaan |
| --- | --- |
| Server | PostgreSQL **15.19** sekali pakai; `max_locks_per_transaction = 2048` — syarat server uji saja, bukan anjuran untuk DEV/STAGING/PRODUCTION |
| Basis data `pat_proof_*` tersisa | **Tidak ada** |
| Kontainer bukti | Dihapus |
| SSH tunnel dan variabel environment bukti | Dibersihkan, sesuai bukti yang tersedia dari pemilik |
| DEV/STAGING/PRODUCTION | Tidak dipakai |

### 11.5 Keadaan task dan gerbang — 25 Oktober 2026

| Hal | Keadaan |
| --- | --- |
| Status task | 🟡 **IMPLEMENTED — VALIDATED LOCALLY** — bukan *deployed* dan bukan *production-ready* |
| Acceptance criteria | Tetap **47 dari 49** AC aktif terbukti. `AC-35` (eksekusi ulang Canary-A tanpa duplikat) dan `AC-36` (audit sesudah Canary-A) membutuhkan eksekusi aktual di Staging — **`NOT RUN`**. `AC-44`/`45` dipindah ke `BE-PAT-MIG-003` |
| DoD fase implementasi | Seluruhnya terpenuhi **kecuali** review source leader |
| Review leader, PR, merge, deploy | **Pending** |
| `PAT-GATE-002` | **`NOT RUN`** |
| Canary-A — 25 pasien `RSMMC-CANARY-A` v1 | **`NOT RUN`** |
| Migrasi aktual 704.506 pasien | **`NOT RUN`** |
| Otorisasi migrasi massal | **Tidak ada** |
| *Follow-up* terpisah | Warning `NU1902`/`NU1903` `SixLabors.ImageSharp` (bagian 10.4) tetap di luar cakupan MIG-002 |

Seluruh keputusan dan batasan migrasi yang disetujui tetap berlaku tanpa perubahan.
