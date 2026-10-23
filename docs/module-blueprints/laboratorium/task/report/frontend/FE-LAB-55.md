# Laporan Perubahan Frontend — `FE-LAB-55`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-55` |
| Judul | Daftar Laporan Pembatalan, menu, unduh, dan cetak bercatat |
| Slice | `S16b-1` Laporan Pembatalan Order & Pemeriksaan — gelombang `MVP-15d` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) — *Gelombang `MVP-15`*, bagian `FE-LAB-55` |
| Trace | `FR-20.1`..`FR-20.3`, `FR-20.7`..`FR-20.9`, `FR-20.12`; `LAB-DEC-237`..`240`, `LAB-DEC-248`; BR-151; `02-backend-architecture.md` 32.10 no. 6; `03-frontend-architecture.md` amandemen 2026-10-20 |
| Contract version | `LAB-API-v1` `r49` 44.2 dan 44.4; `LAB-PERM-v1` revision 17 — keduanya **`approved`** 2026-10-20 |
| Wewenang UI | Label dan letak menu (32.10 no. 6), dua belas kolom dan urutannya (BR-151), teks *Alasan tidak dicatat*, halaman detail terpisah, cetak hanya sesudah `POST /print-log` berhasil — **bukan** `DEV_DISCRETION`. Nama berkas, susunan komponen, gaya tabel, tata letak cetak — `DEV_DISCRETION` |
| Dependency | `FE-LAB-54` ⚠ (hook cetak bercatat bersama); `BE-LAB-99` ✅, `BE-LAB-100` ✅ |
| Klasifikasi | `MEDIUM` — satu layar berhalaman berisi identitas pasien, lima endpoint, satu butir menu; nol perubahan Redux |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan ini beserta tautannya pada roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7fb1e1d3e` (branch `YogaV2`) — di atas pekerjaan `FE-LAB-54` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `4d3e4f98` (branch `yoga`) |
| Tanggal | 2026-10-21 |
| Status | ✅ **`SELESAI`** — 2026-10-21. Semula ⚠; diulang dengan **akun asli dr. Bima** — peramban 73/73 — dengan data uji `MVP-15e`, tautan detail kini terbuka (`FE-LAB-56`), dan selisih label diseragamkan (`LAB-DEC-250`) — bagian 9 |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Keadaan | Bukti |
| --- | --- | --- |
| Layar Laporan Pembatalan | Belum ada | Nol berkas `lab-cancellation-*` |
| Backend | Enam endpoint hidup: metadata, `cancelled-by-options`, daftar, detail, `export`, `print-log` | `BE-LAB-99.md`, `BE-LAB-100.md` |
| Data nyata devYoga (1–21 Oktober 2026) | **Dua** peristiwa: satu pembatalan Pesanan (1 pemeriksaan, 7 Oktober) dan satu pembatalan Pemeriksaan *Trombosit* (1 Oktober, sudah layak tagih); keduanya beralasan | Respons `GET /lab-cancellation-reports` |
| Akun Gilang (Analis) | `403` pada keenam endpoint laporan ini | Uji HTTP 2026-10-21 (langkah rilis `MVP-15e` butir 4) |
| Pola rujukan | `FE-LAB-54` (penyaring, unduh, cetak bercatat), Laporan Penerimaan (tabel berhalaman, token rute detail) | `lab-order-statistics-report-view.jsx`, `use-lab-reception-report.jsx` |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** Kepala Instalasi Laboratorium dan manajemen — pemegang `LabCancellationReport : Read`.

1. Pengguna membuka **Laporan → Laporan Pembatalan**. Butir ini hanya tampil bagi pemegang izinnya;
   hak baca daftar Lab, Laporan Operasional, maupun Statistik Pesanan tidak memunculkannya.
2. Layar memuat pembatalan bulan berjalan (tanggal 1 sampai hari ini, WIB), **terbaru di atas**, 25 baris per halaman.
3. Di atas tabel tertulis total atas **seluruh** hasil penyaring, misalnya *Total: Pesanan 1 · Pemeriksaan 1 ·
   Seluruhnya 2*. Angka ini sama di setiap halaman.
4. Setiap baris satu peristiwa pembatalan dengan dua belas kolom: *Waktu Batal (WIB) · Jenis · No. Order · No. RM ·
   Nama Pasien · Disiplin · Pemeriksaan · Status Sebelum · Sudah Layak Tagih · Alasan · Dibatalkan Oleh · Unit Layanan*.
   Contoh nyata:

   | Waktu Batal (WIB) | Jenis | Pemeriksaan | Status Sebelum | Sudah Layak Tagih | Alasan |
   | --- | --- | --- | --- | --- | --- |
   | 07/10/2026 15.01 | Pesanan | 1 pemeriksaan | Diminta | - | duplikat |
   | 01/10/2026 12.16 | Pemeriksaan | Trombosit | Layak Tagih | Ya | Uji FE-LAB-36 — pemeriksaan batal (AC-234) |

   Waktu dibaca **WIB**: pembatalan 30 September 23.30 UTC tampil *01/10/2026 06.30* dan masuk laporan Oktober.
5. Pembatalan lama yang tersimpan tanpa alasan tampil ***Alasan tidak dicatat*** — miring dan redup, berbeda dari
   alasan sungguhan, bukan sel kosong — dan tetap terhitung di total.
6. Pengguna menyaring menurut **periode, disiplin, jenis (Pesanan/Pemeriksaan), Dibatalkan oleh, unit layanan**, dan
   mengatur jumlah baris per halaman. Setiap perubahan penyaring kembali ke halaman 1. Pilihan *Dibatalkan oleh*
   dimuat ulang setiap kali periode berubah; petugas yang tidak membatalkan apa pun di periode baru dilepas dari penyaring.
7. **No. order** (baris Pesanan) dan **nama pemeriksaan** (baris Pemeriksaan) adalah tautan ke halaman detail peristiwa
   (`FE-LAB-56`).
8. Pemegang `Export`: **Unduh Excel** — berkas `laporan-pembatalan_<awal>_<akhir>.xlsx` berisi **seluruh** baris hasil
   penyaring; **Cetak** — server mencatat lebih dulu, lalu pratinjau dibuka berisi judul, periode, total, dan baris
   halaman yang sedang tampil (kertas mendatar).

**Jalur tidak normal:** periode tanpa pembatalan → *"Tidak ada pembatalan pada periode ini."*, Unduh/Cetak nonaktif;
tanggal awal dihapus → pesan server *"Periode laporan wajib diisi."* dan panel *"Laporan belum dapat dibentuk"*;
pencatatan cetak gagal → *"Cetak belum dapat dicatat. Coba lagi."*, pratinjau tidak dibuka; pemegang `Read` saja →
tanpa tombol Unduh/Cetak; tanpa izin → menu tidak tampil, alamat langsung *Akses Ditolak*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap `FE-LAB-55`; `03-frontend-architecture.md` amandemen 2026-10-20; `api-contract.md` 44.2, 44.4; BR-151,
  `AC-328`..`AC-332`, `AC-337`; `testing/acceptance-test-matrix.md` `T-S16B-04`..`08`, `12`.
- Backend: `LabCancellationReportController.cs`, `LabCancellationReportDtos.cs`, `LabReportXlsxWriter.cs` (judul kolom,
  label jenis, *Alasan tidak dicatat*), `LabFilterMetadataFactory.cs` (label status).
- Frontend: berkas `FE-LAB-54`, Laporan Penerimaan (`use-lab-reception-report.jsx`, `[slug]/route-token.js`),
  `private-route-token-utils.js`, `data-table.jsx`, `pagination.jsx`, `lab-order-constants.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/laboratory-management/lab-cancellation-reports/page.jsx`, `lab-cancellation-reports-client.jsx` | **Baru** — route tipis, judul tab *Laporan Pembatalan Order & Pemeriksaan* |
| `src/components/view/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-report-view.jsx` | **Baru** — Hero, tujuh penyaring, total, Unduh/Cetak, tabel berhalaman, keadaan layar |
| `src/components/view/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-report-table-columns.jsx` | **Baru** — dua belas kolom BR-151; tombol tautan detail; tampilan *Alasan tidak dicatat* |
| `src/components/view/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-print-document.jsx` | **Baru** — dokumen cetak (hanya di kertas): judul, periode, total, halaman tampil |
| `src/lib/hooks/health-services/laboratory-management/use-lab-cancellation-report.jsx` | **Baru** — penyaring, halaman, metadata, *Dibatalkan oleh* per periode, izin, tautan detail lewat token rute privat |
| `src/lib/hooks/health-services/laboratory-management/lab-cancellation-report-rules.js` | **Baru** — aturan murni: parameter daftar vs keluaran, format WIB, label jenis, kolom Pemeriksaan, status sebelum, *Alasan tidak dicatat*, total, nama berkas |
| `src/lib/hooks/health-services/laboratory-management/use-lab-report-download.jsx` | **Baru — unduhan `.xlsx` bersama** kedua laporan `S16b` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report-export.jsx` | Disederhanakan menjadi pembungkus `useLabReportDownload` (perilaku `FE-LAB-54` tetap) |
| `src/lib/services/health-services/laboratory-management/lab-cancellation-report.service.js` | **Baru** — metadata, `cancelled-by-options`, daftar, `export` (`blob`), `print-log` |
| `src/lib/constants/health-services/laboratory-management/lab-cancellation-report-constants.jsx` | **Baru** — izin, rute, token detail, teks, label jenis, dua belas judul kolom |
| `src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx` | `LABORATORY_API.labCancellationReports` |
| `src/style/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-report.module.css` | **Baru** — total, *Alasan tidak dicatat*, tombol tautan, dokumen cetak mendatar |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir *Laporan Pembatalan* sebelum *Statistik Pesanan*, `requiredPermission: { resource: "LabCancellationReport", action: "Read" }` |
| `tests/unit/lab-cancellation-report-rules.test.mjs` | **Baru** — 14 uji: WIB batas hari, *Alasan tidak dicatat*, kolom, total, parameter, *Dibatalkan oleh*, judul kolom, privasi penyimpanan, menu |
| `tests/unit/lab-menu-br137.test.mjs` | Label/key/pathname grup Lab ditambah butir `FE-LAB-55` |

### 3.3 Kepatuhan arsitektur frontend

- Alur `src/app` → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios`.
- Dipakai ulang: `DataTable` + `RegionPagination`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `AccessDeniedGate`,
  `LaboratoryStatePanel`, `BaseDetailSection`, `BaseButton`; `useLabReportLoggedPrint` dan `saveBlob` (`FE-LAB-54`);
  aturan periode/disiplin/galat Laporan Operasional; `buildServiceUnitOptions` Statistik Pesanan; label status
  `LAB_ORDER_STATUS_LABEL`/`LAB_EXAMINATION_STATUS_LABEL`; `registerPrivateRouteToken` (pola Laporan Penerimaan).
- **Penyatuan:** unduhan kedua laporan kini satu hook (`useLabReportDownload`) — menghindari salinan kedua.
- **Privasi:** respons hanya di keadaan React halaman; nol `localStorage`/cache (diuji di tingkat sumber dan di peramban).
  Token detail di `sessionStorage` hanya membawa `historyId` dan No. order — tanpa No. RM maupun nama pasien.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Penyaring tetap; tabel *"Membentuk laporan..."*; Unduh/Cetak nonaktif |
| Kosong | *"Tidak ada pembatalan pada periode ini."* — *"Pembatalan dihitung pada waktu dibatalkan (WIB). Ubah periode atau longgarkan penyaring lain."*; total tidak tampil |
| Gagal | Pesan server atau *"Laporan pembatalan gagal dimuat."* dengan *Coba lagi* |
| Periode salah (`400`/`422`) | Pesan server di bawah penyaring dan panel *"Laporan belum dapat dibentuk"*; tabel tidak dirender |
| Pilihan penyaring gagal | Peringatan *"Pilihan penyaring gagal dimuat."* / *"Pilihan petugas pembatal gagal dimuat."*; laporan tetap tampil |
| Unduh/cetak gagal | Pesan di dekat tombol; cetak tidak membuka pratinjau |
| Tanpa hak akses | *Akses Ditolak* — *"Anda tidak memiliki izin membuka Laporan Pembatalan."* atau pesan `403` server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Cancellation Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-cancellation-reports/filters/metadata` | Pilihan disiplin, jenis, unit layanan | `LabCancellationReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-cancellation-reports/cancelled-by-options` | Pilihan *Dibatalkan oleh* untuk periode terpilih | `LabCancellationReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-cancellation-reports` | Tabel dan total — penyaring + `pageNumber`/`pageSize` | `LabCancellationReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-cancellation-reports/export` | Unduh Excel — penyaring **tanpa** halaman | `LabCancellationReport : Export` |
| `POST` | `/v1/health-services/laboratory-management/lab-cancellation-reports/print-log` | Mencatat cetak sebelum pratinjau | `LabCancellationReport : Export` |

`GET /{historyId}` (detail) belum dipanggil — milik `FE-LAB-56`.

**Delta tampilan layar–berkas (bukan delta kontrak):** kolom *Status Sebelum* di layar memakai label status Lab yang sudah
dipakai layar lain (`Layak Tagih`, `Sedang Dikerjakan`, `Terpesan`, `Digugurkan`), sedangkan berkas `.xlsx` memakai label
backend (`Layak tagih`, `Sedang dikerjakan`, `Dipesan`, `Gugur`). Kodenya sama; hanya bunyinya berbeda pada dua status.
Diserahkan ke pemilik modul bila bunyinya perlu diseragamkan. **Diputuskan 2026-10-21 (`LAB-DEC-250`): berkas mengikuti layar** — label backend diubah; kini layar dan berkas berbunyi sama (bagian 9).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit `lab-cancellation-report-rules`, `lab-order-statistics-report-rules`, `lab-menu-br137` | 40/40 | `PASS` | Keluaran perintah |
| Uji unit penuh `tests/unit/` | 3067/3096; 29 gagal — **daftar identik** dengan sebelum task (baseline lama, di luar Lab) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar gagal |
| `npx eslint` atas seluruh berkas yang berubah | Nol galat, nol peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | 6 galat di 3 berkas finance/billing yang tidak disentuh — sama dengan `FE-LAB-54` | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npm run build` (server BE/FE dimatikan dulu) | Berhasil; `/health-services/laboratory-management/lab-cancellation-reports` terdaftar | `PASS` | Keluaran perintah |
| Peramban terhadap backend nyata (devYoga) | **56/56** — superadmin, pemegang `Read` saja (daftar izin dicegat), akun asli Gilang | `PASS` | Skrip Playwright; tangkapan layar di scratchpad sesi (memuat data pasien, tidak dilampirkan) |
| Log aplikasi backend | Tepat satu `LabCancellationReport.Export` (*2 baris*) dan satu `.Print`; pesan tanpa No. RM, nama, maupun alasan | `PASS` | `[AUD] … LABCANCELLATIONREPORT_EXPORT/PRINT` |

**Rincian peramban:** menu dan judul; periode bawaan dan halaman 1/25; *Dibatalkan oleh* dimuat dengan periode yang sama;
dua belas judul kolom berurutan; setiap sel kedua baris sama dengan respons (waktu WIB, jenis, No. order, No. RM, nama,
disiplin, *n pemeriksaan*/nama pemeriksaan, layak tagih, alasan, petugas, unit); tautan detail di sel yang benar; terbaru
di atas; total = `totals`; **`AC-332`**: dengan ukuran halaman 1 (permintaan diubah di jalan, backend nyata menjawab)
halaman 2 mengirim `pageNumber=2`, total identik di kedua halaman, isi baris berbeda; ganti penyaring kembali ke halaman 1;
penyaring jenis, gabungan jenis + petugas, unit; kosong bersebab; ganti tanggal akhir ke 6 Oktober → `cancelled-by-options`
dimuat ulang dan petugas yang tidak lagi ada dilepas; `VAL-147`; **`AC-328`** (jawaban nyata diubah satu ruas di jalan
karena devYoga tak punya alasan kosong) → *Alasan tidak dicatat* miring, total tetap; unduh satu `GET /export` tanpa
parameter halaman; cetak diputus → nol pratinjau; tekan ganda → satu `print-log`, satu pratinjau berisi judul, periode,
total, dua belas judul kolom, dan baris; klik tautan → `/lab-cancellation-reports/<token>`, `sessionStorage` tanpa
identitas pasien; `localStorage` tanpa identitas pasien; lebar 390 px tanpa geser halaman; `Read` saja → tanpa
Unduh/Cetak dan butir menu Statistik Pesanan tidak ikut tampil; Gilang → tanpa menu, *Akses Ditolak*, nol unduh/cetak.

Uji manual: `PASS` — dengan batas di bagian 8.

**Tidak dijalankan:**

- Halaman detail sesudah tautan diklik — rutenya dibangun `FE-LAB-56`; yang diuji di sini hanya navigasi dan isi token.
- Spec e2e API tiruan — digantikan putaran peramban terhadap backend nyata (sama dengan `FE-LAB-54`).
- `T-S16B-04` dan `T-S16B-05` pada data nyata (alasan kosong; satu order lima pemeriksaan) — menunggu data uji `MVP-15e`
  butir 3. `AC-328` di layar dibuktikan dengan jawaban nyata yang diubah satu ruas; `AC-329` dibuktikan backend (`BE-LAB-99`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-328` sisi layar — *Alasan tidak dicatat* dibedakan, baris tetap terhitung | Terpenuhi | 1 uji unit; peramban (miring, `data-reason-recorded=false`, total tetap) |
| `AC-329` sisi layar — satu baris per peristiwa; baris Pesanan menulis jumlah pemeriksaannya | Terpenuhi | Peramban: Pesanan *1 pemeriksaan* satu baris, Pemeriksaan satu baris; uji unit *5 pemeriksaan* |
| `AC-331` sisi layar — waktu WIB pada batas hari | Terpenuhi | Uji unit 30 September 23.30 UTC → *01/10/2026 06.30*; peramban dua baris nyata |
| `AC-332` sisi layar — total sama di setiap halaman dan dari backend | Terpenuhi | Uji unit; peramban halaman 1 dan 2 |
| `AC-330` — menu hanya bagi `LabCancellationReport : Read`; `Read` saja tanpa Unduh/Cetak; tanpa izin `403` | Terpenuhi | 2 uji unit menu (lima kombinasi izin); peramban Read-saja dan Gilang |
| `AC-337` — cetak fail-closed, satu tekan satu log | Terpenuhi | Uji unit penjalan (`FE-LAB-54`); peramban; log aplikasi satu `.Print` |
| Penyaring *Dibatalkan oleh* dimuat ulang saat periode berubah | Terpenuhi | Uji unit `keepCancelledBySelection`; peramban |
| Tautan ke detail (`FE-LAB-56`) | Terpenuhi | Navigasi, token, dan halaman tujuan (`FE-LAB-56`) terbukti |
| DoD: daftar, menu, unduh, cetak berjalan; uji hijau; laporan | Terpenuhi | Bagian 6; berkas ini |
| Verifikasi dengan akun pemegang izin asli (dr. Bima) | Terpenuhi | 2026-10-21 — bagian 9 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Komentar JSDoc `*Ya*/*Tidak*` sempat menutup blok komentar dan menggagalkan muat modul uji; diperbaiki sebelum uji dinyatakan lulus |
| Masalah yang diketahui | `NONE` — (1) selisih label diseragamkan `LAB-DEC-250`; (2) halaman detail selesai `FE-LAB-56` |
| Dependency backend | `NONE`. Data uji alasan kosong dan order banyak pemeriksaan menunggu `MVP-15e` butir 3 |
| Perubahan sampingan | `NONE` — `use-lab-order-statistics-report-export.jsx` (`FE-LAB-54`, belum di-commit) disederhanakan dengan sengaja; `.next/` hasil build tidak di-track |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | `NONE` — task selesai |

`git status --short` frontend (perubahan Radiologi dan `health-service-select-resources.js` sudah ada sebelum gelombang ini
dan tidak disentuh; berkas `lab-order-statistics-*` milik `FE-LAB-54`):

```text
 M src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-operational-report-export.jsx
 M src/utils/menu-sidebar/menu-items.jsx
 M tests/unit/lab-menu-br137.test.mjs
?? src/app/health-services/laboratory-management/lab-cancellation-reports/
?? src/app/health-services/laboratory-management/lab-order-statistics-reports/
?? src/components/view/health-services/laboratory-management/lab-cancellation-reports/
?? src/components/view/health-services/laboratory-management/lab-order-statistics-reports/
?? src/lib/constants/health-services/laboratory-management/lab-cancellation-report-constants.jsx
?? src/lib/constants/health-services/laboratory-management/lab-order-statistics-report-constants.jsx
?? src/lib/hooks/health-services/laboratory-management/lab-cancellation-report-rules.js
?? src/lib/hooks/health-services/laboratory-management/lab-order-statistics-report-rules.js
?? src/lib/hooks/health-services/laboratory-management/lab-report-print-log-rules.js
?? src/lib/hooks/health-services/laboratory-management/use-lab-cancellation-report.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report-export.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-report-download.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-report-logged-print.jsx
?? src/lib/services/health-services/laboratory-management/lab-cancellation-report.service.js
?? src/lib/services/health-services/laboratory-management/lab-order-statistics-report.service.js
?? src/style/health-services/laboratory-management/lab-cancellation-reports/
?? src/style/health-services/laboratory-management/lab-order-statistics-reports/
?? tests/unit/lab-cancellation-report-rules.test.mjs
?? tests/unit/lab-order-statistics-report-rules.test.mjs
```

---

## 9. Verifikasi ulang dengan akun asli dr. Bima (2026-10-21)

| Hal | Isi |
| --- | --- |
| Akun | dr. Bima (Kepala Instalasi Laboratorium) — pemegang `LabCancellationReport : Read` dan `: Export`; sandi tidak dicatat |
| Data uji | Data uji `MVP-15e` langkah 3 dibuat 2026-10-21 lewat API (izin tulis pemilik modul): kunjungan Rawat Inap (unit Rawat Inap), IGD (unit Kamar Bersalin), IGD (unit IGD); pesanan `LAB-RSMMC-000025`..`000030`, termasuk satu prosedur katalog uji tanpa disiplin (kini nonaktif), pesanan lima pemeriksaan yang dibatalkan (`000029`), dan satu pembatalan yang alasannya dikosongkan lewat satu `UPDATE` SQL untuk meniru data lama (`000030`) |
| Skrip peramban (bagian 6), pemegang izin = dr. Bima | **73/73** `PASS` — baris bertambah karena data uji; uji *Dibatalkan oleh* dialihkan ke petugas yang hanya membatalkan pada 1 Oktober |
| `T-S16B-04` data nyata | `LAB-RSMMC-000030` tampil *Alasan tidak dicatat* (miring) dan terhitung di total — `PASS` |
| `AC-329` / `T-S16B-05` data nyata | Pesanan `000029` lima pemeriksaan = **satu** baris *Pesanan · 5 pemeriksaan*; berkas `.xlsx` memuat *5 pemeriksaan* dan *Alasan tidak dicatat* — `PASS` |
| `LAB-DEC-250` | Kolom *Status Sebelum* di layar dan di berkas `.xlsx` unduhan dr. Bima kini sama (*Layak Tagih*, *Diminta*; nol *Layak tagih*/*Dipesan*) — perubahan backend `LabFilterMetadataFactory`, harness `BE-LAB-100` 31/31 |
| Log aplikasi | Tepat satu `LabCancellationReport.Export` dan satu `.Print` atas nama dr. Bima, tanpa identitas pasien |
| Akun asli Gilang | Tetap tanpa menu, *Akses Ditolak*, nol unduh/cetak — `PASS` |
