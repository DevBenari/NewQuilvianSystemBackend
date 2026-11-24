# Laporan Perubahan Frontend — `FE-HRD-309`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-309` |
| Judul | Setelan kehadiran: jendela scan, tanpa pilihan konversi |
| Slice | `A52` / `MVP-67` / `EPIC HRD-111` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-309` |
| Trace | `FR-PK-010`; `HRD-DEC-211`, `HRD-DEC-215`, `HRD-DEC-216`; `03-frontend-architecture.md` 31.2 (baris Setelan kehadiran), 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 "HR Attendance - Settings" (`GET`/`PUT`), `VR-PK-23`; `approved` 12 November 2026 |
| Wewenang UI | Layar yang sudah ada `/hr/attendance/settings` (`attendance-settings-view.jsx`). Tata letak `DEV_DISCRETION`; base component wajib |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-342` ✅ |
| Klasifikasi | `LIGHT` — satu view, satu utilitas aturan, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/hr/attendance`, `src/utils/hr/attendance`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306` s.d. `308` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-08` dengan data nyata belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Batas toleransi di layar 0–30 menit | `MAX_GRACE_MINUTES = 30` di `attendance-settings-view.jsx`; server menerima 0–60 (`UpdateAttendanceSettingRequest`, `AttendanceSettingService`, `VR-PK-23`) | Admin HRD tidak bisa menyimpan toleransi 31–60 menit yang sah menurut server |
| Ruas jendela scan tidak ada | Layar hanya memuat toleransi, batas koreksi mundur, lampiran wajib, dan konversi | `ScanWindowBeforeMinutes`/`ScanWindowAfterMinutes` (`HRD-DEC-211`) tidak bisa diatur dari aplikasi |
| Sakelar "Ubah Incomplete Punch Menjadi Alfa" masih tampil dan dikirim | `AUTO_ABSENT_FIELD` dan `autoConvertIncompleteToAbsent` di badan `PUT` | Bertentangan dengan `HRD-DEC-216` (pilihan dihapus; konversi selalu terjadi saat tutup) dan kontrak `v26` (ruas dihapus dari respons) |
| Gagal memuat tidak bisa dicoba ulang | Galat hanya ditampilkan | DoD meminta tombol coba lagi |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Admin HRD (hak `AttendanceSetting : Read` dan `: Update`).

1. Admin HRD membuka **Pengaturan Parameter Presensi**. Layar menampilkan empat angka dan satu sakelar:

   | Ruas | Rentang (sama dengan server) | Contoh |
   | --- | --- | --- |
   | Toleransi Keterlambatan | 0–60 menit | 10 |
   | Batas Koreksi Mundur | 1–30 hari | 3 |
   | Jendela Scan Masuk (menit sebelum mulai shift) | 0–720 menit (12 jam) | 120 → "Saat ini: 2 jam." |
   | Jendela Scan Pulang (menit sesudah selesai shift) | 0–960 menit (16 jam) | 240 → "Saat ini: 4 jam." |
   | Koreksi Wajib Berlampiran | ya/tidak | ya |

2. Di bawah jendela scan tampil contoh yang ikut berubah saat angka diketik, sesuai `HRD-DEC-211`: "Contoh shift Pagi 07.00–14.00: scan masuk diterima mulai pukul 05.00, scan pulang sampai pukul 18.00. Scan di luar jendela tidak dibuang; scan itu masuk daftar pengecualian."
3. Pilihan konversi tidak ada lagi. Sebagai gantinya tampil keterangan: "Saat periode kehadiran ditutup, hari yang masih Belum Lengkap atau Tidak Ada Scan selalu menjadi Alfa. Daftarnya dapat diperiksa di pratinjau tutup periode sebelum menekan Tutup."
4. **Contoh UAT-PK-08.** Admin mengubah toleransi dari 10 menjadi 15 menit, lalu menekan **Simpan Pengaturan**. Muncul "Pengaturan presensi tersimpan dan berlaku untuk pemrosesan berikutnya.", dan ruas menampilkan nilai yang dikembalikan server. Scan Rina 07.12 untuk shift 07.00 kemudian dihitung tepat waktu oleh pemrosesan backend.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Mengetik angka di atas batas, misalnya 61 menit atau 0 hari | Ruas langsung dibatasi ke rentang server (61 → 60, 721 → 720, 961 → 960, 0 hari → 1) oleh `BaseInputField` bertipe angka |
| Ruas dikosongkan lalu menekan Simpan | Pesan per ruas, misalnya "Toleransi keterlambatan harus bilangan bulat 0–60 menit."; tidak ada permintaan ke server |
| Server menolak simpan (400) | Pesan server apa adanya, misalnya "Nilai toleransi atau jendela scan di luar batas." |
| Gagal memuat setelan | Pesan server dan tombol **Coba Lagi**; ruas dan tombol Simpan terkunci sampai setelan berhasil dimuat |
| Hanya hak baca | Ruas terkunci, tanpa tombol Simpan, dengan keterangan "Anda hanya dapat melihat setelan ini. …" |
| Tanpa hak baca | "Pengaturan presensi hanya untuk HR Manager atau Admin Sistem." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-309`; `api-contract.md` 32 (Settings); `validation-matrix.md` `VR-PK-23`; `00-interview-decisions.md` `HRD-DEC-211`, `215`, `216`; `04-prd-to-mvp.md` `UAT-PK-08`; `AttendanceSettingDtos.cs`, `AttendanceSettingService.cs`, `AttendanceSettingController.cs`; `attendance-settings-view.jsx`; `base-form-control.jsx` (normalisasi ruas angka); `use-permission.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/attendance-settings-view.jsx` | Ruas jendela scan masuk/pulang dengan padanan jam dan contoh shift Pagi; batas toleransi 0–60; sakelar konversi dihapus dan diganti keterangan `HRD-DEC-216`; Coba Lagi saat gagal memuat; keterangan hak baca saja; nilai form diperbarui dari respons simpan |
| `src/utils/hr/attendance/attendance-setting-rules.js` (baru) | `ATTENDANCE_SETTING_LIMITS` (batas server), `validateAttendanceSettingForm`, `toAttendanceSettingForm`, `buildAttendanceSettingPayload` (tanpa `autoConvertIncompleteToAbsent`), `describeMinutesAsHours`, `describeScanWindowExample` |
| `tests/unit/hr-attendance-setting-rules.test.mjs` (baru) | 6 uji aturan |
| `tests/e2e/hr-attendance-settings.spec.mjs` (baru) | 9 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Layar tetap memakai service `AttendanceManagementService.getSettings`/`updateSettings` yang sudah ada (perluasan ruas `v26` mengalir apa adanya, `FE-HRD-306`), hook `usePermission`, dan base component (`BaseInputField`, `BaseSwitchField`, `BaseButton`, `InformationAlert`, `AccessDeniedAlert`, `ToastStack`). Aturan rentang dipindah ke `src/utils/hr/attendance` sebagai fungsi murni, sehingga batas layar dan batas server diuji di satu tempat. Peringatan ESLint `react-hooks/set-state-in-effect` yang muncul pada perubahan awal diperbaiki: status memuat diatur di tombol Coba Lagi, bukan di badan efek.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Ruas dan tombol Simpan terkunci |
| Kosong | `NOT APPLICABLE` — setelan selalu satu baris per rumah sakit (backend membuat baris bawaan bila belum ada) |
| Gagal | Pesan server; **Coba Lagi** memuat ulang; simpan terkunci selama galat |
| Tanpa hak akses | Tanpa hak baca: "Pengaturan presensi hanya untuk HR Manager atau Admin Sistem."; hanya hak baca: ruas terkunci dan keterangan |

---

## 5. Endpoint yang dikonsumsi

#### HR Attendance - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance-management/settings` | Membaca setelan, termasuk `scanWindowBeforeMinutes`/`scanWindowAfterMinutes` | `AttendanceSetting : Read` |
| `PUT` | `/v1/corporate/human-resource/attendance-management/settings` | Menyimpan `gracePeriodMinutes`, `scanWindowBeforeMinutes`, `scanWindowAfterMinutes`, `maxBackdatedCorrectionDays`, `requireAttachmentForCorrection` | `AttendanceSetting : Update` |

Nilai `[Tags]` controller: `HR Attendance - Settings` (`AttendanceSettingController`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-settings.spec.mjs` (build produksi, port 3710, API tiruan penuh, nol tulis) | 9/9 lulus | `PASS` | Keluaran Playwright |
| — Tampilan setelan | Jendela 120/240 tampil dengan "2 jam"/"4 jam" dan contoh 05.00–18.00; tanpa "Ubah Incomplete Punch"; keterangan Alfa; "Rentang 0 sampai 60 menit" | `PASS` | Skenario 1 |
| — `UAT-PK-08` 10 → 15 menit | Badan `PUT` tepat lima ruas tanpa pilihan konversi; pemberitahuan sukses | `PASS` | Skenario 2 |
| — Nilai di atas batas | 61/721/961/0 dibatasi menjadi 60/720/960/1 | `PASS` | Skenario 3 |
| — Ruas kosong | Empat pesan rentang; tidak ada `PUT` | `PASS` | Skenario 4 |
| — Nilai tepi 60/720/960/30 | Terkirim; "Saat ini: 12 jam." | `PASS` | Skenario 5 |
| — Galat simpan 400, hanya hak baca, tanpa hak baca, gagal memuat + Coba Lagi | Sesuai bagian 2 | `PASS` | Skenario 6–9 |
| Putaran awal uji layar | Satu skenario mengharapkan pesan galat untuk 61 menit; ternyata `BaseInputField` membatasi nilai ke `min`/`max`, sehingga nilai itu tidak pernah masuk. Uji disesuaikan dengan perilaku base component, dan skenario ruas kosong ditambahkan untuk validasi layar. Langkah mengetik huruf dihapus karena peramban menolaknya (input bertipe angka) | Uji diperbaiki; source tidak berubah | Keluaran Playwright |
| `tests/unit/hr-attendance-setting-rules.test.mjs` | 6/6 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (toleransi kembali 30; jendela masuk 240; jendela pulang 480; desimal diterima; pilihan konversi ikut dikirim; jendela tidak dikirim) | 6/6 tertangkap; source dipulihkan | `PASS` | `mut309.cjs`, `mut309b.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.243 lulus, 100 gagal dari 5.343 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-306`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas berkas task | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol yang diubah (empat ruas angka, sakelar lampiran, Simpan Pengaturan, Coba Lagi) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-08` dengan data nyata. Bukti "Rina 07.12 tepat waktu" dihitung backend saat memproses presensi dan memerlukan perubahan setelan sungguhan di devYoga, yang dipakai bersama. Perilaku backend dibuktikan `AT-HRD-PK-24`, `-25`, `-27` (laporan `BE-HRD-342`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Validasi rentang angka di layar sama dengan server | Terpenuhi | `ATTENDANCE_SETTING_LIMITS` = batas `VR-PK-23`/`VR-ATT-18` (uji unit 1); nilai tepi diterima, nilai di luar dibatasi, kosong ditolak (skenario 3–5) |
| Outcome: Admin HRD mengubah toleransi dan jendela scan; pilihan konversi hilang | Terpenuhi | Skenario 1, 2, 5; badan `PUT` tanpa `autoConvertIncompleteToAbsent` (uji unit 4) |
| DoD umum: base component, memuat/galat/coba lagi, tombol nonaktif saat mengirim, tidak berwenang, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-08` | Bagian layar terpenuhi dengan API tiruan; perhitungan data nyata devYoga `NOT RUN` | Skenario 2 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sakelar konversi juga sudah tidak dikirim backend sejak `v26`; data lama yang masih menyimpan nilai konversi tidak lagi dibaca layar |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | Playwright mengubah `test-results/.last-run.json` (ter-track); dipulihkan dari `HEAD` dengan akhir baris CRLF |
| Interupsi | `NONE` |
| Status Git | ` M src/components/view/hr/attendance/attendance-settings-view.jsx`, `?? src/utils/hr/attendance/`, `?? tests/unit/hr-attendance-setting-rules.test.mjs`, `?? tests/e2e/hr-attendance-settings.spec.mjs`; ditambah berkas `FE-HRD-306` s.d. `308` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-310` (monitoring harian: status baru dan pengecualian jendela scan) |
