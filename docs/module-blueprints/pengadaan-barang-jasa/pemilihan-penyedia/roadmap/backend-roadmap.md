# Pemilihan Penyedia — Roadmap Backend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `pemilihan-penyedia` (`approved`, `PBJ-DEC-095`) |
| Revisi roadmap | `1` · status `draft` (rencana; tiap task tetap butuh perintah eksekusi tersendiri) |
| Tanggal | 26 November 2026 |
| Kontrak | set `1.0`: `PBJ-PS-API`, `-STATE`, `-VAL`, `-INT`, `-PERM`, `-TEST`, `-MVP` (semua `approved`); `PBJ-PP-INT` 1.2 |
| Keputusan | `PBJ-DEC-066`..`096`; `PBJ-ASM-005`..`009`; `PBJ-AC-022`..`036`; `PBJ-FE-004` (`decision_revision 19`) |
| SHA | BE `45b22777` (branch `sukmagp`; catatan approval 26 November 2026 masih di working tree), FE `3e5386679` (branch `sukmagpV2`) |
| Hash masukan | Lihat `../blueprint-manifest.md` (`input_hashes`, `artifact_hashes`) |
| Repository target | `NewQuilvianSystemBackend` |
| Registry | Submodule `ProcurementManagement / Sourcing` (`Prc`) `ACTIVE` (`PBJ-DEC-096`); master di `MasterData` (`Mst`) |

**Untuk setiap task:** QBE preflight, kesesuaian engineering, penetapan branch, dan wewenang migration diselesaikan
**pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical. Roadmap ini tidak memberi wewenang itu.

**Migration:** tiga task mengubah skema (`BE-PBJ-PS-001`, `003`, `004`), sesuai urutan 8–10 di `../../02-module-map.md`
bagian 3.2. Task menyertakan berkas migration, Designer, dan snapshot. Pembuatan dan penerapan migration ke basis data
masing-masing butuh konfirmasi pemilik pada saat eksekusi (`CLAUDE.md` backend); basis data uji hanya `QuilvianNewDevSukma`.

**Kebijakan uji backend:** tanpa proyek test. Bukti setiap task: QBE, review diff, build Release, uji API manual, dan bukti
basis data sesuai `testing/acceptance-test-matrix.md`, memakai aktor non-SuperAdmin yang dibuat lewat API. Uji persetujuan
memakai matriks `Wfl` **uji** yang dipasang lalu dicabut sesudahnya (pola `BE-PBJ-008`, `BE-PBJ-VD-007`); definisi
`PBJ-PKT-*` tetap tidak aktif sampai SOP (`PBJ-OQ-019`).

**Menyentuh `perencanaan-permintaan`:** `BE-PBJ-PS-003` (kolom baris PR), `BE-PBJ-PS-005` (tiga metode `PBJ-PP-INT` 1.2 di
`ProcurementPurchaseRequestService`), dan `BE-PBJ-PS-006` (saringan lampiran harga di `Common`). Perubahan itu dirancang dan
disetujui sebagai bagian sub-modul ini (`PBJ-DEC-095`; urutan migration 9 milik `pemilihan-penyedia`), sehingga task-nya ada
di roadmap ini, bukan di roadmap `perencanaan-permintaan`. Ketiganya wajib membuktikan regresi `perencanaan-permintaan`
(DoD `04-prd-to-mvp.md` bagian 19).

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; acceptance criteria dan DoD terbukti di laporan task |
| 🟡 | Sebagian; belum terbukti penuh |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Grafik dipotong menjadi tiga blok supaya lebar baris tetap di bawah 100 karakter. `BE-PBJ-PS-006` dan `BE-PBJ-PS-010` pada
blok berikutnya adalah node yang sama (sambungan), bukan task baru.

Blok 1 — fondasi, paket draf, lampiran:

```text
BE-PBJ-PS-001 ✅ ─┬─> BE-PBJ-PS-002 ✅
                  │
                  └─> BE-PBJ-PS-003 ✅ ─> BE-PBJ-PS-004 ✅ ─> BE-PBJ-PS-005 ─> BE-PBJ-PS-006
```

Blok 2 — persiapan, persetujuan, RFQ, penawaran:

```text
BE-PBJ-PS-006 ─> BE-PBJ-PS-007 ─> BE-PBJ-PS-008 ─> BE-PBJ-PS-009 ─> BE-PBJ-PS-010
```

Blok 3 — evaluasi, negosiasi, penetapan, nomor PO, pecah dan batal:

```text
BE-PBJ-PS-010 ─> BE-PBJ-PS-011 ─> BE-PBJ-PS-012 ─> BE-PBJ-PS-013 ─> BE-PBJ-PS-014 ─> BE-PBJ-PS-015
```

Jumlah pasangan prasyarat → task: **14** (blok 1: 5, blok 2: 4, blok 3: 5), sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-PBJ-PS-001` ✅ — 26 November 2026 |
| 2 | `BE-PBJ-PS-001` | `BE-PBJ-PS-002` ✅, `BE-PBJ-PS-003` ✅ — 26 November 2026 |
| 3 | `BE-PBJ-PS-003` | `BE-PBJ-PS-004` ✅ — 26 November 2026 |
| 4 | `BE-PBJ-PS-004` | `BE-PBJ-PS-005` |
| 5 | `BE-PBJ-PS-005` | `BE-PBJ-PS-006` |
| 6 | `BE-PBJ-PS-006` | `BE-PBJ-PS-007` |
| 7 | `BE-PBJ-PS-007` | `BE-PBJ-PS-008` |
| 8 | `BE-PBJ-PS-008` | `BE-PBJ-PS-009` |
| 9 | `BE-PBJ-PS-009` | `BE-PBJ-PS-010` |
| 10 | `BE-PBJ-PS-010` | `BE-PBJ-PS-011` |
| 11 | `BE-PBJ-PS-011` | `BE-PBJ-PS-012` |
| 12 | `BE-PBJ-PS-012` | `BE-PBJ-PS-013` |
| 13 | `BE-PBJ-PS-013` | `BE-PBJ-PS-014` |
| 14 | `BE-PBJ-PS-014` | `BE-PBJ-PS-015` |

Rantainya sengaja lurus. Setiap tahap membutuhkan data tahap sebelumnya agar kriterianya dapat diuji lewat API: penawaran
butuh RFQ terkirim, evaluasi butuh penawaran, penetapan butuh hasil evaluasi. Frontend berjalan sejajar sejak master siap
(`frontend-roadmap.md`).

**Pemetaan ke gelombang MVP** (`04-prd-to-mvp.md` bagian 20):

- `MVP-PS0` = 001–004 (registry sudah `ACTIVE`; `PS-01`; kolom baris PR; skema).
- `MVP-PS1` = 005–008 (`PS-02` kecuali pecah dan batal; tiga metode baris PR; lampiran harga tertutup).
- `MVP-PS2` = 009–011 (`PS-03`, `PS-04`).
- `MVP-PS3` = 012–015 (`PS-05`, `PS-06`, ditambah pecah dan batal paket).

**Penyimpangan urutan yang disengaja.** Kriteria tetap sama, hanya waktu pembuktiannya yang bergeser:

| Butir PRD | Gelombang PRD | Dibangun di | Sebab |
| --- | --- | --- | --- |
| Tiga metode baris PR dan saringan lampiran | `MVP-PS0` | `BE-PBJ-PS-005`, `006` (`MVP-PS1`) | Metode baris PR dan lampiran `PBJ_PACKAGE` baru dapat dibuktikan setelah paket dapat dibuat lewat API |
| `FR-PS-015` pecah, `FR-PS-016` batal | `MVP-PS1` | `BE-PBJ-PS-015` (`MVP-PS3`) | Pecah menyalin penawaran dan hasil evaluasi (`PBJ-DEC-083`); batal melepas komitmen penetapan dan menolak paket `ORDERED` (`PBJ-AC-033`). `AT-PS-52`..`54` butuh data itu |

## Tabel task

| Task ID | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-PBJ-PS-001` | Fondasi Sourcing: konstanta, tiga master, data awal, dua definisi `Wfl` | `PBJ-DEC-068`, `069`, `072`, `086`, `096`; `PBJ-ASM-007`, `008`; FR-PS-003 | Kamus data §master; arsitektur §5, §8–§10 | `IdentityModel`, seeder Pengadaan, `ProcurementWorkflowDefinitionSeeder` | 3 model master + konfigurasi, migration `AddProcurementSourcingMasterData`, konstanta `Common`, seeder master, 2 definisi tidak aktif | — | `AT-PS-60`, `61`, `62` (master), `64` | QBE, build, skema DB, seeder 2× | Konstanta `Common` dipakai bersama; pemilik: builder backend | Kriteria + laporan | ✅ 26 November 2026 ([laporan](../task/report/backend/BE-PBJ-PS-001.md)) |
| `BE-PBJ-PS-002` | Tiga master pemilihan dapat dikelola | `PBJ-DEC-068`, `095`; `PBJ-ASM-007`; FR-PS-001, 002 | API §Master Data (3 grup); VAL-071 | Pola master `ProcurementVendorDocumentTypes*` | 3 controller × 9 endpoint, 3 service | `BE-PBJ-PS-001` | `UAT-PS-01`, `02`; rentang metode aktif tidak tumpang tindih; dokumen wajib tepat satu dari metode/jenis | API manual | — | Kriteria + laporan | ✅ 26 November 2026 ([laporan](../task/report/backend/BE-PBJ-PS-002.md)) |
| `BE-PBJ-PS-003` | Baris PR siap membawa rujukan paket | `PBJ-DEC-071`, `095` | Kamus data `PrcPurchaseRequestLine`; API §Perubahan sub-modul lain | `PrcPurchaseRequestLine`, `ProcurementPurchaseRequestService` (detail) | 2 kolom + index, migration `AddProcurementPurchaseRequestLinePackage`, `packageId` di detail PR | `BE-PBJ-PS-001` | Skema cocok; `packageId` kosong di PR lama; regresi PR | Build, skema DB, API PR | **Menyentuh `perencanaan-permintaan`**; pemilik PR = pengguna sesi (`PBJ-DEC-095`) | Kriteria + regresi + laporan | ✅ 26 November 2026 ([laporan](../task/report/backend/BE-PBJ-PS-003.md)) |
| `BE-PBJ-PS-004` | Skema 14 tabel Sourcing berdiri | `PBJ-DEC-070`, `071`, `074`, `078`, `083`, `090`, `091`, `096` | Kamus data §Sourcing; arsitektur §3, §6 | `IdentityModel`, `RowVersion` pola Pengadaan | 14 model + konfigurasi, index unik parsial, migration `AddProcurementSourcing`, penanda izin eksplisit, pendaftaran DI | `BE-PBJ-PS-003` | `AT-PS-60`, `61`, `62` | QBE, build, skema DB, uji batasan (rollback) | Migration besar; jalankan sebelum data produksi ada | Kriteria + laporan | ✅ 26 November 2026 ([laporan](../task/report/backend/BE-PBJ-PS-004.md)) |
| `BE-PBJ-PS-005` | Paket draf dibuat dari baris PR | `PBJ-DEC-020`, `022`, `071`, `077`, `079`, `082`, `090`, `091`; FR-PS-010, 011 | API §Procurement Package (daftar, buat, baris, detail, riwayat); `PBJ-PP-INT` 1.2 §2; VAL-120..123 | `ProcurementDataScopeService`, `PrcDocumentHistory`, `ProcurementPurchaseRequestService` | `ProcurementPackagesController` dan `ProcurementPackageService` bagian draf; tiga metode baris PR | `BE-PBJ-PS-004` | `AT-PS-01`..`06`; regresi PR | API manual, DB | **Menyentuh `perencanaan-permintaan`**; harga tertutup di DTO, bukan di layar | Kriteria + regresi + laporan | — |
| `BE-PBJ-PS-006` | Lampiran paket aman dan lampiran harga tertutup | `PBJ-DEC-074`, `079`, `080`; `PBJ-CAP-004`, `026` | API §Perubahan sub-modul lain (`/attachments`); PERM §4 | `ProcurementAttachmentService`, `IProcurementDocumentAccessProvider` | Saringan `IsPriceSensitive`, `ProcurementSourcingAccessProvider` (`PBJ_PACKAGE`, `PBJ_RFQ`) | `BE-PBJ-PS-005` | Unggah/daftar oleh Tim Pengadaan; tanpa `ReadPrice` tidak melihat lampiran harga; regresi lampiran | API manual | **Menyentuh `Common` milik `perencanaan-permintaan`** | Kriteria + regresi + laporan | — |
| `BE-PBJ-PS-007` | Persiapan paket lengkap | `PBJ-DEC-068`, `073`, `074`, `086`; `PBJ-ASM-007`; FR-PS-012, 013 | API `PUT /{id}`, `method-suggestion`, `criteria`, `evaluators`; VAL-125, 126, 149 | `PrcAttachment` (checklist) | Ubah persiapan, saran dan salinan metode, salinan checklist, kriteria, evaluator | `BE-PBJ-PS-006` | `AT-PS-11`, `12` (validasi), `17` | API manual, DB | — | Kriteria + laporan | — |
| `BE-PBJ-PS-008` | Persiapan disetujui lewat `Wfl` | `PBJ-DEC-007`, `038`, `072`, `073`; FR-PS-014 | API `preparation/*`, `approval`, `workflow/synchronize`; STATE §1; VAL-091, 124, 127, 128 | `ProcurementApprovalGateway`, `ProcurementWorkflowLifecycleHandler`, `NumberSeriesAllocator` | Ajukan/tarik/buka ulang, penerap `PBJ_PACKAGE`, kondisi `PBJ_METHOD_OUT_OF_RANGE`, nomor `PKT` | `BE-PBJ-PS-007` | `AT-PS-10`, `12` (kondisi), `13`, `14`, `15`, `16`, `18` | API, DB; matriks uji `Wfl` | Matriks uji wajib dicabut sesudah uji | Kriteria + laporan | — |
| `BE-PBJ-PS-009` | RFQ ke vendor layak | `PBJ-DEC-045`, `054`, `075`, `081`, `087`; `PBJ-ASM-006`; FR-PS-020, 021, 023 | API §Procurement RFQ; STATE §2; VAL-129..132, 148 | `ProcurementVendorEligibilityService.GetEligibleAsync`, QuestPDF (pola `AssetMovementDocumentService`) | `ProcurementRfqsController`, `ProcurementRfqService`, dokumen RFQ PDF, nomor `RFQ` | `BE-PBJ-PS-008` | `AT-PS-20`, `21`, `22`, `24`, `35` | API, DB | Zona waktu batas waktu (`NFR-005`) | Kriteria + laporan | — |
| `BE-PBJ-PS-010` | Penawaran tercatat dengan PPN dan waktu terima | `PBJ-DEC-067`, `081`, `084`; FR-PS-022 | API §Procurement Quotation (catat, ubah, void, daftar, detail); VAL-133, 134, 148 | `MstSupplier` (penanda pajak) | `ProcurementQuotationsController` dan `ProcurementQuotationService` bagian catat | `BE-PBJ-PS-009` | `AT-PS-23`, `25`, `26` | API, DB | Harga tidak masuk log (`NFR-006`) | Kriteria + laporan | — |
| `BE-PBJ-PS-011` | Evaluasi teknis tanpa harga oleh evaluator yang ditunjuk | `PBJ-DEC-074`, `079`, `080`; FR-PS-030..032 | API §Technical Evaluation; VAL-135, 136 | `ProcurementSourcingAccessProvider` (`BE-PBJ-PS-006`) | `TechnicalEvaluationsController`, `ProcurementTechnicalEvaluationService` | `BE-PBJ-PS-010` | `AT-PS-30` (API), `31`, `32`, `33` | API, DB; aktor evaluator tanpa `ProcurementPackage : Read` | Kebocoran harga lewat DTO; uji respons mentah | Kriteria + laporan | — |
| `BE-PBJ-PS-012` | Negosiasi dan perbandingan harga | `PBJ-DEC-073`, `085`; `PBJ-ASM-005`; FR-PS-040, 041 | API `negotiations`, `comparison`; VAL-137, 138 | — | Negosiasi per putaran, perbandingan dan peringkat | `BE-PBJ-PS-011` | `AT-PS-34`; terendah ditandai; penawaran terlambat/tidak lengkap tidak berperingkat | API, DB | — | Kriteria + laporan | — |
| `BE-PBJ-PS-013` | Penetapan disetujui menjadi komitmen pagu per unit | `PBJ-DEC-070`, `073`, `076`, `078`, `085`, `092`, `093`, `094`; FR-PS-042..044 | API §Package Award; STATE §3; VAL-139..143, 146, 148; INT §1 | `ProcurementBudgetCommitmentService`, `ProcurementVendorEligibilityService.CheckAsync`, `ProcurementApprovalGateway` | `ProcurementPackageAwardsController`, `ProcurementAwardService`, penerap `PBJ_PACKAGE_AWARD`, kondisi `PBJ_OVER_HPS` | `BE-PBJ-PS-012` | `AT-PS-40`..`46`, `55` | API, DB; matriks uji `Wfl` | Jumlah komitmen = nilai penetapan; pembulatan ke sumber terakhir | Kriteria + laporan | — |
| `BE-PBJ-PS-014` | Nomor PO tercatat; unit peminta melihat paketnya | `PBJ-DEC-011`, `079`, `080`; `PBJ-ASM-009`; FR-PS-050, 051 | API `purchase-order`, `request-summary`; VAL-145, 146 | — | Catat nomor PO (`ORDERED`), ringkasan paket per PR tanpa harga | `BE-PBJ-PS-013` | `AT-PS-50`, `51`; `request-summary` tanpa field harga | API, DB; aktor Unit Peminta | — | Kriteria + laporan | — |
| `BE-PBJ-PS-015` | Paket dapat dipecah dan dibatalkan | `PBJ-DEC-077`, `083`; FR-PS-015, 016 | API `split`, `cancel`; STATE §1; VAL-144, 147, 148 | `ProcurementBudgetCommitmentService.ReleaseAsync`, `ReleaseLinesAsync` (`BE-PBJ-PS-005`) | `ProcurementPackageSplitService`, aksi batal | `BE-PBJ-PS-014` | `AT-PS-52`, `53`, `54` | API, DB | Salinan penawaran/evaluasi saat pecah | Kriteria + laporan | — |

## Kartu task

### ✅ `BE-PBJ-PS-001` — Fondasi Sourcing

| Aspek | Isi |
| --- | --- |
| Cakupan | Model `MstProcurementMethod`, `MstProcurementPackageDocumentType`, `MstProcurementPackageDocumentRequirement` (folder `MasterData`) beserta konfigurasi EF (CHECK tepat satu dari `MethodId`/`ProcurementTypeId`); migration `AddProcurementSourcingMasterData`; konstanta `Common` (`ProcurementDocumentTypes` `PBJ_PACKAGE`/`PBJ_PACKAGE_AWARD`/`PBJ_RFQ`, `ProcurementNumberSeries` `PKT`/`RFQ`, `ProcurementPermissions` `SourcingModuleCode` + resource dan action `PERM` §2, `ProcurementStatuses` Package/Rfq/Quotation/Award, `ProcurementWorkflowCodes` `PBJ-PKT-PERSIAPAN`/`PBJ-PKT-PENETAPAN`/`PBJ_OVER_HPS`/`PBJ_METHOD_OUT_OF_RANGE`, aksi riwayat); `ProcurementSourcingMasterSeeder` (dua metode contoh `IsSample`, enam jenis dokumen, tiga dokumen wajib — arsitektur §10); dua definisi `Wfl` di `ProcurementWorkflowDefinitionSeeder` (tidak aktif, tanpa matriks) |
| Di luar cakupan | Endpoint apa pun; 14 tabel `Prc*` (`BE-PBJ-PS-004`) |
| Acceptance criteria | `AT-PS-60` registry `Sourcing` `ACTIVE` dan QBE; `AT-PS-61` build; `AT-PS-62` tiga tabel master cocok kamus data; `AT-PS-64` seeder dua kali tanpa baris berlipat, metode contoh bertanda `IsSample` |
| Verifikasi | QBE Strict, build Release, pemeriksaan skema `QuilvianNewDevSukma`, seeder 2× |
| DoD | Kriteria + laporan `task/report/backend/BE-PBJ-PS-001.md` |
| **Status** | ✅ **SELESAI 26 November 2026.** Tiga tabel master terpasang di `QuilvianNewDevSukma` dan cocok kamus data (tanpa `SortOrder`, D-1); build Release `--no-incremental` 0 error, 0 warning dari berkas baru/berubah; QBE Strict `PASS` 19 berkas; migration `AddProcurementSourcingMasterData` diterapkan atas izin pemilik, `has-pending-model-changes` bersih; uji batasan 16/16 `PASS` (rollback); seeder 2× tanpa baris berlipat (2 metode contoh `IsSample`, 6 jenis dokumen, 4 dokumen wajib, 2 definisi `PBJ-PKT-*` tidak aktif tanpa matriks). Delta D-1..D-11 di laporan, termasuk perbaikan snapshot model yang tertinggal sejak merge `c97642bb`. Laporan: [laporan](../task/report/backend/BE-PBJ-PS-001.md) |

### ✅ `BE-PBJ-PS-002` — Master pemilihan

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementMethodsController`, `ProcurementPackageDocumentTypesController`, `ProcurementPackageDocumentRequirementsController`, masing-masing sembilan endpoint baku (`PBJ-DEC-037`) dengan saringan tanggal/periode (`PBJ-DEC-055`); tiga service master |
| Acceptance criteria | (1) Sembilan endpoint per master berjalan. (2) Rentang HPS dua metode aktif tidak boleh tumpang tindih. (3) Dokumen wajib menolak isian yang kosong atau mengisi metode **dan** jenis sekaligus (400). (4) Hapus master yang sudah dipakai → 400 `PBJ-VAL-071`, dibuktikan dengan jenis dokumen yang dipakai dokumen wajib; cabang "metode dipakai paket" (`UAT-PS-02`) diuji di `BE-PBJ-PS-007`. (5) Ganti metode contoh dengan metode SOP lalu nonaktifkan contoh (`UAT-PS-01`) |
| Verifikasi | Uji API manual dengan aktor admin non-SuperAdmin; 403 untuk aktor tanpa izin |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 26 November 2026.** 27 endpoint (3 master × 9) dengan kode modul Sourcing; build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 9 berkas; uji runtime 61/61 `PASS` dengan aktor non-SuperAdmin (tumpang tindih rentang 409, batas Rp 50.000.000 tidak bentrok, `UAT-PS-01`, dua cakupan/tanpa cakupan 400, hapus terpakai 400 `PBJ-VAL-071`, 403/401); satu ekspektasi skrip (`DB-01`, huruf `true`) dikoreksi. Tanpa migration. Fixture uji dibersihkan. Cabang "metode dipakai paket" dan "kode terkunci oleh lampiran paket" baru dapat diuji di `BE-PBJ-PS-006`/`007` (D-5, D-7). Laporan: [laporan](../task/report/backend/BE-PBJ-PS-002.md) |

### ✅ `BE-PBJ-PS-003` — Kolom paket pada baris PR (menyentuh `perencanaan-permintaan`)

| Aspek | Isi |
| --- | --- |
| Cakupan | Kolom `PackageId` (`Guid?`, index) dan `PackagedAt` (`DateTimeOffset?`) pada `PrcPurchaseRequestLine` beserta konfigurasi; migration `AddProcurementPurchaseRequestLinePackage`; `GET /purchase-requests/{id}` membawa `packageId` per baris (aditif) |
| Di luar cakupan | Tiga metode `PBJ-PP-INT` 1.2 (`BE-PBJ-PS-005`, pemanggil pertamanya); mengisi `PrcPurchaseRequest.ConvertedPackageId` (sengaja tidak dibuat) |
| Acceptance criteria | (1) Dua kolom nullable dan index cocok kamus data; baris PR lama bernilai kosong. (2) Detail PR membawa `packageId: null`. (3) Regresi PR: buat, ajukan, verifikasi, batal tetap berjalan. (4) Migration dapat dijalankan tanpa henti layanan dan `Down` menghapus kolom |
| Verifikasi | QBE, build Release, skema DB, uji API PR yang sudah ada |
| Risiko / pemilik | Menyentuh tabel milik `perencanaan-permintaan`; disetujui `PBJ-DEC-095` |
| DoD | Kriteria + regresi + laporan |
| **Status** | ✅ **SELESAI 26 November 2026.** `PackageId` (`uuid` NULL, index) dan `PackagedAt` (`timestamptz` NULL) terpasang di `QuilvianNewDevSukma` lewat migration `AddProcurementPurchaseRequestLinePackage` (tanpa drift, atas izin pemilik); `has-pending-model-changes` bersih; 53 baris PR lama kosong; detail PR membawa `packageId` (aditif). Build Release 0 error (0 warning dari berkas berubah), QBE Strict `PASS` 7 berkas; regresi PR 10/10 `PASS` dengan aktor non-SuperAdmin (buat, ubah, ajukan `PR-000018`, cek stok, selesai verifikasi ditolak 409 `PBJ-VAL-091` fail-closed, batal). Fixture dibersihkan. Temuan: `PBJ-VAL-091` 409 di `PBJ-PP-VAL` vs 422 di `PBJ-PS-VAL` — diputuskan sebelum `BE-PBJ-PS-008`. Laporan: [laporan](../task/report/backend/BE-PBJ-PS-003.md) |

### ✅ `BE-PBJ-PS-004` — Skema Sourcing

| Aspek | Isi |
| --- | --- |
| Cakupan | 14 model di `Areas/Corporate/ProcurementManagement/Sourcing/Models/` dan konfigurasinya di `Repositories/Configurations/Corporate/ProcurementManagement/Sourcing/`; index unik parsial: `PrcPackageItemSource(PurchaseRequestLineId)` untuk baris yang belum dilepas, satu penetapan hidup per paket, nomor paket/RFQ/PO unik, satu RFQ `DRAFT`/`SENT` per paket; `RowVersion` pada paket, RFQ, penawaran, penetapan; migration `AddProcurementSourcing`; `ProcurementSourcingExplicitPermissions.cs` (`ReadAllUnits`, `ReadPrice`); pendaftaran DI di `ProcurementManagementServiceCollectionExtensions` |
| Di luar cakupan | Service dan endpoint |
| Acceptance criteria | `AT-PS-60` (QBE `QBE-MOD-002` lolos untuk folder `Sourcing`); `AT-PS-61`; `AT-PS-62` 14 tabel cocok kamus data dan `dotnet ef migrations has-pending-model-changes` bersih; uji batasan index unik dan CHECK dengan rollback |
| Verifikasi | QBE Strict, build Release, skema DB, uji batasan |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 26 November 2026.** 14 tabel Sourcing terpasang di `QuilvianNewDevSukma` lewat migration `AddProcurementSourcing` (14 `CreateTable`, 48 index, tanpa drift; atas izin pemilik) dan cocok kamus data: 16 index unik parsial, 28 CHECK, 33 FK `RESTRICT`, `RowVersion` pada paket/RFQ/penawaran/penetapan; `has-pending-model-changes` bersih. Build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 32 berkas (`QBE-MOD-002` folder `Sourcing`); uji batasan 26/26 `PASS` (rollback). Delta: `SortOrder` kriteria → `CriterionNo` (D-1); penanda izin eksplisit `ReadAllUnits`/`ReadPrice` **dipindah ke `BE-PBJ-PS-005`** karena registry menolak penanda ke resource tanpa endpoint (D-2); DI belum berubah karena belum ada service (D-3). Laporan: [laporan](../task/report/backend/BE-PBJ-PS-004.md) |

### `BE-PBJ-PS-005` — Paket draf dan tiga metode baris PR (menyentuh `perencanaan-permintaan`)

| Aspek | Isi |
| --- | --- |
| Cakupan | Metode `GetPackageableLinesAsync`, `MarkLinesPackagedAsync(lineIds, packageId)`, `ReleaseLinesAsync(lineIds, packageId)` di `ProcurementPurchaseRequestService` (`PBJ-PP-INT` 1.2 §2): ikut transaksi pemanggil, idempoten untuk paket yang sama, PR menjadi `CONVERTED_TO_PACKAGE` bila semua baris beli berpaket dan kembali `APPROVED` bila ada yang dilepas. Endpoint `GET /procurement-packages/filters/metadata`, `/`, `/packageable-request-lines`, `/{id}`, `/{id}/history`; `POST /` (gabungan per barang katalog, satu `PrcPackageItemSource` per baris PR beserta kunci garis pagu); `POST /{id}/lines`; `DELETE /{id}/sources/{sourceId}`; cakupan unit (`ReadAllUnits`) dan harga `null` tanpa `ReadPrice`; rujukan `PackagePlanId` |
| Acceptance criteria | `AT-PS-01` (15 kursi roda dari IGD 10 + Rawat Inap 5), `AT-PS-02` (409 `PBJ-VAL-121`), `AT-PS-03` (PR dua baris: `APPROVED` lalu `CONVERTED_TO_PACKAGE`; lepas baris → kembali `APPROVED`), `AT-PS-04` (422 `PBJ-VAL-122`/`123`), `AT-PS-05`, `AT-PS-06` (404 di luar cakupan); regresi PR |
| Risiko / pemilik | Menyentuh service milik `perencanaan-permintaan` (`PBJ-DEC-095`) |
| DoD | Kriteria + regresi + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-006` — Lampiran paket dan lampiran harga tertutup (menyentuh `Common`)

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementSourcingAccessProvider` untuk `PBJ_PACKAGE` dan `PBJ_RFQ`: Tim Pengadaan dan penyetuju `Read`/`Edit`; evaluator `Read` hanya pada paket dengan `PrcPackageEvaluator` aktif atas dirinya. `ProcurementAttachmentService`: lampiran berkategori jenis dokumen `IsPriceSensitive` tidak dikembalikan dan unduhannya 404 bagi pengguna tanpa `ProcurementPackage : ReadPrice`; kategori bebas ditolak untuk kedua jenis dokumen itu |
| Acceptance criteria | (1) Tim Pengadaan mengunggah dan melihat lampiran paket. (2) Aktor yang punya `ProcurementPackage : Read` tanpa `ReadPrice` (bundel Keuangan) tidak melihat Dasar HPS di daftar, dan unduhannya 404. (3) Kategori di luar daftar jenis dokumen ditolak. (4) Regresi lampiran PR, rencana, pagu, dan vendor tidak berubah. Cabang evaluator (`AT-PS-31` bagian lampiran) dibuktikan di `BE-PBJ-PS-011`, karena penugasan evaluator baru ada di `BE-PBJ-PS-007` |
| Risiko / pemilik | Menyentuh fondasi lampiran milik `perencanaan-permintaan` (`PBJ-DEC-095`) |
| DoD | Kriteria + regresi + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-007` — Persiapan paket

| Aspek | Isi |
| --- | --- |
| Cakupan | `PUT /{id}` (judul, spesifikasi, HPS satuan, metode, alasan di luar rentang); `GET /{id}/method-suggestion`; salinan metode dan aturannya saat dipilih; salinan checklist dari dokumen wajib metode dan jenis; status checklist dari lampiran `PBJ_PACKAGE` berkategori kode jenis; `PUT /{id}/items/{itemId}/criteria`; `PUT /{id}/evaluators`; HPS terkunci setelah disetujui (409 `PBJ-VAL-149`) |
| Acceptance criteria | `AT-PS-11` (HPS Rp 42.750.000 → saran Penunjukan Langsung contoh), `AT-PS-12` (422 `PBJ-VAL-126` tanpa alasan), `AT-PS-17` (ubah master tidak mengubah salinan di paket); hapus metode yang dipakai paket → 400 `PBJ-VAL-071` (`UAT-PS-02`) |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-008` — Persetujuan persiapan

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /{id}/preparation/submit` (validasi HPS, metode, checklist; nomor `PKT` saat pertama diajukan; `Wfl` dulu baru status), `/withdraw`, `/reopen`; `GET /{id}/approval`; `POST /{id}/workflow/synchronize`; `ProcurementSourcingStatusApplier` untuk `PBJ_PACKAGE` (disetujui → `READY_FOR_RFQ` + `ApprovedHpsTotal`; dikembalikan → `PREPARATION_RETURNED`; ditolak → `REJECTED` + `ReleaseLinesAsync`; ditarik → `DRAFT`); kondisi `PBJ_METHOD_OUT_OF_RANGE` |
| Acceptance criteria | `AT-PS-10`, `12` (langkah tambahan aktif), `13`, `14` (422 `PBJ-VAL-091`, paket tetap `DRAFT`), `15`, `16`, `18` |
| Verifikasi | API dan DB; matriks `Wfl` uji dipasang lalu dicabut atas izin pemilik |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-009` — RFQ

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementRfqsController`: metadata, daftar, `eligible-vendors` (irisan kelayakan semua kategori paket), buat/ubah draf, kirim (nomor `RFQ`, paket `RFQ_OPEN`), perpanjang batas waktu sebelum lewat (berlaku untuk semua undangan, tercatat), tutup (paket `EVALUATION`), gagal (paket `READY_FOR_RFQ`, RFQ lama `FAILED` tersimpan), dokumen RFQ PDF tanpa HPS (`PBJ-DEC-087`) |
| Acceptance criteria | `AT-PS-20` (409 `PBJ-VAL-129`), `21` (422 `PBJ-VAL-130`), `22` (422 `PBJ-VAL-131`), `24`, `35` (RFQ ulang bernomor baru) |
| Risiko / pemilik | Batas waktu dibandingkan dalam zona waktu operasional (`AppDateTimeHelper`); PDF memakai QuestPDF yang sudah dipakai `AssetManagement` |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-010` — Pencatatan penawaran

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /procurement-quotations` (waktu diterima, `IsLate` terhadap batas waktu, PPN bawaan dari `MstSupplier`, harga akhir termasuk PPN, `IsComplete`), `PUT /{id}` (sebelum evaluasi/negosiasi), `POST /{id}/void`, `GET /` per RFQ atau paket, `GET /{id}`; pencatat tersimpan untuk aturan `PBJ-DEC-074` |
| Acceptance criteria | `AT-PS-23` (17.10 vs 16.00 → `IsLate`), `25` (`FinalUnitPrice` 2.775.000,00), `26` (422 `PBJ-VAL-133`, 409 `PBJ-VAL-134`) |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-011` — Evaluasi teknis

| Aspek | Isi |
| --- | --- |
| Cakupan | `GET /technical-evaluations` (paket yang menugaskan saya), `GET /{packageId}` (tanpa satu pun field harga), `PUT /{packageId}/quotations/{quotationId}` (Lulus/Tidak lulus per baris × kriteria); pencatat penawaran ditolak 403 `PBJ-VAL-135`; paket tidak ditugaskan → 404 |
| Acceptance criteria | `AT-PS-30` (sisi API), `31` (respons tanpa field harga; lampiran Surat Penawaran Harga tidak terdaftar dan unduhannya 404), `32`, `33` (422 `PBJ-VAL-136`) |
| Verifikasi | Respons JSON mentah diperiksa untuk nama field harga apa pun |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-012` — Negosiasi dan perbandingan

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /procurement-quotations/{id}/negotiations` (putaran, peserta, ringkasan, harga baru per baris; tidak boleh naik; hanya penawaran sah dan Lulus); `GET /comparison?packageId=` (per vendor: total akhir, sah/terlambat/tidak lengkap, hasil evaluasi, kelayakan, peringkat, terendah) |
| Acceptance criteria | `AT-PS-34` (422 `PBJ-VAL-137`, `PBJ-VAL-138`); perbandingan menandai terendah di antara penawaran sah, Lulus, dan layak (`PBJ-DEC-085`) |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-013` — Penetapan dan komitmen pagu

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementPackageAwardsController`: daftar, detail, ajukan (aturan HPS, terendah, minimum penawaran, kelayakan saat diajukan; `RankingSnapshotJson`), ajukan ulang, tarik, cabut sebelum PO (`PBJ-DEC-093`), sinkron ulang; penerap `PBJ_PACKAGE_AWARD`: cek ulang kelayakan (tidak layak → `NOT_APPLIED`, `PBJ-DEC-092`), `FindCurrentLineageAsync` per sumber (garis pagu tidak ada → tidak diterapkan + galat sinkron, `PBJ-DEC-094`), `CommitAsync` per garis pagu; kondisi `PBJ_OVER_HPS` |
| Acceptance criteria | `AT-PS-40`..`46`, `55` |
| Verifikasi | API dan DB (jumlah komitmen = nilai penetapan); matriks `Wfl` uji dipasang lalu dicabut |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-014` — Nomor PO dan ringkasan untuk PR

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /procurement-packages/{id}/purchase-order` (setelah penetapan disetujui, nomor unik antar paket, paket `ORDERED`); `GET /procurement-packages/request-summary?purchaseRequestId=` (`ReadSummary`; nomor, status, vendor terpilih; tanpa harga) |
| Acceptance criteria | `AT-PS-50` (409 `PBJ-VAL-145` menyebut paket pemakainya), `51` (409 `PBJ-VAL-146`); `request-summary` bagi Unit Peminta tanpa field harga |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

### `BE-PBJ-PS-015` — Pecah dan batal paket

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /{id}/split` sampai sebelum penetapan diajukan (paket baru membawa item, sumber, kriteria, evaluator, RFQ warisan, penawaran, hasil evaluasi, metode asal; nomor baru bila sumber sudah bernomor; baris PR pindah `PackageId`); `POST /{id}/cancel` untuk `DRAFT`..`AWARDED` tanpa persetujuan berjalan (lepas komitmen bila ada, `ReleaseLinesAsync`, PR kembali `APPROVED`); `ORDERED` ditolak |
| Acceptance criteria | `AT-PS-52` (409 `PBJ-VAL-147` untuk `ORDERED`), `53`, `54` (409 `PBJ-VAL-144`) |
| DoD | Kriteria + laporan |
| **Status** | Belum dikerjakan |

## Tidak masuk roadmap ini

- Wajib evaluasi teknis per kategori (`PBJ-DEC-089`) — `POST-MVP`.
- Laporan "direncanakan vs dipaketkan" dan `GetPackagedQuantityByNeedPlanLineKeysAsync` — `POST-MVP`.
- `GetOrderedAsync` untuk `kontrak-pelaksanaan` — dikunci saat sub-modul itu dirancang.
- Pencocokan nomor PO ke PO Finance — menunggu `PBJ-CAP-014` dan `FIN-OQ-141`.
- Notifikasi aktif di luar `Wfl` — menunggu `PBJ-OQ-015`.
- Pengisian matriks persetujuan nyata dan aktivasi definisi `PBJ-PKT-*` — tindakan admin `Wfl` dari SOP (`PBJ-OQ-019`), bukan task kode.
- Penerapan bundel izin `PBJ-DEC-080` ke jabatan nyata — tindakan Admin Akses Role, bukan task kode.
