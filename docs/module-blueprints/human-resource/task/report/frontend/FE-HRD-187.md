# Laporan Task FE-HRD-187: Portal Pengajuan Kredensialing Nakes Mandiri (Self-Service 4-Langkah Wizard)

## 1. Informasi Task
* **Task ID**: `FE-HRD-187`
* **Slice / MVP**: `S-C1 / MVP-42`
* **Nama Fitur**: Portal Kredensialing & Rekredensialing Mandiri Wizard 4 Langkah (Layar FE-HRD-187)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Mengimplementasikan antarmuka wizard 4 langkah bagi dokter, dokter gigi, perawat, bidan, dan tenaga kesehatan lainnya untuk mengajukan kredensialing awal maupun rekredensialing berkala:
* **Langkah 1 (Identitas & Lisensi)**: Pengisian data pribadi, profesi, nomor STR KKI/KTKI, nomor SIP aktif, masa berlaku lisensi, dan unggah pindaian dokumen sah.
* **Langkah 2 (Portofolio Klinis & Riwayat Logbook)**: Pengisian jumlah kasus/tindakan dalam 1-3 tahun terakhir, sertifikat kompetensi keahlian/kolegium, dan riwayat pelatihan ber-SKP.
* **Langkah 3 (Checklist Buku Putih / Whitebook)**: Pemilihan mandiri level kewenangan yang diajukan (Level 3 Di Bawah Supervisi vs Level 4 Mandiri Penuh) per butir prosedur sesuai komite mandiri (Medis / Keperawatan / Nakes Lain).
* **Langkah 4 (Pernyataan Etik & Submit Permohonan)**: Konfirmasi integritas medikolegal, penandatanganan elektronik, dan submit berkas status `Submitted`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/credentialing/portal-credentialing-wizard-view.jsx`
* `QuilvianSystemFrontendDev/src/app/portal/credentialing/application/page.jsx`
* `QuilvianSystemFrontendDev/src/app/ess/credentialing/application/page.jsx`

## 4. Bukti Verifikasi
* Form tervalidasi `VR-CRD-01` & `VR-CRD-04`: butir checklist whitebook terisolasi sesuai komite nakes.
* Navigasi wizard multi-langkah berfungsi mulus dengan indikator progress bar visual.
* Audit linter CI/CD (`npm run lint:errors`): 0 error.
