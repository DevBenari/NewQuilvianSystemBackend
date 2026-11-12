# Laporan Perubahan Frontend — `PAY-FE-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-04` |
| Judul | Halaman Proses Penggajian |
| Slice | Payroll mandiri — persiapan dan perhitungan |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-03/04/05/06 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-04 |
| Klasifikasi | `EPIC` — layar utama alur penggajian |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — Persiapan Penggajian, Perhitungan Gaji, detail run |

## 1. Keadaan yang ditemukan di awal

Layar lama hanya menampilkan angsuran per periode; tidak ada kesiapan data HRD, impor, rincian gross-to-net, riwayat status, atau pelacakan penahan/peringatan.

## 2. Proses bisnis dari sisi pengguna

Petugas membuka **Persiapan Penggajian**, memilih periode, menekan **Buka Payroll Run**, memeriksa **Kesiapan Data HRD**, lalu **Impor Data HRD** (pilih sumber). Di **Perhitungan Gaji** atau detail run ia menekan **Hitung Payroll**, memeriksa penahan/peringatan, membuka **Rincian** penerima (komponen), lalu **Ajukan**. Aksi memakai token versi; konflik 409 menampilkan **Muat Ulang Data**.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/run/payroll-run-preparation-view.jsx` | Persiapan: periode, buka run, kesiapan, daftar run |
| `src/components/view/payroll/run/payroll-run-readiness-panel.jsx, payroll-run-employee-detail-modal.jsx, payroll-run-list-section.jsx, payroll-run-list-view.jsx` | Panel kesiapan+impor, modal rincian penerima, daftar run (4 varian) |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx, payroll-run-table-columns.jsx` | Detail diperluas; kolom baru |
| `src/lib/hooks/payroll/use-payroll-run-detail.js, use-payroll-run-period.js, use-payroll-run-list.js, use-payroll-run-readiness.js` | Hook (dipindah dari `hr/payroll` dan diperluas) |
| `src/lib/services/payroll/payroll-run-service.js` | Service Axios Payroll Run |
| `src/utils/payroll/payroll-run-utils.js, payroll-common-utils.js` | Normalisasi dan aksi berbasis `availableActions` |
| `src/lib/constants/payroll/payroll-run-constants.js, payroll-constants.js` | Endpoint/rute/izin/label |
| `src/style/payroll/payroll-run.module.css` | Dipindah; +beberapa kelas berbasis token |
| `tests/unit/payroll-run-screens.test.mjs` | Diselaraskan dengan path BE dan layar baru |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** 14 elemen — REUSE 12, EXTEND 0, COMPOSE 1, WRAP 1, NEW 0. Komponen dasar yang dipakai: Hero, SummaryGrid, DataFilter, FilterSelect, DataTable, Pagination, StatusBadge, InformationAlert, AccessDeniedGate, BaseButton, ConfirmModal, BaseCheckboxField, BaseTextAreaField; ConfirmModal dipakai sebagai dialog informasi (COMPOSE: `hideCancel`, `size`, `children`). Elemen WRAP: `PayrollSection`/`PayrollInfoGrid`/`PayrollNoticeList`/`PayrollActionError` (`shared/payroll-section.jsx`), pembungkus tipis di atas CSS module Payroll yang sama dengan layar Payroll Run. **Pilihan:** A. tetap WRAP — **Rekomendasi** (konsisten dengan layar Payroll Run yang sudah ada, tanpa mengubah base, biaya terkecil); B. memakai `BaseDetailSection` (gaya kartu detail master data; risiko tampilan berbeda di halaman daftar dan perlu penyesuaian CSS); C. mengangkatnya menjadi base-feature (butuh persetujuan eksplisit dan uji pemakai lain; tidak dikerjakan). Tanpa jawaban, dijalankan A.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/teks "Mengambil data ..." (`DataTable loading`); panel kesiapan memuat terpisah |
| Kosong | "Payroll run tidak ditemukan." / "Tidak ada penerima pada filter ini." |
| Gagal | Banner merah dengan pesan dari backend; kegagalan 5xx/jaringan memakai teks jelas; tombol coba lagi/muat ulang bila relevan; 409 menawarkan Muat Ulang Data |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Payroll Run**.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/payroll-runs` | Daftar run per varian layar | ``PayrollRun : Read`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/summary` | Kartu ringkasan jumlah run | ``PayrollRun : ReadSummary`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/period-options` | Pilihan periode | ``PayrollRun : ReadPeriods`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/period-overview` | Status periode | ``PayrollRun : ReadPeriodOverview`` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs` | Buka payroll run | ``PayrollRun : Create`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{id}` | Detail run | ``PayrollRun : ReadDetail`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{id}/hrd-readiness` | Kesiapan data HRD | ``PayrollRun : ReadHrdReadiness`` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{id}/import-hrd` | Impor absensi/cuti/lembur | ``PayrollRun : ImportHrd`` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{id}/calculate` | Hitung / hitung ulang | ``PayrollRun : Calculate`` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{id}/submit` | Ajukan | ``PayrollRun : Submit`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{id}/employees/{runEmployeeId}` | Rincian gross-to-net penerima | ``PayrollRun : ReadEmployeeDetail`` |

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
| Buat run, cek kesiapan, import HRD, hitung, preview, validasi | Terpenuhi (statis) | Alur Persiapan → Detail; panel kesiapan; tombol Hitung/Ajukan berdasarkan `availableActions` + izin |
| Penahan dan peringatan terlihat | Terpenuhi | `PayrollNoticeList` pada detail run |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Filter tanggal pada daftar run tidak disediakan (BE tidak mendukungnya). |
| Dependency backend | PAY-BE-04 selesai (statis). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Uji alur penuh di peramban pada data pengembangan. |
