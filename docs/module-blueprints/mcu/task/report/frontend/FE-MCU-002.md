# Laporan Perubahan Frontend — `FE-MCU-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-002` |
| Judul | Lapisan data master |
| Slice | `MVP-0` (pasangan `BE-MCU-005`, `006`, `007` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-002`* |
| Trace | `FR-MCU-002`..`006`; `03-frontend-architecture.md` §4 (pesan server apa adanya) |
| Contract version | `MCU-API-v1` grup `Mcu Package`, `Mcu Reference Data`, `Mcu Daily Schedule` — `approved` 2026-10-21; 19 endpoint cocok dengan controller backend `McuPackageController`, `McuReferenceDataController`, `McuScheduleController` |
| Wewenang UI | `NOT APPLICABLE` — tanpa tampilan |
| Dependency | Kontrak API ✅; backend `BE-MCU-005`..`007` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang sudah ada (1), tanpa database (0), keamanan berkaitan — pesan `403` (1), UI kecil (0) |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, service, tiga slice MCU, utilitas permintaan, registrasi store, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d0888bf6f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-23 |
| Status | Selesai — kriteria terbukti uji unit; lint dan build lulus |

---

## 1. Keadaan yang ditemukan di awal

- Frontend belum punya lapisan data MCU (hanya aturan bersama `FE-MCU-001`).
- Backend melayani 19 endpoint ketiga grup, persis `MCU-API-v1` (route, method, nama query). Jawaban dibungkus
  `ApiResponse` (`success`, `statusCode`, `message`, `data`, `errors.code`); daftar berhalaman memakai `PagedResult`
  (`items`, `pageNumber`, `pageSize`, `totalData`, `totalPage`).
- Pola rujukan: service Radiologi (`rad-order.service.js`: `cleanParams`, memulangkan `response.data`) dan slice
  Radiologi (`rad-order-slice.jsx`: penanda muat per sumber, pesan server apa adanya, `listLoadedAt`).
- `InstanceAxios` meneruskan galat apa adanya (`Promise.reject(error)`).

---

## 2. Proses bisnis dari sisi pengguna

Tanpa layar. Lapisan ini dipakai layar paket (`FE-MCU-003`), jenis pemeriksaan + kategori kelayakan (`FE-MCU-004`),
dan jadwal + kuota (`FE-MCU-005`):

- **Memuat** — daftar paket/jenis pemeriksaan berhalaman, pilihan paket (dengan harga perusahaan yang berlaku),
  detail, kategori kelayakan, dan jadwal per rentang tanggal, masing-masing dengan penanda muat sendiri.
- **Menyimpan** — buat/ubah paket, aktif/nonaktif, tambah/akhiri harga perusahaan; buat/ubah jenis pemeriksaan dan
  kategori; atur rentang jadwal; setujui pelampauan kuota.
- **Gagal** — kalimat server ditampilkan apa adanya, mis. *"Total nilai pengembalian Rp 400.000 melebihi harga paket
  Rp 350.000."* (`400`) atau *"Kuota MCU tanggal 27-10-2026 sudah penuh (30/30)."* (`409`), beserta kode status dan
  kode validasi `VAL-MCU-xx` agar layar membedakan bentrok dari kegagalan biasa. Tanpa kirim ulang otomatis.
- **Kosong vs gangguan** — `listLoadedAt` hanya terisi bila permintaan berhasil, sehingga daftar kosong karena gagal
  muat dapat dibedakan dari daftar yang memang kosong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/frontend-roadmap.md` (`FE-MCU-002`), `contracts/api-contract.md` (tiga grup), backend `McuPackageController`,
`McuReferenceDataController`, `McuScheduleController`, DTO query (`McuPackagePagedQuery`, `McuPackageOptionsQuery`,
`McuExaminationTypePagedQuery`, `McuFitnessCategoryQuery`, `McuDailyScheduleQuery`), `McuDailyScheduleResponse`,
`Responses/PagedResult.cs`; frontend `AGENTS.md`, `rules/frontend/frontend-architecture.md`, `test-policy.md`,
`radiology-constants.jsx`, `rad-order.service.js`, `rad-order-slice.jsx`, `InstanceAxios.jsx` (interceptor),
`store.jsx`, uji `crm-membership-list.test.mjs` (pola tukar `InstanceAxios`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/medical-checkup-management/medical-checkup-constants.js` (baru) | `MCU_API` (empat alamat dasar), ukuran halaman bawaan, penyaring bawaan paket dan jenis pemeriksaan |
| `src/lib/services/health-services/medical-checkup-management/mcu-master-data.service.js` (baru) | 19 fungsi lewat `InstanceAxios`, satu per endpoint; query kosong dibuang, `false`/`0` dipertahankan; id dan tanggal di-`encodeURIComponent` |
| `src/utils/health-services/medical-checkup-management/mcu-request-rules.js` (baru) | `readMcuServerFailure` (`{ message, statusCode, code }`), `unwrapMcuData`, `toMcuPagedResult`, `toMcuArray` — dipakai semua slice MCU |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-package-slice.jsx` (baru) | 9 thunk grup `Mcu Package`; penyaring, daftar + halaman, opsi, detail, satu penanda tindakan |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-reference-data-slice.jsx` (baru) | 7 thunk grup `Mcu Reference Data` |
| `src/lib/state/slice/health-services/medical-checkup-management/mcu-schedule-slice.jsx` (baru) | 3 thunk grup `Mcu Daily Schedule`; hasil atur rentang dan pelampauan digabung per `scheduleDate` |
| `src/lib/state/store.jsx` | Registrasi `mcuPackage`, `mcuReferenceData`, `mcuSchedule` |
| `tests/unit/mcu-master-data-slices.test.mjs` (baru) | 6 uji `node:test` |

### 3.3 Kepatuhan arsitektur frontend

- Alur `slice → service → InstanceAxios`, sama dengan Radiologi; tidak ada pemanggilan Axios dari view atau page.
- Alamat endpoint hanya di `medical-checkup-constants.js` (`frontend-architecture.md`: endpoint dari constants domain).
- Pembaca jawaban server di `utils` (fungsi murni) agar dipakai ulang slice order dan pemeriksaan, tidak disalin per slice.
- Tidak ada dependency, abstraksi HTTP, atau pola state baru.

---

## 4. State yang ditangani di layar

| State | Yang tersedia untuk layar |
| --- | --- |
| Memuat | `*Loading` per sumber (`listLoading`, `optionsLoading`, `detailLoading`, `schedulesLoading`, `actionLoading`, …) |
| Kosong | Larik kosong **dan** `*LoadedAt` terisi |
| Gagal | `*Error` = kalimat server apa adanya; `*ErrorStatus` = kode HTTP; tindakan juga `actionErrorCode` = `VAL-MCU-xx` |
| Tanpa hak akses | `403` dengan pesan server di ruas galat yang sama |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management`

#### Health Services / Medical Checkup Management / Mcu Package

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/packages/filters/metadata` | Metadata penyaring | `McuPackage : Read` |
| `GET` | `/packages` | Daftar paket berhalaman | `McuPackage : Read` |
| `GET` | `/packages/options` | Pilihan paket aktif + harga berlaku | `McuPackage : Read` |
| `GET` | `/packages/{id}` | Detail paket | `McuPackage : Read` |
| `POST` | `/packages` | Buat paket | `McuPackage : Create` |
| `PUT` | `/packages/{id}` | Ubah paket | `McuPackage : Update` |
| `PATCH` | `/packages/{id}/status` | Aktif/nonaktif | `McuPackage : Update` |
| `POST` | `/packages/{id}/company-prices` | Tambah harga perusahaan | `McuPackage : Update` |
| `PATCH` | `/packages/{id}/company-prices/{priceId}/end` | Akhiri harga perusahaan | `McuPackage : Update` |

#### Health Services / Medical Checkup Management / Mcu Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/examination-types` | Daftar jenis Pemeriksaan MCU | `McuReferenceData : Read` |
| `GET` | `/examination-types/{id}` | Detail jenis + isian | `McuReferenceData : Read` |
| `POST` | `/examination-types` | Buat jenis | `McuReferenceData : Create` |
| `PUT` | `/examination-types/{id}` | Ubah jenis | `McuReferenceData : Update` |
| `GET` | `/fitness-categories` | Daftar kategori kelayakan | `McuReferenceData : Read` |
| `POST` | `/fitness-categories` | Buat kategori | `McuReferenceData : Create` |
| `PUT` | `/fitness-categories/{id}` | Ubah kategori | `McuReferenceData : Update` |

#### Health Services / Medical Checkup Management / Mcu Daily Schedule

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/daily-schedules` | Jadwal per rentang (`from`, `to`) | `McuSchedule : Read` |
| `PUT` | `/daily-schedules` | Atur rentang hari buka/kuota/dokter jaga | `McuSchedule : Update` |
| `POST` | `/daily-schedules/{scheduleDate}/quota-overrides` | Setujui pelampauan kuota | `McuSchedule : ApproveQuotaOverride` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/mcu-master-data-slices.test.mjs` | 6 lulus | `PASS` | `# pass 6`, `# fail 0` |
| Kriteria: setiap aksi memanggil path dan method persis kontrak | 19 thunk → 19 pasangan method + path sama persis dengan `MCU-API-v1` | `PASS` | Uji `setiap thunk…` |
| Kriteria: pesan `400`/`403`/`409` server diteruskan apa adanya | `rejected` dengan `{ message, statusCode, code }` persis; state `actionError`/`Status`/`Code` sama | `PASS` | Uji `pesan 400/403/409…` |
| Query dan body | Query kosong dibuang, `isActive=false` dipertahankan; tanpa argumen memakai penyaring state; body diteruskan utuh | `PASS` | Dua uji query |
| Bentuk state | Halaman tersimpan; `listLoadedAt` hanya saat berhasil; harga perusahaan tidak menimpa detail paket; pelampauan menimpa baris tanggalnya | `PASS` | Uji `daftar berhalaman…`, `galat kuota…` |
| Uji mutasi | Path pelampauan diubah dan pesan server diabaikan → 4 uji **gagal**; dipulihkan identik (`cmp`), kembali 6/6 | `PASS` | Keluaran `node --test` |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.607 uji: 3.561 lulus, 46 gagal — daftar nama kegagalan **identik** dengan baseline `FE-MCU-001` (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar `not ok` kosong |
| `npm run build` | Exit 0: *Compiled successfully in 58s*, 517/517 halaman statis; tidak ada berkas tersentuh di luar berkas task | `PASS` | Keluaran build |

`MANUAL TEST: NOT APPLICABLE` — tanpa kontrol interaktif; kontrol layar diuji di `FE-MCU-003`..`005`.
`AUTOMATED TEST: npm run test:unit — FAIL (46 kegagalan existing di luar MCU, identik dengan baseline; 18/18 uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Uji unit thunk: setiap aksi memanggil path dan method persis kontrak | Terpenuhi | §6 |
| Pesan `400`/`403`/`409` server diteruskan apa adanya ke state galat | Terpenuhi | §6 |
| DoD: uji unit lulus | Terpenuhi untuk uji MCU; suite penuh membawa 46 kegagalan existing | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Galat tanpa jawaban server (jaringan putus) menyimpan pesan galat jaringan (`Network Error`), sama dengan pola Radiologi; layar dapat menggantinya dengan kalimat sendiri bila `*ErrorStatus` kosong |
| Masalah yang diketahui | Bentuk payload request (`CreateMcuPackageRequest`, dll.) diteruskan apa adanya — penyusunannya milik form layar `FE-MCU-003`..`005` |
| Dependency backend | Tidak ada; `BE-MCU-005`..`007` sudah selesai |
| Perubahan sampingan | `NONE` — dua percobaan skrip mutasi gagal mencocokkan penanda sebelum menulis apa pun; berkas diperiksa identik |
| Interupsi | `NONE` |
| Status Git (frontend) | ` M src/lib/state/store.jsx`; `??` folder konstanta, service, slice, dan utilitas MCU; dua berkas uji MCU (`FE-MCU-001`, `002`) |
| Langkah berikutnya | `FE-MCU-003` (layar kelola paket) — kini siap; atau lapisan data `FE-MCU-006`/`010` |
