# Laporan Perubahan Frontend — `FE-HRD-082`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-082` |
| Judul | Aksi UI Koreksi Mundur Jadwal (<= 3 Hari) & Banner Proteksi Periode Terkunci (Closed Lock) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-19` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-102, VR-SCH-17, VR-SCH-18, BE-HRD-098` |
| Contract version | API Contract v10 /retroactive-correct & /close |
| Dependency | `FE-HRD-077 (✅ Selesai), BE-HRD-098 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx, src/components/view/hr/scheduling/roster-matrix-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pengendalian perubahan jadwal ke tanggal lampau dan penguncian total integritas data penggajian:
1. **Pembatasan Koreksi Mundur (VR-SCH-17)**:
   - Validasi toleransi tanggal mundur maksimal 3 hari kalender ke belakang via utilitas isRetroactiveAllowed.
   - Perubahan untuk tanggal lampau > 3 hari ditolak otomatis oleh antarmuka dengan pesan penolakan resmi.
2. **Banner Proteksi Periode Ditutup (Closed)**:
   - Ketika periode berstatus "Closed" (telah ditutup oleh HRD Payroll), seluruh tombol aksi dan sel jadwal dikunci menjadi baca-saja (read-only).
   - Menampilkan banner informatif abu-abu bahwa data telah dibekukan untuk rekonsiliasi payroll.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Sel jadwal tanggal > 3 hari lalu memvalidasi aturan toleransi koreksi mundur (VR-SCH-17) | ✅ PASS | RetroactiveShiftCorrectionModal memeriksa isRetroactiveAllowed dan menolak submit jika lewat batas. |
| 2 | Periode Closed menonaktifkan seluruh tombol aksi dan mengunci tabel jadwal | ✅ PASS | Kondisi isClosed menyembunyikan tombol manipulasi dan mengaktifkan kelas readOnly. |
| 3 | Banner penguncian abu-abu tampil menjelaskan periode telah ditutup | ✅ PASS | Komponen closedBanner tampil di atas tabel matriks. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx (Modal koreksi mundur 3 hari)`
- `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx (Banner proteksi periode terkunci)`
