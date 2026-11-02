# Rekam Medis — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `3` — **impact scan 28 Oktober 2026** untuk amandemen sambungan seluruh modul (bagian terakhir dokumen ini). Revision 2 di bawahnya dibiarkan utuh sebagai riwayat dan **basi** terhadap SHA terbaru |
| Mode | Revision 2: audit penuh. Revision 3: impact scan terbatas pada cakupan `RM-DEC-037`–`050` |
| Revision 3 — SHA | Backend `363de34f` (branch `yoga`); frontend `f0a3b07b8` (branch `YogaV2`) |
| Revision 3 — input keputusan | `00-interview-decisions.md` revision `7`, `sha256:59e46deb…`, seluruh `RM-DEC-037`–`050` `approved` |
| Backend root/SHA | `NewQuilvianSystemBackend` / `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| Frontend root/SHA | `QuilvianSystemFrontendDev` / `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |
| Tanggal audit | 24 Agustus 2026 |
| Input keputusan | `00-interview-decisions.md` revision `1`, seluruh keputusan berstatus `draft` |
| Sifat audit | **Read-only.** Tidak ada source aplikasi yang diubah selama audit ini |

## Batas audit

Hal-hal berikut **tidak** dapat dibuktikan pada sesi ini. Setiap kesimpulan yang bergantung
padanya ditandai `Unknown`, bukan diasumsikan benar.

| No | Batas | Akibatnya |
|---:|---|---|
| 1 | Database tidak diperiksa | Tidak dapat dipastikan 162 berkas migration sudah benar-benar diterapkan ke database berjalan. Tersedianya berkas migration **bukan** bukti skema sudah terbentuk |
| 2 | Runtime tidak dijalankan | Tidak dapat dipastikan seeder berjalan tanpa galat, dan tidak dapat mengukur perilaku nyata endpoint |
| 3 | Data produksi tidak dilihat | Tidak dapat menilai kualitas data yang sudah ada, misalnya berapa banyak catatan yang tertinggal berstatus `Draft` |
| 4 | Layanan eksternal tidak dihubungi | Tidak ada integrasi eksternal yang relevan ditemukan untuk modul ini, jadi batas ini tidak berdampak |

Notasi bukti yang dipakai sepanjang dokumen:

- `BE` berarti repository `NewQuilvianSystemBackend` pada commit `ab37e3a`.
- `FE` berarti repository `QuilvianSystemFrontendDev` pada commit `c4e2ef2a6`.

Contoh: `BE Filters/AccessPermissionFilter.cs:45` berarti berkas
`Filters/AccessPermissionFilter.cs` baris 45 pada backend commit `ab37e3a`.

---

## Journey yang Ditelusuri

Alur nyata yang disisir dari ujung ke ujung, mengikuti perjalanan satu pasien rawat jalan:

```text
pendaftaran/kiosk -> pasien -> kunjungan (encounter) -> antrean -> asesmen perawat
  -> konsultasi dokter (SOAP) -> diagnosis -> tindakan -> CPPT -> resep
  -> dokumen & surat -> penutupan kunjungan -> [rekam medis: penelusuran, keutuhan, jejak akses]
```

Bagian dalam tanda kurung siku adalah bagian yang menjadi sasaran modul Rekam Medis. Sisanya
sudah berjalan dan diaudit sebagai sumber data.

Tiga kebutuhan rilis pertama pada `RM-DEC-002` diterjemahkan menjadi pertanyaan audit berikut:

1. **Penelusuran lintas kunjungan** — apakah data klinis sudah dapat diambil per pasien tanpa
   harus membuka kunjungan satu per satu?
2. **Keutuhan dokumen** — apakah sudah ada mekanisme yang mencegah catatan final diubah, dan
   apakah perubahan meninggalkan jejak?
3. **Jejak akses** — apakah sudah ada catatan permanen tentang siapa membuka rekam medis siapa?

---

## Capability Register

Status memakai tujuh nilai baku: `Ready to reuse`, `Reuse with adapter`, `Extend`, `Repair`,
`Missing`, `Conflict`, `Unknown`.

### Kelompok A — Penelusuran berkas lintas kunjungan

| ID | Kebutuhan | Owner | Existing evidence | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `RM-CAP-001` | Data klinis tersimpan per pasien | `ClinicalManagement` | `BE Areas/HealthServices/ClinicalManagement/Models/` — 13 model transaksi; 13 dari 15 controller menerima filter `patientId` | `Ready to reuse` | Tidak ada | Rendah |
| `RM-CAP-002` | Query riwayat per pasien berjalan cepat | `ClinicalManagement` | Index gabungan `(PatientId, <tanggal>, IsDelete)` terbukti pada `BE Repositories/Configurations/HealthServices/TrxPatientIntegratedProgressNoteConfiguration.cs:169-174`, dan pola sama pada `TrxPatientDiagnosisConfiguration.cs:238`, `TrxDoctorConsultationConfiguration.cs:312`, `TrxPatientVitalSignConfiguration.cs:166`, `TrxPatientClinicalDocumentConfiguration.cs:183` | `Ready to reuse` | Tidak ada | Rendah |
| `RM-CAP-003` | Timeline CPPT lintas kunjungan lengkap dengan pemakainya | `ClinicalManagement` | API `BE Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs:165` (`GET .../timeline`), dipakai `FE src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js:38-50` dan `FE src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js:229-232` yang mengirim `patientId` | `Ready to reuse` | Tidak ada | Rendah |
| `RM-CAP-004` | Pengambilan gabungan seluruh jenis dokumen dalam satu permintaan | Rekam Medis (baru) | Belum ada. Yang ada 13 endpoint terpisah, masing-masing dengan penomoran halaman sendiri | `Reuse with adapter` | Perlu satu lapisan penggabung di atas 13 sumber yang sudah ada. Tidak perlu tabel baru | Sedang — jumlah query bisa membengkak bila tidak dibatasi |
| `RM-CAP-005` | Pola lapisan penggabung yang sudah terbukti | `PharmacyManagement` | `BE Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkspaceService.cs` (1.201 baris) dan `Controllers/PrescriptionWorkspaceController.cs:39,61`, terdaftar di `BE Program.cs:273` | `Ready to reuse` | Dipakai sebagai contoh pola, bukan kode yang disalin | Rendah |
| `RM-CAP-006` | Nomor rekam medis unik | `PatientManagement` | `BE Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs:22`; keunikan ditegakkan di `BE Migrations/ApplicationDbContextModelSnapshot.cs:63994` (`HasIndex("MedicalRecordNumber").IsUnique()`) | `Ready to reuse` | Tidak ada | Rendah — bergantung batas audit nomor 1 |
| `RM-CAP-007` | Penggabungan pasien duplikat | `PatientManagement` | Lihat penelusuran terarah pada bagian "Penelusuran lanjutan `RM-CAP-007`" di bawah | **`Conflict`** (sebelumnya `Unknown`, ditelusuri 24 Agustus 2026) | Penggabungan hanya berupa **penandaan**, tidak memindahkan data klinis apa pun. Tidak ada modul lain yang mengikuti `MergedToPatientId` | **Tinggi** — riwayat pasien hasil penggabungan pasti tampil terpecah, karena memang tidak ada yang menyatukannya |
| `RM-CAP-033` | Penggabungan pasien dapat dijalankan dari antarmuka | `PatientManagement` | `FE src/lib/constants/health-services/patient-management/master-data/patient-constants.jsx:120` menyediakan pilihan "Digabung ke Pasien", tetapi `FE .../patient-editor-utils.jsx:157` **tidak pernah mengirim** `mergeReason`, padahal `BE .../PatientController.cs:2380` mewajibkannya | **`Repair`** | Fitur penggabungan di layar pasien **selalu gagal** dengan galat 400 | Sedang — fitur tampak tersedia padahal tidak dapat dipakai |
| `RM-CAP-008` | Rute dan menu rekam medis di frontend | Frontend | Tidak ada. `FE src/utils/menu-sidebar/menu-items.jsx` (897 baris) tidak memuat entri rekam medis; tidak ada folder `src/app/health-services/medical-record` | `Missing` | Perlu rute, menu, service, hook, dan halaman baru | Rendah |

**Catatan penting tentang `RM-CAP-008`.** Kunci `menuLaboratorium`, `menuRadiologi`,
`menuMCU`, dan `menuOptik` memang muncul di
`FE src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:6-19`, tetapi itu hanya
daftar nama kelompok menu bersarang (`NESTED_MENU_KEYS`), bukan definisi menu. Tidak ada satu
pun entri menu, rute, atau halaman yang benar-benar dapat dicapai. Jangan menyimpulkan modul
tersebut sudah ada hanya karena namanya tertulis.

### Kelompok B — Keutuhan dokumen

| ID | Kebutuhan | Owner | Existing evidence | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `RM-CAP-009` | Model status yang seragam untuk semua jenis dokumen | `ClinicalManagement` | Empat bentuk status berbeda ditemukan: `DoctorConsultationStatus` (`Draft/InProgress/Completed/Cancelled`), `PatientAssessmentStatus` (sama), `PatientClinicalDocumentStatus` (`Draft/Uploaded/Verified/Approved/Rejected/Archived/Cancelled/EnteredInError`), `ClinicalNoteAttachmentStatus` (tujuh nilai, beda urutan). CPPT tidak punya enum status sama sekali | `Conflict` | Perlu keputusan: satu model status baru yang berlaku umum, atau pemetaan dari empat model yang sudah ada | Tinggi — tanpa keseragaman, aturan penguncian harus ditulis ulang per jenis dokumen |
| `RM-CAP-010` | Catatan final tidak dapat diubah | `ClinicalManagement` | Sudah ada, tetapi hanya untuk konsultasi dan tersebar per controller: `BE .../DoctorConsultationController.cs:414,514,654` menolak perubahan bila status `Completed`; pola sama di `PatientDiagnosisController.cs:834`, `PatientProcedureController.cs:623,1224,1280,1448` | `Extend` | Aturan ada tetapi tidak terpusat dan tidak mencakup CPPT. Perlu satu tempat penegakan, bukan tujuh pemeriksaan terpisah | Tinggi — aturan tersebar mudah terlewat saat controller baru ditambahkan |
| `RM-CAP-011` | CPPT tidak dapat diubah setelah final | `ClinicalManagement` | Satu-satunya penghalang perubahan CPPT adalah `IsCancel` dan `IsReadOnlyGenerated` di `BE .../PatientIntegratedProgressNoteController.cs:511-524`. Tidak ada pemeriksaan status maupun tanda tangan | `Repair` | CPPT yang ditulis dokter dapat diubah tanpa batas waktu oleh siapa pun yang punya izin controller | **Kritis** — inti keputusan `RM-DEC-003` justru belum terlindungi pada dokumen yang paling sering dipakai |
| `RM-CAP-012` | Kepemilikan penulis catatan yang stabil | `ClinicalManagement` | `BE .../PatientIntegratedProgressNoteController.cs:533` menetapkan `entity.ProviderUserId` dari isi permintaan pada operasi ubah. Tidak ada pemeriksaan bahwa pengubah adalah penulisnya | `Repair` | Penulis sebuah catatan klinis dapat dipindahkan ke orang lain lewat permintaan ubah biasa | **Kritis** — `RM-DEC-004` mensyaratkan addendum hanya oleh penulis asli, sementara identitas penulis sendiri belum terjamin |
| `RM-CAP-013` | Penghalang read-only tidak bisa dilepas dari luar | `ClinicalManagement` | `BE .../PatientIntegratedProgressNoteController.cs:519-524` memakai `IsReadOnlyGenerated` sebagai penghalang, tetapi baris `:550` menetapkan ulang nilainya dari isi permintaan | `Repair` | Penanda yang seharusnya melindungi justru dapat diubah oleh pengirim permintaan | Tinggi |
| `RM-CAP-014` | Tanda tangan elektronik penulis | — | Tidak ada. Penelusuran `SignedAt`, `IsSigned`, `IsLocked`, `Amendment`, `Addendum`, `IsFinalized` di seluruh `BE Areas/HealthServices/ClinicalManagement` hanya menemukan `SignedAt` pada `BE .../Models/TrxPatientConsent.cs:185` | `Missing` | Perlu kolom, endpoint, dan aturan baru | Sedang |
| `RM-CAP-015` | Pengesahan ulang identitas saat menandatangani | `Administrator`/Auth | `BE Models/ApplicationUserFingerprintCredential.cs` ada, dan `BE Areas/SelfServices/Biometric/Controllers/FingerprintController.cs` berjalan | `Unknown` | Kemampuan sidik jari tersedia, tetapi apakah dipakai sebagai pengesahan tanda tangan adalah keputusan manusia (`RM-DEC-011`), bukan temuan source | Rendah |
| `RM-CAP-016` | Addendum sebagai koreksi yang tidak menimpa | — | Tidak ada tabel, kolom, maupun endpoint | `Missing` | Perlu entity baru yang menempel pada catatan induk | Sedang |
| `RM-CAP-017` | Penyimpanan nilai lama saat data diubah | — | `BE Models/IdentityModel.cs:1-24` hanya menyimpan `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, penanda batal, dan penanda hapus. Tidak ada tabel riwayat versi di seluruh 445 `DbSet` pada `BE Repositories/ApplicationDbContext.cs` | `Missing` | Perlu mekanisme penyimpanan versi untuk dokumen klinis | Tinggi — tanpa ini, klaim "koreksi meninggalkan jejak" tidak dapat dibuktikan untuk data yang sudah ada |
| `RM-CAP-018` | Penguncian otomatis saat kunjungan ditutup | `RegistrationManagement` | Penutupan kunjungan ada: `BE .../PatientEncounterController.cs:858` mengubah status, dan `:878` menolak perubahan bila `CompletedAt` sudah terisi | `Extend` | Titik sambungnya tersedia, tetapi penutupan kunjungan **tidak** merambat mengunci catatan klinis di dalamnya | Sedang |
| `RM-CAP-019` | Validasi perpindahan status kunjungan | `RegistrationManagement` | `BE .../PatientEncounterController.cs:866-884` hanya memeriksa nilai enum sah dan kunjungan belum batal/selesai. Tidak ada tabel atau fungsi transisi yang diizinkan | `Repair` | Status kunjungan dapat melompat dari nilai mana pun ke nilai mana pun, termasuk langsung ke `Completed` | Sedang — lapis kedua `RM-DEC-003` bergantung pada penutupan kunjungan yang tertib |
| `RM-CAP-020` | Keutuhan berkas lampiran | `ClinicalManagement` | `BE .../Models/TrxPatientClinicalDocument.cs:104-122` menyimpan `FilePath`, `FileName`, `FileSizeBytes`, dan `FileHash`. Direktori `BE Storage/uploads/` tersedia | `Ready to reuse` | Tidak ada | Rendah |
| `RM-CAP-021` | Penanda dokumen salah catat | `ClinicalManagement` | Nilai `EnteredInError` sudah ada pada `PatientClinicalDocumentStatus` dan `ClinicalNoteAttachmentStatus` | `Reuse with adapter` | Konsepnya sudah benar, tetapi hanya berlaku untuk dokumen dan lampiran. Catatan naratif seperti CPPT dan SOAP tidak memilikinya | Rendah |

### Kelompok C — Jejak akses dan kerahasiaan

| ID | Kebutuhan | Owner | Existing evidence | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `RM-CAP-022` | Catatan permanen siapa membuka rekam medis siapa | — | Tidak ada tabel jejak akses di antara 445 `DbSet` pada `BE Repositories/ApplicationDbContext.cs`. `BE Filters/AccessPermissionFilter.cs:56-66` hanya menulis log ketika akses **ditolak**; akses yang berhasil tidak dicatat sama sekali | `Missing` | Perlu tabel jejak akses dan titik pencatatan pada setiap pembacaan rekam medis | **Kritis** — inti kebutuhan ketiga `RM-DEC-002` |
| `RM-CAP-023` | Fungsi bantu pencatatan audit | `Services/Logging` | `BE Services/Logging/LoggerService.cs:34-37` menyediakan `AuditAsync`. Penelusuran seluruh source menemukan **nol** pemanggil di luar berkas definisinya | `Repair` | Fungsi tersedia tetapi tidak pernah dipakai, dan tujuannya adalah log teks/Grafana Loki (`:152-161`), bukan tabel database | Tinggi — log teks dapat berotasi dan hilang, sehingga tidak layak sebagai bukti hukum jangka panjang |
| `RM-CAP-024` | Pembatasan akses per pasien | Security | Tidak ada. Kewenangan bertumpu pada `BE Models/SysAccessPolicy.cs` yang memetakan `DepartmentId` + `PositionId` ke `ControllerAccessId` + `ActionAccessId`. Tidak ada dimensi pasien, unit perawatan, maupun DPJP | `Missing` | Perlu lapisan kewenangan tingkat sumber daya di atas kewenangan fungsi yang sudah ada | **Kritis** — `RM-DEC-005` membedakan pasien rawatan dari bukan rawatan, dan pembeda itu belum ada fondasinya |
| `RM-CAP-025` | Kewenangan SuperAdmin | Security | `BE Services/Security/AccessPermissionService.cs:54-56` mengembalikan `true` untuk seluruh permintaan bila pengguna ber-role `SuperAdmin`, sebelum kebijakan apa pun diperiksa | `Conflict` | Pemilik peran teknis dapat membaca seluruh rekam medis tanpa batas, dan karena `RM-CAP-022` belum ada, tanpa jejak pula | **Kritis** — masalah yang sama sudah tercatat pada blueprint IGD sebagai syarat go-live |
| `RM-CAP-026` | Tingkat kerahasiaan dokumen | `ClinicalManagement` | `PatientClinicalDocumentConfidentialityLevel` dan `ClinicalNoteAttachmentConfidentialityLevel` menyediakan `Normal/Restricted/Confidential/VeryConfidential`. Namun seluruh pemakaiannya hanya sebagai penyaring daftar dan nilai tampilan, contohnya `BE .../ClinicalNoteAttachmentController.cs:791`, `:1130`. Tidak ada satu pun tempat yang menolak akses berdasarkan nilai ini | `Conflict` | Kolom yang tampak melindungi padahal tidak menegakkan apa pun | Tinggi — pengguna dan auditor dapat salah menyimpulkan bahwa dokumen sudah terlindungi |
| `RM-CAP-027` | Kerahasiaan catatan pribadi tenaga klinis | `ClinicalManagement` | Kolom `PrivateNote` ada pada `BE .../Models/TrxPatientIntegratedProgressNote.cs` dan dapat diisi serta diubah lewat `.../PatientIntegratedProgressNoteController.cs:547` | `Unknown` | Aturan siapa boleh membaca belum pernah diputuskan (`RM-DEC-012`). Ini keputusan manusia, bukan temuan source | Sedang |

### Kelompok D — Fondasi lintas kebutuhan

| ID | Kebutuhan | Owner | Existing evidence | Status | Gap/adapter | Risk |
|---|---|---|---|---|---|---|
| `RM-CAP-028` | Pendaftaran izin otomatis untuk controller baru | `Administrator` | `BE Attributes/AccessControllerAttribute.cs` dan `AccessActionAttribute.cs` dibaca oleh `BE Seeders/AccessMenuSeeder.cs`, yang dijalankan saat aplikasi mulai lewat `BE Program.cs:788` | `Ready to reuse` | Tidak ada. Controller rekam medis baru akan otomatis muncul di pengaturan hak akses | Rendah |
| `RM-CAP-029` | Pola pendaftaran service | — | `BE Program.cs` memuat 104 pemanggilan `AddScoped`, termasuk `LoggerService` (`:261`), `AccessPermissionService` (`:264`), `PrescriptionWorkspaceService` (`:273`), `DoctorConsultationLifecycleService` (`:277`) | `Ready to reuse` | Pendaftaran manual, harus ditambahkan untuk service baru | Rendah |
| `RM-CAP-030` | Pola konfigurasi EF per entity | — | `BE Repositories/Configurations/HealthServices/` memuat konfigurasi terpisah per entity dengan index, keunikan, dan `DeleteBehavior.Restrict`, contohnya `TrxPatientIntegratedProgressNoteConfiguration.cs:154-220` | `Ready to reuse` | Tidak ada | Rendah |
| `RM-CAP-031` | Kode diagnosis ICD-10 | `MasterData` | `BE Seeders/Icd10DiagnosisSeeder.cs`; `BE .../Models/MstDiagnosis.cs:29,34` menyimpan `DiagnosisType` dan `IcdVersion` | `Ready to reuse` | Relevan untuk verifikasi koding pada rilis berikutnya, bukan rilis pertama | Rendah |
| `RM-CAP-032` | Uji otomatis sebagai bukti perilaku | — | Backend: tidak ditemukan project test apa pun. Frontend: hanya 4 berkas di `FE tests/` (`auth-security.spec.mjs`, `route-smoke.spec.mjs`, `auth-security.test.mjs`, `base-components-regression.test.mjs`), tidak satu pun menyentuh alur klinis | `Missing` | Tidak ada jaring pengaman otomatis untuk perubahan pada aturan penguncian dan kewenangan | Tinggi — perubahan pada `RM-CAP-010` sampai `RM-CAP-013` menyentuh alur yang sedang dipakai IGD dan antrean dokter |

---

## Backend Inventory

### Entity yang menjadi sumber isi rekam medis

Seluruhnya berada di `BE Areas/HealthServices/ClinicalManagement/Models/`.

| Entity | Nomor dokumen | Punya enum status? | Punya penghalang ubah? |
|---|---|---|---|
| `TrxPatientAssessment` | — | Ya, `PatientAssessmentStatus` | Belum diverifikasi rinci |
| `TrxDoctorConsultation` | `ConsultationNumber` | Ya, `DoctorConsultationStatus` | Ya, `Completed` ditolak |
| `TrxPatientIntegratedProgressNote` | `ProgressNoteNumber` (unik) | **Tidak** | Hanya `IsCancel` dan `IsReadOnlyGenerated` |
| `TrxPatientDiagnosis` | — | Ya, `PatientDiagnosisStatus` | Ya, lewat status konsultasi |
| `TrxPatientProcedure` | — | Ya, `PatientProcedureStatus` (5 nilai) | Ya, lewat status konsultasi |
| `TrxPatientVitalSign` | — | Ya, `PatientVitalSignStatus` | Belum diverifikasi rinci |
| `TrxPatientAllergy` | — | Ya, `PatientAllergyStatus` | Belum diverifikasi rinci |
| `TrxPatientMedicalHistory` | — | Ya, `PatientMedicalHistoryStatus` | Belum diverifikasi rinci |
| `TrxPatientFamilyHistory` | — | Ya, `PatientFamilyHistoryStatus` | Belum diverifikasi rinci |
| `TrxPatientClinicalDocument` | — | Ya, 8 nilai termasuk `EnteredInError` | Belum diverifikasi rinci |
| `TrxClinicalNoteAttachment` | — | Ya, 7 nilai termasuk `EnteredInError` | Belum diverifikasi rinci |
| `TrxMedicalCertificate` | — | Ya, `MedicalCertificateStatus` | Belum diverifikasi rinci |
| `TrxPatientConsent` | — | Ya, `PatientConsentStatus` | Punya `SignedAt` |

Kolom "belum diverifikasi rinci" berarti audit ini belum membuka setiap controller baris per
baris. Yang sudah dibuktikan adalah bahwa **pola penghalangnya tidak seragam**, dan CPPT —
dokumen yang paling sering ditulis — adalah yang paling lemah.

### Titik sambung untuk modul yang belum ada

`BE .../Models/TrxPatientIntegratedProgressNote.cs` menyediakan `SourceModule`,
`SourceReferenceId`, dan `SourceReferenceNumber`, dengan index gabungan di
`BE Repositories/Configurations/HealthServices/TrxPatientIntegratedProgressNoteConfiguration.cs:199-204`.
Artinya modul Laboratorium, Radiologi, dan MCU nanti dapat menitipkan entri ke CPPT tanpa
mengubah struktur tabel.

`BE .../Models/TrxDoctorConsultation.cs:134-142` menyediakan `SupportingOrderText`,
`SupportingOrderCount`, dan `HasSupportingOrder` sebagai ringkasan pesanan penunjang, dengan
komentar yang menyatakan detailnya akan berada di modul order. Modul tersebut belum ada.

### As-Is Contract — API yang sudah tersedia

Judul grup berikut dikutip persis dari nilai `[Tags(...)]` pada controller.

#### Health Services / Clinical Management / Patient Integrated Progress Note

Basis rute: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Izin |
|---|---|---|---|
| `GET` | `/filters/metadata` | Mengambil daftar pilihan penyaring | `PatientIntegratedProgressNote` / `Read` |
| `GET` | `/timeline` | **Riwayat CPPT per pasien atau per kunjungan**, urut waktu | `PatientIntegratedProgressNote` / `Read` |
| `GET` | `/{id}` | Detail satu CPPT | `PatientIntegratedProgressNote` / `Read` |
| `POST` | `/from-consultation/{consultationId}` | Membuat CPPT dari konsultasi | `PatientIntegratedProgressNote` / `Create` |
| `GET` | `/draft-from-consultation/{consultationId}` | Mengambil rancangan CPPT | `PatientIntegratedProgressNote` / `Read` |
| `PUT` | `/{id}` | Mengubah CPPT | `PatientIntegratedProgressNote` / `Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan CPPT | `PatientIntegratedProgressNote` / `Delete` |

Parameter `GET /timeline`: `patientId`, `encounterId`, `professionType`, `startDate`,
`endDate`, `includeCancelled` (default `false`), `limit` (default 100, batas atas 300).
Endpoint menolak permintaan bila `patientId` dan `encounterId` sama-sama kosong
(`BE .../PatientIntegratedProgressNoteController.cs:180-187`).

**Ini kontrak yang paling dekat dengan kebutuhan penelusuran lintas kunjungan, dan sudah
terbukti dipakai frontend.**

#### Grup lain yang menyimpan isi rekam medis

Seluruhnya berada di bawah `api/v1/health-services/clinical-management/` dan menerima
penyaring `patientId`:

| Grup `[Tags]` | Segmen rute |
|---|---|
| `Health Services / Clinical Management / Doctor Consultation` | `doctor-consultations` |
| `Health Services / Clinical Management / Patient Assessment` | `patient-assessments` |
| `Health Services / Clinical Management / Patient Diagnosis` | `patient-diagnoses` |
| `Health Services / Clinical Management / Patient Procedure` | `patient-procedures` |
| `Health Services / Clinical Management / Patient Vital Sign` | `patient-vital-signs` |
| `Health Services / Clinical Management / Patient Allergy` | `patient-allergies` |
| `Health Services / Clinical Management / Patient Medical History` | `patient-medical-histories` |
| `Health Services / Clinical Management / Patient Family History` | `patient-family-histories` |
| `Health Services / Clinical Management / Patient Clinical Document` | `patient-clinical-documents` |
| `Health Services / Clinical Management / Clinical Note Attachment` | `clinical-note-attachments` |
| `Health Services / Clinical Management / Medical Certificate` | `medical-certificates` |
| `Health Services / Clinical Management / Patient Consent` | `patient-consents` |

Seluruh nilai `[Tags(...)]` di atas dikutip persis dari source dan sudah diverifikasi. Total
ada 15 controller di `ClinicalManagement`; 13 di antaranya menerima penyaring `patientId`,
yaitu 12 grup pada tabel ini ditambah grup CPPT yang dirinci sebelumnya. Dua yang tidak
menerima `patientId` adalah `Diagnosis Recommendation Resolver` dan `Prescribing Drug`.

Perbedaan dari kebutuhan target: **tidak ada satu pun endpoint yang menggabungkan ketiga belas
sumber ini menjadi satu riwayat berurut waktu.** Untuk menampilkan satu halaman rekam medis,
frontend saat ini harus memanggil sampai tiga belas endpoint terpisah, masing-masing dengan
penomoran halaman sendiri, lalu mengurutkan hasilnya sendiri. Inilah gap `RM-CAP-004`.

---

## Frontend Inventory

| Aspek | Temuan | Bukti |
|---|---|---|
| Halaman rekam medis berdiri sendiri | Tidak ada | Tidak ada folder `FE src/app/health-services/medical-record` maupun sejenisnya |
| Menu rekam medis | Tidak ada | `FE src/utils/menu-sidebar/menu-items.jsx`, 897 baris, tanpa entri rekam medis |
| Tempat data klinis dapat dilihat sekarang | Hanya di dalam layar antrean dokter, sebagai tab | `FE src/components/view/health-services/registration-management/doctor-queues/tabs/` berisi `cppt/`, `soap/`, `procedure/`, `prescription/`, `certificate/` |
| Status tab CPPT | Aktif, bukan kode mati | `FE .../doctor-queues/doctor-queue-view.jsx:24` mengimpor `doctor-cppt-tab`, dan halaman `FE src/app/health-services/registration-management/doctor-queues/page.jsx` dapat dicapai |
| Pemanggilan API riwayat | Sudah mengirim `patientId`, bukan hanya `encounterId` | `FE src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js:229-232` |
| Penomoran halaman riwayat | Per bulan, dengan batas | `FE .../use-doctor-cppt.js:372` memakai `CPPT_TIMELINE_MONTH_LIMIT` |
| Service klinis yang tersedia | 6 berkas, 736 baris | `FE src/lib/services/health-services/clinical-management/` |
| Hook klinis yang tersedia | 3 berkas | `use-doctor-cppt.js`, `use-doctor-procedure.js`, `use-doctor-soap.js` |
| Penjaga izin di sisi frontend | Tidak ditemukan pola penjaga izin per halaman | Penelusuran `AccessGuard`, `hasAccess`, `usePermission`, `canAccess` tidak menemukan komponen penjaga yang dipakai untuk melindungi rute |
| Uji otomatis alur klinis | Tidak ada | `FE tests/` hanya 4 berkas, tidak ada yang klinis |

**Kesimpulan sisi frontend.** Fondasi pemanggilan riwayat per pasien sudah terbukti bekerja
untuk CPPT. Yang belum ada adalah tempatnya: halaman rekam medis yang berdiri sendiri, terpisah
dari layar antrean dokter. Pola service dan hook yang sudah ada dapat dipakai ulang sebagai
contoh.

---

## Reuse dan Ownership Map

Bagian ini menjawab perintah audit untuk mencegah pembuatan tabel baru yang menduplikasi data
yang sudah dimiliki modul lain.

| Konsep | Pemilik data yang sah | Referensi | Keputusan reuse |
|---|---|---|---|
| Pasien | `PatientManagement` | `BE .../PatientManagement/MasterData/Models/MstPatient.cs` | **Pakai ulang.** Jangan membuat tabel pasien rekam medis. Nomor rekam medis sudah ada di sini |
| Kunjungan | `RegistrationManagement` | `BE .../RegistrationManagement/Models/TrxPatientEncounter.cs` | **Pakai ulang.** Rekam medis mengelompokkan berdasarkan `EncounterId`, bukan membuat konsep kunjungan sendiri |
| Antrean | `RegistrationManagement` | `BE .../Models/TrxQueue.cs` | Tidak relevan untuk rekam medis |
| Dokter dan pegawai | `Corporate/HumanResource` | `MstDoctor`, `MstEmployee` | **Pakai ulang** untuk identitas penulis catatan |
| Isi klinis | `ClinicalManagement` | 13 entity `Trx*` | **Pakai ulang.** Sesuai `RM-DEC-001`, rekam medis tidak membuat ulang isi klinis |
| Diagnosis dan tindakan master | `HealthServices/MasterData` | `MstDiagnosis`, `MstProcedure` | **Pakai ulang** |
| Asuransi dan penjamin | `PatientManagement` dan `Administrator` | `MstPatientInsurance`, `MstInsuranceProvider` | Tidak dipakai pada rilis pertama |
| Resep | `PharmacyManagement` | `TrxPrescription` dan turunannya | **Pakai ulang** sebagai sumber tampilan, bukan disalin |
| Kamar dan tempat tidur | `HealthServices/MasterData` | `MstRoom`, `MstBed` | Tidak dipakai pada rilis pertama |

### Entity baru yang tampaknya memang diperlukan

Daftar ini adalah **temuan gap**, bukan desain. Bentuk akhirnya ditentukan pada tahap desain.

| Kebutuhan | Alasan tidak bisa pakai ulang |
|---|---|
| Jejak akses rekam medis | Tidak ada tabel sejenis di antara 445 `DbSet`. Log teks pada `LoggerService` bukan penyimpanan yang dapat ditelusuri dan dipertahankan |
| Addendum catatan klinis | Tidak ada tabel, kolom, maupun konsep sejenis |
| Riwayat versi dokumen klinis | `IdentityModel` tidak menyimpan nilai lama, dan tidak ada tabel riwayat |
| Tanda tangan catatan klinis | Hanya `TrxPatientConsent` punya `SignedAt`, dan itu untuk persetujuan pasien, bukan pengesahan penulis |

---

## Penelusuran lanjutan `RM-CAP-007` — penggabungan pasien duplikat

Ditelusuri 24 Agustus 2026 pada SHA yang sama. Sebelumnya berstatus `Unknown`; sekarang
tertutup dengan hasil yang berbeda dari dugaan awal.

### Yang ternyata sudah ada

| Yang ditemukan | Bukti |
|---|---|
| Kolom penanda dan alasan | `BE .../Models/MstPatient.cs:109` (`MergedToPatientId`), `:112` (`MergeReason`), `:132` (navigation `MergedToPatient`) |
| Relasi dan index | `BE Repositories/Configurations/HealthServices/MstPatientConfiguration.cs:205`; index pada snapshot `:63997` |
| Aturan validasi yang cukup lengkap | `BE .../PatientController.cs:2358-2395` — menolak menggabungkan pasien ke dirinya sendiri, mewajibkan alasan, dan memastikan pasien tujuan ada serta aktif |
| Perlindungan saat menghapus | `BE .../PatientController.cs:922-925` — pasien yang menjadi tujuan penggabungan tidak dapat dihapus |
| Hitungan pada ringkasan | `BE .../PatientController.cs:185` — jumlah pasien yang digabung tampil di ringkasan |
| Nilai status | `PatientStatus.Merged = 5` tersedia pada `BE .../MasterData/Enums/PatientStatus.cs` |
| Pilihan di antarmuka | `FE .../patient-constants.jsx:120` — pilihan "Digabung ke Pasien" |

### Yang ternyata tidak ada

| Yang tidak ditemukan | Akibatnya |
|---|---|
| **Endpoint penggabungan tersendiri** | Penggabungan dilakukan lewat `PUT /{id}` biasa, bercampur dengan penyuntingan data pasien lain. Tidak ada tindakan khusus yang dapat diberi hak akses terpisah maupun ditelusuri sebagai peristiwa |
| **Perpindahan data klinis** | Penelusuran `MergedToPatientId` di `ClinicalManagement`, `RegistrationManagement`, dan `PharmacyManagement` menghasilkan **nol** kemunculan. Kunjungan, CPPT, diagnosis, dan seluruh isi klinis **tetap melekat pada `PatientId` lama** |
| **Pengalihan pembacaan** | Tidak ada satu pun query di seluruh sistem yang mengikuti `MergedToPatientId` untuk mengalihkan pembacaan ke pasien tujuan |
| **Penetapan status otomatis** | `PatientStatus.Merged` hanya muncul **satu kali** di seluruh source, yaitu sebagai label tampilan pada `PatientController.cs:2664`. Tidak ada kode yang menetapkannya. Pasien yang digabung tetap berstatus `Active` kecuali diubah manual |
| **Pengiriman `mergeReason` dari antarmuka** | `FE .../patient-editor-utils.jsx:157` mengirim `mergedToPatientId` tetapi **tidak pernah** mengirim `mergeReason`, dan `mergeReason` tidak ada sama sekali pada `patient-constants.jsx` |

### Kesimpulan

**Penggabungan pasien di sistem ini hanyalah penandaan, bukan penyatuan.** Menyetel
`MergedToPatientId` menuliskan satu penunjuk pada baris pasien, dan tidak melakukan apa pun
terhadap riwayat klinisnya. Dugaan awal pada audit pertama — bahwa "alur kerjanya belum ada" —
ternyata kurang tepat: alurnya ada sebagian, tetapi berhenti pada penandaan.

Dampaknya bagi modul rekam medis menjadi pasti, bukan lagi kemungkinan:

> Bila seorang pasien memiliki dua nomor rekam medis dan salah satunya ditandai digabung,
> layar penelusuran **pasti** menampilkan riwayat yang terpecah. Bukan karena ada yang keliru,
> melainkan karena memang tidak ada mekanisme yang menyatukannya.

Karena itu statusnya dinaikkan dari `Unknown` menjadi **`Conflict`**: kolom `MergedToPatientId`
menjanjikan penggabungan yang tidak pernah terjadi, sama seperti `ConfidentialityLevel` yang
menjanjikan perlindungan yang tidak ditegakkan.

### Temuan tambahan: fitur penggabungan tidak dapat dipakai dari antarmuka

Dicatat sebagai `RM-CAP-033`, berstatus `Repair`.

Layar pasien menyediakan pilihan "Digabung ke Pasien", tetapi tidak pernah mengirim
`mergeReason`. Sementara `PatientController.cs:2380` menolak permintaan tanpa alasan:

> "Alasan merge wajib diisi jika patient digabung ke patient lain."

Artinya siapa pun yang mencoba menggabungkan pasien lewat antarmuka **selalu menerima galat
400**. Fitur itu tampak tersedia padahal tidak berfungsi.

Dua kemungkinan yang perlu dipastikan pemilik proses, dan keduanya membawa akibat berbeda:

| Kemungkinan | Akibat bagi rekam medis |
|---|---|
| Penggabungan memang belum pernah dipakai | Tidak ada pasien bernomor ganda di data. `BE-16` turun prioritasnya menjadi pengaman semata |
| Penggabungan pernah dijalankan lewat API atau langsung ke basis data | Ada pasien dengan riwayat terpecah. `BE-16` menjadi mendesak, dan perlu pendataan berapa banyak |

Pertanyaan ini **tidak dapat dijawab dari source** — perlu pemeriksaan data, yaitu menghitung
baris `MstPatient` yang `MergedToPatientId`-nya terisi. Itu berada di luar batas audit nomor 3.

### Pilihan yang jelas hanya ada tiga

Disajikan sebagai bahan, bukan sebagai keputusan. Yang memutuskan pemilik proses.

| Pilihan | Isinya | Biaya |
|---|---|---|
| Menolak membuka berkas pasien yang ditandai digabung | Sesuai rancangan `BE-16` sekarang: jawab `409` dan tunjukkan nomor penggantinya | Paling murah, dan jujur kepada pembaca |
| Menyatukan riwayat saat dibaca | Layar penelusuran mengikuti `MergedToPatientId` dan menggabungkan riwayat kedua nomor | Sedang. Perlu aturan bila rantai penggabungan lebih dari satu tingkat |
| Memindahkan data klinis saat penggabungan | Penggabungan sungguhan: seluruh kunjungan dan catatan dialihkan ke pasien tujuan | Paling mahal dan paling berisiko. Menyentuh data klinis nyata dan sulit dibatalkan |

---

## Conflict dan Unknown

### Conflict

| ID | Pertentangan | Mengapa ini pertentangan |
|---|---|---|
| `RM-CAP-009` | Empat model status berbeda untuk dokumen yang sama-sama bagian rekam medis | `RM-DEC-003` mensyaratkan satu aturan penguncian yang berlaku umum. Dengan empat model status, aturan itu harus ditulis empat kali dan akan menyimpang satu dari yang lain |
| `RM-CAP-025` | `SuperAdmin` melewati seluruh pemeriksaan kewenangan | Bertentangan dengan `RM-DEC-005` yang mensyaratkan pembedaan pasien rawatan dan bukan rawatan. Bertentangan pula dengan kebutuhan jejak akses, karena akses yang berhasil tidak dicatat. Masalah yang sama sudah tercatat sebagai syarat go-live pada `docs/module-blueprints/igd/blueprint-manifest.md` |
| `RM-CAP-026` | Kolom tingkat kerahasiaan tersimpan tetapi tidak menegakkan apa pun | Menimbulkan keyakinan palsu. Petugas dapat menandai dokumen `VeryConfidential` dan menganggapnya terlindungi, padahal siapa pun dengan izin controller tetap dapat membacanya |

### Unknown

| ID | Yang belum dapat dipastikan | Cara memastikannya |
|---|---|---|
| ~~`RM-CAP-007`~~ | **Tertutup 24 Agustus 2026.** Penelusuran selesai; statusnya naik menjadi `Conflict`. Lihat bagian "Penelusuran lanjutan `RM-CAP-007`" | — |
| `RM-CAP-015` | Apakah sidik jari dipakai sebagai pengesahan tanda tangan | Keputusan manusia, bukan temuan source. Menunggu `RM-DEC-011` |
| `RM-CAP-027` | Aturan kerahasiaan `PrivateNote` | Keputusan manusia. Menunggu `RM-DEC-012` |
| Seluruh capability | Apakah 162 migration sudah diterapkan ke database berjalan | Perlu akses database. Lihat batas audit nomor 1 |

---

## Ringkasan status

Denominator: 33 capability yang diaudit (32 pada audit awal, ditambah `RM-CAP-033` dari penelusuran lanjutan 24 Agustus 2026), seluruhnya diturunkan dari tiga kebutuhan rilis
pertama pada `RM-DEC-002` beserta fondasi pendukungnya.

| Status | Jumlah | Capability |
|---|---:|---|
| `Ready to reuse` | 10 | `RM-CAP-001`, `002`, `003`, `005`, `006`, `020`, `028`, `029`, `030`, `031` |
| `Reuse with adapter` | 2 | `RM-CAP-004`, `021` |
| `Extend` | 2 | `RM-CAP-010`, `018` |
| `Repair` | 6 | `RM-CAP-011`, `012`, `013`, `019`, `023`, `033` |
| `Missing` | 7 | `RM-CAP-008`, `014`, `016`, `017`, `022`, `024`, `032` |
| `Conflict` | 4 | `RM-CAP-009`, `025`, `026`, `007` |
| `Unknown` | 2 | `RM-CAP-015`, `RM-CAP-027` |
| **Total** | **33** | Setiap capability diberi tepat satu status utama |

Cara membaca angka ini: denominator 33 adalah seluruh capability yang diaudit, dan setiap
capability dihitung sekali saja. Jadi 10 dari 33 capability, atau sekitar 30 persen, sudah
siap dipakai ulang apa adanya.

### Yang paling penting dari audit ini

**Kabar baik.** Sisi *membaca* rekam medis jauh lebih siap daripada perkiraan awal. Data sudah
tersimpan per pasien, sudah terindeks untuk penelusuran urut waktu, dan satu endpoint riwayat
lintas kunjungan sudah berjalan dengan pemakai nyata di frontend. Kebutuhan pertama
`RM-DEC-002` sebagian besar adalah pekerjaan penggabungan dan penyajian, bukan pembangunan
dari nol.

**Kabar buruk.** Sisi *keutuhan* lebih lemah daripada perkiraan awal, dan ada tiga temuan yang
lebih serius daripada sekadar "belum ada":

1. CPPT — dokumen klinis yang paling sering ditulis — dapat diubah tanpa batas waktu oleh siapa
   pun yang punya izin controller (`RM-CAP-011`).
2. Penulis sebuah catatan klinis dapat dipindahkan ke orang lain melalui permintaan ubah biasa
   (`RM-CAP-012`). Ini melemahkan dasar `RM-DEC-004`, karena aturan "hanya penulis asli" belum
   punya identitas penulis yang terjamin.
3. Penanda read-only yang seharusnya melindungi justru dapat dilepas oleh pengirim permintaan
   (`RM-CAP-013`).

Ketiganya berstatus `Repair`, artinya bukan penambahan fitur melainkan penutupan celah pada
kode yang sedang dipakai. Karena tidak ada uji otomatis (`RM-CAP-032`), setiap perbaikan
menyentuh alur yang sedang dipakai IGD dan antrean dokter tanpa jaring pengaman.

---

## Closure Questions

Pertanyaan berikut **tidak dijawab pada audit ini**. Semuanya diteruskan ke `/grill-me`
Closure Pass karena memerlukan keputusan manusia, bukan pembacaan source.

| No | Pertanyaan | Menutup capability | Mengapa perlu diputuskan manusia |
|---:|---|---|---|
| 1 | Empat model status dokumen yang berbeda akan diseragamkan menjadi satu model baru, atau dipetakan lewat lapisan penerjemah? | `RM-CAP-009` | Menyeragamkan menyentuh entity yang sedang dipakai IGD dan antrean dokter; memetakan lebih aman tetapi meninggalkan keragaman |
| 2 | Tiga celah keutuhan pada CPPT (`RM-CAP-011`, `012`, `013`) diperbaiki lebih dulu sebelum modul rekam medis dibangun, atau dikerjakan bersamaan sebagai satu paket? | `RM-CAP-011`, `RM-CAP-012`, `RM-CAP-013` | Ini perbaikan pada kode yang sedang berjalan, tanpa uji otomatis. Urutannya menentukan risiko gangguan pelayanan |
| 3 | Catatan klinis yang **sudah ada sekarang** di database diperlakukan bagaimana ketika aturan penguncian mulai berlaku — dianggap terkunci semuanya, dibiarkan terbuka, atau dikunci hanya yang kunjungannya sudah selesai? | `RM-CAP-010`, `RM-CAP-018` | Menyangkut data klinis nyata milik pasien. Tidak boleh diputuskan oleh pelaksana teknis |
| 4 | Jejak akses disimpan di tabel database, atau cukup log terpusat dengan jaminan retensi? | `RM-CAP-022`, `RM-CAP-023` | Menyangkut kekuatan bukti hukum dan biaya penyimpanan |
| 5 | Berapa lama jejak akses wajib disimpan? | `RM-CAP-022` | Bergantung kebijakan retensi rumah sakit dan regulasi yang berlaku |
| 6 | Pembatasan akses `SuperAdmin` terhadap data klinis disetujui, dan siapa yang menanggung risiko bila akses teknis darurat jadi terhambat? | `RM-CAP-025` | Mengubah perilaku kewenangan di seluruh aplikasi, bukan hanya rekam medis. Preseden IGD menunjukkan ini menahan go-live |
| 7 | Tingkat kerahasiaan dokumen (`Normal` sampai `VeryConfidential`) mulai ditegakkan pada rilis pertama, atau tetap sebagai label sampai rilis berikutnya? | `RM-CAP-026` | Bila ditegakkan, dokumen yang sudah ditandai tinggi bisa mendadak tidak terbaca oleh yang selama ini membacanya |
| 8 | Penggabungan pasien ternyata hanya penandaan, tidak memindahkan data klinis. Riwayat pasien bernomor ganda **pasti** tampil terpecah. Mana yang dipilih: menolak membuka berkasnya, menyatukan saat dibaca, atau memindahkan data klinis sungguhan? | `RM-CAP-007` | Ditelusuri 24 Agustus 2026. Tiga pilihan beserta biayanya ada pada bagian "Penelusuran lanjutan `RM-CAP-007`" |
| 10 | Berapa banyak pasien yang `MergedToPatientId`-nya sudah terisi di data nyata? | `RM-CAP-007`, `RM-CAP-033` | **Tidak dapat dijawab dari source.** Perlu pemeriksaan data, di luar batas audit nomor 3. Menentukan apakah `BE-16` mendesak atau sekadar pengaman |
| 11 | Fitur penggabungan di layar pasien selalu gagal karena `mergeReason` tidak pernah dikirim. Diperbaiki sekarang, atau dibiarkan sampai keputusan nomor 8 diambil? | `RM-CAP-033` | Memperbaikinya sekarang berarti penggabungan menjadi mungkin dilakukan **sebelum** aturan tampilannya diputuskan |
| 9 | Perbaikan pada `RM-CAP-011` sampai `RM-CAP-013` memerlukan uji otomatis lebih dulu, atau cukup uji manual berbukti? | `RM-CAP-032` | Menyangkut alokasi waktu dan toleransi risiko, bukan pilihan teknis |

Empat pertanyaan yang sudah tercatat pada decision log juga tetap terbuka dan tidak dijawab
audit ini: `RM-DEC-007` (batas waktu entri susulan), `RM-DEC-009` (definisi pasien rawatan),
`RM-DEC-010` (definisi berhalangan), `RM-DEC-011` (bentuk tanda tangan), dan `RM-DEC-012`
(kerahasiaan `PrivateNote`).

---

## Impact Scan Trigger

Capability map ini terikat pada dua commit berikut:

| Repository | SHA |
|---|---|
| `NewQuilvianSystemBackend` | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |
| `QuilvianSystemFrontendDev` | `c4e2ef2a6080f3ce328d2faad79be1893ac13e22` |

Tandai map ini **stale** dan jalankan impact scan bila salah satu berkas berikut berubah,
karena seluruh kesimpulan audit bertumpu padanya:

| Berkas | Alasan |
|---|---|
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Sumber temuan `RM-CAP-003`, `011`, `012`, `013`, `027` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Sumber temuan `RM-CAP-010` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Sumber temuan `RM-CAP-018`, `019` |
| `Services/Security/AccessPermissionService.cs` | Sumber temuan `RM-CAP-025` |
| `Services/Logging/LoggerService.cs` | Sumber temuan `RM-CAP-023` |
| `Filters/AccessPermissionFilter.cs` | Sumber temuan `RM-CAP-022` |
| `Models/IdentityModel.cs` | Sumber temuan `RM-CAP-017` |
| `Models/SysAccessPolicy.cs` | Sumber temuan `RM-CAP-024` |
| `Repositories/ApplicationDbContext.cs` | Dipakai untuk membuktikan tidak adanya tabel jejak akses dan tabel versi |
| `Repositories/Configurations/HealthServices/*` | Sumber temuan `RM-CAP-002`, `030` |
| `src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js` | Bukti pemakai nyata `RM-CAP-003` |
| `src/utils/menu-sidebar/menu-items.jsx` | Sumber temuan `RM-CAP-008` |

Tambahan: bila migration baru ditambahkan pada `Migrations/`, periksa ulang batas audit nomor 1
sebelum map ini dipakai sebagai dasar desain.

---

# Revision 3 — Impact scan 28 Oktober 2026

Bagian ini menjawab pertanyaan yang ditinggalkan amandemen revision 7 decision log: **dokumen
apa di tiap modul yang boleh masuk berkas, kapan ia dianggap final, bagaimana menemukannya dari
id pasien, dan bagaimana mengetahui ia pernah diralat.** Seluruh temuan adalah fakta source pada
SHA backend `363de34f` dan frontend `f0a3b07b8`. Rekomendasi ditandai tersendiri.

## Batas audit revision 3

| No | Batas | Akibatnya |
|---:|---|---|
| 1 | Database, runtime, dan data tidak diperiksa | Sama seperti revision 2. Jumlah baris, isi data lama, dan perilaku nyata endpoint tidak dinilai |
| 2 | **Uji otomatis tidak dijalankan** | Klaim "Ready to reuse" bertumpu pada pembacaan kode, bukan hasil uji |
| 3 | Penelusuran per modul **terbatas pada model, enum, dan rute baca** | Aturan internal tiap modul (siapa boleh memfinalkan, alur koreksinya) tidak ditelusuri; itu milik blueprint modul masing-masing |
| 4 | Kontrak integrasi blueprint modul lain hanya dibaca bagian yang menyebut rekam medis | Bila sebuah modul sudah memutuskan arti "final" di dokumen keputusannya tetapi belum di kode, temuan ini tidak menangkapnya |

## Journey yang ditelusuri

**Journey 1 — dokter membuka berkas pasien.** Layar `medical-records/[slug]` memanggil
`MedicalRecordTimelineService`, yang menanyai setiap jenis dokumen secara terpisah. Bila satu
sumber melempar galat, sumber itu masuk `FailedSources` dan layar menampilkan blok peringatan
"RIWAYAT INI TIDAK LENGKAP" (`medical-record-completeness-alert.jsx`). **Jalur gangguan per
bagian sudah ada.** Yang belum ada: sumber dari modul lain.

**Journey 2 — kunjungan ditutup.** Penutupan kunjungan terjadi di **beberapa tempat berbeda**,
masing-masing memanggil `LockOpenDocumentsForEncounterAsync` (lihat `RM-CAP-036`). Penilaian
kelengkapan dan penanda `Susulan` harus berangkat dari salah satu waktu penutupan itu.

## Capability register revision 3

### Kelompok E — Fondasi rekam medis untuk amandemen

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `RM-CAP-034` | Gangguan per bagian pada linimasa (`RM-DEC-044`) | Rekam Medis | `MedicalRecordTimelineDtos.cs:124` `FailedSources`; `MedicalRecordTimelineService.cs:153-160` menangkap galat per sumber; `medical-record-completeness-alert.jsx` | **Ready to reuse** | Sumber modul baru cukup ikut pola yang sama. Keadaan `Tidak dapat dipastikan` pada kelengkapan belum ada (`RM-CAP-040`) | Rendah |
| `RM-CAP-035` | Jenis 14 dan 15 tidak boleh kosong diam-diam | Rekam Medis | `ClinicalDocumentKind.cs` memuat `HemodialysisSession = 14`, `LaboratoryResult = 15`; `AmbilJenisAsync` tidak punya cabangnya dan jatuh ke `_ => Task.FromResult((0, …))` (`MedicalRecordTimelineService.cs` ±baris 470). `TentukanJenis` mengembalikan `SeluruhJenis` bila saringan kosong | **Repair** | Kedua jenis diminta pada setiap pembukaan berkas lalu dijawab kosong tanpa galat — persis keadaan yang dilarang `RM-DEC-030` | **Tinggi** — dokter membaca "tidak ada hasil lab" padahal ada |
| `RM-CAP-036` | Satu waktu penutupan kunjungan sebagai dasar `Susulan` dan kelengkapan | Registrasi, Rawat Inap, IGD, Billing | `RegPatientEncounter.CompletedAt` diisi oleh `BillingFinalizationService.cs:891-892`, `PatientEncounterController.cs:1672-1675`, `EmergencyVisitService.cs:682`, `EmergencyDispositionService.AdmissionRequest.cs:1595-1596`. **Diubah ulang** oleh `EmergencyEncounterReconciliation.cs:188` dan dikembalikan pada `:262`. Rawat inap mengunci dokumen saat **episode** ditutup (`InpDischargeService.Closure.cs:853`, `encounterClosedAtUtc: now`), sedangkan kunjungannya baru `Completed` saat finalisasi tagihan. Waktu kunci per dokumen tersimpan di `MrcClinicalDocumentIntegrity.LockedEncounterClosedAt` | **Conflict** | Ada dua arti "kunjungan ditutup": tutup klinis (episode/visit) dan tutup administratif (`CompletedAt`). Untuk rawat inap jaraknya bisa berhari-hari | **Tinggi** — tanpa satu arti, dokumen yang sama bisa bertanda `Susulan` atau tidak tergantung sumber waktu yang dipakai |
| `RM-CAP-037` | Penanda `Susulan` dihitung saat dibaca (`RM-DEC-043`) | Rekam Medis | Nol temuan `Susulan`/`LateEntry`/`IsLate` (`RM-FACT-018`) | **Missing** | Bergantung `RM-CAP-036` dan waktu final per modul (kelompok F) | Sedang |
| `RM-CAP-038` | Master dokumen wajib (`RM-DEC-045`, `047`) | Rekam Medis | Nol model, controller, atau seeder dokumen wajib di `MedicalRecordManagement` maupun `MasterData` | **Missing** | Entity baru memang diperlukan | Rendah |
| `RM-CAP-039` | Addendum pengesahan atas catatan `LockedUnsigned` (`RM-DEC-049`) | Rekam Medis | `MrcClinicalNoteAddendum` dan `ClinicalNoteAddendumService` sudah ada; `MrcClinicalDocumentIntegrity.AddendumCount` ada. Tidak ada pembeda "addendum pengesahan" dari addendum koreksi biasa | **Extend** | Butuh penanda jenis addendum atau aturan "addendum bertanda tangan pertama = pengesahan" — pilihan desain | Sedang |
| `RM-CAP-040` | Penilaian kelengkapan per kunjungan dan daftar kerja (`RM-DEC-045`, `046`, `050`) | Rekam Medis | Nol service, endpoint, dan layar. `medical-record-completeness-alert.jsx` **bukan** kelengkapan berkas — ia peringatan sumber linimasa yang gagal. Namanya mirip, artinya berbeda | **Missing** | Perlu dibedakan namanya di desain supaya tidak tertukar | Sedang |
| `RM-CAP-041` | Pemberitahuan cakupan di layar berkas | Rekam Medis (frontend) | `medical-record-scope-notices.jsx:17-19` berbunyi "Aturan keutuhan dokumen baru berlaku pada CPPT". Source backend menegakkan **lima** jenis (`ClinicalDocumentIntegrityService.cs:87-93`) | **Repair** | Kalimat basi sejak jenis lain didaftarkan | Rendah — tetapi melanggar semangat `RM-FE-006`: layar wajib jujur tentang cakupannya |
| `RM-CAP-042` | Penarik sumber generik `AmbilAsync` | Rekam Medis | Menuntut entity menyediakan `PatientId` langsung (`RM-FACT-010`) | **Reuse with adapter** | Cocok untuk model yang punya `PatientId` (kelompok G). Model tanpa `PatientId` butuh penerjemah di modul pemilik (`RM-DEC-038`) | Rendah |

### Kelompok F — Sumber per modul (menjawab open question 13)

"Final" di kolom keempat adalah **apa yang ditunjukkan source hari ini**, bukan keputusan.
Kolom kelima adalah kolom yang paling layak menjadi waktu final untuk `RM-DEC-043`.

| ID | Modul | Dokumen | Final menurut source | Waktu final | Jalan ke pasien | Penanda pernah diralat | Status |
|---|---|---|---|---|---|---|---|
| `RM-CAP-043` | Laboratorium | Hasil per pemeriksaan (`LabExamination`) | `ReleasedAt != null`; status hasil **diturunkan** (`LabResultStatus.Released`, `LAB-DEC-080`) | `ReleasedAt` | `LabExamination.LabOrderId` → `LabOrder.EncounterId` → `RegPatientEncounter.PatientId`. Nol `PatientId` | **Belum ada.** `ReopenCount` hanya penyuntingan **sebelum** rilis (`LabExamination.cs:266-275`). Koreksi hasil terrilis (`S6`) belum dibangun; `LabResultCorrectionReason` baru master | **Extend** |
| `RM-CAP-044` | Laboratorium | Laporan patologi (`LabPathologyReport`) | `FinalizedAt` ada; hubungan dengan rilis per pemeriksaan **tidak ditelusuri** | Belum pasti | Lewat `LabOrderId`, sama seperti di atas | Belum ada | **Unknown** |
| `RM-CAP-045` | Radiologi | Laporan bacaan (`RadReport` + `RadReportVersion`) | Status bukan `Pending`/`Drafted`/`Validated` **dan** `FirstReleasedAt != null` | `FirstReleasedAt` | `RadReportService.GetByPatientAsync` sudah menerjemahkan id pasien lewat `RegPatientEncounters` | `CurrentVersionNumber > 1`; versi amandemen bertanda `IsAmendment`, `AmendmentReason`; `LastReleasedAt` | **Reuse with adapter** — lihat `RM-CAP-058` |
| `RM-CAP-046` | IGD | Triase (`EmgTriage`) | `TriageStatus = Completed` | `CompletedAt` | `EmergencyVisitId` → `EmgVisit.PatientId` | Status `Superseded` = triase diganti triase baru | **Extend** |
| `RM-CAP-047` | IGD | Disposisi (`EmgDisposition`) | `Confirmed` atau `Executed` — **dua kandidat** | `ConfirmedAt` / `ExecutedAt` | Lewat `EmgVisit.PatientId` | Tidak ditemukan | **Unknown** — arti final milik blueprint IGD |
| `RM-CAP-048` | IGD | Observasi, resusitasi | `Completed` (observasi juga `Escalated`; resusitasi juga `Stopped`) | `EndedAt` / `CompletedAt` | Lewat `EmgVisit.PatientId` | Tidak ditemukan | **Extend** |
| `RM-CAP-049` | Rawat Inap | Ringkasan pulang (`InpDischargeSummary`) | `SignedAt != null` | `SignedAt` | `EpisodeId` → `InpEpisode.PatientId` | `InpDischargeSummaryRevision` (sesi koreksi, `SupersededAt`) — ada baris revisi = pernah diralat | **Extend** |
| `RM-CAP-050` | Rawat Inap | Dokumen admisi (`InpAdmissionDocument`), termasuk `GeneralConsent = 7` | `Status = Completed` | `CompletedAt` | `PatientId` langsung | `Superseded` + `PreviousVersion` | **Extend** |
| `RM-CAP-051` | Kamar Operasi | Laporan operasi (`OprExecutionRecord`), catatan anestesi (`OprAnesthesiaRecord`) | `OprRecordStatus.Final` | `FinalizedAt` | `OprCaseId` → `OprCase.PatientId` | Laporan operasi: `OprExecutionAddendum`. Anestesi: tidak ditemukan | **Extend** |
| `RM-CAP-052` | Kamar Operasi | Checklist keselamatan (`OprSafetyChecklist`), catatan kelahiran (`OprBirthEvent`) | Checklist `Completed`; kelahiran **tanpa status** — final saat tersimpan | `SignedAt`; `BirthDateTime` (waktu kejadian, bukan waktu final) | Lewat `OprCase.PatientId` | Checklist: kolom `Revision`. Kelahiran: tidak ada | Checklist **Extend**; kelahiran **Unknown** |
| `RM-CAP-053` | Hemodialisa | Catatan sesi (`HmdSession`) | `SessionStatus = Finalized` dan terdaftar di mesin keutuhan (`HmdSessionFinalizationService`) | `SignedAt` | `EpisodeId` → `HmdEpisode.PatientId` | Addendum mesin keutuhan rekam medis (`AddendumCount`) | **Extend** — sudah ikut mesin keutuhan, tinggal cabang linimasa (`RM-CAP-035`) |
| `RM-CAP-054` | Gizi | Catatan asuhan gizi (`GziNutritionCareRecord`) | **Tanpa status.** Ditambahkan sekali (`NutritionOrderService.cs:405-431`), tetapi diagnosisnya dapat diganti lewat `PUT records/{recordId}/diagnoses` (`NutritionRequirementController.cs:129`) | `VisitAt` (waktu kunjungan, bukan waktu final) | `NutritionOrderId` → `GziNutritionOrder.PatientId` | Tidak ada | **Unknown** — arti final milik blueprint Gizi |
| `RM-CAP-055` | Bank Darah | Golongan darah (`BbkBloodGroupExam`), reaksi transfusi (`BbkTransfusionReactionNotice`) | Golongan darah `Validated`; reaksi `Acknowledged` | `ValidatedAt`; `AcknowledgedAt` | `PatientId` langsung (golongan darah **tanpa** `EncounterId`) | Golongan darah: `BbkBloodGroupConflictResolution` untuk selisih hasil | **Reuse with adapter** |
| `RM-CAP-056` | Farmasi | Resep (`PhmPrescription`), pemberian obat (`PhmMedicationAdministration`), rekonsiliasi | Resep: `Submitted` (status alur `FulfillmentStatus` terpisah). Pemberian: `DoseStatus = Administered`. Rekonsiliasi: `DecidedAt` | `SubmittedAt`; `AdministeredAt`/`RecordedAt`; `DecidedAt` | `PatientId` langsung (resep, pemberian) | Pemberian: `PhmMedicationAdministrationRevision` | **Reuse with adapter** |
| `RM-CAP-057` | MCU | Resume (`McuResume`), hasil (`McuExaminationResult`) | Resume `Verified` (atau `Corrected`); hasil **tanpa status** — final mengikuti resume/order `Verified` | `VerifiedAt` | `McuOrderId` → `McuOrder.PatientId` — **nullable**: peserta tanpa nomor rekam medis tidak dapat dijangkau | Resume `Corrected`, `ReopenedAt`; nilai hasil `McuExaminationResultValueRevision` | **Extend** |

### Kelompok G — Model `ClinicalManagement` yang belum terbaca

Seluruh model di bawah menyimpan `PatientId` langsung, sehingga **dapat ditarik dengan
`AmbilAsync` tanpa penerjemah** (`RM-CAP-042`). Yang perlu diputuskan saat desain adalah mana
yang memang dokumen berkas dan mana yang sekadar pencatatan berkala.

| ID | Model | Final menurut source | Waktu | Ralat | Sifat |
|---|---|---|---|---|---|
| `RM-CAP-059` | `CliNursingCarePlan` | Tanpa status; `OpenedAt` | `OpenedAt` | `CliNursingCarePlanItemRevision` | Dokumen |
| `RM-CAP-060` | `CliPhysicianVisit` | `Recorded` (selain `Cancelled`) | Waktu catat | Tidak ditemukan | Dokumen |
| `RM-CAP-061` | `CliNursingIntervention` | `Finalized` | `FinalizedAt` | Tidak ditemukan | Dokumen |
| `RM-CAP-062` | `CliTransfusionMonitoring` | `Completed` (juga `Stopped`) | `CompletedAt` | Tidak ditemukan | **Dokumen wajib bersyarat** `RM-DEC-047` |
| `RM-CAP-063` | `CliDoctorCertificate` | `Issued` | Tidak ditemukan kolom waktu terbit | Tidak ditemukan | Dokumen |
| `RM-CAP-064` | `CliCaseManagementEvaluation` | `Completed` | `CompletedAt` | Tidak ditemukan | Dokumen |
| `RM-CAP-065` | `TrxNosocomialInfection` | `VerifiedAt` | `VerifiedAt` | Tidak ditemukan | Dokumen |
| `RM-CAP-066` | `CliDailyObservation`, `CliFluidBalanceEntry`, `CliBloodGlucoseReading`, `CliEquipmentUsage`, `CliWsdDrain`, `CliSurgicalSiteSurveillance` | `Active` (selain `Cancelled`) | Waktu ukur | `RevisionNumber` + tabel `*Revision` | Pencatatan berkala — **calon banjir linimasa**; satu pasien rawat inap dapat menghasilkan ratusan baris |
| `RM-CAP-067` | `CliTransferHandover` | Tanpa `PatientId`; lewat `InpEpisodeId` | `ReceivedAt` | Tidak ditemukan | Dokumen, **butuh penerjemah** |

### Kelompok H — Jalur baca berlingkup pasien (menjawab open question 15)

| ID | Jalur | Bukti | Status | Catatan |
|---|---|---|---|---|
| `RM-CAP-058` | Radiologi `GetByPatientAsync` | `RadReportService.cs:568`; hanya laporan yang sudah dirilis, kunjungan pasien lewat subquery satu perjalanan; DTO memuat `FirstReleasedAt`, `LastReleasedAt`, `CurrentVersionNumber` | **Reuse with adapter** | Wajib dipanggil sebagai **service dalam proses**, bukan lewat HTTP: rute `GET by-patient/{patientId}` dijaga `RadReport : Read`, sedangkan `RM-DEC-040` hanya menuntut `MedicalRecord : Read` |
| `RM-CAP-068` | Bank Darah `GET patient/{patientId}/valid` | `BbkBloodGroupExamController.cs:187` | **Reuse with adapter** | Namanya menunjukkan hanya golongan darah yang berlaku, bukan riwayat. Isi kembaliannya belum dibaca |
| `RM-CAP-069` | Laboratorium berlingkup pasien | Satu-satunya: `kiosk/pending-by-patient/{patientId}` (`LabOrderController.cs:286`), untuk kiosk. Jalur hasil hanya `by-order` | **Missing** | Blocker 7 tetap terbuka |
| `RM-CAP-070` | IGD, Rawat Inap, Kamar Operasi, Hemodialisa, Gizi, Farmasi, MCU | Nol jalur hasil/dokumen berlingkup pasien yang cocok. `OperatingRoomScheduleLookupController` `lookup-by-patient` dan `NutritionDietController` `patients` bukan jalur dokumen | **Missing** | Lima di antaranya (Farmasi, Bank Darah, sebagian `ClinicalManagement`, Rawat Inap dokumen admisi, Kamar Operasi lewat `OprCase`) cukup sederhana karena `PatientId` tersedia satu langkah |

### Kelompok I — Pemetaan dokumen wajib `RM-DEC-047` (menjawab open question 14)

| Butir dokumen wajib | Sumber | Syarat terpenuhi | Status |
|---|---|---|---|
| Asesmen (Rawat Jalan, IGD) | `TrxPatientAssessment` | `AssessmentStatus = Completed` | **Ready to reuse** — tetapi lihat closure question 3 tentang jenisnya |
| Asesmen awal (Rawat Inap) | `TrxPatientAssessment` | `AssessmentType` `Initial` (keperawatan) **dan/atau** `MedicalInitial` (medis) | **Unknown** — dua jenis asesmen awal; closure question 3 |
| Diagnosis utama | `TrxPatientDiagnosis` | `IsPrimary = true` **atau** `DiagnosisType = Primary` — dua penanda, keduanya dipakai frontend (7 dan 11 berkas) | **Conflict** — closure question 4 |
| CPPT/SOAP dokter | `TrxPatientIntegratedProgressNote` (`ProfessionType`) **atau** `TrxDoctorConsultation` | Belum pasti | **Unknown** — closure question 5 |
| CPPT (Rawat Inap) | `TrxPatientIntegratedProgressNote` | Minimal satu, tidak dibatalkan | **Ready to reuse** |
| Triase | `EmgTriage` | `Completed` | **Extend** (`RM-CAP-046`) |
| Disposisi | `EmgDisposition` | `Confirmed` atau `Executed` | **Unknown** (`RM-CAP-047`) |
| Persetujuan umum (Rawat Inap) | `RegPatientGeneralConsent` (registrasi, per kunjungan, `SignedAt`) **dan** `InpAdmissionDocument` `GeneralConsent` (admisi, `Completed`) | Dua tempat menyimpan persetujuan umum | **Conflict** — closure question 6 |
| Ringkasan pulang | `InpDischargeSummary` | `SignedAt != null` | **Extend** (`RM-CAP-049`) |
| Resume MCU | `McuResume` | `Verified` atau `Corrected` | **Extend** (`RM-CAP-057`) |
| Laporan operasi, catatan anestesi | `OprExecutionRecord`, `OprAnesthesiaRecord` | `Final` | **Extend** (`RM-CAP-051`) |
| Checklist keselamatan | `OprSafetyChecklist` | `Completed` | **Extend** (`RM-CAP-052`) |
| Pemicu "kunjungan memuat operasi" | `OprCase` per `EncounterId` | Kasus berstatus `Completed` (bukan `Cancelled`/`Rejected`/`Postponed`) | **Ready to reuse** |
| Catatan sesi HD + pemicunya | `HmdSession` per `EncounterId` atau `InpEpisodeId` | Sesi `Finalized` dan `MrcClinicalDocumentIntegrity` `Signed`; pemicu = sesi `Completed` ke atas | **Extend** |
| Monitoring transfusi + pemicunya | `CliTransfusionMonitoring` per `EncounterId` | `Completed`; pemicu = baris apa pun yang tidak `Cancelled` | **Ready to reuse** |

## Blocker Laboratorium 5, 6, 7

| Blocker | Temuan revision 3 | Usulan keadaan |
|---|---|---|
| 5 — entri hasil belum ada | Nilai hasil, finalisasi, validasi, dan rilis tersimpan di `LabExamination` (`ResultNumeric`, `FinalizedAt`, `ValidatedAt`, `ReleasedAt`). Hasil dirilis didaftarkan sebagai `ClinicalDocumentKind.LaboratoryResult` | **Tertutup** |
| 6 — mekanisme ralat hasil terrilis | Hanya master alasan (`LabResultCorrectionReasonController`). Nol pemakaian di service koreksi. `ReopenCount` khusus sebelum rilis | **Tetap terbuka.** Penanda "pernah diralat" untuk hasil lab belum punya sumber |
| 7 — jalur baca berlingkup pasien | `RM-CAP-069` | **Tetap terbuka** |

## Conflict dan Unknown revision 3

### Conflict

| ID | Pertentangan | Yang harus memutuskan |
|---|---|---|
| `RM-CAP-036` | Dua arti "kunjungan ditutup" — tutup klinis dan tutup administratif — dan `CompletedAt` IGD dapat diubah rekonsiliasi | Pemilik rekam medis (closure question 1) |
| Pemetaan diagnosis utama | `IsPrimary` dan `DiagnosisType.Primary` hidup berdampingan | Pemilik rekam medis bersama `ClinicalManagement` (closure question 4) |
| Pemetaan persetujuan umum | Registrasi dan admisi rawat inap sama-sama menyimpannya | Pemilik rekam medis (closure question 6) |

### Unknown

| ID | Yang belum pasti | Cara memastikannya |
|---|---|---|
| `RM-CAP-044` | Hubungan laporan patologi dengan rilis per pemeriksaan | Penelusuran terarah di blueprint Laboratorium |
| `RM-CAP-047` | Arti final disposisi IGD | Kontrak blueprint IGD; bila belum ada, pemilik IGD |
| `RM-CAP-052` | Catatan kelahiran tanpa status | Pemilik Kamar Operasi/Kamar Bersalin |
| `RM-CAP-054` | Arti final catatan asuhan gizi | Pemilik Gizi |
| `RM-CAP-063` | Waktu terbit surat dokter | Penelusuran kolom audit warisan (`CreateDateTime`) saat desain |
| `RM-CAP-068` | Isi kembalian jalur Bank Darah | Baca service-nya saat desain |
| Seluruh kelompok F–I | Kualitas data nyata (berapa dokumen lama tanpa waktu final) | Perlu akses database; batas audit nomor 1 |

## Yang paling penting dari impact scan ini

1. **Kabar baik: fondasi gangguan per bagian sudah ada** (`RM-CAP-034`). Menambah sumber modul
   lain tidak perlu merancang ulang cara layar menghadapi gangguan.
2. **Satu kerusakan sedang terjadi hari ini** (`RM-CAP-035`): hasil laboratorium dan sesi
   hemodialisa selalu tampil kosong di berkas. Ini bukan soal amandemen — ia sudah melanggar
   `RM-DEC-030` — dan tidak menunggu siapa pun.
3. **Dua belas sumber, tiga pola jalan ke pasien.** Model dengan `PatientId` langsung (Farmasi,
   Bank Darah, sebagian besar `ClinicalManagement`, dokumen admisi) dapat ditarik dengan helper
   yang ada. Model yang berpusat pada kunjungan atau kasus (Laboratorium, Radiologi, Kamar
   Operasi, Hemodialisa, Gizi, IGD, ringkasan pulang, MCU) butuh penerjemah di modul pemiliknya
   — dan **baru Radiologi yang punya**.
4. **Pencatatan berkala dapat membanjiri linimasa** (`RM-CAP-066`). Keputusan "semua model
   `ClinicalManagement` masuk" (`RM-DEC-039` kelompok C) perlu dibatasi saat desain; ini
   pilihan tampilan, bukan aturan bisnis baru.
5. **Waktu penutupan kunjungan harus diputuskan lebih dulu** (`RM-CAP-036`). Penanda `Susulan`,
   cakupan daftar kerja, dan batas tanggal rilis `RM-DEC-050` seluruhnya bergantung padanya.

## Closure questions revision 3

Seluruhnya keputusan manusia; tidak satu pun dapat dijawab source.

| No | Pertanyaan | Menahan |
|---:|---|---|
| 1 | Waktu penutupan kunjungan yang dipakai untuk `Susulan` dan kelengkapan: tutup klinis (episode rawat inap/visit IGD ditutup, saat dokumen dikunci) atau tutup administratif (`CompletedAt`, untuk rawat inap saat finalisasi tagihan)? Dan bila rekonsiliasi IGD mengubah `CompletedAt`, apakah penanda ikut berubah? | Desain `RM-DEC-043`, `046`, `050` |
| 2 | Apakah pencatatan berkala (`RM-CAP-066`) masuk linimasa sebagai baris per pengukuran, sebagai ringkasan per hari, atau tidak masuk? | Desain kelompok C |
| 3 | Asesmen awal rawat inap: keperawatan (`Initial`), medis (`MedicalInitial`), atau keduanya wajib? Asesmen rawat jalan/IGD: jenis apa saja yang dihitung? | Master dokumen wajib |
| 4 | Diagnosis utama dibaca dari `IsPrimary`, `DiagnosisType = Primary`, atau salah satunya cukup? | Master dokumen wajib |
| 5 | "CPPT/SOAP dokter" dipenuhi oleh CPPT berprofesi dokter, konsultasi dokter (`TrxDoctorConsultation`), atau salah satunya? | Master dokumen wajib |
| 6 | Persetujuan umum rawat inap dipenuhi oleh persetujuan registrasi, dokumen admisi `GeneralConsent`, atau salah satunya? | Master dokumen wajib |
| 7 | Hasil laboratorium sudah dapat tampil, tetapi penanda "pernah diralat" belum punya sumber (blocker 6). Tampilkan hasil lab sekarang tanpa penanda — dengan keterangan bahwa ralat belum terlacak — atau tunggu `S6`? | Urutan `RM-DEC-048` langkah 1 |
| 8 | Disposisi IGD final saat `Confirmed` atau `Executed`; catatan asuhan gizi final kapan; catatan kelahiran final kapan? | Sumber IGD, Gizi, Kamar Operasi. Boleh dijawab pemilik modul masing-masing |

## Impact scan trigger revision 3

Tandai bagian ini **stale** bila berkas berikut berubah:

| Berkas | Temuan |
|---|---|
| `Areas/HealthServices/MedicalRecordManagement/Services/MedicalRecordTimelineService.cs` | `RM-CAP-034`, `035`, `042` |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | `RM-CAP-041`, `053` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`, `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs`, `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs`, `EmergencyEncounterReconciliation.cs`, `RegistrationManagement/Controllers/PatientEncounterController.cs` | `RM-CAP-036` |
| `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs`, `Controllers/LabOrderController.cs`, `Controllers/LabExaminationController.cs` | `RM-CAP-043`, `069`, blocker 5–7 |
| `Areas/HealthServices/RadiologyManagement/Services/RadReportService.cs` | `RM-CAP-045`, `058` |
| Model pada kelompok F dan G | Arti final, waktu final, dan penanda ralat |
| `src/components/view/health-services/medical-record-management/medical-records/medical-record-scope-notices.jsx`, `medical-record-completeness-alert.jsx` | `RM-CAP-034`, `041` |
