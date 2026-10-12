# Laporan Perubahan Frontend — `FE-RWI-231`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-231` |
| Judul | Menu Bed Management dan Monitoring memakai hak, status serta respons fresh |
| Slice | Consumer Bed Management sesuai kartu FE-RWI-231; delivery BM-DEL-20261011-01 revision1 |
| Roadmap | [frontend-roadmap-bed-management.md](../../../roadmap/frontend-roadmap-bed-management.md) — kartu FE-RWI-231 |
| Trace | FR-BM-001, FR-BM-002, FR-BM-004; RWI-DEC-274, RWI-DEC-287, RWI-DEC-289, RWI-DEC-290, RWI-DEC-292; RWI-AC-396, RWI-AC-397, RWI-AC-398, RWI-AC-416, RWI-AC-417, RWI-AC-420, RWI-AC-422, RWI-AC-424; NFR-702, NFR-704 |
| Contract version | 0.12.0 Bed Management; decision46, child manifest11, input24/24 SHA256 cocok. Pengguna mengotorisasi seluruh tujuh task FE dalam sesi ini. Delivery lock existing berstatus APPROVED_BACKEND_IMPLEMENTATION_PARTIAL; approval FE sesi ini dicatat di laporan/roadmap, tanpa mengubah lock/manifest/kontrak. |
| Wewenang UI | DEV_DISCRETION RWI-DEC-292 untuk detail nonkritis, memakai token/base components; tidak menetapkan kebijakan bisnis, grant, SOP atau official class order |
| Dependency | `BE-RWI-211`; source backend pasangan 🟡, runtime/migration belum terbukti; fondasi FE-RWI-231 🟡 |
| Klasifikasi | HEAVY — fondasi bersama, perubahan menu dan dua reader dengan privasi, pembatalan request serta pemulihan operasi; governance tidak menetapkan rumus skor numerik. Skor numerik baku tidak tersedia pada governance yang dibaca. |
| Task mode | FRONTEND + CROSS-REPO terbatas laporan/status/tautan bukti |
| Target tulis | QuilvianSystemFrontendDev/src dan tests terkait; backend hanya task/report/frontend, status frontend-roadmap dan dua requirement-traceability modul |
| Model | Codex berbasis GPT-6; varian model tidak diekspos dalam sesi |
| Commit frontend saat dikerjakan | 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2; branch HamzahV2 |
| Commit backend yang dijadikan rujukan | db85bede83c0ddbc9aaa412c576f2c3fd7447492; branch MHamzah; source Bed Management working tree dibaca apa adanya |
| Tanggal | 2026-10-11 Asia/Jakarta |
| Status | 🟡 SEBAGIAN — 3/4 lengkap secara source, 1/4 parsial karena gap respons backend; acceptance penuh 0/4. Unit scope 58/58 PASS, suite penuh 2694/2720 PASS dengan26 kegagalan baseline. Build/lint dikecualikan pengguna; browser/API/database NOT_RUN. |

Skill build-module-frontend diterapkan berurutan pada ketujuh ID sesuai instruksi eksplisit pengguna “kerjakan seluruh task frontend”; satu laporan tetap dibuat per ID. Instruksi pengguna agar tidak menjalankan build/lint mengesampingkan butir validasi itu pada skill. Dependency backend yang baru selesai pada level source tidak diubah menjadi ✅ dan tidak dianggap bukti runtime.

## 1. Keadaan yang ditemukan di awal

Leaf Papan Tempat Tidur memakai bed-board lama dan belum mempunyai consumer metadata, enam status operasional, history maupun pemulihan hasil ambigu. Status mentah master tidak cukup untuk menyatakan sebuah bed dapat dipesan.

Source as-is backend telah tersedia dalam working tree hasil task BE-RWI-208–221, semuanya 🟡. Kontrak target tetap0.12.0. Delta kontrak/as-is diperlihatkan di bagian8; frontend tidak menulis source backend. Dua lockfile frontend sudah berubah sebelum task dan hash keduanya tetap sama setelah pengerjaan.

## 2. Proses bisnis dari sisi pengguna

1. Pengguna membuka leaf Bed Management pada route bed-board yang sama. Hak Read dimuat sebelum metadata dan Monitoring diminta.
2. Monitoring menampilkan total beserta enam status dari server. Unit, kamar, status dan pencarian dikirim sebagai filter; pergantian unit mereset kamar, dan reset mengosongkan seluruh filter.
3. Pilih detail bed untuk tindakan pembersihan atau pesanan yang tersedia. Refresh ditunggu sampai selesai; respons request terdahulu dibatalkan dan tidak boleh menimpa hasil baru.
4. Tab hanya memasang consumer bila metadata mengizinkannya. Transfer tampil belum dapat diakses karena metadata backend saat ini belum menyediakan key transfer. Child pembersihan berada di Monitoring.

Tanpa hak akses, consumer baru menahan pemasangan/pengambilan data yang dilarang; penolakan403 server tetap ditampilkan. Data kosong dan gagal memuat dibedakan dari keberhasilan mutasi. Akun, role dan fixture rumah sakit nyata belum tersedia untuk pembuktian manual.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

AGENTS.md frontend/backend; canonical GLOBAL_RULES dan frontend-architecture; skill build-module-frontend beserta profile, token, katalog/gate komponen, composition, UI checklist, master-data-feature-standard dan test-policy; roadmap, delivery lock, keputusan46, kontrak/desain target dan laporan dependency backend. Source controller/DTO/query as-is Bed Management, Bed Occupancy, Discharge, AdmissionTransfer dan empat master dibaca untuk memastikan method, field, permission serta delta respons. Referensi visual: census/list rawat inap dan komponen region existing; master mengacu pada standar hr/master-data/job-level dengan batas adapter terhadap bentuk legacy existing.

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Status Git |
| --- | --- | --- |
| [src/app/health-services/inpatient-management/bed-board/page.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/app/health-services/inpatient-management/bed-board/page.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | M |
| [src/utils/menu-sidebar/menu-items.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | M |
| [src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | M |
| [src/lib/hooks/health-services/inpatient-management/use-inpatient-dashboard.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-dashboard.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | M |
| [src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | M |
| [src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/components/view/health-services/inpatient-management/bed-management/bed-monitoring-table-columns.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/bed-management/bed-monitoring-table-columns.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/lib/hooks/health-services/inpatient-management/use-bed-management.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-bed-management.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/lib/hooks/health-services/inpatient-management/use-bed-operation.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-bed-operation.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/lib/services/health-services/inpatient-management/bed-management.service.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/bed-management.service.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/lib/services/health-services/inpatient-management/bed-operation.service.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/bed-operation.service.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/utils/health-services/inpatient-management/bed-management-utils.js](../../../../../../../../QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/bed-management-utils.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/utils/health-services/inpatient-management/bed-operation-utils.js](../../../../../../../../QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/bed-operation-utils.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/lib/constants/health-services/inpatient-management/bed-management-constants.js](../../../../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/bed-management-constants.js) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/components/features/health-services/inpatient-management/bed-operation-recovery.jsx](../../../../../../../../QuilvianSystemFrontendDev/src/components/features/health-services/inpatient-management/bed-operation-recovery.jsx) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [src/style/health-services/inpatient-management/bed-management.module.css](../../../../../../../../QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/bed-management.module.css) | Menu dan halaman baru; proyeksi server dengan daftar field yang diizinkan; koordinator key/body per intent dan pemilik sesi; pembatalan request, akses tab, hitungan, filter dan pemulihan. | ?? |
| [tests/unit/inpatient-bed-management.test.mjs](../../../../../../../../QuilvianSystemFrontendDev/tests/unit/inpatient-bed-management.test.mjs) | Uji kontrak/payload dan penyesuaian assertion terhadap alur shared; bukan bukti browser. | ?? |
| [tests/unit/inpatient-foundation.test.mjs](../../../../../../../../QuilvianSystemFrontendDev/tests/unit/inpatient-foundation.test.mjs) | Uji kontrak/payload dan penyesuaian assertion terhadap alur shared; bukan bukti browser. | M |

Utility shared dari FE-RWI-231 digunakan ulang oleh task berikutnya; hubungan itu dicatat sebagai dependency, bukan implementasi HTTP/state paralel. File laporan ini dan tautan status roadmap/traceability juga ditulis; file backend lainnya tidak diubah oleh pekerjaan frontend.

### 3.3 Kepatuhan arsitektur frontend

Route App Router tetap entry point. View/feature merangkai base components dan memanggil hook; hook memegang state/validasi/request, service memakai InstanceAxios existing, pure normalizer/payload berada pada utils. Redux existing tetap dipakai untuk auth dan master data. Tidak ada state library, HTTP client, framework test atau primitive shared/base baru. Definisi kolom baru dipisahkan sebagai *-table-columns.jsx.

| Elemen | Keputusan | Bukti / alasan |
| --- | --- | --- |
| Hero, ringkasan, alert, toast dan access gate | REUSE | Hero, SummaryGrid, InformationAlert, ToastStack, AccessDeniedGate dari base-features; pola list/census existing |
| Filter/pilihan/input/tombol | REUSE | DataFilter, FilterSelect/ResourceFilterSelect, BaseFormControl dan BaseButton; API props existing tanpa perubahan default |
| Tabel, status, halaman dan dialog | REUSE | DataTable, StatusBadge, PaginationComponent, ConfirmModal; existing correction/master wrappers |
| Shell tiga tab, kolom Monitoring, detail child dan pemulihan operasi | COMPOSE | Komposisi fitur domain memakai primitive di atas; pilihan1 di bawah berlaku untuk tiap komposisi, tidak menambah shared primitive |

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

#### Health Services / Inpatient Management / Bed Management

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/inpatient-management/bed-management/metadata` | Hak tab/action dalam scope unit | `InpatientBedManagement : Read` |
| `GET` | `/v1/health-services/inpatient-management/bed-management/monitoring` | Lokasi, counts, status, versi dan holding context tersanitasi | `InpatientBedManagement : Read` |
| `GET` | `/v1/health-services/inpatient-management/bed-management/operations/{key}` | Receipt milik aktor untuk hasil ambigu | `InpatientBedManagement : Read` |

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
| Satu leaf Bed Management, Monitoring/Transfer/Bed Usage History reachable sesuai hak; HK hanya lokasi/status. | Sebagian source; GAP kontrak as-is; runtime NOT_RUN | Sidebar/page dan use-bed-management; child hanya memuat status/lokasi untuk respons HK yang disanitasi. GAP: metadata controller baris43–45 belum memiliki transfer; akses tiga tab belum lengkap. |
| Confirm disabled selama loading/refresh/unknown; timeout diperiksa lewat own outcome, tidak mengklaim success atau mengganti key otomatis. | Source dipetakan; runtime NOT_RUN | use-bed-operation + bed-operation-utils: busy guard, body tetap, pemeriksaan own outcome sebelum replay; unit timeout/outcome403/404 dan double click PASS. |
| Raw enum/cache tidak menentukan bookable; tab forbidden tidak fetch PHI. | Source dipetakan; runtime NOT_RUN | normalizeOperationalBed menolak fallback raw enum/cache; komponen tab forbidden tidak dimount; unit privacy/raw enum PASS. Payload HK nyata belum diuji. |
| Tidak optimistic/offline overwrite; layout nonkritis DEV_DISCRETION dalam token/base components. | Source dipetakan; runtime NOT_RUN | Pembaruan state menunggu respons server, bukan overwrite lokal; CSS token dan komposisi base. Pemeriksaan anti-regresi PASS; pengujian visual NOT_RUN. |

D-FE: source, laporan, keputusan UI, status/tautan traceability dan validasi statis/unit tersedia. **Belum terpenuhi:** runtime/API/browser, dependency backend selesai penuh, pembuktian privacy/role/gate aktual serta kriteria yang memerlukan transaksi nyata. **Dikecualikan pengguna:** build dan lint. Suite penuh masih FAIL baseline; status task 🟡.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | npm.ps1 diblokir ExecutionPolicy; tes memakai npm.cmd. Git memberi pemberitahuan konversi LF/CRLF; diff-check tidak menunjukkan whitespace error. |
| Masalah yang diketahui | Metadata backend memakai monitoring/housekeeping/history; kontrak menuntut monitoring/transfer/history. Frontend tidak membuat grant sendiri. Status task sumber 3/4 kriteria lengkap dan 1/4 parsial; acceptance penuh 0/4. |
| Batas pemulihan | Stable intent disimpan dalam memori per pemilik/scope selama sesi aplikasi, bukan penyimpanan permanen lintas full reload browser. Pemulihan setelah reload penuh/keluar sesi belum dibuktikan; jangan menganggap unknown selesai atau membuat key baru untuk maksud operasi yang sama tanpa rekonsiliasi. |
| Dependency backend | `BE-RWI-211`; BE source 🟡 dan migration/runtime NOT_RUN. BM-G01 kelas resmi, BM-G02 SOP, BM-G03 scope/privacy akun aktual dan BM-G04 cutover tetap mengikuti cakupan masing-masing, belum ditutup. |
| Perubahan sampingan | NONE oleh task ini. package-lock.json dan yarn.lock dirty sejak awal, SHA256 tetap. Working tree backend user/source/migration yang sudah ada tidak disentuh. |
| Interupsi | Sesi dilanjutkan melalui instruksi continue; status/diff diperiksa sebelum melanjutkan. Tidak ada rollback atau penggandaan perubahan. |
| Status Git | Source/test tabel3.2: M atau ?? sesuai Git; laporan ini baru ??, frontend-roadmap-bed-management dan requirement-traceability-bed-management ?? sejak planning; requirement-traceability.md M. Seluruh perubahan belum di-stage/commit. |
| Langkah berikutnya | Pengguna menjalankan build/lint manual; pemilik backend menyelaraskan metadata transfer/evidence historis bila diwajibkan kontrak; jalankan aplikasi/fixture/role dan BM-AT scoped, lalu perbarui laporan ini dengan bukti. |
