# BE-RWI-183 — Penahanan Tempat Tidur (*Bed Hold*), Penugasan Tri-Dokter, dan Skema Paket Persalinan SC

- **TASK ID:** `BE-RWI-183`
- **TASK TYPE:** Implementasi Backend Admisi Rawat Inap & Manajemen Bed Pasien Operasi
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 4 — pembaruan model entitas, penambahan endpoint status bed hold, dukungan formasi tri-dokter DPJP & konsulen, serta uji unit
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `RWI-DEC-355`, `RWI-DEC-357`, `RWI-DEC-358`, `OPS-DEC-040`
- **KONTRAK:** `0.19.0`
- **FILES INSPECTED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs`
- **FILES CREATED / CHANGED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/DTOs/InpatientEpisodeDtos.cs` (DTO `UpdateBedHoldStatusRequest`, penambahan `AnesthesiologistDoctorId` & `PediatricianDoctorId` pada `OpenAdmissionRequest`)
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` (Pencatatan konsulen Tri-Dokter pada admisi, penambahan method `UpdateBedHoldStatusAsync`)
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` (Endpoint `PATCH /v1/health-services/inpatient-management/episodes/{id}/bed-hold`)
  - `NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/InpatientBedHoldTests.cs` (Unit test 3/3 Fact PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur modul rawat inap & kamar operasi (`RWI-DEC-355`, `RWI-DEC-357`, `RWI-DEC-358`):
1. **Penahanan Tempat Tidur (*Bed Hold*)**:
   - Pasien operasi terencana yang sudah masuk rawat inap tidak melepaskan tempat tidur bangsal saat dibawa ke ruang bedah sentral (IBS) maupun kamar pemulihan (PACU).
   - Ranjang bangsal tetap berstatus `Occupied` di bawah kendali episode pasien tersebut, dan field `InpEpisode.BedHoldStatus` menandai posisi pasien (`InSurgery`, `InRecovery`, atau `Released`/`None`).
   - Hal ini menjamin tempat tidur pasien tidak dapat diambil oleh admisi lain, dan pasca-operasi pasien langsung kembali ke ranjangnya di bangsal via serah terima internal tanpa registrasi ulang.
2. **Formasi Tri-Dokter Kasus Persalinan SC**:
   - Tindakan *Sectio Caesarea* (SC) melibatkan tiga dokter spesialis:
     - Dokter Spesialis Kebidanan & Kandungan (Sp.OG) bertindak sebagai **DPJP Utama** (`InpDoctorAssignmentRole.Dpjp`, Sequence 1).
     - Dokter Spesialis Anestesi (Sp.An) bertindak sebagai **Konsulen** (`InpDoctorAssignmentRole.Consultant`, Sequence 2).
     - Dokter Spesialis Anak (Sp.A) bertindak sebagai **Konsulen** (`InpDoctorAssignmentRole.Consultant`, Sequence 3).
   - Struktur ini menjaga invariant bahwa hanya satu DPJP utama yang memegang kewenangan pelepasan/keputusan episode ranap, sementara dokter anestesi dan anak tercatat sah sebagai perawat bersama.
3. **Paket Persalinan & Boks Bayi Rawat Gabung**:
   - Kolom `IsRoomingInBabyPlanned` (boolean) menandai apakah disiapkan boks bayi satu ruangan dengan ibu di bangsal nifas/maternitas.
   - Kolom `MaternityPackageId` menampung referensi paket tarif melahirkan yang dipilih.

---

## 2. Rincian Implementasi

1. **DTO & Model Perluasan (`InpatientEpisodeDtos.cs`):**
   - Menambahkan DTO `UpdateBedHoldStatusRequest` dengan validasi nilai: `None`, `InSurgery`, `InRecovery`, `Released`.
   - Menambahkan field `AnesthesiologistDoctorId` dan `PediatricianDoctorId` pada `OpenAdmissionRequest`.
2. **Logika Tri-Dokter pada Admisi (`InpEpisodeService.cs`):**
   - Saat admisi dibuka, jika ada `AnesthesiologistDoctorId` atau `PediatricianDoctorId`, sistem langsung menambahkan entitas `InpDoctorAssignment` berturut-turut dengan peran `Consultant` dan tujuan `Regular`.
3. **Endpoint Kontrol Bed Hold (`InpatientEpisodeController.cs`):**
   - Menambahkan endpoint `PATCH api/v1/health-services/inpatient-management/episodes/{id}/bed-hold`.
   - Mengubah status penahanan bed secara audit-compliant dan mengembalikan detail episode terkini.
4. **Unit Test (`InpatientBedHoldTests.cs`):**
   - Menguji persistensi field model `InpEpisode`.
   - Menguji default value DTO request bed hold.
   - Menguji pemisahan peran DPJP utama vs konsulen Tri-Dokter.

---

## 3. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh invariant rawat inap, integritas audit, dan siap dikonsumsi oleh layar frontend admisi (`FE-RWI-281`).
