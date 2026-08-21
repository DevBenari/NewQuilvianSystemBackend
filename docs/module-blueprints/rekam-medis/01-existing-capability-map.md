# Rekam Medis — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `QV-RM-001` |
| Revision | `1` |
| Backend snapshot | `NewQuilvianSystemBackend`, branch `yoga`, commit `5103e68eec5529540d369673c8a4e2651be0344b` |
| Frontend snapshot | `QuilvianSystemFrontendDev`, branch `YogaV2`, commit `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Decision baseline | `00-interview-decisions.md`, revision `1`, SHA-256 `66B3D5772C73F14BC903817842A67D0964DB760EE663092F9DD87D381402836C` |
| Audit boundary | Audit source-only dan read-only terhadap source aplikasi. Runtime, isi database, status penerapan migration, konfigurasi/assignment permission produksi, layanan eksternal, SOP, dan bukti approval formal tidak diperiksa. Migration hanya membuktikan skema dalam source, bukan deployment. |

Setiap baris register memakai tepat satu status dari kontrak `trace-existing-capabilities`.
Kesimpulan `Missing` berarti tidak ditemukan kandidat model/API/consumer pada snapshot source yang
diaudit; bukan pernyataan bahwa capability tersebut tidak ada di luar Quilvian.

## Ringkasan Eksekutif

Fondasi data klinis sudah cukup luas untuk diadaptasi: pasien, encounter, tenaga klinis, unit
pelayanan, assessment, konsultasi, diagnosis, tindakan, alergi, tanda vital, CPPT, dokumen klinis,
consent, dan attachment telah memiliki model/persistence. Workspace dokter rawat jalan juga sudah
reachable dan mengonsumsi sebagian API klinis.

Fondasi tersebut belum menjadi Rekam Medis sesuai keputusan bisnis. Tidak ada aggregate episode
Rekam Medis yang terpisah dari encounter pelayanan, signature/version/addendum yang berlaku umum,
checklist kelengkapan berversi, deadline dan eskalasi per dokumen, care-relationship authorization,
break-glass, masking kategori sensitif, ataupun pelepasan informasi formal. CPPT dan dokumen klinis
bahkan memiliki mutation path yang bertentangan dengan invariant catatan final yang immutable.

## Journey As-Is yang Ditelusuri

```text
patient master
  -> patient encounter / antrean layanan
  -> assessment dan vital sign
  -> konsultasi dokter + SOAP
  -> diagnosis / tindakan / resep
  -> CPPT dibuat dari konsultasi oleh frontend
  -> complete consultation (transaction backend)
  -> queue complete + encounter ConsultationCompleted

jalur terpisah:
patient/encounter -> clinical document upload -> review/verify/approve/archive/cancel/delete

belum tersambung:
episode RM -> checklist versi-frozen -> Belum Lengkap -> reminder/escalation
           -> seluruh dokumen signed -> Ditutup Final
           -> correction/addendum/version history
           -> break-glass/review dan formal medical-information release
```

Journey rawat jalan di atas mempunyai consumer frontend. Source yang diaudit belum membuktikan
workspace Rekam Medis terpadu untuk rawat jalan, IGD, dan rawat inap.

## Capability Register

| ID | Cluster / kebutuhan | Owner existing | Evidence pada snapshot | Status | Gap atau adapter yang diperlukan | Risiko |
|---|---|---|---|---|---|---|
| `RM-CAP-01` | Identity: pasien dan nomor rekam medis | Patient Management | `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#MstPatient@5103e68` (baris 20–22: `MedicalRecordNumber`) | Ready to reuse | Pertahankan Patient Management sebagai source of truth identitas; episode RM mereferensikannya. | Membuat master pasien baru di modul RM akan menduplikasi identitas. |
| `RM-CAP-02` | Identity reconciliation/merge | Patient Management | `NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs#MstPatient@5103e68` (baris 109–112: merge pointer dan alasan) | Repair | Pointer ada, tetapi audit ini tidak menemukan lifecycle maker-checker, reversal, dan riwayat keputusan merge yang immutable. | Salah gabung dapat mengaitkan rekam klinis ke pasien yang salah tanpa pemulihan terkontrol. |
| `RM-CAP-03` | Episode layanan sebagai anchor klinis | Registration Management | `NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#TrxPatientEncounter@5103e68` (baris 25–29, 81–90, 144–164: patient, type/status, completion/cancellation) | Ready to reuse | Gunakan encounter sebagai referensi episode pelayanan. | Mengganti encounter akan memutus relasi klinis yang sudah ada. |
| `RM-CAP-04` | Episode Rekam Medis dan status `Aktif`/`Belum Lengkap`/`Ditutup Final` | Tidak ada owner yang terbukti | Scan model, enum, controller, dan service klinis/registrasi tidak menemukan aggregate atau lifecycle RM; encounter berhenti pada status layanan. Baseline: `NewQuilvianSystemBackend/docs/module-blueprints/rekam-medis/00-interview-decisions.md#Status-dan-Perubahan-Status@5103e68` (baris 215–227) | Missing | Dibutuhkan owner lifecycle RM yang terkait satu encounter tetapi tidak menyamakan akhir layanan dengan final closure. | Encounter dapat selesai saat dokumen wajib belum lengkap tanpa status RM yang dapat dipantau. |
| `RM-CAP-05` | Actor/workforce, profesi, credential, clinical privilege | Human Resource / Identity | `NewQuilvianSystemBackend/Models/ApplicationUser.cs#ApplicationUser@5103e68` (baris 18–22); `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs#MstDoctor@5103e68` (baris 76–106) | Reuse with adapter | Resolusi user-ke-tenaga/profesi tersedia, tetapi tindakan klinis perlu memvalidasi credential, privilege, penugasan, dan pengganti resmi pada waktu kejadian. | Role teknis saja dapat memberi kewenangan klinis yang terlalu luas. |
| `RM-CAP-06` | Location/resource: service unit, clinic, room, bed | Health Services Master Data + Registration | `NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#TrxPatientEncounter@5103e68` (service unit/clinic/room/doctor context); model `MstServiceUnit`, `MstClinic`, `MstRoom`, dan `MstBed` ditemukan di Health Services Master Data | Ready to reuse | Referensikan master existing untuk konteks layanan dan assignment; jangan menjadi owner baru. | Duplikasi master akan membuat scope akses dan reporting tidak konsisten. |
| `RM-CAP-07` | Assessment awal dan completion | Clinical Management | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs#UpdateAssessment@5103e68` (baris 408–432: completed tidak dapat diedit); `#CompleteAssessment` (520–590) | Extend | Completion memberi fondasi lock, tetapi belum signature universal, version lineage, addendum, dan policy dokumen wajib. Endpoint cancel juga dapat mengubah assessment completed menjadi cancelled (baris 593–614). | Catatan completed dapat hilang dari alur normal melalui cancellation tanpa koreksi terpisah. |
| `RM-CAP-08` | Konsultasi dokter/SOAP dan finalisasi transaksional | Clinical Management | `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs#CompleteAsync@5103e68` (baris 39–55, 102–129: transaction, optimistic timestamp, consultation/queue/encounter completion); DI di `Program.cs` baris 276–278 | Extend | Transaksi dan optimistic check dapat dipakai, tetapi completion konsultasi bukan final closure RM dan belum menandatangani/versioning semua dokumen. | Penyamaan consultation complete dengan RM final akan menutup episode terlalu dini. |
| `RM-CAP-09` | Completeness validation saat konsultasi | Clinical Management | `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs#ValidateAsync@5103e68` (baris 48–81: SOAP, diagnosis utama, tindakan) | Reuse with adapter | Validator sempit dan hard-coded; dapat menjadi consumer dari policy kelengkapan, bukan source of truth checklist. | Aturan berbeda layanan/kondisi dan versi efektif tidak dapat direproduksi. |
| `RM-CAP-10` | Checklist dokumen wajib berversi dan frozen saat episode mulai | Tidak ada owner yang terbukti | Tidak ditemukan model/konfigurasi checklist kelengkapan lintas dokumen atau snapshot versi episode. Baseline keputusan di `00-interview-decisions.md` baris 179–209 dan 249–251 | Missing | Dibutuhkan policy owner bersama Unit RM/Komite Medis, effective dating, applicability, dan snapshot versi pada episode. | Perubahan policy dapat retroaktif mengubah kelengkapan episode lama. |
| `RM-CAP-11` | Deadline, reminder, dan eskalasi per jenis dokumen/event | HR Workflow hanya kandidat pola | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/MasterData/Workflow/Models/MstWorkflowStep.cs#MstWorkflowStep@5103e68` (baris 67: `EscalationAfterHours`); workflow didaftarkan di `Program.cs` baris 286–293 | Reuse with adapter | Ada pola workflow/escalation generik, tetapi belum ada trigger klinis, jam kebijakan RM, recipient, reminder history, atau larangan auto-finalize. | Menggunakan workflow HR langsung dapat membawa semantik approver/escalation yang salah. |
| `RM-CAP-12` | CPPT sebagai catatan resmi, signature, correction/addendum | Clinical Management | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs#TrxPatientIntegratedProgressNote@5103e68` (baris 26–114); controller `#Update@5103e68` (492–555) dan `#Delete@5103e68` (604–632) | Conflict | Model memiliki SOAP/provider/source tetapi tidak Draft/Signed/Final/version/addendum. Update mengubah content/provider/source dan delete soft-delete; hanya cancelled/read-only-generated yang diblok. | Catatan yang telah dipakai klinis dapat ditulis ulang tanpa mempertahankan versi lama. |
| `RM-CAP-13` | Dokumen klinis/file, review, verify, approve | Clinical Management | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs#TrxPatientClinicalDocument@5103e68` (baris 25–230); controller `#Create@5103e68` (305–427), `#Update@5103e68` (448–572), `#Delete@5103e68` (766–796) | Conflict | Metadata file/hash dan review fields berguna, tetapi create menerima status/verified/approved dari request; approved/verified masih dapat di-update atau di-delete, dan verify/approve sama-sama memakai permission `Update`. | Approval dan finality dapat ditetapkan atau diubah oleh authority yang tidak terpisah. |
| `RM-CAP-14` | Signature umum, immutable version, correction, addendum | Tidak ada owner yang terbukti | Scan lintas assessment, consultation, CPPT, clinical document, diagnosis, procedure, consent, allergy, dan history tidak menemukan kontrak signature/version lineage/addendum universal. | Missing | Dibutuhkan mekanisme append-only yang menyimpan signer, waktu, meaning, versi lama, alasan koreksi, serta authority aktif/pasca-closure. | Integritas medico-legal tidak dapat dibuktikan secara konsisten. |
| `RM-CAP-15` | Shared clinical facts: diagnosis, procedure, allergy, vital sign, history, consent, attachment | Clinical Management | `NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs#ApplicationDbContext@5103e68` (baris 550–562: DbSet clinical facts/documents/consent/attachments/CPPT) | Reuse with adapter | Pertahankan Clinical Management sebagai owner fakta; masing-masing mutation path harus dipetakan ke signature/finality/correction policy. | Menyalin fakta ke aggregate RM baru menciptakan dua source of truth. |
| `RM-CAP-16` | Lab/radiologi order-result, critical result, acknowledgment, late-result follow-up | Tidak ada owner transaksi yang terbukti | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs#TrxDoctorConsultation@5103e68` (baris 134–142 menyatakan detail kelak di `OrderManagement`); clinical document hanya dapat menampung dokumen hasil | Missing | Dibutuhkan contract owner order/result; clinical document dapat menjadi adapter arsip setelah hasil diterima. | Hasil terlambat/kritis tidak memiliki routing, acknowledgment, dan follow-up yang terlacak. |
| `RM-CAP-17` | Prescription sebagai fakta lintas modul | Pharmacy Management | `NewQuilvianSystemBackend/Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs#TrxPrescription@5103e68`; finalisasi prescription dipanggil dari `ConsultationFinalizationService` baris 90–100 | Reuse with adapter | RM mereferensikan summary/status prescription dari owner farmasi; jangan menduplikasi transaksi dispensing. | Copy prescription ke RM dapat berbeda dari keadaan dispensing aktual. |
| `RM-CAP-18` | Financial touchpoint/charge completeness | Billing Management hanya master data | Scan Billing Management menemukan master payment method/category, tetapi tidak menemukan owner invoice/charge/payment/financial clearance transaksional yang dapat ditautkan ke closure RM. | Unknown | Keputusan wawancara tidak menetapkan apakah final closure RM bergantung pada charge capture; capability billing transaksi juga tidak terbukti pada source. | Ketergantungan implisit dapat membuat closure klinis tertahan atau charge hilang. |
| `RM-CAP-19` | Normal access: role + active care relationship/assignment | Shared Security | `NewQuilvianSystemBackend/Filters/AccessPermissionFilter.cs#OnAuthorizationAsync@5103e68` (baris 45–49); `Services/Security/AccessPermissionService.cs#HasAccessAsync@5103e68` (baris 22–112) | Conflict | Engine hanya menerima user/controller/action dan policy department/position; tidak menerima patient, encounter, care-team, assignment, document category, atau purpose. Ada bypass `SuperAdmin` pada baris 54–57 dan 117–150. | Pengguna dengan permission teknis dapat membuka pasien tanpa hubungan pelayanan yang sah. |
| `RM-CAP-20` | Break-glass, reauthentication, reason, expiry, review | Tidak ada owner yang terbukti | Scan auth/security, clinical controllers, frontend services, dan UI tidak menemukan emergency-access session, reauth evidence, reason, timeout, notification, atau review queue. | Missing | Dibutuhkan policy/session terpisah yang membatasi waktu dan scope, mencatat alasan/reauth, serta mengirim review Unit RM/privasi. | Akses darurat tidak dapat dibedakan dari akses biasa atau ditinjau. |
| `RM-CAP-21` | Sensitive-category masking dan step-up open | Clinical document memiliki confidentiality metadata | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Models/TrxPatientClinicalDocument.cs#TrxPatientClinicalDocument@5103e68` (baris 50–60, 160–172: confidentiality/share flags) | Reuse with adapter | Metadata dapat membantu klasifikasi, tetapi query/authorization/UI belum menerapkan default masking, reauth, alasan khusus, atau priority review. Daftar kategori juga masih blocker kebijakan. | Data sensitif dapat ikut terbuka pada pembacaan umum. |
| `RM-CAP-22` | Formal medical-information release | Tidak ada owner yang terbukti | Scan backend/frontend tidak menemukan request, verifier, legal basis, purpose, recipient, data scope, approval, exception review, atau release package/audit. | Missing | Dibutuhkan lifecycle permintaan formal dan authority Unit RM plus review privasi/hukum untuk kasus sensitif. | Pelepasan tidak dapat dibuktikan sah, terbatas, dan disetujui. |
| `RM-CAP-23` | Domain audit immutable untuk read/mutation/release | Shared identity/logging | `NewQuilvianSystemBackend/Models/IdentityModel.cs#IdentityModel@5103e68` (baris 5–23); `Services/Logging/LoggerService.cs#LoggerService@5103e68` (baris 19–164) | Extend | Metadata CRUD dan app logging tersedia, tetapi bukan ledger immutable untuk view, signature, correction, break-glass, export/release, old/new version reference, dan review outcome. | Log operasional tidak cukup membuktikan jejak akses dan perubahan medico-legal. |
| `RM-CAP-24` | Approval workflow dan assigned approver | HR Workflow Management | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs#WorkflowService@5103e68` (assigned approver, delegation, idempotency); model workflow instance/assignment dan DI `Program.cs` baris 286–293 | Reuse with adapter | Dapat menjadi mesin generik setelah adapter policy klinis/privasi; source saat ini tidak membuktikan authority Komite Medis, Unit RM, atau pejabat privasi pada kasus RM. | Menganggap workflow HR otomatis memenuhi governance klinis akan melewati authority owner. |
| `RM-CAP-25` | Transactional outbox/inbox dan reconciliation lintas modul | Tidak ada owner yang terbukti | Scan source tidak menemukan outbox/inbox/integration-event transactional. Finalisasi konsultasi lokal memakai transaction, tetapi frontend membuat CPPT dan menyelesaikan konsultasi melalui request terpisah. | Missing | Dibutuhkan reliability contract untuk handoff lintas module dan retry idempotent. | Partial failure dapat menyisakan CPPT tanpa consultation completion atau sebaliknya. |
| `RM-CAP-26` | Frontend workspace dokter rawat jalan | Frontend Registration/Clinical workspace | `QuilvianSystemFrontendDev/src/app/health-services/registration-management/doctor-queues/page.jsx#DoctorQueuePage@c4e2ef2`; `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx#DoctorQueueView@c4e2ef2` (baris 23–27, 122–192) | Extend | Route reachable dan tab SOAP/CPPT/resep/tindakan/surat tersedia, tetapi bukan worklist RM, belum mencakup IGD/rawat inap, completion review, addendum, access review, atau release. | UI klinis yang ada dapat disalahartikan sebagai modul RM lengkap. |
| `RM-CAP-27` | Frontend finalization orchestration | Frontend doctor queue workspace | `QuilvianSystemFrontendDev/src/lib/hooks/health-services/registration-management/doctor-queue/useDoctorConsultationWorkspace.js#handleConfirmFinalizeConsultation@c4e2ef2` (baris 246–289); modal baris 31–38 | Conflict | Before-finalize handler dan generated CPPT berjalan sebelum finish dalam HTTP call terpisah; modal menyebut checklist lebih luas daripada validator backend. | Partial write dan false assurance: UI mengatakan lengkap sementara backend memeriksa subset berbeda. |
| `RM-CAP-28` | Frontend resilience dan concurrency klinis | Frontend clinical hooks | `QuilvianSystemFrontendDev/src/lib/hooks/health-services/clinical-management/use-doctor-soap.js#useDoctorSoap@c4e2ef2` (autosave baris 427–488); CPPT tab menampilkan loading/empty/refresh | Extend | Loading/error/save state ada. Tidak ditemukan stale-version conflict UI untuk autosave, signed lock, correction/addendum, atau durable retry/idempotency indicator. | Last-write atau retry dapat menimpa data tanpa pengguna memahami konflik. |
| `RM-CAP-29` | Frontend RM privacy/completeness/release | Tidak ada owner frontend yang terbukti | Scan services/components/routes tidak menemukan consumer clinical-document, break-glass, sensitive open, access review, completeness dashboard, atau release request. | Missing | Diperlukan UI sesuai authority frontend yang sudah dikunci setelah kontrak backend/produk disetujui. | Capability backend kelak tetap tidak operasional bagi Unit RM tanpa consumer. |
| `RM-CAP-30` | Automated tests untuk invariant RM | Test infrastructure ada, coverage RM tidak terbukti | `QuilvianSystemFrontendDev/package.json#scripts@c4e2ef2` mendefinisikan unit/Playwright; scan nama/isi test tidak menemukan scenario signature, immutable correction, completeness, break-glass, atau release. Tidak ditemukan project test backend relevan. | Missing | Dibutuhkan contract, authorization, transition, concurrency, audit, dan E2E tests berbasis acceptance criteria. | Regression pada finality/privacy dapat lolos tanpa bukti otomatis. |

## Provider dan Consumer As-Is

| Swagger group / provider | Route penting | Consumer frontend yang ditemukan | Catatan kontrak |
|---|---|---|---|
| Health Services / Registration Management / Patient Encounter | `GET/POST api/v1/.../patient-encounters`, `PATCH {id}/status`, `check-in`, `cancel`, `DELETE {id}` | Registration dan doctor-queue context | Encounter memiliki lifecycle pelayanan, bukan lifecycle kelengkapan RM. |
| Health Services / Clinical Management / Patient Assessment | list/detail, `active-by-encounter`, `active-by-queue`, create/update/complete/cancel | `doctor-queue.service.js` | Completed di-lock untuk update, tetapi cancellation masih dapat mengganti status. |
| Health Services / Clinical Management / Doctor Consultation | list/detail, `active-by-queue`, create/update, `PATCH soap`, validation, complete, cancel | `doctor-consultation.service.js`, doctor queue hook | Complete memakai transaction backend; checklist masih hard-coded. |
| Health Services / Clinical Management / Patient Integrated Progress Note | list/timeline/detail/create/from-consultation/draft/update/cancel/delete | `patient-integrated-progress-note.service.js`, CPPT tab | Consumer dapat membaca/membuat; backend belum memiliki signed/addendum semantics. |
| Health Services / Clinical Management / Patient Clinical Document | metadata/list/options/detail/create/update/review/verify/approve/archive/cancel/delete | Tidak ditemukan consumer frontend | Provider luas tetapi finality dan authority-nya konflik dengan keputusan RM. |

Semua provider material di atas memakai `AccessPermission(controller, action)` seperti `Read`,
`Create`, `Update`, atau `Delete`. Permission itu tidak membawa resource/patient context.

## Frontend State dan Mismatch

- Doctor queue route langsung merender workspace; tidak ditemukan guard permission di route/component
  tersebut. Backend tetap menjadi enforcement utama.
- Board dan tab mempunyai loading, empty, message/error, refresh, serta autosave state. Ini dapat
  di-extend untuk UX RM, tetapi tidak membuktikan retry-safe mutation atau conflict resolution.
- CPPT duplicate response pada service diperlakukan sebagai hasil menyerupai sukses. Backend hanya
  memakai pre-check untuk generated note; race-safety dan idempotency key tidak terbukti.
- Modal finalisasi meminta pengguna memastikan SOAP, CPPT, resep, tindakan, surat, penunjang, dan
  CDSS, sedangkan backend validator secara eksplisit hanya memeriksa SOAP, diagnosis utama,
  prescription finalization, serta validitas tindakan. Ini adalah provider/consumer mismatch.
- Tidak ditemukan consumer frontend bagi dokumen klinis, allergy, consent, release, break-glass,
  access review, dan completeness worklist pada snapshot ini.

## Conflict, Unknown, dan Dampak

### Conflict yang harus diselesaikan sebelum desain target

1. CPPT dan clinical document dapat di-update/delete setelah status yang secara bisnis dapat
   dianggap resmi; keputusan mewajibkan immutable old version dan correction/addendum.
2. Create clinical document dapat menerima status/verified/approved dari caller, sementara approval
   klinis dan privasi harus mengikuti authority terpisah.
3. Authorization hanya controller/action RBAC dan bypass SuperAdmin; keputusan mewajibkan role plus
   active care relationship atau assignment.
4. UI finalisasi dan backend validator mempunyai definisi kelengkapan berbeda.
5. Frontend membuat CPPT dan melakukan finalisasi melalui beberapa request tanpa atomic handoff.

### Unknown yang tetap terbuka

- Status migration dan tabel di setiap environment.
- Seed permission serta assignment role/department/position produksi.
- Sistem eksternal lab/radiologi, billing, archive, identity provider/reauth, notification, dan legal
  release yang mungkin ada di luar dua repository.
- Kebijakan lokal untuk durasi break-glass, kategori sensitif, SLA per dokumen, bukti kewenangan
  pemohon release, duplicate/downtime, wrong-patient/entered-in-error, dan approval formal. Daftar
  blocker kanonik berada di `00-interview-decisions.md` baris 310–320.
- Dampak final closure RM terhadap coding/claim/billing belum diputuskan.

### Impact-scan trigger

Peta ini harus diaudit ulang bila salah satu hal berikut berubah:

- SHA backend atau frontend, khususnya model/controller/service clinical, encounter, permission,
  workflow, doctor queue, dan clinical service consumers;
- decision-log revision/hash atau approval status;
- migration/deployment evidence, seed permission, atau contract sistem eksternal tersedia;
- policy signature, break-glass, sensitive category, completeness/SLA, release, downtime, duplicate,
  dan wrong-patient disetujui;
- Order Management, billing transactional, atau archive/integration owner ditambahkan.

## Closure Questions untuk Tahap Berikutnya

Audit capability sudah selesai, tetapi domain design belum boleh mengarang jawaban atas pertanyaan
berikut:

1. Apakah setiap jenis catatan memakai satu signature contract, dan apa bukti autentikasi/meaning
   tanda tangan yang diterima?
2. Bagaimana lifecycle `Entered in Error`, salah pasien, pembatalan, dan addendum untuk setiap kelas
   catatan—termasuk apakah catatan tetap muncul dengan watermark?
3. Berapa durasi break-glass, kategori sensitif, reviewer SLA, dan penerima notifikasi?
4. Apa matriks dokumen wajib, event pemicu deadline, nilai SLA, serta eskalasi untuk rawat jalan,
   IGD, dan rawat inap?
5. Apa bukti kewenangan yang sah untuk tiap pemohon pelepasan dan bagaimana paket data diserahkan?
6. Apakah `Ditutup Final` menjadi prasyarat coding/claim/billing atau hanya status governance RM?
7. Sistem mana yang menjadi owner order/result lab-radiologi dan bagaimana hasil terlambat/kritis
   dikirim serta diakui?
8. Siapa individu yang memberi approval formal atas policy klinis, privasi, dan release, beserta
   tanggal dan artefak buktinya?

## Kesimpulan Audit

Tidak direkomendasikan membuat ulang patient, encounter, workforce, location, atau shared clinical
facts. Capability tersebut harus direuse atau diadaptasi dari owner existing. Area yang benar-benar
baru adalah lifecycle episode RM, signature/version/addendum, completeness policy berversi,
deadline/escalation klinis, contextual authorization, break-glass, sensitive access, formal release,
dan domain audit. Mutation CPPT/clinical-document serta authorization existing harus diperlakukan
sebagai conflict yang wajib diselesaikan, bukan sebagai kontrak siap pakai.
