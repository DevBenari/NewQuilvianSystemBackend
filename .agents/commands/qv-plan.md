---
description: Buat roadmap delivery backend/frontend dengan vertical slice dan traceability
argument-hint: <module> [revision]
---

Gunakan skill `plan-module-delivery` untuk blueprint: **$ARGUMENTS**

Aturan pemanggilan:

- Blueprint dan kontrak harus sudah approved. Jika belum, hentikan dan sebutkan item yang menunggu approval.
- Pisahkan roadmap backend dan frontend, gunakan vertical slice.
- Hubungkan setiap task ke requirement ID, decision ID, contract version, acceptance criteria, test, dependency, risiko, owner, dan Definition of Done.
- Task harus kecil dan dapat diuji. Jangan membuat task raksasa seperti "buat seluruh backend modul".
- Backend dan frontend boleh paralel hanya bila contract version terkait sudah approved dan hash-nya terkunci.

Output:

```text
docs/module-blueprints/<module>/roadmap/backend-roadmap.md
docs/module-blueprints/<module>/roadmap/frontend-roadmap.md
docs/module-blueprints/<module>/roadmap/requirement-traceability.md
```

