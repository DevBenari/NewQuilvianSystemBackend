# FE-OPR-013 — Form Pencatatan Kelahiran Bayi Intraoperatif SC di Meja Bedah

- **TASK ID:** `FE-OPR-013`
- **TASK TYPE:** Implementasi Frontend Modul Operasi (Pelaksanaan Bedah SC)
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 4 — komponen modal form kelahiran bayi, integrasi service API, pemasangan di alur catatan pelaksanaan operasi, dan unit test verifikasi
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `FRONTEND`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (Branch `HamzahV2`) & pelaporan di `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `OPS-DEC-041`, `OPS-DEC-042`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`, `OPR-INT-008`
- **FILES INSPECTED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/operating-room-execution-view.jsx`
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/operating-room-management/operating-room-execution.service.js`
- **FILES CREATED / CHANGED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/components/operating-room-birth-event-section.jsx` (Komponen section & form input kelahiran bayi intraoperatif)
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/operating-room-management/operating-room-execution.service.js` (Fungsi API `recordOperatingRoomBirthEvent`)
  - `QuilvianSystemFrontendDev/src/components/view/health-services/operating-room-management/operating-room-case-view/operating-room-execution-view.jsx` (Pemasangan section pencatatan kelahiran di layar pelaksanaan tindakan)
  - `QuilvianSystemFrontendDev/tests/unit/operating-room-birth-event.test.mjs` (Unit test baru 3/3 PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur modul operasi (`OPS-DEC-041`, `OPS-DEC-042`):
1. Pada pelaksanaan operasi kasus kebidanan / persalinan (*Sectio Caesarea*), dokter operator obgyn atau perawat sirkuler mencatat kejadian kelahiran bayi langsung dari meja bedah.
2. Form input menyajikan parameter medis standar:
   - Waktu kelahiran akurat (`birthDateTime`).
   - Jenis kelamin bayi (`gender`).
   - Berat badan lahir (`weightGrams`) dan panjang badan (`lengthCentimeters`).
   - Lingkar kepala (`headCircumferenceCm`).
   - Skor APGAR menit ke-1, menit ke-5, dan menit ke-10.
   - Catatan resusitasi jalan napas, pemotongan tali pusat, dan kondisi umum neonatus.
3. Saat disimpan, backend secara otomatis menerbitkan pesan integrasi outbox ke modul Rawat Inap (`BE-RWI-184`) untuk membuat nomor rekam medis dan episode neonatus tertaut ke episode rawat inap sang ibu.

---

## 2. Rincian Implementasi

1. **Komponen Form Intraoperatif (`operating-room-birth-event-section.jsx`):**
   - Menampilkan badge visual "OBSTETRI / SECTIO CAESAREA".
   - Formulir validasi klinis dengan rentang batas medis aman.
   - Banner konfirmasi keberhasilan penyimpanan beserta informasi pengiriman pesan integrasi ke admisi bayi.
2. **Penyambungan API Service (`operating-room-execution.service.js`):**
   - Menambahkan method `recordOperatingRoomBirthEvent(caseId, payload)`.
   - Mengirimkan request ke endpoint `POST /cases/{caseId}/execution/birth-event`.
3. **Integrasi Layar Pelaksanaan (`operating-room-execution-view.jsx`):**
   - Komponen ditampilkan di atas catatan operasi setelah tindakan bedah dimulai.
4. **Unit Test Terverifikasi (`operating-room-birth-event.test.mjs`):**
   - Pengujian penolakan `caseId` kosong.
   - Validasi struktur parameter medis kelahiran.
   - Kepatuhan struktur DTO response.

---

## 3. Bukti Verifikasi & Acceptance Criteria

### Eksekusi Unit Test:
```bash
cd QuilvianSystemFrontendDev
node --import ./tests/helpers/register.mjs --test tests/unit/operating-room-birth-event.test.mjs
```
Hasil:
```text
▶ FE-OPR-013: Form Pencatatan Kelahiran Bayi Intraoperatif SC di Meja Bedah (OPS-DEC-041, OPS-DEC-042)
  ✔ AC-1: recordOperatingRoomBirthEvent melempar error bila caseId kosong (1.1132ms)
  ✔ AC-2: Struktur payload kelahiran meja bedah mencakup waktu lahir, APGAR, dan ukuran fisik bayi (0.175ms)
  ✔ AC-3: Format response pencatatan kelahiran valid dan menyertakan outbox integration confirmation (0.1386ms)
✔ FE-OPR-013: Form Pencatatan Kelahiran Bayi Intraoperatif SC di Meja Bedah (OPS-DEC-041, OPS-DEC-042) (2.4662ms)
ℹ tests 3
ℹ suites 1
ℹ pass 3
ℹ fail 0
```

---

## 4. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh standar antarmuka modul operasi dan kontrak backend `BE-OPR-014`.
