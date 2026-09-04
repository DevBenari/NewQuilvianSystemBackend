# Laporan Perubahan Backend — `BE-RWI-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-045` |
| Judul | Kajian medis awal tersimpan terpisah dari catatan harian |
| Slice | `DOK-MVP-2` — pintu masuk dan kajian medis |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-045` |
| Trace | `EPIC DOK-02`; `FR-DOK-006` s.d. `FR-DOK-011`; `AC-CAP022-02`; `02-backend-architecture.md` §4.2; `contracts/api-contract.md` §2; `VAL-DOK-01`, `VAL-DOK-05`, `VAL-DOK-10`, `VAL-DOK-11`; `RWI-AC-157` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-044` **selesai** ([laporan](BE-RWI-044.md)) |
| Klasifikasi | `HEAVY`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 2, kontrak API 2, database 0, keamanan/auth 2, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b0c1b956ae9ce221121e056b789024bdc836f1a7` pada branch `MHamzah` |
| Tanggal | 4 September 2026 |
| Status | 🟡 **Sebagian.** Lima dari enam acceptance criteria terbukti. Kriteria 4 **belum terpenuhi seluruhnya**: mekanismenya berjalan dan daftar bagian kosong benar-benar dikembalikan, tetapi **diagnosis, pemeriksaan fisik, dan rencana terapi tidak punya kolom** pada `TrxPatientAssessment` — dan kamus data yang disetujui menyatakan sub-modul ini menambahkan **nol** kolom pada tabel itu. Blocker dirinci pada bagian 6.1 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement`, membaca `MedicalRecordManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli` — `ACTIVE / LEGACY`; `MedicalRecordManagement / Mrc` — `ACTIVE` |
| Applicability | `TOUCHED LEGACY` — `PatientAssessmentController` adalah kode lama; perubahan dibatasi pada jenis kajian medis |
| QBE berlaku | `QBE-VAL-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-TXN-001` |
| Entity operasional baru | `NONE`. Nol model persisted dibuat; `QBE-MOD-002` dan `QBE-NAM-004` tidak berlaku |
| Utang teknis yang sengaja tidak dirapikan | Controller memuat logika bisnis, berlawanan dengan `QBE-SVC-001`. Utang milik modul lain; dicatat, tidak dikerjakan |
| Archetype | Transaksi. Satu endpoint baca baru per arketipe sub-proses ter-scope induk: `GET /episodes/{episodeId}`. Nol `GET /options`, nol `PATCH /{id}/status` generik, nol `DELETE /{id}` |
| Database authority | `NOT APPLICABLE`. Nol perubahan model, nol migration, nol eksekusi database |
| Frontend | Diperiksa read-only. Tidak ada berkas frontend yang diubah |

---

## 1. Masalah yang diperbaiki

**Pemeriksaan menyeluruh pertama oleh DPJP dan catatan perkembangan harian adalah dua dokumen yang
berbeda, tetapi sistem hanya mengenal satu.**

Sebelum task ini, `TrxPatientAssessment` hanya mengenal pengkajian keperawatan. Tidak ada cara
menandai bahwa sebuah baris adalah **kajian medis** milik dokter, sehingga:

- pemeriksaan awal DPJP tidak punya tempat tersendiri, dan
- penjagaan "satu draf pengkajian per kunjungan" membuat draf perawat **menutup** pembuatan
  dokumen dokter, dan sebaliknya.

Contoh nyatanya: Ny. Sari masuk pukul 10.40. Perawat mulai mengisi pengkajian keperawatan dan
menyimpannya sebagai konsep. Pukul 11.15 DPJP hendak menuliskan kajian medisnya — dan permintaannya
ditolak dengan alasan "draf pengkajian untuk kunjungan ini sudah ada", padahal draf itu milik
profesi lain untuk dokumen yang sama sekali berbeda.

`BE-RWI-040` sudah menyiapkan penandanya — nilai enum `MedicalInitial` dan `MedicalReassessment`
beserta kolom `AssessmentType`. Yang belum ada adalah jalur yang memakainya.

---

## 2. Proses bisnis

### 2.1 Alur normal — DPJP mengisi kajian medis awal

1. DPJP membuka pasien dari daftar pasien rawat inap.
2. DPJP memilih "kajian medis awal". Permintaan dikirim dengan jenis `MedicalInitial`.
3. Backend memeriksa berurutan:
   1. **Penanda perawatan** — bila dikirim, wajib cocok dengan perawatan milik kunjungan.
   2. **Kewenangan menulis** — pengguna yang sedang masuk wajib terhubung ke satu baris dokter
      yang aktif.
   3. **Konteks perawatan** — kajian medis hanya lahir di atas perawatan rawat inap yang berjalan.
   4. **Batas satu kajian medis awal** — satu perawatan hanya boleh punya satu yang berlaku.
4. Kajian tersimpan sebagai record tersendiri, bertanda jenis kajian medis, membawa penanda
   perawatan, dan **dokternya diambil dari pengguna yang masuk** — bukan dari antrean maupun
   kunjungan.
5. Setelah isinya lengkap, DPJP menyelesaikan kajian. Pada saat itu juga kajian **didaftarkan ke
   mesin keutuhan rekam medis**, dalam penyimpanan yang sama.
6. Hari-hari berikutnya DPJP menulis catatan harian lewat `POST /doctor-consultations`. Catatan itu
   hidup di tabel yang berbeda dengan mesin status yang berbeda, sehingga **tidak pernah menimpa**
   isi kajian medis.

### 2.2 Kenapa kajian medis dan pengkajian keperawatan berbagi satu tabel

Ini keputusan struktur pada `02-backend-architecture.md` §4.2 — jalan A. `TrxPatientAssessment`
sudah memuat keluhan utama, riwayat, alergi, tanda vital, kesadaran, dan pemeriksaan umum, dan
**sudah punya kolom `DoctorId`**: ia memang tidak pernah menjadi tabel milik perawat saja. Membuat
tabel tersendiri berarti menyalin puluhan kolom yang sama.

Harganya dibayar di sini: **mesin hak akses hanya melihat satu sumber daya untuk dua jenis
dokumen**. Karena itu pembedaannya dijaga aturan bisnis, bukan hak akses —
`permission-audit-matrix.md` bagian 3 baris kedua.

### 2.3 Jalur tidak normal

| Keadaan | Jawaban backend | Kode | Aturan |
| --- | --- | --- | --- |
| Pengguna tidak terhubung ke data dokter mana pun | "Catatan ini hanya dapat ditulis dokter." | `403` | `VAL-DOK-05` |
| Kunjungan tidak menaungi perawatan rawat inap | "Pasien ini tidak sedang dirawat inap." | `422` | `VAL-DOK-01` |
| Perawatan masih `Draft` atau sudah ditutup | Kalimat `VAL-DOK-02` atau `VAL-DOK-03` | `422` | `VAL-DOK-02`, `VAL-DOK-03` |
| Perawatan sudah punya kajian medis awal yang berlaku | "Perawatan ini sudah memiliki kajian medis awal. Lanjutkan kajian yang sudah ada, atau buat kajian medis ulang." | `400` | Acceptance criteria 3 |
| Kajian medis diselesaikan padahal bagiannya masih kosong | "Kajian medis belum dapat diselesaikan. Bagian berikut masih kosong: keluhan utama, riwayat penyakit sekarang." | `400` | `VAL-DOK-10` |

**Kajian yang dibatalkan tidak dihitung** pada batas satu-per-perawatan. Pembatalan memang jalan
keluar dari kajian yang salah, dan menghitungnya akan mengunci perawatan itu selamanya.

### 2.4 Kewenangan diturunkan dari data, bukan dari nama peran

Penolakan `403` pada baris pertama tabel di atas **tidak** membaca nama peran, nama jabatan, nama
departemen, maupun `UserType`. Yang diperiksa adalah apakah pengguna yang sedang masuk benar-benar
terhubung ke satu baris dokter yang aktif, lewat tiga jalur berurutan yang seluruhnya bersandar
pada data:

1. klaim identitas dokter pada token, bila ada;
2. penautan lewat profil tenaga kerja — `ApplicationUser.WorkforceProfileId` dipasangkan dengan
   `MstDoctor.WorkforceProfileId`;
3. penautan lewat surel.

Urutannya mengikuti pola yang sudah dipakai `DoctorQueueController.ResolveAllowedDoctorIdAsync`,
sehingga tidak ada pola kewenangan kedua yang diperkenalkan. **Siapa yang boleh memanggil endpoint
ini tetap ditentukan admin lewat layar Pengaturan → Manajemen Role → Akses Role**; yang dijaga di
sini adalah kewenangan yang melekat pada data.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `roadmap/backend-roadmap.md` | Acceptance criteria, dependency, dan DoD task |
| `02-backend-architecture.md` §4.2 | Keputusan struktur berbagi tabel beserta keberatan yang sudah dicatat |
| `data/data-dictionary.md` §3 | Kolom yang boleh dan tidak boleh ditambahkan pada tabel pengkajian |
| `contracts/api-contract.md` §2 | Bentuk endpoint kajian dan pendaftaran keutuhan |
| `contracts/validation-matrix.md` | Bunyi `VAL-DOK-05`, `VAL-DOK-10`, `VAL-DOK-11` |
| `contracts/permission-audit-matrix.md` §3 | Batas yang tidak dapat dijaga mesin hak akses |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | Kolom yang benar-benar tersedia |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs` | Apakah diagnosis dapat digantung pada kajian |
| `Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` | Cara mendaftarkan dokumen ke mesin keutuhan |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` | Pola pemanggilan `RegisterAsync` yang sudah ada |
| `Areas/HealthServices/RegistrationManagement/Controllers/DoctorQueueController.cs` | Pola penautan pengguna ke data dokter |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs` | `CreatePatientAssessmentRequest` menerima `AssessmentType`, bawaan `Initial`; response membawa `AssessmentType` dan `InpEpisodeId`; response penyelesaian membawa `IsRegisteredToIntegrity` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Penjagaan kajian medis: kewenangan menulis, konteks perawatan, batas satu kajian awal; penjagaan draf disaring jenis; kelengkapan saat penyelesaian; pendaftaran ke mesin keutuhan; endpoint `GET /episodes/{episodeId}` |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/MedicalAssessmentTests.cs` | **Baru.** 15 test acceptance beserta kendali negatif dan regresinya |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `POST /patient-assessments` menerima satu field opsional baru berbawaan `Initial`, sehingga seluruh pengirim lama tidak berubah sedikit pun. Satu endpoint baca baru, `GET /patient-assessments/episodes/{episodeId}`. `PATCH /{id}/complete` memperoleh dua perilaku baru **hanya untuk jenis kajian medis**: penolakan bila bagiannya kosong, dan pendaftaran ke mesin keutuhan |
| Database | `NOT APPLICABLE`. Nol perubahan model, **nol migration**. Kolom `AssessmentType` dan `InpEpisodeId` sudah dibuat `BE-RWI-040`. Baris baru pada `MrcClinicalDocumentIntegrity` adalah data operasional, bukan perubahan schema |
| Keamanan/Auth | Butir hak akses **tidak bertambah**: endpoint baca baru memakai `PatientAssessment : Read` yang sudah ada. Ditambahkan satu penjagaan kewenangan berbasis data pada jenis kajian medis; nol pemeriksaan berbasis nama peran |

### 3.4 Keputusan yang dipersempit dengan sengaja

| Hal | Keputusan | Alasan |
| --- | --- | --- |
| Pendaftaran keutuhan | Hanya untuk **kajian medis** | `api-contract.md` §2 tidak membatasi jenisnya, tetapi mendaftarkan pengkajian keperawatan berarti mengubah perilaku jalur poliklinik dan IGD yang tidak diminta task ini. Pendaftarannya adalah pekerjaan sub-modul `keperawatan`. Dicatat sebagai selisih kontrak |
| Penegakan keutuhan | **Tidak** dinyalakan | `ClinicalDocumentIntegrityService.JenisYangDitegakkan` hari ini hanya memuat `ProgressNote` — `RM-DEC-019`. Menambahkan `Assessment` ke daftar itu adalah keputusan `MedicalRecordManagement` dan cakupan `BE-RWI-038`. Pendaftaran tetap berjalan dan tidak memerlukannya |
| Kajian medis di luar rawat inap | **Ditolak** `422` | `VAL-DOK-01` menyebut kajian medis secara eksplisit sebagai dokumen yang menuntut perawatan rawat inap |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat pengkajian. **Kini menerima** `AssessmentType`. Jenis kajian medis menuntut pengguna yang terhubung ke data dokter dan perawatan rawat inap yang berjalan | `PatientAssessment : Create` |
| `GET` | `/episodes/{episodeId}` | **Baru.** Kajian satu perawatan rawat inap, dapat disaring `assessmentType`, terurut waktu kajian terbaru lebih dulu | `PatientAssessment : Read` |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian. **Untuk jenis kajian medis:** menolak bila bagiannya masih kosong, dan mendaftarkan kajian ke mesin keutuhan rekam medis | `PatientAssessment : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `185 Warning(s)`, seluruhnya peringatan yang sudah ada sebelumnya |
| `dotnet test` project uji SQLite, seluruhnya | Berhasil | `PASS` | `Failed: 0, Passed: 262, Skipped: 0, Total: 262` |
| Kajian medis dan catatan harian sebagai dua record dengan status berdiri sendiri | Dua id berbeda; menyelesaikan kajian **tidak** menggerakkan status catatan | `PASS` | `MedicalAssessmentTests.KajianMedisDanCatatanHarian_DuaRecordDenganStatusYangBerdiriSendiri` |
| **Tiga catatan harian tidak mengubah isi kajian medis** | Keluhan utama, riwayat, status, dan waktu kajian identik sebelum dan sesudah; tiga catatan tersimpan | `PASS` | `...TigaCatatanHarian_TidakMengubahIsiKajianMedis` |
| Kajian medis awal kedua pada perawatan yang sama | Ditolak `400`; jumlah kajian medis tetap `1` | `PASS` | `...KajianMedisAwalKedua_DitolakPadaPerawatanYangSama` |
| Kajian medis **ulang** setelah kajian awal selesai | Kajian awal kedua ditolak `400`; kajian ulang diterima `200` | `PASS` | `...KajianMedisUlang_TetapBolehSetelahKajianAwalSelesai` |
| Perawatan lain tidak ikut terhalang | Diterima `200` | `PASS` | `...PerawatanLain_TidakIkutTerhalang` |
| Draf pengkajian keperawatan tidak menutup kajian medis | Diterima `200`; dua baris pada kunjungan yang sama | `PASS` | `...PengkajianKeperawatanYangMasihDikerjakan_TidakMenutupKajianMedis` |
| Kajian medis yang bagiannya kosong diselesaikan | Ditolak `400`; pesan memuat "keluhan utama" dan "riwayat penyakit sekarang"; status tidak berubah menjadi `Completed` | `PASS` | `...KajianMedisYangBagiannyaKosong_DitolakBesertaDaftarBagiannya` |
| **Regresi:** pengkajian keperawatan kosong tetap dapat diselesaikan | Diterima `200` | `PASS` | `...PengkajianKeperawatanYangKosong_TetapDapatDiselesaikan` |
| Pengguna yang bukan dokter membuat kajian medis | Ditolak `403` dengan kalimat `VAL-DOK-05`; nol baris tersimpan | `PASS` | `...PenggunaYangBukanDokter_Ditolak403SaatMembuatKajianMedis` |
| **Kendali negatif:** pengguna yang sama membuat pengkajian keperawatan | Diterima `200`; tersimpan bertanda `Initial` | `PASS` | `...PenggunaYangBukanDokter_TetapBolehMembuatPengkajianKeperawatan` |
| Kajian medis pada kunjungan tanpa perawatan | Ditolak `422` dengan kalimat `VAL-DOK-01` | `PASS` | `...KajianMedisPadaKunjunganTanpaPerawatan_Ditolak422` |
| **Kajian medis yang selesai terdaftar pada mesin keutuhan** | Nol baris keutuhan sebelum penyelesaian; satu baris sesudahnya, berjenis `Assessment` dengan kunjungan dan pasien yang benar; `isRegisteredToIntegrity` bernilai benar | `PASS` | `...KajianMedisYangSelesai_TerdaftarPadaMesinKeutuhan` |
| **Regresi:** pengkajian keperawatan yang selesai tidak didaftarkan dari sini | Nol baris keutuhan | `PASS` | `...PengkajianKeperawatanYangSelesai_TidakDidaftarkanDariSini` |
| Kajian per perawatan terbaca dan dapat disaring jenisnya | Tanpa penyaring `2` baris; disaring kajian medis `1` baris | `PASS` | `...KajianPerPerawatan_TerbacaDanDapatDisaringJenisnya` |
| Perawatan yang tidak ada | Dijawab `404`, bukan daftar kosong | `PASS` | `...KajianPerPerawatanYangTidakAda_Dijawab404` |
| Hak akses non-SuperAdmin pada seluruh endpoint pengkajian | Seluruh pasangan dapat diberikan dan ditegakkan | `PASS` | `ClinicalRoleAccessContractTests`, 19 test — lihat [BE-RWI-044](BE-RWI-044.md) |

Uji manual: `NOT FEASIBLE` — tidak ada lingkungan runtime beserta database yang tersedia pada sesi
ini.

**Tidak dijalankan:**

- Uji terhadap PostgreSQL. Tidak ada database uji yang tersedia; task ini tidak mengubah schema.
- Uji penolakan diagnosis kosong. **Tidak dapat dijalankan** karena kolomnya memang tidak ada —
  lihat bagian 6.1.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kajian medis dan catatan harian tersimpan sebagai record berbeda dengan mesin status yang berjalan sendiri | Terpenuhi | `KajianMedisDanCatatanHarian_DuaRecordDenganStatusYangBerdiriSendiri`, `PengkajianKeperawatanYangMasihDikerjakan_TidakMenutupKajianMedis` |
| 2. Menulis tiga catatan harian tidak mengubah satu huruf pun isi kajian medis | Terpenuhi | `TigaCatatanHarian_TidakMengubahIsiKajianMedis` — isi dibandingkan lewat konteks basis data yang baru, bukan dari memori konteks sebelumnya |
| 3. Satu perawatan memiliki paling banyak satu kajian medis yang berlaku | Terpenuhi | `KajianMedisAwalKedua_DitolakPadaPerawatanYangSama`, `KajianMedisUlang_TetapBolehSetelahKajianAwalSelesai`, `PerawatanLain_TidakIkutTerhalang` |
| 4. Menyelesaikan kajian tanpa diagnosis ditolak `400` beserta daftar bagian yang kosong | **Belum terpenuhi seluruhnya** | Mekanismenya berjalan dan daftar bagian benar-benar dikembalikan — `KajianMedisYangBagiannyaKosong_DitolakBesertaDaftarBagiannya`. **Bagian "diagnosis" tidak dapat diperiksa**; blocker pada bagian 6.1 |
| 5. Perawat ditolak `403` saat mencoba membuat kajian medis | Terpenuhi | `PenggunaYangBukanDokter_Ditolak403SaatMembuatKajianMedis` beserta kendali negatifnya |
| 6. Kajian yang selesai terdaftar pada mesin keutuhan | Terpenuhi | `KajianMedisYangSelesai_TerdaftarPadaMesinKeutuhan` |

### 6.1 Blocker kriteria 4 — tidak ada tempat menyimpan diagnosis kajian medis

**Ini pertentangan antar dokumen kontrak yang sudah disetujui, bukan kekurangan implementasi.**

| Yang menuntut | Bunyinya |
| --- | --- |
| Roadmap `BE-RWI-045` kriteria 4 | "Menyelesaikan kajian tanpa diagnosis ditolak `400` beserta daftar bagian yang kosong" |
| `VAL-DOK-10` | Ditolak bila "keluhan utama, pemeriksaan, atau rencana" kosong |
| `VAL-DOK-11` | Ditolak bila "daftar masalah atau diagnosis" kosong |

| Yang melarang | Bunyinya |
| --- | --- |
| `data/data-dictionary.md` §3 | "`TrxPatientAssessment` — `Diperbarui`, **hanya nilai enum**. Kolom baru dari sub-modul ini: **Nol**" |
| `02-backend-architecture.md` §4.2 | Tabel "Kolom yang diminta" hanya memuat `AssessmentType`. Bagian yang sama **sudah mencatat keberatannya**: "Jalan A karena itu menuntut penambahan isian medis pada tabel yang sama" |

Keadaan source hari ini:

| Bagian yang dituntut | Kolom yang tersedia |
| --- | --- |
| Keluhan utama | **Ada** — `ChiefComplaint` |
| Anamnesis | **Ada** — `CurrentIllnessHistory`, `MedicationHistory` |
| Pemeriksaan fisik | **Tidak ada.** Yang tersedia hanya tanda vital, kesadaran, nyeri, gizi, dan risiko jatuh — seluruhnya bercorak keperawatan |
| Rencana terapi | **Tidak ada** |
| Diagnosis kerja | **Tidak ada.** `TrxPatientDiagnosis` mewajibkan `ConsultationId`, sehingga diagnosis selalu tergantung pada catatan dokter dan **tidak dapat** digantung pada kajian |

Yang dikerjakan: kerangka daftar bagian kosong sudah berdiri dan mengembalikan bagian yang memang
tersedia. Menambahkan tiga kolom sendiri berarti **mendefinisikan ulang kontrak yang sudah
disetujui secara sepihak**, dan itu dilarang.

**Keputusan yang ditunggu dari pemilik** — Product/Domain bersama `ClinicalManagement`:

1. Apakah `TrxPatientAssessment` memperoleh kolom `PhysicalExamination`, `TherapyPlan`, dan
   `WorkingDiagnosis`, dengan kamus data §3 diperbarui; **atau**
2. apakah diagnosis kajian medis digantung pada `TrxPatientDiagnosis` dengan `ConsultationId`
   dilonggarkan menjadi opsional — perubahan yang menyentuh tabel yang dipakai poliklinik; **atau**
3. apakah pemilik memilih jalan B pada `02-backend-architecture.md` §4.2, yaitu bentuk penyimpanan
   tersendiri bagi kajian medis.

Ketiganya adalah keputusan struktur, bukan pekerjaan implementasi. Sesudah salah satunya diputuskan,
kriteria 4 diselesaikan dengan menambah **satu baris per bagian** pada
`BagianKajianMedisYangKosong`.

### 6.2 Definition of Done

| Butir DoD | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | **Belum** — kriteria 4 terpenuhi sebagian, lihat 6.1 |
| Test pemisahan isi hijau | Terpenuhi — `TigaCatatanHarian_TidakMengubahIsiKajianMedis` |
| Laporan menyebut jenis kajian yang dipakai | Terpenuhi — `PatientAssessmentType.MedicalInitial` untuk kajian medis awal dan `PatientAssessmentType.MedicalReassessment` untuk kajian medis ulang, keduanya dibuat `BE-RWI-040` tanpa menggeser nilai lama |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `185 Warning(s)` pada build, seluruhnya peringatan dokumentasi XML yang sudah ada sebelum task ini. Nol peringatan baru |
| Masalah yang diketahui | Blocker kriteria 4 pada bagian 6.1. Selain itu satu kegagalan uji milik `BillingManagement` yang tidak berkaitan — dirinci pada [BE-RWI-044](BE-RWI-044.md) bagian 7 |
| Risiko tersisa | **Pertama, `VAL-DOK-06` belum ditegakkan**: dokter mana pun yang terhubung ke data dokter dan memegang butir `PatientAssessment : Create` dapat menulis kajian medis untuk pasien rawat inap mana pun, termasuk yang bukan tanggung jawabnya. **Kedua, penegakan keutuhan belum menyala untuk jenis `Assessment`**, sehingga kajian yang sudah terdaftar masih dapat disunting lewat `PUT /{id}`; penyalaannya milik `BE-RWI-038`. **Ketiga**, kajian medis dan pengkajian keperawatan berbagi satu butir hak akses; pembedaannya bergantung sepenuhnya pada aturan bisnis yang ditambahkan task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Sama dengan yang dirinci [BE-RWI-044](BE-RWI-044.md) bagian 7; ketiga task dikerjakan pada sesi yang sama, dan perubahan pengguna yang berjalan bersamaan **tidak disentuh**. Nol operasi Git dijalankan |
| Langkah berikutnya | Ajukan tiga pilihan pada bagian 6.1 kepada Product/Domain bersama `ClinicalManagement`. Setelah diputuskan, kriteria 4 diselesaikan dan status task dinaikkan menjadi ✅. `BE-RWI-047` menunggu `BE-RWI-038`; `BE-RWI-048` menunggu `BE-RWI-041` |
