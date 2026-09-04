# Laporan Perubahan Backend — `BE-LAB-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-18` |
| Judul | Penyaring, pengurutan, dan pagination pada daftar pesanan |
| Slice | `S3` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 3, gelombang `MVP-0` |
| Trace | Instruksi pemilik modul 2026-09-03; `IGD-DEC-105` sebagai temuan yang ditutup; `rules/backend/master-data-endpoint-standard.md` bagian 2.3 |
| Contract version | `LAB-API-v1` **`r5`** — amandemen **breaking** atas `r3`/`r4` |
| Dependency | `BE-LAB-17` — **`SELESAI`** |
| Klasifikasi | `HEAVY` — skor 9. Repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 2, database 1, keamanan/auth 1, UI/workflow 2 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, kontrak dan artefak blueprint. **Frontend strict read-only** |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `17a331b`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — sepuluh parameter diterima dan diproses, kontrak naik ke `r5`, dampak konsumen dinilai |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabOrderController` dan `LabOrderService` sudah ada sebelum blueprint ini. `LabOrderPagedQuery` adalah `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-PAGE-001`, `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | Seluruh `QBE-ENT-*`, `QBE-CFG-*`, `QBE-DB-*`, `QBE-NAM-003`, `QBE-CODE-*` — tidak ada entity, configuration, migration, maupun alokasi nomor bisnis |
| Catatan `QBE-PAGE-001` | Aturan ini menyatakan capability list memakai paging, search, dan sort yang sudah mapan. Sebelum task ini, `GET /lab-orders` **melanggarnya**; task inilah yang menutupnya |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

`GET /lab-orders` tidak menerima satu pun parameter. Ia mengembalikan **seluruh isi tabel
pesanan laboratorium rumah sakit** dalam satu jawaban, terbaru lebih dulu.

Akibatnya bukan sekadar lambat. Modul IGD membutuhkan pesanan laboratorium milik **satu**
pasien yang sedang ditangani, dan karena backend tidak menyediakan penyaringnya, ia terpaksa:

1. menarik seluruh pesanan rumah sakit ke browser petugas;
2. menyaringnya sendiri di sana dengan mencocokkan `encounterId`;
3. membuang sisanya.

Tim IGD mencatat keterbatasan itu apa adanya di dalam source mereka — sebuah komentar yang
jujur dan tidak menutupi apa pun:

> *"Penyaringannya dikerjakan di sini, bukan oleh backend. `GET /laboratory-management/lab-orders`
> tidak menerima satu pun parameter — tanpa `encounterId`, tanpa paging — dan mengembalikan
> seluruh isi tabel. Ini disalin apa adanya sebagai keterbatasan yang dicatat, bukan ditutupi:
> begitu tabelnya membesar, layar ini akan menarik seluruhnya untuk menampilkan beberapa baris.
> Perbaikannya milik pemilik `LaboratoryManagement`, bukan IGD (`IGD-DEC-105`)."*

Dua akibat nyatanya:

| Akibat | Penjelasan |
| --- | --- |
| **Kinerja** | Layar IGD menarik seluruh tabel untuk menampilkan beberapa baris. Begitu rumah sakit berjalan setahun, satu klik dapat memindahkan puluhan ribu baris |
| **Kerahasiaan** | Data pesanan **seluruh pasien** terkirim ke browser petugas yang hanya berwenang atas satu pasien. Penyaringan di browser tidak menghapus kenyataan bahwa datanya sudah sampai ke sana |

Akibat kedua yang membuat ini bukan sekadar urusan kecepatan.

---

## 2. Proses bisnis

### 2.1 Langkah yang berurutan — sesudah perubahan

1. Layar IGD membuka episode seorang pasien dan memanggil
   `GET /lab-orders?encounterId=<kunjungan pasien itu>`.
2. Backend menyaring di database, dan **hanya** pesanan pasien tersebut yang meninggalkan
   server.
3. Jawabannya berbentuk halaman: `pageNumber`, `pageSize`, `totalData`, `totalPage`, dan
   `items`.
4. Layar daftar pesanan laboratorium umum memanggil endpoint yang sama tanpa `encounterId`,
   lalu memakai `orderStatus`, `discipline`, rentang tanggal, dan pencarian sesuai kebutuhannya.

### 2.2 Parameter yang diterima

| Parameter | Tipe | Kegunaan |
| --- | --- | --- |
| `encounterId` | `guid` | Menyaring per kunjungan pasien |
| `orderStatus` | `integer` | Menyaring per status operasional |
| `discipline` | `integer` | Menyaring per disiplin laboratorium |
| `startDate`, `endDate` | `date` | Rentang waktu pesanan dibuat |
| `search` | `string` | Pencarian bebas pada kode dan nama jenis pemeriksaan |
| `sortBy` | `string` | `createDateTime` atau `orderStatus` |
| `sortDirection` | `string` | `asc` atau `desc`; bawaannya `desc` |
| `pageNumber` | `integer` | Bawaannya `1` |
| `pageSize` | `integer` | Bawaannya `25`, paling banyak `100` |

Seluruhnya opsional. Permintaan tanpa satu pun parameter tetap sah dan mengembalikan halaman
pertama.

### 2.3 Contoh berangka

Misalkan tabel memuat 8.400 pesanan, dan Ny. Sari punya 3 di antaranya.

| | Sebelum | Sesudah |
| --- | ---: | ---: |
| Baris yang meninggalkan server | 8.400 | 3 |
| Baris milik pasien lain yang ikut terkirim | 8.397 | 0 |
| Baris yang akhirnya ditampilkan | 3 | 3 |

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | :---: |
| `startDate` melewati `endDate` | Ditolak beserta pesan yang dapat ditampilkan apa adanya | `400` |
| `pageSize` diisi angka sangat besar, misalnya `5000` | Dibatasi menjadi `100`, bukan ditolak | `200` |
| `pageNumber` diisi `0` atau negatif | Dikembalikan menjadi `1` | `200` |
| `sortBy` diisi nama kolom yang tidak dikenal | Kembali ke pengurutan bawaan, **bukan ditolak** | `200` |
| Halaman yang diminta melewati data yang ada | `items` kosong, `totalData` tetap benar | `200` |

Dua keputusan di atas disengaja. Membatasi `pageSize` menutup celah menarik seluruh tabel
dengan menuliskan angka besar. Mengembalikan `sortBy` yang tidak dikenal ke bawaan menjaga
layar lama tetap berfungsi ketika sebuah kolom kelak berganti nama — layar yang gagal total
hanya karena nama kolom lebih buruk daripada layar yang urutannya kurang tepat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../DTOs/LabOrderDtos.cs` | **`LabOrderPagedQuery` baru** — sepuluh ruas penyaring, seluruhnya opsional |
| `.../Services/LabOrderService.cs` | `GetListAsync` menerima `LabOrderPagedQuery` dan mengembalikan `PagedResult<LabOrderListResponse>`. Penyaringan, pengurutan, dan pagination dikerjakan di database, bukan di memori |
| `.../Controllers/LabOrderController.cs` | `GET /` menerima `[FromQuery] LabOrderPagedQuery` dan menolak rentang tanggal terbalik dengan `400` |
| `.../Services/LabFilterMetadataFactory.cs` | Metadata Lab Order diperbarui: `SupportsServerSideFiltering` dan `SupportsServerSidePaging` menjadi benar, dan sepuluh parameternya didaftarkan |
| `.../DTOs/LabFilterAndSummaryDtos.cs` | Keterangan `SupportsServerSideFiltering` pada metadata Lab Order disesuaikan |
| `.../Controllers/LabSpecimenController.cs` | **Di luar task ini:** dua komentar XML terakhir pada action dihapus — lihat bagian 7.1 |
| `tests/.../LabFilterAndSummaryTests.cs` | Tujuh uji baru untuk penyaringan, pengurutan, dan pagination; uji kejujuran metadata Lab Order dibalik arahnya |
| `contracts/api-contract.md` | Revision `4` → `5`, beserta penilaian dampak konsumen |
| `roadmap/backend-roadmap.md` | `BE-LAB-18` ditambahkan |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`LAB-API-v1` naik ke `r5`, dan ini perubahan `breaking` pertama pada modul ini.** Bentuk respons `GET /lab-orders` berubah dari `ApiResponse<List<LabOrderListResponse>>` menjadi `ApiResponse<PagedResult<LabOrderListResponse>>`. Bentuk baris `LabOrderListResponse` sendiri **tidak berubah sama sekali** |
| Database | **Tidak ada dampak schema.** Tidak ada entity, kolom, index, maupun migration. Yang berubah adalah bentuk kueri: penyaringan dan pagination kini dikerjakan database, sehingga jumlah baris yang berpindah jauh lebih sedikit |
| Keamanan/Auth | **Membaik.** Hak aksesnya tidak berubah — tetap `LabOrder : Read` — tetapi jumlah data yang meninggalkan server untuk sebuah layar per-pasien turun dari seluruh tabel menjadi baris milik pasien itu saja. Perlu dicatat jujur: penyaring `encounterId` adalah **kenyamanan dan pembatas volume**, bukan penegak kewenangan. Siapa pun yang memegang `LabOrder : Read` tetap dapat meminta seluruh halaman tanpa penyaring. Kewenangan per-pasien adalah persoalan tersendiri yang belum pernah diputuskan blueprint ini |

### 3.3 Penilaian dampak konsumen

Wajib menurut `API_RULES`: perubahan breaking hanya boleh dengan wewenang eksplisit **dan**
penilaian dampak konsumen.

| Langkah | Hasil |
| --- | --- |
| Pencarian konsumen | `grep` atas `src` repository `QuilvianSystemFrontendDev` untuk `lab-orders` |
| Konsumen yang ditemukan | Tepat satu — `emergency-assessment-slice.jsx` pada modul IGD |
| Cara ia membaca jawaban | Lewat `unwrapItems`, yang berisi `if (Array.isArray(data)) return data; if (Array.isArray(data?.items)) return data.items;` |
| **Apakah ia putus?** | **Tidak.** Pembungkusnya sudah menangani kedua bentuk sejak awal, sehingga ia langsung membaca `data.items` |
| Apa yang berubah baginya | Perilaku, bukan bentuk. Sebelumnya ia menerima seluruh tabel; kini halaman pertama. Selama belum mengirim `?encounterId=`, pesanan pasien yang berada di luar halaman pertama tidak akan tampil |
| Perbaikan yang disarankan | Menambahkan `?encounterId=` pada pemanggilannya, lalu menghapus penyaringan di browser. Satu baris, dan sekaligus menutup `IGD-DEC-105` yang mereka catat sendiri |

**Source frontend tidak disentuh.** `AGENTS.md` mewajibkan temuan frontend dilaporkan, bukan
diam-diam diperbaiki dari task backend.

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pesanan dengan penyaring, pengurutan, dan pagination di sisi server | `LabOrder : Read` |

Request `LabOrderPagedQuery` — lihat bagian 2.2. Response
`ApiResponse<PagedResult<LabOrderListResponse>>`.

Kode status: `200` berhasil; `400` rentang tanggal terbalik; `401` belum login; `403` tanpa hak
baca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test ...UnitTests.InMemory --filter "FullyQualifiedName~LabFilterAndSummary"` | `Failed: 0, Passed: 38, Total: 38` | `PASS` | Keluaran perintah |
| Seluruh suite `QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 1051, Total: 1052` | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `VIOLATION: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| Penyaringan per kunjungan pasien | Dua pesanan pasien ini terbawa; pesanan pasien lain tidak | `PASS` | `DaftarPesanan_DisaringPerKunjunganPasien` |
| Penyaringan per status dan per disiplin | Dua dan satu, sesuai harapan | `PASS` | `DaftarPesanan_DisaringPerStatusDanPerDisiplin` |
| Bentuk paging | `pageNumber`, `pageSize`, `totalData`, `totalPage`, dan `items` seluruhnya benar | `PASS` | `DaftarPesanan_MemakaiBentukPagingYangSudahMapan` |
| Ukuran halaman dibatasi | `pageSize` `5000` menjadi `100`; `pageNumber` `0` menjadi `1` | `PASS` | `DaftarPesanan_UkuranHalamanDibatasiSeratus` |
| Pengurutan bawaan dan menaik | Terbaru lebih dulu secara bawaan; `asc` membalikkannya | `PASS` | `DaftarPesanan_DiurutkanTerbaruLebihDuluSecaraBawaan` |
| Kolom urutan tak dikenal | Kembali ke bawaan, bukan ditolak | `PASS` | `DaftarPesanan_KolomUrutanTidakDikenalKembaliKeBawaan` |
| Baris terhapus tidak tampil | `totalData` menghitung satu dari dua | `PASS` | `DaftarPesanan_TidakMenampilkanBarisYangSudahDitandaiTerhapus` |
| **Metadata tetap jujur** | Kini mengaku menyaring, `GetList` terbukti punya `[FromQuery]`, dan kesepuluh parameternya terbukti ada pada `LabOrderPagedQuery` | `PASS` | `MetadataPesanan_MengakuMenyaringDanSeluruhParameternyaNyata` |

Uji manual: `NOT FEASIBLE`. Menembak endpoint sungguhan menuntut aplikasi berjalan; aplikasi
yang sempat menyala dihentikan atas persetujuan pemilik modul agar build dapat berjalan — lihat
bagian 7.2.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Penyaring `search` | Memakai `EF.Functions.ILike` yang hanya ada pada PostgreSQL, sedangkan bukti ini berjalan di atas provider InMemory. Polanya sama persis dengan `LabValueBoundService` dan `LabRejectionReasonService` yang sudah berjalan |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi |
| Perintah database apa pun | Task ini tidak menyentuh schema |

### 5.1 Tiga uji yang sempat gagal, dan apa yang diajarkannya

Tiga uji daftar pesanan gagal pada percobaan pertama dengan koleksi kosong, padahal
`totalData`-nya benar. Sebabnya: data ujinya memakai `ProcedureId` karangan tanpa baris
`MstProcedure` yang sesungguhnya. Proyeksi daftar menyentuh navigasi `Procedure`, dan pada
relasi wajib yang principal-nya tidak ada, barisnya tidak ikut terbawa sama sekali.

Perbaikannya membenahi data ujinya, bukan melonggarkan proyeksinya. Pada database sungguhan
keadaan itu **tidak mungkin terjadi**: `ProcedureId` wajib dan relasinya `Restrict`, sehingga
jenis pemeriksaan yang masih dirujuk pesanan tidak dapat dihapus. Ini kegagalan yang berguna —
ia memaksa data uji menyerupai keadaan sebenarnya, dan pelajarannya sama dengan yang tercatat
pada laporan [`BE-LAB-17`](BE-LAB-17.md) bagian 5.1.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

`NOT APPLICABLE`. Task lahir dari instruksi langsung pemilik modul, bukan dari acceptance
criteria blueprint.

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Sepuluh parameter diterima dan diproses | **Terpenuhi** | Tujuh uji perilaku pada bagian 5, ditambah uji yang mencocokkan metadata dengan `LabOrderPagedQuery` |
| Kontrak naik ke `r5` | **Terpenuhi** | `contracts/api-contract.md` revision `5` beserta penilaian dampaknya |
| Dampak konsumen dinilai dan dilaporkan | **Terpenuhi** | Bagian 3.3 |
| Seluruh uji lulus | **Terpenuhi** | 38 uji berkas ini lulus; suite penuh menyisakan satu kegagalan Billing yang sudah ada sebelumnya |
| Checker QBE lolos | **Terpenuhi** | `Final result: PASS` |

Tidak ada butir DoD yang belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | Modul IGD belum mengirim `?encounterId=`. Selama itu, pesanan pasien yang berada di luar halaman pertama tidak akan tampil pada layarnya. **Ini perlu disampaikan ke pemilik IGD sebelum rilis** |
| Risiko tersisa | **Sedang.** Satu-satunya perubahan breaking modul ini. Diturunkan oleh penilaian dampak yang membuktikan konsumen tunggalnya tidak putus, tetapi perubahan perilakunya nyata dan perlu tindakan satu baris di sisi IGD |
| Risiko tersisa kedua | Penyaring `encounterId` membatasi volume, **bukan menegakkan kewenangan**. Pemegang `LabOrder : Read` tetap dapat meminta seluruh halaman tanpa penyaring. Kewenangan per-pasien belum pernah diputuskan blueprint ini dan tetap terbuka |
| Perubahan sampingan | `NONE` |
| Interupsi | Aplikasi backend yang sedang menyala mengunci berkas build — lihat bagian 7.2 |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. Memberi tahu pemilik IGD agar menambahkan `?encounterId=` dan menutup `IGD-DEC-105` dari sisi mereka. 2. `BE-LAB-16` — endpoint pemeriksaan terpesan. 3. `BE-LAB-10` — penandaan cito per pemeriksaan |

### 7.1 Penghapusan dua deskripsi Swagger terakhir

Atas permintaan pemilik modul, dua komentar XML terakhir pada action `LabSpecimenController`
dihapus: `POST /{id}/accept` dan `POST /{id}/cancel`. Isinya dipertahankan sebagai komentar
biasa `//`.

Dengan ini **seluruh grup Laboratorium bersih** dari deskripsi endpoint Swagger — total tujuh
belas blok dihapus lintas tiga sesi task, tercatat pada laporan `BE-LAB-09` bagian 7.3,
`BE-LAB-17` bagian 7.1, dan di sini.

### 7.2 Interupsi — aplikasi backend mengunci berkas build

Di tengah verifikasi, `dotnet build` gagal dengan `MSB3027`: proses
`QuilvianSystemBackend` (PID 5068, menyala sejak 15:54) mengunci
`bin\Debug\net9.0\QuilvianSystemBackend.exe`.

Proses itu **tidak dihentikan sepihak**; keputusannya diminta lebih dulu kepada pemilik modul,
yang menyetujui penghentiannya. Sesudah dihentikan, build dan seluruh uji berjalan normal.
Aplikasi perlu dijalankan ulang bila Swagger hendak dibuka — dan justru harus dijalankan ulang
agar endpoint hasil `BE-LAB-17` dan `BE-LAB-18` muncul di sana.
