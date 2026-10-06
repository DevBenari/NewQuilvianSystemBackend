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
41 uji pada `PermissionMatrixTests` dan `ReportPermissionTests`, termasuk bahwa laporan material menuntut
`OperatingRoomMaterial` dan bukan `OperatingRoomCase`. Yang belum terbukti hanyalah jawaban
runtime-nya.

## Catatan pemeriksaan

Saat menelusuri ini, dua berkas CSV tracked sempat dipindahkan sementara untuk melihat seeder
berikutnya, lalu **dipulihkan utuh** — `git diff` pada `SeedData/` kosong. Saklar
`Security:Authorization:Enabled` dinyalakan lewat **env var proses**, bukan dengan mengubah
`appsettings` yang tracked, jadi tidak ada perubahan konfigurasi yang tertinggal.

## Percobaan ulang 5 Oktober 2026 — pada integration `7787318d`

Dicoba lagi setelah `Ikbal` disinkronkan penuh dengan `QuilvianIntegrationBackend` `7787318d`
(merge PR #234). Build 0 error, uji Operasi 117/117, startup dijalankan dengan
`Security__Authorization__Enabled=true` melalui env var proses.

**Startup tetap mati.** Yang berubah hanya urutannya:

```
Npgsql.PostgresException 42P01: relation "public.MstNursingDiagnosisGroup" does not exist
  at ...MasterData.Seeders.MstNursingDiagnosisSeeder.SeedAsync (line 35)
```

13 seeder berhasil lewat lebih dulu — terakhir `ClinicalInstrumentDraftSeeder`. Blocker pertama
kini `MstNursingDiagnosisSeeder`; `IcdDiagnosisGroupSeeder` bahkan **belum tercapai**, karena
urutannya sesudah seeder ini. Catatan sebelumnya yang menempatkan `IcdDiagnosisGroupSeeder`
sebagai yang pertama berasal dari percobaan ketika dua CSV-nya sedang diparkir sementara.

### Temuan baru: membetulkan startup saja belum cukup

Pembacaan **read-only** pada `localhost / QuilvianNewDevIkbalFr` menunjukkan registry izinnya
lengkap tetapi **kebijakannya kosong**:

| Tabel | Baris |
|---|---|
| `SysControllerAccess` | 451 |
| `SysActionAccess` | 1716 |
| `SysAccessPolicy` | **0** |
| `AspNetUserOrganization` | **0** |
| `AspNetUsers` | 5 |
| `AspNetRoles` | 4 — `SuperAdmin`, `User`, `Manajer Finance`, `Supervisor Finance` |

Akibatnya pada `AccessPermissionService.HasAccessAsync`: tanpa satu pun baris
`AspNetUserOrganization`, tidak ada `DepartmentId`/`PositionId` yang bisa dicocokkan; tanpa satu
pun baris `SysAccessPolicy`, tidak ada `IsAllowed` yang bisa bernilai benar. Maka **setiap**
pengguna non-SuperAdmin akan ditolak `403` pada **setiap** endpoint, di seluruh sistem.

Itu berarti sisi `403` akan terpicu, tetapi **hampa** — ia tidak membuktikan otorisasinya
selektif, dan justru sisi `200` yang menjadi **mustahil** dibuktikan. Acceptance `BE-OPR-011`
menuntut keduanya. Jadi memperbaiki startup adalah syarat perlu, bukan syarat cukup.

### Yang dibutuhkan untuk menutup `403` runtime

Selain Jalur A atau B di atas, pemilik lingkungan masih perlu menyediakan:

1. baris `AspNetUserOrganization` bagi akun uji, sehingga tiap akun punya
   `DepartmentId` + `PositionId`;
2. baris `SysAccessPolicy` yang memberi `IsAllowed` pada sebagian pasangan
   `resource:action` Operasi dan menahan sisanya — tanpa kontras itu, `403` tidak membedakan
   apa pun;
3. jabatan dan departemen klinis pada master data; empat role yang ada sekarang tidak memuat
   peran bedah, anestesi, maupun perawat.

Akun dan role uji **tidak** dibuat dalam percobaan ini: aplikasinya tidak dapat dijalankan,
sehingga mekanisme sah pembuatannya — endpoint administrator `role-access/policies` — juga tidak
dapat dipakai. Membuatnya langsung lewat SQL akan menjadi data palsu yang tidak membuktikan
pipeline apa pun.

---

## ✅ Dibuka 6 Oktober 2026 — Jalur B langkah 1

**Aplikasi kini dapat dijalankan.** Yang dikerjakan hanya langkah pertama Jalur B, yaitu membuat
`RunStartupSeederAsync` tidak fatal. Dua langkah lainnya — kedua seeder menghormati
`SeedDefaultData:Enabled`, dan `IcdDiagnosisGroupSeeder` memeriksa tabelnya alih-alih berkas CSV —
**tidak dikerjakan**, karena keduanya menyentuh seeder milik modul lain.

### Bentuknya: opt-in, bawaan tidak berubah

```
Seeders:ContinueOnFailure   // bawaan false
```

Tiga sifat yang disengaja:

1. **Opt-in dan bawaannya `false`.** Perilaku setiap lingkungan yang sudah berjalan tidak berubah
   sedikit pun oleh perubahan ini. Yang membutuhkannya harus memintanya secara sadar.
2. **Diabaikan di Production**, mengikuti pola `Security:Authorization:Enabled`. Rumah sakit yang
   berjalan dengan data master separuh terisi lebih berbahaya daripada rumah sakit yang menolak
   boot: yang pertama gagal saat petugas sedang melayani pasien, dan sebabnya tidak terlihat.
3. **Dicatat `Error`, bukan `Warning`**, per seeder yang gagal, ditambah satu ringkasan sesudah
   seluruh seeder selesai. Yang berubah hanya bahwa startup tidak ikut mati — bukan bahwa
   kegagalannya menjadi wajar. Satu baris di tengah ratusan baris log startup terlalu mudah
   terlewat, karena itu ringkasannya menyebut nama seeder dan menunjuk `__EFMigrationsHistory`
   sebagai tempat memeriksa lebih dulu.

### Catatan polaritas

`Program.cs` kini memuat dua aturan yang tampak berlawanan, dan keduanya benar:

| Pemeriksaan | Di luar Production | Di Production |
|---|---|---|
| `PermissionRegistryValidator` | **menghentikan** startup | mencatat `Critical`, lanjut |
| Seeder data master | lanjut bila diminta | **menghentikan** startup |

Bedanya pada apa yang diperiksa. Validator memeriksa **anotasi pada source**, jadi kegagalannya
cacat kode yang harus tertangkap sebelum rilis. Seeder data master gagal karena **basis datanya
belum lengkap**, dan itu keadaan normal pada lingkungan pengembangan, bukan cacat kode.

### Bukti runtime

Dijalankan pada `localhost / QuilvianNewDevIkbalFr` dengan
`Seeders__ContinueOnFailure=true` dan `Security__Authorization__Enabled=true`, keduanya lewat env
var proses:

| Hal | Hasil |
|---|---|
| `Unhandled exception` | **0** — sebelumnya startup mati di sini |
| Seeder gagal dan dilewati | **3**: `IcdDiagnosisGroupSeeder`, `MstNursingDiagnosisSeeder`, `MstDailyNursingActionSeeder` |
| Ringkasan akhir tercatat | ya, menyebut ketiganya |
| `GET /swagger/index.html` | **200** |
| `POST /api/v1/Auth/login` badan kosong | **400** — validasi berjalan, bukan mati |
| `GET .../operating-room-management/reports/operations` tanpa login | **401** |
| `GET .../operating-room-management/reports/materials` tanpa login | **401** |
| `GET .../operating-room-management/cases` tanpa login | **401** |

### Akar masalahnya tetap ada

Perubahan ini **tidak** memulihkan data yang hilang. Angka sebenarnya, dibaca read-only:

| | Jumlah |
|---|---|
| Migration di source | **287** |
| Tercatat di `__EFMigrationsHistory` | **48** |
| **Belum pernah dijalankan** | **239** |

Migration terakhir yang tercatat `20260929000000_AddOperatingRoomOutboxEventContract`; source sudah
sampai `20261005090000`. Dua migration yang menyediakan tabel pemicu blocker ini, keduanya belum
tercatat:

- `20260928093849_AddMasterNursingDiagnosisSdki` → `MstNursingDiagnosis`, `MstNursingDiagnosisGroup`
- `20261001043557_AddMstDiagnosisGroup` → `MstDiagnosisGroup`

Selain itu, beberapa **background worker** kini mencatat galat berulang atas tabel yang juga belum
ada — `AccAccountingEvent` dan `MstBillingSyncPolicy`. Galat itu tidak mematikan host, tetapi
membuat log ramai dan pekerjaan latar belakangnya tidak berjalan. Itu bagian dari kekurangan
migration yang sama, bukan akibat perubahan ini, dan milik pemilik modul masing-masing.

**Jalur A tetap dibutuhkan.** Yang berubah hanya bahwa ia tidak lagi memblokir seluruh pekerjaan
modul lain sementara ia belum dikerjakan.
