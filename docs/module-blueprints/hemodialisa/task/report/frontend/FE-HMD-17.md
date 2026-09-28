# Laporan Perubahan Frontend — `FE-HMD-17`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-17` |
| Judul | Ruang Kerja Sesi HD — Pengesahan Dokter DPJP, Penguncian Catatan, dan Daftar Koreksi Addendum (`FE-HMD-07` Pengesahan) |
| Slice | `MVP-5` — Pasca-HD, Dokumentasi Perawat, Pengesahan DPJP, dan Penagihan |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.6 |
| Trace | `FR-HMD-071`, `FR-HMD-072`, `FR-HMD-073`, `FR-HMD-074`, `FE-HMD-07`, `CAP-07`, `CAP-08`, `NFR-008`, Temuan Kritis 1; `contracts/permission-audit-matrix.md` Bagian 4 dan 5 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak. **Pengecualian mengikat**: tombol Sahkan hanya boleh muncul bagi dokter penanggung jawab sesi; catatan yang sudah disahkan wajib 100% baca saja |
| Keputusan UI Gate | **8 elemen**: `REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-16` (selesai 23 September 2026), `BE-HMD-17` (selesai 22 September 2026) |
| Klasifikasi | `MEDIUM` — skor 10: repository 0, berkas diperiksa 8, berkas dibuat 2, berkas diubah 3, logika 3, kontrak API 3, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — ketiga acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 16 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. Tiga bagian ruang kerja sesi sudah terpasang dari `FE-HMD-14` s/d `FE-HMD-16`; bagian Finalisasi belum ada.
2. Thunk `finalizeSession` sudah tersedia sejak `FE-HMD-02` dengan jalur `POST /{id}/finalize` yang benar.
3. Service addendum rekam medis **sudah ada dan dapat dipakai ulang**: `src/lib/services/health-services/medical-record-management/clinical-note-addendum.service.js` menyediakan `getClinicalNoteAddendums(documentKind, documentId)` dan `createClinicalNoteAddendum(documentKind, documentId, payload)`. Jenis dokumen yang dikenali backend untuk sesi HD adalah `HemodialysisSession`, sesuai keterangan pada `HmdRecordController.cs`.
4. `userInfo` hasil login **membawa `doctorId`**, sehingga identitas dokter penanggung jawab dapat dibandingkan di layar tanpa permintaan tambahan.
5. `ClinicalAddendumList` membaca item berbentuk `{id, sequence, content, reason, authorUserName, createdAt}`, sedangkan backend mengirim `{id, sequence, addendumText, correctionReason, authorName, signedAt}` — perlu pemetaan.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Dokter penanggung jawab sesi untuk pengesahan; perawat dan dokter lain untuk membaca dan menambahkan koreksi.

**Alur normal.**

1. Setelah perawat mengajukan dokumentasi, sesi berstatus menunggu pengesahan. Dokter penanggung jawab membuka bagian **Pengesahan & Koreksi**.
2. Bila akun yang membuka adalah dokter penanggung jawab sesi itu, tombol **Sahkan & Kunci Catatan Sesi** tersedia aktif.
3. Menekannya membuka dialog tinjauan berisi peringatan: "Pengesahan bersifat permanen — dengan menekan Sahkan, Anda menyatakan telah meninjau seluruh catatan sesi ini dan menandatanganinya secara elektronik. Catatan akan terkunci secara hukum klinis dan tidak dapat disunting lagi."
4. Setelah dikonfirmasi, status sesi menjadi `Finalized`. Banner gembok muncul: "Dokumen telah disahkan oleh dr. Rahmat, Sp.PD-KGH pada 18 Sep 2026 13.15. Catatan terkunci secara hukum klinis. Perubahan data hanya dapat dilakukan melalui Addendum Rekam Medis."
5. Seluruh form pada empat bagian ruang kerja — Pra-HD, Intra-HD, Pasca-HD, dan Finalisasi — berubah menjadi baca saja. Tombol simpan tidak lagi dirender.
6. Untuk membetulkan data, perawat menekan **Tambah Koreksi** pada panel koreksi rekam medis, mengisi alasan koreksi dan teks pembetulan, lalu menyimpannya. Koreksi muncul berdampingan dengan catatan asli; angka awal tidak pernah dihapus.

**Jalur tidak normal.**

- **Perawat Rina membuka sesi berstatus menunggu pengesahan** — tombol Sahkan **tidak tersedia** baginya. Yang tampil adalah penjelasan wewenang: "Akun Anda tidak memiliki wewenang mengesahkan catatan sesi hemodialisa."
- **Dokter lain yang bukan DPJP sesi ini** — tombol juga tidak dirender, dengan penjelasan: "Pengesahan hanya dapat dilakukan oleh dokter penanggung jawab sesi ini. Akun Anda bukan dokter penanggung jawabnya."
- **Sesi belum siap disahkan** — "Sesi belum berada pada keadaan yang dapat disahkan."
- **Sesi tanpa dokter penanggung jawab** — "Sesi ini belum memiliki dokter penanggung jawab."
- **Gagal membaca daftar koreksi** — pesan merah muncul pada panel koreksi; sisa layar tetap dapat dipakai.
- **Gagal mengesahkan** — kunci tombol dilepas supaya dokter dapat mencoba lagi dengan kunci idempotensi yang sama.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend (read-only).** `Controllers/HmdRecordController.cs`; `Controllers/HmdSessionController.cs`; `Areas/HealthServices/MedicalRecordManagement/Controllers/ClinicalNoteAddendumController.cs`; `DTOs/ClinicalDocumentIntegrityDtos.cs` (`CreateClinicalNoteAddendumRequest`, `ClinicalNoteAddendumResponse`); `DTOs/HmdSessionDtos.cs` (`FinalizeHmdSessionRequest`).

**Frontend.** `clinical-workspace/ClinicalAddendumList.jsx` dan `ClinicalAddendumItem.jsx`; `clinical-note-addendum.service.js`; `lib/state/slice/auth/login-slice.jsx` untuk bentuk `userInfo`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/.../hemodialysis-session-display-utils.js` | Menambah `HMD_ADDENDUM_DOCUMENT_KIND`, `evaluateFinalizeAuthority`, `buildFinalizationBanner`, `mapAddendumsForDisplay`, `validateAddendumForm` |
| `src/lib/hooks/.../use-hemodialysis-session-workspace.jsx` | Menambah state dan handler pengesahan serta addendum, penjaga penekanan ganda tombol Sahkan, kunci idempotensi yang dipakai ulang, dan pembacaan daftar koreksi saat bagiannya dibuka |
| `src/components/view/.../sessions/sections/finalization-section.jsx` | **Baru.** Pengesahan, banner terkunci, daftar koreksi, dan dua dialog konfirmasi |
| `src/components/view/.../sessions/hemodialysis-session-workspace-view.jsx` | Menambah butir navigasi Pengesahan & Koreksi beserta penyambungan section |
| `tests/unit/hemodialysis-session-finalization.test.mjs` | **Baru.** 16 unit test |

### 3.3 Kepatuhan Arsitektur Frontend

Pembacaan dan penambahan addendum memakai service rekam medis yang sudah ada, **tanpa** menambah slice Redux baru. Daftar koreksi hanya dipakai layar ini dan tidak dibagi lintas halaman, sehingga menurut `rules/frontend/frontend-architecture.md` service lepas memang jalur yang tepat.

`ClinicalAddendumList` dan `ClinicalAddendumItem` dipakai apa adanya, dan perbedaan bentuk data dijembatani satu fungsi pemetaan murni di lapisan `utils` — bukan dengan menyalin atau memodifikasi komponennya.

**Penegakan ganda wewenang pengesahan.** Backend sudah menolak dokter bukan DPJP dengan `403 HMD-VAL-072`. Layar tetap menegakkannya sendiri karena `permission-audit-matrix.md` menuntut tombol yang tidak berhak **disembunyikan**, bukan ditampilkan lalu ditolak. Penegakan di layar adalah lapisan kenyamanan; backend tetap menjadi penjaga sesungguhnya.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat riwayat koreksi rekam medis..." pada panel koreksi; tombol Sahkan menampilkan keadaan memproses |
| Kosong | "Dokumen ini belum memiliki addendum pembetulan." dari `ClinicalAddendumList` |
| Gagal | `InformationAlert` merah untuk kegagalan pengesahan maupun pembacaan koreksi; kegagalan koreksi tidak menutup sisa layar |
| Tanpa hak akses | Tombol Sahkan tidak dirender, digantikan `ClinicalValidationSummary` yang menyebut alasan wewenangnya. Tombol Tambah Koreksi hanya muncul bagi pemegang `ClinicalNoteAddendum : Create` |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Record

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `.../hemodialysis-sessions/{id}/finalize` | Mengesahkan dan mengunci catatan sesi | `HemodialysisRecord : Finalize` |

#### Health Services / Medical Record Management / Clinical Note Addendum

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/medical-record-management/clinical-note-addendums/by-document/HemodialysisSession/{sessionId}` | Riwayat koreksi rekam medis sesi | `ClinicalNoteAddendum : Read` |
| `POST` | `/v1/health-services/medical-record-management/clinical-note-addendums/by-document/HemodialysisSession/{sessionId}` | Menambahkan koreksi resmi | `ClinicalNoteAddendum : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <berkas task ini>` | 0 error, 0 warning | `PASS` | exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-session-finalization.test.mjs` | 16 test, 16 lolos, 0 gagal | `PASS` | Berkas test baru |
| Grep anti-regresi konsistensi UI, 5 pemeriksaan pada berkas baru | Seluruhnya kosong | `PASS` | Tidak ada tombol non-base, utility typography Bootstrap, inline style, dialog peramban, maupun input mentah |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Tidak ada server pengembangan dan sesi login berotorisasi; pengujian AC-1 secara menyeluruh juga menuntut dua akun berbeda |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

Kontrol interaktif yang **belum** dibuktikan di peramban: perbedaan tampilan antara akun perawat, akun dokter bukan DPJP, dan akun DPJP; dialog tinjauan pengesahan; peralihan seluruh layar menjadi baca saja setelah pengesahan; serta form penambahan koreksi.

Yang **sudah** dibuktikan di tingkat logika oleh 16 unit test: keenam jalur otorisasi pengesahan termasuk perawat, dokter lain, sesi belum siap, dan sesi tanpa DPJP; perbandingan identitas dokter tanpa membedakan besar kecil huruf; isi banner terkunci termasuk penyebutan nama dokter, waktu, dan rujukan ke addendum; pengenalan sesi terkunci dari dua sumber; pemetaan bentuk addendum backend ke bentuk yang dibaca komponen; pengurutan daftar koreksi; dan validasi addendum termasuk kedua batas panjang backend.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Perawat Rina membuka sesi `AwaitingFinalization`; tombol Sahkan tidak tersedia baginya. Dokter Rahmat (DPJP sesi) membuka sesi yang sama; tombol Sahkan tersedia aktif | Terpenuhi | `evaluateFinalizeAuthority` mengembalikan `canFinalize: false` beserta alasannya untuk perawat dan untuk dokter bukan DPJP, dan `true` hanya untuk DPJP sesi yang bersangkutan. View merender tombol **hanya** ketika `canFinalize` bernilai benar |
| **AC-2** Setelah pengesahan, status menjadi `Finalized`. Tidak ada field yang dapat diketik ulang; tombol koreksi yang tersedia adalah "Tambah Koreksi (Addendum)" | Terpenuhi | `isSessionLocked` mengenali `Finalized` maupun penanda `IsLocked`, dan `locked` menutup `checklistEditable`, `preHdEditable`, `intraHdEditable`, serta `postHdEditable` pada seluruh bagian. Panel koreksi tetap tersedia |
| **AC-3** Perawat menambahkan addendum pembetulan tensi akhir. Panel koreksi menampilkan catatan koreksi berdampingan dengan catatan asli tanpa menghapus angka awal | Terpenuhi | Koreksi disimpan lewat endpoint addendum rekam medis terpisah dan dirender `ClinicalAddendumList`; tidak ada jalur di layar yang menimpa nilai asli |

**Definition of Done** — "Pengesahan DPJP, penguncian visual 100% read-only, dan daftar koreksi addendum terpasang sempurna": seluruh butirnya terpenuhi, kecuali component integration test `SessionFinalizeAndAddendumViewTests` yang **tidak dipenuhi dalam bentuk yang disebut roadmap** karena repository tidak memakai Jest maupun `@testing-library`; digantikan 16 unit test `node:test` menurut keputusan pemilik 1 September 2026.

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu peringatan `react-hooks/set-state-in-effect` sempat muncul pada pembacaan daftar koreksi; **sudah dihilangkan** dengan memindahkan seluruh penetapan state ke dalam callback janji, bukan badan effect. Hasil akhir 0 error 0 warning |
| Masalah yang diketahui | Penegakan wewenang pengesahan di layar bergantung pada `userInfo.doctorId` yang dikirim saat login. Bila akun dokter tidak membawa `doctorId`, tombol Sahkan tidak akan pernah muncul walaupun pengguna sebenarnya berhak — backend tetap menjadi penjaga sesungguhnya, dan kasus ini muncul sebagai penjelasan wewenang, bukan sebagai kegagalan diam. Daftar cacat modul di luar cakupan dari `FE-HMD-12` s/d `FE-HMD-16` masih berlaku |
| Dependency backend | Tidak ada yang tertahan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Kerjakan `FE-HMD-18` — beranda eksekutif unit dan panel pemantauan serah terima tagihan |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 8 elemen — REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Komponen | Status |
| --- | --- | --- |
| Daftar koreksi rekam medis | `ClinicalAddendumList`, `ClinicalAddendumItem` | `REUSE` |
| Banner dokumen terkunci dan peringatan pengesahan permanen | `ClinicalSafetyAlert` | `REUSE` |
| Penjelasan wewenang saat tombol Sahkan disembunyikan | `ClinicalValidationSummary` | `REUSE` |
| Dialog tinjauan pengesahan dan dialog koreksi | `ConfirmModal` | `REUSE` |
| Field alasan dan teks pembetulan | `BaseTextAreaField` | `REUSE` |
| Pesan gagal | `InformationAlert` | `REUSE` |
| Tombol Sahkan | `BaseButton` | `REUSE` |
| Panel bagian | Kelas panel modul yang sudah dipakai `FE-HMD-14` | `REUSE` |
