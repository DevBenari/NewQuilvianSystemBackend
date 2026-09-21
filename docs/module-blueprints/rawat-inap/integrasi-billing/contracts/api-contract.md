# Kontrak API (API Contract) — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** — menunggu approval Muhammad Hamzah dan Yasmina |
| Base Path | `/api/v1/health-services/inpatient-management` |
| Autentikasi | Bearer Token (JWT), `Authorization: Bearer <token>` |
| Aturan Swagger | Seluruh endpoint dikelompokkan dengan atribut `[Tags(...)]` dan dilengkapi model DTO presisi. |

---

## 1. Grup Tag: `[Tags("Inpatient Billing Operational")]`

Grup endpoint ini melayani kebutuhan pemantauan status penagihan kasir pada layar bangsal rawat inap, memisahkan pandangan operasional non-finansial dari rincian nominal uang.

### 1.1 Tabel Endpoint Spesifikasi

| Method | Path | Status Ketersediaan | Deskripsi & Tujuan | Hak Akses (Permission) | Request DTO | Response DTO | Kode Status |
|---|---|:---:|---|---|:---:|:---:|:---:|
| `GET` | `/episodes/{episodeId}/billing-status` | `Rencana (belum tersedia)` | Mengambil ringkasan status operasional kasir untuk perawat bangsal (steril dari nominal rupiah). | `InpatientNurse:Read` atau `InpatientEpisode:Read` | — | `InpatientBillingStatusResponseDto` | `200 OK`, `404 Not Found` |
| `GET` | `/episodes/{episodeId}/billing-details` | `Rencana (belum tersedia)` | Mengambil rincian akumulasi biaya finansial lengkap beserta nominal rupiah (khusus staf berizin). | `InpatientBilling:View` | — | `InpatientBillingDetailsResponseDto` | `200 OK`, `403 Forbidden`, `404 Not Found` |

### 1.2 Detail Spesifikasi Endpoint

#### `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/billing-status`
- **Kegunaan:** Menampilkan lencana warna status kasir dan daftar kendala blocker pada layar perawat.
- **Contoh Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "encounterId": "ENC-2026-08891",
    "patientName": "Budi Santoso",
    "medicalRecordNumber": "RM-2026-08891",
    "folioStatus": "OPEN",
    "clearanceStatus": "PENDING",
    "operationalStatusText": "Menunggu Penyelesaian Kasir",
    "statusColor": "amber",
    "canPhysicallyDischarge": false,
    "blockerReasons": [
      "Keluarga pasien belum menyelesaikan administrasi pelunasan di kasir utama",
      "Menunggu konfirmasi verifikasi resep farmasi sore"
    ],
    "lastCheckedAtUtc": "2026-09-17T04:15:30Z"
  },
  "message": "Status operasional kasir berhasil diambil."
}
```

#### `GET /api/v1/health-services/inpatient-management/episodes/{episodeId}/billing-details`
- **Kegunaan:** Menampilkan breakdown biaya rupiah pada drawer rincian finansial (khusus pemegang izin `InpatientBilling:View`).
- **Contoh Response `200 OK`:**
```json
{
  "success": true,
  "data": {
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "encounterId": "ENC-2026-08891",
    "totalCharges": 6741000.00,
    "coveredAmount": 5500000.00,
    "patientExcess": 1241000.00,
    "depositPaid": 1000000.00,
    "outstandingAmount": 241000.00,
    "clearanceStatus": "PENDING",
    "items": [
      {
        "category": "ROOM_CHARGE",
        "description": "Sewa Kamar Melati Kelas 2 (2 Hari)",
        "amount": 1600000.00
      },
      {
        "category": "DOCTOR_VISIT",
        "description": "Visite Dokter Spesialis Penyakit Dalam (dr. Anwar, 3x)",
        "amount": 750000.00
      },
      {
        "category": "PHARMACY",
        "description": "Obat & Alkes Rawat Inap",
        "amount": 2850000.00
      },
      {
        "category": "ADMIN_FEE",
        "description": "Biaya Administrasi Rawat Inap (7%)",
        "amount": 441000.00
      }
    ]
  },
  "message": "Rincian finansial kasir berhasil diambil."
}
```

---

## 2. Grup Tag: `[Tags("Inpatient Discharge Clearance")]`

Grup endpoint ini melayani gerbang pemulangan pasien, webhook sinyal clearance dari kasir, dan penanganan darurat klinis *Supervisor Override*.

### 2.1 Tabel Endpoint Spesifikasi

| Method | Path | Status Ketersediaan | Deskripsi & Tujuan | Hak Akses (Permission) | Request DTO | Response DTO | Kode Status |
|---|---|:---:|---|---|:---:|:---:|:---:|
| `POST` | `/episodes/{episodeId}/discharge-clearance/webhook` | `Rencana (belum tersedia)` | Menerima sinyal clearance dari kasir (ClearanceApproved / ClearanceRevoked). | Internal System Token | `ClearanceSignalWebhookDto` | `BaseResponse` | `200 OK`, `400 Bad Request` |
| `POST` | `/episodes/{episodeId}/supervisor-override` | `Rencana (belum tersedia)` | Mengeksekusi override pelepasan darurat saat clearance dicabut/terblokir. | `InpatientSupervisor:Override` | `SupervisorOverrideRequestDto` | `BaseResponse` | `200 OK`, `403 Forbidden`, `422 Unprocessable` |
| `POST` | `/episodes/{episodeId}/confirm-physical-discharge` | `Rencana (belum tersedia)` | Mengonfirmasi pasien meninggalkan ruangan kamar secara fisik (`PhysicallyLeftAt`). | `InpatientNurse:Write` | `ConfirmPhysicalDischargeRequestDto` | `BaseResponse` | `200 OK`, `422 Unprocessable` |

### 2.2 Detail Spesifikasi Endpoint

#### `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/supervisor-override`
- **Kegunaan:** Otorisasi pelepasan darurat untuk pasien rujukan kritis saat kasir belum menerbitkan clearance atau clearance dibatalkan.
- **Request Body:**
```json
{
  "reason": "Pasien darurat syok kardiogenik, rujukan ambulans prioritas 1 ke RS Harapan Kita. Administrasi kasir dilanjutkan pihak keluarga penjamin di loket kasir utama.",
  "supervisorPin": "123456"
}
```
- **Response `200 OK`:**
```json
{
  "success": true,
  "message": "Supervisor override berhasil disahkan. Tombol pelepasan fisik pasien kini aktif atas izin darurat medis."
}
```

#### `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/confirm-physical-discharge`
- **Kegunaan:** Mengunci waktu keluar fisik pasien (`PhysicallyLeftAt`), menutup hunian kamar, dan menerbitkan event outbox `BED_RELEASED` ke kasir.
- **Request Body:**
```json
{
  "physicalDischargeDateTime": "2026-09-17T05:15:00Z",
  "notes": "Pasien telah dijemput keluarga dengan ambulans RS, resume medis dan obat pulang telah diserahkan."
}
```
- **Response `200 OK`:**
```json
{
  "success": true,
  "message": "Pasien berhasil dipulangkan secara fisik. Jam hunian tempat tidur ditutup presisi dan event kepulangan telah dikirim ke Kasir."
}
```
