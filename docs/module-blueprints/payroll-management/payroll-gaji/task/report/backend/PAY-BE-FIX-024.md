# PAY-BE-FIX-024 — Komponen Dasar dan Tipe Komponen bukan input pengguna (backend)

Tanggal: 23 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `ab340bc7` (tidak berubah; working tree memuat pekerjaan sebelumnya yang tidak disentuh). Lanjutan PAY-BE-FIX-023. Pasangan: PAY-FE-FIX-024.

`AUTOMATED TEST: NOT APPLICABLE` (backend). Tes teks sisi frontend yang membaca source backend: 16 lulus (`payroll-component-category-refactor.test.mjs`). `dotnet build`, migration, database, dan runtime: NOT RUN.

## Preflight
Area Corporate / HumanResource / MasterData / PayrollAndBenefit; prefix `Mst` (existing); TOUCHED LEGACY; tanpa entity baru, tanpa DDL, tanpa snapshot. QBE: QBE-API-001, QBE-PERM-001 (atribut Access tidak diubah), QBE-VAL-001, QBE-DTO-001.

## Klasifikasi dependensi (Tahap 1)

| Kelompok | `BaseComponentId` | `ComponentType` |
| --- | --- | --- |
| A. Input pengguna | Ya, dihapus | Ya, dihapus (sudah diturunkan sejak FIX-023) |
| B. Kolom database | Tetap (nullable) | Tetap (wajib) |
| C. Kalkulasi bisnis | **Tidak dibaca** mesin payroll (hanya controller master) | **Dibaca**: `PayrollCalculationService`, `PayrollVariableInputService` (Earning), `PayrollMedicalFeeIntakeService` (Earning), `PayrollReportService`, `FinanceReceivablePayrollSyncService` (Deduction), `LeavePayrollIntegrationService`, `AllowanceTypeController`, `DeductionTypeController` |
| D. Consumer API lain | Respons komponen | Respons komponen dan **options**: editor Jenis Tunjangan/Potongan memfilter opsi komponen memakai `componentType` |
| E. Relasi historis | Proteksi hapus + hitungan turunan | Klasifikasi legacy tidak selaras dipertahankan |

## Perubahan

| File | Perubahan |
| --- | --- |
| `Controllers/PayrollComponentController.cs` | Create: `BaseComponentId = null` (request diabaikan). Update: `entity.BaseComponentId` tidak disentuh sehingga relasi historis tidak terhapus. `ValidateRequestAsync`: blok validasi komponen dasar (ada/aktif/siklus/keselarasan tipe) dihapus karena jalurnya tak terjangkau lagi; guard reklasifikasi (dipakai Jenis Tunjangan/Potongan, turunan bertipe lain) dipertahankan. Metadata tanpa `ComponentTypeOptions` dan tanpa sortir `componentType`. Helper mati dihapus: `CreatesCircularReferenceAsync`, `NormalizeGuid`, `BuildComponentTypeLabel` |
| `DTOs/PayrollComponentDtos.cs` | `ComponentTypeOptions` dan `DefaultFilter.ComponentType` dihapus dari metadata. `BaseComponentId` dan `ComponentType` pada request tetap ada sebagai properti opsional (kompatibilitas klien lama) |

Klasifikasi otomatis (sudah dari FIX-023, tidak berubah): kategori ditemukan/aktif/tidak terhapus -> `ComponentType` = `ComponentGroup` kategori (Earning, Deduction, EmployerContribution, Information); kategori tidak ada/nonaktif -> 400 "Payroll component category tidak ditemukan atau tidak aktif." sebelum entity dibuat; `ComponentType` pada request, bila dikirim dan bertentangan, ditolak 400 (bukan sumber klasifikasi). Pergantian kategori memakai grup kategori tujuan; guard menolak bila komponen dipakai Jenis Tunjangan/Potongan atau punya turunan bertipe lain. Tidak ada koreksi massal data lama. Perubahan `ComponentGroup` kategori tidak mungkin dari update kategori (tidak pernah ditimpa, FIX-023).

## Kontrak
- Request create/update komponen: `baseComponentId` diabaikan, `componentType` opsional. Contoh: `{ "payrollComponentCategoryId": "<guid>", "payrollComponentName": "Potongan Terlambat", "calculationMethod": "Fixed", "defaultAmount": 0, "defaultPercentage": 0, "isRecurring": true, "isTaxable": false, "isProrated": false, "isDisplayedOnPayslip": true, ... }` -> respons 200, komponen tersimpan dengan `componentType` = kelompok kategori.
- Respons komponen: **tidak berubah** (`componentType`, `baseComponent*`, `derivedComponentCount` tetap ada untuk consumer lain).
- Metadata: `componentTypeOptions` dan sortir `componentType` dihapus. Parameter filter `componentType` pada GET list/options tetap diterima (kompatibilitas).
- Izin dan audit logging tidak berubah.

## Matriks TC (statis)

| TC | Status | Bukti |
| --- | --- | --- |
| 05 | VERIFIED STATIC (BE) / NEEDS RUNTIME UAT | Request tanpa kedua field valid: `ComponentType` string? opsional, `BaseComponentId` diabaikan |
| 06-08 | VERIFIED STATIC | `componentType = categoryGroup`; keempat kelompok ada di `AllowedComponentTypes`. Catatan: kategori legacy EmployerContribution/Information tidak dapat dipilih untuk komponen BARU (keputusan FIX-023: hanya lima kategori operasional); komponen lama dengan kategori itu tetap benar. **NEEDS CONFIRMATION** bila komponen baru bertipe itu dibutuhkan |
| 09 | VERIFIED STATIC | Update tidak meng-assign `BaseComponentId`; tes |
| 10 | VERIFIED STATIC | Summary, filter lain, paginasi, status tidak diubah; sortir `componentType` tetap berfungsi bila diminta langsung |
| 11 | VERIFIED STATIC | Pesan kategori tidak ditemukan/nonaktif, sebelum penyimpanan |
| 12 | VERIFIED STATIC | Respons options membawa `ComponentType` untuk editor Jenis Tunjangan/Potongan; mesin payroll tidak disentuh |
| 14 | VERIFIED STATIC | Tanpa migration/DDL |
| 15 | VERIFIED STATIC | Rute, `[AccessPermission]` (5 Read, 1 Create, 2 Update, 1 Delete), `LoggerService` tidak berubah |

## Risiko dan butir terbuka
- Komponen turunan lama tetap tersimpan dan tampil di respons; tidak ada lagi jalur UI untuk mengubah relasi itu (**NEEDS CONFIRMATION**: keputusan membersihkan relasi historis belum ada; tidak dilakukan).
- Aturan keselarasan tipe komponen dasar dari FIX-023 dihapus karena komponen dasar tidak bisa lagi diubah lewat API.
- `dotnet build` belum dijalankan.
