# Laporan Task Frontend: FE-RWI-133 — UI & Safety Alerts Perencanaan Pulang (Discharge Planning)

## 1. Identitas Task
- **Task ID**: `FE-RWI-133`
- **Modul**: Pelayanan Kesehatan — Rawat Inap (*Inpatient Nursing Workspace*)
- **Sub-Modul**: Menu 1 — Pengkajian Pasien (*Patient Assessment*) — Sub-Menu 7: Perencanaan Pulang (*Discharge Planning*)
- **Komponen Utama**:
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/renderer/clinical-instrument-form-renderer.jsx`
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx`
- **Unit Test**: `QuilvianSystemFrontendDev/tests/unit/inpatient-discharge-planning-instruments.test.mjs`
- **Status**: **SELESAI (100% Verified, 5/5 Test Pass)**

---

## 2. Ringkasan Perubahan
1. **Clinical Safety Alerts Perencanaan Pulang**:
   - Menambahkan 3 peringatan keselamatan klinis real-time pada `clinical-instrument-form-renderer.jsx`:
     - **Peringatan Pemulangan Kompleks (Level: Warning)**: Muncul jika salah satu dari 6 kriteria skrining (usia > 65 tahun, psikiatri, kekerasan, mobilitas, rawat lanjut, ADL) terpenuhi. Memandu perawat berkoordinasi sejak awal dengan DPJP dan Manajer Pelayanan Pasien (MPP).
     - **Peringatan Keselamatan Pasien Tinggal Sendiri (Level: Warning)**: Muncul jika `DP_LIVING_ALONE = true` guna mencegah ketidakpatuhan terapi atau risiko jatuh di rumah tanpa pengawasan.
     - **Peringatan Peralatan Medis & Home Care (Level: Info)**: Muncul jika pasien memerlukan alkes medis di rumah atau perawatan home care guna memastikan edukasi keluarga telah tuntas sebelum admisi pulang.
2. **Kesesuaian Pengikatan Kolom (Zero Migration)**:
   - Kolom `DP_PLAN_STATUS_NOTE` terikat ke `NurseNote` pada entitas `TrxPatientAssessment`.
3. **Pengujian Otomatis**:
   - `inpatient-discharge-planning-instruments.test.mjs` lulus 100% (5/5 passing).

---

## 3. Bukti Verifikasi Pengujian
- Perintah: `node --test --import ./tests/helpers/register.mjs tests/unit/inpatient-discharge-planning-instruments.test.mjs`
- Hasil:
  ```text
  ✔ Instrumen DISCHARGE_PLANNING memuat 8 seksi komprehensif: Kriteria, Caregiver, Lingkungan Rumah, Alkes, Home Care, Transportasi, Rencana Kontrol, dan Resume
  ✔ Menghasilkan Peringatan Pemulangan Kompleks saat satu atau lebih kriteria skrining bernilai true
  ✔ Menghasilkan Peringatan Keselamatan saat DP_LIVING_ALONE bernilai true
  ✔ Menghasilkan Peringatan Alkes/Home Care saat DP_MED_EQUIP_USED atau DP_HOMECARE_NEEDED bernilai true
  ✔ Validasi kelengkapan mendeteksi ketidaklengkapan moda transportasi kepulangan
  ℹ tests 5 | suites 1 | pass 5 | fail 0
  ```
