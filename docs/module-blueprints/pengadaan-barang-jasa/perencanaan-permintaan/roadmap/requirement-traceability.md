# Perencanaan & Permintaan — Traceability Requirement

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · `perencanaan-permintaan` (`approved`, `PBJ-DEC-034`) |
| Revisi roadmap | `1` · `draft` · 12 November 2026 |
| Kontrak | set `1.0` (`approved`) |
| SHA | BE `100c011d`, FE `763d665b` |
| Hash masukan | `../blueprint-manifest.md` |

Kolom **Bukti** merujuk `testing/acceptance-test-matrix.md` (`AT-PP-*`) dan `04-prd-to-mvp.md` (`UAT-*`).
Bukti backend berupa QBE, build, API manual, dan query basis data; ketiadaan automated test backend bukan gap.
Status: belum dikerjakan, kecuali baris yang bertanda (12 November 2026).

## 1. Functional requirement → task

| Requirement | Keputusan | Desain / kontrak | Task BE | Task FE | Bukti |
| --- | --- | --- | --- | --- | --- |
| FR-PBJ-001 penangan dokumen non-HR | `PBJ-DEC-019`, `026` | Arsitektur §8; INT §1 | `BE-PBJ-001` 🟡, `BE-PBJ-005` 🟡 | — | `AT-PP-31`, `34` — titik perluasan ada; penangan `PBJ_*` terdaftar ([laporan 005](../task/report/backend/BE-PBJ-005.md)) (statis, [laporan](../task/report/backend/BE-PBJ-001.md)); runtime menunggu `BE-PBJ-005` |
| FR-PBJ-002 langkah bersyarat | `PBJ-DEC-030` | Arsitektur §8; kamus §6 | `BE-PBJ-001` 🟡 | — | `AT-PP-25`, `26` — saringan langkah ada (statis); skrip EF satu kolom `PASS` |
| FR-PBJ-003 HR tidak berubah | `PBJ-DEC-019` | — | `BE-PBJ-001` 🟡 | — | `AT-PP-35`, `UAT-16` — `NOT RUN`, menunggu migration diterapkan |
| FR-PBJ-010..012 katalog dan master | `PBJ-DEC-022`, `024`, `033` | API §Master Data; VAL-070..072 | `BE-PBJ-003` 🟡 | `FE-PBJ-001` | `AT-PP-50`, `51`, `64`, `UAT-15` — kode `PASS` build/QBE; runtime belum ([laporan](../task/report/backend/BE-PBJ-003.md)) |
| FR-PBJ-020..021 lampiran | `PBJ-DEC-023` | API §Attachment; VAL-080..083 | `BE-PBJ-004` 🟡 | `FE-PBJ-002` | `AT-PP-53`..`56`, `UAT-14` — kode `PASS` build/QBE; penyedia akses `PBJ_PR` ada (`BE-PBJ-006` 🟡); runtime menunggu migration diterapkan ([laporan](../task/report/backend/BE-PBJ-004.md)) |
| FR-PBJ-022 riwayat dokumen | — | PERM §5 | `BE-PBJ-002` 🟡 (+ setiap task aksi) | `FE-PBJ-003`, `004` | `AT-PP-01`, `10`, `31` — penulis riwayat append-only ada ([laporan](../task/report/backend/BE-PBJ-002.md)) |
| FR-PBJ-030 nomor saat diajukan | `PBJ-DEC-025`, `031` | Arsitektur §9 | `BE-PBJ-006` 🟡, `007` | `FE-PBJ-004` | `AT-PP-01`, `10` — draf tanpa nomor ada ([laporan](../task/report/backend/BE-PBJ-006.md)); nomor menyusul `BE-PBJ-007` |
| FR-PBJ-031..032 dalam/luar rencana, kunci bersamaan | `PBJ-DEC-014`, `029` | STATE §1; VAL-009, 012 | `BE-PBJ-007` (butuh `009`) | `FE-PBJ-004` | `AT-PP-04`..`07`, `UAT-07`, `08` |
| FR-PBJ-033 cakupan unit | `PBJ-DEC-020`, `033` (`G-08`), `036` | PERM 1.1 §4 | `BE-PBJ-002` 🟡, `006` 🟡 | `FE-PBJ-003` | `AT-PP-08`, `56`, `UAT-03` — penyaring cakupan + `ReadAllUnits` ada (statis); runtime belum ([laporan](../task/report/backend/BE-PBJ-006.md)) |
| FR-PBJ-034 mendesak | `PBJ-DEC-016` | VAL-008; arsitektur §8 | `BE-PBJ-007`, `008` | `FE-PBJ-004` | `AT-PP-03` |
| FR-PBJ-035 batal/tarik | `PBJ-DEC-033` (`G-04`) | STATE §1 | `BE-PBJ-007` | `FE-PBJ-004` | `AT-PP-06`, `36` |
| FR-PBJ-040..042 cek stok dan nilai beli | `PBJ-DEC-008`, `027` | STATE §1; VAL-020, 021 | `BE-PBJ-008` | `FE-PBJ-005` | `AT-PP-20`..`23`, `UAT-01`, `05` |
| FR-PBJ-043 lewat pagu | `PBJ-DEC-030` | VAL §1 rumus | `BE-PBJ-008` (butuh `010`) | `FE-PBJ-005` | `AT-PP-25`, `UAT-06` |
| FR-PBJ-044 verifikator ≠ pengusul; anti setujui sendiri | `PBJ-DEC-007` | PERM §4 | `BE-PBJ-005` 🟡, `008` | `FE-PBJ-005` | `AT-PP-27`, `30`, `UAT-04` — `AllowSelfApproval=false` + periksa ulang di penangan (statis) |
| FR-PBJ-050..053 rencana dan revisi | `PBJ-DEC-033` (`G-05`) | STATE §2; VAL-040..044 | `BE-PBJ-009` | `FE-PBJ-006` | `AT-PP-40`..`42`, `UAT-09`, `10` |
| FR-PBJ-060..062 pagu | `PBJ-DEC-009`, `033` | STATE §4; VAL-060..062 | `BE-PBJ-010` | `FE-PBJ-007` | `AT-PP-45`, `46`, `UAT-13` |
| FR-PBJ-070..072 konsolidasi dan rencana paket | `PBJ-DEC-028` | STATE §3; VAL-050, 051 | `BE-PBJ-011` | `FE-PBJ-008` | `AT-PP-43`, `44`, `UAT-11`, `12` |
| Persetujuan berjenjang terkonfigurasi | `PBJ-DEC-007`, `019` | Arsitektur §8, §10 | `BE-PBJ-005` 🟡 | `FE-PBJ-005` (`-17`) | `AT-PP-31`..`33` — gerbang + definisi tidak aktif di-seed; instance nyata menunggu `BE-PBJ-009` ([laporan](../task/report/backend/BE-PBJ-005.md)) |
| Menu dan keterjangkauan layar | `PBJ-FE-001`, `PBJ-DEC-032` | FE §3 | — | semua task FE | `AT-PP-63` |
| Registry dan konformitas | `PBJ-OQ-002` → `PBJ-DEC-035` | Arsitektur §5 | `BE-PBJ-002` 🟡 | — | `AT-PP-60` — QBE `PASS` 17 berkas, 0 temuan |
| Build dan skema | — | Kamus data §8 | semua task BE | semua task FE | `AT-PP-61`, `62` |

## 2. Keputusan → task

| Keputusan | Task |
| --- | --- |
| `PBJ-DEC-001` (PO/GR tidak dibuat) | Tidak ada task yang membuat endpoint PO/GR (`PBJ-AC-001`); diperiksa saat review diff semua task BE |
| `PBJ-DEC-005`, `019`, `026` | `BE-PBJ-001`, `005` |
| `PBJ-DEC-007`, `016`, `027`, `030` | `BE-PBJ-005`, `007`, `008` |
| `PBJ-DEC-008` | `BE-PBJ-008`, `FE-PBJ-005` |
| `PBJ-DEC-009` | `BE-PBJ-010` |
| `PBJ-DEC-010`, `015`, `021` | Sub-modul `manajemen-vendor` (bukan roadmap ini) |
| `PBJ-DEC-011`, `012`, `013` | Sub-modul `kontrak-pelaksanaan` (bukan roadmap ini) |
| `PBJ-DEC-014`, `029` | `BE-PBJ-007`, `009` |
| `PBJ-DEC-020` | `BE-PBJ-002`, `006` |
| `PBJ-DEC-022`, `024` | `BE-PBJ-003`, `FE-PBJ-001` |
| `PBJ-DEC-023` | `BE-PBJ-004`, `FE-PBJ-002` |
| `PBJ-DEC-025` (bagian tersisa), `031` | `BE-PBJ-002` (konstanta), `003`, `007`, `009`, `011` |
| `PBJ-DEC-028` | `BE-PBJ-011`, `FE-PBJ-008` |
| `PBJ-DEC-032` | `FE-PBJ-001` |
| `PBJ-DEC-033` | `BE-PBJ-002`, `003`, `007`, `009`, `010`, `011` |

## 3. Gap traceability

| Gap | Keterangan | Tindakan |
| --- | --- | --- |
| ~~`PBJ-OQ-002`~~ | Ditutup 12 November 2026 (`PBJ-DEC-035`) | — |
| `PBJ-OQ-019` | Nilai matriks dari SOP belum ada; approval memakai baris fallback | Tidak memblokir DoD; diisi lewat konfigurasi `Wfl` |
| `AT-PP-36` (tarik) | Baru dapat dibuktikan penuh setelah `BE-PBJ-008` | `BE-PBJ-007` boleh 🟡 untuk kriteria itu saja sampai `BE-PBJ-008` selesai |
| `AT-PP-45`, `46` | Komitmen belum punya pemanggil | Diisi data uji DB; pemanggil nyata di `pemilihan-penyedia` |
| Requirement tanpa task | Tidak ada. K-13 (dashboard/laporan) dan integrasi stok sengaja `POST-MVP` (`04-prd-to-mvp.md` §8) | — |
