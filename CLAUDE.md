# CLAUDE.md — Backend Skill Routing Enforcement

Dokumen ini adalah lapisan enforcement runtime untuk agent Claude Code di repository ini.
`AGENTS.md` tetap **otoritatif** untuk seluruh governance level-repository (task mode,
wewenang tulis, branch, klasifikasi task, kontrak backend, aturan lintas repository).
Dokumen ini hanya mengatur bagaimana permintaan dirutekan ke registered runtime Skill Quilvian
dan tidak menggantikan atau menduplikasi isi `AGENTS.md`.

## Wajib memakai runtime Skill

Setiap permintaan yang membuat, mengubah, memperbaiki, atau refactor source backend **WAJIB**
dijalankan lewat registered runtime Skill:

    quilvian-engineering-skills:build-module-backend

Dilarang mengimplementasikan backend secara langsung (menulis/mengubah kode backend tanpa
memanggil Skill di atas terlebih dahulu), termasuk saat agent merasa yakin dengan solusinya.

Jika Skill tidak tersedia di runtime (invoke gagal dengan "Unknown skill" atau error registry
setara), agent **WAJIB berhenti** dan melaporkan persis:

    BLOCKED — Quilvian runtime skill unavailable

Dilarang melakukan fallback dengan membaca file `SKILL.md` secara manual sebagai pengganti
pemanggilan runtime Skill.

## Requirement/desain yang belum terkunci

Bila requirement, kontrak, atau arsitektur untuk permintaan ini belum disetujui/terkunci,
rutekan lebih dulu lewat chain berikut sebelum implementasi apa pun dimulai:

1. `quilvian-engineering-skills:grill-me`
2. `quilvian-engineering-skills:trace-existing-capabilities`
3. `quilvian-engineering-skills:requirement-completeness-gate`
4. `quilvian-engineering-skills:hospital-domain-architect` — bila relevan (melintasi bounded
   context, master data bersama, dampak billing, atau keselamatan klinis)
5. `quilvian-engineering-skills:design-business-module`
6. `quilvian-engineering-skills:plan-module-delivery`

Implementasi backend baru baru dijalankan lewat `quilvian-engineering-skills:build-module-backend`
setelah task disetujui dari roadmap hasil chain di atas.

## Larangan otomatisasi

Agent dilarang menjalankan secara otomatis, tanpa instruksi eksplisit dan konfirmasi terpisah
dari user untuk masing-masing aksi:

- `git commit`, `git push`, `git pull`, `git merge`, `git rebase`
- deploy
- migration (pembuatan maupun eksekusi)
- eksekusi database langsung

## Presedensi

Urutan berlaku: `AGENTS.md` (governance level-repository) → chain requirement/desain di atas
(bila belum terkunci) → `quilvian-engineering-skills:build-module-backend` (implementasi).
Dokumen ini adalah lapisan routing tambahan di atas governance yang sudah ada, bukan pengganti.
