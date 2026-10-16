# Laporan Perubahan Backend — `BE-RAD-26`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-26` |
| Judul | Template Hasil Radiologi (Setup v1) |
| Trace | `RAD-DEC-022` (fitur bertanda), `RAD-DEC-003` (pengesahan dokter radiolog), `RAD-EVD-001` bagian 7.4 |
| Contract version | `RAD-API-001` revision 17; `RAD-PERM-001` — tanpa string izin baru |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a692`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai.** 71/71 uji Radiologi. Migration diterapkan 2026-10-15; `GET /templates` 200 terhadap devYoga. `RadReport : Validate` kini dipegang Kepala Instalasi Radiologi |

---

## 1. Alur v1 dan penerapannya

Setup → Template Hasil di v1 tayang masih "Coming Soon". V2:

| Bagian | Isi |
| --- | --- |
| Tabel | `RadReportTemplate` — kode (unik di antara yang belum dihapus, disimpan huruf besar), nama, alat dan pemeriksaan opsional (FK `Restrict`), Temuan, Kesan (wajib), Saran, aktif, urutan |
| Baca | `GET /rad-reports/templates` — `RadReport : Read`; saring `modalityId`/`procedureId` (template umum ikut), `onlyActive`, `search` |
| Kelola | `POST`/`PUT`/`DELETE /rad-reports/templates[/{id}]` — `RadReport : Validate`; hapus lunak; `404 RAD_TEMPLATE_NOT_FOUND` |

Template hanya isian awal draf bacaan. Draf yang memakai template tetap ditulis atas nama penulisnya dan wajib
disahkan dokter radiolog — `RAD-DEC-003` tidak berubah. Pengelolaan diletakkan pada `RadReport : Validate` karena isi
template adalah kalimat klinis milik dokter radiolog; tidak ada string izin baru.

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet test` RadiologyTests | **71/71 lulus** — 1 uji di `RadV1FeatureTests.cs` (buat, kode ganda ditolak, ubah, saringan alat menyertakan template umum, hapus lunak) |

## 3. Batas verifikasi

Migration `AddRadReportTemplateAndFilmPrint` belum diterapkan ke devYoga. Saat ini juga belum ada jabatan selain
superadmin yang memegang `RadReport : Validate` — template baru dapat dikelola setelah izin dokter radiolog diberikan.
