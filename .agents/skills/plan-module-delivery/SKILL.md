---
name: plan-module-delivery
description: Ubah blueprint bisnis Quilvian yang sudah disetujui menjadi roadmap delivery backend dan frontend berbasis vertical slice, task yang dapat diuji, dependency, serta traceability. Gunakan setelah keputusan, capability map, arsitektur, ERD, dan kontrak target tersedia; jangan gunakan untuk menggali kebutuhan, mendesain modul, atau langsung menulis source code.
---

# Plan Module Delivery

Susun satu rencana delivery terpadu tanpa menggabungkan tanggung jawab backend dan frontend menjadi task yang kabur.

## Input wajib

Temukan blueprint canonical di `QuilvianBackend/docs/module-blueprints/<module>/`, lalu baca:

1. manifest dan status approval;
2. decision log;
3. capability map beserta commit SHA kedua repository;
4. arsitektur backend dan frontend;
5. ERD per bounded context;
6. kontrak API/event target dan versinya.

Hentikan perencanaan yang bergantung pada keputusan atau kontrak berstatus `DRAFT`, `CONFLICT`, atau `OPEN`. Bila SHA sumber berubah sejak audit, lakukan impact scan dan catat hasilnya sebelum melanjutkan.

## Workflow

### 1. Bentuk vertical slice

Kelompokkan pekerjaan berdasarkan hasil bisnis yang bisa diverifikasi, bukan hanya berdasarkan layer teknis. Satu slice dapat memuat perubahan data, API, permission, UI, dan test yang diperlukan untuk satu hasil pengguna.

### 2. Pisahkan task backend dan frontend

Backend dan frontend boleh berjalan paralel hanya setelah kontrak target terkait berstatus `APPROVED` dan memiliki versi/hash yang dikunci. Jangan membuat frontend menebak payload yang belum disetujui.

### 3. Definisikan setiap task

Setiap task wajib memiliki:

- ID stabil (`BE-...` atau `FE-...`);
- outcome pengguna atau bisnis;
- requirement ID, decision ID, dan contract version yang dilayani;
- dependency dan urutan eksekusi;
- capability existing yang digunakan, diperluas, atau diperbaiki;
- lokasi perubahan yang diperkirakan;
- acceptance criteria yang dapat diuji;
- test dan bukti yang wajib dihasilkan;
- risiko, blocker, serta owner;
- Definition of Done.

Jangan mengunci bentuk menu, layout, atau gaya visual apabila tidak ada keputusan produk/UI yang disetujui. Tandai ruang tersebut sebagai `DEV_DISCRETION` dan tetap cantumkan invariant aksesibilitas, privasi, dan keamanan.

### 4. Susun dependency dan milestone

Urutkan fondasi kontrak, slice minimum yang dapat dipakai, integrasi lintas modul, hardening, lalu readiness. Hindari roadmap yang menyatakan semua model harus selesai sebelum semua API dan UI dimulai jika sebuah vertical slice dapat diuji lebih awal.

### 5. Tulis artefak canonical

Gunakan format di [roadmap-template.md](references/roadmap-template.md). Simpan satu kali di:

```text
QuilvianBackend/docs/module-blueprints/<module>/roadmap/
  backend-roadmap.md
  frontend-roadmap.md
  requirement-traceability.md
```

Frontend hanya mereferensikan module ID, revision, task ID, dan contract version tersebut; jangan menyalin roadmap sebagai sumber kebenaran kedua.

## Guardrail

- Jangan menulis source code aplikasi dari skill ini.
- Jangan menganggap scaffold sama dengan fitur end-to-end.
- Jangan menyembunyikan dependency eksternal sebagai task implementasi biasa.
- Jangan menandai task selesai tanpa acceptance evidence.
- Jika requirement tidak terhubung ke task atau test, keluarkan sebagai coverage gap.

## Handoff

Setelah approval roadmap:

- jalankan `build-module-backend` untuk satu task backend yang dipilih;
- jalankan `build-module-frontend` untuk satu task frontend yang dipilih;
- jalankan `verify-module-readiness` setelah slice memiliki bukti implementasi.
