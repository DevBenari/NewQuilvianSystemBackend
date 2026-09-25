# Laporan Task Backend: BE-RWI-132 — Penyelarasan Seeder Evaluasi Awal MPP (Case Management)

## 1. Identitas Task
- **Task ID**: `BE-RWI-132`
- **Modul**: Pelayanan Kesehatan — Rawat Inap (*Inpatient Nursing Workspace*)
- **Sub-Modul**: Menu 1 — Pengkajian Pasien (*Patient Assessment*) — Sub-Menu 6: Evaluasi Awal MPP (*Case Management Initial Evaluation*)
- **Referensi Bisnis**: Standar KARS PAP 2.1 & TKRS, Quilvian V1 Evaluasi Awal (`evaluasi-awal/form/page.jsx`).
- **Status Database**: **Zero Migration** (Menggunakan tabel `TrxCaseManagementEvaluation` dan instrumen `CASE_MANAGEMENT_CHECKLIST`).
- **Status**: **SELESAI (100% Verified)**

---

## 2. Ringkasan Perubahan
1. **Pembaruan Definisi Baseline Instrumen `CASE_MANAGEMENT_CHECKLIST`**:
   - Berkas: `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs:738-756`.
   - Mengubah draft formulir menjadi versi definitif terstandarisasi KARS PAP 2.1:
     - `MPP_SCREENING`: 1. Identifikasi / Skrining Pasien
     - `MPP_PROBLEM`: 2. Identifikasi Masalah Pasien & Keluarga
     - `MPP_GOAL`: 3. Harapan / Sasaran Asuhan Manajer Pelayanan
     - `MPP_PLAN`: 4. Perencanaan Pelayanan & Kolaborasi Klinis
     - `MPP_SUPPORT`: 5. Dukungan Sosial & Sistem Keluarga
     - `MPP_FINANCIAL`: 6. Aspek Finansial & Jaminan Pembiayaan
     - `MPP_LEGAL`: 7. Aspek Legal & Etika Pelayanan
     - `MPP_DISCHARGE`: 8. Perencanaan Pemulangan (Discharge Planning)
   - Menyelesaikan review flag `G-07` dan mengadopsi struktur telaah dari V1.

---

## 3. Bukti Verifikasi
- Kompilasi: `dotnet build` bersih dengan 0 Error.
- Integrasi Layar: Terhubung langsung ke endpoint `/api/v1/inpatient-care/case-management-evaluations`.
