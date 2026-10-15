# PLAN-REPAIR-019 — Perbaikan Galat 409 Conflict (BED_HOLD_CONFLICT) pada Finalisasi Admisi Pasien Transfer Rawat Inap

```yaml
plan_id: PLAN-REPAIR-EPS-019
issue: ../issue/issue-019-admisi-transfer-bed-hold-conflict.md
status_rencana: DISETUJUI_DAN_DISELESAIKAN
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "User / Product Owner"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup Temuan | Ringkasan Tindakan | Area | Prioritas | Status | Rencana Bukti Verifikasi |
| --- | --- | --- | :---: | :---: | :---: | --- |
| `FIX-EPS-019-01` | `ISS-EPS-019-01` | Daftarkan `paymentSource` secara eksplisit ke dalam `_dbContext.RegPatientEncounterGuarantors.Add(paymentSource)` agar ChangeTracker melacaknya dengan status `EntityState.Added` | BE | P0 (Fatal) | SELESAI | Eksekusi `POST /api/v1/health-services/inpatient-management/admission-transfers/admit` menghasilkan HTTP 201 Created dengan nomor episode, reservasi bed, dan deposit terbentuk |
| `FIX-EPS-019-02` | `ISS-EPS-019-02` | Bersihkan kode diagnostik pelacakan error dan kembalikan penanganan exception `InpBedMutationCoordinator` ke standar kode produksi | BE | P1 (High) | SELESAI | Kompilasi backend `dotnet build` sukses (0 Error, 10 Warning dasar) |
| `FIX-EPS-019-03` | `ISS-EPS-019-03` | Tambahkan DDL seeder idempotensi `ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS ...` pada startup seeder | DB / BE | P0 (Fatal) | SELESAI | Seluruh 6 kolom baru (`NewbornAdmissionSource`, `NewbornCareMode`, `NewbornCareSource`, `RequiresNeonatalIntensiveCare`, `FamilyBillingPreference`, `PlannedDepositAmount`) tersedia di database tanpa error Postgres 42703 |

---

## 2. Solusi Terpilih Per Temuan

### A. Backend: Pendaftaran Eksplisit Penjamin Baru (`FIX-EPS-019-01`)

Pada [EncounterPaymentSourceService.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs):
```csharp
var paymentSource = new RegPatientEncounterGuarantor
{
    EncounterId = encounter.Id,
    PatientId = encounter.PatientId,
    PaymentSourceNumber = await _numbers.AllocatePaymentSourceNumberAsync(cancellationToken),
    PaymentType = EncounterPaymentType.Cash,
    PaymentSourceNameSnapshot = "Tunai",
    PaymentMethodId = command.PaymentMethodId,
    IsEligible = true,
    IsPrimary = true,
    Priority = 1,
    CreateBy = command.ActorUserId,
    CreateDateTime = DateTime.UtcNow
};
encounter.PaymentSource = paymentSource;
// FIX-EPS-019-01: Daftarkan eksplisit sebagai entitas Added agar EF Core ChangeTracker
// menghasilkan perintah INSERT, bukan UPDATE yang berujung DbUpdateConcurrencyException
_dbContext.RegPatientEncounterGuarantors.Add(paymentSource);
encounter.PaymentMethodId = command.PaymentMethodId;
await _dbContext.SaveChangesAsync(cancellationToken);
```

### B. Database: Seeder Idempotensi Kolom `InpEpisode` (`FIX-EPS-019-03`)

Pada [AppVersionSeeder.cs](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Seeders/AppVersionSeeder.cs):
```csharp
await db.Database.ExecuteSqlRawAsync(@"
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornAdmissionSource"" character varying(20) NULL;
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornCareMode"" character varying(20) NULL;
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""NewbornCareSource"" character varying(200) NULL;
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""RequiresNeonatalIntensiveCare"" boolean NOT NULL DEFAULT FALSE;
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""FamilyBillingPreference"" character varying(20) NULL;
    ALTER TABLE public.""InpEpisode"" ADD COLUMN IF NOT EXISTS ""PlannedDepositAmount"" numeric(18,2) NULL;
");
```

---

## 3. Spesifikasi API Endpoint (Bergaya Swagger)

### [Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]

| Properti | Nilai Spesifikasi |
| :--- | :--- |
| **Metode HTTP** | `POST` |
| **Path** | `/api/v1/health-services/inpatient-management/admission-transfers/admit` |
| **Deskripsi** | Mengeksekusi penerimaan pasien admisi transfer IGD/Poli secara atomik: membuat kunjungan rawat inap, membuka episode, mencatat penjamin, memesan ranjang tidur (*bed reservation*), dan menerima uang muka deposit rawat inap. |
| **Otorisasi (Auth)** | Bearer JWT Token (`InpatientEpisode:Create`, `PatientEncounter:Create`, `BillingDeposit:Create`) |
| **Header Wajib** | `Idempotency-Key` (UUIDv4) |

#### Parameter Permintaan (Request Body JSON):
```json
{
  "transferDispositionId": "562b8819-a75a-4858-9cf3-6bee49ee498a",
  "sourceType": "Outpatient",
  "patientId": "4110016b-7535-4b31-826d-56436f297965",
  "serviceUnitId": "fddbe4ae-832b-484c-aba7-d6280e07c311",
  "patientClassId": "976b37f4-23aa-4f93-b171-15157a3c277b",
  "doctorId": "6e2399b6-5a7c-4319-9dad-bdef6bbb3aab",
  "bedId": "1c4424c7-537d-413d-9cdd-ee6cb56ba851",
  "expectedBedVersion": 1,
  "paymentType": 1,
  "depositAmount": 1000000,
  "depositPaymentMethodId": "da4ffc97-0ed9-4ca9-9c63-8dbeb899936c",
  "requiresIsolation": false,
  "notes": "Intake oral sulit karena muntah berulang..."
}
```

#### Respons Berhasil (HTTP 201 Created):
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Admisi transfer dan reservasi tersimpan. Lanjutkan persetujuan pasien.",
  "data": {
    "id": "b92ed7dc-820d-4057-9bae-9fa760be3303",
    "episodeNumber": "RI-261014095602-E09B5B",
    "encounterId": "56cfb5ea-3ca6-401f-8db2-1b3b38a2ac93",
    "encounterNumber": "ENC-RSMMC-261014095601-02D484",
    "patientName": "Ahmad Fauzi Rahman",
    "medicalRecordNumber": "00-00-00-29",
    "serviceUnitName": "Rawat Inap",
    "patientClassName": "KELAS III",
    "episodeStatusName": "Draft",
    "transferReservation": {
      "id": "3a2a8857-55c4-4d30-b83e-e4e4b73982b2",
      "bedCode": "BD-RSMMC-00046",
      "bedName": "BED 002",
      "roomName": "Kamar 01",
      "reservationStatusName": "Active"
    },
    "transferDeposit": {
      "accountNumber": "DEP-00000002",
      "availableBalance": 1000000.0,
      "status": "ACTIVE"
    },
    "operationMeta": {
      "resultEntityId": "3a2a8857-55c4-4d30-b83e-e4e4b73982b2",
      "resultKind": "Reservation",
      "committedVersion": 2
    }
  }
}
```

---

## 4. Bukti Verifikasi Eksekusi Nyata

Pengujian langsung telah dijalankan melalui skrip PowerShell terhadap backend lokal (`https://localhost:7184`) menggunakan data pasien yang dilaporkan:

1. **Autentikasi Akun**: SuperAdmin (`superadmin@admin.com`) login berhasil `HTTP 200`.
2. **Eksekusi Admisi Transfer**:
   - Status respons: `HTTP 201 Created`.
   - Episode Rawat Inap terbentuk: `RI-261014095602-E09B5B`.
   - Kunjungan terbentuk: `ENC-RSMMC-261014095601-02D484`.
   - Reservasi Bed aktif: `BD-RSMMC-00046` (`BED 002`, `Kamar 01`).
   - Deposit masuk: Rekening `DEP-00000002`, Saldo `Rp 1.000.000,00`.
   - Audit trail tercatat lengkap pada `BillingDeposit.TopUp`, `CashierShift.CASH_RECEIPT`, dan `AdmissionTransfer.Created`.
