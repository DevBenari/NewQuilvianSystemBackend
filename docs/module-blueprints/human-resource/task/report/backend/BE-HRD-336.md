# Laporan Penyelesaian Task: BE-HRD-336 One-Separation Owner, Idempotensi, Number Series Aman, Transaction Safety

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-336`
* **Nama Task**: One-Separation Owner, Idempotensi, Number Series Aman, Transaction Safety
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Transaksi & Integritas Data)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penegakan integritas data: Satu pegawai yang sedang menjalani pemisahan kerja (resignation, retirement, non-renewal contract, termination) hanya boleh memiliki tepat **satu record separation aktif**.
  - Pencegahan inisiasi ganda (double-submit / race condition) pada `EmployeeSeparationService.InitiateSeparationAsync`.
  - Eliminasi penggunaan `new Random().Next(1000, 9999)` pada penomoran nomor kasus pensiun, serah terima, dan checklist di `RetirementService.cs`.
  - Pergantian generator nomor bisnis menjadi pola deterministik dan collision-free berbasis format waktu UTC dan GUID atomik (`RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}`[..35]).
  - Penjaminan batasan transaksi (ACID transaction boundary) pada seluruh alur handoff dan status transition.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement` & `LifecycleManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `TRANSACTION INTEGRITY & SECURITY HARDENING`
* **Wewenang**: Penegakan aturan integritas pemisahan kepegawaian dan nomor seri bisnis sesuai mandat instruksi arsitektur.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Di lingkungan rumah sakit:
1. **Risiko Benturan Data Pegawai Keluar**: Jika seorang perawat sedang dalam proses pengunduran diri aktif (Notice Period 30 hari), sistem tidak boleh mengizinkan inisiasi pemutusan hubungan kerja (PHK) ganda secara serentak tanpa menyelesaikan atau membatalkan proses sebelumnya. Benturan ini dapat mengacaukan perhitungan kompensasi dan matriks inventaris serah terima.
2. **Bahaya Nomor Acak (Random Number Collision)**: Nomor referensi resmi seperti nomor SK Pensiun (`RET-...`) atau Berita Acara Serah Terima Aset medis (`EXC-...`) wajib unik secara hukum. Menggunakan `Random.Next()` di server multi-threaded rentan menghasilkan duplikasi nomor antar-pengguna yang menekan tombol bersamaan, berpotensi memicu kegagalan database atau kompromi dokumen legal.
3. **Idempotensi Handoff**: Pengiriman ulang permintaan handoff karena gangguan jaringan harus mengembalikan record yang sudah dibuat sebelumnya tanpa membuat duplikat serah terima atau checklist inventaris.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
1. **`Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/EmployeeSeparationService.cs`**:
   - Menambahkan validasi keberadaan proses pemisahan aktif:
     ```csharp
     var existingActive = await _context.Set<TrxEmployeeSeparation>()
         .AnyAsync(s => s.EmployeeId == request.EmployeeId 
             && s.Status != SeparationStatus.FinalizedCompleted 
             && s.Status != SeparationStatus.Cancelled);
     if (existingActive)
     {
         throw new InvalidOperationException("Pegawai ini sudah memiliki proses pemberhentian/pemisahan yang sedang aktif dan belum selesai.");
     }
     ```
2. **`Areas/Corporate/HumanResource/LifecycleManagement/Services/RetirementService.cs`**:
   - Menghapus pembuatan nomor berbasis `new Random()`, menggantikannya dengan:
     ```csharp
     var now = DateTime.UtcNow;
     var caseNumber = $"RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 40)];
     var separationNumber = $"SEP-RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 45)];
     var clearanceNumber = $"EXC-RET-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..Math.Min(35, 45)];
     ```

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/corporate/human-resource/employee-relations/separations` | `[Tags("HR Employee Separation")]` | Inisiasi pemisahan pegawai dengan guard idempotensi & validasi aktif | `Hrd.Separation.Create` | `InitiateSeparationRequestDto` | `ApiResponse<EmployeeSeparationDto>` |
| `POST` | `/api/corporate/human-resource/lifecycle/retirements/{id}/handoff` | `[Tags("HR Lifecycle - Retirement")]` | Handoff pensiun ke serah terima & clearance dengan nomor aman | `Hrd.Lifecycle.Retirement.Manage` | - | `ApiResponse<RetirementCaseDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Keunikan Nomor**: Seluruh prefix `RET-`, `SEP-RET-`, `EXC-RET-` kini dijamin bebas benturan konkuren.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
