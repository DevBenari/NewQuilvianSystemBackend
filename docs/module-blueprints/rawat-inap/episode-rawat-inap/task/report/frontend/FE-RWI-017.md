# FE-RWI-017 — Admin dapat menemukan tempat tidur yang statusnya menyimpang

- TASK ID: `FE-RWI-017`
- TASK TYPE: Layar laporan baru — satu halaman baca yang mengonsumsi endpoint selisih yang sudah tersedia
- COMPLEXITY: `LOW`
- CLASSIFICATION SCORE: 6 — dua repository 2; 9–20 berkas diperiksa 1; 1–3 berkas diubah 0; logika ringan 0; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran di layar 1; alur berbatas pada satu halaman 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca, kecuali laporan ini beserta tautan buktinya
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `GET /monitoring/bed-drift` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-017` beserta `BE-RWI-029`; `03-frontend-architecture.md` bagian 2, 4.2, 5.1, dan 5.2; `contracts/api-contract.md` bagian Inpatient Monitoring; `contracts/permission-audit-matrix.md` bagian 2 dan 3; `00-interview-decisions.md` `RWI-DEC-039` dan `RWI-RULE-027`; `InpatientMonitoringController.cs` bagian `GetBedDrift`; `InpatientMonitoringDtos.cs` (`BedDriftItemResponse`); `InpCensusQueryService.cs` bagian `GetBedDriftAsync`; `BedStatus.cs`; `InpatientActorClaims.cs`; [laporan `FE-RWI-016`](FE-RWI-016.md); `inpatient-bed-utils.jsx`; `inpatient-bed-board-constants.jsx`; `inpatient-bed-board.jsx`; `inpatient-episode-utils.jsx` bagian `getEpisodeActorContext`; `inpatient-episode-constants.jsx`; `access-denied-gate.jsx` beserta `access-denied-alert.jsx` dan `access-denied-utils.js`; `data-table.jsx`; `data-filter.jsx`
- FILES CHANGED:
  - **Baru** `src/app/health-services/inpatient-management/bed-drift/page.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-bed-drift-view.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-drift.jsx`
  - **Diubah** `src/utils/menu-sidebar/menu-items.jsx` (menu Selisih Tempat Tidur)
  - Konstanta dan util-nya menumpang berkas `inpatient-monitoring-constants.jsx` dan `inpatient-monitoring-utils.jsx` yang dibuat [`FE-RWI-016`](FE-RWI-016.md), karena keduanya memang satu grup endpoint

## 1. Kriteria 2 adalah inti laporan ini, bukan pelengkapnya

Roadmap menuliskannya dengan tegas: setiap baris menyebut **kedua** nilai yang berselisih, bukan
hanya menyatakan ada selisih. Alasannya praktis — yang membetulkan salinan status perlu tahu nilai
mana yang salah dan nilai mana yang seharusnya. Baris yang hanya berbunyi "ada selisih" tidak dapat
ditindaklanjuti siapa pun.

Karena itu setiap baris membawa empat hal: nilai salinan sekarang, nilai yang seharusnya menurut
catatan penempatan, kalimat yang menyebut keduanya sekaligus, dan **sumber selisihnya** — apakah
ada penempatan aktif, pemesanan aktif, atau tidak keduanya, beserta nomor episode pemegangnya bila
memang ada.

## 2. Angkanya yang diterjemahkan, bukan namanya yang ditebak

`BedDriftItemResponse` mengirim `CopiedStatusName` dan `ExpectedStatusName` sebagai **nama enum
berbahasa Inggris** — `Available`, `Occupied`, `Reserved`. `DriftMessage` dari server juga menyisipkan
nama itu apa adanya ke dalam kalimat Bahasa Indonesia.

Yang membaca laporan ini petugas rumah sakit. Karena itu layar menerjemahkan dari **angkanya**
(`CopiedStatus` dan `ExpectedStatus`, yang bagian dari kontrak) memakai peta yang disalin apa adanya
dari enum `BedStatus`, lalu menyusun kalimatnya sendiri. Nama dari server dipakai hanya sebagai
cadangan bila suatu hari muncul angka yang belum dikenal — lebih baik daripada menampilkan angka
telanjang.

e2e memeriksa seluruh isi halaman dan membuktikan kata `Available` maupun `Occupied` **tidak pernah
sampai ke layar**.

## 3. Kriteria 3 adalah penyempitan layar, bukan penyempitan keamanan — dan itu delta yang perlu dicatat

Roadmap menuntut: hanya admin dan supervisor yang dapat membukanya. Server **tidak dapat**
menegakkannya.

`GET /monitoring/bed-drift` dijaga `InpatientMonitoring : Read` — butir hak akses **yang sama
persis** dengan keempat daftar pantau lainnya. Permission matrix bagian 3 memberikan butir itu juga
kepada **petugas admisi** dan **kepala ruangan**. Artinya mesin hak akses tidak punya cara membuat
laporan ini lebih sempit daripada daftar pantau biasa.

Yang dikerjakan layar:

1. Halaman **tidak dibuka sama sekali** bagi peran selain supervisor dan `SuperAdmin` — bukan dibuka
   lalu kosong, sesuai bagian 5.1.
2. Permintaannya **tidak pernah dikirim** untuk peran itu. Layar yang tetap memanggil lalu
   menampilkan 403 justru memperlihatkan bahwa laporannya ada dan berisi.
3. Daftar nama perannya disalin apa adanya dari `InpatientActorClaims.SupervisorRoles`, dan dibaca
   lewat `getEpisodeActorContext` yang sudah dipakai seluruh layar Rawat Inap — bukan lewat
   pembacaan klaim peran kedua di tempat lain.

**Yang perlu diputuskan pemilik kontrak:** apakah laporan selisih perlu butir hak akses tersendiri,
misalnya `InpatientMonitoring : ReadBedDrift`. Selama belum ada, penyempitan di sini adalah
keterbacaan layar dan **bukan** batas keamanan — petugas admisi yang memanggil endpoint-nya langsung
tetap dilayani server. Owner: Backend/API bersama Security/Privacy.

**"Admin" dipetakan ke `SuperAdmin`.** Permission matrix menyebut peran "Admin master data" hanya
dengan `InpatientSetting` dan `InpatientClearanceItem`, tanpa `InpatientMonitoring : Read` — sehingga
peran itu justru **ditolak server** bila dipakai membuka laporan ini. Satu-satunya nama peran
bertaraf admin yang dikenali kontrak backend adalah `SuperAdmin`, dan itulah yang dipakai. Bila
rumah sakit menghendaki admin master data membacanya, butir hak aksesnya perlu ditambahkan lebih
dulu.

## 4. Yang tidak dihitung sebagai selisih, dinyatakan di layar

Empat keadaan wewenang admin — pembersihan, perbaikan, diblokir, nonaktif — tidak pernah dihitung
server sebagai selisih, karena modul Rawat Inap memang tidak menuliskannya. Layar menyatakannya apa
adanya, supaya pembacanya tidak mengira laporan yang pendek berarti laporannya rusak.

Peringatan bahwa laporan ini **perlu dibaca berkala** juga dipasang di layar. `BE-RWI-029` bagian 2
menyebut ini soal proses, bukan kode: bila tidak pernah dibuka siapa pun, salinan status akan
menyimpang diam-diam sampai seorang pasien ditempatkan di tempat tidur yang sudah ada orangnya.
Layar tidak dapat menunjuk penanggung jawab, tetapi dapat memastikan siapa pun yang membukanya tahu
kenapa halaman ini ada.

- IMPLEMENTATION: (1) Halaman tersendiri `/health-services/inpatient-management/bed-drift`, bukan tab kelima pada halaman daftar pantau, karena cakupan pembacanya memang lebih sempit — menjadikannya tab akan membuat penjaga perannya menutup keempat daftar lain sekaligus. (2) State-nya disimpan di dalam hook, bukan pada slot Redux `monitoring`, supaya laporan ini dan halaman daftar pantau tidak saling menimpa ketika dibuka bergantian. (3) Permintaan tidak pernah dikirim untuk peran yang tidak berhak — penjaganya ada pada `useEffect`, bukan hanya pada render. (4) Memakai `AccessDeniedAlert` yang sudah ada untuk penolakan di layar, dan `AccessDeniedGate` yang sudah ada untuk 403 dari server; keduanya komponen yang sama yang dipakai seluruh aplikasi. (5) Penyaring unit layanan, jumlah baris, tabel, dan paginasi memakai `DataFilter`, `ResourceFilterSelect`, `FilterSelect`, `DataTable`, dan `RegionPagination` yang sudah ada. (6) Tautan menuju Papan Tempat Tidur dipasang pada bar penyaring, karena itu layar tempat salinan statusnya benar-benar dibaca petugas
- API CONTRACT IMPACT: Tidak mengubah kontrak. Query memakai nama kolom `InpatientMonitoringQuery` apa adanya (`serviceUnitId`, `pageNumber`, `pageSize`)
- SECURITY IMPACT: Layar menyempitkan pembaca laporan menjadi supervisor dan `SuperAdmin`, **lebih sempit daripada yang dapat ditegakkan server** — bagian 3. Ini keterbacaan, bukan batas keamanan; delta-nya dicatat untuk diputuskan pemilik kontrak. Tidak ada kolom sensitif yang dibaca: `BedDriftItemResponse` hanya memuat identitas tempat tidur, lokasi, kedua nilai status, dan nomor episode pemegangnya — tanpa nama pasien
- DATABASE IMPACT: Tidak ada
- VISUAL REFERENCE: NOT REQUIRED
- WEWENANG UI YANG DIPAKAI: "Bebas". Dipilih halaman tersendiri dengan alasan pada bagian 3, memakai pola daftar bertingkat yang sama dengan census dan daftar pantau. Menu Rawat Inap ditambah satu butir "Selisih Tempat Tidur", diletakkan tepat sesudah Daftar Pantau karena keduanya satu grup endpoint. Tidak ada komponen baru, tidak ada arsitektur state baru

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Menampilkan tempat tidur yang salinan statusnya tidak cocok dengan catatan penempatan | **LULUS** | e2e memakai **dua** selisih yang dibuat sengaja dengan arah berlawanan — persis seperti test `BE-RWI-029` kriteria 5 memutar terbalik salinan status dua tempat tidur pada database uji: `BD-001` benar-benar dihuni tetapi salinannya berbunyi dapat dipakai, `BD-002` tidak dihuni siapa pun tetapi salinannya berbunyi terisi. Keduanya tampil beserta lokasinya |
| 2. Setiap baris menyebut kedua nilai yang berselisih, bukan hanya menyatakan ada selisih | **LULUS** | e2e membaca kolom salinan dan kolom seharusnya secara terpisah untuk kedua baris, lalu membaca kalimat "Salinan status berbunyi Dapat dipakai, sedangkan catatan penempatan menunjukkan Terisi." apa adanya, beserta sumber selisihnya — "penempatan aktif milik episode RWI-2026-000001" untuk baris pertama dan "Tidak ada penempatan maupun pemesanan aktif" untuk baris kedua. Seluruh isi halaman diperiksa dan nama enum `Available` maupun `Occupied` **tidak pernah muncul**. Test unit menutup ketujuh nilai `BedStatus`, angka yang belum dikenal, dan baris yang berselisih karena pemesanan |
| 3. Hanya admin dan supervisor yang dapat membukanya | **LULUS di layar; TIDAK DAPAT ditegakkan server** | e2e per peran: supervisor dan `SuperAdmin` melihat laporannya; petugas admisi, perawat, kepala ruangan, dan kasir melihat layar Akses Ditolak, baris laporannya `toHaveCount(0)`, dan **nol** permintaan terkirim. Test unit menutup keenam peran. **Batas buktinya:** server menjaga endpoint ini dengan `InpatientMonitoring : Read` yang juga dimiliki petugas admisi dan kepala ruangan, sehingga penyempitan ini hanya berlaku di layar — bagian 3 |
| 4. Daftar kosong menampilkan keadaan kosong yang jelas | **LULUS** | e2e membuka laporan dengan jawaban kosong dan membaca "Tidak ada tempat tidur yang statusnya menyimpang." beserta kalimat yang menjelaskan artinya; layar Akses Ditolak dan tombol Coba Lagi sama-sama `toHaveCount(0)`. Test unit membuktikan penormalnya mengembalikan daftar kosong untuk payload kosong dan untuk `null`, bukan melempar |

- VALIDATION: e2e `tests/e2e/inpatient-bed-drift.spec.mjs` | PASS, 8/8 | TASK | dijalankan pada browser sungguhan (Edge) terhadap build produksi; termasuk enam e2e per peran
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-monitoring.test.mjs` | PASS, 24/24 | TASK | berkas yang sama menutup `FE-RWI-016` dan `FE-RWI-017`
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/bed-drift` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 244, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND`; sudah tercatat pada `FE-RWI-015` dan tidak bersinggungan dengan diff ini
- MANUAL TEST: PASS — seluruh kontrol interaktif yang ditambahkan dijalankan di browser sungguhan (Edge) terhadap build produksi lewat e2e dengan enam peran berbeda: penyaring unit layanan, penyaring jumlah baris, tombol atur ulang penyaring, dan tautan Papan Tempat Tidur. Jumlah permintaan pada jalur yang seharusnya menolak diperiksa **nol**, dan seluruh teks halaman diperiksa untuk memastikan nama enum berbahasa Inggris tidak bocor ke layar
- WARNINGS: **Penyempitan peran pada layar ini tidak dapat ditegakkan server** — bagian 3; perlu keputusan apakah laporan selisih memerlukan butir hak akses tersendiri. **Laporan ini tidak berguna bila tidak dibaca siapa pun.** `BE-RWI-029` bagian 2 dan roadmap sama-sama menyebutnya soal proses: perlu ditetapkan siapa yang membacanya dan seberapa sering. Layar sudah menyatakan kenapa halaman ini ada, tetapi tidak dapat menunjuk penanggung jawabnya. Owner: Product/Domain
- KNOWN ISSUES: Laporan dihitung server di memori atas seluruh tempat tidur yang tidak terhapus — `BE-RWI-029` bagian 6.2. Pada skala ratusan sampai ribuan tempat tidur beban itu wajar; penyaring unit layanan pada layar ini adalah cara membatasi cakupannya bila kelak jumlahnya jauh lebih besar. Menu "Selisih Tempat Tidur" tampil bagi semua peran karena penyaringan menu per peran memang belum ada — batas yang sama yang tercatat pada `FE-RWI-003`; peran yang membukanya langsung melihat layar Akses Ditolak
- DEPENDENCY BACKEND: `BE-RWI-029` ✅ **Selesai** — `GET /monitoring/bed-drift` berstatus ✅ `Tersedia`, terbukti berjalan 26 Agustus 2026. `FE-RWI-015` ✅ **Selesai**
- INCIDENTAL CHANGES: Direktori artefak Playwright `test-results/fe-rwi-016-018/` dibuat oleh jalannya e2e lalu dihapus. Config Playwright sementara ditulis di luar repository. Tidak ada perubahan sampingan yang tersisa pada diff
- INTERRUPTIONS: NONE
- GIT STATUS: Pada `QuilvianSystemFrontendDev` branch `HamzahV2`: satu berkas diubah untuk task ini (`menu-items.jsx`, bersama `FE-RWI-016`) dan tiga berkas baru, bersama perubahan `FE-RWI-016` dan `FE-RWI-018` yang dikerjakan berurutan pada sesi yang sama. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya
- NEXT RECOMMENDED STEP: Tetapkan siapa yang membaca laporan ini dan seberapa sering, lalu putuskan apakah ia memerlukan butir hak akses tersendiri. Keduanya keputusan yang sama pentingnya: tanpa yang pertama laporan ini tidak menutup risiko apa pun, dan tanpa yang kedua batas pembacanya hanya berlaku di layar
