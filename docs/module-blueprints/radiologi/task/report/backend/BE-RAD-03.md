# Laporan Perubahan Backend — `BE-RAD-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-03` |
| Judul | Endpoint aturan keselamatan |
| Slice | `S4` — Pengelolaan aturan keselamatan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-005`, `RAD-DEC-015`, `RJ-BIL-DEC-014`; API `RAD-API-001` bagian 2; permission `RAD-PERM-001` bagian 4; kemampuan `RAD-CAP-003` |
| Contract version | `RAD-API-001` **revision 3** dan `RAD-PERM-001` **revision 3**, keduanya `approved` — diamandemen 2026-09-10 atas persetujuan pemilik modul setelah task ini selesai. Lihat bagian 8 |
| Dependency | `BE-RAD-02` — **terpenuhi**, sudah masuk commit `50ccf615` |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 1, kontrak API 1, database 1, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `50ccf615` |
| Tanggal | 2026-09-10 |
| Status | **Selesai.** Build lulus 0 error; 39 uji radiologi lulus, 12 di antaranya baru. Aturan keselamatan kini dapat dikelola lewat aplikasi, tanpa menyentuh database langsung |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Master Data |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` sejak 2026-09-10 |
| Keberlakuan | `NEW CODE` untuk `RadSafetyRuleController` dan seluruh DTO barunya; `TOUCHED LEGACY` untuk `RadSafetyPolicyService` yang diperluas |
| QBE ID yang berlaku | `QBE-MOD-001` capability tinggal di modul pemiliknya; `QBE-SVC-001` orkestrasi dan akses data dimiliki service, controller tidak menyentuh `ApplicationDbContext`; `QBE-API-001` memakai boundary, envelope, dan status yang sudah mapan; `QBE-DTO-001` entity EF tidak diekspos sebagai kontrak; `QBE-PERM-001` memakai metadata Access yang berlaku; `QBE-PAGE-001` daftar memakai paging, search, dan sort yang sudah mapan; `QBE-VAL-001` validasi tetap di backend |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — tidak ada entity, configuration, penamaan fisik, maupun migration; `QBE-CODE-*` — modul ini tidak mengalokasikan nomor bisnis |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

`RAD-FACT-009` mencatat keadaan modul apa adanya: **tidak satu pun pemeriksaan radiologi dapat
dijalankan.** Gerbang keselamatan menolak setiap acquisition selama aturannya belum ada, dan
sampai kemarin tidak ada satu pun cara memasukkan aturan itu selain menulis langsung ke
database.

`BE-RAD-02` sudah menyiapkan aturan mainnya — siapa menyusun, siapa mengesahkan, dan apa yang
terjadi bila keduanya orang yang sama. Yang belum ada adalah **pintunya**. Aturan bisnis yang
tidak dapat dipanggil siapa pun sama saja dengan belum ada.

> **Contoh nyata.** Rumah sakit hendak memberlakukan skrining kehamilan wajib untuk CT-Scan.
> Sebelum task ini, satu-satunya jalan adalah meminta seorang programmer menjalankan perintah
> `INSERT` ke tabel — tanpa pengesahan penanggung jawab klinis, tanpa jejak siapa memutuskan,
> dan tanpa kemungkinan diperiksa auditor. Sesudah task ini, admin Radiologi menyusunnya di
> layar, penanggung jawab klinis menekan Sahkan, dan seluruh jejaknya tersimpan.

Satu endpoint pada task ini menjawab kekhawatiran yang berbeda dan sama pentingnya:
`GET /coverage` menjawab pertanyaan **"alat mana yang hari ini akan menolak semua
pemeriksaannya?"** — pertanyaan yang selama ini hanya terjawab ketika pasien sudah berdiri di
depan alat.

---

## 2. Proses bisnis

**Tujuan.** Memberi rumah sakit cara mengelola aturan keselamatan radiologinya sendiri, dengan
pengesahan berjenjang dan jejak yang lengkap.

**Pelaku dan kewenangannya.**

| Pelaku | Yang boleh dilakukan | Hak akses |
| --- | --- | --- |
| Admin Radiologi | Melihat, menyusun draf, mengubah draf, mengajukan pengesahan | `RadSafetyRule : Read`, `Create`, `Update`, `Submit` |
| Penanggung jawab klinis | Mengesahkan, menolak, menghentikan | `RadSafetyRule : Approve`, `Reject`, `Deactivate` |

`RAD-DEC-015` menetapkan `RadSafetyRule : Approve` sekaligus menjadi **penanda peran**
penanggung jawab klinis. Tidak ada tabel peran tersendiri; siapa yang memegang hak akses itulah
yang dihitung berwenang.

**Langkah berurutan pada layar.**

1. Layar dibuka. Frontend memanggil `GET /filters/metadata` untuk tahu pilihan penyaring,
   pengurutan, dan **aksi apa saja yang sah beserta status asalnya** — sehingga layar tidak
   menebak sendiri kapan tombol Sahkan boleh muncul.
2. `GET /summary` mengisi kartu statistik: berapa draf, berapa menunggu pengesahan, berapa
   berlaku, dan **berapa alat yang belum tercakup**.
3. `GET /` mengisi tabel utama, dengan pencarian, penyaringan, pengurutan, dan halaman.
4. Admin menekan Tambah, mengisi form, lalu `POST /` menyimpannya sebagai draf.
5. Admin menekan Ajukan, `POST /{id}/submit` memindahkannya ke menunggu pengesahan.
6. Penanggung jawab klinis membuka daftar yang disaring `ruleStatus=PendingApproval`, lalu
   menekan Sahkan (`POST /{id}/approve`) atau Tolak (`POST /{id}/reject` beserta alasannya).
7. Ketika aturan perlu diganti, `POST /{id}/deactivate` menghentikan yang lama lebih dulu,
   baru penggantinya disahkan.

**Jalur tidak normal, dan kode yang dikembalikan.**

| Keadaan | Kode | Pesan bagi pengguna |
| --- | :---: | --- |
| Penyusun atau pengaju mencoba mengesahkan aturannya sendiri | `403` | "Aturan yang Anda susun atau ajukan sendiri harus disahkan penanggung jawab klinis lain." |
| Mengubah aturan yang sedang berlaku | `403` | "Aturan yang sedang berlaku tidak dapat diubah. Susun draf baru lalu ajukan pengesahan." |
| Menolak tanpa mengisi alasan | `400` | "Alasan penolakan wajib diisi." |
| Mengesahkan aturan kedua untuk kombinasi alat, pemeriksaan, dan butir yang sama | `409` | "Sudah ada aturan aktif ... Nonaktifkan aturan lama lebih dulu." |
| Mengesahkan draf yang belum diajukan | `409` | "Hanya aturan yang sedang diajukan yang dapat disahkan..." |
| Menyusun aturan untuk alat yang sudah dipensiunkan | `422` | "Alat pencitraan yang dipilih sudah tidak aktif." |
| Aturan tidak ditemukan | `404` | "Aturan keselamatan tidak ditemukan." |

**Hasil akhir.** `RAD-CAP-003` berpindah dari `Repair` menjadi lengkap di sisi backend. Yang
tersisa untuk dapat dipakai orang adalah layarnya (`FE-RAD-04`) dan pengisian data awal
(`BE-RAD-15`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola dan standar.** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`;
`rules/backend/TASK_CLASSIFICATION.md`; `rules/backend/REVIEW_RULES.md`;
`rules/backend/REPORT_TEMPLATE.md`; **`rules/backend/master-data-endpoint-standard.md`**;
**`rules/backend/transaction-endpoint-standard.md`**; `rules/rule-output/`.

**Kontrak.** `contracts/api-contract.md` bagian 2 dan 4; `contracts/permission-audit-matrix.md`
bagian 4, 5, dan 6; `contracts/state-transition-matrix.md` bagian 5;
`contracts/validation-matrix.md` bagian 2; `roadmap/backend-roadmap.md`;
`roadmap/requirement-traceability.md`; laporan `BE-RAD-01.md`, `BE-RAD-02.md`, `BE-RAD-06.md`.

**Source pembanding.** `RadOrderController.cs` dan `RadStudyController.cs` untuk pola controller
radiologi; `MasterData/Controllers/ServiceUnitController.cs` sebagai rujukan bentuk kontrak
master data pada standar; `MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs`
dan `DTOs/MedicalRecordFilterAndSummaryDtos.cs` sebagai rujukan `filters/metadata` dan
`summary`; `Responses/PagedResult.cs`; `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Controllers/RadSafetyRuleController.cs` | **Baru.** Sepuluh endpoint beserta `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, `[Tags]`, dan `[ProducesResponseType]` per status yang benar-benar dikembalikan |
| `Areas/.../Services/RadSafetyPolicyService.cs` | Empat metode baca baru: metadata penyaring, rekap, daftar berhalaman, dan cakupan alat |
| `Areas/.../DTOs/RadiologyFilterAndSummaryDtos.cs` | DTO metadata, rekap, dan cakupan untuk aturan keselamatan |
| `Areas/.../DTOs/RadiologyDtos.cs` | `RadSafetyRulePagedQuery` — penyaring daftar |
| `Tests/.../RadiologyManagement/RadSafetyRuleCatalogTests.cs` | **Baru.** 12 uji tanpa database |
| `docs/.../roadmap/backend-roadmap.md` | Baris keadaan `BE-RAD-03` |
| `docs/.../roadmap/requirement-traceability.md` | Bukti pada `RAD-DEC-005`, `RAD-CAP-003`, slice `S4`, `AC-17`, dan Definition of Done modul |

### 3.3 Keputusan rancangan yang perlu diketahui

**`GET /coverage` hanya memuat alat yang bermasalah.** Daftar ini tidak menampilkan seluruh alat
beserta penandanya, melainkan hanya alat yang belum punya aturan berlaku. Alasannya diambil dari
acceptance test `BE-RAD-15`: "GET /coverage mengembalikan daftar kosong — tidak ada alat tanpa
aturan aktif". **Daftar kosong berarti modul siap dipakai**, dan itu bentuk jawaban yang paling
sulit disalahpahami.

**Penyaring cakupan sama persis dengan penyaring gerbang.** Keduanya memakai
`RuleStatus = Active` ditambah masa berlaku. Kalau berbeda sedikit saja, layar akan menyatakan
sebuah alat sudah siap sementara gerbangnya tetap menolak — persis kekeliruan yang ditutup
`BE-RAD-06`.

**Paging dan pencarian dikerjakan service, bukan controller.** Controller master data yang sudah
ada mengerjakannya sendiri sambil menyentuh `ApplicationDbContext` langsung. Untuk `NEW CODE`,
`QBE-SVC-001` mengalahkan pola legacy itu, dan `AGENTS.md` menyatakannya eksplisit.

**Tiga endpoint baseline master data sengaja tidak dibuat.** Standarnya menyebut sembilan
endpoint; kontrak menyebut delapan. Selisihnya bukan kelalaian:

| Tidak dibuat | Alasan |
| --- | --- |
| `GET /options` | Aturan keselamatan bukan isi dropdown. Yang dipilih pada form lain adalah alat dan butirnya, bukan aturannya |
| `PATCH /{id}/status` | Status aturan berpindah karena kejadian bernama — diajukan, disahkan, ditolak, dihentikan — bukan karena seseorang menyetel nilai. `transaction-endpoint-standard.md` bagian 3 melarangnya untuk hal ber-lifecycle |
| `DELETE /{id}` | Aturan yang pernah berlaku tidak dihapus. Study lama yang lolos memakainya tetap harus dapat ditelusuri; yang tersedia adalah menghentikan |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Delapan endpoint yang direncanakan `RAD-API-001` kini tersedia**, ditambah dua endpoint baseline `filters/metadata` dan `summary`. Tidak ada endpoint lama yang diubah, diganti nama, atau dihapus. Label "Rencana (belum tersedia)" pada kontrak kini basi |
| Database | Tidak ada perubahan schema, entity, index, maupun migration. **Tidak ada satu pun perintah database yang dijalankan.** Seluruh kolom sudah tersedia sejak `BE-RAD-01` |
| Keamanan/Auth | Setiap endpoint memuat `[AccessPermission(...)]` dengan string **persis** seperti `RAD-PERM-001` bagian 4. Pemisahan wewenang menyusun dan mengesahkan tetap ditegakkan service sejak `BE-RAD-02`; task ini memasang penanda kewenangan di endpointnya |

---

## 4. Dokumentasi endpoint

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-rules`

#### Health Services / Radiology Management / Master Data / Rad Safety Rule

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring, pengurutan, dan daftar aksi yang sah beserta status asalnya | `RadSafetyRule : Read` |
| `GET` | `/summary` | Rekap jumlah aturan per keadaan, ditambah jumlah alat yang belum tercakup | `RadSafetyRule : Read` |
| `GET` | `/` | Daftar aturan dengan pencarian, penyaringan, pengurutan, dan halaman | `RadSafetyRule : Read` |
| `GET` | `/coverage` | **Alat yang belum punya aturan berlaku.** Daftar kosong berarti seluruh alat siap dipakai | `RadSafetyRule : Read` |
| `GET` | `/{id}` | Rincian satu aturan; dipakai form ubah | `RadSafetyRule : Read` |
| `POST` | `/` | Menyusun draf aturan | `RadSafetyRule : Create` |
| `PUT` | `/{id}` | Mengubah draf aturan | `RadSafetyRule : Update` |
| `POST` | `/{id}/submit` | Mengajukan aturan untuk disahkan | `RadSafetyRule : Submit` |
| `POST` | `/{id}/approve` | Mengesahkan aturan; nomor versi naik satu | `RadSafetyRule : Approve` |
| `POST` | `/{id}/reject` | Menolak pengajuan, disertai alasan wajib | `RadSafetyRule : Reject` |
| `POST` | `/{id}/deactivate` | Menghentikan aturan yang sedang berlaku | `RadSafetyRule : Deactivate` |

Contoh pemanggilan daftar:

```http
GET /api/v1/health-services/radiology-management/master-data/rad-safety-rules
    ?search=CT&ruleStatus=3&isMandatory=true
    &sortBy=effectiveFrom&sortDirection=desc&pageNumber=1&pageSize=25
```

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan** atas permintaan pemilik modul, supaya
keduanya tidak berebut memori pada waktu yang sama.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -m:1` — percobaan pertama | Gagal `EXIT=1` tanpa satu pun baris error | `EXISTING / ENVIRONMENT ISSUE` | Sisa berkas `obj` terkunci dari proses uji yang dihentikan di tengah kompilasi pada sesi sebelumnya |
| `dotnet build-server shutdown` lalu build ulang | Berhasil, `EXIT=0`, **0 error** | `PASS` | Keluaran perintah |
| Pemeriksaan memori saat gagal | 24,9 GB bebas dari 32 GB; tidak ada proses `dotnet`, `MSBuild`, atau `VBCSCompiler` yang tertinggal | `PASS` | Kegagalan build **bukan** karena kehabisan memori |
| Build akhir dengan `-p:RunAnalyzers=false -p:EnforceCodeStyleInBuild=false` | Berhasil, **0 error**, 197 warning, **2 menit 5 detik** | `PASS` | Mematikan analyzer memangkas waktu build dari 8–11 menit menjadi 2 menit |
| `dotnet test --no-build --filter ...RadiologyManagement` | **42 lulus, 0 gagal**, 12 detik | `PASS` | Keluaran perintah |
| Rincian satu aturan membawa nama alat dan butir | Terisi untuk form ubah | `PASS` | `Rincian_MembawaNamaAlatDanButirUntukFormUbah` |
| Rincian aturan yang tidak ada | Ditolak `404` | `PASS` | `Rincian_AturanYangTidakAda_Ditolak404` |
| Rincian aturan yang sudah dihapus | Diperlakukan tidak ada, bukan dikembalikan berpenanda | `PASS` | `Rincian_AturanYangSudahDihapus_DiperlakukanTidakAda` |
| Cakupan kosong ketika seluruh alat sudah punya aturan berlaku | Daftar kosong | `PASS` | `Cakupan_KosongKetikaSeluruhAlatSudahPunyaAturanBerlaku` |
| Alat yang aturannya masih draf tetap terhitung belum siap | Muncul di daftar, `DraftOrPendingRuleCount` bernilai 1 | `PASS` | `Cakupan_MemuatAlatYangAturannyaMasihDraf` |
| Alat yang belum disentuh sama sekali | Muncul dengan pencacah bernilai 0 | `PASS` | `Cakupan_MemuatAlatYangBelumDisentuhSamaSekali` |
| Alat yang sudah dipensiunkan | **Tidak** muncul | `PASS` | `Cakupan_TidakMemuatAlatYangSudahDipensiunkan` |
| Alat muncul kembali setelah aturannya dihentikan | Kosong sebelum dihentikan, satu baris sesudahnya | `PASS` | `Cakupan_MemuatKembaliAlatSetelahAturannyaDihentikan` |
| Daftar disaring menurut keadaan pengesahan | Berlaku 1, draf 1 | `PASS` | `Daftar_MenyaringMenurutKeadaanPengesahan` |
| Pencarian pada kode alat dan kode butir | Menemukan sesuai kata kuncinya | `PASS` | `Daftar_MencariPadaKodeAlatDanKodeButir` |
| Permintaan halaman yang tidak masuk akal | `pageNumber` 0 menjadi 1; `pageSize` 0 menjadi 25; `pageSize` 500 dibatasi 100 | `PASS` | `Daftar_MenormalkanPermintaanHalamanYangTidakMasukAkal` |
| Jumlah halaman dihitung backend | `TotalData` 3, `TotalPage` 1 | `PASS` | Uji yang sama |
| Daftar membawa nama alat dan butir | Terisi untuk ditampilkan layar | `PASS` | `Daftar_MembawaNamaAlatDanButirUntukDitampilkan` |
| Rekap per keadaan dan alat belum tercakup | Total 2, berlaku 1, draf 1, alat belum tercakup 1 | `PASS` | `Rekap_MenghitungPerKeadaanDanAlatYangBelumTercakup` |
| Metadata menyebut seluruh perpindahan status yang sah | Empat aksi berurutan, asal dan tujuannya benar | `PASS` | `Metadata_MenyebutSeluruhPerpindahanStatusYangSah` |
| Metadata tidak menjanjikan penyaring yang tidak diproses | Setiap parameter yang disebut memang dilayani query | `PASS` | `Metadata_TidakMenjanjikanPenyaringYangTidakDiproses` |
| Regresi 27 uji radiologi yang sudah ada | Seluruhnya tetap lulus tanpa diubah | `PASS` | `RadSafetyPolicyServiceTests` dan `RadiologyFilterAndSummaryTests` |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan seluruh warning build |

Uji manual: `NOT FEASIBLE` — layar `FE-RAD-04` belum dibuat.

### Batas verifikasi yang perlu diketahui

Seluruh uji berjalan di atas penyedia **in-memory**, bukan PostgreSQL. Yang **belum** terbukti:

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Terjemahan SQL untuk pencarian `Contains` dan penyaring cakupan | Butuh database sungguhan; polanya sudah dipakai modul lain di repository ini |
| Index unik sebagai penjaga terakhir tabrakan pengesahan | Penyedia in-memory tidak menegakkan index |
| String `[AccessPermission]` benar-benar cocok dengan baris izin di database | `BE-RAD-14` yang merencanakan uji kontrak hak akses |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Migration ke `QuilvianNewDevYoga` | **Tidak ada yang perlu dimigrasikan.** `BE-RAD-02` dan `BE-RAD-03` tidak menyentuh entity, configuration, maupun snapshot; satu-satunya migration radiologi sudah diterapkan pada 2026-09-10 |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi; mengisinya wewenang terpisah |
| Menjalankan aplikasi untuk memeriksa Swagger | Butuh runtime; tidak diminta task ini |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-17 — alat tanpa aturan aktif dapat diketahui di depan | **Terpenuhi di backend** | `GET /coverage` beserta lima uji cakupan. Peringatan di layar menunggu `FE-RAD-04` |
| `GET /coverage` menampilkan alat tanpa aturan aktif | **Terpenuhi** | Lima uji, termasuk alat yang aturannya baru dihentikan |
| String `[AccessPermission]` persis seperti kontrak | **Terpenuhi** | Sepuluh endpoint dibandingkan baris demi baris dengan `RAD-PERM-001` bagian 4 |
| Delapan endpoint tersedia sesuai kontrak | **Terpenuhi, ditambah dua** | Delapan dari `RAD-API-001`, ditambah `filters/metadata` dan `summary` yang diwajibkan standar endpoint |
| Kode status sesuai `RAD-API-001` bagian 4 | **Terpenuhi** | `400`, `403`, `404`, `409`, `422` dipetakan sesuai artinya; `403` dipisahkan dari `400` karena tidak ada perbaikan isian yang menolong |

Tidak ada butir Definition of Done `BE-RAD-03` yang belum terpenuhi.

---

## 7. Pekerjaan tambahan di luar `BE-RAD-03`

Dua permintaan pemilik modul dikerjakan bersamaan pada rentang perubahan yang sama. Keduanya
**bukan** bagian `BE-RAD-03` dan tidak punya task ID, sehingga dicatat di sini agar tidak
hilang jejaknya.

### 7.1 Deskripsi endpoint dihapus dari Swagger

Pemilik modul menunjukkan teks yang muncul di baris endpoint Swagger dan memintanya dihapus.
Sumbernya XML `/// <summary>` pada action; Swashbuckle merendernya sebagai judul operasi.

Tiga tempat diubah menjadi komentar biasa `//`, sehingga penjelasan dan traceability-nya tetap
ada di kode tetapi tidak ikut ke Swagger: `RadOrderController.GetByEpisode`,
`RadStudyController.GetModalities`, dan `RadStudyController.DecideQuality`.

**Berlaku seterusnya:** action controller radiologi tidak lagi diberi `<summary>` XML.

### 7.2 `filters/metadata` dan `summary` untuk Rad Order dan Rad Study

| Endpoint | Isi |
| --- | --- |
| `GET /rad-orders/filters/metadata` | Sepuluh status pesanan berlabel Bahasa Indonesia, pilihan pengurutan, daftar parameter query |
| `GET /rad-orders/summary` | Jumlah per status, ditambah `BelumDikerjakan` |
| `GET /rad-studies/filters/metadata` | Status study, keadaan butir keselamatan, empat keadaan aturan |
| `GET /rad-studies/summary` | Jumlah per status, `MenungguGerbangKeselamatan`, dan `AlatTanpaAturanKeselamatanAktif` |

Dua keputusan yang perlu diketahui:

- **`PageSizeOptions` sengaja tidak dikirim** untuk kedua grup itu. Daftar pesanan dan study
  belum memakai `PagedResult`; standar melarang metadata menjanjikan penyaring yang tidak
  diproses daftar. Perpindahan ke `PagedResult` adalah perubahan yang merusak konsumen dan
  layak menjadi task tersendiri.
- **`sortBy` dan `sortDirection` dibuat benar-benar bekerja** pada `GET /rad-orders` dan
  `GET /rad-studies/by-order/{id}`, bersifat opsional, dan urutan bawaannya tidak berubah —
  dibuktikan uji `UrutanBawaanTidakBerubah`.

Sepuluh uji pada `RadiologyFilterAndSummaryTests` membuktikan keduanya.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi. Warning yang muncul seluruhnya sudah ada sebelumnya pada modul lain |
| Masalah yang diketahui | `NONE` yang tersisa dari task ini. Tiga temuan yang semula memerlukan keputusan sudah ditutup pemilik modul pada hari yang sama — lihat bagian 9 |
| Risiko tersisa | **Pertama**, penerjemahan query ke PostgreSQL belum diuji. **Kedua**, string hak akses belum dibuktikan cocok dengan baris izin di database — menunggu `BE-RAD-14`. **Ketiga**, modul tetap **belum dapat dipakai** sampai `BE-RAD-15` mengisi data master awal; sampai saat itu `GET /coverage` akan mengembalikan seluruh alat yang aktif |
| Temuan yang perlu keputusan pemilik modul | Tiga temuan **sudah diputuskan dan diterapkan** — lihat bagian 9. Satu selisih lama masih terbuka dan **bukan** milik task ini: `RAD-VAL-001` bagian 4 menyebut kode `409` untuk "aturan keselamatan belum ada" sementara source mengembalikan `422` |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi sebelumnya berakhir saat build berjalan, dan proses uji dihentikan di tengah kompilasi. Akibatnya berkas `obj` tertinggal dalam keadaan terkunci sehingga build berikutnya gagal tanpa pesan. Dipulihkan dengan `dotnet build-server shutdown`, lalu build dan test dijalankan terpisah |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-04` dan `BE-RAD-05` — pengelolaan alat pencitraan dan butir keselamatan, keduanya tanpa dependency. Sesudahnya `BE-RAD-15` mengisi data master awal sampai `GET /coverage` mengembalikan daftar kosong |

### Status Git pada akhir pekerjaan

Berkas yang berubah karena `BE-RAD-03` dan pekerjaan tambahan pada bagian 7:

```text
 M Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs
 M Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs
 M Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadSafetyPolicyService.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadStudyService.cs
 M docs/module-blueprints/radiologi/roadmap/backend-roadmap.md
 M docs/module-blueprints/radiologi/roadmap/requirement-traceability.md
?? Areas/HealthServices/RadiologyManagement/Controllers/RadSafetyRuleController.cs
?? Areas/HealthServices/RadiologyManagement/DTOs/RadiologyFilterAndSummaryDtos.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadSafetyRuleCatalogTests.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadiologyFilterAndSummaryTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-03.md
```

---

## 9. Tiga keputusan pemilik modul, 2026-09-10

Tiga hal yang semula dilaporkan sebagai temuan terbuka disetujui pemilik modul pada hari yang
sama, dengan pilihan diserahkan kepada rekomendasi terbaik. Ketiganya sudah diterapkan.

### 9.1 Penanda `Deactivate` tetap terpisah, tetapi pemberiannya dikunci

**Selisihnya.** `RAD-STATE-001` bagian 5 menyebut penonaktifan aturan sebagai wewenang
penanggung jawab klinis. `RAD-PERM-001` bagian 4 memberi endpointnya penanda
`RadSafetyRule : Deactivate` — penanda yang berbeda dari `Approve`, yang menurut `RAD-DEC-015`
justru merupakan penanda peran penanggung jawab klinis.

**Yang dipilih.** Penandanya **tetap terpisah**, tetapi `RAD-PERM-001` bagian 5.3 yang baru
mengunci pemberiannya: `Deactivate` hanya boleh diberikan kepada peran yang juga memegang
`Approve`.

**Mengapa begitu, bukan digabung.** Menghentikan sebuah aturan berarti menghapus pertanyaan yang
berdiri antara sebuah permintaan dan penyinaran seorang pasien — bahayanya setara dengan
memberlakukan aturan baru, sehingga wewenangnya wajib setara pula. Yang tidak setara adalah
kebutuhan Administrator: dengan penanda terpisah, kedua kewenangan itu tetap dapat dilihat dan
dicabut satu per satu pada layar peran, dan jejak pemberiannya terbaca apa adanya. Yang dikunci
adalah **pemberiannya**, bukan penamaannya.

> **Contoh kalau tidak dikunci.** Administrator memberi seorang staf administrasi hak
> `Deactivate` saja, dengan maksud "sekadar merapikan aturan usang". Staf itu menghentikan
> aturan skrining kehamilan untuk CT-Scan karena dianggap memperlambat antrian. Sejak saat itu
> CT-Scan berjalan tanpa pertanyaan tersebut — tanpa satu pun penanggung jawab klinis pernah
> menyetujuinya.

**Dampaknya pada kode.** Tidak ada. `[AccessPermission("RadSafetyRule", "Deactivate")]` pada
endpoint tetap seperti semula. Yang bertambah adalah butir keempat pada bagian 9 `RAD-PERM-001`:
`BE-RAD-14` wajib memuat uji yang **gagal** ketika ada peran memegang `Deactivate` tanpa
`Approve`. Sampai uji itu ada, penjagaannya bersandar pada disiplin penyusunan peran — dan itu
disebut apa adanya di kontraknya.

### 9.2 `GET /{id}` ditambahkan

**Yang dipilih.** Endpoint rincian satu aturan ditambahkan ke kontrak dan diimplementasikan.

**Mengapa.** Form ubah memerlukan pembacaan satu baris. Memakai `GET /` berfilter untuk keperluan
itu boros — ia menarik seluruh kerangka halaman beserta pencacahnya hanya untuk mengambil satu
baris — dan janggal dibaca. Standar endpoint master data bagian 2.5 juga mewajibkannya.
Penambahannya bersifat additive: tidak ada endpoint lain yang berubah, dan tidak ada konsumen
yang perlu menyesuaikan diri.

Aturan yang sudah ditandai terhapus diperlakukan **tidak ada**, bukan dikembalikan dengan
penanda. Layar tidak perlu tahu bedanya, dan menyembunyikannya menutup satu jalan kebocoran
data yang tidak diperlukan.

### 9.3 Label kontrak diperbarui

`RAD-API-001` naik ke revision 3. Sebelas endpoint grup *Master Data / Rad Safety Rule*
berstatus `Tersedia`, tiga di antaranya baru — `filters/metadata`, `summary`, dan `{id}`.
Ditambahkan pula satu bagian yang menyatakan **tiga endpoint baseline master data sengaja tidak
dibuat** beserta alasannya, supaya pembaca berikutnya tidak menganggapnya kelalaian.

### Ringkasan berkas kontrak yang berubah

| Berkas | Perubahan |
| --- | --- |
| `contracts/api-contract.md` | Revision 2 ke 3; sebelas endpoint `Tersedia`; catatan amandemen; bagian endpoint yang sengaja tidak dibuat |
| `contracts/permission-audit-matrix.md` | Revision 2 ke 3; tiga baris endpoint baca baru pada bagian 4; bagian 5.3 baru; butir keempat pada bagian 9 |

---

### Perubahan yang bukan milik task ini

Empat berkas Laboratorium berikut berubah di worktree dan **bukan** hasil pekerjaan ini.
Keduanya sengaja dibiarkan utuh dan tidak disentuh sama sekali:

```text
 M Areas/HealthServices/LaboratoryManagement/Controllers/LabPatientRegistrationController.cs
 M Areas/HealthServices/LaboratoryManagement/DTOs/LabPatientRegistrationDtos.cs
 M Areas/HealthServices/LaboratoryManagement/Services/LabPatientRegistrationService.cs
 M Tests/QuilvianSystemBackend.Tests/HealthServices/LaboratoryManagement/LabPatientRegistrationTests.cs
```

Tidak ada `git add`, commit, maupun push yang dilakukan.
