# Laporan Perubahan Frontend — `PAY-FE-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-08` |
| Judul | Halaman Pembayaran Gaji & Honor |
| Slice | Payroll mandiri — pembayaran penggajian |
| Roadmap | Tidak ada roadmap Payroll terdaftar di `docs/module-blueprints/`; register status ada di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | CT-PAY-08/09 |
| Contract version | Controller Backend Payroll pada SHA `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal (kontrak dibaca dari source, tidak ditebak) |
| Wewenang UI | Prompt `MASTER_PROMPT_MIGRASI_MODUL_PAYROLL_QUILVIAN_V2.md`: pakai komponen, tema, filter, tabel, status, pagination, navigasi, hak akses yang sudah ada; label Bahasa Indonesia |
| Dependency | PAY-BE-08 |
| Klasifikasi | `HEAVY` — banyak berkas baru, kontrak keuangan |
| Task mode | `CROSS-REPO` (BE dan FE sama-sama diberi wewenang tulis oleh prompt) |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan ini di repository backend |
| Model | Claude (`claude-sonnet-5-5`) |
| Commit frontend saat dikerjakan | `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` + perubahan lokal belum di-commit |
| Commit backend yang dijadikan rujukan | `b5fb6c251eafcd680ec35483732e3e72226e19f3` + perubahan lokal belum di-commit |
| Tanggal | 2026-11-04 |
| Status | 🟡 Sebagian — pengiriman dan riwayat ada; status bayar bergerak setelah Finance mengirim ACK |

## 1. Keadaan yang ditemukan di awal

Tidak ada layar untuk mengirim instruksi pembayaran dan memantau hasil bank.

## 2. Proses bisnis dari sisi pengguna

Staf keuangan membuka **Pengiriman Pembayaran**, menekan **Kirim ke Finance** pada run disahkan, dan mengonfirmasi (pengiriman idempoten). **Riwayat Pembayaran** menampilkan instruksi per penerima dengan status, referensi bank, dan **Coba Ulang** untuk yang gagal. Layar sengaja tidak menyediakan pengisian hasil bank.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`/`CLAUDE.md` frontend, `rules/frontend/*` (arsitektur, katalog base component, gerbang keputusan, token desain, pola halaman, checklist UI, kebijakan tes), modul referensi visual terdekat (`hr/payroll/run`, `hr/master-data/job-level`), `menu-items.jsx`, `InstanceAxios`, controller/DTO Payroll di backend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/payment/*` | Modal pengiriman, riwayat, kolom |
| `src/lib/hooks/payroll/use-payroll-run-dispatch.js, use-payroll-payment-history.js` | Hook |
| `src/lib/services/payroll/payroll-payment-service.js` | Service (tanpa ACK) |
| `src/utils/payroll/payroll-payment-utils.js` | Normalisasi |

### 3.3 Kepatuhan arsitektur frontend

Alur `route tipis → view → hook → service → InstanceAxios`; halaman di `src/app/payroll/**` hanya memuat metadata dan satu view; tidak ada `InstanceAxios` di view atau halaman; alias `@/`; konstanta endpoint/rute/izin dipusatkan di `src/lib/constants/payroll/`; tidak ada Axios instance baru, tidak ada hook/view master-data generik baru, dan tidak ada Redux slice baru (data operasional mengikuti pola hook + service payroll run yang sudah ada).

**Keputusan komponen (UI GATE):** 11 elemen — REUSE 10, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0. Komponen dasar yang dipakai: Hero, SummaryGrid, DataFilter, FilterSelect, DataTable, Pagination, StatusBadge, InformationAlert, AccessDeniedGate, BaseButton, ConfirmModal, BaseTextAreaField.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/teks "Mengambil data ..." (`DataTable loading`) |
| Kosong | "Belum ada instruksi pembayaran." |
| Gagal | Banner merah dengan pesan dari backend; kegagalan 5xx/jaringan memakai teks jelas; tombol coba lagi/muat ulang bila relevan |
| Tanpa hak akses | Halaman diganti peringatan "Ups! Akses Ditolak" (`AccessDeniedGate`); menu tidak tampil bila izin menu tidak ada |

## 5. Endpoint yang dikonsumsi

Grup **Corporate / Payroll Management / Payment** dan **Payroll Run**.

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{runId}/finance-handoff` | Kirim instruksi ke Finance | ``PayrollPayment : FinanceHandoff`` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/{runId}/payments` | Riwayat instruksi | ``PayrollPayment : Read`` |
| `POST` | `/v1/corporate/payroll-management/payroll-runs/{runId}/payments/{paymentId}/retry` | Coba ulang yang gagal | ``PayrollPayment : Retry`` |

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
| Monitoring per batch/pegawai | Terpenuhi (statis) | Ringkasan per run + tabel per penerima |
| Gagal/parsial/berhasil | Sebagian | Status ditampilkan; berubah dari `Dijadwalkan` hanya setelah Finance mengirim ACK (⛔ sisi Finance) |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi/terjemahan LINQ/perilaku layar belum dibuktikan karena `dotnet build`, `npm run build`, dan server dilarang pemilik. |
| Masalah yang diketahui | Finance belum punya jenis pembayaran `PAYROLL`. |
| Dependency backend | PAY-BE-08 sebagian (penerima Finance ⛔). |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 45 file `M`, 65 file `D` (dipindah/diganti), 133 file `??` — tidak ada stage/commit |
| Langkah berikutnya | Bangun penerima instruksi di Finance. |
