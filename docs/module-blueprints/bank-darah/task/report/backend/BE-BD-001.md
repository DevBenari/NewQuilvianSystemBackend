# Laporan Perubahan Backend — `BE-BD-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-001` |
| Judul | Katalog komponen darah & daftar alasan terkendali dapat dikelola |
| Slice | `MVP-0` — fondasi master Bank Darah |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/00-delivery-plan.md` §D.1 |
| Trace | `DEC-BD-024`, `DEC-BD-032`, `BD-DOM-13` · `contracts/api-contract.md` §Blood Component · `contracts/validation-matrix.md` (`VAL-BD-020b`) · `data/data-dictionary.md` §`MstBloodComponent` · `INV-BD-023` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` approval ✅ tertutup · `G2a`/`G2b` registry ✅ tertutup. Tidak ada task pendahulu |
| Klasifikasi | `MEDIUM` — satu entity master baru, sembilan endpoint, satu migration, satu seeder, nol perubahan pada modul lain |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/MasterData/**`, `Repositories/**`, `Migrations/**`, `QuilvianSystemBackend.Tests/**`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6488511` cabang `sukmagp` |
| Tanggal | `2026-09-03` |
| Status | **`SELESAI SEBAGIAN`** — hanya `MstBloodComponent`. `MstBloodBankReason` belum dikerjakan; lihat bagian 6 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, Quilvian **tidak punya satu pun katalog komponen darah**. Akibatnya nyata
dan berantai: petugas Bank Darah tidak dapat membuat order darah, karena order darah menyebut
komponen yang diminta; dan sistem tidak dapat mendeteksi order ganda, karena pendeteksiannya
membandingkan pasien, kunjungan, dan **komponen** sekaligus (`DEC-BD-005`).

Ada akibat kedua yang lebih halus dan lebih berbahaya. Gerbang pemberian darah menghitung masa
berlaku bukti pemeriksaan kecocokan dari konfigurasi **per komponen** (`DEC-BD-032`). Tanpa
katalog, angka itu tidak punya tempat tinggal, dan satu-satunya cara membuatnya bekerja adalah
menanamnya di kode — persis yang dilarang `INV-BD-023`.

**Contoh.** Ny. R membutuhkan transfusi PRC. Tanpa katalog, petugas tidak dapat memilih komponen
apa pun pada layar order, sehingga permintaannya berhenti sebelum dimulai. Seandainya order tetap
dipaksa jalan dengan komponen ketikan bebas, dua order PRC untuk pasien dan kunjungan yang sama
tidak akan terbaca sebagai order ganda, dan darah dapat diminta dua kali ke PMI.

---

## 2. Proses bisnis

**Tujuan.** Memberi BDRS satu tempat resmi untuk mendaftarkan komponen darah yang mereka layani,
beserta aturan masa berlaku bukti kecocokan masing-masing.

**Pelaku.** Admin master data Bank Darah, lewat butir hak akses `BloodComponent : *`.

**Pemicu.** Penyiapan modul Bank Darah sebelum order darah pertama dibuat.

**Langkah pada jalur normal:**

1. Admin membuka layar katalog komponen darah. Layar memanggil `GET /filters/metadata` untuk
   memperoleh pilihan penyaring dan susunan isian form, lalu `GET /summary` untuk kartu statistik,
   lalu `GET /` untuk tabel utamanya.
2. Admin menekan Tambah, mengisi kode dan nama komponen, lalu menyimpan lewat `POST /`. Kode
   disimpan dalam huruf besar dan spasi di ujungnya dibuang, sehingga `" prc "` dan `"PRC"`
   diperlakukan sebagai satu kode yang sama.
3. Bila masa berlaku bukti kecocokan sudah ditetapkan kebijakan klinis, admin mengisinya dalam
   satuan jam. Bila belum, kolom itu **dibiarkan kosong** — dan itu keadaan yang sah.
4. Komponen yang sudah tidak dilayani dinonaktifkan lewat `PATCH /{id}/status`, atau ditandai
   terhapus lewat `DELETE /{id}`.

**Aturan yang berlaku:**

| Aturan | Perilakunya |
| --- | --- |
| Kode komponen tunggal | Kode kembar ditolak `409`, termasuk yang hanya berbeda huruf besar-kecil. Index unik pada `ComponentCode` menjadi penjaga terakhir ketika dua petugas menyimpan hampir bersamaan |
| Masa berlaku boleh kosong | Kekosongan **bukan** kelonggaran, melainkan gerbang tertutup: pemberian komponen itu ditolak `VAL-BD-020b` sampai angkanya diisi |
| Masa berlaku tidak pernah ditebak | Sistem tidak mengisi angka bawaan apa pun, termasuk lewat seeder (`INV-BD-023`) |
| Nol jam ditolak | Nol atau negatif akan membuat bukti kedaluwarsa seketika dan menutup pemberian secara diam-diam. Ditolak `400` sebagai kesalahan isian supaya sebabnya terbaca |
| Penghapusan adalah penandaan | `IsDelete` menjadi benar dan `IsActive` menjadi salah. Baris tidak pernah dihapus fisik, sehingga order darah lama yang menyebutnya tetap terbaca |

**Contoh berangka.** PRC dikonfigurasi 72 jam, TC 24 jam. Bukti kecocokan PRC yang dicatat Senin
pukul 08.00 masih membuka gerbang sampai Kamis pukul 08.00. Bukti TC pada jam yang sama berhenti
berlaku Selasa pukul 08.00. Keduanya dibaca dari katalog, bukan dari angka di dalam kode.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Kode sudah dipakai komponen lain | `409` — "Kode komponen PRC sudah dipakai komponen darah lain." |
| Kode atau nama kosong | `400` dengan sebab yang disebut |
| Masa berlaku diisi nol atau negatif | `400` — "Masa berlaku bukti kecocokan harus lebih dari nol jam." |
| Komponen tidak ada atau sudah dihapus | `404` — "Komponen darah tidak ditemukan atau sudah dihapus." |
| Katalog masih kosong saat `GET /options` dipanggil | `200` dengan daftar kosong, disertai keterangan bahwa order darah tidak dapat dibuat selama katalog kosong |

**Hasil akhir.** Katalog komponen darah dapat dikelola penuh tanpa perubahan kode, dan jumlah
komponen yang masa berlakunya belum ditetapkan terbaca langsung di halaman index.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` | Menetapkan QBE ID yang berlaku untuk `NEW CODE` |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Memastikan prefix `Mst` terdaftar dan berstatus `ACTIVE` |
| `rules/backend/master-data-endpoint-standard.md` | Menetapkan sembilan endpoint baseline |
| `rules/backend/role-access-rules.md` | Menetapkan pasangan `[AccessAction]`/`[AccessPermission]` |
| `AGENTS.md` | Mode task, wewenang tulis, dan urutan presedensi |
| `docs/module-blueprints/bank-darah/data/data-dictionary.md` §`MstBloodComponent` | Kontrak kolom dan DDL |
| `docs/module-blueprints/bank-darah/contracts/api-contract.md` §Blood Component | Kontrak endpoint dan hak akses |
| `docs/module-blueprints/bank-darah/02-backend-architecture.md` | Lokasi berkas dan rencana data master awal |
| `Areas/HealthServices/MasterData/Controllers/MedicalRecordAccessPurposeController.cs` beserta service, model, DTO, dan configuration-nya | Pola rumah untuk master data berbasis service — satu-satunya rujukan `NEW CODE` yang tidak memakai `ApplicationDbContext` langsung di controller |
| `Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs` | Bentuk DTO metadata penyaring dan ringkasan |
| `Areas/HealthServices/MasterData/Seeders/InpatientMasterDataSeeder.cs` | Pola seeder yang menolak berjalan di produksi |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Models/MstBloodComponent.cs` | **Baru.** Entity katalog komponen darah, mewarisi `IdentityModel` |
| `Repositories/Configurations/HealthServices/MasterData/MstBloodComponentConfiguration.cs` | **Baru.** Mapping tabel, index unik `ComponentCode`, index pencarian `IsActive` + `ComponentName` |
| `Areas/HealthServices/MasterData/DTOs/BloodComponentDtos.cs` | **Baru.** Response, option, create/update, status, ringkasan, dan metadata penyaring |
| `Areas/HealthServices/MasterData/Services/BloodComponentService.cs` | **Baru.** Pemilik seluruh pembacaan dan perubahan katalog |
| `Areas/HealthServices/MasterData/Controllers/BloodComponentController.cs` | **Baru.** Sembilan endpoint master data |
| `Areas/HealthServices/MasterData/Seeders/BloodComponentSeeder.cs` | **Baru.** Katalog minimum PRC, TC, FFP |
| `Repositories/ApplicationDbContext.cs` | Menambah `DbSet<MstBloodComponent> MstBloodComponents` pada region baru `BLOOD BANK MANAGEMENT` |
| `Program.cs` | Mendaftarkan `BloodComponentService` sebagai `Scoped` |
| `Migrations/20260903044753_AddMstBloodComponent.cs` beserta `.Designer.cs` | **Baru.** Migration pembuatan tabel |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis oleh `dotnet ef` |
| `QuilvianSystemBackend.Tests/HealthServices/MasterData/BloodComponentServiceTests.cs` | **Baru.** 26 pengujian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sembilan endpoint baru pada base URL `api/v1/health-services/master-data/blood-components`. Kontrak `v4` menuliskan enam; tiga sisanya adalah **delta** yang dituntut standar endpoint master data — lihat bagian 4 |
| Database | Satu tabel baru `public."MstBloodComponent"`, satu index unik, satu index pencarian. **Migration sudah dibuat tetapi BELUM dijalankan.** Eksekusi database adalah wewenang terpisah dan tidak diminta pada task ini |
| Keamanan/Auth | Empat butir hak akses baru pada resource `BloodComponent`: `Read`, `Create`, `Update`, `Delete`. Seluruh action memakai `[AccessAction]` dan `[AccessPermission]`; argumen pertama `[AccessPermission]` sama persis dengan `ControllerName` pada `[AccessController]`, yaitu `BloodComponent`. Tidak ada peran, departemen, jabatan, maupun `UserType` yang ditanam di kode |

---

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Blood Component

Base URL: `api/v1/health-services/master-data/blood-components`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi penyaring, pengurutan, dan isian form untuk halaman katalog | `BloodComponent : Read` |
| `GET` | `/summary` | Mengambil ringkasan jumlah, termasuk jumlah komponen aktif yang masa berlakunya belum ditetapkan | `BloodComponent : Read` |
| `GET` | `/` | Mengambil daftar komponen dengan pencarian, penyaringan, pengurutan, dan halaman | `BloodComponent : Read` |
| `GET` | `/options` | Mengambil pilihan komponen aktif untuk kotak isian layar lain | `BloodComponent : Read` |
| `GET` | `/{id}` | Mengambil detail satu komponen | `BloodComponent : Read` |
| `POST` | `/` | Menambah komponen baru | `BloodComponent : Create` |
| `PUT` | `/{id}` | Mengubah seluruh field bisnis satu komponen | `BloodComponent : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan komponen | `BloodComponent : Update` |
| `DELETE` | `/{id}` | Menandai komponen terhapus tanpa menghapus fisik | `BloodComponent : Delete` |

Kode status yang berlaku: `200` berhasil · `400` isian tidak lengkap atau tidak masuk akal ·
`401` belum masuk · `403` tidak berhak · `404` tidak ditemukan atau sudah dihapus ·
`409` kode komponen sudah dipakai.

**Tiga delta terhadap kontrak `v4`, beserta alasannya:**

| Endpoint | Sebab ditambahkan |
| --- | --- |
| `GET /filters/metadata` | Baseline wajib standar endpoint master data. Tanpa ini layar index tidak punya sumber pilihan penyaring dan susunan form, sehingga frontend akan menanamnya sendiri |
| `GET /summary` | Baseline wajib. Dipakai kartu statistik, dan khusus katalog ini ia membawa angka komponen aktif yang masa berlakunya belum ditetapkan — keadaan yang menahan pemberian darah dan sebaiknya terlihat sebelum ada pasien menunggu |
| `PATCH /{id}/status` | Baseline wajib menurut source: 26 dari 34 controller master data memilikinya. Menonaktifkan komponen tanpa mengirim seluruh body update |

**Satu delta penamaan.** Blueprint `02-backend-architecture.md` menuliskan
`MstBloodComponentController` dan `MstBloodComponentService`. Berkasnya dibuat sebagai
`BloodComponentController` dan `BloodComponentService`, mengikuti **29 dari 29** controller master
data yang ada, yang seluruhnya tidak memakai prefix `Mst` pada controller maupun service. Alasan
kedua bersifat mengikat: `[AccessPermission]` argumen pertama wajib sama persis dengan
`ControllerName` pada `[AccessController]`, dan kontrak `v4` menetapkan resource-nya `BloodComponent`
— bukan `MstBloodComponent`. Nama entity, configuration, DbSet, dan tabel **tetap** `MstBloodComponent`
sebagaimana dituntut kontrak engineering.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil — `0 Error(s)`, `186 Warning(s)` | `PASS` | Seluruh 186 warning pre-existing bergaya `CS1573`/`CS1574`/`CS1587` pada berkas lain. Penyaringan keluaran build terhadap kata `BloodComponent` mengembalikan **nol baris** |
| `dotnet ef migrations add AddMstBloodComponent` | Migration terbentuk | `PASS` | `Migrations/20260903044753_AddMstBloodComponent.cs` |
| Migration diadu dengan DDL kamus data | Cocok penuh | `PASS` | Kolom, tipe, panjang, nullability, PK, dan index unik `ComponentCode` sama persis dengan `data/data-dictionary.md` baris 458–466 |
| 26 pengujian `BloodComponentServiceTests` | `Failed: 0, Passed: 26` | `PASS` | Dijalankan lewat project verifikasi sementara — lihat catatan di bawah |
| `dotnet test QuilvianSystemBackend.Tests` | **Tidak dapat dijalankan** | `EXISTING / ENVIRONMENT ISSUE` | Test project **gagal dikompilasi sebelum perubahan ini**; lihat temuan di bawah |

### Temuan: test project sudah rusak sebelum task ini

`QuilvianSystemBackend.Tests` tidak dapat dikompilasi pada `HEAD` karena kekeliruan pada
`QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/PatientEncounterTestWorld.cs`
baris 133–139 — sisa penggabungan cabang yang meninggalkan argumen konstruktor kembar:

```csharp
var controller = new PatientEncounterController(
    dbContext,
    loggerService,
    new QueueRealtimeService(dbContext, new FakeQueueHubContext(), loggerService),
    new ClinicalDocumentIntegrityService(dbContext));   // <- pernyataan berakhir di sini
    queueRealtimeService,                                // <- dua baris yatim
    integrityService);
```

Error yang muncul: `CS1002` dan `CS1513` pada baris 138 dan 139.

Bukti bahwa ini **bukan** akibat task ini: berkas tersebut tidak muncul pada `git status`, sehingga
isinya sama persis dengan `HEAD`. Riwayatnya menunjuk merge `93b3227`
(*Merge remote-tracking branch 'origin/QuilvianIntegrationBackend' into MHamzah*).

Berkas itu **tidak diperbaiki** pada task ini: ia milik modul Registration Management, berada di
luar wewenang tulis task ini, dan memperbaikinya diam-diam akan menyembunyikan kerusakan yang
seharusnya diketahui pemiliknya. Perbaikannya cukup menghapus dua baris yatim tersebut.

Agar pengujian task ini tetap punya bukti eksekusi nyata, 26 pengujiannya dijalankan lewat project
verifikasi sementara di direktori scratchpad yang me-*link* berkas test yang sama dan mereferensikan
`QuilvianSystemBackend.csproj`. **Nol berkas repository disentuh untuk itu.** Setelah
`PatientEncounterTestWorld.cs` diperbaiki pemiliknya, berkas test ini berjalan apa adanya di dalam
`QuilvianSystemBackend.Tests` tanpa perubahan.

**Rincian 26 pengujian:**

| Kelompok | Jumlah | Yang dibuktikan |
| --- | ---: | --- |
| Pengelolaan dasar | 4 | Kode dinormalkan huruf besar, kode kembar ditolak, kode salah ketik dapat dibetulkan |
| Masa berlaku bukti kecocokan | 6 | `AC-BD-055` nilai berbeda per komponen; `AC-BD-056` sistem tidak menanam angka; nol dan negatif ditolak |
| Ringkasan, pilihan, daftar, metadata | 5 | Pencacah komponen tertahan, pilihan hanya aktif, penyaring `isValidityConfigured`, halaman dihitung backend, metadata selaras dengan daftar |
| Penghapusan | 3 | Penandaan bukan penghapusan fisik, penghapusan ganda ditolak, kode bekas dapat dipakai lagi |
| Seeder | 8 | Mengisi PRC/TC/FFP, tidak menebak masa berlaku, dapat diulang, tidak menimpa nilai petugas, menolak produksi |

Uji manual: `NOT FEASIBLE` — menuntut database PostgreSQL yang sudah dimigrasikan beserta akun
ber-hak-akses, dan eksekusi database adalah wewenang terpisah yang tidak diminta pada task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Eksekusi migration ke database | Wewenang terpisah. `CLAUDE.md` menuntut konfirmasi tersendiri untuk eksekusi database |
| Smoke test endpoint lewat HTTP | Menuntut aplikasi berjalan dengan database yang sudah dimigrasikan |
| Pemeriksaan index unik fisik dan foreign key | Provider InMemory tidak menegakkan keduanya. Menjadi bagian verifikasi migration |
| `dotnet test` seluruh solusi | Terhalang kerusakan pre-existing di atas |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-055` — komponen PRC dan TC dikonfigurasi masa berlaku berbeda, keduanya diterapkan sesuai komponennya dan dibaca dari katalog | **Terpenuhi** | `MasaBerlaku_BerbedaPerKomponen_DibacaDariKatalog` |
| `AC-BD-056` — nilai masa berlaku dicoba ditanam di kode, bukan dibaca dari katalog | **Terpenuhi** | `MasaBerlaku_TidakDiisi_TetapKosongDanDitandaiMenahanPemberian` dan `Seeder_TidakPernahMenebakMasaBerlaku`. Tidak ada satu pun angka masa berlaku di dalam kode; entity, service, dan seeder seluruhnya membiarkannya `null` |
| DoD — CRUD berjalan | **Terpenuhi** untuk `MstBloodComponent` | Sembilan endpoint + 26 pengujian |
| DoD — endpoint master jalan | **Terpenuhi** untuk `MstBloodComponent` | Bagian 4 |
| DoD — seed minimum terisi | **Terpenuhi sebagian** | `BloodComponentSeeder` mengisi PRC, TC, FFP. Seeder untuk `MstBloodBankReason` belum ada |
| DoD — seluruh kategori alasan terseed | **Belum terpenuhi** | `MstBloodBankReason` tidak masuk scope yang diberikan |

### Butir yang belum terpenuhi, disebut apa adanya

`BE-BD-001` pada roadmap mencakup **dua** master: `MstBloodComponent` **dan** `MstBloodBankReason`
beserta kategori alasannya — termasuk dua kategori pembatalan order dari `DEC-BD-044`, kategori
penolakan koreksi dari `DEC-BD-041`, dan enam kategori lain pada rencana data master awal.

Scope task ini dipersempit pemberi tugas menjadi `MstBloodComponent` saja. Karena itu:

- **`BE-BD-001` TIDAK boleh ditandai selesai.** Statusnya `SELESAI SEBAGIAN`.
- `BE-BD-003` (order darah) menuntut `MstBloodBankReason` beserta kategori pembatalannya, sehingga
  ia belum dapat dimulai walaupun `MstBloodComponent` sudah ada.
- `BE-BD-016` (seeder hak akses) belum terpengaruh; ia tetap menunggu gilirannya sendiri.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 186 warning build, seluruhnya pre-existing pada berkas lain. Nol warning dari berkas yang dibuat task ini |
| Masalah yang diketahui | **Pemeriksaan pemakaian pada `DELETE /{id}` belum ada.** Standar master data menuntut penghapusan memeriksa relasi pemakainya lebih dulu. Pemakai katalog ini adalah `BbkBloodOrderLine` dan `BbkBloodUnit`, dan keduanya belum ada di source karena dijadwalkan pada `BE-BD-003` dan `BE-BD-004`. Memeriksa tabel yang belum ada tidak mungkin dilakukan, dan mengarangnya menghasilkan kode mati yang menyesatkan. Batas ini dicatat pada komentar `DeleteAsync` supaya pelaksana `BE-BD-003`/`BE-BD-004` menambahkannya di sana |
| Risiko tersisa | Migration **belum dijalankan**, sehingga endpoint belum dapat dipakai di lingkungan mana pun. Selama `MstBloodBankReason` belum ada, gelombang `MVP-1` tetap tertahan |
| Perubahan sampingan | `Migrations/ApplicationDbContextModelSnapshot.cs` berubah otomatis oleh `dotnet ef migrations add`. Itu perilaku wajar dan bukan suntingan manual |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |
| Langkah berikutnya | 1. Pemilik Registration Management memperbaiki `PatientEncounterTestWorld.cs` agar test project dapat dikompilasi lagi. 2. Lanjutkan `BE-BD-001` bagian `MstBloodBankReason` supaya task dapat ditandai selesai. 3. Jalankan migration lewat wewenang eksekusi database yang terpisah. 4. Lanjut `BE-BD-002` dan `BE-BD-014` |

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
?? Areas/HealthServices/MasterData/Controllers/BloodComponentController.cs
?? Areas/HealthServices/MasterData/DTOs/BloodComponentDtos.cs
?? Areas/HealthServices/MasterData/Models/MstBloodComponent.cs
?? Areas/HealthServices/MasterData/Seeders/BloodComponentSeeder.cs
?? Areas/HealthServices/MasterData/Services/BloodComponentService.cs
?? Migrations/20260903044753_AddMstBloodComponent.Designer.cs
?? Migrations/20260903044753_AddMstBloodComponent.cs
?? QuilvianSystemBackend.Tests/HealthServices/MasterData/
?? Repositories/Configurations/HealthServices/MasterData/MstBloodComponentConfiguration.cs
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-001.md
```

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MasterData` |
| Submodule | Tidak berlaku — master tinggal langsung di `MasterData` |
| Pemilik/prefix registry | `Administrator / HealthServices` · `Master / Reference` · prefix **`Mst`** · Lifecycle **`ACTIVE`** |
| Keberlakuan | `NEW CODE` |
| Status registry | Terdaftar dan `ACTIVE`. Folder `Areas/HealthServices/MasterData/**` sudah tercatat, sehingga `QBE-MOD-003` terpenuhi tanpa entri baru |
| Prefix modul Bank Darah | `Bbk` juga `ACTIVE`, tetapi **tidak dipakai** pada task ini — katalog komponen adalah master, dan masternya dimiliki Master Data dengan prefix `Mst` (`DEC-BD-024`, `02-backend-architecture.md` §D) |

**QBE ID yang benar-benar berlaku dan cara pemenuhannya:**

| QBE ID | Pemenuhan |
| --- | --- |
| `QBE-ENT-001` | `MstBloodComponent` mewarisi `IdentityModel` |
| `QBE-ENT-002` | `Guid` PK, nullability mengikuti semantik domain — masa berlaku sengaja nullable |
| `QBE-ENT-003` | Nol field presentasi dipersistensi. `IsIssuanceBlockedByMissingValidity` **dihitung** pada DTO, tidak disimpan |
| `QBE-NAM-001` | Nol pemakaian `Trx*` |
| `QBE-NAM-002`, `QBE-NAM-004` | Prefix `Mst` diambil dari registry, bukan disimpulkan dari nama folder |
| `QBE-CFG-001` | `MstBloodComponentConfiguration` menyediakan mapping, key, dan index |
| `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` | Capability ditempatkan di bawah Area/Module pemiliknya yang sudah terdaftar |
| `QBE-SVC-001` | Controller **tidak** menyentuh `ApplicationDbContext`; seluruh CRUD dimiliki `BloodComponentService` |
| `QBE-API-001` | Seluruh response terbungkus `ApiResponse<T>`; daftar memakai `PagedResult<T>` |
| `QBE-PERM-001` | `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` terpasang pada seluruh action |
| `QBE-LOG-001` | `LoggerService` mencatat Create, Update, UpdateStatus, dan Delete beserta pelakunya. `GET` tidak dicatat, mengikuti konvensi project |
| `QBE-VAL-001` | Validasi request dan invarian bisnis ada di service, bukan di controller |
| `QBE-DTO-001` | Entity EF tidak pernah dikembalikan sebagai kontrak API |
| `QBE-PAGE-001` | Paging, pencarian, dan pengurutan memakai pola yang sudah mapan |
| `QBE-OPT-001` | `/options` dan `/filters/metadata` disediakan karena layar master memang mengonsumsinya |
| `QBE-DEL-001` | Soft delete beserta `DeleteDateTime` dan `DeleteBy` |
| `QBE-AUD-001` | Audit database (`IdentityModel`) terpisah dari application logging (`LoggerService`) |

**QBE ID yang TIDAK berlaku, beserta alasannya:**

| QBE ID | Alasan tidak berlaku |
| --- | --- |
| `QBE-CODE-001` sampai `QBE-CODE-006` | `ComponentCode` **bukan** nomor bisnis yang dialokasikan sistem. Ia identifier domain yang ditulis pengguna dan sudah dipakai BDRS sehari-hari — `PRC`, `TC`, `FFP` — sama bentuknya dengan `PurposeCode` pada master keperluan akses rekam medis. Tidak ada number-series yang terlibat. `QBE-CODE-004` tetap dihormati semangatnya: kode unik memiliki unique index database |
| `QBE-TXN-001` | Seluruh operasi menyentuh satu baris pada satu tabel; tidak ada konsistensi lintas record yang perlu ditransaksikan |
| `QBE-ENUM-001` | Task ini tidak menambah enum |
| `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` | Khusus `LEGACY MIGRATION`; task ini `NEW CODE` |
| `QBE-CFG-002` | Khusus `TOUCHED LEGACY`; task ini tidak menyentuh configuration legacy |
