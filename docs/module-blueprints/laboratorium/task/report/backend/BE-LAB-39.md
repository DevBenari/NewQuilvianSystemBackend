# Laporan Perubahan — `BE-LAB-39` dan `FE-LAB-20`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-39`/`FE-LAB-20` (`r18`), `BE-LAB-40`/`FE-LAB-21` (`r19`), dan `BE-LAB-42`/`FE-LAB-22` (`r20`), dilaporkan bersama |
| Judul | Penyaring NIK, Kategori Periode, ikon gender, dan dua label cetak pada daftar pantau |
| Slice | `EPIC-LAB-11` |
| Trace | `BR-50` butir 1 dan bagian 5.3; `LAB-DEC-065`, `LAB-DEC-071`; menutup `REC3-NEW-003`, `REC3-NEW-010`, dan `REC3-NEW-009`, serta `REC3-NEW-002` **sebagian** |
| Kontrak | `LAB-API-v1` **`r18`**, **`r19`**, dan **`r20`** — ketiganya `approved` 2026-09-17 |
| Klasifikasi | `LOW` untuk ruasnya; `MEDIUM` untuk perbaikan cacat yang ikut ditemukan |
| Target tulis | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Tanggal | 2026-09-17 |
| Status | **✅ `SELESAI`** — 20 pemeriksaan API, 1067/1067 uji unit, 12 pemeriksaan layar, 10 regresi, seluruhnya lulus. **Satu cacat yang sudah ada ditemukan dan diperbaiki pada ENAM endpoint** |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Keberlakuan | `NEW` satu enum; `TOUCHED LEGACY` empat berkas backend, tiga berkas frontend |
| Status gerbang | Tidak menahan — **nol entity, nol kolom, nol migration, nol endpoint, nol permission** |

---

## 1. Yang dikerjakan

Dua ruas opsional pada `LabMonitoringQuery`, beserta sisi layarnya. Keduanya menurunkan `BR-50`
butir 1 dan **tidak** bergantung pada hasil pemeriksaan, sehingga dapat berjalan walaupun sisa
`BR-50` tertahan `LAB-SIGN-001`.

| Ruas | Menutup | Isi |
|---|---|---|
| `identityNumber` | `REC3-NEW-003` | Penyaring NIK, cocok sebagian, dibaca dari `MstPatient.IdentityNumber` |
| `dateCategory` | `REC3-NEW-002` **sebagian** | Kategori Periode: `OrderDate` (bawaan) dan `SamplingDate` |

### 1.1 Dua keputusan pemilik modul yang membentuk kodenya

**Pertama — satu kotak isian, dua ruas kontrak, dan petugas yang memilih.** Layar menampilkan
`Cari menurut: NIK / No. RM` di samping satu kotak, dan **nol menebak dari bentuk isian**.
Alternatifnya — menebak 16 digit sebagai NIK — ditolak karena gagalnya tidak terlihat: No. RM
yang kebetulan 16 digit akan dikirim sebagai NIK dan daftarnya kosong tanpa satu pun sebab yang
terbaca petugas. **Ada uji unit yang mengunci justru kasus itu.**

**Kedua — Kategori Periode menawarkan dua pilihan, bukan tiga.** `Tanggal Pemeriksaan`
**tidak dicantumkan sama sekali** — bukan ditampilkan nonaktif — karena `LabExamination` punya
20 properti dan nol di antaranya waktu pemeriksaan. Pilihan yang tidak menuju ke mana-mana lebih
buruk daripada pilihan yang belum ada. **Ada uji unit dan pemeriksaan layar yang menguji
ketiadaannya**, sehingga menambahkannya tanpa kolomnya akan gagal lebih dulu.

### 1.2 `Search` sengaja tidak ikut diperlebar

Memasukkan NIK ke pencarian bebas membuat ketiga menu Pemeriksaan yang sudah berjalan
mengembalikan baris yang sebelumnya tidak muncul — tanpa satu pun layar meminta perubahan itu.
**Pelebaran diam-diam sama merusaknya dengan pengetatan diam-diam**, pelajaran `BE-LAB-21` yang
di sini berlaku ke arah sebaliknya. Dibuktikan: `?search=<NIK>` tetap mengembalikan nol baris.

### 1.3 `CollectedAt` kosong tidak pernah cocok — berbeda dari `r17`, dan sengaja

Wadah yang belum dinyatakan waktu pengambilannya **tidak pernah cocok** ke rentang mana pun pada
`SamplingDate`. **Tidak ada jatuh-tempo ke `CreateDateTime`**, walaupun `r17` justru memakainya.

Perbedaannya bukan ketidakkonsistenan: `r17` menjawab *"wadah apa saja yang masuk hari itu"* dan
substitusinya **terlihat pembaca** — `FE-LAB-12` menandai baris yang waktu tibanya tidak pernah
dicatat. Di sini pertanyaannya *"pesanan mana yang diambil sampelnya dalam rentang ini"*, dan
hasilnya hanya daftar: baris bersubstitusi **tidak dapat dibedakan**. Jatuh-tempo akan
memunculkan pesanan yang tidak pernah diambil sampelnya sebagai *"diambil tanggal sekian"*.

---

## 2. Cacat yang sudah ada, ditemukan saat verifikasi

**Penyaring tanggal pada ketiga layar Pemeriksaan menjawab `500`, bukan daftar** — dan sudah
begitu sebelum task ini.

Ditemukan ketika `dateCategory` diuji terhadap aplikasi yang berjalan: **seluruh** uji berentang
tanggal gagal, **termasuk cabang `OrderDate` yang tidak disentuh `r18`**. Itu yang memisahkan
temuan dari regresi.

| Hal | Temuan |
|---|---|
| Yang dikirim layar | `YYYY-MM-DD` apa adanya — `FilterDatePicker` menghasilkan bentuk itu, `cleanParams` hanya membuang nilai kosong |
| Yang dikontrakkan | `Type = "date"`, `Example = "2026-09-01"` — **bentuk itu memang kontraknya** |
| Sebabnya | Nilai tanpa zona terikat `DateTimeKind.Unspecified`; Npgsql **menolak** menulisnya ke `timestamp with time zone` |

**Dua hal diperbaiki, dan yang kedua tidak akan terlihat tanpa yang pertama.**

1. **Penormalan zona** pada `AppDateTimeHelper.ToUtc` — nilai tanpa zona dibaca sebagai **jam
   dinding WIB**, karena itulah yang dimaksud petugas ketika mengetik satu tanggal.
2. **Akhir rentang dinaikkan ke penghabisan hari.** `LAB-DEC-071` menetapkan pembandingnya
   inklusif justru supaya pencarian **satu hari** mungkin. Tanpa ini,
   `startDate=2026-09-16&endDate=2026-09-16` berarti 00:00 sampai 00:00 dan mengembalikan **nol
   baris** — persis kegagalan yang keputusan itu tulis untuk dicegah.

> **Cakupan perbaikan disebut apa adanya:** hanya grup `Lab Monitoring`. Grup `Lab Order` dan
> `Lab Specimen` juga menerima ruas tanggal dan **belum diperiksa** terhadap cacat yang sama.
> Dicatat sebagai pekerjaan tersendiri, bukan diperbaiki diam-diam di luar cakupan.

### 2.1 Penjaga `VAL-76` dicabut karena terbukti tidak pernah tercapai

Penjaga `Enum.IsDefined` sempat ditulis di controller beserta pesan berbahasa Indonesia. Saat
diuji, jawabannya ternyata datang dari tempat lain:

```json
{ "status": 400, "errors": { "DateCategory": ["The value '99' is invalid."] } }
```

Pengikatan query ASP.NET Core sudah menjalankan pemeriksaan yang sama lebih dulu. **Penjaganya
dicabut** — penjaga yang tidak pernah tercapai terbaca seolah menjadi penegaknya, dan pesannya
akan dipelihara orang tanpa pernah sampai ke siapa pun. Komentarnya diganti keterangan mengapa
tidak ada penjaga di sana, supaya pembaca berikutnya tidak menambahkannya kembali.

**Satu selisih dilaporkan, tidak diperbaiki:** bentuk jawaban itu `ProblemDetails` bawaan
framework, bukan amplop `ApiResponse` yang dipakai seluruh modul, dan pesannya berbahasa Inggris.
Selisih ini berlaku bagi **setiap ruas enum pada query string di seluruh aplikasi** —
menyeragamkannya keputusan tingkat aplikasi. Layar tidak terdampak: pilihannya datang dari daftar
tertutup.

---

## 3. Verifikasi

### 3.1 Backend — 20 pemeriksaan terhadap aplikasi yang benar-benar berjalan

Dipanggil lewat HTTP ke `GET /lab-monitoring/clinical-pathology` pada `QuilvianNewDevYoga`.
**Nol baris ditulis** — seluruhnya baca.

| Kelompok | Isi | Hasil |
|---|---|---|
| A1–A5 | NIK penuh, cocok sebagian, tidak ada, spasi | 5/5 |
| **A6** | **`search=<NIK>` tetap nol baris** — pelebaran diam-diam tidak terjadi | 1/1 |
| B1–B4 | Bawaan = `OrderDate` eksplisit; `SamplingDate` vs `OrderDate` pada tanggal yang sama | 4/4 |
| **B5–B6** | **Rentang setahun: `SamplingDate` 3 baris, `OrderDate` 4** | 2/2 |
| B7 | Kategori tanpa rentang = nol penyaring tanggal | 1/1 |
| C1 | Muatan lama persis — nol ruas baru | 1/1 |
| D1–D4 | `99` dan `0` ditolak `400`; nama dan angka sama-sama terikat | 4/4 |
| E1–E2 | Kedua penyaring digabung | 2/2 |

**B5 dan B6 adalah pasangan yang menentukan.** Rentang yang sama persis; `SamplingDate`
mengembalikan **3** dan `OrderDate` mengembalikan **4**. Yang hilang adalah `LAB-RSMMC-000006` —
pesanan yang nol punya wadah ber-`CollectedAt`. **Itulah bukti bahwa "tidak pernah cocok" benar
berjalan**, dan ia tidak dapat dibuktikan oleh uji yang hanya memeriksa baris yang muncul.

Pencarian **satu hari** dengan format polos yang persis dikirim layar, sesudah perbaikan
bagian 2:

| Pencarian | Hasil |
|---|---:|
| `OrderDate` 2026-09-09 | 3 baris |
| `OrderDate` 2026-09-16 | 1 baris |
| `SamplingDate` 2026-09-09 | 3 baris |
| `SamplingDate` 2026-09-16 | **0 baris** — benar; pesanan hari itu nol wadah terambil |

### 3.2 Frontend

| Yang dijalankan | Hasil |
|---|---|
| Lint ketiga berkas yang disentuh | Bersih |
| Uji unit repository | **1053 / 1053** — 10 baru |
| Build produksi | Hijau |
| Pemeriksaan layar `r18` | **4 / 4** |
| Regresi `lab-monitoring-date-guard` pada layar yang sama | **2 / 2** |

**Dua uji unit sengaja menguji hal yang tidak boleh terjadi:** No. RM 16 digit **tidak** berpindah
menjadi NIK, dan `dateCategory` bawaan **tidak** ikut dikirim. Satu pemeriksaan layar menguji
**ketiadaan** pilihan `Tanggal Pemeriksaan`.

---

## 4. Definition of Done

| Butir | Keadaan |
|---|---|
| Penyaring NIK berjalan ujung ke ujung | ✅ Backend, layar, dan muatan permintaannya terbukti |
| Kategori Periode dua pilihan | ✅ `OrderDate` dan `SamplingDate` |
| `Tanggal Pemeriksaan` tidak didirikan tanpa kolomnya | ✅ Nol pada enum, nol pada layar, **diuji sebagai ketiadaan** |
| Pemanggil lama nol berubah | ✅ C1 dan uji unit muatan; regresi layar hijau |
| Nol migration, nol permission | ✅ |
| Nol baris uji tertinggal di database | ✅ Seluruh pemeriksaan baca saja |

---

## 5. Langkah berikutnya

| # | Hal | Pemilik |
|---:|---|---|
| 1 | Memeriksa `Lab Order` dan `Lab Specimen` terhadap cacat zona tanggal yang sama (bagian 2) | Laboratorium |
| 2 | `Tanggal Pemeriksaan` masuk bersama kolomnya | Menunggu `S4` / `LAB-SIGN-001` |
| 3 | Menyeragamkan bentuk galat enum query string ke amplop `ApiResponse` | Tingkat aplikasi, di luar modul |

---

## 6. Susulan `r19` — ikon gender (`BE-LAB-40` / `FE-LAB-21`), 2026-09-17

Dikerjakan pada hari yang sama, dilaporkan di sini karena menyentuh DTO dan layar yang sama.

### 6.1 Koreksi: ia bukan "nol kontrak"

`REC3-NEW-010` tercatat sebagai *"kewenangan UI, tidak menyentuh kontrak backend"*, dan itu
**diulang dalam rekap kepada pemilik modul**. Pemeriksaan source membantahnya:
**`LabMonitoringItemResponse` nol membawa gender**, dan penelusuran seluruh DTO Laboratorium
hanya menemukannya pada jalur pendaftaran. Ikonnya **mustahil** dibangun dari sisi layar saja.

Catatan lamanya keliru karena mengandaikan datanya sudah sampai. `LAB-API-v1` `r19` disetujui
pemilik modul dan menambahkan **satu ruas** — aditif, nol migration, nol permission.

### 6.2 Empat keadaan, tiga tampilan — kediaman artifact tidak ditebak

Artifact hanya menyebut **dua** warna. Enum `Gender` punya **empat** nilai, dan ruasnya
nullable. Kediaman itu diputuskan pemilik modul, bukan diisi sendiri:

| Nilai | Ikon | Label |
|---|---|---|
| `Male` | `Mars`, biru/navy | Laki-laki |
| `Female` | `Venus`, pink | Perempuan |
| `Unknown` / `null` / nilai tak dikenal | `CircleUser`, abu-abu | Gender tidak diketahui |
| `NotDisclosed` | `CircleUser`, abu-abu | **Gender tidak diinformasikan** |

**Warnanya sama, katanya berbeda — dan itu intinya.** `NotDisclosed` adalah pilihan **sadar**
pasien, bukan pengisian yang kurang. Menyatukan keduanya menjadi satu label akan membaca
keputusan pasien sebagai data yang hilang. **Ada uji unit dan pemeriksaan layar yang menguji
justru perbedaan label itu**, sehingga penyatuan kelak akan gagal lebih dulu.

**"Tanpa ikon" ditolak** karena baris tanpa ikon tidak dapat dibedakan dari baris yang ikonnya
gagal dimuat. **"Tanda tanya" ditolak** karena ia membaca pilihan sadar sebagai kekurangan.

### 6.3 Warna bukan satu-satunya pembawa makna

Ikonnya `aria-hidden`, dan artinya dibawa teks yang hanya terbaca pembaca layar. Petugas yang
tidak membedakan biru dari pink tetap memperoleh katanya — dan justru pada nada netral, warnanya
memang **sama** untuk dua hal yang berbeda, sehingga katanya adalah satu-satunya pembeda.

### 6.4 Verifikasi

| Yang dijalankan | Hasil |
|---|---|
| Build backend | 0 error |
| **Respons nyata** `GET /clinical-pathology` | `gender` terisi nama enum; **dicocokkan silang ke database** — 4 dari 4 pesanan cocok (`Female`×3, `Male`×1) |
| Uji unit repository | **1058 / 1058** — 5 baru |
| Build produksi frontend | Hijau |
| Pemeriksaan layar `r19` | **4 / 4** — lima baris meliputi keempat nilai enum **dan** satu baris yang ruasnya tidak ada |
| Regresi `r18` + `date-guard` | **6 / 6** |

**Satu pemeriksaan layar membandingkan warna terhitung**: laki-laki, perempuan, dan netral
terbukti berbeda satu sama lain, sementara `Unknown` dan `NotDisclosed` terbukti **sama**
warnanya dan **berbeda** katanya.

**Satu baris uji sengaja tidak memuat ruas `gender` sama sekali** — bentuk yang akan diterima
pemanggil dari muatan sebelum `r19`. Ia terbukti tetap menghasilkan ikon, bukan galat.

---

## 7. Susulan `r20` — Label Lab dan Label Goldar (`REC3-NEW-009`), 2026-09-17

### 7.1 Penahannya ternyata keputusan, bukan teknis

Kelima butir `Laboratorium (4).md` yang tersisa diperiksa terhadap source. **Empat benar-benar
buntu** — nol kolom waktu pemeriksaan, nol gerbang pesan, nol pustaka PDF, nol entity
persetujuan hasil, dan `Dokter Lantai` nol kemunculan di seluruh source.

**Yang kelima ternyata datanya lengkap:** `MstPatient.BloodType` ✅, `OrderNumber` ✅ (`BE-LAB-36`),
`orderedProcedures` ✅ (`BE-LAB-35`), `react-barcode` ✅, komponen cetak ✅. Penahannya
**`LAB-DEC-030`** — keputusan pemilik modul sendiri yang menempatkannya di Rilis 2 dengan alasan
*"kemampuan cetak, tidak memblokir alur kerja"*.

Pemilik modul memindahkannya ke **Rilis 1** pada 2026-09-17 (`LAB-DEC-075`), karena alasan
penundaan itu tidak lagi sebanding dengan biaya yang sudah mendekati nol.

### 7.2 Nota Lab ternyata sudah berdiri

`FE-LAB-17` membangun **"Ringkasan Pesanan Laboratorium"** — A4, informasi pasien, daftar
pemeriksaan, blok tanda tangan. Artifact menuliskan isi Nota Lab sebagai *"informasi pasien, data
yang dibutuhkan untuk pemeriksaan, dan pemeriksaan yang dipilih pasien"*.

**Itu isi yang sama.** Membangun dokumen A4 kedua hanya akan menduplikasi yang sudah terbukti,
jadi tombolnya dinamai `Nota` dan menunjuk dokumen yang sudah ada. **Yang benar-benar baru dua
label.**

> **Satu hal diserahkan sebagai keputusan kecil, bukan diambil sepihak:** judul dokumennya masih
> berbunyi "Ringkasan Pesanan Laboratorium", bukan "Nota Lab". Menggantinya mengubah kata yang
> sudah dipakai petugas sejak `FE-LAB-17`.

### 7.3 Yang dibangun

| Label | Menempel pada | Isi | Ukuran |
|---|---|---|---|
| **Label Lab** | Amplop hasil pemeriksaan | Nama, No. RM, nomor order, **barcode dari nomor order** | 100 × 50 mm landscape |
| **Label Goldar** | Tube berisi sampling pasien | Nama, No. RM, **golongan darah** | 50 × 25 mm landscape |

**Barcode-nya dari nomor order, bukan barcode wadah**, dan itu keputusan: satu amplop memuat
hasil satu nomor order, sedangkan satu order dapat memiliki beberapa wadah. Memakai barcode wadah
akan menautkan amplop ke salah satu wadahnya saja.

**Ukurannya TIDAK diadopsi sebagai aturan.** `LAB-OPEN-032` tetap terbuka; angkanya hidup sebagai
satu konstanta `LAB_LABEL_SIZE` berlabel "belum dikonfirmasi", sehingga jawabannya kelak cukup
mengganti angkanya alih-alih membongkar dua komponen. Ada uji yang menjaga ia tetap satu tempat.

### 7.4 Dua hal yang sengaja diuji sebagai "tidak boleh terjadi"

Keduanya gagal dengan cara yang sama — hasilnya **terlihat seperti label yang sah sampai
seseorang memakainya**:

| Yang dijaga | Kenapa |
|---|---|
| Nomor order kosong **tidak** menghasilkan barcode | Barcode dari string kosong tercetak sebagai batang yang tidak dapat dipindai, dan ia terlihat sah sampai dicoba. Gantinya teks *"Nomor order tidak tersedia"* |
| Golongan darah tak diketahui **tidak** tercetak kosong | Bagian kosong pada label tube tidak dapat dibedakan dari label yang tercetak tidak sempurna. Gantinya tanda pisah `—` beserta katanya pada tooltip |

### 7.5 Kedua label tidak memanggil rincian pesanan

Seluruh isinya sudah ada pada baris daftar. Memanggil detail untuk label tube 50 × 25 mm berarti
menunggu satu perjalanan jaringan demi dua baris teks yang sudah dipegang. **Dibuktikan pada
layar:** membuka kedua label menghasilkan **nol** panggilan `lab-orders`, sementara Nota tetap
memanggilnya — daftar pemeriksaan terpesan memang hanya ada di detail (`r15`).

### 7.6 Verifikasi

| Yang dijalankan | Hasil |
|---|---|
| Build backend | 0 error |
| Uji unit repository | **1067 / 1067** — 9 baru |
| Build produksi frontend | Hijau |
| Pemeriksaan layar label | **4 / 4** |
| Regresi tiga spec layar pantau lain | **10 / 10** |

### 7.7 Sisa `REC3-NEW-009` yang jujur

`LAB-OPEN-032` **tetap terbuka**. Ukuran yang dipakai hari ini adalah default yang artifact
sendiri tandai `Confidence: Medium`, dan ia belum pernah diadu dengan printer label sungguhan —
margin, DPI, dan media aktual dapat menuntut angka lain.
