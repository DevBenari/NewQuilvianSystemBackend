---
description: Wawancara kritis modul Quilvian (Scope/Closure/Amendment Pass) dan tulis decision log
argument-hint: <module> [scope|closure|amendment] [tujuan bisnis singkat]
---

Gunakan skill `grill-me` untuk modul: **$ARGUMENTS**

Aturan pemanggilan:

- Pass default adalah Scope Pass bila tidak disebutkan. Jika argumen memuat `closure`, jalankan Closure Pass; jika memuat `amendment`, jalankan Amendment Pass.
- Wawancarai saya secara kritis, satu pertanyaan per giliran. Jangan menumpuk banyak pertanyaan sekaligus.
- Jangan menulis source code pada tahap ini.
- Pertanyaan yang bisa dijawab dari source jangan ditanyakan ke saya; kumpulkan sebagai bahan audit untuk `/trace-existing-capabilities`.
- Gali scope, aktor, ownership, invariant, state, exception, permission, privacy, integration, failure behavior, acceptance criteria, dan kewenangan UI.
- Pada Closure Pass: baca capability map lebih dulu, lalu tanyakan hanya conflict, unknown, ownership, dan keputusan yang memblokir desain.

Simpan keputusan pada blueprint canonical `docs/module-blueprints/<module>/00-interview-decisions.md` tanpa menghapus histori. Jangan menandai keputusan sebagai approved tanpa approval nyata dari owner.

