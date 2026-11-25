# Laporan Perubahan Backend — `BE-HRD-412`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-412` |
| Judul | Integritas batch penempatan awal dan integrasi onboarding |
| Slice | `S-FIN-M07` / Penempatan Awal (alias `FIN-HRD-07-BE-006`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.4, kartu `BE-HRD-412` |
| Trace | `HRD-DEC-054`; instruksi Menu 07 `FIN-HRD-07-BE-006` |
| Contract version | Kontrak SK Kepegawaian dan penempatan existing (`BE-HRD-024`, `BE-HRD-027`); tidak ada endpoint baru |
| Dependency | — |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Services/HrdInitialPlacementBatchService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Validasi memakai hak `Update`, batch yang sudah diajukan tidak dapat divalidasi ulang, satu pegawai sekali per batch disetujui, eksekusi all-or-nothing, dan gaji disamarkan di detail. Belum terbukti: penguncian database untuk **dua approver serentak** — keduanya membaca `PendingApproval` dan sama-sama menyetujui, dan dua batch berisi pegawai yang sama dapat lolos pemeriksaan sekali-per-pegawai bersamaan. Eksekusi ganda serentak juga belum dikunci.

## 2. Proses bisnis

1. `Draft → Validated → PendingApproval → Approved → Executed`; batch yang sudah diajukan tidak dapat diubah.
2. Persetujuan: di dalam transaksi diambil kunci global persetujuan batch, status dibaca ulang, dan konflik pegawai diperiksa ulang sesudah kunci; pembuat/pengaju tidak boleh menyetujui.
3. Eksekusi: di dalam transaksi diambil kunci per batch dan status dibaca ulang; satu baris invalid → seluruh batch ditolak (0 penempatan).
4. Penempatan bertanggal efektif masa depan tidak mengubah mirror profil sebelum waktunya (perilaku existing).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdInitialPlacementBatchService.cs` (`ValidateBatchByIdAsync`, `SubmitBatchAsync`, `ApproveBatchAsync`, `ExecuteBatchAsync`), controller batch (`/validate` hak `Update`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs` | Persetujuan dalam transaksi + kunci global + baca ulang status + cek konflik ulang; eksekusi dengan kunci per batch + baca ulang status |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; persetujuan/eksekusi kedua yang serentak ditolak |
| Database | Tanpa migration |
| Keamanan/Auth | Hak tidak berubah (`/validate` tetap `Update`) |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Initial Placement Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/approve` | Setujui batch tepat sekali | hak existing |
| `POST` | `…/initial-batches/{id}/execute` | Eksekusi all-or-nothing tepat sekali | hak existing |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan; dua warning nullable dari build pertama diperbaiki sebelum build final) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | 3 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: peran Read-only memanggil `/validate` | `403` (hak `Update`) | `PASS` | atribut controller |
| Penelusuran: satu baris invalid | Eksekusi ditolak, 0 penempatan | `PASS` | `ExecuteBatchAsync` |
| Penelusuran: dua approver serentak | Satu berhasil, satu ditolak | `PASS` | kunci global + baca ulang |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Peran Read-only mendapat `403` pada `/validate` | Terpenuhi | `[AccessPermission(..., "Update")]` |
| (2) Satu baris invalid → 0 penempatan | Terpenuhi | validasi ulang + transaksi |
| (3) Tanggal efektif masa depan tidak mengubah mirror | Terpenuhi | logika penerapan existing |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | `BE-HRD-413` s.d. `415` (bergantung migration Uraian Tugas `BE-HRD-404`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
