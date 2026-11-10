# Optik — Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `OPT-BP-001` |
| `revision` | `5` |
| `status` | `approved` — r3, r4, dan r5 disetujui pemilik Optik 2026-11-06. Persetujuan mitra MVP `OPT-APR-001`, `002`, `005`..`012` tercatat (`OPT-EVD-002`); `OPT-APR-013`, `014`, `015` tercatat (`OPT-EVD-003`); `OPT-APR-003`, `004` berstatus `POST-MVP` |
| Modul | `optik` — Optik milik rumah sakit (pemeriksaan refraksi + penjualan kacamata/lensa kontak) |
| Tanggal | r1 2026-11-03; r2, r3, r4, dan r5 2026-11-06 |

## Owners

| Wewenang | Pemilik |
| --- | --- |
| Product/domain | Yoga Aji Pratama (`OPT-DEC-002`) |
| API | Yoga Aji Pratama untuk endpoint Optik; perubahan internal Registrasi, Billing, Farmasi, Rekam Medis, Medical Fee, dan Master Data oleh pemilik modul masing-masing (`OPT-APR-001`..`015`) |
| Security & privasi | Yoga Aji Pratama; pemilik arsitektur backend untuk pola izin |
| Frontend authority | Yoga Aji Pratama (`OPT-FE-001`..`005`; `001`/`002` `approved` lewat `05-screen-brief.md`) |
| Registry prefix | Muhammad Hamzah, pemilik registry — baris `Opt` / `ACTIVE` disetujui 2026-11-03 (`OPT-EVD-001`) |

## Approval

| Field | Nilai |
| --- | --- |
| `approved_by` | Yoga Aji Pratama |
| `approved_at` | r2, r3, r4, dan r5: 2026-11-06. r1: 2026-11-03 |
| Catatan | Seluruh keputusan bisnis Optik `approved` (`OPT-DEC-001`..`049`, kecuali `014` superseded). Blueprint dan kontrak `v1` disetujui Yoga Aji Pratama 2026-11-03 untuk sisi Optik. Bagian yang menyentuh modul mitra baru sah setelah `OPT-APR-001`..`012` disetujui pemiliknya; task terkait ditandai tertahan. **r2 (2026-11-06):** Yoga Aji Pratama menyetujui amendment `OPT-DEC-077`..`080`, kontrak di bawah, dan brief r3 untuk sisi Optik. Bagian Registrasi baru sah sesudah `OPT-APR-013`, dan bagian Billing sesudah `OPT-APR-014`. **r3 (2026-11-06):** Yoga Aji Pratama menjawab "lanjutkan sesuai saran kamu" atas pertanyaan persetujuan r3, untuk `OPT-DEC-081`..`083` dan kontrak di bawah. Adjustment tarif terlambat baru sah untuk rilis sesudah `OPT-APR-015`. **2026-11-06:** Yoga Aji Pratama menyampaikan persetujuan Muhammad Hamzah dan Yasmina (`OPT-APR-013`, `014`, `015`, `OPT-OQ-020`, `OPT-EVD-003`, `OPT-DEC-084`); bukti tertulis belum dilampirkan. **r4 (2026-11-06):** Yoga Aji Pratama menjawab "lanjutkan sesuai saran kamu" atas usulan izin lintas modul (`OPT-DEC-085`, `OPT-PERM-v1.3`) dan langkah penyegaran bagian 14. **r5 (2026-11-06):** Yoga Aji Pratama menjawab "cukup saya" atas `OPT-OQ-024` (`OPT-DEC-086`): keenam izin baca itu cukup disetujui Yoga Aji Pratama dan diberikan admin akses, dan pemilik modul diberi tahu |

## Snapshot source

| Field | Nilai |
| --- | --- |
| `backend_commit_sha` | `2a5094ca41817a7f8422576d055ab18dd186d3c4` (branch `yoga`) |
| `frontend_commit_sha` | `b6dd6c304043fb904c664d7751b1ce9d9c532715` (branch `YogaV2`) |
| `backend_commit_sha` r2 | `06e40fb702b85295bafc930b706b64db2d14aaa7` (branch `yoga`) |
| `frontend_commit_sha` r2 | `811f22159ca74e117f4754553086fa58f130b209` (branch `YogaV2`) |
| Snapshot r3 | SHA sama dengan r2, ditambah `BE-OPT-060` dan `FE-OPT-040` di working tree, belum di-commit |
| Snapshot r4 | SHA sama, ditambah `BE-OPT-060`..`062` dan `FE-OPT-040`..`042` di working tree, belum di-commit. Amendment r4 tidak mengubah source aplikasi. Uji backend Optik tidak ter-track Git |
| Snapshot r5 | Sama dengan r4, ditambah uji `BE-OPT-063`. Tanpa perubahan source aplikasi |
| Frontend v1 (acuan) | `7210f5f4db3a0c1f51446249e5c35a57eedb081c` |

## Contract versions

| Kontrak | Versi | Status |
| --- | --- | --- |
| API | `OPT-API-v1.4` | `approved` (sisi Optik), 2026-11-06; menggantikan `v1.3` |
| State | `OPT-STATE-v1.1` | `approved` (sisi Optik), 2026-11-06 |
| Validasi | `OPT-VAL-v1.3` | `approved` (sisi Optik), 2026-11-06 |
| Integrasi | `OPT-INT-v1.3` | `approved` (sisi Optik), 2026-11-06 |
| Permission/audit | `OPT-PERM-v1.4` | `approved` (sisi Optik), 2026-11-06 |
| PRD → MVP | `OPT-PRD-v1.4` | `approved` (sisi Optik), 2026-11-06 |

## Readiness hulu

| Tahap | Artefak | Hasil |
| --- | --- | --- |
| Wawancara | `00-interview-decisions.md` r13 | Keputusan `OPT-DEC-001`..`086`. Penyegaran bagian 11, 12 diterapkan pada r2, bagian 13 pada r3, dan bagian 14 pada r4, kecuali baris yang disebut di *Sisa penyegaran*. Persetujuan mitra MVP lengkap; `OPT-OQ-023` terbuka tanpa memblokir |
| Capability map | `01-existing-capability-map.md` r1 | SHA snapshot r1; lihat *Sisa penyegaran* |
| Requirement gate | `02-requirement-completeness-assessment.md` r2 (`OPT-RCG-001`) | `READY_FOR_DOMAIN_DESIGN` S1–S10 |
| Arsitektur domain | `03-domain-architecture.md` r1 (`OPT-HDA-001`) | `DOMAIN_ARCHITECTURE_READY` S1–S10 |

## Artifact hashes (SHA-256)

Hash dihitung ulang sesudah pencatatan approval r5, 2026-11-06. Perubahan isi artefak sesudah approval wajib menaikkan
revisi dan memicu impact scan dua repository.

| Artefak | SHA-256 |
| --- | --- |
| `00-interview-decisions.md (r13)` | `59e72e35aa461102b210086c448c589a24c0be463499db5395d5fad6a091a719` |
| `01-existing-capability-map.md (r1)` | `52345393b62d3e3b3d9eaab7c4c2e2550aa6ba5d4ceeea86df9fbdc9324d9cac` |
| `02-requirement-completeness-assessment.md (r2)` | `0b12e8abe3fff68403b4addc515a393e38deb07607f3bb3f115e1685053f4350` |
| `03-domain-architecture.md (r1)` | `24255b773a416fcce5d5565f2cabea4f48e7060345c30282677a929432715567` |
| `02-backend-architecture.md (r4)` | `6133359b906a432c589fedf12de626e35dc64871fd5a7083eb8d1ca13735603e` |
| `03-frontend-architecture.md (r2)` | `3191daae1471ee4139d416d409dfac4ed9481d32cac6de1740b6546af9629dcc` |
| `04-prd-to-mvp.md (r5)` | `cc699f21a4ac8f48f1fdf6ac8004ac49bdfc23c0507c7c6299db06b3edc7cd8e` |
| `05-screen-brief.md (r5)` | `a3eec884455cb85699ae7dbfaea3fd416f66d9cfdf87ce2297a468208232f94e` |
| `erd/00-context-erd.md (r1)` | `aa2df594942d73765ca3b6baf3cfc87a49c2b7ded7f0d64a9bdf910e0fb8544c` |
| `erd/optical-clinical.md (r3)` | `0579e1176f124ddd4a1ec5b3a8ed1dcbe110e1cf3dfe449a00ba0cd0d028a036` |
| `erd/optical-order.md (r3)` | `c2c4e1a18bb82e14cc106a83a42f446c56c9d5acf53c729d7eff1ed119ef5b35` |
| `erd/optical-reference.md (r1)` | `b31960c20cc513a8db83b0ee47da0a87402824ee84450146f1e04d527249e181` |
| `erd/data-dictionary.md (r3)` | `560a7251255cd2b484659e586f6e3f528e98cd6096946efc64fb275f39d9a6a1` |
| `contracts/api-contract.md (v1.4)` | `94468c2c259139c694560838fe071244cc2b48683f9485bf74cd88b440fec609` |
| `contracts/state-transition-matrix.md (v1.1)` | `9d72d866df082fd158d51e5350559a9a444f09412da385153a801a63bf181b84` |
| `contracts/validation-matrix.md (v1.3)` | `20187d2cd10c4e98f045c15e7a6eb740fe516f1359ad4daddf1160e5c326fe23` |
| `contracts/integration-contract.md (v1.3)` | `5b46b337a0a118cbefd4b83454bb70bcc03bd6180971f6e09b39d37af2afbb06` |
| `contracts/permission-audit-matrix.md (v1.4)` | `c0e6c3fa7d3b483fc6c27cbadfd26c2e8e1c8074e0e64cb3330b5e9343b71742` |
| `testing/acceptance-test-matrix.md (r5)` | `259b48232a68f14622acf800ebbfe193c37ba0fbb72997178ff7317662617b40` |

## Roadmap

| Roadmap | Revision | Status |
| --- | --- | --- |
| [`roadmap/backend-roadmap.md`](roadmap/backend-roadmap.md) (`OPT-RM-BE-001`) | `6` | r1..r3 `approved`; r4..r6 `DRAFT` (`BE-OPT-060`..`063` ✅ selesai 2026-11-06) |
| [`roadmap/frontend-roadmap.md`](roadmap/frontend-roadmap.md) (`OPT-RM-FE-001`) | `3` | r1, r2 `approved`; r3 `DRAFT` (`FE-OPT-040`, `041`, `042` ✅ selesai 2026-11-06) |
| [`roadmap/requirement-traceability.md`](roadmap/requirement-traceability.md) | `5` | r1 `approved`; r3..r5 `DRAFT` |

Roadmap tidak dikunci hash, karena statusnya berubah setiap task selesai.

## Penahan yang masih berlaku

| Penahan | Pemilik | Yang tertahan |
| --- | --- | --- |
| Data dan konfigurasi `B1`..`B4` | SDM, Master Data, Farmasi, Tarif, Billing | Uji alur asli dan rilis MVP |
| `OPT-OQ-023` tagihan baru pada kunjungan tertutup | Yasmina | Tidak menahan task; disarankan dikonfirmasi sebelum rilis |
| Pemberitahuan pemilik modul atas enam izin baca (`OPT-DEC-086`) | Yoga Aji Pratama | Tidak menahan pengisian izin; belum tercatat |
| Temuan audit 2026-11-06 `T1`..`T4` ([`testing/readiness-report.md`](testing/readiness-report.md)): izin lintas modul (dokumen selesai di r4, pemberian belum), uji diskon pada Billing (harness selesai `BE-OPT-063`, data asli belum), uji Optik tidak ter-track, aturan tanggungan kosong | Yoga Aji Pratama; Yasmina; Muhammad Hamzah | Rilis MVP. Verdict audit `NOT_READY` |

## Sisa penyegaran

| Artefak | Baris | Pemilik | Keterangan |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` | `OPT-CAP-07` (`OPT-DEC-058`), `OPT-CAP-17` (`OPT-DEC-060`) | `trace-existing-capabilities` | Dicatat `00-interview-decisions.md` bagian 11; belum disegarkan. Kontrak dan arsitektur r2 sudah memuat keputusan terbarunya |
| `03-domain-architecture.md` | Kepala berkas | `hospital-domain-architect` | Masih tertulis `draft`, padahal readiness hulu mencatat `DOMAIN_ARCHITECTURE_READY`; belum dipastikan pemilik |

## Prasyarat sebelum `/plan-module-delivery` (riwayat)

1. Pemilik menyetujui blueprint dan kontrak `v1` (status menjadi `approved`).
2. Pertanyaan memblokir di `04-prd-to-mvp.md` bagian 20 terjawab, terutama persetujuan Billing, Registrasi, Master Data/Farmasi, dan Rekam Medis.
3. Baris registry `Opt` diajukan (task pertama roadmap).
