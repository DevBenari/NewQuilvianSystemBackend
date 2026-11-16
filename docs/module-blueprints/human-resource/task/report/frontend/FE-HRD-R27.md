# Laporan Perubahan Frontend — `FE-HRD-R27`

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R27` — Rencana Tahunan |
| Task asal | `FE-HRD-041`, `FE-HRD-042` |
| Tanggal | 12 November 2026 |
| Status | 🟡 Source selesai; keterbatasan kontrak dan uji browser tersisa |

Rute daftar dan detail `/hr/workforce-planning/manpower-plans` telah dibuat. Fitur mencakup pembuatan rencana, addendum, baris unit-jabatan-grade, hitung ulang backend, pengajuan, koreksi Finance, biaya berlabel “estimasi” yang dijaga permission/costVisible, serta perbandingan potret gap pengajuan dengan angka sekarang. Service, konstanta, dan menu memakai kontrak controller aktual tanpa data contoh.

UI GATE: seluruh elemen `REUSE/COMPOSE` dari Hero, DataFilter, DataTable, base form controls, ConfirmModal, StatusBadge, resource selector, dan HrPanel; tidak ada base component baru.

Verifikasi: ESLint target 0 error/warning, CSS gate 2 berkas/0 temuan, dan build penuh lulus 813 halaman. `VISUAL CHECK: NOT FEASIBLE` karena sesi tidak memiliki alat browser.

Keterbatasan backend: kartu meminta cancel, tetapi `HrdManpowerPlanController` tidak mempunyai endpoint cancel. Respons juga tidak menyediakan `availableActions` langkah workflow Finance; frontend membatasi lewat status dan permission, lalu backend tetap menjadi guard akhir. Perubahan belum di-commit/push.
