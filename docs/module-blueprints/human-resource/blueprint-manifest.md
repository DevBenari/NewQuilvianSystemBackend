# Human Resource — Blueprint Manifest

```yaml
blueprint_id: HRD-BP-001
module_name: Human Resource
module_slug: human-resource
module_prefix: HRD
blueprint_shape: SINGLE
revision: 16
status: DRAFT
design_readiness: PARTIAL
current_phase: HRD-PH-SLICE-D2-PLANNED
created_at: 2026-08-27T07:02:01Z
updated_at: 2026-11-03T15:15:00Z
last_verified_at: 2026-11-02T14:03:00Z
backend_source_sha_audited: ecdc135444f0110482c9702212bcea30043983c8
backend_source_sha_verified: 16b8b71f4cd61e083213cf90722f4d768d339739
backend_source_sha_current: e8d47c77
frontend_source_sha: cfa82fbb3
frontend_source_sha_previous: 66b7e5600e1707e4153ca07981ddef054856c8c9
skill_suite_version: 1.0.0-rc2
input_revision_hash: 446d1d3b80bea248f67d799866a542cb457107929eb56c3ddf6df31f1dec6403
decision_revision: 18
backend_baseline_branch: QuilvianIntegrationBackend
capability_map_revision: 1.3
capability_map_hash: 9a809b3b684b294878c43b1ae3f2de88735c9e7621aaa04a91a8909d7a6e91da
contract_versions: v8
contract_status_s_d1: approved
contract_status_s_d2: approved
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
artifact_hashes:
  00-interview-decisions.md: 446d1d3b80bea248f67d799866a542cb457107929eb56c3ddf6df31f1dec6403
  01-existing-capability-map.md: 9a809b3b684b294878c43b1ae3f2de88735c9e7621aaa04a91a8909d7a6e91da
  02-backend-architecture.md: ba94e9c31fc17152ac83fa743d36a15511ac1a7dc2b815dc8f4f792908c1461d
  03-frontend-architecture.md: 40da6cf7196418a1e63d9a60a82b8e923d5edc0a2d17dba861050d628612d882
  04-prd-to-mvp.md: f5b38f09d91040c346e2b00af127ce71cb26fa176cce52afb059fe8f6b86ec13
  data/data-dictionary.md: c47155da69744639f89798f3fdf562a2d72eec71725642c8f6ab35a7f421166d
  contracts/api-contract.md: a6a58e82890cc70b84549f3d38a260984b21f7c7f0e7e6fba3bf192e3522f007
  contracts/state-transition-matrix.md: e53a0b773b38ca85e640ba12a8e20d47f3ac23207f380804592cea6eb23c13d4
  contracts/validation-matrix.md: 752e4e95b1c7cda71b9b5924bed3dd84e161669627ae7c28ee333f5f9d94737f
  contracts/integration-contract.md: fa83373df222f40e49ffd890502801805a8cb7c05bcf7d9b89daa0212647dce2
  contracts/permission-audit-matrix.md: d2d3b1cd35785a525ad93fc101cc3a4898288e970cc32efbc9533b1bd72b9ff3
  testing/acceptance-test-matrix.md: 395c8540be5aa56c48ff3eff63092ee4fbbc3d7e65947682871dc2dc2de422e0
  flowcharts/00-alur-utama.md: b5539994a76e184270cd8c1cb4874552f322be05d040d3b13578840366a025de
  flowcharts/rekrutmen-dan-seleksi.md: 63a769add4cad6bee23de31bf4f31c3b9ae3f228b517f7b0168a8f34e6ad6849
active_dependency_ids:
  - HRD-DEP-001
  - HRD-DEP-002
  - HRD-DEP-003
  - HRD-DEP-004
  - HRD-DEP-005
  - HRD-DEP-006
  - HRD-DEP-007
active_roadmap_revision: 6
approvals:
  - scope: S-D2
    approved_by: Pengguna — pemilik teknis dan produk (HRD-DEC-015)
    approved_at: 2026-11-03
    owner_answer: "desain dan roadmap S-D2 disetujui"
    covers:
      - 02-backend-architecture.md bagian 17
      - 03-frontend-architecture.md bagian 13
      - 04-prd-to-mvp.md bagian 22
      - data/data-dictionary.md bagian 9
      - contracts/* bagian S-D2 (kontrak v8)
      - testing/acceptance-test-matrix.md bagian 16
      - flowcharts/rekrutmen-dan-seleksi.md
      - roadmap/* revisi 6 (BE-HRD-018, BE-HRD-052 s.d. BE-HRD-062, FE-HRD-044 s.d. FE-HRD-057)
    not_covered: "bagian slice lain; HRD-DEC-005 dan HRD-DEC-010 milik Komite Medik dan K3RS"
  - scope: S-D1
    approved_by: Pengguna — pemilik teknis dan produk (HRD-DEC-015)
    approved_at: 2026-11-03
    owner_answer: "Yang masih draft semua ya saya setujui"
    covers:
      - 02-backend-architecture.md bagian 16
      - 03-frontend-architecture.md bagian 12
      - 04-prd-to-mvp.md bagian 21
      - data/data-dictionary.md bagian 8
      - contracts/* bagian S-D1 (kontrak v7)
      - testing/acceptance-test-matrix.md bagian 15
      - flowcharts/standar-kebutuhan-tenaga.md, formasi-dan-rencana-tahunan.md, permintaan-headcount.md
    not_covered: "bagian slice lain; HRD-DEC-005 dan HRD-DEC-010 milik Komite Medik dan K3RS"
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
| `READY FOR DESIGN` | Boleh dirancang sampai final | 14 |
| `PARTIAL` | Sebagian boleh final, sebagian tertahan. Batasnya wajib disebut eksplisit | 1 |
| `BLOCKED` | Tidak boleh dirancang sama sekali | 5 |
| `DEFERRED` | Boleh secara teknis, sengaja ditunda karena prioritas | 1 |

Ditambah **1** kelompok `IMPLEMENTED` (`S-A1`). Angka ini dihitung ulang langsung dari tabel
`MODULE-STATUS.md` bagian 2 pada revision `13`. Angka lama 15, 1, 6, 1 tidak cocok dengan isi tabel
itu; sebelum `S-D1` naik, tabel sebenarnya berisi 7 baris `BLOCKED`.

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
ada adalah approval per keputusan, tercatat pada `00-interview-decisions.md`, dan **approval desain
`S-D1`** oleh Pengguna pada 3 November 2026 (blok `approvals` di atas). Approval `S-D1` hanya
mencakup bagian `S-D1` setiap artefak; bagian lain tetap `draft`.

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
| `HRD-DEC-041` | **Jenjang Pendidikan sebagai dimensi kebijakan gaji.** Hanya jenjang **terverifikasi** yang dipakai; kenaikan jenjang **MUST NOT** mengubah gaji secara langsung | `approved` — kontrak sasaran |
| ~~`HRD-DEC-042`~~ | ~~**Masa Kerja sebagai dimensi yang berdiri sendiri**~~ | **`SUPERSEDED FOR CURRENT MVP`** oleh `HRD-DEC-045`. Dipertahankan sebagai sejarah, tidak dihapus |
| `HRD-DEC-045` | **Masa kerja BUKAN faktor kebijakan gaji pada MVP saat ini.** Tidak ada pita masa kerja, ambang, pemicu, maupun perhitungan gaji berbasis masa kerja. Kebijakan masa kerja di masa depan memerlukan keputusan bisnis dan tinjauan kontrak baru | `approved` |
| `HRD-DEC-043` | **Kebijakan gaji berversi, bertanggal berlaku, dapat dikonfigurasi, dan dapat diperluas.** Aturan **MUST NOT** ditulis sebagai percabangan di source. Evaluasi kebijakan **bukan** perhitungan payroll | `approved` — kontrak sasaran |
| `HRD-DEC-044` | **Payroll Officer TIDAK memperoleh `WfpSalaryAssignment : ViewAmount`** pada MVP administratif | `approved` |
| `HRD-DEC-037` | **Kewenangan konfigurasi kebijakan gaji hanya `HR Manager`**; berversi, bertanggal berlaku, dapat diaudit, riwayat tidak dihapus. Faktor gaji **MUST NOT** ditambah tanpa keputusan bisnis | `approved` — kontrak sasaran |
| `HRD-DEC-038` | **Kepemilikan slip gaji diturunkan backend** dan **otentikasi bertingkat** memakai Identity canonical; `SALARY_SENSITIVE_SESSION` bawaan 5 menit | `approved` — kontrak sasaran |
| `HRD-DEC-039` | **`SENSITIVE_GET_MUST_BE_AUDITED`** — pembacaan gaji dicatat; **nominalnya MUST NOT** masuk catatan | `approved` — kontrak sasaran |
| `HRD-DEC-040` | **`Cache-Control: no-store`**, larangan persistensi di sisi klien, unduhan slip gaji lewat endpoint terautentikasi | `approved` — kontrak sasaran |
| `HRD-DEC-032` | **Baseline peran fungsional.** Usulan peta peran menjadi baseline; nama peran fungsional **MUST NOT** disamakan dengan peran Identity. Hanya `SuperAdmin` dan `User` yang di-seed hari ini; seluruh peran HR berstatus `MAPPING_REQUIRED` | **`SECURITY_APPROVED`** |
| `HRD-DEC-033` | **Nominal gaji tersembunyi secara bawaan** pada daftar lintas pegawai. Butir `: ViewAmount` terpisah dari `: Read`. Keterlihatan massal tidak diberikan pada MVP | **`SECURITY_APPROVED`** |
| `HRD-DEC-034` | **Alur persetujuan bawaan per jenis transaksi**, bukan satu alur untuk semua. Override per unit lewat konfigurasi, tanpa pengembangan ulang | `approved` **seluruhnya** — prinsip dan isi rantai `T1` s.d. `T8`; rantai `T9` s.d. `T12` perencanaan tenaga kerja ditambahkan `HRD-DEC-064` |
| `HRD-DEC-059` | **`HRD-Q-05` ditutup hanya untuk sepuluh tabel `WorkforcePlanning` (`S-D1`).** Setiap migration yang mengubah atau membuang tabel itu **MUST** berhenti bila tabelnya berisi data (`EMPTY_TABLE_GUARD`), dan pengaman itu **MUST NOT** dilewati. Bila pengaman berhenti, keputusan kembali `draft` | `approved` |
| `HRD-DEC-072` | **Direktur adalah pemegang jabatan direktur**, ditetapkan lewat matriks persetujuan yang ditelusuri ke unit induk; kosong naik ke Direktur Utama. **MUST NOT** ada kolom kepala unit baru | `approved` |
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
   audit database yang sebenarnya. **Pengecualian sejak revision `13`:** sepuluh tabel
   `WorkforcePlanning` (`S-D1`) boleh diturunkan ulang sesuai `HRD-DEC-059`, **hanya** dengan
   `EMPTY_TABLE_GUARD` pada setiap migration-nya.
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
| `16` | 2026-11-03 | **Perencanaan Delivery `S-D2` (Rekrutmen & Hiring).** `S-D2` dipecah menjadi 12 task backend (`BE-HRD-018` fondasi dan `BE-HRD-052` s.d. `BE-HRD-062`) dan 14 task frontend (`FE-HRD-044` s.d. `FE-HRD-057`); grafik dependency pohon teks ditambahkan pada kedua roadmap. Status `S-D2` pada `00-slice-roadmap.md` naik menjadi `READY`; seluruh rantai requirement, keputusan, dan acceptance test dipetakan pada `requirement-traceability.md` bagian 1b | Approval Pengguna 2026-11-03 |
| `15` | 2026-11-03 | **Desain `S-D2` Rekrutmen dan Hiring.** Menyerap keputusan bisnis `HRD-DEC-073` s.d. `HRD-DEC-080` (decision log rev 18 bagian 33). Arsitektur backend (§17), arsitektur frontend (§13), 12 entitas `Hrd`, 9 grup API Swagger, 13 layar UI, alur proses rekrutmen-dan-seleksi.md, acceptance test §16, contract versions `v8` | Approval Pengguna 2026-11-03 |
| `14` | 2026-11-03 | **Desain `S-D1` Perencanaan Tenaga Kerja sebagai `draft`.** `02-backend-architecture.md` rev `3` bagian 16 (16 tabel `Hrd*`, 15 service, 9 controller, tanpa tabel lama yang diubah atau dibuang); `03-frontend-architecture.md` rev `3` bagian 12 (13 layar `FE-HRD-97` s.d. `FE-HRD-109`, grup menu baru); `data/data-dictionary.md` bagian 8; kelima kontrak bagian `S-D1` (`T9` s.d. `T13`, 33 butir hak akses, 41 aturan `VR-WFP`, `INT-HR-12` s.d. `INT-HR-18`); `testing/acceptance-test-matrix.md` bagian 15 (27 baris `AT-HRD-D1`); `04-prd-to-mvp.md` bagian 21 (`EPIC HRD-11` s.d. `HRD-15`, gelombang `MVP-7` s.d. `MVP-10`); tiga flowchart baru. `contract_versions` naik ke `v7` karena kelima kontrak berubah isi. Decision log menambah `HRD-DEC-072` dan menutup `HRD-TQ-13`. Snapshot SHA disegarkan ke BE `26066c76` / FE `66b7e5600`. **Belum ada approval manusia** | `HRD-DEC-058` s.d. `HRD-DEC-072` |
| `14` — pembaruan hari yang sama, **tanpa naik revision** | 2026-11-03 | **Approval desain `S-D1` dan roadmap revision `5`.** Pengguna menyetujui seluruh bagian `S-D1` yang masih `draft` ("Yang masih draft semua ya saya setujui"); dicatat pada baris status bagian `S-D1` dan baris `approved_by` setiap artefak, ketiga flowchart `S-D1`, dan blok `approvals`. Status dokumen tetap `draft` karena bagian slice lain belum disetujui. `plan-module-delivery` memecah `S-D1` menjadi `BE-HRD-017`, `BE-HRD-029` s.d. `BE-HRD-051`, dan `FE-HRD-029` s.d. `FE-HRD-043`; grafik dependency kedua roadmap ditulis ulang per slice. Dua gap desain dicatat: `HRD-PLAN-GAP-01` (sumber data lowongan untuk `VR-WFP-36`) dan `HRD-PLAN-GAP-02` (kait sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23`). Hash sepuluh artefak yang berubah baris statusnya disegarkan; isi desain dan kontrak tidak berubah, sehingga `contract_versions` tetap `v7` | Approval Pengguna 2026-11-03 |
| `13` | 2026-11-03 | **Gerbang prasyarat `S-D1`.** Menyerap decision log revision `17` bagian 32 — `HRD-DEC-058` s.d. `HRD-DEC-071`, seluruhnya `approved`. `HRD-DEC-059` menutup `HRD-Q-05` untuk sepuluh tabel `WorkforcePlanning`; buktinya pada `01-prerequisite-readiness.md` bagian 4. `S-D1` naik dari `BLOCKED` menjadi `READY FOR DESIGN`; status modul tetap `PARTIAL`. Larangan 3 diberi pengecualian `S-D1`. Hitungan kelompok kemampuan dikoreksi menjadi 13, 1, 6, 1 ditambah 1 `IMPLEMENTED`. `capability_map_revision` dan `capability_map_hash` disinkronkan ke revisi `1.2` yang hash-nya sudah tercatat pada `artifact_hashes`. `contract_versions` **tetap** `v6`: belum ada kontrak `S-D1` yang ditulis. Dua belas artefak canonical lain identik dengan hash revision `12`. **Catatan:** baris riwayat revision `11` dan `12` tidak tercatat pada tabel ini dan tidak direkonstruksi **Pembaruan metadata hari yang sama, tanpa naik revision:** (1) fakta decision log bagian 32 digeser dari `HRD-TF-020`–`030` menjadi `HRD-TF-022`–`032` karena `HRD-TF-020`/`021` sudah dipakai capability map; (2) capability map naik ke revisi `1.3` lewat impact scan bagian 15 pada BE `26066c76` / FE `66b7e5600`. Kedua hash disegarkan | `HRD-DEC-058` s.d. `HRD-DEC-071` |
| `10` | 2026-08-31 | **PHASE 3.4 — Final Salary Scope Correction.** `HRD-DEC-045` menetapkan **masa kerja bukan faktor kebijakan gaji pada MVP saat ini** dan menggantikan `HRD-DEC-042`, yang dipertahankan sebagai sejarah. `HRD-Q-56` menjadi `DEFERRED / NOT_APPLICABLE_TO_CURRENT_MVP`. Faktor aktif menjadi **empat**: Golongan, Level, Status Kerja, Jenjang Pendidikan. `contract_versions` naik ke `v5` karena kriteria kontrak berkurang | `HRD-DEC-045` |
| `9` | 2026-08-31 | **PHASE 3.3 — Salary Policy Clarification.** `HRD-Q-55` ditutup `HRD-DEC-041` dan `HRD-DEC-042`: istilah "masa studi" ditarik menjadi **Jenjang Pendidikan**, dan **Masa Kerja** menjadi dimensi tersendiri. `HRD-DEC-043` menetapkan kebijakan gaji berversi dan dapat dikonfigurasi. `HRD-DEC-044` mencabut `: ViewAmount` dari Payroll Officer. `HRD-Q-56` mencatat tanggal mulai bekerja yang otoritatif belum dapat dicapai dari `MstWorkforceProfile`. `contract_versions` naik ke `v4` | `HRD-DEC-041` s.d. `HRD-DEC-044` |
| `8` | 2026-08-30 | **PHASE 3.2 — Security Decision Ingestion.** `HRD-DEC-032` dan `HRD-DEC-033` menjadi `SECURITY_APPROVED` oleh project final decision authority. Audit pembacaan gaji sensitif disetujui sebagai `APPROVED — AUDIT REQUIRED`. Empat keputusan sasaran baru `HRD-DEC-037` s.d. `HRD-DEC-040` mengatur kewenangan kebijakan gaji, kepemilikan slip gaji, otentikasi bertingkat, audit `GET` sensitif, dan perlindungan sisi klien. `HRD-Q-55` mencatat ketidaksesuaian istilah "masa studi". `contract_versions` naik ke `v3` karena semantik kontrak berubah | `HRD-DEC-037` s.d. `HRD-DEC-040` |
| `7` | 2026-08-30 | **PHASE 3.1 — Workflow Chain Approval.** Isi rantai `T1` s.d. `T8` disetujui pemilik; `HRD-DEC-034` tidak lagi berstatus usulan. `HRD-DEC-036` menutup `HRD-Q-54` dengan **empat definisi alur terpisah**. Paket tinjauan keamanan disiapkan untuk `HRD-DEC-032` dan `HRD-DEC-033`. `contract_versions` **tetap** `v2` — semantik kontrak tidak berubah, hanya dipertegas | `HRD-DEC-036` |
| `6` | 2026-08-30 | **PHASE 3 — Owner Decision Closure.** `HRD-DEC-031` s.d. `HRD-DEC-035` menutup `HRD-Q-19`, `HRD-Q-33`, `HRD-Q-20`, isi konfigurasi workflow, dan `HRD-Q-49` untuk cakupan MVP. `HRD-Q-54` dicatat. `contract_versions` naik ke `v2` karena semantik kontrak berubah — pemisahan peran, butir hak akses sensitif nominal, dan batas payroll. Empat entity penempatan dan remunerasi naik menjadi `Diperbarui`. `FR-HRD-012` yang sebelumnya over-asserted diperbaiki | `HRD-DEC-031` s.d. `HRD-DEC-035` |
| `5` | 2026-08-30 | **PHASE 3 — Design Completion.** Empat artefak canonical yang belum ada ditulis: `data/data-dictionary.md`, `flowcharts/**`, `testing/acceptance-test-matrix.md`, dan `04-prd-to-mvp.md`. `contract_versions` diisi `v1`; `artifact_hashes` diisi. Snapshot SHA disegarkan ke BE `e0ee42c` dan FE `fff76a1b39` dengan hasil `NO_CAPABILITY_IMPACT`. Satu koreksi berbasis bukti pada bagian 7.1 `02-backend-architecture.md`. Cleanup governance referensi `erd/` mengikuti `blueprint-output-contract` terbaru |

Perubahan setelah approval membuat revision baru dan memicu impact scan pada kedua repository.
Blueprint ini tidak pernah ditandai `approved` oleh skill; approval tetap tindakan manusia.
