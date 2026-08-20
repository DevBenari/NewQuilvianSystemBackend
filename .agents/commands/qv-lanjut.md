---
description: Lanjutkan pekerjaan blueprint pada sesi baru tanpa mengulang dari nol
argument-hint: <module> <revision> <scope/task-id>
---

Lanjutkan pekerjaan blueprint: **$ARGUMENTS**

Langkah:

1. Baca `docs/module-blueprints/<module>/blueprint-manifest.md`, artefak upstream, roadmap, dan readiness report terakhir.
2. Validasi revision, hash artefak, serta source SHA backend dan frontend.
3. Tentukan skill yang tepat untuk melanjutkan, lalu panggil skill tersebut: `grill-me`, `trace-existing-capabilities`, `design-business-module`, `plan-module-delivery`, `build-module-backend`, atau `verify-module-readiness`.
4. Kerjakan **hanya** scope/task ID yang saya sebutkan.

Jika revision/hash/SHA berbeda dari yang tercatat, hentikan area terdampak dan jalankan impact scan lebih dulu.

Artefak canonical adalah sumber kebenaran handoff. Jangan mengandalkan ringkasan chat lama.

