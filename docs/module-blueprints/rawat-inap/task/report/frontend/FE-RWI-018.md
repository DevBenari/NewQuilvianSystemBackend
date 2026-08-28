# FE-RWI-018 — Supervisor dapat membetulkan catatan lewat sesi koreksi

- TASK ID: `FE-RWI-018`
- TASK TYPE: Layar baru per episode — membuka sesi koreksi, mengoreksi resume di dalamnya, lalu menutup sesi beserta daftar perubahannya
- COMPLEXITY: `MEDIUM`
- CLASSIFICATION SCORE: 9 — dua repository 2; 9–20 berkas diperiksa 1; 4–8 berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; tiga aksi tulis pada satu layar 1; satu kesenjangan kontrak yang harus dinyatakan apa adanya 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca, kecuali laporan ini beserta tautan buktinya
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /episodes/{id}/correction-sessions` dan `PATCH /episodes/{id}/correction-sessions/{sessionId}/close` berstatus ✅ **Tersedia**; `PUT /discharges/{episodeId}/summary` ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-018` beserta `BE-RWI-030`, `BE-RWI-021`, dan `BE-RWI-022`; `03-frontend-architecture.md` bagian 2, 3, 5.1, 5.2, 5.3, 5.4, dan 6; `contracts/api-contract.md` bagian Inpatient Episode dan Inpatient Discharge; `contracts/permission-audit-matrix.md` bagian 2 dan 3; `contracts/state-transition-matrix.md` bagian 5 dan 6; `blueprint-manifest.md` bagian 8; `00-interview-decisions.md` `RWI-DEC-009`, `RWI-DEC-028`, `RWI-DEC-057`, `RWI-RULE-020`; `InpatientEpisodeController.cs` bagian `OpenCorrectionSession` dan `CloseCorrectionSession`; `InpatientCorrectionDtos.cs`; `InpEpisodeService.Corrections.cs`; `InpatientDischargeController.cs` bagian `GetSummary` dan `UpsertSummary`; `InpDischargeService.cs` bagian `UpsertSummaryAsync`; `InpDischargeService.Closure.cs` bagian `GuardEpisodeNotClosedAsync`; `InpatientEpisodeDtos.cs` (`InpatientEpisodeDetailResponse`); `InpatientActorClaims.cs`; [laporan `FE-RWI-016`](FE-RWI-016.md); `use-inpatient-discharge.jsx`; `inpatient-discharge-view.jsx`; `inpatient-discharge-utils.jsx`; `inpatient-discharge-constants.jsx`; `use-inpatient-financial-clearance.jsx`; `inpatient-financial-clearance-view.jsx`; `inpatient-episode-utils.jsx`; `inpatient-episode-constants.jsx`; `inpatient-episode-detail-view.jsx`; `inpatient-closure-view.jsx`; `confirm-modal.jsx`; `base-form-control.jsx`; `footer.jsx`
- FILES CHANGED:
  - **Baru** `src/app/health-services/inpatient-management/episodes/[id]/correction/page.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-correction-view.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-correction.jsx`
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-correction-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-correction-utils.jsx`
  - **Baru** `tests/unit/inpatient-correction.test.mjs`
  - **Baru** `tests/e2e/inpatient-correction.spec.mjs`
  - **Diubah** `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (`buildInpatientEpisodeCorrectionRoute`)
  - **Diubah** `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (tautan Sesi Koreksi, hanya bagi supervisor pada episode yang sudah ditutup)

## 1. Kesenjangan kontrak yang dinyatakan apa adanya, bukan ditutupi

Api contract `0.4.0` menyediakan **dua** endpoint sesi koreksi: membuka dan menutup. **Tidak ada
endpoint pembacaan**, dan `InpatientEpisodeDetailResponse` juga tidak membawa sesi koreksi.
`InpEpisodeService.GetCorrectionSessionsAsync` memang ada di backend, tetapi hanya dipakai internal
untuk menyusun jawaban kedua endpoint tulis itu.

Akibatnya bagi layar ini: **ia tidak dapat mengetahui sesi terbuka yang dibuka pada kunjungan
halaman sebelumnya atau oleh supervisor lain.** Satu-satunya sumber yang dapat dipercaya adalah
jawaban dari tindakan yang dijalankan di layar ini sendiri.

Yang dikerjakan:

1. Sesi yang sudah diketahui **tidak** disetel ulang oleh pemuatan ulang data — sekali terbaca, ia
   bertahan sampai halaman ditinggalkan. Pola yang sama sudah dipakai `FE-RWI-013` untuk kelayakan
   keuangan yang juga tidak punya endpoint pembacaan.
2. Layar **menyatakan batas itu di layar**, bukan menyembunyikannya: satu kalimat tetap yang
   menjelaskan bahwa kontrak versi ini tidak menyediakan pembacaan daftar sesi, dan bahwa server
   akan menolak pembukaan berikutnya bila ternyata sudah ada sesi terbuka.
3. Penolakan 409 dari server ditampilkan **apa adanya**.

Berpura-pura dapat membaca sesi terbuka jauh lebih berbahaya daripada mengakuinya: supervisor yang
melihat "belum ada sesi" lalu menekan tombol buka akan menyimpulkan sistemnya rusak ketika ditolak,
padahal yang terjadi adalah rekannya sedang mengoreksi episode yang sama.

**Yang perlu diputuskan:** apakah `GET /episodes/{id}/correction-sessions` perlu dibuka. Service-nya
sudah ada; yang belum ada hanya endpoint-nya. Owner: Backend/API bersama Product/Domain.

## 2. Kriteria 2 dibuktikan dari data, bukan dari kalimat

Godaan terbesar pada layar ini adalah membuat supervisor mengira episodenya terbuka kembali.
`blueprint-manifest.md` bagian 8 butir 5 mengunci sesi koreksi sebagai konsep tersendiri, bukan
status keenam — `RWI-DEC-009` mengunci lima nilai status.

Layar mengatakannya dua kali, dengan cara yang berbeda:

| Cara | Isinya |
| --- | --- |
| Kalimat tetap | Status tetap Selesai, tempat tidur tidak dikembalikan, pasien tidak muncul kembali pada census, lama dirawat tidak bertambah |
| Data | **Detail episode dibaca ulang dari server sesudah sesi dibuka**, dan status yang terbaca di layar adalah status yang benar-benar dijawab server saat itu |

Yang kedua yang penting. Kalimat tetap dapat saja bohong bila suatu hari service berubah; status
yang dibaca ulang tidak.

Test unit ikut menjaga sisi lain dari aturan ini: ia membaca `INPATIENT_EPISODE_STATUS` dan
membuktikan nilainya tetap **lima**, dengan nama yang persis, dan tidak ada nilai bernama koreksi.
Menambah nilai keenam adalah cara paling mudah melanggar `RWI-DEC-009` tanpa disadari.

## 3. Koreksi resume dikerjakan di layar ini, bukan di layar resume

`resolveDischargeSummaryAuthority` pada layar Keputusan Pulang dan Resume **menolak** penyuntingan
resume yang sudah ditandatangani, dan menunjuk ke sini — kalimat itu sudah ditulis `FE-RWI-012`.
Membuka kembali penyuntingan di sana akan membuat dua jalur berbeda menuju amandemen rekam medis
yang sama, dan salah satunya pasti akan lupa diperbarui.

Karena itu form koreksi resume ada **di dalam layar sesi koreksi**, dan hanya dirender ketika
ketiganya benar sekaligus — sama persis dengan yang diperiksa `UpsertSummaryAsync`:

1. resume memang sudah ada;
2. ada sesi koreksi yang terbuka;
3. pelakunya supervisor.

Form-nya memakai `buildDischargeSummaryPayload`, `mapSummaryToForm`, dan
`validateDischargeSummaryForm` yang **sudah ada** — tidak ada pembentuk muatan kedua, sehingga batas
panjang tiap kolom tidak mungkin berselisih antara dua layar.

Sesudah koreksi tersimpan, resume dibaca ulang dengan `includeRevisions=true`. Jawaban
`PUT .../summary` tidak membawa riwayat versi, dan tanpa pembacaan ulang itu layar akan menyimpulkan
versi lama tidak pernah tersimpan — persis kebalikan dari yang diminta `RWI-DEC-057`.

## 4. Satu cacat perilaku ditemukan lewat verifikasi, bukan lewat lint

Sama seperti `FE-RWI-014`: tombol buka sesi, simpan koreksi, dan tutup sesi bergantian menjadi
elemen paling bawah halaman, sedangkan footer aplikasi berposisi `fixed`. Pada percobaan pertama
Playwright melaporkan `<footer class="iq-footer app-footer">…</footer> intercepts pointer events`
berulang kali sampai batas waktu, pada **dua** kasus uji sekaligus. Lint, test unit, dan build
semuanya hijau saat itu.

Perbaikannya dibatasi pada halaman ini dan memakai mekanisme yang sudah ada: `styles.dataShell`
diberi `paddingBottom` dari variabel `--app-footer-safe-space` — bukan angka baru, bukan perubahan
CSS global. Ini pengulangan cacat yang sama pada halaman berbeda, dan pola perbaikannya kini terbukti
dua kali.

## 5. Yang boleh dan tidak boleh dibetulkan, ditulis di layar

State matrix bagian 6.1 menetapkan batasnya, dan `BE-RWI-030` bagian 6.1 mencatat satu di antaranya
**belum** ditegakkan kode: cara pulang tidak dapat dikoreksi lewat sesi, karena
`DecideDischargeAsync` menolak episode `Closed` dan tidak ada jalur lain pada kontrak `0.4.0`.

Layar menuliskan keduanya berdampingan: yang boleh dibetulkan (isi resume termasuk yang sudah
ditandatangani, penandaan butir administrasi, penandaan kelayakan keuangan) dan yang tidak (waktu
admisi, waktu penutupan, riwayat penempatan, riwayat status, **dan cara pulang — belum ada jalurnya
pada kontrak versi ini**).

Supervisor yang membuka sesi untuk membetulkan cara pulang perlu tahu sebelum ia membukanya, bukan
sesudah.

- IMPLEMENTATION: (1) Route `/health-services/inpatient-management/episodes/{id}/correction`, mengikuti keluarga route yang sudah ada — `/discharge`, `/financial-clearance`, `/closure`. (2) Tautan masuknya dipasang pada detail episode, **hanya** bagi supervisor dan **hanya** pada episode yang sudah ditutup; peran lain tidak melihat tautannya sama sekali. (3) Seluruh aksi sesi koreksi tidak dirender bagi peran selain supervisor — `isVisible` dan `canOpen` sengaja dipisah, sehingga supervisor yang episodenya belum ditutup tetap membaca alasannya, sedangkan peran lain tidak melihat bagiannya sama sekali. (4) Ketiga aksi tulis dijaga `useRef` terhadap penekanan ganda — bagian 5.3 — dan dua di antaranya memakai `ConfirmModal` yang sudah ada. (5) Kalimat penolakan disalin apa adanya dari service, sehingga penolakan di layar dan penolakan server berbunyi sama persis. (6) Muatan memakai nama kolom DTO apa adanya dan dipangkas ke `MaxLength`-nya: `openReason` 500, `changedFieldSummary` 4000. (7) Riwayat versi resume dirender sesudah koreksi, memuat nama penandatangan lama dan waktu penggantiannya
- API CONTRACT IMPACT: Tidak mengubah kontrak. **Satu kesenjangan dicatat:** tidak ada `GET .../correction-sessions`, sehingga sesi terbuka yang dibuat di luar kunjungan halaman ini tidak dapat dibaca layar — bagian 1
- DATABASE IMPACT: Tidak ada
- SECURITY IMPACT: Tidak mengubah authorization. Layar menyembunyikan aksi yang pasti ditolak `InpatientEpisode : Reopen` — butir yang menurut permission matrix bagian 3 **hanya** dimiliki supervisor — dan menyembunyikan koreksi resume yang pasti ditolak `UpsertSummaryAsync` bagi non-supervisor. Server tetap satu-satunya penentu. **Satu risiko dicatat:** nama peran supervisor adalah asumsi `InpatientActorClaims` yang belum dikonfirmasi rumah sakit, dan frontend menyalinnya apa adanya. Isi resume bertanda sensitif dan hanya dirender pada layar ini bagi supervisor, tidak pernah pada daftar mana pun
- VISUAL REFERENCE: NOT REQUIRED
- WEWENANG UI YANG DIPAKAI: "Bebas". Dipilih satu layar per episode dengan alasan pada bagian 3, memakai kerangka service dan state `FE-RWI-002` beserta `Hero`, `InformationAlert`, `StatusBadge`, `BaseTextField`, `BaseTextAreaField`, `BaseButton`, `ConfirmModal`, dan `ToastStack` yang sudah ada. Tidak ada komponen baru, tidak ada hook generik baru, tidak ada pola HTTP baru, dan tidak ada arsitektur state baru

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Hanya supervisor yang melihat aksi membuka sesi koreksi | **LULUS** | Empat e2e per peran membuktikan bagian sesi koreksi **tidak dirender sama sekali** bagi petugas admisi, perawat, kepala ruangan, dan kasir — `toHaveCount(0)`, bukan dirender lalu dinonaktifkan — dan **nol** permintaan terkirim. e2e kelima membuktikan hal yang sama bagi DPJP aktif episode itu. e2e keenam membuktikan supervisor melihatnya, dan tautan masuknya pada detail episode berfungsi; e2e ketujuh membuktikan kepala ruangan tidak melihat tautan itu. Test unit menutup keenam peran, termasuk supervisor yang kebetulan juga seorang dokter |
| 2. Selama sesi berjalan, layar menunjukkan bahwa status episode tetap `Closed` | **LULUS** | e2e membuka sesi lalu membuktikan tiga hal sekaligus: status episode terbaca **tetap** "Sudah ditutup" sesudahnya, kalimat "Status episode tetap Sudah ditutup." tampil menempel pada penanda sesi berjalan, dan peringatan yang menyebut tempat tidur tidak dikembalikan serta lama dirawat tidak bertambah terbaca. Rekaman permintaan membuktikan detail episode benar-benar **dibaca ulang dari server sesudah** sesi dibuka, sehingga status itu bukan janji layar. Test unit ikut membuktikan `INPATIENT_EPISODE_STATUS` tetap berisi lima nilai dan tidak ada nilai bernama koreksi |
| 3. Menutup sesi wajib menyertakan daftar perubahannya | **LULUS** | e2e menekan tutup sesi tanpa mengisi daftar perubahan, mengonfirmasi dialognya, lalu membuktikan penolakan "Tuliskan apa saja yang diubah sebelum menutup sesi koreksi." — disalin apa adanya dari `CloseCorrectionSessionAsync` — dan **nol** permintaan terkirim. Sesudah diisi, penutupannya berhasil dan muatannya diperiksa berisi `changedFieldSummary` persis seperti yang diketik. Test unit menutup isian kosong, isian berisi spasi, dan pemangkasan ke 4000 karakter |
| 4. Koreksi resume yang sudah ditandatangani menampilkan peringatan bahwa versi lama akan disimpan | **LULUS** | e2e membuktikan sebelum sesi dibuka resume tertandatangani **tidak dapat dikoreksi sama sekali** beserta kalimat servernya; sesudah sesi dibuka form-nya muncul bersama peringatan "Versi lama resume akan disimpan"; dialog konfirmasinya mengulang peringatan itu dan **nol** permintaan terkirim selagi dialog terbuka; sesudah dikonfirmasi, versi lama tampil beserta nama penandatangan lamanya, dan pembacaan ulang dengan `includeRevisions=true` terbukti terkirim. e2e terpisah membuktikan resume yang **belum** ditandatangani tidak memunculkan peringatan itu — ia memang tidak menyimpan versi. Test unit mengikat peringatan itu pada tanda tangan resume, bukan pada sesi yang kebetulan terbuka |
| 5. Satu episode tidak dapat punya dua sesi terbuka | **LULUS, dengan batas bukti yang dinyatakan** | e2e membuktikan penolakan 409 server "Episode ini sedang dalam sesi koreksi yang belum ditutup." terbaca **apa adanya** di layar. e2e kedua membuktikan sesudah satu sesi dibuka dari layar ini, form pembukaan **tidak dirender lagi** dan keterangannya berbunyi kalimat yang sama. Test unit membuktikan sesi yang sudah ditutup tidak pernah terbaca terbuka — bahkan ketika `isOpen` dari server bertentangan dengan `closedAt` — dan bahwa sesi berikutnya boleh dibuka **setelah** yang pertama ditutup. **Batas buktinya:** layar tidak dapat mengetahui sesi terbuka milik supervisor lain karena tidak ada endpoint pembacaannya; penjaga sesungguhnya tetap server, dan layar menyatakan batas itu apa adanya — bagian 1 |

- VALIDATION: e2e `tests/e2e/inpatient-correction.spec.mjs` | PASS, 14/14 | TASK | dijalankan pada browser sungguhan (Edge) terhadap build produksi; termasuk tujuh e2e per peran
- VALIDATION: e2e regresi `inpatient-episode-detail.spec.mjs`, `inpatient-departure.spec.mjs`, `inpatient-closure.spec.mjs`, `inpatient-census.spec.mjs` | PASS, 40/40 | TASK | detail episode adalah berkas bersama yang diubah task ini
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-correction.test.mjs` | PASS, 26/26 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/episodes/[id]/correction` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 244, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND`; sudah tercatat pada `FE-RWI-015` dan tidak bersinggungan dengan diff ini
- MANUAL TEST: PASS — seluruh kontrol interaktif yang ditambahkan dijalankan di browser sungguhan (Edge) terhadap build produksi lewat e2e dengan peran berbeda per kasus: tombol buka sesi beserta isian alasannya dan penolakan kosongnya, ketujuh isian resume, tombol simpan koreksi beserta dialog konfirmasinya, tombol tutup sesi beserta isian daftar perubahan dan dialog konfirmasinya, kedua tombol Batal, dan tautan Sesi Koreksi pada detail episode. Isi setiap permintaan yang terkirim diperiksa, dan jumlah permintaan pada jalur yang seharusnya menolak diperiksa **nol**. **Satu cacat perilaku ditemukan dan diperbaiki lewat cara ini** — footer aplikasi menelan penekanan tombol pada dua kasus uji; lihat bagian 4
- WARNINGS: **Sesi terbuka milik supervisor lain tidak dapat dibaca layar** — bagian 1; perlu keputusan apakah `GET .../correction-sessions` dibuka. **Cara pulang belum dapat dikoreksi lewat sesi** — `BE-RWI-030` bagian 6.1; layar menuliskannya sebagai batas, tetapi kesalahan cara pulang pada episode tertutup memang tidak dapat dibetulkan sama sekali hari ini. **Unique index parsial satu sesi terbuka belum diuji terhadap PostgreSQL** — `BE-RWI-030` bagian 4.1; dua supervisor yang membuka bersamaan masih bergantung pada pemeriksaan service. **Nama peran supervisor adalah asumsi** yang belum dikonfirmasi rumah sakit
- KNOWN ISSUES: Penandaan butir administrasi dan kelayakan keuangan **boleh** dikerjakan di dalam sesi koreksi menurut `GuardEpisodeNotClosedAsync`, dan layar menuliskannya sebagai cakupan yang boleh dibetulkan — tetapi jalurnya ada pada layar Penutupan Episode dan Kelayakan Keuangan, bukan di sini. Supervisor perlu berpindah layar untuk mengerjakannya selagi sesi terbuka. Menggabungkannya ke layar ini akan menyalin dua form yang sudah ada, dan itu justru yang dilarang aturan tanpa abstraksi ganda
- DEPENDENCY BACKEND: `BE-RWI-030` ✅ **Selesai** — kedua endpoint sesi koreksi berstatus ✅ `Tersedia`, terbukti berjalan 26 Agustus 2026. `BE-RWI-022` ✅ **Selesai** untuk penyimpanan versi resume. `FE-RWI-015` ✅ **Selesai**; `FE-RWI-012` ✅ **Selesai** — kalimat "hanya dapat diubah lewat sesi koreksi yang dibuka supervisor" yang ditulis task itu kini benar-benar berujung pada layar yang ada
- INCIDENTAL CHANGES: Direktori artefak Playwright `test-results/fe-rwi-016-018/` dibuat oleh jalannya e2e lalu dihapus. Config Playwright sementara ditulis di luar repository (direktori scratchpad sesi). Tidak ada perubahan sampingan yang tersisa pada diff
- INTERRUPTIONS: NONE
- GIT STATUS: Pada `QuilvianSystemFrontendDev` branch `HamzahV2`: dua berkas diubah dan lima berkas baru untuk task ini, bersama perubahan `FE-RWI-016` dan `FE-RWI-017` yang dikerjakan berurutan pada sesi yang sama. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya
- NEXT RECOMMENDED STEP: Kerjakan `FE-RWI-019` — kasus e2e kesiapan per peran atas seluruh layar Rawat Inap. Seluruh prasyaratnya kini selesai: `FE-RWI-001` s.d. `FE-RWI-018` ada, dan tiga aturan tombol yang menyentuh kewenangan sudah punya layarnya masing-masing
