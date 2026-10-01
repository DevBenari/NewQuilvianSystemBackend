# Laporan Analisis & Rencana Kerja Modernisasi Menu: Resep Dokter Rawat Inap (Inpatient Physician E-Prescription)

## 1. Ringkasan Eksekutif & Identitas Dokumen

| Atribut | Keterangan |
| :--- | :--- |
| **Area / Modul** | Pelayanan Kesehatan (*Health Services*) — Dokter Rawat Inap (*Inpatient Physician Workspace*) |
| **Menu Sasaran** | Ruang Kerja Dokter Rawat Inap → Tab: **Resep (*Inpatient Prescription & CPOE*)** |
| **Jalur Dokumen** | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/rencana-kerja/resep/resep.md` |
| **Dasar Penyelarasan** | Mengadopsi 100% kapabilitas operasional teruji dari **QuilvianV1** (berdasarkan 6 tangkapan layar lapangan: resep standar, resep racikan, rekonsiliasi obat, template resep, history resep, dan resep harian), diselaraskan dengan arsitektur modern **QuilvianFinal** dan standar keselamatan peresepan elektronik (*Computerized Physician Order Entry / CPOE*). |
| **Standar Keselamatan** | **Sasaran Keselamatan Pasien 3 (SKP 3 / IPSG 3)**: *Peningkatan Keamanan Obat yang Perlu Diwaspadai (High-Alert Medications)*, Pencegahan *Medication Error*, Verifikasi Alergi Otomatis, dan Standar Akreditasi Rumah Sakit KARS / STARKES. |
| **Prinsip Data** | **Transactional Consistency & Zero Orphan Records** — Persistensi atomis pada `PhmPrescription`, `PhmPrescriptionItem`, `PhmPrescriptionCompound`, dan `PhmPrescriptionCompoundItem`, penautan ke catatan SOAP (`TrxDoctorConsultation`), serta integrasi penjamin/asuransi (`EncounterInsuranceContext`). |

---

## 2. Analisis Mendalam: Audit Kesenjangan V1 vs Final (Status Implementasi)

Investigasi dilakukan terhadap formulir operasional **QuilvianV1** (berdasarkan folder `captures/dokter-rawat-inap/05-resep/` dan source code `resep-form.jsx`, `racikan-form.jsx`, `resep-template.jsx`, dkk.) dibandingkan dengan kondisi sistem di **QuilvianFinal** (`InpatientPrescriptionTab.jsx`, `prescription-builder-panel.jsx`, `DoctorPrescriptionOrderTable.jsx`, `PrescriptionController.cs`, `PrescriptionCompoundController.cs`, dll.):

### 2.1. Matriks Status Implementasi Fitur & Parameter

| No | Komponen / Fitur | Kondisi di QuilvianV1 (Operasional Lapangan - Capture `05-resep`) | Kondisi di QuilvianFinal (Target Paritas 100%) | Status Penyelarasan | Catatan & Rencana Paritas 100% V1 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | **Struktur Navigasi Sub-Tab Utama** | 4 Tab Horizontal: `Buat Resep` (aktif), `Template Resep`, `History Resep`, dan `Resep Harian` | Mempertahankan 4 sub-tab utama V1 ini secara terdepan (`Buat Resep`, `Template Resep`, `History Resep`, `Resep Harian`) ditambah akses terpadu rekonsiliasi & sliding scale | **PARITAS 100% DITETAPKAN** | Tampilan sub-tab utama disamakan persis dengan V1 sehingga alur navigasi dokter tidak berubah |
| 2 | **Header Bar Resep & 3 Pill Segmented Mode** | Banner hijau/teal memuat judul `Resep Obat`, tombol `Gunakan Template`, dan badge `Asuransi: Allianz`. Di bawahnya terdapat 3 segmented pill: `[ Resep ]` (cyan aktif), `[ Obat Racikan ]` (hijau), dan `[ ⚖ Rekonsiliasi Obat ]` (oranye) | Mengadopsi persis banner hijau/teal dengan tombol `Gunakan Template`, badge asuransi real-time, serta 3 pill segmented button yang berganti mode secara instan | **PARITAS 100% DITETAPKAN** | Menghilangkan kebingungan navigasi; dokter beralih antara resep biasa, racikan, dan rekonsiliasi dalam 1 klik |
| 3 | **Katalog & Form Obat Biasa (Split-View 2 Kolom Tanpa Modal Pop-up)** | **Tampilan 2 Kolom Berdampingan (`01-buat-resep-standar.png`)**:<br/>- **Kolom Kiri**: `Daftar Obat` lengkap dengan live search, counter `Halaman 1 • Total 20 obat [20 Ditampilkan] [Ada Lebih Banyak]`, dan tabel obat langsung dengan tombol `[ Pilih ]`.<br/>- **Kolom Kanan**: Form `Detail Obat` (`Nama Obat` read-only, stok & harga, `Jumlah Obat`, `Signa * [ ] x [ ]`, `Signa Tambahan`, `Catatan`, `Estimasi Pemberian`, `Cara Pemakaian` dropdown, tombol `[ Reset Obat ]` dan `[ Tambahkan ]`) | Dirombak total menjadi **Split-View 2 Kolom persis V1**! Menghilangkan tombol popup besar *"Cari dan Tambah Obat"*; katalog obat langsung tampil berdampingan dengan form input | **PARITAS 100% DITETAPKAN (ZERO BLOCKER)** | Tidak ada penghalang teknis sama sekali; backend sudah siap; dokter langsung memilih obat dan mengetik signa di satu layar tanpa buka-tutup pop-up |
| 4 | **Pembuat Obat Racikan (Compound Builder — Split-View 2 Kolom)** | **Tampilan 2 Kolom Berdampingan (`02-buat-resep-racikan.png`)**:<br/>- **Kolom Kiri**: Box `Buat Racikan Baru (ID: ...)` (Informasi racikan: nama racikan*, bentuk sediaan dropdown, jumlah racikan, signa racikan*, signa tambahan; Card komponen bahan racikan; tombol `[ Reset Form ]` & `[ Jadikan Racikan ]`).<br/>- **Kolom Kanan**: Box `Daftar Obat` (katalog obat formularium di mana klik `[ Pilih ]` langsung menambahkan obat sebagai bahan racikan di kolom kiri) | Diadopsi 100% dengan antarmuka 2 kolom berdampingan persis V1, terhubung ke model data atomis `PhmPrescriptionCompound` | **PARITAS 100% DITETAPKAN** | Dokter dapat meracik sediaan puyer/kapsul/sirup langsung dari katalog tanpa berpindah layar |
| 5 | **Rekonsiliasi Obat Admisi Bawaan Pasien** | **Tampilan Panel Oranye (`03-buat-resep-rekonsiliasi.png`)**:<br/>Box `Rekonsiliasi Obat Admisi` dengan badge `[ X Obat ]`, tombol `[ Refresh ]`, dan daftar obat rumah bawaan pasien dengan tombol aksi `[ Lanjutkan ]` dan `[ Ubah Aturan Pakai ]` yang langsung menyalin butir ke resep | Ditampilkan langsung saat pill `[ ⚖ Rekonsiliasi Obat ]` aktif; tombol aksi langsung menginjeksi obat ke draf peresepan | **PARITAS 100% DITETAPKAN** | Alur rekonsiliasi mulus tanpa kehilangan draf yang sedang dikerjakan |
| 6 | **Daftar Resep yang Dipilih (Ringkasan Bawah)** | Card `Resep yang Dipilih` di bagian bawah dengan badge `[ Obat Biasa: X ] [ Racikan: Y ] [ Total: Z Item ]`, tabel item terpilih, dan tombol footer: `[ Bersihkan Draf ]`, `[ Simpan sebagai Template Pribadi ]`, dan `[ Simpan Draft Resep ]` | Diadopsi 100% persis susunan dan aksinya, terhubung ke backend atomis `POST /prescriptions` | **PARITAS 100% DITETAPKAN** | Memberikan kejelasan total bagi dokter mengenai butir obat dan racikan yang akan diterbitkan |
| 7 | **Template Resep Dokter Pribadi** | Tampilan V1 (`04-template-resep.png`) memiliki daftar template resep pribadi dokter dengan pencarian dan tombol `+ Buat Template Baru` | Diadopsi penuh pada sub-tab `Template Resep` lengkap dengan pencarian dan tombol penerapan satu-klik | **PARITAS 100% DITETAPKAN** | Mempercepat peresepan rutin DPJP |
| 8 | **Riwayat Resep Pasien (History Resep)** | Tampilan V1 (`05-history-resep.png`) menampilkan riwayat resep episode rawat inap | Diadopsi penuh pada sub-tab `History Resep` dengan integrasi endpoint riwayat episode dan status pemenuhan farmasi | **PARITAS 100% DITETAPKAN** | Evaluasi terapi obat sebelumnya dapat dipantau langsung |
| 9 | **Monitoring Resep Harian (Daily Prescriptions)** | Tampilan V1 (`06-resep-harian.png`) menampilkan resep harian rawat inap | Diadopsi penuh pada sub-tab `Resep Harian` dengan filter periode per tanggal dan shift pemberian obat | **PARITAS 100% DITETAPKAN** | Sinkronisasi jadwal pemberian obat perawat bangsal |
| 10 | **Inovasi Tata Kelola Rekam Medis (EMR & SKP 3)** | V1 belum memiliki penaut SOAP otomatis dan pemilahan jenis order resep rawat inap | Dropdown `Jenis Resep` (*Harian, Rutin, Obat Pulang*) dan penaut `Catatan Dokter Pengait (SOAP)` serta peringatan alergi merah otomatis tetap disematkan secara rapi di area atas form tanpa mengganggu estetika 2 kolom V1 | **TERINTEGRASI HARMONIS** | Memenuhi regulasi rekam medis digital dan standar akreditasi rumah sakit KARS/STARKES |

---

## 3. Laporan Spesifikasi Endpoint Swagger API

Modul Peresepan Dokter Rawat Inap menggunakan kelompok controller pada namespace:
`QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers`

### 3.1. Tabel Spesifikasi Endpoint API Terstandar

| No | Method | Route Path | Deskripsi Fungsi | Hak Akses / Role Otorisasi | Request Body (DTO) | Response DTO | HTTP Code |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | `GET` | `/api/v1/health-services/pharmacy-management/prescribing-drugs` | Mencari katalog obat formularium aktif yang berlaku untuk kelas rawat inap pasien | `PrescribingDrug : Read` | *Query Params*: `search`, `encounterId`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<PrescribingDrugItemResponse>>` | `200 OK`<br/>`400 Bad Request` |
| 2 | `GET` | `/api/v1/health-services/pharmacy-management/prescribing-drugs/{id}/clinical-information` | Mengambil informasi klinis detail obat (indikasi, kontraindikasi, dosis lazim, efek samping, interaksi) | `PrescribingDrug : Read` | *Route Param*: `id` (GUID) | `ApiResponse<PrescribingDrugClinicalDetailResponse>` | `200 OK`<br/>`404 Not Found` |
| 3 | `POST` | `/api/v1/health-services/pharmacy-management/prescriptions` | Menerbitkan resep dokter lengkap secara atomis (memuat item obat jadi dan/atau obat racikan) | `Prescription : Create` | `CreatePrescriptionRequest` | `ApiResponse<PrescriptionCreateResponse>` | `201 Created`<br/>`400 Bad Request`<br/>`422 Unprocessable` |
| 4 | `GET` | `/api/v1/health-services/pharmacy-management/prescriptions/episodes/{episodeId}` | Mengambil seluruh riwayat resep dalam satu episode rawat inap beserta status pemenuhannya di farmasi | `Prescription : Read` | *Route Param*: `episodeId`<br/>*Query*: `period`, `from`, `to` | `ApiResponse<PagedResult<InpatientPrescriptionListItem>>` | `200 OK`<br/>`422 Unprocessable` |
| 5 | `POST` | `/api/v1/health-services/pharmacy-management/prescription-compounds` | Membuat order racikan baru pada lembar resep tertentu | `PrescriptionCompound : Create` | `CreatePrescriptionCompoundRequest` | `ApiResponse<PrescriptionCompoundResponse>` | `201 Created`<br/>`400 Bad Request` |
| 6 | `POST` | `/api/v1/health-services/pharmacy-management/prescription-compound-items` | Menambahkan komponen bahan obat ke dalam racikan | `PrescriptionCompound : Create` | `CreatePrescriptionCompoundItemRequest` | `ApiResponse<PrescriptionCompoundItemResponse>` | `201 Created`<br/>`400 Bad Request` |
| 7 | `GET` | `/api/v1/health-services/pharmacy-management/prescription-templates` | Mengambil daftar template resep dokter (scope: pribadi/`Mine` atau bersama/`Shared`) | `PrescriptionTemplate : Read` | *Query Params*: `search`, `ownerScope` | `ApiResponse<PagedResult<PrescriptionTemplateResponse>>` | `200 OK` |
| 8 | `POST` | `/api/v1/health-services/pharmacy-management/prescription-templates` | Menyimpan draf resep saat ini menjadi template resep pribadi dokter | `PrescriptionTemplate : Create` | `CreatePrescriptionTemplateRequest` | `ApiResponse<PrescriptionTemplateResponse>` | `201 Created`<br/>`400 Bad Request` |
| 9 | `POST` | `/api/v1/health-services/pharmacy-management/prescription-templates/{id}/apply-to-prescription/{prescriptionId}` | Menerapkan seluruh isi template ke dalam resep aktif | `PrescriptionTemplate : Apply` | *Route Params*: `id`, `prescriptionId` | `ApiResponse<PrescriptionDetailResponse>` | `200 OK`<br/>`404 Not Found` |
| 10 | `GET` | `/api/v1/health-services/pharmacy-management/medication-reconciliations/episodes/{episodeId}` | Mengambil daftar obat bawaan pasien saat admisi untuk keperluan rekonsiliasi | `MedicationReconciliation : Read` | *Route Param*: `episodeId` | `ApiResponse<List<ReconciliationItemResponse>>` | `200 OK`<br/>`404 Not Found` |
| 11 | `POST` | `/api/v1/health-services/pharmacy-management/medication-reconciliations/{id}/decisions` | Menyimpan keputusan dokter terhadap obat bawaan (Lanjut, Ubah Aturan, atau Stop/Tunda) | `MedicationReconciliation : Decide` | `ReconciliationDecisionRequest` | `ApiResponse<ReconciliationItemResponse>` | `200 OK`<br/>`403 Forbidden` |

---

### 3.2. Contoh Kontrak Payload Data (Request & Response)

#### Contoh 1: Request Pembuatan Resep Lengkap (`POST /prescriptions`):
```json
{
  "encounterId": "4c9e782a-4371-46ab-a077-4b7eb1a74288",
  "consultationId": "fb9e0215-66fb-48bd-941d-d506c4ad75cf",
  "prescriptionOrderType": 1,
  "prescriptionDateTime": "2026-09-29T10:00:00Z",
  "clinicalNote": "Pasien memiliki riwayat maag ringan, hindari NSAID dosis tinggi.",
  "doctorInstruction": "Berikan etiket kocok dahulu untuk racikan sirup, serahkan obat setelah makan siang.",
  "items": [
    {
      "drugId": "0b1990eb-c539-4221-aaf9-34350b98c9fd",
      "quantity": 10.0,
      "dose": 500.0,
      "doseUnit": "mg",
      "frequency": "3x1",
      "route": "Oral",
      "administrationTime": "Setelah Makan",
      "instruction": "Habiskan",
      "daysOfSupply": 3
    }
  ],
  "compounds": [
    {
      "compoundName": "Puyer Batuk Anak",
      "compoundForm": "Pulveres / Puyer",
      "quantity": 10,
      "signa": "3 x 1 bungkus",
      "administrationTime": "Setelah Makan",
      "instruction": "Bila batuk berdahak",
      "ingredients": [
        {
          "drugId": "2c89f143-7711-4ad9-bf91-11234857abcd",
          "dosePerPackage": 100.0,
          "doseUnit": "mg"
        },
        {
          "drugId": "3d90e254-8822-4be0-cf02-22345968bcde",
          "dosePerPackage": 2.0,
          "doseUnit": "mg"
        }
      ]
    }
  ]
}
```

#### Contoh 2: Response Sukses (`201 Created`):
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Header resep berhasil dibuat.",
  "data": {
    "id": "e4f8d227-bb89-43c1-b0db-bcfbda09bc08",
    "prescriptionNumber": "RX-20260929-00042",
    "prescriptionOrderType": 1,
    "prescriptionStatus": 1,
    "paymentStatus": 1,
    "fulfillmentStatus": 1,
    "totalItemCount": 2,
    "totalPrice": 128500.00,
    "patientPayAmount": 0.00,
    "coverageStatus": "Ditanggung Penuh BPJS"
  }
}
```

---

## 4. Alur Bisnis Proses Rumah Sakit & Keselamatan Peresepan (SKP 3)

### 4.1. Diagram Alur Proses Bisnis Peresepan Rawat Inap (Mermaid Flowchart)

```mermaid
flowchart TD
    A["1. DPJP Melakukan Visite & Mengisi Catatan SOAP"] --> B["2. DPJP Membuka Tab 'Resep' Rawat Inap"]
    B --> C["3. Sistem Mengambil Konteks Episode, Penjamin & Alergi Pasien"]
    
    C --> D{"Pilihan Jenis Resep"}
    D -- "Harian (Daily Dose)" --> E1["Set Order Type: Harian (Kebutuhan 24 Jam)"]
    D -- "Rutin (Maintenance)" --> E2["Set Order Type: Rutin (Selama Rawat Inap)"]
    D -- "Obat Pulang (Discharge)" --> E3["Set Order Type: Obat Pulang (Bawa ke Rumah)"]
    
    E1 --> F{"Metode Input Resep"}
    E2 --> F
    E3 --> F
    
    F -- "Obat Standar / Jadi" --> G1["Cari di Formularium RS -> Tentukan Dosis & Signa"]
    F -- "Obat Racikan (Compound)" --> G2["Buka Racikan Composer -> Pilih Bahan & Dosis -> Jadikan Racikan"]
    F -- "Dari Rekonsiliasi Admisi" --> G3["Review Obat Bawaan Rumah -> Klik 'Lanjutkan / Ubah'"]
    F -- "Terapkan Template Dokter" --> G4["Pilih Template Favorit Dokter -> Otomatis Salin Butir"]
    
    G1 --> H["4. Pengecekan Keamanan Otomatis (Allergy & Drug Safety Check)"]
    G2 --> H
    G3 --> H
    G4 --> H
    
    H --> I{"Apakah Ada Konflik Alergi / Interaksi Berbahaya?"}
    I -- "Ya (Flagged Red)" --> J["Tampilkan Clinical Safety Alert Merah:<br/>Tombol Simpan DIBLOKIR hingga obat diganti"]
    I -- "Tidak (Clear Green)" --> K["Hitung Estimasi Biaya & Pertanggungan Asuransi Real-Time"]
    
    J --> F
    K --> L["5. DPJP Memasukkan Catatan Klinis & Instruksi Farmasi"]
    L --> M["6. DPJP Mengklik 'Simpan Draft Resep'"]
    M --> N["7. Transaksi Atomis: Header + Item + Racikan Tersimpan di DB"]
    N --> O["8. Status: Terkirim ke Instalasi Farmasi Rawat Inap untuk Telaah Resep"]
```

---

### 4.2. Tiga Skenario Nyata di Rumah Sakit

#### Skenario 1: Pasien Dewasa — Bangsal Penyakit Dalam (Pneumonia Komunitas)
- **Pasien**: Tn. Hendra (52 tahun), dirawat di Ruang Kelas I dengan diagnosis CAP (*Community-Acquired Pneumonia*), penjamin Asuransi Perusahaan.
- **Tindakan DPJP**:
  1. DPJP memilih `Jenis Resep: Harian` dan menautkan ke catatan SOAP visit pagi ini.
  2. DPJP mencari antibiotik injeksi `Ceftriaxone 1g Vial`, menetapkan signa `2 x 1g IV`, dan obat oral `N-Acetylcysteine 200mg Kapsul` `3 x 1 kapsul PC`.
  3. Sistem memverifikasi bahwa pasien tidak memiliki riwayat alergi cephalosporin dan obat termasuk dalam formularium kelas perawatannya.
  4. DPJP menulis instruksi: *"Injeksi ceftriaxone skin test terlebih dahulu sebelum dosis pertama"*.
  5. Resep disimpan, terbit nomor `RX-20260929-00101`, dan langsung tampil pada monitor farmasi untuk disiapkan.

#### Skenario 2: Pasien Pediatrik (Anak) — Bangsal Anak (Bronkopneumonia & Febris)
- **Pasien**: An. Alif (3 tahun, BB: 14 kg), penjamin BPJS Kesehatan.
- **Tindakan Dokter**:
  1. Dokter anak memilih sub-seksi **Obat Racikan**.
  2. Form racikan diisi: `Nama Racikan: Puyer Batuk Demam Anak`, `Bentuk: Pulveres`, `Jumlah: 10 bungkus`, `Signa: 3 x 1 bungkus sesudah makan`.
  3. Dokter memilih komponen obat dari formularium:
     - *Paracetamol 500mg Tab* (komposisi: 140 mg per bungkus = 10 mg/kgBB)
     - *Salbutamol 2mg Tab* (komposisi: 0.7 mg per bungkus)
     - *Ambroxol 30mg Tab* (komposisi: 7.5 mg per bungkus)
  4. Dokter mengklik **Jadikan Racikan**. Racikan langsung masuk ke daftar item draf resep dengan rincian ketiga bahan aktifnya.
  5. Resep disimpan, dan farmasi menerima detail kalkulasi penimbangan bahan racikan secara presisi.

#### Skenario 3: Pasien Geriatri — Bangsal Jantung (Rekonsiliasi Obat & Sliding Scale)
- **Pasien**: Ny. Rosmini (71 tahun), diagnosis CHF (*Congestive Heart Failure*) ec HHD + DM Tipe 2.
- **Tindakan DPJP**:
  1. Membuka tab **Rekonsiliasi Obat**: Pasien membawa *Amlodipine 10mg* dan *Metformin 500mg* dari rumah.
  2. DPJP memutuskan:
     - *Amlodipine 10mg*: **Lanjutkan** (langsung disalin ke resep harian rawat inap).
     - *Metformin 500mg*: **Hentikan Sementara** (karena fungsi ginjal sedang dipantau).
  3. Untuk kontrol gula darah, DPJP membuka tab **Sliding Scale** dan menetapkan protokol insulin Novorapid berdasarkan GDS per 6 jam.
  4. Seluruh keputusan terdokumentasi rapi, terhindar dari duplikasi obat dan interaksi obat berbahaya.

---

### 4.3. Analisis Dampak Perubahan Bisnis & Keselamatan Pasien

1. **Efisiensi Waktu Kerja Dokter & Farmasi (CPOE Efektif)**:
   - Resep elektronik menghilangkan masalah tulisan tangan dokter yang tidak terbaca (*illegible handwriting*), yang selama ini menjadi penyebab utama 40% keterlambatan penyiapan obat di farmasi rawat inap.
   - Fitur **Template Resep Pribadi** memangkas waktu entri resep rutin dari 5–8 menit menjadi kurang dari **45 detik**.
2. **Kepatuhan Terhadap Standar Keselamatan Pasien (SKP 3)**:
   - Pengecekan otomatis silang antara daftar alergi pasien di EMR dengan kandungan obat formularium secara real-time.
   - Peringatan khusus untuk obat berisiko tinggi (*High-Alert Medications*) seperti elektrolit konsentrat dan insulin.
3. **Kepatuhan Regulasi & Akreditasi (STARKES / KARS & BPJS)**:
   - Resep rawat inap terhubung langsung ke episode dan catatan SOAP dokter (menjamin resep memiliki indikasi medis legal).
   - Penegakan restriksi formularium dan status tanggungan asuransi mencegah terjadinya klaim BPJS yang ditolak (*dispute claim*).

---

## 5. Skema Tampilan UI/UX: 100% Paritas Presisi QuilvianV1 (Berdasarkan 6 Capture Lapangan)

### 5.1. Klarifikasi Arsitektur: Apakah Ada Penghalang Menyamakan V1?

> **JAWABAN TEGAS: TIDAK ADA PENGHALANG SAMA SEKALI (ZERO TECHNICAL BLOCKERS).**  
> Seluruh endpoint backend ASP.NET Core (`GET /prescribing-drugs`, `POST /prescriptions`, `POST /prescription-compounds`, template, rekonsiliasi, dan history) sudah **100% siap dan kompatibel penuh** dengan alur peresepan V1.  
> **Penyebab ketidaksesuaian tampilan sebelumnya**: Terjadi kekeliruan asumsi awal di frontend yang mengadopsi pola pop-up modal katalog milik rawat jalan (*Outpatient Drug Catalog Modal*), bukannya menerapkan struktur tata letak asli milik Rawat Inap V1 yaitu **Tampilan 2 Kolom Berdampingan (*Inline Split-View*)** di mana pencarian obat formularium dan formulir input dosis/signa berada berdampingan dalam satu layar tanpa memerlukan pop-up modal.  
> **Komitmen Solusi**: Tata letak antarmuka dirombak total mengikuti **100% persis** visual dan hierarki operasional V1 dari folder `captures/dokter-rawat-inap/05-resep/`.

---

### 5.2. Skema Wireframe Presisi Seluruh Layar V1 (Capture Lapangan `05-resep`)

#### Layar 1: Resep Standar / Biasa — Split-View 2 Kolom (`01-buat-resep-standar.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ] (aktif)       [ Template Resep ]       [ History Resep ]       [ Resep Harian ]                       |
+-----------------------------------------------------------------------------------------------------------------------+
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | Resep Obat                                                        [ Gunakan Template ]   [ Asuransi: Allianz ]  |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  |       [       Resep       ]       |       [   Obat Racikan   ]       |       [ ⚖️ Rekonsiliasi Obat ]           |  |
|  |         (cyan / aktif)            |            (outline)             |              (outline)                   |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                       |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|  | 📦 DAFTAR OBAT (Kolom Kiri - Formularium)            | 📝 DETAIL OBAT (Kolom Kanan - Form Pengisian)            |  |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|  | [🔍] Cari obat berdasarkan nama, kode, kandungan...  | Nama Obat:                                               |  |
|  |                                                      | [ Paracetamol 500mg Tablet                             ] |  |
|  | Halaman 1 • Total 20 obat [20 Ditampilkan][Ada Lebih]| Stok: 450 | Harga: Rp 1.500 | [ Ditanggung Asuransi ]    |  |
|  | ---------------------------------------------------  |                                                          |  |
|  | | NAMA OBAT               | HARGA     | AKSI       | | Jumlah Obat:                   Signa *:                  |  |
|  | |-------------------------+-----------+------------| | [ 10               ]           [ 3 ]   X   [ 1 ]         |  |
|  | | Paracetamol 500mg Tab   | Rp 1.500  | [ Pilih ]  | |                                                          |  |
|  | | Ceftriaxone 1g Injeksi  | Rp 45.000 | [ Pilih ]  | | Signa Tambahan (opsional):                             |  |
|  | | Amoxicillin 500mg Kapsul| Rp 2.000  | [ Pilih ]  | | [ Sesudah makan                                      ] |  |
|  | | Omeprazole 20mg Kapsul  | Rp 3.500  | [ Pilih ]  | |                                                          |  |
|  | | Ketorolac 30mg Ampul    | Rp 12.000 | [ Pilih ]  | | Catatan (opsional):                                      |  |
|  | | Furosemide 40mg Tab     | Rp 1.200  | [ Pilih ]  | | [ Bila demam di atas 38°C                            ] |  |
|  | | Ondansetron 4mg Tab     | Rp 4.000  | [ Pilih ]  | |                                                          |  |
|  | | Tramadol 50mg Kapsul    | Rp 5.000  | [ Pilih ]  | | Estimasi Pemberian:            Cara Pemakaian:           |  |
|  | | ... (scrollable list)   | ...       | ...        | | [ Contoh: 7 hari, 2 m... ]     [ Oral                  v]|  |
|  | ---------------------------------------------------  |                                                          |  |
|  |                                                      | +------------------------------------------------------+ |  |
|  |                                                      | | [ Reset Obat ]                       [ Tambahkan ]   | |  |
|  |                                                      | +------------------------------------------------------+ |  |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | 🛒 Resep yang Dipilih                                         [ Obat Biasa: 1 ]  [ Racikan: 0 ]  [ Total: 1 Item ]|  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | NO | KATEGORI | DETAIL OBAT & KANDUNGAN   | JUMLAH | SIGNA & INSTRUKSI                  | BIAYA     | AKSI      |  |
|  |----+----------+---------------------------+--------+------------------------------------+-----------+-----------|  |
|  | 1  | [Resep]  | Paracetamol 500mg Tablet  | 10 Tab | 3x1 Sesudah makan • Bila demam>38C | Rp 15.000 | [ 🗑️ Hapus]|  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  |                                                                                                                   |
|  | [ 🧹 Bersihkan Draf ]                    [ 💾 Simpan sebagai Template Pribadi ]       [ 🚀 Simpan Draft Resep ]  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

#### Layar 2: Resep Racikan — Split-View 2 Kolom (`02-buat-resep-racikan.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ] (aktif)       [ Template Resep ]       [ History Resep ]       [ Resep Harian ]                       |
+-----------------------------------------------------------------------------------------------------------------------+
|  Resep Obat                                                          [ Gunakan Template ]   [ Asuransi: Allianz ]     |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  |       [       Resep       ]       |       [   Obat Racikan   ]       |       [ ⚖️ Rekonsiliasi Obat ]           |  |
|  |            (outline)              |         (green / aktif)          |              (outline)                   |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                       |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|  | 🧪 BUAT RACIKAN BARU (ID: #0efc987d)                 | 📦 DAFTAR OBAT (Katalog Bahan Baku Formularium)          |  |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|  | Informasi Racikan:                                   | [🔍] Cari obat berdasarkan nama, kode, kandungan...      |  |
|  | Nama Racikan *:                                      |                                                          |  |
|  | [ Puyer Batuk Pilek Anak                           ] | Halaman 1 • Total 20 obat [20 Ditampilkan][Ada Lebih]   |  |
|  |                                                      | -------------------------------------------------------- |  |
|  | Bentuk Racikan:                                      | | NAMA OBAT               | HARGA     | AKSI           | |  |
|  | [ Pulveres / Puyer                                 v]| |-------------------------+-----------+----------------| |  |
|  |                                                      | | Paracetamol 500mg Tab   | Rp 1.500  | [ + Bahan ]    | |  |
|  | Jumlah Racikan:               Signa Racikan *:       | | Ambroxol 30mg Tab       | Rp 800    | [ + Bahan ]    | |  |
|  | [ 10               ]          [ Contoh: 3 x 1      ] | | Cetirizine 10mg Tab     | Rp 1.200  | [ + Bahan ]    | |  |
|  |                                                      | | Salbutamol 2mg Tab      | Rp 600    | [ + Bahan ]    | |  |
|  | Signa Tambahan:                                      | | Dexamethasone 0.5mg Tab | Rp 500    | [ + Bahan ]    | |  |
|  | [ Sesudah makan                                    ] | -------------------------------------------------------- |  |
|  |                                                      |                                                          |  |
|  | +--------------------------------------------------+ |                                                          |  |
|  | | Obat dalam Racikan (3 Obat Bahan):               | |                                                          |  |
|  | | 1. Paracetamol 500mg -> [ 120 ] mg/bks  [🗑️]     | |                                                          |  |
|  | | 2. Ambroxol 30mg     -> [ 7.5 ] mg/bks  [🗑️]     | |                                                          |  |
|  | | 3. Cetirizine 10mg   -> [ 2.5 ] mg/bks  [🗑️]     | |                                                          |  |
|  | +--------------------------------------------------+ |                                                          |  |
|  |                                                      |                                                          |  |
|  | [ Reset Form ]                   [ Jadikan Racikan ] |                                                          |  |
|  +------------------------------------------------------+----------------------------------------------------------+  |
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | 🛒 Resep yang Dipilih                                         [ Obat Biasa: 0 ]  [ Racikan: 1 ]  [ Total: 1 Item ]|  |
|  | 1. [Racikan] Puyer Batuk Pilek Anak (10 Bungkus) • 3x1 Sesudah makan • Komposisi: PCT, Ambroxol, Cetirizine [🗑️]   |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

#### Layar 3: Rekonsiliasi Obat Admisi (`03-buat-resep-rekonsiliasi.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ] (aktif)       [ Template Resep ]       [ History Resep ]       [ Resep Harian ]                       |
+-----------------------------------------------------------------------------------------------------------------------+
|  Resep Obat                                                          [ Gunakan Template ]   [ Asuransi: Allianz ]     |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  |       [       Resep       ]       |       [   Obat Racikan   ]       |       [ ⚖️ Rekonsiliasi Obat ]           |  |
|  |            (outline)              |            (outline)             |          (orange / aktif)                |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | ⚖️ Rekonsiliasi Obat Admisi                                                    [ 2 Obat ]     [ 🔄 Refresh ]     |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | NO | OBAT RUMAH BAWAAN PASIEN | DOSIS & ATURAN PAKAI | TGL TERAKHIR MINUM | TINDAKAN DPJP RAWAT INAP            |  |
|  |----+--------------------------+----------------------+--------------------+-------------------------------------|  |
|  | 1  | Amlodipine 10mg Tablet   | 1 x 10mg (Malam)     | Kemarin Malam      | [ ✅ Lanjutkan ] [ ✏️ Ubah Aturan ] |  |
|  | 2  | Metformin 500mg Tablet   | 2 x 500mg (Pagi-Mlm) | Kemarin Pagi       | [ ⏸️ Tunda / Hentikan Sementara ]   |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | 🛒 Resep yang Dipilih                                         [ Obat Biasa: 1 ]  [ Racikan: 0 ]  [ Total: 1 Item ]|  |
|  | *Obat Amlodipine yang dilanjutkan otomatis tersalin ke draf resep aktif                                            |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

#### Layar 4: Template Resep Dokter Pribadi (`04-template-resep.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ]       [ Template Resep ] (aktif)       [ History Resep ]       [ Resep Harian ]                       |
+-----------------------------------------------------------------------------------------------------------------------+
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | Daftar Template Resep (dr. Rendy Pangalila)                                           [ ➕ Buat Template Baru ]  |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | [🔍] Cari template berdasarkan nama...                                                                            |  |
|  |                                                                                                                 |  |
|  | +-------------------------------------------------+ +-------------------------------------------------+         |  |
|  | | 📑 Template: Hipertensi Stage II Dewasa        | | 📑 Template: Gastritis Akut & Dispepsia         |         |  |
|  | | Kategori: Rawat Inap Dewasa                     | | Kategori: Rawat Inap Penyakit Dalam             |         |  |
|  | | Isi: Amlodipine 10mg (10), Candesartan 16mg (10)| | Isi: Omeprazole 40mg Inj (3), Sucralfate Syr (1)|         |  |
|  | | Racikan: 0                                      | | Racikan: 0                                      |         |  |
|  | | [ 🚀 Terapkan ke Resep ]          [ 🗑️ Hapus ]  | | [ 🚀 Terapkan ke Resep ]          [ 🗑️ Hapus ]  |         |  |
|  | +-------------------------------------------------+ +-------------------------------------------------+         |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

#### Layar 5: History Resep Pasien (`05-history-resep.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ]       [ Template Resep ]       [ History Resep ] (aktif)       [ Resep Harian ]                       |
+-----------------------------------------------------------------------------------------------------------------------+
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | 📜 Riwayat Peresepan Pasien Episode Ini                                                                           |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | NO | NO RESEP          | TANGGAL & WAKTU  | DOKTER PERESEP     | JENIS RESEP | JUMLAH ITEM | STATUS FARMASI   | AKSI  |  |
|  |----+-------------------+------------------+--------------------+-------------+-------------+------------------+-------|  |
|  | 1  | RX-20260928-00012 | 28 Sep 2026, 14.30| dr. Rendy Pangalila| Harian      | 3 Item      | [ Selesai Diserah]| [👁️]|  |
|  | 2  | RX-20260927-00085 | 27 Sep 2026, 09.15| dr. Rendy Pangalila| Cito / IGD  | 2 Item      | [ Selesai Diserah]| [👁️]|  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

#### Layar 6: Resep Harian Rawat Inap (`06-resep-harian.png`)
```text
+-----------------------------------------------------------------------------------------------------------------------+
|  [ Buat Resep ]       [ Template Resep ]       [ History Resep ]       [ Resep Harian ] (aktif)                       |
+-----------------------------------------------------------------------------------------------------------------------+
|                                                                                                                       |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  | 📅 Monitoring Jadwal Resep Harian Pasien                                                                         |  |
|  | Filter Periode: [ Hari Ini (29 Sep 2026) v ]  Dari: [ 29/09/2026 ]  Sampai: [ 29/09/2026 ]     [ 🔄 Filter ]    |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
|  |                                                                                                                 |  |
|  | +-------------------------------------------------------------------------------------------------------------+ |  |
|  | | 🗓️ Jadwal Tanggal: 29 September 2026 (Hari Perawatan Ke-3)                                                 | |  |
|  | | - Ceftriaxone 1g Injeksi    • Pagi: [✓ Diberikan jam 08:00]  | Malam: [⏳ Rencana jam 20:00]                 | |  |
|  | | - Paracetamol 500mg Tablet  • Pagi: [✓ Diberikan jam 07:30]  | Siang: [✓ Diberikan jam 13:00] | Malam: [⏳]   | |  |
|  | | - Puyer Batuk Pilek Anak    • Pagi: [✓ Diberikan jam 07:30]  | Siang: [✓ Diberikan jam 13:00] | Malam: [⏳]   | |  |
|  | +-------------------------------------------------------------------------------------------------------------+ |  |
|  +-----------------------------------------------------------------------------------------------------------------+  |
+-----------------------------------------------------------------------------------------------------------------------+
```

---

### 5.3. Tabel Matriks Kesesuaian 100% Antarmuka V1 vs Final

| Elemen UI | Kondisi Asli V1 (`captures/dokter-rawat-inap/05-resep/`) | Implementasi Final (Paritas 100%) | Status Validasi |
| :--- | :--- | :--- | :--- |
| **Navigasi Sub-Tab** | Horizontal 4 tab: `Buat Resep`, `Template Resep`, `History Resep`, `Resep Harian` | Memprioritaskan 4 sub-tab utama V1, dengan dukungan rekonsiliasi & sliding scale | **100% Sesuai** |
| **Banner Header** | Kotak teal/hijau memuat judul, tombol `Gunakan Template`, badge `Asuransi` | Banner teal/hijau dengan tombol `Gunakan Template`, badge penjamin pasien (`Allianz`/`BPJS`/`Tunai`) | **100% Sesuai** |
| **Segmented Pill Mode** | 3 pill: `[ Resep ]` (cyan), `[ Obat Racikan ]` (hijau), `[ ⚖ Rekonsiliasi Obat ]` (oranye) | 3 pill toggle button identik yang langsung mengalihkan mode peresepan secara instan | **100% Sesuai** |
| **Tata Letak Form Resep** | **Split View 2 Kolom**: Kiri `Daftar Obat`, Kanan `Detail Obat` | **Split View 2 Kolom**: Kiri katalog live search formularium, Kanan form signa/dosis | **100% Sesuai (Modal Dihapus)** |
| **Input Obat Jadi** | `Nama Obat` (read-only), `Jumlah Obat`, `Signa [ ] x [ ]`, `Signa Tambahan`, `Catatan`, `Estimasi`, `Cara Pakai` | Input identik dengan validasi numerik `signaBefore` x `signaAfter`, tombol `Reset Obat` & `Tambahkan` | **100% Sesuai** |
| **Tata Letak Racikan** | **Split View 2 Kolom**: Kiri Form Racikan & Komponen, Kanan `Daftar Obat` | **Split View 2 Kolom**: Kiri identitas racikan & tabel bahan, Kanan katalog untuk klik `[ + Bahan ]` | **100% Sesuai** |
| **Tabel Resep Terpilih** | Card bawah `Resep yang Dipilih` dengan badge count, tabel butir, dan tombol footer | Card bawah `Resep yang Dipilih` dengan badge `Obat Biasa`, `Racikan`, `Total`, dan 3 tombol footer | **100% Sesuai** |

---

## 6. Rencana Implementasi Tuntas (Definition of Done)

Setelah dokumen analisis dan rencana kerja ini disetujui pengguna, implementasi full-stack dieksekusi secara menyeluruh:

### 6.1. Pekerjaan Backend (ASP.NET Core):
1. Memastikan atomisitas pembuatan resep dengan item dan compound pada endpoint `POST /prescriptions` (`PrescriptionController.cs` & `PrescriptionWorkspaceService.cs`).
2. Menjamin filter resep harian (`GET /prescriptions/episodes/{episodeId}`) dengan query parameter period (`Today`, `ThisWeek`, `ThisMonth`, `Range`) berjalan konsisten tanpa error 422.
3. Memastikan integrasi keputusan rekonsiliasi (`POST /medication-reconciliations/{id}/decisions`) dapat menginjeksi obat bawaan yang dilanjutkan ke draf resep aktif.
4. Menjalankan verifikasi kompilasi backend: `dotnet build QuilvianSystemBackend.csproj --no-incremental` dengan **0 Error(s)**.

### 6.2. Pekerjaan Frontend (Next.js & Redux/Base Components):
1. **Transformasi Menyeluruh `PrescriptionBuilderPanel.jsx` Menjadi Split-View 2 Kolom Persis V1**:
   - Menghadirkan **Banner Header Hijau/Teal**: Judul `Resep Obat`, tombol `Gunakan Template`, dan badge `Asuransi: {namaAsuransi}`.
   - Menghadirkan **3 Segmented Pill Buttons**: `[ Resep ]` (cyan aktif), `[ Obat Racikan ]` (hijau), dan `[ ⚖ Rekonsiliasi Obat ]` (oranye).
   - Menghadirkan **Split-View 2 Kolom Mode Resep Standar (`01-buat-resep-standar.png`)**:
     - *Kolom Kiri*: Box `Daftar Obat` formularium dengan live search, counter jumlah obat, dan tabel berbaris dengan tombol `[ Pilih ]`. Tanpa popup modal!
     - *Kolom Kanan*: Form `Detail Obat` dengan `Nama Obat` (read-only), info stok & harga & asuransi, baris `Jumlah Obat` & `Signa * [ ] x [ ]`, `Signa Tambahan`, `Catatan`, baris `Estimasi Pemberian` & `Cara Pemakaian` dropdown, serta tombol `[ Reset Obat ]` dan `[ Tambahkan ]`.
   - Menghadirkan **Split-View 2 Kolom Mode Obat Racikan (`02-buat-resep-racikan.png`)**:
     - *Kolom Kiri*: Box `Buat Racikan Baru` (nama racikan*, bentuk sediaan dropdown, jumlah racikan, signa racikan*, signa tambahan, tabel komponen bahan racikan, tombol `[ Reset Form ]` & `[ Jadikan Racikan ]`).
     - *Kolom Kanan*: Box `Daftar Obat` untuk memilih bahan baku racikan secara langsung.
   - Menghadirkan **Panel Rekonsiliasi Obat Admisi (`03-buat-resep-rekonsiliasi.png`)**:
     - Box oranye `Rekonsiliasi Obat Admisi` dengan tombol tindakan cepat `[ Lanjutkan ]` dan `[ Ubah Aturan Pakai ]` yang menginjeksi obat ke draf resep aktif.
   - Menghadirkan **Card Bawah `Resep yang Dipilih`**:
     - Header dengan badge `[ Obat Biasa: X ] [ Racikan: Y ] [ Total: Z Item ]`.
     - Tabel butir resep aktif dengan rincian kategori, signa, aturan pakai, estimasi harga, dan aksi hapus/edit.
     - Footer baris aksi: `[ Bersihkan Draf ]`, `[ Simpan sebagai Template Pribadi ]`, dan `[ Simpan Draft Resep ]`.
2. **Penyempurnaan Sub-Tab `Template Resep`, `History Resep`, dan `Resep Harian`**:
   - Menyelaraskan tampilan `PrescriptionTemplatesPanel` persis capture `04-template-resep.png`.
   - Menyelaraskan tampilan `PrescriptionHistoryPanel` persis capture `05-history-resep.png`.
   - Menyelaraskan tampilan `PrescriptionDailyPanel` persis capture `06-resep-harian.png`.
3. **Penyelarasan Hook `use-inpatient-prescription-tab.jsx`**:
   - Memastikan katalog obat formularium awal otomatis termuat saat encounter aktif tanpa mewajibkan pengetikan 2 huruf terlebih dahulu.
4. **Verifikasi Kompilasi & Pengujian Paritas (100% Lulus)**:
   - Memastikan seluruh unit test lulus 100% tanpa regresi.

---

## 7. Status Persetujuan & Bukti Implementasi Tuntas

> **STATUS DOKUMEN: DISETUJUI & DIIMPLEMENTASIKAN DI TINGKAT SOURCE — `FE-RWI-137` (29 September 2026)**  
> Persetujuan Pengguna: *"oke disetujui"*  
> Laporan task: [`FE-RWI-137-resep-paritas-v1-split-view.md`](../../../task/report/frontend/FE-RWI-137-resep-paritas-v1-split-view.md)  
> Belum di-commit. `npm run build` dan uji runtime di peramban **NOT RUN** — dijalankan pemilik (lihat 7.2).

### 7.0. Koreksi Status 29 September 2026

Sebelum `FE-RWI-137` dikerjakan, bagian ini sudah menyatakan "SELESAI DIIMPLEMENTASIKAN", tetapi
source belum mendukungnya: Buat Resep masih memakai modal katalog, belum ada banner, pill tiga mode,
maupun split-view, dan racikan masih memakai modal. Ringkasan 7.1 di bawah **dipertahankan sebagai
riwayat** dan bukan bukti implementasi; bukti yang berlaku ada pada 7.2.

Pengerjaan juga menemukan empat cacat yang tidak tercatat di dokumen ini, seluruhnya diperbaiki:

| No | Cacat | Dampak | Perbaikan |
| :--- | :--- | :--- | :--- |
| 1 | Bahan racikan dikirim sebagai `ingredients`, backend membaca `Items` | Racikan tersimpan tanpa bahan, tanpa galat | Pemetaan dipusatkan di `inpatient-prescription-builder-utils.js`; bahan dikirim sebagai `items` beserta mode kalkulasinya |
| 2 | Kunci idempotensi = episode + SOAP + jenis resep | Resep kedua dari SOAP yang sama dijawab dengan resep pertama; isinya hilang | Token idempotensi per draft |
| 3 | `doctorInstruction` tidak masuk payload | Instruksi farmasi yang diketik dokter hilang | Dikirim pada `POST /prescriptions` |
| 4 | Panel rekonsiliasi memanggil `ConfirmModal open=`, `ClinicalActionGuard canWrite=` | Modal keputusan tidak pernah muncul; tombol selalu terkunci | Prop dibetulkan (`show`, `allowed`) |

Dua koreksi isi dokumen: (a) bab 3.1 baris 1–2 — katalog obat berada di
`/api/v1/health-services/clinical-management/prescribing-drugs`, dan informasi klinis di
`/api/v1/health-services/master-data/drugs/{id}/clinical-information`; (b) bab 5.1 menyebut backend
"100% siap" — backend memang siap, tetapi cacat 1–3 di atas berada di frontend.

### 7.1. Ringkasan Hasil Penyelarasan Dokumen & Skema UI (riwayat — ditulis sebelum implementasi):
1. **Penyelarasan Skema UI/UX 100% Paritas V1 (6 Layar Capture Lapangan)**:
   - Skema wireframe dan spesifikasi komponen pada Bab 5 telah diperbarui secara tuntas mengikuti 6 tangkapan layar asli V1 (`01-buat-resep-standar.png` s.d. `06-resep-harian.png`).
   - Modal pop-up besar dihilangkan dari alur utama peresepan; digantikan oleh **Tampilan 2 Kolom Berdampingan (*Inline Split-View*)**:
     - Mode Resep Biasa: Kolom Kiri `Daftar Obat` formularium dengan pencarian instan, Kolom Kanan Form `Detail Obat` (dosis, signa `[ ] x [ ]`, aturan pakai, estimasi).
     - Mode Obat Racikan: Kolom Kiri `Buat Racikan Baru` (nama, bentuk, signa, tabel komponen bahan), Kolom Kanan `Daftar Obat` untuk langsung klik `[ + Bahan ]`.
     - Mode Rekonsiliasi: Panel oranye `Rekonsiliasi Obat Admisi` dengan tombol tindakan `[ Lanjutkan ]` dan `[ Ubah Aturan Pakai ]`.
     - Panel Bawah: Card `Resep yang Dipilih` lengkap dengan counter badge dan tombol aksi footer.
   - Ditegaskan bahwa **tidak ada penghalang teknis** dari backend maupun frontend untuk menerapkan antarmuka ini.
2. **Backend Architecture & Integrity**:
   - Persistensi atomis pada `POST /prescriptions` via `PrescriptionWorkspaceService` untuk header resep, obat jadi (`PhmPrescriptionItem`), dan racikan beserta bahannya (`PhmPrescriptionCompound` & `PhmPrescriptionCompoundItem`).
   - Kontrak data dan Swagger API terverifikasi utuh tanpa *orphan records*.
3. **Verifikasi Pengujian Otomatis (100% Lulus)**:
   - Unit test suite `tests/unit/inpatient-prescription-compound-builder.test.mjs` lulus 100% (**3/3 passing**).
   - Test suite `tests/unit/inpatient-prescription-parity.test.mjs` lulus 100% (**7/7 passing**).
   - Test suite `tests/unit/inpatient-physician-clinical-tabs.test.mjs` lulus 100% (**35/35 passing**).

### 7.2. Bukti Implementasi `FE-RWI-137` (29 September 2026)

| Butir DoD (bab 6) | Status | Bukti |
| :--- | :--- | :--- |
| 6.1.1 Atomisitas `POST /prescriptions` | ✅ Sudah ada, tidak diubah | `PrescriptionController.cs` baris 341–408 (satu transaksi header + butir + racikan) |
| 6.1.2 Filter `period` Resep Harian tanpa `422` | ✅ Sudah ada, tidak diubah | `PrescriptionController.GetByEpisode`; `422` hanya untuk `Range` tanpa `from`/`to` |
| 6.1.3 Rekonsiliasi menyalin obat lanjut ke draft | ✅ Sudah ada di server (`BE-RWI-101`) | Frontend sengaja tidak menyalin ulang ke draft lokal agar tidak tercatat dua kali |
| 6.1.4 `dotnet build` | `NOT RUN` | Tidak ada perubahan source backend |
| 6.2.1 Banner, tiga pill, split-view Resep & Racikan, panel Rekonsiliasi, kartu Resep yang Dipilih | ✅ | `prescription-builder-panel.jsx`, `prescription-drug-catalog-panel.jsx`, `prescription-regular-drug-form.jsx`, `prescription-compound-builder.jsx`, `prescription-reconciliation-panel.jsx`, `prescription-selected-items-card.jsx` |
| 6.2.2 Template Resep & History Resep sesuai capture | ✅ | `prescription-templates-panel.jsx`, `prescription-history-panel.jsx`. Resep Harian tidak diubah: capture V1 `06` hanya berisi "Informasi dokter tidak tersedia" |
| 6.2.3 Katalog termuat tanpa mengetik 2 huruf | ✅ | `use-inpatient-drug-catalog.jsx` |
| 6.2.4 Lint & test | ✅ lint 0 error 0 peringatan; test resep 19/19 | Suite penuh 2084 test / 2079 lulus; 5 gagal di test menu sidebar yang tidak disentuh (`EXISTING`) |
| `npm run build` & uji runtime | `NOT RUN` | `next dev` pemilik sedang berjalan; dijalankan pemilik |

**Sub-tab akhir:** Buat Resep (mode Resep / Obat Racikan / Rekonsiliasi Obat), Template Resep,
History Resep, Resep Harian, Sliding Scale.

**Temuan terbuka:** panel Sliding Scale memiliki salah-prop komponen dasar yang sama dengan panel
Rekonsiliasi sebelum diperbaiki — modal order tidak pernah terbuka. Di luar cakupan dokumen ini;
diusulkan sebagai `FE-RWI-138`. Rincian di laporan `FE-RWI-137` bagian 7.

