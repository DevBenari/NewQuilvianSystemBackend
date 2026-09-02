# FE-RWI-016 — Empat daftar pantau tersedia dan tidak menghalangi tindakan

- TASK ID: `FE-RWI-016`
- TASK TYPE: Layar daftar baru — satu halaman gabungan bertab yang mengonsumsi empat endpoint daftar pantau yang sudah tersedia
- COMPLEXITY: `MEDIUM`
- CLASSIFICATION SCORE: 8 — dua repository 2; 9–20 berkas diperiksa 1; 4–8 berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; empat bentuk jawaban berbeda pada satu layar 1; alur berbatas pada satu halaman 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca, kecuali laporan ini beserta tautan buktinya
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — keempat endpoint `GET /monitoring/pending-closures`, `/closures-without-financial-clearance`, `/unassigned-nurse-episodes`, dan `/isolation-mismatch` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-016` beserta `BE-RWI-029` dan `BE-RWI-015`; `03-frontend-architecture.md` bagian 2, 3, 4.4, 5.1, 5.2, dan 6; `contracts/api-contract.md` bagian Inpatient Monitoring; `contracts/permission-audit-matrix.md` bagian 2 dan 3; `requirement-traceability.md`; [laporan `FE-RWI-015`](FE-RWI-015.md); `InpatientMonitoringController.cs`; `InpatientMonitoringDtos.cs` (`InpatientMonitoringQuery`, `PendingClosureItemResponse`, `OverrideClosureItemResponse`, `UnassignedNursePagedResult`, `IsolationMismatchItemResponse`); `InpCensusQueryService.cs` bagian `GetPendingClosuresAsync`, `GetOverrideClosuresAsync`, `GetUnassignedNurseEpisodesPagedAsync`, dan `GetIsolationMismatchAsync`; `AGENTS.md` frontend beserta tujuh dokumen `.codex`; `inpatient-api.service.js`; `inpatient-monitoring.service.js`; `inpatient-management-slice.jsx`; `use-inpatient-census.jsx`; `inpatient-census-view.jsx`; `inpatient-census-constants.jsx`; `inpatient-census-utils.jsx`; `inpatient-episode-detail-view.jsx`; `data-table.jsx`; `data-filter.jsx`; `status-badge.jsx`; `menu-items.jsx`
- FILES CHANGED:
  - **Baru** `src/app/health-services/inpatient-management/monitoring/page.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-monitoring.jsx`
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-monitoring-utils.jsx`
  - **Baru** `tests/unit/inpatient-monitoring.test.mjs`
  - **Baru** `tests/e2e/inpatient-monitoring.spec.mjs`
  - **Diubah** `src/utils/menu-sidebar/menu-items.jsx` (menu Daftar Pantau)

## 1. Satu halaman bertab, dan alasannya bukan kemalasan

`RWI-FE-002` `DEV_DISCRETION` membebaskan satu halaman gabungan atau beberapa halaman terpisah.
Dipilih **satu halaman bertab** karena keempat daftar dibaca **orang yang sama pada saat yang
sama**: kepala ruangan atau supervisor yang sedang memeriksa apa yang perlu ditindaklanjuti hari
ini. Empat halaman terpisah memaksa pembacanya berpindah menu empat kali untuk menjawab satu
pertanyaan.

Konsekuensinya dijaga: **jawaban daftar sebelumnya tidak pernah terbaca sebagai isi daftar yang
sedang dibuka.** Slot Redux `monitoring` memang hanya satu, jadi muatannya membawa `listKey`, dan
layar hanya memakainya ketika kunci itu cocok dengan tab aktif. Tanpa itu, berpindah tab akan
menampilkan baris daftar lama selama sepersekian detik — dan baris daftar pantau yang salah tempat
adalah kebingungan yang mahal.

Laporan selisih tempat tidur **tidak** menjadi tab kelima di sini. Ia layar tersendiri
`FE-RWI-017`, karena kriteria 3 task itu menuntut cakupan pembaca yang lebih sempit daripada
keempat daftar ini ([laporan](FE-RWI-017.md)).

## 2. Lama keterlambatan: satu daftar dihitung server, dua daftar dihitung layar

Ini delta yang perlu dicatat, bukan ditutupi.

| Daftar | Sumber lamanya | Akibatnya |
| --- | --- | --- |
| Penutupan tertunda | `PendingHours` **dari server**, beserta `ThresholdHours` | Angkanya tidak bergantung pada jam browser petugas — dan memang tidak boleh, karena ambangnya milik server |
| Penutupan menembus gerbang | Dihitung layar dari `ClosedAt` | Memakai jam browser |
| Tanpa perawat penanggung jawab | Dihitung layar dari `AdmittedAt` | Memakai jam browser |

`OverrideClosureItemResponse` dan `CensusItemResponse` **tidak punya** kolom lama, sehingga layar
tidak punya pilihan lain selain menurunkannya dari waktu yang memang dikirim. Keduanya tidak
menggerbang apa pun — tidak ada tombol yang aktif atau nonaktif karena angka itu — sehingga jam
browser yang meleset menghasilkan angka yang salah, bukan keputusan yang salah.

Waktu acuannya ditangkap **sekali** pada saat jawaban tiba, bukan pada setiap render. Angka
keterlambatan yang dihitung ulang tiap render akan bergerak sendiri di layar tanpa data baru, dan
angka yang bergerak sendiri tidak dapat dipercaya pembacanya.

## 3. Daftar keempat bernada berbeda, dan perbedaannya dijaga test

`03-frontend-architecture.md` bagian 4.4 menyatakan isi daftar penempatan tidak sesuai **bukan**
keterlambatan petugas melainkan akibat wajar perubahan kondisi klinis. Yang dikerjakan:

1. Daftar itu **tidak punya kolom lama keterlambatan sama sekali** — e2e memeriksa
   `monitoring-delay` bernilai `toHaveCount(0)` pada tab itu.
2. Judul dan kalimat keadaan kosongnya tidak menyebut keterlambatan maupun kelalaian; keterangannya
   menyebut kata itu **hanya untuk menyangkalnya**, dan test unit mengunci penyangkalan itu apa
   adanya sekaligus memastikan penyangkalan yang sama tidak menempel pada tiga daftar lain yang
   memang mengukur keterlambatan.
3. Tindakan berikutnya — **Pindahkan Pasien** — ada pada setiap baris, menuju detail episode tempat
   perpindahan tempat tidur dikerjakan.
4. Kalimat ketidakcocokannya **disalin apa adanya** dari `MismatchMessage` server. Menyusun ulang
   di layar berisiko mengubah nadanya menjadi tuduhan.

## 4. Privasi pada daftar

`IsolationMismatchItemResponse` tidak membawa `IsolationNote`, dan layar juga tidak pernah
membacanya: penormalnya hanya menyalin kolom yang disebut satu per satu. Test unit membuktikan
kolom terlarang yang disisipkan ke payload uji **tidak** ikut masuk hasil penormalan, dan bahwa
kata `isolationNote` tidak muncul sama sekali pada sumber layar. e2e membuktikan keterangan klinis
yang sengaja ditaruh pada payload detail episode tidak pernah terbaca di halaman daftar.

Penanda "Perlu tempat tidur isolasi" boleh tampil; alasan klinisnya tidak pernah —
`03-frontend-architecture.md` bagian 6.

- IMPLEMENTATION: (1) Satu halaman `/health-services/inpatient-management/monitoring` dengan empat tab; kunci tab **sama persis** dengan potongan path endpoint, sehingga selisih penamaan langsung terlihat sebagai 404 alih-alih sebagai daftar yang diam-diam kosong. (2) Hook memakai `inpatientMonitoringService` yang **sudah ada** sejak `FE-RWI-002` dan slot Redux `monitoring` yang **sudah terdaftar** — tidak ada service baru, tidak ada slice baru, tidak ada pola HTTP baru. (3) Empat penormal terpisah, satu per bentuk jawaban; menyatukannya menjadi satu penormal serbaguna akan menyembunyikan perbedaan yang justru penting, misalnya bahwa hanya daftar pertama yang lamanya dihitung server. (4) Daftar episode tanpa perawat memakai `normalizeCensusItem` yang **sudah ada**, sehingga penyaring kolom census ikut berlaku pada daftar ini. (5) Kolom `IsBedStillHeld` dirender sebagai dua keadaan yang berbeda: tempat tidur yang masih ditahan mendesak, yang sudah bebas tidak. (6) Muat ulang saat layar difokuskan kembali — bagian 5.2, risiko basi sedang. (7) Penyaring unit layanan dan jumlah baris memakai `DataFilter`, `ResourceFilterSelect`, `FilterSelect`, `DataTable`, dan `RegionPagination` yang sudah ada; setiap penyaringan mengembalikan pembacaan ke halaman pertama
- API CONTRACT IMPACT: Tidak mengubah kontrak. Query memakai nama kolom `InpatientMonitoringQuery` apa adanya (`serviceUnitId`, `pageNumber`, `pageSize`), dan `IsolationMismatchQuery` menerima ketiganya juga — penyaring `roomId` yang hanya dimiliki daftar keempat sengaja tidak dipakai, karena penyaring unit layanan sudah cukup untuk semua tab dan penyaring yang muncul-hilang antar tab membingungkan
- DATABASE IMPACT: Tidak ada
- SECURITY IMPACT: Tidak mengubah authorization. Kelima endpoint daftar pantau dijaga butir yang sama, `InpatientMonitoring : Read`; layar ini tidak menyempitkannya, dan peran yang tidak punya butir itu dijawab 403 lalu ditangkap `AccessDeniedGate` yang sudah ada. Tidak ada kolom sensitif yang dibaca
- VISUAL REFERENCE: NOT REQUIRED
- WEWENANG UI YANG DIPAKAI: "`RWI-FE-002` `DEV_DISCRETION`: satu halaman gabungan atau beberapa halaman terpisah, urutan kolom, cara menandai keterlambatan, dan penempatan menu semuanya bebas. **Batasnya:** lama keterlambatan wajib terbaca, dan daftar **tidak boleh** menghalangi tindakan apa pun". Dipilih satu halaman bertab dengan alasan pada bagian 1; kedua batasnya dipenuhi dan dibuktikan pada kriteria 2 dan 5. Menu Rawat Inap ditambah satu butir "Daftar Pantau", diletakkan sesudah Pasien Sedang Dirawat karena keduanya sama-sama dibaca kepala ruangan pada pergantian sif. Tidak ada komponen baru, tidak ada arsitektur state baru

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Keempat daftar tersedia | **LULUS** | e2e membuka keempat tab berurutan dan membuktikan tiap tab menampilkan barisnya sendiri, serta merekam bahwa keempat kunci endpoint — `pending-closures`, `closures-without-financial-clearance`, `unassigned-nurse-episodes`, `isolation-mismatch` — benar-benar dipanggil. Test unit mengunci keempat kunci itu sama persis dengan endpoint backend |
| 2. Lama keterlambatan terbaca pada tiga daftar pertama | **LULUS** | e2e membaca "2 hari 3 jam" beserta "Ambang 24 jam" pada daftar pertama, "sejak episode ditutup" pada daftar kedua, dan "sejak waktu admisi" pada daftar ketiga. Test unit membuktikan angka daftar pertama dipakai **apa adanya dari server** (51 jam, bukan hasil hitungan layar), sedangkan dua daftar lain diturunkan dari waktu yang dikirim server; keduanya diuji terhadap waktu acuan tetap. Bentuk katanya diuji pada enam nilai, termasuk nol jam dan waktu yang tidak terbaca |
| 3. Daftar penempatan tidak sesuai menampilkan tindakan berikutnya, dan nadanya tidak menuduh | **LULUS** | e2e membuktikan tombol "Pindahkan Pasien" ada pada barisnya, kalimat ketidakcocokan tampil **disalin apa adanya dari server**, peringatan "Ini bukan daftar keterlambatan" beserta "bukan petugas yang lalai" terbaca, dan kolom lama keterlambatan `toHaveCount(0)` pada tab itu. Test unit mengunci ketiadaan kata keterlambatan pada judul dan kalimat kosongnya, mengunci penyangkalannya pada keterangannya, dan membuktikan penyangkalan itu **tidak** menempel pada tiga daftar lain |
| 4. Daftar kosong menampilkan keadaan kosong yang jelas, bukan galat | **LULUS** | e2e membuka keempat tab dengan jawaban kosong dan membaca empat kalimat kosong yang berbeda, masing-masing menjelaskan kenapa kosong; layar Akses Ditolak dan tombol Coba Lagi sama-sama `toHaveCount(0)`. Test unit membuktikan kelima penormal — termasuk laporan selisih — mengembalikan daftar kosong untuk payload kosong **dan** untuk `null`, bukan melempar |
| 5. Membuka daftar tidak menahan satu pun tindakan di layar lain | **LULUS** | e2e membuka keempat daftar, membuktikan **nol** permintaan selain `GET` terkirim dari halaman ini, lalu menekan "Pindahkan Pasien" menuju detail episode: keterangan penahan `transfer-disabled-reason` `toHaveCount(0)`, tempat tidur isolasi yang kosong dapat dipilih, dan tombol pindah menjadi aktif. Test unit membaca sumber hook dan membuktikan ia tidak punya satu pun `post`, `put`, `patch`, atau `delete` — daftar pantau tidak punya jalan mengubah apa pun |

- VALIDATION: e2e `tests/e2e/inpatient-monitoring.spec.mjs` | PASS, 5/5 | TASK | dijalankan pada browser sungguhan (Edge) terhadap build produksi
- VALIDATION: e2e regresi `inpatient-episode-detail.spec.mjs`, `inpatient-departure.spec.mjs`, `inpatient-closure.spec.mjs`, `inpatient-census.spec.mjs` | PASS, 40/40 | TASK | detail episode adalah berkas bersama yang ikut diubah pada sesi ini
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-monitoring.test.mjs` | PASS, 24/24 | TASK | mencakup `FE-RWI-016` dan `FE-RWI-017`
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/monitoring` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 244, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND` atas `src/utils/auth/base-login-utils.jsx`; berkas yang ada bernama `base-login-utils.js`. Sudah tercatat pada `FE-RWI-015` dan tidak bersinggungan dengan diff ini
- VALIDATION: `npm run test:unit` | NOT RUN sebagai apa adanya | EXISTING / ENVIRONMENT ISSUE | script-nya memakai `--test tests/unit/` dan Node menolaknya dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Dijalankan lewat bentuk glob
- MANUAL TEST: PASS — seluruh kontrol interaktif yang ditambahkan dijalankan di browser sungguhan (Edge) terhadap build produksi lewat e2e dengan peran berbeda per kasus: keempat tombol tab beserta keadaan terpilihnya, penyaring unit layanan, penyaring jumlah baris, tombol atur ulang penyaring, tautan Detail Episode, tautan Penutupan Episode, dan tombol Pindahkan Pasien. Isi setiap permintaan yang terkirim diperiksa — termasuk `listKey` mana yang dipanggil tiap tab — dan jumlah permintaan selain `GET` diperiksa **nol**
- WARNINGS: **Lama keterlambatan pada dua daftar dihitung memakai jam browser** — bagian 2. Keduanya tidak menggerbang apa pun, sehingga jam yang meleset menghasilkan angka yang salah, bukan keputusan yang salah. **Kolom pelaku penutupan menembus gerbang dapat menyebut orang yang bukan pengambil keputusannya**: `OverrideClosureItemResponse.ClosedByUserId` dibaca dari `InpEpisode.UpdateBy`, dan `BE-RWI-029` bagian 6.1 mencatatnya sebagai keputusan yang belum diambil. Karena itu layar **tidak menampilkan kolom pelaku sama sekali** — menampilkan nama yang mungkin salah pada daftar pengecualian jauh lebih berbahaya daripada tidak menampilkannya. Kolomnya dapat ditambahkan begitu sumbernya diputuskan. **Daftar pantau ketiga `RWI-RULE-023`** — kepatuhan pengkajian dan CPPT — memang belum ada, dan itu di luar scope revisi ini (`DEC-INP-001`)
- KNOWN ISSUES: Menu "Daftar Pantau" tampil bagi semua peran, karena `filter-menu-items-by-role.jsx` tidak menyaring apa pun dan frontend tidak punya data hak akses per butir — batas yang sama yang sudah tercatat pada `FE-RWI-003`. Peran tanpa `InpatientMonitoring : Read` yang membukanya akan melihat layar Akses Ditolak, bukan daftar
- DEPENDENCY BACKEND: `BE-RWI-029` ✅ **Selesai** — keempat endpoint berstatus ✅ `Tersedia`, terbukti berjalan 26 Agustus 2026. `BE-RWI-015` ✅ **Selesai** untuk `/isolation-mismatch`. `FE-RWI-015` ✅ **Selesai** — kriteria 4 task itu kini dapat ditutup penuh, karena daftar pantau penutupan tertunda sudah ada dan episode yang kepergiannya tercatat memang muncul di sana bertanda tempat tidur "Sudah bebas"
- INCIDENTAL CHANGES: Direktori artefak Playwright `test-results/fe-rwi-016-018/` dibuat oleh jalannya e2e lalu dihapus. Config Playwright sementara ditulis di luar repository (direktori scratchpad sesi), sehingga repository tidak pernah memuat berkas config yang harus dibersihkan. Tidak ada perubahan sampingan yang tersisa pada diff
- INTERRUPTIONS: NONE
- GIT STATUS: Pada `QuilvianSystemFrontendDev` branch `HamzahV2`: satu berkas diubah untuk task ini (`menu-items.jsx`) dan tujuh berkas baru, bersama perubahan `FE-RWI-017` dan `FE-RWI-018` yang dikerjakan berurutan pada sesi yang sama. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md`
- NEXT RECOMMENDED STEP: Putuskan sumber kolom pelaku pada daftar penutupan menembus gerbang — `BE-RWI-029` bagian 6.1. Selama belum diputuskan, daftar itu menampilkan alasan supervisor tanpa menyebut siapa yang memutuskannya, dan laporan pengecualian yang tidak menyebut pelakunya hanya separuh berguna
