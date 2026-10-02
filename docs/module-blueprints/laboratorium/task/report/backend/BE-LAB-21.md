# Laporan Perubahan Backend — `BE-LAB-21`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-21` |
| Judul | Jenis dan volume pada wadah |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.1`, `FR-11.3`, `FR-11.4`; `LAB-DEC-040`, `LAB-DEC-041`, BR-35, BR-36, `RULE-021`; `AC-58`, `AC-59`, `AC-61`, `AC-62`, `AC-63`, `AC-64`; `VAL-51`..`VAL-57` |
| Contract version | `LAB-API-v1` `r7` perluasan `POST /lab-specimens/by-order/{labOrderId}`; `LAB-VAL-v1` `r4` — keduanya `approved`, disetujui pemilik modul 2026-09-14 |
| Dependency | `BE-LAB-20` ✅ `SELESAI` 2026-09-14. **Gerbang data `DATA-MST-MEASUREMENT` masih terbuka** — lihat bagian 7 |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — 2026-09-15. Source, configuration, migration, **dan eksekusi ke `QuilvianNewDevYoga`** selesai dan terverifikasi; jalur `Down` lalu `Up` ikut dibuktikan. Volume bersatuan **`mL` dan `gram` dapat dipakai sekarang**; `µL`, `blok`, dan `slide` masih menunggu Master Data — lihat bagian 7 |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 — `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 |
| Keberlakuan | `NEW CODE` untuk keempat kolom, kedua relasi, ruas DTO, dan aturan validasinya. Berkas induknya — `LabSpecimen`, configuration, service, controller — sudah selaras kontrak sejak `BE-LAB-11`, sehingga tidak ada pelanggaran legacy yang perlu diperbaiki di sini |
| Status gerbang | `QBE-MOD-002` dan `QBE-MOD-003` **tidak menahan** — modulnya sudah terdaftar dan tidak ada entity baru yang dibuat task ini |
| QBE ID yang berlaku | `QBE-ENT-002`, `QBE-CFG-001`, `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-PERM-001`, `QBE-AUD-001` |

**Tidak ada entity baru, sehingga tidak ada prefix baru.** Task ini menambah kolom pada
`LabSpecimen` yang sudah ada dan menunjuk dua tabel yang sudah ada — `LabSpecimenType` milik
Laboratorium dan `MstMeasurement` milik `master-data`.

---

## 1. Masalah yang diperbaiki

Sampai hari ini sebuah wadah sampel laboratorium tidak dapat menyatakan **dua hal paling dasar
tentang isinya**: bahannya apa, dan berapa banyak.

Jenisnya hanya tersimpan pada `SpecimenDescription` — kolom teks bebas. `BE-LAB-20` sudah
membangun daftar terkendalinya, tetapi daftar itu **belum tersambung ke wadah mana pun**; ia
berdiri sendiri sebagai data induk yang tidak pernah dipakai. Selama sambungannya belum ada,
petugas tetap mengetik "cairan kista", "Cairan Kista", dan "c. kista" sebagai tiga hal berbeda,
dan laporan jenis sampel tetap tidak dapat dipercaya.

Volumenya tidak tersimpan sama sekali. Akibatnya terlihat paling jelas pada patologi anatomi:
sepotong jaringan yang datang dalam bentuk **3 blok** dan sepotong yang datang sebagai **3
slide** tercatat sama persis di sistem, padahal keduanya pekerjaan yang berbeda. Dan ketika
volume akhirnya dicatat, angka tanpa satuan justru berbahaya — "3" bisa berarti 3 mL atau 3
slide, dan tidak ada cara membedakannya setelah petugasnya pulang.

---

## 2. Proses bisnis

**Pelaku.** Petugas penerimaan sampling/specimen.
**Pemicu.** Sebuah wadah fisik — tabung, pot, atau slide — dicatat pada sebuah pesanan
laboratorium.

**Langkah jalur normal:**

1. Petugas membuka layar wadah pada satu pesanan laboratorium.
2. Ia memilih **jenis bahan** dari daftar yang dikembalikan
   `GET /lab-specimen-types/options` — daftar itu hanya memuat jenis yang aktif.
3. Ia mengisi **volume** beserta **satuannya**, bila volumenya memang diketahui.
4. Ia memilih pemeriksaan yang akan dikerjakan dari wadah itu, lalu menyimpan.
5. Wadah tersimpan berstatus `Planned` dengan barcode baru, membawa jenis, keterangan, volume,
   dan satuannya.

**Contoh berangka.** Satu tabung darah ungu: jenis `Blood`, volume `3`, satuan `mL`. Tersimpan
sebagai `VolumeAmount = 3.000` pada kolom `numeric(12,3)` dan `VolumeUnitId` menunjuk baris
`ML`. Responsnya membawa `volumeUnitSymbol: "mL"`, sehingga layar menampilkan **3 mL** dan
angkanya tidak pernah berdiri sendirian.

**Jalur tidak normal — jenisnya belum terdaftar.** Sampel cairan kista datang pukul 21.00 dan
jenis itu belum ada di daftar. Petugas memilih **`Lainnya`** lalu menuliskan "cairan kista" pada
kolom keterangan. **Penerimaannya tetap berhasil.** Inilah inti `LAB-DEC-040`: daftar yang
terkendali tidak boleh menjadi jalan buntu, karena sampel tidak menunggu — ia rusak. Keterangan
itu kelak muncul pada daftar pantau kepala instalasi yang dibangun `BE-LAB-25`, dan dari situ
jenisnya dapat dinaikkan menjadi jenis tetap.

**Tujuh penolakan yang mungkin terjadi:**

| Aturan | Kapan menolak | Yang dibaca petugas |
| --- | --- | --- |
| `VAL-51` | Jenis tidak dipilih sama sekali | "Pilih jenis specimen terlebih dahulu." |
| `VAL-52` | Jenis dikirim sebagai teks, atau penunjuknya tidak ada pada daftar | "Jenis specimen harus dipilih dari daftar. Bila jenisnya belum ada, pilih Lainnya lalu tuliskan keterangannya." |
| `VAL-53` | `Lainnya` dipilih tetapi keterangannya kosong | "Tuliskan jenis specimennya pada kolom keterangan." |
| `VAL-54` | Keterangan `Lainnya` dikirim padahal jenisnya bukan `Lainnya` | "Keterangan jenis hanya diisi bila jenisnya Lainnya." |
| `VAL-55` | Jenis yang dipilih sudah dinonaktifkan | "Jenis specimen ini sudah tidak dipakai lagi. Pilih jenis lain." |
| `VAL-56` | Volume diisi tanpa satuan | "Pilih satuan volumenya." |
| `VAL-57` | Satuan yang dipilih bukan satuan laboratorium | "Satuan ini tidak dipakai laboratorium. Pilih dari daftar satuan yang tersedia." |

Seluruhnya `422`, bukan `400`: bentuk permintaannya benar, isinya yang melanggar aturan.

**Jalur pengambilan ulang.** Ketika sebuah wadah ditolak lalu diminta diambil ulang, wadah
pengganti **mewarisi jenis bahan dan keterangan `Lainnya`-nya**. Alasannya: bahannya diambil
ulang, bukan berganti jenis, dan permintaan pengambilan ulang memang tidak punya ruas untuk
menyatakan jenis. **Volumenya tidak ikut diwariskan** — bahan penggantinya belum diambil,
sehingga berapa banyak yang akan terkumpul belum diketahui, dan menyalin angka lama berarti
mencatat sesuatu yang tidak pernah diukur.

**Yang sengaja tidak dilakukan sistem.** Sistem **tidak** menolak volume yang kecil dan **tidak**
menghitung sendiri apakah bahannya cukup. `RULE-021` menyatakan tidak ada ketentuan bisnis batas
minimum maupun maksimum, dan `LAB-DEC-041` menerimanya apa adanya. Yang menyatakan bahan tidak
cukup adalah **petugas**, lewat penetapan kelayakan (`AC-63`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 6b | Cakupan, DoD, dan batas task |
| `contracts/api-contract.md` bagian perluasan `r7` | Bentuk request dan response |
| `contracts/validation-matrix.md` bagian 7b | Bunyi `VAL-51`..`VAL-57` persis |
| `erd/data-dictionary.md` bagian 12.2 dan 12.4 | Tipe kolom, nullability, nama index dan FK |
| `02-backend-architecture.md` bagian 11.4, 11.8, 11.10 | Alasan nullability dan rencana migration |
| `testing/acceptance-test-matrix.md` bagian 11.3, 11.4, 11.7 | Skenario `T-58a`..`T-64a`, `T-M2`, `T-M4` |
| `task/report/backend/BE-LAB-20.md` | Pola data induk yang menjadi lawan relasi |
| `Models/LabSpecimen.cs`, `LabSpecimenConfiguration.cs` | Kolom dan relasi yang sudah ada |
| `Services/LabSpecimenService.cs` | Jalur perencanaan, pengambilan ulang, dan pemetaan respons |
| `Controllers/LabSpecimenController.cs` | Pemetaan exception ke status dan bentuk respons |
| `Areas/HealthServices/MasterData/Models/MstMeasurement.cs` | Kolom `IsForLaboratory`, `MeasurementSymbol` |
| `QuilvianSystemFrontendDev` — `lab-specimen.service.js`, `use-lab-specimen-workspace.jsx` | Muatan yang dikirim konsumen yang sudah ada (read-only) |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabSpecimen.cs` | Empat kolom baru (`SpecimenTypeId`, `SpecimenTypeOtherNote`, `VolumeAmount`, `VolumeUnitId`), dua navigation (`SpecimenType`, `VolumeUnit`), dan penegasan makna `SpecimenDescription` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabSpecimenConfiguration.cs` | Panjang `SpecimenTypeOtherNote` 128, presisi `numeric(12,3)`, dua relasi `DeleteBehavior.Restrict` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs` | Empat ruas pada `PlanLabSpecimenRequest`; enam ruas pada `LabSpecimenResponse` — keempat kolom ditambah `SpecimenTypeName` dan `VolumeUnitSymbol` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | `ResolveSpecimenMaterialAsync` menegakkan `VAL-51`..`VAL-57`; `CreateSpecimenAsync` menyimpan bahannya; jalur pengambilan ulang mewariskan jenis; proyeksi daftar dan `LoadSpecimenAsync` membawa nama jenis dan simbol satuan |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs` | Enam ruas baru pada `MapResponse` |
| `Migrations/20260915035317_AddLabSpecimenTypeAndVolumeColumns.cs` | **Baru.** Empat kolom nullable, dua index, dua FK `RESTRICT` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tergenerasi mengikuti migration di atas |

**Diff-nya aditif, satu baris dokumentasi terhapus.** `git diff --stat` atas source aplikasi:
503 baris bertambah, **1 baris terhapus** — baris ringkasan XML lama `SpecimenDescription` yang
digantikan penjelasan barunya. Tidak satu pun kolom, ruas, endpoint, atau perilaku lama yang
dihapus atau berganti nama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /lab-specimens/by-order/{labOrderId}` menerima empat ruas baru, satu di antaranya **wajib**. Seluruh jalur baca wadah membawa enam ruas baru, bersifat tambahan. Sesuai `LAB-API-v1` `r7`; tidak ada revisi kontrak baru yang diperlukan |
| Database | Empat kolom **nullable** pada `LabSpecimen`, dua index, dua foreign key `RESTRICT`. Migration **dibuat**, belum diterapkan ke database mana pun. Karena seluruh kolom nullable, `ALTER TABLE`-nya tidak menulis ulang satu baris pun dan tidak mengunci tabel yang sudah berisi data |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada permission, `[AccessAction]`, maupun `[AccessPermission]` yang berubah. `POST /by-order/{labOrderId}` tetap `LabSpecimen : Plan`, dan membaca daftar jenis tetap memerlukan `LabSpecimenType : Read` yang terpisah |

### 3.4 Satu keputusan teknis yang perlu diketahui pembaca berikutnya

Baris `LabSpecimenType` dan `MstMeasurement` dimuat **terlacak**, bukan `AsNoTracking`, walaupun
keduanya hanya dibaca. Ini disengaja dan alasannya ada di
`ResolveSpecimenMaterialAsync` beserta `CreateSpecimenAsync`:

Wadah baru menyimpan **navigation**-nya, bukan hanya foreign key-nya, supaya respons tindakan
dapat menyebut nama jenis dan simbol satuannya tanpa satu query tambahan. Menempelkan entity
yang **tidak terlacak** pada entity yang sedang berstatus `Added` membuat EF ikut menandainya
`Added` — dan baris data induk yang sudah ada akan disisipkan ulang, lalu ditolak index unik
`IX_LabSpecimenType_SpecimenTypeCode` yang dipasang `BE-LAB-20`. Kegagalannya akan muncul sebagai
`23505` yang membingungkan pada saat menyimpan wadah, jauh dari sebabnya.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Specimen

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-order/{labOrderId:guid}` | Mencatat wadah beserta **jenis bahan, keterangan `Lainnya`, volume, dan satuannya** | `LabSpecimen : Plan` |
| `GET` | `/by-order/{labOrderId:guid}` | Daftar wadah pada satu pesanan, kini membawa jenis dan volume beserta nama dan simbolnya | `LabSpecimen : Read` |

Endpoint tindakan lain — `/collect`, `/receive`, `/accept`, `/reject`, `/request-recollection`,
`/hold`, `/resume`, `/cancel` — **tidak berubah kontraknya**; responsnya ikut membawa keenam ruas
baru karena semuanya memakai pemetaan respons yang sama.

**Ruas baru pada `PlanLabSpecimenRequest`:**

| Ruas | Tipe | Wajib | Catatan |
| --- | --- | :---: | --- |
| `specimenTypeId` | `guid` | **Ya** | Dipilih dari `GET /lab-specimen-types/options` |
| `specimenTypeOtherNote` | `string(128)` | Kondisional | Wajib bila jenisnya `Lainnya`, ditolak bila bukan |
| `volumeAmount` | `decimal` | Tidak | Lihat bagian 5.3 |
| `volumeUnitId` | `guid` | Kondisional | Wajib bila `volumeAmount` diisi |

**Ruas baru pada `LabSpecimenResponse`:** `specimenTypeId`, `specimenTypeName`,
`specimenTypeOtherNote`, `volumeAmount`, `volumeUnitId`, `volumeUnitSymbol`.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab`, tidak ada entity baru | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 |
| Review diff/scope | 5 berkas source disentuh secara aditif, 2 berkas migration baru, 1 snapshot tergenerasi | `PASS` | `git diff --stat` — 503 tambah, 1 hapus |
| `dotnet build -p:RunAnalyzers=False --no-incremental` | **0 Error, 191 Warning** | `PASS` | Lihat 5.1 |
| Verifikasi kontrak API terhadap `r7` | Empat ruas request dan enam ruas response terpasang; tidak ada endpoint lama yang berubah | `PASS` | `LabSpecimenDtos.cs:40-74`, `:152-173`; `LabSpecimenController.cs:351-356` |
| Bentuk SQL migration | Empat `ADD COLUMN` nullable, dua `CREATE INDEX`, dua FK `ON DELETE RESTRICT` | `PASS` | Lihat 5.2 |
| `T-63c` — tidak ada jalur kode yang membandingkan volume terhadap batas | **Nol pembandingan** | `PASS` | Lihat 5.3 |
| `T-58a`, `T-58b`, `T-59a`, `T-59b`, `T-62b` — penolakan `VAL-51`..`VAL-54`, `VAL-56` | Ditegakkan di source, belum dijalankan runtime | `NOT RUN` | `LabSpecimenService.cs:1286-1345` |
| **Eksekusi migration ke `QuilvianNewDevYoga`** | Diterapkan; kelima kolom, kedua FK `RESTRICT`, dan ketiga index terbaca dari database | `PASS` | Lihat 5.5 |
| **`T-M2`** — baris `LabSpecimen` lama tetap terbaca, kolom baru kosong | **5 baris utuh**, 4 di antaranya masih membawa `SpecimenDescription`; nol kolom baru terisi | `PASS` | Lihat 5.5 |
| **`T-M4`** — menghapus jenis yang sudah dipakai wadah ditolak `Restrict` | **Ditolak `23503`** pada `FK_LabSpecimen_LabSpecimenType_SpecimenTypeId` | `PASS` | Lihat 5.5 |
| **Jalur `Down` lalu `Up`** | Terbukti; verifikasi ulang sesudahnya tetap lulus seluruhnya | `PASS` | Lihat 5.6 |
| `T-58c`, `T-59c`, `T-61a` | Menunggu aplikasi dijalankan dengan kredensial pengguna | `NOT RUN` | Lihat 5.4 |
| `T-62c`, `T-64a` | **Sebagian terbuka** — `mL` dan `gram` tersedia, `µL`/`blok`/`slide` belum | `NOT RUN` | Lihat bagian 7 |

### 5.1 Hasil build apa adanya

Build bersih penuh dijalankan, bukan incremental:

```
dotnet build -p:RunAnalyzers=False --no-incremental
    191 Warning(s)
    0 Error(s)
```

**Nol dari 191 warning itu berasal dari kelima berkas yang disentuh task ini.** Diperiksa dengan
menyaring keluaran build atas nama `LabSpecimen.cs`, `LabSpecimenService.cs`,
`LabSpecimenDtos.cs`, `LabSpecimenController.cs`, dan `LabSpecimenConfiguration.cs` — hasilnya
kosong. Seluruh 191 warning adalah peninggalan repository: `CS8619`, `CS1573`, `CS8602`,
`CS0618`, dan sejenisnya, tersebar pada modul lain.

> **Selisih terhadap laporan `BE-LAB-20`.** Laporan itu mencatat *"0 Error, 0 Warning pada build
> bersih terakhir"*. Angka itu **tidak dapat direproduksi**: build bersih penuh pada commit yang
> sama hari ini menghasilkan 191 warning. Kemungkinan besar angka lamanya berasal dari build
> incremental yang melewati `CoreCompile`, sehingga warning berkas yang tidak dikompilasi ulang
> tidak ikut tercetak. Dicatat apa adanya, bukan didiamkan; kesimpulan `BE-LAB-20` sendiri tidak
> berubah karena nol warning-nya memang berasal dari berkas barunya.

### 5.2 Bentuk SQL migration

`dotnet ef migrations script 20260914084730_SeedLabSpecimenType 20260915035317_AddLabSpecimenTypeAndVolumeColumns`

```sql
ALTER TABLE public."LabSpecimen" ADD "SpecimenTypeId" uuid;
ALTER TABLE public."LabSpecimen" ADD "SpecimenTypeOtherNote" character varying(128);
ALTER TABLE public."LabSpecimen" ADD "VolumeAmount" numeric(12,3);
ALTER TABLE public."LabSpecimen" ADD "VolumeUnitId" uuid;
CREATE INDEX "IX_LabSpecimen_SpecimenTypeId" ON public."LabSpecimen" ("SpecimenTypeId");
CREATE INDEX "IX_LabSpecimen_VolumeUnitId" ON public."LabSpecimen" ("VolumeUnitId");
ALTER TABLE public."LabSpecimen" ADD CONSTRAINT "FK_LabSpecimen_LabSpecimenType_SpecimenTypeId"
    FOREIGN KEY ("SpecimenTypeId") REFERENCES public."LabSpecimenType" ("Id") ON DELETE RESTRICT;
ALTER TABLE public."LabSpecimen" ADD CONSTRAINT "FK_LabSpecimen_MstMeasurement_VolumeUnitId"
    FOREIGN KEY ("VolumeUnitId") REFERENCES public."MstMeasurement" ("Id") ON DELETE RESTRICT;
```

Cocok baris demi baris dengan DDL pada `erd/data-dictionary.md` bagian 12.4, dikurangi
`PhysicallyReceivedAt` yang memang cakupan `BE-LAB-22`. Jalur `Down` membuang kedua FK, kedua
index, lalu keempat kolom.

### 5.3 `T-63c` — membuktikan ketiadaan aturan

Butir DoD ini bentuknya **ketiadaan**, sehingga pembuktiannya juga harus berbentuk pencarian yang
tidak menemukan apa-apa. Seluruh rujukan `VolumeAmount` pada source aplikasi ditelusuri:

| Tempat | Apa yang dilakukan |
| --- | --- |
| `LabSpecimen.cs:85` | Deklarasi kolom |
| `LabSpecimenDtos.cs:66`, `:165` | Ruas request dan response |
| `LabSpecimenConfiguration.cs:34` | Presisi `numeric(12,3)` |
| `LabSpecimenService.cs:1343` | `request.VolumeAmount.HasValue` — memeriksa **ada atau tidaknya**, bukan besarnya |
| `LabSpecimenService.cs:969`, `:1368`, `:1392` | Penyalinan nilai |
| `LabSpecimenService.cs:679` | Volume **tidak** diwariskan ke wadah pengganti |
| `LabSpecimenController.cs:354` | Pemetaan respons |

**Tidak ada satu pun `<`, `>`, `<=`, atau `>=` yang mengenai `VolumeAmount`.** Pencarian pola
pembandingan hanya menemukan satu kecocokan, dan itu tanda panah lambda `=>` pada baris
`HasPrecision`, bukan pembandingan.

Nilai `0.001` mL karena itu tersimpan sama mulusnya dengan `3` mL — tanpa peringatan, tanpa
penolakan, tanpa catatan apa pun (`T-63b`, `AC-63`).

### 5.4 Yang belum dijalankan, dan kenapa

| Pemeriksaan | Kenapa belum |
| --- | --- |
| `T-58c`, `T-59c`, `T-61a` | Menunggu aplikasi dijalankan dengan kredensial pengguna. Ketiganya aturan tingkat service yang sudah terverifikasi terhadap source |
| `T-62c` — satuan bukan laboratorium ditolak `VAL-57` | Dapat dijalankan sekarang, tetapi memerlukan aplikasi menyala. Datanya **sudah tersedia**: 17 satuan ber-`IsForLaboratory` dan puluhan yang tidak |
| `T-64a` — jaringan bersatuan `gram`, `blok`, `slide` | **Terbuka sebagian.** `gram` tersedia; `blok` dan `slide` belum ada. Lihat bagian 7 |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi dijalankan dengan kredensial
pengguna. Verifikasi tingkat database sudah dijalankan dan hasilnya ada pada 5.5 dan 5.6.

### 5.5 Eksekusi database — dijalankan 2026-09-15

Wewenang diberikan pemilik modul dengan menyebut targetnya secara tegas: **`QuilvianNewDevYoga`
di `160.22.250.77`**. Konfirmasi yang menyebut host diminta lebih dulu karena databasenya bukan
lokal dan dipakai bersama.

`dotnet ef migrations list` lebih dulu memastikan **hanya dua migration** yang berstatus
`Pending` — `AddLabSpecimenTypeAndVolumeColumns` milik task ini dan
`AddLabSpecimenPhysicallyReceivedAt` milik `BE-LAB-22`. Tidak ada migration modul lain yang ikut
terbawa.

**Bukti terhadap database, dibaca langsung lewat Npgsql:**

| Butir | Hasil |
| --- | --- |
| Keempat kolom task ini | **Ada.** `SpecimenTypeId` `uuid` nullable; `SpecimenTypeOtherNote` `character varying` nullable; `VolumeAmount` **`numeric` presisi 12 skala 3** nullable; `VolumeUnitId` `uuid` nullable |
| `FK_LabSpecimen_LabSpecimenType_SpecimenTypeId` | **Ada, `delete_rule = RESTRICT`** |
| `FK_LabSpecimen_MstMeasurement_VolumeUnitId` | **Ada, `delete_rule = RESTRICT`** |
| `IX_LabSpecimen_SpecimenTypeId`, `IX_LabSpecimen_VolumeUnitId` | **Keduanya terbaca dari `pg_indexes`** |
| **`T-M2`** | **LULUS** — 5 baris `LabSpecimen` lama tetap ada dan terbaca; **4 di antaranya masih membawa `SpecimenDescription`**; nol kolom baru terisi. Tidak satu baris pun hilang atau berubah |
| **`T-M4`** | **LULUS** — satu wadah ditempeli jenis `BLOOD` di dalam transaksi, lalu penghapusan jenis itu **ditolak `23503`** pada `FK_LabSpecimen_LabSpecimenType_SpecimenTypeId`. Inilah foreign key yang ditunggu laporan `BE-LAB-20` bagian 6.3 |
| Kebersihan | **Nol baris uji tertinggal.** Seluruh percobaan `T-M4` dijalankan di dalam transaksi yang **selalu di-`ROLLBACK`**, dan pemeriksaan sesudahnya menunjukkan nol wadah berjenis |

### 5.6 Jalur `Down` dibuktikan

| Langkah | Hasil |
| --- | --- |
| `database update 20260914084730_SeedLabSpecimenType` | `Done.` Kedua migration kembali `Pending` |
| `database update` lagi | `Done.` Nol migration `Pending` |
| Verifikasi ulang sesudahnya | **5 kolom, 2 FK `RESTRICT`, 3 index; 5 baris lama utuh dengan 4 keterangan lama; 7 jenis specimen dengan 1 jalan keluar `Lainnya`; `T-M4` tetap LULUS `23503`; tetap bersih** |

Siklus turun-naik karena itu terbukti utuh, bukan sekadar tidak melempar galat.

### 5.7 Alat verifikasinya, dan kenapa ia di luar repository

Backend tidak memiliki klien SQL. Verifikasi di atas dijalankan satu program sekali pakai yang
dibangun di **scratchpad sesi, di luar repository backend**, membaca connection string langsung
dari `appsettings.Development.json` sehingga kata sandinya tidak pernah melewati baris perintah.

Penempatannya disengaja. `rules/backend/TEST_POLICY.md` bagian 3 melarang membuat kelas,
endpoint, atau runner "uji sementara" **di dalam project aplikasi**. Alat ini tidak menyentuh
repository sama sekali.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Butir DoD roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Keempat kolom ada | **✅ Terbukti di database** | Terbaca dari `information_schema.columns` — 5.5 |
| Kedua FK `Restrict` | **✅ Terbukti di database** | `delete_rule = RESTRICT` pada keduanya, dan `T-M4` ditolak `23503` — 5.5 |
| `VAL-51`..`VAL-57` menolak sesuai matriks | **Terpenuhi pada source**, belum dibuktikan runtime | `LabSpecimenService.cs:1286-1366`; pesan dibandingkan kata demi kata dengan `validation-matrix.md` bagian 7b |
| **Tidak ada satu pun jalur kode yang membandingkan volume terhadap batas minimum** | **Terpenuhi** | Bagian 5.3 |
| Data lama utuh | **✅ Terbukti di database** | `T-M2` — 5 baris utuh, 4 keterangan lama tetap ada, nol kolom baru terisi — 5.5 |
| Migration jalan maju dan mundur | **✅ Terbukti** | `Up`, `Down`, lalu `Up` lagi; verifikasi ulang sesudahnya tetap lulus seluruhnya — 5.6 |

### 6.2 Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-58` — jenis dipilih dari daftar terkendali; teks bebas ditolak | **Terpenuhi pada source.** Melengkapi bagian yang `BE-LAB-20` tandai "sebagian" | `VAL-51`, `VAL-52`, `VAL-55` pada `LabSpecimenService.cs:1288-1321` |
| `AC-59` — `Lainnya` tanpa keterangan ditolak, dan penerimaan `Lainnya` tidak pernah dihalangi | **Terpenuhi pada source.** Jalur `Lainnya` berketerangan tidak melewati satu pun penolakan | `LabSpecimenService.cs:1323-1338` |
| `AC-61` — `SpecimenDescription` bukan lagi satu-satunya tempat jenis tersimpan; data lama tetap terbaca | **Terpenuhi pada rancangan.** Kolom penunjuk berdiri, `SpecimenDescription` tidak disentuh, tidak ada migration pemetaan | `LabSpecimen.cs:45-65`; migration tanpa `UPDATE` |
| `AC-62` — volume tidak dapat disimpan tanpa satuan | **Terpenuhi pada source** | `VAL-56` pada `LabSpecimenService.cs:1343` |
| `AC-63` — volume berapa pun diterima tanpa peringatan | **Terpenuhi dan terbukti** | Bagian 5.3 |
| `AC-64` — jaringan dapat bersatuan `gram`, `blok`, `slide` | **Terpenuhi pada kode; datanya tersedia sebagian** | Kodenya tidak membatasi satuan per jenis sama sekali. **`gram` sudah ada dan ber-`IsForLaboratory`**; `blok` dan `slide` belum — bagian 7 |

---

## 7. `DATA-MST-MEASUREMENT` — dugaan yang terbantah sebagian oleh data sebenarnya

> **Koreksi terhadap revisi pertama laporan ini.** Revisi pertama menyatakan *"selama kelima
> baris itu kosong, setiap upaya mengisi volume akan ditolak `VAL-57`"*. **Pernyataan itu
> keliru**, dan dasarnya adalah asumsi blueprint, bukan pembacaan database. Setelah database
> dibaca langsung pada 2026-09-15, keadaannya berbeda: **17 satuan ber-`IsForLaboratory` yang
> aktif sudah ada**, dan dua di antaranya adalah yang dibutuhkan.

**Keadaan sebenarnya per 2026-09-15 pada `QuilvianNewDevYoga`:**

| Yang diminta `02-backend-architecture.md` 11.10 | Ada? | Baris yang sebenarnya |
| --- | :---: | --- |
| Mililiter, simbol `mL` | **✅ Ada** | `STN250826000074` — nama `ML`, simbol `mL`, tipe `Volume`, desimal 3 |
| Gram, simbol `g` | **✅ Ada, bahkan dua** | `STN250826000127` (`GRAM`) dan `STN250826000136` (`GR`), keduanya simbol `g` |
| Mikroliter, simbol `µL` | **❌ Belum ada** | — |
| Blok, simbol `blok` | **❌ Belum ada** | — |
| Slide, simbol `slide` | **❌ Belum ada** | — |

**Akibatnya, ditulis apa adanya: volume bersatuan `mL` dan `gram` dapat dipakai sekarang juga.**
Wadah darah `3` `mL` — contoh pada butir Verifikasi roadmap — tersimpan tanpa penolakan. Yang
masih tertahan hanya tiga satuan: mikroliter untuk sampel bervolume sangat kecil, serta blok dan
slide untuk patologi anatomi. `AC-64` karena itu terpenuhi **sebagian**, bukan nol.

### 7.1 Permintaan ke Master Data perlu ditulis ulang, bukan diteruskan apa adanya

Dua hal membuat permintaan versi blueprint tidak dapat diajukan begitu saja.

**Pertama, jumlahnya tiga, bukan lima.** Meminta `mL` dan `gram` yang sudah ada akan
menghasilkan baris kembar pada data induk global — persis jenis kekacauan yang `LAB-DEC-040`
hindari pada jenis specimen.

**Kedua, kodenya tidak boleh didikte.** Blueprint meminta kode `ML`, `UL`, `G`, `BLOK`, dan
`SLIDE`. Tidak satu pun kode itu ada di database, dan memang tidak akan ada: `MstMeasurement`
memakai **kode berseri tergenerasi** berbentuk `STN` diikuti tanggal dan nomor urut. Menuntut
kode buatan sendiri berarti meminta `master-data` melanggar konvensi penomorannya sendiri.

Permintaan yang benar karena itu berbunyi: **tiga baris baru — mikroliter (`µL`), blok, dan
slide — ber-`IsForLaboratory = true`, dengan kode mengikuti seri `STN` yang berlaku.** Blok dan
slide tidak boleh mengizinkan desimal; setengah slide tidak berarti apa-apa.

### 7.2 Temuan sampingan yang dilaporkan, bukan diperbaiki

**Penanda `IsForLaboratory` jauh lebih longgar daripada yang diandaikan rancangan.** Ketujuh
belas baris yang membawanya termasuk `GALON`, `M3`, `KG`, `ONS`, `IU`, `mcg`, dan `Liter/Jam`.
Artinya daftar pilihan satuan volume di layar penerimaan akan menawarkan **galon dan meter
kubik** sebagai satuan sah untuk sebuah tabung darah.

`VAL-57` sengaja **tidak** dipersempit untuk menutupi ini. Aturannya berbunyi *"bukan satuan
laboratorium"*, dan satu-satunya penanda yang menyatakan hal itu adalah `IsForLaboratory`.
Menambahkan penyaring lain — misalnya hanya `MeasurementType = Volume` — berarti mengarang
aturan yang tidak pernah diputuskan siapa pun, dan sekaligus akan menolak `blok` dan `slide`
yang justru diminta `AC-64`.

**Ada juga dua baris gram bersimbol sama** (`GRAM` dan `GR`). Keduanya sah dipilih dan
menghasilkan tampilan yang identik, sehingga dua wadah bervolume sama dapat menunjuk baris yang
berbeda. Ini kebersihan data induk global, milik `master-data`, dan di luar cakupan task ini.

**Statusnya karena itu berubah:** `DATA-MST-MEASUREMENT` **tidak lagi menahan seluruh kolom
volume** — ia menahan tiga satuan, dan bersamanya separuh `AC-64`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet ef migrations add` mencetak `HostAbortedException` pada keluarannya. Ini **perilaku normal** EF Tools ketika membangun `IHost` untuk membaca `DbContext`, bukan kegagalan; migrationnya terbentuk dan perintahnya berakhir `Done.` |
| Masalah yang diketahui | Lihat 8.1 dan 8.2 |
| Risiko tersisa | **Sudah terjadi, bukan lagi kemungkinan.** Migration diterapkan 2026-09-15 atas wewenang pemilik modul yang menerima konsekuensinya, sehingga layar wadah pada `QuilvianNewDevYoga` **kini menjawab `422`** sampai `FE-LAB-11` selesai — lihat 8.1 |
| Perubahan sampingan | `NONE`. Berkas `LabFilterAndSummaryDtos.cs`, `LabFilterMetadataFactory.cs`, `Program.cs`, dan `ApplicationDbContext.cs` memang muncul pada `git status`, tetapi perubahannya milik `BE-LAB-20` yang belum di-commit dan **tidak disentuh task ini** |
| Interupsi | `NONE` |
| Status Git | Lihat 8.3 |
| Langkah berikutnya | Lihat 8.4 |

### 8.1 Layar wadah yang sudah ada kini menolak — konsekuensi kontrak yang sudah diterima

> **Keadaan per 2026-09-15:** migration **sudah diterapkan** ke `QuilvianNewDevYoga` atas
> wewenang pemilik modul, yang diberi tahu lebih dulu bahwa layar wadah akan menjawab `422` dan
> menerimanya. Bagian di bawah karena itu bukan lagi peringatan sebelum penerapan, melainkan
> keterangan tentang keadaan yang sedang berjalan.


`specimenTypeId` **wajib**, sesuai `LAB-API-v1` `r7`: *"Wajib untuk wadah baru; permintaan tanpa
ruas ini ditolak `422`"*. Konsumen yang sudah berjalan hari ini belum mengirimnya.

Diperiksa langsung pada frontend (read-only):
`src/lib/hooks/health-services/laboratory-management/lab-specimen-rules.js:218` dan
`use-lab-specimen-workspace.jsx:67` menyusun muatan `POST` hanya dari `examinations` dan
`specimenDescription`. **Setelah migration diterapkan, tombol simpan wadah pada layar
`lab-orders/{slug}/specimens` akan menjawab `422` "Pilih jenis specimen terlebih dahulu."**

Ini sudah diperkirakan kontrak — `api-contract.md` bagian *Dampak kompatibilitas `r7`* menandai
formulir wadah **"Perlu penyesuaian"** — dan pekerjaannya ada pada `FE-LAB-11`. Tetapi urutan
penerapannya berarti: **jangan menerapkan migration ini ke lingkungan yang layar wadahnya sedang
dipakai sebelum `FE-LAB-11` siap**, atau sediakan jendela pemakaian yang disepakati.

Backend **tidak** dilonggarkan untuk menutupi hal ini. Membuat `specimenTypeId` opsional akan
membatalkan `AC-58` dan mengembalikan jenis specimen menjadi teks bebas — persis yang
`LAB-DEC-040` cabut.

### 8.2 Dua pembacaan kontrak yang dilaporkan, bukan diputuskan diam-diam

**Pertama: hanya `specimenTypeId` yang dibuat wajib, bukan keempat ruasnya.**
`api-contract.md` menulis kelima ruas `r7` sebagai *"Wajib untuk wadah baru"*, dan
`02-backend-architecture.md` bagian 11.4 menyebut `VolumeAmount` *"wajib pada API untuk wadah
baru"*. Tetapi `validation-matrix.md` `r4` **tidak memiliki satu pun aturan** yang menolak volume
yang tidak diisi — `VAL-56` hanya menolak volume **tanpa satuan**, dan `AC-62` berbunyi sama.

Menolak volume kosong berarti mengarang pesan penolakan yang tidak ada pada matriks, sekaligus
membuat **seluruh** pencatatan wadah tertahan sampai Master Data mengisi kelima baris satuan —
padahal roadmap menyatakan yang tertahan hanyalah pembuktian volumenya. Karena itu volume
dibiarkan opsional, dan selisih ini diajukan kepada pemilik modul, bukan diputuskan sendiri.

**Kedua: satuan yang dikirim tanpa volume diterima.** Tidak ada aturan yang melarangnya. Satuan
itu tetap diperiksa `VAL-57`, lalu disimpan apa adanya.

Bila pemilik modul menghendaki keduanya berubah, keperluannya adalah **aturan validasi baru pada
`LAB-VAL-v1`**, bukan penyesuaian kode diam-diam.

### 8.3 Status Git akhir

`git status --short` menunjukkan **41 entri**, dan **10 di antaranya milik task ini** — lima berkas source, dua berkas migration, snapshot, laporan ini, serta pembaruan roadmap dan traceability:

| Milik `BE-LAB-21` | Keadaan |
| --- | --- |
| `Areas/.../Models/LabSpecimen.cs` | `M` |
| `Areas/.../DTOs/LabSpecimenDtos.cs` | `M` |
| `Areas/.../Services/LabSpecimenService.cs` | `M` |
| `Areas/.../Controllers/LabSpecimenController.cs` | `M` |
| `Repositories/.../LabSpecimenConfiguration.cs` | `M` |
| `Migrations/20260915035317_AddLabSpecimenTypeAndVolumeColumns.cs` beserta `.Designer.cs` | `??` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | `M` — sebagian milik `BE-LAB-20`, sebagian tergenerasi task ini |
| Laporan ini beserta pembaruan roadmap dan traceability | `??` dan `M` |

Sisanya adalah pekerjaan `BE-LAB-20`, `BE-LAB-23`, `BE-LAB-24`, dan artefak blueprint `MVP-5a`
yang belum di-commit. **Tidak ada `git add`, `commit`, maupun `push` yang dijalankan.**

### 8.4 Langkah berikutnya

1. **Ajukan TIGA baris `MstMeasurement` ke pemilik `master-data`** — mikroliter (`µL`), blok, dan
   slide. **Bukan lima**: `mL` dan `gram` sudah ada. Dan **jangan mendikte kodenya** — lihat 7.1.
2. ~~Minta wewenang menerapkan migration~~ — ✅ **Selesai 2026-09-15.** Diterapkan ke
   `QuilvianNewDevYoga`; `T-M2`, `T-M4`, dan jalur `Down` lalu `Up` terbukti (5.5, 5.6).
3. **`FE-LAB-11` kini mendesak, bukan lagi sekadar berurutan.** Layar wadah pada
   `QuilvianNewDevYoga` sudah menjawab `422` — lihat 8.1.
4. **Laporkan kelonggaran `IsForLaboratory` ke `master-data`** — galon dan meter kubik saat ini
   sah dipilih sebagai satuan volume specimen, dan ada dua baris gram bersimbol sama (7.2).
5. `BE-LAB-22` ✅ dan `BE-LAB-25` terbuka setelah task ini.
6. **`LAB-OPEN-018b` sudah terjadi.** Akar `rules/` runtime kembali menjadi 14 berkas dari 35 —
   `GLOBAL_RULES.md`, `TEST_POLICY.md`, dan `backend/engineering/` hilang lagi karena marketplace
   `quilvian` masih terdaftar ke `MHamzah1/QuilvianEngineeringSkillsClaude`. Task ini membaca
   aturannya dari clone canonical `DevBenari/QuilvianEngineeringSkills` dan dari
   `docs/engineering/` di repository ini, sesuai keputusan 2026-09-02. Perbaikan tetapnya tetap
   sama: daftarkan ulang marketplacenya.
