# Laporan Perubahan Backend — `BE-LAB-29`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-29` |
| Judul | Disiplin diturunkan pada endpoint pesanan lama |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6c |
| Trace | `LAB-DEC-048` butir 6; `AC-83` sebagaimana diamandemen `LAB-DEC-055`; `AC-85`; `LAB-CONFLICT-007` |
| Contract version | **Tidak ada perubahan kontrak.** Perilaku yang dikoreksi, bukan bentuk |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — satu berkas source dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `e2152709`, branch `yoga` |
| Tanggal | 2026-09-15 |
| Status | **✅ `SELESAI`** — satu ekspresi diubah, `LAB-CONFLICT-007` ditutup, dan bukti penutupannya dibaca langsung dari database |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | — |
| Pemilik dan prefix registry | Prefix **`Lab`**, lifecycle **`ACTIVE`** sejak 2026-09-02 |
| Keberlakuan | `TOUCHED LEGACY` — satu ekspresi pada method yang sudah ada. Nol entity, nol DTO, nol endpoint |
| Status gerbang | `QBE-MOD-002`/`QBE-MOD-003` **tidak berlaku** — nol model persisted baru |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-LOG-001` |

### Dua temuan gate yang ditutup lebih dulu

Keduanya **kelalaian pembukuan dari langkah perencanaan sesi ini sendiri**, ditemukan saat gate
dijalankan dan ditutup sebelum satu baris kode ditulis:

| Temuan | Keadaan | Penutupan |
| --- | --- | --- |
| `AC-83` **tidak punya satu pun baris uji** | Janji *"tidak ada jalan menyimpan pesanan berdisiplin kosong"* tercatat sejak revision 27 **tanpa pernah ada cara memeriksanya** | `T-83a`..`T-83d` ditambahkan pada matriks uji bagian 11.5b |
| Kepala `backend-roadmap.md` stale | Masih menyebut manifest revision `26` dan Backend SHA `466a7127`, padahal manifest `29` dan `HEAD` `e2152709` | Manifest, SHA, contract version, dan baris Masukan disegarkan |

Bahwa `AC-83` tidak pernah punya baris uji **bukan kebetulan** — itu sebab teknis mengapa
pelanggarannya bertahan sejak 2026-09-14 tanpa seorang pun menyadarinya.

---

## 1. Masalah yang diperbaiki

`LAB-DEC-048` butir 6 menetapkan disiplin pesanan **diturunkan dari pemeriksaan yang dipilih**,
dan `AC-83` menjanjikan *"tidak ada jalan menyimpan pesanan berdisiplin kosong ketika
pemeriksaannya sudah digolongkan"*.

**Janji itu hanya ditegakkan layar.** Di backend, `LabOrderService.CreateAsync` menyalin
`request.Discipline` apa adanya, dan ruas itu **tidak wajib**. Pemanggil mana pun di luar layar
tersebut — modul lain, skrip, Swagger, integrasi — masih dapat membuat pesanan tanpa disiplin.

Akibatnya tidak menimbulkan satu pun pesan kesalahan. Pesanan tersimpan dengan benar, pasiennya
ada, tarifnya ada — tetapi ketiga layar **Pemeriksaan** menyaring tepat atas kolom
`LabOrder.Discipline`, sehingga pesanan berdisiplin kosong **hilang dari ketiga-tiganya**.
Pasien tersimpan dengan benar namun tidak muncul di layar yang justru dipakai petugas
mengerjakannya.

**Ini bukan dugaan.** Pada `QuilvianNewDevYoga` hari ini:

| Pesanan | Pemeriksaan | Disiplin pada katalog | Disiplin pada pesanan |
| --- | --- | --- | --- |
| `494eed02…` | Glukosa Darah Sewaktu | **`1` Patologi Klinik** | **kosong** |
| `854fa320…` | Sitologi FNAB | **`2` Patologi Anatomi** | **kosong** |

Katalognya **sudah tahu** disiplin keduanya sejak awal. Pesanannya saja yang tidak menyalinnya.

---

## 2. Proses bisnis

**Pelaku.** Siapa pun yang membuat pesanan laboratorium lewat `POST /lab-orders` — petugas lewat
layar, maupun modul lain lewat integrasi.

**Jalur normal sesudah perubahan:**

1. Pemanggil mengirim `encounterId` dan `procedureId`, **tanpa** menyebut disiplin.
2. Sistem memuat katalog pemeriksaannya — hal yang memang sudah dilakukannya untuk memvalidasi.
3. Disiplin **diturunkan** dari `MstProcedure.LabDiscipline` milik pemeriksaan itu.
4. Pesanan tersimpan berdisiplin, dan muncul pada menu Pemeriksaan yang sesuai.

**Contoh berangka, memakai dua baris nyata di atas.** Seandainya perubahan ini sudah ada saat
kedua pesanan itu dibuat: Glukosa Darah Sewaktu akan mendarat di **Patologi Klinik**, dan
Sitologi FNAB di **Patologi Anatomi** — tanpa petugas menjawab satu pertanyaan tambahan pun.

**Tiga jalur tidak normal, dan ketiganya sengaja dibiarkan seperti semula:**

| Kejadian | Yang terjadi |
| --- | --- |
| Pemanggil **menyebut** disiplin | Nilainya **dihormati apa adanya**. Penurunan ini mengisi yang kosong, bukan menimpa yang terisi |
| Katalog pemeriksaannya **belum digolongkan** | Pesanan tetap tersimpan **tanpa disiplin**, dan itu sah (`AC-85`). Ia memang tidak akan muncul di menu disiplin mana pun — dan itulah sebabnya penggolongan katalog perlu dirawat |
| Disiplin yang dikirim **tidak dikenal** | Tetap ditolak seperti sebelumnya (`LAB-DEC-025`); perubahan ini tidak menyentuh pemeriksaan itu |

**Nol permintaan yang sebelumnya berhasil menjadi gagal.** Perubahan ini memperluas apa yang
**berhasil dengan benar**, bukan memperketat apa yang ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` bagian 6c | Cakupan, DoD, dan batas task |
| `00-interview-decisions.md` — `AC-83`, `AC-85`, BR-43 | Apa yang sebenarnya dijanjikan, dan apa yang sengaja dibiarkan sah |
| `testing/acceptance-test-matrix.md` | Ditemukan `AC-83` tanpa baris uji |
| `Services/LabOrderService.cs` `CreateAsync` | Jalur tulis disiplin, dan tempat katalog sudah dimuat |
| `Services/LabMonitoringService.cs` | Dasar penyaringan ketiga layar Pemeriksaan |
| `Areas/HealthServices/MasterData/Models/MstProcedure.cs` | Tipe `LabDiscipline?` |
| `Enums/LaboratoryEnums.cs` | Nilai enum `ClinicalPathology = 1`, `AnatomicalPathology = 2` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | **Satu ekspresi**: `Discipline = request.Discipline` menjadi `request.Discipline ?? procedure.LabDiscipline`, beserta dokumentasi alasannya |
| `docs/.../testing/acceptance-test-matrix.md` | `T-83a`..`T-83d` — menutup `AC-83` yang tidak punya baris uji |
| `docs/.../roadmap/backend-roadmap.md` | Kepala dokumen disegarkan; status task |

**Nol DTO, nol endpoint, nol entity, nol migration, nol permission.** `procedure` sudah dimuat
beberapa baris di atas untuk keperluan validasi, sehingga perubahan ini **tidak menambah satu
query pun**.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`NOT APPLICABLE`.** Tidak ada endpoint, ruas, atau bentuk respons yang berubah. Ruas `discipline` pada permintaan tetap opsional dan tetap dihormati bila dikirim |
| Database | **`NOT APPLICABLE`.** Nol schema, nol migration. Dua baris data lama **tidak** diperbaiki — lihat 6.1 |
| Keamanan/Auth | **`NOT APPLICABLE`.** `LabOrder : Create` tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pesanan laboratorium. **Bentuknya tidak berubah**; yang berubah adalah disiplin kini terisi sendiri ketika tidak disebut | `LabOrder : Create` |

Tidak ada endpoint baru, dan tidak ada ruas yang ditambah maupun dihapus.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Backend Governance Preflight | Registry `ACTIVE`; dua temuan gate ditutup lebih dulu | `PASS` | Bagian preflight |
| Review diff/scope | **Satu ekspresi** pada satu berkas source | `PASS` | Bagian 3.2 |
| `dotnet build -p:RunAnalyzers=False --no-incremental` | **0 Error, 207 Warning** | `PASS` | Lihat 5.1 |
| **Jalur tulis `LabOrder` ditelusuri seluruhnya** | **Tepat satu** di seluruh aplikasi | `PASS` | Lihat 5.2 |
| Katalog siap menjadi sumber penurunan | **10 dari 10** pemeriksaan lab sudah digolongkan | `PASS` | Lihat 5.3 |
| Bukti pelanggaran yang ditutup | **2 pesanan tanpa disiplin**, katalognya justru tahu disiplinnya | `PASS` | Lihat 5.3 |
| `T-83a`..`T-83d` runtime | Menunggu aplikasi dijalankan | `NOT RUN` | Lihat 5.4 |

### 5.1 Hasil build apa adanya

```
dotnet build -p:RunAnalyzers=False --no-incremental
    207 Warning(s)
    0 Error(s)
```

**Nol warning berasal dari modul Laboratorium** — disaring atas nama `LaboratoryManagement`,
hasilnya kosong.

> **Angkanya naik dari 191 menjadi 207, dan sebabnya bukan task ini.** `HEAD` bergeser ke
> `e2152709` saat sesi berjalan karena pekerjaan `BE-LAB-20`..`BE-LAB-25` di-commit lalu
> di-merge dari `origin/QuilvianIntegrationBackend`. Keenam belas warning tambahan berasal dari
> kode modul lain yang ikut masuk lewat merge itu. Dicatat supaya selisihnya tidak terbaca
> sebagai akibat perubahan ini.

### 5.2 Penutupan lubangnya dibuktikan dengan pencarian, bukan dengan kepercayaan

Klaim "tidak ada lagi jalan menyimpan pesanan tanpa disiplin" hanya bermakna bila jalur tulisnya
memang cuma satu. Ditelusuri atas `new LabOrder` dan `LabOrders.Add` di seluruh
`Areas/`, `Services/`, `Controllers/`, dan `Seeders/`:

```
Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs:378   var entity = new LabOrder
Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs:410   _dbContext.LabOrders.Add(entity);
```

**Tepat satu jalur, dan itulah yang diubah.** Tidak ada seeder, tidak ada service lain, tidak ada
controller yang membuat pesanan sendiri. Lubangnya karena itu tertutup penuh, bukan sebagian.

### 5.3 Bukti terhadap database

| Butir | Hasil |
| --- | --- |
| Katalog lab aktif | **10 baris, 10 di antaranya sudah digolongkan disiplin** — nol yang belum |
| Pesanan tanpa disiplin | **2 baris**, keduanya atas pemeriksaan yang **katalognya sudah tahu disiplinnya**: Glukosa Darah Sewaktu → `1` Patologi Klinik; Sitologi FNAB → `2` Patologi Anatomi |

Baris kedua adalah bukti terkuat pada laporan ini. Informasinya **selalu ada**; yang hilang cuma
penyalinannya. Seandainya perubahan ini sudah berlaku saat kedua pesanan itu dibuat, keduanya
akan mendarat di menu yang benar tanpa satu pertanyaan tambahan pun kepada petugas.

Karena katalognya 10 dari 10 tergolong, **setiap pesanan baru mulai sekarang akan berdisiplin**,
apa pun jalur pemanggilnya.

### 5.4 Yang belum dijalankan, dan kenapa

| Pemeriksaan | Kenapa belum |
| --- | --- |
| `T-83a`..`T-83d` | Memerlukan aplikasi dijalankan dengan kredensial pengguna. Perilakunya terverifikasi terhadap source, dan kesiapan datanya terverifikasi terhadap database |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan dengan sengaja:** pembuatan pesanan uji terhadap `QuilvianNewDevYoga`.
Membuat pesanan berarti menulis baris `LabOrder` beserta riwayatnya, dan nilainya tidak sebanding
— jalur tulisnya sudah terbukti tunggal, dan datanya sudah terbukti siap.

---

## 6. Acceptance criteria dan Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Tidak ada jalur kode yang menyimpan pesanan tanpa disiplin ketika prosedurnya digolongkan | **✅ Terpenuhi** | 5.2 — jalur tulisnya tepat satu, dan kini menurunkan |
| **Nol permintaan yang sebelumnya berhasil menjadi gagal** | **✅ Terpenuhi** | Perubahannya `??` pada nilai yang sebelumnya dibiarkan kosong; nol pemeriksaan baru ditambahkan |
| `LAB-CONFLICT-007` ditutup | **✅ Terpenuhi** | 5.2 dan 5.3 |

| AC | Status | Bukti |
| --- | --- | --- |
| `AC-83` — pesanan selalu berdisiplin ketika pemeriksaannya digolongkan, **ditegakkan backend** | **✅ Terpenuhi pada source** | `LabOrderService.cs:378-396`; pembuktian runtime menunggu aplikasi dijalankan |
| `AC-85` — pemeriksaan belum digolongkan tetap dapat dipesan tanpa disiplin | **✅ Terpenuhi dan tidak dicabut** | `??` menghasilkan `null` ketika katalognya `null` |

### 6.1 Dua baris data lama sengaja tidak diperbaiki

Kedua pesanan pada 5.3 **tetap tanpa disiplin** sesudah task ini. Memperbaikinya adalah
**perubahan data**, bukan perubahan kode, dan memerlukan wewenang tersendiri — sama seperti
eksekusi migration.

Keduanya akan tetap hilang dari ketiga layar Pemeriksaan sampai seseorang memutuskan mengisinya.
Dicatat, bukan dikerjakan diam-diam.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini |
| Masalah yang diketahui | Dua baris data lama — 6.1 |
| Risiko tersisa | Sangat rendah. Satu ekspresi yang hanya mengisi nilai kosong |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Satu berkas source, dua artefak blueprint, laporan ini. Tidak ada `git add`, `commit`, maupun `push` |
| Langkah berikutnya | Lihat 7.1 |

### 7.1 Langkah berikutnya

1. **Putuskan nasib dua pesanan lama** pada 6.1 — isi disiplinnya, atau biarkan sebagai jejak.
   Keduanya sah; yang tidak sah adalah membiarkannya tanpa keputusan.
2. `BE-LAB-26` berikutnya pada gelombang 1 `MVP-5b`, bersama `BE-EXT-04`.
3. **Pelajaran proses yang pantas dicatat.** `AC-83` bertahan dilanggar sejak 2026-09-14 karena
   ia **tidak punya satu pun baris uji** — bukan karena orang lalai memeriksanya. Acceptance
   criteria tanpa baris uji adalah janji tanpa cara menagihnya, dan matriks uji perlu diperiksa
   kelengkapannya setiap kali AC baru ditulis, bukan hanya saat task dikerjakan.
