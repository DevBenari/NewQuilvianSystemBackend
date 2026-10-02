# FE-RWI-013 — Kasir dapat menandai kelayakan keuangan

- TASK ID: `FE-RWI-013`
- TASK TYPE: Implementasi layar frontend baru beserta aturan tampil per peran
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 10 — dua repository 2; lebih dari 20 berkas diperiksa 2; lebih dari 8 berkas diubah 2; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menampilkan penjaga peran tanpa mengubahnya 1; alur berbatas pada satu layar 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2` (upstream `origin/HamzahV2`). Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /discharges/{episodeId}/financial-clearance` dan `GET /discharges/{episodeId}/closure-readiness` keduanya berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-013` dan `BE-RWI-024`; `03-frontend-architecture.md` bagian 2, 3, 5.3, dan 5.4; `contracts/api-contract.md` bagian Inpatient Discharge; `contracts/validation-matrix.md` bagian 6 dan 7; `contracts/state-transition-matrix.md` bagian 4 beserta 4.1; `data/data-dictionary.md` bagian 15; `00-interview-decisions.md` `RWI-DEC-015` dan `RWI-DEC-040`; [laporan `BE-RWI-024`](../backend/be-rwi-024-kelayakan-keuangan.md); `requirement-traceability.md`; `InpatientDischargeController.cs` bagian `MarkFinancialClearance` dan `GetClosureReadiness`; `InpatientClosureDtos.cs` (`MarkFinancialClearanceRequest`, `FinancialClearanceEntryResponse`, `FinancialClearanceResponse`, `ClosureConditionResponse`, `ClosureReadinessResponse`); `InpFinancialClearanceStatus.cs`; `InpDischargeService.Closure.cs` baris 210–400 dan 610–700 tempat `RWI-RULE-028` dan `BuildClosureConditionsAsync` dibentuk; `InpatientActorClaims.cs` bagian `CashierOrBillingRoles`; `AGENTS.md` frontend beserta enam dokumen `.codex`; `inpatient-api.service.js`; `inpatient-discharge.service.js`; `use-inpatient-discharge.jsx`; `inpatient-discharge-view.jsx`; `inpatient-episode-utils.jsx`; `inpatient-episode-constants.jsx`; `base-form-control.jsx`; `filter-select.jsx`; `status-badge.jsx`; `tests/e2e/inpatient-discharge.spec.mjs`; `tests/unit/inpatient-discharge.test.mjs`
- FILES CHANGED:
  - **Baru** `src/lib/constants/health-services/inpatient-management/inpatient-financial-clearance-constants.jsx`
  - **Baru** `src/utils/health-services/inpatient-management/inpatient-financial-clearance-utils.jsx`
  - **Baru** `src/lib/hooks/health-services/inpatient-management/use-inpatient-financial-clearance.jsx`
  - **Baru** `src/components/view/health-services/inpatient-management/inpatient-financial-clearance-view.jsx`
  - **Baru** `src/app/health-services/inpatient-management/episodes/[id]/financial-clearance/page.jsx`
  - **Baru** `tests/unit/inpatient-financial-clearance.test.mjs`
  - **Baru** `tests/e2e/inpatient-financial-clearance.spec.mjs`
  - **Diubah** `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` (route builder layar kelayakan keuangan)
  - **Diubah** `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` (`isCashierOrBilling` pada konteks pelaku)
  - **Diubah** `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` (satu tautan menuju layar kelayakan keuangan)

## 1. Kriteria 3 terhalang kontrak, bukan terhalang pekerjaan frontend

Ini temuan terpenting task ini, dan ia ditemukan sebelum satu baris kode ditulis.

Roadmap kriteria 3 menuntut *"Pelaku dan waktu penandaan **terbaca** pada layar detail"*. Kontrak
`0.4.0` hanya menyediakan `POST .../financial-clearance`. **Tidak ada `GET`-nya.**

| Yang ada | Yang dapat dibaca darinya |
| --- | --- |
| `GetFinancialClearanceAsync` pada `InpDischargeService.Closure.cs:222` | Nilai terkini, riwayat lengkap, pelaku, waktu, catatan — tetapi method ini **tidak pernah dipasang sebagai aksi controller**. Ia hanya dipakai internal sesudah `POST` dan di dalam `closure-readiness` |
| `GET .../closure-readiness` syarat `FINANCIAL_CLEARED` | Hanya `IsSatisfied` boolean beserta satu kalimat umum. **Tidak** dapat membedakan `Pending` dari `Blocked`, dan tidak memuat pelaku, waktu, maupun catatan |
| Jawaban `POST .../financial-clearance` | Riwayat **lengkap**, termasuk baris yang ditulis sebelum layar dibuka |

Akibatnya: kasir yang membuka layar pagi ini tidak dapat membaca penandaan yang ia buat kemarin.
Satu-satunya jalan membacanya adalah mengirim `POST` — dan itu **menulis baris riwayat baru**,
sehingga bukan pilihan sama sekali.

**`BE-RWI-024` kriteria 3 sendiri berbunyi "Pelaku dan waktu *tersimpan*", dan itu memang
terpenuhi.** Yang tidak pernah dibuat adalah jalan membacanya.

### Yang dikerjakan layar sebagai gantinya

Pilihan yang paling berbahaya di sini adalah merender daftar riwayat kosong. Kasir akan
membacanya sebagai *"episode ini belum pernah ditandai"*, padahal artinya *"penandaannya tidak
dapat dibaca dari sini"* — dan ia akan menandai ulang episode yang sudah benar.

Karena itu ketiadaan data dan data yang kosong dirender **berbeda**:

- Sebelum ada penandaan dikirim dari layar ini: kalimat yang menyatakan riwayatnya tidak dapat
  dibaca, beserta alasannya.
- Sesudah `POST` berhasil: riwayat lengkap dari jawaban server, termasuk baris-baris lama.
- Bila server menjawab riwayat yang benar-benar kosong: *"Belum ada penandaan yang tercatat pada
  episode ini."*

Hal yang sama berlaku pada nilai terkini: selama belum ada penandaan, layar menyatakan apa adanya
bahwa ia hanya dapat membedakan lunas dari belum lunas.

## 2. Tiga nilai terbaca, dua dapat ditandai

Roadmap kriteria 4 menyebut *"Tiga nilai tersedia dan artinya terbaca jelas"*.
`state-transition-matrix.md` bagian 4 hanya mengenal **dua tindakan** — "Tandai lunas" dan "Tandai
tertahan" — dan tidak punya satu pun perpindahan yang kembali ke `Pending`. `Pending` adalah
keadaan awal setiap episode, bukan sesuatu yang ditandai kasir.

Backend sendiri lebih longgar: `Enum.IsDefined` menerima nilai `0` juga, sehingga `POST` berisi
`clearanceStatus: 0` akan **berhasil** dan menulis baris riwayat `Pending`.

Layar mengikuti matriks yang lebih ketat. Ketiga nilai tetap **terbaca** — masing-masing punya
penjelasannya sendiri di layar, dan ketiganya muncul pada daftar pilihan — tetapi `Pending`
dinonaktifkan dan diberi keterangan "keadaan awal, tidak ditandai". Menulis baris riwayat `Pending`
akan membuat riwayat memuat perpindahan yang tidak pernah disahkan matriks.

**Delta yang perlu diputuskan.** Roadmap menuntut tiga nilai tersedia; matriks perpindahan
menyediakan dua tindakan; backend menerima tiga. Salah satu dokumen perlu dikoreksi. **Owner:
Product/Domain bersama pemilik kontrak.**

- IMPLEMENTATION: (1) Aksi penandaan **tidak dirender** bagi siapa pun selain kasir dan billing — termasuk petugas admisi, kepala ruangan, dan DPJP yang pada aksi lain di modul yang sama justru berwenang. (2) Nama peran disalin apa adanya dari `InpatientActorClaims.CashierOrBillingRoles`, dan pencocokannya tidak peka huruf besar-kecil. (3) Peran kasir dibaca lewat `getEpisodeActorContext` yang sudah ada, **bukan** lewat pembacaan klaim peran kedua — dua tempat yang membaca peran sendiri-sendiri akan berselisih diam-diam, dan kesalahannya hanya terlihat sebagai aksi yang hilang tanpa galat apa pun. (4) Catatan diperiksa di layar sebelum permintaan dikirim, setara `RWI-RULE-028` aturan 4. (5) Nilai kelayakan keuangan tidak punya nilai bawaan; `BaseSelectField` menambahkan baris kosong sendiri. (6) Episode `Closed` dan `Cancelled` menutup penandaan bagi kasir sekalipun, setara `GuardEpisodeNotClosedAsync`. (7) Penjaga `markInFlight` menahan klik kedua. (8) Peringatan `RWI-RISK-003` dirender **tanpa syarat apa pun** dan tidak hilang bahkan ketika nilainya sudah lunas; tiap baris riwayat juga membawa penanda "Penandaan manual" sendiri.
- API CONTRACT IMPACT: Tidak mengubah kontrak. Muatan memakai nama kolom `MarkFinancialClearanceRequest` (`clearanceStatus`, `note`) apa adanya, dan hanya dua kolom itu. Pelaku dan waktu tidak pernah dikirim layar — server menurunkannya dari pengguna yang terautentikasi.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization. Menyembunyikan aksi yang pasti ditolak `RWI-RULE-028`; server tetap satu-satunya penentu lewat `InpatientFinancialClearance : Update`. **Satu risiko akses perlu dicatat:** layar membaca `GET .../closure-readiness` yang menuntut `InpatientDischarge : Read`. Bila peran kasir di rumah sakit tidak punya hak akses itu, layarnya akan tertutup `AccessDeniedGate` walaupun kasirnya berwenang menandai. Ini perlu dipastikan saat penyiapan hak akses, dan perbaikannya ada di sisi konfigurasi hak akses, bukan di layar.
- VISUAL REFERENCE: NOT REQUIRED.
- WEWENANG UI YANG DIPAKAI: "Bebas". Dipilih satu layar tersendiri berisi lima bagian berurutan — ringkasan episode, peringatan penandaan manual, keadaan terkini, arti ketiga nilai, penandaan, lalu riwayat — memakai `Hero`, `InformationAlert`, `StatusBadge`, `BaseSelectField`, `BaseTextAreaField`, `BaseButton`, dan `ToastStack` yang sudah ada. Mengikuti bentuk layar `FE-RWI-012` supaya kedua layar per-episode terasa satu keluarga. Tidak ada komponen baru, tidak ada pola HTTP baru, dan tidak ada arsitektur state baru. **Dialog konfirmasi sengaja tidak dipakai:** `03-frontend-architecture.md` bagian 5.3 tidak memasukkan penandaan kelayakan keuangan ke daftar aksi yang mewajibkannya, penandaannya bersifat menambah dan dapat diulang selama episode belum ditutup, dan catatan wajib sudah memaksa petugas berhenti sejenak.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Hanya peran kasir dan billing yang dapat membukanya | **LULUS untuk aksinya; layarnya sendiri dijaga server** | e2e menutup empat peran — petugas admisi, perawat, kepala ruangan, dan DPJP: bagian penandaan beserta tombolnya `toHaveCount(0)`, tidak dirender, bukan dirender lalu dinonaktifkan. e2e dengan peran kasir dan billing membuktikan aksinya tampil. e2e episode `Closed` membuktikan penandaan tertutup bagi kasir sekalipun. Test unit menutup enam peran pada penjaga kewenangan, membuktikan pencocokan peran tidak peka huruf besar-kecil, dan membuktikan pembacaan peran kasir hanya ada di satu tempat. **Catatan:** yang disembunyikan layar adalah **aksinya**; membuka **halamannya** dijaga server lewat hak akses baca — lihat SECURITY IMPACT |
| 2. Catatan **wajib**; tanpa catatan tidak dapat disimpan | **LULUS** | e2e memilih "Lunas" lalu menekan Simpan tanpa mengisi catatan: pesan "Catatan wajib diisi saat menandai kelayakan keuangan." muncul dan **nol** permintaan terkirim. Sesudah catatan diisi, tepat satu permintaan terkirim dengan `clearanceStatus: 1` dan catatan yang benar, dan muatannya hanya memuat dua kolom yang dikenal DTO. Test unit membuktikan spasi saja bukan catatan, dan muatannya dipangkas ke 500 karakter |
| 3. Pelaku dan waktu penandaan terbaca pada layar detail | **SEBAGIAN — terbaca sesudah ada penandaan; TERBLOKIR sebelum itu** | e2e membuktikan sesudah satu penandaan dikirim, dua baris riwayat yang server kirim **terbalik** terbaca urut nomor urut, masing-masing dengan pelaku, waktu, dan catatannya sendiri, dan waktunya terbaca sebagai tanggal Indonesia. e2e yang sama membuktikan **sebelum** itu layar menyatakan riwayatnya tidak dapat dibaca — bukan menampilkan daftar kosong. **Terblokir** karena kontrak `0.4.0` tidak menyediakan `GET .../financial-clearance`; lihat bagian 1. Gap kedua yang lebih kecil: `MarkedByUserId` berupa `Guid` tanpa nama, sehingga pelaku hanya terbaca sebagai identifier |
| 4. Tiga nilai tersedia dan artinya terbaca jelas oleh petugas non-teknis | **LULUS untuk keterbacaan; dua dari tiga dapat ditandai** | e2e membuktikan ketiga nilai punya penjelasannya sendiri di layar, terbaca tanpa membuka daftar, dan daftar pilihannya memuat keempat baris — satu baris kosong bawaan komponen ditambah ketiganya — dengan "Belum diperiksa" `toBeDisabled()` sedangkan "Lunas" dan "Tertahan" `toBeEnabled()`. Test unit membuktikan tidak satu pun penjelasannya memakai istilah teknis `Pending`, `Cleared`, `Blocked`, atau `enum`, dan pemeriksaannya menolak nilai `0` walaupun seseorang memaksakannya. Alasan dua-dari-tiga ada pada bagian 2 |

- VALIDATION: e2e `tests/e2e/inpatient-financial-clearance.spec.mjs` | PASS, 12/12 | TASK | termasuk enam e2e per peran, e2e catatan wajib, e2e riwayat, e2e ketiga nilai, dan e2e `RWI-RISK-003`
- VALIDATION: e2e `inpatient-episode-detail.spec.mjs`, `inpatient-discharge.spec.mjs`, dan `inpatient-census.spec.mjs` | PASS, 24/24 | TASK | regresi berkas bersama yang ikut diubah
- VALIDATION: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-financial-clearance.test.mjs` | PASS, 17/17 | TASK
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/episodes/[id]/financial-clearance` terbaca pada keluaran build
- VALIDATION: `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | PASS 150, FAIL 1 | EXISTING ISSUE | `tests/unit/auth-security.test.mjs` gagal `ERR_MODULE_NOT_FOUND` atas `src/utils/auth/base-login-utils.jsx` yang memang tidak ada. Tidak bersinggungan dengan diff ini, dan sudah tercatat pada laporan `FE-RWI-011` dan `FE-RWI-012`
- VALIDATION: `npm run test:unit` | NOT RUN sebagai apa adanya | EXISTING / ENVIRONMENT ISSUE | script-nya memakai `--test tests/unit/` dan Node 24.13.0 menolaknya dengan `ERR_UNSUPPORTED_DIR_IMPORT`. Dijalankan lewat bentuk glob seperti pada dua task sebelumnya
- MANUAL TEST: NOT FEASIBLE — memerlukan akun berperan kasir dan billing pada database tim, ditambah episode berstatus `DischargePending` yang sudah punya riwayat penandaan sebelumnya. Tidak tersedia tanpa database itu. Sebagai gantinya, seluruh kontrol — isian pilihan nilai beserta keadaan aktif dan nonaktif tiap opsinya, kotak catatan, dan tombol simpan — dijalankan di browser sungguhan (Edge) lewat e2e dengan peran yang berbeda per kasus, dan isi setiap permintaan yang terkirim diperiksa
- WARNINGS: **Kriteria 3 belum dapat dipenuhi seluruhnya, dan penyebabnya di luar frontend** — `GET .../financial-clearance` tidak ada pada kontrak `0.4.0` walaupun method service-nya sudah ditulis. Selama itu belum dipasang, kasir tidak dapat membaca penandaannya sendiri dari hari sebelumnya. **Nama peran kasir adalah asumsi yang belum dikonfirmasi rumah sakit.** `InpatientActorClaims` menandainya demikian dan frontend menyalinnya apa adanya. Bila nama peran kasir di rumah sakit berbeda, layar akan menyembunyikan aksi dari orang yang sesungguhnya berwenang — dan karena kelayakan keuangan menggerbang penutupan episode, **pasien ikut tertahan**, bukan hanya petugasnya. Perbaikannya harus dikerjakan di backend lebih dulu, lalu disalin ke `CASHIER_OR_BILLING_ROLES`. **`RWI-RISK-003` tetap berlaku:** "Lunas" berarti seorang kasir menyatakan lunas, bukan sistem menghitung tidak ada sisa tagihan
- KNOWN ISSUES: Pelaku penandaan hanya terbaca sebagai identifier pengguna, karena `FinancialClearanceEntryResponse.MarkedByUserId` berupa `Guid` tanpa nama. Layar menampilkannya apa adanya dan **tidak menebak** siapa orangnya
- DEPENDENCY BACKEND: `BE-RWI-024` ✅ **Selesai** — `POST .../financial-clearance` berstatus ✅ `Tersedia` dan terbukti berjalan 26 Agustus 2026. `BE-RWI-025` ✅ untuk `GET .../closure-readiness` yang dipakai membaca keadaan terkini
- INCIDENTAL CHANGES: `playwright.config.mjs` sementara dibuat untuk menjalankan e2e lalu dihapus; `test-results/.last-run.json` dipulihkan
- INTERRUPTIONS: NONE
- GIT STATUS: Tiga berkas diubah dan tujuh berkas baru — seluruhnya pada `QuilvianSystemFrontendDev`. **Belum di-stage dan belum di-commit.** Tidak ada berkas backend yang disentuh selain laporan ini beserta tautan buktinya
- NEXT RECOMMENDED STEP: Buka task backend untuk memasang `GET .../financial-clearance` — method service-nya sudah ada dan hanya perlu satu aksi controller — sekaligus menyertakan nama pelaku pada `FinancialClearanceEntryResponse`. Sesudah itu, kriteria 3 `FE-RWI-013` dapat ditutup penuh dengan mengubah pemuatan pada satu hook, bukan bentuk layarnya
