# Laporan Perubahan Backend — `BE-BD-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-015` |
| Judul | Kantong disimpan, dipindahkan, riwayatnya tak pernah ditimpa |
| Jalur | Jalur kritis `BE-BD-004` → **`BE-BD-015`** → `BE-BD-006` → `BE-BD-007` |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` revisi 9, blok task `BE-BD-015` |
| Trace | `DEC-BD-036`, `DEC-BD-037` · `BD-DOM-25` · `INV-BD-025`..`028` · `ARCH-BD-POS-04/05/06` · `contracts/api-contract.md` §Blood Unit dan §Blood Storage Location · `contracts/state-transition-matrix.md` §3 · `contracts/validation-matrix.md` §4b (`VAL-BD-060`..`064`, `VAL-BD-068`) · `contracts/permission-audit-matrix.md` §1 (`BloodUnit : Store`) · `data/data-dictionary.md` §`BbkBloodUnitPlacement`, §`BbkBloodUnit` · `testing/acceptance-test-matrix.md` §7 |
| Acceptance criteria | `AC-BD-060/061/063/066/067/068/069/070` · `AC-BD-062/065` (diteruskan dari `BE-BD-014`) · `AC-BD-023/032` (diteruskan dari `BE-BD-004`, roadmap revisi 9) |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` ✅ · `G2b` ✅ · `BE-BD-004` ✅ · `BE-BD-014` ✅ |
| Klasifikasi | `HEAVY` — entity baru dengan FK melingkar, migration, konkurensi, lintas master data |
| Task mode | `BACKEND` — dinyatakan eksplisit oleh pemilik pekerjaan |
| Target tulis | `NewQuilvianSystemBackend` — source `BE-BD-015`, migration, `Tests/**`, laporan, roadmap, traceability, `MODULE-STATUS` |
| Wewenang database | Migration dibuat dan diterapkan **hanya** ke `QuilvianNewDevSukma` — **dipakai**. Nama database diperiksa skrip penjaga sebelum apply; connection string tidak pernah dicetak |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `46de845` cabang `sukmagp` — "docs(bank-darah): unblock BE-BD-015 storage delivery" |
| Tanggal | `2026-09-11` |
| Status | 🟡 **`SELESAI SEBAGIAN`** — seluruh scope dan seluruh validasi teknis selesai. **9 dari 12 kriteria terbukti penuh.** `AC-BD-060`, `AC-BD-068`, dan `AC-BD-070` terbukti pada tingkat **gerbang alokasi**; bagian "dicoba **dialokasikan**" menuntut endpoint alokasi milik `BE-BD-006`, yang dilarang dibuat pada task ini. Lihat bagian 7 dan 9 |

---

## 1. Masalah yang diselesaikan

Sampai `BE-BD-004`, kantong darah yang diterima dari PMI berhenti di status `Received`: tercatat ada,
tetapi **tidak diketahui berada di kulkas mana**. Kantong seperti itu tidak boleh dialokasikan kepada
siapa pun (`INV-BD-025`), dan belum ada cara menaruhnya.

Task ini menambahkan jawaban **"kantong ini ada di mana, sejak kapan, dan siapa yang menaruhnya"**
dalam bentuk riwayat yang **hanya bertambah**.

**Contoh alur nyata.**

| Waktu | Kejadian | Yang tercatat |
| --- | --- | --- |
| Senin 08.00 | Kantong PRC `PMI-2026-0001` diterima | Status `Received`, belum punya lokasi |
| Senin 08.10 | Petugas A menaruhnya di Kulkas Besar | Penempatan #1. Status `Received` → `Stored` → `Available` |
| Selasa 13.00 | Kulkas Besar rusak dan dinonaktifkan | **Tidak ada yang berubah pada kantong.** Peringatan: "Ada 12 kantong yang masih tercatat di sana…" Gerbang alokasinya tertutup |
| Selasa 13.20 | Petugas B memindahkannya ke Kulkas Kecil | Penempatan #2 yang menunjuk #1. Status tetap `Available`. Gerbang terbuka kembali |
| Kapan pun | Riwayat dibuka | Penempatan #1 tetap terbaca: Kulkas Besar, Senin 08.10, Petugas A — walaupun kulkasnya kini nonaktif |

---

## 2. Proses bisnis dan status

### 2.1 Perpindahan status

| Dari | Tindakan | Ke | Pelaku | Baris riwayat status |
| --- | --- | --- | --- | --- |
| `Received` | Tetapkan lokasi pertama | `Stored` | Petugas Bank Darah | `Store` |
| `Stored` | Akibat penempatan — kantong biasa | `Available` | Sistem, pada detik yang sama | `MakeAvailable` |
| `Stored` | Akibat penempatan — kantong berlebih **atau** permintaan asalnya `ClosedEncounter` | `PendingReview` | Sistem, pada detik yang sama | `HoldForReview`, beserta sebabnya |
| `Stored`/`Available`/`Allocated`/`PendingReview`/`Reallocated` | Pindahkan lokasi | **tidak berubah** | Petugas Bank Darah | Tidak ada — yang bertambah adalah baris penempatan |
| `Stored`/`Available`/`Allocated` | Lokasinya dinonaktifkan | **tidak berubah** | Pengelola Setup | Tidak ada. Kantong tidak dipindahkan, tidak masuk `PendingReview` |

`Stored` **dilewati, bukan disinggahi.** Matriks §3 menetapkan `Stored` → `Available` sebagai akibat
penempatan tanpa tindakan manusia tambahan (`ARCH-BD-POS-04`). Kedua perpindahan dicatat pada
riwayat status, tetapi status yang **tersimpan** sesudah penempatan pertama selalu `Available` atau
`PendingReview`.

### 2.2 Gerbang alokasi dari sisi penyimpanan

`BbkBloodUnitService.EvaluateAllocationGateAsync` menjawab **satu** pertanyaan
(`02-backend-architecture.md` §F.4): kantong sudah melewati `Stored` **dan** penempatan terakhirnya
menunjuk lokasi yang sedang aktif?

| Keadaan kantong | Jawaban |
| --- | --- |
| Masih `Received` | Tertutup — `VAL-BD-063` |
| Tersimpan di lokasi nonaktif | Tertutup — `VAL-BD-064` |
| Tersimpan di lokasi aktif | Terbuka |

Gerbang ini **hanya membaca**. Keaktifan lokasi dibaca dari master pada detik ditanya, tidak pernah
disalin ke kantong (`INV-BD-028`). Pemakainya adalah endpoint `allocate` (`BE-BD-006`) dan `reallocate`
(`BE-BD-009`); keduanya belum ada.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · suite skill `1.18.0`: `rules/backend/*`, `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `transaction-endpoint-standard.md`, `role-access-rules.md`, `rules/rule-output/status-task-roadmap.md` |
| Kontrak modul | Kartu `BE-BD-015` roadmap revisi 9 · `requirement-traceability.md` · `acceptance-test-matrix.md` §2 dan §7 · `api-contract.md` §Blood Unit dan §Blood Storage Location · `state-transition-matrix.md` §3 · `validation-matrix.md` §4b · `permission-audit-matrix.md` §1–2 · `data-dictionary.md` §`BbkBloodUnit`, §`BbkBloodUnitPlacement` · `02-backend-architecture.md` §F.4, FK melingkar · `03-frontend-architecture.md` `FE-BD-05`, `FE-BD-011`, `FE-BD-014`, `FE-BD-015` · laporan `BE-BD-004` dan `BE-BD-014` |
| Source pembanding | `BbkBloodUnit*.cs` · `BbkProviderRequestService.cs` · `MstBloodStorageLocation.cs` · `BloodStorageLocationService.cs` dan controller-nya · test `ProviderRequestServiceTests` dan `ProviderRequestPostgresTests` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnitPlacement.cs` | **Baru.** 8 kolom persis kamus data |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitPlacementConfiguration.cs` | **Baru.** Index unik terfilter `IX_BbkBloodUnitPlacement_CurrentUnit`, index biasa, tiga FK `Restrict` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitStorageDtos.cs` | **Baru.** `AssignStorageLocationRequest`, `MoveStorageLocationRequest`, `BloodUnitPlacementDto` |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnit.cs` | `CurrentPlacementId` + navigasi |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitConfiguration.cs` | FK melingkar `Restrict` dan index biasa `CurrentPlacementId` |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | Penetapan pertama, perpindahan, riwayat penempatan, gerbang alokasi; daftar dan detail membawa lokasi saat ini |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | Tiga endpoint baru; `LoggerService` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | Lokasi saat ini pada daftar dan detail |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkBloodUnitStatus.cs` | Komentar saja |
| `Areas/HealthServices/MasterData/Services/BloodStorageLocationService.cs` | Jumlah kantong pada `VAL-BD-068`; penjaga hapus lokasi yang pernah dipakai |
| `Areas/HealthServices/MasterData/Controllers/BloodStorageLocationController.cs` | `InUse` → `422`; `HeldUnitCount` pada detail |
| `Areas/HealthServices/MasterData/DTOs/BloodStorageLocationDtos.cs` | `HeldUnitCount` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<BbkBloodUnitPlacement>` |
| `Migrations/20260911090848_AddBbkBloodUnitPlacement.cs` + `.Designer.cs` | **Baru.** Satu tabel, satu kolom, enam index, empat FK `Restrict` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bangkitan EF |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/BloodUnitStorage/BloodUnitStorageServiceTests.cs` + `.Endpoints.cs` | **Baru.** 43 test |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/BankDarah/BloodUnitStoragePostgresTests.cs` | **Baru.** 6 uji PostgreSQL |
| `Tests/.../MasterData/BloodBankRoleAccessContractTests.cs` | Cakupan butir hak akses **28 → 29** dari 39 |
| `Tests/.../ProviderRequest/ProviderRequestServiceTests.cs` + `.Endpoints.cs` | Tiga test `BE-BD-004` menyesuaikan kemampuan baru — lihat bagian 6.3 |
| `docs/.../task/report/backend/BE-BD-015.md` | Laporan ini |
| `docs/.../roadmap/backend-roadmap.md` · `requirement-traceability.md` · `frontend-roadmap.md` · `MODULE-STATUS.md` | Tanda status dan tautan bukti |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint baru persis kontrak `v4`. Dua perilaku endpoint master lokasi berubah — bagian 8 |
| Database | Tabel baru `public."BbkBloodUnitPlacement"`; kolom baru `BbkBloodUnit."CurrentPlacementId"`. Diterapkan ke `QuilvianNewDevSukma` saja |
| Keamanan/Auth | Satu butir baru `BloodUnit : Store`, terpisah dari `Allocate`. Nol hardcode peran. Pelaku dari klaim login |
| Batas | Nol endpoint alokasi, nol background job, nol pemindahan massal |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` (dan master `MstBloodStorageLocation` milik Bank Darah di `MasterData`) |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → prefix **`Bbk`** |
| Status registry | **`ACTIVE`** |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-ENT-001/003` · `QBE-NAM-001/002/004` · `QBE-CFG-001` · `QBE-MOD-001/002/003` · `QBE-SVC-001` · `QBE-API-001` · `QBE-PERM-001` · `QBE-LOG-001` · `QBE-DTO-001` · `QBE-VAL-001` · `QBE-TXN-001` |
| Pengecualian QBE | `NONE` |
| Preflight | **Lolos** |

---

## 4. Dokumentasi endpoint

Grup Swagger **Health Services / Blood Bank Management / Blood Unit**. Base URL
`api/v1/health-services/blood-bank-management/blood-units`. Seluruh endpoint menuntut login.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/placements` | Riwayat penempatan: di kulkas mana, sejak kapan, oleh siapa | `BloodUnit : Read` |
| `POST` | `/{id}/storage-location` | Tetapkan lokasi pertama: `Received` → `Stored` → `Available`/`PendingReview` | `BloodUnit : Store` |
| `PUT` | `/{id}/storage-location` | Pindahkan ke lokasi lain; status tidak berubah | `BloodUnit : Store` |

### `POST /{id}/storage-location`

```json
{ "storageLocationId": "8b1c…", "note": "Rak atas", "version": 0 }
```

`note` opsional, paling banyak 500 karakter, **bukan** alasan terkendali. `version` opsional — token
kantong yang dipegang layar.

| HTTP | Kapan | Pesan |
| --- | --- | --- |
| `200` | Tersimpan | "Kantong berhasil disimpan dan kini tersedia." atau "…masuk daftar menunggu keputusan." + detail kantong |
| `400` | Lokasi kosong, tidak ada, atau dihapus; keterangan terlalu panjang; akun login tidak dikenali | Kalimat masing-masing |
| `404` | Kantong tidak ada | "Kantong darah tidak ditemukan atau sudah dihapus." |
| `409` | Kantong berubah di tangan orang lain | "Kantong ini baru saja diubah petugas lain. Muat ulang lalu ulangi tindakan ini." |
| `422` `VAL-BD-060` | Lokasi nonaktif | "Lokasi penyimpanan itu sudah tidak aktif dan tidak dapat dipilih. Pilih lokasi lain yang masih aktif." |
| `422` `VAL-BD-061` | Sudah pernah ditempatkan | "Kantong ini sudah punya lokasi penyimpanan. Gunakan perpindahan lokasi bila ingin memindahkannya." |
| `422` | Tidak ada satu pun lokasi aktif | "Belum ada lokasi penyimpanan darah yang aktif. Tambahkan atau aktifkan minimal satu lokasi pada Setup Bank Darah sebelum kantong dapat disimpan." |

### `PUT /{id}/storage-location`

Body sama bentuknya (`MoveStorageLocationRequest`). Tetap berlaku ketika lokasi **asal** sudah
nonaktif — inilah jalan keluar kantong dari kulkas yang rusak.

| HTTP | Kapan | Pesan |
| --- | --- | --- |
| `200` | Dipindahkan | "Kantong berhasil dipindahkan. Status kantong tidak berubah." + detail |
| `400`/`404`/`409` | Sama dengan `POST` | — |
| `422` `VAL-BD-060` | Lokasi tujuan nonaktif | Pesan `VAL-BD-060` |
| `422` `VAL-BD-062` | Belum pernah ditempatkan | "Kantong ini belum punya lokasi penyimpanan. Tetapkan lokasinya lebih dulu." |
| `422` | Kantong sudah `Issued`, `ReturnedToProvider`, atau `NotUsable` | "Kantong berstatus … sudah keluar dari stok dan tidak dapat dipindahkan." |

### `GET /{id}/placements`

```json
{
  "success": true,
  "message": "Riwayat penempatan kantong darah berhasil diambil.",
  "data": [
    { "storageLocationCode": "KLK-BSR", "storageLocationName": "Kulkas Besar", "isStorageLocationActive": false,
      "previousPlacementId": null, "placedAt": "2026-09-14T01:10:00Z", "placedByUserId": "…", "isCurrent": false },
    { "storageLocationCode": "KLK-KCL", "storageLocationName": "Kulkas Kecil", "isStorageLocationActive": true,
      "previousPlacementId": "…", "previousStorageLocationCode": "KLK-BSR",
      "placedAt": "2026-09-15T06:20:00Z", "placedByUserId": "…", "isCurrent": true, "note": "Kulkas Besar rusak" }
  ]
}
```

Daftar kosong berarti "Kantong belum pernah disimpan." `404` bila kantong tidak ada.

### Perubahan pada endpoint yang sudah ada

| Endpoint | Perubahan |
| --- | --- |
| `GET /blood-units`, `GET /blood-units/{id}` | Membawa `currentStorageLocationId/Code/Name` dan `isCurrentStorageLocationActive` (penanda kantong tertahan, `FE-BD-011`); detail membawa `currentPlacedAt`. `availableActions`: `AssignStorageLocation` selama `Received`, `MoveStorageLocation` sesudahnya |
| `PATCH /blood-storage-locations/{id}/status` dan `PUT /{id}` yang menonaktifkan | Pesan `200` `VAL-BD-068` menyebut jumlah: "Lokasi dinonaktifkan. Ada N kantong yang masih tercatat di sana dan belum dapat dialokasikan sampai dipindahkan ke lokasi aktif." |
| `GET /blood-storage-locations/{id}` | Membawa `heldUnitCount` — jumlah kantong yang akan tertahan bila lokasi dinonaktifkan (`FE-BD-015`) |
| `DELETE /blood-storage-locations/{id}` | **`422`** bila lokasi pernah dipakai menyimpan kantong |

Tidak ada respons yang memuat stack trace, SQL, connection string, maupun detail exception. Nomor
kantong PMI tidak ditulis ke log.

---

## 5. Hanya-tambah dan konkurensi

| Penjaga | Cara kerja |
| --- | --- |
| **Riwayat hanya bertambah** (`INV-BD-026`) | Perpindahan menambah baris baru yang menunjuk baris lama lewat `PreviousPlacementId`. Baris lama hanya berubah `IsCurrent` → `false`. Service tidak punya method ubah/hapus penempatan; controller tidak punya `DELETE`/`PATCH` |
| **Satu penempatan berlaku** | Index unik terfilter `(BloodUnitId) WHERE "IsCurrent" = true` di database |
| **Penunjuk tidak pernah disunting sendiri** (`ARCH-BD-POS-05`) | `CurrentPlacementId` hanya berpindah bersama penambahan penempatan, dalam transaksi yang sama |
| **Perebutan** | Token `Version` kantong. Perpindahan berjalan dalam satu transaksi dua langkah: padamkan penempatan lama + naikkan `Version`, lalu tambah penempatan baru. Dua petugas berebut baris kantong yang sama; yang kalah mendapati `Version` sudah berubah, menerima `409`, dan transaksinya batal seluruhnya |
| **Penonaktifan tanpa job** (`DEC-BD-037`) | Satu `UPDATE` pada satu baris master; jumlah kantong dihitung dengan satu `SELECT COUNT`. Nol penyuntingan kantong, nol job |

---

## 6. Verifikasi

### 6.1 Perintah

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | `0 Error(s)` | `PASS` |
| Build `Tests`, `IntegrationTests.Postgres`, `UnitTests.Sqlite`, `UnitTests.InMemory` dengan `-p:RunAnalyzers=false` | Keempatnya `0 Error(s)` | `PASS` |
| `dotnet ef migrations add AddBbkBloodUnitPlacement` | Satu tabel, satu kolom, index unik terfilter, empat FK `Restrict` — diperiksa baris per baris | `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| Skrip penjaga sebelum apply | `QuilvianNewDevSukma`; 141 migration, 140 applied, tepat 1 tertunda = migration task ini | `PASS` |
| Skrip penjaga `-Apply` | `QuilvianNewDevSukma` — **141 applied, 0 pending** | `PASS` |
| `dotnet test QuilvianSystemBackend.Tests` | **643/643** — naik dari 600; Bank Darah 358/358; penyimpanan 43/43 | `PASS` |
| Uji PostgreSQL Bank Darah di `QuilvianNewDevSukma` | **19/19** — 13 lama + **6 penyimpanan** | `PASS` |
| `dotnet test UnitTests.Sqlite` | **231/231** | `PASS` |
| `dotnet test UnitTests.InMemory` | 896/905 — **9 gagal, seluruhnya Billing**, sama dengan baseline sebelum task ini | `PRE-EXISTING` |

Build dijalankan dengan `-p:RunAnalyzers=false` atas instruksi pemilik, sehingga jumlah warning tidak
dibandingkan dengan baseline `210 Warning(s)` yang diukur dengan analyzer aktif.

### 6.2 Dua koreksi selama pengerjaan

| Temuan | Tindakan |
| --- | --- |
| Migration pertama membuat `IX_BbkBloodUnit_CurrentPlacementId` **unik** — konvensi EF pada FK melingkar — padahal kamus data menulis index biasa | Ditemukan saat migration diperiksa, **sebelum** apply. Index dinyatakan `IsUnique(false)`, dua berkas migration dihapus, snapshot dikembalikan ke `HEAD`, lalu migration dibangkitkan ulang. Yang diterapkan hanya versi yang benar |
| Test "master kosong" gagal | Kesalahan test: hanya dua dari tiga lokasi aktif yang dinonaktifkan. Test diperbaiki; nol perubahan source |

### 6.3 Test `BE-BD-004` yang ikut menyesuaikan

| Test | Semula | Kini | Sebab |
| --- | --- | --- | --- |
| `AC_BD_059_…` | Aksi kantong `Received` kosong | Tepat `AssignStorageLocation`, dan gerbang alokasi tertutup `VAL-BD-063` | Penyimpanan kini ada. Makna `AC-BD-059` — belum dapat dialokasikan — kini dibuktikan lebih kuat |
| `KolomTersimpan_…` | 12 kolom `BbkBloodUnit` | 13, termasuk `CurrentPlacementId` | Kolom kamus data yang memang ditunda ke task ini |
| `Controller_BerbentukTransaksi_…` | 5 endpoint kantong, nol `PUT` | 8 endpoint; `PUT` hanya `{id:guid}/storage-location` | Kontrak `v4` memang punya `PUT /{id}/storage-location` |

---

## 7. Acceptance criteria dan Definition of Done

### 7.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-061` — lokasi ditetapkan pada kantong `Received` → `Stored` lalu `Available`; satu riwayat bertambah | ✅ **Terbukti** | `AC_BD_061_…` — satu penempatan (`PreviousPlacementId` kosong, pelaku, keterangan), `CurrentPlacementId`, `Version` 1, riwayat `Receive` → `Store` → `MakeAvailable` |
| `AC-BD-062`, `AC-BD-065` — lokasi nonaktif untuk penyimpanan baru → `VAL-BD-060` | ✅ **Terbukti** | `AC_BD_062_AC_BD_065_…` — kantong tetap `Received`, nol penempatan. HTTP: `Controller_JalurGagal_…` |
| `AC-BD-063` — kantong tersimpan dipindahkan → riwayat bertambah, status tetap, penerimaan awal tak tersentuh | ✅ **Terbukti** | `AC_BD_063_…` — penempatan lama tetap (lokasi, waktu, pelaku sama; `IsCurrent` `false`), yang baru menunjuknya; status `Available`; baris penerimaan dan seluruh riwayat status identik |
| `AC-BD-066` — lokasi nonaktif sebagai tujuan perpindahan → `VAL-BD-060` | ✅ **Terbukti** | `AC_BD_066_…` — penempatan dan `Version` tidak bergerak |
| `AC-BD-067` — lokasi berisi kantong dinonaktifkan → berhasil, kantong tetap di sana, peringatan menyebut jumlah | ✅ **Terbukti** | `AC_BD_067_…` — "Ada 2 kantong…", kantong berstatus akhir tidak terhitung, potret kantong identik. PostgreSQL: `PenonaktifanLokasi_MenyebutJumlah_…`. Detail lokasi menyebut jumlah **sebelum** penonaktifan: `ControllerLokasi_DetailMenyebutJumlahKantongYangAkanTertahan` |
| `AC-BD-069` — sistem tidak memindahkan kantong sendiri | ✅ **Terbukti** | `AC_BD_069_…` — lewat `PATCH` maupun `PUT`: potret kantong, penempatan, dan jumlah riwayat identik; nol `IHostedService` di Bank Darah dan master lokasi. PostgreSQL: potret identik |
| `AC-BD-023` — kantong sesudah `ClosedEncounter` → disimpan lalu `PendingReview` | ✅ **Terbukti penuh** | `AC_BD_023_…` — dari penerimaan susulan sampai `PendingReview`; asal permintaan dan statusnya utuh; riwayat `HoldForReview` dengan sebabnya; muncul di daftar `PendingReview` hanya **sesudah** disimpan. Bagian penerimaan sudah dibuktikan `BE-BD-004` |
| `AC-BD-032` — kantong ke-3 → `PendingReview` + alasan, muncul di daftar #2 | ✅ **Terbukti penuh** | `AC_BD_032_…` — dua kantong `Available`, kantong berlebih `PendingReview` beralasan "Kiriman melebihi permintaan.", satu-satunya baris daftar `unitStatus=PendingReview`. HTTP: `Controller_DaftarKerjaDua_…` |
| `AC-BD-060` — kantong `Received` dicoba dialokasikan → `VAL-BD-063` | 🟡 **Tingkat gerbang** | `AC_BD_060_…` — gerbang tertutup dengan kode dan pesan kanonis; aksi alokasi tidak ditawarkan. **Endpoint `allocate` belum ada** — milik `BE-BD-006`, yang juga mencantumkan `AC-BD-060` |
| `AC-BD-068` — kantong di lokasi nonaktif dicoba dialokasikan → `VAL-BD-064` | 🟡 **Tingkat gerbang** | `AC_BD_068_…` dan PostgreSQL — gerbang tertutup `VAL-BD-064`. Endpoint `allocate` milik `BE-BD-006`, yang juga mencantumkan `AC-BD-068` |
| `AC-BD-070` — dipindahkan dari lokasi nonaktif ke aktif lalu dialokasikan → berhasil, pelaku & waktu tercatat, gerbang terbuka | 🟡 **Tingkat gerbang** | `AC_BD_070_…` dan PostgreSQL — perpindahan berhasil, pelaku dan waktu tercatat, lokasi asal tetap terbaca, gerbang **terbuka kembali**. Langkah "lalu mengalokasikan → berhasil" menuntut endpoint `allocate` `BE-BD-006` |

**Skenario risiko matriks §7 di luar `AC-BD-*`:**

| Skenario | Bukti |
| --- | --- |
| Dua petugas memindahkan kantong yang sama ke dua lokasi berbeda hampir bersamaan | PostgreSQL `DuaPetugasMemindahkanBersamaan…` — tepat satu `Success`, satu `409`; dua penempatan, tepat satu berlaku. Ditambah sepuluh perpindahan serentak tanpa token (rantai utuh) dan dua penetapan pertama serentak (satu penempatan). InMemory: `DuaPetugasMemindahkanBersamaan_YangKeduaDitolak409_…` |
| Master kosong → kantong berhenti di `Received`, pesan mengarahkan ke Setup | `TanpaSatuPunLokasiAktif_…` |
| Lokasi dinonaktifkan lalu diaktifkan kembali → gerbang terbuka tanpa satu kantong pun disunting | `LokasiDinonaktifkanLaluDiaktifkanKembali_…` |
| Perpindahan kantong `Allocated` → alokasi, pasien, bukti utuh | `PerpindahanKantongAllocated_…` — hanya `CurrentPlacementId`, `Version`, dan kolom audit yang berubah. Status disetel langsung karena alokasi lahir pada `BE-BD-006` |
| Percobaan mengubah atau menghapus riwayat penempatan | `RiwayatPenempatan_TidakPunyaJalurUbahAtauHapus` · `Controller_DelapanEndpoint_…` · index unik terfilter fisik di PostgreSQL |
| Penonaktifan pada 500 kantong sekaligus | **Terbukti secara struktur**, tidak diuji beban: penonaktifan menulis satu baris master dan menghitung dengan satu `SELECT`; nol baris kantong disunting pada jalur mana pun |

**Hak akses dan pelaku:** `Endpoint_MemakaiRouteDanButirHakAksesYangDikunciKontrak` (tiga endpoint),
`BloodBankRoleAccessContractTests` (29 dari 39), `Controller_TanpaKlaimPengguna_…`.

### 7.2 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Seluruh scope implementasi | **Terpenuhi** |
| `BbkBloodUnitPlacement` dan `CurrentPlacementId` sesuai desain | **Terpenuhi** |
| Riwayat hanya-tambah terbukti; tepat satu penempatan berlaku dijaga | **Terpenuhi** — termasuk di PostgreSQL |
| Penetapan pertama, perpindahan, penolakan lokasi nonaktif, penonaktifan tanpa pemindahan | **Terpenuhi** |
| `AC-BD-023`, `AC-BD-032` terbukti penuh | **Terpenuhi** |
| Seluruh `AC-BD-060/061/062/063/065/066/067/068/069/070` terbukti | **Belum terpenuhi** — 7 dari 10 penuh; `060`/`068`/`070` pada tingkat gerbang |
| Konkurensi, otorisasi, jalur gagal | **Terpenuhi** |
| Build, test, PostgreSQL | **Terpenuhi** |
| Migration hanya ke `QuilvianNewDevSukma`, 0 pending, `has-pending-model-changes` bersih | **Terpenuhi** |
| Laporan, roadmap, traceability, `MODULE-STATUS` | **Terpenuhi** |

---

## 8. Delta kontrak dan tafsiran

| No | Hal | Yang dijalankan | Butuh keputusan? |
| ---: | --- | --- | --- |
| 1 | `GET /blood-storage-locations/{id}` membawa `heldUnitCount` | Angka dihitung saat dibaca, supaya konfirmasi penonaktifan menyebutnya lebih dulu (`FE-BD-015`). Traceability menitipkan angka ini ke `BE-BD-015` | Tidak |
| 2 | `DELETE /blood-storage-locations/{id}` menolak `422` bila lokasi pernah dipakai | Turunan api-contract "Lokasi tidak dapat dihapus; hanya dinonaktifkan… riwayat kantong wajib tetap terbaca". `BE-BD-014` menunda pemeriksaan pemakaian ke task ini | Tidak |
| 3 | Penolakan master kosong (`INV-BD-025`) | `422` tanpa kode `VAL-BD-*`; kalimat diturunkan dari `FE-BD-014` | Tidak — kode resmi dapat ditambahkan `design-business-module` |
| 4 | Penolakan perpindahan kantong berstatus akhir | `422` tanpa kode; matriks §3 menyebut syaratnya tanpa kode | Tidak |
| 5 | Riwayat status penempatan pertama | Dua baris: `Store` (`Received` → `Stored`) dan `MakeAvailable`/`HoldForReview` (`Stored` → akhir). Perpindahan tidak menambah riwayat status — baris penempatannya adalah auditnya | Tidak |
| 6 | Lokasi saat ini pada daftar dan detail kantong; `availableActions` baru | Permukaan teknis untuk `FE-BD-011` dan `FE-BD-05` | Tidak |
| 7 | `EvaluateAllocationGateAsync` | Gerbang baca-saja sesuai arsitektur §F.4, tanpa endpoint | Tidak |
| 8 | Jumlah kantong tertahan `VAL-BD-068` | Menghitung kantong `Stored`/`Available`/`Allocated`/`PendingReview`/`Reallocated` yang penempatan berlakunya di lokasi itu; status akhir tidak terhitung karena sudah keluar | Tidak |
| 9 | Perpindahan ke lokasi yang sama | **Tidak ditolak** — kontrak tidak melarang, dan menolaknya berarti mengarang aturan. Hasilnya satu baris riwayat tambahan | Tidak — dicatat supaya disadari |
| 10 | Token `version` opsional pada request | Pola `BE-BD-004`: layar usang ditolak `409` | Tidak |

---

## 9. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Bagian "dicoba dialokasikan" pada `AC-BD-060/068/070` menunggu endpoint `allocate` `BE-BD-006`. Karena `BE-BD-006` bergantung pada `BE-BD-015` ✅, keduanya saling menunggu sampai pemilik roadmap memutuskan — sama persis dengan keadaan `BE-BD-004` sebelum revisi 9 |
| Risiko tersisa | **(1)** Penolakan index unik terfilter dipetakan ke `409` berdasarkan nama constraint; bila nama index diubah kelak, pemetaannya ikut diperbarui (`BbkBloodUnitPlacementConfiguration.CurrentUnitIndexName`). **(2)** Endpoint alokasi `BE-BD-006` wajib memanggil `EvaluateAllocationGateAsync` — bila tidak, gerbang ini tidak menjaga apa pun |
| Temuan di luar scope | **(1)** `FE-BD-011` belum memakai `heldUnitCount`, sehingga `FE-BD-015` masih belum terpenuhi di layar. **(2)** `FE-BD-015` melarang tombol hapus lokasi di layar, sementara `DELETE` master masih ada dari `BE-BD-014`; kini ia menolak lokasi yang pernah dipakai. **(3)** `UnitTests.InMemory` punya 9 kegagalan Billing baseline — milik BillingManagement |
| Perubahan sampingan | `NONE` — snapshot migration adalah bangkitan EF milik migration task ini |
| Interupsi | `NONE` |
| Status Git | Seluruh perubahan pada bagian 3.2 belum di-stage, belum di-commit. Nol push |
| Langkah berikutnya | Keputusan pemilik roadmap: teruskan bagian alokasi `AC-BD-060/068/070` ke `BE-BD-006` (`060`/`068` sudah tercantum di sana; `070` perlu ditambahkan). Sesudahnya `BE-BD-015` sah ✅ dan `BE-BD-006` terbuka |
