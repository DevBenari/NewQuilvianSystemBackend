# Planning Redesain Tampilan Denah Bed — "Peta Bangsal" RS MMC

**Tanggal:** 19 Oktober 2026, diperbarui 20 Oktober 2026 · **Status:** `DISETUJUI` — keputusan
pemilik `RWI-DEC-343` s.d. `RWI-DEC-353` di [decision log](../../../../00-interview-decisions.md)
revision 62–63 (ringkasan di bagian 10). Desain amandemennya (`0.18.0`) disetujui `RWI-DEC-352` di
[frontend architecture bagian 21](../../../03-frontend-architecture.md), PRD bagian 32, dan acceptance
test bagian 26. Task: [roadmap frontend](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md)
`FE-RWI-272` s.d. `FE-RWI-279` (planning `PETA-DEL-20261020-01`).
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
data dari API, aturan status kamar, pop up kamar beserta semua aksinya, serta keempat pemakai denah
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
| B-3 | Aturan status kamar tetap; rupanya mengikuti tabel 5.3: bed tersedia hijau, pesanan ungu, penuh abu, label HD/LD, badge kapasitas/terisi | RWI-DEC-303, 316, 346 |
| B-4 | Badge "kapasitas/terisi" tetap ada pada setiap kamar | RWI-DEC-303 |
| B-5 | Tidak ada nama pasien, jenis kelamin, lencana tier, maupun Guid di peta; nama penghuni hanya di pop up seperti sekarang | RWI-DEC-287, 315, 341 |
| B-6 | Pop up kamar (`floor-plan-room-popup.jsx`) dan semua aksinya tidak diubah | RWI-DEC-325 contoh |
| B-7 | Kontrak API, hook, dan service tidak diubah; tidak ada perubahan backend/database | kontrak API denah yang berlaku (sejak `0.13.0`) |
| B-8 | Komponen baru tinggal di folder feature denah dengan CSS module sendiri; tidak membuat base component di `base-features` | RWI-DEC-325 butir 4 |

**Benturan yang sudah diselesaikan:** RWI-DEC-325 butir 3 berbunyi "tampilan floor plan yang sudah
didesain rapi tidak boleh diubah". Atas arahan atasan, pemilik mencatat perubahan ini atas namanya
sendiri sebagai `RWI-DEC-343` (20-10-2026), yang menggantikan butir 3 itu. Batasan B-2 tetap
dipenuhi karena posisi kamar tidak berubah.

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

### 5.3 Status kamar → tampilan kartu (`RWI-DEC-346`, `RWI-DEC-350`)

Aturan status mengikuti `RWI-DEC-303` dan `RWI-DEC-316`. Rupa yang dipakai sejak commit frontend
`685844a34` — tepi teal, kamar penuh berwarna oranye/hijau, dan aturan suara terbanyak untuk HD/LD —
tidak dipakai lagi (`RWI-CON-018`).

| Keadaan kamar | Tampilan kartu baru |
|---|---|
| Masih punya bed tersedia (`isSelectable`) | isi putih, tepi 2 px **hijau** |
| Masih punya bed tersedia dan ada pesanan menunggu masuk | tepi tetap hijau, ditambah ikon jam **ungu** kecil |
| Tidak punya bed tersedia, ada pesanan menunggu masuk (`hasActiveReservation`) | isi ungu 8 %, tepi **ungu**, ikon jam |
| Penuh tanpa pesanan | isi **abu netral** `#E8EDF3`, tanpa tepi berwarna, teks tetap tegas, tetap bisa diklik |
| Penuh, sekurang-kurangnya satu penghuni HD (`dependencySummary.isHighDependency`) | label kecil **"HD" oranye** di kartu |
| Penuh, tanpa penghuni HD, sekurang-kurangnya satu penghuni LD | label kecil **"LD" abu** |
| Penuh, semua penghuni belum dinilai | tanpa label HD/LD (`RWI-DEC-350`) |
| Tidak ada bed yang bisa dipakai karena persiapan atau tidak tersedia | isi abu muda `#F6F8FA`, tepi abu putus-putus, teks redup |
| Mode Pilih Bed: kamar tidak layak untuk pasien ini | opasitas 0,38 + desaturasi, tidak bisa difokus (perilaku sekarang) |
| Mode Pilih Bed: kamar terpilih | isi amber 22 %, halo amber, ikon centang |
| Kamar yang pop up-nya sedang terbuka | kartu terangkat (bayangan lebih dalam) + cincin fokus |
| Master: ruang cadangan | tepi putus-putus, teks "Ruang cadangan" |

Legenda di bilah atas menjadi: Bed tersedia (hijau), Pesanan menunggu masuk (ungu), Penuh (abu),
label HD/LD, dan Kapasitas/terisi. Contoh warnanya berbentuk kotak mini seperti kartu.

**Contoh:** kamar 329 penuh berisi 1 pasien HD dan 2 pasien LD tampil abu dengan label "HD" dan badge
`3/3`. Kamar 328 berisi Terisi, Dipesan, Tersedia tampil bertepi hijau dengan ikon pesanan ungu dan
badge `3/2`; setelah bed terakhirnya dipesan, tepinya berubah ungu dan badge menjadi `3/3`.

### 5.4 Warna grup kelas

Strip atas dan teks kelas memakai warna grup kelas (`floorPlanClassGroup`):

| Grup | Kelas | Sekarang | Usulan |
|---|---|---|---|
| `umum` | Kelas I/II/III | abu teks sekunder | slate `#64748B` (tetap) |
| `vip` | VIP, VIP Deluxe, VVIP | biru info | biru `#2563EB` (tetap) |
| `eksekutif` | Suite, Luxury | oranye gelap | emas `#A16207` |
| `khusus` | ICU, ODC, Perinatologi, Isolasi | **hijau** | **magenta `#BE185D`** |

Grup `khusus` saat ini hijau, padahal hijau kini berarti "bed tersedia" (`RWI-DEC-346`). Warna
kelompok kelas adalah `DEV_DISCRETION` (`RWI-FE-009`) dengan syarat jelas berbeda dari hijau, ungu,
dan oranye. Emas untuk `eksekutif` perlu dicek agar tidak mirip label HD oranye.

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
│ Lantai 2 (0) | [Lantai 3 (20)] | Lantai 4 (8) | Lantai 5 (3) | Lantai 8 | Lantai 9     │
├────────────────────────────────────────────────────────────────────────────────────────┤
│ 48 bed | ● 20 tersedia | ● 22 terisi | ● 2 dipesan | ● 3 persiapan | ● 1 tdk tersedia  │
│ Kelas: (VIP 17) (VIP Deluxe 1) (ODC 5) (Kelas II 5) (Kelas I 1)                        │
│ Legenda: [hijau] tersedia  [ungu] pesanan  [abu] penuh  [HD]/[LD]   [ - 100% + ] [⤢]   │
└────────────────────────────────────────────────────────────────────────────────────────┘
```

*(angka di atas hanya contoh; angka dalam kurung pada tab lantai = jumlah bed kosong)*

- **Badge bed kosong pada tab lantai** (`RWI-DEC-348`): dari `floors[].availableBedCount` yang sudah
  dikirim API.
- **Ringkasan lantai** (`RWI-DEC-348`): dihitung di frontend dari `rooms[].beds[].statusCode` lantai
  aktif. Persiapan = menunggu pembersihan + dalam pembersihan. Jumlah semua angka sama dengan total
  bed lantai itu, misalnya 48 untuk Lantai 3.
- **Chip kelas** tetap tampil sebagai legenda kelas, bukan filter.
- **Ditunda, bukan dibatalkan** (`RWI-DEC-348`): filter kelas, pilihan "hanya kamar tersedia", dan
  pencarian nomor kamar. Ketiganya dibahas lagi setelah atasan mengesahkan tampilan baru.
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
| `floor-plan-room.jsx` | **tulis ulang render** | Kartu kamar (5.2–5.3). Props, `role="button"`, dan seluruh atribut `data-room-*` **tetap**. Logika status mengikuti `RWI-DEC-346`: label HD dari `dependencySummary.isHighDependency`, bukan suara terbanyak; kalimat `describeRoom` untuk pembaca layar ikut disesuaikan |
| `floor-plan-room-hovercard.jsx` | **baru** | Kartu mengambang saat hover/fokus, dirender sebagai HTML di atas SVG |
| `floor-plan-summary.jsx` | **baru** | Ringkasan bed lantai aktif (`RWI-DEC-348`); chip kelas tetap sebagai legenda |
| `floor-plan-view.jsx` | ubah | Pasang ringkasan, `viewBox` dari skematik, hapus `sheetTitle`, badge bed kosong di tab. Lantai yang belum punya data skematik tetap memakai gambar lama selama masa contoh (`RWI-DEC-351`) |
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

Urutan mengikuti `RWI-DEC-349`: Lantai 3 dikerjakan lebih dulu sebagai contoh, disahkan atasan,
baru lima lantai lain.

| Fase | Isi | Hasil yang bisa dilihat | Ukuran |
|---|---|---|---|
| **0. Persiapan** | Amandemen frontend architecture 16.7 lewat `design-business-module`, lalu task roadmap lewat `plan-module-delivery` (ID frontend berikutnya `FE-RWI-272`, grep ulang sebelum alokasi). Cek `git status` frontend karena working tree dipakai paralel | Dokumen desain dan task terdaftar | S |
| **1. Fondasi geometri + data Lantai 3** | `schematic-geometry.js`, `lantai-3.schematic.js`, dan pendaftarannya di `floors.js`. Skrip Node memeriksa 29 kamar Lantai 3: setiap kamar punya kartu, kartu tidak menimpa koridor/penanda, teks muat | Data Lantai 3 lolos skrip cek | M |
| **2. Kartu kamar** | Tulis ulang render `floor-plan-room.jsx`: kartu, varian ringkas, titik bed, rupa status `RWI-DEC-346`, warna kelas | Kamar Lantai 3 tampil sebagai kartu | M |
| **3. Lapisan peta + kepala halaman** | `floor-plan-schematic.jsx`, ganti `drawing` di 3 pemakai, pangkas `viewBox`, hapus judul lembar, ringkasan lantai, badge tab, kartu hover, kompas & skeleton baru. Lantai lain sementara memakai gambar lama (`RWI-DEC-351`) | Lantai 3 utuh di keempat pemakai | M |
| **4. Pengesahan contoh** | Lantai 3 ditunjukkan ke atasan. Revisi yang diminta dikerjakan di Lantai 3 dulu. Muhammad Hamzah mencatat pengesahannya sebagai keputusan baru di decision log | **Gate** sebelum fase 5 | S |
| **5. Lima lantai lain** | `lantai-2`, `4`, `5`, `8`, `9.schematic.js` beserta blok zona dan penanda; skrip cek untuk 77 kamar sisanya | Keenam lantai menjadi peta bangsal | L |
| **6. Poles & verifikasi** | Pencocokan visual per lantai dengan gambar acuan, kontras teks, keyboard, mode Pilih Bed & Master, hapus gaya CAD yang tidak terpakai, perbarui memori. Rilis ke pengguna rumah sakit setelah keenam lantai selesai (`RWI-DEC-351`) | Siap dipakai | S |

Pemetaan ke roadmap `PETA-DEL-20261020-01`: fase 1 dan lapisan peta fase 3 menjadi `FE-RWI-272`; fase 2
menjadi `FE-RWI-273`; kepala halaman fase 3 menjadi `FE-RWI-274`; palet titik `RWI-DEC-353` menjadi
`FE-RWI-275`; fase 4 menjadi gerbang `DEC-PETA-L3`; fase 5 menjadi `FE-RWI-276` s.d. `FE-RWI-278`; fase 6
menjadi `FE-RWI-279`. Bila ada perbedaan, roadmap yang berlaku.

---

## 8. Kriteria penerimaan

Kriteria bisnis yang mengikat tercatat sebagai `RWI-AC-498` s.d. `RWI-AC-507` di decision log.
Tabel ini merangkumnya bersama kriteria mutu pengerjaan.

| # | Kriteria | Sumber |
|---|---|---|
| AC-1 | Di keenam lantai tidak ada lagi dinding CAD, simbol pintu, anak tangga, garis sumbu, angka ukuran, label ruang teknis, maupun judul lembar berbahasa Inggris | `RWI-AC-498` |
| AC-2 | Jumlah kartu per lantai sama dengan tabel 2.2 (6, 29, 27, 11, 13, 20 = 106). Setiap kartu memuat kelas dan nomor dari API, sama dengan legenda kelas | `RWI-AC-499` |
| AC-3 | Posisi setiap kartu berada di dalam poligon kamar aslinya; kamar berbentuk L tetap berbentuk L | `RWI-AC-499` |
| AC-4 | Rupa status sesuai tabel 5.3: bed tersedia hijau, pesanan menunggu masuk ungu, kamar penuh abu, label "HD" bila sekurang-kurangnya satu penghuni HD, tanpa label bila semua belum dinilai | `RWI-AC-501` s.d. `503` |
| AC-5 | Badge kapasitas/terisi tetap ada di setiap kartu; titik bed sama jumlahnya dengan bed kamar (≤ 6) | `RWI-AC-504` |
| AC-6 | Klik, Enter, atau Spasi pada kartu membuka pop up kamar yang sama seperti sekarang; menyeret peta tidak membuka pop up | `RWI-AC-506` |
| AC-7 | Keempat pemakai menampilkan peta yang sama tanpa gambar CAD. Mode Pilih Bed: kamar tidak layak redup dan tidak bisa difokus; kamar terpilih bersorot amber. Mode Master: ruang cadangan bertepi putus-putus | `RWI-AC-500` |
| AC-8 | Kartu, kartu hover, dan ringkasan lantai tidak memuat nama pasien, jenis kelamin, lencana tier, atau Guid | `RWI-AC-504` |
| AC-9 | Ringkasan bed lantai dan jumlah bed kosong di tab lantai tampil tanpa permintaan API baru; filter kelas, "hanya tersedia", dan pencarian tidak dibangun pada pekerjaan ini | `RWI-AC-505` |
| AC-10 | Teks nomor kamar terbaca pada zoom awal (≥ 12 px di layar 1366 px), kontras teks ≥ 4,5 : 1, animasi mati bila `prefers-reduced-motion` | mutu |
| AC-11 | Tidak ada perubahan pada hook, service, kontrak API, backend, maupun database | `RWI-AC-506` |
| AC-12 | `lint:errors` bersih untuk berkas yang disentuh; `build` dijalankan pemilik (dicatat `NOT RUN` bila belum) | mutu |
| AC-13 | Lantai 3 selesai dan disahkan atasan sebelum lima lantai lain dikerjakan | `RWI-AC-507` |

---

## 9. Risiko dan mitigasi

| Risiko | Mitigasi |
|---|---|
| Petugas yang sudah hafal denah lama kehilangan patokan | Posisi kamar tidak berubah; penanda NS, lift, dan tangga tetap di tempatnya; label koridor membantu membaca arah |
| Kamar kecil (BAYI-1/2, ICU-CK, 520–522) tidak muat teks | Varian ringkas otomatis; pengguna memperbesar peta dengan zoom |
| Lantai 2 dan 5 tampak kosong karena sedikit kamar | Blok zona bernama untuk unit lain (5.9) |
| Bentrok warna kelas khusus (hijau) dengan bed tersedia (hijau, `RWI-DEC-346`) | Kelas khusus menjadi magenta (`RWI-FE-009`) |
| Selera atasan berbeda dari usulan | Contoh Lantai 3 disahkan lebih dulu (`RWI-DEC-349`), sehingga revisi hanya menyentuh satu lantai |
| Pekerjaan paralel mengubah berkas denah di tengah eksekusi | Cek `git status` sebelum tiap fase; tidak menyentuh berkas di luar daftar 6.2 |
| Kinerja SVG | Justru lebih ringan: ribuan segmen CAD diganti puluhan bentuk. Transisi memakai CSS saja |

---

## 10. Keputusan pemilik — 20 Oktober 2026

Usulan D-1 s.d. D-6 diputuskan lewat wawancara dan dicatat di
[decision log](../../../../00-interview-decisions.md) revision 62. Pemilik menjawab: 1 di luar opsi
("Atas nama saya Muhammad Hamzah"), 2A, 3A, 4A, 5A, 6B, 7A.

| # | Usulan semula | Keputusan | ID |
|---|---|---|---|
| D-1 | Amandemen RWI-DEC-325 butir 3 dan RWI-DEC-299 | Rupa denah boleh diubah atas arahan atasan, dicatat atas nama Muhammad Hamzah. `RWI-DEC-325` butir 3 digantikan; `RWI-DEC-299` tetap berlaku | `RWI-DEC-343` |
| — | Konsep tampilan | Peta bangsal: kartu kamar di posisi asli, tanpa CAD | `RWI-DEC-344` |
| — | Layar yang ikut berganti | Keempat pemakai, tanpa tombol CAD | `RWI-DEC-345` |
| — | Rupa status kamar | Kembali ke `RWI-DEC-303`/`316`; label HD bila sekurang-kurangnya satu penghuni HD | `RWI-DEC-346`, `RWI-DEC-350` |
| D-2 | Titik bed per kamar | Ya | `RWI-DEC-347` |
| D-3 | Warna kelas `khusus` ke magenta | `DEV_DISCRETION`; usulan magenta tetap dipakai | `RWI-FE-009` |
| D-4 | Blok zona unit non-rawat-inap | `DEV_DISCRETION` | `RWI-FE-010` |
| D-5 | Ringkasan, filter, "hanya tersedia", cari kamar | Ringkasan lantai dan badge tab sekarang; filter, "hanya tersedia", dan pencarian ditunda | `RWI-DEC-348` |
| D-6 | Tombol denah teknis (CAD) | Tidak ada; data CAD tetap disimpan | `RWI-DEC-345`, `RWI-FE-012` |
| — | Cara pengesahan | Contoh Lantai 3 dulu, lalu lima lantai lain; rilis setelah keenam lantai selesai | `RWI-DEC-349`, `RWI-DEC-351` |
| — | Warna titik status di kartu dan pop up | Satu palet untuk kartu dan pop up: hijau hanya tersedia, ungu hanya dipesan, oranye hanya HD (opsi A, ditemukan saat desain) | `RWI-DEC-353` (`PETA-OPEN-01`) |

`RWI-DEC-350` dan `RWI-DEC-351` adalah asumsi agent yang disahkan `RWI-DEC-352` (20 Oktober 2026).
