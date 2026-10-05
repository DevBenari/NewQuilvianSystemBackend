# Laporan Perubahan Backend — `BE-FIN-069`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-069` |
| Judul | Kejadian penerimaan membawa nomor shift dan metode pembayaran, sehingga Accounting dapat meringkasnya per shift |
| Slice | `REV-14C` — `EPIC FIN-22`, jalur pengiriman |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14C — `EPIC FIN-22`, jalur pengiriman" |
| Trace | `FR-FIN-158`, `FR-FIN-159`; `FIN-DEC-111`, `FIN-DEC-120`; `FIN-DES-083` |
| Contract version | `FIN-INTEGRATION-1.7` §5.12.1-5.12.2. **Catatan ketidaksesuaian dokumen** (dilaporkan, tidak diperbaiki sepihak): header tabel pada `contracts/integration-contract.md` bagian 5.12 masih menulis status `draft`, sedangkan metadata `roadmap/01-backend-roadmap.md` REV-14 dan prasyarat eksekusinya menyatakan seluruh kontrak turunan `FIN-DES-078..091` **approved 1 Oktober 2026**. Task ini tetap dikerjakan karena baris task `BE-FIN-069` sendiri eksplisit menyatakan "Pembangunannya tidak tertahan" oleh gerbang `FIN-OQ-045` |
| Dependency | `—` (berdiri sendiri, sesuai roadmap) |
| Klasifikasi | `MEDIUM` — repo 0 + berkas diperiksa >20 (2) + berkas diubah ≤3 (0) + logika sedang (1) + kontrak diubah (2) + database tidak ada (0) + auth tidak ada (0) + UI tidak ada (0) = 5 |
| Task mode | `BACKEND` (target tulis backend; frontend tidak disentuh — tidak ada task frontend pasangan pada baris `FR-FIN-158`/`159`) |
| Target tulis | `NewQuilvianSystemBackend`, terbatas pada `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` dan `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs`, ditambah laporan tracked ini dan pembaruan register roadmap |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B sebelumnya; lihat bagian 7 "Status Git") |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap dan seluruh acceptance criteria yang dapat diverifikasi lewat pembacaan kode sudah terpenuhi; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, `FinAccountingEventOutbox.PayloadJson` yang dikirim ke kotak masuk Accounting untuk
setiap kejadian penerimaan (kasir maupun pembaliknya) hanya memuat 12 field wajib `ACC-XMOD-0.2`
ditambah `Components`/`SubledgerBalance` opsional. Accounting tidak dapat mengetahui **shift kasir**
mana yang menghasilkan sebuah penerimaan, **metode pembayaran** apa yang dipakai, atau **kuitansi
asli** mana yang sedang dibalik oleh sebuah kejadian pembalik.

Akibatnya, Accounting tidak dapat meringkas jurnal penerimaan **per shift** (`ACC-DEC-062`) maupun
menelusuri pasangan penerimaan↔pembalik ketika keduanya jatuh pada shift atau periode yang berbeda —
padahal `FIN-DEC-120` menetapkan kejadian pembalik membawa shift **saat pembalikan terjadi**, bukan
shift kuitansi aslinya, sehingga satu pasangan penerimaan dan pembaliknya dapat tersebar di dua shift
bahkan dua periode akuntansi.

Contoh konkret: kasir shift pagi menerima Rp 150.000 tunai pukul 08.00 (`PENERIMAAN-KASIR`), lalu
penerimaan itu dibalik oleh kasir shift sore pukul 15.00 (`PEMBALIKAN-PENERIMAAN-KASIR`) karena
kesalahan input. Tanpa ruas baru ini, Accounting menerima dua baris jurnal tanpa cara mengaitkan
keduanya sebagai satu pasangan, dan tanpa cara mengetahui shift pagi mana yang terdampak.

---

## 2. Proses bisnis

**Tujuan.** Menambahkan lima ruas dimensi aditif ke `PayloadJson` setiap kejadian Accounting, dan
mengisinya pada kejadian penerimaan kasir beserta pembaliknya, tanpa mengubah skema database maupun
12 field wajib yang sudah ada.

**Pelaku.** `FinanceAccountingOutboxService` (satu-satunya penulis `FinAccountingEventOutbox`,
FIN-DES-017) dan `FinanceReceiptService` (pemilik logika penerimaan, 02-backend-architecture.md
§4.22) sebagai pemanggil.

**Pemicu dan langkah berurutan:**

1. Billing menyerahkan fakta penerimaan (`BilCollectionHandoff`) ke
   `FinanceBillingIntakeService.ProcessCollectionIntakeAsync`, yang mendelegasikan pembuatan
   `FinReceipt` ke `FinanceReceiptService.CreateFromTenderIntakeAsync`.
2. **Jalur normal (`CreateSucceededReceiptAsync`).** Metode pembayaran pada handoff sudah dimuat untuk
   validasi tunai yang sudah ada sebelumnya (`isCash`/`CashierShiftId`). Task ini menambah satu
   pembacaan baru: nomor shift manusiawi (`BilCashierShift.ShiftNumber`) diambil lewat
   `ResolveCashierShiftNumberAsync` bila `CashierShiftId` terisi. Kelima ruas
   (`CashierShiftId`, `CashierShiftNumber`, `PaymentMethodCode`, `PaymentMethodAccountId`,
   `ReversalOfSourceTransactionId = null`) diteruskan ke `StageEventAsync`.
3. **Jalur pembalik (`CreateReversalReceiptAsync`).** Sesuai `FIN-DEC-120`, shift dan metode pembayaran
   yang dikirim adalah milik **handoff pembalikan ini** (shift saat pembalikan terjadi), bukan
   disalin dari `original`. Task ini menambah satu pembacaan metode pembayaran (sebelumnya tidak
   pernah di-lookup di jalur ini) dan satu pembacaan nomor shift, lalu mengisi
   `ReversalOfSourceTransactionId = original.ReceiptNumber` — nomor kuitansi asli, persis seperti
   yang dipakai sebagai `SourceTransactionId` kejadian asli.
4. `FinanceAccountingOutboxService.BuildPayloadJson` menuliskan kelima ruas itu ke `PayloadJson`
   **selalu**, termasuk ketika nilainya `null` (berbeda dari `Components`/`SubledgerBalance` yang
   disembunyikan total bila kosong) — sesuai pola yang dipakai 12 field wajib yang sudah ada, dan
   sesuai contoh payload pada `FIN-INTEGRATION-1.7` §5.12.1 yang menampilkan
   `"PaymentMethodAccountId": null` apa adanya.

**Aturan yang berlaku.** `FIN-DEC-111` (lima ruas dimensi baru), `FIN-DEC-120` (shift pembalik
memakai shift saat pembalikan). Tidak ada aturan validasi baru pada `FIN-VAL-1.7` bagian F.7
(`210`-`213`) yang menyinggung kelima ruas ini — diperiksa dan dipastikan tidak ada.

**Jalur tidak normal.** Penerimaan non-tunai tanpa shift (`CashierShiftId == null`) menghasilkan
`CashierShiftId`/`CashierShiftNumber` bernilai `null` pada payload — bukan error, sesuai sifat
"uuid?"/"string?" opsional pada kontrak. Metode pembayaran yang sudah dihapus (soft-delete) di antara
penerimaan asli dan pembalikannya menghasilkan `PaymentMethodCode = null` pada kejadian pembalik
(bukan exception) — ini **tidak** menahan pembalikan karena field ini murni informasi tambahan bagi
Accounting, bukan syarat bisnis pembalikan itu sendiri.

**Hasil akhir.** `PayloadJson` setiap kejadian yang distage lewat `FinanceAccountingOutboxService`
kini selalu membawa lima ruas baru, terisi nyata pada kejadian penerimaan kasir dan pembaliknya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` (backend, `NewQuilvianSystemBackend`)
- `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REPORT_TEMPLATE.md`, `REVIEW_RULES.md` (suite skill)
- `rules/rule-output/lokasi-laporan-task.md`, `bentuk-blueprint.md`, `status-task-roadmap.md`, `grafik-dependency-roadmap.md` (suite skill)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` (repository ini — canonical, bukan salinan suite skill)
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, `00-delivery-roadmap.md`
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§5.12.1, §5.12.2), `validation-matrix.md`
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` (baris `FR-FIN-158`, `FR-FIN-159`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs`
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs`
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (memastikan `handoff` tidak membawa navigation property ter-load, sehingga pola lookup eksplisit yang sudah ada tetap diikuti)
- `Areas/HealthServices/BillingManagement/Billing/Models/BilCollectionHandoff.cs`
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs`
- `Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs`
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` dan `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` (diperiksa untuk menilai cakupan — lihat catatan gap pada bagian 7)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` | Menambah lima property nullable baru (`CashierShiftId`, `CashierShiftNumber`, `PaymentMethodCode`, `PaymentMethodAccountId`, `ReversalOfSourceTransactionId`) pada `AccountingOutboxEventRequest`, dan menambah kelima key itu ke dictionary payload pada `BuildPayloadJson` — selalu ditulis (termasuk `null`), tidak dikondisikan seperti `Components`/`SubledgerBalance`. Tidak ada kolom baru pada entity `FinAccountingEventOutbox` dan tidak ada migration |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | Menambah method privat `ResolveCashierShiftNumberAsync` (lookup `BilCashierShift.ShiftNumber` ber-`AsNoTracking`, `null` bila `CashierShiftId` kosong). Di `CreateSucceededReceiptAsync`: memanggil lookup itu dan meneruskan `CashierShiftId`, `CashierShiftNumber`, `PaymentMethodCode` (dari `paymentMethod` yang sudah dimuat), `PaymentMethodAccountId` ke `StageEventAsync`. Di `CreateReversalReceiptAsync`: menambah lookup `PaymentMethodCode` baru (sebelumnya tidak pernah di-lookup di jalur ini) dan lookup shift, lalu meneruskan keempat ruas itu ditambah `ReversalOfSourceTransactionId = original.ReceiptNumber` ke `StageEventAsync` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint HTTP yang berubah. Dampaknya pada kontrak **internal** `PayloadJson` yang dikirim ke kotak masuk Accounting: aditif murni (lima key baru, 12 field wajib lama tidak diubah urutan maupun isinya). Sesuai `FIN-INTEGRATION-1.7` §5.12.1: "Dampak kompatibilitas: Aditif pada payload" |
| Database | **NOT APPLICABLE.** Nol kolom baru pada `FinAccountingEventOutbox`, nol model baru, nol migration — sesuai DoD roadmap "Build PASS; nol migration" |
| Keamanan/Auth | **NOT APPLICABLE.** Tidak ada endpoint, `[Authorize]`, `[AccessController]`/`[AccessAction]`/`[AccessPermission]`, atau `AccessTypes` yang disentuh. Kelima ruas baru bukan data sensitif pasien (`FR-FIN-073` tetap terjaga — tidak ada DoctorId/data pasien yang ditambahkan ke payload) |

---

## 4. Dokumentasi endpoint

**NOT APPLICABLE.** Task ini tidak membuat maupun mengubah endpoint HTTP. Perubahan murni pada
bentuk `PayloadJson` internal yang ditulis `FinanceAccountingOutboxService` dan pada dua method
privat/internal `FinanceReceiptService`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna pada task ini: "jangan lakukan build backend secara automatis". Sesuai `rules/backend/TEST_POLICY.md` dan `status-task-roadmap.md` §2.1, ketiadaan hasil build sebenarnya menahan status `✅` — task ini karena itu ditandai `🟡`, bukan diklaim `PASS` tanpa dijalankan |
| Pembacaan kode: lima ruas muncul di `AccountingOutboxEventRequest` dan `BuildPayloadJson` | Terverifikasi lewat pembacaan source — lihat bagian 3.2 | `PASS` | `FinanceAccountingOutboxService.cs` baris properti baru dan dictionary `BuildPayloadJson` |
| Pembacaan kode: kejadian penerimaan kasir (`CreateSucceededReceiptAsync`) mengisi `CashierShiftId`/`CashierShiftNumber`/`PaymentMethodCode`/`PaymentMethodAccountId` | Terverifikasi — keempat ruas diteruskan dari `handoff` dan `paymentMethod` yang sudah dimuat, ditambah lookup nomor shift baru | `PASS` | `FinanceReceiptService.cs`, method `CreateSucceededReceiptAsync` |
| Pembacaan kode: kejadian pembalik (`CreateReversalReceiptAsync`) mengisi shift **saat pembalikan** (bukan shift asli) dan `ReversalOfSourceTransactionId = original.ReceiptNumber` | Terverifikasi — `handoff.CashierShiftId`/`handoff.PaymentMethodId` dipakai (bukan `original.CashierShiftId`), dan `ReversalOfSourceTransactionId` diisi `original.ReceiptNumber`, sama persis dengan `SourceTransactionId` yang dipakai saat kejadian asli di-stage | `PASS` | `FinanceReceiptService.cs`, method `CreateReversalReceiptAsync` |
| Pembacaan kode: idempotensi tetap memakai `SourceTransactionId`+`EventTypeCode`+`SourceVersion` | Terverifikasi — `ResolveNextSourceVersionAsync` dan `ValidateRequest` tidak disentuh sama sekali oleh task ini | `PASS` | `FinanceAccountingOutboxService.cs`, method `ResolveNextSourceVersionAsync` (tidak berubah) |
| Contoh payload (ilustratif, bukan data nyata — pola sama dengan contoh `FIN-INTEGRATION-1.7` §5.12.1) | Dilampirkan di bawah | `PASS` | Lihat blok JSON berikut |

**Contoh payload penerimaan kasir (normal):**

```json
{
  "EventNumber": "EVT-20261002-9f1c...",
  "EventTypeCode": "PENERIMAAN-KASIR",
  "SourceModule": "Finance",
  "SourceTransactionId": "RCP-20261002-aa01...",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-10-02T01:10:00+00:00",
  "AccountingDate": "2026-10-02",
  "Amount": 150000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "6e2f....",
  "CorrelationId": "c1a2....",
  "CausationId": "c1a2....",
  "CashierShiftId": "0f6f1a9c-....-000000000012",
  "CashierShiftNumber": "SHIFT-20261002-12",
  "PaymentMethodCode": "CASH",
  "PaymentMethodAccountId": null,
  "ReversalOfSourceTransactionId": null
}
```

**Contoh payload kejadian pembalik (shift berbeda dari penerimaan asli):**

```json
{
  "EventNumber": "EVT-20261002-7bd4...",
  "EventTypeCode": "PEMBALIKAN-PENERIMAAN-KASIR",
  "SourceModule": "Finance",
  "SourceTransactionId": "RCP-20261002-bb02...",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-10-02T08:00:00+00:00",
  "AccountingDate": "2026-10-02",
  "Amount": 150000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "6e2f....",
  "CorrelationId": "c1a2....",
  "CausationId": "c1a2....",
  "CashierShiftId": "1a2b3c4d-....-000000000013",
  "CashierShiftNumber": "SHIFT-20261002-13",
  "PaymentMethodCode": "CASH",
  "PaymentMethodAccountId": null,
  "ReversalOfSourceTransactionId": "RCP-20261002-aa01..."
}
```

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan runtime API/database aktif, di luar cakupan
perubahan murni source tanpa migration dan tanpa eksekusi aplikasi pada task ini.

**Tidak dijalankan:** `dotnet build` (lihat tabel di atas), uji manual end-to-end (`NOT FEASIBLE`,
memerlukan lingkungan runtime), dan `dotnet test` (backend tidak memelihara project automated test —
`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Payload memuat kelima ruas | Terpenuhi (source) | `BuildPayloadJson` menulis kelima key secara selalu — bagian 3.2, 5 |
| Kuitansi pembalik membawa shift **pembalikan** (bukan shift asli) beserta `ReversalOfSourceTransactionId` kuitansi asli | Terpenuhi (source) | `CreateReversalReceiptAsync` memakai `handoff.CashierShiftId` (shift pembalikan) dan `original.ReceiptNumber` — bagian 2 langkah 3, bagian 5 |
| Idempotensi tetap memakai `SourceTransactionId`+`EventTypeCode`+`SourceVersion` | Terpenuhi (tidak berubah) | `ResolveNextSourceVersionAsync` tidak disentuh — bagian 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna pada task ini | Bagian 5 |
| Nol migration | Terpenuhi | Bagian 3.3 — nol model/kolom baru |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna pada task ini ("tapi jangan lakukan build backend secara automatis"). Ini **bukan**
pengecualian DoD yang melepas butir tersebut secara permanen (lihat `status-task-roadmap.md` §2.2) —
build tetap wajib dijalankan sebelum task ini dapat dinaikkan ke `✅`, hanya saja bukan pada sesi ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` belum pernah dijalankan terhadap perubahan ini. Risiko compile error (misalnya salah nama property atau tipe) belum tersingkirkan oleh bukti build — hanya oleh pembacaan source yang cermat |
| Masalah yang diketahui | **Gap cakupan vs tabel kontrak.** `FIN-INTEGRATION-1.7` §5.12.1 mencantumkan `PaymentMethodCode`/`PaymentMethodAccountId` "Terisi pada: Penerimaan kasir, penerimaan piutang, pembayaran utang supplier" — tetapi kolom Reuse/Cakupan task `BE-FIN-069` pada roadmap hanya menyebut `FinReceipt` (jalur `FinanceReceiptService`). Task ini **sengaja tidak** mengisi kelima ruas pada `FinanceReceivableService.RecordPaymentAsync` (kejadian `PENERIMAAN-PIUTANG`, baris ±750-762) maupun `FinancePaymentService` (kejadian `PEMBAYARAN-HUTANG-SUPPLIER`, baris ±660-670) karena keduanya tidak disebut pada kolom Reuse/Cakupan `BE-FIN-069`, dan menambahkannya tanpa wewenang task eksplisit berarti memperluas scope sepihak. Dilaporkan sebagai gap kontrak, bukan dikerjakan diam-diam atau diabaikan — pemilik modul perlu memutuskan apakah ini menjadi task baru |
| Risiko tersisa | Kontrak `FIN-INTEGRATION-1.7` masih berstatus `draft` pada header dokumennya sendiri (lihat Metadata). `FIN-OQ-045` (persetujuan Accounting atas kelima ruas ini) belum dijawab — sesuai task, ini tidak menahan **pembangunan**, tetapi berarti kelima ruas ini belum resmi dipakai Accounting sampai `FIN-OQ-045` dijawab |
| Perubahan sampingan | `NONE` — kedua berkas yang diubah sudah membawa perubahan tidak ter-commit dari task `REV-14A`/`REV-14B` sebelumnya (lihat Status Git); task ini hanya menambah perubahan di atasnya, tidak memulihkan maupun mengubah apa pun dari task-task itu |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir task menunjukkan dua berkas task ini (`FinanceAccountingOutboxService.cs`, `FinanceReceiptService.cs`) masih tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`068` (REV-14A/14B) yang memang belum di-commit sejak sebelum task ini dimulai — lihat baris `git status` lengkap pada riwayat percakapan. Tidak ada file di luar scope Finance Management yang tersentuh oleh task ini |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` dan mengonfirmasi hasilnya, lalu laporan ini dan roadmap dinaikkan ke `✅`; (2) pemilik modul memutuskan apakah gap `PENERIMAAN-PIUTANG`/`PEMBAYARAN-HUTANG-SUPPLIER` di atas perlu task baru; (3) `FIN-OQ-045` tetap menunggu jawaban Accounting sebelum kelima ruas ini dipakai produksi |
