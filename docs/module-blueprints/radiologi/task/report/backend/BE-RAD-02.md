# Laporan Perubahan Backend — `BE-RAD-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RAD-02` |
| Judul | Service siklus pengesahan |
| Slice | `S4` — Pengelolaan aturan keselamatan |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` bagian 2, gelombang `MVP-0` |
| Trace | `RAD-DEC-005`, `RAD-DEC-015`; state matrix `RAD-STATE-001` bagian 5; validation matrix `RAD-VAL-001` bagian 2; permission matrix `RAD-PERM-001` bagian 5.2 dan 8; kemampuan `RAD-CAP-003` |
| Contract version | `RAD-STATE-001` dan `RAD-VAL-001` — status `approved` 2026-09-10; nama DTO mengikuti `RAD-API-001` revision 2 `approved`, grup *Master Data / Rad Safety Rule* yang masih berlabel **Rencana (belum tersedia)** |
| Dependency | `BE-RAD-01` — **terpenuhi** 2026-09-10. Ditambah `BE-RAD-06` yang sudah lebih dulu memindahkan penilaian gerbang ke `RuleStatus` |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 2, kontrak API 1, database 1, keamanan 2, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/RadiologyManagement/`, `Program.cs`, `Tests/QuilvianSystemBackend.UnitTests.InMemory/`, `docs/module-blueprints/radiologi/` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `0e2eb105` |
| Tanggal | 2026-09-10 |
| Status | **Selesai untuk lingkupnya.** Build lulus 0 error; `RadSafetyPolicyServiceTests` 18 lulus; `RadiologySafetyGateTests` 20 tetap lulus. Belum dapat dipakai orang: endpoint dan layarnya milik `BE-RAD-03` dan `FE-RAD-04` |

### Preflight Kontrak Rekayasa Backend

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `RadiologyManagement / Radiology` |
| Submodule | Tidak berlaku |
| Pemilik / prefix registry | Prefix `Rad`, lifecycle `ACTIVE` sejak 2026-09-10 |
| Keberlakuan | `NEW CODE` untuk `RadSafetyPolicyService` dan DTO-nya; `TOUCHED LEGACY` untuk `RadOperationResult`, `RadStudyController`, dan `Program.cs` |
| QBE ID yang berlaku | `QBE-MOD-001` capability tinggal di modul pemiliknya; `QBE-SVC-001` orkestrasi domain dimiliki Module Service dan controller tidak menyentuh context; `QBE-VAL-001` request dan invarian bisnis divalidasi backend; `QBE-DTO-001` entity EF tidak dipakai sebagai kontrak API; `QBE-ENUM-001` enum tetap dimiliki modul; `QBE-LOG-001` setiap perubahan status menghasilkan log beserta aktornya; `QBE-AUD-001` audit database terpisah dari application logging |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*` dan `QBE-CFG-*` — tidak ada entity maupun configuration baru; `QBE-NAM-*` dan `QBE-DB-*` — tidak ada penamaan fisik maupun migration; `QBE-CODE-*` — modul ini tidak mengalokasikan nomor bisnis; `QBE-PAGE-001` dan `QBE-OPT-001` — tidak ada capability list pada task ini |
| Pengecualian yang disetujui | `NONE` |

---

## 1. Masalah yang diperbaiki

Sampai hari ini, aturan keselamatan radiologi **tidak dapat dibuat lewat aplikasi sama sekali**.
`RAD-FACT-009` mencatatnya apa adanya: gerbang menolak setiap pemeriksaan bila aturannya belum
ada, dan tidak ada satu pun cara memasukkan aturan itu selain menulis langsung ke database.

`BE-RAD-01` menyiapkan tempat penyimpanan siklus pengesahannya, dan `BE-RAD-06` membuat gerbang
menilai `RuleStatus`. Yang belum ada adalah aturan mainnya sendiri: siapa boleh menyusun, siapa
boleh mengesahkan, dan apa yang terjadi bila keduanya orang yang sama.

> **Contoh nyata yang ditutup task ini.** Admin Radiologi menyusun aturan "skrining kehamilan
> tidak wajib untuk CT abdomen" karena dianggap memperlambat antrian, lalu menekan tombol
> Sahkan sendiri. Tanpa pemeriksaan di dalam service, satu orang baru saja mengubah syarat
> penyinaran bagi seluruh pasien perempuan — dan sistem izin **tidak akan menahannya**, karena
> izin hanya menjawab "boleh mengesahkan atau tidak", bukan "boleh mengesahkan **yang ini**".

Itulah alasan pengaman ini wajib berupa kode, bukan pengaturan peran.

---

## 2. Proses bisnis

**Tujuan.** Memastikan aturan yang menentukan keselamatan pasien hanya berlaku setelah dinilai
dua pihak yang berbeda.

**Pelaku.**

| Pelaku | Tugasnya | Penandanya di sistem |
| --- | --- | --- |
| Admin Radiologi | Menyusun draf, memperbaikinya, lalu mengajukan pengesahan | `RadSafetyRule : Create`, `Update`, `Submit` |
| Penanggung jawab klinis | Mengesahkan, menolak, atau menghentikan aturan | `RadSafetyRule : Approve` — sekaligus penanda perannya, sesuai `RAD-DEC-015` |

**Pemicu.** Rumah sakit menetapkan atau mengubah butir keselamatan yang wajib untuk sebuah alat.

**Prasyarat.** Alat pencitraan dan butir keselamatan sudah terdaftar dan masih aktif.

**Langkah berurutan.**

1. Admin menyusun draf. Aturan lahir berstatus `Draft`, bernomor versi `1`, dan **belum menahan
   apa pun**.
2. Admin memperbaiki drafnya sebanyak yang diperlukan. Selama masih draf, isinya bebas diubah.
3. Admin mengajukan pengesahan. Status menjadi `PendingApproval`, dan nama pengaju beserta
   waktunya dicatat.
4. Penanggung jawab klinis membaca pengajuan, lalu memutuskan:
   - **Sahkan** — status menjadi `Active`, nomor versi naik dari `1` menjadi `2`, dan sejak
     detik itu aturan tersebut ikut menahan pemeriksaan.
   - **Tolak** — status kembali menjadi `Draft`, disertai alasan yang wajib diisi. Nomor versi
     **tidak** berubah.
5. Ketika aturannya perlu diganti, penanggung jawab klinis menghentikannya lebih dulu
   (`Inactive`), baru penggantinya disahkan.

**Aturan yang berlaku, dengan angkanya.**

| Aturan | Contoh berangka |
| --- | --- |
| Versi naik tepat satu kali, hanya saat pengesahan | Draf versi `1` → diajukan, tetap `1` → ditolak, tetap `1` → diajukan ulang, tetap `1` → **disahkan, menjadi `2`**. Bukan `3`, bukan `4` |
| Yang menyusun tidak boleh mengesahkan | Aturan disusun `Admin A`; `Admin A` menekan Sahkan → ditolak `403` |
| Yang mengajukan juga tidak boleh mengesahkan | Disusun `Admin A`, diajukan `Admin B`; `Admin B` menekan Sahkan → ditolak `403` |
| Satu kombinasi hanya boleh punya satu aturan berlaku | CT-Scan + seluruh pemeriksaan + skrining kehamilan sudah punya aturan `Active`; pengesahan aturan kedua untuk kombinasi yang sama → ditolak `409` |
| Aturan yang sedang berlaku tidak dapat diubah | Ditolak `403`, dengan pesan yang menunjukkan jalan yang benar: susun draf baru |

**Jalur tidak normal.**

| Keadaan | Yang terjadi | Mengapa begitu |
| --- | --- | --- |
| Penolakan tanpa alasan | Ditolak `400`, status tetap `PendingApproval` | Tanpa alasan, penyusun tidak tahu apa yang harus diperbaiki, dan pengajuan yang sama akan kembali berulang |
| Draf langsung disahkan tanpa diajukan | Ditolak `409` | Melompati pengesahan berjenjang |
| Aturan yang sudah berlaku disahkan lagi | Ditolak `409`, nomor versi **tidak** naik lagi | Mencegah versi bergerak tanpa keputusan baru |
| Aturan yang belum berlaku dinonaktifkan | Ditolak `409` | Tidak ada yang perlu dihentikan |
| Alat sudah dipensiunkan saat draf diajukan | Ditolak `422` | Keaktifan alat diperiksa ulang saat pengajuan, bukan hanya saat penyusunan; jarak waktu antara keduanya bisa panjang |
| Dua pengesahan berjalan hampir bersamaan | Satu berhasil, satu ditolak `409` | Pemeriksaan di service ditambah index unik pada database |

**Hasil akhir.** Rumah sakit dapat menetapkan aturan keselamatannya sendiri lewat aplikasi,
dengan jejak siapa menyusun, siapa mengajukan, siapa memutuskan, dan atas dasar apa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola.** `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`;
`rules/backend/TASK_CLASSIFICATION.md`; `rules/backend/REVIEW_RULES.md`;
`rules/backend/REPORT_TEMPLATE.md`; `rules/rule-output/aturan-output-dokumentasi.md`.

**Blueprint dan kontrak.** `roadmap/backend-roadmap.md`; `roadmap/requirement-traceability.md`;
`contracts/state-transition-matrix.md` bagian 5; `contracts/validation-matrix.md` bagian 2;
`contracts/permission-audit-matrix.md` bagian 4, 5, 6, 7, dan 8; `contracts/api-contract.md`
bagian 2 dan 4; `testing/acceptance-test-matrix.md`; `00-interview-decisions.md` untuk
`RAD-DEC-015`; laporan `BE-RAD-01.md` dan `BE-RAD-06.md`.

**Source pembanding.** `Services/RadStudyService.cs` dan `Services/RadOrderService.cs` sebagai
pola service radiologi; `Services/RadOperationResult.cs`; `Controllers/RadStudyController.cs`;
`DTOs/RadiologyDtos.cs`; `Models/MstRadModalitySafetyRule.cs`;
`LaboratoryManagement/Services/LabCriticalBoundApprovalService.cs` sebagai preseden pengesahan
berjenjang terdekat di repository ini; `Services/Security/AccessPermissionService.cs`;
`Filters/AccessPermissionFilter.cs`; `Program.cs`;
`Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/` untuk pola uji tanpa
database.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RadiologyManagement/Services/RadSafetyPolicyService.cs` | **Baru.** Enam tindakan: susun draf, ubah draf, ajukan, sahkan, tolak, nonaktifkan. Memuat penomoran versi, penjagaan tabrakan, dan pemeriksaan pengesahan sendiri |
| `Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs` | Empat DTO baru dengan nama persis seperti `RAD-API-001`: `CreateRadSafetyRuleRequest`, `UpdateRadSafetyRuleRequest`, `RadSafetyRuleRejectRequest`, `RadSafetyRuleResponse` |
| `Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs` | Dua jenis hasil baru — `Forbidden` untuk `403` dan `BusinessRule` untuk `422` — beserta delapan kode galat siklus pengesahan |
| `Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs` | Pemetaan status untuk dua jenis hasil baru itu. Tanpa ini keduanya akan jatuh ke cabang bawaan dan terkirim sebagai `400` yang menyesatkan |
| `Program.cs` | Pendaftaran `RadSafetyPolicyService` sebagai scoped service, sebaris dengan dua service radiologi yang sudah ada |
| `Tests/.../UnitTests.InMemory/RadiologyManagement/RadSafetyPolicyServiceTests.cs` | **Baru.** 18 uji tanpa database |
| `docs/module-blueprints/radiologi/roadmap/backend-roadmap.md` | Baris keadaan `BE-RAD-02` |
| `docs/module-blueprints/radiologi/roadmap/requirement-traceability.md` | Bukti pada `RAD-DEC-005`, `RAD-CAP-003`, slice `S4`, dan satu butir Definition of Done modul |

### 3.3 Keputusan rancangan yang perlu diketahui

**Pemeriksaan pengesahan sendiri memeriksa dua kolom, bukan satu.** Yang dibandingkan bukan
hanya pengaju (`SubmittedByUserId`), tetapi juga penyusun (`CreateBy`). Memeriksa pengaju saja
meninggalkan jalan memutar yang paling mudah ditempuh: Admin A menyusun draf, lalu meminta
Admin B sekadar menekan tombol Ajukan, sehingga A bebas mengesahkan aturan yang ia tulis
sendiri.

**Kolom lama `IsActive` ikut diisi.** Sejak `BE-RAD-06`, yang menentukan gerbang adalah
`RuleStatus`. `IsActive` tetap diisi mengikutinya — `false` selama belum disahkan, `true` saat
disahkan, `false` lagi saat dihentikan — supaya kedua kolom itu tidak pernah berselisih pada
baris yang lahir dari jalur ini. Penghapusan kolomnya tetap pekerjaan tersendiri.

**Jejak penolakan tidak dihapus saat pengajuan ulang.** `RAD-PERM-001` bagian 8 menyatakan
penolakan aturan keselamatan termasuk audit yang tidak boleh diubah. Menghapusnya saat aturan
diajukan ulang akan menghilangkan satu-satunya catatan bahwa aturan itu pernah dinilai tidak
layak.

**Draf sengaja tidak diperiksa tabrakannya.** Admin perlu dapat menyiapkan pengganti sebuah
aturan sebelum aturan lamanya dihentikan. Yang dijaga adalah saat pengesahan.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Belum ada endpoint baru.** Empat DTO disiapkan dengan nama persis seperti `RAD-API-001` supaya `BE-RAD-03` tinggal memakainya. Satu perubahan menyentuh endpoint yang sudah berjalan: `RadStudyController` kini memetakan `Forbidden` menjadi `403` dan `BusinessRule` menjadi `422`. Tidak ada endpoint `Rad Study` yang mengembalikan kedua jenis itu hari ini, sehingga tidak ada respons yang berubah |
| Database | Tidak ada perubahan schema, entity, maupun index. **Tidak ada migration yang dibuat dan tidak ada perintah database yang dijalankan.** Seluruh kolom yang dipakai sudah tersedia sejak `BE-RAD-01` |
| Keamanan/Auth | **Inti task ini.** Pemisahan wewenang menyusun dan mengesahkan ditegakkan di dalam service, sesuai `RAD-PERM-001` bagian 5.2. Tidak ada atribut otorisasi yang diubah atau dilemahkan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menambah satu pun endpoint. Seluruh delapan endpoint grup
*Master Data / Rad Safety Rule* pada `RAD-API-001` menjadi pekerjaan `BE-RAD-03`, beserta string
`[AccessPermission(...)]`-nya.

Perlu dicatat: sampai `BE-RAD-03` selesai, **aturan bisnis ini belum dapat dipanggil siapa pun**.
Yang sudah ada adalah isinya, bukan pintunya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test --filter FullyQualifiedName~RadSafetyPolicyServiceTests` | Build berhasil `EXIT=0`, 0 error; **18 lulus, 0 gagal**, 13 detik | `PASS` | Keluaran perintah |
| Draf lahir belum berlaku | Status `Draft`, versi `1`, `IsActive` bernilai `false` | `PASS` | `DrafLahirBelumBerlaku` |
| **AC-13** — penyusun mengesahkan aturannya sendiri | Ditolak `Forbidden`; status tetap `PendingApproval`; versi tetap `1`; `ApprovedAt` tetap kosong | `PASS` | `PenyusunMengesahkanAturannyaSendiri_Ditolak` |
| **AC-13** — pengaju mengesahkan walau bukan penyusun | Keduanya ditolak `Forbidden` | `PASS` | `PengajuMengesahkanWalauBukanPenyusun_Ditolak` |
| **AC-14** — pengesahan menaikkan versi tepat satu kali | Versi `1` menjadi `2`; pengesah dan waktunya tercatat | `PASS` | `Pengesahan_MenaikkanVersiTepatSatuKali` |
| **AC-14** — pengesahan berulang | Ditolak `Conflict`; versi tetap `2` | `PASS` | `PengesahanBerulang_TidakMenaikkanVersiLagi` |
| **AC-14** — penolakan di tengah jalan | Setelah ditolak lalu diajukan ulang dan disahkan, versinya tetap `2` | `PASS` | `PenolakanDiTengahJalan_TidakMenaikkanVersi` |
| **AC-15** — penolakan tanpa alasan | Ditolak `Validation` dengan kode `RAD_REASON_REQUIRED`; status tetap `PendingApproval` | `PASS` | `PenolakanTanpaAlasan_Ditolak` |
| **AC-15** — penolakan beralasan | Kembali `Draft`; alasan, penolak, dan waktunya tercatat; versi tidak berubah | `PASS` | `PenolakanBeralasan_MengembalikanAturanKeDraf` |
| Jejak penolakan bertahan saat diajukan ulang | Alasan dan waktu penolakan tetap ada | `PASS` | `JejakPenolakanTidakTerhapusSaatDiajukanUlang` |
| Pengesahan kedua untuk kombinasi sama | Ditolak `Conflict` dengan kode `RAD_ACTIVE_SAFETY_RULE_EXISTS` | `PASS` | `PengesahanKeduaUntukKombinasiSama_Ditolak` |
| Pengganti disahkan setelah aturan lama dihentikan | Berhasil | `PASS` | `PenggantiDapatDisahkanSetelahAturanLamaDihentikan` |
| Aturan berlaku tidak dapat diubah | Ditolak `Forbidden`; isinya tidak berubah | `PASS` | `AturanYangSedangBerlaku_TidakDapatDiubah` |
| Draf belum diajukan tidak dapat disahkan | Ditolak `Conflict` | `PASS` | `DrafBelumDiajukan_TidakDapatDisahkan` |
| Aturan belum berlaku tidak dapat dinonaktifkan | Ditolak `Conflict` | `PASS` | `AturanYangBelumBerlaku_TidakDapatDinonaktifkan` |
| Aturan yang tidak ada | Ditolak `NotFound` | `PASS` | `AturanYangTidakAda_Ditolak404` |
| Alat sudah dipensiunkan | Ditolak `BusinessRule` — `422` | `PASS` | `DrafPadaAlatYangSudahDipensiunkan_Ditolak` |
| Masa berlaku terbalik | Ditolak `Validation` — `400` | `PASS` | `MasaBerlakuTerbalik_Ditolak` |
| Sambungan ke gerbang keselamatan | Sebelum disahkan gerbang membaca "kebijakan belum ditetapkan"; sesudah disahkan gerbang menahan lalu dapat dituntaskan pada versi `2` | `PASS` | `HanyaAturanYangSudahDisahkanYangMenahanPasien` |
| Regresi `RadiologySafetyGateTests` | **20 lulus, 0 gagal** setelah `RadOperationResult` dan `RadStudyController` disentuh | `PASS` | Perintah dijalankan ulang setelah seluruh perubahan |
| Warning baru dari berkas radiologi | Tidak ada satu pun | `PASS` | Penyaringan 213 warning build; seluruhnya warning lama dari modul lain dan konflik versi paket pada project uji |
| `RadiologyStudyLifecycleTests` | `NOT RUN` | `NOT RUN` | Membutuhkan `QUILVIAN_BILLING_TEST_DB` yang sengaja tidak diisi. Task ini tidak mengubah jalur study, dan uji gerbangnya sudah dijalankan sebagai gantinya |

Uji manual: `NOT FEASIBLE` — belum ada endpoint maupun layar yang dapat dijalankan.

### Batas verifikasi yang perlu diketahui

Seluruh 18 uji berjalan di atas penyedia **in-memory**, bukan PostgreSQL. Akibatnya satu hal
**tidak** ikut terbukti: index unik pada database sebagai penjaga terakhir ketika dua pengesahan
berjalan hampir bersamaan. Yang terbukti adalah pemeriksaan di dalam service; penangkap
`DbUpdateException` di belakangnya baru dapat diuji terhadap database sungguhan.

Ada pula satu hal yang **tidak dapat** dijaga index tersebut, dan ini perlu diketahui pemilik
modul: pada PostgreSQL, dua baris dianggap berbeda bila salah satu kolom kuncinya kosong.
Aturan yang berlaku untuk **seluruh pemeriksaan** menyimpan `ProcedureId` kosong, sehingga dua
aturan semacam itu **tidak** akan ditolak index. Untuk kasus tersebut, pemeriksaan di dalam
service inilah satu-satunya penjaga — dan itu sudah diuji.

**Tidak dijalankan:**

| Yang tidak dijalankan | Alasan |
| --- | --- |
| Uji integrasi Postgres radiologi | Membutuhkan database test yang belum dikonfigurasi; mengisinya adalah wewenang terpisah milik pemilik modul |
| Pembuatan atau penerapan migration | Task ini tidak menyentuh schema |
| Pemasangan endpoint beserta atribut hak aksesnya | Milik `BE-RAD-03` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-13 — admin mengesahkan aturannya sendiri ditolak `403` | **Terpenuhi** | Dua uji: penyusun dan pengaju sama-sama ditolak `Forbidden` |
| AC-14 — pengesahan menaikkan versi tepat satu kali | **Terpenuhi** | Tiga uji, termasuk pengesahan berulang dan penolakan di tengah jalan |
| AC-15 — menolak tanpa alasan ditolak `400` | **Terpenuhi** | Status tetap `PendingApproval` setelah penolakan ditolak |
| Pengesahan kedua untuk kombinasi sama ditolak `409` | **Terpenuhi** | `PengesahanKeduaUntukKombinasiSama_Ditolak` |
| Seluruh transisi `RAD-STATE-001` bagian 5 terbukti | **Terpenuhi** | Susun draf, ubah draf, ajukan, sahkan, tolak, nonaktifkan, dan susun ulang setelah dihentikan — seluruhnya punya uji |
| Transisi tidak sah ditolak dengan kode yang benar | **Terpenuhi** | `403` untuk yang memang tidak boleh, `409` untuk yang salah urutan, `422` untuk data induk yang sudah tidak aktif, `400` untuk isian yang kurang |
| Pemisahan wewenang ditegakkan di service, bukan hanya di atribut endpoint | **Terpenuhi sebagian** | Bagian yang **tidak dapat** ditegakkan sistem izin — perbandingan penyusun versus pengesah — sudah menjadi kode dan diuji. Bagian "pelakunya penanggung jawab klinis" tetap bersandar pada penanda `[AccessPermission("RadSafetyRule", "Approve")]` yang dipasang `BE-RAD-03`. Lihat penjelasan di bawah |

### Mengapa pemeriksaan hak akses tidak diulang di dalam service

`RAD-DEC-015` menetapkan penanggung jawab klinis dikenali lewat kepemilikan
`RadSafetyRule : Approve` — hak akses itu **adalah** penanda perannya. Mengulang pemeriksaan
yang sama di dalam service tidak menambah pengaman apa pun terhadap ancaman yang nyata, karena
keduanya membaca tabel yang sama lewat `AccessPermissionService`, termasuk saklar pengembangan
yang dapat mematikan seluruh pemeriksaan hak akses di luar produksi.

Yang **tidak** dapat dijawab sistem izin adalah pertanyaan "orang ini sudah menyentuh baris ini
sebelumnya atau belum". Persoalan itulah yang menjadi kode di sini. Pembagiannya sama persis
dengan `LabCriticalBoundApprovalService`, preseden terdekat di repository ini, yang mencatat
alasan yang sama.

Konsekuensinya perlu disebut terbuka: sampai `BE-RAD-03` memasang atribut hak aksesnya,
`RadSafetyPolicyService` **tidak** memeriksa apakah pemanggilnya penanggung jawab klinis. Selama
belum ada endpoint yang memanggilnya, tidak ada yang terbuka; begitu endpointnya dibuat,
atribut itu wajib ada. `BE-RAD-14` sudah direncanakan untuk membuktikannya lewat uji kontrak
hak akses.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build menghasilkan 213 warning, seluruhnya sudah ada sebelumnya: warning dokumentasi XML pada modul lain dan konflik versi paket pada project uji. **Tidak ada warning baru dari berkas radiologi** |
| Masalah yang diketahui | Aturan bisnis ini belum dapat dipanggil siapa pun sampai `BE-RAD-03` selesai. Pembacaan daftar aturan dan `GET /coverage` sengaja **tidak** dikerjakan di sini karena keduanya milik `BE-RAD-03` |
| Risiko tersisa | **Pertama**, penjaga terakhir berupa index unik belum teruji terhadap PostgreSQL sungguhan. **Kedua**, index itu tidak menjaga aturan ber-`ProcedureId` kosong, sehingga pemeriksaan di service menjadi satu-satunya penjaga untuk kasus tersebut. **Ketiga**, sampai atribut hak akses terpasang, kewenangan pengesah belum diperiksa di mana pun |
| Temuan di luar cakupan | `RAD-VAL-001` bagian 4 menyebut kode `409` untuk "aturan keselamatan belum ada", sementara source yang berjalan mengembalikan `422` lewat `RadStudyController`. Selisih ini sudah ada sebelum task ini dan **tidak** disentuh; perlu diselaraskan pemilik modul, entah kontraknya atau source-nya. Selain itu `RAD-STATE-001` bagian 5 menyebut penonaktifan aturan sebagai wewenang penanggung jawab klinis, sedangkan `RAD-PERM-001` bagian 4 memberi endpoint itu penanda `RadSafetyRule : Deactivate` yang berbeda dari `Approve` — perlu diputuskan sebelum `BE-RAD-03` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat bagian di bawah |
| Langkah berikutnya | `BE-RAD-03` — delapan endpoint beserta string `[AccessPermission(...)]` persis seperti `RAD-PERM-001` bagian 4, ditambah pembacaan daftar dan `GET /coverage`. Sesudahnya `BE-RAD-04`, `BE-RAD-05`, lalu `BE-RAD-15` untuk mengisi data master awal |

### Perubahan yang bukan milik task ini

Perubahan berikut sudah ada di worktree sebelum task ini dan **sengaja dibiarkan utuh**: berkas
`BE-RAD-01` (`RadiologyEnums.cs`, `MstRadModalitySafetyRule.cs`,
`MstRadModalitySafetyRuleConfiguration.cs`, snapshot model, dan dua berkas migration), berkas
`BE-RAD-06` (`RadSafetyGateEvaluator.cs`, `RadStudyService.cs`, dan dua berkas uji radiologi),
serta `EmergencyOrderKind.cs`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, seluruh berkas blueprint
radiologi selain dua berkas roadmap, dan berkas blueprint modul Rekam Medis.

### Status Git pada akhir pekerjaan

Berkas yang berubah karena `BE-RAD-02`:

```text
 M Areas/HealthServices/RadiologyManagement/Controllers/RadStudyController.cs
 M Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs
 M Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs
 M Program.cs
?? Areas/HealthServices/RadiologyManagement/Services/RadSafetyPolicyService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/RadiologyManagement/
```

Ditambah dua berkas dokumentasi di dalam folder yang belum terlacak Git
(`docs/module-blueprints/radiologi/roadmap/` dan `.../task/`):

```text
docs/module-blueprints/radiologi/roadmap/backend-roadmap.md          — baris keadaan BE-RAD-02
docs/module-blueprints/radiologi/roadmap/requirement-traceability.md — bukti pada empat baris
docs/module-blueprints/radiologi/task/report/backend/BE-RAD-02.md    — laporan ini, berkas baru
```

Tidak ada `git add`, commit, maupun push yang dilakukan.
