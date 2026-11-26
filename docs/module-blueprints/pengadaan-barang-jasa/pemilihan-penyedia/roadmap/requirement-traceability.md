# Pemilihan Penyedia — Traceability Requirement

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · `pemilihan-penyedia` (`approved`, `PBJ-DEC-095`) |
| Revisi roadmap | `1` · `draft` · 26 November 2026 |
| Kontrak | set `1.0` (`approved`); `PBJ-PP-INT` 1.2 |
| SHA | BE `45b22777`, FE `3e5386679` |
| Hash masukan | `../blueprint-manifest.md` |

Kolom **Bukti** merujuk `testing/acceptance-test-matrix.md` (`AT-PS-*`) dan `04-prd-to-mvp.md` (`UAT-PS-*`).

**Status task (26 November 2026):**

- `BE-PBJ-PS-001` ✅ — tiga master, konstanta, seeder contoh, dua definisi `PBJ-PKT-*` tidak aktif; `AT-PS-60`, `61` (backend), `62` (master), `64` terbukti; uji batasan 16/16 `PASS`; laporan [BE-PBJ-PS-001](../task/report/backend/BE-PBJ-PS-001.md).
- `BE-PBJ-PS-002` ✅ — 27 endpoint tiga master; uji runtime 61/61 `PASS` (`UAT-PS-01`, tumpang tindih rentang, 400 cakupan, `PBJ-VAL-071`); laporan [BE-PBJ-PS-002](../task/report/backend/BE-PBJ-PS-002.md).
- `BE-PBJ-PS-003` ✅ — kolom `PackageId`/`PackagedAt` + migration `AddProcurementPurchaseRequestLinePackage`; regresi PR 10/10 `PASS`; laporan [BE-PBJ-PS-003](../task/report/backend/BE-PBJ-PS-003.md).
- `BE-PBJ-PS-004` ✅ — 14 tabel Sourcing + migration `AddProcurementSourcing`; uji batasan 26/26 `PASS`; penanda izin eksplisit dipindah ke `BE-PBJ-PS-005`; laporan [BE-PBJ-PS-004](../task/report/backend/BE-PBJ-PS-004.md).
- Task lain belum dikerjakan.

## 1. Functional requirement → task

| Requirement | Keputusan | Desain / kontrak | Task BE | Task FE | Bukti yang diharapkan |
| --- | --- | --- | --- | --- | --- |
| FR-PS-001 metode berkonfigurasi | `PBJ-DEC-068`, `095` | API §Procurement Method | `BE-PBJ-PS-001` ✅, `002` ✅ | `FE-PBJ-PS-001` | `UAT-PS-01` ✅ API (`UAT-01a`..`e`, [BE-PBJ-PS-002](../task/report/backend/BE-PBJ-PS-002.md)), `02` (cabang paket menunggu `BE-PBJ-PS-007`); `AT-PS-64` ✅ (2 metode contoh, [BE-PBJ-PS-001](../task/report/backend/BE-PBJ-PS-001.md)) |
| FR-PS-002 jenis dan dokumen wajib paket | `PBJ-ASM-007`, `PBJ-DEC-086` | API §Package Document Type/Requirement | `BE-PBJ-PS-001` ✅, `002` ✅ | `FE-PBJ-PS-001` | CRUD + 400 cakupan + `PBJ-VAL-071` ✅ ([BE-PBJ-PS-002](../task/report/backend/BE-PBJ-PS-002.md)); `AT-PS-13`; `AT-PS-64` ✅ (6 jenis, 4 dokumen wajib, [BE-PBJ-PS-001](../task/report/backend/BE-PBJ-PS-001.md)) |
| FR-PS-003 nilai contoh `IsSample` | `PBJ-DEC-069` | Arsitektur §10 | `BE-PBJ-PS-001` ✅ | — | `AT-PS-64` ✅ ([BE-PBJ-PS-001](../task/report/backend/BE-PBJ-PS-001.md)) |
| FR-PS-010 paket dari baris PR | `PBJ-DEC-071`, `082`, `090`, `091` | API §Procurement Package; `PBJ-PP-INT` 1.2 | `BE-PBJ-PS-003` ✅, `004` ✅, `005` | `FE-PBJ-PS-002` | `AT-PS-01`, `02`, `04`, `05`, `06`; kolom baris PR ✅ ([BE-PBJ-PS-003](../task/report/backend/BE-PBJ-PS-003.md)); skema paket dan unik sumber hidup ✅ ([BE-PBJ-PS-004](../task/report/backend/BE-PBJ-PS-004.md)) |
| FR-PS-011 PR `CONVERTED_TO_PACKAGE` | `PBJ-DEC-071`, `077` | STATE §PR; `PBJ-PP-INT` 1.2 | `BE-PBJ-PS-005` | — | `AT-PS-03` |
| FR-PS-012 HPS wajib, saran dan salinan metode | `PBJ-DEC-068`, `073`, `086` | VAL-124..126, 149 | `BE-PBJ-PS-007`, `008` | `FE-PBJ-PS-003` | `AT-PS-10`, `11`, `12`, `17`, `18` |
| FR-PS-013 checklist lengkap | `PBJ-ASM-007` | VAL-127 | `BE-PBJ-PS-007`, `008` | `FE-PBJ-PS-003` | `AT-PS-13`; `UAT-PS-04` |
| FR-PS-014 persetujuan persiapan | `PBJ-DEC-007`, `038`, `072` | STATE §1; INT §1 | `BE-PBJ-PS-008` | `FE-PBJ-PS-003` | `AT-PS-14`, `15`, `16`; `UAT-PS-03` |
| FR-PS-015 pecah paket | `PBJ-DEC-083` | VAL-144 | `BE-PBJ-PS-015` | `FE-PBJ-PS-007` | `AT-PS-53`, `54`; `UAT-PS-13` |
| FR-PS-016 batal paket | `PBJ-DEC-077` | VAL-147, 148 | `BE-PBJ-PS-015` | `FE-PBJ-PS-007` | `AT-PS-52`; `UAT-PS-14` |
| FR-PS-020 RFQ ke vendor layak | `PBJ-ASM-006`, `PBJ-DEC-045`, `054`, `087` | API §Procurement RFQ; VAL-129..131 | `BE-PBJ-PS-009` | `FE-PBJ-PS-004` | `AT-PS-20`, `21`, `22` |
| FR-PS-021 batas waktu mengikat | `PBJ-DEC-081` | VAL-132 | `BE-PBJ-PS-009`, `010` | `FE-PBJ-PS-004` | `AT-PS-23`, `24`; `UAT-PS-06` |
| FR-PS-022 penawaran dan PPN | `PBJ-DEC-067`, `084` | API §Procurement Quotation; VAL-133, 134 | `BE-PBJ-PS-010` | `FE-PBJ-PS-004` | `AT-PS-25`, `26`; `UAT-PS-05` |
| FR-PS-023 RFQ gagal | `PBJ-DEC-075` | STATE §2 | `BE-PBJ-PS-009` | `FE-PBJ-PS-004` | `AT-PS-35` |
| FR-PS-030 evaluator ≠ pencatat | `PBJ-DEC-074` | VAL-135 | `BE-PBJ-PS-007` (penugasan), `011` | `FE-PBJ-PS-003` (penugasan), `005` | `AT-PS-30`, `32`; `UAT-PS-08` |
| FR-PS-031 evaluator tanpa harga | `PBJ-DEC-079`, `080` | PERM §3–§4; API §Technical Evaluation | `BE-PBJ-PS-006`, `011` | `FE-PBJ-PS-005` | `AT-PS-31`; `UAT-PS-07` |
| FR-PS-032 Lulus/Tidak lulus beralasan | `PBJ-DEC-074` | VAL-136 | `BE-PBJ-PS-011` | `FE-PBJ-PS-005` | `AT-PS-33` |
| FR-PS-040 perbandingan dan peringkat | `PBJ-DEC-085` | API `comparison` | `BE-PBJ-PS-012` | `FE-PBJ-PS-006` | `AT-PS-41` |
| FR-PS-041 negosiasi tidak menaikkan harga | `PBJ-ASM-005` | VAL-137, 138 | `BE-PBJ-PS-012` | `FE-PBJ-PS-006` | `AT-PS-34` |
| FR-PS-042 penetapan satu vendor | `PBJ-DEC-070`, `073`, `076`, `085` | API §Package Award; VAL-139..143 | `BE-PBJ-PS-013` | `FE-PBJ-PS-006` | `AT-PS-40`, `41`, `42`, `45`; `UAT-PS-10` |
| FR-PS-043 komitmen per garis pagu | `PBJ-DEC-078`, `084`, `092`, `094` | INT §1; STATE §3 | `BE-PBJ-PS-013` | `FE-PBJ-PS-006` | `AT-PS-43`, `44`, `46`; `UAT-PS-09` |
| FR-PS-044 cabut penetapan sebelum PO | `PBJ-DEC-093` | VAL-146 | `BE-PBJ-PS-013` | `FE-PBJ-PS-006` | `AT-PS-55` |
| FR-PS-050 nomor PO unik | `PBJ-DEC-011`; `PBJ-ASM-009` | VAL-145, 146 | `BE-PBJ-PS-014` | `FE-PBJ-PS-007` | `AT-PS-50`, `51`; `UAT-PS-11`, `12` |
| FR-PS-051 unit peminta melihat paketnya | `PBJ-DEC-079`, `080` | API `request-summary` | `BE-PBJ-PS-014` | `FE-PBJ-PS-007` | `UAT-PS-11` |
| Lampiran harga tertutup | `PBJ-DEC-079`; `PBJ-CAP-004` | API §Perubahan sub-modul lain | `BE-PBJ-PS-006` | `FE-PBJ-PS-003`, `005` | `AT-PS-31` (bagian lampiran) |
| Label "termasuk PPN" di PR dan pagu | `PBJ-DEC-084` | API §Perubahan sub-modul lain | — | `FE-PBJ-PS-007` | Tangkapan layar PR dan pagu |
| Standar layar daftar | `PBJ-FE-004` | FE §4 | — | `FE-PBJ-PS-002`, `004`, `005`, `006` | `AT-PS-65` |
| Setiap layar terjangkau | `PBJ-FE-001` | FE §3 | — | semua task FE; final `FE-PBJ-PS-008` ⛔ | `AT-PS-63` |
| Registry dan konformitas | `PBJ-DEC-096` | Arsitektur §5 | `BE-PBJ-PS-001` ✅, `004` ✅ | — | `AT-PS-60` ✅ (`BE-PBJ-PS-001` QBE 19 berkas; `BE-PBJ-PS-004` QBE 32 berkas, `QBE-MOD-002` folder `Sourcing`) |
| Build dan skema | — | Kamus data | `BE-PBJ-PS-001` ✅, `003` ✅, `004` ✅ | semua task FE (build) | `AT-PS-61`, `62` — ✅ backend (`001` master, `003` kolom baris PR, `004` 14 tabel Sourcing) (3 tabel master cocok kamus data, delta D-1/D-3) |
| Regresi `perencanaan-permintaan` | `PBJ-DEC-095` | `PBJ-PP-INT` 1.2 | `BE-PBJ-PS-003` ✅ (alur PR 10/10), `005`, `006` | `FE-PBJ-PS-007` | DoD `04-prd-to-mvp.md` bagian 19 |

## 2. Keputusan → task

| Keputusan | Task |
| --- | --- |
| `PBJ-DEC-066` (batas scope) | Seluruh roadmap |
| `PBJ-DEC-067`, `087` (penawaran dicatat Tim Pengadaan; RFQ di luar sistem) | `BE-PBJ-PS-009`, `010`; `FE-PBJ-PS-004` |
| `PBJ-DEC-068`, `069`, `086` (metode berkonfigurasi, nilai contoh, dasar HPS) | `BE-PBJ-PS-001` ✅, `002` ✅, `007`; `FE-PBJ-PS-001`, `003` |
| `PBJ-DEC-070` (satu paket satu pemenang) | `BE-PBJ-PS-004` ✅ (unik penetapan hidup), `013` |
| `PBJ-DEC-071`, `082`, `090`, `091` (isi paket baris PR; satu jenis; satu tahun) | `BE-PBJ-PS-003` ✅, `004` ✅, `005`; `FE-PBJ-PS-002` |
| `PBJ-DEC-072` (dua persetujuan `Wfl`) | `BE-PBJ-PS-001`, `008`, `013`; `FE-PBJ-PS-003`, `006` |
| `PBJ-DEC-073` (HPS batas atas) | `BE-PBJ-PS-007`, `013`; `FE-PBJ-PS-006` |
| `PBJ-DEC-074`, `080` (evaluator ditunjuk; bundel per peran) | `BE-PBJ-PS-006`, `007`, `011`; `FE-PBJ-PS-005` |
| `PBJ-DEC-075` (RFQ gagal) | `BE-PBJ-PS-009`; `FE-PBJ-PS-004` |
| `PBJ-DEC-076`, `092` (kelayakan saat diajukan dan diterapkan) | `BE-PBJ-PS-013` |
| `PBJ-DEC-077` (batal sebelum PO) | `BE-PBJ-PS-005` (`ReleaseLinesAsync`), `015`; `FE-PBJ-PS-007` |
| `PBJ-DEC-078`, `094` (komitmen per baris asal; garis pagu wajib ada) | `BE-PBJ-PS-013` |
| `PBJ-DEC-079` (harga terbatas) | `BE-PBJ-PS-005`, `006`, `011`, `014`; `FE-PBJ-PS-002`, `005`, `007` |
| `PBJ-DEC-081` (batas waktu mengikat) | `BE-PBJ-PS-009`, `010`; `FE-PBJ-PS-004` |
| `PBJ-DEC-083` (pecah, metode tetap) | `BE-PBJ-PS-015`; `FE-PBJ-PS-007` |
| `PBJ-DEC-084` (termasuk PPN) | `BE-PBJ-PS-010`, `013`; `FE-PBJ-PS-007` (label) |
| `PBJ-DEC-085` (terendah atau alasan) | `BE-PBJ-PS-012`, `013`; `FE-PBJ-PS-006` |
| `PBJ-DEC-088` (notifikasi lewat `Wfl` dan tanda di daftar) | `FE-PBJ-PS-004`, `005` |
| `PBJ-DEC-089` (evaluasi teknis per kategori) | Tidak masuk roadmap ini — `POST-MVP` |
| `PBJ-DEC-093` (cabut penetapan) | `BE-PBJ-PS-013`; `FE-PBJ-PS-006` |
| `PBJ-DEC-095` (approval desain, `PBJ-PP-INT` 1.2) | `BE-PBJ-PS-003` ✅, `005`, `006`; `FE-PBJ-PS-007` |
| `PBJ-DEC-096` (registry `Sourcing`) | `BE-PBJ-PS-001` ✅, `004` ✅ |
| `PBJ-ASM-005` (negosiasi) | `BE-PBJ-PS-012` |
| `PBJ-ASM-006` (RFQ ke vendor layak) | `BE-PBJ-PS-009` |
| `PBJ-ASM-007` (dokumen wajib persiapan) | `BE-PBJ-PS-001`, `007`, `008` |
| `PBJ-ASM-008` (nomor `PKT`/`RFQ`) | `BE-PBJ-PS-001`, `008`, `009` |
| `PBJ-ASM-009` (nomor PO BLM) | `BE-PBJ-PS-014` |

## 3. Matriks uji → task

| Bagian `acceptance-test-matrix.md` | ID | Task |
| --- | --- | --- |
| 1. Pembentukan paket | `AT-PS-01`..`06` | `BE-PBJ-PS-005`; `FE-PBJ-PS-002` (`01`, `02`) |
| 2. Persiapan dan persetujuan | `AT-PS-10`..`18` | `BE-PBJ-PS-007` (`11`, `12`, `17`), `008` (`10`, `12`..`16`, `18`); `FE-PBJ-PS-003` |
| 3. RFQ dan penawaran | `AT-PS-20`..`26` | `BE-PBJ-PS-009` (`20`..`22`, `24`), `010` (`23`, `25`, `26`); `FE-PBJ-PS-004` |
| 4. Evaluasi dan negosiasi | `AT-PS-30`..`35` | `BE-PBJ-PS-011` (`30`..`33`), `012` (`34`), `009` (`35`); `FE-PBJ-PS-005`, `006` |
| 5. Penetapan dan komitmen | `AT-PS-40`..`46` | `BE-PBJ-PS-013`; `FE-PBJ-PS-006` (`40`, `41`) |
| 6. PO, batal, pecah | `AT-PS-50`..`55` | `BE-PBJ-PS-014` (`50`, `51`), `015` (`52`..`54`), `013` (`55`); `FE-PBJ-PS-006` (`55`), `007` |
| 7. Fondasi dan konformitas | `AT-PS-60`..`65` | `BE-PBJ-PS-001`, `004` (`60`..`62`, `64`); FE (`61`, `63`, `65`) |

Setiap `AT-PS-*` dan `UAT-PS-01`..`14` punya sekurang-kurangnya satu task.

## 4. Gap traceability

| Gap | Keterangan | Tindakan |
| --- | --- | --- |
| `PBJ-OQ-038` | Nama 11 submenu menu 4 dan 6 | `FE-PBJ-PS-001` memakai label usulan; `FE-PBJ-PS-008` ⛔ sampai keputusan ada; `AT-PS-63` belum dapat dinyatakan lulus penuh |
| `PBJ-OQ-005`, `PBJ-OQ-019` | Nilai metode, batas nilai, dan penyetuju dari SOP | Seeder memakai nilai contoh `IsSample`; uji persetujuan memakai matriks uji dev; pemakaian nyata menunggu SOP |
| Bundel izin `PBJ-DEC-080` | Belum diterapkan ke jabatan nyata | Tindakan Admin Akses Role sebelum pemakaian nyata (pola K-2) |
| Pemilik keamanan | Belum ditunjuk untuk seluruh sub-modul Pengadaan (K-3, VD K-5) | Syarat sign-off kesiapan, bukan task kode |
| `PBJ-DEC-089` | Wajib evaluasi teknis per kategori | `POST-MVP`; selama MVP pilih metode yang mewajibkan evaluasi teknis |
| Kamus data vs QBE | `SortOrder` pada `MstProcurementMethod` tidak dibuat; sisi inklusif rentang HPS ditetapkan `Min < HPS ≤ Max` (`BE-PBJ-PS-001` D-1, D-2) | Pemilik desain menyelaraskan kamus data; `BE-PBJ-PS-002` dan `007` memakai tafsiran yang sama |
| `PBJ-VAL-091` beda status | `PBJ-PP-VAL` 409 (selesaikan verifikasi PR) vs `PBJ-PS-VAL`/`AT-PS-14` 422 (ajukan persiapan) — ditemukan `BE-PBJ-PS-003` | Pemilik desain menetapkan satu status sebelum `BE-PBJ-PS-008` |
| Penanda izin eksplisit | `ProcurementPackage : ReadAllUnits`, `ReadPrice` belum terdaftar; dipindah dari `BE-PBJ-PS-004` (D-2) | Wajib dibuat di `BE-PBJ-PS-005` bersama `ProcurementPackagesController` |
| Requirement MVP tanpa task | Tidak ada | — |
