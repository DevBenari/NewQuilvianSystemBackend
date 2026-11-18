# Laporan Perubahan Frontend — `FE-HRD-R15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R15` |
| Judul | Layar Hubungan Industrial: sambung dan migrasi |
| Slice | `S-C5` |
| Trace | API Contract v22 bagian 28; task asal `FE-HRD-248`–`260` |
| Contract version | v22, `approved` |
| Wewenang UI | Sambung API nyata dan migrasi base component pada layar Hubungan Industrial |
| Dependency | `FE-HRD-R03` ✅ |
| Klasifikasi | `EPIC` — 12 layar audit, 163 pelanggaran awal, 59 class CSS hilang |
| Task mode | `FRONTEND` |
| Commit frontend | `ec36164052a033ee4db95fae4a962cae62c2203c` (`HamzahV2`) |
| Commit backend rujukan | `7b2b15ccd54e32771bca4cf89d3f9a855c7eea1f` (`MHamzah`, read-only) |
| Tanggal | 16 November 2026 |
| Status | 🟡 **SEBAGIAN** — source dan validasi statis lulus; live browser terblokir autentikasi lokal |

## 1. Keadaan yang ditemukan di awal

Implementasi migrasi telah berada pada HEAD setelah audit 11 November 2026. Tiga belas berkas JSX dalam domain `hrd/relations` sudah memakai service API nyata, base component, dan CSS Module yang seluruh key-nya valid. Service tidak mengembalikan data contoh ketika gagal.

Daftar sanksi, skorsing, dan exit clearance lintas pegawai belum mempunyai endpoint backend. Service secara eksplisit menghasilkan keadaan “belum tersedia”; gap tersebut dimiliki `FE-HRD-R53` dan tidak ditutup diam-diam oleh task ini.

## 2. Proses bisnis dari sisi pengguna

Petugas mencatat insiden, menjalankan investigasi/BAP, menerbitkan sanksi atau skorsing, menangani keluhan dan whistleblower, mengelola forum bipartit, terminasi, serta exit clearance. Penyimpanan memakai endpoint resmi. Ketika daftar backend belum tersedia, layar menjelaskan keterbatasan dan tidak menampilkan baris fiktif.

## 3. Perubahan dan gerbang tampilan

Source aplikasi tidak memerlukan perubahan tambahan pada sesi ini karena target implementasi sudah ada di HEAD. Pekerjaan sesi memverifikasi source dan memperbarui laporan/status.

| Elemen | Base component / gaya | Keputusan |
| --- | --- | --- |
| Header, aksi, ringkasan | `Hero`, `BaseButton`, `SummaryGrid` | `REUSE` |
| Filter dan tabel | `DataFilter`, `DataTable` | `REUSE` |
| Status | `StatusBadge` | `REUSE` |
| Form | `BaseFormControl`, `ResourceFilterSelect` | `REUSE` |
| Dialog | `ConfirmModal` | `REUSE` |
| Pesan | `InformationAlert`, `ToastStack` | `REUSE` |
| Layout domain | CSS Module existing berbasis token | `COMPOSE` |

`UI GATE: PASS — 6 kelompok REUSE, 1 COMPOSE, 0 NEW.`

```text
$ node rules/frontend/tools/check-css-usage.mjs <13 berkas JSX relations>
OK — 13 berkas diperiksa, 0 temuan.
$ grep anti-Tailwind
NO_TAILWIND_MATCHES
$ grep elemen form/tabel mentah
NO_RAW_FORM_OR_TABLE_ELEMENTS
```

Dialog aksi menggunakan `ConfirmModal`, sehingga mempunyai backdrop gelap dan bukan blok biasa.

`VISUAL CHECK: NOT FEASIBLE — autentikasi localhost gagal membentuk sesi; /api/v1/auth/me menghasilkan 401 pada pengujian Chromium headed FE-HRD-R14 dalam environment yang sama.`

## 4. State layar

| State | Perilaku |
| --- | --- |
| Memuat | Loading state pada tabel/panel |
| Kosong | Empty state tanpa data karangan |
| Gagal | `InformationAlert` dan retry yang relevan |
| Tanpa hak | Aksi disembunyikan berdasarkan permission backend |

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET/POST` | `/api/v1/hrd/disciplinary/incidents` | Daftar dan pencatatan insiden |
| `POST` | `/api/v1/hrd/disciplinary/investigations` | Mulai investigasi |
| `POST` | `/api/v1/hrd/disciplinary/sanctions` | Terbitkan sanksi |
| `POST` | `/api/v1/hrd/disciplinary/suspensions` | Terbitkan skorsing |

#### Corporate - HRD Grievance & Whistleblower

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET` | `/api/v1/hrd/grievances/my-tickets` | Keluhan milik pengguna |
| `POST` | `/api/v1/hrd/grievances` | Ajukan keluhan |
| `POST` | `/api/v1/hrd/grievances/whistleblower` | Laporan whistleblower |

#### Corporate - HRD Employee Separation & Severance

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET/POST` | `/api/v1/hrd/separations` | Daftar dan mulai terminasi |
| `GET` | `/api/v1/hrd/exit-clearance/{separationId}` | Proses exit clearance |

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint` | `0 errors`, `1252 warnings` existing | `PASS` |
| `npm run build` | Compiled, 814 static pages, standalone siap | `PASS` |
| CSS usage | 13 berkas, 0 temuan | `PASS` |
| Data contoh/fallback sukses palsu | Tidak ditemukan | `PASS` |
| Chromium headed | Sesi login localhost gagal (`401`) | `EXISTING / ENVIRONMENT ISSUE` |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kasus dan keluhan memakai API nyata | Terpenuhi secara source | `employee-relation-service.js` dan view domain |
| Komponen standar | Terpenuhi | CSS gate 0 temuan; tidak ada elemen mentah |
| Dialog berlatar gelap | Terpenuhi secara source | Seluruh dialog memakai `ConfirmModal` |
| Screenshot dan alur interaktif setelah login | Belum terpenuhi | Autentikasi localhost `401` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah diketahui | Endpoint daftar tertentu menunggu `FE-HRD-R53`; live browser menunggu sesi valid |
| Perubahan sampingan | `NONE` |
| Langkah berikutnya | Pulihkan autentikasi test lalu verifikasi dialog, submit validasi, dan respons network tanpa menulis data produksi |
