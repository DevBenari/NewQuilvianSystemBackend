# Laporan Task FE-HRD-102: Service Client API Cuti, Perizinan, dan Redux State Management

## 1. Informasi Task
* **Task ID**: `FE-HRD-102`
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Nama Fitur**: Modul Komunikasi API Axios & Redux Slice Cuti Pegawai
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan modul komunikasi API Axios terpusat (`leave-service.js`) dan state management Redux Toolkit (`leave-slice.js`) yang mencakup 22 endpoint cuti rumah sakit:
1. Saldo Mandiri Pegawai & Buku Besar Mutasi (`GET /balances/my`, `GET /balances/ledger`).
2. Manajemen Saldo Seluruh Staf & Runner Carry-Forward (`GET /balances`, `POST /balances/carry-forward/run-*`).
3. Penyesuaian Saldo Berdokumen Audit (`POST /balances/adjustment`).
4. Pengajuan Cuti Mandiri ESS & Pembatalan (`POST /requests`, `GET /requests/my`, `POST /requests/{id}/cancel`).
5. Unggah Surat Sakit Dokter Ber-SIP (`POST /requests/{id}/medical-certificate`).
6. Kotak Masuk Persetujuan Atasan (`GET /approvals/inbox`, `POST /approvals/{id}/approve`, `reject`, `revise`).
7. Pemanggilan Dinas Darurat / Leave Recall (`GET /recalls`, `POST /recalls`, `POST /recalls/{id}/approve`, `reject`).
8. Rekonsiliasi Akhir & Penutupan Periode Payroll Close (`GET /reconciliations/pending`, `POST /reconciliations/close-period`).

## 3. Berkas yang Diubah / Dibuat
* `src/lib/services/hr/leave-service.js` (Dibuat)
* `src/lib/state/slice/hr/leave/leave-slice.js` (Dibuat)
* `src/lib/state/store.jsx` (Didaftarkan ke root store sebagai `leave: leaveSlice`)

## 4. Bukti Verifikasi
* ESLint 0 error pada `leave-service.js` dan `leave-slice.js`.
* Kompilasi Next.js Turbopack (`npm run build`) sukses exit code 0.
