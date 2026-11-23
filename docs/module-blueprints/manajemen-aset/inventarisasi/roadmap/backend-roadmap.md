# Inventarisasi Fisik — Roadmap Backend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `inventarisasi`, `AST-INV-1.0.0` `approved` 2026-11-12 |
| Backend SHA | `1f54a175` (`rizkiG`) |
| Task | `BE-AST-020` s.d. `BE-AST-024` (gelombang `MVP-3`) |
| Handoff | `build-module-backend`, satu task per pemanggilan, `TASK MODE: BACKEND` eksplisit; QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-BE` dan `DoD-BE` sama dengan `../../register/roadmap/backend-roadmap.md`; skenario `AT-INV-*`.
`BE-AST-021` juga menyentuh service `pergerakan` dan `register` untuk memasang gerbang opname
(`AST-DES-031`); task itu dilaporkan di sub-modul ini karena perubahannya milik `inventarisasi`.

## Grafik Urutan Dependency

Dipotong menjadi dua blok karena lebar; `BE-AST-021` muncul lagi sebagai sambungan blok kedua.

```text
                      BE-AST-009 [REG] ─────┐
                                            │
                      BE-AST-018 [MOV] ───┐ │
                                          │ │
                      BE-AST-017 [MOV] ─┐ │ │
                                        │ │ │
BE-AST-004 [REG] ─> BE-AST-020 ✅ ──────┴─┴─┴─> BE-AST-021 ✅
```

```text
BE-AST-008 [REG] ─┬─> BE-AST-022 ✅ ─> BE-AST-023 ✅ ─> BE-AST-024
                  │
   BE-AST-021 ✅ ─┘
```

`[REG]` = task backend sub-modul `register` · `[MOV]` = task backend sub-modul `pergerakan`. Cermin baca-saja.

Pasangan prasyarat→task: **9** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-AST-004` [REG] | `BE-AST-020` |
| 2 | `BE-AST-020`, `BE-AST-009` [REG], `BE-AST-017`, `BE-AST-018` [MOV] | `BE-AST-021` |
| 3 | `BE-AST-021`, `BE-AST-008` [REG] | `BE-AST-022` |
| 4 | `BE-AST-022` | `BE-AST-023` |
| 5 | `BE-AST-023` | `BE-AST-024` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-AST-020` | Enam tabel opname + migration | `CAP-07.*` | data dict | `IdentityModel` | `PhysicalInventory/{Models,Enums}`, configuration, DbSet, `AddAssetPhysicalInventory` | `BE-AST-004` | Kartu | `V-BE` | — | `DoD-BE` |
| `BE-AST-021` | Sesi dapat dijadwalkan, dimulai, dibatalkan; unit ditahan | `CAP-07.1`, `AST-DEC-018`; `AST-DES-008`, `031` | API 2.1 (jadwal, mulai, batal), state §1 | `MovementQueryService` [MOV], `AssetPlacementService` [REG] | `StockTakeSessionService`, `StockTakeGateQueryService`, controller; panggilan gerbang di `AssetTransferService`, `AssetLoanService`, `AssetPlacementService` | `BE-AST-020`, `BE-AST-009`, `BE-AST-017`, `BE-AST-018` | Kartu | `V-BE`, `AT-INV-01..03`, `30`, `31`; `AT-MOV-24`; `AT-REG-38` | Menyentuh service dua sub-modul lain | `DoD-BE` |
| `BE-AST-022` | Pemindaian, temuan, pencocokan | `CAP-07.2`, `07.3`; `AST-DES-027..029` | API 2.1 (pindai, temuan, tutup, hasil), state §2 | `/by-number` [REG], `AssetAttachmentService` | `StockTakeScanService`, `StockTakeMatchingService` | `BE-AST-021`, `BE-AST-008` | Kartu | `V-BE`, `AT-INV-04..06`, `32`, `33` | — | `DoD-BE` |
| `BE-AST-023` | Verifikasi, berita acara, tindak lanjut | `CAP-07.4..07.6`, `AST-DEC-019`, `046`; `AST-DES-007`, `030` | API 2.1 (ajukan, verifikasi), 2.2, state §3 | `AssetTransferService` [MOV], `AssetPlacementService`, `AssetConditionService`, `AssetRecordService` [REG] | `StockTakeFollowUpService`, `StockTakeDocumentService`, controller | `BE-AST-022` | Kartu | `V-BE`, `AT-INV-07..10`, `12`, `34..37` | — | `DoD-BE` |
| `BE-AST-024` | Kartu 1.5 dan laporan inventaris | `CAP-01.5`, `11.4`, `AST-DEC-031` | API 2.3–2.4 | — | `StockTakeQueryService`, controller | `BE-AST-023` | Kartu | `V-BE`, `AT-INV-11` | — | `DoD-BE` |

## Kartu task

### ✅ `BE-AST-020` — Tabel opname dan migration `AddAssetPhysicalInventory`

| Field | Isi |
|---|---|
| **Status** | ✅ Selesai, 2026-11-23: 6 model, 4 enum, 6 configuration, 6 DbSet (source `5e810439`); QBE Strict 14 berkas PASS; migration `20261123032511_AddAssetPhysicalInventory` dibuat dan diterapkan Rizki sesudah merge integration `2d76f11b`, berisi 6 `CreateTable` + 27 `CreateIndex` saja. DB dev: 133 kolom, 21 FK Restrict, check tanggal, unik parsial sesi aktif per unit dan ketua per sesi. Migration belum di-commit — [`BE-AST-020.md`](../task/report/backend/BE-AST-020.md) |
| Acceptance criteria | 1. Enam tabel sesuai data dict. 2. Unik parsial satu sesi aktif per unit; satu ketua per sesi. 3. Migration hanya menambah enam tabel |

### ✅ `BE-AST-021` — Sesi opname dan gerbang penahanan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 23 November 2026.** `StockTakeSessionService` (+`.Query`), `AssetStockTakesController` (8 operasi, resource `AssetStockTake` aksi `Read`/`Create`/`Start`/`Cancel`), gerbang `AssetStockTakeGate` diisi, mutasi/pinjaman kini `AST-VAL-MOV-006` + nama unit, deret `AST-OPN`. QBE Strict 13 berkas `PASS`; nol baris komentar; build Rizki DLL 12:06:17. Runtime **45/45** (putaran 1 39/44 = skrip membandingkan status sebagai teks): `AT-INV-01`, `02`, `03`, `30`, `31`, `AT-MOV-24`, `AT-REG-38` lulus; DB: snapshot 44 baris = `ExpectedCount`, aset dipinjam keluar tercatat unit peminjamnya. Pinjaman `Dipinjam` tidak menahan mulai (tafsiran `AST-DES-027`). Delta: +`filters/metadata`, +`summary`, `startBlockers`, body `{ rowVersion }` pada mulai — [`BE-AST-021.md`](../task/report/backend/BE-AST-021.md) |
| Acceptance criteria | 1. Jadwal dengan tim tepat satu ketua. 2. Mulai menolak bila ada penyerahan/mutasi/pinjaman berjalan di unit, dan menyebut nomornya. 3. Snapshot aset diharapkan dibekukan, termasuk aset dipinjam keluar. 4. `IsUnitUnderStockTake` dipanggil pengajuan mutasi, pinjaman, dan penyerahan (`AST-VAL-MOV-006`, `AST-VAL-REG-026`). 5. Batal hanya Kepala Bagian Aset |

### ✅ `BE-AST-022` — Pemindaian dan pencocokan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 23 November 2026.** `StockTakeScanService` (lookup, pindai, temuan, daftar diharapkan, riwayat), `StockTakeMatchingService` (enam kategori, catatan tim terbawa saat tutup ulang), tutup/buka kembali di `StockTakeSessionService.Counting`; 8 endpoint baru, resource `AssetStockTake` 6 aksi. QBE Strict 9 berkas `PASS`; nol baris komentar; build Rizki DLL 14:40:09. Runtime **37/37**: `AT-INV-04`, `05`, `06`, `32`, `33`, `AST-DES-028`, validasi `010..016`, `020`; ketua tim tanpa `Create` memulai dan menutup sesi. DB: 9 pindaian (1 digantikan), 46 hasil aktif + 46 lama terhapus lunak, 2 foto `StockTakeScan`. Delta: +`GET /{id}/lookup`; `PUT results/{id}` dan izin unduh foto menyusul `BE-AST-023` — [`BE-AST-022.md`](../task/report/backend/BE-AST-022.md) |
| Acceptance criteria | 1. Hanya anggota tim saat `Berjalan`. 2. Pindai ulang menggantikan (`IsLatest`). 3. Temuan wajib keterangan + foto. 4. Tutup menghitung enam kategori; buka kembali dengan alasan. 5. Aset unit lain yang ditemukan → `BedaLokasi` (`AST-DES-028`) |

### ✅ `BE-AST-023` — Verifikasi, berita acara, tindak lanjut

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 23 November 2026.** `StockTakeSessionService.Verification` (catatan tim, ajukan, verifikasi/kembalikan, PDF), `StockTakeFollowUpService` (usulan per selisih saat verifikasi; proses klaim → dokumen tujuan → isi target; abaikan), `StockTakeDocumentService` (QuestPDF), controller `AssetStockTakeFollowUps` (`Read`/`Process`/`Dismiss`); register + `AssetPlacementService.CreateStockTakeCorrectionAsync` dan izin unduh foto `StockTakeScan`. QBE Strict 14 berkas `PASS`; nol baris komentar; build Rizki DLL 15:26:45. Runtime **40/40** (putaran 1 39/40 = skrip memilih aset yang sudah terkunci pemanfaatan lama): `AT-INV-07`, `08`, `09`, `10`, `12`, `34`, `35`, `36`, `37`, `AST-DEC-019`, validasi `020..023`, `030`, `031`; PDF berita acara 2 halaman dibaca langsung. Delta: +`GET /follow-ups/{id}`, `draft` pada proses temuan, `followUps[]` per hasil, dokumen hanya saat `Diverifikasi` — [`BE-AST-023.md`](../task/report/backend/BE-AST-023.md) |
| Acceptance criteria | 1. Ajukan mewajibkan catatan selisih. 2. Verifikator Kepala Bagian Aset bukan anggota tim. 3. Verifikasi membuat usulan tanpa mengubah aset; linimasa `Diopname`. 4. Proses usulan membuat dokumen tujuan sesuai jenis (`AST-DES-007`); abaikan dengan alasan. 5. PDF berita acara (`AST-DEC-046`) |

### `BE-AST-024` — Dashboard 1.5 dan laporan inventaris

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Kartu: sesi lewat tanggal belum diverifikasi + usulan `Usulan`. 2. Laporan per sesi dan kategori, ekspor |
