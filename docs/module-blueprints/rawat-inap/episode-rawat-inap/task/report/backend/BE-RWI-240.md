# Laporan Perubahan Backend — `BE-RWI-240`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-240` |
| Judul | Aturan dan kolom jenis kelamin bed dihapus |
| Slice | Kamar campur diterima tanpa peringatan; tidak ada kolom, field, aturan, atau pesan jenis kelamin pada bed |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-240` |
| Trace | FR-DNH-006; RWI-DEC-315; RWI-AC-439, 452; VAL-DNH-12; GUARD-DNH-07; INV-DNH-04; NFR-805; `DNH-MIS-08`; DNH-AT-439, 452, 912 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`); perubahan **breaking** API 14.1 |
| Dependency | `BE-RWI-237` ✅, `BE-RWI-239` ✅ |
| Klasifikasi | `HEAVY` — skor 11: satu repo 0, diperiksa > 20 = 2, diubah > 8 = 2, logika sedang 1, kontrak breaking 2, schema/migration 2, auth 0, workflow luas 2 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | 13 berkas hasil `git grep IsForMale\|IsForFemale\|BED_GENDER_MISMATCH\|PATIENT_GENDER_UNKNOWN` di HEAD (di luar migration lama), `Migrations/*` baru |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source, migration M-DNH-4, build, dan penerapan ke `QuilvianNewDevHamzah` terbukti. Rilis ke lingkungan bersama **wajib** bersama `FE-RWI-253` (`DNH-MIS-08`) |

---

## 1. Masalah yang diperbaiki

Kamar dan bed menyimpan penanda `IsForMale`/`IsForFemale`, dan kelayakan bed menolak lewat aturan 4 `BED_GENDER_MISMATCH` dan aturan 5 `PATIENT_GENDER_UNKNOWN`. RWI-DEC-315 memutuskan kamar campur diterima: tidak ada lagi aturan, kolom, maupun tampilan jenis kelamin pada bed.

> **Contoh sebelum.** Tn. Budi di 329-A (bed bertanda laki-laki saja). Ny. Sari tidak dapat dipesankan 329-B karena bed itu juga bertanda laki-laki → 422 `BED_GENDER_MISMATCH`.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Penempatan pasien tidak pernah ditolak maupun diperingatkan karena jenis kelamin |
| Pelaku | Petugas admisi, perawat, admin MasterData |
| Pemicu | Pesan bed, tempatkan pasien, transfer, `available-beds`, denah, form master kamar/bed |

Langkah dan aturan:

1. Aturan kelayakan 4 dan 5 beserta `BedGenderMessage`/`NormalizeGender` dihapus. Nomor 4, 5 (dan 6 yang sudah dipensiunkan) dibiarkan kosong supaya nomor aturan 7 dan 8 tidak bergeser bagi pemanggil.
2. Field `IsForMale`/`IsForFemale` dihapus dari `MstRoom`, `MstBed`, konfigurasi EF, DTO kamar/bed/`AvailableBedResponse`, ringkasan (`MaleRoom`, `FemaleRoom`, `MaleBed`, `FemaleBed`), penyaring/sort/metadata form controller, writer, dan daftar perubahan berbahaya `BedMutationService`.
3. Validasi lama "kamar/bed minimal untuk laki-laki, perempuan, atau bayi" dihapus karena maknanya dicabut.
4. Klien lama yang masih mengirim field jenis kelamin **tidak ditolak**: System.Text.Json mengabaikan properti yang tidak dikenal; parameter query lama juga diabaikan.
5. Migration M-DNH-4 dibuat paling akhir, sesudah M-DNH-1..3.

| Contoh sesudah | Hasil |
| --- | --- |
| Ny. Sari dipesankan lalu ditempatkan di 329-B saat Tn. Budi di 329-A | Berhasil, tanpa peringatan |
| Respons `GET /rooms`, `GET /beds`, `GET /available-beds` | Tanpa `isForMale`/`isForFemale` |
| Klien lama mengirim `"isForMale": true` saat `PUT /beds/{id}` | Field diabaikan, simpan berhasil |

Jalur tidak normal: tidak ada kode penolakan baru; kedua kode lama tidak pernah dikembalikan lagi. Data jenis kelamin pasien (`MstPatient.Gender`) tidak disentuh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.2/15.8, kamus data 22.2.1/22.2.2/22.4, API 14.1/14.4; hasil `git grep` di HEAD (13 berkas); `InpBedOccupancyService.EvaluatePlacementEligibilityCore`; `Program.cs` (opsi JSON).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstRoom.cs`, `MstBed.cs` | Dua properti dihapus |
| `Repositories/Configurations/HealthServices/MstRoomConfiguration.cs`, `MstBedConfiguration.cs` | Konfigurasi bawaan dua kolom dihapus |
| `Areas/HealthServices/MasterData/DTOs/RoomDtos.cs`, `BedDtos.cs` | Field dan hitungan jenis kelamin dihapus dari request, response, opsi, ringkasan, filter default |
| `Areas/HealthServices/MasterData/Controllers/RoomController.cs`, `BedController.cs` | Sort, ringkasan, parameter query, penyaring, metadata form, pemetaan, validasi |
| `Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs`, `BedMasterWriteService.cs` | Penetapan field dan validasi lama dihapus |
| `Areas/HealthServices/MasterData/Services/BedMutationService.cs` | Dua field keluar dari daftar perubahan berbahaya (baris 278) |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Aturan 4/5, `BedGenderMessage`, `NormalizeGender`, pemetaan `AvailableBedResponse` dihapus; komentar aturan diperbarui (baris 1591, 1716) |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs`, `Controllers/InpatientBedOccupancyController.cs` | Field `AvailableBedResponse` dihapus; dokumentasi diselaraskan |
| `Migrations/20261016043649_DropGenderFlagsFromRoomAndBed.cs` (+ Designer, snapshot) | M-DNH-4 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Breaking** (API 14.1): field jenis kelamin hilang dari request/response kamar, bed, dan `available-beds`; `MaleRoom`/`FemaleRoom`/`MaleBed`/`FemaleBed` hilang dari ringkasan; kode `BED_GENDER_MISMATCH`/`PATIENT_GENDER_UNKNOWN` dipensiunkan |
| Database | M-DNH-4: drop `IsForMale`/`IsForFemale` pada `MstRoom` dan `MstBed`. **Diterapkan** ke `QuilvianNewDevHamzah` 16 Oktober 2026. Mundur menambah kembali kolom bawaan `true`; isi lama tidak dipulihkan |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/available-beds` | Pencarian bed tersedia, tanpa field jenis kelamin | `InpatientBedOccupancy : Read` | `AvailableBedQuery` | `AvailableBedPagedResult` |
| `POST` | `/reservations` | Pesan bed; aturan jenis kelamin tidak diperiksa | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `BedReservationResponse` |
| `POST` | `/placements` | Tempatkan pasien; aturan jenis kelamin tidak diperiksa | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `BedPlacementResponse` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/`, `/{id}`, `/options`, `/summary` | Data kamar tanpa penanda jenis kelamin | `Room : Read` | Query existing tanpa `isForMale`/`isForFemale` | DTO kamar |
| `POST` / `PUT` | `/`, `/{id}` | Simpan kamar tanpa penanda jenis kelamin | `Room : Create` / `Room : Update` | `CreateRoomRequest` / `UpdateRoomRequest` | DTO kamar |

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/`, `/{id}`, `/options`, `/summary` | Data bed tanpa penanda jenis kelamin | `Bed : Read` | Query existing tanpa `isForMale`/`isForFemale` | DTO bed |
| `POST` / `PUT` | `/`, `/{id}` | Simpan bed tanpa penanda jenis kelamin | `Bed : Create` / `Bed : Update` | `CreateBedRequest` / `UpdateBedRequest` | DTO bed |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Mst`/`Inp` ACTIVE; `TOUCHED LEGACY`; QBE-API-001 (breaking disetujui kontrak 0.13.0), DTO-001 | `PASS` | Bagian 3.2 |
| DNH-AT-439 / pencarian literal | `grep -rnE "IsForMale\|IsForFemale\|BED_GENDER_MISMATCH\|PATIENT_GENDER_UNKNOWN" --include=*.cs` di luar `Migrations/` → **0 hasil** | `PASS` | Keluaran perintah, 16 Oktober 2026 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| `dotnet ef migrations add DropGenderFlagsFromRoomAndBed` | Empat `DropColumn`; `Down` menambah kembali bawaan `true`. Model pekerjaan paralel yang ikut tertangkap dikeluarkan dari migration, Designer, dan snapshot; delta snapshot terverifikasi hanya empat kolom ini | `PASS` | Berkas migration; diff snapshot |
| Penerapan ke `QuilvianNewDevHamzah` | `MstRoom` dan `MstBed` tidak lagi memiliki kolom jenis kelamin; empat migration denah tercatat; `migrations list` 0 pending | `PASS` | Kueri katalog lewat `dotnet fsi` + Npgsql |
| DNH-AT-452 kamar campur | Evaluator tidak lagi membaca `patient.Gender` | `PASS` | `InpBedOccupancyService.cs:1716` |
| DNH-AT-912 gelombang satu rilis | Backend selesai; frontend `FE-RWI-253` belum | `NOT RUN` | Menunggu `FE-RWI-253` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — backend lokal belum dijalankan ulang dengan build baru.

**Tidak dijalankan:** DNH-AT-912 (integrasi backend–frontend) karena frontend belum dikerjakan — ini skenario rilis bersama, bukan acceptance criteria task ini.

**Catatan penerapan database.** `dotnet ef database update --no-build` ditolak EF Core 9 (`PendingModelChangesWarning`) karena pekerjaan paralel menambah model `PatNewbornBirthSequence` dan tiga kolom `MstPatient` tanpa migration. Migration diagnostik sementara membuktikan selisih itu **hanya** milik pekerjaan paralel (dihapus sesudahnya; snapshot dipulihkan byte-per-byte). Empat migration denah lalu diterapkan dengan skrip SQL idempoten hasil `dotnet ef migrations script 20261014100000_FixClearanceReasonCodeReevaluatedByCashier 20261016043649_DropGenderFlagsFromRoomAndBed --idempotent`, dijalankan dalam satu transaksi lewat `dotnet fsi` + Npgsql setelah memastikan target `QuilvianNewDevHamzah`. Skrip yang sama mencatat `__EFMigrationsHistory`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Ny. Sari dipesankan dan ditempatkan di 329-B saat Tn. Budi di 329-A, tanpa peringatan maupun penolakan | Terpenuhi | Aturan 4/5 dihapus |
| Respons kamar, bed, dan `available-beds` tidak memuat field jenis kelamin | Terpenuhi | DTO tanpa field |
| `git grep` source (di luar migration lama) tidak menemukan keempat istilah | Terpenuhi | 0 hasil |
| Klien lama yang mengirim field jenis kelamin tidak ditolak; field diabaikan | Terpenuhi | Properti tak dikenal diabaikan System.Text.Json (tanpa `UnmappedMemberHandling.Disallow`) |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | DB Hamzah sudah tanpa kolom jenis kelamin. **Backend yang sedang berjalan (`dotnet run --no-build`, DLL lama) akan galat `42703 column … does not exist` pada kueri kamar/bed** sampai dihentikan, dibuild, dan dijalankan ulang. Frontend yang masih membaca `isForMale` menerima nilai kosong sampai `FE-RWI-253` |
| Masalah yang diketahui | `watchlogs-log.db` (berkas log biner terlacak) ikut cocok dengan `git grep` di HEAD; bukan source dan tidak disentuh |
| Risiko tersisa | Breaking change: rilis ke lingkungan bersama wajib satu gelombang dengan `FE-RWI-253` (`DNH-MIS-08`) |
| Perubahan sampingan | Pengeluaran model paralel dari M-DNH-4 (bagian 5); pengarsipan Designer migration (lihat `BE-RWI-232`) |
| Interupsi | Build pertama fase ini gagal CS1501 (argumen `null` jenis kelamin pada `GetBedOptions`); diperbaiki dan dibuild ulang |
| Status Git | Lihat tabel di bawah |
| Langkah berikutnya | Hentikan backend → build → jalankan ulang; kerjakan `FE-RWI-253` dalam gelombang yang sama; pemilik pekerjaan paralel membuat migration `PatNewbornBirthSequence`/`MstPatient` |

**Status Git akhir seluruh roadmap (16 Oktober 2026, `git status --short`, ringkas).**

| Kelompok | Berkas |
| --- | --- |
| Diubah roadmap ini (`M`) | `InpatientBedManagementController.cs`, `InpatientBedOccupancyController.cs`, `InpatientEpisodeController.cs`, `InpatientBedManagementDtos.cs`, `InpatientBedOccupancyDtos.cs`, `InpatientEpisodeDtos.cs`, `InpEpisode.cs`, `BedManagementOptions.cs`, `BedManagementOptionsValidator.cs`, `InpBedClassComparisonService.cs`, `InpBedManagementQueryService.cs`, `InpBedOccupancyService.BedManagement.cs`, `BedController.cs`, `RoomController.cs`, `BedDtos.cs`, `RoomDtos.cs`, `MstBed.cs`, `MstRoom.cs`, `BedMasterWriteService.cs`, `BedMutationService.cs`, `PatientClassMasterWriteService.cs`, `RoomMasterWriteService.cs`, `Program.cs`, `InpEpisodeConfiguration.cs`, `MstBedConfiguration.cs`, `MstRoomConfiguration.cs`, `Migrations/ApplicationDbContextModelSnapshot.cs` |
| Berbagi dengan pekerjaan paralel (`M`) | `InpBedOccupancyService.cs`, `InpEpisodeService.cs`, `Repositories/ApplicationDbContext.cs` |
| Baru roadmap ini (`??`) | 16 berkas source (DTO, enum, model, service, konfigurasi denah dan HD/LD) + 8 berkas migration M-DNH-1..4 |
| Pengarsipan Designer (DEC-BUILD-002) | 26 `D Migrations/2026…Designer.cs` ↔ 26 `?? Migrations/History/…Designer.cs`; `M Migrations/MigrationMetadata.g.cs/.props` |
| Milik pekerjaan paralel, tidak disentuh | `InpatientAdmissionConsentDtos.cs`, `InpatientAdmissionWorkspaceDtos.cs`, `InpAdmissionWorkspaceQueryService.cs`, `InpatientAdmissionConsentService.cs`, `PatientController.cs`, `PatientDtos.cs`, `MstPatient.cs`, `MstPatientConfiguration.cs`, `PatientEncounterController.cs`, `ServiceUnitRegistrationRules.cs`, `PatNewbornBirthSequence.cs`, `PatNewbornBirthSequenceConfiguration.cs`, `NewbornPatientRegistrationService.cs`, `Storage/uploads/…` |
| Dokumen | 9 laporan `task/report/backend/BE-RWI-232..240.md`, roadmap dan traceability Integrasi Denah (baris status), serta dokumen blueprint lain yang sudah berubah sebelum sesi ini |

Tidak ada stage, commit, push, maupun deploy.
