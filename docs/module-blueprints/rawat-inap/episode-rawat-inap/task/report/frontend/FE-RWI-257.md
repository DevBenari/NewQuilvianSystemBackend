# Laporan Perubahan Frontend — `FE-RWI-257`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Stepper tujuh langkah dan pemilihan ibu |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source** |

## Hasil

Alur bayi sekarang tepat tujuh langkah dan selalu dimulai dari **Pilih Ibu Persalinan**. Tahap
payment/deposit, pilihan luar RS/rujukan, dan admisi ulang dihilangkan dari cabang bayi. Langkah
pertama memakai endpoint kandidat kasus persalinan dan menampilkan kasus, lokasi, penjamin,
serta jumlah bayi tanpa kartu identitas besar berulang di setiap langkah.

## Bukti dan verifikasi

- `inpatient-newborn-admission-constants.js`: definisi tujuh langkah.
- `use-inpatient-newborn-admission.js`: fetch dan mapping candidate.
- `inpatient-newborn-admission-steps.jsx`: layar kandidat ibu.
- Targeted ESLint `--quiet`: lulus.
- Targeted unit: tiga skenario `FE-RWI-257` lulus.
- Next.js production build: lulus, 498 halaman.

Perilaku tombol pembatalan existing tidak diubah.
