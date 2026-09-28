# Laporan Perubahan Frontend — `FE-HMD-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-16` |
| Judul | Ruang Kerja Sesi HD — Penilaian Pasca-HD, Alur Penghentian Darurat, dan Submit Dokumentasi Perawat (`FE-HMD-07` Pasca-HD) |
| Slice | `MVP-5` — Pasca-HD, Dokumentasi Perawat, Pengesahan DPJP, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.6 |
| Trace | `FR-HMD-070`, `FR-HMD-071`, `FR-HMD-081`, `FE-HMD-07`, `CAP-35`, `HMD-DEC-012` |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak form dan pola konfirmasi. **Pengecualian mengikat**: kondisi akses vaskular pasca-HD wajib diisi, dan penghentian sesi wajib menegaskan bahwa sesi tidak ditagih |
| Keputusan UI Gate | **9 elemen**: `REUSE 9, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-15` (selesai 23 September 2026), `BE-HMD-16` (selesai 22 September 2026) |
| Klasifikasi | `MEDIUM` — skor 10: repository 0, berkas diperiksa 6, berkas dibuat 2, berkas diubah 4, logika 3, kontrak API 3, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 16 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

`FE-HMD-14` dan `FE-HMD-15` sudah memasang bagian Pra-HD dan Intra-HD pada ruang kerja sesi. Thunk `completeSession`, `stopSession`, `savePostAssessment`, dan `submitDocumentation` sudah ada sejak `FE-HMD-02` dengan jalur endpoint yang benar, tetapi belum ada layar yang memakainya. Belum ada pula penanda "Tidak Ditagih" maupun keterangan penyelesai dokumentasi.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Perawat pelaksana, pada akhir sesi cuci darah.

**Alur normal.**

1. Perawat membuka bagian **Pasca-HD & Evaluasi**.
2. Bila tindakan berjalan tuntas sesuai resep durasi dan target penarikan cairan, perawat menekan **Selesaikan Sesi**.
3. Perawat mengisi evaluasi akhir: tekanan darah, nadi, suhu, dan saturasi akhir; berat badan pasca-dialisis; total cairan ditarik sebenarnya; kondisi akses vaskular saat pelepasan jarum; keluhan dan kondisi umum pasien.
4. Perawat memilih **tujuan pasien**: Pulang ke Rumah, Kembali ke Bangsal Rawat Inap, atau Rujuk / Transfer ke IGD / ICU.
5. Perawat menekan **Simpan Evaluasi Pasca-HD**.
6. Perawat menekan **Ajukan Dokumentasi**. Dialog konfirmasi menjelaskan bahwa evaluasi akan disimpan lebih dulu lalu form terkunci. Setelah dikonfirmasi, status sesi menjadi `AwaitingFinalization`, form perawat terkunci, dan banner muncul: "Dokumentasi keperawatan telah diselesaikan pada 23 Sep 2026 13.15. Menunggu pengesahan dokter penanggung jawab."

**Jalur penghentian darurat.**

1. Sesi baru berjalan 60 menit dan pasien mengalami aritmia berat. Perawat menekan **Hentikan Sesi**.
2. Dialog merah terbuka dengan peringatan keselamatan: "Sesi yang dihentikan tidak ditagih — penghentian sesi berarti jasa hemodialisis pada sesi ini tidak dikenakan tagihan kepada pasien. Sebab dan alasan penghentian tercatat permanen pada rekam medis sesi."
3. Perawat memilih sebab penghentian dan **wajib** menuliskan alasannya. Keduanya tidak dapat dilewati.
4. Setelah dikonfirmasi, sesi berhenti dengan status `Stopped` dan lencana **"Tidak Ditagih (Non-Billable)"** muncul di layar.

**Jalur tidak normal.**

- **Kondisi akses vaskular belum diisi** — evaluasi tidak tersimpan; pesan merah muncul pada field itu. Ini penjagaan yang diminta roadmap: pasien tidak boleh meninggalkan unit tanpa catatan hemostasis akses.
- **Tujuan pasien belum dipilih** — "Tujuan pasien setelah sesi wajib dipilih."
- **Angka di luar rentang** — misalnya total cairan ditarik 99999 ml ditolak di layar sebelum terkirim.
- **Dokumentasi sudah diajukan** — form terkunci, tombol simpan tidak dirender, banner penyelesai dokumentasi tampil.
- **Tanpa hak akses** — tombol Hentikan, Selesaikan, dan Ajukan Dokumentasi hanya muncul bila backend mengizinkan aksinya **dan** pengguna memegang hak yang sesuai.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend (read-only).** `Controllers/HmdSessionController.cs`; `DTOs/HmdSessionDtos.cs` (`StopHmdSessionRequest`, `CompleteHmdSessionRequest`, `SaveHmdPostAssessmentRequest`, `HmdSessionAssessmentResponse`, `HmdSessionDetailResponse`); `Enums/HemodialysisEnums.cs` (`HmdSessionStopReason`, `HmdDisposition`, `HmdBillingHandoffStatus`).

**Frontend.** `base-features/confirm-modal.jsx`, `base-form-control.jsx`, `status-badge.jsx`; `clinical-workspace/ClinicalSafetyAlert.jsx`; hook dan section yang dibangun `FE-HMD-14` dan `FE-HMD-15`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../hemodialysis-session-constants.js` | Menambah opsi sebab penghentian, opsi tujuan pasien, tujuh field evaluasi akhir, dan empat field catatan Pasca-HD dengan kondisi akses vaskular ditandai wajib |
| `src/utils/.../hemodialysis-session-display-utils.js` | Menambah `buildPostAssessmentFormValue`, `buildPostAssessmentPayload`, `validatePostAssessmentForm`, `buildStopSessionPayload`, `validateStopSessionForm`, `buildCompleteSessionPayload`, `resolveSessionBillableBadge`, `buildDocumentationBanner` |
| `src/lib/hooks/.../use-hemodialysis-session-workspace.jsx` | Menambah state dan handler Pasca-HD. Pengajuan dokumentasi menyimpan evaluasi lebih dulu dan membatalkan diri bila penyimpanannya gagal, supaya form tidak pernah terkunci dalam keadaan belum lengkap |
| `src/components/view/.../sessions/sections/post-hd-section.jsx` | **Baru.** Penyelesaian sesi, dialog penghentian darurat, evaluasi akhir, dan pengajuan dokumentasi |
| `src/components/view/.../sessions/hemodialysis-session-workspace-view.jsx` | Menambah butir navigasi Pasca-HD beserta penyambungan section |
| `tests/unit/hemodialysis-session-post-hd.test.mjs` | **Baru.** 16 unit test |

### 3.3 Kepatuhan Arsitektur Frontend

Alur dependensi sama dengan `FE-HMD-14`. Tidak ada komponen baru; dialog penghentian memakai `ConfirmModal` dengan `ClinicalSafetyAlert` di dalamnya sebagai penegasan konsekuensi penagihan.

**Urutan pengajuan dokumentasi.** Backend memiliki dua endpoint terpisah: menyimpan evaluasi (`PUT /post-hd`) dan mengajukan dokumentasi (`POST /submit-documentation`). Layar menjalankan keduanya berurutan dalam satu tekan tombol, dan **membatalkan pengajuan** bila penyimpanan evaluasinya gagal validasi. Tanpa urutan itu, form perawat dapat terkunci sementara evaluasinya masih kosong.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol aksi menampilkan "Memproses..." atau "Menyimpan..." selama permintaan berjalan |
| Kosong | "Pengajuan dokumentasi belum tersedia pada keadaan sesi saat ini." ketika backend belum mengizinkan aksinya |
| Gagal | `InformationAlert` merah dari pesan server di atas bagian; pesan validasi per field pada evaluasi dan dialog penghentian |
| Tanpa hak akses | Tombol Hentikan, Selesaikan, dan Ajukan Dokumentasi tidak dirender bagi yang tidak berhak; evaluasi terbaca tanpa dapat disunting |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `.../hemodialysis-sessions/{id}/complete` | Menyelesaikan sesi yang berjalan tuntas | `HemodialysisSession : Complete` |
| `POST` | `.../hemodialysis-sessions/{id}/stop` | Menghentikan sesi lebih awal beserta sebab dan alasannya | `HemodialysisSession : Stop` |
| `PUT` | `.../hemodialysis-sessions/{id}/post-hd` | Menyimpan evaluasi akhir dan tujuan pasien | `HemodialysisSession : Update` |
| `POST` | `.../hemodialysis-sessions/{id}/submit-documentation` | Mengajukan dokumentasi keperawatan | `HemodialysisSession : SubmitDocumentation` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <berkas task ini>` | 0 error, 0 warning | `PASS` | exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-session-post-hd.test.mjs` | 16 test, 16 lolos, 0 gagal | `PASS` | Berkas test baru |
| Grep anti-regresi konsistensi UI, 6 pemeriksaan pada berkas baru | Seluruhnya kosong | `PASS` | Tidak ada warna literal, tombol non-base, utility typography Bootstrap, inline style, dialog peramban, maupun input mentah |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Tidak ada server pengembangan dan sesi login berotorisasi |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

Kontrol interaktif yang **belum** dibuktikan di peramban: dialog penghentian darurat beserta penolakan simpan tanpa alasan, pengisian tujuh field evaluasi, pemilihan tujuan pasien, dialog konfirmasi pengajuan dokumentasi, penguncian form setelah pengajuan, dan munculnya lencana "Tidak Ditagih".

Yang **sudah** dibuktikan di tingkat logika oleh 16 unit test: penolakan penghentian tanpa sebab dan alasan, bentuk payload penghentian, penanda tidak ditagih dari dua sumber terpisah yaitu status `Stopped` dan `BillingHandoffStatus = NotRequired`, kewajiban kondisi akses vaskular, kewajiban tujuan pasien, penegakan rentang angka backend pada evaluasi, bentuk payload evaluasi dan penyelesaian sesi, pembacaan nilai awal dari detail sesi, dan kemunculan banner dokumentasi hanya setelah pengajuan.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Sesi berjalan 60 menit, pasien aritmia berat. Perawat menekan Hentikan Sesi, memilih alasan, menekan konfirmasi. Sesi berstatus `Stopped` dan badge "Tidak Ditagih (Non-Billable)" muncul | Terpenuhi | `ConfirmModal` merah dengan `ClinicalSafetyAlert` menegaskan konsekuensi penagihan; `validateStopSessionForm` menolak simpan tanpa sebab dan alasan; `resolveSessionBillableBadge` menghasilkan lencana "Tidak Ditagih (Non-Billable)" untuk sesi `Stopped` |
| **AC-2** Perawat mengisi evaluasi lengkap dan menekan Ajukan Dokumentasi. Status menjadi `AwaitingFinalization`, form terkunci, banner informasi muncul | Terpenuhi | `handleSubmitDocumentation` menyimpan evaluasi lalu memanggil `POST /submit-documentation`; `postHdEditable` menjadi `false` begitu `DocumentedAt` terisi; `buildDocumentationBanner` menghasilkan banner "Menunggu pengesahan dokter penanggung jawab" |

**Definition of Done** — "Alur evaluasi pasca-HD, submit dokumentasi perawat, dan penghentian darurat berfungsi sesuai kontrak": seluruh butirnya terpenuhi, kecuali component test `SessionPostDialysisViewTests` yang **tidak dipenuhi dalam bentuk yang disebut roadmap** karena repository tidak memakai Jest maupun `@testing-library`; digantikan 16 unit test `node:test` menurut keputusan pemilik 1 September 2026. Mitigasi risiko roadmap berupa kolom kondisi akses vaskular pasca-HD sebagai field wajib **ikut diimplementasikan dan diuji**.

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — 0 error dan 0 warning |
| Masalah yang diketahui | Daftar cacat modul di luar cakupan dari `FE-HMD-12`, `FE-HMD-13`, `FE-HMD-14`, dan `FE-HMD-15` masih berlaku dan belum diperbaiki |
| Dependency backend | Tidak ada yang tertahan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Kerjakan `FE-HMD-17` — pengesahan dokter DPJP, penguncian catatan, dan daftar koreksi addendum |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 9 elemen — REUSE 9, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Komponen | Status |
| --- | --- | --- |
| Dialog penghentian darurat dan konfirmasi pengajuan | `ConfirmModal` | `REUSE` |
| Penegasan konsekuensi penagihan | `ClinicalSafetyAlert` | `REUSE` |
| Lencana Tidak Ditagih | `StatusBadge` | `REUSE` |
| Banner dokumentasi dan pesan gagal | `InformationAlert` | `REUSE` |
| Field angka evaluasi akhir | `BaseTextField` | `REUSE` |
| Catatan panjang dan alasan penghentian | `BaseTextAreaField` | `REUSE` |
| Pilihan sebab penghentian dan tujuan pasien | `FilterSelect` | `REUSE` |
| Tombol Selesaikan, Hentikan, Simpan, Ajukan | `BaseButton` | `REUSE` |
| Panel bagian | Kelas panel modul yang sudah dipakai `FE-HMD-14` | `REUSE` |
