# Laporan Perubahan Frontend — `FE-HRD-088`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-088` |
| Judul | Service Client API Presensi, Perizinan, dan Pengaturan HR (`attendance-service.js`) |
| Slice | `S-B1` · `MVP-21` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-088`; dikerjakan lewat `FE-HRD-R13` |
| Trace | API Contract v11 bagian 17.2 |
| Contract version | API Contract v11 bagian 17 (disetujui pemilik 4 November 2026) |
| Wewenang UI | Struktur modul helper `DEV_DISCRETION` |
| Dependency | `BE-HRD-111` ✅, `BE-HRD-115` ✅ [BE] |
| Klasifikasi | `LIGHT` — service |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

`attendance-management-service.js` hanya punya daftar harian, daftar periode, dan koreksi atas nama.

---

## 2. Proses bisnis dari sisi pengguna

Service dipakai Monitoring Presensi, log mesin, dan Pengaturan Presensi; perizinan pulang cepat, dinas luar, dan penutupan periode siap untuk `MVP-22` dan `MVP-23`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-B1` di `frontend-roadmap.md`, controller dan DTO `AttendanceManagement`, view `attendance-monitoring-view.jsx` lama, `attendance-management-service.js`, uji unit `workforce-core-cross-employee.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/hr/attendance-management-service.js` | Ditambah ringkasan harian, log mentah per hari, pratinjau, tutup, dan buka kembali periode, serta pengaturan |
| `src/lib/services/hr/early-leave-service.js` | Baru: izin pulang cepat |
| `src/lib/services/hr/official-duty-service.js` | Baru: surat tugas dinas luar |

### 3.3 Kepatuhan arsitektur frontend

Nama berkas `attendance-service.js` pada kartu tidak dipakai karena `attendance-management-service.js` sudah ada, dipakai rute, dan dikunci uji unit `FIX-HRD-001-05`; struktur modul memang `DEV_DISCRETION`.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task hanya mengubah service

`UI GATE: N/A — task hanya mengubah service`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — service.

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Attendance Management / Attendance Daily

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/dailies` | Daftar harian | `AttendanceDaily : Read` |
| `GET` | `/dailies/summary` | Ringkasan | `AttendanceDaily : Read` |
| `GET` | `/dailies/{id}/raw-logs` | Log mentah | `AttendanceDaily : Read` |

#### HR Attendance - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`, `PUT` | `/v1/corporate/human-resource/attendance-management/settings` | Pengaturan | `AttendanceSetting : Read` / `Update` |

#### HR Attendance - Early Leave Permissions dan HR Attendance - Official Duty

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET`, `POST` | `/attendance-management/early-leave-permissions`, `/official-duties` | Daftar, buat, setujui, tolak | `EarlyLeavePermission`, `OfficialDutyRequest` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Pencocokan alamat dengan controller backend | Seluruh alamat cocok | `PASS` | Skrip pembanding route backend |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | GET harian, ringkasan, log mentah, dan pengaturan dijawab 200 | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09-presensi-harian.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/10-log-mesin-mentah.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/11-pengaturan-presensi.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** Uji integrasi otomatis service (opsional menurut `test-policy.md`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh fungsi API terdefinisi sesuai endpoint backend | Terpenuhi | Pencocokan route backend |
| Serialisasi saringan tanggal, unit, dan paginasi akurat | Terpenuhi | `buildQueryString` dan uji browser |

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
| Langkah berikutnya | `FE-HRD-R28` dan `FE-HRD-R29` |
