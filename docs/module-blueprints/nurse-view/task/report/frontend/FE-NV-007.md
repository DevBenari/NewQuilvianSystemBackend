# Laporan Perubahan Frontend — `FE-NV-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-007` |
| Judul | Papan bereaksi pada sinyal |
| Slice | `MVP-4` — push TTV |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-007` |
| Trace | `FR-NV-032`; `NV-DEC-023`, `026`; `03-frontend-architecture.md` §5 (Push, Sambung ulang); api §4 |
| Contract version | `0.1.2` — `approved`; api §4 tidak berubah sejak `0.1.0` |
| Wewenang UI | Tanpa perubahan tampilan. Perilaku muat ulang dikunci §5 (segera, paling sering 1×/5 detik, muat ulang 60 detik tetap jalan) |
| Dependency | `BE-NV-007` ✅, `FE-NV-005` 🟡 (bagian papan yang dipakai task ini sudah terbukti) |
| Klasifikasi | `MEDIUM` — satu hook dan satu konstanta milik Nurse View, plus penambahan pada klien realtime bersama |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend | Dikerjakan di atas `739e05f68`; belum di-commit |
| Commit backend yang dijadikan rujukan | `55335caf` (kode `BE-NV-007`), build Rizki 3 Nov 16.33 |
| Tanggal | 2026-11-04 |
| Status | ✅ **SELESAI 4 November 2026** — `npx eslint` 0 error 0 warning; Prettier lulus untuk berkas Nurse View. Uji layar 6/6 `PASS` (31 cek): TTV Kritis → muat ulang 0,01 detik sebelum jawaban simpan tiba, kartu 325-B merah "Kritis" di kelompok kritis teratas, tanpa bunyi; batas 1×/5 detik; sambung ulang 0,08 detik dengan join ulang; muat ulang 60 detik tetap berjalan; TV hanya memanggil `JoinNurseViewFloor`. Rekaman video berjam tersimpan. Regresi `FE-NV-005` 7/8 (43/44 cek); L2-E gagal karena pemesanan bed uji kedaluwarsa 3 Nov 04.09 UTC, bukan karena diff ini (§6). Verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Papan memuat ulang `/board` setiap 60 detik dan saat peramban kembali online. Halaman TV belum tersambung ke
`QueueHub`, sehingga TTV yang baru disimpan baru tampil pada muat ulang berikutnya, paling lambat 60 detik.

---

## 2. Proses bisnis dari sisi pengguna

1. Saat papan dibuka, TV tersambung ke `/hubs/queues` dan memanggil `JoinNurseViewFloor()` tanpa parameter. Lantai
   ditentukan backend dari perangkat.
2. Perawat Sari menyimpan TTV pasien bed 325-B dengan EWS 14 (`Critical`). Backend mengirim `NurseViewBoardChanged`
   ke TV lantai 3.
3. TV langsung memuat ulang `/board`. Kartu 325-B menjadi merah berkedip "EWS 14 · Kritis" dan pindah ke kelompok
   kritis teratas bangsal, tanpa bunyi.
4. Bila beberapa TTV disimpan beruntun, TV memuat ulang paling sering sekali per 5 detik. Sinyal yang datang di antara
   itu digabung menjadi satu muat ulang susulan, jadi tidak ada perubahan yang terlewat.
5. Bila koneksi hub terputus, TV tersambung ulang otomatis (jeda 0, 2, 5, 10, 30 detik), bergabung ulang ke grup
   lantainya, lalu memuat ulang papan.
6. Muat ulang 60 detik tetap berjalan, sehingga TV tetap benar walaupun sinyal tidak sampai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`use-nurse-view-floor-board.jsx`, `nurse-view-display-constants.jsx`, `nurse-view-floor-board-view.jsx`,
`nurse-view-bed-card.jsx`, `queue-realtime-client.js`, `signalrHubClient.jsx`, `use-queue-display-screen.jsx` (pola
display antrean), `next.config.js` (`reactStrictMode`); `03-frontend-architecture.md` §5, api §4, kartu roadmap.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/realtime/queue-realtime-client.js` | **Penambahan saja**: konstanta `NURSE_VIEW_BOARD_EVENT_NAME`, opsi `onNurseViewBoardChanged`, handler event, `joinNurseViewFloor()`/`leaveNurseViewFloor()` dengan pola `joinPriorityReservationBoard`, join ulang saat `onreconnected` (paling akhir, agar grup lain tidak terdampak), reset bendera saat `stop()` |
| `src/lib/hooks/nurse-view-display/use-nurse-view-floor-board.jsx` | Satu koneksi realtime per papan: join saat dipasang, `leaveNurseViewFloor` lalu `stop` saat dilepas. Event → `scheduleRealtimeLoad` (segera, lalu paling sering 1×/5 detik dengan satu susulan). Status `reconnecting` → `connected` → muat ulang. Bila koneksi berstatus `disconnected`, setiap muat ulang 60 detik mencoba join lagi. Satu `setInterval` jam dipecah ulang sesuai Prettier, tanpa perubahan nilai |
| `src/lib/constants/nurse-view-display/nurse-view-display-constants.jsx` | `NURSE_VIEW_REALTIME_MIN_GAP_SECONDS = 5` |

### 3.3 Kepatuhan arsitektur frontend

Tanpa Redux, service, atau komponen baru. Klien realtime bersama di-extend, tidak dibuat klien paralel.
`signalrHubClient.jsx` tidak dipakai, karena tidak punya konsumen dan tidak menyediakan join ulang grup saat sambung
ulang. `globals.css` tidak diubah.

| Kebutuhan | Kandidat | Status | Rekomendasi |
| --- | --- | --- | --- |
| Koneksi hub dan join ulang | `queue-realtime-client.js` | `EXTEND` (non-visual, hanya penambahan) | Dipakai. Perilaku bawaan untuk konsumen lain tidak berubah: opsi baru opsional, metode baru tidak dipanggil modul lain |
| Tampilan papan | `nurse-view-floor-board-view.jsx` | `REUSE` | Tidak disentuh |

`UI GATE: PASS — tanpa elemen UI baru.`

---

## 4. State yang ditangani

| State | Perilaku | Bukti |
| --- | --- | --- |
| Sinyal diterima | Muat ulang `/board` segera | F1, F2 |
| Banyak sinyal beruntun | Satu muat ulang segera dan satu susulan ≥ 5 detik kemudian | F3 |
| Koneksi hub terputus | Sambung ulang otomatis, join ulang, muat ulang | F4 |
| Koneksi awal gagal (`disconnected`) | Muat ulang 60 detik berikutnya mencoba join lagi | Review kode, belum diuji |
| Join ditolak backend (sesi basi, perangkat nonaktif) | Galat join diabaikan; muat ulang `/board` menerima `403` lalu TV keluar ke login (`FE-NV-005`) | Review kode; `BE-NV-007` H9 |
| Hub tidak tersedia sama sekali | Papan tetap berjalan dengan muat ulang 60 detik | F5, `FE-NV-005` L9 |

---

## 5. Endpoint yang dikonsumsi

`QueueHub` (`/hubs/queues`): `JoinNurseViewFloor()`, `LeaveNurseViewFloor()`, event `NurseViewBoardChanged`.
Payload event tidak dipakai untuk isi kartu; TV selalu memuat ulang `GET .../display-runtime/board`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 3 berkas | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `npx prettier --check --end-of-line crlf` hook dan konstanta | Lulus | `PASS` | Keluaran perintah |
| Prettier `queue-realtime-client.js` | Satu temuan `catch {}` di baris 125–126 | `EXISTING` — identik pada `HEAD`, di luar baris yang diubah | Perbandingan Prettier atas `HEAD` |
| Grep anti-regresi | Komentar baru 0; CRLF 3 berkas; tanpa CSS atau JSX baru | `PASS` | Keluaran perintah |
| F0 tersambung dan join | `/board` 200; tepat satu WebSocket hub terbuka; satu koneksi awal ditutup normal (1000) oleh Strict Mode dev; `JoinNurseViewFloor` tanpa argumen, sekali. 5 cek | `PASS` | `uji-nv-007-20261103/layar-2026-11-04T01-53-36-836Z/F0.json` |
| F1 TTV Kritis (`NV-AC-019`, `NV-AC-003`) | POST 200; `/board` dimulai 0,01 detik sebelum jawaban POST tiba, sementara muat ulang terjadwal masih 56 detik lagi; data 325-B `Critical` dalam 0,19 detik; semua kartu di depannya kritis; kartu layar "EWS 14 · Kritis"; `play()` media 0 kali. 7 cek | `PASS` | `F1.json`, `F1-sebelum.png`, `F1-sesudah-kritis.png` |
| F2 pembatalan | Cancel 200; muat ulang 0,02 detik sebelum jawaban tiba; 325-B kembali `High` dalam 0,20 detik. 3 cek | `PASS` | `F2.json` |
| F3 batas 1×/5 detik | POST, PUT, PUT dalam 0,5 detik; tepat 2 muat ulang dalam 9 detik (0,14 dan 5,16 detik), jarak 5.018 ms. 5 cek | `PASS` | `F3.json` |
| F4 sambung ulang | Soket hub ditutup paksa (kode 4000); soket baru 0,08 detik; `JoinNurseViewFloor` dikirim ulang 0,10 detik; papan dimuat ulang; sinyal berikutnya memicu muat ulang 0,01 detik. 6 cek | `PASS` | `F4.json` |
| F5 cadangan dan grup | Panggilan `/board` di keempat kelipatan 60 detik; tetap satu soket hub terbuka; 11 jawaban `/board` 200; metode hub hanya `JoinNurseViewFloor`; tanpa galat halaman. 5 cek | `PASS` | `F5.json` |
| Rekaman berjam (DoD) | Video TV 1280×720 sepanjang uji, jam papan terlihat di kepala layar | Ada | `rekaman-tv.webm` |
| Diagnosis frame hub sambung ulang | Join ulang dijawab sukses tanpa galat; event `NurseViewBoardChanged` tiba di koneksi baru 0,11 detik sesudah simpan, `/board` 0,01 detik kemudian | Bukti pendukung | `diag-sambung-*.json` |
| Regresi layar `FE-NV-005` L1–L8 | 7/8 `PASS`, 43/44 cek. **L2-E gagal**: 326-B tampil "Tidak tersedia", bukan "Dipesan" | `EXISTING / ENVIRONMENT ISSUE` — pemesanan uji pasien E masih `Active` tetapi `ExpiresAt` 3 Nov 04.09 UTC sudah lewat, sehingga backend memberi `Unavailable` (`InpBedManagementQueryService` baris 82, 126–133). Regresi 3 Nov 04.08 masih "Dipesan", semenit sebelum kedaluwarsa. Kartu bed tidak disentuh diff ini | `uji-nv-005-20261103/layar-2026-11-04T01-58-22-854Z`; SELECT baca saja |
| SELECT baca saja TTV uji | 3 TTV uji dan TTV diagnosis berstatus `Cancelled` | `PASS` | Keluaran perintah |
| Pencarian kredensial pada bukti JSON | 50 berkas, 0 sandi, 0 token | `PASS` | Pencarian pola dan nilai env |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-007-20261103/`, kecuali yang disebut lain.

**Riwayat putaran.** Dua putaran awal gagal karena asumsi skrip, dan produk tidak diubah:
(1) skrip mengira hanya ada satu WebSocket, padahal Strict Mode `next dev` memasang efek dua kali dan penghitungnya ikut
menghitung soket HMR Next.js; (2) skenario berjalan berdekatan sehingga batas 5 detik menunda muat ulang, yang memang
diminta kartu; (3) F4 mencari muat ulang sesudah jawaban POST, padahal sinyal sudah tiba lebih dulu. Skrip diubah agar
hanya menghitung soket `/hubs/queues`, menjalankan setiap skenario di jendela 60 detik tersendiri, dan mengukur dari
sebelum simpan. Bukti resmi adalah putaran `01-53-36`.

`MANUAL TEST: DONE — Playwright (Edge headless, ignoreHTTPSErrors) 2026-11-04; F0–F5 PASS, regresi FE-NV-005 7/8 (L2-E data kedaluwarsa).`

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`

**Tidak dijalankan:** `npm run build` (`NOT RUN`: dev server Rizki aktif di port 3000; DoD kartu tidak memintanya).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-019` TV tersambung, TTV Kritis disimpan → kartu berubah tanpa menunggu 60 detik | Terpenuhi, data nyata | F1 |
| `NV-AC-003` kartu Kritis merah berkedip di urutan teratas, tanpa bunyi | Terpenuhi, data nyata (kelompok kritis teratas, sesudah 325-A yang juga kritis) | F1 |
| `NV-AC-022` sisi klien: TV tidak bergabung ke grup lain dan sinyal tidak memuat identitas | Terpenuhi: hanya `JoinNurseViewFloor`; isi kartu tetap dari `/board` | F5; `BE-NV-007` H3, H4 |
| Kartu: join sesudah tersambung, termasuk saat sambung ulang | Terpenuhi | F0, F4 |
| Kartu: event → muat ulang, paling sering 1×/5 detik | Terpenuhi | F1, F3 |
| Kartu: muat ulang 60 detik tetap berjalan | Terpenuhi | F5 |
| DoD — lint lulus | Terpenuhi | §6 |
| DoD — rekaman berjam simpan TTV → kartu berubah | Terpenuhi | `rekaman-tv.webm`, `F1-sebelum.png`, `F1-sesudah-kritis.png` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` `NOT RUN`. Di mode dev, klien realtime mencetak `console.warn` saat sambung ulang (perilaku bawaan klien bersama, diam di produksi) |
| Masalah yang diketahui | Jalur "koneksi awal gagal → coba join lagi pada muat ulang 60 detik" hanya terbukti lewat review kode |
| Data uji | Pemesanan bed uji pasien E (326-B) sudah kedaluwarsa; bila regresi L2 ingin hijau lagi, pemesanan baru perlu dibuat lewat aplikasi. Sandi `NVW-000003` direset skrip |
| Dependency backend | `BE-NV-007` ✅ |
| Perubahan sampingan | Penambahan pada `queue-realtime-client.js` (berkas bersama, tanpa mengubah perilaku konsumen lain) dan satu pemecahan baris Prettier di hook |
| Status Git | Tiga berkas frontend belum di-commit. Laporan ini ada di repo backend dan belum di-commit |
| Langkah berikutnya | Commit oleh Rizki. Sisa modul: butir 🟡 `BE/FE-NV-005` (Morse, isolasi, `NV-AC-021`, keterbacaan ±3 m) dan `BE/FE-NV-006` (`NV-AC-009`, menunggu perbaikan HR) |
