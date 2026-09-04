# Laporan Perubahan Backend — `BE-LAB-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-09` |
| Judul | Entity pemeriksaan terpesan |
| Slice | `S2` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4, gelombang `MVP-1` |
| Trace | `FR-02.1`; `LAB-DEC-024`, `LAB-DEC-026`; BR-20, BR-22; `AC-35`, `AC-40`; `erd/data-dictionary.md` bagian 3 dan 11.1; `LAB-STATE-v1` r2 bagian 2 dan 3 |
| Contract version | `LAB-API-v1` r3 grup Lab Examination, `LAB-STATE-v1` r2 — `approved`, dikunci 2026-09-02. Task ini hanya membangun strukturnya; endpointnya milik `BE-LAB-16` |
| Dependency | `BE-LAB-01` — **`SELESAI`** |
| Klasifikasi | `HEAVY` — skor 9. Repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 0, database 2, keamanan/auth 0, UI/workflow 1, ditambah satu tingkat karena dampak schema dan migration dijalankan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi Laboratorium, project test, dan artefak blueprint modul Laboratorium |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `17a331b`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — entity, configuration, DbSet, dan migration ada; migration terbukti jalan dua arah terhadap database dev; checker QBE `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Submodule | Tidak ada |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` sejak 2026-09-02 lewat `LAB-REQ-002` |
| Status registry | Terdaftar dan `ACTIVE`. `QBE-MOD-002` dan `QBE-MOD-003` tidak menahan pembuatan entity `Lab*` maupun migration modul ini |
| Keberlakuan | `NEW CODE` untuk entity, configuration, dan test. `LabOrder` serta `TrxLabSpecimen` berstatus `TOUCHED LEGACY` — keduanya hanya bertambah satu navigation property, tanpa satu pun kolom baru |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-ENUM-001`, `QBE-CODE-004`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-PAGE-001`, `QBE-OPT-001` — task ini tidak membuat satu pun endpoint, DTO, maupun service; seluruhnya milik `BE-LAB-16`. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — tidak ada `LEGACY MIGRATION` |
| Sumber governance yang dibaca | `AGENTS.md`; `CLAUDE.md`; `rules/GLOBAL_RULES.md`; `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` `TASK_RULES`, `TASK_CLASSIFICATION`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE` |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif. Diverifikasi ulang sesudah repository berubah — lihat bagian 7.2 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, satu baris `TrxLabSpecimen` merangkap **dua peran yang sebenarnya
berbeda**: ia adalah wadah fisik yang diambil dari tubuh pasien, sekaligus jenis pemeriksaan
yang ditagihkan. Kolom `ProcedureId`, `TariffId`, dan `UnitPriceSnapshot` ada di sana, menempel
pada tabung.

Kenyataan di laboratorium tidak berpasangan satu-satu seperti itu. Satu tabung darah bertutup
ungu menopang hemoglobin, leukosit, dan trombosit sekaligus — satu kali tusukan jarum, tiga
jenis pemeriksaan.

Akibat nyatanya bagi pasien dan petugas:

> Ny. Sari dipesankan pemeriksaan darah lengkap. Karena setiap jenis pemeriksaan harus punya
> barisnya sendiri, sementara baris itu juga berarti wadah, sistem memaksa petugas mencatat
> **tiga wadah**: satu untuk hemoglobin, satu untuk leukosit, satu untuk trombosit. Tiga
> barcode dicetak dan ditempel — padahal darahnya diambil sekali, ke dalam satu tabung.
>
> Petugas laboratorium menerima satu tabung dengan tiga barcode dan harus menebak mana yang
> "sungguhan". Bila ia menyatakan layak pada satu barcode saja, dua pemeriksaan lain
> menggantung. Bila ia menyatakan layak pada ketiganya, ia mencatat tiga kali penerimaan untuk
> satu tabung yang sama. Jejak auditnya menjadi tidak dapat dipercaya, karena angka "jumlah
> wadah diterima" tidak lagi berarti jumlah wadah.

Masalah kedua ada pada penandaan cito. Kesegeraan dulu melekat pada **pesanan**. Padahal dokter
kerap membutuhkan satu hasil segera sementara sisanya dapat menunggu:

> dr. Rina memesan natrium dan profil lipid untuk pasien yang sama. Yang ia butuhkan segera
> hanya natrium — pasiennya kejang dan ia menduga hiponatremia. Profil lipid dapat menunggu
> sore. Dengan kesegeraan di tingkat pesanan, menandai cito berarti **seluruh** pesanan menjadi
> cito, sehingga profil lipid ikut mendesak dan ikut mengantre di depan. Daftar kerja cito
> laboratorium lalu penuh oleh pemeriksaan yang sebenarnya tidak mendesak, dan natrium yang
> benar-benar mendesak tenggelam di antaranya.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Aspek | Isi |
| --- | --- |
| Tujuan | Pemeriksaan terpesan berdiri sebagai satuan tersendiri, terpisah dari wadah fisik yang menopangnya |
| Pelaku | Petugas yang merencanakan wadah; dokter pemesan yang menandai kesegeraan; petugas laboratorium yang memutuskan kelayakan wadah |
| Pemicu | Pesanan laboratorium dibuat, lalu wadah beserta isi pemeriksaannya direncanakan |
| Hasil akhir | Satu wadah dengan satu barcode, menopang satu atau lebih baris pemeriksaan yang masing-masing membawa tarif, kesegeraan, dan penanda duplonya sendiri |

### 2.2 Langkah yang berurutan

1. Dokter membuat pesanan laboratorium beserta disiplinnya.
2. Petugas merencanakan wadah. **Satu tabung ungu dicatat sekali**, memperoleh satu barcode.
3. Pada wadah itu, petugas menyertakan jenis pemeriksaan yang akan dikerjakan darinya —
   hemoglobin, leukosit, dan trombosit. Masing-masing menjadi satu baris `LabExamination`
   berstatus `Ordered`, membawa salinan kode, nama, tarif, dan harga satuannya sendiri.
4. Bila dokter menandai salah satu pemeriksaan cito, penandaan itu tersimpan **pada baris
   pemeriksaan tersebut**, lengkap dengan waktu dan siapa yang menandai. Pemeriksaan lain pada
   pesanan yang sama tetap biasa.
5. Wadah diambil dari pasien, tiba di laboratorium, lalu diputuskan.
6. Ketika wadah dinyatakan **layak**, seluruh pemeriksaan di atasnya berpindah menjadi
   `ChargeEligible` dan waktunya dicatat. Inilah saat pemeriksaan menjadi sah ditagihkan.
7. Ketika wadah **ditolak**, seluruh pemeriksaan di atasnya menjadi `Voided` — gugur bersama
   wadahnya.

### 2.3 Contoh berangka — satu wadah, tiga pemeriksaan

| Baris | Jenis pemeriksaan | Wadah | Barcode | Harga satuan | Kesegeraan |
| ---: | --- | --- | --- | ---: | --- |
| 1 | Hemoglobin | Tabung ungu #1 | `BC-0001` | 35.000 | Biasa |
| 2 | Leukosit | Tabung ungu #1 | `BC-0001` | 30.000 | Biasa |
| 3 | Trombosit | Tabung ungu #1 | `BC-0001` | 28.000 | Biasa |

Tiga baris pemeriksaan, **satu** wadah, **satu** barcode, dan tiga harga yang berbeda. Inilah
`AC-35`. Sebelum perubahan ini, tabel yang sama memaksa tiga barcode.

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi | Ditegakkan oleh |
| --- | --- | --- |
| Jenis pemeriksaan yang sama disertakan dua kali pada satu wadah | Penyimpanan ditolak database | Index unik `IX_LabExamination_SpecimenId_ProcedureId` |
| Pesanan, wadah, atau jenis pemeriksaan yang masih dirujuk hendak dihapus | Penghapusan ditolak database | Ketiga foreign key memakai `Restrict` |
| Baris pemeriksaan yang sudah ditandai terhapus, lalu jenis yang sama dipesan ulang pada wadah itu | Diizinkan | Filter `IsDelete = false` pada index unik |
| Dua permintaan mengubah baris pemeriksaan yang sama secara bersamaan | Yang kedua gagal, tidak menimpa diam-diam | Token konkurensi `Version` |

**Yang belum ditegakkan pada task ini.** Penolakan beserta kode status `422` dan `409`, larangan
memindahkan pemeriksaan ke wadah lain, dan larangan menyatakan layak langsung pada pemeriksaan
adalah pekerjaan endpoint pada `BE-LAB-16`. Yang menjadi cakupan `BE-LAB-09` adalah struktur
yang membuat penegakan itu mungkin.

**Pengerjaan ganda tidak melanggar keunikan.** Duplo adalah penanda `IsDuplo` pada satu baris,
bukan dua baris pemeriksaan yang sama pada wadah yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Kontrak dan keputusan modul**

- `roadmap/backend-roadmap.md` bagian 4 dan 8.3
- `erd/data-dictionary.md` bagian 2, 3, 4, dan 11.1
- `02-backend-architecture.md` bagian 3.1, 4.3, dan tabel enum bagian 597
- `contracts/state-transition-matrix.md` bagian 2 dan 3
- `contracts/api-contract.md` grup Lab Examination
- `00-interview-decisions.md` — `AC-35`, `AC-40`, `LAB-DEC-024`, `LAB-DEC-026`

**Source yang menjadi pola terdekat**

- `Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs`
- `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`
- `Repositories/Configurations/HealthServices/LaboratoryManagement/TrxLabSpecimenConfiguration.cs`
- `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundConfiguration.cs`
- `Models/IdentityModel.cs`, `Repositories/ApplicationDbContext.cs`
- `tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabValueBoundTests.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs` | **Baru.** Entity pemeriksaan terpesan beserta 16 kolom bisnis, tiga navigation property, dan token konkurensi |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabExaminationConfiguration.cs` | **Baru.** Pemetaan tabel, panjang kolom, presisi harga, konversi enum ke `int`, satu index unik, empat index pencarian, dan tiga relasi `Restrict` |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | **Bertambah dua enum:** `LabExaminationStatus` (`Ordered`, `ChargeEligible`, `Voided`, `Cancelled`) dan `LabExaminationUrgency` (`Routine`, `Cito`). Tidak satu pun nilai enum lama diubah |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs` | Bertambah satu navigation `Examinations`. **Nol kolom baru** |
| `Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs` | Bertambah satu navigation `Examinations`. **Nol kolom baru**; keenam kolom yang kelak pindah masih utuh di sana — lihat bagian 3.4 |
| `Repositories/ApplicationDbContext.cs` | Bertambah `DbSet<LabExamination> LabExaminations` |
| `Migrations/20260903071535_AddLabExamination.cs` beserta `.Designer.cs` | **Baru.** Membuat tabel `public."LabExamination"` beserta enam index dan tiga foreign key. `Down` menjatuhkan tabel itu |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui EF mengikuti migration |
| `tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabExaminationTests.cs` | **Baru.** 13 uji: `AC-35`, `AC-40`, salinan tarif per pemeriksaan, keunikan, bentuk tabel, penamaan, dan larangan kolom hasil/finansial |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Task ini tidak menambah, mengubah, maupun menghapus satu pun endpoint, DTO, atau route. Grup Lab Examination pada `LAB-API-v1` r3 tetap berstatus **Rencana (belum tersedia)**; pemiliknya `BE-LAB-16` |
| Database | **Satu tabel baru** `public."LabExamination"` beserta enam index dan tiga foreign key. Tidak ada tabel lain yang berubah — migration `Up` hanya memuat satu `CreateTable`. Migration **dibuat dan dijalankan**; rinciannya pada bagian 5 |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada endpoint, tidak ada `[AccessPermission]`, dan tidak ada perubahan model otorisasi. Hak akses `LabExamination : Read` dan `: Update` didaftarkan `BE-LAB-16` bersama endpointnya |

### 3.4 Selisih terhadap kontrak dan dokumen desain

Lima butir berikut adalah selisih yang **disengaja**, dicatat terbuka sesuai `REVIEW_RULES`.

| No | Selisih | Alasan |
| ---: | --- | --- |
| 1 | `erd/data-dictionary.md` bagian 2 menyatakan enam kolom — `ProcedureId`, `ProcedureCodeSnapshot`, `ProcedureNameSnapshot`, `TariffId`, `TariffCodeSnapshot`, `UnitPriceSnapshot` — **dihapus** dari `TrxLabSpecimen`; task ini tidak menghapusnya | Roadmap bagian 8.3 menempatkan pekerjaan itu pada `BE-LAB-11` dan `BE-LAB-12`, dan `BE-LAB-11` berstatus **`BLOCKED`** oleh `LAB-OPEN-012` — jumlah data lab yang sudah terisi belum diverifikasi, sehingga pemindahan datanya belum aman. Menghapus kolomnya sekarang akan memutus jalur tagihan yang sedang berjalan. Kedua tabel hidup berdampingan sampai slice itu dibuka |
| 2 | Kamus data bagian 4 menyatakan `TrxLabTransitionHistory` **`Diperbarui`** dengan kolom baru `LabExaminationId`, dan `LabTransitionScope` bertambah nilai `LabExamination` | Roadmap bagian 8.3 menyatakan sebaliknya: `TrxLabTransitionHistory` "sudah ada, dipakai apa adanya, **tidak ada pekerjaan struktur**". Roadmap adalah wewenang task, sehingga tabel riwayat tidak disentuh. Nilai enum `LabTransitionScope.LabExamination` juga tidak ditambahkan karena belum ada satu pun kode yang mencatat perpindahan pemeriksaan — penambahannya melekat pada task yang mencatat transisi itu. **Selisih antara kamus data dan roadmap ini menjadi utang pemilik blueprint untuk diselaraskan** |
| 3 | Kamus data bagian 3 memberi `TariffId` tanda "Index" dan "FK ke tarif Master Data"; implementasi ini tidak memberi keduanya | DDL pada bagian 11.1 dokumen yang sama **tidak** memuat foreign key maupun index untuk `TariffId`, dan `TrxLabSpecimen` yang sudah berjalan juga menyimpannya sebagai salinan lepas. Menautkannya secara fisik akan membuat penataan ulang tarif di Master Data menyandera baris pemeriksaan yang sudah terbentuk. Implementasi mengikuti DDL dan pola yang sudah ada |
| 4 | DDL menulis `ChargeEligibleAt`, `UrgencyMarkedAt`, dan kolom waktu lain sebagai `timestamp`; migration menghasilkan `timestamp with time zone` | Konvensi Npgsql pada repository ini memetakan `DateTime` menjadi `timestamptz`, dan seluruh tabel Laboratorium yang sudah ada mengikutinya. Menyimpang darinya akan membuat satu tabel berperilaku berbeda saat zona waktu server berganti |
| 5 | Migration menghasilkan satu index tambahan `IX_LabExamination_ProcedureId` yang tidak disebut DDL | Dibuat EF secara otomatis untuk foreign key `ProcedureId`. Bukan keputusan desain, dan tidak bertentangan dengan DDL |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menyentuh satu pun endpoint. Grup
**Health Services / Laboratory Management / Lab Examination** beserta keenam endpointnya adalah
cakupan `BE-LAB-16` dan `BE-LAB-10`, dan sampai kedua task itu selesai statusnya tetap
**Rencana (belum tersedia)** pada `contracts/api-contract.md`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)`, `23 Warning(s)` | `PASS` | Keluaran perintah. Tidak ada warning dari berkas baru task ini |
| `dotnet test ...UnitTests.InMemory --filter "FullyQualifiedName~LabExamination"` | `Failed: 0, Passed: 13, Total: 13` | `PASS` | Keluaran perintah |
| Seluruh suite `QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 1013, Total: 1014` | `EXISTING / ENVIRONMENT ISSUE` | Satu-satunya kegagalan adalah `BillingManagement.BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`, milik modul Billing dan terbuka sejak sebelum task ini — tercatat pada laporan [`BE-LAB-04`](BE-LAB-04.md), [`BE-LAB-05`](BE-LAB-05.md), dan [`BE-LAB-06`](BE-LAB-06.md) |
| Seluruh suite `QuilvianSystemBackend.UnitTests.Sqlite` | `Failed: 0, Passed: 176, Total: 176` | `PASS` | Keluaran perintah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `Files evaluated: 23`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran perintah |
| `AC-35` — satu wadah menopang tiga pemeriksaan dengan satu barcode | Tiga baris tersimpan pada satu `SpecimenId`; wadahnya tetap satu, barcode `BC-0001` | `PASS` | `AC35_SatuWadahMenopangTigaPemeriksaan_DenganSatuBarcodeSaja` |
| Salinan tarif per pemeriksaan | Dua pemeriksaan pada wadah yang sama menyimpan harga berbeda, 35.000 dan 30.000 | `PASS` | `SetiapPemeriksaan_MenyimpanSalinanTarifnyaSendiri` |
| Pemeriksaan baru lahir `Ordered` dan belum layak tagih | Status `Ordered`, `ChargeEligibleAt` kosong | `PASS` | `PemeriksaanBaru_LahirBerstatusOrderedDanBelumLayakTagih` |
| Navigasi dua arah | Pesanan dan wadah sama-sama mengenali kedua pemeriksaannya | `PASS` | `NavigasiDuaArah_WadahDanPesananSamaSamaMengenaliPemeriksaannya` |
| `AC-40` — satu pesanan memuat cito dan biasa sekaligus | Natrium cito beserta jejak pelaku dan waktunya; profil lipid tetap biasa dengan jejak kosong | `PASS` | `AC40_SatuPesananMemuatPemeriksaanCitoDanBiasaSekaligus` |
| `AC-40` — penanda duplo per pemeriksaan | Satu baris duplo, satu baris tidak | `PASS` | `PenandaDuplo_TersimpanPadaBarisPemeriksaan` |
| `AC-40` — pesanan dan wadah **tidak** punya kolom kesegeraan maupun duplo | Keempat kolom terbukti absen di `LabOrder` dan `TrxLabSpecimen`, dan ada di `LabExamination` | `PASS` | `AC40_PesananDanWadah_TidakPunyaKolomKesegeraanMaupunDuplo` |
| Keunikan wadah dan jenis pemeriksaan | Index unik `IX_LabExamination_SpecimenId_ProcedureId` berfilter `"IsDelete" = false` | `PASS` | `SatuWadah_TidakBolehMenopangJenisPemeriksaanYangSamaDuaKali` |
| Empat index pencarian | `LabOrderId`, `ExaminationStatus`, `ChargeEligibleAt`, `Urgency` seluruhnya ada dan tidak unik | `PASS` | `EmpatIndexPencarian_TerpasangSesuaiKamusData` |
| `QBE-NAM-001` — penamaan | Entity `LabExamination`, tabel `LabExamination`, schema `public`, DbSet `LabExaminations`; tidak mengandung `Trx` | `PASS` | `PenamaanEntityDanTabel_MengikutiPrefixLabYangTerdaftar` |
| Bentuk kolom sesuai kamus data | Panjang 50/200/50, presisi `18,2`, kedua enum `int`, `Version` token konkurensi, nullability sesuai | `PASS` | `BentukKolom_SesuaiKamusDataBagian11Satu` |
| Ketiga relasi memakai `Restrict` | Tepat tiga foreign key, seluruhnya `Restrict`; `TariffId` terbukti tanpa foreign key | `PASS` | `KetigaRelasi_MemakaiRestrictAgarTautanTagihanTidakPutus` |
| Tidak ada kolom hasil maupun finansial | Sepuluh nama kolom terlarang terbukti absen | `PASS` | `TidakAdaKolomHasilMaupunKolomFinansial` |

### 5.1 Status migration dan database

Wewenang pembuatan **dan** eksekusi migration diminta terpisah dan **diberikan pemilik modul**
pada sesi ini, sesuai `CLAUDE.md` bagian *Larangan otomatisasi*.

| Langkah | Perintah | Hasil |
| ---: | --- | --- |
| 1 | `dotnet ef migrations add AddLabExamination` | `Done.` Menghasilkan `20260903071535_AddLabExamination` |
| 2 | `dotnet ef migrations list` | Tepat **satu** migration tertunda, yaitu milik task ini. Tidak ada milik task lain yang ikut terbawa |
| 3 | `dotnet ef database update` | `Done.` Tabel terbentuk |
| 4 | `dotnet ef database update 20260902091736_AmendLabValueBoundUniquenessAndSortOrder` | `Done.` Jalur `Down` berjalan; daftar migration kembali menandai `AddLabExamination (Pending)` |
| 5 | `dotnet ef database update` | `Done.` Diterapkan kembali; penanda `(Pending)` hilang |

| Aspek | Nilai |
| --- | --- |
| Database sasaran | `QuilvianNewDevYoga` — database pengembangan pemilik modul |
| Sifat host | **Remote, bukan lokal.** Disebut eksplisit oleh pemilik modul saat memberi wewenang, dan merupakan sasaran yang sama dengan `BE-LAB-01` |
| Keamanan jalur `Down` | Tabel yang dijatuhkan baru terbentuk pada langkah 3 dan belum memuat satu baris pun, sehingga pembuktian `Down` tidak menghilangkan data siapa pun |
| Tabel lain | Tidak ada. `Up` hanya memuat satu `CreateTable` |
| Deployment | **Tidak dilakukan.** Wewenang terpisah yang tidak diminta pada sesi ini |

Uji manual: `NOT APPLICABLE`. Task ini tidak menghasilkan permukaan yang dapat ditembak dari
Swagger; endpointnya milik `BE-LAB-16`.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Penegakan index unik terhadap data sungguhan | Provider InMemory tidak menegakkan index fisik. Keberadaan, keunikan, nama, dan filternya dibaca dari model relasional Npgsql yang dibangun tanpa koneksi. Penegakan runtimenya melekat pada endpoint `BE-LAB-16` yang akan menerjemahkannya menjadi penolakan berkode |
| Kueri langsung ke `pg_indexes` sesudah migration | `AGENTS.md` melarang menjalankan perintah database sekadar untuk memvalidasi source. Bukti penerapan diambil dari keluaran `dotnet ef migrations list` yang menunjukkan penanda `(Pending)` hilang |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Seluruhnya terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi, sebagaimana tercatat pada laporan `BE-LAB-06` bagian 5. Tidak ada uji Laboratorium baru yang ditempatkan di sana |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-35` — satu wadah fisik dapat menopang lebih dari satu pemeriksaan terpesan, dan hanya memiliki satu barcode | **Terpenuhi** | `AC35_SatuWadahMenopangTigaPemeriksaan_DenganSatuBarcodeSaja`; tiga baris pemeriksaan pada satu wadah berbarcode `BC-0001` |
| `AC-40` — penanda cito dan duplo hanya dapat disetel pada baris pemeriksaan | **Terpenuhi pada tingkat struktur** | `AC40_PesananDanWadah_TidakPunyaKolomKesegeraanMaupunDuplo` membuktikan keempat kolom itu hanya ada di `LabExamination`. Bagian kriteria yang berbunyi "percobaan menyetelnya pada pesanan **ditolak sistem**" menuntut endpoint yang menolak, dan itu cakupan `BE-LAB-10` serta `BE-LAB-16`. Di sini penolakan bersifat struktural: tidak ada kolomnya, sehingga tidak ada yang dapat disetel |

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Entity ada dengan nama benar | **Terpenuhi** | `PenamaanEntityDanTabel_MengikutiPrefixLabYangTerdaftar`; `LabExamination`, bukan `TrxLabExamination` |
| Configuration berada di folder submodul | **Terpenuhi** | `Repositories/Configurations/HealthServices/LaboratoryManagement/LabExaminationConfiguration.cs`, sejajar dengan configuration Laboratorium lainnya |
| Migration jalan dua arah | **Terpenuhi** | Bagian 5.1 langkah 3, 4, dan 5: `Up`, lalu `Down`, lalu `Up` kembali — ketiganya `Done.` terhadap `QuilvianNewDevYoga` |
| Checker QBE lolos | **Terpenuhi** | `Final result: PASS`, `VIOLATION: 0` atas 23 berkas |
| Cakupan roadmap — memuat salinan tarif, penanda kesegeraan, dan penanda duplo | **Terpenuhi** | `SetiapPemeriksaan_MenyimpanSalinanTarifnyaSendiri`, `AC40_SatuPesananMemuatPemeriksaanCitoDanBiasaSekaligus`, `PenandaDuplo_TersimpanPadaBarisPemeriksaan` |

Tidak ada butir DoD yang belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution menghasilkan 23 warning, seluruhnya sudah ada sebelum task ini dan tidak satu pun berasal dari berkas baru |
| Masalah yang diketahui | Kamus data dan roadmap **bertentangan** soal `TrxLabTransitionHistory` — lihat bagian 3.4 butir 2. Task ini mengikuti roadmap dan tidak menyentuh tabel riwayat; penyelarasan kedua dokumen menjadi utang pemilik blueprint |
| Risiko tersisa | **Sedang.** Sampai `BE-LAB-11` dan `BE-LAB-12` selesai, `TrxLabSpecimen` dan `LabExamination` sama-sama memuat `ProcedureId` dan salinan tarif. Selama itu berlangsung, ada **dua tempat** yang dapat menjawab "berapa harga pemeriksaan ini", dan keduanya dapat berbeda bila ada kode yang menulis ke salah satunya saja. Belum ada satu pun kode yang menulis ke `LabExamination`, sehingga risikonya belum aktif — tetapi ia menjadi aktif begitu `BE-LAB-16` dibangun sebelum `BE-LAB-11` dan `BE-LAB-12` menutup jalur lama |
| Risiko tersisa kedua | `BE-LAB-11` berstatus `BLOCKED` oleh `LAB-OPEN-012`, dan penahannya bukan milik Laboratorium. Entity ini sudah ada tetapi belum dapat menggantikan perannya sampai penahan itu dicabut |
| Perubahan sampingan | `Migrations/ApplicationDbContextModelSnapshot.cs` diperbarui EF sebagai bagian normal pembuatan migration. Tidak ada perubahan sampingan lain yang perlu dipulihkan |
| Interupsi | Repository berubah dari luar sesi ini di tengah pekerjaan — lihat bagian 7.2 |
| Status Git | Lihat bagian 7.3 |
| Langkah berikutnya | 1. `BE-LAB-16` — endpoint pemeriksaan terpesan, yang menerjemahkan struktur ini menjadi permukaan API beserta penolakannya. 2. `BE-LAB-10` — penandaan cito per pemeriksaan. 3. Menyelaraskan kamus data dengan roadmap soal `TrxLabTransitionHistory`. 4. `BE-LAB-07` masih menunggu `BE-EXT-01` milik pemilik `master-data` |

### 7.2 Perubahan keadaan repository di tengah pekerjaan

Sesi ini dimulai pada commit `d8d67c3`. Di tengah pekerjaan, repository berubah dari luar sesi:
commit `c8fc5cb updates BE modul lab` meng-commit pekerjaan `BE-LAB-01` .. `BE-LAB-06`, lalu
merge Pull Request #77 `RepairStrukturProject` merombak susunan project test, lalu merge
`QuilvianIntegrationBackend` membawa HEAD ke `17a331b`.

Perombakan itu meninggalkan lima berkas uji Laboratorium di jalur lama tanpa project yang
mengompilasinya, sekaligus membuat project utama gagal dibangun karena keluaran build project
lama ikut tersapu masuk. Kelimanya dipindahkan ke
`tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/` — tanpa
perubahan namespace, karena project baru mempertahankan `RootNamespace`
`QuilvianSystemBackend.Tests` — dan sisa keluaran build project lama dihapus. Rinciannya
tercatat pada laporan [`BE-LAB-06`](BE-LAB-06.md) bagian 7.2.

Seluruh sumber governance diverifikasi ulang **sesudah** perubahan itu: `AGENTS.md`,
`CLAUDE.md`, rules root runtime, kontrak rekayasa, dan registry seluruhnya terbaca, dan baris
`LaboratoryManagement / Laboratory / Lab / ACTIVE` tidak berubah.

### 7.3 Status Git

Branch `yoga`, HEAD `17a331b`. Tidak ada operasi Git yang dijalankan dari sesi ini — tidak ada
`add`, `commit`, `push`, `merge`, maupun `rebase`.

**Berkas milik task ini:**

```text
 M Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs
 M Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs
 M Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
?? Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs
?? Migrations/20260903071535_AddLabExamination.cs
?? Migrations/20260903071535_AddLabExamination.Designer.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabExaminationConfiguration.cs
?? tests/QuilvianSystemBackend.UnitTests.InMemory/HealthServices/LaboratoryManagement/LabExaminationTests.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-09.md
```

**Berkas yang berubah pada sesi ini di luar `BE-LAB-09`:**

| Berkas | Sebabnya |
| --- | --- |
| Ketiga controller `LabValueBound`, `LabCriticalBoundApproval`, `LabRejectionReason` | Permintaan langsung pemilik modul: komentar XML `<summary>` pada method action dihapus karena teksnya tampil sebagai deskripsi pada baris endpoint Swagger. Penjelasan aturan bisnisnya dipertahankan sebagai komentar biasa. Sebelas blok dihapus; tidak ada perubahan perilaku |
| Lima berkas uji Laboratorium yang pindah jalur | Perbaikan akibat Pull Request #77 — lihat bagian 7.2 |
| `docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-06.md` | Jalur berkas uji diperbarui mengikuti susunan project yang baru, dan bagian 7.2 ditambahkan |
