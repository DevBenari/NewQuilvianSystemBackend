# Laporan Perubahan Frontend — `FE-RWI-275`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-275` |
| Judul | Satu palet titik status di kartu dan pop up |
| Slice | `PETA-S1` — contoh Lantai 3 (`MVP-PETA-1`) |
| Roadmap | [`frontend-roadmap-peta-bangsal-denah.md`](../../../roadmap/frontend-roadmap-peta-bangsal-denah.md), kartu `FE-RWI-275` |
| Trace | `FR-DNH-025` (`EPIC DNH-08`); `RWI-DEC-353`; `RWI-AC-508`; `DNH-AT-508`; `UAT-819`, `UAT-820` |
| Contract version | `0.18.0` — `approved_design_planned` lewat `RWI-DEC-352`; palet diputuskan `RWI-DEC-353` (`PETA-OPEN-01` opsi A); hash lock dicocokkan ulang 22 dari 22 sama |
| Wewenang UI | Tiga warna kunci terkunci (`RWI-DEC-353`): hijau hanya Tersedia, ungu hanya Dipesan, oranye hanya terisi High Dependency. Warna status lain `DEV_DISCRETION` dengan syarat tidak bertabrakan dengan ketiganya. Di pop up hanya warna status yang berubah; isi, teks, tata letak, dan aksi tetap |
| Dependency | `FE-RWI-273` 🟡 — titik bed kartu sudah memakai token `--fp-bed-*`; yang belum hanya uji manual pengguna |
| Klasifikasi | `MEDIUM` — skor 7: dua repository 2, berkas diperiksa 9–20 bernilai 1, berkas diubah 3 bernilai 0, logika sederhana 0, memakai kontrak API yang ada 1, database 0, keamanan 0, empat pemakai pop up dan kartu 2 (dibulatkan dari pembacaan konservatif atas dampak lintas layar) |
| Task mode | `FRONTEND` untuk source; wewenang lintas repository sempit untuk laporan ini, roadmap, dan traceability |
| Target tulis | `QuilvianSystemFrontendDev/src/**` dan `tests/unit/**`; laporan ini di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a491b05afcb6b3ed70218ccaa557007719d45ac3` (branch `HamzahV2`) ditambah perubahan working tree `FE-RWI-272` s.d. task ini |
| Commit backend yang dijadikan rujukan | `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` (branch `MHamzah`) |
| Tanggal | 20 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap untuk ketiga acceptance criteria, palet diuji otomatis, `npm run lint` dan `npm run build` lulus. Yang belum hanya uji manual di aplikasi, yang dijalankan pengguna sendiri |

---

## 1. Keadaan yang ditemukan di awal

1. Pop up kamar mewarnai titik status dan tepi kiri baris bed dengan palet lama
   (`floor-plan.module.css`, `.bedRow[data-status]` dan `.bedRow[data-status] .bedStatusDot`):
   tersedia = warna utama (teal), terisi LD = hijau (`--color-success`), terisi HD dan Menunggu
   Pembersihan = `--color-warning` (oranye kecokelatan), dalam pembersihan = `--color-info`.
2. Titik bed kartu kamar (`FE-RWI-273`) sudah menunjuk token `--fp-bed-*`, tetapi nilai token itu
   masih menyalin palet lama tersebut. Akibatnya titik hijau di kartu berarti terisi LD, padahal tepi
   hijau kartu berarti bed tersedia — konflik yang dicatat `PETA-OPEN-01`.
3. Tag ketergantungan pada baris bed pop up memakai hijau untuk "Low Dependency" dan oranye
   kecokelatan untuk "High Dependency".

---

## 2. Proses bisnis dari sisi pengguna

**Tujuan.** Satu bahasa warna di seluruh peta: warna yang sama selalu berarti status yang sama, di
kartu kamar, ringkasan lantai, kartu ringkas, dan pop up kamar.

| Status bed | Warna | Dipakai di |
| --- | --- | --- |
| Tersedia | Hijau (`--color-success`) | Titik kartu, ringkasan, titik dan tepi baris pop up |
| Dipesan | Ungu (`--color-purple`) | Idem |
| Terisi High Dependency | Oranye `#ea580c` | Idem, ditambah tag "High Dependency" pop up |
| Terisi Low Dependency | Biru `#2563eb` | Idem, ditambah tag "Low Dependency" pop up |
| Terisi belum dinilai / profil Housekeeping | Abu-biru netral `#64748b` | Idem |
| Menunggu Pembersihan | Kuning `#eab308` dengan garis kuning tua | Idem |
| Dalam Pembersihan | Sian `#0891b2` | Idem |
| Tidak tersedia | Abu berongga | Idem |

**Contoh (`RWI-AC-508`).** Kamar 329 berisi 329-A terisi pasien LD, 329-B dipesan, dan 329-C
tersedia. Di kartu maupun di pop up, 329-A biru, 329-B ungu, 329-C hijau. Hijau hanya ada pada 329-C
dan ungu hanya pada 329-B. Bed 325-A yang Menunggu Pembersihan berwarna kuning, bukan oranye. Bila
petugas Housekeeping membuka kamar yang sama, 329-A memakai warna "terisi" netral karena server tidak
mengirim tingkat ketergantungan untuk profil itu.

**Langkah.**

1. Petugas membuka kamar 329 di Monitoring Bed atau Pilih Bed.
2. Titik bed di kartu dan titik status di baris bed pop up memakai warna yang sama.
3. Isi pop up — nama bed, label status, nama penghuni (hanya untuk profil yang berhak), tombol aksi —
   tidak berubah.

**Jalur tidak normal.** Status bed yang tidak dikenal tampil abu (nada "tidak tersedia"), sama
seperti sebelumnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `floor-plan.module.css` (token `.theme`, aturan baris bed dan tag ketergantungan pop up, titik
  kartu dan ringkasan), `floor-plan-room-popup.jsx` (`data-status`, `dependencyTag`),
  `floor-plan-bed-status.js`, `src/app/globals.css` (`--color-*`), uji
  `floor-plan-dependency-color.test.mjs`.
- Decision log `RWI-DEC-353`, frontend architecture 21.7 dan 21.12, acceptance `DNH-AT-508`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/health-services/inpatient-management/floor-plan/floor-plan.module.css` | Token palet `--fp-bed-*` didefinisikan sekali di `.theme` (ditambah `--fp-bed-waiting-cleaning-line`). Titik status dan tepi kiri baris bed pop up serta tag ketergantungan kini menunjuk token itu; baris "tidak tersedia" mendapat titik berongga seperti kartu; titik kuning di kartu, ringkasan, dan pop up diberi garis kuning tua agar terlihat di latar putih |
| `tests/unit/floor-plan-status-palette.test.mjs` | **Baru.** 5 uji: menyelesaikan token sampai nilai warna akhir dan membuktikan aturan tiga warna kunci, kuning untuk Menunggu Pembersihan, contoh 329-A/B/C dan Housekeeping, serta titik pop up dan kartu menunjuk token yang sama |
| `tests/unit/floor-plan-dependency-color.test.mjs` | Dua pemeriksaan warna "tersedia" pada baris bed pop up diarahkan ke token `--fp-bed-available`; pemeriksaan renderer lama tetap |

`floor-plan-room.jsx` dan `floor-plan-bed-status.js` tidak perlu diubah: titik kartu sudah menunjuk
token yang sama sejak `FE-RWI-273`. Isi, teks, tata letak, dan aksi pop up tidak berubah
(`floor-plan-room-popup.jsx` tidak disentuh pada task ini).

### 3.3 Kepatuhan arsitektur frontend

- Palet didefinisikan sekali sebagai token di blok `.theme`; semua pemakai — titik kartu, ringkasan,
  kartu ringkas, titik dan tepi baris pop up, tag ketergantungan — memakai `var(--fp-bed-*)`.
- Tidak ada perubahan JSX, hook, service, maupun data.

**Keputusan elemen UI (base component decision gate).**

| Elemen | Status | Bukti |
| --- | --- | --- |
| Titik status baris bed pop up | `REUSE` struktur, warna dari token | `RWI-DEC-353`: hanya warna yang berubah |
| Tepi kiri baris bed dan tag ketergantungan pop up | `REUSE` struktur, warna dari token | Larangan task: warna kunci tidak dipakai untuk status lain |
| Titik kartu, ringkasan, kartu ringkas | `REUSE` (token sejak `FE-RWI-273`/`274`) | Nilai token berubah, kode tidak |

`UI GATE: PASS` — semua elemen `REUSE`; tidak ada keputusan yang menunggu pemilik.

**Grep anti-regresi (`ui-consistency-checklist` bagian G, pada baris yang ditambah).** Tidak ada
`<button>`, `<table>`, utilitas tipografi Bootstrap, maupun `!important` baru. Nilai warna literal
hanya muncul sebagai nilai token di blok `.theme`; selektor gaya memakai `var(...)`. Tidak ada
deklarasi tipografi baru.

**Tafsiran yang dicatat.** `RWI-DEC-353` menyebut "di pop up yang berubah hanya warna titik". Tepi
kiri baris bed dan tag "Low Dependency" ikut diselaraskan karena keduanya juga membawa warna status:
tanpa perubahan itu baris 329-A tetap bertepi hijau dan bertag hijau, sehingga melanggar `RWI-AC-508`
("hijau hanya pada 329-C") dan larangan task ("warna kunci tidak dipakai untuk status lain"), dan
baris Menunggu Pembersihan tetap bertepi oranye kecokelatan. Bentuk, teks, dan susunan tag tidak
berubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah (kerangka berbentuk kartu dari `FE-RWI-274`) |
| Kosong | Tidak berubah |
| Gagal | Tidak berubah: "Denah gagal dimuat. Muat ulang." beserta tombol Coba Lagi dari layar pemakai |
| Tanpa hak akses | Profil Housekeeping: titik terisi memakai warna "terisi" netral di kartu dan pop up, tanpa tag ketergantungan |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini hanya mengubah warna; tidak ada panggilan API yang ditambah atau diubah.
Data status bed tetap dari endpoint `FE-RWI-272` bagian 5.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test` atas 13 berkas uji denah dan 4 berkas uji yang membaca `floor-plan-selection.jsx` | 133 uji, 133 lulus, termasuk 5 uji baru `floor-plan-status-palette.test.mjs` | `PASS` | Keluaran `node:test` |
| `npx eslint` pada 2 berkas uji yang disentuh | Exit 0 | `PASS` | Keluaran perintah |
| `npm run lint` (`eslint .`, format JSON) | Exit 0: `0 errors`, 1083 warning pada 5436 berkas — sama persis dengan garis dasar (1083) | `PASS` | Ringkasan JSON ESLint |
| `npm run test:unit` | 3321 uji: 3284 lulus, 37 gagal. 35 kegagalan identik dengan sebelum `FE-RWI-272`. Dua kegagalan baru — "Prescription Selected Card: badge V1 dan tiga aksi footer" (`inpatient-prescription-compound-builder.test.mjs`) dan "canCancelProcedure: mengizinkan penginput asli atau DPJP aktif …" (`inpatient-procedure-utils.test.mjs`) — menguji `prescription-selected-items-card.jsx` dan `inpatient-procedure-utils.jsx`, dua berkas yang sedang diubah pekerjaan paralel di working tree dan tidak disentuh task ini | `EXISTING / ENVIRONMENT ISSUE` (`UNRELATED EXISTING ISSUE` untuk dua kegagalan baru); `PASS` untuk uji denah | Keluaran perintah dan `git status` |
| `npm run build` | `✓ Compiled successfully in 69s` (Next.js 16.2.12, Turbopack), 514 halaman statis, `prepare-standalone` berhasil, exit 0, tanpa warning build. Dijalankan setelah proses `node .next/standalone/server.js` yang sebelumnya mengunci `.next\standalone` berhenti sendiri; working tree memuat `FE-RWI-272` s.d. task ini | `PASS` | Keluaran perintah |
| Pratinjau statis (HTML hasil render + screenshot Edge, data samaran) | Kartu dan contoh baris pop up: 329-A biru, 329-B ungu, 329-C hijau, 325-A kuning, Housekeeping abu-biru netral, HD oranye, tidak tersedia berongga, dalam pembersihan sian; ringkasan lantai memakai titik yang sama | Bukti pendukung, bukan uji manual | Berkas pratinjau di luar repository, tidak dilampirkan |
| Uji di aplikasi: kartu dan pop up kamar 329 dan 325 di Monitoring dan Pilih Bed, akun Housekeeping | Belum dijalankan | `NOT RUN` | Dijalankan pengguna sendiri |

Uji manual: `REQUIRED` — belum dijalankan; dijalankan pengguna sendiri atas keputusan pengguna
20 Oktober 2026, sehingga task ditandai kuning.

`AUTOMATED TEST: node --test tests/unit/floor-plan-status-palette.test.mjs (bersama uji denah lain) — PASS (133/133)`

`AUTOMATED TEST: npm run test:unit — FAIL (37 dari 3321 gagal; tidak satu pun di berkas uji denah: 35 kegagalan lama, 2 dari pekerjaan paralel resep dan tindakan)`

**Tidak dijalankan:** uji di peramban dan akun Housekeeping (keputusan pengguna).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) 329-A terisi LD, 329-B dipesan, 329-C tersedia berwarna sama di kartu dan pop up; hijau hanya 329-C, ungu hanya 329-B | Terpenuhi di source; belum diuji di aplikasi | Titik kartu `.cardDot[data-tone]` dan titik pop up `.bedRow[data-status] .bedStatusDot` menunjuk `var(--fp-bed-<nada>)` yang sama; uji "(a)(c)" dan "titik pop up dan titik kartu menunjuk token palet yang sama" |
| (b) 325-A Menunggu Pembersihan tidak oranye | Terpenuhi di source; belum diuji di aplikasi | `--fp-bed-waiting-cleaning: #eab308` (hue ± 45°, kuning); uji "(b)" |
| (c) Akun Housekeeping: 329-A memakai warna "terisi" netral di kartu dan pop up | Terpenuhi di source; belum diuji di aplikasi | Tanpa `holdingContext` → nada `occupied` → `--fp-bed-occupied` (#64748b) di kedua tempat; uji "(a)(c)" |

| Butir DoD (`D-FE`) | Status |
| --- | --- |
| Setiap acceptance criteria terpetakan ke source yang ada | Terpenuhi |
| `V-FE` 1 — baca ulang source dan `git diff` sebelum edit | Terpenuhi |
| `V-FE` 2 — `lint` | Terpenuhi: `0 errors`, 1083 warning (garis dasar) |
| `V-FE` 3 — `test:unit` | Dijalankan: 3284/3321 lulus; 37 kegagalan bukan regresi task (35 lama, 2 dari pekerjaan paralel) |
| `V-FE` 4 — `build` | Terpenuhi: `✓ Compiled successfully in 69s`, exit 0 |
| `V-FE` 5 — bukti visual di aplikasi | **Belum terpenuhi**: dijalankan pengguna sendiri |
| Tanpa perubahan hook, service, kontrak API, route, menu, permission, package, backend, database | Terpenuhi |
| Laporan dan enam titik status roadmap serta traceability | Terpenuhi pada perubahan yang sama |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning lint baru |
| Masalah yang diketahui | Pop up dipakai keempat pemakai dan semua lantai, sehingga palet baru terlihat juga pada lantai yang masih memakai gambar lama — sesuai `RWI-DEC-353`. Renderer kamar lama (garis tepi teal/oranye/hijau di lantai gambar lama) tidak diubah karena di luar cakupan dan dilepas `FE-RWI-279` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas task: `M floor-plan.module.css`, `M tests/unit/floor-plan-dependency-color.test.mjs`, `?? tests/unit/floor-plan-status-palette.test.mjs`. Berkas lain yang berubah di working tree milik `FE-RWI-272` s.d. `FE-RWI-274` dan pekerjaan paralel |
| Langkah berikutnya | Uji manual Lantai 3 oleh pengguna, lalu tunjukkan contoh Lantai 3 kepada atasan untuk gerbang `DEC-PETA-L3`; `FE-RWI-276` s.d. `FE-RWI-278` baru boleh dimulai sesudah pengesahan dicatat sebagai keputusan baru |
