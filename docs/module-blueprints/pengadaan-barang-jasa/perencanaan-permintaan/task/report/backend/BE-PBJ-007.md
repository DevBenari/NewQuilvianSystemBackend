# Laporan Perubahan Backend — `BE-PBJ-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-007` |
| Judul | Pengajuan, batal, tarik PR |
| Slice | `MVP-1` — permintaan pengadaan (`EPIC PBJ-04`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-007` |
| Trace | `PBJ-DEC-014`, `016`, `025`/`031`, `029`, `033` (`G-04`); FR-PBJ-030..035; STATE §1; `PBJ-VAL-001`..`012`, `030`, `093`; `AT-PP-01`..`07`, `10`, `36` |
| Contract version | `PBJ-PP-API` 1.1 (dengan delta di bagian 3.3), `PBJ-PP-STATE` 1.0, `PBJ-PP-VAL` 1.0, `PBJ-PP-INT` 1.1, `PBJ-PP-PERM` 1.2 — semua `approved` |
| Dependency | `BE-PBJ-006` ✅, `BE-PBJ-009` ✅ |
| Klasifikasi | `HEAVY` (logika 3, konkurensi 2, kontrak API 2, integrasi `Wfl` 2) |
| Task mode | `BACKEND` (diberikan pemilik 16 November 2026 lewat jawaban "BE lalu FE") |
| Target tulis | `Areas/Corporate/ProcurementManagement/{Request,Planning,Common}/**` |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `27fac19c` (belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ **SELESAI.** `AT-PP-01`..`07`, `10`, `36` terbukti runtime (21 skenario `PASS`, termasuk dua pengajuan bersamaan berebut sisa rencana) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Request` |
| Registry | `Corporate / Procurement · ProcurementManagement / Request · Prc · ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk — |
| Arketipe endpoint | Transaksi ber-lifecycle — perpindahan status `POST /{id}/submit`, `POST /{id}/cancel` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-CODE-001..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (53 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

PR draft (`BE-PBJ-006`) belum dapat diajukan, dibatalkan, atau ditarik. Task ini menambahkan pengajuan bernomor dengan
pembagian kuantitas dalam/luar rencana, pembatalan oleh pengusul, dan penarikan dari persetujuan.

## 2. Proses bisnis

1. **Ajukan** hanya oleh pembuat PR saat `DRAFT`/`RETURNED`. Server memeriksa `PBJ-VAL-001` minimal satu baris,
   `002` jumlah > 0, `005` barang aktif, `006` alasan ≥ 10 karakter, `007` tanggal tidak lampau, `008` mendesak wajib
   alasan + batas waktu, `010` penempatan di unit PR, `011` sumber dana aktif, `012` baris rencana cocok barang, unit,
   dan tahun.
2. **Pembagian rencana (`PBJ-DEC-029`)** di bawah kunci `pg_advisory_xact_lock` per `LineKey`: sisa = jumlah baris
   rencana berlaku − pemakaian PR lain berstatus pemakan. Kelebihan menjadi "luar rencana" dan wajib beralasan per
   baris (`PBJ-VAL-009`). Pemeriksaan awal dilakukan sebelum nomor dialokasikan agar galat tidak menghanguskan nomor.
3. Nomor `PR-000001` dialokasikan saat pertama diajukan; `SubmissionCount` naik; hasil cek stok lama dikosongkan
   (pengajuan ulang menuntut verifikasi ulang). `Idempotency-Key` sama → hasil pertama.
4. Pengajuan ulang PR yang dikembalikan penyetuju menutup instance `Wfl` lama lebih dulu (`CloseReturnedAsync`).
5. **Batal** (`DRAFT`/`SUBMITTED`/`RETURNED`) oleh pembuat; alasan wajib bila sudah bernomor (`PBJ-VAL-030`). **Tarik**
   (`PENDING_APPROVAL`): instance `Wfl` ditarik (`Withdrawn`) lalu PR `CANCELLED` dengan satu riwayat `WITHDRAW`.
6. `GET /plan-line-options` memberi baris rencana berlaku beserta sisanya untuk form PR.

**Contoh:** rencana IGD 10 monitor. PR-1 6 unit → dalam rencana 6. PR-2 6 unit tanpa alasan → 422 "Baris 1: 2 UNIT di
luar rencana. Isi alasannya."; dengan alasan → dalam 4, luar 2. PR-1 dibatalkan → sisa naik 6. Dua PR @6 diajukan
bersamaan pada sisa 6 → total dalam rencana keduanya 6.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

STATE §1, VAL §1, kartu `BE-PBJ-007`, layanan PR `BE-PBJ-006`, `ProcurementPlanQuotaService` (`BE-PBJ-009`),
`WorkflowService.WithdrawAsync`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Request/Services/ProcurementPurchaseRequestService.cs` | `SubmitAsync`, `CancelAsync` (termasuk tarik), `GetPlanLineOptionsAsync`, `ApplyFlags`; validasi pengajuan; pembagian rencana; alokasi nomor |
| `Request/Controllers/PurchaseRequestsController.cs` | `POST /{id}/submit`, `POST /{id}/cancel`, `GET /plan-line-options`; galat membawa `errors.code`/`lineNo` |
| `Request/DTOs/PurchaseRequestDtos.cs` | `CancelPurchaseRequestRequest`, `PlanLineOptionsQuery`, `PlanLineQuotaResponse`, flag `CanSubmit`/`CanCancel`/`CanWithdraw`/`CanVerify`/`CanCancelApproved`, `ClosedAt`, `VerifiedBy*` |
| `Request/Services/ProcurementRequestExceptions.cs` | `LineNo` opsional pada galat 422 |
| `Planning/Services/ProcurementPlanQuotaService.cs` | `GetCurrentLinesAsync`, `GetOptionsAsync`, `LockAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint sesuai kontrak; galat kini membawa `errors.code` (sebelumnya hanya pesan) — tambahan yang kompatibel |
| Database | Tidak ada perubahan skema |
| Keamanan/Auth | Ajukan/batal hanya pembuat (403 untuk rekan satu unit); izin `Submit`/`Cancel`; `plan-line-options` unit di luar cakupan → 404 |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Request / Purchase Request

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/plan-line-options` | Baris rencana berlaku + sisa (`departmentId`, `fiscalYear`, `itemId?`) | `ProcurementRequest : Create` |
| `POST` | `/{id}/submit` | Ajukan (`Idempotency-Key`) | `ProcurementRequest : Submit` |
| `POST` | `/{id}/cancel` | Batal atau tarik dari persetujuan | `ProcurementRequest : Cancel` |

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
| `AT-PP-01` | Ajukan → SUBMITTED, RequestNumber PR-xxxxxx, satu riwayat SUBMIT, dalam rencana 6 | `PASS` | http=200; number=PR-000001; message=Permintaan berhasil diajukan. |
| `AT-PP-01a` | PR draft tanpa nomor | `PASS` | http=200 |
| `AT-PP-02a` | Ajukan tanpa baris → 422 PBJ-VAL-001 | `PASS` | http=422; code=PBJ-VAL-001; message=Pengajuan harus berisi minimal satu barang atau jasa. |
| `AT-PP-02c` | Tanpa alasan kebutuhan → 422 PBJ-VAL-006; tetap DRAFT tanpa nomor | `PASS` | http=422; code=PBJ-VAL-006; message=Alasan kebutuhan wajib diisi (minimal 10 karakter). |
| `AT-PP-03` | PR mendesak tanpa alasan → 422 PBJ-VAL-008 | `PASS` | http=422; code=PBJ-VAL-008; message=Permintaan mendesak wajib berisi alasan dan batas waktu. |
| `AT-PP-04` | PR-2 dengan alasan → dalam rencana 4, luar rencana 2, IsOutOfPlan | `PASS` | http=200; in=4.0; out=2.0 |
| `AT-PP-05` | PR-2 6 unit (sisa 4) tanpa alasan luar rencana → 422 PBJ-VAL-009, nomor tidak dialokasikan | `PASS` | http=422; code=PBJ-VAL-009; message=Baris 1: 2 UNIT di luar rencana. Isi alasannya. |
| `AT-PP-06` | PR-1 dibatalkan → sisa rencana naik 6 | `PASS` | http=200; before=0; after=6.0 |
| `AT-PP-07` | Dua PR @6.0 unit diajukan bersamaan pada sisa 6.0 → total dalam rencana tetap 6.0 | `PASS` | http=[200, 200]; sumInPlan=6.0000 |
| `AT-PP-10` | submit dua kali dengan Idempotency-Key sama → satu nomor, satu SUBMIT | `PASS` | http=200; submitRows=1 |
| `AT-PP-36` | Pengusul menarik PR PENDING_APPROVAL → PR CANCELLED, instance Wfl Withdrawn, satu riwayat WITHDRAW | `PASS` | http=200; pr=CANCELLED; wfl=Withdrawn; history=['CREATE', 'SUBMIT', 'STOCK_CHECK', 'VERIFY_COMPLETE', 'WF_RETURNED', 'SUBMIT', 'STOCK_CHECK', 'VERIFY_COMPLETE', 'WITHDRAW']; message=Permintaan berhasil dibatalkan. |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `D-01` | Batalkan PR bernomor tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan wajib diisi. |
| `D-02` | Tanggal dibutuhkan sebelum hari ini → 422 PBJ-VAL-007 | `PASS` | http=422; code=PBJ-VAL-007; message=Tanggal dibutuhkan tidak boleh sebelum hari ini. |
| `D-03` | Baris kursi roda menunjuk baris rencana monitor → 422 PBJ-VAL-012 | `PASS` | http=422; code=PBJ-VAL-012; message=Baris 1 tidak sesuai dengan rencana yang dipilih. |
| `D-05` | Rekan satu unit mengajukan PR orang lain → 403 | `PASS` | http=403; code=None; message=Hanya pembuat pengajuan yang dapat mengajukannya. |
| `D-06` | Flag aksi pembuat draf: canEdit/canSubmit/canCancel benar, canVerify salah | `PASS` |  |
| `D-07` | plan-line-options unit lain → 404 | `PASS` | http=404; code=None; message=Unit tidak ditemukan atau di luar cakupan Anda. |
| `F-09` | Dikembalikan Wfl → RETURNED; ajukan ulang menutup instance lama (Cancelled) dan mengosongkan cek stok; verifikasi ulang → instance baru :2 | `PASS` | oldInstance=Cancelled; newKey=PBJ_PR:9e5a8601510741209d4b80cfc756e2cc:2; message=Verifikasi berhasil diselesaikan. |
| `F-10` | Tarik PR PENDING_APPROVAL tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan wajib diisi. |
| `G-06` | plan-line-options sesudah revisi: rencana 12, dipakai 10, sisa 2 (LineKey sama) | `PASS` |  |

21 skenario, 21 `PASS`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PP-01` draft tanpa nomor → ajukan bernomor, satu `SUBMIT` | Terpenuhi | `AT-PP-01a`, `AT-PP-01` |
| `AT-PP-02` tanpa baris / jumlah 0 / tanpa alasan → 422, tetap draf tanpa nomor | Terpenuhi | `AT-PP-02a`..`c` |
| `AT-PP-03` mendesak tanpa alasan → 422 | Terpenuhi | `AT-PP-03` |
| `AT-PP-04`, `05`, `06` pembagian dan pengembalian sisa | Terpenuhi | `AT-PP-04`..`06` |
| `AT-PP-07` dua pengajuan bersamaan | Terpenuhi | `AT-PP-07` |
| `AT-PP-10` idempoten | Terpenuhi | `AT-PP-10` |
| `AT-PP-36` tarik → `CANCELLED`, `Wfl` `Withdrawn` | Terpenuhi | `AT-PP-36`, `F-10` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `Wfl` hanya mengizinkan tarik saat `InProgress`; di luar itu dijawab 409 dengan pesan `Wfl` |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Nomor yang telanjur dialokasikan hangus bila penyimpanan gagal sesudahnya (`DEC-PLT-008`, disengaja) |
| Perubahan sampingan | `NONE` |
| Interupsi | Perbaikan proyeksi satuan (`ToDictionaryAsync` dengan navigasi) setelah uji pertama 500; diulang `PASS` |
| Status Git | Belum di-commit. 17 berkas berubah dan 11 jalur baru di backend (lihat `git status --short` pada laporan `BE-PBJ-011`). |
| Langkah berikutnya | `FE-PBJ-004` |
