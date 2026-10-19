# Laporan Perubahan Backend — `BE-RWI-244`

## Metadata

| Field | Nilai |
| --- | --- |
| Judul | Calon ibu berbasis kasus persalinan |
| Kontrak | `RWI-BAYI-022-2.0.0` |
| Tanggal | 16 Oktober 2026 |
| Status | **Selesai source**; verifikasi runtime menunggu penerapan migration |

## Hasil

Endpoint `GET /api/v1/health-services/inpatient-management/episodes/newborn-mother-candidates`
sekarang mengambil ibu dari kasus `Obstetric` berstatus `InProgress` atau `Completed`. Jenis
ruangan ibu tidak dipakai sebagai syarat. Respons memuat kasus, encounter dan sumber penjamin,
episode/lokasi aktif bila tersedia, jumlah bayi terdaftar, serta preview urutan berikutnya.

## Bukti perubahan

- `InpEpisodeService.NewbornCandidates.cs`: query, search, tanggal, pagination, lokasi, penjamin,
  jumlah bayi, dan preview urutan.
- `InpatientNewbornDtos.cs`: kontrak query/response berhalaman.
- `InpatientEpisodeController.cs`: route dan permission read existing.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| Build backend Release | Lulus, 0 error; warning baseline tetap ada |
| Sinkronisasi model EF | `No changes have been made to the model since the last migration.` |
| Runtime API | Belum dijalankan karena migration belum diterapkan |

Seluruh acceptance criteria source terpenuhi. Tidak ada commit, push, deploy, atau perubahan
database.
