# Rehab Medik — API Contract

| Field | Nilai |
|---|---|
| Blueprint | `RHB-BP-001` r1 |
| `last_changed_in` | `v1` — status `draft` |
| Owner | Sukma Giri Pratama — belum disetujui |
| Input | `02-backend-architecture.md` r1, `state-transition-matrix.md` v1, `validation-matrix.md` v1 |
| Kompatibilitas | API baru. Tidak mengubah endpoint existing. |

**Konvensi.** Semua respons dibungkus `ApiResponse<T>`. Daftar memakai `PagedResult<T>`
dengan `pageNumber` (bawaan 1) dan `pageSize` (bawaan 25, maksimum 100). Aksi status adalah
`POST /{id}/<aksi>` dengan body minimal `{ expectedVersion, reason? }`. Tidak ada
`DELETE`/`GET options`/`PATCH status` pada transaksi. Seluruh endpoint di bawah berstatus
**Rencana (belum tersedia)**.

Kolom *Hak akses* adalah satu-satunya pemetaan endpoint ke hak akses di blueprint ini.

Base URL bersama: `api/v1/health-services/rehabilitation-management`

---

### Health Services / Rehabilitation Management / Rehab Request

Base URL: `.../rehab-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Opsi filter: status, sumber, disiplin, periode | `RehabRequest : Read` | — | `RehabRequestFilterMetadataResponse` | Rencana (belum tersedia) |
| `GET` | `/summary` | Jumlah per status pada filter aktif | `RehabRequest : Read` | `RehabRequestQuery` | `RehabRequestSummaryResponse` | Rencana (belum tersedia) |
| `GET` | `/` | Worklist permintaan | `RehabRequest : Read` | `RehabRequestQuery` (startDate, endDate, customPeriod, status, sourceType, disciplineId, search, sortBy, sortDirection, pageNumber, pageSize) | `PagedResult<RehabRequestListItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail beserta aksi yang tersedia | `RehabRequest : Read` | — | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `GET` | `/{id}/status-history` | Riwayat status | `RehabRequest : Read` | — | `List<RehabStatusHistoryItem>` | Rencana (belum tersedia) |
| `GET` | `/duplicate-check` | Kandidat duplikasi sebelum submit | `RehabRequest : Read` | `patientId`, `disciplineIds[]`, `excludeRequestId?` | `List<RehabDuplicateCandidate>` | Rencana (belum tersedia) |
| `POST` | `/` | Buat draft | `RehabRequest : Create` | `CreateRehabRequestRequest` | `RehabRequestDetailResponse` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah draft/klarifikasi | `RehabRequest : Update` | `UpdateRehabRequestRequest` (+expectedVersion) | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/submit` | Ajukan untuk telaah | `RehabRequest : Update` | `SubmitRehabRequestRequest` (expectedVersion, duplicateOverrideReason?) | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/return` | Kembalikan (administratif) | `RehabRequest : Return` | `RehabReasonRequest` | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/request-clarification` | Minta klarifikasi (klinis) | `RehabRequest : Disposition` | `RehabReasonRequest` | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/accept` | Terima dan buka episode | `RehabRequest : Disposition` | `AcceptRehabRequestRequest` (expectedVersion, responsibleDoctorId, note?) | `RehabRequestDetailResponse` (+episodeId) | Rencana (belum tersedia) |
| `POST` | `/{id}/redirect` | Alihkan | `RehabRequest : Disposition` | `RedirectRehabRequestRequest` (expectedVersion, reason, redirectTargetText) | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/reject` | Tolak secara klinis | `RehabRequest : Disposition` | `RehabReasonRequest` | `RehabRequestDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/cancel` | Batalkan | `RehabRequest : Cancel` | `RehabReasonRequest` | `RehabRequestDetailResponse` | Rencana (belum tersedia) |

`CreateRehabRequestRequest`: `patientId`, `sourceType`, `sourceEncounterId?`, `inpEpisodeId?`,
`internalReferralLetterId?`, `externalInstitutionName?`, `externalReferralNumber?`,
`requestingDoctorId?`, `requestingPractitionerName?`, `indicationText`, `requestedAt`,
`disciplineIds[]`. Server **mengabaikan** field lain (whitelist DTO). `RequestNumber` dan
status dibuat server.

`RehabRequestListItem` (tanpa kolom sensitif): `id`, `requestNumber`, `patientName`,
`medicalRecordNumber`, `sourceType`, `disciplineNames[]`, `requestedAt`,
`requestingDoctorName`, `requestStatus`, `version`.

---

### Health Services / Rehabilitation Management / Rehab Lookup

Base URL: `.../rehab-lookups` — seluruhnya `GET`, baca saja.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/patients` | Cari pasien (nama/RM) | `RehabLookup : Read` | `search` (≥ 3 karakter), `pageSize` ≤ 20 | `List<RehabPatientOption>` | Rencana (belum tersedia) |
| `GET` | `/encounters` | Kunjungan terbuka milik satu pasien | `RehabLookup : Read` | `patientId` (wajib), `encounterType?` | `List<RehabEncounterOption>` | Rencana (belum tersedia) |
| `GET` | `/internal-referral-letters` | Surat rujukan internal pasien | `RehabLookup : Read` | `patientId` (wajib) | `List<RehabReferralLetterOption>` | Rencana (belum tersedia) |
| `GET` | `/doctors` | Dokter aktif | `RehabLookup : Read` | `search?` | `List<RehabDoctorOption>` | Rencana (belum tersedia) |
| `GET` | `/disciplines` | Disiplin aktif | `RehabLookup : Read` | — | `List<RehabDisciplineOption>` | Rencana (belum tersedia) |
| `GET` | `/disciplines/{disciplineId}/procedures` | Tindakan yang boleh dipakai disiplin | `RehabLookup : Read` | — | `List<RehabProcedureOption>` | Rencana (belum tersedia) |
| `GET` | `/instruments` | Instrumen berlaku per disiplin/pasien | `RehabLookup : Read` | `patientId`, `disciplineId?` (null = medis) | `List<RehabResolvedInstrument>` | Rencana (belum tersedia) |

`/encounters` tanpa `patientId` ditolak dengan kode 400. Hasilnya hanya kunjungan milik pasien
itu yang belum berakhir (RHB-DEC-016).

---

### Health Services / Rehabilitation Management / Rehab Episode

Base URL: `.../rehab-episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Opsi filter | `RehabEpisode : Read` | — | `RehabEpisodeFilterMetadataResponse` | Rencana (belum tersedia) |
| `GET` | `/summary` | Jumlah per status | `RehabEpisode : Read` | `RehabEpisodeQuery` | `RehabEpisodeSummaryResponse` | Rencana (belum tersedia) |
| `GET` | `/` | Worklist episode | `RehabEpisode : Read` | `RehabEpisodeQuery` (startDate, endDate, customPeriod, status, disciplineId, responsibleDoctorId, needsReview, evaluationDue, search, page) | `PagedResult<RehabEpisodeListItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail: identitas pasien (read-only), permintaan, disiplin, ringkasan dokumen, aksi | `RehabEpisode : Read` | — | `RehabEpisodeDetailResponse` | Rencana (belum tersedia) |
| `GET` | `/{id}/timeline` | Urutan kronologis asesmen → program → sesi → reasesmen → ringkasan | `RehabEpisode : Read` | — | `List<RehabTimelineItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}/status-history` | Riwayat status | `RehabEpisode : Read` | — | `List<RehabStatusHistoryItem>` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Medical Assessment

Base URL: `.../rehab-medical-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Asesmen medis episode | `RehabMedicalAssessment : Read` | — | `List<RehabAssessmentListItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail + pengukuran + status keutuhan | `RehabMedicalAssessment : Read` | — | `RehabAssessmentDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}` | Buat draft | `RehabMedicalAssessment : Create` | `SaveRehabMedicalAssessmentRequest` (encounterId, isian, measurements[], prescribedDisciplines[{disciplineId, medicalPrecautionText?}]) | `RehabAssessmentDetailResponse` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah draft | `RehabMedicalAssessment : Update` | sama (+expectedVersion) | `RehabAssessmentDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/sign` | Tanda tangan → episode `Planning` | `RehabMedicalAssessment : Sign` | `RehabSignRequest` (expectedVersion) | `RehabAssessmentDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/cancel` | Batalkan draft | `RehabMedicalAssessment : Update` | `RehabReasonRequest` | `RehabAssessmentDetailResponse` | Rencana (belum tersedia) |

### Health Services / Rehabilitation Management / Rehab Discipline Assessment

Base URL: `.../rehab-discipline-assessments` — bentuk sama dengan asesmen medis, tetapi
memakai butir `RehabDisciplineAssessment : Read/Create/Update/Sign`. Request `create` wajib
memuat `disciplineId`, dan tidak memuat `prescribedDisciplines`. Seluruhnya **Rencana (belum
tersedia)**.

| Method | Path | Hak akses |
|---|---|---|
| `GET` | `/by-episode/{episodeId}` | `RehabDisciplineAssessment : Read` |
| `GET` | `/{id}` | `RehabDisciplineAssessment : Read` |
| `POST` | `/by-episode/{episodeId}` | `RehabDisciplineAssessment : Create` |
| `PUT` | `/{id}` | `RehabDisciplineAssessment : Update` |
| `POST` | `/{id}/sign` | `RehabDisciplineAssessment : Sign` |
| `POST` | `/{id}/cancel` | `RehabDisciplineAssessment : Update` |

---

### Health Services / Rehabilitation Management / Rehab Goal

Base URL: `.../rehab-goals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Tujuan episode (termasuk riwayat versi) | `RehabGoal : Read` | `disciplineId?`, `includeSuperseded?` | `List<RehabGoalItem>` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}` | Buat tujuan | `RehabGoal : Create` | `SaveRehabGoalRequest` | `RehabGoalItem` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah (bila belum dipakai program aktif) | `RehabGoal : Update` | sama (+expectedVersion) | `RehabGoalItem` | Rencana (belum tersedia) |
| `POST` | `/{id}/revise` | Versi baru | `RehabGoal : Update` | `ReviseRehabGoalRequest` (+reason) | `RehabGoalItem` | Rencana (belum tersedia) |
| `POST` | `/{id}/discontinue` | Hentikan | `RehabGoal : Update` | `RehabReasonRequest` | `RehabGoalItem` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Program

Base URL: `.../rehab-programs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Program per disiplin beserta versi | `RehabProgram : Read` | — | `List<RehabProgramListItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail | `RehabProgram : Read` | — | `RehabProgramDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}` | Draft v1 untuk satu disiplin | `RehabProgram : Create` | `SaveRehabProgramRequest` (disciplineId, authoredEncounterId, frequencyText, plannedSessionCount?, precautionText?, startDate, evaluationDueDate, interventions[{procedureId, parametersText}], goalIds[]) | `RehabProgramDetailResponse` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah draft | `RehabProgram : Update` | sama (+expectedVersion) | `RehabProgramDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/revise` | Buat versi baru dari program aktif | `RehabProgram : Create` | `ReviseRehabProgramRequest` (expectedVersion, modificationReason) | `RehabProgramDetailResponse` (201) | Rencana (belum tersedia) |
| `POST` | `/{id}/activate` | Tanda tangan + aktifkan | `RehabProgram : Activate` | `RehabSignRequest` | `RehabProgramDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/stop` | Hentikan program aktif | `RehabProgram : Stop` | `RehabReasonRequest` | `RehabProgramDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/cancel` | Batalkan draft | `RehabProgram : Update` | `RehabReasonRequest` | `RehabProgramDetailResponse` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Education

Base URL: `.../rehab-education-records`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Daftar edukasi/persetujuan | `RehabEducation : Read` | `programId?` | `List<RehabEducationItem>` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}` | Catat (draft) | `RehabEducation : Create` | `SaveRehabEducationRequest` | `RehabEducationItem` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah draft | `RehabEducation : Update` | sama | `RehabEducationItem` | Rencana (belum tersedia) |
| `POST` | `/{id}/sign` | Tanda tangan | `RehabEducation : Sign` | `RehabSignRequest` | `RehabEducationItem` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Session

Base URL: `.../rehab-sessions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Opsi filter | `RehabSession : Read` | — | `RehabSessionFilterMetadataResponse` | Rencana (belum tersedia) |
| `GET` | `/summary` | Jumlah per status hari/periode ini | `RehabSession : Read` | `RehabSessionQuery` | `RehabSessionSummaryResponse` | Rencana (belum tersedia) |
| `GET` | `/` | Worklist sesi | `RehabSession : Read` | `RehabSessionQuery` (startDate, endDate, customPeriod, status, recordStatus, disciplineId, therapistUserId, mine?, billingHandoffStatus, search, page) | `PagedResult<RehabSessionListItem>` | Rencana (belum tersedia) |
| `GET` | `/by-episode/{episodeId}` | Sesi episode | `RehabSession : Read` | `programId?` | `List<RehabSessionListItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail | `RehabSession : Read` | — | `RehabSessionDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/by-program/{programId}/start` | Cek keselamatan + mulai | `RehabSession : Create` | `StartRehabSessionRequest` (encounterId, inpEpisodeId?, identityVerified, programVerified, currentConditionText, contraindicationFound, contraindicationText?) | `RehabSessionDetailResponse` (201) | Rencana (belum tersedia) |
| `POST` | `/by-program/{programId}/not-done` | Catat sesi tidak terlaksana | `RehabSession : Create` | `RecordNotDoneRehabSessionRequest` (encounterId?, notDoneReason, note) | `RehabSessionDetailResponse` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah isi (intervensi aktual, respons, deviasi) | `RehabSession : Update` | `UpdateRehabSessionRequest` (+expectedVersion) | `RehabSessionDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/complete` | Selesai | `RehabSession : Update` | `RehabVersionRequest` | `RehabSessionDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/stop-early` | Henti dini/eskalasi | `RehabSession : Update` | `StopEarlyRehabSessionRequest` (expectedVersion, deviationType, deviationReasonText, immediateActionText, contactedPartyText?, needsProgramReview) | `RehabSessionDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/sign` | Tanda tangan → handoff billing | `RehabSession : Sign` | `RehabSignRequest` | `RehabSessionDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/cancel` | Batalkan (salah input) | `RehabSession : Update` | `RehabReasonRequest` | `RehabSessionDetailResponse` | Rencana (belum tersedia) |

### Health Services / Rehabilitation Management / Rehab Billing Handoff

Base URL: `.../rehab-sessions` — dilayani `RehabSessionBillingController`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id}/billing-handoff` | Status handoff per intervensi | `RehabBillingHandoff : Read` | — | `RehabBillingHandoffResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/billing-handoff/retry` | Ulang pengiriman (idempoten) | `RehabBillingHandoff : Retry` | — | `RehabBillingHandoffResponse` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Progress Review

Base URL: `.../rehab-progress-reviews`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Daftar reasesmen | `RehabProgressReview : Read` | — | `List<RehabProgressReviewItem>` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail + pembanding baseline | `RehabProgressReview : Read` | — | `RehabProgressReviewDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}` | Draft | `RehabProgressReview : Create` | `SaveRehabProgressReviewRequest` (disciplineId?, programId?, encounterId, measurements[], goalReviews[], decision, decisionReasonText, referralTargetText?, instrumentChangeNote?) | `RehabProgressReviewDetailResponse` (201) | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah draft | `RehabProgressReview : Update` | sama | `RehabProgressReviewDetailResponse` | Rencana (belum tersedia) |
| `POST` | `/{id}/sign` | Tanda tangan + efek keputusan | `RehabProgressReview : Sign` | `RehabSignRequest` | `RehabProgressReviewDetailResponse` | Rencana (belum tersedia) |

Reasesmen disiplin juga memeriksa profesi = disiplin. Reasesmen medis (`disciplineId = null`)
memakai butir `RehabProgressReview : Create`. Admin dapat memberikan butir itu hanya kepada
Sp.KFR.

---

### Health Services / Rehabilitation Management / Rehab Discharge Summary

Base URL: `.../rehab-discharge-summaries` — satu dokumen per episode (pola `GET`/`PUT` path tetap).

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-episode/{episodeId}` | Ringkasan (atau kosong) beserta daftar prasyarat yang belum final | `RehabDischargeSummary : Read` | — | `RehabDischargeSummaryResponse` | Rencana (belum tersedia) |
| `PUT` | `/by-episode/{episodeId}` | Simpan draft | `RehabDischargeSummary : Update` | `SaveRehabDischargeSummaryRequest` | `RehabDischargeSummaryResponse` | Rencana (belum tersedia) |
| `POST` | `/by-episode/{episodeId}/sign` | Tanda tangan → episode `Closed` | `RehabDischargeSummary : Sign` | `RehabSignRequest` | `RehabDischargeSummaryResponse` | Rencana (belum tersedia) |

---

### Health Services / Rehabilitation Management / Rehab Discipline

Base URL: `.../rehab-disciplines` — konfigurasi (bentuk master data, 9 endpoint baseline + sub-resource).

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Opsi filter | `RehabConfiguration : Read` | Rencana (belum tersedia) |
| `GET` | `/summary` | Jumlah aktif/tidak | `RehabConfiguration : Read` | Rencana (belum tersedia) |
| `GET` | `/` | Daftar | `RehabConfiguration : Read` | Rencana (belum tersedia) |
| `GET` | `/options` | Pilihan disiplin | `RehabConfiguration : Read` | Rencana (belum tersedia) |
| `GET` | `/{id}` | Detail + tindakan + instrumen | `RehabConfiguration : Read` | Rencana (belum tersedia) |
| `POST` | `/` | Tambah | `RehabConfiguration : Create` | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah nama/profesi | `RehabConfiguration : Update` | Rencana (belum tersedia) |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `RehabConfiguration : Update` | Rencana (belum tersedia) |
| `DELETE` | `/{id}` | Hapus (soft), ditolak bila sudah dipakai | `RehabConfiguration : Delete` | Rencana (belum tersedia) |
| `GET` / `PUT` | `/{id}/procedures` | Daftar tindakan yang boleh dipakai | `RehabConfiguration : Read` / `Update` | Rencana (belum tersedia) |
| `GET` / `PUT` | `/{id}/instruments` | Jenis instrumen yang ditawarkan | `RehabConfiguration : Read` / `Update` | Rencana (belum tersedia) |

`GET`/`PUT /medical-instruments` (disiplin `null`, asesmen medis) memakai butir yang sama.
Status: **Rencana (belum tersedia)**.

---

### Endpoint existing yang dipakai (as-is)

| Grup | Method | Path | Hak akses | Dipakai untuk |
|---|---|---|---|---|
| MedicalRecordManagement / ClinicalNoteAddendum | `GET`, `POST` | `api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | `ClinicalNoteAddendum : Read` / `Create` | Addendum dokumen rehab (`documentKind` 47–52) |
| ClinicalManagement / ClinicalInstrument | semua | `api/v1/health-services/clinical-management/clinical-instruments` | `ClinicalInstrumentConfiguration : *` | Komite memasukkan dan mengesahkan instrumen rehab |

## Kode status

| Kode | Arti bagi pengguna |
|---|---|
| 200 / 201 | Berhasil / data baru dibuat |
| 400 | Isian tidak lengkap atau formatnya salah |
| 401 | Sesi masuk habis |
| 403 | Tidak punya hak akses, bukan penulis catatan, atau profesi tidak sesuai |
| 404 | Data tidak ditemukan (termasuk data pasien lain) |
| 409 | Status tidak mengizinkan, data diubah orang lain, atau ada duplikasi |
| 422 | Aturan bisnis belum terpenuhi (pesan menyebut apa yang kurang) |
