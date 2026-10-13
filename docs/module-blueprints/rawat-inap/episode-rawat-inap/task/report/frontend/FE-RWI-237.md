# Laporan Perubahan Frontend — `FE-RWI-237`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-237` |
| Judul | Helper admisi transfer IGD dan inventaris cutover mengikuti mutation target |
| Slice | Consumer Bed Management sesuai kartu FE-RWI-237; delivery BM-DEL-20261011-01 revision1 |
| Roadmap | [frontend-roadmap-bed-management.md](../../../roadmap/frontend-roadmap-bed-management.md) — kartu FE-RWI-237 |
| Trace | FR-BM-003, FR-BM-004; RWI-DEC-283, RWI-DEC-289, RWI-DEC-290, RWI-DEC-293; RWI-AC-408, RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-425; NFR-704, NFR-706 |
| Contract version | 0.12.0 Bed Management; decision46, child manifest11, input24/24 SHA256 cocok. Pengguna mengotorisasi seluruh tujuh task FE dalam sesi ini. Delivery lock existing berstatus APPROVED_BACKEND_IMPLEMENTATION_PARTIAL; approval FE sesi ini dicatat di laporan/roadmap, tanpa mengubah lock/manifest/kontrak. |
| Wewenang UI | DEV_DISCRETION RWI-DEC-292 untuk detail nonkritis, memakai token/base components; tidak menetapkan kebijakan bisnis, grant, SOP atau official class order |
| Dependency | `FE-RWI-231`, `BE-RWI-221`; source backend pasangan 🟡, runtime/migration belum terbukti; fondasi FE-RWI-231 🟡 |
| Klasifikasi | MEDIUM — satu helper existing, koordinasi stable intent serta inventaris lintas consumer; tidak membuat alur UI baru. Skor numerik baku tidak tersedia pada governance yang dibaca. |
| Task mode | FRONTEND + CROSS-REPO terbatas laporan/status/tautan bukti |
| Target tulis | QuilvianSystemFrontendDev/src dan tests terkait; backend hanya task/report/frontend, status frontend-roadmap dan dua requirement-traceability modul |
| Model | Codex berbasis GPT-6; varian model tidak diekspos dalam sesi |
| Commit frontend saat dikerjakan | 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2; branch HamzahV2 |
| Commit backend yang dijadikan rujukan | db85bede83c0ddbc9aaa412c576f2c3fd7447492; branch MHamzah; source Bed Management working tree dibaca apa adanya |
| Tanggal | 2026-10-11 Asia/Jakarta |
| Status | 🟡 SEBAGIAN — 4/4 dipetakan ke source; acceptance penuh 0/4. Unit scope 58/58 PASS, suite penuh 2694/2720 PASS dengan26 kegagalan baseline. Build/lint dikecualikan pengguna; browser/API/database NOT_RUN. |

Skill build-module-frontend diterapkan berurutan pada ketujuh ID sesuai instruksi eksplisit pengguna “kerjakan seluruh task frontend”; satu laporan tetap dibuat per ID. Instruksi pengguna agar tidak menjalankan build/lint mengesampingkan butir validasi itu pada skill. Dependency backend yang baru selesai pada level source tidak diubah menjadi ✅ dan tidak dianggap bukti runtime.

## 1. Keadaan yang ditemukan di awal

admitFromTransfer telah tersedia, namun pencarian source tidak menemukan caller UI yang benar-benar memanggil helper tersebut. Helper lama belum menuntut versi bed dan stable intent; antrean transfer tetap reader existing.

Source as-is backend telah tersedia dalam working tree hasil task BE-RWI-208–221, semuanya 🟡. Kontrak target tetap0.12.0. Delta kontrak/as-is diperlihatkan di bagian8; frontend tidak menulis source backend. Dua lockfile frontend sudah berubah sebelum task dan hash keduanya tetap sama setelah pengerjaan.

## 2. Proses bisnis dari sisi pengguna

1. Caller yang kelak menggunakan admitFromTransfer harus memberi expectedBedVersion dari available-beds dan mempertahankan objek operationIntent yang sama selama satu maksud operasi.
2. Helper menolak versi/intent yang hilang sebelum POST, lalu mengirim Idempotency-Key melalui utility bersama. HTTP201 diurai sebagai episode dengan OperationMeta tetap tersedia.
3. Jika timeout, pemanggilan ulang menggunakan intent/body yang sama memeriksa own outcome, kemudian replay key yang sama. Respons episode yang sama dibuktikan menggunakan HTTP double, belum jaringan nyata.
4. Inventaris di bawah menandai caller aktif, helper tanpa caller dan status runtime. Cutover tetap ditahan sampai seluruh writer/consumer dan gate lingkungan terbukti; migration/deploy/seed bukan aksi task frontend.

Tanpa hak akses, consumer baru menahan pemasangan/pengambilan data yang dilarang; penolakan403 server tetap ditampilkan. Data kosong dan gagal memuat dibedakan dari keberhasilan mutasi. Akun, role dan fixture rumah sakit nyata belum tersedia untuk pembuktian manual.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

AGENTS.md frontend/backend; canonical GLOBAL_RULES dan frontend-architecture; skill build-module-frontend beserta profile, token, katalog/gate komponen, composition, UI checklist, master-data-feature-standard dan test-policy; roadmap, delivery lock, keputusan46, kontrak/desain target dan laporan dependency backend. Source controller/DTO/query as-is Bed Management, Bed Occupancy, Discharge, AdmissionTransfer dan empat master dibaca untuk memastikan method, field, permission serta delta respons. Referensi visual: census/list rawat inap dan komponen region existing; master mengacu pada standar hr/master-data/job-level dengan batas adapter terhadap bentuk legacy existing.

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Status Git |
| --- | --- | --- |
| [src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js) | Helper menuntut expectedBedVersion dan config.operationIntent stabil; replay key/body, HTTP201/OperationMeta dan tiga test HTTP-double. Inventaris cutover disatukan pada laporan ini. | M |
| [tests/unit/inpatient-bed-admission-helper.test.mjs](../../../../../../../../QuilvianSystemFrontendDev/tests/unit/inpatient-bed-admission-helper.test.mjs) | Uji kontrak/payload dan penyesuaian assertion terhadap alur shared; bukan bukti browser. | ?? |
| [tests/support/instance-axios-double.mjs](../../../../../../../../QuilvianSystemFrontendDev/tests/support/instance-axios-double.mjs) | Uji kontrak/payload dan penyesuaian assertion terhadap alur shared; bukan bukti browser. | M |

Utility shared dari FE-RWI-231 digunakan ulang oleh task berikutnya; hubungan itu dicatat sebagai dependency, bukan implementasi HTTP/state paralel. File laporan ini dan tautan status roadmap/traceability juga ditulis; file backend lainnya tidak diubah oleh pekerjaan frontend.

### 3.3 Kepatuhan arsitektur frontend

Route App Router tetap entry point. View/feature merangkai base components dan memanggil hook; hook memegang state/validasi/request, service memakai InstanceAxios existing, pure normalizer/payload berada pada utils. Redux existing tetap dipakai untuk auth dan master data. Tidak ada state library, HTTP client, framework test atau primitive shared/base baru. Definisi kolom baru dipisahkan sebagai *-table-columns.jsx.

| Elemen | Keputusan | Bukti / alasan |
| --- | --- | --- |
| Hero, ringkasan, alert, toast dan access gate | REUSE | Hero, SummaryGrid, InformationAlert, ToastStack, AccessDeniedGate dari base-features; pola list/census existing |
| Filter/pilihan/input/tombol | REUSE | DataFilter, FilterSelect/ResourceFilterSelect, BaseFormControl dan BaseButton; API props existing tanpa perubahan default |
| Tabel, status, halaman dan dialog | REUSE | DataTable, StatusBadge, PaginationComponent, ConfirmModal; existing correction/master wrappers |
| NOT APPLICABLE — helper tidak membuat layar; pemulihan memakai utility FE-RWI-231 | REUSE | Komposisi fitur domain memakai primitive di atas; pilihan1 di bawah berlaku untuk tiap komposisi, tidak menambah shared primitive |

Pilihan untuk setiap elemen COMPOSE:

1. **Rekomendasi: rangkai base components pada feature domain.** Konsistensi visual mengikuti token/katalog; risiko regresi terbatas pada consumer; biaya perubahan lokal.
2. Perluas base components dengan aturan cycle/transfer/affected-set. Konsistensi dapat terjaga, tetapi risiko pada seluruh pemakai serta biaya review shared lebih besar. Tidak dipilih.

**UI GATE: PASS pada keputusan reuse/komposisi source.** Tidak ada NEW atau perubahan default base; validasi visual browser tetap NOT FEASIBLE. CSS baru tidak memuat warna literal, override typography, !important atau tema gelap. Sembilan JSX baru tidak mempunyai button/table mentah, Bootstrap typography atau inline style statis.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat / refresh | Loading base table/select/button; snapshot belum dapat dikonfirmasi sampai pembacaan fresh selesai |
| Kosong | Pesan daftar kosong atau permintaan melengkapi bed/periode/pilihan; tidak ditampilkan sebagai mutasi berhasil |
| Gagal memuat | InformationAlert atau error existing serta tindakan Muat Ulang |
| Tanpa hak akses /403 | AccessDeniedGate/InformationAlert dan consumer forbidden tidak dipasang; server tetap otoritatif |
| Konflik409 | Snapshot lama ditinggalkan; data dibaca kembali dan pengguna diminta memeriksa/konfirmasi ulang |
| Validasi422 | Alasan/pilihan tetap tersedia untuk diperbaiki; tidak ada perubahan lokasi atau delete sebagai jalan pintas |
| Pending | Utility menolak klik ganda; tombol dan isian terkait dinonaktifkan |
| Unknown/timeout/offline | Tidak menyatakan sukses; body/key tetap; retry membaca own outcome lalu replay key sama. Outcome403 tidak membuang key. |
| Sukses | State dibaca ulang dari server; callback UI gagal tidak membalik hasil mutasi yang telah committed |

Untuk helper FE-RWI-237, state layar adalah tanggung jawab caller dan belum mempunyai caller UI; helper hanya menolak payload tidak lengkap atau melempar error operationUnknown yang bisa ditangani caller.

## 5. Endpoint yang dikonsumsi

Path di bawah memakai prefix /v1; InstanceAxios menambahkan /api. Mutasi berpartisipasi pada coordinator mengirim Idempotency-Key dari utility bersama. Reader/options existing tidak diubah menjadi writer.

#### Health Services / Inpatient Management / Inpatient Admission Transfer

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/admission-transfers` | Reader antrean existing, tidak diubah menjadi wizard | `InpatientAdmissionTransfer : Read` |
| `GET` | `/v1/health-services/inpatient-management/admission-transfers/{id}` | Reader detail existing | `InpatientAdmissionTransfer : Read` |
| `POST` | `/v1/health-services/inpatient-management/admission-transfers/admit` | Helper admisi dengan versi/key,201 dan OperationMeta | `InpatientEpisode : Create` |

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-management/operations/{key}` | Own outcome sebelum replay helper | `InpatientBedManagement : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| AUTOMATED TEST: enam file unit scope melalui node --import ./tests/helpers/register.mjs --test | 58 tests;58 PASS;0 FAIL | PASS | inpatient-bed-management, inpatient-bed-admission-helper, inpatient-bed-status, inpatient-episode-detail, inpatient-placement, inpatient-foundation |
| AUTOMATED TEST: npm.cmd run test:unit | 2720 tests;2694 PASS;26 FAIL | FAIL — EXISTING / ENVIRONMENT ISSUE | Nama26 tes gagal juga ada pada pembanding virtual source HEAD; lihat rincian [FE-RWI-237](FE-RWI-237.md) |
| Unit pure logic baru | 15/15 PASS | PASS | Tests privacy/raw status, counts, positive versions, manual categories, dua versi transfer, UTC >31hari, snapshot tanpa UUID, key/body/replay409/timeout403/404/double click |
| Unit helper admisi transfer baru | 3/3 PASS | PASS | HTTP-double: missing version/intent tanpa POST,201+OperationMeta, timeout→receipt→same-key replay dengan episode sama |
| Parse Babel dan resolusi import/export lokal | 63 JS/JSX/MJS;0 syntax error,0 missing import/export,0 unbound identifier | PASS | Parse dan traverse seluruh source/test yang berubah; pemeriksaan ini bukan build/lint |
| Anti-regresi UI | 9 JSX baru/0 temuan; CSS baru/0 warna literal,0 override typography,0 !important | PASS | Pemeriksaan pola button/table/Bootstrap/inline style dan stylesheet |
| Hash input lock + lockfile pengguna | 24/24 masukan cocok;2/2 lockfile pengguna tetap | PASS | SHA256 raw bytes; package-lock/yarn tidak ditulis |
| Diff/scope | git diff --check bersih; HEAD/branch tetap; tidak ada stage/commit/push | PASS | Pemeriksaan read-only Git |
| Browser localhost | GET http://localhost:3000/health-services/inpatient-management/bed-board gagal ECONNREFUSED | EXISTING / ENVIRONMENT ISSUE | Server frontend tidak berjalan; akun/fixture terotorisasi juga belum tersedia |
| Lint:errors / lint | NOT_RUN | NOT RUN | Dikecualikan eksplisit pengguna; pengguna menjalankan manual |
| npm run build | NOT_RUN | NOT RUN | Dikecualikan eksplisit pengguna; pengguna menjalankan manual |
| BM-AT scoped, API/network role nyata, race database, UAT | NOT_RUN | NOT RUN | Unit source/HTTP double tidak menggantikan skenario end-to-end |

**MANUAL TEST: NOT FEASIBLE** — localhost:3000 menolak koneksi. Belum dapat membuktikan daftar/label select, pencarian, kombinasi/reset filter, pagination, dialog, keyboard/responsif,403/409/422 dan privacy role nyata di browser. Tidak menjalankan test:uat karena membawa build. Prettier API hanya dipakai merapikan source terkait, bukan lint.

Perintah scope: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-bed-management.test.mjs tests/unit/inpatient-bed-admission-helper.test.mjs tests/unit/inpatient-bed-status.test.mjs tests/unit/inpatient-episode-detail.test.mjs tests/unit/inpatient-placement.test.mjs tests/unit/inpatient-foundation.test.mjs`.

## 7. Acceptance criteria dan Definition of Done

Setiap teks kriteria berikut disalin persis dari kartu roadmap. Pemetaan source bukan pernyataan skenario API/browser telah lulus; acceptance penuh tetap0/4 sampai bukti runtime tersedia.

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Helper mengirim version/key dari hasil available-beds, retry same key menerima episode yang sama. | Source dipetakan; runtime NOT_RUN | Helper memvalidasi versi/intent sebelum POST dan memakai koordinator yang sama; tiga unit test HTTP-double membuktikan201, OperationMeta, timeout→own outcome→same-key replay/episode sama. Provenance available-beds caller belum dapat diuji karena caller tidak ditemukan. |
| Daftar consumers menunjukkan route/caller/status target dan pemilik; setiap writer/consumer legacy yang belum aman menahan cutover BM-G04. | Source dipetakan; runtime NOT_RUN | Inventaris reader/writer di bawah memetakan route/hook/service, status target serta owner; belum ada consumer disebut live PASS. BM-G04 tetap terbuka. |
| Missing caller atau UAT yang belum tersedia dicatat NOT_RUN/GAP, tidak disebut alur live lulus. | Source dipetakan; runtime NOT_RUN | Pencarian admitFromTransfer hanya menemukan deklarasi dan test; NO_CALLER/GAP serta API/browser NOT_RUN dicatat. Test double bukan bukti UI. |
| Tidak menjalankan migration/deploy/seed akses dari task frontend. | Source dipetakan; runtime NOT_RUN | Tidak ada eksekusi migration, deploy, seed akses atau perubahan source backend; diff lintas repository hanya laporan dan bukti roadmap/traceability. |

D-FE: source, laporan, keputusan UI, status/tautan traceability dan validasi statis/unit tersedia. **Belum terpenuhi:** runtime/API/browser, dependency backend selesai penuh, pembuktian privacy/role/gate aktual serta kriteria yang memerlukan transaksi nyata. **Dikecualikan pengguna:** build dan lint. Suite penuh masih FAIL baseline; status task 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | npm.ps1 diblokir ExecutionPolicy; tes memakai npm.cmd. Git memberi pemberitahuan konversi LF/CRLF; diff-check tidak menunjukkan whitespace error. |
| Masalah yang diketahui | NO_CALLER untuk admitFromTransfer; runtime API201/replay nyata belum diuji. Intent helper disimpan dalam WeakMap keyed objek caller, bukan persisted browser. Gate BM-G04 dan aktivasi/cutover tetap NOT_READY. |
| Batas pemulihan | Stable intent disimpan dalam memori per pemilik/scope selama sesi aplikasi, bukan penyimpanan permanen lintas full reload browser. Pemulihan setelah reload penuh/keluar sesi belum dibuktikan; jangan menganggap unknown selesai atau membuat key baru untuk maksud operasi yang sama tanpa rekonsiliasi. |
| Dependency backend | `FE-RWI-231`, `BE-RWI-221`; BE source 🟡 dan migration/runtime NOT_RUN. BM-G01 kelas resmi, BM-G02 SOP, BM-G03 scope/privacy akun aktual dan BM-G04 cutover tetap mengikuti cakupan masing-masing, belum ditutup. |
| Perubahan sampingan | NONE oleh task ini. package-lock.json dan yarn.lock dirty sejak awal, SHA256 tetap. Working tree backend user/source/migration yang sudah ada tidak disentuh. |
| Interupsi | Sesi dilanjutkan melalui instruksi continue; status/diff diperiksa sebelum melanjutkan. Tidak ada rollback atau penggandaan perubahan. |
| Status Git | Source/test tabel3.2: M atau ?? sesuai Git; laporan ini baru ??, frontend-roadmap-bed-management dan requirement-traceability-bed-management ?? sejak planning; requirement-traceability.md M. Seluruh perubahan belum di-stage/commit. |
| Langkah berikutnya | Pengguna menjalankan build/lint manual; pemilik backend menyelaraskan metadata transfer/evidence historis bila diwajibkan kontrak; jalankan aplikasi/fixture/role dan BM-AT scoped, lalu perbarui laporan ini dengan bukti. |


### Inventaris consumer dan batas cutover BM-G04

| Consumer / route / pemanggil | Kontrak target dan status source | Pemilik / bukti |
| --- | --- | --- |
| Bed Management /health-services/inpatient-management/bed-board → use-bed-management | Metadata/monitoring; enam status/counts, scope, privacy, fresh refresh. SOURCE_ADAPTED; Transfer GAP metadata. | FE + BE-RWI-211; [FE-RWI-231](FE-RWI-231.md) |
| Admisi /health-services/inpatient-management/admissions → use-inpatient-admission-bed | Reserve/cancel: fresh available/holding, version/reason/key. SOURCE_ADAPTED; expiry lifecycle NOT_RUN. | FE + BE-RWI-212/213; [FE-RWI-232](FE-RWI-232.md) |
| Monitoring → use-inpatient-bed-board-actions | Cancel/place: fresh reservation/episode + expectedBedVersion/key; reason wajib. SOURCE_ADAPTED. | FE + BE-RWI-213; [FE-RWI-232](FE-RWI-232.md) |
| Detail /health-services/inpatient-management/episodes/[id] → use-inpatient-episode-detail | Record-departure: fresh current placement + bed version/key; old closure runtime NOT_RUN. SOURCE_ADAPTED. | FE + BE-RWI-213; [FE-RWI-232](FE-RWI-232.md) |
| Child Monitoring → use-inpatient-bed-cleaning | Start/complete/verify: cycle/attempt/version/evidence/reason/key. SOURCE_ADAPTED; evidence historis tidak diberikan DTO. | FE + BE-RWI-214/215; [FE-RWI-233](FE-RWI-233.md) |
| Master /health-services/master-data/bed, /room, /service-unit, /patient-class → empat editor/detail/slice | Create bed, PUT, PATCH status dan DELETE memakai adapter versions/reason/key. Create hierarchy tetap API existing tanpa replay otomatis. SOURCE_ADAPTED. | FE/MasterData + BE-RWI-217; [FE-RWI-234](FE-RWI-234.md) |
| Bed /availability | Pencarian source tidak menemukan caller UI aktif; backend mempunyai endpoint. NO_CALLER; jangan disebut browser-tested. | MasterData + BE-RWI-216/217; [FE-RWI-234](FE-RWI-234.md) |
| Bed Management Transfer + episodes/[id] + episodes/[id]/nursing → shared InpatientBedTransfer | Satu hook: transfer-context/available-beds, manual category, source placement/version, dua bed versions/key. SOURCE_ADAPTED; entry Bed Management disabled oleh metadata. | FE + BE-RWI-219; [FE-RWI-235](FE-RWI-235.md) |
| History + detail episode → use-inpatient-placement-correction | Fresh placement version + distinct affectedBedVersions/reason/key; Billing guard server. SOURCE_ADAPTED; permissions monitoring aktual NOT_RUN. | FE/Billing + BE-RWI-220; [FE-RWI-236](FE-RWI-236.md) |
| Bed Usage History → use-inpatient-bed-usage-history | Bed/periode UTC/kamar/page; initial/transfer/current/correction tidak dibuang. SOURCE_ADAPTED. | FE + BE-RWI-220; [FE-RWI-236](FE-RWI-236.md) |
| Admission transfer helper → admitFromTransfer | Version/stable operationIntent/key,201/OperationMeta/replay. SOURCE_ADAPTED; NO_CALLER UI. | FE/InPatientManagement + BE-RWI-221; laporan ini |
| Antrean admission-transfers GET / detail GET | Reader existing tetap; daftar tidak otomatis memanggil helper admit. UNCHANGED_READER. | FE/InPatientManagement |
| use-inpatient-admission-flow | Diperiksa: konfirmasi membuat episode, bukan pemanggil POST placements; bed mutation ada pada step Bed. UNCHANGED; tidak ada writer bypass yang dibuat task ini. | FE/InPatientManagement |
| Reports transfer dan riwayat placement nursing existing | Tetap reader laporan/histori existing; bukan pengganti seluruh usage-history. UNCHANGED_READER. | FE/InPatientManagement |

Pencarian callsite dilakukan dengan rg pada src/lib/hooks, src/lib/services, src/lib/state dan view inpatient-management: reservations, placements, placements/transfer, record-departure, corrections, admitFromTransfer dan availability. Inventaris hanya bukti source frontend; inventaris writer internal/generic backend tetap dipegang laporan BE dan tidak diverifikasi ulang sebagai runtime frontend.

**Status cutover: NOT_READY.** Seluruh consumer aktif di atas masih runtime NOT_RUN; metadata transfer, evidence historis, proof class/SOP/access, schema/migration, dry run lingkungan, persaingan writer dan UAT perlu bukti. Environment owner memegang pelaksanaan cutover/migration/deploy dengan otorisasi terpisah. Tidak ada writer legacy yang dinyatakan aman berdasarkan unit test saja.

### Kegagalan suite penuh dan pembanding

Pembanding read-only memakai overlay source dari git show HEAD pada33file tracked yang berubah, melalui loader dan preload sementara; working tree tidak di-checkout/direset. Pembanding saat itu2716test:2686 PASS/30 FAIL. Tes/assertion lama yang memeriksa lokasi implementasi dipindahkan untuk memeriksa flow bersama dan payload versi/reason, tanpa mengubah26 skenario gagal tidak terkait di bawah. Rerun akhir2720test:2694 PASS/26 FAIL; ke26 nama gagal juga muncul pada pembanding. Pembanding ini simulasi source HEAD, bukan klaim full pristine checkout ataupun build baseline. Empat tes tambahan pada hitungan akhir berasal dari tiga helper test dan satu penolakan payload master tidak lengkap.

- `test at tests/unit/hemodialysis-navigation-and-privacy-audit.test.mjs:264:1`
- `test at tests/unit/hemodialysis-navigation-and-privacy-audit.test.mjs:294:1`
- `test at tests/unit/hemodialysis-sidebar-navigation.test.mjs:270:1`
- `test at tests/unit/hemodialysis-sidebar-navigation.test.mjs:347:1`
- `test at tests/unit/inpatient-admission-cancellation.test.mjs:125:1`
- `test at tests/unit/inpatient-billing-status-badge.test.mjs:89:1`
- `test at tests/unit/inpatient-final-consistency.test.mjs:54:1`
- `test at tests/unit/inpatient-nursing-billing-summary.test.mjs:120:1`
- `test at tests/unit/inpatient-nursing-finishing-roadmap.test.mjs:152:1`
- `test at tests/unit/inpatient-nursing-procedure-and-ancillary.test.mjs:110:1`
- `test at tests/unit/inpatient-nursing-transfer-and-unavailable.test.mjs:39:1`
- `test at tests/unit/inpatient-nursing-workspace-revision.test.mjs:255:1`
- `test at tests/unit/inpatient-nursing-workspace.test.mjs:68:1`
- `test at tests/unit/inpatient-physician-entry.test.mjs:149:1`
- `test at tests/unit/inpatient-physician-post-op-marker.test.mjs:10:1`
- `test at tests/unit/inpatient-procedure-parity.test.mjs:75:1`
- `test at tests/unit/menu-permission-filter.test.mjs:110:1`
- `test at tests/unit/outpatient-prescription-parity.test.mjs:13:1`
- `test at tests/unit/petty-cash-finance-separation.test.mjs:51:1`
- `test at tests/unit/post-op-summary.test.mjs:135:1`
- `test at tests/unit/surgery-booking.test.mjs:47:1`
- `test at tests/unit/surgery-booking.test.mjs:97:1`
- `test at tests/unit/surgery-booking.test.mjs:134:1`
- `test at tests/unit/ward-pre-op.test.mjs:41:1`
- `test at tests/unit/ward-pre-op.test.mjs:91:1`
- `test at tests/unit/ward-pre-op.test.mjs:106:1`

Kegagalan nursing-transfer-and-unavailable adalah skenario bedah/alat yang tidak diubah; dua skenario transfer pada file yang sama lulus. Kegagalan suite penuh tetap dicatat FAIL dan tidak ditutup oleh keberhasilan58tes scope.

### Validasi dokumen akhir

Sepuluh dokumen pada scope task diperiksa: tujuh laporan, roadmap frontend, traceability Bed Management dan bagian baru pada traceability utama. Tautan lokal scope tersedia, bentuk tabel konsisten dan seluruh28teks acceptance kartu tetap sama; grafik dependency hanya berubah tanda status. Satu tautan historis pada bagian lama requirement-traceability.md menuju ./issues/issue-002-admisi-bayi-baru-lahir-buntu.md sudah tidak tersedia; tidak disentuh karena di luar bukti Bed Management baru.
