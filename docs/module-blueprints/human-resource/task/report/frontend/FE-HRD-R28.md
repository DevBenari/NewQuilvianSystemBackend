# Laporan Perubahan Frontend — `FE-HRD-R28`

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R28` — Koreksi, Izin, Dinas Luar |
| Task asal | `FE-HRD-095`–`098`, `100` |
| Tanggal | 12 November 2026 |
| Status | 🟡 Sebagian karena gap kontrak backend dan ESS |

Ditambahkan layar/menu admin Koreksi Presensi, Izin Pulang Cepat `[TIDAK MEMOTONG CUTI]`, dan Dinas Luar dengan data API nyata, status, pagination, detail persetujuan, label `OnDuty Disetujui`, serta tautan lampiran bila tersedia. Service attendance diperluas untuk monitoring koreksi dan izin pulang cepat.

UI GATE: `REUSE/COMPOSE`; tidak ada base component baru. ESLint target lulus, CSS gate 3 berkas/0 temuan, build penuh lulus 813 halaman. `VISUAL CHECK: NOT FEASIBLE`.

Belum dapat dipenuhi aman: backend koreksi kuasa menerima `AttendanceDailyId` dan metadata path berkas, tetapi tidak menyediakan endpoint unggah bukti; DTO daftar monitoring juga tidak memberi penanda eksplisit ESS vs HR-kuasa. Ekstensi ESS `FE-HRD-100` belum dipasang karena kontrak self-service untuk dua pengajuan tersebut tidak tersedia. Perubahan belum di-commit/push.
