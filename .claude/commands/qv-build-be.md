---
description: Implementasikan tepat satu task backend approved dari roadmap canonical
argument-hint: <BE-TASK-ID> <module> [contract version]
---

Gunakan skill `build-module-backend` untuk: **$ARGUMENTS**

Aturan pemanggilan:

- Implementasikan **hanya satu task backend** sesuai task ID di atas. Jangan mengerjakan task lain dalam pemanggilan yang sama.
- Lewati gate lebih dulu: blueprint canonical ada, task berstatus approved/ready, punya requirement/decision ID, contract version, dependency, acceptance criteria, dan test plan; manifest revision/hash serta source commit tidak stale. Jika gate gagal, berhenti dengan blocker spesifik.
- Gunakan contract version dan acceptance criteria pada roadmap. Jangan mengubah contract approved secara sepihak.
- Reuse pola modul terdekat; jangan menduplikasi entitas/master bersama.
- Pertahankan perubahan pengguna yang tidak terkait di worktree.
- Jalankan verifikasi yang diizinkan: test paling sempit dahulu, lalu build/test project bila perlu. Verifikasi success path, validation, unauthorized/forbidden, conflict/concurrency, failure rollback, dan contract shape.
- Jangan menerapkan migration ke database non-lokal, jangan reset database, jangan mengubah credential.
- Jangan melakukan git add/commit/push kecuali saya memintanya eksplisit.

Task belum selesai hanya karena build sukses. Perbarui bukti pada task/traceability dan buat laporan perubahan backend sebelum menandai selesai.
