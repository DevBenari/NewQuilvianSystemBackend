# Laporan Perubahan Backend — `BE-LAB-06`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-06` |
| Judul | Pengelolaan alasan penolakan sampel |
| Slice | `S11` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 3, gelombang `MVP-0` |
| Trace | `FR-06.1` .. `FR-06.3`; `LAB-DEC-019`; `BR-15`; `LAB-INH-010`, `LAB-INH-011`; `VAL-36`, `VAL-37`, `VAL-38`; `AC-26` |
| Contract version | `LAB-API-v1` r3 grup Lab Rejection Reason, `LAB-PERM-v1` r3, `LAB-VAL-v1` r3 — seluruhnya `approved`, dikunci 2026-09-02 |
| Dependency | Tidak ada. Roadmap mencatat kolom Dependency bertanda `—` |
| Klasifikasi | `HEAVY` — skor 9. Repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan/auth 2, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, dan artefak blueprint modul Laboratorium |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `d8d67c3`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — kelima endpoint tersedia, seeder data awal ada, `VAL-36` sampai `VAL-38` terbukti, dan jalur baca lama tidak berubah perilakunya |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | Tidak ada |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` sejak 2026-09-02 lewat `LAB-REQ-002` |
| Status registry | Terdaftar dan `ACTIVE`. `QBE-MOD-002` dan `QBE-MOD-003` tidak menahan task ini |
| Keberlakuan | `NEW CODE` untuk controller, service, DTO, seeder, dan test. `MstLabRejectionReason` beserta configuration dan migrationnya adalah `UNTOUCHED LEGACY` — task ini tidak menyentuh satu baris pun di sana |
| QBE ID yang berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-CODE-004`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` .. `QBE-ENT-003` dan `QBE-CFG-001` — tidak ada entity baru. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — tidak ada `LEGACY MIGRATION` |
| Sumber governance yang dibaca | `AGENTS.md`; `rules/GLOBAL_RULES.md`; `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` `TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`, `master-data-endpoint-standard` |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif. Rules root runtime terbaca lengkap, dan salinan `docs/engineering/` pada repository ini identik isinya dengan rules root — hanya berbeda akhiran baris |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, daftar alasan penolakan sampel **hanya bisa dibaca, tidak bisa dikelola**.
Tabelnya sudah ada dan sudah dipakai petugas saat menolak wadah sampel, tetapi satu-satunya
jalan masuk ke sana adalah `GET /lab-specimens/rejection-reasons` — sebuah jalur baca. Tidak ada
layar, tidak ada endpoint, dan tidak ada cara sah bagi rumah sakit untuk menambah alasan baru.

Akibat nyatanya bagi pengguna:

> Kepala instalasi Pak Hendra memperhatikan bahwa dalam sebulan terakhir ada delapan wadah yang
> tiba di laboratorium tanpa label identitas pasien. Ia ingin mencatatnya sebagai alasan
> penolakan tersendiri, "Sampel tidak diberi label", supaya angkanya terlihat dan dapat
> dibicarakan dengan ruang rawat. Sebelum perubahan ini, permintaan itu harus menjadi tiket ke
> tim programmer dan menunggu rilis berikutnya. Petugasnya sementara memakai alasan "Lainnya",
> sehingga delapan kejadian yang punya satu sebab yang sama terkubur di dalam satu keranjang
> serba-ada.

Ada masalah kedua yang lebih halus dan lebih berbahaya. Tabel yang sama memuat dua kolom yang
**bukan sekadar penamaan**:

| Kolom | Yang sesungguhnya ditentukan kolom ini |
| --- | --- |
| Penanda kesalahan internal rumah sakit | Apakah pengambilan darah ulang **gratis** bagi pasien, atau boleh ditagihkan kepadanya |
| Penanda wajib disertai catatan | Apakah petugas dipaksa menuliskan bukti tambahan saat menolak, atau boleh menolak tanpa keterangan |

Kolom pertama memindahkan beban biaya. Menurut `LAB-INH-010`, akibat finansial bukan wewenang
Laboratorium. Kalau layar pengelolaan dibuat begitu saja tanpa pemisahan kewenangan, kepala
instalasi akan dapat menandai sebuah alasan sebagai "kesalahan internal" sendirian, dan sejak
saat itu setiap pengambilan ulang dengan alasan tersebut ditanggung rumah sakit — sebuah
keputusan keuangan yang diambil tanpa Billing pernah tahu.

Masalah ketiga adalah kekosongan data. Bila tabel alasan penolakan kosong di sebuah lingkungan
baru, petugas **tidak dapat menolak sampel sama sekali**. Yang terjadi kemudian bukan sistem
berhenti, melainkan yang lebih buruk: sampel yang jelas tidak layak tetap diperiksa, lalu
menghasilkan angka yang menyesatkan dokter.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Aspek | Isi |
| --- | --- |
| Tujuan | Alasan penolakan sampel dapat dikelola rumah sakit sendiri tanpa perubahan program, tetapi akibat biayanya tetap dipegang pihak yang berwenang |
| Pelaku 1 | **Kepala instalasi laboratorium** — pemegang `LabRejectionReason : Read`, `: Create`, dan `: Update` |
| Pelaku 2 | **Administrator sistem** — pemegang `LabRejectionReason : SystemFlag` |
| Pemicu | Muncul pola penolakan baru yang berulang, atau alasan lama tidak lagi terpakai, atau kesepakatan baru dengan Billing tentang siapa menanggung biaya |
| Hasil akhir | Daftar alasan penolakan yang terkini, dengan pembagian kewenangan yang tidak dapat dilangkahi salah satu pihak |

### 2.2 Langkah yang berurutan — kepala instalasi menambah alasan baru

1. Kepala instalasi membuka layar pengelolaan alasan penolakan. Layar memanggil
   `GET /lab-rejection-reasons`, yang menampilkan **seluruh** alasan — termasuk yang sudah
   dinonaktifkan, karena ia perlu tahu apa yang pernah ada sebelum menambah yang baru.
2. Ia menekan tambah, lalu mengisi tiga hal: kode alasan `UNLABELED_SPECIMEN`, nama
   "Sampel tidak diberi label", dan urutan tampil `10`.
3. Layar mengirim `POST /lab-rejection-reasons`.
4. Sistem menormalkan kodenya menjadi huruf kapital, memastikan kode itu belum dipakai, lalu
   menyimpannya dengan status **aktif** dan kedua penanda terkunci bernilai **tidak**.
5. Alasan itu langsung muncul pada `GET /lab-specimens/rejection-reasons`, sehingga petugas yang
   sedang menolak sampel dapat memakainya saat itu juga — tanpa menunggu apa pun.

Pada layar, kolom "kesalahan internal rumah sakit" tampil terkunci bertanda gembok. Ini bukan
hiasan: menurut `LAB-FE-012`, pengguna harus tahu sebelum mencoba, bukan setelah gagal
menyimpan.

### 2.3 Langkah yang berurutan — administrator sistem menyetel penanda biaya

1. Setelah alasan baru terbentuk, Billing dan Laboratorium bersepakat bahwa wadah tanpa label
   adalah kelalaian petugas rumah sakit, sehingga pengambilan ulangnya tidak boleh ditagihkan
   kepada pasien.
2. Administrator sistem memanggil `PUT /lab-rejection-reasons/{id}/system-flags` dengan
   penanda kesalahan internal bernilai benar, disertai alasan penyetelannya.
3. Sistem menyimpan perubahan itu **beserta catatan log** yang memuat nilai lama, nilai baru,
   siapa pelakunya, dan alasan yang ia tuliskan. Catatan inilah yang kelak menjawab pertanyaan
   "sejak kapan dan atas dasar apa alasan ini menjadi tanggungan rumah sakit".

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Kode | Aturan |
| --- | --- | :---: | --- |
| Kepala instalasi menyertakan penanda kesalahan internal atau penanda wajib catatan pada permintaan ubah biasa | Permintaan **ditolak seluruhnya**. Nama dan urutan yang ikut dikirim juga tidak tersimpan | `403` | `VAL-37` |
| Kode alasan yang dimasukkan sudah dipakai baris lain | Penyimpanan ditolak, tidak ada baris kembar yang terbentuk | `409` | `VAL-36` |
| Alasan yang hendak dinonaktifkan adalah satu-satunya yang masih aktif | Penonaktifan ditolak | `422` | `VAL-38` |
| Alasan yang dituju tidak ada atau sudah ditandai terhapus | Permintaan ditolak | `404` | — |
| Kode atau nama alasan dikirim kosong | Permintaan ditolak | `400` | — |

Dua catatan tentang perilaku di atas.

**Mengapa `VAL-37` menolak, bukan mengabaikan.** Permintaan ubah tetap *menerima* kedua penanda
terkunci, lalu menolaknya begitu salah satunya disertakan. Cara lain — diam-diam mengabaikan
ruas itu dan tetap menyimpan sisanya — terlihat lebih ramah, tetapi meninggalkan pemanggil
dalam keyakinan bahwa penanda biayanya sudah berubah padahal tidak. Keyakinan keliru itu justru
lebih berbahaya daripada penolakan yang terang-terangan. Pola yang sama sudah dipakai `VAL-28`
pada `LabValueBoundService` di `BE-LAB-04`.

**Mengapa `VAL-38` ada.** Menonaktifkan alasan terakhir tidak membuat sistem berhenti; ia membuat
petugas kehilangan cara menolak sampel. Sampel yang tidak layak lalu tetap diperiksa, dan
hasilnya masuk ke rekam medis seolah sah.

### 2.5 Contoh berangka — `VAL-38`

Misalkan tabel memuat tiga alasan aktif: `CLOTTED`, `INSUFFICIENT_QUANTITY`, dan `OTHER`.

| Langkah | Tindakan | Sisa alasan aktif | Hasil |
| ---: | --- | ---: | --- |
| 1 | Menonaktifkan `CLOTTED` | 2 | Berhasil |
| 2 | Menonaktifkan `INSUFFICIENT_QUANTITY` | 1 | Berhasil |
| 3 | Menonaktifkan `OTHER` | 0 | **Ditolak `422`** — "Sekurang-kurangnya satu alasan penolakan harus tetap aktif." |

Langkah 3 ditolak karena setelahnya tidak tersisa satu pun alasan aktif. Kepala instalasi yang
memang ingin mengganti seluruh daftar harus menambah alasan penggantinya lebih dulu, baru
menonaktifkan yang terakhir.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Kontrak dan keputusan modul**

- `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3 dan 8
- `docs/module-blueprints/laboratorium/roadmap/frontend-roadmap.md` — `FE-LAB-03`, untuk memastikan lima endpoint memang cukup
- `docs/module-blueprints/laboratorium/roadmap/traceability.md`
- `docs/module-blueprints/laboratorium/contracts/api-contract.md` bagian Lab Rejection Reason
- `docs/module-blueprints/laboratorium/contracts/validation-matrix.md` — `VAL-10` .. `VAL-12`, `VAL-36` .. `VAL-38`
- `docs/module-blueprints/laboratorium/contracts/permission-audit-matrix.md`
- `docs/module-blueprints/laboratorium/00-interview-decisions.md` — `BR-15`, `LAB-DEC-019`, `AC-26`
- `docs/module-blueprints/laboratorium/02-backend-architecture.md` bagian 4.8, 5, dan 8
- `docs/module-blueprints/laboratorium/testing/acceptance-test-matrix.md` — lima baris `AC-26`

**Source yang menjadi pola terdekat**

- `Areas/HealthServices/LaboratoryManagement/Controllers/LabValueBoundController.cs`
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabCriticalBoundApprovalController.cs`
- `Areas/HealthServices/LaboratoryManagement/Services/LabValueBoundService.cs`
- `Areas/HealthServices/LaboratoryManagement/Services/LabCriticalBoundApprovalService.cs`
- `Areas/HealthServices/LaboratoryManagement/DTOs/LabValueBoundDtos.cs`
- `Areas/HealthServices/PharmacyManagement/Seeders/PrescriptionReviewCriterionSeeder.cs`
- `Seeders/DefaultWorkScheduleSeeder.cs`

**Source yang menjadi bukti keadaan as-is**

- `Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs`
- `Repositories/Configurations/HealthServices/LaboratoryManagement/MstLabRejectionReasonConfiguration.cs`
- `Areas/HealthServices/LaboratoryManagement/Controllers/LabSpecimenController.cs` — jalur baca lama
- `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` — `GetRejectionReasonsAsync`
- `Migrations/20260824091610_AddLaboratorySpecimenLifecycle.cs` — pengisian baseline yang sudah ada
- `Repositories/ApplicationDbContext.cs`, `Program.cs`
- `Attributes/AccessPermissionAttribute.cs`, `Filters/AccessPermissionFilter.cs`, `Constants/AccessTypes.cs`
- `Responses/ApiResponse.cs`, `Responses/PagedResult.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabRejectionReasonDtos.cs` | **Baru.** Memuat `LabRejectionReasonPagedQuery`, `LabRejectionReasonResponse`, `CreateLabRejectionReasonRequest`, `UpdateLabRejectionReasonRequest`, `SetLabRejectionReasonActivationRequest`, dan `SetLabRejectionReasonSystemFlagsRequest` — seluruhnya bernama persis seperti `LAB-API-v1` r3 |
| `Areas/HealthServices/LaboratoryManagement/Services/LabRejectionReasonService.cs` | **Baru.** Lima operasi domain beserta penegakan `VAL-36`, `VAL-37`, `VAL-38`, dan tiga tipe exception yang dipetakan controller menjadi `409`, `403`, dan `422` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabRejectionReasonController.cs` | **Baru.** Lima endpoint beserta `[AccessAction]` dan `[AccessPermission]`, sehingga permissionnya terdaftar sendiri lewat `AccessMenuSeeder` |
| `Areas/HealthServices/LaboratoryManagement/Seeders/LabRejectionReasonSeeder.cs` | **Baru.** Mengisi sepuluh alasan baseline bila kodenya belum ada. Tidak pernah menimpa baris yang sudah tersimpan dan tidak menghidupkan kembali baris yang sudah dihapus |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs` | `LabRejectionReasonResponse` **dipindahkan** ke `LabRejectionReasonDtos.cs`. Namespacenya sama persis, sehingga tidak ada satu pun `using` maupun pemanggil yang perlu diubah |
| `Program.cs` | Registrasi `LabRejectionReasonService` sebagai scoped service, dan `LabRejectionReasonSeeder` pada daftar seeder startup setelah `AccessMenuSeeder` |
| `tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabRejectionReasonServiceTests.cs` | **Baru.** 34 uji: kelima baris `AC-26`, ketiga aturan validasi, bentuk kontrak kelima endpoint, keutuhan jalur baca lama, dan perilaku seeder |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah lima endpoint** pada grup Lab Rejection Reason, persis seperti `LAB-API-v1` r3. Statusnya berubah dari **Rencana (belum tersedia)** menjadi tersedia. Satu ruas `IsActive` ditambahkan pada `LabRejectionReasonResponse` — penambahan yang tidak merusak pemanggil lama. Rinciannya pada bagian 3.4 |
| Database | **Tidak ada dampak schema.** Tidak ada entity baru, tidak ada kolom baru, tidak ada index baru, dan **tidak ada migration yang dibuat maupun dijalankan**. Tabel `MstLabRejectionReason` beserta index uniknya sudah ada sejak `20260824091610_AddLaboratorySpecimenLifecycle`. Tidak ada satu pun perintah database yang dijalankan pada task ini |
| Keamanan/Auth | **Inti task ini.** Hak akses `LabRejectionReason : SystemFlag` diperkenalkan dan dipisahkan tegas dari `: Update`. Pemisahan itu ditegakkan pada dua lapis: `[AccessPermission]` pada endpoint penyetelan, dan `VAL-37` di dalam service yang menolak upaya menyelinapkan penanda terkunci lewat endpoint ubah biasa |

### 3.4 Selisih terhadap kontrak dan standar yang berlaku

Tujuh butir berikut adalah selisih yang **disengaja**, dicatat terbuka sesuai `REVIEW_RULES`.

| No | Selisih | Alasan |
| ---: | --- | --- |
| 1 | `rules/backend/master-data-endpoint-standard.md` mewajibkan sembilan endpoint baseline untuk master data; grup ini hanya punya lima | `LAB-API-v1` r3 mengunci tepat lima endpoint dan roadmap menyebut "Lima endpoint tersedia" pada DoD. Wewenang task eksplisit berada di atas dokumen standar menurut urutan presedensi `GLOBAL_RULES.md`. Empat endpoint yang tidak ada — `GET /filters/metadata`, `GET /summary`, `GET /options`, `GET /{id}` — juga tidak dikonsumsi `FE-LAB-03`, sehingga `QBE-OPT-001` justru melarang membuatnya sekarang. **Ditinggalkan sebagai keputusan pemilik blueprint bila kelak layar pengelolaan membutuhkannya** |
| 2 | Standar master data memakai `PATCH /{id}/status`; kontrak ini memakai `PUT /{id}/activation` | Bentuk route dikunci `LAB-API-v1` r3. Mengubahnya sepihak berarti mendefinisikan ulang kontrak yang sudah disetujui |
| 3 | Tidak ada `DELETE /{id}` | Alasan penolakan yang pernah dipakai menempel pada riwayat penolakan sampel lewat `TrxLabSpecimen.RejectionReasonId`. Ia dinonaktifkan, bukan dihapus. Kontrak juga tidak memuatnya |
| 4 | Standar master data menyatakan kode bisnis dialokasikan backend, bukan dikirim frontend; di sini kode alasan dikirim pemakainya | `BR-15` menetapkan "Kode alasan — Ya, saat membuat baru" sebagai wewenang kepala instalasi, dan baris baseline yang sudah ada memakai kode semantik seperti `IDENTITY_MISMATCH`, bukan nomor urut. Kode ini penanda teknis, bukan nomor bisnis, sehingga `QBE-CODE-001` .. `QBE-CODE-003` tidak berlaku. `QBE-CODE-004` tetap terpenuhi oleh index unik `IX_MstLabRejectionReason_ReasonCode` yang sudah ada |
| 5 | `LabRejectionReasonResponse` bertambah satu ruas `IsActive` | Layar pengelolaan wajib membedakan alasan aktif dan nonaktif agar tombol aktif/nonaktif punya arti. Penambahan ini aditif; jalur baca lama hanya mengembalikan baris aktif sehingga nilainya selalu benar di sana dan tidak ada pemanggil yang rusak |
| 6 | `LabRejectionReasonResponse` berpindah berkas dari `LabSpecimenDtos.cs` ke `LabRejectionReasonDtos.cs` | `02-backend-architecture.md` bagian 5 menetapkan `LabRejectionReasonDtos.cs` sebagai berkas milik grup ini. Namespacenya sama, sehingga perpindahan ini tidak mengubah satu pun pemanggil dan tidak mengubah perilaku apa pun |
| 7 | `SetLabRejectionReasonSystemFlagsRequest` memiliki ruas `ChangeReason` yang tidak disebut kontrak | Kontrak menamai DTO tanpa merinci ruasnya. Ruas ini opsional dan **tidak disimpan ke tabel**; ia hanya masuk catatan log, memenuhi `QBE-LOG-001` agar penyetelan penanda biaya dapat ditelusuri sebabnya |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Rejection Reason

Base URL: `api/v1/health-services/laboratory-management/lab-rejection-reasons`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar alasan penolakan untuk layar pengelolaan, termasuk yang sudah dinonaktifkan | `LabRejectionReason : Read` |
| `POST` | `/` | Menambah alasan penolakan baru | `LabRejectionReason : Create` |
| `PUT` | `/{id}` | Mengubah nama, keterangan, dan urutan tampil | `LabRejectionReason : Update` |
| `PUT` | `/{id}/activation` | Mengaktifkan atau menonaktifkan satu alasan | `LabRejectionReason : Update` |
| `PUT` | `/{id}/system-flags` | Menyetel penanda kesalahan internal dan penanda wajib catatan | `LabRejectionReason : SystemFlag` |

Bentuk permintaan dan tanggapan:

| Endpoint | Request | Response |
| --- | --- | --- |
| `GET /` | `LabRejectionReasonPagedQuery` — `PageNumber`, `PageSize`, `IsActive`, `Search` | `ApiResponse<PagedResult<LabRejectionReasonResponse>>` |
| `POST /` | `CreateLabRejectionReasonRequest` — `ReasonCode`, `ReasonName`, `Description`, `SortOrder` | `ApiResponse<LabRejectionReasonResponse>` |
| `PUT /{id}` | `UpdateLabRejectionReasonRequest` — `ReasonName`, `Description`, `SortOrder` | `ApiResponse<LabRejectionReasonResponse>` |
| `PUT /{id}/activation` | `SetLabRejectionReasonActivationRequest` — `IsActive` | `ApiResponse<LabRejectionReasonResponse>` |
| `PUT /{id}/system-flags` | `SetLabRejectionReasonSystemFlagsRequest` — `IsInternalHospitalError`, `RequiresNote`, `ChangeReason` | `ApiResponse<LabRejectionReasonResponse>` |

Kode status yang dikembalikan:

| Kode | Kapan muncul |
| :---: | --- |
| `200` | Permintaan berhasil |
| `400` | Kode atau nama alasan kosong |
| `403` | Permintaan ubah memuat penanda terkunci (`VAL-37`), atau pemanggil tidak memegang hak aksesnya |
| `404` | Alasan tidak ditemukan atau sudah ditandai terhapus |
| `409` | Kode alasan sudah dipakai data lain (`VAL-36`) |
| `422` | Menonaktifkan alasan aktif terakhir (`VAL-38`) |

#### Endpoint yang **tidak** berubah

`GET /api/v1/health-services/laboratory-management/lab-specimens/rejection-reasons` dengan hak
akses `LabSpecimen : Read` tetap berdiri apa adanya sebagai jalur baca bagi petugas yang sedang
menolak sampel. Route, verb, hak akses, penyaringan hanya-aktif, dan pengurutannya tidak
disentuh. Bentuk muatannya hanya bertambah satu ruas `IsActive`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)`, `186 Warning(s)` | `PASS` | Keluaran perintah. Tidak ada satu pun warning yang berasal dari berkas baru task ini — diperiksa dengan menyaring keluaran build pada kata `LabRejectionReason`, hasilnya kosong |
| `dotnet test --filter "FullyQualifiedName~LabRejectionReason"` | `Failed: 0, Passed: 34, Total: 34` | `PASS` | Keluaran perintah |
| Seluruh suite `QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 963, Total: 964` | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan adalah `BillingManagement.BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, milik modul Billing. Kegagalan yang sama sudah tercatat pada laporan [`BE-LAB-04`](BE-LAB-04.md) bagian 5 dan [`BE-LAB-05`](BE-LAB-05.md) bagian 5, jauh sebelum task ini |
| Seluruh suite `QuilvianSystemBackend.IntegrationTests.Postgres` | `Failed: 52, Passed: 34, Total: 86` | `EXISTING / ENVIRONMENT ISSUE` | Kelima puluh dua kegagalan memakai satu pesan yang sama: `BLOCKED_BY_TEST_DB_CONFIGURATION` karena environment variable `QUILVIAN_BILLING_TEST_DB` belum diisi. Penjaga itu sengaja dipasang setelah temuan `RJ-BIL-BE-002` supaya integration test tidak pernah lagi menerapkan migration ke database dev bersama. Menyediakan database test adalah wewenang terpisah yang tidak dimiliki task ini |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `Files evaluated: 34`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| `AC-26` — kepala instalasi menambah "Sampel tidak diberi label" | Alasan tersimpan, langsung aktif, kedua penanda bernilai bawaan | `PASS` | `AC26_KepalaInstalasiMenambahAlasanBaru_LangsungTersimpanDanAktif` |
| `AC-26` — penanda terkunci tidak dapat diisi dari permintaan pembuatan | `CreateLabRejectionReasonRequest` terbukti tidak punya kedua ruas itu | `PASS` | `AC26_PermintaanMembuatAlasan_TidakPunyaRuasPenandaTerkunciSamaSekali` |
| `AC-26` **gagal** — kepala instalasi mengubah penanda kesalahan internal | `LabRejectionReasonForbiddenException` (`403`), penanda tidak berubah | `PASS` | `VAL37_KepalaInstalasiMengubahPenandaKesalahanInternal_Ditolak` |
| `VAL-37` — penanda wajib catatan juga terkunci | `403`, penanda tidak berubah | `PASS` | `VAL37_KepalaInstalasiMengubahPenandaWajibCatatan_Ditolak` |
| `VAL-37` — penolakan bersifat menyeluruh | Nama dan urutan yang ikut dikirim juga **tidak** tersimpan | `PASS` | `VAL37_PermintaanDitolakSeluruhnya_NamaDanUrutanTidakIkutBerubah` |
| `AC-26` — administrator sistem menyetel penanda | Kedua penanda berubah, pelakunya tercatat pada `UpdateBy` dan pada logger | `PASS` | `AC26_AdministratorSistemMenyetelPenandaKesalahanInternal_Berhasil` |
| `AC-26` **gagal** — kode alasan ganda | `LabRejectionReasonConflictException` (`409`), tidak ada baris kedua | `PASS` | `VAL36_MenambahAlasanDenganKodeYangSudahDipakai_Ditolak` |
| `VAL-36` — kode ganda beda huruf besar-kecil | `409`; "clotted" dan "CLOTTED" terhitung kode yang sama | `PASS` | `VAL36_KodeYangSamaDenganHurufKecil_TetapDianggapGanda` |
| `AC-26` **gagal** — menonaktifkan alasan aktif terakhir | `LabRejectionReasonValidationException` (`422`), alasan tetap aktif | `PASS` | `VAL38_MenonaktifkanAlasanAktifTerakhir_Ditolak` |
| `VAL-38` — penonaktifan sah tetap berjalan | Berhasil selama masih ada alasan aktif lain | `PASS` | `MenonaktifkanAlasan_BerhasilSelamaMasihAdaYangLainAktif` |
| Bentuk kontrak — kelima endpoint | Route, verb, dan `[AccessPermission]` cocok satu per satu dengan `LAB-API-v1` r3; tepat satu endpoint menuntut `SystemFlag`; tidak ada jalur `DELETE` | `PASS` | `KelimaEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak`, `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak`, `ControllerPengelolaan_TidakMemilikiJalurHapus` |
| Jalur baca lama tidak berubah | Route `rejection-reasons`, verb `GET`, dan hak akses `LabSpecimen : Read` tetap sama; tujuh ruas asli muatannya utuh | `PASS` | `JalurBacaLama_RouteDanHakAksesnyaTidakBerubah`, `BentukTanggapanAlasanPenolakan_TetapMemuatSeluruhRuasAsli` |
| Seeder — mengisi tabel kosong | Sepuluh baris baseline tersimpan dan seluruhnya aktif | `PASS` | `Seeder_MengisiTabelKosongDenganSepuluhAlasanBaseline` |
| Seeder — dijalankan dua kali | Tidak ada baris kembar; penambahan kedua bernilai nol | `PASS` | `Seeder_DijalankanDuaKali_TidakMenambahBarisKembar` |
| Seeder — tidak menimpa keputusan pengguna | Nama, urutan, status aktif, dan penanda yang sudah diubah pengguna tetap seperti apa adanya | `PASS` | `Seeder_TidakMenimpaBarisYangSudahDiubahPengguna` |
| Seeder — tidak menghidupkan baris yang dihapus | Baris bertanda terhapus tidak diisi ulang | `PASS` | `Seeder_TidakMenghidupkanKembaliBarisYangSudahDihapus` |
| Seeder — identitas sama dengan migration | `Id` baris baseline sama persis dengan yang dipakai `20260824091610_AddLaboratorySpecimenLifecycle` | `PASS` | `Seeder_MemakaiIdentitasYangSamaDenganMigration` |
| Daftar pengelolaan — menampilkan yang nonaktif, menyaring, mengurutkan, dan mem-paging | Seluruhnya sesuai | `PASS` | Empat uji pada bagian 6 berkas uji |

Uji manual: `NOT FEASIBLE`. Menjalankan aplikasi dan menembak endpoint sungguhan menuntut
database berjalan; wewenang eksekusi database tidak diberikan pada task ini.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Penyaringan `Search` pada `GET /` | `EF.Functions.ILike` hanya ada pada provider PostgreSQL, sedangkan bukti ini berjalan di atas provider InMemory supaya tidak menuntut database mana pun. Jalur ini memakai pola yang sama persis dengan `LabValueBoundService.GetListAsync` yang sudah berjalan sejak `BE-LAB-04` |
| Penjaga terakhir `VAL-36` di tingkat database | Index unik fisik tidak ditegakkan provider InMemory. Yang diuji di sini adalah pemeriksaan di service. Index `IX_MstLabRejectionReason_ReasonCode` beserta filter `IsDelete = false` sudah ada sejak migration 2026-08-24 dan tidak disentuh task ini |
| `LaboratorySpecimenLifecycleTests.AlasanPenolakanOther_WajibDisertaiCatatan` | Termasuk 52 uji `QuilvianSystemBackend.IntegrationTests.Postgres` yang terhalang `QUILVIAN_BILLING_TEST_DB`. Uji inilah yang seharusnya membuktikan `VAL-12` pada jalur baca lama secara runtime; sebagai gantinya keutuhan jalur itu dibuktikan lewat bukti bentuk kontrak |
| Perintah database apa pun | Task ini tidak menyentuh schema, sehingga tidak ada migration yang perlu dibuat maupun dijalankan |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-26` — kepala instalasi dapat menambah alasan penolakan | **Terpenuhi** | `AC26_KepalaInstalasiMenambahAlasanBaru_LangsungTersimpanDanAktif`; alasan "Sampel tidak diberi label" tersimpan dan langsung aktif |
| `AC-26` — kepala instalasi dapat menonaktifkan alasan penolakan | **Terpenuhi** | `MenonaktifkanAlasan_BerhasilSelamaMasihAdaYangLainAktif` dan `MengaktifkanKembaliAlasanYangNonaktif_Berhasil` |
| `AC-26` — percobaan mengubah penanda kesalahan internal ditolak sistem | **Terpenuhi** | `VAL37_KepalaInstalasiMengubahPenandaKesalahanInternal_Ditolak`; `403`, dan penandanya terbukti tidak berubah |
| `AC-26` — percobaan mengubah penanda wajib catatan ditolak sistem | **Terpenuhi** | `VAL37_KepalaInstalasiMengubahPenandaWajibCatatan_Ditolak` |
| `AC-26` — administrator sistem dapat menyetel penanda itu, dan tercatat pada logger | **Terpenuhi** | `AC26_AdministratorSistemMenyetelPenandaKesalahanInternal_Berhasil`; `LabRejectionReason.SetSystemFlags` mencatat nilai lama, nilai baru, pelaku, dan alasannya |

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Lima endpoint tersedia | **Terpenuhi** | `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak` menghitung tepat lima endpoint ber-`[AccessPermission]`; masing-masing route dan verbnya diuji tersendiri |
| Seeder mengisi data awal | **Terpenuhi** | `Seeder_MengisiTabelKosongDenganSepuluhAlasanBaseline`; terdaftar di `Program.cs` setelah `AccessMenuSeeder` |
| `VAL-36` terbukti | **Terpenuhi** | Dua uji: kode ganda dan kode ganda beda huruf besar-kecil |
| `VAL-37` terbukti | **Terpenuhi** | Tiga uji: penanda kesalahan internal, penanda wajib catatan, dan sifat menyeluruh penolakannya |
| `VAL-38` terbukti | **Terpenuhi** | `VAL38_MenonaktifkanAlasanAktifTerakhir_Ditolak` |
| Jalur baca lama tidak berubah perilakunya | **Terpenuhi pada tingkat kontrak, belum pada tingkat runtime** | Route, verb, hak akses, dan ketujuh ruas asli muatannya terbukti utuh lewat dua uji. Pembuktian runtimenya ada pada `QuilvianSystemBackend.IntegrationTests.Postgres`, yang seluruhnya terhalang `QUILVIAN_BILLING_TEST_DB` — lihat bagian 5 |
| Cakupan tambahan roadmap — pemisahan tegas kolom yang boleh dan tidak boleh diubah kepala instalasi | **Terpenuhi** | Pemisahan ditegakkan dua lapis: hak akses `SystemFlag` pada endpoint penyetelan, dan `VAL-37` di dalam service |

Butir yang belum terpenuhi sepenuhnya disebut apa adanya pada baris keenam tabel di atas.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 186 warning, seluruhnya warning dokumentasi XML (`CS1573`, `CS1574`, `CS1587`) yang sudah ada sebelum task ini. Tidak satu pun berasal dari berkas baru task ini |
| Masalah yang diketahui | `QuilvianSystemBackend.IntegrationTests.Postgres` tidak dapat dijalankan tanpa environment variable `QUILVIAN_BILLING_TEST_DB`. Satu uji di dalamnya, `AlasanPenolakanOther_WajibDisertaiCatatan`, adalah bukti runtime `VAL-12` bagi jalur baca lama. Ini pembatas lingkungan yang berlaku untuk seluruh repository, bukan akibat task ini |
| Risiko tersisa | **Rendah untuk jalur teknis, sedang untuk jalur organisasi.** Endpoint `PUT /{id}/system-flags` sudah berdiri, tetapi **siapa pemegang `LabRejectionReason : SystemFlag` belum ditetapkan manajemen rumah sakit**. Selama belum, tidak ada akun yang dapat menyetel penanda biaya lewat aplikasi, sehingga alasan yang ditambahkan kepala instalasi akan selalu bernilai "bukan kesalahan internal" — artinya pengambilan ulang untuk alasan-alasan baru itu **dapat ditagihkan kepada pasien** sampai administrator sistem menyetelnya. Ini keadaan yang perlu diketahui Billing, bukan cacat teknik. Bentuknya sejenis dengan risiko terbuka `LabCriticalBound : Approve` pada `BE-LAB-05` |
| Risiko tersisa kedua | Grup ini belum punya `GET /filters/metadata`, `/summary`, `/options`, dan `GET /{id}` sebagaimana standar master data. Bila `FE-LAB-03` kelak membutuhkannya, penambahannya adalah amandemen kontrak yang harus lewat pemilik blueprint — lihat bagian 3.4 butir 1 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.1 |
| Langkah berikutnya | 1. `BE-LAB-07` — katalog, harga, dan cakupan penjamin, satu-satunya task `MVP-0` yang tersisa; ia menunggu `BE-EXT-01` milik pemilik `master-data`. 2. `FE-LAB-03` dapat mulai dikerjakan; kelima endpointnya sudah tersedia. 3. Menetapkan pemegang `LabRejectionReason : SystemFlag` — keputusan manajemen, bukan pekerjaan teknik |

### 7.1 Status Git

Perintah `git status --short` pada akhir pekerjaan. Branch `yoga`, tidak ada operasi Git yang
dijalankan dari sesi ini — tidak ada `add`, `commit`, `push`, `merge`, maupun `rebase`.

**Berkas milik task ini:**

```text
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabSpecimenDtos.cs
 M Program.cs
 M docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md
 M docs/module-blueprints/laboratorium/roadmap/traceability.md
?? Areas/HealthServices/LaboratoryManagement/Controllers/LabRejectionReasonController.cs
?? Areas/HealthServices/LaboratoryManagement/DTOs/LabRejectionReasonDtos.cs
?? Areas/HealthServices/LaboratoryManagement/Seeders/LabRejectionReasonSeeder.cs
?? Areas/HealthServices/LaboratoryManagement/Services/LabRejectionReasonService.cs
?? tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabRejectionReasonServiceTests.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-06.md
```

**Berkas yang sudah berubah sebelum task ini dimulai dan tidak disentuh:** seluruh berkas
`BE-LAB-01` sampai `BE-LAB-05` yang belum di-commit — `LaboratoryEnums.cs`,
`ApplicationDbContextModelSnapshot.cs`, `ApplicationDbContext.cs`, keempat model `LabValueBound*`,
keempat configurationnya, kedua controller dan service batas nilai, ketiga migration
`20260902*`, keempat berkas uji batas nilai, serta laporan `BE-LAB-02` sampai `BE-LAB-05`.
Tidak satu pun di antaranya diubah, dipulihkan, atau dibuang.

### 7.2 Perubahan keadaan repository sesudah laporan ini ditulis

Beberapa saat setelah bagian 7.1 dicatat, repository berubah dari luar sesi ini. `git log`
mencatat tiga peristiwa berurutan: commit `c8fc5cb updates BE modul lab` yang meng-commit
seluruh pekerjaan `BE-LAB-01` .. `BE-LAB-06`, lalu merge Pull Request #77
`RepairStrukturProject`, lalu merge `QuilvianIntegrationBackend` ke `yoga` — HEAD menjadi
`17a331b`.

Pull Request #77 merombak susunan project test:

| Sebelum | Sesudah |
| --- | --- |
| `QuilvianSystemBackend.Tests` di akar repository | `tests/QuilvianSystemBackend.UnitTests.InMemory` |
| `tests/QuilvianSystemBackend.BillingTests` | `tests/QuilvianSystemBackend.IntegrationTests.Postgres` |
| — | `tests/QuilvianSystemBackend.UnitTests.Sqlite` (baru) |

**Akibat yang perlu diketahui.** Perombakan itu menghapus `.csproj` project test lama, tetapi
lima berkas uji Laboratorium tertinggal di jalur lamanya tanpa project mana pun yang
mengompilasinya — termasuk `LabRejectionReasonServiceTests.cs` milik task ini. Selama keadaan
itu berlangsung, seluruh bukti uji `BE-LAB-02`, `BE-LAB-04`, `BE-LAB-05`, dan `BE-LAB-06`
**tidak dijalankan siapa pun**, dan project utama pun gagal dikompilasi karena keluaran build
project lama ikut tersapu masuk ke dalamnya.

Kelima berkas itu telah dipindahkan ke
`tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/`. Namespace
tidak perlu diubah sama sekali, karena project baru mempertahankan
`RootNamespace` `QuilvianSystemBackend.Tests`. Sisa keluaran build project lama dihapus.

Bukti sesudah pemindahan:

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | `0 Error(s)`, `23 Warning(s)` | `PASS` |
| `dotnet test ...UnitTests.InMemory --filter "FullyQualifiedName~LaboratoryManagement"` | `Failed: 0, Passed: 120, Total: 120` | `PASS` |
| `dotnet test ...UnitTests.InMemory` seluruhnya | `Failed: 1, Passed: 1000, Total: 1001` | `EXISTING / ENVIRONMENT ISSUE` — kegagalan Billing yang sama seperti bagian 5 |

Seluruh jalur berkas uji pada laporan ini sudah disesuaikan ke susunan baru. Laporan
`BE-LAB-02`, `BE-LAB-04`, dan `BE-LAB-05` **masih memuat jalur lama** dan menjadi utang
pembukuan pemilik blueprint.

Dua baris pada diff `Program.cs` — registrasi `LabValueBoundService` dan
`LabCriticalBoundApprovalService` — tampak sebagai penambahan karena keduanya milik `BE-LAB-04`
dan `BE-LAB-05` yang belum di-commit. Yang ditambahkan task ini pada berkas itu hanya tiga
baris: satu `using` seeder, registrasi `LabRejectionReasonService`, dan pemanggilan
`LabRejectionReasonSeeder`.
