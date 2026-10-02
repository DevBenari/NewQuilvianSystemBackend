# Laporan Perubahan Backend — `BE-FIN-044`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-044` |
| Judul | Empat titik pemanggilan service yang sudah berjalan diselaraskan menggunakan nama konstanta `EventTypeCode` katalog resmi Accounting |
| Slice | `REV-6/8` — Penyelarasan Hak Akses (FIN-CQ-08), PPN Retur, dan Katalog Akuntansi (`01-backend-roadmap.md` bagian 3 dan 4) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 8, bagian 3 |
| Trace | Keputusan bisnis `FIN-DEC-039`, `068`, `071`; keputusan arsitektur `FIN-DES-058`; bukti `evidence/14` bagian 3.6, `evidence/15` bagian 5.10; arsitektur backend `02-backend-architecture.md` E.5 & E.9 |
| Contract version | `FIN-INTEGRATION-1.4` Bagian 5.10, `FIN-MVP-1.5`, `FIN-TEST-1.4` Bagian E.4 |
| Dependency | `BE-FIN-008` ✅, `BE-FIN-010` ✅, `BE-FIN-011` ✅, `BE-FIN-019` ✅, `BE-FIN-020` ✅ |
| Klasifikasi | `MEDIUM` — berkas diperiksa 6, berkas diubah 4 (`FinAccountingEventOutbox.cs`, `FinanceSupplierPayableService.cs`, `FinancePaymentService.cs`, `FinanceReceivableService.cs`), nol perubahan skema/migration |
| Task mode | `BACKEND` (`TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` — penghapusan 5 konstanta alias lama dari `FinAccountingEventOutbox.cs`, penggantian pemanggilan konstanta outbox pada 5 titik pemanggil di 3 service, laporan task, dan roadmap modul |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN** — 29 September 2026. Source code pada 4 berkas telah diselaraskan penuh ke katalog resmi, seluruh pemanggil alias lama digantikan, 5 konstanta alias lama dihapus dari outbox model, seluruh kriteria penerimaan terpetakan ke source, verifikasi `dotnet build` sengaja ditunda atas permintaan pengguna ("jangan lakukan build automatis") untuk dijalankan mandiri |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul manajemen keuangan rumah sakit |
| Submodule | `AccountingIntegration` (katalog tipe kejadian), `Payable` (service utang/pembayaran), `Receivable` (service piutang) | Seluruh submodule berstatus `ACTIVE` pada registry kanonikal |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang implementasi dan modifikasi kode |
| Keberlakuan | `TOUCHED LEGACY` | Penyelarasan konstanta string kode kejadian pada service-service yang sudah berjalan |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-NAM-002`, `QBE-AUD-001` | Mengubah string konstanta kejadian murni pada pemanggilan outbox tanpa mengubah kontrak payload |

---

## 2. Masalah yang Diselesaikan dan Landasan Bisnis

Sebelum task ini dijalankan, terdapat ketidakkonsistenan penamaan kode jenis kejadian antara kode service Finance lama dengan katalog resmi Accounting:
1. **Dua Standar Penamaan Bersamaan (`FIN-DES-058`):**
   Pada awal pengembangan sistem (V2 legacy), digunakan 5 kode alias pendek berawalan modul:
   - `AR_CREATED`
   - `AR_PAYMENT`
   - `AR_WRITEOFF`
   - `AP_CREATED`
   - `AP_PAYMENT`
   Namun, pada ratifikasi katalog bersama Accounting (`FIN-DEC-002`, `FIN-INTEGRATION-1.4` §5.10), disepakati bahwa nama kejadian menggunakan Bahasa Indonesia deskriptif baku:
   - `AR_PAYMENT` → `PENERIMAAN-PIUTANG`
   - `AR_WRITEOFF` → `PEMUTIHAN-PIUTANG`
   - `AP_CREATED` → `PENGAKUAN-HUTANG-SUPPLIER`
   - `AP_PAYMENT` → `PEMBAYARAN-HUTANG-SUPPLIER`
2. **Risiko Integritas Data dan Audit:**
   Jika konstanta alias pendek tetap dipertahankan di `FinAccountingEventTypeCodes`, pembuat kode berikutnya dapat menggunakan alias tersebut secara tidak sengaja. Hal ini menyebabkan satu fakta bisnis yang sama diterbitkan dengan dua kode jenis kejadian berbeda, yang membingungkan consumer Akuntansi dan memicu duplikasi atau penolakan transaksi di buku besar.
3. **Penyelarasan Baris Historis (`FIN-DES-058`):**
   Baris outbox yang pernah tertulis sebelumnya dengan nama lama tetap dipertahankan apa adanya di database (`PENDING`) dan **tidak ditimpa** oleh migration data runtime. Penanganannya merupakan wewenang operasional terpisah (`BE-FIN-026`). Task ini berfokus memastikan **seluruh kode pemanggil baru terbit dengan nama resmi dan alias lama dihapus tuntas**.

---

## 3. Proses Bisnis dan Rincian Perubahan

### 3.1. Penghapusan 5 Konstanta Alias Lama dari Outbox Model
Pada [FinAccountingEventOutbox.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs), dihapus 5 konstanta alias:
```csharp
// Dihapus per FIN-DES-058:
// public const string ArCreated = "AR_CREATED";
// public const string ArPayment = "AR_PAYMENT";
// public const string ArWriteOff = "AR_WRITEOFF";
// public const string ApCreated = "AP_CREATED";
// public const string ApPayment = "AP_PAYMENT";
```
Penghapusan ini memastikan compiler C# akan langsung memvalidasi dan menolak bila ada pemanggil lain yang mencoba memakai konstanta alias tersebut.

### 3.2. Penyelarasan Pemanggil pada `FinanceSupplierPayableService`
Pada [FinanceSupplierPayableService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs):
1. **Titik 1 (Pembuatan Utang Supplier - baris ~127):**
   ```csharp
   // Sebelumnya: EventTypeCode = FinAccountingEventTypeCodes.ApCreated
   EventTypeCode = FinAccountingEventTypeCodes.PengakuanHutangSupplier
   ```
2. **Titik 2 (Pencatatan Pembayaran Langsung - baris ~250):**
   ```csharp
   // Sebelumnya: EventTypeCode = FinAccountingEventTypeCodes.ApPayment
   EventTypeCode = FinAccountingEventTypeCodes.PembayaranHutangSupplier
   ```

### 3.3. Penyelarasan Pemanggil pada `FinancePaymentService`
Pada [FinancePaymentService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs):
1. **Titik 3 (Pencairan Pembayaran Utang Supplier - baris ~610):**
   ```csharp
   // Sebelumnya: EventTypeCode = FinAccountingEventTypeCodes.ApPayment
   EventTypeCode = FinAccountingEventTypeCodes.PembayaranHutangSupplier
   ```

### 3.4. Penyelarasan Pemanggil pada `FinanceReceivableService`
Pada [FinanceReceivableService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs):
1. **Titik 4 (Penerimaan Pembayaran Piutang - baris ~604):**
   ```csharp
   // Sebelumnya: EventTypeCode = FinAccountingEventTypeCodes.ArPayment
   EventTypeCode = FinAccountingEventTypeCodes.PenerimaanPiutang
   ```
2. **Titik 5 (Pemutihan / Write-off Piutang - baris ~698):**
   ```csharp
   // Sebelumnya: EventTypeCode = FinAccountingEventTypeCodes.ArWriteOff
   EventTypeCode = FinAccountingEventTypeCodes.PemutihanPiutang
   ```
   *(Sesuai koreksi `FIN-DES-058` dan roadmap revisi 7: menggunakan kode resmi `PEMUTIHAN-PIUTANG`, bukan `PENGHAPUSAN-PIUTANG`).*

---

## 4. Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Menghapus 5 konstanta alias (`ArCreated`, `ArPayment`, `ArWriteOff`, `ApCreated`, `ApPayment`) |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` | Mengubah 2 titik pemanggilan `StageEventAsync` dari `ApCreated` ke `PengakuanHutangSupplier` dan dari `ApPayment` ke `PembayaranHutangSupplier` |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` | Mengubah 1 titik pemanggilan `StageEventAsync` dari `ApPayment` ke `PembayaranHutangSupplier` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | Mengubah 2 titik pemanggilan `StageEventAsync` dari `ArPayment` ke `PenerimaanPiutang` dan dari `ArWriteOff` ke `PemutihanPiutang` |

---

## 5. Matriks Pemetaan Acceptance Criteria

| Acceptance Criteria pada Roadmap | Status | Lokasi Kode / Pembuktian |
| --- | :---: | --- |
| 1. Seluruh outbox baru terbit dengan nama katalog resmi; nol alias lama tersisa di kode pemanggil | **Terpenuhi** | Kelima titik pemanggil di `FinanceSupplierPayableService`, `FinancePaymentService`, dan `FinanceReceivableService` menggunakan konstanta resmi `PengakuanHutangSupplier`, `PembayaranHutangSupplier`, `PenerimaanPiutang`, dan `PemutihanPiutang`. Pencarian `grep` memastikan nol pemanggil alias tersisa di seluruh *.cs |
| 2. Lima konstanta alias yang dilarang dihapus dari `FinAccountingEventOutbox.cs` | **Terpenuhi** | `ArCreated`, `ArPayment`, `ArWriteOff`, `ApCreated`, `ApPayment` telah dihapus dari `FinAccountingEventTypeCodes` |
| 3. Baris outbox historis berstatus `PENDING` tidak diubah | **Terpenuhi** | Nol skrip migrasi data yang menimpa baris lama; integritas riwayat pesan outbox tetap terjaga sesuai `FIN-DES-058` |

---

## 6. Validasi dan Verifikasi Tata Kelola

1. **Review Diff:** Pemeriksaan diff Git membuktikan modifikasi terisolasi secara presisi pada 4 berkas tanpa efek samping ke logika kalkulasi saldo, alokasi, maupun transaksi lainnya.
2. **Pencarian Kode Statis:** Kueri regex terhadap `(ArCreated|ArPayment|ArWriteOff|ApCreated|ApPayment)` pada seluruh basis kode `.cs` menghasilkan nol temuan.
3. **Pengecekan Build:** Sesuai instruksi pengguna ("tpi jangan lakukan build automatis"), eksekusi `dotnet build` tidak dijalankan secara otomatis oleh agen dan didelegasikan kepada pengguna.

---

## 7. Risiko Tersisa dan Rekomendasi Task Berikutnya

1. **Konfirmasi Manual Build:** Pengguna perlu menjalankan `dotnet build` secara mandiri untuk mengonfirmasi kompilasi bersih (0 error).
2. **Task Selanjutnya:**
   - **`BE-FIN-045`**: Penanda shift kasir tertutup (`PENUTUPAN-SHIFT-KASIR`) dan pembaliknya.
   - **`BE-FIN-046`**: Pembalikan tender top-up deposit dikonsumsi menjadi `PEMBALIKAN-PENERIMAAN-UANG-MUKA`.
   - **`BE-FIN-026`**: Pembersihan baris warisan outbox (setelah otorisasi database diberikan).
