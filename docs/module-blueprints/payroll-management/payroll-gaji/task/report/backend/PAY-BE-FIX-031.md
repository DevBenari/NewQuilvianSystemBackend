# PAY-BE-FIX-031 — Kelayakan review-only berbasis klasifikasi terstruktur dan deteksi snapshot lama (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `42798e42` (working tree belum di-commit). Frontend: `yasmina`, HEAD `323330779`. Contoh run: PR-202612-E08FC5 (Desember 2026).

`dotnet build`: **NOT RUN**. `MIGRATION`: tidak ada (metadata pada `ValidationSummaryJson`). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis FE (`payroll-review-recalculation.test.mjs`, 8 lulus). Preflight: Area Corporate, PayrollManagement (`Prl`), `TOUCHED LEGACY`, tanpa entity baru.

## 1. Akar masalah (dari source; data runtime belum diperiksa)
Run dihitung 24 Nov 11.04, sebelum PAY-BE-FIX-030 ada. Snapshot pajak penerima pada run itu **tidak memuat `BlockCode`/`Provisional`**, sehingga `IsTaxHistoryBlocked` = false → daftar penahan histori kosong → `CanSubmitForReview` false → `availableActions` tanpa `SubmitForReview` → FE menampilkan "Belum Siap Diajukan". Status penyebab: **hipotesis A (snapshot lama) terkuat berdasarkan waktu dan source; BELUM TERBUKTI** tanpa data (hipotesis G, kode localhost belum memakai revisi terbaru, juga belum dapat dikesampingkan; lihat langkah UAT). B-F tidak terbukti dari source.

Kelemahan kedua yang ditemukan: kelayakan menggunakan `validation.Blockers.Except(taxHistoryTexts)`, pembandingan teks berawalan nama; rentan terhadap perubahan format pesan, nama ganda, atau isu yang berubah. Dan tiga tempat (detail, availableActions, Submit) memakai potongan logika sendiri-sendiri.

## 2. Perbaikan
- **Metadata klasifikasi saat hitung** (`TaxReviewMeta(version, taxHistoryBlockerCount, otherBlockerCount)` pada `ValidationSummaryJson`): `PayrollCalculationService` menghitung `TaxHistoryBlockerCount` dari snapshot pajak ber-`BlockCode` TAX_YTD_HISTORY_INCOMPLETE (kode terstruktur mesin pajak); `PayrollRunService.CalculateAsync` menyimpan `OtherBlockerCount = jumlah penahan mentah − penahan histori`. Tanpa migration.
- **Satu penilai** `EvaluateTaxReview(validation, employees)` → `NotApplicable | RecalculationRequired | Blocked | Eligible`, dipakai GET detail (`DecisionBlockReasons`, `SubmitForReviewBlockReasons`, `CanSubmitForReview`, availableActions) dan `SubmitForReviewAsync`. Tanpa `Except` teks pada keputusan (hanya untuk menampilkan pesan galat).
- **Snapshot lama**: `RecalculationRequired` bila ada snapshot pajak `IsBlocking` tanpa `BlockCode` dengan bukti terstruktur masa pajak terakhir dan `ytdMissingMonths` terisi (data Detail yang memang sudah ditulis engine lama), atau bila run tidak punya `TaxReviewMeta`. Run yang tidak dapat dinilai tidak ditebak dan tidak dianggap lolos.
- **Penahan lain**: `OtherBlockerCount > 0` → `Blocked`; penahan non-pajak, pegawai bermasalah, input manual, sumber kompensasi tetap menahan lewat alasan yang sama.
- **Estimasi**: `UnavailableEstimateReasons` menyebut penerima dan alasan spesifik (PTKP/TER/kebijakan); dipakai GET dan Submit.
- **Kontrak aditif** (`PayrollRunDetailResponse`): `SubmitForReviewBlockCodes` (`RECALCULATION_REQUIRED_FOR_TAX_REVIEW`, `OTHER_BLOCKERS`, `ESTIMATE_UNAVAILABLE`, `OTHER_REASONS`), `ReviewRecalculationRequired`, `TaxReviewState`. Pesan wajib: "Payroll Run dihitung sebelum fitur PPh sementara tersedia. Hitung ulang Payroll Run untuk menyiapkan estimasi PPh dan pengajuan review." Hitung ulang memakai endpoint `POST {id}/calculate` yang sudah ada (izin `PayrollRun:Calculate`); GET tidak pernah menghitung ulang.
- **Dipertahankan**: `EnsureTaxFinalAsync`, `EnsureReadyToDecide`, `EnsureCompensationSourcesSettledAsync`, RowVersion, penanda ReviewOnly, riwayat `SubmitForReview`, audit, penahan Finance Handoff. Tidak ada endpoint baru.

## 3. File
`Services/PayrollCalculationService.cs`, `Services/PayrollRunService.cs`, `DTOs/PayrollRunDtos.cs`. Keseimbangan kurung lulus.

## 4. Matriks (25 skenario acuan) — statis
| # | Status |
| --- | --- |
| 01-02 | IMPLEMENTED, VERIFIED STATIC (BE: `RecalculationRequired` + kode + alasan) |
| 03-04 | IMPLEMENTED; **NOT RUN** (butuh hitung ulang nyata: BlockCode dan estimasi dari data/kebijakan di database) |
| 05-06 | IMPLEMENTED; NOT RUN (`CanSubmitForReview` dan `SubmitForReview` muncul setelah hitung ulang) |
| 07-10 | FE (lihat laporan FE); data PR-202612-E08FC5 NOT RUN |
| 11-12 | VERIFIED STATIC (endpoint Submit ReviewOnly existing; Calculated → Submitted); eksekusi NOT RUN |
| 13-14 | NO CHANGE REQUIRED dari FIX-030 (marker, badge); NOT RUN |
| 15 | VERIFIED STATIC (`EnsureTaxFinalAsync` pertama pada Approve) |
| 16 | VERIFIED STATIC (`Blocked` dan alasan lain tetap menahan) |
| 17 | NO CHANGE REQUIRED (Submit normal tidak disentuh) |
| 18-20 | VERIFIED STATIC (estimasi terpisah dari snapshot resmi; tanpa Finance pada Submit) |
| 21-22 | VERIFIED STATIC (alasan dan kode spesifik, kontrak BE=FE) |
| 23-25 | VERIFIED STATIC (lock FE, validasi ulang atomik BE, tanpa pencocokan teks, izin Submit/Calculate) |

## 5. UAT manual (wajib sebelum menyatakan kasus aktual berhasil)
1. Pastikan BE localhost dibangun dari HEAD ini (bukan biner lama); cek `GET .../payroll-runs/6ac72445-...`: `taxReviewState`, `submitForReviewBlockCodes`, `reviewRecalculationRequired`, `availableActions`.
2. Bila `RECALCULATION_REQUIRED_FOR_TAX_REVIEW`: di Perhitungan Gaji tekan Ajukan → "Hitung Ulang untuk Review" → konfirmasi.
3. Detail baru: Bayu Ramadhan dan dr. Rendy Pangallia memiliki `provisionalTax` (estimasi tersedia) dan `canSubmitForReview = true`.
4. Ajukan untuk Review (isi alasan, centang pernyataan) → run berstatus Submitted, tampil di Persetujuan Penggajian dengan "PPh Belum Final"; Approve ditolak dengan pesan PPh belum final.

## 6. Risiko dan keputusan terbuka
- **Belum dibuild; data run asli belum diperiksa.** Penyebab akhir belum terbukti.
- Estimasi masih bisa tidak tersedia (PTKP tidak ada pada kebijakan, tabel TER tidak mencakup, profil pajak tidak lengkap); pengajuan review lalu dihentikan dengan alasan per penerima.
- Snapshot sebelum FIX-015 tanpa `ytdMissingMonths` tidak dikenali sebagai kandidat; tetap "Belum Siap Diajukan" biasa (NEEDS CONFIRMATION bila ada run seperti itu).
- Hitung ulang menimpa snapshot aktif run Calculated (perilaku Calculate existing); run yang sudah disahkan tidak terpengaruh.
