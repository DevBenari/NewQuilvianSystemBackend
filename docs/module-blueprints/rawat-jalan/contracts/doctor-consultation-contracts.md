# Doctor Consultation Contracts — Rawat Jalan Clinical

| Field | Nilai |
|---|---|
| Contract versions | `RJ-DOC-COMPLETION-001@1.0.0`, `RJ-DOC-HANDOFF-001@1.0.0` |
| Status | **`FROZEN`** |
| Frozen at | `2026-08-31` |
| Frozen by | Owner approval Sukma Giri — `RJ-DOC-DEC-001` |
| Owner | Doctor / Clinical, bersama API authority |
| Scope | Doctor / Rawat Jalan Clinical — `RJ-DOC` |
| Evidence backend | `801a4f52459e1251ec9bb03c1abfe5e17dd3639c` cabang `sukmagp` |
| Evidence frontend | `baca9650848ded164538ab85405190fafe8785a3` cabang `QuilvianDevV2` |
| Membuka | `RJ-DOC-BE-001`, `BE-002`, `BE-003`, `BE-005`, dan turunannya |

> **`FROZEN` berarti kontrak tidak boleh berubah tanpa revisi versi dan approval owner.** Ia
> **tidak** memberi implementation authority. Setiap task tetap memerlukan handoff, wewenang tulis,
> dan preflight tersendiri.
>
> Kontrak ini dibekukan dari **source yang sudah ada**. Tidak ada field, DTO, route, atau perilaku
> baru yang dikarang; setiap butir menunjuk bukti `file:line`. Yang belum ada pada source ditandai
> `TARGET` secara eksplisit.

---

# Bagian 1 — `RJ-DOC-COMPLETION-001@1.0.0`

## Completion Contract — `RJ-DOC-INT-001`

### 1.1 Canonical endpoint

```text
PATCH /api/v1/health-services/clinical-management/doctor-consultations/{id}/complete
```

| Aspek | Nilai | Evidence |
|---|---|---|
| Route prefix | `api/v1/health-services/clinical-management/doctor-consultations` | `DoctorConsultationController.cs:29` |
| Method dan path | `[HttpPatch("{id:guid}/complete")]` | `:590` |
| Nama parameter path | **`id`**, bertipe `guid` — **bukan** `consultationId` | `:590`, `:597` |
| Authentication | `[Authorize]` pada level controller | `:28` |
| Authorization | `[AccessPermission("DoctorConsultation", "Update")]` | `:595` |
| Handler | `ConsultationFinalizationService.FinalizeAsync` | `:601` |
| Envelope response | `ApiResponse<T>` — `Success`, `StatusCode`, `Message`, `Data`, `Errors`, `Timestamp` | `Responses/ApiResponse.cs:5-15` |

> Penamaan `{id}` dipertahankan apa adanya. Mengubahnya menjadi `{consultationId}` adalah breaking
> change tanpa manfaat teknis, dan governance melarang rename API tanpa alasan.

### 1.2 Identity

| Identitas | Sumber | Aturan |
|---|---|---|
| `ConsultationId` | path parameter `{id}` | Wajib. Aggregate root finalisasi |
| `EncounterId` | dibaca server dari `TrxDoctorConsultation.EncounterId` | **Tidak** diterima dari payload. Konsultasi memiliki tepat satu encounter |
| `QueueId` | dibaca server dari `TrxDoctorConsultation.QueueId` | **Nullable** — pasien IGD tidak berantre (`TrxDoctorConsultation.cs`, `BE-IGD-028`, `FR-IGD-062`). Efek antrean hanya diterapkan bila `Queue != null` |
| Actor | `User.FindFirstValue(ClaimTypes.NameIdentifier)` | **Server-side authoritative.** Tidak pernah dibaca dari payload client | 

Evidence actor: `DoctorConsultationController.cs:1404-1411`, dipanggil pada `:599`.

> **`TARGET` — celah actor kosong.** `GetCurrentUserId` mengembalikan `Guid.Empty` bila klaim tidak
> dapat di-parse, sehingga `CompletedByUserId` berpotensi tersimpan `Guid.Empty`. Kontrak
> mewajibkan finalisasi **ditolak** ketika actor tidak dapat ditentukan; audit `CompletedBy` tidak
> boleh menunjuk pengguna kosong. Ditutup pada `RJ-DOC-BE-001`.

### 1.3 Request contract

Tipe: `FinalizeDoctorConsultationRequest : CompleteDoctorConsultationRequest`
(`ConsultationFinalizationDtos.cs:39-47`).

**Field kontrol finalisasi:**

| Field | Tipe | Wajib | Makna |
|---|---|---|---|
| `ExpectedUpdatedAt` | `DateTime?` | opsional pada source, **`TARGET` wajib** | Concurrency token canonical. Dibandingkan dengan `TrxDoctorConsultation.UpdateDateTime` |
| `AcknowledgedWarningKeys` | `List<string>` | ya, boleh kosong | Berisi `IssueKey` warning yang sudah diakui pengguna secara sadar |
| `FinalizationNote` | `string?` `MaxLength(1000)` | opsional | Digabung ke `DoctorNote` dengan awalan `Finalisasi:` |

**Field dokumentasi klinis** (diwarisi `CompleteDoctorConsultationRequest`,
`DoctorConsultationDtos.cs:313-358`) — seluruhnya opsional, dan `null` berarti *pertahankan nilai
tersimpan*, bukan *kosongkan*:

`ChiefComplaint` `1000` · `HistoryOfPresentIllness` `4000` · `PhysicalExamination` `4000` ·
`Subjective` `4000` · `Objective` `4000` · `Assessment` `4000` · `Plan` `4000` ·
`ProcedurePlan` `2000` · `PrescriptionPlan` `2000` · `SupportingExamPlan` `2000` ·
`ReferralPlan` `2000` · `EducationPlan` `2000` · `FollowUpDate` `DateTime?` ·
`FollowUpNote` `500` · `DoctorNote` `1000`

Semantik `null` terbukti pada `ConsultationFinalizationService.ApplyRequest`, yang memakai pola
`Normalize(request.X) ?? entity.X`.

**Tidak ada field baru yang ditambahkan.** Payload tidak boleh memuat actor, `EncounterId`,
`QueueId`, status, timestamp penyelesaian, maupun field finansial apa pun.

### 1.4 `409 Conflict` — concurrency contract

```text
HTTP 409  =  stale / concurrent consultation state
```

| Aspek | Aturan |
|---|---|
| Pemicu | `ExpectedUpdatedAt` tidak sama dengan `UpdateDateTime` tersimpan (dibandingkan sebagai UTC) |
| Body | `ApiResponse<object>.Fail(409, message)` — `Data` `null` |
| Evidence | `ConsultationFinalizationService.cs:56-60`; controller `:608-614` |
| Efek | **Tidak ada tulisan sama sekali.** Transaksi tidak melanjutkan finalisasi |

Kewajiban frontend:

1. memberitahu bahwa data konsultasi berubah;
2. memuat ulang consultation authoritative dari server;
3. **tidak** melakukan silent overwrite;
4. **tidak** otomatis mengirim ulang dengan versi baru tanpa kesadaran pengguna ketika perubahan
   yang terdeteksi menyentuh isi klinis.

> **`TARGET`.** Pada source, `ExpectedUpdatedAt` bersifat opsional: bila tidak dikirim, tidak ada
> pemeriksaan sama sekali (`:56` mensyaratkan `HasValue`). Kontrak membekukannya menjadi **wajib**,
> sehingga permintaan tanpa concurrency token ditolak. Ditutup pada `RJ-DOC-BE-003`.

### 1.5 `400 Bad Request` — validation contract

```text
HTTP 400  =  clinical validation failed   (BUKAN system error)
```

Body: `ApiResponse<ConsultationFinalizationValidationResponse>` — dikirim melalui `.Ok(...)` di
dalam `BadRequest(...)`, sehingga `Data` **terisi** dan dapat dibaca frontend
(`DoctorConsultationController.cs:620-625`).

`ConsultationFinalizationValidationResponse` (`ConsultationFinalizationDtos.cs:29-37`):

| Field | Tipe |
|---|---|
| `ConsultationId` | `Guid` |
| `CanFinalize` | `bool` |
| `RequiresWarningAcknowledgement` | `bool` |
| `ErrorCount`, `WarningCount`, `InformationCount` | `int` |
| `Sections` | `List<ConsultationFinalizationSectionResponse>` |

`ConsultationFinalizationSectionResponse`: `Section`, `TabKey`, `ErrorCount`, `WarningCount`,
`InformationCount`, `Issues`.

`ConsultationFinalizationIssueResponse` (`:6-17`) — inilah yang mengarahkan dokter ke tempat yang
tepat:

| Field | Kegunaan bagi frontend |
|---|---|
| `IssueKey` | Identitas stabil, format `{Code}:{EntityType\|Section}:{EntityId\|"general"}` (`ConsultationValidationService.cs:126`). **Nilai inilah yang dikirim balik pada `AcknowledgedWarningKeys`** |
| `Code` | Kode mesin, mis. `MISSING_PRIMARY_DIAGNOSIS` |
| `Severity` | `Error` · `Warning` · `Information` |
| `Section` | Pengelompokan, mis. `SOAP`, `Diagnosis`, `Procedure` |
| `TabKey` | Tab tujuan, mis. `soap`, `diagnosis`, `procedure` |
| `Field` | Field spesifik bila ada |
| `Message` | Teks untuk pengguna |
| `EntityType`, `EntityId` | Baris yang bermasalah, mis. `PatientProcedure` |

Dua sebab penolakan yang **berbeda** dan wajib dibedakan frontend:

| Sebab | Penanda | Perlakuan |
|---|---|---|
| Ada error | `ErrorCount > 0` | Wajib diperbaiki. Tidak dapat di-acknowledge |
| Warning belum diakui | `RequiresWarningAcknowledgement = true`, `ErrorCount = 0` | Kirim ulang dengan `IssueKey` warning di `AcknowledgedWarningKeys` |

Evidence: `ConsultationFinalizationService.cs:78-92`.

Aturan validasi yang dibekukan dari source (`ConsultationValidationService.cs`):

| Kode | Severity | Aturan |
|---|---|---|
| `CONSULTATION_NOT_FOUND` | `Error` | Konsultasi tidak ada |
| `CONSULTATION_ALREADY_COMPLETED` | `Error` | Sudah `Completed` |
| `CONSULTATION_CANCELLED` | `Error` | Sudah `Cancelled` |
| `MISSING_SUBJECTIVE` | `Error` | `Subjective` dan `ChiefComplaint` sama-sama kosong |
| `MISSING_OBJECTIVE` | `Error` | `Objective` dan `PhysicalExamination` sama-sama kosong |
| `MISSING_ASSESSMENT` | `Error` | `Assessment` kosong |
| `MISSING_PLAN` | `Error` | `Plan` kosong |
| `MISSING_PRIMARY_DIAGNOSIS` | `Error` | Tidak ada diagnosis utama |
| `INVALID_PROCEDURE_QUANTITY` | `Error` | Jumlah tindakan `<= 0` |
| `MISSING_PROCEDURE_TARIFF` | `Error` | Tarif tindakan belum tersedia |
| `UNAPPROVED_PROCEDURE` | `Error` | Tindakan menuntut approval yang belum ada |
| — | — | Ditambah keluaran `PrescriptionValidationService.ValidateForConsultationAsync` |

### 1.6 Aturan stabilitas clinical order — `RJ-DOC-OQ-005`

Dibekukan dari keputusan owner `RJ-DOC-DEC-004`.

```text
DOCTOR ORDER CREATION MUST BE STABLE
ANCILLARY EXECUTION DOES NOT NEED TO BE FINISHED
```

**Tidak menahan** penyelesaian konsultasi:

| Keadaan | Alasan |
|---|---|
| Lab order berhasil dibuat, specimen belum `Collected`/`Received`/`Resulted` | Lifecycle eksekusi milik unit Laboratorium dan berlanjut setelah dokter selesai |
| Radiology order berhasil dibuat, study belum `Performed`/`Resulted` | Lifecycle eksekusi milik unit Radiologi |
| Resep sudah difinalkan, farmasi belum menyerahkan obat | Penyerahan adalah fulfillment, bukan pembuatan order |

**Wajib menahan** penyelesaian konsultasi — clinical order yang sedang dibuat dokter berada dalam
keadaan tidak stabil:

| Keadaan | `TARGET` severity |
|---|---|
| Request pembuatan order gagal | `Error` |
| Data order invalid | `Error` |
| Penyimpanan belum berhasil | `Error` |
| Pending client-side mutation belum ter-flush | `Error` |
| State order tidak dapat dipastikan | `Error` |
| Pembuatan order belum menghasilkan authoritative server state | `Error` |

Sebagian aturan ini sudah ditegakkan frontend melalui `beforeFinalize` yang membatalkan finalisasi
ketika sebuah tab mengembalikan `false`
(`useDoctorConsultationWorkspace.js:272-273`). Kontrak membekukan bahwa penegakannya
**tidak boleh hanya di client** — backend wajib menolak finalisasi ketika order milik konsultasi
berada pada keadaan di atas. Ditutup pada `RJ-DOC-BE-002`.

### 1.7 Success contract

`HTTP 200`, body `ApiResponse<ConsultationFinalizationResponse>`
(`ConsultationFinalizationDtos.cs:49-66`):

| Field | Tipe | Makna |
|---|---|---|
| `ConsultationId` | `Guid` | — |
| `CompletedAt` | `DateTime` | UTC |
| `CompletedByUserId` | `Guid` | Actor dari auth context |
| `FinalizedPrescriptionCount` | `int` | Resep `Draft` yang difinalkan pada operasi ini |
| `FinalizedProcedureCount` | `int` | Jumlah tindakan aktif konsultasi |
| `BillingHandoffIssues` | `List<string>` | Resep yang sudah final secara klinis tetapi penyerahan faktanya belum tuntas. **Kosong berarti seluruh fakta tersampaikan. Konsultasi tetap selesai apa pun isinya** |
| `Validation` | `ConsultationFinalizationValidationResponse` | Hasil validasi yang meloloskan finalisasi |

State yang **wajib** benar setelah sukses:

```text
TrxDoctorConsultation.ConsultationStatus  = Completed
TrxDoctorConsultation.CompletedAt        != null
TrxDoctorConsultation.CompletedByUserId  != null  (dan bukan Guid.Empty)

TrxQueue.QueueStatus                      = Completed        (bila Queue != null)
TrxQueue.ConsultationCompletedAt         != null             (bila Queue != null)
TrxQueue.CompletedAt, CompletedByUserId  != null             (bila Queue != null)

TrxPatientEncounter.EncounterStatus       = ConsultationCompleted
```

Evidence: `ConsultationFinalizationService.cs:111-113` (konsultasi), `:119-126` (antrean),
`:128-133` (encounter).

### 1.8 Encounter semantics

```text
InConsultation (6)  ->  ConsultationCompleted (7)
```

Modul Dokter **tidak** menaikkan encounter ke `Billing` (`8`) maupun `Completed` (`9`).
Lifecycle setelah `ConsultationCompleted` **bukan** kepemilikan Dokter.

Dasar pembekuan:

| Bukti | Isi |
|---|---|
| `EncounterStatus.cs` | Urutan enum `InConsultation(6) → ConsultationCompleted(7) → Billing(8) → Completed(9)` menyatakan selesainya konsultasi bukan selesainya kunjungan |
| `MedicalRecordAccessAuditService.cs:74` | `Completed` termasuk `StatusKunjunganSelesai` — kewenangan akses rekam medis berubah |
| `MedicalRecordBackfillService.cs:49` | `Completed` termasuk `StatusKunjunganSelesai`; komentarnya: *"catatan pada kunjungan berstatus ini akan dikunci"* |

Karena itu menetapkan `Completed` dari penyelesaian konsultasi akan **mengunci rekam medis
kunjungan yang farmasi, laboratorium, radiologi, dan Billing-nya masih berjalan.**

`RJ-DOC-NOTICE-001` tetap terbuka dan **bukan** milik Dokter: `EncounterStatus.Billing` (`8`) tidak
memiliki satu pun penulis maupun pembaca pada source. Siapa yang menaikkan encounter dari
`ConsultationCompleted` ke `Billing` lalu ke `Completed` adalah pertanyaan Registration dan Billing.

### 1.9 Retry dan idempotency semantics

| Skenario | Perilaku yang dibekukan |
|---|---|
| **Double click** | Tepat satu finalisasi tertulis. Permintaan kedua **tidak** menulis apa pun |
| **Client retry setelah timeout** | Aman diulang. Tidak menggandakan finalisasi, resep yang difinalkan, tindakan, milestone, maupun fakta logis |
| **Duplicate operation** | Sama seperti retry |
| **Dua perangkat** | Yang membawa `ExpectedUpdatedAt` basi menerima `409`. Tidak ada silent overwrite |
| **Sudah `Completed`** | Ditolak sebagai keadaan yang sah, bukan `500`. Tidak menimpa `CompletedAt`/`CompletedByUserId` yang sudah ada |

> **`TARGET` — TOCTOU yang wajib ditutup.** Pada source, penjaga status dibaca pada `:62` sebelum
> baris konsultasi terkunci oleh `SaveChanges` pada `:66`. Dua permintaan serentak sama-sama dapat
> lolos penjaga itu. Pola advisory lock per encounter yang sudah dipakai
> `DoctorConsultationLifecycleService.AcquireLifecycleLockAsync` adalah kandidat reuse pertama.
> Ditutup pada `RJ-DOC-BE-003`.

**Tidak boleh ada dua finalization writes untuk satu konsultasi.**

### 1.10 Secondary route — orchestration layer

```text
POST /api/v1/health-services/registration-management/doctor-queues/{id}/finish-consultation
```

Dibekukan: route ini **bukan** finalisasi domain yang berdiri sendiri. Ia adalah
**orchestration / compatibility layer** yang pada akhirnya memakai semantik canonical consultation
finalization.

| Aturan | Isi |
|---|---|
| Boleh menerapkan efek antrean | Ya — itu memang tanggung jawabnya |
| Boleh memfinalkan konsultasi dengan logikanya sendiri | **Tidak** |
| Hasil akhir state | Wajib identik dengan canonical, termasuk `EncounterStatus = ConsultationCompleted` |
| Perilaku antrean existing | **Wajib dipertahankan** — ini alur produksi aktif |

Keadaan pada source yang menjadi alasan pembekuan (`DoctorQueueController.cs:440-484`): route ini
menutup antrean dan encounter tetapi **tidak menyentuh `TrxDoctorConsultation` sama sekali**, tidak
menjalankan validasi, tidak memfinalkan resep, tidak menerbitkan fakta, dan menetapkan
`EncounterStatus = Completed` alih-alih `ConsultationCompleted`.

Asimetri yang membuktikan ini cacat, bukan desain: `start-consultation` pada `:399` **memanggil**
`DoctorConsultationLifecycleService.GetOrCreateForQueueAsync` dan membuka konsultasi, sedangkan
`finish-consultation` tidak memanggil apa pun yang menutupnya.

### 1.11 `CompleteImmediately` — bukan canonical path

```text
POST /api/v1/health-services/clinical-management/doctor-consultations
Body: { "completeImmediately": true, ... }
```

Dibekukan sesuai keputusan owner `RJ-DOC-DEC-005`:

| Aturan | Isi |
|---|---|
| Status | **Bukan** canonical normal Rawat Jalan completion |
| Consumer baru | **Dilarang** memakainya untuk alur Rawat Jalan normal |
| Treatment | `restrict` / `deprecate` / compatibility remediation |
| Penghapusan API | **Tidak dilakukan** pada task contract freeze ini |
| Waktu implementasi | `RJ-DOC-BE-001` atau task terkait, **setelah** contract freeze |

Alasannya bukan preferensi gaya: jalur ini menghasilkan `ConsultationStatus = Completed` beserta
`CompletedAt` dan `CompletedByUserId` **tanpa** validasi authoritative dan **tanpa** producer
handoff (`DoctorConsultationController.cs:290-291`, `:348-349`).

#### Compatibility requirements — wajib dijaga saat remediasi

| Yang **tidak boleh** rusak | Bukti |
|---|---|
| **`POST /doctor-consultations` itu sendiri.** Endpoint create adalah jalur normal dan aktif dipakai frontend | `use-doctor-soap.js:327`, `use-doctor-prescription.js:239` |
| **Jalur IGD tanpa antrean.** `QueueId` nullable dan encounter di-resolve langsung ketika antrean tidak ada | `DoctorConsultationController.cs:257-268`, komentar source: *"Pasien poli membawa baris antrean; pasien IGD tidak. BE-IGD-028, FR-IGD-062"* |
| **`CompleteImmediately` milik `PatientAssessmentController`.** Field bernama sama pada DTO **berbeda** (`PatientAssessmentDtos.cs:294`), dipakai alur screening perawat. **Di luar cakupan keputusan ini** dan tidak boleh ikut dibatasi | `PatientAssessmentController.cs:296`, `:370-371`, `:678` |

Keadaan consumer yang terverifikasi pada frontend `baca965`: **tidak ada satu pun call site yang
mengirim `completeImmediately: true` untuk konsultasi.** Seluruh pembuatan konsultasi memakai
`false` (`use-doctor-prescription.js:210`). Kemunculan `completeImmediately` pada
`use-doctor-queue.js:786` adalah milik payload **patient assessment**, bukan konsultasi.

Karena itu membatasi jalur ini **tidak memutus satu pun consumer yang diketahui**.

### 1.12 Security requirements — mengikat

| Requirement | Keadaan |
|---|---|
| Authorization server-side | `[Authorize]` + `[AccessPermission("DoctorConsultation","Update")]` — terpenuhi |
| Actor tidak dipercaya dari payload | Diambil dari `ClaimTypes.NameIdentifier` — terpenuhi |
| Actor kosong ditolak | **`TARGET`** — `Guid.Empty` masih mungkin tersimpan |
| Tidak ada konten klinis sensitif di log | **`TARGET`** — `/complete` memakai `InfoAsync` dengan `result.Data`; jalur `finish-consultation` tidak memanggil logger sama sekali. Ditutup `RJ-DOC-BE-006` |
| Tidak ada financial authority di endpoint klinis | Terpenuhi — `RJ-DOC-INV-002` `VERIFIED` |
| Error contract stabil | `400` validasi, `409` konflik, `200` sukses — dibekukan di atas |
| Tidak ada silent stale overwrite | **`TARGET`** — `ExpectedUpdatedAt` menjadi wajib |
| Concurrency-safe | **`TARGET`** — TOCTOU ditutup `RJ-DOC-BE-003` |
| Auditability eksplisit | **`TARGET`** — `RJ-DOC-BE-006` |

Tidak ada credential, connection string, token, atau secret pada dokumen ini.

---

# Bagian 2 — `RJ-DOC-HANDOFF-001@1.0.0`

## Producer Handoff Contract — `RJ-DOC-INT-002`

### 2.1 Aturan inti

```text
FOR EACH ELIGIBLE CLINICAL MILESTONE
        -->  one logical durable clinical fact / version

NO ELIGIBLE MILESTONE
        -->  ZERO FACT
        -->  VALID
```

Aturan berlaku **per eligible milestone**, bukan per konsultasi. Aturan
`every consultation must have a fact` **dilarang** dan tidak boleh dipakai consumer maupun
pemeriksa mana pun.

### 2.2 Identitas fakta

Dibekukan dari `TrxClinicalMilestoneFact` dan `ClinicalMilestoneFactProducer`:

| Elemen | Kegunaan |
|---|---|
| `SourceContext` | Konteks produser, mis. konteks resep pada `BillingSourceContract` |
| `SourceAggregateId` | Aggregate klinis asal |
| `SourceItemId` | Baris di dalam aggregate, opsional |
| `EffectType` | Jenis akibat yang diklaim producer |
| `MilestoneFactId` | **Identitas logis yang stabil** lintas revisi |
| `MilestoneFactVersion` | Versi eksplisit, naik monoton per `MilestoneFactId` |
| `IdempotencyKey` | Diturunkan dari `MilestoneFactId` + `MilestoneFactVersion` |
| `PayloadFingerprint` | Deteksi revisi identik |
| `EncounterId` | Korelasi kunjungan |
| `OccurredAt` | Waktu peristiwa klinis |
| `Quantity`, `Unit` | Ukuran klinis bila ada |
| `TariffSnapshot` | **Rujukan**, bukan perhitungan |
| `CorrelationId`, `CausationId` | Penelusuran |
| `DispatchStatus` | `Pending` · `Dispatched` · `OutcomeUnknown` · `Rejected` · `SuppressedNoPriorCharge` |
| `DispatchAttemptCount`, `DispatchedAt` | Bukti percobaan |
| `ActorUserId` | Actor dari auth context |

### 2.3 Tanggung jawab producer — Doctor / Clinical

Producer **menjamin**:

1. eligible clinical milestone teridentifikasi;
2. fakta memiliki **stable source identity**;
3. fakta memiliki **stable logical milestone identity**;
4. fakta memiliki **explicit version**;
5. fakta memiliki **stable idempotency identity**;
6. fakta **durable** — ditulis sebelum dispatch, bukan sesudah;
7. fakta yang belum berhasil dispatch **dapat ditemukan kembali**;
8. retry producer **tidak membuat logical fact baru** untuk operasi yang sama;
9. kegagalan downstream **tidak me-rollback** clinical completion.

Yang sudah terbukti pada source: butir `2`–`6` dan `9`. Butir `7` dan `8` sebagian —
`DispatchStatus` dan `IdempotencyKey` tersimpan dan index `IX_TrxClinicalMilestoneFact_DispatchStatus`
tersedia, tetapi **tidak ada satu pun pembaca ulang**. Ditutup pada `RJ-DOC-BE-005`.

Butir `9` ditegakkan **secara teknis**, bukan sekadar didokumentasikan:
`ClinicalMilestoneFactProducer` melempar `InvalidOperationException` bila dipanggil di dalam
transaksi klinis yang masih terbuka (`:83-89`). Pemanggil wajib commit lebih dulu.

### 2.4 Tanggung jawab consumer — Billing / downstream

Doctor **tidak menjamin**: Billing Folio berhasil dibuat · charge berhasil dibuat · tariff
calculation · payer allocation · payment · settlement · financial reconciliation ·
**charge deduplication**.

Consumer **wajib menjamin consumer-side idempotency**, memakai `IdempotencyKey`,
`MilestoneFactId`, dan `MilestoneFactVersion` yang diterimanya. Producer menjamin identitas stabil;
menjamin satu identitas tidak menjadi dua charge adalah kewajiban consumer.

Consumer **tidak boleh** menolak, membatalkan, atau menunda penyelesaian konsultasi.

### 2.5 Eligibility — current mandatory baseline

Dibekukan dari keputusan owner `RJ-DOC-DEC-002`.

| Milestone | Kelas | Eligible bila | Evidence |
|---|---|---|---|
| **Prescription finalization** | `MANDATORY` | Konsultasi memiliki resep, dan resep difinalkan bersama penyelesaian konsultasi | `ConsultationFinalizationService.cs:151`; milestone `RJ-BIL-DEC-002` |
| **Procedure execution** | `MANDATORY` | Tindakan benar-benar dieksekusi sesuai lifecycle existing | `PatientProcedureController.cs:967` pada `PATCH /{id}/execute` |
| Lab specimen acceptance | `CONDITIONAL` | — | `LabSpecimenService`; **bukan** mandatory baseline Dokter |
| Radiology acquisition | `CONDITIONAL` | — | `RadStudyService.cs:958`; **bukan** mandatory baseline Dokter |

Konsultasi **tanpa** resep dan **tanpa** tindakan yang dieksekusi adalah konsultasi **tanpa
eligible milestone**, dan karena itu menghasilkan **nol fakta** — hasil yang benar.

> Lab dan Radiologi tetap tercatat sebagai clinical capability yang valid dan dapat dinaikkan
> menjadi mandatory pada rilis berikutnya melalui approval terpisah. Capability dan gap
> implementasinya **tidak dihapus**.

### 2.6 Recovery semantics

| Keadaan | Verdict | Tindakan |
|---|---|---|
| Tidak ada eligible milestone, tidak ada fakta | **`VALID`** | Tidak ada. Bukan galat, bukan gap |
| Ada eligible milestone, fakta yang diharapkan tidak ada | **`RECOVERABLE PRODUCER GAP`** | Producer menemukan dan menerbitkan/mengirim ulang |
| Fakta ada, `DispatchStatus = Pending` | `RECOVERABLE` | Kirim ulang dengan `IdempotencyKey` yang sama |
| Fakta ada, `DispatchStatus = OutcomeUnknown` | `RECONCILIATION REQUIRED` | Dilarang menebak; jangan terbitkan revisi buta sebelum status pasti |
| Fakta ada, `Dispatched`, consumer belum memproses | Urusan consumer | Bukan gap producer |
| Fakta ada, `Rejected` oleh consumer | Urusan consumer | Producer tidak mengulang otomatis |

Celah yang wajib ditutup `RJ-DOC-BE-005`: bila proses berhenti antara clinical commit dan penulisan
baris fakta, **tidak ada baris fakta sama sekali** dan tidak ada jalur pemulihan. Deteksinya
memakai aturan `eligible milestone ada + fakta tidak ada = RECOVERABLE PRODUCER GAP`.

### 2.7 Yang tetap milik downstream

Rekonsiliasi finansial, dead-letter finansial, recovery report Billing, dan deduplikasi charge
adalah `RJ-BIL-BE-007` dan sekitarnya — **`DOWNSTREAM`, bukan Doctor Definition of Done.**

---

## Perubahan kontrak

Kedua kontrak berstatus `FROZEN`. Perubahan apa pun memerlukan kenaikan versi dan approval owner
tersendiri. Butir bertanda `TARGET` adalah perilaku yang **dibekukan sebagai kewajiban** tetapi
belum ada pada source; ia ditutup oleh task implementasi yang disebut, bukan dengan mengubah
kontrak ini.
