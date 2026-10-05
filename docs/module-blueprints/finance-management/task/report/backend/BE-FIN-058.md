# Laporan Perubahan Backend — `BE-FIN-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-058` |
| Judul | Skema tiga buku mutasi berdiri, dan Finance punya satu tempat menghitung tanggal WIB |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-130`..`132`, `FR-FIN-136`; `FIN-DEC-123`, `FIN-DEC-116`; `FIN-DES-079`, `FIN-DES-082`; `erd/data-dictionary.md` R14.1-R14.3; DDL revisi 14 |
| Contract version | `erd/data-dictionary.md` Revisi 14 |
| Dependency | `—` (Akar dari gelombang `REV-14A`) |
| Klasifikasi | `MEDIUM` (3 model baru, 3 EF Configuration, 3 DbSet, 1 helper statis, 1 migration) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `0e256765` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Tidak ada jejak saldo historis (buku mutasi):** Perhitungan posisi piutang, utang supplier, dan kas kasir pada tanggal tertentu di masa lalu tidak dapat direkonstruksi secara deterministik. Angka yang tersedia hanyalah saldo berjalan saat ini (`OutstandingAmount` / `ClosingBalance`), sehingga laporan per tanggal atau audit lintas periode rentan distorsi jika ada transaksi susulan atau pembatalan.
2. **Zona waktu tidak seragam:** Perhitungan tanggal akuntansi dan batas hari kas di berbagai service Finance tersebar memakai waktu UTC (`DateTime.UtcNow` / `DateTimeOffset.UtcNow`). Transaksi yang terjadi pada dini hari WIB (pukul 00.00–06.59 WIB) terhitung sebagai tanggal hari sebelumnya menurut UTC, yang pada akhir bulan menyebabkan transaksi melompat ke periode akuntansi sebelumnya.

Akibat bagi pengguna:
- Staf keuangan tidak dapat melihat riwayat kronologis mutasi saldo piutang dan utang per tanggal transaksi.
- Rekonsiliasi kas dan piutang akhir bulan antara Finance dan Accounting mengalami selisih tanggal tutup buku akibat perbedaan konvensi UTC vs WIB.

---

## 2. Proses bisnis

1. **Konversi Waktu Bisnis Terpusat (WIB):**
   - Seluruh service Finance yang membutuhkan konversi tanggal bisnis dan batas periode memanggil helper terpusat `FinanceBusinessDate`.
   - Menggunakan zona waktu `Asia/Jakarta` (WIB, UTC+7) dengan fallback otomatis ke `SE Asia Standard Time` bila dijalankan pada sistem operasi yang tidak mengenali identifier IANA.
2. **Pencatatan Buku Mutasi Subledger (Skema Data):**
   - **Piutang (`FinReceivableMovement`):** Setiap penambahan (pengakuan/migrasi) atau pengurangan (alokasi penerimaan/potongan/penyesuaian/penghapusan/pembayaran langsung) dicatat sebagai baris mutasi tidak terhapus (*append-only*) dengan invariant ketat: `BalanceAfter = BalanceBefore + Amount`.
   - **Utang Supplier (`FinSupplierPayableMovement`):** Setiap pembentukan utang faktur, pembayaran dokumen per alokasi, penyesuaian, dan pembayaran langsung dicatat dengan invariant saldo yang sama.
   - **Kas Kasir (`FinCashMovement`):** Menjadi sumbu tunggal kalkulasi posisi kas kasir dengan arah `IN` / `OUT` dan nominal selalu positif (`Amount > 0`). Idempotensi transaksi kas dijamin oleh index unik gabungan `(SourceReferenceType, SourceReferenceId, MovementType)`.
3. **Integritas Batas Wewenang Task:**
   - Pada task ini, **nol penulis mutasi** dibuat. Task ini murni menyediakan skema database, pemetaan EF Core, dan helper tanggal agar siap digunakan oleh task penulis berikutnya (`BE-FIN-059` untuk 21 titik WIB, `BE-FIN-060` untuk penulis piutang, `BE-FIN-061` untuk penulis utang, dan `BE-FIN-062` untuk penulis kas).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-079, §FIN-DES-082, §L.5)
3. `docs/module-blueprints/finance-management/erd/data-dictionary.md` (R14.1..R14.3, DDL Revisi 14)
4. `docs/module-blueprints/finance-management/erd/receivable-collection.md`
5. `docs/module-blueprints/finance-management/erd/payable.md`
6. `docs/module-blueprints/finance-management/erd/cash-and-master-data.md`
7. `Areas/HealthServices/BillingManagement/MasterData/Services/AdministrationFeePolicyService.cs` (preseden zona waktu)
8. `Repositories/ApplicationDbContext.cs`
9. `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableConfiguration.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceBusinessDate.cs` | Helper statis terpusat untuk konversi tanggal dan batas periode kalender WIB dengan fallback `SE Asia Standard Time`. |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs` | Entity model buku mutasi piutang beserta konstanta `FinReceivableMovementTypes`. |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` | Penambahan navigation collection `Movements` (`ICollection<FinReceivableMovement>`). |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableMovement.cs` | Entity model buku mutasi utang supplier beserta konstanta `FinSupplierPayableMovementTypes`. |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` | Penambahan navigation collection `Movements` (`ICollection<FinSupplierPayableMovement>`). |
| `Areas/Corporate/FinanceManagement/CashManagement/Models/FinCashMovement.cs` | Entity model buku mutasi kas beserta konstanta jenis mutasi, arah (`IN`/`OUT`), dan jenis rujukan sumber. |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableMovementConfiguration.cs` | Konfigurasi EF Core untuk `FinReceivableMovement` lengkap dengan check constraint `CK_FinReceivableMovement_Balance` dan `CK_FinReceivableMovement_FundingSource`, FK ke `FinReceivable`, serta index parsial unik. |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableMovementConfiguration.cs` | Konfigurasi EF Core untuk `FinSupplierPayableMovement` dengan check constraint `CK_FinSupplierPayableMovement_Balance`, FK ke `FinSupplierPayable`, dan index pendukung. |
| `Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinCashMovementConfiguration.cs` | Konfigurasi EF Core untuk `FinCashMovement` dengan check constraint `CK_FinCashMovement_Direction`, `CK_FinCashMovement_Amount`, dan index unik idempotensi `IX_FinCashMovement_Source`. |
| `Repositories/ApplicationDbContext.cs` | Pendaftaran 3 `DbSet`: `FinReceivableMovements`, `FinSupplierPayableMovements`, `FinCashMovements`. |
| `Migrations/20261002093000_AddFinanceSubledgerMovementLedgers.cs` | Berkas migrasi EF Core untuk membuat ketiga tabel buku mutasi beserta seluruh constraint dan index. |
| `Migrations/20261002093000_AddFinanceSubledgerMovementLedgers.Designer.cs` | Berkas designer/metadata snapshot untuk migration `AddFinanceSubledgerMovementLedgers` yang diselaraskan dengan EF model snapshot. |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Pembaruan model snapshot DbContext dengan ketiga entitas subledger baru beserta konfigurasi relasi dan navigasinya. |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceNonPatientReceivableDtos.cs` | Perbaikan modifier kelas `CreateNonPatientReceivableRequest` dari `sealed` menjadi `unsealed` agar turunan `UpdateNonPatientReceivableRequest` dapat dikompilasi secara valid. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini murni menyediakan skema database, model, dan helper internal. Nol endpoint baru dibuat pada task ini. |
| Database | Penambahan 3 tabel baru (`FinReceivableMovement`, `FinSupplierPayableMovement`, `FinCashMovement`). Migration `20261002093000_AddFinanceSubledgerMovementLedgers` dibuat, **belum dijalankan/dieksekusi** ke basis data. |
| Keamanan/Auth | `NOT APPLICABLE` — Tidak ada endpoint publik yang diekspos. Akses data dijaga oleh service internal pada task berikutnya. |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Tidak ada endpoint controller yang dibuat atau disentuh pada task ini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pengecekan kesesuaian DDL R14.1..R14.3 | Check constraint saldo, check constraint funding source, check constraint direction, dan unique index idempotensi kas telah terdefinisi di model, configuration, dan migration | `PASS` | `FinReceivableMovementConfiguration.cs`, `FinSupplierPayableMovementConfiguration.cs`, `FinCashMovementConfiguration.cs`, `20261002093000_AddFinanceSubledgerMovementLedgers.cs` |
| Verifikasi Fallback Zona Waktu WIB | `FinanceBusinessDate` mengimplementasikan fallback dari `Asia/Jakarta` ke `SE Asia Standard Time` menggunakan `TimeZoneNotFoundException` | `PASS` | `FinanceBusinessDate.cs` |
| Pemeriksaan Lingkup Task (Nol Penulis) | Tidak ada penambahan service penulis mutasi pada task ini (perubahan skema murni dipisahkan dari perubahan perilaku transaksi) | `PASS` | `git status --short` menunjukkan hanya model, konfigurasi EF, DbContext, helper, dan migration yang ditambahkan |

Uji manual: `NOT APPLICABLE` (skema murni, belum ada penulis dan belum dieksekusi ke database).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Ketiga tabel terbentuk beserta check constraint saldo dan unique index idempotensi kas | **Terpenuhi** | Model dan konfigurasi EF Core untuk `FinReceivableMovement`, `FinSupplierPayableMovement`, dan `FinCashMovement` telah dibuat lengkap dengan `CK_FinReceivableMovement_Balance`, `CK_FinSupplierPayableMovement_Balance`, `CK_FinCashMovement_Direction`, `CK_FinCashMovement_Amount`, dan unique index `IX_FinCashMovement_Source`. |
| Helper memulangkan tanggal WIB dan memakai cadangan `SE Asia Standard Time` bila `Asia/Jakarta` tidak tersedia | **Terpenuhi** | Terimplementasi pada `FinanceBusinessDate.cs` dengan mekanisme `Lazy<TimeZoneInfo>` dan penanganan `TimeZoneNotFoundException`. |
| Nol penulis dibuat pada task ini | **Terpenuhi** | Tidak ada modifikasi logic penulis pada `FinanceReceivableService`, `FinanceSupplierPayableService`, atau `FinanceCashManagementService` pada task ini. |
| Migration dibuat, belum dijalankan | **Terpenuhi** | Berkas migrasi `Migrations/20261002093000_AddFinanceSubledgerMovementLedgers.cs` telah dibuat. **Database belum dieksekusi** sesuai batasan kewenangan `FIN-DEC-138`. |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesuai `FIN-DEC-138`, agent **TIDAK** memiliki wewenang untuk mengeksekusi migrasi ke basis data. Penerapan migrasi ke database harus dijalankan secara manual oleh Yasmin/Database Administrator. |
| Langkah eksekusi migrasi oleh Yasmin | Jalankan perintah CLI: `dotnet ef database update 20261002093000_AddFinanceSubledgerMovementLedgers` pada environment target yang berwenang. |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Ketiga tabel belum aktif di database fisik sampai Yasmin menjalankan migrasi. Namun, kode aplikasi dan model sudah siap dikonsumsi oleh task berikutnya (`BE-FIN-059` dan `BE-FIN-060`). |
| Perubahan sampingan | Perbaikan deklarasi kelas `CreateNonPatientReceivableRequest` pada `FinanceNonPatientReceivableDtos.cs` dari `sealed` menjadi `unsealed` agar `UpdateNonPatientReceivableRequest` valid saat dikompilasi. |
| Interupsi | `NONE` |
| Status Git | Seluruh berkas baru dan modifikasi tercatat di `git status --short`. Nol commit/push otomatis dilakukan. |
| Langkah berikutnya | Melanjutkan eksekusi task `BE-FIN-059` (penerapan konversi `FinanceBusinessDate` pada 21 titik WIB di lima service berjalan) dan `BE-FIN-060` (pembangunan service penulis `FinanceSubledgerMovementService`). |
