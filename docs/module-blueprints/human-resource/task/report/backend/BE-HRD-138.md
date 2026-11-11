# Laporan Implementasi Task Backend: BE-HRD-138

## 1. Identitas Task
* **ID Task**: `BE-HRD-138`
* **Nama Task**: Workflow Otorisasi Eskalasi Khusus Direktur Pelayanan Medik Darurat Cito Melampaui Kuota PP 35/2021
* **Modul / Sub-Modul**: Human Resource / Sub-modul 09 — Administrasi Lembur Pegawai Rumah Sakit (Overtime Management)
* **Slice / Milestone**: `Slice S-B3` · `Fase MVP-28`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Ringkasan Eksekutif & Proses Bisnis Rumah Sakit
Peraturan Pemerintah (PP) No. 35 Tahun 2021 menetapkan batas maksimal waktu kerja lembur adalah 4 jam dalam 1 hari dan 18 jam dalam 1 minggu. Namun, dalam pelayanan rumah sakit, situasi kritis penyelamatan nyawa pasien (*life-saving emergency*)—seperti operasi berdurasi panjang komplikasi ganda atau bencana massal (*mass casualty incident*)—dapat memaksa dokter dan perawat bekerja melebihi kuota reguler tersebut.

Sesuai piagam tata kelola klinis Quilvian:
1. Pelanggaran kuota lembur biasa adalah terlarang keras, namun dispensasi darurat medis diakui sah secara hukum rumah sakit HANYA jika memperoleh otorisasi eksplisit dari **Direktur Pelayanan Medik** (`HRD-DEC-117`, `OT-W1`, `VR-OT-12`).
2. Sebelum memberikan otorisasi dispensasi, Direktur Medis disajikan data agregasi beban kerja nakes selama 7 hari terakhir (*Past 7 Days Rolling Work & Overtime Hours*) dari integrasi presensi Sub-07 dan realisasi lembur Sub-09 (`VR-OT-24`).
3. Sistem menyertakan peringatan risiko kelelahan (*Fatigue Risk Indicator*) apabila total jam kerja 7 hari terakhir staf mencapai $\ge 60$ jam.
4. Direktur Pelayanan Medik yang menyetujui dispensasi wajib menetapkan waktu istirahat wajib minimal (*Mandatory Rest Period*, default 11 jam) bagi staf sebelum shift berikutnya dapat dijadwalkan kembali.
5. Apabila Direktur Medis menolak dispensasi, klaim langsung ditutup dengan status `Rejected` (0 jam kompensasi berbayar).

---

## 3. Komponen Teknis yang Diimplementasikan
1. **Interface Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/IMedicalDirectorOvertimeEscalationService.cs`
2. **Implementasi Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/MedicalDirectorOvertimeEscalationService.cs`
   - Melakukan agregasi data presensi `HrdAttendanceDaily` dan realisasi lembur `TrxOvertimeRealization` untuk 7 hari sebelum tanggal dinas lembur.
   - Mengubah status klaim, mencatat nama dan ID Direktur penanggung jawab klinis, alasan dispensasi, dan alokasi jam istirahat wajib.
3. **Integrasi Controller & Persetujuan**:
   - `OvertimeApprovalController.cs`:
     - `GET /api/v1/corporate/human-resource/overtime/approvals/medical-director-inbox`
     - `POST /api/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-approve`
     - `POST /api/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-reject`
   - Tag Swagger: `[Tags("HR Overtime - Approvals & Escalations")]`.
4. **DTO Kontrak Data**:
   - `MedicalDirectorOvertimeEscalationDto`, `ApproveMedicalDirectorEscalationCommand`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)
* **Tag Swagger**: `[Tags("HR Overtime - Approvals & Escalations")]`

| HTTP Method | Path Endpoint | Deskripsi Bisnis | Otorisasi / Role | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/human-resource/overtime/approvals/medical-director-inbox` | Mengambil daftar eskalasi lembur darurat cito yang melampaui kuota PP 35/2021 beserta metrik jam kerja 7 hari terakhir | Direktur Pelayanan Medik (`OvertimeApproval.MedicalDirectorApprove`) | `200 OK` (PagedResult) |
| `POST` | `/api/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-approve` | Memberikan otorisasi dispensasi khusus darurat penyelamatan nyawa dan menetapkan jam istirahat wajib nakes | Direktur Pelayanan Medik (`OvertimeApproval.MedicalDirectorApprove`) | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `POST` | `/api/v1/corporate/human-resource/overtime/approvals/{id}/medical-director-reject` | Menolak permohonan dispensasi darurat medis cito dengan alasan resmi | Direktur Pelayanan Medik (`OvertimeApproval.MedicalDirectorApprove`) | `200 OK`, `404 Not Found` |

---

## 5. Bukti Verifikasi Aturan Bisnis (Acceptance Criteria)
* **VR-OT-12 (Medical Director Authority)**: Klaim darurat berlebih kuota ditahan pada status eskalasi dan tidak dapat disetujui oleh Kepala Ruangan atau HR Admin biasa.
* **VR-OT-24 (Past 7 Days Rolling Work Hours)**: Total jam kerja 7 hari terakhir dihitung akurat dari penggabungan presensi harian shift dan lembur yang telah diverifikasi. Peringatan fatigue risk otomatis dimunculkan bila $\ge 60$ jam.
* **Mandatory Rest Period Enforcement**: Setelah disetujui, catatan istirahat wajib minimal 11 jam tertanam di riwayat realisasi.
