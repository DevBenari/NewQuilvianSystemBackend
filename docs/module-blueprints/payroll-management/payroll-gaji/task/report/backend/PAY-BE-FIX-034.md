# PAY-BE-FIX-034 — Persetujuan administratif: histori PPh sebagai tindak lanjut, tahap persetujuan eksplisit (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `c073f8c1` (working tree belum di-commit). Frontend: PAY-FE-FIX-034. Lanjutan PAY-BE-FIX-032/033.

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada (tahap diturunkan dari `ValidationSummaryJson` yang sudah tersimpan). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis `payroll-continue-approval.test.mjs` (dari root workspace). Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`, tanpa entity/endpoint baru.

## Akar masalah (source)
Penahan histori PPh (`TAX_YTD_HISTORY_INCOMPLETE`) memang sudah dipisah dari penahan lain pada jalur persetujuan administratif: `SubmitForReviewAsync` mengabaikannya (EvaluateTaxReview Eligible) dan `ApproveConditionallyAsync` hanya menolak penahan non-PPh. Yang masih menyesatkan:
1. `RunStatus` tetap `Submitted` setelah persetujuan administratif, tanpa tahap eksplisit di daftar/detail, sehingga tampak "belum diproses".
2. Untuk run yang sudah disetujui, `ContinueApprovalBlockReasons` (PAY-BE-FIX-033) memuat ulang `DecisionBlockReasons` (pesan "PPh belum final", "Terdapat N penahan payroll (termasuk PPh)"), yaitu alasan histori PPh yang sama diulang dan disajikan sebagai penghalang.

## Perubahan
`DTOs/PayrollRunDtos.cs` (aditif): `PayrollRunApprovalStages` (NONE, AWAITING_DECISION, ADMINISTRATIVE_APPROVED_TAX_PENDING, FINAL_APPROVED) dan `ApprovalStage` pada `PayrollRunListItemResponse` (otomatis pada detail); komentar `ContinueApprovalBlockReasons` diperjelas.

`Services/PayrollRunService.cs`:
- `MapListItem`: `ApprovalStage` diturunkan dari status run + `ConditionalApproval` tersimpan (daftar dan detail memakai sumber sama). Tahap ADMINISTRATIVE_APPROVED_TAX_PENDING BUKAN `RunStatus` baru.
- `AddRunQualityReasons` (diekstrak): alasan kualitas run non-PPh (penerima bermasalah, belum terhitung, sumber berubah) dipakai bersama oleh `BuildDecisionBlockReasonsAsync` dan `ApplyApprovalNextStep` (satu sumber).
- `ApplyApprovalNextStep(run, detail, validation, taxReview)`: untuk run PPh sementara, `ContinueApprovalBlockReasons` kini hanya penahan NYATA, disusun dari data terstruktur (`TaxReview.State`/`OtherBlockerCount`, `ErrorEmployeeCount`, `ProcessedEmployeeCount`, `NeedsRecalculation`), tanpa pencocokan teks. Histori PPh tidak lagi masuk daftar penahan; ia ada pada `ContinueApprovalSteps` (tindak lanjut). `ApprovalNextStep` = RESOLVE_OTHER_BLOCKERS bila ada penahan nyata, selain itu RESOLVE_TAX_DATA.

## Klasifikasi penahan (sebelum → sesudah)
| Kategori | Sebelum | Sesudah |
|---|---|---|
| A. TAX_HISTORY_PENDING | `DecisionBlockReasons` + daftar penahan lanjutan | tindak lanjut finalisasi (`ContinueApprovalSteps`), bukan penahan persetujuan administratif |
| B. PAYROLL_INVALID | menahan | menahan (`ContinueApprovalBlockReasons` + `ApproveConditionallyBlockReasons`) |
| C. STALE_CALCULATION | menahan | menahan |
| D. PERMISSION_DENIED | `[AccessPermission("PayrollRun","Approve")]` | tidak berubah |
| E. TAX_NOT_FINAL_FOR_RELEASE | `EnsureTaxFinalAsync` pada `ApproveAsync` | tidak berubah |

## Batas administratif vs final (tidak diubah, diverifikasi)
`ApproveConditionallyAsync` tidak memanggil `ConfirmRunAsync`, tidak mengubah `RunStatus`, tidak mengunci, tidak mengisi `PaymentAmount`; idempoten (`ConditionalApproval is not null && Submitted → return`), memeriksa `PayrollVersionToken`. Konsumen hilir bergantung pada `RunStatus` Approved+ (`AllowsFinanceHandoff`, `AllowsPayslipGeneration`, `IsFinalized`); karena status tetap Submitted, tidak ada yang terbuka. `PayrollFinanceHandoffGuard` tidak disentuh.

## Keputusan bisnis yang BELUM diambil (stop condition)
Tidak dibuat status Approved/pengesahan gaji bersih final tanpa PPh sah, pelepasan transfer, konfirmasi AR, atau pemakaian estimasi sebagai PPh final. Bila pemilik Payroll/Finance/Tax menginginkannya, petakan dulu dasar nominal, kurang/lebih potong, pembatasan pembayaran, rekonsiliasi, dan pejabat berwenang.

## Risiko
Belum dibuild; run harus melewati dua langkah (Ajukan untuk Review oleh pemegang izin Submit, lalu Setujui Administratif oleh pemegang izin Approve; keduanya boleh orang yang sama); tanpa runtime UAT.
