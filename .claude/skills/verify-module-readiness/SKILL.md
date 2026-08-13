---
name: verify-module-readiness
description: Audit kesiapan modul Quilvian secara read-only terhadap requirement, blueprint, kontrak, source backend/frontend, konfigurasi runtime, dan bukti test; hasilkan progress, blocker, gap traceability, serta verdict. Gunakan saat menilai apakah modul benar-benar siap digunakan atau setelah satu delivery slice selesai; jangan gunakan untuk mengimplementasikan perbaikan.
---

# Verify Module Readiness

Nilai kesiapan end-to-end berdasarkan bukti. Pisahkan kemajuan scaffold dari kesiapan operasional agar angka progress tidak menyesatkan.

## Effort dan model minimum

| Field | Nilai |
| --- | --- |
| Minimum effort | `high` |
| Model Claude minimum | Claude Opus 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5, reasoning `high` |
| Alasan | Verdict readiness dipakai sebagai dasar keputusan pakai/tidak pakai; audit dangkal berbahaya karena terlihat meyakinkan |

Jika model minimum tidak tersedia, hentikan dan sampaikan ke pengguna. Lebih baik tidak ada
verdict daripada verdict yang salah.

## Aturan output dokumentasi

Readiness report wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger.

Blocker ditulis dengan kalimat yang dipahami pemilik proses bisnis, disertai contoh kejadian
nyata yang akan terjadi bila modul dipakai dalam kondisi sekarang.

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
NewQuilvianSystemBackend/docs/module-blueprints/<module>/testing/readiness-report.md
```

## Batas verdict

- `READY`: seluruh invariant kritis, kontrak, konfigurasi, dan acceptance scenario terbukti; tidak ada blocker.
- `READY_WITH_CONDITIONS`: fungsi utama terbukti, hanya risiko terbatas dengan owner dan mitigasi eksplisit.
- `NOT_READY`: ada blocker alur utama, security/privacy, integritas data, contract drift, runtime, atau bukti kritis belum tersedia.

Jika pengguna hanya meminta diagnosis, berhenti pada laporan.

## Tawarkan skill berikutnya

Setelah readiness report tuntas, **selalu** tawarkan langkah berikutnya secara eksplisit
berdasarkan jenis gap yang ditemukan, lengkap dengan alasan singkatnya:

| Temuan | Skill yang ditawarkan |
| --- | --- |
| Keputusan, owner, atau invariant belum jelas | `/grill-me` Amendment/Closure Pass |
| Capability atau commit SHA sudah stale | `/trace-existing-capabilities` impact scan |
| Arsitektur, ERD, atau kontrak salah/kurang | `/design-business-module` revisi baru |
| Ada requirement tanpa task atau dependency belum tercakup | `/plan-module-delivery` |
| Gap implementasi backend | `/build-module-backend` untuk satu task |
| Gap implementasi frontend | `/build-module-frontend` untuk satu task |
| Perbaikan sudah selesai | `/verify-module-readiness` ulang |
| Verdict `READY` | tidak ada langkah lanjutan; sampaikan syarat pemeliharaan bukti |

Urutkan tawaran berdasarkan blocker paling berdampak lebih dulu. Tawarkan, jangan jalankan
sendiri, dan jangan memperbaiki source dari skill ini agar audit tetap netral.
