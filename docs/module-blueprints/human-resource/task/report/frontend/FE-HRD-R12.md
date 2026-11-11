# Laporan Perubahan Frontend — `FE-HRD-R12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R12` |
| Judul | Bangun `S-D1` MVP-7: Pengaturan, Formasi, Batch Formasi |
| Slice | `S-D1` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R12` |
| Trace | Task asal `FE-HRD-029`, `FE-HRD-030`, `FE-HRD-031` |
| Contract version | API Contract v7 bagian 13 |
| Wewenang UI | Mengikuti kartu asal |
| Dependency | — |
| Klasifikasi | `HEAVY` — beberapa layar baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — `FE-HRD-029` ✅; `FE-HRD-030` dan `031` menunggu data uji |

---

## 1. Keadaan yang ditemukan di awal

Seluruh task asal belum dikerjakan, padahal backend-nya sudah ✅.

---

## 2. Proses bisnis dari sisi pengguna

Diuraikan per layar pada laporan task asal.

---

## 3. Perubahan yang dikerjakan

Task ini adalah paket. Rincian berkas, gerbang tampilan, dan screenshot ada di laporan task asal:

| Task asal | Status | Laporan |
| --- | --- | --- |
| `FE-HRD-029` | ✅ | [FE-HRD-029.md](FE-HRD-029.md) |
| `FE-HRD-030` | 🟡 | [FE-HRD-030.md](FE-HRD-030.md) |
| `FE-HRD-031` | 🟡 | [FE-HRD-031.md](FE-HRD-031.md) |

---

## 4. State yang ditangani di layar

Lihat laporan task asal.

---

## 5. Endpoint yang dikonsumsi

Lihat laporan task asal.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |

Uji manual: `PASS` untuk layar yang dibuka; rincian di laporan task asal.

**Tidak dijalankan:** alur yang menulis data; lihat laporan task asal.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh kriteria kartu asal terpenuhi | Sebagian | Laporan task asal |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R26` dan `FE-HRD-R27` |
