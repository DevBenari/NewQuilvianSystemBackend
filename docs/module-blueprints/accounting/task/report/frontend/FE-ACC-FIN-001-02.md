# Laporan Perubahan Frontend — `FE-ACC-FIN-001-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-FIN-001-02` |
| Judul | Klasifikasi di form, rincian, dan daftar |
| Slice | Gelombang pengiriman `FIN-3` |
| Roadmap | `roadmap/frontend-roadmap-finishing.md` revisi 2, kartu `FE-ACC-FIN-001-02` |
| Trace | `FR-FIN-054`, `055`, `056`; `ACC-DEC-167`, `168`, `169`, `173`; `03-frontend-architecture.md` bagian 16.4–16.7, 16.10 |
| Contract version | `ACC-API-0.18` (`GET /chart-of-accounts` + `unclassifiedOnly`, `/{id}`, `/tree`, `POST`/`PUT` + `classification`) — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | `BE-ACC-FIN-001-02` ✅ (26 November 2026), `FE-ACC-FIN-001-01` ✅ |
| Task mode | `FRONTEND` — perintah Rizki 25–26 November 2026 "lanjut langkah berikutnya" |
| Target tulis | Repository `QuilvianSystemFrontendDev` branch `RizkiV2`; laporan ini dan baris status roadmap/traceability di repository backend |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `a2ddb52d2` + perubahan `FE-ACC-FIN-001-01` yang belum di-commit |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-9 terbukti 26 November 2026: uji layar Playwright agent terhadap `next dev` Rizki 11/11 (putaran ketiga; dua putaran awal gagal karena pemilih dan asersi skrip, bukan layar — bagian 6) + uji rincian terarah 1/1; ESLint 0 galat (5 peringatan lama). `npm run build` `NOT RUN` (dijalankan Rizki). UAT belum dijalankan — milik tim UAT |

## 1. Masalah yang diperbaiki

Backend kini menyimpan klasifikasi Lancar/Tidak Lancar dan sub-klasifikasi aset lancar beserta pewarisannya
(`BE-ACC-FIN-001-02`), tetapi layar Bagan Akun belum dapat mengisi, menampilkan, maupun menyaring akun yang
klasifikasinya belum lengkap.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Pemilik proses akuntansi mengisi klasifikasi di induk, melihat warisannya di turunan, dan menemukan akun yang belum lengkap |
| Pelaku | `ChartOfAccount : Read` (daftar, rincian); `Create`/`Update` (form) |
| Pemicu | Daftar COA, saringan Belum lengkap, atau tautan `?klasifikasi=belum-lengkap` (dipakai peringatan neraca `FE-ACC-FIN-001-03`) |
| Langkah | 1) Saring "Belum lengkap" atau buka akun. 2) Rincian menunjukkan klasifikasi efektif beserta asal warisannya. 3) Perbarui: pilih Klasifikasi Laporan dan Sub-klasifikasi, atau "Ikut induk: …". 4) Simpan |
| Jalur tidak normal | Sub-klasifikasi mati bila klasifikasi efektif bukan Lancar; pesan `409` backend (turunan bersub-klasifikasi) tampil apa adanya; akun Ekuitas/Pendapatan/Beban tidak menampilkan isian klasifikasi |
| Hasil | Klasifikasi tersimpan; daftar dan pohon menandai akun belum lengkap |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/chart-of-account/chart-of-account-constants.jsx` | + pilihan dan label klasifikasi/sub-klasifikasi, pilihan saringan, kunci alamat `klasifikasi=belum-lengkap`, lencana, teks; + dua isian form; + saringan `unclassifiedOnly`; + kolom Klasifikasi; + dua baris rincian |
| `src/utils/corporate/accounting/chart-of-account/chart-of-account-utils.jsx` | + `isClassifiableType`, `readClassification`, `describeClassificationValue`; `buildPayload`: tambah → `classification` hanya bila diisi, perbarui → selalu utuh (`null` = ikut induk) |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-editor.jsx` | `flattenAccountTree` membawa klasifikasi efektif induk; nilai milik akun dimuat dari rincian; isian Klasifikasi tersembunyi untuk Ekuitas/Pendapatan/Beban, Sub-klasifikasi hanya untuk Aset dan mati bila efektif bukan Lancar; label "Ikut induk: {nilai}" sebagai pilihan kosong; nilai yang tidak berlaku dikosongkan sebelum dikirim |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account-detail.jsx` | Baris "Klasifikasi Laporan" dan "Sub-klasifikasi Aset Lancar": nilai efektif, "(ikut {kode})" bila warisan, "Belum diisi", atau "Tidak berlaku" |
| `src/lib/hooks/corporate/accounting/chart-of-account/use-chart-of-account.jsx` | Saringan dibaca sekali dari `?klasifikasi=belum-lengkap`; `unclassifiedOnly` tidak dikirim bila badan hukum belum terpilih |
| `src/components/view/corporate/accounting/chart-of-account/chart-of-account-view.jsx` | Kolom Klasifikasi ("Lancar · Kas dan Setara Kas" / lencana Belum lengkap / "-"); lencana Belum lengkap di pohon; judul kosong "Seluruh akun sudah terklasifikasi." saat tersaring |
| `src/app/corporate/accounting/chart-of-accounts/page.jsx` | Dibungkus `Suspense` (pola `general-ledger/page.jsx`) karena `useSearchParams` |

Nol baris komentar baru; nol CSS baru; `globals.css` tidak disentuh.

### 3.2 Delta terhadap desain

| Hal | Desain | Implementasi | Alasan |
| --- | --- | --- | --- |
| Saringan tanpa badan hukum | Saringan mati | Saringan tetap dapat dipilih, tetapi `unclassifiedOnly` tidak dikirim bila badan hukum kosong | `FilterSelect` di `DataFilter` layar ini tidak punya keadaan mati per isian; badan hukum selalu terisi otomatis oleh `useAccountingLegalEntity`. Backend tetap menolak `400` bila dikirim tanpa badan hukum |
| Label saringan | "Belum lengkap" | "Belum lengkap" (sempat "Klasifikasi belum lengkap", terpotong di lebar saringan) | Muat utuh di kotak saringan |
| Warisan sub-klasifikasi di label "Ikut induk" | Dari pratinjau/rincian induk | Dari klasifikasi efektif induk di `GET /tree` | Satu sumber untuk tambah dan perbarui; tanpa permintaan tambahan |

## 4. Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 7, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Isian Klasifikasi dan Sub-klasifikasi | `BaseEditorField` select (`hidden`, `disabled`, `placeholder` sebagai pilihan kosong) | Form COA yang sama; `getVisibleFields` menghormati `hidden` | REUSE |
| Baris rincian | `BaseDetailView` `detailRows` | Rincian COA | REUSE |
| Saringan Belum lengkap | `FilterSelect` di `DataFilter` | Saringan Status/Transaksi | REUSE |
| Kolom Klasifikasi | `DataTable` kolom + teks | Daftar COA | REUSE |
| Lencana Belum lengkap (tabel, pohon) | `StatusBadge` nada `warning` | Lencana Control/Status | REUSE |
| Pesan `409` | `InformationAlert` bawaan `BaseEditorView` (galat aksi) | Form yang sama | REUSE |
| Keadaan kosong tersaring | `DataTable` `emptyTitle` | Daftar COA | REUSE |

## 5. Checklist konsistensi UI

| Butir | Hasil |
| --- | --- |
| Modul referensi visual | Layar COA yang sama |
| Grep diff: warna literal / ukuran px / `<button>` / `<table>` / `fw-` `fs-` / `!important` / `style={` | 0 |
| CSS baru | Tidak ada |
| Aksesibilitas | Lencana berteks; alasan sub-klasifikasi mati ditulis sebagai keterangan isian |
| Aksi halaman rincian | Tetap Kembali dan Perbarui (`AGENTS.md`), terbukti L2 |

## 6. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas tersentuh | 0 galat; 5 peringatan `react-hooks/set-state-in-effect` seluruhnya pada efek lama (`setRouteError`, `setForm`, `setTreeItems`) | `PASS` |
| Pemindaian komentar baru | 0 | `PASS` |
| `fin-001-02-20261125/03-uji-layar.mjs` L1–L11 putaran 3 (26 November 2026 09.44–09.47 WIB, `next dev` Rizki, backend DLL 09:30:58) | 11/11 | `PASS` |
| `fin-001-02-20261125/04-uji-rincian-l7.mjs` L7b | 1/1 | `PASS` |
| `npm run build` | Dijalankan Rizki | `NOT RUN` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — kesepakatan kerja Rizki | — |

**Riwayat putaran (dicatat apa adanya).** Putaran 1: 8/11 — skrip memilih opsi "Ikut induk: Kas dan Setara Kas" alih-alih "Kas dan Setara Kas" karena pencocokan teks tidak persis, sehingga `null` terkirim (L5), dan dua uji berikutnya gagal berantai (L6, L7). Layar mengirim persis pilihan yang diklik. Pemilih diperbaiki ke pencocokan persis. Putaran 2: 10/11 — L7 gagal hanya pada asersi teks ("Klasifikasi Laporan Tidak Lancar" dengan spasi), padahal label dan nilai berada di elemen terpisah; dibuktikan uji terarah L7b dan tangkapan `L07-rincian-1-5001.png`. Putaran 3 sesudah asersi diperbaiki: 11/11.

**MANUAL TEST** (Playwright msedge, layar sungguhan):

| Uji | Isi |
| --- | --- |
| L1 | `?klasifikasi=belum-lengkap` → permintaan `unclassifiedOnly=true`, saringan terpilih "Belum lengkap", 17 akun berlencana, kolom Klasifikasi tampil (`L01-belum-lengkap.png`) |
| L2 | Rincian 1-5000: "Belum diisi" / "Tidak berlaku"; tombol hanya Kembali, Perbarui |
| L3 | Perbarui 1-5000: "Ikut induk: belum diisi"; sub-klasifikasi mati sampai Lancar dipilih; `PUT` `{1,1}` `200` |
| L4 | Rincian 9901.01: "Lancar (ikut 1-5000)", "Kas dan Setara Kas (ikut 1-5000)" |
| L5 | Perbarui 9901.01: "Ikut induk: Lancar" / "Ikut induk: Kas dan Setara Kas"; simpan sub-klasifikasi sendiri `PUT` `{null,1}` `200` |
| L6 | 1-5000 → Tidak Lancar: sub-klasifikasi mati, dikirim `null`; `409` "Klasifikasi tidak dapat diubah karena turunan 9901.01 …" tampil (`L06-tolak-409.png`) |
| L7 | 1-5001 → Tidak Lancar: rincian "Tidak Lancar" / "Tidak berlaku" |
| L8 | 1-5001 "Ikut induk" → `PUT` `{null,null}`; rincian "Lancar (ikut 1-5000)" |
| L9 | Tanpa simpan: 4-1002 (Pendapatan) tanpa isian klasifikasi; 2-1001 (Liabilitas) hanya Klasifikasi |
| L10 | Tambah di bawah 1-1000, tanpa simpan: "Ikut induk: belum diisi", sub-klasifikasi mati (`L10-tambah.png`) |
| L11 | Pohon: 20 simpul berlencana Belum lengkap (`L11-pohon.png`) |

**Data dev.** Penulisan hanya pada akun uji nonaktif 1-5000, 1-5001, 9901.01; klasifikasinya dikosongkan kembali lewat API di
akhir setiap putaran (tercatat `bersih` di `03-uji-layar.json`). Akun asli hanya dibuka tanpa disimpan. Nol SQL.

## 7. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 1-5000 Lancar + Kas → Rincian 1-1004/9901.01 "(ikut …)" (`F34`) | Terbukti (akun uji 9901.01 sebagai turunan) | L3, L4 |
| AC-2 Isian tersembunyi untuk Ekuitas/Pendapatan/Beban; sub-klasifikasi tersembunyi untuk Liabilitas dan mati bila tidak Lancar | Terbukti | L3, L6, L9, L10 |
| AC-3 Pesan `409` `VAL-COA-11` tampil apa adanya (`F36`) | Terbukti | L6 |
| AC-4 Tidak Lancar → sub-klasifikasi "Tidak berlaku" (`F37`) | Terbukti | L7, L7b |
| AC-5 "Ikut induk" mengosongkan nilai milik akun | Terbukti | L8 |
| AC-6 `?klasifikasi=belum-lengkap` menyaring; kosong → "Seluruh akun sudah terklasifikasi." | Terbukti (saringan); teks kosong `SOURCE VERIFIED` (DB dev selalu punya akun belum lengkap) | L1 |
| AC-7 Kolom dan lencana pohon cocok dengan `classificationGap` | Terbukti | L1, L11 |
| AC-8 Rincian hanya Kembali dan Perbarui | Terbukti | L2 |
| AC-9 Lint nol galat | Terbukti | Bagian 6 |

**DoD.** AC-1..AC-9 terbukti; laporan; roadmap dan traceability diperbarui → ✅. `npm run build` milik Rizki.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Rendah. Label "Ikut induk" memakai klasifikasi efektif induk langsung; bila induk menimpa menjadi Tidak Lancar sedangkan akun ini memilih Lancar sendiri, label sub-klasifikasi warisan dapat berbeda dari hitungan backend yang menelusuri leluhur lebih jauh — nilai efektif sesudah simpan selalu dibaca dari backend |
| Status Git frontend | `M` tujuh berkas di bagian 3.1 (ditambah empat berkas `FE-ACC-FIN-001-01` yang belum di-commit) |
| Langkah berikutnya | `BE-ACC-FIN-001-03` (peringatan klasifikasi di neraca) |
