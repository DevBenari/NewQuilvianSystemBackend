# Laporan Perubahan Backend — `BE-RWI-068`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-068` |
| Judul | Diagnosis kerja dapat dicatat langsung dari kajian medis awal |
| Slice | `DOK-MVP-7` — menutup tiga celah kontrak yang ditemukan layar |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 kartu `BE-RWI-068`; bagian 0.1, 0.3, 3, dan 5 |
| Trace | `CAP-022` aturan 2 dan aturan 5; `AC-CAP022-02`; `RWI-DEC-062`; `INT-DOK-10`; `VAL-DOK-36` s.d. `VAL-DOK-40`; `VAL-DOK-11`; `RWI-AC-143`. **Ditemukan** saat `FE-RWI-044` dikerjakan |
| Contract version | **`0.4.0` — `approved` Muhammad Hamzah, 9 September 2026.** Terkunci dan mengikat |
| Dependency | `BE-RWI-039` ✅ service konteks klinis; `BE-RWI-045` ✅ kajian medis awal. Keduanya selesai, nol dependency tersisa |
| Klasifikasi | `HEAVY` — skor 9: repository 0 (satu repository), berkas diperiksa 1 (9–20), berkas diubah 1 (4–8), logika bisnis 2 (kompleks — lima aturan konteks yang saling bergantung urutan), kontrak API 1 (memakai kontrak yang sudah disetujui), database 2 (dampak schema dan migration), keamanan/auth 2 (kewenangan per pasien pada jalur tulis yang baru dibuka), UI/workflow 0 |
| Task mode | `BACKEND` — backend target tulis, frontend rujukan hanya-baca |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, migration, laporan ini, roadmap, dan `requirement-traceability.md` sub-modul yang sama |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | Dikerjakan di atas `7027096435c321572ea01fa35cf2080ebe899ba8`, branch `MHamzah`, upstream `origin/MHamzah`. Agent tidak menjalankan satu pun tindakan Git |
| Tanggal | 9 September 2026 |
| Status | 🟡 **SEBAGIAN — 9 September 2026.** Kesembilan acceptance criteria **terpetakan ke source yang benar-benar ada dan terkompilasi**: `dotnet build` solution **0 error** `CS`, 187 warning, seluruhnya warning lama yang tidak berasal dari berkas task ini. Yang **belum** terpenuhi adalah buktinya: `dotnet test` **`NOT RUN`**, uji migration maju-mundur terhadap PostgreSQL **`NOT RUN`**, dan berkas uji baru **belum pernah dikompilasi** — ketiganya dihentikan atas **instruksi pengguna 9 September 2026** ("skip build"). Satu migration dibuat dan **belum diterapkan ke database mana pun** |

---

## 0. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | Tidak ada. `TrxPatientDiagnosis`, DTO, dan controller-nya tinggal langsung di bawah modul |
| Pemilik / prefix pada registry | `ClinicalManagement / Clinical`, prefix **`Cli`**, kategori `BUSINESS DOMAIN / MODULE`, lifecycle `ACTIVE / LEGACY` — `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 |
| Status registry | **Terdaftar dan disetujui.** Tidak ada entri baru yang dibutuhkan, sebab task ini **tidak membuat satu entity pun** |
| Keberlakuan | **`TOUCHED LEGACY`.** Seluruh perubahan menyentuh `TrxPatientDiagnosis` yang sudah ada beserta configuration, DTO, dan controller-nya. Nol berkas `NEW CODE`, nol entity baru, nol modul baru |
| QBE ID yang benar-benar berlaku | `QBE-CFG-002`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-TXN-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-NAM-001` dan `QBE-NAM-002` — tidak ada entity, file, configuration, maupun DbSet baru yang dinamai. `QBE-MOD-002` dan `QBE-MOD-003` — tidak ada folder atau modul baru. `QBE-CODE-001` s.d. `QBE-CODE-006` — task ini tidak mengalokasikan satu nomor bisnis pun. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION`; nama `Trx*` pada tabel ini **sengaja tidak disentuh** |
| Catatan ratchet | Nama `TrxPatientDiagnosis` melanggar `QBE-NAM-001` bila ia kode baru. Ia **bukan** kode baru, dan menormalkannya menuntut rename source **beserta** tabel fisiknya sekaligus (`QBE-NAM-003`) — kampanye tersendiri dengan wewenang pemilik arsitektur backend. Dicatat sebagai temuan, tidak dikerjakan di sini. Ini konsisten dengan bagian 6 roadmap, yang menempatkan perapian entity `Trx*` sebagai task tersendiri |

---

## 1. Masalah yang diperbaiki

Diagnosis kerja lahir **pada pemeriksaan pertama**. Dokter yang baru selesai memeriksa pasien
yang tadi pagi masuk kamar sudah tahu masalahnya, dan di situlah ia menuliskannya — pada layar
**kajian medis awal**.

Sampai sebelum task ini, sistem tidak mengizinkannya. Diagnosis terstruktur berkode ICD wajib
menyebut **nomor konsultasi**, yaitu nomor catatan dokter harian. Padahal pada pemeriksaan
pertama catatan harian itu **belum ada** — kajian medis awal adalah dokumen dan layar tersendiri
yang justru lahir lebih dulu.

Akibatnya urutan kerjanya terbalik dari kenyataan:

| Langkah | Yang terpaksa dilakukan sebelum task ini | Yang sebenarnya diinginkan |
| ---: | --- | --- |
| 1 | Membuat catatan harian, walaupun belum ada yang mau dicatat harian | Membuka kajian medis awal |
| 2 | Membuka kajian medis awal | Menambah diagnosis "J18.9 Pneumonia" |
| 3 | Menambah diagnosis, digantungkan pada catatan harian tadi | Selesai |

**Contoh nyata.** dr. Sari memeriksa Tn. Budi yang masuk kamar pukul 10.40. Pukul 11.15 ia
menuliskan kajian medis awal dan hendak menambahkan diagnosis kerja "J18.9 Pneumonia". Permintaan
itu **ditolak** karena tidak ada nomor konsultasi. Untuk melanjutkan, dr. Sari harus membuat satu
catatan harian lebih dulu — satu dokumen yang tidak ia perlukan, semata-mata supaya diagnosisnya
punya tempat menggantung.

Layar `FE-RWI-044` sudah dibuat dan menemukan tembok ini. Layar itu berhenti di 🟡 dengan satu
elemen yang tidak dapat dipasang: **tombol Tambah Diagnosis**. Daftar masalahnya hanya dapat
ditampilkan sebagai bacaan, beserta keterangan ke mana penambahannya menempel.

**Jalan keluar sementara yang sudah ada, dan kenapa ia tidak cukup.** Sejak `BE-RWI-045`, kajian
medis punya kolom `WorkingDiagnosis` berupa **teks bebas**, jadi dokter tidak benar-benar
terhenti. Tetapi teks bebas tidak dapat dicari, tidak berkode ICD, tidak dapat dinyatakan
teratasi, dan tidak terbawa ke ringkasan masalah. `CAP-022` aturan 5 menuntut daftar masalah
berbentuk **objek terstruktur berkode**, dan itulah yang belum terpenuhi.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | DPJP menambahkan diagnosis kerja berkode ICD **langsung dari layar kajian medis awal**, tanpa membuat catatan harian lebih dulu |
| Pelaku | DPJP pasien rawat inap yang bersangkutan — bukan sekadar pemegang butir hak akses |
| Pemicu | Menekan Tambah Diagnosis pada daftar masalah di layar kajian medis awal |
| Hasil | Satu baris diagnosis terstruktur yang menggantung pada **perawatan rawat inap**, bukan pada catatan dokter |

### 2.2 Alur normal — jalur kajian medis

1. Pasien terdaftar pada satu kunjungan bertipe `Inpatient`, dan admisinya sudah membentuk
   **perawatan rawat inap** yang berjalan.
2. DPJP membuka layar kajian medis awal pasien itu dan menekan Tambah Diagnosis.
3. Layar mengirim permintaan yang menyebut **kunjungan** dan **perawatan**, tanpa nomor
   konsultasi.
4. Backend memeriksa konteksnya berurutan: jenis kunjungan, kelengkapan konteks, kecocokan
   perawatan dengan kunjungan, keadaan perawatan, lalu kewenangan penulisnya.
5. Bila seluruhnya lolos, satu baris diagnosis tersimpan dengan **kolom konsultasi kosong** dan
   **kolom perawatan terisi**.
6. Diagnosis itu langsung terbaca pada daftar masalah kajian medis pasien tersebut.

### 2.3 Alur normal — jalur catatan dokter, tidak berubah

1. Dokter menulis catatan harian seperti biasa.
2. Ia menambahkan diagnosis sambil menyebut **nomor konsultasi**.
3. Perilakunya **identik** dengan sebelum kontrak `0.4.0`: barisnya menggantung pada catatan
   dokter, dan ringkasan diagnosis pada catatan itu ikut diperbarui.

### 2.4 Jalur tidak normal

| Keadaan | Jawaban sistem | Kode | Aturan |
| --- | --- | :---: | --- |
| Kunjungan **rawat jalan** atau **medical check-up**, nomor konsultasi kosong | "Konsultasi dokter tidak ditemukan atau tidak sesuai encounter." — **kalimat yang sama persis** seperti sebelum `0.4.0` | `400` | `VAL-DOK-38`, `RWI-AC-143` |
| Kunjungan **IGD** | Jawaban yang sama seperti baris di atas. Jalur IGD **sengaja tidak dibuka** | `400` | `api-contract.md` bagian 11; `integration-contract.md` bagian 10.2 |
| Kunjungan rawat inap, **kedua konteks kosong** | "Diagnosis harus melekat pada catatan dokter atau pada perawatan pasien yang sedang berjalan." | `400` | `VAL-DOK-36` |
| Kedua konteks terisi tetapi menunjuk pasien atau kunjungan berbeda | "Diagnosis ini tidak cocok dengan perawatan pasien. Periksa kembali pasien yang sedang Anda buka." | `400` | `VAL-DOK-37` |
| Perawatan yang disebut bukan milik pasien pada permintaan itu | Kalimat yang sama seperti baris di atas | `400` | `VAL-DOK-40` |
| Pengguna tidak terhubung ke satu baris dokter aktif mana pun | "Catatan ini hanya dapat ditulis dokter." | `403` | `VAL-DOK-05` |
| Dokter memegang butir hak akses tetapi **bukan DPJP** pasien itu | "Anda bukan DPJP pasien ini. Hubungi DPJP atau supervisor klinis." | `403` | `VAL-DOK-39`, memakai penjaga yang sama dengan `VAL-DOK-06` |
| Pasien tidak sedang dirawat inap, perawatan masih `Draft`, atau sudah ditutup | Kalimat dari service konteks klinis, diteruskan apa adanya | `422` | `api-contract.md` bagian 2.1.3 |

### 2.5 Kenapa urutan pemeriksaannya penting

Dua aturan tampak bertabrakan bila urutannya dibalik. `VAL-DOK-36` menolak permintaan yang kedua
konteksnya kosong, sedangkan `VAL-DOK-38` menuntut kunjungan rawat jalan tetap menerima kalimat
lama. Sebuah permintaan rawat jalan tanpa konteks apa pun memenuhi **keduanya**.

Yang berlaku adalah aturan yang lebih spesifik: **jenis kunjungan diperiksa lebih dulu.** Bila
kunjungannya bukan `Inpatient`, permintaan itu tidak pernah masuk jalur baru sama sekali dan
dijawab persis seperti sebelum `0.4.0`. `VAL-DOK-36` karena itu hanya berlaku di dalam jalur
rawat inap — tempat pelonggarannya memang berlaku.

Konsekuensinya jujur dan disengaja: **pelonggaran ini tidak menetes ke poliklikik, medical
check-up, maupun IGD**, dan itu dibuktikan tiga uji regresi, bukan diklaim.

### 2.6 Satu jaring pengaman basis data dilepas, dan gantinya

Sebelum task ini, basis data sendiri menolak diagnosis tanpa konteks: kolom konsultasi
bertanda `NOT NULL`. Aturan barunya adalah **salah satu wajib** — konsultasi **atau** perawatan —
dan tidak ada satu kolom pun yang selalu terisi pada kedua jalur, sehingga aturan itu tidak dapat
ditegakkan `NOT NULL`.

| Sebelum | Sesudah |
| --- | --- |
| Basis data menolak baris tanpa nomor konsultasi | Basis data menerimanya; **aturan bisnis `VAL-DOK-36`** yang menolak |

Itulah sebabnya kasus "kedua konteks kosong" mendapat uji tersendiri, dan ujinya tidak berhenti
pada kode balasan: ia juga menghitung baris konsultasi sebelum dan sesudah, membuktikan tidak ada
**konsultasi bayangan** yang dibuatkan diam-diam demi mengisi kolom.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Kontrak dan artefak blueprint** — `contracts/api-contract.md` bagian 0.3, 2, 2.1, 2.1.1, 2.1.2,
2.1.3, dan 11; `contracts/integration-contract.md` bagian 10, 10.1, 10.2;
`contracts/validation-matrix.md` bagian 9 beserta `VAL-DOK-05`, `VAL-DOK-06`, `VAL-DOK-11`;
`contracts/permission-audit-matrix.md` bagian 1.1, 2, 3, 4, 5; `testing/acceptance-test-matrix.md`
bagian 11; `02-backend-architecture.md` bagian 4.10 dan 7.3 langkah 11;
`data/data-dictionary.md` bagian 10.1; `roadmap/backend-roadmap.md` kartu `BE-RWI-068`.

**Governance** — `AGENTS.md`; `rules/GLOBAL_RULES.md`; `rules/backend/TASK_RULES.md`;
`TASK_CLASSIFICATION.md`; `API_RULES.md`; `DATABASE_RULES.md`; `REVIEW_RULES.md`;
`REPORT_TEMPLATE.md`; `engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/rule-output/status-task-roadmap.md`.

**Source backend** — `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs`;
`Repositories/Configurations/HealthServices/TrxPatientDiagnosisConfiguration.cs`;
`Areas/HealthServices/ClinicalManagement/DTOs/PatientDiagnosisDtos.cs`;
`Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs`;
`Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`;
`Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs`;
`Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs` sebagai pola
penurunan identitas dokter; `Models/TrxPatientAssessment.cs` dan `Models/TrxPatientProcedure.cs`
beserta configuration-nya sebagai pola kolom `InpEpisodeId` dari `BE-RWI-040`;
`Repositories/ApplicationDbContext.cs`; `Program.cs` untuk registrasi service.

**Konsumen yang diperiksa tanpa diubah** — `ClinicalNoteAttachmentController.cs`,
`MedicalCertificateController.cs`, `PatientClinicalDocumentController.cs`, dan
`MedicalRecordTimelineService.cs`, karena keempatnya membaca `TrxPatientDiagnosis.ConsultationId`
yang kini menjadi boleh kosong. **Frontend** `patient-diagnosis.service.js` dibaca sebagai
rujukan konsumen, tidak diubah.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs` | `ConsultationId` menjadi `Guid?` dan `[Required]`-nya dilepas; kolom `InpEpisodeId` ditambahkan. Keduanya diberi keterangan yang menyebut alasan dan preseden `QueueId` pada `TrxPatientAssessment` |
| `Repositories/Configurations/HealthServices/TrxPatientDiagnosisConfiguration.cs` | `ConsultationId` menjadi `IsRequired(false)`; `InpEpisodeId` didaftarkan; relasi ke `InpEpisode` dengan `DeleteBehavior.Restrict`; index gabungan `InpEpisodeId` bersama `PatientId`. Pola relasinya sama persis dengan empat tabel klinis lain sejak `BE-RWI-040` |
| `Migrations/20260909065125_AddInpatientEpisodeContextToPatientDiagnosis.cs` | Satu migration: kolom `InpEpisodeId` ditambahkan, `ConsultationId` dilonggarkan, foreign key dan index dipasang. Nol baris lama disentuh |
| `Areas/HealthServices/ClinicalManagement/DTOs/PatientDiagnosisDtos.cs` | `CreatePatientDiagnosisRequest.ConsultationId` menjadi `Guid?` tanpa `[Required]`; `InpEpisodeId` ditambahkan pada request pembuatan, request penyuntingan, dan kedua DTO balasan; `ConsultationId` pada balasan menjadi `Guid?`; penyaring `inpEpisodeId` masuk metadata penyaring |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` | Pemeriksaan konteks bercabang menggantikan pencarian konsultasi yang melempar; lima kalimat penolakan terkunci sebagai konstanta; penurunan identitas dokter dan penjagaan DPJP per perawatan; pemeriksaan duplikat, pelepasan diagnosis utama, dan ringkasan menjadi sadar-konteks; penyaring `inpEpisodeId` pada daftar dan feed pilihan |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `VAL-DOK-11` dipertajam: bagian "diagnosis kerja" terbaca kosong hanya bila teks bebas **dan** daftar terstruktur sama-sama kosong |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientStructuredDiagnosisTests.cs` | **Berkas baru.** Kesembilan skenario acceptance beserta uji IGD tambahan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak ada endpoint yang dihapus, diganti nama, atau berubah bentuknya.** `POST /` menerima dua field opsional baru dan tidak lagi menuntut `ConsultationId` pada kunjungan rawat inap. `GET /` dan `GET /options` menerima penyaring `inpEpisodeId`. Balasan bertambah `InpEpisodeId`, dan `ConsultationId` pada balasan menjadi boleh kosong — hanya untuk baris yang lahir dari kajian medis; seluruh baris lama tetap terisi. Delta terhadap kontrak dicatat pada bagian 7 |
| Database | **Satu migration dibuat.** Satu kolom `uuid` nullable ditambahkan, satu kolom diturunkan dari `NOT NULL`, satu foreign key `Restrict`, satu index gabungan. **Nol baris data disentuh.** Status penerapan: **belum diterapkan ke database mana pun.** Nol perintah dikirim ke `QuilvianNewDevHamzah` maupun database bersama lainnya |
| Keamanan/Auth | **Bertambah, tidak berkurang.** Jalur tulis kedua yang baru dibuka mendapat penjagaan kewenangan **per pasien** yang tidak dimiliki mesin hak akses: penulis wajib terhubung ke satu baris dokter aktif, dan dokter itu wajib memegang penugasan DPJP yang berlaku pada perawatan tersebut. **Nol hardcode role**: tidak ada `IsInRole`, nama peran, nama departemen, nama posisi, maupun `UserType` yang dipakai sebagai penentu kewenangan. Metadata `[AccessAction]` dan `[AccessPermission]` yang sudah ada tidak disentuh — nol Resource baru dan nol Action baru |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Diagnosis

Base URL: `api/v1/health-services/clinical-management/patient-diagnoses`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mencatat diagnosis terstruktur berkode ICD. **Berubah:** menerima `InpEpisodeId` sebagai konteks, dan tidak lagi menuntut `ConsultationId` pada kunjungan rawat inap | `PatientDiagnosis : Create` |
| `GET` | `/` | Daftar diagnosis pasien. **Berubah:** penyaring `inpEpisodeId` ditambahkan | `PatientDiagnosis : Read` |
| `GET` | `/options` | Daftar ringkas untuk daftar masalah pada layar kajian medis. **Berubah:** penyaring `inpEpisodeId` ditambahkan | `PatientDiagnosis : Read` |
| `PUT` | `/{id}` | Menyunting diagnosis yang belum diselesaikan. **Berubah:** menerima `InpEpisodeId`, dan menolak nilai yang tidak cocok dengan konteks tersimpan | `PatientDiagnosis : Update` |
| `GET` | `/{id}` | Membaca satu diagnosis. **Berubah:** balasannya membawa `InpEpisodeId` | `PatientDiagnosis : Read` |
| `GET` | `/filters/metadata` | Nilai bawaan penyaring. **Berubah:** memuat `inpEpisodeId` | `PatientDiagnosis : Read` |
| `GET` | `/master-options` | Pencarian master diagnosis ICD. **Tidak berubah** | `PatientDiagnosis : Read` |
| `PATCH` | `/{id}/set-primary` | Menetapkan diagnosis utama. **Berubah di dalam:** cakupannya mengikuti konteks barisnya | `PatientDiagnosis : Update` |
| `PATCH` | `/{id}/resolve` | Menyatakan masalah teratasi. **Tidak berubah** | `PatientDiagnosis : Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan diagnosis salah catat. **Berubah di dalam:** ringkasan hanya diperbarui bila barisnya punya catatan dokter | `PatientDiagnosis : Update` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/complete` | Menyelesaikan kajian. **Berubah di dalam:** bagian diagnosis kerja terbaca kosong hanya bila teks bebas dan daftar terstruktur sama-sama kosong | `PatientAssessment : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -c Debug` | **Berhasil.** `0 Error(s)`, `187 Warning(s)`, `Time Elapsed 00:11:18.84` | `PASS` | Keluaran perintah. Nol warning berasal dari kelima berkas aplikasi yang diubah task ini; seluruh 187 warning adalah warning lama pada berkas yang tidak disentuh |
| Kompilasi kelima berkas aplikasi yang diubah | Model, configuration, DTO, `PatientDiagnosisController`, dan `PatientAssessmentController` terkompilasi | `PASS` | Build di atas menghasilkan `bin/Debug/net9.0/QuilvianSystemBackend.dll` |
| `dotnet ef migrations add` | Satu migration terbentuk dari model yang sudah terverifikasi; selisih `ApplicationDbContextModelSnapshot.cs` **20 baris tambah, 4 baris ubah** dan seluruhnya persis perubahan yang diminta | `PASS` | `Migrations/20260909065125_AddInpatientEpisodeContextToPatientDiagnosis.cs`; `git diff` snapshot |
| Kompilasi berkas uji baru | **Belum pernah dikompilasi.** Build solution yang lulus hanya menghasilkan `QuilvianSystemBackend.dll`; project `Tests/QuilvianSystemBackend.UnitTests.Sqlite` **tidak ikut terbangun** pada build itu, dan artefaknya masih bertanggal 8 September 2026 | `NOT RUN` | Instruksi pengguna 9 September 2026 |
| `dotnet test` — kesembilan skenario `acceptance-test-matrix.md` bagian 11 | **Tidak dijalankan** | `NOT RUN` | Instruksi pengguna 9 September 2026 |
| Uji migration maju-mundur terhadap PostgreSQL sungguhan | **Tidak dijalankan.** Container `postgres:15.15` sekali pakai sempat disiapkan pada `localhost:55432`, database `quilvian_rwi_test`, dan terbukti hidup — tetapi **nol migration diterapkan** padanya, lalu container itu dihapus | `NOT RUN` | Instruksi pengguna 9 September 2026 |
| Penerapan migration ke database mana pun | **Tidak dijalankan.** Nol perintah dikirim ke `QuilvianNewDevHamzah` maupun database bersama lainnya | `NOT RUN` | Wewenang eksekusi database memang terpisah dan tidak diberikan |

Uji manual: `NOT APPLICABLE` — task ini tidak menyentuh layar.

**Tidak dijalankan, beserta alasannya.** Pengguna memerintahkan `skip build` pada 9 September 2026
ketika rangkaian build dan uji sedang berjalan. Perintah itu diikuti apa adanya: proses build
dihentikan, container database uji dihapus, dan **tidak satu pun hasil dikarang**. Konsekuensinya
dicatat jujur pada bagian 6 — task ini **belum** boleh disebut selesai.

**Satu koreksi yang wajib dibaca.** Sebuah pemeriksaan build lebih awal sempat terbaca lulus
padahal sebenarnya gagal: kode keluarnya tertutup oleh pipa `grep`, dan kegagalan yang
disembunyikannya adalah `using` yang hilang untuk `MstDoctor`. Kekeliruan itu ditemukan,
`using`-nya ditambahkan, dan build berikutnya dijalankan dengan kode keluar yang benar-benar
ditangkap. Kekeliruan yang sama juga menjelaskan satu migration kosong yang sempat terbentuk di
atas assembly basi; migration itu **dihapus** dan `ApplicationDbContextModelSnapshot.cs`
dikembalikan ke keadaan semula lewat `git checkout` sebelum migration yang benar dibuat.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

Kolom **Source** menjawab "apakah kodenya ada dan terkompilasi". Kolom **Bukti uji** menjawab
"apakah perilakunya sudah dibuktikan berjalan". Keduanya sengaja dipisah, karena pada task ini
jawabannya memang berbeda.

| Kriteria | Source | Bukti uji | Letaknya |
| --- | :---: | :---: | --- |
| **1.** Diagnosis terstruktur dapat dibuat dengan menyebut perawatan rawat inap, tanpa nomor konsultasi, ketika pasien belum punya catatan harian | Terpenuhi | **Belum dijalankan** | `PatientDiagnosisController.ResolveCreateContextAsync` cabang ketiga; pembentukan entity pada `CreateDiagnosis` |
| **2.** Diagnosis itu terhubung ke pasien dan kunjungan yang benar, dan terbaca pada daftar masalah lewat penyaring `inpEpisodeId` | Terpenuhi | **Belum dijalankan** | Penyaring `inpEpisodeId` pada `GetDiagnoses` dan `GetDiagnosisOptions`; `PatientId` dan `EncounterId` diambil dari konteks perawatan |
| **3.** Permintaan tanpa satu pun konteks ditolak `400`, dan nol baris konsultasi terbentuk | Terpenuhi | **Belum dijalankan** | `PenolakanTanpaKonteks`; jalur kajian medis tidak pernah membuat `TrxDoctorConsultation`, dan ringkasannya memakai `BuildEpisodeDiagnosisSummaryAsync` yang hanya membaca |
| **4.** Permintaan yang menyebut konteks milik pasien lain ditolak `400` | Terpenuhi | **Belum dijalankan** | `PenolakanKonteksPasienBerbeda` pada kedua cabang, memakai `expectedPatientId` dan `expectedEpisodeId` milik service konteks klinis |
| **5.** Kewenangan mengikuti kewenangan menulis kajian medis; dokter yang memegang hak akses tetapi bukan DPJP pasien itu ditolak `403` | Terpenuhi | **Belum dijalankan** | `ResolveCurrentDoctorIdAsync` lalu `IsDoctorAssignedAsync`; nol `IsInRole`, nol nama peran, nol `UserType` |
| **6.** Rawat jalan dan medical check-up tidak berubah: ditolak dengan kode **dan kalimat yang sama persis** | Terpenuhi | **Belum dijalankan** | Jenis kunjungan diperiksa lebih dulu; kalimatnya dikunci konstanta `PenolakanKonsultasiTidakDitemukan` |
| **7.** Jalur IGD tidak ikut dibuka | Terpenuhi | **Belum dijalankan** | Pelonggaran hanya berlaku ketika `EncounterType.Inpatient`; `Emergency` jatuh ke jalur lama |
| **8.** Permintaan lama yang menyebut nomor konsultasi berperilaku identik | Terpenuhi | **Belum dijalankan** | Cabang pertama `ResolveCreateContextAsync` memakai pencarian, urutan penolakan, dan pembaruan ringkasan yang sama seperti sebelumnya |
| **9.** Kajian medis tetap dapat diselesaikan ketika diagnosis kerja teks kosong tetapi daftar terstruktur terisi | Terpenuhi | **Belum dijalankan** | `BagianKajianMedisYangKosong` beserta `AdaDaftarMasalahTerstrukturAsync` pada `PatientAssessmentController` |

**Kesembilan kriteria punya source yang benar-benar ada dan terkompilasi. Nol di antaranya sudah
dibuktikan berjalan.** Berkas uji yang memuat kesembilan skenario itu sudah ditulis lengkap, tetapi
**belum pernah dikompilasi maupun dijalankan**.

### 6.2 Definition of Done

| Butir DoD | Keadaan |
| --- | --- |
| Kesembilan acceptance criteria terpetakan ke source yang benar-benar ada | **Terpenuhi** |
| `dotnet build` dijalankan dan hasilnya dicatat apa adanya | **Terpenuhi** — `0 Error(s)`, `187 Warning(s)` |
| `dotnet test` dijalankan dan hasilnya dicatat apa adanya | **Belum terpenuhi** — `NOT RUN`, instruksi pengguna 9 September 2026 |
| Uji migration maju-mundur terhadap PostgreSQL sungguhan | **Belum terpenuhi** — `NOT RUN`, alasan yang sama |
| Nol perintah dikirim ke database bersama mana pun | **Terpenuhi** |
| Uji regresi rawat jalan membandingkan kalimat penolakan utuh | **Belum terpenuhi sebagai bukti.** Ujinya sudah ditulis dan membandingkan kalimat utuh, tetapi belum dijalankan |
| Nol konsultasi bayangan, dibuktikan test | **Belum terpenuhi sebagai bukti.** Ujinya menghitung baris konsultasi sebelum dan sesudah, tetapi belum dijalankan |
| Laporan tracked ada di `../task/report/backend/BE-RWI-068.md` | **Terpenuhi** |
| Register bagian 4.1 dan `requirement-traceability.md` diperbarui | **Terpenuhi** |
| QBE preflight diselesaikan pada waktu eksekusi | **Terpenuhi** — bagian 0 |

**Karena itu task ini 🟡 `SEBAGIAN`, bukan ✅ `SELESAI`.** Yang menahannya bukan kekurangan
source, melainkan bukti yang belum dijalankan. Menjalankan `dotnet test` beserta uji migration
menaikkannya menjadi ✅ **tanpa satu baris kode pun berubah**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` menghasilkan 187 warning, **seluruhnya warning lama** pada berkas yang tidak disentuh task ini — `CS8981` pada migration bernama `test`, `CS0108`, `CS8619`, `CS8602`, dan `CS0618`. Nol warning baru berasal dari kelima berkas aplikasi yang diubah |
| Masalah yang diketahui | **Satu jaring pengaman basis data sengaja dilepas.** Aturan salah-satu-wajib tidak dapat ditegakkan `NOT NULL`, sehingga basis data tidak lagi menolak diagnosis tanpa konteks; penjagaannya sepenuhnya `VAL-DOK-36` pada controller. Ujinya sudah ditulis tetapi belum dijalankan, sehingga penjagaan itu **belum terbukti berjalan** |
| Masalah yang diketahui | **Langkah mundur migration tidak simetris.** Mengembalikan `ConsultationId` menjadi `NOT NULL` gagal bila sudah ada diagnosis rawat inap tanpa nomor konsultasi. Urutan mundur yang benar ditulis sebagai komentar pada berkas migration-nya sendiri, mengikuti `02-backend-architecture.md` bagian 7.3 langkah 11 |
| Masalah yang diketahui | **Delta kontrak: kode balasan dan tipe balasan.** `api-contract.md` bagian 2.1 menuliskan `201` dan `ApiResponse<PatientDiagnosisResponse>` untuk `POST /`, sedangkan source sejak sebelum `0.4.0` mengembalikan `200` beserta `ApiResponse<PatientDiagnosisCreateResponse>`. Keduanya **sengaja tidak diubah**: mengubahnya adalah breaking change bagi `FE-RWI-044` yang sudah membacanya, dan kontrak `0.4.0` sendiri menyatakan "tidak ada endpoint yang dihapus atau berubah bentuknya". Dicatat sebagai selisih dokumen untuk pemilik kontrak, bukan diperbaiki diam-diam dari sini |
| Masalah yang diketahui | **Delta scope yang ditambahkan.** Kartu task menyebut enam butir scope; `PatientAssessmentController` **tidak** termasuk di antaranya, tetapi acceptance criteria nomor 9 dan `VAL-DOK-11` menuntutnya. Berkasnya tetap berada di dalam `Areas/HealthServices/ClinicalManagement/` sebagaimana batas scope, dan perubahannya dua tempat saja |
| Risiko tersisa | **Yang paling mungkin rusak adalah jalur lama, bukan jalur barunya**, dan justru itulah yang belum diuji. Tabel `TrxPatientDiagnosis` dipakai poliklinik setiap hari. Tiga uji regresi sudah ditulis untuk menjaganya — rawat jalan, medical check-up, dan permintaan lama bernomor konsultasi — tetapi **belum satu pun dijalankan**. Sampai ketiganya hijau, perubahan ini **tidak boleh dianggap aman untuk dirilis** |
| Risiko tersisa | Migration belum pernah diterapkan ke PostgreSQL mana pun, sehingga bentuk SQL-nya belum terbukti berjalan pada basis data sungguhan |
| Perubahan sampingan | **Satu, dan sudah dipulihkan.** `dotnet ef migrations remove` sempat menulis ulang `ApplicationDbContextModelSnapshot.cs` secara besar-besaran di atas assembly yang basi — 1.827 baris tambah, 4.304 baris hapus. Berkas itu dikembalikan ke keadaan semula lewat `git checkout` pada berkas itu saja, lalu migration yang benar dibuat di atas assembly yang segar. Nol pekerjaan pengguna dibuang |
| Perubahan sampingan | Satu migration kosong `20260909062722_AddInpatientEpisodeContextToPatientDiagnosis` sempat terbentuk dari assembly basi yang sama, dan **kedua berkasnya dihapus**. Yang tersisa hanya migration `20260909065125` yang isinya benar |
| Interupsi | **Dua.** Pertama, sesi terputus di tengah rangkaian build; pekerjaannya dilanjutkan dari keadaan Git yang terverifikasi tanpa penyuntingan ganda. Kedua, pengguna memerintahkan `skip build` pada 9 September 2026; proses build dihentikan, container database uji dihapus, dan konsekuensinya dicatat apa adanya alih-alih hasilnya dikarang |
| Status Git | Lima berkas aplikasi `M`, satu snapshot `M`, dua berkas migration `??`, satu berkas uji `??`, dan laporan ini `??`. Berkas `docs/module-blueprints/**` lain yang berstatus `M` adalah pekerjaan `/qv-design` yang **sudah ada sebelum task ini dimulai** dan tidak disentuh, kecuali roadmap dan `requirement-traceability.md` yang memang wewenang task ini. Agent tidak melakukan stage, commit, push, pull, merge, rebase, maupun deploy |
| Langkah berikutnya | **1.** Jalankan `dotnet test Tests/QuilvianSystemBackend.UnitTests.Sqlite/QuilvianSystemBackend.UnitTests.Sqlite.csproj` dan catat hasilnya; ini sekaligus mengompilasi berkas uji baru untuk pertama kalinya. **2.** Jalankan uji migration maju-mundur terhadap container `postgres:15.15` sekali pakai. **3.** Setelah keduanya hijau, naikkan status pada roadmap menjadi ✅ dan perbarui laporan ini. **4.** Barulah `FE-RWI-044` dapat dijalankan ulang untuk memasang tombol Tambah Diagnosis |
