# Laporan Perubahan Backend — `BE-RWI-095`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-095` |
| Judul | Verifikasi CPPT pada episode yang sudah ditutup |
| Slice | Gelombang 3 — `DOK-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-095` |
| Trace | `FR-DOK-083`; `RWI-DEC-125`, `RWI-DEC-126`; `VAL-DOK-44`, `44a`, `44b` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-094`, `BE-RWI-089` — implementasi tersedia pada working tree aktif |
| Klasifikasi | `HEAVY` — kewenangan klinis pada episode tertutup dan pembatasan waktu entri |
| Task mode | `BACKEND` |
| Model | GPT-5 |
| Commit backend saat dikerjakan | `36db5e6d1f1e6ee8b3a3dce8aa6c9d4adcc8060a`, branch `MHamzah` |
| Tanggal | 2026-09-16 |
| Status | ✅ **SELESAI 16 September 2026** berdasarkan validasi source; `dotnet build` tidak dijalankan sesuai instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / ClinicalManagement` |
| QBE relevan | `QBE-SVC-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-LOG-001` |
| Penempatan aturan | Keputusan kewenangan berada pada `CpptVerificationService`; controller tidak menduplikasi aturan |
| Boundary | Pengecualian hanya `EpisodeStatus.Closed`; episode `Cancelled` tidak diperlakukan sebagai closed exception |

## 1. Proses bisnis

Pada episode berjalan, verifikasi tetap hanya oleh DPJP yang aktif pada detik verifikasi. Pada
episode `Closed`, service mencari satu DPJP terakhir berdasarkan penugasan DPJP terbaru. Dokter itu
hanya dapat memverifikasi entri yang waktu klinisnya tidak melewati `ClosedAt`.

Pengecualian ini tidak mengubah jalur pembuatan dokumen. Penulisan dokumen baru tetap melewati
`InpatientClinicalContextService` dengan `forNewDocument: true`, sehingga episode tertutup ditolak.

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `CpptVerificationService.cs` | Cabang kewenangan untuk episode `Closed`; DPJP terakhir; penolakan entri pascapenutupan; `Cancelled` ditolak; `VerificationDueAt` dipertahankan |
| `InpatientClinicalContextService.cs` | `FindLastAttendingDoctorIdAsync`; tie-breaker waktu mulai, sequence, dan waktu pembuatan; assignment dibatalkan tidak dihitung |

Verifikasi hanya mengubah `VerificationStatus`, `VerifiedAt`, `VerifiedByUserId`, dan audit update.
Penulis asli, isi catatan, serta `VerificationDueAt` tidak diubah.

## 3. Dokumentasi endpoint

| Method | Path | Perilaku | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/api/v1/health-services/clinical-management/patient-integrated-progress-notes/{id}/verify` | Episode berjalan: DPJP aktif. Episode `Closed`: hanya DPJP terakhir dan hanya entri prapenutupan | `PatientIntegratedProgressNote : Verify` |

## 4. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| DPJP terakhir | **PASS statis** — service memilih tepat satu assignment DPJP terakhir |
| DPJP sebelumnya | **PASS statis** — ID dokter harus sama dengan hasil pencarian terakhir; selain itu `403` |
| Keterlambatan | **PASS statis** — `VerificationDueAt` tidak dikosongkan atau digeser saat verify |
| Catatan baru pada closed episode | **PASS statis** — jalur create tetap memakai closed guard terpisah |
| Entri pascapenutupan | **PASS statis** — `NoteDateTime > ClosedAt` menghasilkan `422` |
| `ClosedAt` tidak tercatat | **PASS statis/fail closed** — verifikasi ditolak `422` karena batas prapenutupan tidak dapat dibuktikan |
| Episode `Cancelled` | **PASS statis** — bukan closed exception dan menghasilkan `422` |
| QBE checker `Strict` | **PASS** — nol finding |
| `dotnet build` | **NOT RUN — instruksi eksplisit pengguna** |
| Uji proses bisnis runtime | **NOT RUN** |

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — DPJP terakhir dapat memverifikasi entri prapenutupan | Terpenuhi secara statis | Cabang `Closed` dan resolver DPJP terakhir |
| AC-2 — DPJP sebelumnya `403` | Terpenuhi secara statis | Perbandingan exact doctor ID |
| AC-3 — keterlambatan tetap tercatat | Terpenuhi secara statis | Due time dipertahankan setelah verify |
| AC-4 — tidak membuka catatan baru | Terpenuhi secara statis | Tidak ada perubahan create guard |
| AC-5 — entri pascapenutupan tidak dicakup | Terpenuhi secara statis | Penolakan `422` berdasarkan `ClosedAt` |
| Laporan, roadmap, traceability | Terpenuhi | Laporan ini dan pembaruan dokumen delivery |

## 6. Catatan penutup

Task ditandai selesai tanpa build atas instruksi pemilik. Verifikasi runtime masih perlu dilakukan
saat pemilik menjalankan build mandiri. Tidak ada perubahan database, deployment, atau operasi Git.
