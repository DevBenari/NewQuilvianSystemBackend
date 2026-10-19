# Laporan Perubahan Frontend — `FE-RWI-260`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-260` |
| Judul | Penambahan Tipe Pasien One Day Care (ODC) & Penegakan Validasi DPJP Utama |
| Modul / Sub-modul | `rawat-inap` / `episode-rawat-inap` |
| Kontrak Target | `0.15.0` (`APPROVED_DESIGN_PLANNED`) |
| Delivery Lock | `ODC-DEL-20261017-01` |
| Tanggal | 17 Oktober 2026 |
| Status | **Selesai (Source Code Updated)** |

## Hasil Implementasi

1. **Konstanta Tipe Pasien ODC (`INPATIENT_PATIENT_TYPE` & `INPATIENT_PATIENT_TYPE_OPTIONS`)**:
   - Menambahkan enum tipe pasien `ODC: "odc"` pada `INPATIENT_PATIENT_TYPE`.
   - Menambahkan opsi kartu pemilihan pasien pada `INPATIENT_PATIENT_TYPE_OPTIONS` dengan judul *"One Day Care (ODC)"*, deskripsi *"Pelayanan observasi dan perawatan medis terencana paling lama 24 jam"*, ikon `"O"`, dan lencana visual badge `"RAWAT SEHARI"`.
   - Otomatis terintegrasi pada langkah 2 alur admisi Pasien Baru (`PatientTypeStep` di `inpatient-admission-view.jsx`) dan langkah 2 Pasien Lama (`InpatientAdmissionVisitCategoryStep` di `inpatient-admission-visit-category-step.jsx`).

2. **Validasi DPJP Wajib Pasien ODC**:
   - Memperbarui fungsi validasi `validateInpatientDoctorStep` di `inpatient-admission-encounter-utils.jsx` untuk menerima parameter `patientType`.
   - Menegakkan pesan penolakan eksplisit: *"DPJP Utama wajib dipilih. Pasien One Day Care (ODC) wajib memiliki dokter penanggung jawab pelayanan."* apabila `doctorId` kosong pada pasien ODC.
   - Mengalirkan `patientType: flow.patientType` dari wizard controller `inpatient-admission-view.jsx` ke `useInpatientAdmissionDoctor` hook.

## Bukti dan Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Perenderan Pilihan Kartu ODC | Terverifikasi otomatis membaca dari `INPATIENT_PATIENT_TYPE_OPTIONS` dengan lencana `"RAWAT SEHARI"` |
| Validasi DPJP | Terverifikasi `validateInpatientDoctorStep` memblokir tombol Lanjut dan menampilkan pesan khusus ODC bila DPJP kosong |
| Kontrak & Linting | Bebas syntax error; mematuhi konvensi camelCase frontend Quilvian |
| Production Build | Build frontend mandiri oleh user sesuai kesepakatan |

## Batas Perubahan

- Berkas yang diubah:
  1. `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
  2. `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx`
  3. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx`
  4. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
