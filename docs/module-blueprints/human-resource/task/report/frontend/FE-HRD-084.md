# Laporan Perubahan Frontend — `FE-HRD-084`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-084` |
| Judul | Layar Kalender & Daftar Penugasan Siaga (On-Call Coverage) (FE-HRD-141, /hr/scheduling/on-call) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-20` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-103, 03-frontend-architecture.md 15.3.4, BE-HRD-102` |
| Contract version | API Contract v10 /on-call-assignments |
| Dependency | `FE-HRD-083 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/on-call-scheduling-view.jsx, src/app/hr/scheduling/on-call/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Membangun antarmuka pemantauan penugasan siaga on-call dokter spesialis dan analis cito instalasi rumah sakit:
1. **Summary Cards**:
   - Total dokter siaga standby aktif, staf terkonfirmasi, total panggilan cito, dan persentase kepatuhan SLA.
2. **Tabel Penugasan Siaga**:
   - Menampilkan dokter/staf, waktu mulai dan selesai siaga, peran (Primary/Backup), status badge resmi (Scheduled, Confirmed, Active, Completed), dan status respon SLA.
3. **Aksi Alur Kerja Interaktif**:
   - Tombol Konfirmasi, Aktivasi Siaga, Panggil Cito (membuka modal Call-In), dan Selesaikan Siaga.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Menampilkan jadwal siaga dokter dan perawat cito secara jelas | ✅ PASS | OnCallSchedulingView menyajikan tabel filter status dan detail waktu siaga. |
| 2 | Staf siaga yang sedang aktif menampilkan badge hijau Active | ✅ PASS | formatOnCallStatus memetakan badge warna sesuai siklus penugasan. |
| 3 | Menyediakan tombol aksi Panggil Cito untuk unit yang berwenang | ✅ PASS | Tombol Panggil Cito terpasang pada baris penugasan berstatus Active. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/on-call-scheduling-view.jsx (Layar daftar penugasan siaga)`
- `src/app/hr/scheduling/on-call/page.jsx (Rute App Router on-call)`
