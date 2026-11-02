# Blueprint Manifest — Nurse View

```yaml
blueprint_id: nurse-view
module_name: Nurse View — display pemantauan perawat per lantai
revision: 1
status: approved                   # NV-DEC-028
blueprint_shape: SINGLE
shape_decided_by: USER_CONFIRMED          # NV-DEC-002, dikukuhkan NV-DEC-005
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
requirement_readiness: READY_FOR_DOMAIN_DESIGN   # NV-S1..NV-S7
owners:
  product_domain: Rizki Gunawan
  api: Rizki Gunawan
  security: Lead (governance/QBE) — berkas bersama direstui dengan syarat, NV-DEC-030
  frontend_authority: Rizki Gunawan; DEV_DISCRETION sesuai NV-FE-004, 005
  neighbour_modules:
    rawat_inap: Muhammad Hamzah — TLM-NV-001 keperawatan & episode disetujui 2026-10-28 (NV-DEC-027)
    gizi: Kepala Instalasi Gizi (tidak tersentuh scope saat ini)
    clinical_policy: Komite Keperawatan (NV-OQ-010)
approved_by: Rizki Gunawan
approved_at: 2026-10-28
backend_commit_sha: a6f977a3      # branch rizkiG
frontend_commit_sha: 76b0c6e4b    # branch RizkiV2
contract_versions:
  set: 0.1.0
  status: approved
registry:
  area: HealthServices
  module: NurseViewManagement
  prefix: Nvw
  status: APPROVED oleh pemilik modul (NV-DEC-029); baris ditulis di MVP-0
input_revisions:
  00-interview-decisions.md: 2 (+ NV-DEC-027, 028, 029, 030)
  01-existing-capability-map.md: 1
  02-requirement-completeness-assessment.md: 1
  rawat-inap/keperawatan/docs/task-lintas-modul/TLM-NV-001: DISETUJUI 2026-10-28
  rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-001: DISETUJUI 2026-10-28
input_hashes:            # md5, 12 karakter pertama
  00-interview-decisions.md: e6386091b9fb
  01-existing-capability-map.md: a6251f2b2be1
  02-requirement-completeness-assessment.md: 38d40ea21730
  TLM-NV-001-keperawatan: 1f6b484856ab
  TLM-NV-001-episode: 4c8f41f22e85
artifact_hashes:         # md5, 12 karakter pertama — saat approval
  02-backend-architecture.md: d33c88757566
  03-frontend-architecture.md: 2c4a85acba33
  04-prd-to-mvp.md: 5b1cf723fff3
  flowcharts/00-alur-utama.md: c5b0049c0547
  flowcharts/01-pengelolaan-perangkat.md: 194f843af04d
  flowcharts/02-pembaruan-papan.md: e867e48c1200
  flowcharts/03-pengesahan-interval-ttv.md: 9fda6655f3d0
  data/data-dictionary.md: 23217b802623
  contracts/api-contract.md: e042863badb2
  contracts/state-transition-matrix.md: e967de7b87e7
  contracts/validation-matrix.md: c8985d4969da
  contracts/integration-contract.md: 253e96721ded
  contracts/permission-audit-matrix.md: ad22aa3099b6
  testing/acceptance-test-matrix.md: 75a97cfe1d33
roadmap:                 # plan-module-delivery, revision 1, 2026-10-28
  roadmap/backend-roadmap.md: 9c6d8fe553d5   # BE-NV-001..007
  roadmap/frontend-roadmap.md: d554d14d8f5f  # FE-NV-001..007
  roadmap/requirement-traceability.md: ebb039e8f2c0
```

## Berkas

| Berkas | Isi | Ditulis oleh |
|---|---|---|
| `00-analisa-awal.md` | Jejak analisa awal (sebagian dilampaui) | Analisa awal |
| `00-interview-decisions.md` | Decision log | `grill-me` |
| `01-existing-capability-map.md` | Peta kemampuan existing | `trace-existing-capabilities` |
| `02-requirement-completeness-assessment.md` | Gerbang kelengkapan | `requirement-completeness-gate` |
| `02-backend-architecture.md` … `testing/` | Desain (14 berkas canonical `SINGLE`) | `design-business-module` |

## Gerbang sebelum `plan-module-delivery`

| Syarat | Keadaan |
|---|---|
| Blueprint dan kontrak `0.1.0` disetujui owner | **Terpenuhi** — Rizki Gunawan, 2026-10-28 |
| Persetujuan owner Rawat Inap atas TLM | **Terpenuhi** — Muhammad Hamzah, 2026-10-28 |
| Restu berkas bersama `NV-OQ-015` | **Terpenuhi** — lead, lisan, 2026-10-28, dengan syarat (`NV-DEC-030`) |
| Registry `Nvw` `NV-OQ-017` | **Terpenuhi** — pemilik modul, `NV-DEC-029`; baris ditulis di MVP-0 |

Tidak memblokir: `NV-OQ-010` (Komite Keperawatan) — tanda lewat jadwal aman-gagal.

## Catatan sesudah approval

| Tanggal | Perubahan | Material? |
|---|---|---|
| 2026-10-28 | `NV-OQ-017` ditutup (`NV-DEC-029`): status pertanyaan dan prasyarat MVP-0 diperbarui di `00`, `02-backend-architecture`, `02-requirement-completeness-assessment`, `04-prd-to-mvp`, `data-dictionary`. Hash di atas sudah diperbarui | Tidak — status pertanyaan terbuka, bukan isi desain; revision tetap `1` |
| 2026-10-28 | `NV-OQ-015` ditutup (`NV-DEC-030`): restu lead dengan syarat; ditambah kriteria regresi login `NV-AC-023`–`027` dan `UAT-11`. Desain tidak berubah — syarat lead sudah sejalan dengan `NV-DEC-022` (meniru display antrean) | Tidak — penambahan bukti uji dan status pertanyaan; revision tetap `1` |

## Pemicu impact scan

Perubahan sesudah approval wajib menaikkan revision/versi. Lihat juga
`01-existing-capability-map.md` bagian "Pemicu impact scan". Tambahan: perubahan
`NumberSeriesAllocator`, `ApplicationUser`, atau pola login display di `use-base-login.jsx`.
