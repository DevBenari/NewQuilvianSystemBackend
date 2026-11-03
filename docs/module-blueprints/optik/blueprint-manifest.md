# Optik — Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `OPT-BP-001` |
| `revision` | `1` |
| `status` | `approved` — disetujui pemilik Optik; persetujuan modul mitra (`OPT-APR-001`..`012`) masih menunggu |
| Modul | `optik` — Optik milik rumah sakit (pemeriksaan refraksi + penjualan kacamata/lensa kontak) |
| Tanggal | 2026-11-03 |

## Owners

| Wewenang | Pemilik |
| --- | --- |
| Product/domain | Yoga Aji Pratama (`OPT-DEC-002`) |
| API | Yoga Aji Pratama untuk endpoint Optik; perubahan internal Registrasi, Billing, Farmasi, Rekam Medis, Medical Fee, dan Master Data oleh pemilik modul masing-masing (`OPT-APR-001`..`012`) |
| Security & privasi | Yoga Aji Pratama; pemilik arsitektur backend untuk pola izin |
| Frontend authority | Yoga Aji Pratama (`OPT-FE-001`..`005`; `001`/`002` menunggu screen brief) |
| Registry prefix | Muhammad Hamzah, pemilik registry — baris `Opt` / `ACTIVE` disetujui 2026-11-03 (`OPT-EVD-001`) |

## Approval

| Field | Nilai |
| --- | --- |
| `approved_by` | Yoga Aji Pratama |
| `approved_at` | 2026-11-03 |
| Catatan | Seluruh keputusan bisnis Optik `approved` (`OPT-DEC-001`..`049`, kecuali `014` superseded). Blueprint dan kontrak `v1` disetujui Yoga Aji Pratama 2026-11-03 untuk sisi Optik. Bagian yang menyentuh modul mitra baru sah setelah `OPT-APR-001`..`012` disetujui pemiliknya; task terkait ditandai tertahan |

## Snapshot source

| Field | Nilai |
| --- | --- |
| `backend_commit_sha` | `2a5094ca41817a7f8422576d055ab18dd186d3c4` (branch `yoga`) |
| `frontend_commit_sha` | `b6dd6c304043fb904c664d7751b1ce9d9c532715` (branch `YogaV2`) |
| Frontend v1 (acuan) | `7210f5f4db3a0c1f51446249e5c35a57eedb081c` |

## Contract versions

| Kontrak | Versi | Status |
| --- | --- | --- |
| API | `OPT-API-v1` | `approved` (sisi Optik) |
| State | `OPT-STATE-v1` | `approved` (sisi Optik) |
| Validasi | `OPT-VAL-v1` | `approved` (sisi Optik) |
| Integrasi | `OPT-INT-v1` | `approved` (sisi Optik) |
| Permission/audit | `OPT-PERM-v1` | `approved` (sisi Optik) |
| PRD → MVP | `OPT-PRD-v1` | `approved` (sisi Optik) |

## Readiness hulu

| Tahap | Artefak | Hasil |
| --- | --- | --- |
| Wawancara | `00-interview-decisions.md` r4 | Keputusan lengkap; `OPT-APR-001`..`012` menunggu mitra |
| Capability map | `01-existing-capability-map.md` r1 | SHA sama dengan snapshot |
| Requirement gate | `02-requirement-completeness-assessment.md` r2 (`OPT-RCG-001`) | `READY_FOR_DOMAIN_DESIGN` S1–S10 |
| Arsitektur domain | `03-domain-architecture.md` r1 (`OPT-HDA-001`) | `DOMAIN_ARCHITECTURE_READY` S1–S10 |

## Artifact hashes (SHA-256)

Hash dihitung ulang sesudah pencatatan approval 2026-11-03. Perubahan isi artefak sesudah approval wajib menaikkan
revisi dan memicu impact scan dua repository.

| Artefak | SHA-256 |
| --- | --- |
| `00-interview-decisions.md` (r4) | `da9405c40dc44b161b9e43bc1e290e5144c9cac43f91aef54109a53de37e5eeb` |
| `01-existing-capability-map.md` (r1) | `52345393b62d3e3b3d9eaab7c4c2e2550aa6ba5d4ceeea86df9fbdc9324d9cac` |
| `02-requirement-completeness-assessment.md` (r2) | `0b12e8abe3fff68403b4addc515a393e38deb07607f3bb3f115e1685053f4350` |
| `03-domain-architecture.md` (r1) | `24255b773a416fcce5d5565f2cabea4f48e7060345c30282677a929432715567` |
| `02-backend-architecture.md` | `b4c8805e738bf2933a5abe11f6d68e016843e0a121bbfaa3d679e4b37d33a4fe` |
| `03-frontend-architecture.md` | `8b7e7a3eeec3ba54e876da7ff7831015be7f80f9b920ebb19250ca19446643ac` |
| `04-prd-to-mvp.md` | `2c4385bd1b3888095ac4da0713c1e801d3f0921279a81ac6f05df438445cf1b3` |
| `erd/00-context-erd.md` | `aa2df594942d73765ca3b6baf3cfc87a49c2b7ded7f0d64a9bdf910e0fb8544c` |
| `erd/optical-clinical.md` | `a8cb57f7451cae685691829482828aff17fc7364bbe54dac23c54da8429fc2cb` |
| `erd/optical-order.md` | `50cd9069cb36ad2fb055b37cc86ce17c39e5fb587c4dd87e80dddd304f5a0f19` |
| `erd/optical-reference.md` | `b31960c20cc513a8db83b0ee47da0a87402824ee84450146f1e04d527249e181` |
| `erd/data-dictionary.md` | `6c60fd7de77aa66e6d86216b2d407f3f5bb32276375e8c44b938d8cfdbb9234e` |
| `contracts/api-contract.md` | `c10c75b32618ef64ebd19f8d311d537fa248be763125f12f84adde313336adcf` |
| `contracts/state-transition-matrix.md` | `a344ddefa96c6fdfeca1c7a6ebfe1beb26d3d79b99a850ce614dc6bfc5137690` |
| `contracts/validation-matrix.md` | `d5de44a8839aeb7cbc359db7ee841e110273e365a8a23ac23a5519283c06d4d8` |
| `contracts/integration-contract.md` | `a2b9f2e4ac23d30218227ae6ad79c0ebd0124d388310db51c729310320e5dc53` |
| `contracts/permission-audit-matrix.md` | `7e2a968cd79758fc739c862609d3844e1929f9a2aa729996ef47ee644a1f64cc` |
| `testing/acceptance-test-matrix.md` | `ff798ba9d768f6fd9ed0a3fc8c6444a020621c742400c9375e13b02555f95523` |

## Roadmap

| Roadmap | Revision | Status |
| --- | --- | --- |
| [`roadmap/backend-roadmap.md`](roadmap/backend-roadmap.md) (`OPT-RM-BE-001`) | `1` | `approved` (2026-11-03) |
| [`roadmap/frontend-roadmap.md`](roadmap/frontend-roadmap.md) (`OPT-RM-FE-001`) | `1` | `approved` (2026-11-03) |
| [`roadmap/requirement-traceability.md`](roadmap/requirement-traceability.md) | `1` | `approved` (2026-11-03) |

## Prasyarat sebelum `/plan-module-delivery` (riwayat)

1. Pemilik menyetujui blueprint dan kontrak `v1` (status menjadi `approved`).
2. Pertanyaan memblokir di `04-prd-to-mvp.md` bagian 20 terjawab, terutama persetujuan Billing, Registrasi, Master Data/Farmasi, dan Rekam Medis.
3. Baris registry `Opt` diajukan (task pertama roadmap).
