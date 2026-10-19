# Laporan Perubahan Backend — `BE-RAD-25`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-25` |
| Judul | Ubah Pemeriksaan dan Cetak Film (Detail Pasien v1) |
| Trace | `RAD-DEC-022` (fitur bertanda), `RAD-EVD-001` bagian 7.4 |
| Contract version | `RAD-API-001` revision 17; `RAD-PERM-001` — tanpa string izin baru |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a692`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai.** 71/71 uji Radiologi. Migration `20261015054457_AddRadReportTemplateAndFilmPrint` diterapkan ke devYoga 2026-10-15 atas instruksi pemilik modul (riwayat 321 → 322); `GET /rad-orders` dan `GET /{id}` memuat `printFilm` |

---

## 1. Alur v1 dan penerapannya

| Bagian v1 | V2 |
| --- | --- |
| Aksi baris *Ubah Pemeriksaan* | `PUT /rad-orders/{id}/procedure` — `RadOrderChangeProcedureRequest { ProcedureId, ModalityId?, Reason }` |
| Kolom *Cetak Film* (centang) | `PUT /rad-orders/{id}/film-print` — `RadOrderFilmPrintRequest { PrintFilm }`; kolom baru `RadOrder.PrintFilm` |

Pengaman V2 pada Ubah Pemeriksaan (v1 tidak membatasi):

- hanya status `Requested`, `Accepted`, `Scheduled`, dan belum ada study yang tidak batal — sesudah citra diambil,
  mengganti pemeriksaan akan memutus kaitan citra dengan permintaannya (`409 RAD_PROCEDURE_NOT_CHANGEABLE`);
- alasan wajib (≤ 1.000 karakter);
- pemeriksaan pengganti harus `IsRadiology` dan aktif;
- tercatat `Order.ChangeProcedure` (pemeriksaan lama → baru + alasan) di `RadTransitionHistory`.

Cetak Film tercatat `Order.FilmPrint` hanya bila nilainya berubah. Jumlah dan jenis film tetap dicatat sebagai bahan
terpakai `Film` di konsol pemeriksaan (`BE-RAD-22`). `PrintFilm` ikut tersaji di `GET /` dan `GET /{id}`.

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet build` (tanpa analyzer) + `migrations add AddRadReportTemplateAndFilmPrint` | Lulus; migration hanya menambah `RadOrder.PrintFilm` dan tabel `RadReportTemplate` (+ 4 index) |
| `dotnet test` RadiologyTests | **71/71 lulus** — 3 uji di `RadV1FeatureTests.cs` (ubah sebelum study tercatat; ditolak bila study aktif, status lanjut, tanpa alasan, atau bukan radiologi; Cetak Film tercatat sekali per perubahan) |

## 3. Batas verifikasi

Migration belum diterapkan ke devYoga — menunggu instruksi pemilik modul. **Urutan rilis:** backend versi ini
memetakan `RadOrder.PrintFilm`; menjalankannya terhadap basis data tanpa kolom itu menggagalkan setiap kueri
`RadOrder` (`42703`).
