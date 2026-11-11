# Laporan Perubahan Frontend — `FE-HRD-078`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-078` |
| Judul | Komponen Aksi Massal: Salin Seluruh Jadwal Periode Lalu & Terapkan Pola Shift Berulang |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-098, FR-SCH-02, FR-SCH-03` |
| Contract version | API Contract v10 /copy-previous dan /apply-pattern |
| Dependency | `FE-HRD-077 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/modals/CopyPreviousPeriodModal.jsx, src/components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Akselerasi kerja Kepala Unit dalam menyusun jadwal dinas bulanan tanpa harus mengisi ratusan sel secara manual satu per satu:
1. **Modal Salin Periode Lalu (CopyPreviousPeriodModal)**:
   - Memilih periode sumber terdahulu dan menyalin susunan shift ke periode aktif dalam sekali klik.
2. **Modal Terapkan Pola Shift (ApplyShiftPatternModal)**:
   - Pilihan pola dinas rotasi RS (misal: P-S-M-L-L 5 hari, 2P-2S-2M-2L 8 hari, Non-Shift).
   - Opsi target fleksibel: diterapkan ke seluruh staf unit atau ke perawat tertentu yang dipilih.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Klik Salin Periode Lalu menampilkan konfirmasi dan mengisi seluruh grid seketika | ✅ PASS | CopyPreviousPeriodModal terhubung ke RosterSchedulingService.copyPreviousPeriod. |
| 2 | Terapkan pola shift dapat diaplikasikan ke satu perawat atau seluruh perawat | ✅ PASS | ApplyShiftPatternModal menyediakan opsi radio ALL vs SELECTED staf. |
| 3 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Button Quilvian. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/modals/CopyPreviousPeriodModal.jsx (Modal salin jadwal)`
- `src/components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx (Modal pola shift berulang)`
