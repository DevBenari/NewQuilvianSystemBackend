# Finance Management — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-BE-001
parent_roadmap: FIN-ROADMAP-001 revisi 3
roadmap_revision: 1
roadmap_status: ACTIVE
blueprint_id: FIN-BP-001
blueprint_revision: 2
blueprint_status: approved
backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
contracts:
  FIN-API-1.0: locked 2026-09-20
  FIN-STATE-1.0: locked 2026-09-20
  FIN-VAL-1.0: locked 2026-09-20
  FIN-PERM-1.0: locked 2026-09-20
  FIN-INTEGRATION-1.0: locked 2026-09-20 (tiga permukaan dikecualikan)
  FIN-TEST-1.0: locked 2026-09-20
  FIN-MVP-1.0: locked 2026-09-20
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

Dibaca dari atas ke bawah. Task bergaris bawah `BLOCKED` MUST NOT dimulai.

```text
MVP-0   BE-FIN-001 ✅ → BE-FIN-002 🟡 → BE-FIN-003 🟡 → BE-FIN-004 🟡
MVP-1   BE-FIN-005 🟡 → BE-FIN-006 🟡 → BE-FIN-007 🟡 → BE-FIN-008 🟡 → BE-FIN-009 🟡
MVP-5   BE-FIN-010 🟡 → BE-FIN-011 🟡 → BE-FIN-012 🟡      (paralel sejak MVP-1)
MVP-4   BE-FIN-013 🟡 → BE-FIN-014 → BE-FIN-015            (boleh mendahului MVP-2)
MVP-2   BE-FIN-016 → BE-FIN-017                          BLOCKED — owner Billing
MVP-3   BE-FIN-018                                       BLOCKED — turunan MVP-2
POST    BE-FIN-019                                       bebas
POST    BE-FIN-020                                       BLOCKED — FIN-OQ-010
POST    BE-FIN-021                                       BLOCKED — Medical Fee BE-MDF-014
```

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
| 🟡 `BE-FIN-002` | Entity dan EF configuration data induk Finance | `FIN-DES-001`, `003`, `004`, `005` | `FIN-VAL-1.0` §data induk | Pola `MstPettyCashCategoryConfiguration` | `MstBank`, `MstBankAccount`, `MstCurrency`, `MstExchangeRate` + 4 configuration | `BE-FIN-001` | Prefix `Mst`; partial unique index `WHERE "IsDelete" = false`; `HasPrecision(18,2)` | Review konfigurasi + uji unit constraint | Backend Owner | `IdentityModel` diwarisi; nol hard delete |
| 🟡 `BE-FIN-003` | Migration `AddFinanceMasterData` | `FIN-DES-001` | — | — | 4 tabel baru, aditif | `BE-FIN-002` | `Up()` membuat 4 tabel; `Down()` menghapus bersih | Migration dijalankan di lingkungan pengembangan | **Otorisasi terpisah wajib** (`AGENTS.md` Keselamatan Database) | Nol tabel existing tersentuh |
| 🟡 `BE-FIN-004` | API data induk Finance | `FIN-DES-001`, `FR-FIN-001`..`004` | `FIN-API-1.0`, `FIN-PERM-1.0` | `ApiResponse<T>`, `PagedResult<T>`, `[AccessPermission]` | `FinanceMasterDataService` + 3 controller | `BE-FIN-003` | Nomor rekening ganda ditolak; data induk terpakai dinonaktifkan bukan dihapus | `UAT-01`, `UAT-02` | Backend Owner | Route `api/v1/corporate/finance-management/...` hyphenated |
| 🟡 `BE-FIN-005` | Pintu masuk fakta Billing yang idempoten | `FIN-DEC-005` (sisi konsumsi), `FR-FIN-010`..`013` | `FIN-INTEGRATION-1.0` §intake | `BilArHandoff` (`FIN-CAP-001`, `008`) | `FinBillingHandoffIntake` + configuration | `BE-FIN-004` | Fakta sama dua kali → satu piutang; kegagalan tersimpan dan dapat diulang; yang berhasil tidak dapat diulang | `UAT-04` | Backend Owner | `Serializable`; `Idempotency-Key` |
| 🟡 `BE-FIN-006` | Entity buku piutang | `FIN-DES-010`..`013`, `FR-FIN-020`..`024` | `FIN-VAL-1.0` §piutang | — | `FinReceivable`, `FinReceivableItem`, `FinReceivableDocument`, `FinReceivableAdjustment`, `FinReceivableWriteOff` | `BE-FIN-005` | Invariant nilai piutang seimbang terpasang sebagai check constraint | Uji unit invariant | Backend Owner | Prefix `Fin`; `Guid RowVersion` pada aggregate root |
| 🟡 `BE-FIN-007` | Migration `AddFinanceBillingIntake` dan `AddFinanceReceivableAndCollection` | `FIN-DES-001` | — | — | 1 + 7 tabel, aditif | `BE-FIN-006` | Urutan migration 2 lalu 3 sesuai `02-backend-architecture.md` bagian 7 | Migration dijalankan | **Otorisasi terpisah wajib** | Nol tabel existing tersentuh |
| 🟡 `BE-FIN-008` | Layanan piutang: umur, koreksi, penghapusan | `FIN-DES-011`..`013`, `FR-FIN-021`..`023` | `FIN-STATE-1.0`, `FIN-VAL-1.0` | — | `FinanceReceivableService` — satu-satunya penulis `OutstandingAmount` | `BE-FIN-007` | Empat kelompok umur; berkas klaim tidak menahan pengakuan piutang | `UAT-03` | Backend Owner | `Serializable`; satu penulis saja |
| 🟡 `BE-FIN-009` | API intake dan piutang | `FR-FIN-010`..`024` | `FIN-API-1.0`, `FIN-PERM-1.0` | `[AccessPermission]` | `FinanceBillingIntakeController`, `FinanceReceivablesController` | `BE-FIN-008` | Daftar, rincian, umur piutang, penelusuran ke tagihan asal | `UAT-03`, `UAT-04` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-010` 🟡 | Kotak keluar kejadian Accounting | `FIN-DEC-004`, `FIN-DES-017`, `FR-FIN-070`..`075` | `FIN-INTEGRATION-1.0` §outbox, `ACC-XMOD-0.2` | — | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` + migration `AddFinanceAccountingOutbox` | `BE-FIN-007` | Fakta dan kejadian tersimpan atau batal bersama; satu fakta satu kejadian | `UAT-07`, `UAT-17`, `UAT-18` | Backend Owner; **migration butuh otorisasi terpisah** | Data pasien tidak ikut ke muatan kejadian |
| `BE-FIN-011` 🟡 | Penulisan outbox ikut transaksi pemanggil | `FIN-DES-017` | `FIN-INTEGRATION-1.0` | — | `FinanceAccountingOutboxService` | `BE-FIN-010` | Service **tidak** membuka transaksi sendiri; kejadian tertahan tidak terkirim | Uji integrasi rollback, `UAT-19` | Backend Owner | Koreksi memakai versi baru, bukan menimpa |
| `BE-FIN-012` 🟡 | Pantauan kejadian (baca saja) | `FR-FIN-074` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceAccountingEventsController` | `BE-FIN-011` | Kejadian tertahan dan antreannya terlihat | Uji integrasi | Backend Owner | **Tanpa** endpoint pengirim — itu `EPIC FIN-12` |
| `BE-FIN-013` 🟡 | Entity kas dan setoran | `FIN-DES-018`..`020`, `FR-FIN-060`..`065` | `FIN-VAL-1.0` §kas | `BilCashierShift` (`FIN-CAP-006`) | `FinBankDeposit`, `FinDailyCashSnapshot` + migration `AddFinanceCashManagement` | `BE-FIN-009` | Kas kecil tidak memengaruhi kas kasir | Uji unit | Backend Owner; **migration butuh otorisasi terpisah** | Aditif |
| `BE-FIN-014` | Perhitungan kas tersedia dan penutupan harian | `FR-FIN-060`..`065` | `FIN-STATE-1.0` | `BilCashierShift` | `FinanceCashManagementService` | `BE-FIN-013` | Setoran melebihi kas ditolak; setoran sebagian diterima; saldo dihitung saat posting; angka tertutup dibekukan | `UAT-13`..`UAT-16` | Backend Owner — lihat bagian 2.1 | `Serializable` |
| `BE-FIN-015` | API setoran dan kas harian | `FR-FIN-060`..`065` | `FIN-API-1.0`, `FIN-PERM-1.0` | — | `FinanceBankDepositsController`, `FinanceDailyCashController` | `BE-FIN-014` | Penutupan hari menolak setoran belum terposting | `UAT-16` | Backend Owner | Pembungkus `ApiResponse<T>` |
| `BE-FIN-016` | **BLOCKED** — penerimaan dari tender kasir | `FIN-DEC-005`, `FR-FIN-030`..`035` | `FIN-INTEGRATION-1.0` — **permukaan `BilCollectionHandoff` dikecualikan dari penguncian** | `BilTender`, `BilSettlement` (`FIN-CAP-004`, `007`) | `FinReceipt`, `FinReceiptAllocation` | `BE-FIN-009` **dan** konfirmasi owner Billing | Satu tender berhasil → satu penerimaan; nominal disalin apa adanya | `UAT-05`, `UAT-06`, `UAT-20` | **Owner Billing.** Permintaan sudah dikirim lewat `evidence/02-permintaan-kontrak-untuk-owner-billing.md` | — |
| `BE-FIN-017` | **BLOCKED** — pembagian bayar-vs-piutang | `FR-FIN-031`, `FR-FIN-035` | Idem | — | Logika pembagian di `FinanceReceiptService` | `BE-FIN-016` | Pasien lunas tidak melahirkan piutang; dobel-hitung dapat dibuktikan tidak terjadi | `UAT-05`, `UAT-20` | Owner Billing | — |
| `BE-FIN-018` | **BLOCKED** — alokasi, koreksi, penghapusan piutang | `FIN-DES-014`, `FR-FIN-040`..`046` | `FIN-STATE-1.0` §piutang — **terkunci**; yang menahan hanya dependency-nya | — | Alokasi manual, maker-checker, pembalikan | `BE-FIN-017` | Pengaju tidak dapat menyetujui permohonannya sendiri; nilai piutang tidak berubah selama belum diputus; pembalikan tidak menghapus riwayat | `UAT-08`..`UAT-12` | Owner Billing (turunan `MVP-2`) | Maker-checker tiga lapis |
| `BE-FIN-019` | Utang supplier | `FIN-DES-015` (bagian supplier) | `FIN-API-1.0` §payable supplier | `MstSupplier` (`FIN-CAP-014`) | `FinSupplierPayable`, `FinSupplierPayableItem`, `FinPayableAdjustment` | `BE-FIN-009` | Input manual utang dan koreksinya | Uji integrasi | Backend Owner — tidak menunggu siapa pun | `POST-MVP` |
| `BE-FIN-020` | **BLOCKED sebagian** — pembayaran keluar dan potongan | `FIN-DES-015`, `026`, `027`, `028` — **approved** | `FIN-API-1.0`, `FIN-VAL-1.0` §payable — **terkunci** | — | `FinPayment`, `FinPaymentAllocation`, `FinPaymentDeduction`, `NetTransferAmount` | `FIN-OQ-010` — model dan kontraknya sudah bebas | Uang keluar berbeda dari utang lunas; potongan tidak menyisakan utang | `FR-FIN-050`, `FR-FIN-051` | Finance Supervisor + Yasmin — **hanya aturan validasi angkanya** yang tertahan, bukan modelnya | `POST-MVP` |
| `BE-FIN-021` | **BLOCKED** — utang jasa tenaga medis | `FIN-DES-025` — **approved**, `FIN-CAP-021` | `FIN-API-1.0` §payable | — | `FinMedicalServicePayable`, `FinMedicalServicePayableItem` | `BE-FIN-020` **dan** Medical Fee `BE-MDF-014` | Satu penyerahan Medical Fee menghasilkan satu utang; nilai kotor diterima apa adanya | `FR-FIN-075` | **Owner Medical Fee** — `MdfFinanceHandoff` belum ada | — |

## 4. Rincian per gelombang

### `MVP-0` — Fondasi data induk (`EPIC FIN-01`)

| Aspek | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN.** `BE-FIN-001` ✅ selesai 21 September 2026. `BE-FIN-002` 🟡 sebagian 21 September 2026 — `MstBank` **tidak** dibuat baru (sudah ada aktif di `Areas/Administrator/MasterData`, dipakai ulang atas arahan pemilik repository); `MstBankAccount`, `MstCurrency`, `MstExchangeRate` selesai. `BE-FIN-003` 🟡 sebagian 21 September 2026 — file migration untuk 3 tabel dibuat tangan (tanpa `dotnet ef migrations add`, atas instruksi pemilik repository) dan **belum dijalankan**, menunggu `dotnet build` verifikasi pengguna dan otorisasi eksekusi migration terpisah. `BE-FIN-004` 🟡 sebagian 21 September 2026 — `BankAccountsController`/`CurrenciesController` selesai (2 dari 3 controller yang direncanakan; grup `Bank` sengaja tidak dibuat, konsekuensi `BE-FIN-002`), belum dapat diuji end-to-end karena migration belum jalan. Bukti: [BE-FIN-001](../task/report/backend/BE-FIN-001.md), [BE-FIN-002](../task/report/backend/BE-FIN-002.md), [BE-FIN-003](../task/report/backend/BE-FIN-003.md), [BE-FIN-004](../task/report/backend/BE-FIN-004.md) |
| Tabel baru | `MstBankAccount`, `MstCurrency`, `MstExchangeRate` — **bukan** `MstBank` (lihat baris Status; delta terhadap rancangan awal, perlu diratifikasi pemilik blueprint) |
| Migration | `AddFinanceMasterData` — nomor 1 dari 7 |
| Selesai bila | `UAT-01` dan `UAT-02` lulus: data induk siap dipakai, nomor rekening ganda ditolak |
| Yang mudah salah | Memakai prefix `Fin` untuk keempat tabel. `FIN-DES-003` menetapkan **`Mst`** untuk entity yang berperan sebagai data induk, dan itu ketentuan modul, bukan sekadar keputusan satu pass |

### `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`)

| Aspek | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN.** `BE-FIN-005`..`008` 🟡 sebagian — lihat baris masing-masing pada tabel task bagian 3. `BE-FIN-009` 🟡 sebagian 21 September 2026 — `FinanceReceivablesController` selesai penuh; `FinanceBillingIntakeService`+`Controller` dibangun atas otorisasi eksplisit pemilik repository (gap `BE-FIN-005` ditutup di sini) tetapi berdiri di atas beberapa inferensi yang perlu diratifikasi — lihat [laporan BE-FIN-009](../task/report/backend/BE-FIN-009.md) bagian 1. `UAT-03`/`UAT-04` belum dibuktikan runtime |
| Tabel baru | `FinBillingHandoffIntake` + 5 tabel piutang |
| Migration | `AddFinanceBillingIntake`, `AddFinanceReceivableAndCollection` |
| Selesai bila | `UAT-03` dan `UAT-04` lulus |
| Yang mudah salah | Membuat lebih dari satu penulis `OutstandingAmount`. `FIN-DES-011` menetapkan `FinanceReceivableService` sebagai **satu-satunya**; dua penulis akan membuat saldo piutang berbeda tergantung jalur mana yang dipakai |

### `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`)

| Aspek | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN.** `BE-FIN-010` 🟡 sebagian 21 September 2026 — entity, configuration, dan migration `AddFinanceAccountingOutbox` selesai ditulis tangan (atas otorisasi eksplisit pemilik repository), **belum dijalankan**. `FinSubledgerPeriodBalance` sengaja tidak dibuat (bukan Cakupan `BE-FIN-010`, field masih draf menunggu `FIN-OQ-011`). `EventTypeCode` tidak diberi check constraint karena katalog 17 kode (`FIN-DEC-002`) belum diratifikasi Accounting. `BE-FIN-011` 🟡 sebagian 21 September 2026 — `FinanceAccountingOutboxService` selesai, diperluas ke 3 titik pemanggilan nyata (`PENGAKUAN-PIUTANG`, `PENYESUAIAN-PIUTANG`, `PEMUTIHAN-PIUTANG`) atas otorisasi eksplisit; arah DEBIT/CREDIT belum terbawa ke kejadian; QBE `PASS` (47 berkas, 0 pelanggaran). `BE-FIN-012` 🟡 sebagian 21 September 2026 — `FinanceAccountingEventsController` (4 endpoint `GET`, nol endpoint pengirim) dan `FinanceAccountingEventService` selesai; QBE `PASS` (50 berkas, 0 pelanggaran). Ketiga task kodenya selesai; seluruhnya belum dapat diuji end-to-end (migration `BE-FIN-010` belum jalan). Bukti: [BE-FIN-010](../task/report/backend/BE-FIN-010.md), [BE-FIN-011](../task/report/backend/BE-FIN-011.md), [BE-FIN-012](../task/report/backend/BE-FIN-012.md) |
| Tabel baru | `FinAccountingEventOutbox`, `FinAccountingEventAttempt` |
| Migration | `AddFinanceAccountingOutbox` |
| Selesai bila | `UAT-07`, `UAT-17`, `UAT-18`, `UAT-19` lulus |
| Yang mudah salah | `FinanceAccountingOutboxService` membuka transaksi sendiri. `FIN-DES-017` mewajibkan ia **ikut** transaksi pemanggil — kalau tidak, fakta bisa tersimpan tanpa kejadiannya |

### `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`)

| Aspek | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN.** `BE-FIN-013` 🟡 sebagian 21 September 2026 — entity `FinBankDeposit` dan `FinDailyCashSnapshot` selesai beserta EF configuration dan migration `AddFinanceCashManagement` ditulis tangan (belum dijalankan). QBE `PASS`. Bukti: [BE-FIN-013](../task/report/backend/BE-FIN-013.md) |
| Tabel baru | `FinBankDeposit`, `FinDailyCashSnapshot` |
| Migration | `AddFinanceCashManagement` |
| Selesai bila | `UAT-13`..`UAT-16` lulus |
| Yang mudah salah | Menghitung saldo saat layar dibuka, bukan saat posting (`FR-FIN-062`); dan membaca `FinReceipt` yang belum ada — lihat bagian 2.1 |

### `MVP-2`, `MVP-3` — tertahan

Keduanya menunggu owner Billing. Permintaannya sudah dikirim dan berdiri sendiri; tidak ada
pekerjaan backend yang bisa dicicil tanpa jawabannya, karena bentuk `BilCollectionHandoff`
menentukan bentuk `FinReceipt`.

### `POST-MVP`

`BE-FIN-019` bebas dan bisa dikerjakan kapan saja setelah `MVP-1`. `BE-FIN-020` dan
`BE-FIN-021` menunggu, dan keduanya nol baris kode — jadi tidak ada yang perlu dibongkar bila
jawabannya mengubah arah.

## 5. Task yang sengaja tidak dibuat

| Epic | Alasan |
|---|---|
| `EPIC FIN-04` — piutang manfaat karyawan | `OPEN DECISION`; menunggu owner Billing **dan** HR (`FIN-DEC-006`, `FIN-DEC-016`, `FIN-CQ-03`). Kolomnya sudah disiapkan sehingga skema tidak perlu berubah lagi nanti |
| `EPIC FIN-12` — pengiriman kejadian ke Accounting | `OPEN DECISION`; endpoint penerima belum dibangun (`FIN-CAP-018`) dan autentikasi belum final (`FIN-DEC-007`). Kotak keluar tetap terisi lewat `BE-FIN-010` |
| `FinDoctorPayable`, `FinDoctorPayableItem` | Dibatalkan pada revisi 2; digantikan `FinMedicalServicePayable`. Tidak pernah dibuat, jadi tidak ada yang perlu dimigrasikan |
| Migration `AddFinanceSubledgerPeriodBalance` | `FIN-OQ-011` belum dijawab; rumpunnya `POST-MVP` |

## 6. Prasyarat eksekusi

Berlaku untuk **setiap** handoff implementasi backend, tanpa kecuali:

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik — bukan dari roadmap ini | Berlaku terus |
| 2 | `BE-FIN-001` selesai sebelum file model pertama ditulis | ✅ **Sudah** — selesai 21 September 2026. Enam baris submodul (`BillingIntake`, `Receivable`, `Collection`, `Payable`, `CashManagement`, `AccountingIntegration`) terdaftar `Fin`/`ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `Invoke-QbeConformanceCheck.ps1` `Final result: PASS`. Bukti: [laporan](../task/report/backend/BE-FIN-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan setiap migration | 🟡 **Sebagian** — otorisasi pembuatan file `AddFinanceMasterData` diberikan pemilik repository 21 September 2026 (lihat [laporan BE-FIN-003](../task/report/backend/BE-FIN-003.md)); otorisasi **eksekusi** (`dotnet ef database update`) masih **Belum** |
| 4 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` setelah task-nya disetujui | Berlaku terus |
| 5 | Task `BLOCKED` MUST NOT dimulai, walau sebagian pekerjaannya terlihat berdiri sendiri | Berlaku terus |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |

Prasyarat 2 dan 3 adalah tindakan manusia yang belum diberikan. Keduanya diminta terpisah,
per-langkah, bukan sekali di awal.

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
