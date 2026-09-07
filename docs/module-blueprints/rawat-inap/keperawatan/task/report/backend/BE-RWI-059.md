# Laporan Perubahan Backend — `BE-RWI-059`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-059` |
| Judul | Rencana asuhan keperawatan punya tempat menyimpan masalah dan tujuannya |
| Slice | Gelombang `KEP-MVP-2` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-012`, `FR-KEP-013`, `FR-KEP-015`; PRD `CAP-013` aturan 1, 2, 4; `AC-CAP013-01`; `RWI-DEC-083`, `RWI-DEC-090` |
| Contract version | API `0.3.0` grup Nursing Care Plan; state transition `0.3.0` bagian 2; validation `VAL-KEP-16`. Delta kontrak pada bagian 7 |
| Dependency | `BE-RWI-056` — 🟡 sebagian, tidak memblokir slice ini; yang dibutuhkan (pemisahan pengkajian awal dan ulang) sudah mendarat |
| Klasifikasi | `HEAVY` — repository 1 (0), berkas diperiksa > 8 (1), berkas diubah > 3 (1), logika bisnis sedang (1), kontrak baru (1), entity dan migration baru (1), keamanan hak akses baru (1), workflow status baru (1). Total **7** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6f7d81e0` pada branch `MHamzah`; garis dasar perencanaan roadmap `7d4bf2b9` |
| Tanggal | 7 September 2026 |
| Status | **Selesai.** Kelima acceptance criteria terbukti. Migration `20260906144058_AddNursingCarePlan` dibuat dan **belum diterapkan ke database mana pun** |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, rencana asuhan keperawatan **tidak punya tempat sama sekali di dalam
sistem**. Pembacaan source pada `BE@7d4bf2b` menemukan nol berkas `*CarePlan*` dan nol berkas
`*Nursing*` di seluruh repository — tabelnya benar-benar belum ada.

Akibatnya bagi pekerjaan nyata: perawat menetapkan masalah keperawatan pasien, tujuan asuhan, dan
rencana tindakannya **di kertas**. Kertas itu tidak terbaca perawat pada giliran berikutnya tanpa
mencarinya lebih dulu, tidak terbaca dokter sama sekali, dan hilang bersama lembarnya ketika pasien
pindah ruangan.

**Contoh yang sering terjadi.** Ns. Sari menilai Tn. Budi berisiko jatuh tinggi pada hari pertama
dan menyusun rencana pendampingan. Giliran malam dipegang perawat lain yang tidak melihat lembar
itu. Tidak ada satu pun tempat di sistem yang dapat menjawab pertanyaan sederhana "masalah
keperawatan apa yang sedang dikerjakan untuk pasien ini".

---

## 2. Proses bisnis

**Tujuan.** Perawat menetapkan masalah keperawatan pasien beserta tujuan dan rencana tindakannya di
dalam sistem, sehingga seluruh giliran membaca rencana yang sama.

**Pelaku.** Perawat penanggung jawab dan kepala ruangan.

**Pemicu.** Pasien sudah benar-benar masuk kamar, dan pengkajian awalnya sudah dikerjakan.

**Langkah yang berurutan.**

1. Perawat membuka rencana asuhan bagi satu perawatan rawat inap.
   Sistem menurunkan sendiri pasien, kunjungan, dan perawatannya dari kunjungan yang disebut —
   tidak satu pun diterima apa adanya dari layar.
2. Perawat menambahkan masalah keperawatan satu per satu. Setiap butir memuat masalahnya, tujuan
   asuhannya, dan rencana tindakannya.
3. Bila masalah itu lahir dari sebuah pengkajian, butirnya dikaitkan ke pengkajian asal, sehingga
   pembaca berikutnya tahu masalah ini datang dari mana.
4. Selama perawatan berjalan, perawat mencatat evaluasi hasil asuhan pada butir yang bersangkutan.
5. Ketika masalahnya teratasi, butir ditutup sebagai **teratasi** beserta alasannya. Ketika
   masalahnya tidak lagi relevan, butir **dihentikan** beserta alasannya.

**Aturan yang berlaku.**

| Aturan | Isinya | Kode |
| --- | --- | --- |
| Satu perawatan tepat satu rencana | Yang banyak adalah butir masalahnya, bukan rencananya | `409` bila rencana kedua dibuat |
| Rencana hanya untuk pasien yang sedang dirawat | Perawatan `Draft`, `Closed`, atau `Cancelled` ditolak | `422` |
| Tercapai menuntut evaluasi | `VAL-KEP-16`: menyatakan masalah teratasi tanpa satu pun evaluasi membuat rekam medis tidak dapat menunjukkan dasarnya | `400` |
| Dihentikan menuntut alasan | Menutup butir tanpa alasan membuat auditor tidak tahu mengapa | `400` |
| Butir tidak pernah dihapus | `CAP-013` aturan 6 | — |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Perawatan sudah punya rencana asuhan | `409` beserta arahan menambah butir pada rencana yang sudah ada; nol baris kedua terbentuk |
| Pasien belum masuk kamar | `422` — pasien yang belum dirawat belum punya asuhan untuk direncanakan |
| Akun perawat belum tertaut ke data pegawai | `400` beserta arahan menghubungi kepegawaian. Dokumentasi keperawatan wajib menyebut **siapa perawatnya**, dan akun tanpa pegawai tidak dapat menyebut siapa pun |
| Butir merujuk pengkajian milik perawatan lain | `400` — penjaga salah pasien |
| Menutup butir sebagai teratasi tanpa evaluasi | `400`, dan butirnya **tetap** masih dikerjakan — bukan setengah tertutup |

**Contoh berangka penjaga `VAL-KEP-16`.** Ns. Sari hendak menutup masalah "risiko jatuh tinggi"
Tn. Budi sebagai teratasi, padahal belum satu pun evaluasi tercatat. Yang terjadi: permintaan
ditolak dengan kalimat *"Butir ini belum punya catatan evaluasi, sehingga belum dapat dinyatakan
tercapai."*, dan di database butirnya masih berkeadaan dikerjakan — `ResolvedAt` kosong,
`CloseReason` kosong. Setelah Ns. Sari mencatat evaluasi *"pasien mampu berjalan mandiri tanpa
terjatuh selama 3 hari"*, penutupan barulah diterima.

**Hasil akhir.** Satu perawatan memiliki satu rencana asuhan berisi sebanyak apa pun butir masalah,
masing-masing dengan tujuan, rencana tindakan, evaluasi, dan keadaannya sendiri — seluruhnya
terbaca lintas giliran.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `AGENTS.md`, `rules/GLOBAL_RULES.md`, `rules/backend/TASK_RULES.md` | Wewenang, mode task, dan siklus kerja |
| `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` | QBE yang berlaku bagi `NEW CODE` |
| `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Pemilik dan prefix `ClinicalManagement` = `Cli`, lifecycle `ACTIVE` |
| `rules/backend/transaction-endpoint-standard.md`, `role-access-rules.md` | Bentuk endpoint transaksi dan kontrak penamaan hak akses |
| `roadmap/backend-roadmap.md` bagian 0–4 | Scope, acceptance criteria, dan larangan `Trx*` |
| `contracts/api-contract.md` `0.3.0` bagian 2 | Bentuk endpoint grup Nursing Care Plan |
| `contracts/state-transition-matrix.md` `0.3.0` bagian 2 | Mesin status butir asuhan |
| `contracts/validation-matrix.md` bagian 1, 2, 4 | `VAL-KEP-01` s.d. `VAL-KEP-03`, `VAL-KEP-16` |
| `data/data-dictionary.md` bagian 3, 4 | Bentuk kolom kedua tabel |
| `Areas/HealthServices/ClinicalManagement/Models/CliPhysicianVisit.cs` | Pola entity `NEW CODE` terdekat, beserta alasan penamaannya |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliPhysicianVisitConfiguration.cs` | Pola configuration EF terdekat |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | Penurunan konteks perawatan yang dipakai ulang |
| `Areas/HealthServices/RegistrationManagement/Controllers/NurseStationQueueController.cs` | Pola penemuan pegawai dari pengguna yang masuk |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Pola service pemilik perintah domain |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/NursingCarePlanItemStatus.cs` | **Baru.** Tiga keadaan butir: `Active`, `Resolved`, `Discontinued` |
| `Areas/HealthServices/ClinicalManagement/Models/CliNursingCarePlan.cs` | **Baru.** Rencana asuhan satu perawatan |
| `Areas/HealthServices/ClinicalManagement/Models/CliNursingCarePlanItem.cs` | **Baru.** Butir masalah keperawatan beserta tujuan, rencana, dan evaluasinya |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliNursingCarePlanConfiguration.cs` | **Baru.** Unique parsial `InpEpisodeId` pada baris belum terhapus |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliNursingCarePlanItemConfiguration.cs` | **Baru.** Relasi induk `Cascade`, rujukan pengkajian `SetNull` |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingCarePlanDtos.cs` | **Baru.** Kontrak transport permintaan dan balasan |
| `Areas/HealthServices/ClinicalManagement/Services/NursingActorService.cs` | **Baru.** Menemukan pegawai di balik pengguna yang masuk |
| `Areas/HealthServices/ClinicalManagement/Services/NursingCarePlanService.cs` | **Baru.** Pemilik seluruh perintah dan pembacaan rencana asuhan |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingCarePlanController.cs` | **Baru.** Lima endpoint beserta metadata hak aksesnya |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` baru |
| `Program.cs` | Pendaftaran `NursingActorService` dan `NursingCarePlanService` |
| `Migrations/20260906144058_AddNursingCarePlan.cs` | **Baru.** Dua tabel, tujuh index, nol perubahan pada tabel lain |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/Infrastructure/RawatInapTestData.cs` | Penolong `BuatPerawat` — pegawai perawat beserta master pendukung dan penautan akunnya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingCarePlanTests.cs` | **Baru.** 12 uji acceptance |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentAccessContractTests.cs` | Controller baru ikut dijaga kontrak penamaan hak akses dan larangan hardcode role |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah.** Lima endpoint baru pada grup Nursing Care Plan; empat sesuai `api-contract.md` `0.3.0` dan satu delta yang dijelaskan bagian 7 |
| Database | **Dua tabel baru** milik `ClinicalManagement`: `public."CliNursingCarePlan"` dan `public."CliNursingCarePlanItem"`. Migration `20260906144058_AddNursingCarePlan` dibuat dan **belum diterapkan ke database mana pun** |
| Keamanan/Auth | **Resource baru `NursingCarePlan`** dengan tiga action: `Read`, `Create`, `Update`. Seluruhnya muncul di layar Akses Role dan ditegakkan `AccessPermissionFilter`. Nol hardcode nama peran, departemen, jabatan, maupun `UserType` |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Nursing Care Plan

Base URL: `api/v1/health-services/clinical-management/nursing-care-plans`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuka rencana asuhan bagi satu perawatan rawat inap | `NursingCarePlan : Create` |
| `GET` | `/episodes/{episodeId}` | Rencana asuhan satu perawatan beserta seluruh butirnya | `NursingCarePlan : Read` |
| `POST` | `/{id}/items` | Menambah masalah keperawatan beserta tujuan dan rencana tindakannya | `NursingCarePlan : Update` |
| `PATCH` | `/items/{itemId}/evaluate` | Mencatat evaluasi hasil asuhan | `NursingCarePlan : Update` |
| `PATCH` | `/items/{itemId}/close` | Menutup masalah keperawatan sebagai teratasi atau dihentikan | `NursingCarePlan : Update` |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `200` / `201` | Rencana atau butir tersimpan |
| `400` | Isian tidak lengkap, atau butir dinyatakan tercapai tanpa evaluasi. Pesannya menyebut isian mana |
| `404` | Perawatan, rencana, atau butirnya tidak ditemukan |
| `409` | Perawatan sudah punya rencana asuhan, atau butirnya sudah ditutup sebelumnya |
| `422` | Perawatan tidak sedang menerima dokumentasi baru |

### Yang **tidak** dibuat, dan alasannya

| Yang tidak ada | Alasan |
| --- | --- |
| `GET /options` | Rencana asuhan adalah transaksi, bukan isi dropdown — `transaction-endpoint-standard.md` bagian 1 |
| `PATCH /{id}/status` generik | Butir berpindah keadaan karena kejadian bernama, bukan karena seseorang menyetel nilai |
| `DELETE /{id}` | `CAP-013` aturan 6 melarang penutupan butir menghapus jejaknya. Butir ditutup, tidak dihapus |
| `GET /filters/metadata`, `GET /summary`, `GET /` | Kontrak `0.3.0` tidak memintanya; permukaan baca slice ini bersandar pada satu perawatan, bukan pada daftar lintas pasien. Dicatat sebagai kekurangan terhadap baseline pada bagian 7 |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `Build succeeded`, `213 Warning(s)`, `0 Error(s)` | `PASS` | Keluaran perintah; jumlah warning sama persis dengan garis dasar `6f7d81e` |
| `dotnet test` project SQLite, tapis `NursingCarePlanTests` | `Failed: 0, Passed: 12, Total: 12` | `PASS` | Keluaran perintah |
| `dotnet test` project SQLite, seluruhnya | `Failed: 0, Passed: 448, Total: 448` | `PASS` | Garis dasar sebelum slice ini 394 |
| `dotnet test` project InMemory, seluruhnya | `Failed: 9, Passed: 917, Total: 926` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan kegagalan berada di `BillingManagement` dan **sudah ada sebelum slice ini**: worktree pada commit `6f7d81e` tanpa satu pun perubahan slice ini menghasilkan angka yang sama persis |
| Uji migration maju-mundur terhadap PostgreSQL sungguhan | Tidak dijalankan | `NOT RUN` | Wewenang eksekusi database tidak diberikan task ini. Bentuk `Up` dan `Down` diperiksa manual: `Down` menghapus kedua tabel dengan urutan anak lalu induk |
| Skenario penambahan butir | Tiga butir pada satu rencana, terbaca berurutan | `PASS` | `SatuRencana_MenampungBanyakButirMasalah` |
| Percobaan menutup butir tanpa evaluasi | `400`, butir tetap `Active` di database | `PASS` | `ButirTanpaEvaluasi_TidakDapatDinyatakanTercapai` |
| Skenario butir yang merujuk pengkajian | Rujukan tersimpan, nomor pengkajian terbaca kembali | `PASS` | `Butir_DapatMerujukPengkajianAsal` |
| Kontrak penamaan hak akses controller baru | Ketiga nilai cocok huruf demi huruf; nol hardcode role | `PASS` | `NursingAssessmentAccessContractTests`, `Failed: 0, Passed: 16` |

Uji manual: `NOT FEASIBLE` — controller memakai `[Authorize]` dengan JWT bearer dan `Program.cs`
memakai top-level statement tanpa `public partial class Program`, sehingga uji lewat HTTP menuntut
perubahan berkas yang dipakai seluruh tim. Kelima acceptance criteria tidak memerlukan lapisan
transport untuk dibuktikan.

**Tidak dijalankan:**

- **Eksekusi migration ke database mana pun.** Wewenangnya terpisah dan tidak diberikan.
  Roadmap bagian 0.1 menegaskan migration **MUST NOT** diterapkan ke database bersama tanpa izin
  tertulis terpisah.
- **Uji hak akses non-SuperAdmin lewat HTTP.** Project uji memanggil controller langsung sehingga
  `AccessPermissionFilter` dilewati; penggantinya uji refleksi kontrak penamaan atribut, dan
  keterbatasan itu sama seperti yang dilaporkan `BE-RWI-055`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu episode memiliki **tepat satu** rencana asuhan, dengan butir masalah sebanyak yang dibutuhkan | Terpenuhi | Unique parsial `IX_CliNursingCarePlan_InpEpisodeId` `WHERE "IsDelete" = false`; uji `RencanaKedua_PadaPerawatanYangSama_Ditolak409` dan `SatuRencana_MenampungBanyakButirMasalah` |
| 2. Butir memuat masalah, tujuan, rencana tindakan, dan evaluasi | Terpenuhi | Kolom `ProblemStatement`, `GoalStatement`, `PlannedIntervention`, `EvaluationNote`; uji `Butir_MenyimpanMasalahTujuanRencanaDanEvaluasi` |
| 3. Butir dapat dinyatakan tercapai **hanya** bila evaluasinya sudah ada; tanpa evaluasi ditolak `400` dan butir tetap `Active` (`VAL-KEP-16`) | Terpenuhi | `NursingCarePlanService.CloseItemAsync` berhenti sebelum satu nilai pun berubah; uji `ButirTanpaEvaluasi_TidakDapatDinyatakanTercapai`, `ButirDenganEvaluasi_DapatDinyatakanTercapai`, `ButirTanpaEvaluasi_MasihDapatDihentikanDenganAlasan` |
| 4. Butir dapat dikaitkan ke temuan pengkajian asalnya (`AC-CAP013-01`) | Terpenuhi | Kolom `SourceAssessmentId` beserta penjaga salah pasiennya; uji `Butir_DapatMerujukPengkajianAsal` dan `Butir_MenolakPengkajianMilikPerawatanLain` |
| 5. Rencana asuhan hanya dapat dibuat untuk episode `Admitted`; selain itu ditolak `422` | Terpenuhi | `NursingCarePlanService.OpenAsync`; uji `RencanaAsuhan_PadaPerawatanDraft_Ditolak422` dan `RencanaAsuhan_PadaPerawatanTertutup_Ditolak422` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Dua entity | Terpenuhi — `CliNursingCarePlan`, `CliNursingCarePlanItem` |
| Dua configuration | Terpenuhi |
| Dua `DbSet` | Terpenuhi |
| Satu enum | Terpenuhi — `NursingCarePlanItemStatus` |
| Satu migration | Terpenuhi — `20260906144058_AddNursingCarePlan`, **belum diterapkan** |
| Empat endpoint | **Terlampaui** — lima endpoint; yang kelima dijelaskan bagian 7 |
| Kelima acceptance criteria terbukti | Terpenuhi |
| `dotnet build` lulus | Terpenuhi |
| Uji migration maju-mundur terhadap PostgreSQL | **Belum terpenuhi** — wewenang eksekusi database tidak diberikan task ini |

---

## 7. Catatan penutup

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | Tidak ada; berkas tinggal langsung di `Models/`, `Controllers/`, `Services/`, `DTOs/`, `Enums/` |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, kategori `BUSINESS DOMAIN / MODULE` |
| Lifecycle registry | `ACTIVE / LEGACY` — entity operasional `Cli*` **diberi wewenang**; nol entri registry baru dibutuhkan |
| Keberlakuan | `NEW CODE` untuk seluruh berkas baru; `TOUCHED LEGACY` bersifat aditif pada `ApplicationDbContext.cs` dan `Program.cs` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-CODE-001` s.d. `QBE-CODE-006` — kamus data bagian 3 dan 4 tidak menetapkan nomor bisnis bagi kedua tabel ini, sehingga tidak ada kode yang dialokasikan. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION` |
| `QBE-NAM-001` | **Ditegakkan.** Nol nama `Trx*`. `02-backend-architecture.md` bagian 4.2 yang menulis `TrxNursingCarePlan` dibaca sebagai usulan bentuk, dan nama bakunya `CliNursingCarePlan` |

### Delta kontrak yang dilaporkan ke pemilik kontrak

| Delta | Isinya | Kenapa |
| --- | --- | --- |
| **Endpoint kelima** `PATCH /items/{itemId}/close` dikerjakan di slice ini | Kartu task menyebut empat endpoint dan menaruh `close` pada `BE-RWI-060`, sementara kontrak `0.3.0` memang mencantumkannya pada grup yang sama | Acceptance criteria 3 task **ini** menuntut penolakan penutupan tanpa evaluasi. Tanpa endpoint penutupannya, kriteria itu tidak dapat dibuktikan sama sekali |
| **`CloseCarePlanItemRequest` memuat `TargetStatus`** | Kontrak menyebut hanya `Reason` | Mesin status bagian 2 membedakan dua penutupan yang artinya sangat berbeda: `Resolved` menuntut evaluasi, `Discontinued` tidak. Satu endpoint tanpa penanda tujuan tidak dapat membedakannya |
| **Kolom `EvaluationNote` dan `LastEvaluatedAt` pada butir** | Kamus data bagian 4 tidak mencantumkannya, tetapi bagian 5 mencantumkan `EvaluationNote` pada tabel revisi | Acceptance criteria 2 menuntut butir memuat evaluasi, dan `VAL-KEP-16` menuntut keberadaannya dapat diperiksa. Tanpa kedua kolom itu, `PATCH /evaluate` tidak punya tempat menyimpan apa pun |
| **Kolom `SourceAssessmentId` pada butir** | Kamus data bagian 4 tidak mencantumkannya | `AC-CAP013-01` dan acceptance criteria 4 menuntut butir dapat dikaitkan ke pengkajian asalnya. Tidak ada kolom lain yang dapat menyimpan tautan itu |
| **Kolom `AuthoredByEmployeeId` dan `AuthoredAt` pada butir** | Kamus data bagian 4 tidak mencantumkannya | `AC-CAP013-02` menuntut versi lama tetap menyimpan **penulis dan waktu aslinya**. Tanpa kedua kolom ini, versi lama hanya dapat menyebut penulis yang mengubah — persis kesalahan yang dilarang `BE-RWI-060` |
| **Perawatan `DischargePending` ikut ditolak** | Acceptance criteria 5 menulis "hanya `Admitted`", sementara `InpatientClinicalContextService` bersama memperlakukan `DischargePending` sebagai masih berjalan | Kriteria ditegakkan apa adanya. Akibatnya rencana asuhan lebih ketat daripada pengkajian dan catatan dokter. Bila pemilik kontrak menghendaki keduanya seragam, penjaganya satu baris di `NursingCarePlanService` |
| **Tiga endpoint baseline master data tidak dibuat** | `GET /filters/metadata`, `GET /summary`, `GET /` | Kontrak `0.3.0` tidak memintanya, dan permukaan baca slice ini bersandar pada satu perawatan. Dicatat sebagai kekurangan terhadap baseline, bukan didiamkan |

### Ringkasan lain

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning solusi tetap `213`, sama persis dengan garis dasar. Nol warning baru |
| Masalah yang diketahui | Akun pengguna yang belum tertaut ke `MstEmployee` tidak dapat membuka rencana asuhan maupun menambah butir. Ini kelengkapan data induk, bukan hak akses, dan pesannya mengarahkan ke perbaikan datanya |
| Risiko tersisa | Katalog terminologi SDKI/SLKI/SIKI (`OQ-RI-011`) masih terbuka. Kolom `NursingDiagnosisId` sudah disediakan **tanpa foreign key** karena tabel tujuannya belum ada; masalah keperawatan sementara ini ditulis sebagai teks, dan layar **tidak boleh** mengunci bentuknya ke katalog yang belum diputuskan |
| Risiko tersisa | Migration belum pernah diterapkan ke database mana pun, sehingga bentuk tabelnya belum pernah diuji terhadap PostgreSQL sungguhan |
| Perubahan sampingan | `NONE`. Migration `20260906140941` yang sempat dibuat lebih dulu dihapus dan digenerasi ulang menjadi `20260906144058` setelah dua kolom penulis butir ditambahkan; `Migrations/ApplicationDbContextModelSnapshot.cs` dikembalikan ke keadaan tracked sebelum regenerasi |
| Interupsi | `NONE` |
| Status Git | 40 entri: 10 berkas `M`, 30 berkas `??`. Nol berkas milik modul lain tersentuh; nol berkas `BillingManagement` |
| Langkah berikutnya | `BE-RWI-060` menambahkan riwayat versi butir; setelah itu wewenang eksekusi migration diminta terpisah kepada pemilik database |
