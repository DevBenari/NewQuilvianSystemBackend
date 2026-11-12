# Laporan Perubahan Frontend — `FE-HRD-086`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-086` |
| Judul | Layar Pengaturan Kebijakan Penjadwalan & Waktu Kerja HR (FE-HRD-143, /hr/scheduling/settings) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-099, HRD-DEC-102, VR-SCH-80, BE-HRD-088` |
| Contract version | API Contract v10 [Tags("HR Scheduling - Settings")] |
| Dependency | `BE-HRD-088 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/scheduling-setting-service.js, src/components/view/hr/scheduling/scheduling-settings-view.jsx, src/app/hr/scheduling/settings/page.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penyediaan layar konfigurasi bagi Administrator HR untuk menetapkan parameter aturan waktu kerja rumah sakit:
1. **Service Client SchedulingSettingService**:
   - `getSetting`, `updateSetting` dengan parameter hospitalSiteId dan DTO UpdateSchedulingSettingRequest.
2. **Form Parameter Kebijakan**:
   - Jeda istirahat minimal antar shift (8–24 jam, default 11 jam).
   - Batas hari kerja berturut-turut (1–14 hari, default 6 hari).
   - Batas shift malam berturut-turut (1–7 hari, default 2 malam).
   - Toleransi jendela koreksi mundur (0–30 hari, default 3 hari).
   - Toggle eskalasi aturan ke tingkat fatal pemblokir (Blocking).
3. **Validasi Rentang Nilai (VR-SCH-80)**:
   - Validasi ketat di sisi klien sebelum data dikirimkan ke backend.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Admin HR dapat mengubah nilai parameter waktu kerja dengan validasi rentang | ✅ PASS | Pemeriksaan form pada SchedulingSettingsView menegakkan batas nilai VR-SCH-80. |
| 2 | Perubahan tersimpan dan menjadi acuan mesin validasi roster | ✅ PASS | Terhubung ke SchedulingSettingService.updateSetting. |
| 3 | Bebas Tailwind dan mematuhi aturan token Quilvian | ✅ PASS | 100% menggunakan CSS Module dan Base Component. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/lib/services/hr/scheduling-setting-service.js (Service Axios pengaturan penjadwalan)`
- `src/components/view/hr/scheduling/scheduling-settings-view.jsx (Layar form pengaturan waktu kerja)`
- `src/app/hr/scheduling/settings/page.jsx (Rute App Router pengaturan penjadwalan)`
