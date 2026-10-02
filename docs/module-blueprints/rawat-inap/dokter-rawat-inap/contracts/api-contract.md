# API Contract — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` — bentuk `COMPOSITE`, `RWI-DEC-082` |
| Contract version | **`0.6.0`** |
| `last_changed_in` | **`0.6.0`** — penyelarasan `PRD-RWI-V2-001`: kewenangan penulis konsep, verifikasi CPPT hanya DPJP, pesanan dengan pemberi instruksi, Resep Harian, rekonsiliasi obat, sliding scale, template resep, Catatan Saya. Seluruh isi baru pada **bagian 12** |
| Status | **`draft`** untuk `0.6.0`. `0.5.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Product/Domain: **Muhammad Hamzah** (`RWI-DEC-061`); pemilik tabel: `ClinicalManagement`, `PharmacyManagement`, `LaboratoryManagement`, `RadiologyManagement`, `MedicalRecordManagement` (`RWI-DEC-081`) |
| `approved_by` / `approved_at` | **Muhammad Hamzah** / **2026-09-09** untuk `0.4.0`; `0.3.0` disetujui 2026-09-03 |
| `input_revision` | `02-backend-architecture.md` `0.2`; arsitektur domain `0.2`; `PRD-RWI-FINAL-001` v1.0.0 |
| `input_hash` | Arsitektur domain SHA-256 `226c6ef1e4bfec544c366b265fe1e4530e80c510da33c1a9eaf2e62161d0b717` |
| Backend SHA | `93b3227c431401d8f586dec4e1fb25fbf41766e3`; **bagian 12 dibaca pada `df3679c0d5b2f08106702153eb242d3a6cb2929b`** |
| `input_revision` `0.6.0` | `02-backend-architecture.md` `0.5` bagian 11; decision log revision `21` SHA-256 `1c55c80a…2d45102a`; gate `1.6` SHA-256 `f31d207a…e49b5300`; `PRD-RWI-V2-001` SHA-256 `2b3b2f29…0a679f` |
| Compatibility impact | **Tidak ada endpoint yang dihapus atau berubah bentuknya.** `0.4.0` mendaftarkan grup **Patient Diagnosis** yang selama ini terlewat, dan membuka satu jalur baru: diagnosis terstruktur boleh lahir dari kajian medis tanpa nomor konsultasi. **Permintaan lama tetap sah apa adanya** — nomor konsultasi yang dikirim tetap diterima dan tetap diperlakukan sama. Perilaku rawat jalan dan medical check-up tidak berubah — `RWI-AC-143` |
| Tanggal | 2 September 2026; diamendemen 9 September 2026 |

---


## 0.A Perubahan pada `contract_version` `0.5.0` — Gelombang 1A

**Status `approved` sejak 11 September 2026** lewat `RWI-DEC-105`. Menyerap dua koreksi `P0` dari `PRD-to-MVP-Rawat-Inap-V2` yang dimiliki sub-modul ini. `0.4.0` tetap `approved` dan tidak dicabut.

### 0.A.1 Jalur hapus CPPT ditutup — `RWI-DEC-098`

| Method | Path | Keadaan sebelum `0.5.0` | Ketetapan `0.5.0` |
| --- | --- | --- | --- |
| `DELETE` | `/{id}` | **Ada di source**, `PatientIntegratedProgressNoteController` baris 865. Melakukan soft delete tanpa satu pun pemeriksaan status dokumen, tanpa alasan, dan tanpa memeriksa penulis | **DIHAPUS.** Route tidak lagi tersedia. Permintaan ke path itu dijawab `404`, bukan `403`, karena endpointnya memang tidak ada |

**Penggantinya sudah ada dan tidak perlu dibuat.**

| Keadaan dokumen | Jalur yang sah | Endpoint |
| --- | --- | --- |
| Masih draf | Pembatalan beralasan | `PATCH /{id}/cancel`, sudah tersedia sejak `0.3.0` |
| Sudah final atau terverifikasi | Addendum bernomor urut | Grup Clinical Note Addendum bagian 9 |

**Satu perubahan perilaku yang wajib diserap.** `PATCH /{id}/cancel` hari ini **tidak** memanggil
`EnsureMutableAsync`. Pemeriksaan keutuhan dokumen di controller yang sama hanya terpasang pada
`PUT` pembaruan, baris 586. Sejak `0.5.0`, jalur pembatalan **wajib** memanggil pemeriksaan itu
lebih dulu, sehingga catatan yang sudah final atau terverifikasi ditolak dibatalkan dan diarahkan
ke addendum.

| Keadaan | Jawaban sejak `0.5.0` |
| --- | --- |
| Membatalkan catatan berstatus draf | `200`, catatan ditandai batal beserta alasan dan pelakunya |
| Membatalkan catatan yang sudah final atau terverifikasi | **`422`**, disertai keterangan bahwa koreksi dilakukan lewat addendum |
| Membatalkan tanpa alasan | `400` |
| Memanggil `DELETE /{id}` | `404`, route tidak ada |

**Batas yang dinyatakan apa adanya.** `RWI-DEC-098` menutup **dua** controller saja, yaitu CPPT
dan tanda vital. Delapan jalur hapus lain pada `ClinicalManagement` **sengaja tetap berdiri** atas
keputusan pemilik, termasuk consent, alergi, riwayat penyakit, riwayat keluarga, dokumen klinis,
lampiran, surat keterangan medis, dan infeksi nosokomial. Konsekuensinya tercatat pada
`RWI-DEC-098` dan ketegangannya dilacak `RWI-OQ-055`. Kontrak ini **tidak** menyatakan kedelapan
jalur itu aman; ia hanya menyatakan keduanya di luar `Gelombang 1A`.

### 0.A.2 Penulis dan kewenangan dokter ditegakkan — `RWI-DEC-099`

Hari ini `InpatientClinicalContextService.ResolveAsync` memiliki penjaga kewenangan yang benar,
tetapi penjaga itu **mati** karena `isDoctorAuthorized` bernilai benar secara bawaan dan hanya
diuji bila `doctorId` dikirimkan. Dari sembilan titik panggil, hanya `PhysicianVisitController`
yang mengirimkannya.

| Yang berubah | Ketetapan `0.5.0` |
| --- | --- |
| Sumber identitas penulis | Diambil dari `ApplicationUser.DoctorId` milik pengguna terautentikasi. Tanpa pemetaan aktif, penulisan klinis **ditolak `403`** |
| `DoctorId` pada request | **Tidak lagi menentukan penulis.** Bila dikirim dan berbeda dari dokter pengguna, permintaan ditolak `403`, bukan diam-diam dipakai |
| Kewenangan atas pasien | Penulis wajib punya `InpDoctorAssignment` aktif pada episode itu, dengan peran `Dpjp`, `Consultant`, atau `OnCallDoctor` |
| Waktu penilaian | Kewenangan dinilai pada **waktu klinis** dokumen, bukan waktu penyimpanan. Backdating tidak dapat dipakai melewati periode penugasan |
| Titik panggil yang wajib mengirim dokter pelaku | Seluruh jalur tulis pada grup Doctor Consultation, Patient Assessment, Patient Integrated Progress Note, Patient Diagnosis, dan Patient Procedure |

| Keadaan | Jawaban sejak `0.5.0` |
| --- | --- |
| Pengguna tanpa pemetaan dokter menulis catatan | `403` |
| Dokter tanpa penugasan aktif pada episode itu | `403` |
| Dokter mengirim `DoctorId` milik dokter lain | `403`, dan **nol** baris tersimpan atas nama pihak lain |
| Dokter dengan penugasan yang sudah berakhir, menulis dengan waktu klinis di dalam periodenya | `200`, karena penilaian memakai waktu klinis |
| Dokter dengan penugasan yang sudah berakhir, menulis dengan waktu klinis di luar periodenya | `403` |

**Yang belum diputuskan dan berperilaku fail-closed.** Kewenangan konsulen memutuskan pulang
bergantung pada kebijakan yang belum ada sumbernya. Sampai kebijakan itu disetujui, permintaan
keputusan pulang dari peran `Consultant` **ditolak**. Penolakan itu adalah keadaan sementara yang
dinyatakan terbuka, bukan kebijakan yang sudah diputuskan. Dilacak `OPEN-MVP-004`.

**Ketergantungan lintas sub-modul.** Peran pada penugasan adalah kolom milik `InpDoctorAssignment`,
yang dimiliki `episode-rawat-inap`. Kontrak ini **membacanya**, tidak membuatnya. Migration kolom
dan perubahan filter index unik dikerjakan sub-modul itu, dan urutannya dipegang
[`../02-module-map.md`](../02-module-map.md).

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
| `PATCH` | `/{id}/cancel` | Membatalkan catatan beserta alasannya. **Perubahan `0.5.0`:** wajib memanggil pemeriksaan keutuhan dokumen lebih dulu, sehingga catatan final atau terverifikasi ditolak `422` dan diarahkan ke addendum | `PatientIntegratedProgressNote : Update` | Alasan | `ApiResponse<ProgressNoteResponse>` | **Tersedia**, perilaku berubah |
| ~~`DELETE`~~ | ~~`/{id}`~~ | ~~Menghapus CPPT~~ — **DIHAPUS `0.5.0`** oleh `RWI-DEC-098`. Ada di source hari ini dan melakukan soft delete tanpa pemeriksaan status, tanpa alasan, dan tanpa memeriksa penulis | ~~`PatientIntegratedProgressNote : Delete`~~ — | — | — | **Dicabut**, jawaban menjadi `404` |

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

> **Dua baris di atas diperbarui `0.6.0`.** "Melonggarkan `ConsultationId` pada tindakan" **tidak lagi
> ditolak** — lihat 12.5. "Endpoint resume pulang" tetap milik `episode-rawat-inap`, tetapi ditulis dari tab
> Resume Medis ruang kerja dokter — lihat 12.14.

---

## 12. Perubahan pada `contract_version` `0.6.0` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

**Status `draft`.** Seluruh endpoint baru berlabel **Rencana (belum tersedia)**. Endpoint yang sudah ada
tetapi perilakunya berubah berlabel **Tersedia, perilaku Rencana**. Kolom `Hak akses` tetap satu-satunya
tempat pemetaan endpoint ke hak akses.

### 12.0 Dampak kompatibilitas

| Perubahan | Siapa yang terkena | Dampak |
| --- | --- | --- |
| Konsep SOAP dan kajian medis rawat inap hanya disunting dan diselesaikan penulisnya | Pengguna yang selama ini menyelesaikan konsep orang lain | Sebelumnya lolos dan tercatat atas nama penulis (`RWI-FACT-029`); kini `403`. **Poliklinik dan IGD tidak berubah** |
| Verifikasi CPPT hanya DPJP | Konsulen dan dokter jaga | Sebelumnya lolos (`RLN3-CAP-25`); kini `403` |
| Tindakan baru pada episode `Closed` | Siapa pun | Sebelumnya diterima (`RLN3-CAP-35`); kini `422` |
| `ConsultationId` pesanan tindakan boleh kosong pada rawat inap | Klien yang membaca `ConsultationId` sebagai selalu terisi | Wajib menangani `null` pada respons pesanan rawat inap |
| Template resep: pemilik dari akun login | **Poliklinik dan Farmasi** | Permintaan yang menyebut dokter lain sebagai pemilik kini `403`. **Wajib diberitahukan pemilik `rawat-jalan`** sebelum dirilis — `RWI-DEC-135` konsekuensi (1) |
| Pakai template dengan obat tidak tersedia | Semua pemakai | Sebelumnya seluruh pemakaian gagal (`RLN3-CAP-43`); kini butir itu ditandai dan yang lain tetap masuk draft |
| Endpoint lain | — | Tambahan murni; permintaan lama tetap sah |

### 12.1 Health Services / Inpatient Management / Inpatient Census — dipakai, dirancang `episode-rawat-inap`

Base URL: `api/v1/health-services/inpatient-management/census`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Census")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | **Daftar Pasien Rawat Inap dokter**: hanya pasien yang dokter login punya penugasan aktif sebagai DPJP, konsulen, atau dokter jaga | `InpatientCensus : Read` | Query **`assignedToMe=true`**, `search` (nama / No. RM / kamar), `pageNumber`, `pageSize` | `ApiResponse<PagedResult<CensusItemResponse>>` **+ `MyAssignmentRole`, `DischargePending`, `RequiresIsolation`** | **Tersedia**, penyaring **Rencana** |
| `GET` | `/summary` | Angka Total Pasien, Dirawat, Discharge Pending, Perlu Review dari daftar yang sama | `InpatientCensus : Read` | Query `assignedToMe=true` | `ApiResponse<CensusSummaryResponse>` | **Tersedia**, penyaring **Rencana** |

Kontrak kanonisnya `../../episode-rawat-inap/contracts/api-contract.md` bagian `0.9.0`. Aturannya dicantumkan di
sini karena menentukan layar `FE-DOK-09`: bila `assignedToMe=true`, `DoctorId` pada query **diabaikan** dan dokter
diambil dari akun login — `RWI-DEC-111`. "Perlu Review" dihitung dari entri CPPT yang menunggu verifikasi dokter itu
ditambah pesanan perawat yang menunggu verifikasi instruksinya.

### 12.2 Health Services / Clinical Management / Doctor Consultation — `CAP-020`, perubahan perilaku

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter rawat inap. **Perubahan `0.6.0`:** mensyaratkan penugasan pada waktu klinis **dan** aktif saat disimpan (`INV-DOK-15`); membentuk registrasi `Draft` pada mesin keutuhan dalam transaksi yang sama (`INV-DOK-18`) | `DoctorConsultation : Create` | Sama; `Idempotency-Key` wajib pada rawat inap | `ApiResponse<DoctorConsultationResponse>` | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}` | Mengubah konsep. **Perubahan:** hanya penulis (`INV-DOK-14`); konsep terkunci "Tidak Ditandatangani" ditolak dengan arahan addendum | `DoctorConsultation : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/soap` | Simpan otomatis. **Perubahan:** penjaga sama dengan `PUT`; tidak menambah registrasi | `DoctorConsultation : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/complete` | Menyelesaikan = tanda tangan penulis. **Perubahan:** hanya penulis; registrasi yang sama berubah `Signed`; tanpa penugasan aktif tetap diterima bila konsep dibuat saat penugasan aktif dan waktu klinisnya di dalam periode itu (`RWI-DEC-128` butir 2) | `DoctorConsultation : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/cancel` | Membatalkan konsep. **Perubahan:** registrasinya menjadi `Cancelled`, baris tidak dihapus (`RWI-AC-217`) | `DoctorConsultation : Update` | Alasan wajib | Sama | **Tersedia**, perilaku **Rencana** |

Seluruh perubahan hanya menyala bila kunjungan punya episode rawat inap. Kunjungan poliklinik dan IGD tetap memakai
perilaku lama — `RWI-AC-186`, `RWI-AC-218`.

### 12.3 Health Services / Clinical Management / Patient Assessment — kajian medis `CAP-022`, perubahan perilaku

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuat kajian medis (`MedicalInitial`/`MedicalReassessment`). **Perubahan:** penjaga `INV-DOK-15`; registrasi `Draft` sejak dibuat | `PatientAssessment : Create` | Sama | `ApiResponse<PatientAssessmentResponse>` | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}` | Mengubah konsep kajian medis. **Perubahan:** hanya penulis; `EnsureNursingUnitAuthorityAsync` **tidak lagi** meloloskan jenis medis tanpa pemeriksaan (`RLN3-CAP-34`) | `PatientAssessment : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/complete` | Menyelesaikan. **Perubahan:** hanya penulis; tanda tangan atas nama penekan tombol yang sama dengan penulis, bukan `AssessmentByUserId ?? CreateBy` | `PatientAssessment : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/cancel` | **Perubahan:** registrasi `Cancelled` | `PatientAssessment : Update` | Alasan wajib | Sama | **Tersedia**, perilaku **Rencana** |

### 12.4 Health Services / Clinical Management / Patient Integrated Progress Note — `CAP-021`

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menulis catatan terpadu. **Perubahan `0.6.0`:** menerima `NoteKind`; nilai diperiksa terhadap profesi penulis dari akun login (`VAL-DOK-59`) | `PatientIntegratedProgressNote : Create` | `CreateProgressNoteRequest` **+ `NoteKind`** | `ApiResponse<ProgressNoteResponse>` **+ `NoteKind`** | **Tersedia**, perilaku **Rencana** |
| `GET` | `/episodes/{episodeId}` | Lini masa lintas profesi. **Perubahan:** saring `noteKind` dan `professionType`; menampilkan penulis, profesi, jenis, waktu klinis, sumber, status verifikasi | `PatientIntegratedProgressNote : Read` | Query `noteKind`, `professionType`, `from`, `to`, `verificationStatus` | `ApiResponse<PagedResult<ProgressNoteListItem>>` | **Tersedia**, penyaring **Rencana** |
| `PATCH` | `/{id}/verify` | **Perubahan `0.6.0`:** hanya dokter berperan **DPJP** yang aktif pada detik verifikasi; untuk episode `Closed`, hanya **DPJP terakhir** dan hanya entri yang ditulis sebelum penutupan (`INV-DOK-16`) | `PatientIntegratedProgressNote : Verify` | — | `ApiResponse<ProgressNoteResponse>` | **Tersedia**, perilaku **Rencana** |
| `GET` | `/verification-worklist` | Daftar pantau verifikasi milik dokter login: entri pasien yang ia DPJP aktifnya, ditambah episode `Closed` yang ia DPJP terakhirnya dan masih punya entri tertinggal | `PatientIntegratedProgressNote : Read` | Query `pageNumber`, `pageSize`, `includeClosedEpisodes` (bawaan `true`) | `ApiResponse<PagedResult<VerificationWorklistItem>>` | **Rencana (belum tersedia)** |

`VerificationWorklistItem`: `EpisodeId`, `EpisodeNumber`, `PatientName`, `MedicalRecordNumber`, `EpisodeStatus`,
`PendingCount`, `OldestPendingNoteDateTime`, `IsOverdue`. Lamanya keterlambatan dihitung dari kebijakan aktif; bila
kebijakan kosong, `IsOverdue = false` — `RWI-RULE-021` belum final.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `200` | Terverifikasi |
| `403` | "Hanya DPJP yang sedang bertugas atas pasien ini yang dapat memverifikasi." Termasuk konsulen, dokter jaga, dan DPJP sebelum DPJP terakhir |
| `409` | Catatan sudah diverifikasi |
| `422` | Entri ditulis setelah episode ditutup, atau catatan dibatalkan |

### 12.5 Health Services / Clinical Management / Patient Procedure — `CAP-024`

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/inpatient-orders` | Membuat **pesanan tindakan rawat inap** oleh dokter atau perawat. Perawat **wajib** menyebut dokter pemberi instruksi yang punya penugasan aktif; penginput dari akun login. Tidak menuntut `ConsultationId` | `PatientProcedure : Create` | `CreateInpatientProcedureOrderRequest` (`InpEpisodeId`, `ProcedureId`, `Quantity`, `Priority`, `ClinicalReason`, `Instruction`, `InstructingDoctorId` bila perawat) + `Idempotency-Key` | `ApiResponse<PatientProcedureResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Jalur lama dari catatan dokter. **Perubahan:** tindakan baru pada episode `Closed` ditolak `422` (`forNewDocument: true`) | `PatientProcedure : Create` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}` | Mengubah pesanan yang belum dilaksanakan. **Perubahan:** hanya penginput (`INV-DOK-17`); dokter pemberi instruksi pun ditolak | `PatientProcedure : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/cancel` | Membatalkan pesanan yang belum dilaksanakan. **Perubahan:** hanya penginput atau DPJP aktif; alasan wajib; pesanan tertagih tetap ditolak seperti hari ini | `PatientProcedure : Update` | `CancelProcedureRequest` (`Reason` wajib) | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/execute` | Menandai dilaksanakan. **Perubahan:** pelaksana dari akun login menjadi penulis dan penanda tangan catatan pelaksanaan; kewenangan pelaksana dokter lewat penugasan, perawat lewat unit | `PatientProcedure : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PATCH` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi pesanan yang dibuat perawat. **Tidak** mengubah penginput maupun isi pesanan | `PatientProcedure : Verify` | — | Sama | **Rencana (belum tersedia)** |
| `GET` | `/instruction-verification-worklist` | Pesanan tindakan yang menunggu verifikasi dokter login | `PatientProcedure : Read` | Query `pageNumber`, `pageSize` | `ApiResponse<PagedResult<InstructionVerificationItem>>` | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `201` | Pesanan tersimpan dan langsung diteruskan |
| `200` | Kiriman ulang dengan kunci yang sama — pesanan yang sama dikembalikan |
| `400` | Dokter pemberi instruksi belum dipilih, alasan pembatalan kosong, atau isian pesanan tidak lengkap |
| `403` | Anda bukan penginput pesanan ini; atau dokter yang dipilih tidak sedang bertugas atas pasien ini; atau Anda bukan dokter pemberi instruksinya |
| `409` | Pesanan sudah dilaksanakan, sudah dibatalkan, sudah diverifikasi, atau sudah menimbulkan tagihan |
| `422` | Perawatan pasien sudah ditutup |

### 12.6 Health Services / Pharmacy Management / Prescription — `CAP-023-RSP`

Base URL: `api/v1/health-services/pharmacy-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | **Resep Harian**: seluruh resep episode termasuk racikan, obat pulang terbedakan, butir yang dihentikan tetap tampil. **Perubahan:** saring periode | `Prescription : Read` | Query `period` (`today`, `week`, `month`) atau `from`/`to`, `orderType` | `ApiResponse<PagedResult<InpatientPrescriptionListItem>>` **+ butir beserta `IsStopped`, `StoppedAt`, `StoppedByName`, `StopReason`** | **Tersedia**, penyaring **Rencana** |
| `PATCH` | `/items/{itemId}/stop` | **Menghentikan satu butir obat.** Dokter dengan penugasan aktif; alasan wajib; riwayat butir tidak dihapus; dosis MAR `Due` sesudahnya menjadi `Cancelled` dalam transaksi yang sama; butir insulin berdosis skala ikut menghentikan order sliding scale-nya | **`Prescription : Stop`** | `StopPrescriptionItemRequest` (`Reason` wajib) | `ApiResponse<InpatientPrescriptionItemResponse>` | **Rencana (belum tersedia)** |

**Contoh.** 4 September 09.10 dr. Rina menghentikan Ceftriaxone dengan alasan "kultur sensitif, ganti oral". Dosis
`Due` pukul 20.00 menjadi `Cancelled` beralasan "resep dihentikan"; dosis 08.00 yang `Administered` tetap utuh. Ns.
Siti membuka Obat & Alkes dan melihat daftar yang sama **tanpa** tombol hentikan.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | "Alasan penghentian wajib diisi." |
| `403` | Anda tidak sedang bertugas atas pasien ini |
| `409` | Butir sudah dihentikan, atau resepnya sudah dibatalkan |
| `422` | Perawatan pasien sudah ditutup |

### 12.7 Health Services / Pharmacy Management / Prescription Template — `CAP-023-RSP`, perubahan perilaku

Base URL: `api/v1/health-services/pharmacy-management/prescription-templates`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | **Perubahan:** query `ownerScope=Mine` hanya mengembalikan template milik dokter login, **tanpa** template Bersama milik dokter lain | `PrescriptionTemplate : Read` | Query **+ `ownerScope`** | Sama | **Tersedia**, penyaring **Rencana** |
| `POST` | `/` | **Perubahan untuk semua pemakai:** pemilik diambil dari dokter akun login. `OwnerDoctorId` yang berbeda → `403`, nol baris; akun tanpa tautan dokter → `403`. Dari ruang kerja rawat inap `IsShared` dipaksa `false` | `PrescriptionTemplate : Create` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `POST` | `/from-prescription` | Sama dengan `POST /` | `PrescriptionTemplate : Create` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}` | **Perubahan:** hanya pemilik; pemindahan pemilik ditolak; template kosong ditolak | `PrescriptionTemplate : Update` | Sama | Sama | **Tersedia**, perilaku **Rencana** |
| `DELETE` | `/{id}` | **Perubahan:** hanya pemilik | `PrescriptionTemplate : Delete` | — | Sama | **Tersedia**, perilaku **Rencana** |
| `POST` | `/{id}/apply` | **Perubahan:** bila resep tujuan berkonteks rawat inap, hanya template milik dokter login dan hanya oleh dokter; setiap butir diperiksa ulang terhadap alergi aktif pasien (`TrxPatientAllergy`) dan ketersediaan obat (`IsActive`, `IsPrescribable`); butir bermasalah **ditandai**, bukan menggagalkan seluruh pemakaian; tidak pernah langsung menghasilkan resep final | `PrescriptionTemplate : Create` | `ApplyPrescriptionTemplateRequest` | `ApiResponse<ApplyPrescriptionTemplateResponse>` **+ `Items[].Flags` (`AllergyConflict`, `Unavailable`)** | **Tersedia**, perilaku **Rencana** |

**Contoh.** Budi alergi Paracetamol. dr. Rina memakai "Pneumonia dewasa": tiga butir masuk draft; butir Paracetamol
bertanda `AllergyConflict` "bentrok alergi: Paracetamol". Menyimpan draft dengan butir bertanda itu ditolak sampai
butirnya dihapus atau diganti — `VAL-DOK-57`.

### 12.8 Health Services / Pharmacy Management / Medication Reconciliation — grup baru — `CAP-023-RSP`

Base URL: `api/v1/health-services/pharmacy-management/medication-reconciliations`
Judul grup: `[Tags("Health Services / Pharmacy Management / Medication Reconciliation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Daftar obat bawaan satu episode beserta keputusan terakhir | `MedicationReconciliation : Read` | — | `ApiResponse<List<ReconciliationItemResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | **Perawat** mencatat obat yang sedang dipakai pasien. `DrugId` wajib; tidak ada nama teks bebas | `MedicationReconciliation : Create` | `CreateReconciliationItemRequest` (`InpEpisodeId`, `DrugId`, `Dose`, `DoseUnitMeasurementId`, `FrequencyText`, `Route`, `Note`) + `Idempotency-Key` | `ApiResponse<ReconciliationItemResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan baris salah catat **sebelum** ada keputusan dokter | `MedicationReconciliation : Update` | Alasan wajib | Sama | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decisions` | **Dokter** memutuskan: `ContinueSame`, `ContinueModified`, `Stopped`. "Lanjut" mengisi butir pada draft resep episode yang sedang dibuka dokter, atau membuat draft baru bila belum ada | **`MedicationReconciliation : Decide`** | `CreateReconciliationDecisionRequest` (`DecisionType`, `DecisionNote`, `TargetDraftPrescriptionId` opsional) | `ApiResponse<ReconciliationDecisionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/decisions` | Riwayat keputusan satu obat | `MedicationReconciliation : Read` | — | `ApiResponse<List<ReconciliationDecisionResponse>>` | **Rencana (belum tersedia)** |

**Contoh dan jalur tidak normal.** Senin 14.20 Ns. Siti mencatat Amlodipin dan Metformin Budi → `201` dua kali.
15.00 dr. Rina `ContinueSame` Amlodipin → butir draft resep 1×1 terbentuk; `Stopped` Metformin dengan catatan
"pasien dipuasakan". Ns. Siti mencoba `POST /{id}/decisions` → `403`. dr. Rina tidak menyelesaikan resepnya → MAR
tidak membuat dosis Amlodipin sampai resep aktif.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Obat belum dipilih dari master obat; rute belum dipilih |
| `403` | Keputusan per obat hanya dapat diambil dokter yang berwenang menulis resep untuk pasien ini |
| `409` | Keputusan tidak dapat diganti karena resep hasilnya sudah aktif; atau baris sudah punya keputusan sehingga tidak dapat dibatalkan perawat |
| `422` | Perawatan pasien sudah ditutup |

### 12.9 Health Services / Master Data / Drug — pendaftaran non-formularium

Base URL: `api/v1/health-services/master-data/drugs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/non-formulary-registrations` | Mendaftarkan obat bawaan yang belum ada di master obat. Server **selalu** menulis `IsFormulary = false`, apa pun isi permintaan | `Drug : Create` | `CreateNonFormularyDrugRequest` (`DrugCategoryId`, `DrugName` wajib; generik, bentuk, kekuatan, satuan opsional) | `ApiResponse<DrugResponse>` | **Rencana (belum tersedia)** |

Nol hak akses baru. Siapa yang boleh ditentukan butir `Drug : Create` pada Akses Role — `RWI-DEC-134` butir (3).

### 12.10 Health Services / Pharmacy Management / Sliding Scale Template — grup baru

Base URL: `api/v1/health-services/pharmacy-management/sliding-scale-templates`
Judul grup: `[Tags("Health Services / Pharmacy Management / Sliding Scale Template")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar template beserta versi sah yang berlaku | `SlidingScaleTemplate : Read` | Query `isActive` | `ApiResponse<List<SlidingScaleTemplateListItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Template beserta seluruh versinya | `SlidingScaleTemplate : Read` | — | `ApiResponse<SlidingScaleTemplateResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat template baru tanpa versi | `SlidingScaleTemplate : Update` | `CreateSlidingScaleTemplateRequest` | Sama | **Rencana (belum tersedia)** |
| `POST` | `/{id}/versions` | Membuat versi `Draft` baru beserta rentangnya | `SlidingScaleTemplate : Update` | `SaveSlidingScaleVersionRequest` (`GlucoseUnit`, `Ranges[]`) | `ApiResponse<SlidingScaleVersionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/versions/{versionId}` | Mengubah versi yang masih `Draft`; mencatat pengubah terakhir | `SlidingScaleTemplate : Update` | Sama | Sama | **Rencana (belum tersedia)** |
| `POST` | `/versions/{versionId}/approve` | Mengesahkan. Pengesah ≠ pengubah terakhir; versi sah sebelumnya menjadi `Retired` dalam transaksi yang sama | **`SlidingScaleTemplate : Approve`** | `ApproveSlidingScaleVersionRequest` (`ApprovalNote` opsional) | Sama | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Rentang bertumpuk, berlubang, atau tidak menutup seluruh nilai; dosis negatif; satuan belum dipilih |
| `403` | Anda tidak punya hak mengesahkan, atau Anda pengubah terakhir versi ini |
| `409` | Versi sudah disahkan atau sudah diganti sehingga tidak dapat diubah |

### 12.11 Health Services / Pharmacy Management / Sliding Scale Order — grup baru

Base URL: `api/v1/health-services/pharmacy-management/sliding-scale-orders`
Judul grup: `[Tags("Health Services / Pharmacy Management / Sliding Scale Order")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Dokter memesan sliding scale pada butir insulin draft resep. Rentang versi template **disalin**; bila dokter mengubah rentang atau dosis, alasan wajib | `SlidingScaleOrder : Create` | `CreateSlidingScaleOrderRequest` (`PrescriptionItemId`, `TemplateVersionId`, `Ranges[]` opsional, `AdjustmentReason`, `CheckFrequencyCode` usulan `G-22`) + `Idempotency-Key` | `ApiResponse<SlidingScaleOrderResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/episodes/{episodeId}` | Order sliding scale satu episode | `SlidingScaleOrder : Read` | Query `status` | `ApiResponse<List<SlidingScaleOrderListItem>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Order beserta seluruh versi dan rentangnya | `SlidingScaleOrder : Read` | — | `ApiResponse<SlidingScaleOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/versions` | Menyesuaikan order → versi baru; pelaksanaan lama tetap menunjuk versi lama | `SlidingScaleOrder : Update` | `AdjustSlidingScaleOrderRequest` (`ExpectedVersionNumber`, `Ranges[]`, `AdjustmentReason` wajib) | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/stop` | Menghentikan order; pelaksanaan berikutnya ditolak | `SlidingScaleOrder : Update` | Alasan wajib | Sama | **Rencana (belum tersedia)** |

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Alasan penyesuaian kosong; rentang tidak sah |
| `403` | Anda tidak berwenang menulis resep untuk pasien ini |
| `409` | Versi template bukan versi yang sah; butir resep bukan insulin berdosis skala atau sudah punya order; order sudah dihentikan; nomor versi yang dikirim sudah basi karena dokter lain lebih dulu menyesuaikan |
| `422` | Perawatan pasien sudah ditutup; atau belum ada satu pun protokol yang disahkan |

### 12.12 Laboratory dan Radiology — pesanan perawat dengan pemberi instruksi `CAP-015-LAB`, `CAP-015-RAD`

**Gerbang implementasi:** persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` belum tercatat.

#### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | **Perubahan:** menerima `InstructingDoctorId`; wajib bila penginput perawat; status verifikasi `Pending` | `LabOrder : Create` | `CreateLabOrderRequest` **+ `InstructingDoctorId`** | `ApiResponse<LabOrderResponse>` | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi | **`LabOrder : Verify`** | — | Sama | **Rencana (belum tersedia)** |
| `GET` | `/instruction-verification-worklist` | Pesanan laboratorium menunggu verifikasi dokter login | `LabOrder : Read` | Query paging | `ApiResponse<PagedResult<InstructionVerificationItem>>` | **Rencana (belum tersedia)** |

#### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | **Perubahan:** sama dengan laboratorium | `RadOrder : Create` | `CreateRadOrderRequest` **+ `InstructingDoctorId`** | `ApiResponse<RadOrderResponse>` | **Tersedia**, perilaku **Rencana** |
| `PUT` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi | **`RadOrder : Verify`** | — | Sama | **Rencana (belum tersedia)** |
| `GET` | `/instruction-verification-worklist` | Pesanan radiologi menunggu verifikasi dokter login | `RadOrder : Read` | Query paging | `ApiResponse<PagedResult<InstructionVerificationItem>>` | **Rencana (belum tersedia)** |

Verb `PUT` mengikuti konvensi transisi yang sudah dipakai kedua controller (`/{id}/complete`, `/{id}/hold`).

### 12.13 Health Services / Medical Record Management / Clinical Document Integrity — "Catatan Saya"

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/my-unsigned` | Konsep milik pengguna login. **Perubahan diminta:** query `serviceContext=Inpatient` hanya mengembalikan konsep di bawah episode rawat inap | `ClinicalDocumentIntegrity : Read` | Query `pageNumber`, `pageSize` **+ `serviceContext`** | `ApiResponse<ResponseUnsignedDocumentPagedResult>` **+ `PatientName`, `MedicalRecordNumber`, `EpisodeNumber`, `ClinicalDateTime`** | **Tersedia**, penyaring **Rencana** |
| `GET` | `/my-authored` | **Catatan terkunci milik penulis** — `Signed` dan `LockedUnsigned` — sebagai jalan masuk addendum setelah penugasan berakhir | `ClinicalDocumentIntegrity : Read` | Query `status`, `serviceContext`, `from`, `to`, paging | `ApiResponse<PagedResult<AuthoredDocumentItem>>` | **Rencana (belum tersedia)** — **menunggu persetujuan Yoga Aji Pratama** (`RWI-DEC-142` butir 3) |

Kedua endpoint menyaring `AuthorUserId` = pengguna login **di server**. Data pasien yang dikembalikan hanya identitas
minimum `RWI-DEC-127` butir (2). Detail catatan dibuka lewat endpoint dokumen pemiliknya, dan detail milik penulis
lain ditolak — `RWI-DEC-142` jalur tidak normal (b). Addendum memakai grup bagian 9 apa adanya; jalur penulis tidak
memeriksa penugasan (`RWI-FACT-028` butir 1).

### 12.14 Health Services / Inpatient Management / Inpatient Discharge — tab Resume Medis, dirancang `episode-rawat-inap`

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{episodeId}/summary` | Resume yang sama dengan `FE-INP-06`. **Perubahan:** tiga isian baru | `InpatientDischarge : Read` | — | `ApiResponse<DischargeSummaryResponse>` **+ `ImportantFindingsSummary`, `DischargeConditionNote`, `EducationSummary`** | **Tersedia**, isian **Rencana** |
| `GET` | `/{episodeId}/summary-prefill` | Usulan isian dari sumber klinis beserta label sumbernya; **tidak menyimpan apa pun** | `InpatientDischarge : Read` | — | `ApiResponse<DischargeSummaryPrefillResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{episodeId}/summary` | Menyimpan draf. **Perubahan:** tiga isian baru | `InpatientDischarge : Update` | Sama **+ 3 isian** | Sama | **Tersedia**, isian **Rencana** |
| `PATCH` | `/{episodeId}/summary/sign` | Menandatangani. Tidak menutup episode | `InpatientDischarge : Sign` | — | Sama | **Tersedia** |
| `GET` | `/{episodeId}/summary` | History Resume — versi yang pernah ditandatangani | `InpatientDischarge : Read` | Query `includeRevisions=true` | `ApiResponse<DischargeSummaryResponse>` beserta daftar versi | **Tersedia** — dipakai apa adanya |

Resume ODC: **tidak ada endpoint** — tab menampilkan "Integrasi belum tersedia" (`RWI-DEC-123`).

### 12.15 Penunjang Medis — empat layanan tanpa endpoint

| Layanan | Endpoint pada rilis ini | Perilaku layar | Dasar |
| --- | --- | --- | --- |
| Gizi / Konsultasi Gizi | **Tidak ada** | Konteks pasien + "Integrasi belum tersedia" | `RWI-DEC-108`, `113`; `NutritionManagement` sudah ada di source tetapi integrasinya ditunda atas jawaban pemilik 15 September 2026 |
| Hemodialisa | **Tidak ada** | Sama | Modul belum ada |
| Bank Darah | **Tidak ada** | Sama | `BloodBankManagement` ada; ditunda seperti Gizi |
| Rehab Medik | **Tidak ada** | Sama | Modul belum ada |

### 12.16 Yang tidak ada di kontrak `0.6.0`

| Yang tidak ada | Alasan |
| --- | --- |
| Endpoint daftar "catatan saya" di `ClinicalManagement` atau `InPatientManagement` | `RWI-DEC-142`, `RWI-AC-211` |
| Endpoint verifikasi resep | `RWI-DEC-121` butir (6) |
| Endpoint menulis resep oleh perawat | `RWI-DEC-114`, `RWI-DEC-116` |
| Aggregator satu endpoint untuk seluruh pesanan yang menunggu verifikasi | Tiga pemilik data; layar menggabungkan tiga daftar, masing-masing dijaga modulnya |
| Endpoint menghitung dosis sliding scale dari hasil laboratorium | `RUL-DOK-03` |
| Endpoint pelaksanaan sliding scale dan MAR | Dirancang `keperawatan` kontrak `0.5.0` |
| Endpoint handover shift dan transfusi | `RWI-DEC-145` butir (4) |
| Endpoint notifikasi "lapor dokter" | Gate `G-24`; penanda tampilan saja |
