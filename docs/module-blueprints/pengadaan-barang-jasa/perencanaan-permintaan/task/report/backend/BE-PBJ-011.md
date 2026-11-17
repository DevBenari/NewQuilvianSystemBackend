# Laporan Perubahan Backend — `BE-PBJ-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-011` |
| Judul | Konsolidasi dan rencana paket |
| Slice | `MVP-4` — konsolidasi dan rencana paket (`EPIC PBJ-02`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-011` |
| Trace | `PBJ-DEC-028`, `033`, `036`; FR-PBJ-070..072; STATE §3, §5; `PBJ-VAL-050`, `051`, `042`; `AT-PP-43`, `44` |
| Contract version | `PBJ-PP-API` 1.1 (dengan delta di bagian 3.3), `PBJ-PP-STATE` 1.0, `PBJ-PP-VAL` 1.0, `PBJ-PP-INT` 1.1, `PBJ-PP-PERM` 1.2 — semua `approved` |
| Dependency | `BE-PBJ-009` ✅ |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 2, database 2, keamanan 1) |
| Task mode | `BACKEND` (diberikan pemilik 16 November 2026 lewat jawaban "BE lalu FE") |
| Target tulis | `Areas/Corporate/ProcurementManagement/Planning/**`, service master (cek pemakaian), konfigurasi EF, `ApplicationDbContext.cs`, extension DI, migration `AddProcurementPackagePlan` |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `27fac19c` (belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ **SELESAI.** `AT-PP-43`, `44` dan penolakan revisi di bawah alokasi paket terbukti runtime (14 skenario `PASS`); migration diterapkan ke `QuilvianNewDevSukma` |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Planning` |
| Registry | `Corporate / Procurement · ProcurementManagement / Planning · Prc · ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs`, service master Pengadaan |
| Arketipe endpoint | Transaksi ber-lifecycle (rencana paket) + baca-saja (konsolidasi) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-CODE-001..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (53 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

Tim Pengadaan belum bisa menggabungkan kebutuhan antarunit menjadi rencana paket. Task ini menyediakan konsolidasi
baris rencana berlaku per barang dan rencana paket yang mengalokasikan baris-baris itu, lengkap dengan approval.

## 2. Proses bisnis

1. **Konsolidasi** tahun anggaran: per barang, total kuantitas rencana berlaku, unit asal (dengan `LineKey`), yang
   sudah dialokasikan ke rencana paket hidup, dan sisanya.
2. **Rencana paket** dibuat dari baris konsolidasi: setiap baris menunjuk `LineKey` rencana berlaku setahun dengan
   paket, unit dalam cakupan, berjenis sama dengan kepala (`PBJ-VAL-051`), dan Σ alokasi seluruh paket hidup ≤
   kuantitas baris (`PBJ-VAL-050`, diperiksa di bawah kunci per `LineKey`). Nilai estimasi = Σ alokasi × harga
   estimasi baris rencana. Unit asal tersimpan per baris.
3. Ajukan → nomor `RPK-000001`, approval `PBJ-RPK`. Batal dari `DRAFT`/`RETURNED`/`APPROVED` (alasan wajib bila
   disetujui); alokasinya kembali ke sisa konsolidasi.
4. Revisi rencana kebutuhan tidak boleh menurunkan baris di bawah alokasi paket hidup (perluasan `PBJ-VAL-042`).

**Contoh:** kursi roda 2027 IGD 3, Radiologi 5, Pengadaan 22 → satu baris total 30. Paket A mengalokasikan 3 + 3 + 22
(nilai Rp84.000.000). Paket B ingin 3 lagi dari Radiologi → ditolak "melebihi rencana (5), sudah dialokasikan 3".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak bagian Consolidation/Package Plan, STATE §3, VAL §2, kamus data `PrcPackagePlan*`; pola `InpatientEpisode`
(beberapa controller satu `ControllerName`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Planning/Models/PrcPackagePlan.cs`, `PrcPackagePlanLine.cs` + konfigurasi | Entity baru; unik nomor bila terisi, unik (paket, `LineKey`) baris hidup, CHECK alokasi > 0 |
| `Planning/DTOs/PackagePlanDtos.cs` | DTO konsolidasi dan rencana paket |
| `Planning/Services/ProcurementPlanConsolidationService.cs` | Konsolidasi |
| `Planning/Services/ProcurementPackagePlanService.cs` | Rencana paket, penyedia akses lampiran, penerap status `PBJ_PACKAGE_PLAN` |
| `Planning/Services/ProcurementPlanQuotaService.cs` | Alokasi paket dalam pemakaian baris; baris berlaku per tahun |
| `Planning/Controllers/PackagePlansController.cs`, `PlanConsolidationsController.cs` | Endpoint bagian 4 |
| `Planning/ProcurementPlanningExplicitPermissions.cs` | `ProcurementPackagePlan : ReadAllUnits` |
| `MasterData/Services/ProcurementItemService.cs` | Barang dipakai rencana paket tidak boleh dihapus |
| `Migrations/20261116034131_AddProcurementPackagePlan.cs` + Designer, snapshot | 2 tabel, 10 index |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru; tambahan `filters/metadata`, `summary`, `status-history`; `POST` 200. `PlanConsolidationsController` berbagi `ControllerName = ProcurementPackagePlan` (izin `ProcurementPackagePlan : Read` seperti kontrak) |
| Database | 2 tabel baru; delta kolom `SubmissionCount`, `SubmittedAt`, `ApprovedAt`. Diterapkan ke `QuilvianNewDevSukma` |
| Keamanan/Auth | Tanpa `ReadAllUnits`, pengguna hanya melihat paket yang memuat baris unit penempatannya |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Planning / Consolidation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Gabungan per barang (`fiscalYear`, `itemCategoryId?`, `procurementTypeId?`) | `ProcurementPackagePlan : Read` |

#### Corporate / Procurement Management / Planning / Package Plan

Base URL `api/v1/corporate/procurement-management/package-plans`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata`, `/summary`, `/`, `/{id}`, `/{id}/status-history` | Baca | `ProcurementPackagePlan : Read` |
| `POST` | `/` | Buat dari baris konsolidasi | `ProcurementPackagePlan : Create` |
| `PUT` | `/{id}` | Ubah `DRAFT`/`RETURNED` | `ProcurementPackagePlan : Update` |
| `POST` | `/{id}/submit` | Ajukan (`Idempotency-Key`) | `ProcurementPackagePlan : Submit` |
| `POST` | `/{id}/cancel` | Batalkan | `ProcurementPackagePlan : Cancel` |

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
| `AT-PP-43` | Alokasi dua rencana paket melebihi baris rencana (5: 3+3) → 422 PBJ-VAL-050 | `PASS` | http=422; code=PBJ-VAL-050; message=Alokasi TEST-PBJ Kursi Roda Standar 1794802030 unit TEST-PBJ Radiologi Uji 20261116110649 melebihi rencana (5), sudah dialokasikan 3. |
| `AT-PP-44` | Konsolidasi tiga unit satu barang → satu baris, total 30, tiga unit asal | `PASS` | http=200; total=30.0; units=[3.0, 5.0, 22.0] |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `E-01` | Pengguna tanpa ProcurementPackagePlan : Read → 403 | `PASS` | http=403; code=None; message=Anda tidak memiliki akses ke menu atau fitur ini. |
| `E-02` | Buat rencana paket dari konsolidasi → DRAFT, nilai estimasi server 28×3jt, unit asal tersimpan | `PASS` | http=200; value=84000000.0; units=3; message=Draf rencana paket berhasil disimpan. |
| `E-03` | Barang jenis Aset dalam paket jenis Stok → 422 PBJ-VAL-051 | `PASS` | http=422; code=PBJ-VAL-051; message=Semua baris paket harus berjenis Barang Stok. |
| `E-04` | Baris rencana tahun lain → 422 PBJ-VAL-050 | `PASS` | http=422; code=PBJ-VAL-050; message=Baris 1 tidak menunjuk rencana kebutuhan berlaku tahun 2028. |
| `E-05` | Ajukan rencana paket → nomor RPK, disetujui Manajer Pengadaan → APPROVED | `PASS` | approvals=[['PBJ-RPK-01', 'mgr', 200]]; number=RPK-000001 |
| `E-06` | Konsolidasi sesudah alokasi → dialokasikan 28, sisa 2 | `PASS` | allocated=28.0 |
| `E-07` | Batalkan rencana paket APPROVED tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan wajib diisi. |
| `E-08` | Rencana paket draf dibatalkan → alokasinya kembali ke sisa konsolidasi | `PASS` | before=0; after=2.0 |
| `E-09` | Riwayat rencana paket: CREATE, SUBMIT, WF_APPROVED | `PASS` | ['CREATE', 'SUBMIT', 'WF_APPROVED'] |
| `E-10` | Revisi rencana di bawah alokasi rencana paket aktif → 422 PBJ-VAL-042 | `PASS` | http=422; code=PBJ-VAL-042; message=Baris TEST-PBJ Kursi Roda Standar 1794802030: sudah dipakai 3 UNIT, tidak bisa diturunkan ke 2. |
| `F-AT-5` | Penyedia akses rencana paket dan pagu terdaftar (daftar lampiran 200) | `PASS` |  |

14 skenario, 14 `PASS`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PP-43` alokasi melebihi baris → 422 | Terpenuhi | `AT-PP-43` |
| `AT-PP-44` tiga unit satu barang → satu baris gabungan | Terpenuhi | `AT-PP-44`, `E-02` (unit asal tersimpan) |
| Revisi rencana di bawah alokasi paket ditolak | Terpenuhi | `E-10` |
| Delta `PBJ-DEC-036` | Terpenuhi | `ProcurementPlanningExplicitPermissions.cs` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `IndicativeMethod` teks bebas sampai `PBJ-OQ-005` diputuskan |
| Masalah yang diketahui | `GetApprovedAsync` untuk `pemilihan-penyedia` belum dibangun (sesuai roadmap "Tidak masuk roadmap ini") |
| Risiko tersisa | `NONE` di luar data uji dev |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Diubah: `WorkflowService.ActionsV2.cs`, `WorkflowService.cs`, `ProcurementDocumentHistoryActions.cs`, `ProcurementPermissions.cs`, `ProcurementStatuses.cs`, `ProcurementApprovalGateway.cs`, `ProcurementDocumentHistoryWriter.cs`, `ProcurementFundSourceService.cs`, `ProcurementItemCategoryService.cs`, `ProcurementItemService.cs`, `ProcurementManagementServiceCollectionExtensions.cs`, `PurchaseRequestsController.cs`, `PurchaseRequestDtos.cs`, `ProcurementPurchaseRequestService.cs`, `ProcurementRequestExceptions.cs`, `ApplicationDbContextModelSnapshot.cs`, `ApplicationDbContext.cs`. Baru: `Planning/`, `PurchaseRequestVerificationsController.cs`, `PurchaseRequestVerificationDtos.cs`, `ProcurementPurchaseRequestVerificationService.cs`, 3 migration + Designer, `Repositories/Configurations/.../Planning/`, laporan `BE-PBJ-007`..`011` |
| Langkah berikutnya | `FE-PBJ-008` |
