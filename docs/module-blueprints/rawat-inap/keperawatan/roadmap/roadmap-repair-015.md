# Roadmap Repair 015 — Tampilan EWS, Tab Monitoring EWS, dan Retur Obat dari e-MAR

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `keperawatan` |
| tanggal | 2026-11-02 Asia/Jakarta |
| status / approval | Disetujui pemilik sistem pada 2026-11-02 ("oke sekarang kerjakan semua task tersebut dan perbaiki"); K-04 diputuskan pada hari yang sama: perawat mengajukan, apoteker memeriksa, depo dipilih di form |
| issue / rencana | [ISSUE-KEP-015](../docs/issue/issue-015-kalkulasi-ews-pemesanan-diet-gizi-dan-retur-obat.md) / [PLAN-REPAIR-KEP-015](../docs/plan-repair/plan-repair-015-kalkulasi-ews-pemesanan-diet-gizi-dan-retur-obat.md) |
| source SHA backend / frontend saat didaftarkan | `396c8363` (MHamzah) / `9c4000724` (HamzahV2) |
| scope / ID | 4 task: `FE-RWI-283`, `BE-RWI-255`, `FE-RWI-284`, `FE-RWI-285`. ID tertinggi sebelum repair ini `BE-RWI-254` dan `FE-RWI-282`. Task `BE-RWI-256`, `BE-RWI-257`, dan `FE-RWI-286` dipakai [roadmap repair 043](../../episode-rawat-inap/roadmap/roadmap-repair-043.md). Berikutnya `BE-RWI-258` dan `FE-RWI-287` |

---

## Arti Tanda Status

| Tanda | Arti |
| :---: | --- |
| ✅ | Selesai dan acceptance criteria terbukti pada laporan task |
| 🟡 | Implementasi sebagian atau bukti belum lengkap |
| ⛔ | Terblokir oleh prasyarat yang belum terpenuhi |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

```text
FE-RWI-283   tanpa dependency
BE-RWI-255 ✅ ──> FE-RWI-284
FE-RWI-285   tanpa dependency teknis (API Retur Obat Farmasi sudah ada)
```

| Gelombang | Boleh mulai setelah | Task ID |
| ---: | --- | --- |
| 1 | — | `FE-RWI-283`, `BE-RWI-255`, `FE-RWI-285` |
| 2 | `BE-RWI-255` | `FE-RWI-284` |

---

## Register Task

| Task ID / Status | Outcome | Menutup | Lapisan | Dependency | Acceptance criteria | Laporan |
| --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-283` | Golongan, warna, dan anjuran EWS di tab Vital Sign dan Pengawasan Harian mengikuti field server | `FIX-KEP-015-07` (`ISS-KEP-015-T2`, `T3`) | FE | — | Plan repair `FIX-KEP-015-07` AC 1-6 | — |
| `BE-RWI-255` ✅ | Endpoint `GET .../monitoring/ews-alerts`: pasien rawat inap aktif per bangsal yang tanda vital terakhirnya berisiko tinggi/kritis atau perlu lapor dokter | `FIX-KEP-015-04` (`ISS-KEP-015-01`) | BE | — | Plan repair `FIX-KEP-015-04` AC 1-5 | [BE-RWI-255.md](../task/report/backend/BE-RWI-255.md) |
| `FE-RWI-284` | Tab Monitoring "EWS Perlu Lapor Dokter" | `FIX-KEP-015-04` (`ISS-KEP-015-01`) | FE | `BE-RWI-255` | Plan repair `FIX-KEP-015-04` AC 1-5 dari sisi layar | — |
| `FE-RWI-285` | Aksi "Ajukan Retur" pada e-MAR yang membuat dan mengajukan draf retur Farmasi | `FIX-KEP-015-03` (`ISS-KEP-015-03`) | FE | — | Plan repair `FIX-KEP-015-03` AC 1-5 | — |

---

## Kartu Task

### FE-RWI-283 — Tampilan EWS mengikuti server

| Hal | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Berkas | `nursing-workspace/sections/daily-monitoring/daily-monitoring-section.jsx`, `nursing-workspace/sections/nursing-care/tabs/nursing-vital-sign-tab.jsx` |
| Kontrak | `PatientVitalSignSeriesItem` (`EarlyWarningScore`, `EwsRiskLevel`); konstanta `EWS_RISK_LABEL`/`EWS_RISK_TONE` yang sudah ada |
| Acceptance criteria | Plan repair `FIX-KEP-015-07` AC 1-6 |
| DoD | `npm run lint:errors`, `npm run test:unit`, `npm run build`; laporan `task/report/frontend/FE-RWI-283.md` |

### ✅ BE-RWI-255 — Endpoint monitoring EWS per bangsal

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI 9 November 2026.** Source sudah masuk commit `da737fb9` (2 November 2026); kelima AC `FIX-KEP-015-04` terpetakan ke `GetEwsAlertsPagedAsync` dan `GetEwsAlerts`; `dotnet build` 0 error / 12 warning (paket NuGet lama); tanpa perubahan database. Uji manual dengan data uji dijalankan bersama `FE-RWI-284` karena DB dev belum punya tanda vital rawat inap. [Laporan](../task/report/backend/BE-RWI-255.md) |
| Berkas | `Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs`, query monitoring, DTO monitoring |
| Kontrak | `GET api/v1/health-services/inpatient-management/monitoring/ews-alerts`, `InpatientMonitoring : Read`, query `ServiceUnitId`, `MinimumRiskLevel`, `PageNumber`, `PageSize`, respons `PagedResult` |
| Acceptance criteria | Plan repair `FIX-KEP-015-04` AC 1-5 |
| DoD | `dotnet build` pada project aplikasi; laporan `task/report/backend/BE-RWI-255.md` |

### FE-RWI-284 — Tab Monitoring "EWS Perlu Lapor Dokter"

| Hal | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Berkas | `inpatient-management/monitoring/**`, `inpatient-monitoring-constants.jsx`, service monitoring |
| Kontrak | Hasil `BE-RWI-255` |
| Acceptance criteria | Plan repair `FIX-KEP-015-04` AC 1-5 dari sisi layar; rangka plan repair bagian 3.C |
| DoD | `npm run lint:errors`, `npm run test:unit`, `npm run build`; laporan `task/report/frontend/FE-RWI-284.md` |

### FE-RWI-285 — Ajukan Retur dari e-MAR

| Hal | Isi |
| --- | --- |
| Status | Belum dikerjakan |
| Berkas | `nursing-workspace/sections/medication/**`; service `pharmacy-management/drug-return.service.js` yang sudah ada |
| Kontrak | `POST api/v1/health-services/pharmacy-management/drug-returns` (`DrugReturn : Create`) dan `POST .../{id}/submit` (`DrugReturn : Update`) |
| Keputusan | K-04 (2026-11-02): perawat mengajukan, apoteker memeriksa, depo penerima dipilih di form. Pemberian hak `DrugReturn : Create`/`Update` pada peran perawat dilakukan admin lewat layar Akses Role, bukan oleh task ini |
| Acceptance criteria | Plan repair `FIX-KEP-015-03` AC 1-5 |
| DoD | `npm run lint:errors`, `npm run test:unit`, `npm run build`; laporan `task/report/frontend/FE-RWI-285.md` |
