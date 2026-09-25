# Laporan Perubahan Backend — `BE-FIN-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-017` |
| Judul | Pembagian bayar-vs-piutang — `FinanceReceiptService` |
| Slice | `MVP-2` — sebelumnya `BLOCKED — owner Billing` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-2`, `MVP-3` — tertahan) |
| Trace | `FR-FIN-031`, `FR-FIN-035`; kontrak sama dengan `BE-FIN-016` (`FIN-INTEGRATION-1.0` §2) |
| Contract version | `NOT APPLICABLE` — tidak ada kontrak API baru; kontrak data sama dengan `BE-FIN-016` |
| Dependency | `BE-FIN-016` — 🟡 sebagian 22 September 2026, source selesai untuk jalur `SUCCEEDED` (lihat [laporan](BE-FIN-016.md)) |
| Klasifikasi | `MEDIUM` — satu service baru + refactor lintas berkas atas otorisasi eksplisit (bagian 0); nol entity/migration baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` (baru); `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (disunting — refactor delegasi); `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (disunting — registrasi DI) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `a743388b57da91e6a0d7a42813604dc94563e38d` |
| Tanggal | 22 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** `FinanceReceiptService` dibangun (refactor dari `BE-FIN-016` + pembuktian FR-FIN-035). Alokasi manual maker-checker (FR-FIN-040..046) **sengaja tidak dibangun** di sini — itu cakupan `BE-FIN-018` yang terpisah (bagian 1.3), diselesaikan task itu sendiri. `CreateReversalReceiptAsync` di file ini (dipindahkan dari `BE-FIN-016`) diperbaiki 23 September 2026 — lihat Pembaruan bagian 7 dan [laporan BE-FIN-016](BE-FIN-016.md) bagian 1.5 untuk riwayat konflik constraint-nya |

---

## 0. Verifikasi dependency dan otorisasi refactor

Pertanyaan pengguna "Apakah bisa melanjutkan BE-FIN-017?" dijawab dengan verifikasi dua lapis
sebelum menulis kode:

1. **Dependency `BE-FIN-016` terpenuhi.** Roadmap mensyaratkan `BE-FIN-016` selesai lebih dulu.
   Task itu sudah source-complete untuk jalur `TenderStatus = SUCCEEDED` (satu-satunya jalur yang
   relevan bagi FR-FIN-031/035 — jalur `REVERSED` tetap `BLOCKED` terlepas dari task ini, lihat
   `BE-FIN-016` bagian 1.5).
2. **Ditemukan penyimpangan arsitektur sebelum menulis kode baru.** `02-backend-architecture.md`
   §4.22 (tabel service, sudah `approved` sejak revisi 1, tidak ada amandemen) menetapkan
   `FinanceReceiptService` sebagai pemilik logika **pembuatan** penerimaan dari tender, dengan
   `FinanceBillingIntakeService` sebagai **pemanggil** — persis pola `FinanceAccountingOutboxService`.
   `BE-FIN-016` menulis logika itu langsung di dalam `FinanceBillingIntakeService` karena
   perbandingan yang dipakai saat itu (§4.7/4.8, skema entity) tidak disilangkan dengan §4.22
   (tabel tanggung jawab service). Ini penyimpangan nyata dari dokumen yang sudah `approved`,
   bukan ambiguitas yang bisa dibiarkan.

**Pertanyaan diajukan eksplisit**: apakah memindahkan logika `BE-FIN-016` ke `FinanceReceiptService`
sekarang (selaras dokumen, sekaligus menjadi rumah yang tepat untuk logika `BE-FIN-017`) atau
membiarkan penyimpangan itu. **Jawaban yang diterima: "Refactor now (Recommended)"** — logika
pembuatan penerimaan dipindahkan tanpa perubahan perilaku, penomoran, maupun keputusan desain
(termasuk pemblokiran jalur `REVERSED` di bagian 1.5 laporan `BE-FIN-016`, yang tetap berlaku
apa adanya di lokasi barunya).

---

## 1. Keputusan desain dan batas cakupan (didokumentasikan, bukan didiamkan)

### 1.1 Refactor murni pemindahan — nol perubahan perilaku

`CreateSucceededReceiptAsync`, `CreateReversalReceiptAsync` (tetap melempar
`InvalidOperationException` yang sama), dan `GenerateReceiptNumber` dipindahkan **verbatim**
(termasuk seluruh komentar keputusan desainnya) dari `FinanceBillingIntakeService` ke
`FinanceReceiptService`, digabung di balik satu method publik baru,
`CreateFromTenderIntakeAsync(handoff, actorUserId, ct)`, yang melakukan percabangan
`TenderStatus` yang sebelumnya ada di `ProcessCollectionIntakeAsync`.
`FinanceBillingIntakeService.ProcessCollectionIntakeAsync` kini hanya memanggil method itu satu
baris, tetap di dalam transaksi `Serializable` miliknya sendiri — kontrak "MUST NOT membuka
transaksi sendiri" dipertahankan persis dan didokumentasikan ulang di kelas baru.

### 1.2 Perluasan cakupan: `GetInvoiceBreakdownAsync` untuk FR-FIN-035

FR-FIN-035 berbunyi: *"Sistem **dapat menampilkan** ketiga angka itu berdampingan untuk satu
tagihan"* — sebuah kapabilitas query/pembuktian, bukan kontrol tulis. Method baca-saja
`GetInvoiceBreakdownAsync(invoiceId, ct)` ditambahkan untuk memenuhi ini secara literal:
mengembalikan jumlah bersih penerimaan (dikurangi baris pembalik bila ada) dan total piutang
(asli maupun sisa) untuk satu `InvoiceId` yang sama. **Nilai tagihan Billing sendiri sengaja
tidak diikutkan** — itu perhitungan milik Billing (aturan bisnis #1, `FIN-OOS-001`..`004`,
Finance tidak pernah menghitung ulang atau membaca balik ke modul sumber untuk angka yang sudah
dimilikinya); dua angka sisi Finance sudah cukup membuktikan Finance sendiri tidak dobel-mencatat.
Belum ada controller yang memanggilnya — pola yang sama dengan `FinanceReceivableService`
(`BE-FIN-008`) yang selesai penuh sebelum `FinanceReceivablesController` (`BE-FIN-009`) dibangun.

### 1.3 Alokasi manual (FR-FIN-040..046, maker-checker) **TIDAK** dibangun — itu `BE-FIN-018`

Ini batas cakupan yang paling mudah disalahpahami. Roadmap `BE-FIN-017` bertrace `FR-FIN-031`
dan `FR-FIN-035` saja — **bukan** `FR-FIN-040`..`046` (alokasi manual, maker-checker, pembalikan
piutang), yang justru menjadi trace eksplisit `BE-FIN-018` ("Alokasi manual, maker-checker,
pembalikan", dependency: `BE-FIN-017`, `Owner Billing (turunan MVP-2)`). Dua alasan tambahan
memperkuat pemisahan ini, bukan sekadar membaca literal roadmap:

1. **FIN-DES-013** (02-backend-architecture.md §2.4): "Alokasi penerimaan ke piutang **tidak**
   memiliki pencocokan otomatis apa pun di service. Staf mengirim daftar alokasi eksplisit." —
   method otomatis apa pun yang langsung memutuskan pembagian akan melanggar ini.
2. **`FinanceReceivableService` adalah satu-satunya penulis `OutstandingAmount`** piutang
   (`BE-FIN-006`/`008`, ditegaskan berulang sebagai "Yang mudah salah" paling sering dicatat di
   roadmap modul ini). Method alokasi yang memotong `FinReceivable.OutstandingAmount` langsung
   dari `FinanceReceiptService` akan menciptakan **penulis kedua** — persis kesalahan yang
   berulang kali diperingatkan. Menghormati invariant ini berarti alokasi sungguhan (yang
   memindahkan nilai antar kedua aggregate) **MUST** melibatkan `FinanceReceivableService`,
   sebuah keputusan integrasi lintas service yang belum ditentukan bentuknya — pas menjadi
   scope `BE-FIN-018`, bukan diputuskan sepihak di sini.

`FinanceReceiptService` pada task ini karena itu **nol** menulis `FinReceivableAllocation`
apa pun, dan **nol** menyentuh kolom `FinReceivable` apa pun — konsisten dengan `FinReceipt`
yang selalu `AllocatedAmount = 0`, `UnallocatedAmount = Amount` sejak `BE-FIN-016`.

### 1.4 FR-FIN-031 sudah terpenuhi secara struktural, tidak butuh kode baru

"Pasien yang membayar lunas tidak melahirkan piutang" sudah benar sejak desain `BE-FIN-016`:
`FinReceivable` **hanya** pernah dibuat dari `HandoffType = AR` (`ProcessArIntakeAsync`), tidak
pernah dari `HandoffType = COLLECTION`. Task ini tidak menambah jalur apa pun yang bisa
melanggarnya — dicatat di sini sebagai bukti, bukan diklaim tanpa rujukan.

---

## 2. Ringkasan pekerjaan

### 2.1 `FinanceReceiptService` (baru)

| Method | Asal | Kegunaan |
| --- | --- | --- |
| `CreateFromTenderIntakeAsync` | Baru (pembungkus) | Percabangan `TenderStatus`, dipanggil `FinanceBillingIntakeService` |
| `CreateSucceededReceiptAsync` | Dipindah dari `BE-FIN-016`, verbatim | FR-FIN-030/032/033/034 |
| `CreateReversalReceiptAsync` | Dipindah dari `BE-FIN-016`, verbatim | Tetap `BLOCKED`, lihat `BE-FIN-016` bagian 1.5 |
| `GetInvoiceBreakdownAsync` | Baru | FR-FIN-035 — bagian 1.2 |
| `GenerateReceiptNumber` | Dipindah dari `BE-FIN-016`, verbatim | Penomoran non-seri |

### 2.2 `FinanceBillingIntakeService` — refactor delegasi

Konstruktor menerima `FinanceReceiptService` tambahan. `ProcessCollectionIntakeAsync` memanggil
`_receiptService.CreateFromTenderIntakeAsync(...)` alih-alih switch inline. Docstring kelas
diperbarui menyebut pemindahan ini secara eksplisit.

### 2.3 Registrasi DI

`FinanceReceiptService` didaftarkan `AddScoped`, ditempatkan sebelum `FinanceBillingIntakeService`
yang kini menjadi pemanggilnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` §4.22 (tabel service — sumber temuan penyimpangan), §4.7/4.8 (skema entity, sudah diperiksa `BE-FIN-016`)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `FR-FIN-031`, `FR-FIN-035`, `FR-FIN-040`..`046` (untuk memisahkan cakupan dari `BE-FIN-018`)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-011` (alokasi manual penuh, dasar `BE-FIN-018`)
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` — baris `BE-FIN-017` dan `BE-FIN-018`, untuk memastikan batas trace masing-masing
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (dibaca penuh sebelum dan sesudah refactor)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` — dikonfirmasi ulang sebagai satu-satunya penulis `OutstandingAmount` (bagian 1.3)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | **Baru** |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | -3 method (dipindah), +1 field/parameter konstruktor, `ProcessCollectionIntakeAsync` disederhanakan jadi satu panggilan, docstring diperbarui |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | +`using`; +`AddScoped<FinanceReceiptService>()` |
| `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-016.md` | +Addendum mencatat pemindahan lokasi (isi keputusan desain tetap berlaku) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller/DTO API baru |
| Database | `NONE` — nol perubahan skema, nol migration baru |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint. `GetInvoiceBreakdownAsync` hanya membaca dua tabel Finance sendiri, nol data pasien |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan bagian bawah | `PASS` | Dijalankan latar belakang, hasil disalin apa adanya |
| Review manual: refactor tidak mengubah perilaku `CreateSucceededReceiptAsync`/`CreateReversalReceiptAsync` | Dikonfirmasi — diff hanya pemindahan lokasi dan penggantian akses `_dbContext`/`_accountingOutboxService` (kini field milik `FinanceReceiptService`, sebelumnya `FinanceBillingIntakeService`); logika, urutan validasi, dan pesan galat identik | `PASS` | Bagian 1.1 |
| Review manual: `FinanceReceiptService.CreateFromTenderIntakeAsync` tidak membuka/commit/rollback transaksi sendiri | Dikonfirmasi — hanya `Add()`/`StageEventAsync` (yang juga tidak membuka transaksi), mengandalkan `SaveChangesAsync`/`CommitAsync` milik `FinanceBillingIntakeService.ProcessCollectionIntakeAsync` | `PASS` | `FinanceReceiptService.cs`, docstring kelas |
| Review manual: nol penulisan `FinReceivable` dari `FinanceReceiptService` | Dikonfirmasi — `GetInvoiceBreakdownAsync` murni `AsNoTracking()`, kedua method `Create*` hanya menulis `FinReceipt` | `PASS` | Bagian 1.3 |
| `UAT-05`, `UAT-20` | **Tidak dapat dijalankan** — memerlukan database sungguhan dengan migration `BE-FIN-016` diterapkan | `NOT RUN` | `BE-FIN-016` bagian 3.3 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — migration `BE-FIN-016` belum dijalankan, tidak ada database untuk
diuji end-to-end.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 12
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Pasien lunas tidak melahirkan piutang | **Terpenuhi secara struktural**, sejak `BE-FIN-016` — dikonfirmasi ulang, bukan diklaim baru (bagian 1.4) | Bagian 1.4 |
| Dobel-hitung dapat dibuktikan tidak terjadi | **Terpenuhi** — `GetInvoiceBreakdownAsync` (bagian 1.2) | Bagian 1.2 |
| Cakupan `FinanceReceiptService` | **Terpenuhi** — dibangun sesuai `02-backend-architecture.md` §4.22, mewarisi seluruh keputusan desain `BE-FIN-016` | Bagian 2.1 |

**Sengaja tidak dibangun**: alokasi manual maker-checker (`FR-FIN-040`..`046`) — cakupan
`BE-FIN-018`, bukan `BE-FIN-017` (bagian 1.3).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026 (2)** | `CreateReversalReceiptAsync` di file ini (`FinanceReceiptService.cs`) diimplementasikan penuh — baris pembalik `FinReceipt` (`SourceTenderId` kosong, `ReversalOfReceiptId` menunjuk baris asli, `Status = REVERSED`), kejadian `PEMBALIKAN-PENERIMAAN-KASIR` distage. Menutup blocker yang dicatat baris "Masalah yang diketahui" di bawah — lihat [laporan BE-FIN-016](BE-FIN-016.md) bagian 1.5 untuk riwayat konflik constraint dan perbaikannya |
| **Pembaruan 23 September 2026 (1)** | Peringatan di bawah soal integrasi `FinanceReceiptService`↔`FinanceReceivableService` sudah dijawab `BE-FIN-018`: `ApplyAllocationAsync`/`ReverseAllocationAsync` pada `FinanceReceivableService` menjadi satu-satunya jalur yang disentuh `FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync`, invariant satu-penulis `OutstandingAmount` tidak dilanggar — lihat [laporan BE-FIN-018](BE-FIN-018.md) bagian 0 |
| Peringatan (riwayat, sudah dijawab — lihat Pembaruan di atas) | `BE-FIN-018` (alokasi manual maker-checker) **MUST** memutuskan bagaimana `FinanceReceiptService`/`FinanceReceiptsController` masa depan berinteraksi dengan `FinanceReceivableService` tanpa melanggar invariant satu-penulis `OutstandingAmount` (bagian 1.3) — belum ada keputusan integrasi lintas service untuk ini |
| Masalah yang diketahui (riwayat, sudah ditutup) | Sama seperti `BE-FIN-016`: jalur `REVERSED` tetap `BLOCKED` menunggu keputusan skema (lihat `BE-FIN-016` bagian 1.5) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 1 berkas baru (`FinanceReceiptService.cs`), 2 berkas disunting (`FinanceBillingIntakeService.cs`, `BillingManagementServiceCollectionExtensions.cs`) pada task asli 22 September; `FinanceReceiptService.cs` berubah lagi 23 September (implementasi `CreateReversalReceiptAsync` dan `GetByIdAsync`) |
| Langkah berikutnya | `BE-FIN-018` dan `BE-FIN-019` selesai (controller dibangun 23 September 2026). `BE-FIN-020` masih menunggu controller (service sudah ada). Migration `20260923060000_FixFinReceiptTenderRequiredForReversal` menunggu otorisasi eksekusi, sama seperti migration Finance lain yang belum dijalankan |

