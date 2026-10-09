# Laporan Perubahan Backend — `BE-IGD-069`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-069` |
| Judul | Tindakan keperawatan IGD tercatat sebagai tindakan klinis umum |
| Slice | `EPIC IGD-14` / `MVP-9` — tindakan perawat |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-100`; `AT-IGD-206`, `207`; DoD PRD §10.4 butir 3, 6, 7 |
| Keputusan | `IGD-DEC-225`; `IGD-DEC-230` pilihan desain 1 (DPJP aktif sebagai dokter; tanpa DPJP ditolak) dan 5 (ditolak pada kunjungan berakhir) |
| Contract version | API **`0.15.0`** §10.1 nomor 8, §10.6 (`CreateEmergencyNursingActionRequest`); validation **`0.14.0`** §12.3 aturan 7–12; state **`0.10.0`** §10.2; permission §9.1, §9.3 |
| Dependency | — |
| Klasifikasi | `LIGHT` — berkas diubah 3, endpoint baru aditif, nol migration, nol `Program.cs` |
| Task mode | `BACKEND` — instruksi eksplisit pemilik (*"yaa bisa di mulai mas"*) |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/ClinicalManagement/DTOs/PatientProcedureDtos.cs`, `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs`, `Areas/HealthServices/ClinicalManagement/Services/PatientProcedureOrderService.cs`; laporan ini |
| Tanggal | 9 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 9 Oktober 2026: implementasi selesai, diff bersih 3 berkas, nol komentar baru, siap build dan uji API/layar** |

---

## 1. Masalah yang diselesaikan

Perawat IGD membutuhkan kemampuan mencatat tindakan keperawatan yang telah selesai dikerjakan (misalnya pemasangan infus, nebulizer, injeksi darurat) tanpa mengharuskan adanya catatan dokter terbuka (`ConsultationId` kosong). Baris tindakan tersebut harus tersimpan di tabel tindakan klinis umum (`TrxPatientProcedure`) dengan:
- Pelaksana (`ExecutedByUserId`, `PerformedByUserId`) = perawat yang menginput.
- Dokter penanggung jawab (`DoctorId`) = DPJP aktif kunjungan IGD.
- Status tindakan = `Completed`, status sumber = `NursingAction`.
- Snapshot tarif dan asuransi diselesaikan otomatis via `InsuranceCoverageService`.
- Terbaca pada daftar tindakan pasien (`GET /patient-procedures?encounterId=`) dan ringkasan tagihan (Billing).

---

## 2. Aturan Bisnis & Validasi (Aturan 12 → 7 → 8 → 9 → 10 → 11)

| No | Aturan | Kode | Pesan / Perilaku |
| ---: | --- | :-: | --- |
| 12 | **Idempotensi (Paling awal)**: Kiriman ulang dengan `idempotencyKey` yang sama mengembalikan baris yang sudah tercatat | `200` | Baris yang sama dikembalikan, tanpa duplikasi |
| 7 | **Encounter IGD**: Encounter wajib terdaftar dan milik kunjungan IGD (`EmgVisit`) yang aktif | `404` / `400` | Tidak ada encounter: `404` *"Encounter tidak ditemukan."*<br>Bukan IGD: `400` *"Pencatatan tindakan keperawatan ini hanya untuk pasien IGD."* |
| 8 | **Kunjungan Berjalan**: Kunjungan belum `Completed`, `Cancelled`, atau `VisitCompletedAt.HasValue` | `409` | *"Kunjungan IGD ini sudah berakhir, sehingga tindakan baru tidak dapat dicatat."* |
| 9 | **DPJP Aktif**: Kunjungan wajib memiliki DPJP aktif (`EffectiveTo == null`); dokternya menjadi `DoctorId` baris tindakan | `409` | *"Pasien belum punya dokter penanggung jawab. Tetapkan DPJP di layar triage lebih dulu, lalu catat tindakan ini."* |
| 10 | **Master Tindakan Aktif**: Prosedur wajib ada di `MstProcedure`, aktif, dan tidak dihapus | `400` | *"Master tindakan tidak ditemukan atau tidak aktif."* |
| 11 | **Waktu Pelaksanaan**: `performedAt` tidak boleh di masa depan (> toleransi 1 menit); kosong berarti waktu server | `400` | *"Waktu pelaksanaan tindakan tidak boleh melewati waktu sekarang."* |

---

## 3. Rincian Perubahan Kode

### 3.1 DTO (`PatientProcedureDtos.cs`)
Menambahkan DTO permintaan baru:
```csharp
public class CreateEmergencyNursingActionRequest
{
    [Required]
    public Guid EncounterId { get; set; }

    [Required]
    public Guid ProcedureId { get; set; }

    public decimal? Quantity { get; set; }

    public DateTime? PerformedAt { get; set; }

    [MaxLength(1000)]
    public string? ClinicalNote { get; set; }

    [MaxLength(100)]
    public string? IdempotencyKey { get; set; }
}
```

### 3.2 Controller (`PatientProcedureController.cs`)
Menambahkan endpoint `POST emergency-nursing-actions`:
- Route: `POST api/v1/health-services/clinical-management/patient-procedures/emergency-nursing-actions`
- Izin: `[AccessPermission("PatientProcedure", "Create")]`
- Metadata: `[AccessAction("Create", "Create Emergency Nursing Action", Description = "Mencatat tindakan keperawatan pasien IGD tanpa catatan dokter", AccessType = AccessTypes.Create, SortOrder = 3)]`
- Menghubungkan langsung ke `_procedureOrderService.CreateEmergencyNursingActionAsync`.

### 3.3 Service (`PatientProcedureOrderService.cs`)
Menambahkan method `CreateEmergencyNursingActionAsync`:
- Urutan validasi kanonikal 12 → 7 → 8 → 9 → 10 → 11.
- Pembacaan DPJP aktif dari `EmgDoctorAssignment` (`EmergencyVisitId == visit.Id && !IsDelete && EffectiveTo == null`).
- Penyelesaian tarif dan coverage via `_insuranceCoverageService.ResolveProcedureAsync`.
- Pembentukan entitas `TrxPatientProcedure`:
  - `ProcedureSource = PatientProcedureSource.NursingAction`
  - `ProcedureStatus = PatientProcedureStatus.Completed`
  - `IsExecuted = true`, `ExecutedAt = performedAt`, `ExecutedByUserId = actorUserId`
  - `PerformedAt = performedAt`, `PerformedByUserId = actorUserId`
  - `DoctorId = activeAssignment.DoctorId`
  - `ConsultationId = null`, `InpEpisodeId = null`, `PhysicianVisitId = null`
  - `IsEmergencyProcedure = true`, `IsBillable = true`
- Pemetaan `IsEmergencyProcedure = entity.IsEmergencyProcedure` pada `MapToResponse`.

---

## 4. Verifikasi Engineering & Tata Kelola

| Pemeriksaan | Hasil | Keterangan |
| --- | :-: | --- |
| Cakupan Berkas | **PASS** | Persis 3 berkas terubah (`git diff --stat`: 3 files changed, 232 insertions) |
| Komentar Baru | **PASS** | Nol komentar baru (`//`, `///`, `/* */`) |
| Line Endings | **PASS** | Seluruh berkas berakhiran baris CRLF (`git diff --check` PASS) |
| EF Core Migration | **PASS** | Nol migration baru (skema tabel `TrxPatientProcedure` sudah siap) |
| `Program.cs` | **PASS** | Nol sentuhan (`PatientProcedureOrderService` sudah terdaftar di baris 472) |
| Regresi Jalur Dokter & RWI | **PASS** | `POST /` dan `POST /inpatient-orders` tidak berubah satu baris pun |
