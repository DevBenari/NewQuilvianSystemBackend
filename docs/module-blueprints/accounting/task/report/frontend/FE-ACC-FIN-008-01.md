# Laporan Perubahan Frontend — `FE-ACC-FIN-008-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-008-01` |
| Judul | Tombol unduh neraca saldo |
| Slice | Gelombang pengiriman `FIN-0`, gelombang eksekusi 2 |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 1, kartu `FE-ACC-FIN-008-01` |
| Trace | ACC-FIN-008 bagian Trial Balance (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv84`); `FR-FIN-030`; `ACC-DEC-153`; `03-frontend-architecture.md` bagian 15.5 |
| Contract version | `ACC-API-0.17` (`GET /general-ledger/trial-balance/export`) + delta `ACC-DEC-157` (`422`) |
| Dependency | `BE-ACC-FIN-008-01` ✅ (diuji bersama task ini), `FE-ACC-FIN-005-01` ✅ (`accounting-download-utils.js`) |
| Task mode | `FRONTEND` — instruksi Rizki 25 November 2026 |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `ff46618d8` |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-4 terbukti 25 November 2026: uji layar Playwright oleh agent terhadap `next dev` yang dinyalakan Rizki (11/11), ESLint 0/0. UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Neraca Saldo hanya dapat dibaca di layar, jadi angkanya harus disalin satu per satu sebelum diserahkan ke auditor. Selain itu, galat saat neraca saldo dimuat tidak pernah tampil di layar: `listError` disimpan di Redux tetapi tidak dibaca view.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menyimpan neraca saldo satu periode sebagai CSV yang angkanya sama dengan layar |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Tombol Unduh CSV di layar Neraca Saldo |
| Langkah | 1) Pilih periode. 2) Neraca saldo tampil. 3) Klik Unduh CSV; selama proses tombol menjadi "Mengunduh..." dan mati. 4) Berkas `neraca-saldo_{kode periode}.csv` tersimpan |
| Jalur tidak normal | Jaringan gagal → "Berkas gagal diunduh.". Backend menolak (mis. `422` lebih dari 50.000 baris) → pesan backend tampil apa adanya. Neraca saldo gagal dimuat → pesan galat tampil. Tanpa hak → `AccessDeniedGate` |
| Hasil | Berkas CSV; tabel dan ringkasan di layar tidak berubah |

**Contoh.** Petugas memilih September 2026 (2026-09). Layar menampilkan 5 akun, Total Debit dan Total Kredit Rp 2.900.000, penanda Seimbang. Unduh CSV menghasilkan `neraca-saldo_2026-09.csv` berisi 5 baris akun yang sama dan berurutan sama, satu baris Total, dan satu baris Status "Seimbang".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/accounting/general-ledger/trial-balance-view.jsx` | `actions` `DataFilter` menjadi dua tombol: Unduh CSV dan Muat Ulang; `InformationAlert` untuk galat neraca saldo (`errorMessage`) dan galat unduhan (`downloadError`) |
| `src/lib/hooks/corporate/accounting/general-ledger/use-trial-balance.jsx` | `handleDownload` (blob lewat `InstanceAxios`, `saveBlobAsFile`, `readDownloadErrorMessage` dari `FE-ACC-FIN-005-01`), `downloading`, `downloadError`, `canDownload`, `errorMessage`; galat unduhan dibersihkan saat periode berganti atau diatur ulang |
| `src/lib/state/slice/corporate/accounting/accounting-general-ledger-slice.jsx` | `getTrialBalance` memakai pola `requestId`: `fulfilled`/`rejected` permintaan lama diabaikan (perbaikan yang ditugaskan kartu, sama dengan `FE-ACC-FIN-005-01` bagian 9) |
| `src/lib/state/slice/corporate/accounting/accounting-period-slice.jsx` | `getAccountingPeriodList` memakai pola `requestId` yang sama. **Delta lingkup:** berkas ini tidak tercantum di kartu. Diperbaiki karena cacat yang sama ditemukan saat uji layar ini (bagian 9) |

Nol baris komentar baru. Komentar lama tidak disunting.

### 3.2 Dampak kontrak, keamanan, privasi

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Memakai `GET /trial-balance/export` persis `ACC-API-0.17`. **Nol delta kontrak** dari frontend |
| Nama berkas | Disusun frontend `neraca-saldo_{kode periode}.csv`, sama dengan `Content-Disposition` backend (CORS tidak mengekspos header itu — temuan `BE-ACC-FIN-005-01`) |
| Keamanan | Penegakan hak tetap di backend; tombol hanya aktif bila periode terpilih dan data tidak sedang dimuat |
| Pemakai lain `getAccountingPeriodList` | Periode Akuntansi, Tutup Periode, Kotak Masuk Kejadian, Rekonsiliasi Control Account. Semuanya mendispatch dari effect dengan pembatalan di cleanup dan memakai satu state `list`, jadi aturan "permintaan terakhir menang" benar untuk semuanya |

## 4. Gerbang keputusan base component

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Tombol Unduh CSV | `BaseButton` di `actions` `DataFilter` | dipakai `general-ledger-view.jsx` (`FE-ACC-FIN-005-01`) | REUSE | `loading`, `loadingLabel` "Mengunduh..." |
| Galat unduhan | `InformationAlert variant="danger"` | dipakai `general-ledger-view.jsx` | REUSE | — |
| Galat memuat neraca saldo | `InformationAlert variant="danger"` | sama | REUSE | — |

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Layar Buku Besar (`FE-ACC-FIN-005-01`) |
| Letak tombol | Di `actions` `DataFilter`, bukan di header halaman (PRD 7.1). Skema kartu menaruhnya di baris saringan; posisi akhirnya di baris judul kartu saringan, sama dengan Buku Besar |
| Grep hex baru / `!important` / `<button>`/`<table>` mentah / inline style pada diff | 0 / 0 / 0 / 0 / 0 |
| `globals.css` | Tidak disentuh |
| Loading label tombol ber-API | `Mengunduh...` |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 4 berkas tersentuh | Kode keluar `0` — 0 galat, 0 peringatan | `PASS` |
| Pemindaian baris komentar baru pada diff | Nol | `PASS` |
| `npm run build` | Tidak dijalankan — server dan build frontend dijalankan Rizki | `NOT RUN` |
| Uji layar `02-uji-layar.mjs` putaran 1 (09.10 WIB) | 7/8 — V1 gagal: **cacat ditemukan** (bagian 9) | `FAIL` → diperbaiki |
| Putaran 2 sesudah perbaikan + skenario V1b (09.13 WIB) | 9/9 | `PASS` |
| Putaran 3 + skenario V9, V10 untuk AC-3 (09.16 WIB, putaran resmi) | 11/11 | `PASS` |
| CSV dari layar vs CSV dari API (`01-uji-api.mjs` B1) | Identik kecuali baris "Diunduh" | `PASS` |

`MANUAL TEST: PASS` — Playwright (Edge, headless) terhadap `next dev` yang dinyalakan Rizki, 25 November 2026 09.16–09.17 WIB. Layar dibuka lewat klik menu Akuntansi → Neraca Saldo. Bukti JSON, PNG, dan CSV ada di `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-01-20261125/`. JSON putaran 1 dan 2 disimpan terpisah (`02-uji-layar-run1-v1-gagal.json`, `02-uji-layar-run2-9dari9.json`).

Simulasi yang dipakai, sebagai berikut:

- V1b, V2, dan V5 menunda respons **asli** 2–3 detik lewat `page.route`; isi respons tidak diubah.
- V7 dan V9 mensimulasikan putus jaringan (`route.abort`).
- V10 mensimulasikan respons `422` lewat `route.fulfill`, karena data dev tidak dapat mencapai 50.000 baris.

V8 memakai pengguna nyata tanpa hak (`QUILVIAN_ASET_APPROVER`).

`AUTOMATED TEST: SKIPPED (opsional) — unit test tidak ditulis atas kesepakatan kerja Rizki.`

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Tombol mati sebelum periode dipilih | **Terpenuhi** | V1 (PNG `01-awal`) |
| AC-2 Unduhan menghasilkan `neraca-saldo_{kode periode}.csv` | **Terpenuhi** | V5: `neraca-saldo_2026-09.csv`; tombol "Mengunduh..." mati selama proses; kode akun dan urutan sama dengan tabel (PNG `04-sedang-mengunduh`, berkas `04-unduhan-layar.csv`) |
| AC-3 Gagal → "Berkas gagal diunduh."; layar tidak berubah | **Terpenuhi** | V9: pesan tampil, tidak ada berkas, tabel 5 akun tetap, tombol aktif kembali (PNG `04b`). V10: pesan `422` backend tampil apa adanya dan menggantikan pesan sebelumnya (PNG `04c`) |
| AC-4 Lint nol galat | **Terpenuhi** | ESLint kode keluar `0` |

**Regresi kartu** (tabel, pemberitahuan, dan saringan tidak berubah): V2–V4 keadaan memuat dan penguncian saringan, V6 keadaan kosong (PNG `05-kosong`), V7 galat memuat — sebelumnya galat ini tidak pernah tampil (PNG `06-galat`), V8 tanpa hak → "Ups! Akses Ditolak" (PNG `07-tanpa-hak`).

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | (1) Pemilih Tanggal Awal/Tanggal Akhir hanya tampilan; nilainya diisi dari periode dan tidak menyaring data. (2) Saat Muat Ulang gagal sesudah periode kosong, layar menampilkan galat sekaligus "Belum ada saldo pada periode ini." dari data terakhir (PNG `06-galat`). (3) Akun tanpa `GeneralLedger : Read` tetap melihat butir menu Neraca Saldo; halamannya menolak dengan benar (PNG `07`). Ketiganya di luar lingkup kartu |
| Risiko | Rendah. Perbaikan slice periode hanya mengabaikan respons permintaan yang sudah digantikan |
| Status Git frontend | `M` `trial-balance-view.jsx`, `use-trial-balance.jsx`, `accounting-general-ledger-slice.jsx`, `accounting-period-slice.jsx`. Belum di-commit |
| Langkah berikutnya | `BE-ACC-FIN-008-02` (laba rugi), lalu `FE-ACC-FIN-008-02` |

## 9. Cacat yang ditemukan saat uji dan diperbaiki

| Hal | Isi |
| --- | --- |
| Gejala | Saat layar baru dibuka, keadaan kosong menyatakan "Badan hukum ini belum memiliki periode akuntansi." padahal periode ada. Tangkapan 09.10:34.7 WIB, respons `/accounting/periods` baru tiba 09.10:35.3 (PNG `01-awal-run1-v1-gagal.png`) |
| Penyebab | Reducer `getAccountingPeriodList.rejected` menurunkan `listLoading` tanpa memeriksa apakah permintaan itu sudah digantikan. Di dev (`reactStrictMode`) effect berjalan dua kali: pending1, pending2, lalu rejected1 karena dibatalkan → `listLoading` padam saat permintaan kedua masih berjalan. Hook lalu menganggap daftar periode kosong |
| Perbaikan | Pola `requestId` yang sama dengan `getTrialBalance` dan `FE-ACC-FIN-005-01`: `pending` menyimpan `action.meta.requestId`; `fulfilled`/`rejected` milik permintaan lama diabaikan |
| Bukti | V1b: respons periode ditunda 3 detik; empat cuplikan 09.13:44–09.13:45 WIB semuanya "Periode belum dipilih." dan tanpa kosong palsu; periode tiba 09.13:48 (PNG `01b-periode-sedang-dimuat`). Putaran resmi 11/11 |
