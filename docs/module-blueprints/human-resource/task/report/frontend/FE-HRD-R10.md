# Laporan Perubahan Frontend — `FE-HRD-R10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R10` |
| Judul | Perbaikan alamat Onboarding, ESS, dan Administrasi |
| Slice | `S-C4-ONB`, `S-A1` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R10` |
| Trace | Task asal `FE-HRD-058`, `026-ESS`; Kelompok E audit |
| Contract version | API Contract v9 bagian 15, v6 |
| Wewenang UI | Tidak ada wewenang tampilan baru; hanya penanganan galat dan pengosongan data contoh |
| Dependency | — |
| Klasifikasi | `LIGHT` — perubahan alamat service tanpa tampilan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — alamat cocok backend; tombol yang memakainya belum diuji di browser karena setiap tombol menulis data |

---

## 1. Keadaan yang ditemukan di awal

Sepuluh alamat `lifecycle-service.js`, tiga alamat `employee-self-service.js`, dan satu alamat `workforce-core-cross-employee-service.js` tidak ada di backend, misalnya `employment-contracts/{id}/decision`.

---

## 2. Proses bisnis dari sisi pengguna

HR mencatat keputusan kontrak, hadir hari pertama, atau penilaian masa percobaan. Sebelumnya tombol itu gagal 404; kini alamatnya cocok dengan backend. Klaim lembur dan jadwal milik sendiri di layanan mandiri menampilkan "belum tersedia di server" karena backend memang belum menyediakannya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Ketiga service dan controller lifecycle, layanan mandiri, serta perubahan data pegawai.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/lifecycle-service.js` | Sembilan alamat diganti padanan backend, misalnya `decisions`, `corrections`, `declare-independent`, `PUT preceptor`, `probations/{id}/assessments`; unggah berkas batch ditandai belum tersedia |
| `src/lib/services/hr/employee-self-service.js` | Klaim lembur dan jadwal milik sendiri ditandai belum tersedia |
| `src/lib/services/hr/workforce-core-cross-employee-service.js` | `verify/start` menjadi `start-verification` |

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

Grup `[Tags]` lifecycle, `Self Services / Human Resource / Overtime`, dan `Corporate / Human Resource / Workforce Core / Employee Profile Change Administration`. Rincian ada pada Lampiran C `frontend-unfinished-work.md`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend | lifecycle 34 dari 34 (1 belum tersedia), ESS 21 dari 21 (3 belum tersedia), Administrasi 12 dari 12 | `PASS` | Skrip pembanding route backend |

Uji manual: `NOT RUN` — tombolnya menulis data ke database pengembangan.

**Tidak dijalankan:** uji tombol keputusan kontrak, hadir hari pertama, masa percobaan, dan pendampingan klinis; bentuk isian (DTO) belum dicocokkan di browser.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1–3 | Terpenuhi | Pencocokan route backend |
| Contoh `employment-contracts/{id}/decision` menjadi `.../decisions` | Terpenuhi | `submitContractDecision` |
| Outcome: tombol yang kini gagal 404 mulai berfungsi | Belum terbukti | Belum diuji di browser |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Bentuk isian bisa berbeda dari DTO backend; diperiksa saat `FE-HRD-R25` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R25` menguji tombol itu bersama migrasi layar Onboarding |
