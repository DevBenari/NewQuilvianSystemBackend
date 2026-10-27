# Laporan Perubahan Frontend — `FE-RWI-279`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-279` |
| Judul | Penutupan: semua lantai berskematik |
| Slice | `PETA-S3` — penutupan (`MVP-PETA-2`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-279` |
| Trace | `FR-DNH-023`, `FR-DNH-024` (penutupan); `RWI-DEC-345`, `RWI-DEC-351`; `RWI-AC-498`, `499`, `500`, `506` untuk keenam lantai; `DNH-AT-498`–`500`, `506`, `920`, `922`; `NFR-808`, `NFR-812`; `RWI-FE-012` (nasib berkas CAD) |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`. Hash lock [`peta-bangsal-denah-delivery-lock.json`](../../../roadmap/peta-bangsal-denah-delivery-lock.json) dicocokkan ulang 22 Oktober 2026: 18 dari 19 masukan sama persis; dokumen planning `planning-redesain-tampilan-denah-bed.md` hanya berbeda akhir baris (kini CRLF di working tree), dan hash isinya sesudah dinormalkan ke LF sama persis dengan lock (`e15d62b7…`). API contract 14.2 tidak berubah; berkas denah backend (`InpFloorPlanQueryService.cs`, kedua controller denah, `RoomController.cs`, `InpatientFloorPlanDtos.cs`, `FloorPlanLayoutDtos.cs`) identik antara commit snapshot `6e0f2b1b` dan commit rujukan |
| Wewenang UI | Frontend architecture 21.3 dan 21.8: setelah keenam lantai selesai, gambar lama tidak dipanggil lagi; nasib berkas CAD adalah `DEV_DISCRETION` (`RWI-FE-012`); planning 6.2: gaya CAD dihapus setelah tidak ada pemakai, sedangkan `floor-plan-drawing.jsx` serta `layers`/`labels` di `lantai-N.plan.js` disimpan sebagai acuan penelusuran. Batasnya: rupa peta bangsal yang sudah disahkan tidak berubah; `lantai-N.plan.js` dan `rooms[]` tidak dihapus |
| Dependency | `FE-RWI-276`, `FE-RWI-277`, dan `FE-RWI-278` — ketiganya masih 🟡 karena gerbang `DEC-PETA-L3` belum dicatat sebagai keputusan baru di decision log (hash `00-interview-decisions.md` 22 Oktober 2026 masih sama dengan lock). Kartu roadmap bertanda ⛔ "Menunggu `FE-RWI-276`, `FE-RWI-277`, dan `FE-RWI-278`". Task ini dikerjakan atas instruksi eksplisit pengguna sesi (pemilik modul) 22 Oktober 2026 — "⛔ FE-RWI-279 — Penutupan: semua lantai berskematik … task ini tolong di kerjakan" — mengikuti preseden `FE-RWI-277`/`278`. Source keenam lantai sudah lengkap sejak `FE-RWI-276`, sehingga penutupan dapat dikerjakan tanpa menunggu source lain. Tidak ada prasyarat backend |
| Klasifikasi | `HEAVY` — skor 9: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 15 bernilai 2, logika bisnis sederhana 0 (jalur tampilan dilepas, aturan bisnis tetap), memakai kontrak API yang ada 1, database 0, keamanan 0, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `431ac81755be15c93bce4c238e5f8db4b1354ff8` (branch `HamzahV2`, upstream `origin/HamzahV2`; working tree bersih saat mulai) ditambah perubahan working tree task ini |
| Commit backend yang dijadikan rujukan | `41c6451edde4434592f6e4da23f8315152a85b25` (branch `MHamzah`) |
| Tanggal | 22 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Seluruh acceptance criteria terpetakan ke source; jalur gambar lama, renderer kamar lama, legenda lama, dan gaya CAD sudah dilepas; `npm run lint:errors` exit 0; `npm run test:unit` tanpa kegagalan baru; `npm run build` lulus. Yang belum: ketiga task prasyarat masih 🟡 karena pengesahan Lantai 3 belum dicatat sebagai keputusan baru di decision log (gerbang `DEC-PETA-L3`). Uji di aplikasi belum dijalankan; butir ini tidak menahan `✅` atas keputusan pemilik 1 September 2026, tetapi tetap tercantum pada daftar periksa kesiapan rilis (bagian 7) |

---

## 1. Keadaan yang ditemukan di awal

1. **Keenam lantai sudah berskematik.** Sejak `FE-RWI-276` (Lantai 2), `FE-RWI-277` (Lantai 4–5), dan
   `FE-RWI-278` (Lantai 8–9), setiap baris `FLOORS` di `floors.js` membawa properti `schematic`. Dalam
   praktik gambar lama sudah tidak tampil di lantai mana pun.
2. **Tetapi jalur gambar lama masih hidup di kode.** Contohnya:
   - `FloorPlanView` masih memilih lapisan per lantai: `{schematic ? <FloorPlanSchematic … /> : drawing}`,
     `{schematic ? <SchematicLegend … /> : <DrawingLegend … />}`, `variant={schematic ? "card" : "drawing"}`,
     dan atribut `data-floor-plan-layer`.
   - `floor-plan-room.jsx` masih memuat renderer kamar lama `DrawingRoom` beserta `OccupancyBadge` dan
     `describeRoom`. Renderer itu masih menghitung label HD/LD dengan **suara terbanyak**
     (`highCount >= lowCount`), aturan yang sudah dicabut `RWI-DEC-346`.
   - Ketiga pemakai `FloorPlanView` — `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, dan
     `master-data-bed-floor-plan-view.jsx` — masih membuat `<FloorPlanDrawing plan={…} />` lewat
     `useMemo` setiap kali lantai berganti, lalu meneruskannya bersama `viewBox` dan `labelDefaults`
     milik gambar lama.
   - `floor-plan.module.css` masih memuat 609 baris gaya jalur lama yang ikut terkirim ke keempat
     pemakai: 27 kelas gambar CAD (dinding, garis, label sumbu, judul lembar), 15 kelas renderer kamar
     lama, 6 swatch legenda lama, 4 `@keyframes` pulsa pesanan lama, dan 23 token warna/huruf yang
     hanya dipakai gaya-gaya itu.
3. **Uji lama mengunci jalur lama.** Enam uji di enam berkas (`FE-ODC-002`, `FE-RWI-DEP`, satu uji
   Monitoring, dan satu uji masing-masing dari `FE-RWI-272`, `273`, dan `274`) masih menegaskan kelas atau
   cabang gambar lama, sehingga jalur itu tidak dapat dilepas tanpa menyelaraskan ujinya.
4. **Garis dasar `HEAD` `431ac8175`.** `npm run test:unit`: 3396 uji, 3361 lulus, 35 gagal di 27 berkas
   uji. Satu kegagalan berada di berkas denah (`floor-plan-transfer-admission.test.mjs`, uji
   `FIX-EPS-024`), tetapi yang diperiksanya adalah regex pada hook admisi, bukan berkas task ini.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Peta bangsal menjadi satu-satunya rupa denah di keenam lantai dan keempat pemakai
(`RWI-DEC-345`), sehingga tampilan campuran tidak mungkin muncul lagi saat rilis (`RWI-DEC-351`).

**Pelaku.** Petugas Monitoring Bed, petugas admisi (Pilih Bed, termasuk admisi bayi), perawat yang
memindahkan pasien (transfer di detail episode dan di Workspace Keperawatan), dan admin Master Bed.

**Pemicu.** Pengguna membuka salah satu layar denah, lalu berpindah tab lantai.

**Prasyarat.** Data denah lantai dikirim server, satu panggilan per lantai (tidak berubah).

**Langkah utama.**

1. Pengguna membuka layar denah. Kepala halaman menampilkan nama lantai, tab lantai, dan ringkasan bed.
2. Bilah petunjuk menampilkan **legenda peta bangsal** di semua lantai: Bed tersedia, Pesanan menunggu
   masuk, Penuh, HD/LD, dan Kapasitas/terisi. Mode Pilih menambah "Bed terpilih"; Master Bed menambah
   "Ruang belum aktif".
3. Panggung denah menggambar lempeng lantai, koridor, blok zona, dan penanda berikon, lalu kartu kamar
   di atasnya dengan animasi masuk bertahap.
4. Kursor atau fokus keyboard pada kartu membuka kartu ringkas; klik, Enter, atau Spasi membuka pop up
   kamar yang sama seperti sebelumnya. Zoom, geser, `Ctrl` + scroll, layar penuh, dan kompas tetap
   bekerja.

**Yang berubah bagi pengguna: tidak ada.** Karena keenam lantai sudah berskematik sebelum task ini,
cabang gambar lama tidak pernah terpilih. Pembanding markup pada bagian 6 membuktikannya: untuk 6 lantai
× 3 mode, hasil render `FloorPlanView` working tree sama persis dengan `HEAD`, kecuali atribut teknis
`data-floor-plan-layer` yang memang dihapus.

**Contoh angka kartu per layar** (data uji samaran):

| Lantai | Monitoring dan Pilih Bed | Master Bed | Keterangan |
| --- | ---: | ---: | --- |
| 2 | 6 | 6 | Tanpa ruang cadangan |
| 3 | 29 | 31 | Master menambah 2 ruang cadangan |
| 4 | 27 | 28 | Master menambah 1 ruang cadangan |
| 5 | 11 | 11 | — |
| 8 | 13 | 13 | — |
| 9 | 20 | 20 | — |

**Perubahan status.** Tidak ada. Rupa status kartu tetap mengikuti `RWI-DEC-346` dan `RWI-DEC-350`;
label HD tetap dibaca dari `dependencySummary.isHighDependency` server. Aturan suara terbanyak milik
renderer lama kini hilang dari kode, sehingga tidak dapat muncul kembali lewat lantai yang tidak
berskematik.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kamar ada di denah tetapi tidak dikirim server | Bentuk putus-putus tanpa nomor, tidak dapat diklik (urutan 7, tidak berubah) |
| Ruang cadangan di Monitoring atau Pilih Bed | Tidak digambar; hanya Master Bed yang menampilkannya sebagai kartu "Ruang cadangan" |
| Denah gagal dimuat | Tidak berubah: pesan galat dan tombol Coba Lagi/Muat Ulang Denah dari layar pemakai |
| Lantai baru ditambahkan tanpa data skematik | Tidak lagi jatuh ke gambar lama. Uji `FE-RWI-279` mewajibkan setiap baris `FLOORS` punya `schematic`, sehingga kekurangan data tertangkap saat `npm run test:unit`, bukan di depan pengguna |

**Hasil akhir.** Keempat pemakai hanya menggambar peta bangsal. `floor-plan-drawing.jsx`,
`lantai-N.plan.js`, dan `rooms[]` tetap tersimpan; `rooms[]` tetap dipakai registri kunci denah dan
importer susunan.

### 2A. Dasar wewenang — instruksi pengguna sesi 22 Oktober 2026

Pengguna sesi menempelkan judul kartu "⛔ FE-RWI-279 — Penutupan: semua lantai berskematik" dan menulis
"task ini tolong di kerjakan". Penanda ⛔ berasal dari ketiga task prasyarat yang masih 🟡, dan
satu-satunya sebab 🟡 pada ketiganya adalah pencatatan gerbang `DEC-PETA-L3`. Task dikerjakan seperti
preseden `FE-RWI-277`/`278` (dikerjakan atas instruksi eksplisit walau gerbangnya belum dicatat), dan
statusnya ditandai 🟡 sampai gerbang itu tercatat.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md` frontend; `rules/GLOBAL_RULES.md`; `rules/frontend/frontend-architecture.md`,
  `REPORT_TEMPLATE.md`; references `build-module-frontend` (`frontend-project-profile.md`,
  `base-component-decision-gate.md`, `ui-consistency-checklist.md`, `test-policy.md`);
  `rules/rule-output/` (aturan output, lokasi laporan, status roadmap, grafik dependency).
- Blueprint: roadmap, lock, dan traceability peta bangsal; frontend architecture bagian 21 (21.1–21.11);
  PRD 32.19–32.20; acceptance matrix 26 (`DNH-AT-498`–`500`, `506`, `920`, `922`); decision log revision
  62–63 (`RWI-DEC-345`, `RWI-DEC-351`, `RWI-FE-012`, `RWI-AC-498`–`507`); planning redesain 6.2 dan 7;
  laporan `FE-RWI-276` dan `FE-RWI-277`.
- Source frontend: `floor-plan-view.jsx`, `floor-plan-room.jsx`, `floor-plan-drawing.jsx`,
  `floor-plan-schematic.jsx`, `floor-plan-skeleton.jsx`, `floor-plan-bed-status.js`,
  `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, `master-data-bed-floor-plan-view.jsx`,
  `floor-plan.module.css` (seluruh 3.680 baris), `floors.js`, `plan-primitives.js`,
  `floor-plan-room-map.js`, `floor-plan-room-labels.js`, `lantai-N.plan.js` (kelas lapisan `layers`),
  `use-floor-plan.js`, `use-floor-plan-layout.js`, keempat layar pemakai (`bed-management-view.jsx`,
  `inpatient-admission-bed-step.jsx`, `inpatient-bed-transfer.jsx`, `nursing-transfer-form-panel.jsx`),
  dan 19 berkas uji yang membaca kode denah.
- Source backend (baca saja): `[Tags(...)]`, route, dan `[AccessPermission(...)]` ketiga endpoint denah;
  diff berkas denah antara `6e0f2b1b` dan `41c6451e` — kosong.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-view.jsx` | Selalu menggambar `FloorPlanSchematic` dan `SchematicLegend`; bingkai SVG dan rasio lembar dari `schematic.viewBox`. Dihapus: `DrawingLegend`, prop `drawing`/`viewBox`/`labelDefaults`, atribut `data-floor-plan-layer`, cabang `schematic ?` pada kartu ringkas, animasi masuk, dan penangan pointer/fokus, serta penggabungan label renderer lama di `mergeRoomData`. 591 → 510 baris |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room.jsx` | `FloorPlanRoom` tanpa prop `variant`: kartu kamar atau bentuk putus-putus. Dihapus: `DrawingRoom`, `OccupancyBadge`, `describeRoom` (`aria-label` lama bersuara terbanyak), dan konstanta badge lama. Markup kartu tidak berubah. 765 → 593 baris |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-monitoring.jsx` | Impor `FloorPlanDrawing`, `useMemo` gambar, serta prop `drawing`/`viewBox`/`labelDefaults` dilepas |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-selection.jsx` | Sama dengan Monitoring; dipakai Pilih Bed admisi dan kedua layar transfer |
| `src/components/view/health-services/master-data/bed/master-data-bed-floor-plan-view.jsx` | Sama dengan Monitoring, untuk Master Bed |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` | 609 baris jalur lama dihapus lewat skrip berjangkar isi yang menolak jalan bila blok memuat kelas di luar daftar jalur lama: gambar statis CAD (172 baris), renderer kamar lama beserta 4 `@keyframes` pulsa (370), swatch legenda lama (46), aturan `.room` mode Master dan swatch Master lama (21). 23 token `.theme` yang hanya dipakai gaya itu dihapus. Selector warna kelompok kelas kehilangan bagian `.room[...]`; latar `.sheet` langsung memakai `--fp-map-canvas`; magenta kelompok khusus memakai `.page [data-room-group="khusus"]`. 3.680 → 3.032 baris; 471 → 391 aturan; 269 → 221 kelas |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-drawing.jsx` | Hanya komentar kepala: komponen tidak dipanggil lagi sejak `FE-RWI-279`, gaya CAD-nya sudah dihapus, berkas disimpan sebagai acuan penelusuran (`RWI-FE-012`) |
| `src/lib/constants/health-services/inpatient-management/floor-plan/floors.js` | Hanya komentar kepala: `schematic` wajib di setiap lantai; `plan` tetap dipakai untuk `rooms[]`, nama lantai, dan kunci tampilan |
| `tests/unit/floor-plan-legacy-cleanup.test.mjs` | Empat uji baru `FE-RWI-279: …`: tidak ada berkas source yang mengimpor atau merender `FloorPlanDrawing`; keenam lantai berskematik dengan jumlah kartu 6/29/27/11/13/20; 48 kelas jalur lama, `data-floor-plan-layer`, token lama, dan `@keyframes reserved…` tidak ada di stylesheet; setiap `styles.X` yang dipakai 16 komponen denah masih ada di stylesheet |
| `tests/unit/floor-plan-odc-selection.test.mjs` | `FE-ODC-002` kini menguji kartu kamar: mode Pilih tanpa bed layak → `ineligible`, tidak dapat difokus, klik dan keyboard dijaga, `cursor: not-allowed` pada `.card[data-card-state="ineligible"]` |
| `tests/unit/floor-plan-dependency-color.test.mjs` | `FE-RWI-DEP` kini menguji tepi hijau kartu, label HD dari `isHighDependency` server, chip HD/LD di legenda, dan memastikan aturan suara terbanyak serta kelas kamar penuh lama tidak ada |
| `tests/unit/floor-plan-room-card.test.mjs` | Uji `FE-RWI-273/279`: tanpa `variant`, `DrawingRoom`, `OccupancyBadge`, dan `describeRoom`; `FloorPlanRoom` mengembalikan `CardRoom` atau `GhostRoom` |
| `tests/unit/floor-plan-schematic-geometry.test.mjs` | Uji `FE-RWI-272/279`: `FloorPlanView` selalu menggambar `FloorPlanSchematic` dari `schematic.viewBox`; ketiga pemakai tidak lagi meneruskan `drawing`, `viewBox`, atau `labelDefaults` |
| `tests/unit/floor-plan-summary.test.mjs` | Uji `FE-RWI-274/279`: `SchematicLegend` di semua lantai, tanpa `DrawingLegend` |
| `tests/unit/floor-plan-monitoring.test.mjs` | Satu baris: penanda kamar dapat dipilih kini dibaca dari `data-room-selectable` kartu, bukan kelas `roomSelectable` lama |

Hook, service, kontrak API, route, menu, permission, `package.json`, backend, dan database tidak
berubah. `lantai-N.plan.js`, `rooms[]`, `floorPlanKey`, dan `floor-plan-room-popup.jsx` tidak disentuh.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi tetap: data skematik dan fungsi geometri murni di `src/lib/constants/...` →
  komponen feature `FloorPlanSchematic` dan `FloorPlanRoom` → `FloorPlanView` → tiga berkas pemakai →
  empat layar. Tidak ada request API baru; view tidak memanggil Axios.
- Pekerjaan ini murni pelepasan: tidak ada komponen, state, hook, CSS, atau abstraksi baru. Komponen
  tetap tinggal di folder feature denah dengan CSS module sendiri (`RWI-DEC-325` butir 4).

**Keputusan elemen UI (base component decision gate).** Base component tidak wajib untuk modul denah
(`RWI-DEC-325` butir 4). Semua elemen memakai ulang hasil `FE-RWI-272`–`278`.

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Lapisan peta keenam lantai | `FloorPlanSchematic` | `floor-plan-schematic.jsx`; keenam data skematik ada | `REUSE` | Dipanggil tanpa syarat |
| Kartu kamar keenam lantai | `FloorPlanRoom` (`CardRoom`, `GhostRoom`) | Markup 18 render sama dengan `HEAD` | `REUSE` | Satu-satunya renderer |
| Legenda status | `SchematicLegend` | Item per mode sudah ada | `REUSE` | Dipanggil tanpa syarat |
| Kartu ringkas saat kursor atau fokus | `FloorPlanRoomHovercard` | Delegasi event di grup kamar | `REUSE` | Tanpa syarat lapisan |
| Animasi masuk kartu | `.roomEnter` + `--fp-enter-delay` | Sudah menghormati gerak dikurangi | `REUSE` | Tanpa syarat lapisan |
| Latar bingkai peta | `.sheet` + token `--fp-map-canvas` | Token sudah ada | `REUSE` | Dipindah dari selector berlapis ke aturan dasar |

`UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

**Keputusan: nasib `floor-plan-drawing.jsx` dan gaya CAD (`RWI-FE-012`, `DEV_DISCRETION`)**

- **A. Hapus gaya CAD dari `floor-plan.module.css`; simpan `floor-plan-drawing.jsx` tanpa dipanggil dan
  perbarui komentarnya — Rekomendasi, dijalankan.** Sesuai planning 6.2 ("gaya CAD dihapus setelah tidak
  ada pemakai"; berkas gambar "disimpan sebagai acuan penelusuran") dan cakupan roadmap. 609 baris gaya
  mati berhenti terkirim ke keempat pemakai. Konsekuensinya, komponen itu tidak lagi tampil dengan rupa
  aslinya bila suatu saat dipasang kembali tanpa memulihkan gayanya dari riwayat git (`431ac8175`);
  komentar kepalanya menyebut hal itu.
- **B. Pindahkan gaya CAD ke stylesheet tersendiri di samping `floor-plan-drawing.jsx`.** Komponen tetap
  dapat dirender, tetapi menambah berkas baru di luar cakupan roadmap dan gaya mati tetap harus dirawat.
- **C. Biarkan gaya CAD di `floor-plan.module.css`.** Tanpa risiko apa pun, tetapi melanggar cakupan
  "hapus gaya CAD yang tidak terpakai".

**Keputusan: atribut `data-floor-plan-layer`**

- **A. Hapus atributnya dan sederhanakan dua selector yang memakainya — Rekomendasi, dijalankan.**
  Atribut itu hanya berguna selama ada dua lapisan. Latar `.sheet` kini langsung `--fp-map-canvas`;
  magenta kelompok khusus memakai `.page [data-room-group="khusus"]`, yang ditulis sesudah aturan
  `.card`/`.classChip`/`.dialog[data-room-group="khusus"]` sehingga tetap menang. Bukti: latar bingkai
  terukur `rgb(238, 242, 247)` (= `#eef2f7`) dan chip ODC, ICU, serta Perinatologi tetap magenta pada
  screenshot.
- **B. Pertahankan atribut dengan nilai tetap `schematic`.** Tanpa perubahan CSS, tetapi menyisakan
  penanda yang tidak lagi bermakna.

Kedua keputusan ini `DEV_DISCRETION` dan bukan perubahan perilaku base component, sehingga tidak
menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G)** pada 162 baris tak-kosong yang ditambahkan
di 15 berkas task (total 172 baris tambah, 1.005 baris hapus): warna literal 0, tipografi 0, `<button>` 0, `<table>` 0, utilitas tipografi Bootstrap 0,
`!important` 0. Satu `style={{ … }}` adalah jeda animasi `--fp-enter-delay` yang nilainya dinamis per
kartu dan sudah ada sebelumnya; baris itu hanya berpindah karena struktur peta disederhanakan.

**Tafsiran yang dicatat.**

1. "Renderer kamar lama" mencakup `DrawingRoom`, `OccupancyBadge`, `describeRoom`, dan penggabungan
   `label`/`labelLines` di `mergeRoomData`, karena hanya renderer lama yang membacanya. Hook masih
   mengirim `labelLines`; field itu dibiarkan karena hook di luar cakupan (`D-FE`) dan tidak berbahaya.
   `room.label` di `rooms[]` tetap dipakai importer (`floor-plan-import-manifest.js`).
2. Prop `viewBox` dan `labelDefaults` termasuk jalur gambar lama, sehingga ikut dilepas dari
   `FloorPlanView` dan ketiga pemakainya. `plan.viewBox` dan `plan.label` tetap ada di data lantai yang
   disimpan.
3. Uji yang mengunci jalur lama diarahkan ulang ke padanannya di kartu kamar dengan maksud yang sama,
   bukan dihapus. Contoh: `FE-ODC-002` dulu memeriksa `isExplicitlyNonSelectable` di renderer lama, kini
   memeriksa bahwa kamar tanpa bed layak di mode Pilih menjadi `ineligible`, tidak dapat difokus, dan
   klik maupun keyboard-nya dijaga.
4. Token `--fp-reserved-soft` sudah tidak dipakai sebelum task ini; dibiarkan karena bukan bagian jalur
   lama.
5. "Screenshot 6 lantai × 4 pemakai" dipenuhi dengan 6 lantai × 3 mode, karena Pilih Bed admisi dan
   kedua layar transfer memakai `FloorPlanSelection` dengan mode yang sama (`selection`).

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

Task ini tidak menambah atau mengubah panggilan API. Layar yang memakai peta tetap memanggil endpoint
berikut, satu panggilan per lantai. Pembuatan `<FloorPlanDrawing />` yang dihapus tidak memanggil API.

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah satu lantai di Monitoring Bed | `InpatientBedManagement : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah Pilih Bed (admisi, admisi bayi, dan transfer) beserta kelayakan bed | `InpatientBedOccupancy : Read` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan-layout` | Susunan satu lantai untuk editor Master Bed | `Room : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Garis dasar `npm run test:unit` pada `HEAD` `431ac8175`, sebelum edit | 3396 uji: 3361 lulus, 35 gagal di 27 berkas uji | Garis dasar | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test` untuk 19 berkas uji yang membaca kode denah | 169 uji, 168 lulus. Satu-satunya kegagalan, `FIX-EPS-024` di `floor-plan-transfer-admission.test.mjs`, sudah gagal di garis dasar dan memeriksa hook admisi | `PASS` untuk uji denah task ini; `EXISTING / ENVIRONMENT ISSUE` untuk 1 kegagalan | Keluaran `node:test` |
| `npm run lint:errors` | `eslint . --quiet` exit 0 tanpa galat | `PASS` | Keluaran perintah |
| `npx eslint <14 berkas JS/JSX/MJS task> --max-warnings=0` | 0 error, 0 warning; garis dasar berkas yang sama juga 0/0 | `PASS` | Ringkasan keluaran JSON |
| `npm run test:unit` | 3400 uji: 3365 lulus, 35 gagal di 27 berkas uji. Himpunan kegagalannya identik dengan garis dasar; tambahan 4 uji adalah uji baru `FE-RWI-279`, semuanya lulus | `EXISTING / ENVIRONMENT ISSUE` | Perbandingan daftar `test at …` sebelum dan sesudah: tidak ada kegagalan baru, tidak ada yang hilang |
| `npm run build` | `prebuild` (`verify-version-manifest`) valid; `✓ Compiled successfully in 109s` (Next.js 16.2.12, Turbopack); 514 halaman statis; `prepare-standalone` berhasil; exit 0; tanpa baris peringatan. Dijalankan berdampingan dengan `next dev` milik pengguna, yang keluarannya terpisah di `.next/dev` dan tidak dihentikan | `PASS` | Keluaran perintah |
| Analisis pemakaian `floor-plan.module.css` (skrip di folder kerja agent) | 0 rujukan `styles.X` yang menggantung di kode yang dipakai; satu-satunya rujukan ke kelas yang sudah dihapus berasal dari `floor-plan-drawing.jsx` dan kelas lapisan `lantai-N.plan.js` yang memang disimpan tanpa dipanggil | `PASS` | Laporan skrip |
| Pembanding markup `FloorPlanView` `HEAD` vs working tree — render server komponen asli, 6 lantai × 3 mode, data uji samaran | 18 dari 18 render identik setelah atribut `data-floor-plan-layer="schematic"` milik `HEAD` diabaikan; tidak ada kelas CAD atau kelas renderer lama di markup | `PASS` | Skrip render server di luar repository |
| Screenshot Microsoft Edge 6 lantai × 3 mode (lebar 1352 px) dan 6 lantai Monitoring lebar 390 px | Peta bangsal di semua lantai dan mode; tidak ada geser horizontal (`scrollWidth` = `clientWidth` = 390 pada lebar 390 px); latar bingkai `rgb(238, 242, 247)`; legenda Monitoring 5 butir, Pilih 6 (ditambah Bed terpilih), Master 6 (ditambah Ruang belum aktif) | Bukti pendukung, bukan uji manual | Screenshot di luar repository, tidak dilampirkan |
| Hash lock masukan desain | 19 dari 19 isi sama (satu berkas hanya berbeda akhir baris) | `PASS` | Bagian Metadata |
| Uji di aplikasi: 6 lantai × Monitoring, Pilih Bed (termasuk bayi), transfer (detail episode dan Workspace Keperawatan), dan Master; jejak jaringan; keyboard; layar 390 px di dalam aplikasi | Belum dijalankan oleh agent | `NOT RUN` | Butuh login dan data backend; dijalankan pengguna |

Uji manual: `NOT FEASIBLE` untuk agent, karena aplikasi membutuhkan login dan data API backend. Tidak
ada kontrol interaktif yang diubah: jalur klik, Enter, dan Spasi kartu, toleransi geser 6 px, zoom, dan
pop up tetap sama, dan markup-nya terbukti identik dengan `HEAD`. Uji manual di peramban tidak menahan
`✅` atas keputusan pemilik 1 September 2026, tetapi dianjurkan dijalankan pengguna sebelum rilis
(bagian 7).

`MANUAL TEST: NOT FEASIBLE — aplikasi membutuhkan login dan data backend; agent memakai pembanding markup dan screenshot pratinjau statis sebagai bukti pendukung`

`AUTOMATED TEST: node --test (19 berkas uji denah) — FAIL (168 dari 169 lulus; satu-satunya kegagalan FIX-EPS-024 sudah gagal di garis dasar HEAD dan bukan berkas task ini)`

`AUTOMATED TEST: npm run test:unit — FAIL (35 dari 3400 gagal di 27 berkas uji; himpunan kegagalan identik dengan garis dasar HEAD 431ac8175, tidak ada kegagalan baru)`

**Tidak dijalankan:** uji di aplikasi dan jejak jaringan, karena butuh sesi login dan backend yang
berjalan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `DNH-AT-498` lulus untuk keenam lantai — tanpa dinding, pintu, anak tangga, sumbu, angka ukuran, label teknis, maupun "3rd Floor" | Terpenuhi di source; belum diuji di aplikasi | `FloorPlanView` selalu menggambar `FloorPlanSchematic`; uji `FE-RWI-279` keenam lantai berskematik dan gaya CAD terhapus; 18 render tanpa kelas CAD |
| `DNH-AT-499` lulus untuk keenam lantai — kartu 6/29/27/11/13/20 di poligon `rooms[]`, bentuk L, kelas dan nomor dari API | Terpenuhi di source; belum diuji di aplikasi. Pengecualian tetap: kartu penuh ICU CK atas instruksi pemilik pada `FE-RWI-276` | Uji jumlah kartu `FE-RWI-279`; uji geometri `FE-RWI-272`/`276`–`278` lulus; kartu memakai `room.classLabel` dan `room.roomNumber` dari API |
| `DNH-AT-500` lulus — satu peta di empat pemakai, tanpa lapisan atau tombol CAD, ruang cadangan hanya di Master | Terpenuhi di source; belum diuji di aplikasi | Ketiga pemakai `FloorPlanView` hanya meneruskan `schematic` (uji `FE-RWI-272/279`); `FloorPlanSelection` dipakai Pilih Bed admisi dan kedua layar transfer; render: Lantai 3 29 kartu di Monitoring/Pilih dan 31 di Master, Lantai 4 27 dan 28 |
| `DNH-AT-506` lulus — klik, Enter, atau Spasi membuka pop up yang sama; geser lebih dari 6 px tidak membuka; diff tanpa hook, service, backend, database | Terpenuhi di source; belum diuji di aplikasi | Markup kartu identik dengan `HEAD`; `CLICK_MOVE_TOLERANCE` dan `handleRoomSelect` tidak berubah; `floor-plan-room-popup.jsx`, hook, service, backend, dan database tidak ada di diff |
| `DNH-AT-920` lulus — 390 px tanpa geser horizontal; peta tetap dapat diperbesar dan digeser | Terpenuhi pada pratinjau statis; belum diuji di aplikasi | Screenshot 390 px keenam lantai tanpa geser horizontal; `TransformWrapper`, `ZoomToolbar`, dan opsi geser tidak berubah |
| `DNH-AT-922` lulus — satu `GET .../floor-plan` per lantai | Terpenuhi di source; jejak jaringan belum dijalankan | Hook dan service tidak berubah; uji `FE-RWI-274` "tanpa panggilan API baru" lulus |
| Tidak ada pemakai yang masih memanggil gambar lama | Terpenuhi | Uji `FE-RWI-279` memindai seluruh `src/`: tidak ada impor `floor-plan-drawing` maupun `<FloorPlanDrawing` |
| Verifikasi wajib: screenshot 6 lantai × 4 pemakai | Terpenuhi sebagai pratinjau statis komponen asli; screenshot di dalam aplikasi belum | Bagian 6 dan tafsiran 5 |
| Verifikasi wajib: pencarian source membuktikan `FloorPlanDrawing` tidak dirender | Terpenuhi | Uji `FE-RWI-279` dan grep |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi; working tree frontend bersih saat mulai (`HEAD` `431ac8175`) |
| `V-FE` 2 — `lint:errors` | Terpenuhi: exit 0 |
| `V-FE` 3 — `test:unit` | Dijalankan: 3365/3400 lulus; 35 kegagalan identik dengan garis dasar, bukan regresi task |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully in 109s`, exit 0 |
| `V-FE` 5 — bukti visual per lantai dan mode | Pembanding markup dan screenshot pratinjau statis di luar repository; uji di aplikasi belum (tidak menahan `✅`, keputusan 1 September 2026) |
| `V-FE` 6 — data uji samaran | Terpenuhi; tidak ada nama pasien, No. RM, atau identitas asli |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |
| Prasyarat `FE-RWI-276`, `FE-RWI-277`, `FE-RWI-278` sudah `✅` | **Belum terpenuhi**: ketiganya 🟡 karena gerbang `DEC-PETA-L3` belum dicatat di decision log. Inilah satu-satunya alasan status `🟡` |

### Daftar periksa kesiapan rilis (`RWI-DEC-351`, PRD 32.19)

`RWI-DEC-351` menahan rilis ke pengguna rumah sakit sampai keenam lantai selesai, supaya tidak ada
tampilan campuran. Keadaan per 22 Oktober 2026:

| No | Butir kesiapan rilis | Status | Bukti atau pemilik |
| ---: | --- | --- | --- |
| 1 | Keenam lantai tampil tanpa garis CAD di keempat pemakai | Siap di source | Bagian 6 dan 7; `DNH-AT-498`, `DNH-AT-500` |
| 2 | Tampilan campuran tidak mungkin muncul (jalur gambar lama dilepas, setiap lantai wajib berskematik) | Siap | Uji `FE-RWI-279`; `FloorPlanView` tanpa cabang gambar lama |
| 3 | 106 kartu di poligon kamar aslinya | Siap di source | `DNH-AT-499`, `DNH-AT-918` |
| 4 | Pop up, hook, service, API, backend, dan database tidak berubah | Siap | `DNH-AT-506`; diff |
| 5 | `lint:errors` bersih dan `build` lulus | Siap | Bagian 6 |
| 6 | Pengesahan atasan atas contoh Lantai 3 tercatat sebagai keputusan baru (`DEC-PETA-L3`, `DNH-AT-507`) | **Belum** | Muhammad Hamzah mencatat di decision log |
| 7 | `FE-RWI-272` s.d. `FE-RWI-278` naik ke `✅` | **Belum** | `FE-RWI-272`–`275` menunggu uji manual Lantai 3 oleh pengguna untuk ditunjukkan kepada atasan; `FE-RWI-276`–`278` menunggu butir 6 |
| 8 | Uji di aplikasi: 6 lantai × Monitoring, Pilih Bed (termasuk bayi), transfer (detail episode dan Workspace Keperawatan), dan Master; jejak jaringan satu panggilan per lantai; keyboard; layar 390 px | **Belum** (`NOT RUN`) | Pengguna |
| 9 | Gerbang produksi `DNH-G04` (kriteria klinis HD/LD dan persetujuan privasi) | Di luar task; tidak berubah | Pemilik klinis RS |
| 10 | Commit, push, merge ke `QuilvianDevV2`, dan deploy | Belum; dijalankan pengguna | Pisahkan empat berkas pekerjaan paralel (bagian 8) saat commit |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint pada berkas task ini; build tanpa peringatan |
| Masalah yang diketahui | `floor-plan-drawing.jsx` disimpan tanpa gaya, sehingga tidak tampil dengan rupa aslinya bila dipasang kembali tanpa memulihkan gaya dari `431ac8175`. Token `--fp-reserved-soft` sudah mati sebelum task ini dan dibiarkan. Satu kegagalan uji lama di berkas denah (`FIX-EPS-024`) memeriksa hook admisi dan di luar cakupan |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Skrip analisis CSS, skrip render server, halaman pratinjau, dan screenshot hanya ada di folder kerja agent di luar repository |
| Interupsi | `NONE` untuk task ini. Selama pengerjaan, pekerjaan paralel mengubah empat berkas frontend di luar task: `src/style/health-services/inpatient-management/nursing-billing-section.module.css` (pukul 10.26), `.../nursing-workspace/sections/billing/nursing-billing-section.jsx` (10.30), `.../inpatient-newborn-admission-steps.jsx` (10.32), dan `tests/unit/inpatient-newborn-admission.test.mjs` (10.32). Keempatnya tidak disentuh. `npm run test:unit` selesai pukul 10.29 sebelum tiga berkas terakhir berubah, dan himpunan kegagalannya identik dengan garis dasar; `npm run build` (selesai 10.32) mungkin ikut membaca sebagian perubahan paralel itu dan tetap lulus |
| Status Git | Frontend — berkas task: `M floor-plan-drawing.jsx`, `M floor-plan-monitoring.jsx`, `M floor-plan-room.jsx`, `M floor-plan-selection.jsx`, `M floor-plan-view.jsx`, `M floor-plan.module.css`, `M master-data-bed-floor-plan-view.jsx`, `M floors.js`, dan tujuh berkas `tests/unit/floor-plan-*.test.mjs` (`dependency-color`, `legacy-cleanup`, `monitoring`, `odc-selection`, `room-card`, `schematic-geometry`, `summary`). Berkas lain yang berubah (`nursing-billing-section.jsx`, `nursing-billing-section.module.css`, `inpatient-newborn-admission-steps.jsx`, `inpatient-newborn-admission.test.mjs`) milik pekerjaan paralel. Backend: laporan ini (baru), roadmap, dan traceability peta bangsal; berkas `docs/issue/issue-032`/`033` dan `docs/plan-repair/plan-repair-032`/`033` yang belum di-track milik pekerjaan lain. Tidak ada stage, commit, atau push |
| Langkah berikutnya | Muhammad Hamzah mencatat pengesahan Lantai 3 beserta arahan 21 Oktober 2026 (tanpa area servis Lantai 4–5; sayap utara Lantai 2 bernama; ICU CK kartu penuh) sebagai keputusan baru di decision log, lalu `FE-RWI-276` s.d. `FE-RWI-279` dapat dinaikkan ke `✅` dan butir 8 daftar periksa rilis dijalankan sebelum rilis |
