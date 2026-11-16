# Pengadaan Barang & Jasa — Status Modul

| Field | Nilai |
| --- | --- |
| Blueprint ID | `PBJ-BP-001` |
| Nama modul | Pengadaan Barang & Jasa |
| Revisi | `2` |
| Status modul | `PARTIAL` (diturunkan: 1 sub-modul approved, 3 belum dimulai) |
| Fase saat ini | `PBJ-PH-006` — Implementasi `perencanaan-permintaan`: 18/19 task ✅, `BE-PBJ-001` 🟡 (`AT-PP-35` `NOT FEASIBLE`) |
| Terakhir diverifikasi | Bukti per task 16 November 2026 (build, QBE Strict, runtime HTTP 123 skenario, uji layar Playwright); audit `verify-module-readiness` belum dijalankan |
| SHA backend | `27fac19c` (branch `sukmagp`, pekerjaan Pengadaan belum di-commit) |
| SHA frontend | `266e3fc6e` (branch `sukmagpV2`, pekerjaan Pengadaan belum di-commit) |

## Rencana fase

| Fase | Isi | Skill pemilik | Status |
| --- | --- | --- | --- |
| `PBJ-PH-001` | Wawancara kebutuhan | `grill-me` | `DONE` — scope + closure pass (`PBJ-DEC-001..026`), 12 November 2026 |
| `PBJ-PH-002` | Audit kemampuan yang sudah ada | `trace-existing-capabilities` | `DONE` — `01-existing-capability-map.md` revisi 1 @`100c011d`/`763d665b`, 12 November 2026 |
| `PBJ-PH-003` | Penilaian kelengkapan kebutuhan per slice | `requirement-completeness-gate` | `DONE` untuk `perencanaan-permintaan` — `READY_FOR_DOMAIN_DESIGN` (`02-requirement-completeness-assessment.md` revisi 2) |
| `PBJ-PH-004` | Desain blueprint (arsitektur, kontrak, kamus data, alur) | `design-business-module` | `DONE` untuk `perencanaan-permintaan` — `approved` 12 November 2026 (`PBJ-DEC-034`); sub-modul lain `NOT_STARTED` |
| `PBJ-PH-005` | Rencana task BE/FE | `plan-module-delivery` | `DONE` untuk `perencanaan-permintaan` — roadmap revisi 1 (11 task BE, 8 task FE), 12 November 2026 |
| `PBJ-PH-006` | Implementasi per task | `build-module-backend` / `build-module-frontend` | `PARTIAL` — 16 November 2026: `BE-PBJ-002`..`011` ✅, `FE-PBJ-001`..`008` ✅; `BE-PBJ-001` 🟡 karena regresi HR `AT-PP-35` tidak dapat dijalankan (definisi workflow HR di dev tanpa langkah) |

## Status pengiriman

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `PARTIAL` (10/11 ✅, 1 🟡) | `DONE` (8/8 ✅) | `DONE` di dev — alur PR → verifikasi → 4 langkah persetujuan → batal, rencana, pagu, paket diuji ujung-ke-ujung lewat layar | `NOT_STARTED` (`verify-module-readiness`) |

## Penghambat dan pemiliknya

| ID | Ringkasan | Pemilik | Fase terdampak | Yang tetap bisa berjalan |
| --- | --- | --- | --- | --- |
| `PBJ-OQ-005`, `015`, `019` | Metode dan batas nilai, SLA, nilai awal dari SOP | Pengguna sesi (Product Owner Pengadaan) | `PBJ-PH-004` sebagian (nilai awal/konfigurasi) | Audit; desain `perencanaan-permintaan` kecuali jenis PR |
| `PBJ-OQ-008` | Sumber stok untuk integrasi | Belum diketahui | `LATER SLICE` (`PBJ-DEC-008`) | Semua |
| `AT-PP-35` | Regresi persetujuan HR butuh definisi workflow HR yang punya langkah di `QuilvianNewDevSukma` | Pemilik konfigurasi workflow HR | `BE-PBJ-001` 🟡 | Semua task lain |
| Bundel izin | "Unit Peminta" tanpa `ProcurementFundSource : Read`, "Tim Pengadaan" tanpa `Measurement : Read`, "Keuangan" tanpa `Organization : Read`, rincian langkah butuh `WorkflowInstance : Read` | Product Owner Pengadaan / admin Akses Role | Pemakaian produksi (pengusul tidak dapat memilih sumber dana) | Semua; uji dev memberi izin sementara lalu dikembalikan |

Keputusan `PBJ-OQ-001`, `003`, `004`, `006`, `007`, `009`..`014`, `016`..`018`, `020`..`025` sudah ditutup (`PBJ-DEC-004`..`026`, `00-interview-decisions.md`).

## Bukti usang

Belum ada. Bukti 16 November 2026 tercatat di `perencanaan-permintaan/task/report/backend/` dan `.../frontend/`.

## Status sub-modul

| Sub-modul | Status |
| --- | --- |
| `perencanaan-permintaan` | `approved` (`PBJ-DEC-034`); implementasi 18/19 task ✅ |
| `pemilihan-penyedia` | `NOT_STARTED` |
| `manajemen-vendor` | `NOT_STARTED` |
| `kontrak-pelaksanaan` | `NOT_STARTED` |

Status modul diturunkan: `partial`.

## Tindakan berikutnya

1. **Product Owner / admin Akses Role:** putuskan bundel izin (sumber dana untuk pengusul, satuan untuk Tim Pengadaan, organisasi untuk Keuangan, rincian `Wfl`) — rincian di `perencanaan-permintaan/task/report/frontend/FE-PBJ-003.md` §8.
2. **Pemilik workflow HR:** sediakan definisi HR berlangkah di dev agar `AT-PP-35` dapat dijalankan dan `BE-PBJ-001` ditutup.
3. **Admin/pemilik proses:** ganti matriks approval uji (`DATA UJI PBJ (bukan nilai SOP)`) dengan nilai SOP (`PBJ-OQ-019`).
4. Audit kesiapan `verify-module-readiness` untuk `perencanaan-permintaan`, lalu commit/PR oleh pemilik branch.
5. Sub-modul `pemilihan-penyedia`, `manajemen-vendor`, `kontrak-pelaksanaan` belum punya roadmap — mulai dari `grill-me`.
6. Bersihkan data uji Pengadaan di `QuilvianNewDevSukma` (fixture `TEST-PBJ …`, dokumen uji) setelah izin pemilik.

## Progres pengiriman

`perencanaan-permintaan`: 18 / 19 task ✅ (`BE-PBJ-002`..`011`, `FE-PBJ-001`..`008`); 1 task 🟡 (`BE-PBJ-001`, `AT-PP-35` `NOT FEASIBLE`). Tidak ada task yang dikecualikan. Diperbarui 16 November 2026.
