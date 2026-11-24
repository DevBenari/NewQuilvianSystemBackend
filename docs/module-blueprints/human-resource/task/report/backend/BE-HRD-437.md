# Laporan Perubahan Backend — `BE-HRD-437`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-437` |
| Judul | Rekredensialing H-60 dan siklus SPK/RKK |
| Slice | `S-FIN-M09` / Masa Berlaku Kewenangan (alias `FIN-HRD-09-BE-012`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-437` |
| Trace | M09-P0-11; instruksi Menu 09 `FIN-HRD-09-BE-012` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-433` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `SpkRkkExpiryMonitoringWorker`, `CredentialingWorklistQueryService`, `CredentialingWorklistDtos`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji worker `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Draf H-60 bernomor `RECRED/{tahun}/{Count+1}` — dua SPK dalam satu tick mendapat nomor sama; draf yang sudah ditutup dibuat ulang pada siklus yang sama; `CycleNumber` selalu 1; worker tidak berkunci antar-instance; draf H-60 hanya dibuat bila ada SPK yang baru kedaluwarsa pada tick itu; tidak ada penanda kesiapan OPPE.

## 2. Proses bisnis

1. Tick worker (12 jam) dikunci sesi PostgreSQL `HRD_SPK_RKK_EXPIRY_TICK`; instance lain melewati tick.
2. SPK `Active` yang lewat masa → `Expired`; kewenangan hanya ditangguhkan bila tidak ada SPK aktif baru.
3. Untuk SPK yang jatuh tempo ≤ 60 hari: satu draf per siklus (dikenali dari `DueDate = ValidUntil`), tidak dibuat ulang walau draf lama sudah ditutup. Nomor `RECRED/{tahun}/{NNNN}` dari allocator; `CycleNumber` = siklus terakhir + 1.
4. Draf menyimpan `ComplianceSnapshotJson` berisi `OppeReadiness` (`OPPE_READY` bila ada evaluasi OPPE final yang periodenya berakhir dalam siklus SPK, selain itu `DATA_BELUM_CUKUP`), jumlah evaluasi, dan skor terakhir — tanpa skor contoh.
5. Radar (`GET …/recredentialing/cycles`) menampilkan `OppeReadiness` dan `OppeEvaluationsInCycle`.
6. Status administratif pegawai tidak disentuh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`SpkRkkExpiryMonitoringWorker.cs`, `CredentialingWorklistQueryService.GetRecredentialingCyclesAsync`, model `HrdOppeStaffEvaluation`/`HrdOppeEvaluationPeriod`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Workers/SpkRkkExpiryMonitoringWorker.cs` | Kunci antar-instance; satu draf per siklus; nomor allocator; `CycleNumber`; snapshot kesiapan OPPE; H-60 tetap diperiksa walau tak ada SPK kedaluwarsa |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingWorklistQueryService.cs` | Radar: kesiapan OPPE per siklus |
| `Areas/Corporate/HumanResource/CredentialingManagement/DTOs/CredentialingWorklistDtos.cs` | `OppeReadiness`, `OppeEvaluationsInCycle`, `OppeReadinessStatus` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Radar menambah field `oppeReadiness`, `oppeEvaluationsInCycle` (aditif) |
| Database | Tanpa migration |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### HR Performance - Recredentialing Quality

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/hrd/credentialing/recredentialing/cycles` | Radar rekredensial + kesiapan OPPE | `RecredentialingQuality : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: dua SPK dalam satu tick | Dua nomor berbeda | `PASS` | allocator |
| Penelusuran: draf siklus sudah ditutup | Tidak dibuat ulang | `PASS` | cek `DueDate` |
| Penelusuran: tanpa OPPE final | `DATA_BELUM_CUKUP` | `PASS` | snapshot/radar |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) H-60 membuat maksimal satu draf | Terpenuhi | kunci antar-instance + satu draf per siklus |
| (2) OPPE tidak direkayasa | Terpenuhi | hanya evaluasi final nyata; kosong → `DATA_BELUM_CUKUP` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak frontend | Radar dapat menampilkan label kesiapan OPPE |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
