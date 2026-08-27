# Laporan Perubahan Backend — `RJ-BIL-BE-003` Remediasi Penamaan QBE

## Metadata

| Field | Nilai |
| --- | --- |
| TASK ID | `RJ-BIL-BE-003` — remediasi kepatuhan penamaan dan kepemilikan modul |
| TASK TYPE | Remediasi kepatuhan kontrak engineering; bukan penambahan fitur |
| COMPLEXITY | `MEDIUM` |
| MODEL | `claude-opus-5` |
| TASK MODE | `MODULE BLUEPRINT` |
| WRITE TARGET | `NewQuilvianSystemBackend/` — `Areas/`, `Repositories/`, `Migrations/`, `Tests/`, `docs/module-blueprints/rawat-jalan/` |
| Trace | `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-003`, `QBE-MOD-002`, `QBE-DB-001`, `QBE-DB-002` |
| Contract version | `RJ-BIL-CONTRACT-001@1.0.0` — **tidak berubah**. Tidak ada route, request, atau response yang tersentuh |
| Branch / HEAD | `sukmagp` / `6b25e60` |
| Tanggal verifikasi | 26 Agustus 2026 |
| Status | **SELESAI SEBAGIAN** — seluruh pelanggaran penamaan tertutup; `QBE-MOD-002` tetap terbuka dan memerlukan keputusan pemilik |

---

## 1. Kenapa laporan ini ada

Selama `RJ-BIL-BE-002` dan `RJ-BIL-BE-003` saya tidak pernah membaca `AGENTS.md` maupun
`docs/engineering/`. Saya keliru menganggap `docs/system-registry/` sebagai gerbang kepatuhan,
padahal artefak itu tidak ada di repository ini. Akibatnya dua task berjalan penuh tanpa pernah
melewati kontrak engineering yang sebenarnya berlaku.

Kedua berkas governance itu **sudah ada** pada saat kedua task dikerjakan. Hal ini diverifikasi
langsung terhadap object git, bukan disimpulkan:

```
git cat-file -e d0544e5:AGENTS.md                                        -> ada
git cat-file -e d0544e5:docs/engineering/BACKEND_ENGINEERING_CONTRACT.md -> ada
git cat-file -e 92108587:AGENTS.md                                       -> ada
```

Jadi ini bukan aturan yang baru muncul. Ini aturan yang tidak saya baca.

## 2. Tiga pelanggaran yang ditemukan

| Aturan | Bunyi | Yang dilanggar |
| --- | --- | --- |
| `QBE-NAM-001` | MUST NOT / NEW CODE: memakai `Trx*` untuk entity, berkas, configuration, atau `DbSet` operasional | `TrxLabSpecimen`, `TrxLabTransitionHistory`, `TrxClinicalMilestoneFact` |
| `QBE-NAM-002` | MUST / NEW CODE: memakai prefix modul yang terdaftar di registry | Ketiganya memakai `Trx`, yang bukan prefix modul mana pun |
| `QBE-MOD-002` | MUST / NEW CODE: modul operasional punya entry registry yang disetujui sebelum entity pertamanya | `LaboratoryManagement / Lab` berstatus `PLANNED`; `ClinicalBillingIntegration` tidak terdaftar sama sekali |

`docs/engineering/QBE_EXCEPTIONS.json` kosong, sehingga tidak ada pengecualian yang menaungi
satu pun dari ketiganya.

## 3. Yang diubah

| Sebelum | Sesudah | Alasan |
| --- | --- | --- |
| `TrxLabSpecimen` | `LabSpecimen` | Prefix `Lab` terdaftar untuk `LaboratoryManagement` |
| `TrxLabTransitionHistory` | `LabTransitionHistory` | idem |
| `TrxClinicalMilestoneFact` | `BilClinicalMilestoneFact` | Ledger ini dibaca dan ditulis Billing; pemiliknya Billing, dan `Bil` berstatus `ACTIVE` |
| `MstLabRejectionReason` | *tidak berubah* | `Mst*` adalah konvensi sah untuk master/reference, bukan entity operasional |

Berkas `TrxClinicalMilestoneFact.cs` dan configuration-nya juga **berpindah folder** ke
`Areas/HealthServices/BillingManagement/Operational/`, mengikuti kepemilikannya. Setelah
perpindahan, folder `ClinicalBillingIntegration/` hanya berisi DTO, enum, dan service — nol
entity persisted. Registry mengizinkan folder tak terdaftar selama tidak memiliki entity
persisted, sehingga penempatan `ClinicalMilestoneFactProducer` di sana tetap sah.

### Kenapa Billing yang memiliki, bukan Laboratorium

Ledger ini menyimpan fakta klinis yang **diserahkan ke** Billing beserta status pengirimannya —
`DispatchStatus`, `IdempotencyKey`, snapshot tarif. Yang membacanya untuk memutuskan tagihan
adalah Billing. Laboratorium hanya memicu penulisannya. Pemilik data adalah pihak yang
kebenarannya menjadi tanggung jawabnya, dan itu Billing.

## 4. Dua strategi migration yang berbeda, dan kenapa

Kedua tabel diperlakukan berbeda karena posisinya berbeda.

### 4.1 Laboratorium — disunting di tempat

`20260824091610_AddLaboratorySpecimenLifecycle` **belum pernah diterapkan ke database mana pun**.
Tabelnya belum wujud. Karena itu perbaikan nama cukup dilakukan dengan menyunting berkas
migration-nya, tanpa migration tambahan dan tanpa biaya data sama sekali.

Inilah alasan remediasi dikerjakan sekarang dan bukan nanti. Begitu migration diterapkan,
perbaikan yang sama berubah statusnya menjadi `LEGACY MIGRATION` dan tunduk pada `QBE-NAM-003`,
`QBE-DB-001`, dan `QBE-DB-002` — audit foreign key, rename tabel fisik, dan verifikasi jumlah
baris.

Dua perbaikan tangan yang sudah ada pada migration itu tetap utuh setelah penyuntingan:

- `OrderStatus` memakai `defaultValue: 2` (`Requested`), bukan `0`. `LabOrderStatus` tidak
  memiliki anggota bernilai `0`, sehingga default `0` akan menghasilkan baris berstatus tak
  terbaca. Backfill eksplisit memetakan `IsCancel = true` ke `8` (`Cancelled`).
- Seeding sepuluh alasan penolakan memakai GUID tetap dan `NOW()` — bukan `gen_random_uuid()`
  yang berbeda per environment, dan bukan `NOW() AT TIME ZONE 'UTC'` yang menghasilkan
  `timestamp without time zone` lalu ditafsir ulang memakai zona server.

### 4.2 Fakta milestone klinis — rename yang mempertahankan data

`TrxClinicalMilestoneFact` **sudah diterapkan** oleh `20260824074649` dan berisi bukti serah
terima klinis ke Billing. `QBE-DB-002` melarang perbaikan penamaan dilakukan dengan membuang lalu
membuat ulang tabel, karena cara itu menghapus bukti.

Migration baru `20260826101500_RenameClinicalMilestoneFactToBillingOwnership` karenanya berisi
**murni penggantian nama** dan tidak menyentuh satu baris pun:

| Operasi | Objek |
| --- | --- |
| `RenameTable` | `TrxClinicalMilestoneFact` menjadi `BilClinicalMilestoneFact` |
| `ALTER TABLE ... RENAME CONSTRAINT` | primary key |
| `ALTER TABLE ... RENAME CONSTRAINT` | foreign key ke `TrxPatientEncounter` |
| `RenameIndex` lima kali | kelima index, termasuk dua yang namanya dipotong EF pada batas 63 karakter |

PostgreSQL tidak ikut mengganti nama constraint maupun index ketika tabelnya diganti nama, jadi
ketiganya harus ditangani terpisah. Nama index yang terpotong tetap aman karena `Trx` dan `Bil`
sama-sama tiga karakter, sehingga titik potongnya tidak bergeser.

Audit `QBE-DB-001` dijalankan lebih dulu: pencarian di seluruh `Areas/`, `Repositories/`, dan
`Tests/` tidak menemukan satu pun raw SQL yang menyebut nama tabel fisik itu. Satu-satunya
dependensi fisik adalah constraint dan index di atas.

Migration ini **belum diterapkan**. Penerapannya adalah wewenang terpisah.

## 5. Insiden yang saya sebabkan sendiri, dan pemulihannya

`dotnet ef migrations remove` yang saya jalankan untuk mencabut migration Lab **mengembalikan
`ApplicationDbContextModelSnapshot.cs` ke snapshot milik migration terakhir**, yaitu
`20260824095353_CreateInpatientTransactionTables`. Snapshot migration itu dibuat di branch yang
tidak memiliki entity saya, sehingga pengembalian itu menghapus **47 entity** dari snapshot:

- 28 entity `Bil*` — seluruh modul operasional Billing dari `RJ-BIL-BE-001` dan `BE-002`
- 15 entity `Opr*` — modul Kamar Operasi milik rekan lain
- `TrxLabSpecimen`, `TrxLabTransitionHistory`, `MstLabRejectionReason`, `TrxClinicalMilestoneFact`

Bila hal itu lolos dan seseorang membuat migration berikutnya, EF akan menganggap ke-47 tabel itu
belum ada dan menerbitkan `CreateTable` untuk semuanya — terhadap database yang tabelnya sudah
ada.

Pemulihannya `git checkout HEAD -- Migrations/ApplicationDbContextModelSnapshot.cs`, diverifikasi
dengan membandingkan daftar entity: `516` entity unik, selisih terhadap HEAD **kosong**.

Aturan `.codex/DATABASE_RULES.md` sudah menyatakan hal ini sebelumnya — *"Jangan membuat,
menghapus, mereset, menulis ulang, atau menjalankan migration tanpa wewenang yang berlaku dan
target yang jelas batasnya."* Untuk membuang migration percobaan pada langkah verifikasi
berikutnya saya tidak lagi memakai `ef migrations remove`, melainkan menghapus berkasnya dan
memulihkan snapshot dari salinan yang diambil lebih dulu.

## 6. Bukti verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | `Build succeeded`, `0 Error(s)`, `135 Warning(s)` | `PASS` | 135 warning seluruhnya nullability `CS8619` pre-existing di modul HR; tidak satu pun pada berkas yang disentuh |
| Test murni tanpa database | `Failed: 0, Passed: 21, Total: 21` | `PASS` | `LaboratoryAuthorityTests` dan `ClinicalFinancialAuthorityTests`; jumlah identik dengan sebelum rename, jadi tanpa regresi |
| Gerbang fail-closed database test | `18` test berhenti dengan `BLOCKED_BY_TEST_DB_CONFIGURATION` | `PASS` | Berhenti di `ResolveDedicatedTestConnectionString()` sebelum koneksi dibuka; tanpa fallback, tanpa `Database.Migrate()` |
| `dotnet ef migrations list --no-connect` | Kedua migration terdaftar | `PASS` | Keluaran menegaskan *"Pending status not shown"* — tidak ada koneksi database yang dibuka |
| Kesesuaian model terhadap snapshot | Selisih **nol** pada seluruh objek yang disentuh | `PASS` | Migration percobaan menghasilkan `0` referensi ke `ClinicalMilestoneFact`, `LabSpecimen`, `LabTransitionHistory`, `MstLabRejectionReason`, dan `LabOrder` |
| Referensi nama lama tersisa | `0` | `PASS` | Pencarian `TrxLab` dan `TrxClinicalMilestoneFact` pada `Areas/`, `Repositories/`, `Tests/`, `Migrations/` |

Tidak ada database yang disentuh selama seluruh remediasi ini.

## 7. `QBE-MOD-002` tetap terbuka — perlu keputusan pemilik

Penamaan sudah benar. **Kepemilikan modul belum sah.**

`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `18`:

```
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | PLANNED |
```

`QBE-MOD-002` mewajibkan status `ACTIVE` sebelum entity persisted pertama. Menaikkan status
adalah tindakan governance yang memerlukan otoritas bernama. Presedennya ada pada change log
registry baris `31`:

> `2026-08-24 | InPatientManagement / Inp | PLANNED -> ACTIVE | Muhammad Hamzah, blueprint RWI-BP-001 decision RWI-DEC-068. Lifts the QBE-MOD-002 bar on creating Inp* operational entities. Database execution outside local and deployment remain separate authorities.`

Entry sepadan untuk Laboratorium akan berbunyi:

```
| 2026-08-26 | LaboratoryManagement / `Lab` | `PLANNED` -> `ACTIVE` | <nama pemberi otoritas>,
  blueprint `RJ-BIL-BP-001` decision `RJ-BIL-GATE-DEC-003`. Lifts the QBE-MOD-002 bar on creating
  `Lab*` operational entities. Database execution outside local and deployment remain separate
  authorities. |
```

Baris itu **tidak saya tuliskan**. Mengisi nama pemberi otoritas berdasarkan tebakan sama saja
memalsukan catatan persetujuan. Baris ini menunggu pemilik.

Perlu dicatat bahwa kondisi serupa sudah ada sebelumnya dan bukan akibat pekerjaan ini:
`LabOrder` dibuat oleh `20260815103436_initializeLabOrder` ketika `Lab` sudah berstatus `PLANNED`,
dan 15 entity `Opr*` ada di HEAD sementara `Opr` juga masih `PLANNED`.

## 8. Tiga temuan tingkat tim

Ketiganya di luar cakupan task ini dan memerlukan keputusan terpisah, tetapi berdampak pada semua
orang.

### 8.1 Snapshot model berayun antarbranch

Setiap migration menyimpan snapshot model lengkap pada `Designer.cs`-nya. Bila seorang developer
membuat migration di branch yang belum memuat entity rekannya, snapshot itu merekam model yang
**tidak lengkap**. Jejaknya terlihat jelas pada referensi `ClinicalMilestoneFact`:

| Migration | Jumlah referensi |
| --- | --- |
| `...074649_AddClinicalMilestoneFactHandoff` | `5` |
| `...080052_AddBackendBillingDanKasirPart2` | `0` |
| `...080430_StoreClinicalFactSnapshotAsText` | `3` |
| `...082058_EditMaxLeghtModuleCode` | `0` |
| `...095353_CreateInpatientTransactionTables` | `0` |

Snapshot yang tercatat pada HEAD benar, karena merge diselesaikan dengan versi yang lengkap.
Bahayanya muncul pada perintah yang membaca `Designer.cs` migration terakhir — persis yang
terjadi pada bagian `5`.

### 8.2 Model saat ini sudah menyimpang dari snapshot, dan bukan karena task ini

Migration percobaan pada bagian `6` menunjukkan tiga tabel master yang **ada sebagai entity di
kode tetapi belum punya migration**: `MstRegister`, `MstRoomChargePolicy`, dan `MstTaxRule`,
disertai 45 pasang `DropForeignKey`/`AddForeignKey`. Ketiganya milik modul lain. Membuatkan
migration untuk modul orang lain bukan wewenang task ini, sehingga hanya dilaporkan.

### 8.3 Build memakan 22 menit

Folder `Migrations/` berisi **4.522.501 baris** dalam `205` berkas, karena setiap `Designer.cs`
memuat salinan penuh snapshot model — lima berkas terbesar masing-masing di atas 90.000 baris.
Setiap developer membayar ini pada setiap build. Penggabungan migration lama akan memangkasnya,
dan itu keputusan tim.

---

## Ringkasan field laporan

- **API CONTRACT IMPACT**: `NONE`. Tidak ada route, request, response, atau permission yang berubah.
- **DATABASE IMPACT**: satu migration disunting sebelum pernah diterapkan; satu migration rename
  baru ditulis dan **belum diterapkan**. Tidak ada database yang disentuh.
- **SECURITY IMPACT**: `NONE` pada permukaan authorization. Gerbang fail-closed database test
  terbukti masih menutup.
- **VISUAL REFERENCE**: `NOT REQUIRED`
- **MANUAL TEST**: `NOT FEASIBLE` — memerlukan database test khusus yang belum tersedia
- **INCIDENTAL CHANGES**: pemulihan `ApplicationDbContextModelSnapshot.cs` yang rusak akibat
  `ef migrations remove`, beserta 47 entity milik tiga modul. Rinciannya pada bagian `5`.
- **INTERRUPTIONS**: tiga proses `dotnet build` atas project yang sama berjalan bersamaan akibat
  pemanggilan latar saya sendiri, saling mengunci `obj/` selama sekitar 70 menit tanpa kemajuan.
  Dipulihkan dengan menghentikan ketiganya beserta `VBCSCompiler`, lalu satu build bersih.
- **KNOWN ISSUES**: `QBE-MOD-002` terbuka (bagian `7`); drift model milik modul lain (bagian `8.2`).
- **GIT STATUS**: tidak ada `commit`, `push`, `merge`, `deploy`, maupun penerapan migration.
- **NEXT RECOMMENDED STEP**: keputusan pemilik atas baris registry `Lab`, lalu `RJ-BIL-BE-007`.
