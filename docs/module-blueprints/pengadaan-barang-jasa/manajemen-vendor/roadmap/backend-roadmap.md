# Manajemen Vendor — Roadmap Backend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `manajemen-vendor` (`approved`, `PBJ-DEC-062`) |
| Revisi roadmap | `1` · status `draft` (rencana; tiap task tetap butuh perintah eksekusi tersendiri) |
| Tanggal | 17 November 2026 |
| Kontrak | set `1.0`: `PBJ-VD-API`, `-STATE`, `-VAL`, `-INT`, `-PERM`, `-TEST`, `-MVP` (semua `approved`) |
| Keputusan | `PBJ-DEC-010`, `015`, `021`, `043`..`054`, `059`..`063`; `PBJ-ASM-001`..`004`; `PBJ-FE-004` (`decision_revision 16`) |
| SHA | BE `c6b86a6e` (branch `sukmagp`), FE `059b6c612` (branch `sukmagpV2`) |
| Hash masukan | Lihat `../blueprint-manifest.md` (`input_hashes`, `artifact_hashes`) |
| Repository target | `NewQuilvianSystemBackend` |
| Registry | Submodule `ProcurementManagement / Vendor` (`Prc`) `ACTIVE` (`PBJ-DEC-063`); master di `MasterData` (`Mst`) |

**Untuk setiap task:** QBE preflight, kesesuaian engineering, penetapan branch, dan wewenang migration diselesaikan
**pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical. Roadmap ini tidak memberi wewenang itu.

**Migration:** task yang mengubah skema menyertakan berkas migration, Designer, dan snapshot. Penerapan ke basis data
mengikuti izin pemilik pada saat eksekusi (preseden `perencanaan-permintaan`: diterapkan ke `QuilvianNewDevSukma` atas
izin).

**Kebijakan uji backend:** tanpa proyek test. Bukti setiap task: QBE, review diff, build Release, uji API manual, dan bukti
basis data sesuai `testing/acceptance-test-matrix.md`, memakai aktor non-SuperAdmin.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; acceptance criteria dan DoD terbukti di laporan task |
| 🟡 | Sebagian; belum terbukti penuh |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Grafik dipotong menjadi empat blok supaya tidak ada garis yang bersilangan. Node yang muncul lagi di blok berikutnya
(`BE-PBJ-VD-002`, `003`, `004`) adalah node yang sama (sambungan), bukan task baru.

```text
BE-PBJ-VD-001 ✅ ─┬─> BE-PBJ-VD-002 ✅
                  │
                  └─> BE-PBJ-VD-003 ✅
```

```text
BE-PBJ-VD-002 ✅ ─┐
                  │
BE-PBJ-VD-003 ✅ ─┴─> BE-PBJ-VD-004 ✅
```

```text
BE-PBJ-VD-002 ✅ ─┐
                  │
BE-PBJ-VD-003 ✅ ─┴─> BE-PBJ-VD-008 ✅
```

```text
BE-PBJ-VD-003 ✅ ─┬─> BE-PBJ-VD-006 ✅ ─┐
                  │                     │
                  │  BE-PBJ-VD-004 ✅ ──┴─> BE-PBJ-VD-005 ✅
                  │
                  └─> BE-PBJ-VD-007 ✅
```

Jumlah pasangan prasyarat → task: **10**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-PBJ-VD-001` |
| 2 | `BE-PBJ-VD-001` | `BE-PBJ-VD-002`, `BE-PBJ-VD-003` |
| 3 | `BE-PBJ-VD-002` dan/atau `BE-PBJ-VD-003` | `BE-PBJ-VD-004`, `BE-PBJ-VD-006`, `BE-PBJ-VD-007`, `BE-PBJ-VD-008` |
| 4 | `BE-PBJ-VD-004`, `BE-PBJ-VD-006` | `BE-PBJ-VD-005` |

**Pemetaan ke gelombang MVP** (`04-prd-to-mvp.md` bagian 20):
- `MVP-V0` = 001–003.
- `MVP-V1` = 004–005.
- `MVP-V2` = 006–007.
- `MVP-V3` = 008.

`BE-PBJ-VD-007` mengunci penulisan blokir di Master Data Supplier (`PBJ-DEC-060`); rilisnya **bersamaan** dengan
`FE-PBJ-VD-004`, supaya tidak ada jeda tanpa jalur blokir sama sekali.

## Tabel task

| Task ID | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-PBJ-VD-001` | Fondasi vendor: 9 tabel, konstanta, seeder, DI | `PBJ-DEC-010`, `052`, `061`, `063`; `PBJ-ASM-001` | Kamus data 1.0; arsitektur §5–§10 | `IdentityModel`, `NumberSeriesAllocator`, seeder definisi `PBJ-*` | 9 model, konfigurasi EF, migration `AddProcurementVendorManagement`, konstanta, seeder | — | `AT-VD-60`, `61`, `62`, `64` | QBE, build, skema DB, seeder 2× | Migration besar satu kali; pemilik: builder backend | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-001.md)) |
| `BE-PBJ-VD-002` | Master jenis dokumen, dokumen wajib, kriteria | `PBJ-ASM-001`, `PBJ-DEC-050`, `061` | API §Master Data (3 grup); VAL-071, 113 | Pola master `ProcurementItem*` | 3 controller × 9 endpoint, 3 service | `BE-PBJ-VD-001` | Sembilan endpoint × 3; total bobot di `summary`; aktivasi kewajiban mengisi `ActivatedAt` | API manual | — | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-002.md)) |
| `BE-PBJ-VD-003` | Daftar dan detail vendor, kategori, rekening tersamarkan, riwayat | `PBJ-DEC-010`, `015`, `051`, `052`, `053`, `060`; `PBJ-ASM-004` | API §Vendor (kecuali restriction, eligibility); VAL-100 | `MstSupplier`, `PrcDocumentHistory` | `VendorsController`, `ProcurementVendorProfileService` | `BE-PBJ-VD-001` | `AT-VD-01`..`04` | API, DB | Bocor nomor rekening — dijaga di server | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-003.md)) |
| `BE-PBJ-VD-004` | Dokumen legalitas dan verifikasi | `PBJ-DEC-045`, `046`; `PBJ-ASM-001`, `002` | API §Vendor Document; STATE §2; VAL-101..104 | `PrcAttachment`, `ProcurementAttachmentService` | `VendorDocumentsController`, `ProcurementVendorDocumentService`, penyedia akses `PBJ_VENDOR_DOCUMENT` | `BE-PBJ-VD-002`, `BE-PBJ-VD-003` | `AT-VD-10`..`13`, `16` | API, DB | — | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-004.md)) |
| `BE-PBJ-VD-005` | Kelayakan vendor per kategori | `PBJ-DEC-045`, `047`, `052`, `054`, `061` | API `GET /vendors/eligibility`; INT §2 | — | `ProcurementVendorEligibilityService`; hitungan dokumen di `summary` | `BE-PBJ-VD-004`, `BE-PBJ-VD-006` | `AT-VD-14`, `15`, `17`, `27` | API | Kontrak bagi `pemilihan-penyedia`; perubahan bentuk menaikkan `PBJ-VD-INT` | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-005.md)) |
| `BE-PBJ-VD-006` | Status Dibatasi | `PBJ-DEC-047` | API restriction/lift; STATE §1; VAL-105 | — | Aksi di `VendorsController` + service profil | `BE-PBJ-VD-003` | `AT-VD-20`, `21` | API, DB | — | Kriteria + laporan | ✅ 17 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-006.md)) |
| `BE-PBJ-VD-007` | Usulan blokir/buka blokir dan kunci Master Data | `PBJ-DEC-021`, `054`, `060`; `PBJ-ASM-003` | API §Vendor Status Request, §Supplier; STATE §3; VAL-106, 107, 091; INT §1 | `ProcurementApprovalGateway`, `ProcurementWorkflowLifecycleHandler`, `ProcurementApprovalTimelineService` | Controller + service usulan, penerap `PBJ_VENDOR_STATUS`, `SupplierProcurementStatusService`, perubahan `SupplierController` | `BE-PBJ-VD-003` | `AT-VD-22`..`26`, `28`, `29` | API, DB; uji approval dengan matriks **uji** dev | Menyentuh modul Administrator (`TOUCHED LEGACY`); pemilik Master Data = pengguna sesi (`PBJ-DEC-026`) | Kriteria + laporan | ✅ 18 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-007.md)) |
| `BE-PBJ-VD-008` | Penilaian berkala | `PBJ-DEC-044`, `048`, `049`, `050`, `059` | API §Vendor Evaluation; STATE §4; VAL-108..112 | `PrcDocumentHistory` | `VendorEvaluationsController`, `ProcurementVendorEvaluationService` | `BE-PBJ-VD-002`, `BE-PBJ-VD-003` | `AT-VD-30`..`36` | API, DB | — | Kriteria + laporan | ✅ 18 November 2026 ([laporan](../task/report/backend/BE-PBJ-VD-008.md)) |

## Kartu task

### ✅ `BE-PBJ-VD-001` — Fondasi vendor

| Aspek | Isi |
| --- | --- |
| Cakupan | Model `MstProcurementVendorDocumentType`, `MstProcurementCategoryDocumentRequirement`, `MstProcurementEvaluationCriterion` (folder `MasterData`), `PrcVendorProfile`, `PrcVendorCategory`, `PrcVendorDocument`, `PrcVendorStatusRequest`, `PrcVendorEvaluation`, `PrcVendorEvaluationScore` (folder `Vendor`); konfigurasi EF termasuk index unik parsial (usulan terbuka per vendor, penilaian per vendor × kategori × periode) dan CHECK (bobot, nilai 1–5, alasan Dibatasi); migration `AddProcurementVendorManagement`; konstanta jenis dokumen `PBJ_VENDOR*`, deret `PBJ_VENDOR_STATUS`/`UBV`, resource izin, kode definisi `PBJ-VND-BLOKIR`/`BUKA`; seeder jenis dokumen, kriteria `UMUM`, dua definisi (tidak aktif, tanpa matriks); kewajiban dokumen **tidak** di-seed (`PBJ-DEC-061`) |
| Di luar cakupan | Endpoint apa pun |
| Acceptance criteria | `AT-VD-60` registry/QBE; `AT-VD-61` build; `AT-VD-62` skema sesuai kamus data; `AT-VD-64` seeder idempoten |
| DoD | Kriteria + laporan `task/report/backend/BE-PBJ-VD-001.md` |
| **Status** | ✅ **SELESAI 17 November 2026.** Build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 25 berkas; migration `AddProcurementVendorManagement` diterapkan ke `QuilvianNewDevSukma`, skema 9 tabel cocok kamus data, uji batasan 14/14 `PASS` (rollback); seeder 2× tanpa baris berlipat (5 jenis dokumen, 3 kriteria `UMUM`, 2 definisi `PBJ-VND-*` tidak aktif, kewajiban dokumen 0). Delta: deret `UBV` dibuat allocator pada pengajuan pertama, bukan di-seed; DI tidak berubah karena belum ada service. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-001.md) |

### ✅ `BE-PBJ-VD-002` — Master konfigurasi vendor

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementVendorDocumentTypesController`, `ProcurementCategoryDocumentRequirementsController`, `ProcurementEvaluationCriteriaController` masing-masing sembilan endpoint baku; saringan tanggal/periode `PBJ-DEC-055`; hapus master terpakai 400 + `PBJ-VAL-071` |
| Acceptance criteria | Sembilan endpoint per master berjalan; `summary` kriteria memuat total bobot aktif per kategori; `PATCH /status` kewajiban mengisi `ActivatedAt` pada aktivasi pertama; bobot di luar 0–100 → `PBJ-VAL-113` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 17 November 2026.** 27 endpoint (3 master × 9); build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 9 berkas; uji runtime 49/49 `PASS` dengan aktor non-SuperAdmin (total bobot per kategori, `ActivatedAt` aktivasi pertama, `PBJ-VAL-113`, `PBJ-VAL-071`, `PBJ-VAL-094`, 403/401). Tanpa migration. Cabang hapus master yang dipakai dokumen/penilaian baru bisa dibuktikan runtime di `BE-PBJ-VD-004`/`008`. Delta D-1..D-8 di laporan. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-002.md) |

### ✅ `BE-PBJ-VD-003` — Profil, kategori, rekening, riwayat

| Aspek | Isi |
| --- | --- |
| Cakupan | `GET /vendors/filters/metadata`, `/summary`, `/`, `/{supplierId}`, `/{supplierId}/history`; `PUT /{supplierId}/categories` (profil lahir saat pertama, riwayat status awal); status vendor dihitung; rekening `****` + 4 digit |
| Acceptance criteria | `AT-VD-01`..`04` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 17 November 2026; pembaruan 18 dan 19 November 2026** — 19 November: saringan gabungan `vendorStatus=NON_ACTIVE` untuk `FE-PBJ-VD-004` `-06` (delta D-10, usulan API 1.1; build 0 error, QBE `PASS` 2 berkas, uji runtime 8/8 `PASS`). 18 November: daftar vendor membawa rekening tersamarkan dan saringan `bankName` + `bankOptions` untuk `FE-PBJ-VD-002` `-05` (delta D-9, usulan API 1.1; build 0 error, QBE `PASS` 2 berkas, uji runtime 10/10 `PASS`; [laporan](../task/report/backend/BE-PBJ-VD-003.md)). 6 endpoint vendor; build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 7 berkas; uji runtime 25/25 `PASS` dengan aktor non-SuperAdmin (`AT-VD-01`..`04`, 409 `PBJ-VAL-090`, `PBJ-VAL-094`). Tanpa migration. Cabang Dibatasi, keadaan dokumen, dan penilaian terakhir baru dapat dibuktikan runtime di `VD-006`/`VD-004`/`VD-008`. Delta D-1..D-8 di laporan. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-003.md) |

### ✅ `BE-PBJ-VD-004` — Dokumen legalitas

| Aspek | Isi |
| --- | --- |
| Cakupan | `VendorDocumentsController` (metadata, summary, daftar, detail, catat, verify, reject, cancel, history); penyedia akses lampiran `PBJ_VENDOR_DOCUMENT`; keadaan berlaku dihitung dengan `Procurement:Vendor:ExpiryWarningDays` |
| Acceptance criteria | `AT-VD-10`..`13`, `16` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 17 November 2026.** 9 endpoint dokumen + penyedia akses lampiran `PBJ_VENDOR_DOCUMENT`; build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 8 berkas; uji runtime 33/33 `PASS` dengan aktor non-SuperAdmin (putaran 2; putaran 1 31/33 karena dua harapan skrip, dicatat di laporan): `AT-VD-10`..`13`, `16`, `PBJ-VAL-090`/`093`/`104`, idempotensi verifikasi. Tanpa migration. Delta D-1..D-8 di laporan. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-004.md) |

### ✅ `BE-PBJ-VD-005` — Kelayakan vendor

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementVendorEligibilityService.CheckAsync`/`GetEligibleAsync`; `GET /vendors/eligibility`; hitungan kedaluwarsa pada `GET /vendors/summary` |
| Acceptance criteria | `AT-VD-14`, `15`, `17`, `27` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 17 November 2026.** `GET /vendors/eligibility` + `ProcurementVendorEligibilityService.CheckAsync`/`GetEligibleAsync`; build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 4 berkas; uji runtime 14/14 `PASS` dengan aktor non-SuperAdmin (`AT-VD-14`, `15`, `17`, `27`, Dibatasi tetap layak, kategori nonaktif). Hitungan kedaluwarsa ringkasan dari `VD-003`, dibuktikan ulang. Tanpa migration. Delta D-1: respons ditambah `supplierCode`, `supplierName`, `warnings[]` — `PBJ-VD-INT` perlu dinaikkan pemilik desain. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-005.md) |

### ✅ `BE-PBJ-VD-006` — Status Dibatasi

| Aspek | Isi |
| --- | --- |
| Cakupan | `POST /vendors/{supplierId}/restriction`, `/restriction/lift`; riwayat vendor; tolak bila vendor Diblokir |
| Acceptance criteria | `AT-VD-20`, `21` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 17 November 2026.** Batasi dan cabut Dibatasi (`ProcurementVendor : Restrict`); build Release 0 error (0 warning dari berkas yang berubah), QBE Strict `PASS` 4 berkas; uji runtime 19/19 `PASS` dengan aktor non-SuperAdmin (`AT-VD-20`, `21`, `PBJ-VAL-090`/`093`/`104`/`105`, idempotensi). Tanpa migration. Bagian kelayakan `AT-VD-20` dibuktikan lewat detail vendor; endpoint kelayakan milik `VD-005`. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-006.md) |

### ✅ `BE-PBJ-VD-007` — Usulan blokir dan kunci Master Data

| Aspek | Isi |
| --- | --- |
| Cakupan | `VendorStatusRequestsController` (metadata, summary, daftar, detail, buat, ubah, submit, cancel, history, approval); `ProcurementVendorStatusService` + `IProcurementWorkflowStatusApplier` `PBJ_VENDOR_STATUS`; `SupplierProcurementStatusService` (Administrator); `SupplierController` create/update mengabaikan field blokir |
| Acceptance criteria | `AT-VD-22`..`26`, `28`, `29` |
| Catatan | Uji persetujuan memakai matriks **uji** di dev yang dibersihkan sesudahnya (pola `BE-PBJ-008`); definisi tetap tidak aktif di produksi sampai SOP (`PBJ-OQ-019`) |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 18 November 2026.** 11 endpoint usulan + penerap status `Wfl` `PBJ_VENDOR_STATUS` + `SupplierProcurementStatusService`; `SupplierController` berhenti menulis status blokir; build Release 0 error (0 warning baru), QBE Strict `PASS` 10 berkas; uji runtime 28/28 `PASS` dengan aktor non-SuperAdmin dan matriks approval **uji** yang dicabut sesudahnya (`AT-VD-22`..`26`, `28`, `29`). Tanpa migration. Rilis bersamaan dengan `FE-PBJ-VD-004`. Delta D-1..D-7 di laporan. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-007.md) |

### ✅ `BE-PBJ-VD-008` — Penilaian berkala

| Aspek | Isi |
| --- | --- |
| Cakupan | `VendorEvaluationsController` (metadata, summary, daftar, detail, buat, ubah, finalize, revisions, cancel, history); salin kriteria; nilai akhir berbobot; revisi menggantikan |
| Acceptance criteria | `AT-VD-30`..`36` |
| DoD | Kriteria + laporan |
| **Status** | ✅ **SELESAI 18 November 2026.** 10 endpoint penilaian + penyedia akses lampiran `PBJ_VENDOR_EVALUATION`; build Release 0 error (0 warning dari berkas baru/berubah), QBE Strict `PASS` 5 berkas; uji runtime 32/32 `PASS` dengan aktor non-SuperAdmin (putaran 2; putaran 1 berhenti di fixture karena kode kategori uji bentrok): `AT-VD-30`..`36`, `PBJ-VAL-090`/`093`/`104`, idempotensi finalisasi, lampiran. Tanpa migration. Delta D-1..D-8 di laporan. Laporan: [laporan](../task/report/backend/BE-PBJ-VD-008.md) |

## Tidak masuk roadmap ini

- `S-V7` evaluasi per kontrak, rekap keterlambatan dan NCR — menunggu `kontrak-pelaksanaan` (`PBJ-DEC-044`).
- Notifikasi aktif kedaluwarsa — menunggu `PBJ-OQ-015`.
- Pemakaian kelayakan di RFQ/penetapan — milik `pemilihan-penyedia`.
