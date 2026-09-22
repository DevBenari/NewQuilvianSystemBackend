# Kontrak Integrasi Bersama (Shared Integration Contract) — Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |
| Produsen Kontrak A | **Muhammad Hamzah** (Rawat Inap) |
| Konsumen Kontrak A | **Yasmina** (Billing) |
| Produsen Kontrak B | **Yasmina** (Billing) |
| Konsumen Kontrak B | **Muhammad Hamzah** (Rawat Inap) |
| Protokol | Asinkron (Transactional Outbox Event) & Sinkron (REST HTTP/JSON Internal) |

---

## 1. Kontrak A: Rawat Inap → Billing (Asinkron Outbox Event)

Modul Rawat Inap menerbitkan fakta pelayanan klinis dan pergerakan tempat tidur pasien. Seluruh pesan disimpan terlebih dahulu ke dalam tabel `InpIntegrationOutbox` dan dikirimkan ke message broker / HTTP event listener Billing.

### 1.1 Aturan Baku Header & Idempotensi
Setiap event membawa metadata standar:
- `SourceDomain`: `'INPATIENT'`
- `IdempotencyKey`: `{SourceDomain}:{SourceType}:{SourceDetailId}:{Version}` (contoh: `INPATIENT:ROOM_STAY:OCC-08891:1`)
- `CorrelationId`: GUID korelasi penelusuran lintas subsistem.
- `TimestampUtc`: Waktu kejadian perkara di bangsal rawat inap.

### 1.2 Daftar Event Kanonik

| Tipe Event (`EventType`) | Pemicu di Rawat Inap | Payload Utama | Tanggung Jawab Billing Saat Menerima |
|---|---|---|---|
| **`ADMISSION_CONFIRMED`** | Status admisi disahkan menjadi `Admitted` | `EncounterId`, `EpisodeId`, `PatientId`, `AdmissionDateTime`, `GuarantorId` | Membuat `BillingFolio` baru berstatus `OPEN`. Tidak membuat room charge sebelum bed terisi. |
| **`BED_OCCUPIED`** | Pasien menempati tempat tidur secara fisik | `EncounterId`, `EpisodeId`, `BedId`, `RoomId`, `RoomClassId`, `OccupancyStartAt`, `Version` | Mengaktifkan perhitungan sewa kamar harian (*room charge*) sejak `OccupancyStartAt`. |
| **`OCCUPANCY_CORRECTED`** | Mutasi kamar atau koreksi kelas kamar saat billing `OPEN` | `EncounterId`, `EpisodeId`, `OldRoomId`, `NewRoomId`, `OldRoomClassId`, `NewRoomClassId`, `EffectiveAtUtc`, `Reason`, `Version` | Melakukan *repricing* / *adjustment* tarif sewa kamar tanpa menghapus data tagihan historis. |
| **`BED_RELEASED`** | Pasien meninggalkan ruangan secara nyata (`PhysicallyLeftAt`) | `EncounterId`, `EpisodeId`, `BedId`, `RoomId`, `OccupancyEndAt`, `PhysicallyLeftAt`, `DischargeType`, `IsOverridden` | Menghentikan perhitungan sewa kamar secara presisi detik, memfinalisasi invoice, dan menutup folio (`CLOSED`). |

### 1.3 Contoh Payload JSON Kontrak A

#### Event `BED_OCCUPIED`
```json
{
  "eventId": "e9b533d7-2f1d-44a6-9817-642b5883a901",
  "eventType": "BED_OCCUPIED",
  "idempotencyKey": "INPATIENT:ROOM_STAY:OCC-08891:1",
  "sourceDomain": "INPATIENT",
  "sourceType": "ROOM_STAY",
  "sourceDetailId": "OCC-08891",
  "version": 1,
  "timestampUtc": "2026-09-17T02:30:00Z",
  "payload": {
    "encounterId": "ENC-2026-08891",
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patientId": "PAT-9912",
    "roomId": "ROOM-MEL-02",
    "roomName": "Melati Kamar 02",
    "bedId": "BED-MEL-02A",
    "bedCode": "Melati-02A",
    "roomClassId": "CLASS-2",
    "occupancyStartAt": "2026-09-17T02:30:00Z"
  }
}
```

#### Event `BED_RELEASED`
```json
{
  "eventId": "f1a234c9-8d76-43b1-a902-123456789abc",
  "eventType": "BED_RELEASED",
  "idempotencyKey": "INPATIENT:DISCHARGE:DIS-08891:1",
  "sourceDomain": "INPATIENT",
  "sourceType": "DISCHARGE",
  "sourceDetailId": "DIS-08891",
  "version": 1,
  "timestampUtc": "2026-09-17T05:15:00Z",
  "payload": {
    "encounterId": "ENC-2026-08891",
    "episodeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "roomId": "ROOM-MEL-02",
    "bedId": "BED-MEL-02A",
    "occupancyEndAt": "2026-09-17T05:15:00Z",
    "physicallyLeftAt": "2026-09-17T05:15:00Z",
    "dischargeType": "NORMAL_DISCHARGE",
    "isSupervisorOverridden": false,
    "overrideReason": null
  }
}
```

---

## 2. Kontrak B: Billing → Rawat Inap (Sinkron REST & Webhook)

Modul Kasir/Billing memberikan informasi status tagihan dan sinyal persetujuan clearance pemulangan kepada Rawat Inap.

### 2.1 Kueri Sinkron: `GET /billing-summary` (Internal REST)
- **Producer:** Billing (`BillingManagement`)
- **Consumer:** Rawat Inap (`InPatientManagement`)
- **Spesifikasi Payload Response:**
```json
{
  "encounterId": "ENC-2026-08891",
  "billingStatus": "OPEN",
  "financialClearanceStatus": "PENDING",
  "canDischarge": false,
  "depositBalance": 1000000.00,
  "totalCharges": 6741000.00,
  "outstanding": 241000.00,
  "blockerReasons": [
    "Keluarga pasien belum menyelesaikan administrasi pelunasan di kasir utama",
    "Menunggu konfirmasi verifikasi resep farmasi sore"
  ]
}
```
*Catatan Privasi:* Service Rawat Inap menyaring `depositBalance`, `totalCharges`, dan `outstanding` saat menyajikan DTO ke perawat bangsal, sehingga perawat hanya menerima `billingStatus`, `financialClearanceStatus`, dan `blockerReasons`.

### 2.2 Sinyal Webhook: `POST /discharge-clearance/webhook`
- **Producer:** Billing (`BillingManagement`)
- **Consumer:** Rawat Inap (`InPatientManagement`)
- **Pemicu:** Kasir menyetujui (`ClearanceApproved`) atau mencabut persetujuan clearance (`ClearanceRevoked`).
- **Payload Webhook:**
```json
{
  "encounterId": "ENC-2026-08891",
  "action": "CLEARANCE_REVOKED",
  "reason": "Terdapat tagihan susulan resep obat darurat farmasi yang belum dibayar.",
  "revokedByCashierName": "Hendra Pratama",
  "timestampUtc": "2026-09-17T04:45:00Z"
}
```
- **Reaksi Rawat Inap:** Seketika mengeksekusi *Auto-Reblock*: mengubah status clearance menjadi `Revoked`, mengunci tombol pemulangan fisik di bangsal, dan menampilkan peringatan darurat merah.

---

## 3. Kontrak Penanganan Kegagalan (Failure & Resilience Contract)

| Kondisi Kegagalan | Perilaku Modul Rawat Inap | Perilaku Modul Billing |
|---|---|---|
| **Koneksi Jaringan Terputus saat Pengiriman Event Outbox** | Event tetap tersimpan aman di tabel `InpIntegrationOutbox` dengan status `Failed`. Background worker mencoba kembali dengan exponential backoff (5d, 10d, 20d, ... max 1 jam). | Modul kasir tidak menerima event duplikat karena terlindungi oleh *Unique Index* `IdempotencyKey`. |
| **Modul Kasir Sedang Mengalami Downtime / Maintenance** | Operasional klinis di bangsal rawat inap tetap berjalan normal; data admisi dan mutasi kamar tetap tercatat secara lokal. | Begitu server kasir aktif kembali, worker outbox menyalurkan seluruh antrean event yang tertunda secara berurutan. |
| **Kueri Status Kasir Timeout (> 5 detik)** | UI bangsal menampilkan status fallback: *"Status Kasir Tidak Dapat Diperiksa Sementara (Silakan Coba Lagi)"*; tombol pemulangan fisik tetap terkunci secara aman (*Fail-Safe Locked*). | Staf kasir dapat dihubungi melalui jalur telepon operasional rumah sakit jika ada kebutuhan darurat. |
| **Kondisi Kedaruratan Medis Tanpa Sinyal Kasir** | Supervisor Bangsal menggunakan *Supervisor Override* beralasan wajib untuk memulangkan pasien secara fisik demi keselamatan klinis. | Billing menerima event `BED_RELEASED` bertanda `isSupervisorOverridden = true` dan memproses penyelesaian piutang bersama keluarga penjamin. |
