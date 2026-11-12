# Laporan Task FE-HRD-167: Layar FE-HRD-178 – Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo Plafon SIMRS

## 1. Informasi Task
* **Task ID**: `FE-HRD-167`
* **Slice / MVP**: `S-D3 / MVP-37`
* **Nama Fitur**: Layar FE-HRD-178 – Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo Plafon SIMRS
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengimplementasikan antarmuka inquiry online hak subsidi nakes (benefit-inhouse-ledger-view.jsx) pada rute /corporate/hrd/benefits/inhouse-ledger. Menampilkan kartu hak kelas kamar rawat inap, persentase subsidi diskon tindakan staf, sisa saldo plafon berobat tahun berjalan dengan meteran serapan %, serta tabel histori mutasi buku besar double-entry HrdBenefitBalanceMovement (Alokasi Pagu, Pemotongan Tagihan SIMRS, dan Penyesuaian Koreksi).

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/benefits/benefit-inhouse-ledger-view.jsx`
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/benefits/inhouse-ledger/page.jsx`

## 4. Bukti Verifikasi
* Pencarian real-time via NIK menampilkan profil hak dan saldo, tabel buku besar mencatat mutasi debit/kredit secara akurat.
