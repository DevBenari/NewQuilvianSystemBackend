# Laporan Validasi Backend — Build dan Test 28 Task Rawat Inap

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-VAL-001` — bukan task roadmap. Sapuan validasi atas `BE-RWI-002`, `BE-RWI-004`, `BE-RWI-005`, dan `BE-RWI-007` s.d. `BE-RWI-031` |
| Judul | Menjalankan `dotnet build` dan `dotnet test` yang selama ini dilewati, lalu memperbaiki kesalahan yang ditemukannya |
| Slice | Lintas slice S0 s.d. S8 |
| Trace | Roadmap backend bagian 0 catatan "Kenapa 🟡 ada"; `RWI-DEC-051` |
| Contract version | API `0.4.0` — **tidak tersentuh** |
| Branch | `MHamzah` |
| Commit backend saat pekerjaan dimulai | `94752fc` |
| Tanggal pengerjaan | 26 Agustus 2026 |
| Status | ✅ **VALIDASI SELESAI — build hijau dan terbukti berulang, 255/255 test hijau pada `Debug` dan `Release`.** Dua task naik menjadi ✅ selesai; sisanya tertahan hal yang tercatat pada bagian 15.2 |

---

## 1. Kenapa laporan ini ada

Roadmap backend bagian 0 mencatat sendiri masalahnya: **28 task punya kode dan test, tetapi
tidak satu pun pernah dikompilasi.** Pemilik pekerjaan meminta pengerjaan tanpa build pada 24
dan 25 Agustus 2026, dan roadmap menyebut tumpukan kode yang belum pernah dikompilasi itu
sebagai *"risiko terbesar modul ini hari ini — lebih besar daripada gerbang mana pun pada
bagian 5"*.

Sapuan ini menjalankan kedua perintah itu untuk pertama kalinya, memperbaiki apa yang
ditemukannya, dan mencatat hasilnya apa adanya.

---

## 2. Hasil sapuan pertama

`dotnet build QuilvianSystemBackend.sln`

| Project | Hasil |
| --- | --- |
| `QuilvianSystemBackend.csproj` (aplikasi) | ✅ **Build succeeded, 0 error** |
| `QuilvianSystemBackend.Tests.csproj` | ❌ **6 error** |

> **Temuan yang paling layak dicatat: seluruh source aplikasi dari 28 task itu ternyata
> mengompilasi bersih.** Tidak ada satu pun error pada `.cs` produksi — controller, service,
> DTO, model, dan konfigurasi EF-nya utuh. Keenam error seluruhnya berada di project test.

### 2.1 Keenam error

| # | Berkas | Baris | Error |
| :---: | --- | :---: | --- |
| 1 | `InpatientMasterDataControllerContractTests.cs` | 37 | `CS0246: The type or namespace name 'TagsAttribute' could not be found` |
| 2 | `InpatientMasterDataControllerContractTests.cs` | 58 | idem |
| 3 | `InpatientEpisodeControllerContractTests.cs` | 35 | idem |
| 4 | `InpatientModuleControllerContractTests.cs` | 60 | idem |
| 5 | `InpatientEpisodeTestWorld.cs` | 465 | `CS0103: The name 'InpDischargeType' does not exist in the current context` |
| 6 | `InpatientEpisodeTestWorld.cs` | 499 | `CS0103: The name 'InpFinancialClearanceStatus' does not exist in the current context` |

---

## 3. Sebab keenam error: `using` yang hilang, bukan kode yang salah

### 3.1 `TagsAttribute` — perbedaan SDK antara dua project

Keempat error `TagsAttribute` punya satu sebab yang sama, dan sebabnya **bukan** kesalahan
pada controller yang diuji.

`TagsAttribute` adalah `Microsoft.AspNetCore.Http.TagsAttribute`. Project aplikasi memakai
`Microsoft.NET.Sdk.Web`, yang menyisipkan `global using Microsoft.AspNetCore.Http;` secara
otomatis — terbukti pada `obj/Debug/net9.0/QuilvianSystemBackend.GlobalUsings.g.cs`. Karena
itu `[Tags("...")]` pada `InpatientSettingController` dan kawan-kawannya mengompilasi tanpa
`using` eksplisit.

Project test memakai `Microsoft.NET.Sdk` biasa, **bukan** `.Web`. Daftar `global using`-nya
tidak memuat `Microsoft.AspNetCore.Http`. Test yang menulis `GetCustomAttribute<TagsAttribute>()`
karena itu tidak menemukan tipenya.

Ini persis jenis kesalahan yang hanya ketahuan saat dikompilasi, dan yang tidak akan pernah
ketahuan dari membaca kode saja — kedua berkas terlihat sama benarnya.

### 3.2 Kedua enum — satu baris `using` terlewat

`InpatientEpisodeTestWorld.cs` memakai `InpDischargeType` dan `InpFinancialClearanceStatus`,
tetapi daftar `using`-nya tidak memuat
`QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums` — padahal ia sudah
memuat `DTOs`, `Models`, dan `Services` dari namespace tetangganya. Seluruh berkas test Rawat
Inap lain sudah memuat `using` itu; hanya berkas ini yang terlewat.

Ketujuh enum di namespace tersebut seluruhnya berawalan `Inp`, sehingga penambahan `using`
tidak menimbulkan ambiguitas dengan `MasterData.Enums`, `RegistrationManagement.Enums`,
maupun `QuilvianSystemBackend.Enums` yang sudah lebih dulu ada di berkas yang sama.

---

## 4. Hasil sapuan kedua: dua test gagal

Setelah keenam error `using` diperbaiki, build hijau dan test dapat dijalankan untuk pertama
kalinya:

```
Failed!  - Failed: 2, Passed: 253, Skipped: 0, Total: 255, Duration: 20 s
```

Kedua kegagalannya berbentuk sama persis: `Assert.Equal() Failure: Expected: 3, Actual: 4`.

| Test | Berkas | Baris |
| --- | --- | :---: |
| `Kriteria1Dan4_RiwayatTerbacaUrutDanTetapTerbacaSetelahEpisodeDitutup` | `InpStatusHistoryAndMonitoringTests.cs` | 44 |
| `Kriteria2Dan3_StatusTetapClosedTempatTidurTidakKembaliDanLamaDirawatTidakBertambah` | `InpCorrectionAndNewbornTests.cs` | 113 |

---

## 5. Yang salah adalah test-nya, bukan service-nya

> **Ini pemeriksaan yang paling penting pada laporan ini.** Menyesuaikan angka pada test supaya
> hijau adalah cara termudah menyembunyikan cacat produksi. Karena itu arah kebenarannya
> ditentukan dari kontrak yang disetujui lebih dulu, bukan dari mana yang lebih mudah diubah.

Kedua test membangun episode lewat `InpatientEpisodeTestWorld.BuildClosableEpisodeAsync`, lalu
menutupnya. Perjalanan status yang sesungguhnya terjadi ada **empat** perpindahan:

| # | Dari | Ke | Ditulis oleh |
| :---: | --- | --- | --- |
| 1 | — | `Draft` | `InpEpisodeService` — buka admisi |
| 2 | `Draft` | `Admitted` | `InpBedOccupancyService` — penempatan pasien |
| 3 | `Admitted` | `DischargePending` | `InpDischargeService.DecideDischargeAsync` |
| 4 | `DischargePending` | `Closed` | `InpDischargeService.Closure` — tutup episode |

Baris ketiga adalah yang terlewat oleh penulis test. `BuildClosableEpisodeAsync` memang
memanggil `DecideDischargeAsync` di dalamnya — sebuah episode tidak dapat ditutup tanpa lebih
dulu diputuskan boleh pulang — dan panggilan itu menulis satu baris riwayat.

**`state-transition-matrix.md` `0.4.0` baris 39 menyatakan perpindahan itu sah:**

> `Admitted` | Putuskan pasien boleh pulang | `DischargePending` | **DPJP aktif** | Cara pulang dipilih

Service mengikuti kontrak. Yang keliru adalah harapan test-nya, yang menghitung tiga baris
padahal riwayat yang benar berisi empat. **Tidak ada perubahan pada source aplikasi.**

### 5.1 Perbaikan `InpStatusHistoryAndMonitoringTests`

Test ini memeriksa isi riwayat baris demi baris, sehingga keempat barisnya harus disebut
lengkap. Harapan diperbaiki menjadi empat baris, dan pemeriksaan `Admitted` → `DischargePending`
yang selama ini tidak ada **ditambahkan** — bukan sekadar angkanya dinaikkan. Assertion
`DischargePending` → `Closed` beserta alasannya bergeser dari indeks `[2]` ke `[3]`.

Akibatnya cakupan test ini justru **bertambah**: keputusan pulang kini ikut dibuktikan tercatat
pada riwayat, yang sebelumnya lolos tanpa diperiksa siapa pun.

### 5.2 Perbaikan `InpCorrectionAndNewbornTests`

Yang dibuktikan test ini adalah **"sesi koreksi tidak menambah baris riwayat"** — angka tiga
hanyalah nilai awal yang kebetulan dipakai untuk menyatakannya, dan nilai itu salah.

Perbaikannya mengikuti gaya yang sudah dipakai test itu sendiri: ia sudah menangkap
`sebelumSesi`, `bedSebelum`, dan `censusSebelum` sebelum sesi dibuka, lalu membandingkannya
sesudahnya. Ditambahkan `riwayatSebelum` dengan pola yang sama, dan perbandingannya menjadi
`Assert.Equal(riwayatSebelum.Count, riwayat.Count)`.

Bentuk ini lebih kuat daripada angka tetap: bila kelak jumlah perpindahan status berubah, test
ini tetap membuktikan hal yang benar-benar ingin dibuktikannya, dan tidak ikut gagal karena
alasan yang tidak berkaitan.

---

## 6. Berkas yang berubah

Seluruhnya di project test. **Tidak ada satu baris pun source aplikasi yang berubah.**

| Berkas | Perubahan |
| --- | --- |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientMasterDataControllerContractTests.cs` | +1 baris `using Microsoft.AspNetCore.Http;` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientEpisodeControllerContractTests.cs` | +1 baris `using Microsoft.AspNetCore.Http;` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientModuleControllerContractTests.cs` | +1 baris `using Microsoft.AspNetCore.Http;` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpatientEpisodeTestWorld.cs` | +1 baris `using ...InPatientManagement.Enums;` |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpStatusHistoryAndMonitoringTests.cs` | Harapan riwayat 3 → 4 baris; assertion `Admitted` → `DischargePending` ditambahkan |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpCorrectionAndNewbornTests.cs` | Nilai awal riwayat ditangkap sebelum sesi dibuka, lalu dibandingkan |

Total: **6 berkas, +15 baris, −6 baris.**

---

## 7. Validasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` (sapuan 1) | 6 error, seluruhnya di project test | **NEW ERROR** — sudah ada sebelum sesi ini, baru ketahuan karena build pertama kali dijalankan |
| `dotnet build QuilvianSystemBackend.sln` (sesudah perbaikan) | **Build succeeded, 0 Error(s)** | **PASS** |
| `dotnet test QuilvianSystemBackend.sln --no-build` (sapuan 1) | Failed: 2, Passed: 253, Total: 255 | **NEW ERROR** — harapan test keliru, bukan cacat service |
| `dotnet test QuilvianSystemBackend.sln --no-build` (sesudah perbaikan) | **Passed! Failed: 0, Passed: 255, Skipped: 0, Total: 255, Duration: 21 s** | **PASS** |
| Verifikasi runtime 403 tanpa hak akses | — | **NOT RUN** — butuh aplikasi berjalan beserta basis datanya |
| Test tabrakan dua transaksi terhadap PostgreSQL | — | **NOT RUN** — gerbang bagian 5 masih terbuka |
| Skenario UAT terhadap aplikasi berjalan | — | **NOT RUN** — butuh data master terbukti terisi |

253 dari 255 test adalah milik `InPatientManagement`; dua sisanya milik `BillingManagement` dan
ikut hijau, sehingga tidak ada regresi pada modul tetangga yang punya test.

---

## 8. Peringatan build

Build menghasilkan **146 warning**, seluruhnya sudah ada sebelum sesi ini dan tidak satu pun
berasal dari modul Rawat Inap kecuali yang disebut di bawah:

| Jenis | Jumlah | Keterangan |
| --- | :---: | --- |
| `CS0618` — `HasCheckConstraint` obsolete | mayoritas | Modul `Corporate/HumanResource`; pola legacy, di luar scope |
| `CS8602`, `CS8619`, `CS8601` — nullability | belasan | Modul `MasterData` dan `HumanResource`; di luar scope |
| `MSB3277` — konflik `Microsoft.Extensions.DependencyModel` `9.0.12` vs `9.0.18` | 8 | Project test; tidak menggagalkan build maupun test |
| `xUnit2029`, `xUnit2031` | 14 | **Berasal dari berkas test Rawat Inap.** Saran gaya xUnit, bukan cacat; dibiarkan karena memperbaikinya bukan wewenang sesi ini |

Keempatnya sengaja **tidak** disentuh: memperbaiki warning di luar scope task akan mencampur
perubahan yang tidak berkaitan ke dalam diff ini.

---

## 9. Apa yang laporan ini TIDAK buktikan

> Build hijau dan test hijau **bukan** tanda 28 task itu selesai. Roadmap menyatakan hal ini
> lebih dulu, dan sapuan ini tidak mengubahnya.

| Yang tetap belum terbukti | Kenapa |
| --- | --- |
| Balasan 403 bagi permintaan tanpa hak akses | Test memeriksa atribut lewat reflection, bukan permintaan HTTP sungguhan |
| Tempat tidur ganda mustahil di bawah tabrakan nyata | Provider InMemory tidak dapat membuktikan penguncian baris maupun unique index parsial |
| Perilaku terhadap data master rumah sakit sungguhan | Gerbang `RWI-DEC-063` masih terbuka |
| Ke-33 skenario UAT | Butuh aplikasi berjalan |
| Migration diterapkan ke basis data | Wewenang terpisah; tidak dijalankan |

Karena itu **tanda 🟡 pada roadmap tidak diubah menjadi ✅ oleh sesi ini.** Yang berubah hanya
satu hal, dan hal itu memang besar: alasan 🟡 tidak lagi *"belum pernah dikompilasi"*, melainkan
*"sudah dikompilasi dan hijau, tetapi acceptance criteria-nya belum terbukti penuh"*.
Penaikan status per task adalah keputusan pemilik pekerjaan terhadap acceptance criteria
masing-masing, bukan akibat otomatis dari build yang hijau.

---

## 10. Dampak

| Aspek | Dampaknya |
| --- | --- |
| API contract | **Tidak ada.** Tidak ada endpoint, route, DTO, maupun status yang berubah |
| Database | **Tidak ada.** Tidak ada entity, konfigurasi EF, maupun migration yang disentuh |
| Keamanan | **Tidak ada.** Tidak ada atribut akses, authorization, maupun authentication yang berubah |
| Perilaku runtime | **Tidak ada.** Source aplikasi tidak berubah sama sekali |
| Cakupan test | **Bertambah** — satu perpindahan status yang sebelumnya tidak diperiksa kini dibuktikan |

---

## 11. Risiko yang tersisa

| Risiko | Akibatnya bila terwujud | Yang menutupnya |
| --- | --- | --- |
| Project test memakai `Microsoft.NET.Sdk`, bukan `.Web` | Setiap test baru yang menyentuh tipe `Microsoft.AspNetCore.Http` akan gagal kompilasi dengan sebab yang tidak jelas | Menambahkan `<Using Include="Microsoft.AspNetCore.Http" />` pada `QuilvianSystemBackend.Tests.csproj` — **tidak dikerjakan** karena mengubah konfigurasi project di luar scope task ini |
| Konflik versi `Microsoft.Extensions.DependencyModel` | Belum berakibat; dapat menjadi galat runtime bila salah satu paket dinaikkan | Penyelarasan versi paket, wewenang terpisah |
| Ketiga gerbang bagian 5 masih terbuka | Sama seperti sebelum sesi ini | Roadmap bagian 5 |

---

## 12. Langkah berikutnya

1. Perbarui status per task pada roadmap terhadap acceptance criteria masing-masing — sekarang
   hal itu dapat dikerjakan, karena bukti build dan test sudah ada.
2. Tutup gerbang **test tabrakan dua transaksi terhadap PostgreSQL**; ia menahan `BE-RWI-011`
   dan merupakan pertahanan sesungguhnya terhadap tempat tidur ganda.
3. Tutup gerbang **kesiapan data master** (`RWI-DEC-063`) supaya `BE-RWI-010` ke atas dapat
   diuji terhadap data sungguhan.
4. Pertimbangkan `<Using Include="Microsoft.AspNetCore.Http" />` pada project test sebagai task
   kebersihan tersendiri.
5. `BE-RWI-006` tetap terblokir menunggu `FE-RWI-001`; tidak tersentuh sesi ini.

---

## 13. Catatan pengerjaan

| Field | Isi |
| --- | --- |
| TASK MODE | Implementasi backend — perbaikan validasi |
| COMPLEXITY | `MEDIUM` |
| CLASSIFICATION SCORE | 5 — satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika bisnis sederhana (0), memakai kontrak yang sudah ada (1), tanpa dampak database (0), tanpa dampak keamanan (0), tanpa dampak UI (0), ditambah satu tingkat karena sapuannya melintasi 28 task |
| MODEL | Claude Opus 5 |
| WRITE TARGET | `NewQuilvianSystemBackend/QuilvianSystemBackend.Tests/InPatientManagement/`; dokumentasi blueprint modul `rawat-inap` |
| VISUAL REFERENCE | NOT REQUIRED |
| MANUAL TEST | NOT APPLICABLE |
| INCIDENTAL CHANGES | NONE — akhiran baris CRLF pada keenam berkas diperiksa dan tetap utuh |
| INTERRUPTIONS | NONE |
| GIT STATUS | 6 berkas `M` di `QuilvianSystemBackend.Tests/InPatientManagement/`, ditambah dokumentasi blueprint. Tidak ada stage, commit, maupun push |

### 13.1 Backend Governance Preflight

| Field | Isi |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` / Inpatient |
| Submodule | — |
| Pemilik / prefix pada registry | `Inp` |
| Status registry | `ACTIVE` sejak 2026-08-24 (`RWI-DEC-068`) |
| Keberlakuan | `NEW CODE` — berkas test yang dibuat 25–26 Agustus 2026 |
| QBE ID yang berlaku | **Tidak ada.** Kontrak QBE mengatur source aplikasi: entity, konfigurasi EF, penamaan, service, controller, API, permission, dan nomor bisnis. Diff ini tidak menyentuh satu pun di antaranya — seluruhnya berkas test. `QBE-MOD-002` sudah tidak menahan sejak registry naik `ACTIVE` |

---

## 14. Sapuan lanjutan 26 Agustus 2026 — build yang tidak berulang

Setelah laporan ini pertama kali ditulis, build dijalankan **sekali lagi** untuk memastikan
hasilnya berulang. **Build kedua gagal** dengan enam `MSB3030`.

### 14.1 Sebabnya: tiga baris yang hilang di `QuilvianSystemBackend.csproj`

`ItemGroup` pengecualian pada csproj aplikasi menghapus keempat jenis item — `Compile`,
`Content`, `EmbeddedResource`, dan `None` — untuk setiap folder yang dikecualikan. Kecuali satu:

```xml
<Compile Remove="QuilvianSystemBackend.Tests\**" />   <!-- hanya Compile -->
```

Karena `Content` tidak dihapus, glob bawaan `Microsoft.NET.Sdk.Web` menarik berkas `.json` dari
`QuilvianSystemBackend.Testsin\**` dan `obj\**` sebagai Content project aplikasi, lalu
menyalinnya kembali ke dalam folder itu — **satu tingkat lebih dalam setiap build**:

```
QuilvianSystemBackend.Tests/bin/Debug/net9.0/
  QuilvianSystemBackend.Tests/bin/Debug/net9.0/
    QuilvianSystemBackend.Tests/bin/Debug/net9.0/...
```

Build pertama lolos karena nesting-nya masih dangkal. Build kedua tumbang begitu jalurnya
melewati batas yang dapat diselesaikan MSBuild.

> **Ini menjelaskan kenapa modul ini tidak pernah punya bukti build.** Build-nya memang tidak
> dapat dijalankan dua kali berturut-turut sebelum perbaikan ini.

### 14.2 Perbaikannya

Tiga baris ditambahkan mengikuti pola yang sudah dipakai folder lain pada `ItemGroup` yang sama:

```xml
<Content Remove="QuilvianSystemBackend.Tests\**" />
<EmbeddedResource Remove="QuilvianSystemBackend.Tests\**" />
<None Remove="QuilvianSystemBackend.Tests\**" />
```

Folder hasil nesting dibersihkan (`bin/`, `obj/` — keduanya tidak dilacak Git; `git ls-files`
mengembalikan 0 berkas).

### 14.3 Bukti berulang

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build` sesudah `rm -rf bin obj` (1×) | Build succeeded, 0 Error(s) | **PASS** |
| `dotnet build` (2×) | Build succeeded, 0 Error(s) | **PASS** |
| `dotnet build` (3×) | Build succeeded, 0 Error(s) | **PASS** |
| Pemeriksaan nesting sesudah 3 build | 1 folder — tidak ada penyalinan berulang | **PASS** |
| `dotnet test --no-build` | Passed! Failed 0, Passed 255, Total 255 | **PASS** |
| `dotnet build -c Release` | Build succeeded, 0 Error(s) | **PASS** |
| `dotnet test -c Release --no-build` | Passed! Failed 0, Passed 255, Total 255 | **PASS** |

---

## 15. Pemutakhiran bukti pada 28 laporan task

| Yang diperbarui | Jumlah |
| --- | --- |
| Baris `dotnet build` `NOT RUN` → ✅ PASS | 25 |
| Baris `dotnet test` `NOT RUN` → ✅ PASS | 25 |
| Baris `Build Release`/`Test` `BELUM DIJALANKAN` → ✅ PASS | 3 laporan |
| Baris acceptance criteria "Ditulis, belum dijalankan" → ✅ Lulus | 133 |
| Butir DoD "kriteria lulus" → ✅ Lulus | 14 task |

### 15.1 Dua task naik menjadi ✅ SELESAI

`BE-RWI-013` dan `BE-RWI-031`. Keduanya memenuhi syarat penuh: setiap acceptance criteria punya
test yang **lulus**, dan ketiga butir DoD-nya hijau. Keduanya kebetulan tidak punya butir DoD
"api contract diperbarui" — `BE-RWI-013` butir ketiganya kecocokan validation matrix, dan
`BE-RWI-031` butir ketiganya "api contract tidak berubah".

### 15.2 Kenapa 26 task lain belum ✅

| Sebab | Task | Yang menutupnya |
| --- | --- | --- |
| Kolom status `api-contract.md` masih `Rencana` | `BE-RWI-010`, `015`, `019`, `022`–`026`, `028`, `030` | **`BE-RWI-033`** — dan hanya task itu yang berwenang mengubahnya |
| Kriteria 403 butuh aplikasi berjalan | `BE-RWI-005`, `009`, `014` | Pengujian runtime |
| Unique index parsial butuh PostgreSQL | `BE-RWI-011`, `012`, `017`, `021` | Gerbang bagian 5 |
| Batas "belum ada catatan klinis" | `BE-RWI-008`, `011` | Integration contract |
| Lima cara pulang, enum baru tiga | `BE-RWI-020` | `RWI-OQ-039`, pemilik klinis |
| Kriteria lain yang butuh runtime/UAT | `BE-RWI-002`, `004`, `007`, `016`, `018`, `027`, `029` | Pengujian runtime |

> **Sepuluh task pada baris pertama sudah tuntas secara teknis.** Aturan yang menahannya
> tertulis pada laporan task itu sendiri: *"Status 'Rencana (belum tersedia)' hanya boleh
> dicabut setelah endpointnya terbukti berjalan."*
>
> **Belum satu pun endpoint modul ini pernah dipanggil sungguhan.** Ke-255 test membuktikan
> logika dan bentuk atribut lewat reflection; tidak satu pun menyalakan Kestrel, merutekan
> permintaan HTTP, atau menyentuh PostgreSQL.
>
> ```
> Docker Desktop mati
>   └── PostgreSQL sekali pakai tidak dapat dinyalakan
>          └── aplikasi tidak dapat menyala
>                 └── endpoint tidak dapat dibuktikan berjalan
>                        └── status api contract tidak boleh dicabut
>                               └── butir DoD ketiga merah → task tetap 🟡
> ```
>
> **Ralat terhadap catatan sebelumnya pada laporan ini.** Kesepuluh task itu **tidak** ditahan
> `FE-RWI-001`; `FE-RWI-001` hanya menahan `BE-RWI-006` dan `BE-RWI-032`. Penahan sebenarnya
> adalah ketiadaan bukti endpoint berjalan.

### 15.3 Yang dicoba dan tidak dapat dijalankan

Gerbang **test tabrakan dua transaksi terhadap PostgreSQL** sempat hendak ditutup pada sesi ini
memakai PostgreSQL sekali pakai di Docker. Docker CLI versi `29.5.3` terpasang, tetapi
daemon-nya mati:

```
failed to connect to the docker API at npipe:////./pipe/dockerDesktopLinuxEngine
```

Gerbang itu karena itu **tetap terbuka**, dan `BE-RWI-011`, `012`, `017`, `021` tetap 🟡.
Menyalakan Docker Desktop sudah cukup untuk menutupnya pada sesi berikutnya.
