# Roadmap Delivery Dokter / Rawat Jalan Klinis — sampai `Selesai Konsultasi`

## Metadata

```yaml
blueprint_id: RJ-BIL-BP-001
scope_name: "Doctor / Rawat Jalan Clinical Delivery"
scope_prefix: RJ-DOC
module_slug: rawat-jalan
roadmap_revision: 5
status: OWNER_APPROVED
approval_gate: OWNER_APPROVED
approved_by: "Sukma Giri — RJ-DOC-DEC-001"
approved_at: "2026-08-31"
approval_scope: "scope, ownership boundary, klasifikasi capability, arah canonical completion, contract planning, task definitions, dependency sequence, Doctor DoD"
contract_versions:
  - "RJ-DOC-COMPLETION-001@1.0.0 (FROZEN)"
  - "RJ-DOC-HANDOFF-001@1.0.0 (FROZEN)"
owners:
  - "Product/Domain: Sukma Giri"
  - "Clinical Governance: OPEN"
  - "Frontend authority: OPEN"
snapshot_kind: CURRENT_STATE
audited_at: "2026-08-31"
audited_backend_sha: "801a4f52459e1251ec9bb03c1abfe5e17dd3639c"
audited_backend_branch: sukmagp
audited_backend_worktree: "DIRTY — 1 berkas test termodifikasi, 7 berkas agents/rules terhapus; tidak satu pun menyentuh jalur konsultasi"
audited_frontend_sha: "baca9650848ded164538ab85405190fafe8785a3"
audited_frontend_branch: QuilvianDevV2
audited_frontend_worktree: CLEAN
implementation_authority: "GRANTED — RJ-DOC-BE-001 dan RJ-DOC-BE-002; task lain NOT_GRANTED"
builder_execution: "EXECUTED — RJ-DOC-BE-001 dan RJ-DOC-BE-002 (keduanya COMPLETE 2026-08-31); task lain NOT_AUTHORIZED"
downstream_roadmaps:
  - roadmap/backend-roadmap.md
  - roadmap/frontend-roadmap.md
```

> ## `OWNER_APPROVED` — `2026-08-31`, `RJ-DOC-DEC-001`
>
> Scope, ownership boundary, klasifikasi capability, arah canonical completion, contract planning,
> task definitions, dependency sequence, dan Doctor Definition of Done **disetujui** oleh
> Sukma Giri selaku pemilik blueprint.
>
> **Approval ini bukan izin menulis code.** Ia tidak mencakup perubahan application source,
> eksekusi builder, migration, mutasi database, commit, push, merge, deployment, maupun pekerjaan
> Billing. `IMPLEMENTATION_AUTHORITY` tetap `NOT_GRANTED`; setiap task tetap memerlukan handoff,
> wewenang tulis, dan preflight tersendiri.
>
> Kedua contract gate sudah **`FROZEN`** — lihat bagian `4.0`. Empat open question ditutup oleh
> keputusan owner `RJ-DOC-DEC-002` s.d. `RJ-DOC-DEC-005`; **tidak ada open question tersisa** pada
> scope ini.

---

## 0. Mengapa dokumen ini ada

Sampai revisi `21`, `docs/module-blueprints/rawat-jalan/` hanya memiliki satu roadmap backend dan
satu roadmap frontend, keduanya berprefix `RJ-BIL` dan keduanya berisi Billing.

Akibatnya blueprint tidak dapat menjawab pertanyaan yang paling sering ditanyakan:

> *"Apakah pekerjaan developer Dokter / Rawat Jalan sudah selesai?"*

Angka `5 dari 9` backend dan `2 dari 7` frontend adalah angka **Billing**, dan memakainya untuk
menilai pekerjaan Dokter salah ke dua arah sekaligus. Ia menyatakan Dokter belum selesai karena
payer belum ada — padahal payer bukan pekerjaan Dokter. Dan ia menyembunyikan bahwa jalur klinis
yang benar-benar milik Dokter **putus di satu tempat yang sangat spesifik**, yaitu tombol
`Selesai Konsultasi`.

Dokumen ini memisahkan keduanya. Roadmap `RJ-BIL` tetap berlaku dan tidak dihapus; ia menjadi
**roadmap downstream** yang mengonsumsi hasil dokumen ini.

---

## 1. Batas kepemilikan

```text
RAWAT JALAN END-TO-END

+==================== START OF DOCTOR SCOPE ====================+
|                                                               |
|  MANDATORY BASELINE                                           |
|  Patient / Encounter / Visit context                          |
|  Doctor Consultation - mulai / lanjutkan                      |
|  Anamnesis (ChiefComplaint, HistoryOfPresentIllness)          |
|  Pemeriksaan / vital sign                                     |
|  Diagnosis (ICD-10)                                           |
|  SOAP / CPPT sesuai alur existing                             |
|  Prescription  - clinical order + finalisasi klinisnya        |
|  Procedure     - clinical order + eksekusi klinisnya          |
|  Autosave / draft / validasi sebelum ditutup                  |
|                                                               |
|  CONDITIONAL - RJ-DOC-DEC-002, bukan mandatory baseline        |
|  Lab order        dari workspace dokter                       |
|  Radiology order  dari workspace dokter                       |
|                                                               |
|          v  TOMBOL "SELESAI KONSULTASI"                       |
|                                                               |
|  canonical backend finalization (satu jalur, bukan tiga)      |
|  authoritative validation                                     |
|  ConsultationStatus = COMPLETED                               |
|  CompletedAt + CompletedByUserId                              |
|  audit trail                                                  |
|  idempotency terhadap double click dan retry                  |
|  concurrency protection terhadap multi-device                 |
|  completed-state protection                                   |
|                                                               |
|          v                                                    |
|  DURABLE CLINICAL HANDOFF - sisi PRODUCER saja                |
|  untuk SETIAP eligible clinical milestone:                    |
|    satu fact logis yang durable, ber-identity stabil,         |
|    dapat ditemukan kembali, dan dapat dikirim ulang           |
|  nol eligible milestone  ==>  nol fact  ==>  VALID            |
|                                                               |
+============== END OF DOCTOR DEFINITION OF DONE ===============+
                              |
         ============ OWNERSHIP BOUNDARY ============
                              |
                              v
+============== DOWNSTREAM BILLING STARTS HERE =================+
|  Consume clinical fact  (consumer-side idempotency)           |
|  Folio . Charge . Tariff                                      |
|  Payer allocation . Patient responsibility                    |
|  Financial action . Approval . Reconciliation . Dead-letter   |
|  Claim . Invoice . Payment . Settlement . Cashier             |
|                                                               |
|  Owner: Billing / Finance / Payer / Pharmacy / Lab / Radiology |
|  Blocks Doctor DoD: NO                                        |
+===============================================================+
```

### Yang **tidak** boleh dikerjakan dari scope Dokter

Membuat Billing Folio; menghitung tarif; menghitung total charge; menetapkan nominal akhir;
payer allocation; patient responsibility; menetapkan `Paid`, `Settled`, atau `InsuranceApproved`;
payment; kasir; kuitansi; invoice; klaim; settlement; financial adjustment; write-off; refund;
maker-checker finansial; rekonsiliasi finansial; dead-letter finansial; recovery report Billing;
UI rekonsiliasi; dashboard Billing; UI operasional Billing.

Modul klinis **tidak boleh menjadi financial source of truth**. Satu-satunya jalur resmi
penyerahan ke Billing adalah `ClinicalMilestoneFactProducer`.

### Order bukan Billing

| Yang milik Dokter | Yang milik downstream |
| --- | --- |
| Membuat resep, memfinalkan lifecycle klinisnya | Harga obat, charge resep |
| Membuat tindakan, menandainya dieksekusi | Tarif tindakan, charge tindakan |
| Membuat order laboratorium | Charge laboratorium |
| Membuat order radiologi | Charge radiologi |

Dokter bertanggung jawab atas **intent klinis dan lifecycle klinisnya**. Billing bertanggung
jawab atas **interpretasi finansial** dari fakta tersebut.

### Batas durable handoff — sengaja dipersempit

Producer klinis **wajib** menjamin, untuk setiap eligible clinical milestone:

1. milestone menghasilkan fact yang **durable** — tersimpan sebelum dispatch;
2. identity dan versi fact **stabil**;
3. fact **tidak hilang** setelah clinical commit;
4. fact yang belum terkirim **dapat ditemukan kembali**;
5. retry producer memakai **identity yang sama**, sehingga tidak menggandakan fact logis;
6. kegagalan downstream **tidak** me-rollback consultation yang sudah `COMPLETED`.

Producer klinis **tidak** bertanggung jawab atas: pembuatan charge Billing, memastikan charge
Billing tidak duplikat, rekonsiliasi finansial, dead-letter finansial, recovery report Billing,
serta hasil payer/payment. Semuanya milik `RJ-BIL-*`, dan **consumer wajib menerapkan
consumer-side idempotency** sesuai `contracts/integration-contract.md`.

---

## 2. Bukti audit — keadaan aktual per `2026-08-31`

`CURRENT STATE`. Audit read-only terhadap backend `801a4f5` cabang `sukmagp` dan frontend
`baca965` cabang `QuilvianDevV2`. **SHA yang tertulis pada artefak lain sudah usang** dan tidak
dipakai sebagai source of truth.

### 2.1 Tiga permukaan penyelesaian, satu di antaranya dipakai frontend

Terdapat **tiga** permukaan yang dapat menyelesaikan konsultasi atau kunjungan, dan ketiganya
menghasilkan state yang berbeda.

| | `A` — dipakai frontend | `B` — dibangun untuk finalisasi | `C` — jalan pintas |
| --- | --- | --- | --- |
| **Endpoint** | `POST /doctor-queues/{id}/finish-consultation` | `PATCH /doctor-consultations/{id}/complete` | `POST /doctor-consultations` dengan `CompleteImmediately=true` |
| **Source** | `DoctorQueueController.cs:440` | `DoctorConsultationController.cs:590` → `ConsultationFinalizationService.FinalizeAsync` | `DoctorConsultationController.cs:291`, `:348`, `:380` |
| **Frontend** | `finishDoctorConsultation` — **dipakai** | `completeDoctorConsultation` — **nol pemanggil** | `createDoctorConsultation` — dipakai, tetapi **selalu** `completeImmediately: false` |
| Validasi finalisasi | tidak | **ya** | tidak |
| `ConsultationStatus = Completed` | **tidak** | ya | ya |
| `CompletedAt` / `CompletedByUserId` konsultasi | **tidak** | ya | ya |
| Memfinalkan resep `Draft` → `Submitted` | tidak | **ya** | tidak |
| Menerbitkan clinical fact | tidak | **ya** | tidak |
| Concurrency check | tidak | opsional (`ExpectedUpdatedAt`) | tidak |
| `EncounterStatus` hasil | `Completed` (`9`) | `ConsultationCompleted` (`7`) | `ConsultationCompleted` (`7`) |

Asimetri paling telanjang ada pada pasangan endpoint antrean:
`start-consultation` **memanggil** `DoctorConsultationLifecycleService.GetOrCreateForQueueAsync`
dan membuka `TrxDoctorConsultation`; `finish-consultation` **tidak memanggil apa pun** yang
menutupnya.

Akibat berantai yang dapat diverifikasi tanpa menjalankan sistem:

1. `TrxDoctorConsultation.ConsultationStatus` **tidak pernah** menjadi `Completed` pada alur
   dokter yang sebenarnya. Ia berhenti di `InProgress`.
2. Karena itu seluruh penguncian yang sudah ditulis dengan benar menjadi **tidak pernah aktif**.
   Ketujuh penjaga `ConsultationStatus == Completed` — `DoctorConsultationController.cs:424`,
   `:524`, `PatientDiagnosisController.cs:834`, `PatientProcedureController.cs:629`, `:1313`,
   `:1369`, `PrescriptionController.cs:558` — tidak pernah dievaluasi `true`. SOAP, diagnosis,
   resep, dan tindakan tetap dapat ditulis setelah dokter menekan `Selesai Konsultasi`. Tidak ada
   pula penjaga yang berpegang pada `EncounterStatus`.
3. Resep tetap `Draft`. `ConsultationFinalizationService` adalah satu-satunya pemanggil
   `PrescriptionWorkflowService.FinalizeFromConsultationAsync`.
4. **Tidak ada satu pun clinical fact resep yang pernah diterbitkan.** Milestone charge resep
   menurut `RJ-BIL-DEC-002` adalah *"resep difinalkan bersama konsultasi dokter"* — milestone itu
   tidak pernah tercapai.

Fakta tindakan **tidak** ikut terdampak: `PatientProcedureController.cs:967` menerbitkannya pada
`PATCH /{id}/execute`, terlepas dari permukaan penyelesaian mana pun.

### 2.2 Registry capability — empat kelas terpisah

Denominator progress **hanya** diambil dari kelas `MANDATORY`. Ketiga kelas lain tidak pernah
dijumlahkan ke dalamnya.

#### 2.2.1 `MANDATORY` — dihitung sebagai implementation progress

| # | Capability | BE | FE | Status | Evidence | Gap |
|---|---|:--:|:--:|---|---|---|
| `RJ-DOC-CAP-001` | Workspace Dokter Rawat Jalan reachable | — | ada | `COMPLETE` | `doctor-queue-view.jsx`; `menu-items.jsx` | — |
| `RJ-DOC-CAP-002` | Context pasien/encounter/visit/consultation | ada | ada | `COMPLETE` | `DoctorConsultationController.cs:196`; `doctor-consultation.service.js:32` | — |
| `RJ-DOC-CAP-003` | Memulai/melanjutkan konsultasi | ada | ada | `COMPLETE` | `DoctorConsultationLifecycleService.GetOrCreateForQueueAsync`; advisory lock per encounter | — |
| `RJ-DOC-CAP-004` | Anamnesis | ada | ada | `COMPLETE` | `TrxDoctorConsultation.ChiefComplaint`, `HistoryOfPresentIllness` | — |
| `RJ-DOC-CAP-005` | Pemeriksaan / vital sign | ada | ada | `COMPLETE` | snapshot vital dari `TrxPatientAssessment` | — |
| `RJ-DOC-CAP-006` | Diagnosis | ada | ada | `COMPLETE` | `PatientDiagnosisController` | — |
| `RJ-DOC-CAP-007` | SOAP / CPPT | ada | ada | `COMPLETE` | `PATCH /{id}/soap`; `ensureCpptFromDoctorConsultation` | — |
| `RJ-DOC-CAP-008A` | Prescription — pembuatan dan draft clinical order | ada | ada | `COMPLETE` | `PrescriptionController`; `PrescriptionWorkspaceService`; tab resep | — |
| `RJ-DOC-CAP-008B` | Prescription — finalisasi pada penyelesaian konsultasi | ada | tidak terpakai | `PARTIAL` | `FinalizeFromConsultationAsync` ada, hanya dipanggil `ConsultationFinalizationService` | Tidak pernah tercapai; resep tetap `Draft` |
| `RJ-DOC-CAP-009` | Procedure clinical order dan eksekusi | ada | ada | `COMPLETE` | `PatientProcedureController` | — |
| `RJ-DOC-CAP-012` | Autosave clinical data | ada | ada | `COMPLETE` | `PATCH /{id}/soap`; `saveNow`/`flushPending` | — |
| `RJ-DOC-CAP-013` | Pending autosave di-flush saat finalisasi | — | ada | `COMPLETE` | `useDoctorConsultationWorkspace.js:263-290`; `false` membatalkan finalisasi | Orkestrasi milik client |
| `RJ-DOC-CAP-014` | Validasi authoritative sebelum konsultasi ditutup | ada | nihil | `PARTIAL` | `ConsultationValidationService` lengkap untuk SOAP, diagnosis, resep, tindakan | **Tidak pernah dipanggil**; hanya tersedia sebagai `GET` opsional |
| `RJ-DOC-CAP-015` | Tombol `Selesai Konsultasi` ke canonical finalization | ada | salah tujuan | `MISSING` | `ConsultationTab.jsx:101` → endpoint **antrean** | `completeDoctorConsultation` nol pemanggil |
| `RJ-DOC-CAP-016` | `ConsultationStatus = COMPLETED` | ada | nihil | `PARTIAL` | `ConsultationFinalizationService.cs:111` | Tidak tercapai dari alur dokter |
| `RJ-DOC-CAP-017` | `CompletedAt` dan `CompletedByUserId` | ada | nihil | `PARTIAL` | service `:112`, `:113` | Sama |
| `RJ-DOC-CAP-018` | Idempotency finalisasi | sebagian | sebagian | `PARTIAL` | BE penjaga status `:62`; FE `actionLoadingKey` + tombol `disabled` | Tanpa idempotency key. Penjaga status dibaca **sebelum** `SaveChanges` — dua permintaan serentak sama-sama lolos sebelum baris terkunci |
| `RJ-DOC-CAP-019` | Concurrency / multi-device | sebagian | nihil | `PARTIAL` | `ExpectedUpdatedAt` → `409` pada `:57-60` | **Opsional**; frontend tidak mengirimnya |
| `RJ-DOC-CAP-020` | Completed-state protection | sebagian | — | `PARTIAL` | tujuh penjaga `ConsultationStatus == Completed` | Inert — lihat `2.1` butir `2` |
| `RJ-DOC-CAP-021` | Producer handoff resep | ada | — | `PARTIAL` | `ConsultationFinalizationService.cs:151`, setelah commit | Tidak pernah dieksekusi karena `CAP-015` |
| `RJ-DOC-CAP-022` | Producer handoff tindakan | ada | — | `COMPLETE` | `PatientProcedureController.cs:967` pada `execute` | — |
| `RJ-DOC-CAP-023` | Durabilitas dan recoverability producer handoff | sebagian | — | `PARTIAL` | `TrxClinicalMilestoneFact` ditulis **sebelum** dispatch; `Pending`/`OutcomeUnknown` tercatat | **Tidak ada pembaca ulang.** Fact yang belum terkirim tidak dapat ditemukan atau dikirim ulang. Bila proses mati antara clinical commit dan penulisan fact, tidak ada baris fact dan tidak ada jalur pemulihan |
| `RJ-DOC-CAP-025` | Audit trail clinical completion | sebagian | — | `PARTIAL` | `/complete` memakai `LoggerService.InfoAsync`; `ClinicalFact.Dispatch` memakai `AuditAsync` | `finish-consultation` — jalur yang dipakai — **tidak memanggil logger sama sekali** |
| `RJ-DOC-CAP-026` | FE loading / error / success state | — | ada | `COMPLETE` | `runAction`; mengembalikan `false` saat gagal — UI tidak berpura-pura sukses | — |
| `RJ-DOC-CAP-027` | FE conflict (`409`) state | — | nihil | `MISSING` | `use-doctor-queue.js:1142` hanya memuat ulang doctor-call-lock | Tidak ada penanganan konflik versi konsultasi |
| `RJ-DOC-CAP-028` | Refresh menampilkan status authoritative | sebagian | sebagian | `PARTIAL` | `refreshData` memuat ulang antrean | Membaca `QueueStatus`, bukan `ConsultationStatus` |
| `RJ-DOC-CAP-029` | Automated test scope Dokter | nihil | nihil | `MISSING` | Kedua test project backend tidak memuat test konsultasi; `tests/unit` frontend tidak memuat berkas dokter | Nol test |
| `RJ-DOC-CAP-030` | Satu canonical completion path | nihil | — | `MISSING` | Tiga permukaan penyelesaian dengan hasil berbeda — lihat `2.1` | Termasuk `EncounterStatus` yang berbeda antar permukaan |

**Rekapitulasi `MANDATORY` — denominator `28`:**

| Status | Jumlah | Butir |
|---|---:|---|
| `COMPLETE` | `13` | `001`–`007`, `008A`, `009`, `012`, `013`, `022`, `026` |
| `PARTIAL` | `11` | `008B`, `014`, `016`–`021`, `023`, `025`, `028` |
| `MISSING` | `4` | `015`, `027`, `029`, `030` |
| `NEEDS CONFIRMATION` | `0` | — |
| **Total** | **`28`** | |

> **Tidak ada persentase yang diberikan di sini.** Capability `PARTIAL` tidak memiliki bobot
> resmi, sehingga angka seperti `45%` akan menjadi karangan. Yang berlaku adalah hitungan status
> di atas.
>
> Yang penting dibaca dari tabel ini: fondasi klinisnya kuat — workspace, anamnesis, vital,
> diagnosis, SOAP/CPPT, pembuatan resep, tindakan, dan autosave semuanya `COMPLETE`. Sebelas butir
> `PARTIAL` dan tiga dari empat butir `MISSING` hampir seluruhnya adalah akibat berantai dari
> **satu** sambungan yang salah, yaitu `RJ-DOC-CAP-015` dan `RJ-DOC-CAP-030`.

#### 2.2.2 `CONDITIONAL` — tidak dihitung sampai release scope diputuskan

| # | Capability | BE | FE | Implementation | Doctor DoD blocking | Keputusan |
|---|---|:--:|:--:|---|---|---|
| `RJ-DOC-CAP-010` | Lab order dari workspace dokter | ada | **nihil** | BE `LabOrderController` + `LabSpecimenService` ada; nol service/hook/tab Lab pada frontend | `CONDITIONAL` | `RJ-DOC-OQ-003` |
| `RJ-DOC-CAP-011` | Radiology order dari workspace dokter | ada | **nihil** | BE `RadiologyManagement` `17` berkas termasuk `RadOrderController`, `RadStudyService`, `RadSafetyGateEvaluator`; nol consumer frontend | `CONDITIONAL` | `RJ-DOC-OQ-003` |

Selama `RJ-DOC-OQ-003` belum dijawab, kedua butir ini **tidak** membuat Doctor DoD gagal dan
**tidak** masuk denominator. Gap implementasinya tetap tercatat dan tidak dihapus.

#### 2.2.3 `ARCHITECTURAL INVARIANT` — diverifikasi, tidak dihitung

Invariant adalah sifat arsitektur yang harus **tetap benar**, bukan pekerjaan yang harus
**diselesaikan**. Ia diverifikasi ulang setiap rilis, dan tidak pernah menjadi angka progress.

| # | Invariant | Verdict | Evidence |
|---|---|---|---|
| `RJ-DOC-INV-001` | Kegagalan Billing tidak me-rollback consultation yang sudah committed | `VERIFIED` | `ClinicalMilestoneFactProducer:83-89` melempar `InvalidOperationException` bila dipanggil di dalam transaksi klinis; `FinalizeAsync` commit lebih dulu; kegagalan dikembalikan sebagai `BillingHandoffIssues` |
| `RJ-DOC-INV-002` | Clinical endpoint tidak menetapkan `Paid`/`Settled`/`InsuranceApproved`/`PaymentWaived`/`BillingGenerated` | `VERIFIED` | `PrescriptionWorkflowService.cs:62-63`; tidak ada penulisan selain inisialisasi `NotBilled`/`false` |
| `RJ-DOC-INV-003` | Clinical module tidak menghitung tarif, total, atau alokasi | `VERIFIED` | `BuildPrescriptionSnapshot` hanya menyalin harga kotor sebagai rujukan; pembagian tanggungan sengaja tidak disertakan |

> `RJ-DOC-INV-001` sebelumnya tercatat sebagai `RJ-DOC-CAP-024` dan ikut dihitung sebagai
> implementation capability. Itu keliru dan sudah dikoreksi pada revisi ini. ID `RJ-DOC-CAP-024`
> **dipensiunkan dan tidak dipakai ulang**.

#### 2.2.4 `DOWNSTREAM` — tidak pernah dihitung sebagai progress Dokter

Seluruh `RJ-BIL-CAP-*` dan `RJ-BIL-BE/FE-*`. Rinciannya pada
[requirement-traceability.md](requirement-traceability.md) bagian `0.1`.

### 2.3 Ownership violation

| Pemeriksaan | Hasil |
|---|---|
| Endpoint klinis menetapkan status finansial | **TIDAK DITEMUKAN** |
| Modul klinis menulis `PaymentStatus` selain inisialisasi `NotBilled` | **TIDAK DITEMUKAN** |
| Modul klinis menulis `IsBillingGenerated` selain inisialisasi `false` | **TIDAK DITEMUKAN** |
| Modul klinis menghitung total/tarif/alokasi | **TIDAK DITEMUKAN** |
| Modul klinis memanggil Billing di dalam transaksi klinis | **TIDAK DITEMUKAN** — dilarang secara teknis |

**Verdict: `NO OWNERSHIP VIOLATION` pada arah tulis.**

Satu sisa bersifat **baca**, bukan tulis: `PrescriptionResponse.paymentStatus` masih mengirim
nilai finansial dari endpoint klinis (`PrescriptionController.cs:217`, `:599`, `:671`, `:691`).
Klasifikasi `REFERENCE`; pemiliknya Farmasi bersama Billing.

---

## 3. Keputusan canonical completion

### 3.1 Canonical endpoint

| Field | Isi |
|---|---|
| **Canonical** | `PATCH /doctor-consultations/{consultationId}/complete` |
| **Alasan** | Hanya permukaan ini yang menjalankan validasi authoritative, mentransisikan `ConsultationStatus`, mengisi `CompletedAt`/`CompletedByUserId`, memfinalkan resep, menerbitkan clinical fact, dan memiliki kontrak konflik `409`. Ia juga berada pada aggregate yang benar — konsultasi, bukan antrean |
| **Sekunder** | `POST /doctor-queues/{queueId}/finish-consultation` |
| **Perlakuan** | **Orchestration wrapper** yang mendelegasikan finalisasi klinis ke canonical, lalu menerapkan efek antrean. Bukan penghapusan: alur produksi aktif memakainya dan perilaku antreannya wajib dipertahankan |
| **Jalan pintas** | `POST /doctor-consultations` dengan `CompleteImmediately=true` |
| **Perlakuan** | **Deprecation candidate.** Ia menghasilkan konsultasi `Completed` tanpa validasi dan tanpa handoff. Tidak ada call site frontend yang mengirim `true` — terverifikasi. Keputusan mempertahankan, membatasi, atau menghapusnya adalah `RJ-DOC-OQ-006` |
| **Aturan akhir** | Setelah roadmap ini `READY`, **tidak boleh ada dua implementasi finalisasi yang dapat menghasilkan state berbeda.** Semua permukaan bermuara pada satu implementasi |

### 3.2 `EncounterStatus` setelah dokter selesai

| Field | Isi |
|---|---|
| **Keputusan** | `EncounterStatus.ConsultationCompleted` (`7`) |
| **Status keputusan** | `RESOLVED BY SOURCE EVIDENCE` — tidak lagi memerlukan tebakan pemilik |

Buktinya ada pada enum itu sendiri,
`Areas/HealthServices/RegistrationManagement/Enums/EncounterStatus.cs`:

```text
InConsultation (6)  ->  ConsultationCompleted (7)  ->  Billing (8)  ->  Completed (9)
```

Lifecycle-nya sudah menyatakan bahwa selesainya konsultasi klinis **bukan** selesainya kunjungan.
Masih ada `Billing` (`8`) sebelum `Completed` (`9`).

`finish-consultation` hari ini melompat dari `6` langsung ke `9`, melewati `7` dan `8`. Dampaknya
konkret dan dapat ditunjuk:

| Consumer | Perlakuan terhadap `Completed` | Akibat lompatan |
|---|---|---|
| `MedicalRecordAccessAuditService.cs:74` | `StatusKunjunganSelesai` — kunjungan dianggap tidak berjalan | Kewenangan akses rekam medis berubah lebih awal daripada seharusnya |
| `MedicalRecordBackfillService.cs:49` | `StatusKunjunganSelesai` — *"catatan pada kunjungan berstatus ini akan dikunci, karena memang tidak seharusnya diubah lagi"* | Catatan klinis terkunci padahal farmasi, laboratorium, radiologi, dan Billing belum selesai |

Karena itu `ConsultationCompleted` bukan sekadar nama enum yang lebih cocok; memilih `Completed`
akan mengunci rekam medis sebuah kunjungan yang downstream-nya masih berjalan.

Satu catatan yang **tidak** diperbaiki dari roadmap ini: `EncounterStatus.Billing` (`8`) tidak
memiliki satu pun penulis maupun pembaca pada source. Siapa yang menaikkan encounter dari
`ConsultationCompleted` ke `Billing` lalu ke `Completed` adalah **pertanyaan milik Registration
dan Billing**, bukan milik Dokter — dicatat sebagai `RJ-DOC-NOTICE-001`.

---

## 4. Task roadmap scope Dokter

Prefix `RJ-DOC` mengikuti kontrak `_template/roadmap/README.md`: `<PREFIX>-BE-###` dan
`<PREFIX>-FE-###`. Nomor tidak pernah dipakai ulang.

### 4.0 Contract gate — `P0`, **`FROZEN` `2026-08-31`**

Kedua gate sudah ditutup. Artefaknya:
[`contracts/doctor-consultation-contracts.md`](../contracts/doctor-consultation-contracts.md).

#### ✅ `RJ-DOC-INT-001` — Freeze Completion Contract

| Field | Isi |
| --- | --- |
| **Status** | **`COMPLETE / FROZEN`** — `RJ-DOC-COMPLETION-001@1.0.0`, `RJ-DOC-DEC-006` |
| **Outcome** | Kontrak penyelesaian konsultasi dibekukan sebelum ada satu baris code yang mengonsumsinya |
| **Yang dibekukan** | Canonical endpoint `PATCH /doctor-consultations/{id}/complete` beserta nama parameter path `{id}`; identitas dan sumber actor; request DTO existing tanpa field baru; `409` concurrency; `400` validation beserta `IssueKey`/`Section`/`TabKey`/`Field`/`severity`; aturan stabilitas clinical order `RJ-DOC-DEC-004`; success contract; semantik encounter `InConsultation → ConsultationCompleted`; retry/idempotency; status `finish-consultation` sebagai orchestration layer; status `CompleteImmediately` beserta tiga compatibility requirement |
| **Acceptance criteria** | 1. ✅ Setiap field yang dikonsumsi `BE-001` s.d. `BE-004`, `BE-006`, dan `FE-001` s.d. `FE-003` tertulis, berversi, dan menunjuk bukti `file:line`. 2. ✅ Tidak ada perilaku yang tersisa sebagai asumsi implementer — sembilan butir yang belum ada pada source ditandai `TARGET` beserta task penutupnya |
| **Membuka** | `RJ-DOC-BE-001`, `BE-002`, `BE-003`, `BE-004`, `BE-006`, `FE-001`, `FE-002`, `FE-003` menjadi `ELIGIBLE` |
| **DoD** | ✅ Kontrak berversi dan `FROZEN`; implementasi **belum dimulai** |

#### ✅ `RJ-DOC-INT-002` — Freeze Producer Handoff Contract

| Field | Isi |
| --- | --- |
| **Status** | **`COMPLETE / FROZEN`** — `RJ-DOC-HANDOFF-001@1.0.0`, `RJ-DOC-DEC-006` |
| **Outcome** | Batas tanggung jawab producer terhadap consumer dibekukan sebelum implementasi durabilitas ditulis |
| **Yang dibekukan** | Aturan `per eligible milestone` beserta `nol eligible → nol fakta → VALID`; empat belas elemen identitas fakta; sembilan jaminan producer; tujuh hal yang **bukan** jaminan producer; kewajiban consumer-side idempotency; daftar eligibility mandatory (`Prescription finalization`, `Procedure execution`) dan conditional (Lab, Radiologi); enam recovery semantics |
| **Acceptance criteria** | 1. ✅ Kontrak menyatakan nol eligible milestone menghasilkan nol fakta dan itu **sah**, serta melarang aturan `every consultation must have a fact`. 2. ✅ Kontrak menyatakan consumer wajib menerapkan consumer-side idempotency. 3. ✅ Kontrak **tidak** membebankan pencegahan duplikasi charge kepada producer — `charge deduplication` tercantum eksplisit sebagai bukan jaminan Dokter |
| **Membuka** | `RJ-DOC-BE-005` menjadi `ELIGIBLE` |
| **DoD** | ✅ Kontrak berversi dan `FROZEN`; batas producer/consumer eksplisit |

> **`ELIGIBLE` bukan `AUTHORIZED`.** Pembekuan kontrak menghapus *gerbang kontrak*, bukan gerbang
> wewenang. `IMPLEMENTATION_AUTHORITY` tetap `NOT_GRANTED` untuk seluruh task.

### 4.1 Backend

#### ✅ `RJ-DOC-BE-001` — Satukan jalur penyelesaian ke canonical finalization

| Field | Isi |
| --- | --- |
| **Status** | ✅ **`COMPLETE` `2026-08-31`** — build solution `0 error`, `141` uji lulus `0` gagal, `9` di antaranya uji acceptance baru. Bukti: [task/report/backend/RJ-DOC-BE-001.md](../task/report/backend/RJ-DOC-BE-001.md) |
| **Outcome** | Menyelesaikan konsultasi dari layar dokter benar-benar memfinalkan `TrxDoctorConsultation`, melalui satu implementasi |
| **Cakupan** | Menerapkan keputusan `3.1`: `finish-consultation` menjadi orchestration wrapper di atas canonical finalization; menerapkan keputusan `3.2` sehingga `EncounterStatus` menjadi `ConsultationCompleted`; menutup `RJ-DOC-CAP-030` |
| **Reuse** | `ConsultationFinalizationService`, `ConsultationValidationService`, `DoctorConsultationLifecycleService` — sudah ada, tidak boleh ditulis ulang |
| **Dependency** | `RJ-DOC-INT-001` |
| **Acceptance criteria** | ✅ 1. Setelah `Selesai Konsultasi` berhasil, `ConsultationStatus = Completed`, `CompletedAt` dan `CompletedByUserId` terisi. ✅ 2. Efek antrean existing dipertahankan. ✅ 3. `EncounterStatus` menjadi `ConsultationCompleted` dari **setiap** permukaan. ✅ 4. Penguncian `ProgressNote` existing tetap berjalan. ✅ 5. Tidak tersisa dua implementasi finalisasi yang dapat menghasilkan state berbeda |
| **Perubahan perilaku** | Jalur antrean kini **dapat menolak** penyelesaian yang sebelumnya selalu berhasil, ketika dokumentasi klinis belum lengkap atau ada peringatan yang belum dikonfirmasi. Konsekuensi langsung `RJ-DOC-BE-002` dan `RJ-DOC-FE-002` yang belum dikerjakan. Penyelesaian tanpa validasi **sengaja tidak** dipertahankan sebagai jalan pintas |
| **Verifikasi** | Integration test terhadap seluruh permukaan; bukti transisi status |
| **Risiko** | `finish-consultation` adalah alur produksi aktif; `TrxDoctorConsultation` juga dipakai IGD (`BE-IGD-028`). Perubahan wajib mempertahankan keduanya |
| **DoD** | Satu implementasi canonical, terbukti test, tanpa regresi antrean maupun IGD |

#### ✅ `RJ-DOC-BE-002` — Jadikan validasi finalisasi mengikat

| Field | Isi |
| --- | --- |
| **Status** | ✅ **`COMPLETE` `2026-08-31`** — build solution `0 error`, `155` uji lulus `0` gagal, `14` di antaranya uji acceptance baru. Bukti: [task/report/backend/RJ-DOC-BE-002.md](../task/report/backend/RJ-DOC-BE-002.md) |
| **Outcome** | Konsultasi hanya dapat difinalisasi bila lolos validasi backend, bukan bila layar mengizinkan |
| **Cakupan** | `ConsultationValidationService` wajib berjalan pada canonical finalization. Menutup celah bahwa validasi hari ini hanya tersedia sebagai `GET` opsional |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | ✅ 1. `ErrorCount > 0` menolak finalisasi dengan `400` beserta payload validasi. ✅ 2. Warning yang belum di-acknowledge menolak finalisasi. ✅ 3. Konsultasi `Completed`/`Cancelled` tidak dapat difinalisasi ulang |
| **Aturan baru** | Tiga pemeriksaan keutuhan pesanan klinis ditambahkan sesuai kontrak bagian `1.6`, seluruhnya memakai state existing dan tanpa query tambahan: `INCONSISTENT_PROCEDURE_STATUS`, `PROCEDURE_ENCOUNTER_MISMATCH`, dan `PRESCRIPTION_ENCOUNTER_MISMATCH`. Yang terakhir mencegah fakta klinis mendarat pada kunjungan yang salah |
| **Batas `RJ-DOC-DEC-004`** | ✅ Terbukti: pesanan Lab yang sudah tersimpan tetapi belum dikerjakan **tidak** menahan penyelesaian, dan ketiadaan pesanan penunjang juga tidak |
| **Atomicity** | ✅ Terbukti: penolakan lewat jalur antrean tidak meninggalkan catatan antrean maupun penguncian catatan klinis yang terlanjur tersimpan |
| **DoD** | ✅ Tidak ada jalan pintas finalisasi yang melewati validasi; kedua permukaan memakai validator yang sama |

#### `RJ-DOC-BE-003` — Idempotency dan concurrency finalisasi

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P0** |
| **Outcome** | Klik ganda, retry, dan dua perangkat tidak menghasilkan finalisasi ganda maupun penimpaan senyap |
| **Cakupan** | Menutup TOCTOU pada `ConsultationFinalizationService.cs:62`. Pola advisory lock per encounter pada `DoctorConsultationLifecycleService.AcquireLifecycleLockAsync` adalah kandidat reuse pertama. Menentukan apakah `ExpectedUpdatedAt` menjadi wajib |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | 1. Dua permintaan finalisasi serentak: satu berhasil, satu berbalas hasil canonical atau `409` — tidak dua-duanya menulis. 2. Retry dengan operasi sama tidak menggandakan resep yang difinalkan, tindakan, milestone, maupun fact logis. 3. `CompletedAt`/`CompletedByUserId` tidak tertimpa permintaan kedua. 4. State basi dari perangkat lain berbalas `409` |
| **DoD** | Idempotent dan aman terhadap concurrency, terbukti test — bukan hanya terlindung tombol `disabled` |

#### `RJ-DOC-BE-005` — Durabilitas dan recoverability producer handoff

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P0** — durable handoff adalah bagian `END OF DOCTOR SCOPE`, bukan pekerjaan lanjutan |
| **Outcome** | Setiap eligible clinical milestone memiliki satu fact logis yang durable dan dapat dipulihkan, tanpa membebani producer dengan tanggung jawab finansial |
| **Cakupan** | `TrxClinicalMilestoneFact` sudah menyimpan `DispatchStatus`, `IdempotencyKey`, dan `DispatchAttemptCount` sebelum dispatch — fondasinya benar. Yang belum ada adalah **pembacanya**. Menyediakan jalur menemukan dan mengirim ulang fact `Pending`/`OutcomeUnknown`, dan menutup celah proses mati antara clinical commit dan penulisan fact |
| **Reuse** | `ClinicalMilestoneFactProducer`; index `IX_TrxClinicalMilestoneFact_DispatchStatus` |
| **Dependency** | `RJ-DOC-INT-002`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | 1. Konsultasi tetap `COMPLETED` walau downstream tidak dapat dihubungi. 2. Untuk **setiap eligible** clinical milestone terdapat tepat satu fact logis yang durable dengan identity dan versi stabil. 3. **Konsultasi tanpa eligible milestone menghasilkan nol fact, dan itu `VALID`** — bukan galat, bukan gap. 4. Konsultasi yang **memiliki** eligible milestone tetapi fact-nya tidak terbit terdeteksi sebagai `RECOVERABLE PRODUCER GAP`. 5. Fact yang belum terkirim dapat ditemukan kembali. 6. Retry producer memakai identity yang sama sehingga **tidak menggandakan fact logis / milestone identity** |
| **Batas tanggung jawab** | Acceptance criteria di atas **sengaja tidak** menyebut charge. Producer tidak menjamin charge Billing tidak duplikat — itu **consumer-side idempotency** milik `RJ-BIL-*`. Rekonsiliasi finansial, dead-letter finansial, dan recovery report Billing juga di luar task ini |
| **DoD** | Handoff durable, eligibility-aware, dan dapat dipulihkan di sisi producer |

#### `RJ-DOC-BE-004` — Completed-state protection

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P1** |
| **Outcome** | Konsultasi yang sudah selesai tidak dapat diedit bebas |
| **Cakupan** | Memverifikasi ketujuh penjaga `ConsultationStatus == Completed` benar-benar aktif setelah `RJ-DOC-BE-001`, lalu menutup sisa permukaan tulis. Sisa yang sudah terlihat: order Lab dan Radiologi terikat `EncounterId` saja tanpa `ConsultationId`, sehingga tidak tersentuh penjaga mana pun; dan jalan pintas `CompleteImmediately` |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | 1. SOAP, diagnosis, resep, dan tindakan menolak perubahan setelah `Completed`. 2. Setiap penolakan memberi pesan yang menjelaskan sebab, bukan `500`. 3. Perilaku order Lab/Radiologi setelah konsultasi selesai dinyatakan eksplisit |
| **Risiko** | **`NEEDS CONFIRMATION`** — capability reopen/correction tidak ada pada source. Jangan menciptakan workflow reopen tanpa `RJ-DOC-OQ-004` |
| **DoD** | Permukaan tulis pasca-finalisasi terdaftar lengkap; yang dibiarkan terbuka disertai alasan |

#### `RJ-DOC-BE-006` — Audit trail clinical completion

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P1** |
| **Outcome** | Setiap penyelesaian konsultasi meninggalkan jejak actor, waktu, dan hasilnya |
| **Cakupan** | Jalur `finish-consultation` hari ini tidak memanggil logger sama sekali. Menyamakannya dengan pola `AuditAsync` pada `ClinicalMilestoneFactProducer` |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | 1. Penyelesaian berhasil tercatat beserta actor, waktu, dan jumlah order yang difinalkan. 2. Penolakan validasi tercatat. 3. Data klinis sensitif tidak masuk log |
| **DoD** | Audit trail tersedia dan tidak membocorkan data klinis |

#### `RJ-DOC-BE-007` — Verifikasi otomatis backend scope Dokter

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P1** |
| **Outcome** | Setiap acceptance criteria backend punya bukti test |
| **Cakupan** | Menutup `RJ-DOC-CAP-029` sisi backend |
| **Dependency** | `RJ-DOC-BE-001` s.d. `RJ-DOC-BE-006` |
| **Acceptance criteria** | 1. Setiap acceptance criteria `BE-001` s.d. `BE-006` punya test atau pemilik gap-nya bernama. 2. Build backend lulus |
| **Catatan** | `BillingTests` bukan tempat yang tepat untuk test klinis; penempatan mengikuti konvensi existing |
| **DoD** | Laporan cakupan lengkap; tidak ada `DONE` palsu |

### 4.2 Frontend

#### `RJ-DOC-FE-001` — Sambungkan `Selesai Konsultasi` ke canonical finalization

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P0** |
| **Outcome** | Tombol `Selesai Konsultasi` memanggil endpoint yang benar-benar memfinalkan konsultasi |
| **Cakupan** | `handleConfirmFinalizeConsultation` sudah melakukan bagian tersulitnya dengan benar: mem-flush pending autosave berurutan dan membatalkan finalisasi bila salah satu gagal. Yang berubah adalah **tujuan panggilannya**. `completeDoctorConsultation` sudah ada dan siap dipakai |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-BE-001` |
| **Acceptance criteria** | 1. Finalisasi berhasil menghasilkan konsultasi `COMPLETED` yang terbaca dari server. 2. Pending autosave tetap ter-flush sebelum finalisasi. 3. UI tidak berpura-pura sukses bila backend gagal |
| **Wewenang UI** | Detail tampilan `DEV_DISCRETION`; **tujuan endpoint bukan `DEV_DISCRETION`** |
| **DoD** | Satu jalur, terbukti; tidak ada mutasi finansial dari frontend |

#### `RJ-DOC-FE-002` — Tampilkan hasil validasi finalisasi

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P1** |
| **Outcome** | Dokter tahu apa yang harus diperbaiki sebelum konsultasi dapat ditutup |
| **Cakupan** | Mengonsumsi `ConsultationFinalizationValidationResponse`. Struktur `Section`/`TabKey`/`Field`/`IssueKey` sudah dirancang untuk mengarahkan dokter ke tab yang tepat. Acknowledgement warning memakai `AcknowledgedWarningKeys` |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-FE-001`, `RJ-DOC-BE-002` |
| **Acceptance criteria** | 1. Error ditampilkan per tab beserta pesannya. 2. Warning dapat di-acknowledge secara sadar. 3. `400` validasi **tidak** ditampilkan sebagai galat sistem |
| **DoD** | Tidak ada finalisasi yang gagal tanpa penjelasan |

#### `RJ-DOC-FE-003` — Status authoritative, konflik, dan penguncian di layar

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P1** |
| **Outcome** | Layar memantulkan status konsultasi yang sebenarnya, termasuk setelah refresh dan setelah konflik |
| **Cakupan** | Membaca `ConsultationStatus` authoritative, bukan hanya `QueueStatus`. Menangani `409` sebagai konflik konsultasi beserta reload terkontrol. Mengunci editor setelah `COMPLETED`. Mengirim field concurrency sesuai `RJ-DOC-INT-001` |
| **Dependency** | `RJ-DOC-INT-001`, `RJ-DOC-FE-001`, `RJ-DOC-BE-003`, `RJ-DOC-BE-004` |
| **Acceptance criteria** | 1. Refresh setelah sukses tetap menampilkan `COMPLETED`. 2. `409` menampilkan konflik beserta reload terkontrol, tidak menimpa diam-diam. 3. Editor terkunci setelah `COMPLETED`. 4. Response versi lama tidak menimpa state yang lebih baru |
| **DoD** | Layar tidak pernah menampilkan keadaan yang lebih optimis daripada server |

#### `RJ-DOC-FE-004` — Verifikasi otomatis frontend scope Dokter

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P2** |
| **Outcome** | Perilaku finalisasi punya bukti test |
| **Cakupan** | `tests/unit` tidak memuat satu pun berkas dokter/konsultasi. Test untuk kirim ganda, stale response, urutan flush autosave, dan penanganan `409` |
| **Dependency** | `RJ-DOC-FE-001` s.d. `RJ-DOC-FE-003` |
| **Catatan** | Harness `node --test` tanpa `@testing-library` — render test belum mungkin |
| **DoD** | Setiap acceptance criteria kritis punya test atau pemilik gap-nya bernama |

### 4.3 Verification

#### `RJ-DOC-VER-001` — Verifikasi end-to-end penyelesaian konsultasi

| Field | Isi |
| --- | --- |
| **Status** | `NOT_STARTED` — **P2**, gerbang terakhir scope Dokter |
| **Outcome** | Seluruh butir `MANDATORY BASELINE DoD` terbukti; invariant diverifikasi ulang; butir `CONDITIONAL` dinyatakan apa adanya |
| **Dependency** | Seluruh task `RJ-DOC-*` |
| **Acceptance criteria** | 1. Build backend dan frontend lulus. 2. Test relevan lulus. 3. Ketiga invariant `RJ-DOC-INV-001..003` diverifikasi ulang. 4. Tidak ada butir DoD ditandai selesai tanpa bukti |
| **DoD** | Blueprint dapat menjawab *"apakah pekerjaan Dokter sudah selesai"* tanpa melihat Billing |

### 4.4 Urutan dependency

```text
        RJ-DOC-INT-001  (Completion Contract)      [P0 GATE]
        RJ-DOC-INT-002  (Producer Handoff Contract) [P0 GATE]
                        |
        +---------------+----------------+
        |                                |
        v                                v
RJ-DOC-BE-001                    contract-ready FE
canonical completion
        |
        +--> RJ-DOC-BE-002  validasi mengikat            [P0]
        +--> RJ-DOC-BE-003  idempotency / concurrency    [P0]
        +--> RJ-DOC-BE-005  durable producer handoff     [P0]
        +--> RJ-DOC-BE-004  completed-state protection   [P1]
        +--> RJ-DOC-BE-006  audit trail                  [P1]
                        |
                        v
                 BACKEND READY
                        |
                        v
        RJ-DOC-FE-001  tombol -> canonical completion    [P0]
                        |
        +--> RJ-DOC-FE-002  validation UX                [P1]
        +--> RJ-DOC-FE-003  authoritative status / 409 / lock  [P1]
                        |
                        v
        RJ-DOC-BE-007 + RJ-DOC-FE-004  automated verification
                        |
                        v
                RJ-DOC-VER-001
                        |
                        v
                DOCTOR SCOPE DONE
```

Implementasi **BLOCKED** sampai `RJ-DOC-INT-001` dan `RJ-DOC-INT-002` freeze.

---

## 5. Definition of Done scope Dokter

DoD dipisah menurut kelas capability pada `2.2`. Hanya `MANDATORY BASELINE` yang menentukan
apakah pekerjaan Dokter selesai.

### 5.1 `MANDATORY BASELINE DoD`

| # | Butir | Keadaan `2026-08-31` | Ditutup oleh |
|---|---|---|---|
| 1 | Dokter dapat membuka consultation pasien Rawat Jalan | terbukti | — |
| 2 | Data consultation authoritative berasal dari backend | sebagian — layar membaca `QueueStatus` | `FE-003` |
| 3 | Dokumentasi klinis wajib dapat diselesaikan | terbukti | — |
| 4 | Prescription dapat dibuat | terbukti | — |
| 5 | Prescription difinalkan pada penyelesaian konsultasi | belum tercapai | `BE-001` |
| 6 | Procedure dapat dibuat dan dieksekusi | terbukti | — |
| 7 | Pending autosave tidak hilang saat konsultasi diselesaikan | terbukti | — |
| 8 | Tombol `Selesai Konsultasi` terhubung ke canonical finalization | **tidak** — memanggil endpoint antrean | `BE-001` + `FE-001` |
| 9 | Hanya ada satu implementasi finalisasi | **tidak** — tiga permukaan, hasil berbeda | `BE-001` |
| 10 | Backend memvalidasi consultation sebelum complete | service ada, tidak pernah dipanggil | `BE-002` |
| 11 | Consultation hanya dapat difinalisasi secara valid | belum | `BE-002` |
| 12 | Successful completion menghasilkan `COMPLETED` | belum pada alur nyata | `BE-001` |
| 13 | `CompletedAt` tersimpan | belum pada alur nyata | `BE-001` |
| 14 | `CompletedBy`/actor tersimpan | belum pada alur nyata | `BE-001` |
| 15 | `EncounterStatus` konsisten dari setiap permukaan | **tidak** — `Completed` vs `ConsultationCompleted` | `BE-001` |
| 16 | Double click tidak menggandakan finalization | terlindung layar; backend TOCTOU | `BE-003` |
| 17 | Retry tidak menggandakan fact logis / milestone identity | producer idempotent; finalisasi belum | `BE-003` + `BE-005` |
| 18 | Stale update / multi-device ditangani aman | `ExpectedUpdatedAt` opsional dan tidak dikirim | `BE-003` + `FE-003` |
| 19 | Refresh setelah sukses tetap `COMPLETED` | belum | `FE-003` |
| 20 | UI tidak berpura-pura sukses bila backend gagal | terbukti | — |
| 21 | Consultation completed tidak dapat diedit bebas | penjaga ada tetapi inert | `BE-004` |
| 22 | Setiap **eligible** clinical milestone punya satu fact durable ber-identity stabil | resep tidak pernah terbit; tindakan terbit | `BE-001` + `BE-005` |
| 23 | Nol eligible milestone menghasilkan nol fact, dan itu sah | belum dinyatakan kontrak | `INT-002` + `BE-005` |
| 24 | Fact yang belum terkirim dapat ditemukan dan dikirim ulang oleh producer | tidak ada pembaca ulang | `BE-005` |
| 25 | Audit trail penyelesaian tersedia | jalur yang dipakai tidak memanggil logger | `BE-006` |
| 26 | Automated test relevan lulus | nol test | `BE-007` + `FE-004` |
| 27 | Build FE dan BE relevan lulus | `NEEDS CONFIRMATION` — tidak dijalankan pada task audit ini | `VER-001` |

Ringkasan: **`7` terbukti, `10` sebagian, `9` belum, `1` perlu konfirmasi.**

### 5.2 `CONDITIONAL DoD` — tidak membuat Doctor DoD gagal

| Butir | Keadaan | Keputusan |
|---|---|---|
| Lab order tersedia dari workspace dokter | BE ada; FE nihil | `RJ-DOC-OQ-003` |
| Radiology order tersedia dari workspace dokter | BE ada; FE nihil | `RJ-DOC-OQ-003` |
| Perilaku ancillary yang spesifik per unit | belum ditentukan | `RJ-DOC-OQ-003`, `RJ-DOC-OQ-005` |

### 5.3 `ARCHITECTURAL INVARIANTS` — diverifikasi, tidak dihitung

`RJ-DOC-INV-001`, `RJ-DOC-INV-002`, `RJ-DOC-INV-003` — ketiganya `VERIFIED` pada `2.2.3`.
Wajib diverifikasi ulang pada `RJ-DOC-VER-001`, dan **tidak pernah** menjadi angka progress.

### 5.4 `DOWNSTREAM` — di luar Doctor DoD

Seluruh pemrosesan finansial `RJ-BIL-*`. Tidak masuk Doctor DoD dalam bentuk apa pun.

---

## 6. Batas dan hal yang perlu keputusan pemilik

### 6.1 Yang sengaja **tidak** ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Billing Folio, charge, tarif | `DOWNSTREAM` — `RJ-BIL-BE-001` |
| Payer allocation, patient responsibility | `DOWNSTREAM` — `RJ-BIL-BE-005` |
| Financial action, approval, void, refund, write-off | `DOWNSTREAM` — `RJ-BIL-BE-006` |
| Rekonsiliasi finansial, dead-letter, recovery report | `DOWNSTREAM` — `RJ-BIL-BE-007` |
| Klaim, settlement, kasir, invoice | `DOWNSTREAM` — `RJ-BIL-BE-008` |
| Layar Billing apa pun | `DOWNSTREAM` — `RJ-BIL-FE-001` s.d. `FE-007` |
| Adapter payer eksternal | `OUT OF SCOPE` — `RJ-BIL-DEP-009` `INACTIVE` |
| Menjamin charge Billing tidak duplikat | `DOWNSTREAM` — consumer-side idempotency |
| Menaikkan encounter dari `ConsultationCompleted` ke `Billing`/`Completed` | `RJ-DOC-NOTICE-001` — milik Registration dan Billing |
| Workflow reopen konsultasi | **`NEEDS CONFIRMATION`** — `RJ-DOC-OQ-004` |
| Menarik `paymentStatus` dari payload klinis | Milik Farmasi bersama Billing |

### 6.2 Yang perlu dijawab pemilik

**Seluruh open question scope Dokter sudah tertutup.** Tidak ada yang tersisa.

| ID | Pertanyaan | Keputusan | Ditutup oleh |
|---|---|---|---|
| ~~`RJ-DOC-OQ-001`~~ | Endpoint mana yang canonical? | `PATCH /doctor-consultations/{id}/complete`; `finish-consultation` menjadi orchestration layer | Bukti source, bagian `3.1` |
| ~~`RJ-DOC-OQ-002`~~ | `EncounterStatus` setelah selesai? | `ConsultationCompleted` (`7`) | Bukti source, bagian `3.2` |
| ~~`RJ-DOC-OQ-003`~~ | Lab dan Radiologi pada rilis pertama? | **`CONDITIONAL — NOT PART OF CURRENT MANDATORY DOCTOR BASELINE`.** FE ordering Lab dan Radiologi bukan blocker Doctor DoD. Keduanya tetap capability valid yang dapat dinaikkan menjadi mandatory lewat approval terpisah; capability dan gap-nya **tidak dihapus** | `RJ-DOC-DEC-002` |
| ~~`RJ-DOC-OQ-004`~~ | Koreksi/reopen setelah `COMPLETED`? | **`NO ARBITRARY REOPEN IN CURRENT BASELINE`.** Konsultasi `Completed` wajib protected/locked dari normal editing. **Dilarang menciptakan workflow reopen generik.** Bila dibutuhkan kelak, ia menjadi capability tersendiri dengan reason, actor, authorization, audit trail, version/correction semantics, dan approval owner eksplisit | `RJ-DOC-DEC-003` |
| ~~`RJ-DOC-OQ-005`~~ | Boleh selesai dengan order penunjang terbuka? | **`ALLOWED WITH VALID DOCTOR-SIDE ORDER STATE`.** `DOCTOR ORDER CREATION MUST BE STABLE`, tetapi `ANCILLARY EXECUTION DOES NOT NEED TO BE FINISHED`. Masuk validation contract | `RJ-DOC-DEC-004`, dibekukan pada kontrak bagian `1.6` |
| ~~`RJ-DOC-OQ-006`~~ | `CompleteImmediately` dipertahankan, dibatasi, atau dihapus? | **`DEPRECATE / RESTRICT AS ALTERNATE FINALIZATION PATH`.** Bukan canonical; consumer baru dilarang memakainya untuk Rawat Jalan normal; API **tidak dihapus** pada task ini; remediasi pada `RJ-DOC-BE-001` setelah freeze; tiga compatibility requirement wajib dijaga | `RJ-DOC-DEC-005`, dibekukan pada kontrak bagian `1.11` |

Satu catatan tetap terbuka dan **bukan milik Dokter**: `RJ-DOC-NOTICE-001` — `EncounterStatus.Billing`
(`8`) tidak memiliki penulis maupun pembaca pada source. Siapa yang menaikkan encounter dari
`ConsultationCompleted` ke `Billing` lalu ke `Completed` adalah pertanyaan Registration dan Billing.

---

## 7. Hubungan dengan roadmap Billing

```text
DOCTOR / CLINICAL  -- roadmap ini, prefix RJ-DOC

   Selesai Konsultasi (canonical)
          |
          v
   Untuk setiap ELIGIBLE clinical milestone:
   TrxClinicalMilestoneFact
   (SourceContext, SourceAggregateId, MilestoneFactId,
    MilestoneFactVersion, EncounterId, EffectType,
    IdempotencyKey, TariffSnapshot)
   Nol eligible milestone -> nol fact -> VALID
          |
          v
======== INTEGRATION CONTRACT -- RJ-BIL-INT-001 ========
   Consumer WAJIB menerapkan consumer-side idempotency
          |
          v
BILLING / REVENUE CYCLE  -- roadmap RJ-BIL

   BillingFolioService.RecognizeMilestoneAsync
          |
          v
   Folio . Charge . Tariff . Payer . Payment . Claim
```

Seluruh task `RJ-BIL-*` berlabel **`DOWNSTREAM — NOT PART OF DOCTOR DEFINITION OF DONE`**.

Dua arah ketergantungan yang perlu diketahui pemilik Billing, dan **tidak satu pun** membuat
Billing menjadi blocker Dokter:

| Output Dokter | Yang menunggunya di sisi Billing |
|---|---|
| `RJ-DOC-CAP-015` + `RJ-DOC-CAP-030` canonical completion | Kesiapan handoff resep — tanpa ini `RJ-BIL-BE-002` tidak pernah menerima fact resep |
| `RJ-DOC-CAP-023` durabilitas producer handoff | Kesiapan konsumsi yang andal — consumer tidak dapat memulihkan apa yang tidak pernah dapat ditemukan producer |

---

## 8. Aturan eksekusi

Roadmap ini `OWNER_APPROVED` (`RJ-DOC-DEC-001`) dan kedua contract gate sudah `FROZEN`
(`RJ-DOC-DEC-006`). **Yang disetujui adalah rencananya, bukan eksekusinya.**

| Gerbang | Keadaan |
|---|---|
| Approval roadmap | ✅ `OWNER_APPROVED` `2026-08-31` |
| Contract freeze | ✅ `RJ-DOC-COMPLETION-001@1.0.0`, `RJ-DOC-HANDOFF-001@1.0.0` |
| `IMPLEMENTATION_AUTHORITY` | ⛔ **`NOT_GRANTED`** — diberikan terpisah per task |
| `BUILDER_EXECUTION` | ⛔ **`NOT_AUTHORIZED`** |

Task berstatus `ELIGIBLE` — `RJ-DOC-BE-001`, `BE-002`, `BE-003`, `BE-005`, dan turunannya —
berarti dependency kontraknya sudah terpenuhi, **bukan** berarti boleh dikerjakan. Setiap handoff
wajib menyertakan task ID, approval task, kontrak terkunci beserta versinya, source SHA dan keadaan
working tree, dependency state, preflight, dan bukti acceptance yang diminta.

Tidak satu pun task di dokumen ini memberi izin: mengubah application source, menjalankan builder,
commit, push, merge, deployment, membuat atau menjalankan migration, mengubah database,
mengaktifkan `RJ-BIL-DEP-009`, atau mengerjakan Billing.
