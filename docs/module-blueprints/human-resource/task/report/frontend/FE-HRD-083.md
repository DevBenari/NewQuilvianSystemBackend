# Laporan Perubahan Frontend — `FE-HRD-083`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `FE-HRD-083` |
| Judul | Service Client API Penugasan Siaga Medis On-Call & Evaluasi Respon SLA Cito (on-call-service.js) |
| Slice | `S-B4 (Penjadwalan, Roster Dinas 24/7, & On-Call) · MVP-20` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian S-B4 |
| Trace | `HRD-DEC-103, API Contract v10 bagian 16.2, BE-HRD-102` |
| Contract version | API Contract v10 [Tags("HR Scheduling - On-Call Management")] |
| Dependency | `BE-HRD-102 (✅ Selesai)` |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/services/hr/on-call-service.js` |
| Branch frontend | `HamzahV2` |
| Tanggal | 09 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Membangun klien API Axios untuk pengelolaan jadwal staf siaga on-call 24/7 dan pencatatan tiket pemanggilan darurat cito:
1. **Klien API Penugasan Siaga**:
   - `getOnCallAssignments`, `getOnCallAssignmentById`, `createOnCallAssignment`.
2. **Siklus Hidup Status Siaga**:
   - `confirmOnCallAssignment`: Konfirmasi kesediaan staf siaga.
   - `activateOnCallAssignment`: Aktivasi status siaga aktif ("Active").
   - `completeOnCallAssignment`: Penyelesaian penugasan siaga.
3. **Perekaman Pemanggilan Cito & SLA Respon**:
   - `recordCallIn`: Mencatat waktu kontak telepon, waktu kedatangan di RS, dan evaluasi pencapaian target SLA menit.
4. **Integrasi Lintas Modul**:
   - `getOnCallCompensationSummary`: Data kompensasi untuk Payroll dan Lembur (INT-HR-38).
   - `getActiveDoctors`: Integrasi dokter siaga aktif untuk Laboratorium (LAB-DEC-111).

---

## 1. Bukti Verifikasi & Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi pada Source Code |
| :-: | :--- | :---: | :--- |
| 1 | Seluruh endpoint on-call terhubung stabil dengan kontrak v10 | ✅ PASS | OnCallService menyediakan 9 fungsi API client terintegrasi. |
| 2 | Response data memuat atribut SLA dan status penugasan | ✅ PASS | Payload call-in mengembalikan indikator isSlaMet dan responseMinutes. |
| 3 | Bebas Tailwind dan mematuhi arsitektur Quilvian | ✅ PASS | Menggunakan InstanceAxios standar. |

---

## 2. Rincian Berkas yang Dibuat / Diubah

- `src/lib/services/hr/on-call-service.js (Service Axios On-Call lengkap)`
