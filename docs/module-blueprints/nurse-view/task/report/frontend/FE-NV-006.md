# Laporan Perubahan Frontend — `FE-NV-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-006` |
| Judul | Header perawat jaga tampil |
| Slice | `MVP-2` — papan lantai |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — kartu `FE-NV-006` |
| Trace | `FR-NV-017`; `NV-DEC-012`, `021`, `033`; `03-frontend-architecture.md` skema `FE-NVW-04` (wilayah "Kelompok bangsal") |
| Contract version | `0.1.2` — `approved` (Rizki Gunawan, 2026-11-03, `NV-DEC-033`) |
| Wewenang UI | Isi dan teks kepala bangsal dikunci skema `FE-NVW-04`. Tata letak, warna, dan pemotongan teks panjang `DEV_DISCRETION` |
| Dependency | `BE-NV-006` 🟡, `FE-NV-005` 🟡 |
| Klasifikasi | `STANDARD` — tiga berkas milik Nurse View, tanpa berkas bersama |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend | Dikerjakan di atas `61b5e3f35`; source di-commit Rizki sebagai `2e510a555` |
| Commit backend yang dijadikan rujukan | `6489c32c` + perubahan `BE-NV-006` yang dibuild Rizki 3 Nov 10.53 |
| Tanggal | 2026-11-03 |
| Status | 🟡 **SEBAGIAN 3 November 2026** — `npx eslint` 0 error 0 warning, Prettier lulus. Uji kepala bangsal 4/4 `PASS` (S1, S2 data nyata; S3, S4 jawaban tiruan). Regresi layar `FE-NV-005` 8/8 `PASS`. **`NV-AC-009` hanya terbukti dengan jawaban tiruan**, karena roster HR tidak bisa diisi di dev (`BE-NV-006` §5). Verifikasi developer, bukan UAT |

---

## 1. Keadaan yang ditemukan di awal

Kelompok bangsal di papan hanya menampilkan nama bangsal. Data `RosterAvailable` dan `OnDutyNurses` dari `/board`
belum dipakai sama sekali.

---

## 2. Proses bisnis dari sisi pengguna

1. Di atas kartu setiap bangsal, TV menampilkan nama bangsal, lalu di sebelahnya "Perawat jaga: Sari, Dewi (Pagi)".
2. Bila dua shift berimpit, misalnya saat serah terima, nama dikelompokkan per shift: "Sari, Dewi (Pagi) · Rina (Siang)".
3. Bila backend menjawab `RosterAvailable = false`, kepala bangsal menampilkan "Roster shift ini belum tersedia" dengan
   warna redup. Layar tidak pernah menebak nama.
4. Kepala bangsal selalu satu baris. Daftar yang lebih panjang dari lebar layar dipotong dengan elipsis, sehingga
   kartu bed tidak menyusut.
5. Bila satu bangsal terbagi ke beberapa halaman, kepalanya tampil di setiap halaman.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`nurse-view-floor-board-view.jsx`, `nurse-view-board-utils.jsx`, `nurse-view-floor-board.module.css`,
`use-nurse-view-floor-board.jsx`; api §3 (`Wards[]`), skema `FE-NVW-04`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/nurse-view-display/nurse-view-board-utils.jsx` | `formatOnDutyNurses`: nama dikelompokkan per `shiftLabel`, nama kosong dilewati. `paginateBoardWards` menyertakan `rosterText` di setiap segmen, dan teks itu kosong bila `rosterAvailable = false` |
| `src/components/view/nurse-view-display/nurse-view-floor-board-view.jsx` | Kepala bangsal menjadi `div` berisi `h2` nama bangsal dan `p` roster; konstanta "Roster shift ini belum tersedia" |
| `src/style/nurse-view-display/nurse-view-floor-board.module.css` | `.wardHeader`, `.wardRoster`, `.noRoster` dengan token. Satu baris dengan `text-overflow: ellipsis`. Satu baris `color-mix` lama dari `FE-NV-005` dipecah sesuai Prettier, tanpa perubahan nilai |

### 3.3 Kepatuhan arsitektur frontend

Tanpa service, hook, Redux, atau endpoint baru, karena datanya sudah ada di jawaban `/board`. `globals.css` tidak
diubah.

| Kebutuhan UI | Kandidat base | Status | Rekomendasi |
| --- | --- | --- | --- |
| Teks roster di kepala bangsal | — | `COMPOSE` | Satu paragraf dengan token, mengikuti ukuran huruf papan |

`UI GATE: PASS — tanpa komponen baru.`

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna | Bukti |
| --- | --- | --- |
| Roster berisi | "Perawat jaga: Sari, Dewi (Pagi) · Rina (Siang)" | S3 (simulasi) |
| Roster kosong atau bangsal tanpa unit | "Roster shift ini belum tersedia" | S1, S2 |
| Daftar terlalu panjang | Satu baris, diakhiri elipsis | S4 (simulasi) |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Layar memakai `Wards[].RosterAvailable` dan `Wards[].OnDutyNurses[]` (`DisplayName`,
`ShiftLabel`) dari `GET /v1/health-services/nurse-view-management/display-runtime/board`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 2 berkas JSX | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `npx prettier --check --end-of-line crlf` 3 berkas | Lulus | `PASS` | Keluaran perintah |
| Grep anti-regresi | Warna literal 0, `!important` 0, `<button>` mentah 0, komentar 0; CRLF | `PASS` | Keluaran perintah |
| S1 lantai 3, data nyata | Kepala "Rawat Inap" menampilkan "Roster shift ini belum tersedia", satu baris, tanpa kata "Perawat jaga"; kartu tidak terpotong; layar tidak meluap | `PASS` | `roster-papan-2026-11-03T04-13-05-752Z/S1.json`, `S1-lantai-3-tanpa-roster.png` |
| S2 lantai 2, data nyata | ICU dan Ruang Bayi (tanpa `OrganizationUnitId`) sama-sama menampilkan "Roster shift ini belum tersedia" | `PASS` | `R3.json`, `S2-lantai-2-tanpa-unit.png` |
| S3 tiga perawat dua shift (simulasi) | "Perawat jaga: Sari, Dewi (Pagi) · Rina (Siang)", satu baris, tidak terpotong; kartu utuh di 1920×1080 | `PASS` simulasi | `S3.json`, `S3-simulasi-tiga-perawat.png` |
| S4 tiga puluh perawat di 1366×768 (simulasi) | Satu baris diakhiri elipsis; kartu tidak terpotong; layar tidak meluap | `PASS` simulasi | `S4.json`, `S4-simulasi-30-perawat-1366x768.png` |
| Regresi layar `FE-NV-005` L1–L8 | 8/8 `PASS` (44 cek): kritis berkedip, rotasi 66 bed, memori dan suara, pita data basi (90 detik sesudah putus), ikon padat, lantai kosong, dua bangsal dengan tinggi kartu seragam 269 px, 1366×768 | `PASS` | `uji-nv-005-20261103/layar-2026-11-03T04-08-16-148Z` |
| Pencarian kredensial pada bukti JSON | Nol sandi, nol token | `PASS` | Pencarian pola dan nilai env |

Bukti berada di `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-006-20261103/`, kecuali yang disebut lain.

**Riwayat.** Putaran `roster-papan-2026-11-03T04-06-12-428Z` S4 `FAIL`. Penyebabnya asumsi skrip: 14 nama ternyata muat
utuh di 1366×768, sehingga tidak ada elipsis. Tangkapan layarnya menunjukkan semua nama terbaca. Skrip diubah menjadi 30
nama, dan produk tidak diubah. Putaran `04-07-31` lulus, tetapi sempat berjalan bersamaan dengan regresi `FE-NV-005`
yang mereset login perangkat yang sama. Karena itu buktinya diganti dengan putaran tunggal `04-13-05`.

`MANUAL TEST: DONE — Playwright (Edge headless, ignoreHTTPSErrors) 2026-11-03; S1–S4 dan regresi L1–L8 PASS.`

`AUTOMATED TEST: SKIPPED (opsional) — ketentuan proyek.`

**Tidak dijalankan:** `npm run build` (`NOT RUN`: dev server Rizki aktif di port 3000; DoD kartu tidak memintanya).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `NV-AC-009` header menampilkan ketiga perawat jaga | **Tampilan terbukti dengan jawaban tiruan; data nyata belum ada** (`BE-NV-006` §6) | S3 |
| `NV-AC-017` "Roster shift ini belum tersedia", tanpa nama ditebak | Terpenuhi, data nyata: roster kosong (S1) dan bangsal tanpa unit (S2) | S1, S2 |
| DoD — lint lulus | Terpenuhi | §6 |
| DoD — tangkapan layar kedua keadaan | Terpenuhi; keadaan berisi memakai jawaban tiruan | `S1`, `S2`, `S3` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` `NOT RUN` |
| Masalah yang diketahui | Nama yang terpotong elipsis tidak terbaca di TV. Dengan ukuran huruf sekarang, kira-kira 14 nama masih muat di 1366×768 |
| Dependency backend | `BE-NV-006` 🟡 |
| Perubahan sampingan | `NONE`, selain pemecahan satu baris `color-mix` sesuai Prettier |
| Status Git | Source di `2e510a555`; working tree Nurse View bersih. Laporan ini ada di repo backend dan belum di-commit |
| Langkah berikutnya | Uji ulang `NV-AC-009` dengan data nyata sesudah temuan HR diperbaiki; lalu `FE-NV-007` |
