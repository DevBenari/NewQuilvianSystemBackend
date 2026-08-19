# Quilvian Backend API Contract Rules

These rules preserve existing backend conventions. `AGENTS.md` remains authoritative; use the nearest mature implementation in the owning domain rather than introducing a parallel convention.

## Canonical QBE alignment

Read `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` before API work. Apply QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-LOG-001, QBE-DTO-001, QBE-VAL-001 and applicable QBE-CODE rules. Reference implementations illustrate existing behavior only; they do not override the canonical contract.

## Contract authority and scope

- Backend source is authoritative for API contracts and business/security behavior. Frontend code is a consumer reference, not an authority that silently redefines contracts.
- Before API work, inspect the actual controller route/action, HTTP verb, request and response DTOs, validation, authorization, status values, pagination/filter behavior, and workflow rules. Do not guess a contract from a frontend URL.
- Preserve backward compatibility where practical. Do not rename, remove, or break endpoints, fields, envelopes, enum/status values, or actions without explicit authorization and consumer assessment.
- Do not introduce a new response envelope, DTO architecture, validation architecture, error model, or repository abstraction unless explicitly authorized.

## Controller, route, and response conventions

- Follow the nearest controller's `[ApiController]`, `ControllerBase`, versioned `api/v1/...` route, Area/domain naming, Swagger metadata, HTTP verb, binding, and status-code conventions.
- Use the established `ApiResponse<T>` success/failure envelope and `PagedResult<T>` pagination shape where the existing endpoint family does so. Preserve filters, sorting, defaults, and response field names.
- Keep request and response DTOs in the owning domain's `DTOs/` folder when that pattern exists. Do not expose EF entities merely to avoid an established response DTO.
- Preserve nullable, identifier, date/time, default, and data-annotation behavior. Use the nearest DTO's validation attributes such as `[Required]`, `[MaxLength]`, and `[Range]` when applicable.
- Use async APIs and propagate `CancellationToken` according to the nearest controller/service pattern. Return existing error/status semantics; do not conceal failures with invented successful payloads.

## Authorization, ownership, and workflow authority

- Preserve `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, `AccessTypes`, role/permission checks, and current-user/claim resolution as implemented by the owning domain.
- For self-service endpoints, derive ownership from the authenticated current user using the existing context/service pattern. Do not accept arbitrary actor, workforce, or user identifiers to bypass ownership.
- Backend remains authoritative for workflow transitions, actor/delegated-actor authorization, `AvailableActions`, approval/rejection, status transitions, and idempotency. Frontend visibility is not authorization.

## Representative evidence

- Master-data controller, route, DTO, and model: `Areas/Administrator/MasterData/Controllers/BankController.cs`; `Areas/Administrator/MasterData/DTOs/BankDtos.cs`; `Areas/Administrator/MasterData/Models/MstBank.cs`
- Shared response contracts: `Responses/ApiResponse.cs`; `Responses/PagedResult.cs`
- Authorization metadata: `Attributes/AccessControllerAttribute.cs`; `Attributes/AccessActionAttribute.cs`; `Attributes/AccessPermissionAttribute.cs`
- Self-service/current-user pattern: `Areas/SelfServices/HumanResource/Controllers/OvertimeSelfServiceController.cs`; `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceContextService.cs`
- Workflow authority: `Areas/Corporate/HumanResource/WorkflowManagement/Controllers/WorkflowActionV2Controller.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.ActionsV2.cs`

## Multi-developer consistency

New modules follow the nearest mature implementation in their domain. A new module does not justify a new route grammar, response shape, DTO layout, validation style, persistence architecture, or error model.
