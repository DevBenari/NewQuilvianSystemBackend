# Laporan Perubahan Frontend — `FE-RWI-274`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-274` |
| Judul | Kepala halaman, kartu ringkas, kerangka, kompas, dan gerak |
| Slice | `PETA-S1` — contoh Lantai 3 (`MVP-PETA-1`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-274` |
| Trace | `FR-DNH-022`; `RWI-DEC-348`; `RWI-AC-505`; `DNH-AT-505`, `919`, `920`, `922`; `NFR-808`, `811`, `812`; `RWI-FE-011` |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`; hash lock dicocokkan ulang 22 dari 22 sama. API contract 14.2 tidak berubah |
| Wewenang UI | Ringkasan lantai dan angka tab adalah brief produk terkunci (`RWI-DEC-348`); kartu ringkas, animasi, ikon, dan huruf `DEV_DISCRETION` dengan syarat tanpa identitas pasien dan animasi mati saat gerak dikurangi (`RWI-FE-011`); base component tidak wajib (`RWI-DEC-325` butir 4) |
| Dependency | `FE-RWI-273` 🟡 — source kartu kamar lengkap; yang belum hanya `npm run build` (kunci berkas lingkungan) dan uji manual pengguna |
| Klasifikasi | `HEAVY` — skor 10: dua repository 2, berkas diperiksa > 20 bernilai 2, berkas diubah 8 bernilai 1, logika ringkasan dan penempatan kartu ringkas sedang 1, memakai kontrak API yang ada 1, database 0, privasi kartu ringkas berkaitan 1, empat pemakai layar 2 |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a491b05afcb6b3ed70218ccaa557007719d45ac3` (branch `HamzahV2`) ditambah perubahan working tree `FE-RWI-272`, `FE-RWI-273`, dan task ini |
| Commit backend yang dijadikan rujukan | `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` (branch `MHamzah`) |
| Tanggal | 20 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap untuk keenam acceptance criteria; `npm run lint` dan `npm run build` lulus. Yang belum hanya uji manual di aplikasi, yang dijalankan pengguna sendiri |

---

## 1. Keadaan yang ditemukan di awal

1. Kepala halaman denah hanya memuat judul lantai dan tab lantai tanpa angka; tidak ada ringkasan
   bed lantai aktif.
2. Respons `GET .../floor-plan` sudah membawa `floors[].availableBedCount` (dihitung server dari bed
   berstatus Tersedia) dan dinormalisasi `use-floor-plan.js` menjadi `floors[].availableBedCount`,
   tetapi angka itu belum ditampilkan. Daftar lantai Master Bed dibentuk dari `FLOORS` tanpa angka.
3. Legenda masih legenda lama (tepi teal, "Penuh (High Dependency)" oranye, "Penuh (Low
   Dependency)" hijau) yang tidak cocok dengan kartu kamar `FE-RWI-273`.
4. Kerangka pemuatan meniru gambar kerja: grid biru dan blok putus-putus. Kompas berukuran 52 px.
5. Belum ada kartu ringkas saat kursor atau fokus, dan belum ada animasi masuk kartu.

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Petugas langsung melihat kondisi satu lantai dan lantai mana yang masih punya bed kosong,
tanpa membuka kamar satu per satu.

**Langkah utama.**

1. Petugas membuka Monitoring Bed. Setiap tab lantai menampilkan jumlah bed kosong dari server,
   misalnya **Lantai 3 `20`**.
2. Di bawah kepala halaman tampil ringkasan bed lantai aktif:
   **48 bed · 20 tersedia · 22 terisi · 2 dipesan · 3 persiapan · 1 tidak tersedia**. Persiapan =
   Menunggu Pembersihan + Dalam Pembersihan. Status bed yang tidak dikenal dihitung "tidak
   tersedia", sehingga kelima angka selalu berjumlah sama dengan total bed lantai itu.
3. Legenda di bilah alat menjelaskan rupa kartu dengan contoh kartu mini: Bed tersedia (tepi hijau),
   Pesanan menunggu masuk (ungu), Penuh (abu), label HD/LD, dan badge kapasitas/terisi. Mode Pilih
   ditambah "Bed terpilih"; Master Bed ditambah "Ruang belum aktif". Lantai yang masih memakai gambar
   lama tetap memakai legenda lama.
4. Saat kursor berada di atas kartu kamar, atau kartu mendapat fokus keyboard, muncul kartu ringkas
   berisi kelas, nomor, keadaan kamar, label HD/LD bila ada, dan jumlah bed per status. Kartu ringkas
   tidak memuat nama pasien, No. RM, jenis kelamin, lencana tier, maupun Guid. Tombol `Esc`, klik
   kamar, zoom, atau geser menutupnya.
5. Saat lantai dibuka, kartu muncul bertahap (memudar dan naik 4 px, jeda 12 ms per kartu, paling
   lama 300 ms). Bila pengguna memilih gerak dikurangi di sistem operasinya, kartu langsung tampil
   tanpa animasi.

**Contoh kartu ringkas.** Kamar 328 (Terisi, Dipesan, Tersedia): "KELAS II 328 — Masih ada bed
tersedia — 1 tersedia · 1 terisi · 1 dipesan — Ada pesanan menunggu konfirmasi masuk".

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Respons tanpa `floors` (cadangan daftar lantai bawaan) dan Master Bed | Tab tampil tanpa angka |
| Lantai tanpa kamar berdata | Ringkasan berganti menjadi "Belum ada kamar pada lantai ini." |
| Denah sedang dimuat | Kerangka berbentuk kartu kamar di atas lempeng lantai beserta kartu status pemuatan |
| Layar 390 px | Tab, ringkasan, dan legenda turun baris; halaman tidak bergeser horizontal; peta tetap dapat diperbesar dan digeser |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Semua berkas `FE-RWI-272` dan `FE-RWI-273` bagian 3.1.
- Tambahan: `floor-plan-skeleton.jsx`, `floor-plan-compass.jsx`, normalisasi `floors` di
  `use-floor-plan.js`, daftar lantai Master di `use-floor-plan-layout.js`, dan perhitungan
  `AvailableBedCount` di `InpFloorPlanQueryService.cs` baris 132–143.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-summary.jsx` | **Baru.** Ringkasan bed lantai aktif: total dan lima kelompok status; pesan kosong bila lantai tanpa kamar berdata |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-room-hovercard.jsx` | **Baru.** Kartu ringkas HTML di atas peta, tanpa identitas pasien, disembunyikan dari pembaca layar karena isinya sudah ada di label kartu |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-view.jsx` | Angka bed kosong di tab; ringkasan di bawah kepala halaman; legenda per rupa lantai (`SchematicLegend`, legenda lama dipindah utuh ke `DrawingLegend`); kartu ringkas lewat delegasi event pada kelompok kamar; pembungkus animasi masuk kartu |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-bed-status.js` | `BED_SUMMARY_GROUPS`, `bedSummaryGroup`, `summarizeBeds`, `summarizeRoomBeds`, dan `roomStateLabel` |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-skeleton.jsx` | Kerangka berbentuk kartu: dua baris kartu ber-shimmer dan koridor di atas lempeng lantai; kartu status pemuatan dipertahankan |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-compass.jsx` | Ukuran ikon kompas 52 → 40 px |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` | Gaya angka tab, ringkasan, legenda kartu mini, kartu ringkas, animasi masuk, kerangka kartu (menggantikan gaya grid dan blok putus-putus yang tidak dipakai lagi), kompas yang lebih kecil, serta pengaman `prefers-reduced-motion` |
| `tests/unit/floor-plan-summary.test.mjs` | **Baru.** 6 uji ringkasan, kartu ringkas, angka tab, legenda, dan gerak dikurangi |

Hook, service, kontrak API, route, menu, permission, `package.json`, backend, dan database tidak
berubah.

### 3.3 Kepatuhan arsitektur frontend

- Ringkasan dan kartu ringkas menghitung dari data yang sudah ada di props `FloorPlanView`; tidak ada
  request, hook, atau state global baru (`NFR-808`, `DNH-AT-922`).
- Kartu ringkas dibuka lewat delegasi event pada kelompok kamar, sehingga kartu kamar yang di-memo
  tidak ikut me-render ulang ketika kursor berpindah.
- Logika pengelompokan status berada di modul murni `floor-plan-bed-status.js` dan diuji `node:test`.

**Keputusan elemen UI (base component decision gate).**

| Elemen | Status | Bukti |
| --- | --- | --- |
| Ringkasan bed lantai | `NEW` (`floor-plan-summary.jsx`) | Struktur target 21.8 menandai `floor-plan-summary.jsx` `[Baru]`; disetujui `RWI-DEC-352` |
| Kartu ringkas | `NEW` (`floor-plan-room-hovercard.jsx`) | Struktur target 21.8 `[Baru]`; `RWI-FE-011` |
| Tab lantai | `EXTEND` (angka bed kosong) | Tabel Keputusan Reuse roadmap: `FloorTabs` Extend |
| Legenda | `EXTEND` (legenda per rupa lantai) | Tabel Keputusan Reuse: `ClassLegend`/legenda Extend; legenda lama dipindah utuh |
| Kerangka dan kompas | `EXTEND` (gaya) | Tabel Keputusan Reuse: `FloorPlanSkeleton`, `FloorPlanCompass` Extend |

Pilihan untuk elemen `NEW` sudah diputuskan di blueprint: ringkasan sebagai komponen feature baru di
kepala halaman — **direkomendasikan dan disetujui** (21.8) — dibanding (2) memakai base
`SummaryCard`, yang berbentuk kartu metrik besar, menambah tinggi kepala halaman, dan dilepas dari
kewajiban oleh `RWI-DEC-325` butir 4. Kartu ringkas sebagai HTML di atas SVG — **disetujui**
(planning 6.2) — dibanding (2) tooltip bawaan `title`, yang tidak dapat memuat jumlah per status dan
tidak muncul lewat fokus keyboard.

`UI GATE: PASS` — tidak ada keputusan `NEW` atau `EXTEND` yang menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G, pada baris yang ditambah).** Tidak ada
`<button>` non-base, `<table>`, utilitas tipografi Bootstrap, maupun `!important` baru. Gaya baru
memakai token (`var(...)`); bayangan kartu ringkas memakai token modul `--fp-shadow-soft`.
Deklarasi `font-size`/`font-weight` baru hanya mengatur teks milik denah sendiri (angka tab,
ringkasan, legenda, kartu ringkas) dan tidak menimpa komponen shared.

**Tafsiran yang dicatat.**

1. Ringkasan dan angka tab tampil di semua lantai karena merupakan bagian kepala halaman, bukan
   lapisan gambar; legenda tetap per rupa lantai sesuai cakupan task.
2. Legenda Master mengikuti 21.5 secara harfiah: daftar dasar ditambah "Ruang belum aktif". Kartu
   Master tidak bertepi hijau, tetapi "Bed tersedia" tetap relevan untuk titik bed.
3. Ringkasan Master dihitung dari kamar berdata pada data susunan; ruang cadangan dan kamar belum
   dipetakan tidak dihitung karena tidak punya bed.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka berbentuk kartu kamar dan koridor di atas lempeng lantai, ditambah kartu status "Menyiapkan Denah & Status Bed..." |
| Kosong | "Belum ada kamar pada lantai ini." menggantikan ringkasan; tab tanpa angka bila respons tidak membawa `floors` |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." beserta tombol Coba Lagi dari layar pemakai |
| Tanpa hak akses | Tidak berubah. Kartu ringkas profil Housekeeping tanpa label HD/LD dan tanpa catatan pesanan |

---

## 5. Endpoint yang dikonsumsi

Task ini tidak menambah atau mengubah panggilan API; ringkasan, angka tab, dan kartu ringkas memakai
respons yang sudah dimuat (`DNH-AT-922`).

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | `Floors[].AvailableBedCount` untuk angka tab, `Rooms[].Beds[].StatusCode` untuk ringkasan | `InpatientBedManagement : Read` |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan` | Angka tab dan ringkasan pada Pilih Bed | `InpatientBedOccupancy : Read` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/floor-plan-layout` | Ringkasan Master dari bed data susunan; tab Master tanpa angka | `Room : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test` atas 12 berkas uji denah dan 4 berkas uji yang membaca `floor-plan-selection.jsx` | 128 uji, 128 lulus, termasuk 6 uji baru `floor-plan-summary.test.mjs` | `PASS` | Keluaran `node:test` |
| `npx eslint` pada 7 berkas yang disentuh | Exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` (`eslint .`) | Exit 0: `0 errors`, 1083 warning pada 5435 berkas — sama persis dengan garis dasar (1083); tidak ada warning baru pada berkas yang disentuh. Keluaran teks percobaan pertama terpotong oleh galat memori Git Bash (`MEM_COMMIT failed, Win32 error 1455`), sehingga diulang dengan `eslint . -f json` | `PASS` | Ringkasan JSON ESLint |
| `npm run test:unit` | 3316 uji: 3281 lulus, 35 gagal. Daftar 35 kegagalan identik dengan sebelum `FE-RWI-272` (dibandingkan nama per nama) dan berada di berkas uji di luar denah | `EXISTING / ENVIRONMENT ISSUE` untuk 35 kegagalan; `PASS` untuk uji denah | Keluaran perintah |
| `npm run build` | Saat task ini divalidasi, `.next\standalone` masih dikunci proses `node .next/standalone/server.js` (PID 7636). Sesudah proses itu berhenti sendiri, build diulang 20 Oktober 2026 pada working tree yang memuat `FE-RWI-272` s.d. `FE-RWI-275`: `✓ Compiled successfully in 69s`, 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build | `PASS` | Keluaran perintah |
| Pratinjau statis komponen asli (HTML hasil render + screenshot Edge, data samaran) | Lebar 1366 px: tab berangka (Lantai 3 `20`), ringkasan 47 bed yang jumlah kelompoknya cocok, legenda kartu mini, kartu ringkas 328 dan 329, kompas kecil; Master tanpa angka tab dan dengan "Ruang belum aktif"; kerangka berbentuk kartu. Lebar 390 px: Monitoring dan Master tanpa geser horizontal (selisih `scrollWidth` 0 px) | Bukti pendukung, bukan uji manual | Berkas pratinjau di luar repository, tidak dilampirkan |
| Uji di aplikasi: jejak jaringan per lantai, emulasi gerak dikurangi, lebar 390 px | Belum dijalankan | `NOT RUN` | Dijalankan pengguna sendiri |

Uji manual: `REQUIRED` — belum dijalankan; dijalankan pengguna sendiri atas keputusan pengguna
20 Oktober 2026, sehingga task ditandai kuning.

`AUTOMATED TEST: node --test tests/unit/floor-plan-summary.test.mjs (bersama uji denah lain) — PASS (128/128)`

`AUTOMATED TEST: npm run test:unit — FAIL (35 dari 3316 gagal di berkas uji di luar denah, identik dengan sebelum task)`

**Tidak dijalankan:** jejak jaringan, emulasi gerak dikurangi, dan lebar 390 px di aplikasi
(keputusan pengguna).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) Ringkasan Lantai 3 pada data uji berjumlah sama dengan total bed lantai | Terpenuhi di source; belum diuji di aplikasi | `summarizeBeds` menghitung setiap bed tepat ke satu kelompok; uji "(a)" dengan 48 bed |
| (b) Tab Lantai 3 menampilkan `availableBedCount` dari server | Terpenuhi di source; belum diuji di aplikasi | `FloorTabs`: `Number.isFinite(floor.availableBedCount)` |
| (c) Tab Master tanpa angka | Terpenuhi di source; belum diuji di aplikasi | Daftar lantai Master tanpa `availableBedCount` (`use-floor-plan-layout.js`); uji "(b)(c)" |
| (d) Gerak dikurangi → tanpa animasi | Terpenuhi di source; belum diuji di aplikasi | `@media (prefers-reduced-motion: reduce)` untuk `.roomEnter`, `.hovercard`, kerangka, dan gerak kartu; uji "(d)" |
| (e) 390 px tanpa geser horizontal | Terpenuhi di source; diperiksa pada pratinjau statis, belum di aplikasi | Tab, ringkasan, dan legenda `flex-wrap`; selisih `scrollWidth` 0 px pada pratinjau 390 px |
| (f) Satu panggilan API per lantai | Terpenuhi di source; jejak jaringan belum diambil | Tidak ada impor service/Axios baru; uji "legenda per rupa lantai ... tanpa panggilan API baru" |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi |
| `V-FE` 2 — `lint` | Terpenuhi: `0 errors`, 1083 warning (garis dasar) |
| `V-FE` 3 — `test:unit` | Dijalankan: 3281/3316 lulus; 35 kegagalan bukan regresi task |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully`, exit 0 (diulang sesudah kunci `EBUSY` lepas) |
| `V-FE` 5 — bukti visual di aplikasi | **Belum terpenuhi**: dijalankan pengguna sendiri |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan enam titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint baru pada berkas yang disentuh |
| Masalah yang diketahui | Titik ringkasan, legenda, dan kartu masih memakai nada titik lama sampai `FE-RWI-275` menyelaraskan palet. Filter kelas, pilihan "hanya kamar tersedia", dan pencarian nomor kamar sengaja tidak dibuat (`RWI-DEC-348`) |
| Dependency backend | `NONE` |
| Perubahan sampingan | Gaya CSS kerangka lama (`blueprintGrid`, `blueprintWireframe`, `wireframe*`) dihapus karena tidak dipakai lagi oleh `floor-plan-skeleton.jsx`; pencarian source membuktikan tidak ada pemakai lain |
| Interupsi | `npm run build` sempat tertahan proses `node .next/standalone/server.js`; diulang dan lulus sesudah proses itu berhenti. Keluaran teks `npm run lint` pertama terpotong galat memori Git Bash, sehingga lint diulang dengan format JSON |
| Status Git | Berkas task: `M floor-plan-view.jsx`, `M floor-plan-skeleton.jsx`, `M floor-plan-compass.jsx`, `M floor-plan.module.css`, `?? floor-plan-summary.jsx`, `?? floor-plan-room-hovercard.jsx`, `?? tests/unit/floor-plan-summary.test.mjs`, dan `floor-plan-bed-status.js` (masih untracked dari `FE-RWI-273`). Berkas lain yang berubah di working tree milik pekerjaan paralel |
| Langkah berikutnya | Uji manual oleh pengguna: jejak jaringan per lantai, emulasi gerak dikurangi, dan lebar 390 px di aplikasi |
