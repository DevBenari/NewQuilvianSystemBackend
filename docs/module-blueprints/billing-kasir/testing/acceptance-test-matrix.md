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

---

## Amendment 4 September 2026 — Pembagian tanggungan, anomali data, dan gerbang PPN

`last_changed_in: BIL-TEST-0.6` · status **draft** · owner QA + Product/Billing/Finance/Security · `approved_by`/`approved_at`: belum ada. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Data uji **MUST** fiktif; **MUST NOT** memakai data pasien produksi.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-036` | `BKC-DEC-071` | Item dengan aturan `Covered` 100% yang menandai `IsNeedApproval = true` | Domain/Integration | Seluruh nominal masuk `primaryAmount`; `unresolvedAmount` nol; badge baris "Penjamin" |
| `BIL-AT-037` | `BKC-DEC-071` | Item dengan aturan `Covered` 80% yang mengisi `MaxAmountPerMonth = 500000` | Domain/Integration | 80% masuk `primaryAmount`, 20% ke pasien; `unresolvedAmount` nol. **Jalur gagal yang harus TIDAK terjadi**: nominal tertahan seluruhnya seperti perilaku lama |
| `BIL-AT-038` | `BKC-DEC-072` | Item yang tidak punya satu pun aturan yang cocok | Domain/Integration | Seluruh nominal menjadi porsi pasien; `unresolvedAmount` nol; `dataAnomalyAmount` nol; badge "Tunai" |
| `BIL-AT-039` | `BKC-DEC-072` | Item dengan aturan eksplisit `CoverageStatus = "NotCovered"` dan `IsAllowExcessPaymentByPatient = true` | Domain | Seluruh nominal menjadi porsi pasien; badge "Tunai" |
| `BIL-AT-040` (**dikoreksi 4 Sep 2026**) | `BKC-DEC-074`, ~~`BKC-DES-013`~~ **`BKC-DEC-080`**, `BKC-DES-021` | Item dengan aturan `Covered` 70% dan `IsAllowExcessPaymentByPatient = false` | Domain | 70% ke `primaryAmount`; 30% ke **`nonBillableResidualAmount`** — **bukan** ke `unresolvedAmount`, yang wajib nol pada skenario ini; porsi pasien nol. Layar kasir tetap menampilkan baris "Selisih Tidak Ditagihkan" berisi 30% itu, karena baris tersebut menjumlah kedua field |
| `BIL-AT-041` | `BKC-DEC-073`, `BKC-DES-011` | Kunjungan asuransi dengan `IsEligible = false`, biaya coverable Rp 440.000 | Integration | Perhitungan **berhasil** (`200`); `dataAnomalyAmount = 440000`; `anomalyCodes = ["PAYER_NOT_ELIGIBLE"]`; seluruh nominal jatuh ke porsi pasien; `primaryAmount` nol |
| `BIL-AT-042` | `BKC-DEC-073` | Kunjungan asuransi dengan `IsPolicyActive = false` | Integration | Kode `POLICY_INACTIVE`; perilaku nominal sama dengan `BIL-AT-041` |
| `BIL-AT-043` | `BKC-DES-012`, `BIL-VAL-036` | Jalur `REJECTED` dipaksa terjadi tanpa `dataAnomalyAmount` terisi (uji negatif, disimulasikan) | Unit/Domain | `422` beserta pesan "Coverage yang ditolak tidak boleh otomatis dipindahkan ke pasien tanpa policy kontrak." Versi kalkulasi baru **tidak** dibuat |
| `BIL-AT-044` | `BKC-DEC-078` | Invoice rawat inap (`ServiceType = "RANAP"`) berisi obat Rp 1.000.000 dan kamar Rp 2.000.000, tarif PPN aktif 11% | Integration | `taxes` kosong; `taxAmount` setiap item nol; total Rp 3.000.000 — **bukan** Rp 3.110.000 |
| `BIL-AT-045` | `BKC-DEC-078` | Invoice rawat jalan (`ServiceType = "RAJAL"`) berisi obat Rp 1.000.000 yang sama | Integration | PPN Rp 110.000 dikenakan; total Rp 1.110.000 |
| `BIL-AT-046` | `BKC-DEC-079` | Invoice IGD (`ServiceType = "IGD"`) berisi obat yang sama | Integration | PPN Rp 110.000 dikenakan — sama seperti rawat jalan, **bukan** dibebaskan |
| `BIL-AT-047` | `BKC-DEC-077` | Invoice rawat jalan pasien asuransi, obat ditanggung 100%, `MstTaxRule.AllocationRule = "PROPORTIONAL"` | Integration | `taxPrimaryAmount` item obat itu sama dengan seluruh nilai PPN-nya; Pajak Asuransi Rp 110.000, Pajak Mandiri Rp 0 |
| `BIL-AT-048` | `BKC-DEC-077` | Invoice rawat jalan pasien asuransi, obat **tidak** ditanggung (`NotCovered`), `AllocationRule = "PROPORTIONAL"` | Integration | `taxPrimaryAmount` nol; seluruh PPN menjadi porsi pasien. **Jalur gagal yang harus TIDAK terjadi**: PPN ikut ke asuransi (perilaku `GUARANTOR`) |
| `BIL-AT-049` | `BKC-DES-016` | Invoice yang membentuk komponen pajak biaya administrasi **dan** komponen pajak biaya kamar sekaligus, keduanya tanpa `PolicyId` | Unit | Perhitungan tidak melempar `ArgumentException` kunci ganda. Uji ini **MUST** dijalankan dengan basis pajak diperluas secara paksa, karena pada konfigurasi berjalan keduanya tidak pernah terbentuk |
| `BIL-AT-050` | `BKC-DES-017` | Membaca versi kalkulasi yang ditulis sebelum pembaruan (snapshot tanpa field baru) | Unit | Deserialisasi berhasil; `isPerItemAllocationAvailable` bernilai `false`; seluruh field baru bernilai nol tanpa galat |
| `BIL-AT-051` | `BKC-DES-019` | Invoice dengan `ServiceType = "MCU"` berisi obat | Integration | PPN **dikenakan** (daftar bebas pajak hanya memuat `"RANAP"`). Hasil uji ini **MUST** dilampirkan pada `BKC-OQ-083` sebagai bahan keputusan pemilik produk |
| `BIL-AT-052` | `BIL-VAL-028` | Invoice dengan tiga item tercover dan biaya administrasi tercover | Integration | Jumlah `itemPrimaryAmount + taxPrimaryAmount` seluruh baris ditambah `primaryAmount` biaya administrasi sama persis dengan `coverage.primaryAmount` |
| `BIL-AT-053` | `BKC-DEC-075` | Membuka Menu Pembayaran untuk invoice asuransi yang seluruh datanya normal | E2E | Baris "Penjamin Belum Terverifikasi" **tidak ada** di markup. Subtotal Mandiri + Subtotal Asuransi + Pajak Mandiri + Pajak Asuransi menjumlah persis ke Total Tagihan |
| `BIL-AT-054` | `BKC-DEC-073` | Membuka Menu Pembayaran untuk invoice beranomali | E2E | Peringatan kuning tampil di atas Ringkasan Pembayaran; tombol pembayaran **tetap aktif**; pembayaran dapat diselesaikan sampai tuntas |

### Regresi yang wajib diperiksa

| Yang diperiksa | Kenapa berisiko |
| --- | --- |
| Total tagihan pasien tunai tidak berubah sama sekali | Jalur `SelfPay()` tidak disentuh amendment ini, tetapi ketiga titik pembentukan `BillingCoverageDecision` berubah bersamaan — kesalahan urutan argumen pada `record` posisional akan terlihat justru di jalur yang paling sering dipakai |
| Kwitansi dan Struk Pasien tetap mencetak angka yang sama untuk invoice `FINAL` yang sudah ada | Snapshot terkunci tidak boleh ikut berubah. Bila angkanya bergeser, berarti ada yang menghitung ulang invoice yang seharusnya tidak dihitung ulang |
| Invoice rawat inap yang sudah `FINAL` tetap memuat PPN lamanya | Pembebasan PPN berlaku ke depan, bukan surut |
| Badge per baris pada invoice pasien tunai tetap "Tunai" untuk semua baris | Jalur `SelfPay()` mengembalikan daftar outcome kosong; layar **MUST** menafsirkan kekosongan itu sebagai "seluruhnya pasien", bukan sebagai "data belum termuat" |
| Cap `MaxAmountPerVisit` dan `MaxQuantityPerVisit` masih berlaku | `BKC-DEC-071` mencabut limit **bulanan** saja. Mencabut limit per kunjungan sekalian adalah kesalahan yang mudah terjadi karena keduanya bertetangga di kode |
| Galat "lebih dari satu tax rule aktif" masih muncul pada invoice rawat inap | Gerbang PPN ditempatkan di `ApplyInvoiceTax`, bukan di `LoadInvoiceTaxRuleAsync`, justru supaya salah konfigurasi tetap terdeteksi pada kunjungan yang pajaknya dibebaskan |

### Bukti keluar tambahan

Slice ini **MUST** menyertakan: (1) hasil `dotnet build` yang benar-benar dijalankan — dua task ad-hoc yang mendahului amendment ini (`BE-BKC-FIX-003`, `FE-BKC-FIX-008`) berstatus `AUTOMATED TEST: BLOCKED` dan **belum pernah dibangun sekalipun**, sehingga slice ini tidak boleh dinyatakan selesai tanpa build yang lulus; (2) satu tangkapan layar Menu Pembayaran untuk invoice beranomali yang sudah disanitasi; (3) perbandingan angka sebelum dan sesudah untuk satu invoice rawat inap berisi obat, memperlihatkan PPN yang hilang beserta nominalnya; (4) hasil pemeriksaan nilai `MstTaxRule.AllocationRule` yang aktif di lingkungan uji. Approval blueprint bukan bukti test.

---

## Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off

`last_changed_in: BIL-TEST-0.7` · status **draft** · owner QA + Product/Billing/Finance/Security · `approved_by`/`approved_at`: belum ada. Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`. Data uji **MUST** fiktif; **MUST NOT** memakai data pasien produksi.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-055` | `BKC-DEC-080`, `BKC-DES-021`, `BKC-DES-022` | Tindakan Rp 100.000 dengan aturan `Covered` 70% dan `IsAllowExcessPaymentByPatient = false` | Domain/Integration | `primaryAmount = 70000`; `nonBillableResidualAmount = 30000`; `unresolvedAmount = 0`; `patientAmount = 0`; Total Tagihan tetap Rp 100.000. **Jalur gagal yang harus TIDAK terjadi**: Rp 30.000 masuk `unresolvedAmount` (perilaku revisi `0.7`) atau masuk porsi pasien |
| `BIL-AT-056` | `BKC-DEC-070`, `BKC-DES-022` | Tindakan Rp 100.000 dengan aturan `Covered` 70% dan `IsAllowExcessPaymentByPatient = true` | Domain | `primaryAmount = 70000`; `patientAmount = 30000`; `nonBillableResidualAmount = 0`. Uji pasangan untuk `BIL-AT-055`: membuktikan cabang `true` **tidak** ikut berpindah |
| `BIL-AT-057` | `BKC-DES-023` | Membuka `GET .../calculation-preview` sepuluh kali berturut-turut pada tagihan yang memuat residual non-billable | Integration | Jumlah baris `BilWriteOffCase` untuk invoice itu **tetap nol**. Ini uji negatif inti `BKC-DES-023`: mesin kalkulasi tidak boleh melahirkan kasus write-off, sebanyak apa pun layar dibuka |
| `BIL-AT-058` | `BKC-DEC-080`, `BKC-DES-024`, `BIL-VAL-040` | Tagihan dengan `nonBillableResidualAmount = 30000` dan outstanding pasien Rp 85.000. Diajukan write-off `NON_BILLABLE_RESIDUAL` sebesar Rp 45.000 | Integration | `422` "Nominal write-off melebihi selisih yang tidak dapat ditagihkan pada tagihan ini." Plafonnya Rp 30.000, **bukan** Rp 85.000. **Jalur gagal yang harus TIDAK terjadi**: pengajuan lolos karena diuji terhadap outstanding pasien |
| `BIL-AT-059` | `BKC-DEC-036`, `BKC-DES-024` | Tagihan yang sama; write-off `NON_BILLABLE_RESIDUAL` Rp 30.000 diajukan pengaju A dan disetujui penyetuju B | Integration | Kasus menjadi `POSTED`; **outstanding pasien tetap Rp 85.000**; status invoice **tidak berpindah**; sisa residual menjadi Rp 0. **Jalur gagal yang harus TIDAK terjadi**: outstanding turun menjadi Rp 55.000, atau invoice menjadi `SETTLED_BY_WRITE_OFF` |
| `BIL-AT-060` | `BIL-VAL-017`, `BIL-VAL-041`, `BIL-VAL-042` | Tiga uji negatif berurutan pada tagihan yang sama: (a) pengaju menyetujui pengajuannya sendiri; (b) pengajuan `NON_BILLABLE_RESIDUAL` dengan `IsFullSettlement = true`; (c) pengajuan dengan `category = "WRITE_OFF_LAIN"` | Integration | (a) `422` "Pengaju write-off tidak boleh menyetujui pengajuannya sendiri."; (b) `422` `BIL-VAL-041`; (c) `422` `BIL-VAL-042` — **bukan** diterima diam-diam sebagai `PATIENT_AR` |
| `BIL-AT-061` | `BKC-DEC-036`, `BKC-DES-024` | Reversal atas kasus `NON_BILLABLE_RESIDUAL` yang sudah `POSTED` pada `BIL-AT-059` | Integration | `BilAdjustment` `Debit` terbentuk menunjuk kasus aslinya; **outstanding pasien tetap Rp 85.000** (tidak naik); invoice **tidak** dipaksa ke `OPEN`; sisa residual kembali menjadi Rp 30.000 dan dapat diajukan ulang. Histori kasus **tidak** dihapus |

### Regresi yang wajib diperiksa

| Yang diperiksa | Kenapa berisiko |
| --- | --- |
| Write-off piutang pasien yang sudah berjalan berperilaku persis seperti sebelumnya | Kolom `Category` berbawaan `PATIENT_AR`, tetapi penyaringan baru pada `CalculateOutstandingAsync` menyentuh perhitungan uang yang dipakai seluruh alur pembayaran. Satu kesalahan penyaringan membuat write-off lama berhenti mengurangi outstanding |
| Full write-off piutang pasien masih memindahkan invoice ke `SETTLED_BY_WRITE_OFF` | Penjaga status kini bercabang kategori. Mudah sekali cabangnya ditulis terbalik, dan salahnya baru terlihat pada tagihan yang benar-benar dilunasi lewat write-off |
| Reversal write-off piutang pasien masih mengembalikan invoice ke `OPEN` | Pengecualian adjustment reversal kini bersyarat kategori. Bila syaratnya terlalu luas, reversal write-off pasien berhenti membuka kembali AR |
| Angka yang dilihat kasir pada baris "Selisih Tidak Ditagihkan" tidak berubah sama sekali | Nominalnya berpindah field, dan layar menjumlah kedua field. Bila layar lupa menjumlah salah satunya, kasir melihat selisih menghilang tanpa ada yang mengubah tagihan |
| Total Tagihan, Subtotal Mandiri, dan Subtotal Asuransi tidak bergeser satu rupiah pun | Amendment ini **tidak** dimaksudkan mengubah nilai apa pun. Setiap pergeseran nominal pada regresi ini berarti suku yang seharusnya hanya berpindah nama ternyata ikut berubah besarnya |
| Jalur `SelfPay()` dan jalur anomali data tetap mengembalikan `nonBillableResidualAmount = 0` | `BillingCoverageDecision` adalah `record` posisional dan bertambah satu argumen. Kesalahan urutan argumen paling mudah terjadi di sini dan paling terlambat ketahuan |
| Jalur `NotCovered` + `IsAllowExcessPaymentByPatient = false` **masih** mengisi `unresolvedAmount` | Jalur (2) sengaja tidak disentuh (`BKC-OQ-093`). Ikut memindahkannya berarti mengarang keputusan bisnis yang tidak pernah diambil pemiliknya |

### Bukti keluar tambahan

Slice ini **MUST** menyertakan: (1) hasil `dotnet build` yang benar-benar dijalankan dan lulus; (2) bukti migration dibuat **dan direview**, disertai pemeriksaan bahwa kedua kolom baru bernilai bawaan pada seluruh baris lama; (3) perbandingan angka sebelum dan sesudah untuk satu tagihan yang memuat residual non-billable, memperlihatkan Total Tagihan dan outstanding pasien **tidak berubah**; (4) hasil pemeriksaan berapa banyak baris `MstInsuranceCoverageRule` aktif yang bernilai `IsAllowExcessPaymentByPatient = false` di lingkungan uji — angka itu adalah perkiraan beban kerja write-off Finance dan menjadi bahan penilaian kelayakan pemicu manual (`BKC-DES-023`); (5) satu contoh kasus write-off residual yang telah melewati pengajuan, persetujuan oleh orang kedua, dan reversal, dengan seluruh jejak auditnya sudah disanitasi. Approval blueprint bukan bukti test.

Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`.
