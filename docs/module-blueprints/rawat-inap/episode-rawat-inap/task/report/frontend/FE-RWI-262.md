# Laporan Perubahan Frontend — `FE-RWI-262`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-262` |
| Judul | Penyesuaian Formulir & Lembar Cetak General Consent dengan Judul Dinamis ODC dan Klausul Edukasi 24 Jam |
| Modul / Sub-modul | `rawat-inap` / `episode-rawat-inap` |
| Kontrak Target | `0.15.0` (`APPROVED_DESIGN_PLANNED`) |
| Delivery Lock | `ODC-DEL-20261017-01` |
| Tanggal | 17 Oktober 2026 |
| Status | **Selesai (Source Code Updated)** |

## Hasil Implementasi

1. **Konstanta Salinan Teks ODC (`INPATIENT_ODC_CONSENT_COPY`)**:
   - Menambahkan konstanta `INPATIENT_ODC_CONSENT_COPY` pada `inpatient-admission-flow-constants.jsx` dengan judul resmi: *"SURAT PERSETUJUAN PASIEN ONE DAY CARE (ODC)"*, subjudul: *"General Consent — Pelayanan Rawat Sehari (Maksimal 24 Jam)"*, serta teks baku klausul edukasi batas 24 jam dan eskalasi alih rawat inap reguler.

2. **Formulir Persetujuan Langkah 7 / 9 (`InpatientAdmissionConsentFormStep`)**:
   - Judul formulir pada Langkah Persetujuan berubah dinamis menjadi *"Formulir Persetujuan Umum Pasien Rawat Inap (One Day Care - ODC)"* saat pasien bertipe ODC.
   - Menambahkan banner peringatan edukasi (`InformationAlert` varian info) mengenai batas maksimal perawatan 24 jam serta eskalasi penyesuaian biaya rawat inap jika kondisi medis pasien memerlukan rawat inap lanjutan.
   - Menyimpan `patientCategory = "OneDayCare"` pada payload `saveAdmissionConsent` ke server.

3. **Lembar Cetak Persetujuan (`InpatientConsentForm` & `inpatient-consent-utils.jsx`)**:
   - Fungsi pembangun dokumen cetak `buildInpatientConsentDocument` mendeteksi penanda ODC (`patientType === "odc"`, `patientType === 7`, `isOdcBed`, atau kelas/ruangan ODC) dan menandai `isOdc: true`.
   - Komponen cetak `InpatientConsentForm` mencetak judul resmi *"SURAT PERSETUJUAN PASIEN ONE DAY CARE (ODC)"* dan subjudul *"General Consent — Pelayanan Rawat Sehari (Maksimal 24 Jam)"*.
   - Menyajikan panel klausul edukasi batasan observasi 24 jam dan eskalasi kelas perawatan sebelum 12 butir persetujuan umum.

4. **Langkah Konfirmasi Admisi (`InpatientAdmissionConfirmationStep`)**:
   - Kartu Perawatan (`ConfirmationCareSummary`) menyajikan lencana status pill berwarna amber/warning *"One Day Care (ODC)"* pada bagian header.
   - Menampilkan baris rincian *"Tipe Pasien: One Day Care (ODC) — Maks. 24 Jam"* serta menampilkan nama DPJP Utama penanggung jawab secara jelas.

## Bukti dan Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Judul Formulir Persetujuan | Dinamis menampilkan judul ODC untuk tipe pasien ODC |
| Klausul Edukasi 24 Jam | Tampil pada formulir persetujuan dan dokumen cetak persetujuan |
| Payload Persetujuan | Kategori pasien tersimpan sebagai `OneDayCare` |
| Ringkasan Langkah Konfirmasi | Status badge "One Day Care (ODC)" dan DPJP Utama tampil pada kartu ringkasan perawatan |
| Kontrak & Linting | Bebas syntax error; mematuhi konvensi UI komponen Quilvian |
| Production Build | Build frontend mandiri oleh user sesuai kesepakatan |

## Batas Perubahan

- Berkas yang diubah:
  1. `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
  2. `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-consent-utils.jsx`
  3. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-consent-form.jsx`
  4. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx`
  5. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx`
  6. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
