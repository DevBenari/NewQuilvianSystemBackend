# Laporan Perubahan Backend — `BE-ACC-P2-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-020` |
| Judul | Migration kotak masuk (**GATED**, dibuat Rizki) |
| Slice | `P2-1` — Wave B kotak masuk kejadian |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-020` (revisi 4) |
| Trace | `02-backend-architecture.md` bagian 18 migration 2 dan bagian 22.9 |
| Dependency | `BE-ACC-P2-019` |
| Pelaksana | **Rizki** — membuat dan menerapkan migration. Agent hanya memeriksa hasilnya secara read-only |
| Commit | `70ac4240` (branch `rizkiG`), di-push ke `origin/rizkiG`; merge pull `8535dd56` |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — migration `20260924042630_AddAccountingEventInbox` diterapkan ke `QuilvianNewDevRizki`; isi cocok dengan rencana; snapshot nol deletion |

## 1. Yang dibuat

Satu migration, `20260924042630_AddAccountingEventInbox`, yang membangun tempat penyimpanan
kotak masuk kejadian keuangan dan menambah penanda jenis perlakuan pada jenis kejadian.

| Operasi | Objek |
| --- | --- |
| `CreateTable` | `AccAccountingEvent`, `AccAccountingEventAttempt`, `AccAccountingEventComponent` |
| `AddColumn` | `AccEventType.EventKind` — bawaan `1` (`Transaksi`), sehingga jenis kejadian yang sudah terdaftar tidak berubah perilaku |
| `CreateIndex` | 9, termasuk 4 unique: dua kunci anti-ganda kejadian, pasangan nomor percobaan, pasangan kode komponen |

## 2. Bukti

| Pemeriksaan | Diharapkan | Hasil | Sumber |
| --- | ---: | ---: | --- |
| `CreateTable(` pada berkas migration | 3 | **3** | `grep` atas `Migrations/20260924042630_AddAccountingEventInbox.cs`, agent |
| `CreateIndex(` | 9 | **9** | idem |
| `AddColumn<` | 1 | **1** | idem |
| `DropTable(` / `DropColumn(` (seluruhnya bagian `Down`) | 3 / 1 | **3 / 1** | idem |
| `migrationBuilder.Sql` | 0 | **0** | idem |
| Nama objek yang disentuh | Hanya `Acc*` | `AccAccountingEvent`, `AccAccountingEventAttempt`, `AccAccountingEventComponent`, `EventKind` | idem |
| `b.ToTable(` pada snapshot | +3 | **700 → 703** | `git show 70ac4240~1` lawan `70ac4240` |
| Baris snapshot terhapus | 0 | **0** (+336, −0) | `git diff --stat` |
| Status migration | Terapan | `Applying migration '20260924042630_AddAccountingEventInbox'. Done.`; `migrations list` tanpa `(Pending)` | Terminal Rizki |
| Build yang memuat entity `BE-ACC-P2-019` | Berhasil | Dibuktikan tak langsung: `migrations add --no-build` membaca model yang memuat ketiga entity baru dan `EventKind`, yang hanya ada di assembly bila build berhasil | Terminal Rizki + isi migration |

## 3. Akibat

| Hal | Isi |
| --- | --- |
| Kejadian keuangan | Tabelnya ada, tetapi **belum ada endpoint** yang mengisinya — `BE-ACC-P2-021` |
| Jenis kejadian lama | `EventKind = 1` pada semua baris; layar Jenis Kejadian tidak berubah sampai `BE-ACC-P2-022` dan `FE-ACC-P2-013` |
| Mundur | `dotnet ef database update 20260923061124_AddEmergencyEncounterReconciliation --no-build`, lalu `dotnet ef migrations remove --no-build` |

## 4. Berikutnya

`BE-ACC-P2-021`, `022`, dan `024` — boleh paralel.
