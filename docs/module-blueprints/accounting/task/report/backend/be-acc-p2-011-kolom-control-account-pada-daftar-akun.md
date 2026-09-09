# Laporan Perubahan Backend — `BE-ACC-P2-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-011` |
| Judul | Kolom control account pada daftar akun |
| Slice | Gelombang `P2-CTRL` — control account. **Menyentuh artefak Phase 1** |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-011` |
| Trace | `ACC-DEC-064`, turunan `ACC-DEC-062`; `FR-P2-035`; kamus data bagian 1 |
| Contract version | `ACC-API-0.8` grup Chart of Account; blueprint `ACC-BP-001` revisi 11; roadmap revisi 2 `APPROVED` 9 September 2026 (amandemen `ACC-DEC-064`) |
| Dependency | **Tidak ada** |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (5 berkas), logika bisnis 0, kontrak API 1 (memakai kontrak yang sudah ada, bertambah bidang), database 2, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, dan `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `cc59164` |
| Tanggal | 9 September 2026 |
| Status | **`DONE`** — 5 dari 5 acceptance lulus, nol migration dibuat, database tidak disentuh |

## Validasi baseline sebelum mulai

| Yang diperiksa | Hasil |
| --- | --- |
| Roadmap memberi wewenang task ini | Ya — `BE-ACC-P2-011` berstatus `READY`, kolom Dependency kosong |
| Instruksi owner | Ya — Rizki, 9 September 2026: kerjakan `011` lebih dahulu, **berhenti sebelum migration**, migration `004` dikerjakan owner sendiri |
| Working tree saat mulai | Memuat berkas `BE-ACC-P2-003` yang belum di-commit. **Nol tumpang tindih** dengan berkas task ini |
| Kesimpulan | Aman dilanjutkan |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AccountingManagement / Accounting` |
| Submodule | `MasterData / ChartOfAccount` — **sudah ada, tidak ada folder baru** |
| Prefix | `Acc` |
| Pemilik | Rizki |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30 |
| Keberlakuan | **`TOUCHED LEGACY`** — `AccChartOfAccount` adalah entity Phase 1 yang sudah berjalan; yang dikerjakan hanya penambahan satu kolom, **bukan** penulisan ulang |
| QBE yang berlaku | `QBE-MOD-002` (modul terdaftar, lolos), `QBE-ENT-001`, `QBE-CFG-001`, `QBE-NAM-001` sampai `004` (nol berkas dipindah, nol berkas diganti nama). `QBE-MOD-003` / `QBE-NAM-004` **tidak berlaku** — nol folder baru dibuat |
| Hasil checker | **`PASS`** — `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0` atas kelima berkas |

**Legacy ratchet dipatuhi.** `AccChartOfAccountService.cs` berukuran 643 baris dan memuat banyak
pola lama; **nol** di antaranya dirapikan. Yang disentuh hanya empat titik yang benar-benar
memerlukan bidang baru.

---

## 1. Masalah yang diperbaiki

Ada empat kelompok akun yang saldonya **tidak boleh** diubah lewat jurnal manual: Kas Kasir, Kas
Kecil, Piutang, dan Hutang. Saldo keempatnya harus selalu sama dengan catatan rincinya —
subledger kasir, rincian piutang pasien, rincian hutang pemasok.

Sekarang tidak ada yang mencegahnya. Siapa pun yang berwenang membuat jurnal manual dapat
mendebit `Kas Kasir` sebesar Rp 5.000.000 lewat layar Jurnal Manual, dan sistem menerimanya.

**Yang membuat ini mahal: tidak ada error sama sekali.** Jurnalnya seimbang, jadi seluruh
pemeriksaan yang ada lolos. Yang terjadi hanya saldo buku besar `Kas Kasir` berselisih Rp 5.000.000
dari total setoran kasir — dan itu baru ketahuan saat rekonsiliasi, kadang berbulan-bulan kemudian,
ketika sudah tidak jelas lagi selisihnya berasal dari mana.

**Kenapa penandanya harus kolom, bukan diturunkan.** `Kas Kasir` dan `Piutang Pasien` sama-sama
berjenis `Asset`, tetapi tidak setiap akun `Asset` adalah control account — `Perlengkapan Kantor`
juga `Asset` dan boleh dijurnal manual. Jadi penandanya tidak dapat disimpulkan dari data lain.

Ini berbeda dari `RequiresCostCenter`, yang justru **ditolak** menjadi kolom pada `ACC-DEC-019`
karena memang dapat diturunkan dari `AccountType == Expense`. Perbedaan itu disengaja dan tercatat.

Task ini memasang **penandanya**. Penolakan jurnalnya adalah `BE-ACC-P2-012`.

---

## 2. Proses bisnis

### 2.1 Menandai sebuah akun

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pemilik proses akuntansi | Membuka daftar akun, memilih `1-1002 Kas Kasir` |
| 2 | Pemilik proses akuntansi | Mencentang penanda control account, menyimpan |
| 3 | Sistem | `IsControlAccount` bernilai `true` pada akun itu |
| 4 | Sistem | *(menyusul `BE-ACC-P2-012`)* setiap baris jurnal manual ke akun itu ditolak `422` |

Langkah 4 **belum berjalan**. Yang berdiri sekarang hanya penandanya, dan penandanya belum
berakibat apa pun terhadap jurnal.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Akun lama yang tidak pernah disentuh | Tetap `false`, tetap dapat dijurnal manual seperti biasa. **Nol perubahan perilaku** |
| Permintaan `PUT` lama yang tidak mengirim bidang ini | Penanda **terlepas** menjadi `false` — lihat peringatan di bagian 7 |
| Akun sudah ditandai lalu ditandai lagi | Tidak ada akibat; nilainya tetap `true` |

### 2.3 Contoh berangka

Rumah sakit menandai `1-1002 Kas Kasir` sebagai control account. Kasir menyetor Rp 12.500.000 hari
itu, tercatat lewat kejadian akuntansi. Buku besar `Kas Kasir` bertambah Rp 12.500.000, dan
subledger kasir mencatat angka yang sama — **cocok**.

Tanpa penanda ini, seorang staf yang meluruskan selisih dapat menambah jurnal manual Rp 200.000 ke
akun yang sama. Buku besar menjadi Rp 12.700.000, subledger tetap Rp 12.500.000, dan selisih
Rp 200.000 itu akan dikira cacat data kasir — padahal asalnya dari jurnal manual.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `BE-ACC-P2-011`, `012`, `013`; `erd/data-dictionary.md` bagian 1;
`00-interview-decisions.md` `ACC-DEC-064` dan `ACC-DEC-019`; `requirement-traceability-phase2.md`;
`AccChartOfAccount.cs`; `AccChartOfAccountConfiguration.cs`; `ChartOfAccountDtos.cs`;
`AccChartOfAccountService.cs`; `MstLegalEntity.cs`; `TestDatabase.cs`;
`InpatientClinicalSchemaTests.cs` sebagai pola uji schema.

### 3.2 Berkas yang berubah

Lima berkas — satu baru, empat diperbarui. **Nol berkas di luar cakupan disentuh.**

| Berkas | Perubahan |
| --- | --- |
| `.../ChartOfAccount/Models/AccChartOfAccount.cs` | Satu property `bool IsControlAccount` berbawaan `false`, ditempatkan sesudah `IsActive` mengikuti urutan kamus data |
| `Repositories/Configurations/.../MasterData/AccChartOfAccountConfiguration.cs` | `HasDefaultValue(false).IsRequired()` dan `HasIndex(x => x.IsControlAccount)` |
| `.../ChartOfAccount/DTOs/ChartOfAccountDtos.cs` | Bidang baru pada `CreateChartOfAccountRequest`, `UpdateChartOfAccountRequest`, dan `ChartOfAccountListResponse` |
| `.../ChartOfAccount/Services/AccChartOfAccountService.cs` | **Empat titik**: proyeksi daftar, pembuatan akun, pembaruan akun, dan pemetaan rincian |
| `Tests/.../AccountingManagement/AccChartOfAccountControlAccountTests.cs` | **Baru.** Lima uji acceptance |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint dibuat, diubah route-nya, atau dihapus.** Tiga DTO bertambah satu bidang `bool`; seluruhnya **backward compatible** — lihat bagian 4 |
| Database | **Dampak schema: ada.** Satu kolom `IsControlAccount` dan satu index pada `AccChartOfAccount`. **Nol migration dibuat, nol perintah `dotnet ef` dijalankan, database tidak disentuh** |
| Keamanan/Auth | `NOT APPLICABLE` — nol atribut `[AccessAction]` atau `[AccessPermission]` dibuat maupun diubah. Penandaan memakai hak akses `ChartOfAccount : Create` dan `Update` yang sudah ada, karena ia memang bagian dari pembuatan dan pembaruan akun |

### 3.4 Keputusan yang perlu dijelaskan

**Kolom ditempatkan sesudah `IsActive`, bukan di akhir.** Mengikuti urutan kamus data bagian 1.
Urutan property tidak memengaruhi schema, tetapi menjaga berkas tetap terbaca sejajar dengan
kontraknya.

**Index sengaja bukan unique.** Banyak akun boleh menjadi control account sekaligus — memang ada
empat kelompok. Index-nya murni untuk penyaringan, dan `BE-ACC-P2-013` akan memakainya untuk
mengambil daftar control account per badan hukum.

**Proyeksi pohon sengaja TIDAK ikut diubah.** `ChartOfAccountTreeResponse` tidak memuat bidang ini.
Pohon akun dipakai untuk menampilkan susunan, bukan untuk mengambil keputusan pencatatan, jadi
menambahkannya di sana hanya memperbesar respons tanpa ada yang memakainya. Dicatat karena
pola kodenya nyaris sama dengan proyeksi daftar dan mudah terbawa tanpa sengaja.

---

## 4. Dokumentasi endpoint

**Nol endpoint dibuat atau diubah bentuknya.** Tiga DTO pada endpoint yang sudah ada bertambah
satu bidang.

#### Corporate / Accounting / Master Data / Chart of Account

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/master-data/chart-of-accounts` | Membuat akun. **Request bertambah `IsControlAccount`** (opsional, bawaan `false`) | `ChartOfAccount : Create` |
| `PUT` | `/api/v1/corporate/accounting/master-data/chart-of-accounts/{id}` | Memperbarui akun. **Request bertambah `IsControlAccount`** | `ChartOfAccount : Update` |
| `GET` | `/api/v1/corporate/accounting/master-data/chart-of-accounts` | Daftar akun. **Response bertambah `IsControlAccount`** | `ChartOfAccount : Read` |
| `GET` | `/api/v1/corporate/accounting/master-data/chart-of-accounts/{id}` | Rincian akun. **Response bertambah `IsControlAccount`** lewat pewarisan | `ChartOfAccount : Read` |

**Backward compatible.** Bidangnya `bool` bertipe nilai dengan bawaan `false`, jadi consumer lama
yang tidak mengirimnya tetap berhasil dan menghasilkan akun biasa. Nol bidang dihapus, nol bidang
diganti nama, nol nilai enum bergeser.

### Delta kontrak yang dicatat

Kartu roadmap menyebut cakupannya *"penambahan bidang pada `CreateChartOfAccountDto` dan
`UpdateChartOfAccountDto`"* — **dua** DTO. Implementasinya menambah **tiga**, karena
`ChartOfAccountListResponse` juga diberi bidangnya.

Alasannya: tanpa bidang pada response, penanda ini menjadi **hanya-tulis**. Layar dapat
mengirimnya tetapi tidak pernah dapat menampilkannya kembali, sehingga `FE-ACC-P2-007` mustahil
menampilkan akun mana yang bertanda, dan pengguna tidak punya cara memastikan centangnya tersimpan.
Ditambahkan pada `ChartOfAccountListResponse` supaya `ChartOfAccountDetailResponse` ikut
memperolehnya lewat pewarisan — satu bidang, dua respons.

Dua permukaan lain **sengaja tidak** disentuh dan diserahkan ke task pemiliknya:

| Yang tidak ditambahkan | Alasan | Pemilik |
| --- | --- | --- |
| `ChartOfAccountOptionResponse` | Isian pilihan pada form jurnal. Apakah layar jurnal menyaring control account sejak di daftar pilihan, atau membiarkan backend menolaknya `422`, adalah keputusan UI yang belum diambil | `BE-ACC-P2-012` / `FE-ACC-P2-007` |
| `ChartOfAccountPagedQuery.IsControlAccount` | Penyaring daftar. Berguna untuk layar "daftar control account", tetapi `BE-ACC-P2-013` merancang endpoint rekonsiliasi tersendiri | `BE-ACC-P2-013` |

**Catatan penamaan.** Kartu roadmap menulis `CreateChartOfAccountDto` dan
`UpdateChartOfAccountDto`; nama sebenarnya di source adalah `CreateChartOfAccountRequest` dan
`UpdateChartOfAccountRequest`. Implementasi mengikuti source. Roadmap **tidak diubah** — wewenang
pada `roadmap/**` terbatas pada baris status dan tautan bukti.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false --no-incremental` | `0 Error(s)`, `145 Warning(s)`, 00:02:47 | `PASS` | **145 sama persis** dengan sebelum task ini — nol warning baru |
| Uji `BE-ACC-P2-011`, 5 uji | `Failed: 0, Passed: 5` | `PASS` | `AccChartOfAccountControlAccountTests` |
| Seluruh uji Accounting, 13 uji | `Failed: 0, Passed: 13` | `PASS` | 8 dari `BE-ACC-P2-003` + 5 dari task ini |
| Seluruh project `UnitTests.Sqlite`, 461 uji | `Failed: 3, Passed: 458` | `EXISTING / ENVIRONMENT ISSUE` | **Nol regresi** — lihat di bawah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` atas 5 berkas | `VIOLATION: 0`, `PASS` | `PASS` | Keluaran checker |
| `git status --short -- Migrations/` | Nol berkas | `PASS` | Snapshot EF tidak tersentuh |

### Rincian lima uji

| Uji | Yang dibuktikan | Hasil |
| --- | --- | :---: |
| `Kolom_TidakNullableDanBerbawaanFalse` | **Acceptance 1.** Tipe `bool`, `IsNullable = false`, `GetDefaultValue() = false` | `PASS` |
| `Index_IsControlAccount_Terpasang` | **Acceptance 2.** Index satu kolom terpasang, dan **bukan** unique | `PASS` |
| `AkunYangDisimpanTanpaMenyentuhPenanda_TetapBukanControlAccount` | **Acceptance 4.** Akun disimpan tanpa menyentuh penanda sama sekali — meniru baris yang sudah ada sebelum kolom ini lahir — terbaca kembali `false` | `PASS` |
| `PenandaTersimpan_DanTerbacaKembali` | Penanda benar-benar tersimpan, dan akun non-control tetap `false` | `PASS` |
| `PenyaringanMenurutPenanda_HanyaMengembalikanControlAccount` | Bentuk query yang akan dipakai `BE-ACC-P2-013` mengembalikan tepat dua akun bertanda | `PASS` |

### Bukti nol regresi

Jumlah uji lulus naik dari **450 menjadi 458**, tepat sebanyak 8 uji yang ditambahkan sesudah
pengukuran baseline (3 dari `BE-ACC-P2-003`, 5 dari task ini). **Nol uji yang tadinya lulus menjadi
gagal.**

Ketiga kegagalan adalah `SwaggerDocumentationTests` milik `MedicalRecordManagement` yang **sudah
gagal sebelum task ini** dan sudah dilaporkan pada `be-acc-p2-003`: `.csproj` sengaja mematikan
`GenerateDocumentationFile` pada Release. Nama ketiganya identik dengan yang tercatat sebelumnya.

**Tidak dijalankan:**

- **`dotnet ef migrations add` — sesuai instruksi owner 9 September 2026, pekerjaan berhenti tepat
  sebelum titik ini.** `BE-ACC-P2-004` dikerjakan owner sendiri.
- `dotnet ef database update` dan seluruh perintah database.
- Uji integrasi PostgreSQL — belum diperlukan; kolom dan index dibuktikan pada tingkat model EF,
  dan penegakan perilakunya baru lahir di `BE-ACC-P2-012` yang roadmap-nya memang menuntut uji
  integrasi PostgreSQL.

Uji manual: `NOT APPLICABLE` — belum ada perilaku yang dapat dicoba pengguna sampai `012` berdiri.

---

## 6. Acceptance criteria dan Definition of Done

### Acceptance criteria

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Kolom `bool` **tidak nullable**, bawaan `false` | **Terpenuhi** | `Kolom_TidakNullableDanBerbawaanFalse` |
| 2 | Index `(IsControlAccount)` terpasang | **Terpenuhi** | `Index_IsControlAccount_Terpasang` |
| 3 | Kedua DTO memuat bidangnya | **Terpenuhi, dan lebih** | `CreateChartOfAccountRequest` dan `UpdateChartOfAccountRequest`; ditambah `ChartOfAccountListResponse` sebagai delta kontrak bagian 4 |
| 4 | **Akun yang sudah ada tetap bernilai `false`** tanpa perlu pengisian data | **Terpenuhi** | `AkunYangDisimpanTanpaMenyentuhPenanda_TetapBukanControlAccount`, ditambah `HasDefaultValue(false)` pada configuration yang membuat baris lama terisi sendiri saat migration diterapkan |
| 5 | Build lulus | **Terpenuhi** | `0 Error(s)`, 145 warning, seluruhnya pre-existing |

**Lima dari lima terpenuhi.**

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Build lulus | **Ya** — `0 Error(s)` |
| **Nol migration** — kolomnya ikut `BE-ACC-P2-004` | **Ya** |
| Snapshot tidak berubah | **Ya** — `git status --short -- Migrations/` nol berkas |
| Laporan tracked ada | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `✅` pada kartu task, baris `Status`, tabel ringkasan, baris dependency `004` dan `012`, serta baris jalur control account |
| `requirement-traceability-phase2.md` diperbarui | **Ya** — `FR-P2-035`, dan `task_selesai` dinaikkan menjadi 4 |

**Nol butir DoD dikecualikan.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 145 warning solution, seluruhnya pre-existing. **Nol berasal dari task ini** |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **`PUT` yang tidak mengirim `IsControlAccount` akan melepas penandanya.** Ini perilaku `PUT` yang sudah berlaku pada endpoint itu — `IsPostable` bersifat sama persis — jadi **bukan cacat baru**, tetapi akibatnya di sini lebih besar: melepas penanda diam-diam membuka kembali akun kas ke jurnal manual. Bila ini dianggap terlalu berisiko, bentuk yang tepat adalah `PATCH /{id}/control-account` tersendiri, dan itu **keputusan kontrak yang belum diambil** | Rizki |
| 2 | `ChartOfAccountOptionResponse` dan penyaring `IsControlAccount` sengaja tidak ditambahkan — bagian 4 | `BE-ACC-P2-012` / `013` |
| 3 | `SwaggerDocumentationTests` tidak dapat lulus pada Release — temuan `be-acc-p2-003`, belum tertangani | Owner Backend |
| 4 | Delta `Cascade` lawan `Restrict` dari `BE-ACC-P2-001` — masih menunggu ratifikasi | Rizki |
| 5 | Selisih dua salinan registry (`ACC-DEP-007`) | Lead |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Penanda belum berakibat apa pun** | Kolomnya ada, tetapi jurnal manual ke control account **masih diterima** sampai `BE-ACC-P2-012` berdiri. Menandai akun sekarang memberi rasa aman yang belum ada dasarnya — sebaiknya pengisian datanya menunggu `012` |
| **Kolom belum ada di database** | Setiap kode yang membaca `IsControlAccount` akan gagal saat dijalankan sampai migration diterapkan. Ini bentuk yang dikehendaki roadmap |
| **Acceptance (2) `BE-ACC-P2-012` adalah yang paling berbahaya** | Roadmap sudah menandainya: bila aturan penolakan ikut mengenai jalur otomatis — kejadian akuntansi, template berulang, jurnal penutup — seluruh posting Phase 2 mati tanpa error yang menjelaskan sebabnya |

### Status Git

```text
 M Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/DTOs/ChartOfAccountDtos.cs
 M Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Models/AccChartOfAccount.cs
 M Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Services/AccChartOfAccountService.cs
 M Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/JournalTypeController.cs
 M Areas/Corporate/AccountingManagement/MasterData/JournalType/Services/AccJournalTypeService.cs
 M Areas/Corporate/AccountingManagement/MasterData/Seeders/AccountingMasterDataSeeder.cs
 M Repositories/ApplicationDbContext.cs
 M Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccChartOfAccountConfiguration.cs
 M docs/module-blueprints/accounting/roadmap/backend-roadmap-phase2.md
 M docs/module-blueprints/accounting/roadmap/requirement-traceability-phase2.md
?? Areas/Corporate/AccountingManagement/MasterData/Configuration/
?? Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccAccountingConfigurationConfiguration.cs
?? Tests/QuilvianSystemBackend.UnitTests.Sqlite/AccountingManagement/
?? docs/module-blueprints/accounting/task/report/backend/be-acc-p2-003-...md
?? docs/module-blueprints/accounting/task/report/backend/be-acc-p2-011-...md
```

Working tree memuat hasil `BE-ACC-P2-003` **dan** task ini; keduanya belum di-commit.
**Nol commit, push, stage, merge, atau rebase dilakukan.**

### Langkah berikutnya

**Bahan `BE-ACC-P2-004` kini lengkap.** Migration `AddAccountingPhase2Independent` akan memuat:

| Isi | Jumlah | Dari task |
| --- | :---: | --- |
| Tabel baru | 5 | `AccPeriodClosingApproval` (`001`); tiga tabel jurnal berulang (`002`); `AccAccountingConfiguration` (`003`) |
| Kolom baru | 3 | `ClosingSubmittedBy`, `ClosingSubmittedAt` pada `AccAccountingPeriod` (`001`); `IsControlAccount` pada `AccChartOfAccount` (task ini) |

**Pekerjaan berhenti di sini sesuai instruksi owner.** `BE-ACC-P2-004` — pembuatan dan penerapan
migration — dikerjakan **owner sendiri**. Sebelum menjalankannya, Migration Coordination Gate
([`06-shared-migration-coordination-rule.md`](../../../06-shared-migration-coordination-rule.md))
menuntut tujuh pertanyaan terjawab tertulis, dan acceptance `004` menuntut snapshot bertambah
**tanpa satu pun deletion** beserta `CONTAMINATION GUARD` `CLEAN`.

Sesudah migration diterapkan, yang terbuka adalah `005`, `007`, `009`, dan `012`.
