# Laporan Perubahan Frontend — `FE-HRD-311`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-311` |
| Judul | Tutup periode: pratinjau berisi daftar dan jumlah scan tertahan |
| Slice | `A52` / `MVP-67` / `EPIC HRD-111` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-311` |
| Trace | `FR-PK-012`; `HRD-DEC-197`, `HRD-DEC-216`, `HRD-ASM-81`; `03-frontend-architecture.md` 31.2 (baris Tutup periode), 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 "HR Attendance - Period Management" (`close-preview` diperluas, `close`), `approved` 12 November 2026 |
| Wewenang UI | Layar yang sudah ada `/hr/attendance/periods` (`attendance-periods-view.jsx`) dan dialog Tutup baru. Rute usulan kartu sudah ada, jadi dipakai ulang. Tata letak `DEV_DISCRETION`; base component wajib |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-344` ✅ |
| Klasifikasi | `MEDIUM` — satu view, satu komponen dialog, satu utilitas, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/hr/attendance`, `src/utils/hr/attendance`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306` s.d. `310` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-09` dengan data nyata belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Konfirmasi memakai angka dan istilah yang keliru | Dialog lama menulis "`incompletePunchDailyCount` IncompletePunch akan dikonversi menjadi Absent" | `incompletePunchDailyCount` tidak memuat hari "Tidak Ada Scan", padahal keduanya selalu dikonversi (`HRD-DEC-216`). Kode mentah `IncompletePunch` tampil ke pengguna |
| Daftar hari yang akan dikonversi tidak tampil | `conversionCandidates` dari `close-preview` (`BE-HRD-344`) tidak dipakai | HRD tidak bisa meninjau siapa yang akan menjadi Absent sebelum menutup (`HRD-DEC-197` butir 3) |
| Scan tertahan tidak tampil | `heldDeviceScanCount` tidak dipakai | Peringatan `HRD-ASM-81` tidak sampai ke HRD |
| Galat pratinjau dan galat aksi tercampur | Satu `actionError` untuk keduanya | Pratinjau gagal tidak dibedakan dari tutup gagal |
| Tumpang tindih dengan `FE-HRD-092`/`093` (`S-B1`) | Register roadmap: keduanya 🟡 | Task ini menggantikan cakupan pratinjau tutup dan dialog penjaga kedua task itu (dicatat sesuai risiko kartu) |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Admin HRD atau Payroll yang memegang `AttendancePeriod : Close`.

**Contoh UAT-PK-09.** Periode Februari 2027 akan ditutup. Rina masih "Belum Lengkap" pada 10 Februari dan Budi "Tidak Ada Scan" pada 12 Februari.

1. Admin membuka **Periode Presensi** dan menekan **Tutup** pada baris `ATP-202702-0001`.
2. Dialog **Tutup Periode Presensi** menampilkan periode dan rentangnya, lalu kalimat: "Saat periode ditutup, 2 hari Belum Lengkap atau Tidak Ada Scan akan menjadi Tidak hadir. Tindakan ini tidak dapat dibatalkan kecuali lewat Buka Darurat." Label "Tidak hadir" diambil dari server.
3. Di bawahnya tampil tabel **Hari yang akan menjadi Tidak hadir**: tanggal, pegawai, unit, dan status saat ini (Rina "Belum Lengkap", Budi "Tidak Ada Scan").
4. Bila ada scan dari mesin yang belum terdaftar, muncul peringatan: "3 scan dari mesin yang belum terdaftar masih tertahan pada tanggal periode ini. Sebaiknya daftarkan mesinnya lebih dulu agar scan itu ikut dihitung. Bila periode tetap ditutup sekarang, hari pegawai terkait dikonversi menurut data saat ini." Peringatan ini tidak menghalangi penutupan (`HRD-ASM-81`).
5. Admin mengisi **Alasan Penutupan**. Tombol berubah menjadi **Tutup & Konversi 2 Hari** dan baru aktif sesudah alasan terisi.
6. Bila berhasil, dialog tertutup, muncul pemberitahuan "Periode ATP-202702-0001 ditutup. 2 hari dikonversi menjadi Tidak hadir." (angka dari respons server), dan daftar dimuat ulang dengan status `Closed`.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Penutupan gagal di server (misalnya 409) | Pesan server tampil di dialog. Daftar **tidak** dimuat ulang dan status periode di layar tetap `Open` |
| Ada butir pemblokir (misalnya koreksi presensi masih aktif) | "Periode belum dapat ditutup. Selesaikan butir yang memblokir di bawah lebih dulu." beserta pesan dan jumlah dari server; tombol tutup terkunci |
| Daftar calon terlalu panjang dan dipotong server | "Daftar di bawah menampilkan 2 dari 250 hari. Seluruhnya tetap dikonversi saat ditutup."; kalimat konfirmasi dan tombol memakai angka lengkap (250) |
| Tidak ada calon konversi | "Tidak ada hari Belum Lengkap atau Tidak Ada Scan pada periode ini. …"; tombol bernama **Tutup Periode** |
| Pratinjau gagal dimuat | Pesan server; tombol tutup terkunci dan tidak ada permintaan tutup |
| Peringatan lain dari server (misalnya raw log masih Pending) | Tampil apa adanya sebagai peringatan. Dua peringatan server yang sudah disajikan khusus (hari yang dikonversi dan scan tertahan) tidak diulang |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-311`; `api-contract.md` 32 (Period Management); `00-interview-decisions.md` `HRD-DEC-197`, `216`, `HRD-ASM-81`; `04-prd-to-mvp.md` `UAT-PK-09`; `AttendancePeriodDtos.cs` (`AttendancePeriodClosePreviewResponse`, `AttendancePeriodConversionCandidateResponse`, `AttendancePeriodActionResponse`, `CloseAttendancePeriodRequest`), `AttendancePeriodService.cs` (daftar `Issues`); `attendance-periods-view.jsx`; utilitas status `FE-HRD-310`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/attendance-periods-view.jsx` | Ditata ulang. Dialog Tutup memakai komponen baru; status pratinjau (memuat/galat) dipisah dari galat aksi; label status dari metadata; pemberitahuan sukses menyebut jumlah hasil konversi server; galat tutup tidak memuat ulang daftar; modal tidak dapat ditutup selama permintaan berjalan. Alur Buka Darurat tidak berubah |
| `src/components/view/hr/attendance/attendance-period-close-dialog.jsx` (baru) | Dialog Tutup: info periode, kalimat konfirmasi berangka, butir pemblokir/peringatan, peringatan scan tertahan, tabel calon konversi, catatan daftar terpotong, alasan |
| `src/utils/hr/attendance/attendance-period-close-utils.js` (baru) | `summarizeClosePreview`, `buildCloseConfirmationText`, `buildTruncationNote`, `buildHeldScanWarning` |
| `tests/unit/hr-attendance-period-close-utils.test.mjs` (baru) | 5 uji |
| `tests/e2e/hr-attendance-period-close.spec.mjs` (baru) | 5 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Layar memakai service `getPeriodClosePreview`/`closePeriod` yang sudah ada (respons `v26` mengalir apa adanya), `usePermission`, `useHrRemoteData` untuk metadata, dan utilitas label status dari `FE-HRD-310`. Seluruh kontrol memakai base component (`ConfirmModal`, `DataTable`, `StatusBadge`, `InformationAlert`, `BaseTextAreaField`, `HrInfoGrid`, `ToastStack`). Logika teks dan ringkasan berada di `src/utils/hr/attendance` sebagai fungsi murni. Berkas view tetap LF seperti aslinya.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat pratinjau penutupan..."; tombol tutup terkunci; selama tutup berjalan tombol menampilkan pemutar dan dialog tidak dapat ditutup |
| Kosong | Tanpa calon konversi: kalimat tanpa angka dan tanpa tabel |
| Gagal | Pratinjau gagal: pesan server dan tombol terkunci. Tutup gagal: pesan server dan status periode tidak berubah |
| Tanpa hak akses | Tombol **Tutup** hanya tampil bagi pemegang `AttendancePeriod : Close` (perilaku lama); server tetap memeriksa |

---

## 5. Endpoint yang dikonsumsi

Nilai `[Tags]` controller: `HR Attendance - Period Management` (`AttendancePeriodController`).

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/periods` | Daftar periode (tidak berubah) | `AttendancePeriod : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/periods/{id}/close-preview` | Pratinjau: `conversionCandidateCount`, `conversionCandidates`, `isConversionCandidateListTruncated`, `heldDeviceScanCount`, `issues` | `AttendancePeriod : Read` |
| `POST` | `/v1/corporate/human-resource/attendance/periods/{id}/close` | Menutup dengan `{ reason }`; respons `convertedIncompleteToAbsentCount` | `AttendancePeriod : Close` |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/filters/metadata` | Label status calon konversi dan label `Absent` | `AttendanceDaily : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-period-close.spec.mjs` (build produksi, port 3710, API tiruan penuh, nol tulis) | 5/5 lulus | `PASS` | Keluaran Playwright |
| — `UAT-PK-09` | Kalimat "2 hari … akan menjadi Tidak hadir"; tabel Rina "Belum Lengkap" dan Budi "Tidak Ada Scan"; peringatan 3 scan tertahan; peringatan raw log tampil; dua peringatan server khusus tidak diulang; tombol terkunci sampai alasan terisi; badan `{ reason }`; pemberitahuan sukses; daftar dimuat ulang dan status `Closed` | `PASS` | Skenario 1 |
| — Galat transaksi 409 | Pesan server di dialog; daftar tidak dimuat ulang; status tetap `Open` sesudah dialog ditutup | `PASS` | Skenario 2 |
| — Periode terblokir | Pesan pemblokir dan butir server; tombol terkunci walau alasan terisi | `PASS` | Skenario 3 |
| — Daftar terpotong | Catatan "2 dari 250 hari"; kalimat dan tombol memakai 250 | `PASS` | Skenario 4 |
| — Pratinjau gagal | Pesan server; tombol terkunci; tidak ada `POST close` | `PASS` | Skenario 5 |
| `tests/unit/hr-attendance-period-close-utils.test.mjs` | 5/5 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (jumlah dari `incompletePunchDailyCount`; kalimat tanpa jumlah; peringatan ganda tidak disaring; scan tertahan nol; penanda terpotong diabaikan; konfirmasi tampil walau terblokir) | 6/6 tertangkap; source dipulihkan | `PASS` | `mut311.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.255 lulus, 100 gagal dari 5.355 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-306`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas berkas task | Tanpa error maupun peringatan pada berkas task | `PASS` | Keluaran perintah (satu peringatan lama di `RawPunchAuditDrawer.jsx`, di luar task) |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — kontrol yang diubah (Tutup, alasan, Tutup & Konversi, Batal) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-09` dengan data nyata. Menutup periode di devYoga mengonversi status kehadiran sungguhan dan menyentuh serah terima payroll di basis data bersama. Perilaku backend dibuktikan `AT-HRD-PK-22`, `-23`, `-31` (laporan `BE-HRD-344`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Konfirmasi menyebut jumlah yang akan dikonversi | Terpenuhi | Kalimat dan tombol memakai `conversionCandidateCount` (skenario 1 dan 4; uji unit 1; mutasi M1/M2) |
| Galat transaksi tidak mengubah tampilan status | Terpenuhi | Skenario 2: daftar tidak dimuat ulang, status tetap `Open` |
| Outcome: HRD meninjau daftar yang akan menjadi `Absent` sebelum menutup | Terpenuhi | Tabel calon konversi (skenario 1) |
| DoD umum: base component, memuat/galat, tombol nonaktif saat mengirim, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-09` | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kolom status periode di tabel daftar masih menampilkan kode (`Open`/`Closed`) seperti sebelumnya; di luar cakupan task |
| Masalah yang diketahui | `NONE` |
| Tumpang tindih `FE-HRD-092`/`093` | Task ini menggantikan cakupan pratinjau tutup dan dialog penjaga `FE-HRD-092`/`093` (`S-B1`, 🟡). Status kedua baris itu tidak diubah di register; penilaian ulangnya diserahkan ke pass verifikasi `S-B1` |
| Dependency backend | `NONE` |
| Perubahan sampingan | Playwright mengubah `test-results/.last-run.json` (ter-track); dipulihkan dari `HEAD` dengan akhir baris CRLF |
| Interupsi | `NONE` |
| Status Git | ` M src/components/view/hr/attendance/attendance-periods-view.jsx`, `?? src/components/view/hr/attendance/attendance-period-close-dialog.jsx`, `?? src/utils/hr/attendance/attendance-period-close-utils.js`, `?? tests/unit/hr-attendance-period-close-utils.test.mjs`, `?? tests/e2e/hr-attendance-period-close.spec.mjs`; ditambah berkas `FE-HRD-306` s.d. `310` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-312` (layar mesin absen) |
