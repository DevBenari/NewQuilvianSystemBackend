# Laporan Task FE-HRD-280: Layar Lembar Rincian Permohonan Persetujuan, Stepper Berjenjang & Rute App Router

## 1. Informasi Task
* **Task ID**: `FE-HRD-280`
* **Slice / MVP**: `S-A7-V2 / Kotak Masuk Persetujuan Terpadu & ESS V2`
* **Nama Fitur**: Layar Lembar Rincian Permohonan Persetujuan, Stepper Berjenjang & Rute App Router
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun lembar detail komprehensif bagi pejabat penyetuju untuk memeriksa seluruh fakta transaksi sebelum mengambil keputusan:
1. **Layar Detail Permohonan (`unified-approval-detail-view.jsx`)**:
   - Menampilkan rincian identitas pemohon, tipe transaksi, periode waktu, nominal anggaran/restitusi, dan alasan permohonan.
   - Menyediakan panel konteks bisnis: sisa saldo hak cuti/plafon staf saat ini, histori keterlambatan/kehadiran terkini, serta catatan mitigasi risiko.
2. **Visual Workflow Stepper (`WorkflowStepperCard.jsx`)**:
   - Menampilkan rantai persetujuan berjenjang multi-level secara grafis (Contoh: Level 1 - Kepala Ruangan, Level 2 - Manajer Keperawatan, Level 3 - Direktur Medik).
   - Menandai status masing-masing simpul: Selesai Disetujui (Centang Hijau), Sedang Berlangsung (Biru Aktif), Menunggu Giliran (Abu-abu), atau Ditolak (Silang Merah).
3. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/approvals/[id]`: Rute rincian tiket persetujuan pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/approvals/[id]`: Rute rincian tiket persetujuan pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/unified-approval-detail-view.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/components/WorkflowStepperCard.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/approvals/[id]/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/approvals/[id]/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Rincian permohonan memuat data asli transaksi; stepper alur berjenjang merender simpul aktif secara deterministik; rute dinamis `[id]` memuat parameter id dengan benar.
* **Standar UI**: Zero-Tailwind, CSS Modules `approvals.module.css`, dan Design Tokens Quilvian.
