# BE-RWI-184 — Consumer Event Kelahiran Bayi Intra-Op SC & Pembuatan Episode Neonatus Otomatis

- **TASK ID:** `BE-RWI-184`
- **TASK TYPE:** Implementasi Backend Rawat Inap & Consumer Integrasi Kelahiran Bedah SC
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 5 — consumer integration service, auto-generate pasien bayi & rekam medis, auto-admission episode neonatus tertaut ibu, endpoint webhook consumer, dan pengujian unit
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `OPS-DEC-042`, `DSN-OPR-RWI-20261023`, `RWI-DEC-357`
- **KONTRAK:** `0.19.0`, `OPR-INT-008`
- **FILES INSPECTED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpNewbornAdmissionRules.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprIntegrationDelivery.cs`
- **FILES CREATED / CHANGED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpatientNewbornBirthEventConsumerService.cs` (Service consumer asinkron untuk event `operating-room.birth-event.recorded`)
  - `NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Controllers/InpatientNewbornBirthEventConsumerController.cs` (Endpoint `POST /integration/birth-event-consumer`)
  - `NewQuilvianSystemBackend/Program.cs` (Registrasi DI `InpatientNewbornBirthEventConsumerService`)
  - `NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/InpatientNewbornBirthEventTests.cs` (Unit test 2/2 Fact PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan arsitektur integrasi modul operasi dan rawat inap (`OPS-DEC-042`, `RWI-DEC-357`):
1. **Otomasi Pembuatan Episode Bayi Baru Lahir**:
   - Ketika kelahiran bayi dicatat di meja bedah SC pada modul Kamar Operasi (`BE-OPR-014`), sistem menerbitkan pesan integrasi outbox `operating-room.birth-event.recorded`.
   - Modul Rawat Inap mengonsumsi pesan ini secara asinkron dan idempotent:
     - Membuat profil pasien bayi baru lahir (`MstPatient`) secara otomatis lengkap dengan Nomor Rekam Medis sementara (`BY-YYMMDD-XXXX`), nama `"Bayi Ny. [Nama Ibu]"`, jenis kelamin, dan tanggal/jam lahir.
     - Membuka Episode Rawat Inap Neonatus (`InpEpisode`) yang langsung berstatus `Admitted`, mewarisi kelas perawatan sang ibu di bangsal nifas/maternitas, dan menetapkan tautan permanen `MotherEpisodeId` (*Mother-Baby Linkage*).
2. **Kemandirian Transaksi & Idempotensi**:
   - Jika pesan yang sama diterima kembali (replay / retry outbox), consumer mendeteksi bahwa bayi untuk kasus persalinan tersebut telah dibuat, lalu mengembalikan status sukses tanpa membuat duplikasi entitas pasien maupun episode.

---

## 2. Rincian Implementasi

1. **DTO Kontrak Event (`InpatientNewbornBirthEventConsumerService.cs`):**
   - Menampung seluruh data klinis kelahiran dari meja bedah: `CaseId`, `BirthEventId`, `MotherPatientId`, `Gender`, `WeightGrams`, `LengthCentimeters`, `HeadCircumferenceCm`, `ApgarScore1Min`, `ApgarScore5Min`, dan `ApgarScore10Min`.
2. **Logika Consumer Asinkron:**
   - Melakukan pengecekan idempotensi berdasarkan `DeliveryCaseId == @event.CaseId`.
   - Menghubungkan otomatis episode bayi dengan episode rawat inap aktif sang ibu.
   - Mengisi catatan admisi bayi dengan metrik skor APGAR dan data antropometri kelahiran.
3. **Endpoint Controller Webhook (`InpatientNewbornBirthEventConsumerController.cs`):**
   - Menerima POST request ke `api/v1/health-services/inpatient-management/integration/birth-event-consumer`.
4. **Unit Test (`InpatientNewbornBirthEventTests.cs`):**
   - Menguji kebenaran default payload integrasi event.
   - Menguji pelaporan hasil konsumsi sukses dan penanganan idempotensi pengiriman ganda.

---

## 3. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh arsitektur rawat inap bayi baru lahir, integritas tautan ibu-bayi (*Mother-Baby Linkage*), dan kontrak `0.19.0`.
