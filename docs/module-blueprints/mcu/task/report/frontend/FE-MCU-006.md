# Laporan Perubahan Frontend — `FE-MCU-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-006` |
| Judul | Lapisan data order dan check-in |
| Slice | `MVP-1` (pasangan `BE-MCU-010`, `011`, `013`, `014`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-006`* |
| Trace | `FR-MCU-010`..`025`; `03-frontend-architecture.md` §4 (*Data basi*, *Kiriman ganda*) |
| Contract version | `MCU-API-v1` grup `Mcu Order` (tanpa `POST /recommendations` dan `PATCH /{id}/arrival-confirmation`) dan `Mcu Check In` — `approved` 2026-10-21. Ke-13 endpoint cocok dengan `McuOrderController` dan `McuCheckInController` (route, method, nama ruas DTO) |
| Wewenang UI | `NOT APPLICABLE` — tanpa tampilan |
| Dependency | Kontrak API ✅; backend `BE-MCU-010`, `011`, `013`, `014` sudah melayani endpoint |
| Klasifikasi | `MEDIUM` — satu repository, berkas diperiksa 9–20, berkas diubah 4–8, logika sedang (penjaga versi + muat ulang basi), kontrak yang sudah ada, tanpa database, keamanan berkaitan (pesan `403`, `version`), tanpa UI |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, service, slice `mcuOrder`, aturan permintaan bersama, registrasi store, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`, sesudah merge `QuilvianIntegrationFrontend`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-27 |
| Status | Selesai — kriteria terbukti uji unit; lint dan build lulus |

---

## 1. Keadaan yang ditemukan di awal

- Frontend belum punya lapisan data order MCU. Aturan status → aksi (`FE-MCU-001`) dan pembaca galat bersama
  (`mcu-request-rules.js`, `FE-MCU-002`) sudah ada.
- Backend menolak `version` basi dengan `409` kode **`VAL-MCU-46`** *"Data order sudah diubah petugas lain. Muat
  ulang lalu coba lagi."* (`McuBookingService`/`McuCheckInService.VersionConflict`), bukan `MCU-CONCURRENCY`; yang
  terakhir dipakai untuk bentrok baris jadwal.
- `409` lain bukan data basi, misalnya kuota penuh (`VAL-MCU-11`) dan kuota baru terpakai petugas lain
  (`VAL-MCU-12`).
- `McuOrderPagedQuery`: `date`, `status`, `source`, `companyGuarantorId`, `mcuDoctorId`, `search`, `sortBy`,
  `sortDirection`, `pageNumber`, `pageSize`. `status`/`source` berupa angka enum.
- `POST patients` Patient Management sudah tersedia sebagai thunk `createPatient`
  (`master-data-patient-slice.jsx`); kontrak menetapkan MCU tidak membuat pasien sendiri.

---

## 2. Proses bisnis dari sisi pengguna

Tanpa layar. Lapisan ini dipakai Daftar Pasien MCU (`FE-MCU-007`), Booking/Jadwal Ulang/Batal (`FE-MCU-008`), dan
Check-in/*Walk-in* (`FE-MCU-009`):

- **Memuat** — metadata penyaring, ringkasan per status untuk satu tanggal, daftar berhalaman, detail, riwayat,
  calon pasien yang cocok (NIK + tanggal lahir), dan pratinjau check-in (persyaratan, harga, dokter jaga bawaan).
  Masing-masing punya penanda muat dan galat sendiri.
- **Membuat** — booking dan *walk-in* (belum ada versi).
- **Mengubah** — jadwal ulang, batal, check-in, dan ganti dokter MCU. **Setiap aksi wajib membawa `version`**;
  tanpa versi yang sah, permintaan tidak dikirim dan layar menerima *"Versi order tidak diketahui. Muat ulang order
  lalu coba lagi."*
- **Data basi** — bila petugas lain lebih dulu mengubah order, server menjawab `409` `VAL-MCU-46`. Lapisan ini
  memuat ulang detail order (dan pratinjau check-in pada check-in) **sebelum** galat sampai ke layar, sehingga layar
  sudah memegang versi terbaru. Pesan server tampil apa adanya, dan `actionStale` menandai kejadian ini. Contoh:
  admisi A dan B membuka order Siti (versi 5); A menjadwal ulang (versi 6); B menekan Batal → `409`, detail dimuat
  ulang ke versi 6, B membaca pesan server lalu dapat mengulang dengan data terbaru.
- **Berhasil** — detail diganti jawaban server, dan baris daftar order yang sama ikut diperbarui (status, versi,
  dokter, *n dari m*) tanpa memuat ulang daftar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/medical-checkup-management/medical-checkup-constants.js` | `MCU_API.orders`, `MCU_ORDER_DEFAULT_FILTERS`, `MCU_STALE_ORDER_CODES` (`VAL-MCU-46`, `MCU-CONCURRENCY`) |
| `src/lib/services/health-services/medical-checkup-management/mcu-order.service.js` (baru) | 13 fungsi lewat `InstanceAxios`, satu per endpoint; query kosong dibuang; id di-`encodeURIComponent` |
| `src/utils/health-services/medical-checkup-management/mcu-request-rules.js` | `isMcuStaleOrderFailure`, `isMcuOrderVersion` |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-order-slice.jsx` (baru) | 13 thunk; penanda muat/galat per sumber; `runVersionedAction` (penjaga versi + muat ulang basi); gabung baris daftar dari detail |
| `src/lib/state/store.jsx` | Registrasi `mcuOrder` |
| `tests/unit/mcu-order-slices.test.mjs` (baru) | 6 uji `node:test` |

### 3.2 Kepatuhan arsitektur frontend

- Alur `slice → service → InstanceAxios`, sama dengan slice MCU master. Alamat hanya di konstanta domain.
- Pembantu `cleanParams`/`get`/`post`/`patch` ditulis ulang di service baru, sama bentuknya dengan
  `mcu-master-data.service.js`, satu service per kelompok kontrak seperti modul lain. Penyatuan pembantu itu adalah
  refactor di luar task.
- `POST patients` tidak ditulis ulang; layar `FE-MCU-009` memakai thunk `createPatient` yang sudah ada.
- Tidak ada dependency, abstraksi HTTP, atau pola state baru.

---

## 4. State yang ditangani

| State | Yang tersedia untuk layar |
| --- | --- |
| Memuat | `filterMetadataLoading`, `summaryLoading`, `listLoading`, `detailLoading`, `historyLoading`, `patientMatchesLoading`, `checkInPreviewLoading`, `actionLoading` |
| Kosong | Larik kosong **dan** `listLoadedAt` terisi |
| Gagal | `*Error` = kalimat server apa adanya; `*ErrorStatus` = kode HTTP; tindakan juga `actionErrorCode` = `VAL-MCU-xx` |
| Data basi | `actionStale: true` + detail/pratinjau sudah dimuat ulang |
| Tanpa hak akses | `403` dengan pesan server di ruas galat yang sama |
| Pembatalan | Permintaan yang dibatalkan (`meta.aborted`) tidak menulis galat |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management/orders`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata penyaring | `McuOrder : Read` |
| `GET` | `/summary` | Jumlah per status (`date`) | `McuOrder : Read` |
| `GET` | `/` | Daftar Pasien MCU | `McuOrder : Read` |
| `GET` | `/{id}` | Detail order | `McuOrder : Read` |
| `GET` | `/{id}/history` | Riwayat status dan dokter | `McuOrder : Read` |
| `POST` | `/bookings` | Booking | `McuOrder : Create` |
| `POST` | `/walk-ins` | *Walk-in* admisi | `McuOrder : Create` (+ `CheckIn`) |
| `PATCH` | `/{id}/reschedule` | Jadwal ulang (`version`) | `McuOrder : Update` |
| `PATCH` | `/{id}/cancel` | Batal (`version`) | `McuOrder : Cancel` |
| `GET` | `/{id}/patient-matches` | Calon pasien cocok | `McuOrder : CheckIn` |
| `GET` | `/{id}/check-in/preview` | Pratinjau check-in | `McuOrder : CheckIn` |
| `POST` | `/{id}/check-in` | Check-in (`version`) | `McuOrder : CheckIn` |
| `PATCH` | `/{id}/mcu-doctor` | Ganti dokter MCU (`version`) | `McuOrder : ReassignDoctor` |

Dipakai ulang: `POST /v1/health-services/patient-management/master-data/patients` lewat thunk `createPatient`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-order-slices.test.mjs` | 6/6 lulus | `PASS` | `# pass 6`, `# fail 0` |
| Kriteria: uji unit thunk | 13 thunk → 13 pasangan method + path sama persis dengan `MCU-API-v1`; query tanpa nilai kosong; tanpa argumen memakai penyaring state | `PASS` | Uji *setiap thunk…*, *query daftar…* |
| **Kriteria:** setiap aksi ubah mengirim `version` | Keempat aksi mengirim `version: 7` di body. `undefined`, `null`, `"7"`, `-1`, dan `1.5` ditolak tanpa satu pun permintaan; versi `0` sah | `PASS` | Uji *setiap aksi ubah…* |
| **Kriteria:** `409` diubah petugas lain → muat ulang detail | Untuk keempat aksi: urutan permintaan = aksi → `GET /{id}` (check-in juga `GET /check-in/preview`); detail dan baris daftar menjadi versi 9; galat = pesan server persis + `409` + `VAL-MCU-46` + `actionStale: true` | `PASS` | Uji *409 VAL-MCU-46…* |
| `409` lain / `400` / `403` | Tidak memuat ulang (satu permintaan saja); pesan server persis; `actionStale: false` | `PASS` | Uji *409 lain…* |
| Bentuk state | Check-in berhasil mengganti detail, memperbarui baris daftar yang sama (ruas lain seperti `maskedNik` tetap), dan mengosongkan pratinjau; pembatalan permintaan daftar tidak menulis galat | `PASS` | Uji *aksi berhasil…* |
| Uji mutasi | (a) muat ulang detail dihapus → uji *409* **gagal**; (b) penjaga versi dinonaktifkan → uji *version* **gagal**; dipulihkan identik (`cmp`), kembali 6/6 | `PASS` | Keluaran `node --test` |
| Seluruh uji MCU (6 berkas) | 46/46 lulus | `PASS` | `# pass 46` |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.875 uji: 3.833 lulus, 42 gagal — semuanya di luar MCU. Baseline bergeser karena merge `QuilvianIntegrationFrontend` (`f7f60e34c`). Dua nama gagal yang baru (`buildSubjectivePrefill…`, `PROMPT 08 Parity: InpatientResumeTab…`) **tetap gagal ketika perubahan task ini disimpan sementara** (`git stash` atas tiga berkas ter-track), jadi keduanya bukan akibat task ini | `EXISTING / ENVIRONMENT ISSUE` | Keluaran `node --test` dengan dan tanpa perubahan |
| `npm run build` | Exit 0: *Compiled successfully in 58s*, 523 halaman | `PASS` | Keluaran build |

`MANUAL TEST: NOT APPLICABLE` — tanpa kontrol interaktif; kontrol layar diuji di `FE-MCU-007`..`009`.
`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU sesudah merge f7f60e34c; 46/46 uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Uji unit thunk | Terpenuhi | §6 |
| Setiap aksi ubah mengirim `version` | Terpenuhi | §6 |
| `409` *diubah petugas lain* memicu muat ulang detail order | Terpenuhi | §6 |
| DoD: uji unit lulus | Terpenuhi untuk uji MCU; suite penuh membawa 42 kegagalan existing | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Muat ulang basi berjalan **sebelum** penolakan diteruskan, sehingga `detailLoading` sempat menyala sesaat ketika layar menerima galat; layar `FE-MCU-007`..`009` sebaiknya tidak mengunci seluruh halaman pada `detailLoading` sesudah muat awal |
| Masalah yang diketahui | Booking impor rombongan (`Mcu Booking Import`), rekomendasi, dan konfirmasi datang tidak termasuk lapisan ini, sesuai roadmap |
| Dependency backend | Tidak ada; endpoint sudah dilayani |
| Revisi 2026-10-27 (saat `FE-MCU-007`) | Komentar `MCU_ORDER_DEFAULT_FILTERS` semula menyebut *date kosong = hari ini*; yang benar, `GET orders` tanpa `date` mengembalikan **semua tanggal** (hanya `summary` memakai hari ini). Komentar diperbaiki; perilaku lapisan data tidak berubah, dan layar Daftar Pasien MCU selalu mengirim tanggal |
| Perubahan sampingan | `NONE`. Penyimpanan sementara `git stash` untuk pembuktian baseline dikembalikan (`stash pop`); diff ketiga berkas sama seperti sebelum disimpan (40 baris tambahan) dan 46/46 uji MCU lulus sesudahnya |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M medical-checkup-constants.js`, ` M store.jsx`, ` M mcu-request-rules.js`; `??` `mcu-order.service.js`, `mcu-order-slice.jsx`, `tests/unit/mcu-order-slices.test.mjs` |
| Langkah berikutnya | `FE-MCU-007` (Daftar Pasien MCU) — kini siap |
