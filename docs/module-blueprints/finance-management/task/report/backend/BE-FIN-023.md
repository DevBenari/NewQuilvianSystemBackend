# Laporan Perubahan Backend — `BE-FIN-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-023` |
| Judul | Kotak keluar membawa rincian saldo subledger, dan aturan nilai beserta bentuk pesannya diluruskan sekali untuk seluruh kode |
| Slice | `REV-3` — AMENDMENT REVISI 3 (`EPIC FIN-14`), `01-backend-roadmap.md` bagian 3 dan 4 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 8, bagian 3 |
| Trace | Keputusan bisnis `FIN-DEC-035`, `043`, `064`; keputusan arsitektur `FIN-DES-031`, `032`, `033`, `054`, `058` (pelurusan 1 dan 2); kontrak `FIN-INTEGRATION-1.5` §5.2, §5.6, §5.10; aturan validasi `FIN-VAL-1.4` `FIN-VAL-079`..`084`, `138`, `139`; matriks pengujian `FIN-TEST-1.5` §D.2 dan §8a |
| Contract version | `FIN-INTEGRATION-1.5`, `FIN-VAL-1.4`, `FIN-TEST-1.5` — seluruhnya `approved` owner |
| Dependency | `FinanceAccountingOutboxService` (`BE-FIN-011` ✅); nol perubahan skema database |
| Klasifikasi | `MEDIUM` — berkas diperiksa 8, berkas diubah 3 (`FinAccountingEventOutbox.cs`, `FinanceAccountingOutboxService.cs`, `FinanceReceiptService.cs`), nol perubahan skema/migration |
| Task mode | `BACKEND` (`NEW CODE / TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi outbox staging, konstanta tipe kejadian, penyesuaian pemanggil receipt, laporan task, dan roadmap modul |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | ✅ **SELESAI** — 29 September 2026. Source selesai dan seluruh 7 acceptance criteria terpetakan ke source kode, nol perubahan skema database, `dotnet build` dikonfirmasi berhasil (0 error) oleh pengguna secara mandiri, dan rangkaian telah ditutup tuntas bersama `BE-FIN-024` ✅ |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul keuangan rumah sakit |
| Submodule | `AccountingIntegration` (utama), `Collection` (pemanggil tersentuh) | Keduanya berstatus `ACTIVE` pada registry kanonikal |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang implementasi dan modifikasi kode |
| Keberlakuan | `NEW CODE / TOUCHED LEGACY` | Penambahan fitur DTO & pengetatan validasi outbox, perapihan pemanggil receipt |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-NAM-002`, `QBE-AUD-001` | Seluruh invarian ditaati tanpa arsitektur generic repository |

---

## 2. Masalah yang Diperbaiki

Sebelum perubahan ini, penulisan kejadian pada outbox integrasi Akuntansi (`FinanceAccountingOutboxService`) memiliki tiga keterbatasan kritis:

1. **Ketiadaan Rincian Saldo Subledger (`FIN-DES-032`, `FIN-DEC-035`):**
   Kejadian `SALDO-SUBLEDGER` tidak memiliki wadah untuk mengangkut informasi periode akuntansi (`AccountingPeriodCode`) dan kode akun kontrol (`ControlAccountCode`), sehingga rekonsiliasi buku besar dengan subledger Finance saat tutup buku belum dapat dilakukan.
2. **Kelemahan Bentuk Serialisasi `Components` (`FIN-DES-058` Pelurusan 1, `FIN-VAL-139`):**
   `BuildPayloadJson` menyusun objek anonim yang selalu menyertakan kunci `"Components"`. Ketika kejadian tidak memiliki komponen (sebagian besar transaksi bernilai total utuh), pesan terkirim dengan `"Components": null`. Akuntansi menolak keberadaan kunci ini bila kejadian tidak memecah komponen.
3. **Aturan Nilai yang Terlalu Kaku dan Longgar di Tempat yang Salah (`FIN-DES-031`, `FIN-DES-054`, `FIN-DES-058` Pelurusan 2, `FIN-VAL-138`):**
   Pemeriksaan `Amount <= 0` sebelumnya menolak kode penanda status (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`) yang wajib bernilai `0` tanpa jurnal, dan menolak `SALDO-SUBLEDGER` yang dapat bernilai `0` atau negatif. Di sisi lain, pemeriksaan nilai longgar berisiko meloloskan kejadian transaksi biasa bernilai nol.
4. **Properti Kedaluwarsa `RequiresFinalization` (`FIN-DES-033`, `FIN-DEC-030`):**
   Properti ini menahan kejadian penerimaan dengan status `HELD_FOR_FINALIZATION`. Sesuai `FIN-DEC-030`, penahanan status ini telah dicabut dan digantikan dengan pemilihan kode kejadian yang tepat pada pemanggil (`PENERIMAAN-UANG-MUKA` vs `PENERIMAAN-KASIR`). Membiarkan properti ini tetap ada menipu pemanggil berikutnya.

---

## 3. Proses Bisnis dan Rincian Perubahan

### 3.1. Penambahan Rincian Saldo Subledger (`SubledgerBalanceRequest`)
Dibuat kelas DTO baru `SubledgerBalanceRequest` pada `FinanceAccountingOutboxService.cs`:
- `AccountingPeriodCode`: Wajib diisi, format string `YYYY-MM` (maksimal 7 karakter).
- `ControlAccountCode`: Wajib diisi, string kode akun kontrol Akuntansi (maksimal 50 karakter).

Properti `SubledgerBalanceRequest? SubledgerBalance` ditambahkan ke dalam `AccountingOutboxEventRequest`.

### 3.2. Pencabutan `RequiresFinalization`
- Properti `RequiresFinalization` dihapus dari `AccountingOutboxEventRequest`.
- Di `StageEventAsync`, penetapan status outbox kini selalu `DeliveryStatus = FinAccountingEventDeliveryStatuses.Pending`.
- Pada pemanggil berjalan di `FinanceReceiptService.cs` (metode `CreateFromTenderIntakeAsync` dan `CreateReversalFromTenderIntakeAsync`), baris `RequiresFinalization = requiresFinalization` dan variabel lokalnya dihapus agar sinkron dengan perubahan DTO.

### 3.3. Pelurusan Serialisasi `BuildPayloadJson` (`FIN-VAL-139`)
Serialisasi JSON pada `BuildPayloadJson` diubah menggunakan struktur `Dictionary<string, object?>`:
- 12 field wajib amplop akuntansi selalu disertakan (`EventNumber`, `EventTypeCode`, `SourceModule`, `SourceTransactionId`, `SourceVersion`, `EventOccurredAt`, `AccountingDate`, `Amount`, `CurrencyCode`, `LegalEntityId`, `CorrelationId`, `CausationId`).
- Properti `Components` **hanya ditambahkan ke dictionary jika `request.Components` memiliki elemen (`request.Components is { Count: > 0 }`)**. Bila null atau kosong, kunci `"Components"` sama sekali tidak ada di dalam string JSON `PayloadJson`. Kolom database `ComponentsJson` tetap bernilai `null`.
- Objek `SubledgerBalance` disertakan di dalam `PayloadJson` jika dan hanya jika `request.SubledgerBalance != null`.

### 3.4. Pengetatan dan Penyesuaian `ValidateRequest` (`FIN-VAL-079..084`, `138`)
Logika validasi disesuaikan dengan daftar aturan resmi:
1. **Daftar Tertutup Kode Penanda Bernilai Nol (`ZeroAmountAllowedEventTypes`):**
   Didefinisikan di `FinAccountingEventTypeCodes` (`FinAccountingEventOutbox.cs`):
   - `PENUTUPAN-SHIFT-KASIR`
   - `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`
   - `SALDO-SUBLEDGER`
2. **Aturan Nilai Nol dan Negatif:**
   - Jika `Amount == 0`: Diterima hanya jika `EventTypeCode` termasuk dalam daftar tertutup `ZeroAmountAllowedEventTypes`. Seluruh kode di luar daftar ditolak dengan `AccountingOutboxException("Nominal kejadian harus lebih dari nol.")` (`FIN-VAL-138`).
   - Jika `Amount < 0`: Ditolak tanpa pengecualian kecuali jika `EventTypeCode == "SALDO-SUBLEDGER"` (`FIN-DES-058` Pelurusan 2, `FIN-VAL-079`).
3. **Aturan Objek Saldo Subledger:**
   - Untuk kode bukan saldo (`EventTypeCode != "SALDO-SUBLEDGER"`): Bila `SubledgerBalance` diisi, ditolak dengan `AccountingOutboxException("Rincian saldo subledger hanya berlaku untuk pesan saldo.")` (`FIN-VAL-084`).
   - Untuk kode saldo (`EventTypeCode == "SALDO-SUBLEDGER"`):
     - Wajib membawa `SubledgerBalance`, `AccountingPeriodCode`, dan `ControlAccountCode` non-kosong (`FIN-VAL-081`).
     - `AccountingPeriodCode` wajib sesuai format regex `^\d{4}-(0[1-9]|1[0-2])$` dan panjang maksimal 7 karakter; nilai seperti `2026-11-30` ditolak dengan `AccountingOutboxException("Kode periode harus berbentuk tahun-bulan, contoh 2026-11.")` (`FIN-VAL-082`).
     - `ControlAccountCode` maksimal 50 karakter.
     - Bila `SourceVersion` diisi, wajib bilangan bulat positif (`FIN-VAL-083`).

---

## 4. Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Menambahkan konstanta `SaldoSubledger`, `PenutupanShiftKasir`, `PembalikanPenutupanShiftKasir`, dan daftar tertutup `ZeroAmountAllowedEventTypes` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` | Menambahkan DTO `SubledgerBalanceRequest`, memperbarui `AccountingOutboxEventRequest`, mencabut `RequiresFinalization`, menerapkan pelurusan `BuildPayloadJson`, dan memperbarui `ValidateRequest` |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | Menghapus penugasan `RequiresFinalization` pada kedua titik pemanggilan outbox penerimaan dan pembalikan penerimaan |

---

## 5. Matriks Pemetaan Acceptance Criteria

| Acceptance Criteria pada Roadmap | Status Implementasi | Lokasi Kode / Pembuktian |
| --- | :---: | --- |
| Saldo subledger `0` tersimpan | **Terpenuhi** | `FinanceAccountingOutboxService.cs#ValidateRequest`: `request.Amount == 0` lolos untuk `SALDO-SUBLEDGER` |
| Kode penanda bernilai `0` tersimpan (`PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`) | **Terpenuhi** | `FinAccountingEventTypeCodes.ZeroAmountAllowedEventTypes`: kedua kode terdaftar dan lolos saat `Amount == 0` |
| Kejadian **transaksi** bernilai nol ditolak | **Terpenuhi** | `FinanceAccountingOutboxService.cs#ValidateRequest`: melempar `AccountingOutboxException("Nominal kejadian harus lebih dari nol.")` bila `Amount == 0` dan bukan kode penanda |
| Kejadian bernilai **negatif** ditolak — tanpa pengecualian (kecuali saldo) | **Terpenuhi** | `FinanceAccountingOutboxService.cs#ValidateRequest`: melempar `AccountingOutboxException` untuk seluruh `Amount < 0` bila bukan `SALDO-SUBLEDGER` |
| `SubledgerBalance` pada kejadian non-saldo ditolak `400` | **Terpenuhi** | `FinanceAccountingOutboxService.cs#ValidateRequest`: melempar `AccountingOutboxException("Rincian saldo subledger hanya berlaku untuk pesan saldo.")` (`FIN-VAL-084`) |
| Periode `2026-11-30` ditolak `400` | **Terpenuhi** | `FinanceAccountingOutboxService.cs#ValidateRequest`: regex `AccountingPeriodRegex` menolak tanggal atau panjang > 7 dengan pesan `"Kode periode harus berbentuk tahun-bulan, contoh 2026-11."` (`FIN-VAL-082`) |
| `PayloadJson` pesan tanpa komponen **tidak memuat kunci `Components`** sama sekali, sedangkan kolom `ComponentsJson` tetap `null` | **Terpenuhi** | `FinanceAccountingOutboxService.cs#BuildPayloadJson`: dictionary hanya menambahkan kunci `"Components"` bila `Count > 0`; baris 50 memastikan `ComponentsJson` entity tetap `null` |

---

## 6. Risiko yang Tersisa dan Rangkaian Task Berikutnya

1. **Penutupan Rangkaian dengan `BE-FIN-024`:**
   Rangkaian wajib telah ditutup tuntas dengan selesainya `BE-FIN-024` ✅. Penerimaan pra-final kini secara deterministik terbit sebagai `PENERIMAAN-UANG-MUKA` dan pembalikannya mengikuti baris asli (`PEMBALIKAN-PENERIMAAN-UANG-MUKA`), sehingga tidak ada lagi risiko terbit keliru sebagai penerimaan kasir final.
2. **Kompilasi Mandiri Pengguna:**
   Perintah `dotnet build` dijalankan secara mandiri oleh pengguna sesuai instruksi eksplisit dan dikonfirmasi berhasil (0 error, build sukses).

