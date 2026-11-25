# Laporan Perubahan Backend — `BE-HRD-443`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-443` |
| Judul | Antrean, ringkasan, dan `myAction` Kotak Masuk Terpadu dari penugasan engine |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 3 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-443` |
| Trace | `FR-WF-26`, `FR-WF-31` (baca); `HRD-DEC-229`, `HRD-DEC-234` |
| Contract version | `v29` — API 35.1, 35.5 (penilaian kelayakan), 35.6; Integration `INT-HR-111`, `INT-HR-113` — approved 20 November 2026 |
| Dependency | `BE-HRD-441` ✅ (penentu delegasi aktif) |
| Klasifikasi | `STANDARD` — service baca, helper bersama, controller, DTO; tanpa perubahan skema |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: "lanjut") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `815ce085` (branch `yoga`) + working tree `BE-HRD-440`–`442`, `447` dan task ini |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `ApprovalInboxQueryService` (ditulis ulang), `ApprovalInboxService` (kueri alasan dipindah ke helper), `WorkflowService` (satu method `private` → `internal`), controller, DTO V2; `NEW CODE` — `WorkflowRejectionReasonOptions` (helper statis tanpa entity) |
| QBE yang berlaku | `QBE-SVC-001` (aturan di service; controller hanya meneruskan akun dan mengisi `myAction`), `QBE-API-001` (`ApiResponse`, kode status tetap), `QBE-PERM-001` (atribut akses tidak berubah). Tanpa entity/tabel baru, sehingga `QBE-NAM-001`, `QBE-CFG-001`, `QBE-MOD-002` tidak tersentuh. Tanpa `<summary>` XML baru pada action controller |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `API_RULES.md` (suite Skill) |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (tidak diperlukan); eksekusi database ✖; deployment ✖ |

---

## 1. Cara kerja sesudah perubahan

Contoh: Sari (perawat ICU) mengajukan SPPD `SPPD/RSQ/2026/11/0013`. Engine `WorkflowService` membuat tugas persetujuan untuk akun dr. Bima (Kepala Ruang).

| Langkah | Yang terjadi |
| --- | --- |
| dr. Bima membuka `GET /api/v1/hrd/workflow/inbox` | Server membaca tugas engine yang terbuka pada tahap aktif milik **akun** dr. Bima. Butir SPPD tampil dengan `assignmentId`, `referenceNumber = "SPPD/RSQ/2026/11/0013"`, `summaryText = "SPPD/RSQ/2026/11/0013 — Persetujuan Perjalanan Dinas"`, nama pemohon "Sari", dan `canBulkApprove` |
| dr. Bima mendaftarkan Wakil Karu sebagai Plh (delegasi `Active`, rentang berlaku) | Kotak Wakil Karu ikut memuat tugas SPPD tersebut dengan `isDelegated = true` dan `delegatedFromEmployeeName = "dr. Bima"`. Kotak dr. Bima tetap memuatnya tanpa tanda Plh |
| Wakil Karu membuka `GET /instances/{id}` | Respons membawa `myAction`: `assignmentId` tugas dr. Bima, `isDelegated = true`, `onBehalfOfName = "dr. Bima"`, `availableActions = ["Approve", "Reject", "RequestRevision"]`, dan `rejectionReasons` yang sama dengan kotak masuk lama |
| Orang lain (atau Sari sendiri) membuka detail yang sama | `myAction = null` |
| dr. Bima memutuskan SPPD | Tugas hilang dari antrean dan ringkasan; `myAction` menjadi `null` |

Tugas engine V2 lama (penugasan yang mengisi `ApproverEmployeeId`) tidak lagi ditampilkan di kotak terpadu (`HRD-DEC-229`).

---

## 2. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/ApprovalInboxQueryService.cs` | Ditulis ulang. Antrean dan ringkasan dipilih per **akun** (`userId`), dari penugasan dengan status terbuka (`Pending`, `Available`, `InProgress`, `Delegated`), `IsCurrentAssignment`, `ApproverEmployeeId` kosong, instance `InProgress`, dan tahap aktif `InProgress`. Pemegang penugasan = pengguna, **atau** pejabat yang sedang mendelegasikan kepadanya (`WorkflowDelegationResolver.WhereActiveAt` + cakupan definisi/tahap yang sama dengan engine). Baris Plh atas permohonan pengguna sendiri dibuang (`VR-WF-02`). Satu baris per tahap, penugasan sendiri didahulukan. Filter `requestType` cocok persis tanpa membedakan huruf besar. Ruas baru `assignmentId`, `referenceNumber`, `canBulkApprove`. `slaDeadline` = `DueAt` penugasan (cadangan: SLA instance). Method baru `GetMyActionAsync`. Penilai kelayakan massal `ResolveBulkApprovalSkipReason` (teks alasan 35.5) disiapkan untuk dipakai `BE-HRD-445` |
| `…/Services/WorkflowRejectionReasonOptions.cs` (baru) | Kueri alasan penolakan aktif per jenis permohonan, definisi, dan tahap. Dipakai bersama detail kotak masuk lama dan `myAction`, sehingga keduanya identik |
| `…/Services/ApprovalInboxService.cs` | Detail penugasan memakai helper di atas. Perilaku tidak berubah |
| `…/Services/WorkflowService.cs` | `ResolveApprovalActionType` dari `private` menjadi `internal` (dipakai `myAction`) |
| `…/Controllers/WorkflowInboxAndInstancesController.cs` | `GET /inbox` dan `GET /inbox/summary` meneruskan akun lewat `User.GetUserId()` (klaim `user_id` lalu `NameIdentifier`). `GET /instances/{id}` mengisi `myAction` |
| `…/DTOs/WorkflowV2Dtos.cs` | `ApprovalInboxItemV2ResponseDto`: `AssignmentId`, `ReferenceNumber`, `CanBulkApprove`. `WorkflowInstanceDetailV2ResponseDto.MyAction`. Class baru `WorkflowMyActionDto` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowInboxEngineQueryTests.cs` (baru), `AcceptanceTestIndex.md` | 15 uji (12 fakta + 1 teori 4 kasus) dengan engine nyata; satu baris indeks |

### Delta terhadap kartu roadmap

| Hal | Kartu | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| Tempat pengisian `myAction` | `WorkflowInstanceCreationService.cs` (detail) | Controller memanggil `IApprovalInboxQueryService.GetMyActionAsync` sesudah detail dimuat | Detail tidak berubah dan tetap dipakai pihak lain tanpa konteks pengguna. Aturan "siapa boleh bertindak" tetap di satu service yang sama dengan antrean, sehingga antrean dan `myAction` tidak bisa saling berbeda |
| Parameter `status` pada `GET /inbox` | — | Diterima tetapi diabaikan | Kontrak §35.1 tidak lagi memuat filter status: antrean hanya berisi tugas terbuka. Parameter tetap ada agar pemanggil lama tidak menerima `400` |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowInboxEngineQueryTests` | 15/15 lulus | `PASS` (AC1–AC3) | Keluaran `dotnet test` |
| Kontrol negatif: 12 mutasi (engine lama ikut tampil; status selesai ikut tampil; tugas Plh tidak dimuat; Plh melihat permohonan sendiri; cakupan delegasi diabaikan; filter jenis "mengandung"; massal abaikan SLA; SLA dari instance saja; tanda Plh hanya dari pengalihan; alasan penolakan kosong; controller tanpa `myAction`; tanpa penjaga akun kosong) | 12/12 tertangkap; source dipulihkan | `PASS` | `mut443.js`. Putaran pertama 3 lolos/tidak terpasang. H1: instance engine lama di uji tanpa definisi, jadi sudah terbuang oleh join, sehingga uji diperketat dengan definisi nyata. H3: pola mutasi tidak unik. H12: tidak ada penugasan tanpa akun, sehingga uji ditambah penugasan rusak tanpa penyetuju. Sesudah perbaikan ketiganya tertangkap |
| Regresi penuh proyek uji HR | 404/404 lulus: 403 pada run penuh (12 menit 14 detik) + 1 uji skema `BE-HRD-440` yang diulang dengan env `QUILVIAN_BACKEND_ROOT` (uji migration mencari root repository; salinan scratchpad berada di luar repository). Dijalankan dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085` | `PASS` | Keluaran `dotnet test` |
| QBE `Strict` atas 6 berkas | 0 violation, 0 review; probe `TrxZzzQbeProbe443` terdeteksi 3 violation (`QBE-NAM-001`, `QBE-CFG-001`, `QBE-MOD-002`) lalu dihapus | `PASS` | Keluaran checker (7 berkas dinilai dengan probe, 6 tanpa probe) |
| Uji API lokal dengan akun dr. Bima (SPPD `0009`) | Tidak dijalankan | `NOT RUN` | Kartu menandainya "bila diizinkan"; butuh backend lokal dan sandi akun dari pengguna |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. SPPD dan cuti menunggu atasan tampil di antrean atasan dengan ruas baru; penugasan engine lama tidak tampil; ringkasan sama dengan isi antrean (`AT-HRD-WF-23`) | Terpenuhi | `Antrean_atasan_memuat_SPPD_dan_cuti_engine_tanpa_engine_lama_dan_ringkasan_sama`, `Antrean_dibaca_lewat_controller_dengan_akun_login`, `Filter_jenis_…`, `Mendesak_dan_lewat_SLA_…`, `Kelayakan_massal_…` |
| 2. Antrean Plh memuat tugas pejabat selama delegasi aktif, `isDelegated = true` (`AT-HRD-WF-33` bagian baca) | Terpenuhi | `Plh_aktif_melihat_tugas_pejabat_dengan_tanda_delegasi`, `Delegasi_tidak_aktif_atau_berbeda_cakupan_…` (4 kasus), `Plh_tidak_melihat_permohonannya_sendiri` |
| 3. `myAction` terisi untuk pemegang penugasan/Plh, `null` untuk orang lain; `rejectionReasons` sama dengan kotak lama | Terpenuhi | `MyAction_terisi_untuk_pemegang_dan_Plh_…`, `Alasan_penolakan_myAction_sama_dengan_kotak_masuk_lama`, `Sesudah_diputuskan_…`, `Tanpa_akun_antrean_kosong` |

---

## 4. Temuan

| Temuan | Dampak | Tindakan |
| --- | --- | --- |
| As-is: antrean V2 memilih penugasan `Pending` + `ApproverEmployeeId`, padahal seluruh workflow HR nyata dibuat engine dengan `Available` + `AssignedApproverUserId` (`HRD-TF-080`) | Kotak terpadu kosong untuk SPPD, cuti, lembur, dan seterusnya | Diperbaiki (inti task) |
| As-is: `summaryText` diisi `RequestContextJson` mentah | Teks JSON internal tampil di layar | Diganti nomor dokumen modul dan nama workflow (kontrak §35.6) |
| As-is: delegasi dibaca lewat `DelegateEmployeeId` + `StartDate`/`EndDate`, dan nama pejabat diisi teks tetap "Pejabat Definitif Terdelegasi (Plh)" | Plh tidak melihat tugas engine; nama pejabat tidak pernah benar | Delegasi dibaca lewat kunci akun dan rentang `EffectiveStartAt`–`EffectiveEndAt` dengan aturan yang sama dengan engine; nama pejabat dari akun pendelegasi |
| Relasi `WorkflowDefinition` pada instance bersifat wajib, sehingga kueri memakai join. Instance tanpa definisi yang ada (engine V2 lama, `Guid.Empty`) ikut terbuang | Sesuai aturan; filter `ApproverEmployeeId` tetap dipasang dan diuji terpisah | Tidak ada tindakan |
| Paging dilakukan di memori sesudah memuat seluruh tugas terbuka milik pengguna (dan pejabat yang mendelegasikan kepadanya) | Jumlah baris = tugas terbuka satu orang, biasanya puluhan, jadi beban kecil. Kartu mencatat risiko kinerja | Dicatat. Bila kelak ada pejabat dengan ratusan tugas terbuka, paging dapat dipindah ke SQL |
| `canBulkApprove` menilai "mendesak" dari SLA (`isUrgent`/`isOverdue`), bukan dari `PriorityLevel = Urgent` | Sesuai definisi `isUrgent` kontrak §35.6 | Tidak ada tindakan; `BE-HRD-445` memakai penilai yang sama |

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration / database | Tidak ada migration; tidak ada eksekusi database |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `WorkflowRejectionReasonOptions.cs`, `WorkflowInboxEngineQueryTests.cs` (cukup `git add`) |
| Dampak frontend | Layar Kotak Masuk Terpadu kini menerima tugas engine. Aksi Setujui/Tolak/Kembalikan dari layar itu baru meneruskan ke engine sesudah `BE-HRD-444`. Sampai saat itu, aksi tetap dilakukan dari kotak masuk lama |
| Task berikutnya | `BE-HRD-444` (aksi adapter ke engine, alasan berkode, PIN dihapus); `BE-HRD-446` (SLA/worker) dapat dikerjakan paralel |
