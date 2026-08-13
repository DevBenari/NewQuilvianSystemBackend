---
name: build-module-backend
description: Implementasikan tepat satu task backend Quilvian yang telah disetujui dari roadmap canonical menggunakan pola ASP.NET Core dan EF Core project, termasuk verifikasi serta laporan perubahan. Gunakan hanya ketika task ID, requirement, desain, kontrak target, dependency, dan acceptance criteria sudah terkunci; jangan gunakan untuk discovery, audit read-only, desain, atau task frontend.
---

# Build Module Backend

Kerjakan satu task backend sampai memiliki bukti acceptance tanpa memperluas scope diam-diam.

## Effort dan model minimum

| Field | Nilai |
| --- | --- |
| Minimum effort | `high` |
| Model Claude minimum | Claude Sonnet 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5 atau GPT-5-Codex, reasoning `high` |
| Alasan | Skill ini mengubah source, migration, dan permission; kesalahan berdampak langsung ke data produksi |

Jika sesi berjalan di bawah batas ini, beritahu pengguna sebelum menyentuh source dan minta
konfirmasi. Effort rendah cenderung melewatkan failure path dan concurrency.

## Aturan output dokumentasi

Laporan perubahan dan pembaruan traceability wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger.

Bila task ini menambah atau mengubah endpoint, dokumentasikan memakai judul grup `[Tags(...)]`
milik controller terkait diikuti tabel API, sehingga pembaca dapat mencocokkannya dengan
halaman Swagger.

## Gate sebelum mengubah source

1. Baca instruksi repository yang aktif (`AGENTS.md`, `CLAUDE.md`, atau aturan setara jika tersedia).
2. Temukan blueprint canonical di `docs/module-blueprints/<module>/`.
3. Pastikan task backend memiliki ID, status approved/ready, owner, requirement dan decision ID, contract version, dependency, acceptance criteria, dan test plan.
4. Cocokkan manifest revision/hash serta source commit. Jika stale, lakukan impact scan dan minta approval baru bila kontrak atau rule berubah.
5. Pastikan tidak ada entitas/master bersama yang akan diduplikasi.

Jika gate tidak lolos, berhenti dengan blocker spesifik. Jangan menciptakan keputusan bisnis untuk mengisi kekosongan.

## Workflow implementasi

### 1. Inspeksi pola existing

Cari modul terdekat dan identifikasi model, EF configuration, DTO, service, controller, DI, permission, logging, test, migration, dan seed yang relevan. Reuse pola yang masih sesuai aturan aktif; jangan menyalin bug atau konvensi usang.

### 2. Implementasikan scope task

Ubah hanya lapisan yang diperlukan acceptance criteria. Bergantung task, ini dapat mencakup:

- entity/value object dan EF configuration;
- migration atau seed yang aman;
- request/response DTO dan validation;
- application/domain service dan transaction boundary;
- controller/API/event contract;
- DI, permission, audit/logging, concurrency, dan idempotency;
- unit, integration, contract, dan failure-path test.

Pastikan rule penting berada di boundary yang tidak dapat dilewati oleh controller, worker, atau integration handler lain.

### 3. Verifikasi

Jalankan test paling sempit yang relevan dahulu, lalu build/test project bila diizinkan dan diperlukan. Verifikasi success path, validation, unauthorized/forbidden, conflict/concurrency, failure rollback, serta contract shape sesuai task.

Jangan menerapkan migration ke database non-lokal, mengubah credential, atau menulis konfigurasi rahasia tanpa otorisasi eksplisit.

### 4. Perbarui bukti

Setelah acceptance terpenuhi:

- catat file/symbol dan hasil test pada task/traceability;
- perbarui manifest/hash bila artefak canonical memang berubah dan melalui approval yang diwajibkan;
- buat laporan perubahan backend sesuai [backend-project-profile.md](references/backend-project-profile.md).

Jangan menandai task selesai hanya karena source berhasil dikompilasi.

## Batas kewenangan

- Jangan mengerjakan task frontend dari skill ini.
- Jangan mengubah contract approved secara sepihak; buat proposal delta dan impact scan dua repo.
- Jangan menggunakan operasi Git write pada backend bila kebijakan repository menetapkannya read-only untuk agent.
- Jangan menjalankan destructive migration atau reset database.
- Pertahankan perubahan pengguna yang tidak terkait.

## Handoff

Laporkan outcome, file utama, verifikasi yang dijalankan/tidak dijalankan, migration/config impact, risiko tersisa, dan task berikutnya.

## Tawarkan skill berikutnya

Setelah task ini tuntas dan acceptance criteria punya bukti, **selalu** tawarkan langkah
berikutnya secara eksplisit, lengkap dengan alasan singkatnya:

| Kondisi setelah task | Skill yang ditawarkan |
| --- | --- |
| Masih ada task backend berikutnya pada slice yang sama | `/build-module-backend` untuk satu task ID berikutnya |
| Kontrak yang dipakai frontend sudah tersedia dan approved | `/build-module-frontend` untuk task frontend pasangannya, dijalankan dari sesi repo frontend |
| Satu vertical slice backend-frontend sudah lengkap | `/verify-module-readiness` untuk audit netral |
| Muncul contract delta atau keputusan bisnis baru | `/design-business-module` atau `/grill-me` sesuai jenis gap |

Tawarkan, jangan jalankan sendiri. Jika acceptance criteria belum semuanya terbukti, sebutkan
sisa pekerjaannya lebih dulu dan jangan menawarkan langkah maju.
