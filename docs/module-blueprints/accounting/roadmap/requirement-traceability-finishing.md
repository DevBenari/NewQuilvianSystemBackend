# Requirement Traceability dan Matriks Dependency — Accounting Finishing

## Metadata

```yaml
blueprint_id: ACC-BP-001
blueprint_revision: 18
roadmap_revision: 1                      # backend-roadmap-finishing.md dan frontend-roadmap-finishing.md revisi 1
status: APPROVED                         # GATE-DESAIN-1124, Rizki, 24 Nov 2026
source_backend: 1f353c45
source_frontend: 22894248
decision_revision: 19                    # ACC-DEC-141..155
contracts: [ACC-API-0.17, ACC-VALIDATION-0.12, ACC-PERMISSION-0.8, ACC-TEST bagian 4]
requirement_source: MASTER PRD ACC-FIN-PRD-001 (24 Nov 2026) - rujukan sementara; ClickUp belum dibaca (U-01)
```

## 1. Status kebutuhan PRD dalam slice ini

| Kebutuhan PRD | ClickUp | Bagian yang direncanakan | Bagian yang belum | Status sesudah `FIN-0` + `FIN-1` selesai |
|---|---|---|---|---|
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
| `FR-FIN-012` Tanda, hierarki, akun nol | `145`, `146` | BE §25.6 | income-statement, balance-sheet | `BE-ACC-FIN-008-02`, `008-03` | `FE-ACC-FIN-008-02`, `008-03` | `F8`, `F9` | Tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-013` Unit biaya laba rugi | `147` butir 2 | BE §25.6 | income-statement | `BE-ACC-FIN-008-02` | `FE-ACC-FIN-008-02` | `F11` | Tangkapan | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-014` CSV dan Cetak laporan | `149` | BE §25.7; FE §15.6–15.7 | /export | `BE-ACC-FIN-008-02`, `008-03` | `FE-ACC-FIN-008-02`, `008-03` | `F20` | Berkas, tangkapan cetak | [BE ✅](../task/report/backend/BE-ACC-FIN-008-02.md) runtime; [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) layar 16/16 |
| `FR-FIN-015` Log pembacaan dan unduhan | `155` | BE §25.7 | `ACC-PERMISSION-0.8` | `BE-ACC-FIN-005-01`, `008-01`..`03` | — | `F17` | Kutipan log | Sebagian — log unduhan buku besar ([BE-005-01](../task/report/backend/BE-ACC-FIN-005-01.md) A12) dan neraca saldo ([BE-008-01](../task/report/backend/BE-ACC-FIN-008-01.md) B8) terbukti tanpa nominal; laba rugi ([BE-008-02](../task/report/backend/BE-ACC-FIN-008-02.md) I15) terbukti; neraca menunggu `008-03` |
| `FR-FIN-020` Neraca dan pembanding | `144` | BE §25.6 | balance-sheet | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4` | JSON, tangkapan | Belum |
| `FR-FIN-021` Dua baris laba belum ditutup | `143` | BE §25.6 | balance-sheet | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4`, `F5` | JSON, tangkapan | Belum |
| `FR-FIN-022` Status seimbang | `154` | BE §25.6 | balance-sheet; `VAL-GL-09` | `BE-ACC-FIN-008-03` | `FE-ACC-FIN-008-03` | `F4` | Tangkapan | Belum |
| `FR-FIN-040` Menu Laba Rugi dan Neraca | `152` | FE §15.2 | — | — | `FE-ACC-FIN-008-02`, `008-03` | Sidebar | Tangkapan | Sebagian — Laba Rugi [FE ✅](../task/report/frontend/FE-ACC-FIN-008-02.md) L1; Neraca menunggu `008-03` |
| `FR-FIN-041` Hanya `Posted`; hak backend | `141`, `150` | BE §25.1 | `ACC-PERMISSION-0.8` | Semua BE | Semua FE | `F1`, `F16` | JSON `403` | Belum |
| ACC-FIN-008 Rasio keuangan | — | Belum | Belum | ⛔ `BE-ACC-FIN-008-04` | ⛔ `FE-ACC-FIN-008-04` | — | — | ⛔ `U-05` |
| ACC-FIN-008 Target pendapatan | — | Belum | Belum | ⛔ `BE-ACC-FIN-008-05` | ⛔ `FE-ACC-FIN-008-05` | — | — | ⛔ `C-02` |
| ACC-FIN-008 Laporan pajak | — | Belum | Belum | ⛔ `BE-ACC-FIN-008-06` | ⛔ `FE-ACC-FIN-008-06` | — | — | ⛔ `U-07` |

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
| ⛔ `BE-ACC-FIN-008-04` | `U-05`, `BE-ACC-FIN-008-03` | Keputusan + teknis | Pembeda lancar; rasio membaca neraca |
| ⛔ `BE-ACC-FIN-008-05` | `C-02`, `BE-ACC-FIN-008-02` | Keputusan + teknis | Sumber anggaran; realisasi dari laba rugi |
| ⛔ `BE-ACC-FIN-008-06` | `U-07` | Keputusan | Sumber data pajak |
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
| Sumber anggaran (`C-02`) | Rizki + owner Pengadaan | `008-05` | Keputusan | Terbuka |
| Sumber data pajak (`U-07`) | Rizki + Yasmin + owner Payroll + PIC Pajak | `008-06` | Keputusan | Terbuka |

Nol dependency ke Finance, Billing, Medical Fee, Farmasi, atau Aset untuk `FIN-0` dan `FIN-1`.

## 4. Keputusan terbuka

| ID | Isi | Memblokir | Tidak memblokir |
|---|---|---|---|
| `C-05` | Jenis akun anak berbeda dari induk | — | Seluruh `FIN-0`, `FIN-1` (aturan cadangan `ACC-DEC-146` butir 5) |
| `U-01` | Verifikasi isi ClickUp `z8m132yv7x`, `z8m132yv84` | — | Seluruh `FIN-0`, `FIN-1`; bila berbeda, roadmap diamandemen |
| `U-05` | Pembeda aset lancar dan daftar rasio | `008-04` | Sisanya |
| `C-02` | Sumber anggaran resmi | `008-05` | Sisanya |
| `U-07` | Sumber data pajak resmi | `008-06` | Sisanya |

## 5. Coverage gap

| Gap | Alasan | Tindak lanjut |
|---|---|---|
| Rasio, target pendapatan, pajak tanpa desain | Blocker bagian 4 | Kembali ke `grill-me` sesudah blocker dicabut |
| `F20` bergantung perangkat rumah sakit | Bentuk CSV `PROPOSED` | Tim UAT; bila Excel berbahasa Inggris, dua konstanta `AccLedgerCsv` diganti |
| Automated test | `ACC-DEC-081`; kesepakatan kerja Rizki | Tidak ada |
