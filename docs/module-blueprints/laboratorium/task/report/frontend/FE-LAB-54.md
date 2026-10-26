# Laporan Perubahan Frontend — `FE-LAB-54`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-54` |
| Judul | Layar Statistik Pesanan, menu, unduh, dan cetak bercatat |
| Slice | `S16b-2` Statistik Pesanan Laboratorium — gelombang `MVP-15c` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) — *Gelombang `MVP-15`*, bagian `FE-LAB-54` |
| Trace | `FR-20.5`, `FR-20.7`..`FR-20.9`, `FR-20.12`; `LAB-DEC-242`..`246`, `LAB-DEC-248`; BR-152; `02-backend-architecture.md` 32.10 no. 6; `03-frontend-architecture.md` amandemen 2026-10-20 |
| Contract version | `LAB-API-v1` `r49` 44.3 dan 44.4; `LAB-PERM-v1` revision 17 19.2 — keduanya **`approved`** 2026-10-20 |
| Wewenang UI | Label dan letak menu (32.10 no. 6), kolom dan urutannya (BR-152), keterangan titik hitung, cetak hanya sesudah `POST /print-log` berhasil — **bukan** `DEV_DISCRETION`. Nama berkas, susunan komponen, gaya tabel, tata letak cetak — `DEV_DISCRETION` |
| Dependency | `BE-LAB-98` ✅ `SELESAI` (2026-10-20). Langkah rilis `MVP-15e` butir 2 untuk Kepala Instalasi ✅ |
| Klasifikasi | `MEDIUM` — satu layar baru, satu hook bersama, empat endpoint, satu butir menu; nol perubahan Redux |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan ini beserta tautannya pada roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7fb1e1d3e` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `4d3e4f98` (branch `yoga`) |
| Tanggal | 2026-10-21 |
| Status | ✅ **`SELESAI`** — 2026-10-21. Semula ⚠ (pemegang izin diwakili superadmin); diulang dengan **akun asli dr. Bima** — peramban 51/51 — dan data uji `MVP-15e` (bagian 9) |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Keadaan | Bukti |
| --- | --- | --- |
| Layar Statistik Pesanan | Belum ada — nol route, view, hook, service | `git status` bersih pada jalur `lab-order-statistics-*` di `7fb1e1d3e` |
| Backend | Empat endpoint hidup dan berizin: `filters/metadata`, daftar, `export`, `print-log` | `BE-LAB-98.md`; jawaban nyata diambil 2026-10-21 (superadmin) |
| Pola rujukan | Laporan Operasional `S16a` — penyaring periode, unduhan `blob`, galat `400`/`422`/`403` | `lab-operational-report-view.jsx`, `use-lab-operational-report*.jsx` |
| Cetak dari layar | `react-to-print` v3 (`useReactToPrint({ contentRef })`) sudah dipakai Daftar Pasien Lab | `lab-monitoring-view.jsx:195` |
| Bentuk jawaban nyata | Setiap baris membawa **kolom disiplin yang sama** (`total.cells`); periode tanpa pesanan → `groups: []` dengan `total.cells` tetap tiga disiplin bernilai nol | Respons `GET /lab-order-statistics-reports` 2026-01-01..2026-10-21 dan 2025-01 |
| Akun analis Gilang | `403` pada kesepuluh endpoint `S16b`, kontrol positif `lab-orders/filters/metadata` `200` | Uji HTTP 2026-10-21 (langkah rilis `MVP-15e` butir 4) |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** Kepala Instalasi Laboratorium dan manajemen — pemegang `LabOrderStatisticsReport : Read`.

1. Pengguna membuka grup **Laporan** di menu Laboratorium, lalu **Statistik Pesanan**. Butir ini hanya
   tampil bagi pemegang izinnya; pemegang `LabOperationalReport : Read` saja tidak melihatnya.
2. Layar langsung memuat laporan bulan berjalan: tanggal 1 sampai hari ini (WIB).
3. Di atas tabel tertulis tetap: *"Dihitung menurut waktu pesanan diminta. Pesanan dan pemeriksaan yang
   dibatalkan atau gugur tidak dihitung — sama dengan Beranda."*
4. Tabel dibaca per **jenis kunjungan** (mis. *Rawat Jalan*), lalu per **unit layanan** di bawahnya
   (mis. *Laboratorium Klinik*). Setiap disiplin punya dua angka: **Pesanan** dan **Pemeriksaan**;
   kolom **Jumlah** di ujung kanan. Setiap jenis kunjungan ditutup baris **Sub Total**, dan seluruh
   tabel ditutup baris **Total**. Contoh nyata 1–21 Oktober 2026:

   | Jenis Kunjungan | Unit Layanan | Patologi Klinik (Pesanan · Pemeriksaan) | Mikrobiologi (Pesanan · Pemeriksaan) | Jumlah (Pesanan · Pemeriksaan) |
   | --- | --- | --- | --- | --- |
   | Rawat Jalan | Laboratorium Klinik | 3 · 3 | 0 · 0 | 3 · 3 |
   | | Rawat Jalan | 1 · 1 | 2 · 2 | 3 · 3 |
   | **Sub Total Rawat Jalan** | | **4 · 4** | **2 · 2** | **6 · 6** |
   | **Total** | | **4 · 4** | **2 · 2** | **6 · 6** |

5. Pengguna menyaring menurut **periode**, **jenis kunjungan**, **unit layanan**, dan **disiplin** —
   boleh digabung. Setiap perubahan memuat ulang tabel; **Atur ulang filter** kembali ke bulan berjalan.
6. Pemegang `Export` melihat dua tombol:
   - **Unduh Excel** — berkas `statistik-pesanan_<awal>_<akhir>.xlsx` dibentuk server dengan penyaring
     yang sama persis dengan layar.
   - **Cetak** — layar lebih dulu meminta server mencatat cetak. **Hanya bila server menerima**,
     pratinjau cetak terbuka berisi judul, periode, keterangan titik hitung, dan tabel yang sedang tampil.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Periode tanpa pesanan | *"Tidak ada pesanan pada periode ini."*; Unduh dan Cetak nonaktif |
| Tanggal awal dikosongkan | Pesan server *"Periode laporan wajib diisi."* di bawah penyaring, panel *"Laporan belum dapat dibentuk"*, nol tabel |
| Awal sesudah akhir / lebih dari 366 hari | Pesan server `VAL-148` / `VAL-149` apa adanya |
| Pencatatan cetak gagal (jaringan) | *"Cetak belum dapat dicatat. Coba lagi."*; pratinjau **tidak** terbuka |
| Pencatatan cetak ditolak `403` | *"Anda tidak memiliki izin mencetak laporan ini."*; pratinjau tidak terbuka |
| Pemegang `Read` saja | Tabel terbaca; tombol Unduh dan Cetak tidak dirender |
| Tanpa izin `Read` | Menu tidak tampil; alamat langsung → *Akses Ditolak*, nol angka |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap `FE-LAB-54`; `03-frontend-architecture.md` amandemen 2026-10-20; `api-contract.md` 44.3–44.4;
  `00-interview-decisions.md` BR-152, `LAB-DEC-242`..`248`, `AC-330`..`337`; `testing/acceptance-test-matrix.md` `T-S16B-*`.
- Backend: `LabOrderStatisticsReportController.cs`, `LabOrderStatisticsReportDtos.cs`,
  `LabOrderStatisticsReportService.cs` (penyusun kolom dan baris), `LabReportXlsxWriter.cs` (judul kolom berkas),
  `LabFilterMetadataFactory.cs`.
- Frontend: seluruh berkas Laporan Operasional, `lab-monitoring-view.jsx` (cetak), `laboratory-state-panel.jsx`,
  `filter-date-picker.jsx`, `filter-select.jsx`, `use-permission.jsx`, `permission-slice.jsx`,
  `filter-menu-items-by-permission.js`, `menu-items.jsx`, `tests/unit/lab-menu-br137.test.mjs`,
  `tests/unit/menu-permission-filter.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/laboratory-management/lab-order-statistics-reports/page.jsx` | **Baru** — route tipis, judul tab *Statistik Pesanan Laboratorium* |
| `src/app/health-services/laboratory-management/lab-order-statistics-reports/lab-order-statistics-reports-client.jsx` | **Baru** — pembungkus client |
| `src/components/view/health-services/laboratory-management/lab-order-statistics-reports/lab-order-statistics-report-view.jsx` | **Baru** — Hero, penyaring, tombol keluaran, keadaan layar |
| `src/components/view/health-services/laboratory-management/lab-order-statistics-reports/lab-order-statistics-table.jsx` | **Baru** — tabel judul dua tingkat; kepala dokumen khusus kertas; isi yang diserahkan ke `react-to-print` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report.jsx` | **Baru** — penyaring, metadata, laporan, izin `Read`/`Export`, keadaan memuat diturunkan dari kunci permintaan |
| `src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report-export.jsx` | **Baru** — unduhan `.xlsx`, satu klik satu permintaan. *Sejak `FE-LAB-55` (2026-10-21) menjadi pembungkus hook bersama `useLabReportDownload`; perilaku sama, diuji ulang lewat build dan uji unit* |
| `src/lib/hooks/health-services/laboratory-management/use-lab-report-logged-print.jsx` | **Baru — hook cetak bercatat bersama** (dipakai juga `FE-LAB-55`) |
| `src/lib/hooks/health-services/laboratory-management/lab-report-print-log-rules.js` | **Baru** — aturan murni cetak bercatat: urutan, *fail-closed*, penanda tekan ganda |
| `src/lib/hooks/health-services/laboratory-management/lab-order-statistics-report-rules.js` | **Baru** — aturan murni: parameter, pilihan penyaring, kolom dari `total.cells`, baris unit/*Sub Total*/*Total*, nama berkas |
| `src/lib/services/health-services/laboratory-management/lab-order-statistics-report.service.js` | **Baru** — metadata, laporan, `export` (`blob`), `print-log` |
| `src/lib/constants/health-services/laboratory-management/lab-order-statistics-report-constants.jsx` | **Baru** — izin, teks layar, judul kolom (sama dengan berkas `.xlsx`) |
| `src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx` | `LABORATORY_API.labOrderStatisticsReports` |
| `src/lib/hooks/health-services/laboratory-management/use-lab-operational-report-export.jsx` | `saveBlob` kini diekspor (dipakai ulang); perilaku Laporan Operasional tidak berubah |
| `src/style/health-services/laboratory-management/lab-order-statistics-reports/lab-order-statistics-report.module.css` | **Baru** — tata letak tabel dan aturan `@media print`; seluruh warna token |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir *Statistik Pesanan* di grup *Laporan*, `requiredPermission: { resource: "LabOrderStatisticsReport", action: "Read" }` |
| `tests/unit/lab-order-statistics-report-rules.test.mjs` | **Baru** — 19 uji: aturan murni (10), cetak bercatat (7), menu (2) |
| `tests/unit/lab-menu-br137.test.mjs` | Daftar label/key/pathname grup Lab ditambah butir `FE-LAB-54` |

### 3.3 Kepatuhan arsitektur frontend

- Alur `src/app` → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios`; view tidak memanggil Axios.
- Dipakai ulang: `AccessDeniedGate`, `Hero`, `DataFilter`, `FilterDatePicker`, `FilterSelect`, `BaseDetailSection`,
  `BaseButton`, `InformationAlert`, `LaboratoryStatePanel`; aturan periode, disiplin, format angka, dan penggolongan
  galat dari `lab-operational-report-rules.js`; `saveBlob` dari hook unduhan Laporan Operasional; `useReactToPrint`.
- **Pola baru, beralasan:** (1) tabel ditulis manual karena judul kolomnya dua tingkat dan baris jenis kunjungan
  digabung — `DataTable` tidak mendukungnya; atribut `data-flat-table="true"` dipasang sendiri sesuai kontrak
  typography tabel. (2) Hook cetak bercatat bersama — diminta roadmap supaya `FE-LAB-55` memakai yang sama.
- Nol Redux slice baru: laporan ini hanya dipakai satu layar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Penyaring tetap; *"Membentuk laporan..."* di bawah keterangan titik hitung; Unduh/Cetak nonaktif; tombol *Muat ulang* berputar |
| Kosong | *"Tidak ada pesanan pada periode ini."* — *"Pesanan dihitung pada tanggal diminta. Ubah periode atau longgarkan penyaring lain."* |
| Gagal | Pesan server atau *"Statistik pesanan gagal dimuat."* dengan tombol *Coba lagi* |
| Periode salah (`400`/`422`) | Pesan server apa adanya di bawah penyaring, panel *"Laporan belum dapat dibentuk — Perbaiki periode pada penyaring di atas."* |
| Unduhan gagal | Pesan di dekat tombol; hilang begitu penyaring berganti |
| Cetak gagal | *"Cetak belum dapat dicatat. Coba lagi."* atau pesan tidak berizin; pratinjau tidak terbuka |
| Tanpa hak akses | *Akses Ditolak* — *"Anda tidak memiliki izin membuka Statistik Pesanan Laboratorium."* atau pesan `403` server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order Statistics Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-order-statistics-reports/filters/metadata` | Pilihan jenis kunjungan, unit layanan, disiplin | `LabOrderStatisticsReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-order-statistics-reports` | Tabel — `startDate`, `endDate`, `encounterType`, `serviceUnitId`, `discipline` | `LabOrderStatisticsReport : Read` |
| `GET` | `/v1/health-services/laboratory-management/lab-order-statistics-reports/export` | Unduh Excel (`responseType: blob`) | `LabOrderStatisticsReport : Export` |
| `POST` | `/v1/health-services/laboratory-management/lab-order-statistics-reports/print-log` | Mencatat cetak sebelum pratinjau dibuka | `LabOrderStatisticsReport : Export` |

Pemetaan kontrak: nilai `encounterType` = `name` enum dari metadata (`Outpatient`, …); `serviceUnitId` = `id` unit dari
metadata laporan (bukan `service-units/options`, yang berizin lain); kolom disiplin = urutan `total.cells`; disiplin
`null` = kolom *Belum terklasifikasi*. **Delta kontrak–source: nol.**

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-order-statistics-report-rules.test.mjs tests/unit/lab-menu-br137.test.mjs tests/unit/lab-operational-report-rules.test.mjs` | 48/48 | `PASS` | Keluaran perintah |
| Uji unit penuh `tests/unit/` | 3053/3082; **29 gagal di 22 berkas — sama dengan baseline lama**, seluruhnya di luar Lab (Rawat Inap, Hemodialisis, CRM, Bank Darah `M0`, kas kecil) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint` atas seluruh berkas yang berubah | Nol galat, nol peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | 6 galat di 3 berkas finance/billing yang tidak disentuh (`react/no-unescaped-entities`) | `EXISTING / ENVIRONMENT ISSUE` | `shift-detail-modal.jsx`, `inpatient-deposit-receipt-modal.jsx`, `inpatient-deposit-top-up-form.jsx` |
| `npm run build` (server BE dan FE dimatikan dulu) | Berhasil; route `/health-services/laboratory-management/lab-order-statistics-reports` terdaftar statis | `PASS` | Keluaran perintah |
| Peramban terhadap backend nyata (devYoga), superadmin | 39 skenario lulus — rincian di bawah | `PASS` | Skrip Playwright; tangkapan layar awal, gabungan, periode kosong, cetak gagal, ponsel |
| Peramban, pemegang `Read` saja (daftar izin `/v1/auth/permissions` dicegat; backend nyata) | Tabel tampil; Unduh dan Cetak tidak dirender | `PASS` | Skrip Playwright |
| Peramban, akun asli **Gilang** (Analis, tanpa izin) | Menu tidak tampil; alamat langsung → *Akses Ditolak*, nol tabel, nol permintaan unduh/cetak | `PASS` | Skrip Playwright; tangkapan layar |
| Log aplikasi backend | Per putaran: tepat satu `LabOrderStatisticsReport.Export` dan satu `.Print` — tanpa No. RM, nama, maupun alasan | `PASS` | `[AUD] … LABORDERSTATISTICSREPORT_EXPORT/PRINT` |

**Rincian peramban (superadmin, 1–21 Oktober 2026):** menu tampil; judul tab dan Hero; keterangan titik hitung;
periode bawaan `2026-10-01`..`2026-10-21` sebagai tanggal polos; judul kolom dua tingkat sama dengan `total.cells`;
jumlah baris = unit + *Sub Total* + *Total*; angka *Sub Total* dan *Total* = angka backend; nama unit dari respons;
nol identitas pasien; penyaring jenis kunjungan, unit, dan disiplin mengirim nilai yang benar, digabung; label
terpilih; reset; kosong bersebab dengan Unduh/Cetak nonaktif; `VAL-147` (tanggal awal dihapus) → pesan server, panel,
nol tabel, pulih sesudah reset; unduhan `statistik-pesanan_2026-10-01_2026-10-21.xlsx` dengan tepat satu
`GET /export` berparameter sama dengan layar; `print-log` diputus → pesan gagal dan **nol** iframe cetak; tekan ganda
→ **tepat satu** `POST /print-log` berisi penyaring layar, lalu **satu** iframe cetak berisi judul, periode, keterangan,
dan *Total*; lebar 390 px tanpa geser horizontal halaman (tabel menggulir di dalam bingkainya); baris *Total* tidak
tertutup footer sesudah digulir.

Uji manual: `PASS` — dengan batas di bagian 8.

**Tidak dijalankan:**

- E2E Playwright berpola standalone dengan API tiruan — digantikan putaran peramban terhadap backend nyata yang
  mencakup skenario yang sama (termasuk kegagalan pencatat lewat pemutusan jaringan); spec e2e tidak ditambahkan.
- `T-S16B-16` (*Kamar Bersalin* di bawah Rawat Inap) — devYoga hanya memuat order Rawat Jalan; menunggu data uji
  langkah rilis `MVP-15e` butir 3 (izin tulis pemilik modul). Di tingkat unit, nama unit terbukti dibaca apa adanya.
- Dialog cetak sistem operasi — headless Chromium tidak menampilkannya; bukti pembukaannya adalah iframe
  `react-to-print` beserta isinya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-330` (menu) — butir tampil hanya bagi `LabOrderStatisticsReport : Read`, tidak bagi `LabOperationalReport : Read`; `Read` tanpa `Export` → layar terbuka tanpa Unduh/Cetak | Terpenuhi | Uji unit menu (3 kasus); peramban Read-saja dan Gilang; `403` HTTP Gilang |
| `AC-333`, `AC-334`, `AC-336` sisi layar — angka tampil apa adanya dari backend, tidak dihitung ulang | Terpenuhi | Uji unit dengan *Sub Total*/*Total* sengaja tidak sama dengan jumlah baris; peramban: angka = respons. Kesamaan dengan Beranda dibuktikan `BE-LAB-98` |
| `AC-335` sisi layar — kolom *Belum terklasifikasi*, *Unit tidak tercatat*, nol identitas pasien | Terpenuhi | Uji unit; peramban: tabel dan isi cetak tanpa No. RM |
| `AC-337` — cetak hanya sesudah `print-log` berhasil; gagal → pratinjau tidak terbuka; satu tekan satu log | Terpenuhi | 7 uji unit penjalan cetak; peramban: putus → nol iframe; tekan ganda → satu log, satu iframe; log aplikasi satu `.Print` |
| Keterangan titik hitung tetap di atas tabel | Terpenuhi | Peramban (juga saat kosong dan di isi cetak) |
| Nama unit dari respons — *Kamar Bersalin*, bukan *LABOR* (`LAB-DEC-243`) | Terpenuhi | Uji unit; peramban dr. Bima 2026-10-21 (`T-S16B-16`, bagian 9) |
| DoD: layar, menu, unduh, cetak bercatat berjalan | Terpenuhi | Bagian 6 |
| DoD: uji unit hijau | Terpenuhi | 48/48 berkas terkait; baseline penuh tidak bertambah |
| DoD: laporan `task/report/frontend/FE-LAB-54.md` | Terpenuhi | Berkas ini |
| Verifikasi peramban dengan akun pemegang izin asli (dr. Bima) | Terpenuhi | 2026-10-21 — bagian 9 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `react-hooks/refs` sempat muncul pada hook cetak; diselesaikan dengan penanda tekan ganda yang hanya disentuh di handler (`runLoggedPrint(gateRef.current, …)`) |
| Masalah yang diketahui | Footer aplikasi menutupi baris terbawah pada tangkapan layar *full-page* — bukan tumpang-tindih; sesudah digulir baris *Total* berakhir di atas footer (784 px vs 845 px) |
| Dependency backend | `NONE` — `BE-LAB-98` selesai. Data uji jenis kunjungan lain menunggu `MVP-15e` butir 3 |
| Perubahan sampingan | `NONE` — `npm run build` menulis `.next/` (tidak di-track); hasil unduhan dan tangkapan layar ada di scratchpad sesi |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | `NONE` — task selesai |

`git status --short` frontend sesudah pekerjaan (perubahan Radiologi dan `health-service-select-resources.js` sudah ada
sebelum task ini dan tidak disentuh):

```text
 M src/lib/constants/health-services/laboratory-management/laboratory-constants.jsx
 M src/lib/hooks/health-services/laboratory-management/use-lab-operational-report-export.jsx
 M src/utils/menu-sidebar/menu-items.jsx
 M tests/unit/lab-menu-br137.test.mjs
?? src/app/health-services/laboratory-management/lab-order-statistics-reports/
?? src/components/view/health-services/laboratory-management/lab-order-statistics-reports/
?? src/lib/constants/health-services/laboratory-management/lab-order-statistics-report-constants.jsx
?? src/lib/hooks/health-services/laboratory-management/lab-order-statistics-report-rules.js
?? src/lib/hooks/health-services/laboratory-management/lab-report-print-log-rules.js
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report-export.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-order-statistics-report.jsx
?? src/lib/hooks/health-services/laboratory-management/use-lab-report-logged-print.jsx
?? src/lib/services/health-services/laboratory-management/lab-order-statistics-report.service.js
?? src/style/health-services/laboratory-management/lab-order-statistics-reports/
?? tests/unit/lab-order-statistics-report-rules.test.mjs
```

---

## 9. Verifikasi ulang dengan akun asli dr. Bima (2026-10-21)

| Hal | Isi |
| --- | --- |
| Akun | dr. Bima (Kepala Instalasi Laboratorium) — pemegang `LabOrderStatisticsReport : Read` dan `: Export`; sandi diberikan pemilik modul, tidak dicatat |
| Data uji | Data uji `MVP-15e` langkah 3 dibuat 2026-10-21 lewat API (izin tulis pemilik modul): kunjungan Rawat Inap (unit Rawat Inap), IGD (unit Kamar Bersalin), IGD (unit IGD); pesanan `LAB-RSMMC-000025`..`000030`, termasuk satu prosedur katalog uji tanpa disiplin (kini nonaktif), pesanan lima pemeriksaan yang dibatalkan (`000029`), dan satu pembatalan yang alasannya dikosongkan lewat satu `UPDATE` SQL untuk meniru data lama (`000030`) |
| Skrip peramban yang sama (bagian 6), pemegang izin = dr. Bima | **51/51** `PASS` (sebelum data uji: 46/46) |
| `T-S16B-16` di peramban | Kelompok **IGD** memuat unit *Instalasi Gawat Darurat* dan *Kamar Bersalin* (bukan *LABOR*); kelompok **Rawat Inap** dengan *Sub Total* sendiri; Rawat Jalan tetap — `PASS` |
| `T-S16B-11` di peramban dan berkas | Kolom *Belum terklasifikasi* tampil; berkas `.xlsx` unduhan dr. Bima memuat *Kamar Bersalin*, *Sub Total Rawat Inap*, *Sub Total IGD*, *Belum terklasifikasi* — `PASS` |
| Pesanan dibatalkan tidak dihitung | Unit IGD = 1 pesanan (Kalium); dua pesanan IGD yang dibatalkan tidak terhitung (HTTP) — `PASS` |
| Log aplikasi | Tepat satu `LabOrderStatisticsReport.Export` dan satu `.Print` atas nama `bima.kepala.lab@quilvian.local`, tanpa identitas pasien |
| `Read` tanpa `Export` | Layar: tombol tidak dirender (daftar izin dicegat, karena tidak ada akun `Read`-saja). Server: `403` unduh/cetak bagi `Read`-saja sudah dibuktikan dengan akun dr. Bima bertahap 2026-10-20 (`BE-LAB-98`) |
| Catatan data | Pesanan dari prosedur katalog belum digolongkan terhitung 1 pesanan / 0 pemeriksaan di kolom *Belum terklasifikasi*: jalur `POST /lab-orders` lama tidak membentuk baris permintaan. Definisi hitung sama dengan Beranda (`INV-59`) — bukan cacat layar |
