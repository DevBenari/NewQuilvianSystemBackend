# Laporan Perubahan Backend — `BE-FIN-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-002` |
| Judul | Entity dan EF configuration data induk Finance |
| Slice | `MVP-0` — Fondasi data induk (`EPIC FIN-01`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-0`) |
| Trace | `FIN-DES-001`, `FIN-DES-003`; kontrak `FIN-VAL-1.0` §data induk; `00-delivery-roadmap.md` bagian 5 baris `FR-FIN-001`..`004` |
| Contract version | `FIN-VAL-1.0` (locked 20 September 2026) — dipatuhi untuk bagian yang dikerjakan; **belum** menjawab satu permukaan yang ternyata bertabrakan (lihat bagian 1) |
| Dependency | `BE-FIN-001` — ✅ selesai 21 September 2026, lihat [laporan](BE-FIN-001.md) |
| Klasifikasi | `MEDIUM` — satu repository; 7 berkas diubah/dibuat; logika sederhana (data induk murni, tanpa service/controller); ada dampak database (entity + FK baru) tetapi belum migration; tidak ada dampak keamanan/auth |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/MasterData/Models/`, `Repositories/Configurations/Corporate/FinanceManagement/MasterData/`, `Repositories/ApplicationDbContext.cs` (registrasi `DbSet` saja). Tidak ada migration, tidak ada eksekusi database |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN.** 3 dari 4 entity yang direncanakan data-dictionary dibuat (`MstBankAccount`, `MstCurrency`, `MstExchangeRate`); `MstBank` **tidak dibuat sebagai entity baru** karena sudah ada dan aktif di `Areas/Administrator/MasterData` — direktifkan pemilik repository untuk dipakai ulang apa adanya (21 September 2026). Seluruh acceptance criteria yang berlaku pada permukaan yang tersisa terpenuhi |

---

## 1. Masalah yang diperbaiki / temuan yang mengubah cakupan

Data dictionary (`erd/data-dictionary.md` §6) dan ERD (`erd/cash-and-master-data.md` §2) merancang
`MstBank` sebagai entity **baru** milik Finance (`Areas/Corporate/FinanceManagement/MasterData`),
mengikuti scope decision `FIN-SC-006` ("Finance-owned master data: Bank/BankAccount") dan
`FIN-DEC-018`.

Saat memeriksa source sebelum menulis model pertama (langkah wajib `AGENTS.md` §*Aturan DTO,
Model, dan Validasi*), ditemukan bahwa **`MstBank` sudah ada dan aktif**:

- `Areas/Administrator/MasterData/Models/MstBank.cs` — `[Table("MstBank", Schema = "public")]`,
  tabel yang **persis sama namanya** dengan yang dirancang data-dictionary Finance.
- `Areas/Administrator/MasterData/Controllers/BankController.cs` — endpoint aktif
  `api/v1/administrator/master-data/banks`, CRUD penuh, bukan kode mati.
- `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpBankAccount.cs` — sudah mengonsumsi
  `MstBank` ini lewat FK `BankId` (lihat `WfpBankAccountConfiguration.cs` baris 31:
  `HasOne(x => x.Bank).WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict)`).

Membuat `MstBank` baru di Finance dengan nama tabel yang sama akan bertabrakan langsung pada
lapis database (dua `IEntityTypeConfiguration` tak berelasi memetakan CLR type berbeda ke tabel
fisik `"MstBank"` yang sama), dan `01-existing-capability-map.md` — dokumen yang seharusnya
menangkap capability existing seperti ini sebelum blueprint disusun — **tidak menyebut `MstBank`
sama sekali**. Ini gap pada audit kapabilitas blueprint, bukan sesuatu yang boleh saya perbaiki
sendiri dengan mengarang keputusan.

Temuan ini dilaporkan ke pemilik repository di tengah sesi (bukan diselesaikan sepihak), mengikuti
`AGENTS.md`: *"Source backend saat ini adalah bukti otoritatif atas perilaku runtime... berhenti
ketika status atau snapshot source-nya tidak lagi berlaku."* Pemilik repository memutuskan:

> **Keputusan (pemilik repository, 21 September 2026):** Bila sudah ada master data yang
> menyimpan `MstBank`, pakai yang lama. Finance **tidak** membuat `MstBank` baru.

Keputusan ini persis mengikuti preseden `FIN-DEC-014` (Finance AP memakai `MstSupplier` existing
milik Administrator apa adanya lewat `SupplierId`, tidak membuat master Supplier baru) — pola yang
sama, diterapkan pada objek yang berbeda.

**Konsekuensi konkret:**

- `MstBank` **tidak** dibuat sebagai entity/tabel baru di Finance.
- `MstBankAccount` (baru, milik Finance) dibuat dengan `BankId` merujuk **`MstBank` milik
  Administrator** (`Areas.Administrator.MasterData.Models.MstBank`), mengikuti pola FK yang
  sudah terbukti jalan pada `WfpBankAccountConfiguration` (FK sungguhan dengan
  `OnDelete(DeleteBehavior.Restrict)`, bukan referensi lunak tanpa FK — beda dengan pola
  referensi ke tabel transaksional modul lain seperti `SourceHandoffId` pada
  `FinBillingHandoffIntake`, yang sengaja tanpa FK).
- **Delta terhadap kontrak yang disetujui**: `data-dictionary.md` §6.1/6.2, `erd/cash-and-master-
  data.md` §2, dan scope decision `FIN-SC-006` masih menuliskan `MstBank` sebagai data baru milik
  Finance. Delta ini **perlu diratifikasi balik** ke `02-backend-architecture.md`/`erd/*`/
  `00-interview-decisions.md` oleh pemilik blueprint (Yasmin) supaya dokumen kontrak tidak lagi
  berbeda dari source yang sebenarnya berjalan. Saya tidak mengubah dokumen blueprint tersebut
  pada task ini karena itu di luar wewenang tulis `BACKEND MODE` (bukan `MODULE BLUEPRINT MODE`).

---

## 2. Proses bisnis

Task ini murni fondasi data (model + EF configuration), tidak ada endpoint atau workflow pengguna
akhir yang berjalan pada task ini (itu `BE-FIN-004`). Alurnya:

1. **Pemicu.** `BE-FIN-002` giliran berikutnya di `MVP-0` setelah `BE-FIN-001` (registry) selesai.
2. **Langkah:**
   a. Baca `erd/data-dictionary.md` §6 dan `erd/cash-and-master-data.md` §2 untuk bentuk kolom,
      index, dan constraint keempat entity yang dirancang.
   b. Periksa source existing untuk masing-masing nama entity sebelum menulis model pertama —
      menemukan tabrakan `MstBank` (bagian 1 di atas). `MstBankAccount`, `MstCurrency`,
      `MstExchangeRate` dikonfirmasi **belum ada** di source mana pun.
   c. Setelah arahan pemilik repository turun, buat tiga entity (`MstBankAccount`, `MstCurrency`,
      `MstExchangeRate`) beserta `IEntityTypeConfiguration`-nya, mengikuti pola
      `MstPettyCashCategory`/`MstPettyCashCategoryConfiguration` (satu-satunya preseden data induk
      Finance yang sudah ada) dan pola FK lintas-domain `WfpBankAccountConfiguration` untuk
      `MstBankAccount.BankId`.
   d. Daftarkan ketiga `DbSet` baru di `ApplicationDbContext.cs`, dengan komentar yang menjelaskan
      kenapa `MstBank` tidak ikut didaftarkan.
   e. Jalankan `tooling/qbe/Invoke-QbeConformanceCheck.ps1` untuk memverifikasi ketujuh berkas
      (3 model + 3 configuration + `ApplicationDbContext.cs`) tidak melanggar `QBE-ENT-001`,
      `QBE-CFG-001`, `QBE-MOD-002` (lihat bagian 5).
3. **Hasil akhir.** Tiga entity data induk baru siap menjadi dasar migration (`BE-FIN-003`,
   terpisah), dengan `MstBank` yang sudah ada tetap menjadi satu-satunya master Bank di sistem.
4. **Jalur tidak normal.** Temuan tabrakan `MstBank` adalah jalur tidak normal task ini — ditangani
   dengan berhenti dan melapor balik ke pemilik repository, bukan improvisasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §6, `erd/cash-and-master-data.md` §2
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §1.1, §2.1 (`FIN-DES-001`, `003`)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (`FIN-SC-006`, `FIN-DEC-014`, `FIN-DEC-018`)
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` (dicek — tidak menyebut `MstBank`)
- `Areas/Corporate/FinanceManagement/MasterData/Models/MstPettyCashCategory.cs` + `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstPettyCashCategoryConfiguration.cs` (pola reuse)
- `Areas/Administrator/MasterData/Models/MstBank.cs` + `Areas/Administrator/MasterData/Controllers/BankController.cs` (bukti `MstBank` sudah ada dan aktif)
- `Areas/Corporate/HumanResource/WorkforceCore/Models/WfpBankAccount.cs` + `Repositories/Configurations/Corporate/HumanResource/WorkforceCore/WfpBankAccountConfiguration.cs` (pola FK lintas-domain ke `MstBank`)
- `Repositories/ApplicationDbContext.cs` (titik registrasi `DbSet`, pola `ApplyConfigurationsFromAssembly`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstBankAccount.cs` | **Baru.** Entity rekening bank Finance; `BankId` merujuk `MstBank` milik Administrator |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstCurrency.cs` | **Baru.** Entity mata uang Finance |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstExchangeRate.cs` | **Baru.** Entity kurs harian per mata uang |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstBankAccountConfiguration.cs` | **Baru.** FK ke `MstBank` Administrator (`Restrict`), unique index `(BankId, AccountNumber)` filter `IsDelete=false`, check constraint `AccountType` |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstCurrencyConfiguration.cs` | **Baru.** Unique index `CurrencyCode`, unique partial index `IsBaseCurrency` (hanya satu true) |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstExchangeRateConfiguration.cs` | **Baru.** `HasPrecision(18,2)` pada `BuyRate`/`SellRate`/`MiddleRate`, FK ke `MstCurrency` (`Restrict`), unique index `(CurrencyId, RateDate)` |
| `Repositories/ApplicationDbContext.cs` | Registrasi `DbSet<MstBankAccount>`, `DbSet<MstCurrency>`, `DbSet<MstExchangeRate>` + komentar penjelas kenapa `MstBank` tidak ikut didaftarkan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint pada task ini (`BE-FIN-004`) |
| Database | Tiga entity baru siap-migration (`MstBankAccount`, `MstCurrency`, `MstExchangeRate`). **Belum ada migration** — itu `BE-FIN-003`, terpisah, dan menunggu otorisasi migration tersendiri. `MstBank` **tidak** berubah — tetap tabel Administrator apa adanya |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi. Catatan untuk `BE-FIN-004`: `MstBankAccount.AccountNumber` bertanda **Sensitif** pada data-dictionary — MUST NOT masuk custom logger, SHOULD ditinjau masking-nya pada response DTO nanti |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun controller.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan** | `NOT RUN` | Atas instruksi eksplisit pengguna pada sesi ini ("jangan build secara automatis", "biarkan saya coba sendiri") — pengguna memvalidasi build secara mandiri. Ini bukan pelanggaran `TEST_POLICY.md` (build tetap wajib untuk task yang menyentuh source), melainkan penangguhan eksekusi build oleh pemilik repository sendiri; hasil build sebenarnya akan disusulkan pengguna |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` (mode `ReportOnly`, scope `WorkingTree`) | `Files evaluated: 7`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran perintah dikutip di bawah |
| Review manual: tidak ada `MstBank` baru dibuat, tidak ada duplikasi tabel | Dikonfirmasi — hanya 3 model + 3 configuration baru, `MstBank` Administrator tidak disentuh | `PASS` | `git status --short` bagian 7; isi berkas pada bagian 3.2 |
| Review manual: pola FK lintas-domain `MstBankAccount.BankId` konsisten dengan `WfpBankAccountConfiguration` | `HasOne(x => x.Bank).WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict)` — identik strukturnya | `PASS` | Perbandingan kode pada bagian 3.1 |
| Review konfigurasi terhadap acceptance criteria (`Prefix Mst`; partial unique index `WHERE "IsDelete" = false`; `HasPrecision(18,2)`) | Ketiganya terpenuhi pada entity yang dibuat | `PASS` | Lihat bagian 6 |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 7
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — tidak ada UI/endpoint untuk diuji manual pada task ini.

**Tidak dijalankan:** `dotnet build`/`dotnet test` (lihat tabel di atas — ditangguhkan atas
permintaan pengguna). Eksekusi migration/database — `NOT APPLICABLE`, belum ada migration.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Prefix `Mst` | Terpenuhi untuk `MstBankAccount`, `MstCurrency`, `MstExchangeRate` | Nama class dan `[Table(...)]` pada bagian 3.2 |
| Partial unique index `WHERE "IsDelete" = false` | Terpenuhi | `IX_MstBankAccount_Bank_AccountNumber`, `IX_MstCurrency_CurrencyCode`, `IX_MstCurrency_BaseCurrency`, `IX_MstExchangeRate_Currency_RateDate` — seluruhnya `HasFilter("\"IsDelete\" = false")` (dan `IsBaseCurrency` menambah `AND "IsBaseCurrency" = true`) |
| `HasPrecision(18,2)` | Terpenuhi pada `MstExchangeRate.BuyRate/SellRate/MiddleRate` (satu-satunya kolom uang pada keempat entity data induk) | `MstExchangeRateConfiguration.cs` |
| DoD: `IdentityModel` diwarisi | Terpenuhi — ketiga entity `: IdentityModel` | `Models/*.cs` |
| DoD: nol hard delete | Terpenuhi — tidak ada kode `Remove`/`DELETE FROM` pada task ini; seluruh soft-delete mewarisi `IsDelete` dari `IdentityModel` | Tidak ada service/controller ditulis pada task ini yang menghapus baris |
| Cakupan roadmap: `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate` + 4 configuration | 🟡 **Sebagian.** 3 dari 4 entity + 3 dari 4 configuration. `MstBank` sengaja tidak dibuat — lihat bagian 1 untuk temuan dan keputusan pemilik repository | Bagian 1; `git status --short` bagian 7 |

Kriteria yang **belum** terpenuhi: cakupan "4 entity + 4 configuration" pada roadmap tidak
tercapai penuh karena `MstBank` memang tidak boleh dibuat ulang (bukan kelalaian implementasi).
Seluruh kriteria kualitas (prefix, index, precision, `IdentityModel`, soft-delete) terpenuhi penuh
untuk permukaan yang dikerjakan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `data-dictionary.md` §6.1/6.2, `erd/cash-and-master-data.md` §2, dan `FIN-SC-006` pada `00-interview-decisions.md` masih menuliskan `MstBank` sebagai data baru milik Finance — **berbeda dari keputusan yang sebenarnya dijalankan** pada task ini. Dokumen blueprint tersebut PERLU diperbarui pemilik blueprint (Yasmin) agar tidak menyesatkan pembaca berikutnya; task ini tidak mengubahnya karena di luar wewenang tulis `BACKEND MODE` |
| Masalah yang diketahui | `01-existing-capability-map.md` tidak menangkap `MstBank` existing milik Administrator saat audit kapabilitas awal blueprint disusun — gap pada proses capability-audit, dicatat di sini sebagai temuan, bukan diperbaiki sendiri |
| Risiko tersisa | Rendah. `MstBankAccount.BankId` kini bergantung pada `MstBank` milik Administrator tetap stabil (`OnDelete Restrict` mencegah bank yang sudah dipakai rekening Finance terhapus). Tidak ada risiko duplikasi data |
| Perubahan sampingan | `NONE` |
| Interupsi | Task diinterupsi pengguna di tengah sesi ("jangan build secara automatis", "biarkan saya coba sendiri") sebelum laporan ditulis. Dipulihkan dengan menanyakan arah lanjutan lewat `AskUserQuestion`; pengguna memilih menyelesaikan laporan ini sekarang sekaligus memutuskan resolusi `MstBank`. Tidak ada pekerjaan yang dibatalkan atau ditulis ulang akibat interupsi ini |
| Status Git | Lihat keluaran `git status --short` berikut: `M Repositories/ApplicationDbContext.cs`, `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (dari `BE-FIN-001`), `M docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md` (dari `BE-FIN-001`), `M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (dari `BE-FIN-001`), enam berkas baru (`??`) untuk tiga model + tiga configuration `BE-FIN-002`, dan folder laporan baru |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` secara mandiri untuk memverifikasi kompilasi. (2) Pemilik blueprint meratifikasi delta `MstBank` pada `data-dictionary.md`/`erd/cash-and-master-data.md`/`00-interview-decisions.md` (`FIN-SC-006`). (3) `BE-FIN-003` (migration `AddFinanceMasterData`) dapat dilanjutkan — cakupannya kini 3 tabel baru (`MstBankAccount`, `MstCurrency`, `MstExchangeRate`), bukan 4, dan tetap menunggu otorisasi migration terpisah yang belum diberikan |
