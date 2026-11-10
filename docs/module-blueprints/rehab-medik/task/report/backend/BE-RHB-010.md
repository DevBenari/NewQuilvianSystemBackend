# Laporan Perubahan Backend — `BE-RHB-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-010` |
| Judul | Edukasi dan persetujuan |
| Slice | S-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-010` |
| Trace | FR-RHB-020; FEAT-015; `data-dictionary.md` bagian 2.11; `state-transition-matrix.md` §3 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Education* |
| Dependency | `BE-RHB-004` ✅ (2026-11-06), `BE-RHB-008` ✅ |
| Klasifikasi | `STANDARD` — endpoint transaksi baru, tanpa schema baru, memakai mesin keutuhan RM |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Constants}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). Build owner `PASS` (0 error, 0 warning di folder Rehab), QBE Strict `PASS`. Uji API manual 26/26. Bukti DB cocok (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru; `RhbEducationRecord` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi — sub-proses ter-scope induk (episode, opsional program).** Empat endpoint sesuai kontrak: `GET /by-episode/{episodeId}`, `POST /by-episode/{episodeId}`, `PUT /{id}`, `POST /{id}/sign`. Tidak ada `options`, `PATCH status`, `DELETE`, maupun `cancel`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.Education`), aturan transaksi gateway RM |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` jenis 52 (persetujuan RM `RHB-OQ-012` ✅). Baca saja: `RegPatientEncounter`, `InpEpisode`. |

## 1. Masalah yang diperbaiki

Program terapi tidak boleh diaktifkan bila pasien atau keluarga menolak rencananya (FR-RHB-020).
Untuk itu, setiap edukasi beserta jawaban persetujuannya harus tercatat sebagai dokumen klinis yang
bisa dipegang.

**Contoh.** Fisioterapis menjelaskan program latihan kepada anak pasien. Keluarga menolak latihan
jalan di luar ruangan karena pendengaran pasien menurun. Fisioterapis mencatat:
- materi, penerima ("Keluarga Uji", *Anak*), dan metode (lisan + demonstrasi);
- pemahaman *Memahami*, dan persetujuan *Menolak* beserta catatan alasannya.

Ia lalu menandatanganinya. Sejak itu catatan terkunci, dan koreksi hanya lewat addendum Rekam
Medis. Saat `BE-RHB-011` dikerjakan, penolakan ini yang akan memblokir aktivasi program
(`RHB-VAL-045`).

## 2. Proses bisnis

1. **Catat draft** (`Create`).
   - Episode belum ditutup.
   - Kunjungan milik pasien episode, masih terbuka.
   - Isian wajib: materi, penerima, hubungan, metode, dan status persetujuan.
   - Program (opsional) harus milik episode yang sama.
   - Pemberi edukasi = pengguna yang mencatat.
   - Draft didaftarkan ke Rekam Medis jenis 52 dalam transaksi yang sama.
2. **Ubah draft** (`Update`). Hanya pemberi edukasi, dengan `expectedVersion`, selama Rekam Medis mengizinkan. Kunjungan tidak dapat diganti.
3. **Persetujuan *Menolak* atau *Setuju sebagian*** wajib disertai catatan, diperiksa saat simpan maupun tanda tangan (AC2).
4. **Tanda tangan** (`Sign`).
   - Tingkat pemahaman tidak boleh *Belum dinilai* (AC1).
   - Status *Final* dan kunci Rekam Medis ditulis dalam satu transaksi.
5. **Sesudah final**, ubah dan tanda tangan ulang dijawab 409 `RHB-VAL-021` (AC3). Koreksi lewat addendum RM jenis 52.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabEducationController.cs` | Baru | 4 endpoint, Resource `RehabEducation`, aksi `Read`/`Create`/`Update`/`Sign` |
| `Services/RhbEducationService.cs` | Baru | Aturan §2, label Bahasa Indonesia pemahaman dan persetujuan, `availableActions` per kepenulisan |
| `DTOs/RehabEducationDtos.cs` | Baru | `RehabEducationQuery`, `SaveRehabEducationRequest`, `RehabEducationItem` |
| `Constants/RhbErrorCodes.cs` | Diperbarui | `RHB-VAL-083` edukasi tidak lengkap, `RHB-VAL-084` program bukan milik episode. Nama konstanta `RHB-VAL-081` diganti menjadi `ClinicalEncounterInvalid` (nilai tetap) agar dipakai bersama asesmen dan edukasi. |
| `Services/RhbAssessmentService.cs` | Diperbarui | Mengikuti nama konstanta 081 yang baru, tanpa perubahan perilaku |
| `Program.cs` | Diperbarui | `AddScoped<RhbEducationService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Controller baru `RehabEducation` dengan Read, Create, Update, dan Sign (tipe `Update`). Tidak ada nama peran di kode. Kepenulisan adalah kewenangan yang melekat pada data. |
| Logger | id, episode, program, status, persetujuan, versi. Materi, nama penerima, hambatan, dan catatan tidak dicatat. |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| Kode `RHB-VAL-083`/`084` | Matriks validasi v1 tidak punya baris edukasi. Aturannya diturunkan dari kamus data 2.11 (`NotAssessed` tidak boleh saat tanda tangan; catatan wajib bila `Refused`/`PartiallyAgreed`). |
| Catatan persetujuan diperiksa saat simpan | Bukan hanya saat tanda tangan, supaya draft penolakan tanpa alasan tidak pernah tersimpan |
| Tanpa aksi batal | Kontrak v1 tidak memuat `cancel`. Draft yang keliru diperbaiki pemberinya, lalu ditandatangani. Bila owner membutuhkan pembatalan draft, perlu amandemen kontrak. |
| Kunjungan tidak dapat diganti | Sama dengan asesmen, karena baris keutuhan RM terikat ke kunjungan |
| `RHB-VAL-081` dipakai bersama | Nilai kode tidak berubah, jadi tidak berdampak ke klien |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` (perintah owner) | `PASS`, exit 0, 0 error, tidak ada warning di folder `RehabilitationManagement` |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik untuk keempat aksi; grep `IsInRole`/`UserType`/nama peran/departemen/jabatan = 0. |
| Review diff | 3 berkas baru, 3 berkas lama diubah (§3.1); tidak ada berkas modul lain yang ditulis |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-06.
- Skrip Playwright `request` dari scratchpad; kredensial dibaca di dalam skrip.
- Episode `RHB-EPS-2026-000001` (status *Perencanaan* sejak `BE-RHB-009`), kunjungan terbuka pasien A.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | `GET by-episode` sebelum ada edukasi → 200 `[]` | PASS |
| 2 | Catat tanpa kunjungan → 422 `RHB-VAL-081` | PASS |
| 3 | Catat tanpa isian wajib → 400 `RHB-VAL-083`, 5 field ditunjuk | PASS |
| 4 | **AC2**: catat *Menolak* tanpa catatan → 422 `RHB-VAL-083` (`consentNote`); *Setuju sebagian* tanpa catatan → 422 | PASS ×2 |
| 5 | Program bukan milik episode → 422 `RHB-VAL-084` | PASS |
| 6 | Catat draft (*Setuju*, pemahaman kosong) → 201 v0, *Belum dinilai*, keutuhan *Draf*, aksi `update/sign` | PASS |
| 7 | **AC1**: tanda tangan dengan *Belum dinilai* → 422 `RHB-VAL-083` (`understandingLevel`) | PASS |
| 8 | Versi basi → 409 `RHB-VAL-010`; ubah ke *Menolak* tanpa catatan → 422 (AC2); ganti kunjungan → 422 `RHB-VAL-081` | PASS ×3 |
| 9 | Ubah: *Menolak* + catatan + *Memahami* → 200 v1 | PASS |
| 10 | Tanda tangan → 200 *Final*, keutuhan *Ditandatangani*, `signedAt` terisi, aksi `addendum` | PASS |
| 11 | **AC3**: ubah dan tanda tangan ulang sesudah final → 409 `RHB-VAL-021` | PASS ×2 |
| 12 | Addendum RM jenis 52 → 201 | PASS |
| 13 | Daftar 1 baris; saringan `programId` lain → 0 baris | PASS |
| 14 | Ruang kerja episode menghitung edukasi (`educationRecord` = 1) dan timeline memuat *Education* | PASS |
| 15 | Controller akses `RehabEducation` terdaftar dengan Read/Create/Update/Sign | PASS |
| 16 | Akun uji (Read/Create/Update, tanpa Sign): login; mengubah draft milik orang lain → 403 `RHB-VAL-022`; mencatat sendiri → 201; tanda tangan → 403 dari filter hak akses | PASS ×4 |
| 17 | `availableActions`: draft sendiri `update/sign`, draft orang lain kosong | PASS |
| 18 | SuperAdmin menandatangani edukasi milik akun uji → 403 `RHB-VAL-022` (kepenulisan tetap berlaku untuk SuperAdmin) | PASS |
| 19 | Setelah butir Sign diberikan dan akun login ulang, pemberi menandatangani draftnya → 200 *Final* | PASS |

Rekap: **26/26 PASS**.

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| `RhbEducationRecord` + keutuhan jenis 52 | 3 baris, semua *Final* dengan keutuhan *Signed* (pemicu: tanda tangan penulis, IP tercatat). Satu *Menolak* bercatatan (v2), satu *Tidak memerlukan persetujuan*, satu *Setuju*. Tidak ada draft yang tertinggal. |
| `MrcClinicalNoteAddendum` jenis 52 | 1 |

**Pembersihan.**
- 0 dari 20 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- backend dihentikan.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tanda tangan dengan `NotAssessed` ditolak | Terpenuhi | §5 #7 |
| (2) `Refused` tanpa catatan ditolak | Terpenuhi | §5 #4, #8 |
| (3) Edukasi final tidak bisa diubah | Terpenuhi | §5 #11, §6 |
| DoD: laporan | Terpenuhi | Berkas ini (build owner juga dijalankan, §4) |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Efek penolakan ke program | `RHB-VAL-045` (V-PRG-06) dibaca saat aktivasi program di `BE-RHB-011`. Edukasi ini sudah menyediakan datanya (persetujuan *Menolak* pada edukasi final terakhir sebuah program). |
| Draft tanpa batal | Draft edukasi tidak dapat dibatalkan menurut kontrak v1. Draft yang terbengkalai akan menahan penutupan episode (V-DIS-02) sampai ditandatangani pemberinya. Bila perlu, ajukan amandemen kontrak. |
| Task berikutnya | `BE-RHB-011` (tujuan dan program) kini 🟡; `FE-RHB-006` (layar edukasi) kini 🟡 |
