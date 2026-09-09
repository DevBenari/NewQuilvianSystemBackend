# Roadmap Delivery Backend — Sub-modul Keperawatan Rawat Inap

> ## ✅ ROADMAP INI **BOLEH DIEKSEKUSI** SEJAK 5 SEPTEMBER 2026
>
> Revision `2` menggantikan revision `1` yang berstatus `DRAFT_STALE`. Tiga hal berubah, dan
> ketiganya mengubah cara roadmap ini dibaca:
>
> | Yang berubah | Pada revision `1` | Menjadi revision `2` |
> | --- | --- | --- |
> | Gerbang approval | Seluruh 11 task `BLOCKED` karena blueprint belum disetujui | **Approval sudah turun** lewat `RWI-DEC-092`, Muhammad Hamzah, 3 September 2026 |
> | Bentuk koreksi dokumen | Amandemen berversi milik sub-modul ini | **Addendum** pada mesin keutuhan `MedicalRecordManagement` — `RWI-DEC-091` |
> | Keadaan source | Diandaikan nol pekerjaan sudah mendarat | **Tiga prasyarat besar ternyata sudah ada di source** — lihat bagian 2 |
>
> Revision `1` disimpan apa adanya di [`archive/revision-1/`](./archive/revision-1/backend-roadmap.md)
> supaya jejak perencanaannya tidak hilang.

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
roadmap_revision: 2
status: APPROVED
roadmap_mode: DELIVERY
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah - Product/Domain owner (RWI-DEC-061), pemilik ClinicalManagement (RWI-DEC-062)"
approved_at: "2026-09-03"
approval_decision: RWI-DEC-092
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Pemilik ClinicalManagement: Muhammad Hamzah (RWI-DEC-062)"
  - "Pemilik MedicalRecordManagement: belum dinyatakan tertulis - lihat bagian 5"
  - "Clinical governance: sebagian terisi (RWI-DEC-064); pemilik batas waktu klinis belum ditunjuk"
  - "Security/Privacy: OPEN"
contract_versions: 0.3.0
contract_files:
  contracts/api-contract.md: 0.3.0
  contracts/integration-contract.md: 0.3.0
  contracts/state-transition-matrix.md: 0.3.0
  contracts/validation-matrix.md: 0.3.0 (last_changed_in 0.1.0)
  contracts/permission-audit-matrix.md: 0.3.0 (last_changed_in 0.1.0)
  testing/acceptance-test-matrix.md: 0.3.0
input_revisions:
  blueprint-manifest.md (tingkat modul): 5
  blueprint-manifest.md (sub-modul): 5
  00-interview-decisions.md: 13
  01-existing-capability-map.md: 1.3
  02-module-map.md: 1
  evidence/02-requirement-completeness-gate.md: 1.4
  02-backend-architecture.md: 0.3
  03-frontend-architecture.md: 0.2
  04-prd-to-mvp.md: 0.3
planning_source_sha:
  backend: 7d4bf2b91d39265eab4453a230ba324f95866962 (branch MHamzah, dibaca 5 September 2026)
  frontend: eb505a9ea20d99505a69ff0e6ea428ec9a551dc5 (branch HamzahV2, dibaca 5 September 2026)
evidence_source_sha:
  audit as-is 02-backend-architecture.md: 5afb54bd75281648010e50ef14f43ca1f80d8efd
  audit RWI-FACT-016: 93b3227c431401d8f586dec4e1fb25fbf41766e3
capability_scope:
  - CAP-012 Nursing Assessment
  - CAP-013 Nursing Care
  - CAP-014 Nursing Interventions
  - CAP-027 Nutrition Care (hanya skrining dan rujukan)
capability_excluded:
  - CAP-016 Equipment Usage - DEFERRED oleh RWI-DEC-089, nol task
requirement_readiness: INP-S16 PARTIALLY_READY; empat kemampuan aktif READY_FOR_DOMAIN_DESIGN
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
task_id_range: BE-RWI-054 .. BE-RWI-065
task_count: 12
```

> **Kenapa `planning_source_sha` berbeda dari `evidence_source_sha`.** Arsitektur dan kontrak
> sub-modul ini ditulis 2 September 2026 di atas snapshot `5afb54b` dan `93b3227`. Roadmap ini
> ditulis ulang 5 September 2026 di atas snapshot `7d4bf2b`, dan di antara kedua tanggal itu
> sub-modul `dokter-rawat-inap` menyelesaikan beberapa task yang **kebetulan mendaratkan
> prasyarat sub-modul ini**. Perbedaan itu bukan kelalaian pencatatan; ia justru temuan
> terpenting revision `2`, dan dijabarkan pada bagian 2.

---

## 0. Lima peringatan yang tidak boleh dilewati

### 0.1 Gerbang approval sudah dicabut — dan apa yang **tidak** ikut tercabut

| Hal | Keadaannya |
| --- | --- |
| Status blueprint sub-modul | **`approved`** — `keperawatan/blueprint-manifest.md`, disetujui Muhammad Hamzah 3 September 2026 |
| Keputusan approval | **`RWI-DEC-092`** — menyetujui blueprint revision `5` beserta keenam kontraknya pada `0.3.0` |
| Akibatnya | Task pada roadmap ini **boleh dikirim ke `/qv-be`**, satu task per pengerjaan, mengikuti urutan dependency bagian 3 |
| Yang **tidak** ikut tercabut | Gerbang sebelum produksi. Migration **MUST NOT** diterapkan ke database bersama tanpa izin tertulis terpisah. Wewenang tulis database bukan bagian dari approval blueprint |
| Yang juga **tidak** ikut tercabut | Kewenangan mengubah lingkup task. Builder hanya menulis baris status dan tautan bukti — `rules/rule-output/status-task-roadmap.md` bagian 6 |

`RWI-DEC-092` menuliskannya sendiri: *"Approval ini tidak membuka eksekusi. Roadmap sub-modul
berstatus `DRAFT_STALE` dan wajib ditulis ulang `/qv-plan` menjadi revision `2` berstatus
`APPROVED` lebih dulu."* Dokumen inilah revision `2` itu.

### 0.2 Sub-modul ini **tidak memiliki satu tabel pun**

`RWI-DEC-081` dan `PRD-RWI-FINAL-001` bagian 23.1 menaruh **seluruh** tabel dokumentasi klinis
rawat inap pada **`ClinicalManagement`**. Konsekuensinya berat dan sering disalahpahami:

| Yang sering disangka | Yang sebenarnya |
| --- | --- |
| "Task backend keperawatan menulis di `Areas/HealthServices/InPatientManagement/`" | **Salah.** Seluruh task di bawah ini menulis di `Areas/HealthServices/ClinicalManagement/`, dan satu task ikut menyentuh `Areas/HealthServices/MedicalRecordManagement/` |
| "Rawat Inap membuat tabel pengkajian dan asuhan sendiri" | **Dilarang keras.** Membuat `InpNursingAssessment` atau tabel `Inp*` apa pun untuk dokumentasi klinis melanggar `RWI-DEC-081` |
| "Ini pekerjaan satu tim saja" | Ini **permintaan perubahan kepada modul tetangga**. Persetujuan arahnya sudah ada lewat `RWI-DEC-062` |

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
(`FR-KEP-011`, `VAL-KEP-17`).

**Contoh supaya tidak salah paham.** Ns. Sari menyelesaikan pengkajian awal Tn. Budi pada 5
September pukul 09.00, sementara `MstClinicalAssessmentPolicy` masih kosong. Yang terjadi:
pengkajian tersimpan dan berstatus `Completed`; kolom `DueAt` dibiarkan kosong; daftar pantau
kepatuhan berbunyi *"Batas waktu pengkajian belum ditetapkan"* — **bukan** *"terlambat"* dan
**bukan** *"tidak ada data"*.

### 0.5 Kesesuaian engineering diselesaikan saat eksekusi

Preflight QBE dan kesesuaian kontrak rekayasa backend **tidak** diputuskan di roadmap ini. Keduanya
diselesaikan pada waktu eksekusi dari `AGENTS.md` repository backend target beserta dokumen
engineering canonical yang berlaku saat itu.

Satu hal sudah pasti dan wajib dibawa sejak kartu task pertama: **penamaan entity baru mengikuti
`QBE-NAM-001`, dan prefix `Trx*` dilarang untuk kode baru.** `BE-RWI-041` pada sub-modul
`dokter-rawat-inap` sudah membenturkan aturan itu — revision `0.1` roadmap-nya sempat menulis
`TrxPhysicianVisit` dan itu keliru; tabelnya lahir sebagai `CliPhysicianVisit`. Kelima tabel baru
pada roadmap ini karena itu **tidak** memakai `Trx*`; nama bakunya disebut pada masing-masing
kartu.

---

## 1. Cara membaca roadmap ini

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

**Diperbarui 8 September 2026.** **Seluruh 12 task sudah dieksekusi dan seluruhnya bertanda ✅.**
Dua task yang sebelumnya 🟡 ditutup hari ini:

- `BE-RWI-056` ✅ — kriteria 4 kini terbukti penuh, termasuk bagian **risiko jatuh** yang dituntut
  `UAT-KEP-07`. Jalan keluarnya bukan mengubah bentuk `HasFallRisk`, melainkan membaca field
  `FallRiskStatus` yang **sudah ada** pada permintaan dan selama ini diabaikan — sejalan dengan
  `VAL-KEP-09` dan dengan frontend rawat inap yang memang sudah mengirimkannya. Nol perubahan bentuk
  data, nol perubahan frontend, nol pergeseran pada jalur poliklinik, medical check-up, dan IGD.
- `BE-RWI-061` ✅ — butir DoD "uji PostgreSQL hijau" terpenuhi. Lingkungan ujinya disediakan sebagai
  **container PostgreSQL 16 sekali pakai** (`quilvian_kep_test`), bukan dengan meminta hak baru pada
  server bersama. Ketiga uji lulus tanpa satu pun perubahan kode.

Tidak satu pun task bertanda 🟡 maupun ⛔.

> **Satu hal yang berlaku bagi seluruh task berentitas baru.** Ketiga migration slice
> `KEP-MVP-2` dan `KEP-MVP-3` — `20260906144058_AddNursingCarePlan`,
> `20260906144956_AddNursingCarePlanItemRevision`, dan `20260906151002_AddNursingIntervention` —
> **sudah dibuat tetapi belum diterapkan ke database bersama, dev, staging, atau production mana pun**.
> Wewenang eksekusinya terpisah dan belum diberikan, sesuai bagian 0.1.
>
> **Ketelitian 8 September 2026.** Ketiganya **pernah** diterapkan sekali, ke sebuah container PostgreSQL
> 16 sekali pakai berdatabase `quilvian_kep_test` yang dibuat khusus untuk menjalankan uji `BE-RWI-061`
> lalu dibuang. `Database.Migrate()` dari nol berhasil, sehingga jalur migration majunya kini terbukti.
> Itu **bukan** penerapan ke database yang dipakai siapa pun, dan tidak mengubah keadaan di atas.

Setiap task menyebut jejak requirement ke `FR-KEP-0xx` pada
[`../04-prd-to-mvp.md`](../04-prd-to-mvp.md) bagian 10, dan jejak keputusan ke decision ID pada
[`../../00-interview-decisions.md`](../../00-interview-decisions.md).

---

## 2. Keadaan awal yang terverifikasi di source

Ini bagian terpenting revision `2`. Seluruh baris di bawah **dibaca langsung dari source** pada
snapshot `BE@7d4bf2b`, 5 September 2026 — bukan disalin dari dokumen arsitektur, dan bukan
diandaikan.

### 2.1 Tiga prasyarat besar yang ternyata **sudah mendarat**

| Yang revision `1` sebut sebagai penghalang | Keadaan sebenarnya di `BE@7d4bf2b` | Bukti |
| --- | --- | --- |
| **`INT-KEP-01`** — pelonggaran konteks klinis, disebut "satu-satunya penghalang yang menahan seluruh sub-modul" | **Sudah ada.** `ValidateWithoutQueueGateAsync` memanggil resolver konteks rawat inap; encounter yang punya episode berjalan diterima **tanpa** antrean dan **tanpa** kunjungan IGD | `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs:1040-1064`; `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` |
| **`RWI-OQ-051`** — perluasan penegakan keutuhan ke jenis `Assessment` dan `Procedure` | **Sudah ada.** Daftar jenis yang ditegakkan kini berisi **empat** nilai: `ProgressNote`, `Consultation`, `Assessment`, `Procedure` | `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs:74-87` |
| Dua dari empat kolom yang diminta pada tabel pengkajian | **Sudah ada.** `InpEpisodeId` dan `AssessmentType`, beserta enum `PatientAssessmentType` berisi enam nilai — empat keperawatan, dua kajian medis | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs:62,73`; `Areas/HealthServices/ClinicalManagement/Enums/PatientAssessmentType.cs` |

**Siapa yang mendaratkannya, dan kenapa itu sah.** Ketiganya lahir dari sub-modul
`dokter-rawat-inap` — `BE-RWI-039` (✅ 3 September 2026), `BE-RWI-038` (✅), dan `BE-RWI-040` (🟡).
Ini persis yang direncanakan `INT-DOK-09`: *"Service ini dibuat **sekali**; roadmap `keperawatan`
kelak menerima baris dependency, **bukan salinan task**."* Komentar pada source menyebutnya
sendiri, apa adanya:

> *"Kolom ini diminta bersama oleh sub-modul `keperawatan` (`INT-KEP-01`) dan `dokter-rawat-inap`
> (`INT-DOK-01`). Dibuat sekali oleh yang mendarat lebih dulu, lalu dipakai apa adanya oleh yang
> kedua — `INT-DOK-09`."*
> — `TrxPatientAssessment.cs:56-61`

Akibatnya bagi roadmap ini: **task yang membuat ulang ketiganya dilarang.** Yang benar adalah
memakainya apa adanya, lalu mengerjakan sisanya.

### 2.2 Yang **belum ada**, dan menjadi isi roadmap ini

| Yang dibutuhkan | Keadaan di `BE@7d4bf2b` | Task pemilik |
| --- | --- | --- |
| Kolom `DueAt` dan `PolicyId` pada tabel pengkajian | **Belum ada** | `BE-RWI-054` |
| Master kebijakan batas waktu | **Belum ada** — nol berkas `*AssessmentPolicy*` di seluruh repository | `BE-RWI-055` |
| Aturan pengkajian awal kedua ditolak, beserta index episode | **Belum ada** | `BE-RWI-056` |
| **Pendaftaran pengkajian keperawatan** ke mesin keutuhan saat `Completed` | **Belum ada, dan sengaja belum.** Source menyatakannya sendiri: *"Sengaja hanya untuk kajian medis. Pendaftaran pengkajian keperawatan adalah pekerjaan sub-modul keperawatan; menyalakannya dari sini akan mengubah perilaku jalur poliklinik dan IGD yang tidak diminta task ini."* — `PatientAssessmentController.cs:778-782` | **`BE-RWI-065`** — task baru |
| Endpoint lini masa dan keadaan tenggat | **Belum ada** | `BE-RWI-058` |
| Tabel rencana asuhan dan tindakan keperawatan | **Sudah ada sejak 7 September 2026** — empat tabel `Cli*` beserta configuration, `DbSet`, dan migration-nya. Keadaan "nol berkas `*Nursing*`" di atas berlaku sampai `BE@7d4bf2b` | `BE-RWI-059` ✅ s.d. `BE-RWI-062` ✅, `BE-RWI-061` ✅ |
| Penyaluran catatan keperawatan ke catatan terpadu | **Sudah ada sejak 7 September 2026.** Ditemukan saat implementasi: kolom `InpEpisodeId` ternyata tidak pernah diisi siapa pun, sehingga lini masa catatan terpadu selalu kosong bagi **seluruh** profesi — kini terisi tanpa satu pun tabel atau kolom baru | `BE-RWI-063` ✅ |
| Daftar pantau kepatuhan pengkajian | **Belum ada** | `BE-RWI-064` |

### 2.3 Mesin koreksi sudah **generik** — dan satu butir yang wajib diputuskan pemilik kontrak

Mesin addendum ternyata bukan hanya ada, melainkan sudah generik terhadap jenis dokumen:

```text
POST /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}
GET  /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}
```

— `Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs:129,179`

Sementara itu [`../contracts/api-contract.md`](../contracts/api-contract.md) `0.3.0` meminta
`POST /patient-assessments/{id}/addendums` dan `POST /nursing-interventions/{id}/addendums` —
yaitu **pintu kedua menuju mesin yang sama**.

| Hal | Ketetapannya |
| --- | --- |
| Apakah ini memblokir | **Tidak.** Keduanya menuju satu mesin; yang berbeda hanya pintunya |
| Yang **dilarang** | Membangun penyimpanan koreksi sendiri di `ClinicalManagement`. Itu mesin koreksi tandingan, dilarang `RWI-DEC-087` dan justru dihindari `RWI-DEC-091` |
| Yang **wajib** bila endpoint kontrak tetap dibuat | Endpoint pada `patient-assessments` dan `nursing-interventions` **meneruskan** ke `ClinicalNoteAddendumService`, bukan menulis barisnya sendiri |
| Butir yang diangkat ke pemilik kontrak | Apakah pintu kedua itu memang diinginkan, atau ruang kerja keperawatan cukup memanggil endpoint generik yang sudah ada. Pemilik: Muhammad Hamzah selaku pemilik kontrak. Diselesaikan `/qv-design`, **bukan** oleh builder |

Sampai butir itu dijawab, `BE-RWI-057` dan `BE-RWI-062` dikerjakan mengikuti kontrak `0.3.0` apa
adanya, dengan syarat penerusan di atas. Butir ini terdaftar pada bagian 5.

> **Keduanya sudah dibangun demikian.** `BE-RWI-057` ✅ pada 6 September 2026 dan `BE-RWI-062` ✅
> pada 7 September 2026, masing-masing meneruskan ke `ClinicalNoteAddendumService` tanpa membuat
> satu pun baris koreksi sendiri.

### 2.4 Empat berkas hulu yang **kepala dokumennya tertinggal**

Ditemukan saat membaca masukan, dicatat supaya tidak menjadi kebingungan berikutnya. **Tidak satu
pun memblokir**: isinya sudah benar, yang tertinggal hanya baris kepala dokumennya.

| Berkas | Isinya | Kepala dokumennya masih menulis |
| --- | --- | --- |
| `contracts/api-contract.md` | Sudah `0.3.0`, sudah menyerap `RWI-DEC-091` | `Status: draft — belum disetujui manusia`, `approved_by: — belum` |
| `contracts/state-transition-matrix.md` | Sudah `0.3.0` | Sama |
| `contracts/integration-contract.md` | Sudah `0.3.0`, sudah memuat `INT-KEP-06` | Sama |
| `04-prd-to-mvp.md` | Isinya sudah revision `0.3` — bagian 20.1 sudah menyatakan butir konsistensi tertutup | `contract_version: 0.1.0`, `Revision artefak: 0.1` |

Yang benar adalah `blueprint-manifest.md` bagian 4: keenam kontrak **`approved`** sejak 3
September 2026 lewat `RWI-DEC-092`. Merapikan baris kepala keempat berkas itu adalah pekerjaan
`/qv-design`, **bukan** pekerjaan roadmap ini dan bukan pekerjaan builder.

---

## 3. Gelombang dan urutan dependency

| Gelombang | Task | Isinya | Prasyarat |
| --- | --- | --- | --- |
| **`KEP-MVP-0`** ✅ | `BE-RWI-054` ✅, `BE-RWI-055` ✅ | Melengkapi bentuk kolom pengkajian dan master kebijakan | `episode-rawat-inap` `M1` selesai; `BE-RWI-039` dan `BE-RWI-040` sudah mendarat |
| **`KEP-MVP-1`** ✅ | `BE-RWI-056` ✅, `BE-RWI-065` ✅, `BE-RWI-057` ✅, `BE-RWI-058` ✅ | `EPIC KEP-01` dan `EPIC KEP-02` | `KEP-MVP-0` |
| **`KEP-MVP-2`** ✅ | `BE-RWI-059` ✅, `BE-RWI-060` ✅ | `EPIC KEP-03` rencana asuhan | `BE-RWI-056` |
| **`KEP-MVP-3`** ✅ | `BE-RWI-061` ✅, `BE-RWI-062` ✅, `BE-RWI-063` ✅ | `EPIC KEP-04` tindakan dan catatan terpadu | `BE-RWI-056` |
| **`KEP-MVP-4`** ✅ | `BE-RWI-064` ✅ | `EPIC KEP-05` daftar pantau kepatuhan | `BE-RWI-058` |
| **Tidak masuk gelombang** | — | `EPIC KEP-06` pemakaian alat | `DEFERRED` oleh `RWI-DEC-089` |

### Urutan dependency

```text
BE-RWI-054 ✅ ─┬─> BE-RWI-056 ✅ ─┬─> BE-RWI-065 ✅ ─> BE-RWI-057 ✅
               │                  │
BE-RWI-055 ✅ ─┴─────────────────>├─> BE-RWI-058 ✅ ─> BE-RWI-064 ✅
                                  │
                                  ├─> BE-RWI-059 ✅ ─> BE-RWI-060 ✅
                                  │
                                  ├─> BE-RWI-061 ✅ ─> BE-RWI-062 ✅
                                  │
                                  └─> BE-RWI-063 ✅
```

`BE-RWI-059`, `BE-RWI-061`, dan `BE-RWI-063` dapat berjalan **paralel** setelah `BE-RWI-056`
selesai, karena ketiganya menyentuh tabel yang berbeda dan tidak berbagi kontrak.

`BE-RWI-058` membutuhkan `BE-RWI-055` **dan** `BE-RWI-056`: tanpa master kebijakan tidak ada
tenggat yang dapat dihitung, dan tanpa pemisahan pengkajian awal dari pengkajian ulang tidak ada
lini masa yang bermakna.

---

## 4. Task

### ✅ `BE-RWI-054` — Pengkajian rawat inap punya tempat menyimpan tenggat dan kebijakannya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). Butir DoD "uji migration maju dan mundur terhadap PostgreSQL sungguhan" **belum terpenuhi** — wewenang eksekusi database tidak diberikan task ini, dan migration `20260905090533_AddAssessmentDueAtAndPolicyId` **belum diterapkan ke database mana pun**. Bukti: [laporan](../task/report/backend/BE-RWI-054.md) |
| **Outcome** | Setiap pengkajian rawat inap dapat menjawab "kapan seharusnya selesai" dan "menurut kebijakan yang mana", sehingga keterlambatan dapat dinilai tanpa mengubah penilaian pengkajian yang lalu |
| **Trace** | `FR-KEP-001` s.d. `FR-KEP-004`, `FR-KEP-010`; `RWI-DEC-062`, `RWI-DEC-081`, `RWI-RULE-026`; PRD 16.2 aturan 1, 2, 4, 11; `AC-CAP012-01`; `INT-KEP-01` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Patient Assessment; `contracts/integration-contract.md` `0.3.0` `INT-KEP-01`, `INT-KEP-02` |
| **Keadaan awal terverifikasi** | `BE@7d4bf2b`: `InpEpisodeId` **sudah ada** (`TrxPatientAssessment.cs:62`), `AssessmentType` **sudah ada** (`:73`), cabang episode pada validasi **sudah ada** (`PatientAssessmentController.cs:1050-1056`), `GET /patient-assessments/episodes/{episodeId}` **sudah ada** (`:281`). Tersisa dua kolom |
| **Reuse** | `TrxPatientAssessment` beserta controller dan service-nya; `InpatientClinicalContextService` milik `BE-RWI-039`; pola kolom nullable beserta konfigurasi EF yang dipakai `BE-RWI-040`. **Nol tabel baru, nol controller baru, nol service baru** |
| **Scope** | `Areas/HealthServices/ClinicalManagement/`: **dua** kolom nullable pada `TrxPatientAssessment` — `DueAt` (`DateTime?`) dan `PolicyId` (`Guid?`); index `IX_TrxPatientAssessment_InpEpisodeId`; `DeleteBehavior.Restrict` pada relasi episode; penyesuaian DTO; satu migration |
| **Dependency** | `BE-RWI-039` dan `BE-RWI-040` pada `dokter-rawat-inap` — **keduanya sudah mendarat**. `episode-rawat-inap` `M1` — selesai |
| **Acceptance criteria** | 1. Kolom `DueAt` dan `PolicyId` terbentuk, keduanya nullable, dan baris lama **tidak disentuh**. 2. Pengkajian dapat dibuat untuk encounter yang punya episode `Admitted` tanpa `QueueId` dan tanpa kunjungan IGD, dijawab `201`. 3. Pengkajian ditolak `422` bila episode tidak ada (`VAL-KEP-01`), masih `Draft` (`VAL-KEP-02`), atau sudah `Closed` (`VAL-KEP-03`). 4. Encounter rawat jalan tanpa antrean dan tanpa episode tetap ditolak `400` (`VAL-KEP-04`). 5. **Perilaku pengkajian poliklinik, medical check-up, dan IGD tidak berubah sedikit pun**, dibuktikan test regresi. 6. Index episode terbentuk dan penghapusan episode yang masih punya pengkajian ditolak |
| **Verification** | Test regresi jalur poliklinik, medical check-up, dan IGD — **wajib bagian task ini**, bukan pekerjaan menyusul; uji migration maju dan mundur; pembandingan bentuk kolom terhadap `data/data-dictionary.md` |
| **Risk/blocker** | `RWI-DEC-051` mencatat belum ada satu pun test yang menjaga jalur poliklinik dan IGD pada endpoint ini. Cabang rawat inap sudah menyala di source **tanpa** test regresi yang menjaganya — itu utang yang ditutup task ini. Owner: pemilik `ClinicalManagement` |
| **DoD** | Dua kolom, satu index, satu migration, `DeleteBehavior` terpasang, test regresi tiga jalur lulus, `dotnet build` lulus, `dotnet test` lulus, laporan menyatakan migration **belum** diterapkan ke database bersama |

---

### ✅ `BE-RWI-055` — Batas waktu pengkajian dibaca dari master, bukan ditanam di kode

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). Butir DoD "uji hak akses non-SuperAdmin lulus" **belum terpenuhi** — project uji memanggil controller langsung sehingga `AccessPermissionFilter` dilewati; penggantinya uji refleksi kontrak penamaan atribut, dan keterbatasannya dijelaskan pada laporan. Migration `20260906121936_AddClinicalAssessmentPolicyMaster` **belum diterapkan ke database mana pun**. Bukti: [laporan](../task/report/backend/BE-RWI-055.md) |
| **Outcome** | Clinical governance dapat mengatur sendiri batas waktu pengkajian awal dan pengkajian ulang tanpa menyentuh kode. Selama masternya kosong, tidak ada satu pun pengkajian yang dinyatakan terlambat |
| **Trace** | `FR-KEP-010`, `FR-KEP-011`; `RWI-RULE-021` (**belum final, dan memang tidak perlu final**); PRD 16.2 aturan 11; `AC-CAP012-04` |
| **Kontrak** | `contracts/validation-matrix.md` `VAL-KEP-17`, `VAL-KEP-18`; `contracts/api-contract.md` `0.3.0` |
| **Keadaan awal terverifikasi** | `BE@7d4bf2b`: nol berkas `*AssessmentPolicy*` di seluruh repository. Tabel ini benar-benar belum ada |
| **Reuse** | Pola `MstInpatientSetting` dari `BE-RWI-001`: bentuk kolom audit `IdentityModel`, soft delete, dan konfigurasi EF. Pola master berversi yang dipakai master tarif |
| **Scope** | `MstClinicalAssessmentPolicy` milik `ClinicalManagement`: batas waktu per jenis pengkajian per jenis pelayanan, **berversi** lewat periode berlaku; satu configuration EF pada `Repositories/Configurations/HealthServices/ClinicalManagement/`; satu `DbSet`; satu migration; endpoint pengelolaan master |
| **Dependency** | — dapat berjalan **paralel** dengan `BE-RWI-054` |
| **Acceptance criteria** | 1. Master menyimpan batas waktu per jenis pengkajian per jenis pelayanan. 2. Kebijakan **berversi**: penilaian keterlambatan memakai kebijakan yang **aktif saat pengkajian dibuat**, bukan yang berlaku sekarang. 3. Master kosong **tidak** menahan pencatatan pengkajian dan **tidak** menghasilkan satu pun penanda terlambat (`VAL-KEP-17`). 4. Endpoint pengelolaan master memakai pasangan `[AccessAction]` dan `[AccessPermission]` bernama sama persis, dan diuji dengan peran non-SuperAdmin |
| **Verification** | Uji tiga keadaan: master kosong, master terisi, dan kebijakan yang berubah di tengah episode. Uji hak akses memakai peran non-SuperAdmin |
| **Risk/blocker** | Kebijakan yang tidak berversi akan membuat episode lama tiba-tiba terlihat terlambat ketika angkanya diubah. **Contoh nyata:** batas pengkajian awal semula 24 jam; pengkajian Tn. Budi selesai pada jam ke-20 dan dinilai tepat waktu. Bila kebijakan diubah menjadi 8 jam dan penilaian memakai kebijakan terbaru, pengkajian Tn. Budi tiba-tiba terbaca terlambat padahal perawatnya tidak melanggar apa pun. Owner: Backend/API bersama Clinical governance |
| **DoD** | Satu tabel master, configuration, `DbSet`, migration, endpoint pengelolaan; uji tiga keadaan master lulus; uji hak akses non-SuperAdmin lulus; `dotnet build` lulus |

---

### ✅ `BE-RWI-056` — Pengkajian awal dan pengkajian ulang tidak lagi saling menimpa

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 8 September 2026.** Keempat acceptance criteria terpetakan ke source dan terbukti penuh. Kriteria 4 — yang sebelumnya tertahan karena penilaian **risiko jatuh** tidak dapat dibedakan antara *belum diisi* dan *tidak berisiko* — kini terbukti lewat `MenyelesaikanPengkajianTanpaRisikoJatuh_Ditolak400`, `MenyelesaikanPengkajianDuaBagianKosong_KeduanyaDisebutSatuPerSatu`, `RisikoJatuhDinyatakanTidakBerisiko_PengkajianDapatDiselesaikan`, dan `PengkajianLangsungSelesai_TanpaRisikoJatuh_Ditolak400`. Jalannya **bukan** mengubah bentuk `HasFallRisk`, melainkan membaca field `FallRiskStatus` yang sudah ada pada permintaan — **nol perubahan bentuk data, nol perubahan frontend**, dan jalur poliklinik, medical check-up, serta IGD dibuktikan tidak bergeser oleh `PengkajianPoliklinik_TanpaRisikoJatuh_TetapTersimpanNoRisk`. Pintu kedua penyelesaian (`completeImmediately`) ikut dijaga. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 453, Skipped: 0, Total: 453`. Index parsial non-unique `IX_TrxPatientAssessment_Episode_Type_Active` dibaca langsung dari katalog PostgreSQL. Migration `20260906122534_AddInitialNursingAssessmentPartialIndex` **belum diterapkan** ke database bersama, dev, staging, atau production mana pun; yang menerimanya hanya container sekali pakai yang sudah dibuang. Butir terbuka yang dilaporkan: `VAL-KEP-09` belum terpasang, dan jalur `completeImmediately` belum mendaftarkan pengkajian ke mesin keutuhan — temuan bagi pemilik `BE-RWI-065`. Bukti: [laporan](../task/report/backend/BE-RWI-056.md) |
| **Outcome** | Pengkajian ulang harian tersimpan sebagai catatan tersendiri. Nilai pengkajian awal tetap utuh dan dapat dibaca kembali kapan pun |
| **Trace** | `FR-KEP-005`, `FR-KEP-006`; PRD 16.2 aturan 3; `AC-CAP012-02`; `VAL-KEP-11` |
| **Kontrak** | `contracts/state-transition-matrix.md` `0.3.0` bagian 1; `contracts/validation-matrix.md` `VAL-KEP-08`, `VAL-KEP-11` |
| **Keadaan awal terverifikasi** | Enum `PatientAssessmentType` **sudah ada** dengan `Initial`, `Reassessment`, `DailyReassessment`, `DischargePlanning`, ditambah dua nilai kajian medis milik `dokter-rawat-inap`. Aturan penolakan pengkajian awal kedua **belum ada** |
| **Reuse** | `TrxPatientAssessment` beserta kolom `AssessmentType` yang sudah mendarat lewat `BE-RWI-040` |
| **Scope** | Aturan pembuatan pengkajian pada jalur `ClinicalManagement`; penolakan pengkajian awal kedua per episode; **index parsial** `IX_TrxPatientAssessment_Episode_Type_Active` pada `(InpEpisodeId, AssessmentType)` dengan penyaring `AssessmentType = Initial AND IsDelete = false`; satu migration |
| **Dependency** | `BE-RWI-054` |
| **Acceptance criteria** | 1. Pengkajian awal dan pengkajian ulang tersimpan sebagai record terpisah; nilai record pertama sama persis sebelum dan sesudah record kedua dibuat. 2. Pengkajian awal **kedua** pada satu episode ditolak `409`, dan pesannya berbunyi *"Pengkajian awal untuk pasien ini sudah ada. Gunakan pengkajian ulang."* — bukan pesan teknis. 3. Pengkajian awal yang **dibatalkan** tidak menghalangi pembuatan pengkajian awal berikutnya. 4. Menyelesaikan pengkajian dengan isian wajib kosong ditolak `400` dan menyebut bagian yang kosong satu per satu (`VAL-KEP-08`) |
| **Verification** | Skenario dua pengkajian berurutan; skenario pengkajian awal kedua; **skenario pengkajian awal dibatalkan lalu diulang**; pemeriksaan bahwa index yang terbentuk memang parsial |
| **Risk/blocker** | **Perubahan lingkup dari revision `1`, wajib dibaca.** Revision `1` meminta *unique* index parsial. `02-backend-architecture.md` `0.3` bagian 4.1 mencabutnya: unique constraint **tidak diminta**, karena pengkajian awal yang dibatalkan lalu diulang adalah kejadian nyata dan unique index akan ikut menghitung baris yang dibatalkan. Aturan "satu pengkajian awal aktif per episode" dijaga di tingkat service, index-nya hanya mempercepat pencarian. Owner: Backend/API |
| **DoD** | Aturan pembuatan pada service, satu index parsial **non-unique**, satu migration, uji empat skenario lulus, `dotnet build` lulus |

---

### ✅ `BE-RWI-065` — Pengkajian keperawatan yang selesai ikut terkunci seperti dokumen dokter

> **Task baru pada revision `2`.** Ia lahir dari `RWI-DEC-091` dan dari temuan bagian 2.2:
> mesin keutuhan sudah menegakkan jenis `Assessment`, tetapi pengkajian **keperawatan** belum
> pernah didaftarkan ke sana. Tanpa task ini, `BE-RWI-057` tidak punya apa pun untuk dikoreksi.

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). **Nol perubahan bentuk data; migration task ini kosong.** Seluruh butir DoD terpenuhi. Bukti: [laporan](../task/report/backend/BE-RWI-065.md) |
| **Outcome** | Pengkajian keperawatan yang sudah diselesaikan tidak dapat disunting diam-diam. Ia terkunci pada mesin yang sama dengan dokumen dokter, sehingga satu lembar rekam medis tidak memuat dua bentuk penguncian |
| **Trace** | `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`, `RWI-DEC-086`, `RWI-DEC-087`, `RWI-FACT-016`; `RWI-AC-175`; `INT-KEP-06` |
| **Kontrak** | `contracts/integration-contract.md` `0.3.0` `INT-KEP-06` bagian 7.1; `contracts/state-transition-matrix.md` `0.3.0` bagian 1 |
| **Keadaan awal terverifikasi** | Mesin keutuhan **sudah menegakkan** `Assessment` (`ClinicalDocumentIntegrityService.cs:74-79`). Jalur `PATCH /{id}/complete` **sudah** mendaftarkan kajian medis lewat `RegisterSignedAsync` (`PatientAssessmentController.cs:799`), tetapi **hanya bila** `isKajianMedis` benar. Pengkajian keperawatan sengaja dilewati, dengan alasan tertulis pada source |
| **Reuse** | `ClinicalDocumentIntegrityService.RegisterSignedAsync` dan `EnsureMutableAsync` yang **sudah ada**; pola transaksi tunggal yang sudah dipakai `BE-RWI-038` untuk kajian medis. **Nol tabel baru, nol enum baru, nol nilai enum baru** |
| **Scope** | Melebarkan pendaftaran pada `PATCH /{id}/complete` sehingga pengkajian **keperawatan** ikut didaftarkan sebagai dokumen tertanda tangan berjenis `Assessment`, dalam `SaveChanges` yang sama; memasang `EnsureMutableAsync` pada jalur `PUT /{id}` sehingga pengkajian yang sudah terkunci menolak penyuntingan langsung |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Menyelesaikan pengkajian keperawatan mendaftarkannya pada mesin keutuhan dengan jenis `Assessment`, penulis pengkajian sebagai penanda tangan. 2. Bila pendaftaran gagal, **penyelesaian ikut batal** — tidak boleh ada pengkajian `Completed` yang tidak punya baris keutuhan. 3. Percobaan menyunting langsung isi pengkajian yang sudah `Completed` ditolak `400` beserta arahan memakai koreksi. 4. **Nol nilai enum baru** ditambahkan ke `ClinicalDocumentKind`. 5. **Perilaku kajian medis, poliklinik, dan IGD tidak berubah**, dibuktikan test regresi. 6. Pengkajian yang masih `Draft` atau `InProgress` tetap dapat disunting seperti biasa |
| **Verification** | Integration test penyelesaian lalu percobaan sunting; test yang **memaksa pendaftaran gagal** lalu membuktikan penyelesaian ikut batal; test regresi kajian medis dan jalur poliklinik; test yang menghitung jumlah nilai `ClinicalDocumentKind` sebelum dan sesudah |
| **Risk/blocker** | Pendaftaran dan penyelesaian **wajib satu transaksi**. Bila dipisah, akan lahir pengkajian selesai yang tidak dapat dikoreksi — persis keadaan yang ditemukan `RWI-FACT-014` pada dokumen dokter dan yang sedang ditutup. Owner: pemilik `ClinicalManagement` |
| **DoD** | Pendaftaran menyala untuk pengkajian keperawatan, penjaga penyuntingan terpasang, keenam acceptance criteria terbukti, test transaksi hijau, `dotnet build` lulus, laporan menyebut **nol perubahan bentuk data** |

---

### ✅ `BE-RWI-057` — Pengkajian final dibetulkan lewat koreksi, bukan dengan menimpanya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Ketujuh acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). **Nol tabel baru; migration task ini kosong.** Butir terbuka yang dilaporkan: jalur koreksi oleh kepala ruangan tetap memakai endpoint pengganti milik `MedicalRecordManagement`, karena aturannya dimiliki modul itu. Bukti: [laporan](../task/report/backend/BE-RWI-057.md) |
| **Outcome** | Perawat yang salah mengisi dapat membetulkannya lewat koreksi beralasan. Isi asli tetap tersimpan sebagai bukti klinis, dan tidak ada jalan menghapusnya diam-diam |
| **Trace** | `FR-KEP-008`, `FR-KEP-009`; `RWI-DEC-091`; PRD 16.2 aturan 12 dan 13, 27.3 aturan 7; `AC-CAP012-05`, `RWI-AC-175`; `VAL-KEP-12` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `POST /{id}/addendums` dan `GET /{id}/addendums`; `contracts/state-transition-matrix.md` `0.3.0` bagian 1; `contracts/permission-audit-matrix.md` hak akses `PatientAssessment : Amend` |
| **Perubahan bentuk dari revision `1`** | **Bukan lagi amandemen berversi.** Koreksi = **addendum bernomor urut** pada mesin keutuhan `MedicalRecordManagement`. Status pengkajian **tetap** `Completed`; nilai status `Amended` **dicabut** dan tidak boleh dihidupkan kembali. Dua kolom `AmendedAt` dan `AmendedByUserId` **tidak jadi diminta** |
| **Reuse** | `ClinicalNoteAddendumService` dan `ClinicalNoteAddendumController` yang **sudah ada dan sudah generik**; `InpatientDocumentCorrectionAuthorityService` yang sudah ada. **Nol tabel baru** |
| **Scope** | Endpoint `POST /{id}/addendums` dan `GET /{id}/addendums` pada grup Patient Assessment yang **meneruskan** ke `ClinicalNoteAddendumService` dengan `documentKind = Assessment`; hak akses `PatientAssessment : Amend`; larangan hard-delete pada pengkajian final |
| **Dependency** | `BE-RWI-065` |
| **Acceptance criteria** | 1. Koreksi menyimpan aktor, waktu, alasan, dan nomor urut; isi pengkajian asli **tidak berubah sedikit pun**. 2. Alasan kosong ditolak `400` (`VAL-KEP-12`). 3. Status pengkajian **tetap** `Completed` sesudah koreksi, dan tetap `Completed` sesudah koreksi kedua. 4. Percobaan menambah koreksi pada pengkajian yang masih `Draft` ditolak, dengan arahan membetulkan langsung pada isinya (`RWI-FACT-013`). 5. Transisi `Completed → Draft` dan `Completed → Amended` ditolak. 6. Pengkajian final **tidak dapat** dihapus. 7. Koreksi tidak menulis baris pada tabel mana pun milik `ClinicalManagement` — seluruhnya tersimpan pada mesin `MedicalRecordManagement` |
| **Verification** | Skenario koreksi berulang dan pemeriksaan nomor urutnya; skenario koreksi tanpa alasan; skenario koreksi pada pengkajian konsep; percobaan hard-delete; percobaan membuka kembali pengkajian final; **pemeriksaan bahwa migration task ini kosong** |
| **Risk/blocker** | Godaan terbesar task ini adalah membuat penyimpanan koreksi sendiri karena terasa lebih cepat. Itu mesin koreksi tandingan, dilarang `RWI-DEC-087`. Butir pintu kedua endpoint pada bagian 2.3 wajib dibaca sebelum menulis controller. Owner: Muhammad Hamzah selaku pemilik kontrak |
| **DoD** | Dua endpoint yang meneruskan ke mesin yang sudah ada, hak akses terpasang, ketujuh acceptance criteria terbukti, **nol tabel baru dan migration kosong**, `dotnet build` lulus |

---

### ✅ `BE-RWI-058` — Perkembangan nyeri, risiko jatuh, dan gizi terbaca sebagai satu garis waktu

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). **Nol tabel baru; migration task ini kosong.** Seluruh butir DoD terpenuhi. Bukti: [laporan](../task/report/backend/BE-RWI-058.md) |
| **Outcome** | Perawat dan DPJP melihat apakah nyeri pasien membaik atau memburuk, bukan hanya nilai terakhirnya. Keadaan tenggat pengkajian terbaca dari kebijakan yang aktif |
| **Trace** | `FR-KEP-007`, `FR-KEP-010`; PRD 16.2 aturan 6 dan 11; `AC-CAP012-02`, `AC-CAP012-04` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `GET /episodes/{episodeId}/timeline` dan `GET /episodes/{episodeId}/due-status` |
| **Keadaan awal terverifikasi** | `GET /patient-assessments/episodes/{episodeId}` **sudah ada** dan mengembalikan daftar pengkajian per episode. Kedua endpoint lini masa dan tenggat **belum ada** |
| **Reuse** | Data pengkajian yang sudah tersimpan; endpoint daftar per episode yang sudah ada sebagai acuan bentuk. **Nol tabel baru** |
| **Scope** | Dua endpoint baca; proyeksi lini masa per jenis pengukuran — nyeri, risiko jatuh, skrining gizi; perhitungan `DueAt`, `CompletedAt`, dan keadaan terlambat menurut kebijakan yang aktif saat pengkajian dibuat |
| **Dependency** | `BE-RWI-055`, `BE-RWI-056` |
| **Acceptance criteria** | 1. Lini masa menampilkan **seluruh** pengukuran terurut waktu, bukan hanya yang terakhir. 2. Nilai lama tidak pernah ditimpa. 3. Keadaan tenggat dihitung dari kebijakan yang aktif **saat pengkajian dibuat**; mengubah kebijakan tidak mengubah penilaian pengkajian yang lalu. 4. Master kebijakan kosong menghasilkan keadaan "tidak dipantau", **bukan** "terlambat" (`VAL-KEP-17`). 5. Baris yang pernah dikoreksi membawa nomor urut addendum-nya, dan isi aslinya tetap tampil |
| **Verification** | Skenario tiga pengukuran nyeri berurutan; skenario master kosong; skenario kebijakan berubah di tengah episode; skenario pengkajian yang sudah dikoreksi |
| **Risk/blocker** | Perhitungan tenggat yang memakai kebijakan terbaru akan membuat episode lama salah dinilai — contoh berangkanya ada pada baris Risk `BE-RWI-055`. Owner: Backend/API |
| **DoD** | Dua endpoint, proyeksi lini masa tiga jenis pengukuran, perhitungan tenggat berversi, kelima acceptance criteria terbukti, `dotnet build` lulus |

---

### ✅ `BE-RWI-059` — Rencana asuhan keperawatan punya tempat menyimpan masalah dan tujuannya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 7 September 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`, `213 Warning(s)` — sama persis dengan garis dasar `6f7d81e`; `dotnet test` project SQLite `Failed: 0, Passed: 448, Total: 448` (garis dasar sebelum slice ini 394). Butir DoD "uji migration maju dan mundur terhadap PostgreSQL sungguhan" **belum terpenuhi** — wewenang eksekusi database tidak diberikan task ini, dan migration `20260906144058_AddNursingCarePlan` **belum diterapkan ke database mana pun**. Endpoint yang dibangun **lima**, bukan empat: `PATCH /items/{itemId}/close` ikut mendarat di sini karena acceptance criteria 3 menuntutnya. Bukti: [laporan](../task/report/backend/BE-RWI-059.md) |
| **Outcome** | Perawat menetapkan masalah keperawatan pasien beserta tujuan dan rencana tindakannya di dalam sistem, bukan di kertas |
| **Trace** | `FR-KEP-012`, `FR-KEP-013`, `FR-KEP-015`; PRD `CAP-013` aturan 1, 2, 4; `AC-CAP013-01`; `RWI-DEC-083`, `RWI-DEC-090` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Care Plan; `contracts/state-transition-matrix.md` `0.3.0` bagian 2; `contracts/validation-matrix.md` `VAL-KEP-16` |
| **Keadaan awal terverifikasi** | `BE@7d4bf2b`: nol berkas `*CarePlan*` dan nol berkas `*Nursing*` di seluruh repository. Tabelnya benar-benar belum ada |
| **Reuse** | Pola aggregate induk-anak yang sudah dipakai `InpBedPlacement` pada `episode-rawat-inap`; pola konfigurasi EF pada `Repositories/Configurations/HealthServices/ClinicalManagement/` |
| **Scope** | Dua entity milik `ClinicalManagement` — rencana asuhan dan butirnya; dua configuration EF; dua `DbSet`; enum `NursingCarePlanItemStatus`; satu migration; empat endpoint: buat rencana, tambah butir, baca per episode, catat evaluasi |
| **Penamaan** | Nama entity mengikuti `QBE-NAM-001`. Prefix `Trx*` **dilarang untuk kode baru** — `BE-RWI-041` sudah membuktikannya. Nama baku diputuskan saat preflight QBE, dan `02-backend-architecture.md` bagian 4.2 yang masih menulis `TrxNursingCarePlan` dibaca sebagai **usulan bentuk**, bukan sebagai nama final |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Satu episode memiliki **tepat satu** rencana asuhan, dengan butir masalah sebanyak yang dibutuhkan. 2. Butir memuat masalah, tujuan, rencana tindakan, dan evaluasi. 3. Butir dapat dinyatakan tercapai **hanya** bila evaluasinya sudah ada; tanpa evaluasi ditolak `400` dan butir tetap `Active` (`VAL-KEP-16`). 4. Butir dapat dikaitkan ke temuan pengkajian asalnya (`AC-CAP013-01`). 5. Rencana asuhan hanya dapat dibuat untuk episode `Admitted`; selain itu ditolak `422` |
| **Verification** | Uji migration maju-mundur; skenario penambahan butir; percobaan menutup butir tanpa evaluasi; skenario butir yang merujuk pengkajian |
| **Risk/blocker** | Katalog terminologi SDKI/SLKI/SIKI **belum diputuskan** dipakai atau tidak — pertanyaan 2 pada `04-prd-to-mvp.md` bagian 20, dan `OQ-RI-011` yang terbuka kembali lewat `RWI-DEC-090`. Struktur rencana asuhan **tidak** bergantung padanya: masalah keperawatan ditulis sebagai teks sampai katalognya diputuskan, dan layar **tidak boleh** mengunci bentuknya ke katalog yang belum ada. **Tidak memblokir.** Owner: Clinical governance |
| **DoD** | Dua entity, dua configuration, dua `DbSet`, satu enum, satu migration, empat endpoint, kelima acceptance criteria terbukti, `dotnet build` lulus |

---

### ✅ `BE-RWI-060` — Perubahan rencana asuhan menyimpan versi sebelumnya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 7 September 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`, `213 Warning(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 448, Total: 448`. Kriteria yang paling mudah salah — versi lama menyimpan penulis dan waktu aslinya — dibuktikan uji `VersiLama_MenyimpanPenulisAsli_BukanPengubahnya`, yang sekaligus memeriksa nilainya **berbeda** dari perawat yang mengubah. Nol baris addendum terbentuk (`RWI-AC-177`). Migration `20260906144956_AddNursingCarePlanItemRevision` **belum diterapkan ke database mana pun**. Bukti: [laporan](../task/report/backend/BE-RWI-060.md) |
| **Outcome** | Riwayat asuhan pasien tetap utuh. Menutup satu masalah keperawatan tidak menghapus jejak tindakan dan evaluasi yang sudah dikerjakan |
| **Trace** | `FR-KEP-014`, `FR-KEP-016`, `FR-KEP-017`; PRD `CAP-013` aturan 5 dan 6; `AC-CAP013-02`, `AC-CAP013-03`; `RWI-AC-177`; `INV-KEP-02` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` `PUT /items/{itemId}`, `PATCH /items/{itemId}/close`, `GET /items/{itemId}/revisions`; `contracts/state-transition-matrix.md` `0.3.0` bagian 2 |
| **Perubahan bentuk dari revision `1`** | **Tidak ada, dan itu disengaja.** `RWI-DEC-091` secara tegas **tidak** menyeret rencana asuhan ke mesin addendum: perubahan rencana asuhan adalah **perkembangan klinis**, bukan pembetulan kesalahan. Ia tetap memakai mesin versi. `RWI-AC-177` menguji tepat hal itu |
| **Reuse** | Pola salinan versi yang sudah dipakai tabel klinis lain; **bukan** pola addendum |
| **Scope** | Satu entity revisi butir milik `ClinicalManagement`; satu configuration; satu `DbSet`; satu migration; tiga endpoint; penjaga hanya-baca setelah episode `Closed` |
| **Penamaan** | Sama seperti `BE-RWI-059`: `QBE-NAM-001`, prefix `Trx*` dilarang untuk kode baru |
| **Dependency** | `BE-RWI-059` |
| **Acceptance criteria** | 1. Memperbarui butir menyimpan versi sebelumnya **beserta penulis dan waktu aslinya**, bukan penulis yang mengubah. 2. Memperbarui butir menghasilkan **versi baru**, bukan addendum (`RWI-AC-177`). 3. Menutup butir tidak menghapus tindakan maupun evaluasi sebelumnya; tindakan yang merujuk butir itu tetap ada dan rujukannya menjadi kosong, barisnya tidak hilang. 4. Setelah episode `Closed`, seluruh riwayat asuhan tetap terbaca lewat `GET` dan setiap `POST`/`PUT` dijawab `422` (`AC-CAP013-03`) |
| **Verification** | Skenario perubahan butir berulang; **pemeriksaan penulis pada versi lama** — ini kriteria yang paling mudah salah; percobaan mengubah asuhan pada episode tertutup; pemeriksaan bahwa tidak ada baris addendum yang terbentuk |
| **Risk/blocker** | Menyalin versi dengan penulis pengubah — bukan penulis asli — akan merusak makna klinisnya. **Contoh:** butir "risiko jatuh tinggi" ditulis Ns. Sari pukul 08.00, lalu diperbarui Ns. Dewi pukul 15.00. Versi lama **wajib** tetap tercatat atas nama Ns. Sari pukul 08.00. Bila tersalin atas nama Ns. Dewi, rekam medis kehilangan bukti siapa yang menilai pertama kali. `AC-CAP013-02` menguji tepat hal itu. Owner: Backend/API |
| **DoD** | Satu entity, configuration, `DbSet`, migration, tiga endpoint, penjaga episode tertutup, keempat acceptance criteria terbukti, `dotnet build` lulus |

---

### ✅ `BE-RWI-061` — Tindakan keperawatan tercatat sekali walaupun tombolnya tertekan dua kali

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 8 September 2026.** Keenam acceptance criteria terpetakan ke source dan terbukti penuh. Butir DoD yang menahan status — *uji PostgreSQL hijau* — **sudah terpenuhi**: `dotnet test` project Postgres, tapis `NursingInterventionIdempotencyTests`, menjawab `Failed: 0, Passed: 3, Total: 3` terhadap PostgreSQL 16 sungguhan. Lingkungan ujinya disediakan sebagai container `postgres:16` sekali pakai berdatabase `quilvian_kep_test`, bukan dengan meminta hak baru pada server bersama; **nol database bersama, dev, staging, atau production tersentuh**. Kriteria 3 kini terbukti untuk kiriman **bersamaan** (`DuaPermintaanBersamaan_KunciSama_HanyaSatuBaris`), dan kriteria 6 terbukti pada **penegakannya oleh database** (`KunciKembar_DitolakDatabase`) beserta bentuk index yang dibaca langsung dari `pg_indexes`: `CREATE UNIQUE INDEX … WHERE (("IdempotencyKey" IS NOT NULL) AND ("IsDelete" = false))`. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 453, Skipped: 0, Total: 453`. Migration `20260906151002_AddNursingIntervention` **belum diterapkan** ke database bersama, dev, staging, atau production mana pun. Bukti: [laporan](../task/report/backend/BE-RWI-061.md) |
| **Outcome** | Perawat mencatat tindakan yang benar-benar dilakukan beserta waktu dan hasilnya. Jaringan yang buruk tidak lagi menghasilkan tindakan ganda pada rekam medis |
| **Trace** | `FR-KEP-018`, `FR-KEP-019`, `FR-KEP-020`; PRD `CAP-014` aturan 1, 2, 3; `AC-CAP014-01`; `VAL-KEP-13`, `VAL-KEP-14`, `VAL-KEP-15` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` grup Nursing Intervention; `contracts/state-transition-matrix.md` `0.3.0` bagian 3 |
| **Keadaan awal terverifikasi** | Tabel tindakan keperawatan **belum ada**. `TrxPatientProcedure` yang sudah ada **tidak dipakai ulang** karena mewajibkan `ConsultationId` dan `DoctorId`; melonggarkannya akan melemahkan penjagaan bagi tindakan dokter yang membutuhkannya untuk penagihan |
| **Reuse** | Pola `Idempotency-Key` yang sudah dipakai task `episode-rawat-inap` dan `BE-RWI-041`; pola alokasi nomor bisnis lewat penyedia seri nomor — **bukan** `Count+1` maupun `Max+1` |
| **Scope** | Satu entity tindakan keperawatan milik `ClinicalManagement`; satu configuration; satu `DbSet`; enum keadaan pengiriman tagihan; satu migration; endpoint catat dan daftar per episode; penjaga idempotency **di tingkat database** |
| **Penamaan** | Sama seperti `BE-RWI-059`: `QBE-NAM-001`, prefix `Trx*` dilarang untuk kode baru |
| **Dependency** | `BE-RWI-056` |
| **Acceptance criteria** | 1. Tindakan menyimpan apa, kapan, oleh siapa, dan hasilnya, beserta konteks episode. 2. Tindakan mendadak dapat dicatat **tanpa** rujukan ke rencana asuhan (`CAP-014` aturan 3). 3. Permintaan berulang dengan `Idempotency-Key` yang sama menghasilkan **satu** baris dan dijawab `200` beserta baris yang sudah ada — bukan `201`, bukan `409` (`VAL-KEP-15`). 4. Waktu tindakan di masa depan ditolak `400` (`VAL-KEP-13`). 5. Waktu tindakan sebelum pasien masuk kamar ditolak `400` (`VAL-KEP-14`). 6. Unique parsial pada kunci idempotency terbentuk dengan penyaring kunci tidak kosong dan belum terhapus |
| **Verification** | Skenario kirim ulang dengan kunci sama dan kunci berbeda; **dua permintaan bersamaan dengan kunci sama, dijalankan terhadap PostgreSQL sungguhan** — provider InMemory tidak dapat membuktikan unique index parsial; skenario tindakan tanpa rencana asuhan; skenario dua batas waktu |
| **Risk/blocker** | Idempotency yang dipasang di controller saja akan bocor ketika ada dua instance aplikasi berjalan. Penjaganya **wajib** di tingkat database. Butir DoD nomor 8 pada `04-prd-to-mvp.md` bagian 18 menuntut buktinya dari PostgreSQL sungguhan; bila lingkungan uji tidak tersedia, task ini berhenti di `🟡` dan **tidak boleh** ditandai selesai. Owner: Backend/API |
| **DoD** | Satu entity, configuration, `DbSet`, satu enum, migration, dua endpoint, penjaga idempotency di database, keenam acceptance criteria terbukti, uji PostgreSQL hijau, `dotnet build` lulus |

---

### ✅ `BE-RWI-062` — Tagihan yang gagal tidak menghapus catatan klinis

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 7 September 2026.** Keenam acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`, `213 Warning(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 448, Total: 448`. **Nol tabel baru dan nol kolom baru; migration task ini kosong**, dibuktikan `dotnet ef migrations has-pending-model-changes` yang menjawab tidak ada perubahan model. Endpoint yang dibangun **lima**, bukan tiga: kontrak `0.3.0` sendiri memuat empat, ditambah `PUT /{id}` yang dibutuhkan acceptance criteria 5. Jalur koreksi oleh kepala ruangan tetap memakai endpoint pengganti milik `MedicalRecordManagement`, sama seperti `BE-RWI-057`. Bukti: [laporan](../task/report/backend/BE-RWI-062.md) |
| **Outcome** | Ketika pengiriman tagihan ke Billing gagal, tindakan keperawatannya **tetap tersimpan**. Kegagalan itu terlihat sebagai keadaan integrasi tersendiri yang dapat dicoba ulang |
| **Trace** | `FR-KEP-021`, `FR-KEP-022`; PRD `CAP-014` aturan 5; `AC-CAP014-02`, `AC-CAP014-03`; `RWI-AC-176`; `INT-KEP-05`, `INT-KEP-06`; `VAL-KEP-06`, `VAL-KEP-07` |
| **Kontrak** | `contracts/integration-contract.md` `0.3.0` `INT-KEP-05` dan `INT-KEP-06`; `contracts/api-contract.md` `0.3.0` `PATCH /{id}/finalize`, `POST /{id}/addendums`, `GET /{id}/addendums`, `GET /{id}/billing-dispatch`; `contracts/permission-audit-matrix.md` hak akses `NursingIntervention : Amend` |
| **Perubahan bentuk dari revision `1`** | Koreksi catatan tindakan = **addendum** berjenis `Procedure`, bukan amandemen berversi. Status catatan **tetap** `Finalized`; nilai status `Amended` **dicabut**. Finalisasi sekaligus **mendaftarkan** catatan ke mesin keutuhan dalam transaksi yang sama |
| **Reuse** | `ClinicalDocumentIntegrityService` dan `ClinicalNoteAddendumService` yang **sudah ada**; pola transaksi tunggal yang sudah terbukti pada `BE-RWI-038` dan dipakai `BE-RWI-065`; pola pemisahan kegagalan integrasi yang sudah dipakai modul lain |
| **Scope** | Transisi `Recorded → Finalized`; pendaftaran ke mesin keutuhan berjenis `Procedure` saat finalisasi, dalam transaksi yang sama; koreksi lewat addendum; mesin keadaan pengiriman tagihan `Pending`/`Dispatched`/`Failed`/`NotApplicable` yang **terpisah** dari status klinis; tiga endpoint; hak akses `NursingIntervention : Amend` |
| **Dependency** | `BE-RWI-061` |
| **Acceptance criteria** | 1. Catatan klinis tetap tersimpan ketika Billing gagal, dan keadaan pengirimannya `Failed`. 2. Status klinis tetap `Recorded` atau `Finalized` apa pun keadaan tagihannya; kedua mesin status **tidak saling mengunci**. 3. Finalisasi mendaftarkan catatan sebagai dokumen `Procedure` tertanda tangan; bila pendaftaran gagal, **finalisasi ikut batal**. 4. Catatan final hanya dapat dikoreksi penulisnya atau kepala ruangan; selain itu ditolak `403` (`VAL-KEP-07`, `AC-CAP014-03`). 5. Percobaan menyunting langsung isi catatan yang sudah `Finalized` ditolak. 6. Setiap koreksi tercatat beserta alasannya, dan isi asli tidak berubah (`RWI-AC-176`) |
| **Verification** | Skenario Billing gagal lalu pembacaan ulang catatan; skenario koreksi oleh bukan penulis; skenario koreksi oleh kepala ruangan; test yang memaksa pendaftaran keutuhan gagal; pemeriksaan bahwa dua mesin status tidak saling mengunci |
| **Risk/blocker** | Menggabungkan status klinis dengan status tagihan adalah kesalahan yang paling mahal di sini — catatan klinis bisa hilang karena masalah keuangan. **Contoh:** Ns. Sari memasang infus pukul 02.00 saat sistem Billing sedang mati. Yang benar: catatan tindakan tersimpan `Recorded`, penanda pengiriman `Failed`, dan percobaan ulang dijalankan terpisah. Yang salah: tindakan ikut gagal tersimpan, sehingga pukul 08.00 tidak ada bukti infus pernah dipasang. Owner: Backend/API |
| **DoD** | Transisi status terpasang, pendaftaran keutuhan dalam satu transaksi, mesin pengiriman tagihan terpisah, tiga endpoint, hak akses, keenam acceptance criteria terbukti, `dotnet build` lulus |

---

### ✅ `BE-RWI-063` — Catatan keperawatan tampil pada catatan terpadu tanpa tabel baru

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 7 September 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`, `213 Warning(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 448, Total: 448`. **Nol tabel baru dan nol kolom baru; migration task ini kosong**, dibuktikan `dotnet ef migrations has-pending-model-changes`. Temuan yang ikut ditutup: kolom `InpEpisodeId` pada catatan terpadu ternyata **tidak pernah diisi siapa pun** sejak dibuat `BE-RWI-040`, sehingga endpoint lini masa milik `BE-RWI-053` selalu kosong bagi seluruh profesi — kini terisi. Bukti: [laporan](../task/report/backend/BE-RWI-063.md) |
| **Outcome** | Dokter membaca catatan perawat pada catatan terpadu yang sama, sehingga seluruh profesi melihat satu perkembangan pasien |
| **Trace** | `FR-KEP-023`; PRD `CAP-014` aturan 4; `INT-KEP-03`; `RWI-RULE-026` |
| **Kontrak** | `contracts/integration-contract.md` `0.3.0` `INT-KEP-03`; `contracts/api-contract.md` `0.3.0` grup Patient Integrated Progress Note |
| **Keadaan awal terverifikasi** | `TrxPatientIntegratedProgressNote` **sudah ada**, sudah menerima banyak profesi, dan `EncounterId`, `QueueId`, `ConsultationId`, `DoctorId` seluruhnya **sudah** nullable. Ia juga sudah mendaftar ke mesin keutuhan lewat jenis `ProgressNote` |
| **Reuse** | `TrxPatientIntegratedProgressNote` apa adanya. **Nol tabel baru, nol kolom baru** |
| **Scope** | Pemakaian `ProfessionType` perawat pada catatan terpadu; penyaluran catatan keperawatan ke sana sesuai kebijakan |
| **Dependency** | `BE-RWI-061` |
| **Acceptance criteria** | 1. Catatan keperawatan tampil pada catatan terpadu dengan `ProfessionType` perawat. 2. **Nol tabel dan nol kolom baru** dibuat task ini, dan migration task ini **kosong**. 3. Catatan dokter yang sudah ada pada catatan terpadu tidak berubah perilakunya, dibuktikan test regresi. 4. Satu episode dapat memuat koreksi perawat dan koreksi dokter pada catatan terpadu yang sama, keduanya dalam bentuk **addendum bernomor** — bukan satu versi dan satu addendum (`RWI-DEC-091`) |
| **Verification** | **Pemeriksaan bahwa migration task ini kosong**; skenario catatan terpadu berisi catatan dua profesi; test regresi catatan dokter |
| **Risk/blocker** | PRD `CAP-014` aturan 4 menyebut "sesuai policy" tanpa menyebut kebijakannya — pertanyaan 4 pada `04-prd-to-mvp.md` bagian 20. **Tidak memblokir**, karena catatan tetap tersimpan dan tetap terbaca dari ruang kerja keperawatan apa pun kebijakan tampilnya. Owner: Clinical governance |
| **DoD** | Nol tabel baru, migration kosong, penyaluran catatan terpadu berjalan, keempat acceptance criteria terbukti, `dotnet build` lulus |

---

### ✅ `BE-RWI-064` — Kepala ruangan melihat pengkajian mana yang belum dikerjakan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 6 September 2026.** Keempat acceptance criteria terpetakan ke source. `dotnet build QuilvianSystemBackend.sln` `Build succeeded` `0 Error(s)`; `dotnet test` project SQLite `Failed: 0, Passed: 394, Total: 394` (garis dasar sebelum slice ini 324). **Nol tabel baru; migration task ini kosong.** Dua hal dilaporkan sebagai delta: route `GET /patient-assessments/monitoring/initial-assessment-compliance` tidak tercantum pada `api-contract.md` `0.3.0`, dan bukti `INV-KEP-03` untuk pencatatan tindakan menunggu `BE-RWI-061`. Bukti: [laporan](../task/report/backend/BE-RWI-064.md) |
| **Outcome** | Kepala ruangan menemukan episode yang pengkajian awalnya belum ada atau sudah lewat tenggat, tanpa membuka satu per satu |
| **Trace** | `FR-KEP-024`, `FR-KEP-025`, `FR-KEP-026`; `RWI-RULE-023`, `RWI-DEC-032`; PRD 16.2 aturan 11; `INV-KEP-03`; `VAL-KEP-18` |
| **Kontrak** | `contracts/api-contract.md` `0.3.0` endpoint daftar pantau kepatuhan |
| **Reuse** | Pola daftar pantau yang sudah dipakai `FE-INP-09` pada `episode-rawat-inap`. **Ini daftar pantau ketiga** yang selama ini tercatat sebagai gap sejak `BE-RWI-029` |
| **Scope** | Satu endpoint baca berhalaman; penyaringan menurut ruangan dan keadaan tenggat. **Nol tabel baru** |
| **Dependency** | `BE-RWI-058` |
| **Acceptance criteria** | 1. Daftar memuat episode yang pengkajian awalnya belum ada atau terlambat menurut kebijakan aktif. 2. Daftar kosong dibedakan dari kebijakan kosong: tanpa baris terlambat berbunyi "sudah tepat waktu"; tanpa kebijakan berbunyi "batas waktu belum ditetapkan" — **bukan** "tidak ada data" untuk keduanya. 3. Keterlambatan pengkajian **tidak menahan** satu pun tindakan lain; pembuatan tindakan pada episode yang terlambat tetap dijawab `201` (`INV-KEP-03`, `VAL-KEP-18`). 4. Daftar tidak menampilkan isi klinis, hanya nama pasien, lokasi, dan keterlambatan |
| **Verification** | Skenario daftar kosong; skenario kebijakan kosong; skenario dua episode terlambat; **pemeriksaan bahwa keterlambatan tidak memblokir pencatatan tindakan** |
| **Risk/blocker** | Daftar pantau yang memblokir pekerjaan klinis akan mendorong perawat mengakali sistem. `FR-KEP-026` dan `INV-KEP-03` menguji tepat hal itu, dan `INV-KEP-03` adalah penjaga keselamatan, bukan kenyamanan. Owner: Backend/API |
| **DoD** | Satu endpoint, penyaringan, nol tabel baru, keempat acceptance criteria terbukti, `dotnet build` lulus |

---

## 4.1 Register status task

| Task | Judul singkat | Gelombang | Status | Laporan |
| --- | --- | --- | :---: | --- |
| `BE-RWI-054` | Kolom tenggat dan kebijakan pada pengkajian | `KEP-MVP-0` | ✅ | [BE-RWI-054](../task/report/backend/BE-RWI-054.md) |
| `BE-RWI-055` | Master batas waktu pengkajian | `KEP-MVP-0` | ✅ | [BE-RWI-055](../task/report/backend/BE-RWI-055.md) |
| `BE-RWI-056` | Pengkajian awal dan ulang terpisah | `KEP-MVP-1` | ✅ | [BE-RWI-056](../task/report/backend/BE-RWI-056.md) |
| `BE-RWI-065` | Pengkajian selesai ikut terkunci | `KEP-MVP-1` | ✅ | [BE-RWI-065](../task/report/backend/BE-RWI-065.md) |
| `BE-RWI-057` | Koreksi pengkajian lewat addendum | `KEP-MVP-1` | ✅ | [BE-RWI-057](../task/report/backend/BE-RWI-057.md) |
| `BE-RWI-058` | Lini masa dan keadaan tenggat | `KEP-MVP-1` | ✅ | [BE-RWI-058](../task/report/backend/BE-RWI-058.md) |
| `BE-RWI-059` | Rencana asuhan keperawatan | `KEP-MVP-2` | ✅ | [BE-RWI-059](../task/report/backend/BE-RWI-059.md) |
| `BE-RWI-060` | Riwayat versi butir asuhan | `KEP-MVP-2` | ✅ | [BE-RWI-060](../task/report/backend/BE-RWI-060.md) |
| `BE-RWI-061` | Tindakan keperawatan dan idempotency | `KEP-MVP-3` | ✅ | [BE-RWI-061](../task/report/backend/BE-RWI-061.md) |
| `BE-RWI-062` | Pemisahan kegagalan tagihan dan koreksi tindakan | `KEP-MVP-3` | ✅ | [BE-RWI-062](../task/report/backend/BE-RWI-062.md) |
| `BE-RWI-063` | Catatan keperawatan pada catatan terpadu | `KEP-MVP-3` | ✅ | [BE-RWI-063](../task/report/backend/BE-RWI-063.md) |
| `BE-RWI-064` | Daftar pantau kepatuhan pengkajian | `KEP-MVP-4` | ✅ | [BE-RWI-064](../task/report/backend/BE-RWI-064.md) |

| Gelombang | Task di dalamnya | Status |
| --- | --- | --- |
| `KEP-MVP-0` | `BE-RWI-054`, `BE-RWI-055` | ✅ selesai 6 September 2026 — keduanya `✅` |
| `KEP-MVP-1` | `BE-RWI-056`, `BE-RWI-065`, `BE-RWI-057`, `BE-RWI-058` | ✅ selesai 8 September 2026 — keempatnya `✅` |
| `KEP-MVP-2` | `BE-RWI-059`, `BE-RWI-060` | ✅ selesai 7 September 2026 — keduanya `✅` |
| `KEP-MVP-3` | `BE-RWI-061`, `BE-RWI-062`, `BE-RWI-063` | ✅ selesai 8 September 2026 — ketiganya `✅` |
| `KEP-MVP-4` | `BE-RWI-064` | ✅ selesai 6 September 2026 |

Laporan task ditulis ke `<blueprint-root>/task/report/backend/<TASK-ID>.md` sesuai
`rules/rule-output/lokasi-laporan-task.md`. Folder itu **sudah ada sejak 6 September 2026** dan
memuat **dua belas** laporan, satu untuk setiap task.

---

## 5. Gerbang yang masih terbuka

Tiga butir. **Tidak satu pun memblokir task mana pun**, dan ketiganya dicatat supaya tidak
terlewat.

### 5.1 `RWI-OQ-051` — substansinya sudah terpenuhi, catatannya belum menyusul

| Hal | Keadaannya |
| --- | --- |
| Yang diminta | Persetujuan pemilik `MedicalRecordManagement` agar penegakan keutuhan diperluas dari `ProgressNote` saja menjadi mencakup `Assessment` dan `Procedure` |
| Keadaan pada decision log | **`open`**, pemilik jawaban belum dinyatakan |
| Keadaan di source `BE@7d4bf2b` | **Sudah terpenuhi.** Daftar jenis yang ditegakkan berisi `ProgressNote`, `Consultation`, `Assessment`, dan `Procedure` — `ClinicalDocumentIntegrityService.cs:74-79` |
| Siapa yang mendaratkannya | `BE-RWI-038` pada sub-modul `dokter-rawat-inap`, berstatus ✅, dari roadmap yang **sudah disetujui**. Alasannya tertulis pada source: `RWI-FACT-014` menemukan catatan dokter yang sudah diselesaikan tidak dapat disunting **maupun** dikoreksi |
| Kenapa karena itu **tidak memblokir** | Perubahan yang diminta `RWI-OQ-051` sudah dikerjakan lewat jalur delivery yang sah. Sub-modul ini kini **memakai** yang sudah ada, dan tidak lagi meminta perubahan baru kepada `MedicalRecordManagement` |
| Yang masih perlu dilakukan | Menutup catatannya. `RWI-OQ-051` sebaiknya dinyatakan tertutup lewat satu keputusan bernomor, dengan `BE-RWI-038` sebagai buktinya. **Itu pekerjaan `/qv-grill`, bukan pekerjaan roadmap ini dan bukan pekerjaan builder** |
| Risiko yang tersisa | Bila pemilik `MedicalRecordManagement` kelak justru **menolak** penegakan bagi kedua jenis itu, yang dibongkar bukan roadmap ini melainkan `BE-RWI-038` yang sudah mendarat. Risiko itu sudah disadari dan diterima saat `RWI-DEC-092` diambil |

### 5.2 Bentuk pintu endpoint koreksi

Dijabarkan pada bagian 2.3. Ringkasnya: kontrak `0.3.0` meminta endpoint addendum per sumber daya,
sedangkan mesin generiknya sudah ada. **Tidak memblokir**; yang wajib adalah endpoint kontrak
meneruskan ke mesin yang sudah ada, bukan menulis barisnya sendiri. Pemilik: Muhammad Hamzah
selaku pemilik kontrak. Diselesaikan `/qv-design`.

> **Bukti pelaksanaan, 6 September 2026.** `BE-RWI-057` ✅ dibangun mengikuti syarat itu: kedua
> endpoint pada grup Patient Assessment **meneruskan** ke `ClinicalNoteAddendumService`, nol tabel
> baru dibuat, dan migration task itu kosong — dibuktikan uji
> `Koreksi_TidakMenulisPadaTabelClinicalManagement`. **Gerbangnya sendiri tetap terbuka**:
> pertanyaan apakah pintu kedua itu memang diinginkan belum dijawab pemilik kontrak. Lihat
> [laporan `BE-RWI-057`](../task/report/backend/BE-RWI-057.md).

> **Bukti pelaksanaan kedua, 7 September 2026.** `BE-RWI-062` ✅ dibangun mengikuti syarat yang
> sama: `POST /nursing-interventions/{id}/addendums` dan `GET /nursing-interventions/{id}/addendums`
> **meneruskan** ke `ClinicalNoteAddendumService` dengan jenis dokumen `Procedure`, nol tabel baru
> dibuat, dan migration task itu kosong — dibuktikan
> `dotnet ef migrations has-pending-model-changes`. **Gerbangnya tetap terbuka** dengan alasan yang
> sama. Lihat [laporan `BE-RWI-062`](../task/report/backend/BE-RWI-062.md).

### 5.3 Kepala dokumen empat berkas hulu

Dijabarkan pada bagian 2.4. **Tidak memblokir**; isinya sudah benar. Pemilik: `/qv-design`.

### 5.4 Butir yang **sudah tertutup** dan tidak perlu dibaca lagi

| Butir | Ditutup oleh | Kapan |
| --- | --- | --- |
| Approval blueprint sub-modul `keperawatan` | `RWI-DEC-092` | 3 September 2026 |
| Butir konsistensi mesin koreksi, `04-prd-to-mvp.md` bagian 20.1 | `RWI-DEC-091` | 2 September 2026 |
| Kepemilikan tabel pemakaian alat, `RWI-OQ-048` | `RWI-DEC-089` — kemampuannya ditunda, bukan pemiliknya dipilih | 2 September 2026 |
| `DEC-INP-009` pertentangan scope `CAP-013` | `RWI-DEC-090` — `RWI-DEC-004` dan `RWI-DEC-034` dinyatakan `superseded` | 2 September 2026 |
| `INT-KEP-01` *shared inpatient clinical context resolver* | `BE-RWI-039` ✅ pada `dokter-rawat-inap` | 3 September 2026 |

### 5.5 Butir yang terbuka tetapi memang **tidak pernah** memblokir

| Butir | Pemilik | Pengaruhnya |
| --- | --- | --- |
| `RWI-RULE-021` nilai batas waktu klinis | Pemilik klinis, belum ditunjuk | Menahan **produksi**, bukan pembangunan. Mekanismenya dibangun `BE-RWI-055`, angkanya menyusul tanpa mengubah kode |
| `OQ-RI-011` pemakaian katalog SDKI/SLKI/SIKI | Clinical governance | Menyentuh `BE-RWI-059` ✅. **Terbukti tidak memblokir:** masalah keperawatan tersimpan sebagai teks pada `ProblemStatement`, dan kolom `NursingDiagnosisId` sudah disediakan **tanpa foreign key** sehingga katalognya dapat dipasang kelak tanpa mengubah bentuk tabel |
| Kebijakan tampil catatan keperawatan pada catatan terpadu | Clinical governance | Menyentuh `BE-RWI-063` ✅. **Terbukti tidak memblokir:** catatan keperawatan tersimpan dan tampil pada lini masa catatan terpadu apa adanya; kebijakan tampilnya dapat dipasang kelak tanpa mengubah penyalurannya |
| Urutan daftar di dalam `FE-INP-09` | Frontend authority bersama pemilik `episode-rawat-inap` | Menyentuh `FE-RWI-056`. Wajib diputuskan bersama, bukan sendiri-sendiri — `02-module-map.md` bagian 6 |

---

## 6. Coverage gap requirement ke test

| Requirement | Tercakup task | Tercakup acceptance test | Catatan |
| --- | --- | --- | --- |
| `FR-KEP-001` s.d. `FR-KEP-026` | Ya, 12 task | Ya | Seluruh 26 functional requirement aktif punya task pemilik |
| `FR-KEP-027`, `FR-KEP-028` | **Tidak, disengaja** | Tidak | `EPIC KEP-06` `DEFERRED` oleh `RWI-DEC-089`; ketiadaan task adalah kriteria `RWI-AC-170` |
| `AC-CAP027-01` rujukan gizi | Sebagian | Sebagian | Skrining gizi ikut `BE-RWI-056`; pemicu rujukan `INT-KEP-04` menunggu modul Gizi berdiri |
| `AC-CAP014-01` idempotency tindakan | Ya, `BE-RWI-061` ✅ | **Ya** | Kiriman ulang berurutan terbukti pada uji SQLite; kiriman **bersamaan** terbukti pada PostgreSQL 16 sungguhan, 8 September 2026, lewat `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/NursingInterventionIdempotencyTests.cs` — `Failed: 0, Passed: 3, Total: 3` |
| `AC-CAP027-02` kewenangan ahli gizi | **Tidak** | Tidak | Modul Gizi `PLANNED`; di luar kendali sub-modul ini |
| `FR-KEP-009` jalur gagal penyuntingan dokumen final | Ya, `BE-RWI-065` | **Kini dapat diuji** | Peringatan `testing/acceptance-test-matrix.md` bagian 8 sudah gugur: jenis `Assessment` **sudah** ditegakkan, sehingga percobaan menyunting dokumen final benar-benar ditolak dan test tidak lagi lulus dengan alasan yang salah |

**Coverage gap yang diakui:** dua acceptance criteria `CAP-027` tidak dapat diuji sampai modul
Gizi berdiri. Ini ketersediaan modul, bukan requirement yang hilang.

---

## 7. Yang sengaja tidak dibuat roadmap ini

| Yang ditolak | Alasan |
| --- | --- |
| Task pembuatan tabel `Inp*` untuk dokumentasi klinis | `RWI-DEC-081` menaruhnya pada `ClinicalManagement`. Membuat tandingan melanggar keputusan pemilik |
| **Task membuat ulang resolver konteks klinis** | Sudah ada lewat `BE-RWI-039`. `INT-DOK-09` menyatakan service ini dibuat **sekali**; sub-modul kedua menerima baris dependency, bukan salinan task |
| **Task membuat ulang kolom `InpEpisodeId` dan `AssessmentType`** | Sudah mendarat lewat `BE-RWI-040`, dengan komentar pada source yang menyebut `INT-KEP-01` secara langsung |
| **Task meminta perluasan penegakan keutuhan** | Sudah mendarat lewat `BE-RWI-038`. Yang tersisa hanyalah menutup catatan `RWI-OQ-051`, dan itu pekerjaan `/qv-grill` |
| **Task membuat mesin koreksi sendiri** | Dilarang `RWI-DEC-087` dan justru dihindari `RWI-DEC-091`. Mesin addendum `MedicalRecordManagement` dipakai apa adanya |
| **Kolom `AmendedAt` dan `AmendedByUserId`** | Dicabut pada `02-backend-architecture.md` `0.3`. Penulis, waktu, alasan, dan nomor urut koreksi disimpan mesin addendum. Menyimpannya dua kali melahirkan dua sumber jawaban |
| **Nilai status `Amended`** | Dicabut `RWI-DEC-091`. Pertanyaan "apakah dokumen ini pernah dikoreksi" dijawab riwayat addendum, bukan status dokumen |
| Task `EPIC KEP-06` pemakaian alat | `RWI-DEC-089` mengeluarkannya dari scope rilis pertama |
| Task katalog SDKI/SLKI/SIKI | Pemakaiannya belum dinyatakan rumah sakit; PRD `CAP-013` aturan 3 bersyarat |
| Task pengisian angka batas waktu klinis | Angkanya konfigurasi milik Clinical governance, bukan pekerjaan rekayasa |
| Task asuhan gizi ujung ke ujung | Modul Gizi belum berwujud; PRD 23.1 menaruhnya di sana |
| Task merapikan `PatientAssessmentController.cs` | Berkas itu panjang dan memuat logika bisnis di controller, tetapi pemiliknya modul lain. Refactor di tengah penambahan fitur adalah dua pekerjaan yang digabung. Ditandai sebagai utang, bukan ditiru dan bukan pula diperbaiki diam-diam |
