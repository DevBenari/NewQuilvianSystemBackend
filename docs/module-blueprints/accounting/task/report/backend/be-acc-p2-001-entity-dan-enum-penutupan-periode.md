# BE-ACC-P2-001 — Entity dan enum penutupan periode

| Field | Nilai |
|---|---|
| Task ID | `BE-ACC-P2-001` |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md) |
| Blueprint | `ACC-BP-001` revisi 11, Phase 2 `approved` Rizki 8 September 2026 |
| Kontrak berlaku | `ACC-STATE-0.2`, kamus data bagian 16 dan 18 |
| Gelombang | `P2-4` tutup bulan |
| Branch | `rizkiG` |
| Source SHA saat mulai | `bdf37e3` |
| Tanggal | 9 September 2026 |
| Status | **`DONE`** |

## Validasi baseline sebelum mulai

| Yang diperiksa | Hasil |
|---|---|
| Roadmap memberi wewenang task ini | Ya, `BE-ACC-P2-001` berstatus `READY`, dependency kosong |
| Blueprint Phase 2 sudah `approved` | Ya, Rizki 8 September 2026 |
| Source SHA bergeser dari baseline roadmap? | **Ya** — roadmap mencatat `02c3219`, HEAD kini `bdf37e3` |
| Apakah pergeseran itu menyentuh Accounting? | **Tidak.** `git diff --name-only 02c3219..bdf37e3 -- Areas/Corporate/AccountingManagement/ Repositories/` menghasilkan **nol berkas**. Selisihnya hanya dokumen blueprint Accounting dan merge dari `QuilvianIntegrationBackend` |
| Kesimpulan | Baseline **tetap sah** untuk task ini; tidak perlu impact scan ulang |

## Backend Governance Preflight

| Field | Nilai |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement / Accounting` |
| Submodule | `AccountingPeriod` |
| Prefix | `Acc` |
| Pemilik | Rizki |
| Status registry | **`ACTIVE`** — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 30 |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-002` (registry terdaftar, lolos), `QBE-MOD-003` / `QBE-NAM-004` (folder sudah terdaftar, lolos), `QBE-NAM-*` (penamaan `Acc` diikuti) |

### Selisih dua salinan registry — dilaporkan, tidak diperbaiki

Registry yang dibaca skill (`rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`,
suite v1.15.0, 101 baris) **tidak memuat baris `Acc` sama sekali**. Pencarian tidak peka huruf
besar-kecil atas `acc` menghasilkan nol kecocokan.

Registry di repository backend (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`)
**memuatnya**:

```
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |
```

Baris itu ditambahkan 8 September 2026 sebagai **propagasi** `ACC-DEC-038` (persetujuan 1
September 2026), bukan persetujuan baru.

Skill menetapkan bahwa bila isi kedua salinan berbeda, **source repository yang berlaku dan
selisihnya dilaporkan**. Karena itu task ini berjalan, dan selisihnya dicatat di sini.

**Ini bukan cacat baru.** `ACC-DEP-007` sudah mencatat selisih dua salinan registry sejak 2
September 2026, tetap terbuka, dan tetap milik lead. Yang berubah hanya arahnya: dulu suite punya
`Acc` dan backend tidak; kini backend punya dan suite tidak. **Tidak ada berkas registry yang saya
sunting.**

## Files inspected

`AccJournalApproval.cs`, `AccJournalApprovalConfiguration.cs`, `AccAccountingPeriod.cs`,
`AccAccountingPeriodConfiguration.cs`, `AccountingPeriodStatus.cs`, `JournalApprovalAction.cs`,
`ApplicationDbContext.cs`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

## 1. Files changed

| # | Berkas | Status |
|---:|---|---|
| 1 | `Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/PeriodClosingAction.cs` | **Baru** |
| 2 | `Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/AccountingPeriodStatus.cs` | Diperbarui |
| 3 | `Areas/Corporate/AccountingManagement/AccountingPeriod/Models/AccPeriodClosingApproval.cs` | **Baru** |
| 4 | `Areas/Corporate/AccountingManagement/AccountingPeriod/Models/AccAccountingPeriod.cs` | Diperbarui |
| 5 | `Repositories/Configurations/Corporate/AccountingManagement/AccountingPeriod/AccPeriodClosingApprovalConfiguration.cs` | **Baru** |
| 6 | `Repositories/ApplicationDbContext.cs` | Diperbarui |

Tiga berkas baru, tiga diperbarui. **Nol berkas di luar modul Accounting** kecuali `DbSet` pada
`ApplicationDbContext`, yang memang satu-satunya tempat pendaftaran `DbSet` di aplikasi ini.

## 2. Entity yang dibuat

### `AccPeriodClosingApproval`

| Kolom | Tipe | Wajib | Keterangan |
|---|---|:---:|---|
| `Id` | `Guid` | Ya | Kunci utama |
| `AccountingPeriodId` | `Guid` | Ya | FK ke `AccAccountingPeriod` |
| `ActionSequence` | `int` | Ya | Nomor urut tindakan dalam satu periode, mulai 1 |
| `Action` | `PeriodClosingAction` | Ya | `HasConversion<int>()` |
| `ActionBy` | `Guid` | Ya | Pelaku tindakan |
| `ActionAt` | `DateTime` | Ya | `timestamp with time zone` |
| `ActionNote` | `string(500)?` | Tidak | Wajib untuk `Rejected`; penegakannya di service |

Ditambah sepuluh kolom audit warisan `IdentityModel`.

## 3. Enum

### `PeriodClosingAction` — baru

`Submitted = 1`, `Approved = 2`, `Rejected = 3`. Meniru `JournalApprovalAction`.

### `AccountingPeriodStatus` — diperbarui

`PendingClosingApproval = 4` **ditambahkan di belakang**. Nilai `Open = 1`, `SoftClosed = 2`, dan
`Closed = 3` **tidak bergeser sedikit pun**.

Ini yang paling berisiko pada task ini: menyisipkan nilai baru di tengah akan mengubah arti angka
yang sudah tersimpan pada baris periode yang ada, dan kerusakannya tidak menimbulkan error apa pun
— hanya statusnya yang berubah diam-diam.

## 4. Configuration yang dibuat

`AccPeriodClosingApprovalConfiguration` — `ToTable`, `HasKey`, seluruh property, kolom waktu
sebagai `timestamp with time zone`, nilai bawaan `IsDelete` dan `IsCancel`, relasi, dan dua index.

| Index | Bentuk |
|---|---|
| Unique | `(AccountingPeriodId, ActionSequence)` berfilter `"IsDelete" = false` |
| Biasa | `ActionBy` |

Filter `"IsDelete" = false` mengikuti pola `AccAccountingPeriodConfiguration` yang sudah ada,
supaya baris terhapus lunak tidak menahan nomor urut.

## 5. DbContext impact

Satu baris `DbSet<AccPeriodClosingApproval> AccPeriodClosingApprovals` di dalam region
`CORPORATE - ACCOUNTING MANAGEMENT - ACCOUNTING PERIOD`. Configuration terdaftar otomatis lewat
`ApplyConfigurationsFromAssembly`; **nol registrasi manual ditambahkan**.

## 6. Delta terhadap kontrak — WAJIB DIRATIFIKASI OWNER

**Satu selisih, dan saya tidak menutupinya.**

| Hal | Kamus data bagian 16 | Yang diimplementasikan | Alasan |
|---|---|---|---|
| Perilaku hapus FK ke periode | `Cascade` | **`Restrict`** | Lihat di bawah |

Kamus data yang saya tulis 8 September menetapkan `Cascade`. Itu **bertentangan dengan kalimat di
kamus data itu sendiri**, yang menyatakan entity ini *"meniru bentuk `AccJournalApproval` yang
sudah ada, dan alasannya sama: ini data bisnis... bukan log teknis"*.

`AccJournalApproval` memakai **`Restrict`**, dengan komentar eksplisit di kodenya: *"riwayat
persetujuan adalah bukti audit dan tidak boleh ikut terhapus bersama jurnalnya."* Alasan yang sama
berlaku persis untuk riwayat penutupan periode.

`Cascade` di kamus data adalah kekeliruan saya saat menulisnya. Implementasi memakai `Restrict`.

**Yang diminta dari owner:** ratifikasi perubahan kamus data bagian 16 dari `Cascade` menjadi
`Restrict`. Sampai itu diberikan, selisih ini tetap tercatat di sini sebagai delta terbuka. Saya
**tidak** mengubah kamus data sepihak.

## 7. Build result

```
dotnet build QuilvianSystemBackend.sln -c Release -p:RunAnalyzers=false
145 Warning(s)
0 Error(s)
Time Elapsed 00:01:28.74
```

**0 error.** Ke-145 warning seluruhnya pre-existing dan berada di project `Tests/` serta konflik
versi `Microsoft.Extensions.DependencyModel` (MSB3277). **Nol warning berasal dari keenam berkas
task ini.**

Dibangun pada konfigurasi `Release` — bukan `Debug|Any CPU` — karena keempat project test tidak
ikut terbangun di sana (`Debug|Any CPU.Build.0` memang tidak ada di `.sln`).

## 8. Test result

**Nol test ditulis pada task ini**, sesuai cakupan roadmap: task ini hanya membuat source model.
Pengujian aturan penutupan menjadi bagian `BE-ACC-P2-005` dan `BE-ACC-P2-006`.

Perlu dicatat jujur: modul Accounting **tidak memiliki satu pun test** saat ini (`ACC-TD-016`,
keputusan owner). Task ini menambah entity tanpa jaring regresi, sama seperti keadaan modul
sekarang.

## 9. Migration status

**NOL MIGRATION DIBUAT.** `dotnet ef migrations add` tidak dijalankan, dan tidak akan dijalankan
pada task ini. Pembuatannya adalah `BE-ACC-P2-004` yang berstatus **`GATED`** dan menuntut
instruksi eksplisit terpisah dari owner.

## 10. Snapshot status

`git status --short -- Migrations/` menghasilkan **nol berkas**. Snapshot EF tidak tersentuh,
database tidak disentuh, nol perintah `dotnet ef` dijalankan.

## 11. Acceptance criteria `BE-ACC-P2-001`

| # | Kriteria | Hasil | Bukti |
|---:|---|:---:|---|
| 1 | `PendingClosingApproval` bernilai **4**, ditambahkan di belakang; nilai 1–3 tidak bergeser | **LULUS** | `AccountingPeriodStatus.cs` — `Open = 1`, `SoftClosed = 2`, `Closed = 3`, `PendingClosingApproval = 4` |
| 2 | Unique `(AccountingPeriodId, ActionSequence)` terpasang | **LULUS** | `AccPeriodClosingApprovalConfiguration.cs`, `HasIndex(...).IsUnique()` |
| 3 | Kedua kolom baru **nullable** | **LULUS** | `ClosingSubmittedBy` bertipe `Guid?`, `ClosingSubmittedAt` bertipe `DateTime?` |
| 4 | Build lulus | **LULUS** | 0 error, 145 warning pre-existing |

**Empat dari empat lulus.**

## 12. Definition of Done

| Butir | Hasil |
|---|---|
| Build lulus | **Ya** — 0 error |
| Nol migration | **Ya** |
| Snapshot tidak berubah | **Ya** |
| Database tidak disentuh | **Ya** |
| Laporan tracked ada | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `✅` pada seluruh titik |

## Security impact

**Nol.** Task ini tidak membuat endpoint, controller, maupun atribut hak akses. Penegakan
`Period : Approve` beserta peran `Accounting Director` menjadi bagian `BE-ACC-P2-006`.

## API contract impact

**Nol.** Tidak ada endpoint dibuat maupun disentuh.

## Warnings

145 warning solution, seluruhnya pre-existing. Nol berasal dari task ini.

## Known issues

| # | Isu | Pemilik |
|---:|---|---|
| 1 | **Delta `Cascade` lawan `Restrict`** pada kamus data bagian 16 — lihat bagian 6. Menunggu ratifikasi | Rizki |
| 2 | Selisih dua salinan registry (`ACC-DEP-007`) — dilaporkan, tidak diperbaiki | Lead |
| 3 | Nol test backend Accounting (`ACC-TD-016`) — entity ini bertambah tanpa jaring regresi | Rizki |

## Task berikutnya

`BE-ACC-P2-002` entity jurnal berulang, atau `BE-ACC-P2-003` pengaturan akuntansi — keduanya
tanpa dependency dan dapat dikerjakan langsung. `BE-ACC-P2-004` migration tetap **`GATED`**.
