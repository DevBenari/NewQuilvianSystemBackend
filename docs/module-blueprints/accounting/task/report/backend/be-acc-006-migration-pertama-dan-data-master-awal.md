# BE-ACC-006 — Migration pertama dan data master awal

- **TASK ID:** `BE-ACC-006` — Migration pertama dan data master awal
- **TASK TYPE:** Implementasi backend, seeder data master + verifikasi migration
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** **6** — repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (4–8), logika bisnis 0, kontrak API 0, database 2 (dinilai konservatif: task ini memeriksa migration walau tidak mengubah schema), keamanan/auth 1 (`actorUserId` sebagai jejak audit, bukan inti), UI/workflow 0
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `Areas/Corporate/AccountingManagement/`, `Tests/QuilvianSystemBackend.Tests/AccountingManagement/`, `docs/module-blueprints/accounting/`
- **VISUAL REFERENCE:** `NOT REQUIRED` — nol perubahan UI
- **BLUEPRINT STATUS/EVIDENCE:** `NOT APPLICABLE` — bukan `MODULE BLUEPRINT MODE`
- **STALE EVIDENCE / BLOCKED PHASES:** `NOT APPLICABLE` — bukan `MODULE BLUEPRINT MODE`
- **INTERRUPTIONS:** `NONE`
- **WARNINGS:** 203 warning build, **seluruhnya pre-existing dan milik modul lain**; nol berasal dari kedua berkas baru. Rinciannya bagian 6
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 6 `APPROVED`, roadmap revisi 2 `APPROVED`, `decision_revision` 1.3
- **HEAD saat mulai:** `f40177a` pada branch `rizkiG`, working tree bersih

### FILES INSPECTED

Lebih dari 20 berkas. Yang menentukan keputusan:

| Berkas | Untuk apa |
|---|---|
| `blueprint-manifest.md`, `MODULE-STATUS.md`, `roadmap/backend-roadmap.md` | Revision, hash, SHA, definisi dan acceptance `BE-ACC-006` |
| `02-backend-architecture.md` bagian 6, 8, 9 | Isi empat baris master; larangan seeder di `Program.cs`; kewajiban menghitung operasi migration |
| `evidence/04-migration-coordination-gate.md` | Putusan gate sebelumnya dan enam langkah pemulihannya |
| `Migrations/20260902081432_AddAccountingFoundation.cs` | Inventaris operasi, `CONTAMINATION GUARD` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Hitungan tabel dan pemeriksaan deletion |
| `AccJournalType.cs`, `AccJournalTypeConfiguration.cs` | Kolom, nilai bawaan, dan sifat berfilter index uniknya |
| `EmergencyMasterDataSeeder.cs`, `InpatientMasterDataSeeder.cs`, `PrescriptionReviewCriterionSeeder.cs` | Konvensi seeder repository dan pola call site-nya |
| `Program.cs`, `ApplicationDbContext.cs` | Cara seeder dipanggil (atau tidak) di repository ini |
| `TestDatabase.cs`, `AccountingFoundationTests.cs`, `InpatientMasterDataSeederTests.cs` | Harness uji dan konvensi test seeder |
| `AGENTS.md`, `CLAUDE.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Governance preflight dan bentuk laporan |

### DATABASE IMPACT

`NONE` sebagai perubahan schema. Nol migration dibuat, diubah, dihapus, maupun diterapkan; nol
perubahan `ApplicationDbContextModelSnapshot.cs`; nol koneksi ke database mana pun. Yang bertambah
hanyalah **kemampuan** menulis baris ke tabel `AccJournalType` yang sudah berdiri — dan kemampuan
itu belum dipanggil siapa pun. Rinciannya bagian 8 dan 9.

### VALIDATION

| Perintah / pemeriksaan | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | `Build succeeded`, 0 error, 203 warning | **PASS** | Bagian 6 |
| `dotnet test --filter AccountingFoundationTests` | 18 lulus, 0 gagal | **PASS** | Bagian 7 — 18 test lama tetap hijau |
| `dotnet test --filter AccountingMasterDataSeederTests` | 6 lulus, 0 gagal | **PASS** | Bagian 7 — acceptance 3 |
| `dotnet test --filter AccountingManagement` | 24 lulus, 0 gagal | **PASS** | Bagian 7 |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | 200 lulus, 0 gagal | **PASS** | Bagian 7 |
| `dotnet test QuilvianSystemBackend.Tests` (akar) | 852 lulus, 1 gagal | **PRE-EXISTING FAIL** | Bagian 7.A — dibuktikan dengan memindahkan berkas baru keluar lalu menjalankan ulang |
| `dotnet test Tests/QuilvianSystemBackend.BillingTests` | 34 lulus, 52 gagal | **ENVIRONMENT** | Bagian 7.B — `QUILVIAN_BILLING_TEST_DB` tidak disetel; nol test logic berjalan |
| Verifikasi 17 hash artefak canonical | 17/17 cocok | **PASS** | Bagian *Validasi baseline* |
| Impact scan `2b152aa..f40177a` | Nol berkas Accounting berubah | **PASS** | Bagian *Impact scan* |
| `CONTAMINATION GUARD` atas migration | `CLEAN` | **PASS** | Bagian 5 |

## Validasi baseline

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `6` | `6` | Cocok |
| `decision_revision` | `1.3` | `1.3` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |
| `approved_backend_source_sha` | `aa837d7` | `aa837d7` | Cocok |
| `verification_backend_source_sha` | `2b152aa` | `f40177a` | **Berbeda — impact scan dijalankan** |
| Frontend | `5336c44` | tidak relevan | Task backend murni |

### Impact scan `2b152aa` → `f40177a`

71 commit. Ini bukan pergeseran kecil seperti pada task-task sebelumnya: sebagian besar isinya
adalah **merge canonical integration** yang memang menjadi syarat penutupan `ACC-DEP-009`.

| Pemeriksaan | Hasil |
|---|---|
| `f90bcbe` (canonical integration baseline) leluhur `HEAD` | **Ya** — `git merge-base --is-ancestor` |
| Berkas Accounting berubah (`Areas/`, `Repositories/Configurations/`, `Tests/`) | **Nol** — `git diff --stat` kosong |
| `Program.cs` | Berubah `+9/-2`, **seluruhnya milik modul lain** — 2 `AddScoped` Radiology, 1 `AddScoped` Medical Record, 1 pergeseran `using`. Nol baris Accounting, nol pemanggilan seeder |
| `Migrations/` | Bertambah 1 migration Accounting (`f40177a`) dan 4 migration modul lain lewat merge |
| `ApplicationDbContextModelSnapshot.cs` | Berubah pada `f40177a`: **751 insertion, 0 deletion** |

**Nol deletion pada snapshot adalah temuan yang paling perlu dicatat.** Risiko yang tercatat di
`ACC-DEP-001` dan pada riwayat modul lain adalah blok entity modul lain hilang lewat resolusi
merge. Di sini tidak terjadi: `git show f40177a --numstat` melaporkan `751 0`, murni penambahan,
dan hitungan tabel snapshot naik ke **545** dengan **7 `Acc*`** di dalamnya.

**Dampak terhadap `BE-ACC-006`: baseline justru membaik.** `ACC-DEP-009` — satu-satunya
dependency yang menahan task ini dan satu-satunya yang berada di dalam wewenang owner modul —
**tertutup oleh bukti git**, bukan oleh pernyataan.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `MasterData` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002` dan `QBE-MOD-003` **tidak terpicu** — task ini nol model persisted baru. `QBE-NAM-004` tidak dilanggar — nol prefix baru. `QBE-MIG-001`/`QBE-MIG-002` sudah dijalankan lewat Migration Coordination Gate |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. FILE YANG DIBUAT

Dua berkas.

| Berkas | Baris |
|---|---:|
| `Areas/Corporate/AccountingManagement/MasterData/Seeders/AccountingMasterDataSeeder.cs` | 180 |
| `Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingMasterDataSeederTests.cs` | 217 |

## 2. FILE YANG DIUBAH

**Nol berkas source diubah.** Tidak ada `Program.cs`, tidak ada `ApplicationDbContext.cs`, tidak
ada entity, tidak ada configuration, tidak ada berkas migration.

Ditambah register: `roadmap/backend-roadmap.md`, `MODULE-STATUS.md`, `blueprint-manifest.md`,
`evidence/04-migration-coordination-gate.md`.

---

## 3. ISI SEEDER

### Bentuk

`public static class AccountingMasterDataSeeder`, satu metode masuk:

```csharp
public static async Task<AccountingMasterDataSeedResult> SeedAsync(
    ApplicationDbContext db,
    Guid actorUserId,
    CancellationToken ct = default)
```

Bentuk ini diambil apa adanya dari `EmergencyMasterDataSeeder` dan `InpatientMasterDataSeeder`:
static class, `ApplicationDbContext` sebagai parameter, dan objek hasil yang mencatat berapa baris
benar-benar ditambahkan beserta alasan bagian yang dilewati. Nol registrasi DI baru.

### Empat baris, dari `02-backend-architecture.md` bagian 9.1

| Kode | Nama | Awalan nomor | `RequiresApproval` | `IsSystemType` | Trace |
|---|---|---|:---:|:---:|---|
| `JU` | Jurnal Umum | `JU` | `true` | `false` | `ACC-DEC-010` |
| `JP` | Jurnal Penyesuaian | `JP` | `true` | `false` | `ACC-DEC-017` |
| `JB` | Jurnal Pembalik | `JB` | `true` | **`true`** | `ACC-DEC-029` |
| `SA` | Saldo Awal | `SA` | `true` | **`true`** | `ACC-DEC-018`, `ACC-DEC-033` |

Keempatnya `IsActive = true`, `CreateBy = actorUserId`, `CreateDateTime` satu nilai `DateTime.UtcNow`
yang sama untuk seluruh baris dalam satu pemanggilan.

### Dua penjaga, dan alasan keduanya berbeda

**Penjaga kepemilikan** — bila tabel memuat kode yang tidak dikenal daftar seeder (misalnya `GJ`),
seeder berhenti tanpa menambah apa pun dan mengisi `JournalTypeSkippedReason`. Ini pola yang sama
dengan lima master IGD, tetapi akibatnya di sini lebih keras: `NumberPrefix` menjadi awalan nomor
jurnal, sehingga dua set jenis jurnal berarti **dua skema penomoran berjalan bersamaan di atas satu
buku besar**. Penjaga ini membaca baris yang masih hidup saja — kode asing yang sudah dihapus lunak
tidak sedang dipakai siapa pun dan tidak boleh mengunci seeder.

**Idempotensi** — pemeriksaan `JournalTypeCode` membaca **seluruh** baris, termasuk yang sudah
dihapus lunak. Perlu ditegaskan karena alasannya berbeda dari `InpatientMasterDataSeeder`: di sana
pilihan itu wajib karena index uniknya tidak berfilter, sehingga mengabaikan `IsDelete` akan
menabrak index dan menghentikan aplikasi. Di sini `IX_AccJournalType_JournalTypeCode` **berfilter**
`"IsDelete" = false`, jadi tabrakan itu tidak mungkin terjadi. Pilihannya murni soal menghormati
keputusan admin: baris yang sengaja dihapus tidak dihidupkan lagi diam-diam setiap kali seeder
dijalankan. Yang dilewati terhitung pada `JournalTypeSkipped` supaya master yang tampak kurang
lengkap selalu ada penjelasannya.

### Yang sengaja TIDAK diisi seeder

| Master | Alasan | Milik |
|---|---|---|
| `AccChartOfAccount` | Daftar akun adalah kebijakan akuntansi rumah sakit dan wajib disusun pemilik proses (bagian 9.3). Menebaknya menghasilkan master palsu yang terlanjur dipakai pembukuan | Pemilik proses, lewat `BE-ACC-007` |
| `AccAccountingPeriod` | Dibangkitkan sekaligus lewat `POST /generate`, bukan diisi satu per satu (bagian 9.2) | `BE-ACC-009` |

Kerangka lima kelompok akun pada bagian 9.3 **tidak** di-seed. Bagian itu menyebut dirinya
"kerangka minimum" dan menyerahkan penyusunannya kepada pemilik proses; roadmap `BE-ACC-006` juga
hanya menyebut empat baris `AccJournalType` sebagai cakupan. Batas ini dijaga test tersendiri.

## 4. CARA PEMANGGILAN

**Belum ada call site di kode aplikasi, dan itu disengaja** — diputuskan owner pada sesi ini.

Dasarnya dua hal yang sejalan:

1. **Artefak canonical melarang jalur startup.** `02-backend-architecture.md` bagian 6:
   > Yang **tidak** boleh ditambahkan ke `Program.cs`: **pemanggilan seeder**, logika startup, atau
   > konfigurasi khusus Accounting.
2. **Konvensi repository sudah begitu.** `EmergencyMasterDataSeeder` dan `InpatientMasterDataSeeder`
   keduanya **nol call site** di kode aplikasi — diverifikasi lewat pencarian seluruh repository.
   Keduanya static class yang dibuktikan test dan menunggu pemanggilnya. Hanya
   `PrescriptionReviewCriterionSeeder` yang dipanggil `Program.cs`, itu pun di balik flag konfigurasi.

Cara memanggilnya nanti, satu baris:

```csharp
var hasil = await AccountingMasterDataSeeder.SeedAsync(db, actorUserId, ct);
```

Karena idempotensinya sudah dibuktikan test, ia aman dipanggil berulang, apa pun yang kelak menjadi
pemanggilnya.

**Konsekuensi yang perlu dibaca apa adanya:** selama call site belum ada, tabel `AccJournalType`
di database **masih kosong**. Empat baris itu terbukti terbentuk benar di basis data uji, bukan di
database pengembangan. Pengisian sesungguhnya menunggu `BE-ACC-008`, dan tercatat sebagai blocker
pada bagian 12.

## 5. MIGRATION — PEMERIKSAAN ISI

Migration `20260902081432_AddAccountingFoundation` **dibuat dan diterapkan owner modul sebelum
sesi ini**, pada commit `f40177a`. Sesi ini **tidak** membuat, mengubah, menghapus, maupun
menerapkan migration apa pun. Yang dikerjakan di sini adalah pemeriksaan isinya, yang memang
diwajibkan `02-backend-architecture.md` bagian 8.

### Jumlah operasi yang ditemukan — butir DoD

**Operasi tingkat atas `Up()`: 28.**

| Operasi | Jumlah |
|---|---:|
| `CreateTable` | **7** |
| `CreateIndex` | **21** |
| **Total `Up()`** | **28** |

**Constraint yang menyatu di dalam `CreateTable`: 19.**

| Constraint | Jumlah |
|---|---:|
| `table.PrimaryKey` | 7 |
| `table.ForeignKey` | 11 |
| `table.CheckConstraint` | 1 |

**Operasi `Down()`: 7** — tujuh `DropTable`, simetris dengan `Up()`.

Dari 21 index, **6 unique** dan **5 berfilter** `"IsDelete" = false`.

### Sebaran tujuh tabel dan 21 index

| Tabel | `CreateIndex` | Unique |
|---|---:|---:|
| `AccAccountingPeriod` | 3 | 1 |
| `AccChartOfAccount` | 4 | 1 |
| `AccJournal` | 6 | 1 |
| `AccJournalApproval` | 2 | 0 |
| `AccJournalLine` | 3 | 1 |
| `AccJournalType` | 2 | 1 |
| `AccNumberSeries` | 1 | 1 |
| **Total** | **21** | **6** |

### Enam unique index

| Index | Berfilter |
|---|:---:|
| `IX_AccAccountingPeriod_LegalEntityId_PeriodCode` | Ya |
| `IX_AccChartOfAccount_LegalEntityId_AccountCode` | Ya |
| `IX_AccJournal_LegalEntityId_JournalNumber` | Ya |
| `IX_AccJournalLine_JournalId_LineNumber` | Ya |
| `IX_AccJournalType_JournalTypeCode` | Ya |
| `IX_AccNumberSeries_SequenceKey_ScopeKey` | **Tidak** |

`AccNumberSeries` sengaja tidak berfilter: deret nomor tidak pernah dihapus lunak, dan filter
justru akan membuka celah dua deret aktif untuk satu `(SequenceKey, ScopeKey)`.

### Satu check constraint

```
CK_AccJournalLine_TepatSatuSisiTerisi
  ("DebitAmount" > 0 AND "CreditAmount" = 0)
  OR ("DebitAmount" = 0 AND "CreditAmount" > 0)
```

### `CONTAMINATION GUARD` — **LULUS**

Guard tidak dijalankan sebagai penyaring prefix, melainkan sebagai pembandingan terhadap tujuh
tabel yang direncanakan, persis seperti yang diminta roadmap.

| Pemeriksaan | Hasil |
|---|---|
| Nama tujuh `CreateTable` | `AccAccountingPeriod`, `AccChartOfAccount`, `AccJournalType`, `AccNumberSeries`, `AccJournal`, `AccJournalApproval`, `AccJournalLine` — **tujuh, cocok seluruhnya** dengan rencana |
| Target 21 `CreateIndex` | **Seluruhnya** tabel `Acc*` |
| Operasi selain `CreateTable`/`CreateIndex` di `Up()` | **Nol** |
| Operasi menyentuh tabel modul lain | **Nol** — nol `AlterTable`, `AddColumn`, `DropColumn`, `RenameTable`, `DropTable` di `Up()` |
| Pencarian nama non-`Acc*` sebagai objek operasi | **Nol kecocokan** |
| `Mst*` sebagai `principalTable` | `MstLegalEntity` (3), `MstCostCenter` (1) — **tujuan foreign key, bukan operasi schema**. Keduanya tidak diubah sedikit pun |

**Putusan: `CLEAN`.** Nol operasi asing, jadi tidak ada satu pun konsekuensi `CONTAMINATED` yang
berlaku.

## 6. BUILD RESULT

```
dotnet build ./QuilvianSystemBackend.sln
Build succeeded.
    203 Warning(s)
    0 Error(s)
```

**0 error.** Pemeriksaan terarah pada keluaran build: **nol warning berasal dari kedua berkas
baru**. 203 warning seluruhnya pre-existing dan milik modul lain — jumlahnya naik dari 199 yang
tercatat `BE-ACC-005` karena 71 commit modul lain masuk lewat merge integration, bukan karena
task ini.

## 7. TEST RESULT

18 → **24 test Accounting**.

| Perintah | Hasil |
|---|---|
| filter `AccountingFoundationTests` (18 test lama) | **18 lulus**, 0 gagal, 22 detik |
| filter `AccountingMasterDataSeederTests` (6 test baru) | **6 lulus**, 0 gagal, 25 detik |
| filter `AccountingManagement` (seluruhnya) | **24 lulus**, 0 gagal, 29 detik |
| `Tests/QuilvianSystemBackend.Tests` (project penuh) | **200 lulus**, 0 gagal, 2 m 34 s |
| `QuilvianSystemBackend.Tests` (akar) | 852 lulus, **1 gagal**, 853 total |
| `Tests/QuilvianSystemBackend.BillingTests` | 34 lulus, **52 gagal**, 86 total |

**Delapan belas test `BE-ACC-005` tetap hijau seluruhnya** — itu yang diminta scope, dan
terpenuhi.

### Enam test baru

| Test | Membuktikan |
|---|---|
| `Seeder_MengisiEmpatJenisJurnalSesuaiArsitekturBagian91` | Acceptance 3 — empat baris, kolom demi kolom terhadap bagian 9.1 |
| `SeluruhJenisJurnal_MenuntutPersetujuanDanHanyaJbSaBertandaSistem` | Acceptance 3 — `JB` dan `SA` bertanda sistem, `JU`/`JP` tidak, keempatnya `RequiresApproval` |
| `Seeder_DijalankanDuaKali_TidakMenghasilkanDataGanda` | Idempotensi — pemanggilan kedua menyisipkan nol baris |
| `Seeder_TidakMenimpaBarisYangSudahDisesuaikanAdmin` | Baris yang sudah disesuaikan tidak ditimpa; tiga sisanya tetap ditambahkan |
| `Seeder_BerhentiBilaMasterSudahDiisiSumberLain` | Penjaga kepemilikan — nol baris ditambahkan, alasan terisi |
| `Seeder_TidakMengisiDaftarAkunMaupunPeriode` | Batas cakupan — nol baris pada COA, periode, jurnal, dan deret nomor |

Seluruhnya memakai `TestDatabase` (SQLite di memori, `EnsureCreated`) — **tidak pernah** menyentuh
database mana pun di luar prosesnya sendiri.

### Dua kegagalan pre-existing, keduanya bukan regresi

**A. `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`**
— `Assert.Equal()` gagal, `Expected: "FINAL"`, `Actual: "CLOSED"`.

Dibuktikan pre-existing, bukan disimpulkan: kedua berkas baru **dipindahkan keluar sementara**,
project di-build ulang, dan test yang sama gagal **identik**. Sesudah itu berkasnya dikembalikan
dan solution di-build ulang. Penyebabnya berada di semantik status folio Billing yang bergeser
lewat merge integration — milik owner Billing, bukan Accounting.

**B. 52 kegagalan `QuilvianSystemBackend.BillingTests`** — seluruh 52-nya satu sebab yang sama,
`System.InvalidOperationException` atas environment variable `QUILVIAN_BILLING_TEST_DB` yang tidak
disetel. Fixture-nya memang menolak berjalan tanpa database test tersendiri, dan daftar penanda
terlarangnya memuat `dev` dan `shared`. Nol test logic yang dijalankan. **Tidak** disetel di sesi
ini — menyetelnya berarti menerapkan migration ke sebuah database, dan itu di luar wewenang task.

## 8. MIGRATION STATUS

| Pemeriksaan | Hasil |
|---|---|
| Jumlah berkas migration | **119, tidak bertambah** — sama persis dengan keadaan saat sesi dimulai |
| Migration Accounting | **1** — `20260902081432_AddAccountingFoundation`, dibuat owner pada `f40177a` |
| `dotnet ef migrations add` | **Tidak dijalankan** |
| `dotnet ef database update` | **Tidak dijalankan** |
| `dotnet ef migrations remove` | **Tidak dijalankan** |
| Database disentuh | **Tidak** — nol koneksi ke database mana pun |
| `git diff -- Migrations/` | **Kosong** |

## 9. SNAPSHOT STATUS

`Migrations/ApplicationDbContextModelSnapshot.cs` **tidak berubah pada sesi ini** — `git diff`
kosong.

| Pemeriksaan | Nilai |
|---|---:|
| Jumlah `b.ToTable(` | **545** |
| Blok `Acc*` | **7** |
| Deletion pada `f40177a` | **0** |

Ketujuhnya: `AccAccountingPeriod`, `AccChartOfAccount`, `AccJournal`, `AccJournalApproval`,
`AccJournalLine`, `AccJournalType`, `AccNumberSeries`.

## 10. ACCEPTANCE CRITERIA `BE-ACC-006`

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | `CONTAMINATION GUARD` lulus | ✅ | Bagian 5 — tujuh `CreateTable` seluruhnya `Acc*`, 21 `CreateIndex` seluruhnya menunjuk tabel `Acc*`, nol operasi asing, nol tabel modul lain diubah. Putusan `CLEAN` |
| 2 | Migration memuat tepat tujuh `CreateTable` bernama sesuai prefix terdaftar, beserta index dan foreign key-nya | ✅ | Bagian 5 — 7 `CreateTable` + 21 `CreateIndex` + 11 `ForeignKey` + 7 `PrimaryKey` + 1 `CheckConstraint`. Prefix `Acc` terdaftar, lifecycle `ACTIVE` |
| 3 | Empat jenis jurnal terisi dengan `JB` dan `SA` bertanda sistem | ✅ | `Seeder_MengisiEmpatJenisJurnalSesuaiArsitekturBagian91` dan `SeluruhJenisJurnal_MenuntutPersetujuanDanHanyaJbSaBertandaSistem`. **Terbukti pada basis data uji; di database pengembangan belum terisi karena call site menunggu `BE-ACC-008`** — lihat bagian 4 dan 12 |
| 4 | Berkas evidence gate berisi SHA baseline yang menjadi sumber migration ini | ✅ | `evidence/04-migration-coordination-gate.md` diperbarui dengan putusan ulang dan SHA `f40177a`, ditambah bukti `f90bcbe` sebagai leluhur |

Ketentuan tambahan dari cakupan roadmap:

| Ketentuan | Hasil |
|---|:---:|
| Pengisian **bukan** lewat skrip SQL manual | ✅ Nol berkas `.sql`; pengisian lewat aplikasi |
| Konvensi seeder repo diikuti | ✅ Bentuk `EmergencyMasterDataSeeder`/`InpatientMasterDataSeeder` |
| Hanya menambah baris yang belum ada berdasarkan `JournalTypeCode` | ✅ Dijaga dua test |
| Tidak pernah menimpa baris yang sudah tersimpan | ✅ `Seeder_TidakMenimpaBarisYangSudahDisesuaikanAdmin` |

## 11. DEFINITION OF DONE

| Butir DoD roadmap | Hasil |
|---|:---:|
| Migration diperiksa isinya | ✅ Bagian 5 — inventaris operasi lengkap |
| Migration diterapkan owner modul | ✅ Dilakukan owner pada `f40177a`, sebelum sesi ini |
| Master terisi | ⚠️ **Sebagian** — seeder ada dan terbukti; call site menunggu `BE-ACC-008`, atas keputusan owner |
| Laporan berisi jumlah operasi yang ditemukan | ✅ Bagian 5 — 28 operasi `Up()`, 19 constraint inline, 7 operasi `Down()` |
| Build lulus | ✅ 0 error, nol warning baru |
| Test hijau | ✅ 24 lulus, termasuk 18 test lama yang tetap hijau |

**Status yang diusulkan: `DONE` dengan satu catatan terbuka pada butir "master terisi".** Butir
itu tidak dapat ditutup penuh tanpa call site, dan call site-nya sengaja ditunda ke `BE-ACC-008`
atas keputusan owner. Bila owner menghendaki DoD tertutup penuh di task ini, jalannya adalah
menyetujui penambahan endpoint pengisian master — bukan menambah pemanggilan ke `Program.cs`,
yang dilarang artefak canonical.

## 12. KNOWN ISSUES / DEFERRED

### A. Call site seeder belum ada

Diputuskan owner pada sesi ini. Selama belum ada, `AccJournalType` di database pengembangan tetap
kosong, sehingga `BE-ACC-010` tidak akan menemukan awalan nomor jurnal. **Ini blocker fungsional
untuk `BE-ACC-010`, bukan untuk `BE-ACC-007`.**

### B. Dependency yang masih terbuka

| Dependency | Menahan | Pemilik | Keadaan |
|---|---|---|---|
| `ACC-DEP-009` | `BE-ACC-006` | Owner modul | **CLOSED** — `f90bcbe` terbukti leluhur `HEAD` |
| `ACC-DEP-008` | `BE-ACC-007` ke atas | Security / Platform | **Terbuka** — model otorisasi badan hukum belum ada |
| `ACC-DEP-007` | Merge ke integration | Lead / pemilik registry | **Terbuka** — gerbang CI QBE mati |
| `ACC-DEP-005` | Aturan koordinasi migration canonical | Lead | **Terbuka** — `QBE-MIG-001`/`002` masih `PROPOSED` |

### C. Berlanjut dari task sebelumnya

1. **Utang teknis pre-existing** — dua project bernama `QuilvianSystemBackend.Tests`. Tidak disentuh.
2. **`QUILVIAN_BILLING_TEST_DB` tidak disetel** — 52 test Billing tidak dapat berjalan di mesin ini.
   Bukan milik Accounting.
3. **Satu test Billing merah** — semantik status folio, milik owner Billing. Bagian 7.A.

## API CONTRACT IMPACT

`NONE`. Nol controller, endpoint, DTO. Nol integrasi Finance, Billing, atau AR/AP.

## SECURITY IMPACT

`NONE` sebagai perubahan. Seeder tidak membaca identitas pemanggil dan tidak memeriksa hak akses —
ia menerima `actorUserId` sebagai parameter jejak audit `CreateBy`. Penegakan hak akses menjadi
tanggung jawab pemanggilnya kelak, dan `ACC-DEP-008` tetap milik Security/Platform.

`AccJournalType` sengaja tanpa `LegalEntityId` (`ACC-DEC-037` tidak berlaku padanya): jenis jurnal
bersifat struktural dan berlaku sama untuk semua badan hukum. Karena itu seeder ini tidak
memerlukan penyaringan badan hukum, dan tidak terhalang `ACC-DEP-008`.

## MANUAL TEST

`NOT APPLICABLE` — seeder belum punya call site, sehingga tidak ada jalur aplikasi yang dapat
dijalankan secara manual untuk membuktikannya.

Perlu ditegaskan supaya tidak salah dibaca sebagai kehati-hatian yang keliru: database
pengembangan pada `appsettings.Development.json` adalah **`QuilvianNewDevRizki`, milik owner modul
sendiri, bukan database bersama satu tim**. Jadi hambatannya bukan risiko mengganggu pekerjaan
orang lain, melainkan memang belum ada yang memanggil seeder itu.

## INCIDENTAL CHANGES

`NONE`. Kedua berkas baru dipindahkan keluar sementara untuk membuktikan kegagalan test Billing
bersifat pre-existing, lalu dikembalikan ke tempat semula. Keadaan akhir identik dengan sebelum
pemeriksaan itu.

## GIT STATUS

```
?? Areas/Corporate/AccountingManagement/MasterData/Seeders/
?? Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingMasterDataSeederTests.cs
```

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

`MVP-0` **tuntas**: tujuh entity berdiri, satu migration bersih sudah diterapkan, dan data master
awal sudah punya mekanisme pengisiannya.

`BE-ACC-007` **tidak dapat dimulai**. Penahannya bukan `BE-ACC-006` melainkan **`ACC-DEP-008`** —
di titik endpoint pertama, pertanyaan "pengguna ini boleh melihat badan hukum yang mana" tidak lagi
dapat dihindari, dan model otorisasinya belum ada. Ini milik Security/Platform, bukan owner modul.

**Menunggu instruksi eksplisit owner.**
