# Laporan Perubahan Backend — `BE-EXT-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-EXT-02` |
| Judul | [Master Data] Dua data induk perujuk |
| Slice | `S13b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `LAB-DEC-035`, `LAB-COORD-004` — disetujui 2026-09-01; `AC-46`, `AC-50` bergantung padanya |
| Contract version | `erd/data-dictionary.md` bagian 9b.2 dan 9b.3 |
| Dependency | Tidak ada. **Bukan milik Laboratorium** — dikerjakan atas instruksi pemilik modul yang juga kontributor `master-data` |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData`, configuration, migration, project test, registry, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `259d53c`, branch `yoga` |
| Tanggal | 2026-09-04 |
| Status | **`SELESAI`** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `Master / Reference` |
| Pemilik dan prefix registry | Prefix `Mst`, lifecycle `ACTIVE`. Persetujuan dua data induk baru diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001` (`LAB-COORD-004`) |
| Keberlakuan | `NEW CODE` — dua entity, dua configuration, dua `DbSet` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-CFG-001`, `QBE-NAM-002`, `QBE-MOD-002`, `QBE-MOD-003` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-CODE-*`, `QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001` — tidak ada endpoint, service, maupun nomor bisnis pada rilis ini |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |
| Gerbang `BLOCKED BY QBE-MOD-002` | **Sempat aktif, lalu tercabut.** Lihat bagian 3.3 |

---

## 1. Masalah yang diperbaiki

Asal rujukan pasien hidup sebagai teks bebas, sehingga tidak dapat dihitung.

> Sebuah klinik yang sama tercatat sebagai "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan
> "sehat sentosa". Ketiganya terhitung **tiga instansi berbeda**.

Laporan asal rujukan kemudian tidak pernah dapat dipercaya, dan tidak ada cara memperbaikinya
selain menebak mana yang sebenarnya sama. Hal yang sama berlaku untuk nama dokter perujuk.

---

## 2. Proses bisnis

| Sebelum | Sesudah |
| --- | --- |
| Instansi dan dokter perujuk diketik bebas | Dipilih dari daftar data induk |
| Satu klinik dapat punya banyak ejaan | Satu klinik, satu baris, satu kode unik |
| Dokter perujuk tidak punya asal-usul | Dokter tertaut ke instansi tempatnya berpraktik |

Keduanya **data induk global**: Laboratorium, Rawat Jalan, dan IGD sama-sama menerima pasien
rujukan, sehingga pemiliknya Master Data dan bukan salah satu modul pemakainya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../MasterData/Models/MstReferralInstitution.cs` | **Baru** |
| `.../MasterData/Models/MstReferralDoctor.cs` | **Baru** |
| `.../Configurations/HealthServices/MasterData/MstReferralInstitutionConfiguration.cs` | **Baru** |
| `.../Configurations/HealthServices/MasterData/MstReferralDoctorConfiguration.cs` | **Baru** |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` |
| `Migrations/20260904065309_AddLabDisciplineAndReferralMasterData.cs` | **Baru**, bersama `BE-EXT-01` |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Baris `Master / Reference` diperbaiki; keputusannya dicatat bertanggal |
| `QuilvianSystemBackend.csproj` | Dua pengecualian `Compile Remove` dinonaktifkan — lihat bagian 3.4 |
| `Tests/.../MasterData/ReferralMasterDataTests.cs` | **Baru**, empat uji untuk task ini |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Endpoint pengelolaan kedua data induk belum menjadi cakupan siapa pun; keduanya kini dapat dibaca lewat `DbSet` oleh modul mana pun |
| Database | **Aditif.** Dua tabel baru, satu foreign key `Restrict`, tiga index. Dijalankan dua arah pada `QuilvianNewDevYoga` |
| Keamanan/Auth | `NOT APPLICABLE` pada rilis ini |

### 3.3 Gerbang `QBE-MOD-002` yang sempat menutup seluruh data induk baru

Checker QBE menolak kedua entity:

```
[VIOLATION] QBE-MOD-002 | MstReferralInstitution | MstReferralDoctor
Registry owner 'Master / Reference' has non-operational Category 'MASTER / REFERENCE'.
```

Penelusuran menunjukkan **dua** sebab yang menutup jalan sekaligus:

| No | Sebab | Akibat |
| ---: | --- | --- |
| 1 | Nama pemilik pada registry berbunyi `Master / Reference`, sementara foldernya `Areas/HealthServices/MasterData`. Checker mencocokkan alias pemilik dengan segmen folder, dan `master` maupun `reference` tidak pernah sama dengan `masterdata` | Checker melaporkan tidak ada pemilik yang cocok |
| 2 | `Category` berbunyi `MASTER / REFERENCE`, sementara checker hanya mengakui baris ber-`Category` diawali `BUSINESS DOMAIN` sebagai pemberi wewenang entity baru | Sekalipun pemiliknya cocok, wewenangnya tetap ditolak |

Akibat keduanya: **tidak ada satu pun modul yang berwenang membuat data induk baru** —
bukan hanya kedua tabel ini. Gerbang itu menutup seluruh entity `Mst*` yang akan datang, dan
tidak pernah terlihat karena belum ada yang membuat data induk baru sejak checker dipasang.

**Yang tidak dilakukan:** menyimpulkan wewenang sendiri. `QBE-NAM-004` melarangnya, dan
`QBE-MOD-002` menuntut keputusan registry yang tercatat. Pekerjaan dihentikan, keadaannya
disampaikan kepada pemilik modul beserta tiga pilihan penyelesaian, dan yang dipilih adalah
memperbaiki baris registry.

**Yang dilakukan:** baris registry diperbaiki menjadi
`Master / Reference / MasterData | BUSINESS DOMAIN / MASTER / REFERENCE`, dan keputusannya
dicatat bertanggal pada tabel `Catatan perubahan lifecycle` atas nama Yoga Aji Pratama, merujuk
`LAB-REQ-001`. Persetujuan bisnisnya memang sudah ada sejak 2026-09-01; yang belum ada hanya
bentuk registry-nya. Sesudah itu checker `Strict` lulus, dan pemeriksaan ulang atas empat modul
lain membuktikan tidak ada resolusi kepemilikan yang ikut berubah.

### 3.4 Dua pengecualian `csproj` yang mematikan EF Core diam-diam

Saat menyusun migration, EF menghasilkan berkas **70.815 baris** berisi `CreateTable` untuk
seluruh database — tanpa satu pun peringatan. Penyebabnya ada di `QuilvianSystemBackend.csproj`:

```xml
<Compile Remove="Migrations\**\*.Designer.cs" />
<Compile Remove="Migrations\ApplicationDbContextModelSnapshot.cs" />
```

| Berkas yang dikeluarkan | Yang hilang bersamanya |
| --- | --- |
| `ApplicationDbContextModelSnapshot.cs` | Pembanding yang dipakai EF menyusun migration baru. Tanpa itu EF menganggap belum ada schema sama sekali, lalu menyalin seluruh database ke dalam satu migration |
| `Migrations/**/*.Designer.cs` | Atribut `[Migration("...")]`. Tanpa itu **seluruh migration hasil scaffold** — yaitu semua migration sejak 2026-09-01 — tidak terlihat oleh EF. `dotnet ef migrations list` hanya menampilkan **7 dari 127**, dan `database update` menjawab `Done.` tanpa menerapkan apa pun |

Keduanya tidak terasa selama `bin/` masih menyimpan hasil build lama yang memuat kedua berkas
itu. Begitu ada full rebuild, kemampuan EF hilang diam-diam.

Kedua pengecualian dinonaktifkan beserta penjelasan panjang di tempatnya. Sesudah itu
`migrations list` kembali menampilkan 127 migration, dan verifikasi ulang membuktikan dua
migration Laboratorium yang dijalankan lebih awal pada sesi ini memang **sudah** terpasang di
basis data, bukan hanya tampak begitu.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menambah endpoint. Pengelolaan kedua data induk lewat API belum
menjadi cakupan task mana pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 271, Total: 271` | `PASS` | Naik dari 259 |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING` | Kegagalan Billing yang terbuka sejak sebelum sesi ini |
| Checker QBE `Strict` atas 8 berkas | `VIOLATION: 0`, `Final result: PASS` | `PASS` | Sebelum perbaikan registry: `VIOLATION: 2` `QBE-MOD-002` |
| Checker QBE atas empat modul lain | `PASS` | `PASS` | Membuktikan perbaikan registry tidak mengubah resolusi kepemilikan modul lain |
| Kode instansi unik di antara baris hidup | Index unik berfilter `"IsDelete" = false` | `PASS` | `KodeInstansiPerujuk_UnikDiAntaraBarisYangBelumDihapus` |
| Dokter tertaut instansinya | Tepat satu foreign key, `Restrict` | `PASS` | `DokterPerujuk_TertautKeInstansinyaDenganRestrict` |
| Pemisahan dari dokter internal | Nol atribut jadwal, jasa medis, maupun DPJP | `PASS` | `DokterPerujuk_TidakMemilikiAtributDokterInternal` |
| Dapat disimpan dan dibaca kembali | Instansi dan dokternya terbaca lewat navigasi | `PASS` | `InstansiDanDokterPerujuk_DapatDisimpanDanDibacaKembali` |
| `migrations list` sesudah perbaikan `csproj` | **127** migration, dari sebelumnya **7** | `PASS` | Lihat bagian 3.4 |
| Migration maju, mundur, maju | `Done.` ketiganya | `PASS` | `dotnet ef database update` terhadap `QuilvianNewDevYoga` |

Uji manual: `NOT FEASIBLE`.

### 5.1 Yang tidak dijalankan, dan alasannya

| Pemeriksaan | Alasan |
| --- | --- |
| Pengisian daftar instansi perujuk | Data operasional milik rumah sakit, bukan turunan teknis |
| Endpoint pengelolaan kedua data induk | Belum menjadi cakupan task mana pun |
| 52 uji `IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB`; akun aplikasi tanpa hak `CREATEDB` |

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kedua tabel ada | **Terpenuhi** |
| Dokter tertaut ke instansinya | **Terpenuhi**, dengan `Restrict` |
| Keduanya dapat dibaca modul mana pun | **Terpenuhi** — `DbSet` pada `ApplicationDbContext` |

`AC-46` dan `AC-50` bergantung pada task ini dan pada `BE-LAB-08`; keduanya belum dapat
dibuktikan sebelum jalur pendaftarannya ada.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | **(a)** Kedua tabel masih kosong; pengisiannya pekerjaan data. **(b)** Belum ada endpoint pengelolaannya, sehingga pengisian awal harus lewat jalur data langsung |
| Risiko tersisa | **Rendah** untuk kedua tabel. **Perlu diketahui:** perbaikan registry pada bagian 3.3 membuka pembuatan entity `Mst*` baru bagi siapa pun, bukan hanya kedua tabel ini |
| Perubahan sampingan | Dua, keduanya perbaikan cacat yang ditemukan sambil jalan: baris registry `Master / Reference` (bagian 3.3) dan dua pengecualian `Compile Remove` pada `csproj` (bagian 3.4). Keduanya menyentuh berkas milik seluruh tim |
| Interupsi | Satu. Pekerjaan dihentikan pada gerbang `QBE-MOD-002` dan dilanjutkan setelah pemilik modul memilih penyelesaiannya |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. Mengisi daftar instansi dan dokter perujuk. 2. `BE-LAB-08` — pendaftaran pasien laboratorium, yang memakai keduanya |
