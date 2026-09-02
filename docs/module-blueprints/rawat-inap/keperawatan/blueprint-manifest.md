# Sub-modul `keperawatan` — Blueprint Manifest

Sub-modul dari modul [`rawat-inap`](../blueprint-manifest.md), bentuk `COMPOSITE` sejak
`RWI-DEC-082`. Identitas modul, snapshot SHA, hash masukan hulu, dan registry sub-modul dipegang
manifest tingkat modul. Berkas ini memegang **status desain, `contract_versions`,
`artifact_hashes`, approval, dan dependency sub-modul ini sendiri**.

| Field | Value |
|---|---|
| `submodule_slug` | `keperawatan` |
| `judul` | Keperawatan Rawat Inap |
| `blueprint_id` | `RWI-BP-001` — satu untuk seluruh modul |
| `revision` | `5` — satu angka, dipegang tingkat modul |
| `status` | **`draft`** — lahir 2026-09-02 bersama migrasi bentuk; **belum dirancang** |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###`, deret bersama seluruh modul |
| `approved_by` | — belum ada yang disetujui |
| `approved_at` | — |
| `rumpun kemampuan` | Pengkajian, asuhan, dan tindakan keperawatan, ditambah gizi dan pemakaian alat |
| `kemampuan` | **5** — `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, `CAP-027`, sesuai `RWI-DEC-083` |
| `uji pemecahan` | **3/5** syarat `bentuk-blueprint.md` bagian 4.1, sebagaimana dicatat `RWI-DEC-082` |
| `peran pemilik` | Perawat pelaksana dan kepala ruangan |

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
| Pengkajian awal dan pengkajian ulang keperawatan | `CAP-012` | Nursing Assessment |
| Diagnosis, rencana asuhan, dan evaluasi keperawatan | `CAP-013` | Nursing Care |
| Catatan dan tindakan keperawatan | `CAP-014` | Nursing Interventions |
| Pencatatan pemakaian alat | `CAP-016` | Equipment Usage |
| Asuhan gizi | `CAP-027` | Nutrition Care |

Dua dari lima kemampuannya — `CAP-016` pemakaian alat dan `CAP-027` asuhan gizi — mesinnya dimiliki modul yang belum berjalan. Yang menjadi milik sub-modul ini pada keduanya adalah **rujukan dan status**, bukan mesinnya.

Pemetaan lengkap ke-28 kemampuan modul ada di
[`../02-module-map.md`](../02-module-map.md) bagian 4.

---

## 2. Kepemilikan data

**Sub-modul ini tidak memiliki satu tabel pun.** `RWI-DEC-081` menetapkan seluruh tabel dokumentasi
klinis rawat inap — pengkajian, CPPT, SOAP, kajian medis, resep, dan tindakan — dimiliki
`ClinicalManagement`. Rawat Inap hanya menyediakan **workspace, konteks episode, dan kontrak**.

| Yang dimiliki sub-modul ini | Yang dipakai dari modul lain |
|---|---|
| Nol tabel. Nol migration. Nol `DbSet` | `ClinicalManagement` untuk pengkajian, asuhan, dan tindakan keperawatan; modul Gizi untuk asuhan gizi; modul persediaan untuk pemakaian alat |

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
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.0` | `draft` | `f9760a118f27b53c1f2042e3cb291dcec9eec717ffed1426ce98f0c52df83a30` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.0` | `draft` | `99b9913ff8b6ae7b44432e1fbb0335424857a66b52bdd10d985005074d626137` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.0` | `draft` | `4aed01a89a753e51502c6b5c498ddf89bed28cb3859e7a836196e6fbb79475a2` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.0` | `draft` | `860ff330bcbcc84d04e841f8f778b4d8ce6d0811db8509364129d1b4a348addf` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.0` | `draft` | `7ae2a315721d0c752c35f898fbb73a314f12b487a61758c64eca31131ca6b799` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.0` | `draft` | `e9c796878da6011b55be80e2faec1757674573ee3d4d761c888a290ff67dfeca` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.0` | `draft` | `2568006915d15afc70592778fd029d9b0dbb9b038bd979e1c4a1abf112a6ee7c` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.0` | `draft` | `03cb4bbeca10af60f0b3c0bbb7cf7fc8b06a2de8dd5411115078bd7858cb3f5d` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.0` | `draft` | `64011e9775857ea32e5df7b87b599e1209811937a51568fe50001e48f3209343` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.0` | `draft` | `b1de1236c76a96b20c5d1e00f41501e7fe7e0da517e56f97c7dc363225d1db83` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.0` | `draft` | `bf759f4750c27d4fb245ffcccead195e5d5bb4111edae89a3f6c4551e745d6fd` |

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
