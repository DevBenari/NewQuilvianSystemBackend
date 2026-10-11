# IGD — Existing Capability Map

| Field | Value |
|---|---|
| `module` | `igd` |
| `revision` | `3` — diperluas suplemen 3.1–3.6; suplemen **3.6** (*impact scan* Billing IGD) 11 Oktober 2026 pada backend `c7e523fd` / frontend `3335f4b53` |
| `status` | `draft` — audit read-only, belum disetujui siapa pun |
| `audit_date` | 24 Agustus 2026 |
| Backend repository / SHA | `NewQuilvianSystemBackend` / `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` (branch `rizkiG`) |
| Frontend repository / SHA | `QuilvianSystemFrontendDev` / `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| Keadaan working tree backend | Satu berkas dokumen berubah dan belum di-commit: `docs/module-blueprints/igd/00-interview-decisions.md`. **Nol** berkas `.cs` berubah |
| Keadaan working tree frontend | Bersih |
| Decision baseline | `docs/module-blueprints/igd/00-interview-decisions.md`, mencakup `IGD-DEC-001` sampai `IGD-DEC-085`. Keputusan `IGD-DEC-067` sampai `IGD-DEC-085` berstatus `draft` |
| Menggantikan | Revision `2` **seluruhnya** |

---

## 0. Mengapa revision 2 dibuang, bukan ditambal

Revision `2` tidak dapat dipakai lagi. Bukan karena isinya usang saja, melainkan karena
bukti-buktinya **tidak dapat ditelusuri lagi**.

| Masalah pada revision 2 | Bukti |
|---|---|
| Menyebut repository bernama `QuilvianBackend` dan `QuilvianFrontEnd` | Repository yang ada sekarang bernama `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Mengutip path `Repositories/Configurations/HealthService/...` | Folder itu tidak ada. Yang ada `Repositories/Configurations/HealthServices/` — dengan huruf `s` |
| Mencatat SHA backend `fa772b71` | Berjarak **77 commit** dari `HEAD`. **145** berkas `.cs` berubah di antaranya, **49** menyentuh IGD, klinis, atau registrasi |
| SHA pada map berbeda dengan SHA pada manifest | Map menulis `fa772b71`; `blueprint-manifest.md` menulis `e5331a0` untuk map revision `2` yang sama |
| Enam migration masuk setelahnya | `MakeTriageMaxWaitingMinutesNullable`, `initializeLabOrder`, `renameEmployeeRecognitionToHrd`, `AddTriageSlaBreachMarker`, `AllowOutOfQueueScaleTriageLevel`, `AddNosocomialInfection` |

Sembilan belas capability lama `CAP-01` sampai `CAP-19` karena itu ditandai `superseded`
seluruhnya. Penomoran pada revision `3` dimulai ulang dan tidak dapat dibandingkan satu-satu
dengan penomoran lama.

Isi revision `2` **tidak dihapus**. Salinannya disimpan di
[`archive/01-existing-capability-map-rev2.md`](./archive/01-existing-capability-map-rev2.md)
sehingga hash lama tetap dapat dicocokkan.

### 0.1 Yang harus diperbaiki pada `blueprint-manifest.md`

Manifest revision `4` masih menunjuk capability map revision `2`. Tiga field di bawah menjadi
tidak benar sejak berkas ini ditulis, dan **wajib** diperbarui saat blueprint naik revisi:

| Field pada manifest | Nilai lama | Yang benar sekarang |
|---|---|---|
| `input_revisions` | `01-existing-capability-map.md` revision `2` | revision `3` |
| `input_hashes` | capability map `sha256:ee02f069…` | Hash berkas ini, dihitung ulang saat revisi manifest |
| Tabel `Artifact hashes` baris capability map | `ee02f0697226da3de9b6046a28a86594498a520b8a6a7b6843321f00e3d8da51` | Hash berkas ini |

Pembaruan manifest **bukan** pekerjaan audit ini. Ia dikerjakan saat blueprint disusun ulang,
supaya seluruh hash dihitung sekali pada keadaan akhir, bukan sepotong-sepotong.

---

## 1. Cara audit ini dikerjakan dan batasnya

**Yang dikerjakan.** Seluruh `Areas/` disurvei sebagai inventaris utuh lebih dulu, bukan
dicari dengan kata kunci. Barulah setiap kebutuhan pada decision log ditelusuri ke entity,
konfigurasi EF, migration, controller, service, pendaftaran DI, izin akses, test, dan
pemakainya di frontend.

**Mengapa inventaris utuh.** Pada tahap wawancara 24 Agustus, pencarian berbasis kata kunci
dijalankan dan **melewatkan dua hal yang jelas-jelas ada**: modul `LaboratoryManagement` dan
proyek `QuilvianSystemBackend.Tests`. Keduanya dikoreksi pada bagian 8. Audit ini karena itu
tidak memakai metode yang sama.

**Yang tidak dikerjakan.** Aplikasi tidak dibangun, tidak dijalankan, dan basis data tidak
disentuh. Tidak ada perilaku runtime yang diverifikasi. Seluruh pernyataan di bawah adalah
pembacaan source pada SHA di atas.

**Batas ketelitian.** Isi tabel basis data pengembangan tidak diperiksa. Pernyataan seperti
"master belum terisi" berasal dari kode seeder, bukan dari isi basis data.

---

## 2. Inventaris menyeluruh

Backend memiliki **440 entity** dan **247 controller** di seluruh `Areas/`. Yang relevan bagi
IGD ada di `Areas/HealthServices/`:

| Area | Jumlah | Entity |
|---|---:|---|
| `RegistrationManagement` | 4 | `TrxKioskScanSession`, `TrxPatientEncounter`, `TrxPatientEncounterGuarantor`, `TrxQueue` |
| `EmergencyInstallationManagement` | 9 | `TrxEmergencyVisit`, `TrxEmergencyTriage`, `TrxEmergencyTriageDetail`, `TrxEmergencyObservation`, `TrxEmergencyObservationDetail`, `TrxEmergencyResuscitation`, `TrxEmergencyProcedureDetail`, `TrxEmergencyDisposition`, `TrxEmergencyTransfer` |
| `ClinicalManagement` | 14 | `TrxPatientAssessment`, `TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote`, `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `TrxPatientProcedure`, `TrxPatientAllergy`, `TrxPatientMedicalHistory`, `TrxPatientFamilyHistory`, `TrxNosocomialInfection`, `TrxPatientConsent`, `TrxPatientClinicalDocument`, `TrxClinicalNoteAttachment`, `TrxMedicalCertificate` |
| `LaboratoryManagement` | 1 | `LabOrder` |
| `PharmacyManagement` | 15 | `TrxPrescription` beserta item, racikan, telaah, penyiapan, klarifikasi, substitusi, dan lima master template |
| `PatientManagement/MasterData` | 7 | `MstPatient`, `MstPatientInsurance`, `MstPatientCompanyGuarantor`, `MstPatientEmergencyContact`, `MstPatientIdentityDocument`, `MstPatientMembership`, `MstPatientRelationship` |
| `MasterData` | 32 | termasuk `MstServiceUnit`, `MstRoom`, `MstBed`, `MstPatientClass`, `MstProcedure`, dan enam master IGD |
| `BillingManagement/MasterData` | 2 | `MstBillingItemCategory`, `MstPaymentMethod` |

**Area yang dicari dan terbukti tidak ada:** `InpatientManagement`, `InPatientManagement`,
`RadiologyManagement`, `DiagnosticServices`.

---

## 3. Capability Register

Klasifikasi: `Ready to reuse`, `Reuse with adapter`, `Extend`, `Repair`, `Missing`,
`Conflict`, `Unknown`.

Seluruh bukti berformat `repository — path:baris (simbol), commit`. Commit backend
`f69e9e48`, commit frontend `96a91201`, kecuali disebut lain.

### 3.1 Pendaftaran dan kunjungan

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-01` | Kunjungan pasien sebagai jangkar episode | Registration Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs:15,26,29 (TrxPatientEncounter.Id/PatientId/ServiceUnitId)` | Nomor kunjungan unik ada, relasi lengkap |
| `IGD-CAP-02` | Kunjungan IGD sebagai perluasan kunjungan | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:15-99 (TrxEmergencyVisit)` | Satu kunjungan hanya boleh punya satu kunjungan IGD, dijaga index unik |
| `IGD-CAP-03` | Jenis kunjungan `Emergency` | Registration Management | `Conflict` | Enum menyediakannya: `NewQuilvianSystemBackend — Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs (Emergency = 2)`. Validasi menolaknya: `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:97-98` dan `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs:525-526` | Nilai `Emergency` **tidak pernah ditulis** oleh satu jalur pun. Aturan ditulis dua kali. Lihat `IGD-CONF-01` |
| `IGD-CAP-04` | Penghubung kunjungan IGD ke kunjungan rawat inap | Registration Management | `Missing` | `NewQuilvianSystemBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs` tidak memuat kolom penunjuk kunjungan lain | Dibutuhkan `IGD-DEC-075` dan `RWI-RULE-029` aturan 2 |
| `IGD-CAP-05` | Kelas pasien untuk kunjungan IGD | Registration Management + Master Data | `Extend` | Penanda sudah ada: `NewQuilvianSystemBackend — Areas/HealthServices/MasterData/Models/MstPatientClass.cs (IsForEmergency)`. Penetapannya belum memakainya: `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:55 (DefaultOutpatientPatientClassName = "RAWAT JALAN")`, `:1470 (ResolvePatientClassAsync)` | Kolom `IsForEmergency` sudah dirancang tetapi belum dipakai. Penetapan rawat jalan memakai nama tertulis-tetap |
| `IGD-CAP-06` | Penjamin pada kunjungan | Registration Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs` | Satu kunjungan satu sumber pembayaran |
| `IGD-CAP-07` | Pasien tanpa identitas | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs:59-63 (IsUnknownPatient, TemporaryPatientAlias)` | Diizinkan bila setting mengizinkan |
| `IGD-CAP-08` | Pencegahan episode IGD ganda untuk satu pasien | Emergency Installation Management | `Missing` | Yang dijaga hanya satu encounter satu kunjungan IGD: `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs:104-116` | Dibutuhkan `IGD-DEC-084` |
| `IGD-CAP-09` | Antrean untuk pasien IGD | Registration Management | `Missing`, dan memang disengaja | Tidak ada jalur yang membuat `TrxQueue` untuk IGD | `IGD-DEC-068` memilih **tidak** membuat antrean semu |

### 3.2 Triase

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-10` | Penilaian triase beserta indikator | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs:15-120`, `Models/TrxEmergencyTriageDetail.cs` | Salinan nilai master tersimpan pada saat penilaian dibuat |
| `IGD-CAP-11` | Penilaian ulang bersifat tambah, bukan timpa | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTriageService.cs (RetriageAsync)` | Satu transaksi; baris lama menjadi `Superseded`; baris baru menunjuk baris lama. **Inilah pola rujukan untuk `IGD-DEC-080`** |
| `IGD-CAP-12` | Target waktu respons yang belum disahkan dibiarkan kosong | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTriage.cs (MaxWaitingMinutesSnapshot, ResponseDueAt)` | Tidak pernah dianggap nol menit |
| `IGD-CAP-13` | Penandaan pelampauan batas waktu | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyTriageService.cs (MarkSlaBreachesAsync, GetSlaBreachesAsync)`; `Services/EmergencyTriageSlaMonitorHostedService.cs`; terdaftar di `Program.cs:302-304` | Idempoten. **Pola rujukan untuk daftar pantau `IGD-DEC-083`** |
| `IGD-CAP-14` | Status kunjungan mengikuti aturan transisinya | Emergency Installation Management | `Repair` | Aturan benar: `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs (CanTransition)`. Dilewati: `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyTriageController.cs` menulis `visit.VisitStatus = Triaged` langsung pada `Create` dan `UpdateTriageStatus` | Status dapat mundur; kunjungan `Disposed` dapat terbuka lagi. `IGD-GAP-014` |
| `IGD-CAP-15` | Penetapan dokter setelah triase | Registration Management | `Extend` | `NewQuilvianSystemBackend — Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs:929-977 (AssignEncounterDoctor)`; dipakai `QuilvianSystemFrontendDev — src/lib/state/slice/health-services/emergency-installation-management/emergency-management-triage-slice.jsx:521-541` | Hanya menimpa satu kolom. Tanpa riwayat, waktu, alasan, penerimaan |
| `IGD-CAP-16` | Riwayat penugasan dokter pada satu pasien | — | `Missing` | Pencarian entity bernama `Dpjp`, `AttendingDoctor`, `DoctorAssignment`, `ResponsibleDoctor` di seluruh `Areas/` menghasilkan nol hasil | Dibutuhkan `IGD-DEC-082` |

### 3.3 Pencatatan klinis

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-17` | Pengkajian pasien | Clinical Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs (QueueId wajib)`; `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs:138-144 (CreatePatientAssessmentRequest, [Required] QueueId)` | IGD tidak punya antrean, sehingga pengkajian IGD **tidak dapat dibuat**. Lihat `IGD-CONF-02` |
| `IGD-CAP-18` | Konsultasi dokter | Clinical Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs:24-25 ([Required] QueueId)` | Sama; menjadi akar terkuncinya diagnosis, tindakan, dan resep |
| `IGD-CAP-19` | Diagnosis pasien | Clinical Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs:20-21 ([Required] ConsultationId)` | Terkunci di balik konsultasi |
| `IGD-CAP-20` | Tindakan pasien | Clinical Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:20-21 ([Required] ConsultationId)` | Terkunci. Berakibat pula pada `IGD-CAP-27` |
| `IGD-CAP-21` | Tanda vital | Clinical Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/DTOs/PatientVitalSignDtos.cs:217-227 (QueueId dan ConsultationId keduanya boleh kosong)` | **Tidak terkunci.** Sudah dipakai layar IGD |
| `IGD-CAP-22` | Catatan perkembangan terintegrasi | Clinical Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/DTOs/PatientIntegratedProgressNoteDtos.cs:154-166 (QueueId dan ConsultationId boleh kosong)` | **Tidak terkunci.** Sudah dipakai layar IGD untuk SOAP |
| `IGD-CAP-23` | Infeksi nosokomial | Clinical Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxNosocomialInfection.cs (EmergencyVisitId nullable)`; migration `Migrations/20260821063311_AddNosocomialInfection.cs`; konfigurasi `Repositories/Configurations/HealthServices/TrxNosocomialInfectionConfiguration.cs` | Sudah mengenal kunjungan IGD secara langsung |
| `IGD-CAP-24` | Riwayat versi catatan klinis | Clinical Management | `Missing` | Basis audit hanya `NewQuilvianSystemBackend — Models/IdentityModel.cs (CreateBy, UpdateBy, DeleteBy, CancelBy)`. `LoggerService.AuditAsync` menulis ke Serilog, bukan tabel: `Services/Logging/LoggerService.cs:34-36` | Hanya penulis terakhir tersimpan. Dibutuhkan `IGD-DEC-080` |
| `IGD-CAP-25` | Alergi pasien | Clinical Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs` | Sumber untuk bagian otomatis `IGD-DEC-079` |

### 3.4 Observasi, resusitasi, tindakan IGD

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-26` | Observasi dan rinciannya | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyObservation.cs`, `Models/TrxEmergencyObservationDetail.cs`; `Services/EmergencyObservationService.cs` | Dimiliki IGD sepenuhnya |
| `IGD-CAP-27` | Rincian tindakan IGD | Emergency Installation Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyProcedureDetail.cs ([Required] PatientProcedureId)` | Bergantung `IGD-CAP-20` yang terkunci |
| `IGD-CAP-28` | Resusitasi | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyResuscitation.cs`; `Services/EmergencyResuscitationService.cs` | Dimiliki IGD sepenuhnya |

### 3.5 Penunjang dan obat

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-29` | Pemesanan laboratorium | Laboratory Management | `Extend` | `NewQuilvianSystemBackend — Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs (EncounterId, ProcedureId)`; `Controllers/LabOrderController.cs:13,33-108`; migration `Migrations/20260815103436_initializeLabOrder.cs`; commit asal `1a8a9ce` | **Ada dan tidak menuntut `ConsultationId`**, sehingga sudah dapat dipakai IGD hari ini. Tanpa status, hasil, spesimen, dokter pemesan, prioritas, dan nilai kritis |
| `IGD-CAP-30` | Pemesanan radiologi | — | `Missing` | Tidak ada area `RadiologyManagement` maupun `DiagnosticServices` | |
| `IGD-CAP-31` | Peresepan obat | Pharmacy Management | `Conflict` | `NewQuilvianSystemBackend — Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs:27-28 ([Required] ConsultationId)` | Terkunci di balik konsultasi |
| `IGD-CAP-32` | Catatan pemberian obat kepada pasien | — | `Missing` | Lima belas entity Farmasi mencakup resep, telaah, racikan, penyiapan, dan penyerahan; tidak satu pun mencatat pemberian ke pasien | Diperlukan `IGD-DEC-078` untuk membedakan "diserahkan" dari "diberikan" |

### 3.6 Tindak lanjut, kepergian, dan rawat inap

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-33` | Keputusan tindak lanjut | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyDisposition.cs`; `Services/EmergencyDispositionService.cs` | Draft, Confirmed, Executed, Cancelled |
| `IGD-CAP-34` | Penanda jenis tindak lanjut yang menutup kunjungan | Master Data | `Repair` | Kolom ada: `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/MasterData/Models/MstEmergencyDispositionType.cs (ClosesEmergencyVisit)`. Tidak pernah dibaca untuk memutuskan: hanya muncul pada `Controllers/EmergencyDispositionController.cs:481` sebagai isi balasan | Ketujuh jenis diisi `true` oleh seeder |
| `IGD-CAP-35` | Gerbang penutupan kunjungan | Emergency Installation Management | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs (ValidateVisitClosureAsync)` | Menolak observasi aktif dan perpindahan belum tuntas. Belum memeriksa pesanan |
| `IGD-CAP-36` | Catatan kepergian pasien dari IGD | Emergency Installation Management | `Extend` | `NewQuilvianSystemBackend — Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyTransfer.cs` | Enam status tidak cukup memisahkan jalur fisik dan dokumen; `DepartedAt` dan `ArrivedAt` tidak pernah diisi endpoint mana pun |
| `IGD-CAP-37` | Kolom tempat tidur dan ruangan pada kepergian | Emergency Installation Management | `Repair` | `NewQuilvianSystemBackend — Repositories/Configurations/HealthServices/EmergencyInstallationManagement/TrxEmergencyTransferConfiguration.cs:29-32` memberi index pada `FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId` tanpa satu pun `HasOne` | Tanpa foreign key dan tanpa navigation. `IGD-DEC-069` mencabutnya |
| `IGD-CAP-38` | Master tempat tidur dan ruangan | Master Data | `Ready to reuse` | `NewQuilvianSystemBackend — Areas/HealthServices/MasterData/Models/MstBed.cs (RoomId, BedStatus)`; `Models/MstRoom.cs:14,16 (ServiceUnitId, PatientClassId)` | Rantai tempat tidur ke ruangan ke unit sudah utuh. Dipakai Rawat Inap, bukan IGD |
| `IGD-CAP-39` | Episode rawat inap, permintaan rawat inap, alokasi tempat tidur | Inpatient Management | `Missing` | Area `InpatientManagement` tidak ada di source. Blueprint `RWI-BP-001` revision `3` berstatus `approved` tetapi belum diimplementasikan | Bukan pekerjaan IGD |

### 3.7 Kewenangan, audit, dan uji

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-40` | Izin akses per resource | Administrator | `Ready to reuse` | Sepuluh resource terdaftar lewat atribut: `EmergencyVisit`, `EmergencyTriage`, `EmergencyTriageDetail`, `EmergencyObservation`, `EmergencyObservationDetail`, `EmergencyResuscitation`, `EmergencyProcedureDetail`, `EmergencyDisposition`, `EmergencyTransfer`, `LabOrder` | Berbasis peran terhadap endpoint |
| `IGD-CAP-41` | Kewenangan yang mengenal unit pelayanan | Administrator + Corporate/HR | `Missing` | Dua puluh enam entity penugasan, roster, dan shift diperiksa; **nol** memuat `ServiceUnitId`. `MstServiceUnit.cs` tidak memuat kolom ke organisasi. `MstOrganizationUnit`, `MstWorkLocation`, dan `MstHospitalSite` ada tetapi **nol** menyebut `ServiceUnit` | Sisi organisasi lebih kaya daripada dugaan `BE-IGD-010`. Lihat `IGD-TRQ-03` |
| `IGD-CAP-42` | Jejak audit perubahan per baris | — | `Missing` | Lihat `IGD-CAP-24` | |
| `IGD-CAP-43` | Prasarana uji backend | — | `Extend` | `NewQuilvianSystemBackend — QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj` (xunit 2.9.2, EFCore.InMemory 9.0.18, net9.0), terdaftar di `QuilvianSystemBackend.sln`, commit asal `504a90a`. Isi: `QuilvianSystemBackend.Tests/BillingManagement/BillingModuleFoundationTests.cs`, `IsolatedBillingDbContextFactory.cs:6-11` | **Prasarana ada dan berjalan.** Cakupan hanya Billing. Nol test untuk IGD, klinis, registrasi, farmasi |
| `IGD-CAP-44` | Prasarana uji frontend | — | `Ready to reuse` | `QuilvianSystemFrontendDev — package.json:20 (test:unit -> node --test tests/unit/)`; tujuh berkas di `tests/unit/` dan `tests/e2e/` | Tiga di antaranya khusus IGD |
| `IGD-CAP-45` | Realtime untuk IGD | Registration Management | `Missing` | Hanya `NewQuilvianSystemBackend — Hubs/QueueHub.cs`. Tidak ada rujukan hub di `Areas/HealthServices/EmergencyInstallationManagement/` | Daftar pantau IGD hari ini tidak realtime |

### 3.8 Frontend

| ID | Capability | Owner | Klasifikasi | Bukti | Catatan |
|---|---|---|---|---|---|
| `IGD-CAP-46` | Layar pendaftaran IGD | Frontend | `Extend` | `QuilvianSystemFrontendDev — src/app/health-services/registration-management/emergency-registration/page.jsx`; `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js:909-1035` | Tiga panggilan berurutan, tidak atomik |
| `IGD-CAP-47` | Layar triase | Frontend | `Ready to reuse` | `QuilvianSystemFrontendDev — src/app/health-services/emergency-installation-management/emergency-triage/page.jsx` dan `[slug]/page.jsx` | |
| `IGD-CAP-48` | Layar pengkajian IGD | Frontend | `Extend` | `QuilvianSystemFrontendDev — src/app/health-services/emergency-installation-management/emergency-assessment/page.jsx` dan `[slug]/page.jsx`; sembilan tab di `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx:89-125` | Tab "Assesmen Awal IGD" dan "Resep" hanya membaca |
| `IGD-CAP-49` | Konstanta jenis kunjungan di frontend | Frontend | `Ready to reuse` | `QuilvianSystemFrontendDev — src/lib/constants/health-services/registration-management/emergency-management/emergency-registration.constants.js:127-134 (ENCOUNTER_TYPE)` | `Emergency: 2` sudah tersedia |
| `IGD-CAP-50` | Layar rawat inap | Frontend | `Missing` | Tidak ada route rawat inap | Bukan pekerjaan IGD |

---

## 4. Reuse dan ownership

Aturan yang dipatuhi audit ini: **jangan pernah menyalin pasien, dokter, pegawai, kunjungan,
asuransi, tindakan, resep, tempat tidur, atau master bersama ke dalam IGD.**

| Yang dibutuhkan IGD | Dipakai ulang dari | Pemilik tetap | Jangan dibuat tandingannya |
|---|---|---|---|
| Data pasien | `MstPatient` | Patient Management | Tabel pasien IGD |
| Dokter | `MstDoctor` | Master Data | Tabel dokter IGD |
| Kunjungan | `TrxPatientEncounter` | Registration Management | Kunjungan khusus IGD |
| Penjamin | `TrxPatientEncounterGuarantor`, `MstPatientInsurance` | Registration + Patient Management | Penjamin IGD |
| Pengkajian, tanda vital, catatan, diagnosis, tindakan | `ClinicalManagement` | Clinical Management | Tabel klinis IGD |
| Resep | `TrxPrescription` | Pharmacy Management | Resep IGD |
| Laboratorium | `LabOrder` | Laboratory Management | Pemesanan lab IGD |
| Tempat tidur dan ruangan | `MstBed`, `MstRoom` | Master Data | Tempat tidur IGD |
| Unit pelayanan | `MstServiceUnit` | Master Data | Unit IGD |

Entity yang **memang milik IGD** dan boleh diperluas: sembilan `TrxEmergency*` beserta enam
`MstEmergency*`.

---

## 5. Kontrak as-is versus to-be

Bagian ini memisahkan **apa yang berlaku sekarang** dari **apa yang diminta keputusan
`draft`**. Yang to-be belum berlaku dan belum boleh dijadikan dasar implementasi.

### 5.1 Kontrak as-is yang terkunci oleh test

Satu test frontend **mengunci perilaku yang hendak diubah** `IGD-DEC-074`:

```
QuilvianSystemFrontendDev — tests/unit/emergency-registration-payload.test.mjs:24-35
  test("FE-IGD-001 K1: payload encounter mengirim Outpatient, bukan Emergency")
    assert.equal(payload.encounterType, ENCOUNTER_TYPE.Outpatient);
    assert.notEqual(payload.encounterType, ENCOUNTER_TYPE.Emergency);
```

Komentar di dalamnya berbunyi: *"Backend hanya menerima Outpatient. Mengirim Emergency
membuat panggilan kedua ditolak dan meninggalkan encounter menggantung."*

Artinya `npm test` **akan gagal** begitu `IGD-DEC-074` dijalankan, kecuali test ini ikut
diperbarui dalam task yang sama. Ini bukan halangan, melainkan jaring pengaman yang bekerja
sebagaimana mestinya.

### 5.2 Perbandingan as-is dan to-be

| Aspek | As-is (berlaku sekarang) | To-be (keputusan `draft`) |
|---|---|---|
| Jenis kunjungan IGD | `Outpatient`, ditolak bila lain | `Emergency` — `IGD-DEC-074` |
| Kelas pasien IGD | Diambil dari nama master `RAWAT JALAN` | Dari penanda `IsForEmergency` — `IGD-DEC-076` |
| Penghubung IGD ke rawat inap | Tidak ada | Kolom penunjuk kunjungan asal — `IGD-DEC-075` |
| Pengkajian IGD | Tidak dapat dibuat | Dapat dibuat tanpa antrean — `IGD-DEC-068` |
| Diagnosis, tindakan, resep IGD | Tidak dapat dibuat | Dapat dibuat tanpa konsultasi antrean — `IGD-DEC-068` |
| Koreksi catatan klinis | Menimpa baris | Tambah baris, baris lama utuh — `IGD-DEC-080` |
| Status kepergian | Satu rangkaian enam nilai | Dua rangkaian: fisik dan dokumen — `IGD-DEC-070` |
| Tempat tidur pada kepergian | Empat kolom tanpa foreign key | Dicabut; milik Rawat Inap — `IGD-DEC-069` |
| Isi serah terima | Satu kolom teks bebas | Empat bagian SBAR + tiga bagian otomatis — `IGD-DEC-079` |
| Penetapan dokter | Menimpa satu kolom | Riwayat penugasan — `IGD-DEC-082` |
| Kewenangan unit | Tidak ada | Tabel penugasan + penjaga di service — `IGD-DEC-081` |
| Pesanan saat pasien pergi | Tidak diperiksa | Wajib bersikap, tidak menahan kepergian — `IGD-DEC-078` |
| Episode ganda | Tidak dicegah | Ditolak dengan penunjuk kunjungan yang ada — `IGD-DEC-084` |
| `ClosesEmergencyVisit` | Tidak pernah dibaca | Menjadi penentu perilaku — `IGD-DEC-067` |

---

## 6. Conflict

| ID | Conflict | Tingkat | Bukti | Terkait |
|---|---|---|---|---|
| `IGD-CONF-01` | Aturan jenis kunjungan IGD **ditulis dua kali** di tempat berbeda. Mengubah satu saja mengulang cacat `BE-IGD-016` | `HIGH` | `Services/EmergencyVisitService.cs:97-98` dan `Controllers/EmergencyVisitController.cs:525-526` | `IGD-DEC-074` |
| `IGD-CONF-02` | `RWI-RULE-026` aturan 6 melarang perilaku IGD berubah, sedangkan pembatas yang dilonggarkannya adalah penyebab pengkajian IGD tidak dapat disimpan | `CRITICAL` | `IGD-CAP-17`, `IGD-CAP-18` versus `rawat-inap/00-interview-decisions.md` `RWI-RULE-026` | `IGD-DEC-068`, `IGD-CONFLICT-004` |
| `IGD-CONF-03` | Test frontend mengunci `Outpatient` dan menolak `Emergency` | `MEDIUM` | `tests/unit/emergency-registration-payload.test.mjs:24-35` | `IGD-DEC-074` |
| `IGD-CONF-04` | Blueprint Rawat Inap menjanjikan nol perubahan kolom pada tabel modul lain, sedangkan aturannya sendiri menuntut penghubung antar kunjungan | `HIGH` | `rawat-inap/blueprint-manifest.md` field `compatibility_impact` versus `RWI-RULE-029` aturan 2 | `IGD-DEC-075`, `IGD-CONFLICT-005` |
| `IGD-CONF-05` | Status kunjungan dapat mundur dan kunjungan tertutup dapat terbuka kembali | `CRITICAL` | `IGD-CAP-14` | `IGD-GAP-014` |

---

## 7. Unknown

| ID | Yang belum diketahui | Mengapa tidak dapat dijawab audit ini |
|---|---|---|
| `IGD-UNK-01` | Apakah enam master IGD dan master kelas pasien IGD sudah terisi di basis data pengembangan | Audit read-only terhadap source; isi basis data tidak diperiksa |
| `IGD-UNK-02` | Apakah ada data kunjungan IGD lama yang `EncounterId`-nya kosong sehingga tidak terjangkau migration `IGD-DEC-074` | Perlu kueri ke basis data |
| `IGD-UNK-03` | Berapa banyak baris `TrxEmergencyTransfer` yang sudah mengisi empat kolom tempat tidur, sehingga pencabutannya kehilangan data | Perlu kueri ke basis data |
| `IGD-UNK-04` | Apakah `LabOrder` sudah dipakai di produksi atau masih rintisan yang belum tersambung layar | Tidak ditemukan pemakainya di frontend; belum tentu berarti tidak dipakai |
| `IGD-UNK-05` | Apakah `TrxShiftAssignment` dan `WfpOrganizationAssignment` benar-benar terisi untuk petugas IGD | Perlu kueri ke basis data |

---

## 8. Koreksi terhadap temuan wawancara 24 Agustus

Audit ini menemukan **dua pernyataan salah** pada Amendment Pass 2026-08-24 di
`00-interview-decisions.md`. Keduanya berasal dari pencarian berbasis kata kunci yang tidak
menemukan folder yang jelas-jelas ada.

| Butir | Pernyataan yang salah | Yang benar | Akibat |
|---|---|---|---|
| `F-17` | "Permintaan laboratorium dan radiologi belum ada dalam bentuk apa pun" | `LaboratoryManagement` **ada** dengan entity, controller, service, DTO, konfigurasi, dan migration. Radiologi memang belum ada | `IGD-GAP-024` dipecah menjadi `024a` laboratorium `EXTEND_EXISTING` dan `024b` radiologi `MISSING_NEW`. Sudah dikoreksi di decision log |
| `IGD-GAP-033` | "Backend tidak punya proyek test; tidak satu pun `AT-IGD-*` dapat dijalankan" | Proyek `QuilvianSystemBackend.Tests` **ada**, memakai xunit dan EFCore InMemory, terdaftar di solution, dengan pola `IsolatedBillingDbContextFactory` yang dapat dipakai ulang. Frontend juga punya tujuh berkas test, tiga di antaranya khusus IGD | Blokernya berubah dari "membangun prasarana uji" menjadi "memperluas cakupan". Jauh lebih murah |

Catatan memori proyek yang menyatakan "backend tidak punya proyek test" juga sudah tidak
berlaku.

**Pelajaran yang perlu dicatat.** Discovery yang dikerjakan sambil lalu di dalam pass
wawancara bukan pengganti audit kemampuan. Dua kali kesalahan pada satu pass menunjukkan
metode pencarian berbasis kata kunci tidak memadai untuk menyatakan sesuatu **tidak ada**.
Pernyataan negatif harus berasal dari inventaris menyeluruh, seperti pada bagian 2.

---

## 9. Closure question untuk `/grill-me`

Audit tidak menjawab pertanyaan ini. Seluruhnya diserahkan kepada pass wawancara berikutnya.

| ID | Pertanyaan | Memblokir | Dasar |
|---|---|---|---|
| `IGD-TRQ-01` | `LabOrder` sudah ada dan tidak menuntut konsultasi, sehingga pemesanan laboratorium IGD sebenarnya dapat berjalan hari ini. Apakah IGD memakainya apa adanya lebih dulu, atau menunggu `LabOrder` dilengkapi status, hasil, spesimen, dan nilai kritis? | `DESIGN` untuk cakupan pesanan pada `IGD-DEC-078` | `IGD-CAP-29` |
| `IGD-TRQ-02` | Siapa pemilik modul `LaboratoryManagement`, dan apakah pelengkapan `LabOrder` menjadi pekerjaan IGD atau pekerjaan pemiliknya? | `IMPLEMENTATION` | `IGD-CAP-29` |
| `IGD-TRQ-03` | Sisi organisasi sudah memiliki `MstOrganizationUnit`, `MstWorkLocation`, dan penugasan pegawai, tetapi `MstServiceUnit` tidak terhubung ke satu pun di antaranya. Apakah `IGD-DEC-081` tetap membuat tabel penugasan pengguna ke unit pelayanan tersendiri, atau cukup menambah jembatan dari `MstServiceUnit` ke `MstOrganizationUnit` lalu menurunkan kewenangan dari penugasan pegawai yang sudah ada? | `DESIGN` untuk `IGD-DEC-081` | `IGD-CAP-41` |
| `IGD-TRQ-04` | Test `FE-IGD-001 K1` mengunci `Outpatient`. Apakah pembaruannya masuk task yang sama dengan `IGD-DEC-074`, dan apakah ada pemakai lain di luar repositori ini yang bergantung pada jenis kunjungan IGD bernilai `Outpatient`? | `IMPLEMENTATION` | `IGD-CONF-03` |
| `IGD-TRQ-05` | Empat kolom tempat tidur dan ruangan pada `TrxEmergencyTransfer` akan dicabut. Bila ternyata sudah terisi data, apakah nilainya dipindahkan, diarsipkan, atau dibuang? | `IMPLEMENTATION` | `IGD-CAP-37`, `IGD-UNK-03` |
| `IGD-TRQ-06` | Cakupan uji minimum apa yang harus dipenuhi sebelum pelonggaran `IGD-DEC-068` boleh menyentuh `ClinicalManagement`, mengingat prasarana uji sudah ada tetapi jalur rawat jalan belum punya satu pun test? | `IMPLEMENTATION` | `IGD-CAP-43`, `RWI-DEC-051` |
| `IGD-TRQ-07` | Daftar pantau `IGD-DEC-083` dan daftar pelampauan batas waktu triase yang sudah ada tidak realtime. Apakah IGD memerlukan pembaruan langsung lewat `QueueHub` atau cukup muat ulang berkala? | `LATER SLICE` | `IGD-CAP-45` |

---

## 10. Pemicu audit ulang

Map ini menjadi tidak sahih bila salah satu terjadi:

1. SHA backend bergerak dari `f69e9e48` dengan perubahan pada berkas `.cs` mana pun di
   `Areas/HealthServices/`, `Repositories/`, atau `Migrations/`;
2. SHA frontend bergerak dari `96a91201` dengan perubahan di `src/lib/state/`,
   `src/lib/services/`, atau `src/app/health-services/`;
3. Migration baru diterapkan ke basis data bersama;
4. Blueprint `RWI-BP-001` naik revisi dan mengubah `RWI-RULE-026` atau `RWI-RULE-029`.

Pemeriksaan kesahihan cukup dengan membandingkan SHA dan menjalankan pemindaian dampak
terbatas pada berkas yang berubah, bukan mengulang seluruh audit.

---

# Suplemen revision 3.1 — audit terarah `EmergencyTransfer` pada `300922c`

Ditambahkan 26 Agustus 2026 atas permintaan Product/Domain Owner, sebagai **correction pass**
revisi 6. Bukan pengganti revision `3` yang tetap berlaku untuk area lain, dan **tetap stale**
untuk area yang tidak diaudit di sini.

Alasan audit: `IGD-DEC-091` memutuskan penggantian nama `TrxEmergencyTransfer` menjadi
`TrxEmergencyDeparture`. Bukti yang mendasarinya hanya menghitung pemanggil **frontend** dan
berkas backend yang menyebut namanya. Itu **tidak cukup** — penggantian nama menyentuh model,
controller, DTO, service, `DbContext`, migration, dan kemungkinan consumer di modul lain.

## S3.1.1 Footprint sebenarnya

| Berkas | Baris | Peran |
| --- | ---: | --- |
| `Controllers/EmergencyTransferController.cs` | 536 | 6 endpoint |
| `DTOs/EmergencyTransferDtos.cs` | 122 | Request dan response |
| `Services/EmergencyTransferService.cs` | 97 | Validasi dan `CanTransition` |
| `Models/TrxEmergencyTransfer.cs` | 81 | 22 kolom skalar, 7 navigasi |
| `Enums/EmergencyTransferStatus.cs` | 12 | 6 nilai |
| `Repositories/Configurations/…/TrxEmergencyTransferConfiguration.cs` | 70 | Pemetaan EF |
| **Total** | **918** | |

Ditambah tiga titik singgung:

| Tempat | Isi |
| --- | --- |
| `Repositories/ApplicationDbContext.cs:673` | `DbSet<TrxEmergencyTransfer> TrxEmergencyTransfers` |
| `Program.cs:321` | `builder.Services.AddScoped<EmergencyTransferService>()` |
| `Migrations/20260804071642_initializeEmergencyInstallationManagement.cs` | Migration yang membuat tabelnya |

## S3.1.2 Enam endpoint yang berganti route

| Method | Path | Nasib pada revisi 6 |
| --- | --- | --- |
| `GET` | `/` | Berganti route; penyaring bertambah dua rangkaian status |
| `GET` | `/{id}` | Berganti route; response memuat kejadian dan daftar pesanan |
| `POST` | `/` | Berganti route; empat field tempat tidur dan ruangan **dihapus** |
| `PUT` | `/{id}` | Berganti route |
| `PATCH` | `/{id}/transfer-status` | **Dihapus.** Digantikan aksi bernama `depart`, `arrive`, `accept-handover`, `reject-handover` |
| `DELETE` | `/{id}` | Berganti route |

## S3.1.3 Consumer di luar IGD — **nol**, dan satu positif palsu

Penelusuran `EmergencyTransfer` di seluruh `Areas/` menemukan dua berkas `BillingManagement`:

| Berkas | Isi | Terdampak? |
| --- | --- | :-: |
| `Billing/Models/BilFinalizationRecord.cs:32` | `public const string EmergencyTransfer = "EMERGENCY_TRANSFER";` | **Tidak** |
| `Billing/Services/BillingFinalizationService.cs:290` | Memvalidasi `DepartureReason` bernilai `EMERGENCY_TRANSFER` | **Tidak** |

Keduanya **string konstanta**, bukan rujukan ke tabel maupun enum IGD. Penggantian nama tabel
tidak memutus satu pun.

> **Tabrakan kosakata yang perlu diketahui.** Billing sudah memakai kata *departure* untuk hal
> yang berbeda: `DepartureReason` di sana berarti **alasan pasien pergi dengan tagihan belum
> lunas** — `DEATH`, `EMERGENCY_TRANSFER`, `DAMA`. Setelah `IGD-DEC-091`, IGD memakai
> *departure* untuk **catatan kepergian pasien dari IGD**. Dua konsep berbeda dengan satu kata.
> Tidak memblokir, tetapi wajib disebut pada dokumen agar tidak tertukar saat billing IGD
> dirancang.

## S3.1.4 Dua consumer **di dalam** IGD yang wajib ikut berubah

| Tempat | Isi | Akibat |
| --- | --- | --- |
| `Models/TrxEmergencyVisit.cs:98` | `ICollection<TrxEmergencyTransfer> Transfers` | Nama properti navigasi ikut berganti; setiap pembaca `visit.Transfers` terdampak |
| `Services/EmergencyDispositionService.cs:124–130` | Gerbang penutupan kunjungan membaca `TransferStatus != Completed && != Rejected` | **Paling berisiko.** Gerbang ini menentukan boleh-tidaknya kunjungan ditutup, dan bertumpu pada enum yang akan dipecah menjadi dua |

## S3.1.5 Enam nilai enum lama yang wajib dipetakan

`EmergencyTransferStatus`: `Requested`=1, `Accepted`=2, `InTransit`=3, `Completed`=4,
`Rejected`=5, `Cancelled`=6.

Rencana pemetaan ada di `02-backend-architecture.md` bagian 6.1. Yang **belum** dijawab
rencana itu: gerbang penutupan `EmergencyDispositionService` memakai `Completed` dan `Rejected`
sebagai penanda "sudah tuntas", sedangkan model baru menaruh ketuntasan fisik pada
`EmergencyPhysicalStatus` dan ketuntasan dokumen pada `EmergencyHandoverStatus`. **Arti
"tuntas" karena itu berubah**, dan validation bagian 6 aturan 3 perlu dibaca ulang terhadap
kenyataan ini. Dicatat sebagai `IGD-OQ-082`.

## S3.1.6 Empat kolom yang dihapus

`FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId` — seluruh urusan tempat tidur pindah ke Rawat
Inap lewat `IGD-DEC-069`.

**Jumlah baris yang nilainya tidak kosong belum diketahui.** Menghapus kolom yang masih berisi
data berarti kehilangan riwayat penempatan. Hanya dapat dijawab kueri ke basis data bersama,
dan otorisasinya belum ada. Dicatat sebagai `IGD-UNK-08`.

## S3.1.7 Yang audit ini **tidak** cakup

Audit ini **terarah** pada `EmergencyTransfer` saja. Bagian lain capability map revision `3`
tetap dihitung pada `f69e9e48` dan **tetap stale** terhadap `300922c` — termasuk
`ClinicalManagement`, `PharmacyManagement`, dan seluruh area yang tersentuh merge
"Hamzah, Ikbal, Yasmina". `/qv-trace` penuh tetap dibutuhkan sebelum gelombang yang menyentuh
area-area itu.

---

# Suplemen revision 3.2 — impact scan encounter-first pada `0d13f3a8` / `c941012ac`

| Field | Nilai |
| --- | --- |
| Tanggal | 22 September 2026 |
| Jenis | **Impact scan terbatas**, read-only. Nol source diubah, nol migration, nol kueri basis data |
| Backend | `NewQuilvianSystemBackend` branch `rizkiG` `0d13f3a8` — **671 commit** sesudah `f69e9e48` |
| Frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `c941012ac` — **383 commit** sesudah `96a91201` |
| Keputusan yang menjadi batas | `IGD-DEC-139`…`150` (encounter-first dan kasus tepinya) |
| Status revisi 3 | **Stale seluruhnya** untuk area di luar suplemen ini. Keempat pemicu bagian 10 menyala: berkas `.cs` berubah di `EmergencyInstallationManagement` (69), `RegistrationManagement` (12), `ClinicalManagement` (68), `BillingManagement` (124), `Repositories/Configurations` (193), `Migrations/` (232); frontend `src/lib/state` (71), `src/lib/services` (67), `src/app/health-services` (245); puluhan migration diterapkan ke basis data bersama |
| Yang **tidak** diaudit ulang | Pengkajian klinis, penunjang, obat, kepergian, serah terima, kewenangan unit (`IGD-CAP-17`…`42` selain yang disebut di bawah). Nama `Trx*` pada revisi 3 sudah menjadi `Emg*`/`Reg*` — baca dengan catatan itu |

Suplemen ini **bukan** arsitektur target dan **bukan** rencana kerja. Ia hanya menjawab: kemampuan
apa yang sudah ada, dalam bentuk apa, untuk setiap keputusan encounter-first.

## S3.2.1 Kemampuan lama yang berubah status

| ID | Capability | Status revisi 3 | Status sekarang | Bukti (`@0d13f3a8` / `@c941012ac`) |
| --- | --- | --- | --- | --- |
| `IGD-CAP-01` | Kunjungan pasien sebagai jangkar episode | `Ready to reuse` | `Ready to reuse` — nama kini `RegPatientEncounter` | `RegistrationManagement/Models/RegPatientEncounter.cs:15-26` |
| `IGD-CAP-02` | Kunjungan IGD sebagai perluasan kunjungan | `Ready to reuse` | `Ready to reuse` — `EmgVisit`; unique index `EncounterId` **tanpa filter** (termasuk baris hapus lunak) | `EmergencyInstallationManagement/Models/EmgVisit.cs:21-23`; `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs:31` |
| `IGD-CAP-03` | Jenis kunjungan `Emergency` | `Conflict` | `Ready to reuse` — layar menulis `Emergency`; backend menerima `Emergency` dan `Outpatient` masa transisi (`IGD-DEC-109`) | `EmergencyVisitService.cs:261`; `tests/unit/emergency-registration-payload.test.mjs:35` (`FE-IGD-014 K1`) |
| `IGD-CAP-07` | Pasien tanpa identitas | `Ready to reuse` | **`Conflict`** — backend siap, layar tidak memakainya. Lihat `IGD-CONF-06` | Backend `EmergencyVisitService.cs:145-157`; frontend `use-emergency-registration.js:937-942` |
| `IGD-CAP-08` | Pencegahan episode ganda | `Missing` | `Reuse with adapter` — aturan ada tetapi hanya membaca kunjungan (klausa B `IGD-DEC-139`); klausa A (encounter) belum ada | `EmergencyVisitService.cs:343-364` (`CariEpisodeAktifAsync`); `EmergencyVisitController.cs:196` (`GET active-episode`) |
| `IGD-CAP-09` | Antrean untuk pasien IGD | `Missing` (disengaja) | **`Unknown`** — kode IGD tetap nol rujukan `TrxQueue`, tetapi pintu encounter membuat `TrxQueue` bila unit/klinik `IsQueueRequired`; nilainya untuk unit IGD belum diketahui (`IGD-UNK-06`) | `PatientEncounterController.cs:544, 634-670` |
| `IGD-CAP-14` | Status kunjungan mengikuti transisi | `Repair` | `Ready to reuse` — satu penjaga `TryApplyVisitStatus`, enam pemanggil | `EmergencyVisitService.cs:459-484`; pemanggil di `EmergencyDispositionController.cs:337`, `EmergencyObservationController.cs:316`, `EmergencyResuscitationController.cs:300`, `EmergencyTriageController.cs:349, 547`, `EmergencyVisitController.cs:546` |
| `IGD-CAP-15` | Penetapan dokter sesudah triage | `Extend` | `Ready to reuse` untuk riwayat; **`Missing`** untuk kelayakan (`IGD-DEC-141`) | `EmergencyDoctorAssignmentController.cs:58-160`; `EmergencyDoctorAssignmentService.cs:173, 261` |
| `IGD-CAP-16` | Riwayat penugasan dokter | `Missing` | `Ready to reuse` — `EmgDoctorAssignment`, `EmergencyVisitId` wajib | `Models/EmgDoctorAssignment.cs:44-91` |
| `IGD-CAP-43` | Prasarana uji backend | `Extend` | **`Missing`** — seluruh proyek test dihapus 11 September 2026 | commit `cefd927d` "Remove backend test projects and simplify Integration gate" |
| `IGD-CAP-45` | Realtime untuk IGD | `Missing` | `Missing` — tetap nol rujukan hub di IGD; daftar *Menunggu Triage* terpadu tidak realtime | `grep Hub Areas/HealthServices/EmergencyInstallationManagement` → kosong |
| `IGD-CAP-46` | Layar pendaftaran IGD | `Extend` | `Extend` — dua panggilan berurutan + pra-cek `active-episode` (`FE-IGD-034`) | `use-emergency-registration.js:80, 1023, 1050` |
| `IGD-CAP-47` | Layar triase | `Ready to reuse` | `Extend` — daftar hanya membaca kunjungan (`GET /emergency-visits`) | `emergency-management-triage-slice.jsx:125, 137-160` |

## S3.2.2 Kemampuan yang dibutuhkan keputusan encounter-first

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-CAP-51` | Satu pintu pembuatan encounter `Emergency` | Registration Management | `PatientEncounterController.cs:427` (`CreateEncounterCoreAsync`), `:559` transaksi, `:591` `EncounterType = request.EncounterType`; `EncounterIntakeService.cs:317-326` (`Outpatient` tetap); `InpEpisodeService.cs:1116-1125` (`Inpatient` tetap); DI `Program.cs:348`; konsumen `use-emergency-registration.js:1023` | `Reuse with adapter` | Belum ada penjaga episode, tanpa-antrean, maupun catatan override. Semua masuk pintu yang sama | Menengah — berkas milik Registrasi; pemiliknya belum dipetakan |
| `IGD-CAP-52` | Serentak pada pintu encounter | Registration Management | `PatientEncounterNumberService.cs:28-35` mengambil `pg_advisory_xact_lock(hashtext('REG_PATIENT_ENCOUNTER_NUMBER'))`; dipanggil **di dalam** transaksi `CreateEncounterCoreAsync` (`:567`), dipegang sampai commit (`:693`) | `Reuse with adapter` | **Fakta baru:** seluruh pembuatan encounter lewat pintu ini **sudah antre satu per satu** sejak alokasi nomor. Penjaga episode yang dijalankan **sesudah** kunci itu sudah bebas balapan pada pintu ini. Tetapi `POST /emergency-visits` tidak mengambil kunci apa pun, dan bersandar pada kunci penomoran untuk invariant bisnis itu rapuh. Lihat catatan desain di S3.2.6 | Rendah untuk pintu encounter; tinggi bila hanya bersandar kunci penomoran |
| `IGD-CAP-53` | Penutupan encounter oleh IGD | Registration (tabel) + IGD (pemicu) | IGD menyebut `EncounterStatus` **nol** kali; `PATCH …/status` (`PatientEncounterController.cs:906-962`) dan `PATCH …/cancel` (`:1046-1085`) menulis tanda berbeda; `ClinicalDocumentIntegrityService.LockOpenDocumentsForEncounterAsync` tidak menyimpan sendiri, DI `Program.cs:392` | `Missing` (pemicu) / `Ready to reuse` (penguncian catatan) | Titik panggil di `EmergencyVisitController.cs:496, 562` | Menengah-tinggi |
| `IGD-CAP-54` | Tanda "encounter berakhir" yang dibaca modul lain | Blood Bank, Medical Record | `BbkEncounterStatusReader.cs:58-60`, `MedicalRecordAccessAuditService.cs:75-92`, `MedicalRecordBackfillService.cs:49-51, 98-101` — semuanya menganggap `Completed`/`Cancelled`/`NoShow` tertutup; sebagian juga membaca `IsCancel`/`CompletedAt` | `Ready to reuse` | Status `NoShow` dari `IGD-DEC-142` otomatis dibaca tertutup oleh modul-modul ini | Rendah |
| `IGD-CAP-55` | Ruas NoShow pada encounter | Registration Management | `RegPatientEncounter.cs:171-176` (`NoShowAt`, `NoShowByUserId`, `NoShowReason`); penulis tunggal `DoctorQueueController.cs:663-676` (antrean dokter) | `Reuse with adapter` | Aksi IGD untuk baris tanpa kunjungan **belum ada**; `PATCH …/status` menerima `NoShow` **tanpa** mengisi ketiga ruas (`IGD-CONF-07`) | Rendah |
| `IGD-CAP-56` | Billing untuk NoShow | Billing Management | `BillingInvoiceService.cs:1205-1227`; `BillingDepositService.cs:264`; pembuatan encounter tidak membuat tagihan | `Ready to reuse` | Nol perubahan (`IGD-DEC-142`) | Rendah |
| `IGD-CAP-57` | Daftar *Menunggu Triage* terpadu | Emergency Installation Management | Hari ini layar membaca `GET /emergency-visits` saja (`emergency-management-triage-slice.jsx:125, 137-160`); proyeksi daftar kunjungan `EmergencyVisitController.cs:760-790` | `Missing` | Endpoint gabungan encounter + kunjungan belum ada | Menengah |
| `IGD-CAP-58` | Kelahiran kunjungan dari encounter (Mulai Triage / Tangani Segera) | Emergency Installation Management | `POST /emergency-visits` (`EmergencyVisitController.cs:240-332`), `GenerateVisitNumberAsync` (`EmergencyVisitService.cs:487`), `TryApplyVisitStatus`, unique index `EncounterId` | `Extend` | Belum ada titik lahir idempoten dengan dua status awal; Tangani Segera hari ini (`PATCH …/visit-status`, `emergency-management-triage-slice.jsx:176-192`) hanya untuk kunjungan yang sudah ada | Menengah |
| `IGD-CAP-59` | Pencegahan antrean untuk encounter `Emergency` | Registration Management | `PatientEncounterController.cs:544` (`isQueueRequired` dari klinik/unit), `:634-670` (pembuatan `TrxQueue`) | `Missing` | Penjaga kode `IGD-DEC-144` belum ada | Rendah |
| `IGD-CAP-60` | Penyimpanan override pendaftaran ganda | Emergency Installation Management | Encounter tanpa ruas override; `EmgVisit.DuplicateEpisodeOverride*` (`EmgVisit.cs:84-96`) hanya untuk jalur kunjungan | `Missing` | Entity baru `IGD-DEC-145` — butuh registrasi prefix QBE dan migration | Rendah |
| `IGD-CAP-61` | Waktu tiba beserta penanda konfirmasi | Emergency Installation Management | `EmgVisit.ArrivalDateTime` (`EmgVisit.cs:32`, bawaan `UtcNow`); `PUT /emergency-visits/{id}` menimpanya (`EmergencyVisitController.cs:378`); layar: validasi dikomentari, jatuh ke jam browser (`IGD-EV-141`) | `Repair` (nilai) / `Missing` (penanda) | Penanda `IGD-DEC-147` butuh migration pada tabel IGD | Menengah — lihat koreksi fakta S3.2.5 |
| `IGD-CAP-62` | Pola preview lalu eksekusi untuk operasi admin | Accounting, HR | `YearEndClosingController.cs:73` (`GET preview`); `LeaveAccrualController.cs:103` (`POST preview`) | `Reuse with adapter` | Jejak audit per run **tidak** ada sebagai tabel: `LoggerService.AuditAsync` menulis ke Serilog (`IGD-CAP-24`) — `IGD-DEC-148` butuh penyimpanan run tersendiri | Menengah |
| `IGD-CAP-63` | Penautan pasien tanpa identitas ke encounter | Emergency Installation Management | `PUT /emergency-visits/{id}` sudah dapat menimpa `EncounterId`, `PatientId`, `IsUnknownPatient` (`EmergencyVisitController.cs:349-394`), lewat `ValidateRequestAsync` | `Reuse with adapter` | Jalur itu penimpaan umum tanpa jejak khusus penautan identitas; `IGD-DEC-149` butuh aksi eksplisit | Menengah — penimpaan umum dapat mengubah identitas tanpa alasan tercatat |
| `IGD-CAP-64` | Kelayakan dokter jaga IGD | — | Pilihan dokter = seluruh master aktif (`emergency-management-triage-slice.jsx:553-576`); `MstDoctorSchedule` ber-DNA poliklinik (`IGD-EV-140`) | `Unknown` | Menunggu E1–E3. `MstDoctorSchedule` **bukan** sumber final | Tinggi (keselamatan) |
| `IGD-CAP-65` | Hak akses aksi baru IGD | Administrator | Aksi terdaftar lewat pasangan `[AccessAction]` + `[AccessPermission]` pada method; `EmergencyVisitController.cs:26-29` (`AccessController` resource `Emergency Visit`) | `Ready to reuse` | Aksi NoShow, Mulai Triage, tautkan identitas, dan rekonsiliasi admin memilih resource/aksi saat desain; pemberian hak ke peran ada di basis data (`IGD-UNK-11`) | Rendah |

## S3.2.3 Kontrak as-is yang disentuh perjalanan encounter-first

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Perilaku hari ini | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters` (juga `/admin`, `/kiosk`) | Membuat encounter dalam transaksi; mengambil kunci penomoran global; membuat `TrxQueue` bila `IsQueueRequired`; **nol** penjaga episode | `PatientEncounter : Create` |
| `PATCH` | `/api/v1/health-services/registration-management/patient-encounters/{id}/status` | Status apa pun tanpa validasi transisi; `NoShow` tanpa ruas NoShow; `Completed` tanpa `CompletedAt` tetapi mengunci catatan klinis | `PatientEncounter : Update` |
| `PATCH` | `/api/v1/health-services/registration-management/patient-encounters/{id}/cancel` | `IsCancel`, `CancelledAt`/`By`, `CancelReason`, `IsActive = false`, antrean dibatalkan; `EncounterStatus` **tidak** berubah | `PatientEncounter : Update` |

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Perilaku hari ini | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-visits` | Daftar kunjungan; filter/urut `ArrivalDateTime` | `EmergencyVisit : Read` |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-visits/active-episode?patientId=` | Pra-cek; hanya klausa B | `EmergencyVisit : Create` |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-visits` | Melahirkan kunjungan; menolak `409` episode ganda (klausa B); **tanpa** kunci serentak; mendukung pasien tanpa identitas | `EmergencyVisit : Create` |
| `PUT` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Menimpa seluruh ruas termasuk `EncounterId`, `PatientId`, `ArrivalDateTime` | `EmergencyVisit : Update` |
| `PATCH` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}/visit-status` | Transisi lewat penjaga; menolak `Completed`; dipakai Tangani Segera | `EmergencyVisit : Update` |
| `PATCH` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}/complete` | Gerbang penutupan lalu `Completed`; encounter **tidak** ikut ditutup | `EmergencyVisit : Update` |

## S3.2.4 Conflict baru

| ID | Conflict | Tingkat | Bukti | Terkait |
| --- | --- | --- | --- | --- |
| `IGD-CONF-06` | **Jalur U1 tidak punya penghasil di layar.** `IGD-DEC-140` menyatakan pasien tanpa identitas "tetap memakai jalur kunjungan-lebih-dulu seperti hari ini" (kunjungan tanpa `PatientId`/`EncounterId`). Kenyataannya layar pendaftaran selalu mulai dari langkah Pasien, menolak lanjut tanpa `patientId`, dan selalu membuat encounter; kotak "pasien tanpa identitas" hanya menandai kunjungan. Artinya di lapangan pasien tanpa identitas **harus** dibuatkan rekam `MstPatient` dulu — praktik yang menyerupai U2 tanpa tata kelola Master Patient, atau pasien itu tidak dapat didaftarkan lewat layar | `HIGH` | `use-emergency-registration.js:937-942` (`"ID pasien belum tersedia."`), `:1023`; `emergency-registration.constants.js` langkah `PATIENT` → `EMERGENCY_VISIT`; `emergency-visit-step.jsx:78-80, 412-437` | `IGD-DEC-140`, `IGD-DEC-149`, `IGD-OQ-098` |
| `IGD-CONF-07` | **Dua jalur menulis NoShow dengan hasil berbeda.** Antrean dokter mengisi status dan ketiga ruas; `PATCH …/status` hanya status. Encounter IGD yang ditutup lewat jalur Registrasi umum tidak memenuhi `IGD-DEC-142` (pelaku dan alasan kosong) | `MEDIUM` | `DoctorQueueController.cs:663-676` lawan `PatientEncounterController.cs:906-962` | `IGD-DEC-142` |
| `IGD-CONF-08` | **Penimpaan identitas lewat `PUT` umum.** `PUT /emergency-visits/{id}` dapat mengganti `PatientId`/`EncounterId` tanpa alasan tercatat, padahal `IGD-DEC-149` menuntut penautan yang dapat diaudit | `MEDIUM` | `EmergencyVisitController.cs:349-394` | `IGD-DEC-149` |

Revisi 3 memakai `IGD-CONF-01`…`05`; penomoran dilanjutkan dari `06`.

## S3.2.5 Koreksi fakta — waktu tiba bukan dasar SLA triage

Pada wawancara 22 September 2026 (pertanyaan F dan F2) dan pada evidence
`2026-09-22-desain-encounter-first.md` bagian 2, agent menulis bahwa waktu tiba adalah titik nol
SLA triage (`EmgTriage.ResponseDueAt`), sehingga nilai fallback membuat "angka SLA triage tampak
lebih baik". **Itu keliru.**

| Yang ditulis | Yang benar menurut source |
| --- | --- |
| `ResponseDueAt` dihitung dari waktu tiba | `ResponseDueAt = StartedAt + MaxWaitingMinutes` level triage — dari **mulai triage**, bukan waktu tiba (`EmergencyTriageController.cs:311-313`; `EmergencyTriageService.cs:206-208`) |
| Waktu tiba memengaruhi daftar pantau pelanggaran SLA | Tidak. Pemantau SLA (`MarkSlaBreachesAsync`) membaca `ResponseDueAt` (`EmergencyTriageService.cs:273-299`) |
| Ada pengukuran *door-to-triage* | Tidak ada di kode. `IGD-DEC-127` hanya menyebutnya sebagai pengukuran yang mungkin dilakukan dari dua kolom terpisah |

**Pemakai `ArrivalDateTime` yang sebenarnya:** filter dan urutan tanggal daftar kunjungan
(`EmergencyVisitController.cs:138-148`), penjaga "penugasan dokter tidak boleh sebelum waktu tiba"
(`EmergencyDoctorAssignmentService.cs:173, 261`), tampilan layar pengkajian, dan fallback
`EffectiveFrom` pada `BE-IGD-048`.

**Akibat pada keputusan.** `IGD-DEC-147` dan `IGD-DEC-142`…`150` **tidak** berubah isinya. Yang
berubah adalah **alasan** `IGD-DEC-147` butir penanda (F2): penanda konfirmasi tetap berguna untuk
kejujuran catatan waktu tiba dan untuk laporan *door-to-triage* **bila kelak dibuat**, tetapi **tidak**
memengaruhi SLA triage yang berjalan hari ini. Satu akibat baru yang harus dirancang: **koreksi waktu
tiba ke arah lebih lambat** dapat membuat penugasan dokter yang sudah ada tampak "sebelum pasien tiba"
— penjaga di `EmergencyDoctorAssignmentService` hanya memeriksa saat penugasan dibuat. Dicatat sebagai
pertanyaan penutup `IGD-TRQ-09`.

## S3.2.6 Catatan desain dari fakta — bukan keputusan

| Fakta | Yang perlu diperhitungkan `design-business-module` |
| --- | --- |
| Kunci penomoran global sudah membuat pintu encounter antre (`IGD-CAP-52`) | `IGD-DEC-146` tetap dipakai sebagai kunci **eksplisit** per pasien, supaya invariant bisnis tidak bersandar pada efek samping penomoran dan supaya `POST /emergency-visits` (yang tidak mengambil kunci penomoran) ikut terjaga. Urutan pengambilan kunci di pintu encounter harus konsisten (per pasien lalu penomoran, atau sebaliknya — sekali ditetapkan) supaya tidak terjadi *deadlock* |
| Seluruh pendaftaran rumah sakit sudah antre pada satu kunci penomoran | Penjaga episode yang ditambahkan di dalam jendela kunci itu memperpanjang antrean semua pendaftaran (rawat jalan juga). Kueri episode harus satu kueri berindeks (`PatientId` sudah berindeks: `RegPatientEncounterConfiguration.cs:368`) |
| Unique index `EmgVisit.EncounterId` tanpa filter | Idempotensi Mulai Triage/Tangani Segera dapat memakai tangkapan `UniqueViolation` (pola `BbkProviderRequestService.cs:404`); K4 tidak dapat diberi kunjungan baru |
| Tidak ada proyek test backend | Acceptance uji paralel (`IGD-DEC-146`) harus dijalankan sebagai uji API/manual oleh pemilik, bukan test otomatis |

## S3.2.7 Unknown

| ID | Yang belum diketahui | Kenapa audit ini tidak dapat menjawab |
| --- | --- | --- |
| `IGD-UNK-06` | Nilai `IsQueueRequired` unit/klinik IGD, dan jumlah `TrxQueue` yang tertaut encounter `Emergency` | Butuh kueri basis data |
| `IGD-UNK-07` | Jumlah encounter `Emergency` belum berakhir per kelas K1–K4 (kueri D) | Butuh kueri basis data — milik pemilik |
| `IGD-UNK-08` | Apakah jadwal dokter IGD ada di `MstDoctorSchedule` (kueri E1–E3) | Sama |
| `IGD-UNK-09` | Kandidat encounter yatim (kueri A/B `BE-IGD-050`) | Sama |
| `IGD-UNK-10` | Bagaimana pasien tanpa identitas **benar-benar** didaftarkan di lapangan hari ini: berapa `EmgVisit` ber-`IsUnknownPatient = true`, apakah semuanya punya `PatientId`/`EncounterId`, dan rekam `MstPatient` macam apa yang dipakai | Butuh kueri basis data dan keterangan petugas |
| `IGD-UNK-11` | Peran mana yang memegang `EmergencyVisit : Read/Create/Update` dan `PatientEncounter : Update` | Pemberian hak ada di basis data |

## S3.2.8 Pertanyaan penutup untuk `grill-me`

| ID | Pertanyaan | Memblokir | Dasar |
| --- | --- | --- | --- |
| `IGD-TRQ-08` | `IGD-CONF-06`: karena layar tidak pernah membuat kunjungan tanpa pasien, apa arti "U1 seperti hari ini"? (a) Layar pendaftaran diberi jalur U1 sungguhan — kunjungan tanpa pasien dan tanpa encounter; (b) praktik lapangan (rekam `MstPatient` pengganti) diakui sebagai cara transisi, yang berarti menyentuh wilayah pemilik Master Patient; (c) ditahan sampai `IGD-UNK-10` terjawab | `DESIGN` untuk `IGD-DEC-140`/`149` | `IGD-CONF-06`, `IGD-UNK-10` |
| `IGD-TRQ-09` | Bolehkah waktu tiba dikoreksi menjadi **lebih lambat** dari penugasan dokter atau mulai triage yang sudah tercatat? Bila tidak, koreksi itu ditolak; bila boleh, riwayat penugasan menjadi tampak tidak masuk akal | `DESIGN` untuk `IGD-DEC-147` | S3.2.5 |
| `IGD-TRQ-10` | `IGD-CONF-07`: apakah encounter `Emergency` boleh tetap ditutup lewat `PATCH …/status` dan `…/cancel` umum milik Registrasi, atau jalur itu harus menolak tipe `Emergency` supaya penutupan IGD selalu lewat aksi IGD yang mengisi pelaku dan alasan? | `DESIGN` untuk `IGD-DEC-142`; menyentuh berkas Registrasi | `IGD-CONF-07` |
| `IGD-TRQ-11` | `IGD-CONF-08`: haruskah `PUT /emergency-visits/{id}` berhenti menerima perubahan `PatientId`/`EncounterId` begitu aksi penautan `IGD-DEC-149` ada? | `DESIGN` | `IGD-CONF-08` |

> **Dijawab 22 September 2026 (malam):** `IGD-TRQ-08` → `IGD-DEC-151`, `IGD-TRQ-09` → `IGD-DEC-152`, `IGD-TRQ-10` →
> `IGD-DEC-153`, `IGD-TRQ-11` → `IGD-DEC-154`. `IGD-CONF-06`…`08` terselesaikan. Fakta tambahan `IGD-FACT-021`…`023`
> dicatat pada decision log.

## S3.2.9 Pemicu audit ulang suplemen ini

Suplemen menjadi tidak sahih bila `PatientEncounterController.cs`, `PatientEncounterNumberService.cs`,
`EmergencyVisitController.cs`, `EmergencyVisitService.cs`, `EmergencyDoctorAssignmentService.cs`,
`use-emergency-registration.js`, atau `emergency-management-triage-slice.jsx` berubah sesudah
`0d13f3a8`/`c941012ac`. Pemeriksaan cukup dengan `git diff --stat` atas berkas-berkas itu.

---

# Suplemen revision 3.3 — impact scan untuk `IGD-DEC-163`…`168` pada `dce1f138`

Scan terbatas, bukan audit ulang seluruh modul. Tujuannya satu: memastikan keputusan baru — **disposisi yang
dilaksanakan menutup kunjungan IGD**, dan penutupan menyusul otomatis saat penahan terakhir dibereskan — berdiri di
atas kemampuan yang benar-benar ada hari ini, bukan di atas peta yang dibuat 78 commit yang lalu.

| Butir | Isi |
| --- | --- |
| Baseline lama | Backend `0d13f3a8` / frontend `c941012ac` (revisi 3 + suplemen 3.2) |
| Baseline baru | Backend `rizkiG` **`dce1f138`** — 78 commit sesudahnya, termasuk merge `62c8360a` dari `QuilvianIntegrationBackend` |
| Frontend | `RizkiV2` `c941012ac` — **nol commit** sejak suplemen 3.2, jadi tidak di-scan ulang |
| Keputusan yang dilayani | `IGD-DEC-163`…`168` (amendment pass 23 September 2026) |
| Batas | Read-only terhadap source kedua repository; nol kueri basis data; nol perubahan source |

## 0. Di mana source benar-benar bergeser

| Area | Berkas berubah | Relevansi terhadap keputusan baru |
| --- | ---: | --- |
| `LaboratoryManagement` | 91 | **Ya** — pembaca status encounter baru (lihat `IGD-CAP-70`) |
| `Repositories` | 71 | Tidak langsung — `DbSet` dan configuration modul lain |
| `ClinicalManagement` | 69 | Tidak — nol sentuhan ke kunjungan IGD |
| `PharmacyManagement` | 54 | Tidak — nol pembaca status encounter yang menahan |
| `InPatientManagement` | 43 | **Ya** — jalur disposisi rawat inap (lihat `IGD-CAP-68`) |
| `BillingManagement` | 31 | Tidak berubah perilakunya terhadap IGD |
| `BloodBankManagement` | 17 | **Ya** — pembaca status encounter baru (lihat `IGD-CAP-69`) |
| `EmergencyInstallationManagement` | 14 | Seluruhnya pekerjaan `BE-IGD-051` dan `BE-IGD-055` |
| `RegistrationManagement` | 8 | Tidak mengubah jalur penutupan encounter |

**Temuan paling melegakan:** `EmergencyObservationService.cs`, `EmergencyDepartureService.cs`, dan
`EmergencyDispositionService.cs` — ketiga service yang akan menjadi pemicu penutupan susulan — **tidak berubah
satu baris pun** sejak `0d13f3a8`. Peta lama masih berlaku penuh untuk ketiganya. Yang berubah di modul IGD hanya
`EmergencyVisitService.cs` (+408/-2), dan itu pekerjaan kita sendiri.

## 1. Kemampuan yang dipakai ulang keputusan baru

| ID | Capability | Status | Bukti | Catatan untuk desain |
| --- | --- | --- | --- | --- |
| `IGD-CAP-66` | Penjaga "pesanan penahan" pada penutupan kunjungan | `READY TO REUSE` | `EmergencyDepartureService.AmbilPesananPenahanPenutupanAsync:574-585`; dipanggil `ValidateVisitClosureAsync:144` | Kueri hanya menyentuh `EmgHandoverOrderItem` milik IGD sendiri — `IsEffective`, `AcceptanceStatus = Rejected`, tertaut kepergian kunjungan itu. **Nol ketergantungan pada Lab atau Farmasi**, sehingga perubahan 91 + 54 berkas di kedua modul itu tidak memindahkan pemicunya |
| `IGD-CAP-67` | Penetapan sikap pesanan dan status serah terima | `READY TO REUSE` | Seluruh penulis `AcceptanceStatus =` dan `HandoverStatus =` berada di `EmergencyDepartureService` (`:204`, `:212`, `:300`, `:582`, `:682`, `:703`, `:807`, `:822`) dan `EmergencyDepartureController` (`:57`, `:167`, `:177`) | Semua pemicu "penahan terakhir dibereskan" tinggal di **satu service milik IGD**. `PendingHandoverStatus` pada `BillingManagement/Cashier/Services/CashierShiftService.cs:1040` adalah kemiripan nama untuk serah terima kasir, **bukan** serah terima pasien |
| `IGD-CAP-68` | Admisi rawat inap sebagai tujuan disposisi | `READY TO REUSE` | `InPatientManagement` memuat **nol** rujukan ke `EmgVisit`, `EmergencyVisit`, `EncounterType.Emergency`, maupun `EmgDeparture` | `IGD-FACT-025` tetap benar sesudah 43 berkas berubah: admisi membuat encounter `Inpatient` baru dan tidak pernah menyentuh encounter IGD. Serah terima ke ranap tetap dicatat dan diterima di sisi IGD, sehingga pemicu penutupan tidak berpindah modul |

## 2. Kemampuan baru yang muncul sejak baseline — dan konsekuensinya

| ID | Capability | Status | Bukti | Akibat bagi keputusan baru |
| --- | --- | --- | --- | --- |
| `IGD-CAP-69` | Bank Darah membaca "kunjungan sudah berakhir" | `CONFLICT` (konsekuensi, bukan cacat) | `BbkEncounterStatusReader.cs:50-147` — `Completed`, `Cancelled`, `NoShow` dibaca sebagai tertutup; dipakai `BbkBloodOrderService:502`, penolakan pada `:629-632`, dan `BbkBloodUnitService:2768-2776` | Begitu encounter tertutup, Bank Darah **menolak order darah baru** dan **menolak alokasi kantong** ke baris kebutuhan order itu |
| `IGD-CAP-70` | Laboratorium menolak pemesanan pada kunjungan yang sudah selesai | `CONFLICT` (konsekuensi, bukan cacat) | `LabOrderService.CreateByExaminationsAsync:741`, penjaga `VAL-67` pada `:773-780` | Begitu encounter tertutup, **pemeriksaan laboratorium baru tidak dapat dipesankan**. Hanya satu titik penjagaan di service itu, dan letaknya pada pembuatan order — **pencatatan hasil order yang sudah ada tidak ikut tertahan** |

### `IGD-CONF-09` — penutupan yang lebih rajin mematikan pemesanan susulan

Sampai `BE-IGD-051`, encounter IGD **tidak pernah** tertutup. Akibatnya kedua penjaga di atas praktis tidak pernah
menyala untuk pasien IGD: order darah dan pemeriksaan laboratorium selalu diterima, berapa lama pun sesudah pasien
pulang.

`IGD-DEC-163` dan `IGD-DEC-165` mengubah itu secara mendasar — bukan karena artinya berubah, melainkan karena
**waktunya** berubah. Encounter kini tertutup pada saat disposisi dilaksanakan, atau menyusul otomatis begitu
penahan terakhir dibereskan. Sejak detik itu, untuk encounter tersebut:

- order darah baru ditolak, dan kantong tidak dapat dialokasikan ke order lamanya;
- pemeriksaan laboratorium baru ditolak.

*Contoh.* Pasien pulang pukul 14.00 dan disposisinya dilaksanakan saat itu juga. Pukul 16.10 perawat menutup
observasi terakhir, dan kunjungan ikut tertutup otomatis (`IGD-DEC-165`). Pukul 16.30 dokter ingin menambahkan satu
pemeriksaan laboratorium susulan atas spesimen yang sudah diambil tadi siang — permintaan itu **ditolak** dengan
pesan *"Kunjungan ini sudah selesai, pemeriksaan baru tidak dapat dipesankan."*

Ini bukan cacat: justru itulah arti episode yang berakhir, dan pasien yang benar-benar butuh layanan baru
seharusnya didaftarkan ulang. Tetapi konsekuensinya **harus disadari dan diterima pemilik**, bukan ditemukan
petugas di lapangan. Pencatatan hasil untuk order yang sudah terlanjur dibuat tidak terdampak.

**Status:** `open` — bukan blocker teknis, tetapi wajib dijawab sebelum kontrak susulan dikunci.

## 3. Jawaban sisi source untuk `IGD-OQ-110`

Pertanyaannya: adakah jalur yang menulis `RegPatientEncounter.IsActive = false` **tanpa** mengisi satu pun dari lima
tanda berakhir? Seluruh penulisnya ditelusuri:

| Jalur | Bukti | Mengisi tanda berakhir? |
| --- | --- | --- |
| Penutupan encounter oleh IGD | `EmergencyVisitService.cs:627` | **Ya** — `EncounterStatus = Cancelled`, `IsCancel`, `CancelledAt` |
| Rollback encounter jangkar admisi | `InpEpisodeService.cs:956-965` | **Ya** — `Cancelled`, `IsCancel`, `CancelledAt` |
| Pembatalan encounter oleh Registrasi | `PatientEncounterController.cs:1070-1075` | **Ya** — `IsCancel = true` |
| Penutup otomatis pendaftaran kiosk | `KioskEncounterClosureService.cs:246-250` | **Ya** — `NoShow`, `NoShowAt` |
| **Hapus lunak encounter** | `PatientEncounterController.DeleteEncounter:1096-1117` | **Tidak** — tetapi baris itu juga diberi `IsDelete = true`, dan **seluruh** kueri IGD menyaring `!IsDelete`, sehingga baris itu tidak pernah terbaca sebagai "masih terbuka" |

**Kesimpulan sisi source:** tidak ada jalur yang meninggalkan encounter dalam keadaan "nonaktif, belum berakhir, dan
belum terhapus". `IGD-OQ-110` karena itu dipersempit: yang tersisa hanya kemungkinan **data lama** yang ditulis jalur
yang sudah tidak ada lagi, atau lewat SQL langsung. Kuerinya tetap milik pemilik; agent tidak menjalankannya.

## 4. Pertanyaan untuk pemilik

| ID | Pertanyaan | Mengapa penting | Pemilik |
| --- | --- | --- | --- |
| ~~`IGD-TRQ-12`~~ **dijawab `IGD-DEC-169`** (23 September 2026: diterima apa adanya, nol perubahan modul lain, nol tenggang waktu) | Apakah konsekuensi `IGD-CONF-09` diterima apa adanya — sesudah kunjungan tertutup, order darah dan pemeriksaan laboratorium baru untuk encounter itu ditolak? | Menentukan apakah desain cukup menutup kunjungan begitu saja, atau perlu tenggang waktu, atau perlu pesan yang mengarahkan petugas mendaftarkan episode baru | Product/Domain Owner IGD, dengan tinjauan pemilik Bank Darah dan Laboratorium |

## 5. Kesimpulan scan

1. **Peta lama masih dapat dipercaya untuk inti keputusan ini.** Ketiga service pemicu tidak berubah, dan seluruh
   pemicu penutupan susulan berada di dalam modul IGD sendiri — satu service, bukan tersebar lintas modul seperti
   yang sempat saya duga sebelum pengukuran.
2. **Yang benar-benar baru ada di hilir**, bukan di hulu: dua pembaca status encounter (`IGD-CAP-69`, `IGD-CAP-70`)
   yang akan mulai menyala untuk pasien IGD justru karena encounter kini benar-benar ditutup.
3. **Satu keputusan pemilik dibutuhkan sebelum desain dikunci** (`IGD-TRQ-12`). Selain itu, desain boleh berjalan.
4. Frontend tidak perlu di-scan ulang: nol commit sejak suplemen 3.2.

---

# Suplemen revision 3.4 — impact scan Ruang Kerja Dokter IGD pada `43dab6da` / `6680278a2`

Scan terbatas, bukan audit ulang seluruh modul. Tujuannya satu: memastikan keputusan `IGD-DEC-220`…`228` — **layar
dokter IGD tersendiri yang mengikuti Ruang Kerja Dokter Rawat Inap, dengan penunjang lima jenis** — berdiri di atas
kemampuan yang benar-benar ada hari ini. Status memakai tujuh status kontrak bukti (`Ready to reuse`, `Reuse with
adapter`, `Extend`, `Repair`, `Missing`, `Conflict`, `Unknown`).

| Butir | Isi |
| --- | --- |
| Baseline lama | Backend `dce1f138` (suplemen 3.3) / frontend `c941012ac` (suplemen 3.2); butir `IGD-CAP-17`…`20`, `24`, `29`…`31` masih dari revisi 3 (`f69e9e48`, Agustus 2026) |
| Baseline baru | Backend `rizkiG` **`43dab6da`**, frontend `RizkiV2` **`6680278a2`** — keduanya sejajar origin, working tree bersih |
| Keputusan yang dilayani | `IGD-DEC-220`…`228` (*amendment pass* 6 Oktober 2026) |
| Batas | Read-only terhadap source kedua repository; nol kueri basis data; nol perubahan source; nol uji runtime |

## 0. Ringkasan untuk pemilik

1. **Backend sebagian besar sudah siap.** Konsultasi dokter, diagnosis, resep, addendum, dan kelima jenis pesanan penunjang
   menerima pasien IGD hari ini. Contoh: dokter IGD dapat membuat catatan dokter tanpa antrean, menempelkan diagnosis
   J18.9 di dalamnya, lalu menulis resep yang menempel ke catatan itu — semuanya lewat endpoint yang sudah ada.
2. **Tiga perluasan backend wajib**: pengkajian medis dokter untuk pasien IGD (`IGD-CAP-75`, kini ditolak *"Pasien ini
   tidak sedang dirawat inap."*), tindakan oleh perawat tanpa catatan dokter (`IGD-CAP-79`), dan daftar kunjungan yang
   membawa DPJP serta saringan *"Pasien saya"* (`IGD-CAP-71`). Ditambah penjaga diagnosis pada tindak lanjut
   (`IGD-CAP-86`).
3. **Frontend**: komponen tab rawat inap dapat dipakai ulang, tetapi semua hook-nya membaca lewat rute
   `…/episodes/{episodeId}` milik episode rawat inap, sehingga butuh varian per kunjungan IGD/encounter (adapter). Daftar
   umum tiap modul sudah dapat disaring per encounter, kecuali gizi.
4. **Satu conflict butuh keputusan pemilik** (`IGD-CONF-10`): menyelesaikan catatan dokter IGD otomatis mengirim fakta tagih
   jasa konsultasi dan resep ke Billing, padahal desain billing IGD belum ada.

## 1. Status ulang kemampuan revisi 3

| ID | Capability | Status revisi 3 | Status sekarang | Bukti | Catatan |
| --- | --- | --- | --- | --- | --- |
| `IGD-CAP-17` | Pengkajian pasien | `Conflict` (`QueueId` wajib) | **`Extend`** | `PatientAssessmentController` — pengkajian keperawatan IGD sudah dipakai layar perawat (`createPatientAssessment`); kajian medis (`MedicalInitial`, `MedicalReassessment`) memanggil `ResolveForDoctorWriteAsync` dan menolak pasien yang tidak dirawat inap | Lihat `IGD-CAP-75` |
| `IGD-CAP-18` | Konsultasi dokter | `Conflict` (`QueueId` wajib) | **`Ready to reuse`** | `DoctorConsultationController.ValidateWithoutQueueGateAsync` — encounter yang punya `EmgVisit` lolos tanpa antrean (`BE-IGD-028`, `FR-IGD-062`); `CreateDoctorConsultationRequest.QueueId` opsional | Dasar catatan dokter, diagnosis, tindakan dokter, dan resep |
| `IGD-CAP-19` | Diagnosis pasien | `Conflict` (`ConsultationId` wajib) | **`Ready to reuse`** — lewat catatan dokter | `PatientDiagnosisController` resolusi konteks: dengan konsultasi → sah untuk encounter apa pun; tanpa konsultasi → hanya `EncounterType.Inpatient` berepisode (*"Diagnosis harus melekat pada catatan dokter atau pada perawatan pasien yang sedang berjalan."*) | Sejalan dengan `IGD-DEC-226`/`227`: diagnosis IGD ditulis di catatan dokter |
| `IGD-CAP-20` | Tindakan pasien | `Conflict` (`ConsultationId` wajib) | **`Extend`** | `PatientProcedureController` `POST /` — validasi `"ConsultationId wajib diisi."` dan konsultasi belum selesai; `POST /inpatient-orders` mewajibkan `InpEpisodeId` | Dokter: siap lewat catatan dokter (`IGD-CAP-78`); perawat: belum ada jalur (`IGD-CAP-79`) |
| `IGD-CAP-24` | Riwayat versi catatan klinis | `Missing` | **`Ready to reuse`** untuk catatan dokter | `ClinicalNoteAddendumController` `by-document/{documentKind}/{documentId}`; `ClinicalNoteAddendumService.ResolveAuthorityAsync` — penulis asli atau pengganti berwenang; nol syarat rawat inap; `ClinicalDocumentKind.Consultation` | Menjawab `IGD-DEC-227`; jenis catatan lain tidak diperluas |
| `IGD-CAP-29` | Pemesanan laboratorium | `Extend` | **`Reuse with adapter`** (dokter) / **`Extend`** (perawat atas instruksi) | `LabOrderPagedQuery.EncounterId`; `LabOrderService.ResolveInstructionAsync` — instruksi hanya bila `InpEpisodeId` terisi; penjaga `VAL-67` menolak encounter selesai | Lihat `IGD-CAP-80`, `85` |
| `IGD-CAP-30` | Pemesanan radiologi | `Missing` | **`Reuse with adapter`** (dokter) / **`Extend`** (perawat) | `RadOrderController` `POST`, `GET ?encounterId=` (`RadOrderListQuery`); `RadiologyExplicitPermissions.cs` — `RadReport : ActAsRadiologist` kini *dapat dicentang* di Akses Role | Penahan `IGD-DEC-111` (d) gugur di source; `IGD-DEC-228` |
| `IGD-CAP-31` | Peresepan obat | `Conflict` (`ConsultationId` wajib) | **`Reuse with adapter`** | `CreatePrescriptionRequest` — `EncounterId` + `ConsultationId` konsultasi yang belum *Completed*/*Cancelled*; `GET ?encounterId=` | `ClinicId` resep IGD kosong — `IGD-UNK-12` |

## 2. Kemampuan yang dibutuhkan `IGD-DEC-220`…`228`

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-CAP-71` | Daftar pasien dokter IGD dengan saringan *"Pasien saya"* dan nama DPJP (`IGD-DEC-222`) | EmergencyInstallationManagement | backend `EmergencyVisitController.GetList` (saringan: search, encounter, pasien, unit, status, `awaitingClosure`, tanggal — **tanpa** dokter)@43dab6da; `Models/EmgDoctorAssignment.cs` (`DoctorId`, `EffectiveFrom`, `EffectiveTo`); `EmergencyDoctorAssignmentController GET /active?emergencyVisitId=`; frontend rawat inap memakai sensus `assignedToMe=true` + `userInfo.doctorId` (`use-inpatient-physician-patients.jsx`)@6680278a2 | `Extend` | Ruas DPJP aktif pada respons daftar kunjungan dan saringan per dokter pada `GET /emergency-visits` | Tanpa perluasan, layar harus memanggil penugasan per baris (N+1) |
| `IGD-CAP-72` | Kerangka layar dokter: daftar, kartu pasien, header konteks | Frontend IGD | `doctor-inpatient-view.jsx`, `physician-workspace-context.jsx`, `use-inpatient-physician-workspace.jsx` (episode, penempatan tempat tidur, alergi, diagnosis, lama rawat)@6680278a2 | `Reuse with adapter` | Konteks IGD = kunjungan IGD (`GET /emergency-visits/{id}` sudah membawa pasien, unit, cara datang, jenis kasus — `BE-IGD-064`), alergi dan diagnosis per encounter; tanpa tempat tidur dan lama rawat | Komponen terikat istilah rawat inap (*Dirawat*, *Discharge Pending*) |
| `IGD-CAP-73` | Catatan dokter SOAP + diagnosis (`IGD-DEC-221`, `227`) | ClinicalManagement | `DoctorConsultationController` `POST`, `PATCH {id}/soap`, `PATCH {id}/complete` (penulis tunggal `EnsureSoleAuthorAsync`, keutuhan draf), `GET ?encounterId=`; `GET episodes/{episodeId}/soap-timeline` hanya rawat inap@43dab6da; `use-inpatient-progress-note.jsx` memakai `getSoapTimelineByEpisode`@6680278a2 | `Reuse with adapter` | Timeline SOAP per kunjungan IGD — `GET ?encounterId=` atau endpoint timeline per encounter | Pengurutan dan bentuk respons daftar umum belum tentu sama dengan timeline |
| `IGD-CAP-74` | Addendum dan halaman *Catatan Saya* (`IGD-DEC-227`) | MedicalRecordManagement | `ClinicalNoteAddendumController`; `ClinicalDocumentIntegrityController GET my-authored`, `my-unsigned` — per penulis, nol syarat rawat inap@43dab6da; `use-my-authored-notes.js`@6680278a2 | `Ready to reuse` | Catatan dokter IGD muncul bila terdaftar di integritas dokumen — dibuktikan saat uji | Teks halaman berbau rawat inap |
| `IGD-CAP-75` | Pengkajian medis dokter untuk pasien IGD | ClinicalManagement | `PatientAssessmentController` — kajian medis → `ResolveForDoctorWriteAsync`; gagal → *"Pasien ini tidak sedang dirawat inap."*; `MedicalInitial` unik per episode@43dab6da | `Extend` | Konteks kunjungan IGD untuk kajian medis, termasuk aturan keunikan `MedicalInitial` per kunjungan | Pemilik ClinicalManagement `OPEN` |
| `IGD-CAP-76` | CPPT di layar dokter | ClinicalManagement | `PatientIntegratedProgressNoteController GET ?encounterId=`, `GET timeline?encounterId=`; verifikasi DPJP hanya per episode (`episodes/{id}/verification-status`, `verification-worklist`)@43dab6da | `Reuse with adapter` | Baca per encounter; verifikasi CPPT oleh DPJP tidak diputuskan untuk IGD — tidak dibawa | — |
| `IGD-CAP-77` | Resep dari layar dokter IGD (`IGD-DEC-220`) | PharmacyManagement | `PrescriptionController POST`, `GET ?encounterId=`; template resep; rawat inap memakai `getPrescriptionsByEpisode` + timeline SOAP@6680278a2 | `Reuse with adapter` | Daftar resep per encounter; pilihan catatan dokter terbuka sebagai induk resep | `IGD-UNK-12` |
| `IGD-CAP-78` | Tindakan oleh dokter (`IGD-DEC-225`) | ClinicalManagement | `PatientProcedureController POST /` (catatan dokter terbuka wajib), `GET ?encounterId=`@43dab6da | `Reuse with adapter` | Tindakan menempel ke catatan dokter yang belum diselesaikan | Dokter yang sudah menyelesaikan catatannya harus membuka catatan baru untuk menambah tindakan |
| `IGD-CAP-79` | Tindakan oleh perawat tanpa catatan dokter (`IGD-DEC-225`) | ClinicalManagement | `POST /` wajib `ConsultationId`; `POST /inpatient-orders` wajib `InpEpisodeId`; layar perawat IGD hanya membaca `emergency-procedure-details`@43dab6da | `Missing` | Jalur tindakan perawat untuk encounter IGD | Pemilik ClinicalManagement `OPEN` |
| `IGD-CAP-80` | Pesanan laboratorium | LaboratoryManagement | `POST /lab-orders`, `POST by-examinations`, `GET ?encounterId=`, `instruction-verification-worklist`@43dab6da | `Reuse with adapter` (dokter) / `Extend` (perawat) | Instruksi dan verifikasi untuk encounter IGD | `IGD-OQ-117` |
| `IGD-CAP-81` | Pesanan radiologi | RadiologyManagement | `POST /rad-orders`, `GET ?encounterId=`, `instruction-verification-worklist`; `AccessExplicitPermission RadReport/ActAsRadiologist`@43dab6da | `Reuse with adapter` (dokter) / `Extend` (perawat) | Sama dengan lab | `IGD-OQ-117`; `IGD-UNK-15` |
| `IGD-CAP-82` | Pesanan darah | BloodBankManagement | `CreateBloodOrderRequest` (`PatientId`, `EncounterId`, `ServiceUnitId`, `RequestingDoctorId` wajib); `BbkBloodOrderService` menolak encounter berakhir dan unit tanpa `MstServiceUnit.IsAvailableForBloodOrder`; `GET ?encounterId=`@43dab6da | `Reuse with adapter` | Unit pemesan = unit IGD | `IGD-UNK-13` |
| `IGD-CAP-83` | Pesanan hemodialisa | HemodialysisManagement | `CreateHmdOrderRequest` (`PatientId` wajib; `EncounterId`, `InpEpisodeId`, `RequestingDoctorId` opsional — dokter aktif bila diisi); `HmdOrderPagedQuery.EncounterId`@43dab6da | `Reuse with adapter` | — | — |
| `IGD-CAP-84` | Pesanan gizi | NutritionManagement | `CreateGzOrderRequest` (`PatientId`, `EncounterId`, `RequesterDoctorId`, alasan, idempotency); satu order terbuka per encounter; `GziOrderPagedQuery` **tanpa** `EncounterId` (hanya `PatientId`)@43dab6da; rawat inap menyaring di layar (`filterOrdersByEpisode`)@6680278a2 | `Reuse with adapter` | Saring per encounter di layar, atau perluasan query | `IGD-OQ-119` |
| `IGD-CAP-85` | Verifikasi pesanan perawat atas instruksi (`IGD-DEC-224`) | Lab, Radiologi, ClinicalManagement; Bank Darah, Hemodialisa, Gizi | Worklist instruksi lab, radiologi, tindakan — semuanya untuk episode rawat inap; bank darah, hemodialisa, gizi tanpa status verifikasi@43dab6da | `Extend` (lab, radiologi, tindakan) / `Missing` (tiga lainnya) | Daftar *perlu verifikasi* untuk dokter IGD | `IGD-OQ-117`, `118` |
| `IGD-CAP-86` | Tindak lanjut di layar dokter + penjaga diagnosis (`IGD-DEC-223`, `226`) | EmergencyInstallationManagement | `EmergencyDispositionController.UpdateDispositionStatus` (Confirmed baris 367); `Create` menyalin `request.DispositionStatus` apa adanya; `EmergencyDispositionService.ValidateRequestAsync` hanya memeriksa `Enum.IsDefined`@43dab6da | `Extend` + `Repair` | Penjaga diagnosis di konfirmasi; status awal create dibatasi atau ikut dijaga | `IGD-CONF-11` |
| `IGD-CAP-87` | Layar perawat: tindak lanjut hanya *Laksanakan* (`IGD-DEC-223`) | Frontend IGD | Tab Tindak Lanjut perawat: *Simpan Draf*, *Konfirmasi*, *Jalankan*, *Batalkan* (`emergency-assessment-constant.jsx` baris 377–406)@6680278a2 | `Extend` | Sembunyikan buat/konfirmasi di layar perawat; label *Jalankan* ≠ *Laksanakan* pada keputusan | — |
| `IGD-CAP-88` | Layar perawat: pesanan dengan dokter pemberi instruksi (`IGD-DEC-224`) | Frontend IGD | Tab Penunjang perawat memanggil `createLabOrder` tanpa dokter instruksi@6680278a2 | `Extend` | Pilihan dokter pemberi instruksi; kelima jenis pesanan | Bergantung `IGD-CAP-85` |
| `IGD-CAP-89` | Layar perawat: input tindakan (`IGD-DEC-225`) | Frontend IGD | Tab Tindakan perawat membaca `emergency-procedure-details` saja (`fetchProcedures`)@6680278a2; `EmgProcedureDetail` hanya ditulis controller-nya sendiri dan tidak dibaca modul lain@43dab6da | `Extend` | Ganti sumber baca ke tindakan klinis umum dan tambah input | Bergantung `IGD-CAP-79` |
| `IGD-CAP-90` | Hak akses layar dokter | Platform authorization | Resource yang ada: `DoctorConsultation` (Create, Complete), `PatientDiagnosis`, `PatientProcedure`, `Prescription`, `PatientAssessment`, `LabOrder`, `RadOrder`, pesanan darah, hemodialisa, gizi, `EmergencyDisposition` — diatur Akses Role@43dab6da | `Ready to reuse` (resource) / `Unknown` (menu) | Menu sidebar tidak didefinisikan di source; `AccessMenuSeeder` hanya menulis `RoutePath` API | `IGD-UNK-14` |
| `IGD-CAP-91` | Identitas dokter pengguna | Platform | `userInfo.doctorId` dari `auth/me` (frontend rawat inap); klaim `doctor_id` (`PatientAssessmentController.ResolveCurrentDoctorIdAsync`); `InpatientClinicalContextService.ResolveActorDoctorIdAsync`@43dab6da | `Ready to reuse` | — | Akun dokter tanpa `doctorId` tidak dapat memakai saringan *"Pasien saya"* |

## 3. Kontrak as-is yang disentuh

| Method | Path (`/api/v1/health-services/…`) | Yang relevan untuk IGD |
| --- | --- | --- |
| `POST` | `clinical-management/doctor-consultations` | `EncounterId` wajib; `QueueId` opsional; encounter IGD lolos tanpa antrean |
| `PATCH` | `clinical-management/doctor-consultations/{id}/complete` | Hanya penulis; memfinalkan resep draf dan mengirim fakta tagih (`IGD-CONF-10`) |
| `GET` | `clinical-management/doctor-consultations?encounterId=` | Pengganti timeline per episode |
| `POST` | `clinical-management/patient-diagnoses` | Untuk IGD wajib menempel ke catatan dokter yang belum *Completed* |
| `POST` | `clinical-management/patient-procedures` | `ConsultationId` wajib (catatan dokter terbuka) |
| `POST` | `clinical-management/patient-procedures/inpatient-orders` | `InpEpisodeId` wajib — tidak untuk IGD |
| `POST` | `clinical-management/patient-assessments` | Kajian medis ditolak bila pasien tidak dirawat inap |
| `POST` | `medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | Per penulis; tanpa syarat rawat inap |
| `POST` | `pharmacy-management/prescriptions` | `EncounterId` + `ConsultationId` terbuka |
| `POST` | `laboratory-management/lab-orders` | Encounter belum selesai; instruksi hanya rawat inap |
| `POST` | `radiology-management/rad-orders` | Sama dengan lab |
| `POST` | `blood-bank-management/blood-orders` | Unit wajib `IsAvailableForBloodOrder` |
| `POST` | `hemodialysis-management/hemodialysis-orders` | `PatientId` wajib |
| `POST` | `nutrition-management/orders` | Satu order terbuka per encounter |
| `POST` | `emergency-installation-management/emergency-dispositions` | Status awal dari permintaan, tanpa pembatasan (`IGD-CONF-11`) |
| `PATCH` | `emergency-installation-management/emergency-dispositions/{id}/disposition-status` | Titik konfirmasi; tempat penjaga `IGD-DEC-226` |

## 4. Conflict

### `IGD-CONF-10` — catatan dokter IGD yang diselesaikan mengirim fakta tagih

`ConsultationFinalizationService.FinalizeAsync` (dipanggil `PATCH …/doctor-consultations/{id}/complete`) sesudah commit
mengirim fakta `ConsultationCompleted` (jasa konsultasi, satu per catatan dokter) dan satu fakta per resep yang difinalkan
lewat `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync`. Produsen itu **tidak** menyaring jenis kunjungan. Desain
billing IGD belum ada; arah kerja `IGD-DEC-097` (`draft`) menyebut *jasa kunjungan IGD*, bukan jasa konsultasi per catatan.
*Contoh:* dr. Ani menyelesaikan dua catatan dokter untuk Tn. Budi dalam satu kunjungan IGD — dua fakta jasa konsultasi
terkirim. Apakah Billing menagih keduanya untuk encounter IGD belum diaudit (`IGD-UNK-16`); billing di luar scope pass ini.
**Butuh keputusan pemilik** (`IGD-TRQ-13`).

### `IGD-CONF-11` — tindak lanjut dapat lahir langsung *Confirmed* atau *Executed* (temuan lama, `Repair`)

`POST /emergency-dispositions` menyimpan `request.DispositionStatus` apa adanya; validasinya hanya `Enum.IsDefined`.
Akibatnya penjaga diagnosis `IGD-DEC-226` yang hanya dipasang di `PATCH …/disposition-status` dapat dilewati, dan tindak
lanjut yang lahir *Executed* tidak memicu penutupan susulan (pemicu `BE-IGD-060` ada di jalur `PATCH`). Layar perawat saat
ini selalu membuat *Draft* (*Simpan Draf*), sehingga belum terjadi dari layar. Rekomendasi desain: status awal create
dibatasi *Draft* (perubahan kontrak — butuh approval pada desain).

## 5. Unknown

| ID | Hal | Pemilik | Cara menjawab |
| --- | --- | --- | --- |
| `IGD-UNK-12` | `ClinicId` kosong pada resep IGD — apakah telaah, penyiapan, penyerahan, dan penagihan Farmasi memerlukannya | Pemilik Farmasi | Uji runtime pada putaran uji |
| `IGD-UNK-13` | Nilai `MstServiceUnit.IsAvailableForBloodOrder` untuk unit IGD di tiap lingkungan | Master Data / Rizki | Kueri `SELECT` oleh pemilik, atau uji runtime |
| `IGD-UNK-14` | Cara menambah menu Ruang Kerja Dokter IGD di sidebar — tidak didefinisikan di source | Rizki (admin) | Konfirmasi pemilik |
| `IGD-UNK-15` | Apakah `RadReport : ActAsRadiologist` sudah dicentang untuk peran radiolog di tiap lingkungan | Pemilik Radiologi | Layar Akses Role |
| `IGD-UNK-16` | Perlakuan Billing atas fakta `ConsultationCompleted` dan resep dari encounter IGD | Pemilik Billing | Audit modul Billing pada slice billing IGD |
| `IGD-UNK-17` | Jumlah baris lama `EmgProcedureDetail` per lingkungan | Rizki | Kueri `SELECT` oleh pemilik |

## 6. Catatan desain dari fakta — bukan keputusan

1. Tab SOAP di layar perawat menulis CPPT (`patient-integrated-progress-notes`) dengan pilihan profesi, bukan catatan
   dokter; sesudah layar dokter ada, pilihan profesi *Dokter* di layar perawat layak ditinjau.
2. Rawat inap memakai satu rute `…/episodes/{episodeId}` per modul; IGD cukup memakai daftar umum `?encounterId=` yang sudah
   ada untuk CPPT, konsultasi, resep, tindakan, diagnosis, lab, radiologi, darah, dan hemodialisa. Hanya gizi yang tidak.
3. Pola instruksi rawat inap (`InstructingDoctorId`, status verifikasi, worklist) adalah titik awal yang wajar untuk
   `IGD-CAP-85`, tetapi pemeriksaan *"dokter sedang bertugas atas pasien"* rawat inap memakai episode, bukan kunjungan IGD.

## 7. Pertanyaan penutup untuk `grill-me`

| ID | Pertanyaan | Mengapa penting | Pemilik |
| --- | --- | --- | --- |
| ~~`IGD-TRQ-13`~~ **dijawab `IGD-DEC-229`** (6 Oktober 2026: resep tetap mengirim fakta tagih, jasa konsultasi per catatan tidak dikirim untuk pasien IGD) | Bolehkah fakta tagih jasa konsultasi dan resep dari catatan dokter IGD mengalir ke Billing sejak Ruang Kerja Dokter IGD dirilis (`IGD-CONF-10`), atau ditahan sampai slice billing IGD? | Menentukan apakah desain perlu menyaring encounter IGD di finalisasi, atau cukup mencatat konsekuensinya | Product/Domain Owner IGD, dengan pemilik Billing dan Finance |

## 8. Pemicu audit ulang suplemen ini

Commit yang menyentuh `DoctorConsultationController`, `PatientAssessmentController`, `PatientProcedureController`,
`PatientDiagnosisController`, `PatientIntegratedProgressNoteController`, `ClinicalNoteAddendum*`,
`ConsultationFinalizationService`, `ClinicalMilestoneFactProducer`, `PrescriptionController`, kelima service pesanan
penunjang, `EmergencyDisposition*`, `EmergencyVisitController`, `EmergencyDoctorAssignment*`, atau hook
`use-inpatient-*` dan komponen `physician-workspace/**` di frontend. Butir `IGD-CAP-17`…`20`, `24`, `29`…`31` revisi 3
digantikan bagian 1 suplemen ini.

## 9. Kesimpulan scan

1. **Keputusan `IGD-DEC-220`…`228` dapat dirancang sekarang.** Mayoritas kebutuhan `Ready to reuse` atau `Reuse with
   adapter`; empat perluasan backend (`IGD-CAP-71`, `75`, `79`, `86`) dan satu kekurangan lintas modul (`IGD-CAP-85`,
   tergantung `IGD-OQ-117`/`118`) menjadi isi desain.
2. **Satu keputusan pemilik dibutuhkan sebelum desain dikunci** (`IGD-TRQ-13`, atas `IGD-CONF-10`).
3. **Satu temuan lama perlu diperbaiki bersama** (`IGD-CONF-11`), karena tanpanya penjaga diagnosis dapat dilewati.

---

# Suplemen revision 3.5 — impact scan dokter jaga IGD (`S7`) pada `c7e523fd` / `3335f4b53`

Scan terbatas untuk `IGD-OQ-102`, `IGD-OQ-103`, dan `IGD-DEC-141`. Tujuannya satu: mencatat apa yang **sudah** ada di
source untuk pemilih DPJP sesudah perubahan pemilik 8 Oktober 2026, supaya desain `EPIC IGD-12` berdiri di atas kode yang
ada. Status memakai tujuh status kontrak bukti yang sama dengan suplemen 3.4.

| Butir | Isi |
| --- | --- |
| Baseline lama | `IGD-EV-140` (22 September 2026, backend `0d13f3a8`, frontend `c941012ac`): pilihan dokter = seluruh master dokter aktif |
| Baseline baru | Backend `rizkiG` **`c7e523fd`**, frontend `RizkiV2` **`3335f4b53`** — working tree bersih |
| Keputusan yang dilayani | `IGD-DEC-141`; dijawab menjadi `IGD-DEC-237`…`239` |
| Batas | Read-only terhadap source; nol kueri basis data; satu perintah dijalankan — unit test frontend `emergency-doctor-shift-assignment` (7/7 lulus) |

## 1. Kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-CAP-92` | Daftar dokter jaga IGD pada waktu T | IGD | backend `EmergencyDoctorAssignmentService.AmbilDokterJagaAktifAsync` `:425` (pencocokan unit `:452`–`465`, shift `:473`); `EmergencyDoctorAssignmentController.GetOnDutyDoctors` `:62`–`74`@c7e523fd | **`Repair`** | Pencocokan nama/kode unit dibuang (`IGD-DEC-237`); kueri jadwal dan shift lintas tengah malam dipertahankan | Unit IGD yang belum bertipe `Emergency` membuat roster kosong (`IGD-UNK-18`) |
| `IGD-CAP-93` | Kelompok dokter di luar jadwal yang selalu ditawarkan | IGD | Cadangan `:578` — hanya bila roster kosong; label *"(Dokter Jaga IGD)"* `:608`–`609`; `SessionName = "Luar Roster Shift"`@c7e523fd | **`Repair`** | Selalu dikirim; label dan `sessionName` diganti; kelompok kosong → semua dokter tidak jaga (`IGD-DEC-239`) | — |
| `IGD-CAP-94` | Pemeriksaan ulang kelayakan saat penetapan dan pengalihan | IGD | `TetapkanAsync` `:183`, `AlihkanAsync` `:250`; `PeriksaDokterAsync` `:348` hanya memeriksa dokter aktif@c7e523fd | **`Extend`** | Tambah pemeriksaan validation §13.3 | Memutus perilaku — rilis bersama layar |
| `IGD-CAP-95` | Penanda dan alasan override | IGD | `Models/EmgDoctorAssignment.cs` — hanya `AssignmentReason`@c7e523fd | **`Missing`** | Dua kolom baru + migration (`IGD-DEC-238`) | Migration milik Rizki |
| `IGD-CAP-96` | Pemilih DPJP di dua pintu membaca satu sumber | Frontend IGD | `emergency-management-triage-slice.jsx` `fetchEmergencyDoctorOptions` `:698` (cadangan master `:718`); `emergency-doctor-option-utils.js` `isEmergencyDutyDoctor` `:18`, saringan `:97`@3335f4b53 | **`Repair`** | Hapus cadangan master dan saringan spesialisasi; tampilkan dua kelompok dan isian alasan | Saringan sekarang dapat menampilkan semua dokter (`:97`) |
| `IGD-CAP-97` | Penanda override pada riwayat penugasan | Frontend IGD | `DaftarRiwayatDokter` di `emergency-triage-doctor-section.jsx` `:70`@3335f4b53 | **`Extend`** | Tampilkan `isRosterOverride` dan alasannya | — |
| `IGD-CAP-98` | Tipe unit layanan | Master Data | `ServiceUnitType.Emergency = 3` (`Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs`)@c7e523fd | `Ready to reuse` | — | Isi data per lingkungan belum diketahui (`IGD-UNK-18`) |

## 2. Conflict

### `IGD-CONF-12` — perilaku berjalan di luar kontrak

Endpoint `on-duty-doctors` dan alias `options` masuk source pada `f39250df` tanpa kartu dan tanpa kontrak (API §8.5 saat
itu masih *"belum dikontrakkan"*), dan layar sudah memakainya. Diselesaikan dengan menjadikannya kontrak (API §11) dan
menyusun ulang kartu `BE-IGD-056`/`FE-IGD-037` di atas kode itu — bukan dengan membuangnya. Alias `options` tidak punya
pemakai dan dihapus (`IGD-DEC-240`, pilihan desain 7).

## 3. Kesimpulan scan

1. `S7` kini dapat dirancang: sumber roster sudah ada di source dan disahkan pemilik.
2. Empat butir backend (`IGD-CAP-92`…`95`) dan dua butir frontend (`IGD-CAP-96`, `97`) menjadi isi kartu.
3. Pemicu audit ulang suplemen ini: commit yang menyentuh `EmergencyDoctorAssignment*`, `EmgDoctorAssignment*`,
   `MstDoctorSchedule`, `ServiceUnitType`, atau berkas pemilih DPJP di frontend.

---

# Suplemen revision 3.6 — *impact scan* Billing IGD pada `c7e523fd` / `3335f4b53`

Scan terbatas untuk slice Billing IGD (`IGD-DEC-241`…`247`). Tujuannya menjawab tiga pertanyaan fakta source yang menahan
desain: `IGD-UNK-19` (kode tarif), `IGD-UNK-20` (jalur kirim dan kunci anti-ganda), dan `IGD-UNK-21` (apakah tindakan dan
obat pasien IGD tampil di layar kasir). Area billing **belum pernah diaudit** pada peta ini, jadi suplemen ini adalah audit
pertamanya. Status memakai tujuh status kontrak bukti yang sama dengan suplemen 3.4.

| Butir | Isi |
| --- | --- |
| Baseline lama | Area billing: belum pernah diaudit. Butir yang menyinggung billing hanya `IGD-CONF-10` dan `IGD-UNK-16` (suplemen 3.4, `43dab6da` / `6680278a2`) |
| Baseline baru | Backend `rizkiG` **`c7e523fd`**, frontend `RizkiV2` **`3335f4b53`** — sama dengan HEAD dan sejajar origin. Working tree: bagian 0 |
| Keputusan yang dilayani | `IGD-DEC-241`…`247` (*amendment pass* Billing IGD, 11 Oktober 2026) |
| Batas | Read-only terhadap source kedua repository; nol kueri basis data; nol perubahan source; nol uji runtime. Berkas log backend lokal (`Logs/`, 2–11 Oktober 2026) dibaca: nol baris `BillingBridge`, jadi tidak ada bukti runtime lokal |
| Tanggal | 11 Oktober 2026 |

## 0. Perbandingan SHA tercatat dengan HEAD

| Yang tercatat | Backend | Frontend | Dibanding HEAD 11 Oktober 2026 | Akibat |
| --- | --- | --- | --- | --- |
| Kepala peta (revision 3) | `f69e9e48` | `96a91201` | Jauh tertinggal | Butir revisi 3 di luar suplemen tidak dipakai scan ini |
| Suplemen 3.4 | `43dab6da` | `6680278a2` | Tertinggal | Di antara `43dab6da` dan `c7e523fd`, 39 berkas pada `BillingManagement`, `ClinicalMilestoneFactProducer`, `ConsultationFinalizationService`, dan `PrescriptionBillingChargeProducer` berubah (±2.600 baris ditambah, ±1.000 dihapus). Karena itu `IGD-CONF-10` dan `IGD-UNK-16` diperiksa ulang di bagian 5 |
| Suplemen 3.5 | `c7e523fd` | `3335f4b53` | **Sama** | — |
| Working tree backend | — | — | 4 berkas `.cs` IGD milik `BE-IGD-056` + migration dan snapshot; 2 berkas `.cs` Registrasi; dokumen blueprint | Nol berkas di `BillingManagement`, `PharmacyManagement`, dan `ClinicalManagement`, jadi hasil scan berlaku untuk HEAD + working tree. `EmergencyDoctorAssignmentService.cs` dibaca dari working tree; nomor barisnya dapat bergeser saat di-commit |
| Working tree frontend | — | — | Berkas pemilih DPJP `FE-IGD-037` dan Registrasi | Nol berkas billing |

## 1. Ringkasan untuk pemilik

1. **`IGD-UNK-21` — tidak tampil.** Hari ini tindakan, obat, laboratorium, dan radiologi pasien IGD **tidak** muncul di
   layar kasir. Layar kasir hanya membaca invoice, dan tidak ada satu jalur otomatis pun yang membuat invoice untuk
   kunjungan IGD. Invoice IGD hanya lahir bila kasir mengetik tagihan manual dari katalog tarif. *Contoh:* Tn. Budi
   (IGD) dipasangi infus dan diberi resep parasetamol. Di layar *Invoice & Billing Kasir* namanya tidak muncul sama
   sekali, karena invoicenya tidak pernah ada.
2. **`IGD-UNK-20` — jalurnya sudah ada, tetapi gerbangnya tertutup untuk IGD.** Jalur yang sah adalah *fakta klinis*
   (catatan bahwa pelayanan sudah terjadi, tanpa harga) lewat `ClinicalMilestoneFactProducer` → buku folio Billing →
   *jembatan* ke invoice. Jalur ini sudah punya anti-ganda dan kirim ulang otomatis — persis yang diminta `IGD-DEC-246`.
   Namun jembatan sengaja hanya meneruskan kunjungan rawat jalan dan rawat inap; kunjungan IGD dihentikan dengan kode
   `NOT_OUTPATIENT`. Kontrak rawat jalan menyerahkan pembukaan gerbang itu kepada pemilik modul IGD.
3. **`IGD-UNK-19` — belum ada di source.** Tidak ada kode, konstanta, seeder, atau aturan penetapan harga untuk *Jasa
   kunjungan IGD* maupun *Observasi IGD*. Master tarif tersimpan di basis data, jadi apakah barisnya sudah ada di tiap
   lingkungan tidak dapat diketahui dari source. Struktur `MstTariff` sudah cukup untuk menampungnya.
4. **Premis `IGD-DEC-247` tidak terpenuhi.** Keputusan itu menganggap tindakan dan obat *sudah mengalir hari ini*.
   Faktanya tidak ada yang sampai kasir, dan tindakan IGD bahkan tidak menerbitkan fakta tagih. Isi rilis 21 Oktober perlu
   diputuskan ulang (`IGD-OQ-121`).
5. **Temuan samping yang menyangkut keselamatan pasien — inferensi, belum diuji runtime.** Farmasi menahan telaah,
   penyiapan, pemeriksaan akhir, dan penyerahan obat sampai ada *surat clearance* (tanda lunas atau dijamin) dari Billing,
   tanpa pengecualian jenis kunjungan. Surat itu hanya terbit dari invoice. Karena resep IGD tidak pernah sampai invoice,
   resep IGD kemungkinan **tidak dapat ditelaah Farmasi sama sekali**. Ini bertentangan dengan keputusan Farmasi sendiri
   bahwa tagihan obat IGD timbul setelah obat diberikan (`PHA-DEC-007`; `IGD-CONF-15`).

## 2. Proses bisnis hari ini — perjalanan satu tagihan

**Tujuan:** pelayanan yang sudah diberikan muncul sebagai baris tagihan di layar kasir, tanpa petugas klinis mengetik
harga.

**Pelaku:** dokter dan perawat memberi pelayanan; sistem mengirim fakta dan menetapkan harga; kasir melihat dan menagih.

**Pemicu:** pelayanan selesai — catatan dokter diselesaikan, tindakan dikerjakan, spesimen diterima, citra diambil.

**Langkah untuk pasien rawat jalan — jalur yang berfungsi:**

1. Dokter menyelesaikan catatan dokter beserta resep (`PATCH …/doctor-consultations/{id}/complete`).
2. Sesudah data klinis tersimpan, sistem menerbitkan fakta klinis (`ClinicalMilestoneFactProducer`).
3. Billing mencatat fakta itu sebagai baris folio, lalu jembatan meneruskannya ke invoice kunjungan
   (`BillingClinicalChargeBridgeService`).
4. Jembatan mencari harga di master tarif menurut tindakan atau obat, kelas pasien, dan tanggal pelayanan
   (`BillingSourceTariffResolver`). Petugas klinis tidak pernah mengirim harga.
5. Invoice lahir, atau bertambah satu baris, dengan label jenis layanan `RAJAL`.
6. Kasir membuka *Invoice & Billing Kasir* dan melihat pasien itu beserta totalnya.

**Langkah yang sama untuk pasien IGD — tempat berhentinya:**

| Langkah | Tindakan dokter IGD | Tindakan perawat IGD | Resep IGD | Lab / radiologi IGD |
| --- | --- | --- | --- | --- |
| Fakta klinis terbit? | **Tidak.** Tindakan disimpan berstatus *Planned*; fakta hanya terbit saat tindakan di-*execute*, dan layar IGD tidak pernah memanggilnya | **Tidak.** Tindakan disimpan langsung *Completed* tanpa menerbitkan fakta | Ya, saat catatan dokter diselesaikan | Ya, saat spesimen diterima / citra diambil |
| Sampai folio Billing? | — | — | Ya | Ya |
| Diteruskan ke invoice? | — | — | **Tidak** — `NOT_OUTPATIENT` | **Tidak** — `NOT_OUTPATIENT` |
| Ada yang tahu? | — | — | Tidak. Baris `NotApplicable` tidak tampil di layar rekonsiliasi dan tidak dapat dikirim ulang | Sama |

*Contoh:* Ny. Sari datang ke IGD pukul 10.00. Perawat mencatat pemasangan infus, dokter jaga menulis catatan dokter berisi
resep ondansetron dan meminta darah rutin. Pukul 12.00 kasir mencari Ny. Sari: tidak ada. Di Billing hanya ada dua baris
folio — resep dan laboratorium — yang berhenti diam-diam dengan kode `NOT_OUTPATIENT`. Infusnya tidak tercatat sama sekali.

**Jalur tidak normal yang sudah tersedia pada jalur yang berfungsi:** bila Billing gagal dihubungi, fakta dan baris folio
tetap tersimpan; pekerja latar `ClinicalFactDispatchWorker` dan `BilInvoiceSyncWorker` mengirim ulang dengan jeda yang makin
panjang; sesudah batas percobaan habis, baris masuk antrean rekonsiliasi Billing. Data klinis tidak pernah ikut batal.

## 3. Kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `IGD-CAP-99` | Layar kasir menampilkan invoice IGD | Billing/Kasir (frontend) | `cashier-overview-view.jsx` `getServiceTypeBadge` `:72`–`77` (lencana `IGD`); `use-cashier-billing-overview.js` `buildListParams` `:30`–`42`; *Running Invoice* `billing-invoice-constants.js` `:152`–`158` (saringan `IGD`); menu *Billing dan Kasir → Invoice & Billing Kasir* `menu-items.jsx` `:2136`@3335f4b53 | `Ready to reuse` | Nol perubahan begitu invoice IGD ada. *Invoice & Billing Kasir* tidak menyaring jenis layanan — pasien IGD tampil bersama rawat jalan, dibedakan lencana; *Running Invoice* punya saringan `IGD` | Tidak ada layar bernama *Kasir IGD*; demo memakai dua layar ini |
| `IGD-CAP-100` | Daftar kasir dan invoice untuk kunjungan IGD | Billing | `BillingInvoiceService.GetCashierOverviewAsync` `:507`–`551` (hanya `BilInvoices`; bawaan `OPEN` dan hari ini); `MapServiceType` `:2356` (`Emergency → "IGD"`); `BillingInvoicesController` `:79` `BillingInvoice : Read`@c7e523fd | `Ready to reuse` | — | Pasien tanpa invoice tidak pernah tampil |
| `IGD-CAP-101` | Saluran fakta klinis → folio Billing | ClinicalManagement → Billing | `ClinicalMilestoneFactProducer.EmitChargeEligibilityAsync` `:128`; identitas fakta = sumber + agregat + butir + jenis efek, revisi bernomor, kirim ulang memakai kunci yang sama `:353`–`360`; wajib di luar transaksi `:154`–`159`; `BillingFolioService.RecognizeMilestoneAsync` `:122`–`140`@c7e523fd | `Ready to reuse` | Menerima encounter jenis apa pun | — |
| `IGD-CAP-102` | Jembatan folio → invoice untuk kunjungan IGD | Billing | `BillingClinicalChargeBridgeService.BuildPlanAsync` `:139`–`140` (selain `Outpatient`/`Inpatient` → `NotApplicable NOT_OUTPATIENT`); preseden rawat inap `BE-RWI-155` `:135`–`138`; `rawat-jalan/contracts/integration-contract.md` V2-5 `:218`@c7e523fd | `Extend` | Tambah `Emergency` pada gerbang, dengan aturan per jenis: tindakan, lab, radiologi seperti rawat jalan; obat menunggu `IGD-OQ-122` | Perubahan di area Billing — titik sentuh (bagian 7) |
| `IGD-CAP-103` | Sumber *jasa kunjungan IGD* pada daftar sumber Billing | Billing | `BillingSourceContract.AllowedEffectTypes` `:88`–`99` (sumber tak terdaftar ditolak `BIL_SOURCE_INVALID`); `IsCandidateSourceContext` `:58`–`66`; `MapDomain` `:627`–`637`; domain `EMERGENCY` pada adapter `:91`–`97`@c7e523fd | `Missing` (daftar sumber, calon jembatan, pemetaan domain) / `Ready to reuse` (domain `EMERGENCY`) | Satu konteks sumber dan jenis efek baru, dipetakan ke domain `EMERGENCY` | Domain `EMERGENCY` belum pernah dipakai pengirim mana pun (`IGD-FACT-090`) |
| `IGD-CAP-104` | Penetapan harga jasa kunjungan IGD | Billing; data milik pemilik master tarif | `BillingSourceTariffResolver.ResolveAsync` `:33`–`45` (sumber tanpa cabang → `SOURCE_PENDING_SUPPORT`); `FindTariffAsync` `:402`–`428` (menurut `ProcedureId`, klinik, kelas pasien, tanggal berlaku); `MstTariff` `ServiceUnitId` `:22`, `ProcedureId` `:25`, `ChargeBasis` `:42` (`PerService`/`PerHour`), `IsConsultationFee` `:57`; pencarian `kunjungan igd`, `EMERGENCY_VISIT`, `TariffCode = "` di source: kosong; tidak ada seeder tarif@c7e523fd | `Missing` (aturan) / `Unknown` (data) | Satu cabang penetapan harga; bentuknya bergantung `IGD-OQ-123` | Tarif tidak ditemukan → baris masuk rekonsiliasi `TARIFF_NOT_FOUND`, tidak pernah Rp0 |
| `IGD-CAP-105` | Anti-ganda dan kirim ulang (`IGD-DEC-246`) | Billing | `DeterministicKey` `:618`–`625` (per fakta dan revisi); `BilChargeReceipt` dan pencocokan isi `UpsertChargeAsync` `:1776`–`1787`; satu baris per (`SourceDomain`, `SourceDetailId`) yang belum di-void `:1846`–`1863`; satu invoice per kunjungan, kunci penasihat `:1772`–`1773`; `BilInvoiceSyncWorker` dan `ClinicalFactDispatchWorker` (`Program.cs` `:1011`, `:1013`); jeda dari `MstBillingSyncPolicy` `INVOICE_SYNC` `RecordAsync` `:542`–`565`@c7e523fd | `Ready to reuse` | — | Tanpa kebijakan `INVOICE_SYNC` aktif, kirim ulang mati dan baris langsung ke rekonsiliasi (`IGD-UNK-23`) |
| `IGD-CAP-106` | Pemicu jasa kunjungan pada penetapan DPJP pertama | IGD | `EmergencyDoctorAssignmentService.TetapkanAsync` `:195`; `SimpanAsync` `:254` (tanpa transaksi eksplisit); penjaga hanya *penugasan berjalan* `:226`–`233`; `SelaraskanDokterEncounterAsync` `:403`–`421` (kunjungan tanpa encounter dilewati)@working tree `BE-IGD-056` | `Extend` | Terbitkan fakta sesudah simpan berhasil; agregat = kunjungan IGD sehingga satu kunjungan satu baris | Source masih working tree `BE-IGD-056`; kunjungan tanpa encounter tidak dapat ditagih |
| `IGD-CAP-107` | Fakta tagih tindakan dokter IGD | ClinicalManagement / IGD | `PatientProcedureController` `POST /` `:883`–`917` (`ExecuteImmediately` bawaan salah → `Planned`, tanpa fakta); fakta hanya lewat `PATCH {id}/execute` `:1303` (`PatientProcedure : Execute`) → `PatientProcedureExecutionService.ExecuteAsync` `:61`, `:162`@c7e523fd; `emergency-doctor-procedure-tab.jsx` payload `:319`–`331` tanpa `executeImmediately`; nol pemanggil `…/execute` di seluruh `src`@3335f4b53 | `Missing` | Jalur yang menerbitkan fakta untuk tindakan yang dikerjakan saat itu juga di IGD | Tanpa ini, resusitasi yang dicatat dokter tidak pernah tertagih (`IGD-DEC-243`) |
| `IGD-CAP-108` | Fakta tagih tindakan perawat IGD | ClinicalManagement / IGD (`BE-IGD-069`) | `PatientProcedureOrderService.CreateEmergencyNursingActionAsync` `:314` — simpan langsung `Completed` `:437`, `IsExecuted = true` `:460`, `IsBillingGenerated = false` `:468`; nol pemanggilan `ClinicalMilestoneFactProducer` di berkas itu@c7e523fd | `Missing` | Terbitkan fakta tindakan sesudah simpan | Laporan `BE-IGD-069` baris 30 menyebut tindakan perawat *"terbaca pada … ringkasan tagihan (Billing)"*. Source tidak mendukung: ringkasan tagihan per kunjungan membaca invoice dan menjawab *"Belum ada invoice untuk kunjungan ini."* (`GetChargeSummaryByEncounterAsync` `:1024`–`1038`). Laporan itu tidak disunting sesi ini |
| `IGD-CAP-109` | Tagihan resep IGD sampai invoice | Farmasi → Billing | Fakta resep saat finalisasi `ConsultationFinalizationService` `:251`–`254` → jembatan `NOT_OUTPATIENT`. Kiriman langsung `PrescriptionBillingChargeProducer` (`SourceStatus = "SUBMITTED"` `:40`, kontrak `BIL-INTEGRATION-1.2` `:43`, dipanggil `:281`) ditolak adapter untuk **semua** jenis kunjungan, karena domain `PHARMACY` hanya menerima `DISPENSED`, atau `PRESCRIBED` pada kontrak 1.3 (`BillingChargeSourceAdapter.ValidateAndNormalize` `:114`–`115`). Rawat inap menunggu obat diserahkan (`BuildPlanAsync` `:205`–`207`)@c7e523fd | `Conflict` | Kapan obat IGD ditagih dan apakah Farmasi menunggu lunas (`IGD-OQ-122`) | Kiriman langsung yang selalu ditolak adalah urusan Farmasi/Billing, bukan IGD — titik sentuh (bagian 7) |
| `IGD-CAP-110` | Gerbang lunas Farmasi untuk resep IGD | Farmasi | Empat gerbang tanpa pengecualian jenis kunjungan: telaah `PrescriptionReviewService` `:64`, penyiapan `PrescriptionPreparationService` `:31`, pemeriksaan akhir `PrescriptionFinalCheckService` `:62`, penyerahan `PrescriptionDispensingService` `:172`, `:301`. Tanpa surat → `UNKNOWN`, *"Keadaan pembayaran resep ini belum diketahui. Obat belum boleh diserahkan."* (`PrescriptionFinancialClearanceService.DescribeAsync` `:129`–`139`). Surat hanya terbit dari invoice (`BilConsumerHandoffService.PublishForClearanceChangeAsync(invoiceId, …)` `:192`)@c7e523fd | `Conflict` (dengan keputusan Farmasi) / `Unknown` (runtime) | Kebijakan gerbang untuk resep IGD (`IGD-OQ-122`) | **Keselamatan pasien:** obat gawat darurat dapat tertahan di Farmasi. Belum dibuktikan runtime (`IGD-UNK-22`) |
| `IGD-CAP-111` | Lab dan radiologi IGD sampai invoice | Lab, Radiologi → Billing | Fakta terbit `LabSpecimenService` `:1684`, `RadStudyService` `:1253`; calon jembatan `:59`–`61`; berhenti di gerbang `:139`@c7e523fd | `Extend` | Ikut perluasan `IGD-CAP-102` | Di luar batas rilis `IGD-DEC-247` kecuali pemilik memasukkannya |
| `IGD-CAP-112` | Baris IGD yang berhenti terlihat petugas | Billing | Antrean rekonsiliasi hanya `Failed`, `ReconciliationRequired`, `Resolved` (`BillingChargeReconciliationService` `:158`–`161`); kirim ulang hanya dari `Pending`, `Failed`, `ReconciliationRequired` `:254`–`257`@c7e523fd | `Missing` (bagi IGD) | Baris `NotApplicable` lama tidak dapat dihidupkan lewat layar | Pelayanan IGD sebelum gerbang dibuka tidak akan pernah sampai kasir; demo memakai kunjungan baru |
| `IGD-CAP-113` | Tagihan manual kasir untuk kunjungan IGD | Billing/Kasir | `GetActiveEncounterOptionsAsync` `:1332` (tanpa saringan jenis kunjungan; status `Registered`…`Billing` `:1316`–`1326`); `catalog-charges` (`ADHOC_CATALOG`); pengaman `UpsertManualChargeAsync` `:1741`–`1751` hanya menolak domain klinis untuk rawat jalan dan rawat inap — untuk IGD `from-source` lolos@c7e523fd | `Ready to reuse` (sebagai cadangan) | — | Begitu jembatan IGD dibuka, jalur manual domain klinis IGD membuka baris ganda; pengaman `:1748` perlu diperluas bersamaan |
| `IGD-CAP-114` | Biaya administrasi IGD | Billing | `AdministrationFeeServiceTypes.Igd` (`AdministrationFeePolicyDtos.cs` `:8`); kebijakan dicari menurut `ServiceType` invoice; tanpa kebijakan aktif → nol rupiah dengan catatan (`AdministrationFeeCalculationService` `:56`–`66`)@c7e523fd | `Ready to reuse` | — | Isi kebijakan `IGD` per lingkungan belum diketahui — urusan Billing |
| `IGD-CAP-115` | Rincian IGD pada invoice rawat inap gabungan | Billing | `BilInvoiceEncounterLink` dibaca `PatientBillingSummaryService.Breakdown` `:100`–`117` (membaca invoice kunjungan IGD itu sendiri)@c7e523fd; lencana `[IGD]` mengunci `sourceDomain === "EMERGENCY"` (`billing-invoice-items-table.jsx` `:88`, `billing-inpatient-summary-panel.jsx` `:94`)@3335f4b53 | `Reuse with adapter` | Bergantung `IGD-CONF-14` | Rilis berikutnya |

## 4. Kontrak as-is yang disentuh

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Yang relevan untuk IGD | Hak akses |
| --- | --- | --- | --- |
| `GET` | `cashier-overview` | Sumber layar *Invoice & Billing Kasir*. Hanya invoice; bawaan status `OPEN` dan tanggal kunjungan hari ini; menerima `serviceType`, tetapi layar tidak mengirimnya | `BillingInvoice : Read` |
| `GET` | `/` | Sumber *Running Invoice*; saringan `serviceType=IGD` | `BillingInvoice : Read` |
| `GET` | `encounters/{encounterId}/charge-summary` | Kunjungan tanpa invoice dijawab 404 *"Belum ada invoice untuk kunjungan ini."* | `BillingInvoice : Read` |
| `GET` | `encounter-options` | Pilihan kunjungan untuk tagihan manual; kunjungan IGD ikut tampil | `BillingInvoice : Read` |
| `POST` | `catalog-charges` | Kasir menambah tarif katalog (`ADHOC_CATALOG`). Untuk IGD inilah satu-satunya cara invoice lahir hari ini | `BillingInvoice : Create` |
| `POST` | `from-source` | Kiriman charge dengan header `Idempotency-Key`; untuk IGD domain klinis tidak ditolak (`IGD-CAP-113`) | `BillingInvoice : Create` |

### Health Services / Billing Management / Billing / Charge Reconciliations

Base URL: `api/v1/health-services/billing-management/billing/charge-reconciliations`

| Method | Path | Yang relevan untuk IGD | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Tidak menampilkan baris `NotApplicable`, jadi baris IGD yang berhenti tidak terlihat | `BillingChargeReconciliation : Read` |
| `POST` | `{itemType}/{id}/retry` | Tidak menerima baris `NotApplicable` | `BillingChargeReconciliation : Update` |

### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Yang relevan untuk IGD | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Tindakan dokter IGD; tersimpan *Planned*, tanpa fakta tagih | `PatientProcedure : Create` |
| `PATCH` | `{id}/execute` | Satu-satunya pintu yang menerbitkan fakta tindakan; tidak dipanggil layar IGD | `PatientProcedure : Execute` |
| `POST` | `emergency-nursing-actions` | Tindakan perawat IGD; tersimpan *Completed*, tanpa fakta tagih | `PatientProcedure : Create` |

### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Yang relevan untuk IGD | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `{id}/complete` | Fakta resep terbit; jasa konsultasi tidak dikirim untuk IGD (`ConsultationFinalizationService` `:222`). Respons membawa `billingHandoffIssues`; untuk resep diperkirakan berisi `PHM_CHARGE_FAILED: Jumlah obat yang diserahkan belum final.` (inferensi dari `IGD-CAP-109`, dibuktikan lewat uji) | Sesuai suplemen 3.4 |

### Kontrak di dalam proses (bukan HTTP)

| Kontrak | Ruas | Catatan |
| --- | --- | --- |
| `ClinicalMilestoneFactRequest` | `SourceContext`, `SourceAggregateId`, `SourceItemId?`, `EffectType`, `EncounterId`, `OccurredAt`, `Quantity?`, `Unit?`, `TariffSnapshot?`, `RuleSnapshot?`, `CorrelationId?`, `CausationId?` | Tanpa harga. Harga ditetapkan Billing — sejalan dengan `IGD-DEC-241` |
| `UpsertChargeRequest` | `EncounterId`, `SourceDomain`, `SourceDetailId`, `SourceVersion`, `SourceStatus`, `OccurredAt`, `CategoryId`, `TariffId?`, `DescriptionSnapshot`, `Quantity`, `UnitPrice`, `DoctorShare`, `ContractVersion` (`BIL-INTEGRATION-0.4`/`1.2`/`1.3`), `CorrelationId`, `CausationId` | **Pengirim wajib membawa harga dan kategori tarif** |

## 5. Jawaban tiga pertanyaan, dan status butir lama

| ID | Jawaban | Sisa |
| --- | --- | --- |
| `IGD-UNK-19` | **Dijawab sebagian.** Source: tidak ada kode, tarif, atau aturan penetapan harga untuk jasa kunjungan maupun observasi IGD. Struktur `MstTariff` cukup — `ProcedureId`, kelas pasien, unit layanan, dan dasar hitung per jam untuk observasi kelak | Data: apakah barisnya sudah ada di layar *Master Tarif* tiap lingkungan (dilihat pemilik, tanpa mengubah). Bentuk pengenalnya: `IGD-OQ-123` |
| `IGD-UNK-20` | **Dijawab.** Jalur sah: fakta klinis → folio → jembatan → invoice. Kunci anti-ganda berlapis: identitas fakta (sumber + kunjungan IGD + jenis efek) → kunci deterministik per fakta dan revisi → `BilChargeReceipt` → satu baris per (`EMERGENCY`, id kunjungan). *Upsert* langsung tidak direkomendasikan: pengirim wajib membawa harga (berlawanan dengan `IGD-DEC-241`), tidak ada kirim ulang otomatis, dan satu-satunya contoh kiriman langsung (resep) justru selalu ditolak | Perubahan di area Billing (bagian 7) |
| `IGD-UNK-21` | **Dijawab dari source: tidak tampil.** Kekurangannya struktural — jalur ke invoice untuk IGD tidak ada — sehingga jawabannya tidak bergantung data lingkungan. Uji layar konfirmasi **dibatalkan** pemilik 11 Oktober 2026 karena alur klinis IGD (`MVP-9`) belum lulus uji; bukti runtime dipindah ke uji ujung-ke-ujung kartu Billing IGD (`IGD-DEC-251`) | — |
| `IGD-UNK-16` (suplemen 3.4) | **Dijawab.** Billing tidak menerima fakta jasa konsultasi untuk encounter IGD — tidak dikirim (`ConsultationFinalizationService` `:222`). Fakta resep IGD diterima folio, lalu berhenti `NOT_OUTPATIENT` | — |
| `IGD-CONF-10` (suplemen 3.4) | **Selesai** lewat `IGD-DEC-229` dan `BE-IGD-068`; diverifikasi di `:222`@c7e523fd | — |

## 6. Conflict

### `IGD-CONF-13` — premis rilis pertama tidak terpenuhi

`IGD-DEC-247` menetapkan rilis 21 Oktober berisi jasa kunjungan *ditambah tindakan dan obat yang sudah mengalir hari ini*.
Source menunjukkan tidak ada yang mengalir sampai kasir: tindakan IGD tidak menerbitkan fakta (`IGD-CAP-107`, `108`), resep
dan penunjang berhenti di jembatan (`IGD-CAP-102`, `109`, `111`). *Contoh:* bila hanya jasa kunjungan yang dikerjakan, pada
demo kasir melihat Tn. Budi dengan satu baris *Jasa kunjungan IGD* — tanpa infus dan tanpa obatnya. **Butuh keputusan
pemilik** (`IGD-OQ-121`).

### `IGD-CONF-14` — `SourceDomain = "EMERGENCY"` untuk semua baris IGD, atau hanya untuk baris milik IGD

`BKC-DEC-117` menyebut seluruh item selama di IGD mencatat `SourceDomain` asalnya `EMERGENCY`, dan lencana `[IGD]` di
frontend Billing mengunci nilai itu. Sementara jembatan memetakan domain menurut **jenis** pelayanan (`PROCEDURE`,
`PHARMACY`, `LABORATORY`, `RADIOLOGY`), dan surat clearance Farmasi hanya mengenali baris `PHARMACY` ber-`SourceDetailId` =
id resep. Rawat inap sudah memakai domain per jenis, bukan `INPATIENT`. *Contoh:* bila resep IGD ditulis sebagai
`EMERGENCY`, surat clearance tidak menemukan resepnya dan obat tertahan; bila ditulis `PHARMACY`, lencana `[IGD]` tidak
muncul pada invoice rawat inap gabungan. Diusulkan diselesaikan pada desain: domain per jenis untuk tindakan, obat, dan
penunjang; `EMERGENCY` hanya untuk baris milik IGD (jasa kunjungan, kelak observasi); lencana membaca jenis kunjungan asal.
Membutuhkan persetujuan tim Billing karena menafsirkan `BKC-DEC-117`.

### `IGD-CONF-15` — keputusan Farmasi tentang obat IGD versus gerbang lunas

Decision log Farmasi: *"Tagihan rawat inap/IGD timbul setelah obat benar-benar diberikan kepada pasien"* dan *"Dispensing
rawat inap/IGD final ketika obat diserahkan kepada unit"* (`PHA-DEC-007`, `approved`; `pharmacy/00-interview-decisions.md`
`:116`–`119`, `:158`). Source
Farmasi: empat gerbang menolak resep tanpa surat lunas, tanpa pengecualian IGD (`IGD-CAP-110`). *Contoh:* dokter jaga
meresepkan ondansetron injeksi untuk pasien muntah hebat; Farmasi tidak dapat memulai telaah karena *"Keadaan pembayaran
resep ini belum diketahui"*. **Butuh keputusan** (`IGD-OQ-122`), dengan Farmasi sebagai pemilik gerbang.

## 7. Titik sentuh di luar IGD — dicatat, tidak disunting

| Modul | Titik sentuh | Untuk |
| --- | --- | --- |
| Billing — source | Gerbang jembatan `:139`; `BillingSourceContract`; `IsCandidateSourceContext`; `MapDomain`; `MapBillableStatus` `:689`–`697`; cabang baru `BillingSourceTariffResolver`; pengaman `from-source` `:1748` | Jasa kunjungan IGD; tindakan, lab, radiologi IGD |
| Billing — blueprint | Tafsir `BKC-DEC-117` dan lencana `[IGD]` (`IGD-CONF-14`); kebijakan administrasi `IGD` per lingkungan | Konsolidasi rawat inap, rilis berikutnya |
| ClinicalManagement | Penerbitan fakta untuk tindakan perawat IGD (`IGD-CAP-108`) dan tindakan dokter IGD (`IGD-CAP-107`) | Tindakan dan resusitasi IGD |
| Farmasi | Kebijakan gerbang lunas untuk resep IGD (`IGD-CAP-110`); kiriman langsung `PrescriptionBillingChargeProducer` yang selalu ditolak adapter (`IGD-CAP-109`) — berlaku juga untuk rawat jalan | Obat IGD; temuan samping untuk pemilik Farmasi |
| Rawat Jalan — blueprint | Kontrak V2-5 `:218` sudah menyerahkan pembukaan gerbang IGD kepada pemilik IGD | Tidak perlu diubah |

## 8. Unknown

| ID | Hal | Pemilik | Cara menjawab |
| --- | --- | --- | --- |
| `IGD-UNK-22` | Apakah Farmasi benar-benar menolak telaah resep IGD karena surat clearance tidak ada | Farmasi / Rizki | Saat alur resep layar dokter IGD (`FE-IGD-049`) diuji: telaah resep hasil uji oleh akun Apoteker |
| `IGD-UNK-23` | Apakah kebijakan kirim ulang `INVOICE_SYNC` aktif di tiap lingkungan; tanpanya kirim ulang mati (`IGD-DEC-246`) | Billing | `GET /api/v1/health-services/billing-management/master-data/billing-sync-policies` dengan akun ber-hak `BillingSyncPolicy : Read` — belum ada layar frontend |

## 9. Catatan desain dari fakta — bukan keputusan

1. **Satu kunjungan, satu baris.** Jasa kunjungan paling murah lewat fakta klinis dengan agregat = kunjungan IGD, dipetakan ke
   `SourceDomain = "EMERGENCY"` dan `SourceDetailId` = id kunjungan. Pengalihan DPJP tidak menerbitkan fakta. Bila
   `TetapkanAsync` terpanggil lagi pada kunjungan yang sama, hasilnya revisi baris yang sama, bukan baris kedua.
2. **Status sumber `PERFORMED`, bukan `CONFIRMED`.** Adapter `EMERGENCY` menganggap `CONFIRMED`, `ACCEPTED`, dan `ADDED`
   belum selesai, sehingga invoice tidak dapat difinalkan selama baris itu ada (`IsOrderComplete` `:149`–`162`).
   Konsekuensinya, pembatalan kunjungan (rilis berikutnya, `IGD-DEC-245`) berjalan sebagai penyesuaian kredit yang menunggu
   persetujuan Billing (`BuildCancellationPlanAsync` `:403`–`408`), bukan *void* langsung. Ini sejalan dengan *"masuk
   daftar tinjau Billing"*, tetapi kata `VOIDED` pada `IGD-DEC-245` perlu diselaraskan saat desain rilis berikutnya.
3. **Di luar transaksi, sesudah simpan.** Produsen fakta menolak bila transaksi masih terbuka (`:154`). Kegagalan
   menerbitkan tidak boleh membatalkan penetapan DPJP; pekerja latar yang mengirim ulang.
4. **Kunjungan tanpa encounter** dilewati penetapan DPJP; untuk tagihan, ia tidak dapat ditagih sampai encounter tertaut.
   Dengan *encounter-first* hal ini seharusnya tidak terjadi, tetapi perlu penjaga dan catatan.
5. **Satu pengirim per baris.** Pengaman `from-source` diperluas ke `Emergency` bersamaan dengan pembukaan gerbang.
6. **Data lama tidak ikut.** Baris folio IGD yang sudah `NotApplicable` tidak dapat dikirim ulang (`IGD-CAP-112`); demo
   memakai kunjungan yang dibuat sesudah perubahan.

## 10. Pemicu audit ulang suplemen ini

Commit yang menyentuh `BillingClinicalChargeBridgeService`, `BillingSourceContract`, `BillingChargeSourceAdapter`,
`BillingSourceTariffResolver`, `BillingInvoiceService.UpsertChargeAsync`/`UpsertManualChargeAsync`/`GetCashierOverviewAsync`,
`BillingChargeReconciliationService`, `ClinicalMilestoneFactProducer`, `PatientProcedureController`,
`PatientProcedureOrderService.CreateEmergencyNursingActionAsync`, `ConsultationFinalizationService`,
`PrescriptionBillingChargeProducer`, gerbang `PrescriptionFinancialClearanceService`, `EmergencyDoctorAssignmentService`,
atau berkas `billing-invoices/cashier-overview/**` dan `use-cashier-billing-overview.js` di frontend.

## 11. Kesimpulan scan

1. **Desain jasa kunjungan IGD dapat dimulai** begitu `IGD-OQ-123` dijawab: jalurnya ada (`IGD-CAP-101`, `105`), dan yang
   kurang terbatas pada pemicu IGD (`IGD-CAP-106`), satu sumber dan satu cabang harga di Billing (`IGD-CAP-103`, `104`), serta
   pembukaan gerbang (`IGD-CAP-102`).
2. **Rilis 21 Oktober perlu diputuskan ulang** (`IGD-OQ-121`), karena tindakan dan obat IGD hari ini tidak mengalir.
3. **Obat IGD bergantung keputusan Farmasi** (`IGD-OQ-122`, `IGD-CONF-15`) dan membawa risiko keselamatan yang perlu
   dibuktikan runtime (`IGD-UNK-22`).
4. **Satu tafsir keputusan Billing** (`IGD-CONF-14`) diselesaikan pada desain bersama tim Billing.
