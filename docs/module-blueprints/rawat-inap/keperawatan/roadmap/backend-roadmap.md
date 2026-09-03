# Roadmap Delivery Backend — Sub-modul Keperawatan Rawat Inap

> ## ⚠ ROADMAP INI **`STALE`** SEJAK 2026-09-02 SORE
>
> `RWI-DEC-091` terbit **setelah** roadmap ini ditulis, dan desainnya sudah diserap
> `keperawatan/` revision `0.3`. Empat task di bawah **berubah bentuk** dan wajib ditulis ulang
> `/qv-plan` sebelum dipakai:
>
> | Task | Yang berubah |
> | --- | --- |
> | `BE-RWI-057` | Bukan lagi amandemen berversi. Koreksi = **addendum** pada mesin keutuhan `MedicalRecordManagement`, jenis `Assessment`. Status pengkajian **tetap** `Completed` |
> | `BE-RWI-062` | Koreksi catatan tindakan = **addendum**, jenis `Procedure`. Status tetap `Finalized` |
> | `BE-RWI-054` | Dua kolom **dicabut**: `AmendedAt` dan `AmendedByUserId`. Menjadi **empat** kolom, bukan enam |
> | **Task baru dibutuhkan** | Pendaftaran dokumen ke mesin keutuhan saat finalisasi, dan permintaan perluasan penegakan `RWI-OQ-051` |
>
> Dependency baru: **`RWI-OQ-051`** memblokir implementasi `BE-RWI-057` dan `BE-RWI-062`.
> Selama `Assessment` dan `Procedure` belum masuk daftar jenis yang ditegakkan `MedicalRecordManagement`,
> dokumen final **tidak akan terkunci** walaupun sudah terdaftar — lihat `contracts/integration-contract.md` `INT-KEP-06`.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
blueprint_id: RWI-BP-001
blueprint_revision: 5
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_revision: 1
status: DRAFT_STALE
roadmap_mode: FORWARD_TEST
approval_gate: BLUEPRINT_NOT_YET_APPROVED
approved_by: null
approved_at: null
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061), jabatan formal belum diisi"
  - "Pemilik ClinicalManagement: Muhammad Hamzah (RWI-DEC-062)"
  - "Clinical governance: sebagian terisi (RWI-DEC-064); pemilik batas waktu klinis belum ditunjuk"
  - "Security/Privacy: OPEN"
contract_versions: 0.2.0
input_revisions:
  blueprint-manifest.md (tingkat modul): 5
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 11
  01-existing-capability-map.md: 1.3
  02-module-map.md: 1
  evidence/02-requirement-completeness-gate.md: 1.4
  02-backend-architecture.md: 0.2
  03-frontend-architecture.md: 0.1
  04-prd-to-mvp.md: 0.2
input_hashes:
  00-interview-decisions.md: f34b7aef1352d4c5a817ffeaf988c6eed514d668d3d92051b78806bfc09e635c
  01-existing-capability-map.md: 0155b345abea61f1b69e6adaf48ee91056b5efaf7fa672ea6300e0546bf4db03
  02-module-map.md: 29c761eed6a3fdc3a4d76c2803fde6e956a19784c4b3a14fc27d30e81e5a5d08
  evidence/02-requirement-completeness-gate.md: 73203cfb7c78077a2c3d9d1c6c5f1cb0e1bb60a0a09b0e5e3a2c4e7b6c8d9f01
  02-backend-architecture.md: 64079c0e164300958ab1c6c8f0c0e0a6dbb4f7d4b1c6d0f7bb17e6e5c3a2b1d0
backend_source_sha: 93b3227c431401d8f586dec4e1fb25fbf41766e3
frontend_source_sha: 8d6e0998c16d60e19a9f43949758e8895d5c47d0
capability_scope:
  - CAP-012 Nursing Assessment
  - CAP-013 Nursing Care
  - CAP-014 Nursing Interventions
  - CAP-027 Nutrition Care (hanya skrining dan rujukan)
capability_excluded:
  - CAP-016 Equipment Usage — DEFERRED oleh RWI-DEC-089, nol task
requirement_readiness: INP-S16 PARTIALLY_READY; empat kemampuan aktif READY_FOR_DOMAIN_DESIGN
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
task_id_range: BE-RWI-054 .. BE-RWI-064
```

> **Catatan hash.** Nilai `input_hashes` untuk `evidence/02-requirement-completeness-gate.md` dan
> `02-backend-architecture.md` dipotong pada dokumen ini demi keterbacaan. Nilai penuh yang
> mengikat ada pada `blueprint-manifest.md` masing-masing tingkat, dan itulah yang dipakai saat
> pemeriksaan drift.

---

## 0. Peringatan yang tidak boleh dilewati

### 0.1 Roadmap ini **belum boleh dieksekusi siapa pun**

| Hal | Keadaannya |
| --- | --- |
| Status blueprint sub-modul | **`draft`** — `keperawatan/blueprint-manifest.md` menyatakan "belum disetujui manusia" |
| `approved_by` / `approved_at` | **Kosong.** Tidak ada keputusan approval seperti `RWI-DEC-067` yang dipakai `episode-rawat-inap` |
| Akibatnya | **Tidak satu pun task di bawah ini boleh dikirim ke `/qv-be`.** Seluruhnya berstatus `BLOCKED` pada gerbang yang sama |
| Kenapa roadmap ini tetap ditulis | Aturan skill perencanaan mengizinkan roadmap `DRAFT`/`FORWARD_TEST` memuat task `BLOCKED`, asalkan penghalangnya disebut. `BLOCKED` **bukan** cara melewati approval |
| Cara mencabutnya | Pemilik menyetujui blueprint `keperawatan` lewat satu keputusan bernomor pada `00-interview-decisions.md`, lalu `status` roadmap ini dinaikkan menjadi `APPROVED` beserta `approved_by` dan `approved_at` |

### 0.2 Sub-modul ini **tidak memiliki satu tabel pun**

`RWI-DEC-081` dan `PRD-RWI-FINAL-001` bagian 23.1 menaruh **seluruh** tabel dokumentasi klinis
rawat inap pada **`ClinicalManagement`**. Konsekuensinya berat dan sering disalahpahami:

| Yang sering disangka | Yang sebenarnya |
| --- | --- |
| "Task backend keperawatan menulis di `Areas/HealthServices/InPatientManagement/`" | **Salah.** Seluruh task di bawah ini menulis di `Areas/HealthServices/ClinicalManagement/` |
| "Rawat Inap membuat tabel pengkajian dan asuhan sendiri" | **Dilarang keras.** Membuat `InpNursingAssessment` atau tabel `Inp*` apa pun untuk dokumentasi klinis melanggar `RWI-DEC-081` |
| "Ini pekerjaan satu tim saja" | Ini **permintaan perubahan kepada modul tetangga**. Persetujuan arahnya sudah ada lewat `RWI-DEC-062`, tetapi penjadwalannya milik pemilik `ClinicalManagement` |

Sub-modul ini menyediakan **ruang kerja, konteks episode, dan kontrak** — bukan mesin
penyimpanannya.

### 0.3 Nol task untuk `EPIC KEP-06`

`RWI-DEC-089` mengeluarkan pemakaian alat (`CAP-016`) dari scope rilis pertama secara tertulis.
Roadmap ini memuat **nol** task untuk `FR-KEP-027` dan `FR-KEP-028`, dan itu **disengaja** —
`RWI-AC-170` menjadikannya kriteria yang dapat diuji. Membuat task pemakaian alat di sini berarti
melanggar keputusan yang baru saja diambil pemiliknya.

### 0.4 Angka batas waktu klinis memang boleh kosong

`RWI-RULE-021` belum final karena pemilik klinis belum ditunjuk. Itu **tidak** menahan pekerjaan:
`PRD-RWI-FINAL-001` bagian 16.2 aturan 11 justru mewajibkan SLA klinis menjadi **konfigurasi** dan
melarang angkanya ditanam di kode. Master kebijakan yang kosong berarti **tidak ada satu pun
episode yang dinyatakan terlambat** — bukan kesalahan, melainkan perilaku yang dirancang
(`FR-KEP-011`).

### 0.5 Kesesuaian engineering diselesaikan saat eksekusi

Preflight QBE dan kesesuaian kontrak rekayasa backend **tidak** diputuskan di roadmap ini. Keduanya
diselesaikan pada waktu eksekusi dari `AGENTS.md` repository backend target beserta dokumen
engineering canonical yang berlaku saat itu.

---

## 1. Cara membaca roadmap ini

| Lambang | Arti |
| --- | --- |
| ⛔ | `BLOCKED` — ada penghalang yang disebut namanya pada baris **Dependency** atau **Risk/blocker** |
| ⬜ | Belum dikerjakan, dan tidak terhalang apa pun selain gerbang approval |

Hari ini **seluruh** task bertanda ⛔ karena gerbang approval pada bagian 0.1. Tanda itu dicabut
per task begitu penghalang khususnya hilang.

Setiap task menyebut jejak requirement ke `FR-KEP-0xx` pada
[`../04-prd-to-mvp.md`](../04-prd-to-mvp.md) bagian 9, dan jejak keputusan ke decision ID pada
[`../../00-interview-decisions.md`](../../00-interview-decisions.md).

---

## 2. Keadaan awal yang menentukan urutan

| Temuan | Sumber | Akibatnya pada urutan |
| --- | --- | --- |
| `TrxPatientAssessment` masih **mewajibkan** `QueueId` | `01-existing-capability-map.md` baris 282; `BE@5afb54b .../Models/TrxPatientAssessment.cs:21-24` | `BE-RWI-054` **wajib** paling dulu. Tanpa itu tidak satu pun pengkajian rawat inap dapat dibuat |
| Tabel asuhan dan tindakan **belum ada** | `02-backend-architecture.md` bagian 4.2 dan 4.3 | `BE-RWI-059` dan `BE-RWI-061` membawa tabel barunya |
| Master kebijakan batas waktu belum ada | `02-backend-architecture.md` bagian 8 | `BE-RWI-055` mendahului seluruh perhitungan tenggat |
| CPPT sudah ada dan sudah menerima banyak profesi | `02-backend-architecture.md` bagian 4.4 | `BE-RWI-063` murni pemakaian ulang, **nol tabel baru** |
| `episode-rawat-inap` `M1` sudah selesai | Roadmap sub-modul itu, revision `3` | Prasyarat gelombang `KEP-MVP-0` sudah terpenuhi |

---

## 3. Gelombang dan urutan dependency

| Gelombang | Task | Isinya | Prasyarat |
| --- | --- | --- | --- |
| **`KEP-MVP-0`** | `BE-RWI-054`, `BE-RWI-055` | Pintu masuk pengkajian rawat inap dan master kebijakan | `episode-rawat-inap` `M1` selesai |
| **`KEP-MVP-1`** | `BE-RWI-056` s.d. `BE-RWI-058` | `EPIC KEP-01` dan `EPIC KEP-02` | `KEP-MVP-0` |
| **`KEP-MVP-2`** | `BE-RWI-059`, `BE-RWI-060` | `EPIC KEP-03` rencana asuhan | `KEP-MVP-1` |
| **`KEP-MVP-3`** | `BE-RWI-061` s.d. `BE-RWI-063` | `EPIC KEP-04` tindakan dan CPPT | `KEP-MVP-1` |
| **`KEP-MVP-4`** | `BE-RWI-064` | `EPIC KEP-05` daftar pantau kepatuhan | `KEP-MVP-1` |
| **Tidak masuk gelombang** | — | `EPIC KEP-06` pemakaian alat | `DEFERRED` oleh `RWI-DEC-089` |

```text
BE-RWI-054 ─┬─> BE-RWI-056 ─> BE-RWI-057 ─> BE-RWI-058 ─> BE-RWI-064
            │        │
BE-RWI-055 ─┘        ├─> BE-RWI-059 ─> BE-RWI-060
                     └─> BE-RWI-061 ─> BE-RWI-062
                                  └──> BE-RWI-063
```

`BE-RWI-059`, `BE-RWI-061`, dan `BE-RWI-063` dapat berjalan **paralel** setelah `BE-RWI-056`
selesai, karena ketiganya menyentuh tabel yang berbeda dan tidak berbagi kontrak.

---

## 4. Task

### ⛔ `BE-RWI-054` — Perawat dapat membuat pengkajian rawat inap tanpa nomor antrean

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1, **dan** `INT-KEP-01` adalah permintaan perubahan kepada `ClinicalManagement` |
| **Outcome** | Perawat membuka pengkajian bagi pasien rawat inap langsung dari episodenya. Hari ini hal itu mustahil: sistem menuntut nomor antrean rawat jalan yang tidak pernah dimiliki pasien rawat inap |
| **Trace** | `FR-KEP-001` s.d. `FR-KEP-004`; `RWI-DEC-062`, `RWI-DEC-081`, `RWI-RULE-026`; `PRD-RWI-FINAL-001` 16.2 aturan 1, 2, 4; `AC-CAP012-01`; `INT-KEP-01` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Patient Assessment; `contracts/integration-contract.md` `0.2.0` `INT-KEP-01`, `INT-KEP-02` |
| **Reuse** | `TrxPatientAssessment` beserta controller dan service-nya yang **sudah ada**. Tidak ada tabel baru, tidak ada controller baru |
| **Scope** | `Areas/HealthServices/ClinicalManagement/`: enam kolom nullable pada `TrxPatientAssessment` (`InpEpisodeId`, `AssessmentType`, `DueAt`, `PolicyId`, `AmendedAt`, `AmendedByUserId`); pelonggaran validasi `QueueId`; satu enum `AssessmentType`; satu migration; penyesuaian DTO dan configuration EF |
| **Dependency** | `episode-rawat-inap` `M1` (selesai). Penjadwalan milik pemilik `ClinicalManagement` |
| **Acceptance criteria** | 1. Pengkajian dapat dibuat untuk encounter yang punya episode `Admitted` **tanpa** `QueueId` dan tanpa kunjungan IGD aktif. 2. Pengkajian ditolak `422` bila episode tidak ada, masih `Draft`, atau sudah `Closed`. 3. `InpEpisodeId` tersimpan dan pengkajian terbaca per episode. 4. **Perilaku pengkajian poliklinik dan MCU tidak berubah sedikit pun**, dibuktikan test regresi |
| **Verification** | Test regresi jalur poliklinik dan IGD; uji migration maju dan mundur; pemeriksaan bentuk kolom terhadap `data/data-dictionary.md` |
| **Risk/blocker** | **Risiko tertinggi seluruh sub-modul.** Melonggarkan validasi milik modul tetangga dapat memecah jalur rawat jalan dan IGD yang sudah berjalan. `RWI-DEC-051` mencatat belum ada satu pun test yang menjaga jalur itu — test regresinya **bagian dari task ini**, bukan pekerjaan menyusul. Owner: pemilik `ClinicalManagement` |
| **DoD** | Enam kolom, satu enum, satu migration, validasi longgar, test regresi poliklinik dan IGD lulus, build lulus, laporan menyatakan migration belum diterapkan ke database bersama |

---

### ⛔ `BE-RWI-055` — Batas waktu pengkajian dibaca dari master, bukan ditanam di kode

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Clinical governance dapat mengatur sendiri batas waktu pengkajian awal dan pengkajian ulang tanpa menyentuh kode. Selama masternya kosong, tidak ada yang dinyatakan terlambat |
| **Trace** | `FR-KEP-010`, `FR-KEP-011`; `RWI-RULE-021` (**belum final, dan memang tidak perlu final**); `PRD-RWI-FINAL-001` 16.2 aturan 11; `AC-CAP012-04` |
| **Kontrak** | `contracts/validation-matrix.md` `0.2.0`; `contracts/api-contract.md` `0.2.0` |
| **Reuse** | Pola `MstInpatientSetting` dari `BE-RWI-001`: bentuk kolom audit, soft delete, dan konfigurasi EF |
| **Scope** | `MstClinicalAssessmentPolicy` milik `ClinicalManagement`: batas waktu per jenis pengkajian per jenis pelayanan, **berversi**; satu configuration EF; satu `DbSet`; satu migration; endpoint pengelolaan master |
| **Dependency** | — dapat berjalan paralel dengan `BE-RWI-054` |
| **Acceptance criteria** | 1. Master menyimpan batas waktu per jenis pengkajian per jenis pelayanan. 2. Kebijakan **berversi**, sehingga penilaian keterlambatan memakai kebijakan yang **aktif saat pengkajian dibuat**, bukan yang berlaku sekarang. 3. Master kosong **tidak** menahan pencatatan pengkajian dan **tidak** menghasilkan satu pun penanda terlambat |
| **Verification** | Uji dengan master kosong, master terisi, dan kebijakan yang berubah di tengah episode |
| **Risk/blocker** | Kebijakan yang tidak berversi akan membuat episode lama tiba-tiba terlihat terlambat ketika angkanya diubah. Owner: Backend/API bersama Clinical governance |
| **DoD** | Satu tabel master, configuration, `DbSet`, migration, endpoint pengelolaan; uji tiga keadaan master lulus; build lulus |

---

### ⛔ `BE-RWI-056` — Pengkajian awal dan pengkajian ulang tidak lagi saling menimpa

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Pengkajian ulang harian tersimpan sebagai catatan tersendiri. Nilai pengkajian awal tetap utuh dan dapat dibaca kembali kapan pun |
| **Trace** | `FR-KEP-005`, `FR-KEP-006`; `PRD-RWI-FINAL-001` 16.2 aturan 3; `AC-CAP012-02` |
| **Kontrak** | `contracts/state-transition-matrix.md` `0.2.0` bagian 1; `contracts/validation-matrix.md` `0.2.0` |
| **Reuse** | `TrxPatientAssessment` beserta kolom `AssessmentType` dari `BE-RWI-054` |
| **Scope** | Aturan pembuatan pengkajian pada service `ClinicalManagement`; penolakan pengkajian awal kedua; satu unique index parsial per episode untuk jenis awal |
| **Dependency** | `BE-RWI-054` |
| **Acceptance criteria** | 1. Pengkajian awal dan ulang tersimpan sebagai record terpisah. 2. Pengkajian awal **kedua** pada satu episode ditolak, dan pesannya mengarahkan perawat membuat pengkajian ulang. 3. Nilai pengkajian awal tidak berubah sedikit pun setelah pengkajian ulang dibuat |
| **Verification** | Skenario dua pengkajian berurutan; pemeriksaan unique index parsial |
| **Risk/blocker** | Unique index tanpa penyaring status akan ikut menghitung pengkajian yang dibatalkan. Owner: Backend/API |
| **DoD** | Aturan pembuatan, satu unique index parsial, satu migration, uji dua skenario lulus, build lulus |

---

### ⛔ `BE-RWI-057` — Pengkajian final dapat dibetulkan tanpa kehilangan versi lamanya

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat yang salah mengisi dapat membetulkannya lewat amandemen beralasan. Isi lama tetap tersimpan sebagai bukti klinis, dan tidak ada jalan menghapusnya diam-diam |
| **Trace** | `FR-KEP-008`, `FR-KEP-009`; `PRD-RWI-FINAL-001` 16.2 aturan 12 dan 13, 27.3 aturan 7; `AC-CAP012-05` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` `PATCH /{id}/amend`; `contracts/state-transition-matrix.md` `0.2.0` bagian 1; `contracts/permission-audit-matrix.md` `0.1.0` |
| **Reuse** | Pola amandemen yang sudah dipakai dokumen klinis lain pada `ClinicalManagement` |
| **Scope** | Endpoint `PATCH /{id}/amend`; transisi `Completed → Amended` dan `Amended → Amended`; penyimpanan versi sebelumnya; hak akses `PatientAssessment : Amend`; larangan hard-delete |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Amandemen menyimpan aktor, waktu, alasan, dan isi sebelumnya. 2. Alasan kosong ditolak `400`. 3. Pengkajian final **tidak dapat** dihapus maupun ditimpa diam-diam. 4. Transisi `Completed → Draft` ditolak |
| **Verification** | Skenario amandemen berulang; percobaan hard-delete; percobaan membuka kembali pengkajian final |
| **Risk/blocker** | **Butir konsistensi terbuka:** apakah dokumen keperawatan ikut mendaftar ke mesin keutuhan dokumen `ClinicalManagement` seperti dokumen dokter. `04-prd-to-mvp.md` bagian 20.1. PRD 27.3 aturan 7 mengizinkan aturan per jenis dokumen, sehingga ini **tidak memblokir**, tetapi jawabannya sebaiknya turun sebelum task ini dikerjakan agar tidak dibongkar. Owner: Muhammad Hamzah |
| **DoD** | Satu endpoint, dua transisi status, penyimpanan versi, hak akses, uji empat skenario lulus, build lulus |

---

### ⛔ `BE-RWI-058` — Perkembangan nyeri, risiko jatuh, dan gizi terbaca sebagai satu garis waktu

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat dan DPJP melihat apakah nyeri pasien membaik atau memburuk, bukan hanya nilai terakhirnya. Keadaan tenggat pengkajian terbaca dari kebijakan yang aktif |
| **Trace** | `FR-KEP-007`, `FR-KEP-010`; `PRD-RWI-FINAL-001` 16.2 aturan 6 dan 11; `AC-CAP012-02`, `AC-CAP012-04` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` `GET /episodes/{episodeId}/timeline` dan `GET /episodes/{episodeId}/due-status` |
| **Reuse** | Data pengkajian yang sudah tersimpan; **nol tabel baru** |
| **Scope** | Dua endpoint baca; proyeksi lini masa per jenis pengukuran; perhitungan `DueAt`, `CompletedAt`, dan keadaan terlambat menurut kebijakan aktif saat pengkajian dibuat |
| **Dependency** | `BE-RWI-055`, `BE-RWI-056` |
| **Acceptance criteria** | 1. Lini masa menampilkan seluruh pengukuran terurut waktu, bukan hanya yang terakhir. 2. Nilai lama tidak pernah ditimpa. 3. Keadaan tenggat dihitung dari kebijakan yang aktif **saat pengkajian dibuat**. 4. Master kebijakan kosong menghasilkan keadaan "tidak dipantau", bukan "terlambat" |
| **Verification** | Skenario tiga pengukuran nyeri berurutan; skenario master kosong; skenario kebijakan berubah di tengah episode |
| **Risk/blocker** | Perhitungan tenggat yang memakai kebijakan terbaru akan membuat episode lama salah dinilai. Owner: Backend/API |
| **DoD** | Dua endpoint, proyeksi lini masa, perhitungan tenggat berversi, uji empat skenario lulus, build lulus |

---

### ⛔ `BE-RWI-059` — Rencana asuhan keperawatan punya tempat menyimpan masalah dan tujuannya

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat menetapkan masalah keperawatan pasien beserta tujuan dan rencana tindakannya di dalam sistem, bukan di kertas |
| **Trace** | `FR-KEP-012`, `FR-KEP-013`, `FR-KEP-015`; `PRD-RWI-FINAL-001` `CAP-013` aturan 1, 2, 4; `AC-CAP013-01`; `RWI-DEC-083` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Nursing Care Plan; `contracts/state-transition-matrix.md` `0.2.0` bagian 2 |
| **Reuse** | Pola aggregate induk-anak yang sudah dipakai `InpBedPlacement` pada `episode-rawat-inap` |
| **Scope** | `TrxNursingCarePlan` dan `TrxNursingCarePlanItem` milik `ClinicalManagement`; dua configuration EF; dua `DbSet`; satu migration; endpoint buat rencana, tambah butir, baca per episode, catat evaluasi |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Satu episode memiliki **tepat satu** rencana asuhan, dengan butir masalah sebanyak yang dibutuhkan. 2. Butir memuat masalah, tujuan, rencana tindakan, dan evaluasi. 3. Butir dapat dinyatakan tercapai **hanya** bila evaluasinya sudah ada. 4. Butir dapat dikaitkan ke temuan pengkajian |
| **Verification** | Uji migration maju-mundur; skenario penambahan butir; percobaan menutup butir tanpa evaluasi |
| **Risk/blocker** | Katalog terminologi SDKI/SLKI/SIKI **belum diputuskan** dipakai atau tidak (`PROPOSED`/`CONFIGURABLE_DEFAULT` pada gerbang `1.4`). Struktur rencana asuhan **tidak** bergantung padanya; masalah keperawatan ditulis sebagai teks sampai katalognya diputuskan. Owner: Clinical governance |
| **DoD** | Dua tabel, dua configuration, dua `DbSet`, satu migration, empat endpoint, uji tiga skenario lulus, build lulus |

---

### ⛔ `BE-RWI-060` — Perubahan rencana asuhan menyimpan versi sebelumnya

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Riwayat asuhan pasien tetap utuh. Menutup satu masalah keperawatan tidak menghapus jejak tindakan dan evaluasi yang sudah dikerjakan |
| **Trace** | `FR-KEP-014`, `FR-KEP-016`, `FR-KEP-017`; `PRD-RWI-FINAL-001` `CAP-013` aturan 5 dan 6; `AC-CAP013-02`, `AC-CAP013-03` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` `PUT /items/{itemId}`, `PATCH /items/{itemId}/close`, `GET /items/{itemId}/revisions`; `contracts/state-transition-matrix.md` `0.2.0` bagian 2 |
| **Reuse** | Pola salinan versi yang dipakai `BE-RWI-057` |
| **Scope** | `TrxNursingCarePlanItemRevision` milik `ClinicalManagement`; satu configuration; satu `DbSet`; satu migration; tiga endpoint; penjaga hanya-baca setelah episode `Closed` |
| **Dependency** | `BE-RWI-059` |
| **Acceptance criteria** | 1. Memperbarui butir menyimpan versi sebelumnya **beserta penulis dan waktu aslinya**, bukan penulis yang mengubah. 2. Menutup butir tidak menghapus tindakan maupun evaluasi sebelumnya. 3. Setelah episode `Closed`, seluruh riwayat asuhan tetap terbaca dan **tidak dapat diubah** |
| **Verification** | Skenario perubahan butir berulang; pemeriksaan penulis versi lama; percobaan mengubah asuhan pada episode tertutup |
| **Risk/blocker** | Menyalin versi dengan penulis pengubah — bukan penulis asli — akan merusak makna klinisnya. `AC-CAP013-02` menguji tepat hal itu. Owner: Backend/API |
| **DoD** | Satu tabel, configuration, `DbSet`, migration, tiga endpoint, penjaga episode tertutup, uji tiga skenario lulus, build lulus |

---

### ⛔ `BE-RWI-061` — Tindakan keperawatan tercatat sekali walaupun tombolnya tertekan dua kali

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Perawat mencatat tindakan yang benar-benar dilakukan beserta waktu dan hasilnya. Jaringan yang buruk tidak lagi menghasilkan tindakan ganda pada rekam medis |
| **Trace** | `FR-KEP-018`, `FR-KEP-019`, `FR-KEP-020`; `PRD-RWI-FINAL-001` `CAP-014` aturan 1, 2, 3; `AC-CAP014-01` |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` grup Nursing Intervention; `contracts/state-transition-matrix.md` `0.2.0` bagian 3 |
| **Reuse** | Pola `Idempotency-Key` yang sudah dipakai pada task `episode-rawat-inap` |
| **Scope** | `TrxNursingIntervention` milik `ClinicalManagement`; satu configuration; satu `DbSet`; satu migration; endpoint catat dan daftar per episode; penjaga idempotency |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Tindakan menyimpan apa, kapan, oleh siapa, dan hasilnya, beserta konteks episode. 2. Tindakan mendadak dapat dicatat **tanpa** rujukan ke rencana asuhan. 3. Permintaan berulang dengan `Idempotency-Key` yang sama menghasilkan **satu** baris, bukan dua |
| **Verification** | Skenario kirim ulang dengan kunci sama dan kunci berbeda; skenario tindakan tanpa rencana asuhan |
| **Risk/blocker** | Idempotency yang dipasang di controller saja akan bocor ketika ada dua instance. Penjaganya wajib di tingkat database. Owner: Backend/API |
| **DoD** | Satu tabel, configuration, `DbSet`, migration, dua endpoint, penjaga idempotency di database, uji tiga skenario lulus, build lulus |

---

### ⛔ `BE-RWI-062` — Tagihan yang gagal tidak menghapus catatan klinis

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Ketika pengiriman tagihan ke Billing gagal, tindakan keperawatannya **tetap tersimpan**. Kegagalan itu terlihat sebagai keadaan integrasi tersendiri yang dapat dicoba ulang |
| **Trace** | `FR-KEP-021`, `FR-KEP-022`; `PRD-RWI-FINAL-001` `CAP-014` aturan 5; `AC-CAP014-02`, `AC-CAP014-03`; `INT-KEP-05` |
| **Kontrak** | `contracts/integration-contract.md` `0.2.0` `INT-KEP-05`; `contracts/api-contract.md` `0.2.0` `PATCH /{id}/finalize`, `PATCH /{id}/amend`, `GET /{id}/billing-dispatch`; `contracts/permission-audit-matrix.md` `0.1.0` |
| **Reuse** | Pola pemisahan kegagalan integrasi yang sudah dipakai modul lain |
| **Scope** | Transisi `Recorded → Finalized → Amended`; keadaan pengiriman tagihan `Pending`/`Sent`/`Failed` yang **terpisah** dari status klinis; tiga endpoint; hak akses `NursingIntervention : Amend` |
| **Dependency** | `BE-RWI-061` |
| **Acceptance criteria** | 1. Catatan klinis tetap tersimpan ketika Billing gagal, dan keadaan pengirimannya `Failed`. 2. Status klinis tetap `Recorded`/`Finalized` apa pun keadaan tagihannya. 3. Catatan final hanya dapat diamandemen penulisnya atau kepala ruangan; selain itu ditolak `403`. 4. Setiap amandemen tercatat beserta alasannya |
| **Verification** | Skenario Billing gagal; skenario amandemen oleh bukan penulis; pemeriksaan bahwa dua mesin status tidak saling mengunci |
| **Risk/blocker** | Menggabungkan status klinis dengan status tagihan adalah kesalahan yang paling mahal di sini — catatan klinis bisa hilang karena masalah keuangan. Owner: Backend/API |
| **DoD** | Tiga transisi status, mesin pengiriman tagihan terpisah, tiga endpoint, hak akses, uji empat skenario lulus, build lulus |

---

### ⛔ `BE-RWI-063` — Catatan keperawatan tampil pada catatan terpadu tanpa tabel baru

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Dokter membaca catatan perawat pada catatan terpadu yang sama, sehingga seluruh profesi melihat satu perkembangan pasien |
| **Trace** | `FR-KEP-023`; `PRD-RWI-FINAL-001` `CAP-014` aturan 4; `INT-KEP-03`; `RWI-RULE-026` |
| **Kontrak** | `contracts/integration-contract.md` `0.2.0` `INT-KEP-03`; `contracts/api-contract.md` `0.2.0` grup Patient Integrated Progress Note |
| **Reuse** | `TrxPatientIntegratedProgressNote` yang **sudah ada** dan sudah menerima banyak profesi. **Nol tabel baru, nol kolom baru** |
| **Scope** | Pemakaian `ProfessionType` perawat pada CPPT; penyaluran catatan keperawatan ke CPPT sesuai kebijakan |
| **Dependency** | `BE-RWI-061` |
| **Acceptance criteria** | 1. Catatan keperawatan tampil pada CPPT dengan `ProfessionType` perawat. 2. **Nol tabel dan nol kolom baru** dibuat task ini. 3. Catatan dokter yang sudah ada pada CPPT tidak berubah perilakunya |
| **Verification** | Pemeriksaan bahwa migration task ini kosong; skenario CPPT berisi catatan dua profesi |
| **Risk/blocker** | `PRD-RWI-FINAL-001` `CAP-014` aturan 4 menyebut "sesuai policy" tanpa menyebut kebijakannya — pertanyaan 4 pada `04-prd-to-mvp.md` bagian 20, **tidak memblokir** karena catatan tetap tersimpan dan terbaca dari ruang kerja. Owner: Clinical governance |
| **DoD** | Nol tabel baru, penyaluran CPPT berjalan, uji dua skenario lulus, build lulus |

---

### ⛔ `BE-RWI-064` — Kepala ruangan melihat pengkajian mana yang belum dikerjakan

| Field | Isi |
| --- | --- |
| **Status** | ⛔ `BLOCKED` — gerbang approval bagian 0.1 |
| **Outcome** | Kepala ruangan menemukan episode yang pengkajian awalnya belum ada atau sudah lewat tenggat, tanpa membuka satu per satu |
| **Trace** | `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; `PRD-RWI-FINAL-001` 16.2 aturan 11 |
| **Kontrak** | `contracts/api-contract.md` `0.2.0` endpoint daftar pantau kepatuhan |
| **Reuse** | Pola daftar pantau yang sudah dipakai `FE-INP-09` pada `episode-rawat-inap`. **Ini daftar pantau ketiga** yang selama ini tercatat sebagai gap |
| **Scope** | Satu endpoint baca berhalaman; penyaringan menurut ruangan dan keadaan tenggat; **nol tabel baru** |
| **Dependency** | `BE-RWI-058` |
| **Acceptance criteria** | 1. Daftar memuat episode yang pengkajian awalnya belum ada atau terlambat menurut kebijakan aktif. 2. Daftar kosong berbunyi "sudah tepat waktu", **bukan** "tidak ada data". 3. Keterlambatan pengkajian **tidak menahan** satu pun tindakan lain |
| **Verification** | Skenario daftar kosong; skenario episode terlambat; pemeriksaan bahwa keterlambatan tidak memblokir pencatatan tindakan |
| **Risk/blocker** | Daftar pantau yang memblokir pekerjaan klinis akan mendorong perawat mengakali sistem. `FR-KEP-026` menguji tepat hal itu. Owner: Backend/API |
| **DoD** | Satu endpoint, penyaringan, nol tabel baru, uji tiga skenario lulus, build lulus |

---

## 5. Coverage gap requirement ke test

| Requirement | Tercakup task | Tercakup acceptance test | Catatan |
| --- | --- | --- | --- |
| `FR-KEP-001` s.d. `FR-KEP-026` | Ya, 11 task | Ya | Seluruh 26 functional requirement aktif punya task pemilik |
| `FR-KEP-027`, `FR-KEP-028` | **Tidak, disengaja** | Tidak | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089`; ketiadaan task adalah kriteria `RWI-AC-170` |
| `AC-CAP027-01` rujukan gizi | Sebagian | Sebagian | Skrining gizi ikut `BE-RWI-056`; pemicu rujukan `INT-KEP-04` menunggu modul Gizi berdiri |
| `AC-CAP027-02` kewenangan ahli gizi | **Tidak** | Tidak | Modul Gizi `PLANNED`; di luar kendali sub-modul ini |

**Coverage gap yang diakui:** dua acceptance criteria `CAP-027` tidak dapat diuji sampai modul
Gizi berdiri. Ini ketersediaan modul, bukan requirement yang hilang.

---

## 6. Yang sengaja tidak dibuat roadmap ini

| Yang ditolak | Alasan |
| --- | --- |
| Task pembuatan tabel `Inp*` untuk dokumentasi klinis | `RWI-DEC-081` menaruhnya pada `ClinicalManagement`. Membuat tandingan melanggar keputusan pemilik |
| Task `EPIC KEP-06` pemakaian alat | `RWI-DEC-089` mengeluarkannya dari scope rilis pertama |
| Task katalog SDKI/SLKI/SIKI | Pemakaiannya belum dinyatakan rumah sakit; PRD `CAP-013` aturan 3 bersyarat |
| Task pengisian angka batas waktu klinis | Angkanya konfigurasi milik Clinical governance, bukan pekerjaan rekayasa |
| Task asuhan gizi ujung ke ujung | Modul Gizi belum berwujud; PRD 23.1 menaruhnya di sana |
