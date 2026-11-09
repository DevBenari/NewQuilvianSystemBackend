# Laporan Task FE-HRD-124: Komponen Kartu Tiket Saldo Hak Libur Pengganti CTO (90 Hari Masa Aktif)

## 1. Informasi Task
* **Task ID**: `FE-HRD-124`
* **Slice / MVP**: `S-B3` / `MVP-29`
* **Nama Fitur**: Komponen Kartu Tiket Saldo Hak Libur Pengganti CTO (90 Hari Masa Aktif)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun komponen reusable `CompensatoryTimeOffCard`:
1. Menampilkan rincian tiket saldo hak libur: nomor tiket, referensi SPKL, jam diperoleh, jam tersisa, dan tanggal kedaluwarsa.
2. Visual countdown hari aktif (hijau > 30 hari, kuning 14-30 hari, merah < 14 hari, expired diff <= 0).
3. Tombol aksi 'Gunakan untuk Permohonan Cuti' yang menautkan tiket langsung ke modul pengajuan cuti Sub-08.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/overtime/CompensatoryTimeOffCard.jsx (Dibuat)`

## 4. Bukti Verifikasi
* Pengujian visual countdown dan status tiket (Active, PartiallyUsed, Expired) terverifikasi presisi.
