# Laporan Perubahan Backend — `BE-FIN-038`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-038` |
| Judul | Skema AR Invoice Agregat dan Potongan AR (bentuk revisi 5) tersedia |
| Slice | `REV-4` — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian "REV-4 — AR Invoice Agregat dan Potongan AR" |
| Trace | `FIN-DES-041`, `042`, `048`, `049`; `erd/data-dictionary.md` §C.13-C.14 dan §D.3-D.4(c) (`FinReceiptDeduction` bentuk revisi 5 — **bukan** §C.15); `FIN-STATE-1.2`/`1.3` §B.7 |
| Contract version | AMENDMENT REVISI 5 (`02-backend-architecture.md`, `locked` 26 September 2026) — bentuk `FinReceiptDeduction` di sini mengikuti revisi itu, bukan revisi 4 |
| Dependency | — (submodul `Receivable` dan `Collection` sudah terdaftar `ACTIVE` prefix `Fin` sejak `BE-FIN-001` ✅) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 9 berkas diperiksa domain terdekat + kontrak (skor 1); 9 berkas diubah/dibuat (skor 1); logika bisnis sederhana — murni entity+configuration+migration, nol service (skor 0); kontrak API — nol endpoint (skor 0); database — 3 tabel baru + migration (skor 2); keamanan/auth — nol dampak (skor 0); UI/workflow — nol dampak (skor 0). Total 4 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Models/**`, `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptDeduction.cs`, `Repositories/Configurations/Corporate/FinanceManagement/{Receivable,Collection}/**`, `Repositories/ApplicationDbContext.cs`, `Migrations/**`, `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — lihat Status Git bagian 7 (working tree `Yasmina`, HEAD `7811c048`) |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source dan berkas migration selesai; otorisasi pembuatan berkas migration diberikan eksplisit pengguna 29 September 2026 (menjawab prasyarat #10 yang sebelumnya "belum" untuk task ini). `dotnet build` **sengaja tidak dijalankan** — instruksi eksplisit pengguna pada task ini ("tanpa build automatis"). Eksekusi migration ke database **belum diminta** — wewenang terpisah dari pembuatan berkas |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, tidak ada tempat menyimpan dua hal yang dibutuhkan rumpun AR (piutang):
(1) penggabungan beberapa piutang satu penjamin menjadi satu dokumen tagihan resmi (Batch Tagihan
AR), dan (2) potongan saat penerimaan piutang — PPh 23 yang dipotong penjamin atau biaya
administrasi bank — yang harus tercatat terpisah dari uang yang benar-benar diterima.

**Contoh konkret.** Penjamin BPJS Ketenagakerjaan punya tiga piutang terpisah: Rp 5.000.000,
Rp 3.200.000, dan Rp 1.800.000 (total Rp 10.000.000). Tanpa Batch Tagihan AR, RS harus mengirim
tiga dokumen tagihan terpisah ke penjamin yang sama. Dan ketika penjamin melunasi Rp 9.745.000
(dipotong PPh 23 Rp 230.000 dan biaya bank Rp 25.000), tanpa `FinReceiptDeduction` tidak ada
tempat mencatat *kenapa* uang yang diterima lebih kecil dari piutangnya — padahal piutang itu
tetap harus lunas penuh secara akuntansi.

---

## 2. Proses bisnis

Task ini **murni menyiapkan skema** — nol proses bisnis baru berjalan sampai service pemiliknya
dibangun (`BE-FIN-039` untuk Batch Tagihan AR, `BE-FIN-040` untuk potongan penerimaan). Yang
disiapkan di sini adalah bentuk data yang akan dipakai proses bisnis itu:

- **`FinReceivableInvoiceBatch`** mengelompokkan beberapa `FinReceivable` milik satu penjamin
  (`DebtorReferenceId` sama) ke dalam satu batch berstatus `DRAFT` → `ISSUED` →
  `PARTIALLY_PAID`/`PAID` atau `CANCELLED`. Batch **tidak pernah** menjadi sumber kebenaran baru
  untuk pelunasan — `FinanceReceivableService` tetap satu-satunya penulis `OutstandingAmount`
  piutang anggotanya.
- **`FinReceiptDeduction`** melekat pada `FinReceiptAllocation` (bukan hanya `FinReceipt`) karena
  alokasilah yang menyebut piutang mana yang dikurangi. Baris **tidak pernah** dihapus atau
  diubah; potongan yang keliru dibalik dengan baris baru `IsReversal = true` menunjuk baris asli,
  pola yang identik dengan `FinReceiptAllocation` yang sudah berjalan.

**Jalur tidak normal yang relevan bagi task ini sendiri (integritas skema, bukan proses bisnis):**

- Satu `FinReceivable` mencoba masuk dua batch aktif sekaligus → ditolak unique index parsial
  `IX_FinReceivableInvoiceBatchItem_ActiveReceivable` (filter `IsDelete = false` — pembebasan
  piutang saat batch `CANCELLED` adalah keputusan service yang menyusul, bukan cakupan skema ini).
- Potongan jenis `OTHER` tanpa `Reason` → ditolak `CK_FinReceiptDeduction_OtherReason`.
- Baris pembalik potongan tanpa `ReversalOfDeductionId`, atau sebaliknya → ditolak
  `CK_FinReceiptDeduction_Reversal`.
- Satu baris potongan dibalik dua kali → ditolak unique index parsial
  `IX_FinReceiptDeduction_ReversalOnce`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu task
  `BE-FIN-038`, prasyarat #10 migration, tabel gelombang `REV-4`)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §C.13-C.14 (bentuk awal
  batch), §C.15 (bentuk `FinReceiptDeduction` yang **DIGANTIKAN**, dipertahankan sebagai jejak),
  §C.16 (DDL awal), §D.3 (bentuk final `FinReceiptDeduction`), §D.4(c) (DDL final)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §FIN-DES-041 (Batch
  Tagihan AR), §FIN-DES-048/049 (potongan melekat pada alokasi, pembalikan)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` (`DebtorType`/
  `DebtorReferenceId`, `FIN-CAP-031`) — dibaca, **tidak diubah**
- `Areas/Corporate/FinanceManagement/Collection/Models/FinReceipt.cs`,
  `FinReceiptAllocation.cs` (`TargetType`, `IsReversal`, pola self-reference
  `ReversalOfAllocationId`) — dibaca, **tidak diubah**
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableAdjustmentConfiguration.cs`,
  `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptAllocationConfiguration.cs`
  — pola configuration dan self-reference tanpa nav yang ditiru persis
- `Migrations/20260926100000_AddPurchasingApRumpun.cs` dan `.Designer.cs` — pola migration
  tangan multi-tabel ("ringkas", tanpa `BuildTargetModel` penuh) yang ditiru persis
- `Migrations/ApplicationDbContextModelSnapshot.cs` — struktur tiga-pass (properties,
  relationships, navigation) dipelajari dari entity terdekat (`FinReceivable`, `FinReceipt`,
  `FinReceiptAllocation`) sebelum menyisipkan entri baru
- `Repositories/ApplicationDbContext.cs` — lokasi `DbSet` submodul `Receivable`/`Collection`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs` | **Baru.** Aggregate root Batch Tagihan AR + `FinReceivableInvoiceBatchStatuses`/`DebtorTypes` |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatchItem.cs` | **Baru.** Baris keanggotaan batch |
| `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptDeduction.cs` | **Baru.** Bentuk revisi 5 (D.3/D.4(c)) — `DeductionNumber`, `ReceiptAllocationId`, `IsReversal`, `ReversalOfDeductionId` + `FinReceiptDeductionTypes` |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchConfiguration.cs` | **Baru.** Check constraint `DebtorType`/`Status`, index `BatchNumber` (unik), `DebtorType`, `DebtorReferenceId`, `PeriodStart`, `Status` |
| `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchItemConfiguration.cs` | **Baru.** FK `Batch`/`Receivable`, unique index parsial `ActiveReceivable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptDeductionConfiguration.cs` | **Baru.** Check constraint `Type`/`Amount`/`OtherReason`/`Reversal`, FK `Receipt`/`ReceiptAllocation`/self, unique index `DeductionNumber` dan `ReversalOnce` |
| `Repositories/ApplicationDbContext.cs` | Tambah 3 `DbSet`: `FinReceivableInvoiceBatches`, `FinReceivableInvoiceBatchItems`, `FinReceiptDeductions` |
| `Migrations/20260929120000_AddArInvoiceBatchAndReceiptDeduction.cs` | **Baru.** `Up()`: `CreateTable` × 3 + `CreateIndex` × 11. `Down()`: `DropTable` urutan FK terbalik |
| `Migrations/20260929120000_AddArInvoiceBatchAndReceiptDeduction.Designer.cs` | **Baru.** Bentuk ringkas, sumber kebenaran model tetap `ApplicationDbContextModelSnapshot.cs` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tambah 3 entity (properties + relationships + navigation sesuai pola tiga-pass berkas ini) |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-038` → 🟡; prasyarat #10 diperbarui — otorisasi pembuatan berkas migration **diberikan** 29 September 2026 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak menyentuh controller/DTO/endpoint apa pun |
| Database | **Tiga tabel baru**, nol tabel lama tersentuh. Migration `AddArInvoiceBatchAndReceiptDeduction` dibuat dengan otorisasi eksplisit 29 September 2026. **Eksekusi migration ke database belum diminta** — menunggu instruksi eksplisit terpisah. Migration bersifat aditif murni, dapat dijalankan tanpa downtime, `Down()` aman kapan pun karena tabelnya baru (nol data lama yang bisa hilang) |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi/endpoint |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint apa pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Sengaja tidak dijalankan — instruksi eksplisit pengguna pada task ini ("tanpa build automatis") |
| Migration diterapkan di lingkungan pengembangan | — | `NOT RUN` | Eksekusi migration wewenang terpisah dari pembuatan berkas — belum diminta |
| Review konfigurasi terhadap DDL `data-dictionary.md` §C.16 (batch) dan §D.4(c) (potongan) | Nama kolom, tipe, precision, default, check constraint, dan nama index cocok persis | `NOT RUN` (review manual) | Perbandingan `FinReceivableInvoiceBatchConfiguration.cs`/`FinReceiptDeductionConfiguration.cs` terhadap DDL |
| Review `ApplicationDbContextModelSnapshot.cs` konsisten dengan configuration | Ketiga entity muncul tepat sesuai pola tiga-pass (properties/relationships/navigation) yang dipakai entity tetangga | `NOT RUN` (review manual) | `grep` konfirmasi 2 kemunculan `FinReceiptDeduction`/`FinReceivableInvoiceBatchItem` dan 2 kemunculan `FinReceivableInvoiceBatch` (properties+relationships atau properties+navigation, sesuai ada/tidaknya FK keluar dan collection nav) |
| `FinReceiptDeduction` dibangun dari bentuk D.3, bukan C.15 | Kolom `ReceiptAllocationId`/`DeductionNumber`/`IsReversal`/`ReversalOfDeductionId` ada; `PurchasingInvoiceId` (bentuk lama yang salah domain) tidak pernah ada | `NOT RUN` (review kode) | `FinReceiptDeduction.cs` |

Uji manual: `NOT FEASIBLE` — memerlukan migration benar-benar diterapkan ke database, yang belum
diotorisasi.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, eksekusi migration, dan `Invoke-QbeConformanceCheck.ps1` —
ketiganya menunggu instruksi eksplisit terpisah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FinReceivableInvoiceBatch`, `...Item` (submodul `Receivable`) + configuration + `DbSet` | Terpenuhi (source) | Berkas model/configuration/DbSet di atas |
| `FinReceiptDeduction` (submodul `Collection`) ber-`DeductionNumber`, `ReceiptAllocationId`, `IsReversal`, `ReversalOfDeductionId` | Terpenuhi (source) — **bentuk D.3, bukan C.15** | `FinReceiptDeduction.cs` |
| Migration `AddArInvoiceBatchAndReceiptDeduction` | Terpenuhi (berkas dibuat, otorisasi eksplisit) | Migration `.cs`/`.Designer.cs` + `ModelSnapshot.cs` |
| Unique index parsial `ReceivableId` aktif | Terpenuhi (source) | `IX_FinReceivableInvoiceBatchItem_ActiveReceivable` |
| `CK_FinReceivableInvoiceBatch_DebtorType = 'PAYER'` | Terpenuhi (source) | `FinReceivableInvoiceBatchConfiguration.cs` |
| `CK_FinReceiptDeduction_OtherReason`, `_Reversal` | Terpenuhi (source) | `FinReceiptDeductionConfiguration.cs` |
| Unique `DeductionNumber`; unique parsial `ReversalOfDeductionId` | Terpenuhi (source) | `FinReceiptDeductionConfiguration.cs` |
| `dotnet build` berhasil | **Belum terpenuhi** | Sengaja `NOT RUN` atas instruksi pengguna |
| Migration diterapkan | **Belum terpenuhi** | Sengaja `NOT RUN` — wewenang terpisah |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Tidak ada — task ini murni aditif sesuai kontrak `data-dictionary.md` §C.13-C.14 dan §D.3-D.4(c), tanpa deviasi. Sengaja **tidak** menambah nav collection `Deductions` pada `FinReceipt.cs`/`FinReceiptAllocation.cs` — keduanya berkas yang sudah berjalan (`BE-FIN-016` ✅), dan FK dapat dikonfigurasi satu arah (`HasOne(...).WithMany()` tanpa balik nav) mengikuti pola self-reference yang sudah ada di `FinReceiptAllocationConfiguration.cs`, sehingga nol tabel/model modul lain tersentuh |
| Risiko tersisa | Migration belum dieksekusi ke database mana pun — ketiga tabel baru ada di source, belum ada di skema database sesungguhnya. `BE-FIN-039`/`040` yang membaca/menulis tabel ini lewat EF akan gagal terhadap database yang belum menjalankan migration ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini sendiri |
| Status Git | `git status --short` pada akhir pekerjaan mencakup task ini: `M Migrations/ApplicationDbContextModelSnapshot.cs`, `M Repositories/ApplicationDbContext.cs`, `?? Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptDeduction.cs`, `?? Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs`, `?? Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatchItem.cs`, `?? Migrations/20260929120000_AddArInvoiceBatchAndReceiptDeduction.cs`, `?? Migrations/20260929120000_AddArInvoiceBatchAndReceiptDeduction.Designer.cs`, `?? Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptDeductionConfiguration.cs`, `?? Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchConfiguration.cs`, `?? Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchItemConfiguration.cs`, plus `?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-038.md`. Sejumlah besar perubahan **milik sesi lain** (di luar task ini) juga terlihat pada `git status` repository — tidak disentuh, tidak dilaporkan di sini karena bukan bagian task ini |
| Langkah berikutnya | (1) Pemilik repository menjalankan `dotnet build`; (2) pemilik repository mengotorisasi dan menjalankan eksekusi migration `AddArInvoiceBatchAndReceiptDeduction` ke lingkungan pengembangan; (3) setelah keduanya, `BE-FIN-039` (Batch Tagihan AR) dan `BE-FIN-040` (potongan penerimaan) dapat mulai dikerjakan |
