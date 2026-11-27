# PAY-BE-FIX-038 — Langkah lanjutan setelah Saldo Awal PPh disetujui (backend)

Tanggal: 26 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `10563ac5` (working tree belum di-commit). Frontend: PAY-FE-FIX-038.

`dotnet build`: **NOT RUN**. `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat `payroll-continue-approval.test.mjs`. Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`.

## Akar masalah
Modal "Lanjutkan Pengesahan" selalu memuat langkah statis "Lengkapi Saldo Awal PPh", walau Saldo Awal untuk bulan yang kurang sudah disetujui. Run itu masih PPh sementara karena snapshot-nya dibekukan saat dihitung; saldo awal baru terbaca mesin hitung setelah run dikembalikan dan dihitung ulang.

## Perubahan
`DTOs/PayrollRunDtos.cs` (aditif): `TaxHistoryResolved`; konstanta `PayrollRunApprovalNextSteps.RecalculateAfterTaxHistory`.
`Services/PayrollRunService.cs`: `IsTaxHistoryResolvedAsync` (murni baca): benar hanya bila SEMUA penerima dengan penahan histori PPh punya bulan yang kurang (`ytdMissingMonths`) yang seluruhnya tercakup Saldo Awal berstatus APPROVED, sumber SAME_EMPLOYER_PRE_CUTOVER (sama dengan sumber mesin hitung), pada WorkforceProfileId dan tahun pajak yang sama; konservatif (data bulan kosong, tabel belum ada, atau ada yang belum tercakup = false). Dipanggil hanya untuk run Submitted dengan PPh sementara. `ApplyApprovalNextStep`: bila resolved, `ApprovalNextStep` = RECALCULATE_AFTER_TAX_HISTORY (jika tak ada penahan nyata) dan `ContinueApprovalSteps` = Kembalikan → Hitung Ulang → Ajukan → Sahkan.

Tidak diubah: `EnsureTaxFinalAsync`, `EnsureReadyToDecide`, `ApproveAsync`, snapshot, saldo awal, PPh estimasi tetap bukan PPh resmi.

## Risiko
Belum dibuild; tanpa UAT runtime. Penilaian memakai snapshot saat ini; kebenaran akhir tetap ditentukan perhitungan ulang. Run lama tanpa `ytdMissingMonths` pada snapshot dinilai belum resolved (langkah lama tetap tampil).
