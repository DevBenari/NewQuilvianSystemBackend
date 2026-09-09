# API Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | `0.4.0` |
| `last_changed_in` | `0.4.0` |
| Status | **`draft`** — amendment 9 September 2026, menunggu approval pemilik |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`, `MedicalRecordManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | `0.3.0` disetujui **Muhammad Hamzah** / **2026-09-03**. `0.4.0` **belum disetujui** |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2`; `PRD-RWI-FINAL-001` v1.0.0 |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3` |
| Compatibility impact | **Tidak ada endpoint yang dihapus atau berubah bentuknya.** `0.4.0` mendaftarkan grup **Patient Diagnosis** yang selama ini terlewat, dan membuka satu jalur baru: diagnosis terstruktur boleh lahir dari kajian medis tanpa nomor konsultasi. **Permintaan lama tetap sah apa adanya** — nomor konsultasi yang dikirim tetap diterima dan tetap diperlakukan sama. Perilaku rawat jalan dan medical check-up tidak berubah — `RWI-AC-143` |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |

---

## 0. Batas dokumen ini

**Tidak satu pun endpoint di bawah dimiliki modul Rawat Inap.** Dokumen ini menyatakan apa yang
dibutuhkan ruang kerja dokter rawat inap dari modul-modul pemiliknya.

Kolom `Hak akses` adalah **satu-satunya** tempat pemetaan endpoint ke hak akses hidup;
[`permission-audit-matrix.md`](./permission-audit-matrix.md) **tidak** mendaftarnya ulang.

### 0.1 Yang berubah dari `0.1.0`

| No | Perubahan | Alasan |
| ---: | --- | --- |
| 1 | Grup **Radiologi** ditambahkan | Modulnya terbukti ada pada `BE@93b3227` |
| 2 | Tiga endpoint `PATCH /{id}/amend` **dicabut** | Mekanismenya **sudah ada**: `POST /clinical-note-addendums/by-document/{documentKind}/{documentId}` |
| 3 | `PATCH /physician-visits/{id}` yang menyunting waktu dan peran **dicabut**, diganti `PATCH /{id}/cancel` dan `PATCH /{id}/links` | `RWI-DEC-085`: koreksi berbentuk batal lalu catat ulang |
| 4 | Penyaring kunjungan pada daftar pesanan laboratorium ditambahkan | `INV-DOK-12` tidak dapat ditegakkan tanpanya |
| 5 | Catatan perbaikan jalur tanpa antrean ditambahkan pada grup Konsultasi | `DOK-TRC-DEF-01` |

### 0.2 Yang berubah dari `0.2.0`

| No | Perubahan | Alasan |
| ---: | --- | --- |
| 1 | Memfinalkan catatan **sekaligus mendaftarkannya** ke mesin keutuhan sebagai dokumen tertanda tangan | `RWI-DEC-086`, `RWI-DEC-087` |
| 2 | Endpoint koreksi **atas nama penulis lain** ditambahkan pada bagian 9 | Sudah ada di source, terlewat pada `0.2.0` |
| 3 | Grup **penetapan penulis pengganti** ditambahkan sebagai bagian 9.1 | `RWI-DEC-088` menetapkan penerbitnya kepala unit rawat inap |

### 0.3 Yang berubah dari `0.3.0`

| No | Perubahan | Alasan |
| ---: | --- | --- |
| 1 | Grup **Patient Diagnosis** didaftarkan sebagai bagian 2.1 | Grupnya **sudah ada di source** dan sudah dibaca layar kajian medis, tetapi tidak pernah tercatat pada kontrak mana pun. Kelengkapan yang terlewat, sejenis dengan butir 2 pada `0.2.0` |
| 2 | Satu jalur baru: diagnosis terstruktur boleh menyebut **perawatan rawat inap** sebagai konteks, tanpa nomor konsultasi | `PRD-RWI-FINAL-001` `CAP-022` aturan 2 dan aturan 5; temuan `FE-RWI-044`; menutup penghalang `BE-RWI-068` |
| 3 | `INT-DOK-10` lahir pada `integration-contract.md` | Pelonggaran ini mengubah perilaku tabel milik `ClinicalManagement`, dan perubahan sejenis selalu punya entri integrasinya sendiri — preseden `INT-DOK-02` |

> **Butir 2 membalik satu baris yang sebelumnya sengaja ditulis menolak.**
> [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 9 mencantumkan
> "melonggarkan `ConsultationId` pada resep, tindakan, dan diagnosis" sebagai hal yang **tidak
> dibuat**, dengan alasan "yang perlu dibuka adalah konsultasinya". Alasan itu **benar untuk resep
> dan tindakan**, dan tetap dipertahankan bagi keduanya. Ia **tidak cukup** bagi diagnosis, karena
> satu fakta yang belum diketahui saat baris itu ditulis: kajian medis awal adalah **layar dan
> dokumen tersendiri** yang lahir sebelum catatan harian pertama, sedangkan `CAP-022` aturan 2
> menuntut daftar masalah menjadi bagian kajian itu dan aturan 5 menuntutnya berbentuk objek
> terstruktur, bukan teks. Rinciannya pada bagian 2.1.

---

## 1. Health Services / Clinical Management / Doctor Consultation — `CAP-020`

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`
Judul grup: `[Tags("Health Services / Clinical Management / Doctor Consultation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter beserta SOAP. **Perubahan:** menerima `InpEpisodeId`, `ClinicalDateTime`, dan `PhysicianVisitId`; tidak menuntut `QueueId` bila episodenya berjalan | `DoctorConsultation : Create` | `CreateDoctorConsultationRequest` **+ 3 field** | `ApiResponse<DoctorConsultationResponse>` | **Tersedia**, perilaku rawat inap **Rencana** |
| `POST` | `/` | **Catatan kedua dan seterusnya** pada satu kunjungan rawat inap | Sama | Sama | Sama | **Rencana** — hari ini ditolak batas satu konsultasi per kunjungan |
| `PATCH` | `/{id}/soap` | Menyimpan otomatis isi SOAP | `DoctorConsultation : Update` | `UpdateDoctorConsultationSoapRequest` | `ApiResponse<DoctorConsultationSoapUpdateResponse>` | **Tersedia** |
| `PATCH` | `/{id}/complete` | Memfinalkan catatan. **Perubahan:** sekaligus mendaftarkan catatan ke mesin keutuhan sebagai dokumen tertanda tangan, dalam transaksi yang sama | `DoctorConsultation : Update` | — | `ApiResponse<DoctorConsultationResponse>` | **Tersedia**, pendaftaran keutuhan **Rencana** |
| `PATCH` | `/{id}/cancel` | Membatalkan catatan yang belum final | `DoctorConsultation : Update` | Alasan | `ApiResponse<DoctorConsultationResponse>` | **Tersedia** |
| `GET` | `/episodes/{episodeId}/soap-timeline` | Lini masa catatan satu episode, terurut **waktu klinis** | `DoctorConsultation : Read` | Query `from`, `to` | `ApiResponse<SoapTimelineResponse>` | **Rencana (belum tersedia)** |

### 1.1 Perbaikan yang wajib menyertai grup ini

| Hal | Isinya |
| --- | --- |
| Apa | `POST /` pada cabang **tanpa antrean** hari ini berujung kegagalan sistem karena data antrean yang kosong tetap ditulis |
| Bukti | `DoctorConsultationController.cs` baris 258–265 dan 360–366 pada `BE@93b3227` |
| Yang terkena | Pasien rawat inap **dan** pasien IGD |
| Status | `Repair` — wajib selesai sebelum cabang episode dinyalakan |

### 1.2 Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` / `201` | Catatan tersimpan |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `403` | Anda bukan dokter yang berwenang atas pasien ini |
| `409` | Catatan sudah final. Untuk mengubahnya pakai addendum |
| `422` | Pasien tidak sedang dirawat inap, atau perawatannya sudah ditutup |
| `500` | Sistem gagal memproses. **Inilah yang terjadi hari ini pada jalur tanpa antrean** |

---

## 2. Health Services / Clinical Management / Patient Assessment — `CAP-022`

Base URL: `api/v1/health-services/clinical-management/patient-assessments`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Assessment")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat **kajian medis** dengan jenis `MedicalInitial` | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` + `AssessmentType` | `ApiResponse<PatientAssessmentResponse>` | **Tersedia**, jenis medis **Rencana** |
| `GET` | `/active-by-encounter/{encounterId}` | Membaca kajian aktif satu kunjungan | `PatientAssessment : Read` | — | `ApiResponse<PatientAssessmentResponse>` | **Tersedia** |
| `GET` | `/episodes/{episodeId}` | Membaca kajian satu episode, dapat disaring jenis | `PatientAssessment : Read` | Query `assessmentType` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian. **Perubahan:** sekaligus mendaftarkan kajian ke mesin keutuhan | `PatientAssessment : Update` | — | `ApiResponse<...>` | **Tersedia**, pendaftaran keutuhan **Rencana** |

> Grup ini **dibagi** dengan sub-modul `keperawatan`. Pembedanya `AssessmentType`, dan kewenangan
> menulisnya bercabang menurut jenis — `validation-matrix.md` `VAL-DOK-05`.

### 2.1 Health Services / Clinical Management / Patient Diagnosis — `CAP-022` aturan 2 dan 5 ★ grup baru pada `0.4.0`

Base URL: `api/v1/health-services/clinical-management/patient-diagnoses`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Diagnosis")]`

**Grup ini bukan endpoint baru.** Kesepuluh endpoint di bawah sudah berjalan di source sejak sebelum
sub-modul ini dirancang, dan sudah dibaca layar kajian medis `FE-RWI-044`. Yang baru hanyalah
**pendaftarannya ke dalam kontrak** beserta satu jalur tulis tambahan pada baris pertama.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat diagnosis terstruktur berkode ICD. **Perubahan:** menerima `InpEpisodeId` sebagai konteks, dan **tidak lagi menuntut `ConsultationId`** bila konteks perawatan rawat inap terisi | `PatientDiagnosis : Create` | `CreatePatientDiagnosisRequest` **+ `InpEpisodeId`**, `ConsultationId` menjadi opsional | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia**, jalur tanpa konsultasi **Rencana** |
| `GET` | `/` | Daftar diagnosis pasien, dapat disaring. **Perubahan:** penyaring `inpEpisodeId` ditambahkan | `PatientDiagnosis : Read` | Query `search`, `encounterId`, `consultationId`, **`inpEpisodeId`**, `patientId`, `doctorId`, `diagnosisType`, `diagnosisStatus`, `isPrimary`, `startDate`, `endDate`, paging | `ApiResponse<ResponsePatientDiagnosisPagedResult>` | **Tersedia**, penyaring episode **Rencana** |
| `GET` | `/{id}` | Membaca satu diagnosis | `PatientDiagnosis : Read` | — | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia** |
| `GET` | `/options` | Daftar ringkas untuk **daftar masalah** pada layar kajian medis | `PatientDiagnosis : Read` | Query `consultationId`, `encounterId`, **`inpEpisodeId`**, `patientId`, `onlyActive`, `search` | `ApiResponse<List<PatientDiagnosisOptionResponse>>` | **Tersedia**, penyaring episode **Rencana** |
| `GET` | `/master-options` | Pencarian master diagnosis ICD untuk kotak pilih | `PatientDiagnosis : Read` | Query pencarian | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/filters/metadata` | Nilai bawaan penyaring dan pilihan urutan bagi layar | `PatientDiagnosis : Read` | — | `ApiResponse<PatientDiagnosisFilterMetadataResponse>` | **Tersedia** |
| `PUT` | `/{id}` | Menyunting diagnosis yang belum diselesaikan | `PatientDiagnosis : Update` | `UpdatePatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia** |
| `PATCH` | `/{id}/set-primary` | Menetapkan satu diagnosis sebagai diagnosis utama | `PatientDiagnosis : Update` | `SetPrimaryPatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia** |
| `PATCH` | `/{id}/resolve` | Menyatakan masalah sudah teratasi beserta alasannya | `PatientDiagnosis : Update` | `ResolvePatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia** |
| `PATCH` | `/{id}/cancel` | Membatalkan diagnosis salah catat beserta alasannya. Baris tidak dihapus | `PatientDiagnosis : Update` | `CancelPatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisResponse>` | **Tersedia** |

#### 2.1.1 Kenapa grup ini lahir sekarang, dan kenapa bukan sekadar teks bebas

Kajian medis awal sudah punya kolom `WorkingDiagnosis` berupa **teks bebas** sejak `BE-RWI-045`, dan
kolom itulah yang hari ini dipakai `VAL-DOK-11` untuk menolak penyelesaian kajian yang diagnosisnya
kosong. Jadi dokter **tidak** sedang terhenti: ia tetap dapat menuliskan diagnosis kerjanya.

Yang belum terpenuhi adalah `CAP-022` aturan 5, yang meminta daftar masalah berbentuk **objek
klinis terstruktur atau rujukan**, bukan teks. Teks bebas tidak dapat dicari, tidak berkode ICD,
tidak dapat dinyatakan teratasi, dan tidak terbawa ke ringkasan masalah pada kepala ruang kerja.

| Yang dituntut `CAP-022` | Ditampung `WorkingDiagnosis`? | Ditampung grup ini? |
| --- | :---: | :---: |
| Aturan 2 — kajian medis memuat Diagnosis/Problem List | Ya, sebagai teks | Ya |
| Aturan 5 — daftar masalah berbentuk objek terstruktur berkode | **Tidak** | Ya |
| Masalah dapat dinyatakan teratasi atau dibatalkan beralasan | **Tidak** | Ya |

#### 2.1.2 Aturan konteks yang mengikat baris pertama

| Aturan | Bunyinya |
| --- | --- |
| Salah satu wajib | Permintaan wajib menyebut **`ConsultationId`** atau **`InpEpisodeId`**. Keduanya kosong ditolak — `VAL-DOK-36` |
| Bukan pengganti satu sama lain | Bila keduanya terisi, keduanya wajib menunjuk pasien dan kunjungan yang sama — `VAL-DOK-37` |
| Rawat jalan dan medical check-up **tidak berubah** | Pada kunjungan rawat jalan dan medical check-up, `ConsultationId` **tetap wajib** dan permintaan tanpa nomor konsultasi tetap ditolak dengan kalimat yang sama persis seperti sebelumnya — `VAL-DOK-38`, diuji `RWI-AC-143` |
| IGD **tidak ikut dibuka** | Pelonggaran ini **hanya** untuk kunjungan bertipe `Inpatient`. Alasannya pada catatan di bawah |
| Kewenangan mengikuti kajian medis | Yang boleh mencatat diagnosis dari kajian medis adalah yang boleh menulis kajian medis pasien itu, bukan sekadar pemegang peran — `VAL-DOK-39` |
| Lintas pasien ditolak | Perawatan yang disebut wajib milik pasien pada permintaan itu — `VAL-DOK-40` |

> **Kenapa IGD sengaja tidak ikut, padahal `RWI-DEC-070` dulu memperluas pelonggaran ke
> `Emergency`.** Persetujuan lintas modul `RWI-DEC-062` **tidak mencakup** IGD sejak
> `RWI-DEC-069` mencabut bagian itu — pemilik `EmergencyInstallationManagement` adalah **Rizki
> Gunawan**, orang yang berbeda. Membuka jalur IGD dari sini berarti memutuskan atas nama pemilik
> lain. IGD kemungkinan besar menghadapi keterbatasan yang sama, dan itu **dicatat sebagai temuan
> untuk pemiliknya**, bukan dikerjakan diam-diam di sini.

#### 2.1.3 Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `201` | Diagnosis tercatat pada daftar masalah |
| `400` | Konteksnya tidak lengkap atau tidak cocok — tidak ada nomor konsultasi maupun perawatan, atau keduanya menunjuk pasien yang berbeda |
| `403` | Anda tidak berwenang menulis kajian medis pasien ini |
| `409` | Diagnosis sudah dinyatakan teratasi atau dibatalkan |
| `422` | Pasien tidak sedang dirawat inap, atau perawatannya sudah ditutup |

---

## 3. Health Services / Clinical Management / Patient Integrated Progress Note — `CAP-021`

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Integrated Progress Note")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis catatan terpadu. **Perubahan:** menerima `InpEpisodeId` | `PatientIntegratedProgressNote : Create` | `CreateProgressNoteRequest` **+ `InpEpisodeId`** | `ApiResponse<ProgressNoteResponse>` | **Tersedia**, konteks episode **Rencana** |
| `GET` | `/timeline` | Lini masa catatan pasien | `PatientIntegratedProgressNote : Read` | Query penyaring | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/episodes/{episodeId}` | Lini masa lintas profesi satu episode | `PatientIntegratedProgressNote : Read` | Query `professionType`, `from`, `to` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/verify` | DPJP memverifikasi catatan. **Tidak mengubah penulis aslinya** | `PatientIntegratedProgressNote : Verify` | — | `ApiResponse<ProgressNoteResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}/verification-status` | Catatan yang menunggu dan yang lewat batas verifikasi | `PatientIntegratedProgressNote : Read` | — | `ApiResponse<VerificationStatusResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan catatan beserta alasannya | `PatientIntegratedProgressNote : Update` | Alasan | `ApiResponse<ProgressNoteResponse>` | **Tersedia** |

> **`Verify` adalah Action baru pada Resource yang sudah ada.** Ia wajib memakai nama yang sama
> persis pada `[AccessAction]` dan `[AccessPermission]`, dan wajib diuji dengan peran
> non-SuperAdmin — pelajaran `BE-RWI-034`.

---

## 4. Health Services / Clinical Management / Physician Visit — `CAP-025`

Base URL: `api/v1/health-services/clinical-management/physician-visits`
Judul grup: `[Tags("Health Services / Clinical Management / Physician Visit")]`

**Seluruh grup ini baru.** Tidak ada satu pun endpoint visite dokter di repository hari ini.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat visite sebagai **kejadian tersendiri**. Kunci permintaan **wajib** | `PhysicianVisit : Create` | `CreatePhysicianVisitRequest` beserta header `Idempotency-Key` | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Riwayat visite satu episode, terurut waktu visite. Menampilkan yang dibatalkan beserta alasannya | `PhysicianVisit : Read` | Query `doctorId`, `from`, `to`, `includeCancelled` | `ApiResponse<PagedResult<PhysicianVisitListItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Membaca satu event beserta tautan dokumennya | `PhysicianVisit : Read` | — | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | **Membatalkan event yang salah catat.** Alasan wajib. Baris tidak dihapus | `PhysicianVisit : Cancel` | `CancelPhysicianVisitRequest` berisi alasan | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/links` | Menautkan catatan dokter, CPPT, atau tindakan ke event | `PhysicianVisit : Update` | `UpdatePhysicianVisitLinksRequest` | `ApiResponse<PhysicianVisitResponse>` | **Rencana (belum tersedia)** |

### 4.1 Perilaku yang mengikat pada grup ini

| Perilaku | Bunyinya | Acceptance |
| --- | --- | --- |
| Kiriman ulang | Permintaan kedua dengan kunci yang sama mengembalikan **event yang sama** dengan kode `200`, bukan `409` dan bukan event kedua | `RWI-AC-152`, `RWI-AC-155` |
| Waktu | Yang tersimpan adalah **waktu kedatangan**, bukan waktu penyimpanan | `RWI-AC-150` |
| Kemandirian | Event tetap sah tanpa satu pun dokumen tertaut | `RWI-AC-151` |
| Hitungan | Dua event nyata pada hari yang sama tetap **dua** | `RWI-AC-154` |
| Koreksi | **Tidak ada penyuntingan waktu maupun peran.** Batalkan beralasan, lalu catat ulang dengan kunci baru dan `CorrectsVisitId` terisi | `RWI-DEC-085` |
| Agregasi tagihan | Tidak ada endpoint di grup ini yang menggabungkan event. Agregasi milik Billing dan tidak menyentuh riwayat | `RWI-AC-156` |

### 4.2 Kode status

| Kode | Artinya |
| --- | --- |
| `201` | Visite tercatat |
| `200` | Kiriman ulang dengan kunci yang sama — event yang sama dikembalikan |
| `400` | Waktu visite melewati waktu sekarang, atau alasan pembatalan kosong |
| `403` | Anda tidak berwenang mencatat visite untuk pasien ini |
| `409` | Event sudah dibatalkan dan tidak dapat dibatalkan dua kali |
| `422` | Perawatan pasien sudah ditutup |

---

## 5. Health Services / Clinical Management / Patient Procedure — `CAP-024`

Base URL: `api/v1/health-services/clinical-management/patient-procedures`
Judul grup: `[Tags("Health Services / Clinical Management / Patient Procedure")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat rencana tindakan. **Perubahan:** `InpEpisodeId`, `PhysicianVisitId`, kunci idempotency | `PatientProcedure : Create` | `CreatePatientProcedureRequest` **+ 3 field** | `ApiResponse<PatientProcedureResponse>` | **Tersedia**, perubahan **Rencana** |
| `PATCH` | `/{id}/execute` | Menandai tindakan sudah dikerjakan, menerbitkan fakta klinis ke Billing, dan **mendaftarkan tindakan ke mesin keutuhan** | `PatientProcedure : Update` | Waktu dan pelaksana | `ApiResponse<PatientProcedureResponse>` | **Tersedia**, pendaftaran keutuhan **Rencana** |
| `GET` | `/episodes/{episodeId}` | Tindakan satu episode | `PatientProcedure : Read` | Query `from`, `to` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |

> **Urutan yang mengikat.** Catatan klinis disimpan lebih dulu; fakta ke Billing diterbitkan
> sesudahnya. Kegagalan Billing **tidak** membatalkan catatan klinis — `INV-DOK-09`.

---

## 6. Health Services / Pharmacy Management / Prescription — `CAP-023`

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`
Judul grup: `[Tags("Health Services / Pharmacy Management / Prescription")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat resep dari konteks rawat inap. **Perubahan:** `InpEpisodeId`, jenis resep, kunci idempotency | `Prescription : Create` | `CreatePrescriptionRequest` **+ 3 field** | `ApiResponse<PrescriptionResponse>` | **Tersedia**, perubahan **Rencana** |
| `POST` | `/` | Resep **kedua dan seterusnya** sepanjang episode | Sama | Sama | Sama | **Rencana** — hari ini ditolak batas satu resep aktif |
| `GET` | `/active-by-consultation/{consultationId}` | Resep aktif pada satu catatan | `Prescription : Read` | — | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/episodes/{episodeId}` | Seluruh resep satu episode beserta status pemenuhannya, dapat disaring jenis | `Prescription : Read` | Query `orderType` | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |

> **Tidak ada satu pun endpoint tulis status penyerahan di sini, dan itu disengaja.**
> `RUL-DOK-01` melarang Rawat Inap menandai obat sudah diserahkan. Statusnya hanya dibaca.

---

## 7. Health Services / Laboratory Management / Lab Order — `CAP-015`

Base URL: `api/v1/health-services/laboratory-management/lab-orders`
Judul grup: `[Tags("Health Services / Laboratory Management / Lab Order")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan laboratorium. **Perubahan:** menerima `InpEpisodeId` | `LabOrder : Create` | `CreateLabOrderRequest` **+ `InpEpisodeId`** | `ApiResponse<LabOrderResponse>` | **Tersedia**, konteks episode **Rencana** |
| `GET` | `/` | Daftar pesanan. **Perubahan:** menerima penyaring kunjungan | `LabOrder : Read` | Query **+ `encounterId`** | `ApiResponse<PagedResult<...>>` | **Tersedia**, penyaring **Rencana** |
| `GET` | `/episodes/{episodeId}` | Pesanan dan hasil final satu episode | `LabOrder : Read` | — | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |

> **Temuan yang perlu diketahui:** `LabOrder` terikat pada kunjungan saja — tanpa antrean dan tanpa
> catatan dokter. Pemesanan lab rawat inap karena itu **tidak tertahan gerbang mana pun**. Yang
> kurang adalah penanda episode dan penyaring kunjungan, keduanya untuk menegakkan `INV-DOK-12`.

---

## 8. Health Services / Radiology Management / Rad Order — `CAP-015` ★ grup baru

Base URL: `api/v1/health-services/radiology-management/rad-orders`
Judul grup: `[Tags("Health Services / Radiology Management / Rad Order")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Memesan pemeriksaan radiologi. **Perubahan:** menerima `InpEpisodeId` | `RadOrder : Create` | `CreateRadOrderRequest` **+ `InpEpisodeId`** | `ApiResponse<RadOrderResponse>` | **Tersedia**, konteks episode **Rencana** |
| `GET` | `/` | Daftar pesanan radiologi, **sudah** dapat disaring kunjungan | `RadOrder : Read` | Query `encounterId` | `ApiResponse<PagedResult<...>>` | **Tersedia** |
| `GET` | `/episodes/{episodeId}` | Pesanan, studi, dan hasil final satu episode | `RadOrder : Read` | — | `ApiResponse<PagedResult<...>>` | **Rencana (belum tersedia)** |

> **Grup ini tidak ada pada `0.1.0` karena modulnya dianggap belum ada.** Anggapan itu keliru sejak
> migration `20260828093000_AddRadiologyManagement`. Pemesanan dan penjadwalan sudah berjalan; yang
> diminta hanya penanda episode.

---

## 9. Health Services / Medical Record Management / Clinical Note Addendum — koreksi dokumen

Base URL: `api/v1/health-services/medical-record-management/clinical-note-addendums`
Judul grup: `[Tags("Health Services / Medical Record Management / Clinical Note Addendum")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/by-document/{documentKind}/{documentId}` | **Mengoreksi dokumen final** dengan addendum bernomor urut; alasan koreksi wajib | `ClinicalNoteAddendum : Create` | `CreateAddendumRequest` | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/by-document/{documentKind}/{documentId}` | Membaca seluruh addendum satu dokumen | `ClinicalNoteAddendum : Read` | — | `ApiResponse<...>` | **Tersedia** |
| `POST` | `/by-document/{documentKind}/{documentId}/as-substitute` | **Mengoreksi atas nama dokter yang berhalangan.** Hanya sah bila akun penulis nonaktif, atau ada penetapan berhalangan yang berlaku | `ClinicalNoteAddendum : CreateAsSubstitute` | `CreateAddendumRequest` | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa apakah pengguna berwenang mengoreksi dokumen itu, supaya layar tahu tombol mana yang ditampilkan | `ClinicalNoteAddendum : Read` | — | `ApiResponse<...>` | **Tersedia** |

Penulis addendum **tidak pernah** dikirim dari layar. Ia diambil dari pengguna yang sedang masuk,
justru supaya pembuat koreksi tidak dapat mengaku sebagai orang lain.

### 9.1 Health Services / Medical Record Management / Clinical Note Author Delegation

Base URL: `api/v1/health-services/medical-record-management/clinical-note-author-delegations`
Judul grup: `[Tags("Health Services / Medical Record Management / Clinical Note Author Delegation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | **Menerbitkan penetapan berhalangan** atas nama seorang dokter, disertai alasan dan **masa berlaku yang wajib diisi** | `ClinicalNoteAuthorDelegation : Create` | `CreateDelegationRequest` | `ApiResponse<...>` | **Tersedia** |
| `GET` | `/` | Daftar penetapan yang pernah diterbitkan | `ClinicalNoteAuthorDelegation : Read` | Query penyaring | `ApiResponse<PagedResult<...>>` | **Tersedia** |
| `PUT`/`PATCH` | `/{id}` | Mencabut atau memperbarui penetapan | `ClinicalNoteAuthorDelegation : Update` | `UpdateDelegationRequest` | `ApiResponse<...>` | **Tersedia** |

> **Siapa yang menerbitkan.** `RWI-DEC-088` menetapkan kepala unit rawat inap. Penetapan **tanpa
> masa berlaku ditolak** — penetapan permanen sama saja dengan pintu belakang tetap.
>
> **Batas yang tidak dijaga grup ini.** Penetapan menyatakan "dokter ini berhalangan", **tanpa
> menyebut siapa penggantinya**. Pembatasan bahwa hanya DPJP aktif episode itu yang boleh
> mengoreksi dijaga di sisi Rawat Inap — `permission-audit-matrix.md` bagian 3.

> **Inilah alasan tiga endpoint `PATCH /{id}/amend` pada `0.1.0` dicabut.** Mekanisme koreksi sudah
> ada, sudah menyimpan alasan, nomor urut, penulis pengganti, dan waktu tanda tangan, serta sudah
> menjangkau jenis dokumen `Consultation`, `Assessment`, `ProgressNote`, dan `Procedure`. Merancang
> jalur koreksi kedua berarti dua tempat menyimpan alasan koreksi yang sama.

---

## 10. Endpoint milik modul lain yang dibaca ruang kerja ini

| Endpoint | Modul | Dipakai untuk |
| --- | --- | --- |
| `GET /census` | `episode-rawat-inap` | **Daftar pasien dokter** — sumber yang benar, menggantikan antrean rawat jalan |
| `GET /episodes/{id}` | `episode-rawat-inap` | Konteks pasien, lokasi, status episode |
| `GET /episodes/{id}/doctor-assignments` | `episode-rawat-inap` | Menentukan DPJP yang berlaku pada tanggal itu |
| `GET /patient-allergies`, `/patient-vital-signs` | `ClinicalManagement` | Ditampilkan pada kepala ruang kerja |
| `GET /patient-assessments?assessmentType=Initial` | `ClinicalManagement` | Membaca pengkajian keperawatan — **hanya baca** |
| `GET /clinical-document-integrities/by-document/...` | `MedicalRecordManagement` | Menampilkan keadaan tanda tangan dan penguncian dokumen |

---

## 11. Yang **tidak** ada di kontrak ini

| Yang tidak ada | Alasan |
| --- | --- |
| Endpoint menandai obat diserahkan | `RUL-DOK-01` |
| Endpoint menulis hasil laboratorium maupun radiologi | `RUL-DOK-02`; hasil final milik modul pemiliknya |
| Endpoint menyunting waktu atau peran visite | `RWI-DEC-085`; koreksi lewat pembatalan lalu pencatatan ulang |
| Endpoint menghitung visite dari SOAP | `INV-DOK-07` |
| Endpoint agregasi tagihan visite | Milik Billing; kebijakannya belum ada — `ARCH-GAP-012` |
| Endpoint resume pulang | `CAP-026` milik `episode-rawat-inap` — `RWI-DEC-083` |
| Endpoint antrean apa pun untuk pasien rawat inap | `RWI-RULE-026` aturan 2 melarang antrean semu |
| Diagnosis tanpa nomor konsultasi pada kunjungan **IGD** | Pemilik `EmergencyInstallationManagement` adalah **Rizki Gunawan**; `RWI-DEC-069` mencabut IGD dari persetujuan lintas modul `RWI-DEC-062`. Lihat catatan pada bagian 2.1.2 |
| Melonggarkan `ConsultationId` pada **resep** dan **tindakan** | Tetap ditolak. Keduanya memang lahir dari catatan dokter, dan catatan dokter sendiri sudah dibuka `BE-RWI-043` — `02-backend-architecture.md` bagian 9 |
| Endpoint menghapus diagnosis | Diagnosis salah catat **dibatalkan beralasan**, tidak dihapus. Baris tetap terbaca |
