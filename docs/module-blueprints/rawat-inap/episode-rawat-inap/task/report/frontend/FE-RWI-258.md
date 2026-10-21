# Laporan Perubahan Frontend — `FE-RWI-258`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Identitas bayi sistem dan urutan otomatis |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source** |

## Hasil

Form identitas hanya meminta tanggal lahir, jam lahir, jenis kelamin, dan informasi kelahiran
opsional yang relevan. Nama ditampilkan baca-saja sebagai `By. Ny. <ibu>`. Input tunggal/kembar
dan urutan manual dihapus. Penyimpanan memakai endpoint newborn khusus dengan
`Idempotency-Key` stabil per form dan tidak mengirim `birthOrder`.

## Bukti dan verifikasi

- `inpatient-newborn-admission-utils.js`: validasi serta payload minimal tanpa nama/urutan.
- `use-inpatient-admission-patient.jsx`: idempotency key stabil dan save khusus bayi.
- `inpatient-admission-patient.service.js`: `POST /patients/newborns`.
- `inpatient-newborn-admission-steps.jsx`: form identitas khusus bayi.
- Targeted unit `FE-RWI-258`: dua skenario lulus.
- Targeted ESLint dan production build: lulus.

Urutan final dan nomor RM dibaca dari respons server setelah penyimpanan.
