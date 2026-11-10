# Laporan Perubahan Frontend — `FE-RHB-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RHB-007` |
| Judul | Tujuan dan program terapi (layar FE-RHB-08), beserta bagian Tujuan dan Program di ruang kerja episode |
| Slice | FS-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/frontend-roadmap.md` — kartu `FE-RHB-007` (r2) |
| Trace | FR-RHB-010, 021, 022; FEAT-012, 013; RHB-DEC-011; UAT-06, UAT-07; `03-frontend-architecture.md` skema FE-RHB-08 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05. API: *Rehab Goal*, *Rehab Program* (`BE-RHB-011` ✅), *Rehab Lookup* `disciplines/{id}/procedures`, addendum RM |
| Dependency | `FE-RHB-005` ✅, `BE-RHB-011` ✅ |
| Repository | `QuilvianSystemFrontendDev` (lokal `V2QuilvianSystemFrontendDev`), branch `sukmagpV2`, SHA `0c06c0930`, belum di-commit |
| Model | Claude Opus 5.5 |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). ESLint: 0 error, 0 warning. Uji browser + jaringan Playwright: putaran 1 20/23, putaran ulang tanpa mengubah data 3/3. Ketiga FAIL putaran 1 berasal dari pola skrip yang peka huruf kapital, sedangkan `StatusBadge` menampilkan label berhuruf kapital tiap kata (§5). |

## 1. Masalah yang diselesaikan

Setelah asesmen final, terapis menyusun program terapi per layanan: tujuan yang ingin dicapai,
tindakan yang dikerjakan, frekuensi, dan tanggal evaluasi. Program baru boleh dipakai untuk sesi
setelah diaktifkan, dan aktivasi hanya berhasil bila prasyaratnya lengkap. Saat itu backend sudah
menolak aktivasi yang belum lengkap, tetapi belum ada layar yang memberi tahu terapis **apa** yang
kurang dan **di mana** melengkapinya.

**Contoh.** Terapis membuka episode `RHB-EPS-2026-000006`, lalu membuat tujuan "Jalan 100 m mandiri
dengan tongkat" untuk layanan *Uji BE-RHB-007*. Ia kemudian merevisinya: targetnya naik menjadi 150 m
dengan alasan "kemajuan lebih cepat". Ia menyusun program dengan memilih layanan dan kunjungan,
menyimpan draft, lalu menekan *Aktifkan*. Layar menampilkan empat prasyarat yang belum terpenuhi:
- asesmen profesi belum final → *Buka bagian Asesmen*;
- intervensi kosong → *Ke bagian Intervensi*;
- belum ada tujuan → *Ke bagian Tujuan*;
- jadwal belum lengkap → *Ke bagian Jadwal*.

Ia mengisi tujuan, intervensi, dan jadwal, lalu mengaktifkan lagi. Kali ini hanya asesmen profesi
yang tersisa. Setelah asesmen profesi final dan edukasi disetujui, aktivasi berhasil. Program menjadi
*Aktif*, dan episode berpindah ke *Terapi berjalan*. Revisi berikutnya membuat v2. Ketika v2
diaktifkan, v1 tercatat *Digantikan versi baru* di riwayat versi.

## 2. Wewenang dan keputusan

| Hal | Isi |
| --- | --- |
| Wewenang UI | Skema FE-RHB-08 `03-frontend-architecture.md`: layanan (terkunci), tujuan (pilih tujuan aktif atau buat baru), intervensi dari daftar tindakan layanan, frekuensi, rencana jumlah sesi, kewaspadaan, tanggal, alasan modifikasi untuk versi > 1. Tombol Simpan draft dan Aktifkan; 422 `RHB-VAL-040..045` sebagai daftar prasyarat bertaut. Pola satu halaman per versi, modal tujuan, dan menu baris tujuan adalah `DEV_DISCRETION`. |
| Izin owner | "saya approve dan lanjutkan" (2026-11-09): ESLint dan uji browser (backend + `next dev` + Playwright; akun uji dinonaktifkan sesudahnya). Tanpa build, tanpa commit. |
| Keputusan UI | Tidak ada elemen `NEW` atau `EXTEND` (§3.2) |
| Server uji | Port 3000 dan 7184 sudah dipakai `next dev` repo frontend dan `QuilvianSystemBackend.exe` dari `bin\Debug` yang menyala pukul 11:46, bukan dijalankan task ini. Keduanya dipakai apa adanya dan **tidak** dihentikan. |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

Semua jalur relatif terhadap `src/`.

| Berkas | Status | Isi |
| --- | --- | --- |
| `lib/services/health-services/rehabilitation-management/rehab-goal.service.js` | Baru | Daftar per episode (saringan layanan, `includeSuperseded`), buat, ubah, revisi, hentikan |
| `lib/services/.../rehab-program.service.js` | Diperbarui | Melengkapi berkas `FE-RHB-006`: rincian, buat, ubah, revisi, aktivasi, henti, batal |
| `lib/services/.../rehab-lookup.service.js` | Diperbarui | `getRehabLookupDisciplineProcedures(disciplineId)` |
| `lib/constants/.../programs/rehab-program-constants.js` | Baru | Status dan nada badge program/tujuan, pilihan jangka, batas panjang, peta prasyarat aktivasi (kode → isian atau bagian ruang kerja), isian dan kelompok form program, isian modal tujuan |
| `utils/.../programs/rehab-program-utils.js` | Baru | Pemetaan program ↔ form, payload, validasi draft (jumlah sesi, urutan tanggal, tindakan berulang, panjang), pemetaan galat bertingkat, pembaca daftar prasyarat (`errors.details`), form/payload/validasi tujuan |
| `lib/hooks/.../programs/use-rehab-goal-editor.js` | Baru | State modal tujuan (buat/revisi); dipakai form program dan ruang kerja |
| `lib/hooks/.../programs/use-rehab-program.js` | Baru | Controller halaman program: episode, program, riwayat versi, tujuan, tindakan layanan, addendum (jenis 48), simpan, aktivasi (simpan dulu bila berubah), batal/henti/revisi beralasan, tautan prasyarat |
| `components/features/.../programs/rehab-program-fields.jsx` | Baru | Pemilih tujuan, baris intervensi, daftar prasyarat aktivasi, modal tujuan, tabel riwayat versi |
| `components/features/.../programs/rehab-workspace-program-sections.jsx` | Baru | Bagian *Tujuan Terapi* (daftar, buat, revisi, hentikan) dan *Program Terapi* (daftar versi, susun) di ruang kerja |
| `components/view/.../programs/rehab-program-view.jsx` | Baru | Form draft (`BaseGroupedEditorView`) atau baca saja (`BaseDetailView`), modal aktivasi/batal/revisi/henti |
| `app/health-services/rehabilitation-management/episodes/[episodeId]/programs/[programId]/page.jsx` | Baru | Route tipis. Id selain `new`/UUID dijawab not-found. |
| `lib/hooks/.../episodes/use-rehab-episode-workspace.js` | Diperbarui | Memuat tujuan dan program; ditambah `openProgram` dan `refresh` |
| `components/view/.../episodes/rehab-episode-workspace-view.jsx` | Diperbarui | Memasang bagian Tujuan dan Program |
| `style/components/features/rehabilitation-management/rehab-assessment.module.css` | Diperbarui | Kelas tata letak `.interventionRow`, `.issueList`, `.issueItem` (token saja) |

### 3.2 Tabel keputusan base component (UI GATE)

| # | Elemen | Status | Komponen |
| --- | --- | --- | --- |
| 1 | Form layanan, kunjungan, jadwal, kewaspadaan | `REUSE` | `BaseGroupedEditorView` + `BaseGroupedEditorField` (select, number, `FilterDatePicker`) |
| 2 | Pemilih tujuan aktif | `COMPOSE` | `BaseCheckboxCard` per tujuan + `BaseButton` "+ Tujuan Baru" |
| 3 | Baris intervensi | `COMPOSE` | `BaseGroupedEditorField` select + text per baris (pola baris pengukuran `FE-RHB-005`) |
| 4 | Daftar prasyarat aktivasi bertaut | `COMPOSE` | `InformationAlert` (children) + `BaseButton` per butir |
| 5 | Modal tujuan buat/revisi | `COMPOSE` | `ConfirmModal` (children) + `BaseGroupedEditorField` + `BaseCheckboxCard` |
| 6 | Aksi beralasan (batal, henti, revisi program, henti tujuan) | `REUSE` | `ConfirmModal` `requireReason` |
| 7 | Baca saja + tabel tujuan, intervensi, riwayat versi | `REUSE`/`COMPOSE` | `BaseDetailView`, `BaseDetailSection` + `DataTable` |
| 8 | Status program, keutuhan RM, telaah, persetujuan, episode | `REUSE` | `StatusBadge` |
| 9 | Addendum | `REUSE` | `RehabDocumentAddendumPanel` (`FE-RHB-005`) |
| 10 | Menu aksi baris tujuan | `REUSE` | `RowActionMenu` |
| 11 | Bagian Tujuan dan Program di ruang kerja | `COMPOSE` | `BaseDetailSection` + `DataTable` + `BaseButton` |

**Pilihan untuk elemen `COMPOSE` (direkomendasikan dan dipakai):**

1. **Rakit dari base component yang ada** *(direkomendasikan)*. Tampilannya sama dengan layar rehab
   lain, regresi nol karena tidak ada base component yang disentuh, dan biayanya rendah.
2. Membuat komponen khusus (misalnya editor intervensi atau modal tujuan sendiri). Fleksibel, tetapi
   menambah gaya visual paralel dan butuh persetujuan `NEW`.

`UI GATE: PASS — 0 NEW, 0 EXTEND, 0 WRAP, 6 COMPOSE (opsi 1).`

### 3.3 Delta terhadap desain

| Delta | Rincian |
| --- | --- |
| Tautan prasyarat | 040/041 → ruang kerja `#rehab-episode-assessments`; 045 → `#rehab-episode-education`; 042/043/044 → gulir ke isian Intervensi/Tujuan/Frekuensi di form yang sama |
| Aktivasi pertama ditolak | Draft tetap ada; URL dipindah lewat `history.replaceState` agar daftar prasyarat tetap tampil. Simpan berikutnya memakai `PUT`, ditentukan oleh `detail.id` (pola `FE-RHB-006`). |
| Ganti layanan pada draft baru | Pilihan intervensi dan tujuan dikosongkan, karena keduanya milik layanan |
| Tujuan di ruang kerja | Modal menampilkan pilihan Layanan (kosong = tujuan episode untuk semua layanan). Di form program, layanan mengikuti program. |
| Revisi program | Server menyalin intervensi dan tujuan aktif ke draft baru; layar langsung membuka draft itu |
| Tombol izin | *Aktifkan* butuh `RehabProgram : Activate`; *Revisi* butuh `RehabProgram : Create`; *Hentikan* butuh `RehabProgram : Stop`; *+ Tujuan Baru* butuh `RehabGoal : Create`. Semuanya ditambah aksi status dari server (`availableActions`). |
| "Mulai Sesi" (AC 4) | Belum digambar, karena rute FE-RHB-11 baru dibuat `FE-RHB-008`. Tombolnya dipasang task itu dengan syarat `RehabSession : Create`. |
| Waktu tanggal | Isian tanggal memakai `FilterDatePicker` (format `yyyy-MM-dd`, sesuai `DateOnly` server) |

## 4. Verifikasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint atas folder rehab `view`, `features`, `hooks` (programs, episodes), `constants`/`utils` programs, `services`, `app/episodes` | 0 error, 0 warning |
| Grep anti-regresi UI di berkas baru (hex/`rgba`, `style={{`, `<button>`/`<table>` mentah) | 0. CSS baru hanya tata letak dengan token `globals.css` (`--space-2/3/4`, `--color-border`). |
| Grep privasi (`console`, `localStorage`) | 0 |
| Review diff | 4 berkas existing berubah (lookup service, program service milik `FE-RHB-006`, hook + view ruang kerja) dan 1 CSS rehab; sisanya berkas baru di folder rehab. Base component tidak berubah. |

AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; perilaku dibuktikan lewat uji browser + jaringan.

## 5. Uji browser dan jaringan (MANUAL TEST)

**Lingkungan (2026-11-09):**
- backend Debug di `https://localhost:7184` dengan DB dev `QuilvianNewDevSukma`;
- `next dev` di port 3000;
- Playwright Chromium dijalankan dari scratchpad.

**Data uji:**
- episode `RHB-EPS-2026-000006` (*Perencanaan*; asesmen medis final, layanan A dan B diresepkan);
- layanan uji A diaktifkan sementara;
- akun uji berprofesi Dokter Umum, dengan kebijakan seluruh aksi *RehabEpisode*, *RehabLookup*, *RehabGoal*, *RehabProgram*, *RehabEducation*, *RehabDisciplineAssessment*, *RehabMedicalAssessment*, dan *ClinicalNoteAddendum* (25 aksi).

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Ruang kerja *Perencanaan*: tombol *+ Tujuan Terapi* dan *+ Program Terapi* | PASS |
| 2 | **AC4**: tidak ada tombol *Mulai Sesi* di ruang kerja maupun di program aktif | PASS ×2 |
| 3 | Modal tujuan: tanggal target kosong → ditahan "Wajib diisi" tanpa `POST` | PASS |
| 4 | Tujuan baru dari ruang kerja (layanan A, disepakati pasien) → `POST` 201, tampil di daftar | PASS |
| 5 | Revisi tujuan lewat menu baris: alasan wajib di modal; `POST revise` 201; versi baru target 150 m, versi lama *Digantikan* | PASS |
| 6 | Form program memuat bagian Layanan, Tujuan, Intervensi, Jadwal dan Kewaspadaan | PASS |
| 7 | Draft kosong (layanan + kunjungan) → `POST` 201, URL id program | PASS |
| 8 | **UAT-06 / AC1**: *Aktifkan* → 422 dengan 4 prasyarat (asesmen profesi, intervensi, tujuan, jadwal), masing-masing bertaut | PASS |
| 9 | Tautan *Ke bagian Jadwal* menggulir ke isian Frekuensi | PASS |
| 10 | Pemilih tujuan hanya memuat tujuan aktif layanan A (versi revisi 150 m, bukan versi lama) | PASS |
| 11 | Setelah tujuan, intervensi, dan jadwal dilengkapi: *Aktifkan* → `PUT` 200 lalu 422 dengan satu prasyarat (asesmen profesi) | PASS |
| 12 | Tautan *Buka bagian Asesmen* → ruang kerja `#rehab-episode-assessments` | PASS |
| 13 | Setup API (akun uji): asesmen profesi A final + edukasi program dengan persetujuan *Setuju* final | PASS |
| 14 | **UAT-07 / AC2**: *Aktifkan* → 200; baca saja *Program v1: Aktif*, *Rekam Medis: Ditandatangani*, *Persetujuan terakhir: Setuju*; episode *Terapi berjalan* | PASS (putaran ulang + tangkapan layar) |
| 15 | Baca saja: tujuan, intervensi tampil, tanpa isian aktif | PASS (putaran ulang + tangkapan layar) |
| 16 | Program aktif: tombol *Revisi Program* dan *Hentikan Program* | PASS |
| 17 | **AC3**: *Revisi Program* (alasan) → draft v2; alasan modifikasi tampil; riwayat versi v2 Draf + v1 Aktif | PASS |
| 18 | Draft v2 menyalin intervensi dan tujuan aktif v1 | PASS |
| 19 | **AC3**: v2 diaktifkan → v2 Aktif, v1 *Digantikan versi baru* | PASS |
| 20 | Klik baris v1 di riwayat → v1 baca saja, *Digantikan versi baru*, tanpa Revisi/Hentikan | PASS (putaran ulang) |
| 21 | Ruang kerja: bagian Program memuat v2 dan v1; episode *Terapi berjalan* | PASS |
| 22 | Hentikan tujuan (menu baris + alasan) → 200, status *Dihentikan* | PASS |
| 23 | Id program bukan UUID → halaman not-found aplikasi | PASS |

**Kegagalan putaran 1** (#14, #15, #20) — salah ukur skrip. Pola skrip mencari "Program v1: Aktif"
dan "Persetujuan terakhir", sedangkan `StatusBadge` menampilkan "Program V1: Aktif" dan "Persetujuan
Terakhir: Setuju". Tangkapan layar `fe007-active.png` memperlihatkan isi yang benar. Putaran ulang
dengan pola tanpa membedakan huruf kapital, tanpa mengubah data, lulus 3/3.

**Pembersihan.** Dicek di DB dev dengan kueri READ ONLY:
- 0 dari 32 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pada pasangan uji: 0 baris;
- layanan uji aktif: 0.

**Residu data dev** di `RHB-EPS-2026-000006`, dibiarkan karena berguna sebagai data uji `FE-RHB-008`/`010`:
- episode berstatus *Terapi berjalan*;
- program A v1 *Digantikan*, v2 *Aktif*;
- tujuan: 1 aktif, 1 digantikan, 1 dihentikan;
- 1 asesmen profesi A final;
- 1 edukasi program *Setuju* final.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-06: daftar prasyarat bertaut ke bagian terkait | Terpenuhi | §5 #8, #9, #11, #12 |
| (2) UAT-07 | Terpenuhi | §5 #13–15 |
| (3) Riwayat versi program tampil | Terpenuhi | §5 #17, #19, #20 |
| (4) "Mulai Sesi" hanya tampil bila rutenya ada dan pengguna punya `RehabSession:Create` | Terpenuhi (rute belum ada → tidak tampil) | §5 #2; tombol dipasang `FE-RHB-008` |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| `FE-RHB-008` | Wajib memasang tombol *Mulai Sesi* di program aktif (AC 4 task ini) dengan syarat `RehabSession : Create` |
| Label badge berhuruf kapital | `StatusBadge` mengubah label menjadi huruf kapital tiap kata ("Terapi Berjalan"). Bawaan base component, tidak diubah. |
| Prasyarat 045 | Edukasi *Menolak* untuk program memblokir aktivasi. Tautannya menuju bagian Edukasi; jalur ini teruji di API `BE-RHB-011`, belum lewat layar. |
| Dependency backend | Tidak ada yang baru |
