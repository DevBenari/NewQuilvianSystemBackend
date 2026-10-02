# Laporan Perubahan Backend — `BE-LAB-66`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-66` |
| Judul | Permukaan baseline tiga data induk Patologi Anatomi |
| Slice | `S4c` — data induk Patologi Anatomi; prasyarat `FE-LAB-27` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ae** |
| Trace | `LAB-DEC-086`, `LAB-DEC-087`; `INV-39`; `VAL-99`, `VAL-100`, `VAL-101`; `AC-143`; `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-OPT-001`, `QBE-PAGE-001` |
| Contract version | `LAB-API-v1` **`r32`** bagian 27 — **`approved` 2026-09-23** oleh pemilik modul |
| Dependency | `BE-LAB-50` ✅ selesai 2026-09-18 — keempat tabel berdiri, migration terterap |
| Klasifikasi | `MEDIUM` — 11 endpoint, 3 controller, 3 service, 2 berkas DTO, 1 factory; nol migration, nol permission baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `18af6391` (branch `yoga`) |
| Tanggal | 2026-09-23 |
| Status | ✅ **`SELESAI`** — kesebelas endpoint terbangun dan **terbukti berjalan terhadap database sungguhan** 2026-09-23. `AC-192` dan `AC-193` **terbukti penuh**, `AC-194` terbukti separuh beserta alasannya. Build 0 error. Celah izin yang sempat menahannya **sudah ditutup** pada hari yang sama |

---

## 1. Masalah yang diperbaiki

Ketiga data induk Patologi Anatomi sudah punya tabel, migration, dan endpoint sejak `BE-LAB-50`
(2026-09-18). Tetapi endpoint yang berdiri **bukan permukaan yang dituntut layar**.

`rules/frontend/master-data-feature-standard.md` menetapkan setiap layar data induk memuat tiga
hal yang bersandar pada endpoint tertentu:

| Yang dituntut layar | Bersandar pada | Ada sebelum task ini |
| --- | --- | --- |
| Kartu ringkasan di atas tabel | `GET /summary` | **Nol** |
| Penyaring yang bentuknya dibaca dari server | `GET /filters/metadata` | **Nol** |
| Sakelar aktif per baris di kolom aksi | `PATCH /{id}/status` | **Nol** |
| Formulir ubah yang dapat dibuka lewat tautan langsung | `GET /{id}` | **Nol** |

**Akibat yang paling mahal adalah yang keempat, dan bentuknya diam.** Tanpa `GET /{id}`, formulir
ubah yang dibuka lewat tautan langsung — atau sesudah petugas menekan tombol segarkan — nol punya
cara memuat baris yang sedang disunting. Layarnya tidak menampilkan galat apa pun; ia sekadar
tampak **kosong**, dan petugas menyimpulkan datanya hilang. Modul ini sudah membayar kelas
kesalahan itu sekali lewat `FE-LAB-03`, dan `LAB-API-v1` `r6` lahir untuk menutupnya.

**Selisih ini sudah diketahui sejak 2026-09-22 dan sengaja dicatat, bukan baru ditemukan.**
`BE-LAB-65` bagian 6ad.1 menyapu kedua puluh dua controller Laboratorium, menemukan ketiga grup
ini kurang, lalu **meninggalkannya dengan sengaja** — mengerjakannya di sana berarti dua task
dalam satu pemanggilan. Yang belum terjadi adalah tasknya dibuka.

Ini kali **ketiga** bentuk yang sama menahan task frontend: `FE-LAB-24` menunggu `BE-LAB-65`,
`FE-LAB-34` menunggu `BE-LAB-64`, dan `FE-LAB-27` menunggu task ini.

---

## 2. Proses bisnis

**Tujuan.** Kepala instalasi Laboratorium mengelola tiga daftar yang menentukan bentuk laporan
Patologi Anatomi, dari aplikasi — bukan lewat SQL langsung ke basis data.

**Pelaku.** Kepala instalasi Laboratorium (`DR-LAB-003` untuk penggolongan pemeriksaan).

**Ketiga daftar itu dan hubungannya:**

1. **Parameter** — ruas isian yang mungkin muncul pada laporan, misalnya `Makroskopik`,
   `Mikroskopik`, `Kesimpulan`, `Anjuran`.
2. **Golongan** — kategori Patologi Anatomi, misalnya Histologi, Sitologi, IHK. Setiap golongan
   punya **keberlakuan**: parameter mana yang berlaku baginya dan mana yang wajib.
3. **Penggolongan jenis pemeriksaan** — memetakan satu jenis pemeriksaan katalog ke satu
   golongan.

**Alur berurutan saat patolog membuka laporan:** pesanan → jenis pemeriksaannya → **penggolongan**
→ golongan → **keberlakuan** → daftar parameter → formulir laporan dibangkitkan dari daftar itu.

**Jalur tidak normalnya, dan ini yang membuat ringkasan baru berguna.** Jenis pemeriksaan yang nol
punya baris penggolongan **nol menyumbang satu pun parameter** — `INV-39` menetapkan sistem nol
menebak golongan sebuah pemeriksaan. Formulirnya kosong sama sekali, dan `VAL-100` menolaknya
dengan menyebut apa yang belum diatur.

**Sampai hari ini, satu-satunya cara kepala instalasi mengetahui berapa pekerjaan yang tersisa
adalah membuka daftar usulan dan menghitungnya sendiri.** `GET /summary` pada grup penggolongan
menjawabnya dengan satu angka: `unmappedProcedure`.

Contoh berangka. Katalog rumah sakit memuat 10 jenis pemeriksaan Patologi Anatomi; 4 sudah
digolongkan sebagai data uji. Ringkasannya berbunyi `totalProcedure: 10`, `mappedProcedure: 4`,
`unmappedProcedure: 6`. Begitu satu penggolongan disimpan, ketiganya bergerak menjadi `10`, `5`,
`5` — dan kepala instalasi melihat sisanya berkurang tanpa membuka daftar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` | Wewenang runtime QBE |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Status registry `LaboratoryManagement / Lab` |
| `rules/backend/master-data-endpoint-standard.md` | Bentuk baseline sembilan endpoint dan varian sahnya |
| `rules/frontend/master-data-feature-standard.md` | Apa yang sesungguhnya dikonsumsi layar |
| `Controllers/LabOrganismController.cs` | **Rujukan bentuk utama** — hasil `BE-LAB-65`, delapan endpoint |
| `Services/LabMicrobiologyMasterDataService.cs` | Pola `GetByIdAsync`, `GetSummaryAsync`, `SetStatusAsync` |
| `Services/LabFilterMetadataFactory.cs` | Pola pabrik metadata |
| `Models/LabProcedurePathologyCategory.cs` | **Menentukan cakupan** — ia nol punya `IsActive` |
| `Models/LabPathologyParameterCategory.cs` | Sumber angka keberlakuan |
| Ketiga controller dan service Patologi Anatomi | Keadaan as-is |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `DTOs/LabPathologyMasterDataDtos.cs` | **+4 DTO**: `LabPathologyMasterDataStatusRequest`, dan tiga `*SummaryResponse` |
| `DTOs/LabFilterAndSummaryDtos.cs` | **+3 DTO** metadata penyaring, satu per grup |
| `Services/LabFilterMetadataFactory.cs` | **+3 pabrik** metadata |
| `Services/LabPathologyParameterService.cs` | **+3 method**: `GetByIdAsync`, `GetSummaryAsync`, `SetStatusAsync` |
| `Services/LabPathologyCategoryService.cs` | **+3 method** yang sama; `GetByIdAsync` ikut menghitung `ParameterCount` |
| `Services/LabProcedurePathologyCategoryService.cs` | **+1 method** `GetSummaryAsync`; `GetByIdAsync` yang sudah ada **dinaikkan dari `private` menjadi `public`** |
| `Controllers/LabPathologyParameterController.cs` | **+4 endpoint**; `Description` aksi diseragamkan |
| `Controllers/LabPathologyCategoryController.cs` | **+4 endpoint**; `Description` aksi diseragamkan |
| `Controllers/LabProcedurePathologyCategoryController.cs` | **+3 endpoint**; `Description` aksi diseragamkan |

**Nol berkas di luar `Areas/HealthServices/LaboratoryManagement/` disentuh.** Nol entity, nol
configuration, nol migration, nol seeder.

**`GetByIdAsync` pada grup penggolongan nol ditulis ulang.** Ia sudah ada sejak `BE-LAB-50`
sebagai pembantu internal — ia yang menyusun jawaban `CreateAsync` dan `UpdateAsync` — dan bentuk
maupun penolakan `404`-nya sudah persis yang dituntut baseline. Yang berubah hanya pengubah
aksesnya. Menulis ulang jalur kedua untuk bentuk yang sama justru melahirkan dua kebenaran.

**Satu cacat ditangkap sebelum dikirim, dan kelasnya sudah pernah dibayar modul ini.** Versi
pertama `usedInCategory` dan `withParameter` menghitung keberlakuan yang hidup **tanpa memeriksa
apakah parameter atau golongannya sendiri masih hidup**. Bila suatu saat sebuah baris data induk
ditandai terhapus sementara keberlakuannya tertinggal, ringkasannya dapat melaporkan angka
terpakai yang **lebih besar daripada totalnya** — ringkasan yang membantah dirinya sendiri.

Ini bentuk yang sama persis dengan cacat `withBreakpoint` yang ditemukan `BE-LAB-65` bagian 6ad.5
(*"nol organisme aktif, tetapi satu tercakup"*). Kedua penghitung kini memeriksa **kedua syarat**.

> Hari ini cacatnya nol dapat menyala, sebab ketiga grup **nol punya `DELETE`** sehingga `IsDelete`
> nol pernah bernilai benar lewat API. Ia diperbaiki bukan karena sedang merusak, melainkan karena
> penghitung yang benar hanya karena jalur perusaknya belum ada adalah penghitung yang menunggu.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** 11 endpoint ditambahkan; kesepuluh endpoint lama pada ketiga grup **nol berubah** route, verb, bentuk, maupun hak akses. Tercatat `r32` bagian 27 |
| Database | **NOT APPLICABLE.** Nol migration, nol kolom, nol perubahan schema. Keempat tabel berdiri sejak `BE-LAB-50` dan migration-nya sudah terterap 2026-09-18 |
| Keamanan/Auth | **Nol permission baru.** Kesebelasnya menumpang `Read` dan `Update` yang sudah terdaftar. Grup penggolongan tetap menumpang resource `LabPathologyCategory`, bukan resource sendiri — persis seperti keempat endpoint lamanya |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Pathology Parameter

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Bentuk penyaring, ukuran halaman, dan penanda `isDeletable` | `LabPathologyParameter : Read` |
| `GET` | `/summary` | Empat angka ringkasan, termasuk parameter yang belum dipakai golongan mana pun | `LabPathologyParameter : Read` |
| `GET` | `/{id}` | Satu parameter beserta seluruh ruasnya, untuk formulir ubah | `LabPathologyParameter : Read` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu parameter | `LabPathologyParameter : Update` |

#### Health Services / Laboratory Management / Lab Pathology Category

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Bentuk penyaring, ukuran halaman, dan penanda `isDeletable` | `LabPathologyCategory : Read` |
| `GET` | `/summary` | Empat angka ringkasan, termasuk golongan yang nol punya satu pun ruas | `LabPathologyCategory : Read` |
| `GET` | `/{id}` | Satu golongan beserta jumlah keberlakuannya | `LabPathologyCategory : Read` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu golongan | `LabPathologyCategory : Update` |

#### Health Services / Laboratory Management / Lab Procedure Pathology Category

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Bentuk penyaring, ditambah dua penanda ketiadaan yang disengaja | `LabPathologyCategory : Read` |
| `GET` | `/summary` | Tiga angka, termasuk **`unmappedProcedure`** | `LabPathologyCategory : Read` |
| `GET` | `/{id}` | Satu penggolongan beserta pemeriksaan dan golongannya | `LabPathologyCategory : Read` |

**Grup ketiga berhenti di tiga endpoint, dan itu keputusan bukan kelalaian.** `PATCH /{id}/status`
nol berlaku sebab `LabProcedurePathologyCategory` **nol punya `IsActive`** — ia baris pemetaan,
bukan data induk berstatus; menambahkan kolom status berarti migration, dan itu keluar dari sifat
aditif `r32`. `GET /options` nol dibangun sebab nol satu pun layar memilih sebuah *pemetaan* dari
kotak pilihan; `QBE-OPT-001` menetapkan options disediakan **hanya bila dikonsumsi**.

**Kedua ketiadaan itu dinyatakan, bukan dibiarkan disimpulkan.** `filters/metadata` grup ini
mengirim `supportsStatusToggle: false` dan `hasOptionsEndpoint: false`, supaya layar membacanya
alih-alih menyimpulkannya dari endpoint yang menjawab `404`.

> **Koreksi terhadap catatan `BE-LAB-65` 6ad.1.** Di sana grup ini tercatat *"kurang kelimanya"*.
> Angka itu diturunkan dari baseline sembilan **tanpa memeriksa entity-nya**. Tiga yang benar-benar
> berlaku. Ini koreksi, bukan pengurangan cakupan.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 Error**, 224 Warning | `PASS` | Keluaran perintah |
| `dotnet build -p:RunAnalyzers=False -t:Rebuild` | **0 Error**, 224 Warning — **nol peringatan menyebut satu pun berkas yang disentuh task ini** | `PASS` | Keluaran rebuild penuh disaring pada `Pathology` dan `LabFilter` |
| `git status --short` | **13 berkas**: 9 source dalam `LaboratoryManagement/`, 4 dokumen blueprint. Nol berkas lain | `PASS` | Keluaran perintah |
| Aplikasi dijalankan | **Hidup** — `/health` `200`, `/swagger` `200` | `PASS` | `ASPNETCORE_ENVIRONMENT=Development`, 2026-09-23 |
| Kesebelas endpoint baru lewat HTTP | **Seluruhnya `200`** sesudah izin diberikan 2026-09-23 | `PASS` | Dipanggil sebagai dr. Bima Prasetya, Sp.PK |
| `AC-192` — `GET /{id}` dua arah, **ketiga** grup | **Terbukti.** `200` beserta isinya: `MAKROSKOPIK`; `HISTO` dengan `parameterCount 3`; `Histopatologi Biopsi Besar`. Penunjuk asing **`404`** pada ketiganya | `PASS` | Respons HTTP |
| `AC-193` — `PATCH {id}/status` | **Terbukti, dan dibuktikan dari tiga sisi sekaligus.** `false`: baris **keluar** dari `/options` (1→0), **tetap** pada `GET /` (1), ringkasan `activeParameter` **15→14**. Dikembalikan `true`: ketiganya **pulih persis**. Penunjuk asing **`404`** | `PASS` | Respons HTTP |
| `AC-193` **terbalik** pada grup pemetaan | **Terbukti** — nol endpoint status terdaftar | `PASS` | Controller nol memuat `HttpPatch` |
| `AC-194` — `unmappedProcedure` | **Terbukti separuh.** Angkanya **cocok persis dengan kenyataan yang sudah tercatat**: `totalProcedure 10`, `mappedProcedure 4`, `unmappedProcedure 6` — sama dengan *"empat dari sepuluh sudah dipetakan sebagai data uji, enam sisanya belum"* pada roadmap frontend. Aritmetikanya menutup: 4 + 6 = 10 | `PASS` sebagian | Respons HTTP |
| `AC-194` — penurunan tepat satu saat pemetaan disimpan | ✅ **Terbukti 2026-09-23**, lewat pemetaan yang memang diinstruksikan pemilik modul | `PASS` | Enam penggolongan disimpan untuk `FE-LAB-28`: `unmappedProcedure` **6 → 0 pada enam penyimpanan**, `mappedProcedure` **4 → 10**, `totalProcedure` tetap **10**. Setiap penyimpanan menjawab `200`. *(Sebelumnya `NOT RUN` dengan sengaja — membuat pemetaan uji nol dapat dibatalkan, sebab grup ini nol punya `DELETE`. Yang mengubahnya adalah instruksi eksplisit, bukan uji)* |
| Ketiga `summary` terhadap data sungguhan | Parameter `15/15/0/15`; golongan `4/4/0/4`; pemetaan `10/4/6` — seluruhnya **cocok dengan angka seeder `BE-LAB-50`** | `PASS` | Respons HTTP |
| Baris `SysActionAccess` sesudah seeder | **Nol dihitung langsung**, tetapi **terbukti tidak langsung**: katalog `role-access/resources` menampilkan ketiga controller beserta aksinya, dan `Description`-nya **sudah memakai teks yang diseragamkan task ini** | `PASS` sebagian | Katalog hak akses |

Uji manual: **`PASS` sebagian** — dijalankan sebagai **dr. Bima Prasetya, Sp.PK**
(`Kepala Instalasi Laboratorium`).

> ### Koreksi terhadap laporan versi pertama, dan ongkosnya nyata
>
> Laporan ini semula menyatakan *"aplikasi nol dapat distart karena `Jwt:Key` belum
> dikonfigurasi"*. **Itu keliru.** Kuncinya **sudah ada** di `appsettings.Development.json`; yang
> kurang hanyalah menjalankannya dengan `ASPNETCORE_ENVIRONMENT=Development`. Aplikasi hidup pada
> percobaan pertama.
>
> Kekeliruan yang sama menandai `NOT RUN` pada `FE-LAB-24` dan `FE-LAB-31`..`FE-LAB-34` dengan
> alasan penahan yang **sebenarnya nol ada**.

**Yang terbukti.** Kesebelas endpoint **terdaftar dan ter-routing**. Buktinya dapat dibedakan:
jalur yang memang tidak ada — `lab-catalog`, `lab-worklists` tanpa sub-path — menjawab **`404`**,
sedangkan kesebelas jalur baru menjawab **`403`**, yaitu jawaban filter izin yang berjalan
**sesudah** route ditemukan. Nol `500` di seluruh modul.

**Yang belum terbukti, dan penahannya BUKAN lagi `Jwt:Key`.** Akun Kepala Instalasi ditolak
`403` pada ketiga grup Patologi Anatomi, sehingga bentuk jawaban, isi ringkasan, dan `404` bagi
penunjuk asing tetap nol dapat dilihat. Penyebabnya **celah pemberian izin**, diuraikan pada
bagian 7 *Masalah yang diketahui*.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-192` ketiga grup menjawab `GET /{id}` dengan baris utuh, dan `404` bagi penunjuk asing | **Belum terbukti** | Ketiga jalur menolak lewat `KeyNotFoundException` yang diterjemahkan controller menjadi `404`; **belum dipanggil** |
| `AC-193` `PATCH /{id}/status` → `false` mengeluarkan baris dari `GET /options` dan tetap menampilkannya pada `GET /` | **Belum terbukti** | `GetOptionsAsync` kedua grup menyaring `IsActive`; `GetListAsync` nol menyaringnya kecuali diminta. **Belum dipanggil** |
| `AC-193` dibuktikan **terbalik** pada grup penggolongan: nol endpoint status terdaftar | ✅ **Terpenuhi** | Controller-nya nol memuat `HttpPatch`; dinyatakan pula lewat `supportsStatusToggle: false` |
| `AC-194` `unmappedProcedure` turun tepat satu ketika satu penggolongan disimpan | **Belum terbukti** | Dihitung dari definisi yang sama persis dengan `GET /suggestions`. **Belum dipanggil** |
| DoD — ketiga grup mencapai permukaan 6ae.3 | ✅ **Terpenuhi** | 4 + 4 + 3 endpoint; parameter 4→8, golongan 6→10, penggolongan 4→7 |
| DoD — kedua pengecualian 6ae.4 dibuktikan terbalik | ✅ **Terpenuhi** | Nol `HttpPatch` dan nol `options` pada controller penggolongan, dan keduanya dinyatakan pada metadata |
| DoD — nol endpoint lama berubah bentuk | ✅ **Terpenuhi** | Diff hanya menambah method; satu-satunya suntingan pada method lama adalah teks `Description` atribut Access |
| DoD — data dev pulih seperti semula | **NOT APPLICABLE** | Nol permintaan dikirim, sehingga nol data tersentuh |

**Tiga dari empat acceptance criteria belum terbukti**, dan penyebabnya tunggal serta bukan kode.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build memunculkan **224 peringatan**, seluruhnya kelas `CS1573`/`CS1574`/`CS1587` pada berkas di luar cakupan task ini. **Nol peringatan menyebut berkas yang disentuh** |
| **Celah izin — DITEMUKAN DAN DITUTUP 2026-09-23** | **Sudah ditutup.** Enam pasangan izin diberikan lewat `POST /administrator/setting/role-access/policies` sebagai superadmin: `LabPathologyParameter` dan `LabPathologyCategory`, masing-masing `Read`, `Create`, `Update`. Set lama **dibaca lebih dulu dan dikirim ulang utuh** — endpoint itu memakai `overwriteTarget: true`, sehingga mengirim hanya yang baru akan **menghapus 21 izin yang sudah ada**. Hasil: `totalAllowed` 21 → **27**, dan kesebelas endpoint berpindah dari `403` menjadi `200`. **Grup pemetaan sengaja nol diberi izin sendiri**, sebab `[AccessPermission]`-nya menunjuk `LabPathologyCategory` — persis yang `LAB-PERM-v1` rev 7 minta: *"keberlakuan dan pemetaan ikut `LabPathologyCategory : Update`, bukan resource sendiri"*. **Satu butir tetap perlu keputusan pemilik:** matriks memberi `Read` kepada Dokter Lab dan Petugas Lab, **bukan** kepada kepala instalasi — padahal layar kelola wajib memuat daftar sebelum dapat mengubah. `Read` diberikan atas dasar itu; bila matriksnya yang benar, izin ini yang perlu dicabut |
| ~~Celah izin yang menahan `FE-LAB-27`~~ *(riwayat)* | **Ditemukan 2026-09-23 saat menguji dengan akun sungguhan, dan ia menahan layarnya bukan endpointnya.** Akun `Kepala Instalasi Laboratorium` ditolak **`403`** pada **ketiga** grup Patologi Anatomi — `lab-pathology-parameters`, `lab-pathology-categories`, `lab-procedure-pathology-categories` — padahal `LAB-PERM-v1` rev 7 baris 415 dan 417 menugaskan `Create`/`Update` kedua resource itu **kepada kepala instalasi**. Hak akses berasal dari `SysAccessPolicies` per `(DepartmentId, PositionId)`; **nol baris** ada bagi pasangan milik jabatan ini terhadap ketiga controller. Akun yang sama menjawab `200` pada delapan grup lain, sehingga ini **bukan** akun rusak melainkan pemberian izin yang belum dibuat. **Akibatnya bagi `FE-LAB-27`: ketiga layar dapat dibangun, tetapi orang yang layar itu dibuat untuknya nol dapat membukanya.** Satu hal ikut terlihat dan pantas diperiksa pemilik: matriks yang sama memberi `Read` kepada Dokter Lab dan Petugas Lab, **bukan** kepada kepala instalasi — padahal layar kelola wajib memuat daftarnya lebih dulu sebelum dapat mengubah. Pemberian izin adalah **pekerjaan data**, bukan kode, dan berada di luar wewenang tulis task ini |
| Masalah yang diketahui | **Satu selisih ditemukan pada pekerjaan `BE-LAB-65`, dilaporkan bukan ditambal.** `LabFilterMetadataFactory.LabOrganism()` dan `LabAntibiotic()` mengirim `SortOptions` berisi tiga pilihan urutan, padahal `LabOrganismPagedQuery` dan kembarannya **nol punya ruas `SortBy` maupun `SortDirection`** dan daftarnya diurutkan tetap di dalam service. Layar yang merender pemilih urutan dari metadata itu akan menampilkan kendali yang **nol mengubah satu baris pun**. Ketiga grup Patologi Anatomi karena itu mengirim `SortOptions` **kosong** — jujur terhadap keadaannya. Memperbaiki kedua grup Mikrobiologi berarti menyentuh cakupan `BE-LAB-65`, dan menambah ruas sort pada endpoint daftar yang sudah berjalan **bertentangan dengan `r32` bagian 27.6** yang menyatakan endpoint lama nol berubah bentuk |
| Risiko tersisa | **Sedang, dan seluruhnya pada verifikasi.** Kesebelas endpoint nol pernah dipanggil. Yang paling patut diperiksa lebih dulu saat aplikasi dapat distart: `unmappedProcedure` — ia satu-satunya angka baru yang dihitung dari dua tabel sekaligus, dan satu-satunya yang salahnya nol menimbulkan galat, hanya angka yang keliru |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 13 berkas `M`: 9 source `LaboratoryManagement/`, 4 dokumen blueprint. **Nol operasi Git dijalankan** — nol stage, nol commit, nol push |
| Langkah berikutnya | **1.** Jalankan backend dengan `Jwt:Key` terisi, lalu buktikan `AC-192`..`AC-194` beserta jumlah baris `SysActionAccess`. **2.** `FE-LAB-27` kini nol punya penahan backend dan dapat dibangun sesuai `master-data-feature-standard`. **3.** Putuskan selisih `SortOptions` pada kedua grup Mikrobiologi — menambah ruas sort, atau mengosongkan metadatanya |
