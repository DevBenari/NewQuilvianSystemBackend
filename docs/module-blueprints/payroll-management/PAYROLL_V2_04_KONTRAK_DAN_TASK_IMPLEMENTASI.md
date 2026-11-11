# PAYROLL V2 — 04. Kontrak dan Task Implementasi

| Field | Nilai |
| --- | --- |
| Dokumen | `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` |
| Tanggal | 2026-11-04 |
| Acuan SHA | BE `b5fb6c251eafcd680ec35483732e3e72226e19f3` (+ perubahan lokal), FE `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` (+ perubahan lokal) |
| Label bukti | `REPOSITORY FACT`, `TARGET PROPOSAL`, `IMPLEMENTED`, `PARTIAL`, `BLOCKED`, `NEEDS CONFIRMATION` |
| Catatan penamaan (11 November 2026) | Nama entity `Trx*` dan `Wfp*` Payroll pada kolom bukti dan kontrak di dokumen ini (mis. `TrxPayrollApproval`, `UX_TrxPayrollPayment_LiveInstruction`) adalah nama saat task itu dikerjakan. Sejak `PAY-BE-12` dan `PAY-MIG-02` namanya `Prl*` (ganti awalan saja, mis. `PrlPayrollApproval`, `UX_PrlPayrollPayment_LiveInstruction`). Nama controller, DTO, rute, dan izin tidak berubah |
| Catatan roadmap | Tidak ada blueprint atau roadmap Payroll yang terdaftar di `docs/module-blueprints/`. Daftar task di bawah berasal dari prompt dan **menjadi register status** modul ini. Laporan per task ada di `task/report/backend/` dan `task/report/frontend/` di folder yang sama. |

## 1. Cara membaca tanda status

| Tanda | Arti di register ini |
| --- | --- |
| ✅ | Semua kriteria keberhasilan prompt terpetakan ke source yang ada, validasi statis dijalankan, laporan task ada. Runtime **belum** dibuktikan (build dan server dilarang pemilik). |
| 🟡 | Sebagian. Kriteria yang belum dipenuhi disebut di kolom terakhir. |
| ⛔ | Terblokir oleh hal di luar lingkup. Nama blokernya disebut. |

## 2. Register task

| ID | Layer | Pekerjaan | Tanda | Label | Bukti utama (path / simbol) | Yang belum / catatan |
| --- | --- | --- | :-: | --- | --- | --- |
| PAY-BE-01 | BE | Audit dependensi dan pindah bounded context Payroll | 🟡 | `PARTIAL` | `Areas/Corporate/PayrollManagement/{Controllers,Services,DTOs,Constants,Options}`; `Program.cs` (opsi + 11 layanan); `FinanceBillingIntakeService`, `FinanceReceivablePayrollSyncService` (using); tidak ada controller/DbSet/tabel ganda | Entity `Trx*`/`Wfp*`, konfigurasi EF, controller master dan profil belum dipindah fisik (fase 2, menunggu registry `QBE-MOD-002`) — disetujui pemilik sebagai langkah bertahap |
| PAY-FE-01 | FE | Modul navigasi Payroll mandiri | ✅ | `IMPLEMENTED` | `src/utils/menu-sidebar/menu-items.jsx` (modul `corporatePayroll`, 8 menu, 24 fitur); `next.config.js` (`getPayrollRouteRedirects`) | UI belum dijalankan di peramban |
| PAY-BE-02 | BE | Pisahkan master penggajian dari master HRD | 🟡 | `PARTIAL` | 10 controller master + 6 `Wfp*` + `WfpSalaryAssignmentController`: `[Route("api/v1/corporate/payroll-management/...")]` + `[Tags("Corporate / Payroll Management / ...")]`; FK tidak berubah | `moduleCode` Akses Role tetap `HUMAN_RESOURCE_*` (mengubahnya memutus grant yang sudah ada); folder fisik fase 2 |
| PAY-FE-02 | FE | Pindah halaman master data Payroll | ✅ | `IMPLEMENTED` | `src/app/payroll/master-data/<10 entitas>/`; `*-constants.jsx` (`routeBase`, `endpoint` alias); tes `payroll-module-contract` | Implementasi view/hook/slice masih di `hr/master-data/<entitas>` (fase 2, kosmetik) |
| PAY-BE-03 | BE | Pengelolaan profil gaji karyawan/dokter | 🟡 | `PARTIAL` | `PayrollCompensationService`, `PayrollCompensationsController` (daftar, detail, nominal berizin `ViewAmount`, rekening/NPWP tersamar, daftar masalah kesiapan); validasi tumpang tindih/persetujuan di `PayrollCalculationService` | Pembuatan dan persetujuan penetapan gaji tetap di `WfpSalaryAssignmentController` (HRD); pemisahan editor≠penyetuju penetapan gaji `NEEDS CONFIRMATION` |
| PAY-FE-03 | FE | Halaman Data Penghasilan Pegawai & Dokter | ✅ | `IMPLEMENTED` | `view/payroll/compensation/*` (Profil Penggajian + detail, Penetapan Gaji); `hooks/payroll/use-payroll-compensation-*.js`, `use-payroll-salary-assignments.js` | - |
| PAY-BE-04 | BE | Payroll Run dan kalkulasi penghasilan | ✅ | `IMPLEMENTED` | `PayrollCalculationService.CalculateRunAsync` (`EngineVersion = PAYROLL-CALC-V1`), `PayrollRunService.CalculateAsync`, `PayrollHrdImportService`; snapshot kebijakan `PolicySnapshotJson` | Rumus PPh/BPJS dan prorata/cuti `NEEDS CONFIRMATION` (memakai tetapan profil dan parameter terversi) |
| PAY-FE-04 | FE | Halaman Proses Penggajian | ✅ | `IMPLEMENTED` | `payroll-run-preparation-view.jsx`, `payroll-run-readiness-panel.jsx`, `payroll-run-list-view.jsx` (varian perhitungan), `payroll-run-detail-view.jsx` | - |
| PAY-BE-05 | BE | Approval, lock, reversal dan audit run | ✅ | `IMPLEMENTED` | `PayrollRunStatuses`, `PayrollRunService.{Submit,Return,Approve,Cancel,Close,RequestReversal,ApproveReversal,RejectReversal}Async`, `PayrollVersionToken`, `PayrollTransactionRunner`, `TrxPayrollApproval` | Approval berjenjang `NEEDS CONFIRMATION` (satu langkah + opsi pemisahan tugas) |
| PAY-FE-05 | FE | Halaman Persetujuan Penggajian | 🟡 | `PARTIAL` | `/payroll/payroll-approvals` + `payroll-run-detail-view.jsx` (penahan, peringatan, riwayat status dan alasan, Sahkan/Kembalikan dengan alasan) | "Perubahan" dibatasi pada riwayat dan total sekarang; BE tidak menyimpan perbandingan hasil hitung sebelumnya |
| PAY-BE-06 | BE | Integrasi piutang karyawan: jadwal dan realisasi | 🟡 | `PARTIAL` | `PayrollInstallmentDeductionService`, `PayrollEmployeeDeductionService` (riwayat, ringkasan, rekonsiliasi), kunci `FIN_INSTALLMENT_PAYROLL_RESULT_{id}`; uji `payroll-installment-deduction-guards` (6 lulus) | R1: satu angsuran per pegawai per run; titik pengakuan pelunasan diputuskan pilihan A (11 November 2026), pengecekan bayar lewat `PAY-BE-11` |
| PAY-FE-06 | FE | Halaman Potongan Cicilan Piutang & Restitusi | ✅ | `IMPLEMENTED` | `view/payroll/deduction/*`; `payroll-integration-service.js` (endpoint nyata) | - |
| PAY-BE-07 | BE | Integrasi hasil jasa medis yang sudah approved | ✅ | `IMPLEMENTED` | `PayrollMedicalFeeIntakeService`, `PayrollMedicalFeeIntakeController`; indeks `UX_TrxPayrollVariableInput_MedicalFeeClaim` | Jalur Payroll nonaktif bawaan; pajak jasa `NEEDS CONFIRMATION`; Medical Fee tidak diubah |
| PAY-FE-07 | FE | Halaman Remunerasi Dokter | ✅ | `IMPLEMENTED` | `view/payroll/medical-fee/*` | - |
| PAY-BE-08 | BE | Handoff pembayaran Payroll ke Finance | 🟡 | `PARTIAL` | `PayrollPaymentService.{Handoff,GetPayments,Acknowledge,Retry}Async`, `PayrollPaymentsController`; indeks `UX_TrxPayrollPayment_LiveInstruction` | Finance belum punya jenis pembayaran `PAYROLL` (⛔ sisi penerima di luar lingkup) |
| PAY-FE-08 | FE | Halaman Pembayaran Gaji & Honor | 🟡 | `PARTIAL` | `payroll-run-list-view.jsx` (varian pengiriman), `payroll-dispatch-modal.jsx`, `payroll-payment-history-view.jsx` | Status selain `Dijadwalkan` baru muncul setelah Finance mengirim ACK; layar konfirmasi manual dibuat oleh `PAY-FE-12` (keputusan pemilik 11 November 2026, belum ada koneksi bank); sebelumnya sengaja tidak ada (dipanggil Finance) |
| PAY-BE-09 | BE | Pembuatan, versi, publikasi slip gaji | ✅ | `PayrollPayslipService`, `PayrollPayslipDocuments` (PDF dari snapshot), `PayrollPayslipsController`, `PayrollMyPayslipsController` | Aturan akses slip diputuskan 11 November 2026: tetap setelah bayar, dengan konfirmasi pembayaran manual (`PAY-FE-12`) |
| PAY-FE-09 | FE | Halaman Slip Gaji | ✅ | `view/payroll/payslip/*` (admin, Slip Gaji Saya, detail + unduh) | - |
| PAY-BE-10 | BE | Rekap, jurnal dan rekonsiliasi | 🟡 | `PARTIAL` | `PayrollReportService.{GetRecap,GetReconciliation}Async`, `PayrollReportsController` | Jurnal ⛔: mapping COA dan kode peristiwa akuntansi payroll belum ada; rekonsiliasi menandai "Belum Tersedia" |
| PAY-FE-10 | FE | Laporan dan dashboard Payroll | ✅ | `view/payroll/report/*` (Rekap, Rekonsiliasi) | Dashboard grafis tidak dibuat; rekap berbentuk tabel dan kartu |
| PAY-FE-11 | FE | Fase 2 frontend: relokasi 10 entitas data induk Payroll ke `payroll/master-data` | 🟡 | `IMPLEMENTED` (statis) | 90 berkas dipindah dari `hr/master-data` (view, hook, konstanta, util, slice); 45 berkas diubah hanya jalur impornya; [laporan](task/report/frontend/PAY-FE-11.md) | Isi 90 berkas identik dengan `HEAD`, resolver impor, suite unit, dan ESLint sama dengan baseline; menunggu pemilik menjalankan `npm run build` dan membuka 10 layar |
| PAY-BE-11 | BE | Penanda sumber konfirmasi (`MANUAL`/`FINANCE_ACK`) dan status bayar gaji pada potongan piutang, ringkasan pembayaran, dan rekonsiliasi | 🟡 | `IMPLEMENTED` (statis) | `PayrollConfirmationSources`, `PayrollSalaryPaymentStatuses`, `PayrollSalaryPaymentStatusResolver`; `PayrollPaymentService.{Acknowledge,GetPayments}Async`, `PayrollRunService` (ringkasan), `PayrollEmployeeDeductionService`, `PayrollReportService` (`AR_DEDUCTION_PAYMENT`); tanpa migration; [laporan](task/report/backend/PAY-BE-11.md) | Source selesai dan lolos pemeriksaan statis; `dotnet build` dan runtime menunggu pemilik. Tidak ada perubahan kontrak ke Finance maupun titik pengakuan potongan |
| PAY-FE-12 | FE | Layar Konfirmasi Pembayaran Manual (memakai endpoint ACK yang ada) | 🟡 | `IMPLEMENTED` (statis) | `view/payroll/payment/payroll-manual-confirmation-modal.jsx`; aksi per baris dan massal pada Riwayat Pembayaran; `use-payroll-payment-history.js`; `payroll-payment-utils.js`; [laporan](task/report/frontend/PAY-FE-12.md) | 28 unit test baru lulus, suite dan ESLint sama dengan baseline, dialog dirender uji; menunggu pemilik menjalankan `npm run build` dan langkah uji a sampai h. Penanda sumber sementara: kunci `MANUAL-`; penanda tetap `MANUAL` menunggu `PAY-BE-11` |
| PAY-FE-13 | FE | Penanda status bayar gaji dan peringatan pada layar potongan piutang, riwayat pembayaran, dan detail run; konfirmasi manual mengirim `confirmationSource` | 🟡 | `IMPLEMENTED` (statis) | Kolom `Status Bayar Gaji` (`payroll-deduction-table-columns.jsx`), kartu dan peringatan (`payroll-employee-deduction-view.jsx`, `payroll-payment-history-view.jsx`, `payroll-run-detail-view.jsx`), pembangun `buildConfirmedDeductionPaymentNotices`; [laporan](task/report/frontend/PAY-FE-13.md) | 16 unit test baru lulus, suite, resolver, dan ESLint sama dengan baseline, kolom dan peringatan dirender uji; menunggu pemilik `dotnet build` backend (`PAY-BE-11`), `npm run build`, dan langkah uji a sampai i |
| PAY-BE-12 | BE | Fase 2 backend: registry `Prl`, pemindahan 19 entity, penggantian nama kelas | 🟡 | `IMPLEMENTED` (statis) | Baris registry `Prl` (ACTIVE); 19 model, 19 konfigurasi, 6 controller dan 6 DTO `Wfp*` dipindah ke `Areas/Corporate/PayrollManagement` dan `Repositories/Configurations/Corporate/PayrollManagement`; 784 penggantian nama di 84 berkas, 34 berkas diubah di tempat; [laporan](task/report/backend/PAY-BE-12.md) | Transformasi dibalik identik dengan cadangan pada 84 dari 84 berkas, 0 nama lama tersisa, atribut akses dan rute 6 controller identik, replikasi aturan kepemilikan QBE 19 dari 19 lulus, suite unit FE sama dengan baseline. **Pengerjaan ulang setelah CI QBE (11 November 2026):** 83 dari 87 pelanggaran berasal dari merge `dad` yang tidak membawa 39 commit integrasi, bukan dari fase 2; hasil merge yang benar dan dua pengecualian `QBE-CODE-002` bertenggat (sampai 2027-03-31) membuat port checker (tervalidasi 87/12 persis) menyatakan 0 pelanggaran memblokir, diff terhadap basis PR 136 berkas; lihat laporan bagian 8. **Belum:** hasil diterapkan ke working tree dan CI dijalankan ulang oleh pemilik, `dotnet build`, checker QBE resmi (`pwsh` tidak tersedia), uji layar HR dan Payroll. Aplikasi hasil build hanya cocok dengan database yang sudah menjalankan `PAY-MIG-02` |
| PAY-MIG-02 | BE schema | Migration `RenameTable` 19 tabel ke `Prl*` | 🟡 | `IMPLEMENTED` (statis) | `Migrations/20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.cs` + `.Designer.cs`; `ApplicationDbContextModelSnapshot.cs`; [laporan](task/report/backend/PAY-MIG-02.md) dengan skrip pra-cek dan pasca-cek | Seluruhnya `RENAME` (tanpa `DROP`/`CREATE`); simulasi pada 10.499 nama: 10.499 dari 10.499 sama dengan nama yang diharapkan model baru dan `Up` lalu `Down` kembali ke semula; **diulang setelah merge integrasi pada 10.786 nama (289 terdampak): 10.786 dari 10.786 cocok, `Up`/`Down` kembali**, termasuk FK tabel Payroll Modern milik integrasi ke `TrxPayrollRun` dan `TrxPayrollPayslip` (laporan bagian 9). **Belum:** dijalankan terhadap database (dilarang), `dotnet build`, `has-pending-model-changes`. Eksekusi, pra-cek, pasca-cek, uji `Down`/`Up` oleh pemilik |
| PAY-BE-13 | BE | Pindahkan alokasi nomor kebijakan dan transaksi tunjangan transport ke `NumberSeriesManagement`, lalu cabut dua pengecualian `QBE-CODE-002` | | `PLANNED` | `EXP-QBE-CODE-002-PRL-001` dan `-002` pada `docs/engineering/QBE_EXCEPTIONS.json` (berlaku sampai 2027-03-31); `WfpTransportAllowancePolicyController.GenerateCodeAsync`, `WfpTransportAllowanceTransactionController.GenerateTransactionNumberAsync` | Belum dikerjakan. Memerlukan seri nomor baru, kelanjutan nomor lama tanpa bentrok, dan kemungkinan seed atau migration (wewenang terpisah). Pengecualian hanya diperlukan selama dua controller itu masuk rentang diff PR; sesudah berada di branch basis, checker tidak menilainya lagi, tetapi setiap PR yang kelak menyentuh kedua berkas itu setelah 2027-03-31 kembali ditandai `QBE-CODE-002` |
| PAY-MIG-01 | BE schema | Migration, designer, snapshot manual | ✅ | `Migrations/20261104071700_AddPayrollIntegrityConstraints.cs` + `.Designer.cs`; `ApplicationDbContextModelSnapshot.cs`; 3 `*Configuration.cs` | Belum dijalankan terhadap database (dilarang); lihat `PAYROLL_V2_05` |
| PAY-DOC-01 | Docs | Dokumentasi matriks source, proses, kontrak, UAT | ✅ | lima dokumen `PAYROLL_V2_01..05` + laporan task | - |

Urutan dependensi (prompt): semua task FE menunggu kontrak BE terkait; PAY-BE-04 dan PAY-BE-05 mendahului angsuran final dan pembayaran; PAY-BE-08 mendahului status bayar final dan slip terbit; PAY-MIG-01 setelah kebutuhan schema jelas. Urutan pengerjaan: BE-01 → BE-04/05 → BE-03 → BE-06/07/08/09/10 → MIG-01 → FE-01..10 → DOC-01.

## 3. Subtask backend-first per fitur

| Fitur | Subtask BE | Subtask FE | Ketergantungan |
| --- | --- | --- | --- |
| Profil Penggajian | `PayrollCompensationService` + controller (daftar/detail/nominal) | daftar, detail `BaseDetailView`, penyamaran nominal | BE-03 → FE-03 |
| Penetapan Gaji | baca rekap HRD (`WorkforceCoreCrossEmployeeController`) | daftar, filter persetujuan/utama/aktif | BE-03 → FE-03 |
| Persiapan Penggajian | `Create`, `period-options`, `period-overview`, `hrd-readiness`, `import-hrd` | pilih periode, buka run, panel kesiapan, impor | BE-04 → FE-04 |
| Perhitungan Gaji | `Calculate`, `Submit`, `employees/{id}`, `status-history` | daftar run, detail, rincian penerima | BE-04/05 → FE-04 |
| Persetujuan Penggajian | `Approve`, `Return`, `Cancel`, `Close`, pembalikan | antrean, tinjau, konfirmasi beralasan | BE-05 → FE-05 |
| Cicilan Piutang Karyawan | `employee-receivable-deductions` (daftar, ringkasan, rekonsiliasi) | dua tab (riwayat, jadwal), rekonsiliasi | BE-06 → FE-06 |
| Restitusi dan Koreksi | `payroll-restitutions` (daftar) | daftar + buka run | BE-06 → FE-06 |
| Integrasi Jasa Medis | `medical-fee-intake` (daftar posisi, klaim) | daftar, dialog klaim | BE-07 → FE-07 |
| Pengiriman/Riwayat Pembayaran | `finance-handoff`, `payments`, `retry` | konfirmasi kirim, riwayat, coba ulang | BE-08 → FE-08 |
| Slip Gaji | `payslips` (+ `generate`, `publish`, `me`, `download`) | admin, saya, detail, unduh | BE-09 → FE-09 |
| Laporan | `reports/recap`, `reports/reconciliation` | rekap, rekonsiliasi | BE-10 → FE-10 |

## 4. Kontrak API target vs nyata (CT-PAY-01 … 15)

Rute target pada prompt bersifat usulan dan ditulis relatif terhadap `/payroll`. Rute nyata memakai awalan `api/v1/corporate/payroll-management/`.

| Kontrak | Target (prompt) | Rute nyata di kode | Status | Catatan |
| --- | --- | --- | --- | --- |
| CT-PAY-01 | `GET /payroll/master-data/metadata` | per entitas: `GET master-data/<entitas>/filters/metadata`; run: `GET payroll-runs/filters/metadata` | `PARTIAL` | Tidak ada satu endpoint metadata gabungan; sembilan endpoint baku per master tetap dipakai |
| CT-PAY-02 | `GET /payroll/workforce-compensations` | `GET workforce-compensations` | `IMPLEMENTED` | Daftar berhalaman; nominal hanya bagi `ViewAmount` |
| CT-PAY-03 | `POST /payroll/payroll-runs` | `POST payroll-runs` | `IMPLEMENTED` | Satu run `Regular` aktif per periode |
| CT-PAY-04 | `POST .../{id}/import-hrd` | `POST payroll-runs/{id}/import-hrd` | `IMPLEMENTED` | Hasil per sumber |
| CT-PAY-05 | `POST .../{id}/calculate` | `POST payroll-runs/{id}/calculate` | `IMPLEMENTED` | `expectedRowVersion` → 409 bila basi; ditolak bila terkunci |
| CT-PAY-06 | `POST .../{id}/submit` | `POST payroll-runs/{id}/submit` | `IMPLEMENTED` | Tanpa penahan |
| CT-PAY-07 | `POST .../{id}/approve` | `POST payroll-runs/{id}/approve` | `IMPLEMENTED` | Izin `PayrollRun : Approve`; opsi pemisahan tugas |
| CT-PAY-08 | `POST .../{id}/finance-handoff` | `POST payroll-runs/{runId}/finance-handoff` | `IMPLEMENTED` | Idempoten; penerima di Finance `BLOCKED` |
| CT-PAY-09 | `GET .../{id}/payments` | `GET payroll-runs/{runId}/payments` | `IMPLEMENTED` | - |
| CT-PAY-10 | `GET .../{id}/payslips` | `GET payroll-runs/{runId}/payslips` | `IMPLEMENTED` | Nominal bersih hanya bagi `ViewAmount` |
| CT-PAY-11 | `GET /payroll/payslips/me` | `GET payslips/me` dan `api/v1/self-services/payroll/payslips` | `IMPLEMENTED` | Hanya slip milik pengguna |
| CT-PAY-12 | `GET .../employee-receivable-deductions` | `GET employee-receivable-deductions` | `IMPLEMENTED` | Plus `GET .../summary` |
| CT-PAY-13 | `POST .../reconcile` | `POST employee-receivable-deductions/reconcile` | `IMPLEMENTED` | Tanpa bayar ulang |
| CT-PAY-14 | `POST /payroll/medical-fee-intake` | `POST medical-fee-intake` | `IMPLEMENTED` | 409 bila kebijakan jalur Payroll nonaktif |
| CT-PAY-15 | `GET /payroll/reports/reconciliation` | `GET reports/reconciliation?payrollRunId=` | `IMPLEMENTED` | Izin `PayrollReport : ViewAmount` |

Rute lama (alias): `api/v1/corporate/human-resource/payroll-runs`, `.../payroll-restitutions`, `.../master-data/<entitas>`, `.../workforce-profiles/{id}/{payroll-profile|tax-profile|insurance-profile|transport-allowance|transport-allowance-transactions|salary-assignments}`, `.../payroll-management/transport-allowance-policies` tetap hidup di controller yang sama. Penghapusan bertahap setelah seluruh pemakai pindah.

## 5. Dokumentasi endpoint (gaya Swagger)

Judul bagian = nilai `[Tags(...)]` pada controller. Respons dibungkus `ApiResponse<T>` (`success`, `statusCode`, `message`, `data`, `errors`, `timestamp`); daftar memakai `PagedResult<T>` (`items`, `pageNumber`, `pageSize`, `totalData`, `totalPage`).


### Corporate / Payroll Management / Payroll Run

Base URL: `api/v1/corporate/payroll-management/payroll-runs` (alias lama/tambahan: `api/v1/corporate/human-resource/payroll-runs`)

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat daftar payroll run (filter periode, status — boleh beberapa status dipisah koma — dan nomor run) | `PayrollRun : Read` | query `PayrollRunQuery` | `PagedResult<PayrollRunListItemResponse>` |
| `GET` | `/summary` | Melihat jumlah run per status (tanpa nominal gaji) | `PayrollRun : ReadSummary` | - | `PayrollRunSummaryResponse` |
| `GET` | `/filters/metadata` | Mengambil pilihan filter daftar run | `PayrollRun : ReadMetadata` | - | `PayrollRunFilterMetadataResponse` |
| `GET` | `/period-options` | Mengambil daftar periode penggajian untuk pilihan | `PayrollRun : ReadPeriods` | - | `List<PayrollRunPeriodOptionResponse>` |
| `GET` | `/period-overview` | Melihat status satu periode beserta angsuran yang jatuh tempo | `PayrollRun : ReadPeriodOverview` | query `Guid payrollPeriodId` | `PayrollRunPeriodOverviewResponse` |
| `GET` | `/{id:guid}` | Melihat rincian satu run (penerima, penahan, peringatan, aksi yang tersedia) | `PayrollRun : ReadDetail` | path | `PayrollRunDetailResponse` |
| `GET` | `/{id:guid}/status-history` | Melihat riwayat perpindahan status dan alasan | `PayrollRun : ReadStatusHistory` | path | `List<PayrollRunStatusHistoryItemResponse>` |
| `GET` | `/{id:guid}/employees/{runEmployeeId:guid}` | Melihat rincian gross-to-net satu penerima | `PayrollRun : ReadEmployeeDetail` | path | `PayrollRunEmployeeDetailResponse` |
| `GET` | `/{id:guid}/hrd-readiness` | Memeriksa kesiapan data HRD tiap penerima | `PayrollRun : ReadHrdReadiness` | path; query `PayrollHrdReadinessQuery` | `PayrollRunHrdReadinessResponse` |
| `POST` | `/` | Membuka payroll run untuk satu periode | `PayrollRun : Create` | body `CreatePayrollRunRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/import-hrd` | Mengimpor snapshot absensi, cuti, lembur yang sudah disahkan HRD | `PayrollRun : ImportHrd` | path; body `PayrollHrdImportRequest` | `PayrollHrdImportResponse` |
| `POST` | `/{id:guid}/calculate` | Menghitung (atau menghitung ulang) gaji dan potongan | `PayrollRun : Calculate` | path; body `PayrollRunActionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/submit` | Mengajukan run untuk disahkan | `PayrollRun : Submit` | path; body `PayrollRunActionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/return` | Mengembalikan run yang diajukan ke tahap hitung (alasan wajib) | `PayrollRun : Return` | path; body `PayrollRunDecisionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/approve` | Mengesahkan run, mengonfirmasi potongan ke Finance, dan mengunci run | `PayrollRun : Approve` | path; body `PayrollRunActionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/cancel` | Membatalkan run sebelum disahkan (alasan wajib) | `PayrollRun : Cancel` | path; body `PayrollRunDecisionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/close` | Menutup run yang seluruh pembayarannya berhasil | `PayrollRun : Close` | path; body `PayrollRunActionRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/employees/{runEmployeeId:guid}/reversals` | Mengajukan pembalikan potongan cicilan satu penerima | `PayrollRun : RequestReversal` | path; body `RequestPayrollReversalRequest` | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/reversals/{reversalId:guid}/approve` | Menyetujui pembalikan (saldo piutang dipulihkan di Finance) | `PayrollRun : ApproveReversal` | path | `PayrollRunDetailResponse` |
| `POST` | `/{id:guid}/reversals/{reversalId:guid}/reject` | Menolak pembalikan (alasan wajib) | `PayrollRun : RejectReversal` | path; body `PayrollRunDecisionRequest` | `PayrollRunDetailResponse` |

### Corporate / Payroll Management / Workforce Compensation

Base URL: `api/v1/corporate/payroll-management/workforce-compensations`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat daftar profil penggajian penerima | `PayrollCompensation : Read` | query `PayrollCompensationQuery` | `PagedResult<PayrollCompensationListItemResponse>` |
| `GET` | `/{workforceProfileId:guid}` | Melihat rincian penghasilan satu penerima | `PayrollCompensation : ReadDetail` | path | `PayrollCompensationDetailResponse` |
| `GET` | `/{workforceProfileId:guid}/amounts` | Melihat rincian beserta nominal gaji (izin khusus) | `PayrollCompensation : ViewAmount` | path | `PayrollCompensationDetailResponse` |

### Corporate / Payroll Management / Employee Receivable Deduction

Base URL: `api/v1/corporate/payroll-management/employee-receivable-deductions`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat riwayat potongan cicilan per run dan posisinya di Finance | `PayrollEmployeeDeduction : Read` | query `PayrollEmployeeDeductionQuery` | `PagedResult<PayrollEmployeeDeductionItemResponse>` |
| `GET` | `/summary` | Melihat ringkasan potongan cicilan satu periode | `PayrollEmployeeDeduction : ReadSummary` | query `Guid payrollPeriodId` | `PayrollDeductionReconciliationResponse` |
| `POST` | `/reconcile` | Menjalankan rekonsiliasi periode (hanya membandingkan, tanpa bayar ulang) | `PayrollEmployeeDeduction : Reconcile` | body `PayrollDeductionReconcileRequest` | `PayrollDeductionReconciliationResponse` |

### Corporate / Payroll Management / Restitution

Base URL: `api/v1/corporate/payroll-management/payroll-restitutions` (alias lama/tambahan: `api/v1/corporate/human-resource/payroll-restitutions`)

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat daftar restitusi salah potong | `PayrollRestitution : Read` | query `PayrollRestitutionQuery` | `PagedResult<PayrollRestitutionListItemResponse>` |
| `POST` | `/` | Menyematkan restitusi pada slip periode berikutnya (dipanggil Finance, idempoten) | `PayrollRestitution : ProcessRestitution` | body `PayrollRestitutionRequest` | `PayrollRestitutionResponse` |
| `GET` | `/by-source/{sourceId:guid}` | Mencari restitusi berdasarkan sumber koreksi Finance | `PayrollRestitution : GetBySourceId` | path | `PayrollRestitutionResponse` |

### Corporate / Payroll Management / Medical Fee Intake

Base URL: `api/v1/corporate/payroll-management/medical-fee-intake`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat posisi jalur bayar tiap hak jasa medis | `PayrollMedicalFeeIntake : Read` | query `PayrollMedicalFeeQuery` | `PayrollMedicalFeeCandidateListResponse` |
| `POST` | `/` | Mengklaim hak jasa medis ke payroll run terbuka (jalur Payroll) | `PayrollMedicalFeeIntake : Claim` | body `PayrollMedicalFeeIntakeRequest` | `PayrollMedicalFeeIntakeResponse` |

### Corporate / Payroll Management / Payment

Base URL: `api/v1/corporate/payroll-management/payroll-runs/{runId:guid}`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/finance-handoff` | Mengirim instruksi pembayaran satu run ke Finance (idempoten) | `PayrollPayment : FinanceHandoff` | body `PayrollFinanceHandoffRequest` | `PayrollFinanceHandoffResponse` |
| `GET` | `/payments` | Melihat instruksi pembayaran dan hasil bank | `PayrollPayment : Read` | query `PayrollPaymentQuery` | `PayrollPaymentListResponse` |
| `POST` | `/payments/acknowledge` | Menerima hasil pembayaran: hasil bank dari Finance (`confirmationSource` `FINANCE_ACK`, bawaan) atau konfirmasi manual petugas berwenang (`MANUAL`; sejak `PAY-FE-12` lewat layar Konfirmasi Pembayaran Manual). Sumber tak dikenal ditolak 400 sebelum data berubah | `PayrollPayment : Acknowledge` | body `PayrollPaymentAcknowledgeRequest` | `PayrollPaymentAcknowledgeResponse` |
| `POST` | `/payments/{paymentId:guid}/retry` | Membuat instruksi baru untuk pembayaran yang gagal | `PayrollPayment : Retry` | path | `PayrollFinanceHandoffResponse` |

### Corporate / Payroll Management / Payslip

Base URL: `api/v1/corporate/payroll-management`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/payroll-runs/{runId:guid}/payslips` | Melihat slip gaji satu run | `PayrollPayslip : Read` | path; query `PayrollPayslipQuery` | `PagedResult<PayslipSummaryResponse>` |
| `POST` | `/payroll-runs/{runId:guid}/payslips/generate` | Membuat slip dari snapshot run | `PayrollPayslip : Generate` | path | `PayrollPayslipGenerateResponse` |
| `POST` | `/payroll-runs/{runId:guid}/payslips/publish` | Menerbitkan slip sesuai kebijakan akses slip | `PayrollPayslip : Publish` | path | `PayrollPayslipPublishResponse` |
| `GET` | `/payslips/{id:guid}` | Melihat isi slip (snapshot beku) | `PayrollPayslip : ReadDetail` | path | `PayrollPayslipDetailResponse` |
| `GET` | `/payslips/{id:guid}/download` | Mengunduh PDF slip (dicatat sebagai jejak audit) | `PayrollPayslip : ViewAmount` | path | `File PDF (`application/pdf`)` |

### Self Services / Payroll / Payslip

Base URL: `api/v1/corporate/payroll-management/payslips/me` (alias lama/tambahan: `api/v1/self-services/payroll/payslips`)

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Melihat slip milik pengguna yang masuk | `MyPayslip : Read` | query `PayrollPayslipQuery` | `PagedResult<PayslipSummaryResponse>` |
| `GET` | `/{id:guid}` | Melihat isi slip milik sendiri | `MyPayslip : ReadDetail` | path | `PayrollPayslipDetailResponse` |
| `GET` | `/{id:guid}/download` | Mengunduh PDF slip milik sendiri | `MyPayslip : Download` | path | `File PDF (`application/pdf`)` |

### Corporate / Payroll Management / Report

Base URL: `api/v1/corporate/payroll-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response (`ApiResponse<T>`) |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/recap` | Melihat rekap agregat penggajian (tanpa gaji per orang) | `PayrollReport : Read` | query `PayrollRecapQuery` | `PayrollRecapResponse` |
| `GET` | `/reconciliation` | Melihat rekonsiliasi Payroll–Finance–Accounting satu run | `PayrollReport : ViewAmount` | query `Guid payrollRunId` | `PayrollReconciliationResponse` |

### Controller HRD yang mendapat rute alias Payroll

Controller berikut tidak berpindah folder (fase 2) tetapi sudah dapat dipanggil lewat rute Payroll. Tabel endpoint lengkapnya tidak berubah dan tetap mengikuti dokumentasi controller masing-masing.

| `[Tags(...)]` | Base URL baru | Base URL lama (tetap hidup) |
| --- | --- | --- |
| Corporate / Payroll Management / Master Data / Salary Grade (+ 9 master lain) | `api/v1/corporate/payroll-management/master-data/{salary-grades, salary-structures, payroll-periods, payroll-components, payroll-component-categories, allowance-types, deduction-types, shift-allowance-policies, on-call-allowance-policies, hazard-allowance-policies}` | `api/v1/corporate/human-resource/master-data/...` |
| Corporate / Payroll Management / Payroll Profile | `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/payroll-profile` | `api/v1/corporate/human-resource/workforce-profiles/...` |
| Corporate / Payroll Management / Tax Profile | `.../workforce-profiles/{workforceProfileId}/tax-profile` | idem |
| Corporate / Payroll Management / Insurance Profile | `.../workforce-profiles/{workforceProfileId}/insurance-profile` | idem |
| Corporate / Payroll Management / Transport Allowance (+ Transaction) | `.../workforce-profiles/{workforceProfileId}/transport-allowance`, `.../transport-allowance-transactions` | idem |
| Corporate / Payroll Management / Transport Allowance Policy | `api/v1/corporate/payroll-management/transport-allowance-policies` | `api/v1/corporate/human-resource/payroll-management/transport-allowance-policies` |
| Corporate / Payroll Management / Salary Assignment | `api/v1/corporate/payroll-management/workforce-profiles/{workforceProfileId}/salary-assignments` | `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/salary-assignments` |

Satu endpoint HRD yang dikonsumsi layar Payroll: `GET api/v1/corporate/human-resource/workforce-core/salary-assignments` (`WfpSalaryAssignment : ReadAll`, tag HRD cross-employee) untuk rekap Penetapan Gaji.

### Arti kode status bagi pengguna

| Kode | Arti | Contoh |
| ---: | --- | --- |
| 200 | Berhasil | Daftar atau aksi selesai |
| 400 | Aksi tidak sesuai tahap data saat ini | Mengesahkan run yang belum diajukan |
| 401/403 | Belum masuk / tidak punya hak akses untuk tindakan ini | Akun tanpa `PayrollRun : Approve`; pemisahan tugas aktif dan pengaju mencoba mengesahkan sendiri |
| 404 | Data tidak ada atau bukan milik pengguna | Slip milik orang lain |
| 409 | Data sudah diubah pihak lain atau kebijakan belum aktif | Token versi basi; klaim jasa medis saat jalur Payroll nonaktif |
| 422 | Isian atau data sumber tidak valid | Alasan kosong; instruksi bayar ditahan karena rekening belum terverifikasi |

### Bentuk request/response yang paling sering dipakai

| DTO | Isi penting |
| --- | --- |
| `CreatePayrollRunRequest` | `payrollPeriodId`, `notes` |
| `PayrollRunActionRequest` / `PayrollRunDecisionRequest` | `expectedRowVersion` (token dari `rowVersion`), `reason` (wajib untuk Return/Cancel/RejectReversal) |
| `PayrollRunDetailResponse` | `rowVersion`, `runStatus`, `isLocked`, total bruto/potongan/bersih, `blockers`, `warnings`, `availableActions`, `employees[]`, `reversals[]`, `paymentSummary`, `policyVersion` |
| `PayrollHrdImportRequest` | `includeAttendance`, `includeLeave`, `includeOvertime`, `notes`, `expectedRowVersion` |
| `PayrollFinanceHandoffRequest` | `expectedRowVersion`, `idempotencyKey`, `notes` |
| `PayrollPaymentAcknowledgeRequest` | `idempotencyKey`, `confirmationSource` (opsional: `FINANCE_ACK` bawaan atau `MANUAL`; satu nilai untuk seluruh kiriman), `financePaymentBatchId`, `items[]` (`paymentNumber`, `status` `Paid`/`Failed`, `paidAmount`, `paymentReferenceNumber`, `failureReason`). Respons: `confirmationSource` sebagai gema |
| `PayrollRunPaymentSummaryResponse` (pada `GET /payments` dan `paymentSummary` detail run) | `instructionCount`, `scheduledCount`, `paidCount`, `failedCount`, nominal, serta (PAY-BE-11) `confirmedDeductionPaymentPendingCount` dan `confirmedDeductionPaymentFailedCount`: jumlah potongan piutang yang sudah dikonfirmasi saat run disahkan untuk penerima yang gajinya belum dibayar, atau gagal/dibalik |
| `PayrollEmployeeDeductionItemResponse` | selain posisi di Finance, (PAY-BE-11) `salaryPaymentStatus` dan `salaryPaymentNumber`. Kode: `BELUM_DIKIRIM`, `MENUNGGU_BAYAR`, `LUNAS`, `GAGAL`, `DIBALIK`, `TIDAK_ADA_PEMBAYARAN` |
| `PayrollDeductionReconciliationResponse` | selain total dan `discrepancyCount`, (PAY-BE-11) `confirmedPaymentPendingCount`, `confirmedPaymentFailedCount`, dan `salaryPaymentStatus` pada tiap item. Status bayar bukan temuan selisih jadwal |
| `PayrollReconciliationResponse` (`GET /reports/reconciliation`) | selain pemeriksaan lama, (PAY-BE-11) pemeriksaan `AR_DEDUCTION_PAYMENT`: `SELISIH` bila ada potongan terkonfirmasi untuk penerima yang gajinya gagal/dibalik, `INFO` bila hanya menunggu, `OK` bila semua lunas |
| `PayrollMedicalFeeIntakeRequest` | `payrollRunId`, `serviceFeeIds[]` (maks. 200), `notes` |
| `PayslipSummaryResponse` | `payslipNumber`, `versionNumber`, `payslipStatus`, `netPay` (null tanpa `ViewAmount`), `downloadCount` |

Presisi desimal: nominal `decimal(18,2)`, kuantitas `decimal(18,4)`. Zona waktu: semua cap waktu `timestamptz` (UTC) dan ditampilkan dalam waktu lokal di layar.

## 6. Kontrak error dan penegakan di frontend

Frontend mengklasifikasi galat menjadi `forbidden` (mengganti seluruh halaman dengan "Akses Ditolak"), `notFound`, `network`, `server`, dan `request` (banner dengan pesan backend). Konflik versi (409) menampilkan tombol **Muat Ulang Data**. Frontend tidak menebak payload: normalizer membaca field persis seperti DTO dan mempertahankan `null` untuk nominal yang disembunyikan.

## 7. Register risiko

| ID | Risiko | Dampak | Penanganan | Label |
| --- | --- | --- | --- | --- |
| R1 | Satu angsuran per pegawai per run | Angsuran kedua tidak terpotong tanpa pesan | Perbaiki bentuk baris potongan (banyak angsuran per penerima) bersama pemilik Finance | `PARTIAL` |
| R2 | Titik pengakuan pelunasan = saat run disahkan | Pembayaran gagal total setelah disahkan meninggalkan potongan tercatat | Pembalikan beralasan tersedia; keputusan titik resmi di Finance/Accounting | `DECIDED` (pemilik, 11 November 2026: tetap A, ditambah penanda dan peringatan lewat `PAY-BE-11` dan `PAY-FE-13`; persetujuan Finance/Accounting masih diperlukan) |
| R3 | Finance belum menerima instruksi payroll | Status bayar tidak bergerak dari `Dijadwalkan` | Bangun jenis pembayaran `PAYROLL` di Finance | `BLOCKED` |
| R4 | Tidak ada jurnal gaji | Rekonsiliasi akuntansi `Belum Tersedia` | Sahkan mapping COA dan kode peristiwa | `BLOCKED` |
| R5 | PPh 21/BPJS dari tetapan profil | Salah hitung bila tetapan tidak diperbarui | Sahkan rumus dan masa berlaku tarif | `NEEDS CONFIRMATION` |
| R6 | Migration indeks unik gagal bila data lama sudah duplikat | `database update` berhenti | Jalankan SQL pra-cek di `PAYROLL_V2_05` sebelum menerapkan | Risiko operasional |
| R7 | Kode belum dikompilasi/dijalankan | Kesalahan kompilasi atau terjemahan LINQ baru terlihat saat build pertama | Build dan uji oleh pihak berwenang sebelum merge | Batas pembuktian |
| R8 | Pemisahan data per entitas hukum/unit hanya lewat periode | Penerima lintas entitas tidak difilter saat run dibuka | Sahkan aturan cakupan entitas | `NEEDS CONFIRMATION` |
| R9 | Menu HR yang dipindah kini wajib punya izin menu | Peran lama yang tidak punya `Read` tidak melihat menu (akses langsung tetap ditolak 403) | Admin menyesuaikan Akses Role | Perubahan perilaku disengaja |

## 8. Matriks UAT minimal

Status "Bukti statis" = kode/uji statis yang menutup skenario; **belum** ada eksekusi runtime.

| No | Skenario | Hasil yang diharapkan | Bukti statis | Status |
| ---: | --- | --- | --- | --- |
| 1 | Karyawan normal sebulan penuh | Bruto = gaji pokok + tunjangan + lembur; bersih = bruto − pajak − iuran − potongan | `PayrollCalculationService.CalculateEmployee` | `IMPLEMENTED` |
| 2 | Gaji prorata (bergabung 11 Nov, periode 30 hari) | Rp 6.000.000 × 20 ÷ 30 = Rp 4.000.000 | `PayrollCalculationService` (segmen hari); contoh `PAYROLL_V2_02` bagian 5 | `IMPLEMENTED` |
| 3 | Cuti tanpa gaji | Potongan `PAY_UNPAID_LEAVE` sesuai hari | impor cuti + `UnpaidLeaveDeductionEnabled` | `IMPLEMENTED` (`NEEDS CONFIRMATION` rumus) |
| 4 | Lembur disetujui | Komponen `PAY_OVERTIME` dari snapshot lembur | `PayrollHrdImportService`, `TrxPayrollOvertimeInput` | `IMPLEMENTED` |
| 5 | Dokter bergaji tetap tanpa `MstEmployee` | Masuk run via `WorkforceProfileId`, tanpa karyawan palsu | `PayrollCompensationService`, `PayrollCalculationService` (identitas dokter) | `IMPLEMENTED` |
| 6 | Dokter dengan jasa medis | Satu hak hanya satu jalur; tombol klaim tidak muncul bila sudah ada catatan AP | `PayrollMedicalFeeIntakeService`; indeks unik klaim | `IMPLEMENTED` |
| 7 | Potongan AR penuh | Dipotong sekali; piutang turun hanya setelah run disahkan | `payroll-installment-deduction-guards` | `IMPLEMENTED` |
| 8 | Potongan AR sebagian | Dipotong sesuai batas; selisih ditampilkan beserta alasan | `PayrollInstallmentDeductionService` (`SEBAGIAN`), kolom "Rencana Potong" | `IMPLEMENTED` |
| 9 | Replay (konfirmasi ulang, ACK ulang, handoff ulang, klaim ulang) | Tidak ada potongan/instruksi/klaim tambahan | kunci idempotensi, indeks unik, `IsIdempotentReplay` | `IMPLEMENTED` |
| 10 | Periode terkunci | Run baru ditolak; hitung ulang ditolak setelah kunci | `PayrollRunService.CreateAsync`, `AcceptsInputs` | `IMPLEMENTED` |
| 11 | Gagal transfer | Instruksi `Failed` dengan alasan; bisa dicoba ulang tanpa menggandakan yang berhasil | `PayrollPaymentService.AcknowledgeAsync/RetryAsync` | `IMPLEMENTED` (ACK nyata menunggu Finance) |
| 12 | Reversal | Pengajuan → persetujuan → saldo piutang dipulihkan sekali | `RequestReversalAsync`, `ApproveReversalAsync`; uji `payroll-run-screens` | `IMPLEMENTED` |
| 13 | Slip rahasia | Pemilik hanya melihat slipnya; slip orang lain 404; unduhan tercatat | `PayrollMyPayslipsController`, `PayrollPayslipService.GetDetailAsync/DownloadAsync` | `IMPLEMENTED` |
| 14 | Data perusahaan/unit berbeda | Run mencatat entitas dan lokasi dari periode; penyaringan penerima per entitas tidak ditambahkan | `PayrollRunService.CreateAsync` | `PARTIAL` (R8) |

## 9. Checklist penerimaan

| Butir | Status | Bukti |
| --- | --- | --- |
| Tidak ada dua route aktif/dua controller yang membuat run dengan semantik sama | Terpenuhi | satu `PayrollRunsController` dengan `[Route]` ganda; `verify_be.py` (bentrok rute 0) |
| DI, namespace, using, izin, metadata controller diperbarui | Terpenuhi | `Program.cs`; `verify_be.py` 0 kesalahan |
| HRD tetap menjalankan handoff absensi/cuti/lembur | Terpenuhi (statis) | tiga layanan HRD tidak diubah; dipanggil `PayrollHrdImportService` |
| Setiap request FE cocok dengan route BE | Terpenuhi (statis) | `payroll-module-contract.test.mjs` (kontrak Backend) |
| Penetapan gaji tidak tumpang tindih; angka gaji/NPWP/rekening tidak bocor ke peran umum | Terpenuhi sebagian | tumpang tindih → penahan; nominal null tanpa `ViewAmount`; rekening/NPWP tersamar. Pemisahan editor≠penyetuju penetapan gaji `NEEDS CONFIRMATION` |
| Tidak ada hitung ulang setelah lock/paid | Terpenuhi | `AcceptsInputs`, `IsLocked` |
| Restitusi sekali, tanpa mutasi total ganda | Terpenuhi | `PayrollRestitutionService`, komponen non-mesin dihitung sekali |
| gross, deduction, net, payment, AR settlement, jurnal dapat direkonsiliasi | Sebagian | jurnal `BLOCKED` |
| FE berlabel Indonesia, komponen konsisten, `Kembali` sesuai sumber, filter reset bekerja | Terpenuhi (statis) | tes kontrak; reset memakai `DataFilter.onReset`; `Kembali` memakai riwayat peramban bila ada |
| Pengujian runtime | **Belum** | build/server dilarang pemilik |
