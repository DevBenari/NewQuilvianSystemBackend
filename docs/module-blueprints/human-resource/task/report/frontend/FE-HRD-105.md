# Laporan Task FE-HRD-105: Layar Manajemen Alokasi Saldo, Carry-Forward & Runner Kedaluwarsa HR

## 1. Informasi Task
* **Task ID**: `FE-HRD-105`
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Nama Fitur**: Layar Administrasi Saldo Staf HR (`FE-HRD-153` `/hr/leave/balances-admin`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun antarmuka administrasi alokasi saldo cuti bagi HR Leave Administrator:
1. Tabel agregat saldo seluruh staf rumah sakit per unit kerja dengan filter unit dan pencarian nama/NIK.
2. Kolom pemantauan hak berjalan, saldo bawaan carry-forward (disertai lencana s.d. 30 Juni), cuti terpakai, dan sisa saldo aktif.
3. Tombol eksekusi runner massal:
   - Runner Kedaluwarsa 30 Juni (`runCarryForwardMidYearExpiry`) per `HRD-DEC-110`.
   - Runner Rollover Tutup Tahun (`runCarryForwardYearEnd`).
4. Tombol aksi per baris `⚖️ Sesuaikan Saldo` yang membuka dialog modal audit `LeaveBalanceAdjustmentModal`.

## 3. Berkas yang Diubah / Dibuat
* `src/components/view/hr/leave/leave-balances-admin-view.jsx` (Dibuat)
* `src/app/hr/leave/balances-admin/page.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Rute `/hr/leave/balances-admin` terkompilasi bersih pada Next.js Turbopack build.
