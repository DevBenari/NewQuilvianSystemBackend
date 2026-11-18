# Laporan Perubahan Frontend — `FE-HRD-R21`

## Metadata

| Field | Nilai |
| --- | --- |
| Task / judul | `FE-HRD-R21` — Persetujuan bagian 1: kotak masuk, detail, aksi |
| Slice / trace | `S-A7-V2`; `FE-HRD-277`–`283` |
| Kontrak / dependency | API Contract v21 bagian 27 (`approved`); `FE-HRD-R08` ✅ |
| Commit | FE `ec3616405`; BE `7b2b15ccd` (read-only) |
| Tanggal / status | 16 November 2026 / 🟡 **SEBAGIAN** |

## Hasil

Kotak masuk, detail, stepper, serta modal setuju/tolak/kembalikan/bulk sudah memakai `UnifiedApprovalService` nyata. Tombol hanya tampil sesuai permission; validasi PIN dan alasan penolakan dijalankan sebelum request. Tab Plh, deadline SLA, summary, dan hasil bulk berasal dari DTO server. Tidak ditemukan daftar approval contoh atau keberhasilan lokal palsu.

## UI gate dan verifikasi

`UI GATE: PASS — 7 berkas target, 0 raw form/table/button, 0 Tailwind; seluruh dialog memakai backdrop base modal.`

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run lint` | PASS — 0 errors; 1252 warning existing |
| `npm run build` | PASS — 814 static pages |
| Approve/reject/return/bulk | Service nyata; payload mengikuti DTO kontrak |
| Maker-checker/Plh | Ditentukan server dan ditampilkan dari respons |
| Browser terautentikasi | Belum — `/api/v1/auth/me` lokal 401 |

Acceptance terpenuhi secara source, termasuk alasan tolak minimal dan PIN signature. Bukti visual aksi lintas peran belum tersedia, sehingga status tetap 🟡. Perubahan sampingan: `NONE`.
