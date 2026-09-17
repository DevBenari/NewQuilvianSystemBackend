# API Contract — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.5.0`** — bagian 7, `draft` |
| `last_changed_in` | **`0.5.0`** — penyelarasan `PRD-RWI-V2-001`: pengkajian berinstrumen, Pengawasan Harian, Evaluasi Awal, MAR, pelaksanaan sliding scale. Sebelumnya `0.4.0` — Gelombang 1A |
| Compatibility impact | `0.3.0`: dua endpoint `amend` berubah menjadi **penambahan addendum** sesuai `RWI-DEC-091`. Status dokumen **tidak lagi berpindah** ke `Amended`; nilai status itu dicabut. Endpoint rencana asuhan tidak berubah |
| Status | **`draft`** untuk `0.5.0`. `0.4.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | — belum |
| `input_revision` | `02-backend-architecture.md` `0.3`; `PRD-RWI-FINAL-001` v1.0.0; decision log `13` |
| Keputusan yang mengikat | `RWI-DEC-091`, `RWI-DEC-086`, `RWI-DEC-087`, `RWI-FACT-016` |
| Tanggal | 2 September 2026 |

---


## 0.A Perubahan pada `contract_version` `0.4.0` — Gelombang 1A

**Status `approved` sejak 11 September 2026** lewat `RWI-DEC-105`. Menyerap dua koreksi `P0` dari `PRD-to-MVP-Rawat-Inap-V2` yang menyentuh sub-modul ini.

### 0.A.1 Jalur hapus tanda vital ditutup — `RWI-DEC-098`

Tanda vital **bukan** grup endpoint milik kontrak ini, tetapi ia dibaca ruang kerja keperawatan
dan dicatat perawat. Karena itu perubahannya disebut di sini sebagai titik sentuh, dan grup
endpointnya tetap dimiliki `ClinicalManagement`.

| Method | Path | Keadaan sebelum `0.4.0` | Ketetapan `0.4.0` |
| --- | --- | --- | --- |
| `DELETE` | `/patient-vital-signs/{id}` | **Ada di source**, `PatientVitalSignController` baris 799. Soft delete tanpa pemeriksaan status, tanpa alasan, dan tanpa memeriksa penulis. Ia juga mematikan `NeedDoctorNotification`, sehingga pemberitahuan ke dokter ikut hilang diam-diam | **DIHAPUS.** Route tidak lagi tersedia; jawabannya `404` |

**Kenapa tanda vital lebih berbahaya daripada dokumen lain.** Tanda vital adalah dasar penilaian
perburukan pasien. Menghapus satu baris tanda vital tidak hanya menghilangkan angka; ia memutus
deret waktu yang dipakai perawat dan dokter untuk melihat kecenderungan. Baris yang hilang tidak
menyisakan lubang yang terlihat, sehingga grafik tetap tampak wajar.

| Keadaan dokumen | Jalur yang sah sejak `0.4.0` |
| --- | --- |
| Salah catat, belum final | Pembatalan beralasan, mengikuti pola `PATCH /{id}/cancel` yang sudah dipakai pengkajian dan tindakan |
| Sudah final atau terverifikasi | Addendum lewat mesin `MedicalRecordManagement` |

**Satu prasyarat yang belum terpenuhi dan wajib disebut.** `ClinicalDocumentKind.VitalSign`
bernomor `6`, tetapi **belum termasuk** jenis yang ditegakkan mesin keutuhan dokumen. Yang
ditegakkan hari ini hanya `ProgressNote`, `Consultation`, `Assessment`, dan `Procedure`. Selama
`VitalSign` belum masuk daftar itu, pembatalan beralasan **tidak dapat** memeriksa status final,
sehingga pengganti jalur hapus belum utuh. Keputusan menaikkannya milik pemilik
`MedicalRecordManagement` dan dilacak sebagai `V2-UNK-01` pada
[`../01-existing-capability-map.md`](../01-existing-capability-map.md) bagian 16.5.

> **Akibat yang harus diterima secara sadar.** Bila `VitalSign` tidak dinaikkan, menutup `DELETE`
> tetap menghilangkan cara menyembunyikan catatan, tetapi pembatalan draf belum terjaga terhadap
> dokumen yang sudah final. Itu tetap lebih baik daripada keadaan sekarang, dan bukan pengganti
> penegakan yang utuh.

### 0.A.2 Kewenangan menulis perawat ditentukan unit — `RWI-DEC-100`

| Yang berubah | Ketetapan `0.4.0` |
| --- | --- |
| Sumber identitas penulis | Diambil dari `ApplicationUser.EmployeeId` milik pengguna terautentikasi. `nurseId` pada request **tidak** menentukan penulis |
| Syarat boleh menulis | Perawat bertugas pada **unit tempat episode berada**. Penugasan per episode **bukan** syarat |
| Peran `InpNurseAssignment` | Tetap sebagai **penunjukan perawat penanggung jawab** yang tampil di kepala konteks pasien. Ia **tidak** mengunci hak tulis |
| Episode `Closed` atau `Cancelled` | Seluruh aksi tulis ditolak, sebagaimana sudah berlaku |

| Keadaan | Jawaban sejak `0.4.0` |
| --- | --- |
| Perawat bangsal menulis untuk pasien di unitnya, bukan pasien penanggung jawabnya | `200`. Ini jalur normal dinas malam |
| Perawat menulis untuk pasien di unit lain | `403` |
| Pengguna tanpa pemetaan pegawai menulis | `403` |
| Perawat mengirim `nurseId` milik perawat lain | `403` |

**Perbedaan yang disengaja terhadap gerbang dokter.** Gerbang perawat memang lebih longgar
daripada gerbang dokter pada `dokter-rawat-inap` `0.5.0`. Itu **bukan kelalaian**. DPJP melekat
pada pasien selama berhari-hari, sedangkan perawat berganti tiga shift sehari pada bangsal berisi
20 sampai 30 pasien, dan sistem **tidak mempunyai konsep shift perawat sama sekali**. Menuntut
penugasan per episode berarti membuat sekitar 90 baris penugasan manual per hari per bangsal.
Dasarnya `RWI-DEC-100`.

**Kemampuan yang belum ada dan menjadi pekerjaan baru.** `InpatientClinicalContextService` hari
ini **nol menyebut perawat**; satu-satunya pemeriksaan kewenangan yang tersedia adalah
`IsDoctorAssignedAsync`. Resolver perlu kemampuan baru untuk menilai unit tempat perawat bertugas.
Sumber data "unit tempat perawat bertugas" **belum ditetapkan**, karena `InpNurseAssignment` tidak
menyimpannya. Penetapannya adalah pekerjaan desain teknis pada task implementasi, dan **tidak
boleh** diselesaikan dengan menambahkan kolom unit ke `InpNurseAssignment`, karena itu melahirkan
sumber kebenaran kedua yang dapat berbeda dari unit episode saat pasien pindah. Dasarnya
`RWI-FACT-022` dan bagian 0.5 pada `02-backend-architecture.md` sub-modul `episode-rawat-inap`.

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

---

## 7. Perubahan pada `contract_version` `0.5.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| `input_revision` | `02-backend-architecture.md` `0.4` bagian 11; `data/data-dictionary.md` `0.4` bagian 11; decision log revision `21`; gate `1.6` |
| Keputusan yang mengikat | `RWI-DEC-100`, `108`, `113`, `115` s.d. `120`, `124`, `131`, `136`, `137`, `140`, `141`, `145` s.d. `149` |

Seluruh endpoint di bawah berlabel **Rencana (belum tersedia)** kecuali disebut lain. Base URL Clinical:
`api/v1/health-services/clinical-management`; base URL Pharmacy: `api/v1/health-services/pharmacy-management`.
Seluruh response dibungkus `ApiResponse<T>`. Seluruh jalur tulis rawat inap memakai penjaga konteks yang sama:
episode `Admitted` (`VAL-KEP-01`) dan perawat ditempatkan di unit episode (`VAL-KEP-05`, `RWI-DEC-100`).

### 7.0 Dampak kompatibilitas

| Perubahan | Pemakai lama terdampak | Sifat |
| --- | --- | --- |
| Tiga nilai `PatientAssessmentType` baru (`6`, `7`, `8`) | Klien yang memetakan enum secara tertutup | **Aditif**; frontend wajib menyamakan enum (`RLN3-CAP-18`, `29`) |
| Risiko jatuh rawat inap dihitung dari instrumen berversi | Pengkajian rawat inap yang mengirim `HasAtaxia`/`HasPosturalInstability` | **Perubahan perilaku** — kedua isian diabaikan pada jalur rawat inap V2; poliklinik dan IGD tidak berubah |
| `POST /patient-vital-signs` untuk encounter berepisode `Admitted` mengisi `InpEpisodeId` dan menolak isian nyeri | Klien rawat inap | Perubahan perilaku; `400` bila isian nyeri dikirim |
| Grup baru Clinical Instrument, Case Management Evaluation, Fluid Balance, Blood Glucose Reading, Daily Observation, Daily Monitoring, Nursing Shift, Medication Administration, Sliding Scale Execution, Medication Schedule Setting | — | Aditif |
| `POST /patient-allergies/from-medication-administration` | — | Aditif |

### 7.1 Health Services / Clinical Management / Patient Assessment — `CAP-012`, perubahan perilaku

Base URL: `api/v1/health-services/clinical-management/patient-assessments`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Assessment")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | **Perilaku baru** untuk jenis `Initial`, `Reassessment`, `FallRisk`, `PainMonitoring`, `EducationAssessment`, `DischargePlanning` pada episode rawat inap: menyimpan konsep, jawaban instrumen, dan hasil hitung server | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` + `InstrumentResponses[]` (`InstrumentVersionId`, `Responses` objek), `VitalSignId` (Kajian Umum), `PainAssessmentState` (Monitoring Nyeri) + `Idempotency-Key` | `ApiResponse<PatientAssessmentResponse>` beserta `InstrumentResults[]` (`TotalScore`, `BandCode`, `BandLabel`, `IsAlertBand`, `InstrumentName`, `VersionNumber`) | Sudah ada — **perilaku diperluas, Rencana** |
| `PUT` | `/{id}` | Menyimpan ulang konsep; skor dihitung ulang dari versi sah terbaru | `PatientAssessment : Update` | Sama dengan `POST` + `ExpectedUpdateDate` | Sama | Sudah ada — perilaku diperluas |
| `PATCH` | `/{id}/complete` | Menyelesaikan: isian wajib versi terisi; di produksi versi wajib `Approved` | `PatientAssessment : Update` | — | Sama | Sudah ada — perilaku diperluas |
| `GET` | `/episodes/{episodeId}` | Daftar per episode — **bentuk tetap berpaginasi** (`RLN3-CAP-17`); saringan `assessmentType` baru dapat dipakai | `PatientAssessment : Read` | Query `assessmentType`, `status`, `page`, `pageSize` | `ApiResponse<PagedResult<PatientAssessmentListItem>>` | Sudah ada |
| `GET` | `/episodes/{episodeId}/progress` | **Progres Pengkajian Pasien** lima bagian | `PatientAssessment : Read` | — | `ApiResponse<NursingAssessmentProgressResponse>` | **Rencana (belum tersedia)** |

**`NursingAssessmentProgressResponse`** — `Sections[]` berisi lima baris dengan urutan tetap:

| `SectionCode` | Label | Jenis dokumen yang dihitung |
| --- | --- | --- |
| `GENERAL` | Kajian Umum | `Initial` dan `Reassessment` |
| `FALL_RISK` | Resiko Jatuh | `FallRisk` |
| `PAIN` | Monitoring Nyeri | `PainMonitoring` |
| `EDUCATION` | Assesment Edukasi | `EducationAssessment` |
| `DISCHARGE_PLANNING` | Perencanaan Pulang | `DischargePlanning` |

`State` per bagian mengikuti `RWI-DEC-119`, dinilai dari keadaan dokumen, **bukan** temuan klinis:

| `State` | Tampil | Syarat |
| --- | --- | --- |
| `Completed` | ✓ Selesai | Sekurang-kurangnya satu dokumen bagian itu berstatus `Completed`, walaupun ada draft baru |
| `NeedsAttention` | ! Perlu perhatian | Ada draft dan belum ada dokumen `Completed` |
| `NotFilled` | ○ Belum diisi | Tidak ada dokumen apa pun yang tidak dibatalkan |

Setiap baris juga membawa `LastDocumentId`, `LastClinicalDateTime`, `LastAuthorName`, dan `ReassessmentOverdue`
(hanya `PAIN`). Response membawa `CompletedCount`, `TotalCount = 5`, `ProgressPercent = CompletedCount × 20`
(`RWI-DEC-120`), serta dua isian tampil yang **tidak** dihitung: `DailyMonitoringLastRecordedAt` ("terakhir dicatat
06.00") dan `CaseManagementEvaluationStatus` ("Belum diisi — milik MPP"). Temuan berisiko — misalnya pita Tinggi — tidak
masuk `State`; ia dibaca kepala konteks sebagai alert terpisah.

Contoh Budi hari ke-2: Kajian Umum ✓, Resiko Jatuh ✓ (kepala konteks "⚠ Risiko Jatuh Tinggi"), Monitoring Nyeri ! draft
skala 8, Assesment Edukasi ○, Perencanaan Pulang ○ → `CompletedCount = 2`, `ProgressPercent = 40`. Bila endpoint gagal,
layar menampilkan "Gagal memuat progres pengkajian" beserta Coba Lagi — **bukan** lima ○.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Isian tidak sah; pita instrumen tidak dapat dihitung dari jawaban; isian nyeri dikirim pada tanda vital |
| `403` | Anda tidak ditempatkan di unit pasien ini |
| `409` | Versi instrumen yang dipakai sudah diganti versi sah baru — muat ulang formulir; data sudah diubah pengguna lain |
| `422` | Instrumen belum disahkan — dokumen hanya dapat disimpan sebagai konsep; isian wajib belum lengkap; perawatan pasien sudah ditutup |

### 7.2 Health Services / Clinical Management / Clinical Instrument — grup baru

Base URL: `api/v1/health-services/clinical-management/clinical-instruments`
Judul grup: `[Tags("Health Services / Clinical Management / Clinical Instrument")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar instrumen dan formulir beserta versi sah | `ClinicalInstrumentConfiguration : Read` | Query `instrumentKind`, `isActive`, `page`, `pageSize` | `ApiResponse<PagedResult<ClinicalInstrumentListItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Instrumen beserta seluruh versi | `ClinicalInstrumentConfiguration : Read` | — | `ApiResponse<ClinicalInstrumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat instrumen | `ClinicalInstrumentConfiguration : Update` | `CreateClinicalInstrumentRequest` (`Code`, `Name`, `InstrumentKind`, `TargetMinAgeMonths`, `TargetMaxAgeMonths`, `Description`) | Sama | **Rencana (belum tersedia)** |
| `POST` | `/{id}/versions` | Membuat versi `Draft` baru, disalin dari versi terakhir bila ada | `ClinicalInstrumentConfiguration : Update` | `CreateClinicalInstrumentVersionRequest` (`Definition` objek) | `ApiResponse<ClinicalInstrumentVersionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/versions/{versionId}` | Mengubah definisi versi `Draft` | `ClinicalInstrumentConfiguration : Update` | `UpdateClinicalInstrumentVersionRequest` (`Definition`, `ExpectedDefinitionHash`) | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/versions/{versionId}/approve` | Mengesahkan; versi sah lama menjadi `Retired` dalam transaksi yang sama | `ClinicalInstrumentConfiguration : Approve` | `ApproveClinicalInstrumentVersionRequest` (`ApprovalNote`, `ExpectedDefinitionHash`) | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/versions/{versionId}/retire` | Memensiunkan tanpa pengganti | `ClinicalInstrumentConfiguration : Approve` | Alasan wajib | Sama | **Rencana (belum tersedia)** |
| `POST` | `/versions/{versionId}/score-preview` | Uji hitung tanpa menyimpan — untuk pengesah memeriksa pita | `ClinicalInstrumentConfiguration : Read` | `Responses` objek | `ApiResponse<InstrumentScoreResult>` | **Rencana (belum tersedia)** |
| `GET` | `/resolve` | Versi yang berlaku bagi satu pasien dan jenis dokumen — dipanggil formulir | `PatientAssessment : Read` | Query `instrumentKind`, `episodeId` | `ApiResponse<ResolvedInstrumentResponse>` (`VersionId`, `Definition`, `IsApproved`, `IsDraftAllowedInThisEnvironment`) | **Rencana (belum tersedia)** |

Contoh `/resolve`: Budi 67 tahun, `instrumentKind=FallRiskScale` → instrumen dengan rentang usia 216 bulan s.d. terbuka;
bila hanya ada draft dan lingkungan produksi, `IsApproved = false`, formulir menampilkan "Instrumen belum disahkan" dan
tombol Selesai nonaktif.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Pita bertumpuk atau berlubang; kode isian ganda; pengikatan ke kolom yang tidak diizinkan |
| `403` | Anda tidak berwenang mengubah atau mengesahkan konfigurasi klinis; pengesah sama dengan pengubah terakhir |
| `404` | Tidak ada instrumen untuk usia pasien ini |
| `409` | Versi sudah disahkan dan tidak dapat diubah; definisi sudah diubah orang lain; rentang usia bertumpuk dengan instrumen aktif lain |

### 7.3 Health Services / Clinical Management / Case Management Evaluation — grup baru — Evaluasi Awal MPP

Base URL: `api/v1/health-services/clinical-management/case-management-evaluations`
Judul grup: `[Tags("Health Services / Clinical Management / Case Management Evaluation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Evaluasi Awal episode — dibaca seluruh pemegang hak baca, termasuk perawat | `CaseManagementEvaluation : Read` | — | `ApiResponse<CaseManagementEvaluationResponse?>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta delapan bagian dan addendum | `CaseManagementEvaluation : Read` | — | `ApiResponse<CaseManagementEvaluationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat konsep | `CaseManagementEvaluation : Create` | `CreateCaseManagementEvaluationRequest` (`EpisodeId`, `ClinicalDateTime`, `InstrumentVersionId`, `Responses`) + `Idempotency-Key` | Sama | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Menyimpan ulang konsep | `CaseManagementEvaluation : Update` | Sama + `ExpectedUpdateDate` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/complete` | Menyelesaikan | `CaseManagementEvaluation : Update` | — | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan konsep | `CaseManagementEvaluation : Update` | Alasan wajib | Sama | **Rencana (belum tersedia)** |
| `POST` | `/{id}/addendums` | Addendum setelah selesai | `CaseManagementEvaluation : Amend` | `CreateClinicalAddendumRequest` | `ApiResponse<ClinicalAddendumResponse>` | **Rencana (belum tersedia)** — bergantung `INT-KEP-12` |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `403` | Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini |
| `409` | Episode ini sudah punya Evaluasi Awal — lengkapi lewat addendum |
| `422` | Checklist Evaluasi Awal belum disahkan; perawatan pasien sudah ditutup |

### 7.4 Health Services / Clinical Management / Patient Vital Sign — perubahan perilaku dan satu endpoint baru

Base URL: `api/v1/health-services/clinical-management/patient-vital-signs`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Vital Sign")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | **Perilaku baru** pada encounter berepisode `Admitted`: `InpEpisodeId` diisi server, `VitalSignSource = InpatientObservation`, isian nyeri ditolak | `PatientVitalSign : Create` | `CreatePatientVitalSignRequest` yang ada + `Idempotency-Key` | `ApiResponse<PatientVitalSignResponse>` | Sudah ada — perilaku diperluas |
| `GET` | `/episodes/{episodeId}` | Deret tanda vital satu episode untuk tabel dan grafik | `PatientVitalSign : Read` | Query `from`, `to` (bawaan 24 jam terakhir, maksimum 7 hari) | `ApiResponse<List<PatientVitalSignSeriesItem>>` | **Rencana (belum tersedia)** |

### 7.5 Health Services / Clinical Management / Daily Monitoring — grup baru — ringkasan Pengawasan Harian

Base URL: `api/v1/health-services/clinical-management/daily-monitoring`
Judul grup: `[Tags("Health Services / Clinical Management / Daily Monitoring")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/summary` | Satu hari Pengawasan Harian: tanda vital, nyeri terakhir dari Monitoring Nyeri, GDS, cairan per shift dan 24 jam, diet dan mobilisasi, dosis `Administered` tanpa entri intake | `DailyObservation : Read` | Query `date` (`yyyy-MM-dd`, bawaan hari ini zona `Asia/Jakarta`) | `ApiResponse<DailyMonitoringSummaryResponse>` | **Rencana (belum tersedia)** |

**`DailyMonitoringSummaryResponse`:** `Date`; `VitalSigns[]`; `LatestPain` (`AssessmentId`, `PainScale`,
`PainAssessmentState`, `ClinicalDateTime`) atau `null`; `GlucoseReadings[]`; `FluidTotals` (`Shifts[]` berisi
`ShiftCode`, `IntakeMl`, `OutputMl`, `BalanceMl`; `Day` dengan bentuk sama; `ShiftConfigurationMissing` bila tanpa shift);
`FluidEntries[]`; `Observations[]`; `AdministeredDosesWithoutIntake[]` (`AdministrationId`, `DrugName`, `AdministeredAt`).
Hari dihitung dari **07.00 sampai 07.00 esok** bila shift pertama unit dimulai 07.00; bila tanpa shift, 00.00–24.00.

### 7.6 Health Services / Clinical Management / Fluid Balance — grup baru

Base URL: `api/v1/health-services/clinical-management/fluid-balance-entries`
Judul grup: `[Tags("Health Services / Clinical Management / Fluid Balance")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Entri satu rentang waktu | `FluidBalance : Read` | Query `from`, `to`, `includeCancelled` | `ApiResponse<List<FluidBalanceEntryResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat entri | `FluidBalance : Create` | `CreateFluidBalanceEntryRequest` (`EpisodeId`, `Direction`, `SourceCategory`, `SourceDetail`, `VolumeMl`, `EntryDateTime`, `MedicationAdministrationId` untuk sumber obat) + `Idempotency-Key` | `ApiResponse<FluidBalanceEntryResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/correct` | Koreksi — nilai lama disimpan sebagai revisi | `FluidBalance : Update` | `CorrectFluidBalanceEntryRequest` (`VolumeMl`, `EntryDateTime`, `SourceCategory`, `SourceDetail`, `CorrectionReason` wajib, `ExpectedRevisionNumber`) | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan entri | `FluidBalance : Update` | Alasan wajib, `ExpectedRevisionNumber` | Sama | **Rencana (belum tersedia)** |
| `GET` | `/{id}/revisions` | Riwayat koreksi | `FluidBalance : Read` | — | `ApiResponse<List<FluidBalanceEntryRevisionResponse>>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Volume harus lebih dari 0 dan paling banyak 10.000 ml; waktu di masa depan; sumber tidak sesuai arah; alasan koreksi kosong |
| `403` | Anda tidak ditempatkan di unit pasien ini |
| `409` | Dosis ini sudah punya entri intake; dosis belum diberikan; data sudah diubah pengguna lain |
| `422` | Perawatan pasien sudah ditutup; dosis milik pasien lain |

### 7.7 Health Services / Clinical Management / Blood Glucose Reading — grup baru

Base URL: `api/v1/health-services/clinical-management/blood-glucose-readings`
Judul grup: `[Tags("Health Services / Clinical Management / Blood Glucose Reading")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | GDS bangsal satu rentang; tiap baris membawa `UsedBySlidingScaleExecutionId` | `BloodGlucose : Read` | Query `from`, `to` | `ApiResponse<List<BloodGlucoseReadingResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat GDS dari Pengawasan Harian | `BloodGlucose : Create` | `CreateBloodGlucoseReadingRequest` (`EpisodeId`, `MeasuredAt`, `GlucoseValue`, `GlucoseUnit` wajib) + `Idempotency-Key` | `ApiResponse<BloodGlucoseReadingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/correct` | Koreksi; pelaksanaan sliding scale yang memakainya ditandai | `BloodGlucose : Update` | `GlucoseValue`, `GlucoseUnit`, `MeasuredAt`, `CorrectionReason` wajib, `ExpectedRevisionNumber` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan GDS yang **belum** dipakai pelaksanaan | `BloodGlucose : Update` | Alasan wajib | Sama | **Rencana (belum tersedia)** |
| `GET` | `/{id}/revisions` | Riwayat koreksi | `BloodGlucose : Read` | — | `ApiResponse<List<BloodGlucoseReadingRevisionResponse>>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Satuan wajib dipilih; nilai di luar batas yang mungkin untuk satuan itu |
| `409` | GDS sudah dipakai menghitung dosis insulin — koreksi, jangan batalkan |

### 7.8 Health Services / Clinical Management / Daily Observation — grup baru

Base URL: `api/v1/health-services/clinical-management/daily-observations`
Judul grup: `[Tags("Health Services / Clinical Management / Daily Observation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Observasi satu rentang | `DailyObservation : Read` | Query `from`, `to` | `ApiResponse<List<DailyObservationResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat diet, mobilisasi, lingkar perut, agitasi | `DailyObservation : Create` | `CreateDailyObservationRequest` + `Idempotency-Key` | `ApiResponse<DailyObservationResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/correct` | Koreksi | `DailyObservation : Update` | Isian + `CorrectionReason` + `ExpectedRevisionNumber` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan | `DailyObservation : Update` | Alasan wajib | Sama | **Rencana (belum tersedia)** |

### 7.9 Health Services / Clinical Management / Nursing Shift — grup baru — konfigurasi

Base URL: `api/v1/health-services/clinical-management/nursing-shifts`
Judul grup: `[Tags("Health Services / Clinical Management / Nursing Shift")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Shift bawaan dan shift per unit | `NursingShift : Read` | Query `serviceUnitId` | `ApiResponse<NursingShiftSetResponse>` (`ServiceUnitId`, `IsDefault`, `Shifts[]`) | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengganti seluruh shift satu unit atau bawaan sekaligus | `NursingShift : Update` | `ReplaceNursingShiftSetRequest` (`ServiceUnitId` atau `null`, `Shifts[]` berisi `ShiftCode`, `ShiftName`, `StartTime`, `EndTime`, `SortOrder`) | Sama | **Rencana (belum tersedia)** |

`400` bila shift tidak menutup 24 jam atau bertumpuk. Contoh ditolak: Pagi 07–14, Siang 14–20, Malam 21–07 → celah 20.00–21.00.

### 7.10 Health Services / Clinical Management / Patient Allergy — satu endpoint baru

Base URL: `api/v1/health-services/clinical-management/patient-allergies`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Allergy")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/from-medication-administration` | Mencatat dugaan reaksi obat dari satu dosis MAR | `PatientAllergy : Create` | `CreateSuspectedAdverseDrugReactionRequest` (`MedicationAdministrationId`, `ReactionDescription` wajib, `Severity`, `ReactionOnsetAt`, `ClinicalNote`) + `Idempotency-Key` | `ApiResponse<PatientAllergyResponse>` | **Rencana (belum tersedia)** |

`409` bila dosis bukan `Administered`; `422` bila dosis milik episode yang sudah ditutup.

### 7.11 Health Services / Pharmacy Management / Medication Administration — grup baru — MAR `CAP-023-MAR`

Base URL: `api/v1/health-services/pharmacy-management/medication-administrations`
Judul grup: `[Tags("Health Services / Pharmacy Management / Medication Administration")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | MAR satu hari: butir resep aktif sebagai baris, dosis sebagai sel. Memanggil pembentukan dosis sebelum membaca | `MedicationAdministration : Read` | Query `date`, `includeStopped` | `ApiResponse<MedicationAdministrationChartResponse>` (`Items[]` berisi butir, `IsStopped`, `IsHighAlert`, `IsAsNeeded`, `DoseKind`, `ScheduleConfigured`, `Doses[]`) | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail dosis | `MedicationAdministration : Read` | — | `ApiResponse<MedicationAdministrationResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/record` | Mencatat hasil dosis `Due` | `MedicationAdministration : Create` | `RecordMedicationAdministrationRequest` (`DoseStatus` `Administered`/`Held`/`Refused`/`Missed`, `ActualDose`, `ActualRoute`, `AdministeredAt`, `StatusReason`, `DeviationNote`, `ExpectedRevisionNumber`) + `Idempotency-Key` | Sama. Untuk high-alert berstatus `Administered` → `DoubleCheckStatus = Pending`, `DoseStatus` tetap `Due` | **Rencana (belum tersedia)** |
| `POST` | `/as-needed` | Mencatat pemberian PRN | `MedicationAdministration : Create` | `RecordAsNeededAdministrationRequest` (`PrescriptionItemId`, `ActualDose`, `ActualRoute`, `AdministeredAt`, `PrnIndication` wajib) + `Idempotency-Key` | Sama | **Rencana (belum tersedia)** |
| `POST` | `/unscheduled` | Mencatat pemberian butir berfrekuensi yang jadwalnya **belum dikonfigurasi** | `MedicationAdministration : Create` | Sama dengan `/as-needed`, `PrnIndication` diganti `UnscheduledReason` wajib | Sama; `DoseSource = AsNeeded`, alasan disimpan pada `DeviationNote` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/double-check` | Perawat kedua mengonfirmasi atau menolak | `MedicationAdministration : DoubleCheck` | `DoubleCheckRequest` (`Decision` `Confirm`/`Reject`, `Note` wajib bila `Reject`, `ExpectedRevisionNumber`) | Sama | **Rencana (belum tersedia)** |
| `GET` | `/double-check-worklist` | Dosis menunggu cek ganda di unit pengguna | `MedicationAdministration : DoubleCheck` | Query `serviceUnitId` | `ApiResponse<List<MedicationAdministrationListItem>>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/correct` | Koreksi hasil yang sudah tercatat | `MedicationAdministration : Update` | `CorrectMedicationAdministrationRequest` (`DoseStatus`, `ActualDose`, `ActualRoute`, `AdministeredAt`, `StatusReason`, `CorrectionReason` wajib, `ExpectedRevisionNumber`) | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/prn-evaluation` | Evaluasi efek obat PRN | `MedicationAdministration : Update` | `PrnEvaluationNote` wajib | Sama | **Rencana (belum tersedia)** |
| `GET` | `/{id}/revisions` | Riwayat koreksi | `MedicationAdministration : Read` | — | `ApiResponse<List<MedicationAdministrationRevisionResponse>>` | **Rencana (belum tersedia)** |

Contoh `record` ditolak: Ceftriaxone 08.00 dicatat `Administered` 2 g padahal resep 1 g tanpa `DeviationNote` → `400`
"Dosis berbeda dari resep, isi catatan penyimpangan".

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Dosis, rute, atau waktu pemberian kosong; alasan kosong untuk ditahan/ditolak/terlewat; catatan penyimpangan kosong; indikasi PRN kosong |
| `403` | Anda tidak ditempatkan di unit pasien ini; pemeriksa kedua tidak boleh orang yang sama dengan pencatat |
| `409` | Dosis sudah dicatat; butir resep sudah dihentikan; data sudah diubah pengguna lain; butir bukan obat sesuai kebutuhan; butir sliding scale hanya dicatat lewat pelaksanaan sliding scale |
| `422` | Perawatan pasien sudah ditutup |

### 7.12 Health Services / Pharmacy Management / Sliding Scale Execution — grup baru

Base URL: `api/v1/health-services/pharmacy-management/sliding-scale-executions`
Judul grup: `[Tags("Health Services / Pharmacy Management / Sliding Scale Execution")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/preview` | Menampilkan rentang dan dosis hitung **tanpa menyimpan** — perawat melihat sebelum memberi | `SlidingScaleExecution : Read` | `PreviewSlidingScaleExecutionRequest` (`OrderId`, `GlucoseValue` + `GlucoseUnit`, **atau** `BloodGlucoseReadingId`) | `ApiResponse<SlidingScalePreviewResponse>` (`OrderVersionNumber`, `MatchedRange`, `ComputedDoseUnits`, `InstructionText`, `IsHighAlert`) | **Rencana (belum tersedia)** |
| `POST` | `/` | Mencatat pelaksanaan: GDS, dosis MAR, dan pelaksanaan dalam **satu transaksi** | `SlidingScaleExecution : Create` **dan** `MedicationAdministration : Create` diperiksa service | `CreateSlidingScaleExecutionRequest` (`OrderId`, `ExpectedOrderVersionNumber`, `NewReading` {`MeasuredAt`, `GlucoseValue`, `GlucoseUnit`} **atau** `BloodGlucoseReadingId`, `ActualDoseUnits`, `AdministeredAt`, `ActualRoute`, `ExceptionReason` bila dosis aktual ≠ hitung, `DoseSlotAdministrationId` opsional) + `Idempotency-Key` **wajib** | `ApiResponse<SlidingScaleExecutionResponse>` beserta dosis MAR | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Riwayat pelaksanaan: jam, GDS, rentang, dosis hitung, dosis aktual, pelaksana, tanda koreksi GDS | `SlidingScaleExecution : Read` | Query `from`, `to` | `ApiResponse<List<SlidingScaleExecutionListItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail | `SlidingScaleExecution : Read` | — | `ApiResponse<SlidingScaleExecutionResponse>` | **Rencana (belum tersedia)** |

Contoh: Budi, order v2 disetujui. 11.00 perawat mengetik GDS 280 mg/dL → preview `[250, 300)` 3 unit → simpan → satu GDS,
satu dosis MAR 3 unit, satu pelaksanaan. Insulin high-alert → dosis `Pending` cek ganda.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Satuan GDS wajib dipilih; dosis berbeda dari hitungan tanpa alasan; kunci pengiriman kosong |
| `403` | Anda tidak ditempatkan di unit pasien ini |
| `409` | Order sudah dihentikan (`VAL-KEP-27`); order sudah disesuaikan dokter — muat ulang (`ExpectedOrderVersionNumber` basi); GDS sudah dipakai pelaksanaan lain; satuan GDS berbeda dari satuan protokol |
| `422` | GDS yang dipilih bukan GDS bangsal (`VAL-KEP-28`); perawatan pasien sudah ditutup |

### 7.13 Health Services / Pharmacy Management / Medication Schedule Setting — grup baru — konfigurasi

Base URL: `api/v1/health-services/pharmacy-management/medication-schedule-settings`
Judul grup: `[Tags("Health Services / Pharmacy Management / Medication Schedule Setting")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/schedule-times` | Jam standar per kode frekuensi | `MedicationScheduleSetting : Read` | Query `serviceUnitId`, `frequencyCode` | `ApiResponse<List<MedicationScheduleTimeSetResponse>>` | **Rencana (belum tersedia)** |
| `PUT` | `/schedule-times` | Mengganti slot satu kode frekuensi untuk satu unit atau bawaan | `MedicationScheduleSetting : Update` | `ReplaceMedicationScheduleTimesRequest` (`FrequencyCode`, `ServiceUnitId` atau `null`, `Times[]`) | Sama | **Rencana (belum tersedia)** |
| `GET` | `/frequency-codes-without-schedule` | Kode frekuensi yang dipakai resep aktif tetapi belum punya jadwal | `MedicationScheduleSetting : Read` | — | `ApiResponse<List<string>>` | **Rencana (belum tersedia)** |
| `GET` | `/administration-setting` | Pengaturan MAR | `MedicationScheduleSetting : Read` | — | `ApiResponse<MedicationAdministrationSettingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/administration-setting` | Mengubah pengaturan MAR | `MedicationScheduleSetting : Update` | `DoseGenerationHorizonHours`, `MissedAfterMinutes`, `PrnEvaluationMinutes` | Sama | **Rencana (belum tersedia)** |

Mengubah jadwal **tidak** memindahkan dosis `Due` yang sudah terbentuk; berlaku bagi dosis yang dibentuk sesudahnya.

### 7.14 Tagihan Pasien — kontrak diminta, belum ada

| Hal | Isinya |
| --- | --- |
| Diminta dari | Pemilik `BillingManagement` — `RWI-DEC-137` |
| Bentuk yang diminta | Ringkasan satu episode, **baca-saja**, sesuai `RWI-DEC-137` butir (1): penjamin dan status kelayakan keuangan, total tagihan berjalan, deposit, sisa atau kekurangan, jumlah item yang tidak ditanggung penjamin. **Tanpa** harga per item, aksi ubah, posting, maupun konfirmasi pembayaran. Contoh "BPJS — layak — total berjalan Rp 4.250.000 — deposit Rp 1.000.000 — kekurangan deposit Rp 350.000 — 1 item tidak ditanggung" |
| Butir hak akses | **`PatientBillingSummary : Read`**, Resource milik `BillingManagement` — nama ditetapkan desain ini sesuai `RWI-DEC-137` butir (3); tidak otomatis dimiliki semua perawat. Tanpa hak → layar "Anda tidak punya akses", permintaan langsung `403` |
| Kegagalan Billing | Layar menampilkan kegagalan apa adanya — **bukan** angka nol |
| Selama belum ada | Menu Tagihan Pasien tampil "Integrasi belum tersedia", **tanpa** angka tiruan — `INT-KEP-14` |

### 7.15 Endpoint sub-modul lain yang dipakai ruang kerja keperawatan pada `0.5.0`

| Endpoint | Pemilik kontrak | Dipakai untuk |
| --- | --- | --- |
| `POST /patient-integrated-progress-notes` dengan `NoteKind` `NursingSoap`/`NursingNarrative` | `dokter-rawat-inap` `0.6.0` bagian 12.4 | Asuhan Keperawatan → SOAP dan Catatan Keperawatan — `RWI-DEC-140` |
| `POST /patient-procedures/inpatient-orders` | `dokter-rawat-inap` bagian 12.5 | Menu Tindakan — perawat memesan atas instruksi |
| `POST /medication-reconciliations` | `dokter-rawat-inap` bagian 12.8 | Perawat mencatat obat bawaan |
| `GET /prescriptions/episodes/{episodeId}?period=` | `dokter-rawat-inap` bagian 12.6 | Resep Harian hanya baca |
| Pesanan laboratorium dan radiologi dengan `InstructingDoctorId` | `dokter-rawat-inap` bagian 12.12 | Menu Penunjang Medis |
| `GET/POST` perpindahan tempat tidur | `episode-rawat-inap` | Menu Transfer Pasien |
| `GET episodes/{id}` beserta alert klinis | `episode-rawat-inap` | Kepala konteks |

### 7.16 Yang tidak ada di kontrak `0.5.0`

| Tidak ada | Alasan |
| --- | --- |
| Endpoint handover shift dan transfusi | `DEFERRED` — `RWI-DEC-145` |
| Endpoint pemakaian alat, pemesanan kamar operasi | `DEFERRED` — `RWI-DEC-089`, `108` |
| Endpoint Gizi, Hemodialisa, Bank Darah, Rehab Medik | `RWI-DEC-113`; permukaan "Integrasi belum tersedia" |
| `DELETE` untuk entri klinis apa pun | Koreksi dan pembatalan beralasan — `RWI-DEC-098` |
| Jalur tulis GDS dari hasil laboratorium | `RWI-DEC-148` |
| Endpoint menandai dosis `Missed` otomatis | `AC-MVP-027` |
