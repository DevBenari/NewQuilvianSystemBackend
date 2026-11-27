# Laporan Perubahan Frontend — `FE-HRD-411`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-411` |
| Judul | Matriks roster menampilkan petugas pengganti |
| Slice | `A52-B` / `MVP-78` · `EPIC HRD-131` (`MUST HAVE`) |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-411` (amandemen `A52-B`) |
| Trace | `FR-PK-033`; `HRD-DEC-248`; `VR-PK-37`; `api-contract.md` 37.2; desain `03` 36.3 dan 36.5 |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Bentuk tanda pengganti `DEV_DISCRETION` (kartu). Layar yang diubah: `/hr/scheduling/roster/[id]` (`F-T1`); route tidak berubah. Komponen dasar yang dipakai: `StatusBadge`, `HrInfoGrid`, `ConfirmModal`; kisi tetap elemen khusus (`FE-HRD-R38`) |
| Dependency | `FE-HRD-410` ✅; `BE-HRD-463` ✅ (respons detail roster diperluas); bukti data asli memakai data `BE-HRD-468` |
| Klasifikasi | `MEDIUM` — satu view, dua modal, satu stylesheet diubah; satu utilitas baru; uji unit dan spec E2E baru; tanpa service, slice, atau route baru |
| Task mode | `FRONTEND` (instruksi pengguna 27 November 2026: "Langkah berikutnya adalah FE-HRD-411 … kerjakan ini") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/hr/scheduling`, `src/utils/hr/scheduling`, `src/style/hr/scheduling`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan `FE-HRD-410` yang belum di-commit + perubahan Lab milik sesi lain di working tree (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467`/`468` yang belum di-commit; DLL lokal 26 November 2026 12:41 |
| Tanggal | 27 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Kisi membaca shift per baris penugasan apa adanya | `roster-matrix-detail-view.jsx`: `new Map(assignment.shiftAssignments…)` per `assignment` | Sesudah `BE-HRD-463` hasilnya sudah benar, tetapi tergantung sepenuhnya pada pengelompokan server; ruas `shiftAssignments[].workforceProfileId` yang diminta kartu tidak dibaca |
| Ganti Petugas mengirim petugas baris, bukan petugas sel | `openCell` mengisi `workforceProfileId: assignment.workforceProfileId` | Bila shift pengganti tampil di baris petugas awal, kirimannya ditolak `409` (`VR-PK-37`) |
| Baris pengganti-saja (`id` = `00000000-…`) belum dikenali | `<tr key={assignment.id}>`; sel kosong membuka Tambah Shift dengan `positionId` baris | Beberapa baris pengganti-saja berkunci React sama; sel kosongnya membuka Tambah Shift untuk petugas yang tidak punya penugasan |
| Modal Terapkan Pola memakai seluruh `period.assignments` | `ApplyShiftPatternModal.jsx` baris 29 | Baris pengganti-saja ikut menjadi sasaran pola dan ikut dihitung di "Semua staf di periode ini (n)" |
| Ringkasan "Staf Terdaftar" | `totalAssignmentCount ?? assignments.length` | Cadangan ikut menghitung baris pengganti-saja |
| `plannedShiftCount` (temuan `BE-HRD-468`) | Pencarian di `src/` tidak menemukan pemakaian | Angka itu tidak tampil di layar mana pun, jadi selisihnya tidak terlihat pengguna (lihat catatan penutup) |

---

## 2. Proses bisnis dari sisi pengguna

Contoh dari desain dan data devYoga `ROST-202611-0002` (roster ICU Februari–Agustus 2027, terbit):

1. Kepala Unit ICU membuka **Matriks Roster Shift 24/7** dan memilih roster ICU. Pada 16 Februari, Pagi milik Indra sudah diganti ke Raka.
2. Di baris **Raka Firmansyah**, sel 16 Feb berisi "PA" dengan glif ⇄ di pojok kiri atas dan garis bawah putus-putus. Saat kursor diarahkan, keterangan berbunyi "Raka Firmansyah · 16 Feb 2027 · Pagi · Pengganti — menggantikan Indra Setiawan". Pembaca layar membacakan "PA · Pengganti".
3. Di baris **Indra Setiawan**, sel 16 Feb kosong ("–").
4. **Arif Maulana** tidak punya penugasan di roster ini tetapi memegang dua shift sebagai pengganti. Ia tampil di akhir daftar dengan label "Pengganti saja". Sel kosong di barisnya terkunci, karena baris itu bukan penugasan dan tidak bisa diberi shift baru atau dihapus. Sel bershiftnya tetap bisa dibuka untuk Ganti Petugas atau Hapus Shift.
5. Kepala Unit membuka sel Raka 16 Feb. Modal **Ganti Petugas Shift** menampilkan "Petugas Saat Ini: Raka Firmansyah" dan "Menggantikan: Indra Setiawan". Daftar pengganti tidak memuat Raka sendiri.
6. Setelah pengganti, kategori alasan, dan keterangan diisi, layar mengirim `expectedWorkforceProfileId` = Raka. Server menerima permintaan itu. Bila yang dikirim Indra, server akan menolaknya dengan `409`.

Legenda di atas kisi memuat entri "⇄ Pengganti: shift dipegang petugas pengganti (arahkan kursor ke sel untuk nama yang digantikan)".

Jalur tidak normal tetap seperti `FE-HRD-307`. `422` blokir/peringatan dan `409` "diubah orang lain" (dengan tombol Muat Ulang Roster) tampil dari pesan server.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; kartu `FE-HRD-411`; kontrak `api-contract.md` 37.2 dan `VR-PK-37`; desain `03-frontend-architecture.md` 36.3 dan 36.5; laporan `BE-HRD-463`, `BE-HRD-468`, `FE-HRD-410`; `RosterSchedulingDtos.cs` (`IsReplacementOnly`); view `roster-matrix-detail-view.jsx`; modal `QuickShiftReplacementModal`, `AddRosterShiftModal`, `ApplyShiftPatternModal`, `RemovePublishedShiftModal`, `RetroactiveShiftCorrectionModal`; `scheduling-display-utils.js`, `roster-shift-change-utils.js`; `roster-grid.module.css`; spec `hr-roster-published-change.spec.mjs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/hr/scheduling/roster-matrix-utils.js` (baru) | `buildRosterMatrixRows(assignments)` menempatkan setiap shift menurut `shiftAssignments[].workforceProfileId`. Shift tetap di baris asalnya bila petugasnya sama. Bila berbeda, shift pindah ke baris penugasan pertama petugas sekarang. Bila petugas itu tidak punya baris, dibuat baris pengganti-saja (urut nama, di akhir daftar). Shift tanpa ruas baru tetap di barisnya. Kunci baris unik (`pengganti-{workforceProfileId}` untuk baris ber-`id` kosong). Fungsi lain: `isReplacementShift` (hanya `isReplacement === true`), `describeReplacement` ("menggantikan {nama}", cadangan "petugas awal"), `buildShiftCell` (petugas sel = `shift.workforceProfileId`, cadangan petugas baris), `buildShiftCellTitle`, `countOwnAssignments`, `buildReplacementStaffList` (satu pilihan per petugas) |
| `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx` | Kisi dibangun dari `buildRosterMatrixRows`. Sel pengganti diberi kelas `shiftReplacement`, glif ⇄ (`aria-hidden`), teks tersembunyi " · Pengganti" (`visually-hidden`), `title` "… · Pengganti — menggantikan {nama}", dan `data-replacement`. Baris pengganti-saja diberi `StatusBadge` "Pengganti saja", `data-replacement-only`, dan sel kosong terkunci tanpa label "Tambah shift". `openCell` memakai `buildShiftCell`. Modal Ganti Petugas menerima pilihan unik per petugas. "Staf Terdaftar" (cadangan) dan syarat aktif Terapkan Pola memakai jumlah penugasan sungguhan. Legenda ditambah entri pengganti |
| `src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx` | `HrInfoGrid` menampilkan "Menggantikan: {nama}" bila sel pengganti. Payload tidak berubah: `expectedWorkforceProfileId` = `cell.workforceProfileId`, yang kini petugas sel |
| `src/components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx` | Baris `isReplacementOnly` disaring dari pilihan sasaran dan dari hitungan "Semua staf" |
| `src/style/hr/scheduling/roster-grid.module.css` | `.shiftReplacement` (garis bawah putus-putus) dan `.replacementMark` (glif di pojok kiri atas; pojok kanan atas dipakai titik konflik). Semua nilai memakai token |
| `tests/unit/hr-roster-matrix-utils.test.mjs` (baru) | 8 uji: AC1 (respons `BE-HRD-463` dan bentuk lama yang mengelompokkan per penugasan), AC2, AC3, AC4, respons lama, ketatnya `isReplacement`, pilihan pengganti unik |
| `tests/e2e/hr-roster-replacement-grid.spec.mjs` (baru) | 4 uji Playwright API tiruan: AC1+AC4, AC2 (termasuk Terapkan Pola), AC3, respons lama |

### 3.3 Kepatuhan arsitektur frontend

- Tidak ada service, slice Redux, atau hook baru. Detail roster tetap dimuat `useHrRemoteData` → `RosterSchedulingService.getRosterPeriodById` (respons diperluas, tanpa fungsi baru, sesuai `FE-HRD-410`).
- Logika pengelompokan berada di utilitas murni `src/utils/hr/scheduling/`, sejajar dengan `roster-shift-change-utils.js` dan `scheduling-display-utils.js`. Dengan begitu logika itu dapat diuji `node:test` tanpa browser.
- Penanda baris memakai `StatusBadge`; modal tetap `ConfirmModal` + `HrInfoGrid`. Kisi tetap elemen khusus (AC `FE-HRD-R38`). Gaya ditambahkan di `roster-grid.module.css` dengan token, tanpa warna literal.
- Teks tersembunyi memakai kelas Bootstrap `visually-hidden` yang sudah dimuat global (`src/app/layout.js`).

### 3.4 Delta terhadap kartu dan kontrak

| Hal | Kartu/kontrak | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| "tanpa aksi hapus penugasan" pada baris pengganti-saja | Kisi tidak pernah punya tombol hapus penugasan | Tidak ada tombol hapus penugasan, dan sel kosong baris itu juga dikunci (Tambah Shift tidak dibuka) | Tambah Shift memanggil `POST …/assignments`, yang membuat penugasan baru. Baris pengganti-saja tidak punya penugasan maupun jabatan (`positionId` kosong). Pemberian shift baru kepada petugas itu dilakukan lewat Ganti Petugas atau Muat Staf/penugasan biasa |
| Teks "PA · Pengganti" (AC1) | — | Tampil sebagai "PA" + glif ⇄. Teks "· Pengganti" dibacakan pembaca layar dan disebut di `title` serta legenda | Sel 52 px tidak muat teks "Pengganti". Bentuk tanda `DEV_DISCRETION` |
| Pengelompokan menurut `workforceProfileId` | Server (`BE-HRD-463`) sudah mengelompokkan | Frontend tetap mengelompokkan dengan aturan yang sama (idempoten terhadap respons baru) | Outcome kartu meminta penempatan menurut ruas shift. Respons dari server versi lain pun tetap menampilkan satu petugas sekali, tidak dua kali |
| `isReplacement` | API 37.2: "`true` bila shift hasil Ganti Petugas" | Tanda hanya dari `isReplacement === true` server, tanpa menurunkannya sendiri dari `originalWorkforceProfileId` | Mengikuti makna yang ditetapkan `BE-HRD-463` (shift yang dikembalikan ke petugas awal tidak bertanda) |
| "Menggantikan" di modal | Desain 36.3 menyebut keterangan di sel | Ditambahkan juga di modal Ganti Petugas | Pengguna yang memakai layar sentuh tidak melihat `title`; nama petugas awal tetap terbaca sebelum mengganti |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah: Hero "Memuat periode roster…", `SummaryGrid` loading |
| Kosong | Tidak berubah: "Belum ada staf pada periode ini…" bila tidak ada baris sama sekali |
| Gagal | Tidak berubah: `InformationAlert` pesan server / cadangan "Gagal memuat detail periode roster." |
| Tanpa hak akses | Tidak berubah: tanpa `RosterPeriod : Read` → peringatan; tanpa hak ubah → sel terkunci dan keterangan baca-saja (diuji ulang di spec `FE-HRD-307`) |
| Basi (`409`) | Tidak berubah: pesan server + Muat Ulang Roster. Kini jarang terjadi dari sel pengganti, karena yang dikirim petugas sekarang |
| Respons lama | Tanpa ruas baru → kisi sama seperti sebelum task ini (uji unit + E2E) |
| Aksesibilitas | Tanda pengganti tidak hanya warna: glif ⇄, garis bawah putus-putus, teks tersembunyi "Pengganti", `title` "menggantikan {nama}", label baris "Pengganti saja" (desain 36.5) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Scheduling Management / Roster Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}` | Detail roster; ruas baru `assignments[].isReplacementOnly`, `shiftAssignments[].workforceProfileId/Code/DisplayName`, `isReplacement`, `originalWorkforceProfileId`, `originalWorkforceDisplayName` | `RosterPeriod : Read` |

#### Corporate / Human Resource / Scheduling Management / Shift Replacement

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/replace` | Ganti Petugas; `expectedWorkforceProfileId` = petugas sekarang dari sel (`VR-PK-37`) | `ShiftReplacement : Update` |

Endpoint lain di layar ini (populate, copy, apply-pattern, validate, publish, close, assignments, remove shift, retroactive-correct) tidak berubah.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-roster-matrix-utils.test.mjs tests/unit/hr-roster-shift-change-utils.test.mjs` | 12/12 lulus (8 baru + 4 `FE-HRD-307`) | `PASS` | Keluaran perintah |
| Kontrol negatif unit: 8 mutasi pada utilitas (kelompok per baris asal — kontrol yang diminta kartu; baris pengganti-saja tidak dibuat; `isReplacement` longgar; `expected` = petugas baris; `title` tanpa tanda; kunci baris = `id`; hitung semua baris; pilihan tanpa dedupe) | 8/8 tertangkap; source pulih (hash `706655375da3` sama) | `PASS` | `mut411.cjs`, `mut411-hasil.log` di scratchpad sesi |
| E2E API tiruan `hr-roster-replacement-grid.spec.mjs` (server `next dev` 3711) | 4/4 lulus | `PASS` | Keluaran Playwright |
| Kontrol negatif E2E: 5 mutasi pada view/modal (teks tersembunyi dibuang; sel kosong pengganti-saja dapat ditambah; Terapkan Pola memuat pengganti-saja; modal tanpa "Menggantikan"; baris pengganti-saja dari server dibuang) | 5/5 tertangkap (1, 1, 1, 1, 3 uji gagal); ketiga berkas pulih (hash sama) | `PASS` | `mut411e2e.cjs`, `mut411e2e-hasil.log` |
| Regresi E2E roster: `hr-roster-published-change` (`FE-HRD-307`/`316`, 10 uji) + `hr-roster-revision-history` (`FE-HRD-321`, 8 uji) + spec baru | 22/22 lulus sesudah mutasi dipulihkan | `PASS` | `e2e411-akhir.log` |
| Ulang `UAT-PK-03`/`UAT-PK-16` data asli devYoga: backend lokal 7184 (DLL dengan `BE-HRD-463`), layar `next dev` 3711, roster `ROST-202611-0002`. GET diteruskan ke backend (cookie superadmin seed), izin disuapkan seperti Kepala Unit, POST Ganti Petugas dicegat dan tidak diteruskan | Lihat tabel di bawah | `PASS` | `uat411.mjs`, `uat411-hasil.log`, `uat411-kisi.png`, `uat411-modal.png`, `uat411-sesudah.png` |
| `npx eslint` atas enam berkas yang diubah/baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Tanpa galat | `PASS` | Keluaran perintah |
| `npm run build` | "Compiled successfully in 108s", 931 halaman statis, `prepare-standalone` siap | `PASS` | `build411.log` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` (suite penuh) | 5.453 lulus, 152 gagal dari 5.605. Jumlah gagal sama dengan baseline `FE-HRD-410` (152/5.589). Uji HR yang gagal adalah menu SDM (HR-01/02/05) dan payroll (`salaryStructures`), di luar berkas task ini | `EXISTING / ENVIRONMENT ISSUE` | `unit411.log` |

Hasil data asli devYoga (`ROST-202611-0002`):

| Pemeriksaan | Respons API | Layar |
| --- | --- | --- |
| Raka (punya penugasan) Pagi 16 Feb | `isReplacement = true`, asal Indra Setiawan | Sel "PA ⇄", nama aksesibel "PA · Pengganti", `title` "… · Pagi (UJI-A52) · Pengganti — menggantikan Indra Setiawan" |
| Arif (tanpa penugasan) | `isReplacementOnly = true`, `id` kosong, Malam 14 Feb (asal Bagas Saputra) dan Pagi 17 Feb (asal Indra Setiawan) | Baris terakhir berlabel "Pengganti saja"; dua sel bertanda dengan `title` menyebut Bagas Saputra dan Indra Setiawan; 210 dari 212 sel terkunci (sel kosong), 2 sel bershift dapat dibuka |
| Indra 14–18 Feb | 0 shift | Kelima sel "–" |
| Jumlah sel pengganti di kisi | 4 shift pengganti (termasuk Raka 15 Mar) | 4 sel bertanda |
| Staf Terdaftar | `totalAssignmentCount` = 3 | 3 (baris Arif tidak dihitung) |
| Modal Ganti Petugas dari sel Raka 16 Feb | — | "Petugas Saat Ini: Raka Firmansyah", "Menggantikan: Indra Setiawan"; pilihan pengganti Indra, Bagas, Arif (tanpa Raka) |
| Kiriman Ganti Petugas (dicegat) | — | `POST …/shift-replacements/285339b9…/replace`, shift sama dengan Pagi 16 Feb Raka, `expectedWorkforceProfileId` = Raka (bukan Indra). Tidak ada tulis lain |

**Uji manual:** dijalankan lewat Playwright terhadap data asli (bukan klik tangan): buka kisi, baca tanda dan `title`, buka sel pengganti, pilih pengganti dari daftar, isi keterangan, kirim. Tangkapan layar diperiksa visual. Pemeriksaan pertama menemukan glif ⇄ di pojok kiri bawah bertabrakan dengan garis bawah putus-putus. Glif dipindah ke pojok kiri atas, lalu uji layar diulang.

**Tidak dijalankan:** kiriman Ganti Petugas sungguhan ke devYoga, karena akan mengubah data uji `UJI-A52`. Penerimaan `expectedWorkforceProfileId` = Raka oleh server sudah dibuktikan `BE-HRD-463` (uji) dan penolakan `409` untuk Indra oleh `BE-HRD-468` (API lokal). Uji dengan akun Kepala Unit sungguhan juga tidak dijalankan; izin disuapkan seperti di `FE-HRD-307`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Data Indra → Raka: "PA · Pengganti" di baris Raka, sel Indra kosong pada tanggal itu | Terpenuhi | Uji unit AC1 (dua bentuk respons); E2E AC1 (`toHaveAccessibleName("PA · Pengganti")`, sel Indra "–"); data asli devYoga |
| 2. Baris pengganti-saja tampil dengan tanda dan tanpa tombol hapus penugasan | Terpenuhi | Uji unit AC2; E2E AC2 (label "Pengganti saja", baris terakhir, 6 sel kosong terkunci tanpa "Tambah shift", tanpa tombol hapus penugasan, tidak menjadi sasaran Terapkan Pola); data asli (baris Arif) |
| 3. Ganti Petugas dari sel Raka mengirim `expectedWorkforceProfileId` Raka | Terpenuhi | Uji unit AC3; E2E AC3 (body `expectedWorkforceProfileId: RAKA`); data asli (kiriman dicegat, Raka bukan Indra) |
| 4. Tanda pengganti terbaca tanpa warna | Terpenuhi | Glif ⇄ + garis bawah putus-putus (bentuk), teks tersembunyi "Pengganti", `title` "menggantikan {nama}", entri legenda; E2E AC1 memeriksa nama aksesibel, glif, `title`, dan `text-decoration-style: dashed` |
| Outcome: respons lama tanpa ruas baru tetap tampil seperti sebelumnya | Terpenuhi | Uji unit "Respons lama"; E2E respons lama (`title` sama persis dengan format lama, tanpa tanda, kiriman `expected` = petugas baris); 10 uji `FE-HRD-307` tetap lulus |
| DoD: AC terbukti, laporan `FE-HRD-411.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Suite unit penuh tetap memuat 152 kegagalan yang sudah ada di modul lain (sama dengan `FE-HRD-410`) |
| Masalah yang diketahui | (1) Satu sel per tanggal, sama seperti sebelumnya. Bila seorang petugas memegang dua shift pada tanggal yang sama (miliknya sendiri dan hasil ganti), hanya satu yang tampil. Peluangnya kini sedikit lebih besar karena shift pengganti masuk ke baris petugas sekarang. Server menandai dinas ganda sebagai peringatan W*, tetapi layar tidak menampilkan sel ganda. Usulan: kartu lanjutan bila Pengguna menghendaki. (2) `plannedShiftCount` (temuan `BE-HRD-468`) tidak dipakai layar mana pun, jadi angka "jumlah shift" yang tidak cocok tidak terlihat pengguna. Keputusan apakah angka itu dihitung dari shift yang tampil tetap catatan untuk pass blueprint |
| Dependency backend | Tidak ada yang tertunda. Status kontrak API 37.2 masih "Rencana (belum tersedia)"; penyegarannya dicatat untuk pass blueprint (artefak terkunci hash) |
| Perubahan sampingan | `NONE`. Playwright dijalankan dengan `--output` ke scratchpad; `test-results/` yang ter-track tidak berubah |
| Interupsi | Saat mulai, sesi lain sedang menjalankan E2E Lab dengan server standalone 3710. E2E task ini dijalankan lewat `next dev` 3711 (menulis ke `.next/dev`), dan build ditunda sampai sesi itu selesai. Sebelum build, server BE 7184 dan FE 3711 yang dinyalakan task ini dihentikan |
| Status Git | Berkas task ini: `M src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`, `M src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx`, `M src/components/view/hr/scheduling/modals/ApplyShiftPatternModal.jsx`, `M src/style/hr/scheduling/roster-grid.module.css`, `?? src/utils/hr/scheduling/roster-matrix-utils.js`, `?? tests/unit/hr-roster-matrix-utils.test.mjs`, `?? tests/e2e/hr-roster-replacement-grid.spec.mjs`. Perubahan `FE-HRD-410` dan Lab di working tree tidak disentuh. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-HRD-412` (tutup periode menyebut pengecualian yang diselesaikan), satu gelombang `MVP-78` |
