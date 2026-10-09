# Laporan Perubahan Frontend — `FE-LAB-53`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-53` |
| Judul | Pemilih tahun, tiga grafik, dan kartu *Jenis laboratorium* |
| Slice | Gelombang `MVP-13b` — `EPIC-LAB-19` Beranda Lab (BR-140) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Gelombang `MVP-13` — `EPIC-LAB-19`* |
| Trace | `FR-19.3` (*Jenis laboratorium*), `FR-19.4`, `FR-19.5`, `FR-19.7` (isolasi galat bagian tahunan); `LAB-DEC-206`, `LAB-DEC-207`, `LAB-FE-035`; `LAB-REQ-021` butir 7, 8 |
| Contract version | `LAB-API-v1` **`r44`** 39.4, 39.8; `LAB-VAL-v1` **`r20`** (`VAL-154`) — `approved` 2026-10-08 (`LAB-REQ-021`) |
| Wewenang UI | Isi dan letak bagian 4–6 serta satu pemilih tahun: keputusan produk (`LAB-DEC-206`, `LAB-FE-035`). Jenis grafik (batang, donat, garis), warna, lima pilihan tahun, dan nama berkas: `DEV_DISCRETION` dalam tema V2 |
| Dependency | `FE-LAB-52` ⚠ (working tree, berkas yang sama); `BE-LAB-93` ⚠ (working tree backend, belum di-commit). Biner lokal memuat `BE-LAB-93` |
| Klasifikasi | `MEDIUM` — 6 berkas source diubah, 1 berkas grafik baru; uji unit diperluas. Nol route, menu, maupun izin baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap, traceability, dan manifest |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `9bd8b96fe` (branch `YogaV2`) + working tree `FE-LAB-52` |
| Commit backend yang dijadikan rujukan | `38c8a4f4` (branch `yoga`) + working tree `BE-LAB-92`/`BE-LAB-93` |
| Tanggal | 2026-10-09 |
| Status | ✅ **`SELESAI`** — 20/20 skenario peramban lolos dengan superadmin; **Uji ulang 2026-10-09 dengan akun analis asli** (Gilang; FE-LAB-53 juga Vina), tanpa mengganti jawaban sukses, nol tulis: **20/20 untuk Gilang dan 20/20 untuk Vina**. **Catatan sisa, bukan penahan:** (1) `AC-301` lewat `403` tiruan karena tidak ada akun dev tanpa `LabOrder : Read`; (2) *Belum tergolong* bila > 0 terbukti lewat uji unit (DB dev: 0) |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| Slice | `lab-dashboard-slice.jsx` dari `FE-LAB-52` sudah menyediakan bagian `yearly` dan `selectedYear` |
| Kontrak | `dashboard/yearly` selalu mengembalikan tiga disiplin dan 12 bulan; tahun di luar 2000..tahun berjalan → `422` (`VAL-154`) |
| Pola grafik | `inpatient-dashboard-widgets.jsx`: `react-apexcharts` lewat `next/dynamic` dengan `ssr: false` |
| Data dev 2026 | Pemeriksaan: PK 8, PA 5, Mikro 5 (3 disiplin aktif). Pesanan: September 17, Oktober 5. Tahun 2025 seluruhnya 0 |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** petugas laboratorium pemegang `LabOrder : Read`.

1. Di bawah *Fokus Operasional*, petugas melihat **Ringkasan Tahunan** untuk tahun berjalan.
2. Isinya:
   - pemilih **Tahun**;
   - grafik batang *Jumlah Pemeriksaan per Disiplin*;
   - donat *Sebaran Pemeriksaan* (angka dan persen pada legenda);
   - grafik garis *Tren Pesanan Bulanan* tahun terpilih dibanding tahun sebelumnya, beserta total keduanya.
3. Kartu ketiga di *Fokus Operasional* adalah **Jenis Laboratorium**, misalnya *"3 dari 3 — Aktif pada tahun 2026"*.
4. Petugas memilih tahun lain (tahun berjalan dan empat tahun sebelumnya). Hanya ringkasan tahunan dan kartu
   *Jenis Laboratorium* yang dimuat ulang; kartu hari ini, tabel terbaru, dan rekap tidak dimuat ulang.

**Contoh.** Petugas memilih 2025:
- Kedua grafik disiplin berisi *"Belum ada pemeriksaan pada tahun 2025."*
- Grafik tren berisi *"Belum ada pesanan pada tahun 2025 maupun 2024."*
- Kartu menjadi *"0 dari 3 — Aktif pada tahun 2025"*.

**Jalur tidak normal.**

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Ringkasan tahunan gagal (`500`, jaringan) | Bagian tahunan: *"Ringkasan tahunan gagal dimuat"* + *Coba lagi*. Kartu *Jenis Laboratorium* tampil `-` dengan *"Ringkasan tahunan gagal dimuat"*. Bagian lain tetap berisi |
| Ringkasan hari ini gagal | Kartu *Jenis Laboratorium* tetap tampil di deret fokus (sumbernya tahunan) |
| `422` `VAL-154` | Tidak terjadi lewat pemilih. Bila terjadi, pesan backend tampil di bagian tahunan |
| `401`/`403` | Seluruh halaman → *Akses Ditolak* |
| Ada pemeriksaan tanpa disiplin | Batang dan donat menambah butir *Belum tergolong*, hanya bila > 0 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap `FE-LAB-53` dan `r44` 39.4, 39.8.
- `03-frontend-architecture.md` amandemen 2026-10-08 (susunan, penanganan keadaan, aksesibilitas).
- `validation-matrix.md` `VAL-154`.
- `inpatient-dashboard-widgets.jsx`, `filter-select.jsx` (`onChange(value, option)`, `role="option"`),
  `base-detail-section.jsx`, dan `laboratory-state-panel.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/.../lab-order.service.js` | `getLabDashboardYearly({ year })` — `year` dikirim sebagai query bila diisi |
| `src/lib/state/slice/.../lab-dashboard-slice.jsx` | Thunk `fetchLabDashboardYearly` memakai penjaga `requestId` yang sama dengan bagian lain. Reducer `setLabDashboardSelectedYear` mengosongkan nilai yang bukan bilangan bulat |
| `src/lib/hooks/.../laboratory-dashboard-rules.js` | Fungsi murni baru: `LAB_DISCIPLINE_ORDER`, `buildActiveDisciplineCard`, `currentWibYear`, `buildYearOptions`, `resolveDashboardYear`, `buildDisciplineItems`, `withShare`, `buildMonthlyTrend`, dan `buildYearlyEmptyText`. `buildFocusCards` menerima kartu *Jenis Laboratorium* di posisi ketiga |
| `src/lib/hooks/.../use-laboratory-overview.jsx` | Sumber keempat `yearly`. Tahun dihitung tiap render: pilihan sah, atau tahun berjalan WIB. Efek terpisah `[dispatch, year]` memuat `yearly` saja. *Perbarui Data*, data basi, waktu muat, dan pendeteksi `403` mencakup keempat sumber. Ditambah `retryYearly` dan `selectYear` |
| `src/lib/constants/.../laboratory-constants.jsx` | Teks bagian tahunan; deskripsi hero menyebut ringkasan tahunan |
| `src/components/view/.../laboratory-overview-charts.jsx` | **Baru.** `LabDisciplineBarChart`, `LabDisciplineDonutChart`, dan `LabMonthlyTrendChart` lewat `next/dynamic` tanpa SSR. Tiap grafik diberi judul teks dan legenda berangka. Seri tren dibedakan oleh garis utuh/putus-putus dan penanda lingkaran/kotak, bukan warna saja. Tabel bulanan tersembunyi disediakan untuk pembaca layar |
| `src/components/view/.../laboratory-overview-view.jsx` | Bagian *Ringkasan Tahunan* berisi satu pemilih (`<label for>` + `FilterSelect`). `LaboratoryStatePanel` milik bagian ini. Deret fokus memakai kartu *Jenis Laboratorium* |
| `src/style/.../laboratory-overview.module.css` | Toolbar tahun, grid grafik (`auto-fit`, satu kolom di layar sempit), kartu grafik, legenda, keadaan kosong/memuat, `srOnly`. Hanya token tema |
| `tests/unit/laboratory-dashboard-rules.test.mjs` | **+11 uji** (total 22): pilihan tahun dan batas 2000, tahun tak sah, pergantian tahun WIB, pemetaan disiplin lewat nama, *Belum tergolong*, persen tanpa `NaN`, 12 bulan dua seri, kartu *Jenis Laboratorium*, urutan fokus, teks kosong, isolasi galat slice, dan `selectedYear` |

### 3.3 Kepatuhan arsitektur frontend

- Satu slice Beranda, satu hook. Grafik dipisah ke berkas sendiri sesuai Cakupan (6).
- ApexCharts tidak dirender di server (jebakan (a)): nol galat `window`.
- Disiplin dipetakan lewat nama enum, bukan urutan larik (jebakan (b)). Uji unit memakai larik acak.
- Mengganti tahun tidak memuat ulang halaman (jebakan (c)), dibuktikan lewat pencatat permintaan.
- Warna bukan satu-satunya pembeda (jebakan (d)).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Memuat data grafik..."* per grafik. Pemilih tahun redup. Kartu *Jenis Laboratorium*: `-` *"Memuat..."* pada muat pertama |
| Kosong | *"Belum ada pemeriksaan pada tahun …"* / *"Belum ada pesanan pada tahun … maupun …"* |
| Gagal | Panel galat bagian tahunan + *Coba lagi*; kartu *Jenis Laboratorium* ikut bergalat |
| Basi | Ikut aturan 60 detik bersama tiga sumber lain |
| Tanpa hak akses | *Ups! Akses Ditolak* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/dashboard/yearly?year={tahun}` | Tiga grafik dan kartu *Jenis Laboratorium* | `LabOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas yang disentuh | 0 error, 0 warning | `PASS` | Exit 0 |
| Uji unit Beranda | 22/22 | `PASS` | |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2544 uji: 2536 lulus, 8 gagal | `PASS` (nol kegagalan baru) | Kedelapan nama identik dengan baseline |
| `npm run build` | `Compiled successfully in 51s` | `PASS` | Server BE/FE dimatikan sebelum build; tiga proses `next dev` sisa dihentikan dulu |
| `AC-294` — sembilan bagian | Hari Ini → Fokus → Ringkasan Tahunan (batang → donat → tren) → Pesanan Terbaru → Kesiapan → Rekap | `PASS` | Posisi teks berurutan naik |
| `AC-294` — kartu fokus | Total → Menunggu Validasi → **Jenis Laboratorium** → Tingkat Penyelesaian | `PASS` | |
| Bawaan tahun | Permintaan pertama `year=2026` | `PASS` | Pencatat permintaan |
| `FR-19.3` | *"3 dari 3 — Aktif pada tahun 2026"* = `activeDisciplineCount` | `PASS` | Acuan lewat jalur terpisah |
| `AC-297` — 2026 | Legenda batang dan donat: PK 8, PA 5, Mikro 5 (44% / 28% / 28%), total 18 | `PASS` | Sama dengan `dashboard/yearly?year=2026` |
| *Belum tergolong* | Tidak tampil (DB: 0) | `PASS` | Kemunculan bila > 0: uji unit |
| `AC-298` | Dua seri tergambar; tabel 12 baris; *"Total 2026: 22 pesanan · Total 2025: 0 pesanan"* | `PASS` | |
| `VAL-154` | Pilihan 2026, 2025, 2024, 2023, 2022 — tanpa tahun depan | `PASS` | |
| `AC-297` — ganti ke 2025 | Hanya `dashboard/yearly?year=2025` +1; `today`, `recent-orders`, `summary` tetap | `PASS` | Pencatat permintaan |
| `AC-297` — sesudah ganti | Kartu *"0 dari 3 — Aktif pada tahun 2025"*; *"Pesanan tahun 2025 dibanding 2024"*; keadaan kosong menyebut 2025 | `PASS` | Tangkapan layar |
| `AC-300` — *Perbarui Data* | Keempat sumber +1; `yearly` memakai tahun terpilih (2025) | `PASS` | |
| `AC-300` — galat tahunan | `yearly` dicegat `500` → hanya bagian tahunan dan kartu *Jenis Laboratorium* bergalat; kartu hari ini dan 10 baris terbaru tetap | `PASS` | Satu endpoint dicegat (predikat memuat `/v1/`) |
| `AC-301` | `yearly` dicegat `403` → *Akses Ditolak* | `PASS` (tiruan) | |
| Galat runtime | Nol (`window`, ApexCharts) | `PASS` | `pageerror` |
| Penjaga tulis | Nol permintaan non-GET | `PASS` | |
| Ulang dengan akun analis asli Gilang dan Vina (2026-10-09) | 20/20 masing-masing; bagian tahunan 2025 termuat 639 ms dan 631 ms | `PASS` | Login lewat formulir, sandi dari berkas sementara yang sudah dihapus; nol jawaban sukses diganti |

**Temuan lingkungan.** Pada percobaan pertama, satu permintaan `yearly?year=2025` dari peramban butuh **8.065 ms**
(log Serilog `Elapsed`), sedangkan permintaan lain 140–600 ms. Pada percobaan ulang, tahun yang sama termuat dalam
636 ms. Layar menahannya dengan benar: grafik tetap *Memuat* dan pemilih redup sampai jawaban tiba. Lonjakan ini cocok
dengan DB dev remote bersama dan catatan `BE-LAB-93` bahwa kueri tahunan berjalan tanpa index. Lonjakan semacam ini
perlu dipantau sebelum rilis, tetapi tidak menahan task ini.

**Lingkungan.**

- Backend lokal Development `https://localhost:7184`.
- `next dev` port 3000 diarahkan ke backend lokal; `SESSION_SIGNING_SECRET` lewat env proses.
- DB dev bersama, superadmin.
- Kedua server dinyalakan ulang sesudah build dan dibiarkan hidup.

Uji manual: `PASS` — superadmin dan dua akun analis asli.

AUTOMATED TEST: PASS (11 uji baru; nol kegagalan baru).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-294` bagian 4–6 di tempatnya; sembilan bagian lengkap | Terpenuhi | Bagian 6 |
| `AC-297` ganti tahun mengubah ketiga grafik dan kartu; kartu hari ini dan tabel tidak dimuat ulang | Terpenuhi | Bagian 6 |
| `AC-298` 12 bulan dua seri | Terpenuhi | Bagian 6, uji unit |
| `AC-300` `yearly` gagal → hanya bagian tahunan dan kartu *Jenis Laboratorium* | Terpenuhi | Bagian 6 |
| Tambahan: pemilih tidak pernah menawarkan tahun di luar `VAL-154` | Terpenuhi | Bagian 6, uji unit |
| DoD — akun analis asli | Terpenuhi 2026-10-09 | Gilang dan Vina, bagian 6 |
| DoD — uji unit, lint, build, laporan | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol pada berkas yang disentuh |
| Masalah yang diketahui | Footer sticky menutup sebagian bagian pada tangkapan `fullPage` — artefak tangkapan, bukan tata letak |
| Dependency backend | `BE-LAB-93` ⚠ wajib dideploy bersama (`MVP-13c`). Tanpanya bagian tahunan dan kartu *Jenis Laboratorium* bergalat (`404`), bagian lain tetap jalan |
| Perubahan sampingan | `NONE`. Berkas CRLF tetap CRLF |
| Status Git | Frontend: 6 berkas `M`, 5 berkas `??` — gabungan `FE-LAB-52`/`FE-LAB-53`, belum di-commit |
| Langkah berikutnya | **`MVP-13b` selesai.** Sisa `EPIC-LAB-19`: langkah rilis `MVP-13c` serempak dengan `BE-LAB-92`/`93`; pantau lama kueri tahunan di DB bersama |
