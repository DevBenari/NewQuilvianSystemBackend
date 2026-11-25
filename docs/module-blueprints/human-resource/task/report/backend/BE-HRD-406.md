# Laporan Perubahan Backend — `BE-HRD-406`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-406` |
| Judul | Guard Atasan Langsung: Menu 05 hanya membaca `WfpManagerAssignment` |
| Slice | `S-FIN-M05` / Atasan Langsung (alias `FIN-HRD-05-BE-004`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.3, kartu `BE-HRD-406` |
| Trace | Instruksi Menu 05 `FIN-HRD-05-BE-004` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | `BE-HRD-403` ✅, `BE-HRD-405` ✅ |
| Klasifikasi | `LIGHT` — verifikasi source |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji izin `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Organization` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Tidak ada jalur tulis atasan dari Menu 05 sejak laporan Menu 05; task ini mencatat bukti dan memastikan hak baca lintas pegawai dihormati.

## 2. Proses bisnis

1. Menu 05 membaca relasi atasan lewat `GET …/workforce-core/manager-assignments` (rekap lintas pegawai, hak `WfpManagerAssignment : ReadAll`).
2. Tidak ada endpoint tulis atasan di controller master Organization; perubahan atasan lewat SK kategori `ManagerChange` (`BE-HRD-414`) atau data awal terkendali (`BE-HRD-410`).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh controller `MasterData/Organization/Controllers` (tidak ada rujukan `WfpManagerAssignment`), `WorkforceCoreCrossEmployeeController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `WfpManagerAssignment : ReadAll` pada rekap lintas pegawai |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Cross-Employee Administration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-core/manager-assignments` | Rekap relasi atasan lintas pegawai | `WfpManagerAssignment : ReadAll` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Pencarian rujukan `WfpManagerAssignment` di controller Organization | Tidak ada | `PASS` | grep source |
| Atribut akses rekap atasan | `[AccessPermission("WfpManagerAssignment", "ReadAll")]` | `PASS` | controller |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tidak ada endpoint tulis atasan di master Organization | Terpenuhi | pencarian source |
| (2) Izin `WfpManagerAssignment : ReadAll` dihormati | Terpenuhi | atribut akses |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-413` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
