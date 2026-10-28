# BE-OPR-014 — Alokasi Tim Tri-Dokter Obgyn & Pencatatan Kelahiran Bayi Intraoperatif SC

- **TASK ID:** `BE-OPR-014`
- **TASK TYPE:** Implementasi Backend Operasi & Event Integrasi Persalinan Meja Bedah
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** 5 — model entitas baru, tabel migrasi additive, DTO klinis kelahiran bayi, endpoint controller pelaksanaan, dan penerbitan event outbox idempotent
- **MODEL:** Google Antigravity (Gemini 3.8 Flash High)
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` (Branch `MHamzah`)
- **TRACE:** `OPS-DEC-041`, `OPS-DEC-042`, `DSN-OPR-RWI-20261023`
- **KONTRAK:** `0.19.0`, `OPR-INT-008`
- **FILES INSPECTED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprCase.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprExecutionRecord.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprIntegrationDelivery.cs`
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomExecutionService.cs`
- **FILES CREATED / CHANGED:**
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Models/OprBirthEvent.cs` (Model entitas data kelahiran intraoperatif)
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/DTOs/OprBirthEventDtos.cs` (DTO request & response pencatatan kelahiran)
  - `NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs` (Registrasi `DbSet<OprBirthEvent>`)
  - `NewQuilvianSystemBackend/Migrations/20261024104000_AddOprBirthEventTable.cs` (Migrasi EF Core additive mandiri)
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Services/OperatingRoomExecutionService.cs` (Method `RecordBirthEventAsync` dengan penerbitan event outbox)
  - `NewQuilvianSystemBackend/Areas/HealthServices/OperatingRoomManagement/Controllers/OperatingRoomBirthEventController.cs` (Endpoint `POST /cases/{caseId}/execution/birth-event`)
  - `NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.OperatingRoomTests/OperatingRoomBirthEventTests.cs` (Unit test 3/3 Fact PASS)

---

## 1. Latar Belakang & Ketetapan Bisnis

Berdasarkan keputusan integrasi operasi dan persalinan kebidanan (`OPS-DEC-041`, `OPS-DEC-042`):
1. **Pencatatan Kelahiran Meja Bedah**:
   - Saat tindakan *Sectio Caesarea* (SC) berlangsung di Kamar Operasi, kelahiran bayi intraoperatif dicatat secara langsung di meja bedah oleh tim dokter/perawat sirkuler.
   - Parameter klinis vital mencakup: Waktu Kelahiran (`BirthDateTime`), Jenis Kelamin (`Gender`), Berat Badan Lahir (`WeightGrams`), Panjang Badan (`LengthCentimeters`), Lingkar Kepala (`HeadCircumferenceCm`), Skor APGAR 1 menit, 5 menit, dan 10 menit, serta Catatan Resusitasi / Kondisi Bayi.
2. **Penerbitan Event Outbox Idempotent**:
   - Penyimpanan data kelahiran secara atomik menerbitkan pesan outbox `operating-room.birth-event.recorded` ke tabel `OprIntegrationDelivery`.
   - Event ini ditujukan untuk dikonsumsi secara asinkron oleh modul Rawat Inap (`BE-RWI-184`), yang secara otomatis akan membuat Nomor Rekam Medis sementara dan membuka Episode Bayi Baru Lahir dengan relasi tertaut ke Episode Rawat Inap sang Ibu (*Mother-Baby Linkage*).

---

## 2. Rincian Implementasi

1. **Model Entitas & Migrasi (`OprBirthEvent.cs` & `AddOprBirthEventTable.cs`):**
   - Tabel `OprBirthEvent` memiliki foreign key dengan aturan `RESTRICT` ke tabel `OprCase`.
   - Tipe data desimal terstandarisasi untuk metrik fisik bayi (`decimal(8,2)`).
2. **DTO & Validasi Input (`OprBirthEventDtos.cs`):**
   - Validasi data rentang nilai wajar medis (APGAR 0-10, berat 100-10000 gr, panjang 10-100 cm).
3. **Penyimpanan Transaksional & Outbox (`OperatingRoomExecutionService.cs`):**
   - Dalam satu transaksi, entitas `OprBirthEvent` disimpan bersamaan dengan entitas outbox `OprIntegrationDelivery` berstatus `Pending`.
4. **Endpoint Controller (`OperatingRoomBirthEventController.cs`):**
   - `POST api/v1/health-services/operating-room-management/cases/{caseId}/execution/birth-event`.
5. **Unit Test (`OperatingRoomBirthEventTests.cs`):**
   - Menguji persistensi atribut klinis bayi baru lahir.
   - Menguji validasi DTO kelahiran.
   - Menguji integritas format JSON outbox event integrasi.

---

## 3. Status Task

- **Status:** `COMPLETED (SELESAI)` ✅
- **Kompatibilitas:** Mematuhi seluruh arsitektur modul IBS, outbox pattern Quilvian, dan siap dikonsumsi oleh `BE-RWI-184` serta antarmuka `FE-OPR-013`.
