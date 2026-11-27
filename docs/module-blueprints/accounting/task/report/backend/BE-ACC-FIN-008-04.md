# Laporan Perubahan Backend — `BE-ACC-FIN-008-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-008-04` |
| Judul | Rasio keuangan |
| Slice | Gelombang pengiriman `FIN-5` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 3, kartu `BE-ACC-FIN-008-04` |
| Trace | `FR-FIN-059`..`066`; `ACC-DEC-161`, `167` butir 4, `169`, `174`..`184` |
| Contract version | `ACC-API-0.19`, `ACC-VALIDATION-0.14` (`VAL-RAT-01`..`10`), `ACC-PERMISSION-0.10` — approved Rizki 26 November 2026 (`GATE-DESAIN-1126`) |
| Dependency | `BE-ACC-FIN-001-03` ✅ |
| Klasifikasi | `STANDARD`, tanpa perubahan schema |
| Task mode | `BACKEND` — perintah Rizki 26 November 2026 "mulai sekarang" |
| Target tulis | `GeneralLedger/` (enum, DTO, service, controller); laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `26229ec7` (branch `rizkiG`) + `BE-ACC-FIN-001-03` dan dokumen desain belum di-commit |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI (development)** — 26 November 2026: uji API H0–H12 **13/13** (putaran pertama, 11:28 WIB), build Rizki `PASS` (DLL 11:25:52 WIB, lebih baru dari source 11:13:15), QBE Strict `PASS` (7 berkas). AC-1..AC-6, AC-9, AC-10 `RUNTIME VERIFIED`; AC-7 dan jalur Dihitung AC-8 (Current/Quick/Cash dan ROA/ROE) `SOURCE VERIFIED` — dikecualikan atas keputusan izin Rizki (klasifikasi sementara saja; jurnal dan tahun buku 2025 tidak diizinkan), sesuai DoD kartu. Agregasi AC-8 `RUNTIME VERIFIED` (H6). UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — registry backend, `ACTIVE` |
| Keberlakuan | `NEW CODE` (tiga enum, empat DTO, `AccGeneralLedgerService.FinancialRatio.cs`, `BuildFinancialRatioCsvAsync`, dua action) dan `TOUCHED LEGACY` (`FinancialStatementDtos.cs`, `.Export.cs`, `GeneralLedgerController`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-LOG-001` |
| Tidak berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-DB-*` (tanpa entity, konfigurasi, migration); `QBE-MOD-002/003` (prefix `Acc` `ACTIVE`); `QBE-TXN-001` (baca saja); `QBE-PAGE-001` (bukan daftar berhalaman) |
| Jenis capability | Transaksi berarketipe **read-only** (laporan); bukan master data, sehingga sembilan endpoint baseline tidak berlaku |
| Role access | Dua action baru, masing-masing `[AccessAction("Read", "Read General Ledger", ...)]` + `[AccessPermission("GeneralLedger", "Read")]`; `ControllerName = "GeneralLedger"` cocok. Nol hak baru, nol hardcode peran |

## 1. Masalah yang diperbaiki

Accounting belum punya rasio keuangan. Pembaca laporan harus menghitung likuiditas, profitabilitas, dan
solvabilitas sendiri dari neraca dan laba rugi, tanpa tahu apakah klasifikasi akunnya sudah cukup untuk
itu.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Delapan rasio per periode beserta asal angka dan alasannya bila tidak dapat dihitung |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | `GET /general-ledger/financial-ratios` atau unduhan CSV |
| Langkah | 1) Penjaga badan hukum + periode. 2) Posisi neraca per akhir periode, akhir tahun buku lalu, akhir dua tahun buku lalu. 3) Laba dan pendapatan sejak awal tahun (tanpa `JT`). 4) Klasifikasi efektif sekali. 5) Delapan rasio per kolom dengan urutan status desain bagian 27.7 |
| Jalur tidak normal | Bentuk periode salah `400`; periode tidak ada `404`; tanpa hak `403`. Data tidak cukup bukan galat: rasio berstatus N/A, Tidak dapat dihitung, atau Tidak sah beserta alasannya |
| Hasil | `FinancialRatioResponse`; CSV `rasio-keuangan_{periode}.csv`; log tanpa nominal |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/Enums/FinancialRatioCode.cs`, `FinancialRatioUnit.cs`, `FinancialRatioStatus.cs` | **Baru** |
| `GeneralLedger/DTOs/FinancialStatementDtos.cs` | + `FinancialRatioQuery`, `FinancialRatioResponse`, `FinancialRatioRowResponse`, `FinancialRatioValueResponse` |
| `GeneralLedger/Services/AccGeneralLedgerService.FinancialRatio.cs` | **Baru** (`partial`): definisi delapan rasio dalam satu tabel `DaftarRasio`, `GetFinancialRatiosAsync`, `HitungPosisiRasioAsync`, `HitungLabaRasioAsync`, `HitungNilaiRasio`, pembantu alasan |
| `GeneralLedger/Services/AccGeneralLedgerService.Export.cs` | + `BuildFinancialRatioCsvAsync`, `SelRasio`, `LabelStatusRasio` |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | + `GetFinancialRatios`, `ExportFinancialRatios` |

`AccGeneralLedgerService.FinancialStatement.cs` (neraca, laba rugi) **tidak** disentuh oleh task ini; diff
yang tampil di berkas itu milik `BE-ACC-FIN-001-03` yang belum di-commit. Nol perubahan `Program.cs`,
`Migrations/`, model, konfigurasi EF.

### 3.2 Delta terhadap desain

| Butir | Desain | Source | Alasan |
| --- | --- | --- | --- |
| Alasan klasifikasi | "{kode nama, maks. lima}" | Lima pertama + ", dan {n} akun lain" bila lebih | Sama dengan peringatan neraca layar `FE-ACC-FIN-001-03` |
| `Numerator` saat saldo awal tidak tersedia | Tidak dirinci | Laba YTD + `DenominatorClosing` tetap dikirim | Pembaca tetap melihat separuh angka yang tersedia |
| Kolom CSV `Hasil` angka | `1,25x` / `12,50%` | Ditulis lewat `AccLedgerCsv.Teks` (berkutip, anti CSV-injection) | Hasil negatif berawalan `-` tetap aman |

## 4. Dokumentasi endpoint

| Method | Path | Hak | Request | Respons |
| --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/general-ledger/financial-ratios` | `GeneralLedger : Read` | `LegalEntityId`, `PeriodCode` | `ApiResponse<FinancialRatioResponse>` |
| `GET` | `/api/v1/corporate/accounting/general-ledger/financial-ratios/export` | `GeneralLedger : Read` | sama | `text/csv` |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan

| Pemeriksaan | Hasil | Status |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (7 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru | 0 | `PASS` |
| Pemeriksaan nama anggota baru terhadap berkas `partial` lain | Nol bentrok | `PASS` |
| `node --check` skrip uji | Lulus | `PASS` |
| `dotnet build` | Dijalankan Rizki: berhasil; DLL 26 November 2026 11:25:52 WIB, sesudah suntingan terakhir source 11:13:15 | `PASS` |
| Uji API `01-uji-api.mjs` (26 November 2026 11:28:15–11:28:26 WIB) | 13 PASS, 0 FAIL, 0 NOT RUN | `PASS` |

### 5.2 Uji yang dijalankan

Skrip: `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-04-20261126/01-uji-api.mjs`
(`NODE_TLS_REJECT_UNAUTHORIZED=0`). H0 gagal → berhenti tanpa menulis.

| ID | Skenario | AC |
| --- | --- | --- |
| H0 | Prasyarat build baru; 1-1000 dan 1-3000 belum berklasifikasi | — |
| H1 | Delapan baris, nama/satuan; DAR `0.00`, DER `0.00` Dihitung | AC-1 |
| H2 | Pembilang/penyebut DAR, DER = total Neraca; NPM = laba/pendapatan YTD Laba Rugi | AC-2 |
| H3 | Current/Quick/Cash Tidak dapat dihitung, alasan menyebut 4 akun | AC-3 |
| H4 | Pembanding dan ROA/ROE kini Tidak dapat dihitung (2025 tidak ada) | AC-5 |
| H5 | Sesudah klasifikasi sementara: Current/Quick/Cash N/A | AC-6 |
| H6 | Pembilang: aset lancar 3.230.000; Quick = Cash = 2.330.000 | AC-8 (agregasi) |
| H7 | 1-3000 tanpa sub-klasifikasi: Current N/A, Quick/Cash Tidak dapat dihitung menyebut 1-3002 | AC-4 |
| H8 | Klasifikasi dikembalikan kosong | Pembersihan |
| H9 | CSV | AC-9 |
| H10 | `400`, `404`, `403` | AC-10 |
| H11 | Regresi neraca | Regresi |
| H12 | Log tanpa nominal, jumlah status di teks pesan | AC-9 |

### 5.3 Hasil dan pemeriksaan bukti

Keluaran diperiksa sendiri: `01-uji-api.json` dan `01-rasio-keuangan.csv` di folder skrip.

| ID | Hasil | Bukti |
| --- | --- | --- |
| H0 | PASS | `200`; 1-1000 dan 1-3000 belum berklasifikasi |
| H1 | PASS | Delapan baris urutan 1..8, nama dan satuan benar. Kolom kini: Current/Quick/Cash status 3; NPM 1 (`100`); ROA/ROE 3; DAR 1 (`0`); DER 1 (`0`) |
| H2 | PASS | DAR 0 ÷ 3.230.000, DER 0 ÷ 3.230.000 = Total Liabilitas, Total Aset, Total Ekuitas Neraca 2026-09. NPM 2.000.000 ÷ 2.000.000 = laba bersih YTD dan pendapatan YTD Laba Rugi → `100,00%` (tahun 2026 di DB dev hanya punya pendapatan) |
| H3 | PASS | Alasan "Klasifikasi laporan belum lengkap pada 4 akun bersaldo: 1-1003 Kas Kecil, 1-1004 Bank Operasional, 1-3002 Persediaan Alat Kesehatan, 1002 Kas Besar."; `ClassificationGaps` empat akun |
| H4 | PASS | Pembanding seluruhnya "Tahun buku 2025 belum ada di sistem."; ROA/ROE "Saldo awal tahun buku 2026 tidak tersedia: tahun buku 2025 belum ada di sistem." dengan pembilang laba 2.000.000 tetap dikirim |
| H5 | PASS | Sesudah klasifikasi sementara: Current/Quick/Cash N/A "Penyebut bernilai nol.", penyebut 0, `ClassificationGaps` kosong |
| H6 | PASS | Pembilang Current 3.230.000; Quick 2.330.000; Cash 2.330.000 — persediaan 900.000 hanya masuk Aset Lancar |
| H7 | PASS | 1-3000 Lancar tanpa sub-klasifikasi: Current N/A; Quick/Cash "Sub-klasifikasi aset lancar belum diisi pada 1 akun bersaldo: 1-3002 Persediaan Alat Kesehatan."; celah `gapKind = 2` |
| H8 | PASS | 1-1000 dan 1-3000 kembali `null/null` |
| H9 | PASS | CSV ber-BOM, `;`, `rasio-keuangan_2026-09.csv`, delapan baris, `"0,00%"`, `"0,00x"`, `"100,00%"`, status bertulis, empat baris Catatan |
| H10 | PASS | `2026/09` → `400` pesan kontrak; `2099-01` → `404`; akun tanpa hak → `403` pada kedua endpoint |
| H11 | PASS | Neraca 2026-09 tetap: aset 3.230.000, seimbang, 4 akun belum lengkap |
| H12 | PASS | Log INF "…Kolom kini: dihitung 3, N/A 0, tidak dapat dihitung 5, tidak sah 0." (lalu "N/A 3, …2" dan "N/A 1, …4" sesudah klasifikasi sementara); WRN "Periode akuntansi tidak ditemukan."; `GeneralLedger.Export` "Rasio keuangan September 2026 berhasil diunduh."; tanpa nominal |

**Data dev.** Klasifikasi 1-1000 dan 1-3000 diisi sementara lalu dikosongkan, lewat `PUT` aplikasi, nol
SQL — izin Rizki 26 November 2026. Lima log `ChartOfAccount.Classification` ikut tertulis.

## 6. Acceptance criteria

| # | Source | Bukti runtime |
| --- | --- | --- |
| AC-1 | `DaftarRasio`, `GetFinancialRatiosAsync` | `RUNTIME VERIFIED` (H1) |
| AC-2 | `HitungPosisiRasioAsync`, `HitungLabaRasioAsync` memakai `JumlahPerAkunAsync`, `NilaiMenurutJenis`, `HitungLabaBelumDipindahAsync` yang sama dengan neraca | `RUNTIME VERIFIED` (H2) |
| AC-3 | `HitungNilaiRasio` cabang klasifikasi | `RUNTIME VERIFIED` (H3) |
| AC-4 | Cabang sub-klasifikasi | `RUNTIME VERIFIED` (H7) |
| AC-5 | `alasanPembanding`, `AlasanSaldoAwal` | `RUNTIME VERIFIED` (H4) |
| AC-6 | Cabang penyebut nol | `RUNTIME VERIFIED` (H5) |
| AC-7 | Cabang `tidakSeimbang`: posisi kolom, atau posisi awal untuk ROA/ROE; NPM tidak memakai neraca (`MemakaiNeraca = false`) | `SOURCE VERIFIED` — neraca tidak seimbang tidak dapat dibuat lewat aplikasi (`ACC-DEC-154`) |
| AC-8 | Agregasi per klasifikasi; pembagian dan pembulatan `AwayFromZero` (jalur sama yang menghasilkan DAR/DER `0` dan NPM `100` di H1–H2); rata-rata ROA/ROE `(awal + akhir) / 2` | Agregasi `RUNTIME VERIFIED` (H6); jalur Dihitung likuiditas dan ROA/ROE `SOURCE VERIFIED` — data uji tidak diizinkan |
| AC-9 | `BuildFinancialRatioCsvAsync`; log controller | `RUNTIME VERIFIED` (H9, H12) |
| AC-10 | Atribut akses; `SiapkanPeriodeLaporanAsync` | `RUNTIME VERIFIED` (H10) |

## 7. Catatan penutup

| Butir | Isi |
| --- | --- |
| Status migration / database | Tidak ada migration; tidak ada eksekusi database |
| Risiko tersisa | Terjemahan EF query baru tidak ada (memakai pembantu yang sudah berjalan); jumlah query ±11 per permintaan |
| Definition of Done | AC-1..AC-10 terbukti; AC-7 dan jalur Dihitung AC-8 `SOURCE VERIFIED` dengan alasan tertulis (diizinkan DoD kartu); laporan, roadmap, traceability diperbarui |
| Task berikutnya | `FE-ACC-FIN-008-04` |
| Git | Tidak ada stage, commit, push, atau merge |
