# ISSUE-INS-001 — Ketidaktersediaan Kelas pada Pendaftaran Asuransi Pasien Menyebabkan Kegagalan Penentuan Coverage Lintas Modul

```yaml
issue_id: ISSUE-INS-001
module_id: cross-module
submodule: insurance-patient-coverage
layar: "Master Data Asuransi Pasien (FE-PAT-INS-01) & Registrasi Kunjungan (FE-REG-01 / FE-EMG-01)"
sumber_laporan: "Laporan investigasi dan temuan operasional: ketika pasien mendaftarkan asuransi tidak mendaftarkan kelas, mengakibatkan sistem tidak dapat menentukan apakah pasien dicover atau tidak dicover"
tanggal_issue: "2026-10-16"
status: TERBUKA
keparahan_tertinggi: High
source_sha_backend: "ca09cae9 (MHamzah)"
source_sha_frontend: "4d9bf608d (HamzahV2)"
rencana_perbaikan: "./plan-repair-001-penetapan-kelas-asuransi-dan-kegagalan-coverage.md"
```

---

## 1. Ringkasan Eksekutif

Laporan ini menganalisis temuan krusial pada integrasi modul Rekam Medis (Manajemen Pasien), Registrasi Kunjungan (Front Desk, IGD, Rawat Inap), Pelayanan Medis (Resep & Tindakan), serta Billing/Kasir di seluruh lingkungan Quilvian.

Masalah utama berakar dari formulir pendaftaran kartu asuransi pasien ([`MstPatientInsurance`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs)) yang hanya menyediakan isian kelas berupa teks bebas biasa (`ClassName` bertipe `string`, opsional) dan **tidak memiliki Foreign Key ke Master Kelas Pasien (`PatientClassId` -> `MstPatientClass`)**. Bukti basis data menunjukkan **88% data kartu asuransi pasien saat ini memiliki nilai kelas kosong (`NULL`)**.

Di sisi lain, katalog kontrak tarif asuransi rumah sakit ([`MstInsuranceTariff`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/MasterData/Models/MstInsuranceTariff.cs)) yang berisi **1.407.156 baris tarif**, sebanyak **99,99% (1.406.996 baris) dikunci ketat berbasis `PatientClassId`**. Ketika pasien berobat tanpa memiliki kelas yang valid (khususnya di IGD di mana 100% data riil kunjungan asuransinya berkelas `NULL`), mesin penilai penjaminan ([`InsuranceCoverageService`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs)) tidak dapat menemukan tarif kontrak yang cocok. Karena sistem menganut prinsip *positive list* (item hanya ditanggung bila ada di buku tarif kontrak), sistem secara otomatis memvonis seluruh obat dan tindakan sebagai **TIDAK DICOVER (`NotCovered`)**, sehingga tagihan ratusan ribu hingga jutaan rupiah secara keliru dibebankan penuh ke pasien sebagai tagihan tunai mandiri (*out-of-pocket*).

---

## 2. Laporan Asli

| No. Laporan | Kalimat Keluhan Pengguna | Keterangan & Rujukan |
| :---: | :--- | :--- |
| 1 | *"Issue masalah sekarang adalah ketika pasien mendaftarkan asuransi dia tidak mendaftarkan kelas, yang mengakibatkan tidak bisa di tentukan oleh pasien di cover dan tidak di cover. Apakah issue masalah itu benar?"* | Pertanyaan verifikasi atas perilaku kalkulasi coverage asuransi pada data pelayanan dan billing. |

---

## 3. Ringkasan Temuan

| ID Temuan | No. Laporan | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-INS-001-01` | 1 | Kartu asuransi pasien tidak menyimpan ID Master Kelas (`PatientClassId`) melainkan string bebas nullable | `DESIGN_CHANGE` | Backend + Frontend | **High** | `SUDAH-VERIFIKASI` (Source & DB) | `FIX-INS-001-01` |
| `ISS-INS-001-02` | 1 | Buku tarif asuransi (`MstInsuranceTariff`) mengunci kelas wajib pada 99,99% baris tarif kontrak | `BUG` / `LOGIC` | Backend | **High** | `SUDAH-VERIFIKASI` (Source & DB) | `FIX-INS-001-02` |
| `ISS-INS-001-03` | 1 | Pendaftaran kunjungan IGD tidak menetapkan kelas pasien sehingga 100% kunjungan asuransi gagal cover | `BUG` | Backend + Frontend | **High** | `SUDAH-VERIFIKASI` (Source & DB) | `FIX-INS-001-03` |
| `ISS-INS-001-04` | 1 | Mesin coverage langsung menolak item (`NotCovered`) tanpa mekanisme fallback saat kelas layanan null | `BUG` | Backend | **Medium** | `SUDAH-VERIFIKASI` (Source) | `FIX-INS-001-04` |

---

## 4. Rincian Per Temuan

### ISS-INS-001-01 — Kartu Asuransi Pasien Tidak Menyimpan ID Master Kelas (`PatientClassId`)

| Atribut | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `DESIGN_CHANGE` / `CONTRACT_MISMATCH` |
| **Area** | Backend (Model & DTO) + Frontend (Form Editor) |
| **Keparahan** | **High** |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source code dan basis data live PostgreSQL |
| **Perbaikan** | `FIX-INS-001-01` |

#### A. Apa yang Terjadi (Sudut Pandang Operasional Rumah Sakit)
Saat staf rekam medis atau loket pendaftaran menginput kartu asuransi pasien (misalnya BPJS Kesehatan atau Asuransi Swasta), formulir hanya menyediakan isian teks biasa bertuliskan *"Kelas"* yang tidak wajib diisi. Petugas kerap membiarkannya kosong. Jika diisi pun, petugas mengetik secara bebas (seperti *"Kelas 1"*, *"kelas 1"*, atau *"KELAS I"*). Akibatnya, komputer tidak mengenali hak kelas pertanggungan resmi pasien tersebut dalam sistem rumah sakit.

#### B. Kenapa Terjadi (Rantai Sebab & Bukti Kode)
1. **Model Backend [`MstPatientInsurance.cs` baris 33](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientInsurance.cs#L33):**
   ```csharp
   [MaxLength(100)]
   public string? ClassName { get; set; }
   ```
   Entitas hanya memiliki kolom teks string `ClassName`, dan **tidak memiliki Foreign Key `public Guid? PatientClassId { get; set; }`** serta navigasi `public MstPatientClass? PatientClass { get; set; }`.
2. **Konstanta Frontend [`patient-insurance-constants.jsx` baris 119](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/constants/health-services/patient-management/master-data/patient-insurance-constants.jsx#L119):**
   ```javascript
   { name: "className", label: "Kelas", placeholder: "Masukkan kelas" }
   ```
   Field didefinisikan sebagai input teks terbuka tanpa validasi, bukan dropdown yang terikat ke master data `MstPatientClass`.
3. **Bukti Data Nyata di Basis Data PostgreSQL (`QuilvianNewDevHamzah`):**
   * Jumlah kartu asuransi tersimpan: 25 kartu.
   * `ClassName IS NULL`: **22 kartu (88%)**.
   * `ClassName` teks bebas inkonsisten: 3 kartu (`"KELAS I"`, `"Kelas 1"`, `"kelas 1"`).

#### C. Dampak Nyata di Lapangan
Pasien pemegang polis BPJS Kelas 1 yang datang berobat tidak memiliki data hak kelas formal pada rekam medis elektroniknya. Saat registrasi kunjungan dibuat, sistem tidak dapat melakukan pengisian otomatis (*auto-populate*) atau validasi hak kelas kamar/layanan pasien.

---

### ISS-INS-001-02 — Buku Tarif Asuransi (`MstInsuranceTariff`) Mengunci Kelas pada 99,99% Baris

| Atribut | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `LOGIC` / `BUG` |
| **Area** | Backend (`InsuranceCoverageService.cs`) |
| **Keparahan** | **High** |
| **Status Bukti** | `SUDAH-VERIFIKASI` di source code dan basis data live PostgreSQL |
| **Perbaikan** | `FIX-INS-001-02` |

#### A. Apa yang Terjadi (Sudut Pandang Operasional Rumah Sakit)
Rumah sakit memiliki perjanjian kerja sama (PKS) tarif dengan penjamin asuransi untuk setiap kelas perawatan (misal: harga tindakan nebulisasi untuk Kelas Rawat Jalan berbeda dengan Kelas VIP). Karena sistem dirancang dengan aturan *positive list* (hanya item yang secara tertulis ada di buku tarif kontrak yang ditanggung), jika kelas pasien tidak diketahui secara pasti, sistem langsung menganggap penjamin tidak menanggung item tersebut sama sekali.

#### B. Kenapa Terjadi (Rantai Sebab & Bukti Kode)
1. **Kondisi Pencarian Tarif di [`InsuranceCoverageService.cs` baris 605](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L605):**
   ```csharp
   var candidates = await _dbContext.Set<MstInsuranceTariff>()
       .AsNoTracking()
       .Where(x =>
           !x.IsDelete &&
           x.IsActive &&
           x.InsuranceProviderId == context.InsuranceProviderId.Value &&
           x.TariffId == tariffId &&
           (!x.PatientClassId.HasValue || x.PatientClassId == context.PatientClassId) && // <-- BLOKIR UTAMA
           ...)
   ```
2. **Kondisi Penolakan Otomatis di [`InsuranceCoverageService.cs` baris 270–280](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs#L270-L280):**
   ```csharp
   // Positive list: tidak ada insurance tariff berarti tidak dicover.
   if (insuranceTariff == null)
   {
       return BuildNotCoveredResult(
           tariff,
           context,
           quantity,
           hospitalUnitPrice,
           hospitalTotalPrice,
           "Item tidak terdapat pada buku tarif asuransi.",
           isFallbackTariff: isFallbackTariff,
           pricingWarning: pricingWarning);
   }
   ```
3. **Fakta Distribusi Basis Data Live:**
   * Total tarif asuransi: **1.407.156 baris**.
   * Tarif dengan kelas terisi (`PatientClassId IS NOT NULL`): **1.406.996 baris (99,99%)**.
   * Tarif tanpa kelas (`PatientClassId IS NULL`): hanya **160 baris**.
   * Ketika `context.PatientClassId` bernilai `null`, kondisi `(!x.PatientClassId.HasValue || x.PatientClassId == null)` bernilai **FALSE** untuk 1,4 juta baris tersebut. Hasil query menghasilkan list kosong, memicu penolakan coverage seketika.

---

### ISS-INS-001-03 — Kunjungan IGD Asuransi Tidak Menetapkan Kelas (100% Berstatus Null)

| Atribut | Keterangan |
| :--- | :--- |
| **No. Laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Backend (Registrasi IGD) + Basis Data |
| **Keparahan** | **High** |
| **Status Bukti** | `SUDAH-VERIFIKASI` di basis data live |
| **Perbaikan** | `FIX-INS-001-03` |

#### A. Apa yang Terjadi (Sudut Pandang Operasional Rumah Sakit)
Pasien darurat yang datang ke IGD dan didaftarkan dengan jaminan asuransi selalu mendapati obat dan tindakan kegawatdaruratannya (seperti infus, injeksi, oksigen) berstatus **TIDAK DICOVER** di kasir. Pasien atau keluarga dipaksa membayar tunai di akhir pemeriksaan IGD karena sistem menganggap asuransinya tidak berlaku.

#### B. Kenapa Terjadi (Rantai Sebab & Bukti Kode)
1. Pada alur registrasi rawat jalan ([`PatientEncounterController.cs` baris 2066](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs#L2066)), backend memiliki logika proteksi otomatis menetapkan kelas `"RAWAT JALAN"` (`CLS-0`) bila request kosong.
2. Namun untuk **IGD (`EncounterType.Emergency`)**, tidak ada penetapan kelas bawaan. Formulir pendaftaran IGD juga tidak mewajibkan pemilihan kelas pelayanan.
3. **Bukti Basis Data Nyata:**
   * Pemeriksaan tabel `RegPatientEncounter` di database live menunjukkan dari 5 kunjungan IGD berpenjamin asuransi, **5 dari 5 kunjungan (100%) memiliki `PatientClassId = NULL`**.
   * Akibatnya, seluruh perhitungan billing dan order resep/tindakan dokter di IGD langsung jatuh ke hasil *NotCovered*.

---

## 5. Tanya-Jawab Pelapor

> **T:** *Apakah issue masalah itu benar? (Ketika pasien mendaftarkan asuransi tidak mendaftarkan kelas, mengakibatkan tidak bisa ditentukan pasien dicover dan tidak dicover)*
>
> **J: Ya, issue tersebut 100% BENAR secara fungsional dan teknis.**
> 
> Penjelasannya:
> 1. Form pendaftaran kartu asuransi pasien di frontend memang **tidak menyimpan ID Kelas** ke database (hanya teks biasa opsional).
> 2. Di basis data saat ini, **88% kartu asuransi pasien tidak memiliki data kelas**.
> 3. Padahal, buku tarif kontrak asuransi rumah sakit (`MstInsuranceTariff`) **mengunci kelas pada 99,99% tarifnya** (1.406.996 baris tarif).
> 4. Akibatnya, saat kunjungan berlangsung tanpa penetapan ID kelas yang valid, mesin kalkulasi asuransi tidak dapat mencocokkan tarif kontrak asuransi. Sesuai prinsip *positive list*, sistem langsung memutus bahwa pasien **TIDAK DICOVER** dan membebankan seluruh biaya ke pasien secara tunai.

---

## 6. Temuan Tambahan

### ISS-INS-001-T1 — Inkonsistensi Kelas pada Kunjungan Rawat Jalan dengan Asuransi

| Atribut | Keterangan |
| :--- | :--- |
| **Kenapa Dicantumkan** | Ditemukan saat audit data live `RegPatientEncounter` dengan `PaymentType = 1` |
| **Deskripsi** | Dari 165 kunjungan Rawat Jalan berpenjamin asuransi di database: <br>• **114 kunjungan** ditetapkan berkelas `KELAS I` <br>• **46 kunjungan** berkelas `RAWAT JALAN` <br>• **5 kunjungan** berkelas `NULL` |
| **Dampak** | Tarif kontrak asuransi di `MstInsuranceTariff` memiliki baris tersendiri untuk `RAWAT JALAN` (74.060 tarif) dan `KELAS I` (74.052 tarif). Jika kunjungan rawat jalan dicatat sebagai `KELAS I`, sistem akan menggunakan tarif kamar rawat inap Kelas 1 untuk tindakan poliklinik, yang berpotensi menyebabkan *dispute* penagihan klaim dengan pihak asuransi. |

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pemilik Proses Bisnis / Manajemen Rumah Sakit

| No | Keputusan Bisnis | Pilihan Kebijakan | Rekomendasi Solusi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Apakah kartu asuransi pasien wajib memiliki referensi Hak Kelas Pertanggungan Polis? | **A.** Wajib pilih master kelas dari dropdown.<br>**B.** Opsional (tetap teks bebas). | **Pilih A (Wajib Master Kelas).** Hak kelas polis pasien (misal BPJS Kelas 1/2/3 atau Asuransi Eksekutif) wajib tersimpan sebagai relasi formal agar otomatis menjadi rujukan saat pasien berobat. | `FIX-INS-001-01` |
| **K-02** | Kelas default apa yang digunakan untuk pelayanan IGD yang belum menentukan kelas rawat inap? | **A.** Menggunakan kelas setara Rawat Jalan.<br>**B.** Menggunakan hak kelas yang tertera pada kartu polis pasien.<br>**C.** Kelas khusus gawat darurat (Emergency). | **Pilih B**, dengan fallback ke **Pilihan A** bila kartu polis belum mencantumkan kelas. | `FIX-INS-001-03` |
| **K-03** | Bagaimana perilaku mesin coverage jika kelas kunjungan tidak ditemukan di buku tarif asuransi? | **A.** Langsung tolak cover (kondisi saat ini).<br>**B.** Fallback ke tarif kelas standar RS dan beri peringatan ke petugas verifikator asuransi. | **Pilih B.** Pasien tidak boleh langsung ditagih tunai hanya karena kendala mapping teknis kelas di sistem. | `FIX-INS-001-04` |

---

## 8. Catatan Pola & Arsitektur

Masalah ini merupakan contoh klasik dari **kebocoran integritas relasi data induk (*leaky relational integrity*)**:
1. Field relasi kunci (`PatientClassId`) digantikan oleh teks string bebas pada lapisan entri data pasien (`MstPatientInsurance.ClassName`).
2. Namun pada lapisan transaksi hilir (`MstInsuranceTariff` dan `InsuranceCoverageService`), sistem justru menuntut integritas ketat berbasis ID relasional (`PatientClassId == context.PatientClassId`).
3. Ketidaksinkronan antara lapisan hulu (yang longgar) dan lapisan hilir (yang ketat) menyebabkan kegagalan fungsional masif di seluruh modul yang mengonsumsi layanan coverage.

---

## 9. Riwayat Dokumen

| Tanggal | Versi | Catatan Perubahan | Oleh |
| :---: | :---: | :--- | :--- |
| 2026-10-16 | 1.0.0 | Dokumen issue resmi diterbitkan berdasarkan audit kode dan basis data live PostgreSQL `QuilvianNewDevHamzah`. | AI Engineering Agent (Antigravity) |
