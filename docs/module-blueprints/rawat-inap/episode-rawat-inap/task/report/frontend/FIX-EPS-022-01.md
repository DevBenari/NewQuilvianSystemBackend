# Laporan Perubahan Frontend — FIX-EPS-022-01

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Transisi atomik ke alur admisi bayi |
| Plan | PLAN-REPAIR-EPS-022 |
| Target | `QuilvianSystemFrontendDev`, branch `HamzahV2`, basis `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE; live browser dengan API nyata belum dijalankan |

## Hasil

Pilihan tipe pasien sekarang ditahan sebagai state lokal. Tombol **Lanjut** memanggil satu aksi yang menetapkan tipe pasien sekaligus menulis URL tujuan. Untuk bayi, tujuan dikunci ke `newborn-context`; pasien umum tetap menuju `registration`. Dengan demikian perubahan daftar langkah tidak lagi membuat fallback memilih slug bersama `payment`.

## Berkas

- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
- `tests/unit/inpatient-newborn-admission.test.mjs`

## Acceptance dan bukti

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Memilih kartu bayi tidak langsung berpindah | Terpenuhi | `PatientTypeStep` hanya mengubah `selectedPatientType` lokal |
| Lanjut menuju Langkah 1 bayi | Terpenuhi | `resolvePatientTypeContinuationSlug(...NEWBORN) === "newborn-context"` |
| Jalur umum tetap ke pendaftaran | Terpenuhi | Unit test menghasilkan `registration` |
| Tidak bergantung pada fallback slug bersama | Terpenuhi | Aksi `continuePatientType` menulis tujuan eksplisit |

Validasi: test scope admisi bayi lulus 11/11, targeted ESLint lulus, lint penuh lulus, dan production build frontend lulus.

