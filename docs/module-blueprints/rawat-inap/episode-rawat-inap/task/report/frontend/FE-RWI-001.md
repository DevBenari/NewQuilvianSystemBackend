# FE-RWI-001 — Admin dapat kembali menutup tempat tidur yang rusak

- TASK ID: `FE-RWI-001`
- TASK TYPE: Perbaikan frontend, dengan perluasan wewenang UI yang diberikan pemilik pekerjaan pada sesi 26 Agustus 2026
- COMPLEXITY: `MEDIUM`
- CLASSIFICATION SCORE: 8 — dua repository (backend read-only, frontend target tulis) 2; lebih dari 20 berkas diperiksa 2; delapan berkas diubah 1; logika moderat 1; mengonsumsi kontrak API yang sudah ada 1; database 0; security/auth 0; satu layar terbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`, lalu diperluas menjadi lintas repository ketika pemilik pekerjaan meminta cacat backend yang ditemukan ikut diperbaiki
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. `NewQuilvianSystemBackend` (branch `MHamzah`) semula hanya dibaca; sesudah pemilik pekerjaan memberi izin, dua berkas source backend ikut diubah untuk menutup cacat `PUT /beds/{id}`. Selebihnya tulisan ke backend hanya dokumen
- FILES INSPECTED: `AGENTS.md` dan seluruh `.codex/` frontend; roadmap frontend Rawat Inap bagian 1–4; `requirement-traceability.md`; laporan blokir `BE-RWI-006`; `BedController.cs` dan `BedDtos.cs` backend; `ApiResponse.cs`; slice, hook detail, hook editor, constants, utils, view daftar, view detail, base detail view, `Hero`, `BaseButton`, `ConfirmModal`, `InstanceAxios`, `private-route-token-utils`, pola `clinic-detail-view.jsx` dan `employee-relation-master-detail-view.jsx`, serta `tests/unit/` dan `tests/e2e/` yang sudah ada
- FILES CHANGED (backend, atas izin eksplisit): `Areas/HealthServices/MasterData/DTOs/BedDtos.cs`; `Areas/HealthServices/MasterData/Controllers/BedController.cs`
- FILES CHANGED (frontend): `src/components/view/health-services/master-data/bed/detail/bed-detail-view.jsx`; `src/lib/hooks/health-services/master-data/bed/use-master-data-bed-detail.jsx`; `src/lib/constants/health-services/master-data/bed-constants.jsx`; `src/utils/health-services/master-data/bed-utils.jsx`; `tests/unit/inpatient-bed-status.test.mjs`; `tests/e2e/bed-status-toggle.spec.mjs` (baru); `tests/support/module-alias-loader.mjs` (baru); `tests/support/instance-axios-double.mjs` (baru)

## Keadaan yang ditemukan di awal

Perbaikan URL pada slice tempat tidur **sudah ada** di repository frontend sejak commit
`75d174db2`. Kedua aksi memang sudah memanggil `PATCH /v1/health-services/master-data/beds/{id}/status`.
Yang belum pernah diperiksa adalah apakah tombolnya benar-benar ada di layar. Ternyata tidak.

Dua celah membuat outcome task ini belum tercapai. Keduanya baru terlihat setelah layarnya
ditelusuri, bukan dari membaca slice saja.

| Celah | Buktinya | Akibatnya bagi admin |
| --- | --- | --- |
| **Tidak ada tombol aktifkan/nonaktifkan** | `bed-detail-view.jsx` tidak pernah mengambil `handleToggleActive` dari hook-nya. `health-services-master-data-detail-view.jsx` hanya mengenal aksi Kembali, Perbarui, dan Hapus. Daftar tempat tidur hanya menampilkan `StatusBadge` yang tidak dapat diklik | Tidak ada satu pun jalan dari layar untuk menonaktifkan tempat tidur rusak. `handleToggleActive` adalah kode mati |
| **Menyimpan form Perbarui menghidupkan kembali tempat tidur nonaktif** | `buildBedPayload` tidak mengirim `isActive`. `UpdateBedRequest.IsActive` pada backend bernilai bawaan `true`, dan `BedController.cs:456` menugaskannya langsung ke entity | Sekali admin membetulkan nama atau nomor tempat tidur, penonaktifan sebelumnya hilang diam-diam. **Ditutup di kedua sisi** — lihat KNOWN ISSUES |

Roadmap menulis wewenang UI task ini "Tidak ada", karena penulisnya berasumsi tombolnya sudah ada
dan hanya URL-nya yang salah. Asumsi itu tidak sesuai kenyataan source. Temuan ini disampaikan
lebih dulu, dan pemilik pekerjaan **memperluas wewenang UI** agar task ini dikerjakan penuh.

- IMPLEMENTATION: (1) Layar detail tempat tidur kini menampilkan tombol **Nonaktifkan**/**Aktifkan** pada baris aksi, mengikuti pola `renderHero` yang sudah dipakai `clinic-detail-view.jsx`, sehingga komponen bersama tidak diubah dan layar master lain tidak terpengaruh. (2) Tombol itu membuka `ConfirmModal`, mengikuti satu-satunya pola sejenis yang sudah ada di repository, yaitu `employee-relation-master-detail-view.jsx` milik HR. (3) Hook detail mendapat `showStatusConfirm`, `openStatusConfirm`, dan `closeStatusConfirm`; `handleToggleActive` yang sudah ada dipakai apa adanya sebagai aksi konfirmasi dan menutup modal setelah berhasil. (4) `isActive` kini ikut terbawa pada `BED_FORM_DEFAULTS`, `mapBedToForm`, dan `buildBedPayload`, sehingga form Perbarui mengirim balik nilai yang sedang berlaku. `BED_FORM_FIELDS` tidak ditambah, jadi tidak ada isian baru yang muncul di layar — nilainya dibawa, bukan ditanyakan.
- API CONTRACT IMPACT: **Ada satu delta yang perlu dikunci pemilik kontrak.** `contracts/api-contract.md` bagian 7 menyatakan "seluruh endpoint lain pada grup ini" tidak berubah, padahal `PUT /beds/{id}` kini menerima `isActive` sebagai opsional. Perubahannya bersifat menambah keluwesan, bukan memutus consumer: yang mengirim `isActive` tetap dituruti, yang tidak mengirim kini membiarkan status apa adanya — sebelumnya diam-diam dijadikan aktif. Kontraknya **tidak saya ubah** karena berversi `0.4.0` dan tunduk gerbang persetujuan; deltanya dicatat pada `requirement-traceability.md` untuk diputuskan pemiliknya. Baris ke-50, perubahan perilaku `PATCH /beds/{id}/availability`, tidak disentuh.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Tombol baru tunduk pada `AccessPermission("Bed", "Update")` yang sudah berlaku di `BedController.cs`.
- VISUAL REFERENCE: NOT REQUIRED — tidak ada mockup yang disediakan; tata letak mengikuti pola aksi detail master data yang sudah baku di repository.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Tombol aktifkan memanggil `/status`, bukan `/activate` | LULUS | e2e mencatat satu-satunya panggilan PATCH ke `/{id}/status`; nol panggilan ke `/activate` |
| 2. Tombol nonaktifkan memanggil endpoint yang sama, bukan `/deactivate` | LULUS | Sama; body `{"isActive":false}` terbaca apa adanya dari request |
| 3. Keduanya berhasil dan status di layar berubah tanpa muat ulang halaman | LULUS | Label tombol berubah `Nonaktifkan` menjadi `Aktifkan`, baris Status berubah `Aktif` menjadi `Nonaktif`, dan penanda `window.__bedScreenNotReloaded` yang dipasang sebelum klik **masih ada** sesudahnya |
| 4. Tidak ada lagi permintaan yang mengembalikan 404 dari layar ini | LULUS | e2e melayani hanya rute yang benar-benar ada pada `BedController` dan sengaja membalas 404 untuk rute tempat tidur lain; nol respons 404 tercatat sepanjang alur |

- VALIDATION: `npm run test:unit` | PASS, 19/19 | TASK | seluruh unit test repository, termasuk enam test tempat tidur
- VALIDATION: `node --test tests/unit/inpatient-bed-status.test.mjs` | PASS, 6/6 | TASK | thunk asli dieksekusi terhadap axios tiruan; URL, method, dan body terbaca; reducer asli memperbarui `bedDetail`; `mapBedToForm` lalu `buildBedPayload` membawa `isActive` apa adanya
- VALIDATION: uji mutasi pada slice tempat tidur | PASS | TASK | `/status` sengaja dikembalikan menjadi `/activate`; 3 dari 4 test saat itu **gagal**, membuktikan test tidak kosong. Berkas dipulihkan dari salinan dan `git status` kembali bersih
- VALIDATION: e2e `tests/e2e/bed-status-toggle.spec.mjs` di browser sungguhan | PASS, 1/1 dalam 3,6 detik | TASK | buka daftar, klik dua kali baris, masuk detail, tekan Nonaktifkan, konfirmasi; seluruh kriteria 1 sampai 4 diperiksa dalam satu alur
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK | seluruh repository, tanpa error
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | dijalankan setelah seluruh perubahan source selesai; `.next/standalone` terbentuk
- VALIDATION: `git diff --check` | PASS | TASK | tidak ada whitespace error
- VALIDATION: paritas rute layar tempat tidur terhadap `BedController.cs` | PASS | TASK | sepuluh pemanggilan frontend — daftar, `filters/metadata`, `summary`, `options`, detail, tambah, ubah, hapus, dan dua kali `status` — seluruhnya punya rute pada controller. Tidak ada kandidat 404 yang tersisa
- VALIDATION: pemanggilan rute terhadap backend yang benar-benar menyala | PASS | TASK | aplikasi milik pemilik pekerjaan pada `https://127.0.0.1:7184`, `GET /health` → **200**. Tanpa token: `PATCH /beds/{id}/status` → **401** (rute ada, `[Authorize]` tegak), `PATCH /beds/{id}/availability` → **401**, sedangkan `PATCH /beds/{id}/activate` dan `PATCH /beds/{id}/deactivate` → **404**. Ini bukti langsung bahwa dua endpoint lama yang dipanggil kode sebelum perbaikan memang tidak pernah ada
- VALIDATION: `npx playwright test` dengan konfigurasi bawaan repository | NOT RUN | ENVIRONMENT ISSUE | binary browser yang terpasang di mesin ini build `1228`, sedangkan Playwright pada `node_modules` meminta build `1200`. e2e dijalankan memakai Microsoft Edge yang sudah ada di sistem lewat konfigurasi sementara di luar repository; konfigurasi itu tidak ikut menjadi diff
- VALIDATION: `dotnet msbuild QuilvianSystemBackend.csproj -t:Compile -p:Configuration=Debug` | PASS, exit 0 | TASK | perubahan `bool?` pada `UpdateBedRequest` terkompilasi bersih
- VALIDATION: pencarian `UpdateBedRequest` dan `UpdateBed` pada project test | PASS | TASK | nol rujukan, sehingga perubahan tipe ini tidak dapat mematahkan test yang sudah ada
- VALIDATION: `dotnet build` penuh dan `dotnet test` | NOT RUN | ENVIRONMENT ISSUE | aplikasi backend sedang menyala di mesin ini (PID 12220) dan mengunci `bin/Debug/net9.0/QuilvianSystemBackend.exe`, sehingga langkah salin keluaran gagal dengan MSB3027/MSB3021. **Bukan galat kompilasi** — tidak ada satu pun `error CS` pada log. Perlu diulang sesudah aplikasi ditutup

## Cara menjalankan ulang e2e-nya

Repository belum punya `playwright.config`, sehingga `npm run test:e2e` akan ikut menyapu berkas
unit test. Untuk menjalankan ulang bukti di atas:

1. `npm run build`.
2. Jalankan server hasil build pada port 3710, dengan variabel `SESSION_SIGNING_SECRET` berisi minimal 32 karakter.
3. Jalankan Playwright memakai konfigurasi berisi `testDir: "./tests/e2e"` dan `use: { channel: "msedge" }`, menunjuk berkas `bed-status-toggle.spec.mjs`, dengan `SESSION_SIGNING_SECRET` yang sama.

e2e ini **tidak** menyentuh backend maupun database. Seluruh jawaban API dilayani di dalam browser.

- WARNINGS: (1) e2e memakai API tiruan, bukan backend sungguhan. Yang terbukti adalah perilaku layar, bentuk request, dan tidak adanya 404 — bukan bahwa database tim menerima perubahan status. (2) Repository frontend belum punya `playwright.config`, sehingga `npm run test:e2e` apa adanya belum dapat diandalkan. Hal itu di luar scope task dan tidak diperbaiki.
- KNOWN ISSUES: **Cacat backend `PUT /beds/{id}` sudah diperbaiki**, atas izin pemilik pekerjaan pada 26 Agustus 2026. `UpdateBedRequest.IsActive` berubah dari `bool` berbawaan `true` menjadi `bool?` tanpa bawaan, dan controller kini menulis `entity.IsActive = request.IsActive ?? entity.IsActive`. Artinya: consumer yang tidak mengirim `isActive` membiarkan status apa adanya, sedangkan yang mengirimnya tetap dituruti. Perilaku ini tidak memutus consumer mana pun yang sudah ada, karena sebelumnya field itu memang selalu dianggap terkirim.
- MANUAL TEST: NOT FEASIBLE — verifikasi terhadap backend sungguhan memerlukan akun login berhak `Bed.Update` dan database tim; keduanya tidak tersedia, dan menonaktifkan tempat tidur sungguhan pada database bersama tidak diminta task ini. Perilaku tombolnya tetap diverifikasi di browser sungguhan lewat e2e di atas.
- INCIDENTAL CHANGES: `test-results/.last-run.json` sempat berubah karena Playwright dijalankan, lalu dipulihkan dengan `git checkout --` pada berkas itu saja. Folder hasil percobaan e2e yang gagal dan konfigurasi Playwright sementara sudah dihapus. Diff akhir hanya berisi berkas yang memang menjadi scope task.
- INTERRUPTIONS: NONE
- GIT STATUS: Frontend — empat berkas source dan satu berkas test berubah, ditambah tiga berkas baru (`tests/e2e/bed-status-toggle.spec.mjs`, `tests/support/module-alias-loader.mjs`, `tests/support/instance-axios-double.mjs`). Backend — dua berkas source (`BedDtos.cs`, `BedController.cs`) ditambah tiga dokumen: laporan ini, `roadmap/frontend-roadmap.md`, dan `roadmap/requirement-traceability.md`. Seluruhnya **belum di-stage dan belum di-commit**. Tidak ada stage, commit, push, pull, merge, rebase, atau deploy yang dilakukan.
- NEXT RECOMMENDED STEP: Tutup aplikasi backend yang sedang menyala, jalankan `dotnet build` dan `dotnet test` sekali lagi supaya perubahan `bool?` punya bukti test penuh, lalu commit dan push kedua repository. Sesudah itu **baru** buka `BE-RWI-006`. Selama perubahan ini belum rilis, jalan keluar admin belum ada di lingkungan mana pun, dan mencabut wewenang admin atas `Reserved` serta `Occupied` lebih dulu akan meninggalkan tempat tidur rusak tanpa cara menutupnya.
