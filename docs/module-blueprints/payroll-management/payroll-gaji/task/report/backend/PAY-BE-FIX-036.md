# PAY-BE-FIX-036 — Impor massal Saldo Awal PPh (backend)

Tanggal: 26 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `10563ac5` (working tree belum di-commit). Frontend: PAY-FE-FIX-036 (`856875e6`).

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis `payroll-opening-tax-balance-import.test.mjs` (root workspace). Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`, tanpa entity/kolom baru.

## Perubahan
- `Controllers/PayrollOpeningTaxBalancesController.cs`: `GET import/template`, `POST import/validate`, `POST import/commit` (semua `[AccessPermission("PayrollOpeningTaxBalance","Create")]`, tanpa AccessAction baru; layanan juga mewajibkan ViewAmount). Commit yang ditolak → 422 dengan hasil per baris di `errors`.
- `DTOs/PayrollOpeningTaxBalanceDtos.cs`: request baris mentah (string), hasil per baris/ringkasan, template, `OpeningTaxBalanceImportLimits` (1000 baris, 2 MB), `PayrollOpeningTaxBalanceImportException`.
- `Services/PayrollOpeningTaxBalanceService.cs`: kelas `partial`; `ValidateAsync` dipecah: `ValidateFields` (aturan bidang, kode galat + pesan lama) kini dipakai input manual DAN impor; pesan manual tidak berubah.
- `Services/PayrollOpeningTaxBalanceService.Import.cs` (baru): template; `ValidateImportAsync` (baca-saja); `CommitImportAsync` (`PayrollTransactionRunner` serializable + advisory lock yang sama dengan Create/Approve; validasi ULANG; satu `SaveChanges`; semua baris `DRAFT` lewat `Apply`; audit `OpeningTaxBalance.ImportCommit` dengan batch id, jumlah, id; tanpa nominal di log); resolusi identitas satu query ke `MstWorkforceProfiles` (ProfileCode, EmployeeCode/Number, DoctorCode/Number; tidak pernah nama; 0 → `WORKFORCE_NOT_FOUND`, >1 → `AMBIGUOUS_WORKFORCE`); penguraian bulan (`1-11`, `1;2;3`) dan nominal (kosong → `AMOUNT_REQUIRED`, bukan 0).

## Aturan per baris (kode)
WORKFORCE_REQUIRED/NOT_FOUND/AMBIGUOUS, INVALID_WORKFORCE_TYPE, TAX_YEAR_*, SOURCE_INVALID, MONTHS_*, AMOUNT_REQUIRED/INVALID/NEGATIVE/TOO_LARGE, DUPLICATE_IN_FILE, SOURCE_OVERLAP (dalam berkas atau dengan saldo APPROVED sumber lain), EXISTING_BALANCE (draf, pengajuan, atau disetujui pada kunci yang sama; tidak pernah menimpa), RUN_FINAL_OVERLAP (aturan yang sama dengan ApproveAsync).

## Keputusan implementasi (dapat diubah pemilik)
Maks. 1000 baris dan 2 MB per batch; semua empat nominal wajib eksplisit (0 ditulis, kosong ditolak; lebih ketat dari form manual yang membiarkan premi/pensiun kosong); tanpa permission Import tersendiri (memakai Create + ViewAmount); tanpa pengajuan massal; XLSX belum (CSV dulu); irisan payroll run final dan sumber lain ditolak sejak impor (lebih ketat dari Create manual, karena pasti gagal saat Approve).

## Idempotensi dan data
Tanpa migration, id batch hanya pada audit/respons. Permintaan commit yang diulang ditolak `EXISTING_BALANCE` (tidak ada duplikat). Data APPROVED, versi, payroll run, snapshot, dan Finance tidak disentuh; Submit/Approve tetap lewat alur lama (pemisahan pengaju-penyetuju tetap).

## Risiko
Belum dibuild (terjemahan LINQ pada `MstWorkforceProfiles` dan penggunaan pola `is`/record perlu build); tanpa UAT runtime; 1000 baris dalam satu transaksi serializable memegang advisory lock selama commit.
