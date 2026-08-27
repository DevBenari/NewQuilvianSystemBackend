# IGD — Existing Capability Map

| Field | Value |
|---|---|
| `module` | `igd` |
| `revision` | `3` |
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
