# Laporan Task FE-HRD-178: Portal Mandiri Permohonan Layanan SDM Staf (ESS) & Katalog Ubin Interaktif (FE-HRD-182)

## 1. Informasi Task
* **Task ID**: `FE-HRD-178`
* **Slice / MVP**: `S-D4 / MVP-39`
* **Nama Fitur**: Portal Mandiri Permohonan Layanan SDM Staf (ESS) & Katalog Ubin Interaktif (Layar FE-HRD-182)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka portal layanan mandiri (ESS) bagi tenaga kesehatan dan staf rumah sakit untuk mengajukan permohonan layanan SDM secara digital tanpa tatap muka langsung. Menampilkan grid ubin katalog layanan interaktif (*Service Tiles*) dengan ikon representatif, deskripsi keperluan, dan badge estimasi jam SLA operasional (Surat Keterangan Bekerja, Surat Keterangan Penghasilan KPR, Perubahan Rekening Payroll, Legalisir SK, Surat Bebas Sanksi, serta Konseling Karir), serta tab riwayat pengajuan tiket saya lengkap dengan indikator penanganan analis.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/services/portal-service-requests-view.jsx`
* `QuilvianSystemFrontendDev/src/app/portal/services/requests/page.jsx`
* `QuilvianSystemFrontendDev/src/app/ess/services/requests/page.jsx`

## 4. Bukti Verifikasi
* Klik pada ubin layanan interaktif memicu pembukaan formulir permohonan terpandu.
* Tab riwayat tiket saya menyajikan status dan informasi analis penanggung jawab secara transparan.
