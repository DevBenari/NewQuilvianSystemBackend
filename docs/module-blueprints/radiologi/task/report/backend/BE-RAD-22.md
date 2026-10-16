# Laporan Perubahan Backend — `BE-RAD-22`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-22` |
| Judul | Unggah Gambar radiologi (Form Hasil v1) |
| Trace | `RAD-DEC-021` (ikut v1), `RAD-EVD-001` bagian 7.1 |
| Contract version | `RAD-API-001` revision 16; `RAD-PERM-001` — tanpa string izin baru |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a6920`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** 60/60 uji Radiologi. Migration `AddRadOrderImage` belum diterapkan ke devYoga |

---

## 1. Alur v1 dan penerapannya

v1: petugas mengunggah satu atau lebih gambar pada Form Hasil (`photoLabFiles`), disimpan di server
(`/uploads/HasilLabPhoto`) dan tampil pada hasil. V2 mengikuti pola `ReferralDocumentStorageService`:

| Bagian | Isi |
| --- | --- |
| Tabel | `RadOrderImage` — metadata saja (nama asli, jenis, ukuran, jalur relatif, urutan, study opsional); FK `RadOrder`, `RadStudy` `Restrict` |
| Penyimpanan | `FileStorage:PrivateRootPath/radiology-images/{orderId}/{id}.jpg|png` — **privat**; ditolak bila root kosong atau berada di dalam folder publik |
| Jenis | Dibaca dari isi (JPEG `FF D8 FF`, PNG `89 50 4E 47 0D 0A 1A 0A`), bukan nama |
| Batas | `FileStorage:MaxRadiologyImageSizeMb` (bawaan 10 MB) per berkas, 20 gambar aktif per pesanan |
| Endpoint | `GET/POST /rad-orders/{id}/images`, `GET …/images/{imageId}/content` (`no-store`), `DELETE …/images/{imageId}` (hapus lunak) |
| Jejak | `Order.ImageAdded` / `Order.ImageRemoved` di `RadTransitionHistory` + log |
| Keamanan | Berkas fisik yang sudah ditulis dihapus lagi bila metadata gagal tersimpan; gambar hanya terbaca lewat pesanan pemiliknya |

Bukan pengganti PACS — kolom Foto Radiologi (PACS) tetap bertanda belum tersedia.

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet build` (tanpa analyzer) + `migrations add` | Lulus; migration hanya membuat `RadOrderImage` + 2 index |
| `dotnet test` RadiologyTests | **60/60 lulus** — 7 baru di `RadOrderImageTests.cs` (JPG+PNG tersimpan dan tercatat; bukan-gambar ditolak tanpa berkas yatim; pesanan batal 409; terlalu besar ditolak; isi kembali utuh lalu hapus lunak; gambar pesanan lain tidak terbaca; root privat di folder publik ditolak) |

## 3. Batas verifikasi

Migration `20261015043017_AddRadOrderImage` belum diterapkan ke devYoga — menunggu instruksi pemilik modul.
