# Laporan Perubahan Backend — `BE-ACC-P2-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-015` |
| Judul | Entity dan enum master aturan posting |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-015` (revisi 3) |
| Trace | `FR-P2-007`, `FR-P2-008`; `ACC-DEC-045`, `ACC-DEC-058`, `ACC-DEC-074` |
| Contract version | Kamus data bagian 11, 12, 12b; `02-backend-architecture.md` bagian 15.1, 16, 17, 18 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 2 (9 berkas), logika bisnis 0, kontrak API 0, database 2 (entity baru, menuntut migration), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — model, enum, configuration, `DbSet`; laporan ini, baris status roadmap. **Tidak** termasuk migration (`BE-ACC-P2-016`, Rizki) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 14 September 2026 |
| Status | **✅ SELESAI** — 8 dari 8 acceptance terpetakan ke source; build owner `0 error` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/EventType` dan `MasterData/PostingRule` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31. Folder `MasterData/` sudah memuat model persisted `Acc*` sejak `BE-ACC-003`, jadi `QBE-MOD-003` tidak menuntut pendaftaran baru. Salinan suite 1.17.1 tidak memuat `Acc` — `ACC-TD-015` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-NAM-001`/`002` (prefix `Acc`, bukan `Trx`), `QBE-CFG-001` (configuration lengkap), `QBE-MOD-001`, `QBE-MOD-002`, `QBE-ENUM-001` (enum dimiliki modul), `QBE-CODE-004` (kode unik ber-index database) |

---

## 1. Masalah yang diperbaiki

Accounting sudah bisa mencatat jurnal manual, jurnal berulang, dan jurnal penutup tahun. Yang belum
ada adalah **kamus terjemahan** dari kejadian keuangan modul lain menjadi jurnal — misalnya "kasir
menerbitkan faktur rawat jalan Rp 10.000.000" menjadi debit Piutang Penjamin, kredit Pendapatan
Rawat Jalan.

Task ini membuat **wadahnya**: tiga tabel yang menyimpan jenis kejadian dan aturan posting berbaris.
Isinya — daftar jenis kejadian yang sungguh diterbitkan Finance — **belum** ditetapkan dan menunggu
`DEC-ACC-P2-002`. Mesin yang menjalankan aturan ini menunggu gelombang `P2-1`.

---

## 2. Proses bisnis

### 2.1 Bentuk data

| Tabel | Isinya | Contoh |
| --- | --- | --- |
| `AccEventType` | Jenis kejadian yang dikenal Accounting, berlaku untuk semua badan hukum | `PENGAKUAN-PIUTANG` — "Pengakuan piutang pasien", modul asal `Finance` |
| `AccPostingRule` | Kepala aturan: jenis kejadian mana, pada badan hukum mana, menjadi jurnal jenis apa, dengan perlakuan apa | `PENGAKUAN-PIUTANG` pada PT Metropolitan Medical Centre → jenis jurnal `JU`, perlakuan Buat Draft |
| `AccPostingRuleLine` | Baris-baris aturan: komponen nilai, akun, sisi | Lihat 2.2 |

### 2.2 Contoh berangka — pendapatan rawat jalan dengan jasa medis dokter

| Baris | Komponen | Akun | Sisi |
| ---: | --- | --- | --- |
| 1 | `TOTAL` | `1-1201 Piutang Penjamin` | Debit |
| 2 | `TOTAL` | `4-1001 Pendapatan Rawat Jalan` | Kredit |
| 3 | `JASA_MEDIS` | `5-3001 Beban Jasa Medis` | Debit |
| 4 | `JASA_MEDIS` | `2-1301 Utang Jasa Medis Dokter` | Kredit |

Kejadian bernilai total Rp 10.000.000 dengan komponen `JASA_MEDIS` Rp 3.000.000 kelak menjadi jurnal
empat baris: debit Rp 13.000.000, kredit Rp 13.000.000. Bentuk lama — sepasang akun debit dan kredit
— tidak dapat mengungkapkan ini, itulah sebab `ACC-DEC-058`.

### 2.3 Aturan yang dijaga skema

| Aturan | Penjaga | Contoh akibat bila tidak dijaga |
| --- | --- | --- |
| Kode jenis kejadian unik | Unique `EventTypeCode` | Dua `PENGAKUAN-PIUTANG` — kejadian tidak tahu aturan mana yang berlaku |
| Satu jenis kejadian satu aturan **aktif** per badan hukum | Unique `(LegalEntityId, EventTypeId)` berfilter aturan aktif | Faktur yang sama dibukukan dua kali lewat dua aturan |
| Aturan lama tetap tersimpan | Filter `IsActive = true` pada unique di atas | Tanpa filter, menyimpan riwayat aturan menjadi mustahil |
| Nomor baris unik dalam satu aturan | Unique `(PostingRuleId, LineNumber)` | Dua "baris 2" — urutan jurnal tak tentu |
| Perlakuan bawaan yang aman | `Treatment` bawaan `BuatDraft` (2) | Petugas lupa memilih → jurnal langsung masuk buku besar tanpa diperiksa |
| Baris hilang bersama aturannya | Hanya `PostingRuleId` yang `Cascade` | — |
| Akun, jenis kejadian, badan hukum, jenis jurnal tidak dapat dihapus selama dirujuk | FK lain `Restrict` | Aturan menunjuk akun yang sudah lenyap |

**Jalur tidak normal.** Jenis kejadian tidak punya `LegalEntityId` — sama seperti `AccJournalType`,
ia struktural. Baris aturan tidak punya `LegalEntityId` sendiri — badan hukumnya diturunkan dari
akun, dan kesamaannya ditegakkan service `BE-ACC-P2-018`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `015`–`018`; kamus data bagian 11, 12, 12b; `02-backend-architecture.md` baris
752–762, 889–957; `IdentityModel.cs`; `AccRecurringJournalTemplate`/`Line` beserta configuration
sebagai pola; `AccJournalTypeConfiguration`, `AccChartOfAccountConfiguration`,
`AccAccountingConfigurationConfiguration` (preseden `IsActive` bawaan `true`);
`ApplicationDbContext.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Models/AccEventType.cs` | **Baru.** 4 kolom domain + `IdentityModel` |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Models/AccPostingRule.cs` | **Baru.** `LegalEntityId`, `EventTypeId`, `JournalTypeId`, `Treatment`, `IsActive`, navigasi `Lines` |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Models/AccPostingRuleLine.cs` | **Baru.** `PostingRuleId`, `LineNumber`, `ComponentCode` (bawaan `TOTAL`), `AccountId`, `CostCenterId`, `Side`, `Description` |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Enums/AccountingEventTreatment.cs` | **Baru.** `LangsungSahkan = 1`, `BuatDraft = 2` |
| `Areas/Corporate/AccountingManagement/MasterData/PostingRule/Enums/PostingSide.cs` | **Baru.** `Debit = 1`, `Kredit = 2` |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccEventTypeConfiguration.cs` | **Baru** |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccPostingRuleConfiguration.cs` | **Baru** |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccPostingRuleLineConfiguration.cs` | **Baru** |
| `Repositories/ApplicationDbContext.cs` | 3 `DbSet` dan 2 `using` di region `CORPORATE - ACCOUNTING MANAGEMENT - MASTER DATA` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak membuat endpoint |
| Database | **Model EF bertambah 3 entity; skema belum.** Migration `BE-ACC-P2-016` milik Rizki, belum dibuat. Snapshot tidak berubah. Endpoint lama tidak terdampak karena tidak ada tabel lama yang bertambah kolom |
| Keamanan/Auth | `NOT APPLICABLE` |

### 3.4 Delta terhadap kamus data dan arsitektur

| # | Delta | Alasan |
| ---: | --- | --- |
| 1 | Enum `AccountingEventTreatment` diletakkan di `MasterData/PostingRule/Enums`, bukan `AccountingEvent/Enums` seperti `02-backend-architecture.md` baris 957 | Pemakainya saat ini hanya aturan posting; folder kotak masuk kejadian belum dibangun. Dipindah bila `P2-1` berdiri dan memakainya |
| 2 | Unique `EventTypeCode` dan `(PostingRuleId, LineNumber)` berfilter `"IsDelete" = false` | Konvensi unique berfilter seluruh modul Accounting; kamus data tidak menyebut filter |
| 3 | Unique `(LegalEntityId, EventTypeId)` berfilter `"IsActive" = true AND "IsDelete" = false` | Kamus data menyebut `IsActive = true`; `IsDelete` ditambahkan mengikuti konvensi yang sama |
| 4 | Index tambahan `(IsActive, IsDelete)` pada `AccEventType` | Mendukung `GET /options` yang menyaring jenis aktif |
| 5 | Nama index unique `AccPostingRule` memakai bawaan EF `IX_AccPostingRule_LegalEntityId_EventTypeId` | `AccPostingRuleService` mengenali pelanggarannya lewat nama itu (`BE-ACC-P2-018`). Mengganti nama index kelak wajib mengganti konstanta service |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — endpoint grup Event Type dan Posting Rule dibuat `BE-ACC-P2-017` dan `018`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 | `0 error`, 189 warning | `PASS` | Tangkapan layar owner. Asal kenaikan dari 145 belum dipastikan — lihat `BE-ACC-P2-033` bagian 5 |
| Pembandingan terhadap kamus data bagian 11 | 5 kolom cocok tipe, panjang, wajib, bawaan | `PASS` | `AccEventType.cs`, configuration |
| Pembandingan terhadap kamus data bagian 12 | 6 kolom cocok; FK `Restrict` ke `MstLegalEntity`, `AccEventType`, `AccJournalType` | `PASS` | `AccPostingRuleConfiguration.cs` |
| Pembandingan terhadap kamus data bagian 12b | 8 kolom cocok; `Cascade` ke aturan, `Restrict` ke akun dan cost center | `PASS` | `AccPostingRuleLineConfiguration.cs` |
| Snapshot dan folder migration | Tidak berubah | `PASS` | `git status --short` — nol berkas `Migrations/` |

Uji manual: `NOT APPLICABLE` — tidak ada perilaku yang dapat dijalankan sebelum migration dan
endpoint ada.

**Tidak dijalankan:** automated test (`ACC-DEC-081`); `dotnet ef` (wewenang Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Unique `(EventTypeCode)` pada `AccEventType` | **Terpenuhi** | `HasIndex(x => x.EventTypeCode).IsUnique()` — delta 2 |
| 2 | Unique `(LegalEntityId, EventTypeId)` pada `AccPostingRule` berfilter aturan aktif | **Terpenuhi** | `HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")` — delta 3 |
| 3 | `AccPostingRule.JournalTypeId` wajib, FK ke `AccJournalType` (`ACC-DEC-074`) | **Terpenuhi** | `[Required]`, `IsRequired()`, `HasOne(JournalType)…OnDelete(Restrict)` |
| 4 | Unique `(PostingRuleId, LineNumber)` pada `AccPostingRuleLine` | **Terpenuhi** | Configuration baris 78–80 |
| 5 | `Treatment` berbawaan `BuatDraft`; `ComponentCode` berbawaan `TOTAL` | **Terpenuhi** | Initializer model **dan** `HasDefaultValue` pada kedua configuration |
| 6 | Hanya `AccPostingRuleLine.PostingRuleId` yang `Cascade`; FK lain `Restrict` | **Terpenuhi** | 1 `Cascade`, 5 `Restrict` pada ketiga configuration |
| 7 | Enum disimpan sebagai `int` | **Terpenuhi** | `HasConversion<int>()` pada `Treatment` dan `Side` |
| 8 | Nol migration, snapshot tidak berubah | **Terpenuhi** | `git status --short` |

| Butir DoD | Hasil |
| --- | --- |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 189 warning solution — lihat `BE-ACC-P2-033` bagian 5 |
| Masalah yang diketahui | Model EF mendahului skema sampai `BE-ACC-P2-016` diterapkan. `dotnet ef migrations add` berikutnya oleh **siapa pun** di branch ini akan membawa ketiga tabel ini |
| Risiko tersisa | `IsActive` bawaan `true` di database: EF tidak mengirim nilai `false` saat **insert**, sehingga baris yang sengaja dibuat nonaktif akan tersimpan aktif. Aman hari ini karena kedua service selalu membuat baris aktif dan penonaktifan memakai `UPDATE` — pola yang sama dengan `AccChartOfAccount` dan `AccJournalType` |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 sebelumnya; sesi ini memverifikasi tanpa menyunting ulang |
| Status Git | Lihat [`BE-ACC-P2-033`](BE-ACC-P2-033.md) bagian 7 — batch yang sama. Berkas milik task ini: 5 berkas di `MasterData/EventType/Models` dan `MasterData/PostingRule/{Models,Enums}`, 3 configuration, `ApplicationDbContext.cs`. **Nol commit, push, stage, merge, rebase, atau migration** |

### Langkah berikutnya — isi migration `BE-ACC-P2-016` yang diharapkan

| Operasi | Isi |
| --- | --- |
| `CreateTable` | `AccEventType`, `AccPostingRule`, `AccPostingRuleLine` — ketiganya skema `public` |
| Unique index | `IX_AccEventType_EventTypeCode` (filter `IsDelete = false`); `IX_AccPostingRule_LegalEntityId_EventTypeId` (filter `IsActive = true AND IsDelete = false`); `IX_AccPostingRuleLine_PostingRuleId_LineNumber` (filter `IsDelete = false`) |
| Index biasa | `AccEventType`: `EventTypeName`, `(IsActive, IsDelete)`; `AccPostingRule`: `EventTypeId`, `JournalTypeId`; `AccPostingRuleLine`: `ComponentCode`, `AccountId`, `CostCenterId` |
| Boleh digabung | `DropIndex` + `CreateIndex` `IX_AccJournal_ReversalOfJournalId` dari `BE-ACC-P2-031` |
| Tidak boleh ada | `DropTable`, `DropColumn`, perubahan tabel non-`Acc*` — `CONTAMINATION GUARD` |
