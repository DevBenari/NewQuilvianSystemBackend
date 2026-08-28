# Rawat Jalan Billing — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RJ-BIL-BP-001` |
| `module_name` | Dokter / Rawat Jalan Billing |
| `module_slug` | `rawat-jalan` |
| `module_prefix` | `RJ-BIL` |
| `revision` | `18` |
| `status` | `PARTIAL` |
| `current_phase` | `RJ-BIL-PH-009` — Delivery Execution |
| `created_at` | `2026-08-20T15:06:30+07:00` |
| `updated_at` | `2026-08-28T09:22:16+07:00` |
| `last_verified_at` | `2026-08-28T09:22:16+07:00` |
| `backend_source_sha` | `6b25e6049e60e055593968abe463262b59842527` cabang `sukmagp`; working tree `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, `RJ-BIL-BE-007`, dan remediasi penamaan QBE belum di-commit |
| `frontend_source_sha` | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| `skill_suite_version` | `1.0.0-rc2` |
| `input_revision_hash` | `decisions:sha256:4447878A7EB6BAA7DAFCC7E93ECF7D1807FBD0B2058A9576AABCDD6B876AEEF2; capability:sha256:D1CB1D052474FA96F0BE801F7CEA277AEB0604A9969247B7323F82D23F5152B7` |
| `decision_revision` | `13` |
| `contract_versions` | `RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)` |
| `active_dependency_ids` | `RJ-BIL-DEP-001` s.d. `RJ-BIL-DEP-009` |
| `active_roadmap_revision` | `1` |
| `supersedes` | `null` |
| `domain_architecture_revision` | `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; core internal/manual siap independen |
| `owners` | Product/Domain: **Sukma Giri**. Juga Product/Domain Owner `LaboratoryManagement` sejak `RJ-BIL-DEC-007`. Billing/Revenue Cycle, API authority, Security/Privacy, Frontend authority: `OPEN` |
| `approved_by` | `User-provided approval authority` |
| `approved_at` | `2026-08-21` |

## Artifact hashes

Hash artefak target dihitung ulang pada `2026-08-26`. Semua artefak desain masih berstatus
`draft` dan belum menjadi izin implementasi.

**Catatan verifikasi `2026-08-26`.** Perhitungan sebelumnya dilakukan `2026-08-20`, sedangkan
ketiga berkas `roadmap/` baru dibuat commit `fe6d15c` pada `2026-08-21`. Akibatnya ketiga hash
roadmap tidak pernah cocok sejak awal — bukan drift desain, melainkan hash yang dicatat sebelum
berkasnya ada. Sembilan artefak desain dan kontrak diverifikasi ulang pada tanggal ini dan
**seluruhnya cocok**, sehingga tidak ada satu pun artefak desain yang berubah diam-diam.

| Artifact | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `524AF2A661A77092092A9A896571415DA0FA45576CDB5D01B6C624DCA3FFA22E` |
| `03-frontend-architecture.md` | `05304BDFC6323930E80516AC12A73A7CB34FD3A0378C0449EB560E58BADFA4E8` |
| `hospital-domain-architecture.md` | `E7B0B0F08DB9CFB2B7FEF6727F705FA09ED0720CC6FEE32F7EE0C1DD8EADF96E` |
| `contracts/api-contract.md` | `9508986836A537DD88A664F052003AE1A8509EE0555DD6DC2F3F83AB6B871FD6` |
| `contracts/state-transition-matrix.md` | `1688209BFEEAFA3F5A48A70376FCE4CA291239F189343FFF615D05C2C39B5807` |
| `contracts/validation-matrix.md` | `8C3FF0A302BAB10BC6DC904630F3FE0BD2DA675F1DE2491586F08CF596209E53` |
| `contracts/integration-contract.md` | `5743FC7B31A27500360FDE9E3EA61856D1CB4290539B07CDA8640F6F9E112DB6` |
| `contracts/permission-audit-matrix.md` | `1424ADE6BB5084C8A77105477C65B347959C6C17570A88E2E9663C38C23FF093` |
| `testing/acceptance-test-matrix.md` | `FEC2E3816EF086540FB85EAB9242A559906BF65154B1DC311068A5C999840EF6` |
| `owner-review-checklist.md` | `review artifact; hash dihitung setelah owner mengisi record approval` |
| `roadmap/backend-roadmap.md` | `B6CC1AC74D61A779FAA08CF587BF0BBFB8731079C2744F426F87BD948B208343` |
| `roadmap/frontend-roadmap.md` | `BB495410EC08884B5202D76CE3583839BD3A5301334F32F5AFBFA1D23946FC9B` |
| `roadmap/requirement-traceability.md` | `728E7DF2957E680341185E39EDAD14F5622E35D6DBE1ED9DA1FEFAEA6F3E1C5F` |

Catatan verifikasi `2026-08-28` (revision `18`) — **penyelarasan artefak governance**. Seluruh
tiga belas hash dihitung ulang. Sembilan artefak desain dan kontrak cocok tanpa perubahan; ketiga
artefak roadmap berubah dan hash-nya diperbarui di sini, begitu pula `input_revision_hash` untuk
decisions.

Pemicunya: setelah `RJ-BIL-BE-006` selesai `2026-08-27`, sebagian artefak dimutakhirkan dan
sebagian tertinggal, sehingga beberapa dokumen bertentangan satu sama lain. Yang diselaraskan:

| Artefak | Yang melenceng | Perbaikan |
|---|---|---|
| `00-interview-decisions.md` | Header masih `Revision 10` padahal `RJ-BIL-DEC-011` dan `RJ-BIL-DEC-012` sudah tercatat dan `approved` di dalamnya | Header disamakan dengan `decision_revision` yang sudah dicatat manifest, yaitu `13`. Isi keputusan **tidak disentuh** |
| `MODULE-STATUS.md` | `IMPLEMENTATION_AUTHORITY` dan `BUILDER_EXECUTION` belum memuat `RJ-BIL-BE-006`; `Evidence state` masih menyebut `22` test, `3` migration, dan `88` migration terdaftar; **`Next recommended task` masih memakai urutan dependency yang sudah dikoreksi `RJ-BIL-DEC-008`** | Ketiganya dimutakhirkan. Urutan lama diberi catatan koreksi eksplisit, dan langkah berikutnya disusun ulang menurut apa yang benar-benar menahan modul |
| `roadmap/requirement-traceability.md` | Masih menyatakan `IMPLEMENTATION_AUTHORITY NOT_GRANTED`, `BUILDER_EXECUTION NOT_AUTHORIZED`, dan *"tidak ada test project pada snapshot"* | Ditulis ulang mengikuti bentuk kedua roadmap; kolom **Keadaan** dan **Governance** dipisahkan supaya ✅ tidak terbaca sebagai izin production |
| `roadmap/backend-roadmap.md` | Ringkasan bagian 0 tertinggal — `4 dari 9`, `111` test, `RJ-BIL-BE-006` terblokir — bertentangan dengan isi dokumennya sendiri | Disamakan menjadi `5 dari 9` dan `157` test |
| `roadmap/frontend-roadmap.md` | `RJ-BIL-FE-004` masih tercatat terblokir padahal `RJ-BIL-BE-006` sudah selesai | Enam tempat disamakan |
| `testing/readiness-report.md` | Catatan penyeliaan masih menyebut `111` test dan `4` dari `9` task | Catatan dimutakhirkan. **Badan laporan sengaja tidak ditulis ulang** — ia potret audit `2026-08-24`, dan verdict `NOT_READY` tetap berlaku |

Kedua roadmap juga memakai bentuk penyajian baru sejak `2026-08-27`: dari satu tabel sebelas
kolom menjadi struktur bagian bernomor dengan tabel `Field / Isi` vertikal per task, mengikuti
bentuk `rawat-inap`. Cakupan, acceptance criteria, dependency, dan kontrak **tidak berubah**;
`active_roadmap_revision` karena itu tetap `1`.

`current_phase` dinaikkan `RJ-BIL-PH-008` → `RJ-BIL-PH-009` agar cocok dengan `MODULE-STATUS.md`,
yang sejak revision `18` sudah mencatat `PH-008` sebagai fase selesai. `backend_source_sha`
dilengkapi: working tree yang belum di-commit mencakup `RJ-BIL-BE-002`, `003`, `006`, `007`, dan
remediasi penamaan QBE — bukan hanya `RJ-BIL-BE-003`.

Baris `owner-review-checklist.md` sengaja tidak memuat hash; isinya catatan, dan tetap demikian
sampai owner mengisi record approval.

Arsitektur domain revision `1` sudah tersedia. Fase berikutnya adalah `design-business-module`
untuk slice core internal/manual yang siap. Aktivasi adapter eksternal tetap terblokir oleh
`RJ-BIL-DEP-009`.
