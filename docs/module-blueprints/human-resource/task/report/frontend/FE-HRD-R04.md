# Laporan Perubahan Frontend — `FE-HRD-R04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R04` |
| Judul | Service Kinerja jujur dan tersambung |
| Slice | `S-C3` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R04` |
| Trace | Task asal `FE-HRD-231`–`244`; Kelompok C audit |
| Contract version | API Contract v21 bagian 27 |
| Wewenang UI | Tidak ada wewenang tampilan baru; hanya penanganan galat dan pengosongan data contoh |
| Dependency | — |
| Klasifikasi | `MEDIUM` — banyak berkas, pola seragam, tanpa perubahan tampilan |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

`performance-service.js` memakai token yang tidak ada dan 26 blok `catch` jawaban palsu. Dasbor Kinerja menulis angka tetap di markup: rata-rata 81,40, 3 pegawai PIP, H-14, dan sebaran 12/76/8/4%.

---

## 2. Proses bisnis dari sisi pengguna

Kepala bidang membuka Dasbor Kinerja. Total peserta, rata-rata skor, jumlah PIP, dan tenggat kalibrasi dihitung dari data server; bila datanya belum ada, kartu menampilkan "—". Sebaran kalibrasi menampilkan "Sebaran kalibrasi belum tersedia untuk siklus ini." bila siklus belum punya data.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service, slice, delapan view Kinerja, `PerformanceCalibrationController.cs` dan controller kinerja lain, DTO `PerformanceCycleDto`, `CalibrationDistributionMatrixDto`, `PerformanceAppraisalDto`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/performance-service.js` | Pindah ke `InstanceAxios`, alamat `pip/active-list`, `pip/issue-contract`, `disputes/submit`, `disputes/{id}/adjudicate`; sebaran kalibrasi meminta `cycleId`; dua daftar ditandai belum tersedia |
| `src/lib/state/slice/hr/performance/performanceManagementSlice.js` | State awal kosong |
| `src/components/view/hr/performance/performance-dashboard-view.jsx` | Angka tetap diganti hitungan dari data server atau "—"; nama kolom siklus disesuaikan DTO |
| `src/components/view/hr/performance/performance-calibration-view.jsx` | Sebaran tetap diganti data server |
| `src/components/view/hr/performance/*.jsx (6 view lain)` | Panggilan dibungkus `try/catch` |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → service → `InstanceAxios`. Klien `axios` polos dan `axios.create` dengan token `localStorage` dihapus, karena sesi aplikasi memakai cookie dan token itu tidak pernah ditulis. Helper bersama `src/utils/hr/hr-api-utils.js` memakai ulang `getWorkforceCoreErrorMessage` yang sudah ada, ditambah penanda data belum tersedia.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tidak berubah; tampilan modul ini dimigrasikan pada `FE-HRD-R16`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Pesan galat dan data belum tersedia | `InformationAlert` | `REUSE` | Pesan dari `getHrApiErrorMessage`, bukan pesan Axios |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

Elemen lain pada layar belum dimigrasikan; itu cakupan Gelombang 2.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/corporate/hrd/performance/dashboard` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/17b-kinerja-dasbor-tanpa-angka-contoh.png` | Tanpa angka contoh |
| `/corporate/hrd/performance/calibration` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/17c-kinerja-kalibrasi.png` | Sebaran dari server atau keterangan belum tersedia |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sama seperti sebelumnya |
| Kosong | Tabel kosong dengan kalimat bawaan layar; tidak ada lagi baris contoh |
| Gagal | `InformationAlert` merah berisi pesan backend atau terjemahan kode HTTP |
| Data belum tersedia | Pesan seperti "Daftar sanggahan nilai belum tersedia di server." |
| Tanpa hak akses | Pesan "Anda tidak punya hak untuk melakukan tindakan ini." dari kode 403 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Performance Cycles & Goal Cascading

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/hrd/performance/cycles` | Siklus aktif | `PerformanceCycleGoal : ViewCycle` |

Daftar jenjang karier klinis dan daftar sanggahan nilai berstatus `Rencana (belum tersedia)` (`FE-HRD-R53`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend (`[Route]` + `[Http*]`) | 24 dari 24 panggilan cocok; 2 daftar ditandai belum tersedia | `PASS` | Skrip pembanding route backend |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`, akun SuperAdmin) | Tanpa angka contoh; Sebaran dari server atau keterangan belum tersedia | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/17b-kinerja-dasbor-tanpa-angka-contoh.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/17c-kinerja-kalibrasi.png` |

Uji manual: `PASS` untuk layar yang dibuka. Tombol simpan tidak ditekan agar tidak menulis data ke database pengembangan.

**Tidak dijalankan:** uji galat 403 dan 500 sungguhan dari server; jalurnya dibuktikan lewat kode (tidak ada blok `catch` yang mengembalikan berhasil).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1: service memakai `InstanceAxios` | Terpenuhi | `performance-service.js` |
| `AC-S` 2: tidak ada `catch` yang mengembalikan data contoh atau `success: true` | Terpenuhi | `grep "success: true"` kosong pada service |
| `AC-S` 3: alamat cocok backend; yang belum ada ditandai | Terpenuhi | 24 dari 24 panggilan cocok; 2 daftar ditandai belum tersedia |
| Alamat `pip/active`, `pip/contract`, `disputes/submit`, `disputes/{id}/adjudicate` mengikuti Lampiran C | Terpenuhi | Pencocokan route backend |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Endpoint daftar jenjang karier dan sanggahan belum ada (`DEC-FER-05`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R16` menyambungkan dan memigrasikan layar Kinerja |
