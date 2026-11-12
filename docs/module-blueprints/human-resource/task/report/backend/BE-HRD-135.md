# Laporan Implementasi Task Backend: BE-HRD-135

## 1. Identitas Task
* **ID Task**: `BE-HRD-135`
* **Nama Task**: Service Klaim Darurat Medis Cito Pasca-Dinas H+1 & Integrasi Nomor Rekam Medis / Insiden
* **Modul / Sub-Modul**: Human Resource / Sub-modul 09 — Administrasi Lembur Pegawai Rumah Sakit (Overtime Management)
* **Slice / Milestone**: `Slice S-B3` · `Fase MVP-28`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Ringkasan Eksekutif & Proses Bisnis Rumah Sakit
Dalam operasional rumah sakit 24/7, keadaan gawat darurat medis (cito) sering kali terjadi secara mendadak tanpa rencana sebelumnya—seperti operasi bedah darurat (*emergency surgery*), penanganan henti jantung di IGD/ICU, atau respons cepat tim kode biru (*Code Blue*). Tenaga kesehatan seperti dokter spesialis on-call, perawat anestesi, atau analis laboratorium siaga wajib segera bertindak menyelamatkan nyawa pasien tanpa sempat membuat Surat Perintah Kerja Lembur (SPKL) pra-dinas di muka.

Melalui implementasi `BE-HRD-135`:
1. Staf medis diberikan hak mengajukan klaim lembur darurat medis cito secara mandiri pasca-dinas dengan tenggat pelaporan maksimal H+1 hari kerja (`VR-OT-06`, `HRD-DEC-116`).
2. Setiap klaim darurat wajib mencantumkan Nomor Rekam Medis (RM) pasien atau nomor laporan insiden klinis beserta ringkasan tindakan darurat (`VR-OT-02`).
3. Sistem secara otomatis mengevaluasi kepatuhan kuota PP 35/2021 melalui guard `IOvertimeQuotaEnforcementGuard`. Apabila lembur darurat melampaui batas harian 4 jam atau mingguan 18 jam, sistem menandai klaim dengan bendera eskalasi `IsMedicalDirectorEscalation = true` dan mengarahkannya ke antrean otorisasi Direktur Pelayanan Medik (`HRD-DEC-117`, `VR-OT-12`).
4. Pejabat manajerial/struktural terlindungi oleh aturan *Overtime Exempt* sehingga tidak dapat memilih kompensasi finansial (`VR-OT-05`).

---

## 3. Komponen Teknis yang Diimplementasikan
1. **Interface Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/IOvertimeRealizationService.cs`
   - Mendefinisikan kontrak pengajuan realisasi, detail klaim, riwayat klaim mandiri pegawai, dan pembatalan klaim.
2. **Implementasi Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeRealizationService.cs`
   - Melakukan orkestrasi validasi bisnis, verifikasi relasi SPKL terencana atau klaim darurat cito, evaluasi kuota PP 35/2021, penomoran berurutan otomatis `OTR/YYYY/MM/XXXX`, serta penyimpanan entitas `TrxOvertimeRealization`.
3. **Controller API**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Controllers/OvertimeRealizationEssController.cs`
   - Rute ganda: `/api/v1/corporate/human-resource/overtime/realizations` & `/api/overtime/realizations`.
   - Tag Swagger: `[Tags("HR Overtime - Realizations & Cito Claims")]`.
4. **DTO Kontrak Data**:
   - `OvertimeRealizationModernDtos.cs`: `OvertimeRealizationResultDto`, `OvertimeRealizationDetailDto`, `OvertimeRealizationSummaryDto`, `CancelOvertimeRealizationCommand`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)
* **Tag Swagger**: `[Tags("HR Overtime - Realizations & Cito Claims")]`

| HTTP Method | Path Endpoint | Deskripsi Bisnis | Otorisasi / Role | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/overtime/realizations` | Mengajukan klaim realisasi lembur (terencana ber-SPKL atau darurat medis cito H+1) | Pegawai / Nakes Login (`OvertimeRealizationEss.Create`) | `201 Created`, `400 Bad Request`, `404 Not Found` |
| `GET` | `/api/v1/corporate/human-resource/overtime/realizations/{id}` | Mengambil detail lengkap klaim lembur beserta data klinis dan verifikator | Pegawai / Atasan / HR (`OvertimeRealizationEss.View`) | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/corporate/human-resource/overtime/realizations/my` | Mengambil riwayat klaim realisasi lembur yang pernah diajukan pegawai yang login | Pegawai Login (`OvertimeRealizationEss.View`) | `200 OK` (PagedResult) |
| `POST` | `/api/v1/corporate/human-resource/overtime/realizations/{id}/cancel` | Membatalkan klaim realisasi lembur yang belum diverifikasi HR | Pemohon Asli (`OvertimeRealizationEss.Update`) | `200 OK`, `400 Bad Request`, `404 Not Found` |

---

## 5. Bukti Verifikasi Aturan Bisnis (Acceptance Criteria)
* **VR-OT-02 (Mandatory Clinical Reference)**: Pengajuan darurat cito tanpa nomor RM pasien atau ringkasan tindakan ditolak sistem dengan pesan error validasi.
* **VR-OT-06 (Submission Deadline H+1)**: Pengajuan yang melewati H+1 hari kerja dari tanggal pelaksanaan lembur ditolak otomatis.
* **VR-OT-05 (Overtime Exempt)**: Staf berposisi manajerial yang memilih jenis kompensasi `FinancialPayout` diblokir sistem.
* **Format Penomoran**: Nomor realisasi terbentuk secara deterministik dan urut: `OTR/2026/11/0001`.
