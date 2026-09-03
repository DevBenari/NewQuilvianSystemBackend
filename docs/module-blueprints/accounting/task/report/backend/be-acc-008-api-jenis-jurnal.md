# BE-ACC-008 — API jenis jurnal

- **TASK ID:** `BE-ACC-008` — API jenis jurnal
- **TASK TYPE:** Implementasi backend, controller + service + DTO
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** **6** — repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (5 berkas), logika bisnis 1 (tiga aturan validasi, satu aturan penguncian), kontrak API 1, database 1 (persistence saja), keamanan/auth 1, UI/workflow 0
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `Areas/Corporate/AccountingManagement/`, `Program.cs`, `docs/module-blueprints/accounting/`
- **VISUAL REFERENCE:** `NOT REQUIRED`
- **BLUEPRINT STATUS/EVIDENCE:** `NOT APPLICABLE`
- **STALE EVIDENCE / BLOCKED PHASES:** `NOT APPLICABLE`
- **INTERRUPTIONS:** `NONE`
- **WARNINGS:** 203 warning build, seluruhnya pre-existing dan milik modul lain
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 9 `APPROVED`, `decision_revision` 1.6
- **HEAD saat mulai:** `d9a9111` pada branch `rizkiG`, working tree bersih

> ## ✅ STATUS: `DONE`
>
> Ketiga acceptance terbukti **18 test**, seluruhnya lulus. **`ACC-TD-004` ditutup** — seeder
> `BE-ACC-006` akhirnya punya call site.

## Validasi baseline

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `9` | `9` | Cocok |
| `decision_revision` | `1.6` | `1.6` | Cocok |
| `ACC-API` / `ACC-PERMISSION` | `0.2` / `0.3` | `0.2` / `0.3` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |
| `verification_backend_source_sha` | `d9a5a6e` | `d9a9111` | **Berbeda — impact scan dijalankan** |

### Impact scan `d9a5a6e` → `d9a9111`

Dua commit, keduanya pekerjaan Accounting sendiri: `5c81ae4` (`BE-ACC-007`) dan `d9a9111`
(`ACC-DEC-043`). **Nol sentuhan `Migrations/` dan `ModelSnapshot`.** Dampak terhadap `BE-ACC-008`:
nihil — justru menyediakan `AccountingServiceResult` dan `AccountingLegalEntityGuard` yang dipakai
task ini.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `MasterData / JournalType` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002`/`003` tidak terpicu — nol model persisted baru. `QBE-CODE-002` **dipatuhi** — controller tidak mengalokasikan nomor bisnis apa pun. Alur `Controller → Module Service → DbContext` ditegakkan |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. FILE YANG DIBUAT

| Berkas | Baris |
|---|---:|
| `Areas/.../MasterData/JournalType/DTOs/JournalTypeDtos.cs` | 131 |
| `Areas/.../MasterData/JournalType/Services/AccJournalTypeService.cs` | 375 |
| `Areas/.../MasterData/JournalType/Controllers/JournalTypeController.cs` | 138 |
| `Tests/.../AccountingManagement/JournalTypeServiceTests.cs` | 466 |

## 2. FILE YANG DIUBAH

| Berkas | Perubahan |
|---|---|
| `Program.cs` | **+2 baris, 0 penghapusan** — 1 `using`, 1 `AddScoped<AccJournalTypeService>()` |

Nol pemanggilan seeder dan nol logika startup ditambahkan ke `Program.cs`, sesuai larangan
`02-backend-architecture.md` bagian 6.

---

## 3. ENDPOINT YANG DIBUAT

Empat sesuai `ACC-API-0.2`, ditambah satu yang dilaporkan sebagai delta. Base URL
`api/v1/corporate/accounting/master-data/journal-types`.

| Method | Path | Hak akses | Keterangan |
|---|---|---|---|
| `GET` | `/` | `JournalType : Read` | Daftar berhalaman, dapat disaring |
| `GET` | `/options` | `JournalType : Read` | Hanya jenis aktif, membawa `NumberPrefix` |
| `POST` | `/` | `JournalType : Create` | `201` |
| `PUT` | `/{id}` | `JournalType : Update` | — |
| `POST` | `/seed` | `JournalType : Create` | **Delta — lihat bagian 7** |

## 4. ATURAN YANG DITEGAKKAN

Seluruh tiga baris `ACC-VALIDATION-0.2` bagian 2.

| Aturan | Kode | Tempat |
|---|:---:|---|
| Kode jenis wajib, maksimal 10 karakter | `400` | `PeriksaIsianDasar` |
| Kode jenis unik | `409` | `KodeTerpakaiAsync` — perbandingan mengabaikan besar kecil huruf |
| Awalan nomor wajib | `400` | `PeriksaIsianDasar` |
| Jenis sistem terkunci pada kode dan awalan nomor | `409` | `UpdateAsync` |

### Yang terkunci pada jenis sistem hanya dua kolom

`JB` dan `SA` terkunci pada **kode** dan **awalan nomor** saja. Nama dan keaktifannya tetap boleh
disesuaikan, karena keduanya tidak dipakai proses pembalikan maupun saldo awal untuk menemukan
jenisnya — yang dipakai adalah kodenya. Mengunci lebih dari yang perlu akan membuat pemilik proses
tidak dapat menyesuaikan sebutan yang dipakai rumah sakitnya.

### Tanda sistem tidak dapat diberikan pengguna

`CreateJournalTypeRequest` dan `UpdateJournalTypeRequest` sengaja **tanpa** `IsSystemType`.

Kalau pengguna dapat menetapkannya, ia dapat membuat jenis jurnal yang kemudian terkunci dari
perubahan tanpa alasan yang sah — dan aturan "jenis sistem terkunci" berubah dari pengaman menjadi
jebakan yang tidak ada cara keluarnya lewat API. Tanda sistem hanya lahir dari data master awal.

Ketiadaannya dijaga test, sehingga penambahannya harus lewat keputusan sadar.

## 5. CALL SITE SEEDER — `ACC-TD-004` DITUTUP

`AccountingMasterDataSeeder` dibuat pada `BE-ACC-006` tetapi **tidak dipanggil kode aplikasi mana
pun**, sehingga `AccJournalType` di database kosong. Ini yang membuat `BE-ACC-010` kelak tidak
menemukan awalan nomor jurnal.

Call site-nya kini `AccJournalTypeService.SeedAsync`, dipanggil `POST /seed`.

| Pilihan tempat | Dipakai? | Alasan |
|---|:---:|---|
| `Program.cs` saat startup | **Tidak** | Dilarang `02-backend-architecture.md` bagian 6 |
| Diam-diam dari jalur `GET` | **Tidak** | Mengisi data sebagai efek samping sebuah `GET` menyembunyikan siapa yang mengisinya dan kapan |
| Endpoint administratif tersendiri | **Ya** | Eksplisit, tercatat di jejak audit, dan aman dipanggil berulang |

Idempotensinya dijamin seeder dan dibuktikan ulang di sini: pemanggilan kedua menyisipkan **nol**
baris dan melaporkan 4 dilewati.

**Satu hal yang tersisa dan bukan pekerjaan kode:** endpoint ini harus benar-benar **dipanggil
sekali** agar tabelnya terisi. Sampai itu dilakukan, `AccJournalType` di database tetap kosong.
Dicatat sebagai `ACC-TD-011`.

## 6. PERTENTANGAN ROADMAP vs KONTRAK ENGINEERING — dan cara ia terselesaikan

Owner meminta ini diperiksa lebih dahulu dan **tidak diputuskan sepihak**. Hasilnya: pertentangan
itu **terselesaikan oleh roadmap itu sendiri**, bukan oleh saya.

| Sumber | Yang dikatakan |
|---|---|
| `roadmap/backend-roadmap.md` baris 361, kolom *Reuse* `BE-ACC-008` | "`ApplicationDbContext` langsung — CRUD sederhana, tanpa service, sesuai konvensi" |
| `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, bagian *Boundary API/service dan nomor bisnis* | "Alur baru adalah **Controller → Module Service → DbContext**/infrastruktur bersama/integrasi eksternal" |

**Yang menyelesaikannya ada di roadmap baris 72**, pada tabel *Catatan eksekusi yang mengikat
seluruh task*:

> | QBE preflight dan kesesuaian engineering | Diselesaikan **pada waktu eksekusi** dari `AGENTS.md`
> backend target dan dokumen engineering canonical, **bukan dari roadmap ini** |

Roadmap secara eksplisit menyerahkan urusan kesesuaian engineering kepada dokumen canonical.
Karena itu catatan *Reuse* pada baris 361 **tunduk pada aturannya sendiri**, dan yang berlaku
adalah kontrak engineering.

Diperkuat dua hal lain:

1. Skill `build-module-backend` butir 3 menyatakan untuk `NEW CODE`, **"pemakaian
   `ApplicationDbContext` langsung di controller ... tidak menjadi alasan untuk menirunya"**.
2. Konsistensi modul. `BE-ACC-007` sudah memakai service. Dua submodule bersebelahan dengan gaya
   berbeda lebih buruk daripada gaya mana pun yang dipilih konsisten.

**Usulan untuk owner:** perbaiki kolom *Reuse* `BE-ACC-008` pada roadmap agar tidak lagi
bertentangan dengan kontrak engineering. Ini perbaikan teks roadmap, bukan perubahan target — nol
dampak pada kode yang sudah ditulis.

Tambahan yang membuat service semakin diperlukan, dan tidak terlihat saat roadmap disusun:
`AccountingLegalEntityGuard.PeriksaAsync<T>` mengembalikan `AccountingServiceResult<T>`, dan
seeder membutuhkan tempat yang bukan controller. Keduanya lahir sesudah roadmap ditulis.

## 7. DELTA TERHADAP KONTRAK — `POST /seed`

`ACC-API-0.2` grup Journal Type mencantumkan **empat** endpoint. Implementasi ini menambah
**kelima**: `POST /seed`.

| Hal | Keterangan |
|---|---|
| Kenapa perlu | Owner secara eksplisit meminta seeder diberi call site pada task ini, dan kontrak tidak menyediakan tempat yang wajar |
| Hak akses | `JournalType : Create` — akibatnya memang menambah baris master |
| Sifat | Idempoten, aman dipanggil berulang |
| Status | **Delta, menunggu ratifikasi owner.** Diusulkan `ACC-API-0.2` → `0.3` |

Dilaporkan, tidak diputuskan sepihak.

## 8. BUILD RESULT

```
dotnet build ./QuilvianSystemBackend.sln
Build succeeded.
    0 Error(s)
```

Nol warning berasal dari keempat berkas baru.

## 9. VALIDATION

| Perintah / pemeriksaan | Hasil | Klasifikasi |
|---|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | 0 error | **PASS** |
| `dotnet test --filter JournalTypeServiceTests` | **18 lulus**, 0 gagal | **PASS** |
| `dotnet test --filter AccountingManagement` | **62 lulus**, 0 gagal | **PASS** |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **238 lulus**, 0 gagal | **PASS** — nol regresi |
| Verifikasi 17 hash canonical | 17/17 cocok | **PASS** |
| Impact scan `d9a5a6e..d9a9111` | Nol sentuhan migration/snapshot | **PASS** |

## 10. ACCEPTANCE CRITERIA `BE-ACC-008`

| # | Kriteria | Hasil | Test |
|---|---|:---:|---|
| 1 | Kode jenis kembar ditolak `409` | ✅ | `KodeJenisKembar_Ditolak409`, `KodeJenisKembar_TidakPeduliBesarKecilHuruf` |
| 2 | Jenis bertanda sistem gagal diubah kode maupun awalan nomornya | ✅ | `JenisSistem_KodeGagalDiubah_Ditolak409`, `JenisSistem_AwalanNomorGagalDiubah_Ditolak409` |
| 3 | Awalan nomor kosong ditolak `400` | ✅ | `AwalanNomorKosong_Ditolak400`, `AwalanNomorKosongSaatUbah_Ditolak400` |

Ditambah pembuktian batas sebaliknya, supaya aturannya tidak mengunci lebih dari yang seharusnya:

| Test | Membuktikan |
|---|---|
| `JenisSistem_NamaMasihBolehDiubah` | Penguncian hanya pada kode dan awalan nomor |
| `JenisBiasa_KodeMasihBolehDiubah` | `JU`/`JP` tetap dapat disesuaikan pemilik proses |
| `JenisBaru_TidakPernahBertandaSistem` | Tanda sistem tidak dapat diberikan pengguna |
| `Seed_DijalankanDuaKali_TidakMenghasilkanDataGanda` | Idempotensi call site seeder |
| `SesudahSeed_OptionsMemberiAwalanNomor` | `BE-ACC-010` akan menemukan awalan nomornya |
| `BadanHukumUtamaGanda_SeluruhJalurMenolak` | Penjaga `ACC-DEC-043` dipanggil di keempat jalur |

## 11. DEFINITION OF DONE

| Butir DoD roadmap | Hasil |
|---|:---:|
| Acceptance terbukti test | ✅ 18 test |
| Laporan task tersedia | ✅ Berkas ini |

## API CONTRACT IMPACT

Mewujudkan `ACC-API-0.2` grup Journal Type, empat endpoint, ditambah satu delta `POST /seed` yang
dilaporkan pada bagian 7.

## DATABASE IMPACT

`NONE` sebagai perubahan schema. Migration tetap **119**, snapshot tetap **545 tabel**,
`git diff -- Migrations/` kosong. Nol `dotnet ef` dijalankan.

Yang bertambah adalah **kemampuan mengisi** `AccJournalType` — tetapi tabelnya baru terisi setelah
`POST /seed` benar-benar dipanggil.

## SECURITY IMPACT

Memakai mekanisme hak akses yang sudah ada apa adanya. Penjaga `ACC-DEC-043` dipanggil di keempat
jalur service.

`AccJournalType` sengaja tanpa `LegalEntityId` — jenis jurnal bersifat struktural dan berlaku sama
untuk semua badan hukum. Karena itu endpoint ini tidak memerlukan penyaringan badan hukum, dan
tidak terdampak `ACC-TD-002`.

## MANUAL TEST

`NOT APPLICABLE` untuk acceptance — ketiganya tertutup test otomatis.

**Tetapi satu langkah manual tetap diperlukan**: `POST /seed` harus dipanggil sekali terhadap
database agar empat jenis jurnal benar-benar terisi. Lihat `ACC-TD-011`.

## INCIDENTAL CHANGES

`NONE`.

## GIT STATUS

```
 M Program.cs
?? Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/
?? Areas/Corporate/AccountingManagement/MasterData/JournalType/DTOs/
?? Areas/Corporate/AccountingManagement/MasterData/JournalType/Services/
?? Tests/QuilvianSystemBackend.Tests/AccountingManagement/JournalTypeServiceTests.cs
```

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

**Panggil `POST /seed` sekali** agar `AccJournalType` terisi (`ACC-TD-011`). Tanpa itu,
`BE-ACC-010` akan gagal menemukan awalan nomor jurnal.

Sesudah itu `BE-ACC-009` — API periode akuntansi, 5 endpoint. Butir yang paling mudah salah di
sana: membuka kembali periode `Closed` harus menghasilkan `SoftClosed`, **bukan** `Open`
(`ACC-DEC-028`).

Bila tujuannya sampai ke frontend, jalur tercepatnya tetap `ACC-FE-001` — lihat `ACC-TD-009`.

**Menunggu instruksi eksplisit owner.**
