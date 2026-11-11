# Laporan Task FE-HRD-142: Layar Pengaturan Parameter Penggajian RS, Batas Plafon BPJS & Threshold Variansi (/hr/payroll/settings)

## 1. Informasi Task
* **Task ID**: `FE-HRD-142`
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Nama Fitur**: Layar Pengaturan Parameter Penggajian RS, Batas Plafon BPJS & Threshold Variansi (/hr/payroll/settings)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan layar konfigurasi operasional dan regulasi penggajian rumah sakit (FE-HRD-142):
1. Konfigurasi kalender siklus: Tanggal cut-off bulanan (baku tgl 20), tanggal pembayaran resmi (baku tgl 25), dan premi shift malam dinas (Rp 50.000).
2. Konfigurasi batas ambang deviasi variansi gaji staf: Ambang persentase (20.0%) dan ambang nominal kritis (Rp 2.000.000).
3. Konfigurasi plafon upah regulasi dan tarif BPJS resmi pemerintah: Batas upah maksimal BPJS Kesehatan (Rp 12.000.000, 1% pekerja, 4% RS), batas plafon BPJS Ketenagakerjaan JP (Rp 10.042.300, 1% pekerja, 2% RS), tarif JHT (2% pekerja, 3.7% RS), serta batas maksimum potongan pinjaman/kasbon (50%).
4. Aksi simpan parameter terhubung ke API backend dengan persistensi data dan verifikasi audit.

## 3. Berkas yang Diubah / Dibuat
* `src/components/view/hr/payroll/payroll-settings-view.jsx (Dibuat)`
* `src/app/hr/payroll/settings/page.jsx (Dibuat)`

## 4. Bukti Verifikasi
* Lolos validasi batas nilai konfigurasi, persistensi nilai baru ke backend, ESLint 0 error, build Next.js 0 error.
