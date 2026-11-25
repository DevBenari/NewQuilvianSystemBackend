# Kiosk — Blueprint Manifest

```yaml
blueprint_id: KSK-BP-001
module_name: Kiosk (Revisi Module Kiosk & Cek Nomor Rekam Medis)
module_slug: kiosk
module_prefix: KSK
entity_prefix: Reg                   # RegistrationManagement / Registration — MODULE_OWNERSHIP_PREFIX_REGISTRY.md baris 23
revision: 1
status: approved                     # disetujui Sukma Giri Pratama 30 Sep 2026
blueprint_shape: SINGLE
shape_decided_by: USER_CONFIRMED     # KSK-DEC-004, 30 Sep 2026
created_at: 2026-09-30T00:00:00+07:00
updated_at: 2026-09-30T16:00:00+07:00
last_verified_at: null

tahap_saat_ini: KSK-PH-003           # roadmap approved — build BE-KSK-001
tahap_selesai:
  - intake                           # PRD Tim Bisnis 30 Sep 2026 dicatat sebagai bukti input
  - audit_awal                       # audit read-only singkat, belum capability map resmi
  - scope_pass                       # grill-me, 30 Sep 2026 — KSK-DEC-001..012
  - capability_audit                 # trace-existing-capabilities, 30 Sep 2026 — 01-existing-capability-map.md r1
  - closure_pass                     # grill-me, 30 Sep 2026 — KSK-DEC-013..019
  - requirement_completeness_gate    # KSK-RCG-001 r1, 30 Sep 2026 — READY_FOR_DOMAIN_DESIGN (9/9 slice)
  - design_business_module           # 30 Sep 2026 — 14 berkas + flowchart proses, KSK-CONTRACT-v1
  - approval_blueprint               # Sukma Giri Pratama, 30 Sep 2026 (termasuk KSK-DSN-001..009, KSK-GAP-001..011)
  - plan_module_delivery             # 30 Sep 2026 — KSK-RM-BE-001 r1 (3 task), KSK-RM-FE-001 r1 (8 task), KSK-TRACE-001 r1
tahap_belum_dijalankan:
  - build_module_backend
  - build_module_frontend

kesiapan_requirement: READY_FOR_DOMAIN_DESIGN
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN   # opsional; alasan di KSK-RCG-001 §9

owners:
  product_domain: Sukma Giri Pratama # atas nama Tim Bisnis — KSK-DEC-005
  implementation: Sukma Giri Pratama # sukmagp
  security_privacy: BELUM DITUNJUK
  approver_blueprint: Sukma Giri Pratama   # KSK-DEC-005

backend_source_sha: 419b910fca5188285946850a95976ebff83ae8ce   # branch sukmagp
frontend_source_sha: 4ec51b0bf5e724e899b95f118e273351437b71e7

skill_suite_version: 1.18.0
input_revision_hash: KSK-PRD-2026-09-30-r1
decision_revision: 2                 # 00-interview-decisions.md r2
contract_versions: KSK-CONTRACT-v1     # approved — api, state-transition, validation, integration, permission-audit
active_dependency_ids: [KSK-OQ-004, KSK-OQ-005]
approved_by: Sukma Giri Pratama
approved_at: 2026-09-30
active_roadmap_revision: KSK-RM-BE-001 r1 / KSK-RM-FE-001 r1 (approved Sukma 2026-09-30)
supersedes: null

input_revisions:
  interview_decisions: 2
  existing_capability_map: 1
  requirement_completeness_assessment: 1   # KSK-RCG-001

artifact_hashes:                     # sha256, 12 karakter pertama — dihitung ulang setelah approval & roadmap
  00-interview-decisions.md: 186c4182779a
  01-existing-capability-map.md: 0b28195886ca
  02-requirement-completeness-assessment.md: f85c26e4fc76
  02-backend-architecture.md: 6a4c89bda80a
  03-frontend-architecture.md: 7b3ac2d05c4f
  04-prd-to-mvp.md: 757640f112aa
  data/data-dictionary.md: 3b2b251c3d1a
  contracts/api-contract.md: 9642e4257658
  contracts/integration-contract.md: 369bf2ef9a96
  contracts/permission-audit-matrix.md: 3340e791c350
  contracts/state-transition-matrix.md: d733049b6d49
  contracts/validation-matrix.md: c142511582b9
  flowcharts/00-alur-utama.md: 369814a6b192
  flowcharts/cek-nomor-rekam-medis.md: 0ecbbdcf9f30
  flowcharts/pendaftaran-pasien-lama.md: a74474511656
  flowcharts/penjamin-utama.md: a18ff2a78dc2
  testing/acceptance-test-matrix.md: f7df3f3fa9c6
  roadmap/backend-roadmap.md: ac54a31226e7
  roadmap/frontend-roadmap.md: 4d3e936e5a93
  roadmap/requirement-traceability.md: 6b6ed116f838
```

## Bukti input

- [evidence/2026-09-30-prd-revisi-kiosk.md](evidence/2026-09-30-prd-revisi-kiosk.md) — register requirement PRD Tim Bisnis beserta temuan audit awal.

## Revisi 2 — Amendment PO: Pendaftaran Pasien Online dan Check-in Kiosk (`2026-11-19`)

| Field | Nilai |
| --- | --- |
| `revision` | `2` |
| `status` | `approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`). Revisi 1 (blok YAML di atas) tetap `approved` dan tidak berubah |
| `blueprint_shape` | `SINGLE`, `shape_decided_by: USER_CONFIRMED` (`KSK-DEC-028`) |
| Scope | Diperluas menjadi "layanan mandiri pasien: perangkat kiosk dan halaman publik" (`KSK-DEC-026`) |
| `contract_versions` | `KSK-CONTRACT-v2` (`approved` 2026-11-19, aditif dari `v1`): bagian *Amendment PO* pada kelima berkas `contracts/` (`last_changed_in` `v2` pada bagian itu saja) |
| `decision_revision` | `KSK-DEC-026` s.d. `053` (`approved`; `049` `superseded` oleh `053`), `KSK-PO-FE-001` s.d. `005`, `KSK-PO-AS-002`, `003` |
| `requirement_readiness` | `READY_FOR_DOMAIN_DESIGN` 8/8 slice — [02-requirement-completeness-assessment-pendaftaran-online.md](02-requirement-completeness-assessment-pendaftaran-online.md) (`KSK-RCG-002` r2) |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — satu bounded context Registration, billing nihil, keselamatan klinis terbatas pada identifikasi yang sudah diputuskan (`KSK-RCG-002` §9.7) |
| Capability | [01-capability-impact-scan-pendaftaran-online.md](01-capability-impact-scan-pendaftaran-online.md) (`CAP-PO-01` s.d. `37`). `01-existing-capability-map.md` r1 **stale** untuk area Amendment PO |
| `backend_source_sha` | `5e777833` (`sukmagp`) |
| `frontend_source_sha` | `5f18e9f04` (`sukmagpV2`) |
| Artefak | `02` *Amendment PO* (PO.1–PO.15), `03` *Amendment PO* (PO-FE.1–7), `04` *Amendment PO* (PO-1–20), `data/data-dictionary.md` *Amendment PO* (PO-D.1–7), `contracts/*` *Amendment PO*, `testing/acceptance-test-matrix.md` *Amendment PO* (PO-T.1–4), [flowcharts/pendaftaran-pasien-online.md](flowcharts/pendaftaran-pasien-online.md), [flowcharts/check-in-pasien-online.md](flowcharts/check-in-pasien-online.md), [flowcharts/pendaftaran-online-petugas-dan-penutupan.md](flowcharts/pendaftaran-online-petugas-dan-penutupan.md), bagian baru di `flowcharts/00-alur-utama.md` |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-19 — seluruh `KSK-DSN-PO-001` s.d. `017` dan letak menu `KSK-PO-FE-005` (`KSK-DEC-054`) |
| Migration | `AddRegOnlineRegistration` (tiga tabel baru). Dibuat dan dijalankan hanya dengan persetujuan terpisah |
| Go-live | Menunggu `KSK-PO-OQ-010` (domain publik dan IP asli), `015` (akun SMTP), `016` (kunci Turnstile), serta centang Akses Role `OnlineRegistration` |
| Roadmap | r1 — [online-registration-backend-roadmap.md](roadmap/online-registration-backend-roadmap.md) (`KSK-PO-RM-BE-001`, `BE-KSK-PO-001` s.d. `010`), [online-registration-frontend-roadmap.md](roadmap/online-registration-frontend-roadmap.md) (`KSK-PO-RM-FE-001`, `FE-KSK-PO-001` s.d. `008`), [online-registration-traceability.md](roadmap/online-registration-traceability.md) (`KSK-PO-TRACE-001`). `approved` — Sukma Giri Pratama, 2026-11-19 (`KSK-DEC-055`) |

`artifact_hashes` (SHA-256, 12 karakter awal, 2026-11-19, dihitung sebelum approval; sesudah approval hanya label status yang berubah; hash `00-interview-decisions.md` dihitung sesudah catatan desain ditambahkan, sedangkan hash `blueprint-manifest.md` tidak dicatat karena berkas ini memuat tabel ini sendiri):

```yaml
  00-interview-decisions.md: af1c911b138a
  01-capability-impact-scan-pendaftaran-online.md: e948a162a09c
  02-requirement-completeness-assessment-pendaftaran-online.md: e99597a158da
  02-backend-architecture.md: 8f7d19927959
  03-frontend-architecture.md: 47f1a9a9a220
  04-prd-to-mvp.md: 2c77035bb3ea
  data/data-dictionary.md: 4cf45d1a5b84
  contracts/api-contract.md: 210adf186d70
  contracts/integration-contract.md: a4e9b51f5625
  contracts/permission-audit-matrix.md: cf0eec2975fc
  contracts/state-transition-matrix.md: 364e284545e0
  contracts/validation-matrix.md: 161674b4c08f
  flowcharts/00-alur-utama.md: 13056d9c8f0e
  flowcharts/pendaftaran-pasien-online.md: be194bec0fed
  flowcharts/check-in-pasien-online.md: 18609076c51f
  flowcharts/pendaftaran-online-petugas-dan-penutupan.md: ea366609af74
  testing/acceptance-test-matrix.md: a98148d12e4c
```

## Revisi 2.1 — Faskes perujuk mitra di halaman publik (`2026-11-25`, `approved`)

| Field | Nilai |
| --- | --- |
| `revision` | `2.1` (aditif di atas revisi 2) |
| `status` | `approved` — Sukma Giri Pratama, 25 Nov 2026 (`KSK-DEC-058`). Revisi 2 tetap `approved` |
| `blueprint_shape` | `SINGLE` (tidak berubah) |
| `contract_versions` | `KSK-CONTRACT-v2.1` (`approved` 2026-11-25): `contracts/api-contract.md` (baris `GET /public/options/referral-institutions` dan *Perubahan v2.1*), `contracts/validation-matrix.md` (`KSK-VAL-PO-073`, `074`). Tiga berkas kontrak lain tidak berubah dan tetap `last_changed_in` `v2` |
| `decision_revision` | `KSK-DEC-056` (arahan Cetak Antrean tetap; tanpa perubahan desain), `KSK-DEC-057` (faskes perujuk mitra) — `approved` Sukma 2026-11-25 |
| Desain | `KSK-DSN-PO-018` (`approved`); `02` PO.4.5 dan PO.15; `03` wilayah Data Rujukan; `04` `FR-KSK-PO-015`, `043`; flowchart pendaftaran pasien online; `testing/acceptance-test-matrix.md` tiga baris `KSK-DEC-057` |
| Skema | Tidak berubah. Tanpa migration |
| `backend_source_sha` | `356bf463` (`sukmagp`) |
| `frontend_source_sha` | `7bba5cd1c` (`sukmagpV2`) |
| Impact scan | Terbatas, read-only, 2026-11-25: `OnlineRegistrationPublicService.GetReferralInstitutionOptionsAsync` (saringan `IsActive` saja), validasi faskes saat kirim (`OnlineRegistrationPublicService.Submit` — cek aktif saja), `ReferralPartnerEligibility.IsEligibleOn`/`FindEligibleAsync` (dipakai `EncounterReferralService`, partner options RJ), hook frontend `use-online-registration-referral-institutions.js`. Urutan langkah Rujukan: Data Kunjungan → Data Rujukan, sehingga `visitDate` tersedia |
| Kompatibilitas | Query wajib baru pada satu endpoint publik; konsumen tunggal halaman publik → backend dan frontend dirilis bersama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-25 (`KSK-DEC-058`) |
