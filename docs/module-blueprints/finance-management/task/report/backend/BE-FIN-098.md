# Laporan Perubahan Backend — `BE-FIN-098`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-098` |
| Judul | Batch impor saldo lama piutang karyawan divalidasi ke NIP master pegawai HR dan dipetakan ke identitas debitur |
| Slice | `REV-18B2` — Perjanjian Cicilan, Clearance, Pelunasan & Migrasi |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-235`..`237`; `FIN-DEC-196`; `02-backend-architecture.md` P.8 |
| Contract version | `FIN-API-1.9` §P.2 (`draft`); `FIN-VAL-1.11` `FIN-VAL-246` (`approved`); `FIN-PERM-1.10` P.2 |
| Dependency | `BE-FIN-091` 🟡 — belum ✅ (tidak signifikan untuk task ini, sama seperti alasan `BE-FIN-096` §0 — `DebtorType`/`BenefitOwnerId` sudah ada sejak `BE-FIN-079`) |
| Klasifikasi | `MEDIUM` — 1 endpoint baru, 1 service baru berdiri sendiri (keputusan eksplisit pengguna), parsing CSV/XLSX sendiri, nol entity baru, nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,DTOs,Services}/`, `Program.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis.** `dotnet build` **TIDAK DIJALANKAN**. Task ke-8 berturutan tanpa build |

---

## 0. Keputusan scope — WAJIB dibaca

**Temuan sebelum menulis kode:** `FinanceOpeningItemBatchService.cs` (1117 baris) sudah punya mekanisme migrasi tagihan lama generik (`BE-FIN-079` dst.), tetapi **sengaja menolak** `DebtorType = EMPLOYEE_BENEFIT` sejak `BE-FIN-082` (komentar eksplisit di source: template CSV generik tidak punya kolom NIP). Kolom yang dibutuhkan task ini (`NomorKartuLama`, `NIP`, `HubunganKeluarga`, dst. — `data-dictionary.md` §8.2) juga berbeda total dari template generik (`NomorDokumen`, `JenisDebitur`, dst).

**Saya tanyakan ke pengguna** sebelum menulis kode: memperluas `FinanceOpeningItemBatchService` existing (DRY, tapi 5 titik dispatch di file 1117-baris yang sudah dipakai 7 task sebelumnya dan belum pernah di-build sesi ini — risiko regresi terhadap fitur yang sudah ada) ATAU service terpisah berdiri sendiri (duplikasi kecil, nol risiko terhadap file existing). **Pengguna memilih opsi kedua secara eksplisit.**

**Konsekuensi teknis yang baru ditemukan setelah keputusan itu:** `CK_FinOpeningItemBatch_ItemKind` (DB check constraint) terpatok `IN ('RECEIVABLE','SUPPLIER_PAYABLE')` — menambah nilai baru butuh migration tersendiri, di luar wewenang task ini. Diselesaikan dengan **tidak** menambah nilai baru sama sekali: batch header tetap `ItemKind = RECEIVABLE` (nilai yang sudah sah), distingsi manfaat karyawan sepenuhnya hidup pada `FinReceivable.DebtorType`/`BenefitOwnerId` (skema yang sudah mendukungnya sejak `BE-FIN-079`). Nol migration.

---

## 1. Proses bisnis

**Alur (satu langkah, semua-atau-tidak-sama-sekali — berbeda dari siklus DRAFT→VALIDATED→APPROVED→LOCKED milik service generik, karena kontrak `FIN-API-1.9` §P.2 hanya mendefinisikan SATU endpoint dan `M.5.1` mengharapkan `201 Created` langsung):**

1. `POST /receivables/migration-batches/employee` menerima berkas CSV/XLSX + `CutoverDate` + `AccountingReferenceDocument` + `ControlTotalAmount`.
2. Setiap baris divalidasi: `NomorKartuLama` wajib & tidak kembar; `NIP` wajib terdaftar **dan aktif** di `MstEmployee` (`FIN-VAL-246`) → dipetakan ke `BenefitOwnerId`; `HubunganKeluarga` wajib salah satu dari `SELF/SPOUSE/CHILD/PARENT/OTHER`; `OriginalAmount`/`OutstandingAmount` wajib angka positif, `OutstandingAmount ≤ OriginalAmount`; `TanggalTransaksi` wajib valid dan tidak melewati `CutoverDate`.
3. **Satu baris gagal → nol baris diproses** (`M.5.2`), respons `422` memuat daftar lengkap error per baris.
4. Kontrol total (`ControlTotalAmount`) wajib sama persis dengan jumlah `OutstandingAmount` seluruh baris valid — beda sedikit pun → `422`, batch tidak diproses (`M.5.3`).
5. Bila seluruhnya valid: satu `FinOpeningItemBatch` (`ItemKind = RECEIVABLE`, langsung `Status = LOCKED`) + N `FinReceivable` (`DebtorType = EMPLOYEE_BENEFIT`, `OpeningItemBatchId` terisi, `SourceHandoffKey/Id`/`InvoiceId` kosong — sesuai `CK_FinReceivable_OpeningItem`) diterbitkan dalam satu transaksi, masing-masing dengan mutasi `PEMBUKAAN-MIGRASI`.

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/data/data-dictionary.md` §8.2 (struktur kolom CSV)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `M.5.1`..`M.5.3`
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` (dibaca sebagian — baris 600-830 — untuk memahami pola existing dan menemukan temuan §0; TIDAK diubah)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningItemBatch.cs` dan configuration-nya (menemukan `CK_FinOpeningItemBatch_ItemKind`)
- `Areas/Corporate/HumanResource/MasterData/Workforce/Models/MstEmployee.cs` (`IsActive`, `EmployeeNumber`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs` (pola `[FromForm]` + `IFormFile` pada DTO, diikuti persis)
- `.csproj` (paket spreadsheet tersedia: `ClosedXML` — dipakai untuk XLSX)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableMigrationDtos.cs` | **Baru.** `ImportEmployeeReceivableBatchRequest`, `EmployeeReceivableRowError`, `EmployeeReceivableBatchResponse` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableMigrationService.cs` | **Baru.** `ImportAsync` + parsing CSV/XLSX sendiri + `ReceivableMigrationValidationException` |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableMigrationController.cs` | **Baru.** 1 endpoint |
| `Program.cs` | `AddScoped<FinanceReceivableMigrationService>()` |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 1 endpoint baru, `FIN-API-1.9` §P.2 |
| Database | `NOT APPLICABLE` — nol migration, nol entity baru, nol perluasan check constraint (lihat §0) |
| Keamanan/Auth | Resource baru `FinanceReceivableMigration` (`Import`). `AgreementDocumentPath`-setara tidak ada di sini, tetapi `NIP`→`BenefitOwnerId` adalah data sensitif (sama kategori dengan `BenefitOwnerId` di tempat lain) — tidak dicetak ke log selain lewat `AuditAsync` yang hanya mencatat ringkasan angka |

---

## 3. Temuan yang dicatat

**Bug `CK_FinReceivable_Balance` ditemukan dan diperbaiki proaktif SEBELUM diklaim selesai** (pola yang sama persis pertama kali ditemukan di `BE-FIN-095`, diterapkan preventif di sini): baris migrasi dapat punya `OutstandingAmount < OriginalAmount` (sudah terbayar sebagian sebelum cutover) — draf awal tidak mengisi `AllocatedAmount` untuk selisihnya, yang akan melanggar `CK_FinReceivable_Balance`. Diperbaiki: `AllocatedAmount = OriginalAmount - OutstandingAmount` diisi eksplisit.

**Parsing CSV/XLSX ditulis sendiri, bukan dipakai ulang** dari `FinanceOpeningItemBatchService` — helper parsing-nya (`GetCell`, `ReadAndCheckUploadAsync`, dst.) adalah method **privat**, tidak dapat diakses lintas class, dan menjadikannya `internal`/`public` berarti mengubah file yang user minta tidak disentuh. Duplikasi kecil diterima sebagai konsekuensi keputusan §0.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review manual: seluruh check constraint `FinReceivable` yang relevan (`Balance`, `OpeningItem`, `BenefitOwner`, `DebtorType`, `Outstanding`, `Status`) | Dicocokkan baris per baris terhadap entity yang dibangun `ImportAsync` | `PASS` (review manual, bukan compiler) | §3, `FinanceReceivableMigrationService.cs` |
| Review manual: `FinanceOpeningItemBatchService.cs` TIDAK tersentuh | `git status --short` tidak menyebut file itu | `PASS` | Bagian 2.2 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile majemuk 8 task** (`BE-FIN-091`..`098`). Belum satu pun dikompilasi. Task ini punya permukaan API eksternal baru (`ClosedXML`) yang belum pernah dipakai langsung oleh kode yang saya tulis sendiri pada sesi ini — risiko salah nama method (`IsEmpty()`, `GetString()`, dst.) ada, meski diverifikasi semirip mungkin terhadap dokumentasi API yang saya ingat.

Uji manual: `NOT FEASIBLE`.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.5.1` NIP sah → `201`, dipetakan ke `BenefitOwnerId` | **Terpenuhi struktural** | `ImportAsync` |
| `M.5.2` NIP tidak terdaftar → `422`, `FIN-VAL-246` | **Terpenuhi struktural** | `ImportAsync` |
| `M.5.3` Kontrol total tidak cocok → `422`, batch tidak diproses | **Terpenuhi struktural** | `ImportAsync` |

**DoD roadmap** ("Build PASS, unit test validasi NIP, laporan task"): **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Source belum pernah dikompilasi, termasuk pemakaian API `ClosedXML` yang belum pernah diverifikasi compiler pada sesi ini |
| Masalah yang diketahui | `NONE` spesifik task ini di luar risiko compile umum |
| Risiko tersisa | Risiko compile 8 task majemuk, lebih tinggi dari biasanya karena dependency eksternal (`ClosedXML`) yang baru dipakai |
| Perubahan sampingan | `NONE` — `FinanceOpeningItemBatchService.cs` dan file terkaitnya sama sekali tidak disentuh, sesuai keputusan pengguna |
| Interupsi | `NONE` — satu `AskUserQuestion` dijawab pengguna sebelum implementasi dimulai (strategi terpisah vs. perluas existing), dicatat §0, bukan interupsi sesi |
| Status Git | `git status --short`: 3 file baru (DTO, Service, Controller), `Program.cs` dimodifikasi. **Nol** perubahan pada `FinanceOpeningItemBatchService.cs`/`FinOpeningItemBatchConfiguration.cs` |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` untuk memvalidasi `BE-FIN-091`..`098` sekaligus, perhatian khusus pada pemakaian `ClosedXML` di file ini |
