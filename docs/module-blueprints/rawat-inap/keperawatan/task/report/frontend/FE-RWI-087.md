# Laporan Perubahan Frontend — `FE-RWI-087`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-087` |
| **Judul** | Pemberian Obat & Alkes (MAR, Protokol Sliding Scale Insulin, Obat Bawaan Pasien, Resep Aktif, dan Pemakaian Alkes) |
| **Slice** | Gelombang 2 — `FE-KEP-13` Obat & Alkes (Layar Baru) |
| **Roadmap** | [`../../../roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md), Bagian 2 & Kartu `FE-RWI-087` |
| **Traceability** | `FR-KEP-064` s.d. `FR-KEP-076`, `FR-KEP-078`; `RWI-DEC-116`, `RWI-DEC-117`, `RWI-DEC-145` s.d. `148`, `RWI-DEC-150` (`G-22`), `RWI-DEC-155`; `VAL-KEP-27`, `VAL-KEP-28`, `VAL-KEP-29a`–`f`, `VAL-KEP-30a`–`f`, `VAL-KEP-31a`–`c`, `VAL-KEP-32a`–`c`, `VAL-KEP-33a`/`b`, `VAL-KEP-34`, `VAL-KEP-36a`/`b`/`e`; `INV-KEP-05`; State matrix 5.5 & 5.6; Kamus data 11.12–11.15; API-contract keperawatan `0.5.0` Bagian 7.11 & 7.12; API-contract dokter `0.6.0` Bagian 12.5 |
| **Contract Version** | `0.5.0` (`MedicationAdministrationChartResponse`, `MedicationAdministrationResponse`, `RecordMedicationAdministrationRequest`, `SlidingScalePreviewResponse`, `CreateSlidingScaleExecutionRequest`, `SlidingScaleExecutionResponse`) |
| **Dependency** | `FE-RWI-081` ✅ (Workspace V2 Navigation), `BE-RWI-115` ✅ (Pencatatan Pemberian Obat), `BE-RWI-123` ✅ (Pelaksanaan Sliding Scale), `BE-RWI-125` ✅ (Obat Bawaan Pasien) |
| **Klasifikasi** | `CRITICAL / HIGH-RISK` — **Layar paling berbahaya pada roadmap keperawatan**: mengendalikan pemberian obat dan dosis insulin langsung ke pasien rawat inap, penegakan cek ganda obat high-alert (`INV-KEP-05`), kepatuhan rekam medis tanpa penghapusan dosis (`FR-KEP-068`), pratinjau kalkulasi sliding scale (`FR-KEP-075`), dan keputusan rekonsiliasi dokter baca-saja bagi perawat (`FR-KEP-078`) |
| **Task Mode** | `CROSS-REPO` sempit — Kode implementasi di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Tanggal** | 18 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh 8 Acceptance Criteria (AC-1 s.d. AC-8) terbukti penuh. Pengujian unit otomatis lulus 8 dari 8 test (8/8 passing). Seluruh suite keperawatan rawat inap lulus 100% (111/111 passing). ESLint 0 error 0 warning. Next.js production build berhasil. |

---

## 1. Masalah yang Diselesaikan & Rasional Bisnis

### 1.1 Masalah Sebelumnya
1. **Risiko Fatal Kesalahan Dosis (*High-Alert Medication Safety Risk*):**
   Pada sistem konvensional, daftar pemberian obat (*Medication Administration Record* / MAR) sering kali dicatat manual di lembar kertas atau antarmuka yang tidak terintegrasi dengan verifikasi ganda (*double check*). Obat high-alert (seperti insulin, antikoagulan heparin, dan elektrolit pekat) berisiko tinggi diberikan tanpa konfirmasi perawat kedua, yang dapat berakibat fatal bagi pasien rawat inap.
2. **Ketiadaan Rekam Riwayat Dosis (*No Audit Trail on Corrections*):**
   Sebelumnya, apabila perawat salah memasukkan angka dosis atau jam pemberian, data lama dapat ditimpa atau dihapus (*hard delete*) begitu saja. Hal ini melanggar kaidah hukum rekam medis elektronik (RME) di mana setiap koreksi wajib memiliki alasan tertulis dan riwayat data lama harus tersimpan utuh.
3. **Kalkulasi Dosis Insulin Manual yang Rentan Galat (*Sliding Scale Calculation Errors*):**
   Dokter sering meresepkan insulin reguler berdasarkan kadar gula darah sewaktu (GDS) melalui protokol sliding scale. Perawat sebelumnya harus menghitung rentang secara manual di kepala. Salah membaca tabel rentang, salah satuan (misal mg/dL tertukar dengan mmol/L), atau salah menghitung dosis penyesuaian untuk pasien yang sensitif insulin sering menyebabkan hipoglikemia berat atau hiperglikemia yang tidak tertangani.
4. **Duplikasi Dosis pada Jadwal Terjadwal vs Insidental:**
   Pemberian insulin sliding scale sering dicatat dua kali: sekali di log pengawasan gula darah dan sekali lagi dibuat manual di lembar obat, menyebabkan kebingungan apakah pasien sudah disuntik atau belum.
5. **Kebingungan Wewenang Rekonsiliasi Obat Bawaan Pasien:**
   Perawat mendata obat yang dibawa pasien dari rumah, namun pada sistem lama perawat terkadang bingung membedakan antara mencatat obat bawaan dan memutuskan kelanjutan obat tersebut (apakah dilanjutkan, diubah dosisnya, atau dihentikan).

### 1.2 Solusi yang Dihadirkan
Melalui task **`FE-RWI-087`**:
1. **Pemberian Obat (MAR) Terpadu dengan 6 Status Dosis Baku (`AC-1` / `BE-RWI-114` s.d. `115`):**
   - Menampilkan bagan jadwal harian obat dengan 6 status dosis: `Due`, `Administered`, `Held`, `Refused`, `Missed`, dan `Cancelled`.
   - Menegakkan validasi input sesuai status: `Administered` mewajibkan dosis aktual, rute aktual, dan waktu pemberian; `Held`, `Refused`, dan `Missed` mewajibkan alasan status yang jelas (`AC-2` / `VAL-KEP-30b`).
   - Apabila waktu pemberian melenceng lebih dari 60 menit dari jadwal atau dosis aktual berbeda dari resep, sistem mewajibkan pengisian **Catatan Penyimpangan** (*Deviation Note*) (`VAL-KEP-30c`).
2. **Penegakan Cek Ganda Obat High-Alert (`AC-3` / `INV-KEP-05` / `FR-KEP-066`):**
   - Setiap obat berlabel *High-Alert* yang dicatat oleh perawat pertama otomatis berstatus `Pending` cek ganda dan **tidak dapat langsung dianggap `Administered`**.
   - Sistem mengunci tombol konfirmasi bagi perawat yang mencatat (pencatat dilarang mengonfirmasi dirinya sendiri). Konfirmasi wajib dilakukan oleh perawat kedua yang berbeda di unit yang sama.
3. **Koreksi Dosis Tanpa Penghapusan Rekam Medis (`AC-4` / `FR-KEP-068`):**
   - Dosis yang sudah tercatat tidak dapat dihapus (*no DELETE*).
   - Koreksi mewajibkan alasan minimal 5 karakter, nilai lama tersimpan sebagai revisi (*audit revision*), dan nomor revisi (*revision number*) naik secara transparan.
4. **Pratinjau Kalkulasi Protokol Sliding Scale Insulin (`AC-5` / `FR-KEP-075`):**
   - Menyediakan fitur simulasi pratinjau kalkulasi dosis sebelum perawat menyimpan ke sistem.
   - Sistem mencocokkan nilai GDS ke rentang protokol yang sah secara presisi (batas bawah inklusif, batas atas eksklusif).
   - Menampilkan alasan penyesuaian dosis apabila dosis aktual disesuaikan (misal: "GDS 280 mg/dL &rarr; rentang 250–299 (6 unit), disesuaikan separuh karena pasien sensitif insulin &rarr; 3 unit").
   - Rentang dosis 0 unit otomatis ditandai sebagai dosis `Held` dengan alasan *"GDS di bawah rentang pemberian"* (`G-22`).
5. **Proteksi Ketiadaan Order Aktif (`AC-6` / `FR-KEP-072` / `VAL-KEP-27`):**
   - Sistem menolak eksekusi sliding scale bila pasien tidak memiliki order aktif dari dokter DPJP, dan menyajikan instruksi informatif tanpa kegagalan antarmuka (*graceful UI boundary*).
6. **Eksekusi Satu Transaksi Idempoten (`AC-7` / `FR-KEP-074`):**
   - Pencatatan sliding scale mengeksekusi penyimpanan satu transaksi: nilai GDS dicatat ke pengawasan harian, tepat satu dosis MAR terbentuk/terisi, dan data pelaksanaan tersimpan dengan kunci idempotensi (*Idempotency-Key*), menjamin dosis tidak pernah muncul ganda.
7. **Rekonsiliasi Obat Bawaan Pasien Bersifat Baca-Saja (`AC-8` / `FR-KEP-078`):**
   - Perawat memiliki tombol untuk mencatat obat yang dibawa pasien dari luar rumah sakit.
   - Status keputusan klinis dokter (Lanjut Sama, Lanjut Ubah, Hentikan) ditampilkan secara **baca-saja** (*read-only*), tanpa kontrol bagi perawat untuk mengubah keputusan medis dokter.
8. **Konsol Terpadu 5 Sub-Bagian:**
   - Menyediakan navigasi segmental 5 bagian: *Pemberian Obat (MAR)*, *Sliding Scale*, *Obat Bawaan*, *Resep Aktif*, dan *Pemakaian Alkes*.

---

## 2. Alur Proses Bisnis & Skenario Rumah Sakit

### 2.1 Skenario 1: Pemberian Obat Rutin Ceftriaxone & Cek Deviasi Jadwal
- **PPA:** Ns. Siti Rahmawati, S.Kep (Perawat Ruang Melati).
- **Pasien:** Tn. Budi Santoso (Episode `#RWI-20260918-001`, Kamar 302 Bed B).
- **Resep:** Ceftriaxone 1 g IV setiap 12 jam (`q12h`), jadwal pukul 08:00 dan 20:00.
- **Alur Proses:**
  1. Pukul 08:05, Ns. Siti membuka menu **Asuhan Keperawatan** &rarr; tab **Obat & Alkes** &rarr; sub-tab **Pemberian Obat (MAR)**.
  2. Ns. Siti melihat kartu dosis pukul 08:00 berstatus **`Due`** (kuning). Ns. Siti mengklik tombol **"Catat Dosis"**.
  3. Modal pencatatan terbuka. Ns. Siti memilih status **`Administered`**, dosis aktual `1` g, rute `IV`, dan jam `08:05`. Karena waktu pemberian masih dalam toleransi &plusmn;60 menit, catatan deviasi tidak wajib.
  4. Ns. Siti mengklik **"Simpan Pemberian Obat"**. Sistem mengirim request ke server dengan `Idempotency-Key`. Status dosis langsung berubah menjadi **`Administered`** (hijau) lengkap dengan nama pencatat "Ns. Siti Rahmawati".

### 2.2 Skenario 2: Penahanan Dosis Obat Antihipertensi (`Held`)
- **PPA:** Ns. Siti Rahmawati, S.Kep.
- **Kasus:** Tn. Budi terjadwal Amlodipine 10 mg Oral pukul 12:00, namun hasil pemeriksaan tensi menunjukkan hipotensi (TD 90/60 mmHg).
- **Alur Proses:**
  1. Ns. Siti membuka MAR, memilih dosis Amlodipine 12:00, lalu memilih status **`Held`** (Ditahan).
  2. Sistem mewajibkan pengisian alasan. Ns. Siti mengetik: *"TD 90/60 mmHg, pasien pusing, lapor dr. Hendra DPJP instruksi tunda pemberian."*
  3. Setelah disimpan, kartu dosis berstatus **`Held`** (abu-abu) dan alasan penahanan tampil jelas pada rekam medis agar perawat shift berikutnya mengetahuinya.

### 2.3 Skenario 3: Pemberian Insulin Sliding Scale & Verifikasi Perawat Kedua
- **PPA:** Ns. Siti (Perawat 1) & Ns. Dewi (Perawat 2).
- **Kasus:** Tn. Budi memiliki order sliding scale Insulin Aspart. Pukul 11:30 sebelum makan siang, GDS diukur dan bernilai 280 mg/dL.
- **Alur Proses:**
  1. Ns. Siti membuka sub-tab **Sliding Scale**. Terbaca protokol aktif Tn. Budi: *Protokol Insulin Reguler Rawat Inap (v2 - Disesuaikan DPJP)*.
  2. Ns. Siti memasukkan nilai GDS `280` dengan satuan `mg/dL`, lalu mengklik **"Hitung Pratinjau Dosis"**.
  3. Sistem menampilkan pratinjau:
     - Rentang: `250–300 mg/dL`
     - Dosis kalkulasi: `6 Unit`
     - Catatan: *"Pasien sensitif insulin, disesuaikan 50% &rarr; 3 Unit."*
  4. Ns. Siti mengonfirmasi dosis aktual `3` unit dengan alasan *"Pasien sensitif insulin sesuai instruksi tertulis DPJP"*, lalu mengklik **"Simpan Pelaksanaan ke MAR"**.
  5. Dalam satu transaksi idempoten:
     - GDS 280 mg/dL tersimpan ke log pengawasan harian.
     - Tepat satu dosis terbentuk di MAR berstatus **`Due`** dengan penanda **`Pending Cek Ganda`** (karena insulin adalah obat *High-Alert*).
  6. Di akun Ns. Siti, tombol konfirmasi tidak aktif demi mematuhi aturan keselamatan `INV-KEP-05`.
  7. Ns. Dewi (perawat kedua) memeriksa spuit insulin dan mencocokkannya dengan order. Ns. Dewi membuka MAR dan mengklik tombol **"✓ Konfirmasi Cek Ganda"**.
  8. Dosis kini sah berstatus **`Administered`** dengan bukti rekam dua perawat: Ns. Siti (pencatat) dan Ns. Dewi (verifikator kedua).

### 2.4 Skenario 4: Pendataan Obat Bawaan Pasien Saat Admisi
- **PPA:** Ns. Siti Rahmawati.
- **Alur Proses:**
  1. Ns. Siti membuka sub-tab **Obat Bawaan**.
  2. Ns. Siti mengklik **"+ Catat Obat Bawaan"**, lalu memasukkan:
     - Nama: *Metformin 500 mg*
     - Dosis: *500 mg*
     - Rute: *Oral*
     - Frekuensi: *2x1 tablet sesudah makan*
     - Catatan: *Rutin diminum sejak 2 tahun lalu untuk diabetes melitus tipe 2.*
  3. Setelah disimpan, baris muncul di tabel dengan status keputusan dokter: **`Belum Diputuskan`** (abu-abu).
  4. Ns. Siti melihat tabel secara **baca-saja** tanpa tombol untuk memutuskan kelanjutan obat. Saat dr. Hendra visite, dr. Hendra memutuskan *Lanjut Sama* dari ruang kerja dokter, dan status di layar perawat otomatis terbarui menjadi **`Lanjut Sama`** (hijau).

---

## 3. Spesifikasi Teknis Endpoint API (Bergaya Swagger)

Berikut adalah spesifikasi antarmuka pemrograman aplikasi (API) backend yang digunakan pada modul ini:

### 3.1 Grup: Medication Administration (MAR)
`[Tags("Health Services / Pharmacy Management / Medication Administration")]`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Payload Request / Query | Payload Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/pharmacy-management/medication-administrations/episodes/{episodeId}` | Memuat bagan MAR satu hari untuk satu episode; memanggil pembentukan dosis otomatis di server sebelum membaca. | `MedicationAdministration:Read` | Query: `date` (yyyy-MM-dd), `includeStopped` (boolean) | `ApiResponse<MedicationAdministrationChartResponse>` |
| `GET` | `/v1/health-services/pharmacy-management/medication-administrations/{id}` | Memuat detail satu dosis obat beserta aksi yang diperbolehkan (`AvailableActions`). | `MedicationAdministration:Read` | Path: `id` (UUID dosis) | `ApiResponse<MedicationAdministrationResponse>` |
| `GET` | `/v1/health-services/pharmacy-management/medication-administrations/{id}/revisions` | Memuat riwayat revisi koreksi atau penolakan cek ganda dari satu dosis MAR. | `MedicationAdministration:Read` | Path: `id` (UUID dosis) | `ApiResponse<List<MedicationAdministrationRevisionResponse>>` |
| `PATCH` | `/v1/health-services/pharmacy-management/medication-administrations/{id}/record` | Mencatat hasil pemberian dosis `Due` (`Administered`, `Held`, `Refused`, `Missed`). | `MedicationAdministration:Create` | Body: `RecordMedicationAdministrationRequest`<br>Header: `Idempotency-Key` | `ApiResponse<MedicationAdministrationResponse>` |
| `POST` | `/v1/health-services/pharmacy-management/medication-administrations/as-needed` | Mencatat pemberian obat sesuai kebutuhan (PRN) dengan indikasi wajib. | `MedicationAdministration:Create` | Body: `RecordAsNeededAdministrationRequest`<br>Header: `Idempotency-Key` | `ApiResponse<MedicationAdministrationResponse>` |
| `POST` | `/v1/health-services/pharmacy-management/medication-administrations/unscheduled` | Mencatat pemberian obat tanpa jadwal terkonfigurasi dengan alasan wajib. | `MedicationAdministration:Create` | Body: `RecordUnscheduledAdministrationRequest`<br>Header: `Idempotency-Key` | `ApiResponse<MedicationAdministrationResponse>` |
| `PATCH` | `/v1/health-services/pharmacy-management/medication-administrations/{id}/double-check` | Perawat kedua mengonfirmasi (`Confirm`) atau menolak (`Reject`) dosis high-alert. | `MedicationAdministration:DoubleCheck` | Body: `DoubleCheckRequest` (`decision`, `note`, `expectedRevisionNumber`) | `ApiResponse<MedicationAdministrationResponse>` |
| `PUT` | `/v1/health-services/pharmacy-management/medication-administrations/{id}/correct` | Mengoreksi catatan dosis yang sudah tersimpan; alasan wajib dan nomor revisi naik. | `MedicationAdministration:Update` | Body: `CorrectMedicationAdministrationRequest` (`correctionReason`, `expectedRevisionNumber`) | `ApiResponse<MedicationAdministrationResponse>` |
| `PATCH` | `/v1/health-services/pharmacy-management/medication-administrations/{id}/prn-evaluation` | Mencatat hasil evaluasi efek terapi obat PRN setelah interval waktu tertentu. | `MedicationAdministration:Update` | Body: `PrnEvaluationRequest` (`prnEvaluationNote`) | `ApiResponse<MedicationAdministrationResponse>` |

### 3.2 Grup: Sliding Scale Execution
`[Tags("Health Services / Pharmacy Management / Sliding Scale Execution")]`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Payload Request / Query | Payload Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/v1/health-services/pharmacy-management/sliding-scale-executions/preview` | Menghitung simulasi rentang dan dosis insulin tanpa menyimpan ke database (`FR-KEP-075`). | `SlidingScaleExecution:Read` | Body: `PreviewSlidingScaleExecutionRequest` (`orderId`, `glucoseValue`, `glucoseUnit`) | `ApiResponse<SlidingScalePreviewResponse>` |
| `POST` | `/v1/health-services/pharmacy-management/sliding-scale-executions` | Mencatat GDS, membentuk/mengisi dosis MAR, dan menyimpan pelaksanaan dalam satu transaksi idempoten (`FR-KEP-072` s.d. `076`). | `SlidingScaleExecution:Create` + `MedicationAdministration:Create` | Body: `CreateSlidingScaleExecutionRequest`<br>Header: `Idempotency-Key` | `ApiResponse<SlidingScaleExecutionResponse>` |
| `GET` | `/v1/health-services/pharmacy-management/sliding-scale-executions/episodes/{episodeId}` | Memuat riwayat pelaksanaan sliding scale pasien dalam satu episode (default 7 hari). | `SlidingScaleExecution:Read` | Query: `from`, `to` (ISO DateTime) | `ApiResponse<List<SlidingScaleExecutionListItem>>` |
| `GET` | `/v1/health-services/pharmacy-management/sliding-scale-executions/{id}` | Memuat detail satu pelaksanaan sliding scale beserta tautan nomor dosis MAR. | `SlidingScaleExecution:Read` | Path: `id` (UUID pelaksanaan) | `ApiResponse<SlidingScaleExecutionResponse>` |

### 3.3 Grup: Medication Reconciliation (Obat Bawaan)
`[Tags("Health Services / Pharmacy Management / Medication Reconciliation")]`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Payload Request / Query | Payload Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/pharmacy-management/medication-reconciliations/episodes/{episodeId}` | Memuat daftar obat bawaan pasien satu episode beserta keputusan dokter terakhir (`FR-KEP-078`). | `MedicationReconciliation:Read` | Path: `episodeId` | `ApiResponse<List<MedicationReconciliationItemResponse>>` |
| `POST` | `/v1/health-services/pharmacy-management/medication-reconciliations` | Perawat mencatat obat bawaan baru pasien dari luar rumah sakit. | `MedicationReconciliation:Create` | Body: `RecordHomeMedicationRequest` (`inpEpisodeId`, `drugNameSnapshot`, `dose`, `route`, `note`) | `ApiResponse<MedicationReconciliationItemResponse>` |

---

## 4. Perubahan Berkas & Komponen

### 4.1 Berkas Baru (*Created Files*)
1. **`medication-administration.service.js`** ([medication-administration.service.js](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/services/health-services/pharmacy-management/medication-administration.service.js)):
   - Service client API untuk seluruh endpoint MAR rawat inap (`BE-RWI-114` s.d. `116`).
2. **`use-inpatient-medication-administration.js`** ([use-inpatient-medication-administration.js](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-medication-administration.js)):
   - Hook pengelola state MAR, navigasi tanggal harian, mutasi pencatatan dosis dengan idempotensi, koreksi beralasan, dan modal state.
3. **`use-inpatient-sliding-scale-execution.js`** ([use-inpatient-sliding-scale-execution.js](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-sliding-scale-execution.js)):
   - Hook pengelola eksekusi protokol sliding scale insulin: pemuatan order aktif, pratinjau kalkulasi dosis, penyesuaian dosis beralasan, dan transaksi penyimpanan.
4. **`nursing-medication-section.jsx`** ([nursing-medication-section.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/medication/nursing-medication-section.jsx)):
   - Master section konsol Obat & Alkes dengan 5 sub-tab navigasi.
5. **Panels Tampilan:**
   - `medication-administration-chart-panel.jsx`: Bagan jadwal MAR 6 status dosis, badge high-alert, dan aksi dosis.
   - `sliding-scale-execution-panel.jsx`: Pratinjau kalkulasi GDS sebelum simpan dan log riwayat pelaksanaan.
   - `home-medication-reconciliation-panel.jsx`: Daftar obat bawaan dengan keputusan DPJP baca-saja.
   - `active-prescriptions-panel.jsx`: Kartu dan butir resep aktif dokter satu episode.
   - `device-usage-panel.jsx`: Log pemakaian alkes dan BMHP.
6. **Modals Pencatatan Terstruktur:**
   - `record-medication-dose-modal.jsx`: Dialog pencatatan hasil dosis `Due`.
   - `correct-medication-dose-modal.jsx`: Dialog koreksi dosis dengan alasan wajib dan nomor revisi.
   - `record-home-medication-modal.jsx`: Dialog pendataan obat bawaan pasien oleh perawat.
7. **Unit Test:**
   - `tests/unit/inpatient-medication-administration.test.mjs`: Test suite otomatis untuk 8 kriteria penerimaan.

### 4.2 Berkas yang Diperbarui (*Modified Files*)
1. **`sliding-scale.service.js`** ([sliding-scale.service.js](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/services/health-services/pharmacy-management/sliding-scale.service.js)):
   - Menambahkan fungsi API pelaksanaan sliding scale perawat (`previewSlidingScaleExecution`, `createSlidingScaleExecution`, `getSlidingScaleExecutionsByEpisode`, `getSlidingScaleExecutionById`).
2. **`nursing-care-section.jsx`** ([nursing-care-section.jsx](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/nursing-care-section.jsx)):
   - Menggantikan placeholder `NursingUnavailableSection` pada `case "medication":` dengan `<NursingMedicationSection />`.
3. **`nursing-workspace.module.css`** ([nursing-workspace.module.css](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/style/health-services/inpatient-management/nursing-workspace.module.css)):
   - Menambahkan aturan CSS lengkap bertoken untuk tabel MAR, kartu dosis interaktif, lencana status 6 warna, formulir pratinjau GDS, dan kartu resep aktif.

---

## 5. Pemenuhan Kriteria Keberhasilan (Acceptance Criteria AC-1 s.d. AC-8)

| Kriteria | Spesifikasi & Aturan Bisnis | Status | Bukti Implementasi & Pengujian |
| :--- | :--- | :---: | :--- |
| **AC-1** | Daftar dosis menampilkan status `Due`, `Administered`, `Held`, `Refused`, `Missed`, `Cancelled` dengan pembeda yang jelas. | ✅ **Terpenuhi** | Terpetakan pada `DOSE_STATUS_MAP` dengan label dan kode warna terstandar (*warning, success, neutral, danger*). *(Test 1 Lulus)* |
| **AC-2** | Pencatatan dosis mewajibkan isian dan alasan sesuai statusnya (`FR-KEP-065`). | ✅ **Terpenuhi** | `Administered` mewajibkan dosis aktual, rute, dan waktu; deviasi > 60 menit atau beda dosis mewajibkan catatan deviasi (`VAL-KEP-30c`); `Held/Refused/Missed` mewajibkan alasan min 3 karakter (`VAL-KEP-30b`). *(Test 2 Lulus)* |
| **AC-3** | Dosis high-alert menampilkan penanda "Menunggu cek ganda (n)" dan **tidak dapat** ditandai `Administered` sebelum konfirmasi masuk (`FR-KEP-066`, `INV-KEP-05`). | ✅ **Terpenuhi** | Dosis high-alert tetap `Due` dengan `DoubleCheckStatus = Pending`. Tombol konfirmasi dikunci bagi perawat pencatat (`canDoubleCheck = false`) dan hanya dapat dikonfirmasi perawat kedua. *(Test 3 Lulus)* |
| **AC-4** | Koreksi dosis mewajibkan alasan; dosis **tidak pernah hilang** dari daftar (`FR-KEP-068`). | ✅ **Terpenuhi** | Dialog koreksi mewajibkan alasan minimal 5 karakter; nilai lama tersimpan sebagai revisi; tidak ada operasi `DELETE` pada modul MAR. *(Test 4 Lulus)* |
| **AC-5** | Sliding Scale menampilkan pratinjau rentang dan dosis **sebelum** menyimpan, beserta alasan penyesuaian bila ada (`FR-KEP-075`). | ✅ **Terpenuhi** | Panel pratinjau memanggil `previewSlidingScaleExecution` atau simulasi lokal; menampilkan label rentang, dosis hitung, instruksi, dan penyesuaian dosis beralasan (`VAL-KEP-29d`). *(Test 5 Lulus)* |
| **AC-6** | Sliding Scale menolak disimpan bila pasien tidak punya order aktif (`FR-KEP-072`, `VAL-KEP-27`). | ✅ **Terpenuhi** | Sistem memeriksa keberadaan order aktif; jika kosong, eksekusi ditolak dan tampil notice informatif `VAL-KEP-27`. *(Test 6 Lulus)* |
| **AC-7** | Dosis hasil sliding scale muncul **sekali** di MAR, tidak dua kali (`FR-KEP-074`). | ✅ **Terpenuhi** | Penyimpanan menggunakan satu transaksi idempoten dengan `Idempotency-Key` yang menghubungkan eksekusi ke tepat satu dosis MAR (`medicationAdministrationId`). *(Test 7 Lulus)* |
| **AC-8** | Obat Bawaan menampilkan keputusan rekonsiliasi **baca-saja**; perawat tidak melihat kontrol untuk mengubahnya (`FR-KEP-078`). | ✅ **Terpenuhi** | Panel obat bawaan menyajikan riwayat keputusan DPJP secara baca-saja (*read-only*); kontrol keputusan dokter ditiadakan mutlak dari antarmuka perawat. *(Test 8 Lulus)* |

---

## 6. Bukti Verifikasi Pengujian

### 6.1 Pengujian Unit Otomatis `FE-RWI-087` (8/8 Passing)
```bash
cmd /c npx node --test tests/unit/inpatient-medication-administration.test.mjs
```
```text
✔ FE-RWI-087 AC-1: Daftar dosis menampilkan status Due, Administered, Held, Refused, Missed, Cancelled dengan pembeda jelas (4.1ms)
✔ FE-RWI-087 AC-2: Pencatatan dosis mewajibkan isian dan alasan sesuai statusnya (FR-KEP-065, VAL-KEP-30a-c) (1.5ms)
✔ FE-RWI-087 AC-3: Dosis high-alert menampilkan penanda menunggu cek ganda dan tidak dapat ditandai Administered langsung oleh pencatat (INV-KEP-05 / FR-KEP-066) (0.9ms)
✔ FE-RWI-087 AC-4: Koreksi dosis mewajibkan alasan; dosis tidak pernah hilang dari daftar (FR-KEP-068) (0.7ms)
✔ FE-RWI-087 AC-5: Sliding Scale menampilkan pratinjau rentang dan dosis sebelum menyimpan (FR-KEP-075) (1.1ms)
✔ FE-RWI-087 AC-6: Sliding Scale menolak disimpan bila pasien tidak punya order aktif (FR-KEP-072 / VAL-KEP-27) (0.5ms)
✔ FE-RWI-087 AC-7: Dosis hasil sliding scale muncul tepat satu kali di MAR (FR-KEP-074) (0.5ms)
✔ FE-RWI-087 AC-8: Obat Bawaan menampilkan keputusan rekonsiliasi BACA-SAJA; perawat tidak melihat kontrol keputusan (FR-KEP-078) (0.5ms)
ℹ tests 8 | pass 8 | fail 0 | cancelled 0 | skipped 0 | todo 0 | duration_ms 104.87ms
```

### 6.2 Pengujian Regresi Seluruh Suite Keperawatan Rawat Inap (111/111 Passing)
```bash
cmd /c npx node --test tests/unit/inpatient-nursing-*.test.mjs tests/unit/inpatient-clinical-instrument-renderer.test.mjs tests/unit/inpatient-daily-monitoring.test.mjs tests/unit/inpatient-case-management-evaluation.test.mjs tests/unit/inpatient-medication-administration.test.mjs
```
*Hasil:* **111 tests lulus penuh (0 gagal, 0 batal, 0 dilewati)** dalam durasi 345.5 ms.

### 6.3 Validasi Linting ESLint (0 Error, 0 Warning)
```bash
cmd /c npx eslint src/lib/services/health-services/pharmacy-management/medication-administration.service.js \
                 src/lib/services/health-services/pharmacy-management/sliding-scale.service.js \
                 src/lib/hooks/health-services/inpatient-management/use-inpatient-medication-administration.js \
                 src/lib/hooks/health-services/inpatient-management/use-inpatient-sliding-scale-execution.js \
                 src/components/view/health-services/inpatient-management/nursing-workspace/sections/medication/ \
                 src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/nursing-care-section.jsx \
                 tests/unit/inpatient-medication-administration.test.mjs
```
*Hasil:* **Exit Code: 0 (0 errors, 0 warnings)**

### 6.4 Kompilasi Produksi Next.js
```bash
cmd /c npm run build
```
```text
▲ Next.js 16.2.12 (Turbopack)
- Environments: .env
  Creating an optimized production build ...
✓ Compiled successfully in 41s
  Running TypeScript ...
  Finished TypeScript in 320ms ...
  Collecting page data using 15 workers ...
  Generating static pages using 15 workers (320/320) in 3.8s
  Finalizing page optimization ...
> node scripts/prepare-standalone.mjs
[prepare-standalone] Berhasil menyalin static assets.
[prepare-standalone] Berhasil menyalin public assets.
[prepare-standalone] Standalone runtime siap dijalankan.
```
*Hasil:* **Exit Code: 0 (Kompilasi Sukses Penuh)**

---

## 7. Gerbang Kesiapan & Catatan Produksi

| Hal | Keterangan |
| :--- | :--- |
| **Gerbang `RWI-OQ-097`** | **Masih Terbuka Sebagian.** Pemakaian protokol sliding scale telah disetujui lewat `RWI-DEC-155`. Namun, nama pengesah isi protokol belum ada. Layar ini telah selesai dibangun dan teruji penuh; selama nama pengesah belum ada di master data, pasien belum memiliki order aktif dan antarmuka akan menampilkan peringatan `VAL-KEP-27` secara aman. |
| **Pencegahan Malpraktik Cek Ganda** | Sistem membandingkan `RecordedByUserId` dengan akun login saat ini, sehingga perawat yang mencatat dosis high-alert secara fisik tidak dapat mengonfirmasi sendiri pemberian obatnya (`INV-KEP-05`). |
| **Kepatuhan Rekam Medis** | Tidak ada API atau tombol hapus dosis di seluruh antarmuka MAR. Setiap koreksi terdokumentasi rapi di tabel revisi (`PhmMedicationAdministrationRevision`). |
