# Pengadaan Barang & Jasa — Status Modul

| Field | Nilai |
| --- | --- |
| Blueprint ID | `PBJ-BP-001` |
| Nama modul | Pengadaan Barang & Jasa |
| Revisi | `2` |
| Status modul | `PARTIAL` (diturunkan: 1 sub-modul approved, 3 belum dimulai) |
| Fase saat ini | `PBJ-PH-007` audit kesiapan `perencanaan-permintaan`: `READY_WITH_CONDITIONS`, sign-off menunggu syarat K-1..K-5; sub-modul lain belum dimulai |
| Terakhir diverifikasi | 17 November 2026 — `verify-module-readiness`: `perencanaan-permintaan` `READY_WITH_CONDITIONS`, modul `NOT_READY` ([laporan](perencanaan-permintaan/testing/readiness-report.md)); bukti per task 16 November 2026 |
| SHA backend | `9f396063` (branch `sukmagp`) |
| SHA frontend | `608e46ba4` (branch `sukmagpV2`) |

## Rencana fase

| Fase | Isi | Skill pemilik | Status |
| --- | --- | --- | --- |
| `PBJ-PH-001` | Wawancara kebutuhan | `grill-me` | `DONE` — scope + closure pass (`PBJ-DEC-001..026`), 12 November 2026 |
| `PBJ-PH-002` | Audit kemampuan yang sudah ada | `trace-existing-capabilities` | `DONE` — `01-existing-capability-map.md` revisi 1 @`100c011d`/`763d665b`, 12 November 2026 |
| `PBJ-PH-003` | Penilaian kelengkapan kebutuhan per slice | `requirement-completeness-gate` | `DONE` untuk `perencanaan-permintaan` — `READY_FOR_DOMAIN_DESIGN` (`02-requirement-completeness-assessment.md` revisi 2) |
| `PBJ-PH-004` | Desain blueprint (arsitektur, kontrak, kamus data, alur) | `design-business-module` | `DONE` untuk `perencanaan-permintaan` — `approved` 12 November 2026 (`PBJ-DEC-034`); sub-modul lain `NOT_STARTED` |
| `PBJ-PH-005` | Rencana task BE/FE | `plan-module-delivery` | `DONE` untuk `perencanaan-permintaan` — roadmap revisi 1 (11 task BE, 8 task FE), 12 November 2026 |
| `PBJ-PH-006` | Implementasi per task | `build-module-backend` / `build-module-frontend` | `DONE` untuk `perencanaan-permintaan` — 16 November 2026: `BE-PBJ-001`..`011` ✅, `FE-PBJ-001`..`008` ✅; tambahan `PBJ-DEC-039`/`040` |
| `PBJ-PH-007` | Audit kesiapan | `verify-module-readiness` | `IN_PROGRESS` untuk `perencanaan-permintaan` — audit pertama 17 November 2026: `READY_WITH_CONDITIONS` (syarat K-1..K-6, [laporan](perencanaan-permintaan/testing/readiness-report.md)); K-6 selesai hari yang sama; audit ulang untuk sign-off |

## Status pengiriman

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `DONE` (11/11 ✅) | `DONE` (8/8 ✅) | `DONE` di dev — alur PR → verifikasi → 4 langkah persetujuan → batal, rencana, pagu, paket diuji ujung-ke-ujung lewat layar | `READY_WITH_CONDITIONS` — 17 November 2026 ([laporan](perencanaan-permintaan/testing/readiness-report.md)) |

## Penghambat dan pemiliknya

| ID | Ringkasan | Pemilik | Fase terdampak | Yang tetap bisa berjalan |
| --- | --- | --- | --- | --- |
| `PBJ-OQ-005`, `015`, `019` | Metode dan batas nilai, SLA, nilai awal matriks dari SOP | Pengguna sesi (Product Owner Pengadaan) | `PBJ-PH-004` sebagian; `PBJ-OQ-019` juga syarat pemakaian nyata `perencanaan-permintaan` (K-1): tanpa matriks aktif, setiap pengajuan dijawab `PBJ-VAL-091` | Pengembangan dan desain sub-modul lain |
| K-2 | Bundel izin `PBJ-DEC-039` belum diterapkan di Akses Role lingkungan nyata | Admin Akses Role | Pemakaian nyata `perencanaan-permintaan` | Pengembangan |
| K-3 | Pemilik keamanan sub-modul belum ditunjuk (penjaga lampiran, cakupan unit) | Product Owner Pengadaan | Sign-off `PBJ-PH-007` | Pengembangan |
| K-4 | UAT pengguna 0/16; `UAT-16` (cuti) menunggu master cuti | Product Owner Pengadaan; pemilik HR | Sign-off `PBJ-PH-007` | Pengembangan |
| K-5 (G-05, G-06) | Diputuskan 17 November 2026: `PBJ-DEC-041` (hapus master terpakai tetap 400 + `errors.code` `PBJ-VAL-071`), `PBJ-DEC-042` (`GET purchase-requests/budget-remaining` ber-izin PR); implementasi belum | Builder backend/frontend | Sign-off `PBJ-PH-007` | Pengembangan |
| `PBJ-OQ-008` | Sumber stok untuk integrasi | Belum diketahui | `LATER SLICE` (`PBJ-DEC-008`) | Semua |
| Bug HR lama | Empat temuan HR yang terungkap saat `AT-PP-35` (transaksi ganda pengajuan formasi, konkurensi draf standar ketenagaan, kode alur T9, penerap HR menunggu `Approved` padahal mesin `Completed`) | Pemilik modul HR | Bukan fase Pengadaan | Semua |

Keputusan `PBJ-OQ-001`, `003`, `004`, `006`, `007`, `009`..`014`, `016`..`018`, `020`..`025` sudah ditutup (`PBJ-DEC-004`..`026`, `00-interview-decisions.md`).

## Bukti usang

Tidak ada. Manifest modul dan sub-modul diselaraskan 17 November 2026 ke backend `9f396063` dan frontend `608e46ba4` (temuan G-07 audit kesiapan). Bukti uji per task tetap berlaku: kedua commit memuat kode yang diuji 16 November 2026, dan sesudahnya tidak ada commit lain pada `Areas/Corporate/ProcurementManagement` maupun mesin `Wfl`.

## Status sub-modul

| Sub-modul | Status |
| --- | --- |
| `perencanaan-permintaan` | `approved` (`PBJ-DEC-034`); implementasi 19/19 task ✅; kesiapan `READY_WITH_CONDITIONS` (17 November 2026) |
| `pemilihan-penyedia` | `NOT_STARTED` |
| `manajemen-vendor` | `NOT_STARTED` |
| `kontrak-pelaksanaan` | `NOT_STARTED` |

Status modul diturunkan: `partial`. Kesiapan modul diturunkan: `NOT_READY` (tiga sub-modul belum dimulai).

## Tindakan berikutnya

1. **Product Owner + admin `Wfl` (K-1):** isi matriks approval `PBJ-*` dari SOP (`PBJ-OQ-019`), lalu aktifkan definisinya. Matriks uji sudah dibersihkan; definisi tidak aktif (fail-closed, `PBJ-DEC-038`).
2. **Admin Akses Role (K-2):** terapkan bundel peran `PBJ-DEC-039` (izin baca sumber dana, satuan, organisasi).
3. **Product Owner (K-3):** tunjuk pemilik keamanan sub-modul. **Builder (K-5):** terapkan `PBJ-DEC-041` dan `PBJ-DEC-042` (tambahan pada `BE-PBJ-003`, `BE-PBJ-010`, `FE-PBJ-004`).
4. **UAT bersama pengguna (K-4):** `UAT-01`..`15`; `UAT-16` setelah master cuti tersedia.
5. **Pemilik HR:** tindak lanjuti empat bug HR lama di laporan `BE-PBJ-001`.
6. `verify-module-readiness` ulang untuk sign-off `perencanaan-permintaan` sesudah K-1..K-5.
7. Sub-modul berikutnya: `manajemen-vendor` lewat `grill-me` (boleh paralel, `PBJ-DEC-018`), lalu `pemilihan-penyedia` dan `kontrak-pelaksanaan`.

## Progres pengiriman

`perencanaan-permintaan`: 19 / 19 task ✅ (`BE-PBJ-001`..`011`, `FE-PBJ-001`..`008`). Tidak ada task yang dikecualikan. Diperbarui 16 November 2026.
