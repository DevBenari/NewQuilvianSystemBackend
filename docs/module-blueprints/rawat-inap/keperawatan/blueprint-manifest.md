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
| `status` | **`draft`** — **dirancang 2026-09-02**, belum disetujui manusia. Approval tetap tindakan pemilik |
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

**`BLOCKED` berarti menunggu orang. `draft` berarti menunggu pekerjaan.**

> **Diperbarui 2026-09-02 sore.** Desainnya **sudah dikerjakan**. Sub-modul ini kini menunggu dua
> hal yang berbeda: **approval pemilik** atas dokumen ini, dan **satu perubahan teknis** milik
> `ClinicalManagement` (`INT-KEP-01`). Statusnya tetap `draft` karena approval adalah tindakan
> manusia yang tidak tergantikan skill mana pun.

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

**Seluruhnya sudah berisi desain sungguhan sejak 2026-09-02.** Baris alasan bersebab yang
sebelumnya mengisi kesebelas berkas ini sudah digantikan isinya.

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.1` | `draft` | `8b941b477fce913682724697c506892194b686ede164910ae4cd87330f0d9e15` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.1` | `draft` | `ddfb6f05de1d478f34e5c6da52e71ddf9bcfe103c9ae09aba909200b985e9de9` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.1` | `draft` | `c5d3b3742273a85a44d1e66f7240973f30496e0a725395897691cc9deb70f801` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.1` | `draft` | `c72d4ee47fd6678826b507c6a2cb53451186d2129ead1056fd86f1355c1cc0dc` |
| [`flowcharts/01-pengkajian-awal.md`](./flowcharts/01-pengkajian-awal.md) | `0.1` | `draft` | `60a956a3560d204541d8d819503c7f9934faabe72cee81b1393df72490b8ca9c` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.1` | `draft` | `5cde3e03e93c812d69fbc754889552eefc02a89b742499281534c6c1e2783d83` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.1.0` | `draft` | `a3fcb3cc6f25009180b54f65477cd1c9803aafc161b8bde32a4f450799d77cdd` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.1.0` | `draft` | `31b743eaf2b84d24ca6a03235ad9656fa967ca9a3fff36f031b25b03ddceb978` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.1.0` | `draft` | `bf811dddc5e968b596f6ff5c3d9ee3fbe9a4ac02afeb85e5b847f8bd6338cd7a` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.1.0` | `draft` | `3a4166292e3e16842ab1a1fa4a604c53f5e8b109d5494f28e369e1d68747d50c` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.1.0` | `draft` | `574fcb10c092b75e70e0ee14d812cbff9ade91c53fbdc4e626fbe209b4d11cd5` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.1.0` | `draft` | `7e402a8c2bba43d2a496f94160aec576b7b9c6f3bca87f6730d60b6ef6db71bf` |

`flowcharts/01-pengkajian-awal.md` adalah berkas per proses; jumlahnya mengikuti proses, bukan
berkas pasti.

`roadmap/` dan `task/report/` belum ada, dan **itu bukan penyimpangan struktur**: keduanya ditulis
`/qv-plan` dan kedua skill build, bukan oleh skill desain.

---

## 4. Contract version

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.1.0` | `0.1.0` | `draft` — 13 endpoint rencana, nol tersedia |
| State transition | `0.1.0` | `0.1.0` | `draft` — tiga mesin status, nol beririsan dengan status episode |
| Validation | `0.1.0` | `0.1.0` | `draft` — 18 aturan |
| Integration | `0.1.0` | `0.1.0` | `draft` — lima integrasi; `INT-KEP-01` menahan seluruh sub-modul |
| Permission dan audit | `0.1.0` | `0.1.0` | `draft` — dua Resource baru |
| Acceptance test | `0.1.0` | `0.1.0` | `draft` — 24 skenario, 11 di antaranya jalur gagal |

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
| 3 | ~~Butir menu sub-modul ini~~ | **Ditetapkan 2026-09-02** — nol butir menu tingkat dua; keenam layar menjadi layar anak `FE-INP-04` dan `FE-INP-09`. Lihat `03-frontend-architecture.md` bagian 2 | **Tertutup** |
| 4 | **Siapa pemilik tabel catatan pemakaian alat** (`CAP-016`). PRD 23.1 tidak memuat barisnya. Usulan `RWI-OQ-048` | Product/Domain bersama pemilik persediaan | **Ya — untuk `CAP-016` saja.** Tiga kemampuan `MUST HAVE` tidak tertahan |

Butir 1 dan 2 **tidak** menahan `episode-rawat-inap`.

---

## 7. Langkah berikutnya untuk sub-modul ini

| Kondisi | Skill |
|---|---|
| ~~Desain sub-modul ini~~ | **Selesai 2026-09-02.** Hasilnya `draft` |
| `RWI-OQ-048` dijawab, **atau** `EPIC KEP-06` dikeluarkan dari scope secara tertulis | `/qv-plan` untuk sub-modul ini |
| `RWI-OQ-048` ingin ditutup | `/qv-grill` Amendment Pass |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) untuk slice ini |
| Requirement kemampuannya ingin dinilai ulang terhadap `PRD-RWI-FINAL-001` | `/qv-gate` |
| Pemilik klinis sudah ditunjuk dan `RWI-RULE-021` ingin ditutup | `/qv-grill` Amendment Pass |

Sub-modul ini **MUST NOT** diteruskan ke `/qv-plan` sebelum `RWI-OQ-048` dijawab atau `EPIC KEP-06` dikeluarkan dari scope secara tertulis — `04-prd-to-mvp.md` bagian 20.
