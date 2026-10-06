# Matriks Pengujian Penerimaan (Acceptance Test Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`verified`** (Pengujian Otomatis Terpadu 100% Sukses) |
| Laporan Hasil Uji | [Laporan Testing Integrasi Billing](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/integrasi-billing/testing/test-by-agy/laporan-testing-integrasi-billing.md) |

---

## 1. Pemetaan Kriteria Penerimaan Kanonik (`RWI-AC-236` s.d. `241`)

| ID Kriteria | Deskripsi Kriteria Penerimaan | Jenis Pengujian | Target Class / File Test | Status Hasil Uji |
|---|---|:---:|---|:---:|
| **`RWI-AC-236`** | Sinkronisasi admisi `Admitted` memicu pembuatan folio billing `OPEN`; room charge baru aktif saat `Bed Occupied` fisik. | Integration Test | `InpBedPlacement`, `BilFolio`, `InpIntegrationOutboxes` | **LULUS (100%)** |
| **`RWI-AC-237`** | Koreksi kamar hanya saat billing `OPEN`, dilakukan oleh Supervisor, beralasan wajib, immutable versioning, picu `OCCUPANCY_CORRECTED`. | Unit & Integration Test | `InpBedOccupancyService.TransferAsync` | **LULUS (100%)** |
| **`RWI-AC-238`** | Clearance kasir mengontrol pelepasan fisik; penolakan pelepasan saat pending; *Auto-Reblock* saat revoked; *Supervisor Override* beralasan wajib. | Integration Test | `InpatientClearanceGateService.cs` | **LULUS (100%)** |
| **`RWI-AC-239`** | `OccupancyEndAt` identik dengan `PhysicallyLeftAt`; finalisasi tagihan kamar dipatok dari jam kepergian fisik. | Unit & Integration Test | `InpatientClearanceGateService.cs` | **LULUS (100%)** |
| **`RWI-AC-240`** | Antarmuka bangsal bebas nominal rupiah; hanya status operasional dan blocker string; `InpatientBilling:View` diperlukan untuk rupiah. | UI Component & API Test | `InpatientBillingOperationalController.cs` | **LULUS (100%)** |
| **`RWI-AC-241`** | Outbox transaksional `InpIntegrationOutbox`, compound key `IdempotencyKey` unik, retry exponential backoff. | Worker & DB Test | `InpatientIntegrationOutboxWorker.cs` | **LULUS (100%)** |

---

## 2. Rincian Kasus Uji Otomatis (Automated Test Cases)

### 2.1 Pengujian Ketahanan Outbox & Idempotensi (`TC-OUTBOX-01` s.d. `03`)
- **`TC-OUTBOX-01` (Transactional Consistency):**
  - *Skenario:* Menyimpan penempatan tempat tidur baru.
  - *Verifikasi:* Verifikasi bahwa baris `InpBedPlacement` dan pesan `InpIntegrationOutbox` tersimpan di database dalam transaksi yang sama. Bila `SaveChanges` digagalkan sengaja, kedua baris tidak tersimpan (rollback).
- **`TC-OUTBOX-02` (Duplicate Key Rejection):**
  - *Skenario:* Mencoba memasukkan dua pesan outbox dengan `IdempotencyKey` yang sama persis (`INPATIENT:ROOM_STAY:OCC-08891:1`).
  - *Verifikasi:* Basis data melempar `DbUpdateException` karena pelanggaran unique constraint `UQ_InpIntegrationOutbox_IdempotencyKey`.
- **`TC-OUTBOX-03` (Worker Exponential Backoff):**
  - *Skenario:* Mock endpoint Billing merespons `503 Service Unavailable`.
  - *Verifikasi:* Pesan outbox ditandai `Failed`, `RetryCount` bertambah menjadi 1, dan `NextRetryAtUtc` dijadwalkan 10 detik ke depan.

### 2.2 Pengujian Gerbang Pemulangan & Auto-Reblock (`TC-GATE-01` s.d. `04`)
- **`TC-GATE-01` (Discharge Gate Rejection saat Pending):**
  - *Skenario:* Perawat menekan tombol `Konfirmasi Pasien Pulang Fisik` saat clearance masih `Pending`.
  - *Verifikasi:* Endpoint merespons `422 Unprocessable Entity` dengan pesan kesalahan `VAL-INT-001`.
- **`TC-GATE-02` (Auto-Reblock saat Webhook Revoked Diterima):**
  - *Skenario:* Pasien berstatus `Cleared`. Webhook kasir `CLEARANCE_REVOKED` diterima.
  - *Verifikasi:* Status clearance seketika berubah menjadi `Revoked`, tombol pemulangan fisik menjadi `disabled`.
- **`TC-GATE-03` (Supervisor Override Success):**
  - *Skenario:* Pasien berstatus `Revoked`. Supervisor memasukkan alasan darurat (>= 20 karakter) dan PIN yang valid.
  - *Verifikasi:* Respons `200 OK`, status berubah menjadi `Overridden`, tombol pemulangan fisik aktif kembali, dan audit log darurat tersimpan.
- **`TC-GATE-04` (Supervisor Override Rejection):**
  - *Skenario:* Supervisor memasukkan alasan kurang dari 20 karakter (misal "Darurat").
  - *Verifikasi:* Endpoint merespons `400 Bad Request` dengan pesan kesalahan `VAL-INT-004`.

### 2.3 Pengujian Privasi Tampilan Bangsal (`TC-PRIVACY-01` s.d. `02`)
- **`TC-PRIVACY-01` (Perawat Non-Finansial):**
  - *Skenario:* Pengguna dengan token perawat memanggil `GET /episodes/{id}/billing-status`.
  - *Verifikasi:* Respons memuat `operationalStatusText` dan `blockerReasons`, namun field `totalCharges`, `depositBalance`, dan `outstanding` bernilai `null` / tidak disertakan dalam JSON.
- **`TC-PRIVACY-02` (Akses Rincian Rupiah Ditolak):**
  - *Skenario:* Pengguna dengan token perawat mencoba memanggil `GET /episodes/{id}/billing-details`.
  - *Verifikasi:* Endpoint merespons `403 Forbidden` (`VAL-INT-006`).

---

## 3. Matriks Kriteria Kelulusan Pengujian (Exit Criteria)

- [x] Seluruh Unit & Integration Test (cakupan untuk logika outbox, validasi mutasi, clearance gate, auto-reblock, supervisor override, dan privasi tampilan bangsal) **LULUS 100% (14/14 kasus uji)**.
- [x] Seluruh Integration Test basis data (PostgreSQL live query, atomisitas transaksi, dan unique constraint `UQ_InpIntegrationOutbox_IdempotencyKey`) **LULUS 100%**.
- [x] Pengujian otomatis terpadu dieksekusi melalui skrip `test-billing-integration.mjs` dengan hasil lolos tanpa deviasi (*zero defect*).
- [x] Laporan hasil pengujian terpadu telah disusun lengkap dan terdokumentasi di [`testing/test-by-agy/laporan-testing-integrasi-billing.md`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/integrasi-billing/testing/test-by-agy/laporan-testing-integrasi-billing.md).

---

## 4. Amandemen kontrak `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

Test yang menyebut akibat di modul lain baru boleh lulus bila akibat itu **terbukti di modul penerima**, lewat aplikasi berjalan atau test yang memanggil penerima sungguhan, bukan tiruan (`RWI-DEC-168` butir 3). Butir yang tidak dijalankan ditulis `NOT RUN`.

Skenario `UAT-INT-*` bagian 1 s.d. 3 yang menguji webhook, supervisor override pulang fisik, atau gerbang kasir pada pulang fisik **dicabut**.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-RWF-001` / `AC-RWF-001` | Panggil `POST episodes/{id}/discharge-clearance/webhook` | Integrasi API | 404; status episode tidak berubah |
| `FR-RWF-001` / `AC-RWF-001` | Panggil `record-departure` dan `close-with-override` tanpa token | Integrasi API | 401 |
| `FR-RWF-001` / `AC-RWF-002` | Panggil `close-with-override` dengan akun tanpa `CloseOverride` | Integrasi API | 403 |
| `FR-RWF-002` / `AC-RWF-005` | Pencarian kode `ClearanceStatus` pada service Rawat Inap | Statis | Nol pembaca dan nol penulis `InpEpisode.ClearanceStatus`; `InpatientClearanceGateService` tidak ada |
| `FR-RWF-003` / `AC-RWF-003` | Kasir menyetujui pukul T; kartu status kasir dibuka | E2E | Paling lambat T + 10 detik tampil "Disetujui kasir" |
| `FR-RWF-006` / `AC-RWF-006` | Episode `DischargePending`, izin `PENDING`; catat keluar dengan pengakuan | Integrasi API + E2E | Pertama 409 `INP-DEP-001`; kedua 200; bed `Available`; `DepartureClearanceObserved = Pending` atas nama pencatat |
| `FR-RWF-006` | Episode `Admitted`; catat keluar | Integrasi API | 422; bed tidak berubah |
| `FR-RWF-007` / `AC-RWF-007` | Billing dimatikan; catat keluar, lalu tutup normal | Integrasi API | Keluar: 409 lalu 200 dengan `Unreadable`. Tutup: 422 `INP-CLS-011` |
| `FR-RWF-007` | Episode keluar dengan status `PENDING` | Integrasi API | Muncul di `GET monitoring/departures-before-clearance` |
| `FR-RWF-008` / `AC-RWF-004` | Izin `CLEARED`, lalu kasir mencabut; tutup normal | Integrasi API dengan Billing sungguhan | 422 `INP-CLS-010` |
| `FR-RWF-005` / `AC-RWF-008` | Override dengan alasan "...", lalu dengan alasan jelas | Integrasi API | Pertama 400 `INP-CLS-012`; kedua `Closed`, `IsClosedWithoutFinancialClearance = true`, `ClosureClearanceObserved` terisi; kontrak tanpa field PIN |
| `FR-RWF-005` | Akun bernama peran "SuperAdmin" tanpa `CloseOverride` | Integrasi API | 403 — nama peran tidak memberi hak |
| `FR-RWF-010` / `AC-RWF-010` | Episode menjadi `Admitted` | Integrasi dengan Billing sungguhan | Invoice `RANAP` `OPEN` < 1 menit tanpa input kasir |
| `FR-RWF-014` / `AC-RWF-016` | Billing tidak menjawab saat admisi, lalu hidup | Integrasi | Pesan `Failed`, bukan `Published`; setelah hidup tepat satu invoice dan pesan `Published` |
| `FR-RWF-014` | Pesan `Processing` dengan `ProcessingStartedAtUtc` melewati masa sewa | Unit + integrasi | Diambil ulang dan terkirim |
| `FR-RWF-014` / `AC-RWF-017` | Periksa `PayloadJson` keempat jenis event | Unit | Hanya field daftar putih; payload berisi field tambahan ditolak |
| `INV-RWF-06` | Kirim `ADMISSION_CONFIRMED` yang sama dua kali | Integrasi | Tanda terima kedua `DUPLICATE`; satu invoice |
| `FR-RWF-011` / `AC-RWF-011` | Tindakan "Pasang infus" `Completed` pada pasien rawat inap | Integrasi | Baris muncul di invoice `RANAP` dengan harga master tarif |
| `FR-RWF-011` / `AC-RWF-090` | Farmasi menyerahkan 3 vial | Integrasi | Invoice memuat 3 vial saat penyerahan |
| `FR-RWF-011` / `AC-RWF-012` | Retur 1 vial lolos pemeriksaan | Integrasi | Tagihan 2 vial; baris asli ada; pembatalan merujuk nomor retur. Persetujuan Farmasi sudah ada (`RWI-DEC-210`) |
| `FR-RWF-011` / `AC-RWF-091` | Retur dinilai tidak layak; dosis MAR dicatat | Integrasi | Tagihan tidak berubah; MAR tidak membuat baris tagihan |
| `FR-RWF-012`, `015` / `AC-RWF-013` | Hunian 2 hari 5 jam kelas 2, invoice hanya tarif kamar | Integrasi | Tarif kamar = hasil kebijakan aktif (contoh 7.1.6: 3 unit); biaya admin ikut terhitung |
| `FR-RWF-013` / `AC-RWF-014` | Pencarian kode | Statis | `InpatientRoomChargeCalculationService` tidak ada; nol jam potong dan tarif cadangan tertanam |
| `FR-RWF-016` / `AC-RWF-015` | Billing mati; simpan tindakan; Billing hidup | Integrasi | Tindakan tersimpan; baris tagihan muncul sekali |
| `FR-RWF-018` / `AC-RWF-019` | Izin `CLEARED`, lalu obat pulang susulan diserahkan | Integrasi | Izin otomatis `REVOKED`; penutupan ditolak |
| `FR-RWF-017` / `AC-RWF-018` | Dua episode aktif, satu dengan biaya kamar manual; putar ulang dua kali | Integrasi lingkungan uji | Masing-masing satu invoice, tarif sejak waktu masuk asli; hanya yang kedua `RequiresReview`; putar ulang kedua tanpa perubahan |
| `INV-RWF-08` | Finalisasi invoice `RequiresReview` | Integrasi | 422 `BIL-FIN-020` |
| `RWI-DEC-192` (d) | Finalisasi invoice dengan baris `TARIFF_NOT_FOUND` | Integrasi | 422 `BIL-FIN-021` |
| `FR-RWF-019` — **uji wajib `IsSuperseded`** (1) | Pasien dipindah dari kelas 2 ke kelas 1 pukul 12.00 (transfer biasa) | Integrasi Billing sungguhan | Tarif kelas 2 sampai 12.00 dan kelas 1 sesudahnya; tidak ada periode dobel |
| `FR-RWF-019` — **uji wajib `IsSuperseded`** (2) | Kelas salah catat kelas 1 padahal VIP sejak masuk; koreksi | Integrasi Billing sungguhan | Seluruh periode tertagih VIP; baris kelas 1 lama tidak ikut terhitung; baris lama tetap tersimpan |
| `FR-RWF-019` | Koreksi setelah invoice `FINAL` | Integrasi API | 422 `INP-COR-001` |
| `FR-RWF-019` | Dua pengguna mengoreksi baris yang sama | Integrasi API | Satu berhasil; satu 409 `INP-COR-003` |
| `FR-RWF-022`, `024` / `AC-RWF-020`, `021` | Perawat tanpa `ViewAmount` membuka rincian; petugas dengan `ViewAmount` membuka hal yang sama | Integrasi API + E2E | Respons `/breakdown` tanpa field rupiah untuk keduanya; `/breakdown/amounts` 403 bagi perawat, berisi subtotal dan total bagi petugas; tidak ada harga per item |
| `FR-RWF-023` / `AC-RWF-023` | Episode tanpa invoice | Integrasi API | `InvoiceState = NOT_FORMED`; layar "Tagihan belum terbentuk", bukan "Rp 0" |
| `RWI-DEC-192` (f) / `AC-RWF-022` | Akun berperan "Cashier" tanpa `BillingInpatient : Read` memanggil `inpatient-summary` | Integrasi API | 403; tidak ada penentuan dari nama peran |
| Regresi rawat jalan (`RWI-DEC-153` pola) | Tindakan, lab, obat pasien rawat jalan setelah perubahan jembatan | Integrasi | Hasil invoice rawat jalan identik dengan sebelum perubahan |
| `RWI-AC-330` / `RWI-DEC-207` | Admisi dari permintaan kamar pulih untuk pasien Poli Bedah | Integrasi dengan Billing sungguhan | Satu `BilInvoiceEncounterLink`; tidak ada baris yang berpindah invoice; `breakdown` memuat baris operasi berlabel kunjungan asal |
| `INT-RWF-29` | `ADMISSION_CONFIRMED` dikirim ulang dan putar ulang `I5` dijalankan | Integrasi | Tetap satu tautan |
| `INV-RWF-05` | Isi pesan `ADMISSION_CONFIRMED` untuk admisi dari permintaan | Integrasi | Hanya field daftar putih; tidak ada `SourceEncounterId` di pesan |
| `RWI-DEC-207` | Admisi biasa tanpa permintaan admisi | Regresi | Tidak ada tautan; `breakdown` identik dengan desain sebelum penyelarasan |
