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

---

## 4. Perubahan pada `contract_version` `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `1.1.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Owner | Muhammad Hamzah (produsen Rawat Inap); Yasmina (konsumen Billing, disetujui `RWI-DEC-192`); pemilik `PharmacyManagement` Ikbal Yulianto, persetujuan `RWI-DEC-210` |
| Traceability | `FR-RWF-002`, `010` s.d. `018`; `RWI-DEC-166`, `167`, `169`, `192`, `195` |

Bagian 1 s.d. 3 di atas tetap sebagai jejak. Webhook clearance dan publisher tanpa penerima pada bagian itu **dicabut**.

### 4.1 Daftar integrasi

| ID | Arah | Mekanisme | Sinkron/asinkron | Kontrak | Pemilik penerima | Gerbang |
|---|---|---|---|---|---|---|
| `INT-RWF-01` | Rawat Inap → Billing | Outbox + panggilan service di dalam aplikasi `BillingInpatientEventReceiver.ReceiveAsync` | Asinkron (worker) | 4.2 | Billing | — (`RWI-DEC-192`) |
| `INT-RWF-02` | Rawat Inap ← Billing | Panggilan service `InpatientClearanceService.GetLatestStatusAsync` lewat `InpBillingClearanceAdapter` | Sinkron, baca saja | 4.3 | Billing | — |
| `INT-RWF-03` | Rawat Inap ← Billing | Panggilan service `InpatientClearanceService.GetLatestStatusAsync` (bagian `InvoiceStatus`) untuk koreksi penempatan | Sinkron, baca saja | 4.3 | Billing | — |
| `INT-RWF-04` | Modul klinis → Billing | Jembatan folio yang sudah ada, diperluas untuk `RANAP` | Sesuai produsen | 4.4 | Billing | — |
| `INT-RWF-05` | Farmasi → Billing | Fakta pembatalan klinis saat retur diverifikasi | Sesudah commit Farmasi | 4.5 | Billing | Disetujui `RWI-DEC-210` |
| `INT-RWF-06` | Rawat Inap → Clinical | Penutupan pemakaian alat yang masih berjalan saat keluar ruangan | Sesudah commit Rawat Inap | `keperawatan/contracts/integration-contract.md` bagian 9 | Clinical | — |

### 4.2 `INT-RWF-01` — ketukan pintu Rawat Inap ke Billing

**Event dan artinya.**

| Event | `SourceType` | `SourceId` | Diterbitkan saat | Arti bagi Billing |
|---|---|---|---|---|
| `ADMISSION_CONFIRMED` | `ADMISSION` | `EpisodeId` | Episode menjadi `Admitted` (`InpEpisodeService`, sudah ada) | Buka invoice `RANAP` untuk encounter itu bila belum ada |
| `BED_OCCUPIED` | `ROOM_STAY` | `PlacementId` | Bed pertama ditempati, **dan** penempatan baru karena transfer | Hitung ulang tarif kamar |
| `OCCUPANCY_CORRECTED` | `ROOM_STAY` | `PlacementId` baris koreksi | Koreksi salah catat (`InpPlacementCorrectionService`) — **tidak lagi** pada transfer biasa | Hitung ulang tarif kamar |
| `BED_RELEASED` | `DISCHARGE` | `PlacementId` | Keluar ruangan (`RecordPatientDepartureAsync`) dan penutupan yang melepas bed | Hitung ulang tarif kamar sampai waktu keluar |

**Isi pesan — daftar putih (`INV-RWF-05`).**

```json
{
  "eventType": "BED_RELEASED",
  "episodeId": "6a1f…",
  "encounterId": "c39b…",
  "sourceType": "DISCHARGE",
  "sourceId": "9e02…",
  "version": 2,
  "occurredAtUtc": "2026-10-04T02:05:00Z",
  "idempotencyKey": "INPATIENT:DISCHARGE:9e02…:2"
}
```

Field lain apa pun, termasuk ruang, kelas, waktu hunian, tarif, dan status kasir, ditolak saat pendaftaran outbox.

**Kunci idempotensi.** Format `INPATIENT:<SourceType>:<SourceId>:<Version>` (`RWI-DEC-161` butir 4). Putar ulang memakai kunci yang **sama** dengan pesan asli.

**Tanda terima.** `InpatientEventReceipt { Accepted, ReceiptId, Outcome, Message }`. Worker menandai `Published` hanya bila `Accepted = true`.

**Kegagalan.**

| Keadaan | Perilaku produsen | Perilaku konsumen |
|---|---|---|
| Billing melempar kesalahan atau tidak menjawab dalam batas waktu | `Failed`, coba ulang dengan backoff `min(2^n × 5 detik, 3600 detik)` | Transaksi Billing dibatalkan; tidak ada tanda terima tersimpan |
| Pesan dikirim dua kali | — | `DUPLICATE`, `Accepted = true`, tanpa efek kedua |
| Aplikasi mati saat `Processing` | Diambil ulang setelah masa sewa | — |
| Gagal 10 kali | `DeadLetter`, tampil di `GET integration-outbox` | — |
| Billing gangguan saat admisi | Admisi tetap tersimpan (`RWI-DEC-161`) | — |

**Contoh.** Tn. Budi dinyatakan `Admitted` pukul 09.00 saat Billing sedang restart. Pesan `ADMISSION_CONFIRMED` gagal dua kali, lalu pukul 09.20 diterima Billing; invoice `RANAP` terbuka, tanda terima `INVOICE_OPENED` kembali, dan pesan menjadi `Published`. Bila worker mengirim ulang pesan yang sama pukul 09.21, Billing menjawab `DUPLICATE` dan invoice tetap satu.

### 4.3 `INT-RWF-02` dan `INT-RWF-03` — bacaan status kasir

| Hal | Isi |
|---|---|
| Pemanggil | `InpBillingClearanceAdapter`, satu-satunya pembaca di Rawat Inap |
| Yang dipanggil | `InpatientClearanceService.GetLatestStatusAsync(encounterId)` dan versi daftar untuk banyak encounter |
| Keluaran | `InpatientClearanceStatusView { Status (PENDING/BLOCKED/CLEARED/REVOKED), Reasons[] { Code, Label }, EvaluatedAt, InvoiceStatus (OPEN/FINAL/CLOSED/NONE) }` |
| Yang **dilarang** ada di keluaran | `OutstandingBalance`, `TotalPatientResponsibility`, `TotalPaidOrAllocated`, dan field rupiah lain (`RWI-DEC-160`) |
| Invoice belum ada | `Status = PENDING`, `InvoiceStatus = NONE` (perilaku `InpatientClearanceService.cs:64-66` yang sudah ada) |
| Gagal dibaca | Adapter mengembalikan `IsReadable = false` dan tidak melempar kesalahan. Keputusan diambil pemanggil: keluar ruangan memberi peringatan; penutupan menolak |
| Kekinian | Tidak ada cache. Setiap pemanggilan membaca keadaan terbaru, sehingga pencabutan izin langsung terlihat |
| Batas waktu | Mengikuti batas waktu query database aplikasi. Lewat batas = `IsReadable = false` |

### 4.4 `INT-RWF-04` — layanan klinis rawat inap ke invoice

| Produsen | Titik tagih (sudah berjalan, `RWI-DEC-195`) | Jalur |
|---|---|---|
| Laboratorium | Spesimen diterima (`LabSpecimenService.AcceptAsync`) | Folio → `BillingClinicalChargeBridgeService` → invoice `RANAP` |
| Radiologi | Kualitas citra diputuskan (`RadStudyService.DecideQualityAsync`) | Sama |
| Farmasi | Obat diserahkan (`PrescriptionDispensingService.DispenseAsync`). MAR **tidak** menagih | Sama |
| Tindakan | Order tindakan `Completed` (`PatientProcedureController`) | Sama |
| Pemakaian alat | Pemakaian selesai (`keperawatan` kontrak `0.6.0`) | Sama, `SourceContext = EQUIPMENT_USAGE` |
| Kamar Operasi | Kasus `Completed` (`episode-rawat-inap` kontrak `0.10.0`) | Sama, `SourceContext = OPERATING_ROOM` |

Perubahan Billing: penolakan `NotOutpatient` dicabut untuk kunjungan `Inpatient`; invoice tujuannya `RANAP`. Charge line yang tarifnya tidak ditemukan tetap tercatat `TARIFF_NOT_FOUND` dan menahan finalisasi (`VAL-RWF-17`).

### 4.5 `INT-RWF-05` — retur obat membatalkan tagihan

| Hal | Isi |
|---|---|
| Pemicu | `DrugReturnService.VerifyAsync` menetapkan jumlah layak kembali, lalu transaksi Farmasi commit |
| Yang dikirim | Fakta pembatalan klinis untuk baris serah obat yang sama (`SourceDrugUsageId` atau `SourceOprMaterialUsageId`), dengan `Quantity` = jumlah layak kembali dan rujukan nomor retur di `RuleSnapshot` |
| Akibat di Billing | Kuantitas tertagih berkurang sebanyak jumlah itu lewat revisi baru; baris asli tetap ada; pembatalan dapat ditelusuri ke nomor retur |
| Retur tidak layak | Tidak ada fakta yang dikirim; tagihan tetap |
| Invoice sudah final | Tidak lewat jalur ini; kasir memakai adjustment Billing (`RWI-DEC-195` butir 4) |
| Gerbang | ~~`RWI-OQ-108`~~ — **disetujui Ikbal Yulianto (pemilik Farmasi) lewat `RWI-DEC-210`, 2 Oktober 2026** |

**Contoh.** Farmasi menyerahkan 3 vial ceftriaxone untuk Tn. Budi pukul 08.00, sehingga invoice memuat 3 vial. Pukul 10.30 apoteker memverifikasi retur 1 vial sebagai layak. Billing menerima pembatalan 1 vial dengan rujukan `RTR-2026-000045`, dan tagihan ceftriaxone menjadi 2 vial.

### 4.6 Penyelarasan decision log revision `31` — `INT-RWF-29` ★ 2 Oktober 2026

| ID | Dari → ke | Pemicu | Isi | Sinkron | Idempotensi | Bila gagal | Rekonsiliasi |
|---|---|---|---|---|---|---|---|
| `INT-RWF-29` | Billing ← Rawat Inap (baca) | Billing memproses `ADMISSION_CONFIRMED` | `InpAdmissionReferral` dengan `CompletedEpisodeId` = episode: `SourceEncounterId`, `Id` | Ya, dalam transaksi Billing pembukaan invoice | Unique (`RanapInvoiceId`, `LinkedEncounterId`) | Transaksi Billing batal; pesan `Failed` dan dicoba ulang dengan backoff `INT-RWF-01` | Putar ulang `I5` membuat tautan yang terlewat tanpa menggandakan |

Pesan `ADMISSION_CONFIRMED` **tidak** membawa `SourceEncounterId`; daftar putih isi pesan (`INV-RWF-05`) tetap. Keputusan: `RWI-DEC-207`. Konsumen tautan: Tagihan Pasien (`breakdown`) dan penyelesaian satu kwitansi `BKC-DEC-118` milik `billing-kasir`.
