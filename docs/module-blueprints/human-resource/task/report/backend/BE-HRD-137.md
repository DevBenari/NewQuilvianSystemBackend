# Laporan Penyelesaian Task Backend: BE-HRD-137

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-137` |
| **Nama Task** | Workflow persetujuan 2-tahap ruangan & pemblokiran mutlak self-approval (`OvertimeApprovalWorkflowService`) |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-120`, `02-backend-architecture.md` 22.8 (`OT-B5`), `contracts/validation-matrix.md` (`VR-OT-03`), `contracts/api-contract.md` `[Tags("HR Overtime - Approvals & Escalations")]` |

---

## 2. Deskripsi & Tujuan Bisnis

Sistem persetujuan lembur rumah sakit harus menjamin prinsip Good Corporate Governance dan pemisahan fungsi (*Segregation of Duties*). Tanpa kontrol yang memadai, pejabat struktural dapat menyetujui pengajuan lemburnya sendiri, yang berpotensi menimbulkan manipulasi jam kerja dan pemborosan anggaran rumah sakit.

Task ini mengimplementasikan alur kerja persetujuan bertingkat dua tahap yang dilengkapi dengan penjaga mutlak anti-self-approval:
1. **Persetujuan Operasional Bertingkat Dua Tahap (`HRD-DEC-120`)**:
   - **Tahap 1 (Operasional Ruangan)**: Dilakukan oleh Kepala Ruangan / Kepala Unit kerja untuk memvalidasi kebutuhan riil pelayanan.
   - **Tahap 2 (Verifikasi SDM)**: Dilakukan oleh HR Officer / Admin untuk verifikasi administratif sebelum diserahkan ke payroll.
2. **Pemblokiran Mutlak Persetujuan Sendiri (*Anti-Self-Approval* `VR-OT-03`, `OT-B5`)**: Sistem memblokir dengan HTTP 403 Forbidden bila aktor yang menekan tombol setujui adalah pegawai yang sama dengan pemohon lembur atau salah satu anggota tim pelaksana yang terdaftar (`ApproverUserId == RequestEmployeeUserId`).
3. **Eskalasi Hierarki Otomatis**: Jika Kepala Ruangan yang mengajukan lembur, SPKL otomatis dirutekan ke atasan hierarki di atasnya (Kepala Instalasi atau Wakil Direktur terkait).
4. **Kotak Masuk Otorisasi Eksekutif Direktur Pelayanan Medik**: Menyediakan antrean khusus bagi Direktur Medis untuk meninjau dan menerbitkan dispensasi khusus darurat penyelamatan nyawa (*life-saving emergency*) yang melampaui kuota PP 35/2021.

---

## 3. Komponen yang Dikerjakan

1. **DTOs (`OvertimeApprovalDtos.cs`)**:
   - `OvertimeApprovalInboxItemDto`: DTO kotak masuk persetujuan SPKL bawahan bagi atasan ruangan.
   - `ApproveOvertimeCommand` & `RejectOvertimeCommand`: DTO aksi persetujuan dengan catatan atau penolakan dengan alasan resmi.
   - `MedicalDirectorOvertimeEscalationDto`: DTO rincian eskalasi darurat medis cito ke pimpinan medis RS.
   - `ApproveMedicalDirectorEscalationCommand`: Command otorisasi dispensasi darurat medis beserta penetapan jam istirahat wajib nakes.
   - `OvertimeApprovalResultDto`: DTO hasil respon pemrosesan alur persetujuan.

2. **Interface & Service (`IOvertimeApprovalWorkflowService.cs` & `OvertimeApprovalWorkflowService.cs`)**:
   - `GetInboxAsync`: Kueri daftar SPKL bawahan berstatus `WaitingApproval` / `WaitingUnitApproval`.
   - `ApprovePlanAsync`: Validasi anti-self-approval `VR-OT-03`, transisi status bertahap ke `WaitingHrVerification` / `ApprovedForWork`.
   - `RejectPlanAsync`: Validasi anti-self-approval dan pembaharuan status ke `Rejected`.
   - `GetMedicalDirectorInboxAsync`: Kueri eskalasi darurat medis cito yang memerlukan dispensasi.
   - `ApproveMedicalDirectorEscalationAsync`: Otorisasi dispensasi resmi Direktur Medis dan penutupan flag eskalasi.

3. **Controller Bergaya Swagger (`OvertimeApprovalController.cs`)**:
   - Tag Swagger: `[Tags("HR Overtime - Approvals & Escalations")]`.
   - Rute ganda: `/api/v1/corporate/human-resource/overtime/approvals` dan `/api/overtime/approvals`.
   - Endpoint:
     - `GET /inbox`: Kotak masuk persetujuan atasan ruangan (`[AccessPermission("OvertimeApproval", "View")]`).
     - `POST /{id}/approve`: Persetujuan SPKL bawahan (`[AccessPermission("OvertimeApproval", "Approve")]`).
     - `POST /{id}/reject`: Penolakan SPKL bawahan (`[AccessPermission("OvertimeApproval", "Approve")]`).
     - `GET /medical-director-inbox`: Kotak masuk Direktur Medis (`[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]`).
     - `POST /{id}/medical-director-approve`: Otorisasi dispensasi Direktur Medis (`[AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]`).

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario: Pencegahan Self-Approval oleh Kepala Ruangan
* **Kondisi**: Kepala Ruangan IGD mengajukan SPKL untuk dirinya sendiri karena harus menggantikan dokter jaga yang mendadak sakit keras.
* **Tindakan**:
  - Kepala Ruangan mencoba menekan tombol setujui pada pengajuannya sendiri di sistem.
* **Respon Sistem**:
  - Sistem mendeteksi `plan.CreateBy == approverUserId` dan `ApproverUserId` terdaftar dalam detail staf lembur.
  - Sistem memblokir aksi dengan HTTP 403 Forbidden: *"Akses ditolak: Pejabat struktural dilarang menyetujui formulir lemburnya sendiri."* (`VR-OT-03`).
  - SPKL tetap berada pada status `WaitingApproval` dan hanya dapat disetujui oleh Kepala Instalasi Gawat Darurat atau Wakil Direktur Pelayanan Medik.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| Sistem memblokir dengan HTTP 403 Forbidden bila Approver adalah pemohon/pelaksana | ✅ Lolos | Diimplementasikan pada blok pengecekan `plan.CreateBy == approverUserId || plan.Details.Any(...)` mengembalikan pesan baku `VR-OT-03`. |
| Pengajuan bertransisi bertingkat `WaitingApproval` -> `WaitingHrVerification` / `ApprovedForWork` | ✅ Lolos | Diimplementasikan pada logika transisi status bertingkat di `ApprovePlanAsync`. |
| Endpoint kotak masuk dan otorisasi Direktur Medis tersedia | ✅ Lolos | Endpoint `/medical-director-inbox` dan `/{id}/medical-director-approve` terpasang dengan tag `[Tags("HR Overtime - Approvals & Escalations")]`. |
