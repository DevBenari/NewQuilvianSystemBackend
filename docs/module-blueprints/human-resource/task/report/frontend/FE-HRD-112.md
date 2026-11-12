# Laporan Task FE-HRD-112: Layar Inisiasi & Pemantauan Pemanggilan Darurat (Leave Recall)

## 1. Informasi Task
* **Task ID**: `FE-HRD-112`
* **Slice / MVP**: `S-B2` / `MVP-26`
* **Nama Fitur**: Layar Inisiasi Pemanggilan Darurat (`FE-HRD-154` `/hr/leave/recalls`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun antarmuka pemantauan staf cuti aktif dan inisiasi recall darurat bagi Kepala Ruangan (`HRD-DEC-113`):
1. Tabel pemantauan seluruh staf rumah sakit yang saat ini berstatus cuti aktif beserta tanggal mulai/selesai dan nomor kontak darurat.
2. Modal inisiasi pemanggilan darurat (`LeaveRecallInitiationModal.jsx`):
   - Input tanggal efektif wajib kembali berdinas.
   - Pilihan kategori kedaruratan (`KLB / Wabah`, `Bencana Alam / Korban Masal`, `Lonjakan Pasien Kritis ICU/IGD`, `Kekurangan Spesialis Mendadak`).
   - Kalkulasi otomatis estimasi hari cuti yang batal dinikmati untuk dipulihkan ke saldo pegawai.
   - Input justifikasi klinis cito tertulis.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/LeaveRecallInitiationModal.jsx` (Dibuat)
* `src/components/view/hr/leave/leave-recalls-view.jsx` (Dibuat)
* `src/app/hr/leave/recalls/page.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Rute `/hr/leave/recalls` terkompilasi bersih pada Next.js Turbopack build.
