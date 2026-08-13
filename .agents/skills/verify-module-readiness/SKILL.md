---
name: verify-module-readiness
description: Audit kesiapan modul Quilvian secara read-only terhadap requirement, blueprint, kontrak, source backend/frontend, konfigurasi runtime, dan bukti test; hasilkan progress, blocker, gap traceability, serta verdict. Gunakan saat menilai apakah modul benar-benar siap digunakan atau setelah satu delivery slice selesai; jangan gunakan untuk mengimplementasikan perbaikan.
---

# Verify Module Readiness

Nilai kesiapan end-to-end berdasarkan bukti. Pisahkan kemajuan scaffold dari kesiapan operasional agar angka progress tidak menyesatkan.

## Prinsip operasi

- Default read-only: audit dan laporkan; jangan memperbaiki source code.
- Klaim harus memiliki bukti berupa repository, path relatif, line/symbol, dan commit SHA.
- `Ready` tidak boleh diberikan bila input stale, approval hilang, atau ada dua artefak canonical yang bertentangan.
- Build yang sukses bukan bukti bahwa alur bisnis selesai.

## Workflow

### 1. Validasi baseline

Baca manifest blueprint canonical, decision log, capability map, arsitektur, ERD, kontrak, roadmap, dan traceability. Cocokkan `input_revisions`, `artifact_hashes`, `contract_versions`, serta commit SHA dengan keadaan yang diaudit.

Jika ada drift, tandai bagian yang terdampak sebagai `STALE` sampai impact scan atau approval baru tersedia.

### 2. Audit requirement ke bukti

Untuk setiap requirement, telusuri:

```text
Requirement -> Decision -> Design/ERD -> Contract -> Task -> Code/config -> Test/evidence
```

Klasifikasikan gap sebagai desain, implementasi backend, implementasi frontend, integrasi, data/configuration, security/privacy, test, atau operasional.

### 3. Audit backend

Periksa sesuai scope modul:

- entity/configuration/migration dan integritas relasi;
- DTO, validation, service, controller, DI, permission, audit/logging;
- seed/master configuration dan environment dependency;
- workflow state, transaction boundary, concurrency/idempotency;
- contract, unit, integration, dan failure-path tests;
- dependency lintas modul tanpa duplikasi sumber kebenaran.

Keberadaan file hanya dihitung sebagai fondasi jika belum terbukti dapat berjalan dalam alur bisnis.

### 4. Audit frontend

Periksa sesuai scope modul:

- route/menu hanya bila disetujui;
- consumer API sesuai contract version yang dikunci;
- permission, loading, empty, error, retry, validation, dan privacy state;
- konsistensi design system dan ruang `DEV_DISCRETION`;
- test komponen, integration, contract, dan E2E;
- tidak ada UUID/internal identifier yang bocor sebagai informasi pengguna.

### 5. Audit integrasi dan runtime

Periksa skenario utama, variasi, kegagalan, retry, dan penutupan workflow. Validasi DI, migration/seed, worker/event, konfigurasi, serta dependency eksternal tanpa menganggap keberadaan kode sama dengan deployment aktual.

### 6. Hitung dan simpulkan

Gunakan rubric di [readiness-rubric.md](references/readiness-rubric.md). Sebutkan denominator dan bobot eksplisit. Laporkan sedikitnya:

- foundation/scaffold progress;
- backend readiness;
- frontend readiness;
- integration/runtime readiness;
- verification coverage;
- blocker berurutan berdasarkan dampak;
- rekomendasi task berikutnya;
- verdict `READY`, `READY_WITH_CONDITIONS`, atau `NOT_READY`.

Simpan laporan canonical pada:

```text
QuilvianBackend/docs/module-blueprints/<module>/testing/readiness-report.md
```

## Batas verdict

- `READY`: seluruh invariant kritis, kontrak, konfigurasi, dan acceptance scenario terbukti; tidak ada blocker.
- `READY_WITH_CONDITIONS`: fungsi utama terbukti, hanya risiko terbatas dengan owner dan mitigasi eksplisit.
- `NOT_READY`: ada blocker alur utama, security/privacy, integritas data, contract drift, runtime, atau bukti kritis belum tersedia.

Jika pengguna hanya meminta diagnosis, berhenti pada laporan. Tawarkan skill build yang sesuai untuk perbaikan, tetapi jangan menjalankannya tanpa permintaan implementasi.
