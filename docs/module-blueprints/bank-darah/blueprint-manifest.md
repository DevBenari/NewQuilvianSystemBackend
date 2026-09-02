# Blueprint Manifest — Bank Darah

```yaml
blueprint_id: BD-BP-001
module_name: Bank Darah
module_slug: bank-darah
module_prefix: BD
revision: 7
status: PARTIAL
current_phase: BD-PH-005
created_at: 2026-09-02T00:40:53+07:00
updated_at: 2026-09-02T06:00:00+07:00
last_verified_at: null
backend_source_sha: db08c14dbfb9d6b704e8d0bdfb4fd05e2b52a8cb
backend_branch: sukmagp
frontend_source_sha: afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254
frontend_branch: sukmagpV2
skill_suite_version: 1.6.0
input_revision_hash: grill-me-architecture-gap-final-closure-pass-2026-09-02
decision_revision: 4
capability_map_revision: 2
prerequisite_readiness_revision: 3
completeness_assessment_revision: 2
domain_architecture_revision: 3
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY
closed_gap_ids:
  - ARCH-BD-GAP-01
  - ARCH-BD-GAP-02
  - ARCH-BD-GAP-03
  - ARCH-BD-GAP-04
  - ARCH-BD-GAP-05
  - ARCH-BD-GAP-06
  - ARCH-BD-GAP-07
  - ARCH-BD-GAP-08
  - ARCH-BD-GAP-09
contract_versions: []
active_dependency_ids:
  - BD-DEP-001
  - BD-DEP-002
  - BD-DEP-003
  - BD-DEP-004
  - BD-DEP-005
  - BD-DEP-006
  - BD-DEP-007
  - BD-DEP-008
  - BD-DEP-009
  - BD-DEP-010
  - BD-DEP-011
  - BD-DEP-012
  - BD-DEP-013
  - BD-DEP-014
  - BD-DEP-015
active_roadmap_revision: null
supersedes: null
```

## Penjelasan isi manifest

Manifest ini adalah kartu identitas modul Bank Darah. Ia menjawab satu pertanyaan: versi keputusan
mana yang sedang berlaku, dan atas dasar source code versi berapa keputusan itu dibuat.

| Field | Arti dalam bahasa sehari-hari |
| --- | --- |
| `blueprint_id` | Nomor identitas blueprint. Ditetapkan sekali dan tidak pernah diganti. |
| `module_prefix` | Awalan `BD` dipakai untuk penomoran keputusan, fase, dependency, dan task blueprint. **Bukan** awalan penamaan entity backend — awalan itu terpisah dan belum terdaftar. Lihat `BD-DEP-008`. |
| `revision` | Naik hanya bila arsitektur target, kontrak, dependency, atau keputusan yang sudah disetujui berubah secara berarti. Tidak naik hanya karena status berubah. |
| `status` | `PARTIAL` berarti sebagian slice sudah siap dirancang sementara slice lain terblokir keputusan bisnis. |
| `current_phase` | Fase yang sedang berjalan, yaitu `BD-PH-005` Penyusunan Blueprint Target. |
| `last_verified_at` | Masih kosong karena belum ada verifikasi kesiapan yang dijalankan. |
| `backend_source_sha` | Versi source backend yang menjadi dasar seluruh keputusan di blueprint ini. Bila SHA ini berubah, bukti yang bergantung padanya ditandai `STALE` dan perlu tinjauan dampak terbatas. Naik `9522caa` → `9dc7637` → `db08c14` pada 2 September 2026; setiap tinjauan dampak sudah dijalankan dan hasilnya nihil karena seluruh perbedaannya hanya dokumen blueprint Bank Darah. |
| `input_revision_hash` | Menunjuk asal keputusan: sesi wawancara Grill Me architecture gap final closure pass tanggal 2 September 2026, yang melanjutkan scope pass, closure pass, dan architecture gap closure pass di hari yang sama. |
| `closed_gap_ids` | Daftar gap arsitektur yang sudah ditutup keputusan pemilik. `ARCH-BD-GAP-01` sampai `ARCH-BD-GAP-09` seluruhnya tertutup oleh `DEC-BD-025` sampai `DEC-BD-034`. |
| `contract_versions` | Masih kosong karena belum ada kontrak API, ERD, atau integrasi yang dibekukan. |
| `supersedes` | Kosong karena blueprint ini tidak menggantikan blueprint lain. |

## Peringatan yang melekat

Audit kemampuan existing sudah dijalankan pada 2 September 2026 dan hasilnya ada di
`02-existing-capability-map.md`. Peringatan "scope dikunci tanpa audit" **sudah dicabut**.

Peta kemampuan itu terikat pada backend `9522caa` dan frontend `afbb8ab`. Backend sudah bergerak ke
`db08c14` (lewat `9dc7637`), dan pemindaian dampak terbatas sudah dijalankan pada 2 September 2026:
seluruh perbedaannya hanya dokumen blueprint Bank Darah, nol berkas source aplikasi. Peta **tidak**
ditandai `STALE`. Bila SHA berubah lagi, ulangi pemindaian yang sama sebelum peta dipakai.

Blueprint tidak memberi wewenang implementasi. Menulis dokumen di sini tidak sama dengan izin
mengubah controller, service, entity, migration, database, atau melakukan deployment.
