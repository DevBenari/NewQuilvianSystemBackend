# Laporan Perubahan Frontend — `FE-RAD-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-18` |
| Judul | Fitur v1 bertanda yang dibangun: laporan operasional, Tarif Tindakan, Template Hasil, Ubah Pemeriksaan, Cetak Film |
| Slice | `RAD-EVD-001` bagian 7.4 |
| Trace | `RAD-DEC-022`; backend `BE-RAD-24`, `BE-RAD-25`, `BE-RAD-26` |
| Contract | `RAD-API-001` revision 17 |
| Task mode | `FRONTEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `478f08aa9`, branch `YogaV2` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** API tiruan 86/86; uji unit Radiologi 217/217. Terhadap devYoga menunggu migration |

---

## 1. Yang dipasang

| Layar | Isi |
| --- | --- |
| `reports/[report]` | Lima laporan v1 (Statistik Pemeriksaan, Dokter Pengirim, Pasien Perusahaan, Pemeriksaan Harian, Pemeriksaan Per Item): rentang tanggal WIB (kosong = 30 hari, rentang dari server ditampilkan), kartu ringkas, tabel, baris jumlah, Unduh CSV (ber-BOM), Cetak |
| `setup/tarif-tindakan` | Pemeriksaan radiologi beserta tarifnya; "Belum diatur" bila tanpa tarif. Baca-saja |
| `setup/template-hasil` | Tambah, ubah, hapus template (`RadReport : Validate`) |
| Detail Pasien | Kolom Cetak Film menjadi centang (optimistis, dikembalikan bila server menolak); aksi Ubah Pemeriksaan membuka modal pemeriksaan pengganti + alasan wajib, nonaktif bila study sudah ada |
| Draf bacaan | Pemilih "Pakai template hasil" — hanya mengisi Temuan/Kesan/Saran yang masih kosong, tulisan petugas tidak tertimpa; daftar template hanya hidup di state layar |

Menu samping Laporan/Pemeriksaan/Setup kini menuju halaman sungguhan; Setup → Kategori dan Parameter tetap bertanda
"Fitur belum tersedia". Daftar penanda (`rad-pending-feature-constants.jsx`) tinggal Kategori, Parameter, Pasien Luar,
PACS, AI.

## 2. Berkas

- Baru: `rad-operational-report-rules.js`, `use-rad-operational-report.jsx`, `rad-operational-report-view.jsx`,
  `rad-procedure-tariff-view.jsx`, `rad-report-template-rules.js`, `rad-report-template-view.jsx`, tiga rute halaman,
  `tests/unit/rad-operational-report-rules.test.mjs`, `tests/unit/rad-report-template-rules.test.mjs`.
- Diubah: `use-rad-patient-detail.jsx`, `rad-patient-detail-view.jsx`, `rad-patient-detail.module.css`,
  `use-rad-report-draft.jsx`, `rad-report-draft-view.jsx`, `rad-order.service.js`, `rad-report.service.js`,
  `rad-pending-feature-constants.jsx`, `menu-items.jsx`.

## 3. Verifikasi

| Bukti | Hasil |
| --- | --- |
| Uji unit Radiologi | **217/217** — 9 baru (laporan: daftar, tanggal tanpa geser zona, jumlah, kelompok perusahaan, CSV; template: validasi, payload, bentuk form, terapkan hanya ruas kosong). Penjaga penyimpanan bacaan tetap lulus |
| ESLint berkas Radiologi | 0 galat; peringatan `set-state-in-effect` yang tersisa sudah ada sebelumnya |
| `npm run build` | Lulus; rute `reports/[report]`, `setup/tarif-tindakan`, `setup/template-hasil` terbit |
| Layar terhadap API tiruan | **86/86** — Cetak Film mengirim `PUT /film-print { printFilm: true }` dan centang bertahan; Ubah Pemeriksaan nonaktif bila study ada; tanpa pilihan/alasan ditolak di layar tanpa permintaan; `PUT /procedure` membawa pemeriksaan + alasan; Statistik menampilkan rentang server dan jumlah; Tarif dan Template tampil; Kategori tetap bertanda |

Satu cacat ditemukan uji layar dan diperbaiki: centang Cetak Film baru berubah setelah server menjawab, sehingga
klik terasa tidak berpengaruh — kini optimistis.

## 4. Batas verifikasi

Terhadap devYoga menunggu migration `AddRadReportTemplateAndFilmPrint`. Pemilih template di draf bacaan belum diuji
di layar (butuh study dengan bacaan yang masih dapat disunting); logikanya teruji unit.
