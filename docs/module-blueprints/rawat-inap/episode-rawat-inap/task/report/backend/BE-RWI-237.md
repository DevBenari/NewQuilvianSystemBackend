# Laporan Perubahan Backend — `BE-RWI-237`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-237` |
| Judul | Susunan denah diimpor dengan pemeriksaan, catatan run, dan kesiapan per lingkungan |
| Slice | Admin memeriksa lalu mengimpor 106 kamar dan 168 bed; impor ulang tidak menggandakan; produksi tidak dapat menetapkan bed siap |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-237` |
| Trace | FR-DNH-011, 012; RWI-DEC-306, 321, 323; RWI-AC-446, 459, 461; VAL-DNH-01, 05, 06, 18, 19, 23; GUARD-DNH-03, 04; INV-DNH-07, 09, 10; NFR-804; DNH-AT-446, 459, 461, 905, 906 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | `BE-RWI-235` ✅ |
| Klasifikasi | `HEAVY` — skor 12: satu repo 0, diperiksa > 20 = 2, diubah > 8 = 2, logika kompleks 2, kontrak baru 2, schema/migration 2, auth terkait 1, workflow terbatas 1 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | Batas source kartu, `Migrations/*` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source, migration M-DNH-3, build, dan penerapan ke `QuilvianNewDevHamzah` terbukti. **Menjalankan impor** adalah langkah D-DNH-3 dengan wewenang terpisah dan belum dilakukan |

---

## 1. Masalah yang diperbaiki

Seeder sengaja tidak membuat kamar/bed, dan belum ada importer. Memasukkan 106 kamar dan 168 bed denah satu per satu lewat layar master rawan salah dan tidak dapat diulang dengan aman.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Memasukkan susunan denah sekali, aman diulang, dengan laporan per kamar dan jejak audit |
| Pelaku | Admin impor berwenang (`Room : ImportFloorPlan`), juga untuk dry run |
| Pemicu | Layar Impor mengirim manifest dari data denah frontend, kelas dan unit dipilih dari daftar berlabel |
| Prasyarat | Kelas resmi dan pemetaan unit (D-DNH-1, RWI-DEC-323) |

Langkah utama:

1. **Format manifest** (400 bila salah, seluruh permintaan gagal): `DryRun` wajib, `ManifestVersion` ≤ 50, 1–300 kamar, kunci dan lantai sah, nomor kamar wajib, nama ruang wajib untuk kamar tanpa nomor angka, kelas wajib, arah wing 1–8, 0–20 bed per kamar dengan huruf unik.
2. **Rencana per kamar** (dihitung di memori, tanpa menulis): kunci ganda dalam manifest, unit (wajib/aktif/rawat inap/dalam scope), kelas aktif, kamar existing berkunci sama, kamar tanpa kunci yang dipilih (`ExistingRoomId`), label bed unik per lantai.
3. **Dry run** mengembalikan ringkasan dan alasan per kamar, tanpa receipt dan tanpa catatan run.
4. **Impor** (header `Idempotency-Key`): satu koordinator, satu receipt `FloorPlanImport`, satu `MstFloorPlanImportRun`. Kamar baru dibuat beserta bed-nya (`Unverified`, event `BedCreated`); kamar existing hanya diberi kunci/label lokasi — kelas, unit, dan bed **tidak** diubah.
5. **Kesiapan per lingkungan**: bila `BedManagement:FloorPlanImport:MarkImportedBedsReady` aktif (hanya dev/UAT, wajib `ReadyEvidenceReference`), bed yang **dibuat run itu** ditetapkan `Ready` dengan event `ImportReadyForTest`. Aplikasi menolak menyala bila opsi aktif di `Production` atau tanpa referensi.

| Hasil per kamar | Kapan | Contoh kode |
| --- | --- | --- |
| `Created` | Kunci belum ada | — |
| `Updated` | Kunci dilekatkan ke kamar existing, atau label lokasi/nama kamar berkunci disegarkan | `FLOOR_PLAN_DRIFT` bila kelas/unit/jumlah bed berbeda |
| `Unchanged` | Kamar berkunci sudah sama | `FLOOR_PLAN_DRIFT` bila berbeda |
| `Rejected` | Gagal aturan bisnis | `ROOM_UNIT_MAPPING_MISSING`, `ROOM_UNIT_INVALID`, `ACCESS_DENIED`, `PATIENT_CLASS_NOT_FOUND`, `FLOOR_PLAN_KEY_DUPLICATE`, `FLOOR_PLAN_KEY_IMMUTABLE`, `NOT_FOUND`, `BED_LABEL_DUPLICATE` |

> **Contoh berangka.** Manifest 106 kamar, 1 kamar belum dipetakan unit. Dry run: `Rooms` {Created 105, Rejected 1}, `Beds` {Created 166, Rejected 2}; tidak ada yang ditulis. Impor pertama membuat 105 kamar dan 166 bed. Impor kedua dengan manifest sama: `Rooms` {Unchanged 105, Rejected 1}, **0 baris baru**, satu catatan run baru. Di DB Hamzah dengan opsi siap aktif: `BedsMarkedReady` 166 pada impor pertama, 0 pada impor kedua.

Jalur tidak normal: kunci yang mendadak dipakai proses lain sesudah pemeriksaan, atau galat apa pun di tengah impor → seluruh impor dibatalkan; tidak ada kamar, bed, maupun catatan run tersimpan.

Hasil akhir: catatan run memuat manifest hash, lingkungan, penetapan siap, dan ringkasan per kunci (`SummaryJson` skema 22.3) tanpa data pasien.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.1/15.4/15.8, kamus data 22.2.3/22.3/22.4, API 14.2–14.4, validasi 17, hak akses 12, transisi 12.3/12.4; `BedManagementOptions(.cs|Validator.cs)`, `Program.cs` (opsi `ValidateOnStart`), `InpBedMutationCoordinator.cs`, `InpBedReadinessConfiguration.cs` (CHECK `Ready`), `InpBedReadinessService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstFloorPlanImportRun.cs` | Baru, mewarisi `IdentityModel` |
| `Repositories/Configurations/HealthServices/MstFloorPlanImportRunConfiguration.cs` | Baru: `character(64)`, `jsonb`, CHECK referensi siap, index `ManifestHash`/`StartedAtUtc`, FK Restrict |
| `Repositories/ApplicationDbContext.cs` | DbSet `MstFloorPlanImportRuns`; penjaga append-only (baris 1266) |
| `Areas/HealthServices/MasterData/Services/FloorPlanImportService.cs` | Baru: format (baris 168), rencana (baris 212), drift (baris 325), tulis + `ImportReadyForTest` (baris 131), catatan run (baris 141), hash kanonis (baris 476) |
| `Areas/HealthServices/InPatientManagement/Options/BedManagementOptions.cs` | `FloorPlanImport` {`MarkImportedBedsReady` = false, `ReadyEvidenceReference`} (baris 16) |
| `Areas/HealthServices/InPatientManagement/Options/BedManagementOptionsValidator.cs` | Menerima `IHostEnvironment`; menolak siap di `Production` atau tanpa referensi (baris 41); operasi canonical `Room:ImportFloorPlan` (baris 15) |
| `Areas/HealthServices/MasterData/Controllers/RoomController.cs` | `POST floor-plan-layout/import` (baris 100) dengan action baru `ImportFloorPlan` |
| `Areas/HealthServices/MasterData/DTOs/FloorPlanLayoutDtos.cs` | DTO impor |
| `Program.cs` | DI `FloorPlanImportService` (baris 726) |
| `Migrations/20261016042242_AddMstFloorPlanImportRun.cs` (+ Designer, snapshot) | M-DNH-3 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru; kode baris laporan sesuai API 14.4 |
| Database | M-DNH-3: tabel `MstFloorPlanImportRun`. **Diterapkan** ke `QuilvianNewDevHamzah` 16 Oktober 2026. Impor belum dijalankan (D-DNH-3) |
| Keamanan/Auth | Action baru `Room : ImportFloorPlan` (`AccessType` Create, tampil di layar Akses Role); scope unit per baris; konfigurasi kesiapan produksi ditolak saat startup |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/floor-plan-layout/import` | Memeriksa (dry run) atau menjalankan impor susunan kamar dan bed | `Room : ImportFloorPlan` | `FloorPlanImportRequest`; header `Idempotency-Key` (impor) | `FloorPlanImportResult` |

Pemeriksaan hak akses: `[AccessAction("ImportFloorPlan", …)]` dan `[AccessPermission("Room", "ImportFloorPlan")]` cocok dengan `ControllerName = "Room"`.

Kode status: 200 dengan `Rows[].Codes` untuk penolakan per kamar; 400 manifest tidak valid; 403 tanpa hak; 409 kunci idempotency dipakai isian lain atau kunci baru direbut proses lain.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Mst` ACTIVE; entity baru `MstFloorPlanImportRun` (QBE-ENT-001, NAM-002, CFG-001, MOD-002); SVC-001, TXN-001, PERM-001, LOG-001 (aktor pada run dan event), AUD-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| Migration M-DNH-3 | Hanya `CreateTable`/`CreateIndex`. Hasil `migrations add` sempat menangkap model pekerjaan paralel (`MstPatient` + `PatNewbornBirthSequence`); bagian itu dikeluarkan dari migration, Designer, dan snapshot — delta snapshot terverifikasi hanya `MstFloorPlanImportRun` | `PASS` | Diff target model M-DNH-2 vs snapshot |
| Penerapan ke `QuilvianNewDevHamzah` | Tabel 20 kolom, CHECK `CK_MstFloorPlanImportRun_ReadyEvidence`, dua index ada | `PASS` | Kueri katalog |
| DNH-AT-446 idempoten | Kamar berkunci existing → `Unchanged`/`Updated`, tidak ada `Create`; bed hanya dibuat untuk kamar baru | `PASS` | `FloorPlanImportService.PlanAsync` |
| DNH-AT-459 kesiapan | Hanya bed `createdBeds` run itu yang `Ready` + `ImportReadyForTest`; opsi mati → `Unverified` | `PASS` | Baris 88–137 |
| DNH-AT-461 pemetaan unit | Tanpa unit → `ROOM_UNIT_MAPPING_MISSING`, kamar lain tetap diproses | `PASS` | Baris 262 |
| DNH-AT-905 kamar existing | Tidak ada penulisan kelas/unit/bed kamar existing; drift dilaporkan | `PASS` | `LabelExistingRoomAsync`, `AddDriftCode` (baris 325) |
| DNH-AT-906 validator startup | `ValidateOnStart` + validator menolak `Production` dan referensi kosong | `PASS` | `BedManagementOptionsValidator.cs:38-45`; `Program.cs` `AddOptions…ValidateOnStart()` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — menjalankan impor adalah D-DNH-3 (wewenang terpisah); konfigurasi `MarkImportedBedsReady` hanya untuk `appsettings.Development.json` DB Hamzah dan belum diubah.

**Tidak dijalankan:** impor nyata, startup aplikasi dengan konfigurasi `Production` (validator dibuktikan lewat source).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Dry run mengembalikan ringkasan dan alasan per kamar tanpa menulis apa pun | Terpenuhi | `if (dryRun) return result;` (baris 84) sebelum koordinator |
| Impor pertama membuat seluruh kamar terpetakan beserta bed-nya; impor kedua manifest sama 0 baris baru | Terpenuhi | `PlanAsync` + `CreateRoomWithBedsAsync` |
| Opsi dev/UAT aktif → bed run itu `Ready` + `ImportReadyForTest`; opsi mati → `Unverified` | Terpenuhi | Baris 88–137 |
| Kamar tanpa unit ditolak `ROOM_UNIT_MAPPING_MISSING`, kamar lain tetap masuk | Terpenuhi | Baris 262 |
| `Production` + `MarkImportedBedsReady` true → aplikasi menolak menyala | Terpenuhi | Validator + `ValidateOnStart` |
| Kamar existing berbeda tidak diubah; laporan `FLOOR_PLAN_DRIFT`; kegagalan di tengah → tidak ada baris maupun catatan run | Terpenuhi | `AddDriftCode`; satu transaksi koordinator |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pekerjaan paralel menambah model `PatNewbornBirthSequence` dan tiga kolom `MstPatient` tanpa migration; pemiliknya wajib membuat migration sendiri. Selama belum ada, `dotnet ef database update` ditolak `PendingModelChangesWarning` |
| Masalah yang diketahui | Arah wing kamar existing hanya diisi impor bila masih kosong, supaya koreksi admin tidak tertimpa impor ulang |
| Risiko tersisa | Impor produksi menunggu `DNH-G03`; 274 alokasi nomor kode berjalan di koneksi terpisah (nomor hangus bila impor batal) |
| Perubahan sampingan | Pengeluaran model paralel dari M-DNH-3 (bagian 5) |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | D-DNH-2/3 (pembersihan dan impor DB Hamzah, wewenang terpisah); `FE-RWI-251` |
