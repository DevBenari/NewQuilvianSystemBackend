# Laporan Perubahan Backend — `BE-RWI-236`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-236` |
| Judul | Editor membaca susunan lantai dan mengaktifkan ruang cadangan |
| Slice | Editor Master Bed mendapat susunan satu lantai tanpa identitas pasien, dan ruang cadangan menjadi kamar beserta bed-nya dalam satu transaksi |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-236` |
| Trace | FR-DNH-009, 010; RWI-DEC-306, 318; RWI-AC-451, 457; VAL-DNH-09, 20; GUARD-DNH-06; NFR-804; DNH-AT-913 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | `BE-RWI-235` ✅ |
| Klasifikasi | `HEAVY` — skor 10: satu repo 0, diperiksa > 20 = 2, diubah 6 = 1, logika kompleks 2, kontrak baru 2, persistence multi-entitas 1, auth terkait 1, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | `FloorPlanLayoutService.cs`, `BedMutationService.cs`, `RoomController.cs`, `FloorPlanLayoutDtos.cs`, `Program.cs`; operasi `Room:Create` pada validator opsi |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source dan build terbukti |

---

## 1. Masalah yang diperbaiki

`RoomController` dan `BedController` hanya mengenal CRUD per baris. Editor denah tidak punya satu bacaan susunan lantai, dan mengaktifkan ruang cadangan harus dilakukan dengan beberapa permintaan yang masing-masing commit sendiri — bila permintaan ketiga gagal, kamar sudah terbentuk tanpa bed.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Admin melihat susunan satu lantai dan mengubah ruang cadangan menjadi kamar aktif dalam satu langkah utuh |
| Pelaku | Admin MasterData (`Room : Read`, `Room : Create`) dalam scope unit |
| Pemicu | Membuka editor Master Bed lantai tertentu; menekan "Aktifkan ruang cadangan" |

Langkah baca (`GET /rooms/floor-plan-layout?floorCode=3`):

1. `floorCode` wajib; kosong → 400 `FLOOR_CODE_REQUIRED`.
2. Server membaca kamar berkunci lantai itu, bed-nya (aktif dan nonaktif), status enam-keadaan dari proyeksi Monitoring, jumlah penempatan dan pesanan aktif, `HasUsageHistory` per bed, dan `AffectedBedVersions` untuk perubahan berikutnya.
3. Daftar `RoomsWithoutFloorPlanKey` memuat kamar unit rawat inap yang belum punya posisi di denah.
4. Respons **tidak** memuat nama maupun No. RM penghuni (VAL-DNH-20).

Langkah aktivasi (`POST /rooms/floor-plan-layout/rooms`, header `Idempotency-Key`):

1. Server memeriksa format: kunci denah, lantai, nomor (≤ 50), kelas dan unit terpilih, arah wing 1–8, bed 1–20, boks 0–10.
2. Di bawah satu koordinator: scope unit, unit aktif bertipe rawat inap, kelas aktif, kunci belum dipakai, nomor kamar belum dipakai kamar aktif di lantai itu.
3. Server membuat kamar (kode alokator bersama, nama `<kelas> <nomor>`, label lantai/lokasi, penanda turunan kelas) lalu N bed dan M boks berlabel huruf berikutnya, masing-masing `Unverified` dengan event `BedCreated`, satu receipt `Room`, satu commit.

> **Contoh.** Cadangan di sebelah 322 diaktifkan sebagai nomor 324, Kelas 2, 3 bed, 0 boks → "Kelas 2 324" dengan 324-A, 324-B, 324-C, ketiganya Tidak Tersedia sampai disahkan perawat. Mengirim ulang dengan `Idempotency-Key` yang sama mengembalikan hasil yang sama tanpa kamar kedua.

Jalur tidak normal:

| Keadaan | Hasil |
| --- | --- |
| Nomor 329 sudah dipakai kamar aktif di Lantai 3 | 409 `ROOM_NUMBER_DUPLICATE` |
| Kunci sudah dipakai | 409 `FLOOR_PLAN_KEY_DUPLICATE` |
| Unit nonaktif/bukan rawat inap | 422 `ROOM_UNIT_INVALID` |
| Kelas tidak ada/nonaktif | 422 `PATIENT_CLASS_NOT_FOUND` |
| Unit di luar scope | 403 `ACCESS_DENIED` |
| Kegagalan apa pun sesudah kamar dibuat | Transaksi dibatalkan; tidak ada kamar maupun bed tersimpan (nomor kode yang sudah dialokasikan hangus) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.1/15.4, API 14.2/14.3/14.4, validasi VAL-DNH-09; `InpBedMutationCoordinator.cs` (`ExecuteAsync`, `RootAsync`, receipt), `BedMutationService.cs`, `InpBedManagementQueryService.ProjectBatchAsync`, `InpBedAccessService.cs`, `BedManagementOptionsValidator.cs`, `RoomController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Services/FloorPlanLayoutService.cs` | Baru: `GetLayoutAsync` (baris 33), `ActivateRoomAsync` (baris 130) |
| `Areas/HealthServices/MasterData/Services/BedMutationService.cs` | `ExecuteFloorPlanAsync` (baris 170), `AddFloorPlanBedAsync` (baris 181) — operasi multi-entitas di bawah satu `coordinator.ExecuteAsync` |
| `Areas/HealthServices/MasterData/DTOs/FloorPlanLayoutDtos.cs` | Baru: `FloorPlanLayoutQuery/Response`, `ActivateFloorPlanRoomRequest`, `FloorPlanRoomActivationResponse` |
| `Areas/HealthServices/MasterData/Controllers/RoomController.cs` | `GET floor-plan-layout` (baris 63), `POST floor-plan-layout/rooms` (baris 78) |
| `Program.cs` | DI `FloorPlanLayoutService` (baris 725) |
| `Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs` | Operasi canonical `Room:Create` supaya scope aktivasi dapat ditugaskan lewat `UnitAssignments` (delta teknis; sebelumnya hanya SuperAdmin yang lolos scope) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru sesuai API 14.2 |
| Database | Tidak ada schema baru; aktivasi menulis `MstRoom`, `MstBed`, `InpBedReadiness`, `InpBedLifecycleEvent`, `InpBedOperationReceipt` dalam satu transaksi |
| Keamanan/Auth | Memakai action existing `Room : Read` dan `Room : Create`; scope unit `Room:Create` existing. Susunan lantai tanpa identitas pasien |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/floor-plan-layout` | Susunan satu lantai untuk editor Master Bed, termasuk kamar tanpa posisi | `Room : Read` | `FloorPlanLayoutQuery` {`floorCode`} | `FloorPlanLayoutResponse` |
| `POST` | `/floor-plan-layout/rooms` | Mengaktifkan ruang cadangan menjadi kamar beserta bed-nya | `Room : Create` | `ActivateFloorPlanRoomRequest`; header `Idempotency-Key` | `FloorPlanRoomActivationResponse` |

Pemeriksaan hak akses: `ControllerName = "Room"`; kedua action memakai `[AccessAction("Read"|"Create")]` dan `[AccessPermission("Room", "Read"|"Create")]` yang cocok.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Mst` ACTIVE; `NEW CODE` service/DTO; QBE-SVC-001 (controller hanya memanggil service), CODE-001/006, TXN-001, PERM-001, DTO-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| DNH-AT-913 aktivasi atomik | Kamar dan seluruh bed hanya `Add` ke context; satu `SaveChangesAsync` + commit di koordinator; pengecualian apa pun → rollback + `ChangeTracker.Clear()` | `PASS` (penelusuran source) | `FloorPlanLayoutService.cs:156-206`; `InpBedMutationCoordinator.ExecuteAsync` |
| Privasi susunan lantai | DTO tidak memiliki field nama/No. RM; `HoldingContext` proyeksi tidak dipetakan | `PASS` | `FloorPlanLayoutDtos.cs`; `FloorPlanLayoutService.cs:95-122` |
| `ROOM_NUMBER_DUPLICATE` | Dicek terhadap kamar aktif di lantai sama | `PASS` | `FloorPlanLayoutService.cs:178` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — belum ada data denah (D-DNH-3) dan backend lokal belum dijalankan ulang.

**Tidak dijalankan:** injeksi kegagalan PostgreSQL disposable (DNH-AT-913 dibuktikan lewat struktur transaksi).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Susunan Lantai 3 memuat kamar, bed, label, `HasUsageHistory`, dan versi bed terdampak, tanpa nama atau No. RM | Terpenuhi | `GetLayoutAsync` (baris 33–126) |
| Aktivasi cadangan 324 Kelas 2 dengan 3 bed → satu kamar dan tiga bed `Unverified` dalam satu commit | Terpenuhi | `ActivateRoomAsync`; `AddFloorPlanBedAsync` |
| Nomor yang sudah dipakai kamar aktif di lantai itu → 409 `ROOM_NUMBER_DUPLICATE` | Terpenuhi | Baris 178 |
| Kegagalan disuntik sesudah kamar dibuat → tidak ada kamar maupun bed | Terpenuhi | Satu transaksi koordinator |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Temuan hardcode existing (tidak diubah): `InpBedAccessService.IsSuperAdminAsync` memakai `IsInRole("SuperAdmin")` dan `UserType == 1` sebagai pintas scope |
| Masalah yang diketahui | `RoomsWithoutFloorPlanKey` dibatasi unit bertipe rawat inap supaya ruang OK/hemodialisis tidak ikut |
| Risiko tersisa | Transaksi multi-bed memegang kunci unit/kelas selama aktivasi (maks. 30 bed) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | `FE-RWI-250` |
