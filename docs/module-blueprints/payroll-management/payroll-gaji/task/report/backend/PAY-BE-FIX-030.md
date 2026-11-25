# PAY-BE-FIX-030 — Pengajuan Payroll Run untuk Review dengan PPh Sementara (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `261bb431` (working tree belum di-commit). Frontend: branch `yasmina` (dikonfirmasi pemilik pada sesi ini; `yasmin` tidak ada di GitHub), HEAD `b05275ecd`.

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada, tanpa perubahan skema (penanda disimpan pada JSON existing). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis di sisi FE (`payroll-provisional-tax-review.test.mjs`, 10 lulus). Backend Governance Preflight: Area Corporate, modul PayrollManagement (`Prl`), `TOUCHED LEGACY`, tanpa entity baru.

## 1. Akar penahan (dari source)
Dua pegawai berada pada masa pajak terakhir (Desember). `PayrollCalculationService.BuildYtd` menandai histori Januari-November tidak lengkap, `PayrollTaxPolicyEngine.ComputePermanentEmployee` mengembalikan hasil blocked, `PayrollTaxInsuranceRules.AssessTax` menjadikannya `IsBlocking`, lalu pesan masuk `ValidationSummaryJson.Blockers`. `EnsureReadyToDecide` menolak, `BuildAvailableActions` tidak memuat Submit. Sebelumnya tidak ada kode klasifikasi: penyebab hanya berupa teks pesan.

## 2. Keputusan metode PPh sementara
- **Klasifikasi terstruktur** `TAX_YTD_HISTORY_INCOMPLETE` (`PayrollTaxBlockCodes`): diberikan mesin pajak hanya bila (a) tidak ada pemberi kerja sebelumnya, (b) akumulasi tidak lengkap, dan (c) `PayrollTaxYtd.HistoryGapOnly` (satu-satunya masalah adalah bulan yang belum tercakup; saldo awal di luar rentang, cakupan beririsan, dan run lama tanpa rincian akumulasi TIDAK dilonggarkan).
- **Estimasi** `EstimateProvisionalTerProxy`: TER bulanan atas penghasilan periode berjalan dari kebijakan tarif yang sama dan sudah disahkan (PTKP, tabel TER, surcharge NPWP, pembulatan dari kebijakan; tanpa angka bawaan). Hanya PPh 21 pegawai tetap. Bila PTKP atau tabel TER tidak mencakup: `IsAvailable = false` dengan alasan ("Estimasi Tidak Tersedia"), bukan Rp0. Label wajib "bukan rekonsiliasi PPh 21 akhir tahun" ada pada `PayrollProvisionalTaxEstimate.ExplanationText`; status `BELUM_FINAL`; metode `TER_MONTHLY_PROXY`.
- Estimasi disimpan **terpisah** pada `PayrollTaxSnapshot.Provisional` (properti aditif); `EmployeeAmount`, `ResultStatus` (ERROR), `IsBlocking`, `TotalTax`, dan `NetPay` resmi tidak diubah. `Provisional` tidak pernah dibaca sebagai akumulasi PPh tahun berjalan.

## 3. Perubahan mesin pajak dan kalkulasi
`PayrollTaxPolicyEngine.cs` (kode blokir, `HistoryGapOnly`, estimasi proksi, rekaman `PayrollProvisionalTaxEstimate`, `PayrollTaxFinality`); `PayrollTaxInsuranceRules.cs` (`BlockCode` dan `ProvisionalEstimate` pada assessment, `BlockCode`/`Provisional` pada snapshot; cabang estimasi tetap mengembalikan hasil ERROR); `PayrollCalculationService.cs` (`BuildYtd` menetapkan `HistoryGapOnly`). Rumus resmi tidak diubah; histori kosong tidak diterima sebagai lengkap.

## 4. Kontrak DTO
Aditif pada `PayrollRunDtos.cs`: daftar (`IsTaxProvisional`, `TaxFinalStatus`, `ProvisionalTaxTotal`); detail (`CanSubmitForReview`, `SubmitForReviewBlockReasons`, `CanApproveFinal`, `ReviewOnlySubmission`, `ProvisionalTax`); penerima (`ProvisionalTax`: jumlah, `EstimatedNetPay`, `OfficialNetPay`, metode, versi kebijakan, bulan belum lengkap); hasil pajak (`BlockCode`, `Provisional`). Permintaan `PayrollRunSubmitRequest` (`SubmissionMode`, `Reason`, `AcknowledgeProvisionalTax`). Field existing tidak berubah makna.

## 5. PayrollRunService dan validasi
- **Submit**: `POST {id}/submit` memakai izin `PayrollRun:Submit` yang sama, dengan `SubmissionMode = ReviewOnly` eksplisit (tanpa badan/mode = pengajuan normal; mode tidak dikenal = 400). Tidak ada endpoint kedua. `SubmitForReviewAsync` memeriksa ulang: status Calculated, tidak terkunci, input manual dan sumber kompensasi settled, tanpa pegawai bermasalah, **semua penahan selain histori PPh** harus nol (`validation.Blockers.Except(taxHistory)`), estimasi tersedia bagi semua penerima terdampak, alasan wajib, pernyataan "belum final" wajib, RowVersion, transaksi serializable. Hasil: `Submitted`, penanda `ReviewOnly` pada `ValidationSummaryJson` (tanpa migration), baris riwayat `SubmitForReview`, audit `PayrollRun.SubmittedForReview`. Blockers tidak dihapus. Finance, snapshot, kunci, dan nominal resmi tidak disentuh.
- **Approve**: `EnsureTaxFinalAsync` dipanggil lebih dulu (sebelum `EnsureReadyToDecide` dan `ConfirmRunAsync`); menolak bila ada penanda review-only atau snapshot penerima yang masih terklasifikasi/memuat estimasi, dengan pesan "Payroll Run sudah diajukan untuk review, tetapi belum dapat disahkan karena perhitungan PPh 21 belum final." Pemeriksaan lain (RowVersion, kompensasi, konfirmasi potongan Finance, idempotensi, pembekuan snapshot) utuh.
- **Return**: melepas penanda; jejak abadi tetap pada riwayat persetujuan dan audit. **Calculate** menulis ulang `ValidationSummaryJson` tanpa penanda dan membuat snapshot baru, sehingga estimasi lama tidak dipakai lagi.
- **AvailableActions**: `SubmitForReview` hanya bila layak dan Submit normal tidak tersedia; `Approve` tidak pernah bersama PPh sementara; `CanApproveFinal` terpisah dari izin.

## 6. Perlindungan Finance
Finance Handoff mensyaratkan `Approved` (`AllowsFinanceHandoff`), instruksi pembayaran mensyaratkan `Approved` + terkunci, slip mensyaratkan disahkan. Run review-only berstatus `Submitted`, sehingga ketiganya tetap ditolak; konfirmasi potongan ke Finance hanya di `ApproveAsync` (setelah guard). Penahan `PayrollFinanceHandoffGuard` (`PENDING_OWNER_DECISION`) tetap berlaku. Tidak ada perubahan saldo Finance pada Submit.

## 7. File
`Services/PayrollTaxPolicyEngine.cs`, `Services/PayrollTaxInsuranceRules.cs`, `Services/PayrollCalculationService.cs`, `Services/PayrollRunService.cs`, `DTOs/PayrollRunDtos.cs`, `Controllers/PayrollRunsController.cs`. Pemeriksaan keseimbangan kurung: lulus pada keenam berkas.

## 8. Status dan skenario (25) — statis
| # | Status |
| --- | --- |
| 01 | NO CHANGE REQUIRED (jalur normal utuh) |
| 02-03 | IMPLEMENTED, VERIFIED STATIC |
| 04-06 | IMPLEMENTED, VERIFIED STATIC (kebijakan, estimasi terpisah, gagal bukan Rp0) |
| 07 | VERIFIED STATIC (`Except` atas penahan terklasifikasi) |
| 08-10 | IMPLEMENTED (BE: aksi, penanda, `CanApproveFinal`); tampilan NOT RUN |
| 11 | VERIFIED STATIC (`EnsureTaxFinalAsync` pada `ApproveAsync`) |
| 12-15 | VERIFIED STATIC (status gating Finance/slip/pembayaran) |
| 16 | VERIFIED STATIC (Return) |
| 17-19 | NEEDS RUNTIME UAT (Opening Tax Balance dan hitung ulang tidak dijalankan) |
| 20-22 | NO CHANGE REQUIRED (idempotensi dan transaksi serializable existing) |
| 23-25 | NO CHANGE REQUIRED (permission, RowVersion, run historis tidak disentuh) |

## 9. Risiko dan keputusan terbuka
- **`dotnet build` belum dijalankan**; ekspresi record `with`, pola, dan serialisasi snapshot perlu build dan UAT.
- **Run Desember yang sudah dihitung sebelum perubahan ini harus DIHITUNG ULANG** agar snapshot memuat `BlockCode` dan estimasi; sebelum itu opsi review tidak muncul (gagal-tertutup).
- Gaji bersih resmi pada penerima terdampak dihitung dengan PPh resmi Rp0 (penahan); potongan cicilan dihitung terhadap angka itu. Estimasi gaji bersih hanya informasi; hitung ulang final memperbaikinya. NEEDS CONFIRMATION bila pemilik ingin batas potongan memakai estimasi.
- Penyimpanan Opening Tax Balance (tabel) belum diverifikasi di database lokal; bila fitur tidak tersedia, jalur penyelesaian histori tidak jalan.
- Jalur persetujuan lama (`PayrollApprovalWorkflowService`, status WaitingHrReview/WaitingFinalApproval) masih ada di balik `LegacyModernPayrollGateFilter`; tidak diubah (di luar cakupan). NEEDS CONFIRMATION apakah dinonaktifkan permanen.
- Pengesahan dengan nominal PPh manual tetap lewat `ApprovedManualAmount` (persetujuan tersendiri); estimasi tidak pernah menjadi nominal itu.
