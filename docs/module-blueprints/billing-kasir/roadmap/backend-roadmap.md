# Roadmap Delivery Backend — Billing dan Kasir

## Metadata

```yaml
blueprint_id: BIL-CASH-001
blueprint_revision: 0.4
blueprint_status: approved
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
approved_by: []
source_backend: c99f0a51577456c91831870892870f9ae633b4c2
source_frontend: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
contracts: [BIL-API-0.4, BIL-STATE-0.4, BIL-VALIDATION-0.4, BIL-INTEGRATION-0.4, BIL-PERMISSION-0.4, BIL-TEST-0.4]
```

Seluruh task berstatus `READY_FOR_TASK_APPROVAL` bila dependency-nya terpenuhi. Approval blueprint tidak otomatis mengizinkan builder, migration execution, commit, push, atau deployment.

## `BE-BKC-001` — Fondasi modul dan test harness

| Field | Isi |
| --- | --- |
| Outcome | Tim dapat membangun setiap slice Billing dengan test otomatis dan struktur `Bil*` yang mematuhi QBE |
| Trace | `BIL-CTX-01`–`05`; `BKC-DEC-031`; engineering `QBE-*` |
| Kontrak | Seluruh `0.4`, tanpa endpoint bisnis baru |
| Reuse | `ApplicationDbContext`, `IdentityModel`, `ApiResponse<T>`, pola service/configuration existing |
| Scope | QBE preflight; module service skeleton; project test backend bila belum ada; fixture database terisolasi; registration minimum. Tidak membuat tabel bisnis |
| Dependency | Tidak ada; registry `HealthServices/BillingManagement/Billing → Bil` sudah `ACTIVE` |
| Acceptance | Build lulus; test project ditemukan solution; satu smoke test DI; controller dilarang direct-context pada new code |
| Verifikasi | `dotnet build`; `dotnet test`; structural test prefix/inheritance/configuration/service boundary |
| Risiko/pemilik | Penambahan test project memengaruhi solution. Owner Backend/API; migration/database tidak disentuh |
| DoD | QBE evidence, build/test log, file scope, dan handoff tersedia |

## `BE-BKC-002` — Master biaya administrasi effective-dated

| Field | Isi |
| --- | --- |
| Outcome | Finance/IT dapat mengatur nominal admin rajal/IGD/OTC/ranap, sekali per hari, replacement rajal→ranap, dan coverage tanpa hardcode |
| Trace | `BKC-DEC-001`–`006`; `BIL-CPT-007`; `BIL-AT-009`–`011` |
| Kontrak | API/Validation/Permission `0.4` — Administration Fee Policy |
| Reuse | Billing Master Data route/permission conventions; `IdentityModel` |
| Scope | `MstAdministrationFeePolicy`, configuration/service/DTO/controller, overlap validation, seed minimum, migration source generation |
| Dependency | `BE-BKC-001`; isi nominal seed disahkan Finance sebelum aktif |
| Acceptance | CRUD versi efektif; tidak ada overlap; admin tidak discountable; time boundary Asia/Jakarta; deactivate tidak menghapus histori |
| Verifikasi | Unit/integration test dan migration script review; contoh dua kunjungan pasien fiktif hari sama hanya memperoleh satu fee |
| Risiko/pemilik | Salah timezone menggandakan fee. Owner Finance + Billing + IT |
| DoD | Endpoint, tests, seed, migration file, Swagger; database belum di-update |

## `BE-BKC-003` — Master diskon dan batas approval

| Field | Isi |
| --- | --- |
| Outcome | Promo master dapat dipakai otomatis dan diskon dokter memiliki target/approver yang jelas |
| Trace | `BKC-DEC-007`–`012`; `BIL-CPT-008`; `BIL-AT-012` |
| Kontrak | API/Validation/Permission `0.4` — Discount Policy |
| Reuse | Master Data conventions dan permission existing |
| Scope | `MstDiscountPolicy`, effective dates, target component, limit, approval rule, service/controller/DTO/configuration, migration generation |
| Dependency | `BE-BKC-001` |
| Acceptance | Promo aktif tidak perlu ad-hoc approval; doctor policy hanya doctor share; period overlap ditolak; policy historis immutable |
| Verifikasi | Test promo total/item, doctor share limit, overlap, unauthorized mutation |
| Risiko/pemilik | Policy salah dapat mengurangi porsi RS/penjamin. Owner Finance + Doctor authority |
| DoD | API/test/migration/seed prerequisite terdokumentasi; DB execution tidak dilakukan |

## `BE-BKC-004` — Master pajak dan aturan kamar

| Field | Isi |
| --- | --- |
| Outcome | Pajak dan room charge dihitung dari policy effective-dated, bukan konstanta aplikasi |
| Trace | `BKC-DEC-041`,`043`; `BIL-CPT-021`,`023`; `BIL-AT-021` |
| Kontrak | API/Validation/Permission `0.4` — Tax Rule dan Room Charge Policy |
| Reuse | Master policy pattern hasil `BE-BKC-002` |
| Scope | Dua master, service/controller/DTO/configuration, rounding/period validation, migration generation |
| Dependency | `BE-BKC-001`; policy nilai aktual dari Finance/Inpatient |
| Acceptance | Tax setelah item discount; decimal/rounding konsisten; 24 jam/minimum/remainder/tariff moment configurable; overlap ditolak |
| Verifikasi | Boundary test period, rounding, tanggal efektif, unauthorized update |
| Risiko/pemilik | Kontrak penjamin dapat berbeda. Owner Finance/Tax + Inpatient |
| DoD | Dua API master, tests, migration source, tanpa DB execution |

## `BE-BKC-005` — Running invoice dan charge idempotent

| Field | Isi |
| --- | --- |
| Outcome | Pelayanan klinis masuk ke satu invoice encounter tanpa item ganda ketika producer retry |
| Trace | `BKC-DEC-013`–`018`,`040`; `BIL-CPT-001`,`002`,`010`,`011`; `BIL-AT-001`–`004` |
| Kontrak | API/Integration/Validation `0.4` — invoice/from-source |
| Reuse | Encounter/order/pharmacy IDs; existing category master |
| Scope | `BilInvoice`, `BilInvoiceItem`, config/service/controller/DTO, source adapter, safe invoice-number allocation, migration generation |
| Dependency | `BE-BKC-001`; producer contract tersedia. Farmasi memakai actual dispensed qty |
| Acceptance | Satu invoice/encounter; unique active source tuple; replay no-op; order incomplete mengikuti timing producer; nomor tidak `Count/Max+1` |
| Verifikasi | `BIL-AT-001`–`004`, concurrency dan structural QBE tests |
| Risiko/pemilik | Out-of-order event. Owner Billing + producer domain |
| DoD | Charge dapat dilihat via API; migration source/test/build lulus; tidak dijalankan ke DB |

## `BE-BKC-006` — Kalkulasi finansial berversi

| Field | Isi |
| --- | --- |
| Outcome | Invoice OPEN memperlihatkan gross, admin, tax, primary, excess, dan patient responsibility yang dapat dihitung ulang dengan histori |
| Trace | `BKC-DEC-019`–`023`,`041`–`044`; `BIL-CPT-003`–`005`,`022`; `BIL-AT-009`–`013` |
| Kontrak | API recalculate; Integration pricing/coverage; Validation `0.4` |
| Reuse | Policies `BE-BKC-002`–`004`, tariff/coverage adapters |
| Scope | `BilCalculationVersion`, breakdown snapshot, calculation service, admin fee daily/replacement, coverage waterfall, tax/rounding, migration generation |
| Dependency | `BE-BKC-002`,`004`,`005`; insurer contract adapters |
| Acceptance | Versi immutable; primary→excess→patient; coverage cap; admin once local day; rejected claim tidak auto-shift; recalculation OPEN |
| Verifikasi | `BIL-AT-009`–`013`, decimal/time/concurrency tests |
| Risiko/pemilik | Cross-invoice daily fee membutuhkan query/index benar. Owner Billing/Finance/Insurance |
| DoD | Calculation response dan provenance lengkap; tests/build/migration source lulus |

## `BE-BKC-007` — Penerapan dan approval diskon

| Field | Isi |
| --- | --- |
| Outcome | Kasir/Billing menerapkan promo dan dokter menyetujui pengorbanan share miliknya |
| Trace | `BKC-DEC-007`–`012`; `BIL-CPT-009`; `BIL-AT-012`,`022` |
| Kontrak | API BillingDiscount/BillingDoctorDiscount; Permission `0.4` |
| Reuse | Invoice calculation dan master `BE-BKC-003` |
| Scope | `BilDiscountApplication`, service/controller/DTO/configuration, approval/exception maker-checker, migration generation |
| Dependency | `BE-BKC-003`,`005`,`006` |
| Acceptance | Master promo langsung efektif; doctor discount pending sampai dokter benar approve; RS portion impact meminta Finance; admin fee ditolak |
| Verifikasi | Domain/API/security tests termasuk self/other doctor dan limit |
| Risiko/pemilik | Actor-doctor mapping. Owner Doctor + Finance + Security |
| DoD | Audit before/after tanpa data sensitif; tests/build/migration source lulus |

## `BE-BKC-008` — Void charge dan recalculation aman

| Field | Isi |
| --- | --- |
| Outcome | Orderer dapat membatalkan item eligible tanpa menghapus histori; perubahan harga pada invoice OPEN membentuk versi baru |
| Trace | `BKC-DEC-014`–`018`,`024`; `BIL-AT-003`,`020`,`021` |
| Kontrak | API void/recalculate; State/Validation `0.4` |
| Reuse | Source lifecycle adapter dan calculation service |
| Scope | Void command, source authorization, reason/audit, optimistic concurrency, recalculation trigger |
| Dependency | `BE-BKC-005`,`006` |
| Acceptance | Belum diperiksa/dibayar boleh void; sesudahnya ditolak; final immutable; concurrent change memberi 409 |
| Verifikasi | `BIL-AT-003`,`020`,`021`; log privacy test |
| Risiko/pemilik | Definisi complete berbeda per producer. Owner domain produsen + Billing |
| DoD | Tidak ada delete; API/tests/build lulus; source contract per producer tercatat |

## `BE-BKC-009` — Deposit rawat inap dan top-up

| Field | Isi |
| --- | --- |
| Outcome | Kasir menerima deposit/top-up sebagai dana pasien yang belum dialokasikan |
| Trace | `BKC-DEC-025`–`027`; `BIL-CPT-012`,`013`; `BIL-AT-007`,`008` |
| Kontrak | Patient Funds API/State/Validation `0.4` |
| Reuse | Payment method master; invoice encounter reference |
| Scope | `BilDepositAccount/Movement`, top-up settlement boundary, ledger/reversal, migration generation |
| Dependency | `BE-BKC-001`,`005`; cash top-up juga bergantung shift setelah `BE-BKC-012`, noncash dapat diuji dulu |
| Acceptance | Satu account/ranap; append-only movement; top-up idempotent; saldo tidak negatif; belum memotong invoice otomatis |
| Verifikasi | Ledger/retry/concurrency tests dan `BIL-AT-007`,`008` bagian deposit |
| Risiko/pemilik | Cash sebelum shift. Owner Treasury/Cashier |
| DoD | Deposit API/tests/build/migration source lulus; DB execution terpisah |

## `BE-BKC-010` — Split tender dan payment attempt

| Field | Isi |
| --- | --- |
| Outcome | Kasir menerima kombinasi metode; tender sukses tetap tercatat ketika metode lain gagal |
| Trace | `BKC-DEC-028`–`030`,`036`; `BIL-CPT-014`,`015`; `BIL-AT-005`,`006`,`017` |
| Kontrak | Settlement API/State/Integration `0.4` |
| Reuse | `MstPaymentMethod`, provider adapter, cashier context |
| Scope | `BilSettlement`, `BilTender`, service/provider adapter, idempotency/status reconciliation, migration generation |
| Dependency | `BE-BKC-005`; cash tender path membutuhkan `BE-BKC-012` |
| Acceptance | Partial success; timeout remains PENDING; retry no duplicate; outstanding hanya bagian gagal; late noncash tidak ubah physical cash closed |
| Verifikasi | `BIL-AT-005`,`006`,`017`, callback replay test |
| Risiko/pemilik | Provider status ambiguity. Owner Treasury/Integration |
| DoD | Tender lifecycle/test/audit/build/migration source lulus |

## `BE-BKC-011` — Allocation, progress payment, dan refundable credit

| Field | Isi |
| --- | --- |
| Outcome | Deposit/pembayaran dapat mencicil running invoice tanpa menutupnya dan sisa dana menjadi credit yang dapat dikembalikan |
| Trace | `BKC-DEC-025`–`030`; `BIL-CPT-016`,`017`; `BIL-AT-007`,`008`,`020` |
| Kontrak | Patient Funds API/State/Validation `0.4` |
| Reuse | Deposit `009`, tender `010`, calculation version `006` |
| Scope | `BilPaymentAllocation`, `BilRefundableCredit`, allocation service, concurrency, migration generation |
| Dependency | `BE-BKC-006`,`009`,`010` |
| Acceptance | Allocation ≤ successful/available/outstanding; invoice ranap tetap OPEN; new charge menambah outstanding; excess menjadi credit |
| Verifikasi | `BIL-AT-007`,`008`,`020` dengan contoh deposit Rp8 juta/alokasi Rp5 juta |
| Risiko/pemilik | Allocation ke calculation version stale. Owner Billing/Treasury |
| DoD | Ledger-to-invoice reconciliation dan tests/build/migration source lulus |

## `BE-BKC-012` — Shift kasir dan variance

| Field | Isi |
| --- | --- |
| Outcome | Cash hanya diterima dalam shift aktif; saldo sistem/fisik dan selisih dapat direview tanpa menghapus histori |
| Trace | `BKC-DEC-037`–`039`; `BIL-CPT-027`,`028`; `BIL-AT-016`,`017` |
| Kontrak | Cashier Shift API/State/Permission `0.4` |
| Reuse | Current-user/permission/logging patterns |
| Scope | `BilCashierShift`, `BilCashVarianceReview`, service/controller/DTO/configuration, handover dua aktor, migration generation |
| Dependency | `BE-BKC-001`; integration tender link dengan `BE-BKC-010` |
| Acceptance | Satu active shift; opening/system/physical/variance; close with variance tetap tercatat; review/reopen authorized; handover kedua kasir |
| Verifikasi | `BIL-AT-016`,`017`, permission/concurrency tests |
| Risiko/pemilik | Mapping register/cashier. Owner Kepala Kasir + Security |
| DoD | Shift API/tests/audit/build/migration source lulus |

## `BE-BKC-013` — Refund proporsional

| Field | Isi |
| --- | --- |
| Outcome | Finance memproses refund rajal/OTC eligible melalui metode asal, termasuk kegagalan sebagian |
| Trace | `BKC-DEC-032`,`033`; `BIL-CPT-025`; `BIL-AT-008`,`014` |
| Kontrak | Financial Exceptions API/State/Permission `0.4` |
| Reuse | Tender/provider adapter dan refundable credit |
| Scope | `BilRefundCase`, approval, proportional plan, provider execution/retry, adjustment link, migration generation |
| Dependency | `BE-BKC-010`,`011`; Finance maker-checker |
| Acceptance | Inpatient normal refund rule ditolak; original method proportion; partial execution visible; maker≠approver; reversal entry baru |
| Verifikasi | Domain/API/provider/security tests |
| Risiko/pemilik | Provider tidak mendukung refund otomatis. Owner Finance/Treasury |
| DoD | Case lifecycle/audit/tests/build/migration source lulus |

## `BE-BKC-014` — Write-off dan financial adjustment

| Field | Isi |
| --- | --- |
| Outcome | Finance dapat menyelesaikan atau mengoreksi kewajiban tanpa menyebut write-off sebagai pembayaran |
| Trace | `BKC-DEC-034`,`035`,`042`; `BIL-CPT-006`,`026`; `BIL-AT-014`,`015`,`021` |
| Kontrak | Exceptions API/State/Integration `0.4` |
| Reuse | Calculation/outstanding dan approval pattern `013` |
| Scope | `BilAdjustment`, `BilWriteOffCase`, debit/credit/reversal, correlation id, migration generation |
| Dependency | `BE-BKC-006`; post-final integration selesai bersama `BE-BKC-016` |
| Acceptance | Partial mengurangi saldo; full `SETTLED_BY_WRITE_OFF`; reversal membuka AR; posting lama immutable; self-approve ditolak |
| Verifikasi | `BIL-AT-014`,`015`,`021`,`022` |
| Risiko/pemilik | Salah direction debit/credit. Owner Finance/AR/AP |
| DoD | Ledger effect, approval, audit, tests/build/migration source lulus |

## `BE-BKC-015` — Finalisasi invoice dan departure exception

| Field | Isi |
| --- | --- |
| Outcome | Billing menutup invoice hanya ketika syarat terpenuhi, atau mencatat departure darurat dengan debtor sah |
| Trace | `BKC-DEC-031`,`036`,`044`; `BIL-CPT-018`,`031`; `BIL-AT-018`,`019` |
| Kontrak | Finalization API/State/Validation `0.4` |
| Reuse | Order completion adapters, calculation, settlement, exceptions |
| Scope | `BilFinalizationRecord`, preview/checklist, snapshot lock, settlement outcome, departure reason/debtor evidence, migration generation |
| Dependency | `BE-BKC-006`–`014` sesuai outcome invoice |
| Acceptance | Semua order complete; self-pay lunas atau exception; insured lunas pada patient portion; InvoiceDate tetap; one final effect/version |
| Verifikasi | `BIL-AT-018`,`019`,`023`; failure leaves retryable FINAL state |
| Risiko/pemilik | Debtor legal evidence sensitif. Owner Billing/Finance/Security |
| DoD | Preview/finalize tests/audit/build/migration source lulus |

## `BE-BKC-016` — Handoff AR/AP dan adjustment idempotent

| Field | Isi |
| --- | --- |
| Outcome | Invoice final menghasilkan AR per debtor dan AP dokter sekali saja; AP baru siap dibayar sesuai policy |
| Trace | `BKC-DEC-041`–`044`; `BIL-CPT-019`,`029`,`030`; `BIL-AT-019`,`021`,`023` |
| Kontrak | Integration/Finalization API `0.4` |
| Reuse | AR/AP consumer contracts; outbox pattern terdekat saat implementasi |
| Scope | `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment`, outbox/ack/retry, migration generation |
| Dependency | `BE-BKC-014`,`015`; AR/AP consumer owner mengonfirmasi schema |
| Acceptance | At-least-once safe; AR per debtor/due date; AP created not-ready then ready; correction debit/credit links original; consumer down tidak duplicate |
| Verifikasi | `BIL-AT-019`,`021`,`023`, replay/recovery tests |
| Risiko/pemilik | Consumer AR/AP belum punya endpoint/event final. Owner Integration + AR/AP; task `BLOCKED` bila schema belum disetujui |
| DoD | Contract test, outbox evidence, reconciliation query, build/migration source lulus |

## `BE-BKC-017` — Hardening dan acceptance lintas-slice

| Field | Isi |
| --- | --- |
| Outcome | Seluruh invariants, privacy, concurrency, migration, dan failure recovery memiliki bukti yang dapat diaudit |
| Trace | Seluruh `BKC-DEC-001`–`044`; `BIL-AT-001`–`024` |
| Kontrak | Seluruh `0.4` |
| Reuse | Test harness `001` dan semua slice |
| Scope | Full acceptance suite, authorization negative paths, log scan, migration forward/backward lokal, performance/index checks, sanitized Swagger examples |
| Dependency | `BE-BKC-002`–`016`; otorisasi database lokal terpisah untuk migration execution |
| Acceptance | 24 acceptance IDs memiliki bukti; tidak ada sensitive custom log; no duplicate under retry/concurrency; coverage gaps eksplisit |
| Verifikasi | `dotnet build/test`, contract/integration tests, migration dry-run/local only jika diizinkan |
| Risiko/pemilik | Solution/infrastructure test dan provider sandbox. Owner QA/Backend/Security |
| DoD | Evidence matrix diperbarui; zero critical gap; readiness audit dapat dijalankan |

## Urutan dan paralelisme

Setelah `001`, master `002`–`004` boleh paralel. `005` dapat berjalan paralel dengan master, tetapi `006` menunggu master. `009` dan `012` boleh paralel setelah fondasi invoice; `010` menggabungkannya untuk cash path. Exception `013`/`014` mengikuti settlement, lalu `015`/`016`. Setiap task tetap satu unit builder terpisah.

## Amendment 2 September 2026 — Entri manual katalog tarif + coverage per item

```yaml
input_blueprint_revision: 0.5
input_blueprint_status: approved
approved_by: Product/Domain Owner (2 September 2026 13:53 WIB) — BKC-DEC-062 tanpa konfirmasi terpisah Payer/Insurance+Finance/AR, lihat 00-interview-decisions.md
source_backend_at_design: 17b9c0e21e32b41a8dfd6dbde31462d52717646b
source_frontend_at_design: 60febdcdbb39de6cebc2d825906bce949f3b5af3
contracts: [BIL-API-0.4 (amendment 2 Sep 2026), BIL-VALIDATION-0.4 (amendment), BIL-INTEGRATION-0.4 (amendment), BIL-PERMISSION-0.4 (amendment)]
```

Empat task baru (`BE-BKC-018`–`021`) mengoperasikan `BKC-DEC-059`–`062`. Detail desain lengkap: [`02-backend-architecture.md`](../02-backend-architecture.md#amendment-2-september-2026--entri-manual-berbasis-katalog-tarif--coverage-per-item), [`04-prd-to-mvp.md`](../04-prd-to-mvp.md).

## `BE-BKC-018` — Fondasi katalog tarif pada `BilInvoiceItem`

| Field | Isi |
| --- | --- |
| Outcome | `BilInvoiceItem` dapat menyimpan referensi tarif resmi, dan picker encounter mengekspos konteks unit layanan/klinik/kelas pasien yang dibutuhkan untuk memfilter tarif |
| Trace | `BKC-DEC-059`,`061`; `CAP-02`,`CAP-06` (`01-existing-capability-map.md` § 16); `FR-BKC-001`,`FR-BKC-004` (`04-prd-to-mvp.md`) |
| Kontrak | ERD amendment `BilInvoiceItem.TariffId` (`erd/01-billing-account-charge.md`, `erd/data-dictionary.md`); tidak ada status baru (`contracts/state-transition-matrix.md`) |
| Reuse | `MstTariff`/`MstTariffCategory` existing; pola `SourcePolicies["ADHOC"]` existing sebagai referensi entri baru |
| Scope | Migration `TariffId` (`Guid?`, FK `Restrict`, index) pada `BilInvoiceItem` + configuration; tambah `SourcePolicies["ADHOC_CATALOG"]` pada `BillingChargeSourceAdapter`; extend `ActiveEncounterOptionResponse` (+`ServiceUnitId`,+`ClinicId`,+`PatientClassId`) dan `GetActiveEncounterOptionsAsync`. TIDAK termasuk endpoint charge/preview baru (lihat `BE-BKC-019`/`020`) |
| Dependency | `BE-BKC-001`; tidak bergantung task lain pada slice ini |
| Acceptance | Kolom `TariffId` nullable, FK `Restrict` tervalidasi; `"ADHOC_CATALOG"` diterima `BillingChargeSourceAdapter.ValidateAndNormalize`; `ActiveEncounterOptionResponse` mengembalikan 3 field baru tanpa breaking existing consumer |
| Verifikasi | Migration dry-run/structural test; unit test policy baru; contract test `ActiveEncounterOptionResponse` |
| Risiko/pemilik | FK `Restrict` perlu dipastikan tidak memblokir siklus hidup `MstTariff` yang sudah dipakai. Owner Backend/API |
| DoD | Migration source + configuration + test lulus; build lulus; DB tidak dijalankan |

**Status 3 September 2026**: source, configuration, migration (belum dijalankan), dan test
(policy baru + contract test) selesai; build `dotnet build` lulus (`0 Error`). Lihat
`task/report/backend/BE-BKC-018.md` untuk rincian dan dua kegagalan test pra-eksisting yang
ditemukan (tidak terkait task ini, dikonfirmasi lewat `git stash` baseline).

## `BE-BKC-019` — Endpoint entri charge dari katalog tarif

| Field | Isi |
| --- | --- |
| Outcome | Kasir/penguji menambah item invoice dari `MstTariff` dengan harga sepenuhnya ditentukan server, tidak dapat dimanipulasi client |
| Trace | `BKC-DEC-059`; `FR-BKC-002`,`FR-BKC-003`; `BIL-VAL-025`,`026` |
| Kontrak | API amendment `POST catalog-charges` (`contracts/api-contract.md`); Permission `BillingInvoice:Create` (existing, reuse) |
| Reuse | `BillingInvoiceService.UpsertChargeAsync` (idempotensi/locking/invoice-upsert existing, dipakai penuh); `AddOtherChargeAsync` sebagai referensi struktur method |
| Scope | DTO `AddCatalogChargeRequest`; method `BillingInvoiceService.AddCatalogChargeAsync` (lookup `MstTariff` aktif+efektif, ambil `NormalPrice`+`TariffCategoryId`+`TariffName`, build `UpsertChargeRequest` dengan `SourceDomain="ADHOC_CATALOG"`); action `POST catalog-charges` pada `BillingInvoicesController` |
| Dependency | `BE-BKC-018` |
| Acceptance | `BIL-AT-025`,`026` (`testing/acceptance-test-matrix.md`) |
| Verifikasi | Integration test tarif aktif vs nonaktif/kedaluwarsa; idempotency replay test |
| Risiko/pemilik | Tarif dengan scoping ganda perlu konsisten dengan disambiguasi FE (`BKC-DEC-061`). Owner Billing/API |
| DoD | Endpoint + tests + Swagger sesuai `contracts/api-contract.md`; build lulus; DB tidak disentuh |

**Status 3 September 2026**: source (DTO, method, endpoint) dan 4 test (2 acceptance + 1
idempotency + tercakup dalam test aktif/nonaktif/kedaluwarsa) selesai. Build dikonfirmasi lulus
untuk revisi sebelum tiga penyesuaian terakhir (perbaikan determinisme `SourceDetailId`, perbaikan
tabrakan `SortOrder`, penambahan 4 test) — revisi final **belum diverifikasi ulang**, pengguna
mengambil alih menjalankan build/test secara manual. Belum ditandai selesai; lihat
`task/report/backend/BE-BKC-019.md` § 5 dan § 7.

## `BE-BKC-020` — Endpoint preview coverage per tarif

| Field | Isi |
| --- | --- |
| Outcome | Sistem menjawab "apakah tarif ini tercover untuk pasien ini" sebelum item ditambahkan, tanpa efek samping |
| Trace | `BKC-DEC-060`; `FR-BKC-005`; `CAP-04` |
| Kontrak | API amendment `GET catalog-charges/coverage-preview`; Integration `BIL-INT-010` (in-process, `contracts/integration-contract.md`) |
| Reuse | `InsuranceCoverageService.ResolveTariffAsync` (Clinical Management) dipanggil langsung via DI — TIDAK menulis ulang logika matching rule |
| Scope | DTO `CatalogChargeCoveragePreviewResponse`; method `BillingInvoiceService.GetCatalogChargeCoveragePreviewAsync` (validasi encounter/tarif, panggil `ResolveTariffAsync`, map ke response); constructor injection `InsuranceCoverageService` pada `BillingInvoiceService`; action `GET catalog-charges/coverage-preview` |
| Dependency | `BE-BKC-001`; **independen** dari `BE-BKC-018`/`019` — boleh paralel |
| Acceptance | `BIL-AT-027`,`028`; `BIL-VAL-027` |
| Verifikasi | Domain test rule `Covered`+`IsNeedApproval` tetap dihitung tercover; test encounter/tarif invalid |
| Risiko/pemilik | Response **MUST NOT** membocorkan field internal rule (`RuleCode`, `ApprovalInstruction`). Hasil bersifat advisory — didokumentasikan eksplisit BUKAN angka final (lihat `BE-BKC-021`). Owner Backend/Security |
| DoD | Endpoint read-only tanpa transaksi; tests lulus; build lulus |

**Status 3 September 2026**: source (DTO, constructor injection, method, endpoint) dan 2 test
domain (rule butuh-approval tetap Covered; encounter/tarif tidak valid) selesai ditulis. Build dan
test **sengaja belum dijalankan** oleh sesi ini atas instruksi eksplisit pengguna — pengguna
memverifikasi sendiri secara manual. Belum ditandai selesai; lihat
`task/report/backend/BE-BKC-020.md` § 5 dan § 7.

## `BE-BKC-021` — Penyempitan gating approval pada mesin kalkulasi coverage

| Field | Isi |
| --- | --- |
| Outcome | Item dengan rule coverage berstatus `Covered` tidak lagi jatuh ke `unresolved` hanya karena butuh approval/surat jaminan — Subtotal Asuransi di Menu Pembayaran mencerminkan ini untuk **SEMUA invoice**, bukan cuma dari form testing |
| Trace | `BKC-DEC-062` (approved dengan caveat wewenang — lihat `00-interview-decisions.md`); `FR-BKC-007` |
| Kontrak | Tidak ada endpoint baru; perubahan logika internal `RegistrationBillingCoverageAdapter.ResolveAsync` |
| Reuse | Method existing; hanya kondisi gating yang diubah |
| Scope | Hapus `rule.IsNeedApproval \|\| rule.IsNeedGuaranteeLetter` dari kondisi yang menggeser komponen ke `unresolved`. **PERTAHANKAN** `CoverageStatus=="NeedApproval"` dan `MaxAmountPerMonth`/`MaxQuantityPerMonth` sebagai gate — scope dipersempit sesuai `02-backend-architecture.md`, BUKAN pelepasan gating penuh |
| Dependency | `BE-BKC-001`; independen dari `BE-BKC-018`–`020` (boleh paralel), TAPI dampaknya **GLOBAL** ke semua invoice — regresi-sensitif |
| Acceptance | `BIL-AT-027` (bagian kalkulasi resmi); regresi nol pada `BillingCalculationServiceTests.cs` dan 3 file test lain yang mereferensikan adapter/rule (`01-existing-capability-map.md` § 16.1 `CAP-05`) — `NFR-004` |
| Verifikasi | Full regression run atas test coverage existing + test baru kasus `Covered`+`IsNeedApproval`; review manual bahwa `CoverageStatus=NeedApproval` dan limit bulanan TIDAK ikut berubah |
| Risiko/pemilik | **Risiko tertinggi di slice ini** — mengubah kalkulasi finansial semua invoice produksi, bukan hanya data uji. Owner Billing/Finance/AR. **Wajib dibaca sebelum eksekusi**: `BKC-DEC-062` disetujui Product/Domain Owner TANPA konfirmasi terpisah dari Payer/Insurance + Finance/AR (owner asli `BKC-DEC-042` yang diamendemen) — disarankan menginformasikan mereka sebelum task ini di-deploy ke produksi, meski blueprint tidak mewajibkannya sebagai blocker |
| DoD | Regression evidence eksplisit (bukan cuma "test baru lulus"); before/after behavior terdokumentasi di laporan task; build lulus |

**Status 3 September 2026**: Pengguna dikonfirmasi eksplisit atas catatan risiko di atas sebelum
implementasi dimulai (memilih lanjut implementasi source+test, deploy tetap terpisah). Source
(2 kondisi gating dihapus) dan 4 test baru (regresi + gate yang dipertahankan) selesai ditulis;
analisis regresi statis terhadap 4 file test yang memakai adapter ini menemukan nol test existing
yang terdampak (rincian di laporan). Build/test **sengaja belum dijalankan** sesuai instruksi
pengguna yang berlaku sepanjang sesi ini — evidence regresi nyata (`dotnet test`) dan notifikasi
Finance/AR/Payer sebelum deploy tetap prasyarat wajib. Belum ditandai selesai; lihat
`task/report/backend/BE-BKC-021.md` § 5 dan § 7.
