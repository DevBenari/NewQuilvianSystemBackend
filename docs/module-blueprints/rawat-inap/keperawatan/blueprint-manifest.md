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
| `revision` | **`7`** — satu angka, dipegang tingkat modul. Amandemen penyelarasan `PRD-RWI-V2-001` pada bagian 8 |
| `status` | **`draft`** — amandemen revision `7` / kontrak `0.5.0` ditulis 2026-09-15 dan menunggu approval. Sebelumnya `approved`: revision `5` disetujui Muhammad Hamzah 2026-09-03 lewat `RWI-DEC-092`; kontrak `0.4.0` 2026-09-11 lewat `RWI-DEC-105` |
| `upstream_realignment` | **`REALIGNED_DRAFT` sejak 2026-09-15.** Temuan `RLN-04`, `RLN-06` s.d. `RLN-09` diserap revision `7` bagian 8; `RWI-DEC-089` diamendemen `RWI-DEC-108` (menu tampil, backend tetap nol). Sebelumnya `STALE_AGAINST_UPSTREAM` sejak 2026-09-14. Task baru tetap tidak boleh diturunkan sampai revision `7` disetujui |
| `contract_versions` | **`0.5.0`** — seluruh enam kontrak, `draft`, bagian 8.1. `0.4.0` `approved` 2026-09-11 lewat `RWI-DEC-105`. Tabel bagian 4 tetap sebagai jejak `0.3.0` |
| `prefix` | Entity `Inp`; task `BE-RWI-###` dan `FE-RWI-###`, deret bersama seluruh modul |
| `approved_by` | **Muhammad Hamzah** — Product/Domain owner, ditunjuk `RWI-DEC-061` |
| `approved_at` | `2026-09-03` |
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
| [`contracts/api-contract.md`](./contracts/api-contract.md) | `0.4.0` | `draft` | hash dihitung ulang saat approval `0.4.0` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | `0.3.0` | `draft` | `9dbc84303410c716b55bb69ad52734b8569d5ec09dc5eab0418975005106b00e` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | `0.1.0` | `draft` | `bf811dddc5e968b596f6ff5c3d9ee3fbe9a4ac02afeb85e5b847f8bd6338cd7a` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | `0.3.0` | `draft` | `f84ad786a26f7c611c84f3b73cf602c5d1288f7e1ef1850488415171055e399e` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | `0.1.0` | `draft` | `574fcb10c092b75e70e0ee14d812cbff9ade91c53fbdc4e626fbe209b4d11cd5` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | `0.3.0` | `draft` | `ffe947c9aa89e6e4d695e12c04d1e360ae1fe8864e9cd48d7bd1d0a457319c7a` |

`flowcharts/01-pengkajian-awal.md` adalah berkas per proses; jumlahnya mengikuti proses, bukan
berkas pasti.

`roadmap/` berstatus **`APPROVED` sejak 2026-09-05**, ditulis ulang `/qv-plan` menjadi
`roadmap_revision` `2` sebagaimana dituntut `RWI-DEC-092`: **12** task backend `BE-RWI-054` s.d.
`BE-RWI-065` dan 6 task frontend `FE-RWI-051` s.d. `FE-RWI-056`, **nol** di antaranya `BLOCKED`.
Revision `1` yang berstatus `DRAFT_STALE` disimpan di `roadmap/archive/revision-1/`.

Tiga hal menaikkannya: approval `RWI-DEC-092`; penyerapan `RWI-DEC-091` yang mengubah bentuk
koreksi menjadi addendum dan melahirkan task baru `BE-RWI-065`; dan pembacaan source pada
`BE@7d4bf2b` yang menemukan `INT-KEP-01` beserta substansi `RWI-OQ-051` **sudah mendarat** lewat
`BE-RWI-038`, `BE-RWI-039`, dan `BE-RWI-040` milik `dokter-rawat-inap` — sesuai `INT-DOK-09`.
Rinciannya ada pada `roadmap/backend-roadmap.md` bagian 2.

`task/report/` belum ada, dan **itu bukan penyimpangan struktur**: ia ditulis kedua skill build,
bukan oleh skill desain maupun perencanaan.

---

## 4. Contract version

Seluruhnya **`approved`** sejak 2026-09-03 lewat `RWI-DEC-092`, atas nama Muhammad Hamzah.

| Kontrak | Version | `last_changed_in` | Status |
|---|---|---|---|
| API | `0.3.0` | `0.3.0` | `approved` — **21 baris endpoint**, nol tersedia; `0.3.0` mengganti dua endpoint amandemen dengan empat endpoint addendum. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |
| State transition | `0.3.0` | `0.3.0` | `approved` — tiga mesin status, nol beririsan dengan status episode. **Isinya tidak bergerak** pada `0.2.0`; lihat butir konsistensi `04-prd-to-mvp.md` bagian 20.1 |
| Validation | `0.3.0` | `0.1.0` | `approved` — 18 aturan. **Isinya tidak bergerak** pada `0.2.0` |
| Integration | `0.3.0` | `0.3.0` | `approved` — **enam** integrasi sejak `INT-KEP-06`; `INT-KEP-01` menahan seluruh sub-modul. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |
| Permission dan audit | `0.3.0` | `0.1.0` | `approved` — dua Resource baru. **Isinya tidak bergerak** pada `0.2.0` |
| Acceptance test | `0.3.0` | `0.3.0` | `approved` — **22 baris skenario**, termasuk tujuh skenario koreksi baru pada bagian 8. `0.2.0`: baris `CAP-016` menjadi `DEFERRED` |

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

---

## 8. Amandemen revision `7` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status sub-modul: `draft`.** Seluruh artefak di bawah menunggu approval manusia. Approval revision `5` (2026-09-03)
dan kontrak `0.4.0` (2026-09-11) tetap sah untuk isi yang tidak berubah; task `✅` `BE-RWI-054` s.d. `065`, `077`,
`078` dan `FE-RWI-051` s.d. `056` tetap sah; **task baru hanya boleh diturunkan dari revision ini setelah disetujui**.

Bagian 3 dan 4 di atas dipertahankan sebagai jejak revision `5`; nilai yang berlaku ada di bagian ini.

### 8.1 Tabel artefak dan hash

| Artefak | Revision | Status | SHA-256 |
| --- | --- | --- | --- |
| [`02-backend-architecture.md`](./02-backend-architecture.md) | **`0.4`** — bagian 11 | `draft` | `e7e2bcff7a0add53f4533b8d20ad94f8634bda77ee0463dab24e071b8c6579cf` |
| [`03-frontend-architecture.md`](./03-frontend-architecture.md) | **`0.3`** — bagian 10 | `draft` | `778167c91c60d903921adad99f6bad8aed4c2961832ae2b18598ced7a8f65446` |
| [`04-prd-to-mvp.md`](./04-prd-to-mvp.md) | **`0.4`** — bagian 22 | `draft` | `eb4e691c54c2a459cde74a05945ec28801702ea8270addc7b7bc874457c73433` |
| [`flowcharts/00-alur-utama.md`](./flowcharts/00-alur-utama.md) | **`0.3`** — bagian 4 | `draft` | `84e24a5540f1aed7eb3079ecdff91a653e70af74c2b2cacf6d0433fc6c412ebe` |
| [`flowcharts/01-pengkajian-awal.md`](./flowcharts/01-pengkajian-awal.md) | `0.2` — tidak bergerak; dibaca bersama `02-…` untuk jenis dokumen V2 | `approved` | `01705b6c2e40b803884706e8eab0fe3abea715582a2b4836f5964bd9a1346b55` |
| [`flowcharts/02-pengkajian-pasien-dan-instrumen.md`](./flowcharts/02-pengkajian-pasien-dan-instrumen.md) | **`0.3`** — baru | `draft` | `e759445e2185ce30ee84cf0ab07483496b3d020bfa729736c5f9bb3f09d78c39` |
| [`flowcharts/03-obat-mar-dan-sliding-scale.md`](./flowcharts/03-obat-mar-dan-sliding-scale.md) | **`0.3`** — baru | `draft` | `c966b0eb660e7c3c7695d2e4a906bbe5c5d8dee1b509a901c2687732d5761067` |
| [`flowcharts/04-pengawasan-harian-dan-cairan.md`](./flowcharts/04-pengawasan-harian-dan-cairan.md) | **`0.3`** — baru | `draft` | `042c798d962279a655811af7914ac2c607007d56b72657edc74081f9a88dd17e` |
| [`data/data-dictionary.md`](./data/data-dictionary.md) | **`0.4`** — bagian 11 | `draft` | `a5c86ce91d80d7d47d37e8c041abe836a867407346b650e00f1ebdb1f543ac56` |
| [`contracts/api-contract.md`](./contracts/api-contract.md) | **`0.5.0`** — bagian 7 | `draft` | `ce1015c2a12d13a89359f497bf49869b2585cb411c9cea7070726d6f320f1cf0` |
| [`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md) | **`0.5.0`** — bagian 5 | `draft` | `f4d6debb8bd5f6dba51288c900f25986936c88b6e58c726d107bd4e8ce909ec3` |
| [`contracts/validation-matrix.md`](./contracts/validation-matrix.md) | **`0.5.0`** — bagian 6 | `draft` | `e68e4a20afb0b2b2b009341643553cb746afcb05d33e3d04c70e4097a908bc82` |
| [`contracts/integration-contract.md`](./contracts/integration-contract.md) | **`0.5.0`** — bagian 8 | `draft` | `e421eb31816ad2ee63b3ee6746702c3709ba4ba5ae07f3ed6beb0fbc0ef6e14c` |
| [`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md) | **`0.5.0`** — bagian 6 | `draft` | `5de18c7195f08213dc3d1906660fc9e1017cded16bc664db798f189c810a231e` |
| [`testing/acceptance-test-matrix.md`](./testing/acceptance-test-matrix.md) | **`0.5.0`** — bagian 9 | `draft` | `ad5c4384deb77ddb70f9de6445ba29867a32266c3360829f1b8706ad0dae3c33` |
| [`skema-tampilan-keperawatan-rawat-inap.md`](./skema-tampilan-keperawatan-rawat-inap.md) | Pendamping, bukan himpunan canonical | Ditandai sebagian basi | `a96d205331d4412c67a2846bbe902cef93ea53d2e884ae3cdea49c6a606c860c` |

**Seluruh enam kontrak naik ke `0.5.0`**, termasuk validation dan integration yang melompati versi di antaranya karena
isinya tidak bergerak. `roadmap/` revision `2` tetap `APPROVED` untuk isi lamanya dan **tidak** memuat task revision `7`.

### 8.2 Masukan yang diserap

| Masukan | Revision / hash |
| --- | --- |
| `00-interview-decisions.md` | revision `21`, `1c55c80a50aee11ef005ccde6315c2935cbe21504e8596798b89bf7f2d45102a` |
| `01-existing-capability-map.md` | revision `1.4`, `337a10f09d6e91b06395405098bad09623452de062e10a720a637bd22daa543a` — bagian 17 |
| `evidence/02-requirement-completeness-gate.md` | revision `1.6`, `f31d207ae0cac120b0821d4474a3d952e109293c2b517aa630370396e49b5300` |
| `PRD-RWI-V2-001` v`2.0` | `2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f` |
| Backend / frontend | `df3679c0d5b2f08106702153eb242d3a6cb2929b` / `1ce219b40f8e411f3c4e66975626ab33ae81616a` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — gate `1.6` bagian 15.15 |

### 8.3 Kemampuan setelah amandemen

| Kemampuan | ID | Keadaan |
| --- | --- | --- |
| Pengkajian Pasien tujuh isi, progres, konfigurasi berversi, Pengawasan Harian, Evaluasi Awal | `CAP-012` | Dirancang |
| Rencana asuhan | `CAP-013` | Tetap — di dalam Asuhan Keperawatan |
| Tindakan harian, SOAP dan Catatan Keperawatan | `CAP-014` | Diperluas — jenis catatan dari `dokter-rawat-inap` |
| MAR, obat bawaan dicatat perawat, pelaksanaan sliding scale | **`CAP-023-MAR`** | Dirancang |
| Pemakaian alat | `CAP-016` | `DEFERRED`; menu "Integrasi belum tersedia" |
| Asuhan gizi | `CAP-027` | `POST-MVP`; kartu Konsultasi Gizi "Integrasi belum tersedia" |
| Handover shift, transfusi | — | **`DEFERRED`** — `RWI-DEC-145`; tanpa menu, tabel, endpoint |
| Permukaan `CAP-015-LAB`, `RAD`, `GIZ`, `HD`, `BDR`, `RHB`, `CAP-017`, `CAP-018`, `CAP-019` | Milik sub-modul lain | Tampil di ruang kerja perawat |

### 8.4 Dependency dan gerbang — revision `7`

| Bergantung pada | Untuk apa | Keadaan | Menahan |
| --- | --- | --- | --- |
| `dokter-rawat-inap` `0.6.0` | Jenis catatan CPPT, pesanan dengan pemberi instruksi, rekonsiliasi, penghentian butir, order sliding scale | Dirancang pada pass yang sama, `draft` | `KEP-V2-2`, `KEP-V2-3` |
| `episode-rawat-inap` `0.9.0` | Langkah penutupan `INT-KEP-15`; perpindahan tempat tidur | Dirancang pada pass yang sama, `draft` | Bagian penutupan `KEP-V2-2` |
| Yoga Aji Pratama — `MedicalRecordManagement` | Jenis dokumen `CaseManagementEvaluation = 14` | **Belum ada persetujuan** | Addendum dan penguncian Evaluasi Awal |
| Pemilik `BillingManagement` | Ringkasan tagihan | **Belum ada kontrak** | `KEP-V2-4` |
| Pemilik `LaboratoryManagement` dan `RadiologyManagement` | Pesanan perawat | **Belum tercatat** | Bagian Lab/Rad `KEP-V2-3` |
| Pemilik blueprint `rawat-jalan` | Pemberitahuan perubahan perhitungan risiko jatuh sebelum migration K2 | **Belum diberitahu** | Rilis K2 |
| Pemilik klinis | Isi instrumen, checklist MPP, isian wajib, daftar high-alert | **Belum ditunjuk** | Gerbang produksi |
| Farmasi | Jam pemberian per frekuensi | Belum diisi | Pemakaian MAR untuk pasien sungguhan |

### 8.5 Handoff

| Field | Nilai |
| --- | --- |
| `blueprint_id` / `revision` | `RWI-BP-001` / `7` |
| `contract_versions` | `0.5.0` |
| `approval_status` | `draft` — `approved_by` dan `approved_at` kosong |
| `requirement_readiness` | `READY_FOR_DOMAIN_DESIGN` — gate `1.6` |
| `blocking_questions` | **Nol.** Non-blocking untuk dikonfirmasi saat approval: `04-prd-to-mvp.md` 22.20 nomor 5 s.d. 9 |
| `next_owner` | Approval pemilik atas revision `7`; setelah itu `plan-module-delivery` untuk `KEP-V2-0` s.d. `KEP-V2-4` |
| `expected_output` | `roadmap_revision` `3` sub-modul ini dengan task mulai nomor berikut deret bersama `BE-RWI-###`/`FE-RWI-###`; task Evaluasi Awal addendum, Tagihan Pasien, dan pesanan Lab/Rad ditandai tertahan pemilik modul tetangga |
