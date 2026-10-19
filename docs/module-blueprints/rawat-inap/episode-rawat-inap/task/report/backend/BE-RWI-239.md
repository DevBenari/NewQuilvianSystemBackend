# Laporan Perubahan Backend — `BE-RWI-239`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-239` |
| Judul | Pilih Bed admisi dan transfer membaca denah dengan kelayakan per pasien |
| Slice | Langkah Pilih Bed dan tujuan transfer mendapat denah satu lantai dengan kelayakan setiap bed untuk episode itu |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-239` |
| Trace | FR-DNH-005; RWI-DEC-299, 312, 316; RWI-AC-441, 449, 453, 454; VAL-DNH-17, 21, 22; GUARD-DNH-05; NFR-803; DNH-AT-449, 453, 454, 907 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | `BE-RWI-233` ✅, `BE-RWI-238` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: satu repo 0, diperiksa > 20 = 2, diubah 4 = 1, logika sedang 1, kontrak baru 2, query existing 1, auth terkait 1, workflow terbatas 0 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | `InpFloorPlanQueryService.cs`, `InpBedOccupancyService.cs` (saring lantai), `InpatientBedOccupancyDtos.cs`, `InpatientBedOccupancyController.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source dan build terbukti |

---

## 1. Masalah yang diperbaiki

Kelayakan per pasien hanya tersedia lewat `available-beds` berhalaman. Langkah Pilih Bed berbasis denah butuh seluruh bed satu lantai sekaligus, lengkap dengan alasan bed yang tidak layak dan outline kamar untuk pasien tertentu.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Petugas memilih bed yang benar-benar layak untuk pasien langsung dari denah |
| Pelaku | Petugas admisi (Pilih Bed) dan perawat/DPJP (tujuan transfer), hak `InpatientBedOccupancy : Read` |
| Pemicu | Layar admisi langkah Pilih Bed atau layar transfer membuka denah lantai |
| Prasyarat | `EpisodeId` dari konteks layar (tidak diketik) |

Langkah utama:

1. `purpose` wajib `Admission` atau `Transfer`; `episodeId` wajib → selain itu 400 `INVALID_INPUT`.
2. Hak membaca episode diperiksa (`RequireEpisodeAsync`, 403 bila di luar scope).
3. `Admission` hanya untuk episode `Draft`; `Transfer` hanya untuk episode `Admitted` yang menempati bed → selain itu 422 `FLOOR_PLAN_PURPOSE_MISMATCH`.
4. Konteks episode, pasien, dan kamar ibu (bayi rawat gabung) dimuat **sekali**; denah lantai dimuat dengan inti yang sama dengan Monitoring.
5. Untuk setiap bed, evaluator kelayakan existing (`EvaluatePlacementEligibilityCore`) dijalankan di memori: kesiapan/pegangan bed, boks bayi, isolasi. Konteks `Admission` = pemesanan (ikut memeriksa `IsReservable`); `Transfer` = perpindahan.
6. Bed yang tidak layak tetap dikirim beserta `Eligibility.Failures`. `IsSelectable` kamar = ada bed Tersedia yang layak.
7. `Transfer`: `ComparisonResult` dan `AllowedManualCategories` per bed (tiga kategori untuk kelas khusus).
8. `available-beds` menerima saring `floorCode`.

| Contoh | Hasil |
| --- | --- |
| Pasien butuh isolasi; Isolasi 319 punya bed Tersedia | Hanya 319 `IsSelectable`; bed kamar lain gagal `ISOLATION_REQUIRED` |
| Pasien biasa | Bed 319 gagal `ISOLATION_BED_RESERVED`; 319 tidak ber-outline |
| Bayi rawat gabung, ibu di 329-A | Boks 329-D layak; boks kamar lain gagal `NEWBORN_ROOMING_IN_MISMATCH` |
| Episode bayi, Ruang Bayi Perinatologi | Boks Tersedia layak |
| Transfer dari Kelas 3, bed ICU | `ComparisonResult` `Special`, `AllowedManualCategories` [DownGrade, UpGrade, SameGrade] |
| `purpose=Admission` untuk episode Admitted | 422 `FLOOR_PLAN_PURPOSE_MISMATCH` |
| `purpose=Transfer` tanpa penempatan aktif | 422 `FLOOR_PLAN_PURPOSE_MISMATCH` |

HD/LD tidak dibaca evaluator (INV-DNH-05): pasien High Dependency dapat dipilihkan bed layak mana pun. Tidak ada alasan jenis kelamin (aturan dicabut `BE-RWI-240`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.7, API 14.2/14.3/14.4, validasi VAL-DNH-21/22; `InpBedOccupancyService.cs` (`SearchAvailableBedsAsync`, `EvaluatePlacementEligibilityCore`), `InpNewbornAdmissionRules.cs`, `InpBedClassComparisonService.cs`, `InpatientBedOccupancyController.cs`, `InpBedAccessService.RequireEpisodeAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs` | `GetSelectionAsync` (baris 67), `FLOOR_PLAN_PURPOSE_MISMATCH` (baris 89, 95), kelayakan di memori (baris 291), kategori transfer (baris 302), outline (baris 233) |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Saring `FloorCode` pada `available-beds` (baris 176) |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs` | `AvailableBedQuery.FloorCode` |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` | `GET floor-plan` (baris 76) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru; saring `floorCode` aditif pada `available-beds` |
| Database | Tidak ada |
| Keamanan/Auth | Action existing `InpatientBedOccupancy : Read`; scope `available-beds` existing (`GUARD-DNH-05`); hak baca episode diperiksa |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/floor-plan` | Denah Pilih Bed admisi atau tujuan transfer dengan kelayakan per bed | `InpatientBedOccupancy : Read` | `FloorPlanSelectionQuery` {`floorCode?`, `purpose`, `episodeId`} | `FloorPlanResponse` |
| `GET` | `/available-beds` | Pencarian bed tersedia; kini dapat disaring per lantai | `InpatientBedOccupancy : Read` | `AvailableBedQuery` + `floorCode?` | `AvailableBedPagedResult` |

Kode status: 400 `purpose`/`episodeId` salah; 403 di luar scope; 404 episode tidak ada; 422 tujuan denah tidak sesuai status episode.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Inp` ACTIVE; `NEW CODE`; SVC-001, VAL-001, DTO-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| DNH-AT-449 / 454 | Aturan boks bayi existing (`InpNewbornAdmissionRules.ValidateBed`) dipakai lewat evaluator yang sama | `PASS` | `InpFloorPlanQueryService.cs:285-297` |
| DNH-AT-453 (Pilih Bed) | `IsSelectable` hanya bila ada bed Tersedia yang `IsEligible` | `PASS` | Baris 233–235 |
| DNH-AT-907 | Admission butuh `Draft`; Transfer butuh `Admitted` + penempatan aktif | `PASS` | Baris 79–97 |
| Satu evaluator | Evaluator sama dengan `available-beds` dan penolak `POST /placements` | `PASS` | `EvaluatePlacementEligibilityCore` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — belum ada kamar berkunci (D-DNH-3) dan backend lokal belum dijalankan ulang.

**Tidak dijalankan:** uji runtime Pilih Bed.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Pasien butuh isolasi: hanya Isolasi 319 `IsSelectable` bila bednya Tersedia; pasien biasa: 319 tidak ber-outline | Terpenuhi | Aturan 7/8 existing di evaluator |
| Bayi rawat gabung ibu di 329-A: boks 329-D layak; Ruang Bayi Perinatologi layak untuk episode bayi | Terpenuhi | Aturan boks bayi existing; kamar ibu dimuat sekali |
| Transfer dari Kelas 3 menampilkan ketiga kategori pada bed ICU | Terpenuhi | `CompareAsync` → `Special` → `AllowedCategories` |
| `Admission` untuk episode Admitted, atau `Transfer` tanpa penempatan aktif → 422 `FLOOR_PLAN_PURPOSE_MISMATCH` | Terpenuhi | Baris 89, 95 |
| Bed tidak layak tetap dikirim dengan alasan; tidak ada alasan jenis kelamin | Terpenuhi | Semua bed dievaluasi; aturan 4/5 dihapus `BE-RWI-240` |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `InpBedOccupancyService.cs` memuat perubahan paralel issue 022 yang belum di-commit (saring unit layak registrasi pada `available-beds`); denah Pilih Bed sengaja memakai scope `available-beds` HEAD (`ca09cae9`) dan tidak bergantung pada berkas paralel `ServiceUnitRegistrationRules.cs` yang belum terlacak |
| Masalah yang diketahui | Perlu diputuskan apakah denah Pilih Bed ikut menyaring unit `IsAvailableForRegistration` setelah pekerjaan issue 022 di-commit |
| Risiko tersisa | Evaluasi dilakukan untuk semua bed lantai (bukan hanya yang Tersedia) supaya setiap bed membawa alasan; biaya di memori, tanpa query tambahan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | `FE-RWI-248`, `FE-RWI-249` |
