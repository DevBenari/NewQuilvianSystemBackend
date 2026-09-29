# Finance Management — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-001
roadmap_revision: 10
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 9
blueprint_status: approved
created_at: 2026-09-20T00:00:00+07:00
planned_by: /quilvian-engineering-skills:plan-module-delivery
children:
  backend: roadmap/01-backend-roadmap.md — FIN-ROADMAP-BE-001 revisi 8
  frontend: roadmap/02-frontend-roadmap.md — FIN-ROADMAP-FE-001 revisi 7
roadmap_revision_10_note: >
  Revisi 10 (29 September 2026) MENCABUT TANDA BLOCKER ⛔ PADA BE-FIN-047 menyusul disahkannya
  FIN-DES-064 dan FIN-DES-065 via FIN-DEC-080 dan FIN-DEC-081 (Amendment Pass). Pemicu kejadian
  PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT resmi disahkan: mutasi BilDepositMovement bertipe RELEASE
  yang berpasangan dengan REVERSAL pada SettlementId yang sama. Pemicu kode pengembalian uang muka
  dipersempit murni ke BilRefundCase EXECUTED. Task BE-FIN-047 kini SIAP DIKERJAKAN pada gelombang
  R6-4 setelah BE-FIN-046. Gap coverage ditutup dan keterkaitan traceability diperbarui.
roadmap_revision_9_note: >
  Revisi 9 (29 September 2026) menurunkan desain AMENDMENT REVISI 6/7 (FIN-DES-051..060, approved
  28 September 2026) yang belum punya task, dan memperbaiki isi task lama yang acceptance
  criteria-nya bertentangan dengan desain itu.
  NOL task frontend BARU ditambahkan, tetapi roadmap frontend TETAP BERGERAK: FE-FIN-007 diperbaiki
  isinya (tanda ⛔ dicabut, outcome "tujuh jenis kejadian" menjadi katalog final, dua sebab baris
  intake ERROR ditambahkan) dan menerima satu prasyarat baru BE-FIN-025.
  Tiga task lama diperbaiki: BE-FIN-023 (daftar tertutup kode penanda bernilai nol + properti
  Components dihilangkan; acceptance criteria "selisih kas -30.000" DICABUT), BE-FIN-025 (kode
  selisih kas dipecah dua berkunci BilCashierShift.Id, refund SETTLEMENT masuk jalur yang sudah ada,
  REFERRED_OUTPATIENT_ADMIN menjadi baris intake ERROR), BE-FIN-026 (rumpun kedua: baris outbox
  bernama pendek dihitung dan dilaporkan). BE-FIN-024 diperiksa dan TIDAK perlu diubah.
  Tiga task baru: BE-FIN-045 (penanda shift tertutup dan pembaliknya, worker digerbang FIN-OQ-035),
  BE-FIN-046 (pembalikan tender top-up deposit, pemicunya sudah ada sejak Billing menutup
  FIN-OQ-034), dan BE-FIN-047 ⛔ (pembalikan pemakaian uang muka deposit).
  SATU BLOCKER BARU yang ditemukan pass ini, dan ia ada di dalam blueprint sendiri: FIN-DES-057
  menetapkan pemicu PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT sebagai mutasi REVERSAL atas ALLOCATION,
  sedangkan solusi Billing yang disahkan FIN-DEC-077 dan sudah terimplementasi memakai mutasi
  RELEASE. Satu MovementType RELEASE karena itu kini membawa DUA lawan jurnal berbeda, dan cara
  Finance membedakannya belum digambar. BE-FIN-047 ⛔ menunggu amendment /design-business-module.
  SATU HAL YANG PERLU KONFIRMASI PEMILIK: eksekusi migration BE-FIN-022. Pengguna menyatakan sebuah
  migration berhasil pada 29 September 2026, tetapi dua berkas menunggu pada hari yang sama
  (BE-FIN-022 dan BE-FIN-041); status BE-FIN-022 sengaja TIDAK dinaikkan tanpa kepastian.

backend_commit_sha: 7811c048
backend_commit_sha_previous_planning: 96bf9746fedb63119a317a53758b0f0748ec7ad1
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: 49b59cfaa
frontend_commit_sha_previous: abed49b03
frontend_branch: yasmina

input_hashes:
  00-interview-decisions.md: 5fd9d74e22c04b0bb8fcba27a6a6f2b9432e4160eeba83db1d0a8eda697b45dc
  01-existing-capability-map.md: 42e95d720d42a288220f3919cd2e80fa2fe8361cb7362df1cbc92f4431e7e2b9
input_hashes_note_revision_5: >
  Keduanya BERGERAK lagi 25 September 2026, bukan drift: decision log menerima FIN-DEC-045..056
  dan FIN-OQ-022..026 (dibuka/ditutup pass perencanaan dan /grill-me susulan); capability map menerima
  bagian 12-14 (FIN-CAP-026..036). Nilai revisi 4 roadmap: cca250f2... dan 2674907a...
roadmap_revision_8_note: >
  Revisi 8 (28 September 2026) menambahkan empat task baru untuk AMENDMENT REVISI 6 dan 8 blueprint:
  BE-FIN-042 (penyelarasan 6 controller legacy Finance ke nama kanonikal, ekspansi payung Finance.AP/AR ke granular di seeder, dan skrip SQL idempotent migrasi hak peran SysRolePermissions — FIN-CQ-08, FIN-DES-061..063),
  BE-FIN-043 (penambahan kolom PPNAmount pada FinSupplierReturn dan kalkulasi deposit retur saat CONFIRMED membawa PPN — FIN-DES-055),
  BE-FIN-044 (penyelarasan konstanta EventTypeCode pada 4 service call points ke katalog resmi Accounting, hapus 5 alias lama — FIN-DES-058), dan
  FE-FIN-014 (penyelarasan menu sidebar navigasi Finance: submenu Pembelian, relabel Faktur Pembelian, dan butir flat Tagihan Gabungan Penjamin — FIN-DEC-060, FIN-DES-060).
  Seluruh kontrak turunan diperbarui ke FIN-PERM-1.3, FIN-VAL-1.4, FIN-INTEGRATION-1.4, FIN-MVP-1.5.
  Total task kini 44 backend (BE-FIN-001..044) dan 14 frontend (FE-FIN-001..014).
roadmap_revision_7_note: >
  Revisi 7 (26 September 2026) menurunkan AMENDMENT REVISI 5 blueprint (FIN-DES-045..050,
  approved dan kontraknya locked 26 September 2026 — dipilih owner saat memanggil pass ini).
  BE-FIN-036 dan FE-FIN-010 DIBUKA. Satu task baru: BE-FIN-041 (kolom DepositAppliedAmount pada
  FinPayment yang sudah berjalan). Enam task backend diperbarui ke bentuk revisi 5. Nol task
  REV-4 yang ⛔ selain yang bergantung pada FE-FIN-004. Satu risiko baru dicatat: layar
  pembayaran supplier yang ada memakai rute alias, bukan rute kanonik tempat endpoint deposit.
roadmap_revision_6_note: >
  Revisi 6 (25 September 2026, sesudah /grill-me closure pass FIN-DEC-057..060) menutup
  FIN-OQ-022/023/024/025. DUA task DIBUKA: BE-FIN-040 dan FE-FIN-013 (FIN-OQ-024 tertutup sisi
  Finance — kode POTONGAN-PIUTANG-NON-TUNAI diusulkan, FIN-DEC-058; ratifikasi Rizki dicatat
  terpisah FIN-OQ-026, hanya menahan worker pola FIN-DEC-056). SATU task TETAP ⛔: BE-FIN-036
  dan turunannya FE-FIN-010 — FIN-OQ-023 tertutup sisi keputusan bisnis (FIN-DEC-057: Deposit
  Retur jadi baris alokasi non-tunai di FinPayment), tetapi konsekuensi skema
  FinPaymentAllocation/FinSupplierReturnDepositUsage BELUM digambar /design-business-module.
  Endpoint GET /purchasing/reports/aging DICABUT (FIN-DEC-059). Label menu DITETAPKAN
  (FIN-DEC-060). Task lama TIDAK dinomori ulang.
input_hashes_note: >
  Kedua hash BERGERAK pada 25 September 2026 dan keduanya BUKAN drift: decision log menerima
  FIN-DEC-030..044, dan capability map menerima bagian 9.3 (impact scan d6cdfaf9) beserta
  pembaruan FIN-CAP-007/008/009 dan penambahan FIN-CAP-022..025. Nilai lama:
  74529813... dan 0576bf65...
roadmap_revision_4_note: >
  Revisi 4 (25 September 2026) menambahkan enam task untuk AMENDMENT REVISI 3 blueprint:
  BE-FIN-022..026 pada roadmap backend, dan FE-FIN-007 pada roadmap frontend. Tidak ada task
  lama yang dinomori ulang atau diturunkan statusnya. Grafik urutan dependency kedua roadmap
  anak dirapikan menjadi pohon teks sesuai rules/rule-output/grafik-dependency-roadmap.md.

approval_basis: >
  Yasmin (Product/Domain Owner Finance), 20 September 2026 — dua approval terpisah:
  (1) keputusan arsitektur FIN-DES-001..024; (2) cakupan MVP dan urutan gelombang pada
  04-prd-to-mvp.md bagian 7, 8, dan 20.1.
  Ditambah pada hari yang sama, sesudah revisi 1 roadmap ini:
  (3) FIN-DES-025..028; (4) penguncian tujuh kontrak turunan ke 1.0.
  25 September 2026: (5) FIN-DES-029..036 dan penguncian lima kontrak ke 1.1 ("Saya approve
  semua"); (6) FIN-DEC-056 — gerbang FIN-OQ-020 dipersempit; (7) FIN-DES-037..044 dan
  penguncian FIN-API-1.1, FIN-INTEGRATION-1.2, FIN-STATE-1.2, FIN-VAL-1.2, FIN-PERM-1.1,
  FIN-TEST-1.2, FIN-MVP-1.3 — dipilih owner atas pertanyaan eksplisit saat memanggil pass ini.
  Koreksi pass ini atas FIN-DES-039, C.6, dan C.7 menyelaraskan desain dengan keputusan dan
  source yang sudah berjalan; ia tidak mengubah keputusan apa pun yang disetujui.
```

Roadmap ini menurunkan cakupan yang sudah disetujui menjadi task, dan menyebut dengan tegas apa
yang tertahan beserta pemiliknya. **Pembaruan 23 September 2026**: berkas ini semula (revisi 3,
20 September 2026) tidak menyatakan satu pun pekerjaan selesai — sejak itu 14 dari 21 task
`BE-FIN-*` sudah ✅ selesai dan UI brief frontend sudah closed; lihat bagian 4 dan 5 untuk
rinciannya per task, dan laporan tracked masing-masing untuk buktinya.

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 44 task `BE-FIN-*` (`027`..`041` revisi 4-5, `042`..`044` ditambahkan revisi 6/8), urutan eksekusi, rincian per gelombang, prasyarat, DoD backend |
| `02-frontend-roadmap.md` | 14 task `FE-FIN-*` (`008`..`013` revisi 5, `014` ditambahkan revisi 6/8), yang MUST diputuskan UI brief, `DEV_DISCRETION`, DoD frontend |

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

**Penguncian revisi 4 — DISETUJUI 25 September 2026.** Owner mengunci kontrak AMENDMENT REVISI 4
bersama approval `FIN-DES-037`..`044`:

| Kontrak | Dari | Terkunci pada |
|---|---|---|
| `contracts/api-contract.md` | `FIN-API-1.0` | `FIN-API-1.1` ✅ |
| `contracts/integration-contract.md` | `FIN-INTEGRATION-1.1` | `FIN-INTEGRATION-1.2` ✅ |
| `contracts/state-transition-matrix.md` | `FIN-STATE-1.1` | `FIN-STATE-1.2` ✅ |
| `contracts/validation-matrix.md` | `FIN-VAL-1.1` | `FIN-VAL-1.2` ✅ |
| `contracts/permission-audit-matrix.md` | `FIN-PERM-1.0` | `FIN-PERM-1.1` ✅ |
| `testing/acceptance-test-matrix.md` | `FIN-TEST-1.1` | `FIN-TEST-1.2` ✅ — dua baris uji batas Rp 50.000.000 ditambahkan pass ini |
| `04-prd-to-mvp.md` | `FIN-MVP-1.1` | `FIN-MVP-1.3` ✅ |

**Penguncian revisi 5 — DISETUJUI 26 September 2026** bersama `FIN-DES-045`..`050`:
`FIN-API-1.2`, `FIN-INTEGRATION-1.3`, `FIN-STATE-1.3`, `FIN-VAL-1.3`, `FIN-PERM-1.2`,
`FIN-TEST-1.3`, `FIN-MVP-1.4` ✅. Baris yang dicabut di tabel versi sebelumnya diberi coretan dan
rujukan, tidak dihapus.

Satu permukaan revisi 4 tetap **tidak** dianggap final walau kontraknya terkunci: endpoint
`GET /purchasing/reports/aging` (`FIN-OQ-025`). Bila dicabut, kontraknya direvisi bernomor.

**Penguncian revisi 6 — DISETUJUI 28 September 2026** bersama ratifikasi Accounting, AMENDMENT REVISI 6 dan 8:
`FIN-PERM-1.3` (pemetaan granular FIN-CQ-08, payung `Finance.AP`/`Finance.AR` dipertahankan untuk frontend),
`FIN-VAL-1.4` (`PPNAmount >= 0` pada `FinSupplierReturn`, deposit retur membawa PPN saat `CONFIRMED`),
`FIN-INTEGRATION-1.4` (penyelarasan nama kode katalog outbox resmi Accounting dan pencabutan 5 alias lama),
`FIN-MVP-1.5` ✅.

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
| `POST-MVP` (`REV-4`) | `FIN-15` (menggantikan `FIN-07`) | `BE-FIN-027`..`037` · `FE-FIN-008`..`011` | Direncanakan 25 September 2026; `BE-FIN-036`/`FE-FIN-010` **dibuka** revisi 7 (`FIN-DES-045`..`047`), ditambah `BE-FIN-041`. `FIN-OQ-020` **tidak** menahan (`FIN-DEC-056`) |
| `POST-MVP` (`REV-4`) | `FIN-16` | `BE-FIN-038`, `039` · `FE-FIN-012` | Direncanakan 25 September 2026 — tidak bergantung pihak luar |
| `POST-MVP` (`REV-4`) | `FIN-17` | `BE-FIN-040` · `FE-FIN-013` | **Dibuka 25 September 2026** (`FIN-DEC-058`) — kode `POTONGAN-PIUTANG-NON-TUNAI` diusulkan; hanya worker pengirimannya menunggu Rizki (`FIN-OQ-026`) |
| `POST-MVP` (`REV-6/8`) | Penyelarasan Hak Akses (FIN-CQ-08), PPN Retur, Katalog Akuntansi | `BE-FIN-042`..`044` · `FE-FIN-014` | Direncanakan 28 September 2026 — `BE-FIN-042` & `BE-FIN-044` siap jalan paralel; `BE-FIN-043` menunggu `BE-FIN-035` 🟡; `FE-FIN-014` menunggu `BE-FIN-042` dan `FE-FIN-008`..`012` |
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
| `BE-FIN-022` | Empat `HandoffType` baru + satu migration check constraint | — (`REV-3`) | 🟡 **Sebagian 25 September 2026** — source lengkap, `dotnet build` `0 Error`; **eksekusi migration belum dijalankan**, sehingga aturan pemeriksaan nilai di database masih empat nilai — [laporan](../task/report/backend/BE-FIN-022.md) |
| `BE-FIN-023` ✅ | Rincian saldo subledger + aturan nilai dan bentuk pesan diluruskan sekali | — (`REV-3`) | ✅ **Selesai 29 September 2026** — build mandiri dikonfirmasi 0 error oleh pengguna, nol perubahan skema, rangkaian ditutup tuntas oleh `BE-FIN-024` ([laporan](../task/report/backend/BE-FIN-023.md)) |
| `BE-FIN-024` ✅ | Penerimaan pra-final terbit segera dengan jenis kejadian yang benar | — (`REV-3`) | ✅ **Selesai 29 September 2026** — build mandiri dikonfirmasi 0 error oleh pengguna, nol perubahan skema, pemilihan `EventTypeCode` dan kode pembalikan selesai ([laporan](../task/report/backend/BE-FIN-024.md)) |
| `BE-FIN-025` ✅ | Deposit, kelebihan bayar, dan selisih kas masuk kotak keluar | — (`REV-3`) | ✅ **Selesai 29 September 2026** — source 4 jalur sinkronisasi dan pengolahan intake selesai, 13 acceptance criteria terpetakan ke source, nol perubahan tabel `Bil*`, `dotnet build` dikonfirmasi 0 error oleh pengguna ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `BE-FIN-026` ✅ | Dua rumpun baris warisan kotak keluar dibereskan | — (`REV-3`) | ✅ Selesai 29 September 2026 — otorisasi pembacaan database diberikan, pembacaan database fisik `QuilvianNewDevYasmina` membuktikan 0 baris `HELD_FOR_FINALIZATION` dan 0 baris nama pendek `AR_*`/`AP_*`, nol baris diubah pada database fisik ([laporan](../task/report/backend/BE-FIN-026.md)) |
| `BE-FIN-045` 🟡 | Penanda shift kasir tertutup dan pembaliknya | — (`REV-6/8`) | 🟡 Source selesai 29 September 2026 — `dotnet build` tertunda; **nol migration**, nol resource/aksi hak akses baru. **Worker pengirimannya** tetap digerbang `FIN-OQ-035` (`FIN-DES-059`); [laporan](../task/report/backend/BE-FIN-045.md) |
| `BE-FIN-046` 🟡 | Pembalikan tender top-up deposit dikonsumsi | — (`REV-6/8`) | 🟡 Source selesai 29 September 2026 — `dotnet build` tertunda; **nol migration, nol endpoint baru, nol tulisan ke tabel `Bil*`**. Pemicunya **sudah ada** sejak Billing menutup `FIN-OQ-034` (`BKC-DEC-128`..`131`), sehingga pendeteksi `FIN-VAL-142` berfungsi sebagai jaring pengaman; [laporan](../task/report/backend/BE-FIN-046.md) |
| `BE-FIN-047` | Pembalikan pemakaian uang muka deposit | — (`REV-6/8`) | Siap dikerjakan di `R6-4` setelah `BE-FIN-046` — pemicunya mutasi `RELEASE` berpasangan `REVERSAL` ber-`SettlementId` sama (`FIN-DES-064`, `FIN-DES-065`, `FIN-DEC-080`, `FIN-DEC-081`) |
| `BE-FIN-027` | Submodul `Purchasing` terdaftar di registry | `POST-MVP` (`REV-4`) | ✅ Selesai 26 September 2026 — baris registry + `Invoke-QbeConformanceCheck.ps1` `PASS`; [laporan](../task/report/backend/BE-FIN-027.md) |
| `BE-FIN-028` | Satu resolver jenjang approval, batas `>= Rp 50.000.000` | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); [laporan](../task/report/backend/BE-FIN-028.md). **Mengubah perilaku pembayaran pada nilai tepat Rp 50.000.000** |
| `BE-FIN-029` | Model PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); [laporan](../task/report/backend/BE-FIN-029.md) |
| `BE-FIN-030` | Model Retur, Deposit Retur, kolom asal pada utang supplier | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); [laporan](../task/report/backend/BE-FIN-030.md) |
| `BE-FIN-031` | Dua migration Purchasing/AP | `POST-MVP` (`REV-4`) | 🟡 Berkas migration & `ApplicationDbContextModelSnapshot.cs` selesai ditulis 26 September 2026 (otorisasi pembuatan berkas disetujui) — `dotnet build` dan eksekusi migration sengaja belum dijalankan (otorisasi terpisah, belum diminta); [laporan](../task/report/backend/BE-FIN-031.md) |
| `BE-FIN-032` | PO dan Tanda Terima Barang | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); gerbang otorisasi jenjang baru (role Identity) butuh penugasan staf sebelum dapat diuji; [laporan](../task/report/backend/BE-FIN-032.md) |
| `BE-FIN-033` | Tukar Faktur | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); [laporan](../task/report/backend/BE-FIN-033.md) |
| `BE-FIN-034` | Purchasing Invoice → utang + kejadian PPN Masukan | `POST-MVP` (`REV-4`) | 🟡 Source selesai 26 September 2026 — `dotnet build` sengaja belum dijalankan; kejadian pengakuan utang ditulis bernilai pokok saja (`TotalAmount − PPNAmount`), kejadian PPN terpisah; [laporan](../task/report/backend/BE-FIN-034.md) |
| `BE-FIN-035` | Retur dan penerbitan Deposit Retur | `POST-MVP` (`REV-4`) | Belum dikerjakan |
| `BE-FIN-036` | Deposit Retur sebagai sumber dana pembayaran | `POST-MVP` (`REV-4`) | Dibuka revisi 7 — **mengubah `FinancePaymentService` yang sudah berjalan** (bersyarat ada deposit) |
| 🟡 `BE-FIN-037` | Empat laporan Purchasing/AP | `POST-MVP` (`REV-4`) | 🟡 Source selesai 29 September 2026, `dotnet build` tertunda — `/aging` dicabut (`FIN-DEC-059`); [laporan](task/report/backend/BE-FIN-037.md) |
| `BE-FIN-038` 🟡 | Skema AR Invoice Agregat + Potongan AR | `POST-MVP` (`REV-4`) | 🟡 Source & berkas migration selesai 29 September 2026 — `dotnet build` dan eksekusi migration tertunda (keduanya wewenang terpisah); [laporan](../task/report/backend/BE-FIN-038.md) |
| `BE-FIN-039` 🟡 | Batch Tagihan AR | `POST-MVP` (`REV-4`) | 🟡 Source selesai 29 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); menunggu eksekusi migration `BE-FIN-038` untuk verifikasi runtime; [laporan](../task/report/backend/BE-FIN-039.md) |
| `BE-FIN-040` 🟡 | Potongan AR bersama alokasinya, ikut terbalik | `POST-MVP` (`REV-4`) | 🟡 Source selesai 29 September 2026 — `dotnet build` sengaja belum dijalankan (instruksi pengguna); **mengubah `FinanceReceiptService` yang sudah berjalan** (aditif, bersyarat ada potongan); [laporan](../task/report/backend/BE-FIN-040.md) |
| `BE-FIN-041` 🟡 | Kolom `DepositAppliedAmount` pada `FinPayment` | `POST-MVP` (`REV-4`) | 🟡 Source & berkas migration selesai 28 September 2026 — `dotnet build` dan eksekusi migration tertunda (keduanya wewenang terpisah); **tabel yang sudah berjalan**; [laporan](../task/report/backend/BE-FIN-041.md) |
| `BE-FIN-042` 🟡 | Penyelarasan 6 controller legacy, seeder payung Finance.AP/AR, skrip SQL idempotent migrasi peran | `POST-MVP` (`REV-6/8`) | 🟡 Rename 6 controller + skrip SQL migrasi selesai 29 September 2026 (isi skrip dikoreksi — skema nyata `SysAccessPolicy`, bukan `SysRolePermissions` seperti tertulis kontrak); **seeder payung Finance.AP/AR BLOCKED** — bentrok nama dengan `FinanceApController`/`FinanceArController` V2 yang sudah berjalan, dikembalikan ke pass desain; [laporan](../task/report/backend/BE-FIN-042.md) |
| `BE-FIN-043` 🟡 | Kolom `PPNAmount` pada `FinSupplierReturn` dan deposit retur membawa PPN saat `CONFIRMED` | `POST-MVP` (`REV-6/8`) | 🟡 Source & berkas migration selesai 29 September 2026 — `dotnet build` dan eksekusi migration `AddPPNAmountToFinSupplierReturn` tertunda (keduanya wewenang terpisah); [laporan](../task/report/backend/BE-FIN-043.md) |
| `BE-FIN-044` 🟡 | Penyelarasan 4 service call points ke katalog resmi Accounting, hapus 5 alias lama | `POST-MVP` (`REV-6/8`) | 🟡 **Sebagian 29 September 2026** — source 4 berkas selesai, 5 alias dihapus, nol alias tersisa di pemanggil, dotnet build ditunda mandiri pengguna ([laporan](../task/report/backend/BE-FIN-044.md)) |

### 4.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-FIN-001` | Pengelolaan data induk | `BE-FIN-004` ✅ | 🟡 Source lengkap 23 September 2026 (dua fitur master data: Rekening Bank, Mata Uang+Kurs) — `npm run lint:errors`/`npm run build`/uji manual belum dijalankan — [laporan](../task/report/frontend/FE-FIN-001.md) |
| ✅ `FE-FIN-002` | Buku piutang dan umur piutang | `BE-FIN-009` ✅ | ✅ Selesai 23 September 2026 — daftar, rincian, 4 aging bucket cards, maker-checker koreksi/write-off, telusur tagihan asal selesai; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-002.md)) |
| ✅ `FE-FIN-003` | Setoran bank dan kas harian | `BE-FIN-015` ✅ | ✅ Selesai 23 September 2026 — daftar setoran bank, draf saldo kasir tersedia dari backend, detail siklus hidup setoran (posting/verifikasi/batal), riwayat kas harian, breakdown modal, penutupan kas beku; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-003.md)) |
| `FE-FIN-006` | Pemantauan fakta Billing dan kejadian Accounting | `BE-FIN-009` ✅, `BE-FIN-012` ✅ | UI brief closed 23 September 2026 (`FIN-DEC-024`..`029`); kedua dependency selesai — siap menunggu wewenang tulis frontend |
| `FE-FIN-004` | Penerimaan dan alokasi | `BE-FIN-018` ✅ | Owner Billing sudah menjawab 21 September 2026; `BE-FIN-016`..`018` selesai 23 September 2026. UI brief closed (`FIN-DEC-024`..`029`) — tidak lagi `BLOCKED`, siap menunggu wewenang tulis frontend |
| ✅ `FE-FIN-005` | Merapikan Petty Cash ke rute Finance | — | ✅ Selesai 23 September 2026 — slices/hooks/constants dipindahkan ke finance dengan jembatan re-export tanpa regresi, halaman voucher `/finance/petty-cash-voucher` dibangun, menu sidebar terdaftar; `npm run lint:errors` PASS, `npm run build` PASS ([laporan](../task/report/frontend/FE-FIN-005.md)) |
| `FE-FIN-007` ⛔ | Layar pemantauan menampilkan tujuh jenis kejadian baru | `BE-FIN-024` ⛔ | ⛔ Ditambahkan 25 September 2026. Menunggu `BE-FIN-024`, yang sendirinya menunggu `FIN-OQ-017`. Isi dan sumber data dikunci; tata letak tetap `DEV_DISCRETION` |
| `FE-FIN-008` | PO, Tanda Terima Barang, Tukar Faktur | `BE-FIN-032`, `033` | Belum dikerjakan — label menu "Pembelian" ditetapkan (`FIN-DEC-060`) |
| `FE-FIN-009` | Purchasing Invoice, Retur, daftar Deposit Retur | `BE-FIN-034`, `035` | Belum dikerjakan — dilabeli "Faktur Pembelian", tidak menduplikasi `/finance/payable/invoice` |
| `FE-FIN-010` | Pilih Deposit Retur di layar susun pembayaran | `BE-FIN-036` | Dibuka revisi 7 — cakupan pindah ke layar pembayaran; lihat risiko rute alias |
| `FE-FIN-011` | Empat laporan Purchasing/AP | `BE-FIN-037` | Belum dikerjakan — Aging AP tetap `/finance/ap-aging` yang sudah ada |
| `FE-FIN-012` | Batch Tagihan AR | `BE-FIN-039` | Belum dikerjakan — dilabeli "Tagihan Gabungan Penjamin" (`FIN-DEC-060`) |
| `FE-FIN-013` | Potongan AR di layar alokasi | `BE-FIN-040`, `FE-FIN-004` ✅ | Dibuka — `FIN-OQ-024` tertutup sisi Finance (`FIN-DEC-058`) |
| `FE-FIN-014` | Penyelarasan menu sidebar navigasi Finance (submenu Pembelian, relabel Faktur Pembelian, butir flat Tagihan Gabungan Penjamin) | `BE-FIN-042` [BE] 🟡, `FE-FIN-008`..`012` | Belum dikerjakan — rename 6 controller BE-FIN-042 selesai, tetapi seeder payung Finance.AP/AR yang ditunggu menu ini BLOCKED (lihat laporan BE-FIN-042 §7); juga menunggu penyelesaian layar Purchasing FE-FIN-008..012 |

`FE-FIN-006` ditambahkan saat roadmap dipecah: `03-frontend-architecture.md` bagian 3.5
menuntut dua layar pemantauan, dan keduanya sebelumnya tidak punya task frontend sama sekali.

### 4.3 Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`) |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`) |
| `EPIC FIN-14` — uang muka, deposit, kelebihan bayar, selisih kas | `OPEN DECISION`; tujuh kode kejadiannya menunggu ratifikasi Accounting (`FIN-OQ-017`). **Task-nya sudah direncanakan** (`BE-FIN-022`..`026`, `FE-FIN-007`) tetapi tidak diberi nomor gelombang — bukti teknis, kontrak, dan skenario ujinya sudah lengkap, yang belum ada hanya persetujuan nama kode |
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
| `FR-FIN-034` **diperbarui**, `FR-FIN-076` | `FIN-DEC-030` | `FIN-DES-033`, `034` | `FIN-INTEGRATION-1.5` §5.5, `FIN-STATE-1.3` §9, `FIN-VAL-1.4` `FIN-VAL-085` | `BE-FIN-023` ✅, `024` ✅ | `FE-FIN-007` | `FIN-TEST-1.5` §8a, §D.2 | ✅ Selesai 29 September 2026 — `BE-FIN-023` mencabut penahanan pra-final ([laporan](../task/report/backend/BE-FIN-023.md)); `BE-FIN-024` menerbitkan `PENERIMAAN-UANG-MUKA` dan pembalikannya mengikuti penerimaan asli ([laporan](../task/report/backend/BE-FIN-024.md)); build mandiri dikonfirmasi 0 error oleh pengguna |
| `FR-FIN-077` | `FIN-DEC-031`, `040`, `041` | `FIN-DES-029`, `030`, `035` | `FIN-INTEGRATION-1.5` §2a, §5.4 | `BE-FIN-022` 🟡, `025` ✅ | — | `FIN-TEST-1.5` §8a | 🟡 `BE-FIN-022` source selesai 25 September 2026; `BE-FIN-025` ✅ selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-078` | `FIN-DEC-042`, **`067`**, **`074`** | `FIN-DES-030`, `035`, **`056`** | `FIN-INTEGRATION-1.5` §2a, §5.4, §5.10; `FIN-VAL-1.4` **`FIN-VAL-141`** | `BE-FIN-025` ✅ | — | `FIN-TEST-1.5` §D.6 | ✅ `BE-FIN-025` selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna — kredit `SETTLEMENT` masuk jalur yang sudah ada, `REFERRED_OUTPATIENT_ADMIN` menjadi baris intake `ERROR` ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-079` | `FIN-DEC-034`, `043`, **`064`**, **`073`** | `FIN-DES-036`, **`053`** | `FIN-INTEGRATION-1.5` §5.4, §5.10; `FIN-VAL-1.4` `FIN-VAL-086`, **`140`** | `BE-FIN-025` ✅ | — | `FIN-TEST-1.5` §D.3 | ✅ `BE-FIN-025` selesai 29 September 2026, build mandiri dikonfirmasi 0 error oleh pengguna — satu kode selisih kas menjadi dua (`SELISIH-KAS-KURANG`/`LEBIH`) berkunci `BilCashierShift.Id` ([laporan](../task/report/backend/BE-FIN-025.md)) |
| `FR-FIN-080` | `FIN-DEC-035`, **`064`** | `FIN-DES-031`, `032`, **`054`**, **`058`** | `FIN-INTEGRATION-1.5` §5.2, §5.6, §5.10; `FIN-VAL-1.4` `FIN-VAL-081`..`084`, **`138`**, **`139`** | `BE-FIN-023` ✅ | — | `FIN-TEST-1.5` §D.2 | ✅ **Selesai 29 September 2026** — source selesai, build mandiri dikonfirmasi 0 error oleh pengguna, rincian saldo subledger, daftar tertutup kode penanda, dan pelurusan `Components` tuntas ([laporan](../task/report/backend/BE-FIN-023.md)) |
| `FR-FIN-108`..`110` | `FIN-DEC-070`, `072`, `073`, `075` | `FIN-DES-054`, `059` | `FIN-INTEGRATION-1.5` §5.10, §5.11; `FIN-VAL-1.4` `FIN-VAL-138` | `BE-FIN-045` | — | `FIN-TEST-1.5` §D.3, §E.1 | Direncanakan 29 September 2026 (`REV-6/8`) — kode boleh ditulis; **worker** digerbang `FIN-OQ-035` |
| Pembalikan tender top-up deposit | `FIN-DEC-040`, `044`, `077`; `BKC-DEC-128`..`131` | `FIN-DES-035`, `057` | `FIN-INTEGRATION-1.5` §5.10; `FIN-VAL-1.4` `FIN-VAL-142` | `BE-FIN-046` | — | `FIN-TEST-1.5` §D.6 | Direncanakan 29 September 2026 (`REV-6/8`) — pemicunya **sudah ada** sejak Billing menutup `FIN-OQ-034` |
| Pembalikan pemakaian uang muka deposit | `FIN-DEC-063`, `077`, `080`, `081` | `FIN-DES-064`, `065` | `FIN-INTEGRATION-1.6` §5.10 (pemicu `RELEASE` berpasangan); `FIN-VAL-1.5` `FIN-VAL-144`..`146` | `BE-FIN-047` | — | `FIN-TEST-1.6` §F.1..§F.3 | Direncanakan 29 September 2026 (`REV-6/8`, gelombang `R6-4`) — blocker dicabut via `FIN-DEC-080`/`081` |
| ~~`FR-FIN-074`~~ | ~~`FIN-DEC-004`~~ | — | — | ~~`BE-FIN-012`~~ | ~~`FE-FIN-006`~~ | — | **DICABUT 25 September 2026** (`FIN-DEC-030`). Kedua task tetap ✅ dan buktinya tetap berlaku untuk keadaan saat dikerjakan; penanganan baris warisan menjadi `BE-FIN-026` |
| Baris warisan kotak keluar — `HELD_FOR_FINALIZATION` **dan** lima nama pendek `AR_*`/`AP_*` | `FIN-DEC-030`; **`FIN-DES-058`** | `02-backend-architecture.md` B.6 **dan E.9** | `FIN-VAL-1.4` `FIN-VAL-078` | `BE-FIN-026` ✅ | `FE-FIN-007` | `FIN-TEST-1.5` §D.1 baris terakhir, §8a | ✅ **SELESAI 29 September 2026** — otorisasi pembacaan database diberikan pengguna, audit fisik membuktikan 0 baris kedua rumpun pada database pengembangan, rekomendasi penanganan diserahkan ke pemilik ([laporan](../task/report/backend/BE-FIN-026.md)) |
| Manfaat karyawan | `FIN-DEC-006`, `016` | — | — | — | — | — | `OPEN DECISION` |
| Pengiriman ke Accounting | ~~`FIN-DEC-007`~~ `FIN-DEC-036` (syarat organisasi), `FIN-OQ-016` (mekanisme) | `FIN-DES-024` | `FIN-INTEGRATION-1.1` §5.1 | — | — | — | `OPEN DECISION` — tiga syarat akun layanan kini **ditetapkan**; yang terbuka hanya bentuk kredensialnya |
| `FR-FIN-081` | `FIN-DEC-050`, `052` | `FIN-DES-039` (dikoreksi) | `FIN-API-1.1` B.1, `FIN-VAL-1.2` `100`..`103` | `BE-FIN-027` ✅, `028`, `029`, `031`, `032` | `FE-FIN-008` | `FIN-TEST-1.2` B.1 (termasuk dua baris batas Rp 50.000.000) | 🟡 `BE-FIN-027` selesai 26 September 2026 — [laporan](../task/report/backend/BE-FIN-027.md); sisanya direncanakan |
| `FR-FIN-082` | `FIN-DEC-051` | `FIN-DES-037` | `FIN-API-1.1` B.3, `FIN-STATE-1.2` B.3 | `BE-FIN-033` | `FE-FIN-008` | `FIN-TEST-1.2` B.2 | Direncanakan |
| `FR-FIN-083`, `084`, `086` | `FIN-DEC-045`, `051` | `FIN-DES-037`, `040` | `FIN-API-1.1` B.4, `FIN-VAL-1.2` `105`..`109` | `BE-FIN-030`, `034` | `FE-FIN-009` | `FIN-TEST-1.2` B.3 | Direncanakan |
| `FR-FIN-087` | `FIN-DEC-046`, `053`, `056` | `FIN-DES-043` | `FIN-INTEGRATION-1.2` §5.8, `FIN-VAL-1.2` `122` | `BE-FIN-034` (tulis baris); worker = `EPIC FIN-12` | `FE-FIN-009` | `FIN-TEST-1.2` B.3 baris `FIN-VAL-122` | Direncanakan — penulisan saja; pengiriman menunggu `FIN-OQ-020` **dan** `EPIC FIN-12` |
| `FR-FIN-085` (penerbitan) | `FIN-DEC-047` | `FIN-DES-038` | `FIN-API-1.1` B.5, `FIN-VAL-1.2` `110`, `111` | `BE-FIN-035` | `FE-FIN-009` | `FIN-TEST-1.2` B.4 (3 baris pertama) | Direncanakan |
| `FR-FIN-085` (pemakaian), `FR-FIN-096`, `097` | `FIN-DEC-047`, `057`, `061` | `FIN-DES-045`..`047` | `FIN-API-1.2` C.1-C.2, `FIN-STATE-1.3` C.1-C.3, `FIN-VAL-1.3` C.1-C.2 | `BE-FIN-030`, `031`, `036`, `041` | `FE-FIN-010` | `FIN-TEST-1.3` C.1, C.2 | Direncanakan |
| `FR-FIN-098` | `FIN-DEC-061` | `FIN-DES-047` | `FIN-INTEGRATION-1.3` §5.9 kode 28 | `BE-FIN-035` | — | `FIN-TEST-1.3` C.2 | Direncanakan — worker menunggu `FIN-OQ-026` |
| `FR-FIN-088` | — | `FIN-DES-044` | `FIN-API-1.1` B.6 | `BE-FIN-037` | `FE-FIN-011` | — | Direncanakan; `/aging` dicabut (`FIN-DEC-059`) |
| `FR-FIN-089`..`092` | `FIN-DEC-048`, `054` | `FIN-DES-041` | `FIN-API-1.1` B.7, `FIN-STATE-1.2` B.7, `FIN-VAL-1.2` `114`..`117` | `BE-FIN-038`, `039` | `FE-FIN-012` | `FIN-TEST-1.2` B.5 | Direncanakan |
| `FR-FIN-093`..`095`, `099` | `FIN-DEC-049`, `055`, `058`, `062` | `FIN-DES-042`, `048`..`050` | `FIN-API-1.2` C.2, `FIN-STATE-1.3` C.4, `FIN-VAL-1.3` C.1, C.3 | `BE-FIN-038` (tabel), `BE-FIN-040` | `FE-FIN-013` | `FIN-TEST-1.3` C.3, C.4 | Direncanakan — worker kode 26/27 menunggu `FIN-OQ-026` |
| Granularitas hak akses & migrasi peran | `FIN-DEC-061`, `077`..`079` | `FIN-DES-061`..`063` | `FIN-PERM-1.3`, `FIN-MVP-1.5` | `BE-FIN-042` | `FE-FIN-014` | `FIN-TEST-1.4` Bagian E.1, E.2 | Direncanakan 28 September 2026 (`REV-6/8`) |
| PPN Retur Pembelian | `FIN-DEC-047`, `063` | `FIN-DES-055` | `FIN-VAL-1.4` `FIN-VAL-135`, `FIN-MVP-1.5` | `BE-FIN-043` | — | `FIN-TEST-1.4` Bagian E.3 | Direncanakan 28 September 2026 (`REV-6/8`) |
| Penyelarasan katalog kode kejadian Accounting | `FIN-DEC-039`, `071` | `FIN-DES-058` | `FIN-INTEGRATION-1.4` §5.10, `FIN-MVP-1.5` | `BE-FIN-044` 🟡 | — | `FIN-TEST-1.4` Bagian E.4 | 🟡 `BE-FIN-044` source selesai 29 September 2026, build ditunda mandiri pengguna ([laporan](../task/report/backend/BE-FIN-044.md)) |
| Penyelarasan navigasi sidebar | `FIN-DEC-060`, `076` | `FIN-DES-060` | `03-frontend-architecture.md` Bagian 15, `FIN-MVP-1.5` | — | `FE-FIN-014` | Verifikasi visual sidebar | Direncanakan 28 September 2026 (`REV-6/8`) |


## 6. Coverage gap

| Gap | Akibat | Pemilik |
|---|---|---|
| `FR-FIN-051` (potongan tidak menyisakan utang) tanpa baris uji pada `FIN-TEST-0.1` | Aturan paling halus pada rumpun pembayaran tanpa penjaga | Pass desain revisi 2 |
| Rumpun manfaat karyawan tanpa requirement yang lengkap | Tidak dapat direncanakan; sudah dikeluarkan dari seluruh gelombang | Owner Billing + HR |
| ~~Katalog 17 jenis kejadian Accounting belum diratifikasi Rizki~~ | **TERTUTUP 24 September 2026** — ketujuh belas kode diratifikasi apa adanya (`ACC-DEC-083`, dicatat `FIN-DEC-039`) | — |
| ~~**Tujuh kode kejadian baru belum diratifikasi Rizki**~~ (`FIN-OQ-017`) | **TERTUTUP 28 September 2026** — lima kode diratifikasi apa adanya, `PENGAKUAN-KELEBIHAN-BAYAR` bersyarat dan syaratnya diterima (`FIN-DEC-067`), `SELISIH-KAS-SHIFT` diganti dua kode dan namanya diterima (`FIN-DEC-064`); balasan Finance terkirim `evidence/15`. Tanda ⛔ pada `BE-FIN-023`..`025` **dicabut** revisi roadmap 9. Yang menggantikannya: lima ratifikasi kode **baru** (`FIN-OQ-027`..`031`) yang hanya menahan **worker pengiriman**, bukan penulisan kode (pola `FIN-DEC-056`) | — |
| Lawan jurnal refund `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` (`FIN-OQ-018`) | Refund kategori itu tidak diterbitkan sebagai kejadian apa pun; sengaja di luar cakupan | Yasmin (Finance) — `LATER SLICE`, tidak menahan apa pun |
| Mekanisme kredensial akun layanan (`FIN-OQ-016`) | `EPIC FIN-12` dan gerbang `G3` tertahan; tiga syarat organisasinya sudah ditetapkan `FIN-DEC-036` | Platform + Rizki + Yasmin |
| Audit field-per-field rumpun AR/AP/Payable Finance | Blueprint belum dapat dipakai sebagai acuan penuh AR/AP — 59 berkas hasil `BE-FIN-001`..`021` belum diaudit sejak `09101d05` | Yasmin — jalankan `trace-existing-capabilities` |
| ~~Ambang nominal approval AP (`FIN-OQ-010`)~~ | **TERTUTUP 25 September 2026** (`FIN-DEC-052`). Batas tepat Rp 50.000.000 di kode berbeda dari keputusan — dikoreksi `BE-FIN-028`, 26 September 2026 | — |
| `dotnet build` belum dikonfirmasi untuk `BE-FIN-028`, `029`, dan `030` | Perubahan pada `FinancePaymentService.cs`/`FinPayment.cs`/`FinancePaymentsController.cs`/`FinSupplierPayable.cs`/`FinSupplierPayableConfiguration.cs` yang sudah berjalan, dan sebelas entity+configuration baru submodul Purchasing, belum terverifikasi compiler; risiko regresi rendah tapi belum nol. `BE-FIN-031`/`032`/`033`/`034` MUST menunggu konfirmasi sebelum dianggap aman membangun di atasnya | Yasmin — jalankan `dotnet build` sendiri, sesuai instruksi |
| `EPIC FIN-15`/`16`/`17` tanpa skenario UAT pada `04-prd-to-mvp.md` bagian 18 | Bukti penerimaan revisi 4 bersandar pada `FIN-TEST-1.2` B.1-B.7, bukan `UAT-*` bernomor — konsisten dengan `EPIC FIN-14` yang juga tanpa `UAT-*` | Yasmin — tambahkan `UAT-21` dst. bila revisi 4 dimasukkan ke Definition of Done rilis |
| ~~Deposit Retur tanpa mekanisme pengurang utang~~ (`FIN-OQ-023`) | **TERTUTUP PENUH** — keputusan `FIN-DEC-057` (25 September 2026), skema `FIN-DES-045`..`047` (26 September 2026) | — |
| **Layar pembayaran supplier memakai rute alias** — `/finance/payment-ap` memanggil `api/finance/payable/payment` (`FinanceApController`), sedangkan endpoint deposit revisi 5 ada di `api/v1/corporate/finance-management/payments` | `FE-FIN-010` tidak dapat memperluas layar itu tanpa lebih dulu memastikan jalur mana yang kanonik; dua jalur pembayaran dapat menghasilkan dua perilaku | Yasmin — `trace-existing-capabilities` impact scan frontend/backend **sebelum** `FE-FIN-010` |
| ~~Potongan AR tanpa kode kejadian akuntansi~~ (`FIN-OQ-024`) | **TERTUTUP sisi Finance 25 September 2026** (`FIN-DEC-058`) — kode `POTONGAN-PIUTANG-NON-TUNAI` diusulkan, `BE-FIN-040`/`FE-FIN-013` dibuka. Ratifikasi Rizki dicatat `FIN-OQ-026`, hanya menahan worker | Rizki (Accounting) — surat evidence belum dikirim |
| ~~Dua endpoint umur utang~~ (`FIN-OQ-025`) | **TERTUTUP 25 September 2026** (`FIN-DEC-059`) — `/purchasing/reports/aging` dicabut dari kontrak | — |
| ~~Label menu layar revisi 4~~ (`FIN-OQ-022`) | **TERTUTUP 25 September 2026** (`FIN-DEC-060`) | — |
| **Surat evidence `POTONGAN-PIUTANG-NON-TUNAI` belum dikirim** (`FIN-OQ-026`) | Worker pengiriman kode ini tetap dimatikan; tidak menahan `BE-FIN-040`/`FE-FIN-013` | Yasmin — menyusul sebagai instruksi terpisah, pola `evidence/06` |
| ~~Kode kejadian alias di luar katalog ratifikasi~~ | **TERTUTUP 28 September 2026** — ditangani oleh task `BE-FIN-044` yang menyelaraskan 4 service call points ke katalog resmi dan menghapus 5 alias lama (`FIN-DES-058`). | — |
| ~~Rute backend di luar kontrak~~ | **TERTUTUP 28 September 2026** — ditangani oleh task `BE-FIN-042` yang menyelaraskan 6 controller legacy ke `Finance*` dan rute kanonikal (`FIN-DES-062`). | — |
| **Frontend bergerak `abed49b03` → `49b59cfaa` tanpa verifikasi ulang** — tiga commit menambah `/finance/payable/*`, `/finance/receivable/*`, `/finance/ap-aging`, `/finance/payment-ar`, dan lainnya | Status `FE-FIN-001`..`007` (khususnya `FE-FIN-004` yang tercatat "belum dikerjakan") mungkin sudah tidak cocok dengan source | Yasmin — `trace-existing-capabilities` impact scan frontend |

| ~~**`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa pemicu yang terdefinisi**~~ | **TERTUTUP 29 September 2026** oleh `FIN-DES-064`, `FIN-DES-065`, `FIN-DEC-080`, dan `FIN-DEC-081`. Mutasi `RELEASE` berpasangan dengan `REVERSAL` ber-`SettlementId` sama ditetapkan sebagai pemicu resmi. Task `BE-FIN-047` memiliki acceptance criteria lengkap dan teruji pada `FIN-TEST-1.6` §F | — |
| **Eksekusi migration `BE-FIN-022` belum terkonfirmasi** — pengguna menyatakan sebuah migration berhasil 29 September 2026, tetapi dua berkas menunggu pada hari yang sama (`AlterFinBillingHandoffIntakeHandoffTypeCheck` dan `AddDepositAppliedAmountToFinPayment`) | `BE-FIN-025` tidak dapat dipastikan prasyaratnya terpenuhi; status `BE-FIN-022` sengaja dibiarkan 🟡 daripada dinaikkan tanpa bukti | Pemilik repository — konfirmasi satu baris sudah cukup |

Seluruh 20 skenario UAT pada `04-prd-to-mvp.md` bagian 18 sudah tertaut ke task. Yang tersisa
pada tabel di atas adalah gap yang pemiliknya berada **di luar** roadmap ini, ditambah dua gap yang
ditemukan pass perencanaan 29 September 2026 dan pemiliknya justru **di dalam** blueprint ini
sendiri (dua baris terakhir).

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
| 8 | *(revisi 5)* `BE-FIN-027` sebelum model Purchasing pertama; otorisasi terpisah untuk empat migration `REV-4` | ✅ **`BE-FIN-027` selesai** 26 September 2026 — [laporan](../task/report/backend/BE-FIN-027.md). Otorisasi migration **Belum** — rincian `01-backend-roadmap.md` bagian 6 butir 10, 13 |
| 9 | ~~*(revisi 5)* `FIN-OQ-022` sebelum butir menu layar revisi 4 didaftarkan~~ | ✅ **Terpenuhi** 25 September 2026 — `FIN-DEC-060` |
| 10 | *(revisi 6)* Amendment arsitektur menggambar skema `FIN-DEC-057` sebelum `BE-FIN-036`/`FE-FIN-010` | ✅ **Terpenuhi** 26 September 2026 — AMENDMENT REVISI 5 approved |
| 11 | *(revisi 7)* Otorisasi migration terpisah untuk `BE-FIN-041` — **tabel yang sudah berjalan** | **Belum** |
| 12 | *(revisi 8)* Skrip SQL idempotent migrasi hak peran `SysRolePermissions` disiapkan sebelum `BE-FIN-042` dijalankan di staging/production | Menunggu eksekusi `BE-FIN-042` |
| 13 | *(revisi 8)* Otorisasi migration terpisah untuk `AddPPNAmountToFinSupplierReturn` (`BE-FIN-043`) — **tabel yang sudah berjalan** | **Pembuatan berkas: DIBERIKAN** 29 September 2026. **Eksekusi: BELUM** |

## 8. Risiko

| Risiko | Dampak | Mitigasi |
|---|---|---|
| ~~`BilCollectionHandoff` tidak kunjung dikonfirmasi~~ | **RISIKO GUGUR 25 September 2026** — tabelnya sudah dibangun dan sudah dikonsumsi `FinanceBillingIntakeService` (`FIN-CAP-007`, `FIN-CAP-025`) | — |
| Accounting mengoreksi nama tujuh kode setelah `BE-FIN-023`..`026` dikerjakan | Konstanta `EventTypeCode`, aturan validasi, dan skenario uji harus disesuaikan ulang | Inilah sebabnya keempat task itu ⛔ dan **tidak** diberi nomor gelombang. `FIN-DES-030` sengaja tidak memasang check constraint pada `EventTypeCode` supaya penyesuaian nama **tidak** menuntut migration |
| `BE-FIN-023` dikerjakan tanpa `BE-FIN-024` menyusul | Penerimaan pra-final terbit sebagai `PENERIMAAN-KASIR` — dibukukan sebagai penerimaan final padahal uangnya kewajiban ke pasien | `01-backend-roadmap.md` bagian 4 `REV-3` mewajibkan keduanya satu rangkaian; `BE-FIN-023` **tidak** boleh ditandai selesai sendirian |
| Kontrak terkunci ternyata keliru saat implementasi | Perubahan kontrak `1.0` kini berbiaya lebih mahal daripada saat `draft` | Temuan seperti itu MUST dinaikkan sebagai revisi kontrak bernomor, bukan diselesaikan diam-diam di kode |
| Kas tersedia ternyata menuntut `FinReceipt` | Urutan `MVP-4` sebelum `MVP-2` gugur | Catatan 3.1 mewajibkan temuan itu dilaporkan balik, bukan diakali |
| UI brief tidak kunjung ada | Seluruh `FE-FIN-*` berhenti | Backend tidak tertahan; kontrak fungsional sudah berdiri |
| Endpoint Accounting dibangun lebih cepat dari perkiraan | `EPIC FIN-12` dapat dimulai lebih awal | Kotak keluar sudah terisi sejak `MVP-5`; tinggal mengirim antrean |
| *(revisi 5)* PPN Purchasing Invoice terkredit dua kali di Utang Supplier | Buku besar mencatat utang lebih besar dari faktur sebesar nilai PPN | Acceptance criteria `BE-FIN-034` dan DoD backend #20: kejadian pengakuan utang bernilai pokok, kejadian PPN bernilai PPN, jumlahnya `TotalAmount` |
| *(revisi 5)* Pembayaran tepat Rp 50.000.000 tiba-tiba menuntut Manajer | Pengguna mengira sistem rusak | `BE-FIN-028` diumumkan sebagai perubahan perilaku yang disetujui (`FIN-DEC-052`), bukan regresi — [laporan](../task/report/backend/BE-FIN-028.md) |
| *(revisi 5)* Rizki mengoreksi nama `PPN-MASUKAN-PEMBELIAN` setelah `BE-FIN-034` berjalan | Baris outbox tersimpan dengan nama lama | `EventTypeCode` tanpa check constraint (`FIN-DES-030`) — penyesuaian nama tanpa migration; baris `PENDING` yang belum terkirim dibetulkan datanya sebelum worker aktif |
| *(revisi 5)* Potongan AR dikerjakan lewat jalur pelunasan yang ada | Buku besar mencatat kas yang tidak pernah diterima | `BE-FIN-040` MUST menulis kode `POTONGAN-PIUTANG-NON-TUNAI` (`FIN-DEC-058`), bukan `AR_PAYMENT` |
| *(revisi 7)* Perubahan `FinancePaymentService` (`BE-FIN-036`) merusak pembayaran tanpa deposit | Nilai `AP_PAYMENT` atau `NetTransferAmount` pembayaran lama berubah | Setiap perubahan bersyarat `DepositAppliedAmount > 0`; baris regresi `FIN-TEST-1.3` C.2 wajib lulus |
| *(revisi 7)* Deposit dicadangkan tapi tidak pernah dilepas (pembayaran `DRAFT` ditinggalkan) | Saldo deposit tertahan tanpa batas | Pelepasan tersedia manual selama `DRAFT`; pembatalan pembayaran melepas otomatis. Batas waktu otomatis **tidak** dirancang — bila dibutuhkan, keputusan terpisah |
| *(revisi 8)* Inkonsistensi nama permission pada database staging/production | Endpoint controller menolak akses staf lama karena permission granular belum ada | `BE-FIN-042` menyediakan skrip SQL idempotent migrasi data peran `SysRolePermissions` sebelum rilis |
| *(revisi 8)* Deposit retur tidak memperhitungkan PPN | Saldo deposit retur kurang dari uang retur riil | `BE-FIN-043` memastikan deposit retur saat `CONFIRMED` mencakup `TotalAmount + PPNAmount` per `FIN-DES-055` |

## 9. Yang roadmap ini tidak lakukan

- Tidak menandai satu pun task selesai.
- Tidak menetapkan approval sendiri: ketiga approval pada revisi 2 ini diberikan owner secara langsung dan dicatat apa adanya.
- Tidak menyembunyikan satu pun dependency eksternal: keempat pemiliknya disebut namanya.
- Tidak menjalankan builder mana pun.
- Tidak mengubah cakupan MVP yang sudah dikunci owner.
