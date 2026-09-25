# Laporan Perubahan Frontend — `FE-HMD-12`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-12` |
| Judul | Layar Jadwal dan Daftar Kerja Harian Unit Hemodialisa (`FE-HMD-04`) |
| Slice | `MVP-3` — Penjadwalan Sesi dan Daftar Kerja Harian Unit HD |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.4 |
| Trace | `FR-HMD-030`, `FR-HMD-032`, `FE-HMD-04`; `03-frontend-architecture.md` Bagian 4.1; `contracts/api-contract.md` grup Hemodialysis Schedule |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk urutan kolom tabel, warna chip status, dan tata letak penyaring. **Pengecualian mengikat** yang tetap berlaku: penanda isolasi hanya boleh berbunyi "Perlu Isolasi" atau "—", tanpa menyebut diagnosis serologi apa pun |
| Keputusan UI Gate | **11 elemen terverifikasi**: `REUSE 9, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-02` (selesai), `BE-HMD-10` (selesai 22 September 2026), `BE-HMD-11` (selesai 22 September 2026) |
| Klasifikasi | `MEDIUM` — skor 9: repository 0, berkas diperiksa 10, berkas dibuat 6, berkas diubah 5, logika 2, kontrak API 3, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini beserta tautan buktinya pada roadmap dan `requirement-traceability.md` modul Hemodialisa |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source; ESLint `src/` 0 error; unit test baru 20 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. Rute `/health-services/hemodialysis-management/worklist` **sudah terdaftar** dan sudah punya butir menu sidebar sejak `FE-HMD-01`, tetapi isinya masih halaman sementara berisi satu `Hero` tanpa data apa pun.
2. `hemodialysisWorklistSlice` dan `hmdScheduleService` sudah dibentuk pada `FE-HMD-02`, tetapi **kontraknya belum cocok dengan backend yang sekarang sudah jadi**. Tiga ketidakcocokan ditemukan dengan membaca source backend `BE-HMD-11`:
   - Penyaring bawaan memakai kunci `status`, `machineId`, `stationId`, dan `page`, sedangkan `HmdWorklistQuery` backend menerima `Date`, `Shift`, `SessionStatus`, `ServiceUnitId`, `NeedsDocumentation`, `Search`, `PageNumber`, dan `PageSize`. Penyaring `machineId` dan `stationId` tidak ada sama sekali di backend.
   - Pembacaan hasil berhalaman mencari `totalCount`, `page`, dan `totalPages`, sedangkan `Responses/PagedResult.cs` mengirim `TotalData`, `PageNumber`, dan `TotalPage`. Akibatnya jumlah data dan jumlah halaman akan terbaca nol dan paginasi tidak akan pernah berpindah halaman.
   - Tanggal bawaan dihitung dengan `new Date().toISOString().split("T")[0]`, yang menghasilkan tanggal **UTC** dan membeku pada saat berkas di-import. Pada zona WIB, membuka layar sebelum pukul 07.00 akan menampilkan daftar kerja **hari kemarin**.
3. Dua endpoint pembantu daftar kerja sudah tersedia di backend tetapi belum punya pemanggil di frontend: `GET /worklist/summary` dan `GET /worklist/filters/metadata`.
4. Konstanta shift unit HD (`HmdShift`: Morning = 1, Afternoon = 2, Evening = 3) belum ada di berkas konstanta modul; satu-satunya definisi yang ada tertanam di utilitas layar kesiapan unit.
5. Route ruang kerja sesi `/sessions/{sessionId}` — tujuan tombol "Buka Sesi" menurut `03-frontend-architecture.md` Bagian 3.3 — belum ada sama sekali.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Perawat pelaksana unit hemodialisa dan koordinator unit HD. Ini layar yang paling sering dibuka sepanjang hari kerja unit.

**Alur normal.**

1. Pukul 07.00 perawat membuka menu **Hemodialisa → Jadwal & Daftar Kerja**. Layar langsung membuka tanggal hari ini menurut jam dinding perangkat, tanpa perlu mengisi apa pun.
2. Di bagian atas muncul enam kartu ringkasan: Total Sesi Hari Ini, Terjadwal, Persiapan, Sedang Berjalan, Menunggu Pengesahan, dan Disahkan. Angkanya dibaca dari server, bukan dihitung dari baris yang kebetulan sedang tampil, sehingga tetap benar walaupun daftarnya berhalaman.
3. Perawat memilih **Shift Pagi**. Begitu satu shift dipilih, muncul pita kesiapan unit di atas tabel — hijau bertuliskan "Kesiapan unit shift ini: SIAP" bila lembar kesiapan sudah dinyatakan siap, merah "BELUM SIAP" beserta alasannya, atau kuning bila lembarnya masih draf atau belum dibuat. Pita ini selalu membawa tombol **Lihat Kesiapan Unit** yang membuka lembar kesiapan.
4. Tabel menampilkan sesi shift itu, urut menaik menurut jam mulai lalu nomor station — urutan ini datang dari server dan sengaja tidak diacak ulang di layar. Setiap baris memuat jam jadwal beserta rentang jamnya, nama pasien dan No. RM, nomor station, nomor mesin, penanda isolasi, nama perawat penanggung jawab beserta DPJP-nya, chip status sesi, dan tombol **Buka Sesi**.
5. Pasien yang memerlukan alokasi isolasi ditandai lencana kuning bertuliskan **"Perlu Isolasi"**; yang tidak, ditandai tanda hubung. Contoh nyata: pasien di station HD-03 memerlukan isolasi, maka barisnya membawa lencana itu — **tanpa** menyebut penyakit atau hasil pemeriksaan serologi apa pun, karena layar ini dilihat banyak orang sekaligus di ruang terbuka.
6. Perawat menekan **Buka Sesi** pada baris pasiennya dan berpindah ke ruang kerja sesi di `/sessions/{sessionId}`.
7. Selama layar terbuka, daftar menyegarkan dirinya sendiri setiap 60 detik. Baris yang sedang dibaca **tidak** dikosongkan saat penyegaran berjalan; hanya keterangan kecil di atas tabel yang berubah menjadi "Menyegarkan daftar kerja...". Setelah selesai, keterangan itu kembali menyebut jam penyegaran terakhir. Tombol **Segarkan Daftar** tersedia bila perawat ingin memaksa pembacaan ulang saat itu juga.
8. Penyegaran otomatis berhenti sendiri ketika tab peramban ditinggalkan, dan langsung menarik data terbaru begitu tab dibuka kembali.

**Jalur tidak normal.**

- **Tidak ada sesi pada tanggal dan shift itu** — layar menampilkan "Tidak ada jadwal kerja hemodialisa untuk shift ini". Bila kekosongan itu disebabkan penyaring yang sedang aktif, kalimatnya berganti menjadi ajakan mengatur ulang penyaring beserta tombolnya.
- **Daftar kerja gagal dimuat** — layar menampilkan "Daftar kerja gagal dimuat" beserta tombol "Coba lagi".
- **Status kesiapan gagal dibaca** — hanya pitanya yang berubah menjadi kuning "Status kesiapan tidak dapat dimuat". Tabel daftar kerja **tetap tampil dan tetap dapat dipakai**, sesuai `03-frontend-architecture.md` Bagian 4.1.
- **Pengguna tanpa hak akses** — tombol yang tidak berhak disembunyikan, bukan ditampilkan lalu ditolak server. Pengguna tanpa `HemodialysisSession : Read` tidak melihat tombol "Buka Sesi"; tanpa `HemodialysisSchedule : Create` tidak melihat tombol "Jadwalkan Sesi Baru"; tanpa `HemodialysisUnitReadiness : Read` tidak melihat pita kesiapan sama sekali.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Dokumen tata kelola dan blueprint.** `QuilvianSystemFrontendDev/AGENTS.md`; `rules/frontend/frontend-architecture.md`; `rules/frontend/design-tokens.md`; `rules/frontend/base-component-catalog.md`; `rules/frontend/base-component-decision-gate.md`; `rules/frontend/page-composition-patterns.md`; `rules/frontend/ui-consistency-checklist.md`; `rules/frontend/test-policy.md`; `roadmap/frontend-roadmap.md`; `roadmap/backend-roadmap.md`; `03-frontend-architecture.md`; `contracts/api-contract.md`.

**Source backend sebagai sumber kontrak (read-only).** `Areas/HealthServices/HemodialysisManagement/Controllers/HmdScheduleController.cs`; `Services/HmdScheduleService.cs`; `DTOs/HmdSessionDtos.cs`; `DTOs/HmdUnitReadinessDtos.cs`; `Controllers/HmdUnitReadinessController.cs`; `Enums/HemodialysisEnums.cs`; `Responses/PagedResult.cs`; `Program.cs`.

**Source frontend sebagai rujukan pola.** `src/components/features/base-features/data-table.jsx`; `.../status-badge.jsx`; `.../information-alert.jsx`; `.../confirm-modal.jsx`; `.../filter-select.jsx`; `.../summary-grid.jsx`; `.../base-button.jsx`; `src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx`; modul rujukan visual **Permintaan Hemodialisa Masuk** (`orders`) dan **Master Mesin HD** (`master-data/machines`); `src/lib/state/store.jsx`; `src/lib/state/slice/auth/permission-slice.jsx`; `src/lib/services/.../hmdUnitReadinessService.js`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js` | Menambah `HMD_SHIFT` dan `HMD_SHIFT_LABELS`. Menyelaraskan `HMD_DEFAULT_WORKLIST_FILTERS` dengan `HmdWorklistQuery`: kunci `status` menjadi `sessionStatus`, `page` menjadi `pageNumber`, menambah `serviceUnitId`, `needsDocumentation`, dan `search`, serta membuang `machineId` dan `stationId` yang tidak didukung backend. Tanggal bawaan dikosongkan agar tidak membeku saat berkas di-import |
| `src/lib/services/health-services/hemodialysis-management/hmdScheduleService.js` | Menambah `getHmdWorklistSummary` (`GET /worklist/summary`) dan `getHmdWorklistFilterMetadata` (`GET /worklist/filters/metadata`) |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisWorklistSlice.js` | Memperbaiki pembacaan `PagedResult` agar memakai `totalData`, `pageNumber`, dan `totalPage`. Menambah thunk `fetchHmdWorklistSummary` dan `fetchHmdShiftReadiness`. Meneruskan `signal` pembatalan ke Axios dan mengabaikan penolakan akibat pembatalan agar tidak memunculkan pesan gagal palsu |
| `src/utils/health-services/hemodialysis-management/hemodialysis-worklist-display-utils.js` | **Baru.** Fungsi murni layar: `getTodayLocalDateString`, `buildWorklistQuery`, `resolveSessionStatusBadge`, `resolveWorklistIsolationBadge`, `resolveShiftReadinessBanner`, `formatWorklistTime`, `formatWorklistTimeRange`, `buildWorklistSummaryCards`, `getWorklistRowId`, beserta opsi penyaring shift dan status sesi |
| `src/lib/hooks/health-services/hemodialysis-management/use-hemodialysis-worklist.jsx` | **Baru.** Controller layar: penyaring, pembacaan berhalaman, ringkasan, pita kesiapan, penyegaran 60 detik yang sadar visibilitas tab, pembatalan permintaan, dan penegakan hak akses |
| `src/components/view/health-services/hemodialysis-management/worklist/hemodialysis-worklist-view.jsx` | **Baru.** Komposisi layar |
| `src/components/view/health-services/hemodialysis-management/worklist/hemodialysis-worklist-table-columns.jsx` | **Baru.** Sembilan definisi kolom tabel daftar kerja |
| `src/components/view/health-services/hemodialysis-management/worklist/modals/schedule-session-modal.jsx` | **Baru.** Kerangka dialog penjadwalan; isinya cakupan `FE-HMD-13` |
| `src/style/health-services/hemodialysis-management/hemodialysis-worklist.module.css` | **Baru.** Seluruh nilai visual memakai `var(--token)` tanpa literal warna maupun penimpaan typography |
| `src/app/health-services/hemodialysis-management/worklist/page.jsx` | Halaman sementara diganti route tipis berisi metadata dan pemanggilan view |
| `src/app/health-services/hemodialysis-management/sessions/[sessionId]/page.jsx` | **Baru.** Route sementara tujuan tombol "Buka Sesi" agar tidak menghasilkan 404; badannya diganti `FE-HMD-14` |
| `tests/unit/hemodialysis-worklist.test.mjs` | **Baru.** 20 unit test fungsi murni layar |
| `tests/unit/hemodialysis-client-and-redux-slice.test.mjs` | Dua baris pemeriksaan `state.filters.page` disesuaikan menjadi `state.filters.pageNumber` mengikuti kontrak backend yang benar |

### 3.3 Kepatuhan Arsitektur Frontend

Alur dependensi mengikuti `rules/frontend/frontend-architecture.md` tanpa pengecualian:

```text
src/app/.../worklist/page.jsx      -> route tipis, hanya metadata
  -> components/view/.../worklist   -> komposisi layar
  -> lib/hooks/.../use-hemodialysis-worklist.jsx -> controller
  -> lib/state/slice/.../hemodialysisWorklistSlice.js -> thunk dan state
  -> lib/services/.../hmdScheduleService.js + hmdUnitReadinessService.js
  -> lib/axiosInstance/InstanceAxios -> Backend API
```

- View tidak memanggil Axios langsung dan tidak menormalisasi ulang payload.
- Definisi kolom tabel diletakkan pada berkas `<feature>-table-columns.jsx` terpisah.
- Endpoint tetap berasal dari `HEMODIALYSIS_API`; tidak ada string endpoint yang tersebar di view.
- Reducer `hemodialysisWorklist` sudah terdaftar di `src/lib/state/store.jsx` sejak `FE-HMD-02`; tidak ada pendaftaran ganda.
- Tidak ada Axios instance baru, arsitektur state paralel, factory, generator, atau base component baru.

**Catatan kontrak enum.** `Program.cs` backend tidak memasang `JsonStringEnumConverter`, sehingga seluruh enum dikirim sebagai **angka** dan nama bacanya dikirim terpisah pada `ShiftName`, `SessionStatusName`, dan `ReadinessStatusName`. Seluruh resolver di layar ini menerima angka, angka dalam bentuk teks, maupun nama enum secara defensif, lalu memakai field `*Name` sebagai cadangan bila nilai enumnya belum dikenali.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Muat pertama menampilkan kartu memuat `ClinicalStateBoundary` beserta kalimat "Memuat daftar kerja hemodialisa..." dan kerangka lima kartu ringkasan. Penyegaran berkala 60 detik **tidak** mengosongkan tabel; hanya keterangan di atas tabel berubah menjadi "Menyegarkan daftar kerja..." |
| Kosong | "Tidak ada jadwal kerja hemodialisa untuk shift ini" beserta "Belum ada sesi cuci darah yang dijadwalkan pada tanggal dan shift yang dipilih." Bila ada penyaring aktif, kalimatnya berganti menjadi ajakan mengatur ulang penyaring beserta tombol "Atur Ulang Penyaring" |
| Gagal | "Daftar kerja gagal dimuat" beserta tombol "Coba lagi". Kegagalan membaca status kesiapan **tidak** menahan tabel; hanya pitanya berubah menjadi "Status kesiapan tidak dapat dimuat" dan tombol "Lihat Kesiapan Unit" tetap tersedia |
| Tanpa hak akses | Tombol yang tidak berhak disembunyikan. Tanpa `HemodialysisSession : Read` kolom aksi menampilkan tanda hubung; tanpa `HemodialysisSchedule : Create` tombol "Jadwalkan Sesi Baru" tidak dirender; tanpa `HemodialysisUnitReadiness : Read` pita kesiapan tidak dirender |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions/worklist` | Baris tabel daftar kerja beserta paginasi server | `HemodialysisSchedule : Read` |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions/worklist/summary` | Angka enam kartu ringkasan sesi pada tanggal terpilih | `HemodialysisSchedule : Read` |

#### Health Services / Hemodialysis Management / Hemodialysis Unit Readiness

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness` | Status kesiapan unit pada tanggal dan shift terpilih untuk pita di atas tabel | `HemodialysisUnitReadiness : Read` |

**Delta terhadap `contracts/api-contract.md`.** Tabel kontrak grup Hemodialysis Schedule memuat enam endpoint dan **belum** mencantumkan `GET /worklist/summary` maupun `GET /worklist/filters/metadata`, padahal keduanya sudah ada di `HmdScheduleController` hasil `BE-HMD-11`. Layar ini memakai `/worklist/summary` karena backend adalah bukti otoritatif atas perilaku runtime as-is, dan pemakaiannya hanya menambah angka ringkasan tanpa mengubah perilaku yang dikunci kontrak. `GET /worklist/filters/metadata` sudah disediakan pemanggilnya di lapisan service tetapi **belum dipakai layar**; opsi shift dan status sesi masih disusun dari konstanta frontend supaya penyaring tetap dapat dipakai tanpa satu permintaan tambahan saat layar dibuka.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src --quiet` | Selesai dengan exit code 0, tanpa keluaran | `PASS` | Tidak ada error pada seluruh `src/` |
| `npx eslint <8 berkas yang disentuh task ini>` | 0 error, 0 warning | `PASS` | Keluaran perintah kosong |
| `npm run lint:errors` | Gagal sebelum memeriksa berkas mana pun: `A configuration object specifies rule "react-hooks/rules-of-hooks", but could not find plugin "react-hooks"` | `EXISTING / ENVIRONMENT ISSUE` | Penyebabnya folder `test-with-agy/` — folder kerja lokal yang di-`.gitignore` (baris 79) tetapi tidak dikecualikan `eslint.config.mjs`. Pemeriksaan per direktori membuktikan hanya `test-with-agy` yang memicunya, sedangkan `src`, `tests`, `scripts`, `server.js`, `public`, `docs`, dan `agency-local` bersih |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-worklist.test.mjs` | 20 test, 20 lolos, 0 gagal | `PASS` | Berkas test baru milik task ini |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | 1617 test, 1610 lolos, 7 gagal | `PASS` untuk cakupan task ini | Ketujuh kegagalan berada di `accounting-reconciliation.test.mjs`, `inpatient-physician-entry.test.mjs`, `inpatient-physician-workspace.test.mjs`, dan `inpatient-supporting-service-v2.test.mjs` — tidak satu pun berkasnya disentuh task ini |
| `npm run test:unit` | Gagal dengan `ERR_UNSUPPORTED_DIR_IMPORT` pada `tests/unit` | `EXISTING / ENVIRONMENT ISSUE` | Node 24.13.0 menolak direktori sebagai argumen `--test` melalui `tests/helpers/alias-resolver.mjs`. Sudah dikenal sebelum task ini; suite dijalankan memakai pola glob sebagai gantinya |
| Grep anti-regresi konsistensi UI, 8 pemeriksaan | Seluruhnya kosong | `PASS` | Tidak ada warna literal, penimpaan typography, `<button>` mentah, `<table>` mentah, utility typography Bootstrap, `!important`, inline style, maupun blok dark mode pada berkas baru |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Sesi ini tidak memiliki server pengembangan frontend maupun backend yang berjalan beserta sesi login berotorisasi, sehingga penyaring, paginasi, pita kesiapan, dan navigasi tombol tidak dapat dibuktikan di layar sungguhan |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026: build dijalankan pemilik secara mandiri setelah source selesai |

**Uji manual:** `NOT FEASIBLE`.

Kontrol interaktif yang **belum** dibuktikan di peramban: daftar opsi dan keadaan terpilih penyaring shift serta status sesi, efek penyaring terhadap permintaan dan hasil, tombol atur ulang, penyaring gabungan, perpindahan halaman, penyegaran 60 detik, pembukaan dan penutupan dialog penjadwalan, serta navigasi tombol "Buka Sesi" dan "Lihat Kesiapan Unit".

Yang **sudah** dibuktikan di tingkat logika oleh 20 unit test: penyusunan query yang benar-benar dikirim untuk setiap kombinasi penyaring, pembuangan penyaring kosong, batas bawah nomor halaman, pemetaan dua belas status sesi ke tone lencana yang dikenali `StatusBadge`, kejujuran penanda isolasi termasuk pemeriksaan bahwa tidak ada kata "hiv", "hepatitis", "hbsag", "anti-hcv", maupun "serologi" yang bocor ke teks mana pun, ketiga varian pita kesiapan beserta jalur gagalnya, penggunaan tanggal lokal alih-alih UTC, dan penyusunan angka kartu ringkasan.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, dan `npm run test:uat`. Ketiganya dikecualikan atas keputusan tetap pemilik; e2e juga tidak didukung karena repository tidak memiliki `playwright.config.*`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Perawat membuka worklist shift pagi, melihat pasien terjadwal di station HD-01 s/d HD-08, pasien di HD-03 memiliki badge isolasi "Perlu", dan menekan "Buka Sesi" untuk masuk ke ruang kerja sesi | Terpenuhi | Kolom station, mesin, dan isolasi ada di `hemodialysis-worklist-table-columns.jsx`; penanda isolasi dihasilkan `resolveWorklistIsolationBadge` dan hanya berbunyi "Perlu Isolasi" atau "—"; tombol "Buka Sesi" memanggil `handleOpenSession` yang menuju `HEMODIALYSIS_ROUTES.sessionWorkspace(sessionId)`, dan route tujuannya kini ada sehingga tidak berakhir 404 |
| **AC-2** Data otomatis disegarkan bila ada perubahan status sesi atau pembaruan penjadwalan | Terpenuhi | `use-hemodialysis-worklist.jsx` memasang penyegaran berkala 60 detik yang berhenti saat tab tidak terlihat dan menarik data terbaru saat tab dibuka kembali, ditambah tombol "Segarkan Daftar" dan keterangan jam penyegaran terakhir di atas tabel. Ini mitigasi yang memang disebut roadmap; backend belum menyediakan kanal dorong untuk daftar kerja |

**Definition of Done** — "Layar daftar kerja harian berfungsi sesuai spesifikasi kawat layar `FE-HMD-04`, integrasi query filter dan tombol buka sesi terbukti lancar":

| Butir | Status |
| --- | --- |
| Saringan tanggal, shift, status, dan pencarian pasien | Terpenuhi |
| Pita kesiapan unit yang dapat diklik menuju lembar kesiapan | Terpenuhi |
| Tabel jadwal kerja lengkap sembilan kolom sesuai kawat layar | Terpenuhi |
| Penanda isolasi hanya "Perlu" atau "—" tanpa menyebut serologi | Terpenuhi |
| Tombol "Buka Sesi" dan penyembunyian tombol tanpa hak akses | Terpenuhi |
| Tombol "Jadwalkan Sesi Baru" membuka dialog penjadwalan | Terpenuhi sebatas batas slice — tombol, penegakan hak akses, keadaan buka/tutup, dan kerangka dialognya terpasang. **Isi formulir penjadwalannya adalah cakupan `FE-HMD-13`**, yang Definition of Done-nya memang berbunyi "dialog penjadwalan terpasang pada layar worklist" |
| Bukti verifikasi berupa component test `WorklistDailyViewTests` | Tidak dipenuhi dalam bentuk yang disebut roadmap. Repository ini tidak memakai Jest maupun `@testing-library`, sehingga component test tidak dapat ditulis tanpa menambah dependency baru. Sebagai gantinya ditulis 20 unit test `node:test` atas fungsi murni layar. Butir DoD berbentuk test dilepas atas keputusan pemilik 1 September 2026 |

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan `react-hooks/set-state-in-effect` sempat muncul pada hook dan **sudah dihilangkan** dengan membuang state `lastRefreshedAt` yang ternyata tidak dipakai layar; jam penyegaran terakhir dibaca dari `worklistLoadedAt` milik Redux. Hasil akhir: 0 error, 0 warning pada seluruh berkas task ini |
| Masalah yang diketahui | **Tiga temuan di luar cakupan, tidak diperbaiki, dilaporkan apa adanya.** (1) `hemodialysis-orders-table-columns.jsx` dari `FE-HMD-08` memakai kunci kolom `label` dan tanda tangan `render(val, row, index)`, sedangkan `DataTable` membaca `header` dan memanggil `render(item, meta)` — akibatnya judul kolom tabel Permintaan Masuk kosong dan isi selnya salah. (2) `hemodialysis-patients-table-columns.jsx` dari `FE-HMD-09` memakai `header` + `accessor` + `cell` tanpa `key` dan tanpa `render`, sehingga seluruh sel tabel Daftar Pasien jatuh ke tanda hubung. (3) `resolveReadinessBadge` pada `hemodialysis-unit-readiness-display-utils.js` membandingkan status dengan teks `"Ready"`/`"NotReady"`, padahal backend mengirim angka, sehingga hasilnya selalu jatuh ke cabang bawaan "Draf". Ketiganya adalah cacat pada task yang sudah ditandai selesai dan memerlukan keputusan pemilik untuk dijadwalkan ulang |
| Dependency backend | `BE-HMD-10` dan `BE-HMD-11` sudah selesai 22 September 2026; tidak ada dependency backend yang tertahan untuk layar ini |
| Perubahan sampingan | Dua baris pemeriksaan pada `tests/unit/hemodialysis-client-and-redux-slice.test.mjs` disesuaikan dari `state.filters.page` menjadi `state.filters.pageNumber`. Pemeriksaan itu menjadi usang karena penyelarasan kunci paginasi dengan `HmdWorklistQuery`; nilai yang lama justru tidak pernah cocok dengan backend |
| Interupsi | `NONE` |
| Status Git | Lihat bagian 8.1 |
| Langkah berikutnya | Kerjakan `FE-HMD-13` — isi kerangka dialog `schedule-session-modal.jsx` dengan formulir penjadwalan, peringatan rasio perawat, dan penanganan benturan `409` |

### 8.1 Status Git

`git status --short` pada `QuilvianSystemFrontendDev` di akhir pekerjaan. Berkas yang **bukan** milik task ini berasal dari `FE-HMD-07` s/d `FE-HMD-11` yang juga belum di-commit.

```text
 M src/app/health-services/hemodialysis-management/orders/page.jsx
 M src/app/health-services/hemodialysis-management/patients/page.jsx
 M src/app/health-services/hemodialysis-management/worklist/page.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx
 M src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-tab.jsx
 M src/lib/services/health-services/hemodialysis-management/hmdEpisodeService.js
 M src/lib/services/health-services/hemodialysis-management/hmdOrderService.js
 M src/lib/services/health-services/hemodialysis-management/hmdScheduleService.js
 M src/lib/state/slice/health-services/hemodialysis-management/hemodialysisWorklistSlice.js
 M src/lib/state/store.jsx
 M src/utils/health-services/hemodialysis-management/hemodialysis-order-display-utils.js
 M tests/unit/hemodialysis-client-and-redux-slice.test.mjs
?? src/app/health-services/hemodialysis-management/patients/[patientId]/
?? src/app/health-services/hemodialysis-management/sessions/
?? src/components/view/health-services/hemodialysis-management/orders/
?? src/components/view/health-services/hemodialysis-management/patients/
?? src/components/view/health-services/hemodialysis-management/worklist/
?? src/lib/hooks/health-services/hemodialysis-management/use-hemodialysis-worklist.jsx
?? src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js
?? src/lib/state/slice/health-services/hemodialysis-management/hemodialysisOrderSlice.js
?? src/style/health-services/hemodialysis-management/hemodialysis-episode-workspace.module.css
?? src/style/health-services/hemodialysis-management/hemodialysis-orders.module.css
?? src/style/health-services/hemodialysis-management/hemodialysis-patients.module.css
?? src/style/health-services/hemodialysis-management/hemodialysis-worklist.module.css
?? src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js
?? src/utils/health-services/hemodialysis-management/hemodialysis-worklist-display-utils.js
?? tests/unit/hemodialysis-episode-workspace.test.mjs
?? tests/unit/hemodialysis-order-worklist.test.mjs
?? tests/unit/hemodialysis-patient-list.test.mjs
?? tests/unit/hemodialysis-prescription.test.mjs
?? tests/unit/hemodialysis-worklist.test.mjs
```

Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch yang dilakukan.

---

## 9. Tabel Keputusan Base Component

`UI GATE: 11 elemen — REUSE 9, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
| --- | --- | --- | --- | --- |
| Header halaman | `Hero` | `base-features/hero.jsx` | `REUSE` | `eyebrow`, `title`, `description`, dan `actions` berisi dua tombol |
| Kartu ringkasan sesi | `SummaryGrid` | `base-features/summary-grid.jsx` | `REUSE` | Enam kartu `{key, label, value}`, `minWidth` 165 |
| Bilah penyaring dan pencarian | `DataFilter` | `base-features/data-filter.jsx` | `REUSE` | Pencarian berikut debounce, sanitasi, dan tombol atur ulang milik komponen |
| Penyaring tanggal | `FilterDatePicker` | `base-features/filter-date-picker.jsx` | `REUSE` | — |
| Penyaring shift dan status sesi | `FilterSelect` | `base-features/filter-select.jsx` | `REUSE` | Opsi `{value,label}` dari konstanta domain |
| Pita kesiapan shift beserta tombolnya | `InformationAlert` + `BaseButton` | `information-alert.jsx` menerima `children` | `COMPOSE` | Dirangkai di view; tanpa perubahan base sama sekali |
| Tabel daftar kerja | `DataTable` | `data-table.jsx` | `REUSE` | `sortLatestFirst={false}` karena backend sudah menyortir menaik menurut jam mulai |
| Chip status sesi dan penanda isolasi | `StatusBadge` | `status-badge.jsx` menyediakan lima tone | `REUSE` | Dua belas status dipetakan ke lima tone lewat prop `status` dan `label` |
| Tombol aksi | `BaseButton` | `base-button.jsx` | `REUSE` | Tidak ada `<button>` mentah maupun `.btn` Bootstrap |
| Paginasi | `Pagination` | `features/pagination/pagination` | `REUSE` | Diteruskan lewat `PaginationComponent` |
| Kerangka dialog penjadwalan | `ConfirmModal` | `confirm-modal.jsx` dengan `hideFooter` | `COMPOSE` | Badan dialog diisi `FE-HMD-13` |

Tiga keputusan cakupan yang diambil beserta alasannya, seluruhnya opsi paling konservatif yang tetap memenuhi acceptance criteria:

1. **Opsi penyaring status sesi memakai dua belas status konkret, bukan empat kelompok ringkas.** `HmdWorklistQuery.SessionStatus` hanya menerima satu nilai enum, sehingga pengelompokan hanya mungkin dilakukan di sisi klien dan akan membuat jumlah data serta paginasi server salah. Daftar dua belas status merupakan superset dari kelompok yang disebut roadmap.
2. **Tombol "Jadwalkan Sesi Baru" dirender aktif beserta kerangka dialognya.** Jalur interaksinya utuh dan dapat diuji sekarang, dan `FE-HMD-13` cukup mengganti badan dialog tanpa mengubah tanda tangan propsnya.
3. **Route `/sessions/{sessionId}` dibuat sebagai halaman sementara.** Tanpa itu, tombol utama layar ini berakhir 404 dan `AC-1` tidak dapat dibuktikan. Bentuknya mengikuti halaman sementara yang sudah dipakai `FE-HMD-01`, dan `FE-HMD-14` menimpanya.
