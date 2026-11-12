# Roadmap Repair 044 — Perbaikan Alur Admisi Bayi Baru Lahir Pasca-Pengujian

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | `RWI-BP-001` / `episode-rawat-inap` |
| tanggal | 2026-11-02 Asia/Jakarta |
| status / approval | Disetujui pemilik sistem pada 2026-11-02 ("oke kerjakan sesuai plan tersebut") |
| issue / rencana | [ISSUE-EPS-044](../docs/issue/issue-044-temuan-pengujian-admisi-bayi-baru-lahir.md) / [PLAN-REPAIR-EPS-044](../docs/plan-repair/plan-repair-044-temuan-pengujian-admisi-bayi-baru-lahir.md) |
| source SHA backend / frontend saat didaftarkan | `396c8363` (MHamzah) / `9c4000724` (HamzahV2) |
| scope / ID | 3 task: `BE-RWI-258`, `FE-RWI-287`, `FE-RWI-288`. Penomoran berlanjut dari [roadmap repair 043](roadmap-repair-043.md) |

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
BE-RWI-258 (Guard unit denah & dekomposisi galat pendaftaran bayi)
     │
     ├──► FE-RWI-287 (Pembersihan residu form, sinkronisasi pencarian, batas jam, & typo bonding)
     └──► FE-RWI-288 (Penyempurnaan pesan ketersediaan boks & refresh denah)
```

| Gelombang | Boleh mulai setelah | Task ID |
| ---: | --- | --- |
| 1 | — | `BE-RWI-258` |
| 2 | `BE-RWI-258` | `FE-RWI-287`, `FE-RWI-288` |

---

## Register Task

| Task ID / Status | Outcome | Menutup | Lapisan | Dependency | Acceptance Criteria | Laporan |
| --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-258` ✅ | Guard ketersediaan registrasi service unit pada kueri denah kamar admisi, dekomposisi pesan PostgresException, dan sinkronisasi urutan lahir terhadap MstPatient | `FIX-EPS-044-01` (BE), `FIX-EPS-044-02` (BE) | BE | — | Plan repair `FIX-EPS-044-01` & `02` AC backend | [BE-RWI-258.md](../task/report/backend/BE-RWI-258.md) |
| `FE-RWI-287` ✅ | Reset lengkap identitas bayi saat berganti ibu, sinkronisasi pembersihan pencarian server-side FilterSelect, pembatasan jam masa depan di picker, koreksi saltik bonding, dan reset idempotency key saat gagal | `FIX-EPS-044-02` (FE), `FIX-EPS-044-03`, `FIX-EPS-044-06`, `FIX-EPS-044-07`, `FIX-EPS-044-08` | FE | `BE-RWI-258` | Plan repair `FIX-EPS-044-03`, `06`, `07`, `08` AC | [FE-RWI-287.md](../task/report/frontend/FE-RWI-287.md) |
| `FE-RWI-288` ✅ | Pemisahan status ketersediaan boks bayi kamar ibu (tidak menuduh occupied bila boks kosong/tidak siap) dan penyegaran denah aktif saat error service unit | `FIX-EPS-044-04`, `FIX-EPS-044-05` | FE | `BE-RWI-258` | Plan repair `FIX-EPS-044-04` & `05` AC | [FE-RWI-288.md](../task/report/frontend/FE-RWI-288.md) |

---

## Kartu Task

### BE-RWI-258 — Guard ketersediaan unit registrasi dan dekomposisi galat registrasi bayi

| Hal | Isi |
| --- | --- |
| Status | ✅ SELESAI |
| Berkas | `InpBedOccupancyService.cs`, `NewbornPatientRegistrationService.cs` |
| Kontrak | Selaras kontrak `EvaluatePlacementEligibilityCore` dan `POST /patients/newborns` |
| Acceptance criteria | 1. Bed di bawah service unit yang tidak tersedia untuk registrasi tidak berstatus eligible pada denah admisi.<br>2. Error database Postgres ditangkap dan constraint unik diuraikan secara informatif.<br>3. Alokasi urutan kelahiran menyelaraskan `MstPatient` eksisting. |
| DoD | Update data master unit di `QuilvianNewDevMaster`; laporan `task/report/backend/BE-RWI-258.md` |

### FE-RWI-287 — Pemulihan residu formulir, sinkronisasi pencarian, batas jam, dan saltik istilah

| Hal | Isi |
| --- | --- |
| Status | ✅ SELESAI |
| Berkas | `use-inpatient-admission-patient.jsx`, `use-inpatient-newborn-admission.js`, `filter-select.jsx`, `inpatient-newborn-admission-utils.js`, `inpatient-newborn-admission-steps.jsx`, `inpatient-newborn-admission-constants.js` |
| Kontrak | Sesuai desain admisi bayi |
| Acceptance criteria | 1. Berganti ibu mengosongkan seluruh field bayi.<br>2. FilterSelect menutup dengan mereset filter pencarian server.<br>3. Picker jam membatasi jam maksimal untuk hari ini.<br>4. Teks istilah "bonding" terkoreksi.<br>5. Idempotency key direset saat gagal simpan. |
| DoD | `npm run lint:errors` PASS (0 errors), `npm run test:unit` PASS (24/24); laporan `task/report/frontend/FE-RWI-287.md` |

### FE-RWI-288 — Penyempurnaan status boks kamar ibu dan refresh denah

| Hal | Isi |
| --- | --- |
| Status | ✅ SELESAI |
| Berkas | `inpatient-newborn-admission-utils.js`, `inpatient-newborn-admission-steps.jsx`, `use-inpatient-admission-bed.jsx` |
| Kontrak | Sesuai desain rawat gabung |
| Acceptance criteria | 1. Boks kosong yang unavailable menampilkan informasi pemeliharaan/penataan, bukan tuduhan bayi kembar.<br>2. Denah aktif disegarkan saat banner penolakan service unit muncul. |
| DoD | `npm run lint:errors` PASS (0 errors), `npm run test:unit` PASS (24/24); laporan `task/report/frontend/FE-RWI-288.md` |
