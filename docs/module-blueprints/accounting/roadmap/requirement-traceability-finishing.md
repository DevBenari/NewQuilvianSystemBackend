# Requirement Traceability dan Matriks Dependency — Accounting Finishing

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 19
roadmap_revision: 2                      # backend-roadmap-finishing.md dan frontend-roadmap-finishing.md revisi 2 (25 Nov 2026: + ACC-FIN-001)
status: APPROVED                         # GATE-DESAIN-1124 (24 Nov) dan GATE-DESAIN-1125 (25 Nov), Rizki
source_backend: 0f67c5c2                 # revisi 1: 1f353c45
source_frontend: a2ddb52d2               # revisi 1: 22894248
decision_revision: 20                    # ACC-DEC-141..173
contracts: [ACC-API-0.17, ACC-API-0.18, ACC-VALIDATION-0.12, ACC-VALIDATION-0.13, ACC-PERMISSION-0.8, ACC-PERMISSION-0.9, ACC-TEST bagian 4 dan 5]
requirement_source: MASTER PRD ACC-FIN-PRD-001 (24 Nov 2026) - rujukan sementara; ClickUp belum dibaca (U-01)
```

## 1. Status kebutuhan PRD dalam slice ini

| Kebutuhan PRD | ClickUp | Bagian yang direncanakan | Bagian yang belum | Status |
|---|---|---|---|---|
| ACC-FIN-001 Penyempurnaan bagan akun | — (task analis COA, 25 Nov 2026) | Kode otomatis, induk dari daftar, jenis warisan, kode historis/impor, klasifikasi laporan, peringatan neraca (`FIN-2`..`FIN-4`) | Impor bagan akun resmi (skrip, belum diputuskan sebagai fitur); neraca terkelompok (`OQ-173-1`) | **Direncanakan** 25 November 2026 — belum dikerjakan |
| ACC-FIN-005 General Ledger Enhancement | `z8m132yv7x` (Transaksi) | Seluruh butir PRD ACC-FIN-005 **kecuali** "validasi konsistensi saldo" lintas laporan — dipenuhi lewat `F6` sebagai uji, bukan fitur | — | **Selesai (development)** 25 November 2026 — `BE-ACC-FIN-005-01` ✅ dan `FE-ACC-FIN-005-01` ✅; UAT milik tim UAT |
| ACC-FIN-008 Financial Reports | `z8m132yv84` (Laporan Keuangan) | Trial Balance (+CSV), Income Statement, Balance Sheet | Financial Ratio Analysis, Revenue Target Analysis, Tax Report | **Sebagian** — tidak boleh ditandai selesai |

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
| `FR-FIN-057` Peringatan klasifikasi di neraca | `168`, `173` | BE §26.9; FE §16.8 | GET /balance-sheet | `BE-ACC-FIN-001-03` | `FE-ACC-FIN-001-03` | `F38` | JSON, tangkapan | Belum |
| `FR-FIN-058` Hak `ChartOfAccount` ditegakkan backend | `169` | BE §26.3 | `ACC-PERMISSION-0.9` | `BE-ACC-FIN-001-01`, `001-02` | — | `F40` | JSON `403` | `next-code` `403` terbukti (`001-01` A13 ✅); `001-02` belum |
| ACC-FIN-008 Rasio keuangan | `161` | Belum | Belum | ⛔ `BE-ACC-FIN-008-04` | ⛔ `FE-ACC-FIN-008-04` | — | — | ⛔ Desain 008-04 (arah `ACC-DEC-161`); butuh `001-02` |
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
| ⛔ `BE-ACC-FIN-008-04` | Desain 008-04, `BE-ACC-FIN-008-03`, `BE-ACC-FIN-001-02` | Desain + teknis | Rasio membaca neraca dan klasifikasi |
| ⛔ `BE-ACC-FIN-008-05` | ACC-FIN-013, `BE-ACC-FIN-008-02` | Desain + teknis | Anggaran Accounting; realisasi dari laba rugi |
| ⛔ `BE-ACC-FIN-008-06` | `OQ-160-1` | Keputusan | Akun pajak dan kontrak sumber |
| ⛔ `FE-ACC-FIN-008-04` | `BE-ACC-FIN-008-04`, `FE-ACC-FIN-008-03` | Kontrak + teknis | — |
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

Nol dependency ke Finance, Billing, Medical Fee, Farmasi, atau Aset untuk `FIN-0`..`FIN-4`.

## 4. Keputusan terbuka

| ID | Isi | Memblokir | Tidak memblokir |
|---|---|---|---|
| `U-01` | Verifikasi isi ClickUp `z8m132yv7x`, `z8m132yv84` | — | Seluruh `FIN-0`..`FIN-4`; bila berbeda, roadmap diamandemen |
| `OQ-173-1` | Neraca terkelompok Lancar/Tidak Lancar | — | Seluruhnya |
| `OQ-159-1` | Pemetaan anggaran dengan `PrcBudget` | Integrasi anggaran | Fondasi anggaran Accounting |
| `OQ-160-1` | Akun pajak dan kontrak sumber | `008-06` | Sisanya |

Tertutup 25 November 2026: `C-03` → `ACC-DEC-165`; `C-05` → `166`; `U-05` → `161`, `167`; `C-02` → `159` (arah);
`U-07` → `160` (arah); `OQ-167-1` → izin migration.

## 5. Coverage gap

| Gap | Alasan | Tindak lanjut |
|---|---|---|
| Rasio, target pendapatan, pajak tanpa desain | Desain 008-04; ACC-FIN-013; `OQ-160-1` | `design-business-module` per task (urutan `ACC-DEC-171`) |
| `VAL-COA-13` batas 999/99 | Hanya dibuktikan dari source | Dicatat di laporan `BE-ACC-FIN-001-01` |
| `F21`..`F24` laba ditahan belum di `acceptance-test-matrix.md` | Koreksi operasional, bukan task | Dipindahkan bila Rizki meminta |
| ACC-FIN-002 kunci grup COA | Belum dirancang | Definisi grup `ACC-DEC-165` |
| `F20` bergantung perangkat rumah sakit | Bentuk CSV `PROPOSED` | Tim UAT; bila Excel berbahasa Inggris, dua konstanta `AccLedgerCsv` diganti |
| Automated test | `ACC-DEC-081`; kesepakatan kerja Rizki | Tidak ada |
