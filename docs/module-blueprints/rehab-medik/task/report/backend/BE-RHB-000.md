# Laporan Perubahan Backend — `BE-RHB-000`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-000` |
| Judul | Prasyarat governance registry (bukan task kode) |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-000` |
| Trace | RHB-DEC-012, RHB-DEC-014; QBE-MOD-002, QBE-MOD-003, QBE-NAM-004 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | — |
| Klasifikasi | `LIGHT` — verifikasi governance, tanpa perubahan kode permanen |
| Task mode | `BACKEND` |
| Target tulis | Probe sementara di `Areas/HealthServices/` (dihapus); laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) |
| Tanggal | 2026-11-05 |
| Status | **Selesai** |

## 1. Masalah yang diselesaikan

Sebelum model `Rhb*` pertama boleh dibuat, harus terbukti bahwa checker QBE mengenali folder
`Areas/HealthServices/RehabilitationManagement/` sebagai milik baris registry `Rhb`. Tanpa
bukti itu, setiap entity rehab berisiko ditolak `QBE-MOD-002` di pemeriksaan pull request.

## 2. Proses

1. Baris registry `HealthServices / RehabilitationManagement / Rehabilitation / BUSINESS DOMAIN /
   MODULE / Rhb / ACTIVE` ditulis 2026-11-05 (RHB-DEC-014), lengkap dengan kepanjangan prefix
   dan catatan perubahan lifecycle.
2. Dibuat dua probe sementara, masing-masing kelas ber-`[Table]` turunan `IdentityModel`:
   - `Areas/HealthServices/RehabilitationManagement/Models/RhbRegistryProbe.cs`
   - kontrol negatif `Areas/HealthServices/ZzzUnregisteredProbe/Models/ZzzRegistryProbe.cs`
3. Checker dijalankan dua kali secara terpisah (`-Path`, `ReportOnly`).
4. Kedua probe beserta foldernya dihapus.

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| (tidak ada) | Probe dibuat lalu dihapus. Tidak ada perubahan kode permanen. |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path .../RehabilitationManagement/Models/RhbRegistryProbe.cs -Mode ReportOnly` | `Files evaluated: 1`, `VIOLATION: 1` = hanya **QBE-CFG-001** (probe sengaja tanpa configuration). **Tidak ada QBE-MOD-002.** `Final result: PASS`. | `PASS` | Keluaran checker 2026-11-05 |
| `./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path .../ZzzUnregisteredProbe/Models/ZzzRegistryProbe.cs -Mode ReportOnly` | `VIOLATION: 2` = **QBE-CFG-001** dan **QBE-MOD-002** ("Obtain registry decision; do not infer a prefix") | `PASS` (kontrol negatif tertangkap seperti seharusnya) | Keluaran checker 2026-11-05 |
| `git status --short Areas/` sesudah probe dihapus | Hanya `M Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs` (perubahan `BE-RHB-003`, disengaja) | `PASS` | Keluaran git |
| `dotnet build` | — | `NOT APPLICABLE` | Tidak ada kode permanen yang berubah |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT APPLICABLE`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Checker QBE tidak melaporkan QBE-MOD-002 untuk probe `Rhb*` di folder `RehabilitationManagement` | Terpenuhi | §5 baris 1 |
| (2) Kontrol negatif `Zzz*` di folder tak terdaftar tetap terkena | Terpenuhi | §5 baris 2 |
| (3) Probe dihapus, dan `git status --short Areas/` bersih | Terpenuhi (hanya perubahan `BE-RHB-003` yang disengaja) | §5 baris 3 |
| DoD: bukti checker tercatat | Terpenuhi | Laporan ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | QBE-CFG-001 pada probe tidak relevan bagi kriteria. Model nyata di `BE-RHB-001` wajib punya configuration khusus. |
| Masalah yang diketahui | Konfirmasi pemegang registry dan penyelarasan salinan registry di suite skill belum dilakukan. Keduanya tidak memblokir checker repository ini. |
| Risiko tersisa | — |
| Perubahan sampingan | `NONE` (probe dihapus) |
| Interupsi | `NONE` |
| Status Git | `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `M Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs`, `?? docs/module-blueprints/rehab-medik/` |
| Langkah berikutnya | `BE-RHB-001`: fondasi 20 model, enum, configuration, DbSet, konstanta |
