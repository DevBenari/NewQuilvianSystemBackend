# Laporan Perubahan Backend — `BE-RWI-142`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-142` |
| Judul | Penyempurnaan diagnosa untuk SOAP |
| Slice | Rencana kerja SOAP Dokter Rawat Inap Rev 2.1, gelombang 1 |
| Roadmap | [`rencana-kerja/soap/soap.md`](../../../roadmap/rencana-kerja/soap/soap.md) bagian 7.7; didaftarkan di [`backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) |
| Trace | Keputusan pemilik K1 dan K2 (30-09-2026, `soap.md` bagian 8); cacat C4, C10, C11 (`soap.md` bagian 2.3); kontrak `soap.md` bagian 4.2 |
| Contract version | `0.6.1` ditambah delta aditif `soap.md` bagian 4.2, disetujui pemilik 30-09-2026. **Usulan:** naikkan ke `0.6.2` (lihat bagian 7) |
| Dependency | Tidak ada. Dipakai oleh `FE-RWI-139` dan `FE-RWI-141` |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1 (9–20), berkas diubah 1 (6 berkas), logika 1, kontrak API 2, database 1 (perilaku query), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/ClinicalManagement/**`, `Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `b342ae46` (branch `MHamzah`), perubahan belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ Selesai di tingkat source. `dotnet build` PASS. Verifikasi runtime endpoint **NOT RUN** |

---

## 1. Masalah yang diperbaiki

1. **SOAP rawat inap bisa dikunci tanpa diagnosa terkode.** Layar lama membuang kegagalan penyimpanan diagnosa, dan backend tidak memeriksa diagnosa saat catatan rawat inap diselesaikan (aturan lama `VAL-DOK-12` hanya menuntut satu bagian S/O/A/P terisi). Akibatnya resume medis, laporan morbiditas, dan klaim menerima catatan final tanpa kode ICD.
2. **Pencarian ICD-10 bercampur kode tindakan ICD-9.** `MstDiagnosis` menyimpan dua versi dalam satu tabel. Di DB pengembangan pribadi terdapat 18.543 baris ICD-10 dan 4.626 baris ICD-9. Tanpa penyaring versi, mengetik "99" di kolom diagnosa dapat menawarkan 168 kode tindakan ICD-9.
3. **Urutan hasil pencarian tidak relevan.** Hasil diurut per kode saja, sehingga "J18" tidak menjamin J18.0, J18.1, … berada di atas.
4. **Planning per diagnosa tidak bisa menampilkan nama obat atau tindakan.** Resolver rekomendasi hanya mengirim ID, padahal layar perlu menulis "Ceftriaxone 1 g …".
5. **Riwayat SOAP dan Catatan Dokter butuh satu request per kartu** untuk menampilkan diagnosa dan peran dokter (DPJP, Konsulen, Dokter Jaga).

---

## 2. Proses bisnis

**Pelaku:** dokter rawat inap (DPJP, konsulen, dokter jaga).

1. Dokter mengetik kode atau nama diagnosa. Backend hanya menawarkan diagnosa ICD-10 yang aktif dan dapat dipilih, dengan kode yang persis cocok di atas, lalu kode yang berawalan sama, lalu sisanya.
2. Dokter memilih diagnosa. Layar menyimpannya sebagai `TrxPatientDiagnosis` dengan `diagnosisId` dari master (bagian `FE-RWI-139`).
3. Layar meminta rekomendasi terapi untuk diagnosa terpilih. Resolver kini mengembalikan nama obat beserta kekuatan dan sediaannya, serta kode dan nama tindakan.
4. Dokter menekan **Selesaikan SOAP**. Pada catatan rawat inap (`InpEpisodeId` terisi) backend memeriksa:
   - Subjective, Objective, Assessment, dan Plan terisi;
   - minimal satu diagnosa aktif (tidak dibatalkan, tidak dihapus) pada catatan itu;
   - tepat satu Diagnosa Utama.
5. Bila ada yang kurang, backend menolak dengan kalimat yang sama dengan layar. Contoh tanpa diagnosa: *"Silakan pilih minimal satu diagnosa ICD-10 sebelum menyelesaikan SOAP."*
6. Riwayat SOAP memuat seluruh catatan episode dalam satu request, lengkap dengan daftar diagnosa per catatan dan peran penulis pada waktu catatan itu ditulis.

**Jalur tidak normal.**
- Membuat catatan rawat inap yang langsung selesai (`completeImmediately = true`) ditolak `400`: *"Catatan dokter rawat inap diselesaikan lewat Selesaikan SOAP setelah diagnosa ICD-10 dipilih."* Catatan yang lahir sudah selesai tidak pernah sempat diberi diagnosa, sehingga jalur itu satu-satunya celah melewati K1.
- Dokter jaga yang kemudian menjadi konsulen tetap terbaca "Dokter Jaga" pada catatan malamnya, karena peran diambil dari penugasan yang melingkupi waktu catatan. Bila tidak ada penugasan yang melingkupinya, penugasan terakhir dokter itu yang dipakai.
- SOAP poliklinik (tanpa `InpEpisodeId`) tetap memakai aturan lamanya dan tidak terkena gerbang ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/` (`TASK_RULES`, `API_RULES`, `DATABASE_RULES`, `TEST_POLICY`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `role-access-rules.md`, `transaction-endpoint-standard.md`); `PatientDiagnosisController.cs`; `DiagnosisRecommendationResolverController.cs` dan DTO-nya; `DoctorConsultationController.cs` dan `DoctorConsultationDtos.cs`; `ConsultationValidationService.cs`; `MstDiagnosis.cs`; `Seeders/Icd10DiagnosisSeeder.cs`; `InpDoctorAssignment` beserta enum `InpDoctorAssignmentRole`; `TrxPatientDiagnosis` beserta enum `PatientDiagnosisStatus`; `soap.md` Rev 2.1.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` | `GET master-options` menerima `icdVersion` (bawaan `ICD-10`; nilai kosong mematikan penyaring untuk pemanggil lama). Urutan: kode persis → awalan kode → sisanya, lalu per kode |
| `Areas/HealthServices/ClinicalManagement/DTOs/DiagnosisRecommendationResolverDtos.cs` | Rekomendasi obat: `DrugName`, `DrugStrength`, `DrugForm`. Rekomendasi tindakan: `ProcedureCode`, `ProcedureName` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DiagnosisRecommendationResolverController.cs` | Proyeksi membaca `MstDrug` dan `MstProcedure` lewat navigasi yang sudah ada |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationValidationService.cs` | Cabang rawat inap: `ValidateSoap` (empat bagian) + `ValidateInpatientDiagnosisAsync` (`MISSING_ICD10_DIAGNOSIS`, `MISSING_PRIMARY_DIAGNOSIS`, `MULTIPLE_PRIMARY_DIAGNOSIS`). Menggantikan `ValidateInpatientDailyNote` (`VAL-DOK-12`) untuk penyelesaian; aturan draf tidak berubah |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | `POST` menolak `completeImmediately` pada catatan rawat inap (400). `GET soap-timeline` dilengkapi `EnrichTimelineItemsAsync`: tiga kueri untuk seluruh lini masa (diagnosa per catatan, penugasan dokter episode, asal-usul tanda vital milik `BE-RWI-141`) |
| `Areas/HealthServices/ClinicalManagement/DTOs/DoctorConsultationDtos.cs` | `SoapTimelineItemResponse` bertambah kolom Plan terstruktur, TTV lengkap, `DiagnosisCount`, `HasPrimaryDiagnosis`, `Diagnoses[]`, `DoctorAssignmentRole`, `DoctorAssignmentRoleLabel`. Kelas baru `SoapTimelineDiagnosisResponse` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: satu query parameter baru, beberapa properti respons baru. Satu perilaku baru: penyelesaian catatan **rawat inap** tanpa diagnosa kini ditolak (K1, disetujui pemilik). Endpoint dan kode status lain tidak berubah |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema maupun entity. Hanya kueri baca |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada `[AccessAction]` atau `[AccessPermission]` yang berubah, dan tidak ada hardcode peran. Label peran DPJP/Konsulen/Dokter Jaga hanya untuk tampilan, bukan penentu kewenangan |

**Backend Governance Preflight.** Area `HealthServices`; modul pemilik `ClinicalManagement` (tabel `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `MstDiagnosis`) dan `PharmacyManagement` (`ConsultationValidationService`); sub-modul dokter-rawat-inap tidak memiliki tabel (`RWI-DEC-081`). Keberlakuan `TOUCHED LEGACY` (controller `Trx*` dengan `ApplicationDbContext` langsung, pola yang sudah ada). QBE yang berlaku: `QBE-SVC-001` (logika gerbang di service yang sudah ada), `QBE-API` respons `ApiResponse<T>`. Tidak ada pembangkitan nomor, jadi `QBE-CODE-003/006` tidak berlaku.

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-diagnoses/master-options?search=&take=&icdVersion=` | Cari ICD-10 untuk SOAP; `icdVersion` bawaan `ICD-10`, urutan relevansi | `PatientDiagnosis : Read` |

#### Health Services / Clinical Management / Diagnosis Recommendation Resolver

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/diagnosis-recommendations/resolve` | Rekomendasi aktif per diagnosa; kini menyertakan `drugName`, `drugStrength`, `drugForm`, `procedureCode`, `procedureName` | `DiagnosisRecommendationResolver : Read` |

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/doctor-consultations` | Membuat catatan; catatan rawat inap tidak boleh langsung selesai (400) | `DoctorConsultation : Create` |
| `PATCH` | `/api/v1/health-services/clinical-management/doctor-consultations/{id}/complete` | Menyelesaikan catatan; gerbang K1 untuk rawat inap. Penolakan membawa `data.sections[].issues[]` dengan kode dan kalimat di atas | `DoctorConsultation : Complete` |
| `GET` | `/api/v1/health-services/clinical-management/doctor-consultations/episodes/{episodeId}/soap-timeline` | Lini masa episode; kini dengan `diagnoses[]`, `doctorAssignmentRole(Label)`, kolom Plan terstruktur, dan TTV lengkap | `DoctorConsultation : Read` |

Contoh satu butir `diagnoses[]`:

```json
{ "id": "…", "diagnosisId": "…", "diagnosisCode": "J18.0", "diagnosisName": "Bronchopneumonia, unspecified", "isPrimary": true, "diagnosisType": 1 }
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj --no-incremental -c Debug -m:1 -p:UseSharedCompilation=false -p:RunAnalyzers=false` | Exit 0, **0 error**, 230 warning (garis dasar 222), **0 warning di berkas task ini**, 1 m 45 d | `PASS` | Keluaran build 30-09-2026. Delapan warning tambahan berasal dari berkas agen lain yang sedang dikerjakan (`NursingIntervention*`, `DailyNursingAction*`) |
| Nilai `IcdVersion` master cocok dengan penyaring | Seeder memakai `"ICD-10"`/`"ICD-9"`, dan bawaan entity `"ICD-10"`. DB pribadi: ICD-10 = 18.543, ICD-9 = 4.626 baris | `PASS` | `Seeders/Icd10DiagnosisSeeder.cs` baris 25–26; kueri baca-saja `dotnet fsi` + Npgsql ke `QuilvianNewDevHamzah` |
| SQL setara `master-options?search=j18` (tanpa penyaring aktif/dapat dipilih) | Urutan teratas: J18, J18.0, J18.1, J18.2, J18.8, J18.9 | `PASS` (tingkat data) | Kueri baca-saja yang sama |
| Kode ICD-9 yang tersaring | 168 kode ICD-9 memuat "99"; dengan bawaan `icdVersion=ICD-10` tidak ikut tertawar | `PASS` (tingkat data) | Kueri baca-saja yang sama |
| Kalimat penolakan K1 sama dengan layar | `MISSING_ICD10_DIAGNOSIS` = `PROGRESS_NOTE_TEXT.icdRequired`; `MISSING_PRIMARY_DIAGNOSIS` = `PROGRESS_NOTE_TEXT.primaryRequired` | `PASS` | Perbandingan source `ConsultationValidationService.cs` dan `inpatient-progress-note-constants.jsx` |
| Pemanggilan endpoint nyata (master-options, resolve, complete, soap-timeline) | — | `NOT RUN` | Backend tidak dijalankan pada sesi ini; runtime dijalankan pemilik (`soap.md` DoD butir 4) |

Uji manual: `NOT FEASIBLE` — backend dan akun dokter tidak dijalankan pada sesi ini.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** pemanggilan HTTP keempat endpoint dan uji penolakan `complete` pada data nyata. Alasannya: runtime dijalankan pemilik menurut DoD `soap.md` butir 4 dan keputusan pemilik 10-09-2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (`soap.md` 7.7) | Status | Bukti |
| --- | --- | --- |
| 1. Mencari "J18" menempatkan J18.0, J18.1, … di urutan teratas, dan kode ICD-9 tidak muncul | Terpenuhi di tingkat source dan data; runtime `NOT RUN` | `PatientDiagnosisController.GetMasterDiagnosisOptions` (penyaring `icdVersion`, `OrderBy` relevansi); SQL setara di bagian 5 |
| 2. Resolve mengembalikan `drugName` | Terpenuhi di tingkat source; runtime `NOT RUN` | DTO `DrugName` + proyeksi `x.Drug.DrugName` |
| 3. Menyelesaikan SOAP rawat inap tanpa diagnosa menghasilkan 400 dengan pesan yang sama dengan layar | Terpenuhi di tingkat source; runtime `NOT RUN` | `ConsultationValidationService.ValidateInpatientDiagnosisAsync`; jalur `completeImmediately` rawat inap ditutup |
| 4. SOAP poli tidak terkena gerbang ini | Terpenuhi | Gerbang hanya di cabang `consultation.InpEpisodeId.HasValue`; cabang poliklinik tidak diubah |
| DoD 3 — `dotnet build` 0 error | Terpenuhi | Bagian 5 |
| DoD 4 — build/runtime oleh pemilik; bila tidak dijalankan ditulis `NOT RUN` | Terpenuhi | Bagian 5 |
| DoD 7 — laporan, roadmap, dan status `soap.md` diperbarui | Terpenuhi | Laporan ini; `backend-roadmap-v2.md`; `requirement-traceability-v2.md` bagian 17; `soap.md` catatan status 30-09-2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru pada berkas task ini |
| Masalah yang diketahui | Kalimat penolakan S/O/A/P backend ("Subjective atau keluhan utama wajib diisi.") tidak sama persis dengan layar ("Subjective (keluhan pasien) belum diisi."). Kriteria 3 hanya menuntut kalimat ICD yang sama, dan itu sudah sama |
| Risiko tersisa | Klien lain yang membuat catatan rawat inap dengan `completeImmediately = true` kini menerima 400. Grep frontend V2 30-09-2026: `completeImmediately` hanya dipakai kajian perawat antrean poliklinik (`use-doctor-queue.js`), kajian IGD, instrumen klinis, dan catatan resep poliklinik (bernilai `false`). Tidak satu pun membuat catatan dokter rawat inap |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi terpotong ringkasan konteks; dilanjutkan dari kondisi terverifikasi tanpa penyuntingan ganda |
| Status Git | Lihat laporan `BE-RWI-141` bagian 7. Berkas task ini: `PatientDiagnosisController.cs`, `DiagnosisRecommendationResolverController.cs`, `DiagnosisRecommendationResolverDtos.cs`, `ConsultationValidationService.cs`, `DoctorConsultationController.cs`, `DoctorConsultationDtos.cs` (dua berkas terakhir dipakai bersama `BE-RWI-141`) |
| Langkah berikutnya | Pemilik menjalankan backend dan memanggil keempat endpoint di atas. Pemilik memutuskan kenaikan `contract_version` `0.6.1` → `0.6.2` untuk delta aditif ini |
