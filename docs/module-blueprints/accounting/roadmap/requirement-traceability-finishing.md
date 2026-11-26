# Requirement Traceability dan Matriks Dependency — Accounting Finishing

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 20
roadmap_revision: 3                      # revisi 3 (26 Nov 2026: 008-04 rasio dirancang, FIN-5); revisi 2 (25 Nov 2026: + ACC-FIN-001)
status: APPROVED                         # GATE-DESAIN-1124 (24 Nov), GATE-DESAIN-1125 (25 Nov), GATE-DESAIN-1126 (26 Nov), Rizki
source_backend: 26229ec7                 # + BE-ACC-FIN-001-03 belum di-commit; revisi 2: 0f67c5c2; revisi 1: 1f353c45
source_frontend: 64ba2c07e              # + FE-ACC-FIN-001-03 belum di-commit; revisi 2: a2ddb52d2; revisi 1: 22894248
decision_revision: 21                    # ACC-DEC-141..184
contracts: [ACC-API-0.17, ACC-API-0.18, ACC-API-0.19, ACC-VALIDATION-0.12, ACC-VALIDATION-0.13, ACC-VALIDATION-0.14, ACC-PERMISSION-0.8, ACC-PERMISSION-0.9, ACC-PERMISSION-0.10, ACC-TEST bagian 4, 5, dan 6]
requirement_source: MASTER PRD ACC-FIN-PRD-001 (24 Nov 2026) - rujukan sementara; ClickUp belum dibaca (U-01)
```

## 1. Status kebutuhan PRD dalam slice ini

| Kebutuhan PRD | ClickUp | Bagian yang direncanakan | Bagian yang belum | Status |
|---|---|---|---|---|
| ACC-FIN-001 Penyempurnaan bagan akun | — (task analis COA, 25 Nov 2026) | Kode otomatis, induk dari daftar, jenis warisan, kode historis/impor, klasifikasi laporan, peringatan neraca (`FIN-2`..`FIN-4`) | Impor bagan akun resmi (skrip, belum diputuskan sebagai fitur); neraca terkelompok (`OQ-173-1`) | **Direncanakan** 25 November 2026 — belum dikerjakan |
| ACC-FIN-005 General Ledger Enhancement | `z8m132yv7x` (Transaksi) | Seluruh butir PRD ACC-FIN-005 **kecuali** "validasi konsistensi saldo" lintas laporan — dipenuhi lewat `F6` sebagai uji, bukan fitur | — | **Selesai (development)** 25 November 2026 — `BE-ACC-FIN-005-01` ✅ dan `FE-ACC-FIN-005-01` ✅; UAT milik tim UAT |
| ACC-FIN-008 Financial Reports | `z8m132yv84` (Laporan Keuangan) | Trial Balance (+CSV), Income Statement, Balance Sheet, Financial Ratio Analysis (revisi 3, `FIN-5`) | Revenue Target Analysis, Tax Report | **Sebagian** — tidak boleh ditandai selesai |

**Catatan untuk ClickUp.** Task ClickUp tidak diubah statusnya oleh agent (PRD bagian 11). Bila Rizki
memperbarui ClickUp secara manual, `z8m132yv84` tetap berstatus berjalan sampai ketiga laporan sisa
diputuskan.

## 2. Traceability requirement ke task

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | UAT | Bukti | Status |
|---|---|---|---|---|---|---|---|---|
| `FR-FIN-001` Saringan unit biaya buku besar | `147` | BE §25.5; FE §15.4 | `ACC-API-0.17` movements; `VAL-GL-01` | `BE-ACC-FIN-005-01` | `FE-ACC-FIN-005-01` | `F10`, `F12` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-005-01.md) uji API 12/12, uji ulang 25 Nov; [FE ✅](../task/report/frontend/FE-ACC-FIN-005-01.md) uji layar 14/14 |
| `FR-FIN-002` Ringkasan saldo | `141` | BE §25.5 | movements/summary | `BE-ACC-FIN-005-01` | `FE-ACC-FIN-005-01` | `F10`, `F15` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-005-01.md) uji API 12/12, uji ulang 25 Nov; [FE ✅](../task/report/frontend/FE-ACC-FIN-005-01.md) uji layar 14/14 |
| `FR-FIN-003` Asal jurnal dan tautan | `148` | BE §25.5; FE §15.4 | movements (11 field) | `BE-ACC-FIN-005-01` | `FE-ACC-FIN-005-01` | `F13` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-005-01.md) uji API 12/12, uji ulang 25 Nov; [FE ✅](../task/report/frontend/FE-ACC-FIN-005-01.md) uji layar 14/14 |
| `FR-FIN-004` CSV buku besar | `149`, `155` | BE §25.7 | movements/export; `VAL-GL-06` | `BE-ACC-FIN-005-01` | `FE-ACC-FIN-005-01` | `F15`, `F19`, `F20` | Berkas, Excel, log | [BE ✅](../task/report/backend/BE-ACC-FIN-005-01.md) uji API 12/12, uji ulang 25 Nov; [FE ✅](../task/report/frontend/FE-ACC-FIN-005-01.md) uji layar 14/14 |
| `FR-FIN-005` Masuk dari laporan | `148` butir 4 | FE §15.4 | — (frontend) | — | `FE-ACC-FIN-005-01` | `F14` | Tangkapan | [FE ✅](../task/report/frontend/FE-ACC-FIN-005-01.md) U6, U8 |
| `FR-FIN-030` CSV neraca saldo | `153`, `155` | BE §25.7; FE §15.5 | trial-balance/export | `BE-ACC-FIN-008-01` | `FE-ACC-FIN-008-01` | `F18`, `F20` | Berkas, log | [BE ✅](../task/report/backend/BE-ACC-FIN-008-01.md) uji API 8/8; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-01.md) uji layar 11/11; `F20` belum (UAT) |
| `FR-FIN-010` Kolom periode, YTD, pembanding | `144` | BE §25.6 | income-statement | `BE-ACC-FIN-008-02` | `FE-ACC-FIN-008-02` | `F6`, `F7` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-011` Pengecualian `JT` | `142` | BE §25.6 | income-statement | `BE-ACC-FIN-008-02` | `FE-ACC-FIN-008-02` | `F2`, `F3` | JSON | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-012` Tanda, hierarki, akun nol | `145`, `146` | BE §25.6 | income-statement, balance-sheet | `BE-ACC-FIN-008-02`, `008-03` | `FE-ACC-FIN-008-02`, `008-03` | `F8`, `F9` | Tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16; neraca [BE ✅](../task/report/backend/BE-ACC-FIN-008-03.md), [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) |
| `FR-FIN-013` Unit biaya laba rugi | `147` butir 2 | BE §25.6 | income-statement | `BE-ACC-FIN-008-02` | `FE-ACC-FIN-008-02` | `F11` | Tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-014` CSV dan Cetak laporan | `149` | BE §25.7; FE §15.6–15.7 | /export | `BE-ACC-FIN-008-02`, `008-03` | `FE-ACC-FIN-008-02`, `008-03` | `F20` | Berkas, tangkapan cetak | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16; neraca [BE ✅](../task/report/backend/BE-ACC-FIN-008-03.md), [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) |
| `FR-FIN-015` Log pembacaan dan unduhan | `155` | BE §25.7 | `ACC-PERMISSION-0.8` | `BE-ACC-FIN-005-01`, `008-01`..`03` | — | `F17` | Kutipan log | Sebagian — log unduhan buku besar ([BE-005-01](../task/report/backend/BE-ACC-FIN-005-01.md) A12) dan neraca saldo ([BE-008-01](../task/report/backend/BE-ACC-FIN-008-01.md) B8) terbukti tanpa nominal; laba rugi ([BE-008-02](../task/report/backend/BE-ACC-FIN-008-02.md) I15) dan neraca ([BE-008-03](../task/report/backend/BE-ACC-FIN-008-03.md) N12) terbukti tanpa nominal |
| `FR-FIN-020` Neraca dan pembanding | `144` | BE §25.6 | balance-sheet | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-03.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) layar 14/14 |
| `FR-FIN-021` Dua baris laba belum ditutup | `143` | BE §25.6 | balance-sheet | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4`, `F5` | JSON, tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-03.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) layar 14/14 |
| `FR-FIN-022` Status seimbang | `154` | BE §25.6 | balance-sheet; `VAL-GL-09` | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4` | Tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-03.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) layar 14/14 |
| `FR-FIN-040` Menu Laba Rugi dan Neraca | `152` | FE §15.2 | — | — | `FE-ACC-FIN-008-02`, `008-03` | Sidebar | Tangkapan | Laba Rugi [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) L1; Neraca [FE ✅](../task/report/frontend/FE-ACC-FIN-008-03.md) S1 |
| `FR-FIN-041` Hanya `Posted`; hak backend | `141`, `150` | BE §25.1 | `ACC-PERMISSION-0.8` | Semua BE | Semua FE | `F1`, `F16` | JSON `403` | Belum |
| `FR-FIN-050` Kode otomatis + pratinjau, aman simpan bersamaan | `163`, `164`, `172` | BE §26.6; FE §16.4 | `ACC-API-0.18` next-code, POST; `VAL-COA-06`, `13`..`15` | `BE-ACC-FIN-001-01` | `FE-ACC-FIN-001-01` | `F25`, `F26`, `F28`, `F29`, `F30` | JSON, skrip serentak, tangkapan | BE ✅ runtime ([laporan BE-ACC-FIN-001-01](../task/report/backend/BE-ACC-FIN-001-01.md)); FE ✅ layar 6/6 + balapan 1/1 ([laporan FE-ACC-FIN-001-01](../task/report/frontend/FE-ACC-FIN-001-01.md)) |
| `FR-FIN-051` Jenis anak diwarisi dan dikunci | `166` | BE §26.5; FE §16.4 | POST, PUT; `VAL-COA-03`..`05` | `BE-ACC-FIN-001-01` | `FE-ACC-FIN-001-01` | `F31`, `F32` | JSON, tangkapan | BE ✅ runtime ([laporan BE-ACC-FIN-001-01](../task/report/backend/BE-ACC-FIN-001-01.md)); FE ✅ layar 6/6 + balapan 1/1 ([laporan FE-ACC-FIN-001-01](../task/report/frontend/FE-ACC-FIN-001-01.md)) |
| `FR-FIN-052` Induk dari daftar, tingkat mengikuti induk, kode dapat diubah sebelum `Posted` | `162`, `164` butir 2 | BE §26.6 | PUT (lama) | `BE-ACC-FIN-001-01` (regresi) | `FE-ACC-FIN-001-01` (regresi) | `F33` | JSON, tangkapan | BE ✅ runtime ([laporan BE-ACC-FIN-001-01](../task/report/backend/BE-ACC-FIN-001-01.md)); FE ✅ layar 6/6 + balapan 1/1 ([laporan FE-ACC-FIN-001-01](../task/report/frontend/FE-ACC-FIN-001-01.md)) |
| `FR-FIN-053` Kode historis dan impor dipertahankan; anak induk di luar pola `.NN` | `170`, `172` | BE §26.6 | POST berkode; `VAL-COA-01`, `02` | `BE-ACC-FIN-001-01` | — | `F27` | JSON | BE ✅ runtime ([laporan BE-ACC-FIN-001-01](../task/report/backend/BE-ACC-FIN-001-01.md)); tanpa layar (jalur impor) |
| `FR-FIN-054` Klasifikasi dan sub-klasifikasi, diwariskan, dapat ditimpa | `167`, `173` | BE §26.8; FE §16.5–16.7; kamus data §21 | GET /, /{id}, /tree, POST, PUT; `VAL-COA-07`..`11` | `BE-ACC-FIN-001-02` | `FE-ACC-FIN-001-02` | `F34`..`F37` | JSON, tangkapan | BE runtime K2, K4–K8 ✅ ([laporan BE-ACC-FIN-001-02](../task/report/backend/BE-ACC-FIN-001-02.md)); FE ✅ layar 11/11 ([laporan FE-ACC-FIN-001-02](../task/report/frontend/FE-ACC-FIN-001-02.md)) |
| `FR-FIN-055` Berlaku semua periode, log lama → baru | `169` | BE §26.10 | `ACC-PERMISSION-0.9` | `BE-ACC-FIN-001-02` | — | `F39` | Kutipan log | BE ✅ runtime K11b (K11 awal FAIL, diperbaiki) ([laporan BE-ACC-FIN-001-02](../task/report/backend/BE-ACC-FIN-001-02.md)) |
| `FR-FIN-056` Akun belum lengkap tetap dipakai jurnal; saringan Belum lengkap | `168` | BE §26.8; FE §16.7 | GET / `UnclassifiedOnly`; `VAL-COA-12` | `BE-ACC-FIN-001-02` | `FE-ACC-FIN-001-02` | `F41`, `F38` (tautan) | JSON, tangkapan | BE runtime K1, K3, K10 ✅ ([laporan BE-ACC-FIN-001-02](../task/report/backend/BE-ACC-FIN-001-02.md)); FE ✅ layar 11/11 ([laporan FE-ACC-FIN-001-02](../task/report/frontend/FE-ACC-FIN-001-02.md)) |
| `FR-FIN-057` Peringatan klasifikasi di neraca | `168`, `173` | BE §26.9; FE §16.8 | GET /balance-sheet | `BE-ACC-FIN-001-03` | `FE-ACC-FIN-001-03` | `F38` | JSON, tangkapan | BE ✅ 26 Nov 2026: G0–G9 9/10 + G9b 1/1, AC-1..AC-6 runtime ([laporan](../task/report/backend/BE-ACC-FIN-001-03.md)); FE ✅ 26 Nov 2026: uji layar 9/9, ESLint 0/0 ([laporan](../task/report/frontend/FE-ACC-FIN-001-03.md)) |
| `FR-FIN-058` Hak `ChartOfAccount` ditegakkan backend | `169` | BE §26.3 | `ACC-PERMISSION-0.9` | `BE-ACC-FIN-001-01`, `001-02` | — | `F40` | JSON `403` | `next-code` `403` terbukti (`001-01` A13 ✅); `001-02` belum |
| `FR-FIN-059` Delapan rasio kini dan pembanding | `161`, `167`, `176`, `178`, `184` | BE §27.5–27.6; FE §17.4 | `ACC-API-0.19` financial-ratios | `BE-ACC-FIN-008-04` | `FE-ACC-FIN-008-04` | `F42`, `F46`, `F49` | JSON, tangkapan | BE ✅ 26 Nov 2026: H0–H12 13/13 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)); FE ✅ 26 Nov 2026 layar 8/8 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-060` ROA/ROE rata-rata, tanpa disetahunkan; saldo awal | `161` butir 3, `177` | BE §27.6–27.7 | `VAL-RAT-05` | `BE-ACC-FIN-008-04` | — | `F46`, `F47` | JSON | BE ✅ 26 Nov 2026 — jalur Dihitung ROA/ROE `SOURCE VERIFIED` ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)) |
| `FR-FIN-061` Klasifikasi belum lengkap menahan rasio terdampak | `168`, `175` | BE §27.7; FE §17.4 | `VAL-RAT-06`, `07` | `BE-ACC-FIN-008-04` | `FE-ACC-FIN-008-04` | `F44` | JSON, tangkapan | BE ✅ 26 Nov 2026 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)); FE ✅ 26 Nov 2026 layar 8/8 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-062` Status N/A, Tidak sah, catatan ekuitas negatif | `161` butir 5, 6, 8; `184` | BE §27.7 | `VAL-RAT-08`..`10` | `BE-ACC-FIN-008-04` | `FE-ACC-FIN-008-04` | `F43`, `F48` | JSON, simulasi | BE ✅ 26 Nov 2026 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)); FE ✅ 26 Nov 2026 layar 8/8 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-063` Format dua desimal; pembilang/penyebut cocok laporan | `179`, `181` | BE §27.5; FE §17.6 | `ACC-API-0.19` | `BE-ACC-FIN-008-04` | `FE-ACC-FIN-008-04` | `F45` | JSON, tangkapan | BE ✅ 26 Nov 2026 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)); FE ✅ 26 Nov 2026 layar 8/8 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-064` Layar dan menu sesudah Neraca | `180`, `168` | FE §17.2–17.4 | — | — | `FE-ACC-FIN-008-04` | `F49` | Tangkapan | FE ✅ 26 Nov 2026, menu sesudah Neraca terbukti V1 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-065` CSV, cetak, log tanpa nominal | `182` | BE §27.8–27.9; FE §17.4 | financial-ratios/export; `ACC-PERMISSION-0.10` | `BE-ACC-FIN-008-04` | `FE-ACC-FIN-008-04` | `F50`, `F51` | CSV, kutipan log | BE ✅ 26 Nov 2026 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)); FE ✅ 26 Nov 2026 layar 8/8 ([laporan](../task/report/frontend/FE-ACC-FIN-008-04.md)) |
| `FR-FIN-066` Hak `GeneralLedger : Read` | `183` | BE §27.3 | `ACC-PERMISSION-0.10` | `BE-ACC-FIN-008-04` | — | `F52` | JSON `403` | BE ✅ 26 Nov 2026, `403` terbukti H10 ([laporan](../task/report/backend/BE-ACC-FIN-008-04.md)) |
| ACC-FIN-008 Target pendapatan | `159` | Belum | Belum | ⛔ `BE-ACC-FIN-008-05` | ⛔ `FE-ACC-FIN-008-05` | — | — | ⛔ ACC-FIN-013 fondasi anggaran |
| ACC-FIN-008 Laporan pajak | `160` | Belum | Belum | ⛔ `BE-ACC-FIN-008-06` | ⛔ `FE-ACC-FIN-008-06` | — | — | ⛔ `OQ-160-1` |

Setiap `FR-FIN-*` punya sekurang-kurangnya satu task dan satu skenario uji. Requirement tanpa uji:
nihil.

## 3. Matriks dependency

### 3.1 Antar task

| Task | Prasyarat | Jenis | Alasan |
|---|---|---|---|
| `BE-ACC-FIN-008-01` | `BE-ACC-FIN-005-01` | Teknis | `AccLedgerCsv`, berkas `partial`, dan pola log unduhan |
| `BE-ACC-FIN-008-02` | `BE-ACC-FIN-005-01` | Teknis | Sama |
| `BE-ACC-FIN-008-03` | `BE-ACC-FIN-008-02` | Teknis | Penentu rentang periode, pengecualian, penyusun hierarki |
| `FE-ACC-FIN-005-01` | `BE-ACC-FIN-005-01` | Kontrak | Field dan endpoint baru |
| `FE-ACC-FIN-008-01` | `BE-ACC-FIN-008-01`, `FE-ACC-FIN-005-01` | Kontrak + teknis | Endpoint unduh; utilitas pengunduh bersama |
| `FE-ACC-FIN-008-02` | `BE-ACC-FIN-008-02`, `FE-ACC-FIN-005-01` | Kontrak + teknis | Endpoint laporan; pengunduh; Buku Besar menerima saringan dari tautan |
| `FE-ACC-FIN-008-03` | `BE-ACC-FIN-008-03`, `FE-ACC-FIN-008-02` | Kontrak + teknis | Endpoint neraca; komponen tabel laporan |
| `BE-ACC-FIN-001-02` | `BE-ACC-FIN-001-01` | Teknis | Berkas `partial` dan `CreateAsync` yang sama |
| `BE-ACC-FIN-001-03` | `BE-ACC-FIN-001-02`, `BE-ACC-FIN-008-03` | Teknis | Penghitung klasifikasi; neraca |
| `FE-ACC-FIN-001-01` | `BE-ACC-FIN-001-01` | Kontrak | `next-code`, `POST` dilonggarkan |
| `FE-ACC-FIN-001-02` | `BE-ACC-FIN-001-02`, `FE-ACC-FIN-001-01` | Kontrak + teknis | Field klasifikasi; hook editor yang sama |
| `FE-ACC-FIN-001-03` | `BE-ACC-FIN-001-03`, `FE-ACC-FIN-001-02`, `FE-ACC-FIN-008-03` | Kontrak + teknis | Field peringatan; tujuan tautan; layar neraca |
| `BE-ACC-FIN-008-04` | `BE-ACC-FIN-001-03` | Teknis | `MuatAkunLaporanAsync` membawa kolom klasifikasi; transitif `001-02` dan `008-03` (revisi 3: blocker desain dicabut `GATE-DESAIN-1126`) |
| ⛔ `BE-ACC-FIN-008-05` | ACC-FIN-013, `BE-ACC-FIN-008-02` | Desain + teknis | Anggaran Accounting; realisasi dari laba rugi |
| ⛔ `BE-ACC-FIN-008-06` | `OQ-160-1` | Keputusan | Akun pajak dan kontrak sumber |
| `FE-ACC-FIN-008-04` | `BE-ACC-FIN-008-04`, `FE-ACC-FIN-001-03` | Kontrak + teknis | Endpoint rasio; pola peringatan klasifikasi dan tautan Bagan Akun tersaring |
| ⛔ `FE-ACC-FIN-008-05` | `BE-ACC-FIN-008-05`, `FE-ACC-FIN-008-02` | Kontrak + teknis | — |
| ⛔ `FE-ACC-FIN-008-06` | `BE-ACC-FIN-008-06` | Kontrak | — |

### 3.2 Ke modul dan pihak lain

| Dependency | Pemilik | Dipakai untuk | Sifat | Keadaan |
|---|---|---|---|---|
| `MstCostCenter` | HR Master Data | Saringan dan label unit biaya | Baca saja | Ada |
| `MstLegalEntity` + penjaga `IsDefault` | HR Master Data / Accounting | Badan hukum laporan | Baca saja | Ada |
| Hak `GeneralLedger : Read` pada peran | Admin sistem | Akses kedelapan endpoint | Operasional (T-5) | Belum diberikan di lingkungan mana pun — bukan pekerjaan development |
| Bagan akun sah | Pemilik proses akuntansi | Laporan bermakna | Operasional (T-6) | Belum |
| Excel komputer rumah sakit | Tim UAT | Bukti bentuk CSV (`F20`) | Lingkungan | Belum diuji |
| Anggaran Accounting lawan `PrcBudget` (`OQ-159-1`) | Rizki + owner Procurement | Integrasi anggaran (sesudah `008-05`) | Ratifikasi | Terbuka; arah `ACC-DEC-159` |
| Akun pajak dan kontrak sumber (`OQ-160-1`) | PIC Pajak + owner Payroll, Finance, Billing, Medical Fee | `008-06` | Keputusan | Terbuka; arah `ACC-DEC-160` |
| Migration `AddChartOfAccountReportClassification` | Rizki | `BE-ACC-FIN-001-02` | Dikerjakan Rizki sendiri | Diizinkan 25 November 2026 (`OQ-167-1`) |
| Klasifikasi akun sesudah rilis | Pemilik proses akuntansi | Rasio, peringatan neraca bermakna | Operasional | Belum — usulan di BE §26.13 |
| Koreksi laba ditahan (`ACC-DEC-158`) | Rizki lewat layar | Ekuitas benar | Operasional | Prosedur di decision log |

Nol dependency ke Finance, Billing, Medical Fee, Farmasi, atau Aset untuk `FIN-0`..`FIN-5`.

## 4. Keputusan terbuka

| ID | Isi | Memblokir | Tidak memblokir |
|---|---|---|---|
| `U-01` | Verifikasi isi ClickUp `z8m132yv7x`, `z8m132yv84` | — | Seluruh `FIN-0`..`FIN-5`; bila berbeda, roadmap diamandemen |
| `OQ-173-1` | Neraca terkelompok Lancar/Tidak Lancar | — | Seluruhnya |
| `OQ-159-1` | Pemetaan anggaran dengan `PrcBudget` | Integrasi anggaran | Fondasi anggaran Accounting |
| `OQ-160-1` | Akun pajak dan kontrak sumber | `008-06` | Sisanya |

Tertutup 25 November 2026: `C-03` → `ACC-DEC-165`; `C-05` → `166`; `U-05` → `161`, `167`; `C-02` → `159` (arah);
`U-07` → `160` (arah); `OQ-167-1` → izin migration.

## 5. Coverage gap

| Gap | Alasan | Tindak lanjut |
|---|---|---|
| Target pendapatan dan pajak tanpa desain | ACC-FIN-013; `OQ-160-1` (rasio dirancang 26 November 2026, `GATE-DESAIN-1126`) | `design-business-module` per task (urutan `ACC-DEC-171`) |
| Jalur Dihitung Current/Quick/Cash/ROA/ROE di DB dev | DB dev tanpa liabilitas dan tanpa tahun buku 2025 | Data uji berizin Rizki saat `BE-ACC-FIN-008-04`; bila tidak, `SOURCE VERIFIED` (AC-8) |
| `F48` neraca tidak seimbang | Tidak dapat dibuat lewat aplikasi (`ACC-DEC-154`) | Source + simulasi layar |
| `VAL-COA-13` batas 999/99 | Hanya dibuktikan dari source | Dicatat di laporan `BE-ACC-FIN-001-01` |
| `F21`..`F24` laba ditahan belum di `acceptance-test-matrix.md` | Koreksi operasional, bukan task | Dipindahkan bila Rizki meminta |
| ACC-FIN-002 kunci grup COA | Belum dirancang | Definisi grup `ACC-DEC-165` |
| `F20` bergantung perangkat rumah sakit | Bentuk CSV `PROPOSED` | Tim UAT; bila Excel berbahasa Inggris, dua konstanta `AccLedgerCsv` diganti |
| Automated test | `ACC-DEC-081`; kesepakatan kerja Rizki | Tidak ada |
