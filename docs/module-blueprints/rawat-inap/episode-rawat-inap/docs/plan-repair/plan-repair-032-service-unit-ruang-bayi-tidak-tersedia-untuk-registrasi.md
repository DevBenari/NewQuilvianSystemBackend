# PLAN-REPAIR-032 — Rencana Aktivasi Ketersediaan Registrasi Service Unit Ruang Bayi (SU-BABY-001) dan Unit Rawat Inap Khusus

```yaml
plan_id: PLAN-REPAIR-EPS-032
issue: ../issue/issue-032-service-unit-ruang-bayi-tidak-tersedia-untuk-registrasi.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-22"
diputuskan_oleh: "Pemilik Sistem"
tanggal_keputusan: "2026-10-22"
tanggal_selesai: "2026-10-22"
basis_source_backend: "6406fc76 (MHamzah)"
basis_source_frontend: "381ec25fa (HamzahV2)"
target_database: "QuilvianNewDevHamzah"
```

Dokumen ini memuat rencana perbaikan operasional basis data untuk mengatasi kegagalan HTTP 400 Bad Request (`Service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi`) saat pembentukan kunjungan pasien bayi baru lahir di Ruang Perinatologi dan NICU, dengan memperbarui bendera `IsAvailableForRegistration` menjadi `true` pada unit layanan rawat inap terkait di tabel `public."MstServiceUnit"`.

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Tindakan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- | :---: |
| `FIX-EPS-032-01` | `ISS-EPS-032` | Aktivasi bendera `IsAvailableForRegistration = true` pada unit `SU-BABY-001` (Ruang Bayi), `SU-NICU-001` (NICU), `SU-ICU-001` (ICU), dan `SU-HCU-001` (HCU) di DB `QuilvianNewDevHamzah` | DB / Master | P0 (Kritis) | `FIX-EPS-032` | ✅ SELESAI | 4 baris berhasil diupdate pada tabel `public."MstServiceUnit"`; nilai `IsAvailableForRegistration` terverifikasi `TRUE` pada 2026-10-22 10:09 WIB |

**Ringkasan: 1 dari 1 perbaikan selesai (100%).**

---

## 2. Solusi Terpilih per Temuan

### ISS-EPS-032 — Kegagalan Pembentukan Kunjungan Pasien Bayi (HTTP 400)

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A (Terpilih)** | **Aktivasi Bendera `IsAvailableForRegistration = true` pada Unit Layanan Terkait di DB**: Memperbarui tabel `public."MstServiceUnit"` agar unit rawat inap khusus (`Ruang Bayi`, `NICU`, `ICU`, `HCU`) secara resmi memenuhi syarat pendaftaran rawat inap (`InpatientRegistrationEligible`). | Mematuhi 100% aturan bisnis dan validasi backend yang sudah ada tanpa perlu mengubah kode C# atau build ulang backend; langsung memulihkan proses admisi bayi dan unit rawat inap intensif secara permanen. | Memerlukan eksekusi SQL DML via Planning Gate. |
| **B** | Mengubah backend `PatientEncounterController.cs` agar mengecualikan validasi `IsAvailableForRegistration` untuk kunjungan tipe Inpatient. | Tidak perlu menyentuh database. | **Ditolak tegas**: Melanggar kontrak keamanan dan integritas master data (`ServiceUnitRegistrationRules`); unit non-aktif bisa bocor ke transaksi klinis. |
| **C** | Mengubah seluruh kamar bayi (`BAYI-1`, `BAYI-2`, `Ruang NICU`) agar dipindahkan ke unit umum `SU-IPD-001` (Rawat Inap). | `SU-IPD-001` sudah memiliki `IsAvailableForRegistration = true`. | **Ditolak tegas**: Merusak pemisahan departemen rumah sakit (*bounded context* Ruang Bayi vs Rawat Inap Dewasa) dan mengacaukan pemisahan hak akses perawat per unit (*nurse station*). |

**Solusi Terpilih: Opsi A.**  
Opsi ini menyelesaikan akar masalah secara bersih dan sesuai dengan tata kelola master data rumah sakit.

---

## 3. Skrip SQL Terencana (Planning Gate)

Sesuai aturan konstitusi `manage-database-operations`, berikut kueri SQL DML terencana pada database **`QuilvianNewDevHamzah`**:

```sql
-- ============================================================================
-- PLANNING GATE: Aktivasi Ketersediaan Registrasi Unit Rawat Inap Khusus
-- Target: Database QuilvianNewDevHamzah (PostgreSQL 15.15)
-- Tujuan: Mengizinkan pembentukan kunjungan admisi pada Ruang Bayi, NICU, ICU, HCU
-- ============================================================================

BEGIN TRANSACTION;

-- Perbarui bendera IsAvailableForRegistration menjadi TRUE
UPDATE public."MstServiceUnit"
SET 
    "IsAvailableForRegistration" = TRUE,
    "UpdateDateTime" = NOW(),
    "UpdateBy" = '0ba84a1a-2559-49ba-a320-10fb1f399d70' -- SuperAdmin
WHERE "ServiceUnitCode" IN ('SU-BABY-001', 'SU-NICU-001', 'SU-ICU-001', 'SU-HCU-001')
  AND "IsDelete" = FALSE;

COMMIT;
```

---

## 4. Acceptance Criteria & Verifikasi

1. **Acceptance Criteria**:
   - `SELECT "ServiceUnitCode", "IsAvailableForRegistration" FROM public."MstServiceUnit" WHERE "ServiceUnitCode" = 'SU-BABY-001'` mengembalikan `true`.
   - `POST /api/v1/health-services/registration-management/patient-encounters/admin` dengan `serviceUnitId: "dea54fab-052c-4c87-8fb6-b9e916c50b2d"` mengembalikan `200 OK` (atau `201 Created`).
   - Petugas dapat memilih tempat tidur di kamar Perinatologi (`BAYI-1` / `BAYI-2`) maupun Ruang NICU tanpa terhadang banner merah error unit layanan.

2. **Verifikasi**:
   - Uji coba langsung pembentukan encounter admisi bayi baru lahir di peramban.

---

## 5. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-22 | Dokumen rencana perbaikan dibuat untuk aktivasi `IsAvailableForRegistration` pada `SU-BABY-001`, `SU-NICU-001`, `SU-ICU-001`, dan `SU-HCU-001`. Menunggu persetujuan eksekusi pemilik sistem. | `diagnose-module-issue` |
| 2026-10-22 | Eksekusi disetujui pemilik sistem dan selesai dijalankan (100% SELESAI). 4 unit rawat inap khusus berhasil diaktifkan untuk registrasi. | `build-module-backend` / DB Ops |
