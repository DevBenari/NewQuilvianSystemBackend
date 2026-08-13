# IGD — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `IGD` |
| Revision | `1` |
| Backend root/SHA | `QuilvianBackend` / `fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Frontend root/SHA | `QuilvianFrontEnd` / `e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` |
| Audit boundary | Source-only audit. Runtime composition, database contents and migration application state, seed/configuration values, external diagnostic/payment services, and production authorization assignments were not inspected. A migration file is evidence of source schema only, not deployment. |
| Decision baseline | The map traces the requirements recorded in `QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:958–1010 (IGD-DEC-016 through IGD-DEC-038), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`. |
| Snapshot validity | An impact scan from the earlier backend snapshot `36d7eca7cd3d4b3f1f6520a6fe9340936cced320` to this SHA found only a change to the decision log; application source was unchanged. |

The status in each row of the capability register is exactly one of the status terms defined by
`trace-existing-capabilities`. `Missing` conclusions are source-scan conclusions: no matching
model, API, or consumer was found in the audited application source; they are not a claim about
an external service or deployed database.

## Journey yang Ditelusuri

```text
arrival / unknown identity
  -> patient + encounter / emergency visit
  -> triage and retriage
  -> observation, resuscitation, and shared clinical records
  -> disposition
  -> transfer or discharge / completion
  -> billing, financial release, and late-result follow-up

cross-cutting: authorization, audit, approval, idempotency, and integration reliability
```

The journey is assessed against the single-episode, provisional-identity, append-only clinical,
separate disposition/transfer/completion/release, and backend-enforced authority decisions in
`QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:958–1010 (Decision Log), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.

## Capability Register

| ID | Kebutuhan | Owner existing | Existing evidence | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| CAP-01 | Emergency visit sebagai extension satu encounter | Emergency Installation Management + Registration Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:21–67 (TrxEmergencyVisit), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyVisitConfiguration.cs:29–43 (TrxEmergencyVisitConfiguration), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Extend | Entity, FK, API, and unique `EncounterId` already exist; lifecycle and closure rules do not yet represent the decided IGD semantics. | A generic update/delete path can alter or retire a clinical visit without governed correction. |
| CAP-02 | Provisional encounter dan unknown patient pada `EncounterId` yang sama | Registration Management + Emergency Installation Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:21–67 (nullable EncounterId/PatientId and unknown-patient fields), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:25–29 (required PatientId), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:42–130 (CreateAsync), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Conflict | Emergency visit can carry partial identity, but the required registration encounter requires a real patient; this cannot satisfy the decided same-encounter temporary identity flow. | A patient needing immediate care can be blocked or forced into a duplicate/master-patient workaround. |
| CAP-03 | Temporary identity reconciliation, merge, reversal, and audit | Patient Management | `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:109–132 (MergedToPatientId/MergeReason), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs:709–794 (generic patient update), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs:2358–2395 (merge target validation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Repair | A merge pointer exists, but no temporary-identity lifecycle, candidate reconciliation, maker-checker, reversal workflow, idempotency, or controlled audit evidence was found. | Generic update can create an unreviewed link that does not preserve the required identity decision history. |
| CAP-04 | Four-colour triage, retriage, and SLA evidence | Emergency Installation Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyTriageSystem.cs:3–7 (EmergencyTriageSystem), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs:15–43 (TrxEmergencyTriage), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyTriageConfiguration.cs:27–31,48–51 (unique sequence and prior-triage relation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Repair | Sequence and previous-triage relation exist, but source-of-truth remains ATS/ESI; generic update/delete permits rewriting history; SLA is calculated from static master minutes rather than versioned policy/clocks. | Retriage and response measurement can contradict the clinical governance decisions. |
| CAP-05 | Observation, resuscitation, and emergency procedure detail | Emergency Installation Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyObservation.cs:10–22 (TrxEmergencyObservation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyResuscitation.cs:10–22 (TrxEmergencyResuscitation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyProcedureDetail.cs:10–15 (TrxEmergencyProcedureDetail), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Extend | These records are already owned by IGD and linked to `EmergencyVisitId`; their mutation/correction and shared-clinical references still need alignment. | Generic CRUD can make clinically material history mutable. |
| CAP-06 | Vital sign, CPPT, assessment, diagnosis, and procedure without duplication | Clinical Management | `QuilvianBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs:25–40 (TrxPatientVitalSign), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs:25–42 (TrxPatientIntegratedProgressNote), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs:20–40 (TrxPatientAssessment), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Reuse with adapter | The shared clinical owner should remain Clinical Management. Its records require patient/encounter and generally consultation context, so it cannot serve a temporary identity until the provisional context is supported. | Copying these records into IGD would create competing clinical sources of truth. |
| CAP-07 | Disposition distinct from clinical completion | Emergency Installation Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyDisposition.cs:16–80 (TrxEmergencyDisposition), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs:23–110 (CreateAsync and transition rules), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyDispositionController.cs:276–324 (status patch), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Repair | A disposition record and draft/confirmed/executed states exist, but the status patch can execute and complete a visit without repeating the creation gate; clinician privilege and immutable correction were not evidenced. | Disposition can bypass registration/closure gating and collapse decision with completion. |
| CAP-08 | Transfer Requested → Accepted → Departed → Arrived with separate authority | Emergency Installation Management | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTransfer.cs:14–63 (TrxEmergencyTransfer timestamps), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyTransferStatus.cs:3–11 (EmergencyTransferStatus), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTransferService.cs:26–92 (CreateAsync/transition rules), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Repair | Model stores departed/arrived timestamps, but enum/service use `InTransit`/`Completed`; no source/destination actor context is enforced and generic update can move any state. | The receiving/sending separation in the decision log is not protected. |
| CAP-09 | Billing completion, financial clearance, release, deposit, and exception | Billing Management | `QuilvianBackend — Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs:14–49 (MstPaymentMethod), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/BillingManagement/MasterData/Models/MstBillingItemCategory.cs:14–61 (MstBillingItemCategory), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Missing | Only billing master-data evidence was found; no transactional invoice, charge, payment, receivable, clearance, release, deposit allocation, or controlled exception workflow was found. | Financial release rules cannot be enforced or audited from the current source. |
| CAP-10 | Prescription/pharmacy handoff | Pharmacy Management | `QuilvianBackend — Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs:25–69 (TrxPrescription), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs:262–313 (Create validation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Reuse with adapter | Prescription is an existing clinical owner, but its creation validates consultation and encounter context. | An IGD-specific duplicate prescription store would split dispensing ownership. |
| CAP-11 | Lab/radiology ordering and late-result follow-up | No owner evidenced in application source | Source scans for `Laboratory`, `Lab`, `Radiology`, and diagnostic result APIs/controllers in both repositories produced no implementation candidate at the audited SHAs. The required workflow is recorded in `QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:1004 (IGD-DEC-032), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`. | Missing | No source owner or contract was identified for order/result, criticality, acknowledgment, coverage, or escalation. | Late clinical results have no traced delivery or follow-up capability. |
| CAP-12 | Context-aware clinical/business authorization | Security / authorization infrastructure | `QuilvianBackend — Attributes/AccessPermissionAttribute.cs:6–18 (AccessPermissionAttribute), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Filters/AccessPermissionFilter.cs:28–76 (AccessPermissionFilter), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Services/Security/AccessPermissionService.cs:22–150 (AccessPermissionService), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Conflict | Existing controller/action RBAC has a universal `SuperAdmin` bypass and no evidenced unit/resource, clinical credential, privilege, maker-checker, or transfer-side context validation. | Broad technical authority can exceed the decision-log clinical/business authority model. |
| CAP-13 | Approval/maker-checker workflow candidate | HR Workflow Management | `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs:15–74 (TrxWorkflowInstance), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowApproverAssignment.cs:14–65 (TrxWorkflowApproverAssignment), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs:374–390,1024–1043 (idempotency and assigned-approver validation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Reuse with adapter | The workflow foundation has correlation/idempotency and approver assignment, but it does not prove IGD-specific capability, different-maker, credential, resource-context, or privacy controls. | Treating a HR workflow as sufficient without an adapter would leave clinical governance unenforced. |
| CAP-14 | Transactional outbox/inbox and cross-module reconciliation | No owner evidenced in application source | A source scan for technical `outbox`, processed-message/inbox, integration-event, and broker components found no transactional implementation candidate. The reliability requirement is in `QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:1005 (IGD-DEC-033), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`. | Missing | No local outbox/inbox or cross-module reconciliation mechanism was evidenced. | Client retries and partial failures can produce divergent registration/clinical/financial state. |
| CAP-15 | Mutation audit without full PHI payload | Shared identity/logging infrastructure | `QuilvianBackend — Models/IdentityModel.cs:5–23 (IdentityModel audit fields), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Services/Logging/LoggerService.cs:19–37,110–164 (LoggerService), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyVisitController.cs:224–229 (InfoAsync usage), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Extend | Timestamp/user metadata and logging exist, but immutable domain audit, evidence references, approval history, and PHI classification/redaction enforcement were not evidenced. | Generic mutable audit fields/log calls do not prove compliance for high-impact clinical mutations. |
| CAP-16 | IGD service runtime wiring | Application host | `QuilvianBackend — Program.cs:259–281 (service registrations), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyVisitController.cs:40–52 (EmergencyVisitService injection), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Repair | Emergency controllers inject emergency services, but no matching emergency service registration was found in the application source scan. | A valid controller/service implementation can still fail at runtime through unresolved DI. |
| CAP-17 | Reachable frontend emergency registration | Frontend Registration Management | `QuilvianFrontEnd — src/app/health-services/registration-management/emergency-registration/page.jsx:1–5 (page route), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:798–928 (handleSubmitRegistration), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | Conflict | The route is reachable, but it creates a full patient/encounter before the emergency visit and sends contract values that conflict with the backend. | An emergency registration user can hit rejected requests or leave a partial encounter after a failed second call. |
| CAP-18 | Frontend clinical IGD workspace (triage, observation, transfer, disposition, resuscitation) | No owner evidenced in frontend source | Source scans for consumers of `emergency-triages`, `emergency-transfers`, `emergency-dispositions`, `emergency-observations`, and `emergency-resuscitations` found no App Router/page or API-client implementation at commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e. The only evidenced route is `QuilvianFrontEnd — src/app/health-services/registration-management/emergency-registration/page.jsx:1–5 (page route), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`. | Missing | Existing backend endpoints have no traced clinical frontend consumer. | A backend-only workflow cannot be assumed usable by IGD staff. |
| CAP-19 | Mass-casualty/disaster mode | No owner evidenced in application source | Source scans for `mass casualty`, `disaster`, `bencana`, and incident-mode terms in the IGD backend/frontend scope returned no implementation. Initial-release scope is decided in `QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:948 (IGD-DEC-009), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`. | Missing | No incident context, activation state, confirmation, tag, or associated UI/API was identified. | A committed scope item has no traced implementation capability. |

## Backend Inventory

### Emergency domain and persistence

- `ApplicationDbContext` registers emergency setting, visit, triage, observation, resuscitation,
  disposition, transfer, and related transaction sets in
  `QuilvianBackend — Repositories/ApplicationDbContext.cs:581–601 (ApplicationDbContext DbSets), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- A source migration creates the emergency-installation tables in
  `QuilvianBackend — Migrations/20260804071642_initializeEmergencyInstallationManagement.cs:1–end (InitializeEmergencyInstallationManagement migration), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
  It was not used as evidence that the migration is applied anywhere.
- `TrxEmergencyVisit` has nullable patient/encounter fields, status fields, and arrival/identity
  attributes, while its configuration enforces unique `EncounterId` and restricted FKs:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:21–67 (TrxEmergencyVisit), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Repositories/Configurations/HealthService/EmergencyInstallationManagement/TrxEmergencyVisitConfiguration.cs:29–43 (TrxEmergencyVisitConfiguration), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.

### Provider contracts and mutation behaviour

- Emergency-visit creation validates setting, patient/service-unit/encounter relationships, and
  currently requires `EncounterType.Outpatient`:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:42–130 (CreateAsync), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
  The controller exposes generic update, transition, and soft-delete paths in
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyVisitController.cs:163–405 (Create, Update, UpdateStatus, Delete), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Triage creation uses master-level SLA values, but its DTO accepts sequence, status, system, and
  timestamps; generic update/delete can rewrite or remove a triage record:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyTriageDtos.cs:38–100 (CreateEmergencyTriageRequest), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs:158–383 (Create, Update, Delete), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Disposition has a Draft → Confirmed → Executed service transition, but the status endpoint can
  execute/complete after creation without replaying the creation validation:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs:23–110 (EmergencyDispositionService), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyDispositionController.cs:276–324 (UpdateStatus), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Transfer persistence already includes requested, accepted, departed, and arrived timestamps,
  while the current service transition is Requested → Accepted → InTransit → Completed:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTransfer.cs:14–63 (TrxEmergencyTransfer), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTransferService.cs:75–92 (ValidateStatusTransition), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.

### Reusable upstream/downstream owners

- Registration owns `TrxPatientEncounter` and requires a patient reference:
  `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:25–29 (TrxPatientEncounter), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:1160–1206 (Create), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Patient Management owns the master patient and currently exposes merge fields through generic
  patient update, rather than a dedicated reconciliation API:
  `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:109–132 (MstPatient), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs:709–794 (Update), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Clinical Management owns shared clinical records; Pharmacy owns prescriptions and validates
  consultation/encounter context:
  `QuilvianBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs:17–31 (TrxPatientDiagnosis), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs:262–313 (Create), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.

### Cross-cutting infrastructure

- The authorization path is action-level permission filtering. It grants a broad `SuperAdmin`
  bypass and does not evidence clinical-privilege use in IGD. The available privilege model is in
  `QuilvianBackend — Areas/Corporate/HumanResource/CredentialingManagement/Models/WfpClinicalPrivilege.cs:16–68 (WfpClinicalPrivilege), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`, while the permission implementation is in
  `QuilvianBackend — Services/Security/AccessPermissionService.cs:22–150 (AccessPermissionService), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- The HR workflow has useful instance/assignment/idempotency behaviours, but its evidence is not
  a clinical approval contract:
  `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs:15–74 (TrxWorkflowInstance), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs:1024–1043 (assigned approver check), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- No targeted IGD test/spec filename was found in either repository at the audited SHA. This is a
  scan result, not proof that untracked or external tests do not exist.

## Frontend Inventory

- The only confirmed reachable IGD page is the registration route, also referenced from the menu:
  `QuilvianFrontEnd — src/app/health-services/registration-management/emergency-registration/page.jsx:1–5 (default page), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/utils/menu-sidebar/menu-items.jsx:882 (emergency-registration menu item), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
- Its page copy says administration must complete before nursing triage:
  `QuilvianFrontEnd — src/components/view/health-services/registration-management/emergency-registration/emergency-registration-page.jsx:18–30 (EmergencyRegistrationPage), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
  That is incompatible with the patient-safety/provisional decision baseline.
- The API client has separate endpoints and HTTP calls for patient encounter then emergency visit:
  `QuilvianFrontEnd — src/lib/services/health-services/registration-management/emergency-registration.service.js:22–40 (EMERGENCY_REGISTRATION_API_URLS), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/lib/services/health-services/registration-management/emergency-registration.service.js:299–332 (createEmergencyPatientEncounter/createEmergencyVisit), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
- The hook submits those writes sequentially. It retains `completedEncounter` for a second-call
  retry, but has no stable client/server idempotency key or one local backend command:
  `QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:798–928 (handleSubmitRegistration), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/lib/state/slice/health-services/registration-management/emergency-registration-slice.jsx:340–369 (submission states), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
- Service-unit selection has frontend string/code heuristics, including `SU-ER-001` and `IGD`,
  rather than an evidenced authoritative emergency-setting contract:
  `QuilvianFrontEnd — src/lib/services/health-services/registration-management/emergency-registration.service.js:148–164 (isEmergencyServiceUnit), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
- New-patient registration requires identity, phone, address, and geographic fields before the
  user can progress. The unknown-patient toggle is only present in the later visit step:
  `QuilvianFrontEnd — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:394–415 (required fields), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:519–601 (handlePatientStepNext), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/components/view/health-services/registration-management/emergency-registration/emergency-visit-step.jsx:545–579 (unknown patient UI), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
- No clinical IGD route/client consumer was identified for the emergency triage, transfer,
  disposition, observation, or resuscitation endpoint names. Existing legacy-looking `/IGD/`
  strings in `config.jsx` are not treated as active routes because no matching App Router route
  was found.

## Reuse dan Ownership Map

| Data/capability | Existing owner | Reuse boundary | Why IGD should not duplicate it | Evidence |
|---|---|---|---|---|
| Master patient | Patient Management | Reuse definitive `MstPatient` after controlled reconciliation; do not reuse generic merge update as the workflow. | A duplicate master-patient owner breaks duplicate prevention and history. | `QuilvianBackend — Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:109–132 (MstPatient), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Encounter/admission | Registration Management | Reuse one encounter as the episode anchor, subject to resolving provisional identity support. | A second IGD encounter would violate the decided same-episode linkage. | `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:25–29 (TrxPatientEncounter), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| IGD-specific extension | Emergency Installation Management | Retain `TrxEmergencyVisit` and its child observations, resuscitation, triage, disposition, and transfer as IGD-owned data. | Rebuilding the established IGD extension would duplicate the current relationship graph. | `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:21–67 (TrxEmergencyVisit), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Shared clinical facts | Clinical Management | Reference existing vital signs, CPPT, assessment, diagnoses, and procedures through an IGD-compatible clinical context. | Duplicate clinical records create competing clinical source-of-truth and audit trails. | `QuilvianBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs:25–40 (TrxPatientVitalSign), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Prescription/dispensing | Pharmacy Management | Integrate through the existing encounter/consultation contract. | A separate IGD prescription record would split pharmacy fulfilment. | `QuilvianBackend — Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs:25–69 (TrxPrescription), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Approval engine candidate | HR Workflow Management | Adapt workflow instance/assignment only after IGD authority and separation rules are verified. | An ad-hoc approval table would duplicate reusable idempotency/assignment primitives. | `QuilvianBackend — Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowApproverAssignment.cs:14–65 (TrxWorkflowApproverAssignment), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Permission infrastructure | Security | Reuse permission evaluation only if its action-level model is extended to the required context and segregation controls. | Replacing it without tracing users/roles would risk parallel authorization systems. | `QuilvianBackend — Services/Security/AccessPermissionService.cs:22–150 (AccessPermissionService), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |
| Billing and diagnostics | No transactional owner traced | Do not create IGD-owned financial or diagnostic facts based on this audit alone. | Billing/diagnostic ownership, contracts, and external integrations were not identified. | `QuilvianBackend — Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs:14–49 (MstPaymentMethod), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` |

## As-Is Contract

### Current frontend-to-backend registration sequence

```text
frontend form
  -> POST /v1/health-services/registration-management/patient-encounters
  -> extract EncounterId from the response
  -> POST /v1/health-services/emergency-installation-management/emergency-visits
```

The URL constants and separate calls are in
`QuilvianFrontEnd — src/lib/services/health-services/registration-management/emergency-registration.service.js:22–40,299–332 (EMERGENCY_REGISTRATION_API_URLS and create functions), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.
The ordering and retry state are in
`QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:798–928 (handleSubmitRegistration), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`.

### Confirmed provider/consumer mismatches

| Area | Frontend consumer | Backend provider | Consequence |
|---|---|---|---|
| Encounter type | The encounter payload sends `ENCOUNTER_TYPE.Emergency = 2`: `QuilvianFrontEnd — src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js:970–1027 (buildEmergencyEncounterPayload), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:55–62 (ENCOUNTER_TYPE), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | Emergency-visit creation accepts only `EncounterType.Outpatient`: `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:97–101 (CreateAsync), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | A frontend-created emergency encounter cannot be attached to the emergency visit under the current provider rule. |
| Payment type | Constants define `COMPANY = 3`: `QuilvianFrontEnd — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:95–102 (PAYMENT_TYPE), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | The active encounter enum accepts Cash and Insurance only, with controller validation: `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs:5–12 (EncounterPaymentType), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`; `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:1190–1194 (payment validation), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | Corporate payload can be rejected. |
| Emergency registration status | Payload defaults `REGISTERED = 1`: `QuilvianFrontEnd — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:148–158 (EMERGENCY_VISIT_REGISTRATION_STATUS), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e`; `QuilvianFrontEnd — src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js:1047–1079 (buildEmergencyVisitPayload), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | Backend maps `Pending = 1`, `Provisional = 2`, `Registered = 3`, `Completed = 4`: `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Enums/EmergencyRegistrationStatus.cs:3–9 (EmergencyRegistrationStatus), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | The client labels a pending state as registered. |
| Unknown patient | The UI must select/create a patient before it proceeds: `QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:519–601 (handlePatientStepNext), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | The eventual encounter creation requires patient identity: `QuilvianBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:25–29 (TrxPatientEncounter), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` | The later unknown-patient fields do not create a working provisional path. |
| Atomicity/idempotency | Two client requests and a retained UI result are used for retry: `QuilvianFrontEnd — src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:798–928 (handleSubmitRegistration), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` | No IGD local command/idempotency/outbox mechanism was identified in source. | A first-call success plus second-call failure persists a partial state and has no server-guaranteed replay contract. |

## Conflict dan Unknown

### Confirmed conflicts

- The current emergency visit service requires an outpatient encounter, while the current UI sends
  an emergency encounter. See the first row in **As-Is Contract**.
- The new-patient UI requires administrative demographics before reaching the unknown-patient
  controls; this contradicts the provisional-care baseline. Evidence is in
  `QuilvianFrontEnd — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:394–415 (required fields), commit e77ebd8040fae0509b48d810f94fb0ab9b2bab1e` and
  `QuilvianBackend — docs/module-blueprints/igd/00-interview-decisions.md:958 (IGD-DEC-016), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- `EmergencyVisitController` sets `VisitCompletedAt` on `Disposed` or `Cancelled`, rather than
  establishing the decided clinical/billing/administrative closure distinctions:
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyVisitController.cs:338–376 (UpdateStatus), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Current generic update/delete endpoints for visit, triage, disposition, and transfer do not
  evidence the append-only/correction/reopen governance. Examples are
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTriageController.cs:253–383 (Update/Delete), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` and
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyTransferController.cs:276–350 (Update/Delete), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.
- Emergency controller dependency injection is a likely runtime blocker until the registration
  source is reconciled; the controller requests a service but the inspected registration block
  contains no corresponding emergency-service registration. See
  `QuilvianBackend — Areas/HealthServices/EmergencyInstallationManagement/Controller/EmergencyVisitController.cs:40–52 (constructor), commit fa772b71bab3b66811030477aaaaeec48aedcc4b` and
  `QuilvianBackend — Program.cs:259–281 (service registration block), commit fa772b71bab3b66811030477aaaaeec48aedcc4b`.

### Unknowns outside the source boundary

- Whether the emergency migration has been applied and whether an existing database contains
  data inconsistent with the desired lifecycle.
- The effective MMC role-to-capability assignment, clinical privileges, delegation/on-call,
  break-glass policy, and governance approvers.
- Final SOP values for temporary-identity evidence, high-impact correction, SLA policy,
  financial release exception, and disaster-mode activation.
- External laboratory/radiology, payer, payment, messaging, and runtime queue contracts.
- Whether a DI registration is supplied by an uninspected composition mechanism or environment;
  no such source registration was found in the audited application source.

## Closure Questions

Questions below are for `$grill-me`; this map does not answer them.

1. Which Registration/Master Patient owner approves the authoritative provisional-identity contract,
   including the representation used before a definitive `PatientId` exists?
2. Which exact MMC evidence determines the capability mapping and approval requirements for
   reconciliation, reverse merge, high-impact correction, financial exception, and disaster mode?
3. What is the canonical clinical context required by Clinical Management and Pharmacy when the
   emergency visit begins provisionally, and which owner may approve its use?
4. Is `EncounterType.Outpatient` the intentional canonical IGD encounter type, or is the current
   frontend/backend mismatch an unapproved contract divergence?
5. Which disposition, transfer, clinical-completion, administrative-release, and billing states
   already have canonical owners outside IGD, if any?
6. What system owns lab/radiology order/result, critical-result routing, and late-result follow-up;
   what is its supported integration/reliability contract?
7. Which Finance policy and authorization evidence can activate a self-pay `Outstanding + Released`
   exception, if activation is later approved?
8. Does a runtime composition root or deployment module register `Emergency*Service` classes, and
   which test/environment proves controller activation?
9. What disaster/incident domain, if any, exists outside these repositories and is it the intended
   owner of the IGD mass-casualty state?

## Impact Scan Trigger

This map becomes stale when either recorded SHA changes. Before it is used for implementation,
compare the new revision with the SHA above and rescan at least the affected capability rows.

- Changes under `Areas/HealthServices/EmergencyInstallationManagement`, Registration, Patient,
  Clinical, Pharmacy, Billing, or `Repositories/Configurations` require a journey and contract
  impact scan.
- Changes under `Program.cs`, security/permission, credentialing, workflow, logging, or migration
  files require a cross-cutting capability scan.
- Changes under frontend routes, registration services/hooks/constants, or IGD clinical UI require
  a provider/consumer and duplicate-submit scan.
- Changes to SOP/configuration, runtime DI, database deployment, external integration, or role
  assignments invalidate the relevant source-only `Unknown` statements and require evidence from
  the owning environment.
