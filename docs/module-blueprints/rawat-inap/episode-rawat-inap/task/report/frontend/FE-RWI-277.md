# Laporan Perubahan Frontend — `FE-RWI-277`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-277` |
| Judul | Peta bangsal Lantai 4 dan Lantai 5 |
| Slice | `PETA-S2` — lima lantai lain (`MVP-PETA-2`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-277` |
| Trace | `FR-DNH-017`, `FR-DNH-018` untuk Lantai 4–5; `RWI-DEC-344`; `RWI-AC-498`, `RWI-AC-499`; `DNH-AT-498`, `499`, `918`; `RWI-FE-010` (bentuk lantai dan blok zona), `RWI-FE-011` (varian kartu ringkas) |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`. Hash lock [`peta-bangsal-denah-delivery-lock.json`](../../../roadmap/peta-bangsal-denah-delivery-lock.json) dicocokkan ulang 21 Oktober 2026: 19 dari 19 masukan desain sama; roadmap dan traceability berbeda hanya karena baris status `FE-RWI-272` s.d. `FE-RWI-275`. API contract 14.2 tidak berubah, dan berkas denah backend (`InpFloorPlanQueryService.cs`, kedua controller denah, `RoomController.cs`, DTO denah) identik antara commit snapshot dan commit rujukan |
| Wewenang UI | Frontend architecture 21.3: bentuk lantai, koridor, penanda, dan blok zona adalah `DEV_DISCRETION` (`RWI-FE-010`); kartu ringkas dan huruf adalah `DEV_DISCRETION` (`RWI-FE-011`). Ditambah revisi pemilik 21 Oktober 2026: Lantai 4 dan 5 tanpa area servis utara. Batasnya: kartu tidak boleh keluar dari poligon kamar; label ruang teknis, garis sumbu, dan angka ukuran tidak ditampilkan |
| Dependency | Gerbang `DEC-PETA-L3`. Pada sesi 21 Oktober 2026 pemilik menyatakan tampilan Lantai 3 sudah disetujui dan memerintahkan Lantai 4, 5, 8, dan 9 dikerjakan mengikuti tampilan itu. Pencatatan pengesahan sebagai keputusan baru di decision log **belum ada** (hash `00-interview-decisions.md` masih sama dengan lock). Tidak ada prasyarat backend |
| Klasifikasi | `HEAVY` — skor 9: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 6 bernilai 1, logika geometri dan aturan kartu 1, memakai kontrak API yang ada 1, database 0, keamanan 0, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `2e858ba4f9fee7bf5ff1dc65d146a3b4e4962fcb` (branch `HamzahV2`, upstream `origin/HamzahV2`) ditambah perubahan working tree task ini |
| Commit backend yang dijadikan rujukan | `be254d87eff2d0050c8724ec3aee727e105ca4cb` (branch `MHamzah`) |
| Tanggal | 21 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Ketiga acceptance criteria terpetakan ke source; skrip geometri, `npm run lint:errors`, dan `npm run build` lulus. Yang belum: pengesahan Lantai 3 belum dicatat sebagai keputusan baru di decision log, padahal itu syarat gerbang `DEC-PETA-L3`. Uji manual di aplikasi belum dijalankan; butir ini tidak menahan `✅` atas keputusan pemilik 1 September 2026 |

---

## 1. Keadaan yang ditemukan di awal

1. Lantai 4 dan 5 masih digambar dari garis gambar kerja (CAD) lewat `FloorPlanDrawing`, lengkap
   dengan sayap servis utara: meeting room, store, IT room, print room, dan ruang teknis di
   Lantai 4, serta PDI 26 dan ruang kantor di Lantai 5. `floors.js` hanya memberi data skematik
   pada Lantai 3.
2. `rooms[]` Lantai 4 berisi 27 poligon tegak lurus; SVIP 401 dan SVIP 418 berbentuk L. Lantai 5
   berisi 11 poligon; kamar 506 berbentuk L. Lantai 4 juga punya satu ruang tanpa nomor di antara
   kamar 420 dan 429 yang terdaftar sebagai ruang cadangan Master Bed (`floor-plan-room-map.js`).
3. Aturan varian kartu ringkas dari `FE-RWI-273` hanya memakai "sisi pendek < 56 satuan". Dengan
   aturan itu hanya kamar 520 yang ringkas. Kamar 521 (67,5 × 76,5) dan 522 (59 × 56) memakai
   kartu biasa, padahal isi kartu biasa butuh tinggi ± 82 satuan. Akibatnya badge kamar 521 meluber
   8 satuan dan kamar 522 meluber 28,5 satuan keluar kartu. Laporan `FE-RWI-273` (tafsiran nomor 2)
   sudah mencatat selisih ini untuk diputuskan pada task ini. `DNH-AT-918` meminta varian ringkas
   untuk 520, 521, dan 522.
4. Working tree frontend dipakai paralel: berkas resep, catatan perkembangan pasien (SOAP), dan uji
   terkait berubah di luar task ini. Tidak satu pun berkas denah yang berubah sebelum task dimulai.

Satuan pada laporan ini adalah satuan gambar sumber denah: 1 satuan = 1 piksel pada gambar
`denah-map-ruangan-rs-mmc/lantai-N.png` berukuran 1536 × 1024, sama dengan koordinat `rooms[]`.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas melihat Lantai 4 dan Lantai 5 sebagai peta bangsal yang bersih seperti contoh
Lantai 3 yang sudah disahkan, dengan kamar, bed, data, dan alur yang sama seperti sebelumnya.

**Pelaku.** Petugas Monitoring Bed, petugas admisi (Pilih Bed), perawat yang memindahkan pasien
(detail episode dan Workspace Keperawatan), dan admin Master Bed.

**Pemicu.** Pengguna membuka tab **Lantai 4** atau **Lantai 5** di salah satu dari keempat layar
denah.

**Prasyarat.** Data denah lantai itu dikirim server (satu panggilan per lantai, tidak berubah).

**Langkah utama.**

1. Pengguna membuka salah satu layar denah, lalu memilih tab **Lantai 4**.
2. Panggung denah menampilkan kanvas bertitik dan satu lempeng lantai putih seluas blok bangsal
   saja. Sayap servis utara tidak ditampilkan (revisi pemilik, bagian 2A).
3. Di atas lempeng tampil koridor utama berlabel "KORIDOR" dengan garis putus-putus, vestibul di
   kedua ujung, lobi lift di tengah, dan blok abu untuk ruang non-rawat-inap.
4. Tujuh penanda berikon membantu orientasi: Tangga di kedua ujung dan di inti tengah, Lift, NS A,
   NS B, serta Toilet di kelompok ruang utilitas sebelah NS A.
5. Ke-27 kamar tampil sebagai kartu di posisi aslinya: Kelas I 428 dan 429, Kelas II 427–425 dan
   423–420, VIP 402–417, serta VIP Deluxe (SVIP) 401 dan 418 yang tetap berbentuk L.
6. Tab **Lantai 5** menampilkan pola yang sama: kamar di sayap barat (503–510, 506 berbentuk L),
   penanda Tangga, Spoel hoek, Toilet, NS Lt 5 A, Pantry, Lift, dan Tangga di inti tengah, blok
   berlabel "Administrasi" untuk R. Medik, Dep. MD, dan KPR, serta blok berlabel "Rawat jalan &
   administrasi" untuk poliklinik dan kantor di timur.
7. Kamar kecil 520, 521, dan 522 di pojok tenggara Lantai 5 memakai kartu ringkas: nomor dan badge
   satu baris, titik bed di bawahnya. Lorong pendek dari ujung timur koridor menyambung ke ketiganya.
8. Klik, Enter, atau Spasi pada kartu membuka pop up kamar yang sama seperti sebelumnya. Zoom,
   geser, `Ctrl` + scroll, layar penuh, dan kompas tetap bekerja.

**Aturan bisnis yang terlihat di layar.**

- **Varian kartu ringkas.** Kartu memakai varian ringkas bila sisi pendek kotak terbesar kamar
  kurang dari 56 satuan **atau** luas kotak itu kurang dari 6.000 satuan².
  **Contoh:** kamar 521 berukuran 67,5 × 76,5 satuan. Sisi pendeknya 67,5 (tidak < 56), tetapi
  luasnya 67,5 × 76,5 = 5.164 satuan², kurang dari 6.000, sehingga ringkas. Kamar 522 berukuran
  59 × 56 = 3.304 satuan², juga ringkas. Sebaliknya K. 330 di Lantai 3 berukuran 60 × 124 = 7.440
  satuan², sehingga tetap kartu biasa seperti yang sudah disahkan.
- **Ruang cadangan.** Di Master Bed, ruang tanpa nomor di antara 420 dan 429 tampil sebagai kartu
  putus-putus "Ruang cadangan" di atas blok abu polos, sama seperti dua ruang cadangan Lantai 3. Di
  Monitoring dan Pilih Bed hanya blok abunya yang tampil.

**Perubahan status.** Tidak ada. Peta hanya menampilkan status yang dikirim server; rupa status
kartu tetap mengikuti `RWI-DEC-346` dan `RWI-DEC-350` dari `FE-RWI-273`.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kamar ada di denah tetapi tidak dikirim server | Bentuk putus-putus tanpa nomor, tidak dapat diklik (urutan 7, tidak berubah) |
| Denah gagal dimuat | Tidak berubah: "Denah gagal dimuat. Muat ulang." dan tombol Coba Lagi dari layar pemakai |
| Tab Lantai 2 | Masih gambar lama, karena `FE-RWI-276` dilewati dulu atas instruksi pemilik 21 Oktober 2026 |

**Hasil akhir.** Lima dari enam lantai (3, 4, 5, 8, 9) tampil sebagai peta bangsal. Data,
kunci denah, dan aksi tidak berubah.

### 2A. Revisi pemilik 21 Oktober 2026 — tanpa area servis utara

**Pemicu.** Muhammad Hamzah menyatakan tampilan Lantai 3 sudah disetujui dan sesuai, lalu memberi
arahan untuk lantai berikutnya: "Ketika Pembuatan Denah Pada Lantai 4 dan 5 tidak perlu area
service seperti yang saya tandai pada capture tersebut", dengan tangkapan layar yang melingkari
sayap utara di atas blok bangsal kedua lantai itu. Lantai 2 dilewati dulu.

**Dasar wewenang.** Bentuk lantai dan blok zona adalah `DEV_DISCRETION` (`RWI-FE-010`). Revisi ini
sama polanya dengan revisi Lantai 3 pada `FE-RWI-272` bagian 2A: tidak mengubah kamar, kunci denah,
data, maupun kontrak, sehingga tidak memerlukan keputusan baru tersendiri.

| Cakupan semula di kartu task | Hasil sesudah revisi |
| --- | --- |
| Lantai 4: blok "Ruang staf" (meeting, print, IT, store) | Tidak dibuat; keempat ruang itu ada di sayap servis utara. Lempeng berhenti di dinding utara blok bangsal (y 459,5). Bagian ruang servis yang menjorok ke bawah garis itu (x 640–971, y 459,5–506) menjadi satu blok polos tanpa label |
| Lantai 5: blok "Rawat jalan & administrasi" berisi Poli 22/23/24/26, R. Medik, Dep. MD, KPR | PDI 26 ada di sayap servis utara sehingga tidak tampil. Poli 22–24 dan kantor timur menjadi blok "Rawat jalan & administrasi"; R. Medik, Dep. MD, dan KPR yang terpisah oleh inti lift menjadi blok "Administrasi". Lempeng berhenti di y 426 |
| Lantai 5: penanda NS, pantry, toilet | Dibuat: NS Lt 5 A, Pantry, Toilet, ditambah Spoel hoek dari ruang "spool hose" |

**Contoh dampak di layar.** Pada Lantai 4, baris kamar 428–425 dan 423–429 kini langsung menjadi
tepi atas peta; kompas "UTARA" berada di ruang kosong di atas tangga timur, tidak menutupi kartu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/frontend/frontend-architecture.md`,
  `REPORT_TEMPLATE.md`; references `build-module-frontend` (`base-component-decision-gate.md`,
  `ui-consistency-checklist.md`, `test-policy.md`); `rules/rule-output/` (aturan output, status
  roadmap, grafik dependency).
- Blueprint: roadmap, lock, dan traceability peta bangsal; frontend architecture bagian 21;
  acceptance matrix 26 (`DNH-AT-498`, `499`, `918`); planning redesain denah 5.5, 5.9, 6.1; laporan
  `FE-RWI-272` dan `FE-RWI-273`; gambar `denah-map-ruangan-rs-mmc/lantai-3.png`, `lantai-4.png`,
  `lantai-5.png`.
- Source frontend: `lantai-3.schematic.js`, `schematic-geometry.js`, `floors.js`, `lantai-4.plan.js`,
  `lantai-5.plan.js`, `plan-primitives.js`, `floor-plan-room-map.js`, `floor-plan-import-manifest.js`,
  `floor-plan-schematic.jsx`, `floor-plan-room.jsx`, `floor-plan-view.jsx`, `floor-plan-monitoring.jsx`,
  `floor-plan.module.css`, `use-floor-plan.js`, `src/app/globals.css` (token warna), dan
  `tests/unit/floor-plan-schematic-geometry.test.mjs`, `floor-plan-room-card.test.mjs`.
- Source backend (baca saja): diff berkas denah antara `6e0f2b1b` dan `be254d87` — kosong.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/lantai-4.schematic.js` | **Baru.** Lempeng 4 titik seluas blok bangsal (`x 81,5–1446`, `y 449,5–905`); `viewBox` `x 61,5, y 359,5, 1404,5 × 565,5`; 5 jalur sirkulasi termasuk koridor `y 679,5–747`; 13 blok zona; 7 penanda |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/lantai-5.schematic.js` | **Baru.** Lempeng 4 titik (`x 93–1405,5`, `y 416–871`); `viewBox` `x 73, y 326, 1352,5 × 565`; 5 jalur sirkulasi termasuk koridor `y 643,5–706` dan lorong menuju 520–522; 11 blok zona (2 berlabel); 7 penanda |
| `src/lib/constants/health-services/inpatient-management/floor-plan/floors.js` | Lantai 4 dan 5 mendapat properti `schematic` (Lantai 8 dan 9 pada `FE-RWI-278`); komentar menyebut Lantai 2 menunggu `FE-RWI-276` |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/schematic-geometry.js` | Konstanta `COMPACT_ROOM` dan fungsi murni `isCompactRoomBox`: aturan varian ringkas kini sisi pendek < 56 **atau** luas < 6.000 satuan² |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room.jsx` | `cardGeometryOf` memakai `isCompactRoomBox`; konstanta lokal `COMPACT_LIMIT` dihapus karena aturannya pindah ke modul geometri bersama |
| `tests/unit/floor-plan-schematic-floors.test.mjs` | **Baru**, dipakai bersama `FE-RWI-278`. Uji Lantai 4 dan 5 bernama `FE-RWI-277: …`: 27/11 poligon, bentuk L, kartu di dalam poligon, tanpa tumpang tindih, di dalam lempeng dan `viewBox`, tanpa label teknis, aturan lempeng/bingkai sama dengan Lantai 3, tanpa area servis utara, ruang cadangan di atas blok polos, varian ringkas tepat 520–522, dan teks setiap kartu muat |
| `tests/unit/floor-plan-schematic-geometry.test.mjs` | Satu uji `FE-RWI-272` yang mengunci "hanya Lantai 3 berskematik" kini mengunci Lantai 3, 4, 5, 8, dan 9 berskematik serta Lantai 2 tetap gambar lama. Keadaan masa contoh memang berakhir pada task ini |

Hook, service, kontrak API, route, menu, permission, `package.json`, CSS, backend, dan database
tidak berubah. `rooms[]`, `floorPlanKey`, dan berkas CAD tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi tetap: data skematik dan fungsi geometri murni di `src/lib/constants/...` →
  komponen feature `FloorPlanSchematic` dan `FloorPlanRoom` → `FloorPlanView`. Tidak ada request API
  baru; view tidak memanggil Axios.
- Data lantai baru mengikuti bentuk, penamaan, komentar, dan aturan bingkai `lantai-3.schematic.js`:
  lempeng 10 satuan di luar dinding luar, `viewBox` 20 satuan di kiri, kanan, dan bawah lempeng, 90
  satuan di atas sebagai tempat kompas. Uji "lempeng dan bingkai … mengikuti aturan contoh Lantai 3"
  memeriksa kelima lantai dengan rumus yang sama.
- Aturan varian ringkas dipindah ke modul geometri murni supaya renderer dan skrip pemeriksa memakai
  satu aturan yang sama, mengikuti pola `SCHEMATIC_LANDMARK` yang sudah ada.

**Keputusan elemen UI (base component decision gate).** Base component tidak wajib untuk modul denah
(`RWI-DEC-325` butir 4), dan tidak ada base component SVG untuk kartu denah.

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Lapisan peta Lantai 4 dan 5 | `FloorPlanSchematic` | `floor-plan-schematic.jsx`; cukup data baru | `REUSE` | Data skematik per lantai |
| Ikon penanda (tangga, lift, NS, toilet, pantry, spoel hoek) | `LANDMARK_ICONS` `lucide-react` | Keenam jenis sudah ada di peta ikon | `REUSE` | Tanpa ikon baru |
| Label blok zona | `Zone` di `FloorPlanSchematic` | Prop `label` sudah didukung | `REUSE` | "Administrasi", "Rawat jalan & administrasi" |
| Kartu kamar 503–510, 401–429 | `FloorPlanRoom` varian kartu | Renderer `FE-RWI-273`; markup tidak berubah (bagian 6) | `REUSE` | — |
| Kartu kamar kecil 521 dan 522 | `FloorPlanRoom` varian ringkas | Aturan `< 56` tidak menangkap keduanya | `EXTEND` | Tambah syarat luas < 6.000 satuan² |

`UI GATE: 5 elemen — REUSE 4, EXTEND 1, COMPOSE 0, WRAP 0, NEW 0`

**Keputusan: kartu kamar 521 dan 522 yang terlalu kecil untuk isi bertumpuk**

- **A. Tambah syarat luas < 6.000 satuan² pada aturan varian ringkas, di modul geometri bersama —
  Rekomendasi, dijalankan.** Memenuhi `DNH-AT-918` (ringkas tepat 520, 521, 522). Ambang 6.000 jatuh di
  celah lebar antara kamar ringkas terbesar (5.164) dan kartu biasa terkecil (7.440 pada K. 330), jadi
  tidak ada kamar yang berpindah varian secara tidak sengaja. Tidak ada kartu Lantai 3, 4, 8, atau 9
  yang berubah.
- **B. Daftar ID kamar ringkas di data lantai (`k-520`, `k-521`, `k-522`).** Sama hasilnya hari ini,
  tetapi aturan visual pindah ke data dan harus diingat setiap kali kamar kecil baru ditambahkan.
- **C. Membiarkan aturan `< 56`.** Badge kamar 521 dan 522 meluber keluar kartu; melanggar `DNH-AT-918`.

Keputusan ini `DEV_DISCRETION` (`RWI-FE-011`) dan bukan perubahan perilaku base component, sehingga
tidak menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G).** Tidak ada stylesheet yang diubah,
sehingga butir 1, 2, dan 6 tidak punya sasaran. Pada baris JSX yang ditambah dan data lantai baru:
tidak ada `<button>`, `<table>`, utilitas tipografi Bootstrap, `style=`, warna literal, maupun
`!important`.

**Tafsiran yang dicatat.**

1. "Area servis" ditafsirkan sebagai seluruh bagian lantai di utara dinding utara blok bangsal,
   sesuai lingkaran pada tangkapan layar pemilik. Bagian ruang servis yang menjorok ke dalam blok
   (Lantai 4, x 640–971, y 459,5–506) digambar sebagai blok polos tanpa label supaya bentuk blok tetap
   jujur dan tidak terlihat seperti lobi.
2. Ruang "spool hose" pada gambar Lantai 5 adalah ejaan lain "spoel hoek" (ruang spoel), sehingga
   diberi penanda Spoel hoek seperti Lantai 3. Ruang "plg. foam" tidak jelas fungsinya dan menjadi
   blok polos.
3. Kelompok ruang di antara kamar 425 dan NS A (Lantai 4) berisi kloset dan wastafel pada gambar,
   sehingga diberi satu penanda Toilet. Spider deck dan gudang di kedua ujung tidak diberi penanda
   karena tidak membantu orientasi petugas (planning 5.5).
4. Area NS Lt 5 A terbuka ke lobi tanpa dinding, sehingga penandanya diletakkan di lobi tanpa blok
   sendiri.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka berbentuk kartu dari `FE-RWI-274`, sama untuk semua lantai (tidak berubah) |
| Kosong | Lempeng, koridor, blok zona, dan penanda tetap tampil; kamar tanpa data API tampil putus-putus tanpa nomor |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." dengan tombol Coba Lagi/Muat Ulang Denah dari layar pemakai |
| Tanpa hak akses | Tidak berubah: Master Bed memakai `AccessDeniedGate`; layar lain mengikuti halaman induknya |

---

## 5. Endpoint yang dikonsumsi

Task ini tidak menambah atau mengubah panggilan API. Layar yang memakai peta tetap memanggil
endpoint berikut, satu panggilan per lantai.

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah satu lantai di Monitoring Bed | `InpatientBedManagement : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah Pilih Bed (admisi dan transfer) beserta kelayakan bed | `InpatientBedOccupancy : Read` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan-layout` | Susunan satu lantai untuk editor Master Bed | `Room : Read` |

---

## 6. Verifikasi

Validasi dijalankan satu kali pada working tree yang memuat `FE-RWI-277` dan `FE-RWI-278`.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/floor-plan-schematic-floors.test.mjs tests/unit/floor-plan-schematic-geometry.test.mjs` | 35 uji, 35 lulus: 23 uji baru Lantai 4, 5, 8, 9 dan 12 uji Lantai 3 | `PASS` | Keluaran `node:test` |
| Seluruh 20 berkas uji yang membaca kode denah | 161 uji: 159 lulus, 2 gagal di `inpatient-bed-transfer-refresh.test.mjs` (`useBedManagement` opsi `forceMonitoring` dan `bed-management-view`). Keduanya memeriksa berkas yang tidak disentuh dan sudah tercatat gagal sejak laporan `FE-RWI-272` | `PASS` untuk uji denah; `EXISTING / ENVIRONMENT ISSUE` untuk 2 kegagalan | Keluaran `node:test` |
| `npm run test:unit` | 3378 uji: 3341 lulus, 37 gagal di 28 berkas uji. Tidak satu pun berkas yang gagal membaca berkas task ini. Jumlah 37 sama dengan laporan `FE-RWI-275` (35 lama + 2 dari pekerjaan paralel resep/tindakan) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah dan pencarian silang berkas uji |
| `npm run lint:errors` | `eslint . --quiet` selesai dengan exit 0 tanpa satu pun galat | `PASS` | Keluaran perintah |
| `npx eslint . -f json` (setara `npm run lint`) | 5455 berkas, `0 errors`, 1086 warning. Berkas task ini 0 warning. Selisih +3 dari garis dasar 1083 (20 Oktober, 5436 berkas) berasal dari 151 berkas JavaScript yang masuk lewat merge sejak snapshot `a491b05`, bukan dari task ini | `PASS`; selisih `EXISTING WARNING` | Ringkasan keluaran JSON |
| `npm run build` | `✓ Compiled successfully in 55s` (Next.js 16.2.12, Turbopack), 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build. Sebelum build dipastikan tidak ada proses Node yang mengunci `.next` | `PASS` | Keluaran perintah |
| Perbandingan markup kartu `HEAD` vs working tree, semua kamar × 6 keadaan data × 3 mode | Lantai 3: 522 render, **tidak ada kartu berubah**. Lantai 4: 486 render, tidak ada yang berubah. Lantai 5: 198 render, yang berubah hanya `k-521` dan `k-522` (kini ringkas) | `PASS` | Skrip render server di luar repository |
| Tumpang-susun data skematik di atas `lantai-4.png` dan `lantai-5.png` | Lempeng, koridor, blok zona, dan penanda jatuh di ruang yang benar; lempeng berhenti di dinding utara blok bangsal | Bukti pendukung | Screenshot Edge di luar repository |
| Pratinjau statis komponen asli (`FloorPlanSchematic` + `FloorPlanRoom`, token `globals.css`, data samaran) | Lantai 3 sama dengan tampilan yang disahkan. Lantai 4 dan 5 tampil sebagai peta bangsal tanpa unsur CAD; kartu ringkas 520–522 muat di kartunya | Bukti pendukung, bukan uji manual | Screenshot Edge di luar repository, tidak dilampirkan |
| `git diff --check` dan pemeriksaan spasi berkas baru | Bersih | `PASS` | Keluaran perintah |
| Uji di aplikasi: Lantai 4 dan 5 di Monitoring, Pilih Bed, transfer, dan Master | Belum dijalankan | `NOT RUN` | Butuh login dan data backend; dijalankan pengguna |

Uji manual: `NOT FEASIBLE` untuk agent, karena aplikasi butuh login dan data API backend. Tidak ada
kontrol interaktif yang berubah; klik, keyboard, dan pop up tetap memakai jalur yang sama. Uji manual
di peramban tidak menahan `✅` atas keputusan pemilik 1 September 2026, tetapi dianjurkan dijalankan
pengguna saat meninjau.

`AUTOMATED TEST: node --test tests/unit/floor-plan-schematic-floors.test.mjs tests/unit/floor-plan-schematic-geometry.test.mjs — PASS (35/35)`

`AUTOMATED TEST: npm run test:unit — FAIL (37 dari 3378 gagal di 28 berkas uji di luar denah; uji denah lulus kecuali 2 kegagalan lama inpatient-bed-transfer-refresh)`

**Tidak dijalankan:** uji di aplikasi dan jejak jaringan, karena butuh sesi login dan backend yang
berjalan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `DNH-AT-498` lulus untuk Lantai 4 dan 5 — tanpa dinding, pintu, anak tangga, sumbu, angka ukuran, label teknis, dan judul lembar | Terpenuhi di source; belum diuji di aplikasi | `floors.js` memberi `schematic` sehingga `FloorPlanView` menggambar `FloorPlanSchematic`, bukan `FloorPlanDrawing`; uji "data peta Lantai 4/5 tidak memuat unsur gambar kerja"; pratinjau statis |
| `DNH-AT-499` lulus untuk Lantai 4 dan 5 — 27 dan 11 kartu, di dalam poligon `rooms[]`, 401/418/506 berbentuk L, kelas dan nomor dari API | Terpenuhi di source; belum diuji di aplikasi | Uji "terdaftar berskematik dengan 27/11 poligon kamar dan bentuk L tetap L"; kartu memakai `room.classLabel` dan `room.roomNumber` dari data API (renderer `FE-RWI-273`, tidak berubah) |
| `DNH-AT-918` lulus untuk Lantai 4 dan 5 — kartu di dalam poligon, tidak menimpa koridor dan penanda, teks muat, varian ringkas 520–522 | Terpenuhi | `tests/unit/floor-plan-schematic-floors.test.mjs` |
| Verifikasi wajib: bandingkan dengan `lantai-4.png` dan `lantai-5.png` | Terpenuhi | Tumpang-susun di atas gambar sumber |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi; berkas denah bersih saat mulai |
| `V-FE` 2 — `lint:errors` | Terpenuhi: exit 0, `0 errors` |
| `V-FE` 3 — `test:unit` | Dijalankan: 3341/3378 lulus; 37 kegagalan bukan regresi task |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully`, exit 0 |
| `V-FE` 5 — bukti visual | Pratinjau statis komponen asli dan tumpang-susun gambar sumber, di luar repository; uji di aplikasi belum dijalankan (tidak menahan `✅`, keputusan 1 September 2026) |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |
| Gerbang `DEC-PETA-L3` | **Belum tertutup penuh**: pengesahan dinyatakan pemilik 21 Oktober 2026, pencatatan sebagai keputusan baru di decision log belum ada. Inilah satu-satunya alasan status `🟡` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint pada berkas task ini |
| Masalah yang diketahui | Penyederhanaan `RWI-FE-010` dicatat di komentar kepala berkas data: inti lift berhenti di garis koridor, lobi lift berpintu ganda digambar sebagai jalur sirkulasi, lorong poliklinik Lantai 5 masuk ke blok rawat jalan |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Berkas pratinjau, skrip render, dan screenshot hanya ada di folder kerja agent di luar repository |
| Interupsi | `NONE`. Selama task berjalan, pekerjaan paralel mengubah berkas catatan perkembangan pasien (SOAP); tidak ada tumpang tindih dengan berkas denah |
| Status Git | Berkas task: `M floor-plan-room.jsx`, `M floors.js`, `M schematic-geometry.js`, `M tests/unit/floor-plan-schematic-geometry.test.mjs`, `?? schematic/lantai-4.schematic.js`, `?? schematic/lantai-5.schematic.js`, `?? tests/unit/floor-plan-schematic-floors.test.mjs` (`lantai-8`/`lantai-9` milik `FE-RWI-278`). Berkas lain yang berubah milik pekerjaan paralel |
| Langkah berikutnya | Catat pengesahan Lantai 3 dan arahan 21 Oktober 2026 (tanpa area servis Lantai 4–5, Lantai 2 dilewati dulu) sebagai keputusan baru di decision log, lalu naikkan task ini ke `✅` pada sesi yang membaca ulang source |
