# Laporan Perubahan Backend — `BE-BKC-089`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-089` |
| Judul | Model, configuration, dan migration kolom pelaksana dan jejak `DoctorShare` |
| Slice | Gelombang Eksekusi ⛔ (Revisi Blueprint 2.4 — Jasa Medis pada Item Tagihan / `MVP-38`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-089` |
| Trace | `FR-BKC-286`; `BKC-DEC-171`, `BKC-DEC-178`; `BKC-DES-097`; `INV-MF-3`, `INV-MF-5` |
| Contract version | `data/data-dictionary.md` amendment revisi `2.4`; `02-backend-architecture.md` §25.9–25.10 |
| Dependency | `{OTORISASI-MIG-3}` (Otorisasi pembuatan dan eksekusi migration skema) |
| Klasifikasi | `MEDIUM` (Perubahan model entity inti tagihan, konfigurasi EF Core, migrasi basis data, dan model snapshot) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs`<br/>`Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceItemConfiguration.cs`<br/>`Migrations/20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs`<br/>`Migrations/ApplicationDbContextModelSnapshot.cs` |
| Model | Gemini 3.8 Flash |
| Commit backend saat dikerjakan | `ecddd8f8` (pada branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI (Model, konfigurasi EF, migration, dan model snapshot terpasang; eksekusi database dan build otomatis dilepas atas instruksi/otorisasi terpisah)** |

---

## Ringkasan untuk Pembaca Umum

Setiap kali pasien rumah sakit mendapatkan layanan atau tindakan medis (misalnya konsultasi dokter, pemeriksaan USG, atau tindakan keperawatan), sistem kasir mencatat setiap rincian layanan tersebut ke dalam baris tagihan (*invoice item*). Pada sistem sebelumnya, baris tagihan hanya mencatat deskripsi tindakan, jumlah, dan harga satuan, tetapi **tidak mencatat siapa dokter atau tenaga medis pelaksananya**, serta **tidak mencatat asal perhitungan jasa medisnya**.

Hal ini menyebabkan perhitungan bagi hasil (*doctor share / fee sharing*) dokter harus ditelusuri secara manual di luar sistem melalui spreadsheet Excel karena sistem kasir tidak menyimpan data dokter pelaksana di tingkat baris tindakan.

Melalui task `BE-BKC-089`:
1. Tabel baris tagihan (`BilInvoiceItem`) diperluas dengan **enam kolom baru**:
   - Penanda apakah tindakan tersebut memiliki jasa medis (`IsFeeBearing`).
   - Tipe pelaksana (`PerformerType`: dokter atau tenaga medis/nakes).
   - Rujukan identitas pelaksana (`PerformerReferenceId`: ID dokter atau nakes).
   - Kode peran pelaksana (`PerformerRoleCode`: peran utama atau peran pendamping).
   - Asal usul perhitungan bagian dokter (`DoctorShareResolution`: apakah dari aturan Medical Fee, tidak ada aturan, layanan eksternal gangguan, atau tidak berlaku).
   - ID aturan bagi hasil yang digunakan (`DoctorShareSharingRuleId`).
2. Dipasang **empat aturan pengaman ketat di tingkat basis data (*check constraints*)** agar tidak ada data korup yang masuk (misalnya menandai layanan berjasa tetapi tidak mencantumkan nama dokternya).
3. Disiapkan berkas migrasi basis data resmi (`20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs`) beserta pembaruan *Model Snapshot* aplikasi. Seluruh baris tagihan lama rumah sakit tetap sah dan aman (`IsFeeBearing = false`, kolom lainnya `NULL`).

---

## 1. Masalah yang Diperbaiki

Sebelum task ini:
- **Ketiadaan Tempat Penyimpanan Dokter Pelaksana pada Item Tagihan:** Baris tagihan kasir (`BilInvoiceItem`) tidak memiliki kolom untuk mencatat siapa dokter atau tenaga kerja yang melakukan tindakan tersebut.
- **Ketiadaan Akuntabilitas Asal Nilai `DoctorShare`:** Nilai nominal `DoctorShare` pada baris tagihan tidak memiliki riwayat audit apakah nominal tersebut dihitung berdasarkan aturan kontrak Medical Fee resmi atau sekadar nilai default nol (`INV-MF-5`).
- **Risiko Ketidakkonsistenan Data (*Data Integrity Gap*):** Tanpa pengaman di tingkat database (*check constraint*), baris tagihan bisa saja secara tidak sengaja diisi status berjasa tanpa menyertakan dokter pelaksana (`INV-MF-3`).

Dengan implementasi `BE-BKC-089`, tabel `BilInvoiceItem` memiliki skema penyimpanan yang lengkap, konsisten, dan memiliki integritas data yang kokoh di level PostgreSQL.

---

## 2. Alur Proses Bisnis

### 2.1 Hubungan Antara Tagihan Kasir dan Jasa Medis

```
[Kasir Loket] Menambahkan Tindakan "Konsultasi Dokter Spesialis"
   │
   ├── Layanan Berjasa: Ya (IsFeeBearing = true)
   ├── Tipe Pelaksana: DOCTOR
   ├── ID Dokter: ID dr. Rina (PerformerReferenceId)
   ├── Peran: Null (Peran Utama)
   │
   ▼
[Validasi Check Constraint Basis Data PostgreSQL]
   │
   ├── Invariant 1: Apakah IsFeeBearing = true memiliki PerformerType & PerformerReferenceId?
   │     └── Ya -> Lolos (CK_BilInvoiceItem_PerformerRequired)
   ├── Invariant 2: Apakah baris non-berjasa tidak membawa data pelaksana?
   │     └── Ya -> Lolos (CK_BilInvoiceItem_PerformerOnlyWhenFeeBearing)
   ├── Invariant 3: Apakah PerformerType bernilai valid ('DOCTOR' atau 'WORKFORCE')?
   │     └── Ya -> Lolos (CK_BilInvoiceItem_PerformerType)
   └── Invariant 4: Apakah DoctorShareResolution dan DoctorShareSharingRuleId sinkron?
         └── Ya -> Lolos (CK_BilInvoiceItem_DoctorShareResolution)
   │
   ▼
[Tersimpan di BilInvoiceItem]
   └── Data siap dibaca oleh modul Medical Fee untuk penggajian jasa dokter bulanan (BIL-INT-024).
```

### 2.2 Contoh Skenario Nyata di Rumah Sakit

1. **Skenario Layanan Dokter Spesialis:**
   - Pasien berkonsultasi dengan dr. Rina, Sp.A. Biaya tindakan Rp 500.000.
   - Kolom yang tersimpan: `IsFeeBearing = true`, `PerformerType = 'DOCTOR'`, `PerformerReferenceId = <ID dr. Rina>`, `DoctorShareResolution = 'MEDICAL_FEE'`, `DoctorShareSharingRuleId = <ID Aturan>`, `DoctorShare = 200000.00`.
   - Hasil: Jasa medis dr. Rina tercatat akurat dan langsung dapat ditarik oleh bagian komite medik/keuangan.

2. **Skenario Penjualan Barang / Obat Non-Jasa:**
   - Pasien membeli obat bebas di kasir apotek atau membayar meterai Rp 10.000.
   - Kolom yang tersimpan: `IsFeeBearing = false`, `PerformerType = NULL`, `PerformerReferenceId = NULL`, `PerformerRoleCode = NULL`, `DoctorShareResolution = 'NOT_APPLICABLE'`, `DoctorShareSharingRuleId = NULL`, `DoctorShare = 0.00`.
   - Hasil: Transaksi lolos check constraint tanpa membebani entitas pelaksana.

3. **Skenario Perlindungan Data Lama (Backward Compatibility):**
   - Ratusan ribu data tagihan historis yang dibuat sebelum revisi 2.4 memiliki nilai `IsFeeBearing = false` (nilai bawaan default) dan seluruh kolom pelaksana bernilai `NULL`.
   - Tidak diperlukan operasi backfill massal yang membebani database, dan seluruh data historis tetap 100% valid memenuhi seluruh check constraint.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

- `docs/module-blueprints/billing-kasir/data/data-dictionary.md` (amendment revisi 2.4 baris 1788–1865)
- `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (§25.9–25.10)
- `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` (§ `BE-BKC-089`)
- `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs`
- `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceItemConfiguration.cs`
- `Migrations/ApplicationDbContextModelSnapshot.cs`

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs` | **Diperbarui**. Menambahkan 6 properti baru (`IsFeeBearing`, `PerformerType`, `PerformerReferenceId`, `PerformerRoleCode`, `DoctorShareResolution`, `DoctorShareSharingRuleId`) dan dua kelas konstanta (`BillingPerformerTypes` dan `BillingDoctorShareResolutions`). |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceItemConfiguration.cs` | **Diperbarui**. Menambahkan konfigurasi kolom baru (`IsFeeBearing` default `false`, batasan panjang string) dan 4 check constraint PostgreSQL di blok `ToTable`. |
| `Migrations/20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs` | **Dibuat**. Berkas migrasi EF Core resmi berisi metode `Up()` (menambah 6 kolom dan 4 constraint) dan `Down()` (menghapus 4 constraint lalu 6 kolom). |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Diperbarui**. Memperbarui snapshot model EF Core untuk entitas `BilInvoiceItem` agar mencakup 6 properti dan 4 check constraint baru. |

### 3.3 Rincian Enam Kolom Baru

| Kolom | Tipe Data | Nullable? | Nilai Bawaan | Panjang Maks | Keterangan |
|---|---|:---:|:---:|:---:|---|
| `IsFeeBearing` | `boolean` | Tidak | `false` | — | Penanda bahwa item tagihan merupakan layanan yang memiliki komponen jasa pelaksana. |
| `PerformerType` | `varchar(20)` | Ya | `NULL` | 20 | Jenis tenaga medis pelaksana (`DOCTOR` atau `WORKFORCE`). Wajib terisi jika `IsFeeBearing = true`. |
| `PerformerReferenceId` | `uuid` | Ya | `NULL` | — | ID rujukan pelaksana medis (ke `MstDoctor.Id` atau `MstWorkforceProfile.Id`). Tanpa foreign key fisik (polimorfik). |
| `PerformerRoleCode` | `varchar(30)` | Ya | `NULL` | 30 | Kode peran dokter/nakes (misal `OPERATOR_1`, `ANESTHESIOLOGIST`). Kosong/null berarti peran utama. |
| `DoctorShareResolution` | `varchar(20)` | Ya | `NULL` | 20 | Asal nilai bagi hasil: `MEDICAL_FEE`, `NO_RULE`, `UNAVAILABLE`, atau `NOT_APPLICABLE`. Null untuk data historis. |
| `DoctorShareSharingRuleId` | `uuid` | Ya | `NULL` | — | ID aturan bagi hasil Medical Fee (`MdfSharingRule.Id`). Terisi jika dan hanya jika resolusi bernilai `MEDICAL_FEE`. |

### 3.4 Rincian Empat Check Constraint PostgreSQL

| Nama Check Constraint | Definisi SQL Ekspresi | Tujuan Perlindungan Data |
|---|---|---|
| `CK_BilInvoiceItem_PerformerRequired` | `NOT "IsFeeBearing" OR ("PerformerType" IS NOT NULL AND "PerformerReferenceId" IS NOT NULL)` | Mencegah baris berjasa tersimpan tanpa jenis dan identitas pelaksana (`INV-MF-3`). |
| `CK_BilInvoiceItem_PerformerOnlyWhenFeeBearing` | `"IsFeeBearing" OR ("PerformerType" IS NULL AND "PerformerReferenceId" IS NULL AND "PerformerRoleCode" IS NULL)` | Mencegah data pelaksana tersimpan pada baris yang tidak berjasa (`INV-MF-3`). |
| `CK_BilInvoiceItem_PerformerType` | `"PerformerType" IS NULL OR "PerformerType" IN ('DOCTOR','WORKFORCE')` | Membatasi jenis pelaksana hanya pada domain yang diakui (`BIL-VAL-162`). |
| `CK_BilInvoiceItem_DoctorShareResolution` | `"DoctorShareResolution" IS NULL OR ("DoctorShareResolution" IN ('MEDICAL_FEE','NO_RULE','UNAVAILABLE','NOT_APPLICABLE') AND ("DoctorShareResolution" = 'MEDICAL_FEE') = ("DoctorShareSharingRuleId" IS NOT NULL))` | Menjamin nilai resolusi valid dan `DoctorShareSharingRuleId` terisi jika dan hanya jika resolusi adalah `MEDICAL_FEE` (`INV-MF-5`). |

### 3.5 Dampak Kontrak API, Database, dan Keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` pada slice ini — DTO controller dan endpoint kasir akan diperbarui pada task `BE-BKC-091`. |
| Database | **Perubahan Skema Aditif Tanpa Downtime.** Tabel `BilInvoiceItem` mendapat 6 kolom dan 4 constraint. Berkas migration `20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs` disiapkan. Eksekusi database di dev/staging/prod menunggu otorisasi terpisah pemilik (`{OTORISASI-MIG-3}`). |
| Keamanan & Otorisasi | Tidak ada perubahan hak akses. Data nominal `DoctorShare` tidak dicatat ke log publik demi privasi remunerasi tenaga medis. |

---

## 4. Dokumentasi Endpoint

`NOT APPLICABLE` — Task ini adalah perubahan layer persistensi data (Model, EF Core Configuration, dan Database Migration), bukan pembuatan endpoint controller baru.

---

## 5. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Verifikasi Snapshot vs Kamus Data | Identik 100% | `PASS` | Enam kolom dan empat check constraint pada `ApplicationDbContextModelSnapshot.cs` cocok persis dengan kamus data revisi 2.4. |
| Verifikasi Simetri Up/Down Migration | Simetris dan aman | `PASS` | `Up()` menambahkan 6 kolom lalu 4 constraint; `Down()` menjatuhkan 4 constraint terlebih dahulu lalu menjatuhkan 6 kolom. |
| `dotnet build` | Dilepas atas instruksi pengguna | `NOT RUN` | Instruksi pengguna secara eksplisit: *"tanpa dotnet build automatis"*. Tidak ada build otomatis yang dijalankan. |
| Eksekusi Migration ke Database Dev | Menunggu otorisasi terpisah | `NOT RUN` | Sesuai aturan tata kelola `AGENTS.md` dan prasyarat `{OTORISASI-MIG-3}`, eksekusi `dotnet ef database update` tidak dijalankan secara otomatis. |

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Model snapshot memuat enam kolom dan empat constraint persis seperti kamus data. | Terpenuhi | Terdaftar di `ApplicationDbContextModelSnapshot.cs:64264-64405`. |
| 2. `Up()` hanya memuat perubahan cakupan 1–2; `Down()` menghapus constraint lalu kolom. | Terpenuhi | Terdefinisi di `Migrations/20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs:25-144`. |
| 3. Setelah migration dijalankan di dev, seluruh baris lama tetap valid (`IsFeeBearing = false`, kolom lain `NULL`). | Terbukti secara analitis | `IsFeeBearing` memiliki `defaultValue: false`, kolom lainnya `nullable: true`, seluruh baris historis lolos 4 check constraint. |
| 4. Menyisipkan baris `IsFeeBearing = true` tanpa pelaksana ditolak basis data (`CK_BilInvoiceItem_PerformerRequired`). | Terbukti secara definisi constraint | Check constraint `NOT "IsFeeBearing" OR ("PerformerType" IS NOT NULL AND "PerformerReferenceId" IS NOT NULL)` mengevaluasi `false` bila `IsFeeBearing = true` dan pelaksana null. |

### Definition of Done
- [x] Cakupan 1–3 terpasang (`BilInvoiceItem.cs`, `BilInvoiceItemConfiguration.cs`, migration file, model snapshot).
- [x] Kriteria penerimaan 1–2 terbukti penuh; kriteria 3–4 terbukti secara struktural dan analitis skema.
- [x] Build otomatis dilepas atas instruksi eksplisit pengguna: *"tanpa dotnet build automatis"*.
- [x] Laporan tracked `task/report/backend/BE-BKC-089.md` tersusun lengkap.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Eksekusi migration basis data (`dotnet ef database update`) di dev/staging/prod harus dijalankan oleh pihak berwenang (`{OTORISASI-MIG-3}`). Penambahan kolom dan check constraint bersifat non-breaking dan zero-downtime. |
| Masalah yang diketahui | Tidak ada (*none*). |
| Risiko tersisa | Rendah. Menyentuh tabel bersama `BilInvoiceItem`, namun sifatnya murni aditif dan ber-default aman untuk baris lama. |
| Perubahan sampingan | `NONE`. Hanya berkas model `BilInvoiceItem`, konfigurasi EF-nya, migration, dan model snapshot yang disentuh. |
| Interupsi | `NONE`. |
| Status Git | `M Areas/HealthServices/BillingManagement/Billing/Models/BilInvoiceItem.cs`<br/>`M Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInvoiceItemConfiguration.cs`<br/>`M Migrations/ApplicationDbContextModelSnapshot.cs`<br/>`?? Migrations/20261026150000_AddBilInvoiceItemPerformerAndDoctorShareTrace.cs`<br/>`?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-089.md` |
| Langkah berikutnya | Pemilik basis data/repository dapat menjalankan migration di lingkungan PostgreSQL dev, membuka pengerjaan task `BE-BKC-091` (entri kasir berjasa) dan `BE-BKC-093` (salinan item). |
