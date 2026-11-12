# Laporan Penyelesaian Task: BE-HRD-126

## 1. Ikhtisar Task
* **ID Task:** `BE-HRD-126`
* **Nama Task:** Workflow pemanggilan cuti darurat (Leave Recall) & otorisasi HR Manager (`LeaveRecallWorkflowService`)
* **Slice / Milestone:** `S-B2` (Administrasi Cuti & Izin Pegawai Rumah Sakit) / `MVP-26`
* **Status:** ✅ Selesai
* **Tanggal Selesai:** 2026-11-05
* **Pengembang:** Antigravity AI Agent & Human Resource Technical Team
* **Target Arsitektur:** Sub-Modul 08 (Leave Management) — Alur Pemanggilan Dinas Darurat Rumah Sakit

---

## 2. Latar Belakang & Kebutuhan Rumah Sakit
Dalam situasi darurat operasional rumah sakit (seperti lonjakan pasien massal/*mass casualty*, wabah penyakit menular, atau kelangkaan tenaga medis kritis), manajemen rumah sakit memerlukan mekanisme resmi pemanggilan kembali (*Leave Recall*) pegawai yang sedang menjalani cuti aktif:
1. **Inisiasi Berjenjang (`HRD-DEC-113`):** Usulan pemanggilan hanya boleh diinisiasi oleh Kepala Ruangan / Kepala Unit yang mengetahui secara langsung kondisi defisit ketenagaan di unitnya.
2. **Kondisi Cuti Sah (`VR-LEV-14`):** Pemanggilan darurat hanya boleh diajukan untuk permohonan cuti yang statusnya sedang `Approved` atau `Taken` (cuti aktif).
3. **Pemberian Otorisasi Tunggal HR Manager (`LEV-B5`, `VR-LEV-15`):** Menutup celah tata kelola `HRD-Q-36`, persetujuan resmi atau penolakan tiket recall hanya boleh dieksekusi oleh pejabat berwenang dengan hak akses peran HR Manager (`[AccessPermission("LeaveRecall", "Approve")]`).

---

## 3. Komponen yang Dibuat & Diperbarui

### 3.1 Service `LeaveRecallWorkflowService.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRecallWorkflowService.cs`
* **Metode Utama:**
  1. `InitiateRecallAsync`:
     - Memvalidasi bahwa permohonan cuti berstatus `Approved` atau `Taken` (`VR-LEV-14`).
     - Memvalidasi tanggal efektif recall berada dalam rentang pelaksanaan cuti (`StartDate <= EffectiveRecallDate <= EndDate`).
     - Menghitung sisa hari cuti yang dipulihkan (`RestoredDays = EndDate - EffectiveRecallDate + 1`, `VR-LEV-16`).
     - Mencatat tiket `TrxLeaveRecallRequest` dengan status awal `RequestedByHeadUnit`.
  2. `ApproveRecallAsync`:
     - Memverifikasi otorisasi aktor sebagai HR Manager; menolak dengan `403 Forbidden` jika aktor tidak berwenang (`LEV-B5`, `VR-LEV-15`).
     - Mengubah status tiket recall menjadi `ApprovedByHrManager`.
  3. `RejectRecallAsync`:
     - Otorisasi HR Manager untuk menolak usulan pemanggilan darurat dengan mencatat alasan operasional `RejectionReason`.

### 3.2 Controller `LeaveRecallWorkflowController.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Controllers/LeaveRecallWorkflowController.cs`
* **Tag Swagger:** `[Tags("HR Leave - Recall Management")]`
* **Endpoint:**
  - `GET /api/v1/corporate/human-resource/leave/recalls` & `/api/leave/recalls`
  - `POST /api/v1/corporate/human-resource/leave/recalls` & `/api/leave/recalls`
  - `POST /api/v1/corporate/human-resource/leave/recalls/{id}/approve` & `/api/leave/recalls/{id}/approve`
  - `POST /api/v1/corporate/human-resource/leave/recalls/{id}/reject` & `/api/leave/recalls/{id}/reject`

---

## 4. Kepatuhan Aturan Bisnis & Validasi
* **`HRD-DEC-113` & `LEV-B5`:** Menutup celah keamanan `HRD-Q-36` melalui otorisasi persetujuan tunggal oleh HR Manager.
* **`VR-LEV-14`:** Inisiasi recall hanya berlaku untuk permohonan cuti aktif.
* **`VR-LEV-15`:** Penolakan otorisasi non-HR Manager dengan `403 Forbidden`.

---

## 5. Kesimpulan
Workflow pemanggilan cuti darurat rumah sakit telah beroperasi secara berjenjang dari Kepala Ruangan ke HR Manager dengan kontrol hak akses yang ketat.
