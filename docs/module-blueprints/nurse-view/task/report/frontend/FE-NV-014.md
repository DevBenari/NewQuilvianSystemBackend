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
| Dependency | `BE-NV-015` 🟡 (source + QBE `PASS`, build belum), `FE-NV-010` 🟡. Dikerjakan sebelum keduanya ✅ atas urutan Rizki 9 Nov 2026 ("BE-NV-015 … lalu FE-NV-014"; uji dijalankan sesudah `FE-NV-014` terbentuk), sama dengan `FE-NV-011`–`013` |
| Klasifikasi | `MEDIUM` — skor 7: satu repository (0), berkas diperiksa > 20 (2), berkas diubah 8 (2), logika sedang — jadwal muat strip terpisah dari papan, pita data basi gabungan (1), kontrak baru yang sudah disetujui (1), keamanan berkaitan — privasi layar TV (1), UI satu layar (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 |
| Target tulis | `src/{components/view,lib/hooks,lib/services,lib/constants,utils,style}/nurse-view-display/`; laporan ini, baris status roadmap dan traceability di repo backend. Tanpa berkas bersama |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7dd08c585` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `b83e8f05` + perubahan `BE-NV-015` belum di-commit (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | 🟡 **SEBAGIAN 9 November 2026** — AC-1–6 terpetakan ke source; `npx eslint` 7 berkas 0 error, 0 warning; UI gate 6 elemen seluruhnya `COMPOSE`; grep anti-regresi bersih; pemeriksaan fungsi murni 15/15. Belum: `npm run build` (port 3000 dipakai server Next milik Rizki), uji layar TV berjam termasuk putus jaringan, dan regresi papan `FE-NV-005` L1–L8 / `FE-NV-007` — menunggu build backend `BE-NV-014`/`015` |

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
| `npm run build` | Tidak dijalankan: port 3000 dipakai server Next milik Rizki (`node … next/dist/server/lib/start-server.js`, PID 19528); build akan menimpa `.next` miliknya | `NOT RUN` | `netstat`, `Win32_Process` |
| Uji layar TV berjam, putus jaringan, regresi `FE-NV-005` L1–L8 dan `FE-NV-007` | Menunggu build backend `BE-NV-014`/`015` | `NOT RUN` | — |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; ketentuan owner tanpa unit test.`

`MANUAL TEST: NOT FEASIBLE — backend BE-NV-014/015 belum dibangun ulang; /active-calls masih 404 di server yang berjalan.`

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
| AC-1 `NV-AC-031`/`UAT-21`: TV di halaman 1 dari 3; 331-C (halaman 3) memanggil → strip langsung menampilkan 331-C dan tidak ikut berganti halaman; kartu 331-C bertanda saat halaman 3 tampil | 🟡 Terpetakan ke source; runtime belum | Strip dirender di luar `segments` halaman (`nurse-view-floor-board-view.jsx`); `handlePatientCallChanged` memuat strip segera dan menjadwalkan papan; `CallMark` dari `activeCallPriority` |
| AC-2 `NV-AC-028` sisi TV: panggilan 10.00.00 tampil sebelum 10.00.05 | 🟡 Terpetakan ke source; runtime belum | Event → `loadCallStrip()` tanpa jeda 5 detik; waktu = kirim hub + satu `GET` |
| AC-3 `UAT-22`: melebihi kapasitas → "+k lainnya"; putus 3 menit → pita basi; tersambung → strip kembali | 🟡 Logika "+k" terbukti fungsi murni; runtime belum | `buildCallStripEntries` (5 + "+3" untuk 8); `staleSince` gabungan; muat ulang saat `connected` sesudah `reconnecting`, event `online`, dan tiap 60 detik |
| AC-4 `NV-AC-037` sisi layar: tanpa inisial maupun nama di strip | 🟡 Terpetakan ke source; runtime belum | Entri strip hanya `bedLabel`, `typeLabel`, `claimLabel`, `waitText`, `waitLevel`; backend tidak mengirim field pasien |
| AC-5 `NV-AC-040`: ambang kosong → tanpa warna | 🟡 Terpetakan ke source; runtime belum | `waitLevel = "None"` tidak dipetakan ke kelas nada (`WAIT_TONE` hanya `Warning`, `Critical`) |
| AC-6 regresi papan `FE-NV-005` L1–L8 dan `FE-NV-007` tetap lulus | 🟡 Diff papan hanya menambah; runtime belum | Perubahan jalur papan: `scheduleRealtimeLoad` tetap; kartu kosong membungkus nomor bed dalam `cardTitle`; jam pita basi = tertua papan/strip |
| DoD — AC-1–6 terpetakan ke source | Terpenuhi | Tabel ini |
| DoD — lint `0 errors` | Terpenuhi | `npx eslint` 0 error, 0 warning |
| DoD — rekaman berjam TV | **Belum** | Menunggu build backend |
| DoD — laporan | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Server Next di port 3000 akan memakai kode ini bila berjalan dalam mode dev; dengan backend lama, `/active-calls` `404` sehingga strip tidak tampil dan papan tetap seperti sebelumnya |
| Masalah yang diketahui | `WaitLevel` dihitung server saat respons; warna baru berpindah tingkat pada muat ulang berikutnya (≤ 60 detik atau saat ada sinyal), sama dengan konsol. Temuan di luar scope: `resolveServerOffsetMs`, `computeWaitingSeconds`, `formatWaitDuration`, `isEmergencyCall` tersalin di utilitas konsol dan tablet |
| Dependency backend | `BE-NV-015` (endpoint strip, field kartu) dan `BE-NV-014` (sinyal) 🟡 — keduanya belum dibangun ulang, sehingga strip belum bisa tampil di runtime |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` tujuh berkas §3.2, `??` `nurse-view-call-strip.jsx`. Berkas `M`/`??` lain (`row-action-menu.jsx`, `BaseModal.jsx`, `base-modal.module.css`, tabel dan modal disposisi IGD) milik pekerjaan lain dan tidak disentuh |
| Langkah berikutnya | Rizki membangun ulang backend (`BE-NV-014`, `015`) dan menjalankannya; lalu uji tertunda dijalankan berurutan: uji hub `BE-NV-014`, uji HTTP `BE-NV-015`, uji layar §6 |
