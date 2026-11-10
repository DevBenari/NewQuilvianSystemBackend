# Laporan Task FE-HRD-104: Layar Dasbor Kuota & Saldo Cuti Pegawai ESS & Drawer Buku Besar

## 1. Informasi Task
* **Task ID**: `FE-HRD-104`
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Nama Fitur**: Layar Dasbor Saldo Mandiri ESS (`FE-HRD-150` `/hr/leave/my-balance`) & Drawer Mutasi
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan antarmuka portal mandiri pegawai (ESS) untuk:
1. Memantau kuota cuti pribadi secara transparan dengan `LeaveBalanceMetricCards`.
2. Drawer slide-in buku besar mutasi saldo (`LeaveLedgerDrawer.jsx`) yang menampilkan rincian kredit hak dan debit pemakaian (`TrxLeaveBalanceLedger`).
3. Tabel riwayat permohonan cuti pribadi lengkap dengan status badge (`Submitted`, `ApprovedRoom`, `ApprovedWithMitigation`, `ApprovedHR`, dll.) dan tombol pembatalan mandiri jika tiket masih berstatus pending.
4. Tombol cepat `+ Ajukan Cuti Baru` yang mengarahkan ke `/hr/leave/request`.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/LeaveLedgerDrawer.jsx` (Dibuat)
* `src/components/view/hr/leave/my-leave-balance-view.jsx` (Dibuat)
* `src/app/hr/leave/my-balance/page.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Rute `/hr/leave/my-balance` terkompilasi bersih pada Next.js Turbopack build.
