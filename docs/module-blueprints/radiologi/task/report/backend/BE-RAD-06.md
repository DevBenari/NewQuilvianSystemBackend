# Laporan Perubahan Backend — `BE-RAD-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-06` |
| Judul | Gerbang keselamatan menilai `RuleStatus` |
| Slice | `S3` — Penilaian gerbang keselamatan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-005`, `RJ-BIL-DEC-014`; state matrix `RAD-STATE-001` bagian 5; validation matrix `RAD-VAL-001` bagian 4; kemampuan `RAD-CAP-015` |
| Contract version | `RAD-STATE-001` dan `RAD-VAL-001` — status `approved` 2026-09-10, pada blueprint `RAD-BP-001` revision 9 |
| Dependency | `BE-RAD-01` — **terpenuhi** 2026-09-10; kolom `RuleStatus` sudah ada dan migration-nya sudah diterapkan ke `QuilvianNewDevYoga` |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/Services/`, `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Radiology/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `0e2eb105` |
| Tanggal | 2026-09-10 |
| Status | **Selesai.** Build lulus 0 error; `RadiologySafetyGateTests` 20 lulus. Uji integrasi Postgres tidak dapat dijalankan karena database test belum dikonfigurasi — keadaan lingkungan yang sudah ada sebelum task ini |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Tidak berlaku |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` sejak 2026-09-10 |
| Keberlakuan | `TOUCHED LEGACY` — menyunting service dan evaluator yang sudah berjalan; tidak ada entity, modul, prefix, maupun migration baru |
| QBE ID yang berlaku | `QBE-MOD-001` capability tetap di modul pemiliknya; `QBE-SVC-001` logika domain tetap di service dan controller tidak menyentuh context; `QBE-VAL-001` invariant bisnis ditegakkan backend; `QBE-AUD-001` audit database tidak disentuh |
| QBE ID yang **tidak** berlaku | Seluruh aturan `QBE-ENT-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, dan `QBE-DB-*` — tidak ada entity, penamaan, configuration, nomor bisnis, maupun pekerjaan database yang dibuat task ini |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, sistem memakai **dua penanda berbeda** untuk menjawab satu pertanyaan
yang sama: aturan keselamatan mana yang berlaku hari ini.

- Penjaga tabrakan di database sudah memakai `RuleStatus` — hasil `BE-RAD-01`.
- Gerbang keselamatan yang benar-benar menahan pasien masih memakai `IsActive` — kolom lama.

Untuk data yang ada sekarang keduanya sepakat, karena `BE-RAD-01` mengisi `RuleStatus`
**dari** `IsActive`. Masalahnya baru muncul pada aturan yang lahir sesudahnya.

> **Contoh nyata yang akan terjadi tanpa perubahan ini.** `BE-RAD-02` membuka alur pengesahan.
> Penanggung jawab klinis mengesahkan aturan "skrining kehamilan wajib untuk CT-Scan", dan
> aturan itu tersimpan dengan `RuleStatus = Active`. Karena alur pengesahan tidak berurusan
> dengan kolom lama, `IsActive` pada baris itu tidak ikut diisi. Gerbang keselamatan — yang
> masih membaca `IsActive` — **tidak melihat aturan itu sama sekali**. Bagi sistem, CT-Scan
> tersebut seolah tidak punya aturan apa pun.

Arah kekeliruannya inilah yang berbahaya. Aturan yang baru saja disahkan justru menjadi aturan
yang paling mungkin terlewat, dan yang terlewat adalah pertanyaan yang menahan penyinaran pada
pasien yang mungkin sedang hamil.

Perubahan ini menutup celah tersebut **sebelum** `BE-RAD-02` membuka jalur pembuatan aturan,
bukan sesudahnya.

---

## 2. Proses bisnis

**Tujuan.** Memastikan hanya aturan keselamatan yang sudah disahkan penanggung jawab klinis
yang boleh menentukan seorang pasien layak disinari atau tidak.

**Pelaku.**

| Pelaku | Perannya pada alur ini |
| --- | --- |
| Admin Radiologi | Menyusun draf aturan; drafnya tidak berpengaruh pada pemeriksaan yang sedang berjalan |
| Penanggung jawab klinis | Mengesahkan aturan; sejak disahkan, aturan itu mulai menahan |
| Radiografer | Menjawab butir keselamatan lalu memulai acquisition |

**Pemicu.** Petugas membuka sebuah study radiologi dan hendak memulai pengambilan citra.

**Langkah berurutan.**

1. Study dibuat. Sistem menyalin butir keselamatan yang berlaku menjadi daftar pertanyaan pada
   study tersebut. **Yang disalin hanya butir dari aturan berstatus `Active`.**
2. Petugas memverifikasi identitas pasien, kunjungan, pemeriksaan, dan alat.
3. Petugas menjawab setiap butir keselamatan.
4. Petugas menyatakan gerbang tuntas. Sistem menilai ulang, bukan mempercayai langkah 3.
5. Petugas memulai acquisition. Sistem **menilai ulang sekali lagi**, karena aturan dan jawaban
   dapat berubah di antara langkah 4 dan langkah 5.

**Aturan yang berlaku.**

| `RuleStatus` | Ikut dinilai? | Artinya bagi petugas |
| --- | :---: | --- |
| `Draft` | **Tidak** | Aturan masih disusun admin; belum menahan dan belum meloloskan |
| `PendingApproval` | **Tidak** | Sudah diajukan, belum disahkan; belum berlaku |
| `Active` | **Ya** | Inilah aturan yang berlaku hari ini |
| `Inactive` | **Tidak** | Pernah berlaku, sudah dihentikan |

**Urutan pemeriksaan saat memulai acquisition** — dipertahankan persis seperti tuntutan
`RAD-VAL-001` bagian 4:

| Urutan | Yang diperiksa | Bila gagal |
| ---: | --- | --- |
| 1 | Identitas pasien dan alat sudah diverifikasi | "...belum diverifikasi." |
| 2 | Ada aturan keselamatan yang **sudah disahkan** | "Aturan keselamatan untuk modalitas ini belum ditetapkan..." |
| 3 | Study berstatus `SafetyCleared` | "Acquisition ditolak: study berstatus X, bukan SafetyCleared." |
| 4 | Seluruh butir wajib tuntas | "Gerbang keselamatan wajib..." |

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Mengapa begitu |
| --- | --- | --- |
| Alat hanya punya aturan `Draft` | Acquisition **ditolak**; pesannya meminta menghubungi admin | Draf bukan kebijakan. Alat itu dibaca sama persis dengan alat yang belum punya aturan sama sekali |
| Alat hanya punya aturan `PendingApproval` | Sama seperti di atas | Pengajuan yang belum disahkan belum menjadi keputusan siapa pun |
| Aturan lama sudah `Inactive`, penggantinya masih `Draft` | Acquisition **ditolak** | Tidak ada kebijakan yang berlaku pada saat itu. Menolak lebih aman daripada memakai aturan yang sudah dicabut |
| Admin menambah draf baru sementara pemeriksaan berjalan | Pemeriksaan **tetap berjalan** | Draf tidak menahan pekerjaan yang sudah punya aturan sah dan tuntas |
| Study lama sudah lolos, aturannya kemudian dinonaktifkan | Penilaian lamanya **tetap sah** | Nomor versi aturan sudah dibekukan pada study saat ia dinyatakan lolos |

**Hasil akhir.** Aturan yang menentukan keselamatan pasien hanya berasal dari satu sumber, yaitu
`RuleStatus`, dan sumber itu hanya berisi aturan yang sudah melewati pengesahan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola.** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/README.md`;
`rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`;
`rules/backend/REVIEW_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`;
`rules/rule-output/aturan-output-dokumentasi.md`.

**Blueprint dan roadmap.** `roadmap/backend-roadmap.md`; `roadmap/requirement-traceability.md`;
`contracts/state-transition-matrix.md` bagian 5; `contracts/validation-matrix.md` bagian 4;
`testing/acceptance-test-matrix.md`; laporan `task/report/backend/BE-RAD-01.md`.

**Source.** `Services/RadStudyService.cs`; `Services/RadSafetyGateEvaluator.cs`;
`Models/MstRadModalitySafetyRule.cs`; `Enums/RadiologyEnums.cs`;
`Controllers/RadStudyController.cs`; `DTOs/RadiologyDtos.cs`;
`Repositories/Configurations/HealthServices/RadiologyManagement/MstRadModalitySafetyRuleConfiguration.cs`;
`Repositories/Configurations/HealthServices/MstDoctorServiceRuleConfiguration.cs` beserta
`Areas/HealthServices/MasterData/Controllers/DoctorServiceRuleController.cs` sebagai preseden
perbandingan enum di dalam query.

**Uji.** `Radiology/RadiologySafetyGateTests.cs`; `Radiology/RadiologyStudyLifecycleTests.cs`;
`Infrastructure/BillingTestDatabaseFixture.cs`.

Pencarian menyeluruh `MstRadModalitySafetyRule` pada seluruh source memastikan tidak ada
pembaca lain yang tertinggal — tidak ada seeder, controller, maupun service lain yang membaca
tabel ini.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs` | `LoadApplicableRulesAsync` menyaring `RuleStatus == Active`, bukan lagi `IsActive`. Penanda `HasActiveSafetyRule` pada `GetModalitiesAsync` memakai penyaring yang sama persis, supaya layar dan gerbang tidak pernah berbeda pendapat |
| `Areas/HealthServices/RadiologyManagement/Services/RadSafetyGateEvaluator.cs` | Menyingkirkan aturan yang belum disahkan sebelum apa pun dihitung; menambah `IsEnforceable(RadSafetyRuleStatus)` sebagai satu-satunya tempat arti "berlaku" dituliskan |
| `Tests/.../Radiology/RadiologySafetyGateTests.cs` | Perancah `Aturan(...)` menuliskan `RuleStatus` secara eksplisit; tujuh uji baru untuk AC-12 |
| `Tests/.../Radiology/RadiologyStudyLifecycleTests.cs` | Perancah `SiapkanAsync` dapat menyemai aturan pada status mana pun; satu uji baru — aturan `Draft` menolak acquisition sampai ke database |
| `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` | Baris keadaan `BE-RAD-06` beserta tautan laporan ini |
| `docs/module-blueprints/radiologi/roadmap/requirement-traceability.md` | Bukti pada `RJ-BIL-DEC-014`, `RAD-DEC-005`, `RAD-CAP-015`, dan slice `S3` |

**Mengapa penyaringnya ditegakkan dua kali.** Query sudah menyaring, dan penilai menyaring lagi.
Pengulangan itu disengaja, karena dua kekeliruan yang mungkin terjadi tidak setara nilainya:
aturan sah yang tidak sengaja tersaring keluar berakhir sebagai penolakan yang terlihat dan
dapat segera diperbaiki, sedangkan draf yang lolos masuk berakhir sebagai pasien yang disinari
atas dasar aturan yang belum disetujui siapa pun.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bentuknya tidak berubah.** Tidak ada route, verb, field, nilai enum, maupun status code yang ditambah, diganti nama, atau dihapus. Yang berubah adalah **isi jawaban** pada dua hal: `HasActiveSafetyRule` kini bernilai `true` hanya bila ada aturan yang sudah disahkan, dan gerbang menolak alat yang aturannya belum disahkan memakai `RAD_SAFETY_POLICY_NOT_CONFIGURED` yang sudah ada sebelumnya |
| Database | Tidak ada perubahan schema, entity, maupun index. **Tidak ada migration yang dibuat dan tidak ada perintah database yang dijalankan.** Kolom `RuleStatus` sudah tersedia sejak `BE-RAD-01`, dan migration-nya sudah diterapkan ke `QuilvianNewDevYoga` pada 2026-09-10 |
| Keamanan/Auth | `NOT APPLICABLE` untuk authorization — `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, dan resolusi pengguna tidak disentuh. Dampaknya ada pada **keselamatan klinis**: gerbang menjadi lebih ketat, tidak lebih longgar. Setiap perubahan perilaku bergerak ke arah menolak |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Empat endpoint berikut **berubah perilakunya**, bukan bentuknya.
Seluruhnya berada di bawah `api/v1/health-services/radiology-management/rad-studies`.

#### Health Services / Radiology Management / Rad Study

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/modalities` | Melihat daftar alat beserta penanda apakah aturan keselamatannya sudah disahkan. Penanda `HasActiveSafetyRule` kini menghitung `RuleStatus = Active` | `RadStudy : Read` |
| `POST` | `/by-order/{radOrderId}` | Membuat study. Daftar pertanyaan keselamatan disalin hanya dari aturan yang sudah disahkan | `RadStudy : Create` |
| `POST` | `/{id}/clear-safety` | Menyatakan gerbang keselamatan tuntas. Aturan yang belum disahkan tidak ikut dinilai | `RadStudy : Safety` |
| `POST` | `/{id}/start-acquisition` | Memulai pengambilan citra. Gerbang dinilai ulang di sini; alat yang aturannya belum disahkan ditolak sebagai kebijakan yang belum ditetapkan | `RadStudy : Acquire` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter FullyQualifiedName~RadiologySafetyGateTests` | Build berhasil `EXIT=0`, 0 error; **20 lulus, 0 gagal**, 65 ms | `PASS` | Keluaran perintah; 13 uji lama ditambah 7 uji baru |
| 13 uji gerbang yang sudah ada | Seluruhnya tetap lulus; tidak ada assertion yang diubah | `PASS` | Diff berkas uji — hanya perancah `Aturan(...)` yang disentuh |
| Aturan `Draft` saja — kebijakan dibaca belum ada | Ditolak; `PolicyConfigured` bernilai `false`; pesan memuat "belum ditetapkan" | `PASS` | `AturanYangBelumDisahkan_DibacaSebagaiKebijakanYangBelumAda(Draft)` |
| Aturan `PendingApproval` saja | Ditolak, sama seperti di atas | `PASS` | Uji yang sama, kasus kedua |
| Aturan `Inactive` saja | Ditolak, sama seperti di atas | `PASS` | Uji yang sama, kasus ketiga |
| Butir sudah dijawab `Passed` tetapi aturannya `Draft` | Tetap **tidak** lolos | `PASS` | `JawabanYangSudahAdaTidakMenolongAturanYangBelumDisahkan` |
| Draf baru tidak menahan study yang aturan sahnya sudah tuntas | Lolos | `PASS` | `DrafBaruTidakMenahanStudyYangAturanSahnyaSudahTuntas` |
| Versi yang dibekukan dihitung hanya dari aturan sah | `RuleVersion` bernilai `2`, bukan `9` milik draf | `PASS` | `VersiYangDibekukanDihitungHanyaDariAturanYangDisahkan` |
| `IsEnforceable` hanya menerima `Active` | Empat nilai enum diperiksa seluruhnya | `PASS` | `HanyaActiveYangDapatDitegakkan` |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan 200 warning build; seluruhnya warning dokumentasi XML lama pada modul MedicalRecord, OperatingRoom, Pharmacy, dan Registration, ditambah konflik versi `Microsoft.Extensions.DependencyModel` pada project uji |
| `dotnet test --filter FullyQualifiedName~RadiologyStudyLifecycleTests` | **Gagal seluruhnya, 16 uji**, pada `BillingTestDatabaseFixture.InitializeAsync`, sebelum satu pun badan uji berjalan | `EXISTING / ENVIRONMENT ISSUE` | `BLOCKED_BY_TEST_DB_CONFIGURATION: environment variable QUILVIAN_BILLING_TEST_DB belum diisi`. Sudah demikian sebelum task ini — `BE-RAD-01` mencatat hal yang sama pada 2026-09-10 |

Uji manual: `NOT FEASIBLE` — belum ada layar radiologi yang dapat dijalankan; `FE-RAD-08`
belum dikerjakan.

### Batas verifikasi yang perlu diketahui

Dua penyaring baru berjalan di database, bukan di C#: `LoadApplicableRulesAsync` dan penanda
`HasActiveSafetyRule`. Keduanya **tidak dapat dibuktikan** di lingkungan ini karena tidak ada
database test yang dikonfigurasi.

Yang dapat dipastikan tanpa database:

| Yang dipastikan | Dasarnya |
| --- | --- |
| Penyaringnya dapat diterjemahkan ke SQL | Pola yang sama sudah berjalan di repository ini: `MstDoctorServiceRule` juga memakai `HasConversion<int>()`, dan `DoctorServiceRuleController` membandingkan `x.RuleStatus == DoctorServiceRuleStatus.Suspended` di dalam query |
| Penilai menolak aturan yang belum disahkan | Dibuktikan langsung tujuh uji tanpa database |
| Tidak ada pembaca lain yang tertinggal | Pencarian menyeluruh `MstRadModalitySafetyRule` hanya menemukan dua tempat pembacaan, dan keduanya diubah |

Yang **belum** dapat dipastikan: perilaku kedua query terhadap PostgreSQL sungguhan. Pemeriksaan
itu menunggu database test dikonfigurasi, dan sebaiknya dilakukan sebelum `BE-RAD-15` mengisi
data master awal.

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Uji integrasi Postgres radiologi | Membutuhkan `QUILVIAN_BILLING_TEST_DB`. Variable itu sengaja tidak diisi: fixture menerapkan migration dan menulis baris nyata, sehingga mengisinya adalah wewenang terpisah milik pemilik modul |
| Pembuatan atau penerapan migration | Task ini tidak menyentuh schema sama sekali |
| Penghapusan kolom `IsActive` | Pekerjaan tersendiri; lihat bagian 7 |
| Penyesuaian index `{ModalityId, IsActive}` | Perubahan index menuntut migration, dan migration bukan wewenang task ini. Lihat bagian 7 |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-12 — aturan `Draft` atau `PendingApproval` tidak ikut dinilai gerbang | **Terpenuhi** | Tujuh uji baru pada `RadiologySafetyGateTests`, seluruhnya tanpa database sebagaimana diminta `testing/acceptance-test-matrix.md` |
| Study dengan satu aturan `Draft` saja tetap ditolak | **Terpenuhi** | `AturanYangBelumDisahkan_DibacaSebagaiKebijakanYangBelumAda`, ditambah uji setingkat database yang sudah ditulis tetapi belum dapat dijalankan |
| Sifat fail-closed tidak berubah | **Terpenuhi** | Ketiadaan aturan dan aturan yang belum disahkan sama-sama berakhir `PolicyConfigured = false`. Setiap perubahan perilaku bergerak ke arah menolak |
| Urutan pemeriksaan `RAD-VAL-001` bagian 4 dipertahankan | **Terpenuhi** | `StartAcquisitionAsync` tidak disentuh; hanya himpunan aturan yang dimuatnya yang menyempit |
| `RadiologySafetyGateTests` lulus tanpa perubahan | **Terpenuhi sebagian** | 13 uji lama lulus dan **tidak satu pun assertion-nya diubah**; yang diubah hanya perancah pembuat aturan. Lihat penjelasan di bawah |
| `RadiologyStudyLifecycleTests` lulus tanpa perubahan | **Belum terbukti** | Tidak dapat dijalankan karena database test belum dikonfigurasi. Perancah penyemainya juga diubah, dengan alasan yang sama seperti di bawah |

### Penyimpangan dari Definition of Done, disebut apa adanya

Definition of Done pada roadmap menuntut kedua berkas uji lulus **tanpa perubahan**. Tuntutan
itu **tidak dapat dipenuhi secara harfiah**, dan alasannya perlu dicatat terbuka.

Kedua berkas uji membuat aturan keselamatan dengan menuliskan `IsActive = true` saja. Itu cara
lama menyatakan "aturan ini berlaku", ditulis sebelum `BE-RAD-01` memindahkan penentunya ke
`RuleStatus`. Nilai bawaan `RuleStatus` pada entity adalah `Draft` — sengaja, karena aturan
memang lahir sebagai draf. Akibatnya setiap aturan yang dibuat perancah lama sebenarnya berupa
**draf**, dan begitu gerbang berhenti membaca `IsActive`, perancah itu tidak lagi menghasilkan
aturan yang berlaku.

Yang diubah karena itu hanya **data perancahnya**, yaitu satu baris `RuleStatus` pada
masing-masing pembuat aturan. Tidak ada satu pun `Assert` yang diubah, dilemahkan, atau
dihapus, dan tidak ada skenario yang dibuang. Maksud setiap uji tetap sama persis; hanya cara
menyatakan "aturan ini sudah disahkan" yang mengikuti keputusan `RAD-DEC-005`.

Roadmap sendiri menuntut dua hal yang tidak dapat berdiri bersamaan: AC-12 wajib dibuktikan
**uji tanpa database** — yang berarti penilaiannya harus terjadi di `RadSafetyGateEvaluator` —
sementara berkas uji penilai itu diminta tidak berubah sama sekali. Yang dipilih di sini adalah
memenuhi AC-12 beserta contract `RAD-STATE-001` bagian 5, lalu mencatat penyimpangannya, bukan
melemahkan gerbang supaya perancah lama tetap utuh.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 200 warning, seluruhnya sudah ada sebelumnya: warning dokumentasi XML pada modul MedicalRecord, OperatingRoom, Pharmacy, dan Registration, ditambah konflik versi `Microsoft.Extensions.DependencyModel` pada project uji. **Tidak ada warning baru dari berkas radiologi** |
| Masalah yang diketahui | `IsActive` masih ada pada tabel aturan keselamatan dan kini benar-benar tidak menentukan apa pun. Kolom itu tetap dipertahankan demi kompatibilitas; penghapusannya adalah pekerjaan tersendiri yang menuntut migration |
| Risiko tersisa | **Pertama**, dua penyaring baru berjalan di database dan belum pernah diuji terhadap PostgreSQL sungguhan. **Kedua**, index pendukung `{ModalityId, IsActive}` kini tidak lagi sejalan dengan penyaring yang dipakai gerbang, sehingga query aturan berpotensi kehilangan dukungan index; ini persoalan kecepatan, bukan kebenaran, dan perbaikannya menuntut migration. **Ketiga**, database pengembangan lain dan production belum menerima migration `BE-RAD-01`, sehingga perubahan ini belum boleh dinaikkan ke sana lebih dulu |
| Temuan di luar cakupan | `RadSafetyGateEvaluator` mengabaikan baris jawaban yang sudah dihapus (`IsDelete`), tetapi tidak mengabaikan **aturan** yang sudah dihapus. Untuk saat ini tidak berakibat apa pun, karena query pemuatnya sudah menyaring `IsDelete`. Sengaja tidak diubah: `BE-RAD-06` hanya memberi wewenang atas `RuleStatus`. Dicatat agar dapat ditimbang pada `BE-RAD-02` atau `BE-RAD-03` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-02` — service siklus pengesahan. Celah yang membuat task ini didahulukan sudah tertutup: aturan apa pun yang dibuat alur pengesahan nanti hanya akan menahan pemeriksaan setelah benar-benar disahkan. Sebelum `BE-RAD-15`, pemilik modul perlu mengonfigurasi database test agar uji integrasi radiologi dapat berjalan |

### Perubahan yang bukan milik task ini

Perubahan berikut sudah ada di worktree sebelum task ini dimulai dan **sengaja dibiarkan
utuh**: berkas `BE-RAD-01` — `RadiologyEnums.cs`, `MstRadModalitySafetyRule.cs`,
`MstRadModalitySafetyRuleConfiguration.cs`, snapshot model, dan dua berkas migration baru —
serta `EmergencyOrderKind.cs`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, seluruh berkas blueprint
radiologi selain dua berkas roadmap yang diperbarui task ini, dan berkas blueprint modul Rekam
Medis.

### Status Git pada akhir pekerjaan

Berkas yang berubah karena `BE-RAD-06`:

```text
 M Areas/HealthServices/RadiologyManagement/Services/RadSafetyGateEvaluator.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs
 M Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Radiology/RadiologySafetyGateTests.cs
 M Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Radiology/RadiologyStudyLifecycleTests.cs
```

Ditambah tiga berkas dokumentasi yang berada di dalam folder yang **belum terlacak Git** —
`docs/module-blueprints/radiologi/roadmap/` dan `docs/module-blueprints/radiologi/task/`
keduanya muncul sebagai `??` karena berasal dari pekerjaan perencanaan hari yang sama dan belum
pernah di-commit:

```text
docs/module-blueprints/radiologi/roadmap/backend-roadmap.md         — baris keadaan BE-RAD-06
docs/module-blueprints/radiologi/roadmap/requirement-traceability.md — bukti pada empat baris
docs/module-blueprints/radiologi/task/report/backend/BE-RAD-06.md   — laporan ini, berkas baru
```

Tidak ada `git add`, commit, maupun push yang dilakukan.
