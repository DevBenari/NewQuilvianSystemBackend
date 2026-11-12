# Laporan Task FE-HRD-148: Komponen Lembar Kerja Master Jadwal Angsuran Pinjaman RS & Modal Simulasi Hard-Cap 50% (P1-P4 Waterfall)

## 1. Informasi Task
* **Task ID**: `FE-HRD-148`
* **Slice / MVP**: `S-B6 / MVP-32`
* **Nama Fitur**: Komponen Lembar Kerja Master Jadwal Angsuran Pinjaman RS & Modal Simulasi Hard-Cap 50% (P1-P4 Waterfall)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengimplementasikan modal simulasi pemotongan berjenjang (DeductionCapSimulationModal.jsx) dan lembar kerja jadwal angsuran master (deduction-schedules-view.jsx). Menerapkan kalkulasi waterfall 4 tingkat prioritas (P1 Ganti Rugi Aset -> P2 Kasbon RS -> P3 Koperasi -> P4 Iuran Paguyuban) di mana jika total cicilan melebihi 50% upah kotor, sisa cicilan terendah otomatis dipangkas dan ditunda (deferred carry-over) ke bulan berikutnya.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/features/hr/restitution/DeductionCapSimulationModal.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hr/restitution/deduction-schedules-view.jsx`

## 4. Bukti Verifikasi
* Simulasi waterfall mematuhi batas 50% upah kotor, sisa cicilan P4/P3 tertunda otomatis tercatat dalam lembar simulasi.
