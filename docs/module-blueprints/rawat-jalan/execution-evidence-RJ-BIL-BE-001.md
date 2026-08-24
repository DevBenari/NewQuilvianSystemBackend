# Bukti Eksekusi `RJ-BIL-BE-001`

| Field | Nilai |
|---|---|
| TASK_ID | `RJ-BIL-BE-001` |
| IMPLEMENTATION_ORIGIN | `PRE-EXISTING_STAGED_CHANGES` |
| CURRENT_EXECUTION_ACTION | `MIGRATION_APPLY_AND_TEST_EVIDENCE` |
| IMPLEMENTATION_STATUS | `COMPLETE` |
| SOURCE_FILES_CHANGED | Perubahan source task sudah ada pada working tree: `Areas/HealthServices/BillingManagement/Operational/**`, `Program.cs`, dan `Repositories/ApplicationDbContext.cs`. Tidak diduplikasi atau ditimpa pada eksekusi ini. |
| CONTRACT_CONFLICT | `NONE` — inspeksi source terhadap contract API/State/Validation `1.0.0` dan Permission `RJ-BIL-PERM-001@1.0.0`, diverifikasi runtime melalui `10` test yang lulus. |
| SOURCE_COMPILE_EVIDENCE | `PASS` |
| HOST_BUILD | `PASS` — build manual dari host di `C:\ProjectX\QuilvianV2\NewQuilvianSystemBackend` dengan `dotnet build`; restore dan compilation berhasil. |
| HOST_BUILD_ERROR_COUNT | `0` |
| HOST_BUILD_WARNING_COUNT | `125` |
| HOST_BUILD_OUTPUT | `bin/Debug/net9.0/QuilvianSystemBackend.dll` |
| CODEX_SANDBOX_BUILD | `BLOCKED_BY_WINDOWS_ACL` — sandbox tidak dapat menulis `obj/Debug/net9.0/rpswa.dswa.cache.json`; ini tidak membatalkan bukti compile host. |
| BUILD | `PASS` berdasarkan bukti build host; bukti build sandbox dicatat terpisah sebagai blocker environment. |
| ERROR_COUNT | `0` pada host build |
| WARNING_COUNT | `125` pada host build |
| TEST_PROJECT_AVAILABLE | `YES` — `Tests/QuilvianSystemBackend.BillingTests/`, xUnit `2.9.2`, terdaftar pada solution. |
| TEST_EVIDENCE | `PASS` — `10` test dijalankan, `10` lulus, `0` gagal. |
| TEST_SCOPE | Keempat acceptance criteria `RJ-BIL-BE-001`; bukan seluruh acceptance matrix modul Billing. |
| TEST_TEARDOWN_VERIFIED | `PASS` — tidak ada sisa data test pada database target. |
| PERMISSION_REVIEW | `PASS` — ketiga endpoint yang terimplementasi cocok dengan `RJ-BIL-PERM-001@1.0.0`. |
| AUDIT_REVIEW | `PASS` — endpoint recognize memanggil `LoggerService.AuditAsync` pada jalur sukses dan konflik versi; idempotency key disimpan sebagai hash. |
| MIGRATION_GENERATION_AUTHORITY | `GRANTED` |
| MIGRATION_REQUIRED | `YES` — model/configuration dan `DbSet` task ini memiliki dampak schema/index/concurrency. |
| MIGRATION_GENERATED | `YES` |
| MIGRATION_FILE | `Migrations/20260821033911_AddBillingOperationalBaseline.cs`, `Migrations/20260821033911_AddBillingOperationalBaseline.Designer.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs` |
| MIGRATION_STATIC_REVIEW | `PASS` |
| MODEL_SNAPSHOT_REVIEW | `PASS` |
| FOLIO_UNIQUENESS_REVIEW | `PASS` |
| DELIVERY_IDEMPOTENCY_REVIEW | `PASS` |
| FACT_REVISION_UNIQUENESS_REVIEW | `PASS` |
| PK_REVIEW | `PASS` |
| FK_REVIEW | `PASS` |
| DELETE_BEHAVIOR_REVIEW | `PASS` |
| DECIMAL_PRECISION_REVIEW | `PASS` |
| AUDIT_PERSISTENCE_REVIEW | `PASS` |
| FINANCIAL_HISTORY_IMMUTABILITY_REVIEW | `PASS` |
| DATABASE_APPLY_AUTHORITY | `GRANTED` — otorisasi terpisah dan menyusul, diberikan pengguna pada `2026-08-21` dengan menyebut database target secara eksplisit. Otorisasi awal `RJ-BIL-BE-001` melarang apply; lihat bagian eksekusi database apply. |
| DATABASE_APPLY_READY | `YES` (sudah dipakai) |
| MIGRATION_APPLIED | `YES` |
| DATABASE_CHANGED | `YES` — `QuilvianNewDevTim01` pada `160.22.250.77:5432`. |
| DATABASE_APPLY_VERIFIED | `PASS` — `dotnet ef migrations list` pasca-apply: `86` migration terdaftar, `0` pending. |
| DATABASE_APPLY_SCOPE | `1` migration; tidak ada migration milik branch lain yang ikut diterapkan. |
| FRONTEND_CHANGED | `NO` |
| RJ-BIL-DEP-009 | `INACTIVE / OUT_OF_SCOPE` |
| RJ-BIL-DEP-009_CHANGED | `NO` |
| GIT_DIFF_CHECK | `PASS` |

## Cakupan dan governance

Task berada pada Area `HealthServices`, Module `BillingManagement`, Submodule `Operational`,
dengan owner/prefix `Billing`/`Bil` berstatus `ACTIVE`. Klasifikasi source adalah `TOUCHED
LEGACY`/new operational slice pada working tree yang sudah tersedia. QBE yang relevan:
`QBE-ENT-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`,
`QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-CODE-004`,
dan `QBE-AUD-001`.

Inspeksi menunjukkan adanya authorization metadata pada controller, validasi server-side,
actor dari authenticated claims, idempotency key dan fingerprint, unique/index constraint,
optimistic concurrency melalui `Version`, transaksi serializable, retry untuk konflik yang
dapat dipulihkan, serta audit logging. Seluruhnya kemudian dibuktikan melalui permission
review dan `10` test yang lulus.

## Perubahan yang sudah ada sebelum eksekusi ini

Working tree pada awal eksekusi sudah memuat source task dalam status staged. Perubahan tersebut
dipertahankan sebagai pekerjaan pengguna dan tidak diubah secara duplikatif. Tidak ada source
frontend, database, deployment, commit, atau publish yang dilakukan.

## Otorisasi dan tindakan generasi migration

Otorisasi `MIGRATION_GENERATION_AUTHORITY = GRANTED` diberikan untuk generasi migration saja.

Pada awal eksekusi ini, artefak migration `20260821033911_AddBillingOperationalBaseline` sudah
ada pada working tree dalam status untracked, dihasilkan dari host pada `2026-08-21 03:39:11`,
disertai modifikasi `ApplicationDbContextModelSnapshot.cs` yang belum di-stage.

Verifikasi EF dijalankan untuk menentukan apakah generasi ulang diperlukan:

```
dotnet ef migrations has-pending-model-changes --no-build \
  --project QuilvianSystemBackend.csproj \
  --startup-project QuilvianSystemBackend.csproj \
  --context ApplicationDbContext
```

Hasil: `No changes have been made to the model since the last migration.` dengan exit code `0`.

Karena model sudah sepenuhnya terwakili oleh migration yang ada, generasi ulang tidak dilakukan;
generasi ulang hanya akan menghasilkan migration kosong dan menambah artefak tanpa nilai. Tindakan
eksekusi ini adalah verifikasi kelengkapan dan review statis atas artefak migration tersebut.
Tidak ada artefak migration yang ditulis ulang, dihapus, atau di-rename.

## Identitas EF migration

| Field | Nilai |
|---|---|
| DB_CONTEXT | `QuilvianSystemBackend.Repositories.ApplicationDbContext` |
| STARTUP_PROJECT | `QuilvianSystemBackend.csproj` |
| MIGRATION_PROJECT | `QuilvianSystemBackend.csproj` (solusi single-project) |
| MIGRATION_DIRECTORY | `Migrations/` |
| MIGRATION_NAME | `AddBillingOperationalBaseline` |
| MIGRATION_ID | `20260821033911_AddBillingOperationalBaseline` |
| MIGRATION_SEBELUMNYA | `20260818084734_AddTriageSlaBreachMarker` |
| EF_PROVIDER | `Npgsql.EntityFrameworkCore.PostgreSQL` `9.0.4` |
| EF_CORE_VERSION | `9.0.18` (tooling `dotnet ef` `9.0.18`, SDK `9.0.316`) |
| TARGET_FRAMEWORK | `net9.0` |
| KONVENSI_NAMA | PascalCase deskriptif, konsisten dengan `AddTriageSlaBreachMarker` dan `AddAutomaticApplicationVersioning` |
| REGISTRASI_CONFIGURATION | `builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly)` pada `Repositories/ApplicationDbContext.cs:624` |
| DBSET | `BilFolios`, `BilChargeLines`, `BilChargeComponents`, `BilProcessingEffects` (`Repositories/ApplicationDbContext.cs:534-537`) |

## Review statis migration

`Up()` hanya berisi empat `CreateTable` dan tujuh `CreateIndex`. `Down()` hanya berisi empat
`DropTable` atas tabel yang dibuat migration ini sendiri.

| Aspek | Hasil | Bukti |
|---|---|---|
| Folio persistence | `PASS` | Tabel `public."BilFolio"` dengan `Id`, `EncounterId`, `Status`, `Version`, `IsActive` |
| Charge persistence | `PASS` | Tabel `public."BilChargeLine"` dengan identitas source/fact dan nilai finansial |
| Charge Component persistence | `PASS` | Tabel `public."BilChargeComponent"` dengan `ComponentKey`, snapshot `jsonb`, `CalculationVersion` |
| Processing Effect persistence | `PASS` | Tabel `public."BilProcessingEffect"` dengan idempotency, fingerprint, outcome, korelasi |
| Satu Folio kanonik per Encounter | `PASS` | `IX_BilFolio_EncounterId` `unique` dengan filter `"IsDelete" = false` |
| PK | `PASS` | `PK_BilFolio`, `PK_BilChargeLine`, `PK_BilChargeComponent`, `PK_BilProcessingEffect`, seluruhnya `uuid` single-column |
| FK | `PASS` | Lima FK, seluruhnya `ReferentialAction.Restrict` |
| Required/nullable | `PASS` | Identitas dan klasifikasi `NOT NULL`; nilai finansial dan diagnostik `NULL`-able sesuai lifecycle `Received` sampai `Recognized` |
| Delete behavior | `PASS` | Tidak ada `CASCADE` maupun `SET NULL` pada FK Billing |
| Presisi desimal | `PASS` | Lihat bagian presisi desimal |
| String length | `PASS` | `SourceContext` 50; `EffectType`, `Consumer`, `OperationType`, `ComponentKey`, `ReviewReasonCode`, `ErrorCode` 100; `IdempotencyKey` 128; `RequestFingerprint` 64; `Unit` 50; `Currency` 3; `ErrorMessage` 1000 |
| Audit fields | `PASS` | Keempat tabel memuat seluruh kolom `IdentityModel` |
| Concurrency fields | `PASS` (app-level) | `Version` `IsConcurrencyToken()` pada `BilFolio` dan `BilChargeLine`; token EF tidak menghasilkan artefak DDL, penegakan berada pada predikat `UPDATE` |
| Delivery idempotency uniqueness | `PASS` | `IX_BilProcessingEffect_Consumer_OperationType_IdempotencyKey` `unique` |
| Fact-revision processing uniqueness | `PASS` | `IX_BilProcessingEffect_SourceContext_MilestoneFactId_Milestone~` `unique` atas `SourceContext`, `MilestoneFactId`, `MilestoneFactVersion`, `EffectType` |
| Stale-version persistence | `PASS` | `MilestoneFactVersion` `NOT NULL` pada `BilChargeLine` dan `BilProcessingEffect`; penolakan versi lama pada `BillingFolioService.cs:177-180` membaca state tersimpan, tidak menimpanya |
| Newer-version PendingFinancialReview | `PASS` | `CalculationStatus` dan `ReviewReasonCode` `character varying(100)` tersedia; ditulis pada `BillingFolioService.cs:332-333` |
| Immutable historical financial data | `PASS` | Tidak ada `ON DELETE CASCADE`; koreksi memakai status `Superseded`, `Voided`, `Reversed`, bukan penghapusan baris |

## Unique constraint kritikal

| Invariant | Perlindungan level database | Status |
|---|---|---|
| Folio kanonik ganda untuk Encounter yang sama | `IX_BilFolio_EncounterId` `unique`, filter `"IsDelete" = false` | `PASS` |
| Delivery processing ganda | `IX_BilProcessingEffect_Consumer_OperationType_IdempotencyKey` `unique` | `PASS` |
| Pemrosesan ganda atas revisi fakta yang sama | `IX_BilProcessingEffect_SourceContext_MilestoneFactId_MilestoneFactVersion_EffectType` `unique` | `PASS` |

Kedua identitas terpisah. Identitas idempotency delivery adalah `Consumer` + `OperationType` +
`IdempotencyKey`; identitas revisi fakta adalah `SourceContext` + `MilestoneFactId` +
`MilestoneFactVersion` + `EffectType`. Tidak ada kolom yang dipakai bersama antara kedua index
tersebut.

Identitas Charge tidak berubah. `IX_BilChargeLine_SourceContext_SourceAggregateId_SourceItemId_~`
`unique` atas `SourceContext`, `SourceAggregateId`, `SourceItemId`, `MilestoneFactId`, `EffectType`
dengan anotasi `Npgsql:NullsDistinct = false`, sehingga `SourceItemId` bernilai `NULL` tetap
diperlakukan sebagai nilai yang sama untuk keperluan uniqueness. `MilestoneFactVersion` bukan
bagian identitas Charge; koreksi revisi memperbarui Charge kanonik yang sama, bukan membuat baris
duplikat.

## Delete / history safety

Seluruh FK Billing yang dihasilkan migration:

| FK | Kolom | Referensi | Delete behavior |
|---|---|---|---|
| `FK_BilFolio_TrxPatientEncounter_EncounterId` | `BilFolio.EncounterId` | `public."TrxPatientEncounter"."Id"` | `Restrict` |
| `FK_BilChargeLine_BilFolio_FolioId` | `BilChargeLine.FolioId` | `public."BilFolio"."Id"` | `Restrict` |
| `FK_BilChargeComponent_BilChargeLine_ChargeLineId` | `BilChargeComponent.ChargeLineId` | `public."BilChargeLine"."Id"` | `Restrict` |
| `FK_BilProcessingEffect_BilFolio_FolioId` | `BilProcessingEffect.FolioId` | `public."BilFolio"."Id"` | `Restrict` |
| `FK_BilProcessingEffect_BilChargeLine_ChargeLineId` | `BilProcessingEffect.ChargeLineId` | `public."BilChargeLine"."Id"` | `Restrict` |

Tidak ditemukan `CASCADE` maupun `SET NULL`. Penghapusan `Encounter`, `Folio`, atau `ChargeLine`
akan ditolak database selama masih ada Charge, Charge Component, atau Processing Effect yang
merujuknya. `MIGRATION_PERSISTENCE_CONFLICT` tidak terjadi.

## Presisi desimal dan keamanan nilai uang

| Kolom | Tipe yang dihasilkan | Klasifikasi |
|---|---|---|
| `BilChargeLine.GrossAmount` | `numeric(18,2)`, nullable | Uang |
| `BilChargeLine.EligibleAmount` | `numeric(18,2)`, nullable | Uang |
| `BilChargeComponent.CalculatedAmount` | `numeric(18,2)`, nullable | Uang |
| `BilChargeComponent.Quantity` | `numeric(18,6)`, nullable | Kuantitas |
| `BilChargeLine.Currency` | `character varying(3)`, nullable | Denominasi |

Tidak ada kolom bertipe `double precision`, `real`, `float`, atau `money` pada keempat tabel
Billing. Seluruh nilai kanonik uang memakai `numeric` dengan precision dan scale eksplisit.

## Migration safety

| Operasi berisiko | Ditemukan |
|---|---|
| `DropTable` pada `Up()` | Tidak ada |
| `DropColumn` | Tidak ada |
| `AlterColumn` destruktif | Tidak ada |
| Rename kolom atau tabel | Tidak ada |
| Penurunan precision | Tidak ada |
| Konversi nullable ke required | Tidak ada |
| Rekreasi tabel tak terduga | Tidak ada |
| `migrationBuilder.Sql(...)` mentah | Tidak ada |
| Perubahan schema tak terkait | Tidak ada |

`DropTable` hanya muncul pada `Down()` dan hanya atas empat tabel yang dibuat migration ini.
Migration bersifat aditif murni terhadap schema yang sudah ada.

## ModelSnapshot

`BuildTargetModel` pada `20260821033911_AddBillingOperationalBaseline.Designer.cs` dibandingkan
baris demi baris dengan `BuildModel` pada `ApplicationDbContextModelSnapshot.cs`: `90361` baris,
`0` baris berbeda — identik.

`git diff --numstat Migrations/ApplicationDbContextModelSnapshot.cs` = `409` penambahan, `0`
penghapusan. Entity yang tersentuh diff hanya `BilChargeComponent`, `BilChargeLine`, `BilFolio`,
dan `BilProcessingEffect`. Tidak ada entity lain yang berubah.

Snapshot memuat `IsConcurrencyToken()` pada `Version` (`BilFolio`, `BilChargeLine`), filter
`"IsDelete" = false` pada index Encounter, `AreNullsDistinct(false)` pada identitas Charge,
`HasPrecision(18, 2)` dan `HasPrecision(18, 6)`, seluruh `IsRequired()` dan `HasMaxLength(...)`,
serta `OnDelete(DeleteBehavior.Restrict)` pada seluruh relasi Billing. Migration dan snapshot
konsisten.

## Validasi statis

| Pemeriksaan | Hasil |
|---|---|
| `dotnet ef migrations has-pending-model-changes --no-build` | `No changes have been made to the model since the last migration.`, exit `0` |
| `git diff --check` (unstaged) | `PASS`, exit `0`; hanya peringatan normalisasi `LF`/`CRLF` pada tiga dokumen blueprint |
| `git diff --check` atas `Migrations/ApplicationDbContextModelSnapshot.cs` | `PASS`, exit `0` |
| `git diff --check --no-index` atas kedua file migration untracked | `PASS`, tanpa temuan whitespace |

Catatan: `git diff --check --cached` melaporkan `new blank line at EOF` pada sebelas dokumen
blueprint yang sudah di-stage sebelum eksekusi ini. Temuan tersebut berada di luar artefak
migration dan di luar cakupan `RJ-BIL-BE-001`; tidak diubah pada eksekusi ini.

Bukti build host yang sudah terverifikasi dipertahankan apa adanya. Build tidak dijalankan ulang
untuk menghasilkan bukti baru; perintah EF memakai `--no-build` di atas output build host yang
masih fresh (`bin/Debug/net9.0/QuilvianSystemBackend.dll`, tidak ada file source yang lebih baru).

Baris `HostAbortedException` pada log EF adalah perilaku normal tooling `dotnet ef` saat
menghentikan host setelah service provider terbentuk, bukan kegagalan aplikasi.

## Eksekusi database apply

### Jejak otorisasi

Otorisasi awal `RJ-BIL-BE-001` secara eksplisit menempatkan `dotnet ef database update`,
`applying migration to any database`, dan `production/shared database access` pada daftar
**NOT AUTHORIZED**, serta menetapkan `database access would be required` sebagai **STOP
CONDITION**. Atas dasar itu eksekusi review statis berhenti tanpa apply.

Otorisasi apply diberikan **menyusul dan terpisah** oleh pengguna pada `2026-08-21`, setelah
target database dilaporkan dan dikonfirmasi secara eksplisit dengan menyebut nama database.
Otorisasi awal `RJ-BIL-BE-001` tidak mencakup apply; jejak ini dicatat terpisah agar tidak
terbaca seolah apply berada dalam cakupan otorisasi semula.

### Target

| Field | Nilai |
|---|---|
| DATABASE | `QuilvianNewDevTim01` |
| HOST | `160.22.250.77:5432` |
| KLASIFIKASI | Database dev tim bersama, bukan lokal dan bukan production |
| SUMBER KONFIGURASI | `appsettings.Development.json` → `ConnectionStrings:DefaultConnection` |
| OVERRIDE | Tidak ada override pada `appsettings.json` maupun environment variable |

### Pre-flight

Sebelum apply, state migration pada database target dibaca read-only:

```
dotnet ef migrations list --no-build --project QuilvianSystemBackend.csproj \
  --startup-project QuilvianSystemBackend.csproj --context ApplicationDbContext
```

| Pemeriksaan | Hasil |
|---|---|
| Koneksi | Berhasil, exit `0` |
| Migration terdaftar | `86` |
| Pending sebelum apply | `1` — hanya `20260821033911_AddBillingOperationalBaseline` |
| Migration branch lain yang pending | Nihil |

Pemeriksaan ini diperlukan karena `dotnet ef database update` menerapkan seluruh migration
pending sampai target. Branch `sukmagp` baru saja di-merge dari beberapa branch tim, sehingga
kemungkinan tertinggalnya migration milik anggota tim lain harus dibuktikan nihil lebih dulu.
Hasilnya nihil, sehingga apply tidak melampaui cakupan yang diotorisasi.

### Perintah apply

Target migration dipatok eksplisit agar apply tidak dapat melangkah melewati satu migration ini:

```
dotnet ef database update 20260821033911_AddBillingOperationalBaseline --no-build \
  --project QuilvianSystemBackend.csproj \
  --startup-project QuilvianSystemBackend.csproj \
  --context ApplicationDbContext
```

Hasil: `Done.` dengan exit code `0`, tanpa error, tanpa warning.

### Verifikasi pasca-apply

```
dotnet ef migrations list --no-build ... --context ApplicationDbContext
```

| Pemeriksaan | Hasil |
|---|---|
| Exit code | `0` |
| Migration terdaftar | `86` |
| Pending setelah apply | `0` |
| `20260821033911_AddBillingOperationalBaseline` | Tidak lagi berlabel `(Pending)` — tercatat pada `__EFMigrationsHistory` |

### Artefak SQL pendamping

Script idempotent dihasilkan sebagai artefak review sebelum apply, tanpa membuka koneksi
database:

```
dotnet ef migrations script 20260818084734_AddTriageSlaBreachMarker \
  20260821033911_AddBillingOperationalBaseline --idempotent --no-build ...
```

Script berisi `198` baris, dibungkus satu `START TRANSACTION; … COMMIT;`, dengan `13` blok
`DO $EF$` dengan guard `IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" …)`. Isinya `4`
`CREATE TABLE`, `5` `CREATE UNIQUE INDEX`, `3` `CREATE INDEX`, tanpa satu pun `DROP`,
`ALTER TABLE`, atau `TRUNCATE`. Script tidak dieksekusi; apply dilakukan melalui
`dotnet ef database update` karena `psql` tidak tersedia pada host.

## Bukti test

### Keputusan menarik scope test ke depan

DoD `RJ-BIL-BE-001` menuntut test evidence, sementara pembuatan test project di-scope ke
`RJ-BIL-BE-009` yang berada di ujung rantai dependency `BE-001 → … → BE-008 → BE-009`. Dengan
urutan roadmap apa adanya, `RJ-BIL-BE-001` tidak akan pernah dapat ditutup sampai seluruh
`BE-002` sampai `BE-008` selesai.

Pemilik backend memutuskan pada `2026-08-21` untuk menarik sebagian scope `RJ-BIL-BE-009` ke
depan, terbatas pada keempat acceptance criteria `RJ-BIL-BE-001`. Sisa scope `RJ-BIL-BE-009`
tetap berada pada task aslinya.

### Test project

| Field | Nilai |
|---|---|
| LOKASI | `Tests/QuilvianSystemBackend.BillingTests/` |
| FRAMEWORK | xUnit `2.9.2`, `Microsoft.NET.Test.Sdk` `17.12.0` |
| SOLUTION | Terdaftar pada `QuilvianSystemBackend.sln` |
| BUILD | `PASS` — `0` error, `1` warning `MSB3277` (konflik versi `Microsoft.Extensions.DependencyModel` antara test SDK dan project Web; tidak menghalangi eksekusi) |
| PERUBAHAN PROJECT UTAMA | `QuilvianSystemBackend.csproj` ditambah `<Compile Remove="Tests\**" />` dan `<Content Remove="Tests\**" />`, mengikuti pola exclude yang sudah dipakai project tersebut. Tanpa ini, glob `**/*.cs` project utama ikut mengompilasi file test karena `csproj` berada di root repository. |

Tidak ada model, controller, enum, atau grup `[Tags(...)]` baru yang dibuat, sehingga tidak ada
kavling nama atau endpoint yang diambil.

Susunan folder:

```text
Tests/QuilvianSystemBackend.BillingTests/
├── QuilvianSystemBackend.BillingTests.csproj
├── README.md
├── Infrastructure/
│   ├── BillingTestDatabaseFixture.cs   # resolusi connection string, migration, seed, teardown
│   └── EncounterSeed.cs                # identitas prasyarat satu test
└── Operational/
    └── BillingFolioServiceTests.cs     # test submodule Billing Operational
```

`Infrastructure/` memisahkan perkakas bersama dari test. Folder di sebelahnya mengikuti nama
submodule pada `Areas/HealthServices/BillingManagement/`, sehingga test dapat ditemukan dari
lokasi source-nya dan penambahan test `MasterData` pada task berikutnya tinggal menjadi folder
sejajar. Namespace mengikuti struktur folder: `QuilvianSystemBackend.BillingTests.Infrastructure`
dan `QuilvianSystemBackend.BillingTests.Operational`.

`README.md` mendokumentasikan cara menjalankan, urutan resolusi connection string, perilaku
pengaman per nama database, urutan teardown, alasan memakai PostgreSQL sungguhan alih-alih
provider InMemory, cara menambah test baru, dan daftar cakupan yang masih menjadi bagian
`RJ-BIL-BE-009`.

### Pilihan level pengujian

Keempat acceptance criteria adalah invariant persistence, concurrency, dan validasi sumber —
bukan invariant transport. Pengujian dilakukan pada level service terhadap PostgreSQL sungguhan.

Provider InMemory **tidak** dipakai karena tidak menegakkan unique index; test folio-uniqueness
akan lulus secara semu dan justru membuktikan kebalikan dari yang diklaim.

Pengujian lewat HTTP tidak dipilih karena controller `[Authorize]` dengan JWT bearer dan
`Program.cs` memakai top-level statements tanpa `public partial class Program`, sehingga
`WebApplicationFactory<Program>` menuntut perubahan pada `Program.cs` yang dipakai seluruh tim.

### Hasil

| Test | Acceptance criteria | Hasil |
|---|---|---|
| `DuaMilestoneBerbedaPadaEncounterSama_HanyaMenghasilkanSatuFolio` | Folio unik per encounter | `PASS` |
| `FolioKeduaUntukEncounterSama_DitolakUniqueIndexDatabase` | Folio unik per encounter | `PASS` |
| `IdempotencyKeySamaDenganPayloadSama_MenghasilkanReplayTanpaChargeGanda` | Duplicate key menghasilkan replay | `PASS` |
| `VersiLamaSetelahVersiBaruApplied_DitolakDenganVersionConflict` | Stale version ditolak | `PASS` |
| `KonteksKlinis_TidakDapatMembuatMutasiFinansial` — `Pharmacy` | Tidak ada clinical financial mutation | `PASS` |
| `KonteksKlinis_TidakDapatMembuatMutasiFinansial` — `Prescription` | Tidak ada clinical financial mutation | `PASS` |
| `KonteksKlinis_TidakDapatMembuatMutasiFinansial` — `Procedure` | Tidak ada clinical financial mutation | `PASS` |
| `KonteksKlinis_TidakDapatMembuatMutasiFinansial` — `Laboratory` | Tidak ada clinical financial mutation | `PASS` |
| `KonteksKlinis_TidakDapatMembuatMutasiFinansial` — `Radiology` | Tidak ada clinical financial mutation | `PASS` |
| `EffectTypeDiluarKontrak_TidakDapatMembuatMutasiFinansial` | Tidak ada clinical financial mutation | `PASS` |

`Total tests: 10`, `Passed: 10`, `Failed: 0`, exit code `0`.

Yang dibuktikan masing-masing:

- dua milestone berbeda pada encounter yang sama menghasilkan `1` folio dan `2` charge line;
- penyisipan folio kanonik kedua langsung ke database ditolak `PostgresException` `23505` pada
  constraint `IX_BilFolio_EncounterId` — jadi invariant ditegakkan database, bukan hanya logika
  aplikasi;
- idempotency key yang sama dengan payload identik mengembalikan `IsReplay = true` dengan
  `ProcessingEffectId` dan `ChargeLineId` yang sama, serta tetap `1` processing effect dan
  `1` charge line;
- versi `1` yang datang setelah versi `2` applied ditolak `BIL_VERSION_CONFLICT` dengan
  `AppliedVersion = 2`, dan histori versi `2` tetap utuh pada processing effect maupun charge line;
- lima konteks klinis — `Pharmacy`, `Prescription`, `Procedure`, `Laboratory`, `Radiology` —
  serta `EffectType` di luar kontrak ditolak `BIL_SOURCE_INVALID`, dan penolakan terjadi
  **sebelum** jejak finansial tertulis: nol folio, nol charge line, dan nol processing effect
  pada encounter tersebut. Ini menegakkan invariant `#1` decision log, yaitu order klinis tidak
  otomatis menjadi final charge.

## Permission dan audit review

Kolom Verifikasi `RJ-BIL-BE-001` menuntut permission review. Hasil terhadap
`RJ-BIL-PERM-001@1.0.0`:

| Endpoint | Kontrak | Implementasi | Hasil |
|---|---|---|---|
| `GET /by-encounter/{encounterId}` | `BillingFolio` / `Read` | `[AccessPermission("BillingFolio", "Read")]` | `MATCH` |
| `GET /{folioId}` | `BillingFolio` / `Read` | `[AccessPermission("BillingFolio", "Read")]` | `MATCH` |
| `POST /internal/milestones/recognize` | `BillingMilestone` / `RecognizeInternal` | `[AccessPermission("BillingMilestone", "RecognizeInternal")]` | `MATCH` |

Controller memakai `[Authorize]` pada level class. Enam baris lain pada matrix — allocations,
financial-actions, close, reopen, execute, resolve — adalah endpoint task berikutnya dan berada
di luar cakupan `RJ-BIL-BE-001`.

Matrix mewajibkan audit logger pada endpoint recognize. Terpenuhi melalui
`LoggerService.AuditAsync` pada dua jalur:

| Jalur | Action | Isi yang dicatat |
|---|---|---|
| Sukses | `BillingMilestone.RecognizeInternal` | actor, `ProcessingEffectId`, `FolioId`, `ChargeLineId`, `IsReplay`, `Outcome`, `CalculationStatus`, identitas source, `CorrelationId` |
| Konflik versi | `BillingMilestone.VersionConflict` | actor, incoming versus applied version, identitas source, `DetectedAt`, outcome |

Sesuai ketentuan matrix, idempotency key dicatat sebagai `HashReference(...)`, bukan nilai mentah.

### Target database test dan teardown

Test dijalankan terhadap `QuilvianNewDevTim01`, atas keputusan pemilik backend untuk memakai
database yang sudah ada alih-alih membuat database test baru. Connection string diambil fixture
dari `ConnectionStrings:DefaultConnection` pada `appsettings.Development.json` ketika
environment variable `QUILVIAN_BILLING_TEST_DB` kosong, sehingga tidak ada kredensial yang perlu
disalin ke environment variable atau perintah shell.

Fixture menolak database yang namanya mengandung `prod` atau `production` tanpa mekanisme
override, dan mencetak peringatan ke output test ketika targetnya database dev bersama.

Teardown menghapus seluruh baris yang dibuat setiap test, urut dari anak ke induk mengikuti
`DeleteBehavior.Restrict`: processing effect, charge component, charge line, folio, encounter,
pasien, unit layanan, lalu user.

Verifikasi sisa data setelah eksekusi:

| Yang dihitung | Sisa |
|---|---|
| `BilFolio` (seluruh tabel) | `0` |
| `BilChargeLine` `SourceContext = InternalTest` | `0` |
| `BilChargeComponent` (seluruh tabel) | `0` |
| `BilProcessingEffect` `SourceContext = InternalTest` | `0` |
| `ApplicationUser` `UserCode` berawalan `TST` | `0` |
| `MstPatient` `PatientCode` berawalan `PC` | `0` |

Encounter dan unit layanan tidak dapat dibedakan lewat prefix karena data tim memakai awalan
`ENC` dan `SU` yang sama. Keduanya tetap terbukti terhapus melalui dua alasan:

1. penghapusan user adalah langkah **terakhir** teardown dan hasilnya `0`; sebuah rantai yang
   langkah terakhirnya selesai berarti seluruh langkah sebelumnya juga selesai, karena
   `ExecuteDeleteAsync` yang gagal akan melempar exception dan menggagalkan test;
2. `MstPatient` adalah FK wajib ber-`Restrict` dari encounter, sehingga pasien mustahil terhapus
   selama masih ada encounter yang menunjuknya; pasien `0` berarti encounter test sudah lebih
   dulu terhapus.

Database target ditinggalkan dalam keadaan seperti sebelum test dijalankan.

## Status akhir

`MIGRATION_GENERATED = YES`, `MIGRATION_STATIC_REVIEW = PASS`, `MIGRATION_APPLIED = YES`,
`DATABASE_CHANGED = YES` pada `QuilvianNewDevTim01`, `DATABASE_APPLY_VERIFIED = PASS`,
`TEST_PROJECT_AVAILABLE = YES`, `TEST_EVIDENCE = PASS`.

`RJ-BIL-BE-001` dinyatakan `COMPLETE`.

Seluruh butir DoD terpenuhi: source evidence, build evidence, test evidence, migration artifact
reviewed, dan tidak ada database apply tanpa otorisasi. Keempat item kolom Verifikasi juga
terpenuhi: build backend, targeted integration test, migration review, dan permission review.

Keempat acceptance criteria terbukti:

| Acceptance criteria | Bukti |
|---|---|
| Folio unik per encounter | `2` test, termasuk penolakan level database `23505` |
| Duplicate key menghasilkan replay | `1` test |
| Stale version ditolak | `1` test |
| Tidak ada clinical financial mutation | `6` test |

Batas yang melekat pada status ini: bukti test menutup **acceptance criteria `RJ-BIL-BE-001`**,
bukan seluruh acceptance matrix modul Billing. Skenario seperti
`BIL_IDEMPOTENCY_CONFLICT`, outcome unknown, partial component, multi-payer allocation,
financial correction, maker-checker, dan folio close belum diuji dan tetap menjadi cakupan
`RJ-BIL-BE-009`. `COMPLETE` pada `RJ-BIL-BE-001` tidak berarti modul Billing sudah terverifikasi
menyeluruh.

`RJ-BIL-BE-002` tidak dimulai. `RJ-BIL-DEP-009` tetap `INACTIVE / OUT_OF_SCOPE`. Tidak ada seed
execution, perubahan frontend, deployment, atau perubahan source aplikasi Billing yang dijalankan
pada eksekusi ini. Commit `fe6d15c` beserta push ke `origin/sukmagp` dilakukan oleh pengguna.
Perubahan setelah commit tersebut — dokumen ini, test project, `QuilvianSystemBackend.csproj`,
dan `QuilvianSystemBackend.sln` — belum di-commit.
