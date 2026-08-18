---
description: Susun blueprint target modul - arsitektur, ERD, ownership, dan kontrak
argument-hint: <module> [fokus/bounded context tertentu]
---

Gunakan skill `design-business-module` untuk: **$ARGUMENTS**

Aturan pemanggilan:

- Bekerja hanya di atas decision log yang approved dan capability map terbaru. Jika keduanya belum ada atau stale, hentikan dan sebutkan blocker-nya.
- Hasilkan arsitektur backend, arsitektur frontend, ERD per bounded context, API/integration contract, state transition, validation, permission/audit, dan acceptance-test strategy.
- Jangan menulis kode, flowchart, atau use-case diagram.
- Pisahkan as-is contract dari to-be contract.
- Tandai entity sebagai `Existing`, `Extend`, `New`, atau `Adapter/View` beserta owner-nya. Jangan membuat salinan patient/doctor atau master lintas domain.

Output relevan: `blueprint-manifest.md`, `02-backend-architecture.md`, `03-frontend-architecture.md`, `erd/*.md`, `contracts/*.md`, `testing/acceptance-test-matrix.md`.

Hasil tahap ini berstatus `draft`. Approval tetap keputusan manusia.

