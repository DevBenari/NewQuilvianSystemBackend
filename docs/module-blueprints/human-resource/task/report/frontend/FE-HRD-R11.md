# Laporan Perubahan Frontend — `FE-HRD-R11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R11` |
| Judul | Perbaikan alamat Lembur dan Benefit |
| Slice | `S-B3`, `S-D3` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R11` |
| Trace | Task asal `FE-HRD-116`, `158`; Kelompok E audit |
| Contract version | API Contract v13 bagian 19, v16 bagian 22 |
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

Enam alamat `overtime-service.js` dan tujuh alamat `benefits-service.js` tidak ada di backend; klaim COB memanggil `/hr-benefits` alih-alih `/hr-benefits/claims`.

---

## 2. Proses bisnis dari sisi pengguna

Layar Lembur (`FE-HRD-R23`) dan klaim COB (`FE-HRD-R24`) akan memakai service ini. Daftar rencana dan realisasi lembur kini memanggil grup `overtime-management` yang memang menyediakan daftar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kedua service dan controller `OvertimePlan*`, `OvertimeRealization*`, `BenefitClaimController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/overtime-service.js` | Daftar, ubah, validasi, dan terbitkan rencana serta daftar realisasi ke `overtime-management`; unggah logbook ditandai belum tersedia |
| `src/lib/services/hr/benefits-service.js` | Klaim COB ke `/hr-benefits/claims`; analitik ditandai belum tersedia |

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

Grup `[Tags]`: `Corporate / Human Resource / Overtime Management / Overtime Plan`, `Corporate / Human Resource / Overtime Management / Overtime Realization`, `HR Benefits - COB Claims & Settlement`. Rincian ada pada Lampiran C `frontend-unfinished-work.md`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend | lembur 28 dari 28 (1 belum tersedia), benefit 25 dari 25 (1 belum tersedia) | `PASS` | Skrip pembanding route backend |

Uji manual: `NOT APPLICABLE` — layar pemakai disambungkan pada Gelombang 2.

**Tidak dijalankan:** uji browser.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1–3 | Terpenuhi | Pencocokan route backend |
| Klaim memakai grup `hr-benefits/claims` | Terpenuhi | `CLAIMS_URL` |
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
| Langkah berikutnya | `FE-HRD-R23` dan `FE-HRD-R24` |
