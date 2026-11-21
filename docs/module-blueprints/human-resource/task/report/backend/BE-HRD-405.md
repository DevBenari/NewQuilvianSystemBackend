# Laporan Perubahan Backend — `BE-HRD-405`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-405` |
| Judul | Service, API, dan RBAC Uraian Tugas dengan aktivasi transaksional |
| Slice | `S-FIN-M05` / Uraian Tugas (alias `FIN-HRD-05-BE-003`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.3, kartu `BE-HRD-405` |
| Trace | Instruksi Menu 05 `FIN-HRD-05-BE-003` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | `BE-HRD-404` 🟡 (migration ditulis) |
| Klasifikasi | `LIGHT` — verifikasi source + penjaga DB lewat `BE-HRD-404` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpetakan ke source; build dan QBE lulus; migration `20261120091000` sudah diterapkan 21 November 2026 ke DB dev; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Organization` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Service, API, dan RBAC sudah ada; audit 19 November mencatat rute di laporan Menu 05 tidak sama dengan controller. Laporan ini memuat rute yang benar dari controller dan bukti build.

## 2. Proses bisnis

1. Buat draft (`POST draft`) — satu draft per jabatan; nomor versi = versi tertinggi jabatan + 1, dilindungi index unik `(PositionId, VersionNumber)` dan index unik parsial draft.
2. Ubah draft (`PUT {id}/draft`) — hanya status `Draft`.
3. Aktifkan (`POST {id}/activate`) — minimal satu Tugas Pokok; dalam satu transaksi versi aktif lama diarsipkan dan ditutup ke H-1.
4. Nonaktifkan, hapus draft (versi aktif tidak dapat dihapus → `409`).
5. Kode status: `400` validasi, `403` hak akses, `404` tidak ditemukan, `409` konflik status/draft ganda. Aktor dari token.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`JobDescriptionController.cs`, `JobDescriptionService.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah; rute terdokumentasi di bagian 4 sama dengan controller |
| Database | Bergantung migration `BE-HRD-404` |
| Keamanan/Auth | `JobDescription : Read/Create/Update/Delete` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Job Description

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/master-data/job-descriptions` | Daftar | `JobDescription : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/job-descriptions/summary` | Ringkasan | `JobDescription : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/job-descriptions/{id}` | Detail | `JobDescription : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/job-descriptions/position/{positionId}/effective` | Versi efektif per jabatan | `JobDescription : Read` |
| `GET` | `/api/v1/corporate/human-resource/master-data/job-descriptions/position/{positionId}/history` | Riwayat versi | `JobDescription : Read` |
| `POST` | `/api/v1/corporate/human-resource/master-data/job-descriptions/draft` | Buat draft | `JobDescription : Create` |
| `PUT` | `/api/v1/corporate/human-resource/master-data/job-descriptions/{id}/draft` | Ubah draft | `JobDescription : Update` |
| `POST` | `/api/v1/corporate/human-resource/master-data/job-descriptions/{id}/activate` | Aktifkan versi | `JobDescription : Update` |
| `POST` | `/api/v1/corporate/human-resource/master-data/job-descriptions/{id}/deactivate` | Nonaktifkan | `JobDescription : Update` |
| `DELETE` | `/api/v1/corporate/human-resource/master-data/job-descriptions/{id}` | Hapus draft | `JobDescription : Delete` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: aktivasi ganda | Satu versi aktif (transaksi + index unik parsial) | `PASS` | `ActivateAsync` + migration |
| Penelusuran: draft kedua untuk jabatan sama | `409` | `PASS` | `CreateDraftAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Aktivasi ganda tidak membuat dua versi aktif | Terpenuhi | transaksi pengarsipan + index unik parsial |
| (2) `403`/`400`/`404`/`409` sesuai kasus | Terpenuhi | atribut akses + kode hasil service |
| (3) Aktor audit dari server | Terpenuhi | `actorId` dari token |
| (4) Route terdokumentasi sama dengan controller | Terpenuhi | bagian 4 laporan ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan frontend | `job-description-service.js` memanggil rute yang tidak ada di controller; perlu diselaraskan di task frontend Menu 05 |
| Wajib sebelum dipakai | Jalankan migration `20261120091000_AddMstJobDescription` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |


## Pembaruan 21 November 2026 — Status Migration

Migration `20261120091000_AddMstJobDescription` sudah diterapkan ke database pengembangan `QuilvianNewDevHamzah`: pemilik menjalankan `dotnet ef database update --no-build` ("No migrations were applied. The database is already up to date.") dan `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` tidak menunjukkan satu pun migration `Pending`. Pernyataan "migration belum dijalankan" dan "wajib dijalankan sebelum dipakai" pada bagian di atas sudah tidak berlaku untuk DB dev tersebut; database lain (staging/production) tetap wajib menjalankannya sebelum fitur dipakai. Status task tetap ✅. Uji API di DB dev tetap `NOT RUN`.
