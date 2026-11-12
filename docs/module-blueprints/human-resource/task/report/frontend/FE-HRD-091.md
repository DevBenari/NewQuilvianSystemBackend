# Laporan Perubahan Frontend — `FE-HRD-091`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-091` |
| Judul | Komponen Drawer Riwayat Log Mesin Biometrik Mentah (`RawPunchAuditDrawer.jsx`) |
| Slice | `S-B1` · `MVP-21` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-091`; dikerjakan lewat `FE-HRD-R13` |
| Trace | `HRD-DEC-104` |
| Contract version | API Contract v11 bagian 17 (disetujui pemilik 4 November 2026) |
| Wewenang UI | Format tabel di dalam panel `DEV_DISCRETION` |
| Dependency | `FE-HRD-090` 🟡, `BE-HRD-107` ✅ [BE] |
| Klasifikasi | `LIGHT` — satu komponen baca saja |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Komponen belum ada.

---

## 2. Proses bisnis dari sisi pengguna

Dari baris presensi, HR menekan Log Mesin. Panel menampilkan setiap tap mesin pada tanggal dinas itu dengan waktu sampai detik, jenis tap, mesin, metode, lokasi, dan status proses, di bawah label "[DATA MENTAH MESIN - IMMUTABLE]". Tidak ada kontrol ubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-B1` di `frontend-roadmap.md`, controller dan DTO `AttendanceManagement`, view `attendance-monitoring-view.jsx` lama, `attendance-management-service.js`, uji unit `workforce-core-cross-employee.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/attendance/RawPunchAuditDrawer.jsx` | Komponen baru |

### 3.3 Kepatuhan arsitektur frontend

Base drawer belum ada (`DEC-FER-01`), jadi panel memakai `ConfirmModal` dengan `DataTable`; diganti `BaseDrawer` pada `FE-HRD-R50` bila disetujui.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Panel | `ConfirmModal` (`hideCancel`, `size="xl"`) | `COMPOSE` | Tanpa base drawer |
| Tabel log | `DataTable` (`pagination={false}`) | `REUSE` | — |
| Label immutable | `attendance.module.css` | `NEW (CSS Module)` | Nilai memakai token |
| Galat | `InformationAlert` | `REUSE` | — |

`UI GATE: PASS — 2 elemen REUSE, 1 COMPOSE, 1 NEW (CSS Module berbasis token)`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/attendance` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/10-log-mesin-mentah.png` | Log mentah dari server |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat log mesin..." |
| Kosong | "Tidak ada log mesin pada tanggal dinas ini." |
| Gagal | `InformationAlert` merah |
| Tanpa hak akses | Server menolak 403 dan pesannya tampil |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Attendance Management / Attendance Daily

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/attendance/dailies/{id}/raw-logs` | Log mentah | `AttendanceDaily : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Log mentah dan label tampil | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/10-log-mesin-mentah.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** Tidak ada.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Menampilkan seluruh tap mentah pegawai pada tanggal terpilih | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/10-log-mesin-mentah.png` |
| Menegaskan label `[DATA MENTAH MESIN - IMMUTABLE]` | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/10-log-mesin-mentah.png` |

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
| Langkah berikutnya | `FE-HRD-R50` bila `DEC-FER-01` disetujui |
