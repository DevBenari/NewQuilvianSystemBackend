# Laporan Perubahan Frontend — `FE-NV-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-014` |
| Judul | Strip Panggilan Aktif dan penanda kartu di TV |
| Slice | `MVP-8` — `EPIC NV-07` (gelombang frontend 2, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-014` |
| Trace | `FR-NV-046`, `047`, `048`; `NV-DEC-013`, `038`, `044`, `047`, `048`; `NV-FE-006`; `NV-FCT-001`, `002`; `03-frontend-architecture.md` R2.3 (`FE-NVW-04` tambahan), R2.5 |
| Contract version | `0.2.1` `approved` (`NV-DEC-057`); api §9 dan §10 tidak berubah sejak `0.2.0` (`NV-DEC-055`) |
| Wewenang UI | `DEV_DISCRETION`: kapasitas strip (`NV-FE-006`), warna, dan bentuk penanda kartu. Isi item, privasi, urutan, dan TV tanpa suara milik owner (`NV-DEC-013`, `038`, `044`, `047`) |
| Dependency | `BE-NV-015` ✅, `FE-NV-010` ✅ (10 Nov 2026) |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 8 (2), logika sedang — jadwal muat strip terpisah dari papan, pita data basi gabungan (1), kontrak baru yang sudah disetujui (1), keamanan berkaitan — privasi layar TV (1), UI satu layar (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | `src/{components/view,lib/hooks,lib/services,lib/constants,utils,style}/nurse-view-display/`; laporan ini, baris status roadmap dan traceability di repo backend. Tanpa berkas bersama |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7dd08c585` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `b83e8f05` + perubahan `BE-NV-015` belum di-commit (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | ✅ **SELESAI 10 November 2026** — AC-1–6 terbukti di layar TV 1920×1080 (`NVW-000003`). Strip T1–T11: tanpa panggilan strip tidak dirender; Darurat 326-C tampil 0,17 detik sesudah tekan saat TV di halaman 1; strip tetap di halaman 1–4, kartu 326-C bertanda Darurat di halaman 4; urutan Darurat dulu; "‹nama› menuju" 0,17 detik; tanpa inisial, umur, nama, No. RM; ambang kosong tanpa warna; 8 panggilan tiruan → 5 item + "+3 lainnya" dengan warna `Critical`/`Warning`; putus jaringan 150 detik → pita basi pada detik 119, pulih 0,5 detik sesudah online; strip hilang saat semua selesai; TV tanpa bunyi. Putaran akhir 10/11: T8 gagal karena race pembacaan skrip, dibuktikan oleh tangkapan layar putaran itu dan T8 `PASS` putaran sebelumnya. Regresi: `FE-NV-007` F0–F5 6/6; `FE-NV-005` L1–L8 43/44 cek, satu cek L2 gagal karena data (326-B kini Kosong, bukan Dipesan seperti keadaan uji 3 Nov). `npx eslint` 0 error; build `PASS` (Rizki) |

---

## 1. Keadaan yang ditemukan di awal

- Layar TV `/nurse-view-display` (`FE-NV-005`–`007`) hanya memuat `/display-runtime/current` dan
  `/board`, bergabung ke grup `nurse-view-floor:{lantai}`, dan memuat ulang papan saat sinyal
  `NurseViewBoardChanged` paling sering sekali per 5 detik.
- Klien realtime sudah mengenal event `NurseViewPatientCallChanged` lewat opsi
  `onNurseViewPatientCallChanged` (`FE-NV-010`), dan backend mengirimnya ke grup lantai TV yang
  sama (`BE-NV-014`). Belum ada layar TV yang mendengarkannya.
- Backend menyediakan `GET /display-runtime/active-calls` dan field kartu `activeCallPriority`
  (`BE-NV-015`), tetapi build yang sedang berjalan masih lama: `/active-calls` `404`.
- Utilitas waktu panggilan sudah ada di konsol (`patient-call-console-utils.jsx`) dan disalin di
  tablet (`nurse-view-bed-tablet-utils.jsx`).

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** perawat yang melihat TV nurse station dari jarak ±3 m. TV tidak bisa disentuh dan
tidak bersuara (`NV-DEC-013`).

**Langkah normal:**

1. TV login dengan akun perangkatnya dan membuka papan lantai seperti biasa.
2. Bila ada panggilan aktif, **strip Panggilan Aktif** tampil tepat di bawah kepala layar, di atas
   papan. Strip tidak ikut berganti halaman (`NV-DEC-047`).
3. Setiap item berisi dua baris: nomor bed dan lama menunggu (menit:detik, dihitung dari jam
   server), lalu jenis ("DARURAT" atau "Panggilan") dan status pengklaim ("Menunggu" atau
   "Sari menuju"). Tidak ada inisial, nama, umur, maupun No. RM (`NV-DEC-044`).
4. Urutan mengikuti server: Darurat dulu, lalu paling lama menunggu (`NV-DEC-038`).
5. Item berwarna menurut tingkat lama menunggu: kuning muda (`Warning`), merah penuh (`Critical`),
   tanpa warna bila ambang belum diisi (`NV-DEC-048`).
6. Strip memuat paling banyak 5 item. Sisanya ditandai "+k lainnya" (`NV-FE-006`).
7. Kartu bed yang sedang memanggil diberi label "Memanggil" (kuning) atau "DARURAT" (merah) di
   sebelah nomor bed, ditambah bingkai tebal sewarna.
8. Saat pasien menekan tombol, TV menerima sinyal dan memuat ulang strip **saat itu juga**. Papan
   ikut dijadwalkan lewat jalur lama (paling sering sekali per 5 detik) agar penanda kartu muncul.

**Contoh.** Pukul 10.01.00: `PANGGILAN | 331-C 0:20 · DARURAT Menunggu | 305-A 1:00 · Panggilan
Dewi menuju | 318-B 0:40 · Panggilan Menunggu`. Kartu 331-C berlabel "DARURAT", kartu 305-A dan
318-B berlabel "Memanggil".

**Jalur tidak normal:**

| Kejadian | Yang dilihat perawat |
| --- | --- |
| Tidak ada panggilan | Strip tidak tampil dan tidak memakan tempat; papan seperti revision 1 |
| Panggilan lebih dari 5 | 5 item pertama + "+k lainnya" |
| Jaringan TV terputus > 120 detik | Strip dan papan tetap menampilkan data terakhir; pita "Data terakhir hh.mm — koneksi terputus" |
| Tersambung kembali | Strip dan papan dimuat ulang (sambung ulang hub, event `online`, dan muat ulang 60 detik) |
| `/active-calls` belum tersedia di backend (`404`) atau gagal | Strip tidak tampil; papan tetap berjalan |
| Akun TV dinonaktifkan atau sesi kedaluwarsa (`401`/`403`) | Kembali ke layar login, sama dengan papan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `CLAUDE.md` frontend; suite `rules/frontend/*` (frontend-architecture, base-component-decision-gate, base-component-catalog, design-tokens, ui-consistency-checklist, REPORT_TEMPLATE, test-policy)
- Blueprint: kartu `FE-NV-014`; api §9, §10; `03-frontend-architecture.md` R2.3 `FE-NVW-04` tambahan (wireframe strip), R2.5; `00-interview-decisions.md` `NV-DEC-013`, `038`, `044`, `047`, `048`, `NV-FE-006`, `NV-AC-030`, `031`, `032`, `037`; laporan `FE-NV-005` (UI gate TV)
- Source frontend: `nurse-view-floor-board-view.jsx`, `nurse-view-bed-card.jsx`, `use-nurse-view-floor-board.jsx`, `nurse-view-display-service.jsx`, `nurse-view-display-constants.jsx`, `nurse-view-board-utils.jsx`, `nurse-view-floor-board.module.css`; `queue-realtime-client.js` (opsi `onNurseViewPatientCallChanged`, baris 148, 279); `patient-call-console-utils.jsx` dan `-constants.jsx`; `nurse-view-bed-tablet-utils.jsx`; inventaris `src/components/features/base-features/`
- Source backend (read-only): `NvwDisplayRuntimeController.GetActiveCalls`, `NvwDisplayCallStripResponse`/`ItemResponse`, `NvwBedCardResponse.ActiveCallPriority`; enum dikirim angka (tanpa `JsonStringEnumConverter`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/nurse-view-display/nurse-view-display-constants.jsx` | `NURSE_VIEW_CALL_STRIP_CAPACITY = 5`, `NURSE_VIEW_CALL_STRIP_TEXT` (beku), endpoint `activeCalls` |
| `src/lib/services/nurse-view-display/nurse-view-display-service.jsx` | `getNurseViewActiveCalls({ signal })` lewat `InstanceAxios` |
| `src/utils/nurse-view-display/nurse-view-board-utils.jsx` | `normalizeCallStrip` (selisih jam server), `buildCallStripEntries` (item tampil + jumlah tersembunyi), `getActiveCallMarker`; memakai ulang `computeWaitingSeconds`, `formatWaitDuration`, `formatClaimLabel`, `isEmergencyCall`, `resolveServerOffsetMs` dari utilitas konsol |
| `src/lib/hooks/nurse-view-display/use-nurse-view-floor-board.jsx` | `loadCallStrip` (abort, sesi ditolak → login); `handlePatientCallChanged` = strip segera + papan terjadwal; dipasang di `onNurseViewPatientCallChanged`, saat hub tersambung ulang, dan di muat ulang 60 detik/`online`; pita basi memakai data tertua antara papan dan strip (`staleSince`) |
| `src/components/view/nurse-view-display/nurse-view-call-strip.jsx` | **Baru** — strip `<section aria-label="Panggilan aktif" aria-live="polite">`; tidak dirender bila kosong |
| `src/components/view/nurse-view-display/nurse-view-floor-board-view.jsx` | Strip di antara kepala layar dan papan; pita basi memakai `staleSince` |
| `src/components/view/nurse-view-display/nurse-view-bed-card.jsx` | `CallMark` di sebelah nomor bed (kartu terisi dan kosong), kelas `cardCalling`/`cardCallingEmergency`, atribut `data-active-call-priority` |
| `src/style/nurse-view-display/nurse-view-floor-board.module.css` | +175 baris kelas strip, item, nada lama menunggu, dan penanda kartu; token saja |

Diff +355/−11 pada tujuh berkas lama + satu berkas baru (55 baris). Semua CRLF, nol komentar baru.

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: view → hook → service → `InstanceAxios`. View tidak memanggil Axios dan tidak
  menormalisasi payload; normalisasi di `utils`, teks dan endpoint di `constants` (beku).
- Strip dimuat di hook papan yang sama, bukan hook baru, karena memakai koneksi realtime, pengaman
  sesi, dan jam yang sama. Membuat hook kedua berarti koneksi hub kedua atau salinan
  `isCanceledRequest`/`isSessionDenied`.
- Utilitas waktu dipakai ulang dari konsol; tidak ada salinan ketiga (aturan umum no. 7).
- Komponen strip tanpa `"use client"`: tidak memakai hook, dan hanya dirender dari view klien.

**UI gate** (modul referensi visual: papan TV `FE-NV-005`; `ls src/components/features/base-features/`
dan pencarian `marquee|ticker|call-strip` tanpa hasil):

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Wadah strip beserta judul "PANGGILAN" | `InformationAlert`, `Hero` | Alert bisa ditutup dan berukuran halaman admin; `Hero` untuk judul halaman | `COMPOSE` | `<section>` + CSS module bertoken, pola wadah TV `FE-NV-005` |
| Item panggilan dua baris | `StatusBadge`, `BasePatientCard` | Badge `react-bootstrap` ukuran tabel; kartu pasien beridentitas lengkap | `COMPOSE` | `<li>` dengan huruf skala `--board-unit` |
| Tanda "DARURAT" di item | `StatusBadge` | Idem | `COMPOSE` | Pill `--color-danger` |
| Warna lama menunggu | `StatusBadge` (konsol memakainya lewat `PATIENT_CALL_CONSOLE_WAIT_LEVEL_BADGE`) | Ukuran tabel, tidak terbaca dari 3 m | `COMPOSE` | Kelas `callWaitWarning`/`callWaitCritical` |
| "+k lainnya" | `Pagination` | Navigasi yang diklik pengguna | `COMPOSE` | Teks chip |
| Penanda kartu bed | Tanda J/A/I milik kartu TV | Pola feature-local `FE-NV-005` | `COMPOSE` | Pill di sebelah nomor bed + bingkai dalam 3 px |

`UI GATE: 6 elemen — REUSE 0, EXTEND 0, COMPOSE 6, WRAP 0, NEW 0`

Pilihan `COMPOSE` untuk elemen layar TV sudah diputuskan pada `FE-NV-005` di modul yang sama
(dua `REUSE`, sisanya `COMPOSE` dengan token), sehingga keputusan itu dirujuk dan tidak diajukan
ulang (`base-component-decision-gate.md` Langkah 6).

**Keputusan `DEV_DISCRETION` beserta alternatifnya.**

| No | Keputusan | Alternatif yang ditimbang | Alasan |
| ---: | --- | --- | --- |
| 1 | Kapasitas 5 item, satu baris | 4 item lebih lebar; 6 item lebih sempit; dua baris strip | Sama dengan 5 kolom kartu; pada 1920 px tiap item ±330 px, cukup untuk "325-A  12:34 / Panggilan Sari menuju" berhuruf papan. Dua baris strip memakan tinggi papan |
| 2 | Item dua baris (bed + waktu / jenis + pengklaim) | Satu baris seperti wireframe | Empat unsur `NV-DEC-044` dalam satu baris ±330 px terlalu kecil untuk 3 m |
| 3 | Unclaimed menampilkan "Menunggu" | Kosong | Perawat tahu belum ada yang menuju bed; teks sama dengan konsol (`formatClaimLabel`) |
| 4 | Judul strip berwarna sekunder, merah hanya untuk Darurat dan `Critical` | Judul merah | Mengurangi kelelahan alarm (pembanding ECRI di `NV-DEC-013`) |
| 5 | Penanda kartu berupa label teks + bingkai | Huruf bulat seperti J/A/I | Huruf baru mudah tertukar dengan J/A/I |
| 6 | Pita basi memakai data tertua antara papan dan strip | Hanya papan | Strip yang berhenti diperbarui tetap ketahuan |

### 3.4 Perubahan atas permintaan owner — tombol Keluar di TV (10 November 2026)

Rizki meminta tombol Keluar di pojok kanan atas layar TV. Di luar acceptance criteria kartu;
persetujuan lisan owner. TV memakai **sekali klik**, mengikuti display antrean, karena TV
dioperasikan petugas IT dengan tetikus, bukan oleh pasien.

**Alasan bentuk penjaga.** Sesudah keluar, perangkat hanya bisa login lagi dengan kata sandinya, yang
hanya ditampilkan sekali saat Buat Login / Atur Ulang Login. Preseden: display antrean (dilihat petugas)
memakai tombol ikon "Keluar" sekali klik; Kiosk (disentuh pasien) memakai penjaga klik ganda.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/nurse-view-display/nurse-view-display-constants.jsx` | `NURSE_VIEW_DISPLAY_TEXT` (`Keluar`, label aksesibilitas) |
| `src/lib/hooks/nurse-view-display/use-nurse-view-floor-board.jsx` | Mengembalikan `logout` (= `leaveToLogin` yang sudah ada: hapus cookie perangkat, `LogoutUser`, ke `/login`) |
| `src/components/view/nurse-view-display/nurse-view-floor-board-view.jsx` | Jam dan tombol dibungkus `headerActions`; `BaseButton` `secondary` `sm` + ikon `FiLogOut` |
| `src/style/nurse-view-display/nurse-view-floor-board.module.css` | `.headerActions` (flex, token jarak) |

`UI GATE: 2 elemen — REUSE 1 (BaseButton + FiLogOut, preseden display antrean), COMPOSE 1 (wadah kanan header), NEW 0`.

Verifikasi: `npx eslint` 0 error, 0 warning; grep anti-regresi bersih; uji layar `uji-tombol-keluar.mjs` K1 `PASS` — tombol di kanan atas (x 1784, y 38 pada 1920×1080), sekali klik → `/login`, cookie sesi terhapus. Bukti `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-keluar-20261110/layar-2026-11-10T03-01-25-144Z/` (`TV-header-keluar.png`). Build produksi belum memuat perubahan ini.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `LoadingScreen` "Memuat papan lantai..." seperti sebelumnya; strip muncul sesudah muatan pertama |
| Kosong | Tanpa panggilan → strip tidak dirender |
| Gagal | Strip mempertahankan data terakhir; pita "Data terakhir hh.mm — koneksi terputus" bila papan atau strip lebih tua dari 120 detik. Gagal sejak awal (termasuk `404` backend lama) → strip tidak tampil, papan berjalan |
| Tanpa hak akses | `401`/`403` → keluar ke layar login, sama dengan papan |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Nurse View Management / Display Runtime

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/nurse-view-management/display-runtime/active-calls` | Strip Panggilan Aktif: `totalActive`, `thresholdsConfigured`, `serverTimeUtc`, `items[]` (`bedCode`, `bedName`, `priority`, `callStatus`, `firstPressedAt`, `claimedByName`, `waitLevel`) | Policy `NurseViewDisplayRead` (akun TV) |
| `GET` | `/v1/health-services/nurse-view-management/display-runtime/board` | Papan; kini juga `beds[].activeCallPriority` (`1`/`2`/`null`) | Policy `NurseViewDisplayRead` |

Realtime: event `NurseViewPatientCallChanged` di grup `nurse-view-floor:{lantai}` yang sudah
diikuti TV lewat `JoinNurseViewFloor`; tidak ada join baru.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 7 berkas JS/JSX yang disentuh | Exit 0, 0 error, 0 warning | `PASS` | Keluaran perintah 2026-11-09 |
| Grep anti-regresi `ui-consistency-checklist` §G | Warna literal 0, tombol mentah 0, `<table>` 0, utility typography 0, `!important` 0, inline style 0, blok dark 0; 28 selector baru seluruhnya milik layar TV | `PASS` | Keluaran grep |
| Pemeriksaan fungsi murni strip dan penanda (skrip sekali pakai di scratchpad, bukan test repository) | 15/15: selisih jam server, urutan dari server, lama menunggu 0:20/1:00/0:40, jenis, "Dewi menuju", 8 panggilan → 5 tampil + "+3", `totalActive` > item, kosong, penanda `null`/`1`/`2` | `PASS` | Keluaran `cek-utils.mjs` |
| Akhiran baris | 8 berkas CRLF, LF 0 | `PASS` | Hitungan byte Node |
| `npm run build` (oleh Rizki) | Berhasil menurut pernyataan Rizki 9 Nov 2026; agent memeriksa artefaknya: `.next/BUILD_ID` 16.05 (source strip 15.03), `.next/standalone/server.js` 16.06 dan `static` tersalin (`postbuild`), tiga chunk memuat `display-runtime/active-calls`. Sebelumnya build agent dihentikan atas permintaan Rizki ("build semua nyaa saya yang jalankan") | `PASS` | Pernyataan Rizki; pemeriksaan `.next` |
| Uji layar TV berjam (`uji-fe-nv-014-20261110/uji-strip-tv.mjs`), regresi `FE-NV-007` F0–F5 dan `FE-NV-005` L1–L8 | Strip 10/11 (T8 race skrip; terbukti di layar dan putaran sebelumnya); F0–F5 6/6; L1–L8 43/44 cek (1 cek data) | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T02-06-54-206Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T02-01-53-601Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-007-20261103/layar-2026-11-10T01-54-59-644Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-005-20261103/layar-2026-11-10T02-31-41-519Z/` |
| Uji ulang sesudah merge backend dan perbaikan otorisasi `/active-calls` (backend 14.41 dari DLL 14.37), 10 Nov sore | 14.49 WIB strip 10/11. 16.03 WIB, sesudah revisi join ulang hook papan (`FE-NV-007` §3.4), strip 10/11: T8 gagal karena race skrip yang sama, sedangkan tangkapan layar T8 kedua putaran benar (5 item tiruan + "+3 lainnya", 2 `Critical`, 2 `Warning`, tidak meluap). T10: pita basi dihitung dari data tertua (`min(lastSuccessAt, stripSuccessAt)`, ambang 120 detik), jadi muncul 76 detik sesudah putus bila papan terakhir dimuat ±44 detik sebelumnya, dan 119 detik pada putaran 16.03. L1–L8 43/44 (cek data L2 yang sama). Satu putaran 16.01 WIB berhenti di persiapan (timeout DB dev, login tablet `500`); policy Sari dan tablet yatimnya dipulihkan lewat API (`pulihkan-20261110.mjs`: policy 20 identik, tablet nonaktif) | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T07-49-19-210Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T09-03-11-793Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-005-20261103/layar-2026-11-10T08-09-36-422Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/pulihkan-2026-11-10T09-02-46-844Z.json` |
| Uji ulang 11 Nov 2026 (backend 08.21 dari DLL 10 Nov 14.37, `next dev`; kode sama dengan build 10 Nov 16.26) | 08.40 WIB strip 10/11 (T8 race skrip, layar benar). Skrip v2 membaca item, label "+k", dan teks strip dalam satu `evaluate` (skrip lama disimpan `uji-strip-tv-v1.mjs`). Hasil 08.50 WIB **11/11**: T8 `310-X/Critical`, `311-X/Warning`, `312-X/Critical`, `313-X/Warning`, `314-X/None` + "+3 lainnya", tidak meluap; T10 pita basi 118 detik. Regresi L1–L8 43/44 (cek data L2 yang sama); tombol Keluar TV dan tablet 3/3 | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-11T01-50-34-690Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-005-20261103/layar-2026-11-11T01-35-34-391Z/`, `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-keluar-20261110/layar-2026-11-11T01-45-58-596Z/` |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; ketentuan owner tanpa unit test.`

`MANUAL TEST: PASS — 10 Nov 2026. Diuji di `next dev` port 3000 dengan backend build 9 Nov 15.59 (`dotnet run --no-build`), peramban Edge headless.`

**Rencana uji layar sesudah build backend** (TV `NVW-000003` lantai 3 atau peramban 1920×1080
layar penuh, Playwright headed berjam; tablet bed lantai 3 lewat API):

1. Regresi tanpa panggilan: strip tidak tampil; `FE-NV-005` L1–L8 dan `FE-NV-007` F0–F5 sama
   seperti laporan aslinya (AC-6).
2. Lantai 3 berisi 66 kartu = 5 halaman. Saat TV di halaman 1, tablet 326-C menekan "Darurat" →
   strip menampilkan 326-C sebelum 5 detik dari jam tekan; strip tetap saat halaman berganti; kartu
   326-C berlabel "DARURAT" ketika halamannya tampil (AC-1, AC-2).
3. 325-A dan 325-B memanggil biasa → urutan 326-C, 325-A, 325-B; Sari mengklaim 325-A → "Sari
   menuju" (AC-1, `NV-AC-030`, `032`).
4. "+k lainnya": lantai 3 hanya punya 4 bed berpenghuni, jadi 6+ panggilan tidak bisa dibuat
   lewat tablet. Tampilan "+k" dibuktikan dengan respons `/active-calls` tiruan 8 item lewat
   intersepsi Playwright, dan logikanya sudah lewat pemeriksaan fungsi murni (AC-3).
5. Jaringan TV diputus 3 menit (Playwright offline) → pita basi; tersambung → strip kembali (AC-3).
6. Pencarian nama lengkap dan inisial pasien uji di DOM strip → nol (AC-4).
7. Ambang kosong → tanpa kelas warna (`data-wait-level="None"`) (AC-5).
8. Pembersihan: semua panggilan uji ditutup; dipastikan nol panggilan aktif.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 `NV-AC-031`/`UAT-21`: TV di halaman 1 dari 3; 331-C (halaman 3) memanggil → strip langsung menampilkan 331-C dan tidak ikut berganti halaman; kartu 331-C bertanda saat halaman 3 tampil | ✅ Terbukti 10 Nov 2026 | T2, T3: 326-C (halaman 4 dari 5) tampil di strip pada halaman 1; strip ada di halaman 1–4; kartu bertanda `2` |
| AC-2 `NV-AC-028` sisi TV: panggilan 10.00.00 tampil sebelum 10.00.05 di jaringan dev normal (`NFR-008`) | ✅ Terbukti 10 Nov 2026 | T2: 0,17 detik; T5: 0,17 detik |
| AC-3 `UAT-22`: melebihi kapasitas → "+k lainnya"; putus 3 menit → pita data basi; tersambung → strip kembali | ✅ Terbukti 10 Nov 2026 | T8 (+3 lainnya), T10 (putus 150 detik: pita detik 119, pulih 0,5 detik) |
| AC-4 `NV-AC-037` sisi layar: tanpa inisial maupun nama di strip | ✅ Terbukti 10 Nov 2026 | T6: 4 inisial dan 3 umur diperiksa, nol muncul |
| AC-5 `NV-AC-040`: ambang kosong → tanpa warna | ✅ Terbukti 10 Nov 2026 | T7 |
| AC-6 regresi papan `FE-NV-005` L1–L8 dan `FE-NV-007` tetap lulus | ✅ Terbukti 10 Nov 2026 | F0–F5 6/6; L1–L8 43/44 cek, satu cek L2 gagal karena data berubah sejak 3 Nov |
| DoD — AC-1–6 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — lint `0 errors` | Terpenuhi | `npx eslint` 0 error, 0 warning |
| DoD — rekaman berjam TV | Terpenuhi 10 Nov 2026 | Video `.webm` dan tangkapan layar berjam di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-fe-nv-014-20261110/layar-2026-11-10T02-06-54-206Z/` |
| DoD — laporan | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server Next di port 3000 akan memakai kode ini bila berjalan dalam mode dev; dengan backend lama, `/active-calls` `404` sehingga strip tidak tampil dan papan tetap seperti sebelumnya |
| Masalah yang diketahui | `WaitLevel` dihitung server saat respons; warna baru berpindah tingkat pada muat ulang berikutnya (≤ 60 detik atau saat ada sinyal), sama dengan konsol. Temuan di luar scope: `resolveServerOffsetMs`, `computeWaitingSeconds`, `formatWaitDuration`, `isEmergencyCall` tersalin di utilitas konsol dan tablet |
| Dependency backend | `BE-NV-014`, `BE-NV-015` ✅ 10 Nov 2026 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sudah di-commit Rizki (`98d029d54`) |
| Langkah berikutnya | Rekaman bersuara `FE-NV-013` oleh Rizki; UAT `UAT-21`, `UAT-22` |
