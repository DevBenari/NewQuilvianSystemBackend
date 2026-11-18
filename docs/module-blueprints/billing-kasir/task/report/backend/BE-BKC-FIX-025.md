# Laporan Perubahan Backend — `BE-BKC-FIX-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-025` (ad-hoc, di luar roadmap; nomor berikutnya setelah `BE-BKC-FIX-024`). Pasangan frontend: [`FE-BKC-FIX-015`](../frontend/FE-BKC-FIX-015.md) |
| Judul | Pembayaran tunai dengan kembalian: uang fisik yang diterima boleh melebihi tagihan, pembayaran neto tetap yang melunasi tagihan |
| Slice | Lanjutan Menu Pembayaran / split tender (`BKC-DEC-057` kwitansi per tender), tanpa task roadmap |
| Roadmap | `NOT APPLICABLE` — task ad-hoc atas permintaan langsung pemilik modul Finance/HR/Billing (master prompt 17 November 2026). Tidak ada kartu roadmap yang dapat ditandai; tidak ada perubahan pada `roadmap/**` |
| Trace | `NOT APPLICABLE`. Aturan bisnis yang dijalankan: `BKC-DEC-057` (satu Kwitansi per tender), Cash Rounding existing (`BE-02`/`BE-03`), `PC-DEC-001` (Petty Cash dan kas shift kasir dua sumber dana terpisah) |
| Contract version | Awalnya **aditif** (tidak ada endpoint baru, tidak ada field yang diubah atau dihapus). Dua perubahan dari keputusan final 17 November 2026 (bagian 8) tidak lagi aditif: uang diterima WAJIB untuk tender tunai, dan ada satu endpoint baru untuk pembatalan percobaan. Tidak ada versi kontrak terkunci yang berlaku untuk task ad-hoc ini |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing`; registry prefix `Bil` (`ACTIVE`). Keberlakuan: `TOUCHED LEGACY` — entity `BilTender` sudah ada, tidak ada entity/tabel/folder baru sehingga `QBE-MOD-002`/`QBE-MOD-003` tidak terpicu. QBE ID yang berlaku: `QBE-ENT-002`, `QBE-CFG-002`, `QBE-API-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001`. `QBE-PERM-001`: `NOT APPLICABLE` (tidak ada endpoint atau action baru) |
| Dependency | Tidak ada task lain. Syarat deploy: skema database harus disiapkan lebih dulu (lihat bagian 3.3) |
| Klasifikasi | `HEAVY` — skor 9 (repository 0, diperiksa 1, diubah 1, logika bisnis 2, kontrak API 2, database 2, auth 1, UI 0). Master prompt utuh bernilai 14 (`EPIC`) bila dihitung bersama frontend, sehingga dipecah menjadi dua task yang masing-masing dapat direview sendiri dan dikerjakan berurutan backend lebih dulu |
| Task mode | `CROSS-REPO` — kedua repository diberi wewenang tulis eksplisit oleh master prompt; backend dikerjakan lebih dulu |
| Target tulis | `NewQuilvianSystemBackend` (source aplikasi + laporan ini). Agent tidak membuat migration dan tidak mengeksekusi database. Migration dibuat pemilik di luar sesi; agent hanya mengganti namanya atas permintaan pemilik (bagian 3.3) |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `dbac9acc3d8640efb6e6dac892f7024b8ebabd7b` (branch `Yasmina`) + working tree; belum di-commit |
| Tanggal | 17 November 2026 |
| Status | 🟡 **Source selesai (termasuk addendum keputusan final 17 November 2026, bagian 8) dan ditinjau lewat pembacaan diff; belum dibuild dan belum diuji.** `dotnet build` **TIDAK dijalankan** (larangan eksplisit master prompt dan preferensi pemilik). Agent **tidak** membuat maupun menerapkan migration; pemilik membuat migration `20261117030708_AddBilTenderCashTenderedAndChange` sendiri (nama awal `ProbeCheck2`, diganti nama pada 17 November 2026). Penerapannya ke database **tidak diverifikasi agent** — fitur dan seluruh pembacaan `BilTender` bergantung pada kolom itu sudah terpasang (bagian 3.3) |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini sistem hanya mengenal satu angka untuk tender tunai: `BilTender.Amount`, dan batas `Amount ≤ sisa tagihan` ditegakkan backend. Pasien yang membayar tunai dengan uang lebih besar dari tagihan tidak punya tempat di data: kasir hanya bisa mengetik nominal sebesar tagihan, sedangkan uang fisik yang diterima dan kembalian yang diserahkan tidak tercatat di mana pun — tidak di tender, tidak di kwitansi, tidak di jejak audit.

> **Contoh:** Tagihan pasien Rp75.000. Pasien menyerahkan Rp100.000 dan kasir mengembalikan Rp25.000. Sebelumnya hanya "Rp75.000" yang tersimpan. Kwitansi yang dicetak ulang tidak bisa menunjukkan bahwa kasir pernah menerima Rp100.000 dan memberi kembalian Rp25.000, sehingga audit kas shift tidak punya bukti rincian fisiknya.

## 2. Proses bisnis

**Tujuan.** Mencatat uang fisik tunai dan kembalian pada tender yang sama, tanpa mengubah satu pun angka yang memengaruhi tagihan, alokasi, kas shift, atau Finance.

**Pelaku.** Kasir (pemegang shift aktif) menerima uang dan menyerahkan kembalian. Backend memvalidasi, menghitung kembalian, dan mencatat jejaknya. Tidak ada persetujuan atasan — kembalian bukan pengeluaran.

**Tiga nilai yang tidak boleh disamakan**

| Nilai | Kolom | Arti | Dipakai untuk |
| --- | --- | --- | --- |
| Pembayaran neto | `Amount` | Yang melunasi tagihan | Settlement, alokasi, status lunas, `SystemCash` shift, handoff Finance, refund, kwitansi "Pembayaran Bersih" |
| Uang diterima | `CashTenderedAmount` | Uang fisik dari pasien | Hanya rincian dan audit |
| Kembalian | `CashChangeAmount` | Yang dikembalikan kasir | Hanya rincian dan audit |

**Rumus** (hanya untuk metode dengan `MstPaymentMethod.IsCash = true`):

`CashChangeAmount = CashTenderedAmount − Amount`, dan `CashTenderedAmount ≥ Amount`.

**Langkah utama** (`POST …/settlements/{id}/tenders`):

1. Backend memeriksa bentuk request: `Amount` positif, dua desimal, ≤ batas sistem; `CashTenderedAmount` (bila ada) positif, dua desimal, ≤ batas sistem.
2. Backend memuat metode pembayaran dan memeriksa shift aktif seperti sebelumnya.
3. `Amount` tetap dibatasi sisa tagihan (termasuk toleransi pembulatan tunai yang sudah ada).
4. `ResolveCashBreakdown` menentukan rincian: non-tunai menolak field tunai; tunai menolak uang kurang dari `Amount`; kembalian positif **wajib** disertai `CashChangeGivenConfirmed = true`.
5. Tender disimpan (status `CREATED`) bersama `CashTenderedAmount` dan `CashChangeAmount` yang dihitung backend, di transaksi `Serializable` yang sama dengan alokasi nomor kwitansi.
6. Tender tunai langsung direkonsiliasi menjadi `SUCCEEDED`; `SystemCash` shift naik sebesar `Amount` (neto) lewat `ApplyCashReceiptAsync` yang sudah ada dan idempoten per tender.
7. Jejak audit dicatat: `BillingSettlement.TenderCreated` (membawa uang diterima, kembalian, dan pernyataan konfirmasi) dan, bila ada kembalian, `BillingSettlement.CashChangeHandedOver` (pelaku, nominal neto/uang diterima/kembalian, tender, shift, status tender).

**Contoh berangka**

| Skenario | `Amount` | Uang diterima | Kembalian | Sisa tagihan | `SystemCash` shift |
| --- | ---: | ---: | ---: | ---: | --- |
| TC-02 Tagihan Rp75.000, tunai Rp100.000 | 75.000 | 100.000 | 25.000 | 0 (lunas) | +75.000 |
| Kas bukaan Rp200.000 (contoh master prompt) | 75.000 | 100.000 | 25.000 | 0 | `ExpectedClosingCash` = 200.000 + 75.000 = **275.000** (bukan 300.000, bukan 250.000) |
| TC-04 Tagihan Rp100.000, tunai neto Rp50.000 dari Rp100.000 | 50.000 | 100.000 | 50.000 | 50.000 | +50.000 |
| TC-03 QRIS Rp20.000 + tunai neto Rp80.000 dari Rp100.000 | 20.000 dan 80.000 | — dan 100.000 | — dan 20.000 | 0 | +80.000 |

**Jalur tidak normal**

| Kejadian | Yang terjadi | Pesan / kode |
| --- | --- | --- |
| Uang diterima < `Amount` | Ditolak, tidak ada tender | `422` "Uang tunai diterima tidak boleh kurang dari nominal pembayaran tagihan." |
| Kembalian positif tanpa konfirmasi | Ditolak, tidak ada tender, nomor kwitansi tidak terpakai | `422` "Konfirmasi bahwa kembalian sudah diserahkan kepada pasien wajib diisi …" |
| Metode non-tunai mengirim uang diterima atau konfirmasi kembalian | Ditolak | `422` "Uang tunai diterima dan kembalian hanya berlaku untuk metode pembayaran tunai." |
| `Amount` melebihi sisa tagihan | Ditolak seperti sebelumnya | `422` "Total metode pembayaran melebihi saldo yang harus dibayar." |
| Shift tidak aktif | Ditolak seperti sebelumnya | `422` "Buka shift kasir sebelum menerima uang tunai." |
| Permintaan sama (Idempotency-Key sama) dikirim ulang | Mengembalikan tender yang sama, tidak ada tender atau penerimaan kas kedua | `200` replay |
| Idempotency-Key sama, uang diterima berbeda | Ditolak | `409` "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru." |
| Tender `CREATED` tertinggal, percobaan baru dengan uang fisik berbeda | **Tidak** digabung ke tender lama; dibuat sebagai percobaan terpisah | Kasir kini dapat melanjutkan atau membatalkan percobaan itu (bagian 8); risiko R-2 tertutup |
| Tender tunai tanpa `CashTenderedAmount` (termasuk klien lama) | **Ditolak**: uang diterima wajib diisi, juga untuk uang pas (keputusan final 17 November 2026, bagian 8) | `422` "Uang tunai diterima wajib diisi untuk pembayaran tunai. …" |
| Data lama (sebelum kolom ada) | Dibaca `null`/`null`; tidak ada kembalian fiktif | Kwitansi lama tampil seperti sebelumnya |

**Hasil akhir.** Pembayaran neto, alokasi, status lunas, `SystemCash`, handoff Finance, dan refund memakai `Amount` yang sama persis dengan sebelumnya. Uang diterima dan kembalian dapat dibaca ulang dari backend setelah refresh, login ulang, atau cetak ulang kwitansi.

**Yang sengaja tidak diubah.** Cash Rounding (selisih di bawah Rp1 tetap `BilAdjustment`, bukan kembalian); `CashierShiftService`; aturan refund, deposit, iuran membership, dan Finance Receipts; dan tidak ada pemakaian Petty Cash untuk menyediakan pecahan (transfer Petty Cash ke laci kasir di luar scope, sesuai `PC-DEC-001`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BilTender.cs`, `BilTenderConfiguration.cs`, `BillingSettlementDtos.cs`, `BillingPaymentHistoryDtos.cs`, `BillingEncounterPaymentHistoryDtos.cs`, `BillingSettlementService.cs` (seluruh jalur `AddTenderAsync`, `ReconcileTenderAsync`, `RecalculateSettlement`, validasi, hash, audit, mapping), `CashierShiftService.cs` (`ApplyCashReceiptAsync`, `RequireActiveShiftAsync`), `BillingSettlementsController.cs`, `BillingInvoicesController.cs`, `BillingInvoiceService.cs`, `BillingEncounterPaymentHistoryService.cs`, `BilConsumerHandoffService.cs` (`PublishForTenderAsync`), `BillingRefundService.cs`, `CashierShiftRevenueReportService.cs`, `LoggerService.cs`, `Program.cs` (perilaku migrasi saat startup), serta seluruh dokumen governance pada `rules/backend/`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs` | Tambah `decimal? CashTenderedAmount` dan `decimal? CashChangeAmount` beserta komentar kontrak (`Amount` tetap neto) |
| `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilTenderConfiguration.cs` | `HasPrecision(18, 2)` untuk kedua kolom; check constraint `CK_BilTender_CashBreakdown` (keduanya null atau keduanya terisi, kembalian ≥ 0, `Tendered − Amount = Change`) |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingSettlementDtos.cs` | `CreateTenderRequest`: `CashTenderedAmount` (`decimal?`, `[Range]` sama dengan `Amount`) dan `CashChangeGivenConfirmed` (`bool?`); **tidak** ada field kembalian pada request. `TenderResponse`: `CashTenderedAmount`, `CashChangeAmount` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` | `ResolveCashBreakdown` (validasi `IsCash`, `tendered ≥ Amount`, konfirmasi kembalian, uang diterima WAJIB untuk tunai sejak keputusan final 17 November 2026) dipakai pada tender baru dan pemulihan tender `CREATED`; `CashTenderMatches` pada pencocokan kandidat `CREATED`; segmen `CASH_TENDERED` pada hash payload (hanya bila dikirim); validasi bentuk `CashTenderedAmount`; audit `TenderCreated`/`TenderReconciled` diperkaya dan event baru `CashChangeHandedOver`; `MapTender` membawa kedua nilai; komentar pada `ApplyCashReceiptAsync` bahwa hanya `Amount` neto yang masuk `SystemCash` |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPaymentHistoryDtos.cs` dan `Services/BillingInvoiceService.cs` | `PaymentHistoryTenderResponse` membawa kedua nilai (proyeksi query ikut diperluas) |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingEncounterPaymentHistoryDtos.cs` dan `Services/BillingEncounterPaymentHistoryService.cs` | `EncounterPaymentHistoryEntryResponse` membawa kedua nilai untuk entri tender |

Penegasan scope: **tidak ada** perubahan pada `CashierShiftService`, controller, atribut akses, `Program.cs`, atau isi migration dan snapshot. Satu-satunya sentuhan agent pada `Migrations/` adalah mengganti nama migration milik pemilik (bagian 3.3). Repository frontend dikerjakan pada task pasangannya.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif. Request tender menerima dua field opsional; tiga DTO response membawa dua field nullable. Perubahan dari keputusan final 17 November 2026 (bagian 8): request tunai tanpa `CashTenderedAmount` kini ditolak, dan satu endpoint baru `POST …/tenders/{tenderId}/cancel` ditambahkan |
| Database | **Perubahan skema diperlukan; penerapannya belum diverifikasi.** Agent tidak membuat migration (larangan master prompt); pemilik membuatnya sendiri di luar sesi. Detail di bawah |
| Keamanan/Auth | Tidak ada perubahan. Endpoint tetap memakai `[AccessPermission("BillingPayment", "Create")]`. Kembalian tidak dipercaya dari klien: tidak ada field kembalian pada request, backend menghitung sendiri. Konfirmasi penyerahan kembalian dipaksa backend, bukan hanya frontend |

**Skema yang harus disiapkan DBA secara terpisah** (PostgreSQL, idempoten, aman diulang):

| Tabel | Kolom | Tipe | Null | Default | Aturan |
| --- | --- | --- | --- | --- | --- |
| `public."BilTender"` | `CashTenderedAmount` | `numeric(18,2)` | ya | tanpa default | `≥ Amount` bila terisi |
| `public."BilTender"` | `CashChangeAmount` | `numeric(18,2)` | ya | tanpa default | `≥ 0`; `= CashTenderedAmount − Amount` |

```sql
ALTER TABLE public."BilTender"
  ADD COLUMN IF NOT EXISTS "CashTenderedAmount" numeric(18,2) NULL,
  ADD COLUMN IF NOT EXISTS "CashChangeAmount"  numeric(18,2) NULL;

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint
    WHERE conname = 'CK_BilTender_CashBreakdown'
      AND conrelid = 'public."BilTender"'::regclass
  ) THEN
    ALTER TABLE public."BilTender"
      ADD CONSTRAINT "CK_BilTender_CashBreakdown" CHECK (
        ("CashTenderedAmount" IS NULL AND "CashChangeAmount" IS NULL)
        OR ("CashTenderedAmount" IS NOT NULL AND "CashChangeAmount" IS NOT NULL
            AND "CashChangeAmount" >= 0
            AND "CashTenderedAmount" - "Amount" = "CashChangeAmount"));
  END IF;
END $$;
```

Tanpa backfill: baris lama tetap `NULL`/`NULL` dan terbaca sebagai data lama. Cara mundur (hanya bila belum ada data memakainya): `ALTER TABLE public."BilTender" DROP CONSTRAINT IF EXISTS "CK_BilTender_CashBreakdown", DROP COLUMN IF EXISTS "CashChangeAmount", DROP COLUMN IF EXISTS "CashTenderedAmount";`

> **Urutan deploy yang aman: skema database → backend → frontend.** Ini bukan hanya syarat fitur baru. EF Core men-`SELECT` seluruh kolom yang dipetakan setiap kali entity `BilTender` dimuat (settlement, refund, laporan, handoff). Bila backend ini dijalankan terhadap database lama, pembacaan dan penulisan tender yang sudah ada ikut gagal dengan galat "kolom tidak ada" (`42703`). Aplikasi tidak memanggil `Migrate()` saat startup dan `PendingModelChangesWarning` diabaikan (`Program.cs`), jadi tidak ada crash saat start — kegagalan baru terlihat saat tender diakses. Query yang memakai proyeksi `.Select(...)` (mis. daftar riwayat) tidak terpengaruh.

**Migration yang dibuat pemilik dan penggantian namanya (17 November 2026).** Pemilik men-generate migration dari model ini di luar sesi agent dengan nama `ProbeCheck2` (`Migrations/20261117030708_ProbeCheck2.cs` dan `.Designer.cs`), lalu meminta nama yang sesuai konteks. Agent mengganti nama berkas `.cs` dan `.Designer.cs`, nama class pada kedua berkas, dan ID pada atribut `[Migration]` menjadi `20261117030708_AddBilTenderCashTenderedAndChange`. Timestamp dipertahankan supaya urutan migration tidak bergeser, dan nama mengikuti pola migration tetangga (`AddBilTenderKwitansiNumber`). Isi `Up`/`Down`, `BuildTargetModel`, dan `ApplicationDbContextModelSnapshot.cs` **tidak diubah agent**.

Pemeriksaan hasil rename: tidak ada sisa `ProbeCheck` pada kode; BOM UTF-8 dan akhir baris CRLF utuh; ukuran berkas bertambah tepat sebesar selisih panjang nama (`.cs` 1997 → 2019 byte, `.Designer.cs` 6.490.266 → 6.490.310 byte). `Up` cocok persis dengan DDL di atas (dua kolom `numeric(18,2)` nullable + `CK_BilTender_CashBreakdown`); diff snapshot hanya 10 baris tambahan.

Status kesiapan skema: pemilik melaporkan pada 17 November 2026 (setelah rename) bahwa migration ini **sudah diterapkan**. Agent tidak menjalankan `dotnet ef database update`, tidak membaca database, dan tidak memverifikasi klaim itu.

> **Perhatian bila migration ini sudah pernah diterapkan dengan nama lama.** ID migration adalah `<timestamp>_<nama>`, jadi rename mengubah ID. Bila `dotnet ef database update` sudah dijalankan saat nama masih `ProbeCheck2`, tabel riwayat berisi ID lama dan EF menganggap ID baru belum diterapkan, lalu mencoba menambah kolom yang sudah ada (galat "kolom sudah ada"). Agent tidak menjalankan SQL ini. Pemilik dapat memeriksa dan menyelaraskan riwayat sebelum update berikutnya; tabel riwayat memakai nama default.
>
> ```sql
> SELECT "MigrationId" FROM public."__EFMigrationsHistory"
>  WHERE "MigrationId" IN ('20261117030708_ProbeCheck2',
>                          '20261117030708_AddBilTenderCashTenderedAndChange');
>
> UPDATE public."__EFMigrationsHistory"
>    SET "MigrationId" = '20261117030708_AddBilTenderCashTenderedAndChange'
>  WHERE "MigrationId" = '20261117030708_ProbeCheck2';
> ```
>
> Bila migration belum pernah diterapkan, tidak ada yang perlu dilakukan: `dotnet ef database update` langsung memakai nama baru.

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Patient Funds

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/settlements/{id}/tenders` | Mencatat satu pembayaran. Untuk tunai, boleh membawa uang diterima dan konfirmasi kembalian | `BillingPayment : Create` | Header `Idempotency-Key`; body `CreateTenderRequest` (+ `cashTenderedAmount`, `cashChangeGivenConfirmed`) | `ApiResponse<TenderResponse>` (+ `cashTenderedAmount`, `cashChangeAmount`) |
| `GET` | `/invoices/{invoiceId}/settlements` | Daftar settlement invoice beserta tender-nya (dipakai Dokumen Kasir/kwitansi) | `BillingPayment : Read` | — | `ApiResponse<List<SettlementResponse>>`; tiap `TenderResponse` kini membawa kedua nilai |
| `GET` | `/settlements/{id}` | Satu settlement beserta tender-nya | `BillingPayment : Read` | — | `ApiResponse<SettlementResponse>` |

Kode status `POST /settlements/{id}/tenders`: `201` tender baru diproses; `200` permintaan sama dijawab ulang (replay); `409` data berubah atau isi permintaan berbeda dari yang tersimpan; `422` aturan bisnis dilanggar (termasuk uang kurang, konfirmasi kembalian hilang, atau field tunai pada metode non-tunai); `502`/`504` hasil provider belum pasti (non-tunai).

#### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/payment-history` | Riwayat Pembayaran lintas pasien | `BillingInvoice : Read` | Query `PaymentHistoryQuery` | `PaymentHistoryTenderResponse` kini membawa `cashTenderedAmount`, `cashChangeAmount` |
| `GET` | `/encounters/{encounterId}/payment-history` | Riwayat Pembayaran Kunjungan | `BillingInvoice : Read` | — | `EncounterPaymentHistoryEntryResponse` kini membawa kedua nilai untuk entri tender |

Seluruh data pada contoh di atas adalah data samaran.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` | Tidak dijalankan | `NOT RUN` | Tidak ada perubahan berkas project/dependency |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Larangan eksplisit master prompt ("Jangan menjalankan dotnet build") dan preferensi pemilik. **Belum ada bukti kompilasi** — pemilik perlu menjalankannya |
| QBE conformance (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`) | Tidak dijalankan | `NOT RUN` | Preflight tercatat di Metadata; tidak ada entity/folder/prefix baru |
| Kontrak API | Dibaca pada source | `PASS` (pembacaan source) | `BillingSettlementDtos.cs` (`CreateTenderRequest`, `CancelTenderRequest`, `TenderResponse`); route dan `[AccessPermission]` pada `BillingSettlementsController.cs` tidak berubah |
| Hak akses | Tidak ada endpoint/action baru | `PASS` | Nilai `AccessPermission` tetap `BillingPayment : Create` / `Read`; tidak ada `IsInRole` atau daftar peran baru |
| TC-01 tunai pas, kembalian 0 | `ResolveCashBreakdown`: uang diterima diisi sama dengan `Amount` (tombol Uang Pas), `change = 0`, tersimpan `0`; tanpa uang diterima ditolak | `PASS` (penelusuran source, bukan eksekusi) | `BillingSettlementService.cs:2122-2152` |
| TC-02 Rp75.000/Rp100.000 → kembalian Rp25.000, lunas | Tender `Amount = 75.000`, `CashTendered = 100.000`, `CashChange = 25.000`; `RecalculateSettlement` memakai `Amount` | `PASS` (penelusuran source) | `:464,480-481`; `RecalculateSettlement` `:2202` memakai `Amount` (hanya status tender yang berubah) |
| TC-03 QRIS + tunai neto dari uang fisik | Dua tender terpisah; hanya tender tunai membawa rincian; total tender 100.000 | `PASS` (penelusuran source) | `:480` (QRIS: field tunai `null`) |
| TC-04 tunai neto Rp50.000 dari Rp100.000 | `Amount = 50.000 ≤ collectible`; kembalian 50.000; sisa 50.000 | `PASS` (penelusuran source) | `:442-463` |
| TC-05 non-tunai menolak uang fisik/kembalian | Validasi 422 pada `!IsCash` | `PASS` (penelusuran source) | `:2126-2132` |
| TC-07 uang diterima < `Amount` ditolak | `422` | `PASS` (penelusuran source) | `:2144-2146` |
| TC-09 permintaan sama tidak menghasilkan tender/penerimaan kas ganda | Prior status ≠ `CREATED` → bandingkan `PayloadHash` → `MapTender(prior, true)` sebelum `ReconcileTenderAsync`; `ApplyCashReceiptAsync` idempoten per `(TENDER, tender.Id)` | `PASS` (penelusuran source) | `:305-311`; `CashierShiftService.cs:1011-1017` |
| TC-10 shift tidak aktif ditolak | `RequireCashierShiftAsync` dijalankan sebelum validasi rincian, perilaku tidak berubah | `PASS` (penelusuran source) | `:440`; `CashierShiftService.cs:963-974` |
| TC-11 closing shift tidak menghitung kembalian dua kali | Hanya `tender.Amount` masuk `ApplyCashReceiptAsync`; `ExpectedClosingCash = OpeningCash + SystemCash`; tidak ada cash-out baru | `PASS` (penelusuran source) | `:780-790`; `CashierShiftService.cs:1019-1025` |
| TC-12 Cash Rounding tetap berjalan, bukan kembalian | Blok `BilAdjustment` pembulatan bergantung pada `tender.Amount` dan `outstanding`, bukan uang diterima | `PASS` (penelusuran source) | `ReconcileTenderAsync` blok "BE-02 / BE-03" tidak diubah |
| TC-13 data lama tetap terpetakan | `MapTender` membaca kolom apa adanya (`null`); `CashTenderMatches` membaca `null` sebagai `Amount` | `PASS` (penelusuran source); **baru berlaku setelah skema ada** | `:2605`; `:2157` |
| TC-14 Rp0, deposit, non-tunai tanpa regresi | Cabang tersebut tidak menyentuh kode baru kecuali `ResolveCashBreakdown` yang mengembalikan `(null, null)` untuk non-tunai | `PASS` (penelusuran source) | `:2126-2132` |
| TC-15 neto sama, uang fisik berbeda tidak digabung | Hash memuat `CASH_TENDERED:`; kandidat `CREATED` dibandingkan lewat `CashTenderMatches`; Idempotency-Key sama dengan isi berbeda → `409` | `PASS` (penelusuran source) | `:280,327-333,2483` |
| Uji manual / runtime | Tidak dijalankan | `NOT FEASIBLE` | Skema belum ada, build dilarang, dan database tidak boleh dijalankan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Catatan: folder `Tests/QuilvianSystemBackend.PharmacyTests/` masih ada di working tree padahal kebijakan menyatakan project test sudah dihapus; dua berkasnya (`BillingInvoiceDepositPaymentStatusTests.cs`, `BillingEncounterPaymentHistoryTests.cs`) membuat `new BilTender { … }` dan tidak terpengaruh penambahan properti nullable. Tidak diubah dan tidak dijalankan.

**Tidak dijalankan:** `dotnet restore`, `dotnet build`, QBE conformance, semua uji runtime, dan semua perintah database — alasan di tabel.

---

## 6. Acceptance criteria dan Definition of Done

Task ad-hoc tanpa kartu roadmap; kriteria diambil dari master prompt bagian "Stop conditions".

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Backend mendukung pencatatan cash tendered, net payment, dan cash change | Terpenuhi di source | `BilTender.cs`; `:480-481` |
| 2. Tidak ada tabel baru yang tidak diperlukan | Terpenuhi | Dua kolom pada `BilTender` |
| 3. Kontrak request dan response konsisten | Terpenuhi di source | Empat DTO response + satu DTO request |
| 4. Settlement tidak memakai uang fisik bruto sebagai nominal pelunasan | Terpenuhi | `RecalculateSettlement`, `CalculateCollectibleAmount`, alokasi, handoff, refund tidak berubah dan memakai `Amount` |
| 5. Kas shift tidak mencatat kembalian ganda | Terpenuhi | `:780-790` hanya `tender.Amount`; `CashierShiftService` tidak diubah |
| 7. Konfirmasi kembalian dipaksa pada cash dengan kembalian positif | Terpenuhi | `:2148-2150` |
| 9. Idempotency dan concurrency existing dipertahankan | Terpenuhi | Transaksi `Serializable`, advisory lock, `RowVersion`, `ExpectedRowVersion`, `Idempotency-Key` tidak diubah |
| 10. Alur non-tunai, deposit, refund, split payment tidak berubah | Terpenuhi pada penelusuran source | Lihat TC-05/TC-14 |
| 11. Semua perubahan yang dapat dikerjakan tanpa build/migration sudah dilakukan | Terpenuhi | Tabel 3.2 |
| 12. Kebutuhan pembaruan database terdokumentasi eksplisit | Terpenuhi | Bagian 3.3 |
| Skema database tersedia dan `dotnet build` lulus | **Belum terpenuhi** | Dilarang pada task ini; diserahkan ke pemilik/DBA |
| Verifikasi runtime (TC-08 refresh/cetak ulang, TC-09 replay nyata, TC-11 closing shift nyata) | **Belum terpenuhi** | Butuh skema + build + shift nyata |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Deploy kode ini sebelum skema siap membuat pembacaan `BilTender` yang sudah ada ikut gagal (bagian 3.3) |
| Masalah yang diketahui | Tidak ada validasi ketersediaan pecahan di laci (sengaja: saldo total tidak membuktikan pecahan) dan tidak ada mekanisme transfer Petty Cash ke laci — di luar scope |
| Risiko R-1 | Sejak uang diterima wajib: backend baru + frontend lama = pembayaran tunai **ditolak 422** sampai frontend baru dideploy. Frontend baru + backend lama = field tunai diabaikan tanpa galat sehingga kembalian **tidak tercatat**, dan tombol pembatalan memanggil endpoint yang belum ada (404). Karena itu urutan deploy wajib: skema database lebih dulu, lalu backend dan frontend **bersamaan** |
| Risiko R-2 (**TERTUTUP** oleh pembatalan percobaan, bagian 8; teks berikut keadaan sebelum addendum) | Tender tunai yang tertinggal `CREATED` (mis. rekonsiliasi gagal) dan kasir mengulang dengan uang fisik **berbeda**: tender lama tidak diadopsi (sesuai TC-15), percobaan baru dapat ditolak "melebihi saldo" karena tender lama masih memesan sisa. Mengulang dengan uang fisik yang **sama** tetap memulihkan tender lama. Tidak ada endpoint pembatalan tender `CREATED` pada backend saat ini; bila skenario ini sering terjadi perlu keputusan terpisah (mis. izinkan koreksi uang fisik pada tender `CREATED`) |
| Risiko R-3 | Jejak audit: pernyataan penyerahan kembalian dicatat lewat audit existing (`LoggerService.AuditAsync`), bukan kolom tersendiri, sesuai instruksi "gunakan audit existing". Pembacaan ulang pernyataan itu dari database transaksional tidak tersedia; yang tersimpan permanen di tender hanya nominalnya |
| Temuan di luar scope (tidak diubah) | Tes unit frontend `billing-single-payment-action.test.mjs` membaca source backend dan sudah gagal sebelum task ini: string `if (invoice is null || invoice.Status != BillingInvoiceStatuses.Open) return;` tidak ada pada `BillingSettlementService.cs` di `HEAD` (dibuktikan dengan membandingkan `git show HEAD:` terhadap working tree). Dilaporkan, tidak diperbaiki |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` (17 November 2026, sesudah rename): 8 berkas source termodifikasi (daftar di 3.2) + 1 `ApplicationDbContextModelSnapshot.cs` termodifikasi (dari `dotnet ef` milik pemilik) + 2 berkas migration baru milik pemilik (`20261117030708_AddBilTenderCashTenderedAndChange.cs` dan `.Designer.cs`) + dua laporan tracked; tidak ada stage, commit, atau push |
| Langkah berikutnya | (1) Pemilik memastikan skema terpasang di dev lewat `dotnet ef database update` (periksa dulu tabel riwayat bila migration pernah diterapkan dengan nama `ProbeCheck2`, lihat 3.3), atau lewat DDL bagian 3.3; (2) pemilik menjalankan `dotnet build`; (3) uji manual TC-01…TC-15 di dev dengan shift aktif; (4) baru deploy frontend |

---

## 8. Addendum 17 November 2026 — Keputusan Final pemilik modul Billing

Dua keputusan pemilik modul Billing (17 November 2026) mengubah kontrak di atas. Bagian 1–7 sudah disesuaikan; bagian ini merangkum perubahannya.

| No | Keputusan | Dampak backend |
| ---: | --- | --- |
| 1 | `CashTenderedAmount` **wajib** pada setiap tender tunai, termasuk uang pas (frontend menyediakan tombol "Uang Pas"). Data historis tidak diberi kembalian fiktif | `ResolveCashBreakdown` menolak tender tunai tanpa uang diterima (`:2140`). Aturan "klien lama = tunai pas" **dicabut**. Baris lama (`null`/`null`) tidak tersentuh |
| 2 | Tender tunai `CREATED` yang tertahan dapat **dibatalkan** (status baru `CANCELLED`), bukan hanya dilanjutkan | `CancelTenderAsync` + endpoint baru + status `CANCELLED` (bagian 8.1–8.4) |

### 8.1 Proses bisnis pembatalan percobaan tunai

**Tujuan.** Melepas saldo yang tertahan oleh tender tunai yang tidak pernah berhasil dicatat, tanpa menghapus jejak dan tanpa menyentuh kas shift.

**Pelaku dan pemicu.** Kasir melihat baris "Pembayaran Menunggu Konfirmasi" bertanda tunai pada Menu Pembayaran (tender `CREATED`, biasanya karena rekonsiliasi gagal di tengah jalan) dan memilih salah satu dari dua tindakan.

| Pilihan kasir | Yang dikerjakan sistem |
| --- | --- |
| **Lanjutkan Pembayaran** | Frontend mengirim ulang pembayaran yang sama persis (metode, nominal neto, uang diterima, referensi) lewat `POST …/tenders`. Backend mengenali tender `CREATED` yang sama, memulihkannya, lalu merekonsiliasinya menjadi berhasil. Tidak ada tender atau nomor kwitansi kedua. Kembalian positif harus dikonfirmasi ulang |
| **Batalkan Percobaan** | `POST …/tenders/{tenderId}/cancel` dengan alasan dan pernyataan kustodi uang fisik `cashCustodyConfirmed` ("uang pasien belum diterima atau seluruhnya telah dikembalikan") |

**Langkah pembatalan di backend** (`CancelTenderAsync`, `BillingSettlementService.cs:1313`):

1. Validasi bentuk: Idempotency-Key, `ExpectedRowVersion` (RowVersion **tender**), alasan (wajib, maks. 500 karakter), `CashCustodyConfirmed = true`, `CorrelationId`/`CausationId`.
2. Transaksi `Serializable`; kunci settlement lalu tender (urutan sama dengan `ManualConfirmTenderAsync`), sehingga serial dengan pembayaran dan rekonsiliasi.
3. Tender sudah `CANCELLED` → dijawab replay tanpa audit kedua.
4. Status harus `CREATED`; `SUCCEEDED` ditolak (koreksi lewat reversal/refund resmi), `PENDING` ditolak (menunggu penyedia pembayaran), status lain ditolak.
5. Metode harus tunai; non-tunai ditolak.
6. `RowVersion` tender harus cocok, bila tidak `409`.
7. Pengaman terakhir: bila ada jejak penerimaan kas (`BilCashierShiftCommand` bersumber `TENDER` + id tender), `SettledAt`, atau `LastProviderEventId`, pembatalan ditolak `409` dan koreksi diarahkan ke reversal.
8. Status menjadi `CANCELLED`; `IsCancel`, `CancelDateTime`, `CancelBy` (kolom bawaan `IdentityModel`, **tidak ada kolom baru**) diisi; `RowVersion` tender berganti.
9. `RecalculateSettlement`: tender `CANCELLED` bukan `Created`/`Pending`, jadi **keluar dari saldo yang dipesan**; bila tidak ada percobaan lain, settlement kembali `Draft` (bukan `Failed`); `RowVersion` settlement berganti sehingga klien memuat ulang.
10. Setelah commit: audit `BillingSettlement.TenderCancelled` (petugas, waktu, alasan, nominal neto/diterima/kembalian, shift, status sebelum/sesudah, `ShiftCashChanged = false`).

**Yang tidak terjadi:** tidak ada `ApplyCashReceiptAsync`, jadi `SystemCash` shift tidak berubah; tidak ada alokasi, handoff Finance, atau reversal.

**Contoh.** Kasir menerima Rp100.000 untuk tagihan Rp75.000, tetapi rekonsiliasi gagal sehingga tender tertinggal `CREATED` dan memesan Rp75.000. Pasien membatalkan transaksi. Kasir menekan **Batalkan Percobaan**, menulis alasan "pasien tidak jadi membayar tunai", mencentang pernyataan uang sudah dikembalikan, lalu menyimpan. Hasil: tender `CANCELLED`, saldo Rp75.000 kembali bebas, `SystemCash` tetap, riwayat menampilkan "Dibatalkan", dan audit mencatat kasir, waktu, dan alasan.

**Jalur tidak normal**

| Kejadian | Hasil |
| --- | --- |
| Tender sudah `SUCCEEDED` | `422` "…tidak dapat dibatalkan lewat mekanisme ini. Koreksi … reversal atau refund resmi." |
| Tender `PENDING` | `422` "…belum dapat dibatalkan sebelum status dari penyedia pembayaran dipastikan." |
| Tender non-tunai | `422` "Pembatalan percobaan hanya berlaku untuk pembayaran tunai…" |
| `RowVersion` tender berubah | `409` "Data tender telah berubah. Muat ulang…" |
| Ada jejak rekonsiliasi atau penerimaan kas | `409` "…sudah pernah direkonsiliasi atau penerimaan kasnya sudah tercatat…" |
| Pernyataan penanganan uang tidak dicentang atau alasan kosong | `422` |
| Dibatalkan dua kali | Jawaban replay (`200`), tidak ada audit kedua |
| Kasir mengirim ulang pembayaran memakai Idempotency-Key tender yang sudah `CANCELLED` | `409` "Percobaan pembayaran ini sudah dibatalkan kasir. Buat pembayaran baru…" |
| Rekonsiliasi datang setelah pembatalan | Dijawab replay tanpa perubahan (`CANCELLED` terminal), tidak ada penerimaan kas |

### 8.2 Berkas yang berubah pada addendum

| Berkas | Perubahan |
| --- | --- |
| `Models/BilTender.cs` | Konstanta `BillingTenderStatuses.Cancelled`; komentar uang diterima wajib |
| `BilTenderConfiguration.cs` | `CK_BilTender_Status` memuat `'CANCELLED'` |
| `Dtos/BillingSettlementDtos.cs` | `CancelTenderRequest`; komentar `CashTenderedAmount` wajib untuk tunai |
| `Services/BillingSettlementService.cs` | `CancelTenderAsync`, `ValidateCancelTenderRequest`, `AuditTenderCancelledAsync`; `ResolveCashBreakdown` menolak tanpa uang diterima; `IsTerminal` memuat `Cancelled`; `RecalculateSettlement` tidak menganggap percobaan batal sebagai kegagalan; `AddTenderAsync` menolak (`409`) Idempotency-Key milik tender `CANCELLED` |
| `Controllers/BillingSettlementsController.cs` | `POST settlements/{settlementId}/tenders/{tenderId}/cancel` |
| `Services/BillingEncounterPaymentHistoryService.cs` | Nama status "Dibatalkan" pada riwayat |

### 8.3 Dokumentasi endpoint tambahan

#### Health Services / Billing Management / Billing / Patient Funds

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/settlements/{settlementId}/tenders/{tenderId}/cancel` | Membatalkan percobaan pembayaran tunai yang tertahan `CREATED` | `BillingPayment : Update` (aksi terdaftar `CancelTender`, `AccessTypes.Update`) | Header `Idempotency-Key`; body `CancelTenderRequest` (`expectedRowVersion`, `cancellationReason`, `cashCustodyConfirmed`, `correlationId`, `causationId`) | `ApiResponse<TenderResponse>`; `status = CANCELLED` |

Kode status: `200` dibatalkan atau replay; `404` settlement/tender tidak ada; `409` data berubah (`RowVersion`), status bukan `CREATED` (`SUCCEEDED`, `PENDING`, dst.), jejak rekonsiliasi, atau konflik data; `422` aturan bisnis (metode non-tunai, alasan kosong, pernyataan `cashCustodyConfirmed` tidak disetujui).

### 8.4 Skema database tambahan (migration dibuat pemilik; penerapannya tidak diverifikasi agent)

**Pembaruan 17 November 2026.** Pemilik men-generate migration untuk perubahan constraint ini di luar sesi agent dengan nama `ProbeCheck2` (`Migrations/20261117044418_ProbeCheck2.cs` dan `.Designer.cs`), lalu meminta nama yang sesuai konteks. Agent mengganti nama berkas, nama class, dan ID pada atribut `[Migration]` menjadi `20261117044418_AllowBilTenderCancelledStatus` (pola nama sama dengan `AllowEmployeeBenefitInvoiceBatchDebtorType`). Timestamp dipertahankan; isi `Up`/`Down` dan `BuildTargetModel` tidak diubah; BOM dan akhir baris CRLF dipertahankan (dicek lewat jumlah byte). Isi `Up`: `DropCheckConstraint` lalu `AddCheckConstraint` `CK_BilTender_Status` dengan daftar yang memuat `'CANCELLED'`; `Down` mengembalikan daftar lama. Snapshot sudah memuat daftar baru. Bila migration ini sudah pernah diterapkan dengan nama `ProbeCheck2`, riwayat EF berisi ID lama dan `dotnet ef database update` akan mencoba menerapkannya lagi (aman, karena DROP lalu ADD constraint yang sama, tetapi sebaiknya riwayat diselaraskan seperti catatan pada 3.3).

Skema yang dibutuhkan (DDL setara, tidak dijalankan agent):

Tanpa kolom baru. Yang berubah hanya constraint status; tanpa ini, mengubah tender menjadi `CANCELLED` ditolak database dan terlihat sebagai `409` "terjadi konflik data".

```sql
ALTER TABLE public."BilTender" DROP CONSTRAINT IF EXISTS "CK_BilTender_Status";
ALTER TABLE public."BilTender"
  ADD CONSTRAINT "CK_BilTender_Status" CHECK (
    "Status" IN ('CREATED','PENDING','SUCCEEDED','FAILED','EXPIRED','REVERSED','CANCELLED'));
```

Model sudah mencerminkan constraint ini, jadi `dotnet ef migrations add` berikutnya oleh pemilik akan membangkitkannya (snapshot saat ini masih memuat daftar lama; **tidak diubah agent**). Cara mundur: pasang kembali daftar tanpa `'CANCELLED'` setelah tidak ada baris `CANCELLED`.

### 8.5 Verifikasi tambahan

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uang diterima wajib untuk tunai | Request tunai tanpa `CashTenderedAmount` → `422`; non-tunai tidak terpengaruh | `PASS` (penelusuran source) | `BillingSettlementService.cs:2140` |
| Pembatalan tender tunai `CREATED` | Status `CANCELLED`, histori tetap, `RowVersion` berganti | `PASS` (penelusuran source) | `:1391-1400` |
| `SUCCEEDED`, `PENDING` ditolak `409`; non-tunai ditolak `422` | Status tidak diubah; frontend memuat ulang settlement (diselaraskan dengan master prompt 17 Nov 2026, sebelumnya `422`) | `PASS` (penelusuran source) | `:1353-1370` |
| Keluar dari saldo yang dipesan + settlement dihitung ulang | `CalculatePendingAmount` hanya `Created`/`Pending`; `RecalculateSettlement` memakai `attemptCount` tanpa percobaan batal; settlement `SETTLED` dijaga tidak terbuka kembali | `PASS` (penelusuran source) | `:2217`; `RecalculateSettlement` di `:1400`; penjaga `:1405` |
| Kas shift tidak berubah | `CancelTenderAsync` tidak memanggil `ApplyCashReceiptAsync`; pengaman terhadap jejak penerimaan kas | `PASS` (penelusuran source) | `:1378-1386` |
| Idempotency, `RowVersion`, transaksi, kunci | Replay pada `CANCELLED`; `RowVersion` tender dicek; `Serializable` + kunci settlement→tender; `409` untuk Idempotency-Key tender batal | `PASS` (penelusuran source) | `:1345`, `:305`, `:1331-1334`, `:1371` |
| Rekonsiliasi susulan tidak menghidupkan tender batal | `IsTerminal` memuat `Cancelled` | `PASS` (penelusuran source) | `:2195-2200` |
| Audit petugas, waktu, alasan | Event `BillingSettlement.TenderCancelled` | `PASS` (penelusuran source) | `:2336` |
| Hak akses | `[AccessAction("CancelTender", …, Update)]` + `[AccessPermission("BillingPayment", "Update")]`, sama dengan `manual-confirm` | `PASS` (pembacaan source) — lihat temuan di bawah | `BillingSettlementsController.cs:159-160` |
| Brace/kurung seimbang pada empat berkas C# yang diubah | Seimbang | `PASS` (pemeriksaan teks, bukan kompilasi) | Penghitungan karakter |
| `dotnet build`, uji runtime, uji database | Tidak dijalankan | `NOT RUN` | Larangan eksplisit master prompt |

### 8.6 Catatan, temuan, dan risiko addendum

| Hal | Isi |
| --- | --- |
| Temuan hak akses (tidak diubah) | Controller ini terdaftar sebagai `ControllerName = "BillingSettlements"`, sedangkan seluruh `[AccessPermission]` pada controller ini memakai resource `"BillingPayment"` (sesuai `permission-audit-matrix.md`). Pencarian saya tidak menemukan pendaftaran resource `BillingPayment` pada seeder, skrip, atau atribut mana pun. Menurut `role-access-rules.md` pasangan yang tidak terdaftar menolak semua pengguna selain yang melewati pemeriksaan (mis. SuperAdmin). Saya **tidak menjelaskan mengapa pembayaran tetap berjalan di lingkungan Anda** dan tidak mengubahnya; endpoint pembatalan sengaja memakai pasangan yang sama dengan `manual-confirm` agar berperilaku sama dengannya. Mohon pemilik memeriksanya |
| Granularitas izin | Pembatalan memakai izin `BillingPayment : Update` yang sama dengan konfirmasi manual; peran yang boleh mengonfirmasi manual otomatis boleh membatalkan percobaan. Izin terpisah hanya mungkin setelah temuan di atas diselesaikan |
| Nomor kwitansi | Tender yang dibatalkan sudah memakai satu nomor kwitansi saat dibuat. Nomor itu tidak dipakai ulang (terdapat celah penomoran) dan frontend tidak menawarkan cetak kwitansi untuk percobaan batal |
| Iuran membership | Alur pembayaran iuran membership juga memanggil `POST …/tenders`. Karena uang diterima kini wajib, frontend iuran mengirim tunai sebagai uang pas (uang diterima = nominal). Iuran tidak memiliki isian uang diterima/kembalian; bila diinginkan itu task terpisah |
| Status Git | `git status --short` (Billing dan Migrations): 9 berkas source termodifikasi oleh task ini (controller, tiga DTO, model, dua service riwayat/invoice, `BillingSettlementService`, `BilTenderConfiguration`), 1 `ApplicationDbContextModelSnapshot.cs` termodifikasi (dari `dotnet ef` milik pemilik), 2 berkas migration baru milik pemilik, dan dua laporan tracked. Berkas lain pada `git status` (Payroll) bukan dari task ini. Tidak ada stage, commit, atau push |
| Langkah berikutnya | (1) Terapkan DDL bagian 8.4 (atau buat migration dari model), (2) pemilik menjalankan `dotnet build`, (3) uji manual: tender tunai `CREATED` dibatalkan, dilanjutkan, dan ditolak saat `SUCCEEDED`, (4) deploy backend dan frontend bersamaan |

---

## 9. Addendum 17 November 2026 — Penyelarasan dengan master prompt "Finishing Pembayaran Tunai, Kembalian, dan Pembatalan Tender"

Master prompt lanjutan (BE-CASH-001..011) memuat keputusan yang sama dengan bagian 8, dengan nama field dan beberapa aturan yang lebih tegas. Bagian ini mencatat selisih yang ditemukan pada source saat itu dan yang dikerjakan. Larangan tetap: tanpa `dotnet build`, tanpa membuat atau menjalankan migration, tanpa commit.

### 9.1 Pemetaan master prompt ke source

| Butir master prompt | Keadaan sesudah penyelarasan | Bukti |
| --- | --- | --- |
| BE-CASH-001 model `BilTender` (`CashTenderedAmount`, `CashChangeAmount` nullable, `Amount` tetap neto, tanpa tabel baru, tanpa backfill) | Sudah sesuai sejak bagian 3 | `BilTender.cs`, `BilTenderConfiguration.cs` |
| BE-CASH-002 `CreateTenderRequest` (`CashTenderedAmount`, `CashChangeGivenConfirmed`) | **Diubah:** `IsCashChangeHandedOver` → `CashChangeGivenConfirmed`. Kembalian tetap hanya dihitung backend; non-tunai menolak kedua field; tunai tanpa `CashTenderedAmount` → `422` | `BillingSettlementDtos.cs`; `ResolveCashBreakdown` `:2122-2152` |
| BE-CASH-003 `AddTenderAsync` memisahkan `Amount`, uang diterima, kembalian; collectible hanya membatasi `Amount` | Sudah sesuai | `:464-481` |
| BE-CASH-004 idempotency | Hash memuat `CASH_TENDERED`; kandidat `CREATED` harus cocok pada metode, nominal, uang diterima, rekening, referensi. **Ditambah:** bila permintaan tunai baru tidak cocok dengan tender `CREATED` yang tertahan (mis. uang diterima diubah) dan saldo habis dipesan olehnya, backend kini menjawab `409` yang menyuruh kasir melanjutkan atau membatalkan percobaan lama lebih dulu, bukan `422` "melebihi saldo" yang menyesatkan. Tender lama **tidak** diambil alih | `:280`, `:448-463` |
| BE-CASH-005 status `CANCELLED` terminal | Sudah sesuai; tidak dapat hidup kembali (`IsTerminal`), tidak direkonsiliasi ulang | `:2195-2200`, `:305` |
| BE-CASH-006 endpoint `POST …/settlements/{settlementId}/tenders/{tenderId}/cancel` | Route sudah sama dengan master prompt (prefix controller `…/billing/patient-funds`). **Diubah:** `Reason` → `CancellationReason`, `IsCashHandlingConfirmed` → `CashCustodyConfirmed`. **Diubah:** tender `SUCCEEDED`/`PENDING`/status lain kini dijawab `409` (konflik keadaan), bukan `422`; metode non-tunai tetap `422`. **Ditambah:** penjaga agar settlement `SETTLED` tidak terbuka kembali lewat pembatalan | `CancelTenderAsync` `:1313`; `:1353-1370`; `:1405` |
| BE-CASH-007 audit pembatalan | Sudah memuat tender, settlement, status sebelum/sesudah, petugas, waktu, alasan, Idempotency-Key, konfirmasi kustodi (`CashCustodyConfirmed`), nominal neto, uang diterima, kembalian | `AuditTenderCancelledAsync` `:2336` |
| BE-CASH-008 lepas reservasi | Tender `CANCELLED` bukan `Created`/`Pending` sehingga keluar dari `PendingAmount`/`ReservedAmount`; tidak dihitung `SuccessfulAmount`; settlement tidak menjadi `Failed` hanya karena ada percobaan batal | `:2217`, `CalculatePendingAmount` |
| BE-CASH-009 kas shift | Hanya `tender.Amount` neto yang masuk `SystemCash`; pembatalan tidak memanggil `ApplyCashReceiptAsync` | `:780-790`, `:1378-1386` |
| BE-CASH-010 response dan riwayat | **Ditambah:** `TenderResponse.CancelledAt` (dari `CancelDateTime`, hanya untuk status `CANCELLED`). Alasan dan petugas pembatalan sengaja tidak diekspos; keduanya ada pada audit. Data lama tetap `null` | `MapTender` `:2615` |
| BE-CASH-011 dampak lintas proses | Endpoint batal tidak dapat dipakai untuk QRIS/Transfer/VA/provider (`IsCash` dicek, status `PENDING` ditolak) | `:1366` |

### 9.2 Keputusan desain yang menyimpang dari butir master prompt (dengan alasan)

| Butir | Keputusan | Alasan |
| --- | --- | --- |
| BE-CASH-006: "jika menggunakan identitas pembatalan persisten tambahan, tambahkan properti nullable pada `BilTender`" | **Tidak menambah kolom.** Pembatalan memakai `IsCancel`, `CancelDateTime`, `CancelBy` bawaan `IdentityModel`, ditambah audit | Identitas pembatalan tidak perlu disimpan di baris: memanggil ulang pada tender `CANCELLED` mengembalikan hasil idempoten tanpa mutasi dan tanpa audit kedua, apa pun Idempotency-Key-nya. Kolom baru menambah DDL di atas constraint status yang memang sudah wajib diubah |
| BE-CASH-010: "cancellation metadata yang aman ditampilkan" | Hanya `CancelledAt`. Alasan pembatalan **tidak** ada di response | Alasan bebas-teks 500 karakter hanya tersimpan pada audit; menampilkannya butuh kolom baru |
| BE-CASH-006 langkah 9 "tidak ada rekonsiliasi yang sedang atau telah berhasil" | Dipenuhi lewat kunci advisory settlement → tender (serial dengan `AddTenderAsync`/`ReconcileTenderAsync`) ditambah pemeriksaan jejak: `BilCashierShiftCommand` bersumber `TENDER`, `SettledAt`, `LastProviderEventId` | Satu hasil akhir per tender: bila rekonsiliasi lebih dulu → tender `SUCCEEDED` → batal ditolak `409`; bila batal lebih dulu → rekonsiliasi hanya menerima jawaban replay |

### 9.3 Skema database (tidak berubah dari 8.4)

Tidak ada kolom baru pada penyelarasan ini. Yang tetap wajib sebelum fitur dipakai: constraint `CK_BilTender_Status` harus memuat `'CANCELLED'` (DDL di 8.4). Dua kolom tunai dan `CK_BilTender_CashBreakdown` dilaporkan pemilik sudah diterapkan lewat migration `AddBilTenderCashTenderedAndChange`. Nama field JSON yang berubah (`cashChangeGivenConfirmed`, `cancellationReason`, `cashCustodyConfirmed`) hanya kontrak API, bukan skema.

### 9.4 Kontrak API sesudah penyelarasan

| Endpoint | Body | Respons dan kode |
| --- | --- | --- |
| `POST /settlements/{id}/tenders` | `paymentMethodId`, `amount` (neto), `cashTenderedAmount` (wajib untuk tunai), `cashChangeGivenConfirmed` (wajib `true` bila kembalian > 0), `expectedRowVersion`, `correlationId`, `causationId`, dll.; header `Idempotency-Key` | `TenderResponse` (`amount`, `cashTenderedAmount`, `cashChangeAmount`, `status`, `rowVersion`). `422`: uang diterima kosong/kurang/tidak sah, kembalian belum dikonfirmasi, field tunai pada non-tunai. `409`: Idempotency-Key sama dengan isi berbeda, tender sudah dibatalkan, tender tunai `CREATED` tertahan yang tidak cocok |
| `POST /settlements/{id}/tenders/{tenderId}/cancel` | `expectedRowVersion` (tender), `cancellationReason`, `cashCustodyConfirmed` (`true`), `correlationId`, `causationId`; header `Idempotency-Key` | `TenderResponse` dengan `status = CANCELLED`, `cancelledAt`. `404` tidak ada; `409` `RowVersion` basi, status bukan `CREATED`, atau ada jejak penerimaan kas; `422` non-tunai, alasan kosong, pernyataan tidak disetujui |

### 9.5 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| TC-01..TC-20 master prompt (penelusuran source) | Tertangani oleh bagian kode di 9.1; TC-03 (`CashTenderedAmount` kosong ditolak), TC-08 (tender `CANCELLED` tidak direkonsiliasi), TC-09 (`SUCCEEDED` tidak dapat dibatalkan, kini `409`), TC-11 (kunci sama, isi beda ditolak), TC-15 (batal tidak mengubah `SystemCash`) ditelusuri langsung | `PASS` (penelusuran source, **bukan** eksekusi) |
| Nama field kontrak sama antara DTO backend dan thunk frontend | Diperiksa oleh uji frontend yang membaca `BillingSettlementDtos.cs` dan `BillingSettlementsController.cs` | `PASS` (`AUTOMATED TEST` frontend, bukan uji backend) |
| TC-12 dan TC-13 (dua pembatalan serentak; batal bersaing dengan rekonsiliasi) | Hanya penalaran dari kunci advisory dan transaksi `Serializable`; belum ada eksekusi paralel | `NOT RUN` |
| `dotnet build`, uji runtime, uji database | Tidak dijalankan | `NOT RUN` |

### 9.6 Tindak lanjut untuk pemilik

1. Terapkan DDL status `CANCELLED` (8.4) lalu jalankan `dotnet build`.
2. Uji manual pembatalan: tender tunai `CREATED` dibatalkan, saldo kembali, dibuat tender baru; coba batalkan tender `SUCCEEDED` (harus `409`); ubah uang diterima pada tender tertahan (harus `409` menyuruh batalkan dulu).
3. Nama kasir pada kwitansi (`Petugas Kasir: [Nama Kasir]` di mockup master prompt) belum tersedia: `TenderResponse` tidak membawa nama petugas. Itu butuh pencarian nama pengguna dan keputusan privasi; bukan bagian penyelarasan ini.
