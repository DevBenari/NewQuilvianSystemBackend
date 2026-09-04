# Laporan Perubahan Backend — `BE-RWI-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-044` |
| Judul | Dokter membuka pasien rawat inap dan menulis tanpa nomor antrean |
| Slice | `DOK-MVP-2` — pintu masuk dan kajian medis |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-044` |
| Trace | `FR-DOK-001`, `FR-DOK-038`; `EPIC DOK-01`; `contracts/api-contract.md` §1 dan §2; `contracts/permission-audit-matrix.md` §1.1; `VAL-DOK-01` s.d. `VAL-DOK-04`, `VAL-DOK-26` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-039` **selesai** ([laporan](BE-RWI-039.md)); `BE-RWI-040` 🟡 sebagian ([laporan](BE-RWI-040.md)); `BE-RWI-043` 🟡 sebagian ([laporan](BE-RWI-043.md)) |
| Klasifikasi | `HEAVY`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 2, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b0c1b956ae9ce221121e056b789024bdc836f1a7` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | ✅ **Selesai.** Kelima acceptance criteria terbukti. Nol migration dan nol perubahan bentuk data. Satu selisih kontrak dilaporkan: kalimat penolakan `VAL-DOK-04` diperbarui, kodenya tetap `400` |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY` |
| Applicability | `TOUCHED LEGACY` — kedua controller adalah kode lama; perubahan dibatasi pada jalur pembuatan yang disebut task |
| QBE berlaku | `QBE-VAL-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-TXN-001` |
| Entity operasional baru | `NONE`. Task ini tidak membuat satu model persisted pun, sehingga `QBE-MOD-002` dan `QBE-NAM-004` tidak berlaku |
| Utang teknis yang sengaja tidak dirapikan | Kedua controller menaruh logika bisnis di dalam controller, berlawanan dengan `QBE-SVC-001`. Utang milik modul lain; dicatat, tidak dikerjakan |
| Archetype | Transaksi. Nol endpoint baru pada task ini; yang berubah adalah aturan penerimaan pada `POST /` yang sudah ada |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, dokter **tidak punya cara sah** untuk menuliskan catatan bagi pasien yang sedang
menginap.

Alasannya berlapis. Pembuatan catatan dokter mengenal dua jalur: lewat baris antrean, atau tanpa
antrean. Jalur berantre tertutup bagi pasien rawat inap karena pasien menginap memang tidak pernah
mengambil nomor antrean — dan membuatkan antrean semu dilarang `RWI-RULE-026` aturan 2, sebab
antrean semu akan muncul pada layar antrean poliklinik dan ikut terhitung pada laporan kunjungan.
Jalur tanpa antrean pun tertutup, karena penjagaannya berbunyi *"Konsultasi tanpa antrean hanya
untuk pasien IGD"*.

Akibat nyatanya: **Tn. Budi yang dirawat sepuluh hari tidak dapat menerima satu catatan dokter pun
lewat sistem.** Dokumentasinya berpindah ke kertas, atau ditempelkan pada kunjungan lain yang
kebetulan punya antrean.

Dua task sebelumnya sudah menyiapkan lantainya tetapi tidak dapat membuktikan hasilnya karena pintu
ini masih tertutup:

| Task | Yang sudah disiapkan | Yang tertahan |
| --- | --- | --- |
| `BE-RWI-039` | Service konteks klinis yang menjawab "dokumen ini milik perawatan yang mana", lengkap dengan penolakan penanda yang tidak cocok | Belum terpasang pada satu jalur pembuatan pun |
| `BE-RWI-043` | Batas satu catatan per kunjungan dilonggarkan pada aturan aplikasi **dan** pada index database | Catatan kedua hanya dapat lahir lewat cabang tanpa antrean, dan cabang itu masih tertutup |

---

## 2. Proses bisnis

### 2.1 Alur normal — dokter menulis untuk pasien menginap

1. Dokter membuka pasien dari **daftar pasien rawat inap**, bukan dari antrean. Yang ia pegang
   adalah penanda kunjungan, dan boleh juga penanda perawatan.
2. Dokter menekan "buat catatan". Permintaan dikirim **tanpa** nomor antrean.
3. Backend memeriksa berurutan:
   1. **Penanda perawatan** — bila dikirim, ia wajib cocok dengan perawatan milik kunjungan itu.
   2. **Pintu masuk** — kunjungan IGD dilewatkan apa adanya; selain itu kunjungan wajib menaungi
      perawatan rawat inap yang **berjalan**.
   3. **Dokter pemeriksa** — wajib disebut dan wajib ada pada data dokter.
   4. **Batas jumlah catatan** — tidak berlaku bagi catatan yang menempel pada perawatan berjalan.
4. Catatan tersimpan, **distempel penanda perawatannya**, dan kunjungan berpindah keadaan menjadi
   sedang dikonsultasikan.
5. Besoknya dokter menulis catatan berikutnya lewat jalur yang sama. Catatan kedua, ketiga, dan
   seterusnya diterima.

### 2.2 Jalur tidak normal

| Keadaan | Jawaban backend | Kode | Aturan |
| --- | --- | --- | --- |
| Kunjungan poliklinik atau medical check-up tanpa antrean | "Konsultasi untuk pasien poliklinik tetap harus lewat antrean." | `400` | `VAL-DOK-04` |
| Penanda perawatan terisi tetapi milik pasien lain | "Perawatan rawat inap tidak sesuai dengan kunjungannya." | `400` | `VAL-DOK-26` |
| Penanda perawatan dikirim untuk kunjungan yang tidak punya perawatan | Kalimat yang sama seperti di atas | `400` | `VAL-DOK-26` |
| Perawatan masih `Draft` — pasien belum dikonfirmasi tiba di kamar | "Perawatan rawat inap belum dimulai; pasien belum masuk kamar." | `422` | `VAL-DOK-02` |
| Perawatan sudah `Closed` atau `Cancelled` | "Perawatan rawat inap sudah ditutup; dokumen baru tidak dapat dibuat. Gunakan koreksi untuk membetulkan dokumen yang sudah ada." | `422` | `VAL-DOK-03` |
| Dokter pemeriksa tidak disebut pada jalur tanpa antrean | "Dokter pemeriksa wajib diisi untuk konsultasi tanpa antrean." | `400` | Penjagaan lama `BE-IGD-028` |

**Perawatan yang menunggu pemulangan tetap menerima catatan.** Pasien masih berada di kamar sampai
ia benar-benar meninggalkan rumah sakit, dan dokumentasi pada masa itu tetap sah.

### 2.3 Kenapa kunjungan IGD diperiksa lebih dulu

Urutan pemeriksaan pada pintu masuk **disengaja**: keberadaan kunjungan IGD diperiksa sebelum
konteks perawatan dibentuk, dan kunjungan IGD dilewatkan apa adanya.

Alasannya nyata. Pasien IGD yang sedang dalam proses admisi sudah memiliki baris perawatan
berstatus `Draft` sebelum ia benar-benar masuk kamar. Bila keadaan perawatan dinilai lebih dulu,
pencatatan IGD pada pasien itu akan ditolak `422` — menutup jalur yang hari ini berjalan dan tidak
diminta berubah oleh task mana pun. Perilaku itu dikunci oleh test
`IgdDenganPerawatanMasihDraft_TetapBerhasil`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria, dependency, dan DoD task |
| `contracts/api-contract.md` §1, §2 | Bentuk permintaan dan kode status yang mengikat |
| `contracts/validation-matrix.md` | Bunyi kalimat `VAL-DOK-01` s.d. `VAL-DOK-04` dan `VAL-DOK-26` |
| `contracts/permission-audit-matrix.md` | Butir hak akses dan batas yang tidak dijaga mesin hak akses |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Jalur pembuatan catatan beserta penjagaannya |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Jalur pembuatan pengkajian beserta penjagaannya |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Bentuk konteks klinis dari `BE-RWI-039` |
| `Seeders/AccessMenuSeeder.cs` | Cara butir hak akses dibentuk dari atribut |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/InPatientManagement/InpatientRoleAccessContractTests.cs` | Pola uji hak akses non-SuperAdmin dari `BE-RWI-034` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/DoctorConsultationDtos.cs` | `CreateDoctorConsultationRequest` menerima `InpEpisodeId` |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | `CreatePatientAssessmentRequest` menerima `InpEpisodeId`; response membawa `InpEpisodeId` |
| `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` | Pintu masuk tanpa antrean dibuka bagi perawatan berjalan; penjagaan penanda perawatan dipasang pada kedua cabang; penolakan membawa kode HTTP-nya sendiri lewat `CreateGuard` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Perlakuan yang sama untuk pengkajian; konteks perawatan distempel saat dokumen lahir |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Konteks membawa `AdmittedAt`, dipakai `BE-RWI-046` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientDoctorEntryPointTests.cs` | **Baru.** 16 test acceptance dan regresi |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/ClinicalManagement/ClinicalRoleAccessContractTests.cs` | **Baru.** 19 test hak akses non-SuperAdmin dan kesesuaian nama penanda |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/RawatInapTestData.cs` | Akun dokter uji ditautkan ke profil tenaga kerja miliknya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/DoctorConsultationInpatientPathTests.cs` | Uji `BE-RWI-037` kriteria 5 memakai kalimat `VAL-DOK-04` yang baru; kode `400` dan jumlah catatan yang diuji tidak berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `POST /doctor-consultations` dan `POST /patient-assessments` menerima satu field opsional baru, `InpEpisodeId`. Nol field dihapus, nol field berganti nama. Dua kode status baru muncul pada jalur rawat inap: `422` untuk perawatan yang belum dimulai atau sudah ditutup — sesuai `api-contract.md` §1.2 |
| Database | `NOT APPLICABLE`. Nol perubahan model, nol konfigurasi EF baru, **nol migration**. Kolom `InpEpisodeId` yang dipakai sudah dibuat `BE-RWI-040` |
| Keamanan/Auth | Butir hak akses **tidak bertambah**: kedua endpoint memakai `DoctorConsultation : Create` dan `PatientAssessment : Create` yang sudah ada. `AccessMenuSeeder` membentuk butirnya dari atribut secara refleksi, sehingga tidak ada pendaftaran manual yang perlu ditambahkan. Nol pemeriksaan berbasis nama peran diperkenalkan |

### 3.4 Selisih terhadap kontrak yang dilaporkan

| Hal | Isinya |
| --- | --- |
| **Kalimat `VAL-DOK-04` diperbarui** | Sebelumnya *"Konsultasi tanpa antrean hanya untuk pasien IGD. Untuk pasien poli, buat konsultasi dari baris antreannya."* Sejak pintu rawat inap dibuka, kalimat itu **berhenti benar**: ada dua jalur sah tanpa antrean. Penggantinya adalah bunyi `VAL-DOK-04` apa adanya, *"Konsultasi untuk pasien poliklinik tetap harus lewat antrean."* **Kode penolakannya tetap `400`** dan jalur poliklinik tetap tertutup persis seperti sebelumnya. Kalimat pengkajian disamakan bentuknya |
| **`VAL-DOK-06` tidak dikerjakan** | Kewenangan "dokter hanya menulis untuk pasien yang menjadi tanggung jawabnya" **belum** ditegakkan pada jalur ini. Service konteks sudah mampu memeriksanya, tetapi menyalakannya berarti menolak dokter konsulen dan dokter jaga yang bukan DPJP — kebijakan yang tidak disebut satu pun acceptance criteria task ini. Dicatat sebagai risiko tersisa |
| **`ClinicalDateTime` dan `PhysicianVisitId`** | `api-contract.md` §1 menyebut `POST /` menerima tiga field. `ClinicalDateTime` dikerjakan `BE-RWI-046`; `PhysicianVisitId` menunggu tabel visite dinyalakan `BE-RWI-048` |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter. **Kini menerima** `InpEpisodeId`, dan menerima permintaan tanpa nomor antrean bagi pasien yang perawatan rawat inapnya berjalan | `DoctorConsultation : Create` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. **Kini menerima** `InpEpisodeId`, dan menerima permintaan tanpa nomor antrean bagi pasien rawat inap. Konteks perawatan distempel saat dokumen lahir | `PatientAssessment : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `185 Warning(s)` — seluruhnya peringatan dokumentasi XML yang sudah ada sebelum task ini |
| `dotnet test` project uji SQLite, seluruhnya | Berhasil | `PASS` | `Failed: 0, Passed: 262, Skipped: 0, Total: 262` |
| `dotnet test` project uji InMemory, seluruhnya | Satu gagal, di luar cakupan | `EXISTING / ENVIRONMENT ISSUE` | `Failed: 1, Passed: 908, Total: 909`. Kegagalannya `BillingFinalizationServiceTests` milik `BillingManagement` — lihat bagian 7 |
| Catatan tanpa antrean dan tanpa IGD tersimpan pada perawatan berjalan | Tersimpan `200`, `InpEpisodeId` terisi, nol baris antrean lahir | `PASS` | `InpatientDoctorEntryPointTests.RawatInapTanpaAntreanDanTanpaIgd_CatatanTersimpan` |
| Pengkajian tanpa antrean tersimpan beserta konteks perawatannya | Tersimpan `200`, `InpEpisodeId` terisi | `PASS` | `...RawatInapTanpaAntrean_PengkajianTersimpanBesertaKonteksnya` |
| **Catatan kedua diterima lewat endpoint** | Keduanya `200`, dua baris tersimpan | `PASS` | `...RawatInap_CatatanKeduaDiterimaLewatEndpoint` |
| Penanda perawatan milik pasien lain | Ditolak `400`, nol catatan tersimpan | `PASS` | `...PenandaPerawatanMilikPasienLain_Ditolak400` |
| Penanda perawatan yang cocok | Diterima `200` | `PASS` | `...PenandaPerawatanYangCocok_Diterima` |
| Penanda tidak cocok pada jalur **berantre** | Ditolak `400` | `PASS` | `...PenandaTidakCocokPadaJalurBerantre_Ditolak400` |
| Penanda perawatan pada kunjungan tanpa perawatan | Ditolak `400` | `PASS` | `...PenandaPerawatanPadaKunjunganTanpaPerawatan_Ditolak400` |
| Pengkajian keperawatan belum selesai | Catatan tetap dibuat `200` | `PASS` | `...PengkajianKeperawatanBelumSelesai_CatatanTetapDapatDibuat` |
| Perawatan masih `Draft` | Ditolak `422` beserta kalimat `VAL-DOK-02` | `PASS` | `...PerawatanBelumDimulai_Ditolak422` |
| Perawatan sudah ditutup | Ditolak `422` beserta kalimat `VAL-DOK-03` | `PASS` | `...PerawatanSudahDitutup_Ditolak422` |
| Perawatan menunggu pemulangan | Diterima `200` | `PASS` | `...PerawatanMenungguPemulangan_CatatanTetapDapatDibuat` |
| **Regresi poliklinik dan medical check-up** tanpa antrean | Ditolak `400`, nol catatan lahir | `PASS` | `...PoliklinikDanMedicalCheckupTanpaAntrean_TetapDitolak400`, dua theory |
| **Regresi IGD** tanpa antrean | Diterima `200`, nol baris antrean lahir | `PASS` | `...IgdTanpaAntrean_TetapBerhasil` |
| **Regresi IGD** dengan perawatan masih `Draft` | Diterima `200` | `PASS` | `...IgdDenganPerawatanMasihDraft_TetapBerhasil` |
| **Regresi poliklinik** jalur berantre | Diterima `200`, antrean berpindah keadaan | `PASS` | `...JalurBerantre_TetapBerhasil` |
| Nama pada `[AccessAction]` dan `[AccessPermission]` sama persis | Nol penyimpangan pada kedua controller | `PASS` | `ClinicalRoleAccessContractTests.SetiapEndpoint_MemakaiNamaYangSamaPadaKeduaPenanda` |
| Setiap pasangan hak akses terbentuk sebagai baris yang dapat dicentang | Nol pasangan yang tidak terdaftar | `PASS` | `...SetiapPasangan_AdaSebagaiBarisYangDapatDicentang` |
| **Setiap pasangan dapat diberikan kepada peran non-SuperAdmin** | Seluruhnya lolos setelah kebijakan diberikan | `PASS` | `...SetiapPasangan_DapatDiberikanKepadaPeranNonSuperAdmin`, satu theory per pasangan |
| Kendali negatif: tanpa kebijakan, pasangan yang sama ditolak | Seluruhnya ditolak | `PASS` | `...SetiapPasangan_TetapDitolakBilaBelumDiberikan` |
| Butir baca tidak ikut memberi kemampuan menulis | `Read` diberikan, `Create` dan `Update` tetap ditolak | `PASS` | `...ButirBaca_TidakIkutMemberiKemampuanMenulis` |

Uji manual: `NOT FEASIBLE` — tidak ada lingkungan runtime beserta database yang tersedia pada sesi
ini; seluruh bukti diambil dari uji otomatis yang benar-benar dijalankan.

**Tidak dijalankan:**

- Uji terhadap PostgreSQL. Tidak ada database uji yang tersedia, dan task ini tidak mengubah
  schema sehingga tidak menambah utang verifikasi migration yang sudah tercatat pada `BE-RWI-040`
  s.d. `BE-RWI-043`.
- Eksekusi migration dan deployment. Keduanya wewenang terpisah dan tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Catatan dokter dapat dibuat untuk perawatan berjalan tanpa antrean dan tanpa kunjungan IGD | Terpenuhi | `RawatInapTanpaAntreanDanTanpaIgd_CatatanTersimpan` — uji membuktikan lebih dulu bahwa kunjungannya memang **tidak** punya baris IGD |
| 2. Penanda perawatan yang terisi tetapi tidak cocok dengan kunjungannya ditolak `400` | Terpenuhi | `PenandaPerawatanMilikPasienLain_Ditolak400`, `PenandaTidakCocokPadaJalurBerantre_Ditolak400`, `PenandaPerawatanPadaKunjunganTanpaPerawatan_Ditolak400`, beserta kendali positifnya |
| 3. Catatan dapat dibuat walaupun pengkajian awal keperawatan belum selesai | Terpenuhi | `PengkajianKeperawatanBelumSelesai_CatatanTetapDapatDibuat` |
| 4. Seluruh endpoint yang tersentuh dapat dipanggil peran non-SuperAdmin yang berhak | Terpenuhi | `SetiapPasangan_DapatDiberikanKepadaPeranNonSuperAdmin` beserta kendali negatifnya, memakai `UserType.Employee` tanpa peran SuperAdmin |
| 5. Nama pada penanda aksi dan penanda hak akses sama persis | Terpenuhi | `SetiapEndpoint_MemakaiNamaYangSamaPadaKeduaPenanda` — lihat tabel di bawah |

### 6.1 Pasangan nama penanda, apa adanya

Diminta DoD task: *"laporan mencantumkan pasangan nama penanda apa adanya"*.

| Controller | Endpoint | `[AccessController].ControllerName` | `[AccessAction]` argumen ke-1 | `[AccessPermission]` argumen ke-1 dan ke-2 |
| --- | --- | --- | --- | --- |
| `DoctorConsultationController` | `GET /filters/metadata` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `GET /` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `GET /{id}` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `GET /active-by-queue/{queueId}` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `GET /episodes/{episodeId}/soap-timeline` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `GET /{id}/finalization-validation` | `DoctorConsultation` | `Read` | `DoctorConsultation`, `Read` |
| `DoctorConsultationController` | `POST /` | `DoctorConsultation` | `Create` | `DoctorConsultation`, `Create` |
| `DoctorConsultationController` | `PUT /{id}` | `DoctorConsultation` | `Update` | `DoctorConsultation`, `Update` |
| `DoctorConsultationController` | `PATCH /{id}/soap` | `DoctorConsultation` | `Update` | `DoctorConsultation`, `Update` |
| `DoctorConsultationController` | `PATCH /{id}/complete` | `DoctorConsultation` | `Update` | `DoctorConsultation`, `Update` |
| `DoctorConsultationController` | `PATCH /{id}/cancel` | `DoctorConsultation` | `Update` | `DoctorConsultation`, `Update` |
| `PatientAssessmentController` | `GET /` | `PatientAssessment` | `Read` | `PatientAssessment`, `Read` |
| `PatientAssessmentController` | `GET /{id}` | `PatientAssessment` | `Read` | `PatientAssessment`, `Read` |
| `PatientAssessmentController` | `GET /active-by-encounter/{encounterId}` | `PatientAssessment` | `Read` | `PatientAssessment`, `Read` |
| `PatientAssessmentController` | `GET /active-by-queue/{queueId}` | `PatientAssessment` | `Read` | `PatientAssessment`, `Read` |
| `PatientAssessmentController` | `GET /episodes/{episodeId}` | `PatientAssessment` | `Read` | `PatientAssessment`, `Read` |
| `PatientAssessmentController` | `POST /` | `PatientAssessment` | `Create` | `PatientAssessment`, `Create` |
| `PatientAssessmentController` | `PUT /{id}` | `PatientAssessment` | `Update` | `PatientAssessment`, `Update` |
| `PatientAssessmentController` | `PATCH /{id}/complete` | `PatientAssessment` | `Update` | `PatientAssessment`, `Update` |
| `PatientAssessmentController` | `PATCH /{id}/cancel` | `PatientAssessment` | `Update` | `PatientAssessment`, `Update` |

Nol penyimpangan. Enam butir hak akses yang dipakai — `DoctorConsultation : Read`, `Create`,
`Update`, dan `PatientAssessment : Read`, `Create`, `Update` — **seluruhnya sudah ada** sebelum
task ini; tidak ada butir baru yang perlu didaftarkan.

### 6.2 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Kelima acceptance criteria terbukti | Terpenuhi |
| Test peran non-SuperAdmin hijau | Terpenuhi — 19 test pada `ClinicalRoleAccessContractTests` |
| Laporan mencantumkan pasangan nama penanda apa adanya | Terpenuhi — bagian 6.1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `185 Warning(s)` pada build, seluruhnya peringatan dokumentasi XML yang sudah ada sebelum task ini. Nol peringatan baru |
| Masalah yang diketahui | **Satu:** `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` gagal pada project uji InMemory, mengharapkan status `FINAL` tetapi menerima `CLOSED`. Ini **bukan** akibat task ini: commit `ee483e4` tanggal 1 September 2026 mengubah `BillingFinalizationService` agar tagihan yang sudah lunas berstatus `CLOSED`, dan test-nya tidak ikut diperbarui. Milik `BillingManagement`; dilaporkan, tidak diperbaiki |
| Risiko tersisa | **Pertama, `VAL-DOK-06` belum ditegakkan.** Dokter mana pun yang memegang butir `DoctorConsultation : Create` dapat menulis untuk pasien rawat inap mana pun. Kewenangan per pasien belum dijaga di jalur ini. **Kedua, penyelesaian catatan memindahkan keadaan kunjungan** menjadi `ConsultationCompleted` pada setiap catatan harian yang difinalkan; pada perawatan panjang keadaan itu berpindah bolak-balik. Perilakunya sudah ada sebelum task ini dan tidak diubah, tetapi dampaknya baru terasa setelah catatan harian mungkin dibuat |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Branch `MHamzah`, upstream `origin/MHamzah`. **Yang diubah ketiga task ini:** `M` pada enam berkas source — `DoctorConsultationController.cs`, `PatientAssessmentController.cs`, `DoctorConsultationDtos.cs`, `PatientAssessmentDtos.cs`, `InpatientClinicalContextService.cs`, dan `ConsultationValidationService.cs`; `M` pada dua berkas uji — `DoctorConsultationInpatientPathTests.cs` dan `RawatInapTestData.cs`; `M` pada `roadmap/backend-roadmap.md` dan `roadmap/requirement-traceability.md` sub-modul ini; `??` pada empat berkas uji baru dan tiga laporan task ini. **Yang bukan milik ketiga task ini dan tidak disentuh:** seluruh perubahan pada `docs/module-blueprints/rawat-inap/episode-rawat-inap/**`, `docs/module-blueprints/rawat-inap/00-interview-decisions.md`, `docs/module-blueprints/rawat-inap/blueprint-manifest.md`, dan `docs/Modul-RS/**` — seluruhnya pekerjaan pengguna yang berjalan bersamaan pada sesi ini. Nol operasi Git dijalankan |
| Langkah berikutnya | `BE-RWI-045` dan `BE-RWI-046` dikerjakan pada sesi yang sama. Yang lepas berikutnya: `BE-RWI-048` setelah `BE-RWI-041`, lalu `BE-RWI-050` dan `BE-RWI-052`. Utang verifikasi PostgreSQL milik `BE-RWI-040` s.d. `BE-RWI-043` masih terbuka dan tidak bertambah oleh task ini |
