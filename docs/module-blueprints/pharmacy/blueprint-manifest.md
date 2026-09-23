# Pharmacy Blueprint Manifest

```yaml
blueprint_id: PHA-BP-001
module_name: Farmasi
module_slug: pharmacy
module_prefix: PHA
revision: 4
status: approved
status_derivation_revision_4: >
  Revisi 4 menambahkan slice BARU "Financial Clearance Handoff", sementara slice Routing Depo
  beserta seluruh kontrak PHA-*-ROUTING-v1 TETAP `approved` apa adanya dan tidak disentuh.
  Status modul sempat diturunkan menjadi `partial` ketika slice baru masih `draft`, lalu kembali
  `approved` pada hari yang sama setelah PHA-DEC-071 menyetujui PHA-DES-001-006. Kedua slice kini
  sama-sama `approved` di dalam satu blueprint SINGLE.
  Yang TIDAK ikut disetujui, dan MUST tetap diminta terpisah: wewenang menulis source, pembuatan
  migration, dan eksekusinya. Ditambah satu kenyataan teknis yang tidak dapat dicabut approval
  siapa pun — slice ini menunggu sisi penerbit Billing berdiri lebih dulu.
blueprint_shape: SINGLE
shape_decided_by: EXISTING_STRUCTURE
shape_note: >
  Field ini BARU dicatat pada revisi 4; manifest sebelumnya tidak memuatnya karena dibuat sebelum
  field ini diperkenalkan. Nilainya BUKAN pilihan baru agent, melainkan pencatatan keadaan yang
  sudah berjalan sejak blueprint ini berdiri: seluruh berkas berada langsung di folder modul,
  tanpa satu pun folder sub-modul. Bila kelak modul ini hendak dipecah, itu revisi material
  tersendiri yang menuntut keputusan pemilik, bukan efek samping desain.
current_phase: PHA-PH-009
created_at: 2026-08-18T00:00:00+07:00
updated_at: 2026-09-21T00:00:00+07:00
last_verified_at: 2026-09-21T00:00:00+07:00
backend_source_sha: 6782ae652ca53299f7469c49b2edb64d23e77b60
frontend_source_sha: 1b138b9aac7a50524fd751a47c9a76e0a55f8803
previous_backend_source_sha: 767470f742bc6f2eebadbd653a873f69d6f93121
previous_frontend_source_sha: 400104f2a0f3239c14c40f5905b419977a538450
sha_note_revision_4: >
  SHA revisi 3 sudah stale jauh ketika revisi 4 disusun. Impact scan read-only atas area terdampak
  (PharmacyManagement prescription services, BillingManagement invoice/settlement/tender,
  MstPaymentMethod) dikerjakan langsung ke source pada 21 September 2026 dan hasilnya tercatat
  pada `02-requirement-completeness-assessment-financial-clearance.md`.
skill_suite_version: 1.0.0-rc2
input_revision_hash: pharmacy-decisions-r4+clearance-gate-2026-09-21
decision_revision: 4
decision_revision_note: >
  Revisi keputusan naik dari 2 ke 4 sekaligus. Yang terlewat dicatat manifest: keputusan bisnis
  3 September 2026 (PHA-DEC-042 sampai 053) dan audit 7 September 2026 (PHA-DEC-054 sampai 062)
  adalah revisi 3; closure amendment 21 September 2026 (PHA-DEC-063 sampai 070) adalah revisi 4.
owners:
  product_domain: user
  api: Pharmacy Backend owner
  security: pending formal approval
  frontend_authority: pending formal approval
approved_by: product/domain owner (user)
approved_at: 2026-08-21
backend_commit_sha: 767470f742bc6f2eebadbd653a873f69d6f93121
frontend_commit_sha: 400104f2a0f3239c14c40f5905b419977a538450
contract_versions:
  - PHA-DEPOT-ROUTING-v1: approved
  - PHA-API-ROUTING-v1: approved
  - PHA-STATE-ROUTING-v1: approved
  - PHA-VAL-ROUTING-v1: approved
  - PHA-INT-ROUTING-v1: approved
  - PHA-PERM-ROUTING-v1: approved
  - PHA-API-CLEARANCE-v1: approved
  - PHA-STATE-CLEARANCE-v1: approved
  - PHA-VAL-CLEARANCE-v1: approved
  - PHA-INT-CLEARANCE-v1: approved
  - PHA-PERM-CLEARANCE-v1: approved
  - PHA-TEST-CLEARANCE-v1: approved
contract_versions_approval_revision_4: >
  Keenam sumbu CLEARANCE naik dari `draft` menjadi `approved` pada 21 September 2026 lewat
  PHA-DEC-071. Approval ini BUKAN wewenang menulis source maupun migration. Slice ini juga tetap
  MENUNGGU sisi penerbit Billing berdiri — itu ketergantungan teknis yang tidak dapat dicabut
  approval siapa pun.
contract_versions_note_revision_4: >
  Enam sumbu kontrak BARU untuk slice Financial Clearance, seluruhnya `draft`. Keenam sumbu
  ROUTING yang sudah ada TIDAK disentuh dan tetap `approved` — berkasnya memang tidak diubah,
  hanya ditambahi bagian baru yang terpisah di ujungnya dengan judul dan versi sendiri.
  Penamaannya sengaja memakai akhiran slice, bukan angka berurut, karena kedua slice bergerak
  sendiri-sendiri: kontrak clearance dapat naik versi tanpa menyentuh kontrak routing.
active_dependency_ids:
  - PHA-DEP-001
  - PHA-DEP-002
  - PHA-DEP-003
active_roadmap_revision: PHA-RM-001-r1-draft
domain_architecture_revision: PHA-DA-001-r1
domain_architecture_readiness: DOMAIN_ARCHITECTURE_READY_FOR_ROUTING_DEPOT
domain_architecture_readiness_financial_clearance: DOMAIN_ARCHITECTURE_NOT_RUN
domain_architecture_note_revision_4: >
  Arsitektur domain PHA-DA-001-r1 berlaku HANYA untuk slice Routing Depo dan TIDAK dipinjam
  slice Financial Clearance. Slice itu dirancang tanpa hospital-domain-architect — sah karena
  skill itu opsional, dan batas bounded contextnya sudah ditetapkan keputusan pemilik:
  Billing memiliki kebenaran finansial, Farmasi memiliki kebenaran klinis dan operasional
  (PHA-DEC-063, RJ-BIL-GATE-DEC-007). Tidak ada batas domain yang perlu diturunkan sendiri
  oleh desain, sehingga tidak ada yang dikarang.
input_revisions:
  decisions: 4
  requirement_gate: PHA-RCG-001 (Routing Depo), PHA-RCG-002 (Financial Clearance)
  domain_architecture: PHA-DA-001-r1 (Routing Depo saja)
input_hashes:
  decisions_and_capability: pharmacy-decisions-r4+clearance-gate-2026-09-21
artifact_hashes:
  00-interview-decisions.md: 20118d6610385cddd7678364857010286227c030f9d442141bf4d21cadbe1caf
  01-existing-capability-map.md: b7d61a63d15fd8c5a670337df303a064730534548f206ca48bf4bab8b1a41f85
  02-backend-architecture.md: dfd6d636491cb4517e0a269119faca016462ec3bf12e3a411fb5c1822ee87b16
  02-requirement-completeness-assessment-financial-clearance.md: 037cdfa8de913f151f1eaaaf4598761c60ea3e87ac8b43950d87b294feebf5ea
  03-frontend-architecture.md: dee5686ef2ac8fab5b628c55392bcc4bb04c3bc4ce715cdde205938f05e07894
  04-prd-to-mvp.md: 1a942835b39b81d367220e0d89bc9749b210385b364a14d9222ede075009c333
  data/data-dictionary.md: 2ec3985fcce46a78a987126a2892c6272a67a4092e47de9401b00c30e519d5d6
  flowcharts/00-alur-utama.md: cb668d77174da5e5890435bd0b73726effeaca9aa35a0f6bf542564575604ff5
  flowcharts/pelepasan-dan-penahanan-resep.md: 3316a32e36dcf66175bcc5a64e051cd8f498def23d856a592728913d5da74786
  erd/00-context-erd.md: a3951a28ac86c2ea245e4e086306c252503cead35cf60b1a5c050587d7bb6276
  erd/pharmacy-routing.md: 2799d099e08e59e3f38c7c7d81a6e53c2973a8e00d4718aa8138cdc0f267a66c
  erd/data-dictionary.md: 4961346845ba5d33d707c2eca1a7fde8d5434e6569378ae6e391e973ec773203
  contracts/api-contract.md: 70a1ad22f239c36bf89ba71a8ddd8b38820d0f43c448acf7706b12d361600a1f
  contracts/state-transition-matrix.md: 7f4a88dc541bbc48f67a1b392c83d4b9ecd66fc6633c5765fc304f990bbb7b83
  contracts/validation-matrix.md: 2088f5ee7c0fdc471655c40243d7d7f6cf87d4cf1ea86a32ab5ad658ab186172
  contracts/integration-contract.md: ca953f38003afb1e45ae1a189a4822cbe2ea3986a8560b033658a326b78a72e3
  contracts/permission-audit-matrix.md: 8c0356cbbda0e554aecc724b837e0f4effbeac901e598ca0c9ed0916d2552b01
  testing/acceptance-test-matrix.md: 0308612538b9f4482d6781bcb7d25fd389376b22d72177e75f71fb86ad1d6ed5
  roadmap/README.md: 11ccb89808393c6a2c543f69ff84570b5a5b387df76757bf96bda13150f14ef6
artifact_hashes_note_revision_4: >
  Empat berkas BARU lahir pada revisi 4: 04-prd-to-mvp.md, data/data-dictionary.md, dan dua
  flowchart. Ketiga jenis itu belum pernah ada di blueprint ini karena struktur keluaran lamanya
  memakai erd/ tanpa data/ maupun flowcharts/. erd/ TIDAK disentuh dan hashnya tidak berubah —
  preseden yang sama dipakai billing-kasir. Pemindahan isi erd/data-dictionary.md ke data/ adalah
  perapian tersendiri, bukan bagian slice ini.
supersedes: null
```
