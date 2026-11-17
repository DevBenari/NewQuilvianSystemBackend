# Laporan Perubahan Backend — `PAY-BE-22`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-22` |
| Judul | Laporan beban gaji dan PPh/BPJS |
| Slice | `MVP-1` — `EPIC PGJ-06` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/backend-roadmap.md`, kartu `PAY-BE-22` |
| Trace | FR-PGJ-050, 051, 052; PRL-DEC-27, 33, 41; `../../../contracts/api-contract.md` grup Report — tambahan; `../../../contracts/validation-matrix.md` `RPT-01` |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 0 (3 berkas), logika bisnis 1 (pengelompokan dan total), kontrak API 2 (dua endpoint baru), database 0 (murni baca), keamanan 1 (data nominal dijaga `ViewAmount`), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `PayrollReportService.cs`, `PayrollReportsController.cs`, `PayrollReportDtos.cs`; dokumen laporan dan penandaan status |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6722d7ca` (branch `Yasmina`), di atas perubahan `PAY-BE-14`–`21` yang belum di-commit |
| Tanggal | 2026-11-12 |
| Status | 🟡 — source selesai; empat kriteria terbukti statis dan lewat simulasi; **`dotnet build` tidak dijalankan atas instruksi pemilik**; uji API menunggu pemilik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` — service, controller, dan DTO laporan yang sudah ada (`PAY-BE-10`) |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service), `QBE-NAM-001`; arketipe transaksi read-only (tanpa aksi tulis) |
| Hak akses | `PayrollReport : ViewAmount` yang sudah ada; tidak ada action baru. Tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada diff; checker resmi `NOT RUN` (tanpa `pwsh`) |

## 1. Masalah yang diperbaiki

Payroll tidak membuat jurnal akuntansi (PRL-DEC-27), tetapi Finance tetap butuh dasar pembukuan:
berapa beban gaji tiap Cost Center per komponen, serta PPh 21 dan iuran BPJS yang dipotong.
Laporan rekap yang ada hanya memberi angka per run dan per komponen secara global, tanpa irisan
Cost Center × komponen, dan tanpa daftar PPh/BPJS per pegawai.

## 2. Proses bisnis

**Tujuan.** Kepala Payroll dan Finance membaca beban gaji per Cost Center dan laporan PPh/BPJS
dari run yang sudah disahkan (PRL-DEC-41, 33).

**Run yang dipakai:** status `Approved`, `SentToFinance`, `PartiallyPaid`, `Paid`, `Posted`, dan
`Closed`. Run yang belum disahkan, dibatalkan, atau dibalik tidak dipakai. Semua jenis run yang
sudah disahkan ikut, termasuk run restitusi.

**Laporan beban gaji** (`GET /reports/expense-summary`):

1. Petugas memilih periode atau run, dan bila mau satu Cost Center.
2. Sistem mengelompokkan baris komponen pegawai menurut Cost Center (dari snapshot run, dibekukan
   saat disahkan), kode, nama, dan jenis komponen.
3. Tiap baris memuat jumlah pegawai dan nominal.
4. Total penghasilan = baris berjenis `Earning`/`Benefit`. Total potongan = baris berjenis
   `Deduction`/`Tax`/`Insurance`, termasuk cicilan piutang. Keduanya dicocokkan dengan total
   pegawai run; bila berbeda, keterangan meminta petugas memeriksa layar Rekonsiliasi.

**Contoh** (data samaran, dua Cost Center):

| Cost Center | Komponen | Jenis | Pegawai | Nominal |
| --- | --- | --- | ---: | ---: |
| Farmasi | Gaji Pokok | Earning | 1 | Rp 5.000.000 |
| Farmasi | Tunjangan Transportasi | Earning | 1 | Rp 400.000 |
| Farmasi | PPh | Tax | 1 | Rp 300.000 |
| Farmasi | BPJS | Insurance | 1 | Rp 150.000 |
| Rawat Inap | Gaji Pokok | Earning | 2 | Rp 11.900.000 |
| Rawat Inap | Tunjangan Jabatan | Earning | 2 | Rp 1.500.000 |
| Rawat Inap | Tunjangan Transportasi | Earning | 1 | Rp 400.000 |
| Rawat Inap | Cicilan | Deduction | 1 | Rp 100.000 |
| Rawat Inap | PPh | Tax | 2 | Rp 650.000 |
| Rawat Inap | BPJS | Insurance | 2 | Rp 300.000 |

Total penghasilan Rp 19.200.000 = total bruto tiga pegawai; total potongan Rp 1.500.000 = total
potongan tiga pegawai.

**Laporan PPh dan BPJS** (`GET /reports/tax-contribution`): satu baris per pegawai per run yang
disahkan, berisi PPh 21 (`PAY_TAX`), iuran BPJS pegawai (`PAY_INSURANCE_EMPLOYEE`), sumber
`TETAPAN_PROFIL`, dan versi kebijakan dari snapshot run. Bisa dicari menurut nama atau nomor
pegawai, berhalaman, dan memuat total keseluruhan hasil saringan.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Belum ada run yang disahkan pada periode/run itu | 200, daftar kosong, `note` "Belum ada run yang disahkan pada periode ini." (`RPT-01`) |
| Periode dan run sama-sama kosong | 400 "Pilih periode penggajian atau payroll run." |
| Pengguna tanpa `PayrollReport : ViewAmount` | 403 |
| Pegawai tanpa Cost Center | Dikelompokkan sebagai "Tanpa Cost Center" |
| Komponen berjenis lain (mis. `EmployerContribution`) | Ditampilkan sebagai baris, tidak masuk total; dijelaskan di `note` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PayrollReportService.cs` (rekap dan rekonsiliasi yang ada), `PayrollReportsController.cs`,
`PayrollReportDtos.cs`, `PayrollInstallmentDeductionService.cs` (rumus `TotalDeduction` setelah
cicilan), `PayrollCalculationService.cs` (jenis komponen bruto), `PayrollBpjsCalculationService.cs`
(jenis `EmployerContribution`), `PrlPayrollRunEmployee.cs` (snapshot Cost Center), `PrlPayrollRun.cs`
(`PolicySnapshotJson`, `ApprovedAt`), `PayrollRunStatuses.cs`, `PagedResult.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/PayrollReportService.cs` | `GetExpenseSummaryAsync`, `GetTaxContributionAsync`, `LoadApprovedRunsAsync`, `ReadPolicyVersion`; himpunan status run disahkan dan jenis komponen penghasilan/potongan |
| `Areas/Corporate/PayrollManagement/Controllers/PayrollReportsController.cs` | `GET /expense-summary`, `GET /tax-contribution` (`PayrollReport : ViewAmount`) |
| `Areas/Corporate/PayrollManagement/DTOs/PayrollReportDtos.cs` | `PayrollExpenseSummaryQuery`, `PayrollExpenseSummaryRowResponse`, `PayrollExpenseSummaryResponse`, `PayrollTaxContributionQuery`, `PayrollTaxContributionRowResponse`, `PayrollTaxContributionPagedResult`, `PayrollTaxContributionSources` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru; endpoint lama tidak berubah. Delta di bagian 4 |
| Database | Tidak ada perubahan skema. Murni baca |
| Keamanan/Auth | Memakai `PayrollReport : ViewAmount` yang sudah ada. Pemegang hak rekonsiliasi otomatis dapat membuka kedua laporan |

## 4. Dokumentasi endpoint

### `[Tags("Corporate / Payroll Management / Report")]`

Base URL: `api/v1/corporate/payroll-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
|---|---|---|---|---|---|---|
| `GET` | `/expense-summary` | Beban gaji per Cost Center per komponen dari run yang disahkan (pengganti menu Jurnal) | `PayrollReport : ViewAmount` | query `payrollPeriodId` atau `payrollRunId`, `costCenterId` opsional | `PayrollExpenseSummaryResponse` | 200 (termasuk `RPT-01`), 400, 403 |
| `GET` | `/tax-contribution` | PPh 21 dan iuran BPJS pegawai beserta sumbernya | `PayrollReport : ViewAmount` | query `payrollPeriodId` atau `payrollRunId`, `search`, `pageNumber`, `pageSize` (≤ 200) | `PayrollTaxContributionPagedResult` | 200 (termasuk `RPT-01`), 400, 403 |

**Contoh respons `RPT-01`** (`/expense-summary`):

```json
{
  "periodCode": "2026-11",
  "runNumbers": [],
  "rows": [],
  "totalEarning": 0,
  "totalDeduction": 0,
  "note": "Belum ada run yang disahkan pada periode ini."
}
```

**Delta terhadap kontrak `PGJ-C1`** (penambahan):

| Delta | Alasan |
| --- | --- |
| `/tax-contribution` mengembalikan `PayrollTaxContributionPagedResult`, yaitu `PagedResult` baku ditambah `periodCode`, `totalTaxAmount`, `totalInsuranceEmployeeAmount`, dan `note` | `RPT-01` meminta keterangan bersama daftar kosong, sedangkan `PagedResult` tidak punya tempat untuk itu. Field halaman tetap sama |
| Baris PPh/BPJS menambah `runNumber` | Satu periode bisa punya lebih dari satu run yang disahkan (misalnya run restitusi); tanpa nomor run, baris pegawai yang sama tampak ganda |
| 400 bila periode dan run kosong | Kontrak hanya menulis 200 dan 403, tetapi menyatakan salah satu filter wajib |
| `note` pada `/expense-summary` juga memuat keterangan umum dan peringatan selisih | Kontrak tidak menetapkan isi `note` di luar `RPT-01` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan atas instruksi pemilik | `NOT RUN` | — |
| Kriteria 1: hanya run disahkan; jumlah baris = total run | Contoh 3 pegawai: penghasilan Rp 19.200.000 = bruto, potongan Rp 1.500.000 = total potongan; run `Calculated` dan `Reversed` tidak dipakai | `PASS` | Skrip simulasi |
| Kriteria 2: `PAY_TAX` dan `PAY_INSURANCE_EMPLOYEE` dengan sumber `TETAPAN_PROFIL` | Diambil dari baris komponen kedua kode itu; `Source` tetap `TETAPAN_PROFIL` | `PASS` | Review source |
| Kriteria 3: keduanya `PayrollReport : ViewAmount` | 4/4 endpoint controller laporan konsisten | `PASS` | Skrip pemeriksa |
| Kriteria 4: tanpa run disahkan | Kosong dengan `note` `RPT-01` di kedua laporan | `PASS` | Skrip simulasi dan review |
| Keseimbangan kurung | 0 selisih di 3 berkas | `PASS` | Skrip penghitung |
| QBE | Tanpa akses context di controller; murni baca | `PASS` | Review diff |
| Uji API dibandingkan rekap yang ada | Menunggu pemilik | `NOT RUN` | Langkah di bawah |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `REQUIRED` — dijalankan pemilik setelah build.

**Langkah uji untuk pemilik** (DB uji, data samaran):

1. Pilih run yang sudah disahkan. `GET /reports/expense-summary?payrollRunId=…`. Harapan:
   `totalEarning` = `grossPay` dan `totalDeduction` = `totalDeduction` pada `GET /reports/recap`
   untuk run yang sama; `note` tidak memuat peringatan selisih.
2. Ulangi dengan `costCenterId` salah satu Cost Center. Harapan: hanya baris Cost Center itu.
3. `GET /reports/tax-contribution?payrollRunId=…`. Harapan: satu baris per pegawai; jumlah
   `taxAmount` = total PPh run; `source` = `TETAPAN_PROFIL`; `policyVersion` terisi.
4. Pilih periode yang run-nya baru `Calculated`. Harapan: kedua laporan kosong dengan `RPT-01`.
5. Tanpa parameter periode dan run. Harapan: 400.
6. Akun tanpa `PayrollReport : ViewAmount`. Harapan: 403.

**Tidak dijalankan:** `dotnet build`, uji API, checker QBE resmi.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `GET /expense-summary` hanya dari run disahkan; jumlah baris = total penghasilan dan potongan run | Terpenuhi (simulasi); dibandingkan dengan rekap di DB uji menunggu | Bagian 5 |
| 2. `GET /tax-contribution` menampilkan `PAY_TAX` dan `PAY_INSURANCE_EMPLOYEE` dengan sumber `TETAPAN_PROFIL` | Terpenuhi (statis) | Bagian 5 |
| 3. Keduanya `PayrollReport : ViewAmount` | Terpenuhi (statis) | Bagian 5 |
| 4. Tanpa run disahkan: hasil kosong dengan keterangan `RPT-01` | Terpenuhi (simulasi dan statis) | Bagian 5 |
| DoD: build hijau | **Belum terpenuhi** | `NOT RUN` atas instruksi pemilik |
| DoD: QBE bersih | Terpenuhi untuk aturan yang dicek; checker resmi `NOT RUN` | Bagian 5 |
| DoD: kriteria terbukti | Lewat statis dan simulasi; uji API **belum** | Langkah uji menunggu pemilik |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan teknis | (1) Cost Center diambil dari snapshot run, sehingga laporan run lama tidak berubah bila master Cost Center diganti; master hanya dipakai bila snapshot kosong. (2) Komponen dengan kode sama tetapi nama berbeda (misalnya beberapa jenis tunjangan struktur) menjadi baris terpisah. (3) Selisih total tidak dikoreksi diam-diam, hanya dilaporkan di `note` |
| Masalah yang diketahui | (1) Run restitusi yang disahkan ikut dijumlah bersama run reguler di periode yang sama. Ini sesuai sifat beban, tetapi perlu dikonfirmasi Finance. (2) Komponen iuran pemberi kerja (`EmployerContribution`), bila ada, tampil sebagai baris tetapi tidak masuk total |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Task ini: `M` `PayrollReportService.cs`, `PayrollReportsController.cs`, `PayrollReportDtos.cs`; ditambah laporan dan penandaan roadmap. Perubahan `PAY-BE-14`–`21` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit |
| Langkah berikutnya | Pemilik: build dan langkah uji bagian 5. Task backend terakhir di roadmap: `PAY-BE-23` (rekap lembur per run) |
