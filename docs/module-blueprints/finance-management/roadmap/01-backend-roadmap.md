# Finance Management — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-BE-001
parent_roadmap: FIN-ROADMAP-001 revisi 3
roadmap_revision: 2
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 3
blueprint_status: approved
backend_commit_sha: d6cdfaf9fda8889d6fe73db1e97cc28c4f022852
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
contracts:
  FIN-API-1.0: locked 2026-09-20 (tidak bergerak pada revisi 3)
  FIN-PERM-1.0: locked 2026-09-20 (tidak bergerak pada revisi 3)
  FIN-STATE-1.1: locked 2026-09-25
  FIN-VAL-1.1: locked 2026-09-25
  FIN-INTEGRATION-1.1: locked 2026-09-25
  FIN-TEST-1.1: locked 2026-09-25
  FIN-MVP-1.1: locked 2026-09-25
roadmap_revision_2_note: >
  Revisi 2 (25 September 2026) MENAMBAHKAN lima task BE-FIN-022..026 untuk AMENDMENT REVISI 3
  blueprint (FIN-DES-029..036, approved). TIDAK ada task lama yang dinomori ulang, diubah
  outcome-nya, atau dihapus. Grafik urutan dependency dirapikan menjadi pohon teks sesuai
  rules/rule-output/grafik-dependency-roadmap.md pada kesempatan pertama berkas ini disentuh.
  SATU-SATUNYA blocker task baru: FIN-OQ-017 (ratifikasi owner Accounting atas tujuh kode
  kejadian). Empat dari lima task baru berstatus ⛔ karenanya; BE-FIN-022 bebas dari blocker itu.
```

Payung roadmap ada di `00-delivery-roadmap.md`: identitas lengkap, penguncian kontrak,
gelombang, traceability, coverage gap, dan risiko. Berkas ini **hanya** berisi task backend.
Pasangan frontend-nya ada di `02-frontend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai.**

## 2. Urutan eksekusi

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

Task bergaris bawah `⛔` MUST NOT dimulai.

## Grafik Urutan Dependency

Dua puluh enam task melewati batas satu layar, sehingga grafiknya dipecah: satu grafik ringkasan
antar-gelombang, lalu satu grafik per rumpun. Arah garis selalu **prasyarat di kiri, yang
menunggu di kanan**.

### Ringkasan antar-gelombang

```text
MVP-0 ✅ ─> MVP-1 ✅ ─┬─> MVP-5 ✅
                      │
                      ├─> MVP-4 ✅
                      │
                      ├─> MVP-2 ✅ ─> MVP-3 ✅
                      │
                      └─> POST-MVP 🟡

{FIN-OQ-017 ⛔} ─> REV-3 ⛔
```

`REV-3` adalah kelompok task AMENDMENT REVISI 3 (`BE-FIN-022`..`026`). Ia **tidak diberi nomor
gelombang** karena `EPIC FIN-14` berstatus `OPEN DECISION` pada `04-prd-to-mvp.md`.

### MVP-0 dan MVP-1 — fondasi sampai buku piutang

```text
BE-FIN-001 ✅ ─> BE-FIN-002 ✅ ─> BE-FIN-003 ✅ ─> BE-FIN-004 ✅ ─> BE-FIN-005 ✅
  ─> BE-FIN-006 ✅ ─> BE-FIN-007 ✅ ─> BE-FIN-008 ✅ ─> BE-FIN-009 ✅
```

Rantai lurus tanpa percabangan; baris kedua adalah sambungan baris pertama.

### MVP-5, MVP-4, MVP-2, MVP-3, dan POST-MVP

```text
BE-FIN-007 ✅ ─> BE-FIN-010 ✅ ─> BE-FIN-011 ✅ ─> BE-FIN-012 ✅

BE-FIN-009 ✅ ─┬─> BE-FIN-013 ✅ ─> BE-FIN-014 ✅ ─> BE-FIN-015 ✅
               │
               ├─> BE-FIN-016 ✅ ─> BE-FIN-017 ✅ ─> BE-FIN-018 ✅
               │
               └─> BE-FIN-019 ✅

{FIN-OQ-010 ⛔} ─> BE-FIN-020 🟡 ─> BE-FIN-021 🟡 <─ {BE-MDF-014 ⛔}
```

`BE-FIN-007` dan `BE-FIN-009` masing-masing muncul sekali pada grafik MVP-0/MVP-1 di atas; di
sini keduanya digambar sebagai **pangkal cabang** untuk memperlihatkan percabangannya, bukan
sebagai node kedua. `{BE-MDF-014}` adalah task modul Medical Fee — cermin baca-saja, bukan task
roadmap ini; panahnya digambar ke kiri karena ia prasyarat yang datang dari luar.

### REV-3 — AMENDMENT REVISI 3 (task baru)

```text
BE-FIN-022 ────────────────────────────────────────┐
                                                   │
{FIN-OQ-017 ⛔} ─> BE-FIN-023 ⛔ ─> BE-FIN-024 ⛔ ─┴─> BE-FIN-025 ⛔ ─> BE-FIN-026 ⛔
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | Otorisasi migration (`AGENTS.md` Keselamatan Database) | `BE-FIN-022` — **bebas dari `FIN-OQ-017`**, tetapi tidak diberi nomor gelombang karena `EPIC FIN-14` masih `OPEN DECISION` |
| — | ⛔ menunggu `FIN-OQ-017` | `BE-FIN-023` |
| — | ⛔ menunggu `FIN-OQ-017` dan `BE-FIN-023` | `BE-FIN-024` |
| — | ⛔ menunggu `BE-FIN-022` dan `BE-FIN-024` | `BE-FIN-025` |
| — | ⛔ menunggu `BE-FIN-025` dan otorisasi pembacaan database | `BE-FIN-026` |

**Kenapa tidak ada satu pun nomor gelombang di tabel ini.** `04-prd-to-mvp.md` bagian 20.1
menyatakan `EPIC FIN-14` `MUST NOT` masuk gelombang pengiriman mana pun sampai `FIN-OQ-017`
tertutup. Aturan itu berlaku juga untuk `BE-FIN-022` yang sebenarnya bebas dari blocker teknis:
ia boleh **dikerjakan** kapan saja setelah otorisasi migration turun, tetapi tidak dijadwalkan
sebagai bagian gelombang.

### Gelombang eksekusi — task lama

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 (`MVP-0`) | — | `BE-FIN-001`..`004` ✅ |
| 2 (`MVP-1`) | `MVP-0` | `BE-FIN-005`..`009` ✅ |
| 3 (`MVP-5`) | `BE-FIN-007` | `BE-FIN-010`..`012` ✅ — boleh paralel sejak `MVP-1` |
| 3 (`MVP-4`) | `BE-FIN-009` | `BE-FIN-013`..`015` ✅ — boleh mendahului `MVP-2` |
| 3 (`MVP-2`) | `BE-FIN-009` | `BE-FIN-016`..`017` ✅ |
| 4 (`MVP-3`) | `BE-FIN-017` | `BE-FIN-018` ✅ |
| 3 (`POST-MVP`) | `BE-FIN-009` | `BE-FIN-019` ✅ |
| — | ⛔ `FIN-OQ-010` untuk ambang angkanya saja | `BE-FIN-020` 🟡 |
| — | ⛔ `BE-MDF-014` milik Medical Fee | `BE-FIN-021` 🟡 |

### 2.1 Catatan urutan `MVP-4`

`04-prd-to-mvp.md` bagian 20.1 mengizinkan `MVP-4` dikerjakan **lebih dahulu** bila
`BilCollectionHandoff` belum tersedia. Batas yang MUST dihormati:

`FinanceCashManagementService` menghitung kas tersedia dari **kas kasir** (`FIN-CAP-006`,
`BilCashierShift`) — bukan dari `FinReceipt`. Selama `MVP-2` belum ada, `BE-FIN-014` MUST
membaca kas kasir langsung dan MUST NOT membuat jalur sementara yang kelak dibongkar.

Bila implementer menemukan bahwa angka kas tersedia ternyata menuntut `FinReceipt`, itu temuan
yang MUST dilaporkan balik ke pass desain — bukan diselesaikan dengan improvisasi di kode.

## 3. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `BE-FIN-001` | Enam submodul Finance terdaftar di registry kepemilikan modul | `FIN-DES-002` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | `BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration` | — | Enam baris tercatat; prefix `Fin` dan `Mst` tertera | Berkas registry ter-diff | Backend Owner — **prasyarat `QBE-MOD-003`, MUST sebelum file model pertama** | Registry ter-commit terpisah dari kode |
| ✅ `BE-FIN-002` | Entity dan EF configuration data induk Finance | `FIN-DES-001`, `003`, `004`, `005` | `FIN-VAL-1.0` §data induk | Pola `MstPettyCashCategoryConfiguration` | `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate` + 4 configuration | `BE-FIN-001` | Prefix `Mst`; partial unique index `WHERE "IsDelete" = false`; `HasPrecision(18,2)` | Review konfigurasi + uji unit constraint | Backend Owner | `IdentityModel` diwarisi; nol hard delete |
| ✅ `BE-FIN-003` | Migration `AddFinanceMasterData` | `FIN-DES-001` | — | — | 4 tabel baru, aditif | `BE-FIN-002` | `Up()` membuat 4 tabel; `Down()` menghapus bersih | Migration dijalankan di lingkungan pengembangan | **Otorisasi terpisah wajib** (`AGENTS.md` Keselamatan Database) | Nol tabel existing tersentuh |
| ✅ `BE-FIN-004` | API data induk Finance | `FIN-DES-001`, `FR-FIN-001`..`004` | `FIN-API-1.0`, `FIN-PERM-1.0` | `ApiResponse<T>`, `PagedResult<T>`, `[AccessPermission]` | `FinanceMasterDataService` + 3 controller | `BE-FIN-003` | Nomor rekening ganda ditolak; data induk terpakai dinonaktifkan bukan dihapus | `UAT-01`, `UAT-02` | Backend Owner | Route `api/v1/corporate/finance-management/...` hyphenated |
| ✅ `BE-FIN-005` | Pintu masuk fakta Billing yang idempoten | `FIN-DEC-005` (sisi konsumsi), `FR-FIN-010`..`013` | `FIN-INTEGRATION-1.0` §intake | `BilArHandoff` (`FIN-CAP-001`, `008`) | `FinBillingHandoffIntake` + configuration | `BE-FIN-004` | Fakta sama dua kali → satu piutang; kegagalan tersimpan dan dapat diulang; yang berhasil tidak dapat diulang | `UAT-04` | Backend Owner | `Serializable`; `Idempotency-Key` |
| ✅ `BE-FIN-006` | Entity buku piutang | `FIN-DES-010`..`013`, `FR-FIN-020`..`024` | `FIN-VAL-1.0` §piutang | — | `FinReceivable`, `FinReceivableItem`, `FinReceivableDocument`, `FinReceivableAdjustment`, `FinReceivableWriteOff` | `BE-FIN-005` | Invariant nilai piutang seimbang terpasang sebagai check constraint | Uji unit invariant | Backend Owner | Prefix `Fin`; `Guid RowVersion` pada aggregate root |
| ✅ `BE-FIN-007` | Migration `AddFinanceBillingIntake` dan `AddFinanceReceivableAndCollection` | `FIN-DES-001` | — | — | 1 + 7 tabel, aditif | `BE-FIN-006` | Urutan migration 2 lalu 3 sesuai `02-backend-architecture.md` bagian 7 | Migration dijalankan | **Otorisasi terpisah wajib** | Nol tabel existing tersentuh |
| ✅ `BE-FIN-008` | Layanan piutang: umur, koreksi, penghapusan | `FIN-DES-011`..`013`, `FR-FIN-021`..`023` | `FIN-STATE-1.0`, `FIN-VAL-1.0` | — | `FinanceReceivableService` — satu-satunya penulis `OutstandingAmount` | `BE-FIN-007` | Empat kelompok umur; berkas klaim tidak menahan pengakuan piutang | `UAT-03` | Backend Owner | `Serializable`; satu penulis saja |
| ✅ `BE-FIN-009` | API intake dan piutang | `FR-FIN-010`..`024` | `FIN-API-1.0`, `FIN-PERM-1.0` | `[AccessPermission]` | `FinanceBillingIntakeController`, `FinanceReceivablesController` | `BE-FIN-008` | Daftar, rincian, umur piutang, penelusuran ke tagihan asal | `UAT-03`, `UAT-04` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-010` ✅ | Kotak keluar kejadian Accounting | `FIN-DEC-004`, `FIN-DES-017`, `FR-FIN-070`..`075` | `FIN-INTEGRATION-1.0` §outbox, `ACC-XMOD-0.2` | — | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` + migration `AddFinanceAccountingOutbox` | `BE-FIN-007` | Fakta dan kejadian tersimpan atau batal bersama; satu fakta satu kejadian | `UAT-07`, `UAT-17`, `UAT-18` | Backend Owner; **migration butuh otorisasi terpisah** | Data pasien tidak ikut ke muatan kejadian |
| `BE-FIN-011` ✅ | Penulisan outbox ikut transaksi pemanggil | `FIN-DES-017` | `FIN-INTEGRATION-1.0` | — | `FinanceAccountingOutboxService` | `BE-FIN-010` | Service **tidak** membuka transaksi sendiri; kejadian tertahan tidak terkirim | Uji integrasi rollback, `UAT-19` | Backend Owner | Koreksi memakai versi baru, bukan menimpa |
| `BE-FIN-012` ✅ | Pantauan kejadian (baca saja) | `FR-FIN-074` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceAccountingEventsController` | `BE-FIN-011` | Kejadian tertahan dan antreannya terlihat | Uji integrasi | Backend Owner | **Tanpa** endpoint pengirim — itu `EPIC FIN-12` |
| `BE-FIN-013` ✅ | Entity kas dan setoran | `FIN-DES-018`..`020`, `FR-FIN-060`..`065` | `FIN-VAL-1.0` §kas | `BilCashierShift` (`FIN-CAP-006`) | `FinBankDeposit`, `FinDailyCashSnapshot` + migration `AddFinanceCashManagement` | `BE-FIN-009` | Kas kecil tidak memengaruhi kas kasir | Uji unit | Backend Owner; **migration butuh otorisasi terpisah** | Aditif |
| ✅ `BE-FIN-014` | Perhitungan kas tersedia dan penutupan harian | `FR-FIN-060`..`065` | `FIN-STATE-1.0` | `BilCashierShift` | `FinanceCashManagementService` | `BE-FIN-013` | Setoran melebihi kas ditolak; setoran sebagian diterima; saldo dihitung saat posting; angka tertutup dibekukan | `UAT-13`..`UAT-16` | Backend Owner — lihat bagian 2.1 | `Serializable` |
| ✅ `BE-FIN-015` | API setoran dan kas harian | `FR-FIN-060`..`065` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceBankDepositsController`, `FinanceDailyCashController` | `BE-FIN-014` | Penutupan hari menolak setoran belum terposting | `UAT-16` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-016` ✅ | Penerimaan dari tender kasir | `FIN-DEC-005`, `FR-FIN-030`..`035` | `FIN-INTEGRATION-1.0` — **permukaan `BilCollectionHandoff` dikecualikan dari penguncian** | `BilTender`, `BilSettlement` (`FIN-CAP-004`, `007`) | `FinReceipt`, `FinReceiptAllocation` | `BE-FIN-009` **dan** konfirmasi owner Billing — **keduanya terpenuhi** | Satu tender berhasil → satu penerimaan; nominal disalin apa adanya | `UAT-05`, `UAT-06`, `UAT-20` | Owner Billing sudah menjawab (`BKC-DEC-106`/`108`/`109`, 21 September 2026) — lihat `evidence/02-permintaan-kontrak-untuk-owner-billing.md` | — |
| `BE-FIN-017` ✅ | Pembagian bayar-vs-piutang | `FR-FIN-031`, `FR-FIN-035` | Idem | — | Logika pembagian di `FinanceReceiptService` | `BE-FIN-016` | Pasien lunas tidak melahirkan piutang; dobel-hitung dapat dibuktikan tidak terjadi | `UAT-05`, `UAT-20` | — | — |
| `BE-FIN-018` ✅ | Alokasi, koreksi, penghapusan piutang | `FIN-DES-014`, `FR-FIN-040`..`046` | `FIN-STATE-1.0` §piutang — **terkunci**; yang menahan hanya dependency-nya | — | Alokasi manual, maker-checker, pembalikan | `BE-FIN-017` | Pengaju tidak dapat menyetujui permohonannya sendiri; nilai piutang tidak berubah selama belum diputus; pembalikan tidak menghapus riwayat | `UAT-08`..`UAT-12` | — | Maker-checker tiga lapis untuk koreksi/penghapusan; alokasi sendiri aksi langsung (`state-transition-matrix.md` §2) — lihat [laporan](../task/report/backend/BE-FIN-018.md) |
| `BE-FIN-019` ✅ | Utang supplier | `FIN-DES-015` (bagian supplier) | `FIN-API-1.0` §payable supplier | `MstSupplier` (`FIN-CAP-014`) | `FinSupplierPayable`, `FinSupplierPayableItem`, `FinPayableAdjustment` | `BE-FIN-009` | Input manual utang dan koreksinya | Uji integrasi | — | `POST-MVP` |
| `BE-FIN-020` ✅ | Pembayaran keluar dan potongan | `FIN-DES-015`, `026`, `027`, `028` — **approved** | `FIN-API-1.0`, `FIN-VAL-1.0` §payable — **terkunci** | — | `FinPayment`, `FinPaymentAllocation`, `FinPaymentDeduction`, `NetTransferAmount` | `FIN-OQ-010` — model dan kontraknya sudah bebas | Uang keluar berbeda dari utang lunas; potongan tidak menyisakan utang | `FR-FIN-050`, `FR-FIN-051` | Finance Supervisor + Yasmin — **hanya aturan validasi angkanya** (angka rupiah persis ambang approval berjenjang) yang tertahan, bukan modelnya maupun controllernya. `FinancePaymentsController` dibangun 23 September 2026; `ResolveApprovalTier` tetap memakai nilai provisional yang sudah didokumentasikan | `POST-MVP` |
| `BE-FIN-021` 🟡 | **SEBAGIAN** — utang jasa tenaga medis | `FIN-DES-025` — **approved**, `FIN-CAP-021` | `FIN-API-1.0` §payable | — | `FinMedicalServicePayable`, `FinMedicalServicePayableItem` | `BE-FIN-020` **dan** Medical Fee `BE-MDF-014` | Model domain, EF configuration, relasi FK polimorfik, dan migration siap; intake otomatis menunggu handoff Medical Fee | `FR-FIN-075` | **Owner Medical Fee** — `MdfFinanceHandoff` belum ada (intake ditangguhkan) | `POST-MVP` |
| `BE-FIN-022` | Empat jenis fakta Billing baru menjadi nilai `HandoffType` yang sah | `FIN-DES-029`, `FIN-DEC-040`..`044` | `FIN-STATE-1.1` §1, `FIN-VAL-1.1` | `FinBillingHandoffIntake` beserta unique index `(HandoffType, SourceHandoffKey)` yang sudah ada (`FIN-DES-008`, `009`) | Konstanta `FinBillingHandoffTypes` bertambah `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW`; **satu** migration `AlterFinBillingHandoffIntakeHandoffTypeCheck` mengubah `CK_FinBillingHandoffIntake_HandoffType` dari 4 menjadi 8 nilai | — (tabel intake sudah ada sejak `BE-FIN-005` ✅) | Baris ber-`HandoffType` baru dapat disimpan; baris ber-nilai lama tetap sah; unique index tetap menolak fakta yang sama dua kali | `dotnet build`; verifikasi proses bisnis atas check constraint; `FIN-TEST-1.1` §8a baris `FIN-DES-029` (sinkronisasi ganda ditolak unique index) | Backend Owner — **otorisasi migration WAJIB diminta terpisah** (`AGENTS.md` Keselamatan Database). **Bebas dari `FIN-OQ-017`**: nama `HandoffType` adalah keputusan internal Finance, bukan nama kode kejadian yang perlu ratifikasi Accounting | Nol tabel dan nol kolom baru; `Down()` mengembalikan constraint ke 4 nilai dan **hanya aman** bila belum ada baris memakai nilai baru |
| `BE-FIN-023` ⛔ | Kotak keluar menerima nilai nol/negatif untuk dua jenis kejadian, dan membawa rincian saldo subledger | `FIN-DES-031`, `032`, `033`; `FIN-DEC-035`, `043` | `FIN-INTEGRATION-1.1` §5.2, §5.6; `FIN-VAL-1.1` `FIN-VAL-079`..`084` | `FinanceAccountingOutboxService` yang sudah ada (`BE-FIN-011` ✅); **nol perubahan skema** (`Amount` sudah `HasPrecision(18,2)` tanpa check constraint nilai) | `ValidateRequest` dipersempit; `AccountingOutboxEventRequest` bertambah `SubledgerBalance` dan **kehilangan** `RequiresFinalization`; `BuildPayloadJson` menyertakan objek saldo bila terisi | ⛔ `FIN-OQ-017` | Selisih kas `-30.000` tersimpan; saldo subledger `0` tersimpan; `PENERIMAAN-UANG-MUKA` bernilai nol ditolak `400`; `SubledgerBalance` pada kejadian non-saldo ditolak `400`; periode `2026-11-30` ditolak `400` | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-VAL-079`, `080`, `081`, `082`, `084`, `FIN-DES-032` | Backend Owner. **Menyentuh service yang sudah berjalan** — `RequiresFinalization` dipakai `FinanceReceiptService` hari ini, sehingga `BE-FIN-024` MUST menyusul di rangkaian yang sama agar penerimaan pra-final tidak terbit sebagai `PENERIMAAN-KASIR` | Payload tetap dibangun di dalam service, bukan diterima mentah dari pemanggil (`FR-FIN-073` tetap terkunci di level tipe) |
| `BE-FIN-024` ⛔ | Penerimaan sebelum tagihan final terbit segera dengan jenis kejadian yang benar, dan pembalikannya mengikuti penerimaan aslinya | `FIN-DEC-030`, `FIN-DES-033`, `034`; `FR-FIN-034` (diperbarui), `FR-FIN-076` | `FIN-INTEGRATION-1.1` §5.5; `FIN-STATE-1.1` §9; `FIN-VAL-1.1` `FIN-VAL-085` | `FinReceipt.SourceInvoiceStatus` yang **sudah ada** — nol kolom baru (`FIN-DES-034`) | `FinanceReceiptService`: pemilihan `EventTypeCode` dari `SourceInvoiceStatus`, dan pemilihan kode pembalikan dari baris penerimaan asli yang ditunjuk `ReversalOfReceiptId` | ⛔ `BE-FIN-023` — yang sendirinya menunggu `FIN-OQ-017`, sehingga blocker itu ikut berlaku secara berantai | Tagihan `OPEN` → `PENERIMAAN-UANG-MUKA` berstatus `PENDING`, **bukan** tertahan; tagihan `FINAL` → `PENERIMAAN-KASIR`; pembalikan penerimaan uang muka **tetap** `PEMBALIKAN-PENERIMAAN-UANG-MUKA` walaupun tagihannya sudah `FINAL` saat pembalikan | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-DEC-030` (tiga skenario), `FIN-DES-034` (dua skenario), `FIN-VAL-085` | Backend Owner. **INI SATU-SATUNYA TASK YANG MENGUBAH PERILAKU YANG SUDAH BERJALAN** — `FinanceReceiptService` baris ~126 dan ~194 hari ini memakai `RequiresFinalization`; review diff MUST membuktikan tidak ada jalur lain yang ikut berubah | Nol baris baru berstatus `HELD_FOR_FINALIZATION` dihasilkan kode setelah task ini |
| `BE-FIN-025` ⛔ | Deposit, kelebihan bayar, dan selisih kas shift masuk ke kotak keluar dengan jenis kejadian masing-masing | `FIN-DEC-031`, `034`, `040`..`043`; `FIN-DES-035`, `036`; `FR-FIN-077`..`079` | `FIN-INTEGRATION-1.1` §2a, §5.4; `FIN-VAL-1.1` `FIN-VAL-086` | `BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilCashVarianceReview` — **read-only**, sudah terdaftar di `ApplicationDbContext` (`FIN-CAP-022`..`024`) | `FinanceBillingIntakeService`: empat jalur sinkronisasi baru dengan anti-join ke `FinBillingHandoffIntake` beserta penyempitan waktu; `SELISIH-KAS-SHIFT` memakai `BilCashVarianceReview.Id` dan tanggal shift | ⛔ `BE-FIN-022` **dan** `BE-FIN-024` | `ALLOCATION` → `PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa `FinReceipt` baru; `RELEASE` → `PENGEMBALIAN-UANG-MUKA`; refund `ALLOCATION_EXCESS` → `PENGEMBALIAN-UANG-MUKA`; refund `SETTLEMENT` → **nol kejadian**; shift belum `REVIEWED` → **nol kejadian**; `Variance` nol → **nol kejadian**; nol perubahan pada tabel `Bil*` mana pun | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-DEC-040`, `041` (tiga skenario), `042`, `043` (dua skenario), `FIN-VAL-080`, `086`, aturan bisnis #9 | Backend Owner. Risiko utama: menulis ke tabel `Bil*` tanpa sengaja — dibuktikan tidak terjadi dengan membandingkan `RowVersion`/`UpdateDateTime` sebelum dan sesudah | `TargetEntityId` boleh kosong untuk jalur yang tidak melahirkan entity Finance; keempat jenis berhenti di `CONSUMED`, **tidak** mengirim ACK ke Billing |
| `BE-FIN-026` ⛔ | Baris warisan `HELD_FOR_FINALIZATION` dibereskan sebelum pengiriman diaktifkan | `FIN-DEC-030`; `02-backend-architecture.md` bagian B.6; `FIN-VAL-1.1` `FIN-VAL-078` | `FIN-STATE-1.1` §9 (transisi migrasi satu kali) | — | Penghitungan baris berstatus `HELD_FOR_FINALIZATION`; bila ada, pembetulan `EventTypeCode` menjadi `PENERIMAAN-UANG-MUKA` untuk penerimaan ber-`SourceInvoiceStatus` `OPEN`, lalu status dipindah ke `PENDING`. Worker MUST melewati **dan melaporkan** baris yang belum dibetulkan | ⛔ `BE-FIN-025` | Jumlah baris warisan dilaporkan apa adanya; bila nol, task ditutup tanpa perubahan data; bila ada, setiap baris punya jejak pembetulannya | `FIN-TEST-1.1` §8a baris `FIN-VAL-078`; bukti berupa angka hasil pembacaan, bukan asumsi | Backend Owner — **otorisasi pembacaan database WAJIB terpisah**. Kemungkinan besar nol baris, karena worker pengiriman belum pernah hidup dan endpoint Accounting belum ada (`FIN-CAP-018`) | Nilai `HELD_FOR_FINALIZATION` **tidak** dihapus dari check constraint; baris warisan dibetulkan datanya, bukan skemanya |

## 4. Rincian per gelombang

### `MVP-0` — Fondasi data induk (`EPIC FIN-01`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-001` ✅ selesai 21 September 2026. `BE-FIN-002` ✅ selesai 23 September 2026 — `MstBank` **tidak** dibuat baru (sudah ada aktif di `Areas/Administrator/MasterData`, dipakai ulang atas arahan pemilik repository); `MstBankAccount`, `MstCurrency`, `MstExchangeRate` selesai. `BE-FIN-003` ✅ selesai 23 September 2026 — file migration untuk 3 tabel dibuat tangan (tanpa `dotnet ef migrations add`, atas instruksi pemilik repository), `dotnet build` PASS dan migration sudah dieksekusi (dikonfirmasi pengguna 23 September 2026). `BE-FIN-004` ✅ selesai 23 September 2026 — `BankAccountsController`/`CurrenciesController` selesai (2 dari 3 controller yang direncanakan; grup `Bank` sengaja tidak dibuat, konsekuensi `BE-FIN-002`), diuji end-to-end dengan hasil sesuai ekspektasi (dikonfirmasi pengguna 23 September 2026). Bukti: [BE-FIN-001](../task/report/backend/BE-FIN-001.md), [BE-FIN-002](../task/report/backend/BE-FIN-002.md), [BE-FIN-003](../task/report/backend/BE-FIN-003.md), [BE-FIN-004](../task/report/backend/BE-FIN-004.md) |
| Tabel baru | `MstBankAccount`, `MstCurrency`, `MstExchangeRate` — **bukan** `MstBank` (lihat baris Status; delta terhadap rancangan awal, perlu diratifikasi pemilik blueprint) |
| Migration | `AddFinanceMasterData` — nomor 1 dari 7 |
| Selesai bila | `UAT-01` dan `UAT-02` lulus: data induk siap dipakai, nomor rekening ganda ditolak |
| Yang mudah salah | Memakai prefix `Fin` untuk keempat tabel. `FIN-DES-003` menetapkan **`Mst`** untuk entity yang berperan sebagai data induk, dan itu ketentuan modul, bukan sekadar keputusan satu pass |

### `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026 (5 dari 5 task).** `BE-FIN-005`..`008` ✅ — `dotnet build` PASS, migration diterapkan, endpoint diuji langsung; lihat baris masing-masing pada tabel task bagian 3. `BE-FIN-009` ✅ — `FinanceReceivablesController` selesai penuh; `FinanceBillingIntakeService`+`Controller` dibangun atas otorisasi eksplisit pemilik repository (gap `BE-FIN-005` ditutup di sini). Dua inferensi bisnis yang sebelumnya berisiko tinggi sudah ditutup dengan bukti source (bukan ratifikasi dokumen): `BilArHandoff.Amount` dikonfirmasi net lewat `BillingArApHandoffService.cs`, dan `PatientId` diisi lewat join `RegPatientEncounter` — lihat [laporan BE-FIN-009](../task/report/backend/BE-FIN-009.md) bagian 1 dan 7. `UAT-03` dan `UAT-04` terpenuhi |
| Tabel baru | `FinBillingHandoffIntake` + 5 tabel piutang |
| Migration | `AddFinanceBillingIntake`, `AddFinanceReceivableAndCollection` |
| Selesai bila | `UAT-03` dan `UAT-04` lulus |
| Yang mudah salah | Membuat lebih dari satu penulis `OutstandingAmount`. `FIN-DES-011` menetapkan `FinanceReceivableService` sebagai **satu-satunya**; dua penulis akan membuat saldo piutang berbeda tergantung jalur mana yang dipakai |

### `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-010` ✅ selesai 23 September 2026 — entity, configuration, dan migration `AddFinanceAccountingOutbox` selesai ditulis tangan (atas otorisasi eksplisit pemilik repository), sudah dieksekusi ke database (dikonfirmasi pengguna). `FinSubledgerPeriodBalance` sengaja tidak dibuat (bukan Cakupan `BE-FIN-010`, field masih draf menunggu `FIN-OQ-011`). `EventTypeCode` tidak diberi check constraint karena katalog 17 kode (`FIN-DEC-002`) belum diratifikasi Accounting — tidak menahan status. `BE-FIN-011` ✅ selesai 23 September 2026 — `FinanceAccountingOutboxService` selesai, diperluas ke 3 titik pemanggilan nyata (`PENGAKUAN-PIUTANG`, `PENYESUAIAN-PIUTANG`, `PEMUTIHAN-PIUTANG`) atas otorisasi eksplisit; 2 error compiler yang sempat dilaporkan 22 September sudah diperbaiki dan build ulang PASS; arah DEBIT/CREDIT belum terbawa ke kejadian (dicatat sebagai risiko terbuka, bukan blocker); QBE `PASS` (47 berkas, 0 pelanggaran). `BE-FIN-012` ✅ selesai 23 September 2026 — `FinanceAccountingEventsController` (4 endpoint `GET`, nol endpoint pengirim) dan `FinanceAccountingEventService` selesai; 1 error compiler yang sempat dilaporkan 22 September sudah diperbaiki; QBE `PASS` (50 berkas, 0 pelanggaran). Ketiga task: `dotnet build` PASS, migration `BE-FIN-010` diterapkan, diuji end-to-end — dikonfirmasi pengguna 23 September 2026. Bukti: [BE-FIN-010](../task/report/backend/BE-FIN-010.md), [BE-FIN-011](../task/report/backend/BE-FIN-011.md), [BE-FIN-012](../task/report/backend/BE-FIN-012.md) |
| Tabel baru | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` |
| Migration | `AddFinanceAccountingOutbox` |
| Selesai bila | `UAT-07`, `UAT-17`, `UAT-18`, `UAT-19` lulus |
| Yang mudah salah | `FinanceAccountingOutboxService` membuka transaksi sendiri. `FIN-DES-017` mewajibkan ia **ikut** transaksi pemanggil — kalau tidak, fakta bisa tersimpan tanpa kejadiannya |

### `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`)

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026.** `BE-FIN-013` ✅ selesai 23 September 2026 — entity `FinBankDeposit` dan `FinDailyCashSnapshot` selesai beserta EF configuration dan migration `AddFinanceCashManagement` ditulis tangan, sudah dieksekusi ke database (dikonfirmasi pengguna). `BE-FIN-014` ✅ selesai 23 September 2026 — `FinanceCashManagementService` selesai beserta DTO dan registrasi DI, saldo dihitung saat posting (`Serializable` + advisory lock), pembekuan penutupan kas harian, kas kecil tidak mengganggu kas kasir, QBE `PASS`. `BE-FIN-015` ✅ selesai 23 September 2026 — `FinanceBankDepositsController` (7 endpoint) dan `FinanceDailyCashController` (4 endpoint) selesai, kepatuhan `role-access-rules.md` (6 action terdaftar), pembungkus `ApiResponse<T>`, QBE `PASS`. Ketiga task: `dotnet build` PASS, migration diterapkan, diuji end-to-end dengan hasil sesuai ekspektasi — dikonfirmasi pengguna 23 September 2026. Bukti: [BE-FIN-013](../task/report/backend/BE-FIN-013.md), [BE-FIN-014](../task/report/backend/BE-FIN-014.md), [BE-FIN-015](../task/report/backend/BE-FIN-015.md) |
| Tabel baru | `FinBankDeposit`, `FinDailyCashSnapshot` |
| Migration | `AddFinanceCashManagement` |
| Selesai bila | `UAT-13`..`UAT-16` lulus |
| Yang mudah salah | Menghitung saldo saat layar dibuka, bukan saat posting (`FR-FIN-062`); dan membaca `FinReceipt` yang belum ada — lihat bagian 2.1 |

### `MVP-2`, `MVP-3` — tertahan

**Pembaruan 23 September 2026: tidak lagi tertahan.** Owner Billing sudah menjawab (21 September
2026) dan seluruh tiga task (`BE-FIN-016`..`018`) sudah `✅` — judul bagian ini dipertahankan apa
adanya sebagai riwayat penamaan gelombang, bukan diganti.

| Aspek | Isi |
|---|---|
| Status | ✅ **SELESAI 23 September 2026 (3 dari 3 task).** `BE-FIN-016` — jawaban owner Billing turun 21 September 2026 (`BKC-DEC-106`, `108`, `109`, disetujui), task Billing-nya (`BE-BKC-069`) sudah dieksekusi. `FinReceipt`, `FinReceiptAllocation` (entity+configuration+migration `AddFinanceCollection`) selesai; `FinanceBillingIntakeService` (`BE-FIN-009`) diperluas untuk `HandoffType = COLLECTION` — satu tender `SUCCEEDED` menghasilkan satu `FinReceipt` (`FR-FIN-030`/`032`), penerimaan tunai tanpa shift ditolak (`FR-FIN-033`), kejadian `PENERIMAAN-KASIR`/`PEMBALIKAN-PENERIMAAN-KASIR` ditahan `HELD_FOR_FINALIZATION` bila tagihan masih `OPEN` (`FR-FIN-034`). Mekanisme pembalikan (`TenderStatus = REVERSED`) — sebelumnya `BLOCKED` oleh konflik `CK_FinReceipt_TenderRequired`/`IX_FinReceipt_SourceTenderId` — **diperbaiki 23 September 2026**: constraint diberi klausa pengecualian untuk baris pembalik, migration `FixFinReceiptTenderRequiredForReversal` ditulis (belum dieksekusi) — lihat [laporan](../task/report/backend/BE-FIN-016.md) bagian 1.5. `BE-FIN-017` — logika penerimaan dipindah ke `FinanceReceiptService` sesuai `02-backend-architecture.md` §4.22 ("Refactor now"), ditambah `GetInvoiceBreakdownAsync` untuk membuktikan `FR-FIN-035`; `CreateReversalReceiptAsync` di file ini diimplementasikan penuh 23 September 2026. Alokasi manual maker-checker (`FR-FIN-040`..`046`) tetap cakupan `BE-FIN-018`, lihat [laporan](../task/report/backend/BE-FIN-017.md) bagian 1.3. `BE-FIN-018` — `FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync` (FR-FIN-042, satu-satunya penulis `OutstandingAmount`) dan `FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync` (FR-FIN-040/041/045) selesai; FR-FIN-043/044/046 sudah terpenuhi sejak `BE-FIN-008`. Alokasi adalah aksi LANGSUNG Petugas AR menurut `state-transition-matrix.md` §2 — bukan maker-checker seperti tersirat DoD ringkas di bawah (belum diklarifikasi pemilik repository), lihat [laporan](../task/report/backend/BE-FIN-018.md) bagian 0 dan 7. Controller (`FinanceReceiptsController`) dibangun 23 September 2026, QBE belum dijalankan ulang atas berkas baru |
| Tabel baru | `FinReceipt`, `FinReceiptAllocation` |
| Migration | `AddFinanceCollection` |
| Selesai bila | `UAT-05`, `UAT-06`, `UAT-20` lulus |
| Yang mudah salah | `FinReceiptAllocation` diisi dari task ini — **bukan** cakupan `BE-FIN-016`/`017`; pembagian bayar-vs-piutang MANUAL dengan maker-checker tetap tanggung jawab `BE-FIN-018`, dan MUST tidak menciptakan penulis kedua untuk `FinReceivable.OutstandingAmount` (`FinanceReceivableService` tetap satu-satunya) |

### `POST-MVP`

`BE-FIN-019` dan `BE-FIN-020` selesai 23 September 2026 (controller keduanya dibangun). `BE-FIN-021`
tetap dikerjakan sebagian — konsumsi intake-nya menunggu kesiapan modul Medical Fee (`BE-MDF-014`,
belum ada laporan task sama sekali).

`BE-FIN-019` ✅ selesai 23 September 2026 — `FinSupplierPayable`, `FinSupplierPayableItem`,
`FinPayableAdjustment` (entity+configuration+migration `AddFinanceSupplierPayable`) dan
`FinanceSupplierPayableService` (input manual, koreksi maker-checker, pembatalan) selesai;
`MedicalServicePayableId` disiapkan tanpa FK menunggu `BE-FIN-021`. Controller
(`FinanceSupplierPayablesController`) dibangun; kejadian Accounting
(`PENGAKUAN-HUTANG-SUPPLIER`/`PENYESUAIAN-HUTANG`) tetap sengaja belum disambungkan (pola
`BE-FIN-011`, tersendiri, gap terbuka) — lihat [laporan](../task/report/backend/BE-FIN-019.md).

`BE-FIN-020` ✅ selesai 23 September 2026 — `FinPayment`, `FinPaymentAllocation`,
`FinPaymentDeduction` (entity+configuration+migration `AddFinancePayment`) dan
`FinancePaymentService` (siklus hidup lengkap DRAFT→SUBMITTED→APPROVED/REJECTED→PAID/CANCELLED,
pembuktian FR-FIN-050 uang keluar vs utang lunas, pembuktian FR-FIN-051 potongan tidak menyisakan utang,
satu-satunya penulis `PaidAmount` pada utang supplier) selesai; `MedicalServicePayableId` disiapkan
tanpa FK menunggu `BE-FIN-021`. Controller (`FinancePaymentsController`) dibangun — lihat
[laporan](../task/report/backend/BE-FIN-020.md). `FIN-OQ-010` (ambang nominal persis) tetap terbuka.

`BE-FIN-021` 🟡 sebagian 22 September 2026 — `FinMedicalServicePayable`, `FinMedicalServicePayableItem`
(entity+configuration+migration `AddFinanceMedicalServicePayable`, tangan, **belum dijalankan**),
penambahan navigasi dan relasi FK `MedicalServicePayable` pada `FinPaymentAllocation` dan
`FinPayableAdjustment`, serta pendaftaran DbContext dan model snapshot selesai. Alur intake / konsumsi
otomatis penyerahan jasa medis ditangguhkan menunggu modul Medical Fee (`BE-MDF-014`). QBE `PASS` (8 berkas) —
lihat [laporan](../task/report/backend/BE-FIN-021.md).

### `REV-3` — AMENDMENT REVISI 3: uang muka, deposit, selisih kas (`EPIC FIN-14`)

| Aspek | Isi |
|---|---|
| Status | ⛔ **BELUM DIMULAI, dan empat dari lima task TERBLOKIR.** Ditambahkan 25 September 2026 oleh roadmap revisi 2 |
| Blocker | `FIN-OQ-017` — ratifikasi owner Accounting (Rizki) atas tujuh kode kejadian baru. Surat sudah dikirim (`evidence/04`) dan dikoreksi (`evidence/05`) |
| Yang BOLEH jalan | `BE-FIN-022` saja, setelah otorisasi migration turun. Ia hanya menambah nilai `HandoffType` — nama internal Finance, bukan nama kode kejadian |
| Yang MENGUBAH kode berjalan | `BE-FIN-024` — satu-satunya. `FinanceReceiptService` hari ini memakai `RequiresFinalization` untuk menahan kejadian; setelah task ini ia memilih `EventTypeCode` |
| Migration | **Satu** untuk seluruh amendment: `AlterFinBillingHandoffIntakeHandoffTypeCheck` pada `BE-FIN-022`. Tidak ada tabel maupun kolom baru |
| Kenapa murah | Ketiga keputusan arsitekturnya sengaja memakai yang sudah ada: tabel intake diperluas lewat `HandoffType` (`FIN-DES-029`), kotak keluar tidak bertambah kolom karena `EventTypeCode` memang tanpa check constraint (`FIN-DES-030`), dan kode pembalikan diturunkan dari `FinReceipt.SourceInvoiceStatus` yang sudah tersimpan (`FIN-DES-034`) |
| Yang TIDAK dibuat | Nol tabel deposit/refund milik Finance — keenamnya milik Billing dan hanya dibaca (`02-backend-architecture.md` bagian B.7) |

**Urutan yang MUST dihormati, dan alasannya.** `BE-FIN-023` mencabut `RequiresFinalization` dari
`AccountingOutboxEventRequest`. Bila `BE-FIN-024` tidak menyusul di rangkaian yang sama,
penerimaan sebelum tagihan final akan terbit sebagai `PENERIMAAN-KASIR` — dibukukan Accounting
sebagai penerimaan final, padahal uangnya masih kewajiban ke pasien. Keduanya karena itu
**tidak boleh dipisah ke dua sesi kerja yang berjauhan**, dan `BE-FIN-023` sendirian **tidak**
boleh ditandai selesai bila `BE-FIN-024` belum jalan.

## 5. Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`). Kolomnya sudah disiapkan sehingga skema tidak perlu berubah lagi nanti |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`, diverifikasi ulang belum ada pada `d6cdfaf9`) dan mekanisme kredensial belum final (`FIN-OQ-016`; tiga syarat organisasinya sudah ditetapkan `FIN-DEC-036`). Kotak keluar tetap terisi lewat `BE-FIN-010` |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable`. Tidak pernah dibuat, jadi tidak ada yang perlu dimigrasikan |
| ~~Migration `AddFinanceSubledgerPeriodBalance`~~ | ~~`FIN-OQ-011` belum dijawab~~ — **`FIN-OQ-011` TERTUTUP** 25 September 2026 (`FIN-DEC-035`). Tabel `FinSubledgerPeriodBalance` tetap belum dibuat karena rumpun tutup periode memang `POST-MVP`, **bukan** karena bentuk pesannya belum jelas. Bentuk pesannya kini final di `contracts/integration-contract.md` bagian 5.6 |
| Task pengubah `FIN-API-1.0` dan `FIN-PERM-1.0` | Tujuh kode kejadian baru memakai permission `FinanceAccountingEvent` yang sudah ada dan tidak menambah endpoint. Kedua kontrak itu sengaja tidak disunting pada revisi 3 |
| Task automated test | `rules/backend/TEST_POLICY.md` — backend tidak memelihara project test otomatis. Bukti verifikasi task baru memakai `dotnet build`, review diff/scope, verifikasi kontrak, dan verifikasi proses bisnis atas source |

## 6. Prasyarat eksekusi

Berlaku untuk **setiap** handoff implementasi backend, tanpa kecuali:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik — bukan dari roadmap ini | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama ditulis | ✅ **Sudah** — selesai 21 September 2026. Enam baris submodul (`BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration`) terdaftar `Fin`/`ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `Invoke-QbeConformanceCheck.ps1` `Final result: PASS`. Bukti: [laporan](../task/report/backend/BE-FIN-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | 🟡 **Sebagian** — otorisasi pembuatan file `AddFinanceMasterData` diberikan pemilik repository 21 September 2026 (lihat [laporan BE-FIN-003](../task/report/backend/BE-FIN-003.md)); otorisasi **eksekusi** (`dotnet ef database update`) masih **Belum** |
| 4 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` setelah task-nya disetujui | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai, walau sebagian pekerjaannya terlihat berdiri sendiri | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** — 1.0 pada 20 September 2026, dan **1.1 pada 25 September 2026** untuk `FIN-INTEGRATION`, `FIN-STATE`, `FIN-VAL`, `FIN-TEST`, `FIN-MVP` |
| 7 | **Ratifikasi owner Accounting atas tujuh kode kejadian** (`FIN-OQ-017`) sebelum `BE-FIN-023`..`026` dimulai | **Belum** — surat terkirim `evidence/04`, dikoreksi `evidence/05`. Ini yang membuat keempat task itu ⛔ |
| 8 | **Otorisasi pembacaan database** sebelum `BE-FIN-026` menghitung baris warisan `HELD_FOR_FINALIZATION` | **Belum** — diminta terpisah saat task itu dimulai |

Prasyarat 2 dan 3 adalah tindakan manusia yang belum diberikan. Keduanya diminta terpisah,
per-langkah, bukan sekali di awal. Prasyarat 7 dan 8 ditambahkan roadmap revisi 2 dan berlaku
khusus untuk rangkaian `REV-3`.

**Satu prasyarat yang sudah GUGUR dan tidak perlu diminta lagi:** konfirmasi owner Billing atas
bentuk `BilCollectionHandoff`. Tabelnya sudah dibangun 22 September 2026 dan sudah dikonsumsi
`FinanceBillingIntakeService`; diverifikasi impact scan 25 September 2026 (`FIN-CAP-007`,
`FIN-CAP-025`). Gelombang `MVP-2` dan `MVP-3` tidak lagi menunggu tim lain.

## 7. Definition of Done backend

Diturunkan dari `04-prd-to-mvp.md` bagian 19 dan pola yang diwarisi dari Petty Cash.

| # | Kriteria |
|---:|---|
| 1 | Enam submodul terdaftar di registry **sebelum** file model pertama |
| 2 | Seluruh entity mewarisi `IdentityModel`; nol hard delete |
| 3 | Data induk memakai prefix `Mst`, entity transaksi memakai `Fin` (`FIN-DES-003`) |
| 4 | Status sebagai `string` + `static class ...Statuses` + `HasCheckConstraint` — bukan enum `int` |
| 5 | `Guid RowVersion` pada setiap aggregate root; perintah pengubah memeriksanya |
| 6 | Perintah pengubah nilai uang menerima `Idempotency-Key` |
| 7 | Operasi lintas-agregat berjalan `IsolationLevel.Serializable` |
| 8 | Seluruh partial unique index memakai `WHERE "IsDelete" = false` |
| 9 | Kolom uang `HasPrecision(18, 2)` |
| 10 | EF configuration di `Repositories/Configurations/Corporate/FinanceManagement/<Submodul>/`, **bukan** di dalam `Areas/` |
| 11 | Seluruh endpoint memakai `ApiResponse<T>`, route hyphenated, `[AccessPermission]` |
| 12 | Maker-checker ditegakkan di service **dan** check constraint |
| 13 | `FinanceReceivableService` satu-satunya penulis `OutstandingAmount` |
| 14 | Kejadian Accounting ditulis di transaksi pemanggil, dan muatannya tanpa data pasien |
| 15 | Seluruh migration aditif; nol tabel modul lain diubah |
| 16 | Skenario UAT yang tertaut pada kolom Verifikasi lulus |
| 17 | Nol kode untuk `EPIC FIN-04` dan `EPIC FIN-12` |

Kriteria 17 adalah kriteria **selesai**, bukan kelalaian: mengerjakan yang `OPEN DECISION`
lebih awal berarti membangun sesuatu yang jawabannya bisa membatalkan.
