# Laporan Perubahan Backend — `BE-LAB-25`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-25` |
| Judul | Daftar pantau pemakaian `Lainnya` |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.2`; `LAB-DEC-040` butir 4-5, BR-35; `AC-60`; `T-60d` |
| Contract version | `LAB-API-v1` `r7` `GET /lab-specimen-types/other-usage` — `approved`, disetujui pemilik modul 2026-09-14 |
| Dependency | `BE-LAB-21` ✅ **`SELESAI` 2026-09-15** — kolom `SpecimenTypeOtherNote` ada di source **dan di database** |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — endpoint berdiri, `AC-60` terpenuhi, dan **verifikasi proses bisnisnya dijalankan terhadap database sebenarnya**. Nol tabel, nol migration, nol baris uji tertinggal |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `NEW CODE` untuk satu endpoint baca, dua DTO, dan satu method service |
| Status gerbang | `QBE-MOD-002` dan `QBE-MOD-003` **tidak berlaku** — nol entity, nol migration |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-PAGE-001`, `QBE-OPT-001` |

---

## 1. Masalah yang diperbaiki

`BE-LAB-20` membuat baris **`Lainnya`** sebagai jalan keluar: sampel yang jenisnya belum
terdaftar tetap dapat diterima, asalkan petugas menuliskan keterangannya. `BE-LAB-21` membuat
keterangan itu benar-benar tersimpan.

**Tetapi sampai task ini, tidak seorang pun dapat membacanya kembali.**

Akibatnya jalan keluar itu berubah menjadi tempat pembuangan. Keterangan menumpuk tanpa pernah
ditengok; "cairan kista" bisa muncul dua puluh kali dalam tiga bulan tanpa satu orang pun
menyadarinya; dan daftar jenis specimen **tidak pernah tumbuh** walaupun kenyataannya sudah
lama berubah.

Itu kegagalan yang tidak menimbulkan satu pun pesan kesalahan. Sistem tetap berjalan, sampel
tetap diterima, laporan tetap terbit — hanya saja mutunya menurun pelan-pelan, dan penurunannya
tidak terlihat sampai ada yang mencarinya.

---

## 2. Proses bisnis

**Pelaku.** Kepala instalasi laboratorium.
**Pemicu.** Peninjauan berkala atas kosakata jenis specimen.

**Langkah:**

1. Kepala instalasi membuka daftar pantau `GET /lab-specimen-types/other-usage`.
2. Ia melihat setiap keterangan `Lainnya` yang pernah ditulis, **diurutkan dari yang paling
   sering**, beserta jumlah pemakaian, kapan pertama dipakai, dan kapan terakhir dipakai.
3. Keterangan yang sering berulang ia naikkan menjadi **jenis specimen tetap** lewat
   `POST /lab-specimen-types`.
4. Sejak itu petugas memilihnya dari daftar, dan jalan keluar `Lainnya` kembali menjadi
   pengecualian, bukan kebiasaan.

**Contoh berangka, persis butir Verifikasi roadmap.** Tiga wadah dicatat dengan keterangan
`cairan kista`, masing-masing pukul 04.47, 05.47, dan 06.47. Daftar pantau menampilkannya
sebagai **satu baris**:

| Keterangan | Jumlah | Pertama dipakai | Terakhir dipakai |
| --- | ---: | --- | --- |
| `cairan kista` | **3** | 12 Sep 04.47 | 12 Sep 06.47 |

**Jalur yang paling mudah salah dirancang — dan sengaja dibiarkan apa adanya.** Bila pada hari
yang sama ada satu wadah berketerangan `Cairan Kista` dengan huruf besar, ia muncul sebagai
**baris tersendiri**:

| Keterangan | Jumlah | Terakhir dipakai |
| --- | ---: | --- |
| `cairan kista` | 3 | 12 Sep 06.47 |
| `Cairan Kista` | 1 | 12 Sep 09.47 |

Keterangan **tidak** dinormalkan, dan itu keputusan, bukan kelalaian. Justru dengan melihat
kedua ejaan berdampingan kepala instalasi menyimpulkan keduanya satu hal, lalu menaikkannya
menjadi satu jenis tetap. Bila sistem menggabungkannya diam-diam, keragaman ejaan yang menjadi
**alasan layar ini dibuat** akan tersembunyi — dan `LAB-DEC-040` kehilangan maksudnya.

**Jalur tidak normal:**

| Kejadian | Yang terjadi |
| --- | --- |
| Rentang tanggal tidak dikirim | Dipakai **90 hari terakhir**. Rentang panjang disengaja: yang dicari adalah pengulangan, dan pengulangan tidak terlihat pada jendela satu minggu |
| Tanggal awal melewati tanggal akhir | Ditolak `400` — "Tanggal awal tidak boleh melewati tanggal akhir.", mengikuti pola `GET /lab-specimens/summary` |
| Belum ada satu pun pemakaian `Lainnya` | Daftar kosong beserta `TotalData = 0`. Bukan kesalahan — itu justru keadaan sehat |
| Wadahnya kemudian **ditolak** atau **dibatalkan** | **Tetap dihitung.** Yang ditanyakan adalah seberapa sering keterangan itu dipakai, bukan berapa sampel yang lolos; jenis yang belum terdaftar tetap perlu didaftarkan walaupun sampelnya kebetulan ditolak |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 6b | Cakupan, DoD, dan butir Verifikasi |
| `contracts/api-contract.md` grup Lab Specimen Type | Bentuk request, response, hak akses, dan nama DTO yang dikunci kontrak |
| `testing/acceptance-test-matrix.md` `T-60d` | Skenario yang harus dibuktikan |
| `00-interview-decisions.md` `AC-60`, BR-35 | Apa yang sebenarnya dijanjikan kepada kepala instalasi |
| `Services/LabSpecimenTypeService.cs` | Pola paging, pencarian, dan `Normalize` yang sudah ada |
| `Controllers/LabSpecimenTypeController.cs` | Pola `[AccessAction]`/`[AccessPermission]` dan pembungkus respons |
| `Services/LabSpecimenService.cs` `GetSummaryAsync` | Definisi waktu efektif wadah yang ditetapkan `BE-LAB-22` |
| `task/report/backend/BE-LAB-20.md` | Delta kontrak yang sudah dilaporkan untuk grup ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenTypeDtos.cs` | Dua DTO baru: `LabSpecimenOtherUsageQuery` dan `LabSpecimenOtherUsageResponse` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenTypeService.cs` | Satu method `GetOtherUsageAsync` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenTypeController.cs` | Satu endpoint `GET /other-usage` |
| `docs/module-blueprints/laboratorium/contracts/api-contract.md` | Status `GET /other-usage` dikoreksi dari `Rencana (belum tersedia)` menjadi **Tersedia** |

**Nol entity, nol configuration, nol migration, nol tabel.** Tidak satu pun berkas lama berubah
perilakunya; seluruh perubahan bersifat tambahan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baca yang **sudah tercantum pada `r7`** kini benar-benar berdiri. Tidak ada ruas, endpoint, atau nilai enum lama yang berubah. **Tidak ada revisi kontrak baru** — hanya kolom Status yang dikoreksi menjadi Tersedia |
| Database | **`NOT APPLICABLE`.** Nol schema baru, nol migration, nol tabel ringkasan. Rekapnya diturunkan dari `LabSpecimen` yang sudah ada |
| Keamanan/Auth | Memakai `LabSpecimenType : Read` yang sudah terdaftar sejak `BE-LAB-20`. **Tidak ada permission baru.** Hak membaca daftar pantau sama dengan hak membaca daftar jenis — keduanya bacaan kepala instalasi atas data induk yang sama |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Specimen Type

Base URL: `api/v1/health-services/laboratory-management/lab-specimen-types`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/other-usage` | Daftar pantau pemakaian `Lainnya`: keterangan, jumlah pemakaian, pemakaian pertama dan terakhir | `LabSpecimenType : Read` |

**Penyaring — `LabSpecimenOtherUsageQuery`:**

| Ruas | Tipe | Bawaan | Catatan |
| --- | --- | --- | --- |
| `startDate` | `datetime` | `endDate` dikurangi 90 hari | — |
| `endDate` | `datetime` | sekarang | — |
| `search` | `string` | — | Pencarian bebas pada keterangannya, tidak peka huruf besar-kecil |
| `pageNumber` | `int` | `1` | — |
| `pageSize` | `int` | `20` | Dibatasi 1..100 |

**Respons — `ApiResponse<PagedResult<LabSpecimenOtherUsageResponse>>`:**

| Ruas | Tipe | Isi |
| --- | --- | --- |
| `otherNote` | `string` | Keterangan **apa adanya**, tidak dinormalkan |
| `usageCount` | `int` | Berapa kali dipakai pada rentang yang diminta |
| `lastUsedAt` | `datetime` | Pemakaian paling akhir |
| `firstUsedAt` | `datetime` | Pemakaian paling awal |

`firstUsedAt` **tidak diminta kontrak** dan ditambahkan sebagai ruas aditif. Alasannya satu
pertanyaan yang pasti muncul di layar itu: keterangan berjumlah 12 yang tersebar tiga bulan
berbeda maknanya dari yang terkumpul dalam satu minggu. Tanpa ruas ini, keduanya terlihat sama.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`, prefix `Lab`, nol entity baru | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 |
| Review diff/scope | 3 berkas source disentuh secara aditif, nol migration | `PASS` | Bagian 3.2 |
| `dotnet build -p:RunAnalyzers=False` | **0 Error, 191 Warning** | `PASS` | Nol warning berasal dari berkas task ini |
| Verifikasi kontrak API terhadap `r7` | Path, verb, hak akses, dan bentuk respons cocok | `PASS` | Bagian 4 |
| **Terjemahan kueri ke SQL** | **Satu pernyataan `GROUP BY` penuh di database** — nol evaluasi sisi klien | `PASS` | Lihat 5.1 |
| **`T-60d`** — tiga wadah `cairan kista` menjadi satu baris berjumlah tiga | **LULUS**, beserta waktu pemakaian terakhirnya | `PASS` | Lihat 5.2 |
| **Keterangan tidak digabung diam-diam** | **LULUS** — `Cairan Kista` muncul sebagai baris tersendiri | `PASS` | Lihat 5.2 |
| Kebersihan data uji | **Nol baris tertinggal** | `PASS` | Lihat 5.2 |

### 5.1 Kueri benar-benar berjalan di database, bukan di memori

Ini pemeriksaan yang paling penting pada task ini, dan **`dotnet build` tidak dapat
menangkapnya**. Pengelompokan LINQ yang tidak dapat diterjemahkan EF akan tetap berhasil
dikompilasi, lalu gagal saat dipanggil — atau lebih buruk, menarik **seluruh** tabel
`LabSpecimen` ke memori lalu mengelompokkannya di sana.

`ToQueryString()` atas kueri yang sama persis dengan isi `GetOtherUsageAsync` menghasilkan:

```sql
SELECT l."SpecimenTypeOtherNote" AS "OtherNote",
       count(*)::int AS "UsageCount",
       max(COALESCE(l."PhysicallyReceivedAt", l."CreateDateTime")) AS "LastUsedAt",
       min(COALESCE(l."PhysicallyReceivedAt", l."CreateDateTime")) AS "FirstUsedAt"
FROM public."LabSpecimen" AS l
LEFT JOIN public."LabSpecimenType" AS l0 ON l."SpecimenTypeId" = l0."Id"
WHERE NOT (l."IsDelete")
  AND l."SpecimenTypeOtherNote" IS NOT NULL
  AND l0."Id" IS NOT NULL
  AND l0."IsOtherBucket"
  AND COALESCE(l."PhysicallyReceivedAt", l."CreateDateTime") >= @__awal_0
  AND COALESCE(l."PhysicallyReceivedAt", l."CreateDateTime") <= @__akhir_1
GROUP BY l."SpecimenTypeOtherNote"
ORDER BY count(*)::int DESC,
         max(COALESCE(l."PhysicallyReceivedAt", l."CreateDateTime")) DESC,
         l."SpecimenTypeOtherNote"
```

Pengelompokan, pencacahan, pengambilan nilai terbesar dan terkecil, **serta pengurutannya**
seluruhnya dikerjakan PostgreSQL. Paging `Skip`/`Take` menyusul sebagai `OFFSET`/`LIMIT`.

**`COALESCE(PhysicallyReceivedAt, CreateDateTime)` sengaja sama persis** dengan penyaring
`GET /lab-specimens/summary` yang ditetapkan `BE-LAB-22`. Dua layar tentang wadah yang sama
tidak boleh memakai batas hari yang berbeda — itu jenis ketidakcocokan yang baru ketahuan
setelah dua orang membandingkan angkanya di rapat.

### 5.2 `T-60d` — dijalankan terhadap database sebenarnya

Dijalankan pada `QuilvianNewDevYoga` di dalam transaksi yang **selalu di-`ROLLBACK`**. Empat
wadah disisipkan: tiga berketerangan `cairan kista` pada jam berbeda, satu berketerangan
`Cairan Kista`.

Hasil yang benar-benar dikembalikan kueri:

```
cairan kista | jumlah=3 | pertama=2026-09-12 04:47:44Z | terakhir=2026-09-12 06:47:44Z
Cairan Kista | jumlah=1 | pertama=2026-09-12 09:47:44Z | terakhir=2026-09-12 09:47:44Z
```

| Yang dibuktikan | Hasil |
| --- | --- |
| Tiga wadah berketerangan sama menjadi **satu baris berjumlah tiga** | **LULUS** — persis butir Verifikasi roadmap |
| Waktu pemakaian terakhirnya benar | **LULUS** — `06:47`, yaitu wadah ketiga, bukan yang pertama |
| Ejaan berbeda **tidak** digabung diam-diam | **LULUS** — `Cairan Kista` berdiri sebagai baris tersendiri |
| Urutan paling sering lebih dulu | **LULUS** — jumlah 3 mendahului jumlah 1 |
| Kebersihan | **LULUS** — sesudah `ROLLBACK`, wadah berketerangan `Lainnya` yang tersisa = **0** |

**Rekapnya berubah seketika** — butir DoD terakhir. Terbukti dengan sendirinya oleh urutan di
atas: sebelum transaksi, kueri mengembalikan **nol baris**; setelah keempat wadah tersimpan,
kueri yang sama langsung mengembalikan kedua baris tanpa satu pun langkah penyegaran. Tidak ada
tabel ringkasan yang perlu dihitung ulang, karena memang tidak ada.

### 5.3 Alat verifikasinya

Satu program sekali pakai di **scratchpad sesi, di luar repository backend**, yang merujuk
project aplikasi agar dapat memakai `ApplicationDbContext` dan DTO yang sebenarnya — sehingga
yang diuji adalah expression tree yang sama, bukan tiruannya. Connection string dibaca dari
berkas konfigurasi; kata sandinya tidak pernah melewati baris perintah.

`rules/backend/TEST_POLICY.md` bagian 3 melarang runner uji **di dalam project aplikasi**. Alat
ini tidak menyentuh repository sama sekali.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Endpoint menjawab sesuai `r7` | **✅ Terpenuhi** | Path, verb, hak akses, dan bentuk respons — bagian 4 |
| **Tidak ada tabel ringkasan yang dibuat** | **✅ Terpenuhi** | Nol entity, nol configuration, nol migration. SQL pada 5.1 membaca `LabSpecimen` langsung |
| Rekapnya berubah seketika ketika wadah baru dicatat | **✅ Terbukti** | 5.2 — nol baris sebelum transaksi, dua baris sesudah penyimpanan, tanpa langkah penyegaran |

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-60` — setiap pemakaian `Lainnya` terlihat beserta keterangan dan jumlah pemakaiannya | **✅ Terpenuhi dan terbukti** | 5.2. Ini AC terakhir `LAB-DEC-040` yang masih terbuka; `BE-LAB-20` menandainya "belum" dan menunjuk task ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini. 191 warning repository seluruhnya peninggalan modul lain |
| Masalah yang diketahui | `NONE` untuk task ini. Penahan gelombang yang masih terbuka bukan milik task ini — lihat 7.1 |
| Risiko tersisa | Rendah. Endpoint baca-saja, nol tulis, nol schema. Bila `LabSpecimen` kelak tumbuh sangat besar, pengelompokan ini akan memindai kolom `SpecimenTypeOtherNote` tanpa index khusus — dicatat pada 7.2 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tiga berkas source disentuh, satu berkas kontrak dikoreksi, laporan ini ditambahkan, roadmap dan traceability diperbarui. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 7.1 |

### 7.1 Keadaan gelombang `MVP-5a` sesudah task ini

**Keenam task backend `MVP-5a` selesai atau ditutup.** Yang tersisa bukan pekerjaan backend:

| Butir | Milik siapa |
| --- | --- |
| **`LAB-CONFLICT-006`** — `VAL-59` tidak dapat dilaksanakan, separuh `AC-66` | Keputusan pemilik modul. Tiga pilihan pada [`BE-LAB-22.md`](BE-LAB-22.md) §6 |
| **`DATA-MST-MEASUREMENT`** — tiga satuan `µL`, `blok`, `slide` | `master-data`. Permintaannya perlu ditulis ulang — [`BE-LAB-21.md`](BE-LAB-21.md) §7.1 |
| **`FE-LAB-10`, `FE-LAB-11`, `FE-LAB-12`** | Frontend. `FE-LAB-11` **mendesak** — layar wadah menjawab `422` sejak migration `BE-LAB-21` diterapkan |
| **`LAB-REQ-005`** — `FR-11.9` dan `FR-11.10` | Pemilik `master-data`, `registration-management`, `billing-kasir` |

**`FE-LAB-10` kini tidak lagi tertahan.** Roadmap frontend mencatatnya *"Menunggu `BE-LAB-20`
dan `BE-LAB-25`"*; keduanya selesai, sehingga layar pengelolaan jenis specimen beserta daftar
pantau `Lainnya` dapat dikerjakan.

### 7.2 Satu catatan kinerja yang dilaporkan, bukan diperbaiki sekarang

Pengelompokan ini menyaring `SpecimenTypeOtherNote IS NOT NULL` lalu mengelompokkan menurutnya.
`BE-LAB-21` memasang index atas `SpecimenTypeId` dan `VolumeUnitId`, **bukan** atas kolom
keterangan — dan memang tidak diminta kamus data.

Pada ukuran data sekarang hal itu tidak berarti apa-apa: hanya wadah berjenis `Lainnya` yang
lolos penyaring, dan menurut rancangannya jumlah itu memang **harus kecil** — kalau ia
membesar, itu justru pertanda daftar jenis specimen perlu ditambah, yang persis merupakan
pekerjaan layar ini.

Index parsial atas `SpecimenTypeOtherNote` baru pantas dipertimbangkan bila daftar pantau ini
kelak terasa lambat. Menambahkannya sekarang berarti membayar biaya tulis pada setiap
pencatatan wadah untuk persoalan yang belum pernah terjadi. Dicatat sebagai pertimbangan, bukan
utang.
