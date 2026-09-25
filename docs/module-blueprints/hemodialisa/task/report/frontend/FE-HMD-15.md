# Laporan Perubahan Frontend — `FE-HMD-15`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-15` |
| Judul | Ruang Kerja Sesi HD — Eksekusi Mulai Sesi Idempoten, Garis Waktu Pemantauan, Obat, dan Komplikasi (`FE-HMD-07` Intra-HD) |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Komplikasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.5 |
| Trace | `FR-HMD-054`, `FR-HMD-055`, `FR-HMD-060`, `FR-HMD-061`, `FR-HMD-063`, `FE-HMD-07`, `CAP-32`, `CAP-33`, `CAP-34`, `NFR-003` |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak tab dan form. **Pengecualian mengikat**: tombol Mulai wajib terkunci seketika sekali ditekan dan membawa penanda idempotensi |
| Keputusan UI Gate | **12 elemen**: `REUSE 12, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-14` (selesai 23 September 2026), `BE-HMD-13`, `BE-HMD-14`, `BE-HMD-15` (ketiganya selesai 22 September 2026) |
| Klasifikasi | `HEAVY` — skor 13: repository 0, berkas diperiksa 10, berkas dibuat 2, berkas diubah 6, logika 4, kontrak API 5, database 0, UI 4 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — ketiga acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 20 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. `FE-HMD-14` sudah memasang kerangka ruang kerja sesi beserta bagian Pra-HD; bagian Intra-HD belum ada.
2. Tiga celah pada lapisan service dan state ditemukan dengan membandingkan `hmdSessionService.js` terhadap controller backend: **tidak ada** pemanggil untuk `GET /{id}/medications`, `GET /{id}/complications`, maupun `POST /{id}/medications/{medicationId}/pharmacy-handoff/retry`. Tanpa ketiganya, obat dan komplikasi hanya dapat dicatat tetapi tidak pernah dapat ditampilkan kembali, dan penerusan ke Farmasi yang gagal tidak dapat dikirim ulang.
3. Slice sesi belum memiliki state untuk daftar obat maupun komplikasi.
4. Pola pembangkit UUID sudah mapan di modul Billing (`use-billing-deposit.js`) dengan cadangan manual untuk webview lama.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Perawat pelaksana, sepanjang empat sampai lima jam sesi berlangsung.

**Alur normal.**

1. Setelah sesi dinyatakan siap, perawat membuka bagian **Intra-HD & Pemantauan**.
2. Perawat menekan **Mulai Cuci Darah**. Tombol terkunci **seketika** — penguncian memakai penanda langsung, bukan menunggu React merender ulang, sehingga penekanan kedua pada milidetik yang sama tidak menembus. Permintaan membawa kunci idempotensi UUIDv4.
3. Sesi beralih ke `InProgress`, dan keterangan berubah menjadi "Sesi berjalan sejak 23 Sep 2026 07.12 menurut waktu server". Jam yang dipakai adalah jam server, bukan jam perangkat di ruang dialisis.
4. Perawat mencatat **pemantauan berkala**: tensi, nadi, suhu, saturasi, ditambah parameter mesin QB, QD, AP, VP, TMP, dan UF terkumpul, beserta catatan keluhan pasien.
5. Setiap pencatatan menjadi simpul baru pada garis waktu, urut kronologis, menampilkan jam, tensi, nadi, UF terkumpul, dan keluhan. Pemantauan jam ke-1, ke-2, dan ke-3 muncul sebagai tiga simpul berurutan.
6. Pada tab **Pemberian Obat**, perawat memilih obat dari katalog Farmasi, mengisi dosis dan satuan, memilih rute pemberian, dan bila perlu menunjuk dokter pemberi instruksi. Setiap baris obat menampilkan status penerusannya ke Farmasi.
7. Saat pasien mengalami kram, perawat membuka bagian **Komplikasi**, memilih "Kram Otot Ekstremitas", mengisi tanda gejala dan tindakan "Pemberian bolus dekstrosa dan pemijatan", memilih hasil penanganan dan pengaruhnya terhadap sesi. Komplikasi yang tersimpan muncul sebagai peringatan keselamatan di bawah form.

**Jalur tidak normal.**

- **Isian pemantauan belum disimpan lalu halaman ditinggalkan** — pita kuning muncul di atas form, dan peramban meminta konfirmasi sebelum meninggalkan atau memuat ulang halaman. Ini mitigasi risiko yang memang diminta roadmap.
- **Menyimpan pemantauan kosong** — ditolak di layar dengan "Isi setidaknya satu parameter pemantauan sebelum menyimpan."
- **Penerusan obat ke Farmasi gagal** — catatan klinisnya **tetap tersimpan**; barisnya menampilkan lencana merah "Gagal Sinkron ke Farmasi" beserta pesan kesalahan dan tombol **Kirim Ulang**.
- **Gagal memulai sesi** — kunci tombol dilepas supaya perawat dapat mencoba lagi, dan percobaan berikutnya memakai **kunci idempotensi yang sama** sehingga jawaban yang sempat hilang di jaringan tidak pernah melahirkan sesi kedua.
- **Tanpa hak akses** — perawat tanpa hak mencatat observasi, obat, atau komplikasi tetap dapat membaca riwayatnya, tetapi formnya tidak dirender.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend (read-only).** `Controllers/HmdSessionController.cs`, `HmdObservationController.cs`, `HmdMedicationController.cs`, `HmdComplicationController.cs`; `DTOs/HmdSessionDtos.cs` (`StartHmdSessionRequest`, `CreateHmdObservationRequest`, `HmdObservationResponse`, `CreateHmdMedicationRequest`, `HmdMedicationResponse`, `RetryHmdPharmacyHandoffRequest`, `CreateHmdComplicationRequest`, `HmdComplicationResponse`); `Enums/HemodialysisEnums.cs`.

**Frontend.** `clinical-workspace/ClinicalTimeline.jsx` dan `ClinicalTimelineItem.jsx`; `base-features/base-form-control.jsx`; `lib/hooks/select/use-select-resource.jsx`; `use-billing-deposit.js` sebagai rujukan pola idempotensi.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/services/.../hmdSessionService.js` | Menambah `getHmdMedications`, `getHmdComplications`, dan `retryHmdPharmacyHandoff` yang sebelumnya belum punya pemanggil |
| `src/lib/state/slice/.../hemodialysisSessionSlice.js` | Menambah thunk `fetchSessionMedications`, `fetchSessionComplications`, `retryPharmacyHandoff`; menambah state daftar obat dan komplikasi; hasil percobaan ulang Farmasi memperbarui satu baris obat di tempat, bukan memuat ulang seluruh daftar |
| `src/lib/constants/.../hemodialysis-session-constants.js` | Menambah konfigurasi sebelas field observasi beserta rentangnya, opsi rute obat, status penerusan Farmasi, opsi jenis komplikasi, derajat, hasil, dan pengaruh sesi |
| `src/utils/.../hemodialysis-session-display-utils.js` | Menambah `generateIdempotencyKey`, `buildObservationPayload`, `hasObservationInput`, `buildObservationTimelineItems`, `buildMedicationPayload`, `validateMedicationForm`, `buildComplicationPayload`, `validateComplicationForm`, `resolvePharmacyHandoffBadge` |
| `src/lib/hooks/.../use-hemodialysis-session-workspace.jsx` | Menambah state dan handler Intra-HD, penjaga penekanan ganda berbasis ref, kunci idempotensi yang dipakai ulang, penjaga `beforeunload`, dan pembacaan riwayat yang hanya berjalan saat bagiannya dibuka |
| `src/components/view/.../sessions/sections/intra-hd-section.jsx` | **Baru.** Tombol mulai, form dan garis waktu pemantauan, form dan daftar obat, form dan daftar komplikasi |
| `src/components/view/.../sessions/hemodialysis-session-workspace-view.jsx` | Menambah butir navigasi Intra-HD dan penyambungan section beserta katalog obat dan dokter |
| `src/style/.../hemodialysis-session-workspace.module.css` | Kelas daftar catatan dan slot field; seluruhnya `var(--token)` |
| `tests/unit/hemodialysis-session-intra-hd.test.mjs` | **Baru.** 20 unit test |

### 3.3 Kepatuhan Arsitektur Frontend

Alur dependensi sama dengan `FE-HMD-14`. `ClinicalTimeline` dan `ClinicalTimelineItem` dipakai apa adanya dari `@/components/ui/clinical-workspace`, sesuai larangan membuat komponen duplikat pada roadmap bagian 1.

**Catatan waktu.** Payload observasi sengaja **tidak** mengirim `observedAt`, sehingga backend memakai waktu servernya sendiri. Jam perangkat di ruang dialisis tidak dapat dipercaya untuk catatan rekam medis, dan pilihan ini diuji secara eksplisit.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat riwayat pemantauan...", "Memuat riwayat pemberian obat...", "Memuat riwayat komplikasi..." per daftar |
| Kosong | "Belum ada pemantauan pada sesi ini.", "Belum ada pemberian obat pada sesi ini.", "Belum ada komplikasi tercatat pada sesi ini." |
| Gagal | `InformationAlert` merah dari pesan server di atas bagian. Kegagalan penerusan Farmasi ditampilkan per baris obat beserta tombol kirim ulang, tanpa menyembunyikan catatan klinisnya |
| Tanpa hak akses | Form observasi, obat, dan komplikasi tidak dirender bagi yang tidak berhak; riwayatnya tetap terbaca. Tombol Mulai hanya muncul bila backend mengizinkan aksinya **dan** pengguna memegang `HemodialysisSession : Start` |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `.../hemodialysis-sessions/{id}/start` | Memulai cuci darah dengan kunci idempotensi | `HemodialysisSession : Start` |

#### Health Services / Hemodialysis Management / Hemodialysis Observation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-sessions/{id}/observations` | Garis waktu pemantauan | `HemodialysisObservation : Read` |
| `POST` | `.../hemodialysis-sessions/{id}/observations` | Mencatat satu baris pemantauan | `HemodialysisObservation : Create` |

#### Health Services / Hemodialysis Management / Hemodialysis Medication

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-sessions/{id}/medications` | Riwayat pemberian obat beserta status Farmasi | `HemodialysisMedication : Read` |
| `POST` | `.../hemodialysis-sessions/{id}/medications` | Mencatat pemberian obat | `HemodialysisMedication : Administer` |
| `POST` | `.../hemodialysis-sessions/{id}/medications/{medicationId}/pharmacy-handoff/retry` | Mengirim ulang pemakaian obat ke Farmasi | `HemodialysisMedication : Administer` |

#### Health Services / Hemodialysis Management / Hemodialysis Complication

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `.../hemodialysis-sessions/{id}/complications` | Riwayat komplikasi | `HemodialysisComplication : Read` |
| `POST` | `.../hemodialysis-sessions/{id}/complications` | Mencatat komplikasi | `HemodialysisComplication : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <berkas task ini>` | 0 error, 0 warning | `PASS` | exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-session-intra-hd.test.mjs` | 20 test, 20 lolos, 0 gagal | `PASS` | Berkas test baru |
| Grep anti-regresi konsistensi UI, 10 pemeriksaan | Seluruhnya kosong | `PASS` | Termasuk pemeriksaan tambahan bahwa tidak ada `<input>`, `<select>`, maupun `<textarea>` mentah di section ini |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Tidak ada server pengembangan dan sesi login berotorisasi |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

**Cacat kontrak komponen yang ditemukan dan diperbaiki sebelum task ditutup.** `ClinicalTimelineItem` menerima `timeFormatted`, `title`, dan `children`, bukan `time` dan `description`. Pemakaian pertama saya salah dan sudah dikoreksi.

Kontrol interaktif yang **belum** dibuktikan di peramban: penguncian tombol Mulai pada penekanan ganda, pengisian sebelas field pemantauan, pertumbuhan garis waktu, pemilihan obat dari katalog Farmasi, tombol kirim ulang Farmasi, form komplikasi, dan dialog konfirmasi peramban saat meninggalkan halaman dengan isian belum tersimpan.

Yang **sudah** dibuktikan di tingkat logika oleh 20 unit test: bentuk UUID v4 kunci idempotensi beserta keunikan lima puluh pembangkitan berturut-turut, pengurutan kronologis garis waktu, isi simpul garis waktu termasuk penolakan menampilkan tensi ketika hanya sistolik yang ada, bentuk payload observasi termasuk pembuktian bahwa `observedAt` tidak pernah dikirim dari jam perangkat, penjaga isian belum tersimpan, kelengkapan enam parameter mesin pada konfigurasi field, validasi obat termasuk penolakan dosis nol dan negatif, bentuk payload obat, ketiga tone penanda status Farmasi, validasi komplikasi lima field wajib, dan contoh kram otot dari roadmap yang lolos validasi lalu tersusun menjadi payload yang benar.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Perawat menekan Mulai; tombol terkunci seketika. Sesi beralih ke `InProgress`, jam server tampil sebagai waktu mulai | Terpenuhi | Penguncian memakai `startLockRef` yang berlaku sebelum render berikutnya, ditambah `disabled` dan `loading` pada `BaseButton`. Waktu mulai dibaca dari `StartedAt` milik server. Kunci idempotensi dipakai ulang pada percobaan setelah gagal |
| **AC-2** Perawat memasukkan observasi jam ke-1, ke-2, dan ke-3; garis waktu menampilkan 3 simpul kronologis dengan tren tensi dan akumulasi ultrafiltrasi | Terpenuhi | `buildObservationTimelineItems` mengurutkan menaik dan menyusun metrik TD, nadi, serta UF terkumpul per simpul; dirender `ClinicalTimeline` dan `ClinicalTimelineItem` |
| **AC-3** Saat pasien kram, perawat mengklik "Catat Komplikasi", memilih "Kram Otot Ekstremitas", mengisi intervensi, dan status komplikasi muncul dengan banner `ClinicalSafetyAlert` | Terpenuhi | Opsi jenis komplikasi memuat "Kram Otot Ekstremitas" bernilai `2`; komplikasi tersimpan dirender sebagai `ClinicalSafetyAlert` dengan tone mengikuti hasil penanganannya |

**Definition of Done** — "Alur mulai sesi idempoten, garis waktu observasi, pencatatan obat, dan komplikasi berfungsi sempurna di antarmuka": seluruh butirnya terpenuhi, kecuali component test `SessionIntraDialysisViewTests` yang **tidak dipenuhi dalam bentuk yang disebut roadmap** karena repository tidak memakai Jest maupun `@testing-library`; digantikan 20 unit test `node:test` menurut keputusan pemilik 1 September 2026. Mitigasi risiko roadmap berupa peringatan `beforeunload` atas form observasi yang belum disimpan **ikut diimplementasikan**.

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan `react-hooks/refs` sempat muncul karena inisialisasi kunci idempotensi saat render; **sudah dihilangkan** dengan memakai pola `useRef(null)` beserta pemeriksaan `== null` yang memang disarankan aturannya. Hasil akhir 0 error 0 warning |
| Masalah yang diketahui | Pemilihan lokasi stok dan satuan stok Farmasi pada form obat **belum** disediakan; backend menerimanya sebagai field opsional, dan tanpa keduanya penerusan dapat berstatus `Pending`. Tombol kirim ulang sudah tersedia untuk kasus itu. Daftar cacat modul di luar cakupan dari `FE-HMD-12`, `FE-HMD-13`, dan `FE-HMD-14` masih berlaku dan belum diperbaiki |
| Dependency backend | Tidak ada yang tertahan |
| Perubahan sampingan | Tiga fungsi service dan tiga thunk baru. Semuanya dibutuhkan task ini dan tidak mengubah perilaku pemakai lama |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Kerjakan `FE-HMD-16` — penilaian pasca-HD, alur penghentian darurat, dan submit dokumentasi perawat |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 12 elemen — REUSE 12, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Komponen | Status |
| --- | --- | --- |
| Garis waktu pemantauan | `ClinicalTimeline`, `ClinicalTimelineItem` | `REUSE` |
| Banner komplikasi | `ClinicalSafetyAlert` | `REUSE` |
| Pita isian belum tersimpan dan pesan gagal | `InformationAlert` | `REUSE` |
| Field angka pemantauan dan obat | `BaseTextField` | `REUSE` |
| Catatan panjang | `BaseTextAreaField` | `REUSE` |
| Pilihan rute, jenis komplikasi, derajat, hasil, pengaruh sesi | `FilterSelect` | `REUSE` |
| Katalog obat dan dokter | `ResourceFilterSelect` | `REUSE` |
| Penanda status Farmasi | `StatusBadge` | `REUSE` |
| Tombol Mulai, Catat, Kirim Ulang | `BaseButton` | `REUSE` |

Tidak ada elemen yang menuntut komponen baru maupun perubahan base.
