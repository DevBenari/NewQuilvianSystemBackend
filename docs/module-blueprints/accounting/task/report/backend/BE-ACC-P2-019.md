# Laporan Perubahan Backend — `BE-ACC-P2-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-019` |
| Judul | Entity kotak masuk kejadian dan `EventKind` |
| Slice | `P2-1` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-019` (revisi 4, `APPROVED` 24 September 2026) |
| Trace | `FR-P2-001`, `002`, `003`, `006`; `ACC-DEC-035`, `046`, `048`, `058`, `060`, `075`, `084`, `085`, `087` |
| Contract version | Kamus data bagian 9, 10, 11 (`EventKind`), 12c; `02-backend-architecture.md` bagian 15–18 dan 22.9 (`approved`, `GATE-DESAIN-0924`) |
| Dependency | `GATE-DESAIN-0924` ✅ — dibuka Rizki 24 September 2026 |
| Klasifikasi | `MEDIUM` — entity + configuration baru, satu entity lama diperbarui; nol logika bisnis, nol endpoint, nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — model, enum, configuration, `DbSet`; laporan ini; baris status roadmap dan traceability. **Tidak** termasuk migration (`BE-ACC-P2-020`, dibuat Rizki), build, maupun commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6818789d` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **🟡 SEBAGIAN** — 5 dari 5 acceptance terpetakan ke source; **build owner belum dijalankan** (butir Verifikasi dan DoD) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `AccountingEvent` (baru) dan `MasterData/EventType` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 38. Submodule `AccountingEvent` berada di bawah modul terdaftar yang sama; nama foldernya ditetapkan `02-backend-architecture.md` bagian 17 |
| Keberlakuan | `NEW CODE` (tiga entity, dua enum, tiga configuration); `TOUCHED LEGACY` pada `AccEventType` dan `AccEventTypeConfiguration` (buatan `BE-ACC-P2-015`, bukan legacy lama) |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`/`002` (prefix `Acc`), `QBE-MOD-001`/`002`, `QBE-CFG-001`, `QBE-ENUM-001`, `QBE-CODE-004` (dua unique index anti-ganda) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry. Folder `agents/rules/` dan `.codex/` tidak ditemukan |

## 1. Yang dikerjakan

Bentuk data kotak masuk kejadian keuangan — tempat Accounting menyimpan setiap kejadian dari
Finance, riwayat percobaan pemrosesannya, dan rincian nilainya — kini ada di kode, siap dijadikan
migration oleh Rizki pada `BE-ACC-P2-020`. Jenis kejadian bertambah satu penanda, `EventKind`, yang
memisahkan kejadian yang dijurnal dari pesan saldo subledger yang tidak pernah dijurnal.

## 2. Bentuk data

| Tabel | Isinya | Contoh |
| --- | --- | --- |
| `AccAccountingEvent` | Satu baris per kejadian yang pernah diterima, berhasil maupun tidak | `EVT-100`, `PENGAKUAN-PIUTANG`, Rp 10.000.000, status `Terjurnal`, menunjuk jurnal `JU/2026/09/00042` |
| `AccAccountingEventAttempt` | Riwayat tiap percobaan pemrosesan | Percobaan 1 gagal "timeout", percobaan 2 berhasil |
| `AccAccountingEventComponent` | Rincian nilai di samping totalnya | `POTONGAN` Rp 500.000 |
| `AccEventType.EventKind` *(kolom baru)* | `Transaksi` = dijurnal; `SaldoSubledger` = disimpan sebagai saldo rekonsiliasi | `PENGAKUAN-PIUTANG` → `Transaksi`; usulan `SALDO-SUBLEDGER` → `SaldoSubledger` |

### Aturan yang dijaga skema

| Aturan | Penjaga | Contoh akibat bila tidak dijaga |
| --- | --- | --- |
| Nomor kejadian unik (anti-ganda lapis pertama) | Unique `(EventNumber)` | `EVT-100` terkirim tiga kali menjadi tiga jurnal |
| Kejadian yang sama dengan nomor baru tetap tertangkap (lapis kedua) | Unique `(SourceModule, SourceTransactionId, EventTypeCode, SourceVersion)` — memakai **kode** jenis, bukan FK (`ACC-DEC-075`) | Finance keliru mengirim `EVT-101` untuk transaksi yang sama → dua jurnal |
| Satu nomor percobaan per kejadian | Unique `(AccountingEventId, AttemptNumber)` | Riwayat percobaan kembar |
| Satu komponen per kejadian | Unique `(AccountingEventId, ComponentCode)` | Komponen dikirim dua kali, angkanya ganda |
| Jenis kejadian lama tetap `Transaksi` | Bawaan `EventKind = 1` | Jenis yang sudah terdaftar tiba-tiba tidak dijurnal |

## 3. Pemetaan acceptance

| # | Acceptance kartu | Bukti di source | Status |
| ---: | --- | --- | :---: |
| 1 | Dua unique index anti-ganda terpisah | `AccAccountingEventConfiguration` — `HasIndex(x => x.EventNumber).IsUnique()` dan `HasIndex(x => new { SourceModule, SourceTransactionId, EventTypeCode, SourceVersion }).IsUnique()` | ✅ |
| 2 | `EventTypeId` boleh kosong, `EventTypeCode` wajib | `AccAccountingEvent.EventTypeId` bertipe `Guid?`; `EventTypeCode` `[Required]` + `IsRequired()` | ✅ |
| 3 | `EventKind` wajib, bawaan `Transaksi` | `AccEventType.EventKind`; `AccEventTypeConfiguration` — `HasConversion<int>().HasDefaultValue(EventTypeKind.Transaksi).IsRequired()` | ✅ |
| 4 | Kolom sensitif tidak disentuh logger | Task ini tidak menambah satu pun pemanggilan logger; penegakannya jatuh pada service `021`/`024` | ✅ |
| 5 | Nol kolom identitas pasien | Kolom `AccAccountingEvent` hanya nomor transaksi asal dan penelusuran (`ACC-DEC-056`) | ✅ |

## 4. Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Enums/AccountingEventStatus.cs` | Baru | Enam nilai, termasuk `Tercatat = 6` (`ACC-DEC-087`) |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Models/AccAccountingEvent.cs` | Baru | Kamus data bagian 9 + `HoldReasonCode` (`ACC-DEC-085`) |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Models/AccAccountingEventAttempt.cs` | Baru | Kamus data bagian 10 |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Models/AccAccountingEventComponent.cs` | Baru | Kamus data bagian 12c |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Enums/EventTypeKind.cs` | Baru | `Transaksi = 1`, `SaldoSubledger = 2` |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Models/AccEventType.cs` | Diperbarui | + `EventKind` |
| `Repositories/Configurations/Corporate/AccountingManagement/AccountingEvent/AccAccountingEventConfiguration.cs` | Baru | Kolom, tiga FK `Restrict` (`MstLegalEntity`, `AccEventType`, `AccJournal`), dua unique, lima index |
| `Repositories/Configurations/Corporate/AccountingManagement/AccountingEvent/AccAccountingEventAttemptConfiguration.cs` | Baru | FK `Cascade`, unique pasangan |
| `Repositories/Configurations/Corporate/AccountingManagement/AccountingEvent/AccAccountingEventComponentConfiguration.cs` | Baru | FK `Cascade`, unique pasangan |
| `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccEventTypeConfiguration.cs` | Diperbarui | Pemetaan `EventKind` |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | Tiga `DbSet` di region baru dan satu `using`. Configuration terdaftar otomatis lewat `ApplyConfigurationsFromAssembly` |

## 5. Keputusan implementasi yang perlu diketahui

| Hal | Pilihan | Alasan |
| --- | --- | --- |
| Filter `IsDelete` pada unique index anti-ganda | **Tidak** difilter | Kejadian tidak pernah dihapus; kunci anti-ganda harus berlaku mutlak. Berbeda dari index master data yang memakai filter `"IsDelete" = false` |
| Tipe waktu | `EventOccurredAt`, `AttemptedAt` → `DateTimeOffset` + `timestamp with time zone`; `AccountingDate`, `DocumentDate` → `DateTime` + `date` | Mengikuti `AccNumberSeries.LastAllocatedAt` dan `AccJournal.AccountingDate` |
| `IgnoreReason`, `HoldReasonCode` | Kolom teks biasa, bukan enum | Kamus data menetapkannya `string`; `HoldReasonCode` dikirim apa adanya di tanda terima |
| Komentar kode | Nol komentar baris `//` di kode baru | Konvensi owner; alasan ditulis di laporan ini |

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| Tinjauan diff dan cakupan | Hanya berkas pada tabel bagian 4; nol berkas di luar modul Accounting selain `ApplicationDbContext.cs` |
| Kesesuaian nama (`QBE-NAM`) | Entity, file, configuration, `DbSet` jamak, dan tabel tunggal PascalCase satu paket |
| `dotnet build` | **Belum** — dijalankan owner: `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false` |
| Migration | **Tidak dibuat** — `BE-ACC-P2-020`, oleh Rizki |
| Automated test | Tidak dijalankan (`ACC-DEC-081`) |

## 7. Risiko dan langkah berikutnya

| Hal | Isi |
| --- | --- |
| Model berubah tanpa migration | Sampai `BE-ACC-P2-020` dibuat, `dotnet ef migrations has-pending-model-changes` akan melapor perubahan tertunda. Ini diharapkan |
| Menaikkan ke ✅ | Setelah build owner `0 error` |
| Berikutnya | `BE-ACC-P2-020` (migration, Rizki), lalu `021`, `022`, `024` boleh paralel |
