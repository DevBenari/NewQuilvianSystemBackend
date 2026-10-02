# Laporan Perubahan Frontend — `FE-RWI-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-063` |
| Judul | Kepala ruangan menambah dan mengakhiri dokter pendukung dari layar |
| Slice | Gelombang 1 — `PRD-RWI-V2-001`, `EPIC RI-39` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) bagian 4, kartu `FE-RWI-063` |
| Trace | `FR-RI-193`, `FR-RI-194`, `FR-RI-195`; `RWI-DEC-099`, `RWI-DEC-130`; `VAL-INP-01`, `VAL-INP-08`, `VAL-INP-09`; `INV-INP-12`; `contracts/api-contract.md` `0.9.0` bagian 10.2; `03-frontend-architecture.md` 12.3.1 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Wewenang UI | `FE-INP-21` — dialog pada panel Penugasan Dokter di Detail Episode `FE-INP-04`; tombol disembunyikan bagi selain kepala ruangan dan supervisor |
| Dependency | `BE-RWI-080` [BE] ✅ selesai 16 September 2026, dibuktikan laporan [`task/report/backend/BE-RWI-080.md`](../backend/BE-RWI-080.md) |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 10, berkas diubah 5 (source), kontrak API 2, UI/workflow 2 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan ini |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Commit frontend saat dikerjakan | `1ce219b40f8e411f3c4e66975626ab33ae81616a` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `df3679c0d5b2f08106702153eb242d3a6cb2929b` pada branch `HamzahV2` |
| Tanggal | 16 September 2026 |
| Status | ✅ **Selesai 16 September 2026.** Keenam acceptance criteria terbukti pada source dan komponen visual. `npm run lint:errors` bersih (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini, satu-satunya cara berinteraksi dengan dokter penanggung jawab pada layar **Detail Episode (`FE-INP-04`)** adalah melalui panel **"Alihkan DPJP"** (`POST /{id}/doctor-assignments`). Jalur ini menutup penugasan DPJP aktif dan membuka penggantinya.

Ketiadaan antarmuka untuk melibatkan dokter kedua menyebabkan kebutuhan klinis umum rumah sakit tidak dapat diakomodasi dari layar:
1. Melibatkan dokter konsulen (misalnya konsul kardiologi untuk pasien rawat inap penyakit dalam) tanpa memindahkan tanggung jawab utama pasien dari DPJP.
2. Memanggil dokter jaga shift untuk memantau pasien.
3. Memberikan jendela penugasan singkat penulisan catatan terlambat (`LateDocumentation`) bagi dokter yang tertinggal mengisi catatan medis shift sebelumnya (`INV-INP-12`).

Selain itu, riwayat penugasan dokter di layar sebelumnya hanya menampilkan nama dokter dan periode, tanpa membedakan peran dokter (DPJP vs Konsulen vs Dokter Jaga) maupun penanda penugasan singkat.

### 1.2 Bukti keadaan awal

1. Pada `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx`:
   - Panel penugasan hanya memiliki form Alihkan DPJP dan Tugaskan Perawat.
   - Tidak ada tombol "Tambah Dokter Pendukung" maupun tombol "Akhiri".
2. Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx`:
   - Hook hanya menyediakan `handleHandoverDoctor` dan `handleAssignNurse`.
   - Tidak ada fungsi pemanggilan endpoint `POST .../doctor-assignments/supporting` maupun `PATCH .../doctor-assignments/{assignmentId}/end`.
3. Pada `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx`:
   - `normalizeDoctorAssignments` hanya membaca `id`, `doctorId`, `doctorName`, `sequenceNumber`, `startDateTime`, `endDateTime`, `isCurrent`, dan `handoverReason`. Field `assignmentRole` dan `assignmentPurpose` dari kontrak `0.9.0` belum dipetakan.

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Kepala ruangan dan supervisor rawat inap (`actor?.isSupervisorOrWardHead`). Perawat pelaksana, dokter, dan staf admisi tidak diberikan akses untuk menambahkan atau mengakhiri dokter pendukung; tombol disembunyikan secara visual (`UAT-48`).

**Kapan layar dibuka.** Saat pasien rawat inap membutuhkan konsultasi spesialistik tambahan, pergantian dokter jaga ruangan, atau dokter jaga memerlukan jendela waktu singkat untuk mengisi rekam medis yang terlambat.

**Langkah berurutan — Pelibatan Konsulen / Dokter Jaga:**
1. Kepala ruangan membuka layar Detail Episode pasien (misalnya Tn. Budi).
2. Pada bagian **Penanggung Jawab**, kepala ruangan melihat panel **"Dokter Pendukung"** dan menekan tombol **"Tambah Dokter Pendukung"**.
3. Dialog `ConfirmModal` terbuka:
   - Pengguna memilih dokter dari daftar dokter aktif (`ResourceFilterSelect`).
   - Pengguna memilih tujuan: **Biasa**.
   - Pengguna memilih peran: **Konsulen** atau **Dokter Jaga**.
   - Waktu mulai otomatis diisi waktu sekarang; waktu selesai bersifat opsional.
   - Pengguna mengisi alasan pelibatan (misalnya "Konsultasi kardiologi untuk evaluasi aritmia").
4. Pengguna menekan tombol **Simpan**.
5. Sistem mengirim permintaan ke server. Setelah berhasil, dialog tertutup, notifikasi toast sukses muncul, dan daftar dokter pendukung menyegar otomatis (`refresh()`). **DPJP aktif tetap tidak berubah** (`INV-INP-12`).

**Langkah berurutan — Penugasan Singkat Penulisan Catatan Terlambat (`LateDocumentation`):**
1. Kepala ruangan memilih tujuan **"Penulisan catatan terlambat"**.
2. Form secara otomatis:
   - **Mengunci peran** ke **Dokter Jaga** (pilihan Konsulen dinonaktifkan).
   - Menampilkan isian **Waktu Selesai** sebagai isian **wajib**.
   - **Mematikan tombol Simpan** selama waktu selesai belum diisi (`AC-2`).
3. Pengguna mengisi waktu selesai (misalnya 1 jam ke depan) dan alasan.
4. Tombol Simpan aktif. Pengguna menekan Simpan. Waktu mulai diabaikan oleh backend dan selalu ditetapkan ke waktu saat ini untuk mencegah pemalsuan waktu rekam medis.

**Langkah berurutan — Mengakhiri Penugasan Pendukung:**
1. Pada kartu dokter pendukung yang sedang aktif, kepala ruangan menekan tombol **"Akhiri"**.
2. Baris DPJP dipastikan **tidak memiliki** tombol ini (`AC-3`).
3. Dialog konfirmasi muncul dengan opsi memasukkan alasan pengakhiran penugasan.
4. Pengguna menekan tombol konfirmasi **"Akhiri"**.
5. Sistem memanggil `PATCH .../doctor-assignments/{id}/end`, status penugasan berubah menjadi Selesai, dan daftar menyegar seketika.

**Jalur tidak normal dan penanganan galat:**
- Isian form kosong: Pesan validasi client-side langsung ditampilkan di bawah masing-masing field.
- Galat server (`400`, `409`, `422`): Ditampilkan apa adanya melalui komponen `InformationAlert` varian danger di dalam modal (`AC-5`), tanpa ditelan diam-diam.
- Penutupan modal: Isian form di-reset bersih untuk mencegah kebocoran data pada pembukaan berikutnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `contracts/api-contract.md` `0.9.0` bagian 10.2 | Spesifikasi payload request/response endpoint penugasan dokter pendukung |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/03-frontend-architecture.md` 12.3.1 | Wireframe dan spesifikasi fungsional layar `FE-INP-21` |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/task/report/backend/BE-RWI-080.md` | Bukti implementasi dan perilaku endpoint backend yang dikonsumsi |
| `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Layar detail episode induk tempat dialog dan panel penugasan ditambahkan |
| `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` | Hook orkestrasi state, validasi, dan komunikasi API detail episode |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx` | Menambahkan enum `INPATIENT_DOCTOR_ASSIGNMENT_ROLE`, `INPATIENT_DOCTOR_ASSIGNMENT_PURPOSE`, label Bahasa Indonesia, dan batas limit karakter alasan dokter pendukung |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Memperbarui `normalizeDoctorAssignments` untuk memetakan `assignmentRole`, `assignmentPurpose`, dan `reason`; menambahkan `validateSupportingDoctorForm`, `buildSupportingDoctorPayload`, dan `buildEndSupportingPayload` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx` | Menambahkan state modal & form dokter pendukung, handler penambahan & pengakhiran penugasan pendukung, proteksi klik ganda (*in-flight ref*), serta penanganan error response |
| `src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx` | Menambahkan panel Dokter Pendukung di bagian Penanggung Jawab, tombol Tambah Dokter Pendukung, tombol Akhiri pada baris pendukung aktif, pembaruan badge peran pada Riwayat Penugasan Dokter, serta dua dialog modal (`ConfirmModal`) |
| `src/style/health-services/inpatient-management/inpatient-episode-detail.module.css` | Menambahkan CSS module rules untuk kartu dokter pendukung, badge status, radio group, field datetime-local, dan layout dialog form |

### 3.3 Gerbang keputusan base component (`base-component-decision-gate.md`)

| Kebutuhan UI | Kandidat base | Bukti pemakaian | Status | Rekomendasi |
| --- | --- | --- | :---: | --- |
| Tombol "Tambah Dokter Pendukung" | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Gunakan `BaseButton` varian `secondary` ukuran `sm` dengan icon `FaUserPlus` |
| Dialog Form Tambah Dokter Pendukung | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Rangkai `ConfirmModal` dengan form controls sebagai `children` |
| Pemilihan Dokter Aktif | `ResourceFilterSelect` | `src/components/features/base-features/resource-filter-select.jsx` | `REUSE` | Gunakan resource filter `doctorSelect` (active only) |
| Isian Alasan | `BaseTextAreaField` | `src/components/features/base-features/base-form-control.jsx` | `REUSE` | Menggunakan field limit 500 karakter |
| Isian Waktu Selesai | Input datetime-local styled | CSS modul dengan design token | `REUSE` | Input tipe `datetime-local` dengan token warna/border Quilvian |
| Tampilan Pesan Galat Server | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Varian `danger` untuk menampilkan galat 400/409/422 apa adanya |
| Badge Peran & Tujuan | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Badge pill untuk Konsulen, Dokter Jaga, dan Penugasan Singkat |
| Tombol "Akhiri" Penugasan | `BaseButton` | `src/components/features/base-features/base-button.jsx` | `REUSE` | Varian `secondary` ukuran `sm` |
| Modal Konfirmasi Akhiri Penugasan | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | `ConfirmModal` dengan alasan opsional |

### 3.4 Checklist konsistensi visual (`ui-consistency-checklist.md`)

- [x] Tipografi menggunakan variabel desain global (`--font-size-body`, `--font-size-small`, `--font-weight-semibold`).
- [x] Spacing dan padding menggunakan token `--space-*`.
- [x] Radius sudut menggunakan token `--radius-*`.
- [x] Seluruh warna border, background, dan text menggunakan `--color-*`.
- [x] Tidak ada warna literal `#hex`, `rgb()`, atau font family hardcode di CSS.

---

## 4. Dokumentasi endpoint yang dikonsumsi

#### [Tags("Health Services / Inpatient Management / Inpatient Episode")]

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/supporting` | Kepala ruangan / supervisor melibatkan konsulen, dokter jaga, atau penugasan singkat | `InpatientEpisode : Update` + penjaga kepala ruangan/supervisor |
| `PATCH` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments/{assignmentId}/end` | Mengakhiri penugasan konsulen atau dokter jaga aktif | `InpatientEpisode : Update` + penjaga kepala ruangan/supervisor |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}/doctor-assignments` | Memuat riwayat penugasan dokter (membawa `assignmentRole` dan `assignmentPurpose`) | `InpatientEpisode : Read` |

---

## 5. Bukti verifikasi

### 5.1 Pemenuhan Acceptance Criteria

| ID | Acceptance Criteria | Status | Bukti Source |
| --- | --- | :---: | --- |
| **AC-1** | Dialog mengirim `doctorId`, `assignmentPurpose`, dan `reason` sesuai kontrak `0.9.0` API 10.2 | ✅ Terbukti | `buildSupportingDoctorPayload` memetakan form ke body JSON `POST .../doctor-assignments/supporting` sesuai kontrak |
| **AC-2** | Memilih tujuan `LateDocumentation` memunculkan isian waktu selesai, dan tombol simpan mati selama isian itu kosong | ✅ Terbukti | `supportingPurpose === 1` mengunci peran ke `OnCallDoctor` (3), mewajibkan `supportingEndDateTime`, dan `ConfirmModal` memiliki prop `disabled={supportingLoading \|\| (supportingPurpose === 1 && !supportingEndDateTime)}` |
| **AC-3** | Baris DPJP tidak punya tombol "Akhiri" | ✅ Terbukti | Tombol "Akhiri" hanya dirender untuk baris dengan `role !== INPATIENT_DOCTOR_ASSIGNMENT_ROLE.DPJP` (`doc.assignmentRole !== 1`) |
| **AC-4** | Perawat pelaksana tidak melihat tombol "Tambah Dokter Pendukung" — `UAT-48` | ✅ Terbukti | Tombol "Tambah Dokter Pendukung" dan tombol "Akhiri" dibungkus penjaga `{actor?.isSupervisorOrWardHead && assignmentAuthority.canManage ? ... : null}` |
| **AC-5** | Galat `422` dari server ditampilkan apa adanya kepada pemakai, bukan ditelan diam-diam | ✅ Terbukti | `getInpatientSettingErrorMessage` membaca pesan error respon server dan menampilkannya di dalam `InformationAlert` varian danger |
| **AC-6** | Daftar penugasan di panel menyegar sendiri setelah dialog berhasil | ✅ Terbukti | Pemanggilan `refresh()` pada blok `try` mutasi memicu kenaikan `reloadToken` dan memuat ulang `doctorAssignments` |

### 5.2 Hasil Uji Otomatis & Kompilasi

```text
AUTOMATED TEST: npm.cmd run lint:errors — PASS
Output:
> quilvian-app-system@0.1.0 lint:errors
> eslint . --quiet
(Exit code: 0, bersih tanpa error)
```

```text
AUTOMATED TEST: npm.cmd run build — PASS
Output:
> quilvian-app-system@0.1.0 build
> node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build

▲ Next.js 16.2.12 (Turbopack)
- Environments: .env

  Creating an optimized production build ...
✓ Compiled successfully in 45s
  Running TypeScript ...
  Finished TypeScript in 418ms ...
  Collecting page data using 15 workers ...
  Generating static pages using 15 workers (317/317) ...
✓ Generating static pages (317/317)
✓ Finalizing page optimization ...

> quilvian-app-system@0.1.0 postbuild
> node scripts/prepare-standalone.mjs

[prepare-standalone] Berhasil menyalin static assets.
[prepare-standalone] Berhasil menyalin public assets.
[prepare-standalone] Standalone runtime siap dijalankan.
(Exit code: 0, build berhasil)
```

### 5.3 Verifikasi Manual Kontrol Interaktif

1. **Uji Otorisasi Perawat Pelaksana (`UAT-48`):**
   - Saat login sebagai perawat biasa (`actor.isSupervisorOrWardHead === false`), tombol "Tambah Dokter Pendukung" dan tombol "Akhiri" tersembunyi sepenuhnya dari layar.
2. **Uji Otorisasi Kepala Ruangan / Supervisor:**
   - Tombol "Tambah Dokter Pendukung" muncul pada header panel Dokter Pendukung.
   - Baris DPJP aktif di kartu riwayat tidak menampilkan tombol "Akhiri".
   - Baris konsulen/dokter jaga aktif menampilkan tombol "Akhiri".
3. **Uji Skenario LateDocumentation:**
   - Memilih radio "Penulisan catatan terlambat" mengunci radio peran ke "Dokter Jaga".
   - Isian Waktu Selesai ditandai wajib (`*`).
   - Tombol "Simpan" tetap dalam kondisi nonaktif (disabled) hingga tanggal dan jam selesai dipilih.
4. **Uji Penanganan Error 422:**
   - Respons penolakan validasi bisnis dari backend (misal dokter tidak aktif) tampil dengan jelas pada kotak pesan merah di dalam dialog.

---

## 6. Status Git

Pemeriksaan `git status --short` pada repositori `QuilvianSystemFrontendDev`:

```text
 M src/components/view/health-services/inpatient-management/inpatient-episode-detail-view.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-detail.jsx
 M src/style/health-services/inpatient-management/inpatient-episode-detail.module.css
 M src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx
```

Perubahan hanya mencakup 5 file di atas. Tidak ada git commit, add, push, atau branch switch yang dijalankan secara otomatis.

---

## 7. Risiko dan langkah berikutnya

1. **Penjagaan Hak Akses di Server:**
   Menyembunyikan tombol di layar adalah perlindungan visual (`UAT-48`); penjagaan otentik sesungguhnya tetap berada pada pemeriksaan klaim peran di server melalui `BE-RWI-080`.
2. **Langkah Berikutnya:**
   - Menunggu penyelesaian task backend `BE-RWI-085` dan `BE-RWI-086` agar task frontend berikutnya (`FE-RWI-064` — Formulir Resume 8 Bagian) dapat dimulai.
