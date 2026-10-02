# Laporan Perubahan Backend — `BE-LAB-33`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-33` |
| Judul | Ruas respons konfirmasi pada daftar dan detail pesanan |
| Slice | `EPIC-LAB-12`, gelombang `MVP-5d` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6e |
| Trace | `FR-11.12`; `LAB-DEC-061`; melengkapi `AC-94` dan `AC-95`; celah ditemukan `BE-LAB-31` |
| Contract version | `LAB-API-v1` **`r13`** bagian 8 — `approved` pemilik modul 2026-09-16 |
| Dependency | `BE-LAB-30` ✅, `BE-LAB-31` ✅ — keduanya selesai 2026-09-16 |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 0, berkas diubah 0, logika bisnis 0, kontrak API 1, database 1, keamanan 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — DTO dan service Laboratorium. **Nol migration** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-16 |
| Status | **✅ `SELESAI`** — kelima ruas terbaca dari database pada jalur daftar maupun detail; pesanan yang belum dikonfirmasi terbukti mengembalikan kelimanya `null`. 10 pemeriksaan, 10 `PASS` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `TOUCHED LEGACY` pada DTO dan service yang sudah ada; penambahannya mengikuti aturan `NEW CODE` |
| Status gerbang | `QBE-MOD-002` **tidak menahan** — lifecycle `ACTIVE`, nol entity baru |
| QBE ID yang berlaku | `QBE-API-001` (boundary dan bentuk respons yang sudah mapan), `QBE-DTO-001` (nol entity EF diekspos), `QBE-SVC-001` (orkestrasi tetap di service) |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`..`003`, `QBE-CFG-001`, `QBE-NAM-001`..`004`, `QBE-DB-001`..`002` — nol entity, configuration, dan migration; `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001` — nol validasi, permission, dan perubahan state; `QBE-CODE-001`..`006` — nol nomor bisnis |

**Satu catatan tentang asal task ini, ditulis supaya jejaknya jujur.** `BE-LAB-33` **tidak** lahir
dari putaran `plan-module-delivery`. Ia diturunkan langsung dari amandemen `LAB-API-v1` `r13` yang
disetujui pemilik modul pada 2026-09-16, dan ditulis ke roadmap pada sesi yang sama sebagai bagian
dari pencatatan keputusan itu. Nol keputusan baru dikarang: seluruh isinya — ruas, tipe, tempat,
dan alasan penempatannya — sudah tertulis pada kontrak bagian 8 sebelum satu baris kode ditulis.

---

## 1. Masalah yang diperbaiki

**Keadaan sebelum perubahan.** `BE-LAB-30` mendirikan tiga kolom konfirmasi dan `BE-LAB-31`
mengisinya. Keduanya selesai dan terbukti — **tetapi nilainya tidak dapat dibaca siapa pun lewat
API.** Nilai itu tersimpan dengan benar di database dan berhenti di sana.

**Akibat nyatanya, dan ia menahan pekerjaan orang lain.**

| Layar | Yang diwajibkan roadmap | Yang menghalanginya |
| --- | --- | --- |
| `FE-LAB-15` | Kolom Konfirmasi memuat **nama konfirmator beserta tanggal dan waktu** | Ketiga nilai tidak dikembalikan endpoint mana pun |
| `FE-LAB-17` | Tanda tangan **konfirmator** dan **dokter pemeriksa** pada ringkasan cetak | Sebab yang sama |

`AC-94` dan `AC-95` karena itu berhenti pada "terpenuhi sebagian" — dan penyebabnya bukan pekerjaan
yang kurang, melainkan bentuk kontraknya.

**Kenapa celah ini tidak terlihat lebih awal, dan itu pantas dicatat.** Ia tidak ditemukan oleh
build, oleh uji, maupun oleh tinjauan kontrak — ketiganya hijau ketika celahnya masih ada.
Ia ditemukan dengan membaca **apa yang dibutuhkan layar konsumennya**, yaitu `FE-LAB-15`, saat
`BE-LAB-31` hendak ditandai selesai.

---

## 2. Proses bisnis

### 2.1 Yang berubah bagi petugas

Tidak ada satu pun tindakan baru. Yang berubah adalah **apa yang dapat dilihat**:

1. Petugas membuka daftar pesanan pada salah satu menu Pemeriksaan.
2. Pesanan yang sudah dikonfirmasi kini membawa **nama konfirmator**, **waktu konfirmasi**, dan
   **nama dokter pemeriksa** — siap ditampilkan apa adanya, tanpa layar perlu menerjemahkan apa
   pun.
3. Pesanan yang belum dikonfirmasi membawa kelimanya kosong, dan itu keadaan sah — bukan galat.

### 2.2 Kenapa nama ada di daftar dan penunjuk ada di detail

**Nama ada di daftar karena layar tidak boleh menampilkan penunjuk.** Kolom Konfirmasi menampilkan
nama orang. Aturan `no-uuid-display` sudah berlaku untuk `RequestedByName` sejak `r3`. Daftar yang
hanya membawa penunjuk memaksa layar memanggil endpoint kedua **per baris** hanya untuk
menerjemahkannya — 25 pesanan menjadi 51 permintaan.

**Penunjuk ada di detail karena aksi membutuhkannya.** Layar yang kelak mengubah dokter pemeriksa
perlu nilai yang dapat dikirim balik, dan nama bukan nilai yang dapat dikirim balik.

Pembagian ini bukan pola baru: `RequestedByUserId` dan `RequestedByName` sudah memakainya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `contracts/api-contract.md` bagian 8 | Nama, tipe, tempat, dan sumber nilai kelima ruas |
| `roadmap/backend-roadmap.md` bagian 6e | Cakupan, DoD, dan verifikasi task |
| `Areas/.../DTOs/LabOrderDtos.cs` | Bentuk `LabOrderListResponse` dan `LabOrderDetailResponse` |
| `Areas/.../Services/LabOrderService.cs` | Ketiga jalur yang membangun respons: `ProyeksikanDaftarAsync`, `GetDetailAsync`, `MapDetailResponse` |
| `task/report/backend/BE-LAB-31.md` bagian 7.2 | Usul kelima ruas beserta alasannya |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | Tiga ruas pada `LabOrderListResponse`; dua ruas pada `LabOrderDetailResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Pengisian kelimanya pada ketiga jalur pembangun respons; `ProyeksikanDaftarAsync` menjadi method instance |

Nol migration. Nol entity. Nol endpoint baru. Nol permission baru.

### 3.3 Tiga jalur pembangun respons, ketiganya disentuh — dan kenapa berbeda isinya

| Jalur | Dipakai oleh | Yang diisi |
| --- | --- | --- |
| `ProyeksikanDaftarAsync` | `GET /lab-orders` dan `GET /lab-orders/by-episode/{id}` | Ketiga ruas daftar; kedua nama diterjemahkan **di dalam proyeksi yang sama** |
| `GetDetailAsync` | `GET /lab-orders/{id}`, dan **setiap** jalur yang mengembalikan detail sesudah aksi — termasuk konfirmasi dan pembatalan | Kelimanya |
| `MapDetailResponse` | **Hanya** `CreateAsync` dan `CreateByExaminationsAsync` | Ketiga ruas dari entity; **kedua namanya sengaja dibiarkan kosong** |

**Kenapa `MapDetailResponse` tidak menerjemahkan namanya, dan itu benar.** Mapper itu dipakai
**tepat sesudah pesanan dibuat**, dan pesanan yang baru lahir belum mungkin dikonfirmasi — ketiga
kolomnya pasti kosong. Menerjemahkan dua penunjuk yang pasti kosong berarti dua perjalanan ke
database untuk menghasilkan `null`. Jalur yang mengembalikan pesanan **terkonfirmasi** adalah
`GetDetailAsync`, dan di sana kedua namanya memang diisi. Alasannya ditulis sebagai komentar pada
source supaya pembaca berikutnya tidak menyangka itu kelalaian.

### 3.4 Satu method berhenti menjadi `static`, dan itu perubahan yang perlu dilaporkan

`ProyeksikanDaftarAsync` sebelumnya `private static`. Ia kini membutuhkan `DbContext` untuk
menerjemahkan kedua nama di dalam proyeksi yang sama, sehingga `static`-nya dilepas.

**Dua pilihan dipertimbangkan, dan yang lebih murah dipilih:** meneruskan `DbContext` sebagai
parameter, atau menjadikannya method instance. Yang kedua dipilih — ia **privat**, sehingga nol
pemanggil di luar kelas ini terpengaruh, dan kelasnya sendiri sudah memegang context yang sama.

**Kenapa terjemahannya tidak dikerjakan sesudah baris dimuat.** Menerjemahkan per baris sesudah
proyeksi berarti daftar 25 pesanan menjadi **51 perjalanan** ke database. Cara yang dipakai adalah
cara yang sudah dipakai `RequestedByName` pada `GetDetailAsync`: sub-query di dalam proyeksi yang
sama, satu perjalanan.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Lima ruas bertambah**, persis seperti `r13` bagian 8. **Seluruhnya aditif** — nol endpoint, ruas, nilai enum, atau pembungkus yang berubah, berganti nama, atau hilang. Pemanggil yang mengabaikan ruas baru tidak terpengaruh |
| Database | **Nol perubahan schema, nol migration.** Ketiga kolomnya sudah berdiri sejak `BE-LAB-30`. Yang bertambah hanya **pembacaan**: dua sub-query terjemahan nama per jalur respons |
| Keamanan/Auth | `NOT APPLICABLE`. Nol permission baru dan nol aturan otorisasi yang berubah. Nama pengguna yang ditampilkan berasal dari sumber yang sama dengan `RequestedByName`, yang sudah tampil pada layar yang sama |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Tiga endpoint yang sudah ada bertambah ruas responsnya:

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Ruas yang bertambah | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders` | `confirmedAt`, `confirmedByName`, `examinerDoctorName` | `LabOrder : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/by-episode/{episodeId}` | Ketiganya sama | `LabOrder : Read` |
| `GET` | `/api/v1/health-services/laboratory-management/lab-orders/{id}` | Kelimanya | `LabOrder : Read` |

**Ikut terbawa tanpa perubahan kode tambahan:** setiap jalur yang mengembalikan
`LabOrderDetailResponse` lewat `GetDetailAsync` — termasuk `POST /lab-orders/{id}/confirm` dan
`PUT /lab-orders/{id}/cancel` — kini membawa kelimanya juga. Itu memang yang diinginkan: layar
konfirmasi langsung memperoleh nama konfirmator dari jawaban aksinya sendiri, tanpa perlu memuat
ulang daftar.

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Sama persis dengan baseline. Satu warning sempat muncul dan **diperbaiki** — lihat 5.2 |
| Verifikasi kontrak terhadap `r13` bagian 8 | Kelima ruas, tipe, dan tempatnya cocok | `PASS` | Bagian 4 |
| **Detail** — ketiga ruas waktu dan penunjuk terisi | `ConfirmedAt`, `ConfirmedByUserId`, `ExaminerDoctorId` terbaca benar | `PASS` | 5.1 |
| **Detail** — nama konfirmator diterjemahkan | `"Dewi"`, bukan penunjuknya | `PASS` | 5.1 |
| **Detail** — nama dokter pemeriksa diterjemahkan | `"dr. Fajar Rama Ragussa"` dari `MstDoctor.FullName` | `PASS` | 5.1 |
| **Detail** — nama konfirmator memakai jalur **sama** dengan `RequestedByName` | Keduanya menghasilkan nilai identik untuk pengguna yang sama | `PASS` | 5.1 |
| **Detail** — pesanan belum dikonfirmasi mengembalikan **kelimanya `null`** | Kelimanya `null`, tanpa galat | `PASS` | 5.1 |
| **Daftar** — ketiga ruas siap tampil terisi | Terbaca pada jalur berpagination | `PASS` | 5.1 |
| **Daftar** — pesanan belum dikonfirmasi mengembalikan ketiganya `null` | Ketiganya `null` | `PASS` | 5.1 |
| **Daftar** — baris lain tidak ikut terisi | Tepat 1 dari 2 baris membawa nama konfirmator | `PASS` | 5.1 |
| **Regresi** — ruas yang sudah ada tetap terisi apa adanya | `OrderStatus`, `Discipline`, `ProcedureName`, `Version`, `RequestedByUserId` seluruhnya benar | `PASS` | 5.1 |
| Kebersihan database sesudah uji | Nol baris uji tersisa; 5 pesanan dengan sebaran status identik | `PASS` | 5.3 |
| Uji lewat HTTP sungguhan beserta `[Authorize]` dan `[AccessPermission]` | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan. Hak akses tidak berubah, dan ketiga endpoint tetap memakai `LabOrder : Read` yang sudah ada |

### 5.1 Sepuluh pemeriksaan dijalankan sungguhan terhadap database

Harness sekali pakai di scratchpad sesi, **di luar repository**, memakai `ApplicationDbContext` dan
`LabOrderService` yang **sebenarnya**. Dua pesanan uji dibuat sendiri — **nol dari 5 pesanan nyata
disentuh**.

Hasil: **10 `PASS`, 0 `FAIL`.**

**Aktornya sengaja pengguna nyata, bukan GUID karangan.** Nama konfirmator diterjemahkan dari tabel
`Users`; aktor karangan hanya akan menghasilkan `null`, dan `null` tidak membuktikan bahwa
terjemahannya bekerja. Yang dipakai adalah pengguna `"Dewi"` yang benar-benar ada, dan nilainya
dibandingkan terhadap nama yang dibaca langsung dari tabel itu.

**Satu pemeriksaan sengaja menguji ketiadaan.** Baris lain pada daftar yang sama dipastikan
**tidak** ikut terisi nama konfirmator — tepat 1 dari 2 baris yang membawanya. Tanpa ini, sub-query
terjemahan yang keliru mengikat dapat mengisi setiap baris dengan nama yang sama dan tetap terlihat
benar pada baris pertama.

### 5.2 Satu warning muncul dan diperbaiki, bukan dibiarkan

Build sempat menghasilkan **208** warning, satu di atas baseline:

```text
warning CS1574: XML comment has cref attribute 'RequestedByName' that could not be resolved
```

Sebabnya nyata dan bukan sekadar kerapian: komentar pada `LabOrderListResponse` merujuk
`RequestedByName` sebagai `cref`, padahal ruas itu tinggal di `LabOrderDetailResponse` — kelas
**turunan**, bukan kelas dasar. Rujukannya diganti menjadi `<c>` beserta keterangan kenapa, dan
build kembali ke **207**.

### 5.3 Kebersihan database

| Butir | Sebelum | Sesudah |
| --- | :---: | :---: |
| Total `LabOrder` | 5 | **5** |
| Sebaran status | `Requested` 2, `Accepted` 2, `InProcess` 1 | **identik** |
| Pesanan berstatus `Confirmed` | 0 | **0** |
| Kolom `ConfirmedAt` / `ExaminerDoctorId` terisi | 0 | **0** |
| Baris `LabTransitionHistory` milik aktor uji | 0 | **0** |

Dua pesanan uji dan satu baris riwayat dibuat lalu dihapus. Pemeriksaan penutup dijalankan
**terpisah dari harness**, memakai query langsung terhadap database.

### 5.4 Alat verifikasinya

Satu program sekali pakai di scratchpad sesi, di luar repository, yang merujuk project aplikasi.
Ia **menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`** dan tidak menulis
credential ke berkas mana pun.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kelima ruas ada pada respons sesuai `r13` | **Terpenuhi** | Bagian 4; kelimanya terbaca dari database — 5.1 |
| Nama siap tampil, bukan penunjuk | **Terpenuhi** | `"Dewi"` dan `"dr. Fajar Rama Ragussa"` terbaca sebagai nama — 5.1 |
| Pesanan lama mengembalikan `null` tanpa galat | **Terpenuhi** | Kelimanya `null` pada detail, ketiganya `null` pada daftar — 5.1 |
| Nol endpoint yang sudah ada berubah perilakunya | **Terpenuhi** | Pemeriksaan regresi: seluruh ruas lama tetap terisi apa adanya — 5.1 |
| Build | **Terpenuhi** | 0 Error, 207 Warning — sama persis dengan baseline |

Seluruh butir DoD terpenuhi.

### 6.2 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-94` — bagian **"terbaca pada daftar"** | **Sisi backend selesai** | `confirmedByName` dan `confirmedAt` terbaca pada jalur daftar. Penampilannya di layar adalah `FE-LAB-15` |
| `AC-95` — bagian **"tampil pada daftar serta ringkasan cetak"** | **Sisi backend selesai** | `examinerDoctorName` terbaca pada jalur daftar. Penampilannya adalah `FE-LAB-15`; ringkasan cetaknya `FE-LAB-17`, yang **masih tertahan penahan lain** |

Keduanya **belum dapat ditandai terpenuhi penuh**, dan itu bukan kekurangan task ini: keduanya
menagih sesuatu yang **tampil di layar**, dan layarnya belum dibangun. Yang berubah adalah
penyebabnya — dari **jalan keluarnya tidak ada** menjadi **layarnya belum dibuat**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 207 warning, seluruhnya sudah ada sebelum task ini. Satu warning baru sempat muncul dan **diperbaiki**, bukan dibiarkan — 5.2 |
| Masalah yang diketahui | **Satu, diwariskan dan belum jatuh tempo:** `LabOrderSummaryResponse` masih mencacah ke delapan ember status yang tetap, sehingga pesanan berstatus `Confirmed` tidak masuk ember mana pun walaupun ikut pada `TotalPesanan`. `r13` **sengaja tidak menyentuhnya** — memperbaikinya berarti menambah ruas pada rekap, dan itu keputusan tersendiri yang tidak dibutuhkan `FE-LAB-15` maupun `FE-LAB-17`. Tercatat sejak [`BE-LAB-31.md`](BE-LAB-31.md) bagian 7.1 |
| Risiko tersisa | **Pertama**, dua sub-query terjemahan nama bertambah pada setiap pembacaan daftar. Keduanya berjalan di dalam proyeksi yang sama — bukan per baris — sehingga tidak menambah perjalanan ke database, tetapi keduanya **belum diukur pada daftar besar**; database uji hanya berisi 5 pesanan. **Kedua**, `ProyeksikanDaftarAsync` tidak lagi `static`; ia privat sehingga nol pemanggil luar terpengaruh, tetapi perubahan itu perlu diketahui siapa pun yang membacanya sebagai fungsi murni. **Ketiga**, ketiga endpoint belum pernah dilewati lapisan `[Authorize]` dan `[AccessPermission]` secara sungguhan; risikonya rendah karena hak aksesnya tidak berubah |
| Perubahan sampingan | `NONE`. Perubahan yang sudah ada di working tree sebelum task ini tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Lihat 7.1 |
| Langkah berikutnya | **1.** `FE-LAB-15` kini **terbuka sepenuhnya** — ketiga nilai yang dibutuhkan kolom Konfirmasi sudah dapat dibaca. **2.** `FE-LAB-16` juga terbuka, label tombolnya sudah ditetapkan. **3.** `FE-LAB-17` **tetap tertahan** — `r13` hanya menutup penahan tanda tangannya; tombol Print yang tidak pernah ada dan ketiga keputusan pemilik tentang isi ringkasan cetak belum tersentuh. **4.** Putuskan bagaimana `Confirmed` dicacah pada rekap pesanan |

### 7.1 Status Git di akhir pekerjaan

Perubahan milik task ini:

```text
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-33.md
```

Ditambah pembaruan artefak blueprint: `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md`,
dan `roadmap/traceability.md`.

Kedua berkas source **sudah** berstatus `M` sebelum task ini dimulai, karena pekerjaan
`BE-LAB-26`..`BE-LAB-32` belum di-commit. Bagian milik task ini terpisah jelas.

Nol `git add`, `commit`, `push`, `pull`, `merge`, `rebase`, dan `deploy` dijalankan.
