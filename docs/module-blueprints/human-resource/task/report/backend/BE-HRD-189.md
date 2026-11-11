# Laporan Penyelesaian Backend Task: BE-HRD-189

## 1. Identitas Task
* **ID Task**: `BE-HRD-189`
* **Nama Task**: Meja kerja verifikasi klaim sekunder, settlement santunan, penonaktifan benefit saat resign (`BEN-B7`, `VR-BEN-23`), dan pemblokiran Exit Clearance Sub-20 jika ada tunggakan medis (`BEN-B8`, `VR-BEN-24`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-38: Integrasi SIMRS Kasir, Buku Besar Plafon, Klaim COB & Kliring Exit Clearance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Pengelolaan tunjangan kesehatan tidak berhenti pada pengajuan klaim, melainkan membutuhkan mekanisme kontrol operasional dan kepatuhan finansial di tingkat administrasi HR dan tata kelola pemisahan pegawai (*offboarding*):

1. **Meja Kerja Verifikasi Klaim HR Admin (`FR-BEN-19`)**:
   - Tim HR Admin melakukan penelaahan atas keabsahan dokumen kuitansi, nomor SEP BPJS, diagnosis medis, dan pagu plafon yang masih tersedia.
   - Admin dapat menyetujui nominal klaim secara penuh, menyetujui sebagian dengan catatan rasionalisasi biaya, atau menolak klaim dengan alasan spesifik (misal: "Dokumen kuitansi tidak berstempel basah rumah sakit perujuk").
2. **Penyelesaian Santunan Klaim (*Claim Settlement*) (`FR-BEN-20`)**:
   - Setelah klaim disetujui, bagian keuangan/HR mencatat nomor referensi settlement transfer bank pembayaran santunan (`SettlementReference`) dan tanggal transfer (`SettledAt`).
   - Status klaim bertransisi menjadi `Settled`.
3. **Penonaktifan Otomatis Paket Manfaat Saat Pegawai Resign (`BEN-B7`, `VR-BEN-23`)**:
   - Jika pegawai mengundurkan diri, pensiun, atau kontraknya berakhir, seluruh kepesertaan asuransi (`TrxEmployeeInsuranceEnrollment`) dan tunjangan tanggungan non-BPJS dinonaktifkan tepat pada tanggal efektif berakhirnya hubungan kerja (*termination effective date*).
4. **Hard-Blocking Exit Clearance atas Tunggakan Ekses Medis (`BEN-B8`, `VR-BEN-24`)**:
   - Menghubungkan Sub-modul 12 dengan proses Exit Clearance di Sub-modul 20 (Offboarding).
   - Pegawai yang mengajukan pengunduran diri tidak dapat memperoleh persetujuan kelonggaran (*clearance*) jika masih memiliki catatan tagihan ekses medis yang belum diselesaikan di kasir RS Quilvian atau potongan payroll yang belum terlunasi.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxBenefitClaim.cs`
* Kolom terkait verifikasi & penyelesaian: `ApprovedAmount`, `RejectionReason`, `SettledAt`, `SettlementReference`, `VerifiedAt`, `VerifiedByUserId`.

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitInHouseAndClaimDtos.cs`
* `VerifyClaimRequestDto`: Payload persetujuan / penolakan klaim memuat keputusan (`Approved` / `Rejected`), nominal yang disetujui, dan alasan penolakan.
* `SettleClaimRequestDto`: Payload pencatatan transfer santunan memuat nomor referensi pembayaran bank dan tanggal transfer.
* `TerminateEmployeeBenefitsRequestDto`: Payload penonaktifan paket manfaat staf.
* `ExitClearanceMedicalCheckResponseDto`: Respon pengecekan apakah pegawai memiliki tagihan medis tertunggak sebelum menandatangani form pengunduran diri.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitClaimService.cs`
* `VerifyClaimAsync`: Validasi bahwa klaim berstatus `PendingVerification`, nominal disetujui tidak melebihi klaim yang diajukan, dan pencatatan audit log `[AUDIT-BEN-09]`.
* `SettleClaimAsync`: Validasi klaim berstatus `Approved` dan pencatatan audit log `[AUDIT-BEN-10]`.
* `TerminateEmployeeBenefitsAsync`: Penonaktifan kepesertaan aktif dan pemotongan mutasi saldo.
* `CheckExitClearanceOutstandingAsync`: Penelusuran data tagihan tertunggak untuk mengunci gerbang exit clearance jika `HasOutstandingBalance == true`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Verifikasi dan Pencairan Santunan Klaim**:
  - Klaim rawat inap Bidan Siti sebesar Rp 3.000.000 diverifikasi oleh HR Admin. Setelah memeriksa rincian kuitansi, biaya obat vitamin kosmetik sebesar Rp 200.000 tidak ditanggung sesuai SOP rumah sakit.
  - HR menyetujui nominal `ApprovedAmount = 2800000` dengan catatan "Biaya suplemen non-terapi tidak dijamin".
  - Bagian Finance mentransfer dana Rp 2.800.000 ke rekening Bidan Siti dengan nomor batch pembayaran `TRF-BCA-20261109-8871`.
  - HR menginput nomor referensi tersebut, dan klaim berstatus `Settled`.

* **Skenario 2 — Pencegahan Pengunduran Diri Sebelum Melunasi Ekses Medis (Exit Clearance)**:
  - Staf Farmasi Andi mengajukan pengunduran diri.
  - Saat sistem Offboarding Sub-20 memanggil endpoint `/clearance-check/{workforceProfileId}`, terdeteksi bahwa Andi masih memiliki ekses tagihan operasi gigi sebesar Rp 850.000 yang belum dilunasi di kasir SIMRS.
  - Endpoint mengembalikan:
    ```json
    {
      "isCleared": false,
      "outstandingAmount": 850000.00,
      "blockingReason": "Staf masih memiliki tagihan ekses fasilitas medis RS Quilvian sebesar Rp 850.000,00. Harap lunasi di Loket Kasir Pegawai atau setujui potong pesangon."
    }
    ```
  - Petugas Exit Clearance mengunci formulir Andi sampai tagihan dilunasi atau dialihkan ke pemotongan pesangon akhir kerja.

---

## 5. Spesifikasi Endpoint REST API
Grup Swagger Tag: `[Tags("HR Benefits - COB Claims & Settlement")]`

| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/hr/benefits/claims` | Daftar seluruh klaim dengan filter status & tanggal | HR Admin / Manajer |
| `POST` | `/api/v1/hr/benefits/claims/{claimId}/verify` | Menyetujui atau menolak pengajuan klaim COB | HR Admin / Manajer |
| `POST` | `/api/v1/hr/benefits/claims/{claimId}/settle` | Mencatat nomor referensi transfer santunan bank | HR Admin / Finance |
| `POST` | `/api/v1/hr/benefits/claims/employee/{workforceProfileId}/terminate-benefits` | Penonaktifan tunjangan saat pegawai berhenti bekerja | HR Admin / Manajer |
| `GET` | `/api/v1/hr/benefits/claims/employee/{workforceProfileId}/exit-clearance-check` | Pengecekan tunggakan medis untuk Exit Clearance | Sistem Offboarding / HR |

---

## 6. Verifikasi & Integritas Bukti
* **Integritas Alur Kerja**: Alur status klaim `PendingVerification` $\rightarrow$ `Approved` $\rightarrow$ `Settled` atau `Rejected` divalidasi ketat tanpa kemungkinan jalan pintas (*no illegal state jump*).
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-15` s.d. `AT-HRD-BEN-18`.