# Laporan Perubahan Frontend — `FE-HRD-079`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-079` |
| Judul | Drawer Validasi Jadwal Interaktif: Blokir Fatal B1–B7 & Peringatan W1–W7 dengan Alasan Wajib |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-099, HRD-DEC-100, VR-SCH-01 s.d. VR-SCH-16` |
| Contract version | Validation Matrix v10 15.1 & 15.2 |
| Dependency | `FE-HRD-077 (✅ Selesai), BE-HRD-090 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/drawers/RosterValidationDrawer.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penegakan standar keselamatan pasien dan keselamatan nakes RS melalui drawer interaktif hasil audit jadwal:
1. **Pelanggaran Fatal Pemblokir (Blocking B1–B7)**:
   - B1: Double shift hari sama; B2: Jeda istirahat < 11 jam; B3: Shift malam berturut-turut > 2; B4: Kerja berurutan > 6 hari; B5: Bentrok cuti/sakit; B6: Tanpa STR aktif; B7: Staf didampingi tanpa kehadiran preceptor.
   - Ditampilkan dengan lencana merah dan memblokir penerbitan roster secara mutlak.
2. **Peringatan Operasional (Warning W1–W7)**:
   - W1: Rasio tenaga di bawah minimal; W2: Ketimpangan senior-junior; W3: Koreksi mundur; W4: Kerja > 40 jam/minggu; W5: Lintas unit; W6: Kurang sertifikasi BLS; W7: Pemberitahuan mendadak < 24 jam.
   - Mengharuskan Kepala Unit memilih alasan dispensasi resmi dari dropdown sebelum jadwal dapat diterbitkan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Pelanggaran B1–B7 ditampilkan merah dan memblokir tombol terbit | ✅ PASS | RosterValidationDrawer memisahkan blockingItems dan menolak publish jika count > 0. |
| 2 | Peringatan W1–W7 menampilkan pilihan alasan dispensasi wajib | ✅ PASS | Dropdown WARNING_REASON_OPTIONS terintegrasi pada setiap butir peringatan. |
| 3 | Mengisi seluruh alasan mengubah indikator status menjadi siap terbit | ✅ PASS | Kondisi allWarningsResolved mengaktifkan tombol Terbitkan Roster Mandiri. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/drawers/RosterValidationDrawer.jsx (Drawer audit keselamatan dinas B1-B7 & W1-W7)`
