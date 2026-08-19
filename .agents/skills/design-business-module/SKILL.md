---
name: design-business-module
description: Rancang blueprint modul bisnis Quilvian setelah keputusan dan capability audit cukup. Gunakan untuk menghasilkan arsitektur backend, arsitektur frontend, ERD per bounded context, API/integration contract, state-transition, validation, permission, audit, dan test strategy. Jangan gunakan untuk menulis kode aplikasi atau mengunci pilihan UI yang belum disetujui.
---

# Design Business Module

Bangun satu desain target lintas backend dan frontend tanpa menduplikasi ownership existing.

## Effort dan model minimum

| Field | Nilai |
| --- | --- |
| Minimum effort | `high` |
| Model Claude minimum | Claude Opus 5 |
| Model Claude disarankan | Claude Opus 5 |
| Model GPT setara | GPT-5, reasoning `high` |
| Alasan | Kesalahan desain ERD, ownership, dan kontrak baru terasa jauh di belakang dan mahal diperbaiki |

Ini satu-satunya skill yang tidak menyediakan turunan model yang lebih kecil. Jika model
minimum tidak tersedia, hentikan dan sampaikan ke pengguna; jangan melanjutkan desain.

## Aturan output dokumentasi

Seluruh artefak desain wajib mengikuti
[aturan output dokumentasi](../../rules/rule-output/aturan-output-dokumentasi.md): Bahasa Indonesia,
bahasa yang mudah dipahami orang umum, penjelasan detail beserta contoh, bisnis proses yang
jelas, dan endpoint bergaya Swagger.

Khusus dokumen kontrak, setiap grup endpoint ditulis memakai judul grup `[Tags(...)]` diikuti
tabel API. Untuk endpoint yang belum ada di kode, beri label `Rencana (belum tersedia)` agar
pembaca tidak menyangka sudah bisa dipakai.

## Verifikasi gerbang input

1. Temukan blueprint canonical di
   `NewQuilvianSystemBackend/docs/module-blueprints/<module>/`.
2. Baca `blueprint-manifest.md`, decision log, dan capability map.
3. Tolak desain final jika invariant kritis, ownership schema, atau source of truth masih
   belum diputuskan.
4. Bandingkan commit SHA manifest dengan kedua repository. Jika berubah, minta atau lakukan
   impact scan read-only pada area terdampak sebelum melanjutkan.
5. Baca [blueprint output contract](references/blueprint-output-contract.md).

## Pisahkan as-is dan to-be

- **As-is contract** berasal dari bukti controller/DTO/OpenAPI, persistence/runtime wiring,
  dan frontend consumer.
- **To-be contract** adalah target versioned yang disetujui owner.

Jangan mengubah keterbatasan existing menjadi requirement target tanpa keputusan manusia.
Jangan pula menganggap controller terbaru otomatis menggantikan to-be contract approved.

## Rancang backend

Tetapkan:

- bounded context, owner, aggregate root, invariant, transaction boundary, rollback;
- entity Existing/Extend/New/Adapter, PK/FK, optionality, index, unique constraint, delete
  behavior, audit, concurrency;
- API/application/domain/persistence responsibility;
- state transition, correction, cancellation, reopen, illegal transition;
- sync/async integration, idempotency, timeout, retry, dead-letter/reconciliation;
- authentication, permission, privacy, retention, logging, observability;
- migration, seed/default configuration, DI, test strategy, deployment/rollback.

### Artefak backend yang wajib ada

Baca [backend-structure-rules.md](references/backend-structure-rules.md) sebelum menulis
lokasi file apa pun. Jangan menebak folder.

`02-backend-architecture.md` wajib memuat:

1. **Tabel kepemilikan data** — kelompok data, modul pemilik, dipakai modul ini atau tidak,
   dibuat ulang atau tidak. Ini pertahanan langsung terhadap duplikasi entity.
2. **Class diagram** Mermaid, dipecah per bounded context agar satu diagram muat dibaca dalam
   satu layar. Format di [class-and-erd-template.md](references/class-and-erd-template.md).
3. **Penjelasan setiap class** dalam bentuk tabel, wajib memuat baris **Status**
   (`Baru`/`Diperbarui`/`Sudah ada`) dan **Lokasi file**. Mencakup model, service, dan
   controller, bukan model saja.
4. **Arsitektur folder** berupa pohon folder beserta status setiap file. Penyimpangan dari pola
   standar ditandai sebagai utang teknis; jangan ditiru dan jangan dirapikan diam-diam.
5. **Status model** ringkas beserta dampak migration. Untuk status `Diperbarui`, kolom yang
   berubah wajib disebutkan satu per satu — menulis "diperbarui" tanpa merinci kolom membuat
   migration tidak dapat direncanakan.
6. **Rencana migration** — urutan, dapat dijalankan tanpa mematikan layanan atau tidak,
   pengisian data lama, dan langkah mundur bila gagal.
7. **Rencana data master awal** — isi minimum setiap tabel master. Modul dengan master kosong
   tidak dapat dipakai sama sekali.
8. **Yang sengaja tidak dibuat** — class yang dipertimbangkan lalu ditolak beserta alasannya,
   agar tidak diusulkan ulang di kemudian hari.

### Aturan informasi perubahan

Setiap kali desain menambah atau mengubah sesuatu, seluruh baris yang relevan wajib terisi.
Perubahan tanpa informasi ini dianggap belum selesai dirancang.

| Yang berubah | Informasi wajib |
| --- | --- |
| Tabel | Nama, schema, status, kolom yang berubah, index, unique constraint, perilaku hapus |
| Kolom atau parameter | Nama, tipe, wajib atau tidak, nilai bawaan, batas panjang, validasi, penanda sensitif |
| Endpoint | Grup `[Tags(...)]`, base URL, method, path, kegunaan, hak akses, request, response, kode status |
| Controller | Nama file, lokasi folder, status, service yang dipakai, atribut akses |
| Service | Nama, fungsi utama, dipanggil siapa, apakah membuka transaksi database |
| DTO | Nama class, jenis (Create/Update/Status/Response/PagedQuery/Option), field |
| Enum | Nama, daftar nilai, nilai bawaan |
| Configuration | Nama file, lokasi, relasi yang diatur, index, `DeleteBehavior` |
| Migration | Nama, urutan, dapat dijalankan tanpa downtime atau tidak, cara mundur |
| Permission | String `[AccessPermission("Resource", "Action")]` yang persis |

## Rancang frontend

Tetapkan kebutuhan fungsional, action per role, data/status/error contract, validation,
permission, cache/invalidation, loading/empty/error/retry, stale data, duplicate submit,
privacy, accessibility, responsive behavior, dan test dependency.

Gunakan authority hierarchy:

```text
security/privacy/invariant
  -> approved product/UI brief
  -> project design system/convention
  -> DEV_DISCRETION
```

Jangan menentukan sidebar, urutan menu, route final, tab/modal/drawer, warna, layout, atau
component library tanpa requirement/brief yang sah. Sajikan opsi dan trade-off bila perlu.

## Buat ERD dan kamus data per bounded context

- Buat context ERD dan ERD per submodul agar terbaca. Gunakan Mermaid `erDiagram`; satu diagram
  harus muat dibaca dalam satu layar.
- Tandai setiap entity `Existing`, `Extend`, `New`, atau `Adapter/View` beserta owner.
- Tampilkan PK, FK, cardinality, nullability, unique/index, delete behavior, audit, dan
  concurrency yang material.
- Referensikan entity lintas domain; jangan membuat `PatientIGD`, `DoctorIGD`, atau salinan
  master hanya agar modul mandiri.

Tulis `erd/data-dictionary.md` dengan kedalaman bertingkat mengikuti status tabel:

| Status tabel | Yang didokumentasikan |
| --- | --- |
| `Baru` dan `Diperbarui` | Seluruh kolom |
| `Sudah ada` | Kolom kunci saja — PK, FK, kolom status, dan kolom yang dipakai aturan bisnis modul ini — ditambah rujukan ke file model |

Sepuluh kolom warisan `IdentityModel` jangan diulang per tabel; nyatakan sekali di kepala
dokumen. Setiap kolom diberi penanda **Sensitif**; kolom bertanda sensitif tidak boleh masuk
custom logger dan tidak boleh dipakai sebagai contoh berisi data asli.

Format lengkap ada di [class-and-erd-template.md](references/class-and-erd-template.md).

Jangan membuat flowchart atau use-case diagram. Gunakan skenario, actor table,
state-transition matrix, dan acceptance criteria.

## Versioning dan output

Buat seluruh file pada struktur canonical di
[blueprint-output-contract.md](references/blueprint-output-contract.md); daftarnya pasti,
tidak bebas. File yang tidak relevan bagi modul tertentu tetap dibuat dengan satu baris alasan
yang menyebut sebabnya, bukan dihapus dan bukan pula dibiarkan kosong. Setiap contract
menyimpan:

- `contract_version` dan status `draft/approved/superseded`;
- owner, `approved_by`, `approved_at`;
- `input_revision`, `input_hash`, compatibility impact;
- requirement/decision traceability.

Perubahan setelah approval membuat revision/version baru dan memicu impact scan kedua
repository. Jangan menandai desain `approved`; approval tetap tindakan manusia.

## Tawarkan skill berikutnya

Setelah desain tuntas sebagai `draft`, **selalu** tawarkan langkah berikutnya secara
eksplisit, lengkap dengan alasan singkatnya:

| Kondisi setelah desain | Skill yang ditawarkan |
| --- | --- |
| Desain lengkap dan owner sudah menyetujui blueprint serta kontrak | `/plan-module-delivery` |
| Masih ada keputusan bisnis yang menggantung | `/grill-me` Amendment/Closure Pass |
| Capability atau commit SHA ternyata sudah stale | `/trace-existing-capabilities` impact scan |

Sebutkan lebih dulu bahwa hasil skill ini masih `draft` dan approval manusia belum tergantikan.
Tawarkan, jangan jalankan sendiri.

