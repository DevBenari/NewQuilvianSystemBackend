---
description: Audit readiness end-to-end blueprint secara read-only dan beri verdict
argument-hint: <module> [slice/scope tertentu]
---

Gunakan skill `verify-module-readiness` untuk blueprint: **$ARGUMENTS**

Aturan pemanggilan:

- Audit bersifat read-only dan netral. Jangan memperbaiki source dari pemanggilan ini; temuan perbaikan dikembalikan ke skill yang sesuai.
- Telusuri rantai requirement -> decision -> design/ERD -> contract -> task -> code/config -> test.
- Pisahkan scaffold progress dari end-to-end readiness. Jangan menganggap build sukses sebagai bukti readiness.
- Sertakan blocker spesifik beserta bukti dan owner-nya.

Output: `docs/module-blueprints/<module>/testing/readiness-report.md`

Verdict harus salah satu dari `READY`, `READY_WITH_CONDITIONS` (dengan risiko, owner, dan mitigasi), atau `NOT_READY`.

