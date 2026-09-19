# Permintaan Pengisian Data Induk — Tiga Satuan Ukur Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-010` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `DATA-MST-MEASUREMENT`; `LAB-DEC-041`; `AC-62`, `AC-64`; `roadmap/traceability.md` bagian 4.1b dan 5 |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Pemilik modul `master-data` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

> **Penahan ini belum pernah diajukan kepada siapa pun.** Ia tercatat pada
> `roadmap/traceability.md` sejak 2026-09-15 dengan keterangan *"Belum diajukan"*, dan
> menggantung sejak itu. Dokumen inilah permintaannya.
>
> Ini kedua kalinya pola yang sama ditemukan pada modul Laboratorium — `LAB-COORD-010` juga
> disangka sedang menunggu jawaban padahal belum pernah ditanyakan, dan baru diajukan
> 2026-09-17 lewat `LAB-REQ-008`. Dicatat terbuka supaya penahan ketiga tidak menggantung
> dengan cara yang sama.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Laboratorium mencatat **volume atau ukuran bahan** pada setiap wadah yang diterima — 5 mililiter
darah, 3 gram jaringan. Satuannya dipilih dari data induk `MstMeasurement` yang ditandai
`IsForLaboratory`.

Yang diminta: **tiga baris satuan baru** — `µL`, `blok`, dan `slide`. Tanpa ketiganya, pemeriksaan
mikro dan **seluruh patologi anatomi** tidak dapat menyatakan ukuran bahannya.

Ditambah satu hal yang **dilaporkan, bukan diminta**: daftar satuan laboratorium hari ini memuat
**sembilan baris ganda** dan beberapa satuan yang tidak masuk akal untuk sebuah tabung sampel.

---

## 2. Yang diminta — tiga baris

Bentuk kolomnya mengikuti baris `mL` yang sudah ada, dibaca langsung dari database pada
2026-09-17.

| # | Nama | Simbol | `MeasurementType` | `IsDecimalAllowed` | `DecimalPrecision` | Dipakai untuk |
|---:|---|---|---|:---:|:---:|---|
| 1 | Mikroliter | `µL` | `Volume` | **Ya** | 3 | Sampel bervolume sangat kecil — pemeriksaan mikro dan sampel bayi |
| 2 | Blok | `blok` | `Count` **atau yang setara** | **Tidak** | 0 | Blok parafin patologi anatomi |
| 3 | Slide | `slide` | `Count` **atau yang setara** | **Tidak** | 0 | Preparat kaca patologi anatomi |

Ketiganya perlu ditandai **`IsForLaboratory = true`** dan **`IsActive = true`**.

### 2.1 Dua hal yang ditentukan `master-data`, bukan Laboratorium

**Kodenya tidak didikte di sini.** `MstMeasurement` memakai seri tergenerasi berawalan `STN` —
contohnya `STN250826000074` untuk `mL`. Menuliskan kode usulan justru akan bertabrakan dengan
generatornya.

**Nama `MeasurementType` untuk blok dan slide diserahkan kepada `master-data`.** Keduanya bukan
volume dan bukan berat; ia cacahan benda. Nilai yang dipakai daftar hari ini adalah `Volume` dan
`Weight`, dan Laboratorium tidak tahu apakah sudah ada golongan cacahan atau perlu dibuat. Yang
penting bagi Laboratorium hanya **`IsDecimalAllowed = false`** — lihat 2.2.

### 2.2 Kenapa blok dan slide tidak boleh mengizinkan desimal

Ini satu-satunya ketentuan yang Laboratorium minta ditegakkan, dan alasannya praktis.

Blok parafin dan slide adalah **benda utuh yang dihitung**. "2,5 slide" tidak punya arti di meja
kerja — dan bila sistem mengizinkannya, angka itu akan muncul pada laporan tanpa satu pun
kesalahan yang terlihat. `mL` dan `g` sebaliknya memang desimal, dan keduanya sudah disetel
`IsDecimalAllowed = true` dengan `DecimalPrecision = 3`.

### 2.3 Yang **tidak** diminta

| Hal | Keadaan |
|---|---|
| `mL` dan `gram` | **Sudah ada** dan sudah ber-`IsForLaboratory`. Tidak perlu apa pun |
| Perubahan pada baris yang sudah ada | **Tidak diminta**, termasuk pada baris ganda di bagian 3 |
| Aturan validasi baru pada satuan | Tidak. `LAB-DEC-041` menetapkan **tidak ada** batas minimum maupun maksimum volume, dan Laboratorium tidak meminta penyaring tambahan |

---

## 3. Yang dilaporkan, bukan diminta

Dibaca langsung dari `MstMeasurement` pada 2026-09-17: **17 baris** aktif ber-`IsForLaboratory`.

### 3.1 Sembilan baris adalah duplikat dari empat simbol

| Simbol | Baris | Nama |
|---|---:|---|
| `g` | **2** | `GRAM`, `GR` |
| `mg` | **2** | `MILIGRAM`, `MG` |
| `L` | **3** | `LTR`, `LITER`, **`LITTER`** |
| `IU` | **2** | `IU`, `UI` |

Akibatnya di layar: petugas memilih satuan dari daftar dan melihat **dua pilihan bertuliskan `g`**
yang tidak dapat dibedakan. Mana pun yang dipilih tidak salah, tetapi laporan yang mengelompokkan
menurut satuan akan **memecah satu satuan menjadi dua baris**.

`LITTER` tampak salah eja dari `LITER`.

### 3.2 Beberapa satuan tidak masuk akal untuk tabung sampel

Ikut tampil pada daftar pilihan satuan wadah laboratorium: **`gal`** (galon), **`m3`** (meter
kubik), **`kg`**, **`ons`**, dan **`L/jam`**.

**Laboratorium sengaja tidak mempersempitnya sendiri.** Menambahkan penyaring berarti mengarang
aturan yang tidak pernah diputuskan — dan penyaring yang terlalu rajin justru akan menolak `blok`
dan `slide` yang diminta bagian 2. Yang dilaporkan di sini adalah **kebersihan data induk
global**, dan itu wewenang `master-data`.

### 3.3 Kenapa dilaporkan sekarang

Ketiga hal di atas tidak memblokir satu pun task Laboratorium. Ia disampaikan bersama permintaan
ini karena pemiliknya sama dan datanya baru saja dibaca — bukan karena mendesak.

---

## 4. Akibat bila belum dijawab

| Yang tertahan | Akibat nyatanya |
|---|---|
| `AC-64` — patologi anatomi menyatakan ukuran bahan | **Tertahan separuh.** Jaringan dapat memakai `gram`, tetapi **blok dan slide tidak dapat dinyatakan sama sekali** |
| Sampel bervolume sangat kecil | Terpaksa dicatat dalam `mL` sebagai pecahan kecil — `0,05 mL` alih-alih `50 µL` |

**`AC-62` tidak lagi tertahan**, dan itu koreksi atas catatan lama: dugaan bahwa
`MstMeasurement` kosong dari satuan laboratorium **terbantah** pada 2026-09-15. `mL` dan `gram`
memang sudah ada.

Tidak ada task yang berhenti karena permintaan ini. Yang ada adalah **bagian patologi anatomi
yang belum dapat memakai layar penerimaan sebagaimana mestinya.**

---

## 5. Cara menjawab

Cukup balas dokumen ini.

| Butir | Yang dibutuhkan |
|---|---|
| 2 | Ketiga baris dibuat, atau penolakan beserta alasannya. Kode dan `MeasurementType`-nya sepenuhnya wewenang `master-data` |
| 3 | Tidak perlu dijawab. Dilaporkan saja |

Bila `master-data` menilai `blok` dan `slide` sebaiknya tidak tinggal di `MstMeasurement` —
misalnya karena keduanya cacahan benda dan bukan satuan ukur — jawaban itu juga cukup.
Laboratorium membutuhkan **tempat yang sah untuk menyatakan keduanya**, bukan tabel tertentu.

Jawaban akan dicatat sebagai penutupan `DATA-MST-MEASUREMENT` pada `roadmap/traceability.md`.
