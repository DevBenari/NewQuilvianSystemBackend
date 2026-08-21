# Integration Contract — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-integration-v0.2-draft` |
| Status | `draft` |
| Owner | Owner upstream masing-masing; Unit RM sebagai consumer |
| Approval | Belum tersedia — `RM-APR-002` |
| Input | Domain architecture revision `1`, readiness `DOMAIN_ARCHITECTURE_READY` |
| Compatibility | Existing synchronous provider dipertahankan; event target belum dimaterialisasi |
| Traceability | `RM-INT-*`, `RM-EXC-*`, `RM-BIL-001` |

## Kontrak sinkron existing-first

| Producer | Consumer | Data/tujuan | Source of truth | Kunci korelasi | Kegagalan dan rekonsiliasi |
| --- | --- | --- | --- | --- | --- |
| Patient Management | Workspace/adapter RM | Patient reference | Patient Management | `PatientId` | Jangan membuat pasien lokal; retry lookup. |
| Registration | Clinical Management/RM | Encounter dan queue | Registration | `EncounterId`, `QueueId` | Timeout bukan encounter batal. |
| Clinical assessment | Doctor consultation | Assessment dan vital awal | Clinical Management | `AssessmentId` | Mismatch patient–encounter ditolak. |
| Doctor consultation | Diagnosis/procedure/CPPT/document/consent | Konteks konsultasi | Clinical Management | `ConsultationId` | Child record tidak boleh pindah patient otomatis. |
| Clinical Management | View RM existing-first | Fakta klinis | Clinical Management | Owner record ID | Jangan membuat copy editable. |
| Pharmacy | Consultation finalization | Resep draft/finalization | Pharmacy | Prescription/consultation ID | Transaksi finalization di-rollback bila validasi gagal. |
| Billing | Procedure owner | Billing item reference | Billing/Finance | `BillingItemId` | Kegagalan billing tidak boleh membatalkan signature/closure RM. |

`ConsultationFinalizationService` existing membuka transaksi database untuk konsultasi, antrean,
encounter, dan resep. File fisiknya berada di
`Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs`, tetapi
namespace menyatakan Clinical Management. Ini utang struktur existing; jangan dipindahkan diam-diam
dalam task Rekam Medis.

## Event target yang tetap deferred

| Producer | Consumer | Event/domain contract | Status | Failure contract |
| --- | --- | --- | --- | --- |
| Clinical owner | RM episode/governance | Record created/completed/signed/corrected | Belum tersedia | Durable event, idempotent, manual reconciliation. |
| Lab/Radiologi | RM | Result released/corrected/withdrawn + version/hash | Belum tersedia | Simpan semua versi; notify DPJP/tim aktif. |
| RM | Billing/Casemix | Completeness status | Belum tersedia | Billing menentukan readiness sendiri. |
| Downtime import | RM governance | Form number, event time, input time | Belum tersedia | Nomor form unik; duplikat ditolak. |

Event deferred tidak boleh diimplementasikan dari dokumen ini sebagai kontrak approved.

## Envelope minimum untuk extension

Setiap event atau mutation resmi membawa:

- `eventType`/`action` dan versi kontrak;
- owner context dan owner record ID;
- `patientId`, `encounterId`, dan `consultationId` bila relevan;
- versi atau hash isi;
- `occurredAt`, actor/profession reference, correlation ID;
- idempotency key dan payload hash.

Payload klinis hanya dikirim bila benar-benar diperlukan dan tidak boleh masuk log transport.

## Idempotency, retry, dan partial failure

- Kunci sama + payload hash sama: kembalikan hasil pertama.
- Kunci sama + payload hash berbeda: tolak `409` dan buat rekonsiliasi.
- Retry tidak membatalkan record klinis yang sudah sah.
- Status `Sinkronisasi Tertunda` dipakai bila event belum terkirim.
- `Ditutup Final` hanya tertahan bila kegagalan membuat bukti item wajib belum tersedia.
- Nilai timeout, jumlah retry, dan backoff belum dikunci; jangan memakai angka default.

**Contoh:** konsultasi berhasil completed tetapi publish event RM gagal. Konsultasi tetap completed.
Event masuk durable queue dan diulang. Episode RM belum final bila event itu satu-satunya bukti SOAP
wajib.

## Rekonsiliasi

Rekonsiliasi membandingkan owner ID, patient–encounter, status, versi/hash, dan waktu. Perbedaan
patient atau isi klinis tidak diselesaikan dengan overwrite otomatis. Operator harus melihat
metadata konflik tanpa membuka payload sensitif di log.
