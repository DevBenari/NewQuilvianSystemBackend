# PAY-BE-FIX-033 — Lanjutkan Pengesahan Payroll Run yang sudah disahkan bersyarat (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `c073f8c1` (working tree belum di-commit). Frontend: PAY-FE-FIX-033.

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada (DTO dan logika saja). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis `payroll-continue-approval.test.mjs` (6 lulus, dijalankan dari root workspace). Preflight: Area Corporate/PayrollManagement, prefix `Prl`, `TOUCHED LEGACY`, tanpa entity baru, tanpa endpoint baru.

## 1. Akar masalah (terbukti dari source)
- `BuildAvailableActions` menawarkan `Approve` hanya bila `ready && !IsTaxProvisional`, dan `ApproveConditionally` hanya bila `CanApproveConditionally`. Setelah pengesahan bersyarat, `BuildConditionalApprovalReasons` mengembalikan "sudah disahkan bersyarat" sehingga `CanApproveConditionally=false`. Jadi run Submitted + PPh sementara + sudah disahkan bersyarat tidak punya aksi persetujuan sama sekali. Perilaku ini benar (mencegah pengesahan bersyarat ganda dan pengesahan dengan PPh estimasi), tetapi tidak ada jalur lanjutan.
- Penting: pengesahan bersyarat hanya menulis `ConditionalApprovalMarker`; status tetap Submitted. Snapshot pajak run review-only dibekukan saat hitung dan `EnsureTaxFinalAsync` menolak run dengan penanda review-only atau snapshot terklasifikasi histori belum lengkap. **Satu-satunya jalur ke Approved** tetap: Kembalikan (melepas penanda review-only dan bersyarat) → hitung ulang dengan PPh sah → ajukan ulang → Sahkan. Karena itu "Lanjutkan Pengesahan" untuk run PPh sementara adalah panduan penyelesaian + jalur ke aksi existing, BUKAN jalan pintas Sahkan.

## 2. Perubahan
`DTOs/PayrollRunDtos.cs` (aditif, kompatibel): kelas konstanta `PayrollRunApprovalNextSteps` (NONE, APPROVE_FINAL, APPROVE_CONDITIONALLY, RESOLVE_TAX_DATA, RESOLVE_OTHER_BLOCKERS); field `PayrollRunDetailResponse`: `CanContinueApproval`, `ApprovalNextStep`, `TaxResolutionRequired`, `ContinueApprovalBlockReasons`, `ContinueApprovalSteps`.

`Services/PayrollRunService.cs`:
- `ApplyApprovalNextStep` (baru, murni informasi, tanpa tulis data): dihitung setelah `CanApproveFinal`, sebelum `BuildAvailableActions`. Run Submitted: siap final → APPROVE_FINAL; PPh sementara + sudah disahkan bersyarat → `CanContinueApproval=true`, RESOLVE_TAX_DATA, alasan spesifik (referensi pengesahan bersyarat + `DecisionBlockReasons`) dan empat langkah penyelesaian; PPh sementara belum bersyarat → APPROVE_CONDITIONALLY bila layak, selain itu RESOLVE_TAX_DATA; penahan lain → RESOLVE_OTHER_BLOCKERS.
- `BuildAvailableActions`: menambah `"ContinueApproval"` bila `CanContinueApproval`. `Approve` dan `ApproveConditionally` TIDAK berubah.

## 3. Tidak diubah (disengaja)
`EnsureTaxFinalAsync`, `EnsureReadyToDecide`, `EnsureCompensationSourcesSettledAsync`, `ApproveAsync` (konfirmasi cicilan Finance, snapshot beku, kunci, `ApprovedAt/ApprovedByUserId`), `ApproveConditionallyAsync` (idempoten, tanpa jejak ganda), `ReturnAsync`, `GetTaxSummaryAsync`, kontrak endpoint (`POST /{id}/approve` dengan `approvalMode`), `[AccessPermission]`.

## 4. Pengaju = pengesah
`ApproveAsync` tidak membandingkan `SubmittedByUserId` dengan aktor (komentar keputusan 24 Nov 2026 tetap); `RequireDifferentApprover=false`. Temuan (tidak diubah, di luar wewenang): `PayrollApprovalWorkflowService` (jalur lama `PayrollRunModernController`, status `WaitingHrReview`/`WaitingFinalApproval`) masih menolak pengaju = pereview (PAY-B4, VR-PAY-08). Jalur itu bukan yang dipakai halaman `/payroll/payroll-approvals` (memakai `PayrollRunsController`). Perlu keputusan bila jalur lama masih dipakai.

## 5. Risiko / keputusan
- Belum dibuild; kompilasi perlu build.
- Jalur B (PPh manual disetujui) tetap lewat konfigurasi `ApprovedManualAmount` yang sudah ada pada profil pajak; tidak ada jalur final dengan estimasi (NEEDS CONFIRMATION bila pemilik Payroll/Finance/Tax ingin pengecualian: dasar nominal, kurang/lebih potong, rekonsiliasi, pembatasan pembayaran).
- Backend tidak dapat menyatakan "histori sudah lengkap" tanpa menghitung ulang; karena itu langkah tetap dikembalikan → hitung ulang.
