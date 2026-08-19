# Q-CARE Project Rules

| | |
|---|---|
| Nama | Q-CARE — Quilvian Care Reminder & Engagement Project Rules |
| Versi | `0.1.0` |
| Status | **Proposed — belum menjadi aturan aktif sebelum disetujui pemilik yang berwenang** |
| Tanggal | 2026-08-12 |
| Cakupan | Backend, frontend, integrasi, keamanan, audit, dan pengujian Q-CARE |
| Produk induk | Quilvian Patient Journey |
| Dokumen sumber utama | `docs/Q-Care/Q-CARE_Developer_Handoff.pdf` |

## 1. Tujuan

Dokumen ini menjadi calon sumber aturan resmi pengembangan Q-CARE di project Quilvian.
Q-CARE adalah centralized event-driven reminder and engagement engine: menerima event dari
modul pemilik data, mengevaluasi rule, menjadwalkan komunikasi, memproses respons pasien,
membuat tindak lanjut internal, dan mencatat seluruh lifecycle secara dapat diaudit.

Dokumen ini tidak menyatakan bahwa Q-CARE sudah diimplementasikan. Ia menentukan pagar
pengaman agar implementasi backend dan frontend berikutnya:

- tidak menduplikasi patient, doctor, encounter, appointment, prescription, billing, atau
  entity milik modul lain;
- tidak berubah menjadi kumpulan cron job dan WhatsApp sender terpisah;
- dapat menangani event/callback berulang tanpa transaksi ganda;
- menjaga privacy, permission, audit, dan ownership data;
- dapat dibuktikan melalui migration, API contract, automated test, dan UAT.

## 2. Arti Kata Normatif

| Penanda | Arti |
|---|---|
| **MUST** | Wajib dipenuhi. Pelanggaran menjadi blocker implementasi atau release. |
| **MUST NOT** | Dilarang. Tidak boleh dilewati hanya karena UI atau build berhasil. |
| **SHOULD** | Rekomendasi kuat; penyimpangan memerlukan alasan tertulis. |
| **CONFIG** | Nilai bisnis yang harus configurable dan versioned, bukan hardcoded. |
| **DECISION REQUIRED** | Belum menjadi aturan aktif sampai disetujui pemilik yang berwenang. |

Semua aturan dalam versi `0.1.0` berstatus **Proposed**. Label MUST menunjukkan kekuatan
aturan setelah rulebook ini diaktifkan, bukan klaim bahwa persetujuan organisasi sudah ada.

## 3. Hierarki Sumber Kebenaran

Jika terdapat pertentangan, gunakan urutan berikut:

1. regulasi, kewajiban hukum, dan kebijakan privacy/security yang berlaku;
2. kontrak payer serta SOP rumah sakit yang telah disahkan dan masih efektif;
3. keputusan Product Owner/domain owner yang terdokumentasi;
4. rulebook modul yang telah disetujui;
5. API contract, event contract, ERD, dan state-transition matrix yang disetujui;
6. source code dan automated test;
7. mockup, contoh payload, atau contoh message;
8. asumsi developer/agent.

Source code existing adalah bukti keadaan implementasi, tetapi bukan alasan untuk
mengabaikan aturan bisnis yang lebih tinggi. Sebaliknya, PDF atau mockup bukan bukti bahwa
endpoint, tabel, provider, atau workflow sudah tersedia.

Jika dua sumber pada tingkat yang sama bertentangan, implementasi MUST dihentikan pada
bagian yang terdampak sampai keputusan tercatat. Agent/developer MUST NOT memilih diam-diam.

## 4. Batas Domain

### 4.1 Yang dimiliki Q-CARE

Q-CARE menjadi owner untuk:

- reminder rule dan versi rule;
- message template dan versi template;
- reminder instance;
- delivery attempt dan provider callback;
- transport envelope, provider receipt, dan raw inbound message yang diterima melalui
  channel Q-CARE hanya selama retention teknis yang disetujui;
- follow-up task dan escalation yang dibuat Q-CARE;
- status history dan audit lifecycle Q-CARE;
- provider adapter untuk komunikasi pasien;
- dashboard operasional reminder dan engagement.

### 4.2 Yang tidak dimiliki Q-CARE

Q-CARE MUST NOT menjadi owner atau membuat salinan master untuk:

- patient dan medical record;
- doctor, employee, application user, organization unit, dan hospital site;
- encounter, queue, visit IGD, admission, appointment, room, dan bed;
- diagnosis, clinical assessment, procedure, prescription, medication plan, dan result;
- insurance provider, patient insurance, payer contract, tariff, invoice, payment, claim,
  dan reimbursement case;
- survey, complaint, management issue, recommendation, dan action plan Q-VOICE;
- rule bundling, medical necessity, charge hold, dan override Clinical Charge Guard.

Q-CARE menyimpan identifier sumber dan snapshot minimum yang diperlukan untuk histori,
tetapi status bisnis sumber tetap ditentukan oleh modul pemiliknya.

## 5. Matriks Ownership dan Reuse

| Data/kemampuan | Owner | Cara Q-CARE menggunakan | Larangan |
|---|---|---|---|
| Patient dan nomor kontak | Patient Management | Referensi `PatientId`; baca kontak terverifikasi | Jangan membuat `PatientQCare` |
| Communication consent/preference | Patient Management atau shared patient preference — **DECISION REQUIRED** | Dibaca saat scheduling dan pre-send | Jangan menyamakan consent klinis dengan consent komunikasi |
| Doctor/employee/user | HR/Identity | Referensi actor, recipient internal, dan assignee | Jangan membuat master petugas Q-CARE |
| Encounter dan queue | Registration Management | Sumber event dan konteks patient journey | Jangan mengubah status langsung dari reminder worker |
| Visit/disposition IGD | Emergency Installation Management | Sumber event provisional, disposition, dan completion | Q-CARE tidak mengeksekusi disposition atau menutup visit |
| Appointment | Appointment/Registration — source final **DECISION REQUIRED** | Sumber jadwal, reschedule, cancel, dan no-show | Flag `IsAppointment` saja tidak boleh dianggap aggregate lengkap |
| Prescription/medication plan | Pharmacy/Clinical | Sumber reminder obat dan stop/reschedule event | Q-CARE tidak menghitung dosis atau mengubah terapi |
| Lab/Radiology | LIS/RIS atau modul pemilik | Sumber schedule, preparation, completion, result event | Jangan membuat order/result bayangan sebagai sumber utama |
| Invoice/payment | Billing/Finance | Sumber outstanding, partial paid, paid, overdue | Q-CARE tidak menyatakan invoice lunas sendiri |
| Reimbursement | Insurance/Reimbursement | Sumber owner dokumen dan lifecycle kasus | Jangan menyimpulkan status payer dari pesan pasien saja |
| Survey/complaint | Q-VOICE | Q-CARE mengirim invitation/update dan menyerahkan respons melalui contract | Q-CARE hanya menyimpan transport envelope minimum; jangan menduplikasi jawaban survei atau workflow complaint |
| Charge compliance | Clinical Charge Guard | Q-CARE dapat mengirim pemberitahuan/task berdasarkan event | Q-CARE tidak menentukan hold/release finansial |
| Attachment evidence | DMS/object storage/shared attachment service | Simpan secure reference dan metadata minimum | Jangan menyimpan binary besar di message table |

Entity existing yang harus digunakan kembali antara lain `MstPatient`, `MstDoctor`,
`TrxPatientEncounter`, `TrxQueue`, `TrxPrescription`, `TrxPatientProcedure`,
`MstInsuranceProvider`, `MstPatientInsurance`, `MstInsuranceTariff`, dan
`MstInsuranceCoverageRule`.

## 6. Format Register Rule

Setiap business rule yang diaktifkan MUST memiliki register berikut:

| Field | Wajib | Keterangan |
|---|---:|---|
| `RuleCode` | Ya | Kode stabil, misalnya `QCARE-CONTROL-H1` |
| `Name` | Ya | Nama yang dapat dipahami pemilik bisnis |
| `Category` | Ya | CONTROL, LAB, RADIOLOGY, MEDICATION, PAYMENT, atau REIMBURSEMENT |
| `Owner` | Ya | Unit/role yang berwenang mengesahkan |
| `Source` | Ya | SOP, kontrak, regulasi, atau keputusan produk |
| `Version` | Ya | Versi immutable rule |
| `EffectiveFrom`/`EffectiveTo` | Ya | Masa berlaku |
| `TriggerEvent` | Ya | Event yang memulai evaluasi |
| `Conditions` | Ya | Scope unit, status, payer, segment, dan kondisi lain |
| `ReferenceTimeField` | Kondisional | Waktu sumber untuk menghitung schedule |
| `Offset`/`Recurrence` | Kondisional | Jadwal relatif dan pengulangan |
| `RecipientPolicy` | Ya | Patient, authorized contact, atau internal role |
| `TemplateVersion` | Ya | Template yang telah disetujui |
| `StopConditions` | Ya | Kondisi yang membatalkan/mengakhiri reminder |
| `RetryPolicy` | Ya | Error retryable, jumlah retry, dan backoff |
| `EscalationPolicy` | Kondisional | Owner, SLA, priority, dan escalation chain |
| `OverridePolicy` | Kondisional | Permission, actor, reason, dan evidence |
| `AcceptanceTests` | Ya | Skenario normal, exception, duplicate, dan failure |

Rule lama MUST NOT diedit dengan menghilangkan histori. Perubahan membuat versi baru dengan
effective date. Reminder existing mempertahankan snapshot versi rule yang digunakan ketika
dibuat, kecuali terdapat proses migrasi yang disetujui dan dapat diaudit.

## 7. Aturan Governance dan Arsitektur

| ID | Level | Aturan |
|---|---|---|
| `QCARE-GOV-001` | MUST | Q-CARE menggunakan satu Reminder Engine, satu mekanisme scheduler/queue, satu transition validator, dan satu audit model. |
| `QCARE-GOV-002` | MUST NOT | Modul sumber tidak boleh membuat cron reminder atau WhatsApp gateway production sendiri ketika kebutuhannya termasuk kontrak Q-CARE. |
| `QCARE-GOV-003` | MUST | Perbedaan appointment, lab, radiology, medication, billing, dan reimbursement diisolasi melalui domain adapter. |
| `QCARE-GOV-004` | MUST | Business logic berada di domain/application service, bukan controller, React component, provider adapter, atau hosted worker. |
| `QCARE-GOV-005` | MUST | Provider-specific payload berhenti di provider adapter dan tidak bocor ke domain model. |
| `QCARE-GOV-006` | MUST | Seluruh service, adapter, worker, dan option Q-CARE didaftarkan melalui satu module registration yang dapat diverifikasi. |
| `QCARE-GOV-007` | MUST | Open decision tidak boleh diubah menjadi default production tanpa owner dan persetujuan tertulis. |
| `QCARE-GOV-008` | SHOULD | Implementasi awal tetap berada dalam modular monolith Quilvian; ekstraksi service hanya dilakukan berdasarkan kebutuhan scale/deployment yang terbukti. |

## 8. Aturan Event, Queue, dan Konsistensi

| ID | Level | Aturan |
|---|---|---|
| `QCARE-EVT-001` | MUST | Setiap inbound event memiliki `eventId`, `eventType`, `source`, `occurredAt`, `correlationId`, dan exact source reference. |
| `QCARE-EVT-002` | MUST | Inbound event menggunakan durable inbox atau mekanisme setara; simpan penerimaan dan idempotency key sebelum acknowledgement atau pemrosesan asynchronous. |
| `QCARE-EVT-003` | MUST | `eventId` dan idempotency key memiliki unique constraint. Replay menghasilkan no-op atau hasil yang sama, bukan reminder ganda. |
| `QCARE-EVT-004` | MUST | Scheduler hanya memindahkan reminder yang due ke queue; stateless worker melakukan pre-send validation dan pengiriman. |
| `QCARE-EVT-005` | MUST NOT | Membuat satu cron/timer per patient atau menjalankan scheduler bisnis di browser. |
| `QCARE-EVT-006` | MUST | Worker memeriksa ulang consent, kontak, current source state, stop condition, duplicate lock, template, dan quiet-hours policy tepat sebelum send. |
| `QCARE-EVT-007` | MUST | Callback provider diproses idempotent berdasarkan provider message, callback type/status, dan provider timestamp. |
| `QCARE-EVT-008` | MUST | Callback terlambat atau tidak berurutan tidak boleh membuat lifecycle state mundur. |
| `QCARE-EVT-009` | MUST | Retry hanya untuk kegagalan teknis yang diklasifikasikan retryable. Validation error, opt-out, dan permanent rejection tidak di-retry. |
| `QCARE-EVT-010` | MUST | Item yang melewati retry limit masuk dead-letter state dan dapat direplay secara aman setelah penyebab diperbaiki. |
| `QCARE-EVT-011` | MUST | Event final seperti paid, completed, cancelled, specimen received, atau medication stopped diprioritaskan untuk menghentikan send yang menunggu. |
| `QCARE-EVT-012` | MUST | Timestamp persistence konsisten dalam UTC; Asia/Jakarta digunakan sebagai default business/display timezone bila belum ada kebijakan site-specific. |
| `QCARE-EVT-013` | MUST | Outbound integration event disimpan secara atomik dalam transaksi database yang sama dengan perubahan state bisnis melalui transactional outbox atau jaminan setara; dispatcher menerbitkannya setelah commit dan aman terhadap publish ulang. |

Idempotency reminder minimum mempertimbangkan:

```text
patientId + sourceSystem + sourceReference + ruleVersion + occurrenceTime
```

Kombinasi final harus ditetapkan pada ERD dan unique index; concatenated string tanpa
normalisasi tidak cukup sebagai jaminan konsistensi.

## 9. Baseline State Model

Baseline berikut masih memerlukan persetujuan domain owner sebelum status menjadi enum
aktif. Tabel ini adalah projection status reminder. Setiap delivery attempt tetap memiliki
identity dan status sendiri berdasarkan provider message ID. Semua perubahan status MUST
melewati satu transition validator dan ditulis sebagai append-only history.

| Status | Makna | Allowed next |
|---|---|---|
| `SCHEDULED` | Instance dibuat dan menunggu due time | `QUEUED`, `SUPPRESSED`, `CANCELLED` |
| `QUEUED` | Masuk durable queue | `SENT`, `FAILED`, `SUPPRESSED`, `CANCELLED` |
| `SENT` | Provider menerima outbound request | `DELIVERED`, `FAILED`, `EXPIRED` |
| `DELIVERED` | Pesan sampai menurut provider | `READ`, `RESPONDED`, `EXPIRED` |
| `READ` | Pesan dibaca | `RESPONDED`, `EXPIRED` |
| `RESPONDED` | Respons diterima dan diproses | `COMPLETED`, `CLOSED` |
| `FAILED` | Upaya teknis gagal dan belum ada bukti delivery yang lebih kuat | `QUEUED`, `DELIVERED`, `READ`, `CLOSED` |
| `SUPPRESSED` | Tidak dikirim karena policy/state | `SCHEDULED`, `CLOSED` sesuai reason |
| `EXPIRED` | Response window berakhir | `CLOSED` |
| `CANCELLED` | Reminder tidak lagi relevan | Terminal |
| `COMPLETED` | Target business action terkonfirmasi selesai | Terminal |
| `CLOSED` | Ditutup administratif tanpa mengklaim target selesai | Terminal; reason wajib |

Rekomendasi aturan status:

- `ESCALATED` SHOULD menjadi lifecycle task/escalation, bukan delivery status reminder.
- `OVERDUE` SHOULD dihitung dari due time/SLA, bukan menjadi status yang merusak history.
- Transisi koreksi `FAILED -> DELIVERED/READ` hanya berlaku ketika callback bertanda tangan
  untuk provider message/attempt yang sama membuktikan status dengan precedence lebih
  tinggi. History `FAILED` tidak dihapus; projection reminder dihitung ulang secara
  deterministik dari seluruh attempt dan callback.
- Callback failure yang datang setelah `DELIVERED` atau `READ` tidak menurunkan projection.
  Callback milik attempt lama juga tidak boleh menggagalkan attempt lain yang sudah sukses.
- `SUPPRESSED` reason wajib membedakan consent, invalid contact, quiet hours, invalid source
  state, frequency cap, dan policy lain.
- Resume dari `SUPPRESSED` hanya diizinkan untuk reason yang reversible dan setelah
  pre-send validation diulang.
- `COMPLETED` hanya digunakan ketika modul sumber mengonfirmasi target action selesai.

Baseline follow-up task:

```text
OPEN -> ASSIGNED -> IN_PROGRESS -> WAITING_EXTERNAL -> RESOLVED
  \          \              \                         -> CANCELLED
   +---------- escalation level dan SLA history terpisah ----------+
```

## 10. Aturan Reminder dan Respons

| ID | Level | Aturan |
|---|---|---|
| `QCARE-RMD-001` | CONFIG | Offset, recurrence, business day, quiet hours, frequency cap, stop condition, dan escalation policy tidak boleh hardcoded per unit. |
| `QCARE-RMD-002` | MUST | Reminder selalu terikat ke exact source reference; patient saja tidak cukup ketika ada beberapa appointment, invoice, atau kasus. |
| `QCARE-RMD-003` | MUST | Reschedule membatalkan instance lama yang relevan dan membuat schedule baru dari versi sumber terbaru secara idempotent. |
| `QCARE-RMD-004` | MUST | Cancel/completed/paid/stopped pada source menghentikan instance yang masih pending dalam SLA yang disetujui. |
| `QCARE-RMD-005` | MUST | Respons pasien disimpan sebagai fakta komunikasi sebelum dipetakan ke intent/action. |
| `QCARE-RMD-006` | MUST | Intent ambigu atau tidak dikenali membuat task review; tidak boleh menebak perubahan state berisiko. |
| `QCARE-RMD-007` | MUST | Respons yang datang setelah expiry tetap disimpan, lalu dievaluasi terhadap source state terbaru. |
| `QCARE-RMD-008` | MUST | `REQUEST_RESCHEDULE` dan `CANCEL_APPOINTMENT` hanya memanggil source API apabila kontrak dan authorization mengizinkan; selain itu buat task. |
| `QCARE-RMD-009` | MUST | `MED_COMPLAINT` membuat task klinis sesuai severity policy; Q-CARE tidak memberi diagnosis atau mengubah therapy. |
| `QCARE-RMD-010` | MUST | Payment proof atau reimbursement evidence membuat verification task; upload pasien bukan bukti otomatis bahwa pembayaran selesai. |

## 11. Aturan Messaging, Security, dan Privacy

| ID | Level | Aturan |
|---|---|---|
| `QCARE-SEC-001` | MUST | Outbound proactive WhatsApp menggunakan template provider yang telah disetujui dan versioned. |
| `QCARE-SEC-002` | MUST | Webhook provider diverifikasi dengan signature/secret provider dan idempotency; JWT user bukan pengganti webhook authentication. |
| `QCARE-SEC-003` | MUST | Secret, access token, dan signing key berada pada secret/configuration provider yang disetujui, tidak di source code atau log. |
| `QCARE-SEC-004` | MUST | Pesan, toast, log, dan notification preview menggunakan minimum necessary data. Diagnosis dan detail hasil klinis tidak ditampilkan. |
| `QCARE-SEC-005` | MUST | Secure link memiliki opaque token, expiry, intended action/scope, replay protection, dan revocation behavior. Patient ID/GUID tidak menjadi credential. |
| `QCARE-SEC-006` | MUST | Evidence/media disimpan di secure object storage atau DMS; database message hanya menyimpan reference dan metadata aman. |
| `QCARE-SEC-007` | MUST | RBAC dan data scope ditegakkan server-side. Menyembunyikan menu atau tombol bukan security control. |
| `QCARE-SEC-008` | MUST | Manual send, cancel, reschedule, replay, suppress, resume, dan override mencatat actor, reason, permission, timestamp, correlation ID, before/after. |
| `QCARE-SEC-009` | MUST | Nomor telepon lengkap, token, isi keluhan, message body sensitif, bukti pembayaran, dan informasi klinis tidak ditulis mentah ke Serilog. |
| `QCARE-SEC-010` | DECISION REQUIRED | Retention, archive, legal hold, anonymization, dan deletion policy harus disahkan sebelum go-live. |

## 12. Kontrak Backend Project

Lokasi bounded context yang direkomendasikan:

```text
Areas/HealthServices/PatientJourneyManagement/
├── Controllers/
├── DTOs/
├── Enums/
├── Models/
├── Services/
├── Integrations/
└── DependencyInjection.cs

Repositories/Configurations/HealthService/PatientJourneyManagement/
Shared/Integration/
Migrations/
```

Entity minimum yang disarankan:

```text
MstQCareReminderRule
MstQCareMessageTemplate
MstQCareEscalationPolicy
TrxQCareReminder
TrxQCareDeliveryAttempt
TrxQCarePatientResponse
TrxQCareFollowupTask
TrxQCareStatusHistory
TrxIntegrationInbox
TrxIntegrationOutbox
TrxIntegrationDeadLetter
```

`MstPatientCommunicationPreference` direkomendasikan menjadi ownership Patient Management
atau shared patient preference, bukan ditempatkan diam-diam di Q-CARE. Keputusan ownership
harus ditutup sebelum migration.

Backend MUST mengikuti pola project:

- route versioned `api/v1/health-services/...`;
- `[Authorize]`, access controller/action, dan permission per tindakan;
- request/response DTO; entity tidak dikirim langsung;
- `ApiResponse<T>` dan pagination/filtering yang konsisten;
- EF configuration terpisah dengan FK, unique index, concurrency, dan delete behavior;
- service terdaftar di dependency injection;
- migration dan seed/config default yang dapat direview;
- failure logging yang aman serta append-only business audit;
- cancellation token pada I/O asynchronous;
- automated test untuk invariant dan integration contract.

Hosted worker MUST tipis: mengambil job, mengunci/claim secara aman, memanggil application
service, lalu mencatat outcome. Worker MUST NOT menjadi lokasi utama business rule.

## 13. Kontrak Frontend Project

Frontend berfungsi sebagai operational/admin client Q-CARE. Frontend MUST NOT menjadi
scheduler, rules engine, sumber status delivery, atau pengirim WhatsApp langsung ke provider.

Struktur teknis yang direkomendasikan:

```text
src/app/<route-yang-disetujui>/
src/components/view/health-services/patient-journey-management/
src/components/features/health-services/q-care/
src/lib/services/health-services/patient-journey-management/
src/lib/hooks/health-services/patient-journey-management/
src/lib/constants/health-services/patient-journey-management/
src/utils/health-services/patient-journey-management/
src/style/health-services/patient-journey-management/
tests/unit/q-care-*.test.mjs
tests/e2e/q-care-*.spec.mjs
```

| ID | Level | Aturan |
|---|---|---|
| `QCARE-FE-001` | MUST | Gunakan HTTP client/session handling project dan API contract backend; jangan membuat endpoint atau enum sendiri. |
| `QCARE-FE-002` | MUST NOT | Menggunakan `setInterval`, browser tab, atau local storage sebagai scheduler dan sumber delivery truth. |
| `QCARE-FE-003` | MUST | Tangani loading, empty, error, retry, stale response, conflict, dan duplicate submit. |
| `QCARE-FE-004` | MUST | Action cancel, retry, reschedule, resolve, suppress, dan replay ditampilkan berdasarkan permission dan allowed action dari backend. |
| `QCARE-FE-005` | MUST | UUID/GUID hanya untuk mekanisme internal dan tidak ditampilkan kepada pengguna. |
| `QCARE-FE-006` | MUST | Sensitive data tidak ditempatkan di generic toast, console log, URL, atau analytics payload. |
| `QCARE-FE-007` | MUST | Public action/survey link memakai opaque token; halaman tidak membuka patient context di luar scope token. |
| `QCARE-FE-008` | MUST | 401/403 ditangani sebagai akses ditolak; visibility menu bukan pengganti authorization backend. |
| `QCARE-FE-009` | SHOULD | Reuse status badge, timeline, table, filter, confirmation, access-denied, dan toast component project. |
| `QCARE-FE-010` | MUST | Menu, route final, layout, tab/drawer/page, komponen, dan tampilan mengikuti keputusan developer di bawah arahan atasan/product/UI lead. Rulebook tidak memaksakan desain visual yang belum disetujui. |

File legacy frontend `src/lib/services/reminder-scheduler.js` dan folder
`src/components/features/whast-app/` MUST diperlakukan sebagai bahan audit, bukan fondasi
production Q-CARE, sampai kontrak, dependency, security, dan pemakaiannya terbukti valid.

## 14. Aturan Integrasi IGD

Q-CARE dapat membantu langkah administrasi provisional dan tindak lanjut pasien IGD, tetapi
tidak mengambil alih workflow klinis atau penutupan encounter.

| ID | Level | Aturan |
|---|---|---|
| `QCARE-IGD-001` | MUST | `TrxEmergencyVisit`, `TrxEmergencyDisposition`, dan `TrxPatientEncounter` tetap dimiliki modul IGD/Registration. |
| `QCARE-IGD-002` | MUST NOT | Q-CARE dan seluruh komponennya mengubah disposition menjadi Executed/Completed, visit menjadi Completed, atau menutup encounter secara langsung. |
| `QCARE-IGD-003` | MUST | Q-CARE hanya membuat reminder/task dari event/status IGD yang telah dipublikasikan pemilik domain. |
| `QCARE-IGD-004` | MUST | Reminder administrasi provisional berhenti setelah modul Registration mengonfirmasi administrasi lengkap atau encounter ditutup sah. |
| `QCARE-IGD-005` | MUST | Disposition/visit/encounter completed yang tidak sah tidak boleh “diperbaiki” oleh Q-CARE; buat reconciliation/error task. |
| `QCARE-IGD-006` | MUST | Data klinis IGD sensitif tidak dimasukkan ke message preview; gunakan secure link bila komunikasi memang disetujui. |

Event berikut hanya **candidate contract**, belum klaim sebagai event existing:

| Candidate event | Owner | Perilaku Q-CARE |
|---|---|---|
| `emergency.registration.provisional` | IGD/Registration | Buat follow-up task/reminder sesuai rule aktif |
| `emergency.registration.completed` | Registration | Selesaikan/cancel follow-up provisional |
| `emergency.disposition.executed` | IGD | Hentikan reminder klinis/operasional yang tidak lagi relevan |
| `emergency.visit.completed` | IGD | Tutup journey item Q-CARE terkait visit |
| `encounter.completed` | Registration | Hentikan reminder encounter dan dapat memicu eligibility survey Q-VOICE |

Nama, schema, producer, transaction boundary, dan retry behavior candidate event tersebut
MUST disetujui melalui event contract sebelum implementasi.

## 15. Hubungan dengan Q-VOICE dan Clinical Charge Guard

### Q-VOICE

- Q-VOICE memiliki survey, feedback, complaint, SLA, investigation, service recovery,
  management issue, recommendation, action plan, dan effectiveness evaluation.
- Q-VOICE meminta Q-CARE mengirim invitation atau status update melalui integration contract.
- Q-CARE hanya memiliki transport envelope, provider delivery/read receipt, correlation,
  idempotency, serta raw inbound message selama retention teknis yang disetujui.
- Jawaban survei, feedback, complaint, klasifikasi, dan state workflow yang telah
  dinormalisasi dimiliki Q-VOICE. Setelah routing berhasil, Q-CARE menyimpan reference dan
  delivery handoff outcome, bukan salinan domain record Q-VOICE.
- Q-VOICE MUST NOT membuat gateway WhatsApp kedua.

### Clinical Charge Guard

- Clinical Charge Guard memiliki compliance rule, evaluation, finding, evidence, hold,
  override, dan release decision.
- Q-CARE dapat mengirim pemberitahuan atau task berdasarkan event yang disetujui.
- Q-CARE MUST NOT menentukan medical necessity, bundling, approval, hold, atau release.
- Emergency care tidak boleh diblokir oleh Q-CARE; financial gate tetap berada pada Billing/CCG.

## 16. Rule dalam Kode versus Konfigurasi

| Jenis | Tempat | Contoh |
|---|---|---|
| Technical invariant | Code + DB constraint + test | Idempotency, non-regression, webhook signature, exact source reference |
| Business rule berubah | Versioned database configuration | H-1/H-3, quiet hours, frequency cap, template, escalation SLA |
| Source-domain state | Modul pemilik + event/API contract | Appointment cancelled, specimen received, invoice paid |
| Security/privacy policy | Code guard + configuration + audit | Token expiry, data minimization, role scope, retention |
| Arahan UI | Brief/mockup/design system | Menu, layout, component, dan tampilan |
| Open decision | Decision log | Provider, owner, threshold, channel, dan deployment topology |

Frontend MUST NOT menghitung business state yang menjadi sumber kebenaran backend. Database
configuration MUST memiliki validation schema; JSON bebas tanpa version/schema validation
tidak cukup untuk rule production.

## 17. Minimum Automated Tests dan UAT

### Automated tests wajib

- rule offset, recurrence, quiet hours, effective date, dan stop condition;
- allowed/forbidden state transition serta callback out-of-order;
- duplicate inbound event, duplicate provider callback, retry, dan DLQ replay;
- pre-send cancellation ketika source sudah completed/cancelled/paid;
- signature webhook, expired/replayed token, RBAC, dan data leakage;
- provider adapter contract;
- setiap source-domain event contract;
- timezone/midnight/business day;
- concurrent worker claim dan duplicate send prevention;
- public link scope dan expiry;
- frontend permission action, error mapping, duplicate submit, dan UUID sanitization;
- end-to-end untuk setiap domain adapter yang diaktifkan.

### UAT minimum MVP

1. Appointment aktif menghasilkan reminder sesuai schedule.
2. Reschedule membatalkan instance lama dan membuat schedule baru.
3. Cancel/no-show/completed menghasilkan tindak lanjut yang tepat.
4. Duplicate event tidak membuat reminder ganda.
5. Callback out-of-order tidak menurunkan status.
6. Invalid contact/permanent failure membuat task tanpa retry tak terbatas.
7. Opt-out men-suppress kategori yang diizinkan dan mencatat reason.
8. Manual action tanpa permission ditolak backend.
9. Source final state menghentikan queued reminder sebelum dikirim.
10. Audit dapat menelusuri event, rule version, send attempt, response, task, dan actor.

## 18. Definition of Done

Fitur Q-CARE tidak dianggap selesai hanya karena screen, CRUD, migration, atau build tersedia.
Satu vertical slice dinyatakan Done jika:

- requirement dan Rule ID dapat ditelusuri ke API, entity, service, dan test;
- owner data dan source event telah disetujui;
- state transition dan exception path diterapkan di backend;
- DI, migration, index, seed/config, dan permission telah diverifikasi;
- idempotency, retry, stop behavior, failure, dan concurrency diuji;
- outbound, callback, response, task, dan audit terlihat end-to-end;
- frontend tidak menjalankan scheduler atau mengarang business state;
- privacy/security checks lulus;
- OpenAPI dan event schema diperbarui;
- runbook, monitoring, DLQ/replay, deployment, dan rollback tersedia sesuai scope;
- UAT terkait lulus dengan dataset yang disetujui.

## 19. Keputusan yang Harus Ditutup Sebelum Implementasi Production

| ID | Keputusan | Pemilik yang disarankan | Dampak bila belum diputuskan |
|---|---|---|---|
| `OD-001` | Provider WhatsApp dan approval/versioning template | Product/IT/Vendor | Provider adapter dan webhook belum final |
| `OD-002` | Owner serta model communication consent/opt-out | Legal/Privacy/Patient Management | Pengiriman pasien belum aman |
| `OD-003` | Kategori wajib, quiet hours, dan frequency cap | Hospital Operations/Clinical/Privacy | Pre-send policy belum final |
| `OD-004` | Source of truth appointment dan schedule | Registration/Appointment Owner | Reminder kontrol/reschedule belum aman |
| `OD-005` | SLA, priority, task owner, dan escalation chain | Operations/Clinical/Finance | Worklist tidak dapat dioperasikan |
| `OD-006` | Event names, schema, publisher, dan transaction boundary | Backend/Domain Owners | Adapter dan idempotency belum stabil |
| `OD-007` | Secure object storage/DMS dan evidence verifier | IT/Security/Finance | Upload bukti belum aman |
| `OD-008` | Payment gateway/deep link | Finance/IT | CTA pembayaran belum dapat diaktifkan |
| `OD-009` | Model reimbursement per insurer | Insurance/Finance | Reminder settlement berisiko salah |
| `OD-010` | Retention, archive, legal hold, dan deletion | Legal/Privacy/Security | Go-live diblokir |
| `OD-011` | Status `CLOSED` serta pemisahan reminder/task escalation | Product/Backend | State model belum final |
| `OD-012` | Multi-hospital tenancy, RPO/RTO, DR, dan capacity | Architecture/DevOps | Production readiness belum terbukti |

## 20. Urutan Implementasi yang Diizinkan

1. Tutup keputusan high-risk dan setujui rulebook, ownership, ERD, state matrix, serta API/event contract.
2. Bangun shared inbox/outbox, idempotency, audit history, webhook security, provider abstraction, dan test project.
3. Bangun Q-CARE core rule/scheduler/reminder/delivery/task tanpa domain adapter yang belum memiliki source valid.
4. Integrasikan satu vertical slice appointment/kontrol setelah source schedule resmi tersedia.
5. Integrasikan Q-VOICE survey invitation melalui Q-CARE.
6. Tambahkan payment/reimbursement hanya setelah transaksi sumber dan verification flow tersedia.
7. Tambahkan lab, radiology, dan medication setelah event owner serta stop condition masing-masing siap.
8. Jalankan hardening, load test, reconciliation, privacy review, UAT, dan operational readiness.

## 21. Checklist Agent/Developer Sebelum Mengubah Q-CARE

- [ ] Baca rulebook ini, decision log, capability map, ERD, dan contract terbaru.
- [ ] Tentukan Rule ID dan vertical slice yang sedang dikerjakan.
- [ ] Telusuri entity/API existing di backend dan pemakainya di frontend.
- [ ] Tandai kebutuhan sebagai reuse, adapter, extend, repair, missing, conflict, atau unknown.
- [ ] Pastikan source owner dan exact source reference jelas.
- [ ] Pastikan perubahan tidak menduplikasi patient/doctor/encounter/billing/complaint.
- [ ] Pastikan rule configurable tidak di-hardcode.
- [ ] Pastikan state transition, idempotency, concurrency, failure, dan stop behavior memiliki test.
- [ ] Pastikan permission, privacy, audit, logging, dan token/webhook security diperiksa.
- [ ] Pastikan menu/tampilan mengikuti arahan frontend yang disetujui.
- [ ] Hentikan pekerjaan pada keputusan berisiko tinggi yang belum memiliki authority.
- [ ] Jangan menyatakan Ready hanya berdasarkan keberadaan file atau build sukses.

## 22. Aktivasi dan Perubahan Rulebook

Rulebook ini menjadi aktif hanya setelah minimal disetujui oleh:

- Product Owner/Patient Journey owner;
- perwakilan Hospital Operations;
- Clinical Governance untuk alur klinis;
- Finance/Insurance untuk payment dan reimbursement;
- Security/Privacy;
- Backend lead dan Frontend lead.

Saat diaktifkan:

1. ubah status menjadi `Approved`;
2. catat approver, tanggal, versi, dan effective date;
3. tutup atau beri disposition pada seluruh open decision yang memblokir;
4. hubungkan rulebook ke `.claude/rules/q-care-development.md` sebagai referensi canonical;
5. buat requirement traceability dan automated tests berdasarkan Rule ID.

Perubahan material membuat versi baru. Riwayat rulebook, rule runtime, template, dan
acceptance tests MUST tetap dapat ditelusuri. Agent instruction tidak boleh menjadi satu-
satunya tempat penyimpanan business rule karena ia bukan sumber kebenaran produk.
