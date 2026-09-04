# FE-RWI-014 — Kelima syarat penutupan tampil lengkap, dan jalan keluar supervisor tetap sempit

- TASK ID: `FE-RWI-014`
- TASK TYPE: Implementasi layar frontend baru beserta aturan tampil aksi per peran
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 10 — dua repository 2; lebih dari 20 berkas diperiksa 2; lebih dari 8 berkas diubah 2; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; alur berbatas pada satu layar 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca, kecuali laporan ini beserta tautan buktinya
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `GET /discharges/{episodeId}/closure-readiness`, `POST /discharges/{episodeId}/close`, `POST /discharges/{episodeId}/close-with-override`, `GET /discharges/{episodeId}/clearance`, dan `POST /discharges/{episodeId}/clearance/{itemId}/mark` seluruhnya berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-014` beserta `BE-RWI-023`, `BE-RWI-025`, `BE-RWI-026`; `03-frontend-architecture.md` bagian 2, 3, 5.2, 5.3, 5.4, 6, dan 9; `contracts/api-contract.md` bagian Inpatient Discharge; `contracts/permission-audit-matrix.md` bagian 2 dan 3; `contracts/state-transition-matrix.md`; `requirement-traceability.md`; [laporan `FE-RWI-013`](FE-RWI-013.md); `InpatientDischargeController.cs` bagian `GetClosureReadiness`, `CloseEpisode`, `CloseEpisodeWithOverride`, `GetClearanceChecklist`, dan `MarkClearanceItem`; `InpatientClosureDtos.cs` (`ClosureConditionResponse`, `ClosureReadinessResponse`, `CloseEpisodeRequest`, `CloseEpisodeOverrideRequest`, `ClearanceChecklistResponse`, `MarkClearanceItemRequest`); `InpDischargeService.Closure.cs` bagian `BuildClosureConditionsAsync`, `CloseEpisodeInternalAsync`, `CloseWithOverrideAsync`, `GetClearanceChecklistAsync`, dan `MarkClearanceItemAsync`; `InpatientActorClaims.cs`; `AGENTS.md` frontend beserta tujuh dokumen `.codex`; `inpatient-api.service.js`; `inpatient-discharge.service.js`; `use-inpatient-financial-clearance.jsx`; `inpatient-financial-clearance-view.jsx`; `inpatient-discharge-view.jsx`; `inpatient-episode-detail-view.jsx`; `inpatient-episode-utils.jsx`; `inpatient-episode-constants.jsx`; `use-inpatient-bed-board.jsx`; `inpatient-bed-board.jsx`; `base-form-control.jsx`; `confirm-modal.jsx`; `status-badge.jsx`; `footer.jsx` beserta `footer.css`; `tests/unit/inpatient-financial-clearance.test.mjs`; `tests/e2e/inpatient-financial-clearance.spec.mjs`; `tests/e2e/inpatient-episode-detail.spec.mjs`
- FILES CHANGED:
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-closure-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-closure-utils.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-closure-view.jsx`
  - **Baru** `src/app/health-services/inpatient-management/episodes/[id]/closure/page.jsx`
  - **Baru** `tests/unit/inpatient-closure.test.mjs`
  - **Baru** `tests/e2e/inpatient-closure.spec.mjs`
  - **Diubah** `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (route builder layar penutupan; `SUPERVISOR_ROLES`)
  - **Diubah** `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (`isSupervisor` pada konteks pelaku)
  - **Diubah** `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (satu tautan menuju layar penutupan)

## 1. Kriteria 2 dikerjakan sebagai keadaan, bukan sebagai tata letak

Roadmap menulis kriteria 2 sebagai aturan keras: jalan keluar supervisor **tidak** boleh
ditampilkan berdampingan dengan tombol tutup biasa, dan baru muncul **setelah** tombol tutup biasa
ditolak karena kelayakan keuangan, **hanya** untuk supervisor.

Cara yang paling mudah — merender kedua tombol lalu menyembunyikan yang satu dengan CSS, atau
menampilkannya dalam keadaan nonaktif — semuanya gagal memenuhi maksudnya: jalan keluar yang
terlihat sejak awal akan menjadi jalur normal dalam hitungan minggu. Karena itu tampilnya
diturunkan dari **tiga syarat yang harus terpenuhi bersamaan**, dan bagiannya benar-benar tidak
dirender bila salah satunya tidak terpenuhi:

| Syarat | Sumbernya |
| --- | --- |
| Penutupan biasa sudah pernah ditolak **pada layar ini** | Penanda `closeRejected`, yang **hanya** disetel jalur penutupan biasa — jalur jalan keluar tidak pernah menyalakannya sendiri |
| Yang menahan **hanya** kelayakan keuangan | `isBlockedByFinancialClearanceOnly` membaca `conditions` dari `closure-readiness`: seluruh syarat yang belum terpenuhi harus bertanda `canBeOverridden` dan berkode `FINANCIAL_CLEARED` |
| Pelakunya supervisor | `SUPERVISOR_ROLES`, disalin apa adanya dari `InpatientActorClaims.SupervisorRoles` — daftar yang **lebih sempit** daripada daftar kepala ruangan |

Syarat kedua bukan kehati-hatian berlebih. `CloseEpisodeInternalAsync` menembus syarat keuangan
**saja**; empat syarat lainnya tetap menahan. Menawarkan jalan keluar ketika resume belum
ditandatangani hanya akan menghasilkan 422 kedua beserta kebingungan.

Kedua jalur penolakan dicakup: penolakan yang terbaca dari pembacaan ulang kesiapan **sebelum**
permintaan dikirim, dan penolakan 422 yang datang dari server ketika kasir mengubah nilainya di
sela-sela. Keduanya menyalakan penanda yang sama.

## 2. Penambahan `isSupervisor`, dan kenapa ia tidak menumpang daftar yang sudah ada

`getEpisodeActorContext` sebelumnya hanya mengenal `isSupervisorOrWardHead`. Memakainya untuk
jalan keluar keuangan akan **membuka jalan keluar itu bagi kepala ruangan**, yang tidak pernah
diberi wewenang `InpatientEpisode : CloseOverride` pada permission matrix bagian 3 dan akan
ditolak 403 `CloseWithOverrideAsync`. Karena itu ditambahkan `SUPERVISOR_ROLES` yang lebih sempit,
disalin apa adanya dari `InpatientActorClaims.SupervisorRoles`, dan dibaca lewat konteks pelaku
yang sama — bukan lewat pembacaan klaim peran kedua di tempat lain.

## 3. Satu cacat tata letak yang ditemukan lewat verifikasi, bukan lewat lint

Tombol tutup dan tombol jalan keluar adalah elemen paling bawah halaman ini, sedangkan footer
aplikasi berposisi `fixed`. Pada percobaan pertama, **penekanan tombolnya tertelan footer** —
Playwright melaporkan `<footer class="iq-footer app-footer">…</footer> intercepts pointer events`
berulang kali sampai batas waktu. Lint, test unit, dan build semuanya hijau saat itu.

Mekanisme `--app-footer-safe-space` di `footer.jsx` memasang jarak aman pada elemen scroll secara
dinamis, dan pada halaman ini ia tidak menutupi keadaan tersebut. Perbaikannya dibatasi pada
halaman ini: `styles.dataShell` diberi `paddingBottom` memakai variabel `--app-footer-safe-space`
yang **sudah** dipakai tabel dan editor — bukan angka baru, bukan perubahan CSS global.

Ini persis yang dimaksud aturan bahwa lint/test/build yang PASS bukan bukti perilaku.

## 4. Delta kontrak yang dicatat, bukan ditutupi

**Penandaan butir administrasi tidak dapat dipisahkan mesin hak akses.**
`03-frontend-architecture.md` bagian 3 memberi aksi "Menandai butir administrasi" kepada petugas
admisi dan supervisor, dan **tidak** kepada DPJP. Tetapi `MarkClearanceItem` dijaga
`InpatientDischarge : Update` — butir hak akses **yang sama persis** dengan penyusunan resume
pulang, dan resume memang milik DPJP. Server karena itu **tidak dapat** menolak DPJP yang menandai
butir administrasi.

Pemisahannya hanya ada di layar: aksi penandaan dan tombol tutup tidak dirender bagi pengguna
berperan dokter. Yang perlu diputuskan pemilik keamanan: apakah `InpatientDischarge : Update`
dipecah menjadi dua butir, atau aturan bagian 3 dilonggarkan. **Owner: Backend/API bersama pemilik
keamanan.**

**Peran "petugas admisi" tidak punya daftar nama peran di backend.** Berbeda dengan supervisor,
kepala ruangan, dan kasir, tidak ada `AdmissionRoles` pada `InpatientActorClaims`. Akibatnya layar
tidak dapat menyembunyikan tombol tutup biasa dari perawat pelaksana, walaupun bagian 3
menuliskannya tanpa wewenang menutup. Yang menahannya tetap server lewat `InpatientEpisode :
Close`. Layar menutup jalur yang **dapat** diturunkan dari bukti — jalur DPJP — dan tidak menebak
sisanya.

- IMPLEMENTATION: (1) Kelima syarat dibaca dari `closure-readiness` **sejak layar dibuka** dan dirender tanpa satu pun penjaga penolakan; masing-masing membawa tanda sudah atau belum sebagai **teks**, kalimat `unmetMessage` dari server apa adanya, dan keterangan siapa yang dapat memenuhinya. (2) Syarat yang belum dikenal layar tetap ikut dirender — bentuk daftar adalah kontrak, dan jawaban revisi berikutnya tidak boleh hilang diam-diam. (3) Kesiapan dibaca ulang **tepat sebelum** tombol tutup dijalankan, bukan hanya saat layar dibuka — bagian 5.2. Ketika belum siap, permintaan **tidak** dikirim: daftar syarat yang baru dibaca jauh lebih berguna daripada kalimat gabungan 422. (4) Jalan keluar supervisor dirender hanya ketika ketiga syarat pada bagian 1 terpenuhi. (5) Alasan jalan keluar diperiksa di layar dengan aturan yang sama persis dengan `CloseWithOverrideAsync` — kosong maupun tanpa satu pun huruf atau angka sama-sama ditolak sebelum permintaan terkirim. (6) Butir administrasi ditandai dari layar ini; jawaban penandaan membawa daftar periksa utuh, dan daftar syarat ikut dibaca ulang karena butir administrasi adalah syarat ketiga. (7) Butir yang sudah dinonaktifkan admin tetap terbaca beserta penandaan lamanya, tetapi tombol penandaannya tidak dirender — `MarkClearanceItemAsync` menolaknya 422. (8) Tiga penjaga `in-flight` menahan klik kedua pada penandaan, penutupan, dan jalan keluar. (9) Dialog konfirmasi penutupan menyebut nama pasien dan cara pulang; dialog jalan keluar menyebut daftar pantau penutupan menembus gerbang keuangan dan bahwa penutupan tidak dapat dibatalkan — bagian 5.3. (10) Penolakan 409 dan 422 memuat ulang daftar syarat dan **tidak** menyentuh isian yang sedang diketik — bagian 5.4.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Muatan memakai nama kolom DTO apa adanya: `CloseEpisodeRequest.note`, `CloseEpisodeOverrideRequest.reason`, dan `MarkClearanceItemRequest.note`, masing-masing dipangkas ke 500 karakter sesuai `MaxLength`. Pelaku dan waktu tidak pernah dikirim layar
- DATABASE IMPACT: Tidak ada
- SECURITY IMPACT: Tidak mengubah authorization. Layar menyembunyikan aksi yang pasti ditolak server (`InpatientEpisode : CloseOverride`) dan aksi yang menurut bagian 3 bukan milik DPJP; server tetap satu-satunya penentu. **Dua keterbatasan dicatat pada bagian 4:** penandaan butir administrasi memakai butir hak akses yang sama dengan penyusunan resume sehingga server tidak dapat memisahkan DPJP, dan peran "petugas admisi" tidak punya daftar nama peran sehingga tombol tutup biasa tidak dapat disembunyikan dari perawat. **Satu risiko akses lain:** layar membaca `closure-readiness` dan `clearance` yang menuntut `InpatientDischarge : Read`; peran yang tidak memilikinya akan tertutup `AccessDeniedGate`
- VISUAL REFERENCE: NOT REQUIRED
- WEWENANG UI YANG DIPAKAI: "Bentuk daftar syarat bebas, selama kelimanya terbaca". Dipilih satu layar tersendiri berisi lima bagian berurutan — ringkasan episode, hasil penutupan, daftar kelima syarat, butir administrasi, penutupan, lalu jalan keluar supervisor bila berhak — memakai `Hero`, `InformationAlert`, `StatusBadge`, `BaseTextAreaField`, `BaseButton`, `ConfirmModal`, dan `ToastStack` yang sudah ada. Kelima syarat berbentuk daftar bernomor dengan `StatusBadge` "Sudah"/"Belum"; keadaannya **tidak pernah** disampaikan lewat warna saja. Mengikuti bentuk layar `FE-RWI-012` dan `FE-RWI-013` supaya ketiga layar per-episode terasa satu keluarga. Tidak ada komponen baru, tidak ada pola HTTP baru, dan tidak ada arsitektur state baru

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Kelima syarat tampil beserta tanda sudah atau belum, **selalu** | **LULUS** | e2e membuktikan kelima syarat dirender sejak layar dibuka tanpa satu pun tombol ditekan: `li[data-testid^="closure-condition-"]` `toHaveCount(5)`, masing-masing dengan teks "Sudah" atau "Belum", dan kalimat `unmetMessage` server apa adanya untuk yang belum. e2e kedua membuktikan kelimanya tetap terbaca ketika seluruhnya sudah terpenuhi. Test unit membuktikan jawaban server yang **terbalik** tetap terbaca urut nomor syarat, dan syarat keenam yang belum dikenal layar tetap ikut dirender. Test unit terpisah membaca sumber layar dan membuktikan bagian daftar syarat tidak memuat satu pun penjaga penolakan |
| 2. Tombol tutup menembus gerbang **tidak** ditampilkan berdampingan; muncul **hanya setelah** tutup biasa ditolak karena kelayakan keuangan, dan **hanya** untuk supervisor | **LULUS** | e2e membuktikan supervisor **tidak** melihat bagian jalan keluar walaupun syarat keuangan sudah terbaca "Belum" sejak layar dibuka — `toHaveCount(0)`, tidak dirender. e2e berikutnya membuktikan bagian itu muncul tepat setelah tombol tutup biasa ditolak, dan **nol** permintaan penutupan terkirim karena kesiapan dibaca ulang lebih dulu. e2e ketiga membuktikan jalur penolakan server 422 menghasilkan hal yang sama. Tiga e2e per peran membuktikan petugas admisi, perawat, dan kepala ruangan **tetap** tidak melihatnya walau penutupan sudah ditolak. e2e terakhir membuktikan jalan keluar tidak muncul ketika resume juga belum ditandatangani. Test unit menutup keenam kombinasi peran dan membuktikan penanda penolakan hanya disetel jalur penutupan biasa |
| 3. Menembus gerbang wajib beralasan | **LULUS** | e2e menekan tombol jalan keluar dengan alasan kosong: pesan "Alasan penutupan tanpa kelayakan keuangan wajib diisi." muncul dan **nol** permintaan terkirim. Sesudah alasan diisi, tepat satu permintaan terkirim dengan muatan `{ reason: … }` saja. e2e terpisah membuktikan dialog konfirmasinya menyebut daftar pantau penutupan menembus gerbang keuangan dan bahwa tindakan tidak dapat dibatalkan. Test unit membuktikan spasi maupun tanda baca saja ditolak, sama seperti `CloseWithOverrideAsync` |
| 4. Butir administrasi dapat ditandai dari layar ini | **LULUS** | e2e menandai butir "Obat pulang diserahkan" beserta catatannya: tepat satu permintaan terkirim ke `.../clearance/{itemId}/mark` dengan `itemId` yang benar dan muatan `{ note: … }`, butirnya berubah menjadi "Sudah ditandai" beserta catatannya, dan **syarat ketiga ikut berubah** dari "Belum" menjadi "Sudah" karena daftar syarat dibaca ulang. e2e yang sama membuktikan urutan butir mengikuti `sortOrder` walaupun server mengirimnya terbalik. e2e per peran membuktikan DPJP tidak melihat satu pun tombol penandaan maupun tombol tutup, sedangkan kelima syarat tetap terbaca olehnya. Test unit membuktikan butir yang sudah dinonaktifkan tetap terbaca tetapi tidak dapat ditandai |
| 5. Setelah penutupan berhasil, tempat tidur terbaca kembali kosong pada papan ketersediaan | **LULUS untuk yang dapat dibuktikan frontend** | e2e menutup episode, membuktikan tepat satu permintaan `POST .../close` terkirim, lalu berpindah ke papan ketersediaan lewat tautan pada layar dan membuktikan `bed-row-BD-001` terbaca "Dapat dipakai" **tanpa** nama pasien. **Batas bukti ini perlu dinyatakan:** pelepasan tempat tidurnya sendiri dikerjakan `ReleaseActivePlacementAsync` di dalam transaksi penutupan dan dibuktikan integration test `BE-RWI-025` kriteria 4; yang dibuktikan e2e frontend adalah papan membaca keadaan sesudahnya apa adanya dan tidak menyimpan salinan basi |

- VALIDATION: e2e `tests/e2e/inpatient-closure.spec.mjs` | PASS, 15/15 | TASK | dijalankan pada browser sungguhan (Edge) terhadap build produksi; termasuk lima e2e kriteria 2, dua e2e kriteria 3, dua e2e kriteria 4, dan e2e kriteria 5 yang berpindah ke papan ketersediaan
- VALIDATION: e2e regresi `inpatient-episode-detail.spec.mjs`, `inpatient-discharge.spec.mjs`, `inpatient-financial-clearance.spec.mjs`, `inpatient-census.spec.mjs` | PASS, 36/36 | TASK | berkas bersama yang ikut diubah — `inpatient-episode-utils.jsx`, `inpatient-episode-constants.jsx`, dan `inpatient-episode-detail-view.jsx`
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-closure.test.mjs` | PASS, 26/26 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/episodes/[id]/closure` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 194, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND` atas `src/utils/auth/base-login-utils.jsx`; berkas yang ada bernama `base-login-utils.js`. Tidak bersinggungan dengan diff ini, dan sudah tercatat pada laporan `FE-RWI-011` s.d. `FE-RWI-013`
- VALIDATION: `npm run test:unit` | NOT RUN sebagai apa adanya | EXISTING / ENVIRONMENT ISSUE | script-nya memakai `--test tests/unit/` dan Node menolaknya dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Dijalankan lewat bentuk glob seperti pada task-task sebelumnya
- MANUAL TEST: PASS — seluruh kontrol interaktif yang diubah dijalankan di browser sungguhan (Edge) terhadap build produksi lewat e2e dengan peran berbeda per kasus: tombol tutup beserta keadaan memuatnya, tombol jalan keluar beserta kemunculan dan ketidakmunculannya, kotak alasan beserta penolakan kosongnya, tombol penandaan tiap butir beserta catatannya, kedua dialog konfirmasi beserta tombol batalnya, dan perpindahan ke papan ketersediaan. Isi setiap permintaan yang terkirim diperiksa, dan jumlah permintaan pada jalur yang seharusnya menolak diperiksa **nol**. **Satu cacat perilaku ditemukan dan diperbaiki lewat cara ini** — footer aplikasi menelan penekanan tombol tutup; lihat bagian 3
- WARNINGS: **Penandaan butir administrasi tidak dapat dijaga server terhadap DPJP** — butir hak aksesnya sama persis dengan penyusunan resume; lihat bagian 4. **Tombol tutup biasa tidak dapat disembunyikan dari perawat pelaksana** karena tidak ada daftar nama peran petugas admisi di backend; yang menahannya tetap `InpatientEpisode : Close` di server. **Nama peran supervisor adalah asumsi yang belum dikonfirmasi rumah sakit** — `InpatientActorClaims` menandainya demikian dan frontend menyalinnya apa adanya; bila nama peran supervisor di rumah sakit berbeda, jalan keluar keuangan tidak akan pernah tampil bagi orang yang berwenang, dan pasien yang harus segera pulang ikut tertahan
- KNOWN ISSUES: Pelaku penandaan butir administrasi hanya terbaca sebagai identifier pengguna, karena `ClearanceChecklistItemResponse.MarkedByUserId` berupa `Guid` tanpa nama — gap yang sama dengan riwayat kelayakan keuangan pada `FE-RWI-013`. Layar menampilkannya apa adanya dan **tidak menebak** siapa orangnya. Penandaan butir juga **tidak dapat dicabut**: kontrak tidak menyediakan endpoint pencabutan, dan layar menyatakan batas itu apa adanya
- DEPENDENCY BACKEND: `BE-RWI-025` ✅ **Selesai** — `GET .../closure-readiness` dan `POST .../close` berstatus ✅ `Tersedia`, terbukti berjalan 26 Agustus 2026. `BE-RWI-026` ✅ **Selesai** — `POST .../close-with-override`. `BE-RWI-023` ✅ untuk daftar periksa administrasi beserta penandaannya
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan lewat `git checkout --`; direktori artefak `test-results/inpatient-closure-*` dan `.playwright-artifacts-*` dihapus. Tidak ada perubahan sampingan lain yang tersisa pada diff
- INTERRUPTIONS: NONE
- GIT STATUS: Tiga berkas diubah dan tujuh berkas baru pada `QuilvianSystemFrontendDev`, seluruhnya bersama `FE-RWI-015` yang dikerjakan berurutan. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md`
- NEXT RECOMMENDED STEP: Bawa dua delta pada bagian 4 ke pemilik keamanan — pemecahan `InpatientDischarge : Update`, dan ketiadaan daftar nama peran petugas admisi. Keduanya menentukan apakah bagian 3 arsitektur frontend dapat dijaga server atau tetap menjadi aturan layar saja
