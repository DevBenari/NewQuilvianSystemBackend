# Laporan Perubahan Backend — `BE-LAB-20`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-20` |
| Judul | Data induk jenis specimen |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.1`; `LAB-DEC-040`, BR-35; `AC-58`; `VAL-61`, `VAL-62`, `VAL-63` |
| Contract version | `LAB-API-v1` `r7` grup Lab Specimen Type; `LAB-VAL-v1` `r4`; `LAB-PERM-v1` rev 4 — seluruhnya `approved`, disetujui pemilik modul 2026-09-14 |
| Dependency | — (tanpa prasyarat, gelombang eksekusi 1) |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium, configuration, migration, artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-14 |
| Status | **✅ `SELESAI`** — 2026-09-14. Source, migration, **dan eksekusi ke database** selesai dan terverifikasi. Jalur `Down` ikut dibuktikan |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 lewat `LAB-REQ-002`. Baris riwayat tanggal yang sama menetapkan data induk milik Laboratorium memakai prefix `Lab`, bukan `Mst` |
| Keberlakuan | `NEW CODE` seluruhnya |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle sudah `ACTIVE` |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-MOD-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-ENT-002`, `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-AUD-001` |

**Kenapa `LabSpecimenType`, bukan `MstLabSpecimenType`.** Baris riwayat
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tertanggal 2026-09-02 berbunyi: *"menetapkan prefix data
induk milik Laboratorium: entity baru memakai `Lab`, sehingga dua tabel batas nilai bernama
`LabValueBound` dan `LabValueOption`; `MstLabRejectionReason` yang sudah ada diperlakukan legacy
dan tidak dinamai ulang."* Memakai `Mst` pada entity baru justru melanggar keputusan itu.

---

## 1. Masalah yang diperbaiki

Jenis bahan yang dibawa sebuah wadah sampel selama ini hanya tersimpan pada
`LabSpecimen.SpecimenDescription` — **teks bebas**. Sebagai teks bebas, "cairan kista",
"Cairan Kista", dan "c. kista" terhitung tiga jenis berbeda, dan laporan jenis sampel tidak
pernah dapat dipercaya.

Tetapi menutup daftarnya rapat-rapat punya harga sendiri, dan harganya tidak ditanggung
sistem. Sampel cairan kista yang datang pukul 21.00 tidak akan dapat diterima sampai kepala
instalasi menambahkan jenisnya keesokan hari — dan sampelnya tidak menunggu, ia rusak.

`LAB-DEC-040` menyelesaikan ketegangan itu dengan baris **`Lainnya`** yang wajib berketerangan:
sampel aneh tetap diterima, keterangannya tercatat, dan kepala instalasi dapat menaikkannya
menjadi jenis tetap setelah melihat ia sering muncul.

---

## 2. Proses bisnis

**Jalur petugas penerimaan.** Saat mencatat wadah, petugas memilih jenis dari daftar yang
dikembalikan `GET /options`. Daftar itu **hanya** memuat jenis aktif, sehingga jenis yang sudah
ditarik dari peredaran tidak pernah dapat dipilih. Bila jenisnya belum ada, ia memilih
`Lainnya` dan menuliskan keterangannya — penerimaan tetap berhasil.

**Jalur kepala instalasi.** Dari layar pengelolaan ia melihat seluruh jenis lewat `GET /`,
termasuk yang nonaktif — tanpa itu jenis yang dinonaktifkan tidak akan pernah dapat diaktifkan
kembali. Ia menambah jenis baru, mengubah nama, keterangan, dan urutan tampil, serta
mengaktifkan atau menonaktifkan.

**Tiga aturan menjaga baris `Lainnya`:**

| Aturan | Isi | Kenapa |
| --- | --- | --- |
| `VAL-61` | Kode jenis unik di antara baris yang belum terhapus | Dua kode sama membuat rujukan menjadi ambigu |
| `VAL-62` | Hanya satu baris `Lainnya` yang boleh aktif | Dua jalan keluar membuat keterangannya terpecah dua |
| `VAL-63` | Baris `Lainnya` terakhir tidak boleh dinonaktifkan | Tanpa itu jalan buntu di meja penerimaan kembali, dan kembalinya diam-diam |

**Tidak ada penghapusan.** Jenis yang pernah menempel pada wadah adalah bagian riwayat
penerimaan; ia dinonaktifkan, bukan dihapus.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabSpecimenType.cs` | Entity beserta tujuh kolom bisnis |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabSpecimenTypeConfiguration.cs` | Tabel, panjang kolom, dan tiga index |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenTypeDtos.cs` | Tujuh DTO: dua query, dua response, tiga request |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenTypeService.cs` | Tujuh method publik beserta penegakan `VAL-61` .. `VAL-63` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenTypeController.cs` | Delapan endpoint |
| `Areas/HealthServices/LaboratoryManagement/Seeders/LabSpecimenTypeSeeder.cs` | Pengisian tujuh baris baseline saat aplikasi menyala |
| `Migrations/20260914084655_AddLabSpecimenType.cs` | Tabel beserta ketiga index |
| `Migrations/20260914084730_SeedLabSpecimenType.cs` | Tujuh baris baseline, `ON CONFLICT DO NOTHING` |

### 3.2 Berkas yang disentuh

| Berkas | Perubahan |
| --- | --- |
| `Repositories/ApplicationDbContext.cs` | Satu `DbSet<LabSpecimenType>` |
| `Program.cs` | Satu registrasi DI dan satu pemanggilan seeder |
| `Areas/HealthServices/LaboratoryManagement/Services/LabFilterMetadataFactory.cs` | Satu method `LabSpecimenType()` |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabFilterAndSummaryDtos.cs` | Dua DTO: filter metadata dan summary |

**Tidak ada satu pun berkas lama yang berubah perilakunya.** Seluruh perubahan bersifat
tambahan.

### 3.3 Dua penjagaan yang ditaruh di basis data, bukan hanya di service

| Index | Isi |
| --- | --- |
| `IX_LabSpecimenType_SpecimenTypeCode` | `UNIQUE ... WHERE "IsDelete" = false` — penjaga terakhir `VAL-61` |
| `IX_LabSpecimenType_SingleOtherBucket` | `UNIQUE ("IsOtherBucket") WHERE "IsOtherBucket" = true AND "IsActive" = true AND "IsDelete" = false` — penjaga terakhir `VAL-62` |

Aturan yang hanya dijaga service akan bocor lewat seeder, skrip perbaikan data, atau migration
berikutnya — dan bocornya diam-diam.

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-specimen-types`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan urutan, ukuran halaman, dan penanda tidak-dapat-dihapus | `LabSpecimenType : Read` |
| `GET` | `/summary` | Rekap total, aktif, nonaktif, dan jumlah jalan keluar `Lainnya` yang aktif | `LabSpecimenType : Read` |
| `GET` | `/` | Daftar pengelolaan; memuat yang nonaktif | `LabSpecimenType : Read` |
| `GET` | `/options` | Daftar pilihan layar penerimaan; **hanya yang aktif** | `LabSpecimenType : Read` |
| `GET` | `/{id:guid}` | Detail satu jenis | `LabSpecimenType : Read` |
| `POST` | `/` | Menambah jenis | `LabSpecimenType : Create` |
| `PUT` | `/{id:guid}` | Mengubah nama, keterangan, urutan | `LabSpecimenType : Update` |
| `PUT` | `/{id:guid}/activation` | Mengaktifkan atau menonaktifkan | `LabSpecimenType : Update` |

**Angka `JalanKeluarLainnyaAktif` pada `/summary` sengaja ada.** Nilai sehatnya selalu `1`.
Nilai `0` berarti jalan keluar bagi jenis yang belum terdaftar sedang tertutup, dan penerimaan
sampel aneh akan tertahan — keadaan berbahaya yang lebih baik terlihat sebagai angka daripada
ditemukan dari keluhan petugas.

---

## 5. Delta kontrak yang dilaporkan, bukan didiamkan

`rules/backend/master-data-endpoint-standard.md` mewajibkan **sembilan endpoint baseline**.
Yang dibangun delapan. Selisihnya:

| Baseline | Keadaan | Alasan |
| --- | --- | --- |
| `GET /filters/metadata` | **Dibangun** | Tidak ada di `r7`; ditambahkan sebagai permukaan teknis baseline, mengikuti `LabRejectionReason` |
| `GET /summary` | **Dibangun** | Sama seperti di atas |
| `PATCH /{id}/status` | **Diganti** `PUT /{id}/activation` | Mengikuti source modul ini (`LabRejectionReason`, `LabValueBound`). Standar itu sendiri menyatakan source yang berlaku bila berbeda |
| `DELETE /{id}` | **Tidak dibangun** | `LAB-DEC-040` menetapkan jenis dinonaktifkan, bukan dihapus. Ini **kebijakan**, bukan kekurangan teknis — dan kebijakan tidak boleh diubah sepihak dari sini |

Satu endpoint `r7` juga **tidak** dibangun di sini:

| Endpoint `r7` | Keadaan |
| --- | --- |
| `GET /other-usage` | **Ditunda ke `BE-LAB-25`.** Ia membaca `LabSpecimen.SpecimenTypeOtherNote` yang kolomnya baru dibuat `BE-LAB-21`; membangunnya sekarang berarti membaca kolom yang belum ada |

**Akibatnya kontrak perlu naik ke `r8`** untuk mencatat `GET /filters/metadata` dan
`GET /summary`. Perubahannya aditif dan tidak menyentuh satu pun endpoint `r3`..`r7`.

> **Ditutup pada sesi yang sama, 2026-09-14.** `LAB-API-v1` dinaikkan ke **revision 8**,
> disetujui pemilik modul. Kedua endpoint itu kini tercatat pada `contracts/api-contract.md`
> beserta alasannya, dan `blueprint-manifest.md` ikut disinkronkan. Delapan baris grup
> `Lab Specimen Type` juga diubah dari `Rencana (belum tersedia)` menjadi **Tersedia**, karena
> endpointnya memang sudah ada di kode sejak task ini.

---

## 6. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Jenis | Hasil |
| --- | --- |
| Backend Governance Preflight | **Lulus.** Registry `ACTIVE`, prefix `Lab`, `QBE-MOD-002` tidak menahan |
| Review diff/scope | **Lulus.** 8 berkas baru, 4 berkas disentuh secara aditif, nol berkas lama berubah perilaku |
| `dotnet build -p:RunAnalyzers=False` | **Lulus — 0 Error, 0 Warning** pada build bersih terakhir. Tidak satu pun warning berasal dari berkas baru task ini |
| Verifikasi kontrak API | **Lulus.** Delapan endpoint sesuai `r7` beserta dua tambahan baseline; seluruh response dibungkus `ApiResponse<T>`, daftar memakai `PagedResult<T>` |
| Verifikasi hak akses | **Lulus.** Argumen ke-1 `[AccessPermission]` = `LabSpecimenType` sama persis dengan `ControllerName`; argumen ke-2 sama persis dengan argumen ke-1 `[AccessAction]` pada method yang sama |
| Verifikasi bentuk SQL migration | **Lulus.** `dotnet ef migrations script` menghasilkan tabel, ketiga index — termasuk kedua unique parsial — dan tujuh `INSERT ... ON CONFLICT DO NOTHING` |
| Verifikasi proses bisnis terhadap database | **BELUM DIJALANKAN.** Eksekusi migration belum diberi wewenang |

### 6.1 Eksekusi database — dijalankan 2026-09-14

Wewenang diberikan pemilik modul dengan menyebut targetnya secara tegas:
`QuilvianNewDevYoga` di `160.22.250.77`. Konfirmasi yang menyebut host diminta lebih dulu karena
databasenya **bukan lokal** dan dipakai bersama; aturan `rules/backend/` melarang penerapan
migration ke database non-lokal tanpa itu.

**Yang diterapkan.** `dotnet ef migrations list` lebih dulu memastikan **hanya dua migration
task ini** yang berstatus `Pending` — tidak ada migration modul lain yang ikut terbawa.

| Langkah | Hasil |
|---|---|
| `dotnet ef database update` | `Done.` Percobaan pertama timeout saat membuka koneksi; berhasil pada pengulangan. Host memang lambat — `Test-NetConnection` port 5432 menjawab `True` |
| `migrations list` sesudahnya | Kedua migration tidak lagi `Pending` |

**Bukti terhadap database, dibaca langsung lewat Npgsql:**

| Butir | Hasil |
|---|---|
| Tujuh baris baseline | **7 baris**, urutan 1..6 dan `OTHER` pada 99, seluruhnya aktif |
| `IX_LabSpecimenType_SpecimenTypeCode` | `UNIQUE`, filter `("IsDelete" = false)` |
| `IX_LabSpecimenType_SingleOtherBucket` | `UNIQUE`, filter `(("IsOtherBucket" = true) AND ("IsActive" = true) AND ("IsDelete" = false))` |
| **`T-M3`** | **LULUS** — menyisipkan baris `Lainnya` aktif kedua ditolak `23505` pada `IX_LabSpecimenType_SingleOtherBucket` |
| **`VAL-61`** | **LULUS** — menyisipkan kode `BLOOD` kedua ditolak `23505` pada `IX_LabSpecimenType_SpecimenTypeCode` |
| Kebersihan | **Nol baris uji tertinggal.** Kedua percobaan sisip memang dirancang gagal, sehingga tidak ada yang perlu dihapus |

**Jalur `Down` dibuktikan:**

| Langkah | Hasil |
|---|---|
| `database update 20260913070556_AddBbkBloodUnitAllocation` | `Done.` Kedua migration kembali `Pending` |
| `database update` lagi | `Done.` |
| Verifikasi ulang sesudahnya | **7 baris, kedua index, `T-M3` dan `VAL-61` tetap LULUS, tetap bersih** |

Siklus turun-naik karena itu terbukti utuh, bukan sekadar tidak melempar galat.

### 6.2 Alat verifikasinya, dan kenapa ia di luar repository

Backend tidak punya klien SQL maupun `dotnet-script`. Verifikasi di atas dijalankan satu program
sekali pakai yang dibangun di **scratchpad sesi, di luar repository backend**, membaca connection
string langsung dari `appsettings.Development.json` sehingga kata sandinya tidak pernah melewati
baris perintah.

Penempatannya disengaja. `rules/backend/TEST_POLICY.md` bagian 3 melarang membuat kelas,
endpoint, atau runner "uji sementara" **di dalam project aplikasi**. Alat ini tidak menyentuh
repository sama sekali, dan tidak tertinggal di sana.

### 6.3 Dua butir yang tetap belum dibuktikan runtime

| Butir | Kenapa |
|---|---|
| `T-M4` | `DeleteBehavior.Restrict` dari `LabSpecimen` **belum dapat diuji** — foreign key-nya baru dibuat `BE-LAB-21` |
| `VAL-63` | Aturan tingkat service, bukan basis data. Terverifikasi lewat pembacaan source `SetActivationAsync`; pembuktian runtime menunggu aplikasi dijalankan dengan kredensial pengguna |

Keduanya dicatat apa adanya, bukan diklaim lulus.

## 7. Acceptance criteria dan Definition of Done

| Butir DoD | Status |
| --- | --- |
| Tabel berdiri beserta unique parsial `Lainnya` | **✅ Terbukti di database** — kedua index terbaca dari `pg_indexes` beserta filter parsialnya |
| Tujuh baris terisi | **✅ Terbukti** — 7 baris terbaca, `OTHER` pada urutan 99 |
| Ketujuh endpoint menjawab sesuai kontrak | **✅ Delapan endpoint dibangun** — `/other-usage` ditunda ke `BE-LAB-25`; dua baseline ditambahkan dan dicatat lewat `r8` |
| `VAL-61` .. `VAL-63` menolak sesuai matriks | **✅ `VAL-61` dan `VAL-62` terbukti di database** (`23505`). `VAL-63` ditegakkan di source; pembuktian runtime-nya lihat 6.3 |
| Migration jalan maju dan mundur | **✅ Terbukti** — `Up`, `Down`, lalu `Up` lagi; verifikasi ulang sesudahnya tetap lulus |
| Tidak ada endpoint lama yang berubah perilakunya | **Terbukti** lewat review diff |

| AC | Status |
| --- | --- |
| `AC-58` — jenis dipilih dari daftar terkendali | **Sebagian.** Sisi data induk dan daftar pilihannya berdiri; penolakan teks bebas saat mencatat wadah adalah cakupan `BE-LAB-21` |
| `AC-60` — daftar pantau `Lainnya` | **Belum.** Cakupan `BE-LAB-25` |

---

## 8. Catatan penutup

**Satu keputusan kecil yang perlu diketahui pembaca berikutnya.** Seeder memeriksa lebih dulu
apakah sudah ada baris `Lainnya` aktif sebelum menambahkan baris baseline `OTHER`. Tanpa
pemeriksaan itu, lingkungan yang kepala instalasinya sudah membuat jalan keluar sendiri dengan
kode berbeda akan ditolak index unik parsial — dan penolakan itu terjadi saat aplikasi menyala,
sehingga **seluruh backend gagal start**. Kini seeder melewatinya dengan tenang dan menulis satu
baris peringatan.

**Yang perlu dikerjakan berikutnya:**

1. **Minta wewenang eksekusi migration** ke database pemilik, lalu jalankan `AddLabSpecimenType`
   dan `SeedLabSpecimenType` beserta pembuktian jalur `Down`.
2. **Naikkan `LAB-API-v1` ke `r8`** untuk mencatat `GET /filters/metadata` dan `GET /summary`.
3. `BE-LAB-21` dan `BE-LAB-25` yang menunggu task ini.
