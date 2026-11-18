# Laporan Perubahan Frontend — `FE-HRD-R17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R17` |
| Judul | Layar Pelatihan: sambung dan migrasi |
| Slice | `S-C2` |
| Trace | API Contract v20 bagian 26; `FE-HRD-217`–`229` |
| Contract version | v20, `approved` |
| Dependency | `FE-HRD-R05` ✅ |
| Klasifikasi | `EPIC` — 13 berkas target layar/modal Pelatihan |
| Task mode | `FRONTEND` |
| Commit frontend/backend | FE `ec3616405` (`HamzahV2`); BE `7b2b15ccd` (`MHamzah`, read-only) |
| Tanggal | 16 November 2026 |
| Status | 🟡 **SEBAGIAN** — source target dan validasi statis lulus; browser belum terautentikasi |

## 1. Keadaan awal dan proses bisnis

Source target sudah memakai `LearningService` tanpa fallback sukses palsu. Radar sertifikat memanggil endpoint `certificates/expiring-radar`, termasuk zona H-90, H-60, H-30, dan kedaluwarsa. Pengguna dapat meninjau rencana tahunan, aturan wajib, sesi, asesmen, sertifikat, SKP, dan ikatan dinas berdasarkan respons server.

`training-certification-view.jsx` adalah layar lama `FE-HRD-018` yang dimiliki `FE-HRD-R56`, bukan task asal `217`–`229`. Audit menemukan dua raw button/class hilang di sana; temuan tidak diperbaiki secara oportunistik dan tetap dicatat sebagai scope `R56`.

## 2. Gerbang tampilan

| Elemen | Keputusan | Sumber |
| --- | --- | --- |
| Header/ringkasan/aksi | `REUSE` | `Hero`, `SummaryGrid`, `BaseButton` |
| Filter/tabel | `REUSE` | `DataFilter`, `DataTable` |
| Status/form/dialog | `REUSE` | `StatusBadge`, `BaseFormControl`, `ConfirmModal` |
| Alert/toast | `REUSE` | `InformationAlert`, `ToastStack` |
| Layout | `COMPOSE` | CSS Module existing bertoken |

`UI GATE: PASS — 4 kelompok REUSE, 1 COMPOSE, 0 NEW pada 13 berkas target.`

```text
OK — 13 berkas target diperiksa, 0 temuan.
NO_TAILWIND_MATCHES
NO_RAW_FORM_OR_TABLE_ELEMENTS_TARGET
```

Pemeriksaan direktori penuh juga menemukan dua pelanggaran pada layar lama `training-certification-view.jsx`; itu bukti scope `FE-HRD-R56`, bukan kegagalan source target task ini.

`VISUAL CHECK: NOT FEASIBLE — /api/v1/auth/me localhost menghasilkan 401 pada Chromium headed.`

## 3. State dan endpoint

State loading, kosong, gagal, dan tanpa hak memakai base component serta permission constants.

#### Corporate - HRD Learning, Training & Competency

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET` | `/api/v1/corporate/hrd/learning/certificates/expiring-radar` | Radar sertifikat H-90/H-60/H-30 |
| `GET/POST` | `/api/v1/corporate/hrd/learning/plans` | Rencana pelatihan |
| `GET/POST` | `/api/v1/corporate/hrd/learning/sessions` | Sesi dan peserta |
| `GET` | `/api/v1/corporate/hrd/learning/certificates/staff/{id}` | Portofolio sertifikat pegawai |

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint` | 0 errors, 1252 warnings existing | `PASS` |
| `npm run build` | Compiled, 814 static pages, standalone siap | `PASS` |
| CSS gate target | 13 berkas, 0 temuan | `PASS` |
| Radar sertifikat | Request server nyata; tidak ada array contoh | `PASS` |
| Browser setelah login | Auth lokal `401` | `EXISTING / ENVIRONMENT ISSUE` |

## 5. Acceptance criteria dan penutup

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Radar menampilkan staf sebenarnya dari server | Terpenuhi secara source | `getExpiringRadar` dan `training-certificates-view.jsx` |
| Migrasi base component | Terpenuhi pada 13 berkas target | CSS gate 0 temuan |
| Sertifikat H-30 terbukti di browser | Belum | Login localhost gagal |

Langkah berikutnya: pulihkan sesi test dan verifikasi radar dengan data H-30 serta kontrol permission. Perubahan sampingan: `NONE`.
