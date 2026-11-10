# Rehab Medik — Peta Kemampuan Existing

| Field | Nilai |
|---|---|
| Blueprint ID | `RHB-BP-001` |
| Revision | `1` |
| Status | `draft` — hasil audit read-only |
| Decision log acuan | `00-interview-decisions.md` revision 1 (RHB-DEC-001..012) |
| Backend SHA | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) |
| Frontend SHA | `0c06c09305e6ee9c4f21e85393ebc7de10576550` (`sukmagpV2`) |
| Tanggal audit | 5 November 2026 |
| Contract version | Belum ada kontrak to-be |

## 1. Batas Audit

Audit hanya membaca **source code** pada dua SHA di atas. Isi basis data (misalnya apakah poli
rehab sudah dibuat di master klinik) **tidak** diperiksa, karena audit ini tidak punya akses ke
basis data.

Klaster yang ditelusuri: pasien, kunjungan/encounter, tenaga dan profesi, lokasi/poli,
alur status, dokumen dan rekam medis, sumber permintaan, penomoran, hak akses, billing,
integrasi luar, serta konsumen FE (menu, service, komponen).

**Pemicu impact scan:** salah satu SHA berubah, atau ada perubahan pada
`MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs`,
`ClinicalManagement/Services/ClinicalInstrumentService.cs`,
`RegistrationManagement/Models/RegPatientEncounter.cs`, atau
`HemodialysisManagement/Services/HmdServiceSupport.cs`.

---

## 2. Jawaban Dua Pertanyaan Terbuka

### RHB-OQ-003 — Mekanisme finalisasi dan addendum yang dipakai ulang

**Jawaban: pakai mesin keutuhan dokumen milik Rekam Medis (`Mrc`).** Status:
`REUSE WITH ADAPTER`. Hemodialisa dan Optik sudah memakai mesin ini.

Cara kerjanya:

1. Modul pemilik dokumen menyimpan dokumennya sendiri, misalnya `HmdSession`.
2. Modul itu mendaftarkan dokumen ke `MrcClinicalDocumentIntegrity` lewat
   `ClinicalDocumentIntegrityService.RegisterAsync(kind, documentId, patientId, encounterId, authorUserId)`.
   Status awalnya `Draft`.
3. Penulis menandatangani lewat `SignAsync(...)`. Status menjadi `Signed`, dan dokumen terkunci
   (`LockTrigger = AuthorSigned`). Perangkat dan IP diambil server, bukan dari kiriman layar.
4. Sebelum setiap perubahan, modul memanggil `EnsureMutableAsync(kind, documentId)`. Bila dokumen
   sudah terkunci, perubahan ditolak.
5. Koreksi dibuat sebagai addendum (`MrcClinicalNoteAddendum`) lewat endpoint milik Rekam Medis
   `POST api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}`.
   Addendum langsung final, tidak bisa diubah, dan wajib punya alasan. Ada pula jalur
   *penulis pengganti* (`as-substitute`) berbasis penetapan delegasi.
6. Dokumen final dibaca berkas rekam medis lewat `IMedicalRecordDocumentSource`, yang
   diimplementasikan modul pemilik.

**Contoh.** Fisioterapis menyelesaikan catatan sesi pukul 10.40 lalu menandatanganinya.
Pukul 14.00 ia sadar durasinya salah. Layar rehab tidak bisa mengubah catatan itu karena
`EnsureMutableAsync` menolak. Ia membuat addendum "Durasi 30 menit, bukan 45 — salah ketik",
dan berkas rekam medis menampilkan catatan asli beserta addendumnya.

**Adapter yang dibutuhkan (milik modul lain):**

| Kebutuhan | Pemilik | Preseden persetujuan |
|---|---|---|
| Nilai baru `ClinicalDocumentKind` untuk dokumen rehab (nomor 47 dst.) | Rekam Medis | `HemodialysisSession = 14`, `OpticalRefraction = 45` |
| Menambahkan nilai itu ke himpunan `JenisYangDitegakkan` | Rekam Medis (Yoga Aji Pratama) | `OPT-APR-007` (3 Nov 2026), izin pemilik Hemodialisa 22 Sep 2026 |
| Implementasi `IMedicalRecordDocumentSource` untuk rehab dan pendaftarannya di `Program.cs` | Rehab (dengan sepengetahuan Rekam Medis) | `HmdMedicalRecordDocumentSource`, `Program.cs:716` |

> **Peringatan.** Menambah nilai enum saja **tidak cukup**. Tanpa masuk `JenisYangDitegakkan`,
> dokumen tampil "Ditandatangani" tetapi tetap bisa disunting. Kejadian ini sudah dicatat
> sebagai Temuan Kritis 1 pada Hemodialisa.

### RHB-OQ-004 — Tautan sesi ke kunjungan dan rawat inap

**Jawaban: ikuti pola Hemodialisa.** Status: `REUSE WITH ADAPTER`.

| Setting | Yang dirujuk sesi rehab | Bukti |
|---|---|---|
| Rawat jalan berseri | Satu `RegPatientEncounter` bertipe `Outpatient` per kunjungan. Kunjungan didaftarkan oleh modul Registrasi Rawat Jalan ke poli yang `ClinicType = Rehabilitation`. Rehab **tidak** membuat kunjungan sendiri. | `RegPatientEncounter.ClinicId`, `MstClinic.ClinicType`, `EncounterIntakeService.cs:705` |
| Rawat inap | `RegPatientEncounter` rawat inap (satu untuk seluruh masa rawat) **ditambah** `InpEpisodeId`. Pasien yang sudah pulang secara fisik (`InpEpisode.PhysicallyLeftAt`) dianggap kunjungannya berakhir. | `InpEpisode.EncounterId`, `HmdSession.EncounterId` + `InpEpisodeId` |
| IGD | `RegPatientEncounter` bertipe `Emergency` | `EncounterType.Emergency = 2` |

Pola pemeriksaan yang sudah teruji ada di `HmdServiceSupport.CheckEncounterAsync`. Fungsi itu
memeriksa tiga hal: kunjungan ada, milik pasien yang sama, dan belum berakhir. Sesi HD juga
membolehkan `EncounterId` kosong saat dijadwalkan, tetapi mewajibkannya sebelum sesi dimulai.
Ini cocok dengan RHB-INV-04.

**Contoh.** Pasien RJ dengan 8 sesi fisioterapi datang 10 November. Petugas pendaftaran RJ
mendaftarkan kunjungan ke "Poli Rehabilitasi Medik" sehingga terbentuk encounter
`RJ-20261110-0042`. Saat terapis memulai sesi ke-3, sesi itu ditautkan ke encounter tersebut.
Bila terapis memilih encounter 3 November yang sudah selesai, sistem menolak dengan pesan
"kunjungan sudah berakhir".

**Gap yang tersisa:**

- Klinik bertipe `Rehabilitation` **tidak ada di seeder mana pun**. Keberadaannya di basis data
  belum diketahui (`UNKNOWN`, lihat C-07).
- `MstClinic.IsDoctorRequired` sudah ada, sehingga poli rehab dapat dikonfigurasi agar kunjungan
  terapi tidak menunggu dokter. Nilai yang benar adalah keputusan konfigurasi, bukan kode.

---

## 3. Peta Kemampuan

Status memakai satu nilai dari kontrak bukti: `READY TO REUSE`, `REUSE WITH ADAPTER`, `EXTEND`,
`REPAIR`, `MISSING`, `CONFLICT`, `UNKNOWN`.

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| C-01 | Pasien (pilih, bukan buat) | PatientManagement | `BE/Areas/HealthServices/PatientManagement/MasterData/Models#MstPatient@6bb3fba7` | READY TO REUSE | Rehab hanya menyimpan `PatientId` | Rendah |
| C-02 | Kunjungan/encounter RJ, RWI, IGD | RegistrationManagement | `BE/Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs#RegPatientEncounter@6bb3fba7` | REUSE WITH ADAPTER | Validasi kepemilikan dan status mengikuti `HmdServiceSupport.CheckEncounterAsync`. Rehab butuh pembaca/validator sendiri, tanpa menyalin domain HD. | Sedang: duplikasi validator antar modul |
| C-03 | Daftar pilihan encounter untuk form permintaan | RegistrationManagement | `BE/.../RegistrationManagement/Controllers/PatientEncounterController.cs#GetEncounterOptionsForAdmin@6bb3fba7` | REUSE WITH ADAPTER | Endpoint ini diizinkan `PatientEncounter:Read`. Petugas rehab harus diberi izin itu, **atau** rehab menyediakan lookup sendiri di bawah izin rehab. Keputusan ada di RHB-OQ-008. | Sedang: IDOR bila lookup tidak dibatasi pasien |
| C-04 | Rawat inap aktif dan status pulang | InPatientManagement | `BE/.../InPatientManagement/Models#InpEpisode.EncounterId,PhysicallyLeftAt@6bb3fba7` | READY TO REUSE | Baca saja | Rendah |
| C-05 | Rujukan internal antar-poli (dokter RJ → poli rehab) sebagai dasar permintaan | ClinicalManagement / Registrasi | `BE/.../ClinicalManagement/Services/DoctorCertificateService.cs#ValidateInternalReferralAsync@6bb3fba7`; `PatientEncounterController#GetInternalReferralLetters` | REUSE WITH ADAPTER | Surat rujukan internal sudah bisa menargetkan poli mana pun, termasuk "Fisioterapi". Permintaan rehab dapat menautkan id surat sebagai bukti sumber. Rujukan eksternal memakai `RegPatientEncounter.IsReferral`/`ReferralInstitutionId`. | Rendah |
| C-06 | Master poli dengan tipe Rehabilitasi | MasterData | `BE/Areas/HealthServices/MasterData/Enums/ClinicType.cs#Rehabilitation=6@6bb3fba7`; `ClinicController` filter `clinicType` | READY TO REUSE | Enum dan filter sudah ada, tetapi nilai 6 tidak pernah dirujuk di kode lain | Rendah |
| C-07 | Data poli rehab di basis data | MasterData (data) | Tidak ada seeder yang memakai `ClinicType.Rehabilitation` | UNKNOWN | Perlu dicek di DB dev/prod | Sedang: tanpa poli rehab, RJ tidak bisa mendaftar kunjungan sesi |
| C-08 | Identitas dokter (Sp.KFR) | HR Workforce | `BE/Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs#SpecialistName@6bb3fba7` | READY TO REUSE (tampilan) | `SpecialistName` berupa teks bebas, sehingga **tidak boleh** dipakai sebagai penentu kewenangan. Kewenangan tetap lewat Akses Role. | Sedang bila disalahgunakan sebagai gate |
| C-09 | Profesi terapis (fisio/okupasi/wicara) | HR Competency | `BE/Areas/Corporate/HumanResource/MasterData/CompetencyAndCredential/Models/MstProfession.cs@6bb3fba7` (`IsClinicalProfession`, dipakai `InpatientClinicalContextService`) | REUSE WITH ADAPTER | Disiplin rehab dipetakan ke `MstProfession.Id` lewat konfigurasi (RHB-DEC-004). **Jangan** meniru pencocokan teks `MapCpptProfessionType` (hanya mengenal Physiotherapist; okupasi dan wicara jatuh ke "Other"). | Sedang |
| C-10 | Data profesi okupasi dan wicara di DB | HR (data) | Enum label `EmployeeController.cs:3406` hanya memuat Fisioterapis | UNKNOWN | Perlu dicek apakah `MstProfession` untuk OT/TW sudah ada | Sedang |
| C-11 | Finalisasi, tanda tangan, kunci dokumen | MedicalRecordManagement | `BE/.../MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs#RegisterAsync,SignAsync,EnsureMutableAsync,JenisYangDitegakkan@6bb3fba7` | REUSE WITH ADAPTER | Lihat §2 RHB-OQ-003 | **Tinggi** bila jenis rehab tidak masuk `JenisYangDitegakkan` |
| C-12 | Addendum dan penulis pengganti | MedicalRecordManagement | `BE/.../Models/MrcClinicalNoteAddendum.cs`; `Controllers/ClinicalNoteAddendumController.cs` (`ClinicalNoteAddendum:Create`, `CreateAsSubstitute`) | READY TO REUSE | Endpoint generik per `documentKind`. Rehab cukup mendaftarkan jenis dokumennya. | Rendah |
| C-13 | Berkas rekam medis membaca dokumen rehab | MedicalRecordManagement | `BE/.../Services/Sources/IMedicalRecordDocumentSource.cs`; `Program.cs:691-720` | REUSE WITH ADAPTER | Rehab membuat `RhbMedicalRecordDocumentSource`, yang wajib mengembalikan `null` untuk dokumen pasien lain (anti-IDOR) | Sedang |
| C-14 | Kelengkapan rekam medis | MedicalRecordManagement | `BE/.../Services/Sources/IMedicalRecordCompletenessProbe.cs` | UNKNOWN | Belum ditelusuri apakah wajib untuk rilis 1 | Rendah |
| C-15 | Instrumen asesmen berversi + pengesahan | ClinicalManagement (`Cli`) | `BE/.../ClinicalManagement/Controllers/ClinicalInstrumentController.cs` (Read/Update/Approve); `Models/CliClinicalInstrument*.cs`; `Services/ClinicalInstrumentService.cs#ResolveForPatientAsync` | EXTEND | (a) `ClinicalInstrumentKind` hanya memuat 7 jenis keperawatan, sehingga perlu jenis baru untuk instrumen rehab. (b) Resolusi saat ini satu instrumen per *jenis* per pasien (berbasis usia), sedangkan rehab butuh beberapa instrumen per disiplin, sehingga perlu resolusi berdasarkan kode/daftar. (c) Mesin skor hanya `sum`, padahal COPM butuh rata-rata dan MMT/ROM berupa nilai per sendi. (d) Konsep hanya boleh dipakai bila `IsDraftAllowedInThisEnvironment`. | **Tinggi**: mengubah modul milik `Cli` yang dipakai keperawatan RWI |
| C-16 | Penomoran permintaan/episode/sesi | Platform NumberSeries (`Num`) | `BE/Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs#AllocateAsync@6bb3fba7` (preseden deret `HMD_SESSION`) | READY TO REUSE | Butuh definisi deret baru (data), bukan kode | Rendah |
| C-17 | Hak akses (Akses Role) | Platform | `[AccessController]`/`[AccessAction]`/`[AccessPermission]`; preseden konstanta `HemodialysisManagement/Constants/HemodialysisPermissions.cs` | READY TO REUSE | Rehab membuat konstanta modul sendiri | Rendah |
| C-18 | Pola agregat permintaan → episode → resep/program → sesi | Hemodialisa (preseden, bukan reuse data) | `BE/.../HemodialysisManagement/Models/HmdOrder.cs, HmdEpisode.cs, HmdPrescription.cs, HmdSession.cs@6bb3fba7` | READY TO REUSE (sebagai pola) | Ditiru polanya, bukan entity-nya | Rendah |
| C-19 | Entity domain rehab (permintaan, episode, asesmen, program, sesi, reasesmen, ringkasan) | Rehab (`Rhb`) | Tidak ada folder `Areas/HealthServices/RehabilitationManagement`; registry baris 111 | MISSING | Butuh baris registry (RHB-DEC-012) dan izin migration (RHB-OQ-006) | Blocker implementasi |
| C-20 | CPPT profesi fisioterapi di rawat inap | ClinicalManagement | `BE/.../ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs:2214`; `Enums/CpptNoteKind.cs#OtherProfessionNote` | CONFLICT | Fisioterapis rawat inap sudah bisa menulis CPPT naratif. Bila catatan sesi rehab juga ditulis, ada risiko dua catatan untuk satu tindakan. Perlu diputuskan apakah sesi rehab RWI menggantikan, merujuk, atau berdampingan dengan CPPT (RHB-OQ-007). | Sedang |
| C-21 | Billing (Phase 3) | BillingManagement | `BE/.../BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs`; preseden `HmdBillingHandoffService` | UNKNOWN | Tidak diaudit rinci, karena di luar rilis 1 | — |
| C-22 | Klien SATUSEHAT (Phase 3) | — | Tidak ada klien SATUSEHAT di source (hanya satu DTO event Operasi menyebutnya) | MISSING | Di luar rilis 1 | — |
| F-01 | Menu Rehab Medis di sidebar | FE | `FE/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:17#menuRehabMedis@0c06c093` | EXTEND | Key penanganan sudah ada, tetapi **tidak ada** entri di `src/utils/menu-sidebar/menu-items.jsx`. Polanya mengikuti blok Hemodialisa (`permission: "<Resource>:Read"`). | Rendah |
| F-02 | Halaman dan route rehab | FE | Tidak ada `src/app/health-services/*rehab*` | MISSING | — | — |
| F-03 | Base components | FE | `FE/src/components/features/base-features/*` (hero, data-filter, data-table, base-detail-view, base-grouped-editor-*, status-badge, confirm-modal, toast-stack, access-denied-gate, pagination, row-action-menu, base-patient-card) | READY TO REUSE | Props diverifikasi saat task FE | Rendah |
| F-04 | Service FE addendum | FE | `FE/src/lib/services/health-services/medical-record-management/clinical-note-addendum.service.js` | READY TO REUSE | — | Rendah |
| F-05 | UI addendum (buat dan riwayat) | FE | `FE/src/components/view/health-services/inpatient-management/doctor-inpatient/my-notes/addendum-*-modal.jsx`; salinan serupa di IGD; `medical-record-management/medical-records/medical-record-addendum-actions.jsx` | REUSE WITH ADAPTER | Sudah ada dua salinan modal per modul. Rehab sebaiknya memakai salah satunya, bukan membuat salinan ketiga. Kandidat diangkat ke komponen bersama, dengan keputusan owner FE. | Sedang: duplikasi |
| F-06 | Layar konfigurasi instrumen klinis | FE | `FE/src/app/health-services/clinical-management/clinical-instruments/page.jsx`; `clinical-instrument.service.js` | READY TO REUSE | Komite memasukkan dan mengesahkan instrumen rehab di layar ini | Rendah |
| F-07 | Service FE encounter/registrasi | FE | `FE/src/lib/services/health-services/registration-management/outpatient-encounter.service.js` | REUSE WITH ADAPTER | Bergantung keputusan C-03 | Sedang |
| F-08 | Pola layanan FE per modul (axios, unwrap, normalisasi error) | FE | `FE/src/lib/services/health-services/hemodialysis-management/hmdOrderService.js`, `hmdServiceHelper.js` | READY TO REUSE (pola) | — | Rendah |
| F-09 | Tab Rehab Medik di workspace dokter rawat inap | FE | `FE/src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-unavailable-panel.jsx` (RWI-DEC-108), `supporting-landing-grid.jsx` (`rehabOrders`, `rehabState`) | EXTEND (nanti) | Panel "belum tersedia" menunggu modul rehab. Pengisiannya milik slice integrasi (RHB-DEC-008), bukan rilis 1. | Rendah |

---

## 4. Kontrak As-Is yang Relevan (gaya Swagger)

**Grup: MedicalRecordManagement — ClinicalNoteAddendum** (existing)

| Method | Path | Izin | Fungsi |
|---|---|---|---|
| GET | `/api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | `ClinicalNoteAddendum:Read` | Daftar addendum satu dokumen |
| GET | `/api/v1/health-services/medical-record-management/clinical-note-addendums/authority/{documentKind}/{documentId}` | `ClinicalNoteAddendum:Read` | Apakah pengguna boleh membuat addendum |
| POST | `/api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}` | `ClinicalNoteAddendum:Create` | Buat addendum |
| POST | `/api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/{documentKind}/{documentId}/as-substitute` | `ClinicalNoteAddendum:CreateAsSubstitute` | Addendum oleh penulis pengganti |

**Grup: ClinicalManagement — ClinicalInstrument** (existing)

| Method | Path | Izin | Fungsi |
|---|---|---|---|
| GET | `/api/v1/health-services/clinical-management/clinical-instruments` | `ClinicalInstrumentConfiguration:Read` | Daftar instrumen |
| POST | `/api/v1/health-services/clinical-management/clinical-instruments` | `ClinicalInstrumentConfiguration:Update` | Buat instrumen |
| POST | `/api/v1/health-services/clinical-management/clinical-instruments/{id}/versions` | `ClinicalInstrumentConfiguration:Update` | Buat versi konsep |
| PATCH | `/api/v1/health-services/clinical-management/clinical-instruments/versions/{versionId}/approve` | `ClinicalInstrumentConfiguration:Approve` | Sahkan versi (pengesah ≠ pengubah terakhir) |
| PATCH | `/api/v1/health-services/clinical-management/clinical-instruments/versions/{versionId}/retire` | `ClinicalInstrumentConfiguration:Approve` | Pensiunkan versi |

**Grup: RegistrationManagement — PatientEncounter** (existing)

| Method | Path | Izin | Fungsi |
|---|---|---|---|
| GET | `/api/v1/health-services/registration-management/patient-encounters/admin/options` | `PatientEncounter:Read` | Pilihan encounter (`patientId`, `encounterType`, `encounterStatus`, `onlyActive`) |
| GET | `/api/v1/health-services/registration-management/patient-encounters/internal-referral-letters?patientId=&clinicId=` | `PatientEncounter:Read` | Surat rujukan internal yang belum dipakai |

Tidak ada endpoint rehab existing.

---

## 5. Conflict dan Unknown

| ID | Jenis | Uraian | Siapa yang memutuskan |
|---|---|---|---|
| C-11/C-12 | Ketergantungan lintas modul | Jenis dokumen rehab harus ditambahkan ke enum dan `JenisYangDitegakkan` milik Rekam Medis | Pemilik Rekam Medis (Yoga Aji Pratama) |
| C-15 | Ketergantungan lintas modul | Perluasan mesin instrumen (`ClinicalInstrumentKind` baru, resolusi per kode, metode skor) mengubah modul `Cli` yang dipakai keperawatan | Pemilik ClinicalManagement / instrumen keperawatan (belum diketahui namanya) |
| C-20 | Conflict | Catatan sesi rehab RWI berpotensi dobel dengan CPPT fisioterapi | Owner Rehab + pemilik CPPT/Rawat Inap |
| C-03 | Keputusan | Izin `PatientEncounter:Read` untuk petugas rehab, atau lookup milik rehab | Owner Rehab |
| C-07, C-10 | Unknown (data) | Keberadaan poli `ClinicType.Rehabilitation` dan profesi okupasi/wicara di basis data | Admin data / owner Rehab |
| C-14 | Unknown | Apakah probe kelengkapan RM wajib untuk rilis 1 | Owner Rehab + Rekam Medis |

---

## 6. Pertanyaan Penutup (untuk closure pass `grill-me`)

| ID | Pertanyaan |
|---|---|
| RHB-OQ-007 | Untuk pasien rawat inap: apakah catatan sesi rehab **menggantikan** CPPT fisioterapi, **merujuk** ke CPPT (CPPT tetap ditulis), atau **berdampingan** (keduanya ditulis bebas)? |
| RHB-OQ-008 | Pilihan encounter pada form permintaan: beri petugas rehab izin `PatientEncounter:Read`, atau rehab menyediakan lookup sendiri yang dibatasi pasien terpilih? |
| RHB-OQ-009 | Siapa pemilik ClinicalManagement/instrumen yang menyetujui perluasan mesin instrumen (C-15)? Apakah rilis 1 boleh berjalan dengan instrumen yang **cukup** memakai tipe isian dan skor `sum` yang ada (NRS, Barthel, BBS, TUG, Lawton, GUSS, FOIS), sementara COPM, MMT/ROM per sendi, dan TADIR menyusul? |
| RHB-OQ-010 | Apakah pemilik Rekam Medis sudah bersedia menambah jenis dokumen rehab ke `JenisYangDitegakkan`? Siapa yang meminta persetujuannya? |
| RHB-OQ-011 | Apakah poli Rehabilitasi Medik (`ClinicType = Rehabilitation`) dan profesi Terapis Okupasi/Terapis Wicara sudah ada di master data DB, atau perlu dibuat oleh admin data? |
