# BE-ACC-P2-002 — Entity dan enum jurnal berulang

| Field | Nilai |
|---|---|
| Task ID | `BE-ACC-P2-002` |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md) |
| Blueprint | `ACC-BP-001` revisi 11, Phase 2 `approved` Rizki 8 September 2026 |
| Kontrak berlaku | Kamus data bagian 13, 14, 15 |
| Gelombang | `P2-3` jurnal berulang |
| Branch | `rizkiG` |
| Source SHA saat mulai | `bdf37e3` |
| Tanggal | 9 September 2026 |
| Status | **`DONE`** |

## Validasi baseline sebelum mulai

| Yang diperiksa | Hasil |
|---|---|
| Roadmap memberi wewenang task ini | Ya, `BE-ACC-P2-002` berstatus `READY`, dependency kosong |
| Blueprint Phase 2 `approved` | Ya |
| Working tree bersih? | Memuat enam berkas dari `BE-ACC-P2-001` yang belum di-commit. **Nol tumpang tindih** dengan berkas task ini kecuali `ApplicationDbContext.cs`, yang memang satu-satunya tempat pendaftaran `DbSet` |
| Kesimpulan | Aman dilanjutkan |

## Backend Governance Preflight

| Field | Nilai |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement / Accounting` |
| Submodule | `RecurringJournal` — **folder baru** |
| Prefix | `Acc` |
| Pemilik | Rizki |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30 |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-002` (registry terdaftar, lolos), `QBE-MOD-003` / `QBE-NAM-004` (folder `RecurringJournal` berada **di dalam** `AccountingManagement` yang sudah terdaftar beserta prefix `Acc`; tidak menuntut entri registry tersendiri), `QBE-NAM-*` (ketiga entity berawalan `Acc`) |

**Selisih dua salinan registry** tetap seperti dilaporkan `BE-ACC-P2-001`: salinan suite v1.15.0
tidak memuat `Acc`, salinan backend memuatnya. Source repository yang berlaku. `ACC-DEP-007`
tetap terbuka dan tetap milik lead. **Nol berkas registry disunting.**

## Files inspected

`AccJournalLine.cs`, `AccJournalLineConfiguration.cs`, `AccJournal.cs`, `AccJournalType.cs`,
`AccAccountingPeriod.cs`, `ApplicationDbContext.cs`, kamus data bagian 13–15.

## 1. Files changed

| # | Berkas | Status |
|---:|---|---|
| 1 | `Areas/Corporate/AccountingManagement/RecurringJournal/Enums/RecurringFrequency.cs` | **Baru** |
| 2 | `Areas/Corporate/AccountingManagement/RecurringJournal/Models/AccRecurringJournalTemplate.cs` | **Baru** |
| 3 | `Areas/Corporate/AccountingManagement/RecurringJournal/Models/AccRecurringJournalTemplateLine.cs` | **Baru** |
| 4 | `Areas/Corporate/AccountingManagement/RecurringJournal/Models/AccRecurringJournalRun.cs` | **Baru** |
| 5 | `Repositories/Configurations/Corporate/AccountingManagement/RecurringJournal/AccRecurringJournalTemplateConfiguration.cs` | **Baru** |
| 6 | `Repositories/Configurations/Corporate/AccountingManagement/RecurringJournal/AccRecurringJournalTemplateLineConfiguration.cs` | **Baru** |
| 7 | `Repositories/Configurations/Corporate/AccountingManagement/RecurringJournal/AccRecurringJournalRunConfiguration.cs` | **Baru** |
| 8 | `Repositories/ApplicationDbContext.cs` | Diperbarui — satu `using`, tiga `DbSet` |
| 9 | `Repositories/Configurations/.../JournalManagement/AccJournalLineConfiguration.cs` | Diperbarui — **komentar saja**, lihat bagian 6 |

## 2. Entity yang dibuat

### `AccRecurringJournalTemplate`

Kepala template: badan hukum, kode, nama, jenis jurnal yang dihasilkan, frekuensi, tanggal terbit,
masa berlaku, dan penanda aktif.

### `AccRecurringJournalTemplateLine`

Baris template. Meniru `AccJournalLine` termasuk `CostCenterId` opsional dan sepasang
`DebitAmount`/`CreditAmount`. **Sengaja tidak membawa `LegalEntityId` sendiri**, sama seperti
`AccJournalLine` — badan hukumnya diturunkan dari akun yang ditunjuk (`ACC-DEC-037`).

### `AccRecurringJournalRun`

Bukti bahwa satu template sudah terbit untuk satu periode. Tabel terpenting pada task ini.

## 3. Enum

### `RecurringFrequency` — baru

**Hanya satu nilai: `Bulanan = 1`.**

Nilai triwulanan dan tahunan sengaja **tidak** ditambahkan, walaupun kamus data menyediakan
ruangnya. Enum yang memuat nilai tanpa penanganan akan **lolos validasi lalu gagal diam-diam**
saat penjadwal mencoba memakainya — template bertanda triwulanan tidak akan pernah terbit, dan
tidak ada error apa pun yang memberi tahu.

Menambahkannya nanti hanya berarti menambah satu nilai di belakang, tanpa menggeser yang ada.

## 4. Configuration yang dibuat

| Tabel | Index | Check constraint |
|---|---|---|
| `AccRecurringJournalTemplate` | Unique `(LegalEntityId, TemplateCode)` berfilter `IsDelete = false`; index `IsActive` | `CK_AccRecurringJournalTemplate_DayOfMonth_1_28` |
| `AccRecurringJournalTemplateLine` | Unique `(TemplateId, LineNumber)` berfilter `IsDelete = false`; index `AccountId`, `CostCenterId` | `CK_AccRecurringJournalTemplateLine_TepatSatuSisiTerisi` |
| `AccRecurringJournalRun` | **Unique `(TemplateId, AccountingPeriodId)` TANPA filter**; index `JournalId` | — |

### Dua keputusan configuration yang perlu dijelaskan

**Penjaga terbit ganda sengaja TANPA filter `IsDelete`.** Seluruh unique index lain di modul ini
memakai filter `"IsDelete" = false`. Yang ini tidak, dan itu disengaja: menghapus lunak catatan
penerbitan lalu menerbitkan ulang akan menghasilkan **jurnal kedua untuk bulan yang sama** —
persis yang dijaga index ini. Bila sebuah penerbitan memang keliru, jurnalnya dibalik lewat
pembalikan jurnal, bukan dengan menghapus jejak penerbitannya.

**`DayOfMonth` dibatasi di database, bukan hanya di service.** Tanggal 29, 30, dan 31 tidak ada di
setiap bulan. Template bertanggal itu akan **terlewat pada Februari tanpa menimbulkan error apa
pun** — kegagalan diam yang baru ketahuan saat seseorang menyadari beban penyusutan Februari
hilang.

## 5. DbContext impact

Satu `using` dan tiga `DbSet` di dalam region baru
`CORPORATE - ACCOUNTING MANAGEMENT - RECURRING JOURNAL`. Configuration terdaftar otomatis lewat
`ApplyConfigurationsFromAssembly`; **nol registrasi manual**.

## 6. Perubahan pada berkas di luar cakupan — komentar saja

`AccJournalLineConfiguration.cs` memuat komentar:

> *"Satu-satunya relasi Cascade pada modul ini."*

Kalimat itu **menjadi tidak benar** akibat task ini, karena `AccRecurringJournalTemplateLine`
memakai `Cascade` dengan alasan yang sama persis. Komentarnya diperbarui menjadi akurat.

**Nol perubahan perilaku.** Yang disunting hanya blok komentar; `DeleteBehavior.Cascade`,
relasi, dan seluruh property tidak disentuh. Dicatat di sini karena berkas itu berada di luar
cakupan task, dan perubahan sekecil apa pun di luar cakupan wajib disebutkan.

## 7. Delta terhadap kontrak

**Nol delta.** Ketiga tabel, seluruh kolom, tipe, index, dan nilai bawaannya cocok dengan kamus
data bagian 13, 14, dan 15.

Delta `Cascade` lawan `Restrict` dari `BE-ACC-P2-001` **masih terbuka** dan tidak tersentuh task
ini.

## 8. Build result

```
dotnet build QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false
145 Warning(s)
0 Error(s)
Time Elapsed 00:01:50.57
```

**0 error.** Jumlah warning **145, sama persis** dengan build `BE-ACC-P2-001` — artinya kesembilan
berkas task ini menyumbang **nol warning baru**.

## 9. Test result

**Nol test ditulis**, sesuai cakupan roadmap: task ini hanya membuat source model. Pengujian
penjaga terbit ganda menjadi bagian `BE-ACC-P2-008`, dan roadmap sudah menetapkan ia **wajib diuji
terhadap PostgreSQL sungguhan** dengan dua penerbitan bersamaan pada dua koneksi terpisah.

Menguji penjaga itu secara berurutan **tidak membuktikan apa pun** — yang dijaga justru dua proses
yang berjalan bersamaan.

## 10. Migration status

**NOL MIGRATION DIBUAT.** `dotnet ef migrations add` tidak dijalankan. Pembuatannya adalah
`BE-ACC-P2-004` yang berstatus **`GATED`**.

## 11. Snapshot status

`git status --short -- Migrations/` menghasilkan **nol berkas**. Snapshot EF tidak tersentuh,
database tidak disentuh, nol perintah `dotnet ef` dijalankan.

## 12. Acceptance criteria `BE-ACC-P2-002`

| # | Kriteria | Hasil | Bukti |
|---:|---|:---:|---|
| 1 | **Unique `(TemplateId, AccountingPeriodId)`** pada `AccRecurringJournalRun` | **LULUS** | `AccRecurringJournalRunConfiguration.cs`, `HasIndex(...).IsUnique()` tanpa filter |
| 2 | `DayOfMonth` dibatasi 1–28 | **LULUS** | Check constraint `CK_AccRecurringJournalTemplate_DayOfMonth_1_28` |
| 3 | `IsActive` berbawaan `false` | **LULUS** | Property `= false` pada model, `HasDefaultValue(false)` pada configuration |
| 4 | Build lulus | **LULUS** | 0 error |

**Empat dari empat lulus.**

## 13. Definition of Done

| Butir | Hasil |
|---|---|
| Build lulus | **Ya** — 0 error |
| Nol migration | **Ya** |
| Snapshot tidak berubah | **Ya** |
| Database tidak disentuh | **Ya** |
| Laporan tracked ada | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `✅` pada seluruh titik |

## Security impact

**Nol.** Nol endpoint, controller, maupun atribut hak akses dibuat.

## API contract impact

**Nol.**

## Warnings

145 warning solution, seluruhnya pre-existing di project `Tests/` dan konflik versi
`Microsoft.Extensions.DependencyModel`. **Nol berasal dari task ini.**

## Known issues

| # | Isu | Pemilik |
|---:|---|---|
| 1 | Delta `Cascade` lawan `Restrict` dari `BE-ACC-P2-001` — masih menunggu ratifikasi | Rizki |
| 2 | Selisih dua salinan registry (`ACC-DEP-007`) — dilaporkan, tidak diperbaiki | Lead |
| 3 | Nol test backend Accounting (`ACC-TD-016`) | Rizki |
| 4 | Check constraint `TepatSatuSisiTerisi` **mustahil dipenuhi di SQLite** (`ACC-TD-001`, EF menyimpan decimal sebagai TEXT). Pengujiannya wajib PostgreSQL | Owner Backend |

## Task berikutnya

`BE-ACC-P2-003` entity pengaturan akuntansi dan jenis jurnal `JT` — tanpa dependency, satu tabel,
paling kecil. Sesudah itu ketiga entity siap dan `BE-ACC-P2-004` migration dapat dibuat sekaligus
dalam satu berkas — tetapi tetap **`GATED`**, menunggu instruksi terpisah owner.
