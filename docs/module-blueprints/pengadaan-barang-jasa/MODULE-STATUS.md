# Pengadaan Barang & Jasa — Status Modul

| Field | Nilai |
| --- | --- |
| Blueprint ID | `PBJ-BP-001` |
| Nama modul | Pengadaan Barang & Jasa |
| Revisi | `2` |
| Status modul | `PARTIAL` (diturunkan: 2 sub-modul approved, 2 belum dimulai) |
| Fase saat ini | `PBJ-PH-007` audit kesiapan: `perencanaan-permintaan` `READY_WITH_CONDITIONS` (sign-off menunggu K-1..K-4) dan `manajemen-vendor` `READY_WITH_CONDITIONS` (sign-off menunggu VD K-1..K-3, K-5..K-7; K-4 selesai); dua sub-modul lain belum dimulai |
| Terakhir diverifikasi | 19 November 2026 — `verify-module-readiness`: `manajemen-vendor` `READY_WITH_CONDITIONS` ([laporan](manajemen-vendor/testing/readiness-report.md)); `perencanaan-permintaan` `READY_WITH_CONDITIONS` 17 November 2026 ([laporan](perencanaan-permintaan/testing/readiness-report.md)); modul `NOT_READY` |
| SHA backend | `a6df5619` (branch `sukmagp`) + perubahan `manajemen-vendor` belum di-commit |
| SHA frontend | `b1b360efa` (branch `sukmagpV2`) + perubahan `manajemen-vendor` belum di-commit |

## Rencana fase

| Fase | Isi | Skill pemilik | Status |
| --- | --- | --- | --- |
| `PBJ-PH-001` | Wawancara kebutuhan | `grill-me` | `DONE` — scope + closure pass (`PBJ-DEC-001..026`), 12 November 2026 |
| `PBJ-PH-002` | Audit kemampuan yang sudah ada | `trace-existing-capabilities` | `DONE` — `01-existing-capability-map.md` revisi 1 @`100c011d`/`763d665b`, 12 November 2026 |
| `PBJ-PH-003` | Penilaian kelengkapan kebutuhan per slice | `requirement-completeness-gate` | `DONE` untuk `perencanaan-permintaan` — `READY_FOR_DOMAIN_DESIGN` (`02-requirement-completeness-assessment.md` revisi 2); `DONE` untuk `manajemen-vendor` — `S-V1`..`S-V6` `READY_FOR_DOMAIN_DESIGN` (revisi 4) |
| `PBJ-PH-004` | Desain blueprint (arsitektur, kontrak, kamus data, alur) | `design-business-module` | `DONE` untuk `perencanaan-permintaan` — `approved` 12 November 2026 (`PBJ-DEC-034`); `DONE` untuk `manajemen-vendor` — `approved` 17 November 2026 (`PBJ-DEC-062`); dua sub-modul lain `NOT_STARTED` |
| `PBJ-PH-005` | Rencana task BE/FE | `plan-module-delivery` | `DONE` untuk `perencanaan-permintaan` — roadmap revisi 2 (12 task BE, 9 task FE); `DONE` untuk `manajemen-vendor` — roadmap revisi 1 (8 task BE `BE-PBJ-VD-001`..`008`, 5 task FE `FE-PBJ-VD-001`..`005`), 17 November 2026 |
| `PBJ-PH-006` | Implementasi per task | `build-module-backend` / `build-module-frontend` | `DONE` untuk `perencanaan-permintaan` — 16 November 2026: `BE-PBJ-001`..`011` ✅, `FE-PBJ-001`..`008` ✅; tambahan `PBJ-DEC-039`/`040`. `DONE` untuk `manajemen-vendor` — 19 November 2026: `BE-PBJ-VD-001`..`008` ✅ (`003` diperbarui D-9/D-10), `FE-PBJ-VD-001`..`005` ✅ |
| `PBJ-PH-007` | Audit kesiapan | `verify-module-readiness` | `IN_PROGRESS` untuk `perencanaan-permintaan` — audit pertama 17 November 2026: `READY_WITH_CONDITIONS` (syarat K-1..K-6, [laporan](perencanaan-permintaan/testing/readiness-report.md)); K-5 dan K-6 selesai hari yang sama; audit ulang untuk sign-off. `IN_PROGRESS` untuk `manajemen-vendor` — audit pertama 19 November 2026: `READY_WITH_CONDITIONS` (syarat K-1..K-7, [laporan](manajemen-vendor/testing/readiness-report.md)) |

## Status pengiriman

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `perencanaan-permintaan`: `DONE` (11/11 ✅) | `DONE` (8/8 ✅) | `DONE` di dev — alur PR → verifikasi → 4 langkah persetujuan → batal, rencana, pagu, paket diuji ujung-ke-ujung lewat layar | `READY_WITH_CONDITIONS` — 17 November 2026 ([laporan](perencanaan-permintaan/testing/readiness-report.md)) |
| `manajemen-vendor`: `DONE` (8/8 ✅) | `DONE` (5/5 ✅) | `DONE` di dev — dokumen + verifikasi + kelayakan, Batasi/Cabut, usulan blokir → persetujuan `Wfl` (matriks uji) → `MstSupplier.IsBlacklisted`, kunci Master Data, penilaian + revisi; 34/34 `AT-VD` | `READY_WITH_CONDITIONS` — 19 November 2026 ([laporan](manajemen-vendor/testing/readiness-report.md)) |

## Penghambat dan pemiliknya

| ID | Ringkasan | Pemilik | Fase terdampak | Yang tetap bisa berjalan |
| --- | --- | --- | --- | --- |
| `PBJ-OQ-005`, `015`, `019` | Metode dan batas nilai, SLA, nilai awal matriks dari SOP | Pengguna sesi (Product Owner Pengadaan) | `PBJ-PH-004` sebagian; `PBJ-OQ-019` juga syarat pemakaian nyata `perencanaan-permintaan` (K-1): tanpa matriks aktif, setiap pengajuan dijawab `PBJ-VAL-091` | Pengembangan dan desain sub-modul lain |
| K-2 | Bundel izin `PBJ-DEC-039` belum diterapkan di Akses Role lingkungan nyata | Admin Akses Role | Pemakaian nyata `perencanaan-permintaan` | Pengembangan |
| K-3 | Pemilik keamanan sub-modul belum ditunjuk (penjaga lampiran, cakupan unit) | Product Owner Pengadaan | Sign-off `PBJ-PH-007` | Pengembangan |
| K-4 | UAT pengguna 0/16; `UAT-16` (cuti) menunggu master cuti | Product Owner Pengadaan; pemilik HR | Sign-off `PBJ-PH-007` | Pengembangan |
| VD K-1 | `PBJ-VND-BLOKIR`/`BUKA` tidak aktif tanpa matriks (`PBJ-OQ-019`). Master Data sudah dikunci, jadi vendor tidak bisa diblokir sama sekali sampai ini diisi; wajib sebelum kunci Master Data dirilis ke produksi | Product Owner + admin `Wfl` | Pemakaian nyata `manajemen-vendor` | Batasi tetap tersedia; pengembangan |
| VD K-2 | Bundel `PBJ-DEC-051` belum diterapkan (0 jabatan nyata berizin vendor); bundel Direktur ditambah `ProcurementAttachment : Read` dan `ApprovalInbox : RequestRevision` (`PBJ-DEC-065`, 19 November 2026) | Admin Akses Role | Pemakaian nyata `manajemen-vendor` | Pengembangan |
| VD K-3 | Kode `manajemen-vendor` belum di-commit; bukti terikat ke working tree, bukan SHA | Pengguna (commit) | Sign-off `PBJ-PH-007` | Semua |
| VD K-5..K-7 | Pemilik keamanan, UAT 0/12, kategori/kriteria nyata (`PBJ-OQ-034`), label menu 10 (`PBJ-OQ-033`) | Product Owner; Tim Pengadaan | Sign-off `PBJ-PH-007` | Semua |
| Bug Master Data pajak | Form Master Data Supplier selalu mengirim `taxPercent`; backend menolak untuk supplier non-pajak, sehingga supplier non-pajak tidak bisa disimpan dari layar itu (`FE-PBJ-VD-004` T-2) | Pemilik Master Data Supplier | Bukan fase Pengadaan | Semua |
| `PBJ-OQ-008` | Sumber stok untuk integrasi | Belum diketahui | `LATER SLICE` (`PBJ-DEC-008`) | Semua |
| Bug HR kode unit | `GenerateDepartmentCodeAsync` memilih nomor kosong dari unit aktif saja, padahal index unik memuat unit terhapus; setiap unit yang dihapus lunak memblokir pembuatan unit berikutnya (500). Di `QuilvianNewDevSukma` diatasi dengan akhiran `-DEL` pada kode unit uji terhapus (laporan `BE-PBJ-010`) | Pemilik modul HR | Bukan fase Pengadaan | Semua |
| Bug HR lama | Empat temuan HR yang terungkap saat `AT-PP-35` (transaksi ganda pengajuan formasi, konkurensi draf standar ketenagaan, kode alur T9, penerap HR menunggu `Approved` padahal mesin `Completed`) | Pemilik modul HR | Bukan fase Pengadaan | Semua |

Keputusan `PBJ-OQ-001`, `003`, `004`, `006`, `007`, `009`..`014`, `016`..`018`, `020`..`025` sudah ditutup (`PBJ-DEC-004`..`026`, `00-interview-decisions.md`).

## Bukti usang

- `perencanaan-permintaan`: tidak ada. Manifest sub-modul tetap pada `9f396063`/`608e46ba4`. Commit sesudahnya (`a6df5619`/`b1b360efa`) adalah task `BE-PBJ-012`/`FE-PBJ-009` milik roadmap revisi 2 sub-modul itu sendiri, beserta laporannya.
- `manajemen-vendor`: bukti uji 17–19 November 2026 dijalankan pada working tree di atas `a6df5619`/`b1b360efa` yang **belum di-commit** (VD K-3). Manifest diselaraskan 19 November 2026. Hash ke-18 masukan dan artefak desain cocok, sehingga desain tidak basi.

## Status sub-modul

| Sub-modul | Status |
| --- | --- |
| `perencanaan-permintaan` | `approved` (`PBJ-DEC-034`); implementasi 21/21 task ✅; kesiapan `READY_WITH_CONDITIONS` (17 November 2026) |
| `pemilihan-penyedia` | `NOT_STARTED` |
| `manajemen-vendor` | `approved` (`PBJ-DEC-062`, 17 November 2026); registry `Vendor` `ACTIVE` (`PBJ-DEC-063`); kesiapan `READY_WITH_CONDITIONS` (19 November 2026, [laporan](manajemen-vendor/testing/readiness-report.md)); roadmap revisi 1: 13 / 13 task ✅ (backend 8/8 ✅: `BE-PBJ-VD-001`..`006` 17 November 2026, `007`, `008` 18 November 2026, `003` diperbarui 18 dan 19 November 2026; frontend `FE-PBJ-VD-001`, `002` ✅ 18 November 2026, `003`..`005` ✅ 19 November 2026); audit kesiapan selesai 19 November 2026, sign-off menunggu VD K-1..K-7; `FE-PBJ-VD-004` dirilis bersama `BE-PBJ-VD-007`; `PBJ-VD-INT` perlu 1.1 (tambahan respons kelayakan); `PBJ-VD-API` perlu 1.1 (rekening tersamarkan dan saringan bank di daftar vendor, delta D-9; saringan gabungan `vendorStatus=NON_ACTIVE`, delta D-10 `BE-PBJ-VD-003`) |
| `kontrak-pelaksanaan` | `NOT_STARTED` |

Status modul diturunkan: `partial`. Kesiapan modul diturunkan: `NOT_READY` (`pemilihan-penyedia` dan `kontrak-pelaksanaan` belum dimulai).

## Tindakan berikutnya

1. **Product Owner + admin `Wfl` (K-1):** isi matriks approval `PBJ-*` dari SOP (`PBJ-OQ-019`), lalu aktifkan definisinya. Matriks uji sudah dibersihkan; definisi tidak aktif (fail-closed, `PBJ-DEC-038`).
2. **Admin Akses Role (K-2):** terapkan bundel peran `PBJ-DEC-039` (izin baca sumber dana, satuan, organisasi).
3. **Product Owner (K-3):** tunjuk pemilik keamanan sub-modul. K-5 selesai 17 November 2026 (`PBJ-DEC-041`, `PBJ-DEC-042`).
4. **UAT bersama pengguna (K-4):** `UAT-01`..`15`; `UAT-16` setelah master cuti tersedia.
5. **Pemilik HR:** tindak lanjuti empat bug HR lama di laporan `BE-PBJ-001`.
6. `verify-module-readiness` ulang untuk sign-off `perencanaan-permintaan` sesudah K-1..K-4.
7. **`manajemen-vendor`:** isi matriks `PBJ-VND-*` sebelum kunci Master Data dirilis (VD K-1); terapkan bundel `PBJ-DEC-051` + lampiran untuk Direktur (VD K-2); commit kode (VD K-3); lalu UAT dan audit ulang. VD K-4 selesai 19 November 2026: kontrak 1.1 (`PBJ-DEC-064`, `PBJ-DEC-065`) sudah ditulis.
8. Sub-modul berikutnya: `pemilihan-penyedia` lewat `grill-me` (butuh `PBJ-OQ-005`), lalu `kontrak-pelaksanaan`.

## Progres pengiriman

`perencanaan-permintaan`: 21 / 21 task ✅ (`BE-PBJ-001`..`012`, `FE-PBJ-001`..`009`; roadmap revisi 2 menambah `BE-PBJ-012` dan `FE-PBJ-009` untuk standar layar daftar `PBJ-FE-004`). Tidak ada task yang dikecualikan. Diperbarui 17 November 2026.

`manajemen-vendor`: 13 / 13 task ✅ (backend `BE-PBJ-VD-001`..`008` ✅, frontend `FE-PBJ-VD-001`..`005` ✅; roadmap revisi 1). Audit kesiapan 19 November 2026: `READY_WITH_CONDITIONS`. Diperbarui 19 November 2026.
