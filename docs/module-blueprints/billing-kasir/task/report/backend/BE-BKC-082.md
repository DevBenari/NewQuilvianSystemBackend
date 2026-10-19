# Laporan Perubahan Backend — `BE-BKC-082`

## Ringkasan untuk Pembaca Umum

Dalam pelayanan rumah sakit, pasien darurat sering kali masuk pertama kali melalui Instalasi Gawat Darurat (IGD) atau berkonsultasi di Instalasi Rawat Jalan (Poliklinik). Ketika kondisi medis pasien mengharuskan perawatan lebih lanjut, dokter menerbitkan perintah alih rawat (transfer) ke Instalasi Rawat Inap (Ranap). Saat rawat inap dikonfirmasi, sistem penagihan (Billing) harus mengenali dan menghubungkan riwayat kunjungan asal tersebut ke tagihan rawat inap baru agar seluruh tindakan dan obat yang diberikan di IGD/Poli dapat ditagihkan secara utuh atau diselaraskan administrasinya tanpa merugikan pasien.

Sebelum penyempurnaan `BE-BKC-082`:
1. **Tautan Kunjungan Otomatis Hanya Bekerja untuk Rujukan Operasi:**
   Method penaut `BillingInpatientEventReceiver.EnsureEncounterLinksAsync` sebelumnya hanya membaca rujukan operasi bedah (`InpAdmissionReferral` berstatus `Completed`). Pasien yang masuk rawat inap melalui jalur transfer IGD maupun transfer Poliklinik tidak pernah dibuatkan baris penaut (`BilInvoiceEncounterLink`) ke kunjungan asalnya secara otomatis saat rawat inap dikonfirmasi.
2. **Ketiadaan Pembakuan Alasan Penautan (*Link Reason*):**
   Model `BilInvoiceEncounterLink` masih menggunakan string literal mentah `"SURGERY_ORIGIN"` sebagai nilai bawaan tanpa tersedianya konstanta terpusat untuk transfer IGD maupun Poliklinik, meskipun nilai `"EMERGENCY_ORIGIN"` dan `"OUTPATIENT_ORIGIN"` sudah dirancang pada spesifikasi cetak biru arsitektur.
3. **Ketiadaan Validasi Lintas Pasien & Integritas Status Asal:**
   Belum ada penjaga yang memastikan bahwa kunjungan asal transfer benar-benar milik pasien yang sama, belum terhapus, belum dibatalkan, atau memiliki tipe sumber transfer yang sah (`Emergency` atau `Outpatient`).

### Solusi Penyempurnaan `BE-BKC-082`

Melalui implementasi task `BE-BKC-082` (arsitektur `BKC-DES-077`, `BKC-DES-078`, `BKC-DES-079`, kontrak `BIL-INT-021`):
1. **Konstanta Alasan Tautan Terpusat (`BKC-DES-078`):**
   Menambahkan kelas statis `BillingEncounterLinkReasons` di dalam berkas model `BilInvoiceEncounterLink.cs` dengan konstanta `SurgeryOrigin` (`SURGERY_ORIGIN`), `EmergencyOrigin` (`EMERGENCY_ORIGIN`), dan `OutpatientOrigin` (`OUTPATIENT_ORIGIN`).
2. **Perluasan Penautan Asal Transfer pada Konfirmasi Rawat Inap (`BKC-DES-077`):**
   `EnsureEncounterLinksAsync` kini membaca data kunjungan rawat inap (`encounterId`), lalu:
   - Jika `SourceEncounterId` terisi, memeriksa apakah kunjungan asal sudah bertautan (misalnya lewat rujukan operasi). Jika sudah, tautan lama dipertahankan (`SURGERY_ORIGIN` memiliki prioritas lebih dulu).
   - Jika belum, memetakan `AdmissionTransferSourceType`:
     - `Emergency` $\rightarrow$ `EMERGENCY_ORIGIN`
     - `Outpatient` $\rightarrow$ `OUTPATIENT_ORIGIN`
   - Menyimpan baris `BilInvoiceEncounterLink` baru dengan `SourceReferralId = null` dan `ReceiptId` tanda terima event yang sedang diproses.
3. **Perlindungan Privasi & Log Kegagalan Bertingkat (`BKC-DES-079`):**
   Jika kunjungan asal milik pasien lain (`PATIENT_MISMATCH`), telah dihapus (`DELETED_ORIGIN`), dibatalkan (`CANCELLED_ORIGIN`), tidak ditemukan (`ORIGIN_NOT_FOUND`), atau tipe transfer tidak dikenal (`INVALID_TRANSFER_SOURCE_TYPE`), penautan dilewati secara aman dan dicatat melalui `WarningAsync` aksi `InpatientEvent.TransferLinkSkipped` hanya berisi `EpisodeId`, `EncounterId`, `SourceEncounterId`, dan `ReasonCode` — **tanpa** mencantumkan nama pasien, NIK, atau nomor rekam medis. Konfirmasi rawat inap dan pembukaan invoice tetap berhasil dibuka secara aman (*fail-open for billing admission*).
4. **Dukungan Idempoten pada Putar Ulang (*Replay Event*) (`BIL-AT-191`, `BIL-AT-195`):**
   Jalur pesan duplikat/replay pada `BillingInpatientEventReceiver` kini ikut menyertakan `EncounterId` sehingga event lama yang diputar ulang otomatis menautkan kunjungan transfer yang sebelumnya terlewat menggunakan tanda terima asli tanpa membuat duplikasi.

---

## Contoh Kasus Nyata di Rumah Sakit

### Skenario Pasien Tn. Bambang (Transfer Gawat Darurat IGD ke Ruang Rawat Inap)
1. **Masuk IGD (Pukul 02.00 WIB):**
   - Tn. Bambang tiba di IGD dengan keluhan nyeri dada akut. Dilakukan penanganan medis darurat, pemasangan infus, rekam EKG, dan pemeriksaan laboratorium darurat dengan total tagihan IGD Rp 1.500.000.
2. **Keputusan Alih Rawat ke Rawat Inap (Pukul 04.30 WIB):**
   - Dokter IGD memutuskan pasien harus dirawat inap di unit intensif (ICU/Ranap). Petugas memproses transfer IGD ke rawat inap pada sistem registrasi, sehingga `SourceEncounterId` rawat inap menunjuk ke kunjungan IGD dengan tipe `Emergency`.
3. **Konfirmasi Admisi Rawat Inap:**
   - Unit Rawat Inap menerima pasien dan mengonfirmasi admisi tempat tidur. Event outbox `ADMISSION_CONFIRMED` dikirimkan ke modul penagihan (Billing).
4. **Hasil Eksekusi Sistem Baru (`BE-BKC-082`):**
   - `BillingInpatientEventReceiver` memproses event tersebut dalam transaksi atomik dan advisory lock.
   - Invoice rawat inap `RANAP` diterbitkan.
   - `EnsureEncounterLinksAsync` mendeteksi bahwa kunjungan rawat inap ini berasal dari transfer IGD Tn. Bambang.
   - Sistem secara otomatis menerbitkan satu baris penaut pada tabel `BilInvoiceEncounterLink` dengan `LinkReason = "EMERGENCY_ORIGIN"`, menghubungkan invoice ranap ke kunjungan IGD.
   - Saat petugas kasir atau perawat membuka halaman Estimasi Biaya Pasien ranap, tindakan IGD Rp 1.500.000 langsung tampil secara terpadu di bawah rincian tagihan pasien (`BIL-AT-201`), tanpa ada tindakan yang hilang atau tagihan yang tertinggal.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-082` |
| Judul | Penulis tautan kunjungan asal pada jalur transfer (revisi 2.2) |
| Slice / Gelombang | Gelombang Eksekusi 1 (Revisi Blueprint 2.2 / 2.3) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-082` |
| Trace | `FR-BKC-273`–`275`, `BKC-DEC-155`, `BKC-DES-077`–`079`, `BIL-INT-021`, `CAP-BIL-11` |
| Contract Version | `BIL-INTEGRATION-1.7` (`BIL-INT-021`), Kamus Data Amendment 2.2 (`BilInvoiceEncounterLink`) |
| Acceptance Criteria | `BIL-AT-189`–`195`, `BIL-AT-201` |
| Dependency | Tidak ada |
| Klasifikasi | Sedang / Backend Engineering (jalur penerima event rawat inap & integritas data penagihan) |
| Task mode | `BACKEND` |
| Target berkas | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceEncounterLink.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/BillingInpatientEventReceiver.cs` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Terverifikasi penuh)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing / Operational` |
| Entity / Model | `BilInvoiceEncounterLink`, `BilInpatientEventReceipt`, `RegPatientEncounter` (read-only) |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` (Perluasan logika penerima event integrasi rawat inap) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-API-001`, `QBE-SEC-001` (isolasi transaksi database, nol kueri N+1, pembacaan `AsNoTracking` pada entitas luar, perlindungan data privasi pada log) |
| Database Migration | Nol migrasi database baru (kolom dan indeks tabel `BilInvoiceEncounterLink` sudah ada dan mendukung `LinkReason` hingga 40 karakter serta `SourceReferralId` nullable) |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah

#### 1. [`BilInvoiceEncounterLink.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceEncounterLink.cs)
- **Perubahan**:
  - Menambahkan kelas statis `BillingEncounterLinkReasons` dengan konstanta `SurgeryOrigin` (`"SURGERY_ORIGIN"`), `EmergencyOrigin` (`"EMERGENCY_ORIGIN"`), dan `OutpatientOrigin` (`"OUTPATIENT_ORIGIN"`), serta daftar `All`.
  - Mengubah inisialisasi bawaan properti `LinkReason` dari string literal mentah menjadi `BillingEncounterLinkReasons.SurgeryOrigin`.

#### 2. [`BillingInpatientEventReceiver.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingInpatientEventReceiver.cs)
- **Perubahan**:
  - Menambahkan import namespace `QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models`.
  - Mengubah tanda tangan method privat `EnsureEncounterLinksAsync` untuk menerima parameter `Guid encounterId` rawat inap:
    `EnsureEncounterLinksAsync(Guid episodeId, Guid encounterId, Guid invoiceId, Guid receiptId, CancellationToken ct)`.
  - Memperbarui pemanggilan `EnsureEncounterLinksAsync` pada jalur utama event `AdmissionConfirmed` (baris 195) dengan menyertakan `envelope.EncounterId`.
  - Memperbarui pemanggilan `EnsureEncounterLinksAsync` pada jalur duplikat / replay (baris 100) dengan menyertakan `prior.EncounterId` (dengan fallback defensif ke `envelope.EncounterId`).
  - Menambahkan logika penautan transfer setelah pemrosesan rujukan operasi:
    1. Membaca data kunjungan rawat inap (`SourceEncounterId`, `AdmissionTransferSourceType`, `PatientId`) secara `AsNoTracking`.
    2. Melewati penautan secara senyap jika `SourceEncounterId` kosong (pasien masuk langsung tanpa transfer).
    3. Melewati penautan jika kunjungan asal telah bertautan sebelumnya (menjaga prioritas `SURGERY_ORIGIN` dan mencegah duplikasi).
    4. Memvalidasi `AdmissionTransferSourceType` (`Emergency` $\rightarrow$ `EMERGENCY_ORIGIN`, `Outpatient` $\rightarrow$ `OUTPATIENT_ORIGIN`).
    5. Memvalidasi status kunjungan asal (`originEncounter != null`, `!IsDelete`, `!IsCancel`, `PatientId == ranapEncounter.PatientId`).
    6. Mencatat peringatan log audit via `_loggerService.WarningAsync(LogCategory, "InpatientEvent.TransferLinkSkipped", ...)` jika salah satu validasi gagal tanpa membatalkan pembukaan invoice.
    7. Menerbitkan entitas `BilInvoiceEncounterLink` baru jika seluruh validasi terpenuhi.

---

## 2. Hasil Verifikasi dan Acceptance Criteria

| ID Kriteria | Uraian Target | Hasil Uji / Bukti | Status |
| :--- | :--- | :--- | :---: |
| `BIL-AT-189` | Pasien IGD dengan tagihan Rp 1.500.000 ditransfer, lalu rawat inap dikonfirmasi. | `EnsureEncounterLinksAsync` membaca `AdmissionTransferSourceType == "Emergency"`, menerbitkan tepat satu tautan bertipe `EMERGENCY_ORIGIN` dengan `SourceReferralId = null`. | ✅ **LULUS** |
| `BIL-AT-190` | Pasien poli ditransfer, lalu rawat inap dikonfirmasi. | `EnsureEncounterLinksAsync` membaca `AdmissionTransferSourceType == "Outpatient"`, menerbitkan tepat satu tautan bertipe `OUTPATIENT_ORIGIN`. | ✅ **LULUS** |
| `BIL-AT-191` | Event `ADMISSION_CONFIRMED` yang sama diproses dua kali. | Pemeriksaan daftar `linkedEncounterIds` mendeteksi tautan yang sudah ada dan melewatinya. Tautan tetap tepat 1 baris (idempoten). | ✅ **LULUS** |
| `BIL-AT-192` | Pasien dirawat inap langsung tanpa transfer. | `SourceEncounterId` bernilai `null` $\rightarrow$ method selesai seketika tanpa menulis baris tautan baru. | ✅ **LULUS** |
| `BIL-AT-193` | Kunjungan asal transfer ternyata milik pasien lain (`PatientId` berbeda). | Deteksi ketidaksesuaian pasien memicu `ReasonCode = "PATIENT_MISMATCH"`, penautan dilewati, dicatat via `WarningAsync` tanpa data sensitif NIK/nama/RM, dan invoice ranap tetap terbuka normal. | ✅ **LULUS** |
| `BIL-AT-194` | Kunjungan asal yang sama juga memiliki rujukan operasi. | Logika rujukan operasi dieksekusi terlebih dahulu dan menambahkan ID kunjungan ke `linkedEncounterIds`. Saat blok transfer dievaluasi, kondisi `linkedEncounterIds.Contains(sourceEncounterId)` terpenuhi sehingga tautan transfer dilewati dan `SURGERY_ORIGIN` dipertahankan. | ✅ **LULUS** |
| `BIL-AT-195` | Transfer lama tanpa tautan; event `ADMISSION_CONFIRMED` diputar ulang (*replay*). | Jalur duplikat memanggil `EnsureEncounterLinksAsync` dengan `ReceiptId = prior.Id`, sehingga tautan ditulis menggunakan tanda terima asli tanpa membuat tanda terima baru. | ✅ **LULUS** |
| `BIL-AT-201` | Ringkasan tagihan pasien rawat inap sesudah transfer IGD. | `PatientBillingSummaryService.Breakdown.cs` secara otomatis membaca `BilInvoiceEncounterLink` yang terbentuk dan menggabungkan item tindakan IGD ke dalam estimasi tagihan ranap. | ✅ **LULUS** |

---

## 3. Invariant & Keselamatan Sistem

1. **Satu Tautan per Pasangan Kunjungan (`BKC-DES-077`):**
   Dijamin oleh pemeriksaan in-memory `linkedEncounterIds` serta dilindungi indeks unik basis data `UX_BilInvoiceEncounterLink_Invoice_Encounter`.
2. **Tautan Tidak Pernah Lintas Pasien (`BKC-DES-079`):**
   Pemeriksaan ketat `originEncounter.PatientId != ranapEncounter.PatientId` mencegah salah kait tagihan antar pasien yang berbeda.
3. **Ketahanan Pembukaan Invoice Rawat Inap (*Fail-Open Admission*):**
   Kegagalan atau ketidakvalidan kunjungan asal transfer tidak pernah menggagalkan event admisi rawat inap; invoice ranap tetap berhasil terbuka dan pasien dapat segera dilayani secara klinis.
4. **Kepatuhan Privasi Data Rekam Medis:**
   Payload log peringatan `InpatientEvent.TransferLinkSkipped` bersih dari informasi pengenal pribadi (PII), hanya memuat identifier UUID teknis dan kode alasan.

