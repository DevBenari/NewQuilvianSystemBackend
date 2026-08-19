---
name: trace-existing-capabilities
description: Audit kemampuan existing Quilvian lintas backend dan frontend sebelum membuat modul atau tabel baru. Gunakan untuk menelusuri entity, relasi, migration, API, DI, permission, route, UI consumer, test, dan alur upstream/downstream; klasifikasikan reuse, adapter, extend, repair, missing, conflict, atau unknown dengan bukti. Skill ini read-only terhadap source aplikasi.
---

# Trace Existing Capabilities

Petakan kemampuan nyata, bukan sekadar nama file. Jangan memperbaiki kode dalam sesi audit.

## Effort dan model minimum

| Field | Nilai |
| --- | --- |
| Minimum effort | `high` |
| Model Claude minimum | Claude Sonnet 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5, reasoning `high` |
| Alasan | Audit menyisir dua repository sekaligus dan harus membedakan kemampuan nyata dari nama file yang mirip |

Jika sesi berjalan di bawah batas ini, beritahu pengguna sebelum mulai dan minta konfirmasi.
Effort rendah cenderung menghasilkan capability map yang tampak lengkap padahal dangkal.

## Aturan output dokumentasi

Capability map dan seluruh laporan skill ini wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger. Untuk API as-is yang ditemukan, tuliskan judul grup
persis nilai `[Tags(...)]` pada controller lalu sajikan tabel endpoint.

## Siapkan audit

1. Temukan Git root backend dan frontend.
2. Baca decision log modul dan identifikasi kebutuhan yang perlu dibuktikan.
3. Catat commit SHA kedua repository sebelum menyisir.
4. Gunakan output canonical:
   `NewQuilvianSystemBackend/docs/module-blueprints/<module>/01-existing-capability-map.md`.
5. Baca [capability map template](references/capability-map-template.md).

Jika database, service eksternal, atau runtime environment tidak boleh diperiksa, nyatakan
batas tersebut. Jangan menyimpulkan migration sudah diterapkan hanya karena file tersedia.

## Telusuri backend

Periksa sesuai kebutuhan:

- entity/model, enum, `DbSet`, EF configuration, PK/FK, cardinality, nullability, index,
  unique constraint, delete behavior, audit, dan concurrency;
- migration, seeder/default data, serta ownership domain;
- DTO, mapper, controller, route, HTTP verb, validation, service, dan business rule;
- dependency injection, configuration, authentication, permission, logging, privacy;
- unit, integration, contract, dan E2E test;
- pemakai entity/API oleh modul lain.

Gunakan `rg` terlebih dahulu. Baca implementasi yang relevan; jangan menilai hanya dari
hasil nama file.

## Telusuri frontend

Periksa:

- route dan menu yang benar-benar dapat dicapai;
- service/API client, Redux/query state, hook, cache, form, validation, enum mapping;
- permission guard, loading, empty, error, retry, stale state, duplicate submit;
- reusable component dan data source untuk select/master;
- test serta status aktif, dormant, mock, dummy, atau dead code.

Backend controller/DTO adalah bukti as-is provider contract; frontend consumer adalah bukti
bagaimana contract dipakai. Catat mismatch dua arah.

## Telusuri alur lintas modul

Ikuti alur end-to-end, misalnya:

```text
kiosk/registrasi -> patient -> encounter/admission -> clinical -> billing/discharge
```

Untuk setiap kebutuhan, tentukan owner data dan exact source reference. Cari kandidat reuse
sebelum mengusulkan entity baru. Secara khusus cegah duplikasi patient, doctor/employee,
encounter/admission, insurance, procedure, prescription, room/bed, dan shared master.

## Klasifikasikan

Gunakan tepat satu status utama per capability:

- `Ready to reuse`: tersedia, wired, cukup aman, dan terbukti dapat dipakai.
- `Reuse with adapter`: owner benar; perlu mapping/facade/integration layer.
- `Extend`: fondasi dan owner benar; contract/field/rule masih kurang.
- `Repair`: ada, tetapi defect atau runtime blocker harus ditutup.
- `Missing`: belum ada dan memang menjadi kebutuhan baru.
- `Conflict`: implementasi bertentangan dengan rule/owner/source lain.
- `Unknown`: bukti tidak cukup atau perlu environment/keputusan manusia.

Jangan memakai `Ready to reuse` bila DI, migration/seed, permission, consumer, atau test
yang relevan belum dapat dibuktikan.

## Tulis bukti dan hasil

- Format bukti: `repository + relative path + line/symbol + commit SHA`.
- Pisahkan fakta, inference, dan rekomendasi.
- Catat as-is API contract dan perbedaannya dari target yang diminta.
- Catat risiko duplikasi, breaking change, data migration, dan integration dependency.
- Buat closure questions untuk `/grill-me`; jangan menjawabnya sendiri.
- Jangan menghasilkan arsitektur target atau roadmap final.

Jika SHA berubah sebelum tahap berikutnya, tandai map stale dan lakukan impact scan pada
area berubah sebelum map dipakai kembali.

## Tawarkan skill berikutnya

Setelah capability map tuntas, **selalu** tawarkan langkah berikutnya secara eksplisit,
lengkap dengan alasan singkatnya:

| Kondisi setelah audit | Skill yang ditawarkan |
| --- | --- |
| Masih ada `Conflict`, `Unknown`, atau ownership yang belum jelas | `/grill-me` Closure Pass |
| Capability sudah berstatus dan berbukti, keputusan sudah cukup | `/design-business-module` |
| Audit dijalankan sebagai impact scan dan desain sudah ada | `/design-business-module` revisi, atau `/plan-module-delivery` bila kontrak tidak berubah |

Tawarkan, jangan jalankan sendiri. Tunggu persetujuan pengguna sebelum berpindah skill.
