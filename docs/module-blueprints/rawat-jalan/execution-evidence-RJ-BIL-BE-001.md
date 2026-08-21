# Bukti Eksekusi `RJ-BIL-BE-001`

| Field | Nilai |
|---|---|
| TASK_ID | `RJ-BIL-BE-001` |
| IMPLEMENTATION_ORIGIN | `PRE-EXISTING_STAGED_CHANGES` |
| CURRENT_EXECUTION_ACTION | `MIGRATION_GENERATION_AND_STATIC_REVIEW` |
| IMPLEMENTATION_STATUS | `IMPLEMENTED_AWAITING_TEST_EVIDENCE` |
| SOURCE_FILES_CHANGED | Perubahan source task sudah ada pada working tree: `Areas/HealthServices/BillingManagement/Operational/**`, `Program.cs`, dan `Repositories/ApplicationDbContext.cs`. Tidak diduplikasi atau ditimpa pada eksekusi ini. |
| CONTRACT_CONFLICT | `NONE` teridentifikasi dari inspeksi source terhadap contract API/State/Validation `1.0.0`; verifikasi runtime tetap menunggu build/test. |
| SOURCE_COMPILE_EVIDENCE | `PASS` |
| HOST_BUILD | `PASS` — build manual dari host di `C:\ProjectX\QuilvianV2\NewQuilvianSystemBackend` dengan `dotnet build`; restore dan compilation berhasil. |
| HOST_BUILD_ERROR_COUNT | `0` |
| HOST_BUILD_WARNING_COUNT | `125` |
| HOST_BUILD_OUTPUT | `bin/Debug/net9.0/QuilvianSystemBackend.dll` |
| CODEX_SANDBOX_BUILD | `BLOCKED_BY_WINDOWS_ACL` — sandbox tidak dapat menulis `obj/Debug/net9.0/rpswa.dswa.cache.json`; ini tidak membatalkan bukti compile host. |
| BUILD | `PASS` berdasarkan bukti build host; bukti build sandbox dicatat terpisah sebagai blocker environment. |
| ERROR_COUNT | `0` pada host build |
| WARNING_COUNT | `125` pada host build |
| TEST_PROJECT_AVAILABLE | `NO` |
| TEST_EVIDENCE | `PENDING` — tidak ada test project yang terdeteksi pada inspeksi solution; acceptance runtime belum dapat dibuktikan. |
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
| MIGRATION_APPLIED | `NO` |
| DATABASE_CHANGED | `NO` — tidak ada database mutation atau database apply yang dijalankan. |
| DATABASE_APPLY_READY | `YES` |
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
dapat dipulihkan, serta audit logging. Bukti build/test belum cukup untuk menandai task
`COMPLETE`.

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

## Status akhir

`MIGRATION_GENERATED = YES`, `MIGRATION_STATIC_REVIEW = PASS`, `DATABASE_APPLY_READY = YES`,
`MIGRATION_APPLIED = NO`, `DATABASE_CHANGED = NO`.

`RJ-BIL-BE-001` tetap `IMPLEMENTED_AWAITING_TEST_EVIDENCE`. Task tidak ditandai `COMPLETE`
karena `TEST_PROJECT_AVAILABLE = NO` dan `TEST_EVIDENCE = PENDING`.

`RJ-BIL-BE-002` tidak dimulai. `RJ-BIL-DEP-009` tetap `INACTIVE / OUT_OF_SCOPE`. Tidak ada
`dotnet ef database update`, database apply, mutasi database, seed, perubahan frontend,
deployment, commit, atau push yang dijalankan.

Perintah apply berikut sengaja **tidak** dijalankan dan memerlukan otorisasi terpisah:

```
dotnet ef database update --project QuilvianSystemBackend.csproj \
  --startup-project QuilvianSystemBackend.csproj \
  --context ApplicationDbContext
```
