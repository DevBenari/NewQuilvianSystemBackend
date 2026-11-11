# Laporan Perubahan Backend — `BE-HRD-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-030` |
| Judul | Lima Jenis Rujukan Workflow dan Layanan Integrasi Persetujuan S-D1 |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-018`, `HRD-DEC-064`, `INT-HR-18`, `NFR-013`, `API v7 §13.1`, `02-backend-architecture.md` §16.4.9 |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdWorkforcePlanningWorkflowIntegrationService.cs`, `IHrdWorkforcePlanningWorkflowIntegrationService.cs`, `IWorkforcePlanningDecisionApplier.cs`, `WorkflowReferenceLifecycleService.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & seluruh 5 acceptance criteria terbukti |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, dokumen perencanaan tenaga kerja rumah sakit (`S-D1`) belum dapat memanfaatkan kotak masuk persetujuan terpadu (*Unified Approval Inbox*) rumah sakit. Lima jenis dokumen perencanaan tenaga:
1. `WorkforceStaffingStandard` (Standar Kebutuhan Tenaga — alur `T9`)
2. `WorkforceInitialFormationBatch` (Batch Inisialisasi Formasi Awal — alur `T10`)
3. `WorkforceManpowerPlan` (Rencana Tenaga Kerja Tahunan & Addendum — alur `T10`)
4. `WorkforceHeadcountRequest` (Permintaan Headcount — alur `T11` dan `T12`)
5. `WorkforceHeadcountCancellation` (Pembatalan Kursi Permintaan — alur `T13`)

belum terdaftar pada layanan siklus hidup workflow (`WorkflowReferenceLifecycleService`). Tanpa adanya layanan integrasi ini, persetujuan di kotak masuk tidak dapat diteruskan kembali ke domain, berisiko menyebabkan crash jika penerap belum siap, dan rentan terhadap duplikasi mutasi formasi bila peristiwa persetujuan terkirim lebih dari satu kali (*duplicate/stale event*).

Melalui task `BE-HRD-030`, jembatan integrasi antara mesin persetujuan bersama dan domain perencanaan tenaga kerja resmi dibangun secara elegan, idempoten, dan mematuhi batasan privasi data sensitif rumah sakit.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

* **Memulai Alur Persetujuan Dokumen**:
  - Saat dokumen perencanaan (misal usulan formasi awal atau penambahan perawat) diajukan oleh Kepala Unit / HR Officer, service domain memanggil `IHrdWorkforcePlanningWorkflowIntegrationService.StartWorkflowAsync(...)`.
  - Instans persetujuan resmi dibuat dan diajukan ke kotak masuk terpadu para verifikator (HR Manager, Finance, Direktur).
* **Penyelarasan Keputusan Akhir (*Lifecycle Synchronization*)**:
  - Ketika verifikator memberikan keputusan (Setuju, Tolak, atau Kembalikan), mesin workflow memicu `WorkflowReferenceLifecycleService.SynchronizeAsync(...)`.
  - Sistem mendeteksi jenis rujukan `S-D1` dan meneruskannya ke `HrdWorkforcePlanningWorkflowIntegrationService`.
* **Idempotensi & Perlindungan Formasi Ganda (`INT-HR-18`, `NFR-013`)**:
  - Jika keputusan "Setuju" terkirim ulang untuk dokumen yang statusnya sudah final (`Active`, `Scheduled`, `Applied`, `Approved`, `ReadyForRecruitment`, `Fulfilled`, `Closed`, `Cancelled`, `Rejected`), sistem secara otomatis mengabaikan event tersebut tanpa mengubah data dokumen dan tanpa melahirkan formasi versi kedua.
* **Keamanan Penerap Belum Tersedia**:
  - Jika jenis rujukan belum memiliki penerap domain terdaftar (karena task domain berikutnya belum selesai diimplementasikan), sistem mencatat pesan galat secara terstruktur dan dokumen dibiarkan tetap utuh tanpa menyebabkan aplikasi terhenti/crash.
* **Perlindungan Privasi Angka Biaya dan Alasan (`NFR-014`)**:
  - Log audit integrasi workflow dipastikan bersih dari angka nominal biaya (gaji tengah grade/estimasi biaya) maupun teks alasan pengajuan/penolakan.

---

## 3. Rincian Perubahan Teknis

### 3.1 Kontrak Penerap Keputusan Domain (`IWorkforcePlanningDecisionApplier.cs`)
Dibuat di direktori `Areas/Corporate/HumanResource/WorkforcePlanning/Services/`:
* `IWorkforcePlanningDecisionApplier`: Antarmuka yang akan diimplementasikan oleh task domain berikutnya (`BE-HRD-033`, `BE-HRD-035`, `BE-HRD-041`, `BE-HRD-044`, `BE-HRD-047`).
* `WorkforcePlanningDecisionContext`: DTO parameter keputusan mencakup `WorkflowInstanceId`, `ReferenceId`, `ReferenceType`, `WorkflowStatus`, `ActorUserId`, dan flag `AllowAutoApply`.
* `WorkforcePlanningApplyResult`: Hasil eksekusi penerap domain lengkap dengan helper `Success`, `Unhandled`, dan `AlreadyFinal`.

### 3.2 Antarmuka & Layanan Integrasi Workflow (`HrdWorkforcePlanningWorkflowIntegrationService.cs`)
Dibuat di direktori `Areas/Corporate/HumanResource/WorkforcePlanning/Services/`:
* Mengimplementasikan `IHrdWorkforcePlanningWorkflowIntegrationService`.
* **`StartWorkflowAsync`**: Memvalidasi jenis rujukan, memanggil `WorkflowService.CreateAsync`, dan secara otomatis menjalankan `SubmitAsync` ke antrean persetujuan.
* **`SynchronizeAsync`**:
  1. Memverifikasi validitas rujukan.
  2. Mengeksekusi `CheckIfDocumentAlreadyFinalAsync` langsung ke tabel `Hrd*` dengan mode `AsNoTracking()` untuk deteksi status final idempoten.
  3. Mencari penerap terdaftar pada koleksi `IEnumerable<IWorkforcePlanningDecisionApplier>`.
  4. Jika penerap tidak ditemukan, mencatat error pada log dan mengembalikan `IsHandled = false` tanpa merusak dokumen.
  5. Jika penerap ditemukan, mengeksekusi penerapan keputusan domain.
* **Logger Bersih**: Log hanya memuat `WorkflowInstanceId`, `ReferenceId`, `ReferenceType`, `WorkflowStatus`, dan `CurrentStatus`. Tidak ada field biaya maupun teks alasan.

### 3.3 Pembaruan `WorkflowReferenceLifecycleService.cs`
* Mendaftarkan 5 konstanta jenis rujukan dan alias lengkap untuk toleransi format:
  - `WorkforceStaffingStandard` (alias: `StaffingStandard`, `HrdStaffingStandard`)
  - `WorkforceInitialFormationBatch` (alias: `InitialFormationBatch`, `HrdInitialFormationBatch`)
  - `WorkforceManpowerPlan` (alias: `ManpowerPlan`, `HrdManpowerPlan`)
  - `WorkforceHeadcountRequest` (alias: `HeadcountRequest`, `HrdHeadcountRequest`)
  - `WorkforceHeadcountCancellation` (alias: `HeadcountCancellation`, `HrdHeadcountRequestCancellation`).
* Menginjeksi `IHrdWorkforcePlanningWorkflowIntegrationService` pada konstruktor.
* Meneruskan seluruh rujukan `IsWorkforcePlanningReference(workflow.ReferenceType)` ke layanan integrasi.

### 3.4 Pendaftaran Dependency Injection (`Program.cs`)
Mendaftarkan layanan integrasi ke service container ASP.NET Core:
* `builder.Services.AddScoped<IHrdWorkforcePlanningWorkflowIntegrationService, HrdWorkforcePlanningWorkflowIntegrationService>();`
* `builder.Services.AddScoped<HrdWorkforcePlanningWorkflowIntegrationService>();`

---

## 4. Evaluasi Kriteria Penerimaan (Acceptance Criteria)

| No | Kriteria Penerimaan | Status | Bukti Implementasi pada Kode |
| :---: | :--- | :---: | :--- |
| 1 | Kelima jenis rujukan dikenali dan diteruskan ke layanan integrasi; rujukan modul lain tidak terpengaruh | **TERPENUHI** | `WorkflowReferenceLifecycleService.IsWorkforcePlanningReference` mendeteksi 5 jenis rujukan; rantai modul lain tetap melalui jalur aslinya. |
| 2 | Keputusan untuk jenis rujukan yang belum punya penerap dicatat sebagai galat dan dokumen tidak berubah — bukan crash | **TERPENUHI** | `HrdWorkforcePlanningWorkflowIntegrationService.SynchronizeAsync` mencatat `LogError` dan mengembalikan `IsHandled = false`, dokumen tidak berubah. |
| 3 | Keputusan "setujui" yang terkirim ulang untuk dokumen yang sudah final diabaikan dan dicatat, tidak menghasilkan apapun kedua kalinya | **TERPENUHI** | `CheckIfDocumentAlreadyFinalAsync` mendeteksi status akhir entitas (`Active`, `Applied`, `Approved`, `ReadyForRecruitment`, dsb.) dan mengabaikan event ganda. |
| 4 | Definisi langkah `T9` s.d. `T13` tidak ditulis di source; definisinya data (`BE-HRD-050`) | **TERPENUHI** | Tidak ada hardcoded workflow definition steps di kode C#. Definisi workflow disimpan sebagai data database. |
| 5 | Payload logger tanpa angka biaya dan tanpa teks alasan | **TERPENUHI** | Seluruh invocation logger pada service hanya mencatat identifier teknis dan status, bebas dari nominal rupiah dan teks bebas alasan pemohon. |

---

## 5. Hasil Verifikasi dan Kepatuhan

1. **Review Diff Kode**:
   - Berkas baru: `IWorkforcePlanningDecisionApplier.cs`, `IHrdWorkforcePlanningWorkflowIntegrationService.cs`, `HrdWorkforcePlanningWorkflowIntegrationService.cs`.
   - Berkas terintegrasi: `WorkflowReferenceLifecycleService.cs`, `Program.cs`.
2. **Kompilasi Solusi (`dotnet build`)**:
   - Status: **BERHASIL (`Build succeeded`)**
   - Error: **0 Error**
   - Warning: 12 Warning (vulnerability advisories bawaan pustaka ImageSharp).
   - Waktu kompilasi: **6.07 detik**.

---

## 6. Status dan Langkah Selanjutnya

* **Status Task**: Selesai (`PASS`) dan siap melayani task domain berikutnya.
* **Task Siap Dikerjakan Berikutnya**:
  - `BE-HRD-031` — Pengaturan perencanaan per rumah sakit (`HrdWorkforcePlanningSettingService` & Controller).
  - `BE-HRD-032` — Buku formasi berversi: satu-satunya penulis formasi dan API baca (`HrdPositionFormationService` & Controller).
  - `BE-HRD-033` — Batch formasi awal sampai `T10` melahirkan formasi versi 1 (Prasyarat: `BE-HRD-030` ✅, `BE-HRD-032`).
