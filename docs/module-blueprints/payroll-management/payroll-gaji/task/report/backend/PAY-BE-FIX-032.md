# PAY-BE-FIX-032 — Pengesahan bersyarat (administratif, PPh belum final) dan ringkasan pajak untuk Finance (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `8995f533` (working tree belum di-commit). Frontend: `yasmina`, HEAD `17d75c43d`.

`dotnet build`: **NOT RUN**. `MIGRATION`: tidak ada (metadata pada `ValidationSummaryJson`). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis FE (`payroll-conditional-approval.test.mjs`, 11 lulus). Preflight: Area Corporate, PayrollManagement (`Prl`), `TOUCHED LEGACY`, tanpa entity baru.

## 1. Keputusan desain: strategi A (metadata, status TETAP Submitted)
Dievaluasi dua strategi:
- **A (dipilih)**: pengesahan bersyarat = penanda `ConditionalApproval` pada `ValidationSummaryJson`; `RunStatus` tetap `Submitted`.
- B (status baru) ditolak: konsumen status berserakan. Pemetaan source: `PayrollFinanceHandoffService`, `PayrollPaymentService`, `PayrollPayslipService`, `PayrollReportService`, `PayrollDeductionsIngestionService`, `PayrollEarningsCalculationService`, `PayrollOvertimeAndShiftIngestionService`, `PayrollCalculationService.AccumulatedRunStatuses` (akumulasi PPh YTD) semuanya membaca string status (`Approved`, `Paid`, dll.) atau `IsFinalized`. Status baru berisiko diperlakukan final oleh salah satunya; strategi A membuat seluruh konsumen **tidak berubah** (tes statis memastikan tidak satu pun menyebut pengesahan bersyarat). Status `Approved` + flag ditolak karena tiap konsumen Approved akan menganggapnya final.

Konsekuensi: pengesahan bersyarat adalah persetujuan **administratif**. Status run, `IsLocked`, `ApprovedAt`, snapshot, `PaymentAmount`, konfirmasi cicilan ke Finance (`ConfirmRunAsync`), instruksi pembayaran, slip, dan laporan PPh resmi tidak disentuh.

## 2. Perubahan
- `POST {id}/approve` menerima `PayrollRunApproveRequest` (`ApprovalMode` Normal bawaan | Conditional, `Reason`, `AcknowledgeProvisionalTax`, `ExpectedRowVersion`); izin `PayrollRun:Approve` sama; mode tak dikenal = 400; tanpa badan = pengesahan normal (perilaku lama). Tidak ada endpoint kedua.
- `ApproveConditionallyAsync`: alasan dan pernyataan wajib; idempoten (sudah bersyarat pada siklus ini → kembalikan apa adanya, sebelum RowVersion); RowVersion; transaksi serializable dengan advisory lock; kelayakan lewat `BuildConditionalApprovalReasons`: status Submitted, penanda review-only ada, `EvaluateTaxReview` = **Eligible** (kode terstruktur, tanpa teks), tanpa pegawai bermasalah, sumber kompensasi sah, estimasi tersedia. Menyimpan `ConditionalApprovalMarker` (referensi `CA-{RunNumber}-{n}`, aktor, waktu, alasan, jumlah terdampak, **PPh valid terpisah dari estimasi**, versi kebijakan, bulan belum lengkap, `FollowUpRequired = true`, `ReconciliationStatus = PENDING_HISTORY_COMPLETION`), baris riwayat `ApproveConditionally` (status `Conditional`), dan audit `PayrollRun.ApprovedConditionally` (beda dari pengesahan normal).
- **Pengesahan final tetap ditolak**: `EnsureTaxFinalAsync` tidak diubah (penanda review-only tetap ada); `Return` melepas kedua penanda (jejak abadi di riwayat/audit); `Calculate` menimpa semuanya.
- **Status PPh per pegawai** (`ClassifyEmployeeTaxStatus`, dari snapshot): `FINAL_VALID` (CALCULATED/VALID_ZERO), `PROVISIONAL_ESTIMATE`, `INCOMPLETE_NO_ESTIMATE` (tanpa snapshot, ERROR, atau penahan lain; BUKAN Rp0), `NOT_APPLICABLE` (EXEMPT/NOT_APPLICABLE). `OfficialTaxAmount` null bila belum final; estimasi tidak pernah masuk `TotalTax`/`EmployeeAmount`.
- **Detail run** aditif: `CanApproveConditionally`, `ApproveConditionallyBlockReasons`, `TaxApprovalStatus`, `ConditionalApproval`, per penerima `TaxFinalityStatus`/`OfficialTaxAmount`, `ProvisionalTax.ConfirmedTaxTotal`; daftar: `IsConditionallyApproved`, `ConditionalApprovalReference`; `availableActions` memuat `ApproveConditionally` hanya bila layak. `CanApproveFinal` tetap false.
- **Ringkasan PPh untuk Finance**: `GET {id}/tax-summary` (izin baru `PayrollRun:ReadTaxSummary`, tidak diberikan otomatis): run, periode, per pegawai (identitas, tahun/masa, pasal, mode, status, `ConfirmedTaxAmount` hanya bila valid, `ProvisionalTaxAmount` terpisah, metode, versi kebijakan, bulan belum lengkap), referensi pengesahan bersyarat, status rekonsiliasi, `TaxFinalStatus`, `UsageMode = INFORMATIONAL_ONLY`, `IsPostingEligible = false`. **Baca-saja; tidak menulis ke Finance, tidak ada pengiriman.**

## 3. Integrasi Finance, pembayaran, cicilan, laporan
| Butir | Status |
| --- | --- |
| Pengesahan bersyarat tidak mengonfirmasi cicilan, tidak membekukan NetPay, tidak membuat instruksi, tidak masuk laporan PPh | VERIFIED STATIC (status tetap Submitted; konsumen tak tersentuh) |
| Consumer Finance penerima data pajak Payroll | **BLOCKED**: tidak ada yang terverifikasi; `FinPaymentDeduction` (PPH21) tidak dipakai (bukan kontrak Payroll) |
| Posting kewajiban PPh/jurnal | **BLOCKED / NEEDS CONFIRMATION**: kontrak Finance/Accounting belum disepakati; endpoint hanya informatif |
| Outbox/idempotency key terpersist, status penerimaan | **NEEDS CONFIRMATION**: butuh skema baru; tidak dibuat |
| Pembayaran sebelum PPh final | **NEEDS CONFIRMATION** (keputusan owner Finance/Payroll/Tax); tidak diaktifkan |

## 4. Rekonsiliasi dan koreksi
Jalur yang ada: lengkapi Saldo Awal PPh (disetujui) → Return → hitung ulang (PPh final dihitung) → ajukan normal → Approve. Run bersyarat belum final sehingga tidak ada pembayaran/potongan Finance yang harus dikoreksi. **Mekanisme selisih estimasi vs final setelah ada pembayaran, serta koreksi pajak pada run yang sudah Approved, belum ada**; karena itu pengesahan finansial/pembayaran dini tidak diaktifkan.

## 5. File
`Controllers/PayrollRunsController.cs`, `DTOs/PayrollRunDtos.cs`, `Services/PayrollRunService.cs`, `Services/PayrollTaxPolicyEngine.cs`. Keseimbangan kurung lulus.

## 6. Skenario (27 acuan) — statis
Tampilan/popup 01-08: lihat laporan FE. 09: NO CHANGE (Submit review-only). 10: NO CHANGE (Approve normal utuh). 11-12: VERIFIED STATIC (eligible hanya `Eligible`; blocker lain ditolak). 13-15: VERIFIED STATIC (estimasi terpisah; valid dipertahankan; tidak tersedia = null/INCOMPLETE). 16-17: IMPLEMENTED (`tax-summary`); tanpa jurnal. 18-19: VERIFIED STATIC. 20: VERIFIED STATIC (konsumen laporan tak berubah). 21: VERIFIED STATIC (audit + riwayat). 22: VERIFIED STATIC (idempoten). 23-24: VERIFIED STATIC (izin Approve; RowVersion). 25: NO CHANGE. 26: dijelaskan di bagian 4 (celah diakui). 27: VERIFIED (tidak diklaim). Eksekusi runtime semua: **NOT RUN**.

## 7. Risiko dan keputusan owner terbuka
- Belum dibuild; belum dijalankan terhadap database.
- Keputusan tertulis owner Payroll/Finance/Tax atas pengesahan bersyarat (saya mengimplementasikan versi administratif yang aman); apakah juga otorisasi pembayaran; aturan pengakuan PPh valid vs estimasi di Finance; metode penyelesaian selisih; kontrak penerimaan data pajak parsial; dasar kepatuhan pajak.
- Pengesahan bersyarat hanya untuk run yang telah diajukan review-only (estimasi tersedia). Run dengan estimasi tidak tersedia tidak layak (INCOMPLETE_NO_ESTIMATE pada pegawai terdampak menahan).
- Izin `ReadTaxSummary` baru muncul di Akses Role; perlu dicentang admin.
