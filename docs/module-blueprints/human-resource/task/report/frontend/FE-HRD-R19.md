# Laporan Perubahan Frontend — `FE-HRD-R19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R19` |
| Judul | Layanan SDM bagian 1: katalog dan helpdesk |
| Slice / kontrak | `S-D4`; API Contract v17 bagian 23 (`approved`) |
| Trace | `FE-HRD-173`–`177` |
| Dependency | `FE-HRD-R06` ✅ |
| Commit | FE `ec3616405`; BE `7b2b15ccd` (read-only) |
| Tanggal / status | 16 November 2026 / 🟡 **SEBAGIAN** |

## Hasil

Tiga layar target—katalog, editor templat, dan antrean helpdesk—telah memakai `hr-service-slice.js` serta service Axios nyata. Target SLA dikirim ke server saat kategori/jenis layanan disimpan; antrean dihitung dari `requestStatus`, `priorityTier`, dan `slaStatus` server. Penugasan ulang memakai `POST tickets/{id}/assign`, bukan mutasi lokal atau fallback sukses.

Daftar akun analis lintas organisasi belum disediakan kontrak server. UI secara jujur menawarkan analis yang telah muncul dalam antrean dan menjelaskan keterbatasan tersebut; perluasan directory analyst tetap menjadi scope kontrak lanjutan, bukan data contoh.

## UI gate dan endpoint

`UI GATE: PASS — REUSE Hero/SummaryGrid/DataTable/BaseButton/BaseFormControl/ConfirmModal/InformationAlert/ToastStack; COMPOSE CSS Module existing; 0 NEW.`

```text
3 berkas target; 0 raw button/input/select/textarea/table; 0 Tailwind utility.
```

| Method/path | Kegunaan |
| --- | --- |
| `GET/POST/PUT/DELETE services/categories` | Kategori dan SLA bawaan |
| `GET/POST/PUT/DELETE services/types` | Jenis layanan dan SLA respons/resolusi |
| `GET/POST/PUT services/document-types` | Templat naskah/nomor dokumen |
| `GET services/tickets/queue` | Antrean dan KPI helpdesk |
| `POST services/tickets/{id}/assign` | Ambil/tugaskan ulang tiket |

## Verifikasi dan acceptance

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run lint` | PASS — 0 errors; 1252 warning existing |
| `npm run build` | PASS — 814 static pages |
| Source/API/base component | PASS |
| Target SLA tersimpan melalui API | Terpenuhi secara source |
| Browser terautentikasi | Belum — `/api/v1/auth/me` lokal 401 |

Status belum dinaikkan ke ✅ sampai katalog, penyimpanan SLA, dan penugasan tiket dibuktikan pada Chromium dengan akun berizin. Perubahan sampingan: `NONE`.
