# FE-RWI-008 — Perawat tahu siapa dirawat, di mana, dan sudah berapa hari

- TASK ID: `FE-RWI-008`
- TASK TYPE: Implementasi layar daftar frontend
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; enam berkas source diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh aturan privasi tanpa mengubah otorisasi 1; satu layar berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `GET /census` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-008`; `03-frontend-architecture.md` bagian 4.3, 5.1, 5.2, dan 6; `contracts/api-contract.md` bagian Census; `InpatientCensusController.cs`; `InpatientCensusDtos.cs` (`CensusQuery`, `CensusItemResponse`, `CensusPagedResult`); fondasi `FE-RWI-002` (`inpatient-api.service.js`, `inpatient-census.service.js`, `inpatient-management-slice.jsx`); pola daftar terdekat `use-inpatient-clearance-items.jsx` beserta view-nya; `data-filter.jsx`, `data-table.jsx`, `resource-filter-select.jsx`, `filter-select.jsx`
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-census-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-census-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-census.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-census-view.jsx` (baru); `src/app/health-services/inpatient-management/census/page.jsx` (baru); `src/utils/menu-sidebar/menu-items.jsx` (diubah — satu butir menu); `tests/unit/inpatient-census.test.mjs` (baru); `tests/e2e/inpatient-census.spec.mjs` (baru)

## Yang membuat kriteria 2 dapat dibuktikan, bukan sekadar diklaim

Roadmap menaruh kriteria 2 pada **payload**, bukan pada tampilan, karena data sensitif yang
sudah terkirim ke browser tetap bocor walaupun layar menyembunyikannya. Karena itu dipasang
dua lapis, dan keduanya diuji:

| Lapis | Isinya | Bukti |
| --- | --- | --- |
| Kolom yang diizinkan | `CENSUS_ALLOWED_FIELDS` menyalin kolom `CensusItemResponse` apa adanya. `normalizeCensusItem` menjatuhkan apa pun di luar daftar itu, sehingga kolom klinis yang suatu hari ikut terbawa jawaban server tidak pernah masuk pohon React | Test unit menyuntikkan `diagnosis`, `clinicalSummary`, `isolationNote`, dan `notes` pada satu baris, lalu membuktikan keempatnya hilang sesudah dinormalkan |
| Pemeriksaan payload | `findForbiddenCensusFields` menyebutkan kolom terlarang yang benar-benar ada pada jawaban server | e2e menangkap **body jawaban `GET /census` yang sampai ke browser** lalu memastikan daftarnya kosong, ditambah pemeriksaan isi halaman terhadap ketiga nama kolom |

Bahwa `CensusItemResponse` memang tidak memuat diagnosis maupun isi resume sudah dibaca
langsung dari DTO backend. Lapis pertama karena itu bukan penjaga terhadap keadaan hari ini,
melainkan terhadap perubahan di kemudian hari.

- IMPLEMENTATION: (1) `useInpatientCensus` membaca `GET /census` lewat fondasi `FE-RWI-002` dan menyimpannya pada resource `census` milik slice Rawat Inap — tanpa jalur HTTP maupun arsitektur state baru. (2) Daftar disusun **hanya** dari jawaban census; tidak ada penggabungan dengan daftar episode maupun papan tempat tidur. (3) Penyaring unit layanan dan kelas perawatan memakai `ResourceFilterSelect` yang sudah ada; setiap penyaringan mengembalikan pembacaan ke halaman pertama. (4) Angka hari rawat selalu ditulis beserta satuannya sendiri dan disertai keterangan cara hitungnya. (5) Census dibaca ulang ketika layar difokuskan kembali, mengikuti `03-frontend-architecture.md` bagian 5.2. (6) Baris census menautkan ke layar detail episode `FE-RWI-009`.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Mengonsumsi `GET /v1/health-services/inpatient-management/census` beserta `CensusQuery` apa adanya.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Menegakkan aturan privasi bagian 6 di sisi konsumen.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: `RWI-FE-001` `DEV_DISCRETION` untuk kata angka hari rawat. Dipilih **"N hari rawat"** pada setiap baris, ditambah keterangan tetap di atas daftar: "Hari rawat dihitung dari selisih tanggal, bukan dari lama waktu sebenarnya. Pasien yang masuk tadi malam dan dibaca pagi ini sudah terhitung 1 hari rawat." Batas yang mengikat — angka wajib terbaca sebagai hitungan hari rawat — dipenuhi dengan menyebut satuannya penuh, dan test membuktikan kata "jam" tidak pernah muncul pada kolom itu. Nama menu, route, dan tata letak mengikuti konvensi `src/app/health-services/` yang sudah ada.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Census menampilkan nomor episode, nama pasien, lokasi, DPJP, perawat, lama dirawat, dan status | **LULUS** | e2e memeriksa ketujuhnya di browser sungguhan: `RWI-2026-000001`, `Ny. Sari Melati`, `Bed 1`, `Melati 1 — Rawat Inap Melati`, `dr. Andi Pratama`, `Ns. Dewi Anggraini`, `1 hari rawat`, dan `Sedang dirawat`. Episode tanpa perawat terbaca "Belum ditugaskan", bukan sel kosong |
| 2. Tanpa diagnosis dan tanpa isi resume | **LULUS** | **Pemeriksaan payload:** e2e menangkap body jawaban `GET /census` yang diterima browser dan memastikan tidak satu pun dari sembilan kolom terlarang ada di sana, lalu memeriksa isi halaman terhadap `isolationNote`, `diagnosis`, dan `clinicalSummary`. Test unit membuktikan keempat kolom klinis yang sengaja disuntikkan dijatuhkan sebelum data sampai ke component |
| 3. Angka hari rawat tidak dapat disalahartikan sebagai jam | **LULUS** | e2e membaca seluruh sel hari rawat dan memastikan tidak satu pun memuat kata "jam"; nilainya terbaca `1 hari rawat` dan `12 hari rawat`. Test unit menutup jalur nol dan jalur besar |
| 4. Dapat disaring unit layanan dan kelas | **LULUS** | e2e: menyaring unit Anggrek menyisakan satu pasien dan mengirim `serviceUnitId` yang benar dengan `pageNumber=1`; menambah kelas 1 di atasnya menghasilkan daftar kosong beserta kalimat "Belum ada pasien yang dirawat di unit ini."; tombol atur ulang mengembalikan kedua penyaring sekaligus dan permintaannya kembali tanpa kedua kolom itu |
| 5. Pasien yang kepergiannya sudah dicatat tidak muncul | **LULUS** | Pasien itu tidak lagi memegang baris penempatan aktif, sehingga server tidak memasukkannya ke census. Yang dapat dijamin layar adalah tidak adanya sumber kedua: test unit membuktikan hook hanya memakai `inpatientCensusService` — bukan `inpatientEpisodeService` maupun `bedOccupancyService` — dan e2e memastikan nama pasien yang sudah pulang tidak muncul di layar |

- VALIDATION: e2e `tests/e2e/inpatient-census.spec.mjs` | PASS, 3/3 | TASK | kolom wajib beserta satuan hari rawat, pemeriksaan payload, dan penyaring gabungan beserta atur ulang
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-census.test.mjs` | PASS, 8/8 | TASK | kolom wajib, penjatuhan kolom klinis, pemeriksaan payload, satuan hari rawat, penyaring dan halaman pertama, sumber tunggal, layar tanpa keterangan isolasi, dan pembacaan ulang saat difokuskan
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/census` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 106, FAIL 1 | EXISTING ISSUE | satu-satunya kegagalan adalah `tests/unit/auth-security.test.mjs` yang mengimpor `src/utils/auth/base-login-utils.jsx` sedangkan berkasnya berekstensi `.js` — berkas yang sama sudah tercatat rusak pada laporan `FE-RWI-006` dan `FE-RWI-007`, dan tidak disentuh task ini
- VALIDATION: `npm run test:unit` | FAIL | EXISTING ISSUE | `ERR_UNSUPPORTED_DIR_IMPORT` pada `tests/unit` di Node 24.13.0; berkas dijalankan lewat pola `tests/unit/*.test.mjs` sebagai gantinya. Bukan akibat perubahan task ini — `package.json` dan loader test tidak disentuh
- MANUAL TEST: NOT FEASIBLE — memerlukan akun berhak `InpatientCensus : Read` beserta pasien yang benar-benar sedang menempati tempat tidur pada database tim, dan tidak ada connection string lokal untuk itu. Seluruh kontrol layar — kotak pencarian, kedua penyaring sumber daya, pemilih jumlah baris, tombol atur ulang, penomoran halaman, dan tautan detail — dijalankan di browser sungguhan (Edge) lewat e2e dengan API tiruan, termasuk pemeriksaan payload yang tidak dapat dikerjakan dengan melihat layar saja
- WARNINGS: Kriteria 5 sepenuhnya bergantung pada server: census dihitung dari baris penempatan aktif, dan layar tidak punya cara memeriksa ulang tanpa menyalin aturan itu — yang justru dilarang. Yang dijamin layar adalah tidak adanya sumber kedua yang dapat memunculkan kembali pasien yang sudah pulang
- KNOWN ISSUES: Tidak ada cacat implementasi yang diketahui pada scope task
- DEPENDENCY BACKEND: `BE-RWI-016` — `GET /census` berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026. Task backend itu masih 🟡 karena sebagian kriterianya baru terbukti di tingkat service, bukan karena bentuk balasan yang dikonsumsi layar ini
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan; folder keluaran Playwright untuk test auth yang gagal dihapus
- INTERRUPTIONS: NONE
- GIT STATUS: Berkas baru pada constants, utils, hook, view, route, dan test, ditambah satu butir menu pada `menu-items.jsx`. **Belum di-stage dan belum di-commit**
- NEXT RECOMMENDED STEP: Jalankan `FE-RWI-016` daftar pantau memakai pola daftar yang sama, dan pakai ulang `CENSUS_ALLOWED_FIELDS` sebagai contoh ketika layar daftar Rawat Inap berikutnya dibuat
