# API Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement` (`RWI-DEC-081`, PRD 23.1) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.1`; `PRD-RWI-FINAL-001` v1.0.0 |
| Tanggal | 2 September 2026 |

---

## 0. Batas dokumen ini

**Tidak satu pun endpoint di bawah dimiliki modul Rawat Inap.** Dokumen ini menyatakan apa yang
dibutuhkan ruang kerja dokter rawat inap dari modul-modul pemiliknya.

Kolom `Hak akses` adalah **satu-satunya** tempat pemetaan endpoint ke hak akses hidup;
[`permission-audit-matrix.md`](./permission-audit-matrix.md) **tidak** mendaftarnya ulang.

---

## 1. Health Services / Clinical Management / Doctor Consultation — `CAP-020`

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`
Judul grup: `[Tags("Health Services / Clinical Management / Doctor Consultation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat konsultasi beserta SOAP. **Perubahan:** menerima `InpEpisodeId` dan `ClinicalDateTime`, dan tidak menuntut `QueueId` bila episode `Admitted` | `DoctorConsultation : Create` | `CreateDoctorConsultationRequest` **+ `InpEpisodeId`, `ClinicalDateTime`, `VisitId`** | `ApiResponse<DoctorConsultationResponse>` | **Tersedia**, perilaku **Rencana** |
| `POST` | `/` | **Konsultasi kedua dan seterusnya** pada satu kunjungan rawat inap | Sama | Sama | Sama | **Rencana** — hari ini ditolak batas satu konsultasi per kunjungan |
| `GET` | `/episodes/{episodeId}/soap-timeline` | Lini masa SOAP satu episode, terurut **waktu klinis** | `DoctorConsultation : Read` | — | `ApiResponse<SoapTimelineResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/amend` | Mengamandemen SOAP final; alasan wajib | `DoctorConsultation : Amend` | `AmendConsultationRequest` | `ApiResponse<DoctorConsultationResponse>` | **Rencana (belum tersedia)** |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` / `201` | Catatan tersimpan |
| `403` | Anda bukan DPJP maupun dokter yang berwenang atas pasien ini |
| `409` | Catatan sudah final. Untuk mengubahnya pakai amandemen |
| `422` | Episode tidak sedang `Admitted` |

---

## 2. Health Services / Clinical Management / Patient Assessment — `CAP-022`

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat **kajian medis** dengan `AssessmentType = MedicalInitial` | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` + `AssessmentType` | `ApiResponse<PatientAssessmentResponse>` | **Tersedia**, jenis medis **Rencana** |
| `GET` | `/episodes/{episodeId}?assessmentType=MedicalInitial` | Membaca kajian medis satu episode | `PatientAssessment : Read` | — | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/amend` | Amandemen; `AC-CAP022-03` | `PatientAssessment : Amend` | `AmendPatientAssessmentRequest` | `ApiResponse<...>` | **Rencana (belum tersedia)** |

> Grup ini **dibagi** dengan sub-modul `keperawatan`. Pembedanya `AssessmentType`, dan kewenangan
> menulisnya bercabang menurut jenis — `validation-matrix.md` `VAL-DOK-05`.

---

## 3. Health Services / Clinical Management / Patient Integrated Progress Note — `CAP-021`

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis catatan CPPT. **Perubahan:** menerima `InpEpisodeId` | `PatientIntegratedProgressNote : Create` | `CreateProgressNoteRequest` **+ `InpEpisodeId`** | `ApiResponse<ProgressNoteResponse>` | **Tersedia**, konteks episode **Rencana** |
| `GET` | `/episodes/{episodeId}` | Lini masa CPPT lintas profesi | `PatientIntegratedProgressNote : Read` | Query `professionType`, `from`, `to` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/verify` | DPJP memverifikasi catatan. **Tidak mengubah penulis aslinya** | `PatientIntegratedProgressNote : Verify` | — | `ApiResponse<ProgressNoteResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/amend` | Koreksi setelah final; alasan wajib | `PatientIntegratedProgressNote : Amend` | `AmendProgressNoteRequest` | `ApiResponse<ProgressNoteResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/verification-status` | Catatan yang menunggu dan yang lewat batas verifikasi | `PatientIntegratedProgressNote : Read` | — | `ApiResponse<VerificationStatusResponse>` | **Rencana (belum tersedia)** |

---

## 4. Health Services / Clinical Management / Physician Visit — `CAP-025`

Base URL: `api/v1/health-services/clinical-management/physician-visits`
Judul grup: `[Tags("Health Services / Clinical Management / Physician Visit")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat visite sebagai **peristiwa tersendiri**. Menerima `Idempotency-Key` | `PhysicianVisit : Create` | `CreatePhysicianVisitRequest` | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Riwayat visite satu episode, terurut waktu visite | `PhysicianVisit : Read` | Query `doctorId`, `from`, `to` | `ApiResponse<PagedResult<PhysicianVisitListItem>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}` | Membetulkan waktu atau peran visite | `PhysicianVisit : Update` | `UpdatePhysicianVisitRequest` | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |

> **Seluruh grup ini baru.** Tidak ada satu pun endpoint visite dokter di repository hari ini.
> `EmergencyVisit` milik IGD adalah konsep lain dan **tidak** dipakai ulang.

---

## 5. Health Services / Clinical Management / Patient Procedure — `CAP-024`

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat tindakan dokter. **Perubahan:** `InpEpisodeId`, jenis catatan, kunci idempotency | `PatientProcedure : Create` | `CreatePatientProcedureRequest` **+ 3 field** | `ApiResponse<PatientProcedureResponse>` | **Tersedia**, perubahan **Rencana** |
| `GET` | `/episodes/{episodeId}` | Tindakan satu episode | `PatientProcedure : Read` | — | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/billing-dispatch` | Keadaan pengiriman tagihan | `PatientProcedure : Read` | — | `ApiResponse<BillingDispatchResponse>` | **Rencana (belum tersedia)** |

---

## 6. Health Services / Pharmacy Management / Prescription — `CAP-023`

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat resep dari konteks rawat inap. **Perubahan:** `InpEpisodeId`, jenis order, idempotency | `Prescription : Create` | `CreatePrescriptionRequest` **+ 3 field** | `ApiResponse<PrescriptionResponse>` | **Tersedia**, perubahan **Rencana** |
| `POST` | `/` | Resep **kedua dan seterusnya** pada satu konsultasi rawat inap | Sama | Sama | Sama | **Rencana** — hari ini ditolak batas satu resep aktif |
| `GET` | `/episodes/{episodeId}` | Seluruh resep satu episode beserta status pemenuhannya | `Prescription : Read` | Query `orderType` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/fulfillment-status` | Status pemenuhan dari Farmasi; `AC-CAP023-02` | `Prescription : Read` | — | `ApiResponse<FulfillmentStatusResponse>` | **Rencana (belum tersedia)** |

> **Tidak ada satu pun endpoint tulis status penyerahan di sini, dan itu disengaja.**
> `INV-DOK-04` melarang Rawat Inap menandai obat sudah diserahkan. Statusnya hanya dibaca.

---

## 7. Health Services / Laboratory Management / Lab Order — `CAP-015`

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan lab. **Perubahan:** menerima `InpEpisodeId` | `LabOrder : Create` | `CreateLabOrderRequest` **+ `InpEpisodeId`** | `ApiResponse<LabOrderResponse>` | **Tersedia**, konteks episode **Rencana** |
| `GET` | `/episodes/{episodeId}` | Pesanan dan hasil terverifikasi satu episode | `LabOrder : Read` | — | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |

> **Temuan yang perlu diketahui:** `LabOrder` terikat pada `EncounterId` saja — tanpa antrean dan
> tanpa konsultasi. Pemesanan lab rawat inap karena itu **tidak tertahan gerbang mana pun**;
> yang diminta hanya penanda episode supaya `AC-CAP015-01` dapat dibuktikan.

**Radiologi tidak punya grup endpoint** karena modulnya belum ada.

---

## 8. Endpoint milik modul lain yang dibaca ruang kerja ini

| Endpoint | Modul | Dipakai untuk |
| --- | --- | --- |
| `GET /episodes/{id}` | `episode-rawat-inap` | Konteks pasien, lokasi, DPJP |
| `GET /episodes/{id}/doctor-assignments` | `episode-rawat-inap` | Menentukan siapa DPJP pada tanggal tertentu |
| `GET /census` | `episode-rawat-inap` | Daftar pasien yang dirawat |
| `GET /patient-assessments?assessmentType=Initial` | `ClinicalManagement` | Membaca pengkajian keperawatan — **hanya baca** |
| `GET /patient-allergies`, `/patient-vital-signs` | `ClinicalManagement` | Ditampilkan pada kepala ruang kerja |
| `PUT /discharges/{episodeId}/summary` | `episode-rawat-inap` | Resume pulang tetap milik `CAP-026` di sana |

---

## 9. Yang **tidak** ada di kontrak ini

| Yang tidak ada | Alasan |
| --- | --- |
| Endpoint radiologi | Modulnya belum ada; mengarangnya berarti mengarang kepemilikan |
| Endpoint menandai obat diserahkan | `INV-DOK-04` |
| Endpoint menulis hasil laboratorium | `INV-DOK-05`; hasil final milik modul pemiliknya |
| Endpoint resume pulang | `CAP-026` milik `episode-rawat-inap` — `RWI-DEC-083` |
| Endpoint menghitung visite dari SOAP | `INV-DOK-03`; visite dicatat, bukan disimpulkan |
