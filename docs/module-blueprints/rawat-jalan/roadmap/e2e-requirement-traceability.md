# Traceability — Rawat Jalan ke Invoice Canonical (V2)

| Field | Nilai |
|---|---|
| Blueprint | `RJ-BIL-BP-001` revisi `27` |
| Roadmap | `e2e-backend-roadmap.md` rev `1`, `e2e-frontend-roadmap.md` rev `1` — `OWNER_APPROVED` (`RJ-E2E-DEC-017`) |
| Kontrak | `RJ-E2E-CONTRACT-001@1.0.0`, `@1.0.1`, `@1.0.2` — seluruhnya `approved` |
| Approval desain | `RJ-E2E-DEC-015` — Sukma Giri, 2026-09-28 |
| Backend / frontend SHA | `063d38b` (`sukmagp`) / `83b8b72` (`sukmagpV2`) |
| Terpisah dari | `requirement-traceability.md` (scope `RJ-BIL`/`RJ-DOC`) |

## 1. Functional requirement → task → bukti

| Requirement | Keputusan | Desain | Task BE | Task FE | Bukti (UAT / acceptance matrix V2) | Status |
|---|---|---|---|---|---|---|
| `FR-RJE-001` kolom sinkron | `DEC-003` | `02` V2.9 | `BE-RJE-001` | — | `UAT-22`; V2-4 baris `DEC-014` | ✅ `BE-RJE-001` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-001.md) R0–R10 |
| `FR-RJE-002` kebijakan kirim ulang | `DEC-009` | `02` V2.11 | `BE-RJE-001` | — | V2-4 `SYNC_POLICY_INACTIVE` | ✅ `BE-RJE-001` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-001.md) R0–R10 |
| `FR-RJE-003` kontrak `1.3` | `DEC-001`, `005` | `02` V2.7.3 | `BE-RJE-002` | — | `UAT-23` | ✅ `BE-RJE-002` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-002.md) R0–R13 |
| `FR-RJE-004` baris lama | `DEC-014` | `02` V2.10 | `BE-RJE-001` | — | `UAT-22` | ✅ `BE-RJE-001` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-001.md) R0–R10 |
| `FR-RJE-010` kelayakan | `DEC-003` | `02` V2.7.1 | `BE-RJE-003` | — | V2-2 `REPEAT_INTERNAL_ERROR`, `NOT_OUTPATIENT` | ✅ `BE-RJE-003` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-003.md) |
| `FR-RJE-011` identitas stabil | `AC-RJ-002/003` | `02` V2.7.2 | `BE-RJE-003` | — | V2-1 `AC-RJ-002` | ✅ `BE-RJE-003` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-003.md) |
| `FR-RJE-012` harga katalog | `DEC-006` | `02` V2.7.4 | `BE-RJE-003` | — | `UAT-01`; V2-1 snapshot Rp99.999 | ✅ `BE-RJE-003` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-003.md) |
| `FR-RJE-013` tanpa Rp0 | `DEC-006` | `02` V2.7.4 | `BE-RJE-003` | — | `UAT-02` | ✅ `BE-RJE-003` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-003.md) |
| `FR-RJE-014` pengaman `from-source` | `DEC-006` | `02` V2.7.6 | `BE-RJE-004` | — | `UAT-03` | ✅ `BE-RJE-004` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-004.md) R0–R4 |
| `FR-RJE-015` pasca-final | `RJ-BIL-DEC-004` | `02` V2.7.5 | `BE-RJE-005` | — | `UAT-04` | ✅ `BE-RJE-005` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-005.md) R0–R5 |
| `FR-RJE-020` fakta konsultasi | `DEC-001` | `contracts/integration` V2-2 | `BE-RJE-007` | — | `UAT-05` | ✅ `BE-RJE-007` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-007.md) R0–R5 |
| `FR-RJE-021` tarif konsultasi | `DEC-012`, `DEC-023` | `02` V2.7.4 | `BE-RJE-007` | — | `UAT-05`, `UAT-06` | ✅ `BE-RJE-007` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-007.md) R0–R5 |
| `FR-RJE-030` obat tahap 1 | `DEC-005` | `02` V2.7.2 | `BE-RJE-008` | — | `UAT-07` | Belum dikerjakan |
| `FR-RJE-031` obat tahap 2 | `DEC-005` | `contracts/integration` V2-2 | `BE-RJE-008` | — | `UAT-08` | Belum dikerjakan |
| `FR-RJE-032` deadlock hilang | `DEC-005` | `02` V2.7.3 | `BE-RJE-008` | — | `UAT-07`, `UAT-09` | Belum dikerjakan |
| `FR-RJE-040` void | `DEC-010` | `02` V2.7.5 | `BE-RJE-009` | — | `UAT-10` | Belum dikerjakan |
| `FR-RJE-041` adjustment koreksi | `DEC-010` | `02` V2.7.5 | `BE-RJE-009` | — | `UAT-11` | Belum dikerjakan |
| `FR-RJE-042` tanpa pembatalan palsu | `AC-RJ-011` | `flowcharts/pembatalan-dan-koreksi` | `BE-RJE-009` | — | `UAT-12` | Belum dikerjakan |
| `FR-RJE-050` kirim ulang fakta | `DEC-009`, `AC-RJ-014` | `contracts/integration` V2-3 | `BE-RJE-011` | — | V2-4 `AC-RJ-014` | Belum dikerjakan |
| `FR-RJE-051` kirim ulang invoice | `DEC-009` | `02` V2.11 | `BE-RJE-010` | — | `UAT-13`, `UAT-14` | Belum dikerjakan |
| `FR-RJE-052` API antrean | `DEC-009` | `contracts/api` V2 | `BE-RJE-012` | — | `UAT-15`, `UAT-16` | Belum dikerjakan |
| `FR-RJE-053` layar antrean | `DEC-009` | `03` V2.4 | — | `FE-RJE-003` | `UAT-15`, `UAT-16` | Belum dikerjakan |
| `FR-RJE-054` API kebijakan | `DEC-009` | `contracts/api` V2 | `BE-RJE-012` | — | V2-4 `RJE-VAL-031` | Belum dikerjakan |
| `FR-RJE-060` endpoint ringkasan | `DEC-008` | `contracts/api` V2 | `BE-RJE-014` | — | `UAT-17`, `UAT-18` | Belum dikerjakan |
| `FR-RJE-061` tab ringkasan | `DEC-008`, `FE-001/002` | `03` V2.4 | — | `FE-RJE-001` | `UAT-17`, `UAT-18`, `UAT-24` | Belum dikerjakan |
| `FR-RJE-062` pemberitahuan penyerahan | `FE-004` | `contracts/api` V2.1 | `BE-RJE-013` ⛔ | `FE-RJE-002` ⛔ | `UAT-19` | ⛔ menunggu kontrak `1.0.1` |
| `FR-RJE-070` kunjungan tanpa dokter | `DEC-007`, `013` | `02` V2.8 | `BE-RJE-006` | — | `UAT-20`, `UAT-21` | ✅ `BE-RJE-006` 2026-09-28 — [laporan](../task/report/backend/BE-RJE-006.md) R0–R3 |

`DEC-*` di tabel ini singkatan `RJ-E2E-DEC-*`; `FE-00n` singkatan `RJ-E2E-FE-00n`.

## 2. Acceptance PRD → task

| PRD | Task | Bukti |
|---|---|---|
| `AC-RJ-001` satu invoice per kunjungan | `BE-RJE-003` | V2-1 `AC-RJ-001` (Concurrency) |
| `AC-RJ-002`, `003` tanpa efek ganda | `BE-RJE-003`, `BE-RJE-007`, `BE-RJE-010` | V2-1; `UAT-05`, `UAT-13` |
| `AC-RJ-004`..`006` tindakan, Lab, Radiologi | `BE-RJE-003` | `UAT-01`, V2-1 |
| `AC-RJ-007` obat final setelah serah | `BE-RJE-008` | `UAT-07`, `UAT-08` |
| `AC-RJ-008` jasa konsultasi | `BE-RJE-007` | `UAT-05` |
| `AC-RJ-009`, `010` angka dari Billing | `BE-RJE-014`, `FE-RJE-001` | `UAT-24` |
| `AC-RJ-011`, `012` pembatalan | `BE-RJE-009` | `UAT-10`..`12` |
| `AC-RJ-013` klinis tidak batal | `BE-RJE-003`, `BE-RJE-011` | V2-4 `AC-RJ-013` |
| `AC-RJ-014` tanpa kunci baru | `BE-RJE-011` | V2-4 |
| `AC-RJ-015` akses manual ditolak | `BE-RJE-014` | `UAT-18` |
| `SEC-RJ-004` XSS | `FE-RJE-001`, `FE-RJE-003` | V2-5 `SEC-RJ-004` |
| `RJ-E2E-DEC-002` biaya admin | `BE-RJE-003` (AC 7) | V2-2 |

## 3. Coverage gap

| Gap | Sebab | Penanganan |
|---|---|---|
| E2E utuh dokter → kasir → farmasi | Tidak dimiliki satu task pun | `verify-module-readiness` setelah `MVP-4` |
| `FR-RJE-062` | Kontrak `1.0.1` belum disetujui | Approval pemilik |
| `RJ-DOC-BE-005` AC `1`–`4` | Milik scope Dokter | `doctor-consultation-roadmap.md`; wajib memakai ulang `RedispatchAsync` dari `BE-RJE-011` |
| Uji `403` dengan akun tanpa butir | Bergantung ketersediaan akun uji | Dicatat per laporan task; `NOT RUN` bila akun tidak tersedia |
