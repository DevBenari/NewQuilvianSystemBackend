# Laporan Perubahan Frontend — `FE-HRD-R02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R02` |
| Judul | Service K3RS jujur dan tersambung |
| Slice | `S-C6` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R02` |
| Trace | Task asal `FE-HRD-261`, `262`–`271`; Kelompok C audit |
| Contract version | API Contract v23 bagian 29 |
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

`occupational-health-service.js` memakai `axios` polos tanpa sesi login dan 17 blok `catch` mengembalikan data contoh atau `success: true`. Slice `occupationalHealthSlice.js` diisi data contoh `INITIAL_*`, sehingga layar tampak penuh walau server menolak.

---

## 2. Proses bisnis dari sisi pengguna

Dokter K3RS membuka Dasbor K3RS. Kartu kini membaca `GET dashboard/executive`; contoh nyata di uji: "Total Pegawai Terdata K3RS" bernilai 34 dari server, bukan 486 contoh. Bila ia menilai kelaikan kerja dan server menolak, layar menampilkan pesan gagal, bukan "berhasil". Saat membuka Pembatasan Tugas Medis, layar menulis "Daftar pembatasan kerja belum tersedia di server."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service, slice, enam view K3RS, `NeedleStickPanicButton.jsx`, controller `HealthRecordsController.cs` dan kawan-kawan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/occupational-health-service.js` | Pindah ke `InstanceAxios`, hapus data contoh di `catch`, alamat `/v1/hrd/occupational-health`, normalisasi daftar, penanda belum tersedia untuk empat daftar, pemetaan nama kolom dasbor |
| `src/lib/state/slice/hr/occupational-health/occupationalHealthSlice.js` | State awal kosong, bukan data contoh |
| `src/components/view/hr/occupational-health/*.jsx (6 view)` | Panggilan service dibungkus `try/catch` dengan `InformationAlert`; dasbor membaca kolom server |
| `src/components/view/hr/occupational-health/ohs-mcu-view.jsx` | Angka cadangan 120/108 diganti 0 |
| `src/components/view/hr/occupational-health/widget/NeedleStickPanicButton.jsx` | Pesan galat dari `getHrApiErrorMessage` |
| `src/utils/hr/hr-api-utils.js` | Helper bersama baru |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → service → `InstanceAxios`. Klien `axios` polos dan `axios.create` dengan token `localStorage` dihapus, karena sesi aplikasi memakai cookie dan token itu tidak pernah ditulis. Helper bersama `src/utils/hr/hr-api-utils.js` memakai ulang `getWorkforceCoreErrorMessage` yang sudah ada, ditambah penanda data belum tersedia.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tidak berubah; tampilan modul ini dimigrasikan pada `FE-HRD-R14`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Pesan galat dan data belum tersedia | `InformationAlert` | `REUSE` | Pesan dari `getHrApiErrorMessage`, bukan pesan Axios |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

Elemen lain pada layar belum dimigrasikan; itu cakupan Gelombang 2.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hrd/occupational-health/dashboard` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/13-k3rs-dasbor.png` | Kartu membaca data server |
| `/hrd/occupational-health/restrictions` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/14-k3rs-pembatasan-kerja.png` | Pesan data belum tersedia, tabel kosong |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sama seperti sebelumnya |
| Kosong | Tabel kosong dengan kalimat bawaan layar; tidak ada lagi baris contoh |
| Gagal | `InformationAlert` merah berisi pesan backend atau terjemahan kode HTTP |
| Data belum tersedia | Pesan seperti "Daftar pembatasan kerja belum tersedia di server." |
| Tanpa hak akses | Pesan "Anda tidak punya hak untuk melakukan tindakan ini." dari kode 403 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Occupational Health & Medical Privacy

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/occupational-health/dashboard/executive` | Kartu dasbor K3RS | `OccupationalHealthRecord : ViewSummary` |
| `PUT` | `/v1/hrd/occupational-health/records/{employeeId}/fitness-status` | Penilaian kelaikan kerja | `OccupationalHealthRecord : AssessFitnessToWork` |

#### Corporate - HRD Work Restriction & Injury BPJS JKK

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/hrd/occupational-health/restrictions` | Terbitkan pembatasan kerja | `WorkRestrictionAndInjury : IssueWorkRestriction` |
| `POST` | `/v1/hrd/occupational-health/restrictions/{id}/return-to-work` | Asesmen kembali bekerja | `WorkRestrictionAndInjury : AssessReturnToWork` |

Daftar rekam kesehatan, pembatasan kerja, vaksinasi, dan kecelakaan kerja seluruh pegawai berstatus `Rencana (belum tersedia)` dan dicatat pada `FE-HRD-R53`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend (`[Route]` + `[Http*]`) | 13 dari 13 panggilan cocok; 4 daftar ditandai belum tersedia | `PASS` | Skrip pembanding route backend |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`, akun SuperAdmin) | Kartu membaca data server; Pesan data belum tersedia, tabel kosong | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/13-k3rs-dasbor.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/14-k3rs-pembatasan-kerja.png` |

Uji manual: `PASS` untuk layar yang dibuka. Tombol simpan tidak ditekan agar tidak menulis data ke database pengembangan.

**Tidak dijalankan:** uji galat 403 dan 500 sungguhan dari server; jalurnya dibuktikan lewat kode (tidak ada blok `catch` yang mengembalikan berhasil).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1: service memakai `InstanceAxios` | Terpenuhi | `occupational-health-service.js` |
| `AC-S` 2: tidak ada `catch` yang mengembalikan data contoh atau `success: true` | Terpenuhi | `grep "success: true"` kosong pada service |
| `AC-S` 3: alamat cocok backend; yang belum ada ditandai | Terpenuhi | 13 dari 13 panggilan cocok; 4 daftar ditandai belum tersedia |
| `assessFitnessToWork` meneruskan galat | Terpenuhi | Tidak ada `catch` di service; view menampilkan pesan lewat `setApiError` |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Endpoint daftar lintas-pegawai belum ada di backend (`DEC-FER-05`, `FE-HRD-R53`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R14` menyambungkan sisa layar K3RS dan memigrasikannya ke base component |
