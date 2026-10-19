# Planning Redesain Tampilan Denah Bed — "Peta Bangsal" RS MMC

**Tanggal:** 19 Oktober 2026 · **Status:** `DRAFT` — menunggu persetujuan pemilik sebelum eksekusi.
**Basis source:** frontend `QuilvianSystemFrontendDev` @ `a491b05af` (working tree bersih saat audit).
**Gambar acuan:** [UI-Sudah-Diterapkan/](../UI-Sudah-Diterapkan/) (Lantai 2, 3, 4, 5, 8, 9).

**Instruksi pemilik (19-10-2026):**

1. Tampilan jangan terlalu seperti denah arsitektur.
2. Kamar yang sudah diterapkan di database dan di gambar UI tetap dipertahankan.
3. Tampilan harus bagus dan menarik.

---

## 1. Ringkasan rekomendasi

Ganti gambar CAD di bawah kamar dengan **peta bangsal skematik**, seperti peta gedung di mal
atau bandara. Kamar tetap di posisi yang sama seperti sekarang, tetapi tampil sebagai **kartu
kamar** bersudut bulat yang berisi kelas, nomor kamar, titik status setiap bed, dan badge
kapasitas/terisi. Dinding, pintu, anak tangga, garis sumbu, angka ukuran, dan label ruang teknis
dihilangkan. Sebagai gantinya ada:

- satu **lempeng lantai** (bentuk gedung yang disederhanakan);
- **koridor** berupa jalur lembut;
- **penanda** berikon untuk Nurse Station, lift, tangga, pantry, toilet, dan ruang linen;
- **blok zona** abu-abu untuk unit lain, misalnya Kamar Operasi dan Hemodialisa di Lantai 2.

Yang tidak berubah: 106 kamar dan 168 bed, kunci `floorPlanKey` yang tersimpan di database,
data dari API, arti warna status, pop up kamar beserta semua aksinya, serta keempat pemakai denah
(Monitoring Bed, Pilih Bed admisi, transfer, Master Bed). **Backend dan database tidak disentuh.**

---

## 2. Kondisi sekarang (hasil audit)

### 2.1 Mengapa tampilannya terasa seperti denah arsitektur

Setiap lantai digambar ulang dari gambar CAD di `lib/constants/health-services/inpatient-management/floor-plan/lantai-N.plan.js`
dan dirender oleh `floor-plan-drawing.jsx`:

| Unsur CAD yang tampil | Sumber |
|---|---|
| Dinding dua lapis (tepi gelap + inti abu-abu), dinding tipis, fasad kaca | `layers`: `wallEdge`, `wallCore`, `thinWall*`, `glass`, `facade` |
| Simbol pintu berayun, anak tangga, shaft/lift bertanda silang, perabot | `layers`: `detail`, `fine`, `stripe`, `solid` |
| Garis sumbu bangunan (W–Z, 3–12) dan angka ukuran (8000, 8750, 23500) | `labels` bertipe `axis`/`dim` |
| Label ruang teknis (ELEC, LV. ROOM, A.H.U., TEL. ROOM, R. DUCTEX, M.V. SWITCHGEAR) | `labels` |
| Judul lembar berbahasa Inggris ("3rd Floor") | `sheetTitle` |
| Tekstur taman bertitik (Lantai 2) | `pattern#fp-garden-dots` |

Kamar sendiri hanya berupa garis tepi berwarna di atas gambar itu (`floor-plan-room.jsx`), sehingga
kamar sulit menonjol dari ratusan garis CAD. Ini sebabnya halaman terasa seperti lembar gambar
kerja, bukan alat kerja petugas.

### 2.2 Data kamar yang wajib dipertahankan

Hasil membaca registri `FLOORS` dan `roomsWithFloorPlanKeys`:

| Lantai | Kamar | Kelas di database (legenda UI) | Bentuk khusus |
|---|---:|---|---|
| 2 | 6 | Perinatologi 2 (BAYI-1, BAYI-2), ICU 4 (ICU, ICU-VIP-1, ICU-VIP-2, ICU-CK) | kamar bayi & ICU-CK sangat kecil |
| 3 | 29 | VIP 17, VIP Deluxe 1, ODC 5, Kelas II 5, Kelas I 1 | 301 & 318 berbentuk L; 321A/321B |
| 4 | 27 | VIP Deluxe 2, VIP 16, Kelas II 7, Kelas I 2 | 401 & 418 berbentuk L |
| 5 | 11 | Kelas I 6, Kelas III 2, Kelas II 3 | 506 berbentuk L; 520–522 kecil |
| 8 | 13 | VVIP 10, Luxury 1, Suite 2 | — |
| 9 | 20 | VIP Deluxe 6, VIP 14 | 908 & 909 berbentuk L |
| **Total** | **106** | 168 bed (rencana integrasi denah, bagian 1) | |

Ditambah 3 ruang cadangan yang hanya tampil di Master Bed (`FLOOR_PLAN_RESERVE_SPACES`: Lantai 3
dua ruang, Lantai 4 satu ruang).

Kamar dihubungkan ke database lewat `floorPlanKey` (mis. `lantai-3/k-329`) di
`floor-plan-room-map.js`. Nama kelas, nomor, kapasitas, status, dan bed **selalu dari API**
(`use-floor-plan.js` → `normalizeFloorPlanRoom`); label di file `.plan.js` hanya cadangan.
Jadi selama `floorPlanKey` dan koordinat kamar (`rooms[].outline`) tidak diubah, semua kamar di
database tetap tampil dan tetap bisa diklik.

### 2.3 Pemakai komponen denah

`FloorPlanView` + `FloorPlanDrawing` dipakai di tiga tempat, dengan empat mode:

| Pemakai | Berkas | Mode |
|---|---|---|
| Bed Management → Monitoring Bed | `floor-plan-monitoring.jsx` | `monitoring` |
| Admisi → Pilih Bed, transfer, workspace keperawatan | `floor-plan-selection.jsx` | `selection` |
| Master Data → Bed (denah) | `master-data-bed-floor-plan-view.jsx` | `master` |

Ketiganya mengirim `drawing={<FloorPlanDrawing plan={...} />}` ke `FloorPlanView`, sehingga
lapisan gambar bisa diganti di satu titik tanpa mengubah alur data.

---

## 3. Batasan yang tetap mengikat

| # | Batasan | Dasar |
|---|---|---|
| B-1 | 106 kamar, `floorPlanKey`, dan geometri `rooms[]` tidak diubah | instruksi pemilik butir 2; `floor-plan-room-map.js` |
| B-2 | Posisi kamar tetap mengikuti bangunan nyata (denah = patokan bentuk bangunan) | RWI-DEC-299 |
| B-3 | Arti status kamar dan legenda tetap: Bed tersedia, Dipesan, Penuh HD, Penuh LD, kapasitas/terisi | RWI-DEC-303, 316, source sekarang |
| B-4 | Badge "kapasitas/terisi" tetap ada pada setiap kamar | RWI-DEC-303 |
| B-5 | Tidak ada nama pasien, jenis kelamin, lencana tier, maupun Guid di peta; nama penghuni hanya di pop up seperti sekarang | RWI-DEC-287, 315, 341 |
| B-6 | Pop up kamar (`floor-plan-room-popup.jsx`) dan semua aksinya tidak diubah | RWI-DEC-325 contoh |
| B-7 | Kontrak API, hook, dan service tidak diubah; tidak ada perubahan backend/database | kontrak API denah yang berlaku (sejak `0.13.0`) |
| B-8 | Komponen baru tinggal di folder feature denah dengan CSS module sendiri; tidak membuat base component di `base-features` | RWI-DEC-325 butir 4 |

**Benturan yang perlu dicatat:** RWI-DEC-325 butir 3 berbunyi "tampilan floor plan yang sudah
didesain rapi tidak boleh diubah". Instruksi 19-10-2026 ini adalah perubahan yang diminta pemilik
sendiri, jadi perlu dicatat sebagai keputusan baru yang mengamandemen butir itu (lihat **D-1**).
Batasan B-2 tetap dipenuhi karena posisi kamar tidak berubah.

---

## 4. Pilihan konsep

| Opsi | Gambaran | Kelebihan | Kekurangan |
|---|---|---|---|
| **A. Peta bangsal skematik** *(direkomendasikan)* | Kamar menjadi kartu di posisi aslinya, di atas lempeng lantai + koridor + penanda berikon. CAD tidak dirender | Modern dan bersih; petugas tetap mengenali letak kamar; geometri dan kunci database dipakai ulang apa adanya | Perlu menyusun data skematik (lempeng, koridor, penanda) untuk 6 lantai |
| B. Papan kartu per sayap | Kamar dikelompokkan per sayap/kelas sebagai grid kartu, tanpa bentuk gedung | Paling mudah dibaca di layar kecil | Kehilangan orientasi ruang; melanggar RWI-DEC-299 (denah = patokan bentuk bangunan) |
| C. CAD dipudarkan | Gambar CAD tetap ada, opasitas 15–20 %, kamar diberi isi warna | Paling cepat dikerjakan | Masih terlihat seperti gambar kerja; tidak memenuhi instruksi butir 1 |

**Rekomendasi: Opsi A.** Hanya opsi ini yang memenuhi ketiga instruksi sekaligus tanpa melanggar
keputusan yang ada.

---

## 5. Desain visual Opsi A

### 5.1 Lapisan peta (dari bawah ke atas)

| Lapisan | Wujud | Interaksi |
|---|---|---|
| Kanvas | latar `#F4F7FB` dengan pola titik halus 24 px | geser & zoom seperti sekarang |
| Lempeng lantai | bentuk gedung disederhanakan (poligon 4–8 titik), putih, sudut bulat 18, bayangan lembut, garis tepi 1 px `#E2E8F0` | tidak ada |
| Blok zona | ruang non-rawat-inap yang besar (OK, Hemodialisa, Cath Lab, Poli, ruang rapat). Isi `#EEF2F6`, teks abu `#64748B`, sudut bulat 10 | tidak bisa diklik, kursor biasa |
| Koridor | jalur `#F1F5F9` di antara baris kamar, garis putus-putus di tengah, tulisan kecil "KORIDOR" berjarak huruf lebar | tidak ada |
| Penanda | lingkaran putih 26 px berikon + teks kecil di bawahnya | `title` untuk tooltip bawaan |
| Kartu kamar | lihat 5.2 | hover, fokus, klik → pop up |

### 5.2 Anatomi kartu kamar

```
 normal (lebar ≥ 56)              ringkas (kamar kecil < 56)
╭───────────────╮                 ╭────────────╮
│▀▀▀▀▀▀▀▀▀▀▀▀▀▀▀│ ← strip kelas    │▀▀▀▀▀▀▀▀▀▀▀▀│
│   KELAS II    │ ← kelas, 10,     │ BAYI-1 1/0 │ ← nomor + badge satu baris
│               │   kapital        │   ▪        │ ← pip bed
│      329      │ ← nomor 20–22,   ╰────────────╯
│               │   tebal
│    ▪ ▪ ▫      │ ← 1 pip = 1 bed
│     3/2       │ ← badge kapasitas/terisi (B-4)
╰───────────────╯
```

- Bentuk kartu = poligon kamar yang ada, **dimasukkan 3 unit ke dalam** pada setiap sisi dan diberi
  sudut bulat 8. Celah antarkartu menggantikan garis dinding. Kamar berbentuk L tetap berbentuk L.
- Isi teks ditempatkan di tengah kotak terbesar pada poligon (untuk kamar L: lengan yang lebih
  besar), bukan memakai koordinat label CAD.
- **Pip bed:** satu kotak kecil 7×7 per bed, warnanya mengikuti nada status bed yang sama dengan
  pop up (`statusTone`: tersedia, terisi HD/LD, dipesan, menunggu/dalam pembersihan, tidak
  tersedia). Bila bed lebih dari 6 (ICU), pip disembunyikan dan hanya badge yang tampil. Pip hanya
  memuat status, bukan identitas pasien (B-5).
- Kamar ringkas: BAYI-1, BAYI-2, ICU-CK, 520, 521, 522 (lebar atau tinggi < 56 unit).

### 5.3 Status kamar → tampilan kartu

Arti dan prioritas status mengikuti `floor-plan-room.jsx` sekarang. Yang berubah hanya wujudnya,
dari garis tepi di atas CAD menjadi kartu berisi warna.

| Kondisi (source sekarang) | Tampilan kartu baru |
|---|---|
| Bed tersedia (`isSelectable`) | isi putih, tepi 2 px warna primer (teal), titik "hidup" kecil di sudut kanan atas |
| Dipesan, menunggu masuk (`hasActiveReservation`) | isi ungu 8 %, tepi ungu, ikon jam kecil |
| Penuh, dominan High Dependency | isi oranye 12 %, tepi oranye |
| Penuh, dominan Low Dependency | isi hijau 12 %, tepi hijau |
| Tidak ada bed yang bisa dipakai (persiapan / tidak tersedia) | isi abu `#F1F5F9`, tepi abu, teks redup |
| Mode Pilih Bed: kamar tidak layak untuk pasien ini | opasitas 0,38 + desaturasi, tidak bisa difokus (perilaku sekarang) |
| Mode Pilih Bed: kamar terpilih | isi amber 22 %, halo amber, ikon centang |
| Kamar yang pop up-nya sedang terbuka | kartu terangkat (bayangan lebih dalam) + cincin fokus |
| Master: ruang cadangan | tepi putus-putus, teks "Ruang cadangan" |

Legenda di bilah atas tetap berisi butir yang sama; contoh warnanya diganti menjadi kotak mini
berbentuk kartu agar cocok dengan peta.

### 5.4 Warna grup kelas

Strip atas dan teks kelas memakai warna grup kelas (`floorPlanClassGroup`):

| Grup | Kelas | Sekarang | Usulan |
|---|---|---|---|
| `umum` | Kelas I/II/III | abu teks sekunder | slate `#64748B` (tetap) |
| `vip` | VIP, VIP Deluxe, VVIP | biru info | biru `#2563EB` (tetap) |
| `eksekutif` | Suite, Luxury | oranye gelap | emas `#A16207` |
| `khusus` | ICU, ODC, Perinatologi, Isolasi | **hijau** | **magenta `#BE185D`** |

Grup `khusus` saat ini hijau, sama dengan warna "Penuh (Low Dependency)", sehingga kamar ODC
bisa dikira penuh-LD. Usulan perubahan ini ada di **D-3**.

### 5.5 Penanda (landmark)

Hanya penanda yang membantu orientasi petugas yang ditampilkan. Ikon diambil dari `lucide-react`
0.483 yang sudah terpasang.

| Penanda | Ikon | Contoh di gambar |
|---|---|---|
| Nurse Station | `Stethoscope` | NS3A, NS3B, NS A/B, NS Lt 5 A, Nurse Station, R. NS Bayi |
| Lift | `ArrowUpDown` | kotak silang shaft lift |
| Tangga | `Footprints` | anak tangga, DN/UP |
| Pantry | `Coffee` | Pantry LT. 3, PANTRY |
| Toilet / kamar mandi | `Toilet` | Male Toilet, Toilet & Shower |
| Linen / gudang | `Shirt` / `Warehouse` | Gudang Linen, Store, Ka. dep Peralatan |
| Spoel hoek / kebersihan | `Droplets` | Spoel Hook, Scoot Hook, Cleaner Room |
| Ruang dokter | `BriefcaseMedical` | R. Dokter, R. Doctor Room |
| Ruang tunggu / day room / lobby | `Sofa` | Ruang Tunggu, Day Room, Lobby Room |

**Tidak ditampilkan:** ELEC, LV. ROOM, A.H.U., TEL. ROOM, IT ROOM, R. DUCTEX, F.S., M.V.
SWITCHGEAR, PRINT ROOM, angka ukuran, garis sumbu, judul lembar bahasa Inggris, dan tekstur taman.

### 5.6 Kepala halaman dan bilah alat

```
┌────────────────────────────────────────────────────────────────────────────────────────┐
│ [ikon] RAWAT INAP · DENAH RUANGAN                                                      │
│        Lantai 3                                                                        │
│ Lantai 2 (0) | [Lantai 3 (12)] | Lantai 4 (8) | Lantai 5 (3) | Lantai 8 | Lantai 9     │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 58 bed | ● 21 tersedia | ● 30 terisi | ● 2 dipesan | ● 3 persiapan | ● 2 tdk tersedia  │
│ Kelas: (VIP 17) (VIP Deluxe 1) (ODC 5) (Kelas II 5) (Kelas I 1)                        │
│ [ ] Hanya kamar dengan bed tersedia     [Cari nomor kamar...]    [ - 100% + ] [⤢]      │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

*(angka di atas hanya contoh; angka dalam kurung pada tab lantai = jumlah bed kosong)*

- **Badge bed kosong pada tab lantai**: dari `floors[].availableBedCount` yang sudah dikirim API.
- **Ringkasan lantai**: dihitung di frontend dari `rooms[].beds[].statusCode` lantai aktif. Pada
  mode Pilih Bed diganti dengan "N kamar cocok untuk pasien ini".
- **Chip kelas jadi filter**: klik chip → kamar kelas lain diredupkan (bukan disembunyikan, agar
  orientasi tetap utuh). Klik lagi untuk membatalkan.
- **Hanya kamar tersedia**: meredupkan kamar tanpa bed tersedia.
- **Cari kamar**: ketik nomor (mis. `329`) → peta zoom dan geser ke kamar itu
  (`zoomToElement` dari `react-zoom-pan-pinch` yang sudah dipakai), lalu kartu berdenyut sekali.
- Zoom, geser, Ctrl + Scroll, layar penuh, dan kompas tetap ada. Kompas diperkecil dan disamakan
  gayanya dengan kartu.

### 5.7 Interaksi dan gerak

- **Hover/fokus kartu**: kartu naik 2 px, bayangan menebal, lalu muncul kartu mengambang kecil
  berisi kelas, nomor, jumlah bed per status, dan status kamar. Tidak memuat nama pasien (B-5).
- **Klik/Enter/Spasi**: membuka pop up yang sama seperti sekarang. Batas toleransi geser 6 px
  dipertahankan.
- **Ganti lantai**: kartu muncul bertahap (fade + naik 4 px, jeda 12 ms per kartu, maks 300 ms).
- **`prefers-reduced-motion`**: semua animasi dimatikan.
- Semua transisi memakai CSS, tidak memakai framer-motion di dalam SVG, agar ringan.

### 5.8 Sketsa Lantai 3

```
╭────────────────────────────────────────────────────────────────────────────────────────╮
│  [Lift] [Lift]      [Pantry]        [Toilet] [Spoel]                                   │
│ ┌────┬────┬────┬────┬────┬────┐                             ┌────┬────┬────┬────┬────┐ │
│ │ I  │ II │ II │ II │ II │ II │  (NS3A)   [Peralatan]       │ODC │ODC │ODC │ODC │ODC │ │
│ │330 │329 │328 │327 │326 │325 │             (NS3B)          │322 │321A│321B│320 │319 │ │
│ │▪▫  │▪▪▫ │▪▪▪ │▪▪▫ │▪▪▪ │▪▪▪ │                             │▪▪▪ │▪▪▪ │▪▪▫ │▪▪  │▪   │ │
│ └────┴────┴────┴────┴────┴────┘                             └────┴────┴────┴────┴────┘ │
│ ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄  K O R I D O R  ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄ │
│ ┌──────┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬────────┐      │
│ │ VIP  │VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP│VIP DLX │      │
│ │ 301  │302│303│304│305│306│307│308│309│310│311│312│313│314│315│316│317│  318   │      │
│ └──────┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴────────┘      │
╰────────────────────────────────────────────────────────────────────────────────────────╯
```

Sketsa hanya menunjukkan susunan. Letak persisnya mengikuti koordinat kamar di `lantai-3.plan.js`.

### 5.9 Perlakuan per lantai

| Lantai | Perlakuan khusus |
|---|---|
| 2 | Sebagian besar lantai bukan rawat inap. Tampilkan blok zona bernama (Kamar Operasi/OK 1–3, Hemodialisa, Persiapan Endoscopy, Cath Lab, CSSD, Farmasi, Kafetaria, Ruang Tunggu) dengan nada abu, lalu 6 kamar Perinatologi/ICU menonjol. Taman bertekstur diganti blok hijau-pucat polos berlabel "Taman" |
| 3 | Dua baris kamar + koridor tengah. Inti layanan (lift, NS3A/NS3B, pantry) di tengah baris atas |
| 4 | Pola sama dengan Lantai 3. Meeting/Print/IT Room cukup jadi satu blok zona "Ruang staf" |
| 5 | Kamar hanya di sayap barat dan pojok tenggara (520–522). Poli 22/23/24/26 dan R. Medik jadi blok zona "Rawat jalan & administrasi" agar ruang kosong tidak terlihat hampa |
| 8 | Dua baris VVIP + Luxury 811 dan Suite 812–813 di sisi barat. Koridor lebar di tengah; lobby lift service jadi penanda |
| 9 | Dua baris + kolom barat (918–920) dan timur (908–909). Day Room dan Lobby Room jadi penanda `Sofa` |

---

## 6. Rancangan teknis

### 6.1 Data skematik per lantai (berkas baru)

File CAD `lantai-N.plan.js` **tidak diubah**, karena `rooms[]` di dalamnya adalah geometri kamar
yang dipakai registri `floorPlanKey`, manifest impor master, dan editor master. Data skematik
disimpan terpisah:

```
lib/constants/health-services/inpatient-management/floor-plan/
├── schematic/
│   ├── schematic-geometry.js      ← insetOrthogonal(), roundedOrthogonalPath(), largestRect()
│   ├── lantai-2.schematic.js
│   ├── lantai-3.schematic.js
│   ├── lantai-4.schematic.js
│   ├── lantai-5.schematic.js
│   ├── lantai-8.schematic.js
│   └── lantai-9.schematic.js
└── floors.js                      ← tiap lantai mendapat properti `schematic`
```

Bentuk data (satuan sama dengan gambar sumber 1536 × 1024):

```js
export const LANTAI_3_SCHEMATIC = Object.freeze({
  viewBox: { x: 56, y: 230, width: 1424, height: 700 }, // dipangkas ke area gedung
  footprint: [[82, 300], [1436, 300], [1436, 907], [82, 907]],
  corridors: [{ x: 148, y: 697.5, width: 1280, height: 66.5, label: "Koridor" }],
  zones: [{ name: "Ka. dep Peralatan", points: [/* ... */] }],
  landmarks: [
    { type: "nurse-station", label: "NS3A", x: 655, y: 640 },
    { type: "lift", x: 735, y: 610 },
  ],
});
```

Perkiraan awal koridor dari geometri kamar (dicocokkan ulang dengan gambar sumber saat eksekusi):

| Lantai | Baris atas (y) | Koridor (y) | Baris bawah (y) |
|---|---|---|---|
| 3 | 486–697,5 | 697,5–764 | 764–907 |
| 4 | 459,5–679,5 | 679,5–747 | 747–895 |
| 5 | 426–643,5 | 643,5–706 | 706–861 |
| 8 | 299–472 | 472–616,5 | 616,5–794,5 |
| 9 | 292–472 | 472–614,5 | 614,5–798 |
| 2 | kamar tersebar; koridor ditelusuri dari gambar sumber | | |

`viewBox` dipangkas ke area gedung sehingga peta mengisi panggung lebih penuh dan kartu tampil
lebih besar pada zoom awal.

### 6.2 Komponen

| Berkas | Tindakan | Isi |
|---|---|---|
| `floor-plan-schematic.jsx` | **baru** | Lapisan statis: kanvas, lempeng, zona, koridor, penanda. Tanpa state, menggantikan `FloorPlanDrawing` di ketiga pemakai |
| `floor-plan-room.jsx` | **tulis ulang render** | Kartu kamar (5.2–5.3). Props, `role="button"`, `aria-label` (`describeRoom`), seluruh atribut `data-room-*`, dan logika status HD/LD/dipesan **tetap** |
| `floor-plan-room-hovercard.jsx` | **baru** | Kartu mengambang saat hover/fokus, dirender sebagai HTML di atas SVG |
| `floor-plan-summary.jsx` | **baru** | Ringkasan lantai + chip filter kelas + toggle "hanya tersedia" + cari kamar |
| `floor-plan-view.jsx` | ubah | Pasang ringkasan/filter, `viewBox` dari skematik, hapus `sheetTitle`, teruskan status filter ke kartu, badge bed kosong di tab |
| `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, `master-data-bed-floor-plan-view.jsx` | ubah 1 baris masing-masing | `drawing={<FloorPlanSchematic ... />}` |
| `floor-plan-skeleton.jsx` | ubah | Kerangka berbentuk kartu ber-shimmer, bukan kerangka CAD |
| `floor-plan-compass.jsx` | ubah gaya | Lebih kecil, selaras dengan kartu |
| `floor-plan.module.css` | ubah | Token baru (5.1–5.4), gaya kartu & lapisan. Gaya CAD dihapus pada fase 5 setelah tidak ada pemakai |
| `floor-plan-drawing.jsx`, `lantai-N.plan.js` (`layers`, `labels`) | **tidak dihapus** pada pekerjaan ini | Disimpan sebagai acuan penelusuran; penghapusan menjadi keputusan terpisah |
| `floor-plan-room-popup.jsx`, `floor-plan-master-room-popup.jsx`, semua hook & service | **tidak diubah** | B-6, B-7 |

### 6.3 Fungsi geometri inti

- `insetOrthogonal(points, d)`: semua kamar bersisi tegak lurus (persegi atau L). Setiap titik
  digeser ke dalam sejauh `d` menurut normal kedua sisinya. Rumus ini berlaku untuk sudut cembung
  maupun sudut dalam pada bentuk L.
- `roundedOrthogonalPath(points, r)`: path SVG dengan busur di tiap sudut; `r` dibatasi setengah
  sisi terpendek supaya kamar kecil tidak rusak.
- `largestRect(points)`: kotak terbesar di dalam poligon, sebagai tempat teks dan pip.

Ketiganya murni JavaScript seperti `plan-primitives.js`, sehingga bisa diuji dengan skrip Node.

---

## 7. Tahapan eksekusi

| Fase | Isi | Hasil yang bisa dilihat | Ukuran |
|---|---|---|---|
| **0. Persiapan** | Catat D-1 s.d. D-6 di decision log (nomor berikutnya saat ini `RWI-DEC-343`, grep ulang sebelum menulis). Cek `git status` frontend karena working tree dipakai paralel | Keputusan tercatat | S |
| **1. Fondasi geometri** | `schematic-geometry.js` + 6 berkas `lantai-N.schematic.js` + daftar di `floors.js`. Skrip Node memeriksa: 106 kamar punya kartu, kartu tidak menimpa koridor/penanda, teks muat di kartu | Data skematik lengkap, lolos skrip cek | L |
| **2. Kartu kamar** | Tulis ulang render `floor-plan-room.jsx` (kartu, varian ringkas, pip, status, warna kelas) | Kamar tampil sebagai kartu, masih di atas CAD lama untuk perbandingan | M |
| **3. Lapisan peta** | `floor-plan-schematic.jsx`, ganti `drawing` di 3 pemakai, pangkas `viewBox`, hapus judul lembar, kompas & skeleton baru | CAD hilang, peta bangsal utuh di keenam lantai | M |
| **4. Kepala & interaksi** | Ringkasan lantai, badge tab, filter kelas, toggle tersedia, cari kamar, kartu hover, animasi masuk | Halaman Monitoring Bed lengkap | M |
| **5. Poles & verifikasi** | Pencocokan visual per lantai dengan gambar acuan, kontras teks, keyboard, mode Pilih Bed & Master, hapus gaya CAD yang tidak terpakai, perbarui frontend architecture 16.x & memori | Siap dipakai | S |

Bila pemilik ingin pekerjaan ini masuk roadmap, ID task frontend berikutnya yang bebas saat ini
`FE-RWI-272` (grep global ulang sebelum alokasi).

---

## 8. Kriteria penerimaan

| # | Kriteria |
|---|---|
| AC-1 | Di keenam lantai tidak ada lagi dinding CAD, simbol pintu, anak tangga, garis sumbu, angka ukuran, label ruang teknis, maupun judul lembar berbahasa Inggris |
| AC-2 | Jumlah kartu per lantai sama dengan tabel 2.2 (6, 29, 27, 11, 13, 20 = 106). Setiap kartu memuat kelas dan nomor dari API, sama dengan legenda kelas |
| AC-3 | Posisi setiap kartu berada di dalam poligon kamar aslinya; kamar berbentuk L tetap berbentuk L |
| AC-4 | Status Bed tersedia, Dipesan, Penuh HD, dan Penuh LD tampil sesuai tabel 5.3 dan cocok dengan legenda |
| AC-5 | Badge kapasitas/terisi tetap ada di setiap kartu; pip bed sama jumlahnya dengan bed kamar (≤ 6) |
| AC-6 | Klik, Enter, atau Spasi pada kartu membuka pop up kamar yang sama seperti sekarang; menyeret peta tidak membuka pop up |
| AC-7 | Mode Pilih Bed: kamar tidak layak redup dan tidak bisa difokus; kamar terpilih bersorot amber. Mode Master: ruang cadangan tampil bertepi putus-putus |
| AC-8 | Kartu hover dan ringkasan lantai tidak memuat nama pasien, jenis kelamin, lencana tier, atau Guid |
| AC-9 | Filter kelas, "hanya tersedia", dan cari kamar bekerja tanpa permintaan API baru |
| AC-10 | Teks nomor kamar terbaca pada zoom awal (≥ 12 px di layar 1366 px), kontras teks ≥ 4,5 : 1, animasi mati bila `prefers-reduced-motion` |
| AC-11 | Tidak ada perubahan pada hook, service, kontrak API, backend, maupun database |
| AC-12 | `lint:errors` bersih untuk berkas yang disentuh; `build` dijalankan pemilik (dicatat `NOT RUN` bila belum) |

---

## 9. Risiko dan mitigasi

| Risiko | Mitigasi |
|---|---|
| Petugas yang sudah hafal denah lama kehilangan patokan | Posisi kamar tidak berubah; penanda NS, lift, dan tangga tetap di tempatnya; label koridor membantu membaca arah |
| Kamar kecil (BAYI-1/2, ICU-CK, 520–522) tidak muat teks | Varian ringkas otomatis; zoom ke kamar lewat pencarian |
| Lantai 2 dan 5 tampak kosong karena sedikit kamar | Blok zona bernama untuk unit lain (5.9) |
| Bentrok warna kelas khusus (hijau) dengan Penuh LD (hijau) | D-3: kelas khusus menjadi magenta |
| Pekerjaan paralel mengubah berkas denah di tengah eksekusi | Cek `git status` sebelum tiap fase; tidak menyentuh berkas di luar daftar 6.2 |
| Kinerja SVG | Justru lebih ringan: ribuan segmen CAD diganti puluhan bentuk. Transisi memakai CSS saja |

---

## 10. Keputusan yang diminta dari pemilik

Bila planning ini disetujui tanpa catatan, kolom **Default** yang dipakai.

| # | Keputusan | Default |
|---|---|---|
| D-1 | Amandemen RWI-DEC-325 butir 3 dan RWI-DEC-299: **tampilan** denah boleh diganti menjadi peta bangsal skematik; posisi, bentuk dasar, dan identitas kamar tetap | Setuju (ini isi instruksi 19-10-2026) |
| D-2 | Pip bed per kamar ditambahkan di samping badge kapasitas/terisi | Ya |
| D-3 | Warna grup kelas `khusus` (ICU, ODC, Perinatologi, Isolasi) diganti dari hijau ke magenta | Ya |
| D-4 | Unit non-rawat-inap tampil sebagai blok zona bernama, tanpa detail ruang | Ya |
| D-5 | Ringkasan lantai, filter kelas, "hanya tersedia", dan cari kamar masuk lingkup (fase 4) | Ya |
| D-6 | Tombol "tampilkan denah teknis (CAD)" | Tidak. Data CAD disimpan, tidak dirender |
