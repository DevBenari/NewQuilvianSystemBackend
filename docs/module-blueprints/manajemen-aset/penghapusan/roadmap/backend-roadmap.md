# Penghapusan & Pelepasan Aset — Roadmap Backend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` |
| Blueprint / contract | `AST-BP-001` rev 1, `penghapusan`, `AST-HPS-1.0.0` `approved` 2026-11-12 |
| Backend SHA | `1f54a175` (`rizkiG`) |
| Task | `BE-AST-033` s.d. `BE-AST-037` (gelombang `MVP-5`) |
| Handoff | `build-module-backend`, satu task per pemanggilan, `TASK MODE: BACKEND` eksplisit; QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend |

**Legenda tanda status:** ✅ selesai · 🟡 sebagian · ⛔ terblokir · tanpa tanda = belum dikerjakan.

`V-BE` dan `DoD-BE` sama dengan `../../register/roadmap/backend-roadmap.md`; skenario `AT-HPS-*`.
Kejadian `AST-PENGHAPUSAN`/`AST-PEMULIHAN` ditulis ke kotak keluar `nilai` dan baru terkirim setelah
`BE-AST-032` [NLI] menyalakan kodenya; pembangunan task di sini tidak tertahan.

## Grafik Urutan Dependency

```text
BE-AST-025 [NLI] ─> BE-AST-033 ─┬─> BE-AST-034 ─┬─> BE-AST-035 ─┬─> BE-AST-036
                                │               │               │
BE-AST-010 [REG] ───────────────┘               │               └─> BE-AST-037
                                                │
                            BE-AST-027 [NLI] ───┘
```

`[NLI]` = task backend sub-modul `nilai` · `[REG]` = task backend sub-modul `register`. Cermin baca-saja.

Pasangan prasyarat→task: **7** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-AST-025` [NLI] | `BE-AST-033` |
| 2 | `BE-AST-033`, `BE-AST-010` [REG] | `BE-AST-034` |
| 3 | `BE-AST-034`, `BE-AST-027` [NLI] | `BE-AST-035` |
| 4 | `BE-AST-035` | `BE-AST-036`, `BE-AST-037` — boleh paralel |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-AST-033` | Empat tabel penghapusan + migration | `CAP-10.*` | data dict | `IdentityModel` | `Disposal/{Models,Enums}`, configuration, DbSet, `AddAssetDisposal` | `BE-AST-025` | Kartu | `V-BE` | Urutan migration 5 sesudah 4 | `DoD-BE` |
| `BE-AST-034` | Usulan sampai disetujui | `CAP-10.1..10.3`, `AST-DEC-020`, `045`, `049`; `AST-DES-019`, `038..040` | API 2.1, state §1 | `AssetProcessLockService`, `AssetConditionService` [REG]; `AssetBookService`, parameter [NLI] | `AssetDisposalService` (usul, tim, kelayakan, setuju), controller | `BE-AST-033`, `BE-AST-010` | Kartu | `V-BE`, `AT-HPS-01..03`, `30..34` | Batas Direktur `AST-OQ-003` (data) | `DoD-BE` |
| `BE-AST-035` | Pemindahtanganan, finalisasi, berita acara | `CAP-10.4`, `10.5`, `AST-DEC-022`, `046`; `AST-DES-013`, `041` | API 2.1, state §1 | `AssetRecordService` [REG]; `IsPeriodProcessed`, kotak keluar [NLI] | `AssetDisposalService` (lepas, final, batal), `DisposalDocumentService` | `BE-AST-034`, `BE-AST-027` | Kartu | `V-BE`, `AT-HPS-04..07`, `10`, `35..39`, `42` | — | `DoD-BE` |
| `BE-AST-036` | Pemulihan Aset | `CAP-EXT-03`, `AST-DEC-028`; `AST-DES-037` | API 2.2, state §2 | `AssetRecordService` [REG], `AssetBookService` [NLI] | `AssetRestorationService`, controller | `BE-AST-035` | Kartu | `V-BE`, `AT-HPS-08`, `40` | — | `DoD-BE` |
| `BE-AST-037` | Riwayat aset dihapus dan laporan | `CAP-10.6`, `11.5` | API 2.3–2.4 | — | `DisposalQueryService`, controller | `BE-AST-035` | Kartu | `V-BE`, `AT-HPS-09`, `43` | — | `DoD-BE` |

## Kartu task

### `BE-AST-033` — Tabel penghapusan dan migration `AddAssetDisposal`

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Empat tabel sesuai data dict. 2. Unik `(ProposalId, AssetId)`, satu pemulihan terbuka per butir. 3. Migration hanya menambah empat tabel |

### `BE-AST-034` — Usulan, kelayakan, persetujuan berjenjang

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Syarat alasan sesuai kondisi. 2. Kunci `Penghapusan` dipasang; butir Tidak Layak dilepas; semua Tidak Layak → `Ditolak`. 3. Total nilai buku dan batas dibekukan saat masuk persetujuan. 4. Total > batas → Direktur. 5. Penyetuju bukan pengusul/pemeriksa |

### `BE-AST-035` — Pemindahtanganan dan finalisasi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Aturan nilai per metode. 2. Alasan Hilang melewati pemindahtanganan. 3. Final ditolak bila penyusutan bulan lalu belum diproses. 4. Final satu transaksi: aset `Dihapus`, buku `Dihentikan`, entri, satu kejadian per butir, linimasa. 5. Laba/rugi = pelepasan − nilai buku. 6. Batal hanya sebelum pemindahtanganan |

### `BE-AST-036` — Pemulihan Aset

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Hanya butir `Final`; bukti pembatalan untuk aset terjual/dihibahkan/ditukar. 2. Disetujui Kepala Bagian Aset bukan pengaju. 3. Aset `Terdaftar`/`Aktif`/`BelumDitempatkan`; buku aktif; kejadian `AST-PEMULIHAN`; penyesuaian penyusutan dikejar proses berikutnya |

### `BE-AST-037` — Riwayat aset dihapus dan laporan penghapusan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Acceptance criteria | 1. Riwayat tanpa rupiah dengan batas unit. 2. Laporan dengan rupiah hanya `AssetDisposal : ViewAmount`. 3. Ekspor |
