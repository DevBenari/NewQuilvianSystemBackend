# Laporan Perubahan Backend — `PLT-BE-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PLT-BE-005` |
| Judul | Layar pemantauan deret dapat dibaca administrator |
| Slice | `PLT-SLICE-01` — gelombang `MVP-2`, **`SHOULD HAVE`** |
| Roadmap | `docs/module-blueprints/platform/roadmap/backend-roadmap.md` §5 |
| Trace | `FR-PLT-012`, `FR-PLT-013` · `INV-PLT-001`, `INV-PLT-002` · `DEC-PLT-005` · `QBE-CODE-006` |
| Contract version | `v1` — ✅ **`approved`** (`Sukma Giri Pratama` / `2026-09-09`) |
| Dependency | `PLT-BE-003` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository tunggal (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sederhana (0), memakai kontrak yang sudah ada (1), hanya perilaku query (1), keamanan berkaitan tetapi bukan intinya (1), UI nol (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Platform/NumberSeriesManagement/**`, `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/**`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `2eec97d` cabang `sukmagp` |
| Tanggal | `2026-09-10` |
| Status | ✅ **`SELESAI`**. Keempat acceptance criteria terbukti; seluruh butir Definition of Done terpenuhi dan **dipindai**, bukan diyakini. `dotnet test` **54 lulus, 0 gagal**. Migration `PLT-BE-002` **belum dijalankan**, sehingga endpoint ini belum dapat dipanggil di lingkungan mana pun — dinyatakan apa adanya |

---

## 1. Masalah yang diperbaiki

Sejak `PLT-BE-003`, sistem sudah menerbitkan nomor bisnis lewat satu mesin bersama, dan
pencacahnya tersimpan rapi di tabel `NumNumberSeries`. Tetapi **tidak ada satu pun cara melihat
isinya** selain membuka database langsung.

Akibat nyatanya muncul ketika ada keluhan. Seorang petugas melapor "nomor order darah saya
melompat dari 122 ke 124". Untuk menjawabnya, administrator perlu tahu berapa nilai pencacah
sekarang dan kapan alokasi terakhir terjadi. Tanpa layar, satu-satunya jalan adalah meminta
seseorang yang punya akses database menjalankan query — yang berarti menunggu, dan memberi akses
database kepada orang yang sebenarnya hanya perlu membaca satu angka.

**Yang bukan masalah, dan sengaja tidak diperbaiki.** Lompatan 122 ke 124 itu sendiri **bukan
kerusakan**. Nomor 123 hangus karena pekerjaan yang memintanya dibatalkan, dan itu justru perilaku
yang benar (`DEC-PLT-008`). Layar ini ada untuk **menjelaskan** lompatan itu, bukan untuk
merapikannya — dan karena itu layar ini tidak punya satu tombol pun yang mengubah pencacah.

---

## 2. Proses bisnis

### 2.1 Alur normal — administrator menelusuri keluhan nomor

| No | Langkah | Pelaku | Keterangan |
| ---: | --- | --- | --- |
| 1 | Menerima keluhan "nomor melompat" | Administrator | Petugas melaporkan nomor tidak berurutan |
| 2 | Membuka layar pemantauan deret | Administrator | Butuh hak akses `NumberSeries : Read` |
| 3 | Membaca ringkasan | Sistem | Jumlah deret, jumlah baris, waktu alokasi terakhir |
| 4 | Menyaring deret yang dikeluhkan | Administrator | Contoh `BBK_BLOOD_ORDER` |
| 5 | Membaca nilai pencacah dan waktu alokasi terakhir | Sistem | Contoh `CurrentValue = 124` |
| 6 | Menyimpulkan | Administrator | Nomor 123 hangus karena pekerjaannya batal — bukan kerusakan |

**Langkah 6 adalah tujuan sebenarnya layar ini.** Ia tidak memperbaiki apa pun; ia mengubah
keluhan menjadi penjelasan.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi | Alasan |
| --- | --- | --- |
| Belum ada satu deret pun | Ringkasan memulangkan `0`, `0`, dan waktu **kosong** | Tabel lahir kosong; barisnya baru muncul pada alokasi pertama. Ini keadaan sah, bukan galat |
| Deret yang dicari tidak ada | Daftar kosong, bukan galat | Deret memang baru lahir saat dipakai pertama kali |
| Detail deret yang tidak ada dibuka | `404` beserta pesan "Deret nomor tidak ditemukan." | Baris mungkin memang belum pernah ada |
| Pengguna tanpa hak akses membuka layar | `403` dari `AccessPermissionFilter` | Hak akses diberikan admin lewat layar Akses Role, bukan ditanam di kode |
| Ukuran halaman diminta 5000 | Dibatasi menjadi 100 | Satu permintaan tidak boleh menarik seluruh tabel |

### 2.3 Yang sengaja tidak disediakan, beserta akibatnya bila disediakan

Ini bagian terpenting dari task ini, karena yang **tidak** dibuat lebih menentukan daripada yang
dibuat.

| Yang tidak ada | Bila ada, akibatnya |
| --- | --- |
| Tombol menyetel ulang pencacah | Nomor yang sudah menempel pada order, invoice, atau tindakan milik orang lain akan terbit lagi untuk catatan yang berbeda — pelanggaran `INV-PLT-001` |
| Tombol menyunting nilai pencacah | Sama seperti di atas, hanya lebih halus karena terlihat seperti "perbaikan kecil" |
| Tombol menghapus baris deret | Nilai tertinggi yang pernah terbit ikut hilang, dan alokasi berikutnya mengulang dari nol |
| Tombol "rapikan lubang" | Nomor pada lubang justru yang paling mungkin sudah sempat terlihat petugas, tercatat di kertas, atau disebutkan lewat telepon — `INV-PLT-002` |
| Endpoint membuat deret baru | Deret lahir sendiri pada alokasi pertama; tidak ada yang perlu membuatnya |
| `GET /options` | Deret bukan isi kotak pilihan — ia bukan master data, dan tidak pernah dipilih pengguna dari dropdown |

**Alokasi nomor tetap tidak dipaparkan lewat HTTP sama sekali.** Nomor yang dapat diminta lewat
HTTP dapat terbit tanpa catatan yang menempel padanya, sehingga deret berlubang tanpa sebab yang
dapat ditelusuri. Alokasi tetap berupa panggilan dalam proses lewat `NumberSeriesAllocator`,
dijaga butir hak akses milik pekerjaan yang membuat catatannya — misalnya `BloodOrder : Create`,
bukan butir tersendiri.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` bagian *Aturan Otorisasi dan Pengguna Saat Ini*, *Keselamatan Database*, *Layanan dan Pemrosesan Latar Belakang* · `BACKEND_ENGINEERING_CONTRACT.md` · `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Kontrak modul | `platform/contracts/api-contract.md` §1–§4 · `platform/contracts/permission-audit-matrix.md` · `platform/02-backend-architecture.md` §B, §D.2–D.5 · `platform/04-prd-to-mvp.md` (`FR-PLT-012`, `FR-PLT-013`) · `platform/roadmap/backend-roadmap.md` §5 |
| Entity dan konstanta modul | `Areas/Platform/NumberSeriesManagement/Models/NumNumberSeries.cs` · `Constants/NumberSeriesResetPolicies.cs` · `Repositories/ApplicationDbContext.cs` baris `DbSet` |
| Pola pembanding terdekat | `Areas/Administrator/MasterData/Controllers/KioskDeviceController.cs` dan `BankController.cs` — pola `filters/metadata`, `summary`, paging, dan `404` · `Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs` — pola query service baca-saja |
| Pola hak akses | `Attributes/AccessControllerAttribute.cs` · `Constants/AccessTypes.cs` · `Tests/.../BloodBankRoleAccessContractTests.cs` — pola contract test |
| Kontrak response bersama | `Responses/ApiResponse.cs` · `Responses/PagedResult.cs` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Platform/NumberSeriesManagement/DTOs/NumberSeriesDtos.cs` | **Baru.** Enam DTO, seluruhnya baca: `NumberSeriesResponse`, `NumberSeriesSummaryResponse`, `NumberSeriesFilterMetadataResponse`, `NumberSeriesDefaultFilterResponse`, `NumberSeriesSortOptionResponse`, `NumberSeriesPagedQuery`. **Nol DTO permintaan tulis** |
| `Areas/Platform/NumberSeriesManagement/Services/NumberSeriesQueryService.cs` | **Baru.** Empat method baca. Seluruh query memakai `AsNoTracking`; **nol pemanggilan `SaveChanges`** |
| `Areas/Platform/NumberSeriesManagement/Controllers/NumberSeriesController.cs` | **Baru.** Empat endpoint `GET`, seluruhnya dijaga `[AccessPermission("NumberSeries", "Read")]` |
| `Program.cs` | Satu baris `builder.Services.AddScoped<NumberSeriesQueryService>();` beserta komentar alasan pemisahannya dari alokator. Pendaftaran `NumberSeriesAllocator` **tidak disentuh** |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesQueryServiceTests.cs` | **Baru.** 13 uji perilaku bacaan |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesRoleAccessContractTests.cs` | **Baru.** 8 uji kontrak hak akses dan penjaga permukaan baca-saja |

**Nol berkas modul lain disentuh.** `NumberSeriesAllocator`, `NumNumberSeries`,
`BillingNumberSeriesService`, dan seluruh berkas Billing tidak berubah satu baris pun.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Empat endpoint baru**, seluruhnya baca, pada base URL yang sepenuhnya baru. **Nol endpoint existing** yang berubah, berganti nama, atau hilang. **Nol breaking change** |
| Database | **Nol perubahan schema, nol entity baru, nol migration baru.** Task ini hanya membaca tabel yang sudah dibuat `PLT-BE-002`. Migration `20260909070218_AddNumNumberSeries` sudah ada dan **belum dijalankan** — tidak berubah oleh task ini |
| Keamanan/Auth | **Satu butir hak akses baru: `NumberSeries : Read`**, menjaga keempat endpoint. Nol butir `Create`, `Update`, atau `Delete`, karena nol endpoint menulis. **Nol hardcode role**: tidak ada `IsInRole`, nama peran, nama departemen, nama posisi, maupun `UserType` yang dipakai sebagai penentu kewenangan |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Platform` / `NumberSeriesManagement` |
| Submodule | `Number Series` |
| Pemilik / prefix registry | `NumberSeriesManagement / Number Series` → `Num`, Category `BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY`, lifecycle ✅ **`ACTIVE`** |
| Keberlakuan | **`NEW CODE`** — seluruh berkas baru; nol legacy disentuh |
| Status registry | ✅ `ACTIVE` — memberi wewenang implementasi, bukan sekadar penamaan |
| QBE ID yang berlaku | `QBE-MOD-001` penempatan di bawah pemiliknya · `QBE-MOD-002` entity operasional pada modul terdaftar · `QBE-CODE-006` nol pembangkitan nomor di luar alokator |
| QBE ID yang **tidak** berlaku | `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — nol rename, nol pekerjaan database. `QBE-CODE-002`, `QBE-CODE-003` — nol endpoint alokasi dan nol `Count/Max+1` diperkenalkan |

---

## 4. Dokumentasi endpoint

#### Platform / Number Series Management / Number Series

Base URL: `api/v1/platform/number-series-management/number-series`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi penyaring dan pengurutan halaman pemantauan | `NumberSeries : Read` |
| `GET` | `/summary` | Mengambil jumlah deret, jumlah baris, dan waktu alokasi terakhir | `NumberSeries : Read` |
| `GET` | `/` | Mengambil daftar deret beserta nilai pencacahnya | `NumberSeries : Read` |
| `GET` | `/{id}` | Mengambil detail satu deret pada satu periode; `404` bila tidak ada | `NumberSeries : Read` |

**Empat endpoint, seluruhnya baca.** Tidak ada `POST`, `PUT`, `PATCH`, maupun `DELETE` — alasan
masing-masing ada di bagian 2.3.

### 4.1 Parameter daftar

| Parameter | Tipe | Kegunaan |
| --- | --- | --- |
| `search` | `string?` | Mencari pada penanda deret **dan** periode; tidak peka huruf besar-kecil |
| `sequenceKey` | `string?` | Menyaring penanda deret, **sama persis** — bukan mengandung |
| `scopeKey` | `string?` | Menyaring periode, sama persis |
| `resetPolicy` | `string?` | Menyaring kebijakan pengulangan |
| `sortBy` | `string?` | `sequenceKey`, `scopeKey`, `resetPolicy`, `currentValue`, `lastAllocatedAt` |
| `sortDirection` | `string?` | `asc` atau `desc` |
| `pageNumber` | `int` | Bawaan `1`; nilai di bawah 1 dikembalikan ke `1` |
| `pageSize` | `int` | Bawaan `25`, dibatasi maksimum `100` |

**Kenapa `sequenceKey` dicocokkan sama persis, bukan mengandung.** Penanda deret adalah penanda
teknis milik satu modul. Bila dicocokkan separuh, menyaring `LAB_SPECIMEN` akan ikut memulangkan
`LAB_SPECIMEN_BATCH` — dua deret berbeda milik pencacah berbeda, yang justru paling menyesatkan
saat administrator sedang menelusuri keluhan pada salah satunya. Pencocokan separuh tetap tersedia
lewat `search`, dan di sana memang itu yang diharapkan.

### 4.2 Bentuk response

```jsonc
// NumberSeriesResponse
{
  "id": "…",
  "sequenceKey": "BBK_BLOOD_ORDER",
  "scopeKey": "GLOBAL",
  "resetPolicy": "NEVER",
  "currentValue": 124,      // nilai terakhir yang SUDAH terbit, bukan nomor berikutnya
  "lastAllocatedAt": "2026-09-09T08:15:00+07:00"
}
```

```jsonc
// NumberSeriesSummaryResponse
{
  "totalSeries": 2,          // jumlah SequenceKey berbeda
  "totalScope": 3,           // jumlah baris (deret x periode)
  "lastAllocatedAt": "2026-09-09T08:15:00+07:00"   // null bila belum ada deret sama sekali
}
```

**`totalSeries` dan `totalScope` sengaja dilaporkan terpisah.** Contoh berangka: deret
`BBK_BLOOD_ORDER` memakai `GLOBAL` (1 baris), sedangkan `LAB_SPECIMEN` memakai `YEARLY` dan sudah
berjalan dua tahun (2 baris — `2025` dan `2026`). Jumlah deret berbeda adalah **2**, tetapi jumlah
barisnya **3**. Keduanya sama selama seluruh deret memakai `NEVER`, dan mulai berbeda begitu ada
deret yang diulang per periode — menyederhanakannya menjadi satu angka akan menyembunyikan
perbedaan itu tepat ketika ia mulai penting.

Seluruh response dibungkus `ApiResponse<T>`, dan daftar memakai `PagedResult<T>`, mengikuti
kontrak response bersama repository.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` project `UnitTests.Sqlite` beserta project utama | **`0 Error(s)`, `186 Warning(s)`** | `PASS` | Nol peringatan berasal dari berkas task ini; seluruhnya `CS1573`/`CS1574`/`CS1587` pada modul lain yang sudah ada sebelumnya |
| `dotnet test` — filter `~Platform` | **`Failed: 0, Passed: 54, Skipped: 0, Total: 54`** | `PASS` | Naik dari 32 kasus; selisih **22 kasus** adalah uji baru task ini |
| `NumberSeriesQueryServiceTests` | `Failed: 0` dari 14 method | `PASS` | Ringkasan, daftar, penyaringan, pengurutan, paging, detail, metadata, dan penjaga baca-saja |
| `NumberSeriesRoleAccessContractTests` | `Failed: 0` dari 8 method | `PASS` | Kontrak penamaan hak akses dan penjaga permukaan baca-saja |
| Empat endpoint kontrak ada persis, tanpa `GET /options` | Terbukti | `PASS` | `EmpatEndpointKontrak_AdaPersisTanpaOptions` |
| Nol endpoint tulis | Terbukti | `PASS` | `SeluruhEndpoint_HanyaGet_NolPostPutPatchDelete` |
| Nol butir hak akses tulis | Terbukti | `PASS` | `ModulHanyaMendaftarkanButirRead_NolButirTulis` |
| Membaca tidak mengubah satu baris pun | Terbukti | `PASS` | `MembacaSeluruhPermukaan_TidakMengubahSatuBarisPun` — potret tabel dibandingkan lewat konteks baru |
| Eksekusi migration | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; task ini **nol** perubahan schema |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT RUN` | Pemeriksaan hak akses berjalan di lapisan filter yang dilewati uji langsung-controller. Dibuktikan lewat contract test refleksi, sesuai pola `BloodBankRoleAccessContractTests` |

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database yang sudah menerapkan
migration `20260909070218_AddNumNumberSeries`, dan migration itu **belum dijalankan**.

**Tidak dijalankan:** `dotnet build QuilvianSystemBackend.sln` penuh. Build yang dijalankan
menyasar project `UnitTests.Sqlite` beserta project utama yang direferensinya. Angka `186
Warning(s)` karena itu **tidak sebanding** dengan `210 Warning(s)` pada laporan `PLT-BE-002`
sampai `PLT-BE-004`, yang membangun kelima project pada solution. Selisih 24 berasal dari tiga
project test yang tidak ikut dibangun, **bukan** dari peringatan yang hilang.

### 5.1 Cacat yang ditemukan uji, dan perbaikannya

Uji task ini menangkap satu cacat pada kode task ini sendiri sebelum kode itu selesai.

**Gejalanya.** Tiga uji gagal dengan `System.NotSupportedException : SQLite does not support
expressions of type 'DateTimeOffset' in ORDER BY clauses`.

**Sebabnya.** `LastAllocatedAt` bertipe `DateTimeOffset`. Provider SQLite yang dipakai uji menolak
tipe itu pada agregat `MAX` **maupun** pada `ORDER BY`. Percobaan perbaikan pertama — mengganti
`MaxAsync` dengan `OrderByDescending().FirstOrDefaultAsync()` — **gagal**, karena ia hanya
memindahkan kendala dari `MAX` ke `ORDER BY`; ketiga uji yang sama tetap gagal.

**Perbaikannya.** Kedua jalur yang menyentuh `LastAllocatedAt` dipindahkan ke sisi klien:

| Jalur | Bentuk akhir |
| --- | --- |
| Nilai alokasi terakhir pada ringkasan | Kolomnya diproyeksikan lewat `Select`, lalu `.Max()` dijalankan di memori |
| Pengurutan daftar menurut `lastAllocatedAt` | Baris ditarik lebih dulu, lalu diurutkan dan dipotong di memori |
| Empat pengurutan lain | **Tetap dikerjakan database** beserta pagingnya |

**Apakah ini cacat produksi?** **Tidak.** Kolomnya `timestamp with time zone`, dan PostgreSQL
menerjemahkan kedua bentuk semula dengan benar. Yang rusak adalah **kemampuan membuktikannya**:
selama query memakai bentuk itu, jalur ringkasan dan pengurutan waktu mustahil diuji tanpa
PostgreSQL berjalan — persis bentuk kegagalan yang sudah menahan `PLT-BE-004`.

**Ongkos yang disadari.** Hanya pengurutan menurut waktu alokasi yang membayar, dan tabel ini
berisi satu baris per deret per periode — puluhan, bukan jutaan. Bila kelak jumlah deret tumbuh
jauh di luar dugaan, jalan keluar yang lebih bersih adalah memindahkan kolomnya ke `DateTime`;
catatan itu ditinggalkan di komentar kode, bersama peringatan agar cabang `lastAllocatedAt`
**tidak** dikembalikan ke `ApplySorting`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Empat endpoint memulangkan bentuk sesuai kontrak | ✅ **Terpenuhi** | `EmpatEndpointKontrak_AdaPersisTanpaOptions` membuktikan rutenya persis `filters/metadata`, `summary`, `` (daftar), dan `{id:guid}`. Bentuk response dibuktikan `Daftar_MemulangkanNilaiPencacahApaAdanya`, `Ringkasan_MembedakanJumlahDeretDariJumlahBaris`, dan `Detail_MemulangkanDeretYangDiminta` |
| Butir `NumberSeries : Read` muncul dan dapat dicentang di layar Akses Role | ✅ **Terpenuhi** | `ButirKontrakV1_SeluruhnyaLahirDariControllerYangAda` dan `SetiapAksi_MunculDanDapatDiberikanDiLayarAksesRole` — `IsSystemOnly` `false`, `VisibleInRoleAccess` `true`, `AccessType` termasuk `AccessTypes.AllowedForRoleAccess` |
| Pasangan hak akses tidak menghasilkan `403` permanen | ✅ **Terpenuhi** | `ResourcePadaPermission_SamaPersisDenganControllerName` dan `AksiPadaPermission_SamaPersisDenganActionNamePadaMethodYangSama` |
| Nol endpoint hanya terlindungi `[Authorize]` | ✅ **Terpenuhi** | `TidakAdaEndpointBankDarahYangHanyaTerlindungiAuthorize` versi modul ini — keempat endpoint punya `[AccessAction]` **dan** `[AccessPermission]` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| **Nol** endpoint tulis | ✅ **Terpenuhi dan dipindai** — `SeluruhEndpoint_HanyaGet_NolPostPutPatchDelete` gagal bila ada satu `POST`/`PUT`/`PATCH`/`DELETE` masuk |
| Nol tombol yang mengubah pencacah — menyetel ulang melanggar `INV-PLT-001` | ✅ **Terpenuhi dan dipindai** — `ModulHanyaMendaftarkanButirRead_NolButirTulis`, dan `MembacaSeluruhPermukaan_TidakMengubahSatuBarisPun` membuktikan seluruh permukaan baca tidak menyentuh satu baris pun |
| Contract test hak akses bergaya `BloodBankRoleAccessContractTests` | ✅ **Terpenuhi** — 8 uji, pola diturunkan langsung dari berkas tersebut |
| Build dan uji benar-benar dijalankan, hasil sebenarnya dicatat | ✅ **Terpenuhi** — bagian 5 |
| Status migration disebut eksplisit | ✅ **Terpenuhi** — nol migration baru; migration `PLT-BE-002` **belum dijalankan** |

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Nama method detail** | `02-backend-architecture.md` §B menggambarkan `GetByKeyAsync()`; implementasinya `GetByIdAsync()` | Kontrak API §2 menetapkan endpoint `GET /{id}` yang beralamat `Id`, bukan pasangan penanda deret dan periode. Diagram kelas pada §B bersifat gambaran, sedangkan kontrak API yang disetujui bersifat mengikat — dan keduanya hanya dapat disamakan dengan mengubah salah satunya. Yang diikuti adalah kontrak API |
| **Penyaring `scopeKey`** | `02-backend-architecture.md` §D.4 menyebut `search`, `sequenceKey`, `resetPolicy`; implementasinya menambahkan `scopeKey` | Tanpa penyaring periode, deret ber-`YEARLY` atau `MONTHLY` yang punya banyak baris tidak dapat dipersempit ke satu periode. Ini penambahan permukaan teknis pada slice yang sama memakai pola yang sudah ada, **bukan** aturan bisnis baru |
| **Nol delta lain** | Keempat endpoint, hak aksesnya, dan bentuk response-nya diturunkan persis dari `api-contract.md` §2–§3 dan `permission-audit-matrix.md` §1 | — |

**Nol kebijakan baru dikarang.** Kedua delta di atas adalah keputusan teknis, bukan keputusan
bisnis, kewenangan, maupun alur persetujuan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan **`186 Warning(s)`, nol di antaranya dari berkas task ini**. Angka ini **tidak sebanding** dengan `210` pada laporan terdahulu karena cakupan build-nya berbeda — lihat bagian 5 |
| Masalah yang diketahui | **(1)** Pengurutan menurut `lastAllocatedAt` dan pencarian nilai terakhir dikerjakan **di sisi klien**, bukan di database — sebabnya, ongkosnya, dan batas amannya di bagian 5.1. **(2)** `NumNumberSeries` adalah **satu-satunya** entity di seluruh `Areas/` yang memakai `DateTimeOffset`, dan nol tempat lain di repository mengurutkan tipe itu; karena itu tidak ada preseden yang dapat ditiru, dan keputusan di bagian 5.1 diambil dari pesan galat provider, bukan dari pola yang sudah ada |
| Risiko tersisa | **Migration `20260909070218_AddNumNumberSeries` belum dijalankan**, sehingga keempat endpoint ini belum dapat dipanggil di lingkungan mana pun — tabelnya belum ada. Ini risiko yang diwarisi dari `PLT-BE-002`, bukan yang ditambahkan task ini. **Durabilitas alokator tetap belum terbukti** (`PLT-BE-004`); layar ini membaca pencacah apa adanya, sehingga tidak terpengaruh — tetapi ia juga **tidak** membuktikan apa pun tentang durabilitas |
| Perubahan sampingan | **Satu butir dipulihkan.** Folder `QuilvianSystemBackend.Tests/` di akar repository berisi **hanya** artefak `obj/` tanpa satu pun berkas source maupun `.csproj`, dan membuat `dotnet build` solution gagal dengan 9 galat `CS0579`/`CS0400` (atribut assembly kembar). Folder itu **untracked** — dibuktikan `git ls-files` memulangkan kosong — sehingga menghapusnya tidak menyentuh pekerjaan yang dilacak. Dihapus hanya folder itu; nol perubahan lain dipulihkan |
| Interupsi | **Tiga kali sesi terputus** saat build berjalan. Setiap kali dipulihkan sesuai `TASK_RULES`: memeriksa `git status`, membandingkan waktu ubah `.dll` terhadap source, memastikan perbaikan masih terpasang, lalu melanjutkan dari kondisi terverifikasi. **Nol penyuntingan ganda dan nol pekerjaan diulang.** Satu monitor sempat salah melaporkan build mati karena memakai keberadaan proses `dotnet.exe` sebagai penanda dan memeriksa tepat saat MSBuild berganti proses anak; dikoreksi dengan memeriksa isi berkas keluaran, bukan proses |
| Status Git | Lihat bagian 9 |
| Langkah berikutnya | **(1)** Jalankan migration `20260909070218_AddNumNumberSeries` — **wewenang terpisah** — supaya tabelnya ada dan endpoint ini dapat dipanggil. **(2)** Sediakan database test PostgreSQL lalu jalankan `PLT-BE-004` supaya `AC-PLT-003`, `004`, `005`, `012` benar-benar terbukti. **(3)** `PLT-FE-001` (layar frontend) kini punya penyedia backend-nya |

---

## 9. Status Git

```text
 M Program.cs
?? Areas/Platform/NumberSeriesManagement/Controllers/
?? Areas/Platform/NumberSeriesManagement/DTOs/
?? Areas/Platform/NumberSeriesManagement/Services/NumberSeriesQueryService.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesQueryServiceTests.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumberSeriesRoleAccessContractTests.cs
?? docs/module-blueprints/platform/task/report/backend/PLT-BE-005.md
```

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. **Nol perintah database dijalankan.** `HEAD` tetap `2eec97d`.
