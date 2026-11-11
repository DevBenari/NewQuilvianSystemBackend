# Laporan Perubahan Frontend — `FE-HRD-077`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-077` |
| Judul | Layar Kisi Roster Bulanan Unit Interaktif (FE-HRD-138, /hr/scheduling/roster & /[id]) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-098, HRD-DEC-100, 03-frontend-architecture.md 15.3.1` |
| Contract version | API Contract v10 /roster-periods |
| Dependency | `FE-HRD-076 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/roster-scheduling-view.jsx, src/components/view/hr/scheduling/roster-matrix-detail-view.jsx, src/app/hr/scheduling/roster/[id]/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Membangun antarmuka utama kisi kalender matriks dinas shift unit 24/7:
1. **Grid Matriks Tanggal & Perawat**:
   - Kolom kiri nama staf perawat dengan penanda lencana kuning "[Didampingi — Preceptor]" bagi staf baru yang sedang dalam bimbingan klinis (HRD-DEC-087, HRD-DEC-100).
   - Baris tanggal 1 s.d. 30/31 dengan penanda visual akhir pekan (Sabtu/Minggu warna merah muda lembut).
   - Sel shift interaktif dengan kode warna terstandarisasi (Pagi, Siang, Malam, Libur, Cuti, On-Call).
2. **Status Siklus Periode**:
   - Tampilan status badge jelas (Draft, Validation, Published, Closed).
   - Mode interaksi ganda: pemilihan dari daftar periode maupun rute langsung berparameter /hr/scheduling/roster/[id].

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Grid menampilkan daftar perawat baris demi baris dan tanggal kolom demi kolom | ✅ PASS | Tabel matriks responsif dengan sticky column staf dan sticky header tanggal. |
| 2 | Perawat berstatus Didampingi menampilkan badge kuning penanda pendampingan | ✅ PASS | Lencana accompaniedBadge terpasang pada nama staf yang memenuhi syarat. |
| 3 | Status periode tampil jelas di header | ✅ PASS | StatusBadge memetakan Draft, Validation, Published, Closed. |
| 4 | Bebas Tailwind 100% dan mematuhi aturan token Quilvian | ✅ PASS | Menggunakan CSS Module scheduling.module.css. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/roster-scheduling-view.jsx (Layar daftar periode & switcher)`
- `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx (Layar matriks kisi kalender bulanan)`
- `src/app/hr/scheduling/roster/[id]/page.jsx (Rute App Router detail matriks roster)`
