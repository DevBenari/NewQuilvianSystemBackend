# Blueprint Modul Accounting

Blueprint `ACC-BP-001` revisi 3 untuk modul Accounting Quilvian V2. Status modul `PARTIAL`:
seluruh keputusan bisnis dan blueprint target sudah selesai, pembuatan entity dan migration
masih terblokir prasyarat teknis.

**Seluruh artefak berstatus `draft`. Approval adalah tindakan manusia dan belum diberikan.**

## Lima belas artefak canonical

| Berkas | Isi | Status |
|---|---|---|
| [blueprint-manifest.md](blueprint-manifest.md) | Identitas, SHA sumber, versi kontrak, hash artefak | Berlaku |
| [00-interview-decisions.md](00-interview-decisions.md) | 37 keputusan `ACC-DEC-001` sampai `ACC-DEC-037` | 37 dari 37 tertutup |
| [01-existing-capability-map.md](01-existing-capability-map.md) | Keadaan kode saat ini, `ACC-CAP-001` sampai `ACC-CAP-014` | Parsial |
| [02-backend-architecture.md](02-backend-architecture.md) | Bounded context, kepemilikan data, class diagram, folder, migration, master awal | `draft` |
| [03-frontend-architecture.md](03-frontend-architecture.md) | Kontrak fungsional layar, slice, kewenangan UI | `draft` |
| [04-prd-to-mvp.md](04-prd-to-mvp.md) | Batas rilis pertama, 8 epic, 30 functional requirement, Definition of Done | `draft` |
| [erd/00-context-erd.md](erd/00-context-erd.md) | Peta antar bounded context | `draft` |
| [erd/01-chart-of-account.md](erd/01-chart-of-account.md) | ERD daftar akun dan jenis jurnal | `draft` |
| [erd/02-journal.md](erd/02-journal.md) | ERD jurnal, baris, dan riwayat persetujuan | `draft` |
| [erd/03-accounting-period.md](erd/03-accounting-period.md) | ERD periode akuntansi | `draft` |
| [erd/data-dictionary.md](erd/data-dictionary.md) | Kamus data seluruh kolom beserta bentuk DDL | `draft` |
| [contracts/api-contract.md](contracts/api-contract.md) | 30 endpoint bergaya Swagger, seluruhnya rencana | `ACC-API-0.1` |
| [contracts/state-transition-matrix.md](contracts/state-transition-matrix.md) | Perpindahan status yang sah **dan yang tidak sah** | `ACC-STATE-0.1` |
| [contracts/validation-matrix.md](contracts/validation-matrix.md) | Aturan validasi beserta pesan untuk pengguna | `ACC-VALIDATION-0.2` |
| [contracts/integration-contract.md](contracts/integration-contract.md) | Tidak berlaku untuk MVP; batas dan gerbang Phase 2 | `ACC-INTEGRATION-0.2` |
| [contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md) | Enam peran, string permission, dan apa yang dicatat logger | `ACC-PERMISSION-0.1` |
| [contracts/cross-module-contract.md](contracts/cross-module-contract.md) | **Wajib dibaca Finance/Yasmin.** Envelope kejadian, mata uang, idempotency, semantik penolakan | `ACC-XMOD-0.1` |
| [06-shared-migration-coordination-rule.md](06-shared-migration-coordination-rule.md) | Usulan `QBE-MIG-001`/`002`. Aturan bersama, bukan milik Accounting | `PROPOSED` |
| [testing/acceptance-test-matrix.md](testing/acceptance-test-matrix.md) | 30 requirement dan 19 skenario UAT, termasuk jalur gagal | `ACC-TEST-0.1` |
| [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) | 14 task `BE-ACC-001` sampai `014` dalam empat gelombang | `DRAFT_FORWARD_TEST` |
| [roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md) | 11 task `FE-ACC-001` sampai `011` | `DRAFT_FORWARD_TEST` |
| [roadmap/requirement-traceability.md](roadmap/requirement-traceability.md) | Peta requirement ke task ke bukti, beserta coverage gap | `DRAFT_FORWARD_TEST` |
| [evidence/01-design-verification-evidence.md](evidence/01-design-verification-evidence.md) | Sembilan bukti verifikasi yang dapat diulang siapa pun | Berlaku |
| [evidence/02-frontend-rebaseline-impact-scan.md](evidence/02-frontend-rebaseline-impact-scan.md) | Impact scan re-baseline SHA frontend `fc49cc7` → `31a82c8` | Berlaku |

Berkas pendukung di luar daftar canonical:

| Berkas | Isi |
|---|---|
| [MODULE-STATUS.md](MODULE-STATUS.md) | Status modul, fase, penghalang, tindakan berikutnya |
| [00-business-overview.md](00-business-overview.md) | Untuk apa modul ini, batas kepemilikan, istilah dasar |
| [05-prerequisite-readiness.md](05-prerequisite-readiness.md) | Empat prasyarat, dua di antaranya memblokir |

## Yang harus dibaca lebih dulu

Bila melanjutkan pekerjaan ini pada sesi lain, baca dengan urutan berikut:

1. [MODULE-STATUS.md](MODULE-STATUS.md) — posisi terakhir dan apa yang memblokir.
2. [roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) — task mana yang `READY` dan mana yang `BLOCKED`.
3. [04-prd-to-mvp.md](04-prd-to-mvp.md) bagian 20 — pertanyaan terbuka sebelum development lock.
4. [05-prerequisite-readiness.md](05-prerequisite-readiness.md) — apa yang belum boleh dikerjakan.

## Tiga hal yang belum boleh dikerjakan

1. **Membuat entity Accounting**, sebelum prefix penamaannya terdaftar (`ACC-DEP-002`).
2. ~~Membuat migration sebelum snapshot EF dipulihkan~~ — **`ACC-DEP-001` sudah selesai** 30 Agustus 2026.
3. **Menjalankan task mana pun**, sebelum blueprint disetujui owner. Roadmap sudah ada, tetapi
   berstatus `DRAFT_FORWARD_TEST` — bentuknya siap ditinjau, belum menjadi perintah kerja.

Ketiganya bukan pendapat, melainkan akibat dari keadaan repository dan aturan tata kelola yang
berlaku.

## Catatan gerbang

Accounting MVP diklasifikasikan sebagai kemampuan **non-rumah-sakit**, sehingga
`requirement-completeness-gate` dan `hospital-domain-architect` tidak dijalankan. Dasarnya,
beserta syarat yang mengikat, ada di [02-backend-architecture.md](02-backend-architecture.md)
bagian 1.

**Phase 2 tidak mendapat kelonggaran itu.** Kedua skill tersebut wajib dijalankan sebelum
integrasi dengan Billing dan Finance dirancang.
