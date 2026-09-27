# Finance Management — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-BE-001
parent_roadmap: FIN-ROADMAP-001 revisi 5
roadmap_revision: 5
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 5
blueprint_status: approved
backend_commit_sha: 96bf9746fedb63119a317a53758b0f0748ec7ad1
backend_commit_sha_baseline: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
contracts:
  FIN-API-1.2: locked 2026-09-26 (revisi 5; 1.1 revisi 4 tetap berlaku untuk bagian yang tidak diganti)
  FIN-PERM-1.2: locked 2026-09-26 (revisi 5)
  FIN-STATE-1.3: locked 2026-09-26 (revisi 5)
  FIN-VAL-1.3: locked 2026-09-26 (revisi 5)
  FIN-INTEGRATION-1.3: locked 2026-09-26 (revisi 5 — bagian 5.9)
  FIN-TEST-1.3: locked 2026-09-26 (revisi 5)
  FIN-MVP-1.4: locked 2026-09-26 (revisi 5)
roadmap_revision_3_note: >
  Revisi 3 (25 September 2026) MENAMBAHKAN empat belas task BE-FIN-027..040 untuk AMENDMENT
  REVISI 4 blueprint (FIN-DES-037..044, approved dan kontraknya locked hari yang sama) —
  EPIC FIN-15 Purchasing/AP, FIN-16 AR Invoice Agregat, FIN-17 Potongan AR. TIDAK ada task lama
  yang dinomori ulang, diubah outcome-nya, atau diturunkan statusnya.
  EMPAT TEMUAN dari pembacaan source pada 96bf9746 yang mengubah rencana — lihat bagian 4
  "REV-4": (1) batas Rp 50.000.000 tepat berbeda antara kode dan FIN-DEC-052, sehingga
  BE-FIN-028 mengubah perilaku pembayaran yang sudah berjalan pada satu nilai itu;
  (2) FinanceSupplierPayableService sudah menulis AP_CREATED sebesar OriginalAmount penuh,
  sehingga BE-FIN-034 MUST mengirim nilai pokok saja agar PPN tidak terkredit dua kali;
  (3) jalur pelunasan piutang menulis AR_PAYMENT (kas masuk), sehingga potongan AR tidak boleh
  lewat jalur itu — awalnya BE-FIN-040 ⛔ FIN-OQ-024; (4) sudah ada GET api/finance/payable/aging
  atas FinSupplierPayable, sehingga endpoint aging Purchasing dicabut FIN-OQ-025.
  Dua task ⛔ oleh keputusan yang belum ada saat itu: BE-FIN-036 (FIN-OQ-023), BE-FIN-040
  (FIN-OQ-024) — lihat roadmap_revision_4_note untuk keadaan sesudah closure pass. FIN-OQ-020
  TIDAK menahan task mana pun (FIN-DEC-056).
roadmap_revision_5_note: >
  Revisi 5 (26 September 2026) menurunkan AMENDMENT REVISI 5 blueprint (FIN-DES-045..050,
  approved dan kontraknya locked 26 September 2026). BE-FIN-036 DIBUKA — skemanya kini digambar.
  Enam task diperbarui ke bentuk revisi 5 (030, 031, 035, 036, 038, 040); SATU task baru
  BE-FIN-041 (kolom DepositAppliedAmount + constraint pada FinPayment yang sudah berjalan).
  Nol task REV-4 yang ⛔. Task lama tidak dinomori ulang.
roadmap_revision_4_note: >
  Revisi 4 (25 September 2026, sesudah /grill-me closure pass) MEMBUKA BE-FIN-040 dan FE-FIN-013
  (semula ⛔ FIN-OQ-024): kode kejadian POTONGAN-PIUTANG-NON-TUNAI sudah diusulkan (FIN-DEC-058),
  sehingga service dapat menulis outbox seperti BE-FIN-034 menulis PPN-MASUKAN-PEMBELIAN —
  hanya worker pengirimannya yang menunggu Rizki (FIN-OQ-026, pola FIN-DEC-056). BE-FIN-036 dan
  FE-FIN-010 TETAP ⛔: FIN-OQ-023 tertutup sisi keputusan bisnis (FIN-DEC-057), tetapi
  konsekuensi skemanya (FinPaymentAllocation/FinSupplierReturnDepositUsage) belum digambar.
  Task lama TIDAK dinomori ulang.
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

POST-MVP ─> REV-4 (BE-FIN-027 ✅..041)
```

`REV-3` adalah kelompok task AMENDMENT REVISI 3 (`BE-FIN-022`..`026`). Ia **tidak diberi nomor
gelombang** karena `EPIC FIN-14` berstatus `OPEN DECISION` pada `04-prd-to-mvp.md`.

`REV-4` adalah kelompok task AMENDMENT REVISI 4 (`EPIC FIN-15`/`16`/`17`), seluruhnya gelombang
`POST-MVP` pada `04-prd-to-mvp.md` bagian 20.1. Berbeda dari `REV-3`, ketiga epic-nya **bukan**
`OPEN DECISION`, sehingga task-nya boleh dijadwalkan.

### REV-4 — Purchasing/AP (`EPIC FIN-15`)

```text
BE-FIN-027 ✅ ─> BE-FIN-029 🟡 ─> BE-FIN-030 🟡 ─> BE-FIN-031 🟡 ─┬─> BE-FIN-032 🟡
                                                      │
BE-FIN-028 🟡 ───────────────────────────────────────────┤
                                                      │
                                                      └─> BE-FIN-033 🟡 ─> BE-FIN-034 🟡 ─> BE-FIN-035 ─> BE-FIN-037

BE-FIN-041 ─> BE-FIN-036 <─ BE-FIN-035
```

`BE-FIN-028` menjadi prasyarat `BE-FIN-032` dan `BE-FIN-034` (keduanya memanggil resolver),
bukan prasyarat `BE-FIN-033`; panahnya digabung pada satu titik supaya grafik muat satu layar.
`BE-FIN-028` sendiri tidak menunggu apa pun dan boleh dikerjakan paling awal. **Status 26 September
2026:** source selesai, `dotnet build` belum dijalankan (instruksi eksplisit pengguna) —
`BE-FIN-032`/`034` MUST menunggu konfirmasi build sebelum memanggil `FinanceApprovalTierResolver`
dianggap aman dipakai ulang.

### REV-4 — AR Invoice Agregat dan Potongan AR (`EPIC FIN-16`, `FIN-17`)

```text
BE-FIN-038 ─┬─> BE-FIN-039
            │
            └─> BE-FIN-040
```

Rumpun ini **tidak bergantung** pada rumpun Purchasing sama sekali dan boleh berjalan paralel.
`BE-FIN-040` tidak lagi ⛔ — `FIN-DEC-058` (25 September 2026) mengusulkan kode kejadiannya;
hanya worker pengirimannya yang menunggu Rizki (`FIN-OQ-026`), pola `FIN-DEC-056`.

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
BE-FIN-022 🟡 ─────────────────────────────────────┐
                                                   │
{FIN-OQ-017 ⛔} ─> BE-FIN-023 ⛔ ─> BE-FIN-024 ⛔ ─┴─> BE-FIN-025 ⛔ ─> BE-FIN-026 ⛔
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| — | Otorisasi migration (`AGENTS.md` Keselamatan Database) | `BE-FIN-022` 🟡 — **bebas dari `FIN-OQ-017`**, tetapi tidak diberi nomor gelombang karena `EPIC FIN-14` masih `OPEN DECISION`. Source selesai 25 September 2026; **eksekusi migration belum dijalankan** |
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

### Gelombang eksekusi — `REV-4` (`POST-MVP`)

| Urutan | Boleh mulai setelah | Task |
| ---: | --- | --- |
| R4-1 | — | `BE-FIN-027` ✅ (registry, selesai 26 September 2026), `BE-FIN-028` (resolver ambang), `BE-FIN-038` (entity + migration AR), `BE-FIN-041` (kolom `FinPayment`) — keempatnya bebas, boleh paralel |
| R4-2 | `BE-FIN-027` ✅ | `BE-FIN-029` 🟡 — source selesai, `dotnet build` tertunda |
| R4-2 | `BE-FIN-038` + otorisasi eksekusi migration | `BE-FIN-039` |
| R4-3 | `BE-FIN-029` 🟡 | `BE-FIN-030` 🟡 — source selesai, `dotnet build` tertunda |
| R4-4 | `BE-FIN-030`, konfirmasi `dotnet build` (028/029/030), + otorisasi migration | `BE-FIN-031` |
| R4-5 | `BE-FIN-028`, `BE-FIN-031` | `BE-FIN-032` |
| R4-5 | `BE-FIN-031` | `BE-FIN-033` |
| R4-6 | `BE-FIN-028`, `BE-FIN-033` | `BE-FIN-034` |
| R4-7 | `BE-FIN-034` | `BE-FIN-035` |
| R4-8 | `BE-FIN-034`, `BE-FIN-035` | `BE-FIN-037` |
| R4-8 | `BE-FIN-035`, `BE-FIN-041` | `BE-FIN-036` — **dibuka** revisi 5 (`FIN-DES-045`..`047`) |
| R4-9 | `BE-FIN-038` | `BE-FIN-040` — **dibuka** `FIN-DEC-058`; worker pengirimannya sendiri (bukan task ini) tetap menunggu `FIN-OQ-026` |

**Yang TIDAK menahan satu pun task di atas:** ratifikasi `PPN-MASUKAN-PEMBELIAN` (`FIN-OQ-020`).
Sejak `FIN-DEC-056` ia hanya menahan aktivasi worker pengiriman — dan worker itu sendiri bagian
`EPIC FIN-12` yang belum dibangun, jadi tidak ada task di roadmap ini yang menyentuhnya.

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
| `BE-FIN-022` 🟡 | Empat jenis fakta Billing baru menjadi nilai `HandoffType` yang sah | `FIN-DES-029`, `FIN-DEC-040`..`044` | `FIN-STATE-1.1` §1, `FIN-VAL-1.1` | `FinBillingHandoffIntake` beserta unique index `(HandoffType, SourceHandoffKey)` yang sudah ada (`FIN-DES-008`, `009`) | Konstanta `FinBillingHandoffTypes` bertambah `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW`; **satu** migration `AlterFinBillingHandoffIntakeHandoffTypeCheck` mengubah `CK_FinBillingHandoffIntake_HandoffType` dari 4 menjadi 8 nilai | — (tabel intake sudah ada sejak `BE-FIN-005` ✅) | Baris ber-`HandoffType` baru dapat disimpan; baris ber-nilai lama tetap sah; unique index tetap menolak fakta yang sama dua kali | `dotnet build`; verifikasi proses bisnis atas check constraint; `FIN-TEST-1.1` §8a baris `FIN-DES-029` (sinkronisasi ganda ditolak unique index) | Backend Owner — **otorisasi migration WAJIB diminta terpisah** (`AGENTS.md` Keselamatan Database). **Bebas dari `FIN-OQ-017`**: nama `HandoffType` adalah keputusan internal Finance, bukan nama kode kejadian yang perlu ratifikasi Accounting | Nol tabel dan nol kolom baru; `Down()` mengembalikan constraint ke 4 nilai dan **hanya aman** bila belum ada baris memakai nilai baru |
| `BE-FIN-023` ⛔ | Kotak keluar menerima nilai nol/negatif untuk dua jenis kejadian, dan membawa rincian saldo subledger | `FIN-DES-031`, `032`, `033`; `FIN-DEC-035`, `043` | `FIN-INTEGRATION-1.1` §5.2, §5.6; `FIN-VAL-1.1` `FIN-VAL-079`..`084` | `FinanceAccountingOutboxService` yang sudah ada (`BE-FIN-011` ✅); **nol perubahan skema** (`Amount` sudah `HasPrecision(18,2)` tanpa check constraint nilai) | `ValidateRequest` dipersempit; `AccountingOutboxEventRequest` bertambah `SubledgerBalance` dan **kehilangan** `RequiresFinalization`; `BuildPayloadJson` menyertakan objek saldo bila terisi | ⛔ `FIN-OQ-017` | Selisih kas `-30.000` tersimpan; saldo subledger `0` tersimpan; `PENERIMAAN-UANG-MUKA` bernilai nol ditolak `400`; `SubledgerBalance` pada kejadian non-saldo ditolak `400`; periode `2026-11-30` ditolak `400` | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-VAL-079`, `080`, `081`, `082`, `084`, `FIN-DES-032` | Backend Owner. **Menyentuh service yang sudah berjalan** — `RequiresFinalization` dipakai `FinanceReceiptService` hari ini, sehingga `BE-FIN-024` MUST menyusul di rangkaian yang sama agar penerimaan pra-final tidak terbit sebagai `PENERIMAAN-KASIR` | Payload tetap dibangun di dalam service, bukan diterima mentah dari pemanggil (`FR-FIN-073` tetap terkunci di level tipe) |
| `BE-FIN-024` ⛔ | Penerimaan sebelum tagihan final terbit segera dengan jenis kejadian yang benar, dan pembalikannya mengikuti penerimaan aslinya | `FIN-DEC-030`, `FIN-DES-033`, `034`; `FR-FIN-034` (diperbarui), `FR-FIN-076` | `FIN-INTEGRATION-1.1` §5.5; `FIN-STATE-1.1` §9; `FIN-VAL-1.1` `FIN-VAL-085` | `FinReceipt.SourceInvoiceStatus` yang **sudah ada** — nol kolom baru (`FIN-DES-034`) | `FinanceReceiptService`: pemilihan `EventTypeCode` dari `SourceInvoiceStatus`, dan pemilihan kode pembalikan dari baris penerimaan asli yang ditunjuk `ReversalOfReceiptId` | ⛔ `BE-FIN-023` — yang sendirinya menunggu `FIN-OQ-017`, sehingga blocker itu ikut berlaku secara berantai | Tagihan `OPEN` → `PENERIMAAN-UANG-MUKA` berstatus `PENDING`, **bukan** tertahan; tagihan `FINAL` → `PENERIMAAN-KASIR`; pembalikan penerimaan uang muka **tetap** `PEMBALIKAN-PENERIMAAN-UANG-MUKA` walaupun tagihannya sudah `FINAL` saat pembalikan | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-DEC-030` (tiga skenario), `FIN-DES-034` (dua skenario), `FIN-VAL-085` | Backend Owner. **INI SATU-SATUNYA TASK YANG MENGUBAH PERILAKU YANG SUDAH BERJALAN** — `FinanceReceiptService` baris ~126 dan ~194 hari ini memakai `RequiresFinalization`; review diff MUST membuktikan tidak ada jalur lain yang ikut berubah | Nol baris baru berstatus `HELD_FOR_FINALIZATION` dihasilkan kode setelah task ini |
| `BE-FIN-025` ⛔ | Deposit, kelebihan bayar, dan selisih kas shift masuk ke kotak keluar dengan jenis kejadian masing-masing | `FIN-DEC-031`, `034`, `040`..`043`; `FIN-DES-035`, `036`; `FR-FIN-077`..`079` | `FIN-INTEGRATION-1.1` §2a, §5.4; `FIN-VAL-1.1` `FIN-VAL-086` | `BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilCashVarianceReview` — **read-only**, sudah terdaftar di `ApplicationDbContext` (`FIN-CAP-022`..`024`) | `FinanceBillingIntakeService`: empat jalur sinkronisasi baru dengan anti-join ke `FinBillingHandoffIntake` beserta penyempitan waktu; `SELISIH-KAS-SHIFT` memakai `BilCashVarianceReview.Id` dan tanggal shift | ⛔ `BE-FIN-022` **dan** `BE-FIN-024` | `ALLOCATION` → `PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa `FinReceipt` baru; `RELEASE` → `PENGEMBALIAN-UANG-MUKA`; refund `ALLOCATION_EXCESS` → `PENGEMBALIAN-UANG-MUKA`; refund `SETTLEMENT` → **nol kejadian**; shift belum `REVIEWED` → **nol kejadian**; `Variance` nol → **nol kejadian**; nol perubahan pada tabel `Bil*` mana pun | `dotnet build`; `FIN-TEST-1.1` §8a baris `FIN-DEC-040`, `041` (tiga skenario), `042`, `043` (dua skenario), `FIN-VAL-080`, `086`, aturan bisnis #9 | Backend Owner. Risiko utama: menulis ke tabel `Bil*` tanpa sengaja — dibuktikan tidak terjadi dengan membandingkan `RowVersion`/`UpdateDateTime` sebelum dan sesudah | `TargetEntityId` boleh kosong untuk jalur yang tidak melahirkan entity Finance; keempat jenis berhenti di `CONSUMED`, **tidak** mengirim ACK ke Billing |
| `BE-FIN-026` ⛔ | Baris warisan `HELD_FOR_FINALIZATION` dibereskan sebelum pengiriman diaktifkan | `FIN-DEC-030`; `02-backend-architecture.md` bagian B.6; `FIN-VAL-1.1` `FIN-VAL-078` | `FIN-STATE-1.1` §9 (transisi migrasi satu kali) | — | Penghitungan baris berstatus `HELD_FOR_FINALIZATION`; bila ada, pembetulan `EventTypeCode` menjadi `PENERIMAAN-UANG-MUKA` untuk penerimaan ber-`SourceInvoiceStatus` `OPEN`, lalu status dipindah ke `PENDING`. Worker MUST melewati **dan melaporkan** baris yang belum dibetulkan | ⛔ `BE-FIN-025` | Jumlah baris warisan dilaporkan apa adanya; bila nol, task ditutup tanpa perubahan data; bila ada, setiap baris punya jejak pembetulannya | `FIN-TEST-1.1` §8a baris `FIN-VAL-078`; bukti berupa angka hasil pembacaan, bukan asumsi | Backend Owner — **otorisasi pembacaan database WAJIB terpisah**. Kemungkinan besar nol baris, karena worker pengiriman belum pernah hidup dan endpoint Accounting belum ada (`FIN-CAP-018`) | Nilai `HELD_FOR_FINALIZATION` **tidak** dihapus dari check constraint; baris warisan dibetulkan datanya, bukan skemanya |
| ✅ `BE-FIN-027` | Submodul `Purchasing` terdaftar eksplisit di registry kepemilikan modul | `FIN-DES-037`, `FIN-DES-002` (preseden), `02-backend-architecture.md` C.7 | — | Baris registry enam submodul yang sudah ada (`BE-FIN-001` ✅) | Satu baris `Corporate / Finance \| FinanceManagement / Purchasing / Pembelian \| BUSINESS DOMAIN / MODULE \| Fin \| ACTIVE` + satu entri riwayat perubahan | — | Baris tercatat; prefix tetap `Fin` (nol prefix baru); `Invoke-QbeConformanceCheck.ps1` `PASS` | Diff berkas registry; hasil QBE | Backend Owner — **prasyarat `QBE-MOD-003`, MUST sebelum file model Purchasing pertama** | Registry ter-diff terpisah dari kode; tidak memberi wewenang implementasi/migration — ✅ **Selesai 26 September 2026**, [laporan](../task/report/backend/BE-FIN-027.md) |
| 🟡 `BE-FIN-028` | Satu resolver jenjang approval dipakai pembayaran, PO, dan Purchasing Invoice — dengan batas sesuai `FIN-DEC-052` | `FIN-DEC-050`, `052`; `FIN-DES-039` (koreksi 25 September 2026) | `FIN-VAL-1.2` `FIN-VAL-052`, `102`, `109`; `FIN-TEST-1.2` B.1 | `FinancePaymentService.ResolveApprovalTier` + `ApprovalTiers` (`FIN-CAP-035`) — **dipindah, bukan ditulis ulang** | `FinanceApprovalTierResolver` (baru) memuat `ApprovalTiers.Tier1`/`Tier2` dan `Resolve(decimal)`: `< 50.000.000 → TIER_1`, `>= 50.000.000 → TIER_2`; `FinancePaymentService` memanggilnya; komentar "provisional/FIN-OQ-010" dicabut di tiga berkas; registrasi DI | — | Rp 49.999.999 → `TIER_1`; **Rp 50.000.000 tepat → `TIER_2`**; Rp 50.000.001 → `TIER_2`; pembayaran yang sudah `SUBMITTED` tidak dihitung ulang tier-nya | `dotnet build` **(sengaja belum dijalankan atas instruksi pengguna — lihat laporan)**; review diff membuktikan hanya satu nilai batas yang berubah; baris `FIN-DEC-052` pada `FIN-TEST-1.2` B.1 | Backend Owner. **MENGUBAH PERILAKU YANG SUDAH BERJALAN** pada satu nilai: pembayaran tepat Rp 50.000.000 kini butuh Manajer, bukan Supervisor. Disetujui lewat `FIN-DEC-052` | Nol perubahan skema; tidak ada salinan logika ambang kedua di mana pun — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-028.md) |
| 🟡 `BE-FIN-029` | Model dokumen Purchasing: PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice | `FIN-DES-037`, `FIN-DEC-045`, `051` | `FIN-STATE-1.2` B.1-B.4; `erd/data-dictionary.md` C.1-C.7, C.16 | `MstSupplier` (`FIN-CAP-027`) — rujukan FK, nol kolom baru; pola status `string` + `static class` dari `FinPayment` | Tujuh entity + tujuh configuration di `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/` + `DbSet` pada `ApplicationDbContext`; check constraint status dan `ApprovalTier`; `UNIQUE (InvoiceExchangeId)` | `BE-FIN-027` ✅ | Bentuk kolom, tipe, FK, `DeleteBehavior`, unique, dan check constraint persis `data-dictionary.md` C.1-C.7; `FinGoodsReceipt.PurchaseOrderId` wajib; `FinInvoiceExchange.PurchaseOrderId`/`GoodsReceiptId` nullable `SetNull` | `dotnet build` **(sengaja belum dijalankan atas instruksi pengguna)**; review configuration terhadap DDL C.16 | Backend Owner | `IdentityModel` diwarisi; `Guid RowVersion` pada aggregate root; `HasPrecision(18,2)`; nol hard delete — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-029.md) |
| 🟡 `BE-FIN-030` | Model Retur Pembelian dan Deposit Retur (bentuk revisi 5), dan utang supplier tahu asal Purchasing Invoice-nya | `FIN-DES-038`, `040`, `045`, `046`; `FIN-DEC-047`, `057` | `FIN-STATE-1.3` C.1-C.2; `data-dictionary.md` C.8-C.10, C.12 **dan D.2** (`FinSupplierReturnDepositUsage` bentuk revisi 5 — **bukan** C.11) | Pola struktur `BilRefundableCredit` (arah dibalik); `FinSupplierPayable` (`FIN-CAP-026`); `FinPayment` yang sudah ada sebagai FK tujuan pemakaian | Empat entity + configuration; `FinSupplierReturnDepositUsage` ber-`PaymentId`, `Status` (`RESERVED`/`APPLIED`/`RELEASED`), `ReleasedAt`, `RowVersion`, unique index parsial `(PaymentId, SupplierReturnDepositId)`; `FinSupplierPayable.SourcePurchasingInvoiceId` (nullable, FK `SetNull`) | `BE-FIN-029` 🟡 | `AvailableAmount >= 0`; `UNIQUE (SourceReturnId)`; `CK_FinSupplierReturnDepositUsage_ReleasedAt`; baris `FinSupplierPayable` lama tetap valid dengan kolom baru `NULL` | `dotnet build` **(sengaja belum dijalankan atas instruksi pengguna)**; review configuration terhadap DDL `data-dictionary.md` D.4(b) | Backend Owner. **Jangan membangun dari C.11** — bentuk itu digantikan revisi 5 | Jalur input manual `FinSupplierPayable` tidak tersentuh perilakunya (`FIN-DES-040`) — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-030.md) |
| 🟡 `BE-FIN-031` | Skema Purchasing/AP tersedia di database | `FIN-DES-037`, `038`, `040`, `045`; `02-backend-architecture.md` C.9 langkah 1 dan 3, D.7 baris 1 | — | — | Migration `AddPurchasingApRumpun` (11 tabel, `FinSupplierReturnDepositUsage` dengan bentuk D.2) lalu `AddSourcePurchasingInvoiceIdToSupplierPayable` (1 kolom) — dua berkas terpisah, urutan itu; `ApplicationDbContextModelSnapshot.cs` diperbarui manual untuk 11 entity + kolom `FinSupplierPayable.SourcePurchasingInvoiceId` | `BE-FIN-030` 🟡 | `Up()` aditif murni; `Down()` menghapus bersih dalam urutan FK terbalik; nol tabel modul lain tersentuh | `dotnet build`; migration diterapkan di lingkungan pengembangan — **keduanya sengaja belum dijalankan atas instruksi pengguna** | Backend Owner. **Otorisasi terpisah WAJIB** untuk membuat berkas migration **dan** untuk mengeksekusinya (`AGENTS.md` Keselamatan Database) — pembuatan berkas disetujui 26 September 2026, eksekusi **belum**. `Migration.Designer.cs` kedua migration ditulis ringkas (pola `AddFinancePayment`/`AddFinanceMedicalServicePayable` yang sudah ada di repo ini — tanpa `BuildTargetModel` penuh), bukan kekurangan; sumber kebenaran model tetap `ApplicationDbContextModelSnapshot.cs`. Tiga nama FK asli melebihi batas 63 karakter Postgres dan sudah dipendekkan (`FK_FinPurchasingInvoiceItem_PurchasingInvoiceId`, `FK_FinSupplierPayable_SourcePurchasingInvoiceId`, `FK_FinSupplierReturnDepositUsage_SupplierReturnDepositId`) | Dapat dijalankan tanpa downtime; nol backfill — 🟡 **Berkas migration & snapshot selesai ditulis 26 September 2026, belum di-build/dieksekusi**, [laporan](../task/report/backend/BE-FIN-031.md) |
| 🟡 `BE-FIN-032` | Purchase Order dan Tanda Terima Barang dapat dicatat dan disetujui berjenjang | `FIN-DEC-050`, `052`; `FR-FIN-081` | `FIN-API-1.1` B.1, B.2; `FIN-PERM-1.1` B.2, B.3; `FIN-STATE-1.2` B.1, B.2; `FIN-VAL-1.2` `100`..`104` | `FinanceApprovalTierResolver` (`BE-FIN-028`); pola maker-checker `FinancePaymentService` | `FinancePurchaseOrderService`, `FinanceGoodsReceiptService`, `FinancePurchaseOrdersController`, `FinanceGoodsReceiptsController`, DTO, registrasi DI; **baru**: `FinanceApprovalAuthorizationService` (role Identity `Supervisor Finance`/`Manajer Finance`, `FinanceApprovalRoleSeeder`) — lihat KNOWN ISSUES laporan | `BE-FIN-028`, `BE-FIN-031` 🟡 | PO tanpa baris ditolak `400`; pengaju = penyetuju ditolak `422`; Supervisor menyetujui PO Rp 62.000.000 ditolak `403`; PO ber-GR tidak dapat dibatalkan; GR melebihi sisa baris ditolak; GR sebagian → PO `PARTIALLY_RECEIVED` | `dotnet build` **(sengaja belum dijalankan)**; `FIN-TEST-1.2` B.1, B.2 (baris PO/GR); verifikasi proses bisnis atas source | Backend Owner. **FIN-VAL-102 sebelumnya TIDAK punya mekanisme penegakan apa pun di codebase mana pun** (termasuk `FinancePaymentService` ✅) — ditutup lewat `AskUserQuestion` 26 September 2026, role Identity baru. `FinancePaymentService` sendiri **belum** disambungkan ke mekanisme yang sama (FIN-VAL-052 tetap belum tertegakkan, gap lama bukan regresi baru) | Route `api/v1/corporate/finance-management/purchasing/...`; `[AccessPermission]` persis `FIN-PERM-1.1` (satu selisih terdokumentasi: `cancel` PO pakai action `Cancel` per `permission-audit-matrix.md`, bukan `Update` seperti tertulis `api-contract.md`); `Serializable` untuk GR (approve PO cukup optimistic concurrency `RowVersion`, konsisten `FinancePaymentService.ApproveAsync`) — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-032.md) |
| 🟡 `BE-FIN-033` | Tukar Faktur tercatat sebagai checkpoint dokumen, dengan estimasi jatuh tempo dari TOP supplier | `FIN-DEC-051`; `FR-FIN-082` | `FIN-API-1.1` B.3; `FIN-STATE-1.2` B.3 | `MstSupplier.PaymentTermDays` | `FinanceInvoiceExchangeService`, `FinanceInvoiceExchangesController`, DTO, registrasi DI | `BE-FIN-031` 🟡 | Tukar Faktur tanpa PO/GR tersimpan; `EstimatedDueDate = ReceivedDate + PaymentTermDays` dihitung backend (nilai dari request diabaikan — `CreateInvoiceExchangeRequest` sengaja tidak punya field ini); Tukar Faktur yang sudah `LINKED_TO_INVOICE` tidak dapat dibatalkan | `dotnet build` **(sengaja belum dijalankan)**; `FIN-TEST-1.2` B.2 | Backend Owner. Selisih `api-contract.md`/`permission-audit-matrix.md` pada action `cancel` (`Update` vs `Cancel`) terjadi lagi — sama seperti `BE-FIN-032`, kini dua kali berturut-turut, direkomendasikan diperbaiki sebelum `BE-FIN-034`/`035` | Nol perhitungan tanggal di luar service — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-033.md) |
| 🟡 `BE-FIN-034` | Purchasing Invoice yang disetujui menjadi utang supplier dan menulis kejadian PPN Masukan, tanpa PPN terkredit dua kali | `FIN-DEC-045`, `046`, `053`, `056`; `FR-FIN-083`, `084`, `086`, `087` | `FIN-API-1.1` B.4, B.9; `FIN-STATE-1.2` B.4; `FIN-VAL-1.2` `105`..`109`, `122`; `FIN-INTEGRATION-1.2` §5.8 | `FinanceSupplierPayableService` (`BE-FIN-019` ✅) sebagai **satu-satunya** pembuat `FinSupplierPayable` — diperluas dua parameter opsional di akhir (`sourcePurchasingInvoiceId`, `accountingEventAmountOverride`), satu-satunya caller lama terverifikasi tidak berubah; `FinanceAccountingOutboxService` (`BE-FIN-011` ✅, menerima kode apa pun yang tidak kosong); `FinanceApprovalAuthorizationService` (`BE-FIN-032` ✅, dipakai ulang apa adanya) | `FinancePurchasingInvoiceService`, `FinancePurchasingInvoicesController`, DTO, registrasi DI; jalur pembuatan utang dari invoice lewat `FinanceSupplierPayableService` dengan `SourcePurchasingInvoiceId` terisi (satu item sintetis `Quantity=1, UnitPrice=TotalAmount` — lihat laporan) | `BE-FIN-028` 🟡, `BE-FIN-033` 🟡 | Invoice kedua dari Tukar Faktur yang sama ditolak unique index (`409`, FIN-VAL-105); total tidak seimbang ditolak `422` (FIN-VAL-107); approve dalam **satu transaksi**: utang tercipta, Tukar Faktur → `LINKED_TO_INVOICE`, outbox `PPN-MASUKAN-PEMBELIAN` sebesar `PPNAmount` berstatus `PENDING` (dilewati bila `PPNAmount = 0`). **Kejadian pengakuan utang yang ditulis `FinanceSupplierPayableService` MUST bernilai `TotalAmount − PPNAmount`** untuk utang bersumber Purchasing Invoice — bila tetap `OriginalAmount` penuh, PPN terkredit dua kali di Utang Supplier | `dotnet build` **(sengaja belum dijalankan)**; `FIN-TEST-1.2` B.3; bukti isi outbox: dua baris untuk satu invoice ber-PPN, jumlah keduanya = `TotalAmount` | Backend Owner. Risiko: menambah parameter nilai kejadian pada `FinanceSupplierPayableService` menyentuh service yang sudah berjalan — jalur input manual MUST tetap menulis `OriginalAmount` penuh seperti sekarang, **terverifikasi lewat pencarian seluruh caller (tepat satu)**. Endpoint `cancel` dan `GET /` daftar sengaja tidak dibangun — lihat laporan | Nol worker pengiriman dibangun (`EPIC FIN-12`); nol penulis `FinSupplierPayable` kedua — 🟡 **Source selesai 26 September 2026, `dotnet build` tertunda milik pengguna**, [laporan](../task/report/backend/BE-FIN-034.md) |
| `BE-FIN-035` | Retur pembelian dicatat, menerbitkan Deposit Retur, dan tercatat di kotak keluar | `FIN-DEC-047`, `061`; `FR-FIN-085` (penerbitan), `FR-FIN-098` | `FIN-API-1.2` (B.5 kecuali `apply` yang dicabut); `FIN-STATE-1.2` B.5, `FIN-STATE-1.3` C.2; `FIN-VAL-1.2` `110`, `111`; `FIN-INTEGRATION-1.3` §5.9 kode 28 | `FinanceAccountingOutboxService` (`BE-FIN-011` ✅) | `FinanceSupplierReturnService` (catat, konfirmasi, batal, daftar deposit) + `FinanceSupplierReturnsController` tanpa endpoint `apply`, DTO, registrasi DI; outbox `RETUR-PEMBELIAN` saat `CONFIRMED` | `BE-FIN-034` | Retur atas invoice non-`APPROVED` ditolak; nilai retur > nilai invoice ditolak; konfirmasi retur menerbitkan deposit `AVAILABLE` **dan** satu baris outbox `RETUR-PEMBELIAN` sebesar `TotalAmount` berstatus `PENDING`, satu transaksi; deposit ber-baris `RESERVED`/`APPLIED` tidak dapat dibatalkan | `dotnet build`; `FIN-TEST-1.2` B.4 (tiga baris pertama); `FIN-TEST-1.3` C.2 baris `FIN-DEC-061` | Backend Owner | Retur **tidak** mengubah `FinSupplierPayable` apa pun; efeknya ke utang lewat pembayaran (`BE-FIN-036`). Nol worker pengiriman |
| `BE-FIN-036` | Deposit Retur dapat dipakai sebagai sumber dana pembayaran supplier, dan tercatat terpisah dari kas | `FIN-DEC-047`, `057`, `061`; `FIN-DES-045`..`047`; `FR-FIN-085` (pemakaian), `FR-FIN-096`, `097` | `FIN-API-1.2` C.1, C.2; `FIN-PERM-1.2` C.1; `FIN-STATE-1.3` C.1-C.3; `FIN-VAL-1.3` C.1-C.2, `FIN-VAL-131`; `FIN-INTEGRATION-1.3` §5.9 kode 29 | `FinancePaymentService` (`BE-FIN-020` ✅) — pola baris potongan (`FinPaymentDeduction`) ditiru persis; `FinanceSupplierReturnService` (`BE-FIN-035`) sebagai satu-satunya penulis `AvailableAmount` | (a) `FinanceSupplierReturnService`: `ReserveAsync`, `ReleaseAsync`, `MarkAppliedAsync` — **ikut** transaksi pemanggil; (b) `FinancePaymentService`: tambah/lepas deposit (hanya `DRAFT`, `Serializable`, kunci `FIN_RETURN_DEPOSIT_{id}`), rumus `NetTransferAmount` dengan `DepositAppliedAmount`, pengecualian `FIN-VAL-091` dan `FIN-VAL-056`, pelepasan saat `REJECTED`/`CANCELLED`, `MarkPaidAsync`: baris → `APPLIED`, `AP_PAYMENT` = `TotalAmount − DepositAppliedAmount` (dilewati bila nol), tulis `PEMAKAIAN-DEPOSIT-RETUR`; (c) `FinancePaymentsController`: `GET`/`POST`/`DELETE /payments/{id}/return-deposits`; `PaymentDetailResponse` diperluas | `BE-FIN-035`, `BE-FIN-041` | Seluruh baris `FIN-TEST-1.3` C.1 dan C.2; **regresi**: pembayaran tanpa deposit menghasilkan `AP_PAYMENT` bernilai `TotalAmount`, identik dengan sebelum task ini | `dotnet build`; `FIN-TEST-1.3` C.1, C.2; review diff membuktikan jalur tanpa deposit tidak berubah | Backend Owner. **MENGUBAH SERVICE YANG SUDAH BERJALAN** (`FinancePaymentService`) di empat titik — setiap perubahan MUST bersyarat `DepositAppliedAmount > 0` atau aditif | Nol penulis `AvailableAmount` di luar `FinanceSupplierReturnService`; nol penulis `OutstandingAmount` baru; nol worker pengiriman |
| `BE-FIN-037` | Empat laporan Purchasing/AP dari data yang sudah ada | `FIN-DES-044`; `FR-FIN-088` | `FIN-API-1.1` B.6 **kecuali** `/aging`; `FIN-PERM-1.1` B.5 | Entity `BE-FIN-029`/`030` — **nol tabel laporan baru** | `FinancePurchasingReportService`, `FinancePurchasingReportsController`: `/summary`, `/invoice-exchanges`, `/due-dates`, `/reconciliation` | `BE-FIN-034`, `BE-FIN-035` | Rekonsiliasi menampilkan Tukar Faktur yang belum menjadi invoice; laporan jatuh tempo memakai `EstimatedDueDate`/`DueDate` dari backend; seluruhnya read-only | `dotnet build`; verifikasi proses bisnis atas source | Backend Owner. **`/aging` sengaja dikeluarkan** — `GET api/finance/payable/aging` (`FinanceApController`) sudah menghitung umur `FinSupplierPayable`, yang sejak `FIN-DEC-045` juga memuat utang dari Purchasing Invoice. Endpoint kedua = dua angka umur utang yang bisa berbeda. **Dicabut dari kontrak** (`FIN-DEC-059`, `/grill-me` 25 September 2026) | Nol perintah pengubah pada controller laporan |
| `BE-FIN-038` | Skema AR Invoice Agregat dan Potongan AR (bentuk revisi 5) tersedia | `FIN-DES-041`, `042`, `048`, `049` | `data-dictionary.md` C.13-C.14 **dan D.3** (`FinReceiptDeduction` bentuk revisi 5 — **bukan** C.15); `FIN-STATE-1.2` B.7 | `FinReceivable.DebtorType`/`DebtorReferenceId` (`FIN-CAP-031`); `FinReceiptAllocation` yang sudah ada sebagai FK tempat potongan melekat | `FinReceivableInvoiceBatch`, `…Item` (submodul `Receivable`), `FinReceiptDeduction` (submodul `Collection`) ber-`DeductionNumber`, `ReceiptAllocationId`, `IsReversal`, `ReversalOfDeductionId` + configuration + `DbSet` + migration `AddArInvoiceBatchAndReceiptDeduction` | — (submodul `Receivable`/`Collection` sudah terdaftar) | Unique index parsial `ReceivableId` aktif; `CK_FinReceivableInvoiceBatch_DebtorType = 'PAYER'`; `CK_FinReceiptDeduction_OtherReason`, `_Reversal`; unique `DeductionNumber`; unique parsial `ReversalOfDeductionId` | `dotnet build`; review configuration terhadap DDL `data-dictionary.md` C.16 (batch) dan D.4(c) (potongan); migration diterapkan | Backend Owner — **otorisasi migration WAJIB terpisah**. **Jangan membangun `FinReceiptDeduction` dari C.15** | Aditif; nol tabel modul lain tersentuh |
| `BE-FIN-039` | Beberapa piutang satu penjamin dapat diterbitkan sebagai satu dokumen tagihan resmi | `FIN-DEC-048`, `054`; `FR-FIN-089`..`092` | `FIN-API-1.1` B.7; `FIN-PERM-1.1` B.5; `FIN-STATE-1.2` B.7; `FIN-VAL-1.2` `114`..`117` | `BillingCompanyGuarantorInvoiceDocumentService` (`FIN-CAP-030`) — **dipanggil**, tidak disalin | `FinanceReceivableInvoiceBatchService`, `FinanceReceivableInvoiceBatchesController`, DTO, registrasi DI | `BE-FIN-038` | Campur dua penjamin ditolak `400`; piutang di batch aktif lain ditolak `409`; batch kosong tidak dapat terbit; dokumen batch memuat rincian per invoice dari layanan Billing; status `PARTIALLY_PAID`/`PAID` mengikuti status `FinReceivable` anggota | `dotnet build`; `FIN-TEST-1.2` B.5 | Backend Owner. **Batas yang MUST dihormati:** service ini MUST NOT menulis `FinReceivable.OutstandingAmount` (DoD #13). Bila menyegarkan status `PARTIALLY_PAID`/`PAID` ternyata menuntut perubahan `FinanceReceivableService`, itu temuan yang dilaporkan balik ke pass desain — pola yang sama dengan bagian 2.1 | Nol PPN/Faktur Pajak (`FIN-DEC-054`) |
| `BE-FIN-040` | Potongan PPh 23 dan biaya admin bank dicatat bersama alokasinya, melunasi piutang sebagai pembayaran non-tunai, dan ikut terbalik bersama alokasinya | `FIN-DEC-049`, `055`, `058`, `062`; `FIN-DES-048`..`050`; `FR-FIN-093`..`095`, `099` | `FIN-API-1.2` C.2, C.3; `FIN-STATE-1.3` C.4; `FIN-VAL-1.3` C.1, C.3; `FIN-INTEGRATION-1.3` §5.9 kode 26, 27 | `FinanceReceivableService.ApplyAllocationAsync`/`ReverseAllocationAsync` **apa adanya** — tidak diubah; `FinanceAccountingOutboxService` (`BE-FIN-011` ✅) | `FinanceReceiptService.AllocateAsync`: terima `deductions[]` per baris alokasi `RECEIVABLE`, buat `FinReceiptDeduction` + panggil `ApplyAllocationAsync` + tulis `POTONGAN-PIUTANG-NON-TUNAI` per potongan; `FinanceReceiptService.ReverseAllocationAsync` (dipakai manual **dan** pembalikan otomatis `FIN-DEC-021`): baris pembalik per potongan + `ReverseAllocationAsync` + `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI`; `GET /receipts/{id}/deductions`; **tanpa** `POST /receipts/{id}/deductions` | `BE-FIN-038` | Seluruh baris `FIN-TEST-1.3` C.3; **regresi**: permintaan alokasi tanpa `deductions` berperilaku persis seperti sebelum task ini | `dotnet build`; `FIN-TEST-1.3` C.3; review diff membuktikan `FinanceReceiptService` tidak menulis kolom `FinReceivable` langsung | Backend Owner. **MENGUBAH SERVICE YANG SUDAH BERJALAN** (`FinanceReceiptService.AllocateAsync`/`ReverseAllocationAsync`) — perubahan aditif, bersyarat ada potongan | Nol kode `AR_PAYMENT`/`PENERIMAAN-PIUTANG`/`PENYESUAIAN-PIUTANG` untuk potongan; nol worker pengiriman |
| `BE-FIN-041` | Pembayaran supplier punya tempat untuk mencatat porsi yang dilunasi deposit | `FIN-DES-045`; `02-backend-architecture.md` D.6, D.7 baris 3 | `data-dictionary.md` D.1, D.4(a) | `FinPayment` + `FinPaymentConfiguration` (`BE-FIN-020` ✅) | Kolom `DepositAppliedAmount` (default 0) pada entity + configuration; `CK_FinPayment_NetTransfer` diganti rumus baru; `CK_FinPayment_DepositApplied`; migration `AddDepositAppliedAmountToFinPayment` | — (tabel `FinPayment` sudah ada) | Seluruh baris `FinPayment` lama tetap lolos constraint baru tanpa backfill; `NetTransferAmount` hitungan service untuk pembayaran lama tidak berubah | `dotnet build`; migration diterapkan di lingkungan pengembangan; pembuktian constraint baru tidak menolak satu pun baris lama | Backend Owner. **TABEL YANG SUDAH BERJALAN** — otorisasi migration WAJIB terpisah, dan `Down()` hanya aman selama belum ada baris ber-`DepositAppliedAmount > 0` | Nol perubahan perilaku sebelum `BE-FIN-036`: kolom ada, nilainya selalu 0 |

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
| Status | 🟡 **SATU task berjalan sebagian, empat TERBLOKIR.** Ditambahkan 25 September 2026 oleh roadmap revisi 2; `BE-FIN-022` dikerjakan pada hari yang sama |
| Blocker | `FIN-OQ-017` — ratifikasi owner Accounting (Rizki) atas tujuh kode kejadian baru. Surat sudah dikirim (`evidence/04`) dan dikoreksi (`evidence/05`) |
| Yang BOLEH jalan | `BE-FIN-022` saja, setelah otorisasi migration turun. Ia hanya menambah nilai `HandoffType` — nama internal Finance, bukan nama kode kejadian |
| **Status `BE-FIN-022`** | 🟡 **SEBAGIAN, 25 September 2026.** Source lengkap (konstanta, configuration, berkas migration `AlterFinBillingHandoffIntakeHandoffTypeCheck` beserta Designer, `ModelSnapshot`); `dotnet build` **`0 Error`, 229 Warning** (seluruhnya `CS1573`/`CS1734` pada berkas tidak berkaitan, sudah ada sebelumnya). **Yang belum terpenuhi:** acceptance criteria pertama — baris ber-`HandoffType` baru belum dapat disimpan karena aturan pemeriksaan nilai **di database masih empat nilai**; eksekusi migration di luar wewenang sesi itu. Bukti: [laporan](../task/report/backend/BE-FIN-022.md) |
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

### `REV-4` — AMENDMENT REVISI 4: Purchasing/AP, AR Invoice Agregat, Potongan AR (`EPIC FIN-15`, `16`, `17`)

| Aspek | Isi |
|---|---|
| Status | Belum dikerjakan. Ditambahkan 25 September 2026 oleh roadmap revisi 3, sesudah owner menyetujui `FIN-DES-037`..`044` dan mengunci enam kontrak turunannya |
| Gelombang | `POST-MVP` (`04-prd-to-mvp.md` bagian 20.1) — **boleh dijadwalkan**, berbeda dari `REV-3` |
| Task yang BOLEH jalan | `BE-FIN-027`..`041` — **seluruh lima belas task**. `BE-FIN-040` dibuka `/grill-me` 25 September 2026; `BE-FIN-036` dibuka revisi 5 blueprint 26 September 2026 |
| Task ⛔ | **Tidak ada.** Yang tetap menunggu pihak luar hanya worker pengiriman kode ke-25 s.d. 29 (`FIN-OQ-020`, `FIN-OQ-026`) — bagian `EPIC FIN-12`, bukan task roadmap ini |
| Tabel baru | Empat belas — sebelas Purchasing, dua AR Invoice Agregat, satu Potongan AR |
| Kolom baru pada tabel lama | Satu: `FinSupplierPayable.SourcePurchasingInvoiceId` |
| Migration | Empat: `AddPurchasingApRumpun`, `AddSourcePurchasingInvoiceIdToSupplierPayable` (`BE-FIN-031`), `AddArInvoiceBatchAndReceiptDeduction` (`BE-FIN-038`), dan `AddDepositAppliedAmountToFinPayment` (`BE-FIN-041`) — yang terakhir **mengubah tabel yang sudah berjalan** (kolom + check constraint), tetap tanpa downtime dan tanpa backfill |
| Yang MENGUBAH kode berjalan | `BE-FIN-028` — batas Rp 50.000.000 tepat pada approval pembayaran; `BE-FIN-034` — `FinanceSupplierPayableService` perlu menerima nilai kejadian pengakuan utang yang berbeda dari `OriginalAmount` untuk utang bersumber Purchasing Invoice |
| Selesai bila | Seluruh baris `FIN-TEST-1.2` B.1-B.5 terbukti; B.4 tiga baris terakhir, B.6, dan B.7 menunggu task ⛔ |

**Empat temuan dari pembacaan source pada `96bf9746`, dan apa yang dilakukan roadmap atasnya.**

| # | Temuan | Bukti | Tindakan roadmap |
|---:|---|---|---|
| 1 | Kode hari ini `<= 50.000.000 → TIER_1`; `FIN-DEC-052` menetapkan `>= 50.000.000 → Manajer` | `FinancePaymentService.cs` baris 654-658 | `BE-FIN-028` mengikuti keputusan; `02-backend-architecture.md` `FIN-DES-039` dikoreksi; baris uji batas ditambahkan ke `FIN-TEST-1.2` B.1 |
| 2 | Pembuatan utang supplier sudah menulis `AP_CREATED` sebesar `OriginalAmount` | `FinanceSupplierPayableService.cs` baris 108-119 | Acceptance criteria `BE-FIN-034`: nilai pokok saja untuk utang bersumber Purchasing Invoice, supaya PPN tidak terkredit dua kali |
| 3 | Pelunasan piutang menulis `AR_PAYMENT` (dibukukan sebagai kas masuk) | `FinanceReceivableService.cs` baris 600-612 | `BE-FIN-040` menulis kode terpisah `POTONGAN-PIUTANG-NON-TUNAI` (`FIN-DEC-058`), bukan `AR_PAYMENT` |
| 4 | `GET api/finance/payable/aging` sudah ada atas `FinSupplierPayable` | `FinanceApController.cs` baris 18, 91 | `/purchasing/reports/aging` dikeluarkan dari `BE-FIN-037`; **dicabut dari kontrak** `FIN-DEC-059` |

**Satu temuan lain yang sengaja TIDAK ditangani roadmap ini**, karena bukan cakupan revisi 4:
kode yang sudah berjalan menulis kode kejadian alias (`AP_CREATED`, `AP_PAYMENT`, `AR_PAYMENT`,
`AR_WRITEOFF`) di samping katalog 17 kode yang diratifikasi Accounting (`PENGAKUAN-HUTANG-SUPPLIER`
dan seterusnya). Katalog ratifikasi tidak memuat keempat alias itu. Dicatat sebagai coverage gap
pada `00-delivery-roadmap.md` bagian 6; `BE-FIN-034` **mengikuti** kode yang dipakai
`FinanceSupplierPayableService` hari ini dan tidak menambah alias baru.

## 5. Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`). Kolomnya sudah disiapkan sehingga skema tidak perlu berubah lagi nanti |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`, diverifikasi ulang belum ada pada `d6cdfaf9`) dan mekanisme kredensial belum final (`FIN-OQ-016`; tiga syarat organisasinya sudah ditetapkan `FIN-DEC-036`). Kotak keluar tetap terisi lewat `BE-FIN-010` |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable`. Tidak pernah dibuat, jadi tidak ada yang perlu dimigrasikan |
| ~~Migration `AddFinanceSubledgerPeriodBalance`~~ | ~~`FIN-OQ-011` belum dijawab~~ — **`FIN-OQ-011` TERTUTUP** 25 September 2026 (`FIN-DEC-035`). Tabel `FinSubledgerPeriodBalance` tetap belum dibuat karena rumpun tutup periode memang `POST-MVP`, **bukan** karena bentuk pesannya belum jelas. Bentuk pesannya kini final di `contracts/integration-contract.md` bagian 5.6 |
| Task pengubah `FIN-API-1.0` dan `FIN-PERM-1.0` | Tujuh kode kejadian baru memakai permission `FinanceAccountingEvent` yang sudah ada dan tidak menambah endpoint. Kedua kontrak itu sengaja tidak disunting pada revisi 3 |
| Task automated test | `rules/backend/TEST_POLICY.md` — backend tidak memelihara project test otomatis. Bukti verifikasi task baru memakai `dotnet build`, review diff/scope, verifikasi kontrak, dan verifikasi proses bisnis atas source |
| Worker pengiriman `PPN-MASUKAN-PEMBELIAN` | Bagian `EPIC FIN-12` (`OPEN DECISION`) dan tetap menunggu `FIN-OQ-020` sesudah `FIN-DEC-056`. Revisi 4 hanya **menulis** barisnya ke kotak keluar |
| `GET /purchasing/reports/aging` | Menduplikasi `GET api/finance/payable/aging` yang sudah ada. **Dicabut dari `FIN-API-1.1` B.6** (`FIN-DEC-059`, `/grill-me` 25 September 2026) — dicatat di sini sebagai jejak, bukan dihapus diam-diam |
| Pemindahan registrasi DI Finance keluar dari `BillingManagementServiceCollectionExtensions.cs` | Utang teknis yang sudah ada (`02-backend-architecture.md` C.6); revisi 4 mengikuti preseden, tidak merapikannya |
| Katalog produk `MstProduct` | Sengaja tidak dibuat (`02-backend-architecture.md` C.11) |

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

| 9 | **`BE-FIN-027` selesai sebelum file model Purchasing pertama** (`QBE-MOD-003`) | ✅ **Sudah** — selesai 26 September 2026. Baris `Corporate / Finance \| FinanceManagement / Purchasing / Pembelian \| Fin \| ACTIVE` terdaftar; `Invoke-QbeConformanceCheck.ps1` `Final result: PASS`. [Laporan](../task/report/backend/BE-FIN-027.md) |
| 10 | **Otorisasi terpisah** untuk membuat **dan** menjalankan tiga migration `REV-4` (`BE-FIN-031`, `BE-FIN-038`) | 🟡 **Sebagian** — pembuatan berkas `BE-FIN-031` (`AddPurchasingApRumpun`, `AddSourcePurchasingInvoiceIdToSupplierPayable`) disetujui dan selesai 26 September 2026, [laporan](../task/report/backend/BE-FIN-031.md). **Eksekusi** migration ini, dan pembuatan **maupun** eksekusi migration `BE-FIN-038` (`AddArInvoiceBatchAndReceiptDeduction`), masih **belum** — diminta terpisah saat masing-masing dimulai. Approval `FIN-DES-037`..`044` **bukan** otorisasi ini |
| 11 | Penguncian kontrak revisi 4 | **Terpenuhi** 25 September 2026 — `FIN-API-1.1`, `FIN-PERM-1.1`, `FIN-STATE-1.2`, `FIN-VAL-1.2`, `FIN-INTEGRATION-1.2`, `FIN-TEST-1.2`, `FIN-MVP-1.3` |
| 12 | Amendment arsitektur menggambar skema `FIN-DEC-057` sebelum `BE-FIN-036` | ✅ **Terpenuhi** 26 September 2026 — `02-backend-architecture.md` AMENDMENT REVISI 5, approved |
| 13 | *(revisi 5)* Otorisasi migration terpisah untuk `AddDepositAppliedAmountToFinPayment` (`BE-FIN-041`) — **tabel yang sudah berjalan** | **Belum** |

Prasyarat 2 dan 3 adalah tindakan manusia yang belum diberikan. Keduanya diminta terpisah,
per-langkah, bukan sekali di awal. Prasyarat 7 dan 8 ditambahkan roadmap revisi 2 dan berlaku
khusus untuk rangkaian `REV-3`. Prasyarat 9-12 ditambahkan roadmap revisi 3 untuk `REV-4`.

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
| 18 | *(revisi 3)* Satu resolver jenjang approval untuk pembayaran, PO, dan Purchasing Invoice — nol salinan ambang kedua |
| 19 | *(revisi 3)* `FinanceSupplierPayableService` tetap satu-satunya pembuat `FinSupplierPayable`, termasuk yang bersumber Purchasing Invoice |
| 20 | *(revisi 3)* Satu Purchasing Invoice ber-PPN menghasilkan kejadian pengakuan utang **dan** kejadian PPN Masukan yang jumlahnya tepat `TotalAmount` — tidak lebih |
| 21 | *(revisi 3)* `FinanceReceivableInvoiceBatchService` tidak pernah menulis `FinReceivable.OutstandingAmount` |
| 22 | *(revisi 5)* Deposit Retur hanya lewat `FinancePaymentService`/`FinanceSupplierReturnService`; `AP_PAYMENT` tidak memuat porsi deposit; pembayaran tanpa deposit identik dengan sebelumnya |
| 23 | *(revisi 4)* `BE-FIN-040` menulis kode kejadian terpisah untuk potongan AR, tidak pernah `AR_PAYMENT` atau `PENYESUAIAN-PIUTANG` |

Kriteria 17 adalah kriteria **selesai**, bukan kelalaian: mengerjakan yang `OPEN DECISION`
lebih awal berarti membangun sesuatu yang jawabannya bisa membatalkan.
