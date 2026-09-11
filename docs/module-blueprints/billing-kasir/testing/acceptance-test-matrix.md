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

`contract_version: BIL-TEST-0.5` · status **approved** · owner QA + Product/Billing/Finance/Security · approved_by Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · approved_at 4 September 2026 · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009` (approved). Test data wajib fiktif dan **MUST NOT** memakai data pasien produksi; nama perusahaan asuransi pada test memakai nama samaran.

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

`last_changed_in: BIL-TEST-0.6` · status **approved** · owner QA + Product/Billing/Finance/Security · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Data uji **MUST** fiktif; **MUST NOT** memakai data pasien produksi.

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

`last_changed_in: BIL-TEST-0.7` · status **approved** · owner QA + Product/Billing/Finance/Security · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`. Data uji **MUST** fiktif; **MUST NOT** memakai data pasien produksi.

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
| ~~Jalur `NotCovered` + `IsAllowExcessPaymentByPatient = false` **masih** mengisi `unresolvedAmount`~~ **DIKOREKSI revisi `0.9`** | Baris ini berlaku untuk revisi `0.8` saja. `BKC-DEC-089` menutup `BKC-OQ-093` dan memindahkan jalur (2) ke `nonBillableResidual` — lihat `BIL-AT-062` pada amendment revisi `0.9` di bawah, yang menggantikan baris regresi ini |

### Bukti keluar tambahan

Slice ini **MUST** menyertakan: (1) hasil `dotnet build` yang benar-benar dijalankan dan lulus; (2) bukti migration dibuat **dan direview**, disertai pemeriksaan bahwa kedua kolom baru bernilai bawaan pada seluruh baris lama; (3) perbandingan angka sebelum dan sesudah untuk satu tagihan yang memuat residual non-billable, memperlihatkan Total Tagihan dan outstanding pasien **tidak berubah**; (4) hasil pemeriksaan berapa banyak baris `MstInsuranceCoverageRule` aktif yang bernilai `IsAllowExcessPaymentByPatient = false` di lingkungan uji — angka itu adalah perkiraan beban kerja write-off Finance dan menjadi bahan penilaian kelayakan pemicu manual (`BKC-DES-023`); (5) satu contoh kasus write-off residual yang telah melewati pengajuan, persetujuan oleh orang kedua, dan reversal, dengan seluruh jejak auditnya sudah disanitasi. Approval blueprint bukan bukti test.

Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`.

---

## Amendment lanjutan 4 September 2026 — Perluasan perutean write-off ke jalur `NotCovered` (revisi `0.9`)

`last_changed_in: BIL-TEST-0.8` · status **approved** · owner QA + Product/Billing/Finance · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 5 September 2026. Input: **`BKC-DEC-089`** menutup `BKC-OQ-093`; keputusan arsitektur `BKC-DES-026`–`027` (approved 5 September 2026). Data uji **MUST** fiktif; **MUST NOT** memakai data pasien produksi.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-062` | `BKC-DEC-089`, `BKC-DES-026` | Tindakan Akupunktur Rp 200.000 cocok aturan `NotCovered` dengan `IsAllowExcessPaymentByPatient = false` | Domain/Integration | `primaryAmount = 0`; `patientAmount = 0`; `unresolvedAmount = 0`; `nonBillableResidualAmount = 200000`; Total Tagihan tetap Rp 200.000. **Jalur gagal yang harus TIDAK terjadi**: Rp 200.000 masuk `unresolvedAmount` (perilaku revisi `0.8`) atau masuk porsi pasien |
| `BIL-AT-063` | `BKC-DES-026` | Satu tagihan yang memuat nominal dari **kedua** jalur sekaligus: Rp 200.000 dari aturan `NotCovered` (`IsAllowExcessPaymentByPatient = false`) **dan** Rp 30.000 sisa perhitungan Fisioterapi (aturan `Covered` 70%, `IsAllowExcessPaymentByPatient = false`) | Domain/Integration | Satu nominal gabungan `nonBillableResidualAmount = 230000`; satu plafon write-off (`CalculateNonBillableResidualRemainingAsync` mengembalikan Rp 230.000); cukup **satu** pengajuan write-off menutup keduanya. **Jalur gagal yang harus TIDAK terjadi**: dua nominal terpisah, dua plafon, atau Finance dipaksa mengajukan dua write-off untuk satu tagihan |

### Pembanding regresi (menggantikan baris revisi `0.8` yang kini usang)

| Yang diperiksa | Kenapa berisiko |
| --- | --- |
| Aturan `NotCovered` dengan `IsAllowExcessPaymentByPatient = true` (nilai bawaan) tetap menjadi porsi pasien | Amendment ini **hanya** menyentuh aturan yang penandanya di-set `false` secara sengaja. Cabang `true` pada jalur (2) **tidak disentuh** — regresi `BIL-AT-039`/`RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion` **MUST** tetap lulus tanpa perubahan |
| Jalur (5) residual (`Covered` + `IsAllowExcessPaymentByPatient = false`, `BIL-AT-055`) tetap berperilaku sama | Amendment ini memperluas syarat tangkap ke jalur (2) **tambahan**, bukan menggantikan jalur (5). Kedua jalur **MUST** menulis ke akumulator `nonBillableResidual` yang sama, bukan dua akumulator terpisah |
| `unresolvedAmount` pada versi kalkulasi **lama** (dibuat sebelum revisi `0.9`) tetap memuat angka lamanya | Backfill/versi lama **MUST NOT** ditulis ulang — bukti perhitungan yang sudah terjadi tetap valid apa adanya (`BKC-DES-027`) |
| Total Tagihan, Subtotal Mandiri, dan Subtotal Asuransi tidak bergeser satu rupiah pun | Amendment ini **tidak** dimaksudkan mengubah nilai apa pun — hanya memindahkan nasib nominal yang sudah keluar dari porsi pasien sejak revisi `0.7`/`0.8` |
| Write-off kategori `NON_BILLABLE_RESIDUAL` yang sudah diajukan/disetujui pada data jalur (5) (revisi `0.8`) tetap berperilaku sama | Satu akumulator yang sama berarti mekanisme write-off-nya (`BE-BKC-029`) **tidak perlu diubah satu baris pun** — regresi `BIL-AT-058`–`061` **MUST** tetap lulus tanpa perubahan |

### Bukti keluar tambahan

Slice ini **MUST** menyertakan: (1) hasil `dotnet build`/`dotnet test` yang benar-benar dijalankan dan lulus; (2) perbandingan angka sebelum dan sesudah untuk satu tagihan yang memuat nominal jalur (2), memperlihatkan Total Tagihan dan porsi pasien **tidak berubah**; (3) satu contoh tagihan yang memuat nominal dari kedua jalur (2 dan 5) sekaligus, memperlihatkan satu nominal gabungan dan satu pengajuan write-off yang menutup keduanya; (4) hasil pemeriksaan dua kelompok `MstInsuranceCoverageRule` aktif bernilai `IsAllowExcessPaymentByPatient = false` — berapa berstatus `NotCovered` dan berapa `Covered` dengan tanggungan sebagian, sebagai perkiraan beban kerja Finance gabungan. **Nol bukti migration** — amendment ini tidak menyentuh skema. Approval blueprint bukan bukti test.

Trace **`BKC-DEC-089`**, `BKC-DEC-080`, `BKC-DES-026`–`027`.

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

`last_changed_in: BIL-TEST-0.9` · status **draft** · owner QA + Kepala Kasir/Finance Operations + Security · `approved_by`: — · `approved_at`: — · input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014`. Data uji **MUST** fiktif; nama penerima memakai nama samaran.

| ID | Requirement/decision | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-064` | `PC-DEC-013`, `PC-DES-003` | Satu voucher berjalan penuh: dibuat, disetujui, uang diserahkan, nota dimasukkan | E2E | Status berpindah `WAITING_APPROVAL` → `APPROVED` → `CASH_RECEIVED` → `COMPLETED`, dan layar menampilkan label `Menunggu Persetujuan` → `Disetujui` → `Uang Diterima` → `Selesai` **persis** seperti `PC-DEC-013`. Empat baris `BilPettyCashVoucherCommand` terbentuk |
| `BIL-AT-065` | `PC-DES-008`, `CAP-30`, `QBE-CODE-003` | Voucher pertama dibuat pada tanggal Asia/Jakarta tertentu | Integration | `VoucherNumber` berbentuk `PTC-YYYYMMDD-0001`. Satu baris `BilNumberSeries` ber-`SequenceKey = 'BILLING_PETTY_CASH_VOUCHER'` terbentuk. **Jalur gagal yang harus TIDAK terjadi**: nomor berbasis milidetik epoch seperti pada rujukan tampilan, atau nomor hasil `Count+1`/`Max+1` |
| `BIL-AT-066` | `PC-DES-008` | Sepuluh voucher dibuat bersamaan dari sepuluh permintaan paralel | Concurrency | Sepuluh nomor berbeda, tidak ada yang kembar, dan tidak ada nomor yang terlewat. Membuktikan `pg_advisory_xact_lock` yang diwarisi benar-benar bekerja untuk jenis nomor baru |
| `BIL-AT-067` | `PC-DEC-002`, `PC-DEC-009`, `PC-DES-004` | Saldo Rp 5.000.000. Voucher Rp 300.000 dibuat, lalu disetujui, lalu uangnya diserahkan | Integration | Setelah **dibuat**: saldo Rp 5.000.000, komitmen Rp 0. Setelah **disetujui**: saldo **tetap** Rp 5.000.000, komitmen Rp 300.000. Setelah **uang diserahkan**: saldo Rp 4.700.000, komitmen Rp 0, satu baris ledger `DISBURSEMENT` ber-`BalanceBefore` Rp 5.000.000 dan `BalanceAfter` Rp 4.700.000. **Jalur gagal yang harus TIDAK terjadi**: saldo berkurang saat persetujuan |
| `BIL-AT-068` | `PC-DEC-009` | Voucher pada `BIL-AT-067` dimasukkan bukti notanya | Integration | Status menjadi `COMPLETED`; saldo **tetap** Rp 4.700.000; **tidak ada** baris ledger baru. Membuktikan `Selesai` murni administratif |
| `BIL-AT-069` | `PC-DEC-008`, `PC-DES-005`, `BIL-VAL-047` | Saldo Rp 5.000.000. Voucher A Rp 300.000 disetujui. Voucher B Rp 4.800.000 hendak disetujui | Integration | Voucher B ditolak `422` beserta pesan yang menyebut sisa Rp 4.700.000. Voucher B **tetap** `Menunggu Persetujuan`. **Jalur gagal yang harus TIDAK terjadi**: voucher B lolos karena diuji terhadap Rp 5.000.000, lalu keduanya dicairkan dan saldo menjadi minus Rp 100.000 |
| `BIL-AT-070` | `PC-DES-006`, `BIL-VAL-048` | Voucher Rp 300.000 disetujui saat saldo Rp 5.000.000. Saldo lalu diturunkan menjadi Rp 200.000 lewat jalur yang melewati penjaga komitmen. Kasir menekan "Uang Diberikan" | Integration | Ditolak `422`; voucher **tetap** `Disetujui`; saldo **tetap** Rp 200.000; **tidak ada** baris ledger. Membuktikan penjaga kedua benar-benar dipasang, bukan hanya penjaga di persetujuan |
| `BIL-AT-071` | `PC-DES-006`, `BIL-VAL-057` | Tombol "Uang Diberikan" dikirim dua kali: (a) dengan `Idempotency-Key` sama; (b) tanpa kunci; (c) dua permintaan benar-benar bersamaan | Integration/concurrency | Ketiganya menghasilkan saldo berkurang **tepat satu kali** Rp 300.000 dan **tepat satu** baris ledger `DISBURSEMENT`. (a) mengembalikan hasil permintaan pertama; (b) ditolak `422` `BIL-VAL-046`; (c) satu berhasil dan satu ditolak. **Uji negatif inti seluruh rumpun ini** |
| `BIL-AT-072` | `PC-DEC-003`, `PC-DES-013`, `BIL-VAL-052` | Voucher ditolak, lalu dicoba disunting, diajukan ulang, disetujui, dan dibatalkan | API/security | Keempatnya gagal. **Bukti yang diharapkan bukan hanya kode galat**: pemeriksaan daftar route membuktikan **tidak ada** endpoint `PUT`, `PATCH`, atau resubmit pada voucher sama sekali. Baris voucher `REJECTED` identik sebelum dan sesudah keempat percobaan |
| `BIL-AT-073` | `PC-DEC-007`, `PC-DES-007`, `BIL-VAL-050` | Tiga percobaan pembatalan: (a) pemohon membatalkan voucher `Menunggu Persetujuan`; (b) orang lain membatalkan voucher itu; (c) pemohon membatalkan voucher yang sudah `Disetujui` | API/security | (a) berhasil — `IsCancel = true`, `CancelBy` terisi, dan `Status` **tetap** `WAITING_APPROVAL`; (b) ditolak `403`; (c) ditolak `422`. **Jalur gagal yang harus TIDAK terjadi**: lahirnya status keenam `CANCELLED` pada kolom `Status` |
| `BIL-AT-074` | `PC-DEC-005` | Kasir menekan "Uang Diberikan" | Integration | Status langsung `CASH_RECEIVED` dalam **satu** permintaan. **Tidak ada** status antara dan **tidak ada** endpoint konfirmasi penerima yang perlu dipanggil sesudahnya |
| `BIL-AT-075` | `PC-DEC-006` | Voucher `Uang Diterima` dibiarkan tanpa bukti nota, lalu waktu dimajukan melewati akhir bulan dan seluruh pekerjaan latar dijalankan | Integration | Voucher **tetap** `Uang Diterima`; tidak ada perubahan status, tidak ada pemberitahuan, dan tidak ada pemblokiran. **Ini keadaan yang dipilih sengaja**, bukan kelalaian. Test ini ada justru agar penambahan eskalasi kelak terlihat sebagai perubahan perilaku yang disengaja |
| `BIL-AT-076` | `PC-DEC-012`, `PC-DES-002`, `BIL-VAL-053` | Finance membuat kategori, memakainya pada satu voucher, lalu mencoba menghapusnya, lalu menonaktifkannya | API | Penghapusan ditolak `400`. Penonaktifan berhasil. Voucher lama **tetap** menampilkan nama kategorinya; dropdown Buat Voucher **tidak** lagi menawarkannya |
| `BIL-AT-077` | **`PC-DEC-001`** | Shift kasir dibuka, satu pembayaran pasien tunai diterima, satu voucher Petty Cash Rp 300.000 dicairkan, lalu shift ditutup | Integration | `SystemCash`, `PhysicalCash`, dan `Variance` shift **identik** dengan hasil skenario pembanding yang tidak mencairkan voucher sama sekali. **Jalur gagal yang harus TIDAK terjadi**: shift kasir ikut berkurang Rp 300.000. Ini uji regresi paling penting pada amendment ini |
| `BIL-AT-078` | `PC-DEC-004`, hak akses | Pengguna tanpa `PettyCashVoucher : Approve` mencoba menyetujui; pengguna tanpa `PettyCashBudget : TopUp` mencoba menambah anggaran; pengguna tanpa `PettyCashVoucher : Read` membuka daftar | Security | Ketiganya `403`, tanpa mutasi apa pun. **Ditambah pemeriksaan yang wajib**: untuk setiap action pada ketiga controller baru, argumen pertama `[AccessAction]` sama persis dengan argumen kedua `[AccessPermission]`, dan `AccessType` bernilai salah satu dari `Read`/`Create`/`Update`/`Delete`. Kesalahan di sini tidak terlihat saat menguji memakai akun SuperAdmin |
| `BIL-AT-079` | Privasi, `PC-DES-009` | Satu voucher berjalan penuh sambil log aplikasi direkam | Security | Log **tidak memuat** `RecipientName`, `Purpose`, `RejectionReason`, maupun isi `ResponseJson` — diperiksa dengan pencarian teks pada seluruh keluaran log, bukan hanya pada field yang diperiksa satu per satu. Log **memuat** `VoucherId`, `VoucherNumber`, nominal, status sebelum dan sesudah, serta `ActorUserId` |
| `BIL-AT-080` | `PC-DEC-010`, `PC-DES-014`, `BIL-VAL-054` | Tiga percobaan: (a) menambah kolam aktif kedua; (b) koreksi saldo menjadi negatif; (c) koreksi saldo menjadi di bawah nominal voucher yang sudah disetujui | Integration | (a) ditolak unique index parsial; (b) dan (c) ditolak `422` beserta pesan yang menyebut nominal yang sudah dijanjikan. Saldo tidak bergerak pada ketiganya |

### Regresi yang wajib diperiksa

| Yang diperiksa | Kenapa berisiko |
| --- | --- |
| Keempat jenis nomor yang sudah ada tetap berformat sama persis | `BillingNumberSeriesService` disentuh amendment ini. Parameter constructor baru **MUST** opsional berbawaan; bila tidak, seluruh pemanggil dan test yang sudah berjalan rusak sekaligus, dan rusaknya menyentuh Invoice, Deposit, Shift Kasir, serta Kwitansi bersamaan |
| Helper privat `AllocateNumberAsync` tidak berubah satu baris pun | Ia dipakai lima jenis nomor sesudah amendment ini. Perubahan di sana menyentuh seluruh penomoran modul |
| `BilCashierShift` tidak bergerak satu rupiah pun oleh aktivitas Petty Cash | `PC-DEC-001`. Diuji langsung oleh `BIL-AT-077`; disebut ulang di sini karena ia juga perlu diperiksa pada setiap perubahan Petty Cash berikutnya, bukan hanya sekali |
| Seluruh angka tagihan pasien tidak bergeser | Petty Cash **tidak** menyentuh `BilInvoice`, `BilCalculationVersion`, maupun rantai kalkulasi. Setiap pergeseran nominal pada regresi kalkulasi berarti ada sambungan yang tidak seharusnya dibuat |
| `BillingManagementServiceCollectionExtensions` masih mendaftarkan seluruh service lama | Berkas itu disentuh amendment ini. Satu baris yang tidak sengaja terhapus akan melumpuhkan rumpun lain saat aplikasi mulai |
| `ApplicationDbContext` masih memuat seluruh `DbSet` dan configuration lama | Alasan yang sama |

### Bukti keluar tambahan

Slice ini **MUST** menyertakan:

1. hasil `dotnet build` yang benar-benar dijalankan dan lulus;
2. bukti migration **dibuat dan direview**, disertai pemeriksaan bahwa kelima tabel lahir kosong dan tidak ada satu pun tabel existing yang di-`ALTER`;
3. bukti seed berjalan: **tepat satu** baris `BilPettyCashBudget` dan lima baris `MstPettyCashCategory`, sehingga dropdown "Pilih Kategori" tidak kosong pada pemakaian pertama;
4. satu contoh response voucher yang sudah disanitasi untuk masing-masing dari kelima status, memperlihatkan `statusLabel` sesuai `PC-DEC-013`;
5. tangkapan layar perbandingan penutupan shift kasir dengan dan tanpa pencairan voucher pada hari yang sama, memperlihatkan angkanya identik (`BIL-AT-077`);
6. hasil pemeriksaan pasangan `[AccessAction]`/`[AccessPermission]` untuk kedua belas action baru, ditulis sebagai tabel tiga kolom — nama method, argumen pertama `[AccessAction]`, argumen kedua `[AccessPermission]` — sehingga ketidaksesuaian terlihat tanpa membaca kode;
7. bukti kedua belas butir hak akses baru **benar-benar muncul** di layar Akses Role dan dapat dicentang admin. Butir yang tidak muncul berarti permission-nya belum selesai, sekalipun endpointnya berjalan saat diuji memakai SuperAdmin.

Approval blueprint bukan bukti test.

Trace **`PC-DEC-001`–`013`**, `PC-DES-001`–`014`.

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

> `last_changed_in`: `BIL-TEST-1.0` / revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010`, `MPY-DES-001`–`017`.
>
> Seluruh contoh memakai data samaran.

## Mengganti penanggung kunjungan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-081` | `MPY-DEC-003`, `MPY-DES-001` | Kunjungan rawat jalan didaftarkan tunai. Kasir mengganti penanggung menjadi kartu asuransi milik pasien yang masih berlaku | Integration | Penanggung kunjungan berubah menjadi asuransi; tagihan punya versi perhitungan baru; porsi penjamin naik dari nol; jejak perubahan tercatat satu baris |
| `BIL-AT-082` | `MPY-DEC-003` | Kunjungan berasuransi diganti menjadi penjamin perusahaan, lalu diganti lagi menjadi tunai | Integration | Ketiga perubahan berhasil berurutan; kunjungan tetap punya **tepat satu** baris sumber pembayaran sepanjang seluruh rangkaian |
| `BIL-AT-083` | `BIL-VAL-068` | Kasir memilih kartu asuransi milik pasien lain | Integration | Ditolak `422` beserta pesan yang dapat dibaca pengguna; penanggung kunjungan tidak berubah |
| `BIL-AT-084` | `BIL-VAL-070` | Kartu penjamin berlaku sampai 31 Agustus 2026; tanggal pelayanan 11 September 2026 | Integration | Ditolak `422`; tidak ada versi perhitungan baru yang lahir |
| `BIL-AT-085` | `BIL-VAL-060` | Tagihan sudah menerima satu pembayaran berhasil, lalu kasir mencoba mengganti penanggung | Integration | Ditolak `409`; angka tagihan dan pembayaran yang sudah masuk tidak bergeser sama sekali |
| `BIL-AT-086` | `BIL-VAL-061` | Dua kasir membuka tagihan yang sama. Kasir A menyimpan lebih dulu, lalu Kasir B menyimpan dengan versi lama | Integration | Perintah Kasir B ditolak `409`; **nol perubahan tersimpan sebagian** — penanggung, penanggung baris, dan versi perhitungan seluruhnya tetap seperti hasil Kasir A |
| `BIL-AT-087` | `MPY-DES-005`, `MPY-DES-015` | Kasir meminta pratinjau perbandingan lima kali berturut-turut | Integration | Nol versi perhitungan baru lahir; nol baris jejak perubahan lahir; penanggung kunjungan tidak bergerak. Angka pratinjau konsisten pada kelima pemanggilan |
| `BIL-AT-088` | `MPY-DES-003`, NFR idempotency | Perintah ganti penanggung dikirim dua kali dengan kunci idempotensi yang sama | Integration | Hanya satu baris jejak perubahan tercatat; hanya satu versi perhitungan baru lahir; permintaan kedua mengembalikan hasil yang sama tanpa efek tambahan |

## Menentukan penanggung per baris biaya

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-089` | `MPY-DEC-004` | Kunjungan berpenjamin perusahaan. Kasir menandai satu baris vitamin seharga Rp 80.000 menjadi tanggungan pasien | Integration | Porsi pasien bertambah Rp 80.000; porsi penjamin berkurang sebesar porsi yang tadinya ditanggung; **Subtotal Mandiri + Subtotal Penjamin + pajak = Total Tagihan**, tanpa selisih |
| `BIL-AT-090` | `MPY-DEC-004` | Satu baris obat yang menurut aturan tanggungan berstatus tidak tertanggung ditandai ditanggung asuransi | Integration | Perintah **berhasil**, bukan ditolak; hasil perhitungan menunjukkan nol tertanggung dan pasien membayar penuh; keterangan penanggung baris tetap tercatat asuransi |
| `BIL-AT-091` | `BIL-VAL-077`, `BIL-VAL-078` | Kunjungan tunai; kasir menandai satu baris ditanggung penjamin perusahaan | Integration | Ditolak `422` beserta pesan yang menyebut kunjungan tidak memakai penjamin |
| `BIL-AT-092` | **`MPY-DES-009`** | Kunjungan berpenjamin perusahaan dengan tiga baris bertanda penjamin. Kasir mengganti penanggung kunjungan menjadi tunai | Integration | Ketiga baris **otomatis** kembali menjadi tanggungan pasien bertanda sumber otomatis beserta alasan bawaan; response menyebut angka tiga; nol baris tertinggal menunjuk penanggung yang sudah tidak ada |

## Menentukan obat yang masuk tagihan

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-093` | `MPY-DEC-009` | Tagihan rawat jalan dengan empat baris obat; kasir memilih Ditebus | Integration | Keempat baris masuk tagihan; subtotal obat sama dengan sebelum perintah dijalankan |
| `BIL-AT-094` | `BIL-VAL-083`, `MPY-DES-010` | Dari empat baris obat, kasir mencentang dua lalu memilih Tebus Sebagian | Integration | Dua baris masuk tagihan, dua tidak; **jumlah pada keempat baris tidak berubah sedikit pun**; baris yang tidak ditebus tidak muncul sebagai porsi pasien maupun porsi penjamin |
| `BIL-AT-095` | `BIL-VAL-081`, `MPY-DES-011` | (a) Kunjungan rawat inap, kasir mencoba mengatur penebusan. (b) Kunjungan IGD, kasir mengatur penebusan | Integration | (a) Ditolak `422`. (b) **Berhasil** — membuktikan IGD diperlakukan terpisah dari rawat inap dan tidak ikut tertolak |
| `BIL-AT-096` | **`MPY-DEC-009`** | Seluruh alur Edit Billing dijalankan pada tagihan dengan resep yang sudah diserahkan sebagian oleh Farmasi | **Regresi lintas modul** | **Nol baris data penyerahan obat milik Farmasi tersentuh** — jumlah diserahkan, jumlah tersisa, status penyerahan, dan riwayatnya identik sebelum dan sesudah perintah |

## Hak akses, privasi, dan master data

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-097` | `MPY-DEC-006`, `CAP-39` | Pengguna tanpa kewenangan ubah tagihan membuka layar Edit Tagihan; pengguna tanpa kewenangan master data membuka kedua layar master baru | Integration | Pembacaan diizinkan bagi pemegang kewenangan baca; ketiga perintah ubah ditolak `403`; kedua butir kewenangan master baru muncul pada pengaturan peran tanpa berkas seed disunting |
| `BIL-AT-098` | Privasi | Ketiga perintah dijalankan, lalu lembar tagihan perusahaan diunduh | Integration | Catatan log **tidak memuat** nomor polis, nomor kartu, nomor karyawan, maupun nama karyawan; nama berkas lembar tagihan memakai nomor tagihan, bukan nama pasien maupun nama perusahaan |
| `BIL-AT-099` | `BIL-VAL-087`–`092` | Admin membuat rute menanggung sendiri sambil mengisi asuransi mitra; lalu menandai rute kedua sebagai bawaan; lalu membuat aturan tanggungan dengan persentase 80 | Integration | Dua yang pertama ditolak beserta pesan yang dapat dibaca; yang ketiga tersimpan dengan urun biaya **20 yang diturunkan server**, bukan nilai yang dikirim klien |
| `BIL-AT-100` | **Regresi menyeluruh** | Hitung ulang tiga tagihan lama: satu tunai, satu berasuransi, satu berpenjamin perusahaan | **Regresi** | Tagihan tunai dan tagihan berasuransi menghasilkan angka **identik** dengan sebelum amendment ini. Tagihan berpenjamin perusahaan **tidak lagi** menghasilkan anomali "perusahaan asuransi belum dipilih", dan porsi penjaminnya kini terhitung sesuai aturan tanggungan perusahaan |

## Dokumen lembar tagihan perusahaan

Pengujian lembar tagihan perusahaan penjamin mengikuti pola pengujian lembar Invoice Asuransi yang sudah ada (`BIL-AT-031`–`035`), dengan satu tambahan yang mengikat: lembar itu **MUST NOT** dapat dicetak untuk kunjungan tunai maupun kunjungan berasuransi pribadi, dan **MUST** memuat keterangan rute penggantian biaya bila perusahaannya memilikinya.

## Catatan cakupan

Setiap kemampuan wajib pada rumpun ini memiliki **sekurang-kurangnya satu skenario berhasil dan satu skenario gagal**. Tiga pengujian ditandai regresi karena keduanya menjaga hal yang paling mudah rusak tanpa disadari: angka tagihan kunjungan yang tidak disentuh rumpun ini (`BIL-AT-100`), data milik modul lain (`BIL-AT-096`), dan perilaku konsumen lama komponen yang dipakai ulang (acceptance frontend nomor 70).

Approval blueprint bukan bukti test.

Trace **`MPY-DEC-001`–`010`**, `MPY-DES-001`–`017`.
