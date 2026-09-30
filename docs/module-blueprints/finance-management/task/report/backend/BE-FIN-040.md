# Laporan Perubahan Backend — `BE-FIN-040`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-040` |
| Judul | Potongan PPh 23 dan biaya admin bank dicatat bersama alokasinya, melunasi piutang sebagai pembayaran non-tunai, dan ikut terbalik bersama alokasinya |
| Slice | `REV-4` — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian "REV-4 — AR Invoice Agregat dan Potongan AR" |
| Trace | `FIN-DEC-049`, `055`, `058`, `062`; `FIN-DES-048`..`050`, **`052`** (AMENDMENT REVISI 6, pemecahan kode); `FR-FIN-093`..`095`, `099` |
| Contract version | `FIN-API-1.2` §C.2/C.3 (`locked` 26 September 2026); `FIN-STATE-1.3` §C.4; `FIN-VAL-1.4` `FIN-VAL-118`, `119`, `121`, `128`, `129`, `137` (`locked`/`approved` 28 September 2026, `FIN-VAL-1.5`); `FIN-INTEGRATION-1.6` §5.10.2 kode 32-35 |
| Dependency | `BE-FIN-038` 🟡 (skema `FinReceiptDeduction` — source & berkas migration selesai 29 September 2026, eksekusi migration tertunda; task ini murni menulis lewat skema itu, bukan mengubahnya) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 8 berkas diperiksa domain terdekat (skor 0), namun >8 total dibaca lintas kontrak (skor 1); 4 berkas diubah (skor 1); logika bisnis sedang — validasi berlapis + kode kejadian bercabang per `DeductionType` (skor 1); kontrak API — nol endpoint baru, satu endpoint diperluas aditif + satu endpoint baru sederhana (skor 1); database — nol perubahan skema (skor 0); keamanan/auth — nol resource baru, reuse `Receipt : Read` (skor 0); UI/workflow — nol dampak (skor 0). Total 4 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Collection/{Dtos,Services,Controllers}/**` (diperbarui, mengubah service yang sudah berjalan), `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` (tambah 4 konstanta), `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — lihat Status Git bagian 7 (working tree `Yasmina`, HEAD `7811c048`) |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **sengaja tidak dijalankan** — instruksi eksplisit pengguna pada task ini ("tanpa build automatis"). Nol migration disentuh — task ini murni menulis lewat skema `BE-FIN-038` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, skema potongan penerimaan (`FinReceiptDeduction`, `BE-FIN-038`) sudah tersedia
tetapi tidak ada satu pun kode yang menulisnya. Petugas AR tidak dapat mencatat piutang yang
dilunasi lebih kecil dari nilainya karena dipotong PPh 23 atau biaya administrasi bank — satu-
satunya jalan sebelumnya adalah mengalokasikan uang yang diterima apa adanya, meninggalkan sisa
piutang yang tidak akan pernah lunas tanpa penjelasan.

**Contoh konkret.** Piutang PT Asuransi Contoh Rp 10.000.000. Transfer masuk hanya Rp 9.745.000
karena penjamin memotong PPh 23 Rp 230.000 dan biaya administrasi bank Rp 25.000. Sebelum task
ini, petugas AR hanya bisa mengalokasikan Rp 9.745.000, menyisakan piutang Rp 255.000 yang secara
akuntansi keliru — padahal piutang itu sudah lunas penuh, hanya sebagian diterima dalam bentuk
non-tunai (potongan). Sesudah task ini, satu permintaan alokasi (`POST /receipts/{id}/allocations`)
mencatat ketiganya sekaligus: uang Rp 9.745.000 + potongan PPh 23 Rp 230.000 + potongan biaya bank
Rp 25.000 → piutang `SETTLED` penuh, dan dua kejadian Accounting terbit untuk kedua potongan itu.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR (mencatat alokasi bersama potongan; membalik alokasi bila keliru).

**Langkah normal:**

1. Petugas AR menerima transfer sebesar `9.745.000` untuk piutang `10.000.000`.
2. Petugas AR mengirim `POST /receipts/{id}/allocations` dengan satu baris ber-`targetType =
   RECEIVABLE`: `amount = 9.745.000`, dan `deductions[]` berisi dua pos: `PPH23 = 230.000` dan
   `BANK_ADMIN_FEE = 25.000`.
3. Backend memproses ketiganya **dalam satu transaksi**: uang alokasi mengurangi
   `OutstandingAmount` piutang lewat `FinanceReceivableService.ApplyAllocationAsync` (tidak
   diubah, dipanggil apa adanya); setiap potongan **memanggil method yang sama** sekali lagi
   dengan nilainya sendiri — piutang menjadi `SETTLED` (`10.000.000` habis: `9.745.000 + 230.000
   + 25.000`). `UnallocatedAmount` **penerimaan** hanya berkurang `9.745.000` — potongan bukan
   uang penerimaan (`FIN-VAL-121`).
4. Dua baris `FinReceiptDeduction` tersimpan dengan `DeductionNumber` masing-masing, dan dua baris
   kejadian Accounting terbit: `POTONGAN-PPH23-PIUTANG` (Rp 230.000) dan
   `POTONGAN-BIAYA-BANK-PIUTANG` (Rp 25.000).

**Jalur tidak normal:**

- Potongan diletakkan pada baris `INVOICE_DIRECT` (tanpa `receivableId`) → ditolak `400`
  (`FIN-VAL-128`, "Potongan hanya dapat dicatat pada pelunasan piutang.").
- Potongan bertipe `OTHER` → ditolak `400` (`FIN-VAL-137`) — **fail-closed**, akun debitnya belum
  ditetapkan Accounting (`FIN-OQ-033`). Seluruh permintaan gagal, nol baris tersimpan.
- Uang alokasi + seluruh potongan pada satu baris melebihi sisa piutang → ditolak `422`
  (`FIN-VAL-033` diperluas). Nol baris alokasi maupun potongan tersimpan — transaksi dibatalkan
  seluruhnya.
- Alokasi ber-potongan dibalik (`POST /receipts/{id}/allocations/{allocationId}/reverse`) →
  seluruh potongannya ikut dibalik dalam transaksi yang sama; piutang terbuka kembali penuh; dua
  kejadian `PEMBALIKAN-POTONGAN-...` terbit.

**Hasil akhir:** `FinanceReceivableService` tetap satu-satunya penulis `OutstandingAmount` — task
ini **tidak pernah** menulis kolom `FinReceivable` secara langsung, seluruhnya lewat
`ApplyAllocationAsync`/`ReverseAllocationAsync` yang sudah ada dan tidak diubah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu task
  `BE-FIN-040`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §D (`FIN-DES-048`..`050`,
  bentuk awal) **dan** §E.4 AMENDMENT REVISI 6 (`FIN-DES-051`, `052` — pemecahan kode final,
  **superseding** §D)
- `contracts/api-contract.md` §B.8, §C.2/C.3 (bentuk `deductions[]`, endpoint dicabut/tetap);
  `contracts/permission-audit-matrix.md`; `contracts/state-transition-matrix.md` §B.8/§C.4;
  `contracts/validation-matrix.md` §B.4, §C.1/C.3, §D.1/D.2 (`FIN-VAL-118`..`121`, `128`..`132`,
  `137`); `contracts/integration-contract.md` §5.10.2 (kode 32-35), §"Ratifikasi" (`FIN-OQ-028`
  masih terbuka — hanya menahan worker, bukan penulisan)
- `testing/acceptance-test-matrix.md` §C.3, C.4 — dipakai sebagai daftar skenario, **bukan** nama
  kode (lihat 3.3 untuk selisihnya)
- `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptDeduction.cs` (`BE-FIN-038`,
  termasuk komentar kelas yang eksplisit menyerahkan pemilihan `EventTypeCode` ke task ini)
- `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptAllocation.cs`,
  `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs`,
  `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs`
  (`BE-FIN-018`)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` —
  `ApplyAllocationAsync`/`ReverseAllocationAsync` (`BE-FIN-008`/`017`/`018`) — **dipanggil apa
  adanya, tidak diubah**
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs`,
  `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs`
  (`BE-FIN-011`)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` —
  `CreateReversalReceiptAsync`/`ProcessCollectionIntakeAsync` (lihat temuan 3.4)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Tambah 4 konstanta kode 32-35: `PotonganPph23Piutang`, `PembalikanPotonganPph23Piutang`, `PotonganBiayaBankPiutang`, `PembalikanPotonganBiayaBankPiutang` |
| `Areas/Corporate/FinanceManagement/Collection/Dtos/FinanceReceiptDtos.cs` | Tambah `ReceiptDeductionLineRequestDto`; `AllocationLineRequestDto` bertambah field `Deductions` (opsional); tambah `ReceiptDeductionResponse` |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | **Diperbarui — mengubah service yang sudah berjalan.** `AllocationLineRequest` (record) bertambah parameter opsional `Deductions`; tambah record `ReceiptDeductionLineRequest`; `AllocateAsync` memvalidasi dan mencatat potongan per baris + menerbitkan kejadian; `ReverseAllocationAsync` ikut membalik potongan pada alokasi yang dibalik; tambah `GetDeductionsAsync` (baca) dan `GenerateDeductionNumber` |
| `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` | **Diperbarui.** `Allocate` memetakan `Deductions` dari DTO ke service; tambah `GET /{id}/deductions`; tambah `MapDeduction` |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-040` → 🟡 di seluruh titik kemunculan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /receipts/{id}/allocations` **diperluas aditif** — `deductions[]` opsional per baris, persis bentuk `api-contract.md` §C.2 (`deductionType`, `amount`, `reason?`, `referenceNumber?`). `GET /receipts/{id}/deductions` **dibangun** sesuai §B.8. `POST /receipts/{id}/deductions` **tidak dibangun** — dicabut `FIN-API-1.2` (§C.3), sesuai instruksi task |
| Database | **`NOT APPLICABLE`.** Nol tabel/kolom baru — seluruhnya menulis lewat skema `FinReceiptDeduction`/`FinReceiptAllocation` yang sudah ada (`BE-FIN-038`/`018`) |
| Keamanan/Auth | Nol resource baru. Endpoint baru (`GET /{id}/deductions`) memakai `[AccessPermission("Receipt", "Read")]` — nama pendek yang sudah dipakai seluruh controller ini (lihat komentar kelas), bukan penyimpangan. Nol `IsInRole`/nama peran/departemen/`UserType` hardcode |
| **Perubahan pada service yang sudah berjalan** | `FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync` diubah — **seluruhnya bersyarat ada potongan** (`Deductions is { Count: > 0 }`). Permintaan alokasi tanpa `deductions` mengeksekusi baris kode yang **identik** dengan sebelum task ini (loop deduction di-`continue` lebih awal) — regresi nol, dibuktikan lewat review diff |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receipt

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `POST` | `/{id:guid}/allocations` | Mencatat alokasi penerimaan ke piutang, kini boleh membawa `deductions[]` per baris | `Receipt : Allocate` | **Diperluas aditif** (`BE-FIN-040`) |
| `GET` | `/{id:guid}/deductions` | Daftar potongan sisi penerimaan (PPh 23, biaya admin bank) pada satu penerimaan | `Receipt : Read` | **Baru** (`BE-FIN-040`) |
| `POST` | `/{id:guid}/allocations/{allocationId:guid}/reverse` | Membalik satu alokasi — kini ikut membalik seluruh potongan yang melekat padanya | `Receipt : Allocate` | **Diperluas aditif** (`BE-FIN-040`) |

**Contoh request `POST /receipts/{id}/allocations`:**

```json
{
  "lines": [
    {
      "receivableId": "c4d5e6f7-0000-4000-8000-000000000004",
      "amount": 9745000.00,
      "deductions": [
        { "deductionType": "PPH23", "amount": 230000.00, "referenceNumber": "BP-23-0091" },
        { "deductionType": "BANK_ADMIN_FEE", "amount": 25000.00 }
      ]
    }
  ]
}
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Sengaja tidak dijalankan — instruksi eksplisit pengguna |
| Piutang Rp 10.000.000; alokasi Rp 9.745.000 + PPh 23 Rp 230.000 + biaya bank Rp 25.000 | Piutang `SETTLED`; `UnallocatedAmount` penerimaan Rp 0; dua baris `FinReceiptDeduction` `DeductionNumber` berbeda; dua kejadian `POTONGAN-PPH23-PIUTANG`/`POTONGAN-BIAYA-BANK-PIUTANG` | `NOT RUN` (review kode) | `AllocateAsync` |
| Potongan pada baris `INVOICE_DIRECT` | Ditolak `400` (`FIN-VAL-128`) | `NOT RUN` (review kode) | `AllocateAsync` — validasi awal |
| Potongan `OTHER` | Ditolak `400` (`FIN-VAL-137`), nol baris tersimpan | `NOT RUN` (review kode) | `AllocateAsync` — validasi awal, sebelum transaksi dibuka |
| Uang Rp 9.800.000 + potongan Rp 230.000 ke piutang Rp 10.000.000 (total Rp 10.030.000) | Ditolak `422`; nol baris alokasi/potongan tersimpan | `NOT RUN` (review kode) | `ApplyAllocationAsync` dipanggil beruntun terhadap entity tracked yang sama — panggilan kedua melihat sisa piutang yang sudah berkurang panggilan pertama, melebihi lalu melempar sebelum `SaveChangesAsync` |
| Alokasi ber-potongan di atas dibalik manual | Dua baris potongan pembalik; piutang kembali `OUTSTANDING` Rp 10.000.000; dua kejadian `PEMBALIKAN-POTONGAN-...` | `NOT RUN` (review kode) | `ReverseAllocationAsync` |
| Permintaan alokasi tanpa `deductions` | Perilaku identik sebelum task ini | `NOT RUN` (review diff) | Baris `if (line.Deductions is not { Count: > 0 }) continue;` — jalur lama tidak tersentuh |
| `FinanceReceivableService` tetap satu penulis | Terpenuhi — nol baris kode menulis `FinReceivable.OutstandingAmount` langsung | `NOT RUN` (review kode) | Seluruh akses lewat `ApplyAllocationAsync`/`ReverseAllocationAsync` |

Uji manual/runtime: `NOT FEASIBLE` — memerlukan aplikasi berjalan dan migration `BE-FIN-038`
sudah dieksekusi, keduanya belum diotorisasi/dijalankan.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh eksekusi runtime/manual, dan
`Invoke-QbeConformanceCheck.ps1` — ketiganya menunggu instruksi eksplisit terpisah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FIN-DES-048` — piutang `SETTLED`, dua baris `FinReceiptDeduction` `DeductionNumber` berbeda | Terpenuhi (source) | `AllocateAsync` |
| `FIN-VAL-033` diperluas — uang + potongan ≤ sisa piutang, atomik | Terpenuhi (source) | Panggilan `ApplyAllocationAsync` beruntun di dalam satu transaksi |
| `FIN-VAL-128` — potongan hanya pada `RECEIVABLE` | Terpenuhi (source) | Validasi awal `AllocateAsync` |
| `FIN-VAL-137` — `OTHER` ditolak fail-closed | Terpenuhi (source) | Validasi awal `AllocateAsync` |
| `FIN-DES-049` — pembalikan alokasi ikut membalik seluruh potongannya | Terpenuhi (source) | `ReverseAllocationAsync` |
| Regresi nol — alokasi tanpa `deductions` tidak berubah perilaku | Terpenuhi (review diff) | Jalur `continue` awal |
| `FinanceReceivableService` tetap satu-satunya penulis `OutstandingAmount` | Terpenuhi (review kode) | Nol akses langsung ke kolom `FinReceivable` |
| `dotnet build` berhasil | **Belum terpenuhi** | Sengaja `NOT RUN` |
| `FIN-TEST-1.3`/`1.6` §C.3 (integrasi runtime) | **Belum dijalankan** — memerlukan aplikasi berjalan + migration `BE-FIN-038` tereksekusi | — |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kartu roadmap `BE-FIN-040` dan `testing/acceptance-test-matrix.md` §C.3/C.4 masih menyebut nama kode lama `POTONGAN-PIUTANG-NON-TUNAI`/`PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` — nama itu **superseded** oleh `02-backend-architecture.md` §E.4 (`FIN-DES-051`/`052`, AMENDMENT REVISI 6) dan `validation-matrix.md` §D.1, yang memecahnya menjadi `POTONGAN-PPH23-PIUTANG`/`POTONGAN-BIAYA-BANK-PIUTANG` per `DeductionType`. Task ini mengikuti dokumen yang **lebih baru dan lebih spesifik** (§E.4 secara eksplisit menyatakan "Konstanta `PotonganPiutangNonTunai`... **MUST NOT** ditulis"), bukan kartu roadmap/matriks uji yang belum disentuh pass revisi itu. Drift dokumentasi ini dicatat di sini, bukan diperbaiki sendiri di luar wewenang task — pemilik kontrak yang berwenang menyelaraskan `acceptance-test-matrix.md` |
| Masalah yang diketahui — gap pra-ada, bukan regresi task ini | `state-transition-matrix.md` baris 107 mendokumentasikan "Tender aslinya dibalik Billing → **Seluruh alokasinya ikut dibalik otomatis** (`FIN-DEC-021`)", tetapi `FinanceBillingIntakeService.ProcessCollectionIntakeAsync` → `FinanceReceiptService.CreateReversalReceiptAsync` (`BE-FIN-016`/`017`) **hanya** membuat baris `FinReceipt` pembalik — **tidak pernah** memanggil `ReverseAllocationAsync` untuk alokasi yang sudah ada pada penerimaan itu. Ini gap pra-ada dari `BE-FIN-016`/`018`, di luar cakupan literal task ini (potongan PPh 23/biaya bank), dan **tidak disentuh** di sini — mengubah `CreateReversalReceiptAsync` tanpa otorisasi eksplisit berisiko menyentuh perilaku pembalikan penerimaan yang sudah berjalan. Efek pada task ini: skenario "Tender Billing sumber penerimaan dibatalkan → potongan ikut terbalik otomatis" (`acceptance-test-matrix.md` §C.3 baris 6) **tidak akan terjadi** sampai gap ini ditutup — jalur MANUAL (`POST .../reverse`) sudah benar sepenuhnya |
| Risiko tersisa | Nol — `ApplyAllocationAsync`/`ReverseAllocationAsync` tidak diubah sama sekali; seluruh perubahan bersyarat ada potongan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas yang tersentuh task ini akan muncul `M` (tiga berkas Collection/AccountingIntegration + controller) pada `git status --short`. Sejumlah perubahan milik sesi lain juga terlihat pada `git status` repository — tidak disentuh, tidak dilaporkan di sini karena bukan bagian task ini |
| Langkah berikutnya | (1) Pemilik repository menjalankan `dotnet build`; (2) otorisasi dan eksekusi migration `AddArInvoiceBatchAndReceiptDeduction` (`BE-FIN-038`) — endpoint task ini tidak dapat diuji runtime tanpanya; (3) pemilik kontrak menyelaraskan `testing/acceptance-test-matrix.md` §C.3/C.4 ke nama kode final `FIN-DES-052`; (4) menutup gap pembalikan alokasi otomatis (`FIN-DEC-021`) sebagai task terpisah bila diprioritaskan; (5) `BE-FIN-042` (penyelarasan controller legacy) dapat dikerjakan paralel — tidak bergantung pada task ini |
