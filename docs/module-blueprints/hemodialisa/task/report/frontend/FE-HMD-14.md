# Laporan Perubahan Frontend — `FE-HMD-14`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-14` |
| Judul | Ruang Kerja Sesi HD — Header Konteks Pasien, Checklist Pra-HD, dan Status Sesi Siap (`FE-HMD-07` Pra-HD) |
| Slice | `MVP-4` — Pelaksanaan Sesi, Checklist Pra-HD, Pemantauan, dan Komplikasi |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.5 |
| Trace | `FR-HMD-050`, `FR-HMD-051`, `FR-HMD-052`, `FE-HMD-07`, `CAP-30`, `NFR-007`; `contracts/api-contract.md` grup Hemodialysis Session Checklist; `contracts/validation-matrix.md` Bagian 3 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak form dan tab. **Pengecualian mengikat**: prinsip gagal tertutup `NFR-007` — konteks sesi tidak terverifikasi berarti tidak ada data klinis maupun tombol input yang boleh tampil |
| Keputusan UI Gate | **11 elemen**: `REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` |
| Dependency | `FE-HMD-02` (selesai), `BE-HMD-12` (selesai 22 September 2026), `BE-HMD-13` (selesai 22 September 2026) |
| Klasifikasi | `HEAVY` — skor 12: repository 0, berkas diperiksa 14, berkas dibuat 6, berkas diubah 1, logika 4, kontrak API 4, database 0, UI 4 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — kedua acceptance criteria terpetakan ke source; ESLint 0 error 0 warning; unit test baru 24 lolos. `npm run build` `NOT RUN` (dikecualikan atas keputusan tetap pemilik 10 September 2026) |

---

## 1. Keadaan yang Ditemukan di Awal

1. Route `/sessions/{sessionId}` baru berupa halaman sementara yang dipasang `FE-HMD-12` agar tombol "Buka Sesi" tidak berakhir 404. Isinya hanya `Hero` dan satu pemberitahuan.
2. Seluruh dua puluh thunk sesi sudah ada sejak `FE-HMD-02` dan jalur endpoint-nya **sudah benar** terhadap `HmdSessionController` — diperiksa satu per satu. Yang belum ada adalah layar yang memakainya.
3. Kesembilan komponen klinis baku yang diwajibkan roadmap ternyata **tidak** berada di `src/components/ui/doctor-clinical-base/` melainkan di `src/components/ui/clinical-workspace/`, lengkap dengan `ClinicalWorkspaceShell`, `PatientContextHeader`, `ClinicalSectionNav`, `ClinicalCompletionBar`, `ClinicalSafetyAlert`, dan `ClinicalValidationSummary`. Modul rujukan yang sudah memakainya adalah Ruang Kerja Episode (`FE-HMD-10`/`FE-HMD-11`).

---

## 2. Proses Bisnis dari Sisi Pengguna

**Siapa yang memakai.** Perawat pelaksana unit hemodialisa, bekerja di samping tempat tidur pasien.

**Alur normal.**

1. Dari layar Jadwal & Daftar Kerja, perawat menekan **Buka Sesi** pada baris pasiennya dan masuk ke ruang kerja sesi.
2. Kepala layar menampilkan identitas pasien, nomor sesi, nomor station dan mesin, dokter penanggung jawab, status sesi, shift, dan jam mulai terjadwal.
3. Pada bagian **Pra-HD & Persiapan**, perawat mengisi tanda vital sebelum tindakan: tekanan darah sistolik dan diastolik, nadi, laju napas, suhu, saturasi oksigen, dan berat badan pra-dialisis. Berat badan wajib diisi karena menjadi dasar perhitungan penarikan cairan. Rentang angkanya sudah dibatasi sesuai yang diterima server, sehingga suhu 50 derajat atau tekanan sistolik 400 ditolak di layar sebelum terkirim.
4. Perawat mengisi catatan keluhan pasien, kondisi akses vaskular, dan kondisi umum, lalu menekan **Simpan Penilaian Pra-HD**.
5. Di bawahnya tersaji **butir persiapan keselamatan**. Indikator kemajuan menunjukkan berapa butir wajib yang sudah terpenuhi, misalnya "9 dari 12 butir wajib terpenuhi". Setiap butir dipilih hasilnya: Terpenuhi, Tidak Terpenuhi, atau Tidak Berlaku, dengan catatan pemeriksaan opsional.
6. Butir yang memang ditandai unit sebagai dapat dilewati menampilkan tombol **Lewati**. Menekannya membuka dialog konfirmasi yang **mewajibkan alasan**; alasan itu tercatat beserta nama pemeriksa dan waktu server.
7. Perawat menekan **Simpan Butir Persiapan**. Tombol ini tidak aktif selama belum ada butir yang diubah.
8. Selama masih ada butir wajib tertahan, tombol **Nyatakan Sesi Siap** berwarna abu-abu. Di bawahnya, ringkasan validasi menyebut persis butir mana yang menahan — contohnya "Akses Vaskular Layak — belum diperiksa." atau "Dokter penanggung jawab sesi belum ditetapkan."
9. Setelah seluruh butir wajib terpenuhi dan dokter penanggung jawab sudah ditetapkan, tombol menjadi aktif. Perawat menekannya, status sesi berubah menjadi `Ready`, dan layar memuat ulang konteks sesi.

**Jalur tidak normal.**

- **Konteks sesi tidak dapat diverifikasi** — ini jalur keselamatan yang paling penting. Bila pembacaan sesi gagal, atau berhasil tetapi tidak membawa identitas pasien maupun nomor sesi, **seluruh layar** diganti peringatan merah "Konteks Sesi Tidak Terverifikasi" beserta tombol coba lagi dan tombol kembali. Tidak ada satu pun data klinis, form, maupun tombol input yang dirender. Menampilkan ruang kerja setengah terisi jauh lebih berbahaya daripada menolak membukanya.
- **Catatan sudah disahkan dan terkunci** — pita informasi muncul di atas bagian Pra-HD, dan seluruh kontrol penyuntingan tertutup karena backend tidak lagi mengizinkan aksinya.
- **Pengguna tanpa hak sunting** — form dan tombol simpan tidak dirender; butir persiapan tetap terbaca beserta hasilnya dalam bentuk teks.
- **Gagal menyimpan** — pesan merah dari server muncul di atas bagian Pra-HD tanpa menghapus isian perawat.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

**Backend sebagai sumber kontrak (read-only).** `Controllers/HmdSessionController.cs`; `DTOs/HmdSessionDtos.cs` (`HmdSessionDetailResponse`, `HmdSessionChecklistResponse`, `SaveHmdChecklistRequest`, `SaveHmdPreAssessmentRequest`, `HmdVitalSignInput`, `OverrideHmdChecklistRequest`); `DTOs/HmdCommonDtos.cs`; `Enums/HemodialysisEnums.cs`.

**Frontend sebagai rujukan pola.** Seluruh isi `src/components/ui/clinical-workspace/`; modul rujukan `patients/workspace/hemodialysis-episode-workspace-view.jsx`; `base-features/base-form-control.jsx`; `base-features/confirm-modal.jsx`; `hemodialysisSessionSlice.js`; `hmdSessionService.js`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/hemodialysis-management/hemodialysis-session-constants.js` | **Baru.** Kunci bagian ruang kerja, nilai dan label `HmdChecklistResult`, label kategori butir, konfigurasi tujuh field tanda vital beserta rentang yang mengikuti `[Range]` backend, tiga field catatan, dan daftar nama aksi `AvailableActions` |
| `src/utils/health-services/hemodialysis-management/hemodialysis-session-display-utils.js` | **Baru.** Fungsi murni: `evaluateSessionContext` (gerbang `NFR-007`), `calculateChecklistProgress`, `buildReadinessValidationItems`, `canDeclareSessionReady`, `isSessionActionAvailable`, `isSessionLocked`, `buildChecklistPayload`, `buildPreAssessmentPayload`, `buildPreAssessmentFormValue`, `validatePreAssessmentForm`, `formatSessionDateTime` |
| `src/lib/hooks/health-services/hemodialysis-management/use-hemodialysis-session-workspace.jsx` | **Baru.** Controller ruang kerja: pembacaan konteks, pembersihan state antar pasien, draf checklist, form Pra-HD, dan empat aksi sesi |
| `src/components/view/health-services/hemodialysis-management/sessions/hemodialysis-session-workspace-view.jsx` | **Baru.** Kerangka ruang kerja, kepala konteks pasien, navigasi bagian, dan gerbang gagal tertutup |
| `src/components/view/health-services/hemodialysis-management/sessions/sections/pre-hd-section.jsx` | **Baru.** Form tanda vital, daftar butir persiapan, bar kemajuan, ringkasan validasi, dan pernyataan sesi siap |
| `src/style/health-services/hemodialysis-management/hemodialysis-session-workspace.module.css` | **Baru.** Seluruh nilai visual memakai `var(--token)` |
| `src/app/health-services/hemodialysis-management/sessions/[sessionId]/page.jsx` | Halaman sementara `FE-HMD-12` diganti route tipis yang membaca `sessionId` dan memanggil view |
| `tests/unit/hemodialysis-session-pre-hd.test.mjs` | **Baru.** 24 unit test fungsi murni |

### 3.3 Kepatuhan Arsitektur Frontend

```text
src/app/.../sessions/[sessionId]/page.jsx   -> route tipis, hanya membaca parameter
  -> components/view/.../sessions/            -> komposisi layar dan section
  -> lib/hooks/.../use-hemodialysis-session-workspace.jsx -> controller
  -> lib/state/slice/.../hemodialysisSessionSlice.js      -> thunk
  -> lib/services/.../hmdSessionService.js -> InstanceAxios -> Backend API
```

Sembilan komponen klinis baku dipakai apa adanya dari `@/components/ui/clinical-workspace`; tidak ada satu pun duplikatnya dibuat, sesuai larangan keras pada roadmap bagian 1.

**Sumber kebenaran tombol.** Layar tidak menyimpulkan sendiri tombol mana yang boleh muncul dari status sesi, melainkan membaca `AvailableActions` yang dikirim backend. Dengan begitu layar tidak pernah menawarkan aksi yang pasti ditolak server, dan perubahan aturan transisi di backend tidak menuntut perubahan layar.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kartu memuat `ClinicalStateBoundary` beserta "Memuat konteks sesi hemodialisa..." dan penjelasan singkat |
| Kosong | Butir persiapan kosong menampilkan "Butir persiapan belum diisi." |
| Gagal | Gagal membaca konteks sesi **mengambil alih seluruh layar** dengan `ClinicalSafetyAlert` merah "Konteks Sesi Tidak Terverifikasi" beserta tombol coba lagi dan kembali. Gagal menyimpan menampilkan `InformationAlert` merah di atas bagian, tanpa menghapus isian |
| Tanpa hak akses | Tanpa `HemodialysisSession : Update`, form Pra-HD dan penyuntingan butir tidak dirender. Tanpa `HemodialysisSession : DeclareReady`, tombol pernyataan siap tidak pernah aktif. Tanpa `HemodialysisSession : OverrideChecklist`, tombol Lewati tidak dirender |

---

## 5. Endpoint yang Dikonsumsi

#### Health Services / Hemodialysis Management / Hemodialysis Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/hemodialysis-management/hemodialysis-sessions/{id}` | Konteks sesi, penilaian Pra-HD tersimpan, dan daftar aksi yang diizinkan | `HemodialysisSession : Read` |
| `GET` | `.../hemodialysis-sessions/{id}/checklist` | Butir persiapan keselamatan beserta status pemenuhannya | `HemodialysisSession : Read` |
| `PUT` | `.../hemodialysis-sessions/{id}/checklist` | Menyimpan hasil pemeriksaan butir | `HemodialysisSession : Update` |
| `POST` | `.../hemodialysis-sessions/{id}/checklist/{itemId}/override` | Melewati satu butir dengan alasan terdokumentasi | `HemodialysisSession : OverrideChecklist` |
| `PUT` | `.../hemodialysis-sessions/{id}/pre-hd` | Menyimpan penilaian dan tanda vital Pra-HD | `HemodialysisSession : Update` |
| `POST` | `.../hemodialysis-sessions/{id}/ready` | Menyatakan sesi siap untuk tindakan | `HemodialysisSession : DeclareReady` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <7 berkas task ini>` | 0 error, 0 warning | `PASS` | exit code 0, keluaran kosong |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-session-pre-hd.test.mjs` | 24 test, 24 lolos, 0 gagal | `PASS` | Berkas test baru milik task ini |
| Grep anti-regresi konsistensi UI, 9 pemeriksaan | Seluruhnya kosong | `PASS` | Termasuk pemeriksaan tambahan bahwa tidak ada `window.prompt`, `window.alert`, maupun `window.confirm` |
| Uji interaktif di peramban | Tidak dijalankan | `NOT FEASIBLE` | Tidak ada server pengembangan dan sesi login berotorisasi pada sesi kerja ini |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

**Cacat yang ditemukan saat memverifikasi kontrak komponen.** `ClinicalStateBoundary` pada `clinical-workspace` menerima `retryAction` berupa node dan `loadingTitle`, bukan `onRetry` dan `loadingText`. Pemakaian pertama saya salah dan **sudah diperbaiki** sebelum task ditutup. Pemeriksaan yang sama menemukan bahwa Ruang Kerja Episode dari `FE-HMD-10`/`FE-HMD-11` memakai `onRetry` dan `loadingText` — keduanya diabaikan komponen, sehingga tombol coba lagi tidak pernah muncul di sana. Cacat itu di luar cakupan task dan dilaporkan pada bagian 8.

Kontrol interaktif yang **belum** dibuktikan di peramban: pengisian tujuh field tanda vital dan tiga catatan, penyimpanan penilaian Pra-HD, pemilihan hasil pada dua belas butir, dialog konfirmasi melewati butir beserta alasan wajibnya, pergerakan bar kemajuan, pengaktifan tombol pernyataan siap, dan tampilan gagal tertutup.

Yang **sudah** dibuktikan di tingkat logika oleh 24 unit test: kelima jalur gerbang `NFR-007` termasuk tiga bentuk konteks tidak lengkap, penghitungan kemajuan 9 dari 12 butir wajib beserta pengabaian butir tidak wajib, ketiga syarat pengaktifan tombol pernyataan siap dan masing-masing jalur penahannya, pembacaan `AvailableActions` tanpa membedakan besar kecil huruf, pengenalan sesi terkunci dari dua sumber, bentuk payload checklist yang hanya memuat butir bernilai, bentuk payload Pra-HD yang mengirim angka sebagai angka, dan validasi rentang yang mengikuti `[Range]` backend.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Perawat belum mencentang butir "Akses Vaskular Layak"; tombol "Nyatakan Sesi Siap" nonaktif, dan `ClinicalValidationSummary` mencantumkan butir merah "Akses vaskular belum diverifikasi kelaikannya" | Terpenuhi | `canDeclareSessionReady` mengembalikan `false` selama ada butir wajib tertahan, dan `buildReadinessValidationItems` menghasilkan baris "Akses Vaskular Layak — belum diperiksa." Diuji pada tiga jalur penahan terpisah |
| **AC-2** Seluruh 12 butir terisi; tombol menjadi aktif, ditekan, status sesi berubah menjadi `Ready` | Terpenuhi | `calculateChecklistProgress` menandai lengkap pada 12 dari 12, tombol aktif, `handleDeclareReady` memanggil `POST /{id}/ready` lalu memuat ulang konteks sesi |

**Definition of Done** — "Ruang kerja sesi tahap pra-HD berfungsi lengkap dengan komponen bar kemajuan dan ringkasan validasi keselamatan":

| Butir | Status |
| --- | --- |
| Halaman `sessions/[sessionId]/page.jsx` | Terpenuhi |
| Kepala informasi pasien dan sesi | Terpenuhi |
| Prinsip gagal tertutup `NFR-007` | Terpenuhi |
| Form tanda vital Pra-HD | Terpenuhi |
| Dua belas butir checklist dengan status periksa | Terpenuhi — jumlah butirnya datang dari server, bukan dipatok dua belas di layar |
| `ClinicalCompletionBar` menunjukkan kemajuan | Terpenuhi |
| Tombol "Nyatakan Sesi Siap" beserta `ClinicalValidationSummary` | Terpenuhi |
| Component integration test `SessionPreCheckViewTests` | Tidak dipenuhi dalam bentuk yang disebut roadmap. Repository tidak memakai Jest maupun `@testing-library`; digantikan 24 unit test `node:test`. Butir DoD berbentuk test dilepas atas keputusan pemilik 1 September 2026 |

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` — 0 error dan 0 warning pada seluruh berkas task ini |
| Masalah yang diketahui | **Satu cacat baru ditemukan di luar cakupan.** Ruang Kerja Episode (`FE-HMD-10`/`FE-HMD-11`) memanggil `ClinicalStateBoundary` dengan prop `onRetry` dan `loadingText`, padahal komponennya menerima `retryAction` dan `loadingTitle`. Akibatnya tombol coba lagi tidak pernah muncul saat pemuatan episode gagal, dan teks memuatnya memakai bawaan. Menambah daftar cacat modul di luar cakupan yang sudah dilaporkan pada `FE-HMD-12.md` bagian 8 dan `FE-HMD-13.md` bagian 8 |
| Dependency backend | Tidak ada yang tertahan. `BE-HMD-12` dan `BE-HMD-13` selesai 22 September 2026 |
| Perubahan sampingan | `NONE`. Halaman sementara `sessions/[sessionId]/page.jsx` yang diganti memang dipasang `FE-HMD-12` sebagai tempat masuk yang menunggu task ini |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Kerjakan `FE-HMD-15` — tombol mulai sesi berpenanda idempotensi, garis waktu observasi intra-HD, pencatatan obat, dan komplikasi |

---

## 9. Tabel Keputusan Base Component

`UI GATE: 11 elemen — REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Komponen | Status |
| --- | --- | --- |
| Kerangka ruang kerja | `ClinicalWorkspaceShell` | `REUSE` |
| Kepala konteks pasien | `PatientContextHeader` | `REUSE` |
| Navigasi bagian | `ClinicalSectionNav` | `REUSE` |
| Panel isi bagian | `ClinicalContentPanel` | `REUSE` |
| Empat keadaan layar | `ClinicalStateBoundary` | `REUSE` |
| Peringatan keselamatan dan penguncian | `ClinicalSafetyAlert` | `REUSE` |
| Indikator kemajuan checklist | `ClinicalCompletionBar` | `REUSE` |
| Ringkasan butir tertahan | `ClinicalValidationSummary` | `REUSE` |
| Field tanda vital dan catatan | `BaseTextField`, `BaseTextAreaField` | `REUSE` |
| Pemilihan hasil butir | `FilterSelect` | `REUSE` |
| Konfirmasi melewati butir beserta alasan wajib | `ConfirmModal` `requireReason` | `REUSE` |

Tidak ada elemen berstatus `NEW`, `EXTEND`, `COMPOSE`, maupun `WRAP`, sehingga gerbang tidak menahan satu pun bagian task ini.
