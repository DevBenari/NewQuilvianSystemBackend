# IGD — Permission and Audit Matrix

| Field | Value |
|---|---|
| `contract_version` | `0.1.0-draft` |
| Rule | `Allowed = capability ∧ context ∧ state ∧ credential/assignment ∧ SOD` |
| Owner | Security/Privacy with bounded-context governance owners |

| Action family | Capability examples | Context / SOD | Required audit |
|---|---|---|---|
| Intake and administration | `Encounter.Create`, `Encounter.CreateProvisional`, `Encounter.CompleteRegistration` | IGD unit; clinician emergency exception allowed; no clinical authority implied | Actor, encounter, identity mode, reason, server time, correlation |
| Identity | `PatientIdentity.Reconcile`, `Merge`, `ApproveAmbiguousMerge`, `ReverseMerge` | Registration/Patient scope; ambiguous maker ≠ checker; reverse high-impact | Candidate/evidence refs, maker/checker/capability, prior/result status; no PHI payload |
| Nursing care | `Encounter.StartService`, `Triage.Create`, `Triage.Retriage`, observation/resuscitation actions | Unit, credential, encounter and protocol/order context | Clinical event/ref, actor/effective capability/time, amendment reason |
| Doctor clinical decision | disposition, order, clinical amendment, late-result review | Clinical privilege, assigned/on-call/unit scope | Decision/reason, state/version, ordering/review ownership |
| Transfer | request, accept, reject, depart, arrive | Receiver must match destination; sender cannot mark arrival | Source/destination, actor/unit, state/timestamp/reason |
| Financial action | clearance, exception request/approval, release | Finance scope; requester ≠ approver; feature gate | Amount/reference only in protected finance audit; generic audit has safe reference |
| Governance/high impact | correction/cancel/reopen approve | Domain approval and all impacted domains; assignment current | Request, maker/checker, policy/evidence, prior/result state, effective dates |
| Disaster | activate/confirm/reject/deactivate | Legal actor and operation scope | Source/reason/evidence ref, prior/result operation state, actor/time |
| Break-glass | `EmergencyAccess.BreakGlass` | Time/target/reason bounded; never routine, finance, or merge bypass | Actor, target, scope/window/reason, review disposition |

No technical administrator or `SuperAdmin` bypass grants clinical, identity, financial, or governance
approval by itself. Print/export/disclosure are separately capability-gated and privacy-audited.
Generic logs store identifiers/reference IDs and authorization outcome only; protected audit storage
uses the owner’s data classification and retention rules.
