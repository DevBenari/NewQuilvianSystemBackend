# Laporan Perubahan Backend — `BE-ACC-FIN-008-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-008-01` |
| Judul | CSV neraca saldo |
| Slice | Gelombang pengiriman `FIN-0`, gelombang eksekusi 2 |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 1, kartu `BE-ACC-FIN-008-01` |
| Trace | ACC-FIN-008 bagian Trial Balance (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv84`); `FR-FIN-030`; `ACC-DEC-149`, `153`, `154`, `155`, `157` |
| Contract version | `ACC-API-0.17` + delta `ACC-DEC-157` (`VAL-GL-10`, `422`) — approved Rizki 25 November 2026; `ACC-PERMISSION-0.8` |
| Dependency | `BE-ACC-FIN-005-01` 🟡 (`AccLedgerCsv`, berkas `partial`, `BatasBarisUnduhan`) |
| Klasifikasi | `LIGHT` — satu method service, satu action, nol migration, nol hak baru |
| Task mode | `BACKEND` — instruksi tertulis Rizki 25 November 2026 bagian 3 |
| Target tulis | `GeneralLedger/Services/AccGeneralLedgerService.Export.cs`, `GeneralLedger/Controllers/GeneralLedgerController.cs`; delta kontrak; laporan ini; baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `bafbacaa` (branch `rizkiG`, sinkron dengan `origin/rizkiG`) |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-5 `RUNTIME VERIFIED` 25 November 2026 (uji API 8/8, uji layar bersama `FE-ACC-FIN-008-01` 11/11); build `PASS` (Rizki, DLL 09:01:06 WIB); QBE Strict `PASS`. Batas `ACC-DEC-157` `SOURCE VERIFIED`, runtime tidak dapat dicapai di data dev. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — registry baris 42, `ACTIVE` |
| Keberlakuan | `NEW CODE` (method dan action baru) di berkas milik task finishing |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Tidak berlaku | `QBE-DB-*`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` |

## 1. Masalah yang diperbaiki

Neraca Saldo hanya dapat dibaca di layar. Petugas yang perlu menyerahkannya ke auditor harus menyalin angka
satu per satu. Subtask ClickUp "Download Excel (CSV)" belum punya endpoint untuk laporan ini.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menyimpan neraca saldo satu periode sebagai berkas yang angkanya sama dengan layar |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Tombol Unduh CSV di layar Neraca Saldo (`FE-ACC-FIN-008-01`) |
| Langkah | 1) Sistem menyusun neraca saldo lewat `GetTrialBalanceAsync` yang sama dengan layar. 2) Bila gagal (periode tidak ada, penjaga badan hukum), galatnya diteruskan apa adanya. 3) Bila baris akun lebih dari 50.000 → `422`. 4) Sistem menyusun CSV: kepala laporan, satu baris per akun, baris Total, baris Status. 5) Unduhan dicatat tanpa nominal |
| Jalur tidak normal | Periode tidak ada → `404` JSON; tanpa hak → `403`; > 50.000 baris → `422` |
| Hasil | Berkas `neraca-saldo_{kode periode}.csv` |

**Contoh isi berkas** (periode September 2026, angka samaran):

```text
"Laporan";"Neraca Saldo"
"Badan Hukum";"PT Metropolitan Medical Centre"
"Periode";"September 2026 (2026-09)"
"Diunduh";"2026-11-25 09:10 WIB"

"Kode Akun";"Nama Akun";"Saldo Awal";"Debit";"Kredit";"Saldo Akhir"
"1002";"Kas Besar";0,00;1000000,00;0,00;1000000,00
"4001";"Pendapatan Rawat Jalan";0,00;0,00;2000000,00;-2000000,00
;"Total";;1000000,00;2000000,00;
;"Status";"Tidak seimbang, selisih -1000000,00";;;
```

Baris Status mengikuti `ACC-DEC-154`: neraca yang tidak seimbang tetap diunduh, dan selisihnya disebut.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/Services/AccGeneralLedgerService.Export.cs` | + `BuildTrialBalanceCsvAsync`: memakai `GetTrialBalanceAsync`, `AccLedgerCsv`, `NamaBadanHukumAsync`, `SegmenNamaBerkas`, dan `BatasBarisUnduhan` yang sudah ada — **nol aturan hitung baru** |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | + action `GET trial-balance/export` dengan `[AccessAction("Read", "Read General Ledger", …)]` dan `[AccessPermission("GeneralLedger", "Read")]`; log `GeneralLedger.Export` (`InfoAsync` berhasil, `WarningAsync` gagal) berisi jenis laporan `NeracaSaldo`, `LegalEntityId`, `PeriodCode`, status, jumlah baris |
| `contracts/validation-matrix.md` | + `VAL-GL-10` (delta `ACC-DEC-157`) |
| `contracts/api-contract.md` | + baris `422` neraca saldo (delta `ACC-DEC-157`) |

Nol baris komentar baru; nol controller, entity, tabel, kolom, migration, hak akses, atau dependency baru.

### 3.2 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai `ACC-API-0.17`; delta satu kode `422` (`ACC-DEC-157`) |
| Database | `NOT APPLICABLE` — hanya membaca lewat `GetTrialBalanceAsync` |
| Keamanan | Hak `GeneralLedger : Read` yang sama; log tanpa nominal; pengaman CSV injection dari `AccLedgerCsv.Teks` |
| Log pembacaan neraca saldo yang ada | Tidak berubah. Unduhan memanggil service, bukan action `GetTrialBalance`, sehingga log `GeneralLedger.TrialBalance` tidak tertulis dua kali |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / General Ledger

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/general-ledger/trial-balance/export?LegalEntityId=&PeriodCode=` | Berkas CSV neraca saldo satu periode | `GeneralLedger : Read` |

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (2 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru pada diff | 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false` | Dijalankan Rizki; `QuilvianSystemBackend.dll` 25 November 2026 09:01:06 WIB, sesudah perubahan source | `PASS` |
| `01-uji-api.mjs` B1–B8, putaran 1 (09.08 WIB) | 7/8 — B8 gagal karena **ambang skrip salah hitung** (mengharapkan ≥ 4 log, padahal permintaan `403` B6 tidak pernah sampai ke action). Kode tidak diubah | Skrip dikoreksi |
| `01-uji-api.mjs` putaran 2 (09.09–09.10 WIB, putaran resmi), B8 diperketat menjadi tepat 3 log berurutan INF/WRN/INF + 1 `AccessDenied` | 8/8 | `PASS` |
| Uji layar bersama `FE-ACC-FIN-008-01` (`02-uji-layar.mjs`, 09.16 WIB) | 11/11; CSV dari layar identik dengan CSV dari API kecuali baris "Diunduh" | `PASS` |

Uji dijalankan agent terhadap backend yang dinyalakan Rizki. Login memakai SuperAdmin dan pengguna nyata tanpa hak (`QUILVIAN_ASET_APPROVER`). Semua panggilan berupa `GET`; nol SQL. Bukti: `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-01-20261125/` (`01-uji-api.json`, `01-uji-api-run1-ambang-salah.json`, `01-neraca-saldo-2026-09.csv`) dan log `Logs/quilvian-backend-20261125.json`.

| Uji | Hasil mentah |
| --- | --- |
| B1 | `200 text/csv; charset=utf-8`, BOM ada; 5 baris akun JSON = 5 baris CSV, sama persis dan berurutan sama; baris Total `;"Total";;2900000,00;2900000,00;` |
| B2 | `IsBalanced = true` → `;"Status";"Seimbang";;;` |
| B3 | `attachment; filename=neraca-saldo_2026-09.csv` |
| B4 | Periode `2099-01` → `404 application/json`, "Periode akuntansi tidak ditemukan." |
| B5 | Periode `2019-01` tanpa saldo → `200`, Total `0,00;0,00`, Status Seimbang |
| B6 | Pengguna tanpa hak → `403`; log `SECURITY_ACCESSDENIED` dari filter platform |
| B7 | Regresi `BE-ACC-FIN-005-01` — lihat laporan task itu bagian 9 |
| B8 | Log `GeneralLedger.Export` tepat 3: INF "Neraca saldo September 2026 berhasil diunduh.", WRN "Periode akuntansi tidak ditemukan.", INF "Neraca saldo Januari 2019 berhasil diunduh."; nol nominal; log `GeneralLedger.TrialBalance` hanya 1 (dari `GET /trial-balance` eksplisit) |

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Baris akun sama dan berurutan sama dengan `GET /trial-balance` | `RUNTIME VERIFIED` | B1; layar V5 |
| AC-2 Total debit dan kredit sama dengan JSON | `RUNTIME VERIFIED` | B1 |
| AC-3 Bentuk berkas mengikuti bagian 25.7 | `RUNTIME VERIFIED` — BOM, `;`, desimal koma, kepala laporan, baris Total dan Status | B1, B2, B3, B5 |
| AC-4 Satu log `GeneralLedger.Export` tanpa nominal; log `TrialBalance` tidak ganda | `RUNTIME VERIFIED` | B8 |
| AC-5 Periode tidak ada → `404` JSON | `RUNTIME VERIFIED` | B4 |
| Tambahan `ACC-DEC-157` > 50.000 baris → `422` | `SOURCE VERIFIED`; runtime tidak dapat dicapai di data dev. Penanganan pesan `422` di layar dibuktikan dengan simulasi (`FE-ACC-FIN-008-01` V10) | `BuildTrialBalanceCsvAsync` |
| Skenario negatif tanpa hak → `403` | `RUNTIME VERIFIED` | B6; layar V8 |

**DoD.** AC-1..AC-5 terbukti; laporan ditulis; roadmap dan traceability diperbarui → ✅. `F20` (membuka CSV di Excel komputer rumah sakit) tetap skenario UAT dan bukan butir DoD kartu ini.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan | Pemilih Tanggal Awal/Tanggal Akhir di layar Neraca Saldo hanya tampilan: nilainya diisi dari periode dan tidak memengaruhi data (`trial-balance-view.jsx`). Ekspor memakai `PeriodCode` yang sama dengan data layar, jadi tetap konsisten. Tidak diubah — di luar lingkup |
| Risiko | Rendah. Jumlah baris = jumlah akun yang bergerak |
| Status Git | `M` `GeneralLedgerController.cs`, `AccGeneralLedgerService.Export.cs`, dokumen kontrak dan roadmap |
| Langkah berikutnya | `BE-ACC-FIN-008-02` (laba rugi periode dan YTD) |
