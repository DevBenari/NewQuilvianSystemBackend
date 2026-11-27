# Laporan Perubahan Frontend — `FE-ACC-FIN-008-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-008-04` |
| Judul | Layar Rasio Keuangan |
| Slice | Gelombang pengiriman `FIN-5` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 3, kartu `FE-ACC-FIN-008-04` |
| Trace | `FR-FIN-059`, `061`..`065`; `ACC-DEC-174`..`184`; `03-frontend-architecture.md` bagian 17 (skema `FE-L5`) |
| Contract version | `ACC-API-0.19` (`GET /general-ledger/financial-ratios`, `/export`) — approved Rizki 26 November 2026 (`GATE-DESAIN-1126`) |
| Dependency | `BE-ACC-FIN-008-04` ✅ (26 November 2026), `FE-ACC-FIN-001-03` ✅ |
| Task mode | `FRONTEND` — perintah Rizki 26 November 2026 "lanjut" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `64ba2c07e` + `FE-ACC-FIN-001-03` belum di-commit |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-8 terbukti 26 November 2026: uji layar Playwright terhadap `next dev` Rizki **8/8** (putaran ketiga; dua putaran awal gagal karena skrip, bukan layar — bagian 6), ESLint 0 galat 0 peringatan pada berkas tersentuh. `npm run build` `NOT RUN` (milik Rizki). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Backend sudah menghitung delapan rasio (`BE-ACC-FIN-008-04`), tetapi belum ada layar maupun butir menu untuk
membacanya, mengunduhnya, atau mencetaknya.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Petugas akuntansi membaca delapan rasio satu periode beserta asal angka dan alasannya |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Langkah | 1) Menu Akuntansi → **Rasio Keuangan** (sesudah Neraca). 2) Pilih periode. 3) Baca tabel: nama + rumus; per kolom hasil `1,25x`/`12,50%` atau lencana status + alasan; pembilang ÷ penyebut rupiah. 4) Bila ada akun belum berklasifikasi: **Lengkapi di Bagan Akun**. 5) Unduh CSV atau Cetak |
| Jalur tidak normal | Belum pilih periode → keadaan kosong, Unduh/Cetak mati; gagal → pesan + Coba lagi; pembanding tidak ada → info alasan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `src/app/corporate/accounting/financial-ratios/page.jsx`, `financial-ratios-client.jsx` | Baru | Rute, pola `balance-sheet` |
| `src/components/view/corporate/accounting/general-ledger/financial-ratio-view.jsx` | Baru | Layar `FE-L5` |
| `src/lib/hooks/corporate/accounting/general-ledger/use-financial-ratio.jsx` | Baru | Periode, permintaan, normalisasi baris, peringatan klasifikasi, unduhan blob, cetak, tautan Bagan Akun |
| `src/lib/constants/corporate/accounting/general-ledger/financial-ratio-constants.jsx` | Baru | Config, label status, satuan, bunyi keadaan kosong |
| `src/style/corporate/accounting/financial-ratio-view.module.css` | Baru | Sel rasio, aturan cetak (pola `balance-sheet-view.module.css`); token desain saja |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | Diperbarui | + thunk `getFinancialRatios` (pola `requestId`), state, `clearFinancialRatios`, tiga selector |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui | + butir Rasio Keuangan sesudah Neraca (`RiPieChartLine` yang sudah diimpor) |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | Diperbarui | + kartu beranda Rasio Keuangan |

Layar tidak menghitung atau membulatkan ulang: hasil dibaca dari `Value` + `Unit` lalu hanya diformat
(koma desimal, akhiran `x`/`%`). Status dan alasan dibaca apa adanya.

### 3.2 Delta terhadap desain

| Butir | Desain | Source | Alasan |
| --- | --- | --- | --- |
| Keterangan kaki "tanpa ambang" | Kaki tabel | Di layar sebagai deskripsi `DataFilter`; di cetak sebagai paragraf khusus cetak | Hindari kalimat yang sama dua kali di layar; tetap tercetak |
| Judul kolom kini sebelum periode dipilih | Tidak dirinci | "Kini" | Putaran pertama menampilkan "Pembanding" dua kali; dibetulkan saat uji |
| Hasil negatif | Tidak dirinci | `-12,50%`, dibaca "minus 12,50 persen"; rupiah negatif dalam kurung seperti laporan lain | Konsisten dengan `formatStatementAmount` |

## 4. Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 9, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kepala halaman | `REUSE` `Hero` + `AccountingHeroLegalEntity` | Neraca, Laba Rugi |
| Saringan dan aksi | `REUSE` `DataFilter`, `FilterSelect`, `BaseButton` | Neraca |
| Galat, unduhan gagal | `REUSE` `InformationAlert` danger | Neraca |
| Peringatan klasifikasi | `REUSE` `InformationAlert` warning + `BaseButton` | `FE-ACC-FIN-001-03` |
| Info pembanding | `REUSE` `InformationAlert` info | Neraca |
| Tabel rasio | `REUSE` `DataTable` (kolom `render`, tanpa paginasi) | `financial-statement-table.jsx` |
| Lencana status | `REUSE` `StatusBadge` nada `info`/`warning`/`danger` yang ada | Bagan Akun, Neraca |
| Rupiah | `REUSE` `formatStatementAmount` | Laporan keuangan |
| Cetak | `REUSE` pola `screenOnly`/`printRoot` per layar laporan | Neraca, Laba Rugi |

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Layar Neraca (`FE-ACC-FIN-008-03`) |
| Grep berkas baru dan diff: warna literal / `rgb` / `style=` / `!important` / `<button>` / `<table>` / komentar | 0 |
| CSS baru | Satu module CSS layar; seluruh nilai lewat token (`--space-*`, `--base-muted`) |
| `globals.css` | Tidak disentuh |
| Aksesibilitas | Hasil punya teks baca layar ("0,00 persen", "minus 12,50 persen"); status berteks, bukan warna saja |
| Warna sehat/tidak sehat | Tidak ada (`ACC-DEC-161` butir 9) |

## 6. Verifikasi

| Pemeriksaan | Hasil | Status |
| --- | --- | --- |
| `npx eslint` 8 berkas tersentuh (+ ulang 2 berkas sesudah perbaikan judul kolom) | 0 galat, 0 peringatan | `PASS` |
| `npm run build` | Dijalankan Rizki | `NOT RUN` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — kesepakatan kerja Rizki | — |
| Rute dirender `next dev` | `/corporate/accounting/financial-ratios` `200`, dibuka lewat menu | `PASS` |

**MANUAL TEST** (Playwright msedge, layar sungguhan, `next dev` dan backend milik Rizki, 26 November 2026).
Skrip: `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-04-20261126/03-uji-layar.mjs`; keluaran
`03-uji-layar.json`, PNG `V01`, `V02`, `V05-cetak`, `V06`, CSV `V05-rasio-keuangan.csv`. PNG diperiksa sendiri.

| Putaran | Hasil | Sebab |
| --- | --- | --- |
| 1 (11:3x WIB) | V1 PASS lalu galat | Pemilih periode masih mati > 30 detik: `next dev` mengompilasi rute baru pada kunjungan pertama. Jajak `00-jajak.mjs` membuktikan daftar periode termuat dan pemilih aktif 3 detik pada kunjungan berikutnya — bukan cacat layar |
| 2 (11:40) | 7/8 | V5 butir "keterangan cetak" gagal: pemeriksa mengambil kemunculan pertama kalimat (deskripsi filter yang memang disembunyikan saat cetak). PNG cetak menunjukkan keterangan tercetak. Pemeriksa dibetulkan; judul kolom kini sebelum periode dibetulkan menjadi "Kini" |
| 3 (11:41:47–11:42:12) | **8/8** | — |

| ID | Skenario | Hasil |
| --- | --- | --- |
| V1 | Menu: Laba Rugi → Neraca → **Rasio Keuangan** → Rekonsiliasi; belum pilih periode → "Pilih periode untuk menghitung rasio keuangan.", Unduh/Cetak mati | PASS |
| V2 | 2026-09: kepala "Rasio / 30 Sep 2026 / Pembanding" (sebelum pilih: "Rasio / Kini / Pembanding"); delapan baris urutan tetap; DAR `0,00%` dan DER `0,00x` dengan "Rp 0 ÷ Rp 3.230.000"; NPM `100,00%` "Rp 2.000.000 ÷ Rp 2.000.000" | PASS |
| V3 | Current/Quick/Cash dan ROA/ROE lencana "Tidak dapat dihitung" + alasan, tanpa angka; pembanding seluruhnya "Tahun buku 2025 belum ada di sistem." | PASS |
| V4 | Peringatan "Klasifikasi laporan belum lengkap: 4 akun bersaldo." + daftar akun + Lengkapi di Bagan Akun; info pembanding | PASS |
| V5 | Unduh `rasio-keuangan_2026-09.csv` berisi `"0,00%"`; cetak: tanpa filter, tombol, peringatan klasifikasi; tabel, info pembanding, dan keterangan tanpa ambang tercetak | PASS |
| V6 | Simulasi `route.fulfill`: tujuh lencana "Tidak sah" + alasan selisih + "Rp 1.000 ÷ Rp 3.230.000"; NPM tetap `100,00%`; ROE pembanding `-12,50%` "(Rp 25.000) ÷ (Rp 200.000)" + "Ekuitas negatif; tafsirkan dengan hati-hati." | PASS (simulasi) |
| V7 | Tautan membuka `/corporate/accounting/chart-of-accounts?klasifikasi=belum-lengkap`, permintaan `unclassifiedOnly=true` `200` | PASS |
| V8 | Regresi Neraca 2026-09: Rp 3.230.000, Seimbang, peringatan 4 akun | PASS |

Nol data dev ditulis oleh uji layar.

## 7. Acceptance criteria

| # | Kriteria | Bukti |
| --- | --- | --- |
| AC-1 | Menu sesudah Neraca; keadaan kosong; Unduh/Cetak mati | V1 |
| AC-2 | Delapan baris; DAR/DER dengan pembilang ÷ penyebut sama dengan Neraca | V2, V8 |
| AC-3 | Status selain Dihitung tidak pernah angka; layar tidak menghitung ulang | V3, V6; source hook |
| AC-4 | Peringatan klasifikasi + tautan Bagan Akun tersaring | V4, V7 |
| AC-5 | Info pembanding; kolom pembanding seluruhnya lencana | V3, V4 |
| AC-6 | Tidak sah dan catatan ekuitas negatif (simulasi) | V6 |
| AC-7 | Unduh CSV; cetak tanpa filter, tombol, peringatan klasifikasi | V5 |
| AC-8 | ESLint 0 galat | Bagian 6 |

**DoD.** AC-1..AC-8 terbukti; laporan; roadmap dan traceability diperbarui → ✅. `npm run build` milik Rizki.

## 8. Catatan penutup

| Butir | Isi |
| --- | --- |
| Wewenang UI yang dipakai | Kartu roadmap + desain bagian 17; `DEV_DISCRETION` untuk susunan sel dan nada lencana |
| Risiko tersisa | Kunjungan pertama rute baru di `next dev` lambat (kompilasi); tidak berlaku pada build produksi |
| Dependency backend | `BE-ACC-FIN-008-04` ✅ |
| Task berikutnya | Roadmap finishing revisi 3 tidak punya task siap lagi; sisa ⛔ `008-05` (ACC-FIN-013) dan `008-06` (`OQ-160-1`) |
| Git | Tidak ada stage, commit, push, atau merge |
