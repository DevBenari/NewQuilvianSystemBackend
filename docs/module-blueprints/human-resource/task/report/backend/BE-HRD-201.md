# Laporan Penyelesaian Task: BE-HRD-201 Gerbang Persetujuan 2-Tier C&B Supervisor & Penegakan Anti-Self-Approval

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-201`
* **Nama Task**: Gerbang Persetujuan 2-Tier C&B Supervisor & Penegakan Anti-Self-Approval (`HRS-W4`, `VR-HRS-07`, `VR-HRS-08`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IEmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs`

---

## 2. Latar Belakang & Kebutuhan Regulasi Tata Kelola Rumah Sakit
Surat naskah dinas resmi Rumah Sakit Quilvian membawa konsekuensi yuridis dan finansial bagi rumah sakit. Oleh karena itu, penerbitan dokumen resmi, khususnya dokumen yang menyangkut kompensasi finansial (Surat Keterangan Penghasilan, Surat Rekomendasi Finansial Bank), memerlukan sistem pengawasan berjenjang yang ketat:

1. **Alur Persetujuan 2-Tier Dokumen Finansial (`HRS-W4`, `VR-HRS-07`)**:
   - **Tier 1 (Pemeriksaan & Validasi C&B)**: Dilakukan oleh Supervisor Compensation & Benefits (`CbSupervisorApproveAsync`) untuk memverifikasi keabsahan data slip gaji, status potongan ketenagakerjaan, dan kepatuhan perpajakan. Status dokumen berubah menjadi `CbApproved`.
   - **Tier 2 (Otorisasi & Tanda Tangan Digital)**: Dilakukan oleh Manajer SDM atau Direktur RS (`FinalAuthorizeAndIssueAsync`). Dokumen tidak dapat diotorisasi sebelum disetujui pada Tier 1.
2. **Penegakan Anti-Self-Approval (`VR-HRS-08`)**:
   - Menghindari konflik kepentingan di mana seorang pejabat atau supervisor menyetujui surat dinas untuk dirinya sendiri.
   - Jika `authorizedSignerUserId` atau `cbSupervisorUserId` sama dengan `WorkforceProfile.UserAccount.Id` staf pemohon atau pembuat tiket, sistem **WAJIB MEMBLOKIR** tindakan tersebut seketika dengan pesan pelanggaran tata kelola (*governance violation*).

---

## 3. Skenario Riil Rumah Sakit
* **Skenario 1 (Alur Berjenjang Normal)**:
  1. drg. Siti Aminah (Dokter Gigi Poli Eksekutif) mengajukan Surat Keterangan Penghasilan melalui ESS.
  2. Sistem menetapkan status dokumen: `PendingCbApproval`.
  3. Ibu Rina (Supervisor C&B) meninjau antrean permohonan, mencocokkan data potongan pajak PPh 21, lalu menyetujui permohonan. Status dokumen berubah menjadi: `CbApproved`.
  4. Bpk. Hendro (Manajer SDM) menerima notifikasi bahwa dokumen telah `CbApproved`. Bpk. Hendro mengotorisasi dan menandatangani secara digital.
  5. Surat resmi ber-nomor diterbitkan dan siap diunduh oleh drg. Siti Aminah.
* **Skenario 2 (Pelanggaran Anti-Self-Approval Ditangkal)**:
  1. Ibu Rina (Supervisor C&B) mengajukan Surat Keterangan Penghasilan untuk keperluannya sendiri.
  2. Ibu Rina login ke modul manajemen HR dan mencoba menyetujui permohonannya sendiri pada Tier 1.
  3. Sistem mendeteksi `UserAccount.Id == cbSupervisorUserId`.
  4. Sistem menolak seketika: *"Pelanggaran Tata Kelola: Supervisor C&B dilarang keras menyetujui permohonan surat dinas untuk dirinya sendiri (VR-HRS-08 Anti-Self-Approval Enforced)."*
  5. Permohonan harus dialihkan kepada Supervisor C&B pengganti atau langsung diverifikasi oleh Manajer SDM.

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Document Issuance & Verification")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/cb-approve` | Tier 1: Persetujuan / Penolakan oleh Supervisor C&B (`VR-HRS-07`, `VR-HRS-08`) | `SuperAdmin`, `HRAdmin`, `HRManager`, `HRStaff` | `200 OK`, `400 Bad Request` |
| `POST` | `/api/v1/corporate/hrd/services/documents/{id}/authorize-issue` | Tier 2: Otorisasi & Penandatanganan Digital Pejabat Berwenang (`VR-HRS-08`) | `SuperAdmin`, `HRAdmin`, `HRManager` | `200 OK`, `400 Bad Request` |

### DTO Payload Persetujuan C&B (`CbSupervisorApprovalDto`):
```json
{
  "isApproved": true,
  "notes": "Rincian slip gaji final periode Oktober 2026 telah diverifikasi sesuai ledger penggajian. Rekomendasi: Disetujui."
}
```

### DTO Payload Otorisasi Final (`FinalAuthorizeAndIssueDto`):
```json
{
  "signerName": "Drs. Hendro Wibowo, M.M.",
  "signerTitle": "Manajer Sumber Daya Manusia RS Quilvian",
  "notes": "Dokumen resmi telah diotorisasi dan ditandatangani digital.",
  "customSecurityPin": null
}
```

---

## 5. Bukti Verifikasi & Keamanan
1. **Pemeriksaan Akun Berlapis**: Validasi `VR-HRS-08` membandingkan ID akun pengguna dari token klaim JWT (`User.FindFirstValue(ClaimTypes.NameIdentifier)`) dengan `WorkforceProfile.UserAccount.Id` dan `issuance.CreateBy`.
2. **Kepatuhan Regulasi Rumah Sakit**: Penegakan pemisahan wewenang (*Separation of Duties*) terpenuhi secara mutlak pada level backend logic.
3. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
