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

---

## 9. Revisi UAT `2026-09-28` — `RJ-DOC-REV-*`

### 9.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Dua dokumen UAT pemilik: *Skrining Pasien* dan *Dokter Rawat Jalan* (catatan `28/9/2026`) |
| **Keputusan** | `RJ-DOC-DEC-007` — `2026-10-01`, Sukma Giri. Kedua dokumen diterima sebagai requirement yang disetujui dan dicatat sebagai task `RJ-DOC-REV-*` |
| **`RJ-DOC-OQ-003` dibuka ulang** | `RJ-DOC-DEC-008` — order Lab dan Radiologi dari workspace dokter **naik menjadi scope revisi ini**. `RJ-DOC-CAP-010` dan `CAP-011` tidak lagi `CONDITIONAL` |
| **`IMPLEMENTATION_AUTHORITY`** | **`GRANTED`** untuk seluruh task `RJ-DOC-REV-BE-*` dan `RJ-DOC-REV-FE-*`, termasuk membuat migration dan menjalankan migration/seed ke `QuilvianNewDevSukma`. Commit, push, merge, deploy, dan database lain **tetap tidak** termasuk |
| **CPPT** | Tetap disembunyikan (tab dan render dikomentari). Tidak ada task CPPT pada revisi ini |
| **Tidak termasuk** | Penghapusan data test (`RJ-DOC-DEC-009`: tidak dilakukan) |
| **Baseline source** | Backend `33ee2955` cabang `sukmagp`, worktree `CLEAN`. Frontend `7969f959` cabang `sukmagpV2`, worktree `CLEAN` |
| **Verifikasi** | Pola Bank Darah: build, `has-pending-model-changes`, QBE Strict, runtime HTTP ke `QuilvianNewDevSukma`. Tidak ada project test |

### 9.1 Temuan audit yang menjadi dasar task

| # | Temuan | Bukti |
| --- | --- | --- |
| A1 | Layar perawat membaca `primaryGuarantorNameSnapshot`, `primaryGuarantorTypeSnapshot`, `isInsurancePatient`, `isCompanyPatient`, tetapi response antrean perawat tidak mengirim keempatnya, sehingga tampil kosong atau `Tidak` | `PatientInformationTab.jsx:46-66`; `NurseStationQueueController.cs:1189-1199` |
| A2 | Endpoint surat dokter `/doctor-certificates` dipanggil frontend tetapi **tidak ada** di backend; surat tidak pernah tersimpan dan riwayat tidak mungkin dibuat | `doctor-queue.service.js:14-15`; nol hasil `DoctorCertificate` di `Areas/` |
| A3 | Master tindakan (`5495` baris lolos filter dokter rawat jalan) dan master obat (`7862` baris) ada di DB; penyebab katalog kosong di layar perlu dibuktikan runtime | query read-only `QuilvianNewDevSukma` `2026-10-01` |
| A4 | `MstDoctorServiceRule` hanya `2` baris, keduanya data test; `0` rule konsultasi untuk `11` dokter aktif berjadwal. Tarif konsultasi (`IsConsultationFee`) sudah ada `2130` baris untuk `113` tindakan konsultasi | query read-only |
| A5 | Tab Penunjang Medis masih `WorkInProgressTab`; tab hasil hanya Radiologi | `doctor-queue-view.jsx:198-213` |
| A6 | CSV ICD (`icd10_202609281504.csv`, `icd_diagnosa_202609281508.csv`) belum ada di repo | — |

### 9.2 Backend

| Task | Isi | Acceptance criteria | Dependency | Status |
| --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-001` | Data penjamin dan identitas pasien pada antrean perawat dan dokter | 1. Antrean perawat dan dokter mengirim `PrimaryGuarantorNameSnapshot`, `PrimaryGuarantorTypeSnapshot`, `IsInsurancePatient`, `IsCompanyPatient` yang diturunkan dari penjamin aktif kunjungan (`RegPatientEncounterGuarantor`). 2. `PaymentType = Insurance` ⇒ `IsInsurancePatient = true`; `CompanyGuarantor` ⇒ `IsCompanyPatient = true`. 3. Antrean dokter juga mengirim jenis kelamin, umur, alergi aktif, dan rujukan foto KTP/kartu penjamin | — | ✅ `COMPLETE` `2026-10-01` — build `0 Error`, QBE Strict `PASS`, `has-pending-model-changes` bersih, runtime R0–R3 `4/4 PASS`; jalur `CompanyGuarantor` hanya terbukti lewat kode (tidak ada data valid). [Laporan](../task/report/backend/RJ-DOC-REV-BE-001.md) |
| ✅ `RJ-DOC-REV-BE-002` | Risiko jatuh tidak dipilih tidak tersimpan dengan skor | Asesmen dengan `HasFallRisk = false` menyimpan `FallRiskScore` `null`/`0`, tidak lebih dari `0` | — | ✅ `COMPLETE` `2026-10-01` — tanpa perubahan kode: `50/50` asesmen tanpa risiko jatuh berskor `null` (query + runtime `GET /patient-assessments`). Angka `1` di layar adalah enum `FallRiskStatus.NoRisk`, diperbaiki di `REV-FE-002`. [Laporan](../task/report/backend/RJ-DOC-REV-BE-002.md) |
| 🟡 `RJ-DOC-REV-BE-003` | Perbaiki simpan SOAP | `PATCH /doctor-consultations/{id}/soap` dari alur dokter rawat jalan berbalas `200` dan tersimpan; penyebab kegagalan dibuktikan runtime | — | 🟡 `PARTIAL` `2026-10-01` — tanpa perubahan kode; runtime R1–R5 `5/5 PASS` (buat konsultasi, diagnosis, rekomendasi, autosave, muat ulang). Kriteria *penyebab dibuktikan runtime* belum terpenuhi: kegagalan UAT `28/9` tidak dapat direproduksi; dugaan terkuat skema DB tertinggal (`20260930110000`) atau hak akses `WriteSoap`. [Laporan](../task/report/backend/RJ-DOC-REV-BE-003.md) |
| ✅ `RJ-DOC-REV-BE-004` | Surat dokter: persistence, API, dan riwayat | 1. Entity + migration surat dokter. 2. `POST`, `PUT /{id}`, `GET active-by-queue/{queueId}`, dan riwayat per pasien. 3. Dokter yang tercatat adalah Dokter Penanggung Jawab kunjungan. 4. Tujuan rujukan disimpan sebagai rujukan master (unit/klinik) beserta snapshot namanya | — | ✅ `COMPLETE` `2026-10-01` — tabel `CliDoctorCertificate` + migration `20261001040508` diterapkan ke Sukma; build `0 Error`, QBE Strict `PASS` (17 berkas), drift nihil, runtime R0–R13 `PASS`. Hak akses `DoctorCertificate` perlu dicentang admin. [Laporan](../task/report/backend/RJ-DOC-REV-BE-004.md) |
| ✅ `RJ-DOC-REV-BE-005` | Master data aturan layanan dokter dan relasinya | Seeder idempoten: setiap dokter aktif berjadwal punya `MstDoctorServiceRule` konsultasi per (unit layanan, klinik) yang menunjuk tindakan konsultasi bertarif `IsConsultationFee`; relasi `MstDoctor`, `MstServiceUnit`, `MstClinic`, `MstTariffCategory`, `MstTariff`, `MstProcedure`, `MstPatientClass` valid; resolver tarif konsultasi menemukan tarif untuk kelas `RAWAT JALAN` | — | ✅ `COMPLETE` `2026-10-01` — skrip idempoten `seed-doctor-service-rules-outpatient.sql` + `grant-doctor-certificate-access.sql` dijalankan 2× di Sukma: `20` aturan untuk `11` dokter, `0` tanpa tarif konsultasi, `12` policy hak akses; terbaca via API master (`200`, `totalData 20`). [Laporan](../task/report/backend/RJ-DOC-REV-BE-005.md) |
| ✅ `RJ-DOC-REV-BE-006` | Grouping ICD-10 berdasarkan ICD Diagnosa | Mengikuti CSV pemilik | CSV `A6` | ✅ `COMPLETE` `2026-10-01` — `MstDiagnosisGroup` (`528` kelompok DTD) + `MstDiagnosis.DiagnosisGroupId`, migration `20261001043557` diterapkan ke Sukma; `18079/18543` kode ICD-10 terpetakan; seeder idempoten (`19 s` → `0,6 s`); QBE `PASS`; runtime `kolera`/`I10`/`J18`/`leukemia` terkelompok. CSV `icd10` terpotong `1000` baris — sisanya via rentang. [Laporan](../task/report/backend/RJ-DOC-REV-BE-006.md) |

### 9.3 Frontend

| Task | Isi | Acceptance criteria | Dependency | Status |
| --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-001` | Skrining Pasien (perawat) | 1. Penjamin utama, jenis penjamin, pasien asuransi/perusahaan tampil benar. 2. Baris Pembayaran Campuran, Eligibility Diperlukan, Eligibility Selesai, No. Eligibility dihapus. 3. Alert isian tidak sesuai pada Tanda Vital tampil di field masing-masing | `REV-BE-001` | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Test baru `5/5`. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-001.md) |
| ✅ `RJ-DOC-REV-FE-002` | Hasil Skrining dokter dan header pasien | 1. Risiko jatuh tidak dipilih tampil `0`/`-`. 2. Tanggal skrining berspasi. 3. Header menampilkan jenis kelamin, umur, alergi, dan foto KTP/kartu penjamin. 4. Label `Jenis Pembayaran` menjadi `Asuransi/ Penjamin` | `REV-BE-001`, `REV-BE-002` | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Test baru `5/5`. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-002.md) |
| ✅ `RJ-DOC-REV-FE-003` | SOAP | 1. Badge nama pasien, No. RM, dan status `SOAP siap diedit` dihapus dari header. 2. Subjective = keluhan utama asesmen terakhir tersimpan. 3. Objective = vital sign dasar terakhir tersimpan. 4. Assessment = diagnosa ICD terpilih. 5. Simpan SOAP berhasil | `REV-BE-003` | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Test baru `3/3`; dependency `REV-BE-003` 🟡. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-003.md) |
| ✅ `RJ-DOC-REV-FE-004` | Resep — tampilkan master obat | Katalog obat menampilkan master obat dan dapat dipilih | — | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Katalog obat langsung tampil (`3015` obat). [Laporan](../task/report/frontend/RJ-DOC-REV-FE-004.md) |
| ✅ `RJ-DOC-REV-FE-005` | Tindakan — tampilkan master tindakan | Katalog tindakan menampilkan master tindakan dan dapat dipilih | — | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Katalog tindakan langsung tampil (`50` baris). [Laporan](../task/report/frontend/RJ-DOC-REV-FE-005.md) |
| ✅ `RJ-DOC-REV-FE-006` | Surat Dokter | 1. Diagnosa terisi dari Assessment SOAP terakhir. 2. Riwayat surat dokter. 3. Unit/Tujuan Rujukan berupa pilihan yang dapat dicari. 4. Label `Dokter Pemeriksa` menjadi `Dokter Penanggung Jawab` | `REV-BE-004` | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Test baru `2/2`; surat kini ikut tersimpan saat Selesai Konsultasi. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-006.md) |
| ✅ `RJ-DOC-REV-FE-007` | Penunjang Medis — order Lab dan Radiologi dari dokter | Dokter dapat membuat dan melihat order Lab dan Radiologi kunjungan ini memakai endpoint `LabOrder`/`RadOrder` existing | — | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Runtime order Lab `201`, Radiologi `201`. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-007.md) |
| ✅ `RJ-DOC-REV-FE-008` | Hasil Penunjang Medis | Tab `Hasil Radiologi` menjadi `Hasil Penunjang Medis` dan menampilkan hasil Lab dan Radiologi kunjungan per kategori penunjang | `REV-FE-007` | ✅ `COMPLETE` `2026-10-01` — lint `0 error`, `next build` `PASS`, unit `2169/2175` (6 gagal milik modul lain, `EXISTING`); manual klik `NOT FEASIBLE` (tanpa peramban) — kontrak data diuji runtime HTTP. Hasil Lab per disiplin + panel Radiologi. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-008.md) |

## 10. Revisi `2026-10-01` — prioritas Skrining

### 10.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Catatan pemilik 1 Okt 2026 pukul 16.09, "Prioritas Screening" butir 1–3 |
| **Keputusan** | `RJ-DOC-DEC-010` — `2026-10-01`, Sukma Giri (dipilih lewat sesi agent): (a) EWS mengacu NEWS2/MEWS; keduanya hanya menilai sistolik, sehingga diastolik **ditampilkan** di tabel EWS tanpa menambah skor, dengan interpretasi memakai ambang abnormal/kritis backend yang sudah ada (< 60 / > 110 abnormal, ≥ 120 kritis). (b) Kategori IMT dewasa memakai standar Kemenkes RI: < 18,5 kurang; 18,5–25,0 normal; > 25,0–27,0 lebih; > 27,0 obesitas; pasien < 18 tahun tidak dikategorikan (pakai IMT/U). (c) Satu pasien satu kunjungan aktif: pendaftaran rawat jalan (petugas dan Kiosk) ditolak selama pasien masih punya kunjungan yang belum Selesai/Batal/Tidak Hadir, **tanggal berapa pun** |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` untuk `RJ-DOC-REV-BE-007` dan `RJ-DOC-REV-FE-009`. Tanpa migration. Commit, push, merge, deploy tidak termasuk |
| **Baseline source** | Backend `27fd8fb4` (`sukmagp`), frontend `fa9d5dd2` (`sukmagpV2`), keduanya bersih |

### 10.1 Temuan audit

| # | Temuan | Bukti |
| --- | --- | --- |
| B1 | EWS tidak memuat diastolik di preview FE (dua salinan util: perawat dan dokter) maupun skor tersimpan BE | `ews.utils.js`; `PatientVitalSignCalculation.CalculateEwsScore` |
| B2 | BMI hanya angka, tanpa kategori | `vital-preview.utils.js#calculateBmiPreview` |
| B3 | Create kunjungan (admin dan Kiosk) tidak memeriksa kunjungan aktif. DB dev: `14` pasien punya lebih dari satu kunjungan yang belum selesai | `PatientEncounterController.ValidateCreateRequestAsync`; query read-only `2026-10-01` |

### 10.2 Task

| Task | Isi | Acceptance criteria | Status |
| --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-007` | Satu kunjungan aktif per pasien | 1. `POST /admin` dan `POST /kiosk` (alias `/`) menolak `400` bila pasien punya kunjungan yang masih berjalan menurut definisi tunggal `KunjunganMasihBerjalan`; pesan menyebut nomor dan tanggal kunjungan itu. 2. Setelah kunjungan itu dibatalkan/selesai, pendaftaran berhasil. 3. Dua permintaan bersamaan menghasilkan tepat satu kunjungan | ✅ `COMPLETE` `2026-10-01` — build Release `0 Error` (244 warning, tanpa warning baru di berkas task), QBE Strict `PASS` (2 berkas), runtime R0–R8 `9/9 PASS` (termasuk dua `POST` bersamaan → `200`/`400`). Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-007.md) |
| 🟡 `RJ-DOC-REV-FE-009` | EWS diastolik dan kategori BMI di Skrining Perawat dan Dokter | 1. Tabel EWS memuat baris "Tekanan Darah Diastolik" bertanda "Tidak diskor"; skor total tidak berubah. 2. Kartu BMI menampilkan "Kategori (Kemenkes): …" sesuai ambang `RJ-DOC-DEC-010`, dibulatkan satu desimal seperti tampilan. 3. Pasien < 18 tahun tidak diberi kategori dewasa | 🟡 `PARTIAL` `2026-10-01` — logika `30/30 PASS` (skrip node, kedua salinan util; batas 18,5/25,0/27,0 teruji), ESLint tanpa warning baru, `next build` `PASS`. Belum: uji klik layar, karena antrean perawat untuk SuperAdmin kosong (difilter cluster) dan akun perawat tidak tersedia. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-009.md) |


## 11. Revisi `2026-10-02` — Daftar Pasien Rawat Jalan (Amendment DP)

### 11.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Pendaftaran pasien lama ditolak oleh ENC-RSMMC-00146 tanpa layar untuk menutup kunjungan (dampak `RJ-DOC-REV-BE-007` §5 risiko 1) |
| **Keputusan** | `RJ-DOC-DEC-011`..`024`, `RJ-DOC-FE-005`..`009` ([00-interview-decisions.md](../00-interview-decisions.md)) |
| **Desain** | Blueprint revisi `28` *Amendment DP*, `approved` (`RJ-DOC-DEC-024`) |
| **Kontrak** | `RJ-DOC-ENCLIST-001@1.0.0` (`approved`) — `contracts/api-contract.md` SHA-256 `BD8AE89DABAE8C8847A2C83964CBAAD7BADDB2D75480C4A5E977474BD21D51DB` |
| **Capability** | [01-capability-impact-scan-daftar-pasien-rj.md](../01-capability-impact-scan-daftar-pasien-rj.md) |
| **Baseline source** | Backend `245f0464`, frontend `b7e9b7fd4` |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` untuk kelima task (`RJ-DOC-DEC-025`, 2 Okt 2026). Tanpa migration, commit, push, merge, deploy |
| **Migration** | Tidak ada di seluruh task bagian ini |
| **Verifikasi** | Pola Bank Darah: tanpa project test; build, QBE Strict, dan runtime HTTP terhadap `QuilvianNewDevSukma` |
| **Governance backend** | Setiap task backend menjalankan QBE preflight dan kesesuaian engineering pada waktu eksekusi, dari `AGENTS.md` backend dan dokumen engineering canonical (`rules/backend/engineering/`) |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

### Grafik Urutan Dependency

**Backend (`MVP-0`):**

```text
RJ-DOC-REV-BE-008 ✅ ─> RJ-DOC-REV-BE-009 ✅ ─> RJ-DOC-REV-BE-010 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-BE-008` | Wewenang task diberikan |
| 2 | `RJ-DOC-REV-BE-009` | `BE-008` ✅ |
| 3 | `RJ-DOC-REV-BE-010` | `BE-009` ✅ |

**Frontend (`MVP-1`):**

```text
[BE] RJ-DOC-REV-BE-009 ✅ ─> RJ-DOC-REV-FE-010 ✅ ─┬─> RJ-DOC-REV-FE-011 ✅
                                                 │
[BE] RJ-DOC-REV-BE-010 ✅ ────────────────────────┘
```

Legenda: `[BE]` = cermin baca-saja task backend dari grafik di atas; bukan task roadmap frontend.

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-FE-010` | `[BE] BE-009` ✅ |
| 2 | `RJ-DOC-REV-FE-011` | `FE-010` ✅ dan `[BE] BE-010` ✅ |

Pasangan prasyarat → task: backend 2, frontend 3 — sama dengan kolom `Dependency` di bawah.
Kontrak sudah `approved` dan terkunci hash-nya, sehingga `FE-010` **boleh** mulai menyusun layar
lebih awal dengan data tiruan; namun statusnya hanya dapat menjadi `✅` setelah diuji terhadap
`BE-009` yang nyata.

### 11.1 Task backend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-008` | **Aturan kunjungan RJ berklinik dan pelonggaran pemblokir pendaftaran.** `OutpatientEncounterRules` (tiga expression: `IsOutpatientClinicEncounter`, `BlocksRegistration`, `IsCancellableStatus`); `PatientEncounterController.FindActiveEncounterAsync` memakai `BlocksRegistration`. `KunjunganMasihBerjalan` tidak disentuh | — | `AT-DP-15`, `AT-DP-16`, `AT-DP-17`, `AT-DP-18`, `AT-DP-19` | Build Release `0 Error`; QBE Strict `PASS` pada berkas yang disentuh; runtime: pasien dengan kunjungan status 7 / lab walk-in / IGD → `POST /patient-encounters/admin` `200`; status 5 → `400` pesan `RJ-DOC-REV-BE-007` | ✅ `COMPLETE` `2026-10-02` — build Release `0 Error` (244 warning, sama dengan baseline), QBE Strict `PASS` (3 berkas), runtime R0–R7 `8/8 PASS` (AT-DP-15/16/17/19); AT-DP-18 lewat diff; bagian IGD AT-DP-17 lewat aturan, tidak diuji runtime. Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-008.md) |
| ✅ `RJ-DOC-REV-BE-009` | **Daftar, summary, metadata bercakupan dan butir hak akses.** `ClinicalActorScopeService`; `OutpatientEncounterExplicitPermissions` (`ReadAll`); `OutpatientEncounterListService` (list, summary, metadata); `OutpatientEncounterController` tiga `GET`; DTO; registrasi DI | `BE-008` | `AT-DP-01`..`06`, `AT-DP-20`, `AT-DP-21`; butir `OutpatientEncounter : Read/ReadAll` muncul di layar Akses Role | Build, QBE Strict, runtime dengan empat akun: dokter, perawat ber-cluster, pendaftaran ber-`ReadAll`, akun tanpa cakupan (`403`) | ✅ `COMPLETE` `2026-10-02` — build Release `0 Error` (244 warning, sama dengan baseline), QBE Strict `PASS` (7 berkas), runtime SuperAdmin `17/17 PASS` + akun uji dokter/perawat/tanpa cakupan/pendaftaran `16/16 PASS` (AT-DP-01..06, 20, 21). Tanpa migration. Hak akses tiga jabatan digabung untuk uji (lihat laporan §4). [Laporan](../task/report/backend/RJ-DOC-REV-BE-009.md) |
| ✅ `RJ-DOC-REV-BE-010` | **Pembatalan kunjungan.** `PATCH /outpatient-encounters/{id}/cancel`: kunci baris, periksa ulang aturan + konsultasi aktif, isi kolom batal, batalkan antrean, notifikasi setelah commit, log tanpa data medis. Butir `OutpatientEncounter : Cancel` | `BE-009` | `AT-DP-07`..`14`; pesan `RJDP-VAL-002`..`006` persis seperti `validation-matrix` | Build, QBE Strict, runtime termasuk dua `PATCH` paralel (`200`/`400`); data uji dibersihkan lewat endpoint aplikasi | ✅ `COMPLETE` `2026-10-02` — build Release `0 Error` (244 warning, sama dengan baseline), QBE Strict `PASS` (3 berkas), runtime AT-DP-07..14 seluruhnya `PASS` (termasuk dua `PATCH` paralel `200`/`400`; AT-DP-09 pada kunjungan nyata dengan penjaga baca-dulu, data tidak berubah). Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-010.md) |

### 11.2 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-010` | **Layar Daftar Pasien Rawat Jalan (baca) dan butir menu.** Route `/health-services/registration-management/outpatient-encounters`; Hero dengan label cakupan; SummaryGrid lima kartu (klik Menggantung = mode aktif + `hangingOnly`); DataFilter; DataTable + pagination; keadaan memuat/kosong/gagal/`403`; butir menu di bawah Skrining Pasien dengan `requiredPermission` `OutpatientEncounter : Read` | `[BE] BE-009` | `AT-DP-23`; skema `DP-FE.3` keempat keadaan; default mode Hari ini; filter dokter hanya untuk `scope.canReadAll` | Lint tanpa error baru; `next build` `PASS`; runtime terhadap `BE-009` dengan akun dokter, perawat, pendaftaran | ✅ `COMPLETE` `2026-10-02` — ESLint `0 error, 0 warning`, `next build` `PASS`, UI GATE REUSE 12/12; uji layar Playwright (login lewat layar) SuperAdmin/dokter/perawat/tanpa cakupan/tanpa hak: menu, cakupan, tab bawaan, kartu Menggantung, rentang, keadaan memuat/kosong/gagal/berisi/403 seluruhnya `PASS` (satu deteksi filter Dokter dipastikan lewat tangkapan layar). [Laporan](../task/report/frontend/RJ-DOC-REV-FE-010.md) |
| ✅ `RJ-DOC-REV-FE-011` | **Pembatalan dari layar.** Aksi baris Batalkan bersyarat (`item.canCancel` dan `usePermission("OutpatientEncounter", "Cancel")`); keterangan `cancelBlockedReason`; modal alasan 1-250; tombol nonaktif saat mengirim; pesan server di modal; toast dan muat ulang tabel + summary | `FE-010`, `[BE] BE-010` | `AT-DP-22`, `AT-DP-24` (kasus ENC-RSMMC-00146 atau padanannya pada data uji) | Lint, `next build`, runtime batal + daftar ulang pasien | ✅ `COMPLETE` `2026-10-02` — ESLint `0 error, 0 warning`, `npm run build` `PASS`, UI GATE REUSE 6/6; uji layar Playwright `12/12 PASS` (AT-DP-22 dokter tanpa Cancel + keterangan konsultasi aktif; AT-DP-24 batal lewat layar lalu daftar ulang pada data uji; data basi tampil di modal). ENC-RSMMC-00146 nyata tidak disentuh. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-011.md) |

Butir menu menjadi AC `FE-010` agar tidak menganggur di antara task.

### 11.3 Traceability

| Requirement / keputusan | Task | Uji |
| --- | --- | --- |
| `RJ-DOC-DEC-012` hanya RJ | `BE-008`, `BE-009` | `AT-DP-06` |
| `RJ-DOC-DEC-013` cakupan di server | `BE-009` | `AT-DP-01`..`04` |
| `RJ-DOC-DEC-014` `ReadAll` tanpa nama role | `BE-009` | `AT-DP-05` |
| `RJ-DOC-DEC-015` hak batal | `BE-010`, `FE-011` | `AT-DP-12`, `AT-DP-22` |
| `RJ-DOC-DEC-016`/`021` batas status batal | `BE-008`, `BE-010` | `AT-DP-07`..`10` |
| `RJ-DOC-DEC-017`/`023` endpoint lama tetap | `BE-008` | `AT-DP-19` |
| `RJ-DOC-DEC-018` alasan | `BE-010`, `FE-011` | `AT-DP-11` |
| `RJ-DOC-DEC-019`/`022` pemblokir | `BE-008` | `AT-DP-15`..`18` |
| `RJ-DOC-DEC-020` status 6 lewat dokter | `BE-010`, `FE-011` | `AT-DP-09`, `AT-DP-22` |
| `RJ-DOC-FE-005` menu | `FE-010` | `AT-DP-23` |
| `RJ-DOC-FE-006`/`007` kerangka & default | `FE-010` | `AT-DP-21`, skema `DP-FE.3` |
| `RJ-DOC-FE-008` modal batal | `FE-011` | `AT-DP-22`, `AT-DP-24` |
| AC 13 pemblokir dapat ditemukan | `BE-009` | `AT-DP-20` |
| Konkurensi batal | `BE-010` | `AT-DP-14` |

**Coverage gap:**

| Gap | Sebab | Penanganan |
| --- | --- | --- |
| `R-DP-1` — panggilan dokter bersamaan dengan pembatalan | Risiko sisa yang sengaja tidak ditutup (`02` DP.14) | Tidak diuji; dicatat untuk task antrean berikutnya |
| `RJ-DOC-FE-009` kolom/gaya | `DEV_DISCRETION` | Tidak diuji sebagai keputusan produk |
| Uji klik layar FE | Pengalaman `RJ-DOC-REV-FE-009`: akun perawat ber-cluster tidak tersedia | Siapkan akun dokter dan perawat ber-cluster di `QuilvianNewDevSukma` sebelum `FE-010`; bila tidak ada, laporan task wajib menyatakan `NOT FEASIBLE` |

### 11.4 Definition of Done bagian ini

Sama dengan `04-prd-to-mvp.md` *Amendment DP* DP-7: kelima task `✅`, `AT-DP-01`..`24` lulus, dan
ENC-RSMMC-00146 (atau padanannya pada data uji) dapat ditutup lalu pasiennya didaftarkan ulang.

## 12. Revisi `2026-10-02` — penangguhan sementara pemblokir pendaftaran

| Field | Isi |
| --- | --- |
| **Keputusan** | `RJ-DOC-DEC-026` (penangguhan), `RJ-DOC-DEC-027` (wewenang) |
| **Kontrak** | `RJ-DOC-ENCLIST-001@1.0.0` — bagian *Perubahan perilaku endpoint yang sudah ada* ditangguhkan selama saklar mati; bentuk request/response tidak berubah |
| **Baseline source** | Backend `245f0464` + perubahan Amendment DP yang belum di-commit |
| **Migration** | Tidak ada |

### Grafik Urutan Dependency

```text
RJ-DOC-REV-BE-011 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-BE-011` | Wewenang diberikan (`RJ-DOC-DEC-027`) |

### 12.1 Task

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-011` | **Saklar pemblokir kunjungan aktif.** Konfigurasi `HealthServices:Registration:BlockActiveEncounter` (bawaan `false`). Bila `false`: validasi awal dan cek ulang dalam transaksi create dilewati, termasuk advisory lock-nya. Bila `true`: perilaku `RJ-DOC-REV-BE-008` utuh | — | 1. Saklar mati: pasien dengan kunjungan RJ berklinik status 0–6 dapat didaftarkan (`200`). 2. Saklar hidup: ditolak `400` dengan pesan `RJ-DOC-REV-BE-007`. 3. Daftar Pasien Rawat Jalan dan pembatalan tidak berubah | Build Release, QBE Strict, runtime kedua nilai saklar | ✅ `COMPLETE` `2026-10-02` — build Release `0 Error` (244 warning, sama dengan baseline), QBE Strict `PASS`, runtime saklar mati + hidup `7/7 PASS`. Bawaan `false` (aturan ditangguhkan). Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-011.md) |


## 13. Revisi `2026-10-05` — Konsultasi Tertunda di Klinis Dokter (Amendment KT)

### 13.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Saat `BlockActiveEncounter = true`, IKBAL YULIYANTO ditolak mendaftar karena ENC-RSMMC-00172 (30 Sep 2026) masih Sedang Konsultasi. Klinis Dokter hanya memuat antrean hari ini, sehingga dr. Arif Lesmana tidak dapat membuka konsultasi itu |
| **Keputusan** | `RJ-DOC-DEC-028`..`032`, `RJ-DOC-FE-010`..`012` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment Pass 2026-10-05*) |
| **Desain** | Blueprint revisi `29` *Amendment KT*, `approved` (`RJ-DOC-DEC-032`) |
| **Kontrak** | `RJ-DOC-PENDCONS-001@1.0.0` (`approved`) — `contracts/api-contract.md` SHA-256 `20E2FCE45B8869889181A0EF1B1AD5DCF1B6CCE49FCB25A66F7EAC02B8AB8E38` |
| **Capability** | Fakta `F-KT-1`..`5` pada decision log; `02-backend-architecture.md` KT.2–KT.3 |
| **Baseline source** | Backend `bb46ccc8`, frontend `d232feb2b` |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` untuk kedua task (`RJ-DOC-DEC-033`, 5 Okt 2026). Tanpa migration, commit, push, merge, deploy |
| **Migration** | Tidak ada di seluruh task bagian ini |
| **Verifikasi** | Pola Bank Darah: tanpa project test; build, QBE Strict, dan runtime HTTP terhadap `QuilvianNewDevSukma` |
| **Governance backend** | Task backend menjalankan QBE preflight dan kesesuaian engineering pada waktu eksekusi, dari `AGENTS.md` backend dan dokumen engineering canonical (`rules/backend/engineering/`) |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

### Grafik Urutan Dependency

**Backend (`MVP-0`):**

```text
RJ-DOC-REV-BE-012 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-BE-012` | Wewenang task diberikan |

**Frontend (`MVP-1`):**

```text
[BE] RJ-DOC-REV-BE-012 ✅ ─> RJ-DOC-REV-FE-012 ✅
```

Legenda: `[BE]` = cermin baca-saja task backend dari grafik di atas; bukan task roadmap frontend.

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-FE-012` | `[BE] BE-012` ✅ |

Pasangan prasyarat → task: backend 0, frontend 1 — sama dengan kolom `Dependency` di bawah.
Kontrak sudah `approved` dan terkunci hash-nya, sehingga `FE-012` **boleh** mulai menyusun tampilan
dengan data tiruan; statusnya hanya dapat menjadi `✅` setelah diuji terhadap `BE-012` yang nyata.

### 13.1 Task backend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-012` | **Endpoint konsultasi tertunda dan bunyi petunjuk.** (1) `GET /doctor-queues/pending-consultations` di `DoctorQueueController` dengan syarat `02` KT.3.1, cakupan `ResolveAllowedDoctorIdAsync` (KT.3.2), parameter `doctorId`, `queueId`, `search`, `pageNumber`, `pageSize`, urut `QueueDate` menaik; `[AccessPermission("DoctorQueue", "Read")]`. (2) `DoctorPendingConsultationResponse : DoctorQueueResponse` dengan `draftPrescriptionCount`, `procedureCount`, `pendingDays`, `canCancelConsultation` (KT.3.3). (3) Bunyi baru `RJDP-VAL-005` di `OutpatientEncounterListService.GetCancelBlockedReason` (KT.3.4). (4) Hitungan baca-saja `RJ-DOC-OQ-012` di DB uji, dilaporkan. (5) Periksa apakah jabatan dokter uji memegang `DoctorConsultation : Cancel` (KT.10), dilaporkan | — | `AT-KT-01`..`10`; endpoint lain `DoctorQueueController` tidak berubah (`AT-KT-09`); response persis seperti `contracts/api-contract.md` *Amendment KT* | Build Release `0 Error` tanpa warning baru; QBE Strict `PASS` pada berkas yang disentuh; runtime HTTP dengan akun dokter pemilik antrean, dokter lain, dan akun tanpa data dokter; data uji dibuat dan dibersihkan lewat endpoint aplikasi | ✅ `COMPLETE` `2026-10-05` — build Release `0 Error` (244 warning, sama dengan baseline), QBE Strict `PASS` (3 berkas), EF tanpa perubahan model, runtime AT-KT-01..10 seluruhnya `PASS` (termasuk perbandingan endpoint lama 5/5 identik dengan server lama). `RJ-DOC-OQ-012` = 0; jabatan dokter uji memegang `DoctorConsultation : Cancel`. Delta: `doctorId` dokter lain → `403` (sama dengan `GET /doctor-queues`). Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-012.md) |

### 13.2 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-012` | **Konsultasi tertunda di Klinis Dokter.** (1) `getDoctorPendingConsultations` di `doctor-queue.service.js`. (2) State, muat, muat ulang (termasuk event realtime yang sama) di `use-doctor-queue.js`. (3) Wilayah (B) di panel kiri: judul + jumlah, kartu dengan tombol Buka saja, tidak tampil bila kosong, pesan gagal + Coba lagi. (4) `activeItem` dari gabungan antrean hari ini dan daftar tertunda (`useDoctorConsultationWorkspace.js`). (5) Banner (A) untuk item lampau. (6) Modal Simpan (D): baca ulang hitungan lewat `queueId`, centang wajib bila ada resep draf/tindakan. (7) Tombol (C) dan modal (E) Batalkan konsultasi lewat `cancelDoctorConsultation`, tampil bila `canCancelConsultation`, alasan 1–250. (8) Sesudah Simpan/Batalkan: muat ulang daftar tertunda dan ringkasan | `[BE] BE-012` | `AT-KT-11` (`UAT-KT-01`..`07`); skema `03` KT-FE.3 seluruh wilayah dan keadaan KT-FE.5; antrean hari ini berperilaku sama | Lint tanpa error baru; `npm run build` `PASS`; uji layar dengan akun dokter pemilik konsultasi tertunda dan dokter lain, data uji pada `QuilvianNewDevSukma` | ✅ `COMPLETE` `2026-10-05` — ESLint `0 error, 0 warning`, `npm run build` `PASS`, UI GATE REUSE 3 / EXTEND 3 / COMPOSE 1 / NEW 0; uji layar Playwright `22/22 PASS` (UAT-KT-01, 02, 03, 05, 07). Dikecualikan: UAT-KT-04 dan 06 di layar `NOT FEASIBLE` (tidak ada akun dokter kedua; terbukti di backend), muat ulang realtime tidak diuji langsung. Branch `sukmagpV2`, belum di-commit. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-012.md) |

Bentuk kartu, tab atau bagian terpisah, ikon, dan warna tetap `DEV_DISCRETION` dengan base component
Quilvian (`RJ-DOC-FE-006`, `RJ-DOC-FE-010`).

### 13.3 Traceability

| Requirement / keputusan | Task | Uji |
| --- | --- | --- |
| `RJ-DOC-DEC-029` daftar tertunda lintas tanggal per dokter | `BE-012`, `FE-012` | `AT-KT-01`, `AT-KT-02`, `AT-KT-04`, `UAT-KT-01`, `UAT-KT-04` |
| `RJ-DOC-DEC-030` hanya Sedang Konsultasi dengan konsultasi aktif | `BE-012` | `AT-KT-03` |
| `RJ-DOC-DEC-031` Simpan dan Batalkan seperti hari ini | `BE-012` (hitungan, penanda), `FE-012` | `AT-KT-06`..`08`, `UAT-KT-02`, `UAT-KT-03`, `UAT-KT-05`, `UAT-KT-06` |
| `RJ-DOC-FE-010` letak dan jumlah | `FE-012` | `UAT-KT-01`, `UAT-KT-07` |
| `RJ-DOC-FE-011` banner dan konfirmasi | `FE-012` | `UAT-KT-02` |
| `RJ-DOC-FE-012` / `RJDP-VAL-005` bunyi petunjuk | `BE-012` | `AT-KT-10` |
| Cakupan dan `403` | `BE-012` | `AT-KT-04`, `AT-KT-05` |
| Kompatibilitas antrean hari ini | `BE-012`, `FE-012` | `AT-KT-09`, `AT-KT-11` |
| `RJ-DOC-OQ-012` hitungan data | `BE-012` | Laporan task (bukan uji lulus/gagal) |

**Coverage gap:**

| Gap | Sebab | Penanganan |
| --- | --- | --- |
| `RJ-DOC-OQ-014` batal konsultasi untuk antrean hari ini | `POST-MVP` | Tidak diuji |
| `RJ-DOC-OQ-015` resep draf saat konsultasi dibatalkan | Perilaku lama, di luar scope | Tidak diuji |
| Akun dokter kedua untuk `AT-KT-04`/`UAT-KT-04` | Bergantung data `QuilvianNewDevSukma` | Bila tidak tersedia, laporan task wajib menyatakan `NOT FEASIBLE` untuk butir itu |
| Uji Selesaikan pada ENC-RSMMC-00172 nyata | Data klinis nyata pasien | **Tidak** dipakai untuk uji otomatis; uji memakai data uji. Penyelesaian ENC-RSMMC-00172 dilakukan dr. Arif sendiri setelah fitur jadi |

### 13.4 Definition of Done bagian ini

Sama dengan `04-prd-to-mvp.md` *Amendment KT* KT-7: kedua task `✅`, `AT-KT-01`..`11` lulus atau
dinyatakan `NOT FEASIBLE` beserta sebabnya, dan hitungan `RJ-DOC-OQ-012` dilaporkan.

## 14. Revisi `2026-10-06` — Pendaftaran Pasien Rawat Jalan oleh petugas (Amendment PR)

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Sukma Giri, 6 Okt 2026: fitur pendaftaran pasien Rawat Jalan dengan tampilan sama seperti Pendaftaran Pasien IGD |
| **Keputusan** | `RJ-DOC-DEC-034`..`039` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment PR*) |
| **Kontrak** | Endpoint yang sudah ada, tanpa perubahan: `POST /patient-encounters/admin` (`PatientEncounterCreateRequest`), `GET /clinics/admin/options`, `GET /doctor-schedules/admin` (daftar admin; `admin/options` tidak membawa masa berlaku jadwal — lihat laporan task), serta endpoint pasien dan penjamin yang sudah dipakai Pendaftaran IGD |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` untuk `RJ-DOC-REV-FE-013` (`RJ-DOC-DEC-038`). Tanpa backend, migration, commit, push, merge, deploy |
| **Migration** | Tidak ada |

### Grafik Urutan Dependency

```text
RJ-DOC-REV-FE-013 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-FE-013` | Wewenang diberikan (`RJ-DOC-DEC-038`) |

### 14.1 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-013` | **Layar Pendaftaran Pasien Rawat Jalan.** Route `/health-services/registration-management/outpatient-registration`; butir menu *Pendaftaran Pasien* paling atas pada menu Rawat Jalan dengan `requiredPermission` `PatientEncounter : Create`. Alur dan tampilan sama dengan Pendaftaran Pasien IGD; langkah 2 *Data Kunjungan*: tanggal kunjungan (hari ini atau mendatang), poliklinik, jadwal dokter pada tanggal itu, jenis kunjungan (`RJ-DOC-DEC-036`), keluhan utama. Submit ke `POST /patient-encounters/admin` dengan `encounterType` Outpatient | — | 1. Pasien lama dan pasien baru dapat didaftarkan; encounter Outpatient berklinik terbentuk dengan nomor antrean bila klinik memakai antrean. 2. Jadwal dokter wajib hanya bila klinik `IsDoctorRequired` (`RJ-DOC-DEC-035`). 3. Hari ini terkirim sebagai walk-in; tanggal mendatang sebagai appointment (`RJ-DOC-DEC-037`). 4. Tunai, asuransi, dan penjamin perusahaan berjalan seperti IGD. 5. Penolakan backend tampil sebagai pesan di langkah Verifikasi. 6. Layar Pendaftaran IGD tidak berubah | Lint tanpa error baru; `npm run build` `PASS`; uji layar terhadap backend lokal dan `QuilvianNewDevSukma` | ✅ `COMPLETE` `2026-10-06` — ESLint `0 error, 0 warning`, `npm run build` `PASS`, UI GATE REUSE 4 / EXTEND 3 / COMPOSE 2 / NEW 0; uji layar Playwright `28/28 PASS` (pasien lama/baru, walk-in/appointment, tunai/asuransi/perusahaan, penolakan backend, IGD tidak berubah). AC 2 cabang dokter opsional hanya terbukti lewat kode (seluruh poliklinik DB uji `IsDoctorRequired`). Delta: jadwal dari `GET /doctor-schedules/admin`. Data uji dibatalkan; pasien `00-00-00-17` tersisa. Revisi `RJ-DOC-DEC-039` (6 Okt 2026): poliklinik hanya yang buka pada tanggal kunjungan, tanpa kode; tombol RJ tanpa ikon; tombol kembali bergaris primary — ESLint `0 error, 0 warning`, `npm run build` `PASS`, uji layar `20/20 PASS`, IGD tidak berubah. Revisi Input Manual menggulir ke form pasien baru (RJ saja): build `PASS`, uji layar `3/3 PASS`. Branch `sukmagpV2`, belum di-commit. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-013.md) |

## 15. Revisi `2026-10-06` — Scan kartu penjamin pada Pendaftaran Rawat Jalan (Amendment SK)

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Sukma Giri, 6 Okt 2026: tombol scan kartu polis/asuransi pada tabel penjamin, foto disimpan seperti scan kiosk, field scan pada modal *Daftarkan Penjamin Baru* |
| **Keputusan** | `RJ-DOC-DEC-040`..`043` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment SK*) |
| **Kontrak** | Delta pada endpoint penjamin pasien yang sudah ada: `cardImageBase64` opsional pada `POST` create, endpoint baru `PATCH /{id}/card-image`, dan `cardImagePath` pada response. Rincian per task di bawah |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` dalam `CROSS-REPO MODE` (`RJ-DOC-DEC-043`). Tanpa commit, push, merge, deploy |
| **Migration** | `BE-013` tidak ada. `BE-014` menambah satu kolom nullable; pembuatan dan eksekusinya butuh izin terpisah |
| **Verifikasi** | Pola Bank Darah: tanpa project test; build, QBE, EF, dan runtime HTTP terhadap `QuilvianNewDevSukma` |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

### Grafik Urutan Dependency

```text
RJ-DOC-REV-BE-013 ✅ ─> RJ-DOC-REV-BE-014 ✅ ─> RJ-DOC-REV-FE-014 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-BE-013` | Wewenang diberikan (`RJ-DOC-DEC-043`) |
| 2 | `RJ-DOC-REV-BE-014` | `BE-013` selesai (memakai service penyimpan kartu yang sama) dan izin migration |
| 3 | `RJ-DOC-REV-FE-014` | `BE-013` dan `BE-014` selesai |

### 15.1 Task backend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-013` | **Simpan foto kartu asuransi pasien.** (1) Service `PatientPayerCardImageService` menulis base64 gambar kartu ke storage `FileStorage` (`/uploads/patient-payer-cards/<patientId>/...`) dengan validasi ukuran dan format gambar. (2) `CreatePatientInsuranceRequest.CardImageBase64` opsional; bila diisi, `CardImagePath` diisi path file tersimpan. (3) `PATCH /patient-insurances/{id}/card-image` dan `admin/{id}/card-image` dengan `[AccessPermission("PatientInsurance", "Update")]` | — | 1. Create dengan `cardImageBase64` menghasilkan file dan `cardImagePath` publik. 2. `PATCH card-image` mengganti `cardImagePath` asuransi yang ada. 3. Base64 rusak, bukan gambar, atau melebihi batas ditolak `400`. 4. Id tidak ada → `404`. 5. Create tanpa gambar dan endpoint lain tidak berubah | Build tanpa error baru; EF tanpa perubahan model; runtime HTTP terhadap `QuilvianNewDevSukma` | ✅ `COMPLETE` `2026-10-06` — build Release `0 Error` (239 warning, nol pada berkas disentuh), EF tanpa perubahan model, QBE Strict `PASS` (9 berkas), runtime `14/14 PASS` (R0–R13). Uji memakai harness seeder sementara karena DB uji tertinggal 19 migration modul lain; `Program.cs` dipulihkan. Tanpa migration. [Laporan](../task/report/backend/RJ-DOC-REV-BE-013.md) |
| ✅ `RJ-DOC-REV-BE-014` | **Simpan foto kartu penjamin perusahaan pasien.** Kolom `MstPatientCompanyGuarantor.CardImagePath varchar(500) null` beserta configuration dan migration; `CardImageBase64` opsional pada create; `PATCH /patient-company-guarantors/{id}/card-image` dan `admin/{id}/card-image` dengan `[AccessPermission("PatientCompanyGuarantor", "Update")]`; `cardImagePath` pada response list/detail | `BE-013` | Sama dengan `BE-013` AC 1–5 untuk penjamin perusahaan; migration hanya menambah kolom tersebut | Build; migration ter-generate hanya kolom itu; runtime HTTP | ✅ `COMPLETE` `2026-10-06` — build Release `0 Error`, migration `20261006092040_AddCardImagePathToPatientCompanyGuarantor` hanya kolom itu, diterapkan ke `QuilvianNewDevSukma` lewat script idempoten satu migration (`RJ-DOC-DEC-044`; 19 migration lain tetap pending), EF tanpa perubahan model, QBE Strict `PASS`, runtime `7/7 PASS` (C1–C7). Delta: `PUT` tanpa `cardImagePath` mempertahankan kartu. [Laporan](../task/report/backend/RJ-DOC-REV-BE-014.md) |

### 15.2 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-014` | **Scan kartu penjamin di Pendaftaran Rawat Jalan.** Kolom *Kartu* sebelum Status pada tabel penjamin: *Scan Kartu* (belum ada gambar) atau *Lihat Kartu* (sudah ada). Hasil scan Plustek langsung disimpan lewat `PATCH card-image`. Preview kartu tersimpan pada panel *Penjamin Dipilih*. Field scan kartu pada modal *Daftarkan Penjamin Baru*, terkirim sebagai `cardImageBase64`. Menggantikan perubahan sementara tombol scan di header tabel | `BE-013`, `BE-014` | 1. Penjamin tanpa kartu menampilkan *Scan Kartu*; setelah scan berhasil, tombol berubah menjadi *Lihat Kartu* tanpa muat ulang. 2. Penjamin yang kartunya sudah tersimpan langsung menampilkan *Lihat Kartu* dan preview setelah halaman dibuka ulang. 3. Penjamin baru dengan kartu hasil scan tersimpan bersama gambar kartunya. 4. Kegagalan scanner atau backend tampil sebagai pesan. 5. Pendaftaran IGD tidak berubah | ESLint tanpa error baru; `npm run build` `PASS`; uji layar terhadap backend lokal | ✅ `COMPLETE` `2026-10-06` — ESLint `0 error` (4 warning lama di kode yang tidak diubah), `npm run build` `PASS`, unit test modul pendaftaran `46/46 PASS` (suite penuh 8 gagal di modul lain), UI GATE REUSE 4 / COMPOSE 2 / WRAP 1 / NEW 0, uji layar Playwright `15/15 PASS` terhadap backend `BE-013`/`BE-014` dengan agent Plustek **tiruan**. AC 5 (IGD tidak berubah) terbukti lewat kode. Belum diuji dengan scanner fisik. Perubahan sementara tombol header dibuang. Branch `sukmagpV2`, belum di-commit. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-014.md) |

## 16. Revisi `2026-10-07` — Menu Konsultasi Tertunda (Amendment MT)

### 16.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Sukma Giri, 7 Okt 2026: kunjungan tertunda dipindahkan dari Klinis Dokter ke menu baru di bawah Dokter → Rawat Jalan. Aksi baris *Batalkan Konsultasi* dan *Simpan Konsultasi*. Simpan mengarahkan ke Klinis Dokter untuk meninjau ulang hasil terakhir konsultasi. Konsepnya sama dengan *Batalkan Kunjungan* di Daftar Pasien Rawat Jalan |
| **Keputusan** | `RJ-DOC-DEC-045`..`050`, `RJ-DOC-FE-014`..`016` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment Pass 2026-10-07*). `RJ-DOC-FE-010` dan `RJ-DOC-FE-013` `superseded` |
| **Desain** | Blueprint revisi `30` *Amendment MT*, `approved` (`RJ-DOC-DEC-050`). `03-frontend-architecture.md` SHA-256 `0E131F65A07D71350736E7D66DCF21E362EC1A3AEBEAE4BBFD7604A2DF737272` |
| **Kontrak** | `RJ-DOC-PENDCONS-001@1.0.0` (`approved`), **tidak berubah** — `contracts/api-contract.md` SHA-256 `20E2FCE45B8869889181A0EF1B1AD5DCF1B6CCE49FCB25A66F7EAC02B8AB8E38` |
| **Capability** | Fakta `F-MT-1`..`5` pada decision log; `02-backend-architecture.md` *Amendment MT* (tanpa perubahan backend) |
| **Baseline source** | Backend `85962dc4` (`sukmagp`), frontend `9acc42027` (`sukmagpV2`) |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` untuk `RJ-DOC-REV-FE-015` (`RJ-DOC-DEC-051`, 7 Okt 2026). Tanpa backend, migration, commit, push, merge, deploy |
| **Backend / migration** | Tidak ada task backend dan tidak ada migration |
| **Verifikasi** | Lint, `npm run build`, dan uji layar Playwright terhadap FE dev + backend dev dengan `QuilvianNewDevSukma` (pola uji layar `RJ-DOC-REV-FE-012`) |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

### Grafik Urutan Dependency

**Frontend (`MVP-0`):**

```text
[BE] RJ-DOC-REV-BE-012 ✅ ─> RJ-DOC-REV-FE-015 ✅
```

Legenda: `[BE]` = cermin baca-saja task backend bagian 13 (endpoint `pending-consultations` yang
dipakai ulang); bukan task bagian ini. Bagian ini tidak punya task backend.

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-FE-015` | `[BE] BE-012` ✅ dan wewenang `RJ-DOC-DEC-051` ✅ |

Pasangan prasyarat → task: frontend 1 — sama dengan kolom `Dependency` di bawah.

### 16.1 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-015` | **Menu dan halaman Konsultasi Tertunda.** (1) `menu-items.jsx`: Dokter → Rawat Jalan menjadi grup *Klinis Dokter* (rute lama) dan *Konsultasi Tertunda* (rute baru sejajar, `03` MT-FE.2). (2) Halaman baru `doctor-pending-consultations` berpola `outpatient-encounters/*`: `Hero`, `DataFilter` (pencarian, jumlah baris, reset), `DataTable` dengan kolom MT-FE.3, `Pagination`, `AccessDeniedGate`, keadaan kosong/gagal. (3) `RowActionMenu`: *Simpan Konsultasi* → Klinis Dokter `?queueId=`; *Batalkan Konsultasi* (bila `canCancelConsultation`) → `ConfirmModal` alasan wajib 1–250 lewat `cancelDoctorConsultation`. (4) `useDoctorPendingConsultations`: dukung `search`, pagination, dan mode hitung `pageSize=1`. (5) Klinis Dokter: hapus `ClinicalTabNav` dan daftar tertunda di panel kiri; pengingat "Ada {n} konsultasi tertunda" bila `n > 0`; muat satu item dari `?queueId=` lewat `pending-consultations?queueId=`, buka lewat jalur `handleStart` existing tanpa modal Simpan, bersihkan parameter URL; pesan "tidak ditemukan" dengan tautan kembali. (6) Tombol Batalkan Konsultasi di workspace untuk item dari parameter URL. (7) Sesudah Simpan/Batalkan berhasil pada item itu: arahkan ke daftar dengan pesan sukses; Simpan antrean hari ini tetap di Klinis Dokter. (8) Bersihkan cabang `pendingMode` `QueuePatientCard` bila tak terpakai | `[BE] BE-012` | `AT-MT-01`..`12` (`UAT-MT-01`..`12`); skema `03` MT-FE.3, MT-FE.4, keadaan MT-FE.6; antrean hari ini berperilaku sama; tidak ada perubahan backend | ESLint pada berkas tersentuh tanpa error baru; `npm run build` `PASS`; uji layar Playwright dengan akun dokter pemilik konsultasi tertunda; data uji pada `QuilvianNewDevSukma` dibuat/dibersihkan lewat endpoint aplikasi; laporan task dengan keputusan UI GATE (reuse/extend/new) | ✅ `COMPLETE` `2026-10-07` — ESLint `0 error, 0 warning` (14 berkas), `npm run build` `PASS`, UI GATE REUSE 9 / COMPOSE 2 / NEW 0; uji layar Playwright `32/32 PASS` (save 19, ws-cancel 7, list-cancel 6) terhadap backend uji 7185 dan `QuilvianNewDevSukma`. `npm run test:unit` 8 gagal yang sama dengan baseline `HEAD`. Dikecualikan: `UAT-MT-08` dan `UAT-MT-11` di layar `NOT FEASIBLE` (dibuktikan lewat kode dan `BE-012`); muat ulang realtime tidak diuji. Delta: parameter URL `pendingQueueId`. **Revisi 1 `2026-10-07`** (`RJ-DOC-DEC-053`): pengingat dan daftar mengirim `doctorId` sesi sehingga akun dokter yang juga SuperAdmin hanya melihat miliknya — ESLint `0/0`, build `PASS`, uji layar `8/8 PASS`. **Revisi 2 `2026-10-07`** (`RJ-DOC-DEC-054`): antrean hari ini, Total Antrean, kunci panggil, dan grup realtime memakai `doctorId` sesi — ESLint `0 error` (1 warning lama `HEAD`), build `PASS`, uji layar `12/12 PASS`. Branch `sukmagpV2`, belum di-commit. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-015.md) |

Ikon, letak pengingat, nama route/key/parameter URL, dan bunyi pesan tetap `DEV_DISCRETION` dengan
base component Quilvian (`RJ-DOC-FE-006`, `RJ-DOC-FE-016`).

### 16.2 Traceability

| Requirement / keputusan | Task | Uji |
| --- | --- | --- |
| `RJ-DOC-FE-014` menu grup dan Klinis Dokter tanpa tab | `FE-015` | `AT-MT-01`, `AT-MT-02`, `UAT-MT-01` |
| `RJ-DOC-FE-015` pengingat jumlah tertunda | `FE-015` | `AT-MT-03`, `UAT-MT-02` |
| `RJ-DOC-FE-016` isi halaman daftar | `FE-015` | `AT-MT-04`, `UAT-MT-03`, `UAT-MT-11` |
| `RJ-DOC-DEC-046` Simpan = tinjau di Klinis Dokter | `FE-015` | `AT-MT-07`, `AT-MT-10`, `UAT-MT-05`, `UAT-MT-09`, `UAT-MT-12` |
| `RJ-DOC-DEC-047` Batalkan di daftar dan workspace | `FE-015` | `AT-MT-05`, `AT-MT-06`, `AT-MT-09`, `UAT-MT-04`, `UAT-MT-06`..`08` |
| `RJ-DOC-DEC-048` kembali ke daftar sesudah sukses | `FE-015` | `AT-MT-08`, `AT-MT-09`, `AT-MT-11`, `UAT-MT-05`, `UAT-MT-10` |
| `RJ-DOC-DEC-049` batal = pembatalan konsultasi existing | `FE-015` | `AT-MT-05` |
| Build dan lint | `FE-015` | `AT-MT-12` |

**Coverage gap:**

| Gap | Sebab | Penanganan |
| --- | --- | --- |
| `RJ-DOC-OQ-016` filter Dokter | `POST-MVP` | Tidak diuji |
| `UAT-MT-11` dokter kedua | Tidak ada akun dokter kedua di DB uji (`RJ-DOC-REV-FE-012`) | Cakupan sudah terbukti di backend (`AT-KT-04`); laporan task boleh menyatakan `NOT FEASIBLE` di layar |
| `UAT-MT-08` tanpa izin batal | Akun uji memegang `DoctorConsultation : Cancel` dan SuperAdmin | Bila role tidak dapat dicopot sementara dengan izin pemilik, nyatakan `NOT FEASIBLE` di layar dan buktikan lewat kode (`canCancelConsultation`) |

### 16.3 Definition of Done bagian ini

Sama dengan `04-prd-to-mvp.md` *Amendment MT* MT-7: `RJ-DOC-REV-FE-015` `✅`, `AT-MT-01`..`12` lulus
atau dinyatakan `NOT FEASIBLE` beserta sebabnya, dan backend tidak berubah.

## 17. Revisi `2026-10-08` — Antrean prioritas, member, dan privasi layar publik (Amendment AQ)

### 17.0 Dasar dan wewenang

| Field | Isi |
| --- | --- |
| **Sumber requirement** | Sukma Giri, 8 Okt 2026, spesifikasi "Antrean Rawat Jalan MMC" |
| **Keputusan** | `RJ-DOC-DEC-055`..`061` ([00-interview-decisions.md](../00-interview-decisions.md), *Amendment AQ*); fakta `F-AQ-1`..`6` |
| **Kontrak** | Delta aditif: field baru pada DTO Membership Tier, Queue Display Device, response antrean perawat/dokter, response pendaftaran, dan payload realtime. Field lama tidak dihapus atau diganti nama |
| **`IMPLEMENTATION_AUTHORITY`** | `GRANTED` dalam `CROSS-REPO MODE` (`RJ-DOC-DEC-060`). Tanpa commit, push, merge, deploy |
| **Migration** | Satu migration untuk `BE-015` dan `BE-016`; dibuat dan diterapkan hanya ke `QuilvianNewDevSukma` |
| **Verifikasi** | Pola Bank Darah: tanpa project test; build, EF, QBE Strict, runtime HTTP terhadap `QuilvianNewDevSukma` |

Legenda tanda status: `✅` selesai, `🟡` sebagian, `⛔` terblokir, tanpa tanda = belum dimulai.

### Grafik Urutan Dependency

```text
RJ-DOC-REV-BE-015 ✅ ─> RJ-DOC-REV-BE-016 ✅ ─> RJ-DOC-REV-FE-016 ✅
```

| Gelombang | Task | Boleh mulai bila |
| ---: | --- | --- |
| 1 | `RJ-DOC-REV-BE-015` | Wewenang `RJ-DOC-DEC-060` |
| 2 | `RJ-DOC-REV-BE-016` | `BE-015` selesai (memakai snapshot klasifikasi) |
| 3 | `RJ-DOC-REV-FE-016` | `BE-015` dan `BE-016` selesai |

### 17.1 Task backend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-BE-015` | **Klasifikasi dan alokator nomor antrean.** (1) `MstMembershipTier.QueueAudience` dan `PublicDisplayMode` (enum) pada model, configuration, DTO, create/update/response. (2) `OutpatientQueueClassificationService`: membership aktif pada tanggal kunjungan → `IsMember`, `IsPriorityQueue`, `PriorityLevel`, `QueueAudience`, `PublicDisplayMode`, id/kode tier. (3) Snapshot di `TrxQueue`. (4) `OutpatientQueueNumberAllocator` menggantikan `GenerateQueueNumberAsync`: nomor cadangan dari konfigurasi, cakupan sama, advisory lock transaksi, kolom `QueueScopeKey` dan unique index `(QueueDate, ServiceUnitId, QueueScopeKey, QueueNumber)`. (5) Response pendaftaran memuat `queueClassification` | — | Skenario wajib 1–8, 12–14 spesifikasi (`RJ-DOC-DEC-055`, `056`) | Build; EF tanpa perubahan model tertunda; QBE Strict; runtime HTTP R1..Rn | ✅ `COMPLETE` `2026-10-08` — build Release `0 Error` (239 warning), EF tanpa perubahan model, QBE Strict `PASS` (54 berkas), migration `20261008043304_AddOutpatientQueueClassificationAndRenameRegQueue` diterapkan ke `QuilvianNewDevSukma` (213 baris utuh), runtime gabungan `26/26 PASS`. Delta: rename `TrxQueue` → `RegQueue` (`RJ-DOC-DEC-061`). [Laporan](../task/report/backend/RJ-DOC-REV-BE-015.md) |
| ✅ `RJ-DOC-REV-BE-016` | **Layar publik dan realtime.** (1) `MstQueueDisplayDevice.QueueAudienceMode` beserta DTO. (2) `QueueDisplayRuntimeController` menyaring audience dan menerapkan `PublicDisplayModeSnapshot`. (3) `QueueVoiceService` tidak menyebut nama/No. RM bila snapshot melarang. (4) Penanda internal `isMember`/`queueAudience` pada antrean perawat/dokter dan payload realtime (tanpa data member) | `BE-015` | Skenario wajib 9–11, 15 spesifikasi (`RJ-DOC-DEC-057`, `058`) | Build; QBE Strict; runtime HTTP | ✅ `COMPLETE` `2026-10-08` — build/QBE sama dengan `BE-015`, runtime T9a–T9e, T10, T10b, T11, T15b, T15c `PASS`. Realtime WebSocket tidak diuji (dibuktikan lewat kode). [Laporan](../task/report/backend/RJ-DOC-REV-BE-016.md) |

### 17.2 Task frontend

| Task | Isi | Dependency | Acceptance criteria | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| ✅ `RJ-DOC-REV-FE-016` | **Konfigurasi dan penanda.** Form Membership Tier: *Queue Audience* dan *Public Display Mode*. Form Queue Display Device: *Audience Display*. Antrean perawat/dokter: badge *Member* dan *Prioritas* dari backend. Kiosk dan pendaftaran petugas hanya menampilkan nomor dari backend. Tanpa logika nomor cadangan di React | `BE-015`, `BE-016` | Field tersimpan dan terbaca ulang; badge mengikuti flag backend; tidak ada aturan bisnis baru di frontend | ESLint; `npm run build` | ✅ `COMPLETE` `2026-10-08` — ESLint `0 error` (1 warning lama), `npm run build` `PASS`, `npm run test:unit` 8 gagal sama dengan baseline `HEAD`, UI GATE REUSE 3 / COMPOSE 1 / NEW 0, uji layar Playwright `11/11 PASS` terhadap backend uji 7185. Delta: sanitizer slice display ikut diperbarui; label di baris sendiri. Branch `sukmagpV2`, belum di-commit. [Laporan](../task/report/frontend/RJ-DOC-REV-FE-016.md) |

### 17.3 Traceability

| Requirement / keputusan | Task |
| --- | --- |
| `RJ-DOC-DEC-055` klasifikasi dan snapshot | `BE-015`, `FE-016` |
| `RJ-DOC-DEC-056` nomor cadangan dan alokator bersama | `BE-015` |
| `RJ-DOC-DEC-057` privasi layar/suara publik | `BE-016` |
| `RJ-DOC-DEC-058` layar Regular/Member | `BE-016`, `FE-016` |
| `RJ-DOC-DEC-059` data induk MMC | Tidak ada task — menunggu data bisnis terverifikasi |
