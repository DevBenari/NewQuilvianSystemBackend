# Rencana Penetapan Kelas Kamar — Denah Rawat Inap RS MMC

**Tanggal:** 15 Oktober 2026 · **Status:** disetujui pemilik dan diimplementasikan pada 15 Oktober
2026 di source frontend. Register pengerjaan ada di bagian 9 dan laporan implementasi di bagian 10.

**Lingkup:** tampilan denah `/health-services/inpatient-management/floor-plan` dengan data dummy.
Tanpa API/database dan tanpa base component Quilvian. Struktur kode bebas.

## 1. Ringkasan

- Ada **30 kamar berawalan "K."** di Lantai 3, 4, dan 5, dan semuanya belum berkelas. "K." pada
  gambar adalah singkatan *Kamar*, bukan nama kelas.
- Usulan: **19 kamar Kelas II, 9 kamar Kelas I, 2 kamar Kelas III**. Kelas ditentukan dari ukuran
  kamar, lalu jumlah tempat tidurnya dicocokkan dengan data resmi RS MMC di SIRS Kemenkes.
- Format nama mengikuti pola `VIP 304`, yaitu **`<Kelas> <Nomor>`**. Contoh: `Kelas II 329`,
  `Kelas I 330`, `Kelas III 505`.
- Kelas yang sudah ada **tidak diganti**: VIP, SVIP, Suite, Premium, Grand Royal, VIP Deluxe, dan
  VIP Superior. Di luar kamar "K.", hanya format nama `VIP Isolasi K. 319` yang berubah menjadi
  `VIP Isolasi 319`, sedangkan kelasnya tetap.
- Perubahan cukup di folder `_data` ditambah sedikit tampilan. ID kamar tidak berubah, jadi kunci
  data okupansi tetap berlaku.

## 2. Kelas yang berlaku di RS MMC

| Kelas | Sumber | Di denah |
|---|---|---|
| Kelas I, Kelas II, Kelas III | Formulir persetujuan rawat inap (lembar referensi RS MMC), SIRS Kemenkes, situs resmi, PT KAM | **Belum** — inilah kamar "K." |
| VIP (Standar), VIP Deluxe, VIP Superior | Formulir, situs resmi, PT KAM | Lt 3, 4, 9 |
| SVIP (gambar menulis "SVP") | Gambar denah; SIRS mengelompokkannya sebagai "VVIP/Super VIP" | Lt 3, 4 |
| Suite, Premium, Grand Royal, Day Room — *Sujudi Executive Ward* | Situs resmi, PT KAM, formulir | Lt 8 |
| VIP 8.1 / 8.2 / 8.3 — *Aditama Executive Ward* | PT KAM | Belum dipetakan |
| Isolasi, ICU, Perinatologi, ODC (One Day Care) | Formulir, SIRS | Lt 2; VIP Isolasi di Lt 3 |

Jumlah tempat tidur (TT) resmi RS MMC menurut SIRS Kemenkes. Tanggal pembaruan tidak tercantum
di sumbernya.

| VVIP/Super VIP | VIP | Kelas I | Kelas II | Kelas III | Isolasi | Perinatologi | ICU | NICU | PICU |
|---|---|---|---|---|---|---|---|---|---|
| 50 | 28 | 20 | 51 | 12 | 11 | 8 | 12 | 7 | 1 |

## 3. Aturan nama kamar

Nama kamar ditulis **`<Kelas> <Nomor>`**: nama kelas langsung diikuti nomor kamar tiga digit,
dengan digit pertama menunjukkan lantai. Di denah, label ditulis dua baris, kelas di baris pertama
dan nomor di baris kedua, persis seperti kamar VIP sekarang.

| Kelas | Nama (pop up, aria-label) | Label denah |
|---|---|---|
| Kelas I | `Kelas I 330` | `Kelas I` / `330` |
| Kelas II | `Kelas II 329` | `Kelas II` / `329` |
| Kelas III | `Kelas III 505` | `Kelas III` / `505` |
| Kelas lain (tetap) | `VIP 304`, `SVIP 318`, `VIP Isolasi 319`, `Premium 801`, `Suite 812`, `Grand Royal 811`, `VIP Deluxe 901`, `VIP Superior 907` | kelas / nomor |

```
Sebelum        Sesudah
 K. 329        Kelas II
 [3/2]           329
                [3/2]
```

Alasan memakai angka Romawi:

- `Kelas 1 330` terbaca sebagai dua angka berdampingan, sedangkan `Kelas I 330` tidak ambigu.
- Ejaan Romawi dipakai SIRS Kemenkes dan PT KAM, pengelola RS MMC. Formulir RS MMC sendiri menulis
  "Kelas 1/2/3". Keduanya resmi, dan Romawi dipilih karena lebih mudah dibaca.
- Singkatan `K1/K2/K3` tidak dipakai. Di gambar, "K." berarti Kamar, dan "K3" lazim dibaca sebagai
  Keselamatan dan Kesehatan Kerja.

## 4. Aturan penentuan kelas kamar "K."

Tidak ada sumber publik yang mencantumkan kelas per nomor kamar. Karena itu kelas ditentukan dari
luas kamar, cara yang sama dengan penentuan kelas Lantai 8 dan 9 di kode. Luas dihitung dari
koordinat denah dengan skala dari dimensi gambar (bentang 7.200 mm dan W–Z 23.500 mm), dengan
akurasi ±10%.

| Tipe kamar | Luas ± | TT | Kelas | Kamar |
|---|---|---|---|---|
| Bentang ganda di baris bawah Lt 5 | 59–60 m² | 6 | **Kelas III** | 505, 506 |
| Standar, memanjang sampai koridor | 41–46 m² | 3 | **Kelas II** | 19 kamar di Lt 3, 4, 5 |
| Pendek di ujung dekat tangga, dan satu bentang di baris bawah Lt 5 | 22–30 m² | 2 | **Kelas I** | 330, 428, 429, 507, 503, 504 |
| Kecil, tanpa kamar mandi dalam | 10–15 m² | 1 | **Kelas I** ⚠ | 520, 521, 522 |

Hasilnya dibandingkan dengan SIRS:

| | Kelas I | Kelas II | Kelas III | Total |
|---|---|---|---|---|
| Usulan denah | 9 kamar · 15 TT | 19 kamar · 57 TT | 2 kamar · 12 TT | **84 TT** |
| SIRS Kemenkes | 20 TT | 51 TT | 12 TT | **83 TT** |

Total TT hampir sama (84 dan 83), dan Kelas III tepat 12. Selisih 5–6 TT antara Kelas I dan Kelas II
wajar karena jumlah TT per kamar masih berupa perkiraan. Bila RS memberi daftar resmi, cukup ubah
satu kata kelas per kamar (lihat bagian 7).

Situs resmi menulis Class 3 dan VIP Deluxe berada di "Lantai 3–5", dan itu cocok dengan denah. Namun
situs yang sama juga menulis Class 1 di Lantai 1, Class 2 di Lantai 2, dan ICU di Lantai 1, padahal
di denah Lantai 1 berisi lobi dan kantor, sementara ICU ada di Lantai 2. Karena itu, keterangan
lantai dari situs tidak dipakai untuk menentukan kelas per kamar.

## 5. Pemetaan per lantai

### Lantai 3 — NS3A / NS3B

| Nomor | ID (tetap) | Luas ± | TT | Kelas | Nama baru |
|---|---|---|---|---|---|
| 330 | `k-330` | 22 m² | 2 | Kelas I | Kelas I 330 |
| 329 | `k-329` | 43 m² | 3 | Kelas II | Kelas II 329 |
| 328 | `k-328` | 42 m² | 3 | Kelas II | Kelas II 328 |
| 327 | `k-327` | 44 m² | 3 | Kelas II | Kelas II 327 |
| 326 | `k-326` | 42 m² | 3 | Kelas II | Kelas II 326 |
| 325 | `k-325` | 42 m² | 3 | Kelas II | Kelas II 325 |
| 322 | `k-322` | 43 m² | 3 | Kelas II | Kelas II 322 |
| 321 | `k-321-a` | 43 m² | 3 | Kelas II | Kelas II 321 |
| 321 | `k-321-b` | 43 m² | 3 | Kelas II | Kelas II 321 |
| 320 | `k-320` | 43 m² | 3 | Kelas II | Kelas II 320 |
| 319 | `vip-isolasi-k-319` | 28 m² | 1 | VIP Isolasi (tetap) | VIP Isolasi 319 |

Subtotal: Kelas I 1 kamar (2 TT), Kelas II 9 kamar (27 TT).

### Lantai 4 — NS A / NS B

| Nomor | ID (tetap) | Luas ± | TT | Kelas | Nama baru |
|---|---|---|---|---|---|
| 428 | `k-428` | 24 m² | 2 | Kelas I | Kelas I 428 |
| 427 | `k-427` | 42 m² | 3 | Kelas II | Kelas II 427 |
| 426 | `k-426` | 45 m² | 3 | Kelas II | Kelas II 426 |
| 425 | `k-425` | 43 m² | 3 | Kelas II | Kelas II 425 |
| 423 | `k-423` | 42 m² | 3 | Kelas II | Kelas II 423 |
| 422 | `k-422` | 46 m² | 3 | Kelas II | Kelas II 422 |
| 421 | `k-421` | 42 m² | 3 | Kelas II | Kelas II 421 |
| 420 | `k-420` | 41 m² | 3 | Kelas II | Kelas II 420 |
| 429 | `k-429` | 25 m² | 2 | Kelas I | Kelas I 429 |

Subtotal: Kelas I 2 kamar (4 TT), Kelas II 7 kamar (21 TT).

### Lantai 5 — NS Lt 5 A dan sayap kanan

| Nomor | ID (tetap) | Luas ± | TT | Kelas | Nama baru |
|---|---|---|---|---|---|
| 507 | `k-507` | 25 m² | 2 | Kelas I | Kelas I 507 |
| 508 | `k-508` | 42 m² | 3 | Kelas II | Kelas II 508 |
| 509 | `k-509` | 42 m² | 3 | Kelas II | Kelas II 509 |
| 510 | `k-510` | 42 m² | 3 | Kelas II | Kelas II 510 |
| 506 | `k-506` | 60 m² | 6 | Kelas III | Kelas III 506 |
| 505 | `k-505` | 59 m² | 6 | Kelas III | Kelas III 505 |
| 504 | `k-504` | 30 m² | 2 | Kelas I | Kelas I 504 |
| 503 | `k-503` | 29 m² | 2 | Kelas I | Kelas I 503 |
| 520 | `k-520` | 11 m² | 1 | Kelas I ⚠ | Kelas I 520 |
| 521 | `k-521` | 15 m² | 1 | Kelas I ⚠ | Kelas I 521 |
| 522 | `k-522` | 10 m² | 1 | Kelas I ⚠ | Kelas I 522 |

Subtotal: Kelas I 6 kamar (9 TT), Kelas II 3 kamar (9 TT), Kelas III 2 kamar (12 TT).

## 6. Perlu konfirmasi RS MMC (tidak menahan implementasi)

1. **K. 520, 521, 522 (Lt 5).** Luasnya hanya ±10–15 m², tanpa kamar mandi dalam, dan bersebelahan
   dengan Pol 22–24. Bisa jadi ini ruang poli atau observasi, bukan kamar rawat inap. Untuk sementara
   ditetapkan Kelas I dengan 1 TT. Bila RS menyatakan bukan kamar rawat inap, cukup hapus dari
   `rooms` dan dari data okupansi.
2. **Dua ruangan tanpa label di Lt 3**, di antara NS3B dan K. 322. Ukurannya ±42–43 m², sama dengan
   kamar Kelas II, tetapi nomor 323–324 tidak ada di gambar. Ruangan ini tetap tidak bisa diklik
   sampai ada nomor resmi. Bila ternyata kamar, kelasnya Kelas II dengan 3 TT.
3. **Lt 4.** Nomor 424 tidak ada di gambar, dan ruangan tanpa label di antara 420 dan 429 tetap tidak
   bisa diklik.
4. **Sudah diputuskan sebelumnya, tidak diubah:** nomor ganda "K. 321" di Lt 3 tetap dua kamar
   dengan ID berbeda, sehingga keduanya tampil `Kelas II 321`. Kamar "501 / 429" di Lt 4 memakai
   nomor 429.

## 7. Rencana implementasi

Seluruh path relatif terhadap
`QuilvianSystemFrontendDev/src/app/health-services/inpatient-management/floor-plan/`.

### Tahap 1 — Registri kelas dan nama kamar

1. Buat **`_data/room-classes.js`**, satu sumber untuk label kelas dan pembentuk nama:

   ```js
   export const ROOM_CLASSES = Object.freeze({
     "kelas-3": { label: "Kelas III", group: "umum" },
     "kelas-2": { label: "Kelas II", group: "umum" },
     "kelas-1": { label: "Kelas I", group: "umum" },
     vip: { label: "VIP", group: "vip" },
     "vip-deluxe": { label: "VIP Deluxe", group: "vip" },
     "vip-superior": { label: "VIP Superior", group: "vip" },
     svip: { label: "SVIP", group: "eksekutif" },
     premium: { label: "Premium", group: "eksekutif" },
     suite: { label: "Suite", group: "eksekutif" },
     "grand-royal": { label: "Grand Royal", group: "eksekutif" },
     "vip-isolasi": { label: "VIP Isolasi", group: "khusus" },
   });

   export const roomName = (kind, number) => `${ROOM_CLASSES[kind].label} ${number}`;
   export const roomLabelLines = (kind, number) => [ROOM_CLASSES[kind].label, String(number)];
   ```

2. Ubah **`_data/lantai-3.plan.js`, `lantai-4.plan.js`, dan `lantai-5.plan.js`** mengikuti pola
   `room(number, kind, …)` yang sudah dipakai Lantai 9. Kelas ditulis langsung di daftar `ROOMS`,
   sehingga meninjau atau mengganti kelas cukup dengan membaca satu daftar:

   ```js
   const topRowRoom = (number, kind, x1, x2, id = `k-${number}`) =>
     createRoom({
       id,
       name: roomName(kind, number),
       kind,
       points: rectPoints(x1, NORTH_WALL_Y, x2, TOP_ROW_SOUTH_Y),
       label: { lines: roomLabelLines(kind, number), x: (x1 + x2) / 2, y: TOP_LABEL_Y },
     });

   topRowRoom(329, "kelas-2", 208, 275),
   topRowRoom(321, "kelas-2", 1081, 1148, "k-321-a"),
   ```

   - **ID tetap `k-<nomor>`**, tidak mengikuti kelas. Dengan begitu, mengganti kelas cukup satu kata
     dan kunci di `lantai-*.occupancy.js` tidak perlu diubah.
   - `kind: "kamar"` diganti kunci kelas. Saat ini belum ada komponen yang membaca `kind`, jadi
     perubahan ini aman.
   - Pada `VIP Isolasi K. 319`, nama dan label diubah menjadi `VIP Isolasi 319`, sedangkan ID dan
     kelasnya tetap.
   - Komentar "Penamaan …" di setiap berkas diperbarui dengan aturan kelas dari dokumen ini.
3. Pada **`lantai-8.plan.js` dan `lantai-9.plan.js`**, peta kelas lokal (`CLASSES`, `className`)
   diganti dengan registri. Ini hanya perapian: tidak ada nama atau kelas yang berubah di layar.

### Tahap 2 — Okupansi dummy mengikuti kelas

Kapasitas di `lantai-3.occupancy.js`, `lantai-4.occupancy.js`, dan `lantai-5.occupancy.js`
disesuaikan menjadi Kelas I 2 TT (520–522 cukup 1 TT), Kelas II 3 TT, dan Kelas III 6 TT. Isi
`occupied` dibuat bervariasi, yaitu kosong, sebagian, dan penuh, agar setiap keadaan tampil.
Komentar "Kamar K — 3 tempat tidur" diganti dengan penjelasan per kelas.

### Tahap 3 — Tampilan

1. **Posisi label.** Label kini dua baris, jadi kamar yang sempit perlu digeser. Angka di bawah ini
   adalah patokan awal; posisi final ditetapkan lewat cek visual.

   | Kamar | Sekarang | Usulan | Alasan |
   |---|---|---|---|
   | K. 330 (Lt 3) | y 564 | y ±536 | Badge menabrak pintu di sisi bawah |
   | K. 428, K. 429 (Lt 4) | y 562 | y ±530 | Badge keluar dari dinding bawah (588) |
   | 505, 506, 503, 504 (Lt 5) | y 819–820 | y ±806 | Jarak badge ke fasad hanya 8 px |
   | 521 (Lt 5) | y 834 | y ±814 | Badge keluar dari dinding bawah (861) |
   | 520, 522 (Lt 5) | 1 baris, ukuran 13 | 1 baris `Kelas I 520`, ukuran ±10,5 | Tinggi kamar hanya 52–56 px |

   > **Diimplementasikan dengan cara yang lebih aman (bagian 10.5).** Nomor kamar tetap di posisi
   > label lama dan nama kelas ditambahkan satu baris di atasnya, sehingga badge okupansi tidak
   > bergeser sama sekali. Kamar 520 dan 522 memakai satu baris berukuran 10.

2. **Warna kelas dan legenda per lantai** — direkomendasikan, opsional.
   - Elemen `<g>` setiap kamar diberi `data-room-class={room.kind}`, dan baris pertama label (nama
     kelas) diwarnai menurut grup. Tambahkan token di `.theme`, misalnya
     `--fp-class-umum: #0f766e`, `--fp-class-vip: #4338ca`, `--fp-class-eksekutif: #a16207`, dan
     `--fp-class-khusus: #7c3aed`.
   - Merah tetap khusus untuk "Kamar penuh" dan biru tetap untuk hover/aktif, supaya maknanya tidak
     bertabrakan.
   - Legenda di bilah atas menampilkan chip kelas yang ada di lantai tersebut saja. Daftar chip
     dihitung di `page.jsx` (server) dan dikirim sebagai prop, sesuai pola sekarang yang tidak
     mengirim data denah ke klien.

   > **Diimplementasikan (bagian 10.5):** atributnya `data-room-group` (grup warna, bukan kunci
   > kelas), dan warnanya diturunkan dari token global `--color-*`, bukan warna literal di atas.
3. **Pop up kamar** — opsional. Di bawah judul, tambahkan satu baris kecil seperti
   `Kelas II · 3 tempat tidur · 2 terisi`.

### Tahap 4 — Verifikasi

1. **Skrip cek** di scratchpad (tidak di-commit) yang mengimpor seluruh `lantai-*.plan.js` dan
   `lantai-*.occupancy.js`, lalu memastikan:
   - setiap `kind` ada di `ROOM_CLASSES`;
   - setiap nama cocok dengan pola `^<label kelas> \d{3}$` dan tidak ada lagi "K. ";
   - kunci okupansi sama persis dengan ID kamar;
   - kapasitas sesuai kelas;
   - blok label dan badge berada di dalam batas kamar.
2. Jalankan `npx eslint "src/app/health-services/inpatient-management/floor-plan/**/*.{js,jsx}"`
   dari folder frontend; hasilnya harus 0 error.
3. **Cek visual** `?lantai=3`, `?lantai=4`, dan `?lantai=5` pada zoom 100% dan 300%. Pastikan label
   tidak menimpa pintu atau modul toilet, serta hover, pop up, dan garis merah kamar penuh masih
   benar.
4. `npm run build` opsional dan dijalankan sendiri oleh pemilik.

**Di luar cakupan:** API/database, base component, Lantai 2 (unit khusus R. Bayi/Perinatologi, ICU,
ICU VIP yang kelasnya sudah melekat pada nama unit), Lantai 1, 6, 7, dan 10 (tanpa kamar rawat inap),
serta penggantian kelas VIP/SVIP/Suite/Premium/Grand Royal/VIP Deluxe/VIP Superior.

## 8. Catatan untuk pemilik — tidak diubah sesuai instruksi

- **Lantai 8 dan 9 kemungkinan tertukar.** Beberapa petunjuk menempatkan *Sujudi Executive Ward*
  (Suite, Premium, Grand Royal, Day Room) di Lantai 9:
  - situs resmi menulis Suite Room di "Lantai IX";
  - denah Lantai 9 memuat DAY ROOM dan LOBBY ROOM, dan foto SEW di situs menampilkan lobi serta day
    room;
  - PT KAM menamai kamar *Aditama Executive Ward* "VIP 8.1–8.3".

  Kode sekarang kebalikannya: Lt 8 berisi Premium/Suite/Grand Royal dan Lt 9 berisi VIP. Bila ingin
  ditukar, keputusannya ada pada pemilik.
- **VIP di Lantai 3 dan 4.** Situs resmi menyebut VIP Deluxe di Lantai 3–5, sedangkan gambar menulis
  "WP"/"VP". Nama tetap VIP.

## 9. Register status pengerjaan

Pekerjaan ini adalah rencana yang disetujui pemilik di luar roadmap, sehingga tidak punya task ID.
Seperti pola dokumen `plan-repair`, bukti pengerjaannya dicatat di register ini. Seluruh path di
kolom bukti relatif terhadap
`QuilvianSystemFrontendDev/src/app/health-services/inpatient-management/floor-plan/`.

| ID | Tahap | Ringkasan | Area | Task ID | Status | Bukti |
|---|---|---|---|---|---|---|
| `KLS-DENAH-01` | 1 | Registri kelas dan pembentuk nama `<Kelas> <Nomor>`; 30 kamar "K." diberi kelas; `VIP Isolasi 319`; Lantai 8 dan 9 memakai registri yang sama | FE | — | SELESAI | `_data/room-classes.js:14,42,45`; `_data/lantai-3.plan.js:46,85-102`; `_data/lantai-4.plan.js:538,568-582`; `_data/lantai-5.plan.js:465,482-513`; `_data/lantai-8.plan.js:600`; `_data/lantai-9.plan.js:887` |
| `KLS-DENAH-02` | 2 | Kapasitas dummy mengikuti kelas: Kelas I 2 TT (520–522 cukup 1 TT), Kelas II 3 TT, Kelas III 6 TT | FE | — | SELESAI | `_data/lantai-3.occupancy.js:14-17`; `_data/lantai-4.occupancy.js:12-16`; `_data/lantai-5.occupancy.js:12-26` |
| `KLS-DENAH-03` | 3 | Label dua baris, warna grup kelas, legenda kelas per lantai, serta kelas dan okupansi di pop up | FE | — | SELESAI | `_components/floor-plan-room.jsx:83,97`; `_components/floor-plan-view.jsx:58,218`; `_components/room-name-dialog.jsx:19,86-109`; `_data/floors.js:50-57`; `page.jsx:9,36`; `floor-plan.module.css:58,65,315,713,871,888` |
| `KLS-DENAH-04` | 4 | Verifikasi: skrip cek data, ESLint, build produksi, cek visual dan pop up di peramban | FE | — | SELESAI | Bagian 10.4 |

**Ringkasan: 4 dari 4 tahap selesai** (source frontend `HamzahV2` @ `60f7764a`, belum di-commit).

## 10. Laporan implementasi — 15 Oktober 2026

### 10.1 Yang berubah di layar

- Semua kamar "K." kini tampil dengan kelasnya. Contoh: label lama `K. 329` di Lantai 3 menjadi
  `Kelas II` di baris atas dan `329` di baris bawah, dengan badge `3/3` tetap di tempatnya.
- Nama kelas berwarna menurut grup: Kelas I–III abu kebiruan, keluarga VIP biru, SVIP/Premium/Suite/
  Grand Royal perunggu, dan unit khusus (VIP Isolasi, ICU, Ruang Bayi) hijau. Saat kursor masuk,
  nama kelas berubah biru seperti sebelumnya, atau merah bila kamarnya penuh.
- Di atas denah muncul baris **Kelas kamar** berisi chip kelas yang ada di lantai itu beserta jumlah
  kamarnya. Contoh untuk Lantai 3: `Kelas II 9 kamar`, `Kelas I 1 kamar`, `VIP 17 kamar`,
  `SVIP 1 kamar`, `VIP Isolasi 1 kamar`. Urutannya mengikuti formulir RS MMC: Kelas III → II → I →
  VIP → kelas eksekutif → unit khusus.
- Pop up kamar kini menampilkan satu baris rincian di bawah nama. Contoh: klik Kelas II 329 →
  `● Kelas II | 3 tempat tidur · penuh`, dengan bagian okupansi berwarna merah karena kamarnya
  penuh. Garis aksen di atas pop up ikut berwarna kelas.
- Petunjuk di bilah atas menjadi "…lalu klik untuk melihat nama dan kelasnya."

### 10.2 Berkas yang diubah

| Berkas | Perubahan |
|---|---|
| `_data/room-classes.js` (baru) | Registri kelas, `roomName`, `roomLabelLines`, `roomClassInfo`, `classLegendFor` |
| `_data/lantai-3.plan.js`, `lantai-4.plan.js`, `lantai-5.plan.js` | Kelas per kamar, nama baku, posisi label dua baris |
| `_data/lantai-8.plan.js`, `lantai-9.plan.js` | Memakai registri (nama tidak berubah) |
| `_data/lantai-3.occupancy.js`, `lantai-4.occupancy.js`, `lantai-5.occupancy.js` | Kapasitas dummy per kelas |
| `_data/floors.js` | Kamar membawa `classLabel` dan `classGroup` |
| `page.jsx` | Legenda kelas dihitung di server lalu dikirim sebagai prop |
| `_components/floor-plan-room.jsx` | `data-room-group` dan kelas warna pada baris nama kelas |
| `_components/floor-plan-view.jsx` | Baris legenda **Kelas kamar** dan teks petunjuk |
| `_components/room-name-dialog.jsx` | Baris rincian kelas dan okupansi, `aria-describedby` |
| `floor-plan.module.css` | Token `--fp-class-*`, warna grup, gaya legenda, label kelas, dan rincian pop up |

### 10.3 Keputusan tampilan (UI GATE)

Pemilik memutuskan pada 15 Oktober 2026 bahwa denah tidak memakai base component Quilvian. Karena
itu, setiap elemen memperluas komponen lokal denah dan tidak ada komponen baru.

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Nama kelas pada label kamar (SVG `<text>`) | — (base component tidak berbentuk SVG) | `ls src/components/features/base-features/` | EXTEND lokal `FloorPlanRoom` | Baris pertama label diwarnai grup kelas |
| Legenda kelas per lantai | `StatusBadge` | `base-features/status-badge.jsx`; keputusan pemilik tanpa base component | EXTEND lokal `FloorPlanView` | Chip lokal di baris tersendiri |
| Kelas dan okupansi di pop up | — | — | EXTEND lokal `RoomNameDialog` | Satu baris rincian di bawah judul |

`UI GATE: 3 elemen — REUSE 0, EXTEND 3, COMPOSE 0, WRAP 0, NEW 0`

### 10.4 Validasi

| Pemeriksaan | Hasil |
|---|---|
| Skrip cek data (scratchpad, tidak di-commit) | PASS — 106 kamar di 6 lantai, 0 gagal. Kelas I 9 kamar / 15 TT, Kelas II 19 / 57, Kelas III 2 / 12. Nama berpola `<Kelas> <Nomor>`, tidak ada lagi "K.", kunci okupansi = ID kamar, jumlah di legenda = jumlah kamar |
| `npx eslint "src/app/health-services/inpatient-management/floor-plan/**/*.{js,jsx}"` | PASS — 0 error, 0 warning |
| `npm run lint:errors` (seluruh repository) | PASS — exit 0, 3 menit 51 detik |
| `npm run test:unit` | FAIL, `UNRELATED EXISTING ISSUE` — 2.870 test, 2.844 lulus, 26 gagal. Tak satu pun test yang gagal menyentuh denah, dan tidak ada test yang mengimpor modul `floor-plan`. Kegagalan ada di modul lain (pra-operasi bangsal, pemesanan kamar bedah, hemodialisa, keperawatan rawat inap, dan lain-lain). Garis dasar 13 Oktober 2026 sore: 2.832 / 2.802 / 30. Saat test dijalankan, working tree juga memuat perubahan lain di luar denah yang belum di-commit |
| `npm run build` | PASS — compiled 117 detik, 493 halaman, total 2 menit 48 detik; route denah dinamis (`ƒ`) |
| Grep anti-regresi UI pada baris yang ditambahkan | Tidak ada warna literal baru, `<button>`, `<table>`, utilitas tipografi Bootstrap, atau `!important`. Ukuran huruf baru hanya pada class lokal denah, mengikuti gaya modul ini |
| MANUAL TEST — build produksi di `127.0.0.1:3710`, Microsoft Edge lewat Playwright, sesi uji dan API tiruan seperti `tests/e2e/route-smoke.spec.mjs` | PASS — keenam lantai tampil tanpa page error. Label dan badge semua kamar di Lantai 3, 4, 5, 8, dan 9 berada di dalam batas kamar dengan jarak ≥ 3 px; hanya `R. ICU / CK` di Lantai 2 yang berjarak 1,5 px, label lama yang tidak diubah. Pop up diuji pada Kelas II 329 (penuh), VIP 302 (kosong), Kelas III 505 (3/6), dan Suite 812 (penuh), lengkap dengan hover dan tutup lewat Esc. Pada layar 390 px, legenda membungkus tanpa geser horizontal |
| `AUTOMATED TEST` | SKIPPED (opsional) — tidak ada test baru; perubahan berupa data tampilan dan komposisi view |

### 10.5 Penyimpangan dari rencana

1. **Posisi label.** Rencana menggeser label beberapa kamar (bagian 7, Tahap 3). Implementasinya
   lebih aman: nomor kamar tetap di posisi label lama, dan nama kelas ditambahkan satu baris di
   atasnya. Contoh: nomor 330 tetap di y 564 dan "Kelas I" berada di y 548, sehingga badge `2/1`
   tidak berpindah dan tetap tidak menabrak pintu.
2. **Kamar 520 dan 522** memakai satu baris `Kelas I 520` berukuran 10 (rencana ±10,5) agar muat di
   kamar setinggi 52–56 px.
3. **Warna grup eksekutif.** `--color-warning` murni hanya berkontras 3,6:1 di latar putih, di bawah
   batas teks kecil 4,5:1. Warnanya dicampur 80% dengan `--color-text-strong` lewat `color-mix()`
   sehingga tetap keemasan dan kontrasnya ±5:1. Grup lain memakai `--color-text-secondary`,
   `--color-info`, dan `--color-success` apa adanya.
4. **Lantai 2** ikut mendapat warna dan legenda grup khusus (ICU, ICU VIP, Ruang Bayi). Nama
   kamarnya tidak diubah.
5. **Laporan task.** Tidak ada berkas di `task/report/frontend/` karena pekerjaan ini tidak punya
   task ID roadmap; register di bagian 9 menggantikannya.

### 10.6 Risiko dan catatan

- Kelas per kamar masih berupa usulan berbasis ukuran (bagian 4). Bila RS MMC memberi daftar resmi,
  cukup ubah kata kelas pada daftar `ROOMS` di `lantai-*.plan.js` dan kapasitasnya di
  `lantai-*.occupancy.js`; ID kamar tidak perlu diubah.
- `color-mix()` membutuhkan peramban modern (Edge/Chrome 111 ke atas). Pada peramban lama, nama kelas
  grup eksekutif tampil berwarna tinta biasa dan titik warnanya di legenda tidak terlihat.
- Butir di bagian 6 dan 8 masih terbuka dan tidak menahan pekerjaan ini.

### 10.7 Langkah berikutnya

Pemilik meninjau tampilan `?lantai=3`, `?lantai=4`, dan `?lantai=5` di `next dev`, lalu melakukan
commit perubahan frontend dan dokumen ini.

## 11. Sumber

- Formulir persetujuan rawat inap, Section II "Pilihan Ruang Rawat", disusun dari lembar referensi RS
  MMC: `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-consent-form.jsx`.
- Denah sumber `lantai-1.png` sampai `lantai-10.png` di folder ini, beserta koordinat kamar di
  `floor-plan/_data/lantai-*.plan.js`.
- SIRS Kemenkes, profil RS Umum MMC (diakses 15-10-2026):
  <https://sirs.kemkes.go.id/fo/home/profile_rs/3171515>
- Situs resmi RS MMC, Fasilitas Medik: <https://mmchospital.co.id/fasilitas-medik> dan
  <https://mmchospital.co.id/fasilitas-medik/suite-room>
- PT Kosala Agung Metropolitan (pengelola RS MMC), Portofolio:
  <https://kosalaagungmetropolitan.com/portofolio/>
- Popmama, 2 November 2019, rekomendasi RS umum di Jakarta:
  <https://www.popmama.com/life/health/annas/rekomendasi-rumah-sakit-umum-di-jakarta-dengan-fasilitas-lengkap>
