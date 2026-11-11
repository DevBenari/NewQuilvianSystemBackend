# Laporan Perubahan Frontend — `FE-HRD-090`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-090` |
| Judul | Layar Dasbor Administrasi & Monitoring Presensi Harian (`FE-HRD-144` `/hr/attendance`) |
| Slice | `S-B1` · `MVP-21` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-090`; dikerjakan lewat `FE-HRD-R13` |
| Trace | `HRD-DEC-104`, `HRD-DEC-105`, `HRD-DEC-106` |
| Contract version | API Contract v11 bagian 17 (disetujui pemilik 4 November 2026) |
| Wewenang UI | Tata letak kartu dan warna badge `DEV_DISCRETION` |
| Dependency | `FE-HRD-089` 🟡, `BE-HRD-106` ✅ [BE] |
| Klasifikasi | `MEDIUM` — layar baru dengan service, state, dan validasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — kriteria 1 terbukti; kriteria 2 belum ada data shift malam; kriteria 3 jam masuk ditandai warna, bukan huruf tebal |

---

## 1. Keadaan yang ditemukan di awal

`attendance-monitoring-view.jsx` (`FE-HRD-009`) menghitung kartu dari halaman tabel, memakai kolom `title` yang tidak dikenal `DataTable`, `variant="outline-primary"`, dan elemen `input`/`textarea` mentah.

---

## 2. Proses bisnis dari sisi pengguna

HR membuka Monitoring Presensi Harian. Enam kartu menampilkan Hadir Tepat Waktu, Terlambat, Izin Pulang Cepat, Dinas Luar, Incomplete Punch, dan Alfa dari ringkasan server. Contoh di uji: 2 terlambat, 1 pulang cepat, 1 incomplete punch. Baris yang hanya punya satu tap berlabel "1 Punch - Perlu Koreksi"; keterlambatan di atas 10 menit ditulis merah, misalnya "Terlambat 391 menit". Tombol Log Mesin membuka log mentah, dan tombol Koreksi On-Behalf hanya untuk pemegang `AttendanceCorrection : CreateOnBehalf`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-B1` di `frontend-roadmap.md`, controller dan DTO `AttendanceManagement`, view `attendance-monitoring-view.jsx` lama, `attendance-management-service.js`, uji unit `workforce-core-cross-employee.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/attendance-monitoring-view.jsx` | Ditulis ulang memakai slice, ringkasan server, base component, dan modal koreksi atas nama ber-`BaseFormControl` |
| `src/style/hr/attendance/attendance.module.css` | CSS Module baru berbasis token |

### 3.3 Kepatuhan arsitektur frontend

Nama berkas `attendance-daily-view.jsx` pada kartu tidak dipakai; view lama diperbarui di tempat karena dipakai rute dan dikunci uji unit `FIX-HRD-001-04`.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` | `REUSE` | — |
| Enam kartu | `SummaryGrid` | `REUSE` | — |
| Saringan | `DataFilter` + 2 `FilterDatePicker` + `ResourceFilterSelect` | `REUSE` | — |
| Tabel dan paginasi | `DataTable` + `RegionPagination` | `REUSE` | — |
| Status kehadiran | `StatusBadge` | `REUSE` | — |
| Aksi baris | `BaseButton` (`secondary`, `subtle`) | `REUSE` | — |
| Koreksi atas nama | `ConfirmModal` + `BaseInputField` + `BaseTextAreaField` | `COMPOSE` | — |
| Jam dan menit terlambat | `attendance.module.css` | `NEW (CSS Module)` | Belum dicakup base component; nilai memakai token |

`UI GATE: PASS — 6 elemen REUSE, 1 COMPOSE, 1 NEW (CSS Module berbasis token)`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/attendance` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09-presensi-harian.png` | Setara dengan referensi |
| `/hr/attendance` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09b-presensi-harian-warna-terlambat.png` | Sesudah penanda warna |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat presensi harian..." dan kartu kerangka |
| Kosong | "Belum ada data presensi untuk kriteria ini." |
| Gagal | `InformationAlert` merah dari `getHrApiErrorMessage` |
| Tanpa hak akses | Menu tidak tampil; tombol koreksi disembunyikan |

---

## 5. Endpoint yang dikonsumsi

Lihat `FE-HRD-088` grup `Corporate / Human Resource / Attendance Management / Attendance Daily` dan `Attendance Correction Administration` (`POST /correction-requests/on-behalf`, `AttendanceCorrection : CreateOnBehalf`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Kartu dari server, badge 1 Punch, menit terlambat merah | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09-presensi-harian.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09b-presensi-harian-warna-terlambat.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** Pengiriman koreksi atas nama (menulis data); baris shift malam (tidak ada di data uji).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `IncompletePunch` tampil dengan badge peringatan "1 Punch - Perlu Koreksi" | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09-presensi-harian.png` |
| Shift malam menampilkan tanggal dinas hari H dan jam keluar hari H+1 | Belum terbukti | Terpetakan ke source (`isOvernightSchedule`); tidak ada data uji |
| Keterlambatan > 10 menit: menit merah dan jam masuk tebal | Sebagian | Menit dan jam masuk merah; huruf tebal dinetralkan kontrak tabel bersama |

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
| Langkah berikutnya | Keputusan pemilik soal huruf tebal di sel tabel, lalu uji data shift malam |
