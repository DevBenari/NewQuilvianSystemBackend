# Laporan Perubahan Backend — `BE-ACC-FIN-008-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-FIN-008-02` |
| Judul | Laba rugi periode dan YTD |
| Slice | Gelombang pengiriman `FIN-1` |
| Roadmap | `roadmap/backend-roadmap-finishing.md` revisi 1, kartu `BE-ACC-FIN-008-02` |
| Trace | ACC-FIN-008 bagian Income Statement (PRD `ACC-FIN-PRD-001`; ClickUp `z8m132yv84`); `FR-FIN-010`..`015`; `ACC-DEC-142`, `144`, `145`, `146`, `147` butir 2 dan 4, `149`, `150`, `155` |
| Contract version | `ACC-API-0.17` (`GET /general-ledger/income-statement`, `/income-statement/export`); `VAL-GL-01`, `04`, `05`, `08`; `ACC-PERMISSION-0.8` — approved Rizki 24 November 2026 (`GATE-DESAIN-1124`) |
| Dependency | `BE-ACC-FIN-005-01` ✅ (`AccLedgerCsv`, berkas `partial`, `KodeJenisJurnalTutupTahun`) |
| Klasifikasi | `STANDARD` — dua endpoint baca, satu berkas service baru, nol migration, nol hak baru |
| Task mode | `BACKEND` — perintah Rizki 25 November 2026 "lanjut BE-ACC-FIN-008-02" |
| Target tulis | `GeneralLedger/DTOs/FinancialStatementDtos.cs`, `GeneralLedger/Enums/FinancialStatementRowKind.cs`, `GeneralLedger/Services/AccGeneralLedgerService.FinancialStatement.cs` (baru); `AccGeneralLedgerService.Export.cs`, `GeneralLedgerController.cs`; laporan ini; baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `576faba9` (branch `rizkiG`) |
| Tanggal | 25 November 2026 |
| Status | ✅ **SELESAI (development)** — AC-1..AC-10 `RUNTIME VERIFIED` 25 November 2026: uji API 13/13 PASS ditambah uji tutup tahun 5/5 PASS. Build `PASS` (Rizki, DLL 09:48:52 WIB); QBE Strict `PASS`. UAT belum dijalankan — milik tim UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki / `Acc` — registry backend baris 42, `ACTIVE` |
| Keberlakuan | `NEW CODE` (tiga berkas baru, dua method dan dua action baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Tidak berlaku | `QBE-DB-*`, `QBE-ENT-*`, `QBE-CFG-*`, `QBE-TXN-001` (tidak ada tulis), `QBE-PAGE-001` (laporan, bukan daftar berhalaman) |
| Selisih governance | Salinan registry di suite berbeda dari `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — `ACC-DEP-007`, milik lead, tidak disentuh |

## 1. Masalah yang diperbaiki

Accounting belum punya laporan laba rugi. Petugas harus menjumlahkan sendiri saldo akun Pendapatan dan Beban dari neraca saldo, tanpa pembanding tahun lalu. Angka itu juga ikut berubah begitu jurnal tutup tahun (`JT`) disahkan.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Menampilkan laba rugi satu periode beserta YTD dan pembanding periode yang sama tahun lalu |
| Pelaku | Pemegang `GeneralLedger : Read` |
| Pemicu | Layar Laba Rugi (`FE-ACC-FIN-008-02`) atau unduhan CSV |
| Langkah | 1) Periksa bentuk kode periode dan keberadaannya. 2) Periksa unit biaya bila diisi. 3) Tentukan empat rentang: periode, YTD, pembanding, YTD pembanding. 4) Jumlahkan baris jurnal `Posted` akun Pendapatan dan Beban per akun di database, tanpa jurnal `JT` dan pembaliknya. 5) Susun pohon akun, sembunyikan akun nol bila diminta. 6) Hitung total bagian dan laba bersih |
| Jalur tidak normal | Bentuk periode salah → `400`; periode tidak ada → `404`; unit biaya bukan milik badan hukum → `400`; tanpa hak → `403`; pembanding tidak ada → `200` dengan kolom pembanding `null` dan alasannya |
| Hasil | `IncomeStatementResponse` atau berkas `laba-rugi_{kode periode}[_{kode unit}].csv` |

**Contoh.** Periode September 2026 pada data dev:

- Pendapatan Rawat Jalan menerima kredit Rp 2.000.000, jadi kolom Periode bernilai 2.000.000.
- Tahun buku 2025 tidak ada, sehingga `ComparisonAvailable = false` dengan alasan "Periode September 2025 belum ada di sistem.".
- Dengan saringan unit `CC-MMC-00001`, hanya baris unit itu yang terhitung (Rp 1.000.000), dan `CostCenterNotice` terisi.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `GeneralLedger/DTOs/FinancialStatementDtos.cs` | **Baru.** `IncomeStatementQuery`, `IncomeStatementResponse`, `FinancialStatementSectionResponse`, `FinancialStatementRowResponse`, `FinancialStatementAmountsResponse` |
| `GeneralLedger/Enums/FinancialStatementRowKind.cs` | **Baru.** `Account = 1`, `Group = 2`, `Computed = 3` |
| `GeneralLedger/Services/AccGeneralLedgerService.FinancialStatement.cs` | **Baru.** `GetIncomeStatementAsync`, ditambah pembantu yang dipakai ulang `BE-ACC-FIN-008-03`: `SiapkanPeriodeLaporanAsync` (`VAL-GL-04`/`05`), `CariUnitBiayaAsync` (`VAL-GL-01`), `MuatAkunLaporanAsync`, `BarisTanpaTutupTahunAsync`, `JumlahPerAkunAsync`, `NilaiMenurutJenis`, `IndukTampilan`, `SusunBagian` |
| `GeneralLedger/Services/AccGeneralLedgerService.Export.cs` | + `BuildIncomeStatementCsvAsync`, `BarisLaporanKeuangan` |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | + `GET income-statement` (log `FinancialStatement.IncomeStatement`) dan `GET income-statement/export` (log `GeneralLedger.Export`, jenis `LabaRugi`); keduanya `[AccessAction("Read", "Read General Ledger", …)]` + `[AccessPermission("GeneralLedger", "Read")]` |

Nol baris komentar; nol entity, tabel, kolom, migration, hak akses, registrasi `Program.cs`, atau dependency baru.

### 3.2 Cara menghitung (`02-backend-architecture.md` bagian 25.6)

| Unsur | Implementasi |
| --- | --- |
| Pintu data | `BarisDisahkan` → hanya jurnal `Posted` (AC-8) |
| Pengecualian `JT` (AC-3) | Jenis `JT` dicari lewat `AccJournalTypeService.CariMenurutKodeAsync`, tanpa Guid tertulis di kode. Baris dibuang bila jurnalnya berjenis `JT`, atau `ReversalOfJournalId`-nya menunjuk jurnal `JT`. Bila jenis `JT` tidak ada, tidak ada yang dibuang |
| Rentang | Periode `[P.StartDate, P.EndDate]`; YTD dari `StartDate` terkecil tahun buku `P`; pembanding = periode bulan sama tahun buku `P − 1`; YTD pembanding dari awal tahun buku itu |
| Jumlah | Empat `GROUP BY AccountId` di database, satu per rentang; tidak ada baris jurnal yang ditarik ke memori |
| Nilai akun (AC-5) | Pendapatan `kredit − debit`; Beban `debit − kredit`. Dihitung dengan pengurangan, bukan perkalian −1, supaya tidak ada nol bertanda minus. Akun kontra otomatis negatif dan mengurangi subtotal |
| Hierarki | Induk sejenis terdekat; induk berjenis lain dilewati ke leluhur sejenis, atau menjadi akar. Nilai induk = nilai sendiri + jumlah anak. Daftar pra-order, saudara urut `AccountCode` ordinal. Pengaman data rusak: akun yang tidak terjangkau dari akar (siklus induk) dijadikan akar, supaya nilainya tidak hilang dari total |
| Akun nol (AC-6) | Tampil bila `IncludeZeroAccounts`, atau ada kolom bukan nol, atau ada anak yang tampil |
| Pembanding (AC-4) | Tidak ada → `Comparison`, `Variance`, dan kedua YTD pembanding `null`, bukan `0`; alasan "Periode {bulan} {tahun − 1} belum ada di sistem." |
| `PeriodCode` | Bertipe `string?`. Dengan `Nullable` aktif, `string` non-nullable membuat ASP.NET menolak kode kosong dengan ProblemDetails bawaan sebelum service berjalan. Akibatnya pesan `VAL-GL-04` dan log pembacaan tidak muncul |

### 3.3 Dampak

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint sesuai `ACC-API-0.17`; nol delta kontrak |
| Database | `NOT APPLICABLE` — hanya membaca (`AsNoTracking`) |
| Keamanan | Hak `GeneralLedger : Read` yang sama; log tanpa nominal; CSV memakai pengaman rumus `AccLedgerCsv.Teks` |
| Regresi | Buku besar, neraca saldo, dan pratinjau tutup tahun tidak disentuh. `SiapkanMutasiAsync` tidak diubah, sehingga pemeriksaan unit biaya kini ada dua salinan kecil yang sama (sengaja, supaya kode `005-01` yang sudah teruji tidak berubah) |

## 4. Dokumentasi endpoint

#### Corporate / Accounting / General Ledger

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/general-ledger/income-statement?LegalEntityId=&PeriodCode=&CostCenterId=&IncludeZeroAccounts=` | Laba rugi periode, YTD, pembanding | `GeneralLedger : Read` |
| `GET` | `/api/v1/corporate/accounting/general-ledger/income-statement/export?…` (sama) | CSV laba rugi | `GeneralLedger : Read` |

## 5. Verifikasi

| Perintah atau skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (5 berkas) | `Files evaluated: 5`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Pemindaian baris komentar pada diff dan berkas baru | 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false` | Dijalankan Rizki; `QuilvianSystemBackend.dll` 25 November 2026 09:48:52 WIB, sesudah suntingan source terakhir 09:41:52; DLL memuat `BuildIncomeStatementCsvAsync` | `PASS` |
| `01-uji-api.mjs` I1–I15 (09.50 WIB, data apa adanya, nol penulisan) | 13 PASS, 0 FAIL, 2 `NOT RUN` (I13 `JT`, I14 akun kontra — dev belum punya datanya) | `PASS` |
| `04-uji-tutup-tahun.mjs` J1–J5 (run 1 mulai 09.54 WIB; run resmi ketiga 09.55:45–09.55:57 WIB, sesudah persiapan fase S) | 5/5 PASS pada run ketiga. Run 1 berhenti karena akun beban wajib menyebut unit biaya (aturan jurnal yang ada). Run 2 berhenti karena periode wajib ditutup lewat ajukan + setujui orang kedua. Kode task ini tidak diubah di antara run | `PASS` |
| `06-jajak-draft.mjs` | Jurnal Ditolak JU/2026/09/00004 (kredit 4001 100.000) dan Diajukan JB/2026/09/00002 (debit 4001 1.000.000) tidak ikut; 4001 tetap 2.000.000 | `PASS` (AC-8) |

Uji dijalankan agent terhadap backend yang dinyalakan Rizki. Semua lewat API aplikasi; **nol SQL**. Bukti ada di `QuilvianSystemFrontendDev/test-with-agy/accounting/fin-008-02-20261125/`: JSON tiap skrip, CSV `01-laba-rugi-2026-09*.csv`, `04-laba-rugi-2019-12.json` (laba rugi sebelum JT, sesudah JT, sesudah pembalik), dan `persiapan.json`.

### 5.1 Persiapan data fase S (persetujuan Rizki 25 November 2026: "saya setujui semua uji")

Seluruh perubahan tercatat di `persiapan.json` (25 entri: waktu, aktor, aksi, sasaran, sebelum, sesudah):

| Langkah | Hasil di DB dev Rizki |
| --- | --- |
| Izin sementara `Journal : Approve` dan `AccountingPeriod : Approve` untuk Logistik dan Gudang × Staff Logistik (akun penyetuju uji), supaya ada orang kedua sesuai `ACC-DEC-016` | Diberikan di awal setiap run dan **dipulihkan** di blok `finally`: kembali 6 izin semula, kedua izin dicabut (terverifikasi GET sesudahnya) |
| Periode tahun buku **2020** dibangkitkan (`periods/generate`) | 12 periode baru. Dipilih 2020, bukan 2025 seperti usulan semula: laba rugi 2020 membandingkan dengan 2019 yang berisi data uji, sehingga jalur pembanding teruji dengan angka bukan nol |
| Tiga jurnal uji 2019 disahkan | JU/2019/12/00001 (Kas 1002 D / 4-1003 K 500.000), JU/2019/12/00002 (5-1001 D 200.000 unit CC-MMC-00001 / Kas K), JU/2019/11/00001 (4-1002 D / Kas K 30.000 — pendapatan bersaldo debit) |
| 12 periode 2019 ditutup sementara (ajukan superadmin, setujui akun penyetuju uji) | Status 2019-01..12 `SoftClosed` |
| Tutup tahun 2019 | JT/2019/12/00001 disusun, disetujui, disahkan |
| JT dibalik penuh | JB/2019/12/00001 (31 Des 2019) disahkan |

**Keadaan DB sesudahnya.** Tahun buku 2019 kini berisi data uji, dan efektif **belum ditutup** karena JT-nya sudah dibalik. Periode 2019 tetap tutup sementara. Akibatnya:

- Neraca saldo 2026 kini membawa saldo akun Pendapatan/Beban 2019 di saldo awalnya. Uji I3 (`F6`) sebaiknya tidak diulang tanpa memperhitungkannya.
- Neraca `BE-ACC-FIN-008-03` justru mendapat data siap pakai untuk `F4`, yaitu baris "Laba Tahun Lalu Belum Ditutup".
- Angka September 2026 tidak tersentuh.

## 6. Acceptance criteria

| Kriteria | Status | Rencana bukti runtime |
| --- | --- | --- |
| AC-1 Dua bagian, `NetIncome` = Pendapatan − Beban tiap kolom | `RUNTIME VERIFIED` | I1; J2 (300.000 / 270.000) |
| AC-2 YTD periode terakhir = saldo neraca saldo (`F6`) | `RUNTIME VERIFIED` | I3: YTD 2026-12 per akun = saldo akhir neraca saldo 2026-12 (sebelum data 2019 ditambahkan) |
| AC-3 `JT` dan pembaliknya dikecualikan (`F2`, `F3`) | `RUNTIME VERIFIED` | J3: sesudah JT/2019/12/00001 disahkan, laba rugi 2019-12 identik di setiap baris dan kolom, sementara buku besar 4-1003 per 31 Des 2019 menjadi 0. J5: sesudah JB/2019/12/00001 disahkan, tetap identik, dan buku besar kembali −500.000. J4: YTD pembanding 2020-12 = 270.000 (JT tidak ikut) |
| AC-4 Pembanding tidak ada → `null` + alasan (`F7`); pembanding ada → angka dan selisih | `RUNTIME VERIFIED` | I2 ("Periode September 2025 belum ada di sistem."); J4 (2020-12 lawan 2019-12: pembanding 300.000, selisih −300.000; YTD pembanding 270.000, selisih −270.000) |
| AC-5 Akun kontra negatif dan mengurangi subtotal (`F8`) | `RUNTIME VERIFIED` | J1: 4-1002 YTD −30.000; induk 4-1000 YTD 470.000 = 500.000 − 30.000 |
| AC-6 Akun nol disembunyikan/ditampilkan (`F9`) | `RUNTIME VERIFIED` | I4: `true` 14/14 akun, urutan pra-order, subtotal = jumlah anak. I5: `false` 3 baris, laba bersih sama |
| AC-7 Unit biaya + `CostCenterNotice` (`F11`) | `RUNTIME VERIFIED` | I6: 4001 unit CC-MMC-00001 = 1.000.000 = buku besar unit; I7 unit asing → `400` |
| AC-8 Hanya `Posted` (`F1`) | `RUNTIME VERIFIED` | `06-jajak-draft`: jurnal Ditolak dan Diajukan yang menyentuh 4001 tidak ikut |
| AC-9 Log baca dan unduh tanpa nominal (`F17`) | `RUNTIME VERIFIED` | I15: 8 pembacaan = 8 log `FinancialStatement.IncomeStatement`; 3 unduhan = 3 log `GeneralLedger.Export`; 2 × `403` tercatat `AccessDenied`; nol nominal |
| AC-10 CSV = JSON termasuk subtotal, total, laba bersih | `RUNTIME VERIFIED` | I9: badan CSV identik baris per baris dengan JSON; I10: nama `laba-rugi_2026-09_CC-MMC-00001.csv`, baris Catatan, unduhan gagal → `404` JSON |
| Negatif `VAL-GL-04`/`05`, tanpa hak | `RUNTIME VERIFIED` | I8 (`2026/11` dan kosong → `400`, `2099-01` → `404`); I11 (`403` pada keduanya) |
| Regresi neraca saldo dan buku besar | `RUNTIME VERIFIED` | I12 |

**DoD.** AC-1..AC-10 terbukti; laporan ditulis; roadmap dan traceability diperbarui → ✅. `F20` (Excel komputer rumah sakit) tetap skenario UAT.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan, tidak diubah | (1) `TrialBalanceQuery.PeriodCode` bertipe `string` non-nullable, sehingga `PeriodCode` yang tidak dikirim ditolak framework dengan ProblemDetails, bukan pesan "Periode akuntansi tidak ditemukan.". Di luar lingkup. (2) `CariMenurutKodeAsync` hanya mencari jenis jurnal **aktif**; bila jenis `JT` dinonaktifkan sesudah ada jurnal `JT`, jurnal itu ikut terhitung lagi. Mengikuti desain bagian 25.6 |
| Risiko | Rendah. Terjemahan EF untuk `Any` bertingkat (pengecualian pembalik `JT`) dan `Contains` terbukti di runtime (J3, J5) |
| Status Git | `M` `GeneralLedgerController.cs`, `AccGeneralLedgerService.Export.cs`; `??` `FinancialStatementDtos.cs`, `FinancialStatementRowKind.cs`, `AccGeneralLedgerService.FinancialStatement.cs`; laporan ini |
| Temuan uji | Akun beban wajib menyebut unit biaya saat jurnal dibuat — aturan yang ada, bukan cacat; dicatat karena menjelaskan run 1 |
| Langkah berikutnya | `FE-ACC-FIN-008-02` (layar Laba Rugi), lalu `BE-ACC-FIN-008-03` (neraca) |
