# API Contract — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.3.0` |
| `last_changed_in` | `0.3.0` |
| Compatibility impact | `0.3.0`: dua endpoint `amend` berubah menjadi **penambahan addendum** sesuai `RWI-DEC-091`. Status dokumen **tidak lagi berpindah** ke `Amended`; nilai status itu dicabut. Endpoint rencana asuhan tidak berubah |
| Status | `draft` — belum disetujui manusia |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.3`; `PRD-RWI-FINAL-001` v1.0.0; decision log `13` |
| Keputusan yang mengikat | `RWI-DEC-091`, `RWI-DEC-086`, `RWI-DEC-087`, `RWI-FACT-016` |
| Tanggal | 2 September 2026 |

---

## 0. Batas dokumen ini

**Tidak satu pun endpoint di bawah ini dimiliki modul Rawat Inap.** `RWI-DEC-081` dan
`PRD-RWI-FINAL-001` bagian 23.1 menaruh seluruh tabelnya pada `ClinicalManagement`, sehingga
endpoint-nya pun lahir di sana. Dokumen ini menyatakan **apa yang dibutuhkan ruang kerja
keperawatan rawat inap**, bukan mengklaim kepemilikan.

Seluruh baris berlabel **`Rencana (belum tersedia)`** kecuali yang ditandai sebaliknya. Kolom
`Hak akses` pada tabel ini adalah **satu-satunya** tempat pemetaan endpoint ke hak akses hidup;
[`permission-audit-matrix.md`](./permission-audit-matrix.md) **tidak** mendaftarnya ulang.

---

## 1. Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Assessment")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. **Perubahan yang diminta:** menerima `InpEpisodeId` dan tidak menuntut `QueueId` bila episodenya `Admitted` | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` **+ `InpEpisodeId`, `AssessmentType`** | `ApiResponse<PatientAssessmentResponse>` | **Tersedia**, perilaku **Rencana** |
| `GET` | `/` | Daftar pengkajian. **Perubahan:** saring menurut `inpEpisodeId` dan `assessmentType` | `PatientAssessment : Read` | Query `inpEpisodeId`, `assessmentType`, `status` | `ApiResponse<PagedResult<PatientAssessmentListItem>>` | **Tersedia**, penyaring **Rencana** |
| `GET` | `/{id}` | Satu pengkajian utuh | `PatientAssessment : Read` | — | `ApiResponse<PatientAssessmentResponse>` | **Tersedia** |
| `POST` | `/{id}/addendums` | **Menambah koreksi** pada pengkajian yang sudah `Completed`. Isi asli tidak berubah; koreksi tersimpan sebagai addendum bernomor urut pada mesin keutuhan dokumen. Status pengkajian **tetap** `Completed` | `PatientAssessment : Amend` | `CreateAssessmentAddendumRequest` (`Reason` wajib, `Content` wajib) | `ApiResponse<ClinicalDocumentAddendumResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/addendums` | Daftar koreksi satu pengkajian, terurut nomor | `PatientAssessment : Read` | — | `ApiResponse<List<ClinicalDocumentAddendumResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/timeline` | Lini masa pengkajian satu episode; menjawab `AC-CAP012-02` | `PatientAssessment : Read` | — | `ApiResponse<AssessmentTimelineResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/due-status` | Keadaan tenggat dan keterlambatan menurut kebijakan aktif | `PatientAssessment : Read` | — | `ApiResponse<AssessmentDueStatusResponse>` | **Rencana (belum tersedia)** |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` / `201` | Pengkajian tersimpan |
| `400` | Isian tidak lengkap atau tidak masuk akal. Pesannya menyebut isian mana |
| `403` | Anda bukan perawat penanggung jawab episode ini dan bukan kepala ruangan |
| `409` | Pengkajian sudah final. Untuk mengubahnya pakai amandemen, bukan penyuntingan biasa |
| `422` | Episode tidak sedang `Admitted`, sehingga pengkajian rawat inap belum boleh dibuat |

---

## 2. Health Services / Clinical Management / Nursing Care Plan

Base URL: `api/v1/health-services/clinical-management/nursing-care-plans`
Judul grup: `[Tags("Health Services / Clinical Management / Nursing Care Plan")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuka rencana asuhan bagi satu episode | `NursingCarePlan : Create` | `CreateNursingCarePlanRequest` | `ApiResponse<NursingCarePlanResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Rencana asuhan episode beserta butirnya | `NursingCarePlan : Read` | — | `ApiResponse<NursingCarePlanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/items` | Menambah masalah keperawatan beserta tujuan dan rencana tindakannya | `NursingCarePlan : Update` | `CreateCarePlanItemRequest` | `ApiResponse<CarePlanItemResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/items/{itemId}` | Memperbarui butir; versi sebelumnya **tersalin**, bukan ditimpa | `NursingCarePlan : Update` | `UpdateCarePlanItemRequest` | `ApiResponse<CarePlanItemResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/items/{itemId}/evaluate` | Mencatat evaluasi hasil asuhan | `NursingCarePlan : Update` | `EvaluateCarePlanItemRequest` | `ApiResponse<CarePlanItemResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/items/{itemId}/close` | Menutup satu masalah keperawatan tanpa menghapus jejaknya | `NursingCarePlan : Update` | `CloseCarePlanItemRequest` (`Reason` wajib) | `ApiResponse<CarePlanItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/items/{itemId}/revisions` | Riwayat versi satu butir; menjawab `AC-CAP013-02` | `NursingCarePlan : Read` | — | `ApiResponse<List<CarePlanItemRevisionResponse>>` | **Rencana (belum tersedia)** |

---

## 3. Health Services / Clinical Management / Nursing Intervention

Base URL: `api/v1/health-services/clinical-management/nursing-interventions`
Judul grup: `[Tags("Health Services / Clinical Management / Nursing Intervention")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat tindakan yang **sudah dilakukan**. Menerima `Idempotency-Key` | `NursingIntervention : Create` | `CreateNursingInterventionRequest` | `ApiResponse<NursingInterventionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Daftar tindakan satu episode, terurut waktu tindakan | `NursingIntervention : Read` | Query `from`, `to`, `performedBy` | `ApiResponse<PagedResult<NursingInterventionListItem>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/finalize` | Menyatakan catatan final sehingga tidak dapat disunting diam-diam | `NursingIntervention : Update` | — | `ApiResponse<NursingInterventionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/addendums` | **Menambah koreksi** pada catatan tindakan yang sudah `Finalized`. Isi asli tidak berubah; status catatan **tetap** `Finalized` | `NursingIntervention : Amend` | `CreateInterventionAddendumRequest` (`Reason` wajib, `Content` wajib) | `ApiResponse<ClinicalDocumentAddendumResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/addendums` | Daftar koreksi satu catatan tindakan, terurut nomor | `NursingIntervention : Read` | — | `ApiResponse<List<ClinicalDocumentAddendumResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/billing-dispatch` | Keadaan pengiriman tagihan; menjawab `AC-CAP014-02` | `NursingIntervention : Read` | — | `ApiResponse<BillingDispatchResponse>` | **Rencana (belum tersedia)** |

### Catatan idempotency

`AC-CAP014-01` menuntut satu tindakan tersimpan **sekali** walaupun permintaannya diulang.
Pengulangan dengan `Idempotency-Key` yang sama menjawab `200` beserta baris yang **sudah** ada —
bukan `201`, dan bukan `409`. Bagi pengguna, tombol yang tertekan dua kali tidak melahirkan dua
tindakan.

---

## 4. Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis catatan keperawatan ke CPPT dengan `ProfessionType` perawat | `PatientIntegratedProgressNote : Create` | `CreateProgressNoteRequest` | `ApiResponse<ProgressNoteResponse>` | **Tersedia** |

> **Nol perubahan diminta pada grup ini.** Seluruh kolom penghubungnya sudah nullable dan
> `ProfessionType` sudah ada. Kontrak CPPT dimiliki sub-modul `dokter-rawat-inap` (`CAP-021`);
> baris ini dicantumkan supaya pembaca tahu catatan keperawatan **tidak** butuh tabel sendiri.

---

## 5. Endpoint milik modul lain yang dibaca ruang kerja ini

| Endpoint | Modul | Dipakai untuk |
| --- | --- | --- |
| `GET /episodes/{id}` | `episode-rawat-inap` | Konteks: pasien, lokasi, DPJP, perawat, status episode |
| `GET /episodes` | `episode-rawat-inap` | Menemukan episode yang menjadi tanggung jawab perawat |
| `GET /census` | `episode-rawat-inap` | Daftar pasien yang sedang dirawat di unit perawat |
| `GET /patient-vital-signs` | `ClinicalManagement` | Rujukan tanda vital saat pengkajian |
| `GET /patient-allergies` | `ClinicalManagement` | Ditampilkan menonjol pada ruang kerja |

---

## 6. Yang **tidak** ada di kontrak ini

| Yang tidak ada | Alasan |
| --- | --- |
| Endpoint pemakaian alat (`CAP-016`) | **Kemampuannya `DEFERRED`** lewat `RWI-DEC-089` — dikeluarkan dari scope rilis pertama secara tertulis, dan kepemilikan tabelnya sengaja tidak diputuskan. Endpoint-nya ditulis setelah `RWI-OQ-048` dibuka ulang, yaitu ketika modul persediaan/aset ada |
| Endpoint asuhan gizi (`CAP-027`) | Dimiliki modul Gizi yang berstatus `PLANNED`. Yang dimiliki sub-modul ini hanya **pemicu rujukan** dari hasil skrining gizi pada pengkajian, dan itu lahir dari `POST /patient-assessments` |
| Endpoint penghapusan pengkajian | `CAP-012` aturan 12 melarang hard-delete dan penimpaan diam-diam pada pengkajian final |
