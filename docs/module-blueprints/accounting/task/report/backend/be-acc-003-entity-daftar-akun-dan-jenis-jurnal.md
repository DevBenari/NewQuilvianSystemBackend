# BE-ACC-003 — Entity daftar akun dan jenis jurnal

- **TASK ID:** `BE-ACC-003` — Entity daftar akun dan jenis jurnal
- **TASK TYPE:** Implementasi backend, entity persisted pertama modul Accounting
- **COMPLEXITY:** `LIGHT`
- **CLASSIFICATION SCORE:** rendah — dua entity, dua configuration, dua `DbSet`, tanpa endpoint dan tanpa migration
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend`
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 6 `APPROVED`, roadmap revisi 2 `APPROVED`
- **HEAD saat mulai:** `ca6b7e0` pada branch `rizkiG`

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `MasterData/ChartOfAccount`, `MasterData/JournalType` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002` (terpenuhi — prefix `ACTIVE`), `QBE-MOD-003` (terpenuhi — Area/Module terdaftar sebelum model pertama), `QBE-NAM-001` (tidak dilanggar — nol `Trx*` baru), `QBE-CFG-001` (terpenuhi — configuration terpisah per entity) |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `QuilvianEngineeringSkills/agents/rules/backend/engineering/` |

Ini **entity persisted pertama** modul Accounting. `QBE-MOD-002` sebelumnya memblokirnya; penghalang itu dicabut `ACC-DEC-038` yang menaikkan lifecycle `Acc` menjadi `ACTIVE` pada 1 September 2026.

## FILES INSPECTED

`docs/module-blueprints/accounting/erd/01-chart-of-account.md`; `erd/data-dictionary.md` bagian 1 dan 2; `roadmap/backend-roadmap.md` bagian `BE-ACC-003`; `contracts/validation-matrix.md`; `Models/IdentityModel.cs`; `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs`; `.../Models/MstLegalEntity.cs`; `Repositories/Configurations/Corporate/HumanResource/MasterData/Organization/MstCostCenterConfiguration.cs`; `Repositories/ApplicationDbContext.cs`; `Tests/QuilvianSystemBackend.Tests/Infrastructure/TestDatabase.cs`; `Tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj`.

## FILES CHANGED

Empat berkas dibuat, dua berkas existing diubah.

| Berkas | Jenis | Baris |
|---|---|---:|
| `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Models/AccChartOfAccount.cs` | **Baru** — entity | 76 |
| `Areas/Corporate/AccountingManagement/MasterData/JournalType/Models/AccJournalType.cs` | **Baru** — entity | 50 |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccChartOfAccountConfiguration.cs` | **Baru** — configuration | 100 |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccJournalTypeConfiguration.cs` | **Baru** — configuration | 71 |
| `Repositories/ApplicationDbContext.cs` | **Diubah** — 2 `using` + 2 `DbSet` dalam region baru | +7 |
| `Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingFoundationTests.cs` | **Diubah** — 1 test diperbarui, 3 test baru | +180 |

## ENTITY YANG DIBUAT

### `AccChartOfAccount`

Tabel `public.AccChartOfAccount`. Dua belas kolom, persis kamus data bagian 1.

| Kolom | Tipe | Wajib | Bawaan | Index |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `AccountCode` |
| `AccountCode` | `string(20)` | Ya | — | Unique bersama `LegalEntityId` |
| `AccountName` | `string(200)` | Ya | — | Index bersama `LegalEntityId` |
| `ParentAccountId` | `Guid?` | Tidak | — | Index |
| `AccountLevel` | `int` | Ya | `1` | — |
| `AccountType` | `enum → int` | Ya | — | Index |
| `NormalBalance` | `enum → int` | Ya | — | — |
| `IsPostable` | `bool` | Ya | `false` | — |
| `IsActive` | `bool` | Ya | `true` | — |
| `EffectiveStartDate` | `date?` | Tidak | — | — |
| `Description` | `string(500)?` | Tidak | — | — |

Ditambah sepuluh kolom audit warisan `IdentityModel`.

### `AccJournalType`

Tabel `public.AccJournalType`. Tujuh kolom, persis kamus data bagian 2.

| Kolom | Tipe | Wajib | Bawaan | Index |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK |
| `JournalTypeCode` | `string(10)` | Ya | — | **Unique global** |
| `JournalTypeName` | `string(100)` | Ya | — | — |
| `NumberPrefix` | `string(10)` | Ya | — | — |
| `RequiresApproval` | `bool` | Ya | `true` | — |
| `IsSystemType` | `bool` | Ya | `false` | — |
| `IsActive` | `bool` | Ya | `true` | Index bersama `IsDelete` |

**Sengaja tanpa `LegalEntityId`**, sesuai kamus data: jenis jurnal berlaku sama untuk semua badan hukum.

## CONFIGURATION YANG DIBUAT

Keduanya di `Repositories/Configurations/Corporate/AccountingManagement/MasterData/`, mengikuti pola `MstCostCenterConfiguration`.

### Relasi — seluruhnya `DeleteBehavior.Restrict`

| Relasi | Dari | Ke | Perilaku |
|---|---|---|---|
| Badan hukum pemilik | `AccChartOfAccount.LegalEntityId` | `MstLegalEntity` | `Restrict` |
| Induk-anak | `AccChartOfAccount.ParentAccountId` | `AccChartOfAccount` | `Restrict` |

**`MstLegalEntity` tidak disentuh.** Relasinya memakai `.WithMany()` tanpa navigasi balik, jadi entity milik Human Resource tidak perlu ditambahi koleksi. ERD menyatakan `MstLegalEntity` **MUST NOT** disalin; ini juga berarti tidak perlu diubah. Preseden pola yang sama ada di `MstCostCenterConfiguration` untuk relasi `Department`.

### Index

| Entity | Index | Sifat |
|---|---|---|
| `AccChartOfAccount` | `(LegalEntityId, AccountCode)` | **Unique**, filter `"IsDelete" = false` |
| `AccChartOfAccount` | `(LegalEntityId, AccountName)` | Pencarian |
| `AccChartOfAccount` | `ParentAccountId` | Penelusuran susunan |
| `AccChartOfAccount` | `AccountType` | Penyaringan jenis akun |
| `AccJournalType` | `JournalTypeCode` | **Unique**, filter `"IsDelete" = false` |
| `AccJournalType` | `(IsActive, IsDelete)` | Daftar pilihan |

Filter `"IsDelete" = false` mengikuti konvensi repository: 349 dari 353 unique index di `Repositories/Configurations/Corporate/` memakainya. Tanpa filter itu, baris yang sudah dihapus lunak akan menghalangi pemakaian ulang kode akun yang sama.

### Enum disimpan sebagai angka

`AccountType` dan `NormalBalance` memakai `.HasConversion<int>()`, sesuai kamus data dan sejalan dengan 66 configuration lain yang memakai pola sama.

## DBCONTEXT IMPACT

`Repositories/ApplicationDbContext.cs` bertambah **7 baris**: dua `using` dan satu region berisi dua `DbSet`.

```
#region CORPORATE - ACCOUNTING MANAGEMENT - MASTER DATA
public DbSet<AccChartOfAccount> AccChartOfAccounts { get; set; }
public DbSet<AccJournalType> AccJournalTypes { get; set; }
#endregion CORPORATE - ACCOUNTING MANAGEMENT - MASTER DATA
```

Tidak ada baris existing yang diubah atau dihapus — murni penambahan. Configuration **tidak** perlu didaftarkan satu per satu karena `OnModelCreating` sudah memakai `ApplyConfigurationsFromAssembly` (baris 719).

`ApplicationDbContextModelSnapshot.cs` **tidak disentuh**, sesuai larangan.

## VALIDATION YANG DIBUAT

Perlu dibedakan dua lapis, karena hanya lapis pertama yang menjadi cakupan task ini.

### Lapis struktural — dibuat pada task ini

| Aturan | Cara penegakan |
|---|---|
| Kode akun unik per badan hukum | Unique index `(LegalEntityId, AccountCode)` |
| Kode jenis jurnal unik global | Unique index `JournalTypeCode` |
| Kolom wajib dan panjang teks | `[Required]`, `[MaxLength]`, dan `.IsRequired()`/`.HasMaxLength()` |
| Induk dan badan hukum tidak boleh terhapus selama masih dirujuk | `DeleteBehavior.Restrict` |
| Akun tingkat pertama boleh tanpa induk | `ParentAccountId` bertipe `Guid?` |

### Lapis aturan bisnis — **bukan** cakupan task ini

Empat aturan berikut butuh membaca jurnal atau saldo, jadi tempatnya di service pada `BE-ACC-007` ke atas. Ditulis di sini supaya tidak dikira terlewat:

| Aturan | Keputusan | Menunggu |
|---|---|---|
| Akun induk tidak boleh menerima transaksi | `ACC-DEC-022` | `BE-ACC-007` |
| Kode akun tidak boleh berubah setelah dipakai jurnal `Posted` | `ACC-DEC-023` | `BE-ACC-007` + `BE-ACC-005` |
| Akun bersaldo tidak boleh dinonaktifkan | `ACC-DEC-024` | `BE-ACC-012` |
| Pengguna hanya menyentuh badan hukum haknya | `ACC-DEC-037` | **`ACC-DEP-008`** |

Ketiadaan penegakan `ACC-DEC-037` **bukan** kelalaian task ini — mekanismenya memang belum ada di platform, dan itulah `ACC-DEP-008`. Sesuai instruksi owner, **tidak ada** authorization mechanism atau security filter baru yang dibuat di Accounting.

## TEST YANG DIBUAT

Berkas `AccountingFoundationTests.cs` naik dari 9 menjadi **12 test**.

| Test | Sifat | Membuktikan |
|---|---|---|
| `ModulAccounting_HanyaMemilikiEntityCakupanBeAcc003` | **Diperbarui** | Persis dua entity yang boleh ada. Menggagalkan penambahan entity `BE-ACC-004`/`BE-ACC-005` yang mendahului urutan task |
| `AccChartOfAccount_SesuaiKamusData` | **Baru** | Panjang teks, kolom wajib, `ParentAccountId` nullable, `NormalBalance` berdiri sendiri, dan ketiga nilai bawaan |
| `AccJournalType_SesuaiKamusData` | **Baru** | Panjang teks, kolom wajib, **ketiadaan `LegalEntityId`**, dan ketiga nilai bawaan |
| `ModelEfCore_MembentukTabelDanRelasiSesuaiKontrak` | **Baru** | Nama tabel, kedua unique index, seluruh FK `Restrict`, relasi induk-anak menunjuk tabel sendiri, enum tersimpan sebagai `int`, dan tiadanya kolom `RequiresCostCenter` |

Test keempat memakai `TestDatabase` yang sudah baku di repository — SQLite di memori, `EnsureCreated()`, dan **tidak pernah** menyentuh database bersama. Ia memeriksa model EF Core yang benar-benar terbentuk, bukan isi berkas configuration-nya, sehingga membuktikan entity dan configuration sudah tersambung dengan benar.

Test guard lama diperbarui, bukan dihapus. Dulu ia menuntut nol entity; kini menuntut persis dua. Fungsinya sebagai pagar urutan task tetap hidup.

## BUILD RESULT

```
dotnet build QuilvianSystemBackend.sln
Build succeeded.
    199 Warning(s)
    0 Error(s)
```

**0 error.** 199 warning, jumlahnya **sama persis** dengan baseline `BE-ACC-001`, dan pemeriksaan terarah memastikan **nol warning berasal dari berkas Accounting**.

## TEST RESULT

| Perintah | Hasil |
|---|---|
| `dotnet test Tests/QuilvianSystemBackend.Tests --filter AccountingManagement` | **12 lulus**, 0 gagal, 14 detik |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **144 lulus**, 0 gagal, 1 m 51 s |
| `dotnet test QuilvianSystemBackend.Tests` (akar) | **815 lulus**, 0 gagal, 24 detik |

**959 test lulus, nol gagal, nol regresi.** Naik dari 956 pada `BE-ACC-001` karena tiga test baru.

## MIGRATION STATUS

**Tidak ada migration yang dibuat.** Diverifikasi:

| Pemeriksaan | Hasil |
|---|---|
| `Migrations/` berubah | **Tidak** — 0 berkas |
| Berkas migration baru | **Tidak** — 0 berkas |
| `dotnet ef migrations add` dijalankan | **Tidak** |
| `dotnet ef database update` dijalankan | **Tidak** |
| Shared database disentuh | **Tidak** |

Migration adalah cakupan `BE-ACC-006`, dan tetap tertahan Migration Coordination Gate beserta `ACC-DEP-005`.

## SNAPSHOT STATUS

`Migrations/ApplicationDbContextModelSnapshot.cs` **tidak berubah** — `git status` menghasilkan 0 berkas.

**Konsekuensi yang perlu diketahui:** model EF Core kini memuat dua entity yang belum ada di snapshot. Ini **disengaja dan benar** untuk task ini. Akibatnya, `dotnet ef migrations add` berikutnya akan menghasilkan `CreateTable` untuk kedua tabel. Itu justru yang diharapkan pada `BE-ACC-006`, dan menjadi bahan pemeriksaan hitung-operasi yang diwajibkan `02-backend-architecture.md` bagian 8.

Selama `BE-ACC-006` belum dijalankan, aplikasi tetap dapat di-build dan seluruh test tetap lulus, karena tidak ada kode yang membaca kedua tabel itu dari database.

## SECURITY IMPACT

`NONE` sebagai perubahan. Tidak ada endpoint, atribut hak akses, jalur autentikasi, authorization mechanism, maupun security filter yang dibuat atau disentuh.

`LegalEntityId` ditambahkan **hanya sebagai kolom data model**, karena memang bagian kontrak (`ACC-DEC-037`, kamus data bagian 1). Penegakannya menunggu `ACC-DEP-008`.

## API CONTRACT IMPACT

`NONE`. Tidak ada controller, endpoint, atau DTO yang dibuat.

## VISUAL REFERENCE

`NOT REQUIRED`.

## Acceptance criteria

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | Kolom, tipe, panjang, dan index persis seperti kamus data | ✅ | Tiga test schema; tabel kolom di atas dibandingkan baris demi baris terhadap kamus data bagian 1 dan 2 |
| 2 | Relasi induk-anak memakai `DeleteBehavior.Restrict` | ✅ | `ModelEfCore_MembentukTabelDanRelasiSesuaiKontrak` memeriksa **seluruh** FK `AccChartOfAccount`, bukan hanya induk-anak |
| 3 | Build lulus | ✅ | 0 error, nol warning baru |

Tambahan yang diminta roadmap secara eksplisit:

| Ketentuan | Hasil | Bukti |
|---|:---:|---|
| Unique index `(LegalEntityId, AccountCode)` | ✅ | Diperiksa lewat model EF Core |
| Unique index `(JournalTypeCode)` | ✅ | Diperiksa lewat model EF Core |
| **Tanpa** kolom `RequiresCostCenter` | ✅ | `Assert.Null(coa.FindProperty("RequiresCostCenter"))` |

## Definition of Done

| Butir | Hasil |
|---|:---:|
| Build lulus | ✅ 0 error |
| Configuration cocok dengan kamus data | ✅ 3 test schema, seluruhnya lulus |
| Tanpa migration | ✅ 0 berkas migration, snapshot tidak berubah |

## WARNINGS

199 warning build, **seluruhnya pre-existing pada modul lain** dan identik dengan baseline `BE-ACC-001`: `MSB3277` konflik versi `Microsoft.Extensions.DependencyModel` di `BillingTests`, sejumlah `xUnit2029`/`xUnit2031` di `InPatientManagement`, dan satu `CS8603` di `LaboratoryAuthorityTests`. Nol berasal dari berkas Accounting, dan tidak ada yang diperbaiki karena di luar cakupan task.

## KNOWN ISSUES

### 1. Model EF Core kini mendahului snapshot

Dijelaskan pada bagian *Snapshot status*. Disengaja, dan diselesaikan `BE-ACC-006` lewat gerbangnya sendiri.

### 2. `ACC-DEP-008` tetap terbuka

Kolom `LegalEntityId` sudah ada, penegakannya belum. Selama `ACC-DEP-008` terbuka, `BE-ACC-007` ke atas tetap tertahan. Milik Security/Platform.

### 3. Utang teknis pre-existing, tidak disentuh

Sisa folder `agents/rules/` masih terlacak git padahal `AGENTS.md` baris 53 menyatakan sudah dicabut; dan dua project bernama `QuilvianSystemBackend.Tests` di solution. Keduanya diklasifikasikan **pre-existing platform/technical debt** sesuai instruksi owner.

### 4. Gerbang CI QBE masih mati

`ACC-DEP-007`. Kesesuaian task ini terhadap QBE diverifikasi manual terhadap registry canonical suite skill, karena checker tidak dapat dijalankan. Milik lead.

## MANUAL TEST

`NOT APPLICABLE` — tidak ada perilaku runtime yang dapat diamati pengguna. Tabelnya belum berdiri di database mana pun, dan belum ada endpoint yang membacanya.

## INCIDENTAL CHANGES

`NONE`. Satu-satunya berkas existing di luar test yang diubah adalah `ApplicationDbContext.cs`, dan itu memang bagian cakupan task (dua `DbSet`).

## INTERRUPTIONS

`NONE`.

## GIT STATUS

```
 M Repositories/ApplicationDbContext.cs
?? Areas/Corporate/AccountingManagement/
?? Repositories/Configurations/Corporate/AccountingManagement/
?? Tests/QuilvianSystemBackend.Tests/AccountingManagement/
```

Ditambah berkas dokumentasi blueprint dari `BE-ACC-001`, `BE-ACC-002`, dan task ini.

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

`BE-ACC-004` — entity periode akuntansi. Dependency-nya `BE-ACC-003` kini terpenuhi, dan ia **tidak** tertahan `ACC-DEP-008` dengan alasan yang sama seperti task ini.

**Menunggu instruksi eksplisit owner.** Approval roadmap bukan perintah jalan, dan instruksi task ini secara tegas melarang melanjutkan ke `BE-ACC-004`.
