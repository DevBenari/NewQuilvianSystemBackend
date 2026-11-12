# Laporan Task FE-HRD-227: Modal Draf Akta Ikatan Dinas & Rincian Tagihan Penalti Resign

## 1. Informasi Task
* **Task ID**: `FE-HRD-227`
* **Slice / MVP**: `S-C2 / MVP-50`
* **Nama Fitur**: Komponen Modal Dialog Draf Akta & Kalkulator Denda Resign Sepihak (TrainingBondContractModal)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun komponen dialog modal serbaguna `TrainingBondContractModal.jsx` dengan 2 mode operasional:
1. Mode Draf Akta Perjanjian: Pratinjau klausul legalitas ikatan dinas retensi kerja, identitas para pihak, program fellowship, nilai sponsor RS, dan tombol cetak draf akta PDF.
2. Mode Kalkulator Penalti Wanprestasi Resign Sepihak (VR-TRN-17, AT-HRD-TRN-19):
   - Input bulan masa kerja yang telah dijalani saat staf mengajukan resign.
   - Formula Matematis Deterministik: Biaya Pokok Prorata Sisa Bulan + Denda Kompensasi Wanprestasi 50%.
   - Rincian lembar tagihan finansial lengkap dengan tombol cetak invoice tagihan resmi.
   - Tombol pencatatan status wanprestasi 'BreachPenalized' ke sistem kepegawaian.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/learning/modal/TrainingBondContractModal.jsx`

## 4. Bukti Verifikasi
* Formula denda wanprestasi prorata sisa ikatan + denda 50% terbukti matematis akurat (VR-TRN-17).
* Cetak invoice tagihan dan pencatatan status wanprestasi tervalidasi sukses.
* Memenuhi kriteria penerimaan AT-HRD-TRN-19.
