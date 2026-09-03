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
| `status` | **`draft`** — **dirancang 2026-09-02**, belum disetujui manusia. Approval tetap tindakan pemilik |
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

**`BLOCKED` berarti menunggu orang. `draft` berarti menunggu pekerjaan.**

> **Diperbarui 2026-09-02 sore.** Desainnya **sudah dikerjakan**. Yang tersisa: **approval pemilik**
> atas dokumen ini, ditambah dua perubahan teknis milik `ClinicalManagement` dan
> `PharmacyManagement` — `INT-DOK-01` dan `INT-DOK-02` — yang keputusannya sudah turun sejak
> 2026-08-21 tetapi kodenya belum ada.
>
> **Berbeda dari `keperawatan`, sub-modul ini tidak menyisakan satu pun `OPEN DECISION`
> kepemilikan.** Ketujuh kemampuannya punya pemilik data yang tegas pada `PRD-RWI-FINAL-001`
> bagian 23.1.

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

**Seluruhnya sudah berisi desain sungguhan sejak 2026-09-02.**

| Artefak | Revision | Status | SHA-256 |
|---|---|---|---|
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.1` | `draft` | `5f61fd77012768fd6cf36fae63db18a3fdd3dfa73dcbafb2581e0915e40060e0` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.1` | `draft` | `3e6d28e20453491be24a6dfbd1837210768266ef1cae5edf261fa0995a00826f` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.1` | `draft` | `505328a54aecad35825d764754e04bce7fc06003661c6c0586da3260eff3e7ad` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.1` | `draft` | `90b947669d76f429e9a46e1b23521dd58cb960a870381d70852dae40c089e43f` |
| [`flowcharts/01-catatan-harian-dan-visite.md`](./flowcharts/01-catatan-harian-dan-visite.md) | `0.1` | `draft` | `c9f8b567fb619777761f202b9d29702af75adf21572c4e57b700037a6ea84387` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.1` | `draft` | `f621fc875c537a4774fc8a40571132538b304d7212d047ee2b47bb8689e2c235` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.1.0` | `draft` | `6096aa168580c8c2f9e3f6b39ad17b4e885c78f1dd602b9695a725284574974e` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.1.0` | `draft` | `5c60238839a8377bc5cd02f482a47c2b5539fdbc8c7da5ad082dc4c5e500dbd5` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.1.0` | `draft` | `bcaffeafcc8e8e1cfe6d1b56ee31eb825f8c81cfb9df6621b5789c266446105a` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.1.0` | `draft` | `a9d9d8ab8e2395c950f01aa3d82e9e178c925e88779c89afa1e3c9f217bc3553` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.1.0` | `draft` | `5eb0032d0e58d40dbd2f4b7383c971a6488e886c0fe268dbac4b388cb17d3a07` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.1.0` | `draft` | `bb60db8796a905568b3e14713c60d6fde3c1b3ff04278881e5581e8d2d5c18e4` |

`flowcharts/01-catatan-harian-dan-visite.md` adalah berkas per proses; jumlahnya mengikuti proses.

`roadmap/` dan `task/report/` belum ada, dan **itu bukan penyimpangan struktur**: keduanya ditulis
`/qv-plan` dan kedua skill build, bukan oleh skill desain.

---

## 4. Contract version

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.1.0` | `0.1.0` | `draft` — 17 endpoint rencana, tersebar di 3 modul pemilik |
| State transition | `0.1.0` | `0.1.0` | `draft` — 4 mesin dimiliki, 2 hanya dibaca |
| Validation | `0.1.0` | `0.1.0` | `draft` — 25 aturan |
| Integration | `0.1.0` | `0.1.0` | `draft` — 8 integrasi; `INT-DOK-01` dan `INT-DOK-02` menahan seluruh sub-modul |
| Permission dan audit | `0.1.0` | `0.1.0` | `draft` — 1 Resource baru, 2 Action baru |
| Acceptance test | `0.1.0` | `0.1.0` | `draft` — 28 skenario, 12 di antaranya jalur gagal |

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
| 3 | ~~Butir menu sub-modul ini~~ | **Ditetapkan 2026-09-02** — nol butir menu tingkat dua; kedelapan layar menjadi layar anak `FE-INP-04` dan `FE-INP-09` | **Tertutup** |
| 4 | **Pelonggaran batas satu konsultasi per kunjungan dan satu resep aktif per konsultasi** (`INT-DOK-02`). Keputusannya `approved` sejak `RWI-DEC-038`; kodenya belum ada | `ClinicalManagement`, `PharmacyManagement` | **Ya** — tanpanya dokter hanya dapat menulis satu catatan dan satu resep untuk seluruh masa perawatan |
| 5 | Kajian medis memakai ulang `TrxPatientAssessment` atau tabel tersendiri | Product/Domain bersama `ClinicalManagement` | Tidak — keputusan struktur; lihat `02-backend-architecture.md` bagian 4.2 |

Butir 1 dan 2 **tidak** menahan `episode-rawat-inap`.

---

## 7. Langkah berikutnya untuk sub-modul ini

| Kondisi | Skill |
|---|---|
| ~~Desain sub-modul ini~~ | **Selesai 2026-09-02.** Hasilnya `draft` |
| Owner menyetujui dokumen ini | `/qv-plan` untuk sub-modul ini — **nol pertanyaan memblokir** |
| Pertanyaan struktur butir 5 ingin ditutup lebih dulu | `/qv-grill` Amendment Pass |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) untuk slice ini |
| Requirement kemampuannya ingin dinilai ulang terhadap `PRD-RWI-FINAL-001` | `/qv-gate` |
| Pemilik klinis sudah ditunjuk dan `RWI-RULE-021` ingin ditutup | `/qv-grill` Amendment Pass |

Sub-modul ini **dapat** diteruskan ke `/qv-plan` begitu owner menyetujui dokumen ini —
`04-prd-to-mvp.md` bagian 20 tidak memuat satu pun pertanyaan memblokir. Pertanyaan struktur pada
bagian 6 butir 5 sebaiknya dijawab lebih dulu supaya arsitekturnya tidak berubah di tengah
pengerjaan.
