# Blueprint Manifest — CRM Membership

| Field | Nilai |
|---|---|
| `blueprint_id` | `crm-membership` |
| `revision` | `1` |
| `status` | `approved` |
| `owners` | Product/domain: Yoga Aji Pratama. API: belum ditunjuk (usulan: pemilik arsitektur backend). Security: belum ditunjuk. Frontend authority: Yoga Aji Pratama untuk `CRM-FE-001`/`002`/`004`/`005`, sisanya `DEV_DISCRETION` |
| `approved_by` | Yoga Aji Pratama (product/domain owner) |
| `approved_at` | 2026-10-13 (`CRM-DEC-030`) |
| `backend_commit_sha` | Discovery `36422a4d`, impact scan dan desain `1aecf642` (branch `yoga`) |
| `frontend_commit_sha` | Discovery `9aba9929d`, impact scan dan desain `f1e5e273a` |
| Kesiapan requirement | `PARTIALLY_READY` (revisi 1a): S2 `BUSINESS_DECISION_REQUIRED` |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_PARTIAL` (revision 1) |
| Development lock | **Terbatas** pada `MVP-0` s.d. `MVP-2` (`CRM-DEC-030`). `EPIC CRM-08`/`09` tetap terkunci oleh `DEC-CRM-001` |

## `contract_versions`

| Kontrak | Versi | Status |
|---|---|---|
| API (`contracts/api-contract.md`) | `v2` (as-is `v1`) | `approved` |
| State (`contracts/state-transition-matrix.md`) | `v1` | `approved` |
| Validasi (`contracts/validation-matrix.md`) | `v1` | `approved` |
| Integrasi (`contracts/integration-contract.md`) | `v1` | `approved` |
| Izin dan audit (`contracts/permission-audit-matrix.md`) | `v1` | `approved` |

## `input_revisions` dan `input_hashes`

| Dokumen hulu | Revisi | SHA-256 (12 karakter awal) |
|---|---|---|
| `00-interview-decisions.md` | rev 1, pass 3d (`CRM-DEC-001` s.d. `030`) | `6fcb19ef373c` |
| `01-existing-capability-map.md` | rev 1 + impact scan | `4c2dbff681d3` |
| `02-requirement-completeness-assessment.md` | rev 1a | `c73e34e56e9a` |
| `03-domain-architecture.md` | rev 1 | `439733109233` |
| `approval-requests/bkc-amendment-tagihan-iuran-membership.md` | `draft` | `826454369947` |

## `artifact_hashes`

| Artefak | SHA-256 (12 karakter awal) |
|---|---|
| `02-backend-architecture.md` | `c7c17bc91e97` |
| `03-frontend-architecture.md` | `5d1571541619` |
| `04-prd-to-mvp.md` | `891004cf1d5c` |
| `erd/00-context-erd.md` | `de3e057faa24` |
| `erd/patient-membership.md` | `7d060aa928c0` |
| `erd/data-dictionary.md` | `90b630d7ce50` |
| `contracts/api-contract.md` | `251494fd5f96` |
| `contracts/state-transition-matrix.md` | `e0f58f4faad4` |
| `contracts/validation-matrix.md` | `80e269b2654d` |
| `contracts/integration-contract.md` | `1d7d144f89ea` |
| `contracts/permission-audit-matrix.md` | `d75dc0066fd1` |
| `testing/acceptance-test-matrix.md` | `a6a768d6c5aa` |

## Prasyarat sebelum implementasi

1. Owner menyetujui blueprint revision 1 (atau meminta revisi).
2. ✅ Pemilik registry (Muhammad Hamzah) menyetujui baris folder `HealthServices / PatientManagement / Membership` (prefix `Pat`). Tercatat 2026-10-13.
3. Owner billing-kasir menjawab `DEC-CRM-001` untuk `EPIC CRM-08`/`09`, atau owner secara eksplisit mengizinkan perencanaan terbatas `MVP-0` s.d. `MVP-2`.
4. Owner Registration menyetujui `AG-01`, dan owner rawat-jalan menyetujui `AG-03`.

## Riwayat

| Revisi | Tanggal | Ringkasan |
|---|---|---|
| 1 | 2026-10-13 | Blueprint pertama. Disetujui owner pada hari yang sama (`CRM-DEC-029`, `CRM-DEC-030`) |
