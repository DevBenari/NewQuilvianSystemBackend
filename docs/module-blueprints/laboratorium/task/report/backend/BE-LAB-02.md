# Laporan Perubahan Backend — `BE-LAB-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-02` |
| Judul | Tabel batas nilai dan pilihan hasil |
| Slice | `S3` — pengelolaan batas nilai (`roadmap/backend-roadmap.md` bagian 3, gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md` bagian 3 |
| Trace | `FR-03.1`, `FR-03.2`, `FR-03.6`, dan bagian penyimpanan `FR-01.4`; `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-036`; `LAB-VAL-v1` r3 `VAL-21` .. `VAL-24`; `erd/data-dictionary.md` bagian 5, 6, 11.2, dan 11.3 |
| Contract version | `LAB-API-v1` r3 dan `LAB-VAL-v1` r3 — `approved`, dikunci 2026-09-02. Task ini **tidak** menyentuh satu pun endpoint; keduanya dipakai sebagai target bentuk data, bukan sebagai permukaan yang diubah |
| Dependency | — (tidak ada). `LAB-OPEN-021` sudah dijawab 2026-09-02: prefix `Lab`, tabel bernama `LabValueBound` dan `LabValueOption`. `BE-LAB-03`, `BE-LAB-04`, dan `BE-LAB-05` bergantung pada task ini |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 0, database 2, keamanan 0, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Repositories/Configurations/HealthServices/LaboratoryManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/`, `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/`, dan `docs/module-blueprints/laboratorium/` beserta pembaruan bukti pada `roadmap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `d8d67c3` — *Merge remote-tracking branch 'origin/QuilvianIntegrationBackend' into yoga*, 2026-09-02, branch `yoga`. Roadmap menyebut snapshot `c87d9c0`; selisihnya diperiksa dan tidak menyentuh permukaan task ini (lihat bagian 7) |
| Tanggal | 2026-09-02 |
| Status | **Selesai.** Source, test, pembuatan migration, dan eksekusi migration ke database dev pemilik seluruhnya tuntas dan terverifikasi dua arah. Seluruh butir DoD terpenuhi. Dua temuan yang **bukan** butir DoD dicatat apa adanya pada bagian 3.3 dan 7 |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, sistem **tidak menyimpan batas nilai laboratorium sama sekali**.

Ketika hasil Hemoglobin seorang pasien keluar di angka 6,8 g/dL, tidak ada satu pun tempat di
dalam sistem yang tahu bahwa angka itu **kritis**. Tidak ada satuan yang tercatat, tidak ada
batas normal, dan tidak ada batas kritis. Penilaian sepenuhnya bergantung pada ingatan analis
dan lembar rujukan di luar sistem.

Akibatnya ada dua, dan keduanya nyata:

**Pertama, nilai kritis tidak mungkin dideteksi otomatis.** `LAB-DEC-004` mewajibkan pelaporan
nilai kritis, tetapi kewajiban itu menggantung tanpa tabel batas nilai — inilah
`LAB-CONFLICT-001` yang diselesaikan `LAB-DEC-006` dengan menarik tabel batas nilai maju ke
Rilis 1.

**Kedua, batas nilai tidak dapat dibedakan per kelompok pasien.** Hemoglobin normal pria dewasa
13,0–17,0 g/dL, wanita dewasa 12,0–15,0 g/dL, dan anak 11,0–14,0 g/dL. Ketiganya berbeda. Bila
batas nilai ditempelkan sebagai kolom pada `MstProcedure`, Hemoglobin hanya punya **satu** baris
sehingga ketiga batas itu tidak mungkin disimpan sekaligus. Hasil pemeriksaan seorang anak akan
dinilai memakai batas orang dewasa.

Ada pula bentuk hasil yang bukan angka sama sekali. Protein urin keluar sebagai Negatif, +1,
+2, +3, atau +4. Bila analis mengetiknya bebas, satu orang menulis "+4", yang lain "Positif
kuat (4+)", yang ketiga "protein +4" — dan sistem tidak akan pernah mengenali ketiganya sebagai
hal yang sama, sehingga nilai kritis tidak pernah terdeteksi.

Task ini menyediakan **fondasi datanya**: dua tabel baru milik modul Laboratorium yang mampu
menyimpan beberapa baris batas per jenis pemeriksaan, dalam dua bentuk hasil. Layar dan endpoint
pengelolaannya adalah pekerjaan `BE-LAB-04`, yang menjadikan task ini sebagai dependency-nya.

---

## 2. Proses bisnis

**Tujuan.** Setiap jenis pemeriksaan laboratorium memiliki batas nilai rujukannya sendiri, yang
dapat dibedakan menurut jenis kelamin dan kelompok umur, dalam dua bentuk hasil: angka dan
pilihan terbatas.

**Pelaku.** Kepala instalasi laboratorium sebagai pemegang kewenangan isi batas nilai. Task ini
menyiapkan tempat penyimpanannya; layar tempat kepala instalasi bekerja dibangun `BE-LAB-04`.

**Pemicu.** Rumah sakit menetapkan atau meninjau ulang nilai rujukan sebuah pemeriksaan.

**Langkah yang berurutan:**

1. Kepala instalasi menentukan **bentuk hasil** pemeriksaan itu, tepat satu dari dua: hasil
   angka atau hasil pilihan terbatas. Bentuk ini menentukan seluruh langkah berikutnya.
2. Untuk **hasil angka**, ia mengisi satuan — misalnya `g/dL` — beserta batas normal bawah dan
   atas, dan batas kritis bawah dan atas.
3. Untuk **hasil pilihan terbatas**, ia tidak mengisi satuan maupun batas angka. Ia mendaftarkan
   pilihan-pilihan yang sah beserta dua penanda untuk masing-masing: apakah pilihan itu di luar
   nilai rujukan, dan apakah pilihan itu kritis.
4. Ia menentukan untuk **kelompok pasien mana** baris ini berlaku: jenis kelamin (semua, pria,
   atau wanita) dan kelompok umur. Kelompok umur boleh dikosongkan, yang berarti berlaku untuk
   semua umur.
5. Bila perlu, ia mengisi batas waktu penyelesaian pemeriksaan cito, dihitung sejak wadah
   dinyatakan layak.
6. Baris tersimpan. Satu jenis pemeriksaan boleh memiliki **beberapa** baris seperti ini, selama
   kombinasi pemeriksaan, jenis kelamin, dan kelompok umurnya berbeda.

**Aturan yang berlaku:**

| Aturan | Isi |
| --- | --- |
| `VAL-21` | Tidak boleh ada dua baris batas untuk kombinasi pemeriksaan, jenis kelamin, dan kelompok umur yang sama |
| `VAL-22` | Bentuk hasil angka wajib punya satuan |
| `VAL-23` | Bentuk hasil pilihan wajib punya sekurang-kurangnya satu pilihan |
| `VAL-24` | Bentuk hasil angka tidak boleh punya daftar pilihan |
| `LAB-DEC-018` | Batas nilai tinggal di tabel milik Laboratorium, bukan sebagai kolom pada `MstProcedure` |
| `LAB-DEC-036` | Satu-satunya kolom Laboratorium yang boleh ditambahkan ke `MstProcedure` adalah penanda disiplin — dan itu pun bukan pekerjaan task ini, melainkan `BE-EXT-01` milik `master-data` |

**Contoh berangka.** Hemoglobin tersimpan sebagai tiga baris pada satu jenis pemeriksaan yang
sama:

| Jenis kelamin | Kelompok umur | Satuan | Normal bawah | Normal atas | Kritis bawah | Kritis atas |
|---|---|---|---:|---:|---:|---:|
| Pria | Dewasa | g/dL | 13,0 | 17,0 | 7,0 | 20,0 |
| Wanita | Dewasa | g/dL | 12,0 | 15,0 | 7,0 | 20,0 |
| Semua | Anak | g/dL | 11,0 | 14,0 | 6,0 | 18,0 |

Protein urin tersimpan sebagai satu baris berbentuk pilihan, dengan lima pilihan di bawahnya:

| Kode | Nama | Di luar rujukan | Kritis |
|---|---|:---:|:---:|
| `NEG` | Negatif | Tidak | Tidak |
| `P1` | +1 | Ya | Tidak |
| `P2` | +2 | Ya | Tidak |
| `P3` | +3 | Ya | **Ya** |
| `P4` | +4 | Ya | **Ya** |

Golongan darah dan tes kehamilan juga berbentuk pilihan, tetapi seluruh penanda kritisnya
kosong — tidak ada golongan darah yang berbahaya. Itu keadaan yang sah, bukan data yang belum
diisi.

**Status yang dihasilkan.** Tidak ada status baru dan tidak ada perpindahan status. Kedua tabel
ini adalah data induk, bukan transaksi.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Baris kedua dibuat untuk kombinasi pemeriksaan, jenis kelamin, dan kelompok umur yang sama persis | **Ditolak database** lewat index unik `IX_LabValueBound_Procedure_Gender_AgeCategory`. Terbukti langsung, lihat bagian 5.1 |
| Dua baris dibuat untuk kombinasi yang sama tetapi kelompok umurnya **dikosongkan** | **Diterima.** Ini celah nyata dan disengaja dibiarkan; lihat bagian 3.3 |
| Dua pilihan dengan kode sama didaftarkan pada satu batas nilai | Ditolak database lewat index unik `IX_LabValueOption_ValueBoundId_OptionCode` |
| Batas nilai dihapus | Soft delete. Baris berpenanda `IsDelete` tidak lagi menghalangi pembuatan baris baru untuk kelompok pasien yang sama, karena kedua index unik memakai filter `"IsDelete" = false` |
| Batas nilai dihapus sementara masih punya pilihan | Pilihannya ikut terhapus (`Cascade`) — pilihan tidak punya makna tanpa batas nilai induknya |
| Jenis pemeriksaan atau kelompok umur dihapus dari data induk global sementara masih dirujuk | Ditolak database (`Restrict`) |

**Hasil akhir.** Sistem kini punya tempat untuk menyimpan batas nilai per kelompok pasien dalam
dua bentuk hasil, dan bentuk itu tersedia baik pada model aplikasi maupun pada schema database
dev pemilik.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola:**

- `AGENTS.md`
- `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `docs/engineering/QBE_EXCEPTIONS.json`
- `tooling/qbe/Invoke-QbeConformanceCheck.ps1`
- `rules/GLOBAL_RULES.md`; `rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`; `rules/backend/DATABASE_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`

**Blueprint:**

- `roadmap/backend-roadmap.md` bagian 1, 2, 3, dan 7; `roadmap/traceability.md`
- `contracts/api-contract.md` grup Lab Value Bound; `contracts/validation-matrix.md` `VAL-21` .. `VAL-24`
- `00-interview-decisions.md` (BR-14, BR-17, `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021`, `LAB-DEC-036`, daftar acceptance criteria)
- `02-backend-architecture.md` bagian 3.2 dan 597; `04-prd-to-mvp.md` bagian `FR-03`
- `erd/data-dictionary.md` bagian 5, 6, 11.2, dan 11.3

**Source:**

- `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs`
- `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`; `.../Models/MstLabRejectionReason.cs`
- `Areas/HealthServices/MasterData/Models/MstProcedure.cs`; `.../Models/MstAgeCategory.cs`
- `Models/IdentityModel.cs`
- `Repositories/ApplicationDbContext.cs`
- `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs`; `.../LaboratoryManagement/MstLabRejectionReasonConfiguration.cs`; `.../LaboratoryManagement/TrxLabSpecimenConfiguration.cs`
- `QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj`; `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabOrderDisciplineTests.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs` | Menambah dua enum: `LabResultForm` berisi `Numeric = 1` dan `Choice = 2` (`LAB-DEC-021`), serta `LabGenderScope` berisi `All = 1`, `Male = 2`, `Female = 3` (BR-14). Nilai enum lama tidak disentuh sama sekali |
| `Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs` | **Baru.** Entity batas nilai beserta penjelasan mengapa ia berdiri sendiri dan bukan kolom pada `MstProcedure` |
| `Areas/HealthServices/LaboratoryManagement/Models/LabValueOption.cs` | **Baru.** Entity pilihan hasil, beserta alasan mengapa penanda "di luar rujukan" dan "kritis" dipisah |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundConfiguration.cs` | **Baru.** Memetakan tabel `public."LabValueBound"`, enum sebagai `int`, empat kolom batas ber-presisi `18,4`, index unik `VAL-21` berfilter soft delete, dan tiga relasi — `Restrict` ke `MstProcedure` dan `MstAgeCategory`, `Cascade` ke `LabValueOption` |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueOptionConfiguration.cs` | **Baru.** Memetakan tabel `public."LabValueOption"` beserta index unik kode pilihan per batas nilai |
| `Repositories/ApplicationDbContext.cs` | Menambah dua `DbSet` — `LabValueBounds` dan `LabValueOptions` — di dalam region `HEALTH SERVICE - Laboratory Management`. Tepat 4 baris; tidak ada region lain yang tersentuh |
| `Migrations/20260902082722_AddLabValueBoundAndOption.cs` | **Baru.** `Up` membuat dua tabel beserta dua foreign key, satu foreign key cascade, dan tiga index. `Down` membuang kedua tabel |
| `Migrations/20260902082722_AddLabValueBoundAndOption.Designer.cs` | **Baru.** Berkas hasil generate yang menyertai migration |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Bertambah 191 baris, **nol baris terhapus**. Seluruhnya milik kedua entity baru; diperiksa dan tidak ada operasi modul lain yang ikut terbawa |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundTests.cs` | **Baru.** Sepuluh kasus uji yang membuktikan seluruh acceptance criteria task ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Task ini **tidak** menambah, menghapus, mengganti nama, maupun mengubah satu pun endpoint, DTO, atau kode status. Enam endpoint grup Lab Value Bound pada `LAB-API-v1` r3 tetap berstatus **Rencana (belum tersedia)**; membangunnya adalah cakupan `BE-LAB-04` |
| Database | Dua tabel baru `public."LabValueBound"` dan `public."LabValueOption"`, dua foreign key `Restrict` ke data induk global, satu foreign key `Cascade` internal, dan tiga index. Migration **sudah dibuat dan sudah diterapkan** ke `QuilvianNewDevYoga`, database dev pemilik, atas wewenang eksplisit pada sesi ini. Jalur `Down` ikut dibuktikan. Lihat bagian 5.1 |
| Keamanan/Auth | `NOT APPLICABLE`. Tidak ada permission baru, tidak ada endpoint, dan tidak ada data sensitif pasien yang disimpan — kedua tabel berisi nilai rujukan, bukan hasil pasien. Hak akses `LabValueBound : Read`, `: Create`, dan `: Update` pada `LAB-PERM-v1` r3 baru didaftarkan `BE-LAB-04` bersama endpointnya |

**Temuan pertama: celah `NULL` pada index unik `VAL-21`.**

`erd/data-dictionary.md` bagian 11.2 menetapkan index unik atas
`("ProcedureId", "GenderScope", "AgeCategoryId")`. Index itu dibuat persis seperti tertulis.

Yang tidak dibahas ERD: **PostgreSQL memperlakukan `NULL` sebagai selalu berbeda dari `NULL`
lain.** Sementara itu `AgeCategoryId` yang kosong justru punya arti — "berlaku untuk semua
umur". Akibatnya dua baris "Kalium / semua jenis kelamin / semua umur" **lolos** dari index unik,
padahal itu persis kasus Kalium pada contoh BR-14.

Ini bukan dugaan. Diuji langsung terhadap database dev di dalam transaksi yang di-rollback:

| Percobaan | Hasil sebenarnya |
| --- | --- |
| Tiga baris Hemoglobin berbeda kombinasi | Tersimpan berdampingan, 3 baris |
| Baris keempat berkombinasi sama persis | **Ditolak** — `duplicate key value violates unique constraint "IX_LabValueBound_Procedure_Gender_AgeCategory"` |
| Dua baris berkelompok umur kosong, kombinasi lain sama | **Diterima**, 2 baris tersimpan |

Server `QuilvianNewDevYoga` berjalan pada **PostgreSQL 15.15**, sehingga `NULLS NOT DISTINCT`
sebenarnya tersedia dan celah ini dapat ditutup di lapisan database. Pemilik modul memilih
**mempertahankan index persis seperti ERD** dan mencatat celahnya, sehingga penegakan `VAL-21`
untuk kasus "semua umur" menjadi tanggungan service pada `BE-LAB-04`.

**Rekomendasi:** saat `BE-LAB-04` menulis pemeriksaan `VAL-21` di service, pemeriksaan itu wajib
memperlakukan `AgeCategoryId IS NULL` sebagai satu kelompok tersendiri — jangan mengandalkan
index unik untuk kasus itu. Bila kelak diputuskan menutupnya di database, perubahannya kecil:
satu migration yang membuat ulang index dengan `NULLS NOT DISTINCT`.

**Temuan kedua: `SortOrder` yang dipersistensi.**

`erd/data-dictionary.md` bagian 5 dan 6 menetapkan kolom `SortOrder` pada **kedua** entity.
Sementara itu `BACKEND_ENGINEERING_CONTRACT.md` bagian *Entity, penamaan, dan PostgreSQL*
menyatakan `SortOrder` presentasi yang dipersistensi secara generik **dilarang untuk kode baru**,
dan urutan bisnis yang sesungguhnya memakai field semantik (QBE-ENT-003).

Yang diambil: kedua kolom dibuat persis seperti ERD, karena ERD adalah rancangan yang sudah
disetujui dan menjadi dasar `BE-LAB-03`, `BE-LAB-04`, serta roadmap frontend. Menghapus kolom
yang disebut ERD berarti mendefinisikan ulang rancangan yang disetujui secara sepihak.

Selisihnya disebut apa adanya, dengan pembedaan yang jujur antara keduanya:

| Kolom | Sifat | Penilaian terhadap QBE-ENT-003 |
| --- | --- | --- |
| `LabValueOption.SortOrder` | Menyatakan tingkatan skala ordinal hasil — Negatif, +1, +2, +3, +4 | **Semantik.** Urutan ini isi bisnis, bukan tampilan; aturan tidak dilanggar |
| `LabValueBound.SortOrder` | Urutan tampil baris batas pada layar pengelolaan | **Presentasi.** Ini selisih nyata terhadap QBE-ENT-003 dan perlu keputusan pemilik |

Checker QBE tidak memblokir keduanya — QBE-ENT-003 berkelayakan `REVIEW_ONLY` dan memang tidak
diotomasi. **Rekomendasi:** putuskan bersama pemilik apakah `LabValueBound.SortOrder`
dipertahankan atau dibuang lewat amandemen ERD tersendiri, sebaiknya sebelum `BE-LAB-04`
menerbitkan endpoint yang mengeksposnya.

**Selisih kecil yang sudah diselesaikan.** Blok DDL pada ERD menuliskan `DEFAULT true` untuk
`IsActive` dan `DEFAULT 0` untuk `SortOrder`. Keduanya diwujudkan sebagai nilai bawaan pada
model C#, bukan sebagai default kolom database. Alasannya konkret: dengan `HasDefaultValue(true)`
pada kolom `boolean`, EF Core menghilangkan kolom itu dari perintah insert ketika nilainya sama
dengan bawaan CLR — yaitu `false` — sehingga baris yang sengaja dibuat **tidak aktif** justru
tersimpan sebagai aktif. Pola ini sudah terlanjur ada di modul lain repository dan tidak
diteruskan ke kode baru. Blok DDL pada ERD sendiri menyatakan dirinya "bentuk tabel sebagaimana
dihasilkan EF Core, bukan skrip untuk dijalankan", sehingga tidak ada kontrak yang dilanggar.

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Cakupannya berhenti pada entity,
configuration, `DbSet`, dan migration, persis seperti tertulis pada roadmap.

Enam endpoint grup **Health Services / Laboratory Management / Lab Value Bound** pada
`LAB-API-v1` r3 tetap berstatus **Rencana (belum tersedia)** dan dibangun `BE-LAB-04`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil | `PASS` | `0 Error(s)`, `186 Warning(s)` — jumlahnya sama persis dengan baseline sebelum task ini; seluruhnya CS1573/CS1574/CS1587 komentar XML milik modul lain |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | Lolos | `PASS` | `Files evaluated: 7`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` |
| `dotnet ef migrations has-pending-model-changes` sesudah migration dibuat | Tidak ada selisih tersisa | `PASS` | `No changes have been made to the model since the last migration.`, exit code 0 |
| `AC-24` — tiga baris batas Hemoglobin tersimpan berdampingan | Sesuai harapan | `PASS` | `TigaBarisBatasHemoglobin_TersimpanBerdampinganUntukSatuPemeriksaan` |
| Batas berlaku semua umur menyimpan kelompok umur kosong | Sesuai harapan | `PASS` | `BatasBerlakuSemuaUmur_MenyimpanKelompokUmurKosong` |
| `VAL-21` — kombinasi dijaga unik oleh index database berfilter soft delete | Sesuai harapan | `PASS` | `KombinasiPemeriksaanJenisKelaminKelompokUmur_UnikDiDatabase` |
| **Gagal** — baris keempat berkombinasi sama, diuji terhadap database sungguhan | Ditolak index unik | `PASS` | Bagian 5.1, probe yang di-rollback |
| `AC-28` — batas bentuk pilihan menyimpan daftar pilihan beserta dua penandanya | Sesuai harapan | `PASS` | `BatasBentukPilihan_MenyimpanDaftarPilihanBesertaPenandanya` |
| Kode pilihan unik dalam satu batas nilai | Sesuai harapan | `PASS` | `KodePilihan_UnikDalamSatuBatasNilai` |
| `AC-25` — `MstProcedure` tidak bertambah kolom operasional laboratorium | Sesuai harapan | `PASS` | `MstProcedure_TidakBertambahKolomOperasionalLaboratorium`, dan dibuktikan ulang di database: jumlah kolom `MstProcedure` tetap **35** sebelum `Up`, sesudah `Up`, sesudah `Down`, dan sesudah `Up` ulang |
| `AC-49` — kedua entity tinggal di modul Laboratorium dan hanya menunjuk data induk global | Sesuai harapan | `PASS` | `KeduaEntity_TinggalDiModulLaboratoriumDanHanyaMenunjukDataIndukGlobal` |
| Pemetaan `LabValueBound` sesuai kamus data | Sesuai harapan | `PASS` | `LabValueBound_TerpetakanSesuaiKamusData` |
| Pemetaan `LabValueOption` dan perilaku `Cascade` | Sesuai harapan | `PASS` | `LabValueOption_TerpetakanSesuaiKamusDataDanIkutTerhapusBersamaInduknya` |
| Enum memuat nilai yang diputuskan | Sesuai harapan | `PASS` | `EnumBentukHasilDanPembatasJenisKelamin_MemuatNilaiYangDiputuskan` |
| Seluruh test `LabValueBoundTests` | Hijau | `PASS` | `dotnet test --filter FullyQualifiedName~LabValueBoundTests` → `Failed: 0, Passed: 10, Skipped: 0, Total: 10` |
| Uji ulang pada kondisi tree final, sesudah migration dibuat — `LabValueBoundTests` bersama `LabOrderDisciplineTests` | Hijau | `PASS` | `Failed: 0, Passed: 19, Skipped: 0, Total: 19`. Dijalankan paling akhir supaya bukti test benar-benar mencerminkan isi worktree yang diserahkan, bukan keadaan di tengah pengerjaan |
| Seluruh suite `QuilvianSystemBackend.Tests` | 862 lulus, 1 gagal | `EXISTING / ENVIRONMENT ISSUE` | `Failed: 1, Passed: 862, Total: 863, Duration: 37 s`. Satu-satunya kegagalan adalah `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` milik modul Billing — temuan `FINAL`/`CLOSED` yang sudah tercatat pada `BE-LAB-01` bagian 9.2 dan `approval-requests/2026-09-02-temuan-billing-final-closed.md`. Baseline sebelum task ini `Passed: 852, Failed: 1`; selisihnya tepat **+10**, yaitu test baru task ini, sehingga tidak ada regresi |
| Eksekusi migration `Up` ke `QuilvianNewDevYoga` | Berhasil | `PASS` | Bagian 5.1 |
| Eksekusi migration `Down` ke `QuilvianNewDevYoga` | Berhasil, database kembali persis ke keadaan semula | `PASS` | Bagian 5.1 |
| Eksekusi `Up` ulang sesudah `Down` | Berhasil | `PASS` | Bagian 5.1 |
| Empat migration tertunda milik modul lain tetap tidak tersentuh | Terbukti | `PASS` | `dotnet ef migrations list` sesudah eksekusi: `AddRadiologyManagement`, `AddCompanyGuarantorToPatientEncounterGuarantor`, `RenameClinicalMilestoneFactToCliPrefix`, dan `RepairCanonicalModelSnapshotBaseline` seluruhnya masih `(Pending)` |
| Uji lewat HTTP sungguhan | Tidak dijalankan | `NOT APPLICABLE` | Task ini tidak menghasilkan satu pun endpoint |

Uji manual lewat antarmuka: `NOT APPLICABLE` — tidak ada layar maupun endpoint yang dihasilkan
task ini.

**Tidak dijalankan:**

- **Empat migration tertunda milik modul lain.** Bukan wewenang task ini. Riwayat migration pada
  database ini masih tidak berurutan, sehingga `dotnet ef database update` polos tetap berbahaya
  bagi siapa pun yang menjalankannya.
- **Eksekusi ke database selain `QuilvianNewDevYoga`.** Tidak ada database lain yang disentuh.
- **`VAL-22`, `VAL-23`, dan `VAL-24`.** Ketiganya adalah validasi request pada endpoint
  pengelolaan, dan endpoint itu cakupan `BE-LAB-04`. Bentuk data yang membuat ketiganya dapat
  ditegakkan sudah tersedia: `ResultForm`, `Unit`, dan relasi `Options`.
- **Penolakan `409` beserta pesan `VAL-21`.** Kode status dan pesan diterbitkan endpoint, yang
  juga cakupan `BE-LAB-04`. Yang dibuktikan di sini adalah constraint database yang membuat
  penolakan itu dapat ditegakkan.

### 5.1 Bukti eksekusi migration

**Wewenang.** Pemilik repository memilih "buat migration dan jalankan ke `QuilvianNewDevYoga`"
pada sesi ini, dan memilih mempertahankan index unik persis seperti ERD.

**Gerbang yang ditemukan sebelum eksekusi.** `dotnet ef migrations list` menunjukkan **lima**
migration tertunda, bukan satu, dengan riwayat yang tidak berurutan — keadaan yang sama seperti
yang dicatat `BE-LAB-01`. `dotnet ef database update` akan menerapkan kelima-limanya, termasuk
pembuatan tabel modul Radiology dan rename tabel Clinical Milestone Fact, jauh melampaui wewenang
yang diberikan.

Karena itu yang dijalankan hanya SQL milik migration ini. SQL-nya **tidak ditulis tangan**,
melainkan dihasilkan EF sendiri supaya tidak ada kemungkinan salah salin:

```text
dotnet ef migrations script 20260902042242_AddLabOrderDiscipline 20260902082722_AddLabValueBoundAndOption
dotnet ef migrations script 20260902082722_AddLabValueBoundAndOption 20260902042242_AddLabOrderDiscipline
```

Urutan migration diperiksa lebih dulu: `AddLabOrderDiscipline` adalah migration tepat sebelum
migration ini, tanpa satu pun migration lain di antaranya, sehingga skrip yang dihasilkan berisi
tepat satu migration.

`psql` tidak terpasang pada mesin ini, sehingga eksekusinya lewat runner Npgsql sementara di luar
repository yang membaca connection string dari `appsettings.Development.json` saat berjalan.
Runner itu menolak berjalan bila nama database tujuannya bukan `QuilvianNewDevYoga`, menjalankan
setiap berkas dalam satu transaksi, dan tidak pernah menuliskan maupun mencetak credential.

**Urutan bukti yang direkam:**

| Langkah | Tabel `LabValueBound` dan `LabValueOption` | Index | Foreign key | Baris `__EFMigrationsHistory` | Kolom `MstProcedure` |
| --- | --- | --- | --- | --- | --- |
| Sebelum apa pun dijalankan | TIDAK ADA | TIDAK ADA | TIDAK ADA | TIDAK ADA | 35 |
| Sesudah `Up` | ADA, seluruh kolom sesuai kamus data | 3 index + 2 primary key | 3 | ADA | 35 |
| Sesudah `Down` | TIDAK ADA | TIDAK ADA | TIDAK ADA | TIDAK ADA | 35 |
| Sesudah `Up` ulang | ADA | 3 index + 2 primary key | 3 | ADA | 35 |

Index yang terbaca langsung dari `pg_indexes` sesudah `Up`:

```text
CREATE UNIQUE INDEX "IX_LabValueBound_Procedure_Gender_AgeCategory"
    ON public."LabValueBound" USING btree ("ProcedureId", "GenderScope", "AgeCategoryId")
    WHERE ("IsDelete" = false)
CREATE UNIQUE INDEX "IX_LabValueOption_ValueBoundId_OptionCode"
    ON public."LabValueOption" USING btree ("ValueBoundId", "OptionCode")
    WHERE ("IsDelete" = false)
CREATE INDEX "IX_LabValueBound_AgeCategoryId"
    ON public."LabValueBound" USING btree ("AgeCategoryId")
```

Foreign key yang terbaca langsung dari `pg_constraint` sesudah `Up`:

```text
FK_LabValueBound_MstAgeCategory_AgeCategoryId  → "MstAgeCategory"("Id")  ON DELETE RESTRICT
FK_LabValueBound_MstProcedure_ProcedureId      → "MstProcedure"("Id")    ON DELETE RESTRICT
FK_LabValueOption_LabValueBound_ValueBoundId   → "LabValueBound"("Id")   ON DELETE CASCADE
```

**Probe perilaku yang di-rollback.** Sesudah `Up`, satu blok uji dijalankan terhadap database
sungguhan **di dalam transaksi yang selalu dibatalkan**, untuk membuktikan index unik benar-benar
menolak — bukan sekadar terdaftar pada model. Hasilnya:

```text
tiga baris berdampingan: 3
baris keempat duplikat: DITOLAK oleh duplicate key value violates unique constraint
                        "IX_LabValueBound_Procedure_Gender_AgeCategory"
dua baris semua-umur:   DITERIMA, 2 baris (celah NULL terbukti nyata)
```

Sesudah rollback, `LabValueBound` dan `LabValueOption` kembali berisi **nol baris**, dan
`MstProcedure` beserta `MstAgeCategory` tidak berubah jumlah barisnya. Tidak ada satu pun baris
bisnis yang ditinggalkan.

**Keadaan akhir database:** termigrasi. `dotnet ef migrations list` kini menampilkan
`20260902082722_AddLabValueBoundAndOption` tanpa penanda `(Pending)`, sementara keempat migration
milik modul lain tetap `(Pending)`.

**Catatan keadaan data.** Kedua tabel dibuat kosong, dan keduanya benar-benar baru — tidak ada
data lama yang perlu dipindahkan, ditulis ulang, atau ditebak nilainya.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-24` — satu jenis pemeriksaan dapat memiliki lebih dari satu baris batas nilai yang dibedakan menurut jenis kelamin dan kelompok umur | **Terpenuhi** | `TigaBarisBatasHemoglobin_TersimpanBerdampinganUntukSatuPemeriksaan` membuktikan ketiga baris Hemoglobin tersimpan pada satu `ProcedureId` yang sama. Dibuktikan ulang terhadap database sungguhan pada bagian 5.1: tiga baris tersimpan, baris keempat berkombinasi sama ditolak index unik |
| `AC-25` — `MstProcedure` tidak bertambah satu pun kolom **operasional** laboratorium; satu-satunya tambahan yang diizinkan adalah kolom klasifikasi disiplin | **Terpenuhi** | `MstProcedure_TidakBertambahKolomOperasionalLaboratorium` menelusuri seluruh properti `MstProcedure` pada model EF sesudah seluruh migration dan memastikan dua belas nama kolom operasional tidak ada, sekaligus memastikan atribut itu memang tinggal di `LabValueBound`. Dibuktikan ulang di database: jumlah kolom `MstProcedure` tetap 35 pada keempat titik pengukuran bagian 5.1 |
| `AC-28` — pemeriksaan berhasil pilihan hanya menerima nilai dari daftar pilihan yang sah; pengetikan bebas ditolak sistem | **Terpenuhi untuk cakupan task ini** | `BatasBentukPilihan_MenyimpanDaftarPilihanBesertaPenandanya` membuktikan daftar pilihan yang sah tersimpan beserta penanda di luar rujukan dan penanda kritisnya, dan `KodePilihan_UnikDalamSatuBatasNilai` membuktikan kode pilihan dijaga unik per batas nilai. Penolakan pengetikan bebas saat **hasil diinput** terjadi pada jalur input hasil, yang bukan cakupan `BE-LAB-02` maupun `BE-LAB-04`; task ini menyediakan daftar sah yang membuat penolakan itu mungkin |
| `AC-49` — data induk khusus Laboratorium berada di folder Laboratorium dan data induk global tidak disalin ke sana | **Terpenuhi** | `KeduaEntity_TinggalDiModulLaboratoriumDanHanyaMenunjukDataIndukGlobal`. Kedua entity berada di `Areas/HealthServices/LaboratoryManagement/Models/`, dan `MstProcedure` serta `MstAgeCategory` hanya ditunjuk lewat foreign key `Restrict`, tidak disalin |

### 6.2 Definition of Done

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Nama tabel sesuai jawaban registry | **Terpenuhi** | `LabValueBound` dan `LabValueOption`, persis seperti keputusan registry 2026-09-02. Terbaca dari database sesudah `Up`, bukan hanya dari source |
| Dua entity ada beserta configuration di `Repositories/Configurations/HealthServices/LaboratoryManagement/` | **Terpenuhi** | `LabValueBoundConfiguration.cs` dan `LabValueOptionConfiguration.cs` berada tepat di folder itu |
| Migration jalan dua arah | **Terpenuhi** | Dibuktikan terhadap `QuilvianNewDevYoga`: `Up` → dua tabel, tiga index, dan tiga foreign key ada; `Down` → seluruhnya hilang dan database kembali persis ke keadaan semula; `Up` ulang → seluruhnya ada lagi. Rinciannya pada bagian 5.1 |
| `AC-25` terbukti | **Terpenuhi** | Dua lapis bukti: uji terhadap model EF, dan hitungan kolom `MstProcedure` di database pada keempat titik pengukuran |
| Checker QBE lolos | **Terpenuhi** | `Final result: PASS`, `VIOLATION: 0` pada mode `Strict` |

**Seluruh butir DoD terpenuhi.** Dua hal berikut tetap disebut apa adanya karena keduanya
**bukan** butir DoD `BE-LAB-02`, melainkan temuan yang perlu keputusan pemilik:

1. **Celah `NULL` pada index unik `VAL-21`.** Dua baris "semua umur" untuk kombinasi yang sama
   masih lolos. Pemilik memilih mempertahankan index sesuai ERD; penegakannya menjadi tanggungan
   service `BE-LAB-04`. Lihat bagian 3.3.
2. **`LabValueBound.SortOrder` adalah kolom presentasi yang dipersistensi**, yang berselisih
   dengan QBE-ENT-003 sementara ERD menyebutnya. Lihat bagian 3.3.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build backend menghasilkan 186 warning, seluruhnya CS1573/CS1574/CS1587 tentang komentar XML pada modul Inpatient, Medical Record, Pharmacy, dan Registration. Tidak satu pun berasal dari berkas yang diubah task ini, dan jumlahnya tidak bertambah dari baseline |
| Masalah yang diketahui | Dua temuan pada bagian 3.3 — celah `NULL` pada index unik `VAL-21`, dan `LabValueBound.SortOrder` yang berselisih dengan QBE-ENT-003. Keduanya sengaja ditinggalkan sebagai keputusan pemilik, bukan didiamkan. Di luar Laboratorium, satu test Billing tetap merah sejak sebelum task ini |
| Risiko tersisa | **Pertama**, `QuilvianNewDevYoga` sudah dimigrasi, tetapi **database lain belum** — menjalankan kode ini terhadap database yang belum menerima migration akan gagal pada setiap query kedua tabel ini. Berkas migrationnya tersedia, dan penerapannya ke database lain adalah wewenang tersendiri. **Kedua**, celah `NULL` di atas berarti `VAL-21` belum tegak penuh sampai `BE-LAB-04` menuliskannya di service. **Ketiga**, kedua tabel masih kosong dan belum punya jalur pengisian; sampai `BE-LAB-04` selesai, batas nilai hanya dapat diisi lewat perintah database langsung. **Keempat**, di luar Laboratorium: `QuilvianNewDevYoga` masih punya empat migration tertunda dengan riwayat tidak berurutan, sehingga `dotnet ef database update` polos pada database ini tetap berbahaya |
| Perubahan sampingan | `NONE`. Diff `Migrations/ApplicationDbContextModelSnapshot.cs` diperiksa: 191 baris tambahan, **nol baris terhapus**, seluruhnya milik kedua entity baru. Satu komentar pada `LabValueBoundConfiguration.cs` sempat diubah kata-katanya — semula memuat kata `IdentityModel`, yang membuat checker QBE keliru memperlakukan berkas configuration itu sebagai entity persisted dan menerbitkan tiga violation palsu. Kata itu diganti; tidak ada satu baris kode pun yang berubah karenanya |
| Interupsi | `NONE` |
| Selisih snapshot source | Roadmap menyebut Backend SHA `c87d9c0`; pekerjaan ini berjalan di atas `d8d67c3`. Permukaan yang disentuh diperiksa langsung terhadap source saat ini dan **cocok** dengan rancangan: `MstProcedure` masih 35 kolom tanpa atribut laboratorium, `MstAgeCategory` masih ada dengan bentuk yang sama, dan belum ada satu pun tabel batas nilai. Karena itu selisih SHA tidak menahan task ini |
| Status Git | Lihat di bawah |
| Langkah berikutnya | **1.** Putuskan penanganan celah `NULL` pada `VAL-21` — tutup di database lewat `NULLS NOT DISTINCT`, atau tegakkan di service `BE-LAB-04`. Server sudah PostgreSQL 15.15, jadi kedua jalur terbuka. **2.** Putuskan nasib `LabValueBound.SortOrder` terhadap QBE-ENT-003, sebaiknya sebelum `BE-LAB-04` mengeksposnya lewat endpoint. **3.** `BE-LAB-03` kini tidak lagi tertahan; ia hanya menunggu task ini, yang sudah selesai. **4.** Terapkan migration ini ke database lain yang membutuhkannya, lewat wewenang tersendiri. **5.** Bereskan empat migration tertunda pada `QuilvianNewDevYoga` bersama pemilik modul masing-masing |

**Keluaran `git status --short` di akhir pekerjaan:**

```text
 M Areas/HealthServices/LaboratoryManagement/Enums/LaboratoryEnums.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/ApplicationDbContext.cs
 M docs/module-blueprints/laboratorium/roadmap/backend-roadmap.md
 M docs/module-blueprints/laboratorium/roadmap/traceability.md
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs
?? Areas/HealthServices/LaboratoryManagement/Models/LabValueOption.cs
?? Migrations/20260902082722_AddLabValueBoundAndOption.Designer.cs
?? Migrations/20260902082722_AddLabValueBoundAndOption.cs
?? QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundTests.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundConfiguration.cs
?? Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueOptionConfiguration.cs
?? docs/module-blueprints/laboratorium/task/report/backend/BE-LAB-02.md
```

Tidak ada `git add`, `commit`, `push`, `merge`, maupun `rebase` yang dijalankan.

**Pembaruan register yang ikut ditulis.**

| Berkas | Perubahan |
| --- | --- |
| `roadmap/backend-roadmap.md` | Blok status `SELESAI` beserta tautan laporan pada bagian 3; baris `BE-LAB-02` pada tabel bagian 7; penahan `BE-LAB-03` dan `BE-LAB-04` disesuaikan |
| `roadmap/traceability.md` | Baris `FR-03.1`, `FR-03.2`, `FR-03.6`, dan `FR-01.4` diperbarui buktinya. `FR-01.4` ikut tersentuh karena kolom `CitoTurnaroundMinutes` adalah bagian penyimpanannya, sementara `AC-17` — daftar pantau keterlambatan cito — tetap cakupan `BE-LAB-14`. Ketiganya sebelumnya masih tertulis `BLOCKED` `LAB-OPEN-021`, padahal penahan itu sudah dicabut 2026-09-02 — utang pembukuan yang ikut dibereskan di sini |

Tidak ada artefak blueprint lain yang disentuh: kontrak, kamus data, ERD, matriks uji, dan
dokumen arsitektur tidak berubah satu baris pun.

---

## 8. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement` |
| Submodule | — (tidak ada) |
| Pemilik/prefix pada registry | `LaboratoryManagement / Laboratory`, prefix `Lab`, Category `BUSINESS DOMAIN / MODULE` |
| Status registry | `ACTIVE` sejak 2026-09-02, dinaikkan dari `PLANNED` oleh Muhammad Hamzah lewat `LAB-REQ-002`. Wewenangnya mencakup source **dan pembuatan migration** |
| Keberlakuan | `NEW CODE` |
| Sumber tata kelola | `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` seluruhnya terbaca. `docs/engineering/QBE_EXCEPTIONS.json` berisi nol pengecualian, sehingga tidak ada temuan yang disupresi |

### QBE ID yang berlaku

| QBE ID | Bagaimana dipenuhi |
| --- | --- |
| QBE-ENT-001 | `LabValueBound` dan `LabValueOption` keduanya mewarisi `IdentityModel` |
| QBE-ENT-002 | `Guid` untuk kunci dan foreign key; nullability mengikuti semantik domain — `AgeCategoryId` boleh kosong karena "semua umur" memang keadaan yang sah, sementara `ProcedureId` wajib |
| QBE-ENT-003 | Dipatuhi untuk `LabValueOption.SortOrder`, yang semantik. **Berselisih** untuk `LabValueBound.SortOrder`; selisihnya dilaporkan pada bagian 3.3, tidak disamarkan |
| QBE-NAM-001 | Tidak ada satu pun nama `Trx*` yang dibuat |
| QBE-NAM-002 | Kedua entity memakai prefix registry yang disetujui, `Lab` |
| QBE-NAM-004 | Prefix diambil dari keputusan registry 2026-09-02, bukan disimpulkan dari nama folder atau nama task |
| QBE-CFG-001 | Kedua entity punya `IEntityTypeConfiguration<T>` tersendiri beserta mapping, key, index, dan relasinya |
| QBE-MOD-001 | Kedua entity ditempatkan di bawah Area/Module pemiliknya |
| QBE-MOD-002 | Entri registry sudah `ACTIVE` sebelum berkas model pertama dibuat |
| QBE-MOD-003 | Folder `Areas/HealthServices/LaboratoryManagement/` dan `Repositories/Configurations/HealthServices/LaboratoryManagement/` sudah terdaftar dan sudah memuat model persisted sebelumnya |
| QBE-CODE-004 | `OptionCode` bersifat unik dalam satu batas nilai, dan keunikan itu ditegakkan index unik database |
| QBE-DEL-001 | Soft delete `IdentityModel` dihormati: kedua index unik memakai filter `"IsDelete" = false` |
| QBE-ENUM-001 | `LabResultForm` dan `LabGenderScope` dimiliki modul Laboratorium, diletakkan bersama enum modul lainnya |
| QBE-AUD-001 | Kolom audit datang dari `IdentityModel` dan terpisah dari application logging |

### QBE ID yang tidak berlaku

| QBE ID | Alasan |
| --- | --- |
| QBE-NAM-003, QBE-DB-001, QBE-DB-002 | Khusus `LEGACY MIGRATION`. Task ini murni `NEW CODE`; tidak ada rename tabel fisik |
| QBE-SVC-001, QBE-API-001, QBE-DTO-001, QBE-PAGE-001, QBE-OPT-001, QBE-PERM-001 | Task ini tidak menghasilkan controller, service, DTO, endpoint list, options, maupun permission. Seluruhnya cakupan `BE-LAB-04` |
| QBE-CODE-001, QBE-CODE-002, QBE-CODE-003, QBE-CODE-005, QBE-CODE-006 | Tidak ada nomor bisnis yang dialokasikan. `OptionCode` diisi pengguna sebagai kode pilihan, bukan nomor urut yang dibangkitkan sistem |
| QBE-VAL-001 | Validasi request adalah pekerjaan endpoint pengelolaan `BE-LAB-04`. Yang ditegakkan task ini adalah invarian di lapisan database |
| QBE-LOG-001 | Tidak ada perubahan state yang dihasilkan task ini |
| QBE-TXN-001 | Tidak ada workflow multi-record yang ditulis task ini |
| QBE-CFG-002 | Tidak ada configuration legacy yang disentuh |

---

## 10. Amandemen 2026-09-02 — kedua temuan bagian 3.3 ditutup

Pemilik modul menyetujui rekomendasi atas kedua temuan yang tercatat pada bagian 3.3, dan
keduanya dikerjakan pada sesi yang sama lewat satu migration tersendiri,
`20260902091736_AmendLabValueBoundUniquenessAndSortOrder`.

Bagian 3.3 sengaja **tidak** dihapus. Ia tetap berdiri sebagai catatan bagaimana keputusan itu
sampai diambil; bagian ini mencatat hasilnya.

### 10.1 Celah `NULL` pada index unik `VAL-21` — **ditutup di database**

**Keputusan:** tutup di lapisan database dengan `NULLS NOT DISTINCT`, dan tetap pertahankan
pemeriksaan `VAL-21` di dalam service `BE-LAB-04` sebagai lapis kedua.

**Alasannya.** `AgeCategoryId` yang kosong bukan ketiadaan nilai — ia berarti "berlaku untuk
semua umur", sebuah kelompok pasien yang nyata. Memperlakukannya sebagai nilai yang sama adalah
pembacaan yang benar atas maksud datanya, bukan sekadar akal-akalan teknis. Dan karena `VAL-21`
menjaga agar tidak ada dua batas rujukan yang saling bertentangan untuk satu kelompok pasien,
menyandarkannya semata pada service berarti satu jalur tulis baru yang lupa memeriksanya langsung
membuka celahnya kembali. Server `QuilvianNewDevYoga` berjalan pada PostgreSQL 15.15, sehingga
`NULLS NOT DISTINCT` memang tersedia.

**Yang berubah:** `LabValueBoundConfiguration` menambahkan `.AreNullsDistinct(false)` pada index
`IX_LabValueBound_Procedure_Gender_AgeCategory`.

**Buktinya.** Index fisik terbaca dari `pg_indexes` sesudah migration diterapkan:

```text
CREATE UNIQUE INDEX "IX_LabValueBound_Procedure_Gender_AgeCategory"
    ON public."LabValueBound" USING btree ("ProcedureId", "GenderScope", "AgeCategoryId")
    NULLS NOT DISTINCT WHERE ("IsDelete" = false)
```

`pg_index.indnullsnotdistinct` terbaca `True`.

**Kenapa buktinya diambil di database, bukan di uji model.** Setelan ini diterapkan Npgsql pada
tahap finalisasi model relasional, sehingga ia **tidak** terbaca sebagai anotasi index pada
`context.Model`. Tiga cara membacanya dari model sudah dicoba dan seluruhnya gagal — bukan karena
setelannya tidak terpasang, melainkan karena jalur bacanya memang bukan di situ. Memaksakan
pemeriksaan di sana hanya akan menguji pemahaman kita atas API Npgsql, bukan menguji schema yang
sesungguhnya. Karena itu `LabValueBoundTests` memuat catatan yang menunjuk ke bagian ini, dan
buktinya diambil dari `pg_indexes`, `pg_index`, dan percobaan simpan yang sebenarnya.

Probe perilaku yang sama persis dengan bagian 5.1 dijalankan ulang terhadap database sungguhan,
di dalam transaksi yang di-rollback. Perbandingannya:

| Percobaan | Sebelum amandemen | Sesudah amandemen |
| --- | --- | --- |
| Tiga baris Hemoglobin berbeda kombinasi | Tersimpan, 3 baris | Tersimpan, 3 baris — **tidak berubah** |
| Baris keempat berkombinasi sama persis | **Ditolak** | **Ditolak** |
| Dua baris berkelompok umur kosong | **DITERIMA** — celah nyata | **DITOLAK** — celah tertutup |

### 10.2 `LabValueBound.SortOrder` — **dibuang**

**Keputusan:** kolom itu dihapus dari `LabValueBound`. `LabValueOption.SortOrder`
**dipertahankan**.

**Alasannya.** Keduanya bernama sama tetapi bukan hal yang sama.
`LabValueBound.SortOrder` hanya mengatur urutan tampil baris batas pada layar pengelolaan —
kebutuhan presentasi murni, dan `BACKEND_ENGINEERING_CONTRACT` melarangnya untuk kode baru lewat
QBE-ENT-003. Urutan yang bermakna bagi baris batas sudah dapat diturunkan dari datanya sendiri,
yaitu jenis kelamin dan kelompok umur, sehingga tidak ada yang hilang dengan membuangnya.
Sebaliknya `LabValueOption.SortOrder` menyatakan tingkatan skala ordinal hasil — Negatif, +1,
+2, +3, +4 — dan urutan itu adalah isi bisnis, bukan tampilan.

Waktunya dipilih sekarang justru karena `BE-LAB-04` belum menerbitkan endpoint yang
mengeksposnya; membuangnya sesudah itu akan menjadi perubahan kontrak yang merusak.

**Buktinya.** `information_schema.columns` sesudah migration: `LabValueBound` memiliki **nol**
kolom bernama `SortOrder`, sementara `LabValueOption` tetap memilikinya. Diperkuat uji
`LabValueBound_TidakPunyaKolomUrutanTampilYangDipersistensi`, yang memeriksa keduanya sekaligus
supaya penghapusan tidak merembet ke tempat yang salah.

### 10.3 Berkas yang berubah pada amandemen ini

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs` | Properti `SortOrder` dihapus, diganti komentar yang menjelaskan mengapa ia tidak ada dan mengapa `LabValueOption.SortOrder` tetap ada |
| `Repositories/Configurations/HealthServices/LaboratoryManagement/LabValueBoundConfiguration.cs` | Index unik `VAL-21` menambahkan `.AreNullsDistinct(false)` beserta alasannya |
| `Migrations/20260902091736_AmendLabValueBoundUniquenessAndSortOrder.cs` | **Baru.** `Up` membuang index lama, membuang kolom `SortOrder`, lalu membuat ulang index dengan `NULLS NOT DISTINCT`. `Down` mengembalikan keduanya |
| `Migrations/20260902091736_AmendLabValueBoundUniquenessAndSortOrder.Designer.cs` | **Baru.** Berkas hasil generate |
| `QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabValueBoundTests.cs` | Uji index unik bertambah pemeriksaan anotasi `Npgsql:NullsDistinct`, dan satu uji baru untuk ketiadaan `SortOrder` pada `LabValueBound` |

### 10.4 Status database sesudah amandemen

Migration diterapkan ke `QuilvianNewDevYoga` atas wewenang eksplisit pemilik. `LabValueBound`
berisi **nol baris** pada saat kolom dibuang, sehingga tidak ada data yang hilang.

Amandemen ini **tidak** menyentuh `LabValueOption`, `LabValueBoundChangeRequest`,
`LabValueBoundHistory`, maupun tabel modul lain.
