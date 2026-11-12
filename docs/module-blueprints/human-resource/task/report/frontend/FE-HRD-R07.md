# Laporan Perubahan Frontend — `FE-HRD-R07`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R07` |
| Judul | Service OPPE jujur dan tersambung |
| Slice | `S-C1-OPPE` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R07` |
| Trace | Task asal `FE-HRD-201`; Kelompok C dan E audit |
| Contract version | API Contract v19 bagian 25 |
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

`clinical-performance-service.js` memakai token yang tidak ada, 10 blok `catch` mengembalikan daftar kosong seolah berhasil, dan seluruh alamatnya `/clinical-performance/...` tidak ada di backend.

---

## 2. Proses bisnis dari sisi pengguna

Belum ada layar yang memakai service ini; layar OPPE disambungkan pada `FE-HRD-R20`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service dan controller `OppeOngoingEvaluationController.cs`, `FppeFocusedEvaluationController.cs`, `ClinicalPeerReviewAuditController.cs`, `RecredentialingQualityController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/clinical-performance-service.js` | Pindah ke `InstanceAxios`; alamat dipetakan ke grup `oppe`, `fppe`, `peer-review-audit`, dan `credentialing/recredentialing`; rapor OPPE per dokter, pengubahan bobot KSM, dan daftar sidang pleno ditandai belum tersedia |

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

Grup `[Tags]`: `Corporate - HRD OPPE Clinical Performance Evaluation`, `Corporate - HRD FPPE Incident & Focused Evaluation`, `Corporate - HRD Peer Review Confidentiality & Audit Log`, `Corporate - HRD Recredentialing Quality & Composite Scorecard`. Rincian ada pada Lampiran C `frontend-unfinished-work.md`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend | 16 dari 16 panggilan cocok; 3 method ditandai belum tersedia | `PASS` | Skrip pembanding route backend |

Uji manual: `NOT APPLICABLE` — belum ada layar pemakai.

**Tidak dijalankan:** uji browser.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1–3 | Terpenuhi | Service memakai `InstanceAxios`, tanpa `catch` palsu, alamat cocok |
| 13 panggilan yang punya padanan cocok dengan backend | Terpenuhi | 16 cocok, termasuk 3 padanan yang ditemukan sesudah audit |
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
| Langkah berikutnya | `FE-HRD-R20` |
