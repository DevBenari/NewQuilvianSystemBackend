# Laporan Perubahan Frontend — `FE-HRD-075`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-075` |
| Judul | Service Client API Roster Period Kontrak v10 (roster-scheduling-service.js) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-18` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-097, HRD-DEC-098, API Contract v10 bagian 16, BE-HRD-094` |
| Contract version | API Contract v10 [Tags("HR Scheduling - Roster Period")] |
| Dependency | `BE-HRD-094 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/roster-scheduling-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Membangun lapisan klien API Axios yang menghubungkan seluruh siklus hidup penyusunan roster dinas instalasi 24/7 rumah sakit dengan endpoint backend resmi.

Task ini mengimplementasikan:
1. **Endpoint Manajemen Roster Period**:
   - `getRosterPeriodSummary`, `getRosterPeriods`, `getRosterPeriodById`, `createRosterPeriod`.
2. **Penyusunan Massal & Otomasi Staf**:
   - `populateEligibleStaff`: Mengisi otomatis daftar perawat unit beserta status pendampingan klinis.
   - `copyPreviousPeriod`: Menyalin seluruh matriks shift dari periode sebelumnya.
   - `applyShiftPattern`: Menerapkan pola rotasi berulang (P-S-M-L-L dsb.).
3. **Audit Keselamatan & Publikasi Mandiri**:
   - `validateRosterPeriod`: Memeriksa pelanggaran aturan blokir B1-B7 dan peringatan W1-W7.
   - `publishRosterPeriod`: Menerbitkan jadwal dinas mandiri oleh Kepala Unit dengan payload warningReasons.
   - `closeRosterPeriod`: Penguncian final periode oleh bagian HRD Payroll.
4. **Mutasi Pasca-Terbit & Koreksi Mundur**:
   - `directReplaceShift`: Penunjukan staf pengganti darurat langsung pasca-terbit.
   - `retroactiveCorrectShift`: Koreksi mundur jadwal maksimal 3 hari ke belakang.

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Seluruh endpoint roster terhubung dengan tipe payload yang sesuai kontrak v10 | ✅ PASS | RosterSchedulingService mengekspos 13 fungsi API client terstandarisasi. |
| 2 | Penanganan pesan 422 dan error response dipetakan seragam | ✅ PASS | Fungsi unwrap dan try-catch menangani payload error secara aman. |
| 3 | Kepatuhan standar arsitektur Quilvian | ✅ PASS | Menggunakan InstanceAxios terpusat tanpa bypass credential. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/lib/services/hr/roster-scheduling-service.js (Klien API Roster Period lengkap)`
