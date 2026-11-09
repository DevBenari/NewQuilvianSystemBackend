# Laporan Perubahan Backend — `BE-RHB-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-014` |
| Judul | Reasesmen dan keputusan |
| Slice | S-E — Reasesmen dan penutupan |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-014` |
| Trace | FR-RHB-040, 041; FEAT-024, 025; `state-transition-matrix.md` §7–8; V-MEA-01/02 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Progress Review* |
| Dependency | `BE-RHB-011` ✅ (2026-11-06); `BE-RHB-012` ✅ (2026-11-09) |
| Klasifikasi | `STANDARD` — endpoint transaksi baru, tanpa schema baru; memindahkan aturan pengukuran `BE-RHB-009` ke kelas bersama |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Constants}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-09 |
| Status | **✅ Selesai** (2026-11-09). Build owner `PASS` (0 error; 426 warning bawaan, tidak ada di folder Rehab). QBE Strict `PASS`. Uji API: run utama 23/26 (tiga FAIL dijelaskan di §5), lalu uji ulang 11/11 setelah build yang benar-benar memuat perbaikan. AC 1–4 terbukti di runtime. Ada residu: 2 draft reasesmen uji di EP1 (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` (+ refactor perilaku-netral pada kode `BE-RHB-009`) |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru; `RhbProgressReview`, `RhbGoalReview`, `RhbMeasurement` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi — sub-proses ter-scope induk (episode).** Buat lewat `POST /by-episode/{episodeId}`, ubah `PUT /{id}`, tanda tangan + efek keputusan `POST /{id}/sign`, batal `POST /{id}/cancel` (delta). Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001, QBE-API-001, standar hak akses (`RehabilitationPermissions.ProgressReview`), aturan transaksi gateway RM |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` jenis 50 (persetujuan RM `RHB-OQ-012` ✅). Baca saja: `CliClinicalInstrument(Version)`, `RegPatientEncounter`. |

## 1. Masalah yang diperbaiki

Terapis dan dokter rehab perlu menilai kemajuan pasien dengan cara yang sepadan dengan pengukuran awal,
lalu memutuskan nasib program: dilanjutkan, diubah, dihentikan, dirujuk, atau dinyatakan selesai.
Keputusan itu harus langsung tercermin di status program dan tujuan.

**Contoh.** Asesmen awal fisioterapi mencatat jarak tempuh 6 menit 320 meter. Saat reasesmen,
terapis mencatat 400 meter, dan rincian menampilkan baseline 320 meter dengan selisih +80. Bila terapis
mencatat dalam kilometer, reasesmen ditolak sampai ia menjelaskan alasan pergantian satuan.

Terapis menilai tujuan "jalan 75 m" *Tercapai* dan memutuskan *Hentikan*. Saat ditandatangani,
program berstatus *Dihentikan* dengan alasan tercatat, dan tujuan menjadi *Tercapai*. Bila keputusannya
*Ubah program*, program tetap aktif tetapi bertanda *perlu telaah*.

## 2. Proses bisnis

1. **Buat draft.**
   - Episode *Perencanaan* atau *Terapi berjalan*.
   - Reasesmen profesi: wajib program *Aktif* layanan itu (422/409 `RHB-VAL-050`) dan profesi yang cocok (403 `RHB-VAL-031`).
   - Reasesmen medis: tanpa layanan dan tanpa program (400 `RHB-VAL-084` bila membawa program); kewenangannya dari butir `RehabProgressReview` yang admin berikan.
   - Kunjungan milik pasien dan terbuka (`RHB-VAL-081`).
   - Dokumen didaftarkan ke Rekam Medis jenis 50.
2. **Pengukuran** memakai aturan yang sama dengan asesmen (V-MEA-01).
3. **Pembanding baseline (V-MEA-02).**
   - Sumbernya asesmen awal final: profesi untuk layanan itu, atau medis.
   - Pengukuran dicocokkan menurut jenis instrumen (berinstrumen) atau nama + bagian tubuh (terstruktur).
   - Versi instrumen atau satuan yang berbeda wajib disertai `instrumentChangeNote` (422 `RHB-VAL-036`), diperiksa saat simpan dan saat tanda tangan.
   - Rincian menampilkan baseline, `isComparable`, dan selisihnya.
4. **Penilaian tujuan.** Hanya tujuan aktif episode untuk layanan yang sama, dengan nilai *masih berjalan*, *tercapai*, *tercapai sebagian*, atau *tidak tercapai*.
5. **Tanda tangan** (hanya penulis).
   - Program harus masih aktif (409 `RHB-VAL-050`, diperiksa lebih dulu).
   - Ringkasan, keputusan, dan alasan wajib; tujuan rujukan wajib bila *Rujuk* (422 `RHB-VAL-085`).
   - Pengukuran berinstrumen harus lengkap (`RHB-VAL-035`).
   - Dalam satu transaksi: status *Final*, efek keputusan, status tujuan, riwayat, dan kunci Rekam Medis.
6. **Efek keputusan** (§7):

   | Keputusan | Efek |
   | --- | --- |
   | *Lanjutkan* | Tidak ada perubahan. |
   | *Ubah program* | Program `NeedsReview`. |
   | *Hentikan* / *Rujuk* | Program *Dihentikan* + alasan, termasuk tujuan rujukan. |
   | *Selesai* | Program *Selesai*. |

   Pada reasesmen medis, *Hentikan*/*Rujuk*/*Selesai* hanya dicatat sebagai `ready-to-close:<keputusan>` di riwayat episode; status episode tidak berubah.
7. **Batal draft** (delta): penulis, alasan wajib; Rekam Medis menandai *Dibatalkan*.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabProgressReviewController.cs` | Baru | 6 endpoint (5 kontrak + `cancel`), Resource `RehabProgressReview`, aksi `Read`/`Create`/`Update`/`Sign` |
| `Services/RhbProgressReviewService.cs` | Baru | Aturan §2, pembanding baseline, efek keputusan |
| `Services/RhbMeasurementRules.cs` | Baru | Aturan pengukuran bersama (V-MEA-01) yang dipindah dari `RhbAssessmentService` tanpa mengubah perilaku, plus `RhbRuleFailure` dan `RhbMeasurementValue` |
| `DTOs/RehabProgressReviewDtos.cs` | Baru | `SaveRehabProgressReviewRequest`, `RehabGoalReviewRequest`, `RehabProgressReviewItem`, `RehabProgressReviewDetailResponse`, `RehabMeasurementComparison`, `RehabGoalReviewItem` |
| `Services/RhbAssessmentService.cs` | Diperbarui | Memakai `RhbMeasurementRules`; metode lokal menjadi delegasi |
| `Constants/RhbErrorCodes.cs` | Diperbarui | `RHB-VAL-085` reasesmen belum lengkap |
| `Program.cs` | Diperbarui | `AddScoped<RhbMeasurementRules>()`, `AddScoped<RhbProgressReviewService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Controller baru dengan Read, Create, Update, dan Sign. `cancel` memakai butir `Update`. Tidak ada nama peran di kode. |
| Logger | id, program, keputusan, status, versi. Ringkasan, hambatan, dan alasan tidak dicatat. |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| **`POST /{id}/cancel`** | Tidak ada di kontrak v1. Tanpa aksi ini, draft reasesmen yang programnya sudah tidak aktif (misalnya dihentikan reasesmen lain) tidak bisa ditandatangani maupun disingkirkan, dan akan menahan penutupan episode (V-DIS-02). Polanya sama dengan dokumen rehab lain. |
| Urutan pemeriksaan tanda tangan | Program tidak aktif (409 `050`) dilaporkan sebelum kelengkapan isian (422 `085`) |
| Kode `RHB-VAL-085` | Kelengkapan reasesmen saat tanda tangan belum punya baris di matriks v1 |
| Reasesmen medis tanpa program | Kontrak memuat `programId?`; kamus data mewajibkan program hanya untuk reasesmen profesi. Reasesmen medis yang membawa program ditolak 400 `084`. |
| Penanda "siap ditutup" | Tidak ada kolomnya. Dicatat sebagai baris riwayat episode `ready-to-close:<keputusan>` tanpa perubahan status, untuk dibaca `BE-RHB-015`. |
| V-MEA-02 saat simpan | Diperiksa saat simpan maupun tanda tangan |
| Pencocokan baseline | Berinstrumen menurut jenis instrumen; terstruktur menurut nama + bagian tubuh (tanpa beda huruf besar) |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| Build owner 2026-11-09 (`dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false`) | Build 1: `PASS` 0 error, 3m36s. Build 2: 4 detik tanpa kompilasi (MSBuild menilai keluaran lebih baru). Build 3 setelah `touch` berkas yang berubah: `PASS` **0 error**, 426 warning bawaan (tidak ada di folder Rehab), 3m06s. Lihat catatan di bawah. |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik; grep `IsInRole`/`UserType`/nama peran = 0. |
| Review diff | 4 berkas baru, 3 berkas lama diubah (§3.1) |

**Catatan build.** Perubahan aksi batal disimpan ketika build 1 sedang berjalan. Kompiler sudah membaca
sumber sebelum perubahan itu, tetapi DLL selesai sesudahnya. Akibatnya build 2 menganggap semuanya
mutakhir, dan backend berjalan tanpa aksi batal (inilah penyebab FAIL #2/#3 di run utama). Build 3
dijalankan sesudah *timestamp* berkas diperbarui.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-09.
- Episode `RHB-EPS-2026-000001`; layanan uji A diaktifkan sementara.
- Baseline profesi "Jarak tempuh 6 menit" = 320 meter (dari `BE-RHB-009`).
- Akun uji berprofesi Dokter Umum dengan butir `RehabProgressReview`, `RehabProgram`, dan `RehabGoal`.

**Run utama (23/26)**

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | **Regresi asesmen** setelah refactor: terstruktur tanpa nilai → 400 `082`; instrumen konsep tanpa izin → 422 `035` (field `measurements[0].instrumentVersionId`) | PASS ×2 |
| 2 | Controller akses terdaftar; akun uji login; persiapan: revisi v3 → v4 lalu aktivasi (v4 `NeedsReview = false`) | PASS ×3 |
| 3 | Program tidak aktif → 409 `050`; reasesmen profesi tanpa program → 422 `050`; SuperAdmin → 403 `031`; reasesmen medis membawa program → 400 `084`; tujuan *Superseded* dinilai → 422 `047` | PASS ×5 |
| 4 | **AC1**: satuan km vs baseline meter tanpa alasan → 422 `RHB-VAL-036`; dengan alasan → 201, pembanding `isComparable = false` tanpa selisih | PASS ×2 |
| 5 | **AC4**: 400 meter vs baseline 320 meter → `isComparable = true`, selisih **+80**; pengukuran tanpa baseline → `hasBaseline = false` | PASS ×2 |
| 6 | Rincian draft: aksi `update/sign/cancel` | **FAIL**: DLL tanpa aksi batal (catatan build) |
| 7 | Tanda tangan tanpa ringkasan/keputusan/alasan → 422 `085` (3 field); bukan penulis → 403 `022` | PASS ×2 |
| 8 | **AC3**: *Ubah program* → *Final*, keutuhan *Ditandatangani*; program v4 `NeedsReview = true`, tetap *Aktif* | PASS |
| 9 | Ubah reasesmen final → 409 `021` | PASS |
| 10 | **AC2**: *Hentikan* + tujuan G2 *Tercapai* → program v4 *Dihentikan* dengan alasan; G2 *Tercapai* | PASS |
| 11 | Draft tanpa keputusan, program sudah dihentikan → diharapkan 409 `050` | **FAIL**: 422 `085`. Urutan pemeriksaan diperbaiki (program lebih dulu). |
| 12 | Batal draft | **FAIL**: 404 (DLL tanpa aksi batal) |
| 13 | Reasesmen medis *Rujuk* tanpa tujuan rujukan → 422 `085`; lengkap → *Final*, episode tetap *Terapi berjalan* + riwayat `ready-to-close:Refer` | PASS ×2 |
| 14 | Daftar 5 reasesmen; ruang kerja `progressReviewFinal = 3`; timeline memuat reasesmen | PASS |

**Uji ulang setelah build 3 (11/11)**

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | Persiapan: tujuan G4 + program v5 aktif | PASS |
| 2 | Rincian draft: aksi `update/sign/cancel` | PASS |
| 3 | *Hentikan* → program v5 *Dihentikan* | PASS |
| 4 | Draft **belum lengkap** + program sudah dihentikan → **409 `050`** (bukan `085`); draft lengkap → 409 `050` | PASS ×2 |
| 5 | Batal tanpa alasan → 422 `008`; bukan penulis → 403 `022`; penulis → *Dibatalkan* + keutuhan *Dibatalkan* (2 draft); ubah sesudah batal → 409 `020` | PASS ×2 |
| 6 | Batal draft reasesmen medis oleh penulisnya → 200 | PASS |
| 7 | Controller akses, login, layanan aktif | PASS ×3 |

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| `RhbProgressReview` + keutuhan jenis 50 | 4 *Final* (*Ubah program*, *Hentikan* ×2, medis *Rujuk*) dengan keutuhan *Signed*; 3 *Dibatalkan* dengan keutuhan *Cancelled*; **2 draft** (lihat residu) |
| `RhbProgram` layanan A | v1 *Superseded*; v2 *Stopped*; v3 *Superseded* (`NeedsReview`); v4 *Stopped* (`NeedsReview` dari *Ubah program*, alasan dari *Hentikan*); v5 *Stopped* |
| `RhbGoal` / `RhbGoalReview` | G2 *Tercapai*; G4 *Aktif*; penilaian aktif: 1 *masih berjalan*, 1 *tercapai* |
| `RhbMeasurement` | 3 pengukuran reasesmen; 0 baris berpemilik ganda (`CK_RhbMeasurement_Owner` dihormati) |
| `RhbStatusHistory` | `progress-review-stop` ×2 (program), `progress-review` (tujuan → *Achieved*), `ready-to-close:Refer` (episode) |

**Residu — perlu diketahui sebelum `BE-RHB-015`.** Dua draft reasesmen dari run utama tertinggal di
EP1. Pembuatnya akun uji yang sudah dinonaktifkan, dan saat run itu backend belum memuat aksi batal. Draft
itu tidak dapat dibatalkan karena hanya penulis yang boleh, dan sandi akun uji tidak tersimpan. Akibatnya
EP1 akan ditolak saat ringkasan akhir ditandatangani (V-DIS-02). Ini justru bisa dipakai sebagai bukti
jalur gagal di `BE-RHB-015`; jalur berhasil diuji di episode lain.

**Pembersihan.**
- 0 dari 26 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- layanan uji A nonaktif;
- backend dihentikan.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Instrumen beda tanpa alasan → 422 `RHB-VAL-036` | Terpenuhi | Run utama #4 (satuan berbeda) |
| (2) `Stop` → program `Stopped` | Terpenuhi | Run utama #10, uji ulang #3, §6 |
| (3) `Modify` → `NeedsReview` | Terpenuhi | Run utama #8, §6 |
| (4) Detail menampilkan baseline pembanding dengan instrumen dan satuan yang sama | Terpenuhi | Run utama #5 (320 → 400, +80) |
| DoD: laporan (build owner juga dijalankan) | Terpenuhi | §4, berkas ini |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Perubahan instrumen berinstrumen | AC1 dibuktikan lewat beda satuan pada pengukuran terstruktur. Beda versi instrumen memakai cabang kode yang sama (`IsChanged`), tetapi belum diuji di runtime karena belum ada instrumen rehab yang punya dua versi. |
| Draft residu EP1 | Lihat §6 |
| Amandemen kontrak | `cancel` untuk reasesmen (dan untuk edukasi, temuan `BE-RHB-010`) sebaiknya dimasukkan ke kontrak versi berikutnya |
| Task berikutnya | `BE-RHB-015` (ringkasan akhir, penutupan episode, pembaca berkas RM) kini 🟡. `FE-RHB-010` menunggu `FE-RHB-007`. |
