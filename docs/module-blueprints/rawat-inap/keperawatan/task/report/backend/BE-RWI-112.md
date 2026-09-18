# Laporan Perubahan Backend — `BE-RWI-112`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-112` |
| Judul | Progres pengkajian lima bagian |
| Slice | Gelombang 5 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-112` |
| Trace | `FR-KEP-050`, `FR-KEP-051`, `FR-KEP-052`; `RWI-DEC-119`, `RWI-DEC-120`; `INV-KEP-09`; `AC-KEP-072`; `VAL-KEP-36d`; api-contract 7.1 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-110` ✅, `BE-RWI-111` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — permukaan baca agregat |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — enam kriteria terpetakan; `dotnet build` lolos; kontrak API runtime `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` |
| Database | Nol migration |

## 1. Masalah yang diperbaiki

Perawat harus membuka bagian satu per satu untuk tahu mana yang belum dikaji. Menghitung progres di layar
membuat angka berbeda antarlayar, dan layar yang gagal memuat data cenderung menampilkan semua bagian "belum
diisi" — informasi yang salah.

## 2. Proses bisnis

1. Ruang kerja memanggil `GET /patient-assessments/episodes/{episodeId}/progress`.
2. Server menilai lima bagian dari keadaan dokumen yang tidak dibatalkan:
   `Completed` bila ada satu dokumen selesai (walau ada konsep baru), `NeedsAttention` bila hanya konsep,
   `NotFilled` bila tidak ada.
3. **Contoh Budi hari ke-2:** Kajian Umum ✓, Resiko Jatuh ✓ (pita Tinggi), Monitoring Nyeri ! (konsep),
   Edukasi ○, Perencanaan Pulang ○ → `CompletedCount = 2`, `ProgressPercent = 40`; `Alerts` berisi
   "Resiko Jatuh Tinggi". Resiko Jatuh tetap ✓.
4. Pengawasan Harian ("terakhir dicatat …" dari tanda vital, cairan, GDS, observasi) dan Evaluasi Awal
   ("Belum diisi — milik MPP") tampil **tanpa** dihitung ke persen.
5. Monitoring Nyeri yang waktu kajian ulangnya lewat tanpa dokumen nyeri sesudahnya → `ReassessmentOverdue`
   pada bagian PAIN, tanpa mengubah ✓/!/○.
6. **Jalur tidak normal:** episode tidak ada → `404`; galat basis data tidak ditangkap untuk dijawab lima ○ —
   layar menampilkan "Gagal memuat progres pengkajian".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

api-contract 7.1 (`NursingAssessmentProgressResponse`), frontend architecture 10.4.1–10.4.2, `NursingAssessmentMonitoringService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingAssessmentProgressDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentProgressService.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | `GET episodes/{episodeId}/progress` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai 7.1; ditambah isian `Alerts` untuk kepala konteks (frontend architecture 10.4.1 membaca alert dari endpoint ini) |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | Memakai `PatientAssessment : Read` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}/progress` | Progres lima bagian, persen, dua isian tampil, alert klinis | `PatientAssessment : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Verifikasi kontrak API 7.1 terhadap DTO | Seluruh isian kontrak ada | `PASS` | `NursingAssessmentProgressResponse` |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Lima bagian ✓/!/○ menurut keadaan dokumen | Terpenuhi | `GetProgressAsync` — `Completed`/`NeedsAttention`/`NotFilled` |
| 2. Persen kelipatan 20 | Terpenuhi | `ProgressPercent = CompletedCount * 20` |
| 3. Pengawasan Harian dan Evaluasi Awal tampil tanpa dihitung | Terpenuhi | `DailyMonitoringLastRecordedAt`, `CaseManagementEvaluationStatus` di luar `Sections` |
| 4. Temuan berisiko sebagai alert, tidak mengubah progres | Terpenuhi | `Alerts` dari `IsAlertBand` dokumen selesai terakhir |
| 5. Gagal dimuat menampilkan galat, bukan ○ | Terpenuhi | Nol penangkapan galat; `404` hanya untuk episode tidak ada |
| 6. Dihitung server | Terpenuhi | Service server-side |
| DoD: `dotnet build` | `dotnet build` lolos | `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` controller |
| Langkah berikutnya | Frontend progres |

## Lampiran — build dan migration 17 September 2026

Dijalankan atas permintaan pemilik setelah seluruh task roadmap ditandai.

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` |
| `dotnet ef migrations has-pending-model-changes --no-build` (`ASPNETCORE_ENVIRONMENT=Development`) | "No changes have been made to the model since the last migration." — snapshot tulis tangan sama dengan model | `PASS` |
| `dotnet ef migrations list --no-build` sebelum diterapkan | Enam migration K1–K7 `Pending`; nol migration lain tertunda | `PASS` |
| `dotnet ef database update --no-build` | `Done.` dalam 30 detik; target `QuilvianNewDevHamzah` (database pribadi pemilik) | `PASS` |
| Pembacaan katalog lewat `dotnet fsi` + `Npgsql.dll` hasil build | 16 tabel baru, 6 kolom tabel legacy, 7 index unik parsial (2 `NULLS NOT DISTINCT`), 11 check constraint, 63 foreign key, 6 baris `__EFMigrationsHistory` versi 9.0.18 | `PASS` |
| Uji mundur `Down` pada Postgres sekali pakai | Tidak dijalankan — Docker tidak aktif; sengaja tidak diuji pada database pribadi supaya tabel tidak terhapus | `NOT RUN` |
| Verifikasi runtime API dan proses bisnis | Tidak diminta pada putaran build dan migration ini | `NOT RUN` |
