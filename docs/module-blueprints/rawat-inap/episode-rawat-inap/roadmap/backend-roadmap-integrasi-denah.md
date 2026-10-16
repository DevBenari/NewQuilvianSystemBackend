# Roadmap Backend Integrasi Denah ↔ Bed Management

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | RWI-BP-001 / `episode-rawat-inap` (COMPOSITE; kepemilikan data pada [module map 10.1](../../02-module-map.md)) |
| planning_id / revision / tanggal | `DNH-DEL-20261016-01` / 1 / 2026-10-16 Asia/Jakarta |
| status / approval | Desain disetujui: manifest anak revision 12, kontrak `0.13.0` — Muhammad Hamzah, 2026-10-16 (`RWI-DEC-324`). Setiap task dieksekusi hanya lewat `build-module-backend` dengan task ID-nya |
| blueprint / kontrak | [Backend 15](../02-backend-architecture.md), [kamus data 22](../data/data-dictionary.md), [API 14](../contracts/api-contract.md), [hak akses 12](../contracts/permission-audit-matrix.md), [validasi 17](../contracts/validation-matrix.md), [transisi 12](../contracts/state-transition-matrix.md), [integrasi 12](../contracts/integration-contract.md), [PRD 27](../04-prd-to-mvp.md) |
| input revisions | Decision log 51; gate 1.13 `DNH-RCG-20261015-01`; module map 7; impact scan `BM-IMP-20261015-01` rev 1 |
| source SHA backend / frontend | `ca09cae909597442385ce4513be9c803549be10e` / `4d9bf608dbbaddc96a2b4acfc8723753b5dd5028` |
| hash lock | [integrasi-denah-delivery-lock.json](integrasi-denah-delivery-lock.json) |
| scope / ID | 9 task `BE-RWI-232` s.d. `240`. Ruang ID dipindai global 2026-10-16: terpakai sampai `BE-RWI-231`; berikutnya `BE-RWI-241` |
| roadmap pasangan | [Frontend](frontend-roadmap-integrasi-denah.md) · [Traceability](requirement-traceability-integrasi-denah.md) |

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
BE-RWI-232 ✅ ─┬─> BE-RWI-235 ✅ ─┬─> BE-RWI-236 ✅
               │                  │
               │                  └─> BE-RWI-237 ✅ ──────────────────┐
               │                                                      │
               └────────────────┬─> BE-RWI-238 ✅ ─┬─> BE-RWI-239 ✅ ─┴─> BE-RWI-240 ✅
                                │                  │
BE-RWI-234 ✅ ──────────────────┘                  │
                                                   │
BE-RWI-233 ✅ ─────────────────────────────────────┘
```

Garis selalu prasyarat → task. Persetujuan desain `RWI-DEC-324` berlaku untuk seluruh task, sehingga tidak digambar sebagai gerbang. `BE-RWI-240` memakai prasyarat `237` dan `239` supaya migration penghapusan kolom jenis kelamin (M-DNH-4) dibuat paling akhir, sesudah M-DNH-1 s.d. 3.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-232`, `BE-RWI-233`, `BE-RWI-234` — boleh paralel, kecuali dua task pembuat migration (lihat catatan) |
| 2 | `BE-RWI-232` | `BE-RWI-235` |
| 2 | `BE-RWI-232`, `BE-RWI-234` | `BE-RWI-238` |
| 3 | `BE-RWI-235` | `BE-RWI-236`, `BE-RWI-237` — boleh paralel |
| 3 | `BE-RWI-233`, `BE-RWI-238` | `BE-RWI-239` |
| 4 | `BE-RWI-237`, `BE-RWI-239` | `BE-RWI-240` — dirilis bersama `FE-RWI-253` |

Catatan migration: `BE-RWI-232` (M-DNH-1), `234` (M-DNH-2), `237` (M-DNH-3), dan `240` (M-DNH-4) membuat migration. Task pembuat migration tidak dikerjakan bersamaan; migration berikutnya dibuat terhadap model snapshot terbaru, supaya `ApplicationDbContextModelSnapshot.cs` tidak bentrok.

## Register Task

| Task ID / status | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-232` ✅ [laporan](../task/report/backend/BE-RWI-232.md) | Kamar menyimpan lantai, arah wing, dan kunci denah; kode kamar baru tidak bentrok | FR-DNH-008; RWI-DEC-306, 313, 314, 323; `DNH-DES-01` | 0.13.0 approved; lock DNH-DEL-20261016-01 | `DNH-CAP-01`, `02`, `03` | Satu perubahan master kamar + M-DNH-1 + aturan nama | — | RWI-AC-451, 461 (sebagian); VAL-DNH-01–04; DNH-AT-901 | V-BE + DNH-AT-451, 461, 901 | Skema MasterData; seluruh tim MasterData meninjau | D-BE |
| `BE-RWI-233` ✅ [laporan](../task/report/backend/BE-RWI-233.md) | Kelas khusus menerima kategori transfer apa pun | FR-DNH-007; RWI-DEC-312, 319 | 0.13.0 approved | `DNH-CAP-16`, `17` | Satu aturan perbandingan kelas | — | RWI-AC-450, 458; VAL-DNH-13 | V-BE + DNH-AT-450, 458 | Kelas khusus salah jenis membebaskan kategori; MasterData menjaga `PatientClassType` | D-BE |
| `BE-RWI-234` ✅ [laporan](../task/report/backend/BE-RWI-234.md) | HD/LD tercatat per episode beserta riwayat | FR-DNH-013, 014; RWI-DEC-302, 303 | 0.13.0 approved | `DNH-CAP-19` | Satu aggregate episode + M-DNH-2 + dua endpoint | — | RWI-AC-440, 441; VAL-DNH-14–17; DNH-AT-908 | V-BE + DNH-AT-440, 441, 908 | Kriteria klinis RWI-OQ-141; aktivasi produksi `DNH-G04` | D-BE |
| `BE-RWI-235` ✅ [laporan](../task/report/backend/BE-RWI-235.md) | Writer master mengikuti aturan denah | FR-DNH-009, 010, 016; RWI-DEC-304, 317, 318 | 0.13.0 approved | `DNH-CAP-23`, `24`, `26` | Writer bed, kamar, dan kelas existing | `BE-RWI-232` | RWI-AC-444, 454, 455, 456, 457; VAL-DNH-07, 08, 10; DNH-AT-902, 903, 909 | V-BE + DNH-AT-444, 454–457, 902, 903, 909 | Penanda turunan kelas hanya pada kamar kosong; MasterData | D-BE |
| `BE-RWI-236` ✅ [laporan](../task/report/backend/BE-RWI-236.md) | Editor membaca susunan lantai dan mengaktifkan ruang cadangan | FR-DNH-009, 010; RWI-DEC-306, 318 | 0.13.0 approved | `DNH-CAP-41` (sisi API) | Satu service baca + satu mutation multi-entitas | `BE-RWI-235` | RWI-AC-451, 457; VAL-DNH-09; DNH-AT-913 | V-BE + DNH-AT-913 | Transaksi multi-bed; owner InPatientManagement + MasterData | D-BE |
| `BE-RWI-237` ✅ [laporan](../task/report/backend/BE-RWI-237.md) | Susunan denah diimpor dengan pemeriksaan, catatan run, dan kesiapan per lingkungan | FR-DNH-011, 012; RWI-DEC-306, 321, 323 | 0.13.0 approved | `DNH-CAP-08`, `27` | Satu importer + M-DNH-3 + opsi startup | `BE-RWI-235` | RWI-AC-446, 459, 461; VAL-DNH-01, 05, 06, 18, 19, 23; DNH-AT-905, 906 | V-BE + DNH-AT-446, 459, 461, 905, 906 | Impor produksi `DNH-G03`; eksekusi impor butuh wewenang terpisah | D-BE |
| `BE-RWI-238` ✅ [laporan](../task/report/backend/BE-RWI-238.md) | Monitoring membaca denah satu lantai dengan status kamar, badge, dan HD/LD tersamar | FR-DNH-001, 002, 014; RWI-DEC-299, 303, 316, 318, 287 | 0.13.0 approved | `DNH-CAP-09`–`12`, `21` | Satu proyeksi baca + satu endpoint | `BE-RWI-232`, `BE-RWI-234` | RWI-AC-442, 443, 453 (Monitoring); VAL-DNH-11, 20; DNH-AT-904, 910 | V-BE + DNH-AT-442, 443, 453, 904, 910 | Privasi Housekeeping; owner InPatientManagement | D-BE |
| `BE-RWI-239` ✅ [laporan](../task/report/backend/BE-RWI-239.md) | Pilih Bed admisi dan transfer membaca denah dengan kelayakan per pasien | FR-DNH-005; RWI-DEC-299, 312, 316 | 0.13.0 approved | `DNH-CAP-13`, `14`, `18`, `37` (sisi API) | Satu endpoint baca + saring lantai `available-beds` | `BE-RWI-233`, `BE-RWI-238` | RWI-AC-449, 453, 454; VAL-DNH-21, 22; DNH-AT-907 | V-BE + DNH-AT-449, 453, 454, 907 | `InpBedOccupancyService.cs` sedang diubah pekerjaan paralel issue 022 | D-BE |
| `BE-RWI-240` ✅ [laporan](../task/report/backend/BE-RWI-240.md) | Aturan dan kolom jenis kelamin bed dihapus | FR-DNH-006; RWI-DEC-315 | 0.13.0 approved | `DNH-CAP-07`, `15` | Penghapusan lintas kamar/bed/kelayakan + M-DNH-4 | `BE-RWI-237`, `BE-RWI-239` | RWI-AC-439, 452; VAL-DNH-12; DNH-AT-912 | V-BE + DNH-AT-439, 452, 912 | Breaking; rilis bersama `FE-RWI-253` (`DNH-MIS-08`) | D-BE |

## Langkah operasional di luar task

Langkah berikut bukan pekerjaan source, sehingga tidak berupa task dan tidak digambar pada grafik. Bukti runtime beberapa task bergantung padanya.

| Langkah | Isi | Pemilik / wewenang | Dibutuhkan untuk | Status |
| --- | --- | --- | --- | --- |
| D-DNH-1 | Penyelarasan kelas denah ke DB existing (RWI-DEC-326): nama kelas master DB `MstPatientClass` **tidak diubah** (menjaga 18.513 tarif per kelas di `MstTariff`); kelas Premium **tidak dibuat**; kamar 801–810 dialihkan ke VVIP (`CLS-147`), 811 ke LUXURY, 812–813 ke SUITE. Denah memetakan langsung ke 12 kelas existing di DB. Perbaruan `ClassOrder` bila diperlukan (RWI-DEC-319, 326) | Seluruh tim MasterData + pemilik konfigurasi; gate `DNH-G01`, `BM-G01` | Bukti runtime `BE-RWI-233`, `235`, `237`; FR-DNH-015 | Terkunci per RWI-DEC-326 (mutasi nama kelas DB ditiadakan) |
| D-DNH-2 | Pembersihan data rawat inap DB Hamzah: cadangan, dry run per tabel, eksekusi sesudah konfirmasi (RWI-DEC-305) | Pemilik DB pribadi; konfirmasi eksekusi terpisah; izin baca `DNH-UNK-01`, `02` | D-DNH-3 | Belum |
| D-DNH-3 | Impor denah di DB Hamzah: pemetaan unit, dry run, impor, bed langsung siap (RWI-DEC-321) | Admin impor + pemilik DB | Bukti runtime `BE-RWI-237` s.d. `239` dan seluruh task frontend | Belum |
| D-DNH-4 | Produksi: susunan disahkan RS, impor sekali, bed disahkan perawat per lantai | RS, admin impor, perawat verifikator; gate `DNH-G03`, `DNH-G05` | Go-live, bukan development | Belum |

## Batas Handoff dan Definition of Done

### D-BE — bukti wajib setiap task backend

QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical (`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`): owner/prefix, applicability NEW CODE atau TOUCHED LEGACY, branch dari owner, wewenang tulis, dan hash masukan pada lock. Modul `InPatientManagement`/`Inp` dan `MasterData`/`Mst` terdaftar ACTIVE.

V-BE adalah bundel bukti menurut `rules/backend/TEST_POLICY.md`: review diff/scope, `dotnet build` project aplikasi (`dotnet restore` bila project/dependency berubah), verifikasi kontrak API (method, path, `[AccessAction]`/`[AccessPermission]`, request/response, kode status), dan verifikasi proses bisnis dengan contoh berangka untuk jalur normal dan tidak normal. DNH-AT adalah **identitas skenario requirement-ke-bukti**, bukan kewajiban automated test; tidak membuat project test. Bila pemilik memilih menjalankan build sendiri, tulis `NOT RUN` beserta keputusan pemilik, sesuai preseden 10 September 2026; jangan menulis `PASS` untuk perintah yang tidak dijalankan.

Migration dibuat lewat `dotnet ef migrations add` dan ditinjau SQL-nya. Penerapan ke database hanya pada `QuilvianNewDevHamzah` dengan izin eksekusi terpisah; database tim dilarang. Laporan tracked disimpan di `task/report/backend/<TASK-ID>.md`; sesudah bukti tersedia, perbarui kartu, register, grafik, gelombang, dan traceability bersama laporannya.

## Kartu Task

### ✅ BE-RWI-232 — Kamar menyimpan lantai, arah wing, dan kunci denah dengan kode sistem yang aman

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Ketujuh acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Migration M-DNH-1 `AddFloorPlanLocationToMstRoom` dibuat lewat `dotnet ef`, ditinjau (aditif), dan **diterapkan ke `QuilvianNewDevHamzah`**; katalog terverifikasi. Uji manual Swagger `NOT FEASIBLE` (backend lokal belum dijalankan ulang) — bukan butir D-BE. Bukti: [laporan](../task/report/backend/BE-RWI-232.md) |
| Slice / outcome | Kamar denah dapat dibuat dan dibaca per lantai; kode kamar baru tidak pernah bentrok dengan kamar yang dihapus lunak |
| Ukuran / batas | Satu perubahan master kamar: tiga kolom, satu enum, satu migration aditif, aturan nama, dan perbaikan pengalokasi kode |
| Jejak | FR-DNH-008; RWI-DEC-306, 313, 314, 323; `DNH-DES-01`; RWI-AC-451, 461; VAL-DNH-01–04; NFR-806 |
| Desain / kontrak | Backend 15.2–15.6, 15.8 (M-DNH-1); kamus data 22.2.1, 22.3, 22.4; API 14.2 Room (`GET /`, `POST /`, `PUT /{id}`), 14.3 DTO kamar, 14.4 |
| Reuse / disposisi | `DNH-CAP-01`, `02` Missing → baru; `DNH-CAP-03` Reuse with adapter; pengalokasi nomor bersama seperti `AllocateBedCodeAsync` |
| Cakupan | `MstRoom` + `FloorCode` string(10)?, `WingDirection` `RoomWingDirection`?, `FloorPlanRoomKey` string(100)?; enum `RoomWingDirection` (1–8); konfigurasi index `FloorCode`, unique berfilter kunci, CHECK wing dan kunci-butuh-lantai; migration `AddFloorPlanLocationToMstRoom` + Designer + snapshot. `FloorPlanRoomNaming` (aturan murni backend 15.6: nama kamar, label lantai/lokasi, label bed berikutnya, penanda turunan kelas). `RoomMasterWriteService` menerima tiga field, menolak ubah kunci/lantai, memvalidasi format dan keunikan. `GET rooms` saring `FloorCode`, `HasFloorPlanKey`. `BedMutationService.AllocateRoomCodeAsync` (batas bawah = nomor terbesar termasuk baris terhapus) dipakai `RoomController.CreateRoom`; logika isi-celah `RoomController.cs:790-814` dihapus |
| Batas source | `Areas/HealthServices/MasterData/{Models/MstRoom.cs, Enums/RoomWingDirection.cs, DTOs/RoomDtos.cs, Services/RoomMasterWriteService.cs, Services/FloorPlanRoomNaming.cs, Services/BedMutationService.cs, Controllers/RoomController.cs}`; `Repositories/Configurations/HealthServices/MstRoomConfiguration.cs`; `Migrations/*` |
| Dependency langsung | — |
| Pekerjaan independen | Tidak ada; task ini fondasi |
| Risiko / owner / aktivasi | Skema MasterData; seluruh tim MasterData meninjau batas ownership. Penerapan migration ke DB Hamzah butuh izin eksekusi terpisah |
| Verifikasi yang wajib | V-BE (QBE preflight saat eksekusi) + DNH-AT-451 (simpan/ubah wing), 461 (saring lantai), 901 |
| Definition of Done | D-BE; seluruh AC di bawah terpetakan ke source; laporan `task/report/backend/BE-RWI-232.md` |
| Laporan | [BE-RWI-232](../task/report/backend/BE-RWI-232.md) — 16 Oktober 2026 |

Acceptance positif:

- Kamar dengan `FloorCode` `3`, `WingDirection` BaratLaut, kunci `lantai-3/k-329` tersimpan; `FloorName` "Lantai 3" dan `LocationName` "Lantai 3 · Barat Laut" terbentuk otomatis.
- `GET rooms?floorCode=3` hanya mengembalikan kamar Lantai 3; `hasFloorPlanKey=false` mengembalikan kamar tanpa kunci.
- Sesudah kamar `RM-RSMMC-00003` dihapus lunak, kamar baru mendapat nomor di atas nomor terbesar yang pernah dipakai.

Acceptance negatif:

- Kunci yang sudah dipakai kamar lain → 409 `FLOOR_PLAN_KEY_DUPLICATE`; mengubah kunci atau lantai yang terisi → 422 `FLOOR_PLAN_KEY_IMMUTABLE`.
- Arah 9 → 400 `WING_DIRECTION_INVALID`; kunci tanpa lantai → 400 `FLOOR_CODE_REQUIRED`.
- Migration tidak memuat operasi destruktif; kamar existing tanpa kunci tidak berubah.

### ✅ BE-RWI-233 — Kelas khusus menerima kategori transfer apa pun

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Bukti runtime dengan kelas resmi menunggu langkah data D-DNH-1 (risiko kartu, bukan acceptance criteria). Bukti: [laporan](../task/report/backend/BE-RWI-233.md) |
| Slice / outcome | Transfer ke atau dari ICU, Isolasi, ODC, dan Perinatologi tidak tertahan perbandingan naik/turun kelas |
| Ukuran / batas | Satu aturan pada service perbandingan kelas dan validasi kategori transfer |
| Jejak | FR-DNH-007; RWI-DEC-312, 319; RWI-AC-450, 458; VAL-DNH-13 |
| Desain / kontrak | Backend 15.7 (kelas khusus); kamus data 22.1 (`comparisonResult` `Special`); API 14.1, 14.3 (`ComparisonResult`, `AllowedManualCategories`) |
| Reuse / disposisi | `DNH-CAP-16` Extend; `DNH-CAP-17` Reuse with adapter |
| Cakupan | `InpBedClassComparisonService.CompareAsync`: kelas tidak ditemukan/nonaktif → `Unknown`; asal **atau** tujuan berjenis ICU (7), Isolation (10), Odc (14), Perinatology (15) → `Special` tanpa memeriksa digest; Id sama → `SameGrade`; selain itu aturan existing. `AllowedManualCategories` ketiga kategori untuk `Special`; validasi transfer tidak memunculkan `TRANSFER_CATEGORY_MISMATCH`/`CLASS_ORDER_UNVERIFIED` untuk `Special`; snapshot menyimpan `Special` |
| Batas source | `Areas/HealthServices/InPatientManagement/Services/InpBedClassComparisonService.cs`; titik validasi kategori transfer pada `InpBedOccupancyService` (bagian transfer saja) |
| Dependency langsung | — |
| Pekerjaan independen | Tidak bergantung data denah |
| Risiko / owner / aktivasi | `InpBedOccupancyService.cs` sedang diubah pekerjaan paralel issue 022; kerjakan di atas hasil yang sudah di-commit. Bukti runtime butuh D-DNH-1 |
| Verifikasi yang wajib | V-BE + DNH-AT-450, 458 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-233.md` |
| Laporan | [BE-RWI-233](../task/report/backend/BE-RWI-233.md) — 16 Oktober 2026 |

Acceptance positif:

- Kelas 3 → ICU dengan Up, Down, atau Same Grade tersimpan; ICU → ICU juga bebas.
- Snapshot lokasi penempatan tujuan menyimpan hasil perbandingan `Special` dan kategori pilihan petugas.

Acceptance negatif:

- Kelas 2 → Kelas 1 dicatat Down Grade tetap 422 `TRANSFER_CATEGORY_MISMATCH`.
- Kelas biasa dengan digest urutan tidak cocok tetap 422 `CLASS_ORDER_UNVERIFIED`; HCU dan NICU tidak dianggap kelas khusus.

### ✅ BE-RWI-234 — High/Low Dependency tercatat per episode beserta riwayat

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Migration M-DNH-2 `AddCareDependencyToInpEpisode` **diterapkan ke `QuilvianNewDevHamzah`**; katalog terverifikasi. Pemberian hak `SetCareDependency` menunggu `BM-G03`; penayangan produksi menunggu `DNH-G04`. Bukti: [laporan](../task/report/backend/BE-RWI-234.md) |
| Slice / outcome | Petugas berwenang mencatat High/Low Dependency; riwayat tidak pernah ditimpa |
| Ukuran / batas | Satu aggregate episode: lima kolom, satu tabel riwayat, satu migration, dua endpoint, satu action hak akses baru |
| Jejak | FR-DNH-013, 014; RWI-DEC-302, 303; RWI-AC-440, 441; VAL-DNH-14–17; GUARD-DNH-02; NFR-804 |
| Desain / kontrak | Backend 15.4 (`InpEpisodeService.CareDependency`); kamus data 22.2.4, 22.2.5, 22.3; API 14.2 Inpatient Episode, 14.3 `SetCareDependencyRequest`/`CareDependencyHistoryResponse`; hak akses 12.1–12.3; transisi 12.2 |
| Reuse / disposisi | `DNH-CAP-19` Missing → baru; pola `SetIsolationRequirementAsync` |
| Cakupan | `InpEpisode` + `CareDependencyLevel`, `CareDependencySource`, `CareDependencySetByUserId`, `CareDependencySetAt`, `CareDependencyNote`; model + konfigurasi `InpCareDependencyHistory`; enum `InpCareDependencyLevel` dan `InpCareDependencySource`; migration `AddCareDependencyToInpEpisode`. Berkas `InpEpisodeService.CareDependency.cs`: kunci episode, aturan pencatat (Draft → AdmissionNote; Admitted/DischargePending → penugasan perawat aktif lewat `GetEmployeeId` atau DPJP aktif lewat `GetDoctorId`), cek `ExpectedSetAt`, tambah baris riwayat. `PATCH /episodes/{id}/care-dependency` dengan `[AccessAction("SetCareDependency", ...)]` dan `[AccessPermission("InpatientEpisode", "SetCareDependency")]`; `GET /{id}/care-dependency-history`; lima field pada `InpatientEpisodeDetailResponse`. Logger tanpa catatan |
| Batas source | `Areas/HealthServices/InPatientManagement/{Models/InpEpisode.cs, Models/InpCareDependencyHistory.cs, Enums/*, DTOs/InpatientCareDependencyDtos.cs, Services/InpEpisodeService.CareDependency.cs, Controllers/InpatientEpisodeController.cs}`; `Repositories/Configurations/HealthServices/InPatientManagement/*`; `Repositories/ApplicationDbContext.cs`; `Migrations/*` |
| Dependency langsung | — |
| Pekerjaan independen | Tidak bergantung data denah |
| Risiko / owner / aktivasi | Kriteria klinis milik SOP keperawatan (RWI-OQ-141); penayangan produksi menunggu `DNH-G04`. Pemberian hak `SetCareDependency` menunggu `BM-G03` |
| Verifikasi yang wajib | V-BE + DNH-AT-440, 441, 908 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-234.md` |
| Laporan | [BE-RWI-234](../task/report/backend/BE-RWI-234.md) — 16 Oktober 2026 |

Acceptance positif:

- Episode Draft + petugas admisi → Low tercatat bersumber AdmissionNote; perawat penanggung jawab aktif → High bersumber NurseAssessment; DPJP aktif → Low bersumber DoctorDecision; riwayat tiga baris, terbaru di atas.
- Menyimpan nilai yang sama tetap menambah satu baris riwayat sebagai penilaian ulang.

Acceptance negatif:

- Perawat tanpa penugasan aktif → 403 `CARE_DEPENDENCY_ACTOR_NOT_ASSIGNED`; episode Closed/Cancelled → 422 `EPISODE_READ_ONLY`.
- `ExpectedSetAt` berbeda dari nilai terakhir → 409 `STALE_CARE_DEPENDENCY`; level 3 → 400 `CARE_DEPENDENCY_LEVEL_INVALID`.
- Tidak ada jalur kelayakan, kategori transfer, maupun tarif yang membaca HD/LD.

### ✅ BE-RWI-235 — Writer master mengikuti aturan denah

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Bukti nama kelas resmi menunggu D-DNH-1; uji editor menunggu data denah D-DNH-3. Bukti: [laporan](../task/report/backend/BE-RWI-235.md) |
| Slice / outcome | Menambah, mengurangi, melabeli bed, dan mengubah kelas kamar mengikuti aturan denah tanpa menyentuh penghuni |
| Ukuran / batas | Tiga writer existing (bed, kamar, kelas); tanpa endpoint baru |
| Jejak | FR-DNH-009, 010, 016; RWI-DEC-304, 317, 318; RWI-AC-444, 454, 455, 456, 457; VAL-DNH-07, 08, 10; DNH-AT-902, 903, 909 |
| Desain / kontrak | Backend 15.4 (writer), 15.6 (nama, label, penanda turunan); kamus data 22.2.1, 22.2.2; API 14.2 Bed dan Patient Class, 14.4 |
| Reuse / disposisi | `DNH-CAP-23`, `24` Reuse with adapter; `DNH-CAP-26` Extend |
| Cakupan | `BedMasterWriteService`: label server `<nomor kamar>-<huruf>` untuk kamar berkunci, label unik per lantai (`BED_LABEL_DUPLICATE`), hapus hanya bila tidak pernah dirujuk penempatan/pesanan (`BED_HAS_USAGE_HISTORY`), ubah label lewat PUT. `RoomMasterWriteService`: ubah kelas kamar berkunci → nama kamar dan penanda turunan kamar/bed (backend 15.6) dalam guard berbahaya existing; pesan `BED_HOLD_CONFLICT` menyebut jumlah penghuni dan pesanan; ubah wing tidak berbahaya dan menyegarkan `LocationName`. `PatientClassMasterWriteService`: ganti nama kelas menyegarkan nama kamar berkunci dalam transaksi yang sama |
| Batas source | `Areas/HealthServices/MasterData/Services/{BedMasterWriteService.cs, RoomMasterWriteService.cs, PatientClassMasterWriteService.cs, FloorPlanRoomNaming.cs}`; `Controllers/BedController.cs` bila respons perlu field |
| Dependency langsung | `BE-RWI-232` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Penanda turunan hanya ditulis saat kamar kosong; seluruh tim MasterData. Bukti nama kelas resmi butuh D-DNH-1 |
| Verifikasi yang wajib | V-BE + DNH-AT-444, 454, 455, 456, 457, 902, 903, 909 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-235.md` |
| Laporan | [BE-RWI-235](../task/report/backend/BE-RWI-235.md) — 16 Oktober 2026 |

Acceptance positif:

- Tambah bed di kamar 329 berpenghuni 2 → label `329-D`, kode `BD-RSMMC-#####`, kesiapan `Unverified`; tambah boks → `IsForNewborn` true.
- Ubah kelas kamar kosong 505 → nama dan penanda turunan berubah; keenam bed Tidak Tersedia sampai disahkan.
- Ganti nama kelas "Kelas I" → "Kelas 1" → seluruh kamar berkunci kelas itu bernama "Kelas 1 …"; snapshot penempatan lama tetap.

Acceptance negatif:

- Ubah kelas kamar berpenghuni → 409 dengan jumlah penghuni dan pesanan, tanpa perubahan.
- Label ganda di lantai yang sama → 409 `BED_LABEL_DUPLICATE`; kamar 321 kedua melanjutkan `321-D`.
- Hapus bed beriwayat → 422 `BED_HAS_USAGE_HISTORY`; nonaktif tetap dapat dilakukan.

### ✅ BE-RWI-236 — Editor membaca susunan lantai dan mengaktifkan ruang cadangan

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Keempat acceptance criteria terpetakan ke source (aktivasi dalam satu transaksi koordinator). `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Injeksi kegagalan PostgreSQL `NOT RUN`; atomisitas dibuktikan lewat struktur transaksi. Bukti: [laporan](../task/report/backend/BE-RWI-236.md) |
| Slice / outcome | Editor Master Bed mendapat susunan satu lantai tanpa identitas pasien, dan ruang cadangan menjadi kamar beserta bed-nya dalam satu transaksi |
| Ukuran / batas | Satu service, dua endpoint |
| Jejak | FR-DNH-009, 010; RWI-DEC-306, 318; RWI-AC-451, 457; VAL-DNH-09; DNH-AT-913 |
| Desain / kontrak | Backend 15.1 (aktivasi atomik), 15.4 `FloorPlanLayoutService`; API 14.2 Room `GET /floor-plan-layout`, `POST /floor-plan-layout/rooms`, 14.3 `FloorPlanLayoutResponse`, `ActivateFloorPlanRoomRequest`, `FloorPlanRoomActivationResponse` |
| Reuse / disposisi | `DNH-CAP-41` sisi API; coordinator dan receipt existing |
| Cakupan | `FloorPlanLayoutService`: baca kamar berkunci per lantai, bed beserta `HasUsageHistory`, jumlah Terisi/Dipesan, `AffectedBedVersions`, daftar kamar tanpa kunci; aktivasi kamar + N bed + M boks di bawah satu `coordinator.ExecuteAsync` dengan satu receipt `Room` dan event `BedCreated`, nomor unik per lantai (`ROOM_NUMBER_DUPLICATE`). Endpoint pada `RoomController` (Room : Read, Room : Create); DTO `FloorPlanLayoutDtos.cs`; DI |
| Batas source | `Areas/HealthServices/MasterData/{Services/FloorPlanLayoutService.cs, Services/BedMutationService.cs, Controllers/RoomController.cs, DTOs/FloorPlanLayoutDtos.cs}`; `Program.cs` |
| Dependency langsung | `BE-RWI-235` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Transaksi multi-bed; owner InPatientManagement + MasterData |
| Verifikasi yang wajib | V-BE + DNH-AT-913 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-236.md` |
| Laporan | [BE-RWI-236](../task/report/backend/BE-RWI-236.md) — 16 Oktober 2026 |

Acceptance positif:

- Susunan Lantai 3 memuat kamar, bed, label, `HasUsageHistory`, dan versi bed terdampak, tanpa nama atau No. RM pasien.
- Aktivasi cadangan nomor 324 Kelas 2 dengan 3 bed → satu kamar dan tiga bed `Unverified` dalam satu commit.

Acceptance negatif:

- Nomor yang sudah dipakai kamar aktif di lantai itu → 409 `ROOM_NUMBER_DUPLICATE`.
- Kegagalan disuntik sesudah kamar dibuat → tidak ada kamar maupun bed tersimpan.

### ✅ BE-RWI-237 — Susunan denah diimpor dengan pemeriksaan, catatan run, dan kesiapan per lingkungan

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Migration M-DNH-3 `AddMstFloorPlanImportRun` **diterapkan ke `QuilvianNewDevHamzah`** (model pekerjaan paralel dikeluarkan dari migration). Menjalankan impor = D-DNH-3, wewenang terpisah, belum dilakukan. Bukti: [laporan](../task/report/backend/BE-RWI-237.md) |
| Slice / outcome | Admin memeriksa lalu mengimpor 106 kamar dan 168 bed; impor ulang tidak menggandakan; produksi tidak dapat menetapkan bed siap |
| Ukuran / batas | Satu importer, satu tabel catatan run, satu migration, satu opsi startup, satu action hak akses baru |
| Jejak | FR-DNH-011, 012; RWI-DEC-306, 321, 323; RWI-AC-446, 459, 461; VAL-DNH-01, 05, 06, 18, 19, 23; DNH-AT-905, 906; NFR-804 |
| Desain / kontrak | Backend 15.1, 15.4 `FloorPlanImportService`, `MstFloorPlanImportRun`, `BedManagementOptions`; kamus data 22.2.3, 22.3, 22.4; API 14.2 `POST /floor-plan-layout/import`, 14.3 `FloorPlanImportRequest`/`FloorPlanImportResult`, 14.4 |
| Reuse / disposisi | `DNH-CAP-27` Missing → baru; `DNH-CAP-08` diganti pemetaan admin |
| Cakupan | Model + konfigurasi `MstFloorPlanImportRun`, migration `AddMstFloorPlanImportRun`. `FloorPlanImportService`: validasi seluruh baris dulu, laporan per kunci (Created/Updated/Unchanged/Rejected), lampiran `ExistingRoomId`, `FLOOR_PLAN_DRIFT` tanpa mengubah kamar existing, buat kamar dan bed baru di bawah satu coordinator + satu receipt `FloorPlanImport` + satu catatan run; dry run tanpa tulis dan tanpa receipt. `BedManagementOptions.FloorPlanImport` (`MarkImportedBedsReady` = false, `ReadyEvidenceReference`) + validator startup yang menolak `true` di `Production` atau tanpa referensi; bila aktif, hanya bed yang dibuat run itu ditetapkan `Ready` dengan event `ImportReadyForTest`. Endpoint `[AccessAction("ImportFloorPlan", ...)]` + `[AccessPermission("Room", "ImportFloorPlan")]` |
| Batas source | `Areas/HealthServices/MasterData/{Models/MstFloorPlanImportRun.cs, Services/FloorPlanImportService.cs, Controllers/RoomController.cs, DTOs/FloorPlanLayoutDtos.cs}`; `Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs`; `Repositories/Configurations/HealthServices/MstFloorPlanImportRunConfiguration.cs`; `Repositories/ApplicationDbContext.cs`; `Program.cs`; `Migrations/*` |
| Dependency langsung | `BE-RWI-235` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Menjalankan impor adalah langkah D-DNH-3 dengan wewenang terpisah; impor produksi menunggu `DNH-G03`. Konfigurasi `MarkImportedBedsReady` hanya di `appsettings.Development.json` DB Hamzah |
| Verifikasi yang wajib | V-BE + DNH-AT-446, 459, 461, 905, 906 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-237.md` |
| Laporan | [BE-RWI-237](../task/report/backend/BE-RWI-237.md) — 16 Oktober 2026 |

Acceptance positif:

- Dry run mengembalikan ringkasan dan alasan per kamar tanpa menulis apa pun.
- Impor pertama membuat seluruh kamar terpetakan beserta bed-nya; impor kedua dengan manifest sama membuat 0 baris baru.
- Opsi dev/UAT aktif → bed yang dibuat run itu `Ready` dengan event `ImportReadyForTest`; opsi mati → `Unverified`.

Acceptance negatif:

- Kamar tanpa unit ditolak `ROOM_UNIT_MAPPING_MISSING` dan dilaporkan; kamar lain tetap masuk.
- `Production` + `MarkImportedBedsReady` true → aplikasi menolak menyala.
- Kamar existing yang kelas/unit/jumlah bed-nya berbeda tidak diubah; laporan memuat `FLOOR_PLAN_DRIFT`. Kegagalan di tengah impor → tidak ada baris maupun catatan run tersimpan.

### ✅ BE-RWI-238 — Monitoring membaca denah satu lantai

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Data denah nyata menunggu D-DNH-3; pengukuran jumlah query runtime `NOT RUN`. Bukti: [laporan](../task/report/backend/BE-RWI-238.md) |
| Slice / outcome | Satu panggilan per lantai memberi status kamar, badge, outline, ringkasan HD/LD, dan bed dengan enam status, dengan identitas tersamar sesuai hak |
| Ukuran / batas | Satu service proyeksi + satu endpoint baru + saring lantai pada monitoring existing |
| Jejak | FR-DNH-001, 002, 014; RWI-DEC-287, 299, 303, 316, 318, 320; RWI-AC-442, 443, 453 (Monitoring); VAL-DNH-11, 20; GUARD-DNH-01; NFR-802, 803 |
| Desain / kontrak | Backend 15.7; API 14.2 Bed Management `GET /floor-plan`, `GET /monitoring`, 14.3 `FloorPlanResponse`, `FloorPlanRoomResponse`, `FloorPlanBedResponse`, `BedHoldingContext`; transisi 12.1 |
| Reuse / disposisi | `DNH-CAP-09`, `11`, `21` Reuse with adapter; `DNH-CAP-10` Missing; `DNH-CAP-12` Extend |
| Cakupan | `InpFloorPlanQueryService` (mode Monitoring): kamar aktif berkunci di lantai, status bed dari proyeksi existing, `RoomStatus`, `Counts` dan `CribCounts` terpisah, `HasAvailableCrib`, `DependencySummary`, `IsSelectable`, ringkasan `Floors`, lantai bawaan; `BedHoldingContext` + `MedicalRecordNumber`, `CareDependencyLevel` mengikuti penyamaran existing; `GET bed-management/floor-plan`; `BedMonitoringQuery.FloorCode`; DTO `InpatientFloorPlanDtos.cs`; DI. Query terbatas per lantai tanpa N+1 |
| Batas source | `Areas/HealthServices/InPatientManagement/{Services/InpFloorPlanQueryService.cs, Services/InpBedManagementQueryService.cs, DTOs/InpatientFloorPlanDtos.cs, DTOs/InpatientBedManagementDtos.cs, Controllers/InpatientBedManagementController.cs}`; `Program.cs` |
| Dependency langsung | `BE-RWI-232`, `BE-RWI-234` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Privasi Housekeeping; hitungan boks pada badge menunggu RWI-OQ-144 di sisi tampilan, bukan API |
| Verifikasi yang wajib | V-BE + DNH-AT-442, 443, 453, 904, 910 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-238.md` |
| Laporan | [BE-RWI-238](../task/report/backend/BE-RWI-238.md) — 16 Oktober 2026 |

Acceptance positif:

- Kamar 3 bed Terisi/Dipesan/Tersedia → `Available`, `Counts` {3, 2, 1}, `IsSelectable` true; Terisi/Dipesan/Terisi → `Full`.
- Kamar dengan 1 HD dan 1 LD → `DependencySummary` {1, 1, 0}, `IsHighDependency` true.
- Tanpa `floorCode`, lantai dengan bed Tersedia terbanyak dikembalikan beserta ringkasan seluruh lantai.

Acceptance negatif:

- Profil OperationalOnly: JSON tanpa nama, No. RM, HD/LD, dan `DependencySummary`.
- Kapasitas tidak berasal dari `MstRoom.Capacity`; jumlah query per lantai tetap, tidak bertambah per bed.

### ✅ BE-RWI-239 — Pilih Bed admisi dan transfer membaca denah dengan kelayakan per pasien

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Uji runtime Pilih Bed menunggu data denah D-DNH-3. Bukti: [laporan](../task/report/backend/BE-RWI-239.md) |
| Slice / outcome | Langkah Pilih Bed dan tujuan transfer mendapat denah satu lantai dengan kelayakan setiap bed untuk episode itu |
| Ukuran / batas | Satu endpoint baca + saring lantai pada `available-beds` |
| Jejak | FR-DNH-005; RWI-DEC-299, 312, 316; RWI-AC-449, 453, 454; VAL-DNH-21, 22; DNH-AT-907 |
| Desain / kontrak | Backend 15.7 (outline, kelayakan per lantai); API 14.2 Bed Occupancy `GET /floor-plan`, `GET /available-beds`, 14.3 `FloorPlanSelectionQuery`, `FloorPlanBedResponse`, 14.4 `FLOOR_PLAN_PURPOSE_MISMATCH` |
| Reuse / disposisi | `DNH-CAP-13`, `18` Reuse with adapter; `DNH-CAP-14` Ready to reuse; `DNH-CAP-37` sisi API |
| Cakupan | `InpFloorPlanQueryService` mode Admission/Transfer: konteks episode dimuat sekali, aturan isolasi dan boks bayi existing diterapkan di memori untuk semua bed Tersedia, `Eligibility` dengan alasan, `IsSelectable` per pasien, `ComparisonResult` dan `AllowedManualCategories` untuk Transfer; `GET bed-occupancies/floor-plan`; validasi tujuan vs status episode; `AvailableBedQuery.FloorCode` |
| Batas source | `Areas/HealthServices/InPatientManagement/{Services/InpFloorPlanQueryService.cs, Services/InpBedOccupancyService.cs, DTOs/InpatientBedOccupancyDtos.cs, Controllers/InpatientBedOccupancyController.cs}` |
| Dependency langsung | `BE-RWI-233`, `BE-RWI-238` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | `InpBedOccupancyService.cs` sedang diubah pekerjaan paralel issue 022; kerjakan di atas hasil yang sudah di-commit |
| Verifikasi yang wajib | V-BE + DNH-AT-449, 453, 454, 907 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-239.md` |
| Laporan | [BE-RWI-239](../task/report/backend/BE-RWI-239.md) — 16 Oktober 2026 |

Acceptance positif:

- Pasien butuh isolasi: hanya Isolasi 319 `IsSelectable` bila bednya Tersedia; pasien biasa: 319 tidak ber-outline.
- Bayi rawat gabung yang ibunya di 329-A: boks 329-D layak; Ruang Bayi Perinatologi layak untuk episode bayi.
- Transfer dari Kelas 3 menampilkan ketiga kategori pada bed ICU.

Acceptance negatif:

- `Admission` untuk episode Admitted, atau `Transfer` tanpa penempatan aktif → 422 `FLOOR_PLAN_PURPOSE_MISMATCH`.
- Bed tidak layak tetap dikirim dengan alasan; tidak ada alasan jenis kelamin.

### ✅ BE-RWI-240 — Aturan dan kolom jenis kelamin bed dihapus

| Field | Isi |
| --- | --- |
| Status | ✅ **SELESAI 16 Oktober 2026.** Keempat acceptance criteria terpetakan ke source; pencarian literal keempat istilah jenis kelamin di source 0 hasil. `dotnet build --no-incremental` 0 error, 286 warning (0 dari kode baru). Migration M-DNH-4 `DropGenderFlagsFromRoomAndBed` dibuat paling akhir dan **diterapkan ke `QuilvianNewDevHamzah`** (lewat skrip SQL idempoten EF karena `database update` ditolak model paralel tanpa migration). DNH-AT-912 `NOT RUN` — rilis ke lingkungan bersama wajib bersama `FE-RWI-253`. Bukti: [laporan](../task/report/backend/BE-RWI-240.md) |
| Slice / outcome | Kamar campur diterima tanpa peringatan; tidak ada kolom, field, aturan, atau pesan jenis kelamin pada bed |
| Ukuran / batas | Penghapusan lintas model, konfigurasi, DTO, controller, writer, aturan kelayakan + migration M-DNH-4 |
| Jejak | FR-DNH-006; RWI-DEC-315; RWI-AC-439, 452; VAL-DNH-12; DNH-AT-912; NFR-805 |
| Desain / kontrak | Backend 15.2, 15.8 (M-DNH-4); kamus data 22.2.1, 22.2.2, 22.4; API 14.1 (breaking), 14.4 (kode dipensiunkan) |
| Reuse / disposisi | `DNH-CAP-07`, `15` Repair |
| Cakupan | Hapus aturan 4 dan 5 beserta `BedGenderMessage`/`NormalizeGender` di `InpBedOccupancyService`; hapus `IsForMale`/`IsForFemale` dari `MstRoom`, `MstBed`, konfigurasi, DTO kamar/bed/`AvailableBedResponse`, metadata form dan filter controller, writer, dan daftar perubahan berbahaya `BedMutationService`; migration `DropGenderFlagsFromRoomAndBed` dibuat paling akhir. Nomor aturan 4 dan 5 tidak dipakai ulang |
| Batas source | 13 berkas pada `git grep IsForMale\|IsForFemale\|BED_GENDER_MISMATCH\|PATIENT_GENDER_UNKNOWN` di HEAD `ca09cae9` (di luar `Migrations/` lama); `Migrations/*` baru |
| Dependency langsung | `BE-RWI-237`, `BE-RWI-239` |
| Pekerjaan independen | — |
| Risiko / owner / aktivasi | Breaking change; **dirilis bersama `FE-RWI-253`** (`DNH-MIS-08`). Data jenis kelamin pasien tidak disentuh |
| Verifikasi yang wajib | V-BE + DNH-AT-439, 452, 912 |
| Definition of Done | D-BE; laporan `task/report/backend/BE-RWI-240.md` |
| Laporan | [BE-RWI-240](../task/report/backend/BE-RWI-240.md) — 16 Oktober 2026 |

Acceptance positif:

- Ny. Sari dipesankan dan ditempatkan di 329-B saat Tn. Budi di 329-A, tanpa peringatan maupun penolakan.
- Respons kamar, bed, dan `available-beds` tidak memuat field jenis kelamin.

Acceptance negatif:

- `git grep` source (di luar migration lama) tidak menemukan `IsForMale`, `IsForFemale`, `BED_GENDER_MISMATCH`, `PATIENT_GENDER_UNKNOWN`.
- Klien lama yang masih mengirim field jenis kelamin tidak ditolak; field diabaikan.
