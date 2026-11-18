# Laporan Penyelesaian Task: BE-HRD-371 Serah Terima: Task Owner, Verify, Progress dan Finalisasi Persisten

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-371`
* **Nama Task**: Serah Terima: Task Owner, Verify, Progress dan Finalisasi Persisten
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 5: Serah Terima / Handovers)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penegakan transisi siklus hidup status tugas serah terima (*Handover Task Lifecycle*): `Pending -> Submitted -> Completed / Returned`.
  - Pencegahan verifikasi langsung dari status `Pending` (`VerifyTaskAsync` mewajibkan status `Submitted`).
  - Pencegahan pengajuan ulang (*resubmit*) item tugas yang sudah berstatus `Completed`.
  - Penomoran kode unik tugas (`TSK-...`) dengan deteksi benturan konkuren pada `AddTaskAsync`.
  - Penegakan integritas finalisasi checklist pada `CompleteChecklistAsync`: checklist dilarang diselesaikan jika jumlah tugas aktif adalah nol (`activeTasks.Count == 0`) atau masih ada tugas yang belum berstatus `Completed`.
  - Komputasi progres serah terima yang akurat dan otomatis tersimpan di database.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `LIFECYCLE INTEGRITY & STATE MACHINE`
* **Wewenang**: Penguatan logika transisi serah terima tugas operasional dan aset pegawai.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam operasional klinis dan keperawatan:
1. **Penyerahan Tanggung Jawab Klinis**: Dokter penanggung jawab pelayanan (DPJP) atau perawat primer yang berhenti wajib menyerahkan daftar pasien aktif, protokol terapi yang sedang berjalan, dan kunci lemari obat darurat kepada staf pengganti.
2. **Verifikasi 2 Arah**: Pegawai yang keluar mengajukan serah terima (`Submitted`), dan pegawai pengganti / atasan langsung memeriksa dan mengonfirmasi penerimaan tugas (`Verify/Completed`). Atasan tidak boleh menandatangani tugas yang belum diserahkan oleh staf.
3. **Penyelesaian Mutlak**: Checklist serah terima tidak boleh diselesaikan (Complete) secara instan dalam keadaan kosong tanpa item tugas tercatat, karena melanggar standar akreditasi rumah sakit (KARS/JCI) terkait serah terima asuhan pasien (clinical handover).

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
Pada `Areas/Corporate/HumanResource/LifecycleManagement/Services/OffboardingHandoverService.cs`:
1. **Penomoran Unik Tugas**:
   ```csharp
   var baseCode = $"TSK-{DateTime.UtcNow:yyyyMMddHHmmss}";
   var taskCode = baseCode;
   var counter = 1;
   while (checklist.Tasks.Any(t => t.TaskCode == taskCode))
   {
       taskCode = $"{baseCode}-{counter++}";
   }
   ```
2. **Pencegahan Submit Item yang Sudah Selesai**:
   ```csharp
   if (string.Equals(task.Status, "Completed", StringComparison.OrdinalIgnoreCase))
   {
       throw new InvalidOperationException($"Tugas '{task.TaskCode}' sudah selesai dan tidak dapat diserahkan ulang.");
   }
   ```
3. **Guard Verifikasi Hanya untuk Status Submitted**:
   ```csharp
   if (!string.Equals(task.Status, "Submitted", StringComparison.OrdinalIgnoreCase))
   {
       throw new InvalidOperationException($"Tugas '{task.TaskCode}' berstatus '{task.Status}'. Hanya tugas berstatus 'Submitted' yang dapat diverifikasi.");
   }
   ```
4. **Pencegahan Selesai dengan Nol Tugas**:
   ```csharp
   if (activeTasks.Count == 0)
   {
       throw new InvalidOperationException("Checklist serah terima tidak memiliki tugas aktif dan tidak dapat diselesaikan.");
   }
   ```

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/lifecycle/handovers` | `[Tags("HR Lifecycle - Handover")]` | Ambil daftar checklist serah terima | `Hrd.Lifecycle.Handover.View` | `page`, `pageSize`, `status` | `ApiResponse<PagedResult<OffboardingHandoverDto>>` |
| `POST` | `/api/corporate/human-resource/lifecycle/handovers/{id}/tasks` | `[Tags("HR Lifecycle - Handover")]` | Tambah butir tugas serah terima dengan kode unik | `Hrd.Lifecycle.Handover.Manage` | `AddHandoverTaskRequest` | `ApiResponse<OffboardingHandoverTaskDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/handovers/{id}/tasks/{taskId}/submit` | `[Tags("HR Lifecycle - Handover")]` | Ajukan penyerahan butir tugas | `Hrd.Lifecycle.Handover.Manage` | `SubmitHandoverTaskRequest` | `ApiResponse<OffboardingHandoverTaskDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/handovers/{id}/tasks/{taskId}/verify` | `[Tags("HR Lifecycle - Handover")]` | Verifikasi butir tugas oleh penerima | `Hrd.Lifecycle.Handover.Manage` | `VerifyHandoverTaskRequest` | `ApiResponse<OffboardingHandoverTaskDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/handovers/{id}/complete` | `[Tags("HR Lifecycle - Handover")]` | Selesaikan seluruh checklist serah terima | `Hrd.Lifecycle.Handover.Manage` | - | `ApiResponse<OffboardingHandoverDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit State Machine**: Transisi status tugas divalidasi ketat dan penomoran tugas collision-free.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).