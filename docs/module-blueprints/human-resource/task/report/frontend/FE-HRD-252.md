# Laporan Task FE-HRD-252: Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi

## 1. Informasi Task
* **Task ID**: `FE-HRD-252`
* **Slice / MVP**: `S-C5 / MVP-54`
* **Nama Fitur**: Komponen Modal Sanggah Banding Sanksi SP & Risalah Putusan Sidang Direksi
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun dialog modal mekanisme permohonan dan persidangan sanggah banding sanksi disiplin (`SanctionAppealModal.jsx`):
1. Penegakan Batas SLA Banding (VR-EMP-13): Batas waktu permohonan sanggah banding ditetapkan maksimal 7 (tujuh) hari kerja kalender sejak tanggal penerbitan sanksi SP. Jika melewati batas waktu, sistem memunculkan peringatan kadaluwarsa banding.
2. Mode Permohonan Pegawai: Formulir pengisian argumen sanggahan fakta peristiwa, pernyataan saksi pendukung, dan lampiran berkas dokumen bukti klarifikasi.
3. Mode Majelis Sidang Direksi / Komite Etik: Formulir persidangan resmi untuk menetapkan amar putusan:
   - Ditolak (Sanksi SP Tetap Berlaku Penuh).
   - Diterima Sebagian (Penurunan Tingkat Sanksi, misal dari SP 2 ke SP 1).
   - Diterima Penuh (Sanksi SP Dibatalkan / Anulir Total).
4. Terhubung langsung dengan antarmuka sanksi (`relations-sanctions-view.jsx`) dan API backend `submitSanctionAppeal` serta `decideSanctionAppeal`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/modal/SanctionAppealModal.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-sanctions-view.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Batas 7 hari kerja terhitung akurat dari `issuedDate`; form sanggahan tervalidasi; mode reviewer Direksi berhasil mencatat amar putusan dan memperbarui status sanksi di Redux store.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
