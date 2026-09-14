# Laporan Perubahan Backend — `BE-RAD-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-09` |
| Judul | Endpoint hasil bacaan |
| Slice | `S9` — Hasil bacaan radiolog |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 4, gelombang `MVP-2` |
| Trace | `FR-RAD-010` s/d `FR-RAD-015`; `RAD-DEC-003`, `RAD-DEC-006`, `RAD-DEC-015`; `RAD-API-001` grup *Rad Report* dan bagian 4; `RAD-PERM-001` bagian 3, 6, dan 7; `RAD-VAL-001` bagian 1; `RAD-STATE-001` bagian 3 |
| Contract version | `RAD-API-001` rev 5 dan `RAD-PERM-001` rev 5 saat dikerjakan — keduanya `approved`. Diamandemen menjadi **rev 6** oleh task ini, **menunggu konfirmasi pemilik modul** |
| Dependency | `BE-RAD-08` — **selesai** untuk source dan uji |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa **2**, berkas diubah 1, logika bisnis 1, kontrak API **2**, database 1, keamanan/auth **2**, workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `bac46079` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** Sepuluh endpoint berjalan. Build lulus 0 error; 203 uji radiologi lulus, 41 di antaranya baru; seluruh 1.408 uji in-memory lulus. **Penghalang `BE-RAD-08` masih terbuka**: penanda `RadReport : ActAsRadiologist` belum dapat diberikan kepada peran mana pun — bagian 7.1 |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-API-001` memakai boundary API, response, status, dan validasi yang sudah mapan; `QBE-PERM-001` memakai metadata Access yang berlaku; `QBE-SVC-001` controller tidak mengakses context secara langsung; `QBE-DTO-001` entity EF tidak menjadi kontrak API; `QBE-PAGE-001` capability list memakai paging/search/sort yang sudah mapan; `QBE-OPT-001` metadata hanya disediakan bila dikonsumsi; `QBE-VAL-001`; `QBE-LOG-001`; `QBE-AUD-001`; `QBE-NAM-002`; `QBE-MOD-001` |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-DB-001`, `QBE-DB-002` — tidak ada entity, configuration, maupun migration; `QBE-CODE-001` s/d `QBE-CODE-006` — penomoran sudah selesai di `BE-RAD-08`; `QBE-DEL-001` — tidak ada endpoint hapus, dan memang tidak boleh ada |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Setelah `BE-RAD-08`, seluruh aturan penulisan dan pengesahan bacaan sudah berjalan di dalam
`RadReportService` — **tetapi tidak ada satu pun pintu untuk memanggilnya.** Tidak ada layar,
tidak ada integrasi, dan tidak ada cara menguji aturannya sebagaimana ia akan benar-benar
dipakai.

Ini bukan sekadar kekurangan kenyamanan. **Sebuah aturan dapat berjalan sempurna di service lalu
hilang di controller**, dan hilangnya tidak menghasilkan galat yang terlihat:

> **Contoh nyata cara aturan menghilang.** Sebuah controller menangkap hasil penolakan lalu
> mengembalikan `200` dengan pesan keluhan di dalamnya. Service tetap menolak, database tetap
> bersih — tetapi layar membaca `200`, menganggapnya berhasil, lalu menampilkan "Bacaan
> disahkan". Radiolog pergi. Bacaannya tidak pernah sah.

Karena itu task ini tidak berhenti pada "endpointnya ada": kode status tiap penolakan dibuktikan
lewat jalur HTTP.

---

## 2. Proses bisnis

**Tujuan.** Membuka jalan bagi layar dan modul lain untuk membaca, menulis, mengesahkan, dan
merilis hasil bacaan — tanpa satu pun aturan keselamatan berubah artinya di perjalanan.

### 2.1 Alur normal lewat endpoint, berurutan

| Langkah | Pelaku | Endpoint | Hasil |
| ---: | --- | --- | --- |
| 1 | Radiolog membuka daftar kerja | `GET /?belumDirilis=true` | Daftar bacaan yang hasilnya masih ditunggu |
| 2 | Radiolog membuka satu bacaan | `GET /{id}` | Isi versi berlaku beserta `AvailableActions` |
| 3 | Residen menulis draf | `POST /by-study/{radStudyId}/draft` | `200`, status menjadi `Drafted`, perannya dibekukan |
| 4 | Residen memperbaiki drafnya | `PUT /{id}/draft` | `200`, tidak melahirkan versi baru |
| 5 | Dokter radiolog mengesahkan | `POST /{id}/validate` | `200`, status menjadi `Validated` |
| 6 | Dokter radiolog merilis | `POST /{id}/release` | `200`, status menjadi `Released`, isinya beku |
| 7 | Rekam medis membaca | `GET /by-encounter/{encounterId}` | Seluruh bacaan kunjungan itu |

### 2.2 Jalur tidak normal dan kode yang dijawab

Seluruh baris di bawah dibuktikan uji lewat jalur HTTP, bukan disimpulkan dari service.

| Percobaan | Jawaban sistem | Kode |
| --- | --- | --- |
| Membuka bacaan yang tidak ada | "Bacaan yang dimaksud tidak ditemukan." | `404` |
| Membuka bacaan atas pemeriksaan yang belum dibaca siapa pun | "Pemeriksaan ini belum memiliki bacaan." | `404` |
| Membuka bacaan pada kunjungan yang belum punya bacaan | "Kunjungan ini belum memiliki hasil bacaan radiologi." | **`200`, daftar kosong** |
| Menyimpan draf tanpa kesimpulan | "Kesimpulan bacaan wajib diisi." | `400` |
| Menulis bacaan atas citra yang dinyatakan tidak layak | "Citra pemeriksaan ini dinyatakan tidak layak dibaca…" | `422` |
| Menulis bacaan atas citra yang mutunya belum dinilai | "Mutu citra belum dinilai…" | `422` |
| Menulis bacaan kedua atas pemeriksaan yang sama | "Pemeriksaan ini sudah memiliki bacaan. Gunakan koreksi…" | `409` |
| Mengaku dokter radiolog tanpa memegang penandanya | "Anda belum terdaftar sebagai dokter radiolog…" | `403` |
| Mengubah draf orang lain | "Hanya penulis draf yang dapat mengubahnya sebelum disahkan." | `403` |
| **Residen mengesahkan drafnya sendiri** | "Draf yang Anda tulis harus disahkan dokter radiolog." | `403` |
| Bukan-radiolog mengesahkan draf orang lain | "Hanya dokter radiolog yang boleh mengesahkan hasil bacaan." | `403` |
| Mengesahkan bacaan yang sudah disahkan | "Hanya draf yang belum disahkan yang dapat disahkan…" | `409` |
| Merilis bacaan yang belum disahkan | "Bacaan harus disahkan lebih dulu sebelum dirilis." | `409` |

**Mengapa kunjungan tanpa bacaan dijawab `200`, bukan `404`.** Kunjungan yang belum punya bacaan
adalah keadaan yang sepenuhnya wajar — pemeriksaannya mungkin baru dikerjakan pagi ini. Menjawab
`404` menyamakannya dengan kunjungan yang **tidak ada**, dan pembaca rekam medis kehilangan cara
membedakan "belum ada hasilnya" dari "saya salah membuka pasien".

### 2.3 Daftar aksi: bantuan tampilan, bukan pengaman

`GET /{id}` mengembalikan `AvailableActions`, misalnya `["UpdateDraft", "Validate"]`, supaya layar
tidak menebak tombol dari nilai status.

> **Yang wajib dipahami.** Daftar itu diturunkan dari **keadaan bacaan saja**. Ia tidak tahu siapa
> yang sedang melihat. Seorang residen tetap akan melihat tombol Sahkan pada draf yang ia tulis
> sendiri — dan permintaannya tetap ditolak `403` di backend.
>
> Ini disengaja. Menambahkan pemeriksaan kewenangan di daftar aksi berarti aturan `RAD-DEC-003`
> tertulis di dua tempat yang harus sama-sama benar selamanya, dan salah satu diam-diam
> berselisih dari yang lain adalah cara paling umum sebuah aturan keselamatan berhenti berlaku.
> **Menyembunyikan tombol bukan authorization.**

Dibuktikan `DaftarAksiHanyaBantuanTampilanBukanPengaman`.

### 2.4 Daftar tidak membawa isi bacaan

`RadReportListResponse` **tidak punya kolom** `Findings`, `Impression`, maupun `Recommendation`.

Alasannya bukan ukuran muatan. Daftar adalah layar yang paling sering dibiarkan terbuka — papan
kerja unit, monitor bersama, hasil pencarian. Kesimpulan klinis seorang pasien dibuka lewat
`GET /{id}`, ketika seseorang memang bermaksud membacanya. Dibuktikan
`DaftarBacaanTidakMembawaIsiBacaan`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry, `QBE_EXCEPTIONS.json` | Governance canonical dan preflight QBE |
| `rules/backend/transaction-endpoint-standard.md` | Arketipe transaksi, aturan verb, baseline baca, `AvailableActions` |
| `rules/backend/` — `TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE` | Aturan operasional task |
| `contracts/api-contract.md` grup *Rad Report* dan bagian 4 | Daftar endpoint, bentuk response, arti kode status |
| `contracts/permission-audit-matrix.md` bagian 3, 6, 7 | String hak akses per endpoint, penanda peran, larangan log |
| `contracts/validation-matrix.md` bagian 1 | Pesan yang wajib terbaca pengguna |
| `Controllers/RadSafetyRuleController.cs` | Pola terdekat: `Execute` yang memetakan `RadOperationResult` menjadi kode HTTP |
| `Controllers/RadModalityController.cs`, `RadSafetyRequirementController.cs` | Konvensi `[AccessController]`, `[AccessAction]`, `SortOrder`, dan kode sukses `200` |
| `Services/RadSafetyPolicyService.cs` | Pola paging, metadata penyaring, dan ringkasan |
| `Seeders/AccessMenuSeeder.cs`, `Attributes/Access*.cs`, `Filters/AccessPermissionFilter.cs` | Cara pasangan hak akses didaftarkan dan diperiksa |
| `Tests/.../RadiologyRoleAccessContractTests.cs` | Uji kontrak hak akses yang wajib ikut mencakup controller baru |
| `Responses/ApiResponse.cs`, `PagedResult.cs` | Bentuk pembungkus jawaban |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Controllers/RadReportController.cs` | **Baru.** Sepuluh endpoint beserta pemetaan kode status |
| `Areas/.../Services/RadReportService.cs` | Enam method baca ditambahkan: metadata penyaring, ringkasan, daftar berhalaman, rincian, per pemeriksaan, per kunjungan. `AvailableActions` diisi pada rincian |
| `Areas/.../DTOs/RadReportDtos.cs` | Empat DTO baru: `RadReportPagedQuery`, `RadReportListResponse`, `RadReportFilterMetadataResponse`, `RadReportSummaryResponse`. `AvailableActions` ditambahkan pada `RadReportDetailResponse` |
| `Program.cs` | Tidak berubah pada task ini — `RadReportService` sudah terdaftar sejak `BE-RAD-08` |
| `Tests/.../RadReportControllerTests.cs` | **Baru.** 37 uji |
| `Tests/.../RadiologyRoleAccessContractTests.cs` | `RadReportController` dimasukkan ke daftar controller yang diaudit; empat pasangan hak akses `RadReport` ditambahkan sebagai pasangan yang wajib ada |

### 3.3 Tiga keputusan bentuk, beserta alasannya

**Pertama — dua endpoint baca ditambahkan di luar daftar kontrak.** `GET /filters/metadata` dan
`GET /summary` adalah baseline wajib `transaction-endpoint-standard.md` bagian 4. Keduanya
ditambahkan dengan alasan yang sama persis seperti penambahan pada `RAD-API-001` revision 4 dan 5,
dan **tidak memakai satu pun string hak akses baru** — keduanya `RadReport : Read`.

**Kedua — `RadReportValidateRequest` tidak dibuat.** `RAD-API-001` menyebut namanya, tetapi
`RAD-STATE-001` bagian 3 dan `RAD-VAL-001` bagian 1 tidak menetapkan satu pun isian untuk
pengesahan, dan `RAD-ERD-DICT-001` tidak punya kolom yang dapat diisinya. Membuat DTO kosong hanya
untuk memenuhi namanya berarti menjanjikan isian yang tidak akan disimpan ke mana pun.
`POST /{id}/validate` karena itu tidak menerima badan permintaan.

**Ketiga — `POST .../draft` menjawab `200`, bukan `201`.** `RAD-API-001` bagian 4 memberi contoh
`201` untuk "Draf bacaan tersimpan". Yang dikerjakan adalah `200`, karena:

| Alasan | Buktinya |
| --- | --- |
| Seluruh endpoint create modul Radiologi yang sudah berjalan menjawab `200` | `RadModalityController`, `RadSafetyRequirementController`, `RadSafetyRuleController` |
| `ApiResponse<T>.Ok` menuliskan `200` pada badan jawabannya | `Responses/ApiResponse.cs` |

Menjadikan satu endpoint ini `201` akan membuat badan jawaban berkata `200` sementara HTTP-nya
`201`, dan membuat layar Radiologi menangani dua bentuk berbeda untuk perbuatan yang sama.
Selisihnya **dicatat apa adanya** pada amandemen `RAD-API-001` revision 6, bukan didiamkan.

### 3.4 Dua endpoint yang sengaja tidak dibuat

`GET /{id}/versions` dan `POST /{id}/amendments` adalah milik `BE-RAD-10`. Keduanya
**tidak** dibuat, dan ada uji yang memastikannya tidak terbawa diam-diam
(`EndpointAmandemenDanRiwayatVersiBelumDibuat`).

Alasannya bukan kerapian pembagian task. Endpoint amandemen tanpa logika berversi di belakangnya
adalah jalan paling cepat untuk menimpa bacaan yang sudah dirilis — persis yang dilarang
`RJ-BIL-GATE-DEC-004`.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Sepuluh endpoint baru.** `RAD-API-001` diamandemen menjadi revision 6 dan `RAD-PERM-001` menjadi revision 6; keduanya **menunggu konfirmasi pemilik modul**. Tidak ada endpoint yang berubah maupun hilang, sehingga **tidak ada konsumen lama yang rusak** |
| Database | **`NOT APPLICABLE`.** Tidak ada entity, configuration, migration, maupun perubahan snapshot. Migration `AddRadReport` tetap **belum dijalankan** ke database mana pun, sehingga endpoint ini belum pernah diuji terhadap tabel sungguhan |
| Keamanan/Auth | **Sepuluh endpoint diberi `[AccessPermission]`**, seluruhnya sesuai `RAD-PERM-001` bagian 3 dan dibuktikan dua lapis uji. Empat pasangan hak akses baru terdaftar: `RadReport : Create`, `Update`, `Validate`, `Release`. **Penghalang `ActAsRadiologist` masih terbuka** — bagian 7.1. Tidak ada kolom sensitif yang masuk log maupun daftar |

---

## 4. Dokumentasi endpoint

#### Health Services / Radiology Management / Rad Report

Base URL: `api/v1/health-services/radiology-management/rad-reports`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Melihat pilihan penyaring, pengurutan, dan aksi untuk layar bacaan | `RadReport : Read` |
| `GET` | `/summary` | Melihat rekap jumlah bacaan per keadaan, termasuk yang belum sampai ke dokter pengirim | `RadReport : Read` |
| `GET` | `/` | Melihat daftar bacaan dengan penyaringan, pengurutan, dan halaman | `RadReport : Read` |
| `GET` | `/{id}` | Melihat satu bacaan beserta versi berlaku, riwayat versinya, dan aksi yang tersedia | `RadReport : Read` |
| `GET` | `/by-study/{radStudyId}` | Melihat bacaan atas satu pemeriksaan | `RadReport : Read` |
| `GET` | `/by-encounter/{encounterId}` | Melihat seluruh bacaan satu kunjungan — dipakai rekam medis | `RadReport : Read` |
| `POST` | `/by-study/{radStudyId}/draft` | Menulis draf bacaan; peran penulis dibekukan saat itu juga | `RadReport : Create` |
| `PUT` | `/{id}/draft` | Mengubah draf yang belum disahkan, hanya oleh penulisnya | `RadReport : Update` |
| `POST` | `/{id}/validate` | Mengesahkan bacaan; penjagaan pengesahan sendiri ada di service | `RadReport : Validate` |
| `POST` | `/{id}/release` | Merilis bacaan ke dokter pengirim; setelah ini isinya beku | `RadReport : Release` |

**Parameter penyaring `GET /`:** `search`, `radStudyId`, `radOrderId`, `encounterId`,
`reportStatus`, `belumDirilis`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize`. Seluruhnya
diterangkan sendiri oleh `GET /filters/metadata`, sehingga layar tidak perlu menghafalnya.

**Tidak dibuat pada task ini:** `GET /{id}/versions` dan `POST /{id}/amendments` — milik
`BE-RAD-10`.

---

## 5. Verifikasi

Build dan test dijalankan **terpisah dan berurutan**, memakai `-p:RunAnalyzers=False` atas
permintaan pemilik modul.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | Berhasil, **0 error**, 188 warning, 7 menit 54 detik | `PASS` | Keluaran perintah |
| Warning baru dari berkas radiologi | **Tidak ada satu pun** | `PASS` | Penyaringan seluruh warning build atas `RadReport` dan `RadiologyManagement` |
| `dotnet build` project uji in-memory | Berhasil, **0 error**, 10 warning, 2 menit 3 detik (00:02:02) | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadReportControllerTests` | **37 lulus, 0 gagal**, 19 detik | `PASS` | Keluaran perintah |
| `dotnet test --no-build --filter RadiologyManagement` | **203 lulus, 0 gagal**, 14 detik | `PASS` | 162 uji radiologi sebelumnya + 37 uji controller + 4 kasus teori hak akses baru |
| `dotnet test --no-build` seluruh project in-memory | **1.408 lulus, 0 gagal**, 38 detik | `PASS` | 1.367 sebelumnya + 41 baru |

### 5.1 Bukti per butir Definition of Done

**"Endpoint sesuai kontrak"**

| Yang dibuktikan | Uji |
| --- | --- |
| Base URL sama persis dengan `RAD-API-001` | `BaseUrlSesuaiKontrak` |
| Sepuluh endpoint ada dengan kata kerja dan jalur yang benar | `EndpointKontrakTersedia`, 10 kasus |
| Dua endpoint milik `BE-RAD-10` tidak terbawa | `EndpointAmandemenDanRiwayatVersiBelumDibuat` |
| Hak akses tiap endpoint sama persis dengan `RAD-PERM-001` bagian 3 | `HakAksesTiapEndpointSesuaiRadPerm001Bagian3` |
| Tidak ada endpoint radiologi tanpa `[AccessPermission]` dan `[AccessAction]` | `SetiapEndpointRadiologi_PunyaAtributHakAkses`, kini mencakup `RadReportController` |
| Setiap pasangan yang diperiksa filter benar-benar didaftarkan seeder | `SetiapPasanganHakAkses_AdaSebagaiBarisYangDapatDicentang` |

**"Pesan kesalahan memakai bahasa pada `RAD-VAL-001`"** — lima pesan dibandingkan **persis huruf
per huruf** lewat jalur HTTP: kesimpulan wajib, pengubah bukan penulis, pengesahan sendiri,
pengesah bukan radiolog, dan rilis sebelum sah.

**AC-1 sampai AC-4 lewat jalur HTTP** — `AC1_ResidenMengesahkanDrafnyaSendiriDijawab403LewatHttp`,
`AC2_RadiologMengesahkanDrafnyaSendiriDijawab200LewatHttp`,
`AC3_ResidenYangKemudianMenjadiRadiologTetapDijawab403LewatHttp`, ditambah pembuktian bahwa
penolakannya benar-benar menahan — versi tetap `Drafted` dan `ValidatorUserId` tetap kosong.

**Kode status `RAD-API-001` bagian 4** — `200`, `400`, `403`, `404`, `409`, dan `422` seluruhnya
dibuktikan lewat jalur HTTP. `201` **tidak** dipakai; alasannya di bagian 3.3.

### 5.2 Batas verifikasi yang perlu diketahui

| Yang belum terbukti | Sebabnya |
| --- | --- |
| Perilaku endpoint terhadap tabel sungguhan | Migration `AddRadReport` belum dijalankan ke database mana pun |
| Pipeline HTTP sesungguhnya — routing, model binding, filter hak akses | Uji memanggil method controller secara langsung, bukan lewat `WebApplicationFactory`. Yang terbukti adalah pemetaan hasil menjadi kode status dan bentuk permukaannya lewat atribut, **bukan** bahwa ASP.NET benar-benar merutekan permintaan ke method itu |
| `[AccessPermission]` benar-benar menolak pengguna tanpa hak | Filternya berjalan di pipeline yang tidak ikut diuji. Yang terbukti adalah atributnya ada, benar, dan pasangannya terdaftar |
| Penjagaan dua pengesahan yang benar-benar bersamaan | Sama seperti `BE-RAD-08`: penyedia in-memory tidak mendukung transaksi maupun `pg_advisory_xact_lock` |

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| `dotnet ef migrations add` maupun `database update` | Tidak ada perubahan model, dan eksekusi database adalah **wewenang terpisah yang belum diberikan** |
| Uji integrasi Postgres radiologi | `QUILVIAN_BILLING_TEST_DB` sengaja tidak diisi; tabelnya juga belum ada |
| Uji manual lewat Swagger | Menuntut aplikasi berjalan beserta database yang tabelnya belum dibuat. `NOT FEASIBLE` sampai migration dijalankan |
| Analyzer build penuh | Dimatikan atas permintaan pemilik modul untuk memangkas waktu build. Warning compiler tetap dihitung dan disaring |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — residen mengesahkan drafnya sendiri ditolak `403` lewat HTTP | **Terpenuhi** | `AC1_ResidenMengesahkanDrafnyaSendiriDijawab403LewatHttp`, pesan dibandingkan persis |
| AC-2 — radiolog mengesahkan drafnya sendiri diterima lewat HTTP | **Terpenuhi** | `AC2_RadiologMengesahkanDrafnyaSendiriDijawab200LewatHttp` |
| AC-3 — peran dibekukan, tetap ditolak setelah naik jabatan | **Terpenuhi** | `AC3_ResidenYangKemudianMenjadiRadiologTetapDijawab403LewatHttp` |
| AC-4 — penulis dan pengesah terekam terpisah | **Terpenuhi** | Dibuktikan `BE-RAD-08`; lewat HTTP terbaca pada `CurrentVersion` jawaban `POST /{id}/validate` |
| Test — kode status sesuai `RAD-API-001` bagian 4 | **Terpenuhi untuk enam kode yang dipakai** | Tabel bagian 2.2. `201` tidak dipakai; selisihnya di bagian 3.3 |
| Test — `403` dan `422` sesuai matriks validasi | **Terpenuhi** | Enam uji `403` dan dua uji `422`, pesannya dibandingkan |
| DoD — endpoint sesuai kontrak | **Terpenuhi, dengan amandemen** | Tabel bagian 5.1. Dua endpoint baseline ditambahkan dan dicatat sebagai `RAD-API-001` revision 6 |
| DoD — pesan kesalahan memakai bahasa `RAD-VAL-001` | **Terpenuhi** | Lima pesan dibandingkan persis huruf per huruf |

**Butir yang belum terpenuhi, disebut apa adanya:**

1. **Penanda `RadReport : ActAsRadiologist` masih belum dapat diberikan.** Selama itu, seluruh
   endpoint `validate` dan `release` akan menolak semua orang kecuali SuperAdmin. Bagian 7.1.
2. **Amandemen kontrak menunggu konfirmasi.** `RAD-API-001` rev 6 dan `RAD-PERM-001` rev 6
   ditulis pelaksana task, bukan disetujui lebih dulu seperti revision 3 sampai 5.
3. **Belum ada bukti terhadap database sungguhan**, karena migration `AddRadReport` belum
   dijalankan.
4. **Titik pemanggilan kelahiran bacaan masih belum tersambung** — `RadStudyService` belum
   memanggil `EnsurePendingReportAsync`. Hari ini wadah bacaan lahir saat draf pertama ditulis.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas radiologi. Build menghasilkan 188 warning, seluruhnya sudah ada sebelumnya dan berasal dari modul lain |
| Masalah yang diketahui | Penghalang `ActAsRadiologist` di bawah. Selain itu, `RadReport.Version` dan `RadReportVersion.Version` masih belum dideklarasikan `IsConcurrencyToken()` pada configuration — temuan `BE-RAD-08` yang belum diperbaiki karena menuntut migration |
| Risiko tersisa | **Pertama**, tanpa penanda `ActAsRadiologist`, dua endpoint terpenting modul ini menolak semua orang. **Kedua**, migration belum dijalankan, sehingga endpoint belum pernah menyentuh tabel sungguhan. **Ketiga**, pipeline HTTP sesungguhnya belum diuji — lihat batas verifikasi 5.2 |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 7.2 |
| Langkah berikutnya | Selesaikan penghalang `ActAsRadiologist`, lalu `BE-RAD-10` — koreksi berversi |

### 7.1 Penghalang yang masih terbuka, dan apa yang berubah sejak `BE-RAD-08`

**Keadaannya tidak berubah, tetapi perbaikannya menjadi lebih kecil.**

`RAD-PERM-001` bagian 6 menetapkan `RadReport : ActAsRadiologist` **tidak menempel pada satu
endpoint pun**. `AccessMenuSeeder` mendaftarkan pasangan hak akses hanya dari action MVC yang
punya `[AccessController]` dan `[AccessAction]`. Karena itu pasangan ini tidak pernah masuk
`SysActionAccesses`, dan `HasAccessAsync` selalu menjawab `false`.

| Pengguna | Dihitung radiolog? | Yang ia alami pada endpoint baru |
| --- | :---: | --- |
| SuperAdmin | Ya | `POST /{id}/validate` dan `/release` berjalan |
| Siapa pun selain SuperAdmin | **Tidak** | Keduanya dijawab `403` selamanya |

**Yang berubah sejak `BE-RAD-08`:** `RadReportController` kini ada, sehingga seeder **sudah**
membuat baris `SysControllerAccess` untuk `RadReport` beserta empat baris aksinya — `Read`,
`Create`, `Update`, `Validate`, dan `Release`. Yang kurang tinggal **satu baris aksi**:
`ActAsRadiologist`.

**Dua jalan, dan yang pertama lebih saya sarankan:**

| Pilihan | Isinya | Konsekuensinya |
| --- | --- | --- |
| **A — daftarkan aksi tanpa endpoint** *(disarankan)* | Tambahkan satu jalur pendaftaran pada `AccessMenuSeeder` untuk aksi yang dibaca service, lalu daftarkan `RadReport : ActAsRadiologist` lewat jalur itu | **Setia pada `RAD-PERM-001` bagian 6.** Penanda tetap terpisah dari `Validate`, dan perbedaan "boleh mencoba" versus "dihitung sebagai radiolog" tetap utuh. Menyentuh `Seeders/AccessMenuSeeder.cs`, berkas bersama yang dipakai seluruh modul |
| **B — tempelkan pada sebuah endpoint** | Sediakan endpoint yang memakai `[AccessAction("ActAsRadiologist", …)]` | Tidak menyentuh seeder, tetapi **bertentangan dengan `RAD-PERM-001` bagian 6** yang menyatakan penanda ini bukan endpoint, sehingga menuntut amandemen kontrak oleh pemilik modul |

**Mengapa tidak dikerjakan pada task ini.** Pilihan A mengubah cara registri hak akses disusun
untuk **seluruh sistem**, dan "Yang dikerjakan" `BE-RAD-09` hanya menyebut `RadReportController`.
Menyelipkan perubahan sebesar itu dari dalam task endpoint bukan keputusan yang pantas diambil
diam-diam. Ia layak menjadi task kecil tersendiri, atau dikerjakan setelah Anda memberi wewenang
menyentuh seeder.

**Uji kontrak hak akses tidak dapat menangkapnya.** `RadiologyRoleAccessContractTests` memeriksa
pasangan yang dipakai `[AccessPermission]` pada endpoint. `ActAsRadiologist` dibaca service lewat
`HasAccessAsync`, bukan lewat atribut, sehingga tidak pernah masuk pemeriksaannya. Ini sendiri
adalah celah pada uji itu, dan pantas ditutup bersamaan dengan penghalangnya.

### 7.2 Status Git pada akhir pekerjaan

Berkas hasil task ini:

```text
 M docs/module-blueprints/radiologi/contracts/api-contract.md
 M docs/module-blueprints/radiologi/contracts/permission-audit-matrix.md
 M docs/module-blueprints/radiologi/roadmap/backend-roadmap.md
 M docs/module-blueprints/radiologi/roadmap/requirement-traceability.md
?? Areas/HealthServices/RadiologyManagement/Controllers/RadReportController.cs
?? Areas/HealthServices/RadiologyManagement/DTOs/RadReportDtos.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadReportService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadReportControllerTests.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/RadiologyRoleAccessContractTests.cs
?? docs/module-blueprints/radiologi/task/report/backend/BE-RAD-09.md
```

`RadReportDtos.cs`, `RadReportService.cs`, dan `RadiologyRoleAccessContractTests.cs` **disunting**
task ini, tetapi tetap tampil `??` karena ketiganya memang belum pernah di-commit sejak
`BE-RAD-08` dan `BE-RAD-14` — Git belum melacaknya, sehingga tidak ada yang dapat ditandai
berubah.

Berkas lain yang tampak pada `git status` — Laboratorium, laporan `BE-RAD-04`, `BE-RAD-05`,
`BE-RAD-07`, `BE-RAD-08`, `BE-RAD-14`, `BE-RAD-15`, migration `AddRadReport`, serta model dan
configuration hasil `BE-RAD-07` — sudah ada sebelum task ini dimulai dan **bukan** hasil pekerjaan
ini.

Tidak ada `git add`, commit, maupun push yang dilakukan. **Tidak ada perintah database yang
dijalankan.**
