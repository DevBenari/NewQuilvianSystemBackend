# Laporan Perubahan Frontend — `FE-ACC-FIN-008-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-008-03` |
| Judul | Layar neraca dan menunya |
| Slice | Gelombang pengiriman `FIN-1` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 1, kartu `FE-ACC-FIN-008-03` |
| Trace | ACC-FIN-008 Balance Sheet (ClickUp `z8m132yv84`); `FR-FIN-012`, `014`, `020`..`022`, `040`; `ACC-DEC-143`..`146`, `149`, `152`, `154`; `03-frontend-architecture.md` bagian 15.2, 15.7, 15.9 |
| Contract version | `ACC-API-0.17` (`GET /general-ledger/balance-sheet`, `/balance-sheet/export`) — approved Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | `BE-ACC-FIN-008-03` ✅ (commit backend `70f2b869`), `FE-ACC-FIN-008-02` ✅ (tabel laporan, util, pola cetak) |
| Task mode | `FRONTEND` — perintah Rizki 25 November 2026 "lanjut FE-ACC-FIN-008-03" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `ee1271d0a` |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-9 terbukti 25 November 2026: uji layar Playwright agent terhadap `next dev` Rizki 14/14, ESLint 0/0, kompilasi `next dev` 200 pada empat rute; `npm run build` `PASS` (Rizki, 11:11:15 WIB). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Neraca dari backend (`BE-ACC-FIN-008-03`) belum punya layar. Akibatnya tiga hal belum terlihat oleh petugas: posisi aset, liabilitas, dan ekuitas; status seimbang; dan peringatan tahun buku yang belum ditutup.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Membaca neraca per akhir periode beserta pembanding akhir tahun buku lalu, mengenali tahun yang belum ditutup, dan menelusuri saldo ke buku besar |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Menu Akuntansi → Neraca |
| Langkah | 1) Pilih periode; bila perlu tampilkan akun bersaldo nol. 2) Ringkasan Total Aset, Total Liabilitas, Total Ekuitas, dan Selisih dengan status Seimbang/Tidak seimbang. 3) Peringatan tahun belum ditutup, neraca tidak seimbang, atau pembanding tidak ada. 4) Klik saldo akun → Buku Besar akun itu dari awal data sampai tanggal kolom. 5) Unduh CSV atau Cetak |
| Jalur tidak normal | Gagal memuat → "Neraca gagal dimuat." + Coba lagi; gagal unduh → "Berkas gagal diunduh."; tanpa hak → `AccessDeniedGate` |
| Hasil | Neraca di layar, berkas `neraca_{periode}.csv`, atau cetakan tanpa menu, saringan, dan tombol |

**Contoh dari data dev** (30 September 2026):

- **Ringkasan:** Total Aset Rp 3.230.000 sama dengan Total Ekuitas, selisih Rp 0, berstatus Seimbang.
- **Peringatan:** "Tahun buku 2019 belum ditutup." Tahun 2020 tidak disebut karena JT/2020/12/00001 sudah disahkan.
- **Baris hitungan:** dua baris tampil miring dengan penanda "Baris hitungan", yaitu "Laba (Rugi) Tahun Lalu Belum Ditutup" Rp 270.000 dan "Laba (Rugi) Tahun Berjalan" Rp 2.000.000.
- **Akun kontra:** Bank Operasional bersaldo kredit tampil (Rp 40.000).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/corporate/accounting/balance-sheet/page.jsx`, `balance-sheet-client.jsx` | **Baru.** Rute tipis |
| `src/components/view/corporate/accounting/general-ledger/balance-sheet-view.jsx` | **Baru.** Hero, `DataFilter` (periode, akun nol; Unduh CSV, Cetak, Muat Ulang), `SummaryGrid` empat kartu (kartu Selisih memuat `StatusBadge`), `InformationAlert` (galat + Coba lagi, gagal unduh, tahun belum ditutup, tidak seimbang, pembanding tidak seimbang, pembanding tidak ada), `FinancialStatementTable` tiga kolom |
| `src/lib/hooks/corporate/accounting/general-ledger/use-balance-sheet.jsx` | **Baru.** Pola `use-income-statement` tanpa unit biaya dan YTD; tautan Buku Besar tanpa `from` (awal data) sampai tanggal kolom, termasuk kolom pembanding |
| `src/lib/constants/corporate/accounting/general-ledger/balance-sheet-constants.jsx` | **Baru.** Config, keadaan kosong, status, salinan teks |
| `src/style/corporate/accounting/balance-sheet-view.module.css` | **Baru.** Salinan pola `income-statement-view.module.css` (pembungkus saringan, aturan cetak `body:has(.printRoot)`) |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | + thunk `getBalanceSheet` (pola `requestId`), state, `clearBalanceSheet`, tiga selector |
| `src/components/view/corporate/accounting/general-ledger/financial-statement-table.jsx` | + tampilan baris hitungan (`RowKind = 3`): nama miring, penanda "Baris hitungan", tanpa tautan |
| `src/lib/constants/corporate/accounting/general-ledger/financial-statement-constants.jsx` | + `computedMarker` |
| `src/utils/corporate/accounting/general-ledger/financial-statement-utils.js` | + `buildShortDateLabel` ("30 Sep 2026") |
| `src/style/corporate/accounting/financial-statement-table.module.css` | + `.computedName` (token warna) |
| `src/utils/menu-sidebar/menu-items.jsx` | + butir **Neraca** sesudah Laba Rugi (`RiScales3Line`, yang sudah diimpor berkas itu) |
| `src/lib/constants/corporate/accounting/accounting-constants.jsx` | + kartu beranda Neraca |

Nol baris komentar baru. **Delta lingkup:** tiga berkas bersama dari `008-02` disentuh tanpa mengubah perilakunya untuk laba rugi, yaitu tabel, konstanta, dan util. Regresi Laba Rugi ada di bagian 6.

### 3.2 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Persis `ACC-API-0.17`; nol delta |
| Penyusunan angka | Tidak ada penjumlahan ulang; urutan, kedalaman, subtotal, total, status seimbang, dan selisih dibaca dari respons |
| Tautan | Token rute privat; rentang "awal data s.d. tanggal kolom" sesuai `03-frontend-architecture.md` bagian 15.7 |
| Basi | Mengganti periode, akun nol, atau badan hukum membersihkan laporan; respons lama dibuang lewat `requestId` |

## 4. Gerbang keputusan base component

`UI GATE: 9 elemen — REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Hero dan badan hukum | `Hero` + `AccountingHeroLegalEntity` | Laba Rugi | REUSE | — |
| Saringan periode dan akun nol | `DataFilter` + `FilterSelect` | Laba Rugi | REUSE | Pilihan akun nol sama dengan Laba Rugi (`INCOME_STATEMENT_ZERO_OPTIONS`) |
| Unduh, Cetak, Muat Ulang, Coba lagi | `BaseButton` | Laba Rugi | REUSE | — |
| Ringkasan tiga total | `SummaryGrid` | Laba Rugi | REUSE | — |
| Status seimbang | `SummaryGrid` + `StatusBadge` di `subtitle` kartu Selisih | `SummaryGrid` merender `subtitle` sebagai node; kartu Selisih juga ada di Neraca Saldo | COMPOSE | Lihat pilihan A |
| Peringatan dan catatan | `InformationAlert` | Laba Rugi | REUSE | — |
| Tabel bertingkat | `FinancialStatementTable` (COMPOSE atas `DataTable` di `008-02`) | `008-02` | REUSE | + varian baris hitungan |
| Tautan saldo | `BaseButton ghost` | `008-02` | REUSE | — |
| Tampilan cetak | Pola `printRoot` `008-02` | `008-02` | REUSE | Salinan CSS kecil |

**Pilihan A — letak status seimbang.**

1. **`StatusBadge` di `subtitle` kartu keempat "Selisih" pada `SummaryGrid` — direkomendasikan dan dipakai.** Status berada di baris ringkasan, sesuai skema. Nilai selisih juga terbaca. Base component tidak diubah.
2. `StatusBadge` di `actions` Hero di samping badan hukum. Status terlihat di cetakan, tetapi letaknya berbeda dari skema, dan Hero menjadi padat.
3. EXTEND `SummaryGrid` dengan prop `badge` per item. Hasilnya lebih rapi, tetapi menyentuh base component yang dipakai banyak layar.

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Laba Rugi (`FE-ACC-FIN-008-02`) |
| Grep: warna literal / typography / `<button>` / `<table>` / `fw-` `fs-` / `!important` | 0 / 0 / 0 / 0 / 0 / 0 |
| Inline style | Hanya `--statement-depth` dinamis di komponen tabel (dari `008-02`) |
| `globals.css` | Tidak disentuh |
| Aksesibilitas | Negatif dibaca "minus", kosong dibaca "tidak tersedia", baris hitungan bertanda teks, tombol saldo ber-`aria-label` |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 11 berkas JS/JSX tersentuh | Kode keluar `0` — 0 galat, 0 peringatan | `PASS` |
| Pemindaian baris komentar baru | Nol | `PASS` |
| Kompilasi `next dev` (lint tidak menangkap galat sintaks — temuan `008-02`) | `/`, `/balance-sheet`, `/income-statement`, `/trial-balance` → `200` | `PASS` |
| `npm run build` | Dijalankan Rizki 25 November 2026; `.next/BUILD_ID` 11:11:15 WIB, sesudah suntingan source terakhir 11:01:46; keluaran memuat rute `corporate/accounting/balance-sheet` | `PASS` |
| Uji layar `fin-008-03-20261125/03-uji-layar.mjs` S1–S14 (11.04:08–11.04:56 WIB) | 14/14 | `PASS` |
| Regresi Laba Rugi `04-regresi-laba-rugi.mjs` (salinan skrip `008-02`, 11.05–11.06 WIB) | 15/16. L12 (2020-12) gagal karena tahun 2020 kini berisi data uji `BE-ACC-FIN-008-03`. Angka baru (60.000 / 300.000 / (240.000) / 60.000 / 270.000 / (210.000)) sama persis dengan uji backend P4. Bukan regresi kode | `PASS` dengan catatan |

Saat pengerjaan, impor `RiScales3Line` sempat tertulis ganda di `menu-items.jsx`, karena ikon itu sudah diimpor berkas tersebut. Impor ganda itu dihapus sebelum kompilasi dan uji.

`MANUAL TEST: PASS`. Playwright (Edge, headless) dijalankan terhadap `next dev` Rizki, dengan layar dibuka lewat klik menu. Bukti JSON, PNG, dan CSV ada di `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-03-20261125/`.

Simulasi yang dipakai:

- S2 menunda respons asli selama 2,5 detik.
- S10 mengubah respons asli menjadi `isBalanced = false` dan `difference = 100` lewat `route.fulfill`, karena data dev selalu seimbang.
- S12 dan S13 memutus jaringan (`route.abort`).
- S7 memakai emulasi media cetak.

S14 memakai pengguna nyata tanpa hak.

`AUTOMATED TEST: SKIPPED (opsional) — unit test tidak ditulis atas kesepakatan kerja Rizki.`

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Menu Neraca sesudah Laba Rugi membuka layar | **Terpenuhi** | S1: … Neraca Saldo, Laba Rugi, **Neraca**, Rekonsiliasi Control Account … (PNG `S01-menu`) |
| AC-2 Tiga bagian, total L+E, status seimbang | **Terpenuhi** | S2 (PNG `S02-september`) |
| AC-3 Peringatan tahun belum ditutup menyebut setiap tahun (`F4`) dan hilang sesudah `JT` (`F5`) | **Terpenuhi** | S3: peringatan "Tahun buku 2019 belum ditutup." dari `UnclosedFiscalYears = [2019]`; 2020 tidak disebut sesudah JT/2020/12/00001. Keadaan sebelum JT 2020 (`[2019, 2020]`) dibuktikan di API (`BE-ACC-FIN-008-03` P2) |
| AC-4 Tidak seimbang tetap tampil dengan selisih | **Terpenuhi (simulasi)** | S10: "Neraca tidak seimbang, selisih Rp 100. Periksa Neraca Saldo periode ini." + badge Tidak seimbang, tabel tetap tampil (PNG `S07-tidak-seimbang`) |
| AC-5 Baris laba hitungan berbeda dan tidak bertautan | **Terpenuhi** | S4: miring + penanda, nol tombol; baris akun bertombol |
| AC-6 Klik akun → Buku Besar sampai tanggal kolom (`F14`) | **Terpenuhi** | S8: `to=2026-09-30` tanpa `from`, alamat bertoken (PNG `S05-buku-besar`); S9 kolom pembanding → `dateTo=2019-12-31` |
| AC-7 Unduh CSV dan Cetak | **Terpenuhi** | S6 `neraca_2026-09.csv`; S7 cetak (PNG `S04-cetak`) |
| AC-8 Belum pilih, kosong, gagal, memuat | **Terpenuhi** | S1, S11 (PNG `S08-kosong`), S12 (PNG `S09-galat`), S2 (PNG `S02-memuat`) |
| AC-9 Lint nol galat | **Terpenuhi** | ESLint kode keluar `0` |
| Negatif: tanpa hak; gagal unduh | **Terpenuhi** | S14, S13 |
| Tambahan `F8`, pembanding tersedia | **Terpenuhi** | S5 1-1004 "(Rp 40.000)", subtotal 1-1000 Rp 2.330.000; S9 kolom 31 Des 2020 / 31 Des 2019 |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | Akun laba ditahan pada pengaturan akuntansi DB dev menunjuk 3-1001 Modal Disetor (lihat `BE-ACC-FIN-008-03`), sehingga di layar Modal Disetor naik sesudah tutup tahun, bukan 3-3001 Laba Ditahan |
| Risiko | Rendah. Aturan cetak bergantung pada kelas tata letak global yang sama dengan `008-02` |
| Status Git frontend | `M` slice, tabel, konstanta, util, CSS tabel, `menu-items.jsx`, `accounting-constants.jsx`; `??` 6 berkas baru. Belum di-commit |
| Langkah berikutnya | `FIN-1` lengkap (`BE`/`FE` `008-02` dan `008-03`). Sisa ACC-FIN-008 ⛔ (`008-04`..`06`) menunggu keputusan `U-05`, `C-02`, `U-07` |
