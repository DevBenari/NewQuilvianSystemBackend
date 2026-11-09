# Laporan Task FE-HRD-135: Komponen Drawer & Layar Investigasi Anomali Variansi Gaji Pegawai (/hr/payroll/runs/[id]/variances)

## 1. Informasi Task
* **Task ID**: `FE-HRD-135`
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Nama Fitur**: Komponen Drawer & Layar Investigasi Anomali Variansi Gaji Pegawai (/hr/payroll/runs/[id]/variances)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan modul investigasi anomali variansi gaji (FE-HRD-165) mencakup komponen drawer samping (PayrollVarianceDrawer.jsx) dan layar terdedikasi (payroll-variances-view.jsx):
1. Memindai dan menyajikan seluruh staf dengan deviasi gaji bersih >= 20% atau nominal >= Rp 2.000.000 (VR-PAY-14).
2. Menyajikan kotak komparasi side-by-side nominal gaji bulan lalu vs bulan berjalan, delta nominal rupiah, dan persentase perubahan.
3. Memaparkan faktor pemicu lonjakan dari sistem (tindakan on-call cito, penambahan shift malam, rapel insentif spesialis).
4. Formulir input justifikasi audit compliance dengan validasi ketat minimal 20 karakter (VR-PAY-15); tombol simpan terkunci jika karakter belum mencukupi.
5. Penyelesaian tiket mengubah status menjadi Resolved dan secara reaktif mengurangi counter variansi pending.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/payroll/PayrollVarianceDrawer.jsx (Dibuat)`
* `src/components/view/hr/payroll/payroll-variances-view.jsx (Dibuat)`
* `src/app/hr/payroll/runs/[id]/variances/page.jsx (Dibuat)`

## 4. Bukti Verifikasi
* Lolos uji penolakan teks justifikasi < 20 karakter, pembaharuan status tiket Resolved, ESLint 0 error, build Next.js 0 error.
