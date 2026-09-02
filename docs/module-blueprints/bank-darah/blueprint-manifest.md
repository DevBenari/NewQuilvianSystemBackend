# Blueprint Manifest — Bank Darah

```yaml
blueprint_id: BD-BP-001
module_name: Bank Darah
module_slug: bank-darah
module_prefix: BD
revision: 10
status: PARTIAL
current_phase: BD-PH-005
created_at: 2026-09-02T00:40:53+07:00
updated_at: 2026-09-02T16:30:00+07:00
last_verified_at: null
backend_source_sha: 792acb9331a65187d052fffd4a292d3bce2fd828
backend_branch: sukmagp
frontend_source_sha: afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254
frontend_branch: sukmagpV2
skill_suite_version: 1.6.0
input_revision_hash: design-business-module-storage-location-2026-09-02
decision_revision: 7
capability_map_revision: 2
prerequisite_readiness_revision: 3
completeness_assessment_revision: 2
domain_architecture_revision: 6
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
  - ARCH-BD-GAP-10
  - OQ-BD-015
contract_versions:
  - version: v1
    status: superseded
    superseded_by: v2
  - version: v2
    status: draft
    last_changed_in: v2
    covers:
      - 02-backend-architecture.md
      - 03-frontend-architecture.md
      - 04-prd-to-mvp.md
      - data/data-dictionary.md
      - contracts/api-contract.md
      - contracts/state-transition-matrix.md
      - contracts/validation-matrix.md
      - contracts/integration-contract.md
      - contracts/permission-audit-matrix.md
      - flowcharts/
      - testing/acceptance-test-matrix.md
owners:
  product_domain: pemilik proses BDRS
  api: pemilik arsitektur backend
  security: pemilik keamanan platform
  frontend: pemilik proses BDRS
approved_by: null
approved_at: null
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
active_roadmap_revision: 1
roadmap_status: STALE
roadmap_stale_reason: roadmap revisi 1 mendahului Storage Location; EPIC BD-11 dan gelombang MVP-1b belum tercermin
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
| `closed_gap_ids` | Daftar gap arsitektur dan pertanyaan terbuka yang sudah ditutup keputusan pemilik. `ARCH-BD-GAP-01`..`09` ditutup `DEC-BD-025`..`034`; `ARCH-BD-GAP-10` ditutup `DEC-BD-037`; `OQ-BD-015` ditutup `DEC-BD-038`. **Tidak ada gap arsitektur yang masih terbuka.** |
| `roadmap_status` | Berubah dari `FORWARD-TEST` menjadi `STALE`. Roadmap revisi 1 disusun sebelum Storage Location masuk, sehingga belum memuat `EPIC BD-11` maupun gelombang `MVP-1b`. Perlu `plan-module-delivery` ulang. |
| `contract_versions` | Set kontrak desain **`v2`** berstatus `draft`, hasil design-business-module update pass 2 September 2026 yang menyerap Storage Location dan gerbang pemberian. `v1` ditandai `superseded`. Set kontrak berlaku sebagai **satu himpunan**; seluruh berkas yang dicakup ikut naik ke `v2`. |
| `owners` | Pemilik per sumbu (product/domain, API, security, frontend). `approved_by`/`approved_at` kosong: desain masih `draft`. |
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

**Design pass 2 September 2026.** `design-business-module` menghasilkan set kontrak `v1` (`draft`) pada
`02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `data/`, `contracts/`,
`flowcharts/`, dan `testing/`. Seluruhnya memakai **prefix placeholder `Bbk`** yang belum disahkan
(`BD-DEP-008`), sehingga pembuatan entity operasional tetap `BLOCKED`. `04-prd-to-mvp.md` masih memuat
pertanyaan memblokir (`BD-DEP-008`, `DEF-BD-004`) sehingga **belum boleh** diteruskan ke
`plan-module-delivery` sampai keduanya tuntas. Approval manusia belum diklaim.

**Design update pass 2 September 2026 — set kontrak `v2`.** Menyerap empat keputusan Storage Location
dan gerbang pemberian (`DEC-BD-035` sampai `DEC-BD-038`) dari register keputusan revisi 7 dan arsitektur
domain revisi 6.

Yang bertambah pada desain: master `MstBloodStorageLocation`, entity `BbkBloodUnitPlacement`, dua nilai
status kantong di depan (`Received`, `Stored`), kolom `BbkBloodUnit.CurrentPlacementId`, kolom
`BbkEmergencyAuthorization.BypassScope` beserta enum `BbkEmergencyBypassScope`, action hak akses
`Store`, resource `BloodStorageLocation`, layar `FE-BD-10`, epic `EPIC BD-11`, dan gelombang `MVP-1b`.

Yang **tidak** bertambah, dan itu disengaja: nol batas integrasi baru, nol daftar kerja operasional
baru, nol background job, dan nol perubahan pada batas Billing.

Dua catatan yang mengikat pembaca berikutnya:

- **`MstDrugStorageLocation` sengaja tidak dipakai ulang** walaupun sudah ada dan punya tipe
  `ColdStorage`. Penolakannya `DEC-BD-035`, dan alasannya dicatat pada `02-backend-architecture.md` §D
  dan §L serta `contracts/integration-contract.md` §1b — supaya tidak tersambung tanpa sengaja nanti.
- **Master lokasi penyimpanan wajib terisi sebelum go-live.** Tanpa satu pun lokasi aktif, tidak ada
  kantong yang dapat disimpan, dialokasikan, maupun diberikan (`INV-BD-025`). Ini konsekuensi
  *fail-closed* yang disengaja, bukan cacat rancangan.

`roadmap/00-delivery-plan.md` **ditandai `STALE`**: ia disusun sebelum Storage Location masuk. Kedua
pemblokir lama (`BD-DEP-008`, `DEF-BD-004`) **tidak berubah** oleh pass ini; keduanya tetap menahan
penerusan ke `plan-module-delivery`. Approval manusia belum diklaim.
