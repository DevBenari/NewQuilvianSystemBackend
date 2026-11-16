# Laporan Perubahan Frontend — `FE-HRD-R29`

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R29` — Penutupan Periode |
| Task asal | `FE-HRD-092`–`094` |
| Tanggal | 12 November 2026 |
| Status | 🟡 Source utama selesai; metrik kontrak dan uji browser tersisa |

Ditambahkan `/hr/attendance/periods` dan menu berizin. Layar memuat periode server, badge status, preview kelayakan tutup, daftar issue pemblokir, jumlah koreksi aktif, peringatan konversi `IncompletePunch`, aksi tutup, serta buka darurat khusus permission `Reopen` dengan alasan minimum 15 karakter dan peringatan rollback payroll.

UI GATE: `REUSE/COMPOSE`; ESLint target lulus, CSS gate 1 berkas/0 temuan, build penuh lulus 813 halaman. `VISUAL CHECK: NOT FEASIBLE`.

DTO periode backend tidak menyediakan total jam kerja, total jam dinas luar, potongan keterlambatan, atau persentase kepatuhan yang diminta kartu; layar hanya menampilkan metrik yang benar-benar tersedia. Perubahan belum di-commit/push.
