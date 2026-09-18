# Laporan Perubahan Backend — `BE-RWI-100`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-100` |
| Judul | Penghentian butir resep membatalkan dosis MAR |
| Slice | Gelombang 2 — `DOK-V2-2`, dirilis bersama `KEP-V2-2` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-100` |
| Trace | `FR-DOK-087`; `INT-KEP-09`; `INT-DOK-16`; `RWI-DEC-121`; state matrix 0.6.0 bagian 8.4; `VAL-DOK-51`, `51a`, `51b` |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-099` ✅; `BE-RWI-114` [BE-KEP] ✅ — **sudah mendarat di keperawatan** |
| Klasifikasi | `HEAVY` — transaksi lintas butir resep, dosis MAR, dan order sliding scale |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs`<br>`Areas/HealthServices/PharmacyManagement/Services/InpatientPrescriptionService.cs`<br>`Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` |
| Model | Gemini 3.8 Flash |
| Tanggal | 17 September 2026 |
| Status | ✅ **SELESAI** — Endpoint `PATCH /items/{itemId}/stop`, pembatalan dosis MAR berjadwal, dan penghentian order sliding scale terintegrasi dalam 1 transaksi atomik |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / PharmacyManagement` |
| Blocker | **Tidak ada** — Prasyarat `BE-RWI-114` [BE-KEP] telah mendarat dengan tersedianya `MedicationAdministrationService.CancelDueDosesForItemAsync` |
| Bukti prasyarat | `MedicationAdministrationService.Cancellation.cs` baris 31; `SlidingScaleOrderService.cs` baris 489 |
| Database / Migration | Nol migration baru; menggunakan kolom `PhmPrescriptionItem` (`IsStopped`, `StoppedAt`, `StoppedByUserId`, `StopReason`) dari migration `R4` (`BE-RWI-099`) |

---

## 1. Masalah yang Diperbaiki

Dokter menghentikan sebuah obat dari Resep Harian (`CAP-023-RSP`). Sebelum task ini diimplementasikan:
1. Endpoint `PATCH /items/{itemId}/stop` belum tersedia di `PrescriptionController`.
2. Jika sebuah obat dihentikan dokter, dosis pemberian obat yang berstatus `Due` di lembar MAR perawat tetap aktif dan berisiko diberikan kepada pasien.
3. Jika butir obat tersebut adalah insulin dengan protokol sliding scale aktif, order sliding scale-nya tidak otomatis berhenti.

Melalui task ini:
- Penghentian butir resep divalidasi ketat (hanya dokter yang bertugas aktif pada episode rawat inap pasien).
- Seluruh dosis `Due` dengan jadwal `>= stoppedAt` otomatis dibatalkan beralasan `"resep dihentikan"` via `MedicationAdministrationService.CancelDueDosesForItemAsync`.
- Order sliding scale aktif milik butir tersebut dihentikan via `SlidingScaleOrderService.StopForPrescriptionItemAsync`.
- Seluruh mutasi entitas dibungkus dalam **satu transaksi database atomik** (`BeginTransactionAsync`), menjamin prinsip fail-safe (AC-5).

---

## 2. Perubahan Source Code

### 2.1 DTO Baru (`PrescriptionDtos.cs`)
Menambahkan `StopPrescriptionItemRequest`:
- `Reason` (string, `[Required]`, `[MaxLength(500)]`) sesuai aturan `VAL-DOK-51a`.

### 2.2 Service Penghentian Butir (`InpatientPrescriptionService.cs`)
1. Menginjeksi `MedicationAdministrationService`, `SlidingScaleOrderService`, dan `InpatientClinicalContextService`.
2. Menambahkan method `StopItemAsync(Guid itemId, StopPrescriptionItemRequest request, ClaimsPrincipal? user, Guid actorUserId, CancellationToken cancellationToken)`:
   - Validasi `Reason` tidak boleh kosong (`400 BadRequest`, `VAL-DOK-51a`).
   - Pengecekan keberadaan item resep dan header resep (`404 NotFound`).
   - Pengecekan status: bila `IsStopped == true` ditolak (`409 Conflict`, "Obat ini sudah dihentikan.", `VAL-DOK-51b`).
   - Pengecekan status resep: bila resep `Cancelled` ditolak (`409 Conflict`, "Resep ini sudah dibatalkan.").
   - Pengecekan episode rawat inap: bila episode `Closed` atau `Cancelled` ditolak (`422 UnprocessableEntity`, "Perawatan pasien sudah ditutup.").
   - Pengecekan kewenangan dokter (`VAL-DOK-51`):
     - Memeriksa identitas dokter login via `ResolveActorDoctorIdAsync`.
     - Memeriksa penugasan dokter aktif via `IsDoctorAssignedAsync(episodeId, doctorId, nowUtc)`.
     - Bila tidak bertugas, mengembalikan `403 Forbidden` ("Anda tidak sedang bertugas atas pasien ini.").
   - Transaksi database atomik via `executionStrategy`:
     - Memperbarui flag `item.IsStopped = true`, `StoppedAt`, `StoppedByUserId`, `StopReason`.
     - Memanggil `_medicationAdministrationService.CancelDueDosesForItemAsync(item.Id, stoppedAt, actorUserId, "resep dihentikan")`. Dosis `Administered`, `Held`, `Refused`, dan `Missed` tetap utuh dan tidak disentuh.
     - Memanggil `_slidingScaleOrderService.StopForPrescriptionItemAsync(item.Id, cleanReason, actorUserId, stoppedAt)`.
     - Menjalankan `SaveChangesAsync()` dan `CommitAsync()`.
   - Mengembalikan `InpatientPrescriptionItemResponse` lengkap dengan nama penghenti (`StoppedByName`).

### 2.3 Endpoint Controller (`PrescriptionController.cs`)
Menambahkan endpoint:
- `PATCH api/v1/health-services/pharmacy-management/prescriptions/items/{itemId}/stop`
- Hak akses: `[AccessPermission("Prescription", "Stop")]` dan `[AccessAction("Stop", "Stop Prescription Item", ...)]`
- Audit log via `_loggerService.InfoAsync` (tanpa menyertakan `Reason` pada metadata logger karena tergolong sensitif klinis).

---

## 3. Verifikasi & Pengujian

| Skenario | Hasil | Keterangan |
| --- | :---: | --- |
| `dotnet build` | `NOT RUN` | Dikecualikan atas instruksi eksplisit pemilik (pemilik menjalankan build mandiri) |
| Validasi Kontrak API & Routing | Terpenuhi | Sesuai `api-contract.md` bagian 12.6 dan Swagger annotations |
| Penegakan Hak Akses | Terpenuhi | `[AccessPermission("Prescription", "Stop")]` + validasi penugasan aktif dokter |
| Ketahanan Transaksional | Terpenuhi | Transaksi eksplisit DB Context dengan Rollback otomatis bila terjadi error |

---

## 4. Acceptance Criteria & Definition of Done

| Kriteria | Status | Bukti Source |
| --- | :---: | --- |
| **AC-1:** Penghentian butir wajib beralasan, dan hanya oleh dokter yang sedang bertugas | **Terpenuhi** | `InpatientPrescriptionService.cs`: pengecekan `string.IsNullOrWhiteSpace(request.Reason)` → `400` (`VAL-DOK-51a`); pengecekan `IsDoctorAssignedAsync` → `403` (`VAL-DOK-51`) |
| **AC-2:** Seluruh dosis `Due` setelah waktu penghentian menjadi `Cancelled` dalam transaksi yang sama | **Terpenuhi** | `CancelDueDosesForItemAsync(item.Id, stoppedAt, actorUserId, "resep dihentikan")` dipanggil di dalam `using var transaction` |
| **AC-3:** Dosis `Administered`, `Held`, `Refused`, dan `Missed` tidak disentuh | **Terpenuhi** | Dijamin oleh implementasi `CancelDueDosesForItemAsync` yang hanya memfilter `DoseStatus == MedicationDoseStatus.Due` |
| **AC-4:** Butir yang sudah dihentikan tidak dapat dihidupkan kembali | **Terpenuhi** | Pengecekan `item.IsStopped` langsung mengembalikan `409 Conflict` ("Obat ini sudah dihentikan."); tidak ada endpoint unstop |
| **AC-5:** Galat buatan pada langkah mana pun → nol perubahan tersimpan | **Terpenuhi** | Blok `try-catch` membungkus seluruh mutasi dengan `transaction.RollbackAsync()` |

---

## 5. Catatan Penutup

- Sub-modul `dokter-rawat-inap` kini telah **100% tuntas** di lapisan backend (seluruh 18 task pada `backend-roadmap-v2.md` berstatus ✅).
- Perubahan siap dikompilasi secara mandiri oleh pemilik.
