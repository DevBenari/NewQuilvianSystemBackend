# Laporan Task FE-HRD-113: Kotak Masuk Approval Leave Recall HR Manager & Restore Saldo Sisa

## 1. Informasi Task
* **Task ID**: `FE-HRD-113`
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Nama Fitur**: Antrean Otorisasi Recall HR Manager (`LeaveRecallApprovalInbox`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun panel otorisasi tunggal bagi peran HR Manager pada alur pemanggilan darurat (`HRD-DEC-113`, `HRD-Q-36`, `LEV-B5`):
1. Meninjau tiket usulan recall yang diajukan oleh Kepala Ruangan lengkap dengan justifikasi klinis dan kategori kedaruratan.
2. Tombol aksi `Setujui & Pulihkan Saldo`: mengeksekusi recall dinas cito dan secara otomatis memulihkan sisa hari cuti yang batal dinikmati kembali ke buku besar mutasi (`TrxLeaveBalanceLedger`) sebagai transaksi `RecallRestore`.
3. Tombol aksi `Tolak`: menolak usulan recall dengan alasan tertulis jika kebutuhan dinas dapat diatasi dengan staf siaga lain.

## 3. Berkas yang Diubah / Dibuat
* Terintegrasi pada tab `Kotak Masuk Approval HR Manager` di `src/components/view/hr/leave/leave-recalls-view.jsx`.
* Service client `approveRecall` & `rejectRecall` di `leave-service.js`.

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Otorisasi pemulihan saldo cuti teruji aman dan transparan.
