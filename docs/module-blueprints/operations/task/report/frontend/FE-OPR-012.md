# FE-OPR-012 — Visualisasi Kalender Grid Timeline Ruang OK Lantai 2 & Buffer Sterilisasi Otomatis 45 Menit

- **TASK ID:** `FE-OPR-012`
- **TASK TYPE:** Implementasi Frontend Modul Operasi (Penjadwalan Kamar Bedah)
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 4 — integrasi kalender grid timeline horizontal, pemetaan slot occupied vs sterilization buffer 45 menit, interaktivitas klik slot kosong, dan pengujian unit
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `FRONTEND`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (Branch `HamzahV2`) & pelaporan di `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `OPS-DEC-038`, `OPS-DEC-039`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`
- **FILES INSPECTED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/operating-room-schedule-view.jsx`
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/operating-room-management/operating-room-case.service.js`
- **FILES CREATED / CHANGED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/components/operating-room-timeline-grid.jsx` (Komponen visualisasi timeline grid kamar bedah Lantai 2)
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/operating-room-management/operating-room-case.service.js` (Fungsi API `getOperatingRoomTimeline`)
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/operating-room-schedule-view.jsx` (Pemasangan timeline grid di atas formulir penetapan jadwal operasi)
  - `QuilvianSystemFrontendDev/tests/unit/operating-room-timeline.test.mjs` (Unit test baru 3/3 PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur modul kamar operasi (`OPS-DEC-038`, `OPS-DEC-039`):
1. **Visualisasi Linimasa Kamar Bedah**:
   - Seluruh kamar operasi di Lantai 2 (Kamar Bedah Mayor 1, Kamar Bedah Mayor 2, dan Kamar Bedah Obstetri/SC) disajikan dalam bentuk kalender grid linimasa horizontal dari pukul 07:00 hingga 21:00.
2. **Buffer Sterilisasi Otomatis (45 Menit)**:
   - Setiap operasi yang dijadwalkan secara otomatis diikuti oleh blok **Buffer Sterilisasi 45 Menit** berwarna abu-abu arsiran.
   - Slot ini terkunci secara visual dan sistem sehingga tidak dapat dipesan atau ditumpuk oleh kasus operasi lain.
3. **Interaktivitas Pemesanan Cepat**:
   - Slot kosong bertanda hijau muda "+ Tersedia" dapat diklik langsung oleh petugas untuk mengisi secara otomatis nama ruang bedah dan waktu mulai pada formulir penetapan jadwal di bawahnya.

---

## 2. Rincian Implementasi

1. **Komponen Grid Timeline (`operating-room-timeline-grid.jsx`):**
   - Pemilih tanggal dinamis dengan tombol Segarkan.
   - Sumbu waktu horizontal per jam (07:00 s.d. 20:00).
   - Pengelompokan baris per ruang OK di Lantai 2.
   - Pengenalan tiga jenis slot: `Occupied` (ungu), `SterilizationBuffer` (abu-abu bergaris), dan `Available` (hijau muda interaktif).
   - Legenda warna yang jelas di bagian bawah kartu.
2. **Konsumsi API (`operating-room-case.service.js`):**
   - Menambahkan fungsi `getOperatingRoomTimeline({ date, floorLocation })`.
   - Mengonsumsi endpoint backend `GET /api/v1/health-services/operating-room-management/rooms/timeline`.
3. **Pemasangan di Layar Jadwal (`operating-room-schedule-view.jsx`):**
   - Menghubungkan callback `onSelectSlot` ke pengisian formulir jadwal (`roomId`, `roomLabel`, `startAt`).
4. **Unit Test Terverifikasi (`operating-room-timeline.test.mjs`):**
   - Pengujian ketersediaan service API.
   - Validasi struktur slot Occupied dan durasi SterilizationBuffer tepat 45 menit.
   - Validasi pemilihan slot kosong.

---

## 3. Bukti Verifikasi & Acceptance Criteria

### Eksekusi Unit Test:
```bash
cd QuilvianSystemFrontendDev
node --import ./tests/helpers/register.mjs --test tests/unit/operating-room-timeline.test.mjs
```
Hasil:
```text
▶ FE-OPR-012: Visualisasi Kalender Grid Timeline Ruang OK Lantai 2 & Buffer Sterilisasi 45 Menit (OPS-DEC-038, OPS-DEC-039)
  ✔ AC-1: getOperatingRoomTimeline adalah fungsi async yang siap menerima date dan floorLocation (0.7045ms)
  ✔ AC-2: Struktur timeline response mengelompokkan slot Occupied dan SterilizationBuffer per ruang OK (0.198ms)
  ✔ AC-3: Slot Available memungkinkan pemilihan waktu mulai operasi (0.1163ms)
✔ FE-OPR-012: Visualisasi Kalender Grid Timeline Ruang OK Lantai 2 & Buffer Sterilisasi 45 Menit (OPS-DEC-038, OPS-DEC-039) (2.1028ms)
ℹ tests 3
ℹ suites 1
ℹ pass 3
ℹ fail 0
```

---

## 4. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh arsitektur penjadwalan kamar operasi, desain komponen responsif, dan kontrak backend `BE-OPR-012`.
