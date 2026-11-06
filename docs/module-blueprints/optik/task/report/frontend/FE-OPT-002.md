# Laporan Perubahan Frontend — `FE-OPT-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-OPT-002` |
| Judul | Lapisan data klinis: service Axios + slice Redux untuk kunjungan, pemeriksaan, resep, metadata |
| Slice | Lapisan data (paralel dengan backend), prasyarat `FE-OPT-010`, `011`, `012` |
| Roadmap | `roadmap/frontend-roadmap.md` (`OPT-RM-FE-001` r2) — Rincian `FE-OPT-002` / `FE-OPT-003` |
| Trace | `03-frontend-architecture.md` bagian 4 (penanganan state); `OPT-FE-005`; `NFR-001`..`007`; screen brief `05-screen-brief.md` (approved 2026-11-04) bagian 5.1–5.6 |
| Contract version | `OPT-API-v1` grup Opt Visit, Opt Refraction Exam, Opt Prescription, dan `GET reference-data/metadata` — `approved`, terkunci hash |
| Wewenang UI | `NOT APPLICABLE` — task tanpa layar; tidak ada menu, route, atau komponen |
| Dependency | `BE-OPT-005` ✅, `010` ✅, `011` ✅, `012` ✅ (endpoint sudah ada di backend) |
| Klasifikasi | `MEDIUM` — 1 berkas konstanta, 1 utilitas, 1 service, 4 slice, registrasi store, 1 berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: `src/lib/{constants,services,state}/…/optical-management/`, `src/utils/health-services/optical-management/`, `src/lib/state/store.jsx`, `tests/unit/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `2bb756de7` (branch `YogaV2`, upstream `origin/YogaV2`) |
| Commit backend yang dijadikan rujukan | `37ae965c` (branch `yoga`) + `BE-OPT-054` di working tree |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi (uji unit, lint, build). Belum di-commit |

---

## 1. Keadaan yang ditemukan di awal

Frontend v2 belum punya satu pun pemanggil API Optik. Yang sudah ada hanya aturan isian resep dari `FE-OPT-001`
(`src/utils/health-services/optical-management/optical-prescription-rules.js`). Tidak ada konstanta endpoint, service,
maupun slice Redux Optik, sehingga layar `FE-OPT-010`..`012` belum bisa dibangun.

Fakta dari backend yang menentukan bentuk lapisan ini:

| Fakta | Bukti |
| --- | --- |
| Hanya `POST visits` yang membaca header `Idempotency-Key` | `OptVisitController.Register` (`[FromHeader(Name = "Idempotency-Key")]`) |
| Mulai pemeriksaan memulangkan draf yang sudah ada pada kunjungan yang sama | `OptRefractionExamService.StartAsync` |
| Aksi ubah pemeriksaan dan revisi resep menolak bila `ExpectedVersion` kosong atau berbeda (`409` `OPT-CONCURRENCY`) | `RejectStale` pada kedua service |
| Bentuk galat: `{ statusCode, message, errors: { code } }` untuk `400`/`403`/`404`/`409`/`503` | `OptHttp.Map` |

---

## 2. Proses bisnis dari sisi pengguna

Task ini tidak punya layar. Perannya: memastikan layar Optik nanti berperilaku benar saat bicara dengan server.

1. **Admisi mendaftarkan kunjungan.** Ketika tombol Daftarkan ditekan dua kali dengan cepat, hanya satu permintaan
   terkirim. Bila jaringan putus atau server menjawab `503`, menekan Daftarkan lagi dengan isian yang sama memakai
   kunci yang sama. Server lalu memulangkan kunjungan yang sudah tercipta (bila ada), bukan membuat kunjungan kedua.
   **Contoh:** Ibu Sari didaftarkan, jaringan putus, petugas menekan lagi → tetap satu kunjungan ENC-RSMMC-00101.
   Bila petugas mengubah cara bayar sebelum menekan lagi, itu niat baru dan memakai kunci baru.
2. **RO mengisi dan memfinalkan pemeriksaan, atau merevisi resep.** Setiap aksi membawa versi data yang terakhir
   dilihat. Bila petugas lain sudah mengubahnya lebih dulu, server menjawab *"Pemeriksaan sudah diubah petugas lain.
   Muat ulang halaman."*. Lapisan ini langsung memuat ulang detail, sehingga layar menampilkan data terbaru beserta
   pesan itu.
3. **Penolakan lain diteruskan apa adanya.** Contoh: `403` *"STR Anda tidak berlaku sejak 31-10-2026."*, `400`
   *"SPH di luar rentang …"*, `409` resep terkunci, dan `503` modul pendukung tidak terjangkau. Kalimat server tidak
   diganti; kode status dan kodenya ikut disimpan.
4. **Label status** (misalnya *Draf*, *Final*) dibaca dari metadata server sekali per sesi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `test-policy.md`, `REPORT_TEMPLATE.md`; pola MCU
(`mcu-order.service.js`, `mcu-order-slice.jsx`, `mcu-request-rules.js`, `medical-checkup-constants.js`,
`tests/unit/mcu-order-slices.test.mjs`); `src/lib/state/store.jsx`; pola kunci idempoten Billing
(`use-billing-deposit.js`). Backend (baca-saja): `OptVisitController`, `OptRefractionExamController`,
`OptPrescriptionController`, `OptReferenceDataController`, `OptHttp`, DTO terkait, `OptOperationResult`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/optical-management/optical-constants.js` | **Baru.** `OPTICAL_API`, penyaring bawaan Daftar Kunjungan, `OPTICAL_STALE_CODES`, pesan versi kosong, nama header idempoten |
| `src/utils/health-services/optical-management/optical-request-rules.js` | **Baru.** Pembaca galat server, penentu "basi" dan "hasil tidak diketahui", pembuat kunci idempoten, pembuka `ApiResponse`/`PagedResult` |
| `src/lib/services/health-services/optical-management/optical-clinical.service.js` | **Baru.** 18 fungsi endpoint (kunjungan 3, pemeriksaan 7, resep 7, metadata 1) |
| `src/lib/state/slice/health-services/optical-management/optical-visit-slice.jsx` | **Baru.** Daftar, ringkasan, pendaftaran dengan satu kunci per niat |
| `src/lib/state/slice/health-services/optical-management/optical-refraction-exam-slice.jsx` | **Baru.** Daftar, detail, mulai, isi, final, revisi, batal; `409` basi memuat ulang detail |
| `src/lib/state/slice/health-services/optical-management/optical-prescription-slice.jsx` | **Baru.** Daftar, pilihan resep berlaku, kelayakan melewati rentang, detail, buat, salin luar, revisi |
| `src/lib/state/slice/health-services/optical-management/optical-reference-data-slice.jsx` | **Baru.** Metadata sekali muat + `findOpticalLabel` |
| `src/lib/state/store.jsx` | **Diperbarui.** 4 reducer: `opticalVisit`, `opticalRefractionExam`, `opticalPrescription`, `opticalReferenceData` (9 baris, CRLF dipertahankan) |
| `tests/unit/optical-clinical-slices.test.mjs` | **Baru.** 13 uji dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Alur `slice → service → InstanceAxios` mengikuti MCU persis. Tidak ada Axios instance baru, semua endpoint ditulis di
satu berkas konstanta, dan thunk meneruskan `signal`. Utilitas murni berada di `utils`, tanpa React dan tanpa store.

**Pola yang ditambahkan, beserta alasannya:**

- *Kunci idempoten per niat yang diikat ke sidik isi permintaan.* Backend Optik memakai kunci untuk memulangkan
  kunjungan yang sama, sehingga kunci tidak boleh terpakai untuk isi yang berbeda. Pola Billing hanya membuat kunci
  per tindakan di hook. Di sini kunci disimpan di slice agar bertahan saat layar dirender ulang.
- *`condition` pada thunk aksi.* Klik kedua selama kiriman berjalan tidak mengirim apa pun.

Pembuat UUID memakai bentuk yang sama dengan hook Billing; di repository belum ada helper bersamanya.

---

## 4. State yang ditangani di layar

| State | Yang disediakan lapisan data untuk layar |
| --- | --- |
| Memuat | `listLoading`, `detailLoading`, `actionLoading`, `registerLoading`, `metadataLoading` |
| Kosong | Daftar selalu larik (`[]`) dan paginasi berbentuk tetap, sehingga layar dapat menampilkan kalimat kosong |
| Gagal | Pesan server apa adanya + `…ErrorStatus` + `…ErrorCode`; cadangan bila tidak ada jawaban server (mis. *"Daftar kunjungan Optik gagal dimuat."*) |
| Tanpa hak akses | `403` tersimpan dengan status `403` dan pesannya; kelayakan melewati rentang `403` dibaca *tidak berhak* (`canOverrideRange: false`) |
| Data basi | `actionStale: true` sesudah detail dimuat ulang |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Optical Management / Opt Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/visits` | Daftar kunjungan (tanggal, terbuka, ada pesanan terbuka, cari) | `OptVisit : Read` |
| `GET` | `/v1/health-services/optical-management/visits/{encounterId}` | Ringkasan kunjungan | `OptVisit : Read` |
| `POST` | `/v1/health-services/optical-management/visits` | Daftarkan kunjungan; header `Idempotency-Key` | `OptVisit : Create` |

#### Health Services / Optical Management / Opt Refraction Exam

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/refraction-exams` | Pemeriksaan per kunjungan/pasien | `OptRefractionExam : Read` |
| `GET` | `/v1/health-services/optical-management/refraction-exams/{id}` | Detail + revisi | `OptRefractionExam : Read` |
| `POST` | `/v1/health-services/optical-management/refraction-exams` | Mulai pemeriksaan | `OptRefractionExam : Create` |
| `PUT` | `/v1/health-services/optical-management/refraction-exams/{id}/measurements` | Isi hasil ukur (draf) | `OptRefractionExam : Update` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/finalize` | Simpan final | `OptRefractionExam : Finalize` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/revisions` | Revisi sesudah final | `OptRefractionExam : Revise` |
| `POST` | `/v1/health-services/optical-management/refraction-exams/{id}/cancel` | Batalkan draf | `OptRefractionExam : Cancel` |

#### Health Services / Optical Management / Opt Prescription

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/prescriptions` | Resep pasien/kunjungan | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/options` | Resep berlaku untuk pesanan | `OptPrescription : Read` |
| `GET` | `/v1/health-services/optical-management/prescriptions/range-override` | Boleh melewati rentang (Sp.M) | `OptPrescription : OverrideRange` |
| `GET` | `/v1/health-services/optical-management/prescriptions/{id}` | Detail + versi | `OptPrescription : Read` |
| `POST` | `/v1/health-services/optical-management/prescriptions` | Buat resep dari pemeriksaan | `OptPrescription : Create` |
| `POST` | `/v1/health-services/optical-management/prescriptions/external` | Salin resep luar | `OptPrescription : Create` |
| `POST` | `/v1/health-services/optical-management/prescriptions/{id}/revisions` | Revisi resep | `OptPrescription : Revise` |

#### Health Services / Optical Management / Opt Reference Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/optical-management/reference-data/metadata` | Label status dan enum | `OptReferenceData : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/optical-clinical-slices.test.mjs` | 13 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Method + path 18 endpoint | Sama persis dengan controller backend | `PASS` | Uji 1 |
| Klik ganda pendaftaran dan simpan final | Satu permintaan; klik kedua dilewati (`condition`) | `PASS` | Uji 3, 6 |
| Kunci idempoten | Jaringan putus/`503` → kunci sama (termasuk urutan isian berbeda); sesudah berhasil, ditolak `400`, atau isi berubah → kunci baru; tutup formulir membuang niat | `PASS` | Uji 4 |
| `expectedVersion` | Dikirim di 5 aksi; tanpa versi ditolak sebelum mengirim | `PASS` | Uji 5 |
| `409` basi memuat ulang | `OPT-CONCURRENCY` → `GET` detail, versi terbaru di state, `actionStale`; `409` resep terkunci tidak memuat ulang | `PASS` | Uji 7, 8 |
| Galat `400`/`403`/`503` | Pesan, status, dan kode server tersimpan apa adanya | `PASS` | Uji 9, 10 |
| Uji mutasi: muat ulang `409` dihapus / kunci selalu baru | Uji 7 dan uji 4 gagal; berkas dipulihkan (`cmp` identik) | `PASS` | Keluaran perintah |
| `npx eslint --quiet` atas seluruh berkas yang disentuh | 0 error | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 4.268 lulus, 72 gagal dari 4.344. Tidak ada kegagalan di berkas Optik, dan tidak ada berkas gagal yang mengimpor `store.jsx` | `EXISTING / ENVIRONMENT ISSUE` | Daftar berkas gagal: Billing, Rawat Inap, HRD/Finance, CRM, menu, Hemodialisa (baseline 2026-11-02: 55 dari 4.174) |
| `npm run build` (server lokal dimatikan lebih dulu) | `Compiled successfully in 73s`; 566 halaman statis; standalone siap; exit 0 | `PASS` | Keluaran build |

Uji manual: `NOT APPLICABLE` — task tanpa layar. Perilaku di layar diuji pada `FE-OPT-010`..`012`.

**Tidak dijalankan:** `npm run lint:errors` atas seluruh repository (ESLint dijalankan atas berkas yang disentuh); E2E
(belum ada layar); `npm run test:unit` (gagal di Node 20; perintah setara di atas yang dipakai).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Service Axios per grup `[Tags]` dan slice Redux | Terpenuhi | Bagian 3.2, 5 |
| Header `Idempotency-Key` per niat aksi | Terpenuhi untuk pendaftaran kunjungan, satu-satunya endpoint klinis yang membacanya. Aksi lain dijaga `expectedVersion`, `condition` klik ganda, dan backend (mulai pemeriksaan memulangkan draf yang ada) | Uji 3, 4, 6 |
| `ExpectedVersion` dikirim | Terpenuhi | Uji 5 |
| Galat `400/403/409/503` dipetakan ke pesan backend | Terpenuhi | Uji 7–10 |
| Klik ganda memakai kunci yang sama | Terpenuhi | Uji 3, 4 |
| `409` memicu muat ulang | Terpenuhi | Uji 7, 8 |
| DoD: uji unit lulus | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta: `03-frontend-architecture.md` §4 menyebut satu `Idempotency-Key` per niat untuk setiap aksi, tetapi backend klinis hanya membacanya di `POST visits`. Membuat resep dan salin resep luar tidak punya kunci di backend, sehingga hanya dijaga dari klik ganda di layar. Bila jaringan putus sesudah server menyimpan, kiriman ulang dapat membuat resep kedua. Diteruskan sebagai catatan backend, tanpa mengubah backend |
| Masalah yang diketahui | 72 uji unit milik modul lain gagal (bagian 6); tidak diperbaiki karena di luar scope |
| Dependency backend | `NONE` — seluruh endpoint sudah tersedia |
| Perubahan sampingan | `NONE`. Dua mutasi uji dibuat dan dipulihkan |
| Interupsi | `NONE` |
| Status Git | `M src/lib/state/store.jsx`; `??` 7 berkas baru di bagian 3.2 (folder Optik baru) dan `tests/unit/optical-clinical-slices.test.mjs` |
| Langkah berikutnya | `FE-OPT-003` (lapisan data pesanan, termasuk `OPT-API-v1.1`), lalu `FE-OPT-010` |
