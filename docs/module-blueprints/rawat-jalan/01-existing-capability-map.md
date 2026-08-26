# Dokter / Rawat Jalan Billing - Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Blueprint revision | `11` |
| Artifact revision | `2` |
| Status | `CURRENT - scoped impact scan complete; working tree source changes included` |
| Relevant decisions | `RJ-BIL-GATE-DEC-001` sampai `RJ-BIL-GATE-DEC-009`; target `RJ-BIL-CONTRACT-001@1.0.0` |
| Backend source SHA | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend source SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| Input revision/hash | Decision revision `10`; decisions `sha256:115509A84A681646E800D7F6C3382345F31F79C13B2800B6727F356C680D4B0E` |
| Contract version | `RJ-BIL-CONTRACT-001@1.0.0` (`OWNER_APPROVED`) |
| Verified at | `2026-08-20T15:32:00+07:00` |

## Audit Boundary

Audit read-only mencakup:

- episode Rawat Jalan berbasis `EncounterId` dan sumber pembayaran encounter;
- workspace Dokter/Rawat Jalan dan finalisasi konsultasi;
- resep, pricing/coverage, status billing/pembayaran, serta workflow farmasi;
- tindakan, pricing/coverage, eksekusi/completion, FOC, cancellation, dan marker billing;
- order Laboratorium dan pencarian capability Radiologi;
- billing/folio, charge, settlement, financial correction, dan payer allocation;
- authorization, logging/audit, idempotency, reconciliation, consumer frontend, dan test evidence.

Audit tidak menilai konfigurasi runtime, data database aktual, layanan eksternal, atau perilaku deployment. Source tidak diubah oleh audit ini. Snapshot backend memakai commit `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` ditambah perubahan working tree pada `Program.cs`, `Repositories/ApplicationDbContext.cs`, dan `Areas/HealthServices/BillingManagement/Operational/**`; perubahan tersebut belum memiliki SHA commit tersendiri.

## Executive Result

Fondasi klinis Rawat Jalan cukup kuat untuk direuse: encounter, konsultasi dokter, resep, tindakan, tariff/coverage snapshot, serta halaman Dokter/Rawat Jalan sudah nyata dan saling terhubung. Working tree sekarang juga membuktikan fondasi operasional awal untuk folio dan pengenalan milestone Billing, tetapi closure ke settlement finansial belum tersedia end-to-end:

- `BillingManagement` memiliki master `MstBillingItemCategory` dan `MstPaymentMethod`, serta working tree operational `BilFolio`, `BilChargeLine`, `BilChargeComponent`, dan `BilProcessingEffect`; belum ada allocation payer, settlement, void, adjustment, reversal, refund, write-off, atau reconciliation case penuh.
- Resep memiliki `BillingId`, `BillingGeneratedAt`, serta payment status, tetapi endpoint hanya menandai status dan menerima ID billing yang tidak merujuk aggregate billing nyata.
- Tindakan memiliki snapshot harga, coverage, `BillingItemId`, dan `IsBillingGenerated`, tetapi tidak ditemukan aksi yang membuat charge atau mengubah marker billing menjadi true.
- Lab hanya memiliki create/list/detail/cancel dan belum memiliki acceptance/validation, specimen, processing, result, pricing, atau billing milestone.
- Tidak ditemukan domain/controller/service/model operasional Radiologi.
- Payment source encounter saat ini one-to-one dan model aktif hanya mengizinkan `Cash` atau `Insurance`; ini bertentangan dengan kebutuhan settlement multi-payer.

## Capability Evidence Map

| ID | Need | Owner | Evidence (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `RJ-BIL-CAP-001` | Encounter sebagai episode/transaction owner | Registration Management | `backend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#TrxPatientEncounter@9b26be3`; `backend/Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#PatientEncounterController@9b26be3` | Ready to reuse | `EncounterId` sudah menjadi referensi konsultasi, resep, tindakan, dan Lab | Rendah; tetap perlu menjaga satu encounter sebagai anchor |
| `RJ-BIL-CAP-002` | Workspace Dokter/Rawat Jalan yang reachable | Registration + Clinical Management / frontend | `frontend/src/utils/menu-sidebar/menu-items.jsx#Rawat Jalan@ab4bd83`; `frontend/src/app/health-services/registration-management/doctor-queues/page.jsx#DoctorQueuePage@ab4bd83`; `frontend/src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx#DoctorQueueView@ab4bd83` | Ready to reuse | Route, queue, SOAP, CPPT, resep, tindakan, certificate, loading, dan error state tersedia | Tidak ada tab order Lab/Radiologi atau billing summary operasional |
| `RJ-BIL-CAP-003` | Finalisasi konsultasi dan order draft | Clinical + Pharmacy Management | `backend/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs#FinalizeAsync@9b26be3`; `frontend/src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorConsultationWorkspace.js#handleConfirmFinalizeConsultation@ab4bd83` | Reuse with adapter | Finalisasi konsultasi men-submit resep dan menyelesaikan tindakan tertentu, tetapi belum mengorkestrasi billing account/charge | Risiko partial failure dan coupling lintas domain bila langsung diperluas tanpa contract |
| `RJ-BIL-CAP-004` | Resep dan pricing/coverage snapshot | Pharmacy Management | `backend/Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs#TrxPrescription@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionItem.cs#TrxPrescriptionItem@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs#PrescriptionWorkspaceService@9b26be3` | Ready to reuse | Header/item/compound, harga, coverage, patient pay, dan `EncounterId` tersedia | Snapshot perlu dikunci terhadap lifecycle charge yang belum ada |
| `RJ-BIL-CAP-005` | Resep menjadi charge setelah finalisasi | Pharmacy + Billing Management | `backend/Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs#MarkBillingGeneratedAsync@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#MarkBillingGenerated@9b26be3` | Extend | Sudah ada gate `Submitted`, `BillingId`, `BillingGeneratedAt`, dan payment status; belum ada pembentukan charge/folio atau referential contract ke billing aggregate | Endpoint memakai permission generik `Prescription.Update`; caller dapat menandai billing tanpa bukti charge nyata |
| `RJ-BIL-CAP-006` | Workflow fulfillment resep terpisah dari billing | Pharmacy Management | `backend/Areas/HealthServices/PharmacyManagement/Models/TrxPrescriptionPreparation.cs#TrxPrescriptionPreparation@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Enums/PrescriptionFulfillmentStatus.cs#PrescriptionFulfillmentStatus@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Services/PrescriptionPreparationService.cs#PrescriptionPreparationService@9b26be3` | Ready to reuse | Review, preparation, final check, dan fulfillment terpisah dari payment status | Harus dijaga agar penyerahan obat tidak menjadi trigger charge awal |
| `RJ-BIL-CAP-007` | Tindakan, execution/completion, dan snapshot harga | Clinical Management | `backend/Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs#TrxPatientProcedure@9b26be3`; `backend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#Execute@9b26be3`; `backend/Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureStatus.cs#PatientProcedureStatus@9b26be3` | Ready to reuse | Status `Planned/Ordered/InProgress/Completed/Cancelled`, execution actor/time, tariff, coverage, FOC, dan patient pay tersedia | Existing `ExecuteImmediately` dapat langsung membuat `Completed`; authorization dan milestone harus tetap server-authoritative |
| `RJ-BIL-CAP-008` | Charge tindakan pada `Completed` | Clinical + Billing Management | `backend/Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs#IsBillingGenerated@9b26be3`; `backend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#PatientProcedureController@9b26be3` | Extend | Marker `BillingItemId`, `IsBillingGenerated`, dan `BillingGeneratedAt` tersedia, tetapi tidak ditemukan producer/action charge | Marker dapat tetap false selamanya dan belum ada exactly-once guarantee |
| `RJ-BIL-CAP-009` | Cancellation tindakan sebelum financialization | Clinical Management | `backend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#Cancel@9b26be3`; `backend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#RemoveDraft@9b26be3` | Reuse with adapter | Guard menolak update/cancel tertentu ketika executed atau billing generated, menyimpan reason/actor/time | Belum ada cancellation request setelah unit mengambil alih atau actual-consumption decision |
| `RJ-BIL-CAP-010` | Order dan acceptance milestone Laboratorium | Laboratory Management | `backend/Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs#LabOrder@9b26be3`; `backend/Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs#LabOrderService@9b26be3`; `backend/Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs#LabOrderController@9b26be3` | Repair | Create/list/detail/cancel dan validasi procedure Lab tersedia; tidak ada status acceptance/validation, specimen, processing, result, price, coverage, atau billing | Cancellation saat ini selalu diperbolehkan selama belum `IsCancel`, tanpa actual-consumption guard |
| `RJ-BIL-CAP-011` | Radiologi order, acquisition, result, dan billing milestone | Diagnostic/Radiology owner belum ada | Pencarian terarah `backend/Areas/HealthServices/**` pada `@9b26be3`; hanya flag master procedure/tariff ditemukan | Missing | Tidak ditemukan model, service, controller, lifecycle, atau consumer operasional Radiologi | Milestone acquisition dan billing tidak dapat direalisasikan dari capability existing |
| `RJ-BIL-CAP-012` | Billing account/folio per encounter dan charge lines | Billing Management | `backend/Areas/HealthServices/BillingManagement/Operational/Models/BilFolio.cs#BilFolio@9b26be3+WT`; `backend/Areas/HealthServices/BillingManagement/Operational/Models/BilChargeLine.cs#BilChargeLine@9b26be3+WT`; `backend/Areas/HealthServices/BillingManagement/Operational/Controllers/BillingFolioController.cs#BillingFolioController@9b26be3+WT`; `backend/Areas/HealthServices/BillingManagement/Operational/Services/BillingFolioService.cs#BillingFolioService@9b26be3+WT` | Extend | Folio unik per encounter, charge line/component, query folio, dan internal milestone recognition sudah ada di working tree; belum ada payer allocation, payment, correction, close approval, migration evidence, atau committed SHA | Jangan menganggap folio awal sebagai billing end-to-end; source masih berubah dan perlu validation |
| `RJ-BIL-CAP-013` | Multi-payer allocation dan patient excess dalam satu folio | Registration/Billing/Insurance | `backend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs#TrxPatientEncounterGuarantor@9b26be3`; `backend/Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs#EncounterPaymentType@9b26be3`; `backend/Areas/HealthServices/RegistrationManagement/Enums/PatientEncounterGuarantorRole.cs#PatientEncounterGuarantorRole@9b26be3` | Conflict | Active navigation/DB contract adalah one-to-one payment source dan hanya `Cash/Insurance`, sementara enum role/type lain mengisyaratkan model multi-payer yang belum terhubung | Bertentangan dengan `RJ-BIL-DEC-003A`; jangan memilih model persistence sebelum owner/domain contract disetujui |
| `RJ-BIL-CAP-014` | Void, adjustment, reversal, refund, FOC, dan write-off | Billing/Finance | Pencarian terarah `backend/Areas/HealthServices/BillingManagement/**@9b26be3`; `MstPaymentMethod.IsAvailableForRefund` hanya metadata master | Missing | Tidak ada transaction, status, approval, immutable history, atau endpoint financial correction | Blocker `RJ-BIL-DEC-004`, `RJ-BIL-DEC-005`, dan acceptance criteria finansial |
| `RJ-BIL-CAP-015` | Authorization berbasis capability dan status | Security + domain owners | `backend/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#AccessPermission@9b26be3`; `backend/Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs#AccessPermission@9b26be3`; `backend/Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs#AccessPermission@9b26be3` | Reuse with adapter | `[Authorize]`, `AccessAction`, dan `AccessPermission` tersedia | Permission masih CRUD/generik; belum ada capability/SOD khusus posting, void, reversal, refund, FOC, write-off, atau supervisor approval |
| `RJ-BIL-CAP-016` | Audit trail klinis dan finansial | Logging + domain owners | `backend/Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs#LoggerService@9b26be3`; `backend/Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs#LoggerService@9b26be3`; identity audit fields pada model terkait | Reuse with adapter | Actor/time/reason tersedia pada beberapa transition dan operational logging dipanggil | Belum ada append-only financial ledger atau bukti audit immutability untuk correction |
| `RJ-BIL-CAP-017` | Exactly-once charge, idempotency, dan partial-failure reconciliation | Billing/integration owner | `backend/Areas/HealthServices/BillingManagement/Operational/Models/BilProcessingEffect.cs#BilProcessingEffect@9b26be3+WT`; `backend/Areas/HealthServices/BillingManagement/Operational/Services/BillingFolioService.cs#RecognizeMilestoneAsync@9b26be3+WT`; unique configuration pada `BillingOperationalConfigurations.cs@9b26be3+WT` | Reuse with adapter | Idempotency key, request fingerprint, serializable retry, duplicate detection, version conflict, dan `OutcomeUnknown` sudah ada; reconciliation case, dead-letter ownership, recovery report, dan full partial-failure workflow belum ada | Dapat direuse untuk boundary processing setelah review; belum cukup untuk menutup folio atau menyatakan rekonsiliasi selesai |
| `RJ-BIL-CAP-018` | Frontend resep/tindakan di workspace dokter | Frontend Health Services | `frontend/src/components/view/health-services/registration-management/doctor-queues/tabs/prescription/doctor-prescription-tab.jsx#DoctorPrescriptionTab@ab4bd83`; `frontend/src/components/view/health-services/registration-management/doctor-queues/tabs/procedure/doctor-procedure-tab.jsx#DoctorProcedureTab@ab4bd83`; service clients terkait | Ready to reuse | Draft/autosave/finalize integration dan loading/error handling tersedia | Tidak boleh dijadikan financial source of truth |
| `RJ-BIL-CAP-019` | Frontend order Lab/Radiologi dan status milestone | Frontend Health Services | Pencarian terarah pada doctor queue di `frontend@ab4bd83`; referensi Lab/Radiologi hanya display CPPT/master metadata | Missing | Tidak ada service/hook/tab order operasional Lab atau Radiologi dari workspace dokter | Klaim bahwa penunjang sudah tersedia tidak terbukti untuk journey order operasional |
| `RJ-BIL-CAP-020` | Frontend billing/folio, payer split, dan correction status | Frontend Health Services/Billing | Pencarian terarah doctor queue dan health-services consumers di `frontend@ab4bd83` | Missing | Tidak ada consumer billing account, charge status, payer allocation, void/reversal/refund, atau reconciliation | UI belum dapat membedakan clinical order dari final financial charge secara end-to-end |
| `RJ-BIL-CAP-021` | Automated verification untuk journey closure billing | QA/domain owners | Tidak ditemukan test project backend atau test/spec relevan frontend pada snapshot audit | Missing | Belum ada evidence test untuk transition, duplicate, cancellation, partial failure, authorization, atau payer allocation | Risiko regresi tinggi sebelum delivery |
| `RJ-BIL-CAP-022` | External payer/claim integration | Insurance/Corporate Relation + Integration owner | Tidak ada integration contract atau reliability profile dalam boundary audit | Unknown | Nama sistem, owner, protocol, idempotency, timeout, retry, reconciliation, dan claim lifecycle belum tersedia | Stop pada `Unknown`; memerlukan evidence eksternal/manusia |

## As-Is Contracts

### Encounter and payment source

- `TrxPatientEncounter` adalah episode owner dan seluruh capability klinis yang diaudit memakai `EncounterId`.
- `TrxPatientEncounterGuarantor` didokumentasikan source sebagai payment source one-to-one milik encounter.
- Contract aktif membatasi `EncounterPaymentType` ke `Cash` atau `Insurance`; `Company`, `BPJS`, secondary payer, excess payer, dan co-payment role belum menjadi contract operasional encounter.

### Prescription

- Resep dibuat `Draft/NotBilled` dan hanya dapat diedit pada state tersebut.
- Finalisasi konsultasi mengubah resep menjadi `Submitted`.
- `PATCH .../{id}/billing-generated` menerima optional `BillingId`, lalu mengubah payment status menjadi `WaitingForPayment` dan mengisi `BillingGeneratedAt`; endpoint tidak membuat billing transaction.
- Payment dapat ditandai `Paid`, `InsuranceApproved`, atau `PaymentWaived` dari controller Pharmacy dengan permission `Prescription.Update`.
- Cancellation workflow hanya mengizinkan resep yang masih `Draft/NotBilled` dan belum diproses unit farmasi.

### Procedure

- Tindakan mempunyai `Planned`, `Ordered`, `InProgress`, `Completed`, dan `Cancelled`.
- Pricing dan coverage di-snapshot saat selection; `ExecuteImmediately` dapat langsung menghasilkan `Completed`.
- `IsBillingGenerated` dan `BillingItemId` ada pada model, tetapi tidak ditemukan producer charge atau endpoint billing-generated.
- Update/cancel/remove memiliki beberapa guard terhadap executed/billing-generated state, tetapi belum mencakup actual-consumption workflow setelah partial processing.

### Laboratory and Radiology

- Lab order hanya menyimpan `EncounterId` dan `ProcedureId` di atas base audit/cancel fields.
- Lab tidak mempunyai state acceptance/validation/specimen/processing/result/billing.
- Radiologi belum mempunyai operational domain capability.

### Billing and financial correction

- Billing operational aggregate tidak ada. Karena itu belum ada as-is contract untuk folio per encounter, charge lifecycle, settlement, posting, claim, void, adjustment, reversal, refund, FOC, write-off, atau reconciliation.

## Frontend/Backend Mismatches and Conflicts

1. `RJ-BIL-CONFLICT-001`: keputusan multi-payer dalam satu folio bertentangan dengan payment source encounter one-to-one dan enum aktif `Cash/Insurance`.
2. `RJ-BIL-CONFLICT-002`: `Prescription.BillingId` dan `PatientProcedure.BillingItemId` mengisyaratkan billing transaction, tetapi aggregate/table/API pemilik ID tersebut tidak ditemukan.
3. `RJ-BIL-CONFLICT-003`: jawaban closure menyatakan Lab dan Radiologi tersedia di menu dokter; source frontend hanya membuktikan resep dan tindakan sebagai tab order. Lab/Radiologi muncul sebagai label CPPT/master metadata, bukan journey order operasional.
4. `RJ-BIL-CONFLICT-004`: Lab cancellation dapat dilakukan selama order belum dibatalkan tanpa memeriksa acceptance, specimen, processing, atau actual consumption.
5. `RJ-BIL-CONFLICT-005`: payment/waiver resep dapat ditandai melalui permission generik `Prescription.Update`, sedangkan keputusan memerlukan pemisahan clinical correction dan financial authority.

## Closure Questions and Blockers

| ID | Question/blocker | Owner | Effect |
|---|---|---|---|
| `RJ-BIL-TRACE-OQ-001` | Domain mana yang menjadi canonical owner billing account/folio dan charge lifecycle? Registry owner/prefix wajib disetujui sebelum new code | Product/domain owner + Finance/Billing + architecture governance | Blocks target architecture |
| `RJ-BIL-TRACE-OQ-002` | Apakah payment source one-to-one akan diganti/diperluas, atau payer allocation menjadi child dari folio sementara encounter menyimpan primary registration payer? | Registration + Billing/Finance + Insurance/Corporate Relation | Blocks multi-payer contract |
| `RJ-BIL-TRACE-OQ-003` | Siapa pemilik dan state minimum Laboratory acceptance/specimen/processing serta Radiology acceptance/acquisition/result? | Lab, Radiologi, Clinical Governance | Blocks diagnostic milestone design |
| `RJ-BIL-TRACE-OQ-004` | Apa formula actual consumption, unit, rounding, minimum charge, dan component eligibility per layanan? | Unit layanan + Finance/Billing | Blocks partial-charge design |
| `RJ-BIL-TRACE-OQ-005` | Capability dan maker-checker apa yang diwajibkan untuk posting, void, adjustment, reversal, refund, FOC, dan write-off? | Finance/Billing + Security | Blocks permission/approval contract |
| `RJ-BIL-TRACE-OQ-006` | Sistem payer/claim eksternal apa yang digunakan dan siapa owner reliability contract-nya? | Insurance/Corporate Relation + Integration owner | External integration remains `Unknown` |
| `RJ-BIL-TRACE-OQ-007` | Apakah endpoint Pharmacy yang menandai `Paid/InsuranceApproved/PaymentWaived` akan tetap authoritative atau hanya menerima event dari Billing? | Pharmacy + Finance/Billing | Blocks ownership and security boundary |

## Impact-Scan Trigger

Map ini hanya current untuk backend `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` dan frontend `ab4bd836e05c72d0679e02899258f3773f3869a2`. Jika salah satu SHA berubah, tandai artefak `STALE` dan lakukan scoped impact scan pada encounter, consultation finalization, prescription, procedure, Lab, Radiology, BillingManagement, payer, authorization, frontend doctor queue, dan test evidence sebelum map dipakai kembali.

## Scoped Impact Scan 2026-08-20

### Working tree delta

Perubahan yang terlihat tetapi belum memiliki commit SHA:

| Area | Bukti | Hasil audit |
|---|---|---|
| Dependency registration | `Program.cs` mendaftarkan `BillingFolioService` dan `LabOrderService` | Wiring Billing awal ada di working tree; build/test belum dijadikan evidence pada scan ini |
| Persistence registration | `Repositories/ApplicationDbContext.cs` mendaftarkan `BilFolios`, `BilChargeLines`, `BilChargeComponents`, dan `BilProcessingEffects` | Model sudah masuk DbContext; migration tidak ditemukan pada audit terarah |
| Billing operational models | `Areas/HealthServices/BillingManagement/Operational/Models/**` | Folio, charge line, component, dan processing effect tersedia sebagai source baru/working tree |
| Billing operational configuration | `Areas/HealthServices/BillingManagement/Operational/Configurations/BillingOperationalConfigurations.cs` | Unique index encounter/identity dan `Restrict` relationship didefinisikan |
| Billing operational API | `BillingFolioController` | `GET /by-encounter/{encounterId}`, `GET /{folioId}`, dan `POST /internal/milestones/recognize` tersedia di working tree |
| Billing operational service | `BillingFolioService.RecognizeMilestoneAsync` | Ada serializable transaction, idempotency, fingerprint, duplicate detection, version conflict, dan `OutcomeUnknown` |

### Dampak terhadap capability

1. `RJ-BIL-CAP-012` berubah dari `Missing` menjadi `Extend` untuk folio/charge awal saja.
2. `RJ-BIL-CAP-017` berubah dari `Missing` menjadi `Reuse with adapter` untuk processing
   idempotency saja.
3. `RJ-BIL-CAP-013`, `014`, `022` tetap `Conflict`, `Missing`, dan `Unknown` sesuai scope
   masing-masing; source baru tidak membuktikan payer allocation, correction, settlement,
   atau external integration.
4. `RJ-BIL-CAP-021` tetap `Missing` karena tidak ditemukan test evidence pada scan terarah.
5. Working tree belum dapat dianggap release evidence. Commit, migration, build, test,
   authorization review, dan database validation tetap menjadi gate terpisah.

### Kontrak AS-IS Billing Operational yang teramati

| Endpoint | Method | Hak akses | Response | Status evidence |
|---|---|---|---|---|
| `api/v1/health-services/billing-management/folios/by-encounter/{encounterId}` | `GET` | `[AccessPermission("BillingFolio", "Read")]` | `ApiResponse<BillingFolioDetailResponse>` | Working tree, belum committed |
| `api/v1/health-services/billing-management/folios/{folioId}` | `GET` | `[AccessPermission("BillingFolio", "Read")]` | `ApiResponse<BillingFolioDetailResponse>` | Working tree, belum committed |
| `api/v1/health-services/billing-management/folios/internal/milestones/recognize` | `POST` | `[AccessPermission("BillingMilestone", "RecognizeInternal")]` | `ApiResponse<RecognizeBillingMilestoneResponse>` | Working tree, system-only action, belum committed |

Endpoint tersebut belum membuktikan payment, payer allocation, financial correction, atau
production readiness. `RecognizeInternal` hanya boleh menerima fact yang telah diotorisasi oleh
producer internal; ia tidak boleh menjadi endpoint klinis untuk menetapkan `Paid`, `InsuranceApproved`,
atau `PaymentWaived`.

### Rekomendasi status

Capability map dapat dipakai kembali untuk domain/blueprint design dengan batas bukti
`commit SHA + working tree delta`. Sebelum implementation planning, lakukan preflight builder
terhadap source change ini, migration, build, test, dan permission review. Jika working tree
dibersihkan atau commit SHA berubah, jalankan impact scan ulang.

## Recommended Next Skill

Gunakan `requirement-completeness-gate` untuk menilai apakah conflict dan open question di atas menghalangi domain architecture. Jangan menjalankan `design-business-module`, delivery planning, atau implementation sebelum ownership billing, multi-payer contract, diagnostic milestones, financial authority, dan approval evidence cukup.
