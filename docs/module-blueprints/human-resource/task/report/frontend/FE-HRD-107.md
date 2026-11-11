# Laporan Task FE-HRD-107: Komponen Kalender Cuti, Pemilih Sesi Half-Day 0.5 Hari & Validasi Saldo

## 1. Informasi Task
* **Task ID**: `FE-HRD-107`
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Nama Fitur**: Komponen Pemilih Tanggal Pintar (`LeaveDateRangePicker`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun komponen reusable `LeaveDateRangePicker.jsx` untuk formulir cuti:
1. Pemilih rentang tanggal mulai dan selesai dinas.
2. Toggle opsi Cuti Setengah Hari (*Half-Day Leave* 0.5 hari) yang hanya aktif saat jenis cuti adalah Cuti Tahunan (`VR-LEV-05`, `HRD-ASM-20`).
3. Segmented control pemilih sesi dinas setengah hari: Sesi Pagi (07:00–12:00) vs Sesi Siang/Sore (12:00–17:00).
4. Kotak evaluasi durasi hari efektif vs saldo aktif pegawai dengan indikator peringatan merah bila durasi melebihi sisa saldo aktif.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/LeaveDateRangePicker.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Kalkulasi 0.5 hari dan boundary check saldo teruji presisi.
