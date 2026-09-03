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
| `status` | **`draft`** — **dirancang 2026-09-02**, diamandemen 2026-09-02 menyerap `RWI-DEC-089` dan `RWI-DEC-091`. Belum disetujui manusia; approval tetap tindakan pemilik |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###`, deret bersama seluruh modul |
| `approved_by` | — belum ada yang disetujui |
| `approved_at` | — |
| `rumpun kemampuan` | Pengkajian, asuhan, dan tindakan keperawatan, ditambah gizi dan pemakaian alat |
| `kemampuan` | **5** — `CAP-012`, `CAP-013`, `CAP-014`, `CAP-016`, `CAP-027`, sesuai `RWI-DEC-083`. Empat aktif; **`CAP-016` berstatus `DEFERRED`** sejak `RWI-DEC-089` |
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
| [`02-backend-architecture.md`](./02-backend-architecture.md) | `0.3` | `draft` | `251ffd7df8e6a3f641a796f9e44088331694cb0bc054d80edbecb81cb5914baf` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | `0.2` | `draft` | `1c42b19621ef2ebf937233f18f765b4a6f58819d255acecd1943a502e2e736d9` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | `0.3` | `draft` | `a1c5d9c84e9743a48629f07b07aa3a3f59712c4f2296ca70acffd4aa7d5971a0` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | `0.2` | `draft` | `0fc48f722da869eeb633a3ce0c8e163fa8adab4802c87f532a508dad1e4959ce` |
| [`flowcharts/01-pengkajian-awal.md`](./flowcharts/01-pengkajian-awal.md) | `0.2` | `draft` | `01705b6c2e40b803884706e8eab0fe3abea715582a2b4836f5964bd9a1346b55` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | `0.3` | `draft` | `857d2b3a52e0f33278d14598551820c6f03e6348e3a5e241f851c5360da8ffec` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.3.0` | `draft` | `67fc8e97ca6a29a5789972dc31f75fa803db9626a96799a8dea3bf47f7baba9d` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.3.0` | `draft` | `9dbc84303410c716b55bb69ad52734b8569d5ec09dc5eab0418975005106b00e` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.1.0` | `draft` | `bf811dddc5e968b596f6ff5c3d9ee3fbe9a4ac02afeb85e5b847f8bd6338cd7a` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.3.0` | `draft` | `f84ad786a26f7c611c84f3b73cf602c5d1288f7e1ef1850488415171055e399e` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.1.0` | `draft` | `574fcb10c092b75e70e0ee14d812cbff9ade91c53fbdc4e626fbe209b4d11cd5` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.3.0` | `draft` | `ffe947c9aa89e6e4d695e12c04d1e360ae1fe8864e9cd48d7bd1d0a457319c7a` |

`flowcharts/01-pengkajian-awal.md` adalah berkas per proses; jumlahnya mengikuti proses, bukan
berkas pasti.

`roadmap/` **sudah ada sejak 2026-09-02**, ditulis `/qv-plan` dengan status `DRAFT` / `FORWARD_TEST`:
11 task backend `BE-RWI-054` s.d. `BE-RWI-064` dan 6 task frontend `FE-RWI-051` s.d. `FE-RWI-056`,
seluruhnya `BLOCKED` pada gerbang approval sub-modul ini. `task/report/` belum ada, dan **itu bukan
penyimpangan struktur**: ia ditulis kedua skill build, bukan oleh skill desain maupun perencanaan.

---

## 4. Contract version

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.3.0` | `0.3.0` | `draft` — **21 baris endpoint**, nol tersedia; `0.3.0` mengganti dua endpoint amandemen dengan empat endpoint addendum. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |
| State transition | `0.3.0` | `0.3.0` | `draft` — tiga mesin status, nol beririsan dengan status episode. **Isinya tidak bergerak** pada `0.2.0`; lihat butir konsistensi `04-prd-to-mvp.md` bagian 20.1 |
| Validation | `0.3.0` | `0.1.0` | `draft` — 18 aturan. **Isinya tidak bergerak** pada `0.2.0` |
| Integration | `0.3.0` | `0.3.0` | `draft` — **enam** integrasi sejak `INT-KEP-06`; `INT-KEP-01` menahan seluruh sub-modul. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |
| Permission dan audit | `0.3.0` | `0.1.0` | `draft` — dua Resource baru. **Isinya tidak bergerak** pada `0.2.0` |
| Acceptance test | `0.3.0` | `0.3.0` | `draft` — **22 baris skenario**, termasuk tujuh skenario koreksi baru pada bagian 8. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |

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
| **`MedicalRecordManagement`** | **Mesin keutuhan dan addendum dokumen klinis** — jalur koreksi pengkajian dan catatan tindakan sejak `RWI-DEC-091` | **Tersedia dan sudah dipakai**, tetapi **belum menegakkan** jenis `Assessment` dan `Procedure`. `RWI-OQ-051` meminta perluasan itu; nol nilai enum baru |

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
| 4 | ~~**Siapa pemilik tabel catatan pemakaian alat** (`CAP-016`)~~ | **Ditutup 2026-09-02** oleh `RWI-DEC-089`: `EPIC KEP-06` dikeluarkan dari scope rilis pertama secara tertulis, dan kepemilikan tabelnya sengaja ditunda sampai modul persediaan/aset ada | **Tertutup** |
| 5 | ~~Apakah dokumen keperawatan memakai mesin keutuhan dokumen~~ | **Ditutup 2026-09-02** oleh `RWI-DEC-091`: koreksi dibedakan dari perkembangan. Pengkajian dan catatan tindakan memakai addendum; rencana asuhan tetap berversi | **Tertutup** |
| 6 | **Perluasan penegakan keutuhan bagi jenis `Assessment` dan `Procedure`** `RWI-OQ-051`. Mesinnya sudah ada dan nomornya sudah tersedia, tetapi hari ini hanya `ProgressNote` yang ditegakkan `RM-DEC-019` | Pemilik `MedicalRecordManagement`, **belum dinyatakan** | **Tidak menahan desain.** Menahan **implementasi** `BE-RWI-057` dan `BE-RWI-062` |

Butir 1 dan 2 **tidak** menahan `episode-rawat-inap`.

---

## 7. Langkah berikutnya untuk sub-modul ini

| Kondisi | Skill |
|---|---|
| ~~Desain sub-modul ini~~ | **Selesai 2026-09-02.** Hasilnya `draft` |
| ~~`RWI-OQ-048` dijawab, atau `EPIC KEP-06` dikeluarkan dari scope secara tertulis~~ | **Terpenuhi 2026-09-02** lewat `RWI-DEC-089`. `/qv-plan` untuk sub-modul ini **sudah boleh** dijalankan bagi `CAP-012`, `CAP-013`, `CAP-014`, dan `CAP-027` |
| ~~Butir konsistensi mesin koreksi~~ | **Ditutup 2026-09-02** oleh `RWI-DEC-091`; desainnya sudah diserap revision `0.3` |
| `RWI-OQ-051` ingin ditutup sebelum pembangunan | `/qv-grill`, lalu konfirmasi pemilik `MedicalRecordManagement` |
| Batas domain dokumentasi klinis ingin ditetapkan lebih dulu | `/qv-domain` (opsional) untuk slice ini |
| Requirement kemampuannya ingin dinilai ulang terhadap `PRD-RWI-FINAL-001` | `/qv-gate` |
| Pemilik klinis sudah ditunjuk dan `RWI-RULE-021` ingin ditutup | `/qv-grill` Amendment Pass |

**Gerbang `/qv-plan` sudah terbuka sejak 2026-09-02.** `RWI-DEC-089` mengeluarkan `EPIC KEP-06` dari scope rilis pertama secara tertulis, yaitu tepat syarat yang dituntut kalimat ini sebelumnya — `04-prd-to-mvp.md` bagian 20.

Dua hal tetap wajib dibaca sebelum task pertama dibangun, dan keduanya **bukan** penghalang perencanaan:

1. `INT-KEP-01` *shared inpatient clinical context resolver* masih menahan **pemakaian** kelima kemampuan untuk pasien sungguhan. Ia penghalang teknis milik `ClinicalManagement`, bukan keputusan bisnis.
2. Butir konsistensi mesin koreksi pada `04-prd-to-mvp.md` bagian 20.1 wajib dijawab **sebelum** `EPIC KEP-01` s.d. `EPIC KEP-04` dibangun, karena jawabannya menentukan bentuk mesin amandemen keempatnya.
