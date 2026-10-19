# Laporan Perubahan Backend — `BE-BKC-083`

## Ringkasan untuk Pembaca Umum

Dalam pengelolaan keuangan rumah sakit, pasien rawat jalan (Poliklinik) yang dirujuk ke rawat inap (Ranap) pada hari yang sama berhak mendapatkan penggantian biaya administrasi poliklinik agar pasien tidak terbebani biaya administrasi ganda (`BKC-DEC-119`). Selain itu, jika terdapat tagihan poliklinik yang sebelumnya sudah dibayar atau difinalisasi, kelebihan biaya administrasi tersebut dikreditkan pada tagihan rawat inap, atau diubah menjadi kredit pengembalian dana (*refundable credit*) saat tagihan rawat inap difinalisasi jika kredit tersebut melebihi porsi yang harus dibayar pasien (`BKC-DEC-164`).

Di sisi lain, untuk menyelesaikan temuan audit finansial di mana item tindakan medis sempat dibatalkan (*void*) secara keliru dengan alasan administrasi teralihkan rawat inap, sistem memerlukan mekanisme tata kelola bagi Supervisor Kasir untuk meninjau dan memutuskan apakah item tersebut harus dipulihkan (*reinstate*), dipertahankan batal (*keep*), atau ditandai membutuhkan koreksi pasca-tutup (*needs post-close correction*) (`BKC-DEC-158`, `BKC-DEC-162`).

Sebelum task `BE-BKC-083`:
1. **Basis Data Belum Siap Menerima Kredit Administrasi Rawat Jalan:**
   Tabel `BilCalculationVersion` belum memiliki kolom penyimpanan nominal kredit administrasi poli dan sisa kredit yang belum terpakai, serta batasan (*check constraint*) belum menyertakan kedua kolom baru tersebut.
2. **Constraint Sumber Kredit Pengembalian Dana Menolak Jenis Administrasi:**
   Tabel `BilRefundableCredit` memiliki check constraint `CK_BilRefundableCredit_SourceType` yang hanya menerima `'ALLOCATION_EXCESS'` dan `'SETTLEMENT'`. Upaya pembuatan kredit bertipe `'REFERRED_OUTPATIENT_ADMIN'` ditolak langsung oleh basis data PostgreSQL (error validasi integritas).
3. **Ketiadaan Tabel Tata Kelola Peninjauan Item Batal Keliru:**
   Belum ada tabel dan model untuk mencatat keputusan resmi Supervisor Kasir saat meninjau pembatalan item tagihan yang tidak tepat.

### Solusi Penyempurnaan `BE-BKC-083`

Melalui implementasi task `BE-BKC-083` (arsitektur `BKC-DES-085`, `BKC-DES-086`, `BKC-DES-089`, kontrak `BIL-STATE-2.0`):
1. **Penyempurnaan Model dan Konfigurasi `BilCalculationVersion` (`BKC-DES-085`):**
   - Menambahkan properti `ReferredOutpatientAdminCreditAmount` dan `ReferredOutpatientAdminUnappliedCreditAmount` bertipe `numeric(18,2)` dengan nilai bawaan `0`.
   - Memperluas check constraint `CK_BilCalculationVersion_Amounts` pada `BilCalculationVersionConfiguration.cs` untuk memastikan kedua kolom selalu bernilai non-negatif (`>= 0`).
2. **Perluasan Sumber Kredit `BilRefundableCredit` (`BKC-DES-086`):**
   - Memperluas check constraint `CK_BilRefundableCredit_SourceType` pada `BilRefundableCreditConfiguration.cs` sehingga menerima `'ALLOCATION_EXCESS'`, `'SETTLEMENT'`, dan `'REFERRED_OUTPATIENT_ADMIN'`.
3. **Penyediaan Entitas Tata Kelola `BilSupersededItemReview` (`BKC-DES-089`):**
   - Membuat model `BilSupersededItemReview` beserta kelas statis kosakata keputusan `BilSupersededItemReviewDecisions` (`REINSTATE`, `KEEP`, `NEEDS_POST_CLOSE_CORRECTION`).
   - Membuat konfigurasi `BilSupersededItemReviewConfiguration` dengan 3 check constraint ketat (`CK_BilSupersededItemReview_Decision`, `CK_BilSupersededItemReview_ReinstatedItem`, `CK_BilSupersededItemReview_Reason`), 4 indeks performa dan integritas (termasuk indeks unik pada item dan idempotency key), serta relasi FK berstatus `Restrict` ke `BilInvoiceItem` dan `BilInvoice`.
   - Mendaftarkan entitas pada `ApplicationDbContext` sebagai `DbSet<BilSupersededItemReview>`.
4. **Penerbitan Migration Skema Aman Tanpa Downtime:**
   - Menulis migration `20261017031500_AddReferredAdminCreditAndSupersededItemReview.cs` beserta berkas `Designer.cs`-nya.
   - Menggunakan klausa PostgreSQL `ADD CONSTRAINT ... NOT VALID` dilanjutkan `VALIDATE CONSTRAINT` pada pembaruan batasan skema agar penambahan constraint tidak mengunci tabel operasional secara eksklusif dalam durasi lama (*zero downtime safe*).
   - Memutakhirkan `ApplicationDbContextModelSnapshot.cs` secara presisi sesuai kamus data.

---

## Contoh Kasus Nyata di Rumah Sakit

### Skenario 1: Pasien Ny. Siti (Kredit Biaya Administrasi Poli pada Rawat Inap)
1. **Konsultasi Poliklinik:**
   - Ny. Siti berkonsultasi ke Dokter Spesialis Penyakit Dalam di Poliklinik. Biaya pendaftaran dan administrasi poliklinik sebesar Rp 50.000 telah dibayarkan oleh keluarga di kasir poliklinik (invoice poli berstatus `CLOSED`).
2. **Rujukan Masuk Rawat Inap:**
   - Dokter memutuskan Ny. Siti memerlukan rawat inap pada hari yang sama. Pada saat tagihan rawat inap dihitung, sistem mendeteksi bahwa administrasi rawat inap Rp 150.000 telah mencakup administrasi pelayanan hari tersebut.
3. **Pencatatan Versi Kalkulasi:**
   - Sistem mencatat kredit administrasi Rp 50.000 pada tagihan rawat inap. Melalui kolom `ReferredOutpatientAdminCreditAmount` di tabel `BilCalculationVersion`, nilai kredit ini tercatat secara terpisah dan resmi pada rincian perhitungan tagihan ranap, sehingga pasien hanya membayar sisa administrasi rawat inap Rp 100.000 tanpa kebingungan.
4. **Perlindungan Basis Data:**
   - Basis data secara ketat memastikan nilai kredit tidak bernilai negatif melalui batasan `CK_BilCalculationVersion_Amounts`.

### Skenario 2: Supervisor Kasir Memulihkan Tindakan Medis yang Terlanjur Batal
1. **Masalah Operasional:**
   - Pada tagihan rawat jalan sebelumnya, tindakan USG Kandungan sempat dibatalkan secara keliru oleh sistem lama menggunakan alasan *"SUPERSEDED_BY_INPATIENT_ADMISSION"*. Padahal tindakan USG tersebut telah nyata-nyata selesai dikerjakan oleh dokter spesialis.
2. **Peninjauan Supervisor Kasir:**
   - Supervisor Kasir membuka menu peninjauan item batal. Setelah memeriksa bukti pemeriksaan penunjang di berkas rekam medis elektronik, Supervisor memutuskan `REINSTATE` (memulihkan) dengan alasan resmi: *"Tindakan USG telah dilaksanakan oleh dr. A, Sp.OG pada pukul 10.00 dan layak ditagihkan"*.
3. **Pencatatan Audit pada `BilSupersededItemReview`:**
   - Sistem mencatat keputusan ini ke tabel `BilSupersededItemReview`.
   - Constraint `CK_BilSupersededItemReview_ReinstatedItem` menjamin bahwa keputusan `REINSTATE` wajib mencantumkan item tagihan aktif baru sebagai penggantinya (`ReinstatedInvoiceItemId`), sementara keputusan `KEEP` wajib bernilai `NULL`.
   - Constraint `CK_BilSupersededItemReview_Reason` menjamin alasan tidak boleh berupa string kosong.
   - Indeks unik `UX_BilSupersededItemReview_InvoiceItem` menjamin setiap baris tindakan yang batal hanya boleh ditinjau tepat satu kali, mencegah duplikasi tindakan maupun manipulasi tagihan ganda.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-083` |
| Judul | Model, configuration, dan migration skema revisi 2.3 |
| Slice / Gelombang | Gelombang Eksekusi 1 (Revisi Blueprint 2.2 / 2.3) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-083` |
| Trace | `FR-BKC-280`, `FR-BKC-281`, `FR-BKC-284`; `BKC-DEC-158`, `BKC-DEC-162`, `BKC-DEC-164`; `BKC-DES-085`, `BKC-DES-086`, `BKC-DES-089` |
| Contract Version | `BIL-STATE-2.0`, Kamus Data Amendment 2.3 |
| Acceptance Criteria | Kriteria 1–3 pada Roadmap § `BE-BKC-083` (Model snapshot memuat dua kolom, constraint diperluas, tabel baru; Up/Down simetris; eksekusi migrasi diserahkan ke pemilik basis data) |
| Dependency | `{OTORISASI-MIG-1}` |
| Klasifikasi | Tinggi / Backend Engineering + Database Administrator (perubahan skema basis data) |
| Task mode | `BACKEND` |
| Target berkas | `Areas/HealthServices/BillingManagement/Billing/Models/BilCalculationVersion.cs`<br/>`Repositories/Configurations/HealthServices/BillingManagement/Billing/BilCalculationVersionConfiguration.cs`<br/>`Repositories/Configurations/HealthServices/BillingManagement/Billing/BilRefundableCreditConfiguration.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Models/BilSupersededItemReview.cs`<br/>`Repositories/Configurations/HealthServices/BillingManagement/Billing/BilSupersededItemReviewConfiguration.cs`<br/>`Repositories/ApplicationDbContext.cs`<br/>`Migrations/20261017031500_AddReferredAdminCreditAndSupersededItemReview.cs`<br/>`Migrations/20261017031500_AddReferredAdminCreditAndSupersededItemReview.Designer.cs`<br/>`Migrations/ApplicationDbContextModelSnapshot.cs` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Kode skema siap, menunggu eksekusi migrasi mandiri oleh DB Admin)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Entity / Model | `BilCalculationVersion`, `BilRefundableCredit`, `BilSupersededItemReview` |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` & `NEW ENTITY` (Penyempurnaan model perhitungan penagihan + entitas peninjauan item batal) |
| QBE Rules Berlaku | `QBE-MOD-001` (presisi nominal uang 18,2, batasan check constraint non-negatif), `QBE-NAM-001` (pembakuan penamaan entitas dan index), `QBE-SEC-001` (perlindungan integritas data audit dan penghapusan relasi Restrict) |
| Database Migration | 1 Migration baru diterbitkan (`20261017031500_AddReferredAdminCreditAndSupersededItemReview.cs`). **Sesuai instruksi dan batas keselamatan, migrasi database TIDAK dijalankan otomatis** (`dotnet ef database update` atau `Update-Database` diserahkan kepada DBA/pemilik basis data) |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah / Ditambahkan

#### 1. [`BilCalculationVersion.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Models/BilCalculationVersion.cs)
- Menambahkan dua properti baru:
  ```csharp
  public decimal ReferredOutpatientAdminCreditAmount { get; set; } = 0m;
  public decimal ReferredOutpatientAdminUnappliedCreditAmount { get; set; } = 0m;
  ```

#### 2. [`BilCalculationVersionConfiguration.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Repositories/Configurations/HealthServices/BillingManagement/Billing/BilCalculationVersionConfiguration.cs)
- Menambahkan konfigurasi presisi dan nilai bawaan:
  ```csharp
  entity.Property(x => x.ReferredOutpatientAdminCreditAmount).HasPrecision(18, 2).HasDefaultValue(0m);
  entity.Property(x => x.ReferredOutpatientAdminUnappliedCreditAmount).HasPrecision(18, 2).HasDefaultValue(0m);
  ```
- Memperluas check constraint `CK_BilCalculationVersion_Amounts` menyertakan:
  `AND "ReferredOutpatientAdminCreditAmount" >= 0 AND "ReferredOutpatientAdminUnappliedCreditAmount" >= 0`

#### 3. [`BilRefundableCreditConfiguration.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Repositories/Configurations/HealthServices/BillingManagement/Billing/BilRefundableCreditConfiguration.cs)
- Memperluas check constraint `CK_BilRefundableCredit_SourceType`:
  ```csharp
  table.HasCheckConstraint(
      "CK_BilRefundableCredit_SourceType",
      "\"SourceType\" IN ('ALLOCATION_EXCESS','SETTLEMENT','REFERRED_OUTPATIENT_ADMIN')");
  ```

#### 4. [`BilSupersededItemReview.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Models/BilSupersededItemReview.cs)
- Membuat kelas kosakata keputusan `BilSupersededItemReviewDecisions`:
  `REINSTATE`, `KEEP`, `NEEDS_POST_CLOSE_CORRECTION`.
- Membuat entitas append-only `BilSupersededItemReview` turunan dari `IdentityModel`.

#### 5. [`BilSupersededItemReviewConfiguration.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Repositories/Configurations/HealthServices/BillingManagement/Billing/BilSupersededItemReviewConfiguration.cs)
- Mengonfigurasi tabel `BilSupersededItemReview` pada schema `public`.
- Menambahkan 3 check constraint:
  1. `CK_BilSupersededItemReview_Decision`: membatasi keputusan pada 3 kosakata sah.
  2. `CK_BilSupersededItemReview_ReinstatedItem`: memastikan konsistensi ID item pemulihan terhadap keputusan `REINSTATE`.
  3. `CK_BilSupersededItemReview_Reason`: memastikan alasan peninjauan tidak kosong (`length(trim("Reason")) > 0`).
- Menambahkan 4 indeks:
  1. `UX_BilSupersededItemReview_InvoiceItem` (Unik dengan filter `NOT "IsDelete"`)
  2. `UX_BilSupersededItemReview_IdempotencyKey` (Unik)
  3. `IX_BilSupersededItemReview_InvoiceId`
  4. `IX_BilSupersededItemReview_Decision`
- Mengonfigurasi relasi foreign key `Restrict` ke `BilInvoiceItem` (item asal), `BilInvoice` (tagihan pemilik), dan `BilInvoiceItem` (item hasil pemulihan).

#### 6. [`ApplicationDbContext.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs)
- Mendaftarkan entitas:
  ```csharp
  public DbSet<BilSupersededItemReview> BilSupersededItemReviews { get; set; }
  ```

#### 7. Migration Files & Snapshot
- [`20261017031500_AddReferredAdminCreditAndSupersededItemReview.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Migrations/20261017031500_AddReferredAdminCreditAndSupersededItemReview.cs): Migration file dengan metode `Up` dan `Down`.
- [`20261017031500_AddReferredAdminCreditAndSupersededItemReview.Designer.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Migrations/20261017031500_AddReferredAdminCreditAndSupersededItemReview.Designer.cs): Metadata perancangan migrasi EF Core.
- [`ApplicationDbContextModelSnapshot.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Migrations/ApplicationDbContextModelSnapshot.cs): Model snapshot yang telah diperbarui dengan kolom, constraint, dan entitas baru.

---

## 2. Pemenuhan Acceptance Criteria

| ID Kriteria | Deskripsi Kriteria | Status | Bukti Verifikasi |
| :--- | :--- | :---: | :--- |
| **AC-01** | Model snapshot memuat dua kolom versi kalkulasi, constraint yang diperluas, dan tabel baru persis seperti kamus data | ✅ **TERPENUHI** | `ApplicationDbContextModelSnapshot.cs` telah memuat properti `ReferredOutpatientAdminCreditAmount` dan `ReferredOutpatientAdminUnappliedCreditAmount` dengan presisi `(18,2)` bawaan `0`, constraint `CK_BilCalculationVersion_Amounts` dan `CK_BilRefundableCredit_SourceType` diperbarui, serta blok entitas `BilSupersededItemReview` terdaftar lengkap. |
| **AC-02** | `Up()` hanya memuat perubahan cakupan 1–3; `Down()` mengembalikannya dan mendokumentasikan kegagalan bila sudah ada kredit `REFERRED_OUTPATIENT_ADMIN` | ✅ **TERPENUHI** | `Up()` menambahkan kolom, memperluas constraint via `NOT VALID` lalu `VALIDATE CONSTRAINT`, dan membuat tabel `BilSupersededItemReview`. `Down()` mengembalikan tabel, mengembalikan kedua constraint, dan menghapus kolom. Bila constraint dikembalikan saat kredit telah ada, PostgreSQL menolak rollback demi integritas audit finansial. |
| **AC-03** | Migration **tidak** diterapkan oleh builder otomatis; pemilik menjalankan migrasi sendiri | ✅ **TERPENUHI** | Tidak ada perintah `dotnet ef database update` atau `Update-Database` yang dijalankan dalam sesi ini. Seluruh berkas migrasi telah dikomit siap jalan untuk DBA/pemilik sistem. |

---

## 3. Petunjuk Operasional untuk Database Administrator (DBA)

Untuk menerapkan perubahan skema ini pada basis data PostgreSQL Staging / Production:

1. **Pemeriksaan Awal (Pre-check):**
   Pastikan tidak ada transaksi berstatus hanging pada tabel `BilCalculationVersion` atau `BilRefundableCredit`.
2. **Eksekusi Migrasi:**
   Jalankan perintah standar Entity Framework Core melalui terminal di folder backend:
   ```bash
   dotnet ef database update 20261017031500_AddReferredAdminCreditAndSupersededItemReview
   ```
   Atau jalankan skrip SQL DDL secara mandiri jika menggunakan pipeline database terpisah.
3. **Verifikasi Skema Pasca-Migrasi:**
   Pastikan tabel `BilSupersededItemReview` terbentuk dan kedua kolom baru muncul di `BilCalculationVersion`:
   ```sql
   SELECT column_name, data_type, column_default 
   FROM information_schema.columns 
   WHERE table_name = 'BilCalculationVersion' 
     AND column_name IN ('ReferredOutpatientAdminCreditAmount', 'ReferredOutpatientAdminUnappliedCreditAmount');
   ```

