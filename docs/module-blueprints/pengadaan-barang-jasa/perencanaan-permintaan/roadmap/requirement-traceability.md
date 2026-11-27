# Perencanaan & Permintaan — Traceability Requirement

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · `perencanaan-permintaan` (`approved`, `PBJ-DEC-034`) |
| Revisi roadmap | `2` · `draft` · 12 November 2026; rev 2 17 November 2026 |
| Kontrak | set `1.0` (`approved`) |
| SHA | BE `100c011d`, FE `763d665b` |
| Hash masukan | `../blueprint-manifest.md` |

Kolom **Bukti** merujuk `testing/acceptance-test-matrix.md` (`AT-PP-*`) dan `04-prd-to-mvp.md` (`UAT-*`).
Bukti backend berupa QBE, build, API manual, dan query basis data; ketiadaan automated test backend bukan gap.
Status: baris bertanda diperbarui 16 November 2026 (backend dan frontend). Bukti frontend berupa lint, build, dan uji
layar Playwright dengan aktor non-SuperAdmin; tangkapan layar tercatat pada laporan task frontend.

## 1. Functional requirement → task

| Requirement | Keputusan | Desain / kontrak | Task BE | Task FE | Bukti |
| --- | --- | --- | --- | --- | --- |
| FR-PBJ-001 penangan dokumen non-HR | `PBJ-DEC-019`, `026` | Arsitektur §8; INT §1 | `BE-PBJ-001` ✅, `BE-PBJ-005` ✅ | — | `AT-PP-31`, `34` `PASS` runtime 16 Nov 2026 ([BE-PBJ-005](../task/report/backend/BE-PBJ-005.md), [BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)) |
| FR-PBJ-002 langkah bersyarat | `PBJ-DEC-030` | Arsitektur §8; kamus §6 | `BE-PBJ-001` ✅ | — | `AT-PP-25`, `26` `PASS` runtime ([BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)) |
| FR-PBJ-003 HR tidak berubah | `PBJ-DEC-019` | — | `BE-PBJ-001` ✅ | — | `AT-PP-35` `PASS` dengan dokumen HR pengganti `WorkforceStaffingStandard` (cuti/lembur: master dev kosong); `UAT-16` menunggu master cuti ([BE-PBJ-001](../task/report/backend/BE-PBJ-001.md)) |
| FR-PBJ-010..012 katalog dan master | `PBJ-DEC-022`, `024`, `033` | API §Master Data; VAL-070..072 | `BE-PBJ-003` ✅ | `FE-PBJ-001` ✅ | `AT-PP-50`, `51`, `64` `PASS` runtime ([BE-PBJ-003](../task/report/backend/BE-PBJ-003.md)); layar: `KTL-000013` dari server, `PBJ-VAL-070`/`071` terbaca ([FE-PBJ-001](../task/report/frontend/FE-PBJ-001.md)) |
| FR-PBJ-020..021 lampiran | `PBJ-DEC-023` | API §Attachment; VAL-080..083 | `BE-PBJ-004` ✅ | `FE-PBJ-002` ✅ | `AT-PP-53`..`56` `PASS` runtime ([BE-PBJ-004](../task/report/backend/BE-PBJ-004.md)); layar: unggah, ganti versi, `PBJ-VAL-081`, `readOnly` ([FE-PBJ-002](../task/report/frontend/FE-PBJ-002.md)) |
| FR-PBJ-022 riwayat dokumen | — | PERM §5 | `BE-PBJ-002` ✅ (+ setiap task aksi) | `FE-PBJ-003` ✅, `004` ✅ | `AT-PP-01`, `10`, `31` `PASS` ([BE-PBJ-002](../task/report/backend/BE-PBJ-002.md)); layar: tab Riwayat ([FE-PBJ-003](../task/report/frontend/FE-PBJ-003.md)) |
| FR-PBJ-030 nomor saat diajukan | `PBJ-DEC-025`, `031` | Arsitektur §9 | `BE-PBJ-006` ✅, `007` ✅ | `FE-PBJ-004` ✅ | `AT-PP-01`, `10` `PASS` ([BE-PBJ-007](../task/report/backend/BE-PBJ-007.md)); layar: PR-000014 tampil setelah Ajukan ([FE-PBJ-004](../task/report/frontend/FE-PBJ-004.md)) |
| FR-PBJ-031..032 dalam/luar rencana, kunci bersamaan | `PBJ-DEC-014`, `029` | STATE §1; VAL-009, 012 | `BE-PBJ-007` ✅ | `FE-PBJ-004` ✅ | `AT-PP-04`..`07` `PASS` ([BE-PBJ-007](../task/report/backend/BE-PBJ-007.md)); layar: baris rencana + sisa, galat luar rencana di baris ([FE-PBJ-004](../task/report/frontend/FE-PBJ-004.md)) |
| FR-PBJ-033 cakupan unit | `PBJ-DEC-020`, `033` (`G-08`), `036` | PERM 1.1 §4 | `BE-PBJ-002` ✅, `006` ✅ | `FE-PBJ-003` ✅ | `AT-PP-08`, `56` `PASS` ([BE-PBJ-006](../task/report/backend/BE-PBJ-006.md), [BE-PBJ-004](../task/report/backend/BE-PBJ-004.md)); layar: saringan unit dalam cakupan, 404 unit lain ([FE-PBJ-003](../task/report/frontend/FE-PBJ-003.md)) |
| FR-PBJ-034 mendesak | `PBJ-DEC-016` | VAL-008; arsitektur §8 | `BE-PBJ-007` ✅, `008` ✅ | `FE-PBJ-004` ✅ | `AT-PP-03` `PASS` ([BE-PBJ-007](../task/report/backend/BE-PBJ-007.md)); layar: `-14` terkunci `URGENT` ([FE-PBJ-004](../task/report/frontend/FE-PBJ-004.md)) |
| FR-PBJ-035 batal/tarik | `PBJ-DEC-033` (`G-04`) | STATE §1 | `BE-PBJ-007` ✅ | `FE-PBJ-004` ✅ | `AT-PP-06`, `36` `PASS` ([BE-PBJ-007](../task/report/backend/BE-PBJ-007.md)); layar: Batalkan dan Tarik beralasan wajib ([FE-PBJ-004](../task/report/frontend/FE-PBJ-004.md)) |
| FR-PBJ-040..042 cek stok dan nilai beli | `PBJ-DEC-008`, `027` | STATE §1; VAL-020, 021 | `BE-PBJ-008` ✅ | `FE-PBJ-005` ✅ | `AT-PP-20`..`23` `PASS` ([BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)); layar: cek stok + selesaikan ([FE-PBJ-005](../task/report/frontend/FE-PBJ-005.md)) |
| FR-PBJ-043 lewat pagu | `PBJ-DEC-030` | VAL §1 rumus | `BE-PBJ-008` ✅ | `FE-PBJ-005` ✅ | `AT-PP-25` `PASS` ([BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)); layar: tanda "Melebihi Pagu Rp 150.000" ([FE-PBJ-005](../task/report/frontend/FE-PBJ-005.md)) |
| FR-PBJ-044 verifikator ≠ pengusul; anti setujui sendiri | `PBJ-DEC-007` | PERM §4 | `BE-PBJ-005` ✅, `008` ✅ | `FE-PBJ-005` ✅ | `AT-PP-27`, `30` `PASS` ([BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)); layar: tombol verifikasi tersembunyi bagi pembuat ([FE-PBJ-005](../task/report/frontend/FE-PBJ-005.md)) |
| FR-PBJ-050..053 rencana dan revisi | `PBJ-DEC-033` (`G-05`) | STATE §2; VAL-040..044 | `BE-PBJ-009` ✅ | `FE-PBJ-006` ✅ | `AT-PP-40`..`42` `PASS` ([BE-PBJ-009](../task/report/backend/BE-PBJ-009.md)); layar: `PBJ-VAL-040` + tautan, `PBJ-VAL-042` di baris, banding versi ([FE-PBJ-006](../task/report/frontend/FE-PBJ-006.md)) |
| FR-PBJ-060..062 pagu | `PBJ-DEC-009`, `033` | STATE §4; VAL-060..062 | `BE-PBJ-010` ✅ | `FE-PBJ-007` ✅ | `AT-PP-45`, `46` `PASS` ([BE-PBJ-010](../task/report/backend/BE-PBJ-010.md)); layar: `PBJ-VAL-060`/`062`, nominal tersembunyi tanpa izin ([FE-PBJ-007](../task/report/frontend/FE-PBJ-007.md)) |
| FR-PBJ-070..072 konsolidasi dan rencana paket | `PBJ-DEC-028` | STATE §3; VAL-050, 051 | `BE-PBJ-011` ✅ | `FE-PBJ-008` ✅ | `AT-PP-43`, `44` `PASS` ([BE-PBJ-011](../task/report/backend/BE-PBJ-011.md)); layar: konsolidasi → paket berunit asal, `PBJ-VAL-050` ([FE-PBJ-008](../task/report/frontend/FE-PBJ-008.md)) |
| Persetujuan berjenjang terkonfigurasi | `PBJ-DEC-007`, `019` | Arsitektur §8, §10 | `BE-PBJ-005` ✅ | `FE-PBJ-005` ✅ (`-17`) | `AT-PP-31`..`33` `PASS` dengan matriks **uji** dev ([BE-PBJ-008](../task/report/backend/BE-PBJ-008.md)); matriks SOP menunggu `PBJ-OQ-019`; layar: 4 langkah disetujui dari Antrean Persetujuan, `PBJ-VAL-091` ([FE-PBJ-005](../task/report/frontend/FE-PBJ-005.md)) |
| Menu dan keterjangkauan layar | `PBJ-FE-001`, `PBJ-DEC-032` | FE §3 | — | semua task FE ✅ | `AT-PP-63` sebagian: 24 rute dan butir menu §3 terdaftar; butir tersembunyi tanpa izin (uji aktor Keuangan) ([FE-PBJ-001](../task/report/frontend/FE-PBJ-001.md)) |
| Registry dan konformitas | `PBJ-OQ-002` → `PBJ-DEC-035` | Arsitektur §5 | `BE-PBJ-002` ✅ | — | `AT-PP-60` — QBE Strict `PASS` 53 berkas, 0 temuan (16 Nov 2026) |
| Standar layar daftar: baris filter satu baris, saringan tanggal/periode, hero tanpa tombol | `PBJ-FE-004`, `PBJ-DEC-055`..`057` | API 1.4; VAL-094; FE §4.7 | `BE-PBJ-012` ✅ | `FE-PBJ-009` ✅ | `AT-PP-65` 13/13 `PASS` runtime ([BE-PBJ-012](../task/report/backend/BE-PBJ-012.md)); `AT-PP-66` 81/81 `PASS` uji layar ([FE-PBJ-009](../task/report/frontend/FE-PBJ-009.md)) — 17 Nov 2026 |
| Build dan skema | — | Kamus data §8 | semua task BE | semua task FE ✅ | `AT-PP-61` BE `PASS`; `AT-PP-62` `PASS` ([BE-PBJ-011](../task/report/backend/BE-PBJ-011.md)); FE: `npm run lint:errors` dan `npm run build` `PASS` 16 Nov 2026 |

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
| `PBJ-DEC-055` | `BE-PBJ-012` |
| `PBJ-FE-004`, `PBJ-DEC-056`, `057`, `058` | `FE-PBJ-009` |

## 3. Gap traceability

| Gap | Keterangan | Tindakan |
| --- | --- | --- |
| ~~`PBJ-OQ-002`~~ | Ditutup 12 November 2026 (`PBJ-DEC-035`) | — |
| `PBJ-OQ-019` | Nilai matriks dari SOP belum ada; definisi `PBJ-*` tidak aktif tanpa matriks, sehingga pengajuan dijawab `PBJ-VAL-091` (fail-closed, `PBJ-DEC-038`) | Tidak memblokir DoD kode; syarat pemakaian nyata K-1 ([laporan kesiapan](../testing/readiness-report.md)) |
| ~~`AT-PP-36` (tarik)~~ | Terbukti 16 November 2026 bersama `BE-PBJ-008` | — |
| `AT-PP-45`, `46` | Komitmen belum punya pemanggil | Diisi data uji DB (`UJI_PBJ`) — `PASS`; pemanggil nyata di `pemilihan-penyedia` |
| ~~`AT-PP-35` (regresi HR)~~ | Dijalankan 16 November 2026 dengan dokumen HR pengganti; cuti/lembur menunggu master HR (`UAT-16`). Empat bug HR lama tercatat di laporan `BE-PBJ-001` | Pemilik HR |
| ~~Bundel izin vs kebutuhan layar~~ | Ditutup `PBJ-DEC-039` (izin baca master ditambahkan ke bundel) dan `PBJ-DEC-040` (tab Persetujuan lewat `GET /{id}/approval`, tanpa `WorkflowInstance : Read`) | — |
| ~~Delta kolom daftar PR~~ | Ditutup `PBJ-DEC-040`: `urgencyDeadline`, aksi terakhir, urutan `urgency`. Kolom "Langkah saat ini" di `-16` tetap lewat tab Persetujuan | — |
| ~~`PBJ-VAL-071`~~ | Ditutup `PBJ-DEC-041` (17 November 2026): tetap 400 sesuai standar master data, ditambah `errors.code` `PBJ-VAL-071`; `validation-matrix.md` 1.1 dan `AT-PP-51` diselaraskan. Runtime 5/5 `PASS` ([BE-PBJ-003](../task/report/backend/BE-PBJ-003.md)) | — |
| ~~Sisa pagu untuk pengusul~~ | Ditutup `PBJ-DEC-042` (17 November 2026): `GET purchase-requests/budget-remaining` ber-izin PR. `AT-PP-11` backend 9/9 dan layar 4/4 `PASS` ([BE-PBJ-010](../task/report/backend/BE-PBJ-010.md), [FE-PBJ-004](../task/report/frontend/FE-PBJ-004.md)) | — |
| Requirement tanpa task | Tidak ada. K-13 (dashboard/laporan) dan integrasi stok sengaja `POST-MVP` (`04-prd-to-mvp.md` §8) | — |
