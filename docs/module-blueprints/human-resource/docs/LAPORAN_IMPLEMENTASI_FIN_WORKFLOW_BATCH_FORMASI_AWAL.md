# LAPORAN IMPLEMENTASI FINISHING INTEGRASI WORKFLOW `T10` — BATCH FORMASI AWAL (`IFB-000001`)

**Tanggal Pelaporan:** 27 November 2026  
**Modul:** Sumber Daya Manusia (Human Resource)  
**Menu:** Perencanaan Strategis SDM & Kotak Masuk Persetujuan Terpadu (V2)  
**Submenu Utama:** Batch Formasi Awal (`/hr/workforce-planning/initial-formation-batches`), Formasi Pegawai / Buku Formasi (`/hr/workforce-planning/position-formations`), Persetujuan V2 (`/hrd/approvals/inbox`, `/hrd/approvals/[id]`, `/hrd/approvals/matrix`)  
**Repositori Backend:** `DevBenari/NewQuilvianSystemBackend` (branch `MHamzah`, commit `8a698521`, `3915bfba`)  
**Repositori Frontend:** `DevBenari/QuilvianSystemFrontendDev` (branch `HamzahV2`, commit `c7376fe82`)  
**Status Tugas:** `BE-HRD-FIN-WFP-008` s.d. `BE-HRD-FIN-WFP-011` & `FE-HRD-FIN-WFP-008` s.d. `FE-HRD-FIN-WFP-009` — **SELESAI & TERVERIFIKASI**

---

## 1. Ringkasan Eksekutif

Pekerjaan finishing ini menuntaskan integrasi end-to-end alur persetujuan berjenjang **Workflow `T10`** (`HR Manager -> Finance -> Direktur Utama`) untuk **Batch Formasi Awal** pada modul Perencanaan Strategis SDM, sekaligus memulihkan secara resmi dan beraudit batch UAT `IFB-000001` (`3089fe12-7ddb-4500-8047-6e1d98e05355`) yang sebelumnya terjebak pada status `Submitted` tanpa instans workflow.

Setelah perbaikan pada Backend (.NET) dan Frontend (Next.js/JSX), seluruh alur nyata dari pemulihan batch terputus ke `Draft`, pengajuan ulang ke workflow `T10`, persetujuan berjenjang 3 tingkat pada lingkungan live (`http://localhost:3000` + `https://localhost:7184` + PostgreSQL `QuilvianNewDevHamzah`), hingga pembentukan resmi formasi Versi 1 di Buku Formasi telah berjalan penuh dan diverifikasi:

- **Batch `IFB-000001`** resmi berstatus **`Applied`** (`Berlaku`, `BatchStatus = 5`) dan terhubung ke instans **`WF-20261127025915-C044A0FDF6C348C79F0EA1F04BC3327D`** (`WorkflowInstanceId = a63c5126-157f-4afd-8f39-0e022b35c99b`).
- **Tepat 1 baris formasi Versi 1** terbentuk di `public."HrdPositionFormation"` (`Id = facd7888-b2b5-4601-865c-c2918140ab7e`) untuk **Instalasi Rawat Inap — Perawat Rawat Inap** sebanyak **`10.00` FTE** dengan tanggal efektif **`2027-01-01`** (1 Januari 2027).
- Mekanisme **anti false-success**, **anti-duplikat / idempotency**, serta **pemulihan resmi berizin dan beraudit** (`POST .../recover`) berjalan dan lolos pengujian unit, API nyata, maupun live browser testing.

---

## 2. Permasalahan Awal & Root Cause Terverifikasi

### 2.1 Bukti Masalah UAT Awal

1. Batch `IFB-000001` tampil berstatus `Submitted` (`Menunggu Persetujuan`), tetapi `WorkflowInstanceId = null` dan `CreatedFormationId = null`.
2. Pencarian kode lama `HRD_T10_INITIAL_FORMATION` mengembalikan HTTP `200` dengan `totalData: 0`.
3. Halaman daftar dan detail Batch Formasi Awal belum menampilkan status koneksi workflow `T10`, daftar approver per tahap, riwayat aksi workflow, maupun aksi pemulihan beraudit untuk batch yang terputus.
4. Halaman Kotak Masuk Persetujuan Terpadu (V2) belum menyertakan `ReferenceId` untuk membuka dokumen sumber Batch Formasi Awal dan `WorkflowRoutingMatrixService` belum menampilkan seluruh definisi aktif (termasuk `T10`) pada Matriks Alur Persetujuan.

### 2.2 Ringkasan Root Cause pada Kode & Data

| Area | Temuan Root Cause | Dampak ke Proses Rumah Sakit |
|---|---|---|
| **Kode Workflow & Konteks** | Kode definisi aktif di engine adalah `"T10"` (`Pengesahan Rencana Kebutuhan Tenaga Tahunan & Batch Formasi Awal`), bukan kode lama. | Pengajuan Batch Formasi Awal gagal menemukan definisi workflow yang sah. |
| **False Success & Transaksi** | `SubmitAsync()` pada `HrdInitialFormationBatchService.cs` belum menjamin fail-closed tanpa mengubah status menjadi `Submitted` ketika workflow gagal dibuat/diajukan serta berisiko memicu nested transaction/meninggalkan Draft workflow yatim. | Batch seolah berhasil diajukan (`Submitted`), tetapi tidak ada tiket yang masuk ke kotak masuk approver. |
| **Decision Applier & Callback** | `HrdInitialFormationBatchDecisionApplier.cs` dan `HrdManpowerPlanDecisionApplier.cs` perlu menyelaraskan status final `"Completed"` / `"RevisionRequested"`, menjaga batas transaksi (`CurrentTransaction == null`), serta menjaga idempotency pembentukan formasi & tanggal efektif. | Risiko formasi gagal terbentuk setelah persetujuan Direktur Utama atau terbentuk ganda bila callback diulang. |
| **Master Struktur & Approver `T10`** | Perlu penyiapan unit `Direksi`, jabatan `Direktur Utama`, langkah workflow 3 tingkat (`T10_HR_MGR`, `T10_FINANCE`, `T10_DIRUT`), penempatan aktif, approval matrix, serta master alasan penolakan (`MstRejectionReason`) lewat API resmi. | Penugasan approver Level 2 (`Finance`) dan Level 3 (`Direktur Utama`) harus resolved deterministik dari data organisasi aktif. |
| **Antarmuka Batch & Approval V2** | Daftar & detail Batch belum menampilkan informasi workflow 2 kolom + aksi `Recover`; UI Approval Detail perlu dukungan status `InProgress`/`Available`, konteks terstruktur `summary`, tautan dokumen sumber (`ReferenceId`), serta aksi `Detail` + `Dokumen`. | Pengelola SDM tidak melihat hambatan persetujuan di detail batch dan approver kesulitan meninjau berkas sumber batch sebelum menyetujui. |

---

## 3. Alur Proses Bisnis & Skenario Rumah Sakit Konkret

**Skenario Konkret: Penetapan Formasi Awal 10 Perawat Rawat Inap untuk 1 Januari 2027**

Sebuah rumah sakit (**Metropolitan Medical Centre Hospital**) mengesahkan formasi awal unit **Instalasi Rawat Inap** untuk jabatan **Perawat Rawat Inap** sebanyak **10 FTE** berdasarkan **SK `UAT/SK-FORMASI/001/2027`** dengan tanggal mulai berlaku **1 Januari 2027**. Alur bisnis berjalan berurutan sebagai berikut:

1. **Pemulihan Batch Terputus (`POST .../recover`)**:
   - Staf Operasional HR (**Yudha Hermawan, S.E.**) membuka detail `IFB-000001`. Sistem mendeteksi masalah integritas karena batch berstatus `Submitted` dengan `WorkflowInstanceId = null`.
   - Sistem memeriksa engine workflow berdasarkan `ReferenceType = "WorkforceInitialFormationBatch"` dan `ReferenceId`. Karena belum ada instans sah maupun formasi, Yudha menekan tombol pemulihan dan mengisi alasan audit minimal 10 karakter pada `ConfirmModal`.
   - Batch kembali ke status `Draft` dengan catatan audit berstempel waktu UTC pada kolom `Notes`, sementara baris (`1` baris valid `10 FTE`) dan lampiran SK (`SK_Formasi_Awal_2027_Khusus_UAT.pdf`) tetap utuh.
2. **Pengajuan ke Workflow `T10` (`POST .../submit`)**:
   - Yudha mengajukan ulang `IFB-000001`. Backend secara transaksional membentuk dan mengajukan instans workflow `"T10"` (`WF-20261127025915-C044A0FDF6C348C79F0EA1F04BC3327D`), menyertakan `RequestContextJson.summary` terstruktur (`1 posisi · 10 FTE`, berlaku `2027-01-01`), dan menugaskan Tahap 1 ke **HR Manager**.
3. **Persetujuan Tahap 1 — HR Manager (`T10_HR_MGR`)**:
   - **Hartini Sulistyowati, S.Psi., M.M.** membuka Kotak Masuk Persetujuan Terpadu (V2), memeriksa kelengkapan baris & dokumen SK, lalu menyetujui dengan catatan dan PIN otorisasi.
   - Workflow bergerak ke Tahap 2 (`T10_FINANCE`). Pada tahap ini formasi **belum terbentuk** (`CreatedFormationId` masih `null`, status batch tetap `Submitted`).
4. **Persetujuan Tahap 2 — Finance (`T10_FINANCE`)**:
   - **Bambang Wijaya, S.E., M.Ak.** membuka detail permohonan di `/hrd/approvals/[workflowInstanceId]` dan menyetujui dengan catatan serta PIN otorisasi 6 digit (input PIN `< 6` digit dicegah oleh modal sebelum dikirim ke server).
   - Workflow bergerak ke Tahap 3 (`T10_DIRUT`). Formasi tetap **belum terbentuk** sebelum keputusan akhir Direktur Utama.
5. **Persetujuan Akhir Tahap 3 — Direktur Utama (`T10_DIRUT`)**:
   - **dr. Hendra Kusuma, MARS** membuka permohonan dari kotak masuk, mengklik **Buka Dokumen Sumber** untuk meninjau `IFB-000001`, lalu melakukan pengesahan akhir dengan PIN otorisasi.
6. **Penerapan Otomatis ke Buku Formasi (`HrdInitialFormationBatchDecisionApplier`)**:
   - Saat workflow mencapai `WorkflowStatus = "Completed"`, decision applier memverifikasi idempotency, membuat tepat 1 baris `HrdPositionFormation` **Versi 1** (`10.00` FTE, efektif `2027-01-01`, `SourceType = 1`), mengisi `CreatedFormationId`, menandai batch `Applied` (`Berlaku`), serta mencatat `ApprovedAt` dan `AppliedAt`.
   - Pengiriman ulang `approve`, `submit`, atau `recover` setelah selesai ditolak dengan kode status `4xx` (teruji HTTP `404`/`400`) tanpa menghasilkan workflow atau formasi ganda.

---

## 4. Implementasi Backend & Dokumentasi Endpoint Bergaya Swagger

### 4.1 Daftar Berkas Backend yang Diubah / Ditambahkan

| Berkas | Ringkasan Perubahan |
|---|---|
| `Areas/Corporate/HumanResource/WorkforcePlanning/Constants/WorkforcePlanningValueConstants.cs` | Menambahkan `WorkflowDefinitionCodes.ManpowerPlanAndInitialFormation = "T10"`. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/InitialFormationBatchDtos.cs` | Menambah `WorkflowInstanceId`, `WorkflowNumber`, `WorkflowStatus`, `HasWorkflowIntegrityIssue` pada item daftar batch; properti tahap/approver/riwayat workflow pada DTO detail; serta `RecoverInitialFormationBatchRequest`. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdInitialFormationBatchService.cs` | Menambahkan kontrak metode `RecoverAsync(...)`. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdInitialFormationBatchController.cs` | Menambahkan action `[HttpPost("{id:guid}/recover")]` (`[AccessAction("Recover", ...)]`, `[AccessPermission("WorkforceInitialFormationBatch", "Update")]`). |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdInitialFormationBatchService.cs` | Menyelaraskan kode workflow `"T10"`, menghilangkan false success dan nested transaction di `SubmitAsync()`, menambahkan idempotency/concurrency guard, memformat `RequestContextJson.summary` (baris `994-1010`) menjadi `{ label, value }[]`, mengimplementasikan `RecoverAsync()`, dan memperkaya query daftar/detail dengan diagnosa workflow engine. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdInitialFormationBatchDecisionApplier.cs` | Menyelaraskan status callback `"Completed"` dan `"RevisionRequested"`, menjaga transaksi (`CurrentTransaction == null`), melindungi pembuatan formasi dari duplikasi (idempotent), dan mempertahankan `EffectiveStartDate` per baris. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdManpowerPlanDecisionApplier.cs` | Penyelarasan handler status `"Completed"` / `"RevisionRequested"` dan proteksi transaksi serupa untuk konsistensi lintas `T10`. |
| `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningWorkflowIntegrationService.cs` | Menambahkan pembersihan instans workflow berstatus `Draft` apabila langkah `SubmitAsync` ke engine gagal setelah `CreateAsync`. |
| `Areas/Corporate/HumanResource/WorkflowManagement/DTOs/WorkflowV2Dtos.cs` | Menambahkan properti `ReferenceId` pada `ApprovalInboxItemV2ResponseDto`. |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/ApprovalInboxQueryService.cs` | Memetakan `ReferenceId` pada item kotak masuk persetujuan terpadu V2 dan menjaga konsistensi `TotalSteps`. |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowInstanceCreationService.cs` | Mengisi nama pemohon, unit, dan daftar approver aktual saat pembuatan instans workflow. |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowRoutingMatrixService.cs` | Membaca definisi & langkah workflow aktif dari database (termasuk `T10`) untuk endpoint Matriks Alur Persetujuan. |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs` | Penyesuaian resolusi approver dan perhitungan `TotalSteps`. |
| `Tests/QuilvianSystemBackend.HumanResourceTests/InitialFormationBatchWorkflowTests.cs` | Menambahkan 4 kasus uji xUnit menggunakan `LocalSqliteDb` terisolasi (meregistrasikan fungsi SQLite `btrim` tanpa memodifikasi `TestInfrastructure.cs`) dan `WorkflowReferenceLifecycleService`. |

### 4.2 Dokumentasi Endpoint API (Bergaya Swagger)

#### Grup Endpoint (`[Tags]` pada `HrdInitialFormationBatchController`): Perencanaan Strategis SDM — Batch Formasi Awal

| Method | Path | Deskripsi | Auth / Permission | Request Body | Response Sukses (`200`) / Error |
|---|---|---|---|---|---|
| `GET` | `/api/v1/corporate/human-resource/workforce-planning/initial-formation-batches/{id}` | Mengambil detail batch termasuk diagnosa integritas workflow, tahap aktif, approver yang ditugaskan, dan riwayat aksi workflow `T10`. | Terautentikasi + `WorkforceInitialFormationBatch : Read` | — | `200 OK`: `InitialFormationBatchDetailResponse` berisi `batchStatusName`, `workflowInstanceId`, `workflowNumber`, `workflowStatus`, `hasWorkflowIntegrityIssue`, `currentStepOrder`, `currentStepName`, `assignedApprovers[]`, `workflowHistories[]`, `rows[]` (termasuk `createdFormationId`, `effectiveStartDate`). `404` bila tidak ditemukan. |
| `POST` | `/api/v1/corporate/human-resource/workforce-planning/initial-formation-batches/{id}/submit` | Mengajukan batch `Draft`/`Returned` ke workflow `"T10"` (`ReferenceType = WorkforceInitialFormationBatch`). Bersifat fail-closed bila workflow gagal dan idempotent terhadap pengajuan ganda. | Terautentikasi + `WorkforceInitialFormationBatch : Update` | `{ "notes": "string?", "rowVersion": "uint" }` | `200 OK`: detail batch terbarui (`batchStatusName = "Submitted"`, `workflowInstanceId` terisi). `400 Bad Request` bila gagal validasi/workflow gagal (status batch tidak berubah ke `Submitted`). `409 Conflict` bila `RowVersion` usang. |
| `POST` | `/api/v1/corporate/human-resource/workforce-planning/initial-formation-batches/{id}/recover` | Memulihkan secara beraudit batch `Submitted` terputus (`WorkflowInstanceId == null`), belum menghasilkan formasi, dan belum memiliki keputusan akhir: merekonsiliasi bila workflow sah ditemukan atau mengembalikan batch ke `Draft`. | Terautentikasi + `[AccessAction("Recover")]` + `[AccessPermission("WorkforceInitialFormationBatch", "Update")]` | `{ "reason": "string (>=10 karakter)", "rowVersion": "uint" }` | `200 OK`: detail batch hasil rekonsiliasi atau berstatus `Draft` dengan catatan audit pemulihan. `400 Bad Request` bila status bukan `Submitted`, alasan `< 10` karakter, atau sudah `Applied`/berformasi. `409 Conflict` bila `RowVersion` usang. |

---

## 5. Implementasi Frontend

Seluruh komponen UI dibangun di `QuilvianSystemFrontendDev` (commit `c7376fe82`) menggunakan Next.js App Router, React/JSX, design token & base component Quilvian (`Hero`, `BaseButton`, `ConfirmModal`, `InformationAlert`, `StatusBadge`, `SummaryGrid`, `DataTable`, `HrPanel`, `HrInfoGrid`):

1. **Konstanta, Layanan, & Utilitas Tampilan**:
   - `src/lib/constants/hr/approvals/approval-constants.js`: menambahkan label `WorkforceInitialFormationBatch`, `ManpowerPlan`, `HospitalDirector`, `FinanceManager`, serta status `InProgress`, `Submitted`, `RevisionRequested`.
   - `src/lib/constants/hr/workforce-planning/workforce-planning-constants.js` & `src/lib/services/hr/workforce-planning-service.js`: menambahkan metode `recoverBatch(id, payload)`.
   - `src/utils/hr/approvals/workflow-display-utils.js`: mengutamakan parsing array `summary` terstruktur (langsung kembali saat non-empty agar tidak menduplikasi pasangan kunci datar) dan menambahkan `resolveWorkflowSourceHref`.
2. **Daftar & Detail Batch Formasi Awal**:
   - `src/components/view/hr/workforce-planning/initial-formation-batches-view.jsx`: menampilkan nomor/status workflow `T10` dan penanda jika integritas workflow bermasalah.
   - `src/components/view/hr/workforce-planning/initial-formation-batch-detail-view.jsx`: tata letak 2 kolom (panel utama dokumen, baris formasi, scan SK, riwayat persetujuan; panel samping status & workflow `T10` beserta approver per tahap), tombol pemulihan `ConfirmModal` dengan `<InformationAlert message={recoverError} />`, guard anti-false-success saat submit, serta tombol **Lihat Buku Formasi** setelah status `Applied`.
3. **Kotak Masuk & Detail Persetujuan Terpadu (V2)**:
   - `src/components/view/hr/approvals/unified-approval-inbox-view.jsx`: tombol aksi **Detail** dan **Dokumen**.
   - `src/components/view/hr/approvals/unified-approval-detail-view.jsx` & `WorkflowStepperCard.jsx`: mendukung `InProgress`/`Available`, tombol **Buka Dokumen Sumber**, `Penyetuju Tahap Aktif`, serta konteks terstruktur `summary` (`1 posisi · 10 FTE`).

---

## 6. Penyiapan Master Data & Bukti Database PostgreSQL Nyata

Penyiapan master pada database `QuilvianNewDevHamzah` dijalankan via API resmi modul organisasi & workflow (`test-with-agy/scripts/wfp-batch-t10-setup.mjs`, daftar ID pada `wfp-batch-t10-data.json`, tanpa mencetak atau menyimpan kredensial/secret): unit `Direksi`, jabatan `Direktur Utama`, pembaruan kebijakan `SysAccessPolicy`, akun penguji peran (`finance.manager`, `direktur.utama`), 3 langkah `T10` (`T10_HR_MGR`, `T10_FINANCE`, `T10_DIRUT`), 6 baris `MstApprovalMatrix`, dan 3 baris `MstRejectionReason` (`T10_TOLAK_01..03`).

**Hasil Kueri Read-Only PostgreSQL (`QuilvianNewDevHamzah`) Pasca-Pengesahan Akhir:**

| Tabel PostgreSQL | Kunci / ID | Nilai Terverifikasi |
|---|---|---|
| `public."HrdInitialFormationBatch"` | `Id = 3089fe12-7ddb-4500-8047-6e1d98e05355` (`IFB-000001`) | `BatchStatus = 5` (`Applied`), `WorkflowDefinitionId = 6af90c7b-8398-40bc-a6f5-a1cc844015e0`, `WorkflowInstanceId = a63c5126-157f-4afd-8f39-0e022b35c99b`, `SubmittedByUserId = 838055d4-...`, `SubmittedAt = 2026-11-27 02:59:17.793829+00`, `ApprovedByUserId = 92d7c80a-...`, `ApprovedAt = 2026-11-27 03:27:47.447526+00`, `AppliedAt = 2026-11-27 03:27:47.447526+00`, `TotalRows = 1`, `ValidRows = 1`, `InvalidRows = 0`, `Notes` berisi jejak `[2026-11-27 02:59:12 UTC][Pemulihan ke Draft oleh Yudha Hermawan, S.E.]`. |
| `public."HrdInitialFormationBatchRow"` | `Id = 43f94a80-7e28-4c1b-ae13-dd9125572e26` | `FormationQuantity = 10.00`, `EffectiveStartDate = 2027-01-01`, `IsValid = true`, `CreatedFormationId = facd7888-b2b5-4601-865c-c2918140ab7e`. |
| `public."HrdPositionFormation"` | `Id = facd7888-b2b5-4601-865c-c2918140ab7e` | **`POSITION_FORMATION_COUNT_IFB000001 = 1`** (`IsDelete = false`, `IsCancel = false`): `VersionNumber = 1`, `FormationQuantity = 10.00`, `EffectiveStartDate = 2027-01-01`, `SourceType = 1`, `SourceDocumentId = 3089fe12-7ddb-4500-8047-6e1d98e05355`, `SourceDocumentNumber = IFB-000001`. |
| `public."TrxWorkflowInstance"` | `Id = a63c5126-157f-4afd-8f39-0e022b35c99b` | `WF_INSTANCE_COUNT = 1`, `WorkflowStatus = "Completed"`, `CompletedAt = 2026-11-27 03:27:47.361778+00`, `ReferenceType = "WorkforceInitialFormationBatch"`. |
| `public."TrxWorkflowStepInstance"` | 3 langkah pada instans di atas | 1) `T10_HR_MGR`: `Approved` (`2026-11-27 03:00:36.278657+00`); 2) `T10_FINANCE`: `Approved` (`2026-11-27 03:27:37.447915+00`); 3) `T10_DIRUT`: `Approved` (`2026-11-27 03:27:47.361778+00`). |

---

## 7. Ringkasan Hasil Pengujian & Catatan Transparan

### 7.1 Pemeriksaan Statis, Build, & Unit Test

| Jenis Pemeriksaan | Ruang Lingkup / Perintah | Hasil Terakhir Teramati |
|---|---|---|
| **QBE Conformance Check (`Strict`)** | `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path ...` (13 berkas Backend terkait) | **PASS** — `Files evaluated: 13`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`. |
| **xUnit Backend Workflow Tests** | `dotnet test ...\WfpBatchRunner\WfpBatchRunner.csproj --no-build --filter "FullyQualifiedName~InitialFormationBatchWorkflowTests"` | **PASS** — `Total tests: 4, Passed: 4` (`Gagal_pengajuan_tidak_menyimpan_Submitted_dan_tidak_meninggalkan_workflow_yatim`, `Ajukan_T10_dan_setujui_tiga_tingkat_membentuk_formasi_2027_01_01_secara_idempoten`, `Pengembalian_revisi_dan_rekonsiliasi_workflow_berjalan_aman`, `Pemulihan_batch_Submitted_tanpa_workflow_mengembalikan_ke_Draft_lalu_berhasil_diajukan`). *Catatan: memakai `LocalSqliteDb` lokal di file test karena isu pra-eksisting `btrim` SQLite di `PrlOpeningTaxBalanceConfiguration.cs`.* |
| **ESLint Frontend** | `npx eslint --quiet` pada seluruh 10 berkas commit `c7376fe82` | **PASS** — `0` error. |
| **Unit Test Target Frontend** | `node --import ./tests/helpers/register.mjs --test` (4 suite: `initial-formation-batch-workflow`, `business-travel-v28-gates`, `workforce-planning-settings`, `approval-inbox-legacy-actions`) | **PASS** — `22 pass, 0 fail`. *Catatan: suite penuh `npm run test:unit` sebelum ringkasan akhir mencatat `5427 pass, 149 fail` pra-eksisting di modul lain di luar ruang lingkup WFP/Approval; skrip `check-css-usage.mjs` tidak tersedia di repo Frontend.* |
| **Production Build Frontend** | `npm run build` setelah edit terakhir kode FE | **PASS** — `frontendVersion 2.2.0 valid`, `942/942` halaman statis terbangun. |

### 7.2 Live Browser Testing (Chrome Headed — Playwright)

Seluruh skrip uji dan tangkapan layar disimpan dalam `QuilvianSystemFrontendDev/test-with-agy/`:

1. **Rangkaian Alur Persetujuan Penuh (`01` s.d. `13`)**:
   - Pemulihan & pengajuan ulang (`01-batch-list-before-recover.png`, `02-ifb000001-stuck-before-recover.png`, `03-ifb000001-recovered-draft.png`, `04-ifb000001-submitted-t10.png`).
   - Matriks `T10` 3 tingkat & kotak masuk Level 1-3 termasuk guard PIN `< 6` digit dan guard tahap selesai (`05-approval-matrix-t10.png`, `06-inbox-level1-hartini.png`, `06-approval-detail-readonly-step1-done.png`, `06b-approval-detail-mobile.png`, `07-inbox-level2-finance.png`, `08-negative-pin-validation.png`, `09-after-approve-level2-finance.png`, `10-inbox-level3-dirut.png`, `11-dirut-opened-source-batch.png`, `12-after-approve-level3-completed.png`).
2. **Verifikasi Akhir Pasca-Applied & Non-Regresi (`wfp-batch-t10-verify-applied.mjs` & `wfp-batch-t10-final-verify.mjs`)**:
   - Daftar & detail `IFB-000001` berstatus `Berlaku` (`13b-batches-list-applied.png`, `13-ifb-applied-detail.png`), responsivitas mobile `390px` (`03-ifb-detail-mobile.png`), validasi form kosong `/baru` (`04-negative-empty-batch-form.png`).
   - Buku Formasi efektif `01 Jan 2027` (`14-buku-formasi-v1-10fte.png`) menampilkan `Instalasi Rawat Inap | Perawat Rawat Inap | 10 | 01 Jan 2027 | Batch Formasi Awal IFB-000001` beserta modal riwayat Versi 1 (`14b-buku-formasi-history.png`).
   - Uji idempotency API (`POST .../approve` ulang pada langkah selesai ditolak `404`, `POST .../submit` dan `POST .../recover` ulang ditolak `4xx`; jumlah baris `HrdPositionFormation` tetap `1`).
   - Non-regresi ke `Rencana Tahunan` (`15-manpower-plans-nonregression.png`) dan `Pengaturan Perencanaan` (`16-settings-nonregression.png`).
   - Stabilitas runtime pada verifikasi `wfp-batch-t10-verify-applied.mjs`: **`pageErrors = 0`, `failedApiCalls (5xx) = 0`, `notFoundUrls (404) = 0`, `forbiddenUrls (403) = 0`**.

### 7.3 Catatan Temuan Minor Transparan

1. Pada run terpisah ketika akun `direktur.utama` membuka `/hrd/approvals/matrix`, pemuatan filter dropdown unit opsional (`/organization/departments/options`) mengembalikan `403` karena peran Direktur Utama tidak diberi izin baca master unit organisasi, sementara tabel Matriks `T10` tetap tampil normal.
2. Pada detail permohonan berstatus `Completed`, field `currentStepOrder` bernilai `0` sehingga ringkasan teratas menampilkan `Tahap: 0 dari 3` (sedangkan tabel langkah menampilkan ke-3 tahap berstatus `Selesai`).
3. Pada layar `/hr/workforce-planning/initial-formation-batches/baru`, `+ Tambah Baris` memvalidasi input di sisi klien, sedangkan tombol `Simpan` kosong meneruskan validasi ke backend dengan alert inline.
4. Pada tangkapan layar non-regresi `16-settings-nonregression.png`, foto layar diambil saat indikator skeleton tabel daftar rumah sakit masih memuat (verifikasi lengkap modul Pengaturan Perencanaan berada pada `LAPORAN_IMPLEMENTASI_FIN_PENGATURAN_PERENCANAAN_SDM.md`).
