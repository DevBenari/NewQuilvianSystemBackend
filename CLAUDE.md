# CLAUDE.md — Panduan Agent Backend

`AGENTS.md` tetap menjadi acuan governance repository ini (konvensi kode, kontrak backend,
`docs/engineering/`, aturan lintas repository). Dokumen ini hanya mengatur pemakaian Skill
Quilvian.

## Skill Quilvian bersifat opsional

Skill Quilvian (`quilvian-engineering-skills:*`) **tidak wajib**. Prompter yang menentukan:

- Bila prompter meminta skill tertentu (misalnya `grill-me` atau `build-module-backend`),
  jalankan skill itu.
- Bila prompter tidak menyebut skill, kerjakan permintaannya langsung: baca kode yang relevan,
  ikuti pola controller/service/DTO yang sudah ada, lalu ubah source seperlunya.
- Agent boleh **menyarankan** skill bila jelas membantu, misalnya `grill-me` untuk modul baru
  yang requirement-nya belum jelas, tetapi tidak memaksakannya dan tidak menahan pekerjaan yang
  permintaannya sudah jelas.

Tidak ada chain wajib (`grill-me` → … → `plan-module-delivery`) sebelum implementasi, dan tidak
ada status `BLOCKED` hanya karena sebuah skill tidak dipanggil atau tidak tersedia.

## Target tulis

Permintaan prompter yang jelas meminta perubahan pada repository ini sudah dihitung sebagai
target tulis eksplisit. Prompter tidak perlu menulis `TASK MODE: BACKEND` secara formal.
Perubahan ke repository frontend tetap hanya dilakukan bila prompter memintanya.

## Larangan otomatisasi

Agent dilarang menjalankan secara otomatis, tanpa instruksi eksplisit dan konfirmasi terpisah
dari user untuk masing-masing aksi:

- `git commit`, `git push`, `git pull`, `git merge`, `git rebase`
- deploy
- migration (pembuatan maupun eksekusi)
- eksekusi database langsung
