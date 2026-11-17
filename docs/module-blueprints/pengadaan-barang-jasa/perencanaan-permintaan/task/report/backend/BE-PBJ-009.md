# Laporan Perubahan Backend — `BE-PBJ-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-009` |
| Judul | Rencana kebutuhan |
| Slice | `MVP-2` — rencana kebutuhan tahunan (`EPIC PBJ-02`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-009` |
| Trace | `PBJ-DEC-014`, `033` (`G-05`), `036`; FR-PBJ-050..053; STATE §2, §5; `PBJ-VAL-040`..`044`; `AT-PP-40`, `41`, `42` |
| Contract version | `PBJ-PP-API` 1.1 (dengan delta di bagian 3.3), `PBJ-PP-STATE` 1.0, `PBJ-PP-VAL` 1.0, `PBJ-PP-INT` 1.1, `PBJ-PP-PERM` 1.2 — semua `approved` |
| Dependency | `BE-PBJ-005` (✅ bersama laporan ini) |
| Klasifikasi | `HEAVY` (logika 3, kontrak API 2, database 2, keamanan 1, integrasi `Wfl` 2) |
| Task mode | `BACKEND` (diberikan pemilik 16 November 2026 lewat jawaban "BE lalu FE") |
| Target tulis | `Areas/Corporate/ProcurementManagement/{Planning,Common}/**`, `Wfl` (`WorkflowService.cs`, `WorkflowService.ActionsV2.cs`), konfigurasi EF, `ApplicationDbContext.cs`, extension DI, service master (cek pemakaian), migration `AddProcurementNeedPlan`, model snapshot |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `27fac19c` (belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ **SELESAI.** `AT-PP-40`, `41`, `42` dan instance `PBJ-RKB` pertama disetujui end-to-end terbukti runtime (32 skenario `PASS`); build `PASS`; QBE `PASS`; migration diterapkan ke `QuilvianNewDevSukma` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Planning` |
| Registry | `Corporate / Procurement · ProcurementManagement / Planning · Prc · ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs`, service master Pengadaan, `WorkflowService` (`Wfl`, pemilik = pengguna sesi, `PBJ-DEC-026`), model snapshot EF |
| Arketipe endpoint | Transaksi ber-lifecycle — tanpa `GET /options`, `PATCH /{id}/status`, `DELETE /{id}` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-CODE-001..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (53 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

Unit belum bisa menyusun rencana kebutuhan tahunan. Akibatnya setiap PR otomatis "di luar rencana", sisa rencana
tidak bisa dihitung, dan Tim Pengadaan tidak punya bahan konsolidasi. Task ini menyediakan rencana kebutuhan per unit ×
tahun yang berversi: dibuat, diajukan ke persetujuan, direvisi, dan dibandingkan antarversi.

## 2. Proses bisnis

1. Penyusun unit membuat rencana tahun anggaran (tahun berjalan −1 sampai +2, `PBJ-VAL-043`) untuk **unit
   penempatannya sendiri** (`PBJ-VAL-010`). Satu unit × tahun hanya boleh punya satu rencana hidup (`PBJ-VAL-040`).
2. Baris berisi barang katalog aktif, jumlah > 0, perkiraan harga ≥ 0, bulan kebutuhan, dan sumber dana opsional.
   Barang yang sama tidak boleh dua kali (`PBJ-VAL-044`). Total dihitung server.
3. Selama `DRAFT`/`RETURNED`, rencana dapat diubah dengan versi data terakhir (`PBJ-VAL-090`). Baris lama dikenali
   lewat `LineKey`; barangnya tidak dapat diganti.
4. **Ajukan:** nomor `RKB-000006` dialokasikan saat versi 1 pertama diajukan; instance `PBJ-RKB` dibuat atas nama
   penyusun. Bila definisi approval belum aktif → 409 `PBJ-VAL-091`, rencana tetap `DRAFT`.
5. **Hasil approval** diterapkan penangan `Wfl`: disetujui → `APPROVED` dan menjadi versi berlaku; ditolak →
   `REJECTED`; dikembalikan → `RETURNED` (dapat diajukan ulang; instance lama ditutup lebih dulu).
6. **Revisi** dari versi berlaku yang disetujui (alasan wajib) membuat versi baru `DRAFT` dengan nomor dan `LineKey`
   yang sama. Hanya boleh satu revisi berjalan (`PBJ-VAL-041`). Revisi tidak boleh menurunkan jumlah di bawah yang
   sudah dipakai PR aktif atau dialokasikan rencana paket (`PBJ-VAL-042`). Saat revisi disetujui, versi lama
   menjadi `SUPERSEDED` dan penanda versi berlaku berpindah dalam satu transaksi.

**Contoh:** rencana IGD 2027 v1 berisi 10 monitor; PR aktif sudah memakai 10. Revisi v2 menurunkan monitor ke 5 →
ditolak "sudah dipakai 10 UNIT, tidak bisa diturunkan ke 5". Revisi v2 menaikkan ke 12 → disetujui; sisa untuk PR = 2.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak `api-contract.md`, `state-transition-matrix.md`, `validation-matrix.md`, `integration-contract.md`,
`permission-audit-matrix.md`, `data-dictionary.md`, `acceptance-test-matrix.md`; `WorkflowService.cs`,
`WorkflowService.ActionsV2.cs`, `WorkflowReferenceLifecycleService.cs`, `NumberSeriesAllocator.cs`,
`HumanResourceContextService.cs`; service Pengadaan `BE-PBJ-002`..`006`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Planning/Models/PrcNeedPlan.cs`, `PrcNeedPlanLine.cs` | Entity baru (kamus data bagian 3) |
| `Repositories/Configurations/.../Planning/PrcNeedPlan*Configuration.cs` | Index unik parsial versi berlaku, unik nomor+versi, unik barang/`LineKey` per rencana, CHECK jumlah > 0 |
| `Planning/DTOs/NeedPlanDtos.cs` | DTO kueri, buat, ubah, respons, banding versi |
| `Planning/Services/ProcurementNeedPlanService.cs` | Layanan rencana, penyedia akses lampiran `PBJ_NEED_PLAN`, penerap status `Wfl` (diambil malas dari `IServiceProvider` agar tidak melingkar) |
| `Planning/Services/ProcurementPlanQuotaService.cs` | Pemakaian baris rencana per `LineKey` (dipakai juga `BE-PBJ-007`, `011`) |
| `Planning/Services/ProcurementPlanningExceptions.cs` | Galat 422/409/403 dengan `code`, `lineNo`, `existingId` |
| `Planning/Controllers/NeedPlansController.cs` | Endpoint bagian 4 |
| `Planning/ProcurementPlanningExplicitPermissions.cs` | `ProcurementNeedPlan : ReadAllUnits` |
| `Common/Constants/*` | Kode modul Planning, aksi `Submit`/`Cancel`/`Revise`/`Verify`/`CancelApproved`, label status rencana, aksi riwayat `SUPERSEDE` |
| `Common/Services/ProcurementDocumentHistoryWriter.cs` | `HasIdempotencyKeyAsync` untuk `Idempotency-Key` aksi |
| `Common/Services/ProcurementApprovalGateway.cs` | `CloseReturnedAsync` (tutup instance yang dikembalikan sebelum diajukan ulang), `WithdrawAsync` |
| `Wfl` `WorkflowService.ActionsV2.cs` | `CancelOnBehalfAsync` (refactor `CancelAsync` → `CancelCoreAsync`, perilaku lama tidak berubah) |
| `Wfl` `WorkflowService.cs` | **Perbaikan cacat** `AddStatusHistory`/`AddComment`: baris baru didaftarkan eksplisit sebagai `Added` (lihat 3.3) |
| `MasterData/Services/ProcurementItemService.cs`, `ProcurementFundSourceService.cs` | `PBJ-VAL-071`: barang/sumber dana dipakai baris rencana tidak boleh dihapus |
| `ApplicationDbContext.cs`, `ProcurementManagementServiceCollectionExtensions.cs` | `DbSet` dan registrasi DI |
| `Migrations/20261116031906_AddProcurementNeedPlan.cs` + Designer | 2 tabel, 11 index |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Dipulihkan** (lihat 3.3) lalu digenerate ulang EF |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru. Delta terhadap 1.1: riwayat di `GET /{id}/status-history` (seragam dengan PR), tambahan `GET /filters/metadata` dan `GET /summary`, `POST`/`revisions` menjawab 200 (pola `ApiResponse`), body `PUT` hanya baris (unit dan tahun tetap), galat membawa `errors.code`/`lineNo`/`existingId` |
| Database | 2 tabel baru. Delta kamus data: kolom `SubmissionCount` (kunci idempoten `Wfl` untuk pengajuan ulang); keunikan nomor pada pasangan `PlanNumber` + `VersionNo` karena revisi memakai nomor yang sama. Migration diterapkan ke `QuilvianNewDevSukma` 16 November 2026 |
| Model snapshot EF | Snapshot pada `HEAD` rusak oleh merge `27fac19c`: seluruh entity Pengadaan hilang dan tersisa satu potongan `PrcPurchaseRequest.Navigation("Lines")`, sehingga `dotnet ef migrations add` gagal untuk siapa pun. Dipulihkan dari sisi `6fb9ab5f`, lalu EF menulis ulang snapshot penuh dari model; diff terhadap sisi lain merge hanya menambah entity Pengadaan + `ActivationConditionCode` (tidak ada drift lain). Operasi modul lain dari hasil scaffold dibuang |
| Perbaikan `Wfl` | `WorkflowService.CreateAndSubmitOnBehalfAsync` (`BE-PBJ-005`) gagal 409 di runtime: riwayat status baru ber-`Id` terisi yang ditambahkan lewat navigasi instance terlacak di-`UPDATE` EF Core (aturan sejak EF 3.0), 0 baris → `DbUpdateConcurrencyException`. Ditemukan lewat log EF tingkat Debug. Perbaikan: `_dbContext.Set<T>().Add(...)` eksplisit di `AddStatusHistory` dan `AddComment`. Pola Create lalu Submit dalam satu request juga dipakai HR (`AttendanceCorrectionService`, `LeaveAdjustmentService`), jadi perbaikan ini ikut berlaku bagi HR |
| Keamanan/Auth | Data disaring `ProcurementDataScopeService` (resource `ProcurementNeedPlan`); unit lain → 404. Izin per endpoint di bagian 4; `AccessAction` = argumen ke-2 `AccessPermission` |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Planning / Need Plan

Base URL `api/v1/corporate/procurement-management/need-plans`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status, urutan, tahun | `ProcurementNeedPlan : Read` |
| `GET` | `/summary` | Jumlah per status, versi berlaku, menunggu persetujuan | `ProcurementNeedPlan : Read` |
| `GET` | `/` | Daftar (bawaan menyembunyikan `SUPERSEDED`; `includeHistory=true` menampilkan semua) | `ProcurementNeedPlan : Read` |
| `GET` | `/{id}` | Detail + pemakaian baris (dipakai PR, dialokasikan paket, sisa) | `ProcurementNeedPlan : Read` |
| `GET` | `/{id}/versions` | Semua versi bernomor sama | `ProcurementNeedPlan : Read` |
| `GET` | `/{id}/compare/{otherId}` | Banding per `LineKey`: sebelum = versi lebih kecil | `ProcurementNeedPlan : Read` |
| `GET` | `/{id}/status-history` | Riwayat | `ProcurementNeedPlan : Read` |
| `POST` | `/` | Buat versi 1 | `ProcurementNeedPlan : Create` |
| `PUT` | `/{id}` | Ubah baris `DRAFT`/`RETURNED` | `ProcurementNeedPlan : Update` |
| `POST` | `/{id}/submit` | Ajukan (`Idempotency-Key`) | `ProcurementNeedPlan : Submit` |
| `POST` | `/{id}/cancel` | Batalkan `DRAFT` | `ProcurementNeedPlan : Update` |
| `POST` | `/{id}/revisions` | Buat revisi dari versi berlaku | `ProcurementNeedPlan : Revise` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release -p:UseSharedCompilation=false` (diulang sesudah setiap perbaikan) | 0 error, 601 warning (= baseline hari ini); 0 warning dari berkas Pengadaan, `Wfl`, atau migration yang berubah | `PASS` | Keluaran build |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 53 berkas `.cs` yang berubah/baru | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` | Laporan QBE |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| `dotnet ef database update` ke `QuilvianNewDevSukma` (izin pemilik 16 November 2026) | `AddProcurementNeedPlan`, `AddProcurementBudget`, `AddProcurementPackagePlan` diterapkan; 0 pending | `PASS` | Keluaran perintah + `migrations list` |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Uji runtime HTTP ke build baru di `https://localhost:7185` terhadap `QuilvianNewDevSukma`, aktor non-SuperAdmin `TEST-PBJ` (fixture lewat API) | Lihat tabel skenario di bawah; seluruh suite Pengadaan 123 skenario `PASS` | `PASS` | `results.json` scratchpad sesi + query DB |

**Fixture uji (`TEST-PBJ-20261116110649`, dibuat lewat API sebagai SuperAdmin, belum dibersihkan).** Lima unit uji
(`TEST-PBJ IGD/Radiologi/Pengadaan/Manajemen/Keuangan Uji`), tujuh jabatan, delapan akun `Employee` **tanpa role**
(`reqA`, `reqA2`, `headA`, `reqB`, `proc`, `mgr`, `dirk`, `keu`), dan policy Akses Role per unit × jabatan sesuai peta
peran `permission-audit-matrix.md` bagian 3. Konfigurasi approval uji (izin pemilik "nilai uji di dev"): definisi
`PBJ-*` diaktifkan, satu baris `MstApprovalMatrix` fallback `SpecificUser` per langkah, dan empat
`MstRejectionReason` — semuanya bertanda `DATA UJI PBJ (bukan nilai SOP)`. Komitmen pagu uji Rp10.000.000
(`SourceDocumentType = UJI_PBJ`) disisipkan lewat DB sesuai catatan `AT-PP-45`. Sisa fixture percobaan pertama
(`TEST-PBJ-20261116105253`) juga masih ada.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `AT-PP-40` | Rencana kedua IGD 2027 → 409 PBJ-VAL-040 + tautan rencana yang ada | `PASS` | http=409; code=PBJ-VAL-040; message=Unit TEST-PBJ IGD Uji 20261116110649 sudah memiliki rencana 2027. Buka rencana itu atau buat revisi. |
| `AT-PP-41` | Revisi disetujui → v1 SUPERSEDED, v2 IsCurrentVersion, LineKey sama, nomor sama | `PASS` | approvals=[['PBJ-RKB-01', 'headA', 200], ['PBJ-RKB-02', 'mgr', 200]]; v1=SUPERSEDED; v2=APPROVED |
| `AT-PP-42` | Revisi menurunkan monitor ke 5 padahal dipakai PR 10.0 → 422 PBJ-VAL-042 | `PASS` | http=422; code=PBJ-VAL-042; message=Baris TEST-PBJ Monitor Pasien 1794802030: sudah dipakai 10 UNIT, tidak bisa diturunkan ke 5. |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `B-01` | Buat rencana IGD 2027 v1 → DRAFT tanpa nomor, total server | `PASS` | http=200; status=DRAFT; total=509000000.0 |
| `B-02` | Pengguna Radiologi membuka rencana IGD → 404 (cakupan unit) | `PASS` | http=404; code=None; message=Rencana tidak ditemukan atau bukan milik unit Anda. |
| `B-03` | Daftar rencana Radiologi tidak memuat rencana IGD | `PASS` | http=200; count=0 |
| `B-04` | Membuat rencana untuk unit bukan penempatan → 422 PBJ-VAL-010 | `PASS` | http=422; code=PBJ-VAL-010; message=Anda tidak terdaftar di unit TEST-PBJ Radiologi Uji 20261116110649. |
| `B-05` | Tahun di luar rentang → 422 PBJ-VAL-043 | `PASS` | http=422; code=PBJ-VAL-043; message=Tahun rencana harus antara 2025 dan 2028. |
| `B-06` | Barang sama dua kali → 422 PBJ-VAL-044 dengan lineNo | `PASS` | http=422; code=PBJ-VAL-044; message=Barang KTL-000010 sudah ada di rencana ini. Gabungkan jumlahnya. |
| `B-07` | Baris baru dengan barang nonaktif → 422 PBJ-VAL-005 | `PASS` | http=422; code=PBJ-VAL-005; message=Barang pada baris 3 tidak aktif di katalog. Pilih barang lain atau minta Tim Pengadaan mengaktifkannya. |
| `B-08` | Ubah draf menambah baris; LineKey baris lama tetap | `PASS` | http=200; lines=3 |
| `B-09` | Ubah dengan RowVersion lama → 409 PBJ-VAL-090 | `PASS` | http=409; code=PBJ-VAL-090; message=Data sudah diubah orang lain. Muat ulang lalu ulangi perubahan Anda. |
| `B-10` | Hapus baris kasa (baris lain tetap) | `PASS` | http=200 |
| `B-11` | Definisi PBJ-RKB tidak aktif → 409 PBJ-VAL-091, rencana tetap DRAFT, tanpa instance | `PASS` | http=409; code=PBJ-VAL-091; message=Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator. |
| `B-13` | Ajukan → PENDING_APPROVAL, nomor RKB, instance Wfl | `PASS` | http=200; status=PENDING_APPROVAL; number=RKB-000006; message=Rencana berhasil diajukan. |
| `B-14` | Ajukan ulang dengan Idempotency-Key sama → hasil pertama, satu SUBMIT | `PASS` | http=200; submitRows=1 |
| `B-16` | Disetujui kedua jenjang di kotak masuk → APPROVED, versi berlaku, WF_APPROVED sekali | `PASS` | approvals=[['PBJ-RKB-01', 'headA', 200], ['PBJ-RKB-02', 'mgr', 200]]; status=APPROVED; wfApproved=1 |
| `B-17-planB` | Rencana planB 5 kursi roda diajukan dan disetujui | `PASS` | approvals=[['PBJ-RKB-01', 'headA', 200], ['PBJ-RKB-02', 'mgr', 200]] |
| `B-17-planP` | Rencana planP 22 kursi roda diajukan dan disetujui | `PASS` | approvals=[['PBJ-RKB-01', 'headA', 200], ['PBJ-RKB-02', 'mgr', 200]] |
| `B-18` | Batalkan draf rencana → CANCELLED | `PASS` | http=200 |
| `B-19` | Batalkan rencana APPROVED → 409 PBJ-VAL-093 | `PASS` | http=409; code=PBJ-VAL-093; message=Status rencana tidak memungkinkan tindakan ini. |
| `B-20` | Riwayat rencana memuat CREATE, UPDATE, SUBMIT, WF_APPROVED | `PASS` |  |
| `E-10` | Revisi rencana di bawah alokasi rencana paket aktif → 422 PBJ-VAL-042 | `PASS` | http=422; code=PBJ-VAL-042; message=Baris TEST-PBJ Kursi Roda Standar 1794802030: sudah dipakai 3 UNIT, tidak bisa diturunkan ke 2. |
| `F-AT-4` | Penyedia akses rencana kebutuhan aktif: rencana APPROVED → 409 PBJ-VAL-083 (bukan 404 fail-closed) | `PASS` | http=409; code=None; message=Dokumen sudah final; lampiran tidak dapat diubah. |
| `G-01` | Buat revisi tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan revisi wajib diisi. |
| `G-02` | Revisi v2 dibuat dari v1 berlaku; LineKey disalin | `PASS` | http=200 |
| `G-03` | Revisi kedua saat v2 belum final → 409 PBJ-VAL-041 | `PASS` | http=409; code=PBJ-VAL-041; message=Masih ada revisi yang belum selesai. |
| `G-04` | Banding versi v1→v2: monitor 10 → 12 (CHANGED), sebelum = versi lebih kecil | `PASS` | change=CHANGED; before=10.0; after=12.0 |
| `G-05` | Daftar versi rencana memuat v1 dan v2 | `PASS` | ['SUPERSEDED', 'APPROVED'] |
| `G-06` | plan-line-options sesudah revisi: rencana 12, dipakai 10, sisa 2 (LineKey sama) | `PASS` |  |
| `G-07` | Daftar bawaan menyembunyikan versi SUPERSEDED; includeHistory menampilkannya | `PASS` | http=200 |

32 skenario, 32 `PASS`.

**Tidak dijalankan:** `dotnet test` (tidak ada project test).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PP-40` rencana kedua unit-tahun → 409 | Terpenuhi | `AT-PP-40` (dengan `existingId`) |
| `AT-PP-41` revisi disetujui → v1 `SUPERSEDED`, v2 berlaku, `LineKey` sama | Terpenuhi | `AT-PP-41`, `G-05`, `G-06` |
| `AT-PP-42` revisi di bawah pemakaian PR → 422 | Terpenuhi | `AT-PP-42`; juga terhadap alokasi paket `E-10` |
| Instance `Wfl` pertama nyata `PBJ-RKB` disetujui end-to-end | Terpenuhi | `B-13`..`B-16`, `B-17-*` |
| Delta `PBJ-DEC-036`: `ProcurementNeedPlan : ReadAllUnits` + cakupan | Terpenuhi | `B-02`, `B-03`, `ProcurementPlanningExplicitPermissions.cs` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Definisi `PBJ-*` dan matriksnya di dev memakai **data uji**, bukan nilai SOP (`PBJ-OQ-019` masih terbuka) |
| Masalah yang diketahui | Endpoint penyunting langkah `Wfl` belum dapat mengisi `ActivationConditionCode` (tetap lewat data awal) |
| Risiko tersisa | Perbaikan `AddStatusHistory`/`AddComment` menyentuh jalur HR; belum ada uji runtime HR karena definisi HR di dev tanpa langkah (lihat `BE-PBJ-001`) |
| Perubahan sampingan | Snapshot EF dipulihkan (cacat merge, wajib agar migration bisa dibuat) |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. 17 berkas berubah dan 11 jalur baru di backend (lihat `git status --short` pada laporan `BE-PBJ-011`). |
| Langkah berikutnya | `FE-PBJ-006`; bersihkan fixture `TEST-PBJ` dan data uji `Wfl` atas izin pemilik |
