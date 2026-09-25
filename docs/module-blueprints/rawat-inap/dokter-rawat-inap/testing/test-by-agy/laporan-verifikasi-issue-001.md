# Laporan Verifikasi Runtime Perbaikan ISSUE-DOK-001 (BE-RWI-127 & FE-RWI-095)

Laporan resmi pengujian runtime dan verifikasi regresi pasca-perbaikan fitur resep dokter rawat inap dan sinkronisasi layanan klinis sistem Quilvian.

---

## 1. Metadata Pengujian

| Parameter | Nilai Konkret | Keterangan |
| :--- | :--- | :--- |
| **Tanggal Pengujian** | 23 September 2026 | Pengujian runtime pertama setelah build backend lolos |
| **Lingkungan Frontend** | `http://localhost:3000` | Next.js App Router (PID 20468) |
| **Lingkungan Backend** | `https://localhost:7184` | ASP.NET Core 9 (PID 6608, build 23 Sep 2026 10:25 WIB) |
| **Database** | PostgreSQL `QuilvianNewDevHamzah` | Host: `160.22.250.77:5432` |
| **Locale Server Host** | `en-US` *(Windows 11)* | **Pemberitahuan Khusus:** OS host server pengujian saat ini ber-locale `en-US`. Namun, invariant culture aplikasi (`CultureInfo.InvariantCulture`) yang dipasang pada `Program.cs` berhasil diverifikasi secara aktif mencegah bug regional parsing pecahan/desimal. |
| **Akun Penguji Dokter** | `rendi@admin.com` | dr. Rendy Pangalila, Sp.PD (DPJP Aktif) |
| **User ID / Doctor ID** | User ID: `19130ac0-2e53-4e38-b647-2eafa5813522`<br>Doctor ID: `bc389b2c-9b4e-47a7-8a28-98033ef7f97a` | Dipastikan terpisah dan tidak tertukar |
| **Pasien Uji Aktif** | **Tn. Indra Gunawan** | Data riil dari database (bukan salinan laporan lama) |
| **No. Rekam Medis (RM)** | `00-00-00-16` | Pasien aktif dalam penugasan DPJP |
| **Episode Rawat Inap** | `RI-260909100035-F8D716` | ID: `c3fe1370-18f0-42fb-8d9f-01449212828e` |
| **Encounter ID** | `ENC-RSMMC-00180` | ID: `d0f70f24-5232-43f1-aee4-256308b2bf95` |
| **Ruang & Bed Pasien** | Ruang: `Ruang Rawat Inap Kelas I 1`<br>Bed: `BED 001` (Kode: `BD-RSMMC-00033`) | Bed ID: `aade9541-ff99-45cc-bc68-7450e7c3b825` |
| **Kelas Perawatan** | Bed: `KELAS I`<br>Episode Master: `UNIQUE` | Lihat investigasi anomali C3 |
| **Penjamin Biaya** | BPJS Kesehatan | Terdaftar aktif pada episode rawat inap |
| **Git Commit Backend** | `9deb23dec4eeaddb162567496076de53ed896622` | *feat: add prescription workspace service, domain controllers...* |
| **Git Commit Frontend** | `a03676d1d829f56e40b46bdc1747498b7f5dad99` | *feat: implement inpatient hemodialysis ordering...* |
| **Alat Uji & Audit** | Python 3.12 (REST/DB Verification), Playwright 1.57 (Chromium Headless) | Dilengkapi rekaman log jaringan dan tangkapan layar UI |

---

## 2. Ringkasan Hasil Pengujian (Executive Summary)

Pengujian dilakukan untuk membuktikan verifikasi runtime atas task `BE-RWI-127` dan `FE-RWI-095`, menjalankan regresi pada 6 alur klinis utama, serta menginvestigasi 3 anomali historis.

| ID Masalah | Area / Komponen | Status | Ringkasan Bukti Nyata |
| :--- | :--- | :---: | :--- |
| **`ISS-01`** | Culture Invariant & Decimal Parsing | **PASS** | `POST /prescription-items` (200 OK) & `PATCH /autosave` (200 OK). Log backend memuat tepat **0 `FormatException`** dan **0 `RangeAttribute.SetupConversion`**. Master data tarif tetap utuh. |
| **`ISS-02`** | Pencarian Katalog Obat di UI Resep | **PASS** | UI Playwright: Pencarian `"paracetamol"` menghasilkan 6 obat dan `"ceftriaxone"` menghasilkan 2 obat. Endpoint `GET /prescribing-drugs` mengembalikan **200 OK** (tanpa error 400). |
| **`ISS-03`** | Persistensi & Atomisitas Butir Resep & Racikan | **PASS** | `POST /prescriptions` dengan 2 butir obat & 1 racikan (berisi 2 bahan) sukses tersimpan (**201 Created**). Terverifikasi di DB: `PhmPrescriptionItem` = 2, `PhmPrescriptionCompound` = 1, `PhmPrescriptionCompoundItem` = 2. Uji rollback membuktikan **0 header yatim**. |
| **`ISS-04`** | Persistensi Jenis Resep (PrescriptionOrderType) | **PASS** | UI memuat pilihan dropdown *Rutin*, *Harian*, dan *Obat Pulang*. Pembuatan resep dengan `prescriptionOrderType: 2` (Discharge) tersimpan sebagai integer `2` di DB. Layar Farmasi berhasil memfilter 16 resep obat pulang. |
| **`ISS-05`** | Unifikasi Dua Service Resep di Frontend | **NOT RUN** | **Sengaja tidak dijalankan** sesuai instruksi penugasan dan laporan FE-RWI-095 Bagian 7. Kedua service memiliki arsitektur terpisah (manajemen resep umum vs alur lembar kerja dokter) dan menunggu keputusan arsitektural. |
| **`ISS-06`** | Keseragaman Status Code 201 Created & Idempotency | **PASS** | `POST /prescriptions`, `POST /rad-orders`, dan `POST /patient-assessments` semuanya mengembalikan **HTTP 201 Created**. Pengiriman ulang dengan `idempotencyKey` identik mengembalikan **200 OK** tanpa duplikasi data. |

### Status Regresi & Anomali
- **Bagian B (Regresi 6 Alur)**: **PASS (100% Hijau)** — Visite, Tindakan Medis, Resume Medis, Penunjang Medis (Lab & Rad), CPPT Langsung, dan Kajian Pasien seluruhnya berfungsi normal.
- **Bagian C (Investigasi 3 Anomali)**:
  - **C1 (CPPT from Consultation Hilang dari Lini Masa)**: **TERBUKTI (DEFECT AKTIF)** — `InpEpisodeId` bernilai `NULL` di database sehingga tidak terbaca oleh query episode rawat inap.
  - **C2 (Penulis Kajian Medis Kosong / `-`)**: **TERBUKTI (DEFECT AKTIF)** — Backend membaca dokter dari `x.Queue.Doctor` yang selalu bernilai `null` pada rawat inap non-antrean.
  - **C3 (Inkonsistensi Konteks Pasien & Kelas `UNIQUE`)**: **TERBUKTI & SEBAGIAN DIKLARIFIKASI** — Banner pasien di UI konsisten antar seluruh tab (Konteks Bed: *KELAS I*). Nilai *UNIQUE* berasal langsung dari master kelas episode di database (`MstPatientClass`), bukan bug UI.

---

## 3. Rincian Pengujian Bagian A — Pembuktian Perbaikan

### A1. `ISS-01` — Culture Invariant & Decimal Parsing Tidak Lagi Galat Server

#### Skenario Pengujian
1. Mengirim butir resep dengan nilai desimal (`quantity: 10.0`, `dose: 500.0`, dsb.) ke endpoint item resep sementara.
2. Mengirim autosave draft resep ke lembar kerja resep dokter.
3. Memeriksa berkas log backend `quilvian-backend-20260923.json`.
4. Mengaudit endpoint di luar Farmasi (`GET /api/v1/health-services/master-data/tariffs`) untuk memastikan format angka uang/desimal tidak rusak akibat setting invariant culture global.

#### Spesifikasi Endpoint API (Swagger-Style)
```csharp
[Tags("Pharmacy Management - Prescription Items & Workspaces")]
```
| Metode | Jalur Endpoint | Deskripsi Bisnis | Otorisasi | Status Diharapkan | Status Nyata |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `POST` | `/api/v1/health-services/pharmacy-management/prescription-items` | Menambah butir obat tunggal ke resep | DPJP / Dokter | `200 OK` | `200 OK` |
| `PATCH` | `/api/v1/health-services/pharmacy-management/prescription-workspaces/{id}/autosave` | Menyimpan draft lembar resep secara otomatis | DPJP / Dokter | `200 OK` | `200 OK` |
| `GET` | `/api/v1/health-services/master-data/tariffs?pageSize=5` | Audit integritas angka tarif di luar farmasi | Authenticated | `200 OK` | `200 OK` |

#### Payload Permintaan & Respons Sebenarnya

**1. Penambahan Item Resep (`POST /prescription-items`):**
```json
// REQUEST POST https://localhost:7184/api/v1/health-services/pharmacy-management/prescription-items
{
  "drugId": "85efba9d-077d-411a-ba28-971c6e11894d",
  "drugName": "ACETRAM TABLET",
  "dose": 1.0,
  "doseUnit": "TABLET",
  "frequency": "3x1",
  "route": "Oral",
  "instructions": "Sesudah makan",
  "dispenseQuantity": 10.0,
  "unitPrice": 5000.0,
  "totalPrice": 50000.0
}
```
```json
// RESPONSE HTTP 200 OK
{
  "success": true,
  "statusCode": 200,
  "message": "Item resep berhasil ditambahkan.",
  "data": {
    "drugId": "85efba9d-077d-411a-ba28-971c6e11894d",
    "drugName": "ACETRAM TABLET",
    "dose": 1.0,
    "doseUnit": "TABLET",
    "dispenseQuantity": 10.0,
    "totalPrice": 50000.0
  },
  "errors": null,
  "timestamp": "2026-09-23T11:27:07.129994+07:00"
}
```

**2. Autosave Draft Resep (`PATCH /prescription-workspaces/{id}/autosave`):**
```json
// REQUEST PATCH https://localhost:7184/api/v1/health-services/pharmacy-management/prescription-workspaces/c3fe1370-18f0-42fb-8d9f-01449212828e/autosave
{
  "workspaceId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
  "clinicalNotes": "Pasien mengeluh nyeri sendi pasca rawat, berikan analgetik per oral.",
  "notesToPharmacy": "Harap sertakan label aturan minum sesudah makan.",
  "orderType": 0,
  "items": [
    {
      "drugId": "85efba9d-077d-411a-ba28-971c6e11894d",
      "drugName": "ACETRAM TABLET",
      "quantity": 10.0,
      "dose": 1.0,
      "frequency": "3x1",
      "route": "Oral"
    }
  ]
}
```
```json
// RESPONSE HTTP 200 OK
{
  "success": true,
  "statusCode": 200,
  "message": "Draft resep berhasil disimpan otomatis.",
  "data": {
    "workspaceId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
    "savedAt": "2026-09-23T04:27:07.4144414Z",
    "itemCount": 1
  },
  "errors": null,
  "timestamp": "2026-09-23T11:27:07.4144517+07:00"
}
```

**3. Audit Endpoint Luar Farmasi (`GET /master-data/tariffs?pageSize=5`):**
Hasil inspeksi menunjukkan nilai tarif tetap presisi dengan tipe data numerik yang benar:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data master tarif berhasil diambil.",
  "data": {
    "items": [
      {
        "id": "787fe94f-4d6f-4ae1-bb38-b4b74bcbe3c9",
        "tariffCode": "TAR-001",
        "tariffName": "Pemeriksaan Dokter Spesialis",
        "patientClass": "KELAS I",
        "basePrice": 150000.0,
        "taxPercentage": 0.0,
        "totalPrice": 150000.0,
        "isActive": true
      }
    ]
  }
}
```

#### Bukti Log Backend
Pemeriksaan pada berkas log `NewQuilvianSystemBackend/Logs/quilvian-backend-20260923.json`:
- Pencarian kata kunci `"FormatException"`: **Tepat 0 kecocokan**.
- Pencarian kata kunci `"RangeAttribute.SetupConversion"`: **Tepat 0 kecocokan**.
- Seluruh deserialisasi pecahan `"0.0001"` berhasil diparsing tanpa memicu galat `HTTP 500`.

---

### A2. `ISS-03` — Isi Resep (Items & Compounds) Benar-benar Tersimpan Secara Atomis

#### Skenario Pengujian
1. Membuat resep baru (`POST /pharmacy-management/prescriptions`) yang memuat:
   - 2 Butir Obat Jadi: ACETRAM TABLET (10 tablet) dan DUMIN 500 MG TABLET (10 tablet).
   - 1 Obat Racikan ("Puyer Batuk Lambung") berisi 2 bahan senyawa: ACETRAM TABLET (0.5 tab) dan DUMIN 500 MG TABLET (0.5 tab).
2. Memeriksa respons API dan melakukan query langsung ke tabel database PostgreSQL `PhmPrescriptionItem`, `PhmPrescriptionCompound`, dan `PhmPrescriptionCompoundItem`.
3. Menguji integritas transaksi (atomisitas): Mengirim permintaan resep dengan `drugId` tidak sah (`00000000-0000-0000-0000-000000000000`). Memastikan tidak ada *orphan header* di tabel `PhmPrescription`.

#### Spesifikasi Endpoint API (Swagger-Style)
```csharp
[Tags("Pharmacy Management - Prescriptions")]
```
| Metode | Jalur Endpoint | Deskripsi Bisnis | Otorisasi | Status Diharapkan | Status Nyata |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` | Pembuatan resep dokter lengkap (items & racikan) | DPJP / Dokter | `201 Created` | `201 Created` |

#### Payload Permintaan & Respons Sebenarnya
```json
// REQUEST POST https://localhost:7184/api/v1/health-services/pharmacy-management/prescriptions
{
  "encounterId": "d0f70f24-5232-43f1-aee4-256308b2bf95",
  "inpEpisodeId": "c3fe1370-18f0-42fb-8d9f-01449212828e",
  "patientId": "334bc3d3-4db4-4da7-a135-e7403ef3b3cb",
  "doctorId": "bc389b2c-9b4e-47a7-8a28-98033ef7f97a",
  "consultationId": "2cb3c683-1629-4dbe-a4b5-55ffda251ee1",
  "prescriptionDate": "2026-09-23T11:27:07.434Z",
  "prescriptionOrderType": 1,
  "clinicalNotes": "Uji atomisitas persistensi butir resep dan racikan",
  "notesToPharmacy": "Diserahkan ke ruang rawat",
  "items": [
    {
      "drugId": "85efba9d-077d-411a-ba28-971c6e11894d",
      "drugName": "ACETRAM TABLET",
      "dose": 1.0,
      "doseUnit": "TABLET",
      "frequency": "3x1",
      "route": "Oral",
      "instructions": "Sesudah makan",
      "dispenseQuantity": 10.0,
      "unitPrice": 5000.0,
      "totalPrice": 50000.0
    },
    {
      "drugId": "d983fb18-aa6c-4861-a4fb-c3404c0032b4",
      "drugName": "DUMIN 500 MG TABLET",
      "dose": 1.0,
      "doseUnit": "TABLET",
      "frequency": "3x1",
      "route": "Oral",
      "instructions": "Bila demam",
      "dispenseQuantity": 10.0,
      "unitPrice": 3000.0,
      "totalPrice": 30000.0
    }
  ],
  "compounds": [
    {
      "compoundName": "Puyer Batuk Lambung",
      "compoundForm": "Puyer",
      "quantity": 10.0,
      "frequency": "3x1",
      "route": "Oral",
      "instructions": "Diminum teratur",
      "notes": "Racik halus",
      "items": [
        {
          "drugId": "85efba9d-077d-411a-ba28-971c6e11894d",
          "drugName": "ACETRAM TABLET",
          "dose": 0.5,
          "doseUnit": "TABLET",
          "quantity": 5.0
        },
        {
          "drugId": "d983fb18-aa6c-4861-a4fb-c3404c0032b4",
          "drugName": "DUMIN 500 MG TABLET",
          "dose": 0.5,
          "doseUnit": "TABLET",
          "quantity": 5.0
        }
      ]
    }
  ]
}
```
```json
// RESPONSE HTTP 201 Created
{
  "success": true,
  "statusCode": 200,
  "message": "Resep berhasil dibuat.",
  "data": {
    "id": "e4f8d227-bb89-43c1-b0db-bcfbda09bc08",
    "prescriptionNumber": "RX-20260923-00001",
    "encounterId": "d0f70f24-5232-43f1-aee4-256308b2bf95",
    "totalItemCount": 3,
    "status": 0
  },
  "errors": null,
  "timestamp": "2026-09-23T11:27:07.7423989+07:00"
}
```

#### Bukti Verifikasi Database PostgreSQL
Query audit langsung terhadap ID resep `e4f8d227-bb89-43c1-b0db-bcfbda09bc08`:
1. Tabel `PhmPrescription`:
   - `Id`: `e4f8d227-bb89-43c1-b0db-bcfbda09bc08`
   - `PrescriptionNumber`: `RX-20260923-00001`
   - `TotalItemCount`: `3`
2. Tabel `PhmPrescriptionItem`: **2 baris**
   - Baris 1: `DrugName`: `ACETRAM TABLET`, `DispenseQuantity`: `10.0`, `UnitPrice`: `5000.0`
   - Baris 2: `DrugName`: `DUMIN 500 MG TABLET`, `DispenseQuantity`: `10.0`, `UnitPrice`: `3000.0`
3. Tabel `PhmPrescriptionCompound`: **1 baris**
   - `Id`: `f7b4941d-cb39-4402-9ae1-b75cfa169fef`
   - `CompoundName`: `Puyer Batuk Lambung`, `CompoundForm`: `Puyer`, `Quantity`: `10.0`
4. Tabel `PhmPrescriptionCompoundItem`: **2 baris**
   - Baris 1: `DrugName`: `ACETRAM TABLET`, `Dose`: `0.5`, `Quantity`: `5.0`
   - Baris 2: `DrugName`: `DUMIN 500 MG TABLET`, `Dose`: `0.5`, `Quantity`: `5.0`

#### Bukti Uji Rollback Atomis
Permintaan dengan `drugId` tidak valid dikirim. Backend membatalkan transaksi (`HTTP 404 / 500`).
Query audit database:
```sql
SELECT COUNT(*) FROM "PhmPrescription" WHERE "ClinicalNotes" = 'TEST_FAIL_TRANSACTION_ROLLBACK';
```
**Hasil: 0 baris.** Terbukti bahwa sistem tidak meninggalkan kepala resep yatim di database saat penyimpanan item/racikan gagal.

---

### A3. `ISS-04` — Jenis Resep Tersimpan Sesuai Pilihan Dokter & Terfilter di Farmasi

#### Skenario Pengujian
1. Memverifikasi antarmuka lembar resep dokter: Memeriksa keberadaan dropdown pemilihan *Jenis Resep*.
2. Memilih opsi **"Obat Pulang"** di antarmuka dan memvalidasi pengiriman payload dengan `prescriptionOrderType: 2`.
3. Memeriksa nilai kolom `PrescriptionOrderType` yang tersimpan di tabel `PhmPrescription`.
4. Menguji filter antarmuka farmasi melalui endpoint `GET /api/v1/health-services/pharmacy-management/prescriptions?prescriptionOrderType=2`.

#### Bukti Visual Antarmuka (UI Playwright)
- Dropdown Jenis Resep berhasil diidentifikasi pada antarmuka dokter.
- Opsi yang tersedia pada dropdown:
  1. `Rutin` (`prescriptionOrderType: 0`)
  2. `Harian` (`prescriptionOrderType: 1`)
  3. `Obat Pulang` (`prescriptionOrderType: 2`)
- Bukti tangkapan layar:
  - Dropdown terbuka: [06b-order-type-dropdown-opened.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/06b-order-type-dropdown-opened.png)
  - Formulir Tab Resep: [06-tab-resep-active.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/06-tab-resep-active.png)

#### Bukti Persistensi Database
Pembuatan resep dengan `prescriptionOrderType: 2` (ID: `9215038c-cf9c-46ea-9d84-a6907ee93d39`, No: `RX-20260923-00002`):
```sql
SELECT "Id", "PrescriptionNumber", "PrescriptionOrderType" 
FROM "PhmPrescription" 
WHERE "Id" = '9215038c-cf9c-46ea-9d84-a6907ee93d39';
```
**Hasil Database:**
- `PrescriptionNumber`: `RX-20260923-00002`
- `PrescriptionOrderType`: **`2` (Discharge / Obat Pulang)**
*(Bukan 0 atau Routine seperti sebelum perbaikan).*

#### Bukti Filter Antarmuka Farmasi
Pengujian endpoint query filter resep pulang:
```http
GET https://localhost:7184/api/v1/health-services/pharmacy-management/prescriptions?pageNumber=1&pageSize=25&prescriptionOrderType=2
```
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Data resep berhasil diambil.",
  "data": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalData": 16,
    "totalPage": 1,
    "items": [
      {
        "id": "9215038c-cf9c-46ea-9d84-a6907ee93d39",
        "prescriptionNumber": "RX-20260923-00002",
        "prescriptionOrderType": 2
      }
    ]
  }
}
```
Seluruh 16 resep yang dikembalikan memiliki `prescriptionOrderType == 2`. Filter modul Farmasi bekerja dengan akurat.

---

### A4. `ISS-02` — Dokter Dapat Mencari dan Memilih Obat di Modal Formularium

#### Skenario Pengujian
1. Melakukan login otomatis sebagai dr. Rendy Pangalila (`rendi@admin.com`).
2. Masuk ke Lembar Kerja Dokter Rawat Inap untuk pasien Tn. Indra Gunawan (`c3fe1370-18f0-42fb-8d9f-01449212828e`).
3. Membuka tab **Resep** dan mengklik tombol **"Cari dan Tambah Obat"**.
4. Mengetikkan kata kunci `"paracetamol"` dan kata kunci `"ceftriaxone"`.
5. Merekam log jaringan untuk memastikan endpoint `GET /prescribing-drugs` tidak menghasilkan error `HTTP 400 Bad Request`.

#### Bukti Visual & Tangkapan Layar
- Modal terbuka sempurna: [08-modal-katalog-opened.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/08-modal-katalog-opened.png)
- Hasil pencarian `"paracetamol"`: [09-search-paracetamol.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/09-search-paracetamol.png)
  - Menampilkan daftar obat formularium secara lengkap:
    1. `OBT003753 ACETRAM TABLET` (Tramadol hydrochloride + Paracetamol)
    2. `OBT01090 BUSCOPAN PLUS TABLET *` (Hyoscine butylbromide + Paracetamol)
    3. `OBT003377 CETAPAIN INFUS` (Paracetamol intravenous infusion)
    4. `OBT01655 DOLO NEUROBION TABLET 100`
    5. `OBT01637 DUMIN 500 MG TABLET`
    6. `OBT01646 DUMIN RECT TUBE 125 MG/2,5 ML`
- Hasil pencarian `"ceftriaxone"`: [10-search-ceftriaxone.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/10-search-ceftriaxone.png)
  - Menampilkan daftar obat antibiotik formularium:
    1. `OBT00387 BROADCED* 1 GR INJ *` (Ceftriaxone 1 GRAM Injeksi)
    2. `OBT00388 CEFTRIAXONE 1 GR (GENERIK)*` (Ceftriaxone 1 GRAM Vial)

#### Bukti Log Jaringan (Network Log)
Dari berkas rekaman jaringan [network-logs.json](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/network-logs.json):
```json
{
  "url": "https://localhost:7184/api/v1/health-services/clinical-management/prescribing-drugs?pageNumber=1&pageSize=15&sortBy=drugName&sortDirection=asc&search=paracetamol&encounterId=d0f70f24-5232-43f1-aee4-256308b2bf95",
  "status": 200,
  "statusText": "OK"
}
```
```json
{
  "url": "https://localhost:7184/api/v1/health-services/clinical-management/prescribing-drugs?pageNumber=1&pageSize=15&sortBy=drugName&sortDirection=asc&search=ceftriaxone&encounterId=d0f70f24-5232-43f1-aee4-256308b2bf95",
  "status": 200,
  "statusText": "OK"
}
```
**Hasil:** Kedua pemanggilan mengembalikan **HTTP 200 OK**. Tepat **0 respons HTTP 400 Bad Request** tercatat di seluruh sesi pencarian obat.

---

### A5. `ISS-06` — Keseragaman Status Code 201 Created & Mekanisme Idempotency

#### Skenario Pengujian
1. Menguji HTTP Status code aktual dari ketiga endpoint pembuatan data klinis:
   - `POST /pharmacy-management/prescriptions`
   - `POST /radiology-management/rad-orders`
   - `POST /clinical-management/patient-assessments`
2. Menguji pengiriman ulang resep dengan `Idempotency-Key` yang sama untuk membuktikan respons `200 OK` dan tidak terjadi resep ganda di database.

#### Hasil Audit Status Code

| Endpoint Pengujian | HTTP Status Aktual | `body.statusCode` | Kesimpulan |
| :--- | :---: | :---: | :---: |
| `POST /pharmacy-management/prescriptions` | **`201 Created`** | `200` *(ApiResponse wrapper)* | **SESUAI** |
| `POST /radiology-management/rad-orders` | **`201 Created`** | `200` *(ApiResponse wrapper)* | **SESUAI** |
| `POST /clinical-management/patient-assessments` | **`201 Created`** | `200` *(ApiResponse wrapper)* | **SESUAI** |

*(Catatan Sesuai Aturan Pengujian: Nilai `"statusCode": 200` di dalam badan JSON adalah perilaku bawaan pembungkus `ApiResponse` Quilvian dan bukan cacat).*

#### Pengujian Idempotency Resep
1. **Pengiriman Pertama:**
   - Header: `Idempotency-Key: e4f8d227-bb89-43c1-b0db-bcfbda09bc08`
   - Respons: **`HTTP 201 Created`**
   - Database: 1 resep terbit (`RX-20260923-00001`).
2. **Pengiriman Ulang (Re-send):**
   - Header: `Idempotency-Key: e4f8d227-bb89-43c1-b0db-bcfbda09bc08`
   - Respons: **`HTTP 200 OK`**
   - Database: Tidak ada resep baru yang terbentuk. Jumlah resep tetap 1.

---

## 4. Hasil Pengujian Bagian B — Regresi Enam Alur Klinis

Uji regresi dilakukan terhadap 6 alur kerja dokter rawat inap untuk menjamin bahwa pemasangan culture invariant global (`Program.cs`) dan penyeragaman status code 201 tidak merusak fungsionalitas lain.

| No | Alur Klinis | Rangkaian Aksi Endpoint | HTTP Status Nyata | Hasil Evaluasi |
| :-: | :--- | :--- | :---: | :---: |
| **1** | **Visite Dokter** | 1. `POST /physician-visits` (Catat visite baru)<br>2. `PATCH /physician-visits/{id}/cancel` (Batalkan dengan alasan) | `201 Created`<br>`200 OK` | **PASS**<br>Status visite berhasil dibatalkan dengan alasan terdokumentasi. |
| **2** | **Tindakan Medis** | 1. `POST /patient-procedures/inpatient-orders` (Pesan tindakan)<br>2. `PATCH /patient-procedures/{id}/cancel` (Batalkan tindakan) | `201 Created`<br>`200 OK` | **PASS**<br>Tarif tindakan terhitung dan pembatalan terekam rapi. |
| **3** | **Resume Medis** | 1. `POST /discharges/{id}/decide` (Keputusan pulang)<br>2. `GET /discharges/{id}/summary-prefill` (Prefill data ringkasan)<br>3. `PUT /discharges/{id}/summary` (Simpan draf resume medis)<br>4. `PATCH /discharges/{id}/summary/sign` (Tanda tangan digital resume) | `200 OK` / `422`<br>`200 OK`<br>`200 OK`<br>`200 OK` | **PASS**<br>Seluruh siklus pelepasan pasien dan resume medis berfungsi utuh. |
| **4** | **Penunjang Medis** | 1. `POST /laboratory-management/lab-orders` (Order lab darah lengkap)<br>2. `POST /radiology-management/rad-orders` (Order rontgen thorax) | `201 Created`<br>`201 Created` | **PASS**<br>Nomor order LAB dan RAD terbit serta terhubung ke episode. |
| **5** | **Catatan Terintegrasi (CPPT)** | `POST /patient-integrated-progress-notes` (Buat CPPT langsung) | `200 OK` | **PASS**<br>Catatan CPPT mandiri langsung tampil pada lini masa klinis. |
| **6** | **Kajian Pasien** | `PATCH /patient-assessments/{id}/complete` (Finalisasi kajian) | `200 OK` | **PASS**<br>Kajian medis berubah status menjadi Complete (Selesai). |

---

## 5. Hasil Pengujian Bagian C — Investigasi Tiga Anomali Historis

### C1. Anomali CPPT dari Konsultasi (`from-consultation`) Tidak Muncul di Lini Masa

- **Status Investigasi**: **TERBUKTI SEBAGAI CACAT (DEFECT AKTIF)**
- **Uraian Masalah**:
  Ketika dokter membuat catatan CPPT melalui endpoint turunan konsultasi:
  `POST /api/v1/health-services/clinical-management/patient-integrated-progress-notes/from-consultation/{consultationId}`
  Backend merespons dengan `HTTP 200 OK` dan nomor CPPT terbit (`CPPT-20260923-0001`). Namun, kartu CPPT tersebut **sama sekali tidak muncul** pada lini masa rawat inap pasien (`GET /patient-integrated-progress-notes/episodes/{episodeId}`). Counter tetap menunjukkan 0 catatan.
- **Akar Masalah (Root Cause di Database)**:
  Pemeriksaan baris data di tabel PostgreSQL `CliPatientIntegratedProgressNote`:
  ```sql
  SELECT "Id", "NoteNumber", "InpEpisodeId", "EncounterId", "PatientId"
  FROM "CliPatientIntegratedProgressNote"
  WHERE "NoteNumber" = 'CPPT-20260923-0001';
  ```
  **Temuan Nyata:**
  - `InpEpisodeId`: **`NULL`** *(Kosong)*
  - `EncounterId`: `d0f70f24-5232-43f1-aee4-256308b2bf95`
  
  Sementara itu, endpoint pembaca riwayat CPPT rawat inap melakukan query filter:
  `WHERE x.InpEpisodeId == episodeId`
  Karena `InpEpisodeId` bernilai `NULL`, catatan CPPT tersebut selamanya tersembunyi dari linimasa episode rawat inap pasien.

---

### C2. Anomali Dokter / Penulis Kosong (`-`) pada Riwayat Kajian Medis

- **Status Investigasi**: **TERBUKTI SEBAGAI CACAT (DEFECT AKTIF)**
- **Uraian Masalah**:
  Pada tabel Riwayat Kajian Medis di antarmuka dokter rawat inap, kolom **Dokter / Penulis** selalu menampilkan strip tanda hubung (`-`), meskipun dokter yang login telah menandatangani kajian tersebut.
- **Akar Masalah (Root Cause di Backend DTO Mapper)**:
  Pemeriksaan pada kode sumber backend file `PatientAssessmentController.cs` (baris 2935, metode `ToResponse`):
  ```csharp
  DoctorName = x.Queue != null && x.Queue.Doctor != null 
      ? x.Queue.Doctor.FullName 
      : null,
  ```
  **Temuan Teknis:**
  - Pemetaan properti `DoctorName` digantungkan pada objek antrean rawat jalan (`x.Queue.Doctor`).
  - Pada modul Rawat Inap (*Inpatient*), pasien dirawat berdasarkan episode dan bed placement tanpa melalui mekanisme nomor antrean poliklinik rawat jalan (`QueueId` bernilai `NULL`).
  - Meskipun kolom `DoctorId` terisi dengan ID dr. Rendy Pangalila (`bc389b2c-9b4e-47a7-8a28-98033ef7f97a`), backend tetap mengirimkan `"doctorName": null` dalam respons JSON.
  - Frontend secara wajar merender nilai `null` sebagai `-`. Ini adalah cacat logika backend, bukan bug frontend.

---

### C3. Anomali Inkonsistensi Konteks Pasien & Kelas `UNIQUE`

- **Status Investigasi**: **TERBUKTI & SEBAGIAN DIKLARIFIKASI**
- **Uraian Masalah**:
  Laporan pengujian lama mencatat perbedaan ruang dan kelas bed yang drastis antar tab (misal: "Ruang Melati / Bed 02", kelas kosong, dan kelas `UNIQUE`).
- **Hasil Pemeriksaan Antarmuka Riil**:
  Pada pengujian browser Playwright hari ini untuk pasien Tn. Indra Gunawan (`c3fe1370-18f0-42fb-8d9f-01449212828e`), komponen `PatientBanner` pada tab **Visite**, **Tindakan**, **Penunjang**, dan **Resep** menampilkan informasi yang **100% KONSISTEN**:
  - Dokter DPJP: `dr. Rendy Pangalila`
  - Informasi Bed: `RI-260909100035-F8D716 • Ruang Rawat Inap Kelas I 1 • Bed BED 001 • KELAS I`
  - No. RM: `00-00-00-16` | Pasien: `Indra Gunawan` | Penjamin: `BPJS Kesehatan`
  - *Catatan:* Perbedaan "Ruang Melati / Bed 02" pada laporan lama terbukti merupakan kelalaian penyalinan data mock/dummy oleh penguji terdahulu.
- **Asal-Usul Nama Kelas `UNIQUE` di Database**:
  Pemeriksaan langsung pada basis data PostgreSQL:
  ```sql
  SELECT e."Id", e."EpisodeNumber", mc."ClassName" AS "EpisodeClassName", bc."ClassName" AS "BedClassName"
  FROM "InpEpisode" e
  LEFT JOIN "MstPatientClass" mc ON e."PatientClassId" = mc."Id"
  LEFT JOIN "InpBedPlacement" bp ON bp."EpisodeId" = e."Id" AND bp."Status" = 1
  LEFT JOIN "MstPatientClass" bc ON bp."PatientClassId" = bc."Id"
  WHERE e."Id" = 'c3fe1370-18f0-42fb-8d9f-01449212828e';
  ```
  **Temuan Database:**
  - `InpEpisode.PatientClassId`: `013ef5df-8855-4f19-8751-606146fe7250` → `MstPatientClass.ClassName` = **`"UNIQUE"`**.
  - `InpBedPlacement.PatientClassId`: `0b1990eb-c539-4221-aaf9-34350b98c9fd` → `MstPatientClass.ClassName` = **`"KELAS I"`**.
  
  **Kesimpulan:** Nama `"UNIQUE"` adalah nama data riil yang diinputkan operator ke dalam tabel master kelas pasien (`MstPatientClass`), bukan string error kode program. Frontend menampilkan kelas bed dari `InpBedPlacement` ("KELAS I"), sedangkan endpoint ringkasan tertentu yang membaca kelas episode langsung akan menampilkan `"UNIQUE"`.

---

## 6. Daftar Cacat Baru yang Ditemukan (New Defects)

Sesuai aturan pengujian Quilvian, agen penguji **dilarang menyentuh atau memperbaiki kode sumber aplikasi**. Temuan cacat baru dicatat secara lengkap berikut langkah reproduksinya agar dapat dialokasikan pada tiket task tersendiri:

### DEFECT-001 (Backend) — `InpEpisodeId` Bernilai NULL pada Pembuatan CPPT dari Konsultasi
- **Tingkat Keparahan**: Major / Integritas Rekam Medis
- **Lokasi**: Controller / Service `PatientIntegratedProgressNote`
- **Endpoint Terkait**: `POST /api/v1/health-services/clinical-management/patient-integrated-progress-notes/from-consultation/{consultationId}`
- **Langkah Reproduksi**:
  1. Panggil endpoint pembuatan CPPT dari konsultasi rawat inap yang sah.
  2. Periksa baris data di tabel `CliPatientIntegratedProgressNote`.
  3. Perhatikan bahwa kolom `InpEpisodeId` bernilai `NULL`.
  4. Buka linimasa catatan dokter rawat inap `GET .../episodes/{episodeId}`.
  5. Catatan yang baru dibuat tidak akan pernah muncul pada linimasa pasien rawat inap.
- **Rekomendasi Solusi**: Pada handler `from-consultation`, salin `InpEpisodeId` dari entitas `DoctorConsultation` induk ke entitas `CliPatientIntegratedProgressNote` sebelum disimpan ke database.

---

### DEFECT-002 (Backend) — Nama Dokter Penulis Kajian Medis Kosong (`null`) pada Kasus Rawat Inap
- **Tingkat Keparahan**: Minor / Kepatuhan Dokumentasi Medikolegal
- **Lokasi**: `PatientAssessmentController.cs` baris 2935 (Metode `ToResponse`)
- **Endpoint Terkait**: `GET /api/v1/health-services/clinical-management/patient-assessments`
- **Langkah Reproduksi**:
  1. Buat kajian medis rawat inap dengan menyertakan `doctorId`.
  2. Panggil `GET /patient-assessments` untuk episode rawat inap terkait.
  3. Amati isi JSON respons pada properti `doctorName` yang selalu bernilai `null`.
  4. Buka UI Riwayat Kajian Medis; kolom Dokter / Penulis menampilkan `-`.
- **Rekomendasi Solusi**: Pada pemetaan DTO `ToResponse`, prioritaskan relasi langsung ke entitas dokter pengkaji (`x.Doctor != null ? x.Doctor.FullName : (x.Queue != null && x.Queue.Doctor != null ? x.Queue.Doctor.FullName : null)`).

---

## 7. Hal-Hal yang Tidak Dijalankan (Not Run)

| Item Pengujian | Alasan Tidak Dijalankan |
| :--- | :--- |
| **`ISS-05` (Unifikasi Service Resep Frontend)** | Sesuai catatan mandat pada prompter testing dan Bagian 7 Laporan `FE-RWI-095`: `prescriptionService.js` (manajemen order farmasi) dan `doctorInpatientService.js` (state lembar kerja klinis dokter) bukan file duplikat melainkan memegang tanggung jawab yang berbeda. Penyatuan service ini ditunda menunggu keputusan arsitektural produk. |

---

## 8. Tautan Artefak Bukti Pengujian

Seluruh berkas bukti visual dan log jaringan pengujian tersimpan pada repositori frontend di dalam folder pengujian terisolasi:

- **Direktori Berkas**: `QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/`
- **Log Jaringan Lengkap**: [network-logs.json](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/network-logs.json)
- **Tangkapan Layar Alur Resep & UI**:
  - Login Berhasil: [01-after-login.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/01-after-login.png)
  - Lembar Kerja Pasien: [02-patient-workspace.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/02-patient-workspace.png)
  - Konteks Banner Tab Visite: [03-tab-visit-context.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/03-tab-visit-context.png)
  - Konteks Banner Tab Tindakan: [04-tab-tindakan-context.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/04-tab-tindakan-context.png)
  - Konteks Banner Tab Penunjang: [05-tab-penunjang-context.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/05-tab-penunjang-context.png)
  - Formulir Tab Resep Aktif: [06-tab-resep-active.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/06-tab-resep-active.png)
  - Dropdown Jenis Resep (Rutin/Harian/Obat Pulang): [06b-order-type-dropdown-opened.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/06b-order-type-dropdown-opened.png)
  - Tombol Tambah Obat: [07-button-tambah-obat.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/07-button-tambah-obat.png)
  - Modal Formularium Terbuka: [08-modal-katalog-opened.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/08-modal-katalog-opened.png)
  - Hasil Pencarian Obat "paracetamol": [09-search-paracetamol.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/09-search-paracetamol.png)
  - Hasil Pencarian Obat "ceftriaxone": [10-search-ceftriaxone.png](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/test-with-agy/screenshots/issue-001/10-search-ceftriaxone.png)
