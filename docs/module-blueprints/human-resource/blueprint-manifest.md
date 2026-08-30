# Human Resource — Blueprint Manifest

```yaml
blueprint_id: HRD-BP-001
module_name: Human Resource
module_slug: human-resource
module_prefix: HRD
revision: 7
status: DRAFT
design_readiness: PARTIAL
current_phase: HRD-PH-DECISION-CLOSURE
created_at: 2026-08-27T07:02:01Z
updated_at: 2026-08-30T18:00:00Z
last_verified_at: 2026-08-30T18:00:00Z
backend_source_sha_audited: ecdc135444f0110482c9702212bcea30043983c8
backend_source_sha_verified: 16b8b71f4cd61e083213cf90722f4d768d339739
backend_source_sha_current: e0ee42c752a5f92c5b1663ff88bef07a5859f79f
frontend_source_sha: fff76a1b394d4b247c70a04f106c8ec098c9696e
frontend_source_sha_previous: 2a1cea7841a4433f8637d486204e60314c09d131
skill_suite_version: 1.0.0-rc2
input_revision_hash: 0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3
decision_revision: 12
backend_baseline_branch: QuilvianIntegrationBackend
capability_map_revision: 1.1
capability_map_hash: f66edd1514d28ce338130d9aaebfd40ee5678a0037667a3b07fdfbd1326cc510
contract_versions: v2
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
artifact_hashes:
  00-interview-decisions.md: 0f4bb66d96d5fcd10a388e7b98efa08510f9edf50e3033dddf84951ad09854a3
  01-existing-capability-map.md: f66edd1514d28ce338130d9aaebfd40ee5678a0037667a3b07fdfbd1326cc510
  02-backend-architecture.md: b7747cd96b38700af69610877af02e18555c024891460eb421b4967a3db1d21e
  03-frontend-architecture.md: 08305024dd3fa7d08957cc5d92cd2a476cb52d9ecff77602c5f1cf438de93d1e
  04-prd-to-mvp.md: e50b67f1bc915719282b0dc15a0a714cb1e96762658e9108484dde03f5d36af9
  data/data-dictionary.md: c11b41b75eb7ef24ad6e4f211bf6be489d2ff17ed96a432edadb41614bc76fd6
  contracts/api-contract.md: b5adc427b299408227ba82d354fbe1f58d3ec90df80132bb8a8edb0478f25168
  contracts/state-transition-matrix.md: 477291eabe3a3849717ca83acd9ea018c4ed6c6d8b9e2c2f725f74efb2c184fd
  contracts/validation-matrix.md: 44155151cd755ebf819ee63473cc6c4f5772c57d58a214d34914e195a5c0d9c5
  contracts/integration-contract.md: 1eebac415e3e20e0df291ae2485b95a4179665298d199060ef85beb3db8891ab
  contracts/permission-audit-matrix.md: 8d0a488989623372f83eb816285f24acf1c09a319701b72adff169791384b216
  testing/acceptance-test-matrix.md: e6326a80dc5887d14577b25a3591e722b909fc114d996dd00ba0a120e437b9e1
  flowcharts/00-alur-utama.md: ef9b4d06e170d56963b72c398c197e1dd91cb184ba8bfea636ac5f2e189be485
active_dependency_ids:
  - HRD-DEP-001
  - HRD-DEP-002
  - HRD-DEP-003
  - HRD-DEP-004
  - HRD-DEP-005
  - HRD-DEP-006
  - HRD-DEP-007
active_roadmap_revision: 3
supersedes: null
```

---

## 1. Cara membaca manifest ini

| Field | Arti dalam bahasa sehari-hari |
| --- | --- |
| `blueprint_id` | Nama tetap blueprint ini. Tidak pernah berubah |
| `revision` | Naik hanya bila arsitektur target, kontrak, dependency, atau keputusan yang disetujui berubah secara material. Tidak naik hanya karena status berubah |
| `status` | Keadaan blueprint sebagai dokumen. Masih `DRAFT` karena belum ada approval manusia atas keseluruhannya |
| `design_readiness` | Keadaan modul dilihat dari boleh tidaknya dirancang. `PARTIAL` karena sebagian siap dan sebagian terblokir |
| `input_revision_hash` | Sidik jari `00-interview-decisions.md` yang menjadi dasar revision ini |
| `capability_map_hash` | Sidik jari `01-existing-capability-map.md` yang menjadi dasar revision ini |
| `contract_versions` | `v1`. Berlaku bagi **satu set** kontrak sekaligus, bukan per berkas. Yang menjaga kelimanya tetap satu himpunan adalah `artifact_hashes`, bukan angka yang disamakan tangan |
| `artifact_hashes` | Sidik jari ketiga belas artefak canonical. Dipakai mendeteksi drift dokumen, bukan drift source |
| `backend_source_sha_current` | Commit tempat arsitektur target dan seluruh kontrak ditulis. Berbeda dari `backend_source_sha_audited`, yang tetap dipertahankan sebagai provenance audit lama |

`status: DRAFT` dan `design_readiness: PARTIAL` menjawab dua pertanyaan berbeda. Yang pertama:
apakah dokumen ini sudah disetujui. Yang kedua: apakah isinya boleh dipakai untuk merancang.
Sebuah blueprint bisa saja `DRAFT` tetapi sebagian besarnya sudah `READY FOR DESIGN`, dan itu
persis keadaan modul ini.

---

## 2. Klasifikasi kesiapan desain

Empat nilai yang dipakai konsisten di seluruh blueprint HR:

| Nilai | Artinya | Jumlah kelompok kemampuan |
| --- | --- | ---: |
| `READY FOR DESIGN` | Boleh dirancang sampai final | 15 |
| `PARTIAL` | Sebagian boleh final, sebagian tertahan. Batasnya wajib disebut eksplisit | 1 |
| `BLOCKED` | Tidak boleh dirancang sama sekali | 6 |
| `DEFERRED` | Boleh secara teknis, sengaja ditunda karena prioritas | 1 |

**Aturan yang mengikat manifest ini:** nilai `BLOCKED` tidak boleh diubah menjadi
`READY FOR DESIGN` tanpa dependency-nya benar-benar terpenuhi dan buktinya dicatat pada
`01-prerequisite-readiness.md`. Menaikkan status berdasarkan asumsi, kemiripan dengan modul
lain, atau desakan jadwal adalah pelanggaran terhadap manifest ini.

Rincian per kelompok kemampuan ada di [`MODULE-STATUS.md`](./MODULE-STATUS.md) bagian 2.

---

## 3. Kepemilikan

| Peran | Pemilik | Dasar |
| --- | --- | --- |
| Keputusan rekayasa dan produk teknis | Pengguna | `HRD-DEC-015`, `approved` 2026-08-27 |
| Kebijakan bisnis HR | `OPEN` | `HRD-Q-01` masih terbuka |
| Kewenangan klinis dan keselamatan pasien | Komite Medik — belum ada wakil yang ditunjuk | `HRD-Q-08` |
| Kesehatan dan keselamatan kerja | K3RS — belum ada wakil yang ditunjuk | `HRD-DEC-010` masih `draft` |
| Batas payroll dengan Finance | Pemilik produk bersama Finance | `HRD-Q-10`, `HRD-Q-11` |
| Kewenangan API | Backend, mengikuti `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` | `AGENTS.md` |
| Kewenangan frontend | Pengguna, dengan ruang `DEV_DISCRETION` sesuai `HRD-FE-03` | `HRD-DEC-007`, `HRD-DEC-015` |

`approved_by` dan `approved_at` untuk blueprint secara keseluruhan: **belum ada**. Yang sudah
ada adalah approval per keputusan, tercatat pada `00-interview-decisions.md`.

---

## 4. Dependency aktif

| ID | Ringkasan | Jenis | Status kemampuan |
| --- | --- | --- | --- |
| `HRD-DEP-001` | Registry kepemilikan dan prefix modul | `MODULE_FOUNDATION` | `REPAIR` — `Wfp` belum terdaftar |
| `HRD-DEP-002` | Mesin workflow dan persetujuan bersama | `MODULE_FOUNDATION` | `EXTEND` |
| `HRD-DEP-003` | Identitas dan hak akses aplikasi | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-004` | Finance untuk penyelesaian payroll | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-005` | Health Services untuk pengecekan kewenangan klinis | `INTEGRATION` | `UNKNOWN` |
| `HRD-DEP-006` | Penyimpanan berkas dan dokumen | `EXTERNAL` | `UNKNOWN` |
| `HRD-DEP-007` | Arsitektur domain rumah sakit untuk slice klinis | `PHASE` | `MISSING` |

Rincian lengkap beserta bukti dan dampak pemblokirannya ada di
[`01-prerequisite-readiness.md`](./01-prerequisite-readiness.md).

---

## 5. Keputusan yang mengikat seluruh artefak

Sembilan keputusan berikut berlaku di setiap dokumen blueprint HR dan tidak boleh ditafsir ulang
per artefak.

| Decision ID | Isi yang mengikat | Status |
| --- | --- | --- |
| `HRD-DEC-003` | Satu blueprint utuh untuk 21 capability; batas rilis ditulis di `04-prd-to-mvp.md` | `approved` |
| `HRD-DEC-004` | Otoritas skema hybrid: domain berjalan dikunci, domain tanpa controller diturunkan ulang | `approved` |
| `HRD-DEC-005` | Kredensial kedaluwarsa memberi peringatan tercatat, tidak menghentikan pelayanan | `draft` — menunggu Komite Medik |
| `HRD-DEC-009` | Tanggung jawab HR atas payroll berhenti setelah `execute` | `approved` |
| `HRD-DEC-010` | Rekam kesehatan kerja hanya untuk K3RS dan pegawai bersangkutan | `draft` — menunggu K3RS |
| `HRD-DEC-016` | Kebab-case sebagai route canonical, route lama tetap hidup sebagai compatibility alias. **Bukan** hard breaking rename | `approved` |
| `HRD-DEC-017` | ~~`Hrd` sebagai target naming; migrasi legacy bertahap per domain sebagai campaign~~ | `superseded` oleh `HRD-DEC-019` |
| `HRD-DEC-018` | Satu UX kotak masuk; workflow, policy, permission, validasi, SLA, dan eskalasi tetap per jenis transaksi | `approved` |
| `HRD-DEC-031` | **Persetujuan wajib dan pemisahan peran** untuk penetapan gaji, penempatan organisasi, penempatan jabatan, dan penetapan atasan. Penyetuju **MUST** berbeda dari pembuat; unit dengan satu petugas dieskalasi, **tidak pernah** swa-setuju | `approved` |
| `HRD-DEC-036` | **Empat definisi alur terpisah** untuk penetapan gaji, penempatan organisasi, penempatan jabatan, dan penetapan atasan. Pola persetujuan awal boleh sama; **satu definisi bersama MUST NOT dibuat** | `approved` |
| `HRD-DEC-032` | **Baseline peran fungsional.** Usulan peta peran menjadi baseline; nama peran fungsional **MUST NOT** disamakan dengan peran Identity. Hanya `SuperAdmin` dan `User` yang di-seed hari ini; seluruh peran HR berstatus `MAPPING_REQUIRED` | `OWNER_DECIDED_PENDING_SECURITY_COSIGN` |
| `HRD-DEC-033` | **Nominal gaji tersembunyi secara bawaan** pada daftar lintas pegawai. Butir `: ViewAmount` terpisah dari `: Read`. Keterlihatan massal tidak diberikan pada MVP | `OWNER_DECIDED_PENDING_SECURITY_COSIGN` |
| `HRD-DEC-034` | **Alur persetujuan bawaan per jenis transaksi**, bukan satu alur untuk semua. Override per unit lewat konfigurasi, tanpa pengembangan ulang | `approved` **seluruhnya** — prinsip dan isi rantai `T1` s.d. `T8` |
| `HRD-DEC-035` | **Batas payroll MVP:** HR menghasilkan masukan yang siap payroll, **bukan** membuat, membayar, atau memfinalkan putaran payroll. Orkestrasi putaran payroll `POST-MVP` | `approved` |
| `HRD-DEC-019` | **Kebijakan penamaan canonical HR.** `Mst` tetap master/reference. `Wfp` prefix yang sah untuk keluarga workforce, **bukan** legacy. `Hrd` canonical dan default untuk entity operasional HR baru. `Trx` legacy generik dengan **ratchet hanya saat materially touched** | `approved` |

`HRD-DEC-016` menggantikan `HRD-DEC-014`. `HRD-DEC-017` menggantikan `HRD-DEC-008`, lalu
`HRD-DEC-017` sendiri digantikan `HRD-DEC-019`. Seluruhnya tercatat `superseded` pada decision
log dan tidak dihapus.

### 5.1 Matriks prefix yang berlaku

| Prefix | Arti | Entity baru boleh? | Kebijakan untuk yang sudah ada |
| --- | --- | :---: | --- |
| `Mst` | Data master atau referensi | Ya, bila memang master/reference | Tidak diubah |
| `Wfp` | Keluarga workforce dan profil HR | Ya, bila memang bagian keluarga itu | Tidak diubah. **Bukan** legacy yang akan dihapus |
| `Hrd` | Entity operasional HR — canonical dan default | Ya, ini pilihan bawaan | Tetap `Hrd` |
| `Trx` | Prefix generik warisan | **Tidak** | Dibiarkan berjalan; menjadi `Hrd*` hanya saat materially touched |

Definisi *material touch* selengkapnya ada di `00-interview-decisions.md` bagian 16.5. Ringkasnya:
yang memicu ratchet adalah perubahan pada entity, konfigurasi EF, tabel atau kolom, relasi,
index, lifecycle persistence, atau migration yang mengenai entity itu. Pekerjaan frontend,
pembacaan data, dokumentasi, perubahan tampilan, dan perbaikan bug yang tidak mengubah kontrak
persistence **tidak** memicu ratchet.

---

## 6. Larangan yang berlaku sepanjang blueprint

1. **Jangan mengarang bounded context klinis.** Kredensial, kewenangan klinis, SPK/RKK, OPPE,
   FPPE, dan kesehatan kerja menunggu `requirement-completeness-gate` dan
   `hospital-domain-architect`.
2. **Jangan memfinalkan bentuk serah terima payroll** sebelum `HRD-Q-10` dan `HRD-Q-11` dijawab
   bersama Finance.
3. **Jangan mengambil keputusan skema yang merusak data** sebelum `HRD-Q-05` dijawab dengan
   audit database yang sebenarnya.
4. **Jangan melakukan rename massal, dan jangan membuat kampanye migration yang mengejar seluruh
   `Trx*` sekaligus.** `HRD-DEC-019` hanya mengizinkan ratchet saat entity itu benar-benar
   disentuh, mengikuti cakupan task yang sedang dikerjakan.
5. **Jangan mengubah `Wfp*` menjadi `Hrd*`** hanya karena namanya berbeda. `Wfp` adalah prefix
   yang sah dan tetap dipakai.
6. **Jangan mengubah `Mst*` menjadi `Hrd*`** hanya karena entity-nya berada di domain HR.
7. **Jangan menetapkan tenggat pembersihan seluruh `Trx*`.** Tidak ada target pembersihan
   menyeluruh.
8. **Jangan membuat entity transaksional HR baru dengan prefix `Trx`.**
9. **Jangan mematikan route lama.** `HRD-DEC-016` menuntut alias tetap hidup sampai audit
   consumer selesai dan masa deprecation berakhir.
10. **Jangan menyimpulkan kepemilikan entity dari prefix.** Empat puluh entity `Trx*` adalah
    milik Health Services, bukan HR.
11. **Jangan memutus proses bisnis yang sedang berjalan** hanya demi konsistensi penamaan.
12. **Blueprint tidak memberi wewenang implementasi.** Tidak ada source aplikasi, migration,
    controller, entity, frontend, maupun database yang boleh disentuh dari alur blueprint ini.

---

## 7. Struktur artefak canonical

Struktur di bawah mengikuti `design-business-module/references/blueprint-output-contract.md`
versi canonical pada plugin `quilvian-engineering-skills`. Ketiga belas berkas berikut **MUST**
ada, dan daftarnya pasti.

```text
docs/module-blueprints/human-resource/
├── blueprint-manifest.md
├── 00-interview-decisions.md
├── 01-existing-capability-map.md
├── 02-backend-architecture.md
├── 03-frontend-architecture.md
├── 04-prd-to-mvp.md
├── flowcharts/
│   ├── 00-alur-utama.md
│   └── <proses>.md
├── data/
│   └── data-dictionary.md
├── contracts/
│   ├── api-contract.md
│   ├── state-transition-matrix.md
│   ├── validation-matrix.md
│   ├── integration-contract.md
│   └── permission-audit-matrix.md
└── testing/
    └── acceptance-test-matrix.md
```

**Folder `erd/` tidak dipakai, tidak ada, dan tidak boleh dibuat.** Kontrak keluaran terbaru
menghapusnya sebagai artefak. Penggantinya:

| Kebutuhan yang dulu dijawab `erd/` | Tempatnya sekarang |
| --- | --- |
| Relasi antar entity per bounded context | `02-backend-architecture.md` — Mermaid `classDiagram`, tujuh diagram per konteks |
| Struktur tabel, kolom, nullability, index, unique, perilaku hapus, kolom sensitif | `data/data-dictionary.md` |
| Alur kerja pengguna dan percabangan proses bisnis | `flowcharts/**` |
| Lifecycle dan perpindahan status | `contracts/state-transition-matrix.md` |

### 7.1 Berkas di folder ini yang BUKAN bagian ketiga belas artefak

Kehadirannya bukan penyimpangan struktur, dan ketiadaannya bukan blueprint yang belum lengkap.

| Berkas atau folder | Pemilik | Kedudukan |
| --- | --- | --- |
| `MODULE-STATUS.md`, `01-prerequisite-readiness.md`, `README.md` | `manage-module-blueprint` | Artefak siklus hidup |
| `00-business-overview.md`, `02-existing-capability-map.md` | Pass sebelumnya | `02-existing-capability-map.md` hanya penunjuk, bukan salinan |
| `evidence/**` | `grill-me` | Masukan produk historis, tidak mengikat apa pun |
| `flows/**` | Pass `PHASE 2A` s.d. `PHASE 2C` | **Bukti dan penalaran** di balik aturan bisnis beserta penanda provenance per aturan. Berbeda isi dan tujuan dari `flowcharts/**`; keduanya bukan salinan satu sama lain |
| `roadmap/**` | `plan-module-delivery` | Menyusul saat delivery direncanakan |
| `task/report/**` | Kedua build skill | Menyusul saat task dikerjakan |

---

## 8. Riwayat revision

| Revision | Tanggal | Isi | Dasar |
| --- | --- | --- | --- |
| `1` | 2026-08-27 | Manifest pertama. Menyerap decision log revision `2` dan capability map revision `1.1`. Scaffold fondasi dan roadmap slice | `HRD-DEC-003` s.d. `HRD-DEC-018` |
| `2` | 2026-08-27 | Amendment Pass 1.1. Menyerap `HRD-DEC-019` kebijakan penamaan canonical yang menggantikan `HRD-DEC-017`. Memperbaiki hitungan slice dan definisi angka 68/67 | `HRD-DEC-019` |
| `3` | 2026-08-27 | Baseline Impact Gate. Baseline backend berpindah ke `origin/QuilvianIntegrationBackend` dengan hasil `NO_IMPACT`; capability map tetap `CURRENT`. Koreksi kelas QBE `S-C4` per entity, revision decision log, dan jumlah flow | `HRD-Q-16`, `HRD-Q-17` |
| `4` | 2026-08-27 | PHASE 2A. Lima flow inti administratif ditulis. `HRD-DEC-020` provenance masukan produk dan `HRD-DEC-021` baseline implementasi canonical. Enam belas pertanyaan baru `HRD-Q-18` s.d. `HRD-Q-33` |
| `7` | 2026-08-30 | **PHASE 3.1 — Workflow Chain Approval.** Isi rantai `T1` s.d. `T8` disetujui pemilik; `HRD-DEC-034` tidak lagi berstatus usulan. `HRD-DEC-036` menutup `HRD-Q-54` dengan **empat definisi alur terpisah**. Paket tinjauan keamanan disiapkan untuk `HRD-DEC-032` dan `HRD-DEC-033`. `contract_versions` **tetap** `v2` — semantik kontrak tidak berubah, hanya dipertegas | `HRD-DEC-036` |
| `6` | 2026-08-30 | **PHASE 3 — Owner Decision Closure.** `HRD-DEC-031` s.d. `HRD-DEC-035` menutup `HRD-Q-19`, `HRD-Q-33`, `HRD-Q-20`, isi konfigurasi workflow, dan `HRD-Q-49` untuk cakupan MVP. `HRD-Q-54` dicatat. `contract_versions` naik ke `v2` karena semantik kontrak berubah — pemisahan peran, butir hak akses sensitif nominal, dan batas payroll. Empat entity penempatan dan remunerasi naik menjadi `Diperbarui`. `FR-HRD-012` yang sebelumnya over-asserted diperbaiki | `HRD-DEC-031` s.d. `HRD-DEC-035` |
| `5` | 2026-08-30 | **PHASE 3 — Design Completion.** Empat artefak canonical yang belum ada ditulis: `data/data-dictionary.md`, `flowcharts/**`, `testing/acceptance-test-matrix.md`, dan `04-prd-to-mvp.md`. `contract_versions` diisi `v1`; `artifact_hashes` diisi. Snapshot SHA disegarkan ke BE `e0ee42c` dan FE `fff76a1b39` dengan hasil `NO_CAPABILITY_IMPACT`. Satu koreksi berbasis bukti pada bagian 7.1 `02-backend-architecture.md`. Cleanup governance referensi `erd/` mengikuti `blueprint-output-contract` terbaru |

Perubahan setelah approval membuat revision baru dan memicu impact scan pada kedua repository.
Blueprint ini tidak pernah ditandai `approved` oleh skill; approval tetap tindakan manusia.
