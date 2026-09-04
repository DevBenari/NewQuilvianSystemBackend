# FE-RWI-012 — DPJP dapat menyatakan pasien boleh pulang dan menandatangani resume

- TASK ID: `FE-RWI-012`
- TASK TYPE: Implementasi layar frontend baru beserta aturan tampil per peran
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 10 — dua repository 2; lebih dari 20 berkas diperiksa 2; lebih dari 8 berkas diubah 2; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; alur berbatas pada satu layar 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /discharges/{episodeId}/decide`, `GET /discharges/{episodeId}/summary`, `PUT /discharges/{episodeId}/summary`, dan `PATCH /discharges/{episodeId}/summary/sign` keempatnya berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-011` dan `FE-RWI-012`; `03-frontend-architecture.md` bagian 2, 3, 5.3, 5.4, dan 6; `contracts/api-contract.md` bagian Inpatient Discharge; `contracts/validation-matrix.md` bagian 6; `data/data-dictionary.md` bagian 15; `00-interview-decisions.md` bagian cara pulang dan `RWI-DEC-059`; `04-prd-to-mvp.md` baris cara pulang meninggal dan kabur; [laporan `BE-RWI-020`](../backend/be-rwi-020-keputusan-pasien-boleh-pulang.md) bagian 5.1; `InpatientDischargeController.cs`; `InpatientDischargeDtos.cs`; `InpDischargeType.cs`; `InpDischargeService.cs` baris 95–540 tempat `GUARD-INP-02`, `GUARD-INP-03`, dan jalur amandemen dibentuk; `InpatientEpisodeDtos.cs` bagian `InpatientEpisodeDetailResponse`; `AGENTS.md` frontend beserta enam dokumen `.codex`; `inpatient-api.service.js`; `inpatient-discharge.service.js`; `inpatient-management-slice.jsx`; `use-inpatient-episode-detail.jsx`; `inpatient-episode-detail-view.jsx`; `inpatient-episode-constants.jsx`; `inpatient-episode-utils.jsx`; `inpatient-census-view.jsx`; `inpatient-census-constants.jsx`; `inpatient-census-utils.jsx`; `base-form-control.jsx`; `filter-select.jsx`; `confirm-modal.jsx`; `status-badge.jsx`; `access-denied-gate.jsx`; `tests/e2e/inpatient-episode-detail.spec.mjs`; `tests/e2e/inpatient-census.spec.mjs`; `tests/unit/inpatient-episode-detail.test.mjs`
- FILES CHANGED:
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-discharge-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-discharge-utils.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-discharge.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-discharge-view.jsx`
  - **Baru** `src/app/health-services/inpatient-management/episodes/[id]/discharge/page.jsx`
  - **Baru** `tests/unit/inpatient-discharge.test.mjs`
  - **Baru** `tests/e2e/inpatient-discharge.spec.mjs`
  - **Diubah** `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (route builder layar pulang)
  - **Diubah** `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (`dischargeType` dan `dischargeTypeName` ikut dinormalisasi)
  - **Diubah** `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (satu tautan menuju layar pulang)
  - **Diubah** `src/lib/constants/health-services/inpatient-management/inpatient-census-constants.jsx` (`referralDestination` ikut dilarang)
  - **Diubah** `tests/e2e/inpatient-census.spec.mjs` (salinan mandiri daftar larangan ikut disesuaikan)

## 1. Layar berdiri sendiri, bukan menempel pada detail episode

`FE-RWI-010` dan `FE-RWI-011` keduanya menempel pada layar detail episode, sehingga menempelkan
`FE-INP-06` di sana adalah pilihan yang paling mudah dibela. Yang membuatnya **tidak** dipilih
bukan panjang berkas, melainkan satu akibat teknis: isi resume dijaga hak akses `InpatientDischarge
: Read` yang **terpisah** dari hak akses membaca episode. Bila `GET /discharges/{id}/summary`
menempel pada pemuatan detail episode, satu jawaban 403 dari endpoint resume akan membuat
`AccessDeniedGate` menutup **seluruh** layar detail — termasuk lokasi terkini, riwayat penempatan,
dan aksi perpindahan yang sama sekali tidak bersangkutan.

Karena itu layar pulang berdiri sebagai route sendiri di
`/health-services/inpatient-management/episodes/{id}/discharge`, dijangkau lewat satu tautan pada
ringkasan detail episode. `03-frontend-architecture.md` bagian 2 memang membebaskan pelaksana
menamai ulang dan menggabungkan layar selama kemampuannya tercapai.

## 2. Cara pulang: tiga tersedia, dua disebut apa adanya

Roadmap acceptance criteria 2 menyebut **lima** cara pulang. Enum `InpDischargeType` menyediakan
**tiga**; nomor 4 dan 5 sengaja dikosongkan `BE-RWI-003` untuk meninggal dan kabur, dan backend
menolak keduanya dengan 422 *"Cara pulang yang dipilih belum tersedia pada versi ini."*

Delta ini **sudah tercatat** pada [laporan `BE-RWI-020`](../backend/be-rwi-020-keputusan-pasien-boleh-pulang.md)
bagian 5.1 dan pada `requirement-traceability.md` sebagai `RWI-OQ-039` dengan `RWI-DEC-059`
berstatus `draft`. Frontend **tidak membuka butir baru** dan tidak mengarang nomor enum sendiri:
mengirim nilai 4 atau 5 hanya akan menghasilkan 422.

Yang dikerjakan layar adalah menyebut keduanya apa adanya. Menyembunyikannya sama sekali akan
membuat petugas yang menghadapi pasien meninggal mengira kasusnya tidak terpikirkan, lalu memilih
cara pulang lain yang salah supaya pekerjaannya bisa lanjut — persis kesalahan data yang paling
mahal diperbaiki. Kalimat jalan keluarnya disalin dari `04-prd-to-mvp.md`: kedua kasus itu ditutup
lewat jalan keluar supervisor disertai alasan tertulis dan tercatat pada laporan pengecualian.

**Kriteria 2 karena itu terpenuhi sebagian**, dan bagian yang belum terpenuhi bukan pekerjaan
frontend. Rinciannya ada pada tabel acceptance criteria di bawah.

## 3. Dialog konfirmasi bukan tempat memeriksa isian

Cacat ini ditemukan e2e, bukan dibaca ulang dari kode. Versi pertama membuka dialog konfirmasi
lebih dulu lalu memeriksa isian ketika pengguna menekan Ya. Akibatnya, menekan **Nyatakan Boleh
Pulang** tanpa memilih cara pulang memunculkan dialog yang berbunyi *"…dengan cara pulang **yang
dipilih**"* padahal belum ada satu pun yang dipilih — pertanyaan yang menyesatkan, dan penolakannya
baru muncul satu klik kemudian.

Pemeriksaannya dipindahkan ke depan dialog, dan **tetap dijalankan lagi** tepat sebelum permintaan
dikirim supaya dialog bukan satu-satunya penjaga. Hal yang sama berlaku pada tombol tanda tangan.

- IMPLEMENTATION: (1) Keputusan pulang dan tanda tangan **tidak dirender** bagi siapa pun selain DPJP aktif episode itu — termasuk supervisor dan kepala ruangan yang pada aksi lain justru berwenang, karena `GUARD-INP-02` dan `GUARD-INP-03` menjaga hubungan seorang dokter dengan seorang pasien, bukan peran. (2) Cara pulang memakai `BaseSelectField` yang menambahkan baris kosong sendiri, sehingga keadaan awalnya `""` dan tidak ada nilai bawaan yang dapat tersimpan diam-diam. (3) Resume hanya dapat disusun setelah keputusan pulang dibuat, setara penolakan service *"Resume pulang hanya dapat disusun setelah DPJP menyatakan pasien boleh pulang."* (4) Resume yang sudah ditandatangani berubah menjadi tampilan baca saja: tidak ada isian, tidak ada tombol simpan, dan tidak ada tanda tangan ulang. (5) Tombol tanda tangan tertahan selama masih ada perubahan yang belum tersimpan, karena tanda tangan membubuhi isi yang tersimpan di server — bukan isian yang sedang diketik. (6) Tujuan rujukan diperiksa terhadap resume **tersimpan**, sama seperti server. (7) Penjaga `decisionInFlight`, `summaryInFlight`, dan `signatureInFlight` menahan klik kedua. (8) Riwayat versi selalu diminta lewat `includeRevisions=true`; tanpa itu jawaban server memuat daftar kosong dan layar akan menyimpulkan koreksi resume tidak pernah terjadi.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Muatan memakai nama kolom `DecideDischargeRequest` (`dischargeType`, `reason`), `UpsertDischargeSummaryRequest` (ketujuh kolom isi resume), dan `SignDischargeSummaryRequest` (`note`) apa adanya. `PATCH /summary/sign` tidak pernah dikirimi identifier dokter — penandatangan diturunkan server dari DPJP aktif.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization. Menyembunyikan aksi yang pasti ditolak `GUARD-INP-02` dan `GUARD-INP-03`; server tetap satu-satunya penentu. Kewenangan **membaca** resume sengaja tidak ditebak layar — `GET /summary` dibiarkan dijawab server, dan 403 ditangani `AccessDeniedGate` seperti layar lain. `referralDestination` ditambahkan ke `CENSUS_FORBIDDEN_FIELDS` sehingga penjaga payload census kini mencakup ketujuh kolom isi resume, bukan enam.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Bebas". Dipilih satu layar tersendiri berisi lima bagian berurutan mengikuti alur kerjanya — ringkasan episode, keputusan pulang, resume, tanda tangan, lalu versi resume — memakai `Hero`, `InformationAlert`, `StatusBadge`, `BaseSelectField`, `BaseTextField`, `BaseTextAreaField`, `BaseButton`, `ConfirmModal`, dan `ToastStack` yang sudah ada. Tidak ada komponen baru, tidak ada pola HTTP baru, dan tidak ada arsitektur state baru.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Aksi menyatakan boleh pulang dan menandatangani resume hanya tampil bagi DPJP aktif | **LULUS** | e2e menutup lima peran — perawat, petugas admisi, kepala ruangan, supervisor, dan dokter yang bukan DPJP episode itu. Pada kelimanya, bagian keputusan, isian resume, bagian tanda tangan, dan ketiga tombolnya `toHaveCount(0)`: tidak dirender, bukan dirender lalu dinonaktifkan. e2e dengan DPJP aktif membuktikan aksinya tampil. Test unit memeriksa keenam peran yang sama pada kedua penjaga kewenangan |
| 2. Lima cara pulang tersedia dan dipilih sadar, bukan ada nilai bawaan yang tersimpan diam-diam | **SEBAGIAN — bagian "dipilih sadar" LULUS; bagian "lima" TERBLOKIR di luar frontend** | **Dipilih sadar:** e2e membuka daftar dan menemukan tepat satu baris kosong bawaan komponen ditambah tiga cara pulang, baris kosong `aria-selected="true"` dan ketiganya `false`; menekan tombol tanpa memilih menampilkan "Cara pulang wajib dipilih.", dialog konfirmasi **tidak** terbuka, dan **nol** permintaan terkirim; sesudah dipilih sadar, tepat satu `POST` terkirim dengan `dischargeType: 3`. Test unit membuktikan keadaan awalnya `""` dan daftar pilihannya tidak memuat `Unknown` maupun penanda terpilih bawaan. **Lima:** enum `InpDischargeType` menyediakan tiga; nomor 4 dan 5 sengaja dikosongkan dan ditolak 422. Terblokir pada `RWI-OQ-039` dan `RWI-DEC-059` yang berstatus `draft` — butir yang **sudah** dicatat `BE-RWI-020`, bukan temuan baru. Layar menyebut keduanya beserta jalan keluar supervisor, sesuai `04-prd-to-mvp.md` |
| 3. Resume yang sudah ditandatangani tidak dapat disunting dari layar ini | **LULUS** | e2e dengan DPJP aktif sendiri pada resume tertandatangani: isian resume, tombol simpan, bagian tanda tangan, dan tombol tanda tangan semuanya `toHaveCount(0)`; keterangan penguncian tampil apa adanya seperti pesan 409 service; isinya tetap terbaca; **nol** permintaan `PUT` maupun `PATCH` terkirim sepanjang test. Test unit membuktikan `canEdit` dan `canSign` keduanya `false` bahkan bagi DPJP aktif, dan memeriksa urutan cabang render pada source view |
| 4. Daftar versi resume terbaca beserta nama penandatangan tiap versi | **LULUS** | e2e mengirim dua versi **terbalik**; layar menampilkannya urut nomor versi, masing-masing dengan nama penandatangannya sendiri — versi 1 "dr. Andi Pratama", versi 2 "dr. Budi Santoso" — dan isi versi lama tetap terbaca utuh sementara isi yang berlaku sekarang tetap yang terbaru. e2e juga memeriksa permintaannya benar-benar membawa `includeRevisions=true`. Test unit memeriksa pengurutan, nama penandatangan per versi, dan penamaan cara pulang lama |
| 5. Isi resume **tidak** tampil pada daftar episode maupun census | **LULUS** | Ketujuh kolom isi resume tidak disentuh sama sekali oleh `inpatient-census-view`, `use-inpatient-census`, `inpatient-bed-board`, `use-inpatient-bed-board`, maupun `inpatient-episode-detail-view`, dan tidak satu pun memanggil `inpatient-discharge.service`. `normalizeEpisodeDetail` diuji dengan payload yang **sengaja menyelipkan** `primaryDiagnosisText` dan `clinicalSummary`: keduanya tidak ikut terbaca. Penjaga payload census kini mencakup ketujuh kolom — `referralDestination` yang semula terlewat ikut ditambahkan — dan `findForbiddenCensusFields` terbukti menangkapnya pada jawaban tiruan. e2e census `pemeriksaan payload` lulus dengan daftar larangan yang sudah diperluas |

- VALIDATION: e2e `tests/e2e/inpatient-discharge.spec.mjs` | PASS, 11/11 | TASK | termasuk lima e2e per peran, e2e cara pulang tanpa nilai bawaan, e2e resume terkunci, dan e2e versi resume
- VALIDATION: e2e `tests/e2e/inpatient-census.spec.mjs` dan `tests/e2e/inpatient-episode-detail.spec.mjs` | PASS, 13/13 | TASK | regresi berkas bersama yang ikut diubah
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-discharge.test.mjs` | PASS, 27/27 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/episodes/[id]/discharge` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 132, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND` atas `src/utils/auth/base-login-utils.jsx` yang memang tidak ada. Tidak bersinggungan dengan diff ini, dan sudah tercatat pada laporan `FE-RWI-011`
- VALIDATION: `npx playwright test` seluruh spec | PASS 39, FAIL 3 | EXISTING ISSUE | dua kegagalan `auth-security.spec.mjs` menyangkut pengalihan `/login` dan penghitungan permintaan login; satu kegagalan `route-smoke.spec.mjs` adalah `expect(applicationRoutes).toHaveLength(219)` yang kini membaca **473**. Selisih 254 route jauh melampaui satu route yang ditambahkan task ini, sehingga angka 219 memang sudah usang sejak sebelumnya. Ketiganya di luar modul Rawat Inap
- VALIDATION: `npm run test:unit` | NOT RUN sebagai apa adanya | EXISTING / ENVIRONMENT ISSUE | script-nya memakai `--test tests/unit/` dan Node 24.13.0 menolaknya dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Dijalankan lewat bentuk glob seperti pada `FE-RWI-011`
- MANUAL TEST: NOT FEASIBLE — memerlukan akun dokter yang benar-benar terdaftar sebagai DPJP aktif sebuah episode pada database tim, ditambah episode berstatus `Admitted` dan episode lain yang resumenya sudah ditandatangani beserta versi koreksinya. Tidak tersedia tanpa database itu. Sebagai gantinya, seluruh kontrol — isian pilihan cara pulang beserta daftar opsinya, ketujuh isian resume, kedua dialog konfirmasi, dan ketiga tombol — dijalankan di browser sungguhan (Edge) lewat e2e dengan peran yang berbeda per kasus, dan setiap permintaan yang terkirim diperiksa isinya
- WARNINGS: **Kriteria 2 belum dapat dipenuhi seluruhnya, dan penyebabnya di luar frontend.** Selama `RWI-DEC-059` belum naik ke `approved`, pasien yang meninggal atau kabur tidak dapat dicatat cara pulangnya sama sekali — di backend maupun di layar. Yang dilakukan layar hanya menyebut keadaan itu dan menunjukkan jalan keluar supervisor. **Kewenangan membaca isi resume tidak diperiksa layar.** Layar mengandalkan server menolak `GET /summary` dengan 403 bagi peran yang tidak punya `InpatientDischarge : Read`. Bila hak akses itu ternyata terlalu longgar di backend, isi resume akan terbaca peran yang tidak seharusnya — dan perbaikannya wajib dikerjakan di backend, bukan ditambal dengan penyembunyian di layar
- KNOWN ISSUES: Amandemen resume tertandatangani — jalur supervisor di dalam sesi koreksi — **tidak** dikerjakan di sini; itu scope `FE-RWI-018`. Layar ini hanya membaca hasilnya sebagai daftar versi. Akibatnya, DPJP yang salah mengetik lalu menandatangani tidak punya jalan perbaikan dari layar ini sampai `FE-RWI-018` tersedia; ini sesuai kriteria 3, dan risiko alur kerjanya sudah dicatat `BE-RWI-022` bagian 7.1
- DEPENDENCY BACKEND: `BE-RWI-020` 🟡, `BE-RWI-021` ✅, dan `BE-RWI-022` ✅ — keempat endpoint yang dipakai berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026. `BE-RWI-020` masih 🟡 karena butir lima cara pulang yang sama, bukan karena bentuk balasan yang dikonsumsi layar ini
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan; satu folder artefak kegagalan `test-results/auth-security-double-submi-…` yang dihasilkan proses e2e dihapus
- INTERRUPTIONS: Satu interupsi sesi di tengah pekerjaan. Dipulihkan dengan memeriksa `git status` dan diff, lalu dilanjutkan dari keadaan terverifikasi terakhir tanpa penggandaan maupun pengulangan implementasi
- GIT STATUS: Lima berkas diubah dan tujuh berkas baru — seluruhnya pada `QuilvianSystemFrontendDev`. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya
- NEXT RECOMMENDED STEP: Naikkan `RWI-DEC-059` kepada pemilik klinis. Butir itu kini menahan dua task sekaligus — `BE-RWI-020` dan kriteria 2 `FE-RWI-012` — dan selama belum diputuskan, pasien yang meninggal atau kabur tetap tidak dapat dicatat cara pulangnya di mana pun
