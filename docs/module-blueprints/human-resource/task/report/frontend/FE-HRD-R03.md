# Laporan Perubahan Frontend — `FE-HRD-R03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R03` |
| Judul | Service Hubungan Industrial jujur dan tersambung |
| Slice | `S-C5` |
| Roadmap | `roadmap/frontend-unfinished-work.md` kartu `FE-HRD-R03` |
| Trace | Task asal `FE-HRD-246`, `248`–`260`; Kelompok C audit |
| Contract version | API Contract v22 bagian 28 |
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

`employee-relation-service.js` memakai `axios.create` dengan token yang tidak pernah ada dan 20 blok `catch` jawaban palsu. View memanggil 17 nama method yang tidak ada di service, misalnya `createSanction` dan `getBipartiteMeetings`, sehingga tombolnya gagal diam-diam.

---

## 2. Proses bisnis dari sisi pengguna

Petugas HR membuka Dasbor Hubungan Industrial. Insiden, keluhan, dan PHK dibaca dari server. Daftar sanksi dan skorsing seluruh pegawai belum ada di backend, jadi layar Surat Peringatan menulis "Daftar sanksi seluruh pegawai belum tersedia di server." Penerbitan SP yang ditolak server menampilkan pesan gagal dan tidak menambah baris.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Service, slice, delapan view Hubungan Industrial, controller `EmployeeDisciplinaryController.cs`, `EmployeeGrievanceController.cs`, `BipartiteForumController.cs`, `EmployeeSeparationController.cs`, `MultiDeptExitClearanceController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/employee-relation-service.js` | Pindah ke `InstanceAxios`, hapus jawaban palsu, alamat `/v1/hrd/...`, method `approveSeveranceFinance` ditambahkan, empat method ditandai belum tersedia |
| `src/lib/state/slice/hr/relations/employeeRelationSlice.js` | State awal kosong |
| `src/components/view/hrd/relations/*.jsx (8 view)` | 13 nama method diselaraskan dengan service; panggilan dibungkus `try/catch`; dasbor tidak lagi memanggil daftar yang belum ada; gaji cadangan 10.000.000 diganti 0 |

### 3.3 Kepatuhan arsitektur frontend

Alur tetap view → service → `InstanceAxios`. Klien `axios` polos dan `axios.create` dengan token `localStorage` dihapus, karena sesi aplikasi memakai cookie dan token itu tidak pernah ditulis. Helper bersama `src/utils/hr/hr-api-utils.js` memakai ulang `getWorkforceCoreErrorMessage` yang sudah ada, ditambah penanda data belum tersedia.

### 3.4 Gerbang tampilan

**Modul referensi visual:** tidak berubah; tampilan modul ini dimigrasikan pada `FE-HRD-R15`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Pesan galat dan data belum tersedia | `InformationAlert` | `REUSE` | Pesan dari `getHrApiErrorMessage`, bukan pesan Axios |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

Elemen lain pada layar belum dimigrasikan; itu cakupan Gelombang 2.

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hrd/relations/dashboard` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/15-hi-dasbor.png` | Tiga daftar dari server, tanpa galat |
| `/hrd/relations/sanctions` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/16-hi-sanksi.png` | Pesan data belum tersedia |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Sama seperti sebelumnya |
| Kosong | Tabel kosong dengan kalimat bawaan layar; tidak ada lagi baris contoh |
| Gagal | `InformationAlert` merah berisi pesan backend atau terjemahan kode HTTP |
| Data belum tersedia | Pesan seperti "Daftar sanksi seluruh pegawai belum tersedia di server." |
| Tanpa hak akses | Pesan "Anda tidak punya hak untuk melakukan tindakan ini." dari kode 403 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/hrd/disciplinary/incidents` | Daftar insiden | `EmployeeDisciplinary : ViewIncidents` |
| `POST` | `/v1/hrd/disciplinary/sanctions` | Terbitkan SP | `EmployeeDisciplinary : IssueSanction` |
| `POST` | `/v1/hrd/disciplinary/suspensions` | Terbitkan skorsing | `EmployeeDisciplinary : IssueSuspension` |

Daftar sanksi, daftar skorsing, daftar exit clearance, putusan banding, dan pengaturan akses SIMRS berstatus `Rencana (belum tersedia)` (`FE-HRD-R53`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat service dengan controller backend (`[Route]` + `[Http*]`) | 19 dari 19 panggilan cocok; 5 method ditandai belum tersedia | `PASS` | Skrip pembanding route backend |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`, akun SuperAdmin) | Tiga daftar dari server, tanpa galat; Pesan data belum tersedia | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/15-hi-dasbor.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/16-hi-sanksi.png` |

Uji manual: `PASS` untuk layar yang dibuka. Tombol simpan tidak ditekan agar tidak menulis data ke database pengembangan.

**Tidak dijalankan:** uji galat 403 dan 500 sungguhan dari server; jalurnya dibuktikan lewat kode (tidak ada blok `catch` yang mengembalikan berhasil).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-S` 1: service memakai `InstanceAxios` | Terpenuhi | `employee-relation-service.js` |
| `AC-S` 2: tidak ada `catch` yang mengembalikan data contoh atau `success: true` | Terpenuhi | `grep "success: true"` kosong pada service |
| `AC-S` 3: alamat cocok backend; yang belum ada ditandai | Terpenuhi | 19 dari 19 panggilan cocok; 5 method ditandai belum tersedia |
| Penerbitan SP yang gagal menampilkan pesan gagal dan tidak menambah baris | Terpenuhi | `handleCreateSanction` kini dibungkus `try/catch`; baris ditambah hanya bila server berhasil |
| DoD | Terpenuhi | Lint, build, laporan, status roadmap |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | Endpoint daftar sanksi dan skorsing belum ada (`DEC-FER-05`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R15` menyambungkan dan memigrasikan layar Hubungan Industrial |
