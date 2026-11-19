# Pergerakan Aset — Roadmap Backend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `pergerakan`, `AST-MOV-1.0.0` `approved` 2026-11-12 |
| Backend SHA | `1f54a175` (`rizkiG`) |
| Task | `BE-AST-016` s.d. `BE-AST-019` (gelombang `MVP-2`) |
| Handoff | `build-module-backend`, satu task per pemanggilan, `TASK MODE: BACKEND` eksplisit; QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-BE` dan `DoD-BE` sama dengan `../../register/roadmap/backend-roadmap.md`; skenario `AT-MOV-*`.
Penahanan saat opname (`AST-VAL-MOV-006`) dipasang `BE-AST-021` sub-modul `inventarisasi` (`AST-DES-026`),
bukan task di sini.

## Grafik Urutan Dependency

```text
                   BE-AST-009 [REG] ─┐
                                     │
BE-AST-004 [REG] ─> BE-AST-016 ✅ ─┬────┴─> BE-AST-017 ✅ ─────────────────┐
                                │                                    │
                                │  BE-AST-010 [REG] ─┐               │
                                │                    │               │
                                └────────────────────┴─> BE-AST-018 ─┴─> BE-AST-019
```

`[REG]` = task backend sub-modul `register`, cermin baca-saja.

Pasangan prasyarat→task: **7** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-AST-004` [REG] | `BE-AST-016` |
| 2 | `BE-AST-016` + `BE-AST-009` / `BE-AST-010` [REG] | `BE-AST-017`, `BE-AST-018` — boleh paralel |
| 3 | `BE-AST-017`, `BE-AST-018` | `BE-AST-019` |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-AST-016` | Lima tabel pergerakan + migration | `CAP-05.*` | data dict | `IdentityModel` | `Movement/{Models,Enums}`, configuration, DbSet, `AddAssetMovement` | `BE-AST-004` | Kartu | `V-BE` | — | `DoD-BE` |
| `BE-AST-017` | Mutasi antar unit berjalan penuh | `CAP-05.1..05.3`, `AST-DEC-011`, `048`; `AST-DES-023..025` | API 2.1, state §1 | `AssetPlacementService`, `AssetProcessLockService`, `AssetActorResolver` [REG] | `AssetTransferService`, controller, PDF serah terima | `BE-AST-016`, `BE-AST-009` | Kartu | `V-BE`, `AT-MOV-01..03`, `20..23`, `25..31` | `AST-UNK-004` | `DoD-BE` |
| `BE-AST-018` | Peminjaman dan pengembalian | `CAP-05.4`, `05.5`, `AST-DEC-012`; `AST-DES-021`, `022` | API 2.2, state §2–3 | `AssetConditionService` [REG] | `AssetLoanService`, controller, PDF | `BE-AST-016`, `BE-AST-010` | Kartu | `V-BE`, `AT-MOV-04..06`, `32..35` | `AST-UNK-004` | `DoD-BE` |
| `BE-AST-019` | Kartu 1.4 dan laporan mutasi | `CAP-01.4`, `11.2` | API 2.3–2.4 | — | `MovementQueryService` (ringkasan, laporan, `HasInFlightForUnit`), 1 controller | `BE-AST-017`, `BE-AST-018` | Kartu | `V-BE`, `AT-MOV-07` | — | `DoD-BE` |

## Kartu task

### ✅ `BE-AST-016` — Tabel pergerakan dan migration `AddAssetMovement`

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 19 November 2026.** 4 enum, 5 model, 5 configuration, 5 DbSet (`9514ed6b`); QBE Strict 12 berkas `PASS`, nol baris komentar. Migration `20261119042534_AddAssetMovement` dibuat dan diterapkan Rizki sesudah merge integration; DB baca-saja: 5 tabel Ast, 3 check constraint, 12 FK Restrict, index unik parsial perpanjangan terbuka, tanpa perubahan tabel lama. Kriteria 3 dengan pengecualian disetujui Rizki: migration ikut membuat `MstJobDescription`/`MstJobDescriptionItem` (model HR tanpa migration), Hamzah dikabari. Laporan: [`BE-AST-016.md`](../task/report/backend/BE-AST-016.md) |
| Acceptance criteria | 1. Lima tabel sesuai data dict, check constraint unit berbeda dan tanggal kembali ≥ mulai. 2. `RowVersion` pada mutasi dan pinjaman. 3. Migration hanya menambah lima tabel; `Down()` menghapusnya |

### ✅ `BE-AST-017` — Mutasi antar unit

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 19 November 2026.** `AssetTransferService` (+`.Query`, `CreateFromStockTakeAsync`), `AssetMovementDocumentService` (BA serah terima mutasi), `AssetTransfersController` (10 endpoint, resource `AssetTransfer` 6 aksi); register: sumber laporan kondisi `PenerimaanMutasi`, deret `AST-MUT`. QBE Strict 11 berkas `PASS`; atribut akses 10/10; nol baris komentar; build DLL 11:27:30. Runtime **65/65** dengan Kepala Unit dari penugasan HR (Siti = Instalasi Rawat Inap, Dimas = IGD, dibuat permanen atas izin Rizki): `AT-MOV-01`, `02`, `03`, `20..23`, `25..31` lulus (`21` lewat kunci pemanfaatan, `31` lewat versi basi; `24` opname menyusul `BE-AST-021`); validasi `001..005`, `007`, `008`, `010..016`; kondisi berbeda → `AST-KND-000012`; PDF diperiksa visual. `006` memakai gerbang opname yang terisi `BE-AST-021`. Delta (body keputusan, `rowVersion` wajib, linimasa hanya saat diterima) dicatat di laporan. Laporan: [`BE-AST-017.md`](../task/report/backend/BE-AST-017.md) |
| Acceptance criteria | 1. Matriks state §1 lengkap, termasuk tahap 1 otomatis bila pengaju Kepala Unit asal. 2. Penempatan berpindah hanya saat diterima Kepala Unit tujuan (`ApplyPlacement`). 3. Kondisi diterima berbeda membuka laporan kondisi. 4. Kunci proses dipasang/dilepas. 5. `AST-VAL-MOV-001..016`. 6. `CreateFromStockTake` tersedia untuk `inventarisasi`. 7. `AvailableActions` dihitung backend |

### `BE-AST-018` — Peminjaman dan pengembalian

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Matriks state §2–3. 2. `isOverdue` turunan pada respons. 3. Konfirmasi kembali dengan kondisi per butir; kondisi berubah → laporan kondisi `PengembalianPinjaman`. 4. Satu perpanjangan terbuka. 5. Aset tetap milik unit pemilik |

### `BE-AST-019` — Dashboard 1.4 dan laporan mutasi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Ringkasan mutasi berjalan per status, pinjaman aktif, pinjaman terlambat. 2. Laporan mutasi per periode dan ekspor. 3. `HasInFlightForUnit` untuk `inventarisasi` |
