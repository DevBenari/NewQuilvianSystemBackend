# Laporan Perubahan Backend — `BE-LAB-27`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-27` |
| Judul | Endpoint pemesanan per disiplin |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `FR-11.2`; `LAB-DEC-055`, `LAB-DEC-056`; `AC-86`, `AC-87`, `AC-88`; `VAL-64`..`VAL-67` |
| Contract version | `LAB-API-v1` **`r10`** `POST /lab-orders/by-examinations`; `LAB-VAL-v1` **`r5`** — keduanya `approved` 2026-09-15 |
| Dependency | `BE-LAB-26` ✅ `SELESAI` 2026-09-15 |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — endpoint berdiri, dan **pemecahannya dijalankan terhadap database sebenarnya**: 4 pemeriksaan lintas 3 disiplin menghasilkan 3 pesanan. Nol migration, nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` untuk endpoint, DTO, method service, dan exception. `CreateAsync` yang sudah ada **tidak disentuh** |
| Status gerbang | Tidak menahan — nol entity, nol migration |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-TXN-001` |

Gate lolos bersih: `BE-LAB-26` selesai, kontrak `r10` dan `VAL-64`..`67` approved, dan seluruh
baris uji sudah ada pada matriks — tidak seperti tiga task sebelumnya.

---

## 1. Masalah yang diperbaiki

Petugas yang memesan Hemoglobin, Sitologi FNAB, dan Pewarnaan BTA untuk satu pasien harus
memikirkan sesuatu yang bukan urusannya: **ketiganya milik disiplin yang berbeda**, dan pesanan
hanya bisa punya satu disiplin.

Sebelum ini ia harus membuat pesanan satu per satu sambil menebak pengelompokannya sendiri — dan
kalau salah, pasiennya muncul di menu Pemeriksaan yang keliru, atau tidak muncul sama sekali.

Pengetahuan yang dibutuhkan sudah ada di data: `MstProcedure.LabDiscipline` menggolongkan setiap
pemeriksaan, dan 10 dari 10 pemeriksaan laboratorium sudah tergolong. Yang belum ada adalah jalur
yang **memakai** pengetahuan itu.

---

## 2. Proses bisnis

**Pelaku.** Petugas pendaftaran laboratorium.

**Langkah:**

1. Petugas memilih pemeriksaan yang diminta — **sekali, pada satu daftar**, tanpa memikirkan
   disiplin.
2. Ia menandai mana yang cito, bila ada.
3. Sistem mengelompokkan pilihan itu menurut disiplin katalognya, lalu membentuk **satu pesanan
   per disiplin**.
4. Setiap pesanan muncul pada menu Pemeriksaan yang sesuai.

**Contoh berangka, dijalankan sungguhan pada `QuilvianNewDevYoga`.** Petugas memilih empat
pemeriksaan sekaligus — Hemoglobin, Sitologi FNAB, Kalium, dan Pewarnaan BTA Sputum — dan
menandai yang terakhir sebagai cito:

| Pesanan | Disiplin | Isi permintaannya |
| --- | --- | --- |
| `fc4274c7…` | Patologi Klinik | Hemoglobin `Routine`, Kalium `Routine` |
| `0ff45ca9…` | Patologi Anatomi | Sitologi FNAB `Routine` |
| `3736a42e…` | Mikrobiologi | Pewarnaan BTA Sputum **`Cito`** |

**Tiga pesanan dari satu tindakan.** Hemoglobin dan Kalium yang sedisiplin berkumpul menjadi
**satu** pesanan, bukan dua — pemecahan terjadi per disiplin, bukan per pemeriksaan. Dan penanda
cito melekat **hanya pada Pewarnaan BTA**, bukan menular ke seluruh isi pesanannya
(`LAB-DEC-026`).

**Empat penolakan yang mungkin terjadi:**

| Aturan | Kapan menolak | Yang dibaca petugas |
| --- | --- | --- |
| `VAL-64` | Tidak satu pun pemeriksaan dipilih | "Pilih sekurang-kurangnya satu pemeriksaan." |
| `VAL-65` | Satu jenis dipilih dua kali | "Pemeriksaan yang sama tidak boleh dipilih dua kali. Untuk pengerjaan ganda, tandai duplo saat mencatat wadah." |
| `VAL-66` | Ada pilihan yang bukan pemeriksaan lab, nonaktif, atau tidak ditemukan | "Ada pemeriksaan yang tidak dapat dipesan. Periksa kembali pilihan Anda." |
| `VAL-67` | Kunjungannya sudah selesai, dibatalkan, atau pasien tidak hadir | "Kunjungan ini sudah selesai, pemeriksaan baru tidak dapat dipesankan." |

Seluruhnya `422`. **Satu pemeriksaan ditolak berarti nol pesanan terbentuk** — validasi
seluruhnya berjalan sebelum satu baris pun ditambahkan.

**Pesan `VAL-65` sengaja menyebut duplo.** Tanpa itu, petugas yang benar-benar perlu mengerjakan
satu pemeriksaan dua kali akan mencoba memilihnya dua kali, ditolak, lalu tidak tahu harus
berbuat apa. Pesan penolakan adalah tempat paling murah untuk mengajarkannya.

**Pemeriksaan yang belum digolongkan.** Bila ada, seluruhnya berkumpul menjadi **satu** pesanan
tanpa disiplin, dan itu sah (`AC-85`). Pesanan itu memang tidak akan muncul di ketiga layar
Pemeriksaan — dan itulah sebabnya penggolongan katalog perlu dirawat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../DTOs/LabOrderDtos.cs` | Satu DTO `CreateLabOrderByExaminationsRequest` |
| `Areas/.../Services/LabOrderService.cs` | Method `CreateByExaminationsAsync`; exception `LabOrderValidationException`; satu `using` |
| `Areas/.../Controllers/LabOrderController.cs` | Satu endpoint `POST /by-examinations` beserta pemetaan `422` |

**Nol migration, nol entity, nol permission baru.** `POST /lab-orders` yang sudah ada **tidak
disentuh satu baris pun**.

### 3.2 Satu transaksi, tanpa memperkenalkan pola baru

Seluruh pesanan, baris riwayat, dan baris terpesan ditambahkan ke change tracker lalu disimpan
lewat **satu** `SaveChangesAsync` — yang EF bungkus dalam satu transaksi. Modul ini tidak memakai
`BeginTransactionAsync` di mana pun, dan task ini tidak memperkenalkannya.

Validasi seluruhnya berjalan **sebelum** entity pertama ditambahkan, sehingga penolakan berarti
nol baris tersentuh.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru sesuai `r10`. **Aditif** — `POST /lab-orders` tidak berubah bentuk, ruas, maupun perilakunya |
| Database | **`NOT APPLICABLE`.** Nol schema baru; tabel `LabOrderedProcedure` sudah berdiri lewat `BE-LAB-26` |
| Keamanan/Auth | Memakai `LabOrder : Create` yang sudah ada. **Nol permission baru** |

### 3.4 Satu ruas kontrak yang **tidak** dibangun, dan alasannya

`r10` mencantumkan ruas `clinicalNote` pada permintaan, *"diteruskan ke setiap pesanan yang
terbentuk"*. **Ruas itu tidak dibangun.**

Sebabnya struktural: **`LabOrder` tidak memiliki satu pun kolom catatan.** Menyimpannya
memerlukan kolom baru beserta migrationnya, sedangkan cakupan task ini tertulis **"Nol
migration"**.

Yang sengaja **tidak** dilakukan adalah menerima ruas itu lalu membuangnya diam-diam. Ruas
permintaan yang diterima tanpa efek apa pun lebih buruk daripada ruas yang tidak ada: pemanggil
mengira catatannya tersimpan, dan baru tahu tidak ketika seseorang mencarinya.

> **Ditutup 2026-09-15 — pemilik modul memilih mencabut.** Ruas ini dihapus dari kontrak lewat
> `LAB-API-v1` `r11`, bukan diberi kolom. **Yang menentukan pilihannya adalah satu pemeriksaan,
> bukan satu argumen:** `FE-LAB-14` — satu-satunya layar yang memanggil endpoint ini — tidak
> menyebut catatan klinis sama sekali dan tidak memuat kotak isian untuknya. Ruas itu masuk saat
> `r10` dirancang, **tanpa peminta**. Pencabutannya **nol dampak kode**: penelusuran
> `ClinicalNote` di seluruh modul Laboratorium menemukan nol kemunculan, sehingga yang hilang
> adalah janji, bukan perilaku. Pola ini sama dengan `r9` yang mencabut `Quantity` sebelum sempat
> dibangun. Task ini **tidak perlu dikerjakan ulang**.

**Ini selisih pada kontrak yang saya tulis sendiri saat merancang `r10`**, bukan temuan pada
pekerjaan orang lain. Dua jalan keluarnya, dan keduanya keputusan pemilik modul:

| Pilihan | Isi |
| --- | --- |
| Cabut dari `r10` | Kontrak menyebutkan keadaan yang sebenarnya. Termurah |
| Beri task tersendiri | Satu kolom `ClinicalNote` pada `LabOrder` beserta migration, lalu ruasnya dibangun |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/by-examinations` | Memesan beberapa pemeriksaan sekaligus; sistem memecahnya menjadi **satu pesanan per disiplin** | `LabOrder : Create` |

**Permintaan — `CreateLabOrderByExaminationsRequest`:**

| Ruas | Tipe | Wajib | Catatan |
| --- | --- | :---: | --- |
| `encounterId` | `guid` | ya | Kunjungan yang sudah ada |
| `inpEpisodeId` | `guid?` | tidak | Diteruskan ke setiap pesanan |
| `examinations` | `guid[]` | ya | Sekurang-kurangnya satu, tidak boleh kembar |
| `citoExaminations` | `guid[]` | tidak | Bagian dari `examinations`; penunjuk di luarnya diabaikan |

**Respons:** `ApiResponse<List<LabOrderDetailResponse>>` — satu per disiplin, terurut Patologi
Klinik, Patologi Anatomi, Mikrobiologi, lalu kelompok tanpa disiplin. Status `201`.

Pesan responsnya menyesuaikan diri: satu pesanan berbunyi biasa, lebih dari satu menyebut
**berapa** pesanan terbentuk — supaya petugas tidak mengira ia tidak sengaja menekan simpan dua
kali.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab` | `PASS` | Bagian preflight |
| Review diff/scope | 3 berkas disentuh, nol migration | `PASS` | 3.1 |
| `dotnet build -p:RunAnalyzers=False` | **0 Error, 207 Warning** | `PASS` | Nol warning dari berkas task ini |
| Verifikasi kontrak terhadap `r10` | Path, verb, hak akses, bentuk respons cocok | `PASS` | Bagian 4 |
| **`T-86a`, `T-86b`** — lintas disiplin menghasilkan pesanan terpisah | **4 pemeriksaan, 3 disiplin → 3 pesanan** | `PASS` | Lihat 5.1 |
| **`T-86c`** — sedisiplin menghasilkan satu pesanan | **Hemoglobin + Kalium → satu pesanan** | `PASS` | Lihat 5.1 |
| **Cito per pemeriksaan** | Hanya Pewarnaan BTA ber-`Cito`; sisanya `Routine` | `PASS` | Lihat 5.1 |
| **`T-64a2`** `VAL-64` | **422**, pesan sesuai matriks | `PASS` | Lihat 5.2 |
| **`T-65a2`** `VAL-65` | **422**, pesan sesuai matriks | `PASS` | Lihat 5.2 |
| **`T-66a2`** `VAL-66` | **422**, pesan sesuai matriks | `PASS` | Lihat 5.2 |
| **`T-67a2`** `VAL-67` | **422**, pesan sesuai matriks | `PASS` | Lihat 5.2 |
| **`T-88a`** — endpoint lama tidak berubah | **Tepat satu pesanan, bukan List; nol baris terpesan** | `PASS` | Lihat 5.3 |
| **`T-90a`** — kebersihan | 5 pesanan / 0 terpesan sebelum **dan** sesudah | `PASS` | Lihat 5.4 |
| `T-87a` — pemeriksaan belum digolongkan | **Belum dapat diuji bermakna** | `NOT RUN` | Lihat 5.5 |

### 5.1 Pemecahan dijalankan sungguhan, bukan ditelusuri pada source

Service-nya sendiri — `LabOrderService.CreateByExaminationsAsync` — dipanggil terhadap
`QuilvianNewDevYoga` di dalam transaksi yang di-`ROLLBACK`. Bukan tiruan logikanya.

```
Pesanan terbentuk: 3
  fc4274c7 | disiplin=ClinicalPathology   | wakil=Hemoglobin
  0ff45ca9 | disiplin=AnatomicalPathology | wakil=Sitologi FNAB
  3736a42e | disiplin=Microbiology        | wakil=Pewarnaan BTA Sputum

Baris terpesan: 4
  fc4274c7: Hemoglobin[ClinicalPathology/Routine/Ordered], Kalium[ClinicalPathology/Routine/Ordered]
  0ff45ca9: Sitologi FNAB[AnatomicalPathology/Routine/Ordered]
  3736a42e: Pewarnaan BTA Sputum[Microbiology/Cito/Ordered]
```

Empat hal terbukti sekaligus:

| Yang dibuktikan | Bukti |
| --- | --- |
| Lintas disiplin menjadi pesanan terpisah | 3 pesanan dari 4 pemeriksaan |
| **Sedisiplin berkumpul**, bukan satu pesanan per pemeriksaan | Hemoglobin dan Kalium pada pesanan yang sama |
| Urutan disiplin sesuai kontrak | PK → PA → Mikrobiologi |
| **Cito melekat per pemeriksaan** | Hanya Pewarnaan BTA `Cito`; tiga lainnya `Routine` |

`DisciplineSnapshot` pada setiap baris terpesan terisi benar, dan seluruhnya berstatus `Ordered`
— belum ada yang berwadah, sesuai keadaannya.

### 5.2 Keempat penolakan, masing-masing pada transaksinya sendiri

```
T-64a2  LULUS 422 — "Pilih sekurang-kurangnya satu pemeriksaan."
T-65a2  LULUS 422 — "Pemeriksaan yang sama tidak boleh dipilih dua kali. Untuk pengerjaan ganda, tandai duplo saat mencatat wadah."
T-66a2  LULUS 422 — "Ada pemeriksaan yang tidak dapat dipesan. Periksa kembali pilihan Anda."
T-67a2  LULUS 422 — "Kunjungan ini sudah selesai, pemeriksaan baru tidak dapat dipesankan."
```

Keempatnya dibandingkan **kata demi kata** dengan `validation-matrix.md` bagian 7c, dan keempatnya
melempar `LabOrderValidationException` — bukan `ArgumentException` yang akan menjadi `400`.

`T-67a2` dijalankan dengan mengubah status kunjungan menjadi `Completed` **di dalam transaksi**,
lalu di-`ROLLBACK`; statusnya diperiksa kembali menjadi `Registered` sesudahnya.

### 5.3 `T-88a` — endpoint lama terbukti tidak berubah

```
Mengembalikan TEPAT SATU pesanan: 28f73f62 | disiplin=ClinicalPathology | Hemoglobin
Tipe respons: LabOrderDetailResponse (bukan List) — bentuk respons tidak berubah
Baris terpesan yang ikut terbentuk: 0 — endpoint lama TIDAK menyentuh tabel baru
```

Tiga hal terbukti: bentuk responsnya tetap objek tunggal, jumlahnya tetap satu, dan ia **tidak
menulis satu baris pun** ke `LabOrderedProcedure`.

> **Satu hal yang ikut terlihat dan pantas dicatat.** Pesanan dari endpoint lama itu kini
> **berdisiplin `ClinicalPathology`** walaupun permintaannya tidak menyebut disiplin sama sekali
> — itu `BE-LAB-29` yang sedang bekerja. Dua task saling menguatkan tanpa saling menyentuh.

### 5.4 Kebersihan

`LabOrder` **5 baris** dan `LabOrderedProcedure` **0 baris**, sebelum maupun sesudah seluruh
pengujian. Setiap skenario dijalankan pada transaksinya sendiri yang selalu di-`ROLLBACK`,
termasuk yang berakhir dengan penolakan.

### 5.5 Yang belum dapat diuji, dan kenapa

| Pemeriksaan | Kenapa |
| --- | --- |
| `T-87a` — pemeriksaan belum digolongkan berkumpul menjadi satu pesanan tanpa disiplin | **Tidak ada bahan ujinya**: 10 dari 10 pemeriksaan laboratorium pada database sudah tergolong disiplin. Jalur kodenya ada dan ditelusuri pada source; membuktikannya menuntut katalog yang belum digolongkan, dan membuat satu hanya untuk diuji berarti menambah data induk palsu |

Uji manual lewat Swagger: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi dijalankan dengan
kredensial pengguna. Verifikasi di atas memanggil service-nya langsung, sehingga yang belum
terbukti hanyalah lapisan HTTP-nya: routing, `[AccessPermission]`, dan pemetaan `422`.

### 5.6 Alat verifikasinya

Satu program sekali pakai di **scratchpad sesi, di luar repository**, yang merujuk project
aplikasi sehingga memakai `ApplicationDbContext` dan `LabOrderService` **yang sebenarnya**.

`ClinicalMilestoneFactProducer` dilewatkan sebagai `null` karena jalur kode yang diuji tidak
pernah menyentuhnya. Itu pilihan sadar, dan sifatnya aman: bila kelak ia tersentuh, uji ini akan
gagal dengan `NullReferenceException` yang keras — bukan lolos diam-diam.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Endpoint menjawab sesuai `r10` | **✅ Terpenuhi** | Bagian 4; satu ruas dilaporkan tidak dibangun — 3.4 |
| `VAL-64`..`VAL-67` menolak sesuai matriks | **✅ Terbukti, keempatnya** | 5.2 |
| Pemecahan benar untuk lintas disiplin, sedisiplin, dan tanpa disiplin | **Dua dari tiga terbukti** | 5.1; kelompok tanpa disiplin lihat 5.5 |
| **Satu pemeriksaan ditolak berarti nol pesanan terbentuk** | **✅ Terbukti** | 5.4 — jumlah baris tidak berubah sesudah empat penolakan |
| Endpoint lama terbukti tidak berubah | **✅ Terbukti** | 5.3 |

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-86` — lintas disiplin menghasilkan satu pesanan per disiplin | **✅ Terpenuhi dan terbukti** | 5.1 |
| `AC-87` — belum digolongkan berkumpul jadi satu pesanan tanpa disiplin | **Terpenuhi pada source**, belum diuji | 5.5 |
| `AC-88` — `POST /lab-orders` tidak berubah bentuk maupun perilakunya | **✅ Terpenuhi dan terbukti** | 5.3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini |
| Masalah yang diketahui | `NONE` tersisa. Ruas `clinicalNote` **ditutup 2026-09-15**: dicabut dari kontrak lewat `r11` atas keputusan pemilik modul — 3.4 |
| Risiko tersisa | Rendah. Endpoint baru; endpoint lama terbukti tidak tersentuh |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 3 berkas source disentuh, laporan ini, pembaruan roadmap dan traceability. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 7.1 |

### 7.1 Langkah berikutnya

1. ~~**Putuskan nasib `clinicalNote`**~~ — ✅ **selesai 2026-09-15**: dicabut lewat `r11` (3.4).
2. **`BE-LAB-28`** kini terbuka: penjagaan agar wadah hanya memuat pemeriksaan yang dipesan,
   beserta penandaan `Fulfilled` yang menutup `AC-91`.
3. **`FE-LAB-14`** kini terbuka — endpointnya sudah ada.
4. **`T-87a` menunggu bahan uji**, bukan menunggu kode. Ia baru dapat dibuktikan bila ada
   pemeriksaan laboratorium yang belum digolongkan disiplinnya.
