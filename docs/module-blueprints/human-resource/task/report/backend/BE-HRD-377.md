# Laporan Perubahan Backend — `BE-HRD-377`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-377` |
| Judul | Batal SPPD menarik workflow dan melepas gembok roster |
| Slice | `S-D5` amandemen / `MVP-70` — gelombang 4 · `EPIC HRD-121` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-377` |
| Trace | `FR-TRV-30`; `HRD-DEC-225`; API 34.1 (`PUT /{id}/cancel`), 34.5; Validation `VR-TRV-33`; State 32.2; Integration 25.2 (pelepasan); AT `AT-HRD-TRV-29` |
| Contract version | `v28` — approved 19 November 2026; tidak diubah |
| Dependency | `BE-HRD-376` ✅ |
| Klasifikasi | `MEDIUM` — satu repository, 2 berkas source diubah, perubahan perilaku endpoint yang ada, integrasi Workflow V2 dan presensi/roster |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "lanjutkan") |
| Target tulis | `Services/BusinessTravelRequestService.cs` (`CancelRequestAsync`, konstruktor), `Controllers/BusinessTravelRequestsController.cs` (`CancelRequest`); uji di `TravelWorkflowLifecycleTests.cs` (5 baru), penyesuaian konstruktor di `TravelSubmitTests.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `43f66daf` (branch `yoga`); `BE-HRD-376` belum di-commit |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terbukti: harness 5 uji baru (37/37 uji perjalanan dinas, kontrol negatif 5 mutasi tertangkap), proyek uji HR 364/364, QBE Strict tanpa temuan, dan API devYoga dengan akun asli Dewi (pemohon) dan dr. Bima (penyetuju) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Tanpa entity baru; menutup instance Workflow V2 (`Wfl`) lewat aksi resminya (`WithdrawAsync`, `CancelAsync`); `QBE-MOD-002` tidak berlaku |
| Keberlakuan | `TOUCHED LEGACY` (`CancelRequestAsync`, action `CancelRequest`) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001` (`200/400/404/409`), `QBE-VAL-001` (`VR-TRV-20`, `VR-TRV-33`), `QBE-TXN-001` (workflow ditutup lebih dulu; bila gagal SPPD tidak berubah; pembatalan + pelepasan gembok satu `SaveChanges`), `QBE-LOG-001` (`CancelledByUserId`, `UpdateBy` tautan presensi = pelaku), `QBE-DEL-001` (pembatalan beserta aktor) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-PERM-001` (atribut tidak berubah), `QBE-DB-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Kondisi as-is terhadap kontrak

| Hal | As-is | Kontrak `v28` |
| --- | --- | --- |
| Batal SPPD menunggu | Status langsung `Cancelled`; workflow tetap `InProgress` dan tugas atasan tetap terbuka | Workflow ditarik lebih dulu; bila ditolak `409` dan SPPD tetap `Submitted` (`VR-TRV-33`) |
| Batal SPPD disetujui | Gembok dan presensi Dinas Luar tidak dilepas | `ReleaseRosterLockAsync` di transaksi pembatalan (25.2) |
| Galat | Semua `400` | `409` untuk penarikan ditolak; `404` tidak ditemukan |

**Fakta Workflow V2:** `WithdrawAsync` hanya dari `InProgress`; `CancelAsync` dari `Draft`/`Submitted`/`RevisionRequested`/`Returned`; keduanya hanya oleh pemohon instance dan menurut izin definisi (`AllowRequesterWithdraw`/`AllowRequesterCancel`, keduanya `true` pada definisi `BUSINESS_TRAVEL_REQUEST`). Sesudah workflow disetujui final engine menyimpan `Completed`.

**Delta dicatat (tidak mengubah kontrak):**

| Delta | Alasan |
| --- | --- |
| SPPD `NeedRevision` yang dibatalkan juga menutup workflow-nya (`CancelAsync`), padahal State 32.2 hanya menyebut `CancelledAt` | Tanpa itu instance `RevisionRequested` tertinggal terbuka; sejalan dengan `HRD-DEC-225` ("Dibatalkan = workflow ditarik") |
| SPPD `Rejected` tidak dapat dibatalkan (`400`, pesan diperluas "sudah selesai, ditolak, atau dibatalkan") | State 32.1 menandai `Rejected` final; as-is mengizinkannya |
| Pesan `400` untuk alasan < 15 karakter berupa `ProblemDetails` dari atribut DTO `MinLength(15)` | Perilaku lama, tidak berubah |

---

## 2. Perubahan yang dikerjakan

| Berkas | Isi |
| --- | --- |
| `Services/BusinessTravelRequestService.cs` | Konstruktor menerima `ITravelAttendanceAndRosterLockService` (DI sudah ada). `CancelRequestAsync(…, CancellationToken)`: tolak `Completed`/`Cancelled`/`Rejected`; alasan ≥ 15 karakter (`VR-TRV-20`); bila SPPD berinstance: workflow `InProgress` → `WithdrawAsync`, `Draft`/`Submitted`/`RevisionRequested`/`Returned` → `CancelAsync`, alasan pembatalan diteruskan; gagal → bersihkan change tracker, `409` dengan pesan engine; berhasil → muat ulang SPPD (penangan `BE-HRD-376` sudah dapat menyetelnya). Status `Cancelled`, alasan, `CancelledAt`/`ByUserId` (tidak menimpa nilai penangan). SPPD yang pernah disetujui → `ReleaseRosterLockAsync(id, pelaku)` yang menyimpan pembatalan dan pelepasan bersama |
| `Controllers/BusinessTravelRequestsController.cs` | `CancelRequest`: `BusinessTravelRequestException` → kode status, `KeyNotFoundException` → `404`; `[ProducesResponseType]` 200/400/404/409; `CancellationToken`; `<summary>` XML dibuang |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat | `PASS` | Keluaran build |
| `dotnet test --filter TravelWorkflowLifecycleTests\|TravelSubmitTests` | 37/37 lulus (5 baru) | `PASS` | Keluaran `dotnet test` |
| — Batal saat menunggu | Workflow `Withdrawn`, tidak ada tugas terbuka; SPPD `Cancelled`, alasan, `CancelledByUserId` = Sari | `PASS` (AT-29) | idem |
| — Batal oleh Bima (bukan pemohon) | `409` "Hanya pemohon yang dapat menarik workflow."; SPPD tetap `Submitted` tanpa alasan; workflow tetap `InProgress` | `PASS` (VR-TRV-33) | idem |
| — Batal saat `NeedRevision` | Workflow `Cancelled`; SPPD `Cancelled` | `PASS` | idem |
| — Batal sesudah disetujui | Gembok mati; tautan presensi `Cancelled` dengan `UpdateBy` = Sari; `ApprovedAt` tetap; workflow `Completed` tidak disentuh | `PASS` (25.2) | idem |
| — Draf; alasan pendek; SPPD ditolak | Draf → `Cancelled`; alasan pendek ditolak (Draf tetap); `Rejected` ditolak, tetap `Rejected` | `PASS` | idem |
| Catatan proses | Satu asersi awal mengharapkan workflow `Approved` sesudah persetujuan; engine menyimpan `Completed` — asersi dibetulkan (kode tidak berubah) | Catatan | — |
| Kontrol negatif (5 mutasi; berkas dipulihkan, `cmp` identik) | C1 cabang tarik, C2 penanganan galat engine, C3 batal saat dikembalikan, C4 pelepasan gembok, C5 `Rejected` final — semuanya tertangkap | `PASS` | Keluaran per putaran |
| QBE `ReportOnly` dengan probe; `Strict` dua berkas | Probe 2 `REVIEW` + 1 `VIOLATION`; berkas asli 0 temuan | `PASS` | Keluaran checker |
| Proyek uji HR penuh | 364/364 lulus (359 + 5 baru) | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2563 …"; worker penyelarasan berjalan | `PASS` | `be377.log` |
| devYoga (Dewi pemohon, Bima penyetuju; hak sementara Dewi `BusinessTravelRequest` R/C/U/Cancel, Bima `WorkflowInstance` Read/Approve/RequestRevision) | — | — | `at-377.out` |
| — SPPD `0005` menunggu, dibatalkan superadmin | `409` "Hanya pemohon yang dapat menarik workflow."; SPPD tetap `Submitted`, workflow `InProgress` | `PASS` (VR-TRV-33) | idem |
| — SPPD `0005` dibatalkan Dewi | `200` "Perjalanan dinas berhasil dibatalkan"; `Cancelled`, alasan, `CancelledByUserId` = Dewi; workflow `Withdrawn`; 0 tugas terbuka | `PASS` (AT-29) | idem |
| — SPPD `0006` dikembalikan Bima lalu dibatalkan Dewi | `200`; SPPD `Cancelled`; workflow `Cancelled` | `PASS` | idem |
| — SPPD `0007` disetujui Bima (presensi 21–22 Desember `Synced`, gembok) lalu dibatalkan Dewi | `200`; gembok mati; presensi `Cancelled` diubah oleh Dewi; workflow `Completed` | `PASS` (25.2) | idem |
| — Alasan "batal saja"; batal SPPD `0004` (ditolak) | `400` (atribut DTO; SPPD `0008` tetap Draf); `400` "Perjalanan dinas yang sudah selesai, ditolak, atau dibatalkan tidak dapat diubah" | `PASS` | idem |
| — SPPD `0003` | Tidak disentuh: tetap `Approved` dengan gembok (untuk `BE-HRD-378`) | `PASS` | idem |
| — Pemulihan hak | `identik: true` — Dewi 11, Bima 45 | `PASS` | idem |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Batal SPPD menunggu → workflow `Withdrawn`/`Cancelled`, SPPD `Cancelled`, tugas hilang | Terpenuhi | Harness + devYoga `0005` |
| 2. Penarikan ditolak engine → `409` pesan engine, SPPD tetap `Submitted` | Terpenuhi | Harness + devYoga (superadmin) |
| 3. Batal SPPD disetujui → gembok dan presensi Dinas Luar dilepas | Terpenuhi | Harness + devYoga `0007` |
| 4. Batal `Draft`/`NeedRevision` tetap aturan lama (alasan ≥ 15) | Terpenuhi (NeedRevision juga menutup workflow — delta 1) | Harness + devYoga `0006`, `0008` |
| Migration | Tidak ada | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Data uji yang tertinggal di devYoga | SPPD `0005`, `0006`, `0007` Dibatalkan; `0008` Draf "UJI-377 Alasan pendek" — disimpan sesuai pilihan pengguna |
| Risiko | Pembatalan oleh petugas selain pemohon pada SPPD berworkflow selalu `409` (aturan engine); untuk SPPD tanpa workflow aktif, pembatalan tetap mengikuti hak `BusinessTravelRequest : Cancel` tanpa pemeriksaan pemohon (perilaku lama, PRD 41.7 menyebut "SPPD sendiri" — tidak menjadi AC task ini) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Task berikutnya | `BE-HRD-378` (gerbang uang muka/settlement) — SPPD `0003` Disetujui siap dipakai |
