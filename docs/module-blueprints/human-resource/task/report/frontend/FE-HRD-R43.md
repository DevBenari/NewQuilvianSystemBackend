# Laporan Perubahan Frontend — `FE-HRD-R43`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R43` |
| Judul | Verifikasi `S-B1` |
| Slice | `S-B1` (Presensi & Kehadiran), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R43`; task asal `FE-HRD-101` |
| Trace | API Contract v11; laporan `FE-HRD-R13`, `FE-HRD-R28`, `FE-HRD-R29` |
| Contract version | API Contract v11 + source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Verifikasi menu, hak akses, dan keterjangkauan; perbaikan kecil keterjangkauan |
| Dependency | `FE-HRD-R28` 🟡, `FE-HRD-R29` 🟡 |
| Klasifikasi | `LIGHT` — verifikasi 8 rute, 2 berkas diubah |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — verifikasi statis lulus setelah dua layar ESS dipasang di menu; uji browser live (`live-browser-testing`) belum dapat dijalankan |

---

## 1. Keadaan yang ditemukan di awal

- Enam layar admin S-B1 sudah ada di menu "Presensi & Kehadiran" dengan `requiredPermission`.
- Dua layar ESS S-B1 — **Presensi Saya** (`/self-services/human-resource/attendance`) dan **Koreksi Presensi** (`/self-services/human-resource/attendance-correction`) — tidak terjangkau dari menu samping maupun dari portal ESS.

## 2. Proses bisnis dari sisi pengguna

1. Petugas HR membuka menu **Presensi & Kehadiran** untuk monitoring harian, pengaturan, koreksi, izin pulang cepat, dinas luar, dan periode presensi; setiap menu hanya tampil bila pengguna punya hak baca terkait.
2. Pegawai membuka **Presensi Saya** dan **Koreksi Presensi Saya** dari grup menu layanan mandiri, atau dari tautan cepat di **Portal Layanan Mandiri (ESS)**.
3. Jalur tidak normal: pengguna tanpa hak tidak melihat menu terkait; aksi yang ditolak server (403) tampil sebagai pesan galat di layar.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Delapan `page.jsx` S-B1, view terkait, `menu-items.jsx`, `unified-ess-portal-view.jsx`, controller presensi backend (`AttendanceDailyController`, `AttendanceSettingController`, `AttendanceCorrectionMonitoringController`, `EarlyLeavePermissionController`, `OfficialDutyRequestController`, `AttendancePeriodController`, `AttendanceCorrectionController`, `AttendanceSelfServiceController`, `AttendanceCorrectionSelfServiceController`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Menu "Presensi Saya" dan "Koreksi Presensi Saya" (`MyAttendanceCorrection : Read`) di grup layanan mandiri |
| `src/components/view/hr/approvals/unified-ess-portal-view.jsx` | Dua tautan cepat: Presensi Saya, Koreksi Presensi |

### 3.3 Kepatuhan arsitektur frontend

Hanya konfigurasi menu dan tautan; tidak ada pola baru.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` untuk komponen baru — R43 hanya menambah entri menu dan tautan pada komponen yang sudah memakai base component.

```text
$ npx eslint src/utils/menu-sidebar/menu-items.jsx src/components/view/hr/approvals/unified-ess-portal-view.jsx
(0 error, 0 warning)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama akan menulis data sesi/audit, di luar wewenang mode `FRONTEND`. Uji `live-browser-testing` per peran belum dijalankan.

## 4. State yang ditangani di layar

Tidak berubah; ditangani oleh layar S-B1 masing-masing (lihat laporan `FE-HRD-R13`, `R28`, `R29`).

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — tidak ada pemanggilan API baru. Matriks keterjangkauan dan hak akses yang diverifikasi:

| Rute | Menu | `requiredPermission` | Hak di backend | Gerbang aksi di layar |
| --- | --- | --- | --- | --- |
| `/hr/attendance` | Monitoring Presensi Harian | `AttendanceDaily : Read` | Ada | `AttendanceCorrection : CreateOnBehalf` (ada) |
| `/hr/attendance/settings` | Pengaturan Presensi | `AttendanceSetting : Read` | Ada | `AttendanceSetting : Read/Update` (ada) |
| `/hr/attendance/corrections` | Koreksi Presensi | `AttendanceCorrectionMonitoring : Read` | Ada | Tanpa gerbang aksi di UI (403 server) |
| `/hr/attendance/early-leave` | Izin Pulang Cepat | `EarlyLeavePermission : Read` | Ada | Tanpa gerbang aksi di UI (`Approve` ada di server) |
| `/hr/attendance/official-duty` | Dinas Luar | `OfficialDutyRequest : Read` | Ada | Tanpa gerbang aksi di UI (`Approve` ada di server) |
| `/hr/attendance/periods` | Periode Presensi | `AttendancePeriod : Read` | Ada | `AttendancePeriod : Close/Reopen` (ada) |
| `/self-services/human-resource/attendance` | Presensi Saya (BARU) | — (controller tanpa `AccessPermission`) | — | — |
| `/self-services/human-resource/attendance-correction` | Koreksi Presensi Saya (BARU) | `MyAttendanceCorrection : Read` | Ada | — |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pencocokan 8 rute S-B1 terhadap menu | 6/8 di awal → 8/8 setelah perbaikan | `PASS` | Tabel bagian 5 |
| Pencocokan permission menu dan layar terhadap `[AccessPermission]` backend | Semua nama ada di controller | `PASS` | grep controller |
| `npx eslint` 2 berkas | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `npm run build` BUILD-2 | 832/832 halaman, `BUILD_EXIT=0` | `PASS` | Log build |
| `live-browser-testing` per peran | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh layar terjangkau dari menu | Terpenuhi (source) | 8/8 rute punya entri menu; ESS juga dari portal |
| Hak akses benar | Terpenuhi (statis) | Seluruh permission cocok dengan backend |
| Bukti uji browser per peran | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga layar admin (koreksi, izin pulang cepat, dinas luar) belum menyembunyikan tombol aksi per hak; penolakan mengandalkan 403 server |
| Masalah yang diketahui | `AttendanceSelfServiceController` tidak memakai `AccessPermission`, sehingga menu Presensi Saya tidak bergerbang |
| Dependency backend | Pertimbangkan `AccessPermission` pada presensi mandiri |
| Perubahan sampingan | `NONE` |
| Interupsi | Konteks sesi dipadatkan sekali |
| Status Git | 81 entri belum di-commit di repo frontend untuk Gelombang 3 |
| Langkah berikutnya | Jalankan `live-browser-testing` S-B1 per peran saat backend dapat dinyalakan dengan izin pemilik |
