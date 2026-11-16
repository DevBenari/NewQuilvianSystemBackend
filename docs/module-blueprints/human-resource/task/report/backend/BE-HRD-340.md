# Laporan Penyelesaian Task: BE-HRD-340 Pemberhentian dan Perhitungan/Otorisasi Pesangon Authoritative

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-340`
* **Nama Task**: Pemberhentian dan Perhitungan/Otorisasi Pesangon Authoritative
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12: Separation & Retirement (Pemberhentian & Pensiun)
* **Slice Delivery**: `S-C5-SEP` (Submenu 4: Pemberhentian)
* **Status**: `✅ Selesai (2026-11-16)`
* **Deliverables**:
  - Penegakan integritas inisiasi pemutusan hubungan kerja (PHK/Pemberhentian) dengan pencegahan kasus duplikat aktif pada `EmployeeSeparationService.InitiateSeparationAsync`.
  - Kalkulasi hak pesangon resmi (Uang Pesangon, Uang Penghargaan Masa Kerja / UPMK, Uang Penggantian Hak / UPH) secara authoritative melalui `SeveranceCalculationEngine` di backend berdasarkan PP 35/2021.
  - Penolakan manipulasi rumus dan nilai pesangon buatan dari client side.
  - Alur persetujuan finansial terpadu oleh Direktur Keuangan / Finance Manager yang tercatat dalam log audit.
  - Integrasi pembentukan record separation dan relasi ke checklist exit clearance unit.

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `EmployeeRelationManagement`
* **Prefix Registry**: `Hrd` / `Trx`
* **Klasifikasi Kode**: `BUSINESS LOGIC & FINANCIAL COMPLIANCE`
* **Wewenang**: Penguatan logika pemutusan hubungan kerja dan kepatuhan perhitungan pesangon.
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung.

---

## 3. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Pemutusan hubungan kerja karena pelanggaran disiplin berat, efisiensi organisasi, atau putusan pengadilan hubungan industrial (PHI) merupakan tindakan berisiko hukum tinggi bagi rumah sakit:
1. **Authoritative Severance Engine**: Nilai pesangon tidak boleh dihitung manual di browser atau disuntikkan sembarangan. Backend wajib menjadi penentu tunggal besaran hak berdasarkan upah pokok resmi terakhir, tunjangan tetap, dan total masa kerja pegawai.
2. **Otorisasi Direktur Keuangan**: Pembayaran pesangon nakes bernilai puluhan hingga ratusan juta rupiah wajib disetujui otorisasi keuangan tingkat tinggi sebelum status final ditetapkan.
3. **Pencegahan Kasus Ganda**: Satu nakes yang sedang dalam proses audit PHK tidak boleh dibuatkan kasus PHK kedua secara simultan.

---

## 4. Rincian Implementasi & Alur Teknis

### 4.1 Modifikasi Kode Sumber
Pada `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/EmployeeSeparationService.cs`:
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
- Endpoint kalkulasi pesangon `CalculateSeveranceAsync` memetakan komponen gaji dan masa kerja sesuai PP 35/2021 secara deterministik.

### 4.2 Endpoint Spesifikasi Swagger Terkait
| Method | Path | Tags | Deskripsi | Auth / Permission | Request Payload / Query | Response |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/employee-relations/separations` | `[Tags("HR Employee Separation")]` | Ambil daftar kasus pemberhentian terpaginasi | `Hrd.Separation.View` | `page`, `pageNumber`, `pageSize`, `separationType` | `ApiResponse<PagedResult<EmployeeSeparationDto>>` |
| `POST` | `/api/corporate/human-resource/employee-relations/separations` | `[Tags("HR Employee Separation")]` | Inisiasi kasus pemberhentian baru | `Hrd.Separation.Create` | `InitiateSeparationRequestDto` | `ApiResponse<EmployeeSeparationDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/separations/calculate-severance` | `[Tags("HR Employee Separation")]` | Hitung simulasi pesangon authoritative dari backend | `Hrd.Separation.View` | `CalculateSeveranceRequestDto` | `ApiResponse<SeveranceCalculationResultDto>` |
| `POST` | `/api/corporate/human-resource/employee-relations/separations/{id}/approve-finance` | `[Tags("HR Employee Separation")]` | Persetujuan nominal pesangon oleh Direktur Keuangan | `Hrd.Separation.ApproveFinance` | `ApproveFinanceRequestDto` | `ApiResponse<EmployeeSeparationDto>` |

---

## 5. Bukti Verifikasi Rekayasa
* **Kompilasi .NET**: `dotnet build QuilvianSystemBackend.csproj --no-restore` menghasilkan **0 Error(s)**.
* **Audit Kepatuhan Perhitungan**: Rumus perhitungan pesangon sepenuhnya dijalankan di engine backend.
* **Traceability Requirement**: Terpetakan pada `requirement-traceability.md` Bagian 1x (`S-C5-SEP`).
