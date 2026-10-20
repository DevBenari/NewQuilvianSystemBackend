# Laporan Perubahan Frontend — `FE-RWI-273`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-273` |
| Judul | Kartu kamar, rupa status, titik bed, dan fokus keyboard |
| Slice | `PETA-S1` — contoh Lantai 3 (`MVP-PETA-1`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-273` |
| Trace | `FR-DNH-018`, `019`, `020`, `021`, `023` (keyboard); `RWI-DEC-344`, `346`, `347`, `350`; `RWI-AC-499`, `501`, `502`, `503`, `504` untuk Lantai 3; `DNH-AT-499`, `501`–`504`, `914`, `915`, `916`, `921`, `923`; `NFR-809`, `NFR-810`; `RWI-FE-009` |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`; hash lock dicocokkan ulang 22 dari 22 sama. API contract 14.2 tidak berubah |
| Wewenang UI | Rupa status 21.6 dan titik bed 21.7 adalah brief produk terkunci (`RWI-DEC-346`, `347`, `350`); warna kelompok kelas `DEV_DISCRETION` dengan syarat jelas berbeda dari hijau, ungu, dan oranye (`RWI-FE-009`); ukuran, jarak, sudut, dan bayangan `DEV_DISCRETION` (21.3); base component tidak wajib (`RWI-DEC-325` butir 4) |
| Dependency | `FE-RWI-272` 🟡 — source lapisan peta dan fungsi geometri lengkap; yang belum hanya `npm run build` (kunci berkas lingkungan) dan uji manual pengguna, sehingga tidak menghalangi pekerjaan ini |
| Klasifikasi | `HEAVY` — skor 11: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 8 bernilai 1, aturan rupa status kompleks 2, memakai kontrak API yang ada 1, database 0, privasi Housekeeping berkaitan 1, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a491b05afcb6b3ed70218ccaa557007719d45ac3` (branch `HamzahV2`) ditambah perubahan working tree `FE-RWI-272` dan task ini |
| Commit backend yang dijadikan rujukan | `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` (branch `MHamzah`) |
| Tanggal | 20 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap untuk keenam acceptance criteria; aturan rupa diuji otomatis; `npm run lint` dan `npm run build` lulus. Yang belum hanya uji manual di aplikasi, yang dijalankan pengguna sendiri |

---

## 1. Keadaan yang ditemukan di awal

1. Sesudah `FE-RWI-272`, Lantai 3 sudah memakai lapisan peta, tetapi kamarnya masih digambar
   renderer lama: garis tepi tipis, label, dan badge di atas lempeng putih.
2. Renderer lama memberi label HD/LD kamar penuh dengan **suara terbanyak**
   (`highCount > 0 && highCount >= lowCount`), memakai tepi teal untuk kamar tersedia, serta oranye
   atau hijau untuk kamar penuh — rupa yang dicabut `RWI-DEC-346` (`RWI-CON-018`).
3. Kamar dengan `isSelectable = false` diberi `tabIndex = -1` di **semua** mode, sehingga kamar penuh
   di Monitoring dan seluruh kamar di Master hanya dapat dibuka dengan tetikus (frontend
   architecture 21.2 no. 10).
4. Nada status bed (`statusTone`) hanya ada di dalam `floor-plan-room-popup.jsx`, sehingga belum
   dapat dipakai bersama oleh kartu kamar.
5. Bukti backend (`InpFloorPlanQueryService.cs`, baca saja): bed tiap kamar sudah diurutkan menurut
   huruf bed; `IsSelectable` Monitoring = ada bed biasa Tersedia; `Counts.Filled` = Terisi +
   Dipesan; `DependencySummary.IsHighDependency = HighCount >= 1`; profil Housekeeping tidak menerima
   `DependencySummary` maupun `HoldingContext`.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas langsung tahu kamar mana yang masih punya bed, mana yang berpesanan, dan mana
yang penuh, tanpa membuka setiap kamar.

**Langkah utama di Monitoring Bed, Lantai 3.**

1. Setiap kamar tampil sebagai kartu bersudut bulat di posisi dan bentuk aslinya; kamar 301 dan 318
   tetap berbentuk L.
2. Kartu memuat strip warna kelompok kelas di sisi atas, nama kelas, nomor kamar besar, satu titik
   per bed, dan badge kapasitas/terisi.
3. Rupa kartu mengikuti aturan berurutan berikut; aturan pertama yang cocok berlaku.

| Urutan | Keadaan kamar | Rupa kartu | Contoh data uji samaran |
| ---: | --- | --- | --- |
| 1 | Mode Pilih, tidak ada bed layak untuk pasien ini | Redup, tidak dapat difokus | Pasien isolasi: semua kamar tanpa bed isolasi tersedia redup |
| 2 | Mode Pilih, berisi bed yang sedang dipilih | Isi amber, halo amber, ikon centang | 321A dipilih → kartu 321A amber |
| 3 | Masih ada bed tersedia | Tepi hijau 2 px; ikon jam ungu bila ada pesanan | 328 (Terisi, Dipesan, Tersedia) → tepi hijau, ikon ungu, `3/2` |
| 4 | Tidak ada bed tersedia, ada pesanan menunggu masuk | Isi ungu tipis, tepi ungu, ikon jam | 328 setelah bed C dipesan → ungu, `3/3` |
| 5 | Penuh tanpa pesanan | Abu netral tanpa tepi berwarna; tetap dapat diklik | 329 berisi 3 pasien → abu, `3/3` |
| 6 | Bed tidak dapat dipakai karena persiapan atau tidak tersedia | Abu muda, tepi putus-putus, teks redup | 325 (Terisi, Menunggu Pembersihan, Dalam Pembersihan) |
| 7 | Bentuk ada di denah tetapi kamar tidak dikirim API | Garis putus-putus tanpa nomor; tidak dapat diklik maupun difokus | Kunci kamar belum diimpor |

4. Kamar penuh (urutan 4 dan 5) diberi label kecil dari ringkasan server: "HD" oranye bila
   sekurang-kurangnya satu penghuni High Dependency, "LD" abu bila tidak ada HD dan ada penghuni LD,
   tanpa label bila semua penghuni belum dinilai atau ringkasan tidak dikirim.

**Contoh label.** Kamar 329 berisi 1 pasien HD dan 2 pasien LD → label "HD", bukan "LD", karena
`dependencySummary.isHighDependency = true`. Kamar 330 berisi 2 pasien yang belum dinilai → tanpa
label.

5. Kartu dapat dibuka dengan klik, `Enter`, atau `Spasi`; pop up yang sama seperti sebelumnya
   terbuka. Menggeser peta lebih dari 6 px tidak membuka pop up.

**Mode lain.**

| Layar | Perilaku kartu |
| --- | --- |
| Pilih Bed admisi, transfer detail episode, Workspace Keperawatan | Urutan 1–3; kartu tidak layak `tabIndex = -1` dan `aria-disabled` |
| Master Bed | Tanpa tepi hijau. Kamar penuh abu, kamar tanpa bed yang dapat dipakai putus-putus, kamar lain putih netral. Ruang cadangan dan kamar belum dipetakan putus-putus berlabel "Ruang cadangan" atau "<kode> belum dipetakan"; semua kartu berdata dapat difokus |
| Profil Housekeeping | Tanpa label HD/LD dan tanpa ikon pesanan; titik bed tetap menunjukkan status, misalnya Dipesan |
| Lantai 2, 4, 5, 8, 9 | Tetap renderer lama, tidak berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Semua berkas `FE-RWI-272` bagian 3.1.
- Tambahan: `floor-plan-room.jsx` (renderer lama), `floor-plan-room-popup.jsx` (`statusTone`,
  `data-status`), `use-floor-plan.js` (`normalizeFloorPlanRoom`, `floorPlanClassGroup`),
  `use-floor-plan-layout.js` (data Master, ruang cadangan), `floor-plan-bed-capacities.js`,
  `InpFloorPlanQueryService.cs` baris 166–262, dan uji `floor-plan-dependency-color`,
  `floor-plan-odc-selection`, `floor-plan-monitoring`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-bed-status.js` | **Baru.** Modul status bersama: `bedStatusTone` (diekstrak apa adanya dari pop up), `roomBedTones`, `resolveRoomCardState` (urutan 1–6 dan mode Master), `resolveDependencyLabel`/`roomCardDependencyLabel` (HD/LD dari server, hanya kamar penuh), `isRoomCardFocusable`, `describeRoomCard` (label pembaca layar), `roomHasActiveReservation`, `BED_DOT_LIMIT = 6` |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room.jsx` | Renderer kartu (`CardRoom`), varian ringkas, kartu cadangan/belum dipetakan, dan bentuk tanpa data API (`GhostRoom`). Renderer lama dipertahankan utuh sebagai `DrawingRoom` untuk lantai gambar lama |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-view.jsx` | Meneruskan `variant` (`card` bila lantai berskematik) dan `mode` ke setiap kamar; komentar `mergeRoomData` diperbarui |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room-popup.jsx` | Hanya mengganti fungsi lokal `statusTone` dengan impor `bedStatusTone`; isi, aksi, dan warna pop up tidak berubah |
| `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/schematic-geometry.js` | Fungsi `topEdges` untuk strip warna kelas bertingkat pada kamar L |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` | Token kartu `--fp-card-*`, nada titik bed `--fp-bed-*` (masih sama dengan pop up), warna kelompok khusus peta `--fp-map-class-khusus` (magenta), gaya kartu per urutan rupa, cincin fokus, dan pengaman `prefers-reduced-motion` untuk gerak kartu. Selektor warna kelompok kelas diperluas ke `.card`; zona peta dibuat lebih terang agar tidak mirip kamar penuh |
| `tests/unit/floor-plan-room-card.test.mjs` | **Baru.** 10 uji aturan kartu untuk kriteria (a)–(f), mode Master, Housekeeping, dan source renderer |
| `tests/unit/floor-plan-dependency-color.test.mjs` | Dua pemeriksaan `statusTone` diarahkan ke modul status bersama, ditambah pemeriksaan bahwa pop up memakai `bedStatusTone`. Pemeriksaan renderer lama tetap |

Hook, service, kontrak API, route, menu, permission, `package.json`, backend, dan database tidak
berubah.

### 3.3 Kepatuhan arsitektur frontend

- Aturan rupa dan nada status adalah fungsi murni di satu modul yang dipakai kartu dan pop up, sehingga
  logika status tidak tersebar di markup dan dapat diuji `node:test`.
- Modul status ditempatkan di folder feature denah, bersebelahan dengan kedua pemakainya
  (`RWI-DEC-325` butir 4).
- Kartu menerima seluruh data lewat props dari `FloorPlanView`; tidak ada request API, Redux, atau
  state global baru.

**Keputusan elemen UI (base component decision gate).**

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu kamar (bentuk, strip, teks, badge, titik bed) | `EXTEND` `FloorPlanRoom` | Tabel Keputusan Reuse roadmap: "`FloorPlanRoom` — Extend; renderer kartu untuk lantai berskematik" |
| Pemetaan status bed | `REUSE` lewat ekstraksi `statusTone` | Tabel Keputusan Reuse: "Diekstrak ke modul bersama" |
| Pop up kamar | `REUSE` tanpa perubahan perilaku | `RWI-DEC-343` |
| Ikon jam pesanan dan centang | `REUSE` `lucide-react` (`Clock3`, `Check`) | Sudah terpasang |
| Badge, label HD/LD | `NEW` di dalam SVG kartu | Base component tidak wajib (`RWI-DEC-325` butir 4); base `Badge`/`StatusBadge` adalah elemen HTML dan tidak dapat dipakai di dalam SVG denah |

Pilihan untuk kartu kamar sudah diputuskan di blueprint: (1) memperluas `FloorPlanRoom` dengan
renderer kartu dan mempertahankan renderer lama — **direkomendasikan dan disetujui** karena lantai
gambar lama tidak ikut berubah dan renderer lama dapat dilepas di `FE-RWI-279`; (2) komponen kartu
terpisah yang dipilih `FloorPlanView` — memecah atribut `data-room-*` dan penanganan klik menjadi dua
tempat; (3) mengubah renderer lama untuk semua lantai — melanggar larangan "tidak mengubah rupa
lantai gambar lama".

`UI GATE: PASS` — tidak ada keputusan `NEW` atau `EXTEND` yang menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G, pada baris yang ditambah).** Tidak ada
`<button>` non-base, `<table>`, utilitas tipografi Bootstrap, maupun `!important` baru. Warna
literal hanya muncul sebagai nilai token di blok `.theme` (pola modul denah yang sudah ada); selektor
gaya hanya memakai `var(...)`. Delapan deklarasi `font-weight` baru mengatur teks SVG milik denah
sendiri dan tidak menimpa komponen shared.

**Tafsiran yang dicatat.**

1. Master Bed: 21.6 menyebut "urutan 5–7 tanpa tepi hijau", tetapi tidak mengatur kamar Master yang
   masih punya bed terpakai. Kamar itu tampil putih netral tanpa tepi berwarna.
2. Varian ringkas memakai lebar atau tinggi poligon kamar < 56 satuan. Tidak ada kamar Lantai 3 yang
   ringkas. Dari data denah, kamar 521 (67,5 × 76,5) dan 522 (59 × 56) di Lantai 5 tidak memenuhi
   aturan ini walau disebut planning; hal ini diputuskan saat `FE-RWI-277`.
3. Titik bed = satu titik per baris bed yang dikirim API, sama dengan baris bed di pop up, termasuk
   boks bayi bila ada.
4. Pada task ini warna titik masih sama dengan palet pop up saat ini (tersedia teal, terisi LD hijau).
   Penyelarasan palet `RWI-DEC-353` adalah `FE-RWI-275`.
5. Pop up tidak berubah, sehingga warna kelas khusus di dalam pop up tetap hijau sementara kartu dan
   legenda kelas pada lantai berskematik memakai magenta.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah pada task ini; kerangka berbentuk kartu dikerjakan `FE-RWI-274` |
| Kosong | Kamar yang tidak dikirim API tampil sebagai garis putus-putus tanpa nomor, tidak dapat diklik maupun difokus. Ruang cadangan tidak tampil di luar Master Bed |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." beserta tombol Coba Lagi dari layar pemakai |
| Tanpa hak akses | Tidak berubah. Profil Housekeeping melihat kartu tanpa label HD/LD dan tanpa ikon pesanan |

---

## 5. Endpoint yang dikonsumsi

Task ini tidak menambah atau mengubah panggilan API. Data kartu berasal dari respons yang sama
dengan `FE-RWI-272` bagian 5.

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Kelas, nomor, badge, `IsSelectable`, `DependencySummary`, dan status bed kartu di Monitoring | `InpatientBedManagement : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Kartu Pilih Bed beserta kelayakan bed per pasien | `InpatientBedOccupancy : Read` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan-layout` | Kartu Master Bed, termasuk ruang cadangan | `Room : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test` atas 11 berkas uji denah dan 4 berkas uji yang membaca `floor-plan-selection.jsx` | 122 uji, 122 lulus, termasuk 10 uji baru `floor-plan-room-card.test.mjs` | `PASS` | Keluaran `node:test` |
| `npx eslint` pada 7 berkas yang disentuh | Exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` | Exit 0: `0 errors`, 1083 warning — sama persis dengan garis dasar sebelum `FE-RWI-272` (1083) | `PASS` | Keluaran perintah |
| `npm run test:unit` | 3310 uji: 3275 lulus, 35 gagal. Daftar 35 kegagalan identik dengan sebelum task ini (dibandingkan nama per nama) dan berada di berkas uji di luar denah | `EXISTING / ENVIRONMENT ISSUE` untuk 35 kegagalan; `PASS` untuk uji denah | Keluaran perintah |
| `npm run build` | Saat task ini divalidasi, folder `.next\standalone` masih dikunci proses `node .next/standalone/server.js` (PID 7636). Sesudah proses itu berhenti sendiri, build diulang 20 Oktober 2026 pada working tree yang memuat `FE-RWI-272` s.d. `FE-RWI-275`: `✓ Compiled successfully in 69s`, 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build | `PASS` | Keluaran perintah; agent tidak menghentikan proses milik pengguna |
| Pratinjau statis komponen asli (HTML hasil render + screenshot Edge, data samaran) | Monitoring: 328 tepi hijau + ikon jam + `3/2`; 329 abu + "HD" + `3/3`; 330 abu tanpa label; 327 ungu; 325 putus-putus; ODC magenta. Housekeeping: tanpa label dan ikon pesanan; 312/313 tanpa data jadi putus-putus tanpa nomor. Pilih: hanya kamar layak bertepi hijau, kamar terpilih amber. Master: tanpa tepi hijau, ruang cadangan berlabel. Tanpa geser horizontal pada 1366 px | Bukti pendukung, bukan uji manual | Berkas pratinjau di luar repository, tidak dilampirkan |
| Uji di aplikasi dengan data uji 328/329/330, akun Housekeeping, keyboard di Monitoring dan Master, mode Pilih pasien isolasi | Belum dijalankan | `NOT RUN` | Dijalankan pengguna sendiri |

Uji manual: `REQUIRED` — belum dijalankan; dijalankan pengguna sendiri atas keputusan pengguna
20 Oktober 2026, sehingga task ditandai kuning.

`AUTOMATED TEST: node --test tests/unit/floor-plan-room-card.test.mjs (bersama uji denah lain) — PASS (122/122)`

`AUTOMATED TEST: npm run test:unit — FAIL (35 dari 3310 gagal di berkas uji di luar denah, identik dengan sebelum task)`

**Tidak dijalankan:** uji di peramban, akun Housekeeping, dan keyboard di aplikasi (keputusan
pengguna).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) 328 Terisi/Dipesan/Tersedia → tepi hijau, ikon ungu, `3/2` | Terpenuhi di source; belum diuji di aplikasi | `resolveRoomCardState` → `available`; ikon `Clock3` pada `CardIndicators`; badge `capacity/occupied`; uji "(a)" |
| (b) 329 penuh 1 HD + 2 LD → abu, label "HD", `3/3` | Terpenuhi di source; belum diuji di aplikasi | `resolveDependencyLabel` membaca `isHighDependency`; `.card[data-card-state="full"]`; uji "(b)" termasuk label pembaca layar `DNH-AT-914` |
| (c) 330 penuh, semua belum dinilai → tanpa label | Terpenuhi di source; belum diuji di aplikasi | Uji "(c)" |
| (d) Akun Housekeeping → tanpa label dan ikon pesanan | Terpenuhi di source; belum diuji di aplikasi | `dependencySummary = null` → tanpa label; tanpa `holdingContext` → tanpa pesanan; uji "(d)" |
| (e) Kamar tanpa data API → putus-putus, tidak dapat diklik | Terpenuhi di source; belum diuji di aplikasi | `GhostRoom`: path `.cardGhost` tanpa `role`, `tabIndex`, maupun handler; `pointer-events: none` |
| (f) Tab dan Enter membuka kamar penuh di Monitoring dan kamar di Master | Terpenuhi di source; belum diuji di aplikasi | `tabIndex={focusable ? 0 : -1}` dengan `isRoomCardFocusable`; handler `Enter`/`Spasi`; uji "(f)" |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi; `floor-plan-room.jsx` dan pop up belum disentuh pekerjaan lain |
| `V-FE` 2 — `lint` | Terpenuhi: `0 errors`, 1083 warning (garis dasar) |
| `V-FE` 3 — `test:unit` | Dijalankan: 3275/3310 lulus; 35 kegagalan bukan regresi task |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully`, exit 0 (diulang sesudah kunci `EBUSY` lepas) |
| `V-FE` 5 — bukti visual di aplikasi | **Belum terpenuhi**: dijalankan pengguna sendiri |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan enam titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint baru pada berkas yang disentuh |
| Masalah yang diketahui | Warna titik masih palet pop up lama sampai `FE-RWI-275`; legenda kepala halaman masih legenda lama sampai `FE-RWI-274`. Kartu kamar 301 hanya punya strip di lengan kecilnya karena strip mengikuti sisi atas bentuk L |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `npm run build` sempat tertahan proses `node .next/standalone/server.js`; diulang dan lulus sesudah proses itu berhenti |
| Status Git | Berkas task: `M floor-plan-room.jsx`, `M floor-plan-room-popup.jsx`, `M floor-plan-view.jsx`, `M floor-plan.module.css`, `M tests/unit/floor-plan-dependency-color.test.mjs`, `?? floor-plan-bed-status.js`, `?? tests/unit/floor-plan-room-card.test.mjs`, dan `schematic-geometry.js` (masih untracked dari `FE-RWI-272`). Berkas lain yang berubah di working tree milik pekerjaan paralel |
| Langkah berikutnya | Uji manual oleh pengguna: data uji 328/329/330, akun Housekeeping, keyboard di Monitoring dan Master, mode Pilih untuk pasien isolasi |
