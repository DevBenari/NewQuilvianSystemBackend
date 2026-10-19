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

---

## Amendment 15 September 2026 — Revisi Petty Cash: pencairan langsung dan anggaran per periode

`last_changed_in: BIL-TEST-1.1` · status **approved** (`PC-DEC-026`, 15 September 2026) · input: **`PC-DEC-016`–`PC-DEC-025`**; keputusan arsitektur `PC-DES-015`–`PC-DES-025`.

Dua puluh acceptance test, `BIL-AT-101`–`BIL-AT-120`. **Lima di antaranya regresi** — menguji bahwa yang seharusnya tidak berubah memang tidak berubah, dan bahwa yang dihapus benar-benar tidak dapat dipanggil lagi.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-101` | `PC-DEC-016` | Kasir membuat permintaan lalu langsung mencairkannya tanpa persetujuan siapa pun | Integrasi | Status berpindah `Menunggu Pencairan` lalu `Menunggu Bukti` dalam dua panggilan; saldo berkurang tepat sekali; satu baris `DISBURSEMENT` lahir |
| `BIL-AT-102` | `PC-DEC-016`, `PC-DEC-024` | Memanggil `POST /vouchers/{id}/approve` setelah rilis | Integrasi | `404` — endpointnya tidak ada lagi. **Regresi** |
| `BIL-AT-103` | `PC-DEC-024` | Memanggil `POST /vouchers/{id}/reject` setelah rilis | Integrasi | `404`. **Regresi** |
| `BIL-AT-104` | `PC-DES-015` | Membaca voucher warisan yang sebelumnya `WAITING_APPROVAL` | Integrasi | `status` bernilai `REQUESTED`, `statusLabel` bernilai `Menunggu Pencairan`; nomor voucher, nominal, dan penerimanya tidak berubah |
| `BIL-AT-105` | `PC-DES-015`, `PC-DES-024` | Membaca voucher warisan yang sebelumnya `APPROVED` dan belum dicairkan | Integrasi | `status` bernilai `REQUESTED` dan **dapat langsung dicairkan**; `decidedAt`/`decidedByName` tetap terbaca sebagai jejak lama |
| `BIL-AT-106` | `PC-DES-015`, `PC-DEC-003` | Membaca voucher warisan berstatus `REJECTED` | Integrasi | Tetap `REJECTED` dengan label `Ditolak (arsip)`; `rejectionReason` utuh; seluruh aksi atasnya ditolak. **Regresi** |
| `BIL-AT-107` | `PC-DES-016` | Membaca `GET /budget/current` | Integrasi | Response **tidak** lagi memuat `reservedAmount` maupun `availableAmount` |
| `BIL-AT-108` | `PC-DES-016` | Sepuluh permintaan Rp 500.000 dibuat berturut-turut di atas saldo Rp 1.000.000 | Integrasi | Seluruh permintaan **berhasil dibuat** (tidak ada saldo yang terkunci); pencairan ketiga **ditolak** `BIL-VAL-048` karena saldo benar-benar habis |
| `BIL-AT-109` | `PC-DES-006` | Dua kasir mencairkan bersamaan dari saldo yang hanya cukup untuk satu | Integrasi/konkurensi | Tepat satu berhasil; yang lain `422` `BIL-VAL-048`; saldo akhir **tidak** negatif. **Regresi** |
| `BIL-AT-110` | `PC-DEC-017` | Finance membuat periode anggaran baru lalu mengaktifkannya sementara periode lain masih `ACTIVE` | Integrasi | Pembuatan berhasil (`DRAFT`); pengaktifan **ditolak** `BIL-VAL-103` |
| `BIL-AT-111` | `PC-DEC-018`, `PC-DES-018` | Finance menutup periode bersisa Rp 1.250.000 dengan penerus yang sah | Integrasi | Periode lama bersaldo `0` dengan satu baris `CARRY_FORWARD_OUT`; periode penerus bertambah Rp 1.250.000 dengan satu baris `CARRY_FORWARD_IN`; keduanya dalam satu transaction |
| `BIL-AT-112` | `BIL-VAL-104` | Finance menutup periode yang masih punya permintaan `Menunggu Pencairan` | Integrasi | `422` `BIL-VAL-104`; periode tetap `ACTIVE`; tidak ada baris carry-forward yang lahir |
| `BIL-AT-113` | `BIL-VAL-104` | Finance menutup periode bersisa saldo tanpa menyebutkan periode penerus | Integrasi | `422`; saldo tidak berpindah ke mana pun |
| `BIL-AT-114` | `BIL-VAL-105` | Penambahan saldo dijalankan pada periode `CLOSED` | Integrasi | `422` `BIL-VAL-105` |
| `BIL-AT-115` | `BIL-VAL-106` | Pencairan dijalankan ketika tidak ada periode `ACTIVE` sama sekali | Integrasi | `422` `BIL-VAL-106` dengan pesan yang mengarahkan Finance membuat periode, **bukan** pesan saldo tidak mencukupi |
| `BIL-AT-116` | `PC-DES-020` | Penerima mengembalikan sisa Rp 50.000 lalu Rp 60.000 dari voucher Rp 500.000 | Integrasi | Dua baris `RETURN`; `returnedAmount` menjadi Rp 110.000; **status voucher tidak berubah**; saldo bertambah Rp 110.000 |
| `BIL-AT-117` | `BIL-VAL-098` | Pengembalian Rp 400.000 diajukan saat sisa di tangan hanya Rp 390.000 | Integrasi | `422` `BIL-VAL-098`; `returnedAmount` tidak berubah |
| `BIL-AT-118` | `PC-DES-020` | Kasir membalik pencairan voucher Rp 500.000 yang `returnedAmount`-nya Rp 110.000 | Integrasi | Satu baris `REVERSAL` sebesar Rp 390.000; status menjadi `Dibatalkan (Uang Dikembalikan)`; total yang kembali ke kolam Rp 500.000 terbaca dari dua jenis baris yang terpisah |
| `BIL-AT-119` | `BIL-VAL-099` | Pembalikan kedua diajukan atas voucher yang sudah dibalik | Integrasi | `422` `BIL-VAL-099`; unique index parsial juga menolaknya di lapis database |
| `BIL-AT-120` | `PC-DEC-023`, `PC-DES-023` | Seluruh alur pencairan, pengembalian, pembalikan, dan penutupan periode dijalankan | Integrasi | **Nol** baris `AccJournal` lahir; **nol** pemanggilan ke `AccountingManagement`. **Regresi** terhadap keputusan menunda integrasi |

### Yang sengaja tidak diuji

| Yang tidak diuji | Alasan |
| --- | --- |
| Unggah berkas bukti | Tidak dibangun (`PC-DEC-021`) |
| Perpindahan `EVIDENCE_SUBMITTED` ke `SETTLED` | Status itu tidak ada (`PC-DEC-025`) |
| Posting jurnal otomatis ke Kas Kecil | Ditunda (`PC-DEC-023`); `BIL-AT-120` justru menguji ketiadaannya |
| Approval berjenjang berdasarkan nominal | Tidak pernah masuk scope revisi ini |

---

## Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED`

`last_changed_in: BIL-TEST-1.2` · status **approved** (`BKC-DEC-105`, 18 September 2026) · input: **`BKC-DEC-100`–`BKC-DEC-102`**; keputusan arsitektur `BKC-DES-028`–`BKC-DES-035`.

Empat belas acceptance test, `BIL-AT-121`–`BIL-AT-134`. **Lima di antaranya regresi** — menjaga hal-hal yang paling mudah rusak tanpa disadari ketika status invoice mulai berpindah sendiri.

### Jalur utama — tagihan lunas menjadi tertutup

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-121` | `BKC-DEC-100` | Tagihan `FINAL` senilai Rp 2.000.000 dibayar lunas sekali bayar | Integrasi | Status berpindah ke `CLOSED` pada transaksi yang sama dengan pembayarannya; `closedAt` terisi **waktu pembayaran itu**, bukan waktu baris kode dieksekusi; `rowVersion` berubah tepat sekali |
| `BIL-AT-122` | `BKC-DEC-100`, § 21 capability map | Tagihan `FINAL` Rp 2.000.000 dicicil dua kali (Rp 1.200.000 lalu Rp 800.000) lewat **dua settlement terpisah** | Integrasi | Sesudah cicilan pertama status **tetap** `FINAL`; sesudah cicilan kedua berpindah ke `CLOSED`. Membuktikan perhitungan kumulatif lintas settlement, bukan per settlement |
| `BIL-AT-123` | `BKC-DES-029` | Tagihan `FINAL` dilunasi **seluruhnya dari alokasi deposit pasien**, tanpa satu pun tender baru | Integrasi | Status berpindah ke `CLOSED`. Membuktikan penyelarasan tidak hanya terpasang di jalur tender |
| `BIL-AT-124` | **`BKC-DEC-102`** | Tagihan departure exception (DAMA) difinalisasi dengan sisa tagihan, lalu piutangnya dilunasi beberapa hari kemudian | Integrasi | Saat finalisasi status `FINAL` (**tidak** langsung `CLOSED`); sesudah pelunasan berpindah ke `CLOSED`. Membuktikan invoice departure exception tidak dikecualikan |
| `BIL-AT-125` | `BKC-DEC-100` | Tagihan `FINAL` yang sisa tagihannya dinolkan oleh **penyesuaian arah `Credit`** yang diposting | Integrasi | Status berpindah ke `CLOSED` tanpa satu rupiah pembayaran baru |

### Jalur balik — tagihan tertutup yang terbuka kembali

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-126` | **`BKC-DES-031`** | Tagihan yang sudah `CLOSED` pembayarannya dibalik (`SUCCEEDED` → `REVERSED`) | Integrasi | Status kembali ke `FINAL`; `closedAt` kembali **kosong**; tagihan muncul lagi sebagai punya sisa tagihan |
| `BIL-AT-127` | `BKC-DES-031` | Tagihan yang sudah `CLOSED` menerima penyesuaian arah `Debit` yang diposting | Integrasi | Status kembali ke `FINAL`; sisa tagihan sama dengan nominal penyesuaian itu |
| `BIL-AT-128` | `BKC-DES-031` | Tagihan yang kembali `FINAL` pada `BIL-AT-126` dibayar lunas lagi | Integrasi | Status berpindah ke `CLOSED` lagi; `closedAt` terisi waktu pembayaran **yang kedua**, bukan yang pertama |

### Lubang koreksi AR — inti perbaikan `BKC-DEC-101`

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-129` | **`BKC-DEC-101`**, `BKC-DES-035` | Tagihan lunas berstatus `CLOSED`; tiga hari kemudian ketahuan salah tagih Rp 300.000 dan petugas memposting penyesuaian atasnya | Integrasi | **Satu baris koreksi AR (`BilHandoffAdjustment`) benar-benar lahir.** Inilah skenario Tn. Budi pada temuan 2 September 2026 yang selama ini gagal diam-diam |
| `BIL-AT-130` | `BKC-DES-035` | Write-off diposting atas tagihan berstatus `SETTLED_BY_WRITE_OFF` | Integrasi | **Nol** baris koreksi AR lahir — penjaga tetap menolak status ini dengan sengaja. **Regresi** |
| `BIL-AT-131` | `BKC-DES-035` | Penyesuaian diposting atas tagihan yang masih `OPEN` | Integrasi | **Nol** baris koreksi AR lahir; belum ada handoff yang dapat dikoreksi. **Regresi** |

### Idempotency, konkurensi, dan regresi

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-132` | `BKC-DES-030` | Event provider yang sama dikirim **dua kali** untuk tender pelunasan yang sama | Integrasi | Status berpindah **tepat sekali**; `rowVersion` invoice naik tepat sekali; tepat satu baris audit perpindahan status; `closedAt` tidak bergeser pada pengiriman kedua |
| `BIL-AT-133` | **`BKC-DES-032`** | Dua tender berbeda pada **satu invoice yang sama** direkonsiliasi bersamaan, dan bersama-sama melunasinya | Integrasi/konkurensi | Tepat satu perpindahan status; **tidak ada** galat "Data telah berubah" yang sampai ke kasir; saldo dan status akhir benar |
| `BIL-AT-134` | `BKC-DES-028`, `BKC-DES-034` | (a) Finalisasi tagihan lunas dijalankan. (b) Sisa tagihan sepuluh invoice lama dihitung sebelum dan sesudah konsolidasi perhitungan. (c) Dry-run backfill dijalankan, lalu migration-nya | Regresi | (a) Status hasil finalisasi **tetap `FINAL`**, tidak pernah langsung `CLOSED`. (b) Kesepuluh angka **identik** sebelum dan sesudah konsolidasi. (c) Jumlah baris yang benar-benar berpindah **sama persis** dengan jumlah yang dilaporkan dry-run; `closedAt` hasil backfill berasal dari waktu pelunasan, dan **tidak ada** dua baris hasil backfill yang `closedAt`-nya sama dengan waktu migration dijalankan |

### Yang sengaja tidak diuji

| Yang tidak diuji | Alasan |
| --- | --- |
| Penyerahan nyata ke sistem AR/AP | Konsumennya belum ada (`BKC-BLK-INT-001`); justru ketiadaannya yang memicu amendment ini |
| Perpindahan status `BilArHandoff` menjadi "tertagih" | Status itu tidak ada dan sengaja tidak dibuat (`BKC-DES-033`) |
| Endpoint manual untuk menutup invoice | Tidak ada endpointnya; `BIL-AT-134`(a) menguji bahwa satu-satunya jalur ke `CLOSED` adalah penyelarasan otomatis |
| Layar frontend baru | Nol perubahan frontend (§ 21 capability map) |

Approval blueprint bukan bukti test.

Trace **`BKC-DEC-100`–`102`**, `BKC-DES-028`–`035`.

---

## Amendment 21 September 2026 — Penerbitan fakta ke modul konsumen

`last_changed_in: BIL-TEST-1.3` · status **draft** · input `BKC-DEC-106`–`109`, `BKC-DES-036`–`041`.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- | --- |
| `BIL-AT-135` | `BKC-DEC-106` | Satu pembayaran berhasil melunasi tagihan yang memuat resep | Integrasi | Tepat satu surat penerimaan **dan** tepat satu surat clearance, keduanya membawa korelasi yang sama, keduanya tercipta dalam satu transaksi |
| `BIL-AT-136` | `BKC-DEC-106`, `FIN-DEC-005` | Pembayaran berhasil tetapi tagihan belum lunas | Integrasi | Surat penerimaan terbit; **nol** surat clearance. Membuktikan surat Finance tidak menunggu pelunasan maupun finalisasi |
| `BIL-AT-137` | `PHA-DEC-068` | Biaya tindakan ditambahkan pada tagihan yang sudah lunas | Integrasi | **Nol** surat clearance pencabutan. Keadaan clearance resep tetap boleh diambil walau tagihan kembali bersisa |
| `BIL-AT-138` | `PHA-DEC-068` | Harga obat pada resep dikoreksi naik | Integrasi | Satu surat pencabutan bersebab kenaikan biaya resep, bernomor versi lebih tinggi dari surat sebelumnya |
| `BIL-AT-139` | `PHA-DEC-068-A` | Pembayaran dibalik pada tagihan yang memuat tiga resep | Integrasi | Tiga surat pencabutan — satu per resep — seluruhnya bersebab pembalikan pembayaran. Membuktikan perilaku fail-closed berlaku menyeluruh, bukan selektif |
| `BIL-AT-140` | `PHA-DEC-065` | Tagihan lunas dengan tender bercampur: sebagian asuransi, sebagian tunai | Integrasi | Surat clearance berhasil finansial penjaminan, bukan pembayaran tunai, terlepas dari proporsi nominalnya |
| `BIL-AT-141` | `BKC-DEC-107` | Surat clearance terbit tetapi tidak pernah diproses konsumen, lalu keadaan resep ditanyakan lewat permukaan pemeriksaan | Integrasi | Jawaban pemeriksaan sama persis dengan isi surat terakhir yang sah, walau suratnya belum pernah diakui |
| `BIL-AT-142` | `BKC-DEC-108`, `BKC-DEC-109` | Surat diakui dua kali berturut-turut | API | Pengakuan pertama berhasil; pengakuan kedua ditolak tanpa mengubah apa pun, dan baris tetap ada |

### Jalur gagal yang wajib dibuktikan

| ID | Skenario gagal | Bukti yang diharapkan |
| --- | --- | --- |
| `BIL-AT-135-F` | Penerbitan surat gagal di tengah transaksi pembayaran | **Pembayarannya ikut batal.** Tidak ada keadaan uang tercatat masuk tanpa suratnya |
| `BIL-AT-136-F` | Peristiwa yang sama diproses dua kali karena percobaan ulang | Tepat satu baris efektif per tender per keadaan; percobaan kedua tidak menambah baris |
| `BIL-AT-139-F` | Dua perubahan clearance pada resep yang sama terjadi bersamaan | Tepat satu yang berhasil per nomor versi; yang kalah diulang dengan nomor berikutnya, bukan menimpa |
| `BIL-AT-141-F` | Keadaan clearance ditanyakan untuk resep yang belum pernah punya surat | Dijawab "belum diketahui". **Bukan** galat, dan **bukan** boleh diambil |
| `BIL-AT-142-F` | Tender tunai tanpa identitas shift kasir | Penerbitan ditolak beserta transaksinya; pesan menyebut shift kasir, bukan istilah teknis |

Empat dari lima jalur gagal di atas menguji hal yang sama dari sudut berbeda: **uang dan
suratnya tidak pernah boleh terpisah nasib.** Itu invariant paling mahal bila dilanggar, karena
kerusakannya baru terlihat saat rekonsiliasi bulanan.

Trace `BKC-DEC-106`–`109`, `BKC-DES-036`–`041`, `PHA-DEC-065`, `PHA-DEC-068`, `PHA-DEC-068-A`.


# Amendment 24 September 2026 — Revisi UI Billing (Revisi 1.6, `BIL-TEST-1.5`)

Status: `draft`. Menurunkan langsung dari `BUI-AC-01`–`17` (`00-interview-decisions.md`
amendment 24 September 2026) plus `BUI-VAL-01`–`07` (`contracts/validation-matrix.md`).

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `BUI-DES-005` | Layar Billing dibuka tanpa filter | Integrasi | Query pertama yang terkirim: `StartDate=EndDate=hari ini, Status=OPEN` — bukan query kosong lalu difilter klien |
| `BUI-DES-005` | Filter tanggal diterapkan | Integrasi | Hanya invoice pada rentang itu yang tampil, default hari ini tergantikan |
| `BUI-DES-005` — jalur gagal | Tanggal akhir sebelum tanggal awal | Unit | `BUI-VAL-06` ditolak sebelum request terkirim, pesan menyebut "tanggal akhir tidak boleh sebelum tanggal awal" |
| `BUI-DES-006` | Seluruh label "Drug" pada layar Billing dan turunannya | Regresi visual/snapshot | Terbaca "Obat / Medicine"; nilai data master obat TIDAK berubah |
| `BUI-DES-004` | Modal perbandingan asuransi dibuka | Integrasi | Daftar kandidat hanya berisi `PayerType === "INSURANCE"`; `CASH`/`COMPANY_GUARANTOR` tidak muncul di daftar KANDIDAT (tetap muncul sebagai kategori pemilihan) |
| `CAP-BUI-05` (regresi) | Pasien polis aktif Allianz membuka daftar kandidat | Integrasi | Allianz tidak muncul; provider lain muncul — **memverifikasi ulang** perilaku yang sudah ada, bukan perilaku baru |
| `BUI-DES-003` | Pasien Allianz, SELURUH item tidak tercover, buka Edit Status Tagihan | Integrasi (backend `BUI-DES-001` + frontend) | Tombol "Pribadi" tersorot sebagai default |
| `BUI-DES-003` | Pasien Allianz, SELURUH item tercover, buka Edit Status Tagihan | Integrasi | Tombol "Asuransi" tersorot sebagai default |
| `BUI-DES-001` — **kasus penentu konflik** | Pasien Allianz, SEBAGIAN item tercover (mis. 2 dari 5) | Unit backend | `suggestedBillingStatus == "CASH"` — **ini test yang membuktikan perbaikan `BUI-DEC-014` benar-benar berjalan**, bukan hanya kasus nol/seluruh yang kebetulan sama di kedua aturan |
| `BUI-DES-003` | Tiga tombol payment method dirender | Visual/snapshot | Tersusun satu baris horizontal, bukan grid 2 kolom `.categorySelector` lama |
| `BUI-DES-007` | Card Billing/Status Tagihan pada breakpoint mobile, tablet, desktop | Regresi responsif | Tidak ada elemen terpotong/tumpang tindih pada ketiga breakpoint |
| `BUI-DES-009` | Form Apply Discount tanpa memo diunggah | Unit/integrasi | Tombol submit nonaktif, pesan `BUI-VAL-01` tampil |
| `BUI-DES-009` — jalur gagal | Memo diunggah lalu dihapus (tombol Hapus) sebelum submit | Integrasi | Tombol submit kembali nonaktif — memverifikasi state tidak "nyangkut" tervalidasi |
| `BUI-DES-009` | Memo diunggah, form disubmit | Integrasi | `ApplyDiscountRequest.DoctorDiscountMemoFile` terisi nilai bukan null/kosong |
| `BUI-DES-010`/`011` | Field/card refundable credit lama | Regresi | Tidak tampil di layar mana pun pada modul ini |
| `BUI-DES-011` | Modal refund, sumber "Billing", dua item dicentang | Integrasi | Total refund = penjumlahan `RefundableAmount` kedua item, dihitung otomatis tanpa input manual; kolom Tanggal terisi dari `TransactionDate` |
| `BUI-DES-011` — jalur gagal | Modal refund, sumber "Billing", submit tanpa item dicentang | Unit | `BUI-VAL-03` ditolak sebelum request terkirim |
| `BUI-DES-011` | Modal refund, sumber "Deposito" dipilih | Integrasi | Nominal terisi otomatis sebesar `RemainingDepositAmount`; field tidak dapat diketik manual melebihi itu |
| `BUI-DES-011` — jalur gagal | Modal refund, sumber "Deposito", sisa deposito Rp 0 | Integrasi | Nominal tampil Rp 0; submit ditolak `BUI-VAL-05` (nominal harus > 0) |
| `BUI-DES-012` | Halaman Menu Pembayaran setelah amendment | Regresi | Tombol Ajukan Refund/Adjustment/Write-Off **tidak lagi tampil** |
| `BUI-DES-012` | Halaman Riwayat Pembayaran setelah amendment | Integrasi | Ketiga tombol tampil pada area/kolom Aksi, memicu modal/alur yang sama seperti sebelum dipindah |
| `BUI-DES-012` — jalur gagal | Kolom "Kwitansi" existing pada Riwayat Pembayaran | Regresi | Perilaku cetak struk tidak berubah sedikit pun setelah kolom Aksi ditambahkan |
| Pemisahan wewenang (`contracts/permission-audit-matrix.md`) | Peran tanpa butir akses refund mencoba mengklik tombol di lokasi baru | Integrasi | Ditolak — butir akses yang sama persis dengan lokasi lama, bukan wewenang baru yang lebih longgar |

**Cakupan yang sengaja belum diuji pada amendment ini** (`OPEN DECISION`, tidak memblokir
gelombang lain): skenario Catatan Penting (`BUI-DEC-009`) dan skenario upload memo end-to-end
lewat endpoint sungguhan (`BUI-DEC-010` — baru bisa diuji setelah `BUI-CQ-06` terjawab dan
endpoint upload ada; test validasi wajib di atas sudah cukup untuk membuktikan PERILAKU
FRONTEND tanpa endpoint sungguhan, memakai upload tiruan/mock).

Trace `BUI-DEC-001`–`015`, `BUI-DES-001`–`012`, `BUI-AC-01`–`17`, `BUI-VAL-01`–`07`.

---

# Amendment 29 September 2026 — Pengujian Pembalikan Tender Top-Up Deposit dan Alokasi Tagihan (Revisi 1.8, `BIL-TEST-1.7`)

`last_changed_in: BIL-TEST-1.7` · status **draft** · input `BKC-DEC-128`–`134`, `BKC-DES-051`–`056`, `BIL-VAL-127`–`133`, `BIL-INT-018`.

| ID | Requirement | Skenario | Jenis Test | Bukti yang Diharapkan |
|---|---|---|---|---|
| `BIL-AT-149` | `BKC-DEC-128`, `BKC-DES-051` | Tender top-up dibalik saat saldo deposit masih utuh (belum ada alokasi tagihan) | Integrasi / Service | Tepat satu mutasi `REVERSAL` tercatat dengan `ReversesMovementId` menunjuk ke mutasi `TOP_UP` awal; `AvailableBalance` dipotong tepat sebesar nilai top-up; saldo akhir >= 0; nol alokasi yang dibatalkan. |
| `BIL-AT-150` | `BKC-DEC-128`, `BKC-DEC-130` | Tender top-up dibalik saat dana telah terpakai melunasi tagihan invoice (`CLOSED`) | Integrasi / End-to-End | Alokasi invoice dibatalkan sebesar defisit via baris pembalik `BilPaymentAllocation` ber-`ReversesAllocationId`; mutasi `RELEASE` tercatat; saldo akun pulih lalu dipotong via mutasi `REVERSAL`; `AvailableBalance` akhir tetap >= 0 (tidak minus); status invoice otomatis berubah dari `CLOSED` kembali ke `FINAL`. |
| `BIL-AT-151` | `BKC-DEC-129`, `BKC-DES-052` | Tender top-up dibalik saat dana terpakai pada beberapa alokasi invoice (LIFO Test) | Unit / Integrasi | Alokasi invoice yang paling akhir dibuat (`AllocatedAt DESC`) dibatalkan terlebih dahulu; jika defisit belum tertutup, alokasi sebelumnya dibatalkan berurutan sampai total pembalikan terpenuhi; alokasi yang lebih lama tetap utuh jika defisit sudah tertutup. |
| `BIL-AT-152` | `BKC-DEC-130`, `BKC-DES-053` | Penyelarasan status invoice via `SyncClosureAsync` pasca-pembatalan alokasi | Integrasi | Invoice terdampak yang sebelumnya `CLOSED` bertransisi kembali ke `FINAL`; `ClosedAt` menjadi null; sisa tagihan pasien (`PatientOutstanding`) naik kembali sebesar alokasi yang dibatalkan; alasan clearance mencatat `PrescriptionClearanceReasonCodes.PaymentReversed`. |
| `BIL-AT-153` | `BKC-DEC-131`, `BKC-DES-054` | Integrasi ke Finance Management via fakta `DEPOSIT_MOVEMENT` | Integrasi Lintas Modul | Mutasi `RELEASE` dan `REVERSAL` yang tercatat pada `BilDepositMovement` dapat dibaca dan dikonsumsi oleh `FinanceBillingIntakeService` untuk membentuk kejadian akuntansi `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` dan `PEMBALIKAN-PENERIMAAN-UANG-MUKA`. |
| `BIL-AT-154` | `BKC-DEC-130`, `BKC-DES-051` | Jalur gagal: kegagalan transaksi di tengah proses pembalikan | Ketahanan / Transaksi | Bila terjadi galat pada tahap pembatalan alokasi atau penulisan mutasi, seluruh transaksi database di-rollback secara atomik; saldo deposit dan status tender tetap pada keadaan sebelum reversal dipicu. |
| `BIL-AT-155` | `BIL-INT-018` | Idempotensi penanganan webhook tender `REVERSED` berulang | Integrasi / Idempotensi | Panggilan kedua dengan `tenderId` dan status `REVERSED` yang sama bersifat no-op (*idempotent*); tidak ada mutasi `RELEASE` atau `REVERSAL` duplikat yang tercatat. |
| `BIL-AT-156` | `BIL-VAL-129` | Jalur gagal: tender belum pernah `SUCCEEDED` bertransisi ke `REVERSED` | Guard / Validasi | Transisi ditolak atau diabaikan tanpa menulis mutasi deposit apa pun; membuktikan hanya tender yang benar-benar pernah menambah saldo yang dapat dibalikkan saldonya. |
| `BIL-AT-157` | `BKC-DEC-132`, `BKC-DEC-133`, `BKC-DES-055`, `BIL-VAL-132` | Penanda eksplisit `ReversesMovementId` dan granularitas 1-ke-1 mutasi `RELEASE` | Unit / Integrasi | Bila pembatalan LIFO membatalkan $N$ alokasi tagihan ($N \ge 1$), terbit tepat $N$ baris mutasi `RELEASE` di `BilDepositMovement`. Setiap baris memiliki `ReversesMovementId` yang cocok dengan ID mutasi `ALLOCATION` asalnya, `SettlementId` alokasi terkait, dan `Amount` cocok persis. Nol mutasi `RELEASE` ber-`ReversesMovementId == null`. |
| `BIL-AT-158` | `BKC-DEC-134`, `BKC-DES-056`, `BIL-VAL-133` | Penyelarasan ringkasan deposit (`totalRefunded`) dan efek saldo mutasi rekening di `BillingDepositService` | Unit / Service | (1) `GetEpisodeDepositSummaryAsync` menghasilkan `TotalRefunded == 0` bila hanya ada mutasi `RELEASE` pembatalan alokasi (`ReversesMovementId != null`); (2) `GetDepositStatementAsync` menghasilkan `BalanceEffect == +Amount` untuk mutasi `RELEASE` pembatalan alokasi, memulihkan running balance secara tepat sebelum mutasi `REVERSAL` menariknya. |

Trace `BKC-DEC-128`–`134`, `BKC-DES-051`–`056`, `BIL-VAL-127`–`133`, `BIL-INT-018`.

---

## Amendment 13 Oktober 2026 — Pelunasan Gabungan Lintas Kunjungan (Revisi 1.9)

> `last_changed_in`: `BIL-TEST-1.7` atas `BIL-TEST-1.6` — status **approved** (14 Oktober 2026).
> Owner: Billing Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-118`, `BKC-DEC-142`, `BKC-DEC-143`, `BKC-AC-091`, `BKC-DES-057`–`BKC-DES-062`, `BIL-VAL-134`–`140`,
> `BIL-INT-019`. Backend SHA: `e049a65b`.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-159` | `FR-BKC-251` | **Jalur berhasil.** Tagihan IGD bersisa Rp 1.500.000 dan tagihan ranap bersisa Rp 8.000.000; keluarga membayar Rp 6.000.000 gabungan | Unit + integrasi | Tagihan IGD teralokasi tepat Rp 1.500.000 dan berstatus tertutup; tagihan ranap teralokasi Rp 4.500.000 dan tetap terbuka bersisa Rp 3.500.000; transaksi berstatus `PARTIALLY_SETTLED` |
| `BIL-AT-160` | `FR-BKC-251` | **Jalur berhasil.** Uang cukup menutup seluruh tagihan peserta | Unit | Seluruh peserta berstatus `ALLOCATED`; transaksi berstatus `SETTLED`; setiap invoice tertutup sendiri-sendiri |
| `BIL-AT-161` | `FR-BKC-252` | **Urutan alokasi.** Tiga tagihan terbit 9 Okt pagi, 9 Okt sore, dan 10 Okt; uang hanya menutup dua tagihan pertama | Unit | `AllocationSequence` bernilai 1, 2, 3 mengikuti tanggal terbit; tagihan ketiga berstatus `SKIPPED` dengan nol alokasi |
| `BIL-AT-162` | `FR-BKC-253` | **Jalur gagal.** Kasir mengirim hanya satu tagihan sebagai peserta | Unit | Ditolak kode `422` dengan pesan `BIL-VAL-134`; nol baris peserta dan nol alokasi tertulis |
| `BIL-AT-163` | `FR-BKC-253` | **Jalur gagal.** Salah satu tagihan peserta milik pasien berbeda | Unit | Ditolak kode `422` dengan pesan `BIL-VAL-136`; nol baris tertulis |
| `BIL-AT-164` | `FR-BKC-254` | **Jalur gagal.** Biaya susulan masuk setelah daftar ditampilkan sehingga sisa tagihan berubah | Integrasi | Ditolak kode `409` dengan pesan `BIL-VAL-137`; nol alokasi tertulis; kasir dapat memuat ulang lalu berhasil memproses |
| `BIL-AT-165` | `FR-BKC-255` | **Jalur gagal.** Tagihan yang sama dimasukkan ke dua transaksi gabungan hampir bersamaan | Integrasi (konkurensi) | Transaksi kedua ditolak kode `409` dengan pesan `BIL-VAL-138`; hanya satu transaksi yang memegang tagihan itu; nol alokasi ganda |
| `BIL-AT-166` | `FR-BKC-256` | **Pembayaran terpisah tetap bekerja.** Keluarga memilih membayar tagihan IGD saja lewat jalur satu tagihan | Regresi | Pembayaran berhasil tanpa membentuk grup; tagihan ranap tetap terbuka apa adanya; nol baris `BilSettlementInvoice` tertulis |
| `BIL-AT-167` | `FR-BKC-257` | **Kwitansi per unit.** Satu transaksi gabungan dicetak kwitansinya | Integrasi | Kwitansi memuat satu baris per unit layanan (`IGD` dan `RANAP`) beserta nominal masing-masing; total kwitansi sama dengan uang yang benar-benar diterima |
| `BIL-AT-168` | `FR-BKC-258` | **Non-destruktif.** Setelah pembayaran gabungan, rincian item IGD diperiksa | Integrasi | Setiap item IGD tetap ber-`SourceDomain = "EMERGENCY"` pada invoice asalnya; nol item berpindah invoice; `BilInvoice.EncounterId` kedua invoice tetap berbeda dan tetap unik |

### Yang belum dapat diuji dan sebabnya

| Hal | Sebab | Penggantinya sementara |
|---|---|---|
| Uji ujung-ke-ujung di peramban | Belum ada satu pun spesifikasi end-to-end Billing, dan environment ter-autentikasi belum tersedia — keadaan yang sudah tercatat sejak `FE-BKC-010` | Uji integrasi tingkat service ditambah pemeriksaan manual ter-autentikasi oleh kasir saat UAT |
| Interaksi dengan kelayakan pemulangan | **Sudah diputuskan** lewat `BKC-DEC-144` (14 Oktober 2026) dan dirancang pada amendment revisi `2.0` | Skenario ujinya ada di amendment revisi `2.0` (`BIL-AT-169`–`BIL-AT-178`) |
| Rekonsiliasi penerimaan kas oleh Finance | Menunggu `BKC-OQ-111` | Pemeriksaan bahwa baris alokasi per invoice benar-benar tertulis, sebagai bahan rekonsiliasi |

Trace `BKC-DEC-118`, `BKC-DEC-142`, `BKC-DEC-143`, `BKC-AC-091`, `BKC-DES-057`–`BKC-DES-062`, `BIL-VAL-134`–`140`, `BIL-INT-019`.




---

## Amendment 14 Oktober 2026 — Kelayakan Pulang Lintas Kunjungan (Revisi 2.0)

> `last_changed_in`: `BIL-TEST-1.8` atas `BIL-TEST-1.7` — status **approved** (14 Oktober 2026).
> Owner: Billing Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-144`, `BKC-AC-094`, `BKC-DES-063`–`BKC-DES-069`, `BIL-VAL-141`–`146`,
> `BIL-INT-020`. Backend SHA: `d166cf4f`.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-169` | `FR-BKC-259` | **Jalur berhasil.** Rantai Rawat Jalan → IGD → Rawat Inap; sisa Rp 3.500.000 (ranap), Rp 0 (IGD, sudah lunas), Rp 250.000 (rajal) | Integrasi | Status `BLOCKED` dengan reason code `CROSS_ENCOUNTER_OUTSTANDING`; sisa total Rp 3.750.000; tiga baris rincian dengan `ChainLevel` 0, 1, 2 |
| `BIL-AT-170` | `FR-BKC-259` | **Jalur berhasil.** Seluruh tagihan dalam rantai lunas | Integrasi | Status `CLEARED`; sisa total nol; `ChainInvoiceCount` sesuai jumlah tagihan yang ditemukan |
| `BIL-AT-171` | `FR-BKC-260` | **Anti-hitung-ganda.** Biaya admin rawat jalan sudah dibayar lalu dialihkan menjadi kredit pada tagihan rawat inap (`BKC-DEC-119`) | Unit + integrasi | Nominal admin itu **hanya** mengurangi tagihan rawat inap, dan **tidak** muncul lagi sebagai kewajiban terbuka pada tagihan rawat jalan. Selisih total terhadap perhitungan manual bernilai nol rupiah |
| `BIL-AT-172` | `FR-BKC-261` | **Jalur gagal.** Rantai rujukan berputar: kunjungan A menunjuk B, B menunjuk A | Unit | Penelusuran berhenti tanpa menggantung; kode anomali `CYCLE_DETECTED`; status `NEEDS_RECONCILIATION`; nol tagihan terhitung dua kali |
| `BIL-AT-173` | `FR-BKC-261` | **Jalur gagal.** Rantai lebih panjang dari batas kedalaman | Unit | Kode anomali `DEPTH_LIMIT_REACHED`; status `NEEDS_RECONCILIATION`; penilaian **tetap** menghasilkan angka dari tagihan yang sudah ditemukan, bukan galat dan bukan nol |
| `BIL-AT-174` | `FR-BKC-262` | **Jalur gagal.** Kunjungan asal ternyata milik pasien lain, tagihannya bersisa Rp 900.000 | Unit | Rp 900.000 **tidak** masuk total; baris rincian tercatat dengan `IsCounted` bernilai `false` dan alasan terisi; kode anomali `PATIENT_MISMATCH`; status `NEEDS_RECONCILIATION` |
| `BIL-AT-175` | `FR-BKC-263` | **Privasi.** Proyeksi daftar penghambat yang dibaca perawat bangsal diperiksa | Integrasi | Response perawat memuat unit layanan dan penanda penghambat, dan **nol** field bernominal rupiah. Response kasir memuat rupiah. Keduanya dari sumber yang sama |
| `BIL-AT-176` | `FR-BKC-264` | **Rekonsiliasi.** Petugas menyelesaikan surat `NEEDS_RECONCILIATION` dengan mengecualikan satu tagihan beserta alasan | Integrasi | Status berpindah ke `BLOCKED` atau `CLEARED` sesuai hitung ulang; alasan dan identitas petugas tersimpan; surat versi sebelumnya tetap dapat dibaca utuh |
| `BIL-AT-177` | `FR-BKC-265` | **Jalur gagal.** Rekonsiliasi dicoba pada surat yang statusnya `CLEARED` | Unit | Ditolak kode `422`; nol perubahan status; nol baris audit keputusan |
| `BIL-AT-178` | `FR-BKC-266` | **Regresi.** Pasien tanpa kunjungan asal sama sekali, hanya ada tagihan rawat inap | Regresi | Perilaku **identik** dengan sebelum amendment: status dan sisa tagihan sama seperti penilaian satu tagihan; `ChainInvoiceCount` bernilai `1`; nol anomali |

### Yang belum dapat diuji dan sebabnya

| Hal | Sebab | Penggantinya sementara |
|---|---|---|
| Uji ujung-ke-ujung di peramban | Belum ada spesifikasi end-to-end Billing dan environment ter-autentikasi belum tersedia — keadaan yang sudah tercatat sejak `FE-BKC-010` | Uji integrasi tingkat service ditambah pemeriksaan manual ter-autentikasi saat UAT |
| Konsumsi status `NEEDS_RECONCILIATION` di layar bangsal | Menunggu `BKC-OQ-121`, yaitu persetujuan pemilik Rawat Inap | Pemeriksaan bahwa proyeksi perawat benar bentuknya (`BIL-AT-175`), tanpa menguji tampilannya di bangsal |
| Kewenangan Action `Reconcile` per peran | **Terjawab** lewat `BKC-DEC-150` | Diuji pada amendment revisi `2.1` (`BIL-AT-185`): Kasir dan Supervisor Kasir berhasil, Petugas Billing dan Finance/AR ditolak `403` |
| Kebenaran isi alasan rekonsiliasi | Tidak dapat diuji mesin | Jejak audit diperiksa manual saat tinjauan berkala |

Trace `BKC-DEC-144`, `BKC-AC-094`, `BKC-DES-063`–`BKC-DES-069`, `BIL-VAL-141`–`146`, `BIL-INT-020`.


---

## Amendment 14 Oktober 2026 — Keputusan Rantai Permanen dan Perbaikan Constraint (Revisi 2.1)

> `last_changed_in`: `BIL-TEST-1.9` atas `BIL-TEST-1.8` — status **approved** (14 Oktober 2026).
> Owner: Billing Owner · `approved_by`: Pemilik modul Billing (Product/Domain Owner), pernyataan langsung dalam sesi · `approved_at`: 2026-10-14.
> Masukan: `BKC-DEC-150`, `BKC-DEC-151`, `BKC-DES-070`–`BKC-DES-076`, `BIL-VAL-147`–`150`.
> Backend SHA: `d166cf4f`.

| ID | Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-179` | `FR-BKC-267` | **Permanen.** Kunjungan dikecualikan pada surat versi 3, lalu tagihan susulan memicu versi 4 | Integrasi | Versi 4 **tidak** memuat kejanggalan atas kunjungan itu; baris rinciannya tercatat tidak dihitung dengan alasan keputusan permanen. Status `BLOCKED` atau `CLEARED`, bukan `NEEDS_RECONCILIATION` |
| `BIL-AT-180` | `FR-BKC-268` | **Memutus penelusuran.** Kunjungan tingkat 2 dikecualikan, dan ia punya kunjungan asal tingkat 3 bersisa Rp 400.000 | Unit | Kunjungan tingkat 3 **tidak** muncul pada rincian dan Rp 400.000 **tidak** masuk total |
| `BIL-AT-181` | `FR-BKC-267` | **Mengikutkan permanen.** Rujukan berputar diikutkan pada versi 3; versi 4 dinilai | Integrasi | Kejanggalan `CYCLE_DETECTED` atas kunjungan itu **tidak** muncul lagi. Kejanggalan **baru** pada kunjungan lain tetap muncul |
| `BIL-AT-182` | `FR-BKC-269` | **Jalur gagal.** Keputusan kedua untuk kunjungan dan kejanggalan yang sama, `IdempotencyKey` berbeda | Integrasi (konkurensi) | Ditolak `409` (`BIL-VAL-147`); nol baris kedua. Dengan `IdempotencyKey` yang sama: `200` dan hasil identik |
| `BIL-AT-183` | `FR-BKC-269` | **Jalur gagal.** Membalik keputusan mengikutkan menjadi mengecualikan | Unit | Ditolak `422` (`BIL-VAL-149`); nol perubahan |
| `BIL-AT-184` | `FR-BKC-270` | **Jalur gagal.** Mengikutkan kunjungan `PATIENT_MISMATCH` | Unit + basis data | Ditolak `422` oleh service (`BIL-VAL-148`). Bila service dilewati, basis data menolak lewat `CK_BilClearanceChainDecision_NoIncludeOnPatientMismatch` |
| `BIL-AT-185` | `FR-BKC-271` | **Hak akses.** Kasir, Supervisor Kasir, Petugas Billing, dan Finance/AR masing-masing mencoba menetapkan keputusan | Integrasi | Kasir dan Supervisor Kasir berhasil; Petugas Billing dan Finance/AR ditolak `403`. Keempatnya dapat membaca riwayat keputusan |
| `BIL-AT-186` | `FR-BKC-267` | **Kunjungan tanpa invoice.** Kejanggalan `MISSING_INVOICE` diputuskan | Integrasi | Keputusan tersimpan dengan `TargetInvoiceId` kosong; penilaian berikutnya tidak menandainya lagi |
| `BIL-AT-187` | `FR-BKC-272` | **Perbaikan cacat.** Kasir menekan evaluasi ulang kelayakan | Integrasi + regresi | Surat baru tersimpan dengan reason code `REEVALUATED_BY_CASHIER`, nol pelanggaran `CHECK`. Test ini **MUST** ditulis gagal lebih dulu di environment yang belum diperbaiki, untuk membuktikan cacatnya nyata |
| `BIL-AT-188` | `FR-BKC-269` | **Penjaga tanda hapus.** Baris keputusan ditandai hapus langsung di basis data, lalu keputusan baru untuk pasangan yang sama dikirim | Basis data | Keputusan baru tetap ditolak indeks unik, karena indeks tidak memfilter `IsDelete` |

### Yang belum dapat diuji dan sebabnya

| Hal | Sebab | Penggantinya sementara |
|---|---|---|
| Bukti runtime cacat `REEVALUATED_BY_CASHIER` | Temuan baru berasal dari pembacaan source; pemilik meminta verifikasi di lingkungan nyata lebih dulu | `BIL-AT-187` ditulis gagal lebih dulu, atau verifikasi lewat `/diagnose-module-issue` |
| Kebenaran isi alasan keputusan | Tidak dapat diuji mesin | Tinjauan jejak audit berkala |

Trace `BKC-DEC-150`, `BKC-DEC-151`, `BKC-DES-070`–`BKC-DES-076`, `BIL-VAL-147`–`150`.



---

## Amendment 14 Oktober 2026 — Tautan Kunjungan Asal Jalur Transfer dan Lingkup Penggantian Biaya Admin (Revisi 2.2)

> `last_changed_in`: `BIL-TEST-2.0` atas `BIL-TEST-1.9` — status **draft**.
> Owner: Billing Owner · `approved_by`: belum · `approved_at`: belum.
> Masukan: `BKC-AC-106`–`BKC-AC-110`, `BKC-DES-077`–`BKC-DES-081`. Backend SHA: `d268a41f`.
> Jenis "Integrasi" dan "Manual" berarti verifikasi runtime dan bukti tercatat; backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

| ID | Requirement | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-189` | `FR-BKC-273` | Pasien IGD dengan tagihan Rp 1.500.000 ditransfer, lalu rawat inap dikonfirmasi | Integrasi | Satu tautan `EMERGENCY_ORIGIN`, `SourceReferralId` kosong (`BKC-AC-106`) |
| `BIL-AT-190` | `FR-BKC-273` | Pasien poli ditransfer, lalu rawat inap dikonfirmasi | Integrasi | Satu tautan `OUTPATIENT_ORIGIN` (`BKC-AC-107`) |
| `BIL-AT-191` | `FR-BKC-273` | Event `ADMISSION_CONFIRMED` yang sama diproses dua kali | Integrasi | Tetap satu tautan (`BKC-AC-108`) |
| `BIL-AT-192` | `FR-BKC-273` | Pasien dirawat inap langsung tanpa transfer | Integrasi | Nol tautan; rincian tagihan identik dengan sebelumnya (`BKC-AC-109`) |
| `BIL-AT-193` | `FR-BKC-274` | Kunjungan asal transfer ternyata milik pasien lain (data disiapkan langsung) | Integrasi | Nol tautan; satu peringatan `InpatientEvent.TransferLinkSkipped` tanpa nama atau nomor rekam medis; invoice rawat inap tetap terbuka |
| `BIL-AT-194` | `FR-BKC-273` | Kunjungan asal yang sama juga punya rujukan operasi | Integrasi | Satu tautan saja, sebab `SURGERY_ORIGIN` |
| `BIL-AT-195` | `FR-BKC-275` | Transfer lama tanpa tautan; event `ADMISSION_CONFIRMED`-nya diputar ulang | Integrasi | Tautan tertulis memakai tanda terima asli; tidak ada tanda terima baru |
| `BIL-AT-196` | `FR-BKC-276` | Transfer lama tanpa tautan dinilai kelayakannya | Integrasi | `UNLINKED_TRANSFER_ORIGIN`, status `NEEDS_RECONCILIATION`, **bukan** `CLEARED`; tagihan asal tampil dengan `isCounted` `false` |
| `BIL-AT-197` | `FR-BKC-276` | Kasir mengikutkan kejanggalan `UNLINKED_TRANSFER_ORIGIN`, lalu mencoba mengecualikannya | Integrasi | Tagihan asal dihitung; percobaan membalik ditolak dan nol baris keputusan berubah (`BKC-AC-110`) |
| `BIL-AT-198` | `FR-BKC-276` | Asal transfer tanpa tautan milik pasien lain | Integrasi | Hanya `PATIENT_MISMATCH` yang muncul; `INCLUDE` ditolak |
| `BIL-AT-199` | `FR-BKC-277` | Pasien dengan kunjungan poli Maret yang sudah dibayar, lalu poli Oktober yang ditransfer dan sudah dibayar | Integrasi | Kredit alihan Rp 150.000 dari kunjungan Oktober saja |
| `BIL-AT-200` | `FR-BKC-277` | Pasien dengan kunjungan poli terbuka yang tidak tertaut, lalu dirawat inap langsung | Integrasi | Nol item di-void, nol kredit |
| `BIL-AT-201` | `FR-BKC-273` | Ringkasan tagihan pasien rawat inap sesudah transfer IGD | Manual | Item tagihan IGD ikut tergabung pada rincian |

> **[Revisi `2.3`]** `BIL-AT-199` dan `BIL-AT-200` **superseded** bersama `FR-BKC-277`. Keduanya menguji jalur void/kredit
> item yang dihapus `BKC-DES-083`. Penggantinya `BIL-AT-205` dan `BIL-AT-208`.

---

## Amendment 16 Oktober 2026 — Penggantian Admin Rawat Jalan, Backfill Tautan, dan Tinjauan Void Keliru (Revisi 2.3)

> `last_changed_in`: `BIL-TEST-2.1` atas `BIL-TEST-2.0` — status **draft**.
> Masukan: `BKC-DES-083`–`BKC-DES-089`, `BIL-VAL-151`–`BIL-VAL-160`, arahan A1–A3. Backend SHA: `738e03a0`.
> Data contoh memakai nama samaran. Angka dasar: admin poli Rp 150.000 (porsi pasien), admin rawat inap Rp 1.000.000,
> porsi pasien rawat inap sebelum kredit Rp 3.650.000.

### Hentikan jalur lama (`FR-BKC-278`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-202` | `FR-BKC-278` | Pasien punya tagihan poli lama yang lunas berisi item "Administrasi Rekam Medis Rp 25.000"; tagihan rawat inapnya dihitung ulang | Integrasi (PostgreSQL, bukan InMemory, agar `CHECK` ikut diuji) | Kalkulasi **berhasil**; nol kredit, nol item di-void. Regresi F10 |
| `BIL-AT-203` | `FR-BKC-278` | Pratinjau kalkulasi invoice rawat inap dan invoice poli dibuka tiga kali | Integrasi | Jumlah baris dan isi `BilInvoiceItem`, `BilRefundableCredit`, `BilCalculationVersion` identik sebelum dan sesudah (`BIL-VAL-159`). Regresi F11 |

### Penggantian admin (`FR-BKC-279`–`282`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-204` | `FR-BKC-279` | Pasien poli ditransfer resmi; tagihan poli masih `OPEN`, lalu dihitung | Integrasi | Admin poli Rp 0; `isSupersededByInpatientAdmission = true`; kode `SUPERSEDED_BY_INPATIENT_ADMISSION`. Tagihan rawat inap: `treatment = SUPERSEDED`, kredit Rp 0 |
| `BIL-AT-205` | `FR-BKC-280` | Seperti `BIL-AT-204`, tetapi tagihan poli sudah `CLOSED` | Integrasi | Kredit Rp 150.000; porsi pasien rawat inap Rp 3.500.000 (`BKC-AC-096`); `ReferredOutpatientAdminCreditAmount = 150.000` |
| `BIL-AT-206` | `FR-BKC-280` | Tagihan poli `FINAL` dengan sisa piutang Rp 150.000 | Integrasi | Kredit penuh Rp 150.000 di rawat inap; sisa piutang poli **tidak** berubah |
| `BIL-AT-207` | `FR-BKC-280` | Tagihan poli `SETTLED_BY_WRITE_OFF` dengan alokasi bersih Rp 60.000 | Integrasi | Kredit Rp 60.000 (nilai terkecil antara porsi admin dan alokasi bersih) |
| `BIL-AT-208` | `FR-BKC-282` | Pasien ke poli kemarin untuk urusan lain (lunas, **tidak** tertaut), lalu masuk rawat inap lewat IGD | Integrasi | Kredit Rp 0; admin rawat inap Rp 1.000.000 utuh, **tidak** dipotong Rp 50.000. Regresi F12 |
| `BIL-AT-209` | `FR-BKC-281` | Seperti `BIL-AT-205`, tetapi rawat inap ditanggung penuh BPJS (porsi pasien Rp 0), lalu difinalisasi dua kali (finalisasi, buka kembali, finalisasi) | Integrasi | Versi: kredit terpakai Rp 0, sisa Rp 150.000; porsi pasien Rp 0, bukan negatif (`BIL-VAL-160`). Finalisasi pertama melahirkan **satu** kredit `REFERRED_OUTPATIENT_ADMIN` Rp 150.000 dengan `SourceId` = id invoice rawat inap; finalisasi kedua tidak membuat baris kedua |
| `BIL-AT-210` | `FR-BKC-281` | Kredit `REFERRED_OUTPATIENT_ADMIN` sudah ada; kelayakan pulang dinilai | Integrasi | Outstanding kelayakan = porsi pasien − pembayaran; kredit refund **tidak** ikut mengurangi (dihitung sekali saja) |
| `BIL-AT-211` | `FR-BKC-279`, `FR-BKC-280` | Tagihan poli dihitung ulang sesudah tautan lahir (admin Rp 0), lalu dibayar sampai `CLOSED`; tagihan rawat inap dihitung | Integrasi | Kredit Rp 0, karena porsi admin pada versi terkini poli sudah 0 (invariant 2: tidak dobel) |
| `BIL-AT-212` | `FR-BKC-279` | Seperti `BIL-AT-204`, lalu invoice rawat inap dibatalkan; tagihan poli dihitung lagi | Integrasi | Admin poli kembali Rp 150.000; `isSupersededByInpatientAdmission = false` |
| `BIL-AT-213` | `FR-BKC-282` | Dua tagihan poli pada hari yang sama, tanpa rawat inap | Integrasi | Perilaku **sama persis** dengan sebelum revisi (`BKC-DEC-017` tidak disentuh). Uji ini mengunci perilaku *as-is* sampai `BKC-OQ-138` |

### Backfill (`FR-BKC-283`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-214` | `FR-BKC-283` | Dry-run pada data berisi satu transfer sah, satu asal milik pasien lain, satu asal batal, satu tanpa invoice rawat inap, dan satu yang sudah tertaut | Integrasi | Kandidat 1; terlewat `PATIENT_MISMATCH` 1, `SOURCE_DELETED_OR_CANCELLED` 1, `NO_RANAP_INVOICE` 1, `ALREADY_LINKED` 1. Nol baris tertulis |
| `BIL-AT-215` | `FR-BKC-283` | Migration dijalankan dua kali | Integrasi | Satu tautan; `ReceiptId` = tanda terima `ADMISSION_CONFIRMED` paling awal; `LinkedAt` = `ReceivedAtUtc` tanda terima itu, **bukan** waktu jalan; `CreateBy` = penanda backfill; jalan kedua menulis nol baris |
| `BIL-AT-216` | `FR-BKC-283` | `Down` dijalankan sesudah backfill | Integrasi | Hanya baris berpenanda yang hilang; tautan runtime dan rujukan operasi utuh |
| `BIL-AT-217` | `FR-BKC-283` | Sesudah backfill, penelusur rantai `MVP-37` menilai pasien transfer lama | Integrasi | Tidak ada `UNLINKED_TRANSFER_ORIGIN` untuk transfer yang tertaut lewat backfill |

### Tinjauan void keliru (`FR-BKC-284`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-218` | `FR-BKC-284` | Supervisor Kasir memulihkan item `VOIDED` pada tagihan `OPEN` | Integrasi | Item lama tetap `VOIDED` tanpa perubahan kolom; item salinan `ACTIVE` dengan `SourceVersion` + 1; satu baris tinjauan `REINSTATE`; `RowVersion` invoice berubah; kalkulasi berikutnya memuat item itu |
| `BIL-AT-219` | `FR-BKC-284` | `REINSTATE` pada tagihan `CLOSED`; lalu `NEEDS_POST_CLOSE_CORRECTION` pada item yang sama | Integrasi | Pertama `422` (`BIL-VAL-154`); kedua `201`; tagihan tidak berubah |
| `BIL-AT-220` | `FR-BKC-284` | Keputusan kedua untuk item yang sama; dan permintaan ulang dengan `Idempotency-Key` yang sama | Integrasi | `409` (`BIL-VAL-153`); replay `200` dengan `isReplay = true` |
| `BIL-AT-221` | `FR-BKC-284` | Kasir biasa dan Petugas Billing memanggil `POST .../decision` | Integrasi | `403` |
| `BIL-AT-222` | `FR-BKC-284` | `REINSTATE` ketika sudah ada item `ACTIVE` dengan kunci sumber sama | Integrasi | `409` (`BIL-VAL-156`); nol baris tertulis |
| `BIL-AT-223` | `FR-BKC-284` | Alasan 5 karakter | Unit | `400` (`BIL-VAL-151`) |
| `BIL-AT-224` | `FR-BKC-284` | Periksa log audit sesudah `BIL-AT-218` | Integrasi | Ada `BillingSupersededItemReview.Decide`; payload tidak memuat nama, NIK, maupun nomor rekam medis |

### Frontend (`FR-BKC-285`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| `BIL-AT-225` | `FR-BKC-285` | Menu Pembayaran tagihan poli yang digantikan | Manual | Baris biaya admin Rp 0 berlabel "Digantikan rawat inap" dengan nomor tagihan rawat inap |
| `BIL-AT-226` | `FR-BKC-285` | Panel ringkasan rawat inap dengan satu asal `CREDITED` dan sisa kredit | Manual | Kredit terpakai, daftar asal, dan keterangan "akan menjadi kredit refund saat finalisasi" tampil; tidak ada lagi indikator "void" |
| `BIL-AT-227` | `FR-BKC-285` | `BIL-SCR-45`: daftar kosong, gagal muat, dan tombol yang tidak diizinkan status invoice | Manual | Pesan kosong dan gagal sesuai `03-frontend-architecture.md`; tombol di luar `allowedDecisions` tidak tampil |

### Adendum item tarif berkategori admin (`BKC-DES-092`)

| ID | FR | Skenario | Jenis | Hasil yang diharapkan |
|---|---|---|---|---|
| ~~`BIL-AT-228`~~ | — | **Dicabut** bersama `BKC-DES-092` (`BKC-DEC-167`) | — | — |
| ~~`BIL-AT-229`~~ | — | **Dicabut** bersama `BKC-DES-092` (`BKC-DEC-167`) | — | — |
| `BIL-AT-230` | `FR-BKC-279` | Tagihan poli tertaut memuat item **bukan** berkategori admin tetapi bernama "Administrasi Rekam Medis" | Integrasi | Item tetap dihitung penuh. Kata pada deskripsi tidak berpengaruh |

Seluruh uji integrasi yang menyentuh `CHECK` atau transaksi **MUST** berjalan di PostgreSQL. F10 lolos selama ini karena
nol test, dan InMemory tidak menegakkan `CHECK`.
