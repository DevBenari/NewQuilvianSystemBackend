# Laporan Perubahan Backend — `BE-RWI-232`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-232` |
| Judul | Kamar menyimpan lantai, arah wing, dan kunci denah dengan kode sistem yang aman |
| Slice | Kamar denah dapat dibuat dan dibaca per lantai; kode kamar baru tidak pernah bentrok dengan kamar yang dihapus lunak |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-232` |
| Trace | FR-DNH-008; RWI-DEC-306, 313, 314, 323; `DNH-DES-01`; RWI-AC-451, 461 (sebagian); VAL-DNH-01–04; INV-DNH-01; NFR-806; DNH-AT-451, 461, 901 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`); lock `DNH-DEL-20261016-01` |
| Dependency | Tidak ada (task fondasi) |
| Klasifikasi | `HEAVY` — skor 10: satu repo 0, berkas diperiksa > 20 = 2, berkas diubah 8 = 1, logika sedang 1, mengubah kontrak 2, schema/migration 2, keamanan terkait 1, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026: kerjakan seluruh task roadmap lewat `build-module-backend`, implementasi code dulu, `dotnet build` dan update database di akhir |
| Target tulis | `NewQuilvianSystemBackend`: source sesuai batas kartu, `Migrations/*`, laporan ini, roadmap/traceability (status saja) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, upstream `origin/MHamzah`; perubahan belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source, migration M-DNH-1, build, dan penerapan ke `QuilvianNewDevHamzah` terbukti |

---

## 1. Masalah yang diperbaiki

Sebelumnya kamar hanya punya teks bebas `FloorName` dan `LocationName`. Denah gedung tidak bisa tahu kamar mana yang digambar di posisi mana, sehingga denah tidak dapat dipakai untuk melihat, memilih, atau mengelola bed.

Kode kamar `RM-RSMMC-#####` juga dibuat controller dengan **mengisi nomor kosong terkecil** di antara kamar yang belum dihapus (`RoomController.cs:790-814` pada HEAD). Index unik `RoomCode` tidak berfilter, sehingga kamar yang sudah dihapus lunak tetap memegang kodenya.

> **Contoh masalah nyata.** Kamar RM-RSMMC-00001 s.d. 00003 ada, lalu 00003 dihapus lunak. Pembuatan kamar berikutnya menghitung 00003 sebagai "kosong" dan memakainya lagi, lalu database menolak karena index unik masih memuat 00003 milik kamar terhapus — petugas menerima galat sistem.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Setiap kamar yang tergambar di denah menyimpan lantai, arah wing, dan kunci bentuknya, dengan label lantai/lokasi yang selalu konsisten |
| Pelaku | Admin MasterData (hak `Room : Create`, `Room : Update`, `Room : Read`) |
| Pemicu | Admin membuat kamar baru atau mengubah kamar existing; layar Master Bed/denah menyaring kamar per lantai |
| Prasyarat | Unit layanan aktif; kelas pasien aktif bila diisi |

Langkah utama:

1. Admin mengirim `POST /rooms` dengan `FloorCode`, `WingDirection`, dan `FloorPlanRoomKey` (opsional).
2. Server memvalidasi arah wing 1–8, format kunci `lantai-<lantai>/<slug>`, kunci wajib bersama lantai, dan keunikan kunci.
3. Server mengalokasikan kode kamar dari pengalokasi nomor bersama `MST_ROOM` dengan batas bawah = nomor terbesar yang pernah dipakai, **termasuk** kamar terhapus.
4. Untuk kamar berkunci, server mengisi `FloorName` "Lantai 3", `LocationName` "Lantai 3 · Barat Laut", nama kamar `<kelas> <nomor>`, dan penanda turunan kelas.
5. Saat diubah lewat `PUT /rooms/{id}`, kunci dan lantai yang sudah terisi tidak dapat berubah; arah wing boleh diubah dan label lokasi ikut disegarkan.
6. `GET /rooms?floorCode=3` hanya mengembalikan kamar Lantai 3; `hasFloorPlanKey=false` hanya kamar tanpa kunci.

Aturan beserta contoh:

| Aturan | Contoh benar | Contoh ditolak |
| --- | --- | --- |
| Kunci `lantai-<FloorCode>/<slug>`, huruf kecil `[a-z0-9-]`, maks. 100 | `lantai-3/k-329` untuk lantai `3` | `Lantai-3/K-329` → 400 `INVALID_INPUT`; `lantai-2/k-329` untuk lantai `3` → 400 |
| Kunci butuh lantai | `FloorCode` `3` + kunci | kunci tanpa lantai → 400 `FLOOR_CODE_REQUIRED` |
| Arah wing 1–8 | `8` = Barat Laut | `9` → 400 `WING_DIRECTION_INVALID` |
| Kunci unik di antara kamar belum dihapus | — | kunci yang sudah dipakai kamar lain → 409 `FLOOR_PLAN_KEY_DUPLICATE` |
| Kunci dan lantai tetap setelah terisi | ubah wing 8 → 5 diterima, lokasi menjadi "Lantai 3 · Selatan" | ubah kunci `k-329` → `k-330` atau lantai `3` → `4` → 422 `FLOOR_PLAN_KEY_IMMUTABLE` |
| Kode kamar monotonik | 00001–00003 ada, 00003 terhapus → kamar baru 00004 | — |

Perubahan status: tidak ada status baru; kamar tetap master data aktif/nonaktif seperti sebelumnya.

Jalur tidak normal: semua penolakan di atas dikembalikan sebelum data disimpan; pada `PUT` penolakan membatalkan seluruh transaksi `BedMutationService`. Nomor kode yang sudah dialokasikan lalu batal disimpan **hangus** (perilaku baku pengalokasi bersama, `DEC-PLT-008`), tidak dipakai ulang.

Hasil akhir: kamar menyimpan tiga kolom baru; kamar existing tanpa kunci tidak berubah sama sekali.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Roadmap, lock `integrasi-denah-delivery-lock.json`, backend 15.1–15.8, kamus data 22.2.1/22.3/22.4, API 14.1–14.4, validasi 17, `AGENTS.md`, `rules/backend/*`, `docs/engineering/*`; `MstRoom.cs`, `MstRoomConfiguration.cs`, `RoomDtos.cs`, `RoomController.cs`, `RoomMasterWriteService.cs`, `BedMutationService.cs`, `NumberSeriesAllocator.cs`, `InpBedMutationCoordinator.cs`, `InpBedOperationFilter.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Enums/RoomWingDirection.cs` | Baru: enum Utara = 1 … BaratLaut = 8 |
| `Areas/HealthServices/MasterData/Models/MstRoom.cs` | Tambah `FloorCode` (10), `WingDirection`, `FloorPlanRoomKey` (100) |
| `Repositories/Configurations/HealthServices/MstRoomConfiguration.cs` | Konversi int, panjang kolom, index `FloorCode`, UNIQUE berfilter kunci, dua CHECK (baris 13–17, 72–80, 124–129) |
| `Areas/HealthServices/MasterData/Services/FloorPlanRoomNaming.cs` | Baru: aturan murni kunci, label lantai/lokasi, nama kamar, huruf bed, penanda turunan kelas (backend 15.6) |
| `Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs` | `ApplyFloorPlanLocationAsync` (baris 328): validasi dan penetapan lokasi; `ApplyKeyedRoomDerivationsAsync` (baris 378); dipanggil `UpdateRoomAsync`; response membawa tiga field + `LocationName` |
| `Areas/HealthServices/MasterData/Services/BedMutationService.cs` | `AllocateRoomCodeAsync` (baris 143) memakai `NumberSeriesAllocator` deret `MST_ROOM` |
| `Areas/HealthServices/MasterData/Controllers/RoomController.cs` | `CreateRoom` memakai alokator bersama (baris 418) dan writer lokasi denah; `GenerateRoomCodeAsync`/`ExtractRoomCodeNumber` (isi celah) dihapus; saring `floorCode`/`hasFloorPlanKey` (`ApplyFloorPlanFilter`, baris 642); sort `floorCode`; metadata form dan opsi arah wing |
| `Areas/HealthServices/MasterData/DTOs/RoomDtos.cs` | Field baru pada `RoomResponse`, `CreateRoomRequest`, `RoomCreateResponse`, `RoomUpdateResponse`, filter default, metadata |
| `Migrations/20261016035829_AddFloorPlanLocationToMstRoom.cs` (+ Designer, snapshot) | M-DNH-1, dibuat lewat `dotnet ef migrations add` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: tiga field baru pada request/response kamar; query `floorCode`, `hasFloorPlanKey`; kode galat baru `FLOOR_CODE_REQUIRED`, `WING_DIRECTION_INVALID`, `FLOOR_PLAN_KEY_DUPLICATE`, `FLOOR_PLAN_KEY_IMMUTABLE` |
| Database | M-DNH-1: tiga kolom nullable, `IX_MstRoom_FloorCode`, `IX_MstRoom_FloorPlanRoomKey` UNIQUE berfilter, `CK_MstRoom_WingDirection`, `CK_MstRoom_FloorPlanRoomKey_Floor`. Tanpa operasi destruktif. **Diterapkan** ke `QuilvianNewDevHamzah` 16 Oktober 2026 |
| Keamanan/Auth | Tidak ada action baru; endpoint existing tetap `Room : Read/Create/Update` |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kamar, kini dapat disaring per lantai dan ada/tidaknya kunci denah | `Room : Read` | Query existing + `floorCode?`, `hasFloorPlanKey?` | `PagedResult<RoomResponse>` |
| `POST` | `/` | Buat kamar; kode dari pengalokasi bersama; kamar berkunci mendapat label otomatis | `Room : Create` | `CreateRoomRequest` + `FloorCode?`, `WingDirection?`, `FloorPlanRoomKey?` | `RoomCreateResponse` |
| `PUT` | `/{id}` | Ubah kamar; arah wing boleh, kunci/lantai yang terisi tetap | `Room : Update` | `UpdateRoomRequest` (header `Idempotency-Key`) | `RoomUpdateResponse` + `OperationMeta` |

Kode status: 400 isian salah (`INVALID_INPUT`, `FLOOR_CODE_REQUIRED`, `WING_DIRECTION_INVALID`); 409 kunci sudah dipakai kamar lain; 422 kunci/lantai tidak boleh diubah; 403 di luar scope unit (existing).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | Area HealthServices, Module MasterData, registry `Mst` ACTIVE; `NEW CODE` untuk enum/naming, `TOUCHED LEGACY` untuk controller/writer; QBE-ENT-002, CFG-001/002, CODE-001/002/003/004/006, SVC-001, API-001, VAL-001, DTO-001 | `PASS` | Bagian 3.2; registry `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| `dotnet build` (setelah fase A) | Berhasil, 0 error, 286 warning (2 m 57 d) | `PASS` | Keluaran perintah, keluaran ke scratchpad karena `bin/` dikunci backend yang berjalan |
| `dotnet build --no-incremental` akhir (seluruh roadmap) | Berhasil, 0 error, 286 warning (1 m 5 d); 0 warning dari kode baru | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddFloorPlanLocationToMstRoom` | Migration hanya `AddColumn`, `CreateIndex`, `AddCheckConstraint` | `PASS` | Berkas migration |
| Penerapan ke `QuilvianNewDevHamzah` | Migration tercatat; kolom, CHECK, dan index ada di katalog; 68 kamar existing, 0 berkunci | `PASS` | Kueri `information_schema`/`pg_indexes`/`pg_constraint` lewat `dotnet fsi` + Npgsql; `dotnet ef migrations list` 0 pending |
| DNH-AT-451 simpan/ubah wing | `ApplyFloorPlanLocationAsync` menyimpan wing 1–8, menolak 9, menyegarkan `LocationName` | `PASS` | `RoomMasterWriteService.cs:328-372` |
| DNH-AT-461 saring lantai | `ApplyFloorPlanFilter` membandingkan `FloorCode` tanpa beda huruf; `hasFloorPlanKey` | `PASS` | `RoomController.cs:642` |
| DNH-AT-901 kode sesudah hapus lunak | Batas bawah alokator dihitung dari seluruh `RM-RSMMC-*` termasuk terhapus (`IgnoreQueryFilters`, tanpa saring `IsDelete`) | `PASS` | `BedMutationService.cs:143-148` |
| Verifikasi kontrak API | Method/path/hak akses tidak berubah; field baru sesuai API 14.3 | `PASS` | `RoomController.cs`, `RoomDtos.cs` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — backend yang berjalan masih memakai DLL lama (`dotnet run --no-build`, PID 24560); pengguna perlu build dan menjalankan ulang lebih dulu.

**Tidak dijalankan:** uji runtime lewat Swagger (alasan di atas); uji PostgreSQL disposable untuk dua pembuatan bersamaan berkunci sama (dijaga UNIQUE berfilter di database).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kamar `FloorCode` 3, wing BaratLaut, kunci `lantai-3/k-329` tersimpan; `FloorName` "Lantai 3" dan `LocationName` "Lantai 3 · Barat Laut" otomatis | Terpenuhi | `ApplyFloorPlanLocationAsync`; `FloorPlanRoomNaming.FloorName/LocationName` |
| `GET rooms?floorCode=3` hanya Lantai 3; `hasFloorPlanKey=false` kamar tanpa kunci | Terpenuhi | `ApplyFloorPlanFilter` |
| Sesudah `RM-RSMMC-00003` dihapus lunak, kamar baru di atas nomor terbesar | Terpenuhi | `AllocateRoomCodeAsync`; logika isi-celah dihapus |
| Kunci ganda → 409 `FLOOR_PLAN_KEY_DUPLICATE`; ubah kunci/lantai terisi → 422 `FLOOR_PLAN_KEY_IMMUTABLE` | Terpenuhi | `RoomMasterWriteService.cs:340, 343, 359` |
| Arah 9 → 400 `WING_DIRECTION_INVALID`; kunci tanpa lantai → 400 `FLOOR_CODE_REQUIRED` | Terpenuhi | `RoomMasterWriteService.cs:335`; `FloorPlanRoomNaming.ValidateRoomKey` |
| Migration tanpa operasi destruktif; kamar existing tanpa kunci tidak berubah | Terpenuhi | Isi M-DNH-1; katalog: 68 kamar, 0 berkunci |
| D-BE: QBE preflight, build, kontrak API, proses bisnis, migration ditinjau, penerapan DB Hamzah dengan izin | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `POST /rooms` untuk kamar berkunci tetap melewati validasi lama "nama/nomor kamar unik per unit". Dua kamar bernomor sama di unit yang sama (kasus "321" kedua) sebaiknya masuk lewat importer (`BE-RWI-237`), bukan `POST /rooms` |
| Masalah yang diketahui | `CreateRoom` masih menulis `DbContext` langsung (legacy, `TOUCHED LEGACY`); hanya alokasi kode dan lokasi denah yang dipindah ke service |
| Risiko tersisa | Skema MasterData ditinjau seluruh tim MasterData; backend lokal harus dibuild ulang sebelum dipakai karena DB sudah memuat M-DNH-1..4 |
| Perubahan sampingan | Build kedua roadmap ini gagal `OutOfMemoryException` di Roslyn karena 33 Designer migration (169 MB) ikut dikompilasi. Dijalankan `tooling/migrations/Update-MigrationHistory.ps1` (keputusan repo DEC-BUILD-002): 26 Designer migration lama dipindah ke `Migrations/History/`, `MigrationMetadata.g.cs/.props` dibangkitkan ulang (293 deklarasi); isi migration tidak berubah dan EF tetap mengenali seluruh migration (`migrations list` lengkap). Snapshot ikut diurutkan ulang EF pada blok `InpEpisode` (urutan properti) |
| Interupsi | Build fase B gagal OOM, dipulihkan dengan langkah di atas tanpa mengulang pekerjaan |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 (status akhir seluruh roadmap). Berkas task ini: 8 berkas source + 2 berkas migration baru + snapshot |
| Langkah berikutnya | Pengguna: hentikan backend, build, jalankan ulang. Lanjut `FE-RWI-250` (editor master) memakai field baru |
