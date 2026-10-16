# Laporan Perubahan Backend — `BE-RAD-20`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-20` |
| Judul | Petugas Radiologi dan Dokter Pemeriksa per pesanan (+ WhatsApp dan tarif di baris daftar) |
| Slice | Paritas v1, Fase 4b — `RAD-EVD-001` bagian 7.2 |
| Trace | `RAD-DEC-021` (ikut v1), `RAD-DEC-003` dan `RAD-DEC-012` tetap |
| Contract version | `RAD-API-001` revision 15; `RAD-PERM-001` — tanpa string izin baru |
| Task mode | `BACKEND` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `49c6a6920`, branch `yoga` |
| Tanggal | 2026-10-15 |
| Status | **Selesai — batas verifikasi.** 53/53 uji Radiologi lulus. Migration belum diterapkan ke devYoga |

---

## 1. Yang dibuat

| Bagian | Isi |
| --- | --- |
| Model | `RadOrder.RadiographerUserId` (FK `AspNetUsers`), `ExaminingDoctorId` (FK `MstDoctor`), keduanya nullable + index |
| Migration | `AddRadOrderAssignment` — dua kolom, dua index, dua FK `Restrict` |
| `PUT /{id}/assignment` | `RadOrder : Update`. `400 RAD_ASSIGNEE_NOT_FOUND` (petugas/dokter tidak ada atau nonaktif), `409 RAD_ASSIGNMENT_NOT_CHANGEABLE` (pesanan batal/ditolak). Tanpa perubahan → tanpa jejak baru. Perubahan → `Order.Assign` di `RadTransitionHistory` + log |
| `GET /examiner-doctor-options`, `/radiographer-options` | `RadOrder : Update`, `PagedResult`, cari nama/kode. Milik Radiologi karena `doctors/options` SDM dijaga `KioskRead` dan menolak petugas penunjang (temuan Lab `FE-LAB-50` T2) |
| Baris daftar dan rincian | `RadiographerName`, `ExaminingDoctorName`, `PatientPhoneNumber` (`WhatsAppNumber` ?? `PhoneNumber`), `TariffAmount` (`MstTariff` berlaku; kelas pasien kunjungan didahulukan, lalu tarif tanpa kelas). `GET /{id}` kini memanggil `LengkapiKolomV1Async` juga |

Penunjukan Dokter Pemeriksa **tidak** memberi hak mengesahkan bacaan — pengesahan tetap `ActAsRadiologist`
(`RAD-DEC-003`). Daftar kerja tetap per alat (`RAD-DEC-012`).

## 2. Verifikasi

| Bukti | Hasil |
| --- | --- |
| `dotnet build` (tanpa analyzer) + `dotnet ef migrations add` | Lulus; snapshot +23 baris, hanya dua kolom ini |
| `dotnet test` RadiologyTests | **53/53 lulus** — 7 baru di `RadOrderAssignmentTests.cs` |

**Satu cacat ditemukan uji dan diperbaiki.** `TariffAmount` memakai `GetValueOrDefault` atas `Dictionary<Guid, decimal>`
sehingga tarif yang belum diatur terbaca **0**. Nota akan mencetak Rp0 tanpa ada yang menyadari. Kini `TryGetValue`
→ `null`, dan FE menulis "Belum diatur".

## 3. Batas verifikasi

Dua migration Radiologi tertunda di devYoga: `AddRadOrderRequiresPreparation` (`BE-RAD-18`) dan `AddRadOrderAssignment`.
Penerapan lewat skrip ditolak pengaman izin sesi ini (perubahan sumber daya bersama); menunggu izin/eksekusi pemilik.
