# Laporan Perubahan Frontend — `FE-HRD-321`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-321` |
| Judul | Dashboard kehadiran lima kartu (`POST-MVP`) |
| Slice | `A52` / `POST-MVP` / `EPIC HRD-119` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-321` |
| Trace | `FR-PK-030`, `FR-PK-031`; `HRD-DEC-208`; `VR-PK-27`; PRD M01 (FR-01.01 s.d. FR-01.05); `03-frontend-architecture.md` 31.1 baris M01 |
| Contract version | `v26` — `api-contract.md` §32 "Attendance Management / Attendance Dashboard", `approved` 12 November 2026; bentuk respons dari `BE-HRD-357` |
| Wewenang UI | Rute `/hr/attendance/dashboard` dan `/hr/attendance/dashboard/{kartu}` sesuai usulan kartu (`/hr/attendance/dashboard/*`, satu rute per submenu PRD M01); butir menu **Dashboard Kehadiran** di grup Presensi & Kehadiran (submenu per kartu menunggu `FE-HRD-324` ⛔). Tata letak `DEV_DISCRETION`; base component wajib. Pemilih unit hanya untuk `AttendanceReport : ReadAllUnits`, mengikuti keputusan pengguna 24 November 2026 pada layar laporan (cakupan dashboard = cakupan laporan, `BE-HRD-357` bagian 2) |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-357` ✅ |
| Klasifikasi | `MEDIUM` — dua rute, satu view, satu utilitas, perluasan service, menu, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 24 November 2026: "lanjutkan FE-HRD-321") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/attendance/dashboard`, `src/components/view/hr/attendance`, `src/utils/hr/attendance`, `src/lib/services/hr/attendance-management-service.js`, `src/utils/menu-sidebar/menu-items.jsx`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `004524f4c` (branch `YogaV2`) + working tree `FE-HRD-313`–`316` |
| Commit backend rujukan | `4d2c0162` (branch `yoga`) |
| Tanggal | 24 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; uji data asli dijadwalkan pada `FE-HRD-323` (verifikasi `POST-MVP`) |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar dashboard kehadiran | Tidak ada rute `/hr/attendance/dashboard`; tidak ada pemanggilan `attendance/dashboard` di `src/` | PRD M01 (lima submenu) belum punya layar |
| Klien API belum ada | `FE-HRD-306` hanya mencakup endpoint MVP | Dua fungsi service ditambahkan di task ini (dalam kontrak `v26`) |
| Cakupan dashboard memakai penanda yang sama dengan laporan | `AttendanceDashboardService` memakai `HrdUnitScopeService` + `AttendanceReport : ReadAllUnits` | Aturan pemilih unit laporan berlaku sama |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: Kepala Unit, Kepala Bidang, Admin HRD (`AttendanceDashboard : Read`). Pegawai biasa tidak dapat membukanya.

**Contoh `AT-HRD-PK-40`/`-41`.** Sari (Kepala Unit ICU) membuka **Presensi & Kehadiran → Dashboard Kehadiran** pukul 09.00.

1. Tanggal bawaan hari ini (WIB). Lima panel tampil: Kehadiran Hari Ini (Terjadwal 13, Hadir 3, Terlambat 1, Belum Hadir 1, Tidak Ada Scan 2, Sakit 1, …), Pegawai Bertugas (Bertugas, Shift Berjalan, Siaga Utama), Keterlambatan, Ketidakhadiran, Jadwal Belum Lengkap. Label dan urutan angka dari server.
2. Di atas kartu: "Dihitung pukul 24 Nov 2026 09.00 WIB. Angka tidak diperbarui otomatis; tekan Muat Ulang untuk menghitung ulang." Layar tidak mengirim permintaan lagi sampai **Muat Ulang** ditekan atau filter diubah.
3. Sari mengeklik angka "Terlambat 1" pada kartu Keterlambatan → panel **Rincian: Keterlambatan — Terlambat** menampilkan Tono Wibowo, shift Pagi, jadwal 07.00, masuk 07.12, terlambat 12 menit. Jumlah baris = angka kartu. Cuti sakit tampil "Sakit" saja.
4. Admin HRD mendapat pemilih **Semua unit**; unit terpilih ikut ke kartu dan rincian. Kepala Unit tidak diberi pemilih: server membatasi ke cakupannya.
5. Menu per submenu PRD tersedia sebagai alamat: `/hr/attendance/dashboard/attendance-today`, `on-duty`, `lateness`, `absence`, `incomplete-schedule` membuka dashboard dengan rincian angka utama kartu itu.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Unit di luar cakupan (`403`) | Pesan server "Anda tidak berwenang melihat data unit ini" dan **Coba Lagi** |
| Gagal memuat rincian | Pesan server di panel rincian |
| Angka 0 / rincian kosong | "Tidak ada pegawai untuk angka ini." |
| Tanpa hak | "Anda tidak berwenang membuka dashboard kehadiran."; tanpa permintaan data |
| Kode kartu di luar lima kode | Halaman tidak ditemukan bawaan aplikasi; tanpa permintaan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-321`; `HRD-DEC-208`; laporan `BE-HRD-357`; `AttendanceDashboardDtos.cs`, `AttendanceDashboardController.cs`; `03-frontend-architecture.md` 31.1 M01; `attendance-management-service.js`; `summary-grid.jsx` (kartu dapat diklik), `hr-panel.jsx`, `attendance-report-view.jsx`/`-utils.js` (pola filter, cakupan, format sel); `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/attendance/dashboard/page.jsx` (baru) | Rute dashboard |
| `src/app/hr/attendance/dashboard/[cardCode]/page.jsx` (baru) | Rute per kartu; kode tak dikenal → `notFound()` |
| `src/components/view/hr/attendance/attendance-dashboard-view.jsx` (baru) | Filter tanggal/unit, Muat Ulang, jam hitung, lima panel kartu dengan angka dapat diklik, rincian berpaginasi, hak `AttendanceDashboard : Read` |
| `src/utils/hr/attendance/attendance-dashboard-utils.js` (baru) | Metadata lima kartu, kolom rincian per kartu, tanggal WIB, query kartu = query rincian (+ `metric`, paginasi), jam hitung WIB, urutan kartu; format sel memakai `formatReportCell` |
| `src/lib/services/hr/attendance-management-service.js` | `ATTENDANCE_DASHBOARD_CARD_CODES`, `getAttendanceDashboard`, `getAttendanceDashboardCardDetails` (kode kartu divalidasi sebelum request) |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Dashboard Kehadiran** (`AttendanceDashboard : Read`) di awal grup Presensi & Kehadiran |
| `tests/unit/hr-attendance-dashboard-utils.test.mjs` (baru) | 4 uji |
| `tests/e2e/hr-attendance-dashboard.spec.mjs` (baru) | 4 skenario layar |

### 3.3 Kepatuhan arsitektur frontend

Data memakai `AttendanceManagementService` + `useHrRemoteData` seperti layar `A52` lain; tidak ada slice baru (data dashboard tidak dipakai bersama layar lain dan dihitung ulang setiap dibuka). Hak `useStrictPermissions`; pemilih unit `useSelectResource` + `ResourceFilterSelect`. Kartu memakai `SummaryCards` (`onClick`, `active`, `ariaLabel`) di dalam `HrPanel`. Tidak ada interval/polling.

---

## 4. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/dashboard` | Lima kartu; query `date`, `organizationUnitId` (hanya `ReadAllUnits`) | `AttendanceDashboard : Read` |
| `GET` | `/v1/corporate/human-resource/attendance/dashboard/cards/{cardCode}/details` | Rincian satu angka; query sama + `metric`, `pageNumber`, `pageSize` | `AttendanceDashboard : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/organization-units/options` | Pemilih unit, hanya `ReadAllUnits` | sesuai endpoint options |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-dashboard.spec.mjs` (build produksi, port 3710, API tiruan) | 4/4 lulus pada putaran pertama | `PASS` | Keluaran Playwright |
| — `AT-HRD-PK-40` | Lima kartu; jam hitung 09.00; 6 detik tanpa permintaan tambahan; Muat Ulang → permintaan kedua, jam 09.05; Kepala Unit tanpa pemilih unit dan tanpa permintaan daftar unit | `PASS` | Skenario 1 |
| — `AT-HRD-PK-41` | Klik Terlambat → `metric=late`, 1 baris Tono 07.12; Bertugas → 3 baris termasuk "Petugas Pengganti" dan "Siaga Utama"; Sakit → "Sakit" tanpa alasan; Tutup Rincian | `PASS` | Skenario 2 |
| — Admin HRD | Unit ICU ikut ke kartu dan rincian; HCU → pesan 403 server | `PASS` | Skenario 3 |
| — Rute per kartu, kode tak dikenal, tanpa hak | `/on-duty` membuka rincian Bertugas (`metric=total`); `/payroll` halaman tidak ditemukan; tanpa hak pesan dan nol permintaan | `PASS` | Skenario 4 |
| Pemeriksaan visual | Lima panel, angka tersorot saat dipilih, rincian di bawah | `PASS` | Tangkapan di scratchpad sesi |
| `tests/unit/hr-attendance-dashboard-utils.test.mjs` + regresi `hr-a52-api-client.test.mjs` | 4/4 + 12/12 | `PASS` | Keluaran perintah |
| Kontrol negatif: 5 mutasi (rincian tanpa unit; hari ini UTC; jam hitung UTC; metric tidak dikirim; metric utama salah) | 5/5 tertangkap; source dipulihkan identik byte | `PASS` | `mut321.cjs` di scratchpad sesi |
| Seluruh uji unit | 5.285 lulus, 100 gagal dari 5.385 | `EXISTING / ENVIRONMENT ISSUE` | Daftar gagal identik per nama dengan putaran `FE-HRD-313`; seluruhnya modul lain |
| `npx eslint` atas berkas task | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil; `/hr/attendance/dashboard` dan `[cardCode]` terdaftar | `PASS` | Keluaran perintah |

Uji manual: `PASS` — tanggal, pemilih unit, Muat Ulang, klik angka, Tutup Rincian, paginasi, rute per kartu dijalankan di Chromium terhadap build produksi.

**Tidak dijalankan:** data asli devYoga. `AttendanceDashboard : Read` belum diberikan ke jabatan mana pun; perilaku server (`AT-HRD-PK-40`, `-41`, cakupan) dibuktikan harness `AttendanceDashboardTests` (`BE-HRD-357`). Uji data asli masuk `FE-HRD-323`.

---

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak memperbarui diri terus-menerus; tombol Muat Ulang | Terpenuhi | Skenario 1 |
| Outcome: lima kartu per cakupan dengan jam hitung dan rincian | Terpenuhi | Skenario 1–4 |
| Verifikasi kartu: `AT-HRD-PK-40`, `-41` | Layar terpenuhi dengan API tiruan; data asli di `FE-HRD-323` | Skenario 1–2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Submenu per kartu di sidebar menunggu `FE-HRD-324` (grup menu ⛔ `DSG-Q-01`/`-02`); sementara satu butir menu + lima alamat rute |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Status Git | `?? src/app/hr/attendance/dashboard/`, `?? src/components/view/hr/attendance/attendance-dashboard-view.jsx`, `?? src/utils/hr/attendance/attendance-dashboard-utils.js`, `?? tests/unit/hr-attendance-dashboard-utils.test.mjs`, `?? tests/e2e/hr-attendance-dashboard.spec.mjs`, ` M src/lib/services/hr/attendance-management-service.js`, ` M src/utils/menu-sidebar/menu-items.jsx`; ditambah berkas `FE-HRD-313`–`316`. Tidak ada commit |
| Langkah berikutnya | `FE-HRD-322` (Realisasi Jadwal) |
