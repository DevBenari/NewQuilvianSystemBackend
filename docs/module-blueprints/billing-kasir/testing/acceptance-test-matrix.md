# Billing dan Kasir — Acceptance Test Matrix

`contract_version: BIL-TEST-0.4` · status **approved** · owner QA + Product/Billing/Finance/Security · approved 20 Agustus 2026. Test data wajib fiktif dan tidak memakai data pasien produksi.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-001` | Satu invoice/encounter | Dua charge source masuk untuk encounter sama | Integration | Satu invoice, dua item |
| `BIL-AT-002` | Source idempotent | Event yang sama dikirim 3 kali | Integration/concurrency | Satu item aktif; response replay konsisten |
| `BIL-AT-003` | Void rule | Void sebelum complete berhasil; sesudah complete ditolak | Domain/API | Histori void dan pesan `BIL-VAL-003` |
| `BIL-AT-004` | Pharmacy actual qty | Order 10, diserahkan 7 | Contract | Charge qty 7, bukan 10 |
| `BIL-AT-005` | Split tender | Tunai 300 ribu sukses, QRIS 700 ribu gagal | E2E | Tunai tetap tercatat; outstanding 700 ribu |
| `BIL-AT-006` | Provider timeout | Callback terlambat | Integration | Tender PENDING; retry tidak menggandakan charge |
| `BIL-AT-007` | Ranap progress | Deposit 8 juta, allocation 5 juta, charge bertambah | E2E | Invoice tetap OPEN; ledger immutable; saldo recalculated |
| `BIL-AT-008` | Deposit release | Final bill di bawah saldo deposit | Domain | Refundable credit terbentuk; bukan auto cash out |
| `BIL-AT-009` | Admin fee harian | Dua encounter rajal pasien sama pada tanggal Jakarta sama | Integration | Fee hanya invoice pertama |
| `BIL-AT-010` | Transfer rajal→ranap | Fee rajal telah dihitung lalu transfer | Domain | Rajal diganti ranap melalui version/adjustment, tidak dobel |
| `BIL-AT-011` | Admin fee rules | Coba diskon admin; insurer cover flag true | Domain | Diskon ditolak; coverage mengikuti policy |
| `BIL-AT-012` | Discount | Promo master otomatis; doctor discount perlu doctor approval | API/security | Promo efektif; doctor share pending lalu approved oleh dokter benar |
| `BIL-AT-013` | Insurance waterfall | Primary sebagian, excess residual, sisanya pasien | Domain | Total coverage ≤ eligible; AR per debtor benar |
| `BIL-AT-014` | Write-off | Partial dan full; maker mencoba self-approve | API/security | Self-approve 403/422; full outcome SETTLED_BY_WRITE_OFF |
| `BIL-AT-015` | Reversal | Reverse write-off posted | Domain/integration | Entry baru membuka AR; histori lama tetap |
| `BIL-AT-016` | Shift variance | Physical cash berbeda lalu close/review/reopen | E2E | Variance persisted; audit authority lengkap |
| `BIL-AT-017` | Late noncash | QRIS settle setelah shift closed | Integration | Tender asal berubah; physical shift tidak berubah |
| `BIL-AT-018` | Departure exception | Death partial family payment dan DAMA unpaid | Domain | Departure boleh; AR ke family/lawful debtor; bukan PAID |
| `BIL-AT-019` | Final AR/AP | Finalisasi insured invoice dengan doctor share | Integration | AR dan AP handoff idempotent; AP not-ready lalu ready by policy |
| `BIL-AT-020` | Optimistic concurrency | Dua user recalculate/allocate versi sama | Concurrency | Satu sukses; satu `409`; tak ada lost update |
| `BIL-AT-021` | Post-final correction | Harga berkurang setelah final | Integration | Credit adjustment/refundable credit dan AR/AP correction |
| `BIL-AT-022` | Authorization | Kasir coba approve write-off/reopen shift | Security | `403`, tidak ada mutation, denied access evidence |
| `BIL-AT-023` | Failure recovery | AR consumer down saat final | Resilience | Invoice FINAL, outbox retry, satu AR saat pulih |
| `BIL-AT-024` | Privacy/a11y | Scan logs/UI keyboard/status | Security/UI | Tak ada field sensitif; status tidak hanya warna; fokus/label valid |
| `BIL-AT-025` (**baru, approved**) | Harga katalog server-side (`BKC-DEC-059`) | Kasir pilih tarif Rp150.000 di dropdown; submit tanpa field harga sama sekali | API/domain | `BilInvoiceItem.UnitPrice` = `MstTariff.NormalPrice` persis; `TariffId` terisi; `SourceDomain="ADHOC_CATALOG"` |
| `BIL-AT-026` (**baru, approved**) | Tolak tarif tidak aktif/kedaluwarsa | `TariffId` valid tapi `IsActive=false` atau di luar `EffectiveEndDate` | API | `422` dengan `BIL-VAL-025`; tidak ada `BilInvoiceItem` tersimpan |
| `BIL-AT-027` (**baru, approved**) | Preview coverage 3 status, approval tidak menggagalkan (`BKC-DEC-060`,`062`) | Tarif dengan rule `CoverageStatus=Covered, IsNeedApproval=true` untuk pasien asuransi | Domain | Preview mengembalikan `CoveredAmount`/`PatientPayAmount` terhitung penuh, `IsNeedApproval=true` hanya sebagai info — BUKAN status `NotCovered`/gagal |
| `BIL-AT-028` (**baru, approved**) | Disparitas preview vs kalkulasi final terdokumentasi (§ 16.2.A) | Tarif tanpa baris `MstInsuranceTariff` (preview → `NotCovered`) tapi ada `MstInsuranceCoverageRule` yang cocok (kalkulasi final `RegistrationBillingCoverageAdapter` → berpotensi coverable) | Domain/dokumentasi | Kedua angka BOLEH berbeda; UI menampilkan disclaimer preview bersifat perkiraan; angka final Menu Pembayaran tetap dari `RegistrationBillingCoverageAdapter`, tidak pernah dari preview |

## Exit evidence

Setiap slice harus menyertakan test command dan hasil, request/response tersanitasi, database assertion untuk unique/idempotency, audit assertion, serta screenshot hanya untuk behavior UI yang relevan. Uji nominal memakai decimal boundary/rounding, waktu melewati tengah malam Asia/Jakarta, effective-date boundary, retry, out-of-order event, dan unauthorized paths. Approval blueprint bukan bukti test; build tetap belum dimulai.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-TEST-0.5` · status **draft** · owner QA + Product/Billing/Finance/Security · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009`. Test data wajib fiktif dan **MUST NOT** memakai data pasien produksi; nama perusahaan asuransi pada test memakai nama samaran.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-029` (**baru, draft**) | Pecahan rupiah per baris terekspos dan menjumlah (`BKC-DEC-069`, `BKC-DES-001`, `BIL-VAL-028`) | Pasien asuransi. Item A Rp 100.000 (aturan `Covered` 100%), item B Rp 300.000 (aturan `Covered` 80%), item C Rp 25.000 (tanpa aturan cocok), biaya administrasi Rp 15.000 (`Coverable=true`, aturan `Covered` 100%) | Domain (unit) | `breakdown.items[A].coveredAmount = 100000`; `[B] = 240000`; `[C] = 0`; `administrationFee.coveredAmount = 15000`. Jumlah keempatnya = `coverage.primaryAmount = 355000`. `coverage.isPerItemAllocationAvailable = true` |
| `BIL-AT-030` (**baru, draft**) | Kunci alokasi tidak tertukar antar baris pajak (`BKC-DES-002`) | Aturan pajak aktif, dua item dengan nominal berbeda (Rp 100.000 dan Rp 400.000), keduanya coverable dan tercover penuh | Domain (unit) | Porsi pajak yang tercover menempel pada baris item **masing-masing** sesuai proporsi nominalnya. **Jalur gagal yang wajib diuji**: bila alokasi dikunci memakai `ComponentId`, kedua baris pajak akan bertumpuk pada satu entri karena `TaxRuleId`-nya sama — test **MUST** gagal bila implementasi memakai `ComponentId` sebagai kunci |
| `BIL-AT-031` (**baru, draft**) | Dokumen hanya memuat baris yang ditanggung asuransi (`BKC-DEC-068`) | Invoice sama seperti `BIL-AT-029`, lalu `GET {id}/insurance-invoice-document` | API/integration | Response memuat **tiga** baris `items` (A, B, biaya administrasi). Item C **tidak ada** di `items`, meskipun ada di `GET {id}` dan di Struk Pasien. `totals.totalCoveredAmount = 355000` |
| `BIL-AT-032` (**baru, draft**) | Blok perusahaan asuransi berasal dari `MstInsuranceProvider`, bukan penjamin perusahaan (`BKC-DEC-067`) | Kunjungan pasien asuransi dengan `InsuranceProviderId` menunjuk perusahaan samaran "Asuransi Sejahtera Nusantara" (`ContractNumber`, `OfficeAddress` terisi) | API/integration | `payer.insuranceProviderName`, `payer.contractNumber`, dan `payer.officeAddress` terisi dari `MstInsuranceProvider`. Field polis (`policyNumber`, `memberNumber`, `planName`) terisi dari kolom snapshot `TrxPatientEncounterGuarantor`, **bukan** dari `MstPatientInsurance` terkini (`BKC-DES-009`) |
| `BIL-AT-033` (**baru, draft**) | Kunjungan bukan-asuransi dijawab sebagai keadaan wajar, bukan galat (`BKC-DES-008`, `BIL-VAL-029`/`030`/`031`) | Tiga permintaan: kunjungan tunai; kunjungan penjamin perusahaan; kunjungan tanpa baris penjamin sama sekali | API | Ketiganya `200`. `payerKind` berturut-turut `CASH`, `COMPANY_GUARANTOR`, `UNKNOWN`; ketiganya `isPrintable=false` dengan satu pesan `warnings` yang sesuai. **Jalur gagal yang wajib diuji**: tidak boleh ada yang mengembalikan `422`/`404`, dan `items` ketiganya kosong |
| `BIL-AT-034` (**baru, draft**) | Invoice terfinalisasi dengan snapshot lama jujur menyatakan keterbatasannya (`BKC-DES-004`, `BIL-VAL-033`) | `BilCalculationVersion` disiapkan dengan `BreakdownSnapshot` JSON **tanpa** properti `isPerItemAllocationAvailable` (meniru data yang lahir pada `BIL-CALCULATION-0.4`), invoice berstatus `FINAL` | Integration | `200` dengan `isFromLockedSnapshot=true`, `isPerItemBreakdownAvailable=false`, `items` kosong, `totals.primaryAmount` terisi dari kolom relasional, `isPrintable=false`, dan `warnings` memuat pesan `BIL-VAL-033`. **MUST NOT** mengembalikan `422` dan **MUST NOT** menampilkan rincian Rp 0 seolah-olah itu angka sungguhan |
| `BIL-AT-035` (**baru, draft**) | Dokumen tidak membocorkan isi kesepakatan asuransi maupun nomor kartu | Invoice pasien asuransi dengan aturan coverage yang `RuleCode`, `ApprovalInstruction`, dan `BillingInstruction`-nya terisi, serta `CardNumberSnapshot` terisi | Security | Response JSON **tidak memuat** `ruleCode`, `ruleName`, `approvalInstruction`, `billingInstruction`, `cardNumber`, `picName`, `picPhoneNumber`, `picEmail` — diperiksa dengan pencarian teks pada seluruh payload, bukan hanya pada field yang diperiksa satu per satu. Log aplikasi selama permintaan itu tidak memuat nama pasien, nomor rekam medis, maupun nomor polis |

### Regresi yang wajib diperiksa

| Yang diperiksa | Alasan |
| --- | --- |
| Seluruh test coverage yang sudah ada tetap lulus dengan nominal yang **sama persis** | Perubahan pada `ResolveAsync` hanya menambah pencatatan alokasi. Bila satu saja nilai `primaryAmount`/`unresolvedAmount` pada test existing berubah, itu berarti formula ikut tersentuh — yang **MUST NOT** terjadi. Titik uji yang paling langsung: `QuilvianSystemBackend.Tests/BillingManagement/BillingCalculationServiceTests.cs` (`CoverageWaterfallAppliesPrimaryThenExcessThenPatient`) dan `BillingArApHandoffServiceTests.cs` (`InsuredInvoiceCreatesPayerArHandoffAndKeepsApNotReady`) |
| Snapshot yang dihasilkan versi baru masih dapat dibaca kode versi lama | Jaminan rollback tanpa langkah mundur basis data (`02-backend-architecture.md` § Rencana migration butir 4) |
| Cetak Kwitansi dan Struk Pasien tetap menghasilkan PDF A5 | Perubahan `buildPdf` menambah parameter opsional dengan bawaan `"a5"`; regresi di sini berarti dua dokumen yang sudah dipakai kasir ikut rusak |
| Menu Pembayaran tetap menampilkan angka yang sama seperti sebelum amendment | Field baru bersifat aditif; tidak ada baris tampilan existing yang boleh berubah nilainya pada slice ini |

### Bukti keluar tambahan

Selain bukti keluar yang sudah berlaku, slice ini **MUST** menyertakan: satu contoh response `insurance-invoice-document` yang sudah disanitasi (nama dan nomor diganti data samaran) untuk masing-masing dari empat keadaan `payerKind`, hasil pemeriksaan teks yang membuktikan tidak ada field terlarang pada payload (`BIL-AT-035`), dan satu berkas PDF hasil cetak yang memperlihatkan seluruh kolom tabel terbaca utuh pada kertas A4. Approval blueprint bukan bukti test.
