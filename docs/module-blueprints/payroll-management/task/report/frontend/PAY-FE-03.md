# Laporan Perubahan Frontend — `PAY-FE-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-03` |
| Judul | Halaman Data Penghasilan Pegawai & Dokter |
| Slice | Payroll mandiri — penghasilan pegawai |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-02; `PAYROLL_V2_02` bagian 1 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-03 |
| Klasifikasi | `HEAVY` — banyak berkas baru, kontrak keuangan |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — Profil Penggajian (daftar+detail) dan Penetapan Gaji |

## 1. Keadaan yang ditemukan di awal

Layar Penetapan Gaji lama memanggil rute yang tidak ada dan membaca field yang tidak dikirim backend (nominal tampil "IDR 0"). Tidak ada tinjauan profil penggajian terpadu.

## 2. Proses bisnis dari sisi pengguna

Petugas Payroll membuka **Profil Penggajian**, mencari dan memfilter penerima (jenis, status, kesiapan), lalu klik dua kali untuk melihat rincian: penetapan gaji berlaku, riwayat penetapan, rekening tersamar, pajak/iuran, dan daftar masalah. Nominal tampil hanya bila berizin; selain itu tertulis "Tidak ditampilkan". **Penetapan Gaji** menampilkan rekap masa berlaku dan persetujuan tanpa nominal.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/compensation/*` | Daftar, detail (`BaseDetailView`), kolom, rekap penetapan gaji |
| `src/lib/hooks/payroll/use-payroll-compensation-list.js, use-payroll-compensation-detail.js, use-payroll-salary-assignments.js` | Hook daftar/detail/rekap |
| `src/lib/services/payroll/payroll-compensation-service.js` | Service Axios |
| `src/utils/payroll/payroll-compensation-utils.js` | Normalisasi (nominal null tetap null) |
| `src/app/payroll/workforce-compensations/**, salary-assignments/page.jsx` | Rute tipis |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** 11 elemen — REUSE 10, EXTEND 0, COMPOSE 0, WRAP 1, NEW 0. Komponen dasar yang dipakai: Hero, SummaryGrid, DataFilter, FilterSelect, DataTable, Pagination, StatusBadge, InformationAlert, AccessDeniedGate, BaseButton, BaseDetailView. Elemen WRAP: `PayrollSection`/`PayrollInfoGrid`/`PayrollNoticeList`/`PayrollActionError` (`shared/payroll-section.jsx`), pembungkus tipis di atas CSS module Payroll yang sama dengan layar Payroll Run. **Pilihan:** A. tetap WRAP — **Rekomendasi** (konsisten dengan layar Payroll Run yang sudah ada, tanpa mengubah base, biaya terkecil); B. memakai `BaseDetailSection` (gaya kartu detail master data; risiko tampilan berbeda di halaman daftar dan perlu penyesuaian CSS); C. mengangkatnya menjadi base-feature (butuh persetujuan eksplisit dan uji pemakai lain; tidak dikerjakan). Tanpa jawaban, dijalankan A.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/teks "Mengambil data ..." (`DataTable loading`) |
| Kosong | "Data profil penggajian tidak ditemukan." + saran filter |
| Gagal | Banner merah dengan pesan dari backend; kegagalan 5xx/jaringan memakai teks jelas; tombol coba lagi/muat ulang bila relevan |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Workforce Compensation** dan rekap HRD **WorkforceCoreCrossEmployee** (read-only).

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/workforce-compensations` | Daftar profil penggajian | ``PayrollCompensation : Read`` |
| `GET` | `/v1/corporate/payroll-management/workforce-compensations/{workforceProfileId}` | Detail penghasilan penerima | ``PayrollCompensation : ReadDetail`` |
| `GET` | `/v1/corporate/human-resource/workforce-core/salary-assignments` | Rekap penetapan gaji lintas pegawai | ``WfpSalaryAssignment : ReadAll`` |

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
| Profil, grade, struktur, bank terverifikasi, pajak/iuran dapat ditinjau | Terpenuhi (statis) | Kolom dan detail pada `payroll-compensation-table-columns.jsx` |
| Nominal tidak bocor ke peran umum | Terpenuhi (statis) | `toNumberOrNull`; teks "Tidak ditampilkan"; tes kontrak normalisasi |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Tidak ada aksi ubah/setujui penetapan gaji di layar ini (milik HRD). |
| Dependency backend | PAY-BE-03 selesai sebagian (statis). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Uji di peramban dengan peran berizin dan tidak berizin `ViewAmount`. |
