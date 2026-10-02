# Laporan Perubahan Backend — `BE-LAB-35`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-35` |
| Judul | Daftar pemeriksaan terpesan pada detail pesanan |
| Slice | `MVP-5e`, `EPIC-LAB-12` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6f |
| Trace | `BR-47`, `LAB-DEC-055`, `LAB-DEC-057`; keputusan pemilik modul 2026-09-16 atas cakupan `FE-LAB-17` |
| Contract version | `LAB-API-v1` **`r15`** bagian 10 — **`approved` 2026-09-16** |
| Dependency | `BE-LAB-26` ✅, `BE-LAB-27` ✅, persetujuan `r15` ✅. **Nol penahan** |
| Klasifikasi | `LIGHT` — satu DTO baru, satu ruas, satu proyeksi. Nol migration, nol endpoint, nol permission |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709` pada branch `yoga` (perubahan task ini belum di-stage maupun di-commit) |
| Tanggal | 2026-09-16 |
| Status | **`SELESAI`** — keempat butir DoD terpenuhi dan terbukti dari database |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement` / Laboratory |
| Submodule | — |
| Pemilik / prefix pada registry | `Lab`, status **`ACTIVE`** sejak 2026-09-02 lewat `LAB-REQ-002` |
| Keberlakuan | **`NEW CODE`** untuk DTO `LabOrderedProcedureResponse`; **`TOUCHED LEGACY`** untuk proyeksi `GetDetailAsync` yang sudah ada |
| Status registry | Terdaftar dan aktif. **Nol penghalang `QBE-MOD-002` maupun `QBE-MOD-003`** — task ini tidak membuat satu pun entity yang dipersistensi |
| QBE yang berlaku | `QBE-DTO-001` — entity EF tidak diekspos sebagai kontrak API; `QBE-SVC-001` — kueri dimiliki Module Service, controller tidak menyentuh context; `QBE-API-001` — memakai boundary respons yang sudah mapan; `QBE-OPT-001` — hanya menyediakan yang benar-benar dikonsumsi |
| QBE yang **tidak** berlaku, dan alasannya | `QBE-ENT-001`..`003`, `QBE-CFG-001` — nol entity dipersistensi dibuat. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`. `QBE-CODE-001`..`006` — nol nomor bisnis dialokasikan. `QBE-PERM-001` — nol permission baru; endpointnya sudah ada beserta `LabOrder : Read` |

---

## 1. Masalah yang diperbaiki

`BR-47` menetapkan satu tindakan petugas menghasilkan **satu pesanan per disiplin**, dan
pemeriksaan yang sedisiplin **berkumpul pada pesanan yang sama**. `BE-LAB-26` mendirikan
`LabOrderedProcedure` untuk menyimpan daftarnya; `BE-LAB-27` mengisinya.

Yang tidak pernah dikerjakan: **mengembalikannya.**

| Yang dinilai | Keadaan sebelum task ini |
| --- | --- |
| DTO respons yang memuat daftar pemeriksaan terpesan | **Nol.** Pencarian `ProcedureNameSnapshot` pada seluruh area `LaboratoryManagement` menghasilkan nol kemunculan |
| Endpoint yang mengembalikannya | **Nol** |
| `LabOrder.ProcedureId` | **Penunjuk wakil**, dinyatakan komentar kodenya sendiri: *"Penunjuk wakil, bukan satu-satunya pemeriksaan pesanan ini"* |
| `LabMonitoringItemResponse.ExaminationCount` | Menghitung `LabExamination` — yang sedang dikerjakan **dari wadah**, bukan yang dipesan. Pesanan yang belum berwadah bernilai `0` |

Akibatnya tunggal: **tidak ada satu pun konsumen yang dapat mengetahui isi sebuah pesanan.**
Untuk pesanan Hemoglobin + Kalium yang sengaja digabung `BR-47`, layar hanya mengetahui satu nama.

**Kenapa celah ini tidak memutus apa pun selama dua hari.** Ketiga menu pemeriksaan adalah layar
**antrean** — mereka memang tidak menampilkan isi pesanan — dan `VAL-68` serta `VAL-69` membaca
`LabOrderedProcedure` **di dalam** backend, sehingga kedua aturan tetap tegak tanpa satu pun ruas
respons. Yang pertama membutuhkannya adalah konsumen yang **mencetak**, yaitu `FE-LAB-17`.

---

## 2. Proses bisnis

Petugas menekan tombol cetak pada satu baris di menu pemeriksaan. Layar memanggil
`GET /lab-orders/{id}`, dan sejak task ini jawabannya memuat **pemeriksaan apa saja yang
benar-benar dipesan** — bukan hanya satu nama wakil.

Contoh konkret. Petugas memilih Hemoglobin, Kalium, dan Natrium sekaligus; ketiganya Patologi
Klinik, sehingga `BR-47` menggabungkannya menjadi **satu** pesanan. Sebelum task ini, dokumen
cetak hanya dapat menyebut `Hemoglobin`. Sesudahnya, ia menyebut ketiganya beserta kesegeraan
masing-masing — karena penanda cito melekat pada pemeriksaan, bukan pada pesanan
(`LAB-DEC-026`), sehingga satu pesanan boleh memuat cito dan biasa sekaligus.

### 2.1 Jalur tidak normal, dan aturannya ditulis supaya tidak ditafsirkan sendiri

**Pesanan lama tidak punya baris terpesan.** Pesanan yang dibuat lewat jalur lama
`POST /lab-orders` berpemeriksaan tunggal dan **nol** memiliki baris `LabOrderedProcedure` —
keadaan yang sudah diakui `LabSpecimenService` sejak `BE-LAB-28`. Jawabannya **array kosong**,
bukan galat.

> Bila `orderedProcedures` **kosong**, pesanan itu berpemeriksaan tunggal dan `procedureName`
> **adalah** isi lengkapnya. Bila **terisi**, `procedureName` hanyalah wakil dan **tidak boleh**
> dipakai sebagai isi pesanan.

**Baris yang dibatalkan ikut dikembalikan**, beserta `orderedStatus`-nya. Menyaringnya di sumber
berarti konsumen tidak dapat membedakan pemeriksaan yang **tidak pernah dipesan** dari yang
**dipesan lalu dibatalkan** — dan pada dokumen resmi keduanya bermakna berbeda.

**Baris yang ditandai terhapus tidak ikut.** `IsDelete` disaring di proyeksi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs` | DTO baru `LabOrderedProcedureResponse` berisi empat ruas snapshot, dan satu ruas `OrderedProcedures` pada `LabOrderDetailResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | Proyeksi `OrderedProcedures` di dalam `GetDetailAsync` |

**Nol berkas lain disentuh.** Nol migration, nol controller, nol configuration, nol enum baru.

### 3.2 Tiga keputusan implementasi yang pantas dibaca ulang

**Pertama — dibaca dari kolom snapshot, bukan dari katalog hari ini.** Keempat ruasnya mengambil
`ProcedureCodeSnapshot` dan `ProcedureNameSnapshot`, bukan menoleh ke `MstProcedure` yang berlaku
saat dokumen dicetak. Dokumen resmi harus menyebut apa yang **dipesan waktu itu**; nama katalog
yang kemudian diganti tidak boleh mengubah isi dokumen yang sudah pernah dicetak. Hal ini diuji
langsung — lihat `S5`.

**Kedua — nol penunjuk dikirim.** `ProcedureId` **tidak** ada pada DTO. Konsumen pertamanya
adalah dokumen cetak, keluaran **baca** yang tidak melakukan aksi apa pun terhadap baris
pemeriksaan; mengirim penunjuk berarti mengirim nilai yang tidak boleh ditampilkan
(`no-uuid-display`) ke konsumen yang tidak membutuhkannya. Alasan yang sama dipakai `r14`
bagian 9.3. Diuji lewat refleksi atas DTO-nya — lihat `S11`.

**Ketiga — proyeksinya sub-query, bukan navigation property.** `LabOrder` tidak memiliki koleksi
`OrderedProcedures`, dan task ini **tidak menambahkannya**: menambah navigation berarti menyentuh
entity dan configuration untuk kebutuhan yang murni pembacaan. Sub-query dipakai, persis pola yang
sudah dipakai proyeksi yang sama untuk `Users` dan `MstDoctor`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Yang dinilai | Hasil |
| --- | --- |
| Bentuk respons | **Aditif.** Nol endpoint, ruas, nilai enum, atau pembungkus yang berubah, berganti nama, atau hilang |
| Pembaca lama `GET /lab-orders/{id}` | Menerima satu ruas tambahan yang boleh diabaikan |
| Database | **Nol migration.** Tabel beserta keempat kolom snapshotnya berdiri sejak `BE-LAB-26` |
| Permission | **Nol** resource maupun action baru. Tetap `LabOrder : Read` |
| Keamanan | Nol penunjuk dan nol data pasien tambahan diekspos. Keempat ruasnya adalah kode, nama, kesegeraan, dan status pemeriksaan |

---

## 4. Endpoint yang berubah

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Yang berubah | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Responsnya bertambah ruas `orderedProcedures`. Bentuk, status, dan hak aksesnya **tidak berubah** | `LabOrder : Read` |

Bentuk `LabOrderedProcedureResponse`:

| Ruas | Tipe | Isi |
| --- | --- | --- |
| `procedureCode` | `string?` | Kode pemeriksaan pada saat dipesan |
| `procedureName` | `string?` | Nama pemeriksaan pada saat dipesan, siap tampil |
| `urgency` | `string` | `Routine` atau `Cito` |
| `orderedStatus` | `string` | `Ordered`, `Fulfilled`, atau `Cancelled` |

---

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Sama persis dengan baseline; **nol warning** dari kedua berkas yang diubah |
| Verifikasi kontrak terhadap `r15` bagian 10 | Ruas, tipe, dan tempatnya cocok | `PASS` | Bagian 4 |
| Dua belas pemeriksaan terhadap database sungguhan | **12 `PASS`, 0 `FAIL`** | `PASS` | 5.1 |
| Kebersihan database sesudah uji | Nol baris uji tersisa | `PASS` | 5.2 |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT RUN` | Memerlukan aplikasi berjalan; hak akses dan bentuk endpointnya tidak berubah |
| Migration | Tidak ada, dan memang tidak diperlukan | `NOT APPLICABLE` | Tabel sudah berdiri sejak `BE-LAB-26` |

### 5.1 Dua belas pemeriksaan dijalankan sungguhan terhadap database

Harness sekali pakai di scratchpad sesi, **di luar repository**, memakai `ApplicationDbContext`
dan `LabOrderService` yang sebenarnya. Dua pesanan uji dibuat sendiri — **nol dari 5 pesanan
nyata disentuh**.

| Butir | Hasil |
| --- | --- |
| `S1` pesanan jamak mengembalikan daftar terpesan | 3 baris, dari 4 yang tersimpan |
| `S2` baris ber-`IsDelete` **tidak** ikut terbaca | Nol baris terhapus pada hasil |
| `S3` baris `Cancelled` **ikut** terbaca beserta statusnya | `Ordered,Fulfilled,Cancelled` |
| `S4` urutan mengikuti penyimpanan | `SNAP-A,SNAP-B,SNAP-C` |
| `S5` nama dibaca dari **snapshot**, bukan katalog hari ini | `AAA Snapshot Lama` |
| `S6` kesegeraan terbaca sebagai nama enum | `Cito` / `Routine` |
| `S7` status terpesan terbaca sebagai nama enum | `Fulfilled` |
| `S8` pesanan lama tanpa baris terpesan mengembalikan array **kosong**, bukan `null` | `null?=False jumlah=0` |
| `S9` ruas lama detail tetap terisi apa adanya | `status=Requested disiplin=ClinicalPathology wakil=Glukosa Darah Sewaktu` |
| `S10` daftar terpesan **tidak** ikut mengubah ruas wakil | `wakil=Glukosa Darah Sewaktu` |
| `S11` DTO **nol** memuat penunjuk | `OrderedStatus,ProcedureCode,ProcedureName,Urgency` |
| `S12` database bersih sesudah uji | `LabOrder 5→5`, `LabOrderedProcedure 0→0` |

**Empat pemeriksaan sengaja menguji ketiadaan**, bukan keberadaan — `S2`, `S8`, `S10`, dan `S11`.
Tanpa `S10`, proyeksi yang keliru mengikat dapat menimpa `ProcedureName` wakil dengan nama
snapshot dan tetap terlihat benar pada pandangan pertama. Tanpa `S11`, penunjuk dapat ikut
terkirim tanpa ada yang menyadarinya sampai muncul di layar.

`S5` **dirancang agar gagal bila implementasinya salah**: nama snapshotnya sengaja dibuat berbeda
dari nama katalog aslinya. Proyeksi yang menoleh ke `MstProcedure` akan mengembalikan nama
katalog, dan pemeriksaan ini akan menangkapnya.

### 5.2 Kebersihan database

| Butir | Sebelum | Sesudah |
| --- | :---: | :---: |
| Total `LabOrder` | 5 | **5** |
| Total `LabOrderedProcedure` | 0 | **0** |

Kedua pesanan uji beserta keempat baris terpesannya dihapus **permanen**, dan hitungannya
diulang dari **koneksi baru**, bukan dari context yang sama.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| `orderedProcedures` ada pada detail sesuai `r15` | **Terpenuhi** | Bagian 4; `S1` |
| Keempat ruasnya dibaca dari kolom **snapshot**, bukan katalog hari ini | **Terpenuhi** | `S5` — nama snapshot sengaja dibuat berbeda dari katalog, dan yang terbaca adalah snapshotnya |
| Pesanan lama mengembalikan array kosong | **Terpenuhi** | `S8` — kosong, dan **bukan** `null` |
| Nol penunjuk dikirim | **Terpenuhi** | `S11` — DTO tepat berisi empat ruas, nol di antaranya penunjuk |
| Nol ruas lama berubah | **Terpenuhi** | `S9`, `S10`; build 0 Error dengan baseline warning yang sama persis |

Task ini **tidak menambah acceptance criteria baru**. Ia melengkapi prasyarat `FE-LAB-17`, yang
menurunkan bagian "tampil pada ringkasan cetak" milik `AC-95`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Nol peringatan baru.** 207 warning build seluruhnya XML-doc lama pada modul lain, dan jumlahnya sama persis dengan baseline. Nol warning berasal dari kedua berkas yang diubah |
| Masalah yang diketahui | **Satu temuan yang perlu diketahui `FE-LAB-17` sebelum dimulai:** `LabOrderedProcedure` berisi **0 baris** pada database sebelum uji ini. Artinya **seluruh 5 pesanan nyata hari ini akan menempuh jalur "array kosong"**, dan layar cetak wajib menangani jalur itu dengan benar — bukan memperlakukannya sebagai data rusak. Jalur berisi baru muncul untuk pesanan yang dibuat lewat `POST /lab-orders/by-examinations` |
| Dependency | Nol tersisa. `BE-LAB-26` ✅, `BE-LAB-27` ✅, `r15` ✅ |
| Migration / eksekusi database | **Nol migration dibuat dan nol dijalankan.** Yang menyentuh database hanyalah dua pesanan uji yang dibuat lalu dihapus permanen dalam sesi ini |
| Perubahan sampingan | `NONE`. Kedua berkas yang diubah memang sudah berstatus `M` sebelum task ini karena `BE-LAB-33` dan `BE-LAB-34` belum di-commit; perubahan task ini bertambah di atasnya, nol baris milik task lain disentuh |
| Interupsi | `NONE` |
| Status Git | Branch `yoga`. **Nol `git add`, nol commit, nol push.** Berkas milik task ini: `LabOrderDtos.cs` dan `LabOrderService.cs`, keduanya `M` |
| Langkah berikutnya | **`FE-LAB-17` kini nol penahan.** Ketiga keputusan pemiliknya sudah ditetapkan, ketiga tanda tangan dapat diisi, dan isi pesanannya kini dapat dibaca. Itu satu-satunya sisa modul Laboratorium |
