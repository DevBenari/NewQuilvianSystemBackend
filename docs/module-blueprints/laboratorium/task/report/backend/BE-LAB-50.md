# Laporan Perubahan Backend — `BE-LAB-50`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-50` (backend) |
| Judul | Empat data induk Patologi Anatomi — gelombang `MVP-6b1`, slice `S4c` |
| Trace | `FR-13.11`; `LAB-DEC-086`, `LAB-DEC-087`; `LAB-DC-045`..`LAB-DC-048`; `INV-34`, `INV-37`, `INV-39` |
| Kontrak | `LAB-API-v1` **`r25`** bagian 20.3, `LAB-VAL-v1` **`r8`** (`VAL-101`), `LAB-PERM-v1` **rev 7** bagian 9.2 — ketiganya `approved` 2026-09-18 |
| Klasifikasi | `MEDIUM` — data induk murni, nol menyentuh alur yang sedang berjalan |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-18 |
| Status | **✅ `SELESAI`** — build hijau 0 error; migration **diterapkan** dan seeder **dijalankan** atas wewenang eksplisit pemilik modul 2026-09-18; `AC-128`..`AC-131` **keempatnya terbukti pada database sungguhan**. `AC-129` dikoreksi dari 21 menjadi **19**, disetujui pemilik modul |

---

## 1. Gerbang yang dilewati sebelum satu baris ditulis

| Gerbang | Hasil |
| --- | --- |
| Blueprint canonical ada | ✅ `LAB-BP-001` revision 68 |
| Task `SIAP DIKERJAKAN` | ✅ dinaikkan revision 68 sesudah kontrak disetujui |
| Requirement/decision ID | ✅ `FR-13.11`, `LAB-DEC-086`, `LAB-DEC-087` |
| Contract version | ✅ `r25` / `r8` / `rev 7`, seluruhnya `approved` 2026-09-18 |
| Dependency | ✅ satu-satunya dependency-nya adalah persetujuan kontrak, dan ia terpenuhi |
| Acceptance criteria | ✅ `AC-128`..`AC-131` |
| Registry prefix | ✅ `LaboratoryManagement` / `Lab` berstatus `ACTIVE`; baris riwayat 2026-09-02 mencakup entity `Lab*` **dan** pembuatan migration |
| Manifest revision/hash | ✅ keempat `input_hashes` **cocok persis** |
| Source SHA | ⚠ bergeser, dan **impact scan dijalankan lebih dulu** — lihat bagian 2 |

### 1.1 Impact scan yang dijalankan sebelum gerbang dinyatakan lolos

Kedua SHA pada manifest sudah tidak lagi menunjuk `HEAD`, sehingga area terdampak dihentikan
dan dipindai lebih dulu.

| Repository | Tercatat | Sebenarnya | Isi | Dampak |
| --- | --- | --- | --- | --- |
| Backend | `13665452` | `5ee03294` | Satu commit `updates BE modul lab` — 44 berkas, 10 di antaranya source Laboratorium | **Nol** |
| Frontend | `686038858` | `f89b728b7` | Satu commit `updates FE modul lab` — 25 berkas Laboratorium | **Nol** |

Isinya pekerjaan yang **sudah** dirancang dan dilaporkan: `LabOrderNumberService`,
`LabEncounterContinuationProbe`, DTO penerimaan, serta layar Penerimaan Sampling/Specimen.
Penelusuran `LabPathology` di seluruh backend menghasilkan **nol kemunculan**, sehingga nol
bagian rancangan ini bersandar pada sesuatu yang bergeser. Kedua nilai sudah disegarkan pada
manifest revision 68.

---

## 2. Selisih aritmetika `AC-129` — dilaporkan, tidak ditambal diam-diam

**`AC-129` menuntut keberlakuan berisi 21 pasangan. Rinciannya sendiri berjumlah 19.**

`roadmap/backend-roadmap.md` dan `02-backend-architecture.md` bagian 15.12 sama-sama menulis
**21**, dan keduanya membawa rincian yang sama: *"Histologi 3, Sitologi Non-Gin 3, Sitologi Gin 3,
IHK 10"*. Ketiganya ditambah sepuluh berjumlah **19**.

Bukti sumbernya, `LAB-EVD-003` bagian 5.6, juga menghasilkan 19 dan dapat dihitung ulang:

| Golongan | Ruas | Jumlah |
| --- | --- | ---: |
| Histologi | Makroskopik, Mikroskopik, Kesimpulan | 3 |
| Sitologi Non-Ginekologi | Makroskopik, Mikroskopik, Kesimpulan | 3 |
| Sitologi Ginekologi | Kondisi, Kategori, Anjuran | 3 |
| Imunohistokimia | Diagnosa Klinis, Diagnosa PA, ER, PR, HER2, Ki-67, Status ER, Status PR, HER2 IHK, Anjuran | 10 |
| | **Total** | **19** |

**Penjelasan selisih 19 terhadap 15 ruas berbeda**, yang justru merupakan hal yang `AC-129`
hendak jaga: **empat** ruas dipakai lebih dari satu golongan — `MAKROSKOPIK`, `MIKROSKOPIK`,
`KESIMPULAN` (Histologi + Sitologi Non-Ginekologi) dan `ANJURAN` (Sitologi Ginekologi +
Imunohistokimia). Karena itu 15 + 4 = **19**.

Catatan risiko pada roadmap memperingatkan `AC-129` *"mudah dibaca sebagai 15 pasangan, bukan 21"*
— peringatan itu benar arahnya dan salah angkanya: selisihnya memang parameter yang dipakai dua
golongan, dan selisih itu bernilai **empat**, bukan enam.

**Yang dikerjakan:** seeder diisi **19 pasangan** sesuai bukti. Angka 21 tidak dipakai, dan
**tidak** dicari-carikan empat pasangan tambahan agar cocok — mengarang keberlakuan berarti
memunculkan ruas pada formulir diagnostik yang nol pernah diminta siapa pun.

> ### ✅ DIPUTUSKAN 2026-09-18 — `19` yang benar
>
> Pemilik modul menyetujui koreksi. `AC-129` pada `roadmap/backend-roadmap.md` dan bagian 15.12
> pada `02-backend-architecture.md` **sudah diperbarui menjadi 19**, keduanya beserta catatan
> asal-usul selisihnya sehingga angka 21 tidak muncul kembali dari dokumen lain.
>
> Database kemudian membuktikan angkanya sendiri: `LabPathologyParameterCategory` berisi **19**
> baris, terpecah **3 / 3 / 3 / 10** persis seperti rincian yang dibawa kedua dokumen — lihat
> bagian 4.

---

## 3. Yang dibangun

### 3.1 Empat model

| Model | Isi | Yang tidak terlihat dari namanya |
| --- | --- | --- |
| `LabPathologyCategory` | `CategoryCode` (32), `CategoryName` (128), `SortOrder`, `IsActive` | **Tabel, bukan enum.** Golongan kelima kelak cukup menambah satu baris; sebagai enum ia menuntut rilis backend — dan `LAB-EVD-003` sudah membuktikan daftarnya belum selesai |
| `LabPathologyParameter` | `ParameterCode` (32), `ParameterName` (200), `SortOrder`, `IsActive` | Ruas laporan menjadi **data**, bukan kolom. Inilah yang membatalkan tiga kolom `Pathology*` pada rencana `BE-LAB-45` |
| `LabPathologyParameterCategory` | `LabPathologyParameterId`, `LabPathologyCategoryId`, `IsRequired` | **`IsRequired` adalah penegak `INV-34`.** Kelengkapan laporan diuji terhadap kolom ini, bukan terhadap tiga nama kolom yang ditulis di kode — persis sebab `VAL-88` dicabut |
| `LabProcedurePathologyCategory` | `ProcedureId` (unik parsial), `LabPathologyCategoryId` | **`MstProcedure` nol disentuh.** Tabel ini milik Laboratorium dan hanya *menunjuk* katalog modul lain |

### 3.2 Satu migration — `20260918060130_AddLabPathologyMasterData`

Empat `CreateTable`, **nol `AddColumn`, nol `AlterColumn`, nol `DropColumn`**. `Down` berisi
`DROP TABLE` saja. Seluruh foreign key `Restrict`.

### 3.3 Tiga controller, dan bukan dua

**Ini selisih terhadap cakupan yang direncanakan, dan disengaja.** Roadmap menulis *"dua
controller"*; kontrak `r25` bagian 20.3 menetapkan **tiga base URL**:

| Base URL | Controller | Hak akses |
| --- | --- | --- |
| `.../lab-pathology-parameters` | `LabPathologyParameterController` | `LabPathologyParameter : Read/Create/Update` |
| `.../lab-pathology-categories` (+ `/{id}/parameters`) | `LabPathologyCategoryController` | `LabPathologyCategory : Read/Create/Update` |
| `.../lab-procedure-pathology-categories` | `LabProcedurePathologyCategoryController` | `LabPathologyCategory : Read/Update` |

Keberlakuan memang menumpang pada controller golongan — ia sub-jalur `lab-pathology-categories`,
sesuai kontrak dan sesuai `rev 7` bagian 9.2 yang menolak mendirikannya sebagai resource
tersendiri. Yang **tidak** dapat menumpang adalah pemetaan: base URL-nya berbeda. Memaksanya
masuk ke controller golongan berarti menulis route absolut yang melawan pola seluruh repository,
atau menggeser base URL-nya dan **mengubah kontrak yang baru disetujui secara sepihak**.
Angka "dua" diperlakukan sebagai taksiran perencanaan, dan kontrak yang diikuti.

### 3.4 Satu seeder — tiga data induk, bukan empat

`LabPathologyMasterDataSeeder` mengisi golongan, ruas, dan keberlakuan. **Pemetaan jenis
pemeriksaan sengaja tidak diseed**: ia bergantung katalog rumah sakit yang bersangkutan dan
pengisiannya milik kepala instalasi bersama `DR-LAB-003`.

Seeder menuliskan **peringatan penyalaan** ketika pemetaan masih kosong, sebab selama ia kosong
nol pemeriksaan Patologi Anatomi punya golongan — `INV-39` melarang sistem menebak — sehingga
formulir hasilnya kosong sama sekali. Tanpa peringatan itu, keadaan tersebut baru ketahuan ketika
patolog pertama membuka layarnya.

Seeder **hanya menambah kode yang belum ada** dan tidak pernah menimpa baris tersimpan. Pasangan
keberlakuan yang sudah ditandai terhapus **ikut dihitung sebagai ada**: keberlakuan yang sengaja
dicabut kepala instalasi tidak boleh dipasang kembali diam-diam setiap kali aplikasi menyala.

---

## 4. Verifikasi

| # | Pemeriksaan | Hasil | Bukti |
| ---: | --- | --- | --- |
| 1 | `dotnet build -p:RunAnalyzers=False` | ✅ **0 error**, 209 warning | Seluruh warning pre-existing (`CS1573`/`CS1574` XML doc lintas modul); **nol** berasal dari berkas baru |
| 2 | **`AC-128`** keempat index unik **parsial** | ✅ **4/4** | `grep -c "unique: true"` = 4 dan `grep -c 'filter: "\"IsDelete\" = false"'` = 4 pada migration |
| 3 | **`AC-130`** nol endpoint `DELETE` | ✅ **0/0/0** | `grep -c HttpDelete` = 0 pada ketiga controller |
| 4 | **`AC-131`** `GET /suggestions` mengusulkan, bukan menyimpan | ✅ | `GetSuggestionsAsync` berada pada baris 145-238; satu-satunya `Add(` ada di baris 285 (`CreateAsync`) dan satu-satunya `SaveChangesAsync` di baris 429 (`SaveAsync`) — keduanya **di luar** jalurnya |
| 5 | Nol `ALTER TABLE` pada tabel berisi data | ✅ | `grep -c "AlterColumn\|AddColumn\|DropColumn"` = 0 pada migration |
| 6 | **`AC-129`** seeder mengisi 4 / 15 / **19** | ✅ **terbukti** | Log penyalaan: `CategoryCount: 4, ParameterCount: 15, ApplicabilityCount: 19` |
| 7 | **Keempat tabel berdiri pada database** | ✅ **4/4** | `information_schema.tables` — lihat bagian 4.2 |
| 8 | **Keempat index unik PARSIAL pada database** | ✅ **4/4** | `pg_index.indpred` = `("IsDelete" = false)` pada keempatnya |
| 9 | **Isi ketiga data induk tetap** | ✅ **4 / 15 / 19 / 0** | Pemetaan `0` **memang disengaja** |
| 10 | Peringatan pemetaan kosong benar-benar muncul | ✅ | Log `Warning` pada penyalaan |

### 4.1 Wewenang eksekusi database

`DefaultConnection` pada `appsettings.Development.json` menunjuk **`Host=160.22.250.77`**,
basis data **non-lokal** — di luar bawaan wewenang task. **Pemilik modul memberi wewenang
eksplisit pada 2026-09-18**, sehingga `dotnet ef database update` dijalankan terhadap dev miliknya
sendiri (`QuilvianNewDevYoga`), lalu aplikasi dinyalakan sekali supaya seeder penyalaan berjalan
dan dihentikan kembali sesudah verifikasi. Nol reset database, nol perubahan credential, nol
eksekusi di luar dev pemilik.

`dotnet ef migrations list` sesudahnya menunjukkan `20260918060130_AddLabPathologyMasterData`
**terterap**. `20260917065900_AddLabExaminationResultEntry` milik `BE-LAB-43` juga tercatat
terterap — laporan task itu sendiri mencatat migration-nya sudah diterapkan 2026-09-17, sehingga
satu-satunya migration baru pada penerapan ini adalah milik `BE-LAB-50`.

### 4.2 Keluaran pemeriksaan database — read-only

```text
===== 1. KEEMPAT TABEL BERDIRI =====
LabPathologyCategory
LabPathologyParameter
LabPathologyParameterCategory
LabProcedurePathologyCategory
(4 baris)

===== 2. INDEX UNIK: apakah PARSIAL (AC-128) =====
index_name                                              | is_unique | predicate
IX_LabPathologyCategory_CategoryCode                    | True      | ("IsDelete" = false)
IX_LabPathologyParameterCategory_ParameterId_CategoryId | True      | ("IsDelete" = false)
IX_LabPathologyParameter_ParameterCode                  | True      | ("IsDelete" = false)
IX_LabProcedurePathologyCategory_ProcedureId            | True      | ("IsDelete" = false)
(4 baris)

===== 3. ISI KETIGA DATA INDUK TETAP (AC-129) =====
LabPathologyCategory                           | 4
LabPathologyParameter                          | 15
LabPathologyParameterCategory                  | 19
LabProcedurePathologyCategory (sengaja kosong) | 0

===== 4. KEBERLAKUAN PER GOLONGAN =====
HISTO       | Histologi               |  3 ruas |  3 wajib
SITO_GIN    | Sitologi Ginekologi     |  3 ruas |  3 wajib
SITO_NONGIN | Sitologi Non-Ginekologi |  3 ruas |  3 wajib
IHK         | Imunohistokimia         | 10 ruas | 10 wajib

===== 5. RUAS YANG DIPAKAI LEBIH DARI SATU GOLONGAN =====
ANJURAN | Anjuran | 2
KESIMPULAN | Kesimpulan | 2
MAKROSKOPIK | Makroskopik | 2
MIKROSKOPIK | Mikroskopik | 2
(4 baris)

===== 6. BENTUK FORMULIR IHK (urut tampil) =====
 6 | Diagnosa Klinis                         | True
 7 | Diagnosa PA                             | True
 8 | Reseptor Estrogen (ER)                  | True
 9 | Reseptor Progesteron (PR)               | True
10 | HER2                                    | True
11 | Ki-67                                   | True
12 | Status Reseptor Estrogen (ER)           | True
13 | Status Reseptor Progesteron (PR)        | True
14 | HER2 dengan pemeriksaan Imunohistokimia | True
99 | Anjuran                                 | True
(10 baris)
```

**Bagian 4 dan 5 adalah pembuktian aritmetika `AC-129` oleh database sendiri:** 3+3+3+10 = **19**,
dan selisihnya terhadap 15 ruas adalah **empat** ruas yang dipakai dua golongan. Bukan 21, dan
bukan enam.

**Bagian 6 dibaca terhadap `LAB-EVD-003` bagian 5.6 kata demi kata** — urutan sepuluh ruas
Imunohistokimia cocok persis, termasuk `Anjuran` yang jatuh paling akhir lewat `SortOrder` 99
walaupun ia ruas yang sama dengan milik Sitologi Ginekologi.

> **Pemeriksaannya read-only**, dijalankan lewat program sekali pakai di direktori scratchpad yang
> sudah dihapus sesudahnya. Nol berkas ditambahkan ke repository untuk keperluan ini.

---

## 5. Yang **tidak** dibangun, dan sebabnya

| Hal | Alasan |
| --- | --- |
| Tabel laporan, nilai, dan konteks klinis | `BE-LAB-51` — gelombang `MVP-6b2` |
| Enam endpoint laporan dan konteks klinis | `BE-LAB-52` |
| Validasi dan rilis laporan | `S4e`, tertahan `DEC-LAB-011` (`LAB-REQ-013`, dr. Arya Wicaksana) |
| Penyimpanan gambar Patologi Anatomi | `DEC-LAB-016` |
| Ruas sumber HL7 | `LAB-COORD-012` — nol kemunculan HL7 di seluruh backend |
| Cetak laporan bahasa Inggris | `LAB-COORD-013` |
| Isi pemetaan jenis pemeriksaan | Bukan pekerjaan programmer; kepala instalasi bersama `DR-LAB-003` |
| `GET /{id}` pada ketiga resource | **Tidak ada di kontrak `r25` bagian 20.3** — lihat bagian 6 |

---

## 6. Temuan yang dibawa keluar

### 6.1 `AC-129` berselisih empat pasangan — ✅ **ditutup 2026-09-18**

Dikoreksi menjadi **19** atas persetujuan pemilik modul, pada roadmap dan arsitektur sekaligus.
Lihat bagian 2.

### 6.2 Ketiga resource nol punya `GET /{id}`, dan modul ini pernah membayar persis itu

Kontrak `r25` bagian 20.3 menyediakan `GET`, `GET /options`, `POST`, dan `PUT /{id}` — **nol
jalur detail satu baris**. Formulir ubah yang dibuka lewat tautan langsung atau sesudah halaman
disegarkan karena itu nol punya jalur untuk memuat barisnya.

**Ini kelas kesalahan yang sudah pernah dibayar modul ini.** `LAB-API-v1` `r6` lahir tepat untuk
menambahkan `GET /lab-rejection-reasons/{id}` sesudah `FE-LAB-03` terbukti memuat barisnya dari
halaman daftar yang sedang terbuka lalu **diam-diam gagal** di luar itu.

**Tidak ditambal sepihak.** Menambahkan endpoint yang tidak ada pada kontrak yang baru disetujui
beberapa jam sebelumnya adalah perubahan kontrak, bukan pelaksanaannya. Diusulkan sebagai `r26`
bila pemilik modul menghendaki — cakupannya tiga endpoint baca, aditif, nol migration.

### 6.3 Penonaktifan menumpang pada `PUT /{id}`

Konsekuensi langsung dari nol `DELETE` **dan** nol jalur aktivasi pada kontrak. Berbeda dari
`LabSpecimenType` yang punya `PUT /{id}/activation` tersendiri. Dicatat supaya `FE-LAB-27` nol
mencari jalur yang memang tidak ada.

### 6.4 Kata kunci usulan menoleransi spasi

`LAB-DEC-087` menyebut kata kunci `PAPSMEAR` menyatu, sedangkan katalog rumah sakit lazim menulis
`PAP SMEAR`. Pembanding pada `GET /suggestions` karena itu mengabaikan spasi **dan** besar kecil
huruf. Keputusan pelaksanaan, bukan perluasan daftar kata kunci: keenamnya tetap enam. Dicatat
karena ia melonggarkan pencocokan melebihi bunyi harfiah keputusannya — dan hasilnya tetap
diperiksa manusia sebelum disimpan.

### 6.5 `BE-LAB-44` belum pernah dibangun

Cakupan `BE-LAB-50` menyebut reuse *"pola `LabSpecimenType` dan `BE-LAB-44` sepenuhnya"*.
Penelusuran `LabOrganism`/`LabAntibiotic` di seluruh backend menghasilkan **nol kemunculan**, dan
nol laporan `BE-LAB-44.md` berdiri. Pola acuan yang benar-benar dipakai karena itu
**`LabSpecimenType` saja**. Nol menahan task ini — `BE-LAB-44` bukan dependency-nya — tetapi
dicatat supaya urutan gelombang `MVP-6c` tidak dikira sudah berjalan.

### 6.6 `LAB-SRC-UNCOMMITTED` masih aktif

`S4a` tetap hanya hidup di working tree. `BE-LAB-50` nol bergantung padanya, tetapi pekerjaan ini
menambah berkas pada repository yang sebagian slice-nya belum pernah masuk commit mana pun.

---

## 7. Berkas

### 7.1 Baru — 13 berkas

| Jalur | Isi |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabPathologyCategory.cs` | Model golongan |
| `Areas/.../Models/LabPathologyParameter.cs` | Model ruas isian |
| `Areas/.../Models/LabPathologyParameterCategory.cs` | Model keberlakuan |
| `Areas/.../Models/LabProcedurePathologyCategory.cs` | Model pemetaan |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabPathologyCategoryConfiguration.cs` | Index unik parsial `CategoryCode` |
| `Repositories/.../LabPathologyParameterConfiguration.cs` | Index unik parsial `ParameterCode` |
| `Repositories/.../LabPathologyParameterCategoryConfiguration.cs` | Index unik parsial pasangan |
| `Repositories/.../LabProcedurePathologyCategoryConfiguration.cs` | Index unik parsial `ProcedureId` |
| `Areas/.../DTOs/LabPathologyMasterDataDtos.cs` | 19 DTO |
| `Areas/.../Services/LabPathologyParameterService.cs` | Service ruas + pembantu bersama + dua exception |
| `Areas/.../Services/LabPathologyCategoryService.cs` | Service golongan + keberlakuan |
| `Areas/.../Services/LabProcedurePathologyCategoryService.cs` | Service pemetaan + usulan |
| `Areas/.../Seeders/LabPathologyMasterDataSeeder.cs` | Seeder tiga data induk tetap |
| `Areas/.../Controllers/LabPathologyParameterController.cs` | 4 endpoint |
| `Areas/.../Controllers/LabPathologyCategoryController.cs` | 6 endpoint |
| `Areas/.../Controllers/LabProcedurePathologyCategoryController.cs` | 4 endpoint |
| `Migrations/20260918060130_AddLabPathologyMasterData.cs` (+ `.Designer.cs`) | Empat tabel |

### 7.2 Diubah — 3 berkas

| Jalur | Perubahan |
| --- | --- |
| `Repositories/ApplicationDbContext.cs` | Empat `DbSet` |
| `Program.cs` | Tiga `AddScoped` + satu `RunStartupSeederAsync` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disegarkan EF |

**Perubahan tidak terkait di working tree dipertahankan apa adanya.** Nol `git add`, nol commit,
nol push.

---

## 8. Langkah berikutnya

| # | Langkah | Pemilik | Keadaan |
| ---: | --- | --- | --- |
| ~~1~~ | ~~Putuskan `AC-129`: 19 atau 21~~ | Pemilik modul | ✅ **Selesai** — 19, dokumennya dikoreksi |
| ~~2~~ | ~~Terapkan migration, jalankan seeder, buktikan pada database~~ | Pemilik modul | ✅ **Selesai** — 4 tabel, 4 index parsial, 4/15/19 baris |
| 3 | Putuskan apakah `GET /{id}` diusulkan sebagai `r26` | Pemilik modul | Terbuka |
| 4 | Isi pemetaan jenis pemeriksaan lewat `GET /suggestions` lalu `POST` | Kepala instalasi + `DR-LAB-003` | **Penahan nyata `FE-LAB-28`** |
| 5 | `BE-LAB-51` — tabel laporan, nilai, dan konteks klinis | Berikutnya | Siap dikerjakan |
