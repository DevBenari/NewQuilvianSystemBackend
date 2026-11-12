# Laporan Perubahan Frontend — `PAY-FE-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-01` |
| Judul | Buat modul navigasi Payroll mandiri |
| Slice | Payroll mandiri — navigasi |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | Prompt Rancangan Modul > Menu > Submenu; `PAYROLL_V2_02` bagian 1 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-01 (rute dan izin tersedia) |
| Klasifikasi | `MEDIUM` — satu berkas menu besar + konfigurasi pengalihan |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | ✅ Selesai (statis) — modul Payroll 8 menu / 24 fitur, tautan lama dialihkan |

## 1. Keadaan yang ditemukan di awal

Menu Payroll tersebar di HRD (grup "Payroll & Penggajian" di Master Data HR, "Penetapan Gaji", "Payroll Run & Potong Gaji") dan berlabel campur Inggris/Indonesia.

## 2. Proses bisnis dari sisi pengguna

Pengguna memilih modul **Payroll** di bagian Perusahaan, memilih menu (Data Induk, Penghasilan Pegawai, Proses Penggajian, Potongan dan Penyesuaian, Remunerasi Dokter, Pembayaran Penggajian, Slip Gaji, Laporan), lalu submenu/fitur. Butir hanya tampil bila pengguna punya izin menu. Tautan atau bookmark lama (mis. `/hr/payroll/runs`) otomatis dialihkan ke alamat baru.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Modul `corporatePayroll` ditambahkan setelah HR; grup Payroll, Penetapan Gaji, dan Payroll Run dikeluarkan dari HR; semua butir punya `requiredPermission` |
| `next.config.js` | `getPayrollRouteRedirects()` — 10 master + run + detail run + restitusi + penetapan gaji (sementara/non-permanen) |
| `src/app/payroll/page.jsx` | `/payroll` diarahkan ke Persiapan Penggajian |
| `tests/unit/hr-master-data-sidebar-grouping.test.mjs` | Ekspektasi HR 10 grup/52 butir; Grade Gaji di modul Payroll |
| `tests/unit/payroll-module-contract.test.mjs` | Tes baru: hierarki, label, izin, halaman nyata, pengalihan |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** `UI GATE: N/A — hanya konfigurasi menu dan pengalihan; tidak ada elemen UI baru`

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sidebar memuat daftar izin dulu; butir yang izinnya belum diketahui disembunyikan (keputusan ketat) |
| Kosong | Modul tanpa butir yang boleh diakses ikut disembunyikan |
| Gagal | Bila izin gagal dimuat, butir berizin tidak tampil; akses langsung tetap dijaga 403/`AccessDeniedGate` |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Task ini tidak memanggil API.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `-` | `-` | Tidak memanggil API (`NOT APPLICABLE`) | `-` |

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
| Menu/submenu Bahasa Indonesia | Terpenuhi | `payroll-module-contract` — label dan larangan istilah Inggris |
| Tautan lama tidak putus | Terpenuhi (statis) | Pengalihan di `next.config.js`; berkas rute lama sudah dihapus; tes kontrak |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Pengalihan bersifat sementara (307); penghapusan setelah seluruh pemakai pindah. Pengujian di peramban belum dilakukan. |
| Dependency backend | Tidak ada; izin menu mengikuti `AccessController` BE. |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Admin menambahkan izin menu pada Akses Role. |
