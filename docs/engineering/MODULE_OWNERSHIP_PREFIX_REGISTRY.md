# Module Ownership & Prefix Registry

This is the approved authority for operational entity ownership/naming. Lifecycle values are registry metadata: PLANNED, ACTIVE, LEGACY, DEPRECATED. Empty Git-folder absence is not planning authority.

Registry approval grants naming and ownership authority only. It does **not** authorize implementation, migration, database work, deployment, or activation of a PLANNED module. For example, `InsuranceManagement` / `Ins` / PLANNED defines its future naming owner if separately authorized; it does not authorize Insurance production work.

| Area | Module/owner | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE / LEGACY |
| Finance | Finance | BUSINESS DOMAIN | Fin | ACTIVE |
| Administrator / HealthServices | Master / Reference | MASTER / REFERENCE | Mst | ACTIVE |
| HealthServices | ClinicalManagement / Clinical | BUSINESS DOMAIN / MODULE | Cli | ACTIVE / LEGACY |
| HealthServices | RegistrationManagement / Registration | BUSINESS DOMAIN / MODULE | Reg | ACTIVE / LEGACY |
| HealthServices | PatientManagement operational | BUSINESS DOMAIN / MODULE | Pat | ACTIVE |
| HealthServices | PharmacyManagement / Pharmacy | BUSINESS DOMAIN / MODULE | Phm | ACTIVE / LEGACY |
| HealthServices | EmergencyInstallationManagement / Emergency | BUSINESS DOMAIN / MODULE | Emg | ACTIVE / LEGACY |
| HealthServices | BillingManagement / Billing | BUSINESS DOMAIN / MODULE | Bil | ACTIVE |
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | PLANNED |
| HealthServices | RadiologyManagement / Radiology | BUSINESS DOMAIN / MODULE | Rad | PLANNED |
| HealthServices | InPatientManagement / Inpatient | BUSINESS DOMAIN / MODULE | Inp | PLANNED |
| HealthServices | OutPatientManagement / Outpatient | BUSINESS DOMAIN / MODULE | Out | PLANNED |
| HealthServices | InsuranceManagement / Insurance | BUSINESS DOMAIN / MODULE | Ins | PLANNED |
| Corporate/HumanResource | WorkflowManagement / Workflow | SHARED PLATFORM CAPABILITY | Wfl | ACTIVE / LEGACY |
| HealthServices | OperatingRoomManagement / Operating Room | BUSINESS DOMAIN / MODULE | Opr | PLANNED |
| HealthServices | MedicalRecordManagement / Medical Record | BUSINESS DOMAIN / MODULE | Mrc | PLANNED |

DoctorAndScheduleManagement is MASTER / REFERENCE by current evidence and has no independent operational prefix. For a new operational entity use `<ApprovedOwnerPrefix><BusinessConcept>` without redundant owner text, e.g. `RegPatientEncounter`, `EmgVisit`, `WflInstance`, `LabOrder`.

## QBE-MOD-002

A module that owns persisted operational entities MUST have an APPROVED registry entry before the first entity is created. Developer/Codex must resolve Area, Module, owner, category, prefix and table behavior. If absent, operational entity creation is BLOCKED; prefix must not be invented from folder name.

An authorized task MAY create or plan an unregistered module folder, but its first persisted operational entity remains BLOCKED until entry approval. Thus `HealthServices/RehabilitationManagement` cannot create `Reh*`, `Rhb*`, or `Trx*` operational entities without a registry decision.
