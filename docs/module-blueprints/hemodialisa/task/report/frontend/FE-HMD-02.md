# Laporan Perubahan Frontend — `FE-HMD-02`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-02` |
| Judul | Manajemen State Redux, Axios API Services, Constants, dan Hook Transisi Status |
| Slice | `MVP-0` — Navigasi, State Management, dan Fondasi Modul |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.1 |
| Trace | `03-frontend-architecture.md` Bagian 6 & 7; `contracts/api-contract.md`; `contracts/state-transition-matrix.md` |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk salinan teks status, penamaan helper error, dan struktur slice Redux sesuai pola standardisasi Quilvian |
| Dependency | `FE-HMD-01` (selesai) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 8, berkas dibuat 12, berkas diubah 1, logika 2, kontrak API 2, database 0, UI 0 (data layer) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/constants/health-services/hemodialysis-management/**`, `src/lib/services/health-services/hemodialysis-management/**`, `src/lib/hooks/health-services/hemodialysis-management/**`, `src/lib/state/slice/health-services/hemodialysis-management/**`, `src/lib/state/store.jsx`, `tests/unit/hemodialysis-client-and-redux-slice.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `f09426938` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis dan build Next.js sukses |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini dijalankan, modul Hemodialisa di frontend belum memiliki lapisan data (*data layer*), yang meliputi:
1. **Belum ada Axios client service** untuk memanggil endpoint backend yang sudah ditetapkan pada kontrak `HMD-CONTRACT-v1` (meliputi Order, Episode, Prescription, Schedule, Session, Unit Readiness, dan Master Data Resources).
2. **Belum ada konstanta modul terpusat**, sehingga alamat endpoint, nama status enum, rute App Router, dan pesan error berpotensi ditulis sebagai teks lepas (*hardcoded string*) yang rentan salah ketik.
3. **Belum ada Redux slice terdaftar** untuk caching data lokal, penyaringan daftar kerja (*worklist*), alur kerja klinis sesi dialisis, dan master data.
4. **Belum ada aturan transisi status di sisi klien**, sehingga komponen UI tidak memiliki acuan terpadu untuk menonaktifkan atau menyembunyikan tombol aksi ilegal (misalnya membatalkan sesi yang sedang berjalan atau mengedit resep aktif).
5. **Risiko State Kotor (*Stale State*) Antar Pasien**: Belum ada mekanisme reset state eksplisit saat pengguna berpindah dari ruang kerja satu pasien ke pasien lainnya, yang berisiko mencatat observasi klinis ke pasien yang salah.

---

## 2. Proses bisnis dari sisi pengguna

1. **Pemanggilan API Terstandar**: Setiap tindakan pengguna pada layar operasional (memuat daftar kerja, menjadwalkan sesi, mencatat observasi) memanggil Axios service terisolasi yang otomatis membongkar format *envelope* backend `ApiResponse<T>` dan menangani error secara defensif.
2. **Deteksi Sesi Terkunci (HTTP 423 Locked)**:
   - Skenario: Dokter atau perawat membuka sesi yang sudah disahkan secara hukum klinis (`Finalized`).
   - Apabila terjadi upaya manipulasi data atau pengiriman data langsung ke backend, service mendeteksi status `HTTP 423 Locked`.
   - Antarmuka tidak menampilkan error teknis "Request failed with status code 423", melainkan menampilkan notifikasi informatif: *"Catatan sesi hemodialisa ini sudah disahkan secara final dan terkunci secara hukum klinis. Perubahan data hanya dapat dilakukan melalui Addendum Rekam Medis."* dan menyajikan tombol pembuka tab addendum.
3. **Pencegahan Aksi Ilegal via Hook Transisi Status**:
   - Ketika perawat melihat sesi dengan status `InProgress` (sedang cuci darah), hook `useHemodialysisStatusTransition` secara otomatis menonaktifkan tombol `Start` dan `Cancel`, dan hanya mengaktifkan tombol pemantauan (`Add Observation`, `Add Medication`, `Add Complication`), `Stop`, atau `Complete`.
   - Dokter tidak dapat menyunting resep berstatus `Active`; perubahan instruksi dialisis diarahkan untuk membuat resep baru yang menggantikan (*superseded*) resep lama.
   - Koordinator unit tidak dapat mengubah status mesin menjadi `Maintenance` atau `Blocked` bila mesin tersebut sedang dipakai dalam sesi aktif.
4. **Mitigasi Data Kotor Antar Pasien**:
   - Ketika perawat berpindah dari ruang kerja sesi Tn. A ke Ny. B, sistem mengeksekusi aksi Redux `resetSessionState()`.
   - Seluruh detail pasien sebelumnya, riwayat checklist, dan observasi langsung dibersihkan sebelum data pasien baru dimuat, mencegah risiko pencatatan ke rekam medis yang salah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 6 & 7
- `docs/module-blueprints/hemodialisa/contracts/api-contract.md`
- `docs/module-blueprints/hemodialisa/contracts/state-transition-matrix.md`
- `docs/module-blueprints/hemodialisa/contracts/validation-matrix.md`
- `QuilvianSystemFrontendDev/src/lib/axiosInstance/InstanceAxios.jsx`
- `QuilvianSystemFrontendDev/src/lib/state/store.jsx`
- `QuilvianSystemFrontendDev/src/lib/state/slice/health-services/radiology-management/rad-order-slice.jsx`

### 3.2 Berkas yang dibuat dan berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js` | Berkas baru. Konstanta alamat API (`HEMODIALYSIS_API`), rute navigasi (`HEMODIALYSIS_ROUTES`), enum status (`HMD_ORDER_STATUS`, `HMD_EPISODE_STATUS`, `HMD_PRESCRIPTION_STATUS`, `HMD_SESSION_STATUS`, `HMD_MACHINE_STATUS`, `HMD_STATION_STATUS`, `HMD_READINESS_STATUS`, `HMD_COMPETENCY_STATUS`), label Bahasa Indonesia, salinan teks state (`HEMODIALYSIS_STATE_COPY`), dan filter default |
| `src/lib/services/health-services/hemodialysis-management/hmdServiceHelper.js` | Berkas baru. Helper pembongkaran envelope `unwrapApiResponse` (mendukung `data` dan `Data`) dan normalisasi error `normalizeHmdError` dengan deteksi khusus status HTTP 423 Locked dan HTTP 409 Conflict |
| `src/lib/services/health-services/hemodialysis-management/hmdOrderService.js` | Berkas baru. Service API untuk Permintaan HD (`getHmdOrders`, `getHmdOrderById`, `createHmdOrder`, `acceptHmdOrder`, `holdHmdOrder`, `releaseHoldHmdOrder`, `rejectHmdOrder`, `cancelHmdOrder`) |
| `src/lib/services/health-services/hemodialysis-management/hmdEpisodeService.js` | Berkas baru. Service API untuk Episode HD, Penilaian Kelayakan, Akses Vaskular, Serologi, dan Keputusan Isolasi |
| `src/lib/services/health-services/hemodialysis-management/hmdPrescriptionService.js` | Berkas baru. Service API untuk Resep HD (`getHmdPrescriptions`, `getHmdPrescriptionById`, `createHmdPrescription`, `updateHmdPrescription`, `activateHmdPrescription`, `cancelHmdPrescription`) |
| `src/lib/services/health-services/hemodialysis-management/hmdScheduleService.js` | Berkas baru. Service API untuk Daftar Kerja / Penjadwalan Sesi dan Penugasan Petugas (`getHmdWorklist`, `createHmdSession`, `rescheduleHmdSession`, `cancelHmdSession`, `getHmdStaffAssignments`, `assignHmdStaff`) |
| `src/lib/services/health-services/hemodialysis-management/hmdSessionService.js` | Berkas baru. Service API untuk Ruang Kerja Sesi HD (Check-in, Checklist Pra-HD, Penilaian Pra-HD, Ready, Hold, Start, Observasi, Obat, Komplikasi, Stop, Complete, Pasca-HD, Dokumentasi, Finalisasi, Billing Handoff) |
| `src/lib/services/health-services/hemodialysis-management/hmdUnitReadinessService.js` | Berkas baru. Service API untuk Lembar Kesiapan Unit HD (`getHmdUnitReadinessList`, `getHmdUnitReadinessDetail`, `createHmdUnitReadiness`, `saveHmdReadinessItems`, `declareHmdUnitReady`, `declareHmdUnitNotReady`) |
| `src/lib/services/health-services/hemodialysis-management/hmdResourceService.js` | Berkas baru. Service API untuk Master Data Mesin, Station, Butir Checklist Overridable, dan Pengaturan Unit HD |
| `src/lib/hooks/health-services/hemodialysis-management/useHemodialysisStatusTransition.js` | Berkas baru. Hook React dan pure domain functions untuk validasi transisi status sesi, permintaan, resep, mesin, dan kesiapan unit sesuai matriks perpindahan status |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisWorklistSlice.js` | Berkas baru. Redux slice untuk daftar kerja sesi HD, penyaringan tanggal/shift, dan operasi penjadwalan |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisSessionSlice.js` | Berkas baru. Redux slice untuk alur kerja klinis sesi HD dengan penanganan mitigasi stale state (`resetSessionState`), boundary kunci dokumen (`isLocked`), dan sub-resources |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js` | Berkas baru. Redux slice untuk master data mesin, riwayat mesin, station, butir checklist, dan pengaturan unit |
| `src/lib/state/store.jsx` | Mendaftarkan reducer `hemodialysisWorklist`, `hemodialysisSession`, dan `hemodialysisMaster` ke store pusat Redux Quilvian |
| `tests/unit/hemodialysis-client-and-redux-slice.test.mjs` | Berkas baru. Unit test komprehensif menguji keberadaan berkas, pembongkaran envelope, normalisasi error 423/409, logika transisi status sesi/resep/mesin, dan reducers Redux slice |

### 3.3 Kepatuhan arsitektur frontend

- **Pola Komponen Base & UI Gate**:
```text
UI GATE: Data layer task — REUSE Redux store Quilvian, REUSE InstanceAxios, REUSE ApiResponse unwrap pattern. Tidak ada penambahan komponen visual baru pada task ini.
```
- **Pemisahan Boundary**: Seluruh service dan slice berada di folder canonical `src/lib/services/` dan `src/lib/state/slice/` di bawah `health-services/hemodialysis-management/`.
- **Pure Function Decoupling**: Logika evaluasi status transisi diekstrak sebagai *pure functions* di samping hook React, memungkinkan pengujian unit langsung tanpa overhead React DOM renderer.

---

## 4. State yang ditangani di layar

| State | Penanganan di Lapisan State & Service |
| --- | --- |
| **Memuat (*Loading*)** | Setiap slice menyediakan flag loading terisolasi (`worklistLoading`, `detailLoading`, `checklistLoading`, `observationsLoading`, `actionLoading`) sehingga pemuatan satu data tidak mengunci data lain |
| **Kosong (*Empty*)** | Disediakan penanda `loadedAt` (`worklistLoadedAt`, `detailLoadedAt`). Pembeda eksplisit antara data kosong murni vs kegagalan jaringan yang belum pernah memuat data |
| **Gagal (*Error*)** | Pesan kesalahan server dari `body.message` atau `body.title` disimpan apa adanya; fallback informatif tersedia tanpa kalimat generik "Request failed with status code 500" |
| **Konflik (*Conflict 409*)** | Deteksi `isConflict: true` pada `normalizeHmdError` dengan pesan informatif meminta pengguna memuat ulang layar |
| **Terkunci (*Locked 423*)** | Deteksi `isLocked: true` dan `suggestAddendum: true` pada `normalizeHmdError` dan `hemodialysisSessionSlice` yang memandu pengguna membuka Addendum Rekam Medis |

---

## 5. Endpoint yang dikonsumsi

Sesuai dengan kontrak `HMD-CONTRACT-v1` yang dibungkus oleh 7 Axios services:

| Service | Method | Endpoint Path | Kegunaan |
| --- | --- | --- | --- |
| `hmdOrderService` | `GET`, `POST` | `/v1/health-services/hemodialysis-management/hemodialysis-orders` | Daftar, rincian, buat, terima, tahan, lepas tahanan, tolak, batalkan permintaan HD |
| `hmdEpisodeService` | `GET`, `POST`, `PUT`, `PATCH` | `/v1/health-services/hemodialysis-management/hemodialysis-episodes` | Kelola episode HD, penilaian kelayakan, akses vaskular, serologi, dan keputusan isolasi |
| `hmdPrescriptionService` | `GET`, `POST`, `PUT` | `/v1/health-services/hemodialysis-management/hemodialysis-prescriptions` | Kelola draf resep HD, aktivasi (penggantian resep), dan pembatalan resep |
| `hmdScheduleService` | `GET`, `POST`, `PATCH`, `PUT` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions` | Daftar kerja unit (`/worklist`), buat jadwal sesi, jadwal ulang, pembatalan, dan penugasan staf |
| `hmdSessionService` | `GET`, `POST`, `PUT` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions` | Alur sesi dialisis: check-in, checklist Pra-HD, penilaian Pra-HD, ready, hold, start, observasi, obat, komplikasi, stop, complete, Pasca-HD, submit dokumentasi, finalisasi, dan billing handoff |
| `hmdUnitReadinessService` | `GET`, `POST`, `PUT` | `/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness` | Lembar kesiapan unit per shift, penilaian butir air/alat, deklarasi siap/tidak siap |
| `hmdResourceService` | `GET`, `POST`, `PUT`, `PATCH`, `DELETE` | `/v1/health-services/hemodialysis-management/master-data/*` | Master mesin HD, riwayat status mesin, station HD, butir checklist overridable, dan pengaturan unit |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-client-and-redux-slice.test.mjs` | `10 pass, 0 fail, duration 323ms` | `PASS` | 10 skenario unit test lulus: File integrity 13 berkas, unwrap ApiResponse format camel/PascalCase, normalisasi error 423 Locked, normalisasi error 409 Conflict, penonaktifan tombol ilegal sesi InProgress, penguncian sesi Finalized, aturan resep/mesin, dan reducers ketiga slice |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-sidebar-navigation.test.mjs tests/unit/hemodialysis-client-and-redux-slice.test.mjs` | `16 pass, 0 fail, duration 343ms` | `PASS` | Seluruh 16 unit test modul Hemodialisa (navigasi dan data layer) lulus tanpa regresi |
| `node ./node_modules/eslint/bin/eslint.js "src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js" "src/lib/services/health-services/hemodialysis-management/hmd*.js" "src/lib/hooks/health-services/hemodialysis-management/use*.js" "src/lib/state/slice/health-services/hemodialysis-management/hemodialysis*.js" "src/lib/state/store.jsx"` | `0 error(s), 0 warning(s)` | `PASS` | Seluruh berkas data layer bersih dari pelanggaran linter ESLint |
| `node ./node_modules/next/dist/bin/next build` | Build sukses dengan exit code 0 | `PASS` | Seluruh modul dan Redux store terkompilasi 100% tanpa kendala tipe atau modul |

Uji manual: `NOT APPLICABLE` — Task `FE-HMD-02` adalah implementasi data layer tanpa tampilan visual langsung. Logika dan alur state telah diverifikasi penuh via pengujian unit otomatis.

**Tidak dijalankan:** `test:e2e` Playwright karena tidak diminta dan task berfokus pada fondasi data layer tingkat unit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. *Contoh Interceptor Respons*: Service mendeteksi respons backend HTTP 423 Locked pada dokumen final dan menyajikan notifikasi informatif yang membimbing pengguna membuka tab addendum. | Terpenuhi | `normalizeHmdError` mendeteksi status 423, menetapkan `isLocked: true`, `suggestAddendum: true`, dan menyertakan pesan panduan Addendum Rekam Medis (teruji di `tests/unit/hemodialysis-client-and-redux-slice.test.mjs`) |
| 2. Hook status transisi menonaktifkan tombol aksi yang tidak sah (misal: sesi `InProgress` tidak menampilkan tombol `Start` atau `Cancel`). | Terpenuhi | `getAvailableSessionActions` dan `canTransitionSession` melarang transisi `InProgress` -> `Cancelled` / `Planned`, serta memastikan tombol `Start` dan `Cancel` bernilai `false` pada sesi `InProgress` |
| 3. Error serialisasi API tertangkap dengan baik dan memicu state boundary. | Terpenuhi | Thunks pada ketiga Redux slice menggunakan `rejectWithValue` dengan payload terstruktur dari `normalizeHmdError`, mengisi `error` dan `errorStatus` pada state Redux |
| DoD: Service, Redux slice, dan hook selesai 100%, lulus pengujian unit dengan cakupan >80%. | Terpenuhi | 7 services, 3 slices, 1 hook, 1 constants selesai 100%, terdaftar di `store.jsx`, lulus uji 10/10 test unit (100% pass) dan `next build` lolos tanpa error |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Seluruh aksi pada ruang kerja sesi pasien wajib memanggil `resetSessionState` saat unmount atau saat terjadi pergantian pasien untuk mencegah risiko *stale state*. |
| Rekomendasi | Layar berikutnya (`FE-HMD-03` Master Data Mesin dan `FE-HMD-04` Master Station) dapat langsung memanfaatkan thunk dari `hemodialysisMasterSlice` dan `useHemodialysisStatusTransition`. |
| Ketergantungan Terbuka | Selesainya `FE-HMD-02` secara resmi membuka prasyarat bagi task Gelombang `MVP-1`: `FE-HMD-03` (Master Mesin HD) dan `FE-HMD-04` (Master Station & Checklist Items). |
