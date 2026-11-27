# Laporan Perubahan Backend — `BE-ACC-FIN-001-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-001-03` |
| Judul | Peringatan klasifikasi di neraca |
| Slice | Gelombang pengiriman `FIN-4` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 2, kartu `BE-ACC-FIN-001-03` |
| Trace | `FR-FIN-057`; `ACC-DEC-168`, `173` butir 5–6; menyentuh FIN-1 lewat `ACC-DEC-171` |
| Contract version | `ACC-API-0.18` `GET /general-ledger/balance-sheet` (+ `UnclassifiedAccountCount`, `ClassificationGaps`); `ACC-PERMISSION-0.9` (log) — approved Rizki 25 November 2026 (`GATE-DESAIN-1125`) |
| Dependency | `BE-ACC-FIN-001-02` ✅, `BE-ACC-FIN-008-03` ✅ |
| Klasifikasi | `STANDARD`, tanpa perubahan schema |
| Task mode | `BACKEND` — perintah Rizki 26 November 2026 "lanjut BE-ACC-FIN-001-03" |
| Target tulis | `GeneralLedger/` (service, DTO, controller); laporan ini; baris status roadmap dan traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `26229ec7` (branch `rizkiG`), working tree bersih sebelum task |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-6 `RUNTIME VERIFIED` 26 November 2026: uji API G0–G9 9/10 + periksa ulang log G9b 1/1 (G9 awal FAIL karena jendela waktu skrip, bukan cacat kode; bagian 5.3). Build Rizki `PASS` (DLL 26 November 2026 10:08:46 WIB); QBE Strict `PASS`. Izin menulis klasifikasi 1-1000 sementara diberikan Rizki 26 November 2026; sudah dikosongkan kembali. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — registry backend, `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` (`AkunLaporan`, `MuatAkunLaporanAsync`, `GetBalanceSheetAsync`, `BalanceSheetResponse`, action `GetBalanceSheet`) dan `NEW CODE` (`SusunCelahKlasifikasi`, `FinancialStatementClassificationGapResponse`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-LOG-001` |
| Tidak berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-DB-*` (tanpa entity, konfigurasi, atau migration); `QBE-MOD-002/003` (prefix `Acc` sudah `ACTIVE`); `QBE-TXN-001` (baca saja) |
| Role access | Nol action baru. `GET /balance-sheet` tetap `[AccessPermission("GeneralLedger", "Read")]` dengan `[AccessAction("Read", …)]` yang sama. Nol hardcode peran |

## 1. Masalah yang diperbaiki

Sesudah kolom klasifikasi ada (`BE-ACC-FIN-001-02`), neraca belum memberi tahu bila ada akun bersaldo yang
klasifikasinya belum lengkap. Akibatnya pengguna tidak tahu bahwa data untuk rasio likuiditas dan neraca
terkelompok belum siap, padahal angkanya sudah tampil.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Neraca menyebut akun bersaldo yang klasifikasinya belum lengkap, **tanpa** mengubah angka |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Membuka neraca (`GET /balance-sheet`) |
| Langkah | 1) Backend memuat akun badan hukum beserta klasifikasinya (satu kueri, sudah ada). 2) Menghitung klasifikasi efektif dengan `AccChartOfAccountClassification.Hitung`. 3) Mengambil akun bercelah yang nilainya tidak nol pada kolom berjalan atau pembanding. 4) Mengembalikan daftar dan jumlahnya; log mencatat jumlahnya |
| Jalur tidak normal | Periode tidak ada → `404` lama; tanpa hak → `403` lama |
| Hasil | `ClassificationGaps` (urut kode) dan `UnclassifiedAccountCount`; `FE-ACC-FIN-001-03` menampilkannya sebagai peringatan |

**Contoh.** Neraca 2026-09 DB dev sebelum klasifikasi: 1002, 1-1003, 1-1004 muncul dengan
`GapKind = 1` (klasifikasi belum diisi). Sesudah 1-1000 diisi Lancar · Kas dan Setara Kas, ketiganya
hilang dari daftar; total aset tetap.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/Services/AccGeneralLedgerService.FinancialStatement.cs` | `AkunLaporan` + `IsPostable`, `ReportClassification`, `CurrentAssetCategory`; `MuatAkunLaporanAsync` memproyeksikan ketiganya; `SusunCelahKlasifikasi` baru (memetakan ke `AkunKlasifikasi`, memanggil `Hitung` sekali, menyaring akun Aset/Liabilitas bercelah bernilai ≠ 0, urut kode ordinal); `GetBalanceSheetAsync` mengisi dua field baru |
| `GeneralLedger/DTOs/FinancialStatementDtos.cs` | `BalanceSheetResponse` + `UnclassifiedAccountCount`, `ClassificationGaps`; kelas baru `FinancialStatementClassificationGapResponse` (`AccountId`, `AccountCode`, `AccountName`, `AccountType`, `GapKind`) — tanpa nominal |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | `GetBalanceSheet`: muatan + `UnclassifiedAccountCount`; teks pesan log berhasil + "Akun bersaldo dengan klasifikasi belum lengkap: N." |

Nol perubahan pada `Program.cs`, `Migrations/`, model, konfigurasi EF, `BuildBalanceSheetCsvAsync`, dan
laba rugi.

### 3.2 Delta terhadap desain

| Butir | Desain | Source | Alasan |
| --- | --- | --- | --- |
| Muatan log `UnclassifiedAccountCount` | Satu muatan tambahan (bagian 26.9, `ACC-PERMISSION-0.9`) | Muatan tetap ditambahkan, **dan** jumlahnya ditulis di teks pesan | `LoggerService` platform tidak menyimpan objek muatan (terbukti `BE-ACC-FIN-001-02` K11). Pesan respons API tidak berubah; hanya teks log |
| Nilai "tidak nol" | Kolom berjalan atau pembanding | Nilai milik akun itu sendiri (`nilaiAkun`), bukan total bersama turunan | Celah hanya dihitung untuk akun penerima transaksi; akun induk tidak dihitung (bagian 26.8) |

### 3.3 Kenapa angka dan CSV tidak berubah

`SusunCelahKlasifikasi` hanya membaca `akun` dan `nilaiAkun` yang sudah dipakai `SusunBagian`; ia tidak
menulis apa pun ke keduanya, dan dipanggil sesudah seluruh bagian dan selisih dihitung.
`BuildBalanceSheetCsvAsync` memakai `GetBalanceSheetAsync` tetapi hanya membaca `Sections`, total, dan
status, sehingga dua field baru tidak masuk CSV. Laba rugi memakai pemuat akun yang sama; tiga kolom
tambahan tidak dibaca di jalurnya.

## 4. Dokumentasi endpoint

| Method | Path | Hak | Request | Respons |
| --- | --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/general-ledger/balance-sheet` | `GeneralLedger : Read` | Tidak berubah (`LegalEntityId`, `PeriodCode`, `IncludeZeroAccounts`) | Field lama + `unclassifiedAccountCount` (`int`) + `classificationGaps[]` (`accountId`, `accountCode`, `accountName`, `accountType`, `gapKind`) |
| `GET` | `/api/v1/corporate/accounting/general-ledger/balance-sheet/export` | `GeneralLedger : Read` | Tidak berubah | Tidak berubah |

## 5. Verifikasi

### 5.1 Yang sudah dijalankan

| Pemeriksaan | Hasil | Status |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (3 berkas) | `Files evaluated: 3`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru di diff | 0 | `PASS` |
| `git diff --stat` | 3 berkas, +73/−4 | — |
| `node --check` skrip uji | Lulus | `PASS` |
| `dotnet build` | Dijalankan Rizki: berhasil; DLL 26 November 2026 10:08:46 WIB, lebih baru dari suntingan terakhir source 10:00:51 | `PASS` |
| Uji API `01-uji-api.mjs` (26 November 2026 10:17:54–10:19:24 WIB) | G0–G8 PASS, G9 FAIL → 9/10 | Lihat 5.3 |
| Periksa ulang log `02-cek-log.mjs` (baca berkas log saja, nol tulis) | G9b PASS → 1/1 | `PASS` |

### 5.2 Uji yang dijalankan

Skrip: `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-001-03-20261126/01-uji-api.mjs`
(`NODE_TLS_REJECT_UNAUTHORIZED=0`). Keluaran: `01-uji-api.json`, `01-neraca-sebelum.csv`,
`01-neraca-sesudah.csv`.

| ID | Skenario | AC |
| --- | --- | --- |
| G0 | Prasyarat: build baru membawa dua field; 1-1000 dan 1002/1-1003/1-1004 belum berklasifikasi. Gagal → skrip berhenti tanpa menulis | — |
| G1 | `ClassificationGaps` = akun Aset/Liabilitas bercelah bernilai ≠ 0, urut kode, dicocokkan dengan `classificationGap` daftar COA dan baris neraca; jumlah = `UnclassifiedAccountCount`; tanpa nominal | AC-1 |
| G2 | 1002, 1-1003, 1-1004 disebut; akun bercelah bernilai nol (akun uji 1-5001 dst.) tidak disebut | AC-1 |
| G3 | `PUT` 1-1000 `{Lancar, Kas}` → ketiganya keluar | AC-2 |
| G4 | `Sections`, total, `IsBalanced`, `Difference`, pembanding identik | AC-3 |
| G5 | CSV identik kecuali baris "Diunduh" | AC-4 |
| G6 | Laba rugi 2026-09 identik | AC-6 |
| G7 | Periode 2099-01 → `404`; akun tanpa hak → `403` | Negatif |
| G8 | Klasifikasi 1-1000 dikosongkan kembali; neraca kembali seperti semula | Pembersihan |
| G9 | Log `FinancialStatement.BalanceSheet`: tiga pesan INF berisi jumlah, WRN `404` pesan lama, tanpa nominal | AC-5 |

**Data dev yang berubah sementara.** Klasifikasi 1-1000 Kas dan Setara Kas (akun asli) diisi Lancar · Kas
dan Setara Kas lalu dikosongkan kembali dalam satu run, lewat `PUT` aplikasi, nol SQL (izin Rizki 26 November
2026). Keadaan akhir: `reportClassification = null`, `currentAssetCategory = null` (G8). Dua log
`ChartOfAccount.Classification` ikut tertulis (kosong → Lancar/Kas, lalu Lancar/Kas → kosong). Akun uji
1-5000 tidak dapat dipakai karena seluruh turunannya bersaldo nol, sehingga tidak pernah muncul di neraca.

### 5.3 Hasil dan pemeriksaan bukti

Keluaran diperiksa sendiri: `01-uji-api.json`, `02-cek-log.json`, `01-neraca-sebelum.csv`,
`01-neraca-sesudah.csv` di folder skrip.

| ID | Hasil | Bukti |
| --- | --- | --- |
| G0 | PASS | Neraca 2026-09 `200` membawa kedua field; 1-1000 dan 1002/1-1003/1-1004 belum berklasifikasi |
| G1 | PASS | `unclassifiedAccountCount = 4`: 1-1003 Kas Kecil, 1-1004 Bank Operasional, 1-3002 Persediaan Alat Kesehatan, 1002 Kas Besar — semua `gapKind = 1`; cocok persis dengan himpunan harapan dari daftar COA × baris neraca; tanpa field nominal. Urutan ordinal (`-` sebelum `0`) sehingga 1002 di akhir |
| G2 | PASS | 1002, 1-1003, 1-1004 disebut; akun bercelah bersaldo nol (antara lain akun uji 1-5001) tidak disebut |
| G3 | PASS | `PUT` 1-1000 `200`; jumlah 4 → 1, sisa hanya 1-3002 |
| G4 | PASS | `Sections`, total, `IsBalanced`, `Difference`, pembanding, `UnclosedFiscalYears` identik; total aset Rp 3.230.000 |
| G5 | PASS | Kedua CSV `200`, identik kecuali baris "Diunduh"; tidak ada kata klasifikasi di CSV |
| G6 | PASS | Laba rugi 2026-09 identik, dua bagian |
| G7 | PASS | Periode 2099-01 → `404` "Periode akuntansi tidak ditemukan."; akun tanpa hak → `403` |
| G8 | PASS | 1-1000 kembali kosong; neraca kembali 4 akun, angka identik dengan awal |
| G9 | **FAIL** | Hanya 2 dari 3 pesan INF tertangkap. **Cacat skrip:** `logAwal` diambil sesudah pembacaan neraca pertama, sehingga log d0 jatuh di luar jendela. Dua pesan yang tertangkap benar isinya. Skrip sudah dibetulkan (`logAwal = startedAt`) untuk run berikutnya |
| G9b | PASS | Jendela seluruh run 01: tepat empat log `FinancialStatement.BalanceSheet` berurutan — INF "…belum lengkap: 4.", INF "…: 1.", WRN "Periode akuntansi tidak ditemukan.", INF "…: 4."; tanpa nominal |

## 6. Acceptance criteria

| # | Kriteria | Source | Bukti runtime |
| --- | --- | --- | --- |
| AC-1 | Celah neraca 2026-09 sebelum klasifikasi, tanpa akun bernilai nol | `SusunCelahKlasifikasi` | `RUNTIME VERIFIED` (G1, G2) |
| AC-2 | Sesudah 1-1000 diklasifikasi, 1002/1-1003/1-1004 keluar | `Hitung` mewariskan dari 1-1000 | `RUNTIME VERIFIED` (G3) |
| AC-3 | Angka dan susunan identik | Bagian 3.3 | `RUNTIME VERIFIED` (G4, G8) |
| AC-4 | CSV identik | `BuildBalanceSheetCsvAsync` tidak berubah | `RUNTIME VERIFIED` (G5) |
| AC-5 | Log memuat jumlah, tanpa nominal | Controller `GetBalanceSheet` | `RUNTIME VERIFIED` (G9b) |
| AC-6 | Laba rugi tidak berubah | `GetIncomeStatementAsync` tidak membaca kolom baru | `RUNTIME VERIFIED` (G6) |

## 7. Catatan penutup

| Butir | Isi |
| --- | --- |
| Status migration / database | Tidak ada migration; tidak ada eksekusi database |
| Risiko tersisa | Tidak ada yang diketahui untuk lingkup ini. Temuan data (bukan cacat): 1-3002 Persediaan Alat Kesehatan tetap belum lengkap sampai 1-3000 diklasifikasi pemilik proses akuntansi (bagian 26.13) |
| Definition of Done | AC-1..AC-6 terbukti; laporan ditulis; roadmap dan traceability diperbarui. Tidak ada butir yang dikecualikan. UAT `F38` milik tim UAT |
| Task berikutnya | `FE-ACC-FIN-001-03` (peringatan di `balance-sheet-view`, tautan `?klasifikasi=belum-lengkap`, sembunyi saat cetak) |
| Git | Tidak ada stage, commit, push, atau merge |
