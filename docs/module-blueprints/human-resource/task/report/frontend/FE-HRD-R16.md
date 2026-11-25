# Laporan Perubahan Frontend — `FE-HRD-R16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R16` |
| Judul | Layar Kinerja: sambung dan migrasi |
| Slice | `S-C3` |
| Trace | API Contract v21 bagian 27; `FE-HRD-232`–`244` |
| Contract version | v21, `approved` |
| Wewenang UI | Sambung data kinerja nyata dan migrasi komponen standar |
| Dependency | `FE-HRD-R04` ✅ |
| Klasifikasi | `EPIC` — 14 berkas audit domain dan modal, 96 pelanggaran awal |
| Task mode | `FRONTEND` |
| Commit frontend/backend | FE `ec3616405` (`HamzahV2`); BE `7b2b15ccd` (`MHamzah`, read-only) |
| Tanggal | 16 November 2026 |
| Status | 🟡 **SEBAGIAN** — source/statis lulus; visual setelah login belum terverifikasi |

## 1. Keadaan awal dan proses bisnis

Implementasi target telah ada pada HEAD sesudah audit. Layar siklus, KPI, appraisal, kalibrasi, sengketa, PIP, dan jenjang klinis memakai `PerformanceService` serta state gagal yang jujur. Pengajuan yang baru dibuat ditampilkan dari response server, bukan objek contoh.

Petugas membuka siklus, menyusun KPI, menjalankan self/manager appraisal, kalibrasi, sengketa, PIP, dan penilaian jenjang klinis. Daftar sanggahan dan pengajuan jenjang lintas pegawai yang backend-nya belum tersedia ditampilkan sebagai keterbatasan, tidak diganti data fiktif; gap dimiliki `FE-HRD-R53`.

## 2. Gerbang tampilan dan arsitektur

Alur tetap App Router → view → hook HR → `PerformanceService` → `InstanceAxios`. `performance-review-view.jsx` milik layar lama `FE-HRD-016` diperiksa oleh CSS gate tetapi tidak diubah karena bukan task asal `232`–`244`.

| Elemen | Keputusan | Sumber |
| --- | --- | --- |
| Hero, aksi, ringkasan | `REUSE` | `Hero`, `BaseButton`, `SummaryGrid` |
| Filter/tabel | `REUSE` | `DataFilter`, `DataTable` |
| Status/form/dialog | `REUSE` | `StatusBadge`, `BaseFormControl`, `ConfirmModal` |
| Pesan/notifikasi | `REUSE` | `InformationAlert`, `ToastStack` |
| Layout | `COMPOSE` | CSS Module existing bertoken |

`UI GATE: PASS — 4 kelompok REUSE, 1 COMPOSE, 0 NEW.`

```text
OK — 15 berkas diperiksa, 0 temuan.
NO_TAILWIND_MATCHES
NO_RAW_FORM_OR_TABLE_ELEMENTS
```

`VISUAL CHECK: NOT FEASIBLE — autentikasi localhost pada environment yang sama menghasilkan /api/v1/auth/me 401.`

## 3. State dan endpoint

State memuat, kosong, gagal, dan tanpa hak ditangani melalui base component dan permission constants.

#### Corporate - HRD Performance Management & Clinical Career

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET/POST` | `/api/v1/corporate/hrd/performance/cycles` | Siklus kinerja |
| `GET/POST` | `/api/v1/corporate/hrd/performance/kpi-catalogs` | Katalog KPI |
| `GET/POST` | `/api/v1/corporate/hrd/performance/appraisals` | Penilaian |
| `GET/POST` | `/api/v1/corporate/hrd/performance/calibration` | Kalibrasi |
| `GET/POST` | `/api/v1/corporate/hrd/performance/pip` | PIP |
| `POST` | `/api/v1/corporate/hrd/performance/clinical-ladder/apply` | Pengajuan jenjang klinis |

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint` | 0 errors, 1252 warnings existing | `PASS` |
| `npm run build` | Compiled; 814 halaman statis; standalone siap | `PASS` |
| CSS gate | 15 berkas, 0 temuan | `PASS` |
| Data contoh/sukses palsu | Tidak ditemukan pada service/view target | `PASS` |
| Browser setelah login | Auth lokal `401` | `EXISTING / ENVIRONMENT ISSUE` |

## 5. Acceptance criteria dan penutup

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kinerja pegawai dari data nyata | Terpenuhi secara source | `performance-service.js` dan view `FE-HRD-232`–`244` |
| Komponen standar | Terpenuhi | CSS gate dan grep anti-regresi |
| Seluruh kriteria kartu asal tervalidasi di browser | Belum | Sesi localhost tidak valid |

Langkah berikutnya: pulihkan autentikasi test, lalu verifikasi filter, modal, validasi, permission, dan network per layar. Perubahan sampingan: `NONE`.
