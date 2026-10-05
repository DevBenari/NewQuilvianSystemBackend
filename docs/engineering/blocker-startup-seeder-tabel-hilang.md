# Blocker — seeder startup menggagalkan aplikasi pada basis data dev

Ditemukan 5 Oktober 2026 saat menyiapkan runtime proof `BE-OPR-011` pada
`localhost / QuilvianNewDevIkbalFr`.

| Hal | Isi |
|---|---|
| Sifat | **Blocker lingkungan/deployment**, bukan cacat satu modul |
| Dampak | Aplikasi **tidak dapat dijalankan sama sekali** pada basis data dev ini sejak integration HEAD terbaru |
| Pemilik | lintas modul — Platform/deployment, dengan dua seeder milik Rawat Jalan dan Master Data |

## Gejala

Startup mati dengan `Unhandled exception` sebelum Kestrel mengikat port:

```
Npgsql.PostgresException 42P01: relation "public.MstDiagnosisGroup" does not exist
  at QuilvianSystemBackend.Seeders.IcdDiagnosisGroupSeeder.SeedAsync (line 45)
```

Setelah seeder itu dilewati, seeder berikutnya mati dengan sebab yang sama:

```
Npgsql.PostgresException 42P01: relation "public.MstNursingDiagnosisGroup" does not exist
  at ...MasterData.Seeders.MstNursingDiagnosisSeeder.SeedAsync (line 35)
```

Jadi ini **cascade**, bukan satu tabel. Dua sudah teridentifikasi; jumlah sebenarnya belum
dihitung karena tiap percobaan hanya menampakkan satu seeder berikutnya.

## Akar masalah

Basis data dev dibangun dari baseline hasil squash, sehingga tabel yang ditambahkan migration
sesudahnya tidak ada — blocker yang sudah tercatat pada
[`blocker-rantai-migration.md`](blocker-rantai-migration.md). Yang **baru** adalah akibatnya:
sejak kode integration terbaru masuk, kekurangan itu tidak lagi hanya menggagalkan satu endpoint,
melainkan **menggagalkan seluruh startup**.

Tiga hal yang membuatnya fatal, dan ketiganya dapat diperbaiki terpisah dari migrationnya:

### 1. `RunStartupSeederAsync` tidak menangkap kegagalan

```csharp
static async Task RunStartupSeederAsync(string seederName, Func<Task> seed)
{
    var stopwatch = Stopwatch.StartNew();
    await seed();                      // tidak ada try/catch
    ...
}
```

Ada **19** pemanggilan `RunStartupSeederAsync` pada `Program.cs`. Satu pun yang melempar akan
mematikan aplikasi. Seeder data master adalah kenyamanan pengembangan; kegagalannya tidak
seharusnya setara dengan gagal migrasi.

### 2. Dua seeder tidak menghormati saklar `SeedDefaultData:Enabled`

Saklar itu sudah dipakai seeder lain — `ClinicalInstrumentDraftSeeder`,
`HemodialysisMasterDataSeeder`, `LabDisciplineSettingSeeder` semuanya memeriksanya di awal.
Tetapi `IcdDiagnosisGroupSeeder` dan `MstNursingDiagnosisSeeder` **tidak**, sehingga tidak ada
jalan mematikannya lewat konfigurasi.

### 3. `IcdDiagnosisGroupSeeder` hanya dijaga keberadaan berkas CSV

```csharp
var folder = ResolveFolder();
if (folder == null) return;
```

`ResolveFolder` mencari `SeedData/ICD10/icd_diagnosa_dtd.csv` dan `icd10_dtd_mapping.csv` pada
`AppContext.BaseDirectory` **dan** `Directory.GetCurrentDirectory()`. Kedua berkas itu
**tracked** di repositori (dibawa `RJ-DOC-REV-BE-006`), jadi syaratnya selalu terpenuhi pada
setiap pemasangan — bukan hanya pada mesin yang kebetulan punya berkasnya.

## Yang dibutuhkan

Salah satu dari dua jalur, dan keduanya keputusan di luar modul Farmasi/Operasi/Gizi:

**Jalur A — bereskan basis datanya.** Jalankan migration yang tertinggal pada lingkungan dev.
Ini yang benar secara jangka panjang, tetapi menyentuh seluruh rantai migration dan karena itu
berulang kali dikecualikan sebagai pekerjaan tersendiri.

**Jalur B — buat seeder tidak fatal.** Tiga perbaikan kecil yang berdiri sendiri:

1. `RunStartupSeederAsync` menangkap kegagalan, mencatatnya sebagai peringatan, dan melanjutkan —
   kecuali bila lingkungannya produksi;
2. kedua seeder di atas menghormati `SeedDefaultData:Enabled` seperti seeder sejenisnya;
3. `IcdDiagnosisGroupSeeder` memeriksa keberadaan tabelnya lebih dulu, atau dijaga saklar
   konfigurasi alih-alih keberadaan berkas.

Jalur B tidak memperbaiki data yang hilang, tetapi mengembalikan kemampuan menjalankan aplikasi
pada basis data yang belum lengkap — keadaan yang normal pada lingkungan pengembangan.

## Akibat bagi `BE-OPR-011`

Acceptance `403` untuk peran non-SuperAdmin **menuntut aplikasi berjalan**: pipeline otorisasi,
data peran dan izin dari basis data runtime, dan beberapa akun uji. Selama startup mati, runtime
proof itu tidak dapat dikerjakan sama sekali.

Yang **sudah** dibuktikan tanpa runtime, dan tetap berlaku: kontrak izin ketiga laporan Operasi —
13 uji pada `ReportPermissionTests`, termasuk bahwa laporan material menuntut
`OperatingRoomMaterial` dan bukan `OperatingRoomCase`. Yang belum terbukti hanyalah jawaban
runtime-nya.

## Catatan pemeriksaan

Saat menelusuri ini, dua berkas CSV tracked sempat dipindahkan sementara untuk melihat seeder
berikutnya, lalu **dipulihkan utuh** — `git diff` pada `SeedData/` kosong. Saklar
`Security:Authorization:Enabled` dinyalakan lewat **env var proses**, bukan dengan mengubah
`appsettings` yang tracked, jadi tidak ada perubahan konfigurasi yang tertinggal.
