# QuilvianSystemBackend.IntegrationTests.Postgres

Test otomatis untuk modul Billing Operational, Area `HealthServices`.

| Field | Nilai |
| --- | --- |
| Task asal | `RJ-BIL-BE-001` |
| Framework | xUnit `2.9.2` |
| Target framework | `net9.0` |
| Jenis test | Integration di level service, terhadap PostgreSQL sungguhan |
| Cakupan saat ini | Keempat acceptance criteria `RJ-BIL-BE-001` |

## Cara menjalankan

```
dotnet test Tests/QuilvianSystemBackend.IntegrationTests.Postgres/QuilvianSystemBackend.IntegrationTests.Postgres.csproj
```

Test memerlukan database PostgreSQL yang dapat dijangkau. Tidak ada langkah persiapan manual:
fixture menjalankan migration sendiri sebelum test pertama.

## Database yang dipakai

Connection string **hanya** dibaca dari environment variable `QUILVIAN_BILLING_TEST_DB`. Bila
variable itu kosong, fixture berhenti dengan `BLOCKED_BY_TEST_DB_CONFIGURATION` tanpa membuka
koneksi ke database mana pun. Fallback ke `appsettings.Development.json` sudah dihapus sejak
`RJ-BIL-BE-003`, karena fallback itulah yang dulu membuat `dotnet test` menerapkan migration ke
database dev bersama tanpa ada yang memerintahkannya.

Contoh mengarahkan test ke database test tersendiri:

```
$env:QUILVIAN_BILLING_TEST_DB = "Host=localhost;Port=5432;Username=<role>;Password=<password>;Database=QuilvianBillingTest;"
dotnet test Tests/QuilvianSystemBackend.IntegrationTests.Postgres/QuilvianSystemBackend.IntegrationTests.Postgres.csproj
```

### Database pengembangan personal

Keputusan `RJ-BIL-DEC-019` membolehkan test berjalan terhadap database pengembangan milik
developer sendiri, misalnya `QuilvianNewDevSukma`, lewat opt-in kedua yang harus diisi dengan
sengaja. Nilainya wajib **sama persis** dengan nama database pada connection string:

```
$env:QUILVIAN_BILLING_TEST_DB = "Host=<host>;Port=5432;Username=<role>;Password=<password>;Database=QuilvianNewDevSukma;"
$env:QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL = "QuilvianNewDevSukma"
dotnet test Tests/QuilvianSystemBackend.IntegrationTests.Postgres/QuilvianSystemBackend.IntegrationTests.Postgres.csproj --filter "FullyQualifiedName~NumberSeriesDurabilityTests"
```

Pakai hanya untuk database yang memang milik Anda sendiri. Fixture menjalankan
`Database.Migrate()`, sehingga migration yang tertunda **ikut diterapkan** ke database itu. Beberapa
test juga meninggalkan baris permanen — contohnya deret `PLT_UJI_*` pada `NumNumberSeries`, yang
memang tidak pernah dihapus karena pencacah nomor tidak boleh mundur.

### Pengaman target database

Pemeriksaan dijalankan berurutan sebelum satu perintah pun dikirim ke server:

| Keadaan | Tanpa opt-in personal | Dengan opt-in personal yang cocok |
| --- | --- | --- |
| `QUILVIAN_BILLING_TEST_DB` kosong atau tidak sah | Ditolak | Ditolak |
| Nama database `QuilvianNewDevTim01` | Ditolak | Ditolak |
| Nilai opt-in berbeda dari nama database | — | Ditolak |
| Nama mengandung `prod`, `production`, `live`, `staging`, `stage`, `uat`, atau `shared` | Ditolak | Ditolak — tidak mengenal override |
| Nama mengandung `dev` | Ditolak | **Diteruskan**, dengan peringatan `[BILLING-TEST]` |
| Nama tidak mengandung `test` | Ditolak | **Diteruskan** |
| Nama sah, misalnya `QuilvianBillingTest` | Diteruskan | Diteruskan |

Contoh: `QuilvianNewDevSukma` tanpa opt-in ditolak karena mengandung `dev`. Dengan
`QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL=QuilvianNewDevSukma`, test berjalan. Dengan
`QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL=QuilvianNewDevsukma` — huruf `s` kecil — test ditolak
karena nilainya tidak sama persis.

## Data yang dibuat dan dibersihkan

Setiap test membuat prasyarat sendiri dengan GUID baru, sehingga test tidak saling mengganggu
dan tidak bergantung pada data yang kebetulan sudah ada:

| Urutan dibuat | Entity | Keterangan |
| ---: | --- | --- |
| 1 | `ApplicationUser` | Actor pemrosesan |
| 2 | `MstPatient` | Pasien rujukan encounter |
| 3 | `MstServiceUnit` | Unit layanan rujukan encounter |
| 4 | `TrxPatientEncounter` | Induk folio Billing |

Teardown menghapus seluruhnya kembali, urut dari anak ke induk mengikuti
`DeleteBehavior.Restrict`:

```text
BilProcessingEffect → BilChargeComponent → BilChargeLine → BilFolio
  → TrxPatientEncounter → MstPatient → MstServiceUnit → ApplicationUser
```

Database ditinggalkan dalam keadaan seperti sebelum test dijalankan.

## Mengapa memakai PostgreSQL sungguhan

Acceptance criteria `RJ-BIL-BE-001` menguji invariant yang ditegakkan **database**, bukan
hanya logika aplikasi. Contohnya satu folio kanonik per encounter, yang dijaga unique index
`IX_BilFolio_EncounterId` dengan filter `"IsDelete" = false`.

Provider InMemory tidak menegakkan unique index. Bila dipakai, test folio-uniqueness akan
lulus tanpa membuktikan apa pun — hasilnya justru menyesatkan.

Pengujian lewat HTTP tidak dipakai karena controller memakai `[Authorize]` dengan JWT bearer,
dan `Program.cs` memakai top-level statements tanpa `public partial class Program`. Keduanya
menuntut perubahan pada file yang dipakai seluruh tim, sementara keempat acceptance criteria
tidak memerlukan lapisan transport untuk dibuktikan.

## Susunan folder

```text
Tests/QuilvianSystemBackend.IntegrationTests.Postgres/
├── Infrastructure/
│   ├── BillingTestDatabaseFixture.cs   # resolusi connection string, migration, seed, teardown
│   └── EncounterSeed.cs                # identitas prasyarat satu test
└── Operational/
    └── BillingFolioServiceTests.cs     # test submodule Billing Operational
```

`Infrastructure/` berisi perkakas bersama. Folder di sebelahnya mengikuti nama submodule pada
`Areas/HealthServices/BillingManagement/`, sehingga test baru mudah ditemukan dari lokasi
source-nya.

## Menambah test baru

Letakkan test pada folder yang menyerupai submodule sumbernya. Test untuk
`Areas/HealthServices/BillingManagement/MasterData/` masuk ke `MasterData/`.

Gunakan `BillingTestDatabaseFixture` lewat `IClassFixture<BillingTestDatabaseFixture>`, ambil
prasyarat dengan `SeedEncounterAsync`, dan pastikan setiap seed dibersihkan melalui
`CleanupEncounterAsync` pada `DisposeAsync`. Test yang membuat data tanpa membersihkannya akan
meninggalkan jejak pada database bersama.

## Cakupan yang belum ditutup

Test di sini menutup keempat acceptance criteria `RJ-BIL-BE-001` saja. Skenario berikut berasal
dari `docs/module-blueprints/rawat-jalan/testing/acceptance-test-matrix.md` dan masih menjadi
cakupan `RJ-BIL-BE-009`:

- `BIL_IDEMPOTENCY_CONFLICT` — key sama dengan input material berbeda;
- outcome unknown dan pemulihannya;
- partial component;
- multi-payer allocation;
- financial correction dan maker-checker;
- folio close ketika reconciliation masih pending.
