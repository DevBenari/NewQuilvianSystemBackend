# Laporan Perubahan Backend — `BE-HRD-403`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-403` |
| Judul | Hierarki unit kerja aman dari siklus dan endpoint pohon hierarki |
| Slice | `S-FIN-M05` / Unit Kerja (alias `FIN-HRD-05-BE-001`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.3, kartu `BE-HRD-403` |
| Trace | Instruksi Menu 05 `FIN-HRD-05-BE-001`; baseline `BE-HRD-029` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `LIGHT` — verifikasi source |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Organization` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Seluruh kriteria sudah ada di source sejak laporan Menu 05 (16 November), tetapi belum ada laporan tracked dan build berbukti. Task ini mencatat buktinya; tidak ditemukan cacat baru.

## 2. Proses bisnis

1. Petugas membuat/mengubah unit kerja dengan unit induk.
2. Unit induk = diri sendiri atau turunannya → ditolak (`IsDescendantAsync`, pesan perulangan); unit induk wajib aktif dan satu badan hukum; `LevelNumber` dihitung otomatis.
3. `GET organization-units/hierarchy?asOfDate=` mengambil seluruh unit sekali dari database dengan filter tanggal efektif, lalu menyusun pohon di memori (tanpa N+1).
4. `MstOrganizationUnit` tetap satu-satunya sumber Unit Kerja.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OrganizationUnitController.cs` (validasi parent, `IsDescendantAsync`, `hierarchy`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Tanpa migration; tanpa entity baru |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Organization Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/master-data/organization-units/hierarchy?asOfDate=` | Pohon unit kerja per tanggal | `OrganizationUnit : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: unit A diberi induk turunannya | Ditolak (perulangan) | `PASS` | `IsDescendantAsync` |
| Penelusuran: pohon per tanggal | Satu query + filter tanggal efektif | `PASS` | aksi `hierarchy` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Parent diri sendiri atau turunan ditolak | Terpenuhi | validasi parent |
| (2) Pohon hierarki bersumber dari DB dengan filter tanggal efektif | Terpenuhi | `asOfDate` |
| (3) Tidak ada entity unit baru | Terpenuhi | hanya `MstOrganizationUnit` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-404` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
