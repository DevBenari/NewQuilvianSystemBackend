# Laporan Perubahan Backend — `BE-RWI-128`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-128` |
| Judul | Perbaikan anomali CPPT dari konsultasi dan penulis kajian medis rawat inap |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap-v2.md` |
| Trace | `ISSUE-DOK-002` butir `ISS-07`, `ISS-08`; `ISS-09` **TIDAK DIKERJAKAN** beserta alasannya |
| Contract version | `0.6.1` — **tidak naik**. Tidak ada properti, endpoint, status code, maupun bentuk payload yang berubah |
| Dependency | Tidak ada task pendahulu |
| Klasifikasi | `MEDIUM` — skor 5 (repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 0, database 1, keamanan 0, UI 0, perbaikan data 1) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, `Migrations/scripts/`, dan `docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9deb23dec4eeaddb162567496076de53ed896622` |
| Tanggal | 23 September 2026 |
| Status | Selesai untuk lingkup source. **Kompilasi PASS** — `0 error`, `224 warning`, tidak satu pun warning baru. **`dotnet build` penuh EXISTING / ENVIRONMENT ISSUE** — penyalinan ke `bin` ditolak karena proses backend `PID 6608` sedang berjalan. Verifikasi runtime **NOT RUN**. Skrip perbaikan data historis **belum dijalankan** |

---

## 0. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | — (blueprint sub-modul `dokter-rawat-inap` pada modul bisnis `rawat-inap`) |
| Pemilik/prefix pada registry | `ClinicalManagement / Clinical` — prefix `Cli`, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | **`TOUCHED LEGACY`** — kedua tabel yang dibaca dan ditulis masih bernama `Trx*` (`TrxPatientIntegratedProgressNote`, `TrxPatientAssessment`), bukan `Cli*` canonical |
| Status registry | Terdaftar dan `ACTIVE`; tidak ada entity operasional baru yang diminta |
| QBE ID yang benar-benar berlaku | Tidak ada. Task ini **tidak** membuat entity, tabel, kolom, endpoint, maupun rename apa pun. `QBE-NAM-003`, `QBE-DB-001`, dan `QBE-DB-002` **tidak berlaku** karena tidak ada `LEGACY MIGRATION` yang dikerjakan |
| Catatan penamaan legacy | Nama `Trx*` yang bertahan pada kedua tabel adalah peninggalan yang **dicatat sebagai temuan, bukan diperbaiki di sini**. Rename tabel milik `ClinicalManagement` menuntut wewenang migration dan database tersendiri, dan tidak diberikan pada task ini. Inilah juga sebab `ISSUE-DOK-002` keliru menyebut `CliPatientIntegratedProgressNote`: prefix canonical registry ditulis seolah sudah diterapkan |

---

## 1. Masalah yang diperbaiki

**Pertama, catatan perkembangan yang dibuat dari SOAP konsultasi tidak pernah sampai ke lembar
terpadu pasien.** Dokter menekan terbitkan CPPT dari lembar konsultasi rawat inap, server menjawab
`200 OK` dan menerbitkan nomor catatan, lalu catatan itu menghilang. Tab Catatan Terintegrasi tetap
kosong dan penghitungnya tetap nol.

Sebabnya satu baris yang tidak pernah ditulis. Jalur `POST .../from-consultation/{consultationId}`
membentuk sendiri entitas catatannya dan melewatkan pemetaan `InpEpisodeId`, sehingga kolom
perawatan tersimpan kosong. Lini masa satu perawatan menyaring dengan `InpEpisodeId = episodeId`,
dan catatan berkolom kosong tersaring keluar.

Ini bukan sekadar kolom yang hilang di layar. Lembar terpadu adalah tempat perawat jaga shift
berikutnya, farmasis, dan ahli gizi membaca instruksi dokter. Instruksi yang tidak terbaca di sana
adalah kegagalan komunikasi klinis, dan kegagalannya tidak terlihat oleh dokter yang menulis — ia
menerima notifikasi berhasil beserta nomor catatannya.

**Kedua, kolom Dokter / Penulis pada riwayat kajian medis selalu kosong.** Nama dokter dibaca
semata-mata dari antrean poliklinik (`Queue.Doctor`). Pasien rawat inap tidak pernah berantre —
`QueueId` mereka memang kosong — sehingga setiap kajian medis rawat inap mengembalikan
`doctorName: null` walaupun `doctorId`-nya terisi benar, dan layar menampilkannya sebagai tanda
hubung. Dokumen rekam medis yang tampil tanpa nama penanggung jawab medis membingungkan verifikator
rekam medis dan perawat yang membacanya.

**Ketiga (`ISS-09`), penamaan master kelas pasien dan pembedaan kelas hak penjamin dari kelas bed.**
Tidak dikerjakan pada task ini; alasannya ada pada bagian 5 dan 7.

---

## 2. Proses bisnis

**Tujuan.** Seluruh catatan perkembangan dan kajian medis satu perawatan rawat inap terbaca oleh
seluruh Profesional Pemberi Asuhan pada perawatan itu, lengkap beserta nama penulisnya.

**Pelaku.** DPJP sebagai penulis; perawat, farmasis, dan ahli gizi sebagai pembaca lembar terpadu;
verifikator rekam medis sebagai pemeriksa kelengkapan dokumen.

**Pemicu.** Dokter menyelesaikan SOAP konsultasi rawat inap lalu menerbitkan CPPT dari SOAP itu;
atau siapa pun membuka tab Kajian Pasien pada lembar kerja dokter.

**Langkah berurutan sesudah perbaikan — penerbitan CPPT dari SOAP:**

1. Dokter menekan terbitkan CPPT dari lembar konsultasi.
2. Backend membaca konsultasinya, menolak bila konsultasi itu sudah pernah menerbitkan CPPT.
3. Profesi penulis diambil dari tautan akun, bukan dari payload, lalu diperiksa terhadap jenis
   catatan yang hendak dibuat. Tidak ada perubahan pada langkah ini.
4. **Perawatan rawat inap yang menaungi konsultasi diturunkan** — stempel pada konsultasinya lebih
   dulu, lalu kunjungannya sebagai cadangan.
5. Catatan disimpan beserta perawatan itu, bersama baris keutuhannya, dalam satu `SaveChanges`.
6. `GET .../episodes/{episodeId}` menemukan catatan tersebut, dan seluruh PPA membacanya pada lembar
   yang sama.

**Aturan yang berlaku.**

- Stempel perawatan pada konsultasi didahulukan. Konsultasi rawat inap distempel perawatannya sejak
  `BE-RWI-043`, dan stempel itu tetap terbaca sesudah perawatannya ditutup.
- Penurunan dari kunjungan hanya cadangan, dipakai untuk konsultasi lama yang lahir sebelum stempel
  itu ada. Cadangan ini hanya mengenali perawatan yang **masih berjalan** — sifat yang diwarisi dari
  `FindOpenEpisodeIdAsync` dan sengaja tidak diubah pada task ini.
- Catatan rawat jalan, IGD, dan medical check-up tetap kosong perawatannya. Kunjungan mereka memang
  tidak menaungi perawatan rawat inap, sehingga kedua langkah tidak menemukan apa pun untuk diisi.
- Nama dokter pengkaji dibaca dari dokter yang **tertulis pada kajian**, sehingga `doctorId` dan
  `doctorName` pada satu respons selalu menyebut orang yang sama. Antrean tetap menjadi cadangan
  bagi baris lama yang hanya punya antrean tanpa `DoctorId`.
- Dokter yang sudah dihapus tidak disaring saat namanya dibaca. Nama pada dokumen rekam medis harus
  tetap terbaca sesudah dokternya tidak lagi aktif.

**Status yang dihasilkan.** Tidak ada status yang berubah. Kedua perbaikan hanya mengisi kolom dan
ruas jawaban yang sebelumnya kosong.

**Jalur tidak normal.**

- Konsultasi tidak ditemukan → `404`, seperti sebelumnya.
- Konsultasi sudah pernah menerbitkan CPPT → `400`, seperti sebelumnya.
- Konsultasi tanpa stempel perawatan dan kunjungannya tidak menaungi perawatan berjalan →
  `InpEpisodeId` tetap kosong, dan itu memang jawaban yang benar untuk catatan non-rawat-inap.
- Kajian tanpa `DoctorId` dan tanpa antrean → `doctorName` tetap `null`. Tidak ada nama yang ditebak.

**Hasil akhir.** CPPT dari SOAP muncul pada lini masa perawatan yang benar, dan riwayat kajian medis
menampilkan nama dokter pengkajinya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/issues/issue-002-perbaikan-anomali-cppt-dan-kajian-medis.md`
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`
- `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` — rujukan
  pola penurunan `InpEpisodeId` saat konsultasi lahir
- `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` —
  `FindOpenEpisodeIdAsync`, `IsOpen`
- `Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs`,
  `TrxPatientAssessment.cs`, `TrxPatientIntegratedProgressNote.cs`
- `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs`
- `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstDoctor.cs`
- `Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeReadDtos.cs` — penilaian `ISS-09`
- `Migrations/ApplicationDbContextModelSnapshot.cs` — index pada `TrxPatientIntegratedProgressNote`
- `Migrations/scripts/README.md` beserta skrip `repair-*.sql` yang sudah ada
- Frontend, dibaca saja tanpa perubahan:
  `src/lib/services/health-services/clinical-management/patient-integrated-progress-note.service.js`,
  `src/components/view/health-services/inpatient-management/physician-workspace/tabs/assessment/medical-assessment-history.jsx`,
  `medical-assessment-tab.jsx`,
  `src/utils/health-services/inpatient-management/inpatient-medical-assessment-utils.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../Controllers/PatientIntegratedProgressNoteController.cs` | `ISS-07`. `CreateFromConsultation` mengisi `InpEpisodeId` pada entitas catatan. Penurunannya dipisah ke `ResolveInpEpisodeIdForConsultationAsync` — stempel pada konsultasi lebih dulu, lalu `FindOpenEpisodeIdAsync` dari kunjungannya. `BuildRequestFromConsultation` ikut menyalin `consultation.InpEpisodeId`, dan `BuildDraftFromConsultation` memakai penurunan yang sama supaya draft yang dibaca klien menyebut perawatan yang sama dengan yang akan tersimpan |
| `.../Controllers/PatientAssessmentController.cs` | `ISS-08`. `DoctorName` tidak lagi bergantung pada antrean. Penurunannya dipisah ke `ResolveDoctorName`, yang memakai kamus nama dokter hasil `ResolveDoctorNamesAsync` — satu pembacaan `MstDoctor` untuk seluruh halaman, bukan satu per baris. Kelima endpoint baca dialiri kamus yang sama |
| `Migrations/scripts/repair-cppt-inpepisode-from-consultation-20260923.sql` | **Baru.** `ISS-07` butir 3. Mengisi `InpEpisodeId` pada CPPT lama yang telanjur tersimpan kosong. Dua langkah — dari stempel konsultasinya, lalu dari kunjungan bagi konsultasi lama yang belum distempel dan hanya bila kunjungan itu menaungi tepat satu perawatan. **Hanya mengisi kolom yang masih kosong**; tidak ada `DROP`, `DELETE`, maupun `TRUNCATE` |
| `Migrations/scripts/README.md` | Skrip di atas didaftarkan pada tabel daftar skrip |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Tidak berubah.** Tidak ada properti baru, endpoint baru, perubahan status code, maupun perubahan bentuk payload. Yang berubah adalah **nilai** dua ruas yang sudah lama ada pada kontrak: `inpEpisodeId` pada jawaban pembuatan CPPT dari konsultasi, dan `doctorName` pada jawaban kajian pasien — keduanya sebelumnya `null` karena cacat, kini terisi. `contract_version` tetap `0.6.1` |
| Database | Tidak ada perubahan schema, entity, maupun migration. Kolom `InpEpisodeId` sudah ada sejak `BE-RWI-040`, dan ketiga index yang memuatnya **tidak unik**, sehingga pengisiannya tidak dapat menabrak constraint apa pun. Nama dokter dibaca dari `MstDoctor` tanpa navigation property baru — menambahkannya akan melahirkan foreign key dan migration untuk sesuatu yang dapat dijawab satu pembacaan per halaman. Status migration: `NOT APPLICABLE`. Skrip perbaikan data historis disediakan tetapi **belum dijalankan** |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada perubahan pada `[AccessAction]`, `[AccessPermission]`, `AccessTypes`, penjaga profesi, penjaga unit perawat, maupun penjaga kewenangan dokter. Hak akses `PatientIntegratedProgressNote : Create` dan `PatientAssessment : Read` tetap persis seperti sebelumnya |

---

## 4. Dokumentasi endpoint

#### `Health Services / Clinical Management / Patient Integrated Progress Notes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/from-consultation/{consultationId}` | Menerbitkan CPPT dari SOAP konsultasi dokter. Jawaban kini membawa `inpEpisodeId` yang terisi untuk konsultasi rawat inap | `PatientIntegratedProgressNote : Create` |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/draft-from-consultation/{consultationId}` | Menyusun draft CPPT dari SOAP tanpa menyimpan. Draft kini menyebut perawatan yang sama dengan yang akan tersimpan | `PatientIntegratedProgressNote : Read` |
| `GET` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}` | Lini masa CPPT seluruh PPA pada satu perawatan. Tidak diubah; kini menemukan catatan yang sebelumnya tersaring keluar | `PatientIntegratedProgressNote : Read` |

#### `Health Services / Clinical Management / Patient Assessments`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments` | Daftar kajian pasien. `doctorName` kini terisi untuk kajian rawat inap | `PatientAssessment : Read` |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/{id}` | Detail satu kajian. `doctorName` kini terisi | `PatientAssessment : Read` |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/episodes/{episodeId}` | Kajian satu perawatan rawat inap. `doctorName` kini terisi | `PatientAssessment : Read` |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/active-by-encounter/{encounterId}` | Kajian aktif satu kunjungan. `doctorName` kini terisi | `PatientAssessment : Read` |
| `GET` | `/api/v1/health-services/clinical-management/patient-assessments/active-by-queue/{queueId}` | Kajian aktif satu antrean. Nilainya tidak berubah — jalur ini selalu punya antrean | `PatientAssessment : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran akar masalah `ISS-07` pada source | Terbukti. `CreateFromConsultation` membentuk entitas sendiri tanpa `InpEpisodeId`; `CreateProgressNote` mengisinya lewat `ResolveClinicalContextAsync`. Lini masa menyaring `x.InpEpisodeId == episodeId` | `PASS` | Pembacaan source kedua jalur pembuatan dan endpoint lini masa |
| Penelusuran akar masalah `ISS-08` pada source | Terbukti. `ToResponse` dan `ToDetailResponse` menurunkan `DoctorName` hanya dari `x.Queue.Doctor`, sedangkan `TrxPatientAssessment.QueueId` boleh kosong sejak `BE-IGD-028` | `PASS` | Pembacaan `ToResponse`, `ToDetailResponse`, dan entity |
| Penilaian risiko constraint atas pengisian `InpEpisodeId` | Aman. Ketiga index yang memuat `InpEpisodeId` pada `TrxPatientIntegratedProgressNote` **tidak unik**; tidak ada constraint yang dapat ditabrak baris yang kolomnya berubah dari kosong menjadi terisi | `PASS` | `Migrations/ApplicationDbContextModelSnapshot.cs` |
| Penilaian kebutuhan migration untuk nama dokter | Tidak diperlukan. Nama dibaca lewat satu query `MstDoctor` per halaman, bukan lewat navigation property baru yang akan menuntut foreign key dan migration | `PASS` | Pembacaan `TrxPatientAssessment`, yang tidak punya navigasi `Doctor`, dan `MstDoctor` |
| Penilaian dampak pada jalur rawat jalan dan IGD | Tidak berubah. `ResolveInpEpisodeIdForConsultationAsync` mengembalikan kosong bila kunjungan tidak menaungi perawatan; `ResolveDoctorName` jatuh ke antrean persis seperti perilaku lama ketika `DoctorId` kosong | `PASS` | Pembacaan cabang pada kedua helper |
| Penilaian kebutuhan perubahan frontend | Tidak ada. Layar riwayat kajian sudah merender `item.doctorName` dengan fallback `-`, dan lembar kerja sudah memanggil `getProgressNotesByEpisode` | `PASS` | Pembacaan `medical-assessment-history.jsx`, `medical-assessment-tab.jsx`, `patient-integrated-progress-note.service.js` |
| Nama tabel yang disebut `ISSUE-DOK-002` | **Tidak akurat.** Dokumen isu menyebut `CliPatientIntegratedProgressNote`; nama sebenarnya `public."TrxPatientIntegratedProgressNote"`. Skrip perbaikan data memakai nama yang sebenarnya | `PASS` | `[Table]` pada entity dan `ToTable` pada snapshot |
| `dotnet msbuild QuilvianSystemBackend.csproj -t:Compile -p:Configuration=Debug` | Berhasil, exit code `0`, **`0 error`**, `224 warning` | `PASS` | Keluaran perintah; dijalankan dua kali, yang kedua sesudah kedua berkas yang diubah disentuh ulang supaya seluruh assembly benar-benar dikompilasi ulang |
| Pemeriksaan warning baru pada berkas yang diubah | **Tidak ada warning baru.** Dari 224 warning, hanya **satu** yang berada di kedua berkas yang disunting: `CS8602` pada `PatientAssessmentController.cs` baris `2973`, yaitu `.ThenInclude(x => x.Doctor)` di dalam `BuildBaseQuery` — baris yang **tidak disentuh task ini** dan hanya bergeser nomornya | `PASS` | Keluaran perintah disaring pada kedua nama berkas, lalu dibandingkan dengan diff |
| `dotnet build QuilvianSystemBackend.csproj -p:Configuration=Debug` | Dijalankan. **Kompilasi tidak menghasilkan satu pun error** (`0 error CS`), tetapi build berhenti pada langkah penyalinan: `MSB3027` dan `MSB3021` — `bin\Debug
et9.0\QuilvianSystemBackend.exe` dikunci proses `QuilvianSystemBackend (PID 6608)` yang sedang berjalan, sesudah 10 kali percobaan ulang | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah menyebut nama dan PID proses pengunci. Kedua error bertipe MSBuild file-copy, bukan diagnostik compiler; langkah `CoreCompile` sendiri dilewati karena keluarannya sudah mutakhir dari target `Compile` sebelumnya |
| `POST .../from-consultation/{id}` menyimpan `InpEpisodeId` terisi | Belum dijalankan | `NOT RUN` | Menunggu backend dijalankan pemilik |
| `GET .../episodes/{episodeId}` mengembalikan CPPT tersebut | Belum dijalankan | `NOT RUN` | Sama seperti di atas |
| `GET /patient-assessments` mengembalikan `doctorName` terisi | Belum dijalankan | `NOT RUN` | Sama seperti di atas |
| `repair-cppt-inpepisode-from-consultation-20260923.sql` dijalankan pada database | Belum dijalankan | `NOT RUN` | Eksekusi database memerlukan wewenang terpisah; skripnya tersedia dan aman diulang |

### Review penutup (`REVIEW_RULES`)

| Butir review | Hasil |
| --- | --- |
| Kesesuaian QBE | Tidak ada QBE ID yang berlaku — tidak ada entity, tabel, kolom, endpoint, maupun rename yang dibuat. Lihat bagian 0 |
| Review diff | Tiga berkas source/skrip berubah: `PatientAssessmentController.cs` (+95/−9), `PatientIntegratedProgressNoteController.cs` (+53/−0), `Migrations/scripts/README.md` (+1/−0), ditambah satu berkas SQL baru. Seluruh diff hanya mengisi `InpEpisodeId` dan `DoctorName` beserta helper penurunannya |
| Review scope | Bersih. **Tidak ada** `.csproj`, `.sln`, `appsettings*`, `Program.cs`, workflow `.github`, `Dockerfile`, dependency, migration, maupun keluaran hasil generate yang tersentuh |
| Review regresi | (1) Kelima pemanggil `ToResponse`/`ToDetailResponse` sudah dialiri kamus nama dokter; tidak ada pemanggil lain di luar controller ini. (2) `NursingAssessmentDocumentService.EnrichDetailAsync` diperiksa — hanya menyentuh `InstrumentResults` dan `ReferencedVitalSign`, **tidak menimpa** `DoctorName`. (3) Jalur rawat jalan dan IGD memakai cabang cadangan yang perilakunya sama persis seperti sebelumnya. (4) Tidak ada route, status code, atau bentuk payload yang berubah |
| Pemeriksaan rahasia | Bersih. Tidak ada credential, token, connection string, key, maupun nilai konfigurasi sensitif pada berkas yang berubah maupun pada laporan ini. Skrip SQL tidak memuat kredensial; cara menjalankannya ditulis sebagai contoh perintah `psql` tanpa kata sandi |
| Dampak berkas bersama | Tidak ada. Kedua controller milik `ClinicalManagement`; tidak ada komponen bersama, konfigurasi, atau konsumen lintas repository yang berubah. Frontend diperiksa dan **tidak** memerlukan perubahan |
| Perintah pada skrip SQL | Diverifikasi dengan pencacahan: `1 BEGIN`, `1 COMMIT`, `8 SELECT`, `2 UPDATE`. **Nol** `DROP`, `DELETE`, `TRUNCATE`, `ALTER`, maupun `CREATE` — sesuai klaim pada bagian 3.2 |
| Status Git akhir | Lihat bagian 7 |

Uji manual: `REQUIRED` — perlu dijalankan pemilik sesudah proses backend dihentikan dan build diulang.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan:**

- Seluruh verifikasi runtime. Kompilasi sudah terbukti `PASS` dan `dotnet build` penuh sudah
  **dicoba** — yang menggagalkannya bukan kode, melainkan `bin` yang dikunci proses backend
  `PID 6608` yang sedang berjalan. Biner hasil perbaikan karena itu belum dapat dijalankan, dan
  perilakunya ditulis apa adanya sebagai `NOT RUN`, bukan disimpulkan lulus.
- Skrip perbaikan data historis. Disediakan, belum dieksekusi.
- **`ISS-09` tidak dikerjakan.** Tiga alasan, seluruhnya di luar kendali task ini. *Pertama*, butir
  1-nya adalah pembersihan **data induk** — menghapus atau mengganti nama record kelas pasien
  bernama `"UNIQUE"` — dan itu eksekusi database pada master data milik modul Administrator, bukan
  perubahan source. *Kedua*, butir 2-nya menyentuh kontrak DTO milik sub-modul
  `episode-rawat-inap`, bukan `dokter-rawat-inap`. *Ketiga*, `ISSUE-DOK-002` sendiri tidak
  mengalokasikan task untuk `ISS-09` dan tidak menuliskan acceptance criteria untuknya. Temuan yang
  perlu diketahui pemiliknya: **kontrak episode sebenarnya sudah memisahkan kedua kelas itu secara
  struktur** — kelas hak pada `InpatientEpisodeDetailResponse.PatientClassId`/`PatientClassName`,
  kelas bed pada `InpatientEpisodeCurrentLocationResponse.PatientClassId`/`PatientClassName`. Yang
  belum ada adalah **pembedaan nama ruasnya**; keduanya sama-sama bernama `PatientClassName`, dan
  itulah yang membuat layar terbaca membingungkan. Pekerjaan tersisa karena itu adalah pembersihan
  data induk ditambah penamaan ulang ruas, dan keduanya milik pemilik lain.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `ISS-07` — `POST .../from-consultation/{id}` menghasilkan baris ber-`InpEpisodeId` terisi | Terpenuhi pada source | `InpEpisodeId` dipetakan pada entitas; penurunannya diperiksa pada kedua cabang. Runtime `NOT RUN` |
| `ISS-07` — `GET .../episodes/{episodeId}` mengembalikan catatan yang baru dibuat | Terpenuhi pada source | Penyaring lini masa tidak diubah; yang diperbaiki adalah nilai yang disaringnya. Runtime `NOT RUN` |
| `ISS-07` — kartu CPPT terlihat pada lini masa dan penghitungnya bertambah | Belum terbukti | Bagian tampilan; tidak ada perubahan frontend yang diperlukan, tetapi pembuktiannya menunggu backend dijalankan |
| `ISS-08` — `GET /patient-assessments` mengembalikan `doctorName` berisi nama lengkap | Terpenuhi pada source | Nama dibaca dari `MstDoctor` menurut `DoctorId` kajian. Runtime `NOT RUN` |
| `ISS-08` — `GET /patient-assessments/{id}` mengembalikan `doctorName` terisi | Terpenuhi pada source | Jalur detail memakai penurunan yang sama. Runtime `NOT RUN` |
| `ISS-08` — tabel Riwayat Kajian Medis menampilkan nama dokter | Belum terbukti | Layar sudah merender `doctorName`; pembuktiannya menunggu backend dijalankan |
| CPPT historis yang telanjur kosong ikut diperbaiki | Belum terpenuhi | Skrip tersedia dan aman diulang, tetapi **belum dijalankan** |

Butir yang belum terpenuhi bertumpu pada dua hal yang sama-sama di luar lingkup tulis task ini:
backend belum dijalankan ulang, dan eksekusi database belum diberi wewenang. Tidak satu pun
dinyatakan lulus tanpa bukti.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi `PASS` dengan `0 error` dan `224 warning`. **Tidak ada warning baru.** Satu-satunya warning pada berkas yang disunting — `CS8602` di `BuildBaseQuery` — sudah ada sebelum task ini dan barisnya tidak disentuh. Perubahan tidak menambah tipe, dependency, maupun pemakaian API yang usang |
| Masalah yang diketahui | (1) `ISSUE-DOK-002` menyebut tabel `CliPatientIntegratedProgressNote`; nama sebenarnya `TrxPatientIntegratedProgressNote`. Perlu dibetulkan pada dokumen isunya supaya skrip yang ditulis orang lain tidak mencari tabel yang tidak ada. (2) **`FE-RWI-096` yang dialokasikan `ISSUE-DOK-002` sudah dipakai sub-modul `episode-rawat-inap` pada tanggal yang sama** — laporannya ada di `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/frontend/FE-RWI-096.md` dan roadmap-nya mencatat `task_id_next_free: FE-RWI-097`. ID frontend untuk isu ini karena itu **harus `FE-RWI-097`**, bukan `FE-RWI-096` |
| Risiko tersisa | (1) Verifikasi runtime belum dijalankan; kompilasi sudah `PASS` dan satu-satunya penghalang build penuh adalah proses backend lama yang mengunci `bin`. (2) Cadangan penurunan perawatan hanya mengenali perawatan yang **masih berjalan**; CPPT baru dari konsultasi tanpa stempel pada perawatan yang sudah ditutup karena itu tidak dapat dipulihkan lewat jalur aplikasi, dan hanya dapat diperbaiki lewat skrip data — yang memang menangani kasus itu karena membaca `InpEpisode` tanpa menyaring statusnya. (3) Kunjungan yang menaungi lebih dari satu perawatan sengaja dilewati skrip; sisanya dilaporkan skrip itu sendiri pada baris `sesudah` |
| Perubahan sampingan | `NONE`. Berkas lain yang berubah pada working tree — roadmap hemodialisa, `episode-rawat-inap`, dan laporan pengujian — sudah ada sebelum task ini dan tidak disentuh |
| Interupsi | `NONE` |
| Status Git | **Hasil task ini.** Diubah: `M Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`, `M Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs`, `M Migrations/scripts/README.md`, `M docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap-v2.md`, `M docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/requirement-traceability-v2.md`. Baru: `?? Migrations/scripts/repair-cppt-inpepisode-from-consultation-20260923.sql`, `?? docs/module-blueprints/rawat-inap/dokter-rawat-inap/task/report/backend/BE-RWI-128.md`. **Sudah ada sebelum task ini** dan tidak disentuh: `?? .../roadmap/issues/issue-002-...md` (dokumen isu, hanya disunting isinya — berkasnya memang belum pernah di-commit), `?? .../testing/test-by-agy/laporan-verifikasi-issue-001.md`, serta perubahan pada `hemodialisa`, `episode-rawat-inap`, dan `keperawatan`. Tidak ada stage, commit, maupun push yang dilakukan |
| Langkah berikutnya | (1) Pemilik menghentikan proses `QuilvianSystemBackend (PID 6608)`, menjalankan `dotnet build`, lalu menjalankan ulang skenario CPPT dari SOAP dan riwayat kajian medis ujung ke ujung. (2) Jalankan `repair-cppt-inpepisode-from-consultation-20260923.sql` sesudah wewenang database diberikan, lalu periksa baris `sesudah` pada keluarannya. (3) Betulkan nama tabel dan ID task frontend pada `ISSUE-DOK-002`. (4) Putuskan pemilik dan alokasi task untuk `ISS-09` |
