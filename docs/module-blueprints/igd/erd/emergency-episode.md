# IGD — Emergency Episode ERD

| Field | Value |
|---|---|
| Contract version/status | `0.1.0-draft` |
| Owner | Registration Management and Emergency Installation |
| Traceability | `IGD-DEC-001`, `004`, `005`, `016`–`021`, `039`–`042` |

| Entity | Status / owner | PK and references | Material constraints |
|---|---|---|---|
| `TrxPatientEncounter` | Extend / Registration | `EncounterId` PK; `PatientId?`; `ProvisionalIdentityId?`; `ServiceUnitId` FK | `EncounterType=Outpatient` for IGD; exactly one active identity reference; arrival/server audit; administrative state/version. Index `(ServiceUnitId, ArrivalAt)`. Restrict delete. |
| `TrxEmergencyVisit` | Existing+Extend / Emergency | `EmergencyVisitId` PK; `EncounterId` FK unique active | One active visit per encounter; episode/clinical state and concurrency token; no hard delete. |
| `TrxEmergencyTriage` | Existing+Extend / Emergency | `EmergencyTriageId` PK; `EmergencyVisitId` FK; `PreviousTriageId?` self-FK | Unique `(EmergencyVisitId, SequenceNo)`; append-only effective assessment; prior link is restrict delete. |
| `TrxEmergencyObservation` | Existing+Extend / Emergency | PK; `EmergencyVisitId` FK | Start <= end; shared clinical fact IDs are optional references, not copied values. |
| `TrxEmergencyResuscitation` | Existing+Extend / Emergency | PK; `EmergencyVisitId` FK | Start <= end; clinical actor/capability and amendment provenance required. |
| `TrxEmergencyDisposition` | Existing+Extend / Emergency | PK; `EmergencyVisitId` FK | Versioned Draft/Confirmed/Executed history; execution does not set completion by itself. |
| `TrxEmergencyTransfer` | Existing+Extend / Emergency | PK; `EmergencyVisitId` FK; source/destination unit IDs | State/version/history. Receiver action validates destination context; timestamp sequence requested ≤ accepted ≤ departed ≤ arrived. |
| `IgdEpisodeStateLedger` | New / Emergency | PK; `EncounterId`, `EmergencyVisitId?`, command/correlation IDs | Immutable append-only state/evidence reference, actor/capability, expected/result version; no PHI payload. Unique command/idempotency scope. |

The current generic update/delete controllers are not the target mutation model. A correction creates
an amendment linked to the original record; cancellation is a semantic terminal state only when
there is no material clinical activity; every manual reopen is scoped and maker-checker governed.
