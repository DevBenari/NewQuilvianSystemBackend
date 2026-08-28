# Peta Kemampuan Existing — Modul Gizi

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Revision | `1` |
| Status | `draft` |
| Backend SHA | `f2c5090` |
| Frontend SHA | `847be1fc0` |
| Titik mulai | Registry sistem `SEGAR`, dipindai 2026-08-27 |

Audit ini read-only terhadap source. Ia menjawab satu pertanyaan: **kebutuhan modul Gizi mana
yang sudah tersedia, dan mana yang benar-benar belum ada.**

## Ringkasan

| Status | Jumlah kemampuan |
|---|---:|
| Ready to reuse | 11 |
| Extend | 1 |
| Missing | 4 |
| Conflict | 0 |
| Unknown | 1 |

## Temuan paling menentukan

**Skrining gizi awal ternyata sudah ada.** Ini menjawab `GIZ-OQ-001` yang sebelumnya ditandai
memblokir desain.

`TrxPatientAssessment` milik Clinical Management sudah membawa bagian bertanda `NUTRITION`:

| Kolom | Tipe | Kegunaan bagi Gizi |
|---|---|---|
| `NutritionRiskStatus` | enum `NoRisk`, `LowRisk`, `MediumRisk`, `HighRisk` | Penentu apakah pasien perlu dirujuk ke ahli gizi |
| `NutritionRiskScore` | `int?` | Nilai skor skrining |
| `NutritionNote` | `varchar(500)?` | Catatan skrining |
| `AppetiteStatus` | enum | Nafsu makan |
| `HasNausea`, `HasVomiting` | `bool` | Gejala yang memengaruhi asupan |
| `Weight`, `Height`, `BMI` | `decimal?` | Data antropometri dasar |

Bukti: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` baris 82 sampai
173 @ `f2c5090`. Enum pada `Areas/HealthServices/ClinicalManagement/Enums/NutritionRiskStatus.cs`
@ `f2c5090`.

Artinya modul Gizi **tidak boleh membuat skrining gizi sendiri**. Ia membaca
`NutritionRiskStatus` dari asesmen pasien sebagai prasyarat order, persis seperti yang
diputuskan pada `GIZ-DEC-003`.

> **Contoh:** Perawat mengisi asesmen pasien saat masuk bangsal dan menandai
> `NutritionRiskStatus = HighRisk` dengan `NutritionRiskScore = 4`. Modul Gizi membaca nilai
> itu dan menampilkan pasien tersebut sebagai kandidat rujukan. Dokter penanggung jawab lalu
> membuat order konsultasi gizi. Modul Gizi tidak pernah menulis ke `TrxPatientAssessment`.

## Temuan kedua: CPPT sudah menyediakan tempat untuk Gizi

`TrxPatientIntegratedProgressNote`, Catatan Perkembangan Pasien Terintegrasi, berada pada
tingkat `L4 Terpakai` dan endpoint-nya sudah dipanggil frontend.

Yang menentukan bukan kemiripan struktur, melainkan bahwa **tempat untuk gizi sudah tertulis
di dalam kodenya**:

> `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`
> @ `f2c5090`
>
> | Baris | Isi |
> |---:|---|
> | 1268 | `new() { Value = "Nutrition", Label = "Gizi" }` pada daftar pilihan `SourceModule` |
> | 1294 | `"gizi" or "nutrition" or "nutritionist" => "Nutritionist"` |
> | 1309 | `"Nutritionist" => "Gizi"` sebagai nama profesi bawaan |

Perancang CPPT sudah menyiapkan jalur bagi catatan ahli gizi. Yang belum ada hanyalah modul
yang mengisinya.

### Kolom CPPT yang dipakai Gizi

| Kolom | Kegunaan bagi Gizi |
|---|---|
| `ProfessionType`, `ProfessionName` | Menandai catatan sebagai catatan ahli gizi |
| `SourceModule`, `SourceReferenceId`, `SourceReferenceNumber` | Menunjuk balik ke catatan asuhan gizi terstruktur |
| `SubjectiveSummary`, `ObjectiveSummary`, `AssessmentSummary`, `PlanSummary` | Narasi kunjungan |
| `Instruction` | Instruksi untuk perawat dan tenaga lain |
| `Evaluation` | Evaluasi tindak lanjut |
| `ProviderUserId`, `ProviderDisplayNameSnapshot` | Siapa ahli gizi yang menulis |

### Yang tetap harus dibuat Gizi

CPPT menyimpan **narasi**, bukan **angka**. Seluruh kolom isinya teks bebas, sehingga tidak
dapat dipakai menghitung laporan mutu gizi maupun menelusuri diagnosis per kelompok.

Karena itu empat hal berikut tetap disimpan terstruktur di entity milik Gizi: diagnosis gizi
berkode, target dan capaian intervensi, recall asupan, serta diet dan kebutuhan nutrisi.

## Peta kemampuan

| Kemampuan yang dibutuhkan Gizi | Status | Entity atau lokasi | Tingkat | Catatan |
|---|---|---|---|---|
| Identitas pasien | Ready to reuse | `MstPatient` | `L4` | Jangan buat pasien versi Gizi |
| Episode pelayanan | Ready to reuse | `TrxPatientEncounter` | `L4` | Induk seluruh pelayanan |
| Episode rawat inap | Ready to reuse | `InpEpisode` | `L3` | Membawa `EpisodeStatus`, `DischargeDecidedAt`, `PhysicallyLeftAt`, `ClosedAt` |
| **Skrining gizi awal** | **Ready to reuse** | `TrxPatientAssessment` bagian `NUTRITION` | `L4` | Menjawab `GIZ-OQ-001` |
| Antropometri berat, tinggi, BMI | Ready to reuse | `TrxPatientAssessment` | `L4` | Sudah ada, tidak perlu dibuat ulang |
| Dokter penanggung jawab | Ready to reuse | `MstDoctor`, `InpDoctorAssignment` | `L4`, `L3` | Penentu kewenangan order pada `GIZ-DEC-007` |
| Profil tenaga dan profesi | Ready to reuse | `MstWorkforceProfile`, `MstProfession`, `MstSpecialization` | `L4` | `MstProfession` membawa `ProfessionCode`, `ProfessionName`, `IsClinicalProfession`, dan `RequiresLicense`. Ahli gizi cukup menjadi satu baris profesi klinis |
| Diagnosis medis | Ready to reuse | `MstDiagnosis` | `L4` | Untuk diagnosis awal pada order. **Bukan** diagnosis gizi |
| Unit layanan | Ready to reuse | `MstServiceUnit` | `L4` | Unit gizi sebagai unit layanan |
| Penutupan saat pasien keluar | Extend | `InpEpisode` | `L3` | Modul Gizi perlu membaca perubahan status episode. Mekanismenya belum ada |
| Order konsultasi gizi | Missing | — | — | Diputuskan entity sendiri pada `GIZ-DEC-001` |
| Kunjungan ahli gizi | Ready to reuse | `TrxPatientIntegratedProgressNote` (CPPT) | `L4` | `GIZ-DEC-010`. Tempat untuk gizi sudah disiapkan: `SourceModule` `Nutrition` dan `ProfessionType` `Nutritionist` sudah terdaftar di controller |
| Asuhan gizi per kunjungan | Missing | — | — | Asesmen, diagnosis, intervensi, monitoring dan evaluasi |
| Master diagnosis gizi berkode | Ready to reuse | `MstDiagnosis` dengan `DiagnosisType` baru | `L4` | `GIZ-DEC-009`. Master sudah menampung banyak jenis; isinya menunggu `GIZ-OQ-002` |
| Recall asupan makanan | Missing | — | — | Belum ada entity mana pun |
| Penentuan diet dan kebutuhan nutrisi | Missing | — | — | Bentuknya menunggu `GIZ-OQ-004` |
| Cara mengetahui pasien keluar rawat inap | Unknown | — | — | Belum diketahui apakah ada event, notifikasi, atau hanya polling status |

## Kontrak as-is yang relevan

### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

Grup ini sudah ada di Swagger dan **sudah dipanggil frontend**, jadi tingkatnya `L4 Terpakai`.
Modul Gizi akan menjadi pembaca tambahan endpoint ini, bukan pemiliknya.

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

CPPT. Tingkat `L4 Terpakai`. Modul Gizi menjadi penulis tambahan pada endpoint ini dengan
`SourceModule` bernilai `Nutrition`. Aturan CPPT tetap milik Clinical Management.

### Health Services / InPatient Management

Base URL: `api/v1/health-services/inpatient-management/...`

Lima controller tersedia, tetapi **belum satu pun dipanggil frontend**. Seluruh entity `Inp*`
berada di `L3`, bukan `L4`. Modul Gizi tetap dapat membacanya dari sisi backend.

## Yang sengaja tidak akan dibuat ulang

| Yang ditolak | Alasan |
|---|---|
| `MstPatientGizi` atau sejenisnya | Pasien dimiliki Patient Management, dipakai lewat `PatientId` |
| Skrining gizi versi Gizi | Sudah ada di `TrxPatientAssessment`, tingkat `L4` |
| Berat, tinggi, dan BMI versi Gizi | Sudah ada di asesmen pasien |
| Master dokter atau ahli gizi versi Gizi | Dimiliki HR Master Data |
| Konsultasi versi Gizi yang meniru `TrxDoctorConsultation` | `GIZ-DEC-001` memutuskan entity terpisah dengan alasan tertulis, bukan meniru |
| Catatan kunjungan gizi versi Gizi | Sudah ada di CPPT `TrxPatientIntegratedProgressNote`, dan tempat untuk gizi sudah disiapkan di dalamnya. `GIZ-DEC-010` |
| Master diagnosis gizi tersendiri | `MstDiagnosis` sudah menampung banyak jenis lewat `DiagnosisType`. `GIZ-DEC-009` |

## Conflict dan Unknown

| ID | Temuan | Dampak |
|---|---|---|
| `GIZ-UK-001` | Belum diketahui bagaimana modul lain mengetahui pasien keluar rawat inap. Tidak ditemukan mekanisme event atau notifikasi lintas modul di source | `GIZ-DEC-008` menetapkan asuhan gizi ditutup saat pasien keluar. Tanpa mekanisme yang jelas, penutupan hanya dapat dilakukan dengan memeriksa status episode setiap kali halaman dibuka, dan itu tidak menutup order pasien yang sudah lama pulang |

Tidak ditemukan conflict. `KF-003` pada registry sudah ditutup lewat `GIZ-DEC-001`.

## Dampak terhadap pertanyaan terbuka

| ID | Sebelum audit | Setelah audit |
|---|---|---|
| `GIZ-OQ-001` pemilik data skrining gizi | Memblokir desain | **Tertutup.** Skrining ada di `TrxPatientAssessment`, dimiliki Clinical Management |
| `GIZ-OQ-003` penanda ahli gizi | Tidak memblokir | **Hampir tertutup.** `MstProfession` tersedia di `L4`. Tinggal memastikan ada baris profesi untuk ahli gizi |
| `GIZ-OQ-002` isi master diagnosis gizi | Memblokir desain | **Masih memblokir.** Tidak dapat dijawab audit kode |
| `GIZ-OQ-004` bentuk kebutuhan nutrisi | Memblokir desain | **Masih memblokir.** Tidak dapat dijawab audit kode |
| `GIZ-OQ-006` pemilik proses bisnis | Memblokir persetujuan | **Masih terbuka** |

Dua pertanyaan pemblokir tersisa keduanya membutuhkan pemilik proses gizi, bukan pembacaan
source. Desain belum boleh dimulai.

## Batas audit ini

Yang **tidak** diperiksa: isi data sungguhan, termasuk apakah `MstProfession` benar-benar sudah
berisi baris untuk ahli gizi, dan apakah `NutritionRiskStatus` benar-benar diisi petugas dalam
praktik sehari-hari. Keduanya memerlukan pemeriksaan basis data berjalan, bukan pembacaan
source.

