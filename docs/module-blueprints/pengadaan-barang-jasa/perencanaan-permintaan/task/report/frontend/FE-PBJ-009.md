# FE-PBJ-009 — Standar baris filter dan hero

| Field | Nilai |
| --- | --- |
| Task | `FE-PBJ-009` (roadmap frontend revisi 2) |
| Trace | `PBJ-FE-004`, `PBJ-DEC-056`, `PBJ-DEC-057`, `PBJ-DEC-058`; `03-frontend-architecture.md` §4.7; `PBJ-PP-API` 1.4; `AT-PP-66` |
| Dependency | `FE-PBJ-005` ✅, `FE-PBJ-007` ✅, `FE-PBJ-008` ✅, `BE-PBJ-012` ✅ |
| Branch / SHA awal | `sukmagpV2` @`059b6c612` |
| Tanggal | 17 November 2026 |
| Wewenang UI | `PBJ-FE-004` (`approved`, pengguna sesi); rupa lain `PBJ-FE-002` (`DEV_DISCRETION`) |
| Status | ✅ **SELESAI.** Uji layar 81/81 `PASS` (15 layar × 1440 px dan 1920 px + interaksi), `npm run lint:errors` dan `npm run build` exit 0 |

## Apa yang berubah bagi pengguna

Semua layar daftar Pengadaan kini punya **satu baris filter** dengan urutan sama seperti modul lain. **Contoh pada Daftar
PR:** Tanggal Awal · Tanggal Akhir · Periode · Unit · Tahun · Status · Cari nomor PR · Jumlah baris · ↻. Tombol "+ Buat
PR" pindah ke pojok kanan kotak filter dengan warna primary. Hero hanya berisi judul dan keterangan.

Perilaku kontrol:

1. Memilih Periode selain "Custom" (mis. "Bulan ini") mengosongkan Tanggal Awal dan Tanggal Akhir. Server mengabaikan
   tanggal pada periode itu.
2. Mengisi Tanggal Awal atau Tanggal Akhir otomatis mengubah Periode menjadi "Custom", supaya tanggal tidak diabaikan
   diam-diam. Tanggal Akhir tidak bisa dipilih sebelum Tanggal Awal (`min`/`max` pada `FilterDatePicker`).
3. Jumlah baris (10/25/50/100) mengubah ukuran halaman dan kembali ke halaman 1.
4. ↻ mengembalikan semua saringan ke bawaan layar.

## Layar dan saringan

| Layar | Saringan relevan (`PBJ-DEC-056`) | Status | Tombol di filter |
| --- | --- | --- | --- |
| Daftar PR, Riwayat | Unit, Tahun | Status PR | "+ Buat PR" (Daftar PR) |
| Mendesak | Unit, Tahun | Status PR | — |
| Menunggu Persetujuan, Verifikasi, Validasi | Unit, Tahun | Status terkunci, tampil nonaktif ("Menunggu Persetujuan"/"Menunggu Verifikasi") | — |
| Pemeriksaan Stock | Unit, Sudah/belum dicek | Status PR | — |
| Rencana Kebutuhan, Rencana Anggaran | Unit, Tahun | Status rencana | "+ Buat Rencana" / "+ Buat Pagu" |
| Rencana Paket | Jenis, Tahun | Status rencana | "+ Buat dari Konsolidasi" |
| Revisi Perencanaan | Unit, Tahun | Status rencana | — |
| Katalog | Jenis, Kategori | Aktif/Nonaktif | "+ Tambah Barang/Jasa" |
| Kategori, Sumber Dana, Jenis (tab) | — | Aktif/Nonaktif | "+ Tambah …" (kecuali Jenis, baca saja) |
| Antrean Persetujuan | Jenis dokumen | Tampilan (Perlu tindakan/Sudah diproses/Semua) | "Kotak Masuk Terpadu" — dipindah dari hero |
| Konsolidasi (pengecualian `PBJ-DEC-057`) | Tahun, Kategori, Jenis | — | "Buat rencana paket dari pilihan" |

Saringan yang dilepas sesuai `PBJ-DEC-056`: urgensi (Daftar PR; layar Mendesak tetap ada), kategori, sumber dana, dan
"versi lama" (Rencana Anggaran; versi lama tetap terbaca di Revisi Perencanaan).

## Gerbang keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kotak filter, pencarian, ↻, tombol di header | `REUSE` | `DataFilter` (`actions`, `onSearchChange`, `onReset`) |
| Tanggal Awal/Akhir | `REUSE` | `FilterDatePicker` (`min`/`max` opt-in) |
| Periode, saringan, Status, Jumlah baris | `REUSE` | `FilterSelect` (`icon="☷"` seperti Master Data Supplier) |
| Tombol aksi | `REUSE` | `BaseButton variant="primary"` |
| Hero | `REUSE` | `Hero` tanpa `actions` |
| Rangkaian Tanggal/Periode/Jumlah baris yang dipakai 10 layar | `COMPOSE` | `shared/procurement-list-filter.jsx` mengembalikan elemen base component apa adanya (bukan komponen pembungkus), supaya `DataFilter` tetap mengenali date picker |
| Satu baris di desktop | `COMPOSE` | Kelas fitur `filterRow` lewat prop resmi `controlsClassName`; hanya `flex-wrap`/`flex` di ≥1181 px (titik patah bawaan `DataFilter`) |

Pilihan untuk dua elemen `COMPOSE`:

1. **Helper fitur + `controlsClassName` (dipakai, direkomendasikan).** Tidak mengubah base component; risiko regresi
   modul lain nol. Biaya kecil: satu berkas helper dan satu blok CSS fitur.
2. Menambah prop "satu baris" ke `DataFilter`. Konsisten lintas modul, tetapi mengubah base component yang dipakai ratusan
   layar, sehingga butuh keputusan pemilik base component.
3. Menyalin kontrol ke setiap layar tanpa helper. Tanpa abstraksi, tetapi 10 salinan logika Periode↔tanggal yang mudah
   menyimpang.

`UI GATE: PASS` — tidak ada elemen `NEW`; tidak ada `EXTEND` yang mengubah perilaku bawaan base component.

## Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/procurement/shared/procurement-list-filter.jsx` (baru) | `periodFilterControls`, `pageSizeControl` |
| `src/lib/constants/corporate/procurement/procurement-constants.jsx` | `PAGE_SIZE_OPTIONS`, `PERIOD_OPTIONS`, `PERIOD_FILTER_DEFAULTS` |
| `src/lib/hooks/corporate/procurement/use-procurement-document-list.jsx` | `updateFilters(patch)` untuk perubahan beberapa saringan sekaligus |
| `src/lib/hooks/corporate/procurement/use-procurement-master-data.jsx` | Saringan tanggal bawaan; tab Jenis memakai `GET /` berhalaman (bukan `/options`) |
| `src/style/corporate/procurement/procurement.module.css` | `.filterRow` (≥1181 px: satu baris) |
| `requests/purchase-request-list-view.jsx`, `verification/stock-check-list-view.jsx`, `planning/plan-document-list-view.jsx`, `planning/planning-revision-view.jsx`, `settings/procurement-item-view.jsx`, `settings/procurement-reference-view.jsx`, `approvals/procurement-approval-inbox-view.jsx`, `planning/plan-consolidation-view.jsx` | Baris filter standar, `controlsClassName={styles.filterRow}`, tombol `variant="primary"`; Antrean Persetujuan: tombol keluar dari hero, Periode → `period` `Wfl` |

Halaman form/detail dokumen (PR, rencana, pagu, paket) tidak diubah: tombol hero di sana sah menurut `PBJ-FE-004`.

## Verifikasi

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` berkas Pengadaan | 0 error, 9 warning `react-hooks/set-state-in-effect` (pola muat-data yang sudah ada) | `PASS` |
| `npm run lint:errors` | exit 0 | `PASS` |
| `npm run build` | `Compiled successfully`, exit 0; 24 rute `/corporate/procurement/**` | `PASS` |
| Grep anti-regresi `ui-consistency-checklist` pada kode baru | Tidak ada warna literal, tipografi, `!important`, `<button>` mentah, `<table>`, atau kelas bootstrap | `PASS` |
| Uji layar Playwright `ui_fe_pbj_009.cjs` (SuperAdmin, dev server 3000 → backend uji 7185) | 81/81, rincian di bawah | `PASS` |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — perubahan susunan dan sambungan saringan; tidak ada logika murni baru di luar helper kecil | — |

| Skenario | Hasil |
| --- | --- |
| 15 layar × 2 lebar: semua kontrol filter pada satu baris dan bilah filter tidak meluap | 30/30 |
| 15 layar × 2 lebar: hero tanpa tombol | 30/30 |
| 8 layar bertombol × 2 lebar: tombol aksi berkelas `primary` | 16/16 |
| Daftar PR: Periode "Bulan ini" → request `customPeriod=thismonth` | `PASS` |
| Daftar PR: Jumlah baris 10 → `pageSize=10`, periode tetap terkirim | `PASS` |
| Daftar PR: ↻ → request tanpa `customPeriod`, `pageSize=25` | `PASS` |
| Antrean Persetujuan: tanpa Periode → `period=all`; "Hari ini" → `period=today` | `PASS` (2) |

`MANUAL TEST`: pengisian tanggal lewat kalender tidak diklik di uji otomatis. Logikanya (tanggal → Periode "Custom")
ada di helper bersama, dan saringan tanggal di server sudah terbukti di `BE-PBJ-012` (`AT-PP-65h`, `65i`).

## Risiko dan catatan

| Butir | Penjelasan |
| --- | --- |
| Label terpotong di 1440 px | Delapan kontrol berbagi ±1.080 px, sehingga label tampil terpotong ("Tanggal …", "Semua p…"). Di 1920 px semua label utuh. Ini konsekuensi aturan satu baris; nilai terpilih tetap terbaca saat menu dibuka |
| Layar sempit (<1181 px) | Baris kembali melipat seperti bawaan `DataFilter`, supaya tidak ada gulir horizontal |
| Saringan dilepas | Urgensi, kategori, sumber dana, dan versi lama tidak lagi di baris filter (`PBJ-DEC-056`) |
| Masalah yang diketahui | `NONE` |
