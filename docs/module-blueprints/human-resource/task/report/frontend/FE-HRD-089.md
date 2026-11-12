# Laporan Perubahan Frontend — `FE-HRD-089`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-089` |
| Judul | Redux Slice State Management Presensi Harian & Periode (`attendanceSlice.js`) |
| Slice | `S-B1` · `MVP-21` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-089`; dikerjakan lewat `FE-HRD-R13` |
| Trace | `03-frontend-architecture.md` bagian 16.2 |
| Contract version | API Contract v11 bagian 17 (disetujui pemilik 4 November 2026) |
| Wewenang UI | Struktur action dan selector `DEV_DISCRETION` |
| Dependency | `FE-HRD-088` ✅ |
| Klasifikasi | `LIGHT` — state |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — fetch harian, ringkasan, dan filter ada; status periode menyusul `FE-HRD-R29` |

---

## 1. Keadaan yang ditemukan di awal

Belum ada slice presensi.

---

## 2. Proses bisnis dari sisi pengguna

Saat HR mengubah tanggal atau unit di Monitoring Presensi, slice menyimpan saringan, memuat ulang daftar dan ringkasan, lalu membatalkan permintaan lama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-B1` di `frontend-roadmap.md`, controller dan DTO `AttendanceManagement`, view `attendance-monitoring-view.jsx` lama, `attendance-management-service.js`, uji unit `workforce-core-cross-employee.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/state/slice/hr/attendance/attendance-slice.js` | Slice baru: saringan, daftar, ringkasan, thunk `fetchAttendanceDailies` |
| `src/lib/state/store.jsx` | Reducer `attendance` didaftarkan |

### 3.3 Kepatuhan arsitektur frontend

Pola slice HR yang ada (`leave-slice.js`). Lokasi mengikuti repo (`src/lib/state/slice/`), bukan `src/lib/redux/` pada kartu.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task hanya menambah state

`UI GATE: N/A — task hanya menambah state`

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — state.

---

## 5. Endpoint yang dikonsumsi

Lihat `FE-HRD-088`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Saringan dan muat ulang daftar berjalan | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/09-presensi-harian.png` |

Uji manual: `PASS`.

**Tidak dijalankan:** Uji unit reducer (opsional).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Reducer menangani fetch presensi harian, update filter, dan status periode | Sebagian | Fetch dan filter ada; status periode belum |
| Selector menyediakan data tersaring | Terpenuhi | `selectAttendanceState` |

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
| Langkah berikutnya | `FE-HRD-R29` menambah status periode |
