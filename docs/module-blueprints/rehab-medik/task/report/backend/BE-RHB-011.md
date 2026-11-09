# Laporan Perubahan Backend — `BE-RHB-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-011` |
| Judul | Tujuan dan program berversi |
| Slice | S-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-011` |
| Trace | FR-RHB-010, 021, 022; FEAT-012, 013; RHB-DEC-005, 011; RHB-INV-03, 10; `state-transition-matrix.md` §4, §8; V-PRG-01..06, V-GOL-01/02 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Goal* dan *Rehab Program* |
| Dependency | `BE-RHB-009` ✅, `BE-RHB-010` ✅ (2026-11-06) |
| Klasifikasi | `STANDARD` — endpoint transaksi baru, tanpa schema baru, memakai mesin keutuhan RM dan advisory lock |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). Build owner `PASS` (0 error, 0 warning di folder Rehab), QBE Strict `PASS`. Uji API manual 42/42. Bukti DB cocok (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru; `RhbGoal`, `RhbProgram`, `RhbProgramIntervention`, `RhbProgramGoal` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi.** Tujuan: sub-proses ter-scope episode (`revise`, `discontinue`). Program: aggregate ber-lifecycle ter-scope episode (`revise`, `activate`, `stop`, `cancel`). Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.Goal`/`TherapyProgram`), aturan transaksi gateway RM |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` jenis 48 (persetujuan RM `RHB-OQ-012` ✅). Baca saja: `MstProcedure`, `RegPatientEncounter`. |
| Penomoran | `ProgramVersion` adalah nomor urut versi dalam satu (episode, layanan), bukan nomor bisnis. Dihitung di bawah advisory lock `RHB_PROGRAM:{episode}:{layanan}` (pola HD, sesuai kartu) dan dijaga indeks unik `UX_RhbProgram_Version`. |

## 1. Masalah yang diperbaiki

Setelah asesmen dan edukasi, terapis perlu menyusun tujuan dan program latihan yang terkunci saat
mulai dijalankan. Bila rencana berubah, versi lamanya tetap tersimpan, dan setiap layanan hanya boleh
punya satu program yang berlaku.

**Contoh.** Terapis layanan A menulis tujuan "jalan mandiri 50 m dengan walker", lalu menyusun draft
program v1: 3× seminggu, dua intervensi, evaluasi 14 hari lagi. Keluarga sempat menolak rencana, dan
aktivasi ditolak `RHB-VAL-045`. Setelah edukasi ulang disetujui:
- program v1 diaktifkan, dan episode pindah ke *Terapi berjalan*;
- seminggu kemudian target dinaikkan menjadi 75 m: tujuan direvisi (G1 → G2) dan program direvisi (v1 → v2);
- saat v2 diaktifkan, v1 otomatis menjadi *Digantikan versi baru*.

## 2. Proses bisnis

**Tujuan**
1. Dibuat saat episode *Perencanaan* atau *Terapi berjalan* (409 bila belum diasesmen). Layanan opsional, tetapi bila diisi harus sudah diresepkan (`RHB-VAL-032`).
2. Jangka, deskripsi, baseline, target, dan tanggal target wajib; tanggal target tidak lampau (V-GOL-02, `RHB-VAL-047`).
3. Ubah hanya oleh penulis, dan ditolak 409 `RHB-VAL-046` bila dirujuk program aktif (V-GOL-01).
4. Revisi: versi lama *Superseded*, versi baru *Aktif*. Hentikan: *Dihentikan*. Keduanya wajib beralasan, dan alasannya dicatat di riwayat status (agregat `Goal`).

**Program**
1. **Draft.**
   - Layanan harus sudah diresepkan (`RHB-VAL-032`), dan profesi akun harus sesuai layanan (`RHB-VAL-031`).
   - Kunjungan penyusunan milik pasien dan masih terbuka (`RHB-VAL-081`).
   - Hanya satu draft atau program aktif per layanan (409); untuk perubahan, gunakan revisi.
   - Draft didaftarkan ke Rekam Medis jenis 48.
   - Data yang salah ditolak sejak draft: tindakan di luar daftar layanan (042), tujuan di luar episode/layanan atau tidak aktif (043), tanggal evaluasi sebelum mulai (044).
2. **Ubah dan batal draft** hanya oleh penulis berprofesi sesuai, dengan `expectedVersion`. Batal wajib beralasan, dan Rekam Medis menandai dokumen *Dibatalkan*.
3. **Aktivasi** (butir `Activate`, oleh penulis, karena Rekam Medis hanya menerima tanda tangan penulis).
   - Seluruh prasyarat V-PRG-01..06 dikumpulkan dalam satu jawaban 422 beserta daftar kodenya.
   - Dalam satu transaksi: versi aktif lama → *Superseded* (disimpan lebih dulu), program → *Aktif*, episode *Perencanaan* → *Terapi berjalan*, riwayat status, lalu tanda tangan Rekam Medis.
4. **Revisi** (butir `Create`, profesi sesuai, alasan wajib). Membuat draft vN+1 yang menyalin isian, intervensi, dan tujuan yang masih aktif. Versi lama tetap aktif sampai versi baru diaktifkan.
5. **Hentikan** (butir `Stop`, alasan wajib). Hanya untuk program aktif; tanpa syarat profesi, karena Sp.KFR juga memegang butir ini.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabGoalController.cs` | Baru | 5 endpoint, Resource `RehabGoal`, aksi `Read`/`Create`/`Update` |
| `Controllers/RehabProgramController.cs` | Baru | 8 endpoint, Resource `RehabProgram`, aksi `Read`/`Create`/`Update`/`Activate`/`Stop` |
| `Services/RhbGoalService.cs` | Baru | Aturan tujuan §2 |
| `Services/RhbProgramService.cs` | Baru | Aturan program §2, prasyarat aktivasi, salin revisi, rincian dengan persetujuan terakhir |
| `DTOs/RehabProgramDtos.cs` | Baru | DTO tujuan dan program, termasuk `RehabActivationIssue` |
| `Services/RhbStatusHistoryWriter.cs` | Diperbarui | Konstanta agregat `Goal` |
| `Program.cs` | Diperbarui | `AddScoped<RhbGoalService>()`, `AddScoped<RhbProgramService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Dua controller baru di Akses Role. `Activate`/`Stop` bertipe `Update`. Tidak ada nama peran di kode. Profesi dan kepenulisan adalah kewenangan yang melekat pada data. |
| Logger | id, episode, layanan, versi, status. Isi tujuan, parameter intervensi, dan alasan tidak dicatat. |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| Jawaban 422 aktivasi | `errors.code` = kode prasyarat pertama yang gagal; `errors.details` = daftar lengkap `{ code, message }`, supaya layar menampilkan semua sekaligus (UAT-06) |
| `revise` tujuan dijawab 201 | Revisi membuat baris tujuan baru, jadi jawabannya 201 (`Created`) dengan isi tujuan baru |
| `ReviseRehabProgramRequest.authoredEncounterId` (opsional) | Kontrak hanya memuat `expectedVersion` dan `modificationReason`. Kunjungan penyusunan versi baru bisa dipilih ulang; bila kosong, memakai kunjungan versi aktif. |
| Satu draft/aktif per layanan | Pembuatan draft kedua dijawab 409 `RHB-VAL-020`. Revisi kedua saat draft ada juga 409. |
| Aktivasi oleh penulis | Matriks menyebut "profesi = disiplin". Karena aktivasi adalah tanda tangan RM, penandatangan juga harus penulis (`RHB-VAL-022`). |
| Tujuan dibuat saat *Perencanaan*/*Terapi berjalan* | Mengikuti aturan aksi episode `BE-RHB-008`; selain itu 409 `RHB-VAL-020` |
| Tujuan tanpa syarat profesi | Matriks tidak mensyaratkan profesi untuk tujuan; yang berlaku hanya butir `RehabGoal` |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` (perintah owner) | `PASS`, exit 0, 0 error, tidak ada warning di folder `RehabilitationManagement` |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik untuk setiap aksi (`Activate` dan `Stop` memakai konstanta `TherapyProgram`); grep `IsInRole`/`UserType`/nama peran = 0. |
| Review diff | 5 berkas baru, 2 berkas lama diubah (§3.1) |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-06.
- Skrip Playwright `request` dari scratchpad.
- Episode `RHB-EPS-2026-000001` (*Perencanaan* sebelum uji).
- Layanan uji A (Dokter Umum) dan B (Perawat) diaktifkan sementara. Dua tindakan `MstProcedure` aktif dipasang ke layanan A.
- Dua akun uji: profesi Dokter Umum dan Perawat, dengan butir Goal, Program, dan Education.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Controller akses `RehabGoal` dan `RehabProgram` (Read/Create/Update/Activate/Stop) terdaftar; dua akun uji login | PASS ×3 |
| 2 | V-GOL-02: tanggal target lampau → 422 `RHB-VAL-047`; isian kosong → 422 dengan 5 field | PASS ×2 |
| 3 | Tujuan di episode yang belum diasesmen → 409 `RHB-VAL-020`; tujuan G1 → 201, aksi `update/revise/discontinue` | PASS ×2 |
| 4 | Program oleh SuperAdmin (tanpa profesi) dan oleh Dokter Umum untuk layanan Perawat → 403 `RHB-VAL-031`; layanan belum diresepkan → 422 `RHB-VAL-032` | PASS ×3 |
| 5 | Draft v1 kosong → 201, keutuhan *Draf*, aksi `update/activate/cancel`; draft kedua → 409 `RHB-VAL-020` | PASS ×2 |
| 6 | **UAT-06**: aktivasi draft kosong → 422 dengan daftar `042/043/044` (040 tidak muncul karena asesmen medis sudah final) | PASS |
| 7 | Tindakan di luar daftar → 422 `042`; tujuan bukan milik episode → 422 `043`; tanggal evaluasi sebelum mulai → 422 `044` | PASS ×3 |
| 8 | **AC4**: dua `PUT` bersamaan dengan versi sama → satu 200 dan satu 409 `RHB-VAL-010`; hasil akhir v1, 2 intervensi, 1 tujuan, tanpa duplikasi | PASS ×2 |
| 9 | V-PRG-06: edukasi final terakhir *Menolak* → 422 hanya `RHB-VAL-045`; setelah edukasi *Setuju*, rincian memuat persetujuan terakhir *Setuju* | PASS ×2 |
| 10 | **UAT-07**: aktivasi → 200 *Aktif*, keutuhan *Ditandatangani*, aksi `revise/stop/addendum`; episode *Terapi berjalan* | PASS ×2 |
| 11 | Ubah program aktif → 409 `RHB-VAL-021` | PASS |
| 12 | **AC5**: ubah tujuan yang dipakai program aktif → 409 `RHB-VAL-046`; daftar tujuan menandai `usedByActiveProgram` dan aksi `update` hilang | PASS ×2 |
| 13 | Revisi tanpa alasan → 422 `RHB-VAL-008`; revisi → 201 v2 menyalin 2 intervensi + 1 tujuan; revisi kedua → 409 `RHB-VAL-020` | PASS ×3 |
| 14 | Revisi tujuan → G2 *Aktif* menunjuk G1; daftar bawaan menyembunyikan G1, `includeSuperseded=true` menampilkannya | PASS ×2 |
| 15 | **AC3**: ubah v2 (tujuan G2) lalu aktifkan → 200; layanan A punya tepat satu *Aktif* (v2), dan v1 *Digantikan versi baru* | PASS ×3 |
| 16 | Hentikan tanpa alasan → 422 `008`; hentikan → 200 *Dihentikan*; program baru → v3, aktivasi 200 | PASS ×3 |
| 17 | **UAT-06**: layanan B (Perawat) tanpa asesmen profesi final → 422 memuat `RHB-VAL-041` | PASS |
| 18 | Bukan penulis membatalkan → 403 `RHB-VAL-022`; hentikan draft → 409 `RHB-VAL-020`; batal oleh penulis → 200 *Dibatalkan* + keutuhan *Dibatalkan* | PASS ×3 |
| 19 | Hentikan tujuan → 200 *Dihentikan*; ubah sesudahnya → 409 | PASS |
| 20 | Riwayat episode memuat `activate-program` (*Planning* → *Active*) | PASS |

Rekap: **42/42 PASS**.

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| `RhbProgram` EP1 + keutuhan jenis 48 | Layanan A: v1 *Superseded*, v2 *Stopped* (revisi dari v1), v3 *Active*; ketiganya keutuhan *Signed* oleh penulis. Layanan B: v1 *Cancelled*, keutuhan *Cancelled*. |
| Satu aktif per layanan | 1 baris aktif untuk layanan A, tidak ada untuk B |
| `RhbProgramIntervention` | v1: 2 aktif; v2: 2 aktif + 2 terhapus lunak (diganti saat ubah draft); v3: 2 aktif |
| `RhbGoal` | G1 *Superseded*, G2 *Aktif* (revisi), G3 *Dihentikan* |
| `RhbStatusHistory` | `create` → `activate` (+ episode `activate-program`) → `revise` → `superseded` → `activate` → `stop` → `create` → `activate`; tujuan `revise` ×2 dan `discontinue`; program B `create` → `cancel` |
| `RhbEpisode` EP1 | Status 3 (*Active*), versi 2 |

**Pembersihan.**
- 0 dari 22 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- layanan uji A/B nonaktif;
- backend dihentikan.

Program v3 layanan A sengaja dibiarkan aktif sebagai bahan uji sesi (`BE-RHB-012`). Dua tindakan yang
dipasang ke layanan uji A tetap terpasang; layanan itu sendiri nonaktif.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-06: 422 daftar prasyarat `RHB-VAL-040..045` | Terpenuhi | §5 #6, #9, #17 (042, 043, 044, 045, 041 terbukti di runtime). 040 terbukti lewat kode; data episode yang tersedia sudah punya asesmen medis final, dan program memang tidak dapat dibuat sebelum ada resep. |
| (2) UAT-07 | Terpenuhi | §5 #10, §6 |
| (3) Revisi v1→v2: satu aktif per layanan | Terpenuhi | §5 #15, §6 |
| (4) Simpan bersamaan → 409 | Terpenuhi | §5 #8 |
| (5) Tujuan dipakai program aktif tidak bisa di-`PUT` (409 `RHB-VAL-046`) | Terpenuhi | §5 #12 |
| DoD: build owner, laporan | Terpenuhi | §4, berkas ini |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| `RHB-VAL-040` di runtime | Program hanya dapat dibuat untuk layanan yang diresepkan asesmen medis final, sehingga 040 praktis hanya muncul bila asesmen medis dibatalkan lewat jalur lain. Kodenya tetap diperiksa. |
| `NeedsReview` | Disetel oleh deviasi sesi (`BE-RHB-012`) dan keputusan reasesmen `Modify` (`BE-RHB-014`); task ini hanya membacanya |
| Status *Completed* | Diberikan otomatis saat ringkasan akhir ditandatangani (`BE-RHB-015`) |
| Task berikutnya | `BE-RHB-012` (sesi) dan `BE-RHB-014` (reasesmen) kini 🟡. `FE-RHB-007` masih menunggu `FE-RHB-005`. |
