# FE-RWI-003 — Admin dapat mengubah pengaturan Rawat Inap

- TASK ID: `FE-RWI-003`
- TASK TYPE: Implementasi layar frontend
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 9 — dua repository 2; lebih dari 20 berkas diperiksa 2; delapan berkas diubah 1; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh penjagaan akses tanpa mengubahnya 1; satu layar terbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. `NewQuilvianSystemBackend` (branch `MHamzah`) hanya dibaca; tulisan ke backend terbatas pada laporan ini dan penanda status pada roadmap frontend
- TASK/CONTRACT VERSION: roadmap frontend revision `2` (`APPROVED` 24 Agustus 2026); api contract `0.4.0` bagian Inpatient Setting, berstatus ✅ **Tersedia** sejak 26 Agustus 2026
- FILES INSPECTED: roadmap frontend bagian 1–4; api contract bagian Inpatient Setting; `InpatientSettingController.cs`, `InpatientSettingService.cs`, dan `InpatientSettingDtos.cs` backend; fondasi `FE-RWI-002` (`inpatient-api.service.js`, `inpatient-setting.service.js`, `inpatient-management-slice.jsx`, store); `BaseEditorView`, `BaseEditorForm`, `BaseEditorField`, `base-form-control.jsx`, `HealthServicesMasterDataEditorView`, `AccessDeniedGate`, `access-denied-utils.jsx`, `InformationAlert`, `ToastStack`; `menu-items.jsx`, `left-sidebar-items-virtualized.jsx`, `filter-menu-items-by-role.jsx`, `route-guard.jsx`, `route-guard-link.js`; pola layar master data tempat tidur dan klinik
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-setting-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-setting-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-setting.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-setting-view.jsx` (baru); `src/app/health-services/inpatient-management/settings/page.jsx` (baru); `tests/unit/inpatient-setting.test.mjs` (baru); `tests/e2e/inpatient-setting.spec.mjs` (baru); `src/utils/menu-sidebar/menu-items.jsx` (diubah)

## Yang dibangun

Layar `/health-services/inpatient-management/settings` membaca satu baris pengaturan dari
`GET /master-data/inpatient-settings` dan menyimpannya kembali lewat `PUT /{id}`. Menu **Rawat
Inap** yang tadinya satu tautan tunggal kini menjadi induk dengan dua anak: **Beranda Rawat Inap**
(route lama, tidak berubah) dan **Pengaturan Rawat Inap**.

Batas angka pada layar disalin apa adanya dari `UpdateInpatientSettingRequest`: pemesanan tempat
tidur 1–1440 menit, keempat nilai jam 1–720 jam, nama paling panjang 150 karakter, awalan nomor
episode 20 karakter, catatan 1000 karakter. Pemeriksaan di layar hanya menyaring kesalahan yang
sudah jelas sebelum permintaan dikirim; keputusan akhir tetap milik server.

- IMPLEMENTATION: (1) Layar memakai `HealthServicesMasterDataEditorView` bermode `update`, sehingga tata letak, keadaan loading, dan tumpukan notifikasinya sama dengan seluruh layar master data lain. (2) Seluruh panggilan lewat `inpatientSettingService` milik fondasi `FE-RWI-002`, bukan `InstanceAxios` langsung dan bukan `fetch`. Keadaannya disimpan pada slice fondasi `inpatientManagement.resources.inpatientSettings`. (3) Jawaban 404 dari `GET` diperlakukan sebagai "master belum terisi", bukan layar rusak: pesan server ditampilkan lengkap dengan tombol **Muat ulang**, dan tombol simpan disembunyikan karena memang belum ada yang bisa disimpan. (4) Kegagalan **membaca** dan kegagalan **menyimpan** sengaja dipisah. Hanya kegagalan membaca yang boleh mengganti seluruh isi halaman dengan layar Akses Ditolak; kegagalan menyimpan tampil sebagai peringatan di dalam form. Tanpa pemisahan ini, satu pesan penolakan yang kebetulan memuat kata "hak akses" akan mengganti halaman dan ikut menghapus isian petugas.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Mengonsumsi dua endpoint `Inpatient Setting` pada api contract `0.4.0` apa adanya.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication. Layar bergantung penuh pada `AccessPermission("InpatientSetting", "Read"/"Update")` di server; frontend hanya menampilkan penolakannya.
- VISUAL REFERENCE: NOT REQUIRED — roadmap menyatakan tata letak dan pengelompokan isian bebas. Yang dipakai adalah pola editor master data yang sudah baku di repository, tanpa komponen baru.
- WEWENANG UI YANG DIPAKAI: "Tata letak dan pengelompokan isian bebas" dari baris **Wewenang UI** roadmap. Isian disusun datar dalam satu kolom, mengikuti layar master data lain. Tidak ada pengelompokan bagian, warna, atau komponen baru yang diperkenalkan.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Kedelapan nilai pengaturan terbaca dan dapat diubah | **LULUS** | e2e membaca ketujuh isian teks/angka langsung dari DOM dengan nilai server; test unit membuktikan kesembilan isian layar punya pasangan di payload, tidak ada yang tertinggal |
| 2. Hanya peran admin master data yang dapat membuka; peran lain tidak melihat menunya | **SEBAGIAN** | Separuh pertama terpenuhi lewat server: 403 dari backend memunculkan layar Akses Ditolak. Separuh kedua **tidak dapat dipenuhi hari ini** — lihat bagian di bawah |
| 3. Perubahan yang gagal menampilkan pesan server apa adanya, bukan kalimat umum | **LULUS** | e2e menolak penyimpanan dengan kalimat asli `InpatientSettingService`, dan kalimat itu yang muncul di layar — bukan "Gagal menyimpan pengaturan Rawat Inap." |
| 4. Isian yang sudah diketik tidak hilang ketika penyimpanan ditolak | **LULUS** | e2e mengisi 90 menit dan satu catatan, penyimpanan ditolak, kedua nilai **masih ada**. Diuji balik dengan mutasi: begitu jalur gagal dibuat mengembalikan form ke nilai server, test langsung gagal dengan `Expected "90" / Received "120"` |

## Kenapa kriteria 2 baru separuh

Roadmap menuntut dua hal berbeda, dan hanya satu yang punya mesinnya di frontend hari ini.

| Tuntutan | Keadaan | Buktinya |
| --- | --- | --- |
| Peran lain tidak dapat **memakai** layar | Terpenuhi, dijaga server | `[AccessPermission("InpatientSetting", "Read")]` menolak dengan 403; `AccessDeniedGate` mengubahnya menjadi layar "Akses Ditolak" |
| Peran lain tidak **melihat menunya** | **Belum ada mesinnya** | `filter-menu-items-by-role.jsx` menyaring apa pun hanya untuk `Admin` dan `Manajer` — dan itu pun mengembalikan seluruh menu. Aturan per peran di dalamnya **dikomentari**, sehingga peran mana pun melihat menu yang sama |

Frontend juga tidak punya data hak akses per butir untuk pengguna yang sedang login. Yang tersedia
hanya dua penanda kasar, `canAccessWorkforceModules` dan `canAccessQueueDisplayRuntime`. Tidak ada
`InpatientSetting : Read` di sisi layar, jadi tidak ada yang bisa dipakai menyembunyikan menu.

Ada satu hal lagi yang perlu diketahui pemilik roadmap: `route-guard-link.js` memberi akses penuh
kepada `Admin` dan `Manajer`, menolak seluruh peran yang tidak dikenal, dan untuk `Dokter` serta
`Perawat` hanya memblokir empat path lama (`/MasterData`, `/farmasi`, `/Optik`, `/pendaftaran`).
Tidak satu pun cocok dengan `/health-services/...`, sehingga **Dokter dan Perawat dapat membuka
route layar ini**. Mereka tetap ditolak server saat layarnya menarik data, jadi datanya aman —
tetapi "tidak dapat membuka" dalam arti route belum berlaku.

Menambahkan path Rawat Inap ke daftar itu **sengaja tidak dilakukan**: daftar tersebut menghardcode
nama peran, sedangkan sumber kebenaran hak akses adalah mesin permission backend. Menebak bahwa
"Dokter tidak boleh melihat pengaturan Rawat Inap" berarti mengarang kebijakan rumah sakit, dan
laporan `BE-RWI-008` sudah mencatat bahwa nama peran di sistem ini adalah asumsi yang belum
diverifikasi. Yang dibutuhkan adalah penyaring menu berbasis hak akses yang benar-benar dibaca dari
server — pekerjaan lintas layar untuk seluruh 467 route, bukan bagian dari satu layar pengaturan.
Ini prasyarat nyata bagi `FE-RWI-019`, yang justru bertugas membuktikan "layar hanya dijangkau
peran yang berhak".

- VALIDATION: e2e `tests/e2e/inpatient-setting.spec.mjs` di browser sungguhan | PASS, 2/2 | TASK | jalur berhasil dan jalur gagal keduanya diperiksa dalam satu alur, ditambah keadaan master belum terisi
- VALIDATION: uji mutasi kriteria 4 | PASS | TASK | jalur gagal sengaja diubah menjadi `setForm(mapInpatientSettingToForm(item))`, aplikasi dibangun ulang, dan e2e **gagal** persis pada isian yang kembali ke 120. Hook dipulihkan dari salinan dan dibangun ulang
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-setting.test.mjs` | PASS, 6/6 | TASK | pembacaan sembilan nilai, kelengkapan payload, kesamaan batas angka dengan backend, pesan server apa adanya, jalur gagal tidak menyentuh `setForm`, dan larangan jalur HTTP baru
- VALIDATION: seluruh unit test kecuali satu berkas yang rusak sejak merge | PASS, 49/49 | TASK | `auth-security.test.mjs` dikecualikan — lihat KNOWN ISSUES
- VALIDATION: e2e `bed-status-toggle.spec.mjs` milik `FE-RWI-001` | PASS, 1/1 | TASK | dijalankan ulang sesudah merge dan sesudah perubahan menu; layar tempat tidur tidak terpengaruh
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK | seluruh repository, tanpa error
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/settings` terbaca pada keluaran build
- VALIDATION: `git diff --check` | PASS | TASK | tidak ada whitespace error
- VALIDATION: `npm run test:unit` | NOT RUN | EXISTING ISSUE | script-nya sendiri rusak sejak merge `6bba90ae1` — lihat KNOWN ISSUES
- VALIDATION: `npx playwright test` dengan konfigurasi bawaan repository | NOT RUN | ENVIRONMENT ISSUE | repository belum punya `playwright.config`, dan binary browser Playwright yang terpasang build `1228` sedangkan `node_modules` meminta `1200`. e2e dijalankan lewat Edge sistem memakai konfigurasi sementara yang dihapus setelah selesai

## Verifikasi manual

- MANUAL TEST: NOT FEASIBLE — menekan tombolnya terhadap backend sungguhan memerlukan akun login berhak `InpatientSetting : Update`, dan akun itu tidak tersedia. Rute-nya sendiri sudah dipastikan hidup: `GET /api/v1/health-services/master-data/inpatient-settings` pada aplikasi backend yang menyala menjawab **401** tanpa token, artinya rute ada dan otorisasi tegak. Seluruh perilaku layar — pembacaan sembilan nilai, penyuntingan, penolakan server, bertahannya isian, dan keadaan master belum terisi — diverifikasi di browser sungguhan lewat e2e dengan API tiruan.

## Delta terhadap roadmap

| Butir | Roadmap | Kenyataan | Alasan |
| --- | --- | --- | --- |
| Jumlah nilai pengaturan | "Kedelapan nilai" | `UpdateInpatientSettingRequest` punya **sembilan** field yang dapat diubah: nama, lima angka, awalan nomor, penanda aktif, dan catatan | Kesembilannya ditampilkan supaya kriteria 1 terpenuhi dengan hitungan mana pun |
| Reuse state | "Pola layar master data pada `state/slice/health-services/master-data/`" | Memakai slice fondasi `FE-RWI-002` | Roadmap `FE-RWI-002` menyatakan fondasi itu dibuat agar layar berikutnya tidak menemukan ulang cara menyimpan keadaan. Membuat slice ke-28 justru akan meninggalkan fondasi itu tak terpakai |

- WARNINGS: (1) e2e memakai API tiruan. Yang terbukti adalah perilaku layar dan bentuk permintaan, bukan bahwa database tim menerima perubahan. (2) Baris pengaturan mungkin belum ada di database tim; seeder-nya milik `BE-RWI-002` yang masih 🟡. Layar sudah menyiapkan keadaan itu dan menampilkan pesan server beserta jalan keluarnya.
- KNOWN ISSUES: **Tiga hal yang ditemukan, tidak satu pun berasal dari task ini.** (1) `npm run test:unit` **tidak dapat dijalankan sama sekali** sejak merge `6bba90ae1`: script-nya berubah menjadi `--test tests/unit/` dan resolver baru `tests/helpers/alias-resolver.mjs` menolak import direktori dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Bentuk glob `tests/unit/*.test.mjs` berjalan normal. (2) `tests/unit/auth-security.test.mjs` mengimpor `src/utils/auth/base-login-utils.jsx`, sedangkan berkasnya bernama `.js`. Berkas `.jsx` itu tidak pernah ada, baik sebelum maupun sesudah merge. (3) `tests/e2e/route-smoke.spec.mjs` menuntut tepat 219 route, sedangkan repository kini punya **467**. Angka itu sudah lama basi, jauh sebelum task ini menambah satu route. Ketiganya tidak saya sentuh karena berada di luar scope task dan pemiliknya bukan modul Rawat Inap.
- RISIKO: Kriteria 2 belum utuh, dan penyebabnya bukan layar ini melainkan ketiadaan penyaring menu berbasis hak akses di seluruh frontend. Selama itu belum ada, setiap layar Rawat Inap berikutnya akan mewarisi kekurangan yang sama, dan `FE-RWI-019` tidak akan dapat membuktikan tugasnya.
- DEPENDENCY BACKEND: `BE-RWI-005` — dua endpoint `Inpatient Setting` terbukti hidup (401 tanpa token pada aplikasi yang menyala, dan ✅ `Tersedia` pada api contract `0.4.0`). Tidak ada perubahan backend yang dibutuhkan maupun dilakukan.
- INCIDENTAL CHANGES: `test-results/.last-run.json` sempat berubah karena Playwright dijalankan, lalu dipulihkan dengan `git checkout --` pada berkas itu saja. Konfigurasi Playwright sementara dan folder hasil percobaan e2e sudah dihapus.
- INTERRUPTIONS: Repository berubah dari luar sesi ini di tengah pengerjaan — commit `44d658a15` (pekerjaan `FE-RWI-001` di-commit pemilik pekerjaan) dan merge `6bba90ae1` dari branch `origin/rizkiV2`. Sesuai aturan pemulihan, Git status dan diff diperiksa, tidak ada pekerjaan yang digandakan atau dibatalkan, lalu aplikasi dibangun ulang dan seluruh e2e dijalankan ulang di atas kondisi hasil merge. Ketiganya lulus.
- GIT STATUS: Tujuh berkas baru dan satu berkas diubah (`menu-items.jsx`), seluruhnya **belum di-stage dan belum di-commit**. Backend hanya menerima laporan ini dan penanda status pada `roadmap/frontend-roadmap.md`. Tidak ada stage, commit, push, pull, merge, rebase, atau deploy yang dilakukan.
- NEXT RECOMMENDED STEP: Putuskan lebih dulu kriteria 2 — apakah penyaring menu berbasis hak akses dijadikan task tersendiri sebelum `FE-RWI-019`, atau kalimat kriterianya diturunkan menjadi "peran lain ditolak server". Sesudah itu `FE-RWI-004` dapat langsung dikerjakan: dependency-nya sama persis dan sudah terpenuhi.
