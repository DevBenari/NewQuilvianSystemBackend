# Laporan Penyelesaian Backend Task: BE-HRD-168

## 1. Identitas Task
* **ID Task**: `BE-HRD-168`
* **Nama Task**: Engine alur verifikasi & persetujuan 3-tier klaim restitusi dengan penegakan mutlak anti-self-approval (`DED-B8`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-34: Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam tata kelola pengeluaran keuangan rumah sakit, klaim reimbursement kesehatan mandiri memiliki risiko kecurangan (*fraud*) dan konflik kepentingan jika tidak diawasi dengan rantai persetujuan berjenjang. Sesuai konsensus bisnis `HRD-DEC-130`, ditetapkan alur persetujuan 3 tingkat (*3-Tier Approval Workflow*) yang transparan dan akuntabel:
1. **Tier 1 — Atasan Langsung / Kepala Ruangan / Supervisor (`SupervisorApproved`)**: Memvalidasi kewajaran pengajuan staf di unit kerjanya.
2. **Tier 2 — HR Compensation & Benefit Reviewer (`HrReviewed`)**: Memverifikasi keaslian dokumen fisik nota/kuitansi, keabsahan nomor izin faskes luar, dan kepatuhan plafon tahunan.
3. **Tier 3 — Manajer SDM / HR Manager Final Approval (`Approved`)**: Otorisasi pencairan dana dan pengesahan nominal akhir reimbursement.

### Penegakan Mutlak Anti-Self-Approval (`DED-B8`, `VR-DED-08`)
Pejabat rumah sakit (termasuk Kepala Ruangan, Kepala Instalasi, Dokter Penanggung Jawab, hingga Direktur) **dilarang keras menyetujui klaim restitusi miliknya sendiri**. Jika seorang kepala unit mengajukan klaim, alur persetujuan Tier-1 wajib dialihkan ke atasan langsung satu tingkat di atasnya (misal: Kepala Bidang Pelayanan Medis atau Direktur Medik).

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `SupervisorApprovalRequestDto`: Catatan persetujuan atasan langsung.
* `HrReviewRequestDto`: Catatan verifikasi berkas oleh tim HR C&B.
* `FinalApprovalRequestDto`: Nominal persetujuan final yang disahkan (`ApprovedAmount`) dan catatan otorisasi.
* `RejectClaimRequestDto`: Payload penolakan dengan penegakan validasi minimal 15 karakter audit (`VR-DED-22`).

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode:
1. `ApproveBySupervisorAsync`:
   - Memeriksa identitas penjamin: `claim.EmployeeId == supervisorUserId || claim.CreateBy == supervisorUserId` $\rightarrow$ Tolak dengan `InvalidOperationException("Pelanggaran aturan Anti-Self-Approval (DED-B8)")`.
   - Mengubah status klaim menjadi `SupervisorApproved`.
2. `ReviewByHrAsync`:
   - Memastikan status asal adalah `SupervisorApproved`.
   - Mencegah reviewer HR memverifikasi klaimnya sendiri.
   - Mengubah status klaim menjadi `HrReviewed`.
3. `FinalApproveAsync`:
   - Memastikan status asal adalah `HrReviewed`.
   - Mencegah final approver menyetujui klaimnya sendiri.
   - Mengesahkan nominal `ApprovedAmount` dengan batas $\min(\text{request.ApprovedAmount}, \text{claim.ClaimAmount}, \text{sisa kuota})$.
   - Jika klaim non-cito regular, secara otomatis membentuk entitas `TrxPayrollRestitution` berstatus `Approved` dan memindahkan status klaim ke `QueuedForPayroll`.
4. `RejectClaimAsync`:
   - Menolak klaim pada tier mana pun dan mencatat alasan penolakan resmi minimal 15 karakter (`VR-DED-22`).

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitution Claims")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/claims/{id}/approve-supervisor` | Persetujuan Tier-1 oleh atasan langsung (Anti-Self-Approval) | Bearer JWT | `id: Guid`, Body DTO | `RestitutionClaimResponseDto` |
| `POST` | `/api/hr-deductions/claims/{id}/review-hr` | Verifikasi dokumen Tier-2 oleh HR C&B Reviewer | Bearer JWT | `id: Guid`, Body DTO | `RestitutionClaimResponseDto` |
| `POST` | `/api/hr-deductions/claims/{id}/approve-final` | Pengesahan final Tier-3 oleh HR Manager & trigger payroll | Bearer JWT | `id: Guid`, Body DTO | `RestitutionClaimResponseDto` |
| `POST` | `/api/hr-deductions/claims/{id}/reject` | Penolakan klaim berjenjang dengan alasan audit $\ge 15$ karakter | Bearer JWT | `id: Guid`, Body DTO | `RestitutionClaimResponseDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Penegakan Anti-Self-Approval Kepala Ruangan ICU**:
  - Ns. Budi (Kepala Ruangan ICU) mengajukan klaim kacamata sebesar Rp 1.500.000.
  - Ns. Budi mencoba membuka portal approval supervisor dan menekan tombol *Setujui*.
  - **Hasil Proteksi Sistem**: Sistem seketika menolak permintaan dan memblokir aksi approval dengan pesan: *"Pelanggaran aturan Anti-Self-Approval: Atasan dilarang keras menyetujui klaim restitusi miliknya sendiri (DED-B8)."* Klaim Ns. Budi secara otomatis dialihkan ke antrean persetujuan Kepala Bidang Keperawatan.
