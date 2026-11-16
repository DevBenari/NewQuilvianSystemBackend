# Laporan Perubahan Backend — `BE-PBJ-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-008` |
| Judul | Verifikasi dan approval PR |
| Slice | `MVP-1` — permintaan pengadaan (`EPIC PBJ-04`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-008` |
| Trace | `PBJ-DEC-008`, `027`, `030`, `033`; FR-PBJ-040..044; STATE §1, §5; `PBJ-VAL-020`, `021`, `030`, `091`..`093`; `AT-PP-20`..`28`, `30`..`34` |
| Contract version | `PBJ-PP-API` 1.1 (dengan delta di bagian 3.3), `PBJ-PP-STATE` 1.0, `PBJ-PP-VAL` 1.0, `PBJ-PP-INT` 1.1, `PBJ-PP-PERM` 1.2 — semua `approved` |
| Dependency | `BE-PBJ-007` ✅, `BE-PBJ-010` ✅ |
| Klasifikasi | `HEAVY` (logika 3, kontrak API 2, integrasi `Wfl` 3, keamanan 2) |
| Task mode | `BACKEND` (diberikan pemilik 16 November 2026 lewat jawaban "BE lalu FE") |
| Target tulis | `Areas/Corporate/ProcurementManagement/{Request,Planning,Common}/**`, extension DI |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `27fac19c` (belum di-commit) |
| Tanggal | 16 November 2026 |
| Status | ✅ **SELESAI.** `AT-PP-20`..`28` dan `AT-PP-30`..`34` terbukti runtime (29 skenario `PASS`), termasuk persetujuan empat jenjang di kotak masuk dan sinkron ulang setelah penangan gagal |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Request` |
| Registry | `Corporate / Procurement · ProcurementManagement / Request · Prc · ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk — |
| Arketipe endpoint | Worklist verifikasi — antrean + `POST /{id}/<aksi>`, tanpa `POST /` maupun `DELETE` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-CODE-001..004`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE Strict `PASS` (53 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

PR yang diajukan belum dapat diverifikasi Tim Pengadaan maupun masuk persetujuan. Task ini menyediakan antrean
verifikasi, cek stok per baris, penyelesaian verifikasi yang mengirim PR ke approval `Wfl` dengan nominal kebutuhan
beli dan kode kondisi, pengembalian, penolakan, pembatalan PR yang sudah disetujui, serta penerapan hasil approval ke PR.

## 2. Proses bisnis

1. Antrean berisi PR `SUBMITTED` dalam cakupan (Tim Pengadaan memegang `ProcurementRequest : ReadAllUnits`).
2. **Cek stok** per baris saat `SUBMITTED`: dipenuhi ≤ tersedia dan ≤ diminta, tidak negatif (`PBJ-VAL-020`). Server
   menghitung kebutuhan beli = diminta − dipenuhi dan nilai beli = kebutuhan beli × perkiraan harga.
3. **Selesaikan** (semua baris sudah dicek, `PBJ-VAL-021`):
   - kebutuhan beli 0 → `FULFILLED_FROM_STOCK`, tanpa approval;
   - selain itu → uji lewat pagu per kategori (kelebihan = max(0, nilai beli − sisa pagu)); kode kondisi
     `PBJ_OUT_OF_PLAN`/`PBJ_OVER_BUDGET`; definisi `PBJ-PR` atau `PBJ-PR-MENDESAK`; instance dibuat **atas nama
     pengusul** (`Wfl` dulu), lalu PR `PENDING_APPROVAL`. Definisi tidak aktif → 409 `PBJ-VAL-091`, PR tetap `SUBMITTED`.
4. **Verifikator ≠ pengusul**: pengusul yang juga Tim Pengadaan ditolak 403.
5. **Kembalikan**/**tolak** dari `SUBMITTED`, alasan wajib (`PBJ-VAL-030`).
6. **Hasil approval** (penangan `PBJ_PR`): disetujui → `APPROVED`; ditolak → `REJECTED`; dikembalikan/minta revisi →
   `RETURNED`; ditarik → `CANCELLED`. Hanya PR `PENDING_APPROVAL` yang tertaut instance itu yang dipindahkan;
   pemanggilan ulang tidak menggandakan riwayat. Status `APPROVED` hanya lahir dari sinkron ini.
7. **Batal setelah disetujui** oleh Tim Pengadaan, alasan wajib; PR yang sudah masuk paket → 409 `PBJ-VAL-092`.
8. **Sinkron ulang** `POST /{id}/workflow/synchronize` bila penerapan hasil approval sempat gagal.

**Contoh:** 20 kursi roda @Rp3.000.000, 5 dipenuhi stok → kebutuhan beli 15, nilai Rp45.000.000. Sisa pagu Rp30.000.000
→ lewat pagu Rp15.000.000; instance `PBJ-PR` ber-`amount` 45.000.000 dengan langkah 01, 02, 03 (luar rencana), dan 04
(Direktur Keuangan). PR 2 kursi roda dalam rencana dan dalam pagu → hanya langkah 01 dan 02.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `BE-PBJ-008`, STATE §1/§5, VAL §1, `ApprovalInboxController`/`ApprovalInboxService`,
`WorkflowService.ApproveAsync`/`RejectAsync`/`RequestRevisionAsync`, `AttendanceCorrectionController` (pola
synchronize).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Request/Services/ProcurementPurchaseRequestVerificationService.cs` | Antrean, cek stok, selesai, kembalikan, tolak, batal-disetujui, sinkron ulang, penerap status `PBJ_PR` (`PurchaseRequestWorkflowStatusApplier`, diambil malas) |
| `Request/Controllers/PurchaseRequestVerificationsController.cs` | Endpoint bagian 4 |
| `Request/DTOs/PurchaseRequestVerificationDtos.cs` | DTO cek stok, selesai, kembalikan, tolak |
| `ProcurementManagementServiceCollectionExtensions.cs` | Registrasi service + penerap |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta: body `complete` mewajibkan `expectedRowVersion`; **tambahan** `POST /{id}/workflow/synchronize` (`ProcurementRequestVerification : Verify`) — arsitektur bagian 8 mengandalkan "endpoint synchronize yang sudah ada", padahal `Wfl` tidak punya endpoint sinkron generik (hanya milik Attendance) |
| Database | Tidak ada perubahan skema |
| Keamanan/Auth | Izin `Read`/`Verify`/`CancelApproved`; verifikator ≠ pengusul; penyetuju bertindak di kotak masuk `Wfl` (`ApprovalInbox : *`); anti-setujui-sendiri berlapis (`Wfl` + penangan) |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Request / Verification

Base URL `api/v1/corporate/procurement-management/purchase-request-verifications`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Antrean PR `SUBMITTED` | `ProcurementRequestVerification : Read` |
| `GET` | `/stock-checks` | Daftar hasil cek stok per baris | `ProcurementRequestVerification : Read` |
| `PUT` | `/{id}/stock-checks` | Catat/ubah cek stok | `ProcurementRequestVerification : Verify` |
| `POST` | `/{id}/complete` | Selesaikan verifikasi (`Idempotency-Key`) | `ProcurementRequestVerification : Verify` |
| `POST` | `/{id}/return` | Kembalikan ke pengusul | `ProcurementRequestVerification : Verify` |
| `POST` | `/{id}/reject` | Tolak | `ProcurementRequestVerification : Verify` |
| `POST` | `/{id}/cancel-approved` | Batalkan PR disetujui yang belum masuk paket | `ProcurementRequestVerification : CancelApproved` |
| `POST` | `/{id}/workflow/synchronize` | Sinkron ulang status dari `Wfl` | `ProcurementRequestVerification : Verify` |

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
| `AT-PP-20` | Selesaikan → PENDING_APPROVAL, PurchaseQuantity 15, PurchaseTotal 45jt, instance Wfl amount 45jt | `PASS` | http=200; status=PENDING_APPROVAL; amount=45000000.0; message=Verifikasi berhasil diselesaikan. |
| `AT-PP-21` | Semua baris dipenuhi stok → FULFILLED_FROM_STOCK, tanpa instance Wfl | `PASS` | http=200; status=FULFILLED_FROM_STOCK |
| `AT-PP-22` | Dipenuhi 8 dari tersedia 5 → 422 PBJ-VAL-020 | `PASS` | http=422; code=PBJ-VAL-020; message=Jumlah dipenuhi dari stok tidak boleh melebihi stok tersedia (5) maupun jumlah diminta (20). |
| `AT-PP-23` | Selesaikan dengan satu baris belum dicek → 422 PBJ-VAL-021 | `PASS` | http=422; code=PBJ-VAL-021; message=Masih ada 1 baris yang belum dicek stoknya. |
| `AT-PP-24` | Definisi PBJ-PR dinonaktifkan → 409 PBJ-VAL-091; PR tetap SUBMITTED; tanpa instance | `PASS` | http=409; code=PBJ-VAL-091; message=Alur persetujuan pengadaan belum dikonfigurasi. Hubungi administrator. |
| `AT-PP-25` | Sisa pagu 30jt, nilai beli 45jt → IsOverBudget, kelebihan 15jt, PBJ_OVER_BUDGET & langkah Direktur Keuangan | `PASS` | over=15000000.00; codes=['PBJ_OUT_OF_PLAN', 'PBJ_OVER_BUDGET']; steps=['PBJ-PR-01', 'PBJ-PR-02', 'PBJ-PR-03', 'PBJ-PR-04'] |
| `AT-PP-26` | PR dalam rencana dan dalam pagu → tanpa kode kondisi, langkah bersyarat tidak dibuat | `PASS` | http=200; steps=['PBJ-PR-01', 'PBJ-PR-02']; message=Verifikasi berhasil diselesaikan. |
| `AT-PP-27` | Tim Pengadaan yang juga pengusul memverifikasi PR-nya sendiri → 403 (cek stok dan selesai) | `PASS` | message=Pengusul tidak dapat memverifikasi pengajuannya sendiri. |
| `AT-PP-28` | Kembalikan dengan alasan → RETURNED, alasan tercatat di riwayat | `PASS` | http=200 |
| `AT-PP-28a` | Kembalikan tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan wajib diisi. |
| `AT-PP-30` | Pengusul (dengan izin kotak masuk) menyetujui PR sendiri → ditolak Wfl; PR tetap PENDING_APPROVAL | `PASS` | http=404; message=Approval assignment tidak ditemukan pada inbox user login. |
| `AT-PP-31` | Semua jenjang menyetujui → PR APPROVED, riwayat WF_APPROVED sekali | `PASS` | approvals=[['PBJ-PR-01', 'headA', 200], ['PBJ-PR-02', 'mgr', 200], ['PBJ-PR-03', 'mgr', 200], ['PBJ-PR-04', 'dirk', 200]]; status=APPROVED |
| `AT-PP-32` | Penyetuju menolak → REJECTED; penyetuju minta revisi → RETURNED | `PASS` |  |
| `AT-PP-33` | Matriks diubah sesudah disetujui → jejak approval tetap 4 jenjang, nominal konteks tetap 45jt | `PASS` | steps=['PBJ-PR-01', 'PBJ-PR-02', 'PBJ-PR-03', 'PBJ-PR-04']; amount=45000000.0 |
| `AT-PP-34` | Sinkron gagal (penangan tidak menemukan PR) → keputusan Wfl sah (Completed), PR tetap PENDING; sinkron ulang → APPROVED, WF_APPROVED sekali | `PASS` | approvals=[['PBJ-PR-02', 'mgr', 200]]; afterFailure=PENDING_APPROVAL; wfl=Completed; sync1=200; sync2=200; wfApproved=1; finalPr=APPROVED |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `F-01` | Antrean verifikasi hanya PR SUBMITTED, lintas unit (ReadAllUnits) | `PASS` | http=200; count=4 |
| `F-02` | Pengusul tanpa ProcurementRequestVerification : Read → 403 | `PASS` | http=403; code=None; message=Anda tidak memiliki akses ke menu atau fitur ini. |
| `F-03` | Flag canVerify salah untuk pengusul sendiri | `PASS` |  |
| `F-04` | Cek stok 5 dari 20 → kebutuhan beli 15, PurchaseTotal 45jt (server) | `PASS` | http=200 |
| `F-05` | Instance atas nama pengusul (bukan verifikator), kunci idempoten :1 | `PASS` | key=PBJ_PR:4164a87d97994fd9b239c54a2b76f196:1 |
| `F-06` | Selesaikan ulang dengan Idempotency-Key sama → hasil pertama, satu VERIFY_COMPLETE | `PASS` | http=200 |
| `F-07` | Tolak di verifikasi dengan alasan → REJECTED (final) | `PASS` | http=200 |
| `F-08` | Aksi verifikasi pada PR final → 409 PBJ-VAL-093 | `PASS` | http=409; code=PBJ-VAL-093; message=Status pengajuan tidak memungkinkan tindakan ini. |
| `F-09` | Dikembalikan Wfl → RETURNED; ajukan ulang menutup instance lama (Cancelled) dan mengosongkan cek stok; verifikasi ulang → instance baru :2 | `PASS` | oldInstance=Cancelled; newKey=PBJ_PR:9e5a8601510741209d4b80cfc756e2cc:2; message=Verifikasi berhasil diselesaikan. |
| `F-11` | Batalkan PR APPROVED tanpa alasan → 422 PBJ-VAL-030 | `PASS` | http=422; code=PBJ-VAL-030; message=Alasan wajib diisi. |
| `F-12` | Batalkan PR APPROVED (belum masuk paket) → CANCELLED | `PASS` | http=200 |
| `F-13` | PR sudah masuk paket (disimulasikan lewat DB) → 409 PBJ-VAL-092 | `PASS` | http=409; code=PBJ-VAL-092; message=Pengajuan sudah masuk paket dan tidak dapat dibatalkan dari sini. |
| `F-14` | Daftar cek stok per baris tersaring status | `PASS` | http=200; rows=1 |

29 skenario, 29 `PASS`.

`AT-PP-34`: "penangan dibuat gagal sementara" disimulasikan dengan menyembunyikan PR (`IsDelete = true`) saat langkah
terakhir disetujui, lalu dipulihkan dan disinkronkan ulang lewat endpoint baru.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PP-20`..`28` | Terpenuhi | Tabel di atas |
| `AT-PP-30`..`34` | Terpenuhi | Tabel di atas |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Menolak di kotak masuk mewajibkan `RejectionReasonId`; dev memakai alasan uji `UJI-TOLAK-PBJ_PR` |
| Masalah yang diketahui | `CONVERTED_TO_PACKAGE` baru dapat ditulis `pemilihan-penyedia` (`MarkConvertedAsync` belum dibangun, sesuai roadmap) |
| Risiko tersisa | Matriks approval nyata menunggu SOP `PBJ-OQ-019` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. 17 berkas berubah dan 11 jalur baru di backend (lihat `git status --short` pada laporan `BE-PBJ-011`). |
| Langkah berikutnya | `FE-PBJ-005` |

## Tambahan `PBJ-DEC-040` — 16 November 2026 (malam)

Dikerjakan atas izin penuh pengguna untuk menutup temuan uji layar `FE-PBJ-003`/`004`/`005`.

| Perubahan | Berkas | Alasan |
| --- | --- | --- |
| `GET /{id}/approval` pada `purchase-requests`, `need-plans`, `budgets`, `package-plans` (izin `Read` dokumen masing-masing) | `Common/DTOs/ProcurementApprovalTimelineDtos.cs`, `Common/Services/ProcurementApprovalTimelineService.cs`, empat controller dokumen, registrasi DI | `GET workflow-instances/{id}` tidak membatasi data per peserta, sehingga `WorkflowInstance : Read` tidak boleh diberikan ke pengusul. Cakupan memakai `IProcurementDocumentAccessProvider` yang sama dengan lampiran; instance dibaca lewat `WorkflowService.GetByIdAsync` |
| `PurchaseRequestListItem` + `urgencyDeadline`, `lastActionCode`, `lastActionAt`, `lastActorName` | `Request/DTOs/PurchaseRequestDtos.cs`, `ProcurementPurchaseRequestService.cs` | Kolom "Batas Waktu" (`-14`) dan "Aksi Terakhir" (`-15`) di `03-frontend-architecture.md` §4.6 |
| `sortBy=urgency` (mendesak lebih dulu, lalu terlama diajukan); bawaan antrean verifikasi | `ProcurementPurchaseRequestService.cs`, `ProcurementPurchaseRequestVerificationService.cs` | "Mendesak di atas" pada `-11` |

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release` | 0 error, 601 warning (baseline) | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (10 berkas) | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` |
| Runtime `phase_i.py` (12 skenario): pengusul membaca 4 langkah PR-000014 tanpa izin `Wfl`; unit lain 404; PR tanpa instance → `data: null`; pengusul tetap 403 di `workflow-instances`; rencana, pagu, paket 200; pengusul tanpa izin pagu 403; daftar memuat aksi terakhir; metadata memuat `urgency` | 12/12 | `PASS` |
| Antrean verifikasi dengan PR mendesak `PR-000017` | `PR-000017` (URGENT, batas 30 Nov 2026) di urutan pertama di depan dua PR biasa | `PASS` |

Catatan: karena `PurchaseRequestQuery.SortBy` bernilai bawaan `createdAt`, antrean memperlakukan `createdAt` sebagai
"tidak memilih" dan memakai `urgency`; urutan lain (`neededByDate`, `requestNumber`, `purchaseTotal`) tetap dihormati.
