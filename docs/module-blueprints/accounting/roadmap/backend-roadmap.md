# Roadmap Delivery Backend — Accounting

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 5
blueprint_status: approved
roadmap_revision: 2
roadmap_status: APPROVED
approved_by: [Rizki]
approved_at: 2026-09-01
source_backend: aa837d784ff51cb2b889cf975ada3a204018f1f5
source_frontend: 31a82c8052a3c59445ae49e6f1ccce2bf717d6c0
decision_revision: 1.1
contracts: [ACC-API-0.1, ACC-STATE-0.1, ACC-VALIDATION-0.2, ACC-INTEGRATION-0.2, ACC-PERMISSION-0.1, ACC-TEST-0.1, ACC-MVP-0.1, ACC-XMOD-0.1]
shared_engineering_rules: [QBE-MIG-001, QBE-MIG-002]   # PROPOSED — lihat ../06-shared-migration-coordination-rule.md
```

## Baca ini lebih dahulu

**FINAL OWNER APPROVAL sudah diberikan** Rizki pada 1 September 2026 atas `ACC-BP-001` revisi 5.
Roadmap ini berstatus `APPROVED`.

Approval itu memberi wewenang **source model persisted Accounting**. Ia **tidak** otomatis
mengizinkan `dotnet ef migrations add`, `dotnet ef database update`, perubahan shared database,
deployment, production activation, commit, maupun push. Semuanya tetap wewenang terpisah, dan
`BE-ACC-006` punya gerbangnya sendiri.

**Mulai satu task hanya atas instruksi eksplisit owner.** Approval roadmap bukan perintah jalan.

### Jangan tertukar dua penomoran ini

| Pola | Artinya | Contoh |
|---|---|---|
| `BE-ACC-###` dan `FE-ACC-###` | **Task** pada roadmap ini | `BE-ACC-003` |
| `ACC-FE-###` | **Keputusan wewenang UI** pada arsitektur frontend | `ACC-FE-001` letak menu |

Keduanya mirip dan mudah tertukar. `BE`/`FE` di **depan** berarti task; di **belakang** berarti
keputusan.

### Dua jenis readiness, jangan disamakan

Sejak 1 September 2026 roadmap ini membedakan dua hal yang sebelumnya menyatu:

| Status | Artinya |
|---|---|
| `ROADMAP_READY` | Dependency roadmap terpenuhi dan task boleh dijadwalkan |
| `EXECUTION_READY` | `build-module-backend` benar-benar dapat menyelesaikannya sekarang |

Keduanya dapat berbeda. Sebuah task bisa `ROADMAP_READY` tetapi belum `EXECUTION_READY` bila
governance yang wajib dibaca skill tidak terbaca, atau bila wewenang tulisnya belum turun.

**Bukti keadaan sekarang.** `build-module-backend` mensyaratkan *Backend Governance Preflight*
yang bersifat **membaca dokumen**, bukan menjalankan script. Ia memblokir hanya bila `AGENTS.md`
backend atau dokumen `rules/backend/` tidak dapat dibaca. Tidak ada satu pun aturan di suite yang
mewajibkan `Invoke-QbeConformanceCheck.ps1` dijalankan sebagai syarat eksekusi task.

Karena itu `ACC-DEP-007` **tidak** membuat task menjadi `BLOCKED` untuk eksekusi. Ia mematikan
**gerbang CI saat merge**, bukan kemampuan skill menulis kode.

### Catatan eksekusi yang mengikat seluruh task

| Hal | Ketentuan |
|---|---|
| Build, `dotnet ef migrations add`, `dotnet ef database update` | **Dijalankan sendiri oleh owner modul secara manual.** Tidak satu pun task boleh mengasumsikan agent menjalankannya |
| Database pengembangan | Dipakai bersama satu tim. Menulis ke sana mengubah data yang sedang dipakai orang lain |
| QBE preflight dan kesesuaian engineering | Diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen engineering canonical, bukan dari roadmap ini |
| Implementasi | Wajib lewat `quilvian-engineering-skills:build-module-backend`, satu task per pemanggilan |

---

## Ringkasan gelombang

| Gelombang | Task | Status | Syarat mulai |
|---|---|---|---|
| `MVP-0` Fondasi | `BE-ACC-001` sampai `BE-ACC-006` | 3 `EXECUTION_READY`, 3 berantai | Blueprint **disetujui** |
| `MVP-1` Jurnal manual | `BE-ACC-007` sampai `BE-ACC-011` | `BLOCKED` berantai | `MVP-0` selesai |
| `MVP-2` Buku besar | `BE-ACC-012` | `BLOCKED` berantai | `MVP-1` selesai |
| `MVP-3` Koreksi dan saldo awal | `BE-ACC-013`, `BE-ACC-014` | `BLOCKED` berantai | `MVP-2` selesai |

**Tiga task kini `EXECUTION_READY`:** `BE-ACC-001`, `BE-ACC-002`, dan — sejak lifecycle `Acc`
menjadi `ACTIVE` (`ACC-DEC-038`) — `BE-ACC-003`, yang dependency-nya tinggal `BE-ACC-001`.

---

## `MVP-0` — Fondasi

### `BE-ACC-001` — Kerangka modul, enum, dan test harness

| Field | Isi |
|---|---|
| Outcome | Folder modul Accounting berdiri beserta seluruh enum-nya, dan ada test yang membuktikan struktur itu sesuai konvensi repository. Belum ada satu pun tabel bisnis |
| Trace | `ACC-DEC-001`, `ACC-DEC-002`; `EPIC ACC-01` sampai `ACC-08` (fondasi bersama) |
| Kontrak | `ACC-STATE-0.1` untuk nilai enum. Tidak ada endpoint |
| Reuse | `IdentityModel`, `ApiResponse<T>`, `PagedResult<T>`, pola folder `Areas/Corporate/HumanResource/` |
| Cakupan | Buat `Areas/Corporate/AccountingManagement/{MasterData/ChartOfAccount, MasterData/JournalType, JournalManagement, AccountingPeriod, GeneralLedger}` beserta subfolder `Controllers/`, `DTOs/`, `Models/`, `Services/`, `Enums/` yang kosong. Buat enam enum: `AccountType`, `NormalBalance`, `JournalStatus`, `JournalApprovalAction`, `JournalCorrectionType`, `AccountingPeriodStatus`. **Tidak** membuat berkas di `Models/` |
| Dependency | Tidak ada |
| Acceptance | (1) Build lulus. (2) Keenam enum ada dengan nilai persis seperti kontrak state. (3) Folder `Controllers/` memakai bentuk jamak. (4) Tidak ada satu pun berkas di `Models/` |
| Verifikasi | `dotnet build` oleh owner modul; pemeriksaan daftar berkas |
| Risiko/pemilik | Rendah. Owner Backend. Membuat folder tidak dilarang QBE-MOD-002; yang dilarang adalah entity persisted pertama |
| DoD | Enum lengkap, build lulus, laporan task berisi daftar berkas yang dibuat |
| **Status** | `ROADMAP_READY` setelah blueprint disetujui · **`EXECUTION_READY`** — tidak menyentuh `Models/`, jadi bebas `ACC-DEP-006`; governance skill terbaca, jadi bebas `ACC-DEP-007` |

### `BE-ACC-002` — Audit mekanisme hak akses badan hukum

| Field | Isi |
|---|---|
| Outcome | Cara sistem menentukan badan hukum mana yang menjadi hak seorang pengguna terdokumentasi berbasis bukti, sehingga penyaringan `LegalEntityId` dapat ditegakkan dengan benar |
| Trace | `ACC-DEC-037`; `NFR-005`; pertanyaan memblokir pada `04-prd-to-mvp.md` bagian 20 |
| Kontrak | `ACC-PERMISSION-0.1` bagian 5 |
| Reuse | Mekanisme `[AccessController]`, `[AccessPermission]`, `AccessTypes` yang sudah ada |
| Cakupan | **Read-only.** Telusuri bagaimana modul Corporate lain menyaring `LegalEntityId`, apakah hak itu melekat pada pengguna, peran, atau penugasan. Tulis hasilnya ke `evidence/02-legal-entity-authority.md`. Bila ternyata belum ada mekanismenya, laporkan sebagai temuan, jangan mengarang |
| Dependency | Tidak ada |
| Acceptance | (1) Berkas evidence berisi path dan simbol nyata beserta SHA. (2) Menyatakan tegas apakah mekanismenya sudah ada, atau belum ada dan perlu keputusan owner keamanan |
| Verifikasi | Berkas evidence dapat ditelusuri ulang oleh orang lain |
| Risiko/pemilik | Owner keamanan platform. **Bila hasilnya "belum ada", `BE-ACC-007` sampai `BE-ACC-014` tetap tertahan** sampai keputusan turun |
| DoD | Evidence tertulis, dan pertanyaan memblokir pada PRD ke MVP diperbarui statusnya |
| **Status** | `ROADMAP_READY` setelah blueprint disetujui · **`EXECUTION_READY`** — read-only, tanpa entity. Task ini yang menutup satu dari dua pertanyaan memblokir |

### `BE-ACC-003` — Entity daftar akun dan jenis jurnal

| Field | Isi |
|---|---|
| Outcome | Tabel `AccChartOfAccount` dan `AccJournalType` berdiri beserta configuration dan `DbSet`-nya |
| Trace | `ACC-DEC-022`, `ACC-DEC-023`, `ACC-DEC-024`, `ACC-DEC-037`, `ACC-DEC-010`; `FR-ACC-001` sampai `007` |
| Kontrak | `erd/01-chart-of-account.md`, `erd/data-dictionary.md` bagian 1 dan 2 |
| Reuse | `IdentityModel`, `MstLegalEntity`, `ApplicationDbContext` |
| Cakupan | `Models/AccChartOfAccount.cs`, `Models/AccJournalType.cs`, dua berkas configuration di `Repositories/Configurations/Corporate/AccountingManagement/MasterData/`, dua `DbSet`. Unique index `(LegalEntityId, AccountCode)` dan `(JournalTypeCode)`. **Tanpa** kolom `RequiresCostCenter` |
| Dependency | `BE-ACC-001` |
| Acceptance | (1) Kolom, tipe, panjang, dan index persis seperti kamus data. (2) Relasi induk-anak memakai `DeleteBehavior.Restrict`. (3) Build lulus |
| Verifikasi | `dotnet build`; pembandingan berkas configuration terhadap kamus data |
| Risiko/pemilik | Owner Backend. Prefix registry yang disetujui adalah **`Acc`** (terdaftar 1 Sep 2026), jadi `AccChartOfAccount` dan `AccJournalType` sudah sesuai |
| DoD | Build lulus, configuration cocok dengan kamus data, tanpa migration |
| **Status** | `ROADMAP_READY` · **`EXECUTION_READY`** — lifecycle `Acc` `ACTIVE` sejak `ACC-DEC-038`; menunggu `BE-ACC-001` selesai |

### `BE-ACC-004` — Entity periode akuntansi

| Field | Isi |
|---|---|
| Outcome | Tabel `AccAccountingPeriod` berdiri beserta configuration dan `DbSet`-nya |
| Trace | `ACC-DEC-012`, `ACC-DEC-013`, `ACC-DEC-037`; `FR-ACC-010` sampai `015` |
| Kontrak | `erd/03-accounting-period.md`, `erd/data-dictionary.md` bagian 3 |
| Reuse | `IdentityModel`, `MstLegalEntity` |
| Cakupan | `Models/AccAccountingPeriod.cs`, satu configuration, satu `DbSet`, unique index `(LegalEntityId, PeriodCode)` |
| Dependency | `BE-ACC-001` |
| Acceptance | (1) Tiga nilai status tersimpan sebagai integer. (2) `PeriodCode` panjang 7. (3) Build lulus |
| Verifikasi | `dotnet build`; pembandingan terhadap kamus data |
| Risiko/pemilik | Owner Backend |
| DoD | Build lulus, tanpa migration |
| **Status** | `ROADMAP_READY` · `EXECUTION_READY` setelah dependency task-nya selesai. Tidak ada lagi penghalang lifecycle |

### `BE-ACC-005` — Entity jurnal, baris jurnal, dan riwayat persetujuan

| Field | Isi |
|---|---|
| Outcome | Tiga tabel inti jurnal berdiri beserta configuration dan `DbSet`-nya, ditambah tabel alokator nomor milik Accounting |
| Trace | `ACC-DEC-006`, `ACC-DEC-014`, `ACC-DEC-017`, `ACC-DEC-019`, `ACC-DEC-020`, `ACC-DEC-037`; `FR-ACC-020` sampai `026` |
| Kontrak | `erd/02-journal.md`, `erd/data-dictionary.md` bagian 4 sampai 6 |
| Reuse | `IdentityModel`, `MstCostCenter`, `MstLegalEntity` |
| Cakupan | `Models/AccJournal.cs`, `AccJournalLine.cs`, `AccJournalApproval.cs`, **`AccNumberSeries.cs`**, empat configuration, empat `DbSet`. `JournalId` pada baris memakai `Cascade`; sisanya `Restrict`. Check constraint "tepat satu sisi terisi" pada baris. **Tanpa** kolom `SourceDomain`, `SourceTransactionId`, maupun `CurrencyCode` — lihat catatan di bawah |
| Dependency | `BE-ACC-003`, `BE-ACC-004` |
| Acceptance | (1) `decimal(18,2)` untuk seluruh kolom nilai. (2) Unique index `(LegalEntityId, JournalNumber)`, `(JournalId, LineNumber)`, dan `(SequenceKey, ScopeKey)` pada `AccNumberSeries`. (3) Foreign key ke `MstCostCenter` ada dan boleh kosong. (4) Build lulus |
| Verifikasi | `dotnet build`; pembandingan terhadap kamus data dan bentuk DDL |
| Risiko/pemilik | Owner Backend. Salah tipe kolom nilai berakibat langsung pada ketepatan angka — lihat `NFR-008` |
| DoD | Build lulus, tanpa migration |
| **Status** | `ROADMAP_READY` · `EXECUTION_READY` setelah dependency task-nya selesai. Tidak ada lagi penghalang lifecycle |

#### Kenapa `AccNumberSeries` ada, dan kenapa kolom tertentu sengaja tidak ada

`AccNumberSeries` adalah alokator nomor jurnal milik Accounting, bentuknya meniru
`BilNumberSeries` di `Areas/HealthServices/BillingManagement/Billing/Models/`: `SequenceKey`,
`ScopeKey`, `ResetPolicy`, `CurrentValue`, `LastAllocatedAt`. Accounting **tidak boleh** memakai
tabel Billing (`ACC-DEC-004`), dan `QBE-CODE-006` menuntut alokator yang atomik dan ber-scope,
sehingga Accounting memerlukan tabelnya sendiri. Alasan lengkapnya di `BE-ACC-010`.

Tiga kolom sengaja **tidak** dibuat, dan ketiadaannya adalah keputusan, bukan kelalaian:

| Kolom | Alasan tidak dibuat |
|---|---|
| `SourceDomain`, `SourceTransactionId` | MVP tidak punya jurnal otomatis (`ACC-DEC-009`). Menambahkannya sekarang berarti menambah kolom Phase 2 untuk future proofing semata. Strategi perluasannya ada di [`../04-prd-to-mvp.md`](../04-prd-to-mvp.md) bagian *Strategi perluasan source traceability Phase 2* |
| `CurrencyCode` | MVP hanya IDR (`ACC-DEC-020`) dan keseimbangan diukur dalam IDR (`ACC-DEC-021`). `CurrencyCode` wajib pada **envelope kejadian** Phase 2, bukan pada tabel jurnal MVP — lihat [`../contracts/cross-module-contract.md`](../contracts/cross-module-contract.md) bagian 4 |

### `BE-ACC-006` — Migration pertama dan data master awal

| Field | Isi |
|---|---|
| Outcome | Tujuh tabel Accounting ada di database, dan master jenis jurnal terisi empat baris sehingga modul dapat dipakai |
| Trace | `ACC-DEC-008`, `ACC-DEC-010`, `ACC-DEC-013`; `02-backend-architecture.md` bagian 8 dan 9 |
| Kontrak | `erd/data-dictionary.md` bagian DDL |
| Reuse | Tidak ada |
| Cakupan | Satu migration berisi **tujuh** `CreateTable` beserta index dan foreign key. Pengisian empat baris `AccJournalType` lewat migration atau lewat aplikasi — bukan lewat skrip SQL manual |
| Dependency | `BE-ACC-003`, `BE-ACC-004`, `BE-ACC-005`; **`ACC-DEP-005`** gate koordinasi migration |
| Gate | **`MIGRATION COORDINATION GATE` wajib lulus sebelum `dotnet ef migrations add` dijalankan.** Tujuh pertanyaannya ada di [`../06-shared-migration-coordination-rule.md`](../06-shared-migration-coordination-rule.md). Jawabannya ditulis ke `evidence/04-migration-coordination-gate.md` sebelum migration dibuat |
| Acceptance | (1) **`CONTAMINATION GUARD` lulus** — lihat blok di bawah tabel ini. (2) Berkas migration memuat tepat tujuh `CreateTable` bernama sesuai prefix terdaftar, beserta index dan foreign key-nya. (3) Empat jenis jurnal terisi dengan `JB` dan `SA` bertanda sistem. (4) Berkas evidence gate berisi SHA baseline yang menjadi sumber migration ini |
| Verifikasi | Gate koordinasi; pemeriksaan isi berkas migration sebelum diterapkan; `dotnet ef database update` **oleh owner modul** ke database yang disepakati |
| Risiko/pemilik | **Tertinggi pada gelombang ini.** Owner modul. Database pengembangan dipakai bersama satu tim — konfirmasikan sebelum menerapkan. Snapshot sudah dipulihkan (`EV-ACC-006`), tetapi pemeriksaan hitung-operasi tetap wajib |
| DoD | Migration diperiksa isinya, diterapkan owner modul, master terisi, laporan berisi jumlah operasi yang ditemukan |
| **Status** | **`BLOCKED`** berantai dari `BE-ACC-003` sampai `005`, ditambah `ACC-DEP-005` |

#### `CONTAMINATION GUARD` — aturan penerimaan migration

Prinsip yang mengikat, dan sengaja ditulis positif bukan sebagai daftar larangan:

> **Migration Accounting hanya boleh membawa schema operation yang memang berasal dari perubahan
> Accounting yang direncanakan.**

Daftar prefix tidak dipakai sebagai penyaring, karena daftar selalu ketinggalan. Yang dipakai
adalah pembandingan terhadap tujuh tabel yang direncanakan. Setiap operasi di luar itu — apa pun
prefixnya, termasuk `Fin*`, `Bil*`, `Opr*`, `Mst*`, atau modul yang belum ada saat ini —
menjadikan migration berstatus **`CONTAMINATED`**.

Bila `CONTAMINATED`, maka seluruh hal berikut berlaku sekaligus:

| Tindakan | Ketentuan |
|---|---|
| Menerapkan migration | **Jangan** |
| `dotnet ef database update` | **Jangan** |
| Merge migration tersebut | **Jangan** |
| Operasi asing yang ditemukan | **Laporkan** — nama tabel, jenis operasi, dan modul pemiliknya |
| Langkah berikutnya | Selesaikan baseline/`ModelSnapshot` lebih dahulu bersama pemilik modul terkait dan lead |
| Setelah baseline beres | Generate **ulang** migration sesuai governance, lalu jalankan guard ini lagi |

Migration yang `CONTAMINATED` **tidak diperbaiki dengan menyunting berkasnya**. Ia dibuang dan
dibuat ulang dari baseline yang benar. Menyunting berkas migration menyembunyikan penyebabnya
dan menyisakan snapshot yang tetap salah.

---

## `MVP-1` — Jurnal manual dari dibuat sampai disahkan

### `BE-ACC-007` — API daftar akun

| Field | Isi |
|---|---|
| Outcome | Administrator dapat menyusun daftar akun bertingkat per badan hukum lewat API, dengan seluruh aturan COA ditegakkan backend |
| Trace | `ACC-DEC-022`, `ACC-DEC-023`, `ACC-DEC-024`, `ACC-DEC-037`; `FR-ACC-001` sampai `005`; `EPIC ACC-01` |
| Kontrak | `ACC-API-0.1` grup Chart of Account; `ACC-VALIDATION-0.2` bagian 1; `ACC-PERMISSION-0.1` |
| Reuse | `ApiResponse<T>`, `PagedResult<T>`, `LoggerService`, atribut hak akses |
| Cakupan | `ChartOfAccountController` (8 endpoint), `AccChartOfAccountService`, `ChartOfAccountDtos`, satu baris `AddScoped`. `/options` hanya mengembalikan akun yang menerima transaksi dan aktif, dan menyertakan `RequiresCostCenter` yang **diturunkan** dari jenis akun |
| Dependency | `BE-ACC-006`, `BE-ACC-002` |
| Acceptance | (1) Kode akun kembar pada badan hukum sama ditolak `409`. (2) Akun beranak tidak dapat `IsPostable = true`. (3) Akun bersaldo bukan nol gagal dinonaktifkan, pesannya menyebut jumlah saldo. (4) Kode akun bertransaksi gagal diubah. (5) Permintaan atas badan hukum yang bukan hak pengguna ditolak `403` |
| Verifikasi | Test integrasi `FR-ACC-001` sampai `005`; `UAT-01`, `UAT-17` |
| Risiko/pemilik | Owner Backend. Aturan (3) menuntut perhitungan saldo — pastikan menyaring `JournalStatus == Posted` |
| DoD | Seluruh acceptance terbukti test, `[AccessPermission]` sesuai matriks, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai; ditambah hasil `BE-ACC-002` untuk butir (5) |

### `BE-ACC-008` — API jenis jurnal

| Field | Isi |
|---|---|
| Outcome | Administrator dapat mengatur jenis jurnal dan awalan nomornya tanpa menyentuh kode |
| Trace | `ACC-DEC-010`; `FR-ACC-006`, `FR-ACC-007`; `EPIC ACC-02` |
| Kontrak | `ACC-API-0.1` grup Journal Type |
| Reuse | `ApplicationDbContext` langsung — CRUD sederhana, tanpa service, sesuai konvensi |
| Cakupan | `JournalTypeController` (4 endpoint), `JournalTypeDtos` |
| Dependency | `BE-ACC-006` |
| Acceptance | (1) Kode jenis kembar ditolak `409`. (2) Jenis bertanda sistem gagal diubah kode maupun awalan nomornya. (3) Awalan nomor kosong ditolak `400` |
| Verifikasi | Test integrasi `FR-ACC-006`, `FR-ACC-007` |
| Risiko/pemilik | Rendah. Owner Backend |
| DoD | Acceptance terbukti test, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

### `BE-ACC-009` — API periode akuntansi

| Field | Isi |
|---|---|
| Outcome | Periode satu tahun buku dapat dibangkitkan sekaligus, ditutup bertahap, dan dibuka kembali dengan alasan tertulis |
| Trace | `ACC-DEC-012`, `ACC-DEC-013`, `ACC-DEC-026`, `ACC-DEC-027`, `ACC-DEC-028`; `FR-ACC-010` sampai `015`; `EPIC ACC-03` |
| Kontrak | `ACC-API-0.1` grup Accounting Period; `ACC-STATE-0.1` bagian 2 |
| Reuse | `LoggerService` untuk mencatat alasan penutupan dan pembukaan kembali |
| Cakupan | `AccountingPeriodController` (5 endpoint), `AccAccountingPeriodService`, `AccountingPeriodDtos`. Pemeriksaan "apakah periode menerima jenis jurnal ini" dibuat `public static` menerima `ApplicationDbContext`, agar dipakai `AccJournalService` tanpa registrasi baru |
| Dependency | `BE-ACC-006`, `BE-ACC-002` |
| Acceptance | (1) `POST /generate` menghasilkan tepat 12 periode, tahun kabisat benar. (2) Membangkitkan tahun yang sama dua kali ditolak `409`. (3) **Membuka kembali periode `Closed` menghasilkan `SoftClosed`, bukan `Open`.** (4) Membuka kembali tanpa alasan ditolak `400`. (5) Hanya pemegang `AccountingPeriod : Close` yang dapat menutup |
| Verifikasi | Test integrasi `FR-ACC-010` sampai `015`; `UAT-08`, `UAT-09` |
| Risiko/pemilik | Owner Backend. Butir (3) paling mudah salah — mengembalikan ke `Open` akan melanggar `ACC-DEC-028` |
| DoD | Acceptance terbukti test, alasan tercatat di jejak audit, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

### `BE-ACC-010` — Jurnal draft: simpan, ubah, hapus, dan penomoran

| Field | Isi |
|---|---|
| Outcome | Petugas dapat menyusun jurnal beserta barisnya, menyimpannya walaupun belum seimbang, dan setiap jurnal mendapat nomor yang benar |
| Trace | `ACC-DEC-014`, `ACC-DEC-019`, `ACC-DEC-020`, `ACC-DEC-025`, `ACC-DEC-037`; `FR-ACC-020`, `022` sampai `026`; `EPIC ACC-04` |
| Kontrak | `ACC-API-0.1` grup Journal (5 endpoint pertama); `ACC-VALIDATION-0.2` bagian 3 |
| Reuse | `MstCostCenter`, `AccChartOfAccount`, `LoggerService` |
| Cakupan | `JournalController` bagian CRUD, `AccJournalService` bagian penyimpanan dan penomoran, `JournalDtos`. Baris dikirim **utuh** dan menggantikan seluruh baris sebelumnya. Periode ditentukan sistem dari `AccountingDate` |
| Dependency | `BE-ACC-007`, `BE-ACC-008`, `BE-ACC-009` |
| Acceptance | (1) Jurnal timpang tersimpan sebagai `Draft`. (2) Nomor berbentuk `{prefix}/{yyyy}/{MM}/{00001}`, awalan dari master. (3) **Dua atau lebih permintaan create yang berjalan bersamaan menghasilkan `JournalNumber` yang seluruhnya unik.** (4) **`JournalNumber` kembar adalah pelanggaran invariant** — bukan sekadar bug yang dapat ditoleransi. (5) Baris yang mengisi debit dan kredit sekaligus ditolak `400` beserta nomor barisnya. (6) Akun beban tanpa Cost Center ditolak `400`. (7) Akun milik badan hukum lain ditolak `409`. (8) Jurnal beserta barisnya tersimpan dalam satu transaksi |
| Verifikasi | Unit test penurunan kewajiban Cost Center; **test integrasi konkurensi nyata** yang menjalankan sejumlah permintaan create paralel terhadap database sungguhan lalu membuktikan tidak ada nomor kembar — inilah penutup `GAP-ACC-004`; test integrasi `FR-ACC-020`, `022` sampai `026`; `UAT-02`, `UAT-04`, `UAT-05` |
| Risiko/pemilik | Owner Backend. Butir (3) dan (4) paling sering salah dirancang. Lihat blok mekanisme di bawah |
| DoD | Acceptance terbukti test, **`GAP-ACC-004` tertutup**, laporan task tersedia. `BE-ACC-010` **tidak boleh** dinyatakan `DONE` selama `GAP-ACC-004` masih terbuka |
| **Status** | **`BLOCKED`** berantai |

#### Mekanisme penomoran — apa yang terkunci dan apa yang tidak

| Ketentuan | Isi |
|---|---|
| Nomor kembar | **Dilarang.** Invariant, ditegakkan juga oleh unique index `(LegalEntityId, JournalNumber)` sebagai jaring terakhir |
| Nomor terlewat | **Diizinkan** (`ACC-DEC-014`). Gap bukan cacat |
| Application-level global lock | **Dilarang.** Lock proses tidak melindungi saat aplikasi berjalan lebih dari satu instance, dan `QBE-CODE-003` melarangnya sebagai satu-satunya alokator |
| `Count+1` / `Max+1` tanpa proteksi | **Dilarang** oleh `QBE-CODE-003` |
| DB sequence | **Tidak dikunci.** Ia belum terbukti sebagai pola repository, jadi jangan dipilih hanya karena terdengar benar |
| Yang dipakai | Alokasi atomik ber-scope pada database, mengikuti pola yang **sudah terbukti** di repository |

Pola repository yang dimaksud, terverifikasi pada `aa837d7`: `pg_advisory_xact_lock(hashtext(<key>))`
yang diambil di dalam transaction, lalu penambahan `CurrentValue` pada tabel number-series
ber-`(SequenceKey, ScopeKey)`. Implementasinya ada di
`Areas/HealthServices/BillingManagement/Billing/Services/BillingNumberSeriesService.cs`, dan
primitif `pg_advisory_xact_lock` yang sama dipakai lintas area — Billing maupun
`Areas/Corporate/HumanResource/` — sehingga ia konvensi repository, bukan kebiasaan satu modul.

Lock ini **bukan** application-level global lock: ia dipegang database, ber-scope pada kunci
nomor, dan lepas sendiri saat transaction berakhir.

Accounting memakai tabelnya sendiri, `AccNumberSeries` dari `BE-ACC-005`, karena `ACC-DEC-004`
melarangnya menulis tabel Billing. Bila kelak alokator bersama yang benar-benar lintas modul
diekstrak sesuai `QBE-CODE-006`, Accounting berpindah ke sana dan `AccNumberSeries` dipensiunkan
lewat keputusan arsitektur tersendiri.

### `BE-ACC-011` — Jurnal: pengajuan, persetujuan, penolakan, pengesahan

| Field | Isi |
|---|---|
| Outcome | Jurnal melewati pemeriksaan orang kedua sebelum menjadi riwayat permanen, dan jurnal yang disahkan tidak dapat diubah lagi |
| Trace | `ACC-DEC-006`, `ACC-DEC-010`, `ACC-DEC-015`, `ACC-DEC-016`, `ACC-DEC-021`; `FR-ACC-021`, `030` sampai `034`; `EPIC ACC-05` |
| Kontrak | `ACC-API-0.1` grup Journal (4 endpoint aksi); `ACC-STATE-0.1` bagian 1; `ACC-VALIDATION-0.2` bagian 4 |
| Reuse | `AccAccountingPeriodService` bagian pemeriksaan periode, `LoggerService` |
| Cakupan | Endpoint `submit`, `approve`, `reject`, `post`; `AccJournalService` bagian daur hidup; penulisan `AccJournalApproval`; `AvailableActions` pada DTO rincian |
| Dependency | `BE-ACC-010` |
| Acceptance | (1) Sembilan syarat pengajuan diperiksa saat `submit` **dan diperiksa ulang saat `post`**. (2) Penyetuju sama dengan pembuat ditolak `403`, tanpa pengecualian. (3) Mengesahkan jurnal yang belum disetujui ditolak `409`. (4) Mengubah maupun menghapus jurnal `Posted` ditolak `409`, dan `IsDelete` tetap salah. (5) Periode yang menolak jenis jurnal itu menghasilkan `422` beserta nama periode. (6) `AvailableActions` sesuai status, hak akses, dan aturan pembuat-bukan-penyetuju |
| Verifikasi | Test integrasi `FR-ACC-021`, `030` sampai `034`; `UAT-01`, `UAT-03`, `UAT-06`, `UAT-07`, `UAT-13` |
| Risiko/pemilik | **Tertinggi pada modul ini.** Owner Backend. Butir (1) dan (4) adalah invariant akuntansi; kegagalan di sini merusak seluruh laporan |
| DoD | Acceptance terbukti test, riwayat persetujuan terisi, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

---

## `MVP-2` — Buku besar dan neraca saldo

### `BE-ACC-012` — Buku besar, saldo per akun, dan neraca saldo

| Field | Isi |
|---|---|
| Outcome | Hasil pembukuan terlihat: mutasi per akun dengan saldo berjalan, dan neraca saldo satu periode yang selalu seimbang |
| Trace | `ACC-DEC-030`, `ACC-DEC-032`, `ACC-DEC-037`; `FR-ACC-050` sampai `053`; `EPIC ACC-07` |
| Kontrak | `ACC-API-0.1` grup General Ledger; `ACC-PERMISSION-0.1` |
| Reuse | `AccJournalLine` sebagai sumber tunggal; `AsNoTracking`; `LoggerService` |
| Cakupan | `GeneralLedgerController` (3 endpoint), `AccGeneralLedgerService`, `GeneralLedgerDtos`. Seluruh perhitungan dari baris jurnal berstatus `Posted`. **Tidak** membuat tabel buku besar |
| Dependency | `BE-ACC-011` |
| Acceptance | (1) Neraca saldo total debit sama persis dengan total kredit. (2) Jurnal berstatus selain `Posted` **tidak** ikut terhitung — termasuk `Draft`, `Submitted`, `Approved`, dan `Rejected`. (3) **Saldo berjalan deterministic**: dua pemanggilan atas data yang sama menghasilkan urutan dan saldo yang identik. (4) **Urutan sekunder yang stabil** dipakai ketika `AccountingDate` sama — lihat blok di bawah. (5) Saldo dua badan hukum tidak pernah tercampur. (6) Pembacaan `/trial-balance` dicatat logger; dua endpoint lain tidak |
| Verifikasi | Test integrasi `FR-ACC-050` sampai `053`; test determinisme urutan pada `AccountingDate` kembar; **verifikasi performa/readiness** untuk Buku Besar, Saldo per Akun, dan Neraca Saldo; `UAT-14`, `UAT-15` |
| Risiko/pemilik | Owner Backend. Butir (2) paling mudah terlewat dan akibatnya laporan salah tanpa terlihat |
| DoD | Acceptance terbukti test, hasil verifikasi performa tercatat, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

#### Urutan yang deterministic, memakai field yang benar-benar ada

Saldo berjalan hanya bermakna bila urutannya tidak pernah berubah. `AccountingDate` saja tidak
cukup — beberapa jurnal sah berbagi tanggal yang sama, dan tanpa urutan sekunder, saldo berjalan
akan berbeda antar pemanggilan.

Urutannya: **`AccountingDate`, lalu `JournalNumber`, lalu `LineNumber`.**

Ketiganya sudah ada pada rancangan MVP dan bersama-sama unik, karena
`(LegalEntityId, JournalNumber)` unik dan `(JournalId, LineNumber)` unik. **Jangan menciptakan
field baru semata-mata untuk pengurutan** — itu memerlukan keputusan arsitektur tersendiri, dan
`SortOrder` presentasi yang dipersistensi dilarang untuk kode baru.

#### Index strategy — dari query nyata, bukan tebakan

Index ditetapkan **setelah** query final tertulis, berdasarkan rencana eksekusi yang benar-benar
diukur pada data yang menyerupai produksi. Kandidat yang masuk akal dicatat sebagai kandidat,
bukan sebagai keputusan:

| Kandidat | Melayani | Status |
|---|---|---|
| `AccJournalLine (AccountId, JournalId)` | Buku besar per akun | Kandidat — buktikan lewat rencana eksekusi |
| `AccJournal (LegalEntityId, JournalStatus, AccountingDate)` | Penyaringan `Posted` per periode | Kandidat — buktikan lewat rencana eksekusi |

Unique index yang sudah ditetapkan kamus data tetap berlaku dan tidak termasuk kandidat di atas.
Menambah index spekulatif memperlambat tulis tanpa bukti bahwa baca menjadi lebih cepat.

---

## `MVP-3` — Koreksi dan saldo awal

### `BE-ACC-013` — Pembalikan penuh dan jurnal penyesuaian

| Field | Isi |
|---|---|
| Outcome | Kesalahan pada jurnal yang sudah disahkan dapat diperbaiki tanpa menyentuh riwayatnya |
| Trace | `ACC-DEC-006`, `ACC-DEC-017`, `ACC-DEC-029`; `FR-ACC-040` sampai `043`; `EPIC ACC-06` |
| Kontrak | `ACC-API-0.1` endpoint `reverse`; `ACC-VALIDATION-0.2` bagian 5 |
| Reuse | `AccJournalService`, jenis jurnal `JB` dan `JP` dari master |
| Cakupan | Endpoint `POST /{id}/reverse`, dua cara koreksi, penautan `ReversalOfJournalId`, `ReverseJournalDto` |
| Dependency | `BE-ACC-011` |
| Acceptance | (1) Pembalikan penuh menghasilkan jurnal `JB` berisi kebalikan seluruh baris. (2) Penyesuaian menghasilkan jurnal `JP` berisi baris selisih yang dikirim pengguna. (3) **Jurnal asal tetap `Posted` dan isinya tidak berubah sama sekali.** (4) Membalik dua kali ditolak `409` beserta nomor jurnal pembaliknya. (5) Jurnal pembalik lahir menunggu persetujuan, bukan langsung disahkan. (6) Alasan wajib diisi |
| Verifikasi | Test integrasi `FR-ACC-040` sampai `043`; `UAT-10`, `UAT-11`, `UAT-12` |
| Risiko/pemilik | Owner Backend. Butir (3) adalah inti `ACC-DEC-006` |
| DoD | Acceptance terbukti test, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

### `BE-ACC-014` — Saldo awal

| Field | Isi |
|---|---|
| Outcome | Pembukuan punya titik tumpu: saldo pembuka tercatat sebagai jurnal dan ikut terhitung di seluruh laporan |
| Trace | `ACC-DEC-018`, `ACC-DEC-033`; `FR-ACC-060`, `FR-ACC-061`; `EPIC ACC-08` |
| Kontrak | `ACC-API-0.1` grup Journal; jenis jurnal `SA` |
| Reuse | Seluruh jalur jurnal yang sudah ada. **Tidak ada endpoint baru** |
| Cakupan | Pemastian jenis `SA` bertanda sistem dan menuntut persetujuan; pemastian jurnal `SA` ikut terhitung buku besar dan neraca saldo; pengujian ujung ke ujung |
| Dependency | `BE-ACC-012` |
| Acceptance | (1) Jurnal `SA` tersimpan, disetujui, dan disahkan lewat jalur jurnal biasa. (2) Neraca saldo periode pertama menampilkan saldo pembuka dan tetap seimbang. (3) Hanya pemegang `Journal : Post` yang dapat mengesahkannya |
| Verifikasi | Test integrasi `FR-ACC-060`, `FR-ACC-061`; `UAT-16` |
| Risiko/pemilik | Owner Backend. Persetujuan pimpinan keuangan berlangsung **di luar sistem** sebelum Manajer menekan Sahkan — jangan membangun alur persetujuan kedua di dalam sistem tanpa keputusan owner |
| DoD | Acceptance terbukti test, laporan task tersedia |
| **Status** | **`BLOCKED`** berantai |

---

## Ringkasan penghalang

| Penghalang | Task terdampak | Pemilik | Yang tetap bisa jalan |
|---|---|---|---|
| `ACC-DEP-007` governance checker hilang | Merge ke integration, bukan penulisan kode | Lead | Seluruh task lokal |
| `ACC-DEP-005` aturan koordinasi migration belum canonical | `BE-ACC-006` saja, lewat gate-nya | Lead | Seluruh task selain `BE-ACC-006`. Gate tetap dapat dijalankan memakai usulan `QBE-MIG-001` |
| Hak atas badan hukum belum jelas | `BE-ACC-007` dan seterusnya, pada butir penyaringan | Owner keamanan platform | `BE-ACC-002` justru dirancang untuk menutup ini |

`ACC-DEP-001` **tidak** ada dalam daftar ini. Ia sudah selesai; buktinya di
`evidence/01-design-verification-evidence.md` bagian `EV-ACC-006`.

### Gap yang mengikat task tertentu

| Gap | Task pengikat | Ketentuan |
|---|---|---|
| `GAP-ACC-003` | `BE-ACC-012` | Ditutup dengan menetapkan cara menguji pembatasan pencatatan `ACC-DEC-032`, atau dengan menerimanya sebagai pemeriksaan manual **yang dicatat**. Keputusannya diambil saat task dikerjakan, bukan didiamkan |
| `GAP-ACC-004` | `BE-ACC-010` | **Wajib tertutup** sebelum `BE-ACC-010` dinyatakan `DONE`, lewat test integrasi konkurensi nyata |
| `GAP-ACC-005` | `BE-ACC-002` | `BE-ACC-002` memang dirancang untuk menutupnya |

## Yang sengaja tidak ada di roadmap ini

| Yang tidak ada | Alasan |
|---|---|
| Task integrasi otomatis dan pemetaan posting | Phase 2 menurut `ACC-DEC-009` dan `ACC-DEC-036`. `ACC-XM-001` juga belum diputuskan |
| Task jurnal berulang dan impor CSV | Ditunda dengan alasan bersebab pada `04-prd-to-mvp.md` bagian 8 |
| Task tutup buku berdaftar periksa | Ditunda; penutupan periode tetap tersedia lewat `BE-ACC-009` |
| Task Laba Rugi dan Neraca | `ACC-DEC-030` membatasi laporan MVP pada Neraca Saldo dan Buku Besar |
| Task pembuatan tabel buku besar | Buku besar dihitung, bukan disimpan |
