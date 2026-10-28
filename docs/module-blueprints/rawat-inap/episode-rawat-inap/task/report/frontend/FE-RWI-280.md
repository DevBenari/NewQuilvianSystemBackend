# FE-RWI-280 — Banner Deteksi Booking Jadwal Kamar Operasi IBS pada Formulir Langkah 1 Admisi Rawat Inap

- **TASK ID:** `FE-RWI-280`
- **TASK TYPE:** Implementasi Frontend Admisi Rawat Inap & Integrasi Operasi
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 4 — integrasi lookup API, penyusunan kartu komponen visual penanda operasi IBS, integrasi ke langkah peninjauan pasien lama, dan unit test verifikasi
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `FRONTEND`
- **WRITE TARGET:** `QuilvianSystemFrontendDev` (Branch `HamzahV2`) & pelaporan di `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `RWI-DEC-356`, `OPS-DEC-040`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`
- **FILES INSPECTED:**
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js`
  - `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx`
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx`
- **FILES CREATED / CHANGED:**
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-surgical-booking-card.jsx` (Komponen kartu banner IBS)
  - `QuilvianSystemFrontendDev/src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js` (`fetchInpatientAdmissionSurgicalBookingLookup` disiapkan dengan penanganan fail-open dan fallback)
  - `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx` (Penambahan state `surgicalBooking` dan auto-fetch saat detail pasien termuat)
  - `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx` (Menampilkan `InpatientAdmissionSurgicalBookingCard` pada Langkah Tinjauan Pasien Lama)
  - `QuilvianSystemFrontendDev/tests/unit/inpatient-admission-surgical-booking.test.mjs` (Unit test baru 3/3 PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur integrasi Operasi ↔ Rawat Inap (`RWI-DEC-356` dan `OPS-DEC-040`):
1. Ketika petugas admisi memilih pasien lama yang telah memiliki jadwal pemesanan kamar operasi (IBS) yang disetujui, sistem admisi rawat inap wajib secara otomatis mendeteksi keberadaan jadwal tersebut.
2. Pada Langkah 1 (Informasi / Peninjauan Pasien Lama), sistem menampilkan kartu banner informatif berwarna ungu khas Bedah (`#7c3aed`) yang menyajikan:
   - Nomor Booking Operasi & Kategori Klinis (Elektif, Cito/Darurat, Obstetri SC).
   - Kamar Operasi (Ruang OK Lantai 2) dan Waktu Pelaksanaan.
   - Tim Dokter Bedah (DPJP Bedah / Operator Utama, Dokter Anestesi, dan Dokter Anak untuk kasus persalinan SC).
   - Indikator Penahanan Tempat Tidur (*Bed Hold*) bahwa ranjang bangsal akan dipertahankan saat pasien menjalani operasi.
3. Pendeteksian ini bersifat *fail-open*; jika layanan IBS sedang tidak tersedia, alur pendaftaran admisi reguler tetap dapat berjalan tanpa hambatan.

---

## 2. Rincian Implementasi

1. **Komponen Kartu Visual (`inpatient-admission-surgical-booking-card.jsx`):**
   - Menampilkan badge "TERJADWAL DI INSTALASI BEDAH SENTRAL (IBS)".
   - Menampilkan badge tambahan bila kasus merupakan "Operasi Cito / Darurat" atau "Persalinan SC / Maternitas".
   - Menyajikan grid metadata lengkap: Nomor Booking, Ruang Kamar Bedah, Jadwal Operasi, Tindakan Rencana, DPJP Bedah, Dokter Anestesi, dan Dokter Anak Sp.A.
   - Dilengkapi catatan kebijakan *Bed Hold* bangsal.
2. **Hook Konsumsi API (`use-inpatient-admission-patient.jsx`):**
   - Memanggil `fetchInpatientAdmissionSurgicalBookingLookup` saat pasien terpilih (`activeSelectedPatientId`).
   - Menyediakan state `surgicalBooking` dan `surgicalBookingLoading`.
   - Membersihkan state secara otomatis ketika pasien diganti atau di-reset (`handleClearPatient`).
3. **Penyisipan ke Langkah Tinjauan Pasien (`inpatient-admission-existing-patient-step.jsx`):**
   - Ditempatkan di bagian atas kartu verifikasi identitas pasien agar petugas admisi langsung terinformasi sebelum melangkah ke pemilihan tipe penjamin dan kelas kamar.
4. **Unit Test Terverifikasi (`inpatient-admission-surgical-booking.test.mjs`):**
   - Verifikasi fail-safe jika `patientId` kosong atau tanpa jadwal.
   - Validasi struktur DTO integrasi operasi.
   - Seluruh test lulus 100% HIJAU (PASS).

---

## 3. Bukti Verifikasi & Acceptance Criteria

### Eksekusi Unit Test:
```bash
cd QuilvianSystemFrontendDev
node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-surgical-booking.test.mjs
```
Hasil:
```text
▶ FE-RWI-280: Lookup & Banner Deteksi Booking Jadwal Kamar Operasi IBS di Admisi Ranap
  ✔ AC-1: fetchInpatientAdmissionSurgicalBookingLookup mengembalikan hasBooking false jika patientId kosong atau tanpa jadwal (0.7602ms)
  ✔ AC-2: Struktur booking IBS mencakup identitas booking, nomor registrasi operasi, dan kategori klinis (0.1459ms)
  ✔ AC-3: Format payload DTO lookup valid dan aman diparsing oleh kartu informasi banner admisi (0.1294ms)
✔ FE-RWI-280: Lookup & Banner Deteksi Booking Jadwal Kamar Operasi IBS di Admisi Ranap (2.0975ms)
ℹ tests 3
ℹ suites 1
ℹ pass 3
ℹ fail 0
```

---

## 4. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh standar desain antarmuka Quilvian, CSS modular, dan aman dari pemutusan koneksi backend (fail-open).
