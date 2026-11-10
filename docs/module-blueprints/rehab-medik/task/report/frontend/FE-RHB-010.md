# Laporan Perubahan Frontend — `FE-RHB-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-010` |
| Judul | Form reasesmen (FE-RHB-12): pengukuran kini berdampingan dengan baseline, alasan pergantian instrumen, capaian per tujuan, keputusan |
| Slice | FS-E — Reasesmen dan ringkasan akhir |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-010` (r2) |
| Trace | FR-RHB-040, 041; FEAT-024, 025; V-MEA-02; `03-frontend-architecture.md` skema FE-RHB-12 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Progress Review* (`BE-RHB-014` ✅); baca *Rehab Program*, *Rehab Goal*, asesmen (baseline), *Rehab Lookup* `/instruments` |
| Dependency | `FE-RHB-007` ✅, `BE-RHB-014` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright: 12/13. Satu FAIL berasal dari pola skrip yang membaca label berhuruf kapital (CSS), dan data membuktikan butir itu benar (§5). |

## 1. Masalah yang diselesaikan

Di tengah program terapi, terapis atau dokter rehab mengevaluasi kemajuan. Mereka mengukur ulang,
membandingkannya dengan baseline asesmen awal, menilai setiap tujuan, lalu memutuskan apakah program
dilanjutkan, diubah, dihentikan, dirujuk, atau diselesaikan. Bila alat atau satuan ukurnya berbeda dari
baseline, perbandingannya tidak sepadan, sehingga alasannya wajib dicatat. Keputusan *Hentikan* langsung
mengubah status program.

**Contoh.** Terapis membuka reasesmen layanan *Uji BE-RHB-007 · program v2* di episode
`RHB-EPS-2026-000006`. Layar menampilkan baseline asesmen awal "Jarak tempuh 6 menit — 200 meter", dan
baris pengukuran sudah terisi nama dan satuan yang sama. Pasien kini memakai walker, sehingga terapis
mencatat 250 **langkah**. Saat disimpan, server menolak: "Instrumen berbeda dari baseline. Jelaskan
alasannya." Isian *Alasan Pergantian Instrumen* tersorot, dan peringatan menyebut "Jarak tempuh 6 menit".

Setelah alasan diisi, draft tersimpan. Tabel pembanding menampilkan 200 meter → 250 langkah dengan badge
*Instrumen berbeda*. Terapis menilai tujuan *Tercapai sebagian*, memutuskan *Hentikan*, dan
menandatanganinya; modal tanda tangan menyebut "Program akan dihentikan." Halaman langsung menampilkan
*Program v2: Dihentikan*, dan ruang kerja episode pun memperlihatkannya tanpa muat ulang manual.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-12: layanan/program; pengukuran kini berdampingan dengan baseline (instrumen dan satuan sama); alasan pergantian instrumen; capaian per tujuan; ringkasan; hambatan; keputusan + alasan; tujuan rujukan. Satu halaman per reasesmen dan tabel pembanding adalah `DEV_DISCRETION`. |
| Izin owner | "lanjutkan" (2026-11-09), batas sama: ESLint dan uji browser (akun uji dinonaktifkan sesudahnya). Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND` (§3.2) |
| Server uji | Memakai backend dan `next dev` yang sudah berjalan sejak 11:46 (bukan dijalankan task ini); tidak dihentikan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-progress-review.service.js` | Baru | Daftar per episode, rincian, buat, ubah, tanda tangan, batal |
| `lib/constants/.../reviews/rehab-review-constants.js` | Baru | Pilihan keputusan dan dampaknya, pilihan capaian tujuan (label sama dengan server), batas panjang, isian dan kelompok form |
| `utils/.../reviews/rehab-review-utils.js` | Baru | Pemetaan rincian ↔ form, baris dari baseline, payload (`scope` = layanan + program aktif, atau reasesmen medis), validasi draft dan tanda tangan, pemetaan galat, pembaca daftar instrumen berbeda (`errors.details`) |
| `utils/.../assessments/rehab-assessment-utils.js` | Diperbarui | Pembentukan daftar pengukuran diekstrak ke `buildMeasurementsPayload` agar dipakai bersama asesmen dan reasesmen. Perilaku asesmen tidak berubah. |
| `lib/hooks/.../reviews/use-rehab-review.js` | Baru | Controller halaman: episode, program, tujuan, baseline (asesmen awal final terbaru), instrumen, addendum (jenis 50), simpan, tanda tangan, batal |
| `components/features/.../reviews/rehab-review-fields.jsx` | Baru | Tabel pengukuran kini–baseline (+ peringatan instrumen berbeda), capaian per tujuan |
| `components/view/.../reviews/rehab-review-view.jsx` | Baru | Form (`BaseGroupedEditorView`) atau baca saja (`BaseDetailView`) + modal tanda tangan (dengan dampak keputusan) dan batal |
| `app/health-services/rehabilitation-management/episodes/[episodeId]/reviews/[reviewId]/page.jsx` | Baru | Route tipis; selain `new`/UUID dijawab not-found |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js`, `components/view/.../episodes/rehab-episode-workspace-view.jsx`, `components/features/.../programs/rehab-workspace-program-sections.jsx` | Diperbarui | Bagian *Reasesmen* di ruang kerja: daftar dan tombol *+ Reasesmen*. Tombol hanya tampil bila episode memuat aksi `create-progress-review`. |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Form layanan, kunjungan, teks, keputusan | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` |
| 2 | Pengukuran terstruktur dan instrumen | `REUSE` | `RehabAssessmentMeasurements`, `RehabAssessmentInstruments` (`FE-RHB-005`, bungkus renderer keperawatan) |
| 3 | Pengukuran kini berdampingan dengan baseline | `COMPOSE` | `DataTable` + `StatusBadge` (*Sepadan* / *Instrumen berbeda*) + `InformationAlert` (`RHB-VAL-036`) |
| 4 | Capaian per tujuan | `COMPOSE` | `BaseGroupedEditorField` select + text per tujuan |
| 5 | Tanda tangan dan batal | `REUSE` | `ConfirmModal` |
| 6 | Baca saja + addendum | `REUSE`/`COMPOSE` | `BaseDetailView`, `BaseDetailSection` + `DataTable`, `RehabDocumentAddendumPanel` |
| 7 | Bagian Reasesmen di ruang kerja | `COMPOSE` | `BaseDetailSection` + `DataTable` + `BaseButton` |

**Pilihan untuk elemen `COMPOSE` (direkomendasikan dan dipakai):**

1. **Rakit dari base component yang ada** *(direkomendasikan)*. Tampilan konsisten, tanpa regresi base
   component, biaya rendah.
2. Membuat komponen pembanding khusus (grafik/kartu berdampingan). Lebih kaya, tetapi menambah gaya
   paralel dan butuh persetujuan `NEW`.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 3 COMPOSE (opsi 1).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Berdampingan dengan baseline | Sebelum disimpan, layar menampilkan daftar baseline asesmen awal final terbaru, dan baris terstruktur baru diisi dari baseline (nama, bagian tubuh, satuan). Sesudah disimpan, tabel memakai pembanding resmi server (`comparisons`), yang juga memperhitungkan reasesmen sebelumnya. |
| `RHB-VAL-036` | Ditolak server saat simpan atau tanda tangan. Layar menyorot isian alasan (`fields`) dan menyebut pengukuran yang berbeda (`errors.details`); peringatan hilang begitu alasan mulai diisi. |
| Jenis reasesmen | Pilihan: layanan yang punya program aktif (program ikut otomatis) atau *Reasesmen medis (tanpa program)*. Tidak dapat diganti setelah draft dibuat. |
| Tujuan dinilai | Tujuan aktif layanan itu dan tujuan episode tanpa layanan; tujuan yang belum dinilai tidak dikirim |
| Dampak keputusan | Modal tanda tangan menyebut dampaknya (dihentikan / ditandai perlu telaah / selesai / dirujuk). Rincian hasil tanda tangan memuat status program terbaru dan ditampilkan langsung. |
| Ruang kerja | Dimuat ulang setiap kali dibuka. Kembali dari reasesmen langsung memperlihatkan status program baru (AC 2). |
| Tujuan rujukan | Isian hanya tampil dan wajib bila keputusan *Rujuk* |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas folder rehab `view`, `features`, `hooks` (reviews, episodes, assessments), `constants` reviews, seluruh `utils` rehab, `services`, `app` | 0 error, 0 warning |
| Grep anti-regresi UI di berkas baru (hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. Tidak ada CSS baru. |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | Berkas existing yang berubah: utils asesmen (ekstraksi fungsi, perilaku sama), hook + view + komponen bagian ruang kerja. Base component tidak berubah. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium.

**Data uji:**
- program *Uji BE-RHB-007 v2* aktif di `RHB-EPS-2026-000006`;
- baseline asesmen profesi "Jarak tempuh 6 menit — 200 meter";
- akun uji Dokter Umum, dengan kebijakan *RehabEpisode*, *RehabLookup*, *RehabProgressReview*, *ClinicalNoteAddendum*, ditambah *Read* untuk program, tujuan, dan asesmen (13 aksi).

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Ruang kerja: tombol *+ Reasesmen* dan bagian Reasesmen | PASS |
| 2 | Pilihan jenis: "Uji BE-RHB-007 · program v2" dan "Reasesmen medis (tanpa program)" | PASS |
| 3 | Baseline asesmen awal tampil (Jarak tempuh 6 menit, 200 meter); baris pengukuran diisi nama + satuan dari baseline | PASS |
| 4 | Capaian tujuan memuat tujuan aktif layanan A | FAIL skrip — lihat catatan |
| 5 | **AC1**: 250 *langkah* → 422 `RHB-VAL-036`; isian *Alasan Pergantian Instrumen* tersorot (kelas invalid + pesan); peringatan menyebut "Jarak tempuh 6 menit"; tetap di halaman `new` | PASS |
| 6 | Dengan alasan → draft 201; pembanding server: baseline 200 meter · kini 250 langkah · *Instrumen berbeda* | PASS |
| 7 | Tanda tangan tanpa ringkasan/keputusan → ditahan di layar, tanpa request | PASS |
| 8 | Keputusan *Hentikan*: modal menyebut "Program akan dihentikan."; `PUT` 200 + `sign` 200 | PASS |
| 9 | **AC2**: sesudah tanda tangan, halaman langsung menampilkan *Program v2: Dihentikan*, *Reasesmen: Final*, *Keputusan: Hentikan* | PASS |
| 10 | Baca saja: pembanding + capaian *Tercapai sebagian*, tanpa isian aktif | PASS |
| 11 | **AC2**: *Kembali ke Episode* → ruang kerja menampilkan program v2 *Dihentikan* dan reasesmen *Hentikan*, tanpa muat ulang manual | PASS |
| 12 | Id reasesmen bukan UUID → halaman not-found aplikasi | PASS |

**Catatan #4 — salah ukur skrip.** Label isian dirender berhuruf kapital lewat CSS, sehingga `innerText`
membaca "JALAN 100 M MANDIRI…" dan pola skrip tidak cocok. Tiga bukti menunjukkan daftar tujuan memang
ada:
- langkah berikutnya berhasil memilih baris tujuan pertama dan mengisi *Tercapai sebagian*;
- halaman baca saja (#10) menampilkannya;
- kueri DB READ ONLY mencatat 1 tujuan dinilai pada reasesmen itu.

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 37 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

**Residu data dev:** `RHB-EPS-2026-000006` kini punya 1 reasesmen Final (keputusan *Hentikan*, 1 tujuan
dinilai), dan program v2 berstatus *Dihentikan*. Episode tetap *Terapi berjalan* karena aturan backend
tidak mengubahnya.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) 422 `RHB-VAL-036` menyorot isian alasan | Terpenuhi | §5 #5 |
| (2) Keputusan `Stop` memperbarui status program di ruang kerja tanpa muat ulang manual | Terpenuhi | §5 #9, #11 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Baseline layar sebelum simpan | Daftar baseline sebelum simpan diambil dari asesmen awal. Pembanding resmi sesudah simpan dihitung server dan dapat memakai reasesmen sebelumnya. |
| Instrumen berversi | Jalur renderer berisi instrumen menunggu instrumen rehab disahkan (sama dengan `FE-RHB-005`) |
| Data uji | Program v2 `RHB-EPS-2026-000006` kini dihentikan. Uji berikutnya yang butuh program aktif memakai episode lain atau revisi program. |
| Dependency backend | Tidak ada yang baru |
