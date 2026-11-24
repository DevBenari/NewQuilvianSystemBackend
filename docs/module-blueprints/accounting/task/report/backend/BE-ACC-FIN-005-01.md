# Laporan Perubahan Backend — `BE-ACC-FIN-005-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-005-01` |
| Judul | Buku besar diperluas dan CSV |
| Slice | Gelombang pengiriman `FIN-0`, gelombang eksekusi 1 |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 1, kartu `BE-ACC-FIN-005-01` |
| Trace | ACC-FIN-005 (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv7x`); `FR-FIN-001`..`005`; `ACC-DEC-141`, `147`, `148`, `149`, `155`; `VAL-GL-01`, `02`, `03`, `06`; `02-backend-architecture.md` bagian 25.3–25.5, 25.7 |
| Contract version | `ACC-API-0.17`, `ACC-VALIDATION-0.12`, `ACC-PERMISSION-0.8` — `approved` Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — satu service dipecah per berkas, tiga endpoint (satu diperluas), nol migration, nol hak baru |
| Task mode | `BACKEND` — perintah Rizki 24 November 2026 "commit semua … lalu lanjutkan langkah pertama" |
| Target tulis | `Areas/Corporate/AccountingManagement/GeneralLedger/**`; laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `74255d59` (branch `rizkiG`) |
| Tanggal | 24 November 2026 |
| Status | 🟡 **SEBAGIAN** — source lengkap untuk AC-1..AC-8, QBE Strict `PASS`, **build `PASS`** (Rizki, 24 November 2026; DLL 15:19:56 WIB). **Uji runtime oleh agent 24 November 2026 15.35–15.50 WIB: AC-1..AC-7 terbukti**; AC-8 (`> 50.000` baris → `422`) terpetakan ke source tetapi `NOT FEASIBLE` di data dev (2 baris per akun) — menunggu keputusan pengecualian pemilik |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AccountingManagement` / Accounting |
| Submodule | `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 42, `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk `AccGeneralLedgerService.cs`, `GeneralLedgerController.cs`, `GeneralLedgerDtos.cs`; `NEW CODE` untuk empat berkas baru |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-PAGE-001`, `QBE-AUD-001`, `QBE-LOG-001` |
| Tidak berlaku | `QBE-DB-*`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` — tanpa entity, konfigurasi EF, migration, maupun nomor bisnis |
| Branch | `rizkiG`, upstream `origin/rizkiG` (ahead 4, belum di-push) |
| Sisa `agents/rules/` atau `.codex/` | Tidak ditemukan |

## 1. Masalah yang diperbaiki

Buku besar sebelum task ini hanya dapat disaring per akun dan rentang tanggal. Akibatnya bagi petugas:

- **Tidak dapat melihat bagian satu unit.** Akun Beban Obat bernilai Rp 13.000.000 pada November,
  tetapi petugas tidak dapat melihat berapa yang milik Radiologi tanpa membuka jurnal satu per satu.
- **Saldo awal dan akhir tidak dikirim.** Saldo awal sudah dihitung di server, tetapi tidak pernah
  sampai ke layar.
- **Asal baris tidak terlihat.** Petugas tidak tahu apakah sebuah baris berasal dari jurnal manual,
  kiriman Finance, jurnal berulang, tutup tahun, atau koreksi, dan tidak ada pengenal jurnal untuk
  membuka rinciannya.
- **Tidak dapat diunduh.** Subtask ClickUp "Download Excel (CSV)" belum punya endpoint.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Petugas menelusuri mutasi satu akun — seluruhnya atau bagian satu unit biaya — sampai jurnal dan asalnya, lalu menyimpannya sebagai berkas |
| Pelaku | Pemegang hak `GeneralLedger : Read` |
| Pemicu | Membuka Buku Besar, memilih akun, rentang tanggal, dan bila perlu unit biaya |
| Prasyarat | Akun dan jurnal `Posted` ada; unit biaya, bila dipilih, milik badan hukum laporan |
| Langkah | 1) Sistem memeriksa penjaga badan hukum, rentang tanggal, akun, dan unit biaya. 2) Sistem mengambil baris jurnal `Posted` akun itu, disaring unit biaya bila ada. 3) Sistem menghitung saldo pembuka sebelum rentang. 4) Daftar: sistem mengurutkan menurut tanggal, nomor jurnal, nomor baris, lalu menghitung saldo berjalan per halaman. 5) Sistem menentukan asal setiap baris. 6) Ringkasan: sistem menjumlahkan seluruh rentang. 7) Unduhan: sistem menyusun CSV seluruh baris dan mencatat unduhan |
| Aturan | Hanya jurnal `Posted`. Asal ditentukan berurutan: Koreksi → Tutup Tahun → Kejadian → Jurnal Berulang → Manual. Unduhan paling banyak 50.000 baris |
| Status | Tidak ada perubahan status; seluruh endpoint membaca |
| Jalur tidak normal | Unit biaya badan hukum lain → `400`; rentang terbalik → `400`; akun tidak ada → `404`; tanpa hak → `403`; unduhan > 50.000 baris → `422` |
| Hasil akhir | Daftar mutasi bertanda asal, ringkasan saldo, atau berkas CSV; satu baris log per unduhan |

**Contoh saringan unit biaya.** Beban Obat (5-1001) November 2026: Radiologi Rp 4.000.000 dan
Farmasi Rp 9.000.000, tanpa saldo sebelum November. Tanpa saringan, saldo akhir Rp 13.000.000. Dengan
saringan Radiologi, saldo awal Rp 0, debit Rp 4.000.000, saldo akhir Rp 4.000.000.

**Contoh penentuan asal.** Jurnal `JB/2026/11/00002` membalik `JU/2026/11/00031` yang berasal dari
kejadian Finance. Barisnya berlabel `Koreksi · JU/2026/11/00031`, bukan Kejadian, karena koreksi
diperiksa lebih dulu.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AccGeneralLedgerService.cs`, `GeneralLedgerController.cs`, `GeneralLedgerDtos.cs`,
`AccJournal.cs`, `AccJournalLine.cs`, `AccAccountingEvent.cs`, `AccRecurringJournalRun.cs`,
`AccRecurringJournalTemplate.cs`, `AccJournalType.cs`, `MstCostCenter.cs`, `MstLegalEntity.cs`,
`AccountingServiceResult.cs`, `AccountingLegalEntityGuard.cs`, `LoggerService.cs`, `PagedResult.cs`,
`CashierShiftRevenueReportController.cs` (preseden `File(...)`), `AccAccountingEventService.cs`
(preseden zona WIB), `Program.cs` (baca saja: registrasi service, konfigurasi JSON, CORS).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/Services/AccGeneralLedgerService.cs` | `class` → `partial class`; `GetMovementsAsync` beserta komentar lamanya dipindah apa adanya |
| `GeneralLedger/Services/AccGeneralLedgerService.Movement.cs` | **Baru.** `GetMovementsAsync` (diperluas), `GetMovementSummaryAsync`, `SiapkanMutasiAsync` (validasi dan query dasar bersama), `UrutkanMutasi`, `PetakanMutasiAsync` (asal, unit biaya, saldo berjalan) |
| `GeneralLedger/Services/AccGeneralLedgerService.Export.cs` | **Baru.** `BuildMovementCsvAsync` dan pembantunya |
| `GeneralLedger/Services/AccLedgerCsv.cs` | **Baru**, statis. Penyusun CSV: BOM UTF-8, pemisah `;`, desimal koma, pengaman awalan rumus |
| `GeneralLedger/Enums/LedgerSourceKind.cs` | **Baru.** `Manual = 1`, `AccountingEvent = 2`, `RecurringJournal = 3`, `YearEnd = 4`, `Correction = 5` |
| `GeneralLedger/DTOs/GeneralLedgerDtos.cs` | `LedgerMovementQuery` + `CostCenterId`; `LedgerMovementResponse` + 11 field; baru `LedgerMovementSummaryResponse`, `LedgerExportFile` |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | + `GET movements/summary`, `GET movements/export` |

Nol baris komentar baru; komentar lama yang ikut pindah tidak disunting.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /movements`: satu parameter opsional dan sebelas field tambahan, field lama tidak berubah. Dua endpoint baru. Sesuai `ACC-API-0.17`; **nol delta kontrak** |
| Database | `NOT APPLICABLE` — nol tabel, kolom, konfigurasi EF, migration. Seluruh query `AsNoTracking` |
| Keamanan/Auth | Kedua action baru memakai `[AccessAction("Read", "Read General Ledger", …)]` dan `[AccessPermission("GeneralLedger", "Read")]`, sama dengan tiga action yang ada; nol hak baru. Unit biaya divalidasi milik badan hukum laporan. Log unduhan tanpa nominal |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / General Ledger

Base URL `api/v1/corporate/accounting/general-ledger`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/movements` *(diperluas)* | Mutasi satu akun, kini dapat disaring `CostCenterId` dan membawa asal jurnal | `GeneralLedger : Read` |
| `GET` | `/movements/summary` | Saldo awal, total debit, total kredit, saldo akhir, jumlah baris seluruh rentang | `GeneralLedger : Read` |
| `GET` | `/movements/export` | Berkas CSV seluruh mutasi sesuai saringan | `GeneralLedger : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <7 berkas> -Mode Strict` | `Files evaluated: 7`; `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`; `Findings: none`; `Final result: PASS` | `PASS` | Keluaran perintah, 24 November 2026 |
| Pemeriksaan baris komentar baru pada diff | Nol | `PASS` | `git diff -U0` dan pemindaian berkas baru |
| `dotnet build` oleh Rizki | Berhasil, dinyatakan Rizki 24 November 2026 | `PASS` | `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 2026-11-24 15:19:56 WIB, sesudah suntingan source terakhir 15:12:30; DLL memuat `GetMovementSummaryAsync`, `BuildMovementCsvAsync`, `AccLedgerCsv`, `LedgerSourceKind` |
| Uji API `02-uji-api.mjs` (A1–A12) terhadap backend Rizki (DLL 15:19:56) | 12/12 lulus | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/accounting/fe-acc-fin-005-01-20261124/``02-uji-api.json` (response body, cap waktu 08:35:36–08:35:38 UTC), `02-unduhan-buku-besar.csv` |
| Uji layar `03-uji-layar.mjs` (U1–U10), `04-uji-layar-tambahan.mjs` (T1–T3), `05-uji-memuat.mjs` (T4) | 14/14 lulus pada putaran akhir | `PASS` | JSON + PNG `01`..`14` di folder yang sama; `03-jaringan-api.json` (5 respons non-2xx, seluruhnya milik pengguna tanpa hak di U10) |
| `F19` unduhan > 50.000 baris | Tidak dapat dijalankan: akun terbanyak di DB dev hanya 2 baris; membuat 50.000 jurnal disahkan tidak sepadan dan menuntut penulisan data | `NOT FEASIBLE` | — |

Uji manual: `REQUIRED`.

**Tidak dijalankan:** build dan uji runtime (lihat di atas); automated test tidak ditulis (`ACC-DEC-081`,
kesepakatan kerja Rizki).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 Tanpa `CostCenterId`, isi, urutan, dan `RunningBalance` sama dengan sebelumnya | **Terpenuhi** — A4: pageSize 1 halaman 1–2 bersambung (−1.000.000 → −2.000.000) dan sama dengan pageSize 25; U2 dua baris | Penyaring, urutan `AccountingDate` → `JournalNumber` → `LineNumber`, dan perhitungan saldo dipertahankan di `SiapkanMutasiAsync` / `UrutkanMutasi` / `PetakanMutasiAsync`; deskripsi tetap `baris ?? jurnal` |
| AC-2 Dengan `CostCenterId`, hanya baris unit itu dan saldo berjalan dari saldo pembuka unit | **Terpenuhi** — A1/A2: akun 4001, unit `CC-MMC-00001` → 1 baris `JB/2026/09/00001`, saldo −1.000.000 | Penyaring unit diterapkan pada `dasar` sebelum saldo pembuka (`SiapkanMutasiAsync`) |
| AC-3 Asal jurnal dan tepat satu rujukan asal | **Terpenuhi untuk Manual dan Kejadian** — A5: 4001 `Manual · 2030002348` tanpa rujukan; 1-3002 `Finance · AR-UJI-0113` dengan `AccountingEventId`. Cabang Jurnal Berulang, Tutup Tahun, Koreksi tidak punya data `Posted` di DB dev — terpetakan ke source | `PetakanMutasiAsync`, urutan lima cabang |
| AC-4 `ClosingBalance` ringkasan = saldo berjalan baris terakhir | **Terpenuhi** — A3, tanpa dan dengan unit biaya | `GetMovementSummaryAsync` memakai `DalamRentang` dan `SaldoPembuka` yang sama dengan daftar |
| AC-5 CSV sesuai bagian 25.7 dan nama berkasnya | **Terpenuhi** — A6/A7: `text/csv`, BOM `EF BB BF`, `;`, `0,00`/`1000000,00`, kepala laporan, saldo awal dan akhir; nama `buku-besar_4001_awal-akhir.csv` | `AccLedgerCsv`, `BuildMovementCsvAsync` |
| AC-6 Satu log `GeneralLedger.Export` tanpa nominal | **Terpenuhi** — A12: 3 baris log sesi uji di `Logs/quilvian-backend-20261124.json`, berisi `UserId` superadmin dan saringan, tanpa nominal; unduhan gagal tercatat `WRN` | `ExportMovements`: muatan berisi jenis laporan, saringan, status, jumlah baris |
| AC-7 Unit biaya badan hukum lain → `400` | **Terpenuhi** — A8: unit acak dan `CC-MMC-00001` dengan badan hukum `LE-MDC-001` → `400` pada ketiga endpoint | `SiapkanMutasiAsync` |
| AC-8 > 50.000 baris → `422` | **Belum terbukti** — terpetakan ke `BuildMovementCsvAsync`; `F19` `NOT FEASIBLE` di data dev | `BuildMovementCsvAsync` |
| DoD: laporan, roadmap, traceability | Terpenuhi | Berkas ini; roadmap dan traceability bertanda 🟡 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `using QuilvianSystemBackend.Responses;` di `AccGeneralLedgerService.cs` kini tidak terpakai; dibiarkan supaya berkas lama tidak disunting lebih dari yang perlu. Hanya peringatan kompilasi |
| Masalah yang diketahui | **CORS tidak mengekspos `Content-Disposition`** (`Program.cs`, di luar wewenang). Peramban lintas asal tidak dapat membaca nama berkas dari header, jadi `FE-ACC-FIN-005-01` menyusun nama berkas sendiri dengan pola yang sama — preseden hook unduhan Finance. Bila nama dari header diinginkan, itu perubahan `Program.cs` tersendiri |
| Risiko tersisa | (1) Proyeksi `Join` ke `BarisMutasi` lalu `OrderBy`/`Take`/`Sum` belum dibuktikan diterjemahkan EF tanpa galat — dibuktikan saat build dan uji pertama. (2) Bentuk CSV untuk Excel komputer rumah sakit `PROPOSED` (`F20`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` `GeneralLedgerController.cs`, `GeneralLedgerDtos.cs`, `AccGeneralLedgerService.cs`; `??` `Enums/`, `AccGeneralLedgerService.Export.cs`, `AccGeneralLedgerService.Movement.cs`, `AccLedgerCsv.cs`; ditambah laporan ini dan baris status roadmap/traceability |
| Langkah berikutnya | Rizki menjalankan `dotnet build -p:RunAnalyzers=false`; perhatikan galat pada ketiga berkas `AccGeneralLedgerService*.cs` dan `GeneralLedgerController.cs`. Sesudah lulus: `FE-ACC-FIN-005-01`, lalu uji bersama dari layar (`F10`, `F12`, `F13`, `F15`, `F16`, `F19`) dan task ini dinaikkan ke ✅ |
