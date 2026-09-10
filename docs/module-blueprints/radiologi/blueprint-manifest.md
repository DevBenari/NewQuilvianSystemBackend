# Radiologi — Blueprint Manifest

```yaml
blueprint_id: RAD-BP-001
module_name: Radiologi
module_slug: radiologi
module_prefix: RAD
revision: 6
status: draft
bentuk: SINGLE
created_at: 2026-09-09T00:00:00+07:00
updated_at: 2026-09-09T00:00:00+07:00

tahap_saat_ini: DESIGN_SELESAI_MENUNGGU_APPROVAL
tahap_selesai:
  - scope_pass                      # /qv-grill, 2026-09-09
  - capability_audit                # /qv-trace, 2026-09-09
  - closure_pass                    # /qv-grill, 2026-09-09, dua putaran
  - requirement_completeness_gate    # RAD-RCG-001-r2, 2026-09-09, PARTIALLY_READY
  - hospital_domain_architecture     # RAD-DA-001-r2, 2026-09-09, DOMAIN_ARCHITECTURE_PARTIAL
  - design_business_module           # RAD-PRD-001, 2026-09-09, 11 berkas desain
tahap_belum_dijalankan:
  - plan_module_delivery            # TERTAHAN — dua pertanyaan pemblokir, lihat 04-prd-to-mvp.md bagian 20

kesiapan_requirement: PARTIALLY_READY
kesiapan_arsitektur: DOMAIN_ARCHITECTURE_PARTIAL
slices_ready_for_domain_design: [S1, S2, S3, S4, S6, S7, S8, S9, S10, S12, S13, S14]
slices_business_decision_required: [S5, S11]
slices_partially_ready: [S15]

scope:
  release: Rilis 1 — sampai hasil bacaan radiolog dirilis (RAD-DEC-001)
  modality_in_scope: [X-Ray, CT-Scan, MRI, USG, Mamografi, Fluoroskopi]
  modality_out_of_scope: [Kedokteran Nuklir, Radioterapi]
  out_of_scope_owner_lain:
    - Tarif dan tagihan -> billing-kasir
    - Stok kontras, film, BHP -> pharmacy/inventory
    - PACS, DICOM, RIS eksternal -> tidak diaktifkan (RJ-BIL-GATE-DEC-004)
    - Dosis radiasi petugas -> human-resource/K3

owners:
  product_domain: Yoga Aji Pratama <yogaaji452@gmail.com>
  api: belum ditetapkan
  security: belum ditetapkan
  frontend_authority: belum ditetapkan
  clinical_governance: belum ditetapkan — memblokir RAD-OPEN-001, RAD-OPEN-007, RAD-CQ-003, dan pemakaian production RAD-DEC-008
  module_owner_registry: Sukma Giri — ditunjuk RJ-BIL-DEC-014 pada 2026-08-28, tetapi belum tercermin di registry

approved_by: belum ada approval tingkat blueprint
approved_at: null

backend_commit_sha: "64da911"
frontend_commit_sha: "f66ed1885"

input_revisions:
  decisions: 8
  capability_map: 1
  requirement_gate: RAD-RCG-001-r2
  domain_architecture: RAD-DA-001-r2

input_hashes:
  00-interview-decisions.md: 4967149167b97842
  01-existing-capability-map.md: 3346c846367a021e81eb7be53810dc460d90004b131e925dbf01a1df470a6d75
  02-requirement-completeness-assessment.md: 500ff57712890ba9
  03-domain-architecture.md: 225e47b7bd75f73c
input_hashes_method: |
  sha256 atas isi berkas dengan line ending LF — sama dengan isi blob yang disimpan Git.
  Perintah: tr -d '\r' < <berkas> | sha256sum
  Konvensi diambil dari laboratorium/blueprint-manifest.md.

contract_versions:
  billing_integration: "BIL-INTEGRATION-0.4"   # as-is, milik BillingManagement
  backend_architecture: "RAD-ARCH-BE-001"
  frontend_architecture: "RAD-ARCH-FE-001"
  api_contract: "RAD-API-001"
  state_contract: "RAD-STATE-001"
  validation_contract: "RAD-VAL-001"
  integration_contract: "RAD-INT-001"
  permission_contract: "RAD-PERM-001"
  test_contract: "RAD-TEST-001"
  prd_to_mvp: "RAD-PRD-001"

artifact_hashes:                # sha256 16 karakter pertama, metode sama dengan input_hashes
  02-backend-architecture.md: 41d9c650beae5b5e
  03-frontend-architecture.md: ba7789c815bbecde
  04-prd-to-mvp.md: 056abc622669d683
  erd/00-context-erd.md: a037b466a1abce0d
  erd/radiology-ordering-acquisition.md: 810087749caa496b
  erd/radiology-reporting.md: c9bb4e259c4f98bc
  erd/radiology-safety-policy.md: addd51154a17d1d2
  erd/data-dictionary.md: 6f07a04ab81b1bb5
  contracts/api-contract.md: 54568960193029e7
  contracts/state-transition-matrix.md: aa767d00cfbac5da
  contracts/validation-matrix.md: 238cd2f7d019875b
  contracts/integration-contract.md: 401a569f4a6ccabf
  contracts/permission-audit-matrix.md: 0184606dbedc3008
  testing/acceptance-test-matrix.md: 9f6b15d9671ab725
```

---

## Kedudukan manifest ini

Manifest dibuat lebih dulu supaya keadaan modul terbaca oleh sesi berikutnya tanpa harus
mengulang penelusuran. Isinya mencatat apa yang sudah selesai, apa yang belum, dan apa yang
sedang menahan.

Desain sudah selesai. Manifest ini kini menjadi ringkasan keadaan seluruh blueprint.

---

## Gerbang Input Design — LOLOS untuk 12 Slice

Skill `design-business-module` memiliki gerbang wajib untuk kemampuan bisnis rumah sakit.
Kedua masukan wajibnya kini tersedia.

| Masukan wajib | Berkas | Keadaan |
|---|---|---|
| Bukti kelengkapan requirement | `02-requirement-completeness-assessment.md` | **Ada** — `RAD-RCG-001-r2`, verdict `PARTIALLY_READY` |
| Handoff arsitektur domain rumah sakit | `03-domain-architecture.md` | **Ada** — `RAD-DA-001-r2`, verdict `DOMAIN_ARCHITECTURE_PARTIAL` |

Perbandingan dengan modul sebanding:

| Modul | Requirement gate | Domain architecture |
|---|---|---|
| `laboratorium` | `LAB-RCG-001-r4` | `LAB-DA-001-r4` |
| `radiologi` | `RAD-RCG-001-r2` | `RAD-DA-001-r2` |

**Yang boleh masuk desain:** 12 slice `READY_FOR_DOMAIN_DESIGN`, dinyatakan eksplisit berdiri
sendiri dari verdict `PARTIAL`.

**Yang tidak boleh:** `S5` pelewatan darurat dan `S11` temuan kritis. Keduanya menunggu penanggung jawab tata kelola klinis yang belum ditunjuk.

### Catatan urutan pengerjaan dari arsitektur domain

Walaupun `S9` hasil bacaan adalah inti Rilis 1, `S4` pengelolaan aturan keselamatan **harus
dikerjakan lebih dulu atau bersamaan**. Gerbang keselamatan bersifat fail-closed: tanpa aturan
aktif, tidak satu pun pemeriksaan berjalan, sehingga tidak akan pernah ada citra layak untuk
dibaca.

### Pesan yang dibawa ke tahap desain

Saat merancang `RadReport`, sediakan ruang bagi penanda temuan kritis **tanpa menetapkan
bentuknya** (`GAP-RAD-01`). `S11` menyusul setelah `DEC-RAD-002` ditutup, dan penambahannya
tidak boleh membongkar identitas, lifecycle, maupun aturan pengesahan hasil bacaan.

---

## Permintaan Approval yang Sedang Berjalan

Dokumen operasional, **bukan** artefak desain, sehingga tidak masuk daftar hash di atas.

| `request_id` | Berkas | Ditujukan kepada | Menahan | Status |
|---|---|---|---|---|
| `RAD-REQ-001` | `approval-requests/2026-09-09-permintaan-pembukaan-penghambat.md` | Pemegang registry; pemilik modul + Administrator; pemilik modul IGD | `/plan-module-delivery` | `terbuka` |
| `RAD-REQ-002` | `approval-requests/2026-09-09-permintaan-tanda-tangan-klinis.md` | Manajemen rumah sakit, diteruskan ke dokter penanggung jawab radiologi atau Komite Medis | Slice `S5` dan `S11`; pemakaian production `RAD-DEC-008` | `terbuka` |

---

## Blocker Aktif

| ID | Blocker | Pemilik | Menahan |
|---|---|---|---|
| ~~`RAD-GATE-001`~~ | ~~`02-requirement-completeness-assessment.md` belum ada~~ **DITUTUP 2026-09-09** oleh `RAD-RCG-001-r2` | — | — |
| ~~`RAD-GATE-002`~~ | ~~`03-domain-architecture.md` belum ada~~ **DITUTUP 2026-09-09** oleh `RAD-DA-001-r2` | — | — |
| `DEC-RAD-001` | Pengesahan klinis atas pelewatan gerbang keselamatan | Clinical Governance | Slice `S5` |
| `DEC-RAD-002` | Daftar temuan kritis belum ada | Clinical Governance | Slice `S11` |
| ~~`DEC-RAD-003`~~ | ~~Kebutuhan daftar kerja petugas~~ **DITUTUP 2026-09-09** oleh `RAD-DEC-012` dan `RAD-DEC-013` | — | — |
| `RAD-OPEN-001` | Penanggung jawab tata kelola klinis belum ditunjuk | Manajemen rumah sakit | `RAD-GATE-002`, dan empat open question lain |
| `RAD-OPEN-005` | Registry masih `PLANNED`; `RAD-DEC-007` sudah menetapkan caranya tetapi belum dijalankan | Pemegang registry | Pembuatan seluruh entity `Rad*` baru |
| `RAD-CONFLICT-002` | Modul IGD masih menyatakan modul Radiologi belum ada; `RAD-DEC-009` sudah menetapkan jalan keluarnya tetapi belum dijalankan | Pemilik modul IGD | Titik sentuh IGD |

---

## Yang Sudah Selesai dan Masih Sahih

| Artefak | Revision | Isi | Sahih terhadap |
|---|---:|---|---|
| `00-interview-decisions.md` | 8 | 13 keputusan terkunci, 42 acceptance criteria dapat diuji, 2 conflict beserta jalan keluarnya | BE `64da911`, FE `f66ed1885` |
| `01-existing-capability-map.md` | 1 | 34 kemampuan terklasifikasi, 2 conflict, 7 closure question | BE `64da911`, FE `f66ed1885` |

Keduanya menjadi masukan sah bagi `requirement-completeness-gate` tanpa perlu diulang.

---

## Daftar Berkas Canonical dan Keadaannya

Kontrak keluaran blueprint mewajibkan 13 berkas. Keadaannya sekarang:

| Berkas | Keadaan | Pemilik tahap |
|---|---|---|
| `blueprint-manifest.md` | **Ada** — dokumen ini | manage-module-blueprint |
| `00-interview-decisions.md` | **Ada**, revision 7 | grill-me |
| `01-existing-capability-map.md` | **Ada**, revision 1 | trace-existing-capabilities |
| `02-requirement-completeness-assessment.md` | **Ada**, `RAD-RCG-001-r1` | requirement-completeness-gate |
| `03-domain-architecture.md` | **Ada**, `RAD-DA-001-r1` | hospital-domain-architect |
| `02-backend-architecture.md` | **Ada**, `RAD-ARCH-BE-001` | design-business-module |
| `03-frontend-architecture.md` | **Ada**, `RAD-ARCH-FE-001` | design-business-module |
| `04-prd-to-mvp.md` | **Ada**, `RAD-PRD-001` | design-business-module |
| `erd/00-context-erd.md` | **Ada**, `RAD-ERD-CTX-001` | design-business-module |
| `erd/radiology-ordering-acquisition.md` | **Ada**, `RAD-ERD-ORD-001` | design-business-module |
| `erd/radiology-reporting.md` | **Ada**, `RAD-ERD-REP-001` | design-business-module |
| `erd/radiology-safety-policy.md` | **Ada**, `RAD-ERD-SAF-001` | design-business-module |
| `erd/data-dictionary.md` | **Ada**, `RAD-ERD-DICT-001` | design-business-module |
| `contracts/api-contract.md` | **Ada**, `RAD-API-001` | design-business-module |
| `contracts/state-transition-matrix.md` | **Ada**, `RAD-STATE-001` | design-business-module |
| `contracts/validation-matrix.md` | **Ada**, `RAD-VAL-001` | design-business-module |
| `contracts/integration-contract.md` | **Ada**, `RAD-INT-001` | design-business-module |
| `contracts/permission-audit-matrix.md` | **Ada**, `RAD-PERM-001` | design-business-module |
| `testing/acceptance-test-matrix.md` | **Ada**, `RAD-TEST-001` | design-business-module |

Seluruh berkas canonical sudah lengkap. Tiga ERD per bounded context dibuat karena modul ini
punya empat konteks; menggabungkannya menjadi satu diagram akan melanggar aturan "satu diagram
muat dibaca dalam satu layar".

---

## Urutan Kerja Berikutnya

| Urutan | Skill | Menghasilkan | Prasyarat |
|---:|---|---|---|
| ~~1~~ | ~~`/requirement-completeness-gate`~~ | **Selesai** — `RAD-RCG-001-r1` | — |
| ~~2~~ | ~~`/hospital-domain-architect`~~ | **Selesai** — `RAD-DA-001-r1` | — |
| ~~3~~ | ~~`/design-business-module`~~ | **Selesai** — 11 berkas desain, `RAD-PRD-001` | — |
| 4 | `/plan-module-delivery` | Roadmap task backend dan frontend | **TERTAHAN** — dua pertanyaan pemblokir |

Langkah 3 dikerjakan hanya untuk 11 slice yang siap. Tiga slice `S5`, `S11`, dan `S12`
**tidak** ikut, dan penutupannya lewat `/qv-grill` Amendment pass.

### Dua pertanyaan pemblokir menuju langkah 4

| Pertanyaan | Pemilik | Menahan |
|---|---|---|
| Kapan registry `Rad` dinaikkan `ACTIVE`? Caranya sudah ditetapkan `RAD-DEC-007` | Pemegang registry | Gelombang `MVP-0` |
| Peran Quilvian mana yang setara dokter radiolog dan penanggung jawab klinis? (`DEC-RAD-004`) | Pemilik modul + Administrator | `EPIC RAD-01` dan `EPIC RAD-02` |

Keduanya **bukan** pertanyaan bisnis yang sulit — keputusannya sudah diambil. Yang belum
terjadi adalah eksekusinya, dan keduanya di luar kendali modul Radiologi.

Di luar urutan itu, dua tindakan dapat berjalan paralel karena tidak bergantung pada tahap mana
pun: pemegang registry menjalankan `RAD-DEC-007`, dan pemilik modul IGD menjalankan
`RAD-DEC-009`.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | Manifest dibuat. Mencatat selesainya `Scope pass`, audit capability, dan `Closure pass`, serta gagalnya gerbang input desain. | `draft` |
| 2 | 2026-09-09 | `requirement-completeness-gate` selesai — `RAD-RCG-001-r1`, verdict `PARTIALLY_READY`. `RAD-GATE-001` ditutup. Enam Decision ID `DEC-RAD-001` sampai `DEC-RAD-006` diterbitkan. | `draft` |
| 3 | 2026-09-09 | `hospital-domain-architect` selesai — `RAD-DA-001-r1`, verdict `DOMAIN_ARCHITECTURE_PARTIAL`. `RAD-GATE-002` ditutup. Empat bounded context, empat aggregate, 11 konsep domain, lima gap arsitektur. Gerbang desain lolos untuk 11 slice. | `draft` |
| 6 | 2026-09-09 | Dua permintaan approval diterbitkan: `RAD-REQ-001` untuk penghambat pengembangan (registry, pemetaan peran, koordinasi IGD) dan `RAD-REQ-002` untuk penunjukan dan tanda tangan tata kelola klinis. Keduanya dokumen operasional, tidak masuk hash. | `draft` |
| 5 | 2026-09-09 | `Amendment pass` — `DEC-RAD-003` ditutup oleh `RAD-DEC-012` dan `RAD-DEC-013`. `S12` daftar kerja naik menjadi siap dan masuk desain sebagai `EPIC RAD-07`. `RadOrder` menjadi `Diperbarui` dengan tiga kolom penanda cito; migration 3 ditambahkan. **Daftar kerja tidak membutuhkan tabel baru**, sehingga tidak tertahan blocker registry. Sebelas berkas diperbarui. | `draft` |
| 4 | 2026-09-09 | `design-business-module` selesai — 11 berkas desain untuk 11 slice siap. Dua model baru, satu diperbarui, empat enum, dua service, empat controller, dua migration direncanakan. Enam epic, 24 functional requirement, 12 skenario UAT. Seluruh berkas canonical lengkap. `/plan-module-delivery` tertahan dua pertanyaan pemblokir. | `draft` |
