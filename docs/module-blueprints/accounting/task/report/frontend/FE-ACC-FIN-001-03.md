# Laporan Perubahan Frontend — `FE-ACC-FIN-001-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-001-03` |
| Judul | Peringatan klasifikasi di Neraca |
| Slice | Gelombang pengiriman `FIN-4` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 2, kartu `FE-ACC-FIN-001-03` |
| Trace | `FR-FIN-057`; `ACC-DEC-168`, `173` butir 5–6; `03-frontend-architecture.md` bagian 16.8 |
| Contract version | `ACC-API-0.18` `GET /general-ledger/balance-sheet` (`UnclassifiedAccountCount`, `ClassificationGaps`) — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | `BE-ACC-FIN-001-03` ✅, `FE-ACC-FIN-001-02` ✅, `FE-ACC-FIN-008-03` ✅ |
| Task mode | `FRONTEND` — perintah Rizki 26 November 2026 "lanjutkan" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `64ba2c07e`, working tree bersih sebelum task |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-6 terbukti 26 November 2026: uji layar Playwright agent terhadap `next dev` Rizki 9/9 (putaran pertama), ESLint 0 galat 0 peringatan pada 3 berkas tersentuh. `npm run build` `NOT RUN` (dijalankan Rizki). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Backend sudah mengirim daftar akun bersaldo yang klasifikasinya belum lengkap (`BE-ACC-FIN-001-03`), tetapi
layar Neraca belum menampilkannya. Pengguna tidak tahu bahwa data untuk rasio keuangan belum siap, dan tidak
punya jalan pintas ke akun yang perlu dilengkapi.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Neraca memberi peringatan bila ada akun bersaldo yang klasifikasinya belum lengkap, dan menautkan ke Daftar COA tersaring |
| Pelaku | Pemegang `GeneralLedger : Read` (melihat); `ChartOfAccount : Read` di layar tujuan |
| Pemicu | Membuka neraca suatu periode |
| Langkah | 1) Pilih periode. 2) Bila `UnclassifiedAccountCount > 0`, peringatan kuning tampil: jumlah, paling banyak lima kode + nama, sisanya "dan {n} akun lain". 3) Tombol **Lengkapi di Bagan Akun** membuka `/corporate/accounting/chart-of-accounts?klasifikasi=belum-lengkap` |
| Jalur tidak normal | Jumlah 0 → peringatan tidak tampil; cetak → peringatan tidak ikut tercetak |
| Hasil | Angka, tabel, ringkasan, dan CSV tidak berubah |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/general-ledger/balance-sheet-constants.jsx` | Empat bunyi peringatan di `BALANCE_SHEET_COPY`; `BALANCE_SHEET_CLASSIFICATION_GAP_PREVIEW = 5` |
| `src/lib/hooks/corporate/accounting/general-ledger/use-balance-sheet.jsx` | `classificationGap` (dibaca dari laporan, `null` bila 0); `openClassificationGaps` memakai `CHART_OF_ACCOUNT_CONFIG.routeBase` + `CLASSIFICATION_FILTER_QUERY` milik `FE-ACC-FIN-001-02`, tanpa string alamat tandingan |
| `src/components/view/corporate/accounting/general-ledger/balance-sheet-view.jsx` | Satu `InformationAlert` warning sesudah peringatan tidak seimbang, dibungkus `styles.screenOnly` yang sudah ada |

+69/−0, nol berkas baru, nol CSS baru, nol perubahan Redux/slice.

### 3.2 Delta terhadap desain

| Butir | Desain | Source | Alasan |
| --- | --- | --- | --- |
| Letak | "Di bawah peringatan yang ada (tahun belum ditutup, tidak seimbang)" | Sesudah peringatan tidak seimbang dan pembanding tidak seimbang, sebelum info "pembanding belum ada" | Info pembanding bukan peringatan; urutan desain terpenuhi |
| Cetak | "Mengikuti perlakuan peringatan lain di cetak yang sudah ada" | Peringatan lain **tetap** tercetak; peringatan ini disembunyikan | Kartu roadmap dan AC-5 meminta disembunyikan; desain bagian 16.8 menyatakan "tidak memuat peringatan ini". Kalimat "mengikuti perlakuan lain" tidak tepat — dicatat, tidak diubah |

## 4. Gerbang keputusan base component

`UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Kotak peringatan | `REUSE` `InformationAlert` `variant="warning"`, `closable={false}` | Peringatan tahun belum ditutup di layar yang sama |
| Tombol tautan | `REUSE` `BaseButton` `secondary` `sm` dalam `styles.alertBody` | Tombol Coba lagi di peringatan galat layar yang sama |
| Sembunyi saat cetak | `REUSE` kelas `styles.screenOnly` | Filter dan galat layar yang sama |
| Bunyi teks | `REUSE` pola konstanta `BALANCE_SHEET_COPY` | — |

Tidak ada elemen di luar `REUSE`, sehingga tidak ada pilihan yang menunggu keputusan.

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Layar Neraca yang sama (`FE-ACC-FIN-008-03`) |
| Grep diff: `style=` / warna literal / `rgb` / ukuran px / `<button>` / `<table>` / `<a ` | 0 |
| CSS baru | Tidak ada |
| Komentar baru | 0 |
| Aksesibilitas | Peringatan berjudul dan berteks; tombol berlabel kata kerja |

## 6. Verifikasi

| Pemeriksaan | Hasil | Status |
| --- | --- | --- |
| `npx eslint` 3 berkas tersentuh | 0 galat, 0 peringatan | `PASS` |
| `npm run build` | Dijalankan Rizki | `NOT RUN` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — kesepakatan kerja Rizki | — |
| Rute dirender `next dev` | Halaman neraca dan COA termuat tanpa galat (V1, V9) | `PASS` |

**MANUAL TEST** (Playwright msedge, layar sungguhan, `next dev` dan backend milik Rizki, 26 November 2026
10:29:32–10:30:08 WIB). Skrip: `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-001-03-20261126/03-uji-layar.mjs`;
keluaran `03-uji-layar.json`, PNG `V01`, `V03`, `V05`, `V07`, `V08`, `V09`, CSV `V04`, `V06`. PNG diperiksa sendiri.

| ID | Skenario | Hasil |
| --- | --- | --- |
| V1 | Neraca 2026-09: "Klasifikasi laporan belum lengkap: 4 akun bersaldo." + "1-1003 Kas Kecil, 1-1004 Bank Operasional, 1-3002 Persediaan Alat Kesehatan, 1002 Kas Besar." + kalimat rasio + tombol | PASS |
| V2 | Regresi: peringatan tahun 2019 belum ditutup, info pembanding 2025, total aset Rp 3.230.000 tetap | PASS |
| V3 | Tampilan cetak: peringatan klasifikasi tidak terlihat; peringatan tahun belum ditutup tetap tercetak | PASS |
| V5 | Sesudah 1-1000 diisi Lancar · Kas (lewat API) + Muat Ulang: "1 akun bersaldo", hanya 1-3002 | PASS |
| V6 | Isi tabel, ringkasan "Seimbang", dan CSV unduhan layar identik sebelum vs sesudah | PASS |
| V7 | Simulasi `route.fulfill` 7 akun: lima disebut + "dan 2 akun lain."; akun keenam tidak disebut | PASS (simulasi) |
| V8 | Simulasi `route.fulfill` jumlah 0: peringatan tidak tampil; peringatan lain tetap | PASS (simulasi) |
| V9 | Tombol membuka Daftar COA `?klasifikasi=belum-lengkap`, permintaan `unclassifiedOnly=true` `200`, saringan "Belum lengkap" terpilih | PASS |
| V10 | Pembersihan: klasifikasi 1-1000 dikosongkan kembali | PASS |

**Data dev.** Klasifikasi 1-1000 diisi sementara lalu dikosongkan, lewat `PUT` aplikasi, nol SQL — izin Rizki
26 November 2026. Keadaan akhir terverifikasi kosong (V10).

**Catatan tangkapan.** Pada PNG halaman penuh (`V01`, `V07`), footer aplikasi yang melayang menutupi sebagian
konten di bawah lipatan. Itu perilaku footer yang sudah ada, bukan akibat perubahan ini.

## 7. Acceptance criteria

| # | Kriteria | Source | Bukti |
| --- | --- | --- | --- |
| AC-1 | Peringatan dengan jumlah dan kode akun (`F38`) | `classificationGap` + view | V1 |
| AC-2 | Berkurang sesudah `F34`; tidak tampil bila 0 | `count === 0 → null` | V5, V8 (simulasi) |
| AC-3 | Tautan membuka Daftar COA tersaring | `openClassificationGaps` | V9 |
| AC-4 | Tabel, ringkasan, status, CSV tidak berubah | Hanya menambah elemen | V2, V6 |
| AC-5 | Tidak ada di tampilan cetak | `styles.screenOnly` | V3 |
| AC-6 | `npm run lint` nol galat pada berkas tersentuh | — | ESLint 0/0 |

**DoD.** AC-1..AC-6 terbukti; laporan; roadmap dan traceability diperbarui → ✅. `npm run build` milik Rizki.

## 8. Catatan penutup

| Butir | Isi |
| --- | --- |
| Wewenang UI yang dipakai | Kartu roadmap + desain bagian 16.8 |
| Risiko tersisa | Tidak ada yang diketahui. 1-3002 tetap disebut sampai 1-3000 diklasifikasi pemilik proses akuntansi |
| Dependency backend | `BE-ACC-FIN-001-03` ✅ |
| Task berikutnya | Roadmap finishing rev 2 tidak punya task siap lagi; sisa ⛔ `008-04`..`06` menunggu desain dan keputusan |
| Git | Tidak ada stage, commit, push, atau merge |
