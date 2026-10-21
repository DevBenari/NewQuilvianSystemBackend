# Laporan Perubahan Frontend — `FE-RWI-272`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-272` |
| Judul | Fondasi peta bangsal dan lapisan peta Lantai 3 |
| Slice | `PETA-S1` — contoh Lantai 3 (`MVP-PETA-1`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-272` |
| Trace | `FR-DNH-017`, `FR-DNH-024`; `RWI-DEC-344`, `345`, `349`, `351`; `RWI-AC-498` dan `500` untuk Lantai 3, `RWI-AC-507` bagian contoh; `DNH-AT-498`, `500`, `917`, `918`, `922`; `NFR-808`, `NFR-813`; `RWI-FE-010` |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352` (20 Oktober 2026). Hash lock [`peta-bangsal-denah-delivery-lock.json`](../../../roadmap/peta-bangsal-denah-delivery-lock.json) dicocokkan ulang: 22 dari 22 berkas sama. API contract 14.2 tidak berubah |
| Wewenang UI | Frontend architecture 21.3: bentuk lantai, koridor, penanda, dan blok zona adalah `DEV_DISCRETION` (`RWI-FE-010`); base component dan references `build-module-frontend` tidak wajib (`RWI-DEC-325` butir 4). Batasnya: kartu tidak boleh keluar dari poligon kamar, label ruang teknis, garis sumbu, dan angka ukuran tidak ditampilkan |
| Dependency | Tidak ada (gelombang 1). Tidak ada prasyarat backend |
| Klasifikasi | `HEAVY` — skor 10: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 10 bernilai 2, logika geometri sedang 1, memakai kontrak API yang ada 1, database 0, keamanan 0, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a491b05afcb6b3ed70218ccaa557007719d45ac3` (branch `HamzahV2`) ditambah perubahan working tree task ini |
| Commit backend yang dijadikan rujukan | `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` (branch `MHamzah`) |
| Tanggal | 20 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap untuk kelima acceptance criteria; skrip geometri, `npm run lint`, dan `npm run build` lulus. Dikerjakan ulang 20 Oktober 2026 atas revisi pemilik (Area Servis Lantai 3 dihapus, bagian 2A); validasi sesudah revisi lulus. Yang belum hanya uji manual di aplikasi, yang dijalankan pengguna sendiri |

---

## 1. Keadaan yang ditemukan di awal

1. Setiap lantai digambar dari garis gambar kerja (CAD) di `lantai-N.plan.js` lewat
   `floor-plan-drawing.jsx`: dinding dua lapis, simbol pintu, anak tangga, label ruang teknis
   (ELEC, A.H.U., LV. ROOM, TEL. ROOM), dan judul lembar "3rd Floor" (`floor-plan-drawing.jsx:59-76`).
2. Ketiga berkas pemakai — `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, dan
   `master-data-bed-floor-plan-view.jsx` — membuat elemen `<FloorPlanDrawing plan={...} />` lalu
   mengirimnya ke `FloorPlanView` lewat prop `drawing`, bersama `viewBox` gambar lama
   (`x 56, y 86, 1424 × 846`) yang ikut memuat area judul lembar.
3. Belum ada data peta bangsal, fungsi geometri kartu, maupun lapisan peta pengganti. Kamar hanya
   berupa garis tepi berwarna di atas ratusan garis CAD.
4. `rooms[]` Lantai 3 berisi 29 poligon tegak lurus (dua di antaranya berbentuk L: VIP 301 dan
   SVIP 318). Poligon disimpan sebagai string path `outline`, bukan daftar titik.
5. Working tree frontend dipakai paralel: berkas admisi, transfer, dan workspace berubah di luar
   task ini. Tidak satu pun berkas denah yang ikut berubah sebelum task dimulai.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas melihat Lantai 3 sebagai peta bangsal yang bersih, bukan lembar gambar kerja,
dengan kamar, bed, data, dan alur yang sama.

**Pelaku.** Petugas Monitoring Bed, petugas admisi (Pilih Bed), perawat yang memindahkan pasien
(detail episode dan Workspace Keperawatan), dan admin Master Bed.

**Langkah utama.**

1. Pengguna membuka salah satu dari keempat layar denah, lalu memilih tab **Lantai 3**.
2. Di panggung denah tampil kanvas bertitik dan satu lempeng lantai putih seluas blok bangsal.
   Sayap servis utara tidak ditampilkan (revisi pemilik 20 Oktober 2026, bagian 2A).
3. Di atas lempeng tampil jalur sirkulasi (koridor utama berlabel "KORIDOR" dengan garis putus-putus,
   lobi tengah, dan dua vestibul), blok abu untuk ruang non-rawat-inap, dan 13 penanda berikon:
   Tangga (3), Lift (1), NS3A, NS3B, Pantry, Peralatan, Spoel hoek (2), Linen (2), dan Toilet (1).
4. Kamar tetap di posisi aslinya. Pada task ini kamar masih digambar renderer lama (garis tepi,
   label, badge); kartu kamar baru dikerjakan `FE-RWI-273`.
5. Klik kamar tetap membuka pop up yang sama. Zoom, geser, `Ctrl` + scroll, tombol tampilkan
   seluruh denah, dan kompas tetap bekerja.

**Contoh.** Lantai 3 tetap memperlihatkan baris atas 330–325 di kiri dan 322–319 di kanan,
koridor di tengah, serta baris VIP 301–318 di bawah. Yang hilang: garis dinding, pintu, anak
tangga, label ELEC/A.H.U./LV. ROOM, dan judul "3rd Floor".

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Tab Lantai 2, 4, 5, 8, atau 9 | Lantai belum punya data skematik, sehingga gambar lama tampil persis seperti sebelumnya (`RWI-DEC-351`). Tampilan campuran ini hanya untuk pemeriksaan internal dan tidak dirilis |
| Denah gagal dimuat | Tidak berubah: pesan galat dan tombol Coba Lagi dari layar pemakai |
| Ruang cadangan di Monitoring | Tetap tidak dirender; di Master Bed ruang cadangan tampil di atas blok abu polos |

### 2A. Pengerjaan ulang — revisi pemilik 20 Oktober 2026

**Pemicu.** Muhammad Hamzah meninjau Lantai 3 di aplikasi dan menyatakan tampilan peta sudah
bagus serta posisi kamar sudah mengikuti bentuk UI, lalu meminta satu revisi untuk Lantai 3:
"Hilangkan Area Service, tidak perlu ditampilkan", dengan tangkapan layar yang menandai blok
"AREA SERVIS" di utara blok bangsal beserta penandanya.

**Dasar wewenang.** Bentuk lantai dan blok zona adalah `DEV_DISCRETION` (`RWI-FE-010`), dan
`RWI-DEC-349` menetapkan bahwa revisi dikerjakan pada Lantai 3 lebih dulu sebelum lantai lain.
Revisi ini tidak mengubah kamar, kunci denah, data, maupun kontrak, sehingga tidak memerlukan
keputusan baru di decision log.

| Sebelum | Sesudah |
| --- | --- |
| Lempeng lantai 8 titik: blok bangsal ditambah sayap servis utara (`y 279–476`, `x 467–1196`) | Lempeng lantai 4 titik seluas blok bangsal saja (`x 72–1438`, `y 476–917`) |
| Blok zona "Area servis" (`x 477–1186`, `y 288,5–486`) | Dihapus |
| Penanda Lift (lift ranjang), Toilet, dan Spoel hoek di sayap servis | Dihapus; Lift dan Tangga inti tengah tetap ada |
| `viewBox` `x 52, y 259, 1406 × 678` | `viewBox` `x 52, y 386, 1406 × 551`: 20 satuan di kiri, kanan, dan bawah lempeng; 90 satuan di atas sebagai tempat kompas di pojok kanan atas supaya tidak menutupi kartu kamar |

**Contoh dampak di layar.** Pada lebar 1366 px, peta Lantai 3 kini dimulai langsung dari baris kamar
330–319; kompas "UTARA" berada di ruang kosong di atas kamar 319 dan tangga timur. Karena bingkai
lebih pendek, kartu kamar tampil sedikit lebih besar pada zoom awal.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/frontend-architecture.md`,
  `REPORT_TEMPLATE.md`, `test-policy.md`; `rules/rule-output/` (status roadmap, lokasi laporan,
  grafik dependency, aturan output); `rules/GLOBAL_RULES.md`.
- Blueprint: roadmap, lock, dan traceability peta bangsal; decision log revision 61–63
  (`RWI-DEC-343` s.d. `353`); frontend architecture bagian 21; acceptance bagian 26; planning
  redesain denah; gambar `denah-map-ruangan-rs-mmc/lantai-3.png` dan `UI-Sudah-Diterapkan/lantai-3.png`.
- Source frontend: `floor-plan-view.jsx`, `floor-plan-room.jsx`, `floor-plan-drawing.jsx`,
  `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, `floor-plan-skeleton.jsx`,
  `floor-plan-compass.jsx`, `zoom-toolbar.jsx`, `floor-plan-room-popup.jsx`, `floor-plan.module.css`,
  `master-data-bed-floor-plan-view.jsx`, `floors.js`, `lantai-3.plan.js`, `plan-primitives.js`,
  `floor-plan-room-map.js`, `floor-plan-room-labels.js`, `use-floor-plan.js`,
  `use-floor-plan-layout.js`, `src/app/globals.css`, keempat pemanggil `FloorPlanSelection`/
  `FloorPlanMonitoring`, dan sepuluh berkas `tests/unit/floor-plan-*.test.mjs`.
- Source backend (baca saja): `InpFloorPlanQueryService.cs`, `InpatientBedManagementController.cs`,
  `InpatientBedOccupancyController.cs`, `RoomController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/schematic-geometry.js` | **Baru.** Fungsi murni `insetOrthogonal`, `roundedOrthogonalPath`, `largestRect`, ditambah pembantu `pointsFromPolygonPath` (mengurai `outline` kamar tanpa mengubah `rooms[]`), `orthogonalCells`, `isPointInPolygon`, `polygonArea`, `landmarkBounds`, dan ukuran penanda `SCHEMATIC_LANDMARK` |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/lantai-3.schematic.js` | **Baru.** Konstanta beku Lantai 3. Sesudah revisi 20 Oktober 2026: `viewBox` `x 52, y 386, 1406 × 551`; lempeng 4 titik seluas blok bangsal; 5 jalur sirkulasi termasuk koridor `y 697,5–764`; 14 blok zona; 13 penanda (versi pertama: lempeng 8 titik dengan sayap servis, 15 blok zona termasuk "Area servis", 16 penanda) |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-schematic.jsx` | **Baru.** Lapisan peta tanpa state: kanvas bertitik, lempeng dengan bayangan, sirkulasi, blok zona, dan penanda berikon `lucide-react` |
| `src/lib/constants/health-services/inpatient-management/floor-plan/floors.js` | Properti `schematic` hanya pada Lantai 3 |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-view.jsx` | Prop baru `schematic`; bila ada, menggambar `FloorPlanSchematic` dan memakai `viewBox` skematik (tanpa judul lembar); bila tidak, gambar lama apa adanya. Atribut `data-floor-plan-layer` untuk gaya per lapisan |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-monitoring.jsx` | Satu baris: `schematic={state.floor.schematic}` |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx` | Satu baris yang sama; berlaku untuk Pilih Bed admisi, transfer detail episode, dan panel transfer Workspace Keperawatan |
| `src/components/view/health-services/master-data/bed/master-data-bed-floor-plan-view.jsx` | Satu baris yang sama |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` | Token `--fp-map-*` di `.theme` dan gaya lapisan peta (`.schematic*`). Gaya CAD tidak dihapus |
| `tests/unit/floor-plan-schematic-geometry.test.mjs` | **Baru.** Skrip pemeriksa geometri `node:test` (12 uji; uji ke-12 ditambahkan pada revisi untuk mengunci bahwa tidak ada lempeng, zona, sirkulasi, maupun penanda di utara blok bangsal) |

Hook, service, kontrak API, route, menu, permission, `package.json`, backend, dan database tidak
berubah. `rooms[]`, `floorPlanKey`, dan berkas CAD tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: data skematik dan fungsi geometri murni di `src/lib/constants/...` →
  komponen feature `FloorPlanSchematic` → `FloorPlanView`. Tidak ada request API baru; view tidak
  memanggil Axios.
- Komponen baru tinggal di folder feature denah dengan CSS module yang sama (`RWI-DEC-325` butir 4).
  Ikon memakai `lucide-react` 0.483 yang sudah terpasang (`NFR-813`).
- Warna peta didefinisikan sekali sebagai token `--fp-map-*` di `.theme`, mengikuti pola token
  modul denah yang sudah ada; selektor gaya tidak memakai nilai literal di luar blok token.

**Keputusan elemen UI (base component decision gate).**

| Elemen | Status | Bukti |
| --- | --- | --- |
| Lempeng lantai, sirkulasi, blok zona, penanda | `NEW` (`FloorPlanSchematic`) | Struktur target 21.8 menandai `floor-plan-schematic.jsx` sebagai `[Baru]`; disetujui `RWI-DEC-352` |
| `FloorPlanView` | `EXTEND` lewat prop `schematic` | Tabel Keputusan Reuse roadmap |
| `FloorPlanDrawing` | `REUSE` sebagai cadangan masa contoh | `RWI-DEC-351` |
| Zoom, geser, kompas, tab lantai | `REUSE` tanpa perubahan | Tabel Keputusan Reuse; `FE-RWI-274` mengubah gaya kompas |
| Ikon penanda | `REUSE` `lucide-react` | Planning 5.5 |

Pilihan untuk elemen `NEW` sudah diputuskan di blueprint: (1) komponen feature baru
`FloorPlanSchematic` — **direkomendasikan dan disetujui** karena gambar CAD tetap utuh sebagai
cadangan dan dapat dilepas di `FE-RWI-279`; (2) menambah mode skematik ke `FloorPlanDrawing` —
mencampur dua rupa dalam satu berkas dan menyulitkan penghapusan jalur lama; (3) base component di
`base-features` — dilarang `RWI-DEC-325` butir 4.

`UI GATE: PASS` — tidak ada keputusan `NEW` atau `EXTEND` yang menunggu pemilik.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah pada task ini: kerangka pemuatan lama. Kerangka berbentuk kartu dikerjakan `FE-RWI-274` |
| Kosong | Lempeng dan penanda tetap tampil; kamar tanpa data API tidak dirender (renderer lama). Rupa putus-putus dikerjakan `FE-RWI-273` |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." beserta tombol Coba Lagi/Muat Ulang Denah dari layar pemakai |
| Tanpa hak akses | Tidak berubah: Master Bed memakai `AccessDeniedGate`; layar lain mengikuti halaman induknya |

---

## 5. Endpoint yang dikonsumsi

Task ini tidak menambah atau mengubah panggilan API. Layar yang disentuh tetap memakai endpoint
berikut, satu panggilan per lantai.

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

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/floor-plan-schematic-geometry.test.mjs` | 11 uji, 11 lulus, 0 gagal: 29 poligon, inset di dalam poligon, bentuk L tetap L, tanpa varian ringkas di Lantai 3, koridor/zona/penanda tidak menimpa kamar, semua unsur di dalam lempeng dan `viewBox`, data tanpa label teknis, dan ketiga pemakai meneruskan skematik | `PASS` | Keluaran `node:test` |
| `npx eslint` pada 9 berkas yang disentuh | Exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | `0 errors`, 1083 warning — sama persis dengan garis dasar sebelum task (1083) | `PASS` | Ringkasan `eslint -f json`: 5429 berkas, tidak ada warning baru pada berkas yang disentuh |
| `npm run test:unit` | 3300 uji: 3265 lulus, 35 gagal. Ke-35 kegagalan berada di berkas uji lain (antara lain admisi, deposit, hemodialisa, bank darah, workspace dokter/perawat, `inpatient-bed-transfer-refresh`) yang tidak membaca berkas task ini. Sebelas berkas uji yang membaca berkas denah yang diubah (`floor-plan-*.test.mjs`, `inpatient-bed-board`, `inpatient-bed-realtime`, `inpatient-discharge`, `inpatient-episode-detail`) dijalankan ulang terpisah: 112 dari 112 lulus | `EXISTING / ENVIRONMENT ISSUE` untuk 35 kegagalan; `PASS` untuk uji denah | Keluaran perintah. Garis dasar memori 15 Oktober 2026 (2870/2844/26) sudah tidak sebanding karena suite bertambah 430 uji dari pekerjaan lain |
| `npm run build` (percobaan pertama) | Berhenti sebelum kompilasi: `Error: EBUSY: resource busy or locked, rmdir '...\.next\standalone'`. Folder dikunci proses `node .next/standalone/server.js` (PID 7636, berjalan sejak 10.35) | `EXISTING / ENVIRONMENT ISSUE` | Agent tidak menghentikan proses milik pengguna |
| `npm run build` (diulang 20 Oktober 2026 sesudah proses itu berhenti sendiri) | `✓ Compiled successfully in 69s` (Next.js 16.2.12, Turbopack), 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build. Working tree saat build memuat perubahan `FE-RWI-272` s.d. `FE-RWI-275` | `PASS` | Keluaran perintah |
| `git diff --check` pada berkas denah | Bersih | `PASS` | Keluaran perintah |
| **Revisi 20 Oktober 2026** — `node --import ./tests/helpers/register.mjs --test` atas berkas uji denah dan 4 berkas uji yang membaca `floor-plan-selection.jsx` | 134 uji, 134 lulus, termasuk uji baru "Area Servis utara tidak ditampilkan" | `PASS` | Keluaran `node:test` |
| **Revisi** — `npm run build` | `✓ Compiled successfully in 60s` (Next.js 16.2.12, Turbopack), 514 halaman statis, `prepare-standalone` berhasil, exit 0. Sebelum build dipastikan tidak ada proses `node .next/standalone/server.js` yang berjalan | `PASS` | Keluaran perintah |
| **Revisi** — `npm run lint` (`eslint . -f json`) | `0 errors`, 1083 warning — sama dengan garis dasar; 5436 berkas; tidak ada warning baru pada berkas yang disentuh | `PASS` | Ringkasan keluaran JSON |
| **Revisi** — pratinjau statis Lantai 3 (screenshot Edge, data samaran) | Lebar 1366 px dan 1920 px: Area Servis beserta Toilet, Spoel hoek, dan Lift sayap utara hilang; kompas di ruang kosong atas tanpa menutupi kartu. Lebar 390 px: tanpa geser horizontal | Bukti pendukung, bukan uji manual | Berkas pratinjau di luar repository |
| Pratinjau statis komponen asli (HTML hasil render + screenshot Edge, data samaran) | Lantai 3 Monitoring tampil sebagai peta tanpa unsur CAD; kompas berada di ruang kosong kanan atas; tanpa geser horizontal pada lebar 1366 px. Lantai 4 tetap gambar lama | Bukti pendukung, bukan uji manual | Berkas pratinjau di luar repository, tidak dilampirkan |
| Uji di aplikasi: Lantai 3 di empat pemakai, satu lantai lain di setiap pemakai, jejak jaringan | Belum dijalankan | `NOT RUN` | Dijalankan pengguna sendiri |

Uji manual: `REQUIRED` — belum dijalankan; dijalankan pengguna sendiri atas keputusan pengguna
20 Oktober 2026, sehingga task ditandai kuning.

`AUTOMATED TEST: node --test tests/unit/floor-plan-schematic-geometry.test.mjs — PASS (11/11; sesudah revisi 12/12 dalam rangkaian uji denah 134/134)`

`AUTOMATED TEST: npm run test:unit — FAIL (35 dari 3300 gagal di berkas uji di luar denah; 112/112 uji yang membaca berkas task ini lulus)`

**Tidak dijalankan:** uji di peramban dan jejak jaringan (keputusan pengguna).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) Lantai 3 di keempat pemakai tanpa unsur CAD | Terpenuhi di source; belum diuji di aplikasi | `floors.js` (Lantai 3 `schematic`); `floor-plan-view.jsx` menggambar `FloorPlanSchematic` dan tidak lagi menggambar judul lembar; tiga berkas pemakai meneruskan `schematic`, termasuk `FloorPlanSelection` yang dipakai admisi, transfer detail episode, dan Workspace Keperawatan; uji "data peta Lantai 3 tidak memuat unsur gambar kerja" |
| (b) Lantai 2, 4, 5, 8, dan 9 tetap gambar lama tanpa galat | Terpenuhi di source; belum diuji di aplikasi | Uji "hanya Lantai 3 yang berskematik"; cabang `drawing` di `FloorPlanView` tidak berubah; pratinjau Lantai 4 |
| (c) Satu panggilan `GET .../floor-plan` per lantai | Terpenuhi di source; jejak jaringan belum diambil | Hook dan service tidak berubah (`git diff` kosong pada `src/lib/hooks` dan `src/lib/services` untuk denah) |
| (d) Skrip geometri Lantai 3 lulus | Terpenuhi | `tests/unit/floor-plan-schematic-geometry.test.mjs` 11/11 |
| (e) Zoom, geser, Ctrl + Scroll, layar penuh, dan kompas tetap bekerja | Terpenuhi di source; belum diuji di aplikasi | Konfigurasi `TransformWrapper`, `ZoomToolbar`, dan `FloorPlanCompass` tidak berubah; kanvas peta melebar di luar `viewBox` sehingga tetap terisi saat diperkecil |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi; berkas denah bersih saat mulai |
| `V-FE` 2 — `lint` | Terpenuhi: `0 errors` |
| `V-FE` 3 — `test:unit` | Dijalankan: 3265/3300 lulus; 35 kegagalan bukan regresi task (berkas uji di luar denah); uji denah 112/112 lulus |
| `V-FE` 4 — `build` | Terpenuhi: percobaan pertama `EBUSY` (proses standalone server), diulang sesudah proses berhenti — `✓ Compiled successfully`, exit 0 |
| `V-FE` 5 — bukti visual di aplikasi | **Belum terpenuhi**: dijalankan pengguna sendiri; Lantai 3 harus dapat ditunjukkan kepada atasan untuk gerbang `DEC-PETA-L3` |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan enam titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint baru. Warning lama berkas denah (`react-hooks/set-state-in-effect` di `floor-plan-master-room-popup.jsx`, `use-floor-plan.js`, `use-floor-plan-layout.js`, `use-floor-plan-import.js`) di luar cakupan |
| Masalah yang diketahui | Penyederhanaan `RWI-FE-010`: inti lift/tangga dan NS3B berhenti di garis koridor. Sesudah revisi 20 Oktober 2026 sayap servis utara tidak ditampilkan; lorong sempit di antara Peralatan dan Pantry tetap digambar sebagai jalur sirkulasi yang berujung di tepi lempeng |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `npm run build` sempat gagal `EBUSY` karena proses `node .next/standalone/server.js` milik pengguna/sesi lain; agent tidak menghentikannya. Sesudah proses itu berhenti, build diulang dan lulus |
| Status Git | Berkas task: `M floor-plan-monitoring.jsx`, `M floor-plan-selection.jsx`, `M floor-plan-view.jsx`, `M floor-plan.module.css`, `M master-data-bed-floor-plan-view.jsx`, `M floors.js`, `?? floor-plan-schematic.jsx`, `?? floor-plan/schematic/`, `?? tests/unit/floor-plan-schematic-geometry.test.mjs`. Berkas lain yang berubah di working tree milik pekerjaan paralel |
| Langkah berikutnya | Uji manual Lantai 3 hasil revisi oleh pengguna di keempat pemakai, lalu tunjukkan kepada atasan; bila disahkan, pengesahan dicatat sebagai keputusan baru di decision log (gerbang `DEC-PETA-L3`) sebelum `FE-RWI-276` s.d. `FE-RWI-278` dimulai |
