# Laporan Perubahan Backend — `BE-RAD-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-12` |
| Judul | Penanda cito pada pesanan |
| Slice | `S12` — Daftar kerja petugas (prasyarat datanya) |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 5, gelombang `MVP-4` |
| Trace | `FR-RAD-064`, `FR-RAD-065`; `RAD-DEC-013`; `RAD-ARCH-BE-001` bagian 4.1 dan 7; `RAD-ERD-DICT-001` bagian 4; `RAD-API-001` bagian *Rad Order — tambahan* |
| Contract version | `RAD-API-001` rev 8 saat dikerjakan. Diamandemen menjadi **rev 9** oleh task ini, **menunggu konfirmasi pemilik modul** |
| Dependency | **Tidak ada.** Roadmap menyatakan task ini dapat dimajukan ke gelombang mana pun |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 1, berkas diubah **2** (enam berkas), logika bisnis 1, kontrak API 1, database **2**, keamanan/auth 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Repositories/Configurations/`, `Migrations/`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai untuk source dan migration.** Build lulus 0 error; 259 uji radiologi lulus, 14 di antaranya baru; 1.464 uji in-memory dan **481 uji Sqlite** lulus. Migration `AddRadOrderUrgency` **dibuat, tidak dijalankan** — eksekusi database wewenang terpisah yang belum diberikan |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE`. Entri riwayat registry 2026-09-10 menyatakan wewenangnya **mencakup source model persisted Radiologi dan pembuatan migration**; eksekusi database tetap terpisah |
| Keberlakuan | `NEW CODE` — kolom baru pada entity `Rad*` yang sudah dimiliki modul ini |
| QBE ID yang berlaku | `QBE-ENT-002` field dan nullability mengikuti semantik domain; `QBE-ENT-003` kolom yang ditambahkan bukan kebutuhan presentasi, melainkan fakta klinis yang diputuskan dokter; `QBE-CFG-001` configuration menyediakan index; `QBE-SVC-001`; `QBE-DTO-001`; `QBE-VAL-001`; `QBE-LOG-001`; `QBE-NAM-002`; `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | `QBE-API-001`, `QBE-PERM-001` — tidak ada endpoint baru; `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`, tidak ada rename maupun DROP; `QBE-CODE-*`; `QBE-PAGE-001` |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Pesanan radiologi tidak punya cara menyatakan **mendesak**. Seluruh pesanan terlihat sama, dan
daftar kerja petugas hanya dapat mengurutkannya berdasarkan waktu masuk.

> **Akibatnya di lapangan.** Pukul 02.15 dr. Andi memesan CT kepala untuk pasien IGD yang
> kesadarannya menurun. Pesanan itu masuk ke antrean yang sama dengan pemeriksaan rutin yang
> dijadwalkan sejak sore. Petugas radiologi tidak punya satu pun penanda di layarnya yang
> membedakan keduanya.
>
> Yang terjadi kemudian bukan kelalaian: petugas mengerjakan antrean berdasarkan urutan, karena
> memang itu satu-satunya yang terlihat.

Penanda cito adalah prasyarat data bagi daftar kerja (`BE-RAD-13`). Tanpa kolomnya, daftar kerja
tidak punya apa pun untuk didahulukan.

---

## 2. Proses bisnis

**Tujuan.** Dokter pengirim dapat menyatakan sebuah pesanan mendesak, dan pernyataan itu tercatat
siapa yang membuatnya.

### 2.1 Alur, berurutan

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Dokter pengirim | Membuat pesanan dan mencentang cito |
| 2 | Sistem | Menyimpan `IsUrgent = true`, **beserta** siapa yang menandai dan kapan |
| 3 | Petugas Radiologi | Melihat penandanya pada daftar pesanan, tanpa perlu membuka rincian |
| 4 | Daftar kerja per alat | Mendahulukan pesanan bertanda cito — **milik `BE-RAD-13`** |

### 2.2 Tiga kolom, dan mengapa tidak cukup satu

Kolom `IsUrgent` saja sudah cukup untuk mengurutkan daftar kerja. Dua kolom lainnya ada karena
alasan yang berbeda sama sekali:

> **Penanda cito mendahulukan seorang pasien di atas pasien lain yang sudah menunggu lebih
> lama.** Wewenang seperti itu tidak boleh anonim.
>
> Ketika sebuah unit mendapati hampir seluruh pesanannya bertanda cito — dan itu terjadi di
> banyak rumah sakit — pertanyaan pertamanya adalah **siapa yang menandai, dan sejak kapan**.
> Tanpa kedua kolom itu, pertanyaan tersebut tidak punya jawaban, dan penanda cito perlahan
> kehilangan artinya karena semua orang memakainya.

| Kolom | Wajib | Bawaan | Kegunaannya |
| --- | :---: | --- | --- |
| `IsUrgent` | Ya | `false` | Yang dibaca daftar kerja |
| `UrgentMarkedByUserId` | Tidak | kosong | Siapa yang menyatakan mendesak |
| `UrgentMarkedAt` | Tidak | kosong | Kapan dinyatakan |

Jejaknya **hanya distempel ketika penandanya benar-benar dinyalakan**. Menstempelnya pada seluruh
pesanan akan membuat kolom "siapa menandai cito" terisi pada pesanan yang tidak pernah ditandai
cito oleh siapa pun, dan jejak yang selalu terisi berhenti menjadi jejak.

### 2.3 Mengapa field permintaannya boleh kosong

`RadOrder` **bukan tabel baru**. Ia sudah dipakai tiga modul lain hari ini: Rawat Jalan, IGD, dan
Rawat Inap semuanya memesan pemeriksaan radiologi lewat endpoint yang sama.

| Pilihan | Akibatnya |
| --- | --- |
| `IsUrgent` wajib diisi | **Ketiga modul rusak seketika** — permintaan mereka ditolak karena kurang field |
| `IsUrgent` bawaannya `true` | Daftar kerja penuh pesanan yang terlihat mendesak padahal tidak ada yang pernah menyatakannya begitu |
| **`IsUrgent` boleh kosong, bawaannya `false`** | Pemanggil lama berjalan tanpa perubahan apa pun di sisi mereka |

Pilihan ketiga yang dipakai — `FR-RAD-065`. Buktinya bukan hanya uji khusus: **481 uji modul
Rawat Inap yang memanggil endpoint ini seluruhnya tetap lulus tanpa satu pun disunting.**

### 2.4 Pesanan lama menjadi tidak-cito, dijamin migration

Migration menambahkan kolom dengan `defaultValue: false`. Itu yang membuat seluruh baris yang
sudah ada — berapa pun jumlahnya — terbaca sebagai tidak-cito begitu migration dijalankan.

Kalau kolomnya dibuat wajib **tanpa** bawaan, migration akan **gagal** pada tabel yang sudah
berisi data, karena PostgreSQL tidak tahu nilai apa yang harus diisikan pada baris lama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md`, `docs/engineering/` beserta registry | Governance canonical, preflight QBE, dan wewenang pembuatan migration |
| `rules/backend/DATABASE_RULES.md` | Batas wewenang migration versus eksekusi database |
| `rules/backend/` — `TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE` | Aturan operasional task |
| `erd/data-dictionary.md` bagian 4 | Ketiga kolom beserta tipe, kewajiban, bawaan, dan index gabungannya |
| `00-interview-decisions.md` `RAD-DEC-013` beserta kriteria 39–42 | Keputusan asal dan acceptance criteria |
| `04-prd-to-mvp.md` `FR-RAD-064`, `FR-RAD-065` | Contoh berangka |
| `contracts/api-contract.md` bagian *Rad Order — tambahan* | Perubahan endpoint yang sudah dinyatakan aman |
| `Models/RadOrder.cs`, `RadOrderConfiguration.cs`, `RadOrderService.cs` | Yang disentuh |
| `task/report/backend/BE-RAD-07.md` | Cara membuat migration pada repository ini — `--no-build` **tidak** dipakai |
| `Tests/.../RadiologyFilterAndSummaryTests.cs` | Perancah seeding master yang sudah terbukti |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Models/RadOrder.cs` | Tiga kolom: `IsUrgent`, `UrgentMarkedByUserId`, `UrgentMarkedAt` |
| `Repositories/Configurations/.../RadOrderConfiguration.cs` | Index gabungan `ModalityId` + `IsUrgent` + `OrderStatus` |
| `Areas/.../DTOs/RadiologyDtos.cs` | `CreateRadOrderRequest.IsUrgent` (`bool?`); `RadOrderListResponse.IsUrgent`; `RadOrderDetailResponse` bertambah dua kolom jejak |
| `Areas/.../Services/RadOrderService.cs` | Pengisian ketiga kolom saat pesanan dibuat; ketiganya ikut terbawa daftar dan rincian |
| `Migrations/20260911045053_AddRadOrderUrgency.cs` beserta `.Designer.cs` | **Baru.** Dibuat, **belum dijalankan** |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tiga kolom dan satu index bertambah |
| `Tests/.../RadOrderUrgencyTests.cs` | **Baru.** 14 uji |

### 3.3 Urutan kolom index yang tidak boleh ditukar

```csharp
builder.HasIndex(x => new { x.ModalityId, x.IsUrgent, x.OrderStatus });
```

Daftar kerja selalu bertanya dengan urutan yang sama: **"pekerjaan pada alat ini"**, lalu
**"yang cito lebih dulu"**, lalu **"yang statusnya masih berjalan"**. Index dengan urutan itu
terpakai penuh.

Urutan lain — misalnya `IsUrgent` di depan — memaksa database memindai seluruh pesanan bertanda
cito dari **semua** alat, lalu membuang yang bukan miliknya. Pada unit dengan enam alat, itu
berarti memeriksa enam kali lebih banyak baris daripada yang diperlukan.

Urutannya dikunci uji `IndexDaftarKerjaDideklarasikanDenganUrutanYangBenar`, yang memeriksa
posisi tiap kolom, bukan sekadar keberadaan index-nya.

### 3.4 Penanda cito **tidak** disalin ke study

Kriteria 41 berbunyi *"penanda cito ikut terbawa ke seluruh study yang lahir dari pesanan itu"*.
Itu **diturunkan** dari pesanannya lewat relasi yang sudah ada, bukan disalin ke kolom baru pada
`RadStudy`.

Menyalinnya akan menghasilkan dua sumber kebenaran yang dapat berselisih begitu penanda pesanan
diubah — dan `PUT /{id}/urgency` pada `BE-RAD-13` memang akan mengubahnya. Kamus data pun tidak
mencantumkan kolom cito pada `RadStudy`. Dijaga uji `PenandaCitoTidakDitambahkanKeStudy`.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak ada endpoint baru.** Dua perubahan yang sudah dinyatakan aman pada kontrak kini berjalan: `POST /` menerima `IsUrgent`, dan `GET /` beserta `GET /{id}` memuatnya. Ditambah dua field jejak pada rincian — **di luar teks kontrak**, dicatat pada `RAD-API-001` revision 9 beserta alasannya. **Tidak ada pemanggil lama yang rusak** |
| Database | **Tiga kolom dan satu index baru pada `RadOrder`.** Migration `20260911045053_AddRadOrderUrgency` dibuat dan diperiksa, **tidak dijalankan ke database mana pun**. Tidak ada tabel lain yang tersentuh; snapshot bertambah tanpa satu baris pun hilang |
| Keamanan/Auth | Tidak ada string hak akses baru. **Dampak audit positif**: keputusan mendahulukan seorang pasien kini punya pelaku dan waktu yang tercatat. Tidak ada kolom sensitif baru — ketiganya bukan isi klinis |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah maupun mengubah satu pun route. Yang berubah adalah
isi permintaan dan jawaban endpoint `POST /`, `GET /`, dan `GET /{id}` yang sudah ada;
rinciannya pada bagian 3.5 dan `RAD-API-001` revision 9.

---

## 5. Verifikasi

Build, migration, dan test dijalankan **terpisah dan berurutan**.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 1 menit 42 detik | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddRadOrderUrgency` | Berhasil, `Done.` | `PASS` | `Migrations/20260911045053_AddRadOrderUrgency.cs` |
| Warning baru dari berkas radiologi | **Tidak ada satu pun** | `PASS` | Penyaringan warning build atas `RadOrder`, `RadReport`, dan `RadiologyManagement` |
| `dotnet build` project uji in-memory | Berhasil, **0 error** | `PASS` | Keluaran perintah |
| `dotnet build` project uji Sqlite | Berhasil, **0 error, 0 warning** | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadOrderUrgencyTests` | **14 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadiologyManagement` | **259 lulus, 0 gagal**, 14 detik | `PASS` | 245 sebelumnya + 14 baru |
| `dotnet test --no-build` seluruh project in-memory | **1.464 lulus, 0 gagal**, 32 detik | `PASS` | 1.450 sebelumnya + 14 |
| **`dotnet test --no-build` project Sqlite** | **481 lulus, 0 gagal**, 6 menit 7 detik | `PASS` | Termasuk `InpatientSupportingOrderTests` yang memanggil `RadOrderService` dari modul lain |

### 5.1 Bukti per butir yang diminta roadmap

| Yang wajib dibuktikan | Uji | Hasil |
| --- | --- | --- |
| **Pemanggil lama yang tidak mengirim field tetap berhasil** | `FR065_PemanggilYangTidakMengirimPenandaTetapBerhasil`, diperkuat 481 uji Sqlite | `PASS` |
| **Pesanan lama seluruhnya bernilai `IsUrgent = false`** | `PesananLamaTerbacaSebagaiTidakCito` pada model; `MigrationMenambahTigaKolomDenganPenandaBawaanTidakCito` pada migration | `PASS` |
| AC-42 — penanda cito tercatat siapa dan kapan | `AC42_PenandaCitoTercatatSiapaYangMenetapkannyaDanKapan`, `AC42_JejakPenandaanTerbacaPadaRincianPesanan` | `PASS` |
| Pesanan tidak-cito tidak meninggalkan jejak palsu | `PesananYangTidakCitoTidakMeninggalkanJejakPenandaan` | `PASS` |
| Kriteria 40 — penanda terlihat tanpa membuka rincian | `PenandaCitoTerbacaPadaDaftarPesanan` | `PASS` |
| Index daftar kerja ada dengan urutan yang benar | `IndexDaftarKerjaDideklarasikanDenganUrutanYangBenar` | `PASS` |
| **Index yang sudah ada tidak hilang** | `IndexYangSudahAdaTidakHilang` — keempat index lama diperiksa satu per satu | `PASS` |
| **Kolom lama tidak ada yang hilang** | `KolomLamaRadOrderTidakAdaYangHilang` — tiga belas kolom diperiksa | `PASS` |
| Penanda tidak disalin ke study | `PenandaCitoTidakDitambahkanKeStudy` | `PASS` |
| Migration hanya menyentuh `RadOrder` | `MigrationHanyaMenyentuhTabelRadOrder` — dibaca dari berkas migrationnya | `PASS` |
| Migration dapat dimundurkan dengan urutan yang benar | `MigrationDapatDimundurkanDenganUrutanYangBenar` — index dijatuhkan sebelum kolom | `PASS` |
| Snapshot tidak kehilangan apa pun | 238 baris bertambah, **0 baris terhapus** | `PASS` | `git diff --stat` atas snapshot |

Uji manual: `NOT APPLICABLE` — tidak ada endpoint maupun layar baru.

### 5.2 Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| **Migration dapat dijalankan dan dimundurkan sungguhan** | Menjalankannya menuntut database. `Up()` dan `Down()` ada, terbaca benar, terkompilasi, dan urutannya diperiksa uji — tetapi **belum pernah dijalankan**. Sama persis seperti `BE-RAD-01` dan `BE-RAD-07` |
| Perilaku `defaultValue: false` terhadap baris yang benar-benar ada | Yang terbukti adalah migration menuliskannya. Penerapannya pada tabel berisi data menunggu migration dijalankan |
| Index benar-benar dipakai query daftar kerja | Query-nya belum ada — milik `BE-RAD-13`. Yang terbukti adalah index-nya dideklarasikan dengan urutan yang benar |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef database update` ke database mana pun | **Wewenang terpisah dan belum diberikan.** `AGENTS.md`: perubahan model tidak dengan sendirinya memberi wewenang menjalankan migration |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi |
| `--no-build` pada `dotnet ef` | **Sengaja tidak dipakai.** Laporan `BE-RAD-01` mencatat flag itu menghasilkan migration kosong sekaligus meregenerasi snapshot dari assembly lama |
| Analyzer build penuh | Dimatikan atas permintaan pemilik modul. Warning compiler tetap dihitung dan disaring |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-42 — penanda cito tercatat siapa yang menetapkannya dan kapan | **Terpenuhi** | Dua uji AC-42 |
| Test — pesanan lama seluruhnya bernilai `IsUrgent = false` | **Terpenuhi pada model dan migration** | Dua uji; penerapannya pada data sungguhan menunggu migration dijalankan |
| Test — pemanggil lama yang tidak mengirim field tetap berhasil | **Terpenuhi** | Uji khusus, diperkuat 481 uji modul Rawat Inap |
| **DoD — kompatibilitas mundur terbukti; tidak ada pemanggil lama yang rusak** | **Terpenuhi** | 1.464 uji in-memory dan 481 uji Sqlite seluruhnya lulus, tidak satu pun disunting |

**Butir yang belum terpenuhi, disebut apa adanya:**

1. **Migration belum dijalankan ke satu database pun.** Selama itu, ketiga kolom belum ada di
   database mana pun dan `BE-RAD-13` tidak dapat diuji terhadap database sungguhan.
2. **Kriteria 39 dan 41 belum tersentuh** — "pesanan cito di urutan atas" dan "penanda ikut
   terbawa ke study" adalah perilaku daftar kerja, milik `BE-RAD-13`. Task ini menyiapkan
   datanya.
3. **`PUT /{id}/urgency` belum ada** — mengubah penanda setelah pesanan dibuat masih belum
   mungkin. Milik `BE-RAD-13`.
4. **Penghalang `RadReport : ActAsRadiologist` masih terbuka** — tidak berkaitan dengan task ini,
   tetapi masih menunggu keputusan Anda.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi |
| Masalah yang diketahui | `MapDetail` pada `RadOrderService` **tidak memetakan** `InpEpisodeId`, `IsResultFinal`, dan `ResultAvailabilityNote`, padahal ketiganya ada pada `RadOrderListResponse` yang diwarisinya. Cacat ini **sudah ada sebelum task ini** dan tidak diperbaiki di sini karena di luar scope; disebut supaya tidak hilang |
| Risiko tersisa | **Pertama**, migration belum dijalankan. **Kedua**, migration ini menyalin ulang model 1.309 entity ke satu Designer berukuran **4,45 MB** lagi — folder `Migrations` kini **404 MB**; lihat catatan di bawah. **Ketiga**, kolomnya sudah ada tetapi belum ada satu pun jalan mengubah penanda setelah pesanan dibuat |
| Perubahan sampingan | `NONE`. Snapshot berubah karena memang harus, dan perubahannya diperiksa: 238 baris bertambah, **0 terhapus** |
| Interupsi | `NONE`. Pembuatan migration berjalan lebih dari 10 menit dan dipindahkan ke latar belakang oleh harness, lalu selesai dengan `exit code 0`; tidak ada pekerjaan yang hilang |
| Status Git | Lihat bagian 7.1 |
| Langkah berikutnya | `BE-RAD-13` — daftar kerja per alat, yang memakai ketiga kolom ini |

### Catatan yang berulang untuk ketiga kalinya: ukuran folder migration

Migration ini menambah satu berkas Designer berukuran **4,45 MB** — salinan penuh model 1.309
entity, sama seperti yang dilakukan `AddRadReport` pada `BE-RAD-07`. Folder `Migrations` kini
**404 MB**.

Setiap migration berikutnya akan melakukan hal yang sama, dan biayanya bukan hanya ruang disk:
`dotnet ef migrations add` pada task ini berjalan **lebih dari sepuluh menit**, hampir
seluruhnya untuk menulis ulang salinan model itu. Usulan **squash migration** yang dilaporkan
`BE-RAD-07` semakin layak dikerjakan.

### 7.1 Status Git pada akhir pekerjaan

Berkas hasil task ini:

```text
 M Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs
 M Repositories/Configurations/HealthServices/RadiologyManagement/RadOrderConfiguration.cs
 M Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M docs/module-blueprints/radiologi/contracts/api-contract.md
 M docs/module-blueprints/radiologi/roadmap/backend-roadmap.md
 M docs/module-blueprints/radiologi/roadmap/requirement-traceability.md
?? Migrations/20260911045053_AddRadOrderUrgency.cs
?? Migrations/20260911045053_AddRadOrderUrgency.Designer.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadOrderUrgencyTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-12.md
```

`RadiologyDtos.cs` dan `ApplicationDbContextModelSnapshot.cs` tampil ` M` bersama perubahan
`BE-RAD-04` sampai `BE-RAD-07` yang belum di-commit.

Berkas lain yang tampak pada `git status` — Laboratorium, laporan `BE-RAD-04` sampai `BE-RAD-11`,
migration `AddRadReport`, serta seluruh berkas hasil bacaan — sudah ada sebelum task ini dimulai
dan **bukan** hasil pekerjaan ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
