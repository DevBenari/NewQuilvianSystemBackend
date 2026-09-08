# Accounting — Existing Capability Map

Berkas ini mencatat keadaan **apa adanya** di kode saat ini, bukan rancangan target.

| Field | Value |
|---|---|
| Status peta | `PARSIAL` — pemeriksaan terarah, belum audit penuh `/trace-existing-capabilities` |
| Revision | `2` |
| Backend source SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend source SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat dokumen ini disusun**. Baseline blueprint kini `31a82c8` (`QuilvianIntegrationFrontend`); kutipan di bawah tetap berlaku, lihat `evidence/02-frontend-rebaseline-impact-scan.md` |
| Cara pemeriksaan | Pencarian nama entity, `DbSet`, folder, dan kolom pada kedua repository |

Peta ini berasal dari pemeriksaan terarah. Cukup untuk mencegah pembangunan ganda dan untuk
menarik batas MVP, tetapi belum cukup dipakai sebagai bukti acceptance.

## Ringkasan kemampuan

| ID | Kebutuhan | Owner | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `ACC-CAP-001` | Chart of Accounts (daftar akun) | Belum ada | Tidak ditemukan entity `ChartOfAccount` pada `NewQuilvianSystemBackend@aa837d7` | `MISSING` | Dibangun baru | Rendah |
| `ACC-CAP-002` | Journal dan Journal Line | Belum ada | Tidak ditemukan entity `Journal`/`JournalLine` pada `NewQuilvianSystemBackend@aa837d7` | `MISSING` | Dibangun baru | Rendah |
| `ACC-CAP-003` | General Ledger (buku besar) | Belum ada | Tidak ditemukan entity `GeneralLedger` pada `NewQuilvianSystemBackend@aa837d7` | `MISSING` | Dihitung dari jurnal yang sudah disahkan, tanpa tabel terpisah | Rendah |
| `ACC-CAP-004` | Accounting Period (periode akuntansi) | Belum ada | Tidak ditemukan entity `AccountingPeriod` pada `NewQuilvianSystemBackend@aa837d7` | `MISSING` | Dibangun baru | Rendah |
| `ACC-CAP-005` | Halaman akuntansi di aplikasi web | Belum ada | Tidak ditemukan folder akuntansi/keuangan pada `QuilvianSystemFrontendDev/src@fc49cc7` | `MISSING` | Dibangun baru | Rendah |
| `ACC-CAP-006` | Titik sentuh dengan Billing | Billing | `docs/module-blueprints/billing-kasir/contracts/integration-contract.md#BIL-INT-007..009@aa837d7` | `CONFLICT` | Perlu keputusan lintas modul `ACC-XM-001` | **Tinggi**, tetapi hanya menyentuh Phase 2 |
| `ACC-CAP-007` | Master Cost Center | Corporate / Human Resource / Master Data / Organization | `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs@aa837d7` | **`READY TO REUSE`** | Dirujuk lewat `CostCenterId`, **tidak** disalin | Rendah |
| `ACC-CAP-008` | Master Badan Hukum dan Lokasi | Corporate / Master Data | `Repositories/ApplicationDbContext.cs#MstLegalEntities@aa837d7`, `#MstHospitalSites@aa837d7` | **`READY TO REUSE`** | Dirujuk lewat `LegalEntityId` | Rendah |
| `ACC-CAP-009` | Pencatatan jejak audit | Platform | `Services/Logging/LoggerService.cs@aa837d7` | **`READY TO REUSE`** | Dipakai apa adanya; Accounting tidak membuat tabel audit sendiri | Rendah |
| `ACC-CAP-010` | Mekanisme hak akses | Platform | `Repositories/ApplicationDbContext.cs`, atribut `[AccessController]`/`[AccessPermission]`@aa837d7 | **`READY TO REUSE`** | Nilai `Resource`/`Action` baru didaftarkan mengikuti pola yang ada | Rendah |
| `ACC-CAP-011` | Rekening deposit dan rekening bank yang sudah ada | Billing dan Workforce | `Repositories/ApplicationDbContext.cs#BilDepositAccounts@aa837d7`, `#WfpBankAccounts@aa837d7` | `READY TO REUSE` sebagai rujukan | Bukan buku besar; hanya dirujuk, tidak diambil alih | Rendah |
| `ACC-CAP-012` | Slot folder backend untuk Accounting | Belum ada | `Areas/Corporate/` hanya berisi `HumanResource@aa837d7` | `MISSING` | Folder baru, mengikuti pola `HumanResource` | Sedang — pemindahan folder memicu pemeriksa QBE |
| `ACC-CAP-013` | Komponen tabel, penyaring, dan form di frontend | Platform frontend | `src/components/features/base-features/data-table.jsx`, `data-filter.jsx`@fc49cc7 | **`READY TO REUSE`** | Dipakai apa adanya | Rendah |
| `ACC-CAP-014` | Pola slice CRUD master data | Platform frontend | `src/lib/state/slice/master-data-resource-slice-factory.jsx`@fc49cc7 | **`READY TO REUSE`** | Dipakai untuk slice COA | Rendah |

## Temuan yang mengubah rancangan

### `MstCostCenter` sudah ada, dan memang disiapkan untuk akuntansi

Ini temuan terpenting pada revisi ini. `ACC-DEC-019` mewajibkan Cost Center pada baris jurnal
akun beban, dan sempat diperkirakan masternya belum ada sehingga Accounting harus membuatnya.
Ternyata sudah ada:

```
Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs
```

Isinya, apa adanya pada `aa837d7`:

| Kolom | Catatan |
|---|---|
| `Id` | Kunci utama |
| `LegalEntityId` | Wajib — inilah yang memicu `ACC-OQ-037` |
| `HospitalSiteId`, `OrganizationUnitId`, `DepartmentId` | Boleh kosong |
| `CostCenterCode`, `CostCenterName` | Wajib |
| **`AccountingCode`** | Boleh kosong, panjang 100 |
| `EffectiveStartDate`, `EffectiveEndDate` | Masa berlaku |
| `IsActive` | Penanda aktif |

**Akibatnya bagi Accounting:** modul ini **tidak boleh** membuat tabel Cost Center sendiri.
Baris jurnal menyimpan `CostCenterId` sebagai foreign key ke `MstCostCenter`. Hal ini dicatat
pada bagian "Yang sengaja tidak dibuat" di
[02-backend-architecture.md](02-backend-architecture.md).

**Satu hal yang perlu diperjelas nanti.** Kolom `AccountingCode` pada `MstCostCenter` tampaknya
menyimpan kode akun dalam bentuk teks bebas. Setelah Accounting menjadi pemilik resmi COA
(`ACC-DEC-002`), makna kolom itu menjadi kabur: apakah ia rujukan ke `AccChartOfAccount`, atau
peninggalan sistem lama. Ini **tidak memblokir MVP**, karena Accounting tidak membacanya. Dicatat
sebagai pertanyaan lanjutan bagi pemilik Human Resource.

### Modul Accounting benar-benar dimulai dari nol

Tidak ditemukan satu pun tabel atau layar akuntansi di kedua repository. Ini kabar baik untuk
`ACC-DEC-002`, yang melarang adanya buku besar tandingan: memang belum ada yang menyaingi.

Dua tabel bernama mirip ditemukan, dan keduanya **bukan** buku besar:

| Tabel | Milik modul | Isinya | Kesimpulan |
|---|---|---|---|
| `BilDepositAccounts` | Billing | Titipan uang muka pasien | Sumber kejadian keuangan, bukan buku besar |
| `WfpBankAccounts` | Workforce/HR | Nomor rekening bank pegawai | Data induk pegawai, bukan buku besar |

Contoh bedanya: `BilDepositAccounts` mencatat "pasien Budi menitipkan Rp 2.000.000 sebagai uang
muka". Buku besar akuntansi mencatat hal berbeda, yaitu "Kas bertambah Rp 2.000.000 di sisi
debit, dan Titipan Pasien bertambah Rp 2.000.000 di sisi kredit". Yang pertama transaksi
operasional milik Billing; yang kedua pencatatan akuntansi milik Accounting.

### Pola badan hukum adalah kebiasaan domain Corporate

Kolom `LegalEntityId` ditemukan pada **83 berkas**, hampir seluruhnya di bawah
`Areas/Corporate/`. Modul Billing tidak memakainya sama sekali. Karena Accounting berada di
domain Corporate dan wajib merujuk `MstCostCenter` yang mensyaratkan kolom itu, pola yang sama
diikuti. Keputusannya `ACC-DEC-037`.

### Peringatan soal pemindahan folder

Pemeriksa kesesuaian QBE membandingkan berkas berdasarkan lokasinya dan tidak mengenali bahwa
sebuah berkas hanya dipindah. Berkas yang cuma berpindah folder dinilai sebagai kode baru dan
seluruh isinya diperiksa ulang.

Karena Accounting dibangun dari nol, ini tidak menjadi masalah **selama tidak ada berkas modul
lain yang ikut dipindahkan**. Bila muncul usulan menata ulang folder demi Accounting, jalankan
pemeriksa lebih dulu dan hitung dampaknya.

## Kapan peta ini harus diperbarui

Tandai peta ini `STALE` dan jalankan tinjauan dampak terbatas bila SHA salah satu repository
berubah. Untuk audit penuh, jalankan `/trace-existing-capabilities`.
