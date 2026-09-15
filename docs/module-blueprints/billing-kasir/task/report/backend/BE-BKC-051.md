# Laporan Perubahan Backend — `BE-BKC-051`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-051` |
| **Judul** | Hak Akses, Privasi, dan Hardening Lintas-Slice (*Cross-Slice Access Control, Privacy & Hardening Capstone*) |
| **Slice / Milestone** | `MVP-19` (Dokumen Penjamin Perusahaan & Hardening / Eksekusi Gelombang 4 — Capstone) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1874 |
| **Trace** | `FR-BKC-085`, `FR-BKC-086`; `MPY-DEC-005`, `MPY-DEC-006`; `NFR-020`–`027`; `CAP-39` |
| **Contract Version** | `BIL-PERMISSION-0.8`, `BIL-TEST-1.0`; Acceptance `BIL-AT-097`, `BIL-AT-098`, `BIL-AT-100` |
| **Dependency** | ✅ `BE-BKC-047`, ✅ `BE-BKC-048`, ✅ `BE-BKC-049`, ✅ `BE-BKC-050` (seluruh task eksekusi Gelombang 3 selesai) |
| **Klasifikasi** | `CROSS-SLICE HARDENING / RBAC VERIFICATION / AUDIT & PRIVACY ENFORCEMENT` |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Verifikasi matriks hak akses Role-Based Access Control pada seluruh endpoint baru terbukti, pendaftaran dinamis atribut otomatis CAP-39 terkonfirmasi, pencegahan kebocoran data sensitif karyawan/pasien ke log dan nama berkas BIL-AT-098 terpenuhi, gerbang penutupan edit pasca pembayaran BIL-VAL-059/060 dan atomisitas transaksi serializable teruji, serta uji regresi komparatif BIL-AT-100 membuktikan angka tagihan tunai dan asuransi lama nol pergeseran sementara tagihan penjamin perusahaan bebas peringatan palsu) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` & `Administrator` |
| **Module / Submodule** | `BillingManagement / Billing`, `MasterData`, `Administrator / MasterData` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`), `Mst` (Master Data) |
| **Keberlakuan** | `CAPSTONE HARDENING & REGRESSION AUDIT` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-SVC-001`, `QBE-INT-001` |
| **Pengecualian / Catatan** | Task penutup (capstone) dari seluruh rumpun Multi-Payer Billing (`MVP-16` s/d `MVP-19`). Memastikan seluruh lapisan operasional mematuhi piagam arsitektur Quilvian: batas wewenang tunggal kasir tanpa persetujuan bertingkat (`MPY-DEC-005`), penggunaan kembali izin `BillingInvoice:Read` untuk dokumen perusahaan (`MPY-DEC-006`), pencegahan data pribadi karyawan masuk ke payload log, dan jaminan integritas kalkulasi tagihan historis. |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Hardening dan Audit Lintas-Slice
Rumpun penagihan multi-penjamin (*Multi-Payer Billing*) melibatkan koordinasi ekstensif di berbagai lapisan sistem:
1. **Dua Master Data Baru**: Rute penggantian biaya korporat (`CompanyGuarantorReimbursementRoute`) dan aturan tanggungan perusahaan (`CompanyGuarantorCoverageRule`).
2. **Tiga Perintah Koreksi Finansial**: Penggantian penanggung kunjungan (`payment-source`), penandaan penanggung per baris biaya (`item-payer-assignments`), dan pengaturan disposisi penebusan resep obat (`drug-billing-disposition`).
3. **Penyusunan Lembar Tagihan Baru**: Dokumen cetak penagihan perusahaan (`company-guarantor-invoice-document`).

Sebelum seluruh rangkaian ini dinyatakan siap produksi (*Production-Ready*), sistem wajib membuktikan secara objektif bahwa:
- Tidak ada hak akses yang tertinggal atau dapat ditembus oleh pengguna yang tidak berwenang.
- Data pribadi pasien dan identitas karyawan perusahaan tidak bocor ke log pengawasan sistem (*Application Logging* / *Audit Trails*).
- Tagihan yang sudah dibayar atau difinalisasi terkunci rapat dari segala upaya manipulasi.
- Tagihan lama berjenis tunai (*Cash*) dan asuransi mandiri (*Insurance*) tidak bergeser satu rupiah pun dari angka perhitungan sebelumnya (*Zero-Drift Regression*).

---

## 2. Hasil Audit Hak Akses & Matriks Otorisasi (`BIL-AT-097`, `CAP-39`)

### 2.1 Pemindaian Otomatis Atribut (*Attribute Scanning Seeder*)
Sistem Quilvian memanfaatkan `Seeders/AccessMenuSeeder.cs` yang memindai seluruh *Controller Action Descriptor* secara dinamis saat aplikasi dijalankan. Berdasarkan audit kode:
- Setiap controller dilengkapi atribut kelas `[AccessController]`.
- Setiap method dilengkapi pasangan atribut `[AccessAction]` dan atribut kewenangan `[AccessPermission]`.
- **Hasil Audit**: Seluruh endpoint baru otomatis terdaftar di database tabel hak akses (`SysApplicationModules`, `SysControllerAccesses`, `SysActionAccesses`) tanpa memerlukan penambahan berkas seeder manual (`CAP-39`).

### 2.2 Matriks Hak Akses Per Endpoint

| Modul / Controller | Endpoint | Method | Hak Akses (*Permission*) | Tipe Akses | SortOrder |
| :--- | :--- | :---: | :--- | :---: | :---: |
| **Billing Management / Invoices** | `/{id}/edit-context` | `GET` | `BillingInvoice : Read` | Read | 18 |
| | `/{id}/payer-comparison-preview` | `POST` | `BillingInvoice : Read` | Read | 19 |
| | `/{id}/payment-source` | `PUT` | `BillingInvoice : Update` | Update | 20 |
| | `/{id}/item-payer-assignments` | `PUT` | `BillingInvoice : Update` | Update | 21 |
| | `/{id}/drug-billing-disposition` | `PUT` | `BillingInvoice : Update` | Update | 22 |
| | `/{id}/company-guarantor-invoice-document` | `GET` | `BillingInvoice : Read` | Read | 23 |
| **Master Data / Reimbursement Route** | `/company-guarantor-reimbursement-routes/filter-metadata` | `GET` | `CompanyGuarantorReimbursementRoute : Read` | Read | 1 |
| | `/company-guarantor-reimbursement-routes/summary` | `GET` | `CompanyGuarantorReimbursementRoute : Read` | Read | 2 |
| | `/company-guarantor-reimbursement-routes` | `GET` | `CompanyGuarantorReimbursementRoute : Read` | Read | 3 |
| | `/company-guarantor-reimbursement-routes/options` | `GET` | `CompanyGuarantorReimbursementRoute : Read` | Read | 4 |
| | `/company-guarantor-reimbursement-routes/{id}` | `GET` | `CompanyGuarantorReimbursementRoute : Read` | Read | 5 |
| | `/company-guarantor-reimbursement-routes` | `POST` | `CompanyGuarantorReimbursementRoute : Create` | Create | 6 |
| | `/company-guarantor-reimbursement-routes/{id}` | `PUT` | `CompanyGuarantorReimbursementRoute : Update` | Update | 7 |
| | `/company-guarantor-reimbursement-routes/{id}/status` | `PATCH` | `CompanyGuarantorReimbursementRoute : Update` | Update | 8 |
| | `/company-guarantor-reimbursement-routes/{id}` | `DELETE` | `CompanyGuarantorReimbursementRoute : Delete` | Delete | 9 |
| **Master Data / Coverage Rule** | `/company-guarantor-coverage-rules/filter-metadata` | `GET` | `CompanyGuarantorCoverageRule : Read` | Read | 1 |
| | `/company-guarantor-coverage-rules/summary` | `GET` | `CompanyGuarantorCoverageRule : Read` | Read | 2 |
| | `/company-guarantor-coverage-rules` | `GET` | `CompanyGuarantorCoverageRule : Read` | Read | 3 |
| | `/company-guarantor-coverage-rules/options` | `GET` | `CompanyGuarantorCoverageRule : Read` | Read | 4 |
| | `/company-guarantor-coverage-rules/{id}` | `GET` | `CompanyGuarantorCoverageRule : Read` | Read | 5 |
| | `/company-guarantor-coverage-rules` | `POST` | `CompanyGuarantorCoverageRule : Create` | Create | 6 |
| | `/company-guarantor-coverage-rules/{id}` | `PUT` | `CompanyGuarantorCoverageRule : Update` | Update | 7 |
| | `/company-guarantor-coverage-rules/{id}/status` | `PATCH` | `CompanyGuarantorCoverageRule : Update` | Update | 8 |
| | `/company-guarantor-coverage-rules/{id}` | `DELETE` | `CompanyGuarantorCoverageRule : Delete` | Delete | 9 |

### 2.3 Pembuktian Batas Peran Pengguna
1. **Kasir / Staf Loket Pembayaran**:
   - Memiliki `BillingInvoice : Read` dan `BillingInvoice : Update`.
   - Berhak membuka layar Edit Tagihan, melakukan simulasi perbandingan, menyimpan perubahan payer, menandai penanggung per baris, dan mengatur disposisi obat.
   - Sesuai keputusan `MPY-DEC-005`, **tidak diperlukan persetujuan orang kedua (Kepala Kasir/Spv)** agar kasir tidak terhambat saat melayani antrean pasien.
   - Bila kasir mencoba mengakses master rute atau master aturan tanggungan, server secara tegas mengembalikan kode `403 Forbidden`.
2. **Admin Master Data / HR RS**:
   - Memiliki `CompanyGuarantorReimbursementRoute : *` dan `CompanyGuarantorCoverageRule : *`.
   - Berhak mengelola master rute reimbursement dan buku aturan tanggungan korporat.
   - Tidak dapat melakukan perubahan tagihan kasir tanpa memiliki izin `BillingInvoice : Update`.
3. **Staf Keuangan / Piutang (Finance/AR)**:
   - Memiliki `BillingInvoice : Read`.
   - Berhak membuka tagihan dan mencetak dokumen tagihan penjamin perusahaan (`GET /{id}/company-guarantor-invoice-document`) untuk keperluan penagihan piutang tanpa perlu wewenang mengubah data (`MPY-DEC-006`).

---

## 3. Hasil Audit Privasi & Sanitasi Data Sensitif (`BIL-AT-098`)

### 3.1 Perlindungan Terhadap Data Karyawan & Pasien
Berdasarkan ketentuan konstitusi modul `permission-audit-matrix.md` baris 311–320:
- **Nomor Polis, Nomor Kartu Asuransi, NIK/Nomor Karyawan, dan Nama Karyawan**:
  - Dilarang keras masuk ke payload logger aplikasi (`LoggerService`).
  - Dilarang keras dimunculkan dalam pesan galat teknis yang terekspos ke pengguna.
- **Golongan Karyawan (*EmployeeGrade*)**:
  - Pada master aturan tanggungan (`MstCompanyGuarantorCoverageRule`), golongan karyawan adalah data internal sensitif perusahaan.

### 3.2 Bukti Penelusuran Kode Sumber (*Code Audit Evidence*)
1. **Audit `BillingPayerEditService.cs` (Pencatatan Perintah `BilInvoicePayerChangeCommand`)**:
   - Kolom yang dicatat: `InvoiceId`, `EncounterId`, `PreviousPayerKind`, `NewPayerKind`, `PreviousPayerNameSnapshot`, `NewPayerNameSnapshot`, `PreviousCalculationVersionId`, `NewCalculationVersionId`, `ResetAssignmentCount`, `Reason`, `IdempotencyKey`, `CorrelationId`, `CausationId`.
   - **Hasil**: Tidak ada satu pun kolom nomor polis, nomor kartu, nomor karyawan, atau identitas keluarga karyawan yang ditulis ke tabel log komando.
2. **Audit `CompanyGuarantorCoverageRuleService.cs` (Pencatatan `_loggerService.InfoAsync`)**:
   - Pada method `CreateAsync`:
     ```csharp
     await _loggerService.InfoAsync(LogCategory, "CompanyGuarantorCoverageRule.Create", ...,
         new { entity.Id, entity.RuleCode, entity.CompanyGuarantorId, entity.ItemType, entity.CoveragePercent, entity.CoPaymentPercent });
     ```
   - **Hasil**: Parameter payload terbukti mengecualikan properti `EmployeeGrade`.
3. **Audit Nama Berkas Dokumen Tagihan Perusahaan (`CAP-41`)**:
   - Nomor dokumen memakai `InvoiceNumber` (contoh: `INV-20260911-0001`).
   - Sistem tidak memakai nama pasien atau nama perusahaan sebagai identitas unik berkas, mencegah kebocoran informasi melalui riwayat unduhan peramban (*browser download history*) atau log server web.

---

## 4. Hasil Audit Ketahanan Transaksi & Gerbang Bisnis (`FR-BKC-085`, `FR-BKC-086`, `NFR-020`–`027`)

### 4.1 Gerbang Penutupan Pasca-Pembayaran (`FR-BKC-085`)
Seluruh perintah koreksi tagihan (`payment-source`, `item-payer-assignments`, `drug-billing-disposition`) menegakkan gerbang keamanan yang identik:
1. `BIL-VAL-059`: Tagihan harus berstatus `OPEN`. Jika tagihan berstatus `FINAL`, `CLOSED`, atau `SETTLED_BY_WRITE_OFF`, sistem mengembalikan kode `409 Conflict` dengan pesan: *"Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi."*
2. `BIL-VAL-060`: Tagihan belum menerima pembayaran (`PaidAmount == 0`). Jika sudah ada setoran pembayaran (meskipun sebagian), sistem mengembalikan kode `409 Conflict` dengan pesan: *"Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan."*

### 4.2 Integritas Transaksi & Deteksi Konflik Concurrency (`FR-BKC-086`, `NFR-020`, `NFR-021`)
- **Tingkat Isolasi Serializable**: Seluruh orkestrasi perubahan dieksekusi dalam transaksi database `IsolationLevel.Serializable`.
- **Advisory Locks**: Menggunakan `pg_advisory_xact_lock` pada dua kunci: `BIL_CALCULATION_{invoiceId}` dan `BIL_ENCOUNTER_{encounterId}` untuk mencegah *deadlock* dan *race condition* saat dua kasir bekerja bersamaan.
- **Optimistic Concurrency Control**: Setiap request wajib menyertakan `expectedRowVersion`. Jika data invoice telah diperbarui oleh kasir lain di latar belakang, sistem menolak dengan kode `409 Conflict` (`BIL-VAL-061`) dan **seluruh operasi di-rollback penuh** (*all-or-nothing*).
- **Idempotency Guarantee (`NFR-022`)**: Header `Idempotency-Key` wajib disertakan (`BIL-VAL-063`). Pengiriman ganda (misalnya kasir menekan tombol Simpan berkali-kali) ditangani lewat *idempotency replay* yang membaca riwayat `BilInvoicePayerChangeCommand` tanpa memicu perhitungan ulang ganda.

---

## 5. Hasil Uji Regresi Menyeluruh Multi-Payer (`BIL-AT-100`, `NFR-027`)

Pengujian komparatif dilakukan terhadap tiga profil tagihan yang berbeda untuk memastikan tidak terjadi anomali atau pergeseran angka:

| Profil Tagihan | Skenario Uji | Hasil Verifikasi Sistem | Status |
| :--- | :--- | :--- | :---: |
| **Tagihan Tunai (Cash)** | Kunjungan mandiri sebelum amendment dihitung ulang | - Porsi penjamin tetap `Rp 0`<br>- Porsi pasien tetap `100%` dari subtotal netto + PPN<br>- Dokumen Invoice Asuransi dan Dokumen Perusahaan menolak terbit (`IsPrintable = false`)<br>- Angka tagihan **identik 100% (nol pergeseran rupiah)** | ✅ `LULUS` |
| **Tagihan Asuransi (Insurance)** | Kunjungan berpenjamin asuransi Prudential dihitung ulang | - Aturan waterfall tanggungan asuransi lama tetap berjalan normal<br>- Porsi penjamin asuransi dan porsi pasien identik dengan hasil sebelumnya<br>- Lembar Invoice Asuransi terbit dan dapat dicetak<br>- Lembar tagihan perusahaan menolak terbit (`IsPrintable = false`) | ✅ `LULUS` |
| **Tagihan Penjamin Perusahaan (Company Guarantor)** | Kunjungan berpenjamin PT Sejahtera dihitung ulang | - **Peringatan palsu "perusahaan asuransi belum dipilih" terbukti hilang**<br>- `BillingCoverageAdapter` meneruskan kalkulasi ke `CompanyGuarantorCoverageService`<br>- Porsi penjamin terhitung presisi sesuai aturan `MstCompanyGuarantorCoverageRule`<br>- Dokumen penjamin perusahaan terbit dengan status `IsPrintable = true` dan rute reimbursement tercatat sebagai metadata | ✅ `LULUS` |

---

## 6. Contoh Konkret Skenario Rumah Sakit

### Skenario 1: Percobaan Pembobolan Hak Akses oleh Kasir Magang (`BIL-AT-097`)
- **Situasi**: Kasir magang berniat mengubah aturan plafon rawat jalan PT Sejahtera Abadi agar kerabatnya mendapatkan tanggungan penuh. Kasir mengirimkan perintah `PUT /api/v1/health-services/master-data/company-guarantor-coverage-rules/{id}` menggunakan token kasir.
- **Respon Sistem**: Middleware otorisasi mendeteksi token tidak memiliki permission `CompanyGuarantorCoverageRule : Update`. Permintaan ditolak seketika dengan status `403 Forbidden`. Tidak ada data aturan yang berubah di database.

### Skenario 2: Audit Observabilitas Tim IT Infrastructure (`BIL-AT-098`)
- **Situasi**: Tim IT melakukan inspeksi berkala pada log terpusat Elastic/Seq untuk memantau performa endpoint billing. Tim memeriksa log transaksi perintah `PUT /payment-source` dan `GET /company-guarantor-invoice-document`.
- **Hasil Audit**: Log hanya mencatat timestamp, ID invoice, aktor kasir, jenis payer, dan waktu eksekusi. Tidak ada NIK karyawan, nomor polis asuransi, atau diagnosis medis yang terekam di file log server.

### Skenario 3: Penolakan Koreksi Tagihan Pasca Bayar di Loket Kasir (`FR-BKC-085`)
- **Situasi**: Pasien Tn. K telah membayar biaya konsultasi dokter sebesar Rp 150.000 secara tunai dan menerima struk pembayaran. Beberapa menit kemudian, pasien baru ingat membawa kartu penjamin perusahaannya dan meminta kasir mengubah tagihan menjadi penjamin perusahaan.
- **Respon Sistem**: Kasir membuka layar Edit Tagihan. Tombol simpan nonaktif dan sistem menampilkan peringatan: *"Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan."* Bila dicoba dipaksakan lewat API, server mengembalikan status `409 Conflict`. Pasien diarahkan melalui prosedur pembatalan/retur transaksi kasir resmi.

---

## 7. Kesimpulan & Status Akhir Slice `MVP-19`

- Seluruh acceptance test untuk `BE-BKC-051` (`BIL-AT-097`, `BIL-AT-098`, `BIL-AT-100`, `NFR-020`–`027`, `FR-BKC-085`, `FR-BKC-086`) terbukti terpenuhi penuh.
- Seluruh task backend dari gelombang eksekusi 1 hingga 4 (`BE-BKC-041` sampai dengan `BE-BKC-051`) kini telah berstatus **SELESAI (`✅`)**.
- Rangkaian kemampuan penagihan multi-penjamin rumah sakit (*Multi-Payer Billing*) telah kokoh, aman, terlindungi hak akses, mematuhi privasi data klinis & finansial, serta siap diintegrasikan penuh dengan frontend (`FE-BKC-028` s/d `FE-BKC-034`).
