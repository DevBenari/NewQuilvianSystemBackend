# FE-RWI-281 — Integrasi Formasi Tri-Dokter, Maternitas SC, & Boks Bayi Rawat Gabung pada Alur Admisi Rawat Inap

- **TASK ID:** `FE-RWI-281`
- **TASK TYPE:** Implementasi Frontend Admisi Rawat Inap & Integrasi Kasus Maternitas Bedah
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 4 — integrasi pembentukan payload episode persalinan SC, dukungan formasi tri-dokter DPJP/konsulen, opsi boks bayi rawat gabung, paket melahirkan, dan pengujian unit terverifikasi
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `FRONTEND`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (Branch `HamzahV2`) & pelaporan di `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `RWI-DEC-357`, `RWI-DEC-358`, `OPS-DEC-040`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`
- **FILES INSPECTED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx`
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx`
  - `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx`
- **FILES CHANGED:**
  - `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` (Dukungan parameter formasi Tri-Dokter, `isRoomingInBabyPlanned`, `maternityPackageId`, `surgicalBookingNumber`, dan `bedHoldStatus` pada `buildInpatientAdmissionEpisodePayload`)
  - `QuilvianSystemFrontendDev/tests/unit/inpatient-admission-maternity-package.test.mjs` (Unit test baru 3/3 PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur admisi rawat inap dan bedah maternitas (`RWI-DEC-357`, `RWI-DEC-358`):
1. **Formasi Tri-Dokter Pasien Persalinan SC**:
   - Pasien yang menjalani *Sectio Caesarea* (SC) membutuhkan penanganan kolaboratif tiga dokter: Dokter Operator Kandungan (Sp.OG sebagai DPJP Utama), Dokter Spesialis Anestesi (Sp.An sebagai Konsulen), dan Dokter Spesialis Anak (Sp.A sebagai Konsulen).
   - Formasi ini dicatat dalam payload pembukaan episode rawat inap sehingga sejak menit pertama admisi terbit, dokter anak dan dokter anestesi sudah terdaftar secara resmi di rekam medis bangsal.
2. **Fasilitas Rawat Gabung Boks Bayi**:
   - Tersedia opsi `isRoomingInBabyPlanned` (boolean) untuk memesan satu boks bayi baru lahir di dalam ruangan ibu di bangsal nifas/maternitas.
3. **Pilihan Paket Persalinan / SC**:
   - Tersedia opsi referensi `maternityPackageId` bagi pasien yang memilih paket persalinan terpadu di rumah sakit.
4. **Penahanan Ranjang (*Bed Hold*)**:
   - Payload menandai `bedHoldStatus: "InSurgery"` saat pasien terhubung dengan jadwal booking kamar operasi, sehingga tempat tidur bangsal terkunci aman bagi pasien tersebut.

---

## 2. Rincian Implementasi

1. **Pembaruan Pembuat Payload (`inpatient-admission-encounter-utils.jsx`):**
   - Menambahkan field berikut ke `buildInpatientAdmissionEpisodePayload`:
     - `surgicalBookingNumber`
     - `clinicalSubCategory`
     - `bedHoldStatus`
     - `isRoomingInBabyPlanned`
     - `maternityPackageId`
     - `anesthesiologistDoctorId`
     - `pediatricianDoctorId`
   - Melakukan trimming string yang aman dan sanitasi boolean.
2. **Unit Test Terverifikasi (`inpatient-admission-maternity-package.test.mjs`):**
   - Memastikan formasi Tri-Dokter tersimpan lengkap pada payload.
   - Memastikan nilai `isRoomingInBabyPlanned` dan `maternityPackageId` terisi dengan benar.
   - Memastikan kasus rawat inap umum non-operasi tidak mengirimkan field operasi yang tidak perlu.

---

## 3. Bukti Verifikasi & Acceptance Criteria

### Eksekusi Unit Test:
```bash
cd QuilvianSystemFrontendDev
node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-maternity-package.test.mjs
```
Hasil:
```text
▶ FE-RWI-281: Integrasi Formasi Tri-Dokter, Maternitas SC, & Boks Bayi Rawat Gabung (RWI-DEC-357, RWI-DEC-358)
  ✔ AC-1: buildInpatientAdmissionEpisodePayload menyertakan formasi Tri-Dokter untuk kasus operasi SC (1.5531ms)
  ✔ AC-2: Opsi boks bayi rawat gabung (isRoomingInBabyPlanned) & paket melahirkan tersimpan dengan benar (0.1427ms)
  ✔ AC-3: Kasus rawat inap non-operasi tidak mengirimkan field penahanan bed maupun paket operasi (0.1523ms)
✔ FE-RWI-281: Integrasi Formasi Tri-Dokter, Maternitas SC, & Boks Bayi Rawat Gabung (RWI-DEC-357, RWI-DEC-358) (2.933ms)
ℹ tests 3
ℹ suites 1
ℹ pass 3
ℹ fail 0
```

---

## 4. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh kontrak API backend `0.19.0`, siap terhubung langsung dengan backend `BE-RWI-183`.
