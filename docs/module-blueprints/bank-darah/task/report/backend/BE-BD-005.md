# Laporan Perubahan Backend — `BE-BD-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-005` |
| Judul | Golongan darah pasien diperiksa dan divalidasi |
| Slice | `MVP-0` — jalur terbuka, tidak menyentuh number-series |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` §5 |
| Trace | `DEC-BD-015`, `DEC-BD-018`, `DEC-BD-026`, `DEC-BD-039` · `BD-AGG-04`, `BD-DOM-09/10/21`, `BD-XINV-04` · `INV-BD-014`, `INV-BD-018` · `contracts/api-contract.md` §Blood Group Exam · `contracts/state-transition-matrix.md` §4 · `contracts/validation-matrix.md` (`VAL-BD-030`, `VAL-BD-034`, `VAL-BD-037`) |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` ✅, `G2b` ✅ — nol dependency task. **`G4` tidak berlaku**, lihat bagian 3.4 |
| Klasifikasi | `HEAVY` — tiga entity baru, satu enum baru, delapan endpoint, satu migration, aturan konflik lintas-baris, risiko klinis tinggi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BloodBankManagement/**`, `Repositories/**`, `Migrations/**`, `Tests/**`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` cabang `sukmagp` |
| Tanggal | `2026-09-09` |
| Status | **`SELESAI`** untuk scope task. Satu delta kontrak dan tiga gap dicatat di bagian 7 dan 8 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, Quilvian **tidak punya hasil pemeriksaan golongan darah sama sekali**. Yang
ada hanya kolom `MstPatient.BloodType` — data induk administratif yang diisi saat pendaftaran, dari
wawancara atau salinan kartu identitas, **bukan** dari pemeriksaan laboratorium.

Akibatnya nyata dan berbahaya. Kolom itu tampak seperti golongan darah pasien, sehingga siapa pun
yang membangun alur transfusi di atasnya akan mengira sudah memakai data klinis. Kekeliruan ini
sudah tercatat sendiri sebagai `CONF-BD-005` dan ditutup `DEC-BD-015`: sumber sah golongan darah
adalah hasil pemeriksaan milik Bank Darah, dan `MstPatient.BloodType` **tidak pernah** menjadi
sumber klinis (`INV-BD-014`).

**Contoh konkretnya.** Pasien mendaftar dan menyebut golongan darahnya O. Petugas pendaftaran
mengisi `MstPatient.BloodType = OPositive`. Enam bulan kemudian pasien perlu transfusi. Tanpa
perubahan ini, tidak ada satu pun tempat di sistem yang dapat menjawab pertanyaan *"apa golongan
darah pasien ini menurut pemeriksaan?"* — yang ada hanya ingatan pasien yang diketik petugas
pendaftaran.

Masalah kedua yang lebih halus: **apa yang terjadi ketika dua pemeriksaan berselisih.** Tanpa aturan
yang ditulis, sistem yang lazim akan menimpa hasil lama dengan hasil baru. Untuk golongan darah, itu
persis tindakan yang salah — perbedaan hasil ABO adalah tanda ada yang keliru (tabung tertukar,
label salah, atau pemeriksaan meleset), dan menimpanya menyembunyikan tanda itu.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Menghasilkan **satu** golongan darah sah per pasien yang benar-benar berasal dari pemeriksaan, dan menahan pemakaiannya ketika hasilnya meragukan |
| Pelaku | Petugas pengambil sampel · Pemeriksa · Petugas BDRS berwenang validasi |
| Pemicu | Pasien perlu golongan darah sah, biasanya karena akan menjalani alur Bank Darah |
| Hasil akhir | Golongan darah sah yang dapat dibaca seluruh gerbang klinis, **atau** keadaan tertahan yang menutup gerbang itu |

### 2.2 Langkah normal, berurutan

| No | Langkah | Pelaku | Status sesudahnya | Yang tersimpan |
| ---: | --- | --- | --- | --- |
| 1 | Ambil sampel, tulis identifier pada tabung | Petugas pengambil | `SampleTaken` | Pasien, identifier sampel, pengambil, waktu |
| 2 | Catat hasil ABO dan Rhesus | Pemeriksa | `ResultRecorded` | Hasil, pemeriksa, waktu pemeriksaan |
| 3 | Validasi hasil rutin | Petugas BDRS berwenang validasi | `Validated` | Validator, waktu validasi |
| 4 | Sistem menilai perbedaan | Sistem | — | Hasil berlaku, **atau** keadaan tertahan |

Pada langkah 4 sistem hanya punya empat kemungkinan, dan tidak ada kemungkinan kelima:

| Keadaan pasien sebelumnya | Hasil baru | Akibat |
| --- | --- | --- |
| Belum punya hasil sah | apa pun | Hasil baru langsung berlaku |
| Punya hasil sah, nilainya **sama** | sama | Hasil terbaru berlaku, **tanpa penahanan apa pun** (`AC-BD-035`) |
| Punya hasil sah, nilainya **berbeda** | berbeda | **Keduanya ditahan**; pasien tidak punya hasil sah (`AC-BD-034`, `BD-XINV-04`) |
| Sedang menahan perbedaan | apa pun | Hasil baru tervalidasi sebagai **kandidat pemeriksaan ulang**; perbedaan **tetap tertahan** |

### 2.3 Jalur tidak normal

**Hasil berselisih — inilah jalur yang paling penting.** Contoh berangka:

> Pasien punya hasil sah **O Positif** dari 1 Agustus. Pada 9 September sampel baru diperiksa dan
> hasilnya **A Positif**, lalu divalidasi. Sistem **tidak memilih** mana yang benar. Yang terjadi:
> kedua pemeriksaan ditandai tertahan, pasien tercatat **tidak punya golongan darah sah**, dan setiap
> gerbang klinis yang menuntutnya tertutup. Kedua nilai — O Positif dan A Positif — **tetap tersimpan
> apa adanya**; tidak satu pun ditimpa atau dihapus.

Penutupannya bukan bagian task ini; ia dikerjakan `BE-BD-011` dan menuntut pemeriksaan ulang yang
tervalidasi ditambah pernyataan validator klinis (`DEC-BD-031`).

**Hasil belum divalidasi.** Hasil yang sudah dicatat tetapi belum divalidasi **tidak pernah** menjadi
golongan darah sah. Ia terbaca di layar sebagai hasil tercatat, tetapi `GET /patient/{id}/valid`
memulangkan nilai kosong (`BD-DOM-09`).

**Percobaan menimpa hasil tervalidasi.** Ditolak. Koreksi hasil dilakukan lewat pemeriksaan ulang,
bukan lewat penyuntingan (`DEC-BD-026`).

**Identifier sampel kembar.** Ditolak dengan `409`. Dua tabung berlabel sama membuat penelusuran dari
pasien ke hasil putus.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `rules/GLOBAL_RULES.md` · `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` · `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` · `rules/backend/{TASK_RULES,API_RULES,DATABASE_RULES,REPORT_TEMPLATE,role-access-rules,transaction-endpoint-standard,master-data-endpoint-standard}.md` |
| Kontrak modul | `roadmap/backend-roadmap.md` · `contracts/api-contract.md` · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` · `contracts/permission-audit-matrix.md` · `data/data-dictionary.md` · `02-backend-architecture.md` · `03-domain-architecture.md` · `03-frontend-architecture.md` · `00-interview-decisions.md` · `flowcharts/pemeriksaan-golongan-darah.md` |
| Pola source terdekat | `BloodStorageLocationController.cs` + `Service` + `Dtos` (pola master Bank Darah) · `LabSpecimenController.cs` (pola aksi transaksi `POST /{id}/<aksi>`) · `TrxPatientEncounter.cs` (pola FK ke `MstPatient`) · `MstBloodBankReason.cs` (kategori alasan) · `Enums/BloodType.cs` · `Attributes/AccessControllerAttribute.cs` · `Constants/AccessTypes.cs` |
| Bukti gerbang `G4` | `BillingManagement/Billing/Services/BillingNumberSeriesService.cs` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkBloodGroupExamStatus.cs` | **Baru.** Tiga nilai: `SampleTaken`, `ResultRecorded`, `Validated`. Keadaan konflik sengaja **bukan** nilai enum |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodGroupExam.cs` | **Baru.** Aggregate root `BD-AGG-04`; memuat `PatientId`, hasil, penanda `IsValidResult` dan `IsConflictHeld`, token `Version` |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodGroupSample.cs` | **Baru.** Sampel Bank Darah, `SampleIdentifier` wajib dan unik |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodGroupConflictResolution.cs` | **Baru.** Disiapkan di task ini karena satu migration; **dipakai** `BE-BD-011` |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodGroupExamDtos.cs` | **Baru.** Permintaan dan balasan seluruh endpoint, termasuk `ValidBloodGroupDto` |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodGroupExamService.cs` | **Baru.** Pemilik seluruh baca/tulis; memegang aturan perbedaan hasil `BD-XINV-04` |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodGroupExamController.cs` | **Baru.** Delapan endpoint task ini + satu milik `BE-BD-011` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodGroupExamConfiguration.cs` | **Baru.** Empat index + FK `MstPatient` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodGroupSampleConfiguration.cs` | **Baru.** Index unik `SampleIdentifier` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodGroupConflictResolutionConfiguration.cs` | **Baru.** FK `Restrict` ke pemeriksaan pemutus dan ke `MstPatient` |
| `Repositories/ApplicationDbContext.cs` | Tiga `DbSet` ditambahkan pada region `BLOOD BANK MANAGEMENT` + satu `using` |
| `Program.cs` | `AddScoped<BbkBloodGroupExamService>()` + satu `using` |
| `Migrations/20260909015603_AddBbkBloodGroupExam.cs` | **Baru.** Tiga `CreateTable` + index. **Belum dijalankan** |
| `Tests/.../BankDarah/BloodGroupExam/BloodGroupExamServiceTests.cs` | **Baru.** 29 pengujian |
| `Tests/.../BankDarah/MasterData/BloodBankRoleAccessContractTests.cs` | Controller baru didaftarkan; cakupan butir hak akses **12 → 17**; satu pengujian penjaga `DEC-BD-039` ditambahkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif murni.** Delapan endpoint baru pada base URL baru; nol endpoint existing berubah, berganti nama, atau hilang. Dua endpoint di luar daftar kontrak `v4` — lihat delta di bagian 7 |
| Database | Tiga tabel baru — `BbkBloodGroupExam`, `BbkBloodGroupSample`, `BbkBloodGroupConflictResolution`. Nol tabel existing diubah. Migration `20260909015603_AddBbkBloodGroupExam` **dibuat, BELUM dijalankan** |
| Keamanan/Auth | Lima butir hak akses baru lahir lewat `AccessMenuSeeder`. Pemisahan `Validate` dari `ResolveConflict` (`DEC-BD-039`) ditegakkan dua butir berbeda. **Nol pemeriksaan nama peran, nama jabatan, atau `UserType` di dalam kode.** `AboRhesusResult` adalah data klinis dan **tidak ikut tertulis ke log** |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` |
| Submodule | `NOT APPLICABLE` — modul tanpa sub-modul |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → prefix **`Bbk`** |
| Status registry | **`ACTIVE`** (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 29; `PLANNED` → `ACTIVE` commit `8075784`) |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-001` folder di bawah pemiliknya · `QBE-MOD-002`/`QBE-MOD-003` registry `ACTIVE` sebelum entity pertama · `QBE-NAM-002`/`QBE-NAM-004` prefix `Bbk` dari registry, bukan disimpulkan dari folder · `QBE-NAM-001` nol `Trx*` · `QBE-SVC-001` controller nol akses `ApplicationDbContext` langsung |
| `QBE-CODE-001/002/003` | **Tidak berlaku pada task ini.** Nol nomor bisnis dialokasikan di seluruh slice — lihat di bawah |
| Gerbang `G4` | **Tidak mengenai task ini.** Diperiksa ulang ke bukti di `f0d6855`: `BillingNumberSeriesService` masih memaparkan empat method publik yang seluruhnya dipatok kunci `BILLING_*`, dan `AllocateNumberAsync` generiknya masih `private` (baris 141) — nol pintu masuk untuk Bank Darah. Karena slice ini tidak mengalokasikan nomor apa pun, ketiadaan provider itu tidak menahannya |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Group Exam

Base URL: `api/v1/health-services/blood-bank-management/blood-group-exams`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi penyaring dan pengurutan halaman pemeriksaan | `BloodGroupExam : Read` |
| `GET` | `/summary` | Ringkasan jumlah per status, termasuk jumlah pasien yang perbedaannya tertahan | `BloodGroupExam : Read` |
| `GET` | `/` | Daftar pemeriksaan dengan penyaring, pencarian identifier sampel, dan halaman | `BloodGroupExam : Read` |
| `GET` | `/{id}` | Detail satu pemeriksaan beserta sampel dan aksi yang tersedia | `BloodGroupExam : Read` |
| `GET` | `/patient/{patientId}/valid` | Golongan darah sah pasien, atau penanda perbedaan tertahan | `BloodGroupExam : Read` |
| `POST` | `/` | Catat pengambilan sampel, membuka pemeriksaan baru | `BloodGroupExam : Create` |
| `POST` | `/{id}/result` | Catat hasil ABO dan Rhesus | `BloodGroupExam : Update` |
| `POST` | `/{id}/validate` | Validasi hasil **rutin**, sekaligus menilai perbedaan | `BloodGroupExam : Validate` |

Endpoint `POST /conflict-resolution` ada pada controller yang sama tetapi merupakan lingkup
`BE-BD-011`; lihat laporannya.

**Bentuk transaksi, bukan master data.** Sesuai standar endpoint transaksi, controller ini sengaja
**tidak** menyediakan `GET /options`, `PATCH /{id}/status` generik, maupun `DELETE /{id}`:
pemeriksaan yang sudah terjadi tidak dihapus, dan statusnya berpindah karena kejadian bernama.
`PUT /{id}` juga tidak dibuat — matriks perpindahan status `v4` tidak memuat satu pun transisi
"sunting", dan seluruh perubahan isi berjalan lewat aksi bernama.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | Berhasil — `0 Error(s)`, **`210 Warning(s)`** | `PASS` | Sama persis dengan baseline sebelum task (`0 Error(s)`, `210 Warning(s)`) — **nol peringatan baru** |
| `dotnet test` — `QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 419, Total: 419` | `PASS` | Keluaran perintah |
| `dotnet test --filter BankDarah` | `Failed: 0, Passed: 134, Total: 134` | `PASS` | Naik dari 102 sebelum task |
| `dotnet test --filter BloodGroupExamServiceTests` | `Failed: 0, Passed: 29, Total: 29` | `PASS` | 20 di antaranya membuktikan `BE-BD-005`; 9 sisanya `BE-BD-011` |
| `dotnet test` — `UnitTests.Sqlite` | `Failed: 0, Passed: 177, Total: 177` | `PASS` | Membangun model relasional, sehingga FK dan index ikut terperiksa |
| `dotnet test` — `UnitTests.InMemory` | `Failed: 9, Passed: 896, Total: 905` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilannya milik `BillingManagement`; lihat catatan di bawah |
| `dotnet ef migrations add` | Migration dibuat; `Up()` hanya 3 `CreateTable` + index, nol operasi merusak | `PASS` | `git diff --numstat` snapshot: `253` tambahan, **`0` penghapusan** |
| Eksekusi migration ke database | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; belum diminta |

**Sembilan kegagalan `UnitTests.InMemory` adalah keadaan bawaan, bukan akibat task ini.** Buktinya
tiga lapis: **(a)** seluruhnya di `QuilvianSystemBackend.Tests.BillingManagement.*` —
`BillingInvoiceServiceTests`, `BillingCalculationServiceTests`, `BillingFinalizationServiceTests`;
**(b)** `git status --short` menunjukkan **nol** berkas Billing tersentuh task ini, dan service
Billing terakhir berubah pada commit `c35f36b`/`a42b651`/`5dc874d` yang seluruhnya mendahului sesi
ini; **(c)** bentuk kegagalannya aturan bisnis Billing, misalnya
`NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` mengharapkan status `FINAL`
tetapi memperoleh `CLOSED`. Menambah tiga `DbSet` Bank Darah tidak dapat mengubah konstanta status
invoice. Perbaikannya milik pemilik Billing dan **di luar wewenang task ini**.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta database, sedangkan migration belum
dijalankan dan eksekusinya wewenang terpisah.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| `IntegrationTests.Postgres` | Menuntut database PostgreSQL yang berjalan; eksekusi database adalah wewenang terpisah |
| Eksekusi migration | Wewenang terpisah dan belum diminta. Index unik fisik `SampleIdentifier` karena itu belum terbukti di database |
| Uji lewat Swagger | `AccessPermissionService.HasAccessAsync` memulangkan `true` untuk SuperAdmin sebelum satu baris hak akses pun dibaca, sehingga cacat penamaan tidak akan terlihat. Penggantinya adalah contract test, yang memang dijalankan |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-030` — hasil tanpa pemeriksa atau waktu pemeriksaan ditolak | **Terpenuhi** | `Hasil_Dicatat_SelaluMenyimpanPemeriksaDanWaktuPemeriksaan`, `Hasil_PelakuTidakDikenali_Ditolak`. Ditegakkan secara struktural: pemeriksa dan waktu **tidak pernah diterima** dari pemanggil, sehingga tidak ada jalan mengosongkannya |
| `AC-BD-034` — hasil tervalidasi baru yang berbeda menahan keduanya | **Terpenuhi** | `Validasi_HasilBaruBerbeda_MenahanKeduanyaDanPasienKehilanganHasilSah` — memeriksa ketiganya: pasien tak punya hasil sah, gerbang tertahan, kedua hasil tetap tersimpan |
| `AC-BD-035` — hasil tervalidasi baru yang sama berlaku tanpa penahanan | **Terpenuhi** | `Validasi_HasilBaruSamaDenganHasilSah_BerlakuTanpaPenahanan` |
| `AC-BD-077` — petugas BDRS berwenang validasi memvalidasi hasil rutin | **Terpenuhi pada tingkat kontrak dan penegakan atribut** | `ValidasiRutinDanPenyelesaianKonflik_DijagaDuaButirBerbedaPadaController`. Endpoint `POST /{id}/validate` dijaga butir `BloodGroupExam : Validate` yang terpisah dari `ResolveConflict`, sehingga validasi rutin tidak menunggu Dokter BDRS |
| `AC-BD-078` — petugas berwenang validasi **tidak** dapat menutup konflik | **Terpenuhi pada tingkat kontrak dan penegakan atribut** | Butir yang berbeda pada dua endpoint yang berbeda; penegakan runtime-nya milik `AccessPermissionFilter`. Pembuktian ujung-ke-ujung menuntut aplikasi berjalan — lihat batas di bagian 8 |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Seluruh AC lulus | **Terpenuhi**, dengan batas pembuktian `AC-BD-077`/`AC-BD-078` yang disebut apa adanya di atas |
| Butir hak akses `Validate` terdaftar | **Terpenuhi** — lahir dari `[AccessAction("Validate", …)]` pada controller |
| Butir hak akses `ResolveConflict` terdaftar | **Terpenuhi** — lahir bersama endpoint `BE-BD-011` pada controller yang sama |
| Laporan tracked ditulis | **Terpenuhi** — berkas ini |

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Asal `SampleIdentifier` — diputuskan pemilik** | `03-domain-architecture.md:283` (`BD-DOM-10`) menulis *"Identifier sampel **terbitan sistem**"*, sedangkan `03-frontend-architecture.md:218` dan `00-interview-decisions.md:214` memperlakukannya sebagai isian petugas, dan `02-backend-architecture.md:487` menyebut daftar **tertutup** tiga field number-series yang tidak memuatnya. Roadmap memerintahkan builder berhenti dan melapor bila ternyata harus dibuat server. **Pertentangan ini dilaporkan sebelum satu baris kode ditulis, dan pemilik memutuskan `SampleIdentifier` ditulis petugas** (9 September 2026) | Frasa "terbitan sistem" di `BD-DOM-10` perlu dikoreksi agar tidak menyesatkan pembaca berikutnya. Konsekuensi keputusan ini: nol field pada slice ini memerlukan provider nomor, sehingga `G4` benar-benar tidak mengenainya |
| **Dua endpoint di luar daftar kontrak `v4`** | `GET /filters/metadata` dan `GET /summary` | Keduanya permukaan teknis baku standar endpoint transaksi, memakai pola yang sudah ada di source. Nol aturan bisnis baru diperkenalkan |
| **`BbkBloodGroupConflictResolution` lahir di task ini** | Entity milik `BE-BD-011` ikut dibuat pada migration `BE-BD-005` | Ketiga tabel berbagi satu migration supaya tidak ada dua migration berurutan yang menyentuh kelompok tabel yang sama. Endpoint dan aturannya tetap milik `BE-BD-011` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build bersih akhir menghasilkan **`210 Warning(s)`, sama persis dengan baseline** — nol peringatan baru. Build pertengahan sempat `211` karena satu `xUnit2031` pada berkas test baru (`Assert.Single` dengan klausa `Where`); temuan itu **diperbaiki**, bukan didiamkan, dengan memakai overload `Assert.Single(collection, predicate)` |
| Masalah yang diketahui | **(1)** Kontrak `v4` **belum menetapkan kategori alasan** untuk penyelesaian perbedaan golongan darah — kesepuluh kategori `MstBloodBankReason` seluruhnya menyangkut order dan kantong. Service menuntut kode alasan yang **ada dan aktif** tanpa memaksakan kategori yang belum diputuskan siapa pun. **(2)** `GET /{id}/status-history` tidak dibuat karena entity pendukungnya `BbkTransitionHistory` (`BD-DOM-15`) berada di luar scope task ini. **(3)** Index unik fisik `SampleIdentifier` belum terbukti karena migration belum dijalankan; pengujian memakai provider InMemory |
| Risiko tersisa | **Migration belum dijalankan**, sehingga seluruh endpoint ini belum dapat dipakai di lingkungan mana pun. Menjalankannya kini menuntut koordinasi lintas modul karena migration Bank Darah berselang-seling dengan migration Laboratorium, Billing, dan Registration |
| Temuan di luar scope | `MstBillingItemCategory` **sudah tidak ada di model** tetapi migration `20260527165204` yang membuatnya masih ada, sehingga model dan migration Billing tidak sinkron. Terbukti bawaan: `git show HEAD:…ModelSnapshot.cs` juga tidak memuatnya. **Tidak disentuh** — milik pemilik Billing |
| Perubahan sampingan | `NONE` pada hasil akhir. Selama pengerjaan, `dotnet ef migrations remove` sempat merusak snapshot antara sehingga migration pertama ikut menjadwalkan pembuatan ulang tabel milik modul lain. Migration itu **dibuang**, snapshot dipulihkan dari `HEAD`, lalu migration dibuat ulang dari snapshot bersih. Hasil akhirnya terbukti additive murni: `253` tambahan, `0` penghapusan |
| Interupsi | Dua kali. **(1)** Proses `VBCSCompiler` bocor hingga 10 GB dan menggantung build; dihentikan, lalu build diulang dengan `-p:UseSharedCompilation=false` dan berhasil dalam 3 detik. **(2)** Sesi terputus saat build; dilanjutkan dari keadaan terverifikasi lewat `git status` tanpa penyuntingan ganda |
| Status Git | Lihat bagian 9 |
| Langkah berikutnya | **(1)** `BE-BD-011` — sudah dikerjakan bersama task ini, lihat laporannya. **(2)** Koreksi frasa `BD-DOM-10` pada `03-domain-architecture.md`. **(3)** Putuskan kategori alasan penyelesaian perbedaan golongan darah. **(4)** Jadwalkan eksekusi keempat migration Bank Darah sebagai satu tindakan terkoordinasi |

---

## 9. Status Git

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/BloodBankRoleAccessContractTests.cs
 M docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md
?? Areas/HealthServices/BloodBankManagement/
?? Migrations/20260909015603_AddBbkBloodGroupExam.Designer.cs
?? Migrations/20260909015603_AddBbkBloodGroupExam.cs
?? Repositories/Configurations/HealthServices/BloodBankManagement/
?? Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/BloodGroupExam/
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-005.md
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-011.md
?? docs/module-blueprints/bank-darah/task/report/frontend/
```

Satu baris **bukan milik task ini dan tidak disentuh**: `task/report/frontend/` berasal dari
pekerjaan frontend yang berjalan paralel di repository yang sama.

Dua berkas disunting **kedua pihak**, dan suntingan task ini sengaja dibatasi pada baris status
backend beserta tautan buktinya:

- `requirement-traceability.md` — bagian frontend oleh pekerjaan paralel, bagian backend oleh task ini;
- `frontend-roadmap.md` — task ini hanya menandai ulang `BE-BD-005`/`BE-BD-011` pada blok dependency
  dan membuka `FE-BD-009`, konsekuensi yang **sudah tertulis sendiri** di kartu task itu. `FE-BD-005`
  sengaja **dibiarkan ⛔** karena dependency-nya juga `BE-BD-007`/`BE-BD-008` yang tertahan `G4`, dan
  roadmap melarang memecahnya tanpa persetujuan pemilik.

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy` dijalankan
(`git stash list` kosong; `HEAD` tetap `f0d6855`).
