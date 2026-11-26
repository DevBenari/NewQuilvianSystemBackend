# Laporan Perubahan Backend — `BE-HRD-376`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-376` |
| Judul | Penangan workflow SPPD dan sinkron presensi Dinas Luar serta gembok roster (`INT-HR-109`, `INT-HR-110`) |
| Slice | `S-D5` amandemen / `MVP-70` — gelombang 3 · `EPIC HRD-121` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 27, kartu `BE-HRD-376` |
| Trace | `FR-TRV-28`, `FR-TRV-29`; `HRD-DEC-225`, `HRD-DEC-226`; `HRD-TF-077`; Integration 25.1–25.2; State 32.2–32.3; `VR-TRV-32`; AT `AT-HRD-TRV-26`, `27`, `28`, `32`, `33` |
| Contract version | `v28` — approved 19 November 2026; **delta integration 25.1 disetujui pengguna** (lihat 1.1), penyegaran dokumen untuk pass blueprint |
| Dependency | `BE-HRD-375` 🟡 (source dan harness selesai; bukti devYoga menunggu sandi) |
| Klasifikasi | `MEDIUM` — satu repository, 5 berkas source (2 baru), integrasi lintas modul (Workflow V2 → SPPD → presensi/roster), worker baru |
| Task mode | `BACKEND` (instruksi pengguna 19 November 2026: "pilih (a), lanjut BE-HRD-376") |
| Target tulis | `Services/BusinessTravelWorkflowLifecycleHandler.cs` (baru), `Services/BusinessTravelWorkflowReconciliationService.cs` (baru), `Services/TravelAttendanceAndRosterLockService.cs`, `Workers/BusinessTravelWorkers.cs`, `Program.cs`; uji baru `TravelWorkflowLifecycleTests.cs`, `TravelSubmitTests.cs` disesuaikan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `43f66daf` (branch `yoga`; commit pengguna berisi `BE-HRD-374`/`375`) |
| Tanggal | 19 November 2026 |
| Status | ✅ Selesai (19 November 2026) — 7/7 acceptance criteria terbukti (AC6 bagian kedua diganti keputusan pengguna (a)): harness 12 uji penangan + 20 uji Ajukan dengan penangan terdaftar (Workflow V2 nyata; kontrol negatif 12 mutasi tertangkap), proyek uji HR 359/359, QBE Strict tanpa temuan, dan API devYoga dengan akun asli Dewi (pemohon) dan dr. Bima (penyetuju) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `BusinessTravelManagement` |
| Pemilik / prefix registry | Tanpa entity baru; menulis kolom `TrxBusinessTravelRequest`/`TrxTravelAttendanceLink` yang ada dan membaca `TrxWorkflowInstance`/`TrxApprovalAction` milik Workflow V2 (`Wfl`, `SHARED PLATFORM CAPABILITY`) lewat titik perluasan resminya `IWorkflowReferenceLifecycleHandler`; `QBE-MOD-002` tidak berlaku |
| Keberlakuan | `NEW CODE` (penangan, penyelarasan ulang, worker); `TOUCHED LEGACY` (`TravelAttendanceAndRosterLockService`, `TravelRosterLockExpiryWorker`, `Program.cs`) |
| QBE yang berlaku | `QBE-MOD-001` (penangan di modul pemilik SPPD, bukan di Workflow V2), `QBE-SVC-001`, `QBE-LOG-001` (pelaku nyata menggantikan `Guid.Empty`; log `[TRV-WF]` per perpindahan status), `QBE-TXN-001` (lihat 1.1), `QBE-ENUM-001` (peta status di modul), `QBE-AUD-001` |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-API-*`, `QBE-PERM-001`, `QBE-DTO-001` (tanpa endpoint), `QBE-DB-*` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill |

---

## 1. Kondisi as-is terhadap kontrak

| Hal | As-is | Kontrak `v28` |
| --- | --- | --- |
| Penangan SPPD | Tidak ada; Workflow V2 menjawab `IsHandled = false` untuk `HRD_TRV_REQUEST` | `BusinessTravelWorkflowLifecycleHandler` (Integration 25.1) |
| Presensi/gembok | `SyncAttendanceAndLockRosterAsync` tidak pernah dipanggil; `ReleaseRosterLockAsync` hanya worker; pelaku `Guid.Empty` (`HRD-TF-077`) | Dipanggil saat `Approved` dengan pelaku nyata (25.2) |
| Pemanggilan penangan | `WorkflowService` memanggil `WorkflowReferenceLifecycleService.HandleAsync` **sesudah** `transaction.CommitAsync` dan menelan galat (`WorkflowService.cs` baris 728, 972; ActionsV2 299, 498, 733, 916, 1066) | 25.1: "ikut transaksi; galat penangan menggagalkan aksi" |

### 1.1 Delta yang disetujui pengguna

| Delta | Keputusan | Akibat pada implementasi |
| --- | --- | --- |
| Penangan berjalan sesudah commit, galat tidak menggagalkan aksi workflow | **Pilihan (a)**, pengguna 19 November 2026: Workflow V2 tidak diubah; kontrak 25.1 disesuaikan dengan perilaku engine pada pass blueprint | Ditambah `BusinessTravelWorkflowReconciliationService` + `TravelWorkflowReconciliationWorker` (15 menit) yang menyelaraskan ulang SPPD tertinggal — menggantikan "rekonsiliasi manual" pada 25.1 tanpa endpoint baru |
| Instance `Draft` tidak memindahkan SPPD (32.3 memetakan `Draft` → `Submitted`) | Konsekuensi teknis pilihan (a): bila proses terputus di antara pembuatan dan pengiriman workflow, penyelarasan ulang tidak boleh menjadikan SPPD "menunggu" padahal belum ada tugas atasan. SPPD menjadi `Submitted` oleh Ajukan (`BE-HRD-375`) atau saat workflow `Submitted`/`InProgress` | Dicatat untuk penyegaran State 32.3 pada pass blueprint |

---

## 2. Perubahan yang dikerjakan

| Berkas | Status | Isi |
| --- | --- | --- |
| `Services/BusinessTravelWorkflowLifecycleHandler.cs` | Baru | `CanHandle` = `HRD_TRV_REQUEST` (murni). `SynchronizeAsync`: SPPD tidak ada → `IsHandled = false` + log peringatan; instance bukan instance SPPD saat ini → diabaikan; status target dari `BusinessTravelWorkflowStatusMap`; tidak berubah bila sama, bila workflow `Draft`, atau bila SPPD di luar fase persetujuan (`Draft`/`Submitted`/`NeedRevision`) — status lanjutan dan final tidak pernah diturunkan. `Approved`: `ApprovedAt` (waktu selesai workflow), `ApprovedByUserId` = pelaku aksi `Approve`/`AutoApprove` terakhir, `ApprovedBudgetAmount = EstimatedTotalAmount` (`HRD-DEC-226`), lalu presensi + gembok dengan pelaku penyetuju. `Rejected`: `RejectedAt`/`RejectedByUserId` dari aksi `Reject` terakhir. `Cancelled`: `CancelledAt`/`ByUserId`. `NeedRevision`/`Submitted`: status + tahap. Log `[TRV-WF] SPPD {nomor}: {lama} → {baru}` |
| `Services/BusinessTravelWorkflowReconciliationService.cs` | Baru | Mencari SPPD fase persetujuan berinstance yang status workflow-nya memetakan ke status lain, lalu menjalankan penangan yang sama; galat per SPPD dicatat dan tidak menghentikan putaran |
| `Services/TravelAttendanceAndRosterLockService.cs` | Diubah | Parameter `actorUserId` pada `SyncAttendanceAndLockRosterAsync` dan `ReleaseRosterLockAsync` mengisi `CreateBy`/`UpdateBy` (sebelumnya `Guid.Empty`) |
| `Workers/BusinessTravelWorkers.cs` | Diubah | `TravelRosterLockExpiryWorker` memakai `SystemFallbackUserId`; **baru** `TravelWorkflowReconciliationWorker` (15 menit) |
| `Program.cs` | Diubah | Penangan terdaftar sebagai `IWorkflowReferenceLifecycleHandler` (pola Pengadaan), layanan penyelarasan, dan worker |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | Berhasil, 0 galat | `PASS` | Keluaran build |
| `dotnet test --filter TravelWorkflowLifecycleTests\|TravelSubmitTests` (Workflow V2 nyata, penangan terdaftar seperti produksi) | 32/32 lulus | `PASS` | Keluaran `dotnet test` |
| — Bima menyetujui (`ApproveAsync`) | SPPD `Approved`, `ApprovedByUserId` = Bima, anggaran 5.000.000, presensi `Synced` 1/2/3 Desember dibuat oleh Bima, `HasScheduleConflict`, detail `IsApproved` | `PASS` (AT-26) | idem |
| — Bima mengembalikan (`RequestRevisionAsync`) | `NeedRevision` tanpa penyetelan manual; ajukan ulang instance sama → `Submitted` → disetujui → 3 presensi | `PASS` (AT-27) | idem |
| — Bima menolak (`RejectAsync`, alasan `CloseRequest`) | `Rejected`, `RejectedByUserId` = Bima, tanpa presensi/gembok, anggaran 0; ajukan ulang `409` | `PASS` (AT-28) | idem |
| — Sari menarik (`WithdrawAsync`) | `Cancelled`, `CancelledAt` | `PASS` | idem |
| — Tanpa presensi/gembok sebelum disetujui | Sesudah Ajukan, dikembalikan, ditolak, ditarik: 0 tautan, gembok mati | `PASS` (AT-32) | idem |
| — Penyelarasan dua kali sesudah disetujui; penyelarasan saat menunggu | Tidak ada perubahan; `ApprovedAt`, tautan presensi beserta `SyncedAt`, `UpdateDateTime` tetap | `PASS` (AT-33) | idem |
| — Status lanjutan, instance lama, referensi hilang | `AdvancePaid` tidak turun; instance lama `Withdrawn` tidak membatalkan SPPD menunggu; referensi hilang `IsHandled = false` | `PASS` | idem |
| — Penangan tidak terpanggil saat disetujui (komposisi tanpa penangan) | SPPD tertinggal `Submitted`; penyelarasan ulang → `Approved` dengan penyetuju Bima dan 3 presensi; putaran kedua 0 | `PASS` (delta 1.1) | idem |
| — Proses terputus sesudah `CreateAsync` | Penyelarasan ulang 0, SPPD tetap Draf; Ajukan memakai ulang instance Draf | `PASS` (delta 1.1) | idem |
| — Pelepasan gembok | `UpdateBy` SPPD dan tautan = pelaku | `PASS` | idem |
| Kontrol negatif (12 mutasi; berkas dipulihkan dan dibandingkan byte demi byte) | H1 anggaran, H2 presensi, H3 penjaga fase, H4 penjaga instance lama, H5 penjaga status sama, H6 peta dikembalikan, H7 penolak, H8 penyetuju dari aksi, H9 penyaring penyelarasan, H10 pelaku presensi, H11 `CanHandle`, H12 instance Draf — **semuanya tertangkap** (H4/H5/H12 sesudah tiga uji pengisolasi ditambahkan; putaran pertama lolos karena penjaga lain menutupi) | `PASS` | Keluaran per putaran |
| QBE `ReportOnly` dengan probe | Probe: 2 `REVIEW` `QBE-SVC-001` + 1 `VIOLATION` `QBE-CODE-003`; berkas `375`/`376` tanpa temuan | `PASS` (kontrol) | Keluaran checker |
| QBE `Strict` atas lima berkas task ini | 0 temuan | `PASS` | Keluaran checker |
| Proyek uji HR penuh (`dotnet test -p:RunAnalyzers=False`, termasuk build aplikasi) | 359/359 lulus (347 + 12 baru) | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2563 identitas kanonik dari 2563 kemampuan terdaftar."; "TravelWorkflowReconciliationWorker dimulai."; tanpa galat DI. Backend dimatikan kembali | `PASS` | `be376.log` |
| devYoga putaran 1 (penyetuju Cahyo) | Login Cahyo `401` "Email atau password salah." (akun aktif, tidak terkunci); langkah persetujuan tidak terjadi. Alasan penolakan uji dibuat lewat API master (kode tersimpan dinormalisasi menjadi `UJI_376_TOLAK`, `CloseRequest`); SPPD `0002` dibuat & diajukan; hak sementara dikembalikan identik | Catatan | `at-376.out` |
| devYoga putaran 2 (pilihan pengguna: penyetuju dr. Bima) — penyiapan | Cek login kedua akun dulu (`200`/`200`); atasan Dewi → Cahyo dinonaktifkan (`PATCH …/status` `200`), Dewi → Bima dipasang (`200`), atasan aktif tunggal = Bima; hak sementara Dewi (+ `WorkflowInstance` Read/Withdraw) dan Bima (`ApprovalInbox` Read, `WorkflowInstance` Read/Approve/Reject/RequestRevision) | Catatan | `at-376b.out` |
| — Tarik dua SPPD lama (`0001`, `0002`) oleh Dewi | `200` "Workflow berhasil ditarik oleh pemohon."; workflow `Withdrawn`; SPPD `Cancelled` dengan `CancelledAt` — lewat penangan | `PASS` (State 32.3) | idem |
| — SPPD `0003` diajukan | `201`/`200`; tugas `Available` untuk dr. Bima; kotak masuk Bima memuat baris `HRD_TRV_REQUEST` / `BUSINESS_TRAVEL_REQUEST` | `PASS` | idem |
| — Bima mengembalikan (AT-27) | `200`; SPPD `NeedRevision`; Dewi mengubah hotel (`200`); ajukan ulang `200`, instance sama, `Submitted`; presensi masih 0 | `PASS` | idem |
| — Bima menyetujui (AT-26, VR-TRV-32) | `200`; SPPD `Approved`, `ApprovedByUserId` = Bima, anggaran = estimasi baru 4.700.000, `HasScheduleConflict`; presensi `Synced` 1/2/3 Desember dibuat oleh Bima; detail Dewi `isApproved = true`, `canSubmit = false` | `PASS` | idem |
| — SPPD `0004` ditolak (putaran 2) | `400` "RejectionReasonId wajib diisi." — galat skrip (pencarian kode `UJI-376-TOLAK`, tersimpan `UJI_376_TOLAK`) | Catatan (galat skrip) | idem |
| — SPPD `0004` ditolak (putaran 3, AT-28 / VR-TRV-28) | `200` "Workflow berhasil ditolak."; workflow `Rejected`; SPPD `Rejected`, `RejectedByUserId` = Bima, anggaran 0, tanpa presensi/gembok; ajukan ulang `409` "SPPD yang ditolak tidak dapat diajukan ulang. Buat SPPD baru." | `PASS` | `at-376c.out` |
| — Pemulihan hak (setiap putaran) | `identik: true` — Dewi 11 izin, Bima 45 izin, Cahyo 8 izin | `PASS` | idem |
| Catatan perilaku | Sesudah workflow selesai/ditarik engine mengosongkan tahap (`CurrentStepOrder = 0`), sehingga `CurrentApprovalStep` SPPD menjadi 0 | Catatan | idem |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Persetujuan final → `Approved`, kolom persetujuan, satu tautan presensi per hari dinas, gembok | Terpenuhi (harness) | AT-26 + H1/H2/H8/H10 |
| 2. Dikembalikan → `NeedRevision`, diajukan ulang instance sama | Terpenuhi (harness) | AT-27 + H6 |
| 3. Ditolak → `Rejected`, `RejectedAt` | Terpenuhi (harness) | AT-28 + H7 |
| 4. Tidak ada presensi/gembok sebelum `Approved` | Terpenuhi (harness) | AT-32 |
| 5. Penyelarasan dua kali tidak mengubah apa pun; status lanjutan tidak diturunkan | Terpenuhi (harness) | AT-33 + H3/H5 |
| 6. Referensi tidak ditemukan → `IsHandled = false` tanpa galat; ~~galat penangan menggagalkan aksi workflow~~ | Bagian pertama terpenuhi; bagian kedua **diganti** keputusan (a): galat penangan dicatat engine dan SPPD diselaraskan ulang oleh worker | Uji referensi hilang + penyelarasan ulang + H9 |
| 7. `CreateBy`/`UpdateBy` tautan presensi = pelaku persetujuan | Terpenuhi (harness) | H10 |
| Migration | Tidak ada | — |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan — alasan penolakan | Workflow V2 mewajibkan `RejectionReasonId` yang cocok dengan `RequestType` `HRD_TRV_REQUEST`; devYoga belum punya satu pun alasan penolakan (untuk jenis apa pun). Agar "Tolak" berarti **final** (`HRD-DEC-225`), alasan harus ber-`RejectAction = CloseRequest`; `ReturnToRequester` (bawaan) berperilaku "dikembalikan", `CancelRequest` berperilaku "dibatalkan". Konfigurasi Admin HR di Master Alasan Penolakan (API sudah ada) |
| Penyegaran blueprint | Integration 25.1 (penangan sesudah commit + penyelarasan berkala) dan State 32.3 (instance `Draft` tidak memindahkan SPPD) |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy |
| Data uji yang tertinggal di devYoga | Atasan Dewi → Bima (aktif; Dewi → Cahyo nonaktif); alasan penolakan `UJI_376_TOLAK`; SPPD `0001`/`0002` Dibatalkan, `0003` **Disetujui** dengan presensi 1–3 Desember dan gembok (dipakai `BE-HRD-377`/`378`), `0004` Ditolak — disimpan sesuai pilihan pengguna |
| Task berikutnya | `BE-HRD-377` (batal menarik workflow, melepas gembok) dan `BE-HRD-378` (gerbang uang muka/settlement) |
