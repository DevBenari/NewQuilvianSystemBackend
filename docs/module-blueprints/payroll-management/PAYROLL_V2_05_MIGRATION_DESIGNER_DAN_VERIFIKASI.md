# PAYROLL V2 — 05. Migration, Designer, dan Verifikasi

| Field | Nilai |
| --- | --- |
| Dokumen | `PAYROLL_V2_05_MIGRATION_DESIGNER_DAN_VERIFIKASI.md` |
| Tanggal | 2026-11-04 |
| Acuan SHA | BE `b5fb6c251eafcd680ec35483732e3e72226e19f3` (+ perubahan lokal), FE `6a980d8c9dee0cae92c616fcd74f903ec0c67d73` (+ perubahan lokal) |
| Label bukti | `REPOSITORY FACT`, `TARGET PROPOSAL`, `IMPLEMENTED`, `PARTIAL`, `BLOCKED`, `NEEDS CONFIRMATION` |
| Catatan penamaan (11 November 2026) | Bagian 1 sampai 8 menulis keadaan fase 1 dan memakai nama tabel lama (`TrxPayrollRun` dan seterusnya). Migration rename fase 2 (`PAY-MIG-02`) ada di **bagian 9**; setelah ia diterapkan, ganti `Trx*`/`Wfp*` Payroll dengan `Prl*` (ganti awalan saja) pada SQL di dokumen ini |

## 1. Pernyataan penting

1. **Tidak ada** `dotnet ef migrations add`, `dotnet ef database update`, `Update-Database`, `dotnet build`, `dotnet test`, `dotnet run`, `npm run build`, `npm run dev`, atau server/API yang dijalankan selama pekerjaan ini. Tidak ada database yang disentuh, dan tidak ada perintah Git yang mengubah repository (tanpa `add`, `commit`, `push`, `pull`, `checkout`, `stash`).
2. File migration di bawah **ditulis tangan** sesuai permintaan eksplisit pada task `PAY-MIG-01`. Aturan repository backend (`CLAUDE.md`, `DATABASE_RULES.md`) menyatakan pembuatan migration adalah wewenang terpisah; dasar wewenangnya di sini adalah instruksi tertulis pemilik pada prompt ("tulis `.cs`, `.Designer.cs`, dan sinkronkan snapshot bila schema berubah; jangan jalankan perintah EF"). File itu **inert** sampai seseorang yang berwenang menerapkannya. Bila pemilik tidak menghendaki migration ini, hapus tiga file/perubahan pada bagian 3 dan kembalikan tiga baris `HasIndex` pada konfigurasi (daftar di bagian 3.4).

## 2. Apa yang berubah di schema

### 2.1 Pemindahan namespace dan alias tidak memerlukan migration

Tidak memerlukan migration untuk: (a) memindah controller/service/DTO/konstanta ke `Areas/Corporate/PayrollManagement`; (b) menambah rute alias; (c) semua perubahan frontend; (d) parameter kebijakan (`Payroll:Policy`, konfigurasi aplikasi). Alasan: nama tabel, nama kolom, relasi, dan tipe data **tidak berubah**. Entity `Trx*`/`Wfp*` tidak dipindah (fase 2), sehingga nama CLR yang dibaca snapshot juga tidak berubah.

Status kolom (`RunStatus`, `PaymentStatus`, `PayslipStatus`, `InputStatus`) tetap `varchar` bebas; nilai baru (`SentToFinance`, `PartiallyPaid`, `Published`, ...) tidak memerlukan perubahan kolom. Konsep slip, instruksi bayar, pembalikan, dan penyesuaian memakai tabel yang sudah ada, sehingga **tidak ada tabel atau kolom baru**.

### 2.2 Satu-satunya perubahan schema: tiga indeks unik parsial

| No | Tabel (`public`) | Nama indeks | Kolom | Syarat (`filter`) | Tujuan bisnis |
| ---: | --- | --- | --- | --- | --- |
| 1 | `TrxPayrollRun` | `UX_TrxPayrollRun_RegularPeriod` | `PayrollPeriodId` | `"IsDelete" = false AND "RunType" = 'Regular' AND "RunStatus" NOT IN ('Cancelled', 'Reversed')` | Satu periode hanya punya satu run `Regular` yang aktif, bahkan bila dua orang menekan **Buka Payroll Run** hampir bersamaan |
| 2 | `TrxPayrollPayment` | `UX_TrxPayrollPayment_LiveInstruction` | `PayrollRunEmployeeId` | `"IsDelete" = false AND "PaymentStatus" IN ('Scheduled', 'Processing', 'Paid', 'PartiallyReversed')` | Satu penerima tidak pernah punya dua instruksi pembayaran "hidup" sekaligus (gaji tidak dibayar dua kali). Instruksi `Failed`, `Cancelled`, `Reversed` tidak dihitung, sehingga coba ulang tetap bisa |
| 3 | `TrxPayrollVariableInput` | `UX_TrxPayrollVariableInput_MedicalFeeClaim` | `SourceId` | `"IsDelete" = false AND "SourceType" = 'MdfServiceFee' AND "SourceId" IS NOT NULL AND "InputStatus" NOT IN ('Cancelled', 'Rejected')` | Satu hak jasa medis hanya bisa diklaim satu kali ke Payroll |

Detail kolom (nullability, default, FK, tipe) tidak berubah. Spesifikasi sesuai Tahap F prompt:

| Unsur | Nilai |
| --- | --- |
| Schema | `public` |
| Tipe PostgreSQL | tidak berubah (`uuid` untuk ketiga kolom indeks) |
| Default / nullability | tidak berubah |
| Check constraint | tidak ada penambahan |
| FK / `Restrict` | tidak berubah |
| Idempotensi | indeks adalah jaring pengaman terakhir; pemeriksaan bisnis dan transaksi serializable tetap ada di layanan |
| Nilai histori lama | tidak diubah; indeks gagal dibuat bila data lama melanggar (bagian 5) |
| Dampak kebijakan | tidak ada; hanya mencegah duplikasi |
| Rencana rollback | `Down` menghapus tiga indeks; tidak ada data yang hilang |

## 3. File yang dibuat

### 3.1 Tiga file konsisten (bukan designer kosong)

| Berkas | Isi | Ukuran |
| --- | --- | --- |
| `Migrations/20261104071700_AddPayrollIntegrityConstraints.cs` | `Up`: tiga `CreateIndex(... unique: true, filter: ...)`; `Down`: tiga `DropIndex` | 57 baris |
| `Migrations/20261104071700_AddPayrollIntegrityConstraints.Designer.cs` | `[DbContext(typeof(ApplicationDbContext))]`, `[Migration("20261104071700_AddPayrollIntegrityConstraints")]`, `BuildTargetModel` = salinan penuh model target | 142.258 baris |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Model target final: +15 baris (tiga blok `HasIndex` di posisi urut EF) | 142.255 baris |

### 3.2 Konfigurasi EF yang disesuaikan

| Berkas | Perubahan |
| --- | --- |
| `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/TrxPayrollRunConfiguration.cs` | `HasIndex(x => x.PayrollPeriodId).IsUnique().HasFilter(...).HasDatabaseName("UX_TrxPayrollRun_RegularPeriod")` |
| `.../TrxPayrollPaymentConfiguration.cs` | idem untuk `PayrollRunEmployeeId`, `UX_TrxPayrollPayment_LiveInstruction` |
| `.../TrxPayrollVariableInputConfiguration.cs` | idem untuk `SourceId`, `UX_TrxPayrollVariableInput_MedicalFeeClaim` |

`REPOSITORY FACT`: konfigurasi dimuat otomatis lewat `builder.ApplyConfigurationsFromAssembly(...)` (`Repositories/ApplicationDbContext.cs`), dan `DbSet` ketiga entity sudah ada (`TrxPayrollRuns`, `TrxPayrollVariableInputs`, `TrxPayrollPayments`). Pola nama `UX_<Tabel>_<Tujuan>` sama dengan indeks Finance yang sudah ada (`UX_FinPayrollDeductionBatch_RegularPeriod`).

### 3.3 Isi migration (`Up`)

```csharp
migrationBuilder.CreateIndex(
    name: "UX_TrxPayrollPayment_LiveInstruction", schema: "public", table: "TrxPayrollPayment",
    column: "PayrollRunEmployeeId", unique: true,
    filter: "\"IsDelete\" = false AND \"PaymentStatus\" IN ('Scheduled', 'Processing', 'Paid', 'PartiallyReversed')");
// ... dua CreateIndex lain (TrxPayrollRun, TrxPayrollVariableInput) dengan filter pada bagian 2.2
```

### 3.4 Cara membatalkan seluruh perubahan schema (bila tidak dikehendaki)

1. Hapus `Migrations/20261104071700_AddPayrollIntegrityConstraints.cs` dan `.Designer.cs`.
2. Hapus tiga blok `b.HasIndex("PayrollRunEmployeeId") ... UX_TrxPayrollPayment_LiveInstruction`, `b.HasIndex("PayrollPeriodId") ... UX_TrxPayrollRun_RegularPeriod`, dan `b.HasIndex("SourceId") ... UX_TrxPayrollVariableInput_MedicalFeeClaim` dari `ApplicationDbContextModelSnapshot.cs`.
3. Hapus tiga pemanggilan `HasIndex(...)....HasDatabaseName("UX_...")` dari tiga `*Configuration.cs`.

Sistem tetap berfungsi tanpa indeks (pemeriksaan bisnis dan transaksi serializable tetap ada); yang hilang hanya jaring pengaman tingkat database.

## 4. Pemeriksaan statis (QA tanpa menjalankan EF)

| No | Pemeriksaan | Hasil |
| ---: | --- | --- |
| 1 | Timestamp `20261104071700` lebih besar dari migration terakhir (`20261104044009_AddPatEmployeeBenefitCardSync`) dan belum dipakai; nama kelas `AddPayrollIntegrityConstraints` belum ada | Lulus |
| 2 | Designer baru dibandingkan Designer terbaru sebelumnya (`diff`) | Beda hanya: 2 baris atribut/kelas dan 3 blok `HasIndex` (15 baris) |
| 3 | Snapshot (tanpa BOM) dibandingkan Designer baru (`diff`) | Beda hanya 3 perubahan header baku (`using ...Migrations`, `[Migration]`+nama kelas, `BuildTargetModel` + `<inheritdoc />`) — pola identik dengan Designer sebelumnya |
| 4 | Nama indeks dan teks `filter` identik di konfigurasi, snapshot, dan migration (skrip pembanding) | Sama untuk ketiganya |
| 5 | Urutan blok indeks pada snapshot mengikuti aturan pengurutan generator EF (kolom tunggal berurut nama, lalu komposit) | Sesuai (diverifikasi terhadap blok tetangga) |
| 6 | Format berkas: snapshot tetap berBOM; berkas baru ASCII/LF tanpa CR | Lulus |
| 7 | Indeks tidak bentrok nama dengan indeks lain di snapshot | Lulus (nol kemunculan sebelumnya) |
| 8 | `Migrations/History/` (293 Designer) sinkron dengan `MigrationMetadata.g.cs` (293 atribut); target `ValidateMigrationHistoryCoverage` hanya menghitung `History/`, jadi Designer baru di akar tidak memengaruhinya | Lulus |
| 9 | Baseline migration yang ada tidak diubah; tidak ada `DropTable`/`DropColumn` | Lulus |
| 10 | `Program.cs` tidak memanggil `Migrate()`/`EnsureCreated()` — migration tidak berjalan otomatis saat aplikasi start | Lulus |

`Update-MigrationHistory.ps1` (pengarsipan Designer lama ke `History/`) **tidak** dijalankan. Skrip itu opsional: migration baru lahir di `Migrations/` dan ikut dikompilasi. Menjalankannya (oleh pihak berwenang) memindahkan Designer `20261103050001_AddOpticalManagement` ke arsip dan menaikkan hitungan metadata menjadi 294.

`PARTIAL`: kesesuaian dengan hasil `dotnet ef migrations add` sungguhan (urutan anotasi, hash model) **belum dibuktikan** karena perintah EF dilarang. Bila ada selisih kecil, `dotnet ef migrations has-pending-model-changes` oleh pihak berwenang akan menunjukkannya.

## 5. Risiko data lama dan SQL pra-cek

Indeks unik tidak dapat dibuat bila data lama sudah melanggar aturannya; `database update` akan berhenti dengan pesan "could not create unique index ... is duplicated" dan migration dibatalkan sebagai satu transaksi. Jalankan pra-cek **baca-saja** ini terlebih dulu (nama kolom diambil dari konfigurasi EF). Hasil kosong berarti aman.

```sql
-- 1) Lebih dari satu run Regular aktif untuk satu periode
SELECT "PayrollPeriodId", COUNT(*) AS jumlah
FROM public."TrxPayrollRun"
WHERE "IsDelete" = false AND "RunType" = 'Regular' AND "RunStatus" NOT IN ('Cancelled', 'Reversed')
GROUP BY "PayrollPeriodId" HAVING COUNT(*) > 1;

-- 2) Lebih dari satu instruksi pembayaran hidup untuk satu penerima run
SELECT "PayrollRunEmployeeId", COUNT(*) AS jumlah
FROM public."TrxPayrollPayment"
WHERE "IsDelete" = false AND "PaymentStatus" IN ('Scheduled', 'Processing', 'Paid', 'PartiallyReversed')
GROUP BY "PayrollRunEmployeeId" HAVING COUNT(*) > 1;

-- 3) Lebih dari satu klaim aktif untuk satu hak jasa medis
SELECT "SourceId", COUNT(*) AS jumlah
FROM public."TrxPayrollVariableInput"
WHERE "IsDelete" = false AND "SourceType" = 'MdfServiceFee' AND "SourceId" IS NOT NULL
  AND "InputStatus" NOT IN ('Cancelled', 'Rejected')
GROUP BY "SourceId" HAVING COUNT(*) > 1;
```

Bila pra-cek menemukan baris: **jangan menghapus data secara otomatis**. Selesaikan per kasus bersama pemilik proses (mis. membatalkan run kembar lewat aksi **Batalkan Run** yang beraudit, atau menandai instruksi ganda `Cancelled` dengan alasan), lalu ulangi pra-cek. Jangan menimpa atau menghapus bukti pembayaran. Contoh akibat bila diabaikan: run kembar periode Nov 2026 membuat `database update` gagal; belum ada data yang rusak karena migration dibatalkan penuh.

Indeks ini dibuat di tabel transaksi payroll yang kecil; tidak ada rewrite tabel dan tidak ada kolom `NOT NULL` baru, sehingga strategi nullable/backfill/constraint bertahap tidak diperlukan.

## 6. Hasil verifikasi statis yang dijalankan

| Pemeriksaan | Perintah / alat | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| Konsistensi kode BE: `using` hilang/ambigu, arity pemanggilan, kecocokan atribut akses, nama tipe ganda (Swagger), bentrok rute, namespace kosong | skrip analisis statis kustom (`verify_be.py`) | 38 file Payroll, 16 controller → **0 kesalahan, 0 peringatan** (dijalankan ulang setelah perubahan terakhir) | `PASS` (bukan pengganti kompilator) |
| Kontrak FE ↔ BE, navigasi, pengalihan, rute, kesetaraan izin | `node --test tests/unit/payroll-module-contract.test.mjs` | 12 lulus | `PASS` |
| Layar dan aturan Payroll Run, termasuk pembacaan source BE | `payroll-run-screens.test.mjs` | 11 lulus | `PASS` |
| Penjaga potongan cicilan terhadap source BE di path baru | `payroll-installment-deduction-guards.test.mjs` | 6 lulus (sebelumnya terlewat/skip diam-diam karena path lama; kini dialamatkan ke `Areas/Corporate/PayrollManagement/...`) | `PASS` |
| Pengelompokan sidebar HR setelah payroll dikeluarkan | `hr-master-data-sidebar-grouping.test.mjs` | 6 lulus | `PASS` |
| Seluruh suite unit FE (`tests/unit/*.test.mjs`) | `node --test` | Ada kegagalan lama yang tidak terkait Payroll; dibandingkan dengan ekspor bersih SHA dasar (`git archive`): tidak ada kegagalan baru akibat perubahan ini kecuali satu uji billing yang membaca `BillingSettlementService.cs` (tidak disentuh) | `UNRELATED EXISTING ISSUE` |
| Lint | `eslint` pada cakupan yang disentuh | 0 error; 27 peringatan satu aturan `react-hooks/set-state-in-effect` pada hook baru (pola yang sama muncul di hook payroll lama) | `EXISTING WARNING` |
| Resolver impor FE | skrip statis (`verify_fe.py`) | 0 impor baru yang tak terselesaikan dibanding SHA dasar (58 yang lama tidak terkait) | `PASS` |
| Anti-regresi tampilan | tes kontrak (tanpa `<button>`/`<table>` mentah, tanpa warna literal/`!important`, tanpa utilitas tipografi Bootstrap) | Lulus | `PASS` |
| **Build backend oleh pemilik** (2026-11-05, dijalankan pemilik, bukan agen) | `dotnet build ./QuilvianSystemBackend.csproj -c Release --nologo -clp:ErrorsOnly` | **1 error, 128 peringatan.** Error: `CS0509` di `DTOs/PayrollRunDtos.cs(132,56)` — `PayrollRunEmployeeDetailResponse` mewarisi kelas `sealed` `PayrollRunEmployeeResponse`. Diperbaiki dengan melepas `sealed` pada kelas dasar (baris 88). Skrip `verify_be.py` di atas **tidak mendeteksi** kesalahan ini; hanya kompilator yang menemukannya. Peringatan belum dirinci dan belum dipilah mana yang berasal dari modul Payroll. | `FAIL` → diperbaiki, **build ulang belum dibuktikan hijau** |
| Pemeriksaan statis tambahan setelah error di atas (2026-11-05, skrip sekali pakai) | pemindaian: pewarisan dari kelas `sealed`; tipe non-publik yang bocor di tanda tangan publik; nama tipe ganda di dalam dan di luar modul; keberadaan dan jumlah argumen setiap pemanggilan controller → service (44 pemanggilan); nama properti pada 88 object initializer; anggota yang diakses lambda LINQ pada rantai `DbSet` (447 akses); `using` yang diperlukan per file | 0 temuan baru. Satu positif palsu (`Status` sebagai nama properti, bukan tipe) | `PASS` (heuristik; bukan pengganti kompilator) |

Catatan: `verify_be.py` dan `verify_fe.py` adalah skrip bantu sekali pakai di folder kerja sesi dan **tidak** disertakan di repository; keluarannya dicatat di sini sebagai bukti, bukan sebagai alat CI. Pengganti yang resmi tetap `dotnet build` dan CI QBE oleh pihak berwenang.

## 7. Yang sengaja tidak dijalankan

| Pemeriksaan | Alasan | Status |
| --- | --- | --- |
| `dotnet restore/build/test` oleh agen | Dilarang pemilik (`dotnet build` otomatis juga dilarang oleh aturan memori pemilik untuk repo backend). Satu kali build dijalankan pemilik: lihat bagian 6 | `NOT RUN` oleh agen; build pemilik terakhir `FAIL` (CS0509, sudah diperbaiki, belum dibuktikan hijau) |
| `dotnet ef migrations add/list/script`, `dotnet ef database update` | Dilarang pemilik | `NOT RUN` |
| `npm run build`, `npm run dev`, `npm run test:e2e` | Dilarang pemilik / butuh server | `NOT RUN` |
| Uji manual layar (filter, paginasi, kirim, unduh) | Tidak ada server yang boleh dijalankan | `NOT FEASIBLE` |
| Eksekusi ke database bersama | Dilarang | `NOT RUN` |

Akibatnya: kompilasi C# **belum terbukti hijau** (build pemilik menemukan satu error; kompilator berhenti pada kesalahan tahap deklarasi sehingga kesalahan pada isi metode baru bisa muncul setelah build ulang), terjemahan LINQ ke SQL (mis. `statuses.Contains(...)` pada filter status, agregasi rekap), perilaku Npgsql pada transaksi serializable, dan tampilan layar **belum dibuktikan**. Langkah pertama yang disarankan bagi pihak berwenang: `dotnet build`, jalankan SQL pra-cek bagian 5, `dotnet ef migrations has-pending-model-changes`, terapkan migration di lingkungan pengembangan, lalu uji alur UAT (`PAYROLL_V2_04` bagian 8).

## 8. Daftar periksa sebelum merge

- [ ] Build backend dan frontend hijau oleh pihak berwenang.
- [ ] Pra-cek SQL bagian 5 kosong.
- [ ] Pemilik repository memutuskan migration ini diterima atau dibatalkan (bagian 3.4).
- [x] Registry fase 2: baris `Prl` ACTIVE ditulis ke registry repository pada 11 November 2026 (`PAY-BE-12`). Salinan registry pada suite skill menunggu disamakan pemilik.
- [ ] Akses Role: beri izin baru (`PayrollCompensation`, `PayrollPayment`, `PayrollPayslip`, `MyPayslip`, `PayrollEmployeeDeduction`, `PayrollMedicalFeeIntake`, `PayrollReport`, dan aksi baru `PayrollRun`) kepada peran yang sesuai; tanpa itu menu tidak tampil.
- [ ] Keputusan `NEEDS CONFIRMATION` (`PAYROLL_V2_01` bagian 8) dicatat pemilik proses.
- [ ] Fase 2: `dotnet build`, `has-pending-model-changes`, lalu `PAY-MIG-02` diterapkan pada database uji dengan pra-cek dan pasca-cek (bagian 9).

## 9. Fase 2: migration rename tabel ke `Prl*` (`PAY-MIG-02`, 11 November 2026)

Pada fase 2 nama kelas **dan tabel** 19 entity Payroll berganti dari `Trx*`/`Wfp*` ke `Prl*` (`PAYROLL_V2_01` bagian 7.3). Pernyataan di bagian 2.1 bahwa pemindahan tidak memerlukan migration hanya berlaku untuk pemindahan namespace; penggantian nama tabel memerlukannya.

| Hal | Isi |
| --- | --- |
| Berkas | `Migrations/20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.cs` dan `.Designer.cs`; `ApplicationDbContextModelSnapshot.cs` diperbarui |
| Sifat | Seluruhnya `RENAME` (tanpa `DROP`/`CREATE`): 19 tabel, seluruh constraint (PK, unique, FK, CHECK), dan seluruh indeks bernama yang memuat nama tabel lama. Isi dan kolom tidak berubah. `Down` mengembalikan semuanya |
| Cara mengganti nama | Dicari dari katalog PostgreSQL dengan pencocokan token (`(^|_)token(?=_|~|$)`), bukan substring, agar `Down` tidak salah memetakan `PrlPayroll` ke `WfpPayroll` pada nama `PrlPayrollRun`. Awalan tiga huruf diganti tiga huruf, jadi panjang nama dan titik potong `~` tidak berubah |
| Bukti | Simulasi pada 10.499 nama: 10.499 dari 10.499 sama dengan nama yang diharapkan model baru; `Up` lalu `Down` kembali ke semula. **Bukan eksekusi PostgreSQL** |
| Status | 🟡 ditulis dan lolos simulasi statis; **belum dijalankan terhadap database** (dilarang) |
| Pra-cek, pasca-cek, uji `Down`/`Up` | Skrip siap pakai ada di [laporan `PAY-MIG-02`](task/report/backend/PAY-MIG-02.md) bagian 6. SQL pra-cek duplikasi di bagian 5 dokumen ini memakai nama tabel lama dan berlaku **sebelum** migration rename diterapkan |
| Rilis | Satu paket: database (migration) lebih dulu, baru aplikasi. Aplikasi hasil build fase 2 gagal total terhadap database yang belum dimigrasi, dan sebaliknya |
