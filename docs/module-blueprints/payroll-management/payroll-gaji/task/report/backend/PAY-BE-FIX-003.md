# Laporan Perubahan Backend — `PAY-BE-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-003` (ad-hoc, bukan kartu roadmap) |
| Judul | Rincian Penghasilan penerima Payroll Run: status hitung yang jujur, rincian penghasilan/potongan/pajak, SEMUA cicilan piutang dengan batas kumulatif |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap, traceability, dan kontrak `PGJ-C1` tidak disentuh (preseden `PAY-BE-FIX-001/002`) |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Finishing Popup Rincian Penghasilan dan Perhitungan Payroll"). Tanpa FR-PGJ/PRL-DEC bernomor — tidak dikarang |
| Contract version | Tidak tercakup `PGJ-C1`. Perubahan kontrak bersifat **additive** pada endpoint yang sudah ada; tidak ada endpoint baru |
| Klasifikasi | `HIGH` — perubahan mesin potongan cicilan yang menyentuh jalur pengesahan ke Finance |
| Task mode | `BACKEND` (Frontend lewat `PAY-FE-FIX-003`, target tulis terpisah) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan.** `dotnet build` **NOT RUN** (dilarang; kode belum pernah dikompilasi). Migration **tidak dibuat dan tidak diperlukan** (tanpa perubahan skema). Uji API, uji hitung, dan runtime **NOT RUN** |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry: `Corporate | PayrollManagement / Payroll | Prl | ACTIVE`). Tidak ada entity baru |
| Keberlakuan | `TOUCHED LEGACY` (`PayrollRunService`, `PayrollCalculationService`, `PayrollInstallmentDeductionService`, `PayrollRunDtos`) |
| QBE yang berlaku | `QBE-SVC-001` (controller tetap hanya memanggil service), `QBE-API-001` (tanpa endpoint baru; field additive), `QBE-PERM-001` (izin `ReadEmployeeDetail` existing, tanpa hardcode peran), `QBE-DTO-001`, `QBE-TXN-001` (hitung dan konfirmasi tetap di transaksi pemanggil), `QBE-LOG-001/AUD-001` |
| Checker QBE | **NOT RUN** pada task ini |
| Hak akses | `GET .../employees/{runEmployeeId}` tetap `[AccessAction("ReadEmployeeDetail")]`. Resource `PayrollRun` tidak punya aksi `ViewAmount`; menambahkannya akan mengubah visibilitas seluruh layar run, jadi **tidak ditambahkan** (keputusan pemilik, lihat bagian 6) |

---

## 1. Akar masalah

| # | Gejala di popup | Akar masalah (bukti source) |
| --- | --- | --- |
| 1 | Hampir semua nominal Rp0, status "Menunggu Hitung" | `GetEmployeeDetailAsync` hanya meneruskan kolom `PrlPayrollRunEmployee`. Sebelum run dihitung kolom itu bernilai 0 dan status penerima `Pending`; respons **tidak punya penanda** "belum dihitung" sehingga FE menampilkan 0 sebagai data. Penyebab data Bayu Ramadhan tidak diverifikasi ke database (tidak ada akses DB pada task ini) |
| 2 | Alasan penerima bermasalah tidak terlihat | Galat dan peringatan per penerima hanya hidup di memori `CalculateRunAsync`; yang disimpan hanya ringkasan run. Rincian tidak punya sumber alasan |
| 3 | Hanya satu cicilan terbaca | `FirstOrDefault` pada `CalculateRunAsync`, `ConfirmRunAsync`, dan `ReadLine` — penerima dengan dua angsuran sah pada satu periode hanya dihitung/dibaca satu |
| 4 | Potongan bisa berkurang tanpa konfirmasi Finance | Akibat nomor 3: angsuran kedua ikut mengurangi gaji bersih (komponen ada) tetapi tidak pernah dikonfirmasi ke Finance saat pengesahan |
| 5 | Batas potongan berpotensi dipakai berulang | Batas persen gaji bersih dihitung per angsuran, bukan kumulatif per penerima |
| 6 | Pajak/iuran Rp0 terbaca sebagai sah | Mesin run aktif memakai tetapan profil (`PrlPayroll.TaxAmount/InsuranceAmount`); nol pada keduanya (belum ditetapkan) tidak dibedakan dari pajak sah Rp0 |

## 2. Perubahan

Seluruhnya di `Areas/Corporate/PayrollManagement/`.

- **`DTOs/PayrollRunDtos.cs`** (additive): `PayrollRunInstallmentLineResponse` + `PlanId`, `PlanNumber`, `InstallmentNumber`, `AgreementStatus`, `AgreementApprovedAt`, `HasAgreementDocument`, `AuthorizationEvidenceStatus`, `ScheduledAmount`, `CarriedOverAmount`, `InstallmentStatus`, `RemainingUnpaidAmount`, `CapAvailableForInstallment`, `MaxAllowedSource`, `AllocationOrder`, `SyncStatus`, `Notes`. `PayrollRunComponentLineResponse` + `SourceId`, `CalculationMethod`. `PayrollRunEmployeeDetailResponse` + identitas/periode (`RunId`, `RunNumber`, `RunStatus`, `IsRunLocked`, `DepartmentName`, `PositionName`, `PayrollPeriodCode/Name`, `PeriodStartDate/EndDate`), status proses (`CalculationState`, `IsCalculated`, `CalculationFailed`, `CalculatedAt`, `SnapshotFrozenAt`, `ValidationIssues`, `CalculationWarnings`, `ConsistencyIssues`), ringkasan (`TotalFixedAllowance`, `TotalTransportAllowance`, `TotalAttendanceDeduction`, `TotalUnpaidLeaveDeduction`, `TotalInstallmentDeduction`, `TotalOtherDeduction`, `TotalEmployerInsuranceContribution`), pajak (`TaxCalculationSource`, `TaxCalculationStatus`, `TaxConfigurationIssues`), dan `Installments` (daftar). Field lama tidak diubah; `Installment` tetap berisi angsuran pertama.
- **`DTOs/PayrollInstallmentDeductionDtos.cs`**: ringkasan potongan + `Warnings`.
- **`Services/PayrollRunService.cs`**: `GetEmployeeDetailAsync` ditulis ulang **murni baca** (tanpa hitung, tanpa mutasi, tanpa tulis log perubahan). Status: `SnapshotFrozen` (penerima beku/final) › `Error` (run pernah dihitung dan penerima berstatus Error) › `NotCalculated` (run belum dihitung atau penerima belum sukses) › `DataIncomplete` (sukses tetapi profil pajak belum diisi atau PPh dan iuran sama-sama nol) › `Calculated`. Alasan/peringatan penerima dibaca dari `CalculationResultJson` ditambah penahan/peringatan run yang diawali nama penerima. Total ringkasan dijumlah dari komponen hasil hitung; **selisih** terhadap `GrossPay`, `TotalDeduction`, `NetPay` (toleransi 0,01) dilaporkan di `ConsistencyIssues`, tidak diperbaiki diam-diam. Seluruh angsuran dibaca lewat **satu** query (`LoadInstallmentFactsAsync`), bukan N+1. `CalculateAsync` kini meneruskan `deductions.Warnings` ke peringatan run.
- **`Services/PayrollCalculationService.cs`**: `CalculationResultJson` sukses memuat `warnings`; jalur gagal menyimpan `{ engine, policyVersion, calculatedAt, failed:true, errors, warnings }` agar alasan gagal tersedia untuk GET tanpa menghitung ulang. Pemakai lain `CalculationResultJson` tidak ada (dicek).
- **`Services/PayrollInstallmentDeductionService.cs`** (ditulis ulang): `CalculateRunAsync`, `ConfirmRunAsync`, dan `ReadLine(s)` memakai himpunan angsuran yang sama (`InstallmentInputs`: seluruh input angsuran, **dedupe per angsuran Finance**). Detail pada bagian 3.

## 3. Perjanjian cicilan dan mesin potongan

1. **Dasar sah potongan.** Angsuran hanya dipotong bila: ada di Finance, periode ≥ cutover otomatis, **perjanjian berstatus `DISETUJUI`**, dan angsuran belum `DIBATALKAN`/`TERBAYAR`. Selain itu menjadi **penahan** run (dan tidak dipotong). Saldo piutang positif **bukan** dasar potongan; `OutstandingAmount` tidak pernah dikurangi langsung.
2. **Beberapa cicilan.** Setiap angsuran punya komponen sendiri (`SourceId` = input angsuran). Hitung ulang memperbarui komponen yang sama (tanpa potongan ganda) dan membuang komponen yatim (`RemoveStaleComponents`). Konfirmasi saat pengesahan memproses **semua** angsuran, idempoten per angsuran dan periode (kunci advisory `FIN_INSTALLMENT_PAYROLL_RESULT_{id}` tetap).
3. **Batas kumulatif.** Batas = `bersih tersedia × persen / 100`, dibatasi `MaximumAmount` master dan bersih tersedia, dibulatkan ke bawah, dihitung **sekali** per penerima lalu dialokasikan berurutan. Gaji bersih tidak pernah negatif karena potongan cicilan; kekurangan tetap menjadi kewajiban (status `SEBAGIAN`/`GAGAL`, dibawa Finance ke angsuran berikutnya saat pengesahan).
4. **Hitung tidak menyentuh Finance.** Hanya `ConfirmRunAsync` (pengesahan) yang memanggil Finance.
5. **Bukti otorisasi.** `AgreementDocumentPath` nullable dan **tidak pernah divalidasi** saat persetujuan atau pengesahan; sistem tidak punya penanda bukti otorisasi terverifikasi. Respons hanya memuat `HasAgreementDocument` dan `AuthorizationEvidenceStatus` (`DocumentRecorded` | `DocumentMissing`) beserta catatan bahwa isi dan tanda tangan **belum diverifikasi**. Jalur berkas dan isi dokumen tidak dikirim. Tidak ada gerbang baru yang dikarang.

## 4. Kontrak API (additive)

`GET /api/v1/corporate/payroll-management/payroll-runs/{runId}/employees/{runEmployeeId}` — izin `PayrollRun : ReadEmployeeDetail` (tidak berubah). Kunci: `RunId` + `RunEmployeeId` (bukan `WorkforceProfileId`). Kode: `200`, `403`, `404` (`KeyNotFoundException`). Semua field baru bertipe nullable atau berdefault aman, sehingga klien lama tetap berjalan. Bentuk lengkap ada di `PayrollRunDtos.cs`; FE mengunci nama field lewat uji kontrak (`payroll-run-employee-detail.test.mjs`).

Tidak ada endpoint baru dan tidak ada mesin hitung kedua. Aksi hitung tetap `POST {id}/calculate` (`PayrollRunService.CalculateAsync → PayrollCalculationService.CalculateRunAsync → PayrollInstallmentDeductionService`).

## 5. Pajak dan iuran

Tidak ada rumus PPh 21 maupun BPJS baru. Nominal tetap dari tetapan profil; `TaxCalculationStatus` = `Configured` | `NotConfigured` (PPh dan iuran sama-sama nol → belum ditetapkan, **bukan** pajak sah Rp0) | `NotCalculated`. `TaxCalculationSource` menyebut asalnya. Kontribusi pemberi kerja hanya informasi dan **tidak** mengurangi gaji bersih (mesin run mengisinya 0).

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` / `dotnet restore` | **NOT RUN** (dilarang) |
| `dotnet ef` / migration | **NOT RUN** — tidak ada perubahan skema, tidak diperlukan |
| Automated test backend | `AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test (TEST_POLICY). Penjaga source dijalankan dari FE: `payroll-installment-deduction-guards.test.mjs` 6/6 lulus setelah satu asersi diperbarui (lihat risiko 7) |
| Tinjauan kompilasi lewat pembacaan | Nama konstanta, properti entity (`FinReceivableInstallment`, `FinReceivableInstallmentPlan`, `MstDeductionType`, `PrlPayrollRunEmployee`), dan tipe nullable dicocokkan ke source. **Belum dikompilasi**, jadi masih ada risiko galat kompilasi |
| Uji hitung / API / runtime | **NOT RUN** |

## 7. Keputusan yang dibutuhkan dan risiko

1. **Urutan alokasi antar-cicilan** (periode → tanggal persetujuan perjanjian → nomor angsuran) bersifat **sementara**. Dilaporkan sebagai peringatan run dan di `AllocationOrder`; belum kebijakan. **NEEDS BUSINESS DECISION.**
2. **Persen batas potongan.** Bila Master Jenis Potongan tidak menetapkan `MaxNetSalaryPercentage`, dipakai nilai bawaan 50% (`PayrollPolicyOptions`) yang **belum disahkan**; ditandai `MaxAllowedSource = PolicyDefault` dan peringatan run, **bukan penahan**. Pemilik dapat memutuskan menjadikannya penahan.
3. **Bukti otorisasi wajib terverifikasi** belum menjadi aturan sistem (butir 3.5). **NEEDS BUSINESS DECISION.**
4. **`ViewAmount` untuk `PayrollRun`** tidak ditambahkan (mengubah visibilitas seluruh layar run).
5. **Alur pembalikan** (`RequestDeductionReversal` dan persetujuannya) masih memakai `ReadLine` = angsuran **pertama**. Penerima dengan beberapa angsuran belum bisa dibalik per angsuran. Tidak diubah pada task ini; risiko terdokumentasi.
6. **`PayrollEarningsCalculationService`** (alur "Modern" lama di `HumanResource/PayrollManagement`) tidak disentuh.
7. Penjaga source `payroll-installment-deduction-guards.test.mjs` mengunci teks `status = PayrollResultStatuses.Gagal;`. Karena status kini diturunkan per angsuran dari sisa batas, asersi diganti ke bentuk ternary dengan maksud yang sama (Gagal bila sisa batas 0). Ini perubahan **penjaga**, bukan pelonggaran aturan.
8. **Data lama:** run yang dihitung sebelum task ini tidak punya `warnings` pada `CalculationResultJson`; rincian membacanya kosong (bukan galat) sampai run dihitung ulang.
9. `git status`: tidak ada yang di-stage atau di-commit. Perubahan tugas sebelumnya masih berada di working tree yang sama.
