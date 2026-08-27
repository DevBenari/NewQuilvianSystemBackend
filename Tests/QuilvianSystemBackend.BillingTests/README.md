# QuilvianSystemBackend.BillingTests

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
dotnet test Tests/QuilvianSystemBackend.BillingTests/QuilvianSystemBackend.BillingTests.csproj
```

Test memerlukan database PostgreSQL yang dapat dijangkau. Tidak ada langkah persiapan manual:
fixture menjalankan migration sendiri sebelum test pertama.

## Database yang dipakai

Connection string dicari dengan urutan berikut:

1. Environment variable `QUILVIAN_BILLING_TEST_DB`, bila diisi.
2. Bila kosong, `ConnectionStrings:DefaultConnection` pada `appsettings.Development.json`
   milik project utama.

Urutan ini dipilih agar test dapat dijalankan tanpa menyalin kredensial ke environment
variable atau perintah shell.

Contoh mengarahkan test ke database sendiri:

```
$env:QUILVIAN_BILLING_TEST_DB = "Host=localhost;Port=5432;Username=postgres;Password=rahasia;Database=QuilvianBillingTest;"
dotnet test Tests/QuilvianSystemBackend.BillingTests/QuilvianSystemBackend.BillingTests.csproj
```

### Pengaman target database

| Nama database | Perilaku |
| --- | --- |
| Mengandung `prod` atau `production` | Ditolak. Tidak ada mekanisme override |
| `QuilvianNewDevTim01` | Berjalan, disertai peringatan `[BILLING-TEST]` pada output |
| Lainnya | Berjalan tanpa peringatan |

Database production ditolak karena test membuat lalu menghapus baris.

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
Tests/QuilvianSystemBackend.BillingTests/
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
