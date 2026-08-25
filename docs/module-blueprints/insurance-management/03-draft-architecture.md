# Insurance Management — Evidence-Constrained Draft Architecture

Status: `DRAFT`; no approval evidence. This describes safe ownership boundaries only, not a decision to implement new entities/endpoints/UI.

| Context | Ownership | Current status | Intended relationship |
| --- | --- | --- | --- |
| Provider configuration | Existing `MstInsuranceProvider` / Administrator Master Data | Existing | Insurance Management reads/reuses provider configuration; it does not duplicate it. |
| Patient policy | Existing `MstPatientInsurance` / Patient Management | Existing | Supplies patient-policy eligibility context subject to lifecycle decision. |
| Encounter payment source | Existing `TrxPatientEncounterGuarantor` / Registration Management | Existing | Supplies the selected insurer/company guarantor at encounter. |
| Tariff and coverage | Existing `MstInsuranceTariff`, `MstInsuranceCoverageRule`, runtime services | Existing / Adapter | Supplies evaluation inputs; an approved future financial lifecycle must define snapshots/versioning. |
| Clinical and pharmacy | Existing Clinical/Pharmacy services | Adapter | May request coverage/approval context; no cross-domain workflow is approved. |
| Insurance operations | New bounded context, conditional | Blocked | Only after `INS-DEC-001`–`INS-DEC-004`: eligibility/GL/pre-auth/claim/correction/reconciliation records and transitions may be designed. |
| External provider | Adapter, conditional | Blocked | Only after provider-specific contract, idempotency, retry, privacy, and reconciliation rules are approved. |

## Required invariants to decide, not assume

1. Which record is immutable after encounter/billing and which values must be snapshotted.
2. Whether one encounter may have more than one insurer/guarantor and their priority/excess allocation.
3. Who can create, submit, amend, approve, reject, cancel, or reconcile each operation.
4. What clinical/financial event makes a claim eligible, and what occurs after rejection/partial approval.
5. What patient data and documents may leave the system, with audit/retention requirements.
