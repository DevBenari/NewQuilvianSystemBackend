# Laporan Perubahan Frontend — `FE-HRD-R06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R06` |
| Judul | Perbaikan alamat Kredensial dan Layanan SDM |
| Slice | `S-C1`, `S-D4` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R06` |
| Trace | Task asal `FE-HRD-186`, `172`; Kelompok E audit |
| Contract version | API Contract v18 bagian 24, v17 bagian 23 |
| Wewenang UI | Tidak ada wewenang tampilan baru; hanya penanganan galat dan pengosongan data contoh |
| Dependency | — |
| Klasifikasi | `LIGHT` — perubahan alamat service tanpa tampilan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Seluruh 58 panggilan `credentialing-service.js` dan `hr-service.js` kurang segmen `/v1`, sehingga backend selalu menjawab 404.

---

## 2. Proses bisnis dari sisi pengguna

Belum ada layar yang memakai kedua service ini; layar Kredensial dan Layanan SDM disambungkan pada `FE-HRD-R18` dan `FE-HRD-R19`. Dengan alamat yang benar, panggilan pertama dari layar itu langsung sampai ke backend.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kedua service dan controller `Credentialing*`, `HrService*`, `HrPublicDocumentVerificationController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/credentialing-service.js` | Tiga konstanta alamat diberi `/v1` |
| `src/lib/services/hr/hr-service.js` | Lima konstanta alamat diberi `/v1`, termasuk `ESS_TICKETS_URL` dan `PUBLIC_VERIFY_URL` |

### 3.3 Kepatuhan arsitektur frontend

Service tetap memakai `InstanceAxios` dan pola `unwrap` yang sudah ada. Alamat yang belum ada di backend melempar galat `createHrUnavailableError`, sehingga layar pemanggil menampilkan "belum tersedia" alih-alih galat 404.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task hanya mengubah service

`UI GATE: N/A — task hanya mengubah service`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — tidak ada perubahan layar.

---

## 5. Endpoint yang dikonsumsi

Base URL baru: `/v1/corporate/hrd/credentialing`, `/v1/corporate/hrd/clinical-privileges`, `/v1/corporate/hrd/services`, `/v1/self-service/hr/services/tickets`, `/v1/public/hr-documents`. Rincian per grup `[Tags]` ada pada Lampiran C `frontend-unfinished-work.md`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend | 58 dari 58 panggilan cocok | `PASS` | Skrip pembanding route backend |

Uji manual: `NOT APPLICABLE` — belum ada layar pemakai.

**Tidak dijalankan:** uji browser, karena belum ada layar pemakai.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1–3 | Terpenuhi | Kedua service memakai `InstanceAxios`, tanpa `catch` palsu, alamat cocok |
| 58 panggilan cocok dengan backend | Terpenuhi | Pencocokan route backend |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

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
| Langkah berikutnya | `FE-HRD-R18` dan `FE-HRD-R19` |
