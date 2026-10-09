# Laporan Perubahan Backend — `BE-LAB-95`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-95` |
| Judul | Rencana ulang sesudah wadah dibatalkan dan pendahulu tanpa penanda |
| Slice | Gelombang `MVP-14a` (susulan) — temuan T1 `BE-LAB-94`, Amendment Pass putaran 29 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6at.4 |
| Trace | `LAB-DEC-229`; `LAB-REQ-023` butir 1–5; `AC-323`..`AC-325` |
| Contract version | `LAB-API-v1` **`r46`** bagian 41 — `approved` 2026-10-09 (`LAB-REQ-023`, *"setuju kelima butir"*) |
| Desain | [`02-backend-architecture.md`](../../../02-backend-architecture.md) bagian 30 |
| Dependency | `BE-LAB-94` ⚠ (berkas yang sama, working tree) |
| Klasifikasi | `LOW` — 1 berkas source diubah; nol migration, tabel, kolom, endpoint, DTO, izin, berkas frontend |
| Task mode | `BACKEND` |
| Wewenang | Source backend. Tanpa eksekusi database, tanpa deploy |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e6c0e451` (branch `yoga`) + working tree `BE-LAB-94` |
| Tanggal | 2026-10-09 |
| Status | ✅ **`SELESAI`** — harness InMemory **24/24** (6 baru + 18 regresi `BE-LAB-94`), build 0 error. Seluruh AC task ini bersifat logika service dan terbukti di harness. Tidak ada permukaan layar baru: kelima layar sudah membaca `LabExamination.Urgency` (verifikasi layar `MVP-14` tetap di langkah rilis `MVP-14c`) |

---

## Backend Governance Preflight

Sama dengan [`BE-LAB-94`](BE-LAB-94.md) (dibaca pada sesi yang sama, tidak berubah): `HealthServices` / `LaboratoryManagement`,
prefix `Lab` **`ACTIVE`**, keberlakuan `TOUCHED LEGACY`, nol entity baru. Dua record privat (`LabExaminationPlan` dari
`BE-LAB-94`, `LabUrgencyDecision` baru) hanya pembawa nilai di dalam service.

## 1. Masalah yang diperbaiki

| Jalur | Sebelum (`BE-LAB-94`) | Akibat |
| --- | --- | --- |
| Wadah **dibatalkan** lalu direncanakan ulang | Membaca permintaan | *Tandai Cito* atau pencabutan dokter pada wadah yang dibatalkan hilang (temuan T1) |
| Ambil ulang dengan pemeriksaan lama **tanpa penanda** | Disalin mentah | Pemeriksaan sebelum `BE-LAB-94` yang lahir Routine karena cacat menurunkan Routine palsu ke wadah pengganti |

## 2. Proses bisnis

1. dr. Arif menandai Leukosit CITO pukul 13.20.
2. Analis membatalkan wadah karena salah tabung, lalu merencanakan wadah baru.
3. Leukosit tetap **CITO**, dengan nama dr. Arif dan jam 13.20.

Bila dr. Arif sebelumnya **mencabut** CITO Hemoglobin, wadah baru tetap biasa walaupun permintaannya CITO.

Pemeriksaan yang tidak pernah disentuh dokter selalu mengikuti permintaannya. Aturan ini berlaku sama di rencana
pertama, rencana ulang, dan ambil ulang.

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Services/LabSpecimenService.cs` | **Record baru `LabUrgencyDecision`.** **Method baru `ResolveLatestMarkedUrgencyAsync`:** satu kueri proyeksi atas pemeriksaan pesanan itu untuk prosedur yang direncanakan (tidak terhapus), lalu per prosedur dipilih yang `CreateDateTime`-nya terbaru dan hanya dipakai bila berpenanda. Kueri dijalankan sebelum pemeriksaan baru dibuat. **`PlanAsync`:** keputusan dokter didahulukan, selain itu permintaan seperti `BE-LAB-94`. **`RequestRecollectionAsync`:** pemeriksaan lama berpenanda disalin; tanpa penanda dibaca dari permintaan lewat `ResolveOrderedUrgencyAsync` |

**Tidak berubah:**

- Migration `BE-LAB-94` (`LAB-REQ-023` butir 4).
- `CreateExaminationsAsync`, `MarkOrderedProceduresFulfilledAsync`, `LabExaminationService`.
- Controller, DTO, snapshot, dan frontend.

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

| Method | Path | Hak akses | Perubahan perilaku (`r46`) |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/laboratory-management/lab-specimens/by-order/{labOrderId}` | `LabSpecimen : Plan` | Pendahulu berpenanda pada pesanan yang sama → `urgency` dan penanda disalin |
| `POST` | `/api/v1/health-services/laboratory-management/lab-specimens/{id}/request-recollection` | `LabSpecimen : Accept` | Pemeriksaan lama tanpa penanda → `urgency` permintaan, penanda `null` |

Request, response, kode status, dan pesan galat tidak berubah.

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `AC-323` — Leukosit Cito berpenanda; wadah dibatalkan; rencana ulang | Cito, penanda dokter dan waktu **persis** | `PASS` |
| `AC-324` — Hemoglobin dicabut (berpenanda), permintaan Cito; wadah dibatalkan | Routine berpenanda pencabutan | `PASS` |
| `AC-325` — pendahulu Routine tanpa penanda (meniru data sebelum `BE-LAB-94`), permintaan Cito; rencana ulang | Cito, penanda kosong | `PASS` |
| `AC-325` — pendahulu tanpa penanda, permintaan biasa | Routine, penanda kosong | `PASS` |
| Butir 3 — ambil ulang dengan pemeriksaan lama Routine tanpa penanda, permintaan Cito | Cito, penanda kosong | `PASS` |
| Batas — pesanan lain dengan prosedur sama | Tidak mewarisi keputusan dokter pesanan pertama | `PASS` |
| Regresi `BE-LAB-94` (18 uji, termasuk ambil ulang dengan pencabutan dan *Tandai Cito*) | 18/18 | `PASS` |
| `dotnet build -p:RunAnalyzers=False` | 0 error, 1 menit 28 detik; server BE/FE dimatikan sebelumnya, dinyalakan lagi sesudahnya | `PASS` |
| Uji SQL | Tidak relevan — nol perubahan migration | — |

AUTOMATED TEST: PASS (harness 24/24; tinggal di scratchpad sesi — `LAB-RDY-C04`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status |
| --- | --- |
| `AC-323` | Terpenuhi |
| `AC-324` | Terpenuhi |
| `AC-325` (rencana ulang dan ambil ulang) | Terpenuhi |
| Regresi `BE-LAB-94` | Terpenuhi |
| DoD — build, laporan | Terpenuhi |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko tersisa | Tidak ada pada scope. Kueri pendahulu tambahan (satu proyeksi per rencana wadah, dibatasi pesanan itu) |
| Perubahan sampingan | `NONE` |
| Status Git | Backend: `LabSpecimenService.cs` `M` (gabungan `BE-LAB-94` + `BE-LAB-95`), migration `??` + dokumen — belum di-commit |
| Langkah berikutnya | Deploy backend `BE-LAB-94` + `BE-LAB-95` bersamaan (tanpa migration tambahan), lalu langkah rilis `MVP-14c` langkah 5 (cek layar) |
