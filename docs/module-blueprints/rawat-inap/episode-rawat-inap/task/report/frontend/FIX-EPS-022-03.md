# Laporan Perubahan Frontend — FIX-EPS-022-03

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Konteks tanpa duplikasi dan admisi ulang yang jelas |
| Keputusan owner | K-01 opsi B: admisi ulang hanya bila server menemukan episode selesai |
| Tanggal selesai source | 2026-10-16 |
| Status | SELESAI SOURCE |

## Hasil

Pilihan bebas `Datang dari luar RS / rujukan` dihapus. Langkah konteks membaca `visitType` dan data fasilitas rujukan yang telah diisi pada langkah awal, lalu menampilkannya sebagai informasi baca-saja. Adapter `resolveNewbornAdmissionSource` menetapkan `HospitalBirth`, `External`, atau `Readmission` secara deterministik.

Untuk pasien lama, hook meminta daftar episode pasien dengan status `Closed`. Pilihan **Admisi ulang setelah bayi pernah pulang** hanya muncul bila hasil server membuktikan sedikitnya satu episode tertutup. Helper menjelaskan bahwa sistem membuat episode baru dan tidak membuka episode lama.

## Berkas utama

- `inpatient-newborn-admission-steps.jsx`
- `use-inpatient-newborn-admission.js`
- `inpatient-newborn-admission-utils.js`
- `inpatient-newborn-admission-constants.js`
- `inpatient-newborn-admission.test.mjs`

## Acceptance dan bukti

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tidak ada input rujukan kedua | Terpenuhi | Sumber diturunkan dari `flow.visitType` |
| Rujukan menghasilkan `External` | Terpenuhi | Unit test adapter |
| Pasien baru tidak dapat memilih readmission | Terpenuhi | Opsi hanya untuk entry `existing` dengan `hasClosedEpisode` |
| Episode lama tidak dibuka kembali | Terpenuhi | Helper UI dan pembuatan episode tetap memakai alur baru |

Kegagalan pembacaan riwayat menahan pilihan readmission dan menyediakan aksi **Periksa Ulang Riwayat**; UI tidak mengarang status episode.

