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
  set: 0.1.2   # delta NV-DEC-033, 2026-11-03; sebelumnya 0.1.1 (NV-DEC-031), 0.1.0
  status: approved
registry:
  area: HealthServices
  module: NurseViewManagement
  prefix: Nvw
  status: APPROVED oleh pemilik modul (NV-DEC-029); baris ditulis di MVP-0
input_revisions:
  00-interview-decisions.md: 4 (+ NV-DEC-027, 028, 029, 030, 031, 032, 033)
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

## Revision 2 — tablet bed dan panggilan pasien

Blok di atas tetap catatan revision 1 yang `approved` dan menjadi acuan 14 task MVP-0 s.d. MVP-4.
Blok ini mencatat revision 2, disetujui Rizki Gunawan 2026-11-05 (`NV-DEC-055`).

```yaml
revision: 2
status: approved                   # NV-DEC-055
scope: Amendment revision 5 — tablet bed, panggilan pasien, strip TV, Konsol Panggilan
blueprint_shape: SINGLE            # NV-DEC-036, shape_decided_by USER_CONFIRMED
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
requirement_readiness: READY_FOR_DOMAIN_DESIGN   # NV-S8a..S8d, assessment R2.10
owners:
  product_domain: Rizki Gunawan
  api: Rizki Gunawan
  security: Lead (governance/QBE) — berkas bersama revision 2 direstui dengan syarat NV-DEC-030 (NV-DEC-056, NV-OQ-028 ditutup 2026-11-05)
  frontend_authority: Rizki Gunawan; DEV_DISCRETION NV-FE-006, 007; usulan NV-FE-008..010
  neighbour_modules:
    rawat_inap: Muhammad Hamzah — TLM-NV-002 episode (pemberitahuan baca InpBedPlacement.EndDateTime/EndReason + kabar butir menu konsol di grup Rawat Inap, NV-FE-008); ditulis 2026-11-05, disampaikan dan dijawab 2026-11-09 (TLM-NV-002 bagian 8; NV-DEC-057)
    clinical_policy: Komite Keperawatan (NV-OQ-022, angka ambang)
approved_by: Rizki Gunawan
approved_at: 2026-11-05
backend_commit_sha: 0c6eab34      # branch rizkiG
frontend_commit_sha: 91a11b08f    # branch RizkiV2
contract_versions:
  set: 0.2.1
  status: approved                # 0.2.1: NV-DEC-057, 2026-11-09 (state §5, kamus data R2.3, permission-audit R2 §9); 0.2.0: NV-DEC-055; 0.1.2 tetap berlaku untuk bagian revision 1
proposals_approved: [AD-01, AD-02, AD-03, AD-04, AD-05, AD-06, G-09, G-10, G-11, G-12, G-13, G-14, G-16, G-17, NV-FE-008, NV-FE-009, NV-FE-010]
implementation_gates: []                   # NV-OQ-028 ditutup NV-DEC-056 (2026-11-05); NV-OQ-022 runtime saja, bukan gerbang implementasi
input_revisions:
  00-interview-decisions.md: 5 (NV-DEC-034..057, NV-AC-028..045; NV-AC-042 diubah NV-DEC-057)
  01-existing-capability-map.md: 2 (NV-CAP-17..29)
  02-requirement-completeness-assessment.md: 2 (NV-S8, R2.10; R2.5 NV-OQ-028 ditutup)
input_hashes:            # md5, 12 karakter pertama, 2026-11-05 — sesudah NV-DEC-056
  00-interview-decisions.md: cb16bfd13447          # sesudah NV-DEC-057 (2026-11-09); sebelumnya 9c7cd23add31, saat approval NV-DEC-055 505c91692171
  01-existing-capability-map.md: 9ad8b350b119
  02-requirement-completeness-assessment.md: 82f6a9e2dab7   # sebelumnya a4ce11b45f98
artifact_hashes:         # md5, 12 karakter pertama, 2026-11-05 — seluruh berkas memuat revision 1 + bagian Revision 2
  02-backend-architecture.md: a8bfd619f55a   # NV-DEC-057 (R2.4 reconciliation, enum CloseReason); sebelumnya a3c7827f1292
  03-frontend-architecture.md: d7c435ebc16b
  04-prd-to-mvp.md: a4155103aa87          # sesudah NV-DEC-056 (R2.20); saat approval 7f5ecfb34d63
  flowcharts/00-alur-utama.md: bd44dd2b74db
  flowcharts/04-panggilan-pasien.md: 436df9aa73a3        # baru
  flowcharts/05-penutupan-panggilan.md: 821daea0be8f     # baru
  flowcharts/06-pengelolaan-tablet-bed.md: 7f1fad5d4e95  # baru
  data/data-dictionary.md: 79716f287115   # 0.2.1 NV-DEC-057; sebelumnya c9d6e132d01c
  contracts/api-contract.md: 8dc7b3aae027   # label status §5–§8 dibetulkan ke approved, 2026-11-05; saat approval d85638ceed17
  contracts/state-transition-matrix.md: 6fb323f5509a   # 0.2.1 NV-DEC-057; sebelumnya ae9363a99ca1
  contracts/validation-matrix.md: 8f4fef93d6b7
  contracts/integration-contract.md: bb26c9ca2010
  contracts/permission-audit-matrix.md: 65a52575f4f9   # 0.2.1 NV-DEC-057; sebelumnya 49e9854221be
  testing/acceptance-test-matrix.md: 7590f3722184   # NV-AC-042 diubah NV-DEC-057; sebelumnya 29ecf09745bf
unchanged_in_revision_2: [flowcharts/01-pengelolaan-perangkat.md, flowcharts/02-pembaruan-papan.md, flowcharts/03-pengesahan-interval-ttv.md]
roadmap:                 # plan-module-delivery, roadmap revision 2, 2026-11-05 — bagian "Revision 2" di ketiga berkas
  status: approved       # Rizki Gunawan, 2026-11-05
  approved_by: Rizki Gunawan
  approved_at: 2026-11-05
  backend_tasks: BE-NV-008..BE-NV-016      # 9 task, 11 panah dependency, 6 gelombang
  frontend_tasks: FE-NV-008..FE-NV-015     # 8 task, 12 panah dependency, 3 gelombang
  blocked_tasks: []
  hashes:                # md5, 12 karakter pertama — berkas utuh (revision 1 + bagian Revision 2), saat approval
    roadmap/backend-roadmap.md: de21efd9e904
    roadmap/frontend-roadmap.md: 8840fed391ec
    roadmap/requirement-traceability.md: 6bb818de7611
cross_module_documents:
  rawat-inap/episode-rawat-inap/docs/task-lintas-modul/TLM-NV-002-akhir-penempatan-bed-dan-menu-konsol-panggilan.md:
    hash: aa8a0f26ba58   # sesudah jawaban, NV-DEC-057, dan catatan uji BE-NV-013; sebelumnya 3fae4ee074cd
    status: DIJAWAB — disampaikan Rizki Gunawan, dijawab Muhammad Hamzah 2026-11-09 (bagian 8); ditindaklanjuti NV-DEC-057
go_live_checklist: G-16 (NV-UNK-005..007) — roadmap/requirement-traceability.md R2.8, belum diperiksa
```

### Gerbang sebelum `plan-module-delivery` — revision 2

| Syarat | Keadaan |
|---|---|
| Blueprint revision 2 dan kontrak `0.2.0` disetujui owner | **Terpenuhi** — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Restu berkas bersama `NV-OQ-028` | **Terpenuhi** — lead, lisan, dilaporkan Rizki Gunawan 2026-11-05, dengan syarat `NV-DEC-030` (`NV-DEC-056`) |
| Registry `Nvw` | **Terpenuhi** sejak revision 1 (`ACTIVE`) |
| SHA sama dengan baseline revision 2 | **Ya** — BE `0c6eab34`, FE `91a11b08f` pada 2026-11-05; impact scan tidak diulang |

Tidak memblokir: `NV-OQ-022` (angka ambang, Komite Keperawatan) — panggilan tampil tanpa warna;
`G-16` (`NV-UNK-005`–`007`) — menahan go-live, bukan implementasi.

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
| 2026-11-05 | Amendment pass revision 5 dibuka (`draft`): tablet bed dan panggilan pasien, `NV-DEC-034`–`049` (Konsol Panggilan, scope dikunci, `SINGLE`, tablet melengkapi bel kabel, dua tombol panggil, arti Darurat, klaim tunggal, "Perawat hadir" tekan-tahan, Darurat ringan tidak dinilai, konsol pilih lantai + izin, privasi item, bunyi konsol, bed kosong/pindah, strip tetap, ambang warna aman-gagal, data waktu tanggap) beserta acceptance criteria draft `NV-AC-028`–`041`. Masih terbuka: `NV-OQ-022` (angka, Komite Keperawatan), `NV-OQ-028` (restu lead), dan `NV-UNK-005` di `00-interview-decisions.md`. Hash `00-interview-decisions.md` di atas sengaja belum diperbarui karena amendment belum disetujui | Ya, bila disetujui — revision blueprint naik. Sebelum itu blueprint revision `1` dan kontrak `0.1.2` tetap acuan |
| 2026-11-05 | Impact scan `01-existing-capability-map.md` revision 2 pada backend `0c6eab34` / frontend `91a11b08f`: `NV-CAP-17`–`29` (5 Ready to reuse, 4 Reuse with adapter, 1 Extend, 2 Missing, 1 Conflict), `NV-CON-003`, `NV-UNK-006`–`007`, `NV-CQ-006`–`008`. Bagian revision 1 tidak disunting. Hash `01-existing-capability-map.md` di atas sengaja belum diperbarui | Ya, bila amendment disetujui. Sebelum itu peta revision 1 tetap acuan 14 task MVP |
| 2026-11-05 | Closure pass amendment: `NV-DEC-050` (pembatalan dan koreksi penempatan tidak menutup panggilan), `NV-DEC-051` (prototipe `nurseCallContext.jsx` dihapus di task frontend amendment), `NV-DEC-052` (penutupan karena pindah/pulang paling lambat 60 detik); ditambah `NV-AC-042`, `043`. `NV-CON-003` dan `NV-CQ-006`–`008` tertutup. Sisa: `NV-OQ-028` (lead, memblokir implementasi), `NV-OQ-022` (angka, runtime), `NV-UNK-005`–`007` (runtime/go-live) | Ya, bila amendment disetujui |
| 2026-11-05 | `02-requirement-completeness-assessment.md` revision 2: slice `NV-S8` `PARTIALLY_READY` — `NV-S8a` (perangkat tablet bed) siap; `NV-S8b`–`S8d` `BUSINESS_DECISION_REQUIRED` karena `NV-OQ-034` (penutupan cadangan) dan `NV-OQ-035` (pembatalan oleh pasien); gap usulan `G-09`–`G-17`. Bagian `NV-S1`–`S7` tidak disunting | Ya, bila amendment disetujui |
| 2026-11-05 | Closure: `NV-DEC-053` (tutup dari konsol dengan alasan wajib) dan `NV-DEC-054` (pasien membatalkan panggilan biasa yang belum diklaim); ditambah `NV-AC-044`, `045`. Seluruh `NV-S8` kini `READY_FOR_DOMAIN_DESIGN` (`02-requirement-completeness-assessment.md` R2.10). `G-13` tetap usulan | Ya, bila amendment disetujui |
| 2026-11-05 | `design-business-module`: blueprint revision 2 dan kontrak `0.2.0` sebagai `draft` — bagian "Revision 2" di 11 berkas desain dan tiga flowchart baru (`04`–`06`). Blok revision 2 di atas memuat hash artefak | **Ya** — menunggu approval owner dan restu lead `NV-OQ-028` |
| 2026-11-05 | Approval owner: blueprint revision 2 dan kontrak `0.2.0` disetujui Rizki Gunawan (`NV-DEC-055`), termasuk usulan `AD-01`–`06`, `G-09`–`G-14`, `G-16`, `G-17`, `NV-FE-008`–`010`. Status di seluruh bagian Revision 2 diubah ke `approved`; hash di blok revision 2 diperbarui | Ya — revision blueprint menjadi 2. Implementasi `MVP-6`–`MVP-8` menunggu `NV-OQ-028` |
| 2026-11-05 | `NV-OQ-028` ditutup (`NV-DEC-056`): lead merestui sebelas berkas bersama sesuai `02-backend-architecture.md` R2.5 dan `03-frontend-architecture.md` R2.7 dengan syarat `NV-DEC-030`, dilaporkan Rizki Gunawan secara lisan. Status pertanyaan diperbarui di `00-interview-decisions.md`, `02-requirement-completeness-assessment.md` R2.5, `04-prd-to-mvp.md` R2.20, dan blok revision 2 di atas (`implementation_gates`, `input_hashes`, `artifact_hashes`). Penyebutan "menunggu restu lead" di `02-backend-architecture.md` R2.5, `03-frontend-architecture.md` R2.7, `contracts/integration-contract.md` §7, dan `01-existing-capability-map.md` sengaja tidak disunting; `NV-DEC-056` yang berlaku | Tidak — status pertanyaan terbuka, bukan isi desain; revision tetap `2`, kontrak tetap `0.2.0` |
| 2026-11-05 | `plan-module-delivery` revision 2: bagian "Revision 2" ditambahkan ke `roadmap/backend-roadmap.md` (`BE-NV-008`–`016`), `roadmap/frontend-roadmap.md` (`FE-NV-008`–`015`), dan `roadmap/requirement-traceability.md` (FR, AC, UAT, NFR, DoD, `G-16`, lintas modul). Node, tabel, dan status revision 1 tidak disunting. Dokumen lintas modul `TLM-NV-002` ditulis di folder `episode-rawat-inap`. Grafik revision 2 memakai Mermaid karena berkas yang sama sudah memakai Mermaid (`grafik-dependency-roadmap.md` §13). Roadmap revision 2 berstatus `draft` sampai disetujui Rizki Gunawan | Tidak untuk blueprint; roadmap revision menjadi `2` |
| 2026-11-05 | Approval owner: roadmap revision 2 (17 task, `BE-NV-008`–`016`, `FE-NV-008`–`015`) disetujui Rizki Gunawan ("lanjutkan saya setujui"). Status di metadata ketiga roadmap dan blok `roadmap` di atas diubah ke `approved`. Bersamaan, label "status `draft`" pada baris Contract version `contracts/api-contract.md` §5–§8 dibetulkan menjadi `approved` (`NV-DEC-055`), karena kepala dokumen dan approval kontrak `0.2.0` sudah `approved` sejak `NV-DEC-055` | Tidak — label status, bukan isi kontrak; kontrak tetap `0.2.0` |
| 2026-11-09 | `TLM-NV-002` disampaikan Rizki Gunawan dan dijawab Muhammad Hamzah (bagian 8). Atas jawaban itu Rizki memutuskan `NV-DEC-057`: penutupan sistem mengikuti rantai `SupersededByCorrectionId`; `AdmissionCancelled` ditutup dengan alasan baru "admisi dibatalkan" (`NvwPatientCallCloseReason.AdmissionCancelled = 8`); koreksi ke bed lain ditutup "pasien pindah"; koreksi di bed yang sama tetap aktif. Diubah: `00-interview-decisions.md` (`NV-DEC-057`, `NV-AC-042`, catatan pada `NV-DEC-050`), state §5, kamus data R2.3, permission-audit R2 §9 (ketiganya `0.2.1`), `02-backend-architecture.md` R2.4 dan tabel enum, acceptance-test-matrix `NV-AC-042`, kartu `BE-NV-013` (Cakupan, AC-3, Kontrak) atas keputusan owner, `TLM-NV-002` bagian 2. Hash di atas diperbarui. `NV-FE-008` (letak menu) tidak ditolak Hamzah | **Ya** — kontrak `0.2.1`; revision blueprint tetap `2`. Source `BE-NV-013` disesuaikan di sesi yang sama |

## Pemicu impact scan

Perubahan sesudah approval wajib menaikkan revision/versi. Lihat juga
`01-existing-capability-map.md` bagian "Pemicu impact scan". Tambahan: perubahan
`NumberSeriesAllocator`, `ApplicationUser`, atau pola login display di `use-base-login.jsx`.
