# Authorization Verifier

Gerbang regresi otorisasi yang permanen dan **baca-saja**. Menggantikan invarian yang dulu dijaga
backend test project, tanpa menghidupkan kembali folder `Tests/`.

## Menjalankan

```bash
# 1. bangun project utama lebih dulu
dotnet build ./QuilvianSystemBackend.csproj -c Release

# 2. jalankan verifier
bash tools/authorization-verifier/verify-authorization.sh --configuration Release
```

Keluar dengan `0` bila seluruh invarian terpenuhi, `1` bila ada yang dilanggar, `2` bila assembly
atau berkas governance tidak ditemukan.

Dipanggil lewat `bash`, bukan `./`, supaya tidak bergantung pada bit executable yang sering hilang
saat berkas dibuat di Windows.

## Yang TIDAK dilakukan

Tidak menyalakan aplikasi. Tidak memakai host ASP.NET. Tidak membuka koneksi database. Tidak
memanggil `AccessMenuSeeder`. Tidak menyentuh `SysAccessPolicy`. Tidak menulis apa pun ke dalam
repository.

## Satu-satunya otoritas penemuan

```csharp
PermissionRegistryDescriptor.BuildFromAssembly(typeof(AccessPermissionService).Assembly)
```

Sama persis dengan yang dipakai `AccessMenuSeeder`. Verifier ini **tidak** memakai `grep`, **tidak**
mem-parsing source, dan **tidak** punya algoritma penemuan sendiri. Bila kelak seeder berubah cara
menurunkan identitas, verifier ikut berubah dengan sendirinya — itu memang tujuannya.

## Kenapa runner-nya dibuat saat dijalankan

`QuilvianSystemBackend.csproj` sudah tidak lagi memiliki `DefaultItemExcludes` untuk `Tests\**`.
Akibatnya berkas `.cs` mana pun di dalam repository ikut ter-glob dan dikompilasi ke web project
utama. Runner C# karena itu ditulis ke direktori temp OS saat dijalankan, lalu dihapus — termasuk
artefak build-nya di `bin/`. Tidak ada `.cs` milik verifier yang pernah tinggal di repository.

## Invarian yang ditegakkan

| # | Invarian | Kenapa gagal itu mahal |
|---|---|---|
| 1 | Nol metadata gap | Endpoint terproteksi tanpa kunci registry tidak dapat diberikan admin, sehingga menolak semua orang kecuali SuperAdmin — selamanya |
| 2 | Himpunan fallback kompatibilitas tidak berubah | Endpoint terproteksi baru yang memakai fallback lolos tanpa `[AccessPermission]`. Dibandingkan sebagai **himpunan**, bukan jumlah, supaya pertukaran diam-diam tertangkap |
| 3 | Identitas kanonik tidak ganda dan tidak bertentangan | Resource yang terdaftar pada dua modul meyatimkan policy, karena identitas `SysControllerAccess` adalah (`ModuleId`, `ResourceName`) |
| 4 | Identitas wajib ada di source | Prasyarat `BE-SEC-003` Fase B Tahap 1: 24 kunci wajib terdaftar sebelum satu baris policy pun dibuat |

Jumlah registry (modules/resources/actions) **bukan** kriteria lulus-gagal — angka itu memang
tumbuh saat modul baru ditambahkan. Keduanya hanya dicetak sebagai diagnostik.

## Berkas governance

| Berkas | Isi | Mengubahnya berarti |
|---|---|---|
| `approved-compatibility-fallback.txt` | 69 endpoint warisan yang boleh memakai fallback | Keputusan governance, bukan perbaikan teknis |
| `required-identities.txt` | 24 identitas yang wajib ada | Keputusan pemilik sistem |

Keduanya sengaja berupa berkas data, bukan konstanta di dalam kode, supaya perubahannya terlihat
sebagai diff yang dapat ditinjau.

## Di CI

Terpasang pada `.github/workflows/integration-to-dev.yml`, **sesudah** build backend dan **sebelum**
keputusan migration serta promosi ke DEV. Kandidat yang gagal di sini tidak dipromosikan.

## Latar belakang

`docs/module-blueprints/platform-authorization/evidence/08-authorization-validation-intent-after-tests-removal.md`
mencatat 73 invarian asli beserta alasannya. Verifier ini menutup prioritas 1–3 di sana. Prioritas
4–5 menuntut database dan belum tercakup.
