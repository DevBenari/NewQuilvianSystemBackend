---
description: Audit read-only capability existing backend+frontend menjadi capability map
argument-hint: <module> [impact-scan]
---

Gunakan skill `trace-existing-capabilities` untuk blueprint: **$ARGUMENTS**

Aturan pemanggilan:

- Audit bersifat read-only. Jangan build, jangan memperbaiki kode, jangan mengubah source.
- Telusuri tabel/entity, relasi, migration, API, DI, permission, route, UI consumer, integrasi upstream/downstream, dan test pada backend maupun frontend.
- Cari reuse lebih dulu sebelum mengusulkan tabel baru, terutama untuk patient, doctor, employee, encounter/admission, insurance, procedure, prescription, room/bed, dan master bersama.
- Bila argumen memuat `impact-scan`, batasi pada capability dan kontrak as-is yang terdampak perubahan SHA saja.

Klasifikasikan setiap capability sebagai `Ready to reuse`, `Reuse with adapter`, `Extend`, `Repair`, `Missing`, `Conflict`, atau `Unknown`, dengan bukti berformat `repository + relative path + line/symbol + commit SHA`.

Output: `docs/module-blueprints/<module>/01-existing-capability-map.md`. Buat closure question untuk `/grill-me`; jangan menjawabnya sendiri.

