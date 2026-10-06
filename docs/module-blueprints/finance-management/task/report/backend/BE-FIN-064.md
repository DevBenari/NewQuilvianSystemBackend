# Laporan Perubahan Backend — `BE-FIN-064`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-064` |
| Judul | Skema pemetaan akun control dan saldo awal cutover berdiri |
| Slice | `REV-14B` (`EPIC FIN-21` — pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-141`, `FR-FIN-146`; `FIN-DEC-113`, `FIN-DEC-128`, `FIN-DEC-138`; `FIN-DES-080`, `FIN-DES-088`; `erd/data-dictionary.md` R14.4, R14.5; DDL revisi 14 |
| Contract version | `erd/data-dictionary.md` Revisi 14 |
| Dependency | `BE-FIN-063` |
| Klasifikasi | `MEDIUM` (2 model baru, 2 EF Configuration, 2 DbSet, 1 migration, 1 migration designer, 1 update snapshot) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Belum Ada Skema Pemetaan Akun Control Subledger:**
   Sistem Finance belum memiliki tabel konfigurasi yang menghubungkan kelompok saldo subledger (`KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`, `UTANG-JASA-MEDIS`) dengan kode akun pengendali (*control account*) pada Chart of Accounts (COA) Accounting. Akibatnya, sinkronisasi dan rekonsiliasi antara buku pembantu subledger dan buku besar (GL) tidak dapat dipetakan secara terkonfigurasi per unit/segmen bisnis.
2. **Belum Ada Skema Pencatatan Saldo Awal Cutover:**
   Saat rumah sakit melakukan proses *go-live* atau migrasi sistem dari sistem warisan (*legacy*) ke Quilvian V2, tidak ada wadah resmi di basis data untuk merekam saldo awal per tanggal *cutover*. Tanpa mekanisme ini, perhitungan posisi berjalan subledger tidak memiliki angka awal yang sah.
3. **Risiko Pelanggaran Invariant Saldo Awal:**
   Pada sistem rumah sakit, saldo awal piutang dan utang tidak boleh dimasukkan sebagai satu angka gelondongan (*lump-sum*) tanpa rincian transaksi karena kasir dan staf penagihan tidak akan bisa mengalokasikan pelunasan faktur atau klaim asuransi pasien secara valid. Diperlukan penegakan integritas data pada level skema basis data (*check constraint*) agar kelompok piutang dan utang wajib bernilai `0.00` pada header saldo awal cutover (rinciannya masuk lewat faktur migrasi individual).

---

## 2. Proses bisnis

1. **Pemetaan Akun Control Subledger (`FinSubledgerControlAccountMap`):**
   - Staf akuntansi rumah sakit mendefinisikan pemetaan antara kelompok saldo subledger (`BalanceGroup`) dan kode akun control COA (`ControlAccountCode`).
   - Mendukung pemisahan segmen bisnis (`SegmentKey`) opsional, misalnya pemetaan akun kas kasir per lokasi gedung rawat inap vs rawat jalan, atau pemetaan per unit operasional tertentu.
   - Diproteksi dengan *unique index* parsial `(BalanceGroup, SegmentKey)` dan `ControlAccountCode` untuk baris aktif (`IsActive = true` dan `IsDelete = false`), mencegah duplikasi konfigurasi ganda pada akun yang sama.
   - Menggunakan *check constraint* `CK_FinSubledgerControlAccountMap_BalanceGroup` yang membatasi kelompok saldo hanya pada nilai sah: `'KAS-KASIR'`, `'KAS-KECIL'`, `'PIUTANG'`, `'UTANG-SUPPLIER'`, dan `'UTANG-JASA-MEDIS'`.

2. **Pencatatan Saldo Awal Cutover (`FinOpeningBalance`):**
   - Menjadi basis angka awal posisi subledger pada tanggal peralihan sistem (*CutoverDate*).
   - Setiap kelompok saldo (`BalanceGroup`) hanya diperbolehkan memiliki tepat 1 rekaman saldo awal aktif (dijamin oleh *unique index* `IX_FinOpeningBalance_BalanceGroup` dengan filter `IsDelete = false`).
   - **Siklus Hidup Status:**
     - `DRAFT`: Saldo awal baru dicatat oleh staf keuangan dan masih dapat disesuaikan.
     - `APPROVED`: Disetujui oleh Kepala Bagian Keuangan/Manajer Akuntansi dengan mencatat identitas verifikator (`ApprovedBy`) dan waktu persetujuan (`ApprovedAt`).
     - `LOCKED`: Saldo awal telah terkunci permanen (`LockedAt`), menjadi titik referensi final yang tidak dapat diubah lagi oleh proses operasional mana pun.
   - **Penegakan Invariant Skema Basis Data:**
     - *Check constraint* `CK_FinOpeningBalance_ItemGroupZero`: Memastikan bahwa untuk kelompok `'PIUTANG'`, `'UTANG-SUPPLIER'`, dan `'UTANG-JASA-MEDIS'`, kolom `Amount` **wajib bernilai 0.00**. Hal ini menjamin bahwa seluruh saldo awal piutang dan utang harus terbentuk dari dokumen rincian individual (seperti faktur tagihan migrasi pasien atau faktur utang supplier migrasi).
     - *Check constraint* `CK_FinOpeningBalance_Status`: Membatasi status saldo awal hanya pada `'DRAFT'`, `'APPROVED'`, dan `'LOCKED'`.

3. **Batas Wewenang Task (Scope Boundary):**
   - Pada task ini, **nol service bisnis baru** dibuat. Task ini murni menyediakan struktur data, pemetaan EF Core, DbSet DbContext, dan berkas migrasi database.
   - Logika bisnis service pemetaan akun control diimplementasikan pada task berikutnya (`BE-FIN-065`), dan service saldo awal diimplementasikan pada `BE-FIN-066`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-080, §FIN-DES-088)
3. `docs/module-blueprints/finance-management/erd/data-dictionary.md` (R14.4, R14.5, DDL Revisi 14)
4. `Repositories/ApplicationDbContext.cs`
5. `Migrations/ApplicationDbContextModelSnapshot.cs`

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinSubledgerControlAccountMap.cs` | Model entitas pemetaan akun control subledger beserta daftar kelompok saldo sah (`FinSubledgerBalanceGroups`). |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningBalance.cs` | Model entitas saldo awal cutover subledger beserta daftar status sah (`FinOpeningBalanceStatuses`). |
| `Repositories/Configurations/Corporate/FinanceManagement/AccountingIntegration/FinSubledgerControlAccountMapConfiguration.cs` | Konfigurasi EF Core untuk `FinSubledgerControlAccountMap` lengkap dengan *check constraint* `CK_FinSubledgerControlAccountMap_BalanceGroup` dan *unique index* parsial. |
| `Repositories/Configurations/Corporate/FinanceManagement/AccountingIntegration/FinOpeningBalanceConfiguration.cs` | Konfigurasi EF Core untuk `FinOpeningBalance` dengan *check constraint* `CK_FinOpeningBalance_Status` & `CK_FinOpeningBalance_ItemGroupZero`, serta *unique index* per kelompok saldo. |
| `Repositories/ApplicationDbContext.cs` | Pendaftaran 2 `DbSet`: `FinSubledgerControlAccountMaps` dan `FinOpeningBalances`. |
| `Migrations/20261002103000_AddFinanceSubledgerSetup.cs` | Berkas migrasi EF Core untuk pembuatan tabel `FinSubledgerControlAccountMap` dan `FinOpeningBalance` beserta semua constraint dan index. |
| `Migrations/20261002103000_AddFinanceSubledgerSetup.Designer.cs` | Berkas metadata/designer migrasi yang disinkronkan secara presisi dengan snapshot model database terbaru. |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Pembaruan snapshot model DbContext dengan kedua entitas baru di namespace `AccountingIntegration.Models`. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini murni menyediakan skema database dan model entitas. Nol endpoint baru dibuat pada task ini. |
| Database | Penambahan 2 tabel baru (`FinSubledgerControlAccountMap` dan `FinOpeningBalance`). Berkas migrasi `20261002103000_AddFinanceSubledgerSetup` dibuat secara terisolasi dan **belum dijalankan/dieksekusi** ke basis data PostgreSQL (menunggu approval dan eksekusi rilis terpusat). |
| Keamanan/Auth | `NOT APPLICABLE` — Tidak ada endpoint publik yang diekspos. Akses data akan dilindungi oleh permission RBAC Finance pada task service dan controller berikutnya (`BE-FIN-065` dan `BE-FIN-066`). |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Tidak ada endpoint controller yang dibuat atau disentuh pada task ini.

---

## 5. Verifikasi dan Bukti Kepatuhan

1. **Struktur Model dan EF Core Configuration:**
   - Semua kolom audit (`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`) diwarisi dari `IdentityModel` dan dikonfigurasi secara konsisten.
   - Model `FinOpeningBalance` memiliki concurrency token `RowVersion` untuk mencegah konflik saat persetujuan/penguncian saldo.
2. **Sinkronisasi Migrasi dan Snapshot Model:**
   - Berkas migrasi `20261002103000_AddFinanceSubledgerSetup.cs` menyertakan method `Up` dan `Down` yang simetris dan aman.
   - `ApplicationDbContextModelSnapshot.cs` diperbarui dengan kedua entitas pada posisi urutan namespace yang benar.
   - Berkas designer `20261002103000_AddFinanceSubledgerSetup.Designer.cs` dibuat dan diverifikasi valid terhadap target model EF Core.
3. **Batas Task:**
   - Tidak ada modifikasi pada service atau logika bisnis pada task ini.
   - Tidak ada pemanggilan build otomatis (`dotnet build`) yang dijalankan sesuai batasan pengguna.
