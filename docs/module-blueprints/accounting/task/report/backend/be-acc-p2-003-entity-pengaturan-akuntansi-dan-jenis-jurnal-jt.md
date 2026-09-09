# Laporan Perubahan Backend — `BE-ACC-P2-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-003` |
| Judul | Entity pengaturan akuntansi dan jenis jurnal `JT` |
| Slice | Gelombang `P2-0a` — bagian `P2-0` yang mandiri dan tidak menyentuh modul Finance |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-003` |
| Trace | `ACC-DEC-053`, `ACC-DEC-054`; `FR-P2-032`, `FR-P2-033`; kamus data bagian 17 dan 19 |
| Contract version | `ACC-API-0.8`, `ACC-STATE-0.2`, `ACC-VALIDATION-0.6` — blueprint `ACC-BP-001` revisi 11, Phase 2 `approved` Rizki 8 September 2026; roadmap revisi 2 `APPROVED` 9 September 2026 |
| Dependency | **Tidak ada.** Roadmap mencatat kartu ini tanpa dependency |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 2 (lebih dari 20), berkas diubah 1 (8 berkas), logika bisnis 0, kontrak API 0, database 2, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, dan `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `cc59164` |
| Tanggal | 9 September 2026 |
| Status | **`DONE`** — 4 dari 4 acceptance lulus, nol migration dibuat, database tidak disentuh |

---

## Validasi baseline sebelum mulai

| Yang diperiksa | Hasil |
| --- | --- |
| Roadmap memberi wewenang task ini | Ya — `BE-ACC-P2-003` berstatus `READY`, kolom Dependency kosong |
| Blueprint Phase 2 `approved` | Ya — revisi 11, disetujui Rizki 8 September 2026 |
| Branch sesuai pemegang modul | Ya — `rizkiG`, branch backend Rizki |
| Working tree bersih? | **Ya, sepenuhnya bersih.** `BE-ACC-P2-001` dan `BE-ACC-P2-002` sudah masuk commit `cc59164`, berbeda dari keadaan saat `002` dikerjakan |
| Kesimpulan | Aman dilanjutkan |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AccountingManagement / Accounting` |
| Submodule | `MasterData / Configuration` — **folder baru** |
| Prefix | `Acc` |
| Pemilik | Rizki |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30 |
| Keberlakuan | `NEW CODE` untuk entity dan configuration; `TOUCHED LEGACY` untuk seeder dan `ApplicationDbContext` |
| QBE yang berlaku | `QBE-MOD-002` (modul terdaftar, lolos), `QBE-MOD-003` / `QBE-NAM-004` (lihat catatan di bawah), `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001` sampai `004` (entity berawalan `Acc`, bukan `Trx`), `QBE-DB-001` / `QBE-DB-002` (tidak berlaku — nol pekerjaan database) |
| Hasil checker | **`PASS`** — `tooling/qbe/Invoke-QbeConformanceCheck.ps1` atas kedelapan berkas: `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0` |

**Kenapa `MasterData/Configuration/` tidak menuntut baris registry tersendiri.** Registry mendaftarkan
kepemilikan pada tingkat **Module**, yaitu `AccountingManagement`, beserta prefix `Acc`. Folder di
dalamnya tidak punya baris masing-masing: `MasterData/ChartOfAccount`, `MasterData/JournalType`,
`AccountingPeriod`, `JournalManagement`, dan `RecurringJournal` semuanya sudah memuat model
persisted `Acc*` tanpa entri sendiri. `MasterData/Configuration/` mengikuti preseden yang sama, dan
alasan yang sama sudah dicatat `BE-ACC-P2-002` untuk folder `RecurringJournal`. **Nol berkas
registry disunting** — registry adalah milik lead, dan menambahnya sendiri untuk meloloskan
pekerjaan sendiri adalah hal yang justru dilarang.

**Selisih dua salinan registry** tetap seperti dilaporkan `BE-ACC-P2-001` dan `002`: salinan suite
skill v1.15.0 **tidak** memuat baris `Acc`, salinan backend memuatnya. Sesuai aturan skill, source
repository target yang berlaku dan selisihnya dilaporkan. `ACC-DEP-007` **tetap terbuka dan tetap
milik lead**.

---

## 1. Masalah yang diperbaiki

Dua hal yang belum ada, dan keduanya menghalangi tutup tahun.

**Pertama, sistem belum tahu ke akun mana laba dibukukan.** Pada akhir tahun, seluruh akun
pendapatan dan beban dinolkan, lalu selisihnya — labanya — dipindahkan ke satu akun ekuitas
bernama laba ditahan. Sampai sekarang tidak ada tempat untuk mencatat *akun mana* itu.

Menebaknya tidak bisa. Syarat "berjenis Ekuitas" saja tidak cukup, karena satu badan hukum lazim
punya beberapa akun ekuitas sekaligus — modal disetor, laba ditahan, dan laba tahun berjalan
ketiganya berjenis ekuitas. Bila sistem menebak, ia akan menebak dengan meyakinkan dan salah:
jurnal penutupnya tetap seimbang, tidak ada error apa pun, dan angka yang salah itu terbawa ke
tahun berikutnya sebagai saldo awal.

**Kedua, belum ada jenis jurnal untuk jurnal penutup.** Master jenis jurnal berisi empat baris:
`JU` jurnal umum, `JP` penyesuaian, `JB` pembalik, dan `SA` saldo awal. Jurnal penutup tahun tidak
cocok masuk keempatnya. Tanpa jenisnya sendiri, jurnal penutup akan bercampur dengan jurnal umum
biasa, dan laporan yang dikelompokkan menurut jenis jurnal berhenti dapat dibaca.

Task ini menyediakan **tempatnya**, bukan perilakunya. Endpoint pengisian pengaturan adalah
`BE-ACC-P2-009`, dan penyusunan jurnal penutupnya `BE-ACC-P2-010`.

---

## 2. Proses bisnis

### 2.1 Menetapkan akun laba ditahan

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pemilik proses akuntansi | Membuat akun laba ditahan di daftar akun, misalnya `3-2001 Laba Ditahan`, berjenis `Equity` dan menerima transaksi |
| 2 | Pemilik proses akuntansi | Menunjuk akun itu sebagai akun laba ditahan badan hukumnya |
| 3 | Sistem | Menyimpan satu baris `AccAccountingConfiguration` untuk badan hukum tersebut |
| 4 | Sistem | Saat tutup tahun dijalankan, akun inilah yang dituju jurnal penutup |

Langkah 2 dan 3 adalah **`BE-ACC-P2-009`**, dan belum berjalan. Yang berdiri sekarang hanya
tempat penyimpanannya.

**Aturannya satu badan hukum satu pengaturan.** Ini bukan sekadar kerapian. Bila dua pengaturan
dapat berdiri untuk badan hukum yang sama dan menunjuk akun berbeda, tutup tahun akan memilih
salah satunya tanpa aturan yang jelas — dan laba mendarat di akun yang salah tanpa error apa pun.
Karena itu penjaganya dipasang di **database** sebagai unique index, bukan hanya di service.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi | Ditegakkan di mana |
| --- | --- | --- |
| Akun yang ditunjuk bukan `Equity` | Ditolak `422` | Service, `BE-ACC-P2-009` |
| Akun yang ditunjuk adalah akun induk yang tidak menerima transaksi | Ditolak `422` | Service, `BE-ACC-P2-009` |
| Badan hukum sudah punya pengaturan lalu dibuatkan lagi | Ditolak database | **Task ini** — unique index |
| Pengaturan lama dihapus lunak lalu badan hukum itu dibuatkan pengaturan baru | **Diizinkan** | **Task ini** — index berfilter `IsDelete = false` |
| Akun laba ditahan dicoba dihapus selagi masih dipakai pengaturan | Tertahan | **Task ini** — relasi `Restrict` |

### 2.3 Jenis jurnal `JT`

Seeder jenis jurnal kini memuat **lima** baris, bertambah satu:

| Kode | Nama | Awalan nomor | Butuh persetujuan | Jenis sistem |
| --- | --- | --- | :---: | :---: |
| `JU` | Jurnal Umum | `JU` | Ya | Tidak |
| `JP` | Jurnal Penyesuaian | `JP` | Ya | Tidak |
| `JB` | Jurnal Pembalik | `JB` | Ya | Ya |
| `SA` | Saldo Awal | `SA` | Ya | Ya |
| **`JT`** | **Jurnal Tutup Tahun** | **`JT`** | **Ya** | **Ya** |

`JT` berstatus **jenis sistem**, sama seperti `JB` dan `SA`, karena ketiganya lahir dari langkah
yang dikendalikan sistem. Akibatnya kode dan awalan nomornya tidak dapat diubah admin lewat layar
`BE-ACC-008`. Bila awalan `JT` dapat diubah, nomor jurnal penutup yang sudah terbit berhenti cocok
dengan jenisnya.

`RequiresApproval` bernilai **`true`** mengikuti `ACC-DEC-053`: jurnal penutup **disusun sistem
sebagai `Draft`, disahkan manusia**. Bila nilainya `false`, jurnal penutup akan sah tanpa pernah
dilihat siapa pun.

**Seeder tetap idempoten.** Database yang seeder-nya sudah pernah dijalankan sebelum Phase 2 berisi
empat baris; menjalankannya lagi sekarang menambah **tepat satu** baris `JT` dan **tidak menyentuh**
keempat baris lama. Menjalankannya sekali lagi sesudah itu menambah **nol** baris. Ini yang diuji,
dan buktinya ada di bagian 5.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen:** roadmap Phase 2 kartu `BE-ACC-P2-003` dan `004`; laporan `be-acc-p2-002`;
`erd/data-dictionary.md` bagian 17, 18, 19; `02-backend-architecture.md` bagian 18 beserta peta
folder; `roadmap/requirement-traceability-phase2.md`; `00-interview-decisions.md` `ACC-DEC-053`
dan `054`; `AGENTS.md`; `rules/GLOBAL_RULES.md`, `TASK_RULES.md`, `TASK_CLASSIFICATION.md`,
`DATABASE_RULES.md`, `REPORT_TEMPLATE.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` kedua salinan.

**Source:** `AccountingMasterDataSeeder.cs`, `AccJournalType.cs`, `AccJournalTypeService.cs`,
`JournalTypeController.cs`, `JournalTypeDtos.cs`, `AccPeriodClosingApproval.cs` beserta
configuration-nya, `AccChartOfAccount.cs` beserta configuration-nya,
`AccRecurringJournalTemplate.cs` beserta configuration-nya, `ApplicationDbContext.cs`,
`IdentityModel.cs`, `TestDatabase.cs`, `ClinicalAssessmentPolicyMasterTests.cs`,
`InpatientClinicalSchemaTests.cs`, `QuilvianSystemBackend.csproj`, `QuilvianSystemBackend.sln`.

### 3.2 Berkas yang berubah

Delapan berkas — empat baru, empat diperbarui.

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/MasterData/Configuration/Models/AccAccountingConfiguration.cs` | **Baru.** Entity pengaturan akuntansi per badan hukum |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccAccountingConfigurationConfiguration.cs` | **Baru.** Nama tabel, kolom, dua relasi `Restrict`, unique index, index pencarian |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/AccountingMasterDataSeederTests.cs` | **Baru.** Lima uji idempotensi seeder |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/AccAccountingConfigurationSchemaTests.cs` | **Baru.** Tiga uji bentuk tabel terhadap kamus data bagian 17 |
| `Repositories/ApplicationDbContext.cs` | Satu `using`, satu `DbSet` di dalam region `MASTER DATA` yang sudah ada. **Dua baris**, nol baris lain tersentuh |
| `Areas/Corporate/AccountingManagement/MasterData/Seeders/AccountingMasterDataSeeder.cs` | Satu baris definisi `JT`, ditambah pembaruan komentar yang menyebut "empat" |
| `Areas/Corporate/AccountingManagement/MasterData/JournalType/Services/AccJournalTypeService.cs` | **Komentar saja** — lihat bagian 3.4 |
| `Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/JournalTypeController.cs` | **Komentar saja** — lihat bagian 3.4 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint dibuat, diubah bentuknya, atau dihapus.** Yang berubah adalah **hasil** satu endpoint yang sudah ada — lihat bagian 4 |
| Database | **Dampak schema: ada.** Satu tabel baru `AccAccountingConfiguration` berdiri di model EF. **Nol migration dibuat, nol perintah `dotnet ef` dijalankan, database tidak disentuh.** Penerapannya adalah `BE-ACC-P2-004` yang berstatus **`GATED`** |
| Keamanan/Auth | `NOT APPLICABLE` — nol controller, nol action, nol atribut `[AccessAction]` atau `[AccessPermission]` dibuat maupun diubah. Hak akses endpoint `seed` yang perilakunya berubah tetap `JournalType : Create` seperti sebelumnya |

### 3.4 Perubahan pada berkas di luar cakupan — komentar saja

Cakupan roadmap menyebut `Models/AccAccountingConfiguration.cs`, satu configuration, satu `DbSet`,
dan penambahan baris `JT` pada `AccountingMasterDataSeeder`. Dua berkas di luar daftar itu ikut
disunting, **komentarnya saja**, karena keduanya menyatakan jumlah yang menjadi tidak benar akibat
task ini:

- `AccJournalTypeService.cs` — *"Mengisi **empat** jenis jurnal bawaan"* → *"**lima**"*;
- `JournalTypeController.cs` — kalimat yang sama, ditambah penyebutan sumber baris kelima.

**Nol perubahan perilaku.** Yang disunting hanya blok dokumentasi XML; badan method, atribut,
route, dan hak akses tidak disentuh. Dicatat di sini karena perubahan sekecil apa pun di luar
cakupan wajib disebutkan — mengikuti preseden `BE-ACC-P2-002` yang memperbarui komentar
`AccJournalLineConfiguration.cs` dengan alasan yang sama.

Komentar `JournalTypeService.cs` juga menyebut *"idempotensinya dijamin seeder dan dibuktikan
`AccountingMasterDataSeederTests`"* — sebuah berkas yang selama ini **belum pernah ada**. Task ini
membuatnya, sehingga kalimat itu berhenti menjadi janji dan menjadi pernyataan yang benar.

### 3.5 Dua keputusan configuration yang perlu dijelaskan

**Unique index memakai filter `IsDelete = false`.** Ini **berlawanan arah** dengan penjaga terbit
ganda `AccRecurringJournalRun` pada `BE-ACC-P2-002`, yang sengaja **tanpa** filter — dan
perbedaannya disengaja, bukan kelalaian.

Pada `AccRecurringJournalRun`, menghapus lunak catatan penerbitan lalu menerbitkan ulang akan
menghasilkan **jurnal kedua untuk bulan yang sama**; akibatnya nyata di buku besar, jadi filternya
dibuang. Pada pengaturan akuntansi akibatnya berlawanan: pengaturan yang sudah dihapus lunak tidak
menimbulkan akibat akuntansi apa pun, sedangkan menahannya akan **mengunci badan hukum itu dari
membuat pengaturan baru selamanya** — artinya tutup tahun mati karena baris yang sudah tidak
dipakai. Yang perlu dijaga cukup "hanya satu yang berlaku sekarang", dan itulah yang dijaga.

**Kedua relasi memakai `Restrict`.** Akun laba ditahan yang masih ditunjuk pengaturan tidak boleh
terhapus, dan pengaturan tidak boleh ikut hilang bersama induknya. Pola yang sama dipakai
`AccChartOfAccount` terhadap `MstLegalEntity`.

### 3.6 Dampak pada `ApplicationDbContext`

Satu `using` dan satu `DbSet` di dalam region `CORPORATE - ACCOUNTING MANAGEMENT - MASTER DATA`
yang **sudah ada**; nol region baru dibuat. Configuration terdaftar otomatis lewat
`ApplyConfigurationsFromAssembly`; **nol registrasi manual, nol perubahan pada `Program.cs`**.

---

## 4. Dokumentasi endpoint

**Nol endpoint dibuat.** Yang berubah adalah **hasil** satu endpoint yang sudah ada, karena daftar
bawaan seeder yang dipanggilnya bertambah satu baris.

#### Corporate / Accounting / Master Data / Journal Type

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/master-data/journal-types/seed` | Mengisi jenis jurnal bawaan. **Sebelum task ini mengisi 4 baris, sesudahnya 5** — bertambah `JT` Jurnal Tutup Tahun. Bentuk request dan response **tidak berubah** | `JournalType : Create` |

**Bentuk `JournalTypeSeedResponse` tidak berubah**, hanya angkanya. Pada database yang seeder-nya
sudah pernah dijalankan, panggilan berikutnya mengembalikan `Inserted = 1` dan `Skipped = 4`; pada
database yang belum, `Inserted = 5` dan `Skipped = 0`.

**Endpoint ini belum pernah dijalankan terhadap database mana pun oleh task ini.** DoD roadmap
menyatakan *"seeder belum dijalankan"*, dan itu dipatuhi.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false --no-incremental` | `0 Error(s)`, `145 Warning(s)`, 00:02:15 | `PASS` | Keluaran perintah. **145 adalah angka yang sama persis** dengan build `BE-ACC-P2-002`, jadi kedelapan berkas menyumbang **nol warning baru** |
| Uji idempotensi seeder, 5 uji | `Failed: 0, Passed: 5` | `PASS` | `AccountingMasterDataSeederTests` |
| Uji bentuk tabel, 3 uji | `Failed: 0, Passed: 3` | `PASS` | `AccAccountingConfigurationSchemaTests` |
| Seluruh project `UnitTests.Sqlite`, 453 uji | `Failed: 3, Passed: 450` | `EXISTING / ENVIRONMENT ISSUE` | Ketiganya `SwaggerDocumentationTests` milik `MedicalRecordManagement` — lihat di bawah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` atas 8 berkas | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran checker |
| `git status --short -- Migrations/` | Nol berkas | `PASS` | Snapshot EF tidak tersentuh |

### Rincian uji idempotensi seeder

| Uji | Yang dibuktikan | Hasil |
| --- | --- | :---: |
| `PemanggilanPertama_MengisiLimaJenisJurnal` | Master kosong terisi tepat lima kode: `JB`, `JP`, `JT`, `JU`, `SA` | `PASS` |
| `DipanggilDuaKali_TetapLimaJenisJurnal` | **Acceptance 2.** Panggilan kedua `Inserted = 0`, `Skipped = 5`; isi tabel tetap 5 baris, bukan 6 dan bukan 10 | `PASS` |
| `MasterEmpatBarisLama_HanyaBertambahJt` | Master berisi empat baris lama hanya bertambah `JT`; **`Id` keempat baris lama tidak berubah**, jadi tidak ada yang tertimpa diam-diam | `PASS` |
| `JenisJurnalJt_MenuntutPersetujuanDanBerjenisSistem` | **Acceptance 3.** `RequiresApproval = true`, `IsSystemType = true`, nama dan awalan sesuai kamus data bagian 19 | `PASS` |
| `MasterMilikSumberLain_TidakDisentuh` | Penjaga kepemilikan tidak rusak oleh baris kelima: master berisi kode asing `GJ` tetap tidak disentuh | `PASS` |

### Tiga uji yang gagal — bukan akibat task ini

Ketiganya `SwaggerDocumentationTests` di `MedicalRecordManagement`, dengan pesan yang sama:
*"Berkas dokumentasi XML tidak ditemukan ... `QuilvianSystemBackend.xml`."*

**Dibuktikan bergantung konfigurasi, bukan pada perubahan task ini:**

1. `QuilvianSystemBackend.csproj` baris 33 menetapkan `<GenerateDocumentationFile>false</GenerateDocumentationFile>`
   di dalam `PropertyGroup` bersyarat `Configuration == Release`, dengan komentar
   *"Release dimatikan untuk mempercepat build"*. Berkas XML itu **memang tidak pernah dibuat**
   pada Release.
2. Berkas `.csproj` **tidak disentuh task ini** — `git diff --stat QuilvianSystemBackend.csproj`
   kosong.
3. Ketiga uji yang sama dijalankan pada `Debug`, tempat `GenerateDocumentationFile` bernilai
   `true`: **`Failed: 0, Passed: 3`**.

Kesimpulannya `SwaggerDocumentationTests` **tidak akan pernah lulus pada Release**, termasuk di CI
bila CI menjalankan Release. Ini cacat yang nyata, tetapi milik modul lain dan di luar cakupan
task ini — **dilaporkan, tidak diperbaiki**. Dicatat sebagai temuan pada bagian 7.

**Tidak dijalankan:**

- `dotnet ef migrations add` — `BE-ACC-P2-004` berstatus **`GATED`** dan menuntut instruksi
  eksplisit terpisah dari owner beserta Migration Coordination Gate.
- `dotnet ef database update` dan seluruh perintah database — tidak diberi wewenang, dan DoD
  roadmap justru mensyaratkan database tidak disentuh.
- Endpoint `POST .../journal-types/seed` terhadap database sungguhan — DoD menyatakan
  *"seeder belum dijalankan"*.
- Uji integrasi PostgreSQL — tidak diminta task ini; unique index `(LegalEntityId)` berfilter
  dibuktikan pada tingkat model EF, dan pembuktiannya terhadap PostgreSQL sungguhan menyusul
  bersama `BE-ACC-P2-009`.

Uji manual: `NOT APPLICABLE` — task ini tidak menghasilkan permukaan yang dapat dicoba pengguna.

---

## 6. Acceptance criteria dan Definition of Done

### Acceptance criteria

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Unique `(LegalEntityId)` terpasang | **Terpenuhi** | `AccAccountingConfigurationConfiguration.cs`, `HasIndex(x => x.LegalEntityId).IsUnique().HasFilter(...)`; diuji `SatuBadanHukum_HanyaSatuPengaturanYangHidup` yang membaca model EF sungguhan |
| 2 | Seeder tetap **idempoten** — dipanggil dua kali tetap menghasilkan 5 jenis jurnal, bukan 6 | **Terpenuhi** | `DipanggilDuaKali_TetapLimaJenisJurnal` — panggilan kedua `Inserted = 0`, `Skipped = 5`, isi tabel tetap 5 baris. Diperkuat `MasterEmpatBarisLama_HanyaBertambahJt` |
| 3 | `JT` bernilai `RequiresApproval = true` | **Terpenuhi** | `JenisJurnalJt_MenuntutPersetujuanDanBerjenisSistem` |
| 4 | Build lulus | **Terpenuhi** | `0 Error(s)`, 145 warning, seluruhnya pre-existing |

**Empat dari empat terpenuhi.**

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Build lulus | **Ya** — `0 Error(s)` |
| **Nol migration** | **Ya** — `dotnet ef migrations add` tidak dijalankan |
| Snapshot tidak berubah | **Ya** — `git status --short -- Migrations/` menghasilkan nol berkas |
| **Seeder belum dijalankan** | **Ya** — nol pemanggilan terhadap database mana pun; yang dijalankan hanya uji di atas SQLite dalam memori yang dibuang setiap selesai |
| Database tidak disentuh | **Ya** — nol perintah database dijalankan |
| Laporan tracked ada | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `✅` pada kartu task, baris `Status`, tabel ringkasan, dan baris dependency `BE-ACC-P2-004` |
| `requirement-traceability-phase2.md` diperbarui | **Ya** — `FR-P2-032` dan `FR-P2-033`, serta `task_selesai` dinaikkan menjadi 3 |

**Nol butir DoD dikecualikan.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 145 warning solution, seluruhnya pre-existing di project `Tests/` dan konflik versi `Microsoft.Extensions.DependencyModel`. **Nol berasal dari task ini** — angkanya sama persis dengan sebelum task ini dikerjakan |
| Perubahan sampingan | `NONE` — nol berkas tergenerasi, nol berkas dipulihkan atau dihapus |
| Interupsi | `NONE` |
| Status Git | Lihat di bawah |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **`SwaggerDocumentationTests` tidak dapat lulus pada Release** — `GenerateDocumentationFile` sengaja dimatikan pada Release oleh `.csproj`, sedangkan uji itu menuntut berkas XML-nya ada. **Temuan baru task ini**, milik modul `MedicalRecordManagement`, dilaporkan tanpa diperbaiki | Owner Backend / pemilik modul Medical Record |
| 2 | Delta `Cascade` lawan `Restrict` dari `BE-ACC-P2-001` — kamus data bagian 16 menulis `Cascade`, diimplementasikan `Restrict`. **Masih menunggu ratifikasi**, tidak tersentuh task ini | Rizki |
| 3 | Selisih dua salinan registry (`ACC-DEP-007`) — dilaporkan, tidak diperbaiki | Lead |
| 4 | Nol test backend Accounting (`ACC-TD-016`) — **sebagian berkurang**: task ini menambah 8 uji pertama modul Accounting. Yang masih kosong adalah jaring regresi `AccJournalService` yang akan disentuh `BE-ACC-P2-007` dan `010` | Rizki |
| 5 | Nama berkas laporan ini memakai bentuk **slug** (`be-acc-p2-003-...`), mengikuti 17 laporan sebelumnya dan tautan pada roadmap, sedangkan `REPORT_TEMPLATE.md` bagian 1 mensyaratkan nama berkas **sama persis dengan task ID** (`BE-ACC-P2-003.md`). Konvensi modul dipertahankan supaya tautan roadmap tetap seragam; **selisihnya dilaporkan, bukan diperbaiki sepihak** | Lead / Rizki |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Tabel belum ada di database** | `AccAccountingConfiguration` baru berdiri di model EF. Setiap kode yang menyentuhnya akan gagal saat dijalankan sampai `BE-ACC-P2-004` diterapkan owner. Ini memang bentuk yang dikehendaki roadmap, bukan cacat |
| **Nilai `Restrict` bergantung ratifikasi** | Bila owner kelak menetapkan `Cascade` untuk `BE-ACC-P2-001`, keputusan yang sama perlu ditinjau untuk tabel ini sebelum migration dibuat — sesudah migration diterapkan, mengubahnya menuntut migration kedua |
| ~~`JT` belum ada di database~~ | **DITUTUP.** **Diisi 9 September 2026** lewat `AccJournalTypeService.SeedAsync` sebagai `superadmin`: `Inserted: 1, Skipped: 4`, master menjadi 5 baris. Baris `JT` bernilai `RequiresApproval = true` dan `IsSystemType = true`, dan keempat baris lama tidak tersentuh |

### Status Git

```text
 M Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/JournalTypeController.cs
 M Areas/Corporate/AccountingManagement/MasterData/JournalType/Services/AccJournalTypeService.cs
 M Areas/Corporate/AccountingManagement/MasterData/Seeders/AccountingMasterDataSeeder.cs
 M Repositories/ApplicationDbContext.cs
?? Areas/Corporate/AccountingManagement/MasterData/Configuration/
?? Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccAccountingConfigurationConfiguration.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/
```

**Nol commit, push, stage, merge, atau rebase dilakukan.** Seluruh perubahan tertinggal di working
tree untuk ditinjau owner.

### Langkah berikutnya

Ketiga entity gelombang mandiri kini **lengkap** — `001` ✅, `002` ✅, `003` ✅. Dua jalan terbuka:

1. **`BE-ACC-P2-011`** kolom control account pada daftar akun. **Tanpa dependency, dan harus
   selesai sebelum `BE-ACC-P2-004`** karena kolomnya wajib masuk migration yang sama. Ini
   satu-satunya task yang masih menghalangi migration.
2. **`BE-ACC-P2-004`** migration gelombang mandiri — **`GATED`**. Ia menunggu `011` selesai
   **dan** instruksi eksplisit terpisah dari owner beserta Migration Coordination Gate.
   **Jangan dijalankan hanya karena task ini selesai.**

Urutan yang disarankan: kerjakan `011` lebih dahulu, lalu ajukan `004` ke owner sebagai satu
permintaan yang lengkap.
