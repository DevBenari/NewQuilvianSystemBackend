# Laporan Perubahan Backend — `BE-HRD-413`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-413` |
| Judul | Uraian tugas pegawai per tanggal acuan |
| Slice | `S-FIN-M07` / Uraian Tugas (alias `FIN-HRD-07-BE-007`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-413` |
| Trace | Instruksi Menu 07 `FIN-HRD-07-BE-007` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | `BE-HRD-405` ✅ |
| Klasifikasi | `LIGHT` — verifikasi source |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; migration `20261120091000` sudah diterapkan 21 November 2026 ke DB dev; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Query sudah membaca penempatan efektif per tanggal acuan tanpa filter `IsActive` dan tanpa jatuh ke `PrimaryPositionId`; yang menahan hanya tabel `MstJobDescription` (kini dibuat migration `BE-HRD-404`).

## 2. Proses bisnis

1. `GET …/employees/{workforceProfileId}/effective-job-description?asOfDate=` mencari penempatan jabatan yang berlaku pada tanggal acuan (aktif maupun sudah berakhir).
2. Tanggal sebelum penempatan pertama → `positionId = null`, tanpa uraian tugas.
3. Bila ada penempatan, versi uraian tugas jabatan yang berlaku pada tanggal itu dikembalikan beserta riwayat versinya.

**Contoh.** Perawat mutasi 1 Juli 2026 dari IGD ke ICU; `asOfDate=2026-03-01` → uraian tugas jabatan IGD walau penempatan IGD sudah tidak aktif hari ini.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkforceCoreCrossEmployeeController.cs` (aksi `effective-job-description`) dan query pendukungnya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Bergantung migration `BE-HRD-404` |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Cross-Employee Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/employees/{workforceProfileId}/effective-job-description?asOfDate=` | Uraian tugas pegawai per tanggal | hak existing endpoint |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: tanggal sebelum penempatan | `positionId = null` | `PASS` | query per tanggal |
| Penelusuran: penempatan lama tidak aktif | Tetap terbaca | `PASS` | tanpa filter `IsActive` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tanggal sebelum penempatan → `positionId=null` | Terpenuhi | query per tanggal |
| (2) Penempatan lama tetap terbaca walau tidak aktif hari ini | Terpenuhi | tanpa filter `IsActive` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Wajib sebelum dipakai | Jalankan migration `20261120091000_AddMstJobDescription` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |


## Pembaruan 21 November 2026 — Status Migration

Migration `20261120091000_AddMstJobDescription` sudah diterapkan ke database pengembangan `QuilvianNewDevHamzah`: pemilik menjalankan `dotnet ef database update --no-build` ("No migrations were applied. The database is already up to date.") dan `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` tidak menunjukkan satu pun migration `Pending`. Pernyataan "migration belum dijalankan" dan "wajib dijalankan sebelum dipakai" pada bagian di atas sudah tidak berlaku untuk DB dev tersebut; database lain (staging/production) tetap wajib menjalankannya sebelum fitur dipakai. Status task tetap ✅. Uji API di DB dev tetap `NOT RUN`.
