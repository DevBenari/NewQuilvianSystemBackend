# Finance Management — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-001
roadmap_revision: 3
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 2
blueprint_status: approved
created_at: 2026-09-20T00:00:00+07:00
planned_by: /quilvian-engineering-skills:plan-module-delivery
children:
  backend: roadmap/01-backend-roadmap.md — FIN-ROADMAP-BE-001
  frontend: roadmap/02-frontend-roadmap.md — FIN-ROADMAP-FE-001

backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: abed49b03

input_hashes:
  00-interview-decisions.md: 74529813218c0354e9289b4f9eea5a2bc68205572e195333b0a784e11cfccc2a
  01-existing-capability-map.md: 0576bf65224c76acd7853c18f04bf5683162f4620a15042ba95486a59a018b8a

approval_basis: >
  Yasmin (Product/Domain Owner Finance), 20 September 2026 — dua approval terpisah:
  (1) keputusan arsitektur FIN-DES-001..024; (2) cakupan MVP dan urutan gelombang pada
  04-prd-to-mvp.md bagian 7, 8, dan 20.1.
  Ditambah pada hari yang sama, sesudah revisi 1 roadmap ini:
  (3) FIN-DES-025..028; (4) penguncian tujuh kontrak turunan ke 1.0.
  Tidak ada lagi keputusan arsitektur Finance yang `draft`.
```

Roadmap ini menurunkan cakupan yang sudah disetujui menjadi task, dan menyebut dengan tegas apa
yang tertahan beserta pemiliknya. **Pembaruan 23 September 2026**: berkas ini semula (revisi 3,
20 September 2026) tidak menyatakan satu pun pekerjaan selesai — sejak itu 14 dari 21 task
`BE-FIN-*` sudah ✅ selesai dan UI brief frontend sudah closed; lihat bagian 4 dan 5 untuk
rinciannya per task, dan laporan tracked masing-masing untuk buktinya.

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 21 task `BE-FIN-*`, urutan eksekusi, rincian per gelombang, prasyarat, DoD backend |
| `02-frontend-roadmap.md` | 6 task `FE-FIN-*`, yang MUST diputuskan UI brief, `DEV_DISCRETION`, DoD frontend |

Yang tetap di sini: identitas, penguncian kontrak, gelombang, traceability lintas keduanya,
coverage gap, dan risiko.

## 2. Penguncian versi kontrak — **DISETUJUI 20 September 2026**

Owner menyetujui usulan penguncian revisi 1 apa adanya. Ketujuh kontrak kini `locked`.

| Kontrak | Dari | Terkunci pada | Cakupan penguncian |
|---|---|---|---|
| `contracts/api-contract.md` | `FIN-API-0.1` | `FIN-API-1.0` ✅ | Seluruh permukaan yang berakar `FIN-DES-001`..`028` |
| `contracts/state-transition-matrix.md` | `FIN-STATE-0.1` | `FIN-STATE-1.0` ✅ | Idem |
| `contracts/validation-matrix.md` | `FIN-VAL-0.1` | `FIN-VAL-1.0` ✅ | Idem |
| `contracts/permission-audit-matrix.md` | `FIN-PERM-0.1` | `FIN-PERM-1.0` ✅ | Idem |
| `contracts/integration-contract.md` | `FIN-INTEGRATION-0.1` | `FIN-INTEGRATION-1.0` ✅ | Intake Billing dan kotak keluar Accounting; **tidak** termasuk `BilCollectionHandoff` |
| `testing/acceptance-test-matrix.md` | `FIN-TEST-0.1` | `FIN-TEST-1.0` ✅ | Idem |
| `04-prd-to-mvp.md` | `FIN-MVP-0.1` | `FIN-MVP-1.0` ✅ | Bagian 7, 8, dan 20.1 yang sudah disetujui |

Rumpun Payable **ikut terkunci**: dasar pengecualiannya pada revisi 1 adalah status `draft`
`FIN-DES-025`..`028`, dan status itu sudah tercabut oleh approval hari ini.

**Tiga permukaan tetap tidak terkunci**, dan alasannya bukan status keputusan melainkan
ketergantungan pada pihak lain:

| Permukaan | Menunggu siapa |
|---|---|
| `BilCollectionHandoff` | Owner Billing (`FIN-DEC-005`) |
| Perluasan `BilArHandoff` untuk manfaat karyawan | Owner Billing + HR (`FIN-DEC-006`, `FIN-DEC-016`) |
| Pengiriman kejadian ke Accounting | Rizki — endpoint penerima belum ada (`FIN-CAP-018`) |

Ketiganya kelak **menambah permukaan baru** yang dikunci tersendiri; kedatangannya tidak
menaikkan versi ketujuh kontrak di atas.

**Konsekuensi penguncian:** kerja paralel backend–frontend kini **diizinkan** untuk seluruh task
yang kontraknya terkunci. Yang masih menahan task `FE-FIN-*` hanyalah ketiadaan UI brief —
dan itu keputusan Product Owner, bukan keterikatan kontrak.

## 3. Gelombang

Mengikuti `04-prd-to-mvp.md` bagian 20.1 apa adanya. Tidak ada epic yang dipindah, ditambah,
atau dihapus.

| Gelombang | Epic | Task | Keadaan |
|---|---|---|---|
| `MVP-0` | `FIN-01` | `BE-FIN-001`..`004` · `FE-FIN-001` | 🟡 Backend selesai 23 September 2026 (4 dari 4 task); `FE-FIN-001` source lengkap 23 September 2026, `npm run lint:errors`/`npm run build` belum dijalankan — [laporan](../task/report/frontend/FE-FIN-001.md) |
| `MVP-1` | `FIN-02`, `FIN-03` | `BE-FIN-005`..`009` · `FE-FIN-002` | ✅ Selesai 23 September 2026 — Backend (5 dari 5 task) & Frontend FE-FIN-002 selesai (lint & build PASS) |
| `MVP-5` | `FIN-11` | `BE-FIN-010`..`012` · `FE-FIN-006` | ✅ Selesai 23 September 2026 — Backend (3 dari 3 task) & Frontend FE-FIN-006 selesai (lint & build PASS) |
| `MVP-4` | `FIN-10` | `BE-FIN-013`..`015` · `FE-FIN-003` | ✅ Selesai 23 September 2026 — Backend (3 dari 3 task) & Frontend FE-FIN-003 selesai (lint & build PASS) |
| `MVP-2` | `FIN-05` | `BE-FIN-016`..`017` · `FE-FIN-004` | ✅ Backend selesai 23 September 2026 — owner Billing sudah menjawab, reversal diperbaiki |
| `MVP-3` | `FIN-06` | `BE-FIN-018` | ✅ Selesai 23 September 2026 — alokasi/pembalikan dan controller selesai |
| `POST-MVP` | `FIN-07` | `BE-FIN-019` | ✅ Selesai 23 September 2026 — entity, service, dan controller selesai |
| `POST-MVP` | `FIN-09` | `BE-FIN-020` | ✅ Selesai 23 September 2026 — entity, service, dan controller selesai; `FIN-OQ-010` (ambang nominal) tetap terbuka |
| `POST-MVP` | `FIN-08` | `BE-FIN-021` | 🟡 Sebagian 22 September 2026 — entity+configuration+migration selesai, intake BLOCKED `BE-MDF-014` |
| `POST-MVP` | `FIN-13` | `FE-FIN-005` | ✅ Selesai 23 September 2026 — Rute /finance/petty-cash-voucher, menu sidebar, & re-export bridges (lint & build PASS) |
| **Di luar gelombang** | `FIN-04`, `FIN-12` | — | `OPEN DECISION`, tidak diturunkan menjadi task |

### 3.1 Catatan urutan `MVP-4`

Bagian 20.1 menyatakan `MVP-4` dapat dikerjakan **lebih dahulu** bila `BilCollectionHandoff`
belum tersedia. Roadmap ini mengikuti itu, dengan satu batas yang MUST dihormati:

`FinanceCashManagementService` menghitung kas tersedia dari **kas kasir** (`FIN-CAP-006`,
`BilCashierShift`) — bukan dari `FinReceipt`. Selama `MVP-2` belum ada, task `BE-FIN-014`
MUST membaca kas kasir langsung dan MUST NOT membuat jalur sementara yang kelak dibongkar.
Bila implementer menemukan bahwa angka kas tersedia ternyata menuntut `FinReceipt`, itu temuan
yang MUST dilaporkan balik ke pass desain, bukan diselesaikan dengan improvisasi.

## 4. Indeks task

Rincian lengkap 11 kolom ada di berkas anak. Tabel ini hanya indeks.

### 4.1 Backend — `01-backend-roadmap.md`

| Task | Outcome ringkas | Gelombang | Keadaan |
|---|---|---|---|
| `BE-FIN-001` | Pendaftaran enam submodul ke registry | `MVP-0` | ✅ Selesai 21 September 2026 — [laporan](../task/report/backend/BE-FIN-001.md) |
| `BE-FIN-002` | Entity dan configuration data induk | `MVP-0` | ✅ Selesai 23 September 2026 — `MstBank` tidak dibuat baru (dipakai ulang dari Administrator), 3 entity lain selesai; `dotnet build` PASS — [laporan](../task/report/backend/BE-FIN-002.md) |
| `BE-FIN-003` | Migration `AddFinanceMasterData` | `MVP-0` | ✅ Selesai 23 September 2026 — file migration (3 tabel) dibuat tangan, sudah dieksekusi ke database — [laporan](../task/report/backend/BE-FIN-003.md) |
| `BE-FIN-004` | API data induk | `MVP-0` | ✅ Selesai 23 September 2026 — 2 dari 3 controller (grup Bank sengaja dilewati), diuji end-to-end — [laporan](../task/report/backend/BE-FIN-004.md) |
| `BE-FIN-005` | Pintu masuk fakta Billing | `MVP-1` | ✅ Selesai 23 September 2026 — entity+configuration selesai; service konsumen dibangun `BE-FIN-009` — [laporan](../task/report/backend/BE-FIN-005.md) |
| `BE-FIN-006` | Entity buku piutang | `MVP-1` | ✅ Selesai 23 September 2026 — 5 entity+configuration selesai, invariant seimbang sudah check constraint — [laporan](../task/report/backend/BE-FIN-006.md) |
| `BE-FIN-007` | Migration intake dan piutang | `MVP-1` | ✅ Selesai 23 September 2026 — file migration dieksekusi (6 dari 8 tabel; 2 tabel Collection dibangun terpisah di `BE-FIN-016`) — [laporan](../task/report/backend/BE-FIN-007.md) |
| `BE-FIN-008` | Layanan piutang dan umur piutang | `MVP-1` | ✅ Selesai 23 September 2026 — `FinanceReceivableService` selesai; 3 temuan kontrak tetap terbuka untuk ratifikasi (bukan blocker) — [laporan](../task/report/backend/BE-FIN-008.md) |
| `BE-FIN-009` | API intake dan piutang | `MVP-1` | ✅ Selesai 23 September 2026 — Receivable API selesai; 2 asumsi bisnis Billing Intake ditutup bukti source (`BillingArApHandoffService.cs`, `RegPatientEncounter.PatientId`) — [laporan](../task/report/backend/BE-FIN-009.md) |
| `BE-FIN-010` | Kotak keluar kejadian Accounting | `MVP-5` | ✅ Selesai 23 September 2026 — entity, configuration, dan migration `AddFinanceAccountingOutbox` dieksekusi; `FinSubledgerPeriodBalance` sengaja dilewati (bukan Cakupan, menunggu `FIN-OQ-011`) — [laporan](../task/report/backend/BE-FIN-010.md) |
| `BE-FIN-011` | Outbox ikut transaksi pemanggil | `MVP-5` | ✅ Selesai 23 September 2026 — `FinanceAccountingOutboxService` selesai, diperluas ke 3 pemanggilan nyata atas otorisasi eksplisit, QBE `PASS` (47 berkas) — [laporan](../task/report/backend/BE-FIN-011.md) |
| `BE-FIN-012` | Pantauan kejadian (baca saja) | `MVP-5` | ✅ Selesai 23 September 2026 — `FinanceAccountingEventsController` (4 endpoint `GET`) dan `FinanceAccountingEventService` selesai, QBE `PASS` (50 berkas) — [laporan](../task/report/backend/BE-FIN-012.md) |
| `BE-FIN-013` | Entity kas dan setoran | `MVP-4` | ✅ Selesai 23 September 2026 — entity `FinBankDeposit` dan `FinDailyCashSnapshot` selesai beserta EF configuration dan migration `AddFinanceCashManagement` dieksekusi, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-013.md) |
| `BE-FIN-014` | Kas tersedia dan penutupan harian | `MVP-4` | ✅ Selesai 23 September 2026 — `FinanceCashManagementService` selesai beserta DTO dan registrasi DI, saldo dihitung saat posting (`Serializable` + advisory lock), pembekuan penutupan kas harian, kas kecil tidak mengganggu kas kasir, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-014.md) |
| `BE-FIN-015` | API setoran dan kas harian | `MVP-4` | ✅ Selesai 23 September 2026 — `FinanceBankDepositsController` (7 endpoint) dan `FinanceDailyCashController` (4 endpoint) selesai, kepatuhan `role-access-rules.md`, QBE `PASS`, diuji end-to-end — [laporan](../task/report/backend/BE-FIN-015.md) |
| `BE-FIN-016` | Penerimaan dari tender kasir | `MVP-2` | ✅ Selesai 23 September 2026 — `FinReceipt`/`FinReceiptAllocation` selesai; `FinanceBillingIntakeService` diperluas untuk `HandoffType = COLLECTION`; jalur `REVERSED` diperbaiki (konflik constraint, bagian 1.5 laporan) — [laporan](../task/report/backend/BE-FIN-016.md) |
| `BE-FIN-017` | Pembagian bayar-vs-piutang | `MVP-2` | ✅ Selesai 23 September 2026 — `FinanceReceiptService` dibangun (refactor `BE-FIN-016` + `GetInvoiceBreakdownAsync` untuk `FR-FIN-035`); `CreateReversalReceiptAsync` diimplementasikan penuh; alokasi manual maker-checker cakupan `BE-FIN-018` — [laporan](../task/report/backend/BE-FIN-017.md) |
| `BE-FIN-018` | Alokasi, koreksi, penghapusan | `MVP-3` | ✅ Selesai 23 September 2026 — `ApplyAllocationAsync`/`ReverseAllocationAsync` (`FinanceReceivableService`) dan `AllocateAsync`/`ReverseAllocationAsync` (`FinanceReceiptService`) selesai (FR-FIN-040/041/042/045); FR-FIN-043/044/046 sudah terpenuhi sejak `BE-FIN-008`; controller `FinanceReceiptsController` dibangun — [laporan](../task/report/backend/BE-FIN-018.md) |
| `BE-FIN-019` | Utang supplier | `POST-MVP` | ✅ Selesai 23 September 2026 — `FinSupplierPayable`/`FinSupplierPayableItem`/`FinPayableAdjustment` dan `FinanceSupplierPayableService` (input manual, koreksi maker-checker, pembatalan) selesai; controller `FinanceSupplierPayablesController` dibangun — [laporan](../task/report/backend/BE-FIN-019.md) |
| `BE-FIN-020` | Pembayaran keluar dan potongan | `POST-MVP` | ✅ Selesai 23 September 2026 — `FinPayment`/`FinPaymentAllocation`/`FinPaymentDeduction` dan `FinancePaymentService` (siklus hidup lengkap, pembuktian `FR-FIN-050` & `FR-FIN-051`) selesai; controller `FinancePaymentsController` dibangun; `FIN-OQ-010` (ambang nominal) tetap terbuka — [laporan](../task/report/backend/BE-FIN-020.md) |
| `BE-FIN-021` | Utang jasa tenaga medis | `POST-MVP` | 🟡 Sebagian 22 September 2026 — `FinMedicalServicePayable`/`FinMedicalServicePayableItem` (entity+configuration+migration `AddFinanceMedicalServicePayable` ditulis tangan, belum dijalankan), navigasi polimorfik selesai, intake menunggu `BE-MDF-014`, QBE `PASS` — [laporan](../task/report/backend/BE-FIN-021.md) |

### 4.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-FIN-001` | Pengelolaan data induk | `BE-FIN-004` ✅ | 🟡 Source lengkap 23 September 2026 (dua fitur master data: Rekening Bank, Mata Uang+Kurs) — `npm run lint:errors`/`npm run build`/uji manual belum dijalankan — [laporan](../task/report/frontend/FE-FIN-001.md) |
| ✅ `FE-FIN-002` | Buku piutang dan umur piutang | `BE-FIN-009` ✅ | ✅ Selesai 23 September 2026 — daftar, rincian, 4 aging bucket cards, maker-checker koreksi/write-off, telusur tagihan asal selesai; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-002.md)) |
| ✅ `FE-FIN-003` | Setoran bank dan kas harian | `BE-FIN-015` ✅ | ✅ Selesai 23 September 2026 — daftar setoran bank, draf saldo kasir tersedia dari backend, detail siklus hidup setoran (posting/verifikasi/batal), riwayat kas harian, breakdown modal, penutupan kas beku; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-003.md)) |
| `FE-FIN-006` | Pemantauan fakta Billing dan kejadian Accounting | `BE-FIN-009` ✅, `BE-FIN-012` ✅ | UI brief closed 23 September 2026 (`FIN-DEC-024`..`029`); kedua dependency selesai — siap menunggu wewenang tulis frontend |
| `FE-FIN-004` | Penerimaan dan alokasi | `BE-FIN-018` ✅ | Owner Billing sudah menjawab 21 September 2026; `BE-FIN-016`..`018` selesai 23 September 2026. UI brief closed (`FIN-DEC-024`..`029`) — tidak lagi `BLOCKED`, siap menunggu wewenang tulis frontend |
| ✅ `FE-FIN-005` | Merapikan Petty Cash ke rute Finance | — | ✅ Selesai 23 September 2026 — slices/hooks/constants dipindahkan ke finance dengan jembatan re-export tanpa regresi, halaman voucher `/finance/petty-cash-voucher` dibangun, menu sidebar terdaftar; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-005.md)) |

`FE-FIN-006` ditambahkan saat roadmap dipecah: `03-frontend-architecture.md` bagian 3.5
menuntut dua layar pemantauan, dan keduanya sebelumnya tidak punya task frontend sama sekali.

### 4.3 Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`) |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`) |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable` |
| Migration `AddFinanceSubledgerPeriodBalance` | `FIN-OQ-011` belum dijawab; rumpunnya `POST-MVP` |

## 5. Traceability

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-FIN-001`..`004` | `FIN-DEC-013`, `FIN-DEC-014` | `FIN-DES-001`..`005` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-001`..`004` | `FE-FIN-001` | `UAT-01`, `UAT-02` | 🟡 Backend selesai 23 September 2026 — `BE-FIN-001` ✅ 21 September 2026 ([laporan](../task/report/backend/BE-FIN-001.md)); `BE-FIN-002` ✅, `MstBank` dipakai ulang dari Administrator bukan dibuat baru ([laporan](../task/report/backend/BE-FIN-002.md)); `BE-FIN-003` ✅, migration dieksekusi ([laporan](../task/report/backend/BE-FIN-003.md)); `BE-FIN-004` ✅, 2 dari 3 controller, `UAT-01`/`UAT-02` diuji end-to-end ([laporan](../task/report/backend/BE-FIN-004.md)). Frontend `FE-FIN-001` 🟡 source lengkap 23 September 2026, `UAT-01`/`UAT-02` belum dibuktikan runtime, validasi `npm` belum dijalankan ([laporan](../task/report/frontend/FE-FIN-001.md)) |
| `FR-FIN-010`..`013` | `FIN-DEC-005` (konsumsi) | `FIN-DES-008`, `009` | `FIN-INTEGRATION-1.0` | `BE-FIN-005`, `009` | `FE-FIN-006` | `UAT-04` | ✅ Selesai 23 September 2026 — `BE-FIN-005` ([laporan](../task/report/backend/BE-FIN-005.md)); `FinanceBillingIntakeService` selesai dibangun `BE-FIN-009` (otorisasi eksplisit, menutup gap `BE-FIN-005`), 2 asumsi bisnis ditutup bukti source, `UAT-04` terpenuhi ([laporan](../task/report/backend/BE-FIN-009.md)) |
| `FR-FIN-020`..`024` | `FIN-DEC-010`..`012` | `FIN-DES-010`..`013` | `FIN-VAL-1.0` | `BE-FIN-006`..`009` | `FE-FIN-002` | `UAT-03` | ✅ Selesai 23 September 2026 — `BE-FIN-006` entity+configuration selesai, invariant seimbang sudah check constraint ([laporan](../task/report/backend/BE-FIN-006.md)); `BE-FIN-007` migration dieksekusi, 2 tabel Collection dibangun terpisah di `BE-FIN-016` ([laporan](../task/report/backend/BE-FIN-007.md)); `BE-FIN-008` `FinanceReceivableService` selesai, 3 temuan kontrak terbuka untuk ratifikasi (tidak menahan) ([laporan](../task/report/backend/BE-FIN-008.md)); `BE-FIN-009` `FinanceReceivablesController` selesai, `FR-FIN-024` (PatientId) kini terpenuhi ([laporan](../task/report/backend/BE-FIN-009.md)). Frontend `FE-FIN-002` ✅ selesai 23 September 2026, `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-002.md)) |
| `FR-FIN-030`..`035` | `FIN-DEC-005`, `FIN-DEC-015` | `FIN-DES-014` | `FIN-INTEGRATION-1.0` — permukaan collection dikecualikan | `BE-FIN-016`, `017` | `FE-FIN-004` | `UAT-05`, `06`, `20` | ✅ Selesai 23 September 2026 — `BE-FIN-016` `FinReceipt`/`FinReceiptAllocation`+konsumsi `COLLECTION` selesai, jalur `REVERSED` diperbaiki ([laporan](../task/report/backend/BE-FIN-016.md)); `BE-FIN-017` `FinanceReceiptService`+pembuktian `FR-FIN-035`+`CreateReversalReceiptAsync` selesai ([laporan](../task/report/backend/BE-FIN-017.md)); migration `AddFinanceCollection`/`FixFinReceiptTenderRequiredForReversal` masih menunggu eksekusi |
| `FR-FIN-040`..`046` | `FIN-DEC-017` | `FIN-DES-014` | `FIN-STATE-1.0` | `BE-FIN-018` | `FE-FIN-004` | `UAT-08`..`12` | ✅ Selesai 23 September 2026 — `FR-FIN-040`/`041`/`042`/`045` selesai (`AllocateAsync`/`ReverseAllocationAsync`); `FR-FIN-043`/`044`/`046` sudah terpenuhi sejak `BE-FIN-008`; controller `FinanceReceiptsController` dibangun ([laporan](../task/report/backend/BE-FIN-018.md)) |
| `FR-FIN-050`, `051` | `FIN-DEC-019` | `FIN-DES-015`, `026`..`028` | `FIN-API-1.0`, `FIN-VAL-1.0` | `BE-FIN-020` | — | — | ✅ Selesai 23 September 2026 — `FinPayment`/`FinPaymentAllocation`/`FinPaymentDeduction`+`FinancePaymentService` membuktikan uang keluar berbeda dari utang lunas (`FR-FIN-050`) dan potongan tidak menyisakan utang (`FR-FIN-051`); controller dibangun ([laporan](../task/report/backend/BE-FIN-020.md)) |
| `FR-FIN-060`..`065` | `FIN-DEC-018` | `FIN-DES-018`..`020` | `FIN-VAL-1.0` | `BE-FIN-013`..`015` | `FE-FIN-003` | `UAT-13`..`16` | ✅ Backend selesai 23 September 2026 — `BE-FIN-013` entity+configuration+migration dieksekusi ([laporan](../task/report/backend/BE-FIN-013.md)); `BE-FIN-014` service selesai, saldo dihitung saat posting ([laporan](../task/report/backend/BE-FIN-014.md)); `BE-FIN-015` 2 controller selesai, QBE `PASS` ([laporan](../task/report/backend/BE-FIN-015.md)); ketiganya diuji end-to-end dengan hasil sesuai ekspektasi |
| `FR-FIN-070`..`074` | `FIN-DEC-001`, `004` | `FIN-DES-017`, `021`..`023` | `FIN-INTEGRATION-1.0`, `ACC-XMOD-0.2` | `BE-FIN-010`..`012` | `FE-FIN-006` | `UAT-07`, `UAT-17`..`19` | ✅ Backend selesai 23 September 2026 — `BE-FIN-010` entity+configuration+migration dieksekusi ([laporan](../task/report/backend/BE-FIN-010.md)); `BE-FIN-011` service+3 pemanggilan selesai, QBE `PASS` (47 berkas) ([laporan](../task/report/backend/BE-FIN-011.md)); `BE-FIN-012` `FinanceAccountingEventsController`+`Service` selesai, QBE `PASS` (50 berkas) ([laporan](../task/report/backend/BE-FIN-012.md)); ketiganya diuji end-to-end dengan hasil sesuai ekspektasi |
| `FR-FIN-075` | `FIN-DEC-003`, `019` | `FIN-DES-025` | `FIN-API-1.0` | `BE-FIN-021` | — | — | 🟡 Sebagian 22 September 2026 — model+skema selesai, intake menunggu `BE-MDF-014` ([laporan](../task/report/backend/BE-FIN-021.md)) |
| Manfaat karyawan | `FIN-DEC-006`, `016` | — | — | — | — | — | `OPEN DECISION` |
| Pengiriman ke Accounting | `FIN-DEC-007` | `FIN-DES-024` | — | — | — | — | `OPEN DECISION` |

## 6. Coverage gap

| Gap | Akibat | Pemilik |
|---|---|---|
| `FR-FIN-051` (potongan tidak menyisakan utang) tanpa baris uji pada `FIN-TEST-0.1` | Aturan paling halus pada rumpun pembayaran tanpa penjaga | Pass desain revisi 2 |
| Rumpun manfaat karyawan tanpa requirement yang lengkap | Tidak dapat direncanakan; sudah dikeluarkan dari seluruh gelombang | Owner Billing + HR |
| Katalog 17 jenis kejadian Accounting belum diratifikasi Rizki | Kejadian yang jenisnya belum terdaftar akan tertahan saat pengiriman aktif | Rizki (Accounting) |
| Ambang nominal approval AP (`FIN-OQ-010`) | `BE-FIN-020` tidak dapat mengunci aturan validasi angkanya | Finance Supervisor + Yasmin |

Seluruh 20 skenario UAT pada `04-prd-to-mvp.md` bagian 18 sudah tertaut ke task. Yang tersisa
pada tabel di atas adalah gap yang pemiliknya berada **di luar** roadmap ini.

## 7. Prasyarat eksekusi

Rinciannya ada di berkas anak: `01-backend-roadmap.md` bagian 6 dan `02-frontend-roadmap.md`
bagian 5. Ringkasnya:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama | ✅ **Sudah** — selesai 21 September 2026. Lihat [laporan](../task/report/backend/BE-FIN-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | ✅ **Sudah** — otorisasi pembuatan file diberikan pemilik repository 21 September 2026 (lihat [laporan BE-FIN-003](../task/report/backend/BE-FIN-003.md)); otorisasi **eksekusi** (`dotnet ef database update`) diberikan dan seluruh migration Finance sudah diterapkan — dikonfirmasi pengguna 23 September 2026 |
| 4 | Implementasi lewat `quilvian-engineering-skills:build-module-backend` | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |
| 7 | UI brief disetujui Product Owner sebelum task `FE-FIN-*` pertama | ✅ **Sudah** — closed 23 September 2026, `FIN-DEC-024`..`029` di `00-interview-decisions.md` |

## 8. Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| `BilCollectionHandoff` tidak kunjung dikonfirmasi | `MVP-2` dan `MVP-3` berhenti | `MVP-4` sudah diizinkan berjalan lebih dahulu; `MVP-5` paralel |
| Kontrak terkunci ternyata keliru saat implementasi | Perubahan kontrak `1.0` kini berbiaya lebih mahal daripada saat `draft` | Temuan seperti itu MUST dinaikkan sebagai revisi kontrak bernomor, bukan diselesaikan diam-diam di kode |
| Kas tersedia ternyata menuntut `FinReceipt` | Urutan `MVP-4` sebelum `MVP-2` gugur | Catatan 3.1 mewajibkan temuan itu dilaporkan balik, bukan diakali |
| UI brief tidak kunjung ada | Seluruh `FE-FIN-*` berhenti | Backend tidak tertahan; kontrak fungsional sudah berdiri |
| Endpoint Accounting dibangun lebih cepat dari perkiraan | `EPIC FIN-12` dapat dimulai lebih awal | Kotak keluar sudah terisi sejak `MVP-5`; tinggal mengirim antrean |

## 9. Yang roadmap ini tidak lakukan

- Tidak menandai satu pun task selesai.
- Tidak menetapkan approval sendiri: ketiga approval pada revisi 2 ini diberikan owner secara langsung dan dicatat apa adanya.
- Tidak menyembunyikan satu pun dependency eksternal: keempat pemiliknya disebut namanya.
- Tidak menjalankan builder mana pun.
- Tidak mengubah cakupan MVP yang sudah dikunci owner.
