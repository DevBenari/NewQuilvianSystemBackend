# Laporan Perubahan Frontend — `FE-HRD-099`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-099` |
| Judul | Layar Pengaturan Parameter Presensi & Toleransi Waktu HR (`FE-HRD-149` `/hr/attendance/settings`) |
| Slice | `S-B1` · `MVP-21` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-099`; dikerjakan lewat `FE-HRD-R13` |
| Trace | `HRD-DEC-105`, `HRD-DEC-106`, `VR-ATT-17` |
| Contract version | API Contract v11 bagian 17 (disetujui pemilik 4 November 2026) |
| Wewenang UI | Pengelompokan isian `DEV_DISCRETION` |
| Dependency | `FE-HRD-088` ✅, `BE-HRD-105` ✅ [BE] |
| Klasifikasi | `MEDIUM` — layar baru dengan service, state, dan validasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Layar belum ada.

---

## 2. Proses bisnis dari sisi pengguna

HR Manager membuka Pengaturan Presensi, mengubah toleransi keterlambatan (0–30 menit), batas koreksi mundur (1–30 hari), kewajiban lampiran koreksi, dan konversi incomplete punch menjadi Alfa, lalu menekan Simpan. Contoh: isian 45 menit otomatis dibatasi menjadi 30. Pengguna tanpa hak melihat "Ups! Akses Ditolak" dengan keterangan "Pengaturan presensi hanya untuk HR Manager atau Admin Sistem."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-B1` di `frontend-roadmap.md`, controller dan DTO `AttendanceManagement`, view `attendance-monitoring-view.jsx` lama, `attendance-management-service.js`, uji unit `workforce-core-cross-employee.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/attendance-settings-view.jsx` | Layar baru |
| `src/app/hr/attendance/settings/page.jsx` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Grup "Presensi & Kehadiran" dengan butir Monitoring dan Pengaturan |

### 3.3 Kepatuhan arsitektur frontend

Rute → view → service `FE-HRD-088`. Parameter cut-off shift malam yang disebut outcome kartu tidak ada di DTO `UpdateAttendanceSettingRequest`, jadi tidak ditampilkan.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` | `REUSE` | — |
| Dua isian angka | `BaseInputField` | `REUSE` | — |
| Dua sakelar | `BaseSwitchField` | `REUSE` | — |
| Tombol simpan | `BaseButton` | `REUSE` | — |
| Tanpa hak akses | `AccessDeniedAlert` | `REUSE` | — |
| Galat dan notifikasi | `InformationAlert`, `ToastStack` | `REUSE` | — |
| Panel isian | `attendance.module.css` | `NEW (CSS Module)` | Nilai memakai token |

`UI GATE: PASS — 6 elemen REUSE, 0 COMPOSE, 1 NEW (CSS Module berbasis token)`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/attendance/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/11-pengaturan-presensi.png` | Nilai dari server |
| `/hr/attendance/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/12-pengaturan-presensi-validasi.png` | Isian 45 dibatasi 30 dan tersimpan |
| `/hr/attendance/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/12b-pengaturan-presensi-dipulihkan.png` | Nilai dikembalikan ke 10 |
| `/hr/attendance/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/21-tanpa-hak-pengaturan-presensi.png` | Akun tanpa hak |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Isian nonaktif |
| Kosong | `NOT APPLICABLE` — backend selalu mengembalikan satu pengaturan |
| Gagal | `InformationAlert` merah |
| Tanpa hak akses | `AccessDeniedAlert` "Pengaturan presensi hanya untuk HR Manager atau Admin Sistem." |

---

## 5. Endpoint yang dikonsumsi

#### HR Attendance - Settings

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance-management/settings` | Membaca | `AttendanceSetting : Read` |
| `PUT` | `/v1/corporate/human-resource/attendance-management/settings` | Menyimpan | `AttendanceSetting : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Baca, batas 30 menit, simpan dengan notifikasi, akses ditolak untuk akun tanpa hak | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/11-pengaturan-presensi.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/12-pengaturan-presensi-validasi.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/21-tanpa-hak-pengaturan-presensi.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** Tidak ada.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Toleransi dapat diubah dalam rentang 0–30 menit dengan validasi klien (`VR-ATT-17`) | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/12-pengaturan-presensi-validasi.png` |
| Hanya dapat diakses HR Manager atau Admin Sistem | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/21-tanpa-hak-pengaturan-presensi.png` |
| Simpan menampilkan konfirmasi dan tersimpan di server | Terpenuhi | `PUT 200` dan notifikasi "Pengaturan presensi tersimpan." |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Parameter cut-off shift malam pada outcome kartu belum ada di kontrak backend |
| Dependency backend | `NONE` |
| Perubahan sampingan | Langkah uji 12 dirancang hanya memeriksa validasi, tetapi isian 45 dibatasi otomatis menjadi 30 lalu tersimpan ke database pengembangan. Nilai segera dikembalikan ke 10 menit lewat layar yang sama, lalu dibuktikan sesudah halaman dimuat ulang (`QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/12b-pengaturan-presensi-dipulihkan.png`). Kolom `UpdatedAt`/`UpdatedBy` pengaturan itu kini tercatat atas akun SuperAdmin. |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R28` (`S-B1` MVP-22) |
