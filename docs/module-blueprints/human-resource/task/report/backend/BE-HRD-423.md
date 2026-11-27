# Laporan Perubahan Backend — `BE-HRD-423`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-423` |
| Judul | Scheduler siklus kontrak, pengingat, dan masa kerja |
| Slice | `S-FIN-M08` / Lintas Submenu (alias `FIN-HRD-08-BE-008`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.5, kartu `BE-HRD-423` |
| Trace | Instruksi Menu 08 `FIN-HRD-08-BE-008` |
| Contract version | Kontrak Kontrak Kerja existing (`BE-HRD-065`, `BE-HRD-080`) |
| Dependency | `BE-HRD-417` ✅, `BE-HRD-422` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `HrdEmploymentLifecycleSchedulerService`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji scheduler `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Tanggal operasional WIB, aktivasi kontrak `Signed` tepat pada hari H, dan penanganan kontrak lewat masa sudah ada. Celah: **tidak ada kunci antar-instance** — pada deployment lebih dari satu instance (mis. Blue-Green), dua scheduler dapat menjalankan langkah harian bersamaan dan memproses kontrak yang sama dua kali.

## 2. Proses bisnis

1. Langkah harian membuka koneksi dan mencoba kunci sesi PostgreSQL `HRD_EMPLOYMENT_LIFECYCLE_DAILY_TICK` (`pg_try_advisory_lock`).
2. Instance yang tidak mendapat kunci mencatat log dan melewati tick.
3. Instance pemegang kunci menjalankan: aktivasi kontrak bertanggal mulai ≤ hari ini (WIB), pengakhiran kontrak lewat masa, sinkron riwayat dan cermin profil, butir otomatis onboarding, eskalasi H-7 (`BE-HRD-419`).
4. Kunci dilepas dan koneksi ditutup di blok `finally`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdEmploymentLifecycleSchedulerService.cs`, hosted service pemanggilnya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentLifecycleSchedulerService.cs` | Kunci sesi antar-instance di `ExecuteDailyTickAsync`; inti tick dipindah ke `ExecuteDailyTickCoreAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Tanpa migration |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint — proses latar.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (berkas Menu 08) | 9 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: kontrak mulai besok | Tidak diaktifkan | `PASS` | filter `StartDate <= today` (WIB) |
| Penelusuran: dua instance | Instance kedua melewati tick | `PASS` | `pg_try_advisory_lock` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tidak ada aktivasi sebelum tanggal | Terpenuhi | filter tanggal WIB |
| (2) Dua instance tidak memproses ganda | Terpenuhi | kunci sesi antar-instance |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-424` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
