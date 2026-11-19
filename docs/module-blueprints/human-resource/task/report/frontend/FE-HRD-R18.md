# Laporan Perubahan Frontend — `FE-HRD-R18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R18` |
| Judul | Kredensial bagian 1: ESS, PSV, radar, katalog |
| Slice | `S-C1` |
| Trace | API Contract v18 bagian 24; `FE-HRD-187`–`190` |
| Contract version | v18, `approved` |
| Dependency | `FE-HRD-R06` ✅ |
| Klasifikasi | `EPIC` — 5 berkas target layar/modal Kredensial |
| Task mode | `FRONTEND` |
| Commit frontend/backend | FE `ec3616405` (`HamzahV2`); BE `7b2b15ccd` (`MHamzah`, read-only) |
| Tanggal | 16 November 2026 |
| Status | 🟡 **SEBAGIAN** — source target dan validasi statis lulus; browser belum terautentikasi |

## 1. Hasil implementasi

Source target sudah tersambung ke Redux/service nyata untuk pengajuan kredensial, verifikasi PSV, radar lisensi, katalog whitebook, serta perpanjangan lisensi. Tidak ditemukan daftar antrean contoh atau fallback sukses palsu. Radar SIP memanggil endpoint server `licenses/radar/expiring` dan seluruh aksi tulis tetap melalui thunk/service.

## 2. Gerbang tampilan

| Elemen | Keputusan | Sumber |
| --- | --- | --- |
| Header/ringkasan/aksi | `REUSE` | `Hero`, `SummaryGrid`, `BaseButton` |
| Filter/tabel/status | `REUSE` | `DataFilter`, `DataTable`, `StatusBadge` |
| Form/dialog | `REUSE` | `BaseFormControl`, `ConfirmModal` |
| Alert/toast | `REUSE` | `InformationAlert`, `ToastStack` |
| Layout | `COMPOSE` | CSS Module existing bertoken |

`UI GATE: PASS — 4 kelompok REUSE, 1 COMPOSE, 0 NEW.`

```text
OK — 5 berkas diperiksa, 0 temuan.
NO_TAILWIND_MATCHES
NO_RAW_FORM_OR_TABLE_ELEMENTS
```

`VISUAL CHECK: NOT FEASIBLE — /api/v1/auth/me localhost menghasilkan 401 pada Chromium headed.`

## 3. State dan endpoint

State loading, kosong, gagal, permission, dan aksi klinis memakai komponen baku. Alur yang diperiksa memakai endpoint nyata berikut.

| Method | Path | Kegunaan |
| --- | --- | --- |
| `GET/POST` | `/api/v1/corporate/hrd/credentialing/applications` | Pengajuan kredensial |
| `GET` | `/api/v1/corporate/hrd/credentialing/licenses/employee/{id}` | Lisensi per pegawai |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id}/verify-psv` | Verifikasi sumber primer |
| `GET` | `/api/v1/corporate/hrd/credentialing/licenses/radar/expiring` | Radar masa berlaku SIP |
| `GET` | `/api/v1/corporate/hrd/credentialing/whitebook` | Katalog whitebook |

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint` | 0 errors, 1252 warnings existing | `PASS` |
| `npm run build` | Compiled, 814 static pages, standalone siap | `PASS` |
| CSS gate target | 5 berkas, 0 temuan | `PASS` |
| Data contoh/fallback sukses | Tidak ditemukan pada target | `PASS` |
| Browser setelah login | Auth lokal `401` | `EXISTING / ENVIRONMENT ISSUE` |

## 5. Acceptance criteria dan penutup

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Radar SIP membaca data server | Terpenuhi secara source | `fetchRadarExpiringLicenses` dan service `/licenses/radar/expiring` |
| ESS, PSV, dan whitebook memakai API nyata | Terpenuhi secara source | thunk/service pada empat layar target |
| Uji per peran komite di browser | Belum | Sesi localhost gagal pada `/auth/me` |

Langkah berikutnya: pulihkan akun/sesi test, lalu verifikasi pemisahan hak dokter, verifier PSV, dan komite menggunakan data non-produksi. Perubahan sampingan: `NONE`.
