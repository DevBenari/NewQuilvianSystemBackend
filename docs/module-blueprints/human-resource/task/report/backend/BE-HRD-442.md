# Laporan Perubahan Backend — `BE-HRD-442`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-442` |
| Judul | Engine: eskalasi menambah penugasan setara |
| Slice | `S-A7-V2` amandemen / `MVP-72` gelombang 2 · `EPIC HRD-124` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 29, kartu `BE-HRD-442` |
| Trace | `FR-WF-30`; `HRD-DEC-232`, `HRD-DEC-185` |
| Contract version | `v29` — State 33.2–33.4; Validation `VR-WF-31`, `VR-WF-11`; Permission 33.2 — approved 20 November 2026 |
| Dependency | `BE-HRD-440` ✅ (kolom `EscalatedFromAssignmentId`, konstanta `Escalate`); berkas engine bersama `BE-HRD-441` ✅ |
| Klasifikasi | `STANDARD` — perubahan engine workflow (penghitungan tahap, penugasan baru); uji harness engine nyata |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026: "lanjut yang BE-HRD-442") |
| Model | Claude Opus 5.5 |
| Source acuan | Backend `815ce085` (branch `yoga`) + working tree `BE-HRD-440`/`441` dan task ini |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpenuhi |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkflowManagement` (registry: `SHARED PLATFORM CAPABILITY`, prefix `Wfl`, `ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY` — `WorkflowService` (berkas parsial baru `WorkflowService.Escalation.cs` + `WorkflowService.cs`); tanpa entity, migration, atau endpoint baru |
| QBE yang berlaku | `QBE-SVC-001` (orkestrasi tetap di service engine), `QBE-API-001` (hasil `WorkflowServiceResult` dengan kode status yang sudah dipakai engine). Urutan penugasan memakai nilai terbesar + 1 **di dalam transaksi Serializable** pada satu tahap; ini urutan internal, bukan penomoran dokumen |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `DATABASE_RULES.md` (suite Skill) |
| Wewenang terpisah | Implementasi source ✅; migration ✖ (kolom dari `BE-HRD-440`); eksekusi database ✖; deployment ✖ |

---

## 1. Perubahan

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.Escalation.cs` (baru, parsial) | `AddEscalationAssignmentAsync(assignmentId, targetApproverUserId, reason, isSystem, actorUserId)` → `WorkflowServiceResult<Guid>` (id penugasan baru). Transaksi Serializable. Penugasan baru: `Available`, `IsRequired = false`, `IsCurrentAssignment = true`, `EscalatedFromAssignmentId`, `AssignmentOrder` berikutnya, `DueAt` = sekarang + `EscalationAfterHours ?? ReminderAfterHours` tahap master, pengalihan Plh atasan (`ApplyDelegationAsync`). `TotalAssignmentCount` tahap +1. Aksi `Escalate` (`IsSystemAction` = otomatis; sumber `Scheduler`/`Web`; komentar alasan atau "Dieskalasi ke {nama}: lewat batas waktu {DueAt}."). Validasi `VR-WF-31` (`ValidateEscalationAsync`). Pembantu `EscalationRoot`, `EscalationGroup`, `IsRequiredSlotApproved`, `SkipEscalationTwins` |
| `…/Services/WorkflowService.cs` | `IsApprovalRequirementSatisfied`: slot wajib dihitung disetujui bila ia atau turunan eskalasinya `Approved` (state 33.3). `ApproveAsync`: sesudah penugasan disetujui, kembaran terbuka pada kelompok eskalasinya ditutup `Skipped` dengan aksi sistem `Skip` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/WorkflowEscalationEngineTests.cs` (baru), `AcceptanceTestIndex.md` | 12 uji (8 fakta + 4 kasus teori) dan satu baris indeks |

### Aturan `VR-WF-31` yang ditegakkan

| Kondisi | Hasil |
| --- | --- |
| Workflow/tahap tidak berjalan, atau penugasan sudah diputuskan | `409` "Penugasan tidak lagi menunggu keputusan sehingga tidak dapat dieskalasi." |
| Penugasan tidak wajib dan bukan hasil eskalasi | `409` "Hanya penugasan wajib yang dapat dieskalasi." |
| Penugasan sudah pernah dieskalasi | `409` "Penugasan ini sudah pernah dieskalasi." (rantai tetap boleh: penugasan eskalasi yang lewat batas dapat dieskalasi lagi) |
| Otomatis sebelum `DueAt` lewat | `409` "Penugasan belum melewati batas waktu." (eskalasi manual boleh kapan saja) |
| Tujuan nonaktif atau pemohon | `400` "Pejabat tujuan eskalasi tidak sah." |
| Tujuan sudah penyetuju pada tahap itu | `409` "Pejabat tujuan sudah menjadi penyetuju pada tahap ini." (menjaga indeks unik tahap + pengguna, temuan `BE-HRD-440`) |

---

## 2. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `WorkflowEscalationEngineTests` + `WorkflowDelegatedActionTests` + `TravelSubmitTests` + `TravelWorkflowLifecycleTests` | 67/67 lulus | `PASS` (AC1–AC4) | Keluaran `dotnet test` |
| Kontrol negatif: 10 mutasi (hitung slot tanpa eskalasi; kembaran tidak ditutup; eskalasi wajib; tanpa cek sudah dieskalasi; otomatis tanpa cek `DueAt`; tujuan pemohon/nonaktif/sudah penyetuju diizinkan; urutan tetap; aksi manual tercatat sistem) | 10/10 tertangkap; source dipulihkan | `PASS` | `mut442.js` |
| Regresi penuh proyek uji HR | 383/383 lulus (10 menit 57 detik; dari salinan tanpa empat uji payroll yang tidak terkompilasi sejak merge `815ce085`) | `PASS` | Keluaran `dotnet test` |
| QBE `Strict` atas 2 berkas | 0 violation, 0 review; probe `TrxZzzQbeProbe442` terdeteksi 4 violation lalu dihapus | `PASS` | Keluaran checker |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Eskalasi membuat penugasan atasan sesuai outcome; penugasan awal tetap `Available` | Terpenuhi | `Eskalasi_otomatis_menambah_penugasan_atasan_setara_tanpa_persetujuan_otomatis` |
| 2. Mode `Any`: atasan menyetujui → tahap selesai, penugasan awal `Skipped`; mode `All`: selesai hanya sesudah penyetuju wajib lain juga menyetujui | Terpenuhi (juga penyetuju awal tetap dapat menyetujui sesudah eskalasi) | `Mode_Any_…` (2 uji), `Mode_All_butuh_slot_lain_dan_kembaran_ditutup_Skipped`, `Mode_All_tanpa_eskalasi_tetap_butuh_semua_penyetuju` |
| 3. Eskalasi kedua atas penugasan sama ditolak/dilewati; pelaku eskalasi tidak boleh pemohon | Terpenuhi | `Eskalasi_sekali_per_penugasan_dan_rantai_berlanjut`, teori `Eskalasi_ditolak_…` (4 kasus), `Penugasan_yang_sudah_diputuskan_…` |
| 4. Tidak ada persetujuan otomatis | Terpenuhi | Uji AC1: SPPD tetap `Submitted`, workflow `InProgress` sesudah eskalasi |

---

## 3. Keputusan implementasi (dalam batas kontrak)

| Hal | Keputusan | Alasan |
| --- | --- | --- |
| Pemanggil | Metode engine dipakai `WorkflowSlaEnforcementService` (otomatis, `BE-HRD-446`) dan eskalasi manual HR (`BE-HRD-446`). Pelaku manual diteruskan sebagai parameter karena worker tidak punya konteks HTTP | Engine menjaga aturan; penentuan atasan dan notifikasi tetap di layanan SLA (desain 37.4) |
| `ApproverSourceSnapshot` penugasan eskalasi | Disalin dari penugasan asal | Menghindari nilai sumber baru yang tidak dikenal `NormalizeApproverSource`; status eskalasi terlihat dari `EscalatedFromAssignmentId` |
| Penutupan kembaran saat tahap selesai | Memakai perilaku engine as-is (`CompleteApprovedStep` men-`Skipped` sisa penugasan tanpa aksi); aksi `Skip` hanya dicatat bila tahap belum selesai | Tidak mengubah jejak workflow lain |
| Tolak/Kembalikan oleh anggota kelompok eskalasi | Perilaku engine as-is (penugasan lain di tahap dibatalkan) | Sudah menutup kembaran |

---

## 4. Catatan penutup

| Hal | Isi |
| --- | --- |
| Git | Tidak ada stage, commit, push, pull, merge, rebase, atau deploy. Berkas baru: `WorkflowService.Escalation.cs`, `WorkflowEscalationEngineTests.cs` (cukup `git add`) |
| Task berikutnya | `BE-HRD-446` (SLA memakai metode ini), `BE-HRD-447` (endpoint generik), `BE-HRD-443`/`444` |
