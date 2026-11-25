# Laporan Perubahan Backend — `BE-ACC-FIN-008-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-008-03` |
| Judul | Neraca dengan laba belum ditutup |
| Slice | Gelombang pengiriman `FIN-1` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 1, kartu `BE-ACC-FIN-008-03` |
| Trace | ACC-FIN-008 bagian Balance Sheet (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv84`); `FR-FIN-012`, `014`, `015`, `020`..`022`; `ACC-DEC-143`, `144`, `145`, `146`, `147` butir 3, `149`, `150`, `154`, `155` |
| Contract version | `ACC-API-0.17` (`GET /general-ledger/balance-sheet`, `/balance-sheet/export`); `VAL-GL-04`, `05`, `07`, `08`, `09`; `ACC-PERMISSION-0.8` — approved Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | `BE-ACC-FIN-008-02` ✅ (pembantu periode, hierarki, nilai per jenis akun) |
| Klasifikasi | `STANDARD` — dua endpoint baca, nol migration, nol hak baru |
| Task mode | `BACKEND` — perintah Rizki 25 November 2026 "lanjut BE-ACC-FIN-008-03" |
| Target tulis | `GeneralLedger/DTOs/FinancialStatementDtos.cs`, `GeneralLedger/Services/AccGeneralLedgerService.FinancialStatement.cs`, `AccGeneralLedgerService.Export.cs`, `GeneralLedgerController.cs`; laporan ini; baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `cbe20681` (branch `rizkiG`, sesudah merge `QuilvianIntegrationBackend` oleh Rizki) |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-9 `RUNTIME VERIFIED` 25 November 2026: uji API 12/12 + uji tutup tahun 2020 4/4 (`F5`, `F8` dengan data uji yang disetujui Rizki). Build `PASS` (Rizki, DLL 10:54:02 WIB); QBE Strict `PASS`. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — registry backend baris 42, `ACTIVE` |
| Keberlakuan | `NEW CODE` (dua method, dua action, dua DTO) dan `TOUCHED` pada kode `008-02` sendiri (`JumlahPerAkunAsync`, `BarisLaporanKeuangan`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Tidak berlaku | `QBE-DB-*`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-TXN-001`, `QBE-PAGE-001` |

## 1. Masalah yang diperbaiki

Accounting belum punya neraca. Selain itu, laba tahun yang belum ditutup tidak terlihat di mana pun. Akibatnya kelalaian tutup tahun tercampur diam-diam ke laba tahun berjalan (`ACC-DEC-143`).

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menampilkan posisi Aset, Liabilitas, dan Ekuitas per akhir periode, beserta pembanding akhir tahun buku lalu |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Layar Neraca (`FE-ACC-FIN-008-03`) atau unduhan CSV |
| Langkah | 1) Periksa bentuk dan keberadaan periode. 2) Jumlahkan saldo akun Aset, Liabilitas, dan Ekuitas sampai `EndDate` periode, dari seluruh jurnal `Posted` termasuk `JT`. 3) Hitung dua baris laba dari akun Pendapatan dan Beban: sejak awal tahun buku, dan sebelum awal tahun buku dikelompokkan per tahun. 4) Susun hierarki, tambahkan baris laba di Ekuitas. 5) Hitung status seimbang apa adanya |
| Jalur tidak normal | Bentuk periode salah → `400`; periode tidak ada → `404`; tanpa hak → `403`; tahun buku lalu tidak ada → `200` dengan pembanding `null` dan alasan; tidak seimbang → `200` dengan `IsBalanced = false` dan `Difference` |
| Hasil | `BalanceSheetResponse` atau berkas `neraca_{kode periode}.csv` |

**Contoh dari data dev** (periode September 2026). Tahun buku 2019 berisi data uji `BE-ACC-FIN-008-02`, dan JT-nya sudah dibalik, sehingga 2019 efektif belum ditutup. Hasilnya:

- Neraca memuat baris "Laba (Rugi) Tahun Lalu Belum Ditutup" sebesar Rp 270.000 dengan `UnclosedFiscalYears = [2019]`.
- Baris "Laba (Rugi) Tahun Berjalan" sebesar Rp 2.000.000.
- Total aset Rp 3.170.000 = total liabilitas dan ekuitas, sehingga statusnya Seimbang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/DTOs/FinancialStatementDtos.cs` | + `BalanceSheetQuery` (`PeriodCode` bertipe `string?`, alasannya sama dengan `008-02`), `BalanceSheetResponse` |
| `GeneralLedger/Services/AccGeneralLedgerService.FinancialStatement.cs` | + `GetBalanceSheetAsync`, `HitungLabaBelumDipindahAsync`, `TambahBarisLaba`, kelas `LabaBelumDipindah`, dua nama baris laba. `JumlahPerAkunAsync` kini menerima batas awal `null` (saldo sejak awal data); pemanggil `008-02` tidak berubah |
| `GeneralLedger/Services/AccGeneralLedgerService.Export.cs` | + `BuildBalanceSheetCsvAsync`, `StatusSeimbang`; `BarisLaporanKeuangan` mendapat parameter `denganYtd` (bawaan `true`, CSV laba rugi tidak berubah) |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | + `GET balance-sheet` (log `FinancialStatement.BalanceSheet`) dan `GET balance-sheet/export` (log `GeneralLedger.Export`, jenis `Neraca`); keduanya `[AccessAction("Read", "Read General Ledger", …)]` + `[AccessPermission("GeneralLedger", "Read")]` |

Nol baris komentar; nol entity, tabel, migration, hak akses, atau registrasi baru.

### 3.2 Cara menghitung (`02-backend-architecture.md` bagian 25.6)

| Unsur | Implementasi |
| --- | --- |
| Saldo akun | Seluruh baris `Posted` (termasuk `JT`) akun Aset/Liabilitas/Ekuitas bertanggal ≤ `EndDate`, satu `GROUP BY` per kolom. Aset debit − kredit; Liabilitas dan Ekuitas kredit − debit |
| Pembanding | `EndDate` terbesar periode tahun buku `P − 1`; tidak ada → `ComparisonAvailable = false`, alasan "Tahun buku {P − 1} belum ada di sistem." |
| Laba Tahun Berjalan | Σ(kredit − debit) Pendapatan dan Beban bertanggal [awal tahun buku, tanggal laporan], termasuk `JT`. Selalu tampil |
| Laba Tahun Lalu Belum Ditutup | Σ(kredit − debit) Pendapatan dan Beban sebelum awal tahun buku, dikelompokkan per `FiscalYear` periode jurnalnya; tahun yang ≠ 0 masuk `UnclosedFiscalYears`. Tampil hanya bila ≠ 0 di salah satu kolom |
| Baris laba | `RowKind = 3`, `AccountId` dan kode kosong, kedalaman 0, sesudah akun Ekuitas; ikut total Ekuitas |
| Seimbang | `Difference = Aset − (Liabilitas + Ekuitas)` apa adanya, tanpa pembulatan (`ACC-DEC-154`); begitu pula untuk kolom pembanding |
| `CostCenterId` | Tidak ada di `BalanceSheetQuery`, sehingga parameter yang terlanjur dikirim diabaikan (`VAL-GL-07`) |

### 3.3 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `ACC-API-0.17`. Bunyi alasan pembanding neraca ditetapkan "Tahun buku {tahun} belum ada di sistem." — kontrak hanya memberi contoh untuk laba rugi |
| Database | `NOT APPLICABLE` — hanya membaca |
| Log | Muatan berisi jenis laporan, badan hukum, periode, pilihan akun nol, status, jumlah baris; **tanpa** nominal maupun `Difference` |
| Regresi | `JumlahPerAkunAsync` dan `BarisLaporanKeuangan` disentuh dengan perilaku bawaan yang sama untuk laba rugi |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / General Ledger

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/general-ledger/balance-sheet?LegalEntityId=&PeriodCode=&IncludeZeroAccounts=` | Neraca per akhir periode dan pembanding | `GeneralLedger : Read` |
| `GET` | `/api/v1/corporate/accounting/general-ledger/balance-sheet/export?…` (sama) | CSV neraca | `GeneralLedger : Read` |

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (4 berkas) | `Files evaluated: 4`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar baru pada diff | 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false` | Dijalankan Rizki; DLL 25 November 2026 10:54:02 WIB, sesudah suntingan source terakhir 10:45:56; DLL memuat `BuildBalanceSheetCsvAsync` | `PASS` |
| `fin-008-03-20261125/01-uji-api.mjs` N1–N14 (10.56:50–10.56:56 WIB, data apa adanya, nol penulisan) | 12 PASS, 0 FAIL, 2 `NOT RUN` (N13 `F5`, N14 `F8` — diganti uji 02) | `PASS` |
| `fin-008-03-20261125/02-uji-tutup-tahun-2020.mjs` P1–P4 (10.57:09–10.57:21 WIB, sesudah persiapan fase S) | 4/4 | `PASS` |

Dijalankan agent terhadap backend yang dinyalakan Rizki. Semua lewat API aplikasi, **nol SQL**. Persiapan tercatat di `persiapan.json` (17 entri):

- Izin sementara `Journal : Approve` dan `AccountingPeriod : Approve` untuk akun penyetuju uji, dipulihkan sesudahnya (kembali 6 izin, terverifikasi).
- JU/2020/12/00001 (Kas 1002 / 4-1003 Rp 100.000) dan JU/2020/12/00002 (5-2003 unit CC-MMC-00001 / 1-1004 Rp 40.000).
- Periode 2020-01..12 ditutup sementara lewat ajukan + setujui.
- JT/2020/12/00001 disahkan.

**Hasil mentah utama:**

| Uji | Isi |
| --- | --- |
| N1/N2 (2026-09) | Aset 3.170.000 = 1002 1.270.000 + 1-1003 1.000.000 + 1-3002 900.000; Liabilitas 0; Ekuitas 3-3001 900.000 + Laba Tahun Lalu Belum Ditutup 270.000 + Laba Tahun Berjalan 2.000.000 = 3.170.000; `IsBalanced = true`, `Difference = 0` |
| N5 (2020-12 lawan 31 Des 2019) | Kedua kolom seimbang; Laba Tahun Berjalan 0 / pembanding 270.000; Laba Tahun Lalu Belum Ditutup 270.000 / pembanding 0 |
| N6 | `IncludeZeroAccounts = true` 25 dari 25 akun neraca; `false` 9 baris akun |
| N9 | CSV identik baris per baris dengan JSON, termasuk dua baris laba, `"Total Liabilitas dan Ekuitas"`, `;"Status";"Seimbang";;`, dan `"Catatan";"Tahun buku 2019 belum ditutup."` |
| N12 | 6 pembacaan = 6 log `FinancialStatement.BalanceSheet`; 2 unduhan = 2 log `GeneralLedger.Export`; nol nominal maupun `Difference` |
| P1 | 1-1004 = −40.000; subtotal 1-1000 2.330.000 = 1-1003 1.000.000 + 1-1004 −40.000 + 1002 1.370.000 |
| P2 | Sebelum JT 2020: `UnclosedFiscalYears = [2019, 2020]`, laba tahun lalu 330.000 |
| P3 | Sesudah JT/2020/12/00001: `UnclosedFiscalYears = [2019]`, laba tahun lalu 270.000, akun laba ditahan 3-1001 naik tepat 60.000, total ekuitas tetap 3.230.000, seimbang |
| P4 | Laba rugi 2020-12 identik sebelum dan sesudah JT 2020 (laba bersih 60.000) |

## 6. Acceptance criteria

| Kriteria | Status | Rencana bukti runtime |
| --- | --- | --- |
| AC-1 Tiga bagian; baris laba `RowKind = 3` di Ekuitas | `RUNTIME VERIFIED` | N1 |
| AC-2 Data sehat seimbang di kedua kolom (`F4`) | `RUNTIME VERIFIED` | N2, N5, P1–P3 |
| AC-3 Tahun lalu belum ditutup → baris dan `UnclosedFiscalYears` (`F4`) | `RUNTIME VERIFIED` | N3 (`[2019]`), P2 (`[2019, 2020]`) |
| AC-4 Sesudah `JT` disahkan baris hilang, Laba Ditahan naik, ekuitas tetap (`F5`) | `RUNTIME VERIFIED` | P3 |
| AC-5 Akun kontra negatif (`F8`); akun nol (`F9`) | `RUNTIME VERIFIED` | P1; N6 |
| AC-6 Pembanding tidak ada → `null` dan alasan | `RUNTIME VERIFIED` | N4 |
| AC-7 `CostCenterId` tidak mengubah angka | `RUNTIME VERIFIED` | N7 |
| AC-8 Log tanpa nominal termasuk `Difference` (`F17`) | `RUNTIME VERIFIED` | N12 |
| AC-9 CSV lengkap dengan total L+E dan status | `RUNTIME VERIFIED` | N9 |
| Negatif `VAL-GL-04`/`05`, tanpa hak | `RUNTIME VERIFIED` | N8, N10 |
| Regresi laba rugi dan neraca saldo | `RUNTIME VERIFIED` | N11, P4 |

**DoD.** AC-1..AC-9 terbukti; laporan ditulis; roadmap dan traceability diperbarui → ✅.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | **Tutup tahun tidak dapat disusun ulang sesudah JT dibalik.** `AccYearEndClosingService.CariJurnalPenutupAsync` menolak bila ada jurnal `JT` berstatus apa pun pada tahun itu, termasuk yang sudah dibalik. Padahal pesannya menyarankan "Hapus atau balik jurnal itu lebih dahulu bila memang perlu disusun ulang". Jurnal `Posted` tidak dapat dihapus, sehingga tahun 2019 di dev kini tidak dapat ditutup ulang. Di luar lingkup (`AccountingPeriod`, `BE-ACC-P2-010`) |
| Risiko | Rendah. Terjemahan EF untuk `Join` jurnal → periode lalu `GroupBy FiscalYear` terbukti di runtime (N3, P2, P3) |
| Status Git | `M` `GeneralLedgerController.cs`, `AccGeneralLedgerService.Export.cs`, `AccGeneralLedgerService.FinancialStatement.cs`, `FinancialStatementDtos.cs`; laporan ini |
| Temuan konfigurasi DB dev | Akun laba ditahan pada pengaturan akuntansi badan hukum ini menunjuk **3-1001 Modal Disetor**, bukan 3-3001 Laba Ditahan. JT 2019 dan 2020 karena itu mengkredit Modal Disetor. Kode benar (memakai akun yang ditunjuk pengaturan); yang perlu diperiksa adalah pengaturannya. Tidak diubah |
| Keadaan DB dev sesudah uji | Tahun buku 2020 berisi data uji dan tertutup (JT/2020/12/00001 disahkan; periode tutup sementara). 2019 tetap belum ditutup |
| Langkah berikutnya | `FE-ACC-FIN-008-03` (layar Neraca, memakai ulang `financial-statement-table`) |

## 8. Data uji yang dibutuhkan untuk `F5` dan `F8`

**Disetujui Rizki 25 November 2026** ("Boleh, buat data 2020"). Skrip: `fin-008-03-20261125/02-uji-tutup-tahun-2020.mjs`, dijalankan **sesudah** `01-uji-api.mjs`, karena 01 mengharapkan keadaan sebelum data 2020.

Tahun buku 2019 tidak dapat ditutup ulang (bagian 7). Rencananya memakai tahun buku 2020, lewat aplikasi dan tanpa SQL, dengan izin persetujuan sementara akun penyetuju uji yang dipulihkan sesudahnya:

1. Dua jurnal Desember 2020:
   - Kas / Pendapatan Penunjang Medis Rp 100.000.
   - Beban Penyusutan (5-2003, unit CC-MMC-00001) / Bank Operasional (1-1004) Rp 40.000. Akun 1-1004 menjadi aset bersaldo kredit. Akumulasi Penyusutan (1-4002) sengaja tidak dipakai karena berstatus *control account*: jurnal manual ke sana dapat membuat rekonsiliasi subledger 2020 tidak bersih, padahal rekonsiliasi itu diperiksa saat pengajuan penutupan periode.
2. Neraca 2026-09 diambil sebelum tutup tahun. Harapannya:
   - `UnclosedFiscalYears = [2019, 2020]`.
   - Baris tahun lalu sebesar 270.000 + 60.000.
   - 1-1004 tampil −40.000 dan mengurangi subtotal 1-1000 (`F8`).
3. Periode 2020 ditutup sementara lewat ajukan + setujui, lalu JT 2020 disusun dan disahkan.
4. Neraca 2026-09 diambil sesudah tutup tahun. Harapannya:
   - `UnclosedFiscalYears = [2019]`.
   - Baris tahun lalu 270.000.
   - Akun laba ditahan pada pengaturan akuntansi naik 60.000.
   - Total ekuitas tetap (`F5`).
