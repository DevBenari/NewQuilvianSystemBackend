# Sub-modul `dokter-rawat-inap` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `dokter-rawat-inap` |
| `judul` | Dokter Rawat Inap |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | `5` — satu angka, dipegang tingkat modul |
| `status` | **`draft`** — lahir 2026-09-02 bersama migrasi bentuk; **belum dirancang** |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###`, deret bersama seluruh modul |
| `approved_by` | — belum ada yang disetujui |
| `approved_at` | — |
| `rumpun kemampuan` | Dokumentasi dokter — kajian medis, SOAP, CPPT, tindakan, visite, resep, dan penunjang |
| `kemampuan` | **7** — `CAP-015`, `CAP-020` s.d. `CAP-025`, sesuai `RWI-DEC-083` |
| `uji pemecahan` | **3/5** syarat `bentuk-blueprint.md` bagian 4.1, sebagaimana dicatat `RWI-DEC-082` |
| `peran pemilik` | DPJP dan dokter jaga ruangan |

---

## 0. Kenapa `draft` dan bukan `BLOCKED`

`bentuk-blueprint.md` bagian 6 gerakan ③ menyatakan sub-modul yang **batas kepemilikan datanya belum
diputuskan** lahir berstatus `BLOCKED`. Sub-modul ini **tidak** dalam keadaan itu.

| Hal | Keadaannya | Sumbernya |
|---|---|---|
| Kepemilikan tabel dokumentasi klinis | **Sudah diputuskan** — milik `ClinicalManagement`. Rawat Inap tidak membuat tabel tandingan | `RWI-DEC-081` |
| Persetujuan pemilik `ClinicalManagement` dan `PharmacyManagement` | **Sudah diberikan** 2026-08-21; menutup `RWI-OQ-032` dan `DEC-INP-001` | `RWI-DEC-062` |
| Kemampuan yang menjadi jatah sub-modul ini | **Sudah dipetakan**, nol kemampuan yatim | `RWI-DEC-083` |
| Masuknya dokumentasi klinis ke dalam scope modul | **Sudah diputuskan** 2026-09-02 | `RWI-DEC-080` |
| Yang benar-benar tersisa | Pekerjaan desain yang belum dikerjakan, ditambah satu penghalang **teknis**: *shared inpatient clinical context resolver* | `PRD-RWI-FINAL-001` bagian 30.3 |

**`BLOCKED` berarti menunggu orang. `draft` berarti menunggu pekerjaan.** Sub-modul ini menunggu
pekerjaan.

---

## 1. Kemampuan yang dimiliki sub-modul ini

| Kemampuan | ID | Nama pada `PRD-RWI-FINAL-001` |
|---|---|---|
| Pemeriksaan penunjang — laboratorium dan radiologi | `CAP-015` | Supporting Services |
| Dokumentasi SOAP | `CAP-020` | Clinical Documentation — SOAP |
| CPPT | `CAP-021` | Clinical Documentation — CPPT |
| Kajian medis awal | `CAP-022` | Medical Assessment |
| Resep rawat inap dan obat pulang | `CAP-023` | Medication Management |
| Tindakan dokter | `CAP-024` | Physician Procedures |
| Pencatatan visite dokter | `CAP-025` | Physician Visit |

`CAP-021` CPPT memang ditulis lintas profesi bersama `keperawatan`. Itu **sifat** CPPT, bukan tabrakan kepemilikan: pemilik kontraknya tetap satu, yaitu sub-modul ini, sesuai `RWI-DEC-083`.

Pemetaan lengkap ke-28 kemampuan modul ada di
[`../02-module-map.md`](../02-module-map.md) bagian 4.

---

## 2. Kepemilikan data

**Sub-modul ini tidak memiliki satu tabel pun.** `RWI-DEC-081` menetapkan seluruh tabel dokumentasi
klinis rawat inap — pengkajian, CPPT, SOAP, kajian medis, resep, dan tindakan — dimiliki
`ClinicalManagement`. Rawat Inap hanya menyediakan **workspace, konteks episode, dan kontrak**.

| Yang dimiliki sub-modul ini | Yang dipakai dari modul lain |
|---|---|
| Nol tabel. Nol migration. Nol `DbSet` | `ClinicalManagement` untuk kajian medis, SOAP, CPPT, tindakan, dan visite; `PharmacyManagement` untuk resep; modul Laboratory dan Radiology untuk penunjang |

Baris kepemilikan datanya dibaca di [`../02-module-map.md`](../02-module-map.md) bagian 2.3, bukan
di `02-backend-architecture.md` sub-modul ini.

> **Larangan yang mengikat sejak hari pertama:** sub-modul ini **MUST NOT** membuat tabel tandingan
> untuk kemampuan di atas. Bila kelak desainnya terasa menuntut tabel baru, yang benar adalah
> kembali ke `/qv-grill`, bukan membuatnya diam-diam. Aturan ini diwariskan `RWI-DEC-081`.

---

## 3. Daftar artefak dan hash

Kesebelas berkas berikut **sudah dibuat** dan masing-masing berisi satu baris alasan bersebab,
mengikuti `blueprint-output-contract.md` bagian "File yang tidak relevan bagi modul tertentu".
Berkas dihapus tanpa jejak akan membuat pembaca tidak dapat membedakan "memang belum perlu" dari
"terlupa ditulis".

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.0` | `draft` | `3e439914e89ac90ae690caac58df4930b4ec44fba7933708d3c37007068326da` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.0` | `draft` | `82a74902d8403745af566186d09be72e344b92d40a854225137c62c8d5709e2e` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.0` | `draft` | `cf8c392c2d3bd98b79c3ffcb8ec6e99cb68e990e8d0697ca787572c6821be655` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.0` | `draft` | `70aadc6bd13107127e80fbc42c11e35491697a1f9709a89787a290c13af3710e` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.0` | `draft` | `f25fec05a2d091c370f16bce73c6792c4fb412d1c7f451f68444dd9677005a81` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.0` | `draft` | `033054f9765d91d7e8bbfc9ca627a61c825670f57e22e50bdb02529bdd7b5bc4` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.0` | `draft` | `2d8408c460a71a28ce65b8396d930a0b6d8bd5b831eee3c4eaa6dac8c9a9d737` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.0` | `draft` | `708d42cbfafbcf888732754fc409e226a897129647e5ba8fcda69d7e3130eef5` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.0` | `draft` | `2ee2422b154b9366853fe0e18a54f3049403425437954595967d593a5c10388a` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.0` | `draft` | `53e3d4160260b82e1f9b164d048f185342a9462b045a74f8100647909a2dffbd` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.0` | `draft` | `09cc8faaf6fbac6f96ab1d81baca7d6b9be463354370c97641182f6028c631cf` |

`roadmap/` dan `task/report/` belum ada, dan **itu bukan penyimpangan struktur**: keduanya ditulis
`/qv-plan` dan kedua skill build, bukan oleh skill desain.

---

## 4. Contract version

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.0.0` | — | `draft`, belum berisi |
| State transition | `0.0.0` | — | `draft`, belum berisi |
| Validation | `0.0.0` | — | `draft`, belum berisi |
| Integration | `0.0.0` | — | `draft`, belum berisi |
| Permission dan audit | `0.0.0` | — | `draft`, belum berisi |
| Acceptance test | `0.0.0` | — | `draft`, belum berisi |

Angka ini bergerak **sendiri**, terpisah dari `contract_versions` milik `episode-rawat-inap` yang
sudah berada di `0.4.0`. Itulah gunanya bentuk `COMPOSITE`: satu sub-modul boleh maju tanpa menunggu
yang lain.

---

## 5. Dependency sub-modul ini

| Bergantung pada | Untuk apa | Keadaan |
|---|---|---|
| `episode-rawat-inap` | Episode sebagai **konteks** setiap dokumen: siapa pasiennya, di mana dirawat, siapa penanggung jawabnya, dan apakah episodenya masih hidup | **Tersedia** — `approved` 2026-08-24. Sub-modul ini **membaca**, tidak menulis |
| `ClinicalManagement` | Tabel dan mesin dokumentasi klinis | **Disetujui** `RWI-DEC-062`; **belum dikerjakan** — butuh *shared inpatient clinical context resolver* |
| `Corporate HR Workforce` | Identitas penulis dokumen | Tersedia |

Arah ketergantungannya satu arah: sub-modul ini butuh `episode-rawat-inap`, tetapi
`episode-rawat-inap` **tidak** butuh sub-modul ini. Karena itu tidak ada satu pun task
`episode-rawat-inap` yang tertahan menunggu folder ini terisi.

---

## 6. Yang harus dilakukan sebelum sub-modul ini dapat dirancang

| No | Butir | Pemilik | Memblokir? |
|---:|---|---|:---:|
| 1 | Bentuk *shared inpatient clinical context resolver* — bagaimana dokumen klinis menemukan konteks rawat inap tanpa antrean dan tanpa konsultasi | Pemilik `ClinicalManagement`, yaitu Muhammad Hamzah lewat `RWI-DEC-062` | **Ya** — ini penghalang **teknis**, bukan keputusan bisnis |
| 2 | Batas waktu klinis `RWI-RULE-021` | Pemilik klinis, **belum ditunjuk** | Ya, untuk aturan waktunya saja |
| 3 | Butir menu sub-modul ini, mengingat kuota sembilan butir `IA-INP-05` sudah penuh | Blueprint, saat sub-modul dirancang | Tidak — dapat diputuskan bersamaan dengan desainnya |

Butir 1 dan 2 **tidak** menahan `episode-rawat-inap`.

---

## 7. Langkah berikutnya untuk sub-modul ini

| Kondisi | Skill |
|---|---|
| Butir 1 pada bagian 6 sudah punya bentuk yang disepakati | `/qv-design` untuk sub-modul ini |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) untuk slice ini |
| Requirement kemampuannya ingin dinilai ulang terhadap `PRD-RWI-FINAL-001` | `/qv-gate` |
| Pemilik klinis sudah ditunjuk dan `RWI-RULE-021` ingin ditutup | `/qv-grill` Amendment Pass |

Sub-modul ini **MUST NOT** diteruskan ke `/qv-plan`: belum ada satu pun kontrak yang berisi.
