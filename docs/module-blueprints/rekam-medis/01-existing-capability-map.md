# Rekam Medis — Existing Capability Map

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `2` |
| Mode | Audit penuh (bukan impact scan) |
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
| `RM-CAP-002` | Query riwayat per pasien berjalan cepat | `ClinicalManagement` | Index gabungan `(PatientId, <tanggal>, IsDelete)` terbukti pada `BE Repositories/Configurations/HealthService/TrxPatientIntegratedProgressNoteConfiguration.cs:169-174`, dan pola sama pada `TrxPatientDiagnosisConfiguration.cs:238`, `TrxDoctorConsultationConfiguration.cs:312`, `TrxPatientVitalSignConfiguration.cs:166`, `TrxPatientClinicalDocumentConfiguration.cs:183` | `Ready to reuse` | Tidak ada | Rendah |
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
| `RM-CAP-030` | Pola konfigurasi EF per entity | — | `BE Repositories/Configurations/HealthService/` memuat konfigurasi terpisah per entity dengan index, keunikan, dan `DeleteBehavior.Restrict`, contohnya `TrxPatientIntegratedProgressNoteConfiguration.cs:154-220` | `Ready to reuse` | Tidak ada | Rendah |
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
`BE Repositories/Configurations/HealthService/TrxPatientIntegratedProgressNoteConfiguration.cs:199-204`.
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
| Relasi dan index | `BE Repositories/Configurations/HealthService/MstPatientConfiguration.cs:205`; index pada snapshot `:63997` |
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
| `Repositories/Configurations/HealthService/*` | Sumber temuan `RM-CAP-002`, `030` |
| `src/lib/hooks/health-services/clinical-management/use-doctor-cppt.js` | Bukti pemakai nyata `RM-CAP-003` |
| `src/utils/menu-sidebar/menu-items.jsx` | Sumber temuan `RM-CAP-008` |

Tambahan: bila migration baru ditambahkan pada `Migrations/`, periksa ulang batas audit nomor 1
sebelum map ini dipakai sebagai dasar desain.
