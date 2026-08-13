---
name: design-business-module
description: Rancang blueprint modul bisnis Quilvian setelah keputusan dan capability audit cukup. Gunakan untuk menghasilkan arsitektur backend, arsitektur frontend, ERD per bounded context, API/integration contract, state-transition, validation, permission, audit, dan test strategy. Jangan gunakan untuk menulis kode aplikasi atau mengunci pilihan UI yang belum disetujui.
---

# Design Business Module

Bangun satu desain target lintas backend dan frontend tanpa menduplikasi ownership existing.

## Verifikasi gerbang input

1. Temukan blueprint canonical di
   `QuilvianBackend/docs/module-blueprints/<module>/`.
2. Baca `blueprint-manifest.md`, decision log, dan capability map.
3. Tolak desain final jika invariant kritis, ownership schema, atau source of truth masih
   belum diputuskan.
4. Bandingkan commit SHA manifest dengan kedua repository. Jika berubah, minta atau lakukan
   impact scan read-only pada area terdampak sebelum melanjutkan.
5. Baca [blueprint output contract](references/blueprint-output-contract.md).

## Pisahkan as-is dan to-be

- **As-is contract** berasal dari bukti controller/DTO/OpenAPI, persistence/runtime wiring,
  dan frontend consumer.
- **To-be contract** adalah target versioned yang disetujui owner.

Jangan mengubah keterbatasan existing menjadi requirement target tanpa keputusan manusia.
Jangan pula menganggap controller terbaru otomatis menggantikan to-be contract approved.

## Rancang backend

Tetapkan:

- bounded context, owner, aggregate root, invariant, transaction boundary, rollback;
- entity Existing/Extend/New/Adapter, PK/FK, optionality, index, unique constraint, delete
  behavior, audit, concurrency;
- API/application/domain/persistence responsibility;
- state transition, correction, cancellation, reopen, illegal transition;
- sync/async integration, idempotency, timeout, retry, dead-letter/reconciliation;
- authentication, permission, privacy, retention, logging, observability;
- migration, seed/default configuration, DI, test strategy, deployment/rollback.

## Rancang frontend

Tetapkan kebutuhan fungsional, action per role, data/status/error contract, validation,
permission, cache/invalidation, loading/empty/error/retry, stale data, duplicate submit,
privacy, accessibility, responsive behavior, dan test dependency.

Gunakan authority hierarchy:

```text
security/privacy/invariant
  -> approved product/UI brief
  -> project design system/convention
  -> DEV_DISCRETION
```

Jangan menentukan sidebar, urutan menu, route final, tab/modal/drawer, warna, layout, atau
component library tanpa requirement/brief yang sah. Sajikan opsi dan trade-off bila perlu.

## Buat ERD per bounded context

- Buat context ERD dan ERD per submodul agar terbaca.
- Tandai setiap entity `Existing`, `Extend`, `New`, atau `Adapter/View` beserta owner.
- Tampilkan PK, FK, cardinality, nullability, unique/index, delete behavior, audit, dan
  concurrency yang material.
- Referensikan entity lintas domain; jangan membuat `PatientIGD`, `DoctorIGD`, atau salinan
  master hanya agar modul mandiri.

Jangan membuat flowchart atau use-case diagram. Gunakan skenario, actor table,
state-transition matrix, dan acceptance criteria.

## Versioning dan output

Buat hanya artefak yang relevan di struktur canonical. Setiap contract menyimpan:

- `contract_version` dan status `draft/approved/superseded`;
- owner, `approved_by`, `approved_at`;
- `input_revision`, `input_hash`, compatibility impact;
- requirement/decision traceability.

Perubahan setelah approval membuat revision/version baru dan memicu impact scan kedua
repository. Jangan menandai desain `approved`; approval tetap tindakan manusia.
