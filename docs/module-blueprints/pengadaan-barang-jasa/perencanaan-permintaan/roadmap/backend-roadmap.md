# Perencanaan & Permintaan — Roadmap Backend

| Field | Nilai |
| --- | --- |
| Blueprint | `PBJ-BP-001` revisi 2 · sub-modul `perencanaan-permintaan` (`approved`, `PBJ-DEC-034`) |
| Revisi roadmap | `1` · status `draft` (rencana; tiap task tetap butuh perintah eksekusi tersendiri) |
| Tanggal | 12 November 2026 |
| Kontrak | set `1.0`: `PBJ-PP-API`, `-STATE`, `-VAL`, `-INT`, `-PERM`, `-TEST`, `-MVP` (semua `approved`) |
| Keputusan | `PBJ-DEC-001`..`034` (`decision_revision 5`) |
| SHA | BE `100c011d` (branch `sukmagp`), FE `763d665b` (branch `sukmagpV2`) |
| Hash masukan | Lihat `../blueprint-manifest.md` (`input_hashes`, `artifact_hashes`) |
| Repository target | `NewQuilvianSystemBackend` |

**Untuk setiap task:** QBE preflight, kesesuaian engineering, penetapan branch, dan wewenang migration
diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend dan dokumen engineering canonical. Roadmap ini
tidak memberi wewenang itu.

**Migration:** mengikuti preferensi pemilik 2026-10-05 di `AGENTS.md`. Task yang mengubah skema menyertakan
berkas migration, Designer, dan snapshot, tetapi **tidak menerapkannya** ke basis data. Pemilik menjalankan
`database update` sendiri, dengan urutan database dulu, lalu aplikasi.

**Kebijakan uji backend:** tidak ada task penulisan unit/integration test atau proyek test. Bukti setiap task:
QBE preflight/conformance, review diff dan cakupan, `dotnet restore` + `dotnet build` project aplikasi
(dijalankan di latar belakang karena lama), verifikasi kontrak API manual, serta bukti basis data sesuai
`testing/acceptance-test-matrix.md`.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai; acceptance criteria dan DoD terbukti di laporan task |
| 🟡 | Sebagian; belum terbukti penuh |
| ⛔ | Terblokir; nama blocker disebut |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

Grafik dipotong menjadi dua blok karena lebar baris. `BE-PBJ-005` dan `BE-PBJ-006` pada blok 2 adalah node
yang sama dengan blok 1 (sambungan), bukan task baru.

```text
{PBJ-OQ-002 ✅} ─> BE-PBJ-002 🟡 ─┬─> BE-PBJ-004 🟡
                                  │
                                  └─> BE-PBJ-003 🟡 ─┬─> BE-PBJ-006 🟡
                                                     │
BE-PBJ-001 🟡 ───────────────────────────────────────┴─> BE-PBJ-005 🟡
```

```text
BE-PBJ-005 🟡 ─┬─> BE-PBJ-009 ─┬─> BE-PBJ-011
               │               │
               │               └─────┬─> BE-PBJ-007 ─┬─> BE-PBJ-008
               │                     │               │
               │   BE-PBJ-006 🟡 ────┘               │
               │                                     │
               └─> BE-PBJ-010 ───────────────────────┘
```

`{PBJ-OQ-002}` = gerbang registry prefix `Prc` dan Area `Corporate / ProcurementManagement`
(`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`), di luar wewenang blueprint.

Jumlah pasangan prasyarat → task: **13**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-PBJ-001` 🟡 — dikerjakan 12 November 2026; menunggu migration diterapkan untuk `AT-PP-35` |
| 1 | `{PBJ-OQ-002}` ✅ diputuskan 2026-11-12 (`PBJ-DEC-035`) | `BE-PBJ-002` 🟡 — dikerjakan 12 November 2026 |
| 2 | `BE-PBJ-002` | `BE-PBJ-003` 🟡, `BE-PBJ-004` 🟡 — dikerjakan 12 November 2026 |
| 3 | `BE-PBJ-003` (+ `BE-PBJ-001` untuk 005) | `BE-PBJ-005` 🟡, `BE-PBJ-006` 🟡 — dikerjakan 12 November 2026 |
| 4 | `BE-PBJ-005` | `BE-PBJ-009`, `BE-PBJ-010` — paralel |
| 5 | `BE-PBJ-009` (+ `BE-PBJ-006` untuk 007) | `BE-PBJ-007`, `BE-PBJ-011` — paralel |
| 6 | `BE-PBJ-007`, `BE-PBJ-010` | `BE-PBJ-008` |

Gerbang `PBJ-OQ-002` ditutup 12 November 2026: registry `Prc` disetujui (`PBJ-DEC-035`), sehingga `BE-PBJ-002` dapat dimulai.

**Pemetaan ke gelombang MVP** (`04-prd-to-mvp.md` bagian 20):
- `MVP-0` = 001–005.
- `MVP-1` = 006–008.
- `MVP-2` = 009.
- `MVP-3` = 010.
- `MVP-4` = 011.

`BE-PBJ-009` (rencana) dan `BE-PBJ-010` (pagu) maju ke depan `BE-PBJ-007`/`008`, karena pengajuan PR membagi
dalam/luar rencana dan verifikasi menguji pagu. Dengan urutan ini, catatan "semua PR di luar rencana sebelum
MVP-2" pada PRD tidak terjadi.

## Tabel task

| Task ID | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-PBJ-001` | Mesin `Wfl` menerima penangan dokumen non-HR dan langkah bersyarat | `PBJ-DEC-019`, `026`, `030`; FR-PBJ-001..003 | `PBJ-PP-INT` 1.0 §1; arsitektur §8 | `WorkflowService`, `WorkflowReferenceLifecycleService` | `Wfl` + 1 kolom + migration | — | 🟡 2026-11-12 — `AT-PP-35` belum jalan ([laporan](../task/report/backend/BE-PBJ-001.md)) |
| `BE-PBJ-002` | Kerangka Area Pengadaan, riwayat dokumen, cakupan unit | `PBJ-DEC-020`, `PBJ-OQ-002` | `PBJ-PP-PERM` §1, §4–5; kamus data §5 | `AspNetUserOrganization` | Area baru, 1 tabel | `{PBJ-OQ-002}` | 🟡 2026-11-12 — skema DB dan runtime belum ([laporan](../task/report/backend/BE-PBJ-002.md)) |
| `BE-PBJ-003` | Katalog, kategori, jenis, sumber dana + data awal | `PBJ-DEC-022`, `024`, `033` (`G-07`) | API §Master Data; VAL-070..072 | `NumberSeriesAllocator`, `MstMeasurement`, `MstDrug` | 4 tabel Mst, 2 controller | `BE-PBJ-002` | 🟡 2026-11-12 ([laporan](../task/report/backend/BE-PBJ-003.md)) |
| `BE-PBJ-004` | Lampiran berversi dengan cek isi berkas | `PBJ-DEC-023`; FR-PBJ-020..021 | API §Attachment; VAL-080..083 | pola `WorkflowFileStorageService` (acuan, tidak dipanggil) | 1 tabel, 1 controller | `BE-PBJ-002` | 🟡 2026-11-12 ([laporan](../task/report/backend/BE-PBJ-004.md)) |
| `BE-PBJ-005` | Gerbang approval Pengadaan + definisi `PBJ-*` di-seed | `PBJ-DEC-007`, `016`, `019`, `027` | INT §1; arsitektur §8, §10 | `WorkflowService.CreateAsync/SubmitAsync` | 2 service, seeder | `BE-PBJ-001`, `BE-PBJ-003` | 🟡 2026-11-12 ([laporan](../task/report/backend/BE-PBJ-005.md)) |
| `BE-PBJ-006` | PR draft: buat, ubah, daftar, ringkasan, detail, riwayat | `PBJ-DEC-020`, `022`; FR-PBJ-030, 033 | API §Purchase Request (GET, POST, PUT, history); VAL-002..005, 010, 090 | — | 2 tabel, 1 controller | `BE-PBJ-003` | 🟡 2026-11-12 ([laporan](../task/report/backend/BE-PBJ-006.md)) |
| `BE-PBJ-007` | PR diajukan bernomor, dibagi dalam/luar rencana; batal dan tarik | `PBJ-DEC-014`, `016`, `025`/`031`, `029`, `033` (`G-04`); FR-PBJ-030..035 | API submit/cancel/plan-line-options; STATE §1; VAL-001..012, 093 | `NumberSeriesAllocator` | service kuota + aksi | `BE-PBJ-006`, `BE-PBJ-009` | — |
| `BE-PBJ-008` | Verifikasi, cek stok, kirim ke approval, sinkron hasil | `PBJ-DEC-008`, `027`, `030`; FR-PBJ-040..044 | API §Verification; STATE §1, §5; VAL-020, 021, 030, 091, 092 | `Wfl` (via 005) | 1 service, 1 controller, penangan | `BE-PBJ-007`, `BE-PBJ-010` | — |
| `BE-PBJ-009` | Rencana kebutuhan berversi, revisi, banding versi | `PBJ-DEC-014`, `033` (`G-05`); FR-PBJ-050..053 | API §Need Plan; STATE §2; VAL-040..044 | — | 2 tabel, 1 controller | `BE-PBJ-005` | — |
| `BE-PBJ-010` | Pagu berversi, sisa pagu, layanan komitmen | `PBJ-DEC-009`, `030`, `033`; FR-PBJ-060..062 | API §Budget; STATE §4; VAL-060..062; INT §2 | — | 2 tabel, 1 controller | `BE-PBJ-005` | — |
| `BE-PBJ-011` | Konsolidasi dan rencana paket | `PBJ-DEC-028`, `033`; FR-PBJ-070..072 | API §Consolidation, §Package Plan; STATE §3; VAL-050, 051 | — | 2 tabel, 2 controller | `BE-PBJ-009` | — |

## Kartu task

### 🟡 `BE-PBJ-001` — Perluasan mesin approval `Wfl`

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS` (0 error, 608 warning); skrip EF = satu `ADD COLUMN` nullable. Kriteria 1, 3, 4, 5 terbukti (statis/EF); kriteria 2 menunggu penangan pertama (`BE-PBJ-005`); `AT-PP-35` regresi HR di runtime **belum dijalankan** karena migration belum diterapkan ke basis data dev. Laporan: [`BE-PBJ-001.md`](../task/report/backend/BE-PBJ-001.md) |
| Outcome | Modul non-HR dapat mendaftarkan penangan dokumen, dan langkah approval dapat dibuat bersyarat |
| Cakupan | Baru: `Areas/Corporate/HumanResource/WorkflowManagement/Services/IWorkflowReferenceLifecycleHandler.cs` (`bool CanHandle(string referenceType)`, `Task<WorkflowReferenceLifecycleSynchronizationResult> SynchronizeAsync(TrxWorkflowInstance, Guid actorUserId, bool allowAutoApply, CancellationToken)`). Diperbarui: `WorkflowReferenceLifecycleService.SynchronizeAsync` memanggil `IEnumerable<IWorkflowReferenceLifecycleHandler>` sebelum fallback `IsHandled = false`; `WorkflowService` melewati `MstWorkflowStep` yang `ActivationConditionCode`-nya tidak ada pada `RequestContext.conditionCodes`; `WorkflowRequestContext.Parse` membaca `conditionCodes`; `MstWorkflowStep` + konfigurasi EF + migration `AddWorkflowStepActivationCondition` (Designer + snapshot) |
| Di luar cakupan | Perbaikan jalur V2 (`WorkflowInstanceCreationService` fallback, `WorkflowRoutingMatrixService` hardcode): temuan terpisah untuk pemilik `Wfl` |
| Acceptance criteria | (1) Instance dengan `ReferenceType` HR tetap ditangani penangan HR yang sama. (2) Penangan terdaftar dipanggil untuk tipe yang ia klaim. (3) Langkah ber-kode dibuat hanya bila kode ada di konteks. (4) Langkah tanpa kode (`null`) selalu dibuat. (5) Migration hanya menambah satu kolom nullable |
| Verifikasi | QBE preflight; build; review diff (hanya berkas di atas); API manual: ajukan dan setujui cuti/lembur sebelum dan sesudah (`AT-PP-35`); DB: kolom ada, data lama `NULL` setelah pemilik menerapkan migration |
| Risiko / pemilik | Menyentuh mesin bersama; regresi HR. Pemilik: pengguna sesi (`PBJ-DEC-026`) |
| DoD | Kriteria 1–5 terbukti dan laporan `task/report/backend/BE-PBJ-001.md` ditulis |

### 🟡 `BE-PBJ-002` — Kerangka Area Pengadaan

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS` (0 error, 608 warning = baseline); QBE `PASS` (17 berkas, 0 temuan); skrip EF = satu `CREATE TABLE` + 3 index; snapshot konsisten. Kriteria 1–4 terbukti (kriteria 4 menurut delta `PBJ-DEC-036`). Belum: skema di basis data dan uji runtime, karena migration tidak diterapkan (atas instruksi pemilik) dan aplikasi belum bisa start di basis data dev. Laporan: [`BE-PBJ-002.md`](../task/report/backend/BE-PBJ-002.md) |
| Outcome | Fondasi bersama siap dipakai task berikutnya |
| Cakupan | `ProcurementManagementServiceCollectionExtensions` + panggilan di `Program.cs`; `Common/Constants/*` (jenis dokumen, status, deret nomor dengan `NEVER` dan 6 digit sesuai `PBJ-DEC-031`, kode workflow); `PrcDocumentHistory` + konfigurasi + `DbSet` + migration `AddProcurementFoundation` (bagian riwayat); `ProcurementDocumentHistoryWriter`; `ProcurementDataScopeService` (cakupan dari `AspNetUserOrganization` aktif atau semua bila `<resource dokumen> : ReadAllUnits`, `PBJ-DEC-036`) |
| Acceptance criteria | (1) QBE conformance lulus untuk Area baru. (2) Riwayat hanya bisa ditambah, tidak ada metode ubah/hapus. (3) Pengguna tanpa penempatan aktif mendapat cakupan kosong. (4) Izin `ProcurementCrossUnit : Read` terdaftar di registry izin — **delta `PBJ-DEC-036`:** diganti penanda `<resource dokumen> : ReadAllUnits` yang didaftarkan task controller dokumennya; di task ini cukup `ProcurementDataScopeService` menanyakan aksi itu dan menolak bila belum terdaftar |
| Verifikasi | QBE (`AT-PP-60`); build; DB skema `PrcDocumentHistory` |
| Risiko / pemilik | Penamaan bisa berubah bila registry menetapkan prefix lain; seluruh task berikut ikut menyesuaikan |
| DoD | Kriteria 1–4 + laporan |

### 🟡 `BE-PBJ-003` — Katalog dan master

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS` (0 error, 608 warning = baseline); QBE `PASS` (41 berkas); skrip EF = 4 tabel master + 4 FK `RESTRICT`; snapshot konsisten. Delta kontrak `PBJ-DEC-037` (API 1.1, PERM 1.2): satu controller per master, 9 endpoint baku, `ProcurementType` hanya baca. Belum: `AT-PP-50`, `51`, `64` (runtime) dan `PBJ-VAL-071` penuh (tabel pemakai menyusul `BE-PBJ-006`/`009`). Laporan: [`BE-PBJ-003.md`](../task/report/backend/BE-PBJ-003.md) |
| Cakupan | Model `MstProcurementType`, `MstProcurementItemCategory`, `MstProcurementFundSource`, `MstProcurementItem` + konfigurasi + migration (bagian master `AddProcurementFoundation`); `ProcurementCatalogService`; `ProcurementItemsController`, `ProcurementMasterDataController` (endpoint §Master Data); seeder idempoten (4 jenis, kategori `UMUM`, sumber dana `OPERASIONAL`/`INVESTASI`); deret `PBJ_ITEM`/`KTL` |
| Acceptance criteria | (1) Kode `KTL-000001` dst. dibuat server. (2) `PBJ-VAL-070`, `071`, `072` berlaku. (3) Seeder dua kali tidak menggandakan data. (4) Barang terpakai hanya bisa dinonaktifkan |
| Verifikasi | API manual `AT-PP-50`, `51`; DB `AT-PP-64`; build; QBE |
| DoD | Kriteria + laporan |

### 🟡 `BE-PBJ-004` — Lampiran

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS`; QBE `PASS`; skrip EF = `PrcAttachment` + 4 index; snapshot konsisten. Pemeriksa tanda tangan berkas, versi, penyimpanan di luar `wwwroot`, dan akses dokumen induk *fail-closed* sudah ada. Belum: `AT-PP-53`..`56` (runtime), karena migration belum diterapkan dan penyedia akses dokumen pertama baru ada di `BE-PBJ-006`. Risiko: `App_Data/` belum di `.gitignore`. Laporan: [`BE-PBJ-004.md`](../task/report/backend/BE-PBJ-004.md) |
| Cakupan | `PrcAttachment` + konfigurasi + migration; `ProcurementAttachmentService` (daftar ekstensi dan ukuran dari `Procurement:Attachment:*`, cek tanda tangan berkas, nama acak, versi via `AttachmentKey`, cek hak dokumen induk lewat registrasi penyedia akses per `DocumentType`); `ProcurementAttachmentsController`; entri `appsettings` |
| Acceptance criteria | (1) `AT-PP-53`: HTML, PDF berisi HTML, dan EXE berganti nama ditolak tanpa berkas tersimpan. (2) `AT-PP-54`: batas ukuran. (3) `AT-PP-55`: versi. (4) `AT-PP-56`: unduh oleh unit lain → 404. (5) Unduhan memakai `Content-Disposition: attachment` |
| Catatan | Penyedia akses per `DocumentType` didaftarkan oleh task pemilik dokumen (`BE-PBJ-006`, `009`, `010`, `011`). Sebelum terdaftar, tipe dokumen itu ditolak (*fail-closed*) |
| DoD | Kriteria + laporan |

### 🟡 `BE-PBJ-005` — Gerbang approval Pengadaan

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS` (0 error, 608 warning = baseline); QBE `PASS` (15 berkas bersama `BE-PBJ-006`). Kriteria 1–4 terbukti statis. Delta `PBJ-DEC-038`: Wfl diperluas `CreateAndSubmitOnBehalfAsync` (pengaju = pemohon asli, idempoten); definisi `PBJ-*` di-seed **tidak aktif tanpa baris matriks** (fail-closed, nilai menunggu `PBJ-OQ-019`) alih-alih matriks fallback. Belum: bukti DB definisi/langkah dan instance nyata (aplikasi tidak dapat start di dev DB; instance pertama di `BE-PBJ-009`). Laporan: [`BE-PBJ-005.md`](../task/report/backend/BE-PBJ-005.md) |
| Cakupan | `ProcurementApprovalGateway` (pilih kode definisi, susun `RequestContext` dengan `amount` dan `conditionCodes`, `CreateAsync` + `SubmitAsync` dengan `IdempotencyKey = <ReferenceType>:<DocumentId>:<SubmissionCount>`); kerangka `ProcurementWorkflowLifecycleHandler` (klaim `PBJ_*`, delegasi ke penerap per jenis dokumen yang didaftarkan task pemiliknya; periksa ulang penyetuju ≠ pengusul); seeder 5 definisi `PBJ-PR`, `PBJ-PR-MENDESAK`, `PBJ-RKB`, `PBJ-RPK`, `PBJ-PAGU` (`WorkflowCategory = "Procurement"`, `AllowSelfApproval = false`, langkah `PBJ_OVER_BUDGET` dan `PBJ_OUT_OF_PLAN` ber-kode kondisi) dengan baris matriks fallback **[NILAI AWAL]** |
| Acceptance criteria | (1) Gateway membuat instance satu kali untuk kunci idempoten yang sama. (2) Definisi tidak aktif → galat yang dipetakan ke `PBJ-VAL-091`. (3) Penangan terdaftar di DI dan diklaim untuk `PBJ_*`. (4) Seeder idempoten |
| Verifikasi | Build; DB definisi/langkah/matriks; uji manual lewat dokumen uji di `BE-PBJ-009` (instance pertama nyata) |
| DoD | Kriteria + laporan. Nilai matriks dari SOP (`PBJ-OQ-019`) **bukan** syarat DoD |

### 🟡 `BE-PBJ-006` — PR draft

| Aspek | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN 12 November 2026.** Build `PASS`; QBE `PASS`; skrip EF = 2 tabel, 4 FK, 1 `CHECK`, 11 index; snapshot konsisten (pemeriksa sementara: 0 selisih Pengadaan). Kriteria 1, 4, 5 terbukti statis; `ReadAllUnits` terdaftar; penyedia akses lampiran `PBJ_PR` aktif; hapus barang/sumber dana yang dipakai PR ditolak. Delta kontrak: `GET /{id}/status-history` (bukan `/history`), tambahan `GET /filters/metadata` dan query `mineOnly`. Belum: `AT-PP-08`, `09` (runtime; migration belum diterapkan). Laporan: [`BE-PBJ-006.md`](../task/report/backend/BE-PBJ-006.md) |
| Cakupan | `PrcPurchaseRequest`, `PrcPurchaseRequestLine` + konfigurasi + migration `AddProcurementPurchaseRequest`; `ProcurementPurchaseRequestService` (buat, ubah dengan `ExpectedRowVersion`, daftar tersaring cakupan, ringkasan, detail, riwayat); `PurchaseRequestsController` (`GET /`, `/summary`, `/{id}`, `POST /`, `PUT /{id}`, `GET /{id}/history`); penyedia akses lampiran `PBJ_PR` |
| Acceptance criteria | (1) Draft tanpa nomor. (2) `AT-PP-08`: unit lain → 404. (3) `AT-PP-09`: data basi → 409. (4) Hanya pembuat yang dapat mengubah. (5) `PBJ-VAL-002`..`005` saat simpan |
| Delta `PBJ-DEC-036` | Daftarkan `[assembly: AccessExplicitPermission]` `ProcurementRequest : ReadAllUnits` bersama controller task ini, dan pakai `ProcurementDataScopeService` dengan resource `ProcurementRequest` |
| DoD | Kriteria + laporan |

### `BE-PBJ-007` — Pengajuan, batal, tarik PR

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementPlanQuotaService` (sisa per `NeedPlanLineKey` dari rencana `APPROVED` berlaku dikurangi PR berstatus pemakan; advisory lock `pg_advisory_xact_lock` per kunci); aksi `submit` (validasi `PBJ-VAL-001`..`012`, alokasi nomor `PBJ_PR` saat pertama, pembagian dalam/luar rencana, `SubmissionCount++`, riwayat); `cancel` (termasuk tarik dari `PENDING_APPROVAL` lewat `Wfl` withdraw); `GET /plan-line-options`; header `Idempotency-Key` |
| Acceptance criteria | `AT-PP-01` s.d. `AT-PP-07`, `AT-PP-10`, `AT-PP-36` |
| Catatan | `AT-PP-36` (tarik) baru dapat dibuktikan penuh setelah `BE-PBJ-008` membuat instance; sampai itu task ini boleh 🟡 untuk kriteria tersebut saja |
| DoD | Kriteria + laporan |

### `BE-PBJ-008` — Verifikasi dan approval PR

| Aspek | Isi |
| --- | --- |
| Cakupan | `ProcurementPurchaseRequestVerificationService` + `PurchaseRequestVerificationsController` (antrean, daftar cek stok, catat cek stok, `complete`, `return`, `reject`, `cancel-approved`); hitung `PurchaseQuantity`/`PurchaseTotal`; uji lewat pagu per kategori lewat `ProcurementBudgetService`; tanda `IsOutOfPlan`/`IsOverBudget` → `conditionCodes`; pemilihan definisi normal/mendesak; penerap status `PBJ_PR` pada penangan (`state-transition-matrix` §5); verifikator ≠ pengusul |
| Acceptance criteria | `AT-PP-20` s.d. `AT-PP-28`, `AT-PP-30` s.d. `AT-PP-34` |
| DoD | Kriteria + laporan |

### `BE-PBJ-009` — Rencana kebutuhan

| Aspek | Isi |
| --- | --- |
| Cakupan | `PrcNeedPlan`, `PrcNeedPlanLine` + migration `AddProcurementNeedPlan`; `ProcurementNeedPlanService`; `NeedPlansController` (semua endpoint §Need Plan); penerap status `PBJ_NEED_PLAN` (perpindahan `IsCurrentVersion` dan `SUPERSEDED` dalam satu transaksi); penyedia akses lampiran; `PBJ-VAL-042` terhadap pemakaian PR |
| Acceptance criteria | `AT-PP-40`, `41`, `42`; instance `Wfl` pertama nyata untuk `PBJ-RKB` disetujui end-to-end (sekaligus bukti `BE-PBJ-005`) |
| Delta `PBJ-DEC-036` | Daftarkan `[assembly: AccessExplicitPermission]` `ProcurementNeedPlan : ReadAllUnits` bersama controller task ini, dan pakai `ProcurementDataScopeService` dengan resource `ProcurementNeedPlan` |
| DoD | Kriteria + laporan |

### `BE-PBJ-010` — Pagu

| Aspek | Isi |
| --- | --- |
| Cakupan | `PrcBudget` (dengan `LineageKey`), `PrcBudgetCommitment` + migration `AddProcurementBudget`; `ProcurementBudgetService` (CRUD, ajukan, revisi, `GET /remaining`); `ProcurementBudgetCommitmentService` (`CommitAsync`/`ReleaseAsync`, belum ada pemanggil); `BudgetsController`; penerap status `PBJ_BUDGET` |
| Acceptance criteria | `AT-PP-45`, `46`; sisa = pagu berlaku − komitmen aktif |
| Catatan | Komitmen untuk uji `AT-PP-45`/`46` diisi lewat data uji di basis data karena pemanggilnya ada di sub-modul `pemilihan-penyedia` |
| Delta `PBJ-DEC-036` | Daftarkan `[assembly: AccessExplicitPermission]` `ProcurementBudget : ReadAllUnits` bersama controller task ini, dan pakai `ProcurementDataScopeService` dengan resource `ProcurementBudget` |
| DoD | Kriteria + laporan |

### `BE-PBJ-011` — Konsolidasi dan rencana paket

| Aspek | Isi |
| --- | --- |
| Cakupan | `PrcPackagePlan`, `PrcPackagePlanLine` + migration `AddProcurementPackagePlan`; `ProcurementPlanConsolidationService`, `ProcurementPackagePlanService`; `PlanConsolidationsController`, `PackagePlansController`; penerap status `PBJ_PACKAGE_PLAN`; menambah pemeriksaan alokasi paket ke `PBJ-VAL-042` milik `BE-PBJ-009` |
| Acceptance criteria | `AT-PP-43`, `44`; revisi rencana di bawah alokasi paket ditolak |
| Delta `PBJ-DEC-036` | Daftarkan `[assembly: AccessExplicitPermission]` `ProcurementPackagePlan : ReadAllUnits` bersama controller task ini, dan pakai `ProcurementDataScopeService` dengan resource `ProcurementPackagePlan` |
| DoD | Kriteria + laporan |

## Tidak masuk roadmap ini

| Pekerjaan | Pemilik / alasan |
| --- | --- |
| `MarkConvertedAsync`, `GetApprovedForPackagingAsync`, `GetApprovedAsync` rencana paket | Dibangun bersama `pemilihan-penyedia` (`PBJ-PP-INT` §2) |
| Integrasi stok, dashboard, laporan | `POST-MVP` (`04-prd-to-mvp.md` §8) |
| Perbaikan jalur V2 `Wfl` | Temuan untuk pemilik `Wfl`; bukan bagian modul ini |
