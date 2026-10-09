# PLAN-REPAIR-004 — Membereskan Dukungan Penjamin Perusahaan pada Peresepan Dokter Rawat Inap dan Koreksi Label Penjamin

```yaml
plan_id: PLAN-REPAIR-DOK-004
issue: ../issue/issue-004-tipe-pembayaran-encounter-resep.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-06"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-06"
basis_source_backend: "f2c48e5b251f80e7f3963bfd02b720e3ad8fb978 (MHamzah)"
basis_source_frontend: "b010ffb9722f236160477c95c931c22467550200 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

Rencana ini menutup dua butir laporan masalah dan dua temuan tambahan pada [ISSUE-004](../issue/issue-004-tipe-pembayaran-encounter-resep.md). Seluruh 4 butir perbaikan telah selesai diimplementasikan pada kode backend dan frontend sesuai keputusan pemilik sistem (K-01 Opsi A dan K-02 Opsi A).

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-DOK-004-01` | `ISS-DOK-004-01` | Dukung `EncounterPaymentType.CompanyGuarantor` pada `EncounterInsuranceService.cs` | BE | 1 | — | ✅ SELESAI | Implementasi eager loading dan validasi kontrak korporat di `EncounterInsuranceService.cs` |
| `FIX-DOK-004-02` | `ISS-DOK-004-T2` | Integrasikan mesin coverage `CompanyGuarantorCoverageService` ke resolusi obat peresepan | BE | 1 | — | ✅ SELESAI | Integrasi di `InsuranceCoverageService.cs` & `PrescribingDrugController.cs` |
| `FIX-DOK-004-03` | `ISS-DOK-004-T1` | Hapus fallback hardcoded `"BPJS Kesehatan"` pada header profil dokter rawat inap | FE | 2 | — | ✅ SELESAI | Pembacaan berjenjang penjamin riil & fallback `"-"` di `inpatient-physician-context-header.jsx` |
| `FIX-DOK-004-04` | `ISS-DOK-004-02` | Format badge penjamin perusahaan di panel resep agar informatif | FE | 2 | — | ✅ SELESAI | Pemformatan `Penjamin: [Perusahaan]` di `inpatient-prescription-builder-utils.js` |

**Ringkasan: 4 dari 4 perbaikan selesai (seluruh implementasi kode tuntas).**

Kolom **Task ID** akan diisi ketika task backend dan frontend resmi didaftarkan ke roadmap terkait (`roadmap/backend-roadmap.md` dan `roadmap/frontend-roadmap-v2.md`).

---

## 2. Solusi Terpilih per Temuan

### 2.1 `ISS-DOK-004-01` — Galat "Tipe pembayaran encounter tidak didukung" di `EncounterInsuranceService`

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Perluas `EncounterInsuranceService.GetContextAsync` untuk memproses `EncounterPaymentType.CompanyGuarantor`, membaca relasi `PaymentSource.CompanyGuarantor` dan `PaymentSource.PatientCompanyGuarantor`, serta menandai `IsValid = true` dan `HasInsurance = false` (atau `HasCompanyGuarantor = true`) | Bersih, menyelesaikan akar masalah penolakan HTTP 400 di seluruh pemanggil `GetContextAsync` | Memerlukan mapping DTO konteks yang tepat |
| B | Di `PrescribingDrugController`, lewati validasi `GetContextAsync` jika tipe encounter bukan Cash atau Insurance | Cepat | Memotong validasi di satu controller saja, controller lain (`PatientProcedureController`, dll.) tetap akan gagal |
| C | Ubah tipe pembayaran di registrasi menjadi Insurance | Mengubah integritas data | Salah besar; melanggar aturan integritas entitas Quilvian |

**Solusi terpilih: Opsi A.**
1. Memenuhi prinsip konstitusi: jangan menyembunyikan atau mengubah data transaksi.
2. Memulihkan fungsionalitas seluruh modul klinis yang bergantung pada `EncounterInsuranceService` saat menangani pasien penjamin perusahaan.

---

### 2.2 `ISS-DOK-004-T2` — Resolusi Status Pertanggungan Obat untuk Penjamin Perusahaan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Di `InsuranceCoverageService.ResolveDrugAsync`, deteksi jika encounter bertipe `CompanyGuarantor`, lalu delegasikan evaluasi aturan ke `CompanyGuarantorCoverageService` yang sudah ada di modul Billing (`BE-BKC-044`) | Konsisten dengan mesin kalkulasi tagihan kasir, memakai aturan `MstCompanyGuarantorCoverageRule` yang sudah teruji | Memerlukan injeksi service dan penyelarasan DTO hasil |
| B | Di `InsuranceCoverageService.ResolveDrugAsync`, jika `CompanyGuarantor`, kembalikan tarif normal RS dengan catatan dijamin perusahaan (mirip perlakuan cash advisory) | Sangat cepat diimplementasikan, aman dari risiko crash | Informasi plafon atau persentase aturan perusahaan belum tampil di preview resep |

**Solusi terpilih: Opsi A (dengan fallback ke tarif RS bila aturan belum dibuat).**
1. Memakai ulang logika yang sudah ada (*reuse existing capabilities*) sesuai piagam arsitektur Quilvian.
2. Memberikan kepastian transparansi coverage obat bagi dokter dan pasien korporat.

---

### 2.3 `ISS-DOK-004-T1` — Fallback Hardcoded "BPJS Kesehatan" di Header Dokter Rawat Inap

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Ganti fallback teks `"BPJS Kesehatan"` di `inpatient-physician-context-header.jsx` dengan pembacaan berjenjang dari `patient?.payerName`, `patient?.primaryGuarantorNameSnapshot`, `episode?.payerName`, dan fallback netral `"-"` | Tidak pernah menampilkan nama penjamin palsu, akurat 100% | Jika semua sumber kosong, tampil tanda strip `"-"` |
| B | Tetap gunakan default jika kosong | Mempertahankan tampilan terisi | Sangat menyesatkan petugas medis dan manajemen RS |

**Solusi terpilih: Opsi A.**
1. Sesuai dengan aturan integritas data: sistem tidak boleh mengarang penjamin pasien.
2. Menghilangkan kebingungan dokter dan petugas farmasi.

---

### 2.4 `ISS-DOK-004-02` — Badge Status Penjamin di Banner Resep Obat

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Di `inpatient-prescription-builder-utils.jsx:describePayerBadge`, dukung tipe `CompanyGuarantor` dengan menampilkan badge berformat: `Penjamin Perusahaan — [Nama Perusahaan]` (contoh: `Penjamin Perusahaan — PT Telkom Indonesia`) | Dokter langsung tahu identitas penjamin saat menyusun draf resep | Perlu penyesuaian styling jika teks perusahaan panjang |
| B | Biarkan menampilkan nama penjamin tanpa label jenis | Singkat | Dokter tidak membedakan apakah itu asuransi swasta atau korporat |

**Solusi terpilih: Opsi A.**
1. Memperjelas hak tanggungan pasien pada formulir peresepan rawat inap.

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Skema Rangka Tampilan

```text
====================================================================================================
SEBELUM (Keadaan Saat Ini — Cacat Blocker):
====================================================================================================
+--------------------------------------------------------------------------------------------------+
| DOKTER PENANGGUNG JAWAB       NO. RM PASIEN    NAMA PASIEN       PENJAMIN          LAMA RAWAT    |
| dr. Rendy Pangalila           00-00-00-19      Hendro Wibowo     BPJS Kesehatan    1 hari rawat  | <-- PALSU!
| RI-261006042632-C0303 - Ruang Rawat Inap Kelas II 1 - Bed BED 001 - KELAS II                     |
+--------------------------------------------------------------------------------------------------+
| [ Buat Resep 0 ]   [ Template Resep 2 ]   [ History Resep 0 ]   [ Resep Harian 0 ]               |
+--------------------------------------------------------------------------------------------------+
| Resep Obat                                       [ Gunakan Template ]  ( Penjamin belum terbaca )| <-- CACAT!
+--------------------------------------------------------------------------------------------------+
| Jenis Resep: [ Harian        ▾]     Catatan Dokter Pengait (SOAP): [ Pilih catatan dokter     ▾] |
+--------------------------------------------------------------------------------------------------+
| (•) Resep                   ( ) Obat Racikan                 ( ) Rekonsiliasi Obat               |
+--------------------------------------------------------------------------------------------------+
| Daftar Obat                                            | Nama Obat                               |
| [ Cari obat berdasarkan nama, kode, atau kandungan... ]| [ Pilih obat dari daftar              ] |
| Halaman 1 • Total 0 obat                 (0 Ditampilkan)|                                         |
| +----------------------------------------------------+ | Jumlah Obat               Signa *       |
| | ⚠ Tipe pembayaran encounter tidak didukung.        | | [ Jumlah obat      ]      [        ] x  |
| |                                      [ Coba Lagi ] | |                                         |
| +----------------------------------------------------+ | Signa Tambahan (opsional)               |
+--------------------------------------------------------------------------------------------------+

====================================================================================================
SESUDAH (Hasil Perbaikan):
====================================================================================================
+--------------------------------------------------------------------------------------------------+
| DOKTER PENANGGUNG JAWAB       NO. RM PASIEN    NAMA PASIEN       PENJAMIN          LAMA RAWAT    |
| dr. Rendy Pangalila           00-00-00-19      Hendro Wibowo     PT Telkom Ind...  1 hari rawat  | <-- AKURAT!
| RI-261006042632-C0303 - Ruang Rawat Inap Kelas II 1 - Bed BED 001 - KELAS II                     |
+--------------------------------------------------------------------------------------------------+
| [ Buat Resep 0 ]   [ Template Resep 2 ]   [ History Resep 0 ]   [ Resep Harian 0 ]               |
+--------------------------------------------------------------------------------------------------+
| Resep Obat                                       [ Gunakan Template ]  ( Perusahaan: PT Telkom ) | <-- JELAS!
+--------------------------------------------------------------------------------------------------+
| Jenis Resep: [ Harian        ▾]     Catatan Dokter Pengait (SOAP): [ Pilih catatan dokter     ▾] |
+--------------------------------------------------------------------------------------------------+
| (•) Resep                   ( ) Obat Racikan                 ( ) Rekonsiliasi Obat               |
+--------------------------------------------------------------------------------------------------+
| Daftar Obat                                            | Nama Obat                               |
| [ Cari obat berdasarkan nama, kode, atau kandungan... ]| [ Pilih obat dari daftar              ] |
| Halaman 1 • Total 25 obat               (25 Ditampilkan)|                                         |
| +----------------------------------------------------+ | Jumlah Obat               Signa *       |
| | Paracetamol 500 mg Tablet                          | | [ Jumlah obat      ]      [        ] x  |
| | OBT-001 • Generik • Rp 5.000 (Ditanggung Perusahaan)| |                                         |
| | Amoxicillin 500 mg Kapsul                          | | Signa Tambahan (opsional)               |
| | OBT-002 • Antibiotik • Rp 8.000 (Ditanggung PT Tel)| |                                         |
| +----------------------------------------------------+ | [ Tambah ke Resep ]                     |
+--------------------------------------------------------------------------------------------------+
```

### 3.2 Wilayah Tampilan

| Wilayah | Isi Tampilan | Sumber Data | Komponen |
| --- | --- | --- | --- |
| Header Profil Pasien | No RM, Nama Pasien, Penjamin riil (`PT Telkom Indonesia`), Lama Rawat | Sensus Rawat Inap / Detail Episode | `InpatientPhysicianContextHeader` |
| Banner Resep Obat | Judul "Resep Obat", tombol "Gunakan Template", chip penjamin (`Perusahaan: PT Telkom Indonesia`) | Respon `PrescribingDrugPagedResponse` via `payerContext` | `PrescriptionBuilderPanel` |
| Katalog Daftar Obat | Input pencarian obat, daftar obat aktif, info formularium & status tanggungan perusahaan | Endpoint `GET /api/v1/PrescribingDrug?encounterId=...` | `PrescriptionBuilderPanel` |

### 3.3 Tombol Tampilan

| Tombol | Jenis | Kapan Aktif | Yang Terjadi Saat Diklik |
| --- | --- | --- | --- |
| `[ Gunakan Template ]` | Sekunder | Selalu aktif bila dokter memiliki hak tulis | Membuka daftar template resep pribadi dokter |
| `[ Tambah ke Resep ]` | Primer | Ketika obat telah dipilih, jumlah dan signa terisi valid | Memasukkan item obat ke dalam daftar draf resep |

---

## 4. Rincian Perbaikan

### FIX-DOK-004-01 — Dukung `EncounterPaymentType.CompanyGuarantor` pada `EncounterInsuranceService.cs`

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-004-01` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs` |
| **Radius dampak** | `PrescribingDrugController`, `PatientProcedureController`, `InsuranceCoverageService`, `MedicationReconciliationService` |
| **Bergantung pada** | Tidak ada |

**Langkah:**
1. Tambahkan eager-loading `.Include(x => x.PaymentSource).ThenInclude(x => x!.CompanyGuarantor)` dan `.ThenInclude(x => x!.PatientCompanyGuarantor)` pada kueri `GetContextAsync`.
2. Pada pengecekan `PaymentType`, tambahkan blok penanganan untuk `encounter.PaymentType == EncounterPaymentType.CompanyGuarantor`:
   - Validasi bahwa `paymentSource.PaymentType == EncounterPaymentType.CompanyGuarantor` dan `paymentSource.CompanyGuarantorId.HasValue`.
   - Validasi masa berlaku kontrak kerja sama perusahaan (`ContractStartDate` dan `ContractEndDate`).
   - Bangun dan kembalikan objek `EncounterInsuranceContext` yang valid dengan properti:
     - `IsValid = true`
     - `PaymentType = EncounterPaymentType.CompanyGuarantor`
     - `PaymentTypeName = "Penjamin Perusahaan"`
     - `PaymentSourceName = paymentSource.PaymentSourceNameSnapshot ?? companyGuarantor.CompanyGuarantorName`
     - `BenefitPlanCode = paymentSource.BenefitPlanCodeSnapshot`
     - `HasInsurance = false` (bukan asuransi pihak ketiga, melainkan korporat)
     - `IsInsuranceReady = true`
3. Pertahankan penolakan hanya jika `PaymentType` di luar `Cash`, `Insurance`, dan `CompanyGuarantor`.

**Acceptance criteria:**
1. Pemanggilan `GetContextAsync` untuk encounter `951dbdea-415f-4592-8e62-03d1171eb4c4` (pasien Hendro Wibowo, `PaymentType = 3`) mengembalikan objek dengan `IsValid = true` dan `PaymentTypeName = "Penjamin Perusahaan"`.
2. Endpoint `GET /api/v1/PrescribingDrug?encounterId=951dbdea-415f-4592-8e62-03d1171eb4c4` mengembalikan HTTP 200 OK dengan daftar obat formularium, bukan HTTP 400 Bad Request.

**Verifikasi:**
- Uji pemanggilan API `GET /api/v1/PrescribingDrug` dengan `encounterId` terkait menggunakan script verifikasi backend.

**Risiko:**
- Pemanggil existing yang mengecek `HasInsurance == true` perlu dipastikan perilakunya tetap konsisten atau membaca `PaymentType`.

---

### FIX-DOK-004-02 — Integrasikan Mesin Tanggungan Perusahaan ke Resolusi Obat

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-004-T2` |
| **Area** | Backend |
| **Jenis perubahan** | Source backend |
| **Berkas yang diubah** | `NewQuilvianSystemBackend/Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs` |
| **Radius dampak** | Seluruh perhitungan coverage obat peresepan |
| **Bergantung pada** | `FIX-DOK-004-01`, keputusan `K-01` |

**Langkah:**
1. Injeksi `CompanyGuarantorCoverageService` ke `InsuranceCoverageService` via Constructor Dependency Injection.
2. Pada method `ResolveTariffInternalAsync`, tambahkan percabangan:
   - Jika `context.PaymentType == EncounterPaymentType.CompanyGuarantor`:
     - Panggil `CompanyGuarantorCoverageService.ResolveTariffAsync` untuk mendapatkan evaluasi aturan `MstCompanyGuarantorCoverageRule`.
     - Petakan hasil `CompanyGuarantorCoverageResult` ke dalam `InsuranceCoverageResult` (mencakup harga satuan, porsi ditanggung, dan catatan aturan).
     - Jika aturan pertanggungan perusahaan belum dikonfigurasi, gunakan fallback aman ke tarif normal rumah sakit dengan keterangan `"Ditanggung sesuai kebijakan perusahaan"`.

**Acceptance criteria:**
1. Item obat yang di-resolve untuk pasien penjamin perusahaan tidak melempar exception dan menampilkan nominal tarif rumah sakit serta status coverage yang valid.

**Verifikasi:**
- Eksekusi unit test peresepan obat atau validasi script `ResolveDrugAsync`.

**Risiko:**
- Circular dependency antara namespace service (dapat dicegah karena keduanya berada di bawah namespace `QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services`).

---

### FIX-DOK-004-03 — Perbaiki Label Penjamin pada Header Profil Pasien Dokter Rawat Inap

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-004-T1` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/doctor-inpatient/inpatient-physician-context-header.jsx` |
| **Radius dampak** | Tampilan header lembar kerja dokter rawat inap |
| **Bergantung pada** | Keputusan `K-02` |

**Langkah:**
1. Di `inpatient-physician-context-header.jsx:150-154`, ubah penentuan `payerName`:
```javascript
const payerName =
  patient?.payerName ||
  patient?.primaryGuarantorNameSnapshot ||
  workspaceContext?.episode?.payerName ||
  workspaceContext?.episode?.insuranceName ||
  "-";
```
2. Pastikan nilai `"BPJS Kesehatan"` statis dihapus sepenuhnya. Jika penjamin belum terbaca dari semua properti di atas, gunakan tanda strip `"-"`.

**Acceptance criteria:**
1. Pada pasien Tn. Hendro Wibowo (`00-00-00-19`), header profil dokter rawat inap menampilkan penjamin riil dari sensus (`"PT Telkom Indonesia"`), bukan `"BPJS Kesehatan"`.
2. Jika pasien fiktif tanpa data penjamin dibuka, label menampilkan `"-"`, bukan nama asuransi mana pun.

**Verifikasi:**
- Periksa tampilan header di browser pada pasien terkait.

**Risiko:**
- Sangat rendah, hanya menyentuh fallback teks pada header presentasi.

---

### FIX-DOK-004-04 — Format Badge Penjamin Perusahaan di Panel Resep

| | |
| --- | --- |
| **Menutup** | `ISS-DOK-004-02` |
| **Area** | Frontend |
| **Jenis perubahan** | Source frontend |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-prescription-builder-utils.jsx` |
| **Radius dampak** | Badge penjamin di panel Buat Resep dokter rawat inap |
| **Bergantung pada** | `FIX-DOK-004-01` |

**Langkah:**
1. Pada fungsi `describePayerBadge(payerContext)`, tambahkan pembacaan untuk `PaymentType` bertipe Penjamin Perusahaan:
   - Jika `payerContext.paymentTypeName === "Penjamin Perusahaan"` atau terdapat `companyGuarantorName`:
   - Kembalikan label berformat: `Penjamin: ${payerContext.insuranceProviderName || payerContext.paymentSourceName || "Perusahaan"}`.
2. Pastikan jika data berhasil dimuat, badge tidak lagi menampilkan *"Penjamin belum terbaca"*.

**Acceptance criteria:**
1. Ketika katalog obat berhasil dimuat untuk pasien Hendro Wibowo, badge di kanan atas Resep Obat menampilkan teks `"Penjamin: PT Telkom Indonesia"`.

**Verifikasi:**
- Periksa antarmuka Buat Resep rawat inap setelah backend mengembalikan data penjamin.

**Risiko:**
- Sangat rendah, murni pemformatan label string presentasi.

---

## 5. Urutan Pengerjaan

```text
FIX-DOK-004-01 (Backend: dukung CompanyGuarantor pada EncounterInsuranceService)
├── FIX-DOK-004-02 (Backend: integrasi CompanyGuarantorCoverageService ke peresepan) [menunggu K-01]
└── FIX-DOK-004-04 (Frontend: perbaiki badge penjamin di panel resep)

K-02 (Keputusan pemilik: format fallback penjamin)
└── FIX-DOK-004-03 (Frontend: hapus fallback hardcoded BPJS Kesehatan di context header)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Penggunaan mesin aturan `CompanyGuarantorCoverageService` untuk advisory obat peresepan dokter | (A) Sambungkan ke aturan `MstCompanyGuarantorCoverageRule`<br>(B) Hanya tampilkan tarif normal RS | **Pilihan A** (mengikuti pola resmi Billing Kasir `BE-BKC-044`) | `FIX-DOK-004-02` |
| K-02 | Teks pengganti fallback jika penjamin tidak ditemukan di DTO episode | (A) Tanda strip `"-"`<br>(B) Teks `"Umum / Tidak Ada Penjamin"` | **Pilihan A** | `FIX-DOK-004-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

| Dokumen | Bagian | Sekarang | Menjadi |
| --- | --- | --- | --- |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/02-backend-architecture.md` | Bagian integrasi peresepan & penjamin | `EncounterInsuranceService` hanya menangani `Cash` dan `Insurance` | Menyatakan dukungan penuh untuk 3 tipe pembayaran: `Cash`, `Insurance`, dan `CompanyGuarantor` |
| `docs/module-blueprints/rawat-inap/dokter-rawat-inap/skema-tampilan-dokter-rawat-inap.md` | Bagian Header & Resep Obat | Tidak menjelaskan penjamin perusahaan | Menegaskan format badge untuk penjamin korporat/perusahaan |

---

## 8. Verifikasi Menyeluruh

Setelah keempat perbaikan selesai dikerjakan:
1. **Langkah 1 (Backend Verification):** Panggil endpoint `GET /api/v1/PrescribingDrug?encounterId=951dbdea-415f-4592-8e62-03d1171eb4c4` dengan token otorisasi dokter. Pastikan status respons adalah **200 OK**, `items` berisi daftar obat, dan metadata mengembalikan `paymentTypeName = "Penjamin Perusahaan"`.
2. **Langkah 2 (Frontend Visual Verification):** Buka URL lembar kerja dokter rawat inap untuk pasien Hendro Wibowo (`RI-261006042632-C0303`).
   - Periksa header atas: Penjamin harus menampilkan `"PT Telkom Indonesia"`.
   - Buka tab **Resep**: Badge di banner resep harus menampilkan `"Penjamin: PT Telkom Indonesia"`.
   - Periksa tabel katalog obat: Daftar obat formularium harus tampil lengkap (misal: 25 obat per halaman), kolom pencarian dapat digunakan, dan kotak galat *"Tipe pembayaran encounter tidak didukung"* hilang sepenuhnya.
3. **Langkah 3 (Regression Test):** Buka pasien rawat inap lain yang bertipe `Cash` (Tunai) dan `Insurance` (BPJS Kesehatan). Pastikan katalog obat dan header tetap berfungsi normal tanpa regresi.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-06 | Rencana perbaikan dibuat mencakup 4 butir perbaikan (2 Backend, 2 Frontend) | `diagnose-module-issue` |
| 2026-10-06 | Rencana disetujui penuh oleh pemilik sistem; K-01 Opsi A & K-02 Opsi A dipilih | Pemilik Sistem (User) |
| 2026-10-06 | Seluruh 4 implementasi kode selesai dikerjakan (FIX-DOK-004-01 s/d 04); status diubah ke SELESAI | Antigravity AI |
