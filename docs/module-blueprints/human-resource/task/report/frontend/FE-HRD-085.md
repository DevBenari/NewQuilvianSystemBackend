# Laporan Perubahan Frontend — `FE-HRD-085`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-085` |
| Judul | Modal Pencatatan Pemanggilan Cito (Call-In) & Indikator Kepatuhan Target Waktu Respon SLA Menit |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-20` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-103, BE-HRD-100` |
| Contract version | API Contract v10 /on-call-assignments/{id}/call-in |
| Dependency | `FE-HRD-084 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/components/view/hr/scheduling/modals/OnCallCallInModal.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Fasilitas pencatatan pemanggilan darurat bagi unit pemanggil (IGD, OK Bedah, Laboratorium Cito):
1. **Formulir Panggilan Cito**:
   - Input waktu kontak telepon (callInTime) dan waktu kedatangan staf di rumah sakit (arrivalTime).
2. **Kalkulasi & Evaluasi SLA Real-Time**:
   - Menghitung durasi selisih waktu respon secara langsung di form dan membandingkannya dengan target SLA (default 30 menit).
   - Menampilkan lencana hijau "[MEMENUHI TARGET SLA (25 Menit)]" atau lencana merah "[MELAMPAUI TARGET SLA (45 Menit)]".
3. **Validasi Waktu**:
   - Mencegah pengisian waktu kedatangan yang lebih awal daripada waktu panggilan.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Form mencatat waktu panggilan telepon dan waktu tiba dokter | ✅ PASS | OnCallCallInModal menyediakan input datetime-local presisi. |
| 2 | Selisih menit dihitung real-time dan dibandingkan dengan target SLA | ✅ PASS | Hook useMemo slaEvaluation menghitung diffMinutes dan status isMet. |
| 3 | Badge hijau atau merah tampil sesuai hasil evaluasi respon | ✅ PASS | slaBadgeMet dan slaBadgeOver terpasang di modal. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/components/view/hr/scheduling/modals/OnCallCallInModal.jsx (Modal call-in & evaluasi SLA)`
