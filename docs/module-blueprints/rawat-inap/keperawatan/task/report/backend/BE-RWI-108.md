# Laporan Perubahan Backend — `BE-RWI-108`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-108` |
| Judul | Pengesahan versi instrumen |
| Slice | Gelombang 3 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-108` |
| Trace | `FR-KEP-040`; `VAL-KEP-20a`–`c`; `RWI-AC-196`; `RWI-DEC-136`; state matrix 0.5.0 bagian 5.1 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-107` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — mesin status versi, kendali empat mata, transaksi pemensiunan |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` (service dan controller `BE-RWI-107`) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — empat kriteria terpetakan; `dotnet build` lolos; uji runtime pengesahan `NOT RUN` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001` |
| Hak akses | `ClinicalInstrumentConfiguration : Approve` (dibuat `BE-RWI-107`) |
| Database | Nol migration tambahan; check constraint dan index unik parsial lahir di K1 |

## 1. Masalah yang diperbaiki

Orang yang mengetik angka batas tidak boleh menjadi orang yang menyatakan angka itu sah dipakai pada pasien.
Tanpa kendali empat mata, satu admin dapat mengubah pita Tinggi menjadi `≥ 90` dan langsung memberlakukannya.

## 2. Proses bisnis

1. Andi mengubah konsep "Morse Dewasa v2" Senin 09.00 → `LastModifiedByUserId = Andi`.
2. Selasa 10.00 Ns. Wati (komite keperawatan) mengesahkan → `PATCH /versions/{v2}/approve` dengan
   `ExpectedDefinitionHash` yang ia baca. Dalam **satu transaksi**: v1 yang sah → `Retired`
   (`RetireReason = "Digantikan versi 2."`), lalu v2 → `Approved`. Dua `SaveChanges` berurutan di dalam
   transaksi karena index unik parsial "satu versi sah per instrumen" diperiksa per pernyataan.
3. **Jalur tidak normal:**
   - Andi sendiri mencoba mengesahkan → `403` "Versi ini terakhir diubah oleh Anda. Pengesahan harus dilakukan
     orang lain." — walaupun Andi memegang butir `Approve`. Check constraint
     `CK_CliClinicalInstrumentVersion_ApproverDiffers` menjaga lapis basis data.
   - Andi mengubah definisi pukul 09.59 sementara layar Wati masih memuat hash lama → `409` "Definisi sudah
     diubah orang lain."
   - Mengesahkan versi yang sudah `Approved` → `409`; versi `Retired` → `409` "…tidak dapat disahkan kembali.
     Buat versi baru dari salinannya."
   - Konsep yang masih punya penanda tinjauan seeder atau gagal validasi pita → `400`.
   - Dua pengesahan bersamaan untuk instrumen yang sama → index unik menolak yang kedua → `409`.
4. Pemensiunan tanpa pengganti → `PATCH /versions/{id}/retire` beralasan; hanya dari `Approved`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

State matrix 5.1, validation matrix 6.1, `SlidingScaleTemplateService.cs` (pola pengesahan), laporan
`BE-RWI-107`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Services/ClinicalInstrumentService.cs` | `ApproveVersionAsync`, `RetireVersionAsync` (ditulis bersama `BE-RWI-107`) |
| `Areas/HealthServices/ClinicalManagement/Controllers/ClinicalInstrumentController.cs` | `PATCH approve`, `PATCH retire` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai 7.2 |
| Database | `NOT APPLICABLE` — memakai tabel K1 |
| Keamanan/Auth | Kendali empat mata ditegakkan dari baris data, bukan nama peran |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Clinical Instrument

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/versions/{versionId}/approve` | Mengesahkan konsep; versi sah lama dipensiunkan pada transaksi yang sama | `ClinicalInstrumentConfiguration : Approve` |
| `PATCH` | `/versions/{versionId}/retire` | Memensiunkan versi sah tanpa pengganti | `ClinicalInstrumentConfiguration : Approve` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran proses bisnis kriteria 1–4 (termasuk percobaan pengesahan oleh pengubah terakhir) | Terpetakan pada source | `PASS` | Bagian 6 |
| `dotnet build` | `0 Error(s)`, `212 Warning(s)`, `00:06:56.91` — sama dengan garis dasar 212; nol warning dari berkas baru | `PASS` | Lampiran |
| Uji runtime pengesahan oleh pengubah terakhir | Tidak dijalankan | `NOT RUN` | Build lolos; belum dijalankan pada putaran ini |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `Draft` → `Approved` → `Retired`; `Retired` tidak kembali | Terpenuhi | `ApproveVersionAsync` hanya dari `Draft`; `RetireVersionAsync` hanya dari `Approved`; `UpdateVersionAsync` hanya `Draft` |
| 2. Pengesah bukan pengubah terakhir | Terpenuhi | `versi.LastModifiedByUserId == actorUserId` → `403`; check constraint K1 |
| 3. Pengesahan memensiunkan versi sah sebelumnya pada transaksi yang sama | Terpenuhi | `BeginTransactionAsync` → pensiun → `SaveChanges` → sahkan → `SaveChanges` → `Commit` |
| 4. Hanya satu versi `Approved` per instrumen | Terpenuhi | Index `UX_CliClinicalInstrumentVersion_Approved` (filter `VersionStatus = 2`) + penanganan `DbUpdateException` → `409` |
| DoD: `dotnet build` | `dotnet build` lolos | `PASS` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengesahan juga menuntut penanda tinjauan seeder dikosongkan — perluasan yang menjaga `BE-RWI-107` kriteria 4 |
| Masalah yang diketahui | Pengesah isi instrumen belum ditunjuk (`RWI-OQ-056`) |
| Risiko tersisa | Verifikasi runtime API dan proses bisnis belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tercakup berkas `BE-RWI-107` |
| Langkah berikutnya | `BE-RWI-109` memakai versi sah |

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
