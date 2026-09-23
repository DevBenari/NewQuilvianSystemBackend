# Laporan Perubahan Backend — `BE-LAB-36`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-36` |
| Judul | Kolom nomor order beserta layanan alokasinya |
| Slice | `MVP-5f`, `EPIC-LAB-11` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6g |
| Trace | `LAB-DEC-072` (`approved` 2026-09-16); menutup `LAB-OPEN-033`; `REC3-NEW-001` |
| Contract version | **Nol perubahan kontrak.** Usul `LAB-API-v1` `r16` ditulis pada bagian 7 dan **belum disetujui** |
| Dependency | **Nol.** Tidak menunggu task mana pun |
| Klasifikasi | `MEDIUM` — satu kolom `NOT NULL` pada tabel berisi, satu layanan baru, satu index unik, satu migration bersunting tangan, dan dua jalur tulis yang dibungkus transaksi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `13665452` (perubahan task ini belum di-stage maupun di-commit) |
| Tanggal | 2026-09-17 |
| Status | **`SELESAI`** — seluruh butir DoD terpenuhi dan terbukti terhadap database. **Satu klaim perancangan dikoreksi** sesudah diuji; lihat bagian 5.3 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement` / Laboratory |
| Pemilik / prefix pada registry | `Lab`, status **`ACTIVE`** sejak 2026-09-02 lewat `LAB-REQ-002` |
| Keberlakuan | **`NEW CODE`** untuk `LabOrderNumberService`; **`TOUCHED LEGACY`** untuk `LabOrder`, `LabOrderConfiguration`, dan kedua jalur pembuatan pada `LabOrderService` |
| QBE yang berlaku | `QBE-CODE-001`..`005`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-SVC-001` |

### Kepatuhan `QBE-CODE`, ditulis butir demi butir

Task ini mengalokasikan nomor bisnis, sehingga keluarga aturan ini berlaku penuh — berbeda dari
`BE-LAB-33`..`35` yang mencatatnya sebagai tidak berlaku.

| Aturan | Bunyi | Keadaan |
| --- | --- | --- |
| `QBE-CODE-001` | Service memiliki kebutuhan dan format kode; alokasinya deterministik dan aman di database | **Patuh.** `LabOrderNumberService` memiliki format, awalan, dan panjangnya sebagai konstanta |
| `QBE-CODE-002` | Controller **tidak** mengalokasikan nomor bisnis | **Patuh.** Alokasi hanya dipanggil `LabOrderService`; `LabOrderController` nol menyentuhnya |
| `QBE-CODE-003` | **`Max/Last + 1` tanpa proteksi dilarang** sebagai satu-satunya alokator | **Patuh, dan ini butir yang paling perlu dibaca.** `MAX + 1` memang dipakai, tetapi **tidak tanpa proteksi**: ia berjalan di bawah `pg_advisory_xact_lock` — kunci **tingkat database**, bukan process-local — di dalam transaksi eksplisit, dan **bukan satu-satunya** penjaga, karena index unik menolak duplikat. Yang dilarang aturan ini adalah `Max + 1` telanjang |
| `QBE-CODE-004` | Kode bisnis unik memiliki unique constraint/index sesuai scope-nya | **Patuh.** `IX_LabOrder_OrderNumber` unik, dibuktikan menolak `23505` |
| `QBE-CODE-005` | Modul memiliki format, prefix, reset, dan scope kodenya sendiri | **Patuh.** Format `LAB-RSMMC-000001`, scope satu rumah sakit, **nol reset** — nomor berjalan terus, tidak berulang per tahun |
| `QBE-CODE-006` | Provider bersama mendukung alokasi atomik ber-scope yang durabel beserta observability retry | **Tidak terpenuhi, dan disebut apa adanya.** Aplikasi ini **tidak punya provider alokasi bersama**; penomoran dikerjakan per modul, dan `PatientEncounterNumberService` adalah preseden yang sudah berjalan. Task ini mengikuti pola yang ada, bukan mendirikan provider bersama — pekerjaan itu melampaui `LAB-DEC-072`. **Nol retry** dipasang; lihat bagian 8 |

---

## 1. Masalah yang diperbaiki

`LabOrder` **nol punya nomor yang dapat disebut manusia**. Yang ada hanyalah `Id` berbentuk GUID,
dan GUID tidak dapat dibacakan lewat telepon, tidak dapat dicetak pada amplop dengan enak dibaca,
dan tidak dapat dipakai pasien menanyakan pesanannya di loket.

Requirement Menu Hasil (`Laboratorium (4).md` bagian 5.3 dan 5.5) menuntut dua hal yang keduanya
berdiri di atas nomor itu: kolom **`No. Order`** pada datatable, dan **barcode Label Lab yang
dibangkitkan dari nomor order**. Rekonsiliasi bukti putaran 3 mencatatnya sebagai `REC3-NEW-001`
dan membuktikan ketiadaannya: penelusuran `OrderNumber` pada seluruh area `LaboratoryManagement`
menghasilkan **nol kemunculan**. Yang ada hanyalah barcode **wadah** (`LabSpecimen.SpecimenBarcode`)
berpola `LSP-{Guid:N}` — dibaca mesin, bukan manusia.

`LAB-DEC-072` menutup pertanyaannya pada 2026-09-16, tetapi **sengaja tidak langsung diturunkan
menjadi task**: rekonsiliasi menulis bahwa perancangannya belum ada, dan bahwa meniru pola
acuannya tanpa meniru kelemahannya adalah pekerjaan perancangan. Perancangan itu dikerjakan
2026-09-17 dan tertulis pada roadmap bagian 6g.

---

## 2. Proses bisnis

Dari sisi orang yang memakainya:

1. Petugas membuat pesanan laboratorium — lewat layar pesanan, atau lewat layar pendaftaran yang
   memesan beberapa pemeriksaan sekaligus.
2. Sistem memberi pesanan itu nomor berbentuk **`LAB-RSMMC-000009`** — awalan tetap, lalu nomor
   urut berpadding enam digit.
3. Nomor itu **melekat selamanya**. Ia tidak berubah ketika pesanan dikonfirmasi, diproses,
   ditahan, maupun dibatalkan.
4. Bila pesanan memuat pemeriksaan lintas disiplin, sistem memecahnya menjadi beberapa pesanan —
   dan **masing-masing memperoleh nomornya sendiri yang berbeda**.

### 2.1 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Dua petugas membuat pesanan pada saat yang sama | Kunci advisory membuat keduanya mengantre; masing-masing memperoleh nomor berbeda |
| Kunci gagal menjaga karena sebab apa pun | Index unik menolak yang kedua; permintaannya gagal secara **terlihat**, bukan menghasilkan dua pesanan bernomor sama |
| Satu pemeriksaan pada permintaan jamak ditolak | Nol pesanan terbentuk, dan **nol nomor terpakai** — seluruh transaksi dibatalkan |
| Pesanan dibatalkan | Nomornya tetap melekat. Ia tidak dikembalikan ke kumpulan nomor bebas |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../LaboratoryManagement/Services/LabOrderNumberService.cs` | **Baru.** Layanan alokasi beserta seluruh keputusan perancangannya yang ditulis sebagai dokumentasi |
| `Areas/.../LaboratoryManagement/Models/LabOrder.cs` | **+1 kolom** `OrderNumber`, wajib, maksimal 32 karakter |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs` | Kolomnya wajib, panjang dibatasi, **`PropertySaveBehavior.Throw`**, dan satu index unik |
| `Areas/.../LaboratoryManagement/Services/LabOrderService.cs` | Layanan alokasi di-inject; **kedua** jalur pembuatan dibungkus transaksi eksplisit dan mengalokasikan nomor; `orderNumber` masuk ke muatan log |
| `Migrations/20260917023651_AddLabOrderNumber.cs` | **Baru, disunting tangan.** Lihat 3.3 |
| `Program.cs` | **+1 baris** registrasi DI |

### 3.2 Empat keputusan implementasi yang pantas dibaca ulang

**1. Enam digit, bukan lima.** Pola acuannya memakai lima — cukup untuk 99.999 kunjungan. Satu
kunjungan dapat melahirkan beberapa pesanan sejak `BR-47` memecah per disiplin, sehingga pesanan
bertambah jauh lebih cepat daripada kunjungan. Lima digit adalah utang yang jatuh temponya tidak
terlihat sampai ia jatuh.

**2. Bagian angkanya dibandingkan sebagai angka, bukan teks.** Perbandingan teks lebih murah dan
tetap benar selama lebarnya seragam — tetapi ia **pecah diam-diam pada digit ketujuh**:
`LAB-RSMMC-1000000` berurutan **sebelum** `LAB-RSMMC-999999` secara leksikografis, sehingga nomor
berikutnya akan mundur dan bertabrakan. Kegagalannya tidak menimbulkan galat pada hari ia terjadi;
ia hanya mulai memberi nomor yang salah.

**3. Alokasi berblok, dan ini bukan optimasi.** `POST /lab-orders/by-examinations` membentuk
beberapa pesanan dalam satu `SaveChangesAsync`. Entity yang belum tersimpan **tidak terlihat** oleh
kueri SQL mentah, sehingga alokasi satu per satu di dalam transaksi yang sama akan mengembalikan
**nomor yang sama berulang kali**, lalu ditolak index unik, dan **seluruh permintaan gagal**.
Kegagalannya hanya muncul ketika pasien memesan pemeriksaan lintas disiplin — bukan pada pemakaian
biasa, sehingga ia mudah lolos dari pengujian yang memesan satu disiplin.

**4. Transaksi eksplisit, karena tanpanya kuncinya tidak menjaga apa pun.**
`pg_advisory_xact_lock` dilepas ketika transaksi berakhir. Dipanggil di luar transaksi eksplisit,
ia memperoleh dan melepas kuncinya **seketika**. `LabOrderService` tidak punya transaksi eksplisit
pada kedua jalur pembuatannya, sehingga keduanya wajib dibungkus. Pemanggil pola acuannya sudah
benar — `PatientEncounterController` membungkusnya sejak semula.

### 3.3 Migration disunting tangan, dan bentuk bawaannya pasti gagal

EF menghasilkan satu `AddColumn` ber-`nullable: false` dengan `defaultValue: ""`, lalu satu index
unik. Pada tabel yang sudah berisi, urutan itu **pasti gagal**: seluruh baris lama memperoleh nilai
yang sama — string kosong — dan index unik menolaknya pada baris kedua.

Urutan yang dipakai: kolomnya lahir **boleh kosong** → baris lama diisi nomor berurutan menurut
`CreateDateTime` dengan `Id` sebagai pemecah seri → kolomnya dijadikan wajib → index dipasang.

Pemecah seri itu bukan kehati-hatian berlebih: tiga dari delapan pesanan pada database uji
ber-`CreateDateTime` **identik**, karena ketiganya lahir dari satu panggilan `by-examinations`.
Tanpa pemecah seri, urutannya tidak deterministik dan hasil `Down` lalu `Up` tidak akan sama.

### 3.4 Dampak kontrak API, database, dan keamanan

| Hal | Dampak |
| --- | --- |
| Kontrak API | **Nol.** Tidak satu pun DTO respons memuat `orderNumber` — disengaja; lihat bagian 7 |
| Database | **Satu kolom, satu index unik, satu migration.** Diterapkan ke `QuilvianNewDevYoga` |
| Keamanan | **Nol permission baru.** Kedua endpoint pembuatan sudah dijaga `LabOrder : Create` |
| Perilaku endpoint lama | **Nol berubah** dari sisi pemanggil — bentuk permintaan dan jawabannya sama persis |

---

## 4. Endpoint yang berubah

Nol endpoint ditambah, dihapus, atau berubah bentuknya. Dua endpoint berubah **perilaku
internalnya** tanpa berubah kontraknya.

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Yang berubah | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/laboratory-management/lab-orders` | Mengalokasikan satu nomor di dalam transaksi eksplisit | `LabOrder : Create` |
| `POST` | `/v1/health-services/laboratory-management/lab-orders/by-examinations` | Mengalokasikan **satu blok** nomor sebanyak pesanan yang akan dibentuk | `LabOrder : Create` |

---

## 5. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build -p:RunAnalyzers=False` | **0 error.** Nol warning baru — diperiksa dengan menyaring keluaran build atas keempat berkas yang disentuh, dan hasilnya kosong |
| `dotnet ef migrations list` | **Tepat satu** migration `Pending` sebelum penerapan, sehingga nol migration modul lain ikut terbawa — pelajaran `BE-LAB-26` dipakai sebagai pemeriksaan **sebelum** eksekusi |

### 5.1 Sembilan pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga`

| # | Pemeriksaan | Hasil |
| ---: | --- | --- |
| `T1` | Keadaan sebelum: jumlah pesanan dan keberadaan kolom | **8 pesanan**, 0 terhapus; kolom `OrderNumber` **belum ada** |
| `T2` | Bentuk kolom sesudah migration | `is_nullable = NO`, `character varying(32)` — **`PASS`** |
| `T3` | Pengisian baris lama | Kedelapan baris memperoleh `LAB-RSMMC-000001`..`000008` **urut menurut `CreateDateTime`** — **`PASS`** |
| `T4` | Index unik berdiri | `CREATE UNIQUE INDEX "IX_LabOrder_OrderNumber" … USING btree ("OrderNumber")` — **`PASS`** |
| `T5` | Duplikat ditolak | `23505 duplicate key value violates unique constraint "IX_LabOrder_OrderNumber"` — **`PASS`** |
| `T6` | Keadaan sesudah uji duplikat | 8 baris, 8 nomor unik, **utuh** — transaksinya benar-benar dibatalkan |
| `T7` | Jalur `Down` lalu `Up` | Kolom hilang lalu kembali; **kedelapan pesanan utuh** di antara keduanya; penomoran sesudah `Up` kedua **identik** dengan yang pertama — **`PASS`** |
| `T8` | Alokasi pesanan tunggal | `POST /lab-orders` → `HTTP 201`, dan barisnya bernomor **`LAB-RSMMC-000009`** — tepat berikutnya |
| `T9` | **Alokasi berblok** | `POST /by-examinations` dengan 3 pemeriksaan lintas 3 disiplin → `HTTP 201`, `"3 pesanan … berhasil dibuat"`, dan ketiganya bernomor **`000010`, `000011`, `000012`** — tiga nomor **berbeda dan berurutan**. Total 12 baris, 12 nomor unik — **`PASS`** |

**`T7` pantas dibaca dua kali.** Yang dibuktikan bukan sekadar bahwa migration dapat dibalik,
melainkan bahwa pengisiannya **deterministik**: menjalankan `Down` lalu `Up` menghasilkan
penomoran yang sama persis. Tanpa pemecah seri `Id`, ketiga baris berwaktu identik akan memperoleh
nomor yang berbeda pada setiap penjalanan.

**`T9` adalah pemeriksaan yang dirancang agar gagal bila implementasinya salah.** Alokasi per
pesanan — bentuk yang paling wajar ditulis orang — akan memberi nomor yang sama tiga kali, lalu
ditolak index unik, dan seluruh permintaan gagal dengan `23505`.

### 5.2 Kebersihan database

Empat pesanan uji (`000009`..`000012`) dan satu pesanan uji tambahan dihapus beserta jejak
transisinya. Keadaan akhir diperiksa **dari koneksi baru**:

```
total | terkecil          | terbesar          | unik
8     | LAB-RSMMC-000001  | LAB-RSMMC-000008  | 8
```

**Kembali persis seperti sebelum task ini dimulai.** Nol baris uji tersisa.

### 5.3 Satu klaim perancangan dikoreksi sesudah diuji

Perancangan awal pada roadmap bagian 6g menyatakan nomor **"tidak pernah dipakai ulang"**.
Pernyataan itu **terlalu kuat**, dan pengujian membuktikannya.

Sesudah `000009`..`000012` dihapus **secara fisik** lewat SQL, pesanan berikutnya memperoleh
**`LAB-RSMMC-000009` kembali** — bukan `000013`. Sebabnya lurus: `MAX + 1` membaca nilai tertinggi
yang **masih ada di tabel**, dan penghapusan fisik atas baris tertinggi menurunkan nilai itu.

Yang benar, dan kini tertulis di kode maupun roadmap:

> `MAX + 1` menjamin nomor tidak kembali **selama barisnya tetap ada di tabel**, termasuk baris
> ber-`IsDelete` yang tetap terbaca agregatnya. Yang dapat mengembalikan sebuah nomor hanyalah
> penghapusan **fisik** dari luar aplikasi, dan hanya untuk nomor **tertinggi** — bukan celah di
> tengah, yang tetap tidak akan pernah diisi ulang.

**Dalam pemakaian aplikasi, keadaan itu tidak terjadi.** Penelusuran seluruh area Laboratorium
menghasilkan **nol** `LabOrders.Remove`, **nol** endpoint `DELETE` pada `LabOrderController`, dan
pembatalan hanya memindahkan status. Penghapusan fisik pada pengujian ini dikerjakan **oleh saya
lewat SQL**, bukan oleh jalur aplikasi mana pun.

Dicatat sebagai batas yang diketahui, bukan sebagai jaminan yang dilebihkan.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Kolom berdiri `NOT NULL` dan unik | **Terpenuhi** | `T2`, `T4` |
| Seluruh pesanan lama terisi | **Terpenuhi** | `T3` — kedelapannya |
| Kedua jalur pembuatan mengalokasikan nomor | **Terpenuhi** | `T8`, `T9` |
| Alokasi berblok terbukti pada jalur `by-examinations` | **Terpenuhi** | `T9` — 3 nomor berbeda dari satu panggilan |
| Celah tidak diisi ulang, beserta batas jaminannya | **Terpenuhi, dengan koreksi** | 5.3 |
| Jalur `Down` terbukti | **Terpenuhi** | `T7`, termasuk determinismenya |
| Usul `r16` ditulis lengkap | **Terpenuhi** | Bagian 7 |

---

## 7. Usul amandemen `LAB-API-v1` `r16` — **belum disetujui**

Nomor order hari ini **tersimpan tetapi tidak dapat dibaca siapa pun di luar backend**. Itu
disengaja: menampilkannya adalah perubahan kontrak, dan kontrak terkunci penuh sejak 2026-09-02.

**Dan itu persis pola yang sudah tiga kali menimpa modul ini** — `MVP-5d`, `MVP-5e`, dan celah
`LabOrderedProcedure` — yaitu sesuatu yang berdiri tanpa pembaca lalu tidak menghasilkan galat apa
pun sampai seseorang membutuhkannya. Usulnya karena itu ditulis sekarang, bukan ditunggu sampai
sebuah layar tertahan.

| Ruas | DTO | Alasan penempatan |
| --- | --- | --- |
| `orderNumber` | `LabOrderListResponse` | Kolom `No. Order` pada daftar. Siap tampil, bukan penunjuk |
| `orderNumber` | `LabMonitoringItemResponse` | Ketiga menu pemeriksaan membaca grup **Lab Monitoring**, bukan `LabOrderListResponse` — **pelajaran `r13`/`r14`**, yang menyebut DTO paling masuk akal namanya alih-alih DTO yang benar-benar dipanggil konsumennya |

`LabOrderDetailResponse` **tidak perlu disebut terpisah**: ia mewarisi `LabOrderListResponse`.

**Seluruhnya aditif** — nol endpoint baru, nol migration, nol permission baru, nol ruas lama
berubah. Pelaksanaannya satu task ringan.

> **Kenapa dua DTO, bukan satu.** Diperiksa dari source konsumennya, bukan ditebak: jalur yang
> dipakai ketiga menu Pemeriksaan adalah grup Lab Monitoring. Menambahkan ruas hanya pada
> `LabOrderListResponse` akan mengulang `r13` — benar secara kontrak, dan nol terpakai layar.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Nol warning baru.** Build akhir 0 error, 0 warning |
| Masalah yang diketahui | **`QBE-CODE-006` tidak terpenuhi:** nol provider alokasi bersama, dan **nol retry** atas `23505`. Dengan kunci advisory, tabrakan praktis tidak terjadi; bila tetap terjadi, permintaannya gagal `500` alih-alih dicoba ulang. Mendirikan provider bersama melampaui `LAB-DEC-072` dan menyentuh modul lain |
| Batas jaminan | Lihat 5.3. Penghapusan **fisik** baris bernomor tertinggi mengembalikan nomornya; aplikasi tidak pernah melakukannya |
| Dependency | `NONE` |
| Perubahan sampingan | `NONE` pada repository. Satu alat verifikasi database sekali pakai dibuat **di luar repository**, pada direktori scratchpad, karena `psql` maupun klien Postgres lain tidak tersedia di mesin ini |
| Interupsi | `NONE` |
| Status Git | 6 berkas tersentuh, nol di-stage, nol di-commit |
| Langkah berikutnya | Persetujuan `r16` lalu satu task ringan melaksanakannya. Sesudah itu Label Lab dan kolom `No. Order` **tidak lagi tertahan oleh bahan nomornya** — keduanya tetap menunggu `LAB-SIGN-001` untuk sebab yang lain |
