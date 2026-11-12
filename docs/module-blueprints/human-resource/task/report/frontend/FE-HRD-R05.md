# Laporan Perubahan Frontend — `FE-HRD-R05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R05` |
| Judul | Service Pelatihan jujur dan tersambung |
| Slice | `S-C2` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R05` |
| Trace | Task asal `FE-HRD-216`–`229`; Kelompok C audit |
| Contract version | API Contract v20 bagian 26 |
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

`learning-service.js` memakai token yang tidak ada dan 22 blok `catch` jawaban palsu, termasuk radar berisi staf karangan. Dasbor Pelatihan menulis angka tetap: kepatuhan 94,8%, 1.248 sertifikat, 18 radar, 3 safeguard, dan lima kartu kepatuhan 91,5–97,5%. Rencana Tahunan menulis pagu Rp450 juta.

---

## 2. Proses bisnis dari sisi pengguna

Bagian Diklat membuka Dasbor Pelatihan. Radar kedaluwarsa dan jumlah sertifikat dipantau dibaca dari server; angka yang belum disediakan backend tampil "—" dengan keterangan "belum tersedia di server". Layar yang butuh identitas pegawai (sertifikat saya, ikatan dinas, SKP) menjelaskan bahwa identitas pegawai belum tersedia, alih-alih memanggil server dengan "ME".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service, slice, sembilan view dan lima modal Pelatihan, controller `MandatoryHospitalTrainingController.cs`, `TrainingBondingController.cs`, `PlataranSehatSkpController.cs`, DTO `CertificateExpiryRadarSummaryDto`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/learning-service.js` | Pindah ke `InstanceAxios`, alamat `training/*` untuk ikatan dinas, SKP, dan sertifikat; ringkasan radar ikut dikembalikan; default `"ME"` dihapus |
| `src/lib/state/slice/hr/learning/learningDevelopmentSlice.js` | Aturan wajib awal kosong |
| `src/components/view/hr/learning/learning-dashboard-view.jsx` | Angka tetap dan lima kartu kepatuhan contoh diganti data server atau keterangan belum tersedia |
| `src/components/view/hr/learning/annual-plans-view.jsx` | Pagu anggaran contoh diganti "—"; daftar anggaran contoh dikosongkan |
| `src/components/view/hr/learning/*.jsx (7 view lain) dan modal/*.jsx (4 modal)` | Panggilan dibungkus `try/catch`; data contoh peserta dan presensi dikosongkan; nilai cadangan diganti 0 |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → service → `InstanceAxios`. Klien `axios` polos dan `axios.create` dengan token `localStorage` dihapus, karena sesi aplikasi memakai cookie dan token itu tidak pernah ditulis. Helper bersama `src/utils/hr/hr-api-utils.js` memakai ulang `getWorkforceCoreErrorMessage` yang sudah ada, ditambah penanda data belum tersedia.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tidak berubah; tampilan modul ini dimigrasikan pada `FE-HRD-R17`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Pesan galat dan data belum tersedia | `InformationAlert` | `REUSE` | Pesan dari `getHrApiErrorMessage`, bukan pesan Axios |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

Elemen lain pada layar belum dimigrasikan; itu cakupan Gelombang 2.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/corporate/hrd/learning/dashboard` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/18b-pelatihan-dasbor-tanpa-angka-contoh.png` | Tanpa angka contoh |
| `/corporate/hrd/learning/annual-plans` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/18c-pelatihan-rencana-tahunan.png` | Pagu tampil "—" |
| `/corporate/hrd/learning/mandatory-rules` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/19-pelatihan-aturan-wajib.png` | Daftar dari server |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sama seperti sebelumnya |
| Kosong | Tabel kosong dengan kalimat bawaan layar; tidak ada lagi baris contoh |
| Gagal | `InformationAlert` merah berisi pesan backend atau terjemahan kode HTTP |
| Data belum tersedia | Pesan seperti "Sertifikat pegawai belum dapat dimuat karena identitas pegawai belum tersedia di layar ini." |
| Tanpa hak akses | Pesan "Anda tidak punya hak untuk melakukan tindakan ini." dari kode 403 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Mandatory Hospital Training & Certification

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/hrd/learning/certificates/expiring-radar` | Radar kedaluwarsa dan ringkasannya | `MandatoryHospitalTraining : Read` |
| `GET` | `/v1/corporate/hrd/learning/mandatory-rules` | Daftar aturan pelatihan wajib | `MandatoryHospitalTraining : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend (`[Route]` + `[Http*]`) | 22 dari 22 panggilan cocok | `PASS` | Skrip pembanding route backend |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`, akun SuperAdmin) | Tanpa angka contoh; Pagu tampil "—"; Daftar dari server | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/18b-pelatihan-dasbor-tanpa-angka-contoh.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/18c-pelatihan-rencana-tahunan.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/19-pelatihan-aturan-wajib.png` |

Uji manual: `PASS` untuk layar yang dibuka. Tombol simpan tidak ditekan agar tidak menulis data ke database pengembangan.

**Tidak dijalankan:** uji galat 403 dan 500 sungguhan dari server; jalurnya dibuktikan lewat kode (tidak ada blok `catch` yang mengembalikan berhasil).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1: service memakai `InstanceAxios` | Terpenuhi | `learning-service.js` |
| `AC-S` 2: tidak ada `catch` yang mengembalikan data contoh atau `success: true` | Terpenuhi | `grep "success: true"` kosong pada service |
| `AC-S` 3: alamat cocok backend; yang belum ada ditandai | Terpenuhi | 22 dari 22 panggilan cocok |
| Alamat `bonds` dan `skp` beralih ke grup `training` | Terpenuhi | Pencocokan route backend |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Persentase kepatuhan per pelatihan dan jumlah safeguard belum disediakan backend |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R17` menyambungkan dan memigrasikan layar Pelatihan |
