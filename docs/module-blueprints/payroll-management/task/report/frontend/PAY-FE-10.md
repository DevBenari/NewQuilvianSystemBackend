# Laporan Perubahan Frontend — `PAY-FE-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-10` |
| Judul | Laporan & dashboard Payroll |
| Slice | Payroll mandiri — laporan |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-15 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-10 |
| Klasifikasi | `HEAVY` — banyak berkas baru, kontrak keuangan |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — Rekap Penggajian dan Rekonsiliasi Keuangan; dashboard grafis tidak dibuat |

## 1. Keadaan yang ditemukan di awal

Tidak ada laporan agregat tanpa gaji per orang maupun rekonsiliasi Payroll–Finance–Accounting.

## 2. Proses bisnis dari sisi pengguna

Pengguna memilih periode, run, jenis penerima, dan unit di **Rekap Penggajian** dan melihat kartu serta tabel agregat (nominal hanya bagi pemegang izin). Di **Rekonsiliasi Keuangan** ia memilih run dan melihat pemeriksaan sesuai/selisih beserta penjelasan; event Accounting tertulis "Belum Tersedia".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/report/*` | Rekap, rekonsiliasi, kolom |
| `src/lib/hooks/payroll/use-payroll-recap.js, use-payroll-reconciliation.js` | Hook |
| `src/lib/services/payroll/payroll-report-service.js` | Service |
| `src/utils/payroll/payroll-report-utils.js` | Normalisasi |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** 12 elemen — REUSE 11, EXTEND 0, COMPOSE 0, WRAP 1, NEW 0. Komponen dasar yang dipakai: Hero, SummaryGrid, DataFilter, FilterSelect, DataTable, Pagination, StatusBadge, InformationAlert, AccessDeniedGate, BaseButton, ResourceFilterSelect (unit/pusat biaya via `useSelectResource("costCenters")`). Elemen WRAP: `PayrollSection`/`PayrollInfoGrid`/`PayrollNoticeList`/`PayrollActionError` (`shared/payroll-section.jsx`), pembungkus tipis di atas CSS module Payroll yang sama dengan layar Payroll Run. **Pilihan:** A. tetap WRAP — **Rekomendasi** (konsisten dengan layar Payroll Run yang sudah ada, tanpa mengubah base, biaya terkecil); B. memakai `BaseDetailSection` (gaya kartu detail master data; risiko tampilan berbeda di halaman daftar dan perlu penyesuaian CSS); C. mengangkatnya menjadi base-feature (butuh persetujuan eksplisit dan uji pemakai lain; tidak dikerjakan). Tanpa jawaban, dijalankan A.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/teks "Mengambil data ..." (`DataTable loading`) |
| Kosong | "Tidak ada payroll run pada filter ini." |
| Gagal | Banner merah dengan pesan dari backend; kegagalan 5xx/jaringan memakai teks jelas; tombol coba lagi/muat ulang bila relevan |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Report**.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/reports/recap` | Rekap agregat | ``PayrollReport : Read`` |
| `GET` | `/v1/corporate/payroll-management/reports/reconciliation` | Rekonsiliasi satu run | ``PayrollReport : ViewAmount`` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `eslint` pada cakupan yang disentuh | 0 error; 27 peringatan `react-hooks/set-state-in-effect` pada hook baru (pola yang sama ada di hook payroll lama) | `EXISTING WARNING` | keluaran eslint |
| `node --test tests/unit/payroll-module-contract.test.mjs` | 12 lulus | `PASS` | berkas tes |
| `node --test tests/unit/payroll-run-screens.test.mjs`, `payroll-installment-deduction-guards`, `hr-master-data-sidebar-grouping` | 11 + 6 + 6 lulus | `PASS` | berkas tes |
| Resolver impor statis (`verify_fe.py`) | Tidak ada impor baru yang tak terselesaikan dibanding SHA dasar | `PASS` | keluaran skrip |
| Grep anti-regresi (tombol/tabel mentah, warna literal, `!important`, tipografi Bootstrap, inline style) | Kosong pada berkas baru | `PASS` | tes kontrak + grep |
| `npm run build` | Tidak dijalankan (dilarang pemilik) | `NOT RUN` | - |
| Uji manual kontrol interaktif (filter, cari, reset, paginasi, kirim, unduh) | Tidak ada server yang boleh dijalankan | `NOT FEASIBLE` | - |

AUTOMATED TEST: `node --test` pada berkas tes di atas — PASS. Seluruh suite unit memiliki kegagalan lama yang tidak terkait (dibandingkan dengan ekspor bersih SHA dasar).

**Tidak dijalankan:** build, server pengembangan, e2e. Perbandingan visual berdampingan dengan modul referensi belum dilakukan.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Filter periode/unit/kategori | Terpenuhi (statis) | Periode, run, jenis penerima, unit |
| Rekap tidak bocorkan data gaji | Terpenuhi | Hanya agregat; nominal null tanpa `ViewAmount` |
| Dashboard | Tidak dikerjakan | Tidak ada grafik; kartu ringkasan dan tabel |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Tidak ada ekspor berkas. |
| Dependency backend | PAY-BE-10 sebagian (jurnal ⛔). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Tambahkan ekspor/dashboard bila diminta. |
