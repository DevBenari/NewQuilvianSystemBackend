# Laporan Perubahan Backend — `PLT-BE-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PLT-BE-002` |
| Judul | Tabel pencacah deret berdiri |
| Slice | `PLT-SLICE-01` — mesin alokasi nomor untuk deret baru · gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/platform/roadmap/backend-roadmap.md` §5 |
| Trace | `FR-PLT-008`..`FR-PLT-011` · `DEC-PLT-002`, `DEC-PLT-004`, `DEC-PLT-009`, `DEC-PLT-010` · `INV-PLT-001`, `INV-PLT-002` · `AC-PLT-011` |
| Contract version | `v1` — ✅ **`approved`** (`Sukma Giri Pratama` / `2026-09-09`) |
| Dependency | `P0` ✅ approval blueprint · `P1` ✅ baris registry · `PLT-BE-001` ✅ |
| Klasifikasi | `LIGHT` — satu entity, satu configuration, satu konstanta, satu `DbSet`, satu migration. Nol service, nol controller, nol endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Platform/**`, `Repositories/**`, `Migrations/**`, `Tests/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` cabang `sukmagp` |
| Tanggal | `2026-09-09` |
| Status | **`SELESAI`** untuk scope task. Migration **dibuat, belum dijalankan** |

---

## 1. Masalah yang diperbaiki

Modul Platform sudah punya blueprint, kontrak, dan roadmap yang disetujui — tetapi **nol baris
kode**. Tidak ada satu pun tempat menyimpan pencacah deret nomor, sehingga alokator yang akan
dibangun `PLT-BE-003` tidak punya apa pun untuk ditulis.

Akibatnya berantai jauh: gerbang `G4` modul Bank Darah menahan sembilan task backend, dan tiga
modul lain — Billing & Kasir, Rawat Inap, Rawat Jalan — sudah merencanakan pemakaian provider
yang belum ada (`FACT-PLT-006`).

**Contoh konkretnya.** Petugas Bank Darah tidak dapat membuat satu pun order darah, karena
`BbkBloodOrder.OrderNumber` wajib dialokasikan provider atomik dan `QBE-CODE-002/003` melarang
`Count+1`/`Max+1` sebagai gantinya. Task ini membangun fondasi paling bawahnya.

---

## 2. Proses bisnis

Task ini **tidak** menambah proses bisnis apa pun yang terlihat pengguna. Ia membangun tempat
penyimpanan; yang memakainya adalah `PLT-BE-003`.

Yang perlu dipahami dari bentuk tabelnya:

| Hal | Isi |
| --- | --- |
| Satu baris menyimpan | Nilai terakhir yang **sudah terbit** untuk satu deret pada satu periode |
| Baris lahir | **Sendiri**, saat deret dipakai pertama kali. Nol layar membuatnya, nol seeder mengisinya |
| Pencacah bergerak | **Hanya naik.** Nol jalur kode menurunkan, menyetel ulang, atau menghapusnya |
| Deret berlubang | **Keadaan sah** (`INV-PLT-002`), bukan cacat data. Tidak boleh dirapikan |

**Kenapa tidak ada baris yang di-seed.** Menyemai baris dengan nilai nol melanggar check
constraint `CurrentValue > 0`; menyemainya dengan nilai tebakan berisiko menerbitkan nomor yang
sudah menempel pada catatan lain. Keduanya lebih buruk daripada tabel kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` · `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` · `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` · `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| Kontrak modul | `platform/blueprint-manifest.md` · `platform/02-backend-architecture.md` §D.1/§F · `platform/data/data-dictionary.md` · `platform/roadmap/backend-roadmap.md` |
| Pola source terdekat | `BilNumberSeries.cs` + `BilNumberSeriesConfiguration.cs` — bentuk tabel yang sengaja ditiru · `TestDatabase.cs` — harness SQLite |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Platform/NumberSeriesManagement/Constants/NumberSeriesResetPolicies.cs` | **Baru.** Empat kebijakan pengulangan; nilainya sama persis dengan `BillingNumberResetPolicies` |
| `Areas/Platform/NumberSeriesManagement/Models/NumNumberSeries.cs` | **Baru.** Pencacah deret; lima kolom di luar warisan `IdentityModel` |
| `Repositories/Configurations/Platform/NumberSeriesManagement/NumNumberSeriesConfiguration.cs` | **Baru.** Index unik `(SequenceKey, ScopeKey)` + dua check constraint |
| `Repositories/ApplicationDbContext.cs` | Satu `DbSet` pada region baru `PLATFORM` + satu `using` |
| `Migrations/20260909070218_AddNumNumberSeries.cs` | **Baru.** Satu `CreateTable` + satu `CreateIndex`. **Belum dijalankan** |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/NumNumberSeriesSchemaTests.cs` | **Baru.** 10 pengujian bentuk tabel |

**Nol berkas existing dihapus, dinamai ulang, atau diubah perilakunya.** `BilNumberSeries` dan
`BillingNumberSeriesService` **tidak disentuh satu baris pun**.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini nol endpoint. Empat endpoint baca milik `PLT-BE-005` |
| Database | Satu tabel baru `NumNumberSeries` + satu index unik + dua check constraint. **Nol tabel existing disentuh.** Migration `20260909070218_AddNumNumberSeries` **dibuat, BELUM dijalankan** |
| Keamanan/Auth | `NOT APPLICABLE` — nol endpoint, nol butir hak akses. Butir `NumberSeries : Read` lahir bersama `PLT-BE-005` |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | **`Platform`** — Area baru, `DEC-PLT-009` |
| Module | `NumberSeriesManagement` |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `NumberSeriesManagement / Number Series` → prefix **`Num`**, `DEC-PLT-010` |
| Status registry | ✅ **`ACTIVE`** — didaftarkan `PLT-BE-001` pada 2026-09-09 |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-001` folder di bawah pemiliknya · `QBE-MOD-002`/`QBE-MOD-003` registry `ACTIVE` sebelum entity pertama · `QBE-NAM-002`/`QBE-NAM-004` prefix `Num` dari registry · `QBE-NAM-001` nol `Trx*` · `QBE-CODE-004` kode bisnis unik punya unique index sesuai scope-nya |
| `QBE-SVC-001` | **Belum berlaku** — task ini nol service dan nol controller |
| `QBE-CODE-001/002/003` | **Belum berlaku** — task ini nol alokasi nomor. Berlaku penuh pada `PLT-BE-003` |

**Verifikasi preflight dijalankan, bukan diasumsikan.** `Resolve-RegistryOwnership` atas berkas
yang benar-benar dibuat memulangkan:

```text
Resolved : True
Reason   : Resolved Area 'Platform', owner 'NumberSeriesManagement / Number Series',
           Category 'BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY', Prefix 'Num',
           Lifecycle 'ACTIVE'.
```

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Permukaan HTTP modul Platform
seluruhnya milik `PLT-BE-005`, dan kemampuan intinya (alokasi) memang **tidak** dipaparkan lewat
HTTP sama sekali (`contracts/api-contract.md` §1).

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | Berhasil — `0 Error(s)`, **`210 Warning(s)`** | `PASS` | Sama persis dengan baseline — **nol peringatan baru** |
| `NumNumberSeriesSchemaTests` | `Failed: 0, Passed: 10, Total: 10` | `PASS` | Keluaran perintah |
| `dotnet test` — `UnitTests.Sqlite` | `Failed: 0, Passed: 187, Total: 187` | `PASS` | Naik dari 177; selisih 10 adalah uji baru task ini |
| `dotnet test` — `QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 419, Total: 419` | `PASS` | Tidak berubah dari baseline |
| `dotnet test` — `UnitTests.InMemory` | `Failed: 9, Passed: 896, Total: 905` | `EXISTING / ENVIRONMENT ISSUE` | **Jumlah dan nama kegagalannya identik dengan baseline** — lihat catatan |
| `Resolve-RegistryOwnership` atas `NumNumberSeries.cs` | `Resolved: True` | `PASS` | `QBE-MOD-002` terpenuhi, dibuktikan bukan diasumsikan |
| `dotnet ef migrations add` | `Up()` hanya 1 `CreateTable` + 1 `CreateIndex`; nol operasi merusak | `PASS` | `git diff --numstat` snapshot: **323 tambahan, 0 penghapusan** |
| Eksekusi migration ke database | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; belum diminta |

**Sembilan kegagalan `UnitTests.InMemory` adalah keadaan bawaan yang sama persis.** Jumlahnya
tetap 9 dan namanya tidak berubah — seluruhnya `QuilvianSystemBackend.Tests.BillingManagement.*`
(`BillingCalculationServiceTests` 7, `BillingFinalizationServiceTests` 1,
`BillingInvoiceServiceTests` 1). Task ini **nol** menyentuh berkas Billing. Bukti lengkapnya ada
pada laporan `BE-BD-005` bagian 5; perbaikannya milik pemilik Billing dan **di luar wewenang task
ini**.

Uji manual: `NOT FEASIBLE` — tabel belum ada di database mana pun karena migration belum
dijalankan, dan tidak ada endpoint yang dapat dipanggil.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `IntegrationTests.Postgres` | Menuntut PostgreSQL berjalan. **Uji inti modul ini memang di sana** — durabilitas pencacah dan antrean alokasi — tetapi keduanya milik `PLT-BE-004`, bukan task ini |
| Eksekusi migration | Wewenang terpisah. Index unik dan check constraint **sudah terbukti di SQLite**, tetapi belum di PostgreSQL |
| Uji alokasi nomor | Alokator belum ada; ia milik `PLT-BE-003` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-PLT-011` — baris kembar `(SequenceKey, ScopeKey)` ditolak index unik | **Terpenuhi** | `PasanganDeretDanPeriode_KemBar_DitolakDatabase` — penyisipan kedua melempar `DbUpdateException`, dan hitungan baris tetap 1 |
| Dua check constraint terpasang | **Terpenuhi** | `PencacahNol_DitolakCheckConstraint` dan `KebijakanPengulanganAsing_DitolakCheckConstraint`; keempat kebijakan sah dibuktikan diterima lewat `[Theory]` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Migration **dibuat** | **Terpenuhi** — `20260909070218_AddNumNumberSeries` |
| Migration **belum dijalankan** | **Terpenuhi apa adanya** — dinyatakan eksplisit, bukan didiamkan |
| Nol baris di-seed | **Terpenuhi** — dibuktikan `TabelLahirKosong_NolBarisDiSeed` |
| `UnitTests.Sqlite` membangun model relasional | **Terpenuhi** — 187 lulus, index dan constraint ikut terbentuk |
| Migration `Up()` aditif murni | **Terpenuhi** — 1 `CreateTable` + 1 `CreateIndex`, nol tabel existing disentuh |

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Nol delta** | Bentuk tabel, index, dan kedua check constraint dibuat persis seperti `data/data-dictionary.md` §`NumNumberSeries` dan `02-backend-architecture.md` §D.1 | Kontrak `v1` sudah cukup rinci; tidak ada yang perlu ditambahkan atau ditafsirkan |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build bersih menghasilkan **`210 Warning(s)`, sama persis dengan baseline** — nol peringatan baru dari berkas task ini |
| Masalah yang diketahui | **(1)** Index unik dan check constraint terbukti di **SQLite**, belum di PostgreSQL — pembuktian di sana menunggu migration dijalankan. **(2)** Nol perilaku alokasi diuji; itu memang bukan scope task ini |
| Risiko tersisa | **Migration belum dijalankan**, sehingga tabel belum ada di lingkungan mana pun. `PLT-BE-003` dapat dibangun dan diuji terhadap model, tetapi alokator tidak akan berjalan sungguhan sampai migration diterapkan |
| **Temuan tata kelola yang dilaporkan** | **Registry di suite skill dan di repository backend berselisih.** Baris `Platform`/`Num` **ada** di `NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — yang benar-benar dibaca `Invoke-QbeConformanceCheck.ps1` baris 200 — tetapi **belum ada** di salinan suite skill yang dirujuk `AGENTS.md`. Task ini dilanjutkan karena aturan skill sendiri menyatakan *"bila isinya berbeda dari source repository target, source yang berlaku dan selisihnya dilaporkan"*. Sinkronisasinya milik pemilik registry, dan berkasnya ada di repository `QuilvianEngineeringSkills` — di luar wewenang tulis task ini. Dicatat sebagai `FACT-PLT-012` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 9 |
| Langkah berikutnya | **(1)** `PLT-BE-003` — alokator durabel; **inilah task yang menutup `G4`**. **(2)** `PLT-BE-004` — uji durabilitas di PostgreSQL sungguhan; disarankan lulus sebelum task Bank Darah dijadwalkan. **(3)** Sinkronkan ketiga salinan registry |

---

## 9. Status Git

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
 M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
?? Areas/Platform/
?? Migrations/20260909070218_AddNumNumberSeries.cs
?? Migrations/20260909070218_AddNumNumberSeries.Designer.cs
?? Repositories/Configurations/Platform/
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/Platform/
```

Berkas lain pada working tree berasal dari pekerjaan sebelumnya di sesi yang sama — `BE-BD-005`,
`BE-BD-011`, blueprint Platform, dan roadmap — bukan dari task ini.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. `HEAD` tetap `f0d6855`.
