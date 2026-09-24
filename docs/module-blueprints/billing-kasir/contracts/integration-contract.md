# Billing dan Kasir — Integration Contract

`contract_version: BIL-INTEGRATION-0.4` · status **approved** · owner masing-masing producer + Billing + AR/AP · approved 20 Agustus 2026. Transport final boleh in-process/outbox/message, tetapi semantics berikut terkunci.

| ID | Producer → Consumer | Trigger/payload minimum | Idempotency | Failure/retry | Security/privacy |
| --- | --- | --- | --- | --- | --- |
| `BIL-INT-001` | Registration → Billing | encounter opened/transfer; EncounterId, patient ref, service type, payer context | EncounterId+version | retry; satu invoice | ID sensitif, least privilege |
| `BIL-INT-002` | Clinical/Lab/Radiology → Billing | order billable/completed/cancelled; SourceDomain, SourceDetailId, qty, status, timestamps | source tuple | duplicate no-op; out-of-order version check | tanpa clinical narrative |
| `BIL-INT-003` | Pharmacy → Billing | dispensed final; actual qty, item/tariff ref | dispense detail ID | correction sebagai event baru | item obat minimum |
| `BIL-INT-004` | Inpatient/Bed → Billing | occupancy timeline/transfer/correction | occupancy segment+version | reject overlap; adjustment after posting | room/episode ref |
| `BIL-INT-005` | Pricing/Coverage → Billing | effective tariff/share/primary/excess result | policy/version ID | snapshot calculation; recalc while open | contract detail dibatasi |
| `BIL-INT-006` | Payment Provider → Billing | attempt result/reference/status/time | provider reference+idempotency key | timeout remains pending; reconciliation callback | token/credential dilarang log |
| `BIL-INT-007` | Billing → AR | per debtor, amount, invoice/due date, finalization/version | handoff key | at-least-once safe; ack stored | debtor sensitive |
| `BIL-INT-008` | Billing → AP | doctor, share amount, readiness policy/status | handoff key | at-least-once safe | doctor ID sensitive |
| `BIL-INT-009` | Billing → AR/AP | debit/credit adjustment, original ref, correlation | correlation key | immutable retry | reason minimum |
| `BIL-INT-010` (**baru, approved**, `BKC-DEC-060`) | Clinical Management (`InsuranceCoverageService`) → Billing | Panggilan **in-process/sinkron** (bukan message/event — satu assembly), `ResolveTariffAsync(encounterId, tariffId, quantity)`; hasil dipakai preview badge, TIDAK dipersist | N/A — read-only, tanpa side effect, aman dipanggil berulang | Exception/timeout DB mengembalikan `422`/`500` biasa, bukan retry/outbox — konsisten pola panggilan sinkron in-process lain di modul ini | Field internal rule (`RuleCode`, `ApprovalInstruction`) tidak diteruskan ke response publik Billing |

Urutan coverage adalah primary dahulu, excess hanya residual, lalu patient. Klaim ditolak tidak memindahkan debtor tanpa contract policy. InvoiceDate tidak berubah karena pembayaran; self-pay due pada invoice date, penjamin mengikuti term. Late noncash settlement tetap dikaitkan ke tender asal dan tidak mengubah physical cash shift closed.

Setiap message menyertakan `ContractVersion`, `OccurredAt`, `CorrelationId`, `CausationId`, source version, dan schema validation. Dead-letter/replay wajib terlihat operasional. Tidak ada distributed transaction; producer mempertahankan source of truth, Billing menyimpan receipt/outbox dan reconciliation status. Tests `BIL-AT-002`,`004`,`009`,`017`,`019`,`021`.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

`contract_version: BIL-INTEGRATION-0.5` · status **approved** · approved_by Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · approved_at 4 September 2026 · input `BKC-DEC-065`–`069`, `BKC-DES-001`–`009` (approved).

| ID | Producer → Consumer | Trigger/payload minimum | Idempotency | Failure/retry | Security/privacy |
| --- | --- | --- | --- | --- | --- |
| `BIL-INT-011` (**baru, draft**, `BKC-DEC-067`) | Administrator Master Data (`MstInsuranceProvider`) → Billing | Bacaan **in-process** satu baris master berdasarkan `TrxPatientEncounterGuarantor.InsuranceProviderId`; hanya `InsuranceProviderName`, `InsuranceGroupName`, `ProviderType`, `ClaimMethod`, `ContractNumber`, `OfficeAddress` yang dibaca. Hasilnya **tidak** dipersist di tabel `Bil*` | N/A — baca murni, tanpa efek samping, aman dipanggil berulang | Baris tidak ditemukan **MUST NOT** melempar galat: dokumen tetap `200` dengan blok asuransi berisi `—`, `isPrintable=false`, dan peringatan `BIL-VAL-034`. Kegagalan koneksi database mengembalikan `500` biasa, bukan retry/outbox | `PicName`/`PicPhoneNumber`/`PicWhatsAppNumber`/`PicEmail`, `BillingInstruction`, dan `ClaimInstruction` **MUST NOT** dibaca maupun diteruskan ke response |
| `BIL-INT-012` (**baru, draft**, `BKC-DES-009`) | Registration Management (`TrxPatientEncounterGuarantor`) → Billing | Bacaan **in-process** kolom snapshot polis kunjungan: `PaymentType`, `InsuranceProviderId`, `PolicyNumberSnapshot`, `MemberNumberSnapshot`, `PlanNameSnapshot`, `ClassNameSnapshot`, `BenefitPlanCodeSnapshot`, `EffectiveStartDateSnapshot`, `EffectiveEndDateSnapshot`, `IsEligible`, `IsPolicyActive` | N/A — baca murni | Baris tidak ada → `200` dengan `payerKind="UNKNOWN"` dan peringatan `BIL-VAL-031`; bukan `404` | `CardNumberSnapshot` **MUST NOT** dibaca. Nilai yang dibaca berasal dari snapshot registrasi, **bukan** dari `MstPatientInsurance` terkini — lihat `BKC-DES-009` |

Kedua bacaan di atas adalah panggilan langsung `ApplicationDbContext` dalam proses yang sama, bukan pesan maupun HTTP — konsisten dengan pola `RegistrationBillingCoverageAdapter` yang sudah membaca tabel Registration secara langsung, dan dengan `BIL-INT-010` (`InsuranceCoverageService`) pada amendment 2 September 2026. Modul ini adalah satu assembly; tidak ada distributed transaction, tidak ada outbox, dan tidak ada dead-letter untuk keduanya karena tidak ada penulisan yang bisa gagal separuh jalan.

**Yang tidak ditambahkan:** tidak ada kontrak baru Billing → pihak asuransi. Dokumen ini dicetak dan diserahkan secara manual (`BKC-DEC-065`: pola presentasi sama dengan Kwitansi, PDF di browser). Pengiriman klaim elektronik ke perusahaan asuransi tetap milik `InsuranceManagement` (`PLANNED`) dan tetap `INS-DEC-005` yang belum diputuskan.

Trace `BKC-DEC-065`–`069`, `BKC-DES-009`. Tests `BIL-AT-031`, `BIL-AT-034`.

---

## Amendment 4 September 2026 — Bacaan care setting dan status penjamin

`last_changed_in: BIL-INTEGRATION-0.6` · status **approved** · owner Registration + Billing + Finance/Tax · `approved_by`: Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) · `approved_at`: 4 September 2026. Input: `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`.

### Yang berubah pada kontrak integrasi yang sudah ada

| ID | Producer → Consumer | Yang berubah | Dampak |
| --- | --- | --- | --- |
| `BIL-INT-001` | Registration → Billing | **Tidak berubah bentuknya**, tetapi konsekuensi datanya menguat. `IsEligible`, `IsPolicyActive`, dan `InsuranceProviderId` pada `TrxPatientEncounterGuarantor` kini menentukan apakah tagihan menampilkan peringatan anomali data ke kasir. Sebelumnya kesalahan pada ketiganya berakhir sebagai angka yang tidak dapat dijelaskan; kini ia berakhir sebagai kalimat yang menyebut Registrasi | Registrasi **MUST** diberi tahu bahwa ketiga kolom itu sekarang terlihat dampaknya di layar kasir |
| `BIL-INT-001` | Registration → Billing | `EncounterType` menjadi **penentu basis pajak**, lewat snapshot `BilInvoice.ServiceType` yang dibuat saat invoice dibuka. Koreksi `EncounterType` **setelah** invoice dibuka **tidak** mengubah basis pajak tagihan itu | Bila sebuah kunjungan salah didaftarkan sebagai rawat jalan padahal rawat inap, tagihannya akan tetap dikenai PPN sampai invoicenya dibatalkan dan dibuat ulang. Ini konsekuensi yang disengaja dari `BKC-DES-018` |
| `BIL-INT-005` | Pricing/Coverage → Billing | Hasil penilaian penjamin kini membawa **rincian per komponen** dan **daftar anomali**, bukan hanya total | Konsumen internal (`BillingCalculationService`) sudah menyesuaikan; tidak ada konsumen luar |
| `BIL-INT-010` | `InsuranceCoverageService` (Clinical) → Billing | **Tidak berubah.** Layanan advisory itu masih membaca `IsNeedApproval`, `IsNeedGuaranteeLetter`, `MaxAmountPerMonth`, dan `MaxQuantityPerMonth` untuk badge di layar entri | **Selisih yang MUST diketahui**: setelah `BKC-DEC-071`, badge advisory di layar entri dapat menunjukkan "butuh approval" sementara perhitungan tagihan sudah menanggungnya penuh. Keduanya menjawab pertanyaan berbeda, tetapi bagi pengguna keduanya terlihat bertentangan. Lihat `BKC-OQ-087` |

### Bacaan baru yang tidak menambah kontrak

| Yang dibaca | Dari | Cara | Alasan tidak menjadi kontrak baru |
| --- | --- | --- | --- |
| `BilInvoice.ServiceType` | Tabel milik modul ini sendiri | Properti pada entity yang sudah dimuat | Bukan lintas modul |
| `MstTaxRule.AllocationRule` | Billing Master Data | `LoadInvoiceTaxRuleAsync`, sudah ada | Sudah dibaca sebelum amendment ini; yang berubah hanya **nilainya** |

### Yang tidak ditambahkan

Tidak ada pesan, outbox, dead-letter, maupun rekonsiliasi baru. Seluruh bacaan di atas adalah panggilan `ApplicationDbContext` dalam satu proses yang sama, dan tidak ada penulisan yang dapat gagal separuh jalan. Tidak ada kontrak baru Billing → pihak asuransi maupun Billing → kantor pajak; pelaporan PPN tetap berjalan lewat proses keuangan yang sudah ada di luar modul ini.

Trace `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Tests `BIL-AT-036`–`048`.

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

`last_changed_in: BIL-INTEGRATION-0.7` · status **draft** · owner Billing dan Kepala Kasir/Finance Operations · `approved_by`: — · `approved_at`: — · input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014`.

### Tidak ada kontrak integrasi baru — dan itu keputusan, bukan kelalaian

Petty Cash adalah rumpun paling terisolasi di seluruh modul ini. Ia **tidak** membaca dari modul lain, **tidak** menulis ke modul lain, dan **tidak** mengirim maupun menerima pesan.

Berkas ini tetap disunting — walaupun tidak ada baris `BIL-INT-*` baru — karena yang perlu dicatat justru **ketiadaannya**. Dua titik singgung yang paling wajar diharapkan pembaca memang ada, dan keduanya **diputus secara eksplisit** oleh keputusan pemilik. Membiarkan berkas ini diam akan membuat pembaca berikutnya menyangka sambungan itu terlupa dirancang.

### Titik singgung yang sengaja diputus

| Titik singgung yang wajar diharapkan | Keadaan sebenarnya | Dasar |
| --- | --- | --- |
| Petty Cash → Cashier Operations (`BilCashierShift`) | **Tidak ada, dan tidak boleh ada.** Pencairan voucher **MUST NOT** menambah `SystemCash`, mengurangi `PhysicalCash`, atau menggerakkan `Variance` shift mana pun. Menutup shift pada hari yang sama dengan pencairan voucher menghasilkan angka yang persis sama seperti bila voucher itu tidak pernah ada | **`PC-DEC-001`** — anggaran Petty Cash adalah kolam terpisah |
| Petty Cash → Billing (`BilInvoice`, `BilCalculationVersion`) | **Tidak ada.** Tidak satu rupiah pun pengeluaran kas kecil masuk `PatientAmount`, `PrimaryAmount`, `TaxAmount`, maupun `NonBillableResidualAmount`. Petty Cash bukan biaya pasien | `PC-DEC-001`; batas scope amendment |
| Petty Cash → Human Resource (`WorkforceProfile`, `MstEmployee`) | **Tidak ada.** "Nama Penerima" adalah teks bebas, bukan pilihan dari master pegawai | **`PC-DEC-011`** |
| Petty Cash → `Corporate/HumanResource/ExpenseManagement` (`TrxExpenseClaim`, `MstExpenseCategory`) | **Tidak ada.** Kategori Petty Cash adalah master data milik `billing-kasir` sendiri | `PC-DEC-012`; `01-existing-capability-map.md` § 18.3 |
| Petty Cash → Finance/Accounting (jurnal, buku besar) | **Tidak ada pada rilis ini.** Tidak ada pos jurnal yang dibentuk otomatis dari pencairan voucher | Tidak ada keputusan yang memintanya. `Areas/Corporate/AccountingManagement` terverifikasi **kosong** (`01-existing-capability-map.md` § 18.5), sehingga tidak ada konsumen yang dapat dituju |

### Bacaan dalam proses yang **tidak** menjadi kontrak baru

Ketiganya adalah pembacaan langsung `ApplicationDbContext` di dalam satu proses yang sama, pada tabel yang dimiliki modul ini sendiri atau pada infrastruktur bersama yang sudah terpakai. Tidak ada pesan, tidak ada HTTP, tidak ada outbox, dan tidak ada dead-letter — karena tidak ada penulisan yang dapat gagal separuh jalan di luar transaction.

| Yang dibaca atau ditulis | Dari | Cara | Alasan tidak menjadi kontrak baru |
| --- | --- | --- | --- |
| `BilNumberSeries` | Tabel milik modul ini sendiri | `BillingNumberSeriesService.AllocatePettyCashVoucherNumberAsync`, memakai helper privat `AllocateNumberAsync` yang sudah ada | Bukan lintas modul. Mekanisme yang sama sudah dipakai empat jenis nomor lain di modul ini (`CAP-30`) |
| `MstPettyCashCategory` | Tabel milik modul ini sendiri | Properti navigasi pada entity yang sudah dimuat | Bukan lintas modul |
| Identitas pengguna (`ApplicationUser`) | Administrator / Identity | `Guid` dari claim pengguna yang sedang login, **tanpa** foreign key | Pola yang sudah berjalan di seluruh modul ini, misalnya `BilCashierShiftCommand.ActorUserId` (`CAP-32`). Menampilkan nama pengguna pada response memakai jalur pembacaan identitas yang sudah ada, bukan kontrak baru |

### Konsekuensi yang **MUST** diketahui pemilik proses

| Konsekuensi | Penjelasan |
| --- | --- |
| Uang kas kecil tidak muncul di laporan kas shift kasir | Ini yang `PC-DEC-001` minta. Bila kelak Finance menghendaki keduanya tampil dalam satu laporan kas rumah sakit, itu **kemampuan pelaporan baru** yang membaca kedua sumber, **bukan** menyambungkan kedua tabel |
| Pengeluaran kas kecil tidak otomatis menjadi jurnal akuntansi | Selama MVP, Finance menjurnal manual dari riwayat pergerakan anggaran (`GET /budget/movements`). Bila kelak diperlukan otomatis, itu kontrak `Billing → Accounting` baru yang menuntut modul Accounting berdiri lebih dulu |
| Tidak ada rekonsiliasi otomatis antara saldo sistem dan uang fisik di laci | Tidak ada penghitungan uang fisik seperti pada penutupan shift. Selisih dikoreksi lewat `POST /budget/adjustments` yang beralasan dan tercatat di ledger |

### Yang tidak ditambahkan

Tidak ada pesan, outbox, dead-letter, rekonsiliasi, penjadwal, maupun pekerjaan latar apa pun. Secara khusus, **tidak ada** pekerjaan latar yang memindai voucher `Uang Diterima` yang bukti notanya belum masuk — `PC-DEC-006` menyatakan eksplisit tidak ada mekanisme pemaksaan pada MVP ini, dan menandainya sebagai kandidat rilis berikutnya.

Trace **`PC-DEC-001`**, `PC-DEC-011`, `PC-DEC-012`, `PC-DES-001`, `PC-DES-008`, `PC-DES-010`. Tests `BIL-AT-077` (bukti `BilCashierShift` tidak bergerak).

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

> `last_changed_in`: `BIL-INTEGRATION-0.8` / revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010`, `MPY-DES-001`–`017`.
>
> **Berbeda dari rumpun Petty Cash yang tidak punya satu pun titik singgung**, rumpun ini adalah rumpun dengan permukaan lintas modul **paling lebar** di `billing-kasir`. Ia menyentuh empat modul lain, dan satu di antaranya ditulis — bukan hanya dibaca.

## Peta arah baca dan tulis

| Modul lawan | Data | Arah | Cara | Catatan |
| --- | --- | --- | --- | --- |
| **Registration Management** | Sumber pembayaran kunjungan | **Baca dan TULIS** | Pemanggilan langsung `EncounterPaymentSourceService` di dalam proses yang sama | **Satu-satunya titik tulis lintas modul pada rumpun ini.** `billing-kasir` **MUST NOT** menulis tabelnya sendiri (`MPY-DEC-007`, `MPY-DES-004`) |
| Registration Management | Data kunjungan — kelas perawatan, jenis kunjungan, pasien | Baca | Query langsung, hanya baca | Sudah menjadi pola modul ini sejak baseline |
| **Patient Management** | Kartu asuransi pasien, kartu penjamin perusahaan pasien | **Baca saja** | Query langsung, hanya baca | Dipakai menyusun daftar pilihan payer dan memvalidasi kandidat. Kartu **MUST NOT** dibuat atau diubah dari sini (`MPY-DEC-003`) |
| **Administrator / Master Data** | Perusahaan penjamin, perusahaan asuransi, rute reimbursement | Baca | Query langsung | Rute reimbursement adalah tabel baru milik area itu, dikelola CRUD-nya sendiri |
| **Health Services / Master Data** | Aturan tanggungan asuransi dan aturan tanggungan perusahaan | Baca | Query langsung oleh mesin tanggungan | Aturan tanggungan perusahaan adalah tabel baru milik area itu |
| **Pharmacy Management** | Fakta penyerahan obat | **Baca saja** | Query langsung, hanya baca | **MUST NOT** ditulis dalam keadaan apa pun (`MPY-DEC-009`). Lihat bagian khusus di bawah |

## Kontrak pemanggilan ke Registration Management

Ini satu-satunya kontrak antar modul yang perlu disepakati dua pihak pada rumpun ini.

| Aspek | Kesepakatan |
| --- | --- |
| Siapa yang memanggil | Orkestrator edit tagihan milik `billing-kasir` |
| Siapa yang dipanggil | `EncounterPaymentSourceService` milik `RegistrationManagement` — **belum ada, wajib dibangun** (`CAP-33`) |
| Bentuk pemanggilan | Pemanggilan method langsung di dalam proses yang sama, bukan HTTP, bukan pesan |
| Transaksi | **Ikut transaksi pemanggil.** Layanan yang dipanggil **MUST NOT** membuka atau menutup transaksinya sendiri, supaya perubahan payer dan hasil perhitungan menjadi satu kesatuan yang batal bersama |
| Yang dijamin pemanggil | Gerbang kelayakan edit sudah lolos, versi baris tagihan sudah diperiksa, dan alasan sudah terisi |
| Yang dijamin yang dipanggil | Invariant satu sumber pembayaran per kunjungan tetap utuh; seluruh kolom salinan dibangun ulang; kartu yang dipilih sah milik pasien yang sama dan masih berlaku |
| Kegagalan | Dikembalikan sebagai penolakan bisnis yang dapat dibaca pengguna, bukan sebagai galat teknis. Pemanggil membatalkan seluruh transaksi |
| Persetujuan yang dibutuhkan | **Pembuatan layanan ini menunggu persetujuan pemilik `RegistrationManagement`, Muhammad Hamzah** (`MPY-DEC-010`). Ini memblokir implementasi, bukan desain |

**Yang sengaja tidak dipakai:** HTTP antar modul, antrian pesan, outbox, dan *eventual consistency*. Ketiganya akan memecah perubahan payer dan perhitungan ulang menjadi dua kejadian yang dapat berbeda nasib — tepat yang tidak boleh terjadi pada angka tagihan. Karena kedua modul berbagi satu `ApplicationDbContext`, satu transaksi biasa sudah cukup.

## Batas terhadap Pharmacy Management

Bagian ini ditulis panjang karena ia batas yang paling mudah dilanggar tanpa sengaja.

| Hal | Ketentuan |
| --- | --- |
| Yang dibaca | Baris penyerahan obat, untuk mengetahui obat apa yang benar-benar diserahkan kepada pasien |
| Yang **MUST NOT** dilakukan | Membuat, mengubah, membatalkan, atau menandai hapus satu baris pun milik Farmasi; mengubah jumlah yang diserahkan; mengubah status penyerahan |
| Sebabnya | Farmasi adalah pemilik otoritatif fakta penyerahan obat, lengkap dengan riwayat, jumlah tersisa per baris resep, dan penyerahan bertahap. Source Farmasi sendiri menyatakan batas ini: *"Pencatatan ini berhenti sebagai transaksi yang dapat ditagihkan. Keputusan menagih beserta aturannya milik Billing."* |
| Yang dicatat `billing-kasir` sebagai gantinya | Keputusan **inklusi finansial** pada tabel miliknya sendiri. Dua pertanyaan berbeda: Farmasi menjawab "obat ini diserahkan atau tidak", Billing menjawab "obat ini ditagihkan atau tidak" |
| Titik singgung yang belum diputuskan | Kolom penanda "sudah ditagih" pada baris penyerahan Farmasi sudah ada tetapi belum diketahui dipakai proses apa. **MUST** diperiksa sebelum implementasi supaya rumpun ini tidak membuat mekanisme paralel yang bertentangan — dicatat sebagai pertanyaan terbuka pada `04-prd-to-mvp.md` |

## Yang tidak ada pada rumpun ini

Tidak ada pesan, outbox, dead-letter, rekonsiliasi, penjadwal, maupun pekerjaan latar apa pun. Secara khusus **tidak ada** pekerjaan latar yang menyelaraskan penanggung baris biaya dengan payer kunjungan: penyelarasan terjadi **serentak** di dalam transaksi perubahan payer (`MPY-DES-009`), bukan menyusul belakangan. Alasannya sederhana — angka tagihan yang menyusul benar adalah angka tagihan yang sempat salah.

Tidak ada pula perubahan pada arah piutang. Rute reimbursement perusahaan ke asuransi mitra adalah **keterangan pada dokumen**, bukan perpindahan debitur: rumah sakit tetap menagih perusahaan penjamin (`MPY-DES-014`).

Trace **`MPY-DEC-001`**, `MPY-DEC-003`, `MPY-DEC-007`–`010`, `MPY-DES-004`, `MPY-DES-009`, `MPY-DES-014`. Tests `BIL-AT-081`–`100`, khususnya `BIL-AT-096` (bukti nol baris Farmasi tersentuh).

---

## Amendment 15 September 2026 — Revisi Petty Cash: ketiadaan sambungan Accounting yang disengaja

`last_changed_in: BIL-INTEGRATION-0.9` · status **approved** · owner Billing dan Accounting · `approved_by`: Product/Domain Owner (`PC-DEC-026`) · `approved_at`: 2026-09-15 · input: **`PC-DEC-023`**; keputusan arsitektur `PC-DES-023`.

Amendment ini **tidak menambah satu pun titik integrasi**. Isinya justru mencatat sebuah ketiadaan — dan ketiadaan itu adalah isi, bukan penomoran kosong, karena dokumen sumber revisi ini secara eksplisit meminta sebaliknya.

### Yang diminta dokumen revisi, dan kenapa tidak dikerjakan sekarang

Dokumen BRD/PRD revisi Petty Cash (15 September 2026) meminta pada BR-PC-016 dan PRD § 9 agar Kas Kecil menjadi sumber *accounting event*/subledger terkontrol, dan secara khusus melarang transaksi operasional dibuat lewat jurnal manual bebas ke akun kontrol Kas Kecil.

Keadaan yang sebenarnya pada backend SHA `0ca85ba4`, terverifikasi `01-existing-capability-map.md` § 20.2:

| Fakta | Bukti |
| --- | --- |
| Modul Accounting **sudah ada** | `Areas/Corporate/AccountingManagement/` memuat `AccJournal`, `AccJournalLine`, `AccJournalType`, `AccChartOfAccount`, `AccNumberSeries`, dan `AccJournalService` |
| `AccJournalService` **hanya** melayani jurnal manual berjenjang | `CreateAsync` selalu menetapkan `JournalStatus = Draft`; pengesahan menuntut `SubmitAsync`, lalu `ApproveAsync`, lalu `PostAsync` — seluruhnya dipicu manusia. Tidak ada parameter maupun jalur yang membuat jurnal langsung `Posted` |
| **Belum ada** modul domain mana pun yang memanggilnya | Pencarian `AccJournalService` di seluruh `Areas/` hanya menemukan pemakaian di dalam `AccountingManagement` sendiri dan pendaftaran di `Program.cs`. Tidak ada preseden pola integrasi lintas modul untuk ditiru |

Memanggil `AccJournalService` dari Petty Cash berarti setiap pencairan melahirkan jurnal `Draft` yang menunggu pengesahan manusia di modul lain. Dengan kata lain: gerbang persetujuan yang baru saja dicabut `PC-DEC-016` dari Petty Cash akan muncul kembali satu lapis di belakangnya, di modul yang pemiliknya berbeda — dan itu justru bentuk "jurnal manual" yang BR-PC-016 larang.

`PC-DEC-023` karena itu memilih menunda integrasi, bukan memaksakannya lewat jalur yang salah.

### Titik integrasi Petty Cash setelah revisi

| Arah | Modul lawan | Keadaan | Dasar |
| --- | --- | --- | --- |
| Petty Cash ke Accounting | `AccountingManagement` | **Tidak ada, disengaja** | `PC-DEC-023`, `PC-DES-023` |
| Petty Cash ke kas fisik shift kasir | `billing-kasir` (`BIL-CTX-04`) | **Tidak ada, disengaja** | `PC-DEC-001`, tetap berlaku |
| Petty Cash ke tagihan pasien | `billing-kasir` (`BIL-CTX-01`) | **Tidak ada, disengaja** | `PC-DES-001`, tetap berlaku |
| Petty Cash ke master pegawai | `HumanResource` | **Tidak ada, disengaja** | `PC-DEC-011`, ditegaskan ulang `PC-DEC-019` |
| Petty Cash ke penyimpanan berkas | Platform storage service | **Tidak ada, disengaja** | `PC-DEC-021` — bukti tetap berupa nomor referensi teks |

Petty Cash setelah revisi ini tetap menjadi **rumpun paling terisolasi di seluruh modul**: nol titik integrasi keluar, nol ketergantungan lintas modul yang memblokir implementasi.

### Apa yang menggantikan integrasi Accounting selama MVP

`BilPettyCashBudgetMovement` diperlakukan sebagai **subledger kas kecil** yang berdiri sendiri. Ia sudah memenuhi syarat yang dituntut dokumen revisi atas sebuah subledger:

| Syarat dokumen revisi | Dipenuhi oleh |
| --- | --- |
| Setiap mutasi saldo punya saldo sebelum dan sesudah | `BalanceBefore`, `BalanceAfter` |
| Setiap mutasi punya pelaku dan waktu | `ActorUserId`, `OccurredAt` |
| Ledger tidak dapat dihapus atau disunting | Append-only; koreksi lewat baris baru (`ADJUSTMENT`, `RETURN`, `REVERSAL`) |
| Mutasi tidak terjadi dua kali karena tombol tertekan ganda | `IdempotencyKey` |
| Setiap mutasi dapat ditelusuri ke dokumen sumbernya | `VoucherId` pada pergerakan bervoucher, `Reason` pada yang lain |

Yang **belum** dipenuhi dan memang ditunda: pemetaan ke bagan akun, pembentukan jurnal, dan rekonsiliasi otomatis dengan buku besar. Selama MVP, rekonsiliasi antara saldo kas kecil dan Accounting dikerjakan **manual** oleh Finance dari laporan pergerakan, dengan periode anggaran sebagai satuan rekonsiliasinya.

### Prasyarat bila integrasi dilanjutkan pada rilis berikutnya

Ditulis sekarang supaya rilis berikutnya tidak mengulang penelusuran yang sama:

1. Pemilik modul Accounting **MUST** memutuskan apakah `AccJournalService` mendapat jalur posting sistem yang melewati `Submit`/`Approve` manusia, atau apakah jurnal dari subledger tetap melewati pengesahan.
2. Pemetaan akun (Kas Kecil, akun perantara uang muka, akun beban per kategori) **MUST** datang dari konfigurasi Accounting, **MUST NOT** ditulis tetap di controller maupun service Petty Cash.
3. Titik pemicu jurnal **MUST** ditetapkan per peristiwa: pencairan, pengembalian, pembalikan, penambahan saldo, dan penutupan periode — kelimanya sudah punya baris ledger sendiri, sehingga pemicunya sudah tersedia tanpa perubahan skema.

---

## Amendment 18 September 2026 — Ketergantungan AR/AP diputus dari status invoice

`last_changed_in: BIL-INTEGRATION-1.0` · status **approved** (`BKC-DEC-105`, 18 September 2026) · input: **`BKC-DEC-100`–`BKC-DEC-102`**; keputusan arsitektur `BKC-DES-028`–`BKC-DES-035`.

### Yang berubah pada permukaan AR/AP

| Hal | Hari ini | Sesudah amendment ini |
| --- | --- | --- |
| Syarat invoice berpindah ke `CLOSED` | Menunggu "AR/AP posting sukses" — peristiwa yang **tidak pernah terjadi** karena konsumen AR/AP-nya belum ada | Sisa tagihan pasien mencapai nol; tidak menunggu pihak luar mana pun |
| Penjaga pencatatan koreksi AR (`RecordCorrectionIfLinkedAsync`) | Hanya menerima invoice `FINAL` | Menerima `FINAL` **dan** `CLOSED`; tetap menolak `OPEN` dan `SETTLED_BY_WRITE_OFF` (`BKC-DES-035`) |
| Bentuk dan isi `BilArHandoff`/`BilApHandoff` | — | **Tidak disentuh sama sekali** (`BKC-DES-033`) |
| Penyerahan nyata ke sistem AR/AP | Belum ada konsumennya (`BKC-BLK-INT-001`) | **Masih belum ada** — amendment ini tidak membangunnya dan tidak berpura-pura membangunnya |

### Kenapa ketergantungan pada AR/AP diputus, bukan ditunggu

`BKC-BLK-INT-001` (consumer contract AR/AP belum dibuktikan) sudah tercatat sebagai dependency terbuka sejak awal modul ini. Selama syarat transisi `CLOSED` menggantung pada peristiwa milik konsumen yang belum ada, seluruh invoice lunas ikut menggantung — dependency yang seharusnya menahan **satu** kemampuan ternyata menahan **jalur paling umum** di modul ini. `BKC-DEC-100` memutus ketergantungan itu: status invoice kini ditentukan fakta yang dimiliki Billing sendiri (sisa tagihan pasien), sementara penyerahan fakta ke AR/AP tetap menjadi pekerjaan terpisah yang menunggu konsumennya.

`BKC-BLK-INT-001` **tetap terbuka** dan tetap menahan hal-hal yang memang miliknya: penyerahan nyata, pengakuan (`ACKNOWLEDGED`) oleh sistem AR, dan sumbu status penagihan piutang.

### Kontrak yang MUST dijaga ketika konsumen AR/AP kelak dibangun

1. Sumbu status `BilArHandoff` (`CREATED`/`ACKNOWLEDGED`) menyatakan **penyerahan fakta**, bukan tertagihnya piutang. Bila kelak dibutuhkan sumbu "tertagih", ia dirancang bersama pemilik konsumen AR/AP — **MUST NOT** ditebak sekarang (`BKC-DES-033`).
2. `BilInvoice.Status`/`ClosedAt` adalah sumber kebenaran "tagihan pasien ini sudah lunas". Konsumen AR/AP membacanya, **MUST NOT** membentuk salinan kebenarannya sendiri.
3. Koreksi AR (`BilHandoffAdjustment`) tetap idempotent per sumber (`SourceAdjustmentId`/`SourceWriteOffCaseId`); amendment ini tidak mengubahnya.

Trace **`BKC-DEC-100`–`102`**, `BKC-DES-028`–`035`. Tests `BIL-AT-129`–`BIL-AT-131`.

---

## Amendment 21 September 2026 — Penerbitan fakta finansial ke dua modul konsumen

`last_changed_in: BIL-INTEGRATION-1.1` · status **draft** · owner Billing + Finance (AR/AP) + Farmasi · `approved_by`: — · `approved_at`: — · input: **`BKC-DEC-106`–`109`** (approved 21 September 2026); keputusan arsitektur `BKC-DES-036`–`041`.

### Apa yang berubah secara mendasar

Sampai amendment sebelumnya, Billing **tidak punya satu pun konsumen nyata**. `BKC-BLK-INT-001`
mencatatnya sebagai dependency terbuka, dan `BIL-INTEGRATION-1.0` bahkan memutus ketergantungan
status invoice dari AR/AP justru karena konsumennya belum ada.

Keadaan itu **berubah**. Dua konsumen kini nyata, dan keduanya menunggu:

| Konsumen | Keadaannya | Yang tertahan karenanya |
| --- | --- | --- |
| Finance (AR/AP) | Modul berdiri, `FinBillingHandoffIntake` sudah dibangun dan `HandoffType`-nya sudah memuat nilai `COLLECTION` | `BE-FIN-016`, `BE-FIN-017`, `BE-FIN-018` |
| Farmasi | Keputusan `PHA-DEC-063`–`070` sudah approved; slice-nya sudah `READY_FOR_DOMAIN_DESIGN` | Seluruh resep rawat jalan macet permanen di `WaitingForPayment` |

### Dua kontrak baru

| ID | Producer → Consumer | Trigger/payload minimum | Idempotency | Failure/retry | Security/privacy |
| --- | --- | --- | --- | --- | --- |
| `BIL-INT-013` (**baru, draft**, `BKC-DEC-106`) | Billing → Finance (AR/AP) | Satu tender mencapai `SUCCEEDED` **atau** `REVERSED`. Muatan: identitas tender, penyelesaian, dan tagihan; identitas alokasi pembayaran bila ada; cara bayar beserta rekening/kanalnya; nominal apa adanya; nomor kwitansi; shift kasir untuk tunai; rujukan dan identitas kejadian penyedia untuk non-tunai; waktu uang diterima; **status tagihan saat itu**; status tender; kunci handoff; korelasi dan sebab | Kunci handoff diturunkan dari pasangan identitas tender dan status tender. Tender yang sama dengan status yang sama **MUST** menghasilkan tepat satu baris efektif | Baris bersifat tetap. Percobaan ulang tidak menghasilkan baris kedua. Kegagalan penerbitan membatalkan transaksi pemanggil — surat dan pergerakan uangnya tidak pernah terpisah nasib | Identitas debitur dan rujukan penyedia bersifat sensitif. Token, kredensial, dan data kartu **MUST NOT** ikut |
| `BIL-INT-014` (**baru, draft**, `BKC-DEC-106`) | Billing → Farmasi | Keadaan clearance sebuah resep **berubah**. Muatan: identitas resep; identitas tagihan; keadaan clearance (`Cleared`/`Revoked`); hasil finansial (`Paid`/`InsuranceApproved`/`PaymentWaived`); kode sebab; nomor versi finansial; waktu berlaku; korelasi dan sebab | Dikunci pasangan identitas resep dan nomor versi finansial. Nomor versi naik monoton per resep, dilindungi kunci penasihat tagihan yang sudah ada (`BKC-DES-032`) | Sama dengan `BIL-INT-013`. Ditambah: versi yang lebih lama **MUST NOT** menimpa versi yang lebih baru di sisi konsumen (`PHA-DEC-063`) | Hanya identitas dan keadaan finansial. **MUST NOT** memuat nama obat, dosis, aturan pakai, maupun keterangan klinis apa pun |

### Satu titik deteksi, dua surat

`BKC-DEC-106` mengunci bentuknya: Billing mendeteksi peristiwa finansialnya **sekali di satu
tempat**, lalu menerbitkan surat sesuai konsumen yang memang terdampak.

Yang dijamin bentuk ini: **mustahil** Finance mengetahui sebuah pembayaran sementara Farmasi
tidak, atau sebaliknya. Keduanya lahir dari deteksi yang sama, di dalam transaksi yang sama.

Penting: satu peristiwa **tidak selalu** melahirkan dua surat. Keduanya punya syarat sendiri.

| Peristiwa | Surat ke Finance | Surat ke Farmasi |
| --- | :---: | :---: |
| Tender berhasil, tagihan belum lunas | **Ya** | Tidak — keadaan resep belum berubah |
| Tender berhasil, tagihan menjadi lunas | **Ya** | **Ya** — resep menjadi `Cleared` |
| Tagihan lunas lewat penghapusan tagihan, tanpa uang masuk | Tidak — tidak ada tender | **Ya** — hasil finansial `PaymentWaived` |
| Biaya tindakan ditambahkan pada tagihan yang sudah lunas | Tidak | **Tidak** — `PHA-DEC-068`, biaya non-farmasi tidak mencabut clearance obat |
| Harga atau jumlah obat pada resep dikoreksi naik | Tidak | **Ya** — `Revoked` |
| Pembayaran dibalik | **Ya**, baris baru berstatus dibalik | **Ya** — `Revoked` untuk seluruh resep pada tagihan itu (`PHA-DEC-068-A`) |

Baris keempat adalah yang paling mudah salah dirancang, dan sengaja ditulis eksplisit: tagihan
kembali memiliki sisa, tetapi obat yang sudah dibayar **tetap** boleh diserahkan.

### Mengapa surat Finance tidak menunggu finalisasi

Permintaan Finance menyebutnya sebagai bagian terpenting, dan alasannya cocok dengan keadaan
source: Billing hari ini memungkinkan pasien membayar sementara tagihannya masih terbuka, dan
finalisasi menyusul belakangan. Bila surat baru terbit saat finalisasi, uang yang masuk lebih
dulu tidak akan pernah sampai ke buku Finance.

Konsekuensinya bagi bentuk data: `BilCollectionHandoff` **MUST NOT** menjadi anak
`BilFinalizationRecord`. Ini membedakannya dari `BilArHandoff` yang memang lahir dari
finalisasi, dan mengubah invariant `BIL-CTX-05` — lihat `02-backend-architecture.md`.

Status tagihan pada saat itu tetap ikut dikirim, karena Finance mencatat penerimaannya segera
tetapi **menahan** penerbitan jurnal sampai tagihannya final. Untuk penanda "tagihan akhirnya
final", Finance menerima `BilArHandoff` yang menyusul saat finalisasi sebagai penanda yang
sudah cukup — tidak ada jenis surat ketiga yang dibuat.

### Permukaan pemeriksaan ulang untuk Farmasi

`BKC-DEC-107` menuntut lebih dari sekadar menerbitkan surat. Surat bisa gagal diproses —
konsumen error, sempat mati, atau baris terlewat — dan bila itu terjadi, resep akan tertahan
sampai ada perubahan finansial berikutnya, yang mungkin **tidak pernah terjadi** karena
tagihannya memang sudah lunas.

Karena itu Billing menyediakan **cara membaca keadaan clearance terkini sebuah resep**, dapat
dipanggil kapan saja tanpa menunggu perubahan.

| Aspek | Ketentuan |
| --- | --- |
| Bentuk | Pemanggilan langsung di dalam proses yang sama, bukan HTTP dan bukan pesan |
| Alasan bentuknya | Konsisten dengan `BIL-INT-010`, `011`, dan `012` yang seluruhnya in-process. Modul ini satu assembly; tidak ada distributed transaction |
| Sifat | Baca murni, tanpa efek samping, aman dipanggil berulang |
| Yang dikembalikan | Keadaan clearance, hasil finansial, nomor versi finansial, dan waktu berlaku — bentuk yang sama dengan isi surat |
| Kegagalan | Resep yang tidak dikenal mengembalikan keadaan "tidak diketahui", **MUST NOT** melempar galat dan **MUST NOT** diperlakukan sebagai clear (`PHA-DEC-067`) |

Ini memenuhi janji `PHA-DEC-063` bahwa proyeksi di sisi Farmasi bersifat *reconcilable*. Tanpa
permukaan ini, janji itu tidak dapat ditepati oleh pihak mana pun.

### Pengakuan penerimaan dan surat yang menggantung

`BKC-DEC-108`: Billing mencatat kapan sebuah surat diambil konsumen, dan surat yang belum
diambil dapat ditemukan serta dihitung.

Billing sendiri **tidak menahan apa pun** dan tidak mengubah perilakunya karena sebuah surat
belum diambil. Uang sudah diterima; pelayanan tidak boleh tertahan karena urusan teknis antar
modul.

Ini menegaskan ulang ketentuan yang sudah berlaku bagi `BilArHandoff` (`BKC-DES-033`): sumbu
status handoff menyatakan **penyerahan fakta**, bukan hasil di sisi konsumen. Sumber kebenaran
"tagihan ini lunas" tetap `BilInvoice.Status`/`ClosedAt`.

Berapa lama sebuah surat boleh menggantung sebelum dianggap tidak wajar, dan siapa yang
menerima peringatannya, **belum diputuskan** — `BKC-OQ-101`, tidak memblokir.

### Masa simpan

`BKC-DEC-109`: baris handoff disimpan selamanya, tanpa pembersihan maupun pengarsipan. Ia jejak
audit lintas modul yang membuktikan Billing pernah memberi tahu, dan kapan persisnya. Sejalan
dengan tiga jalur handoff yang sudah ada, yang memang tidak punya mekanisme pembersihan.

### Dampak pada `BKC-BLK-INT-001`

| Bagian blocker | Keadaan sesudah amendment ini |
| --- | --- |
| "Konsumen AR/AP belum dibuktikan" | **Tidak berlaku lagi untuk AR.** Finance berdiri, intake-nya sudah dibangun, dan kontraknya kini disepakati |
| Pengakuan (`ACKNOWLEDGED`) oleh sistem AR | **Terbuka jalurnya** — mekanismenya dirancang amendment ini; pelaksanaannya menunggu task |
| Sumbu status penagihan piutang | **Tetap terbuka.** `BKC-DES-033` tetap berlaku: sumbu "tertagih" **MUST NOT** ditebak, dan dirancang bersama pemilik konsumen ketika memang dibutuhkan |
| Konsumen AP (utang dokter) | **Tetap terbuka.** Amendment ini tidak menyentuh `BilApHandoff` |

### Yang tidak ditambahkan

| Yang wajar diharapkan | Keadaan sebenarnya | Alasan |
| --- | --- | --- |
| Antrian pesan, outbox, atau dead-letter | Tidak ada | Satu assembly, satu `ApplicationDbContext`, satu transaksi. Memecah penerbitan menjadi kejadian terpisah justru menciptakan kemungkinan uang bergerak tanpa suratnya |
| HTTP antar modul | Tidak ada | Konsisten `BIL-INT-010`–`012` |
| Alokasi uang per baris tagihan | Tidak ada | Ditolak dua kali — `PHA-DEC-064` dan `PHA-DEC-068-A`. `BilPaymentAllocation` tetap hanya mengenal sasaran tagihan utuh |
| Pekerjaan latar yang menyapu surat menggantung | Tidak ada | `BKC-DEC-108` memilih keterlihatan pasif; ambang dan penerimanya belum diputuskan (`BKC-OQ-101`) |
| Jenis surat ketiga untuk pemberitahuan finalisasi | Tidak ada | Finance menerima `BilArHandoff` yang menyusul sebagai penanda yang sudah cukup |
| Billing mengirim apa pun langsung ke Accounting | Tidak ada, dan **MUST NOT** ada | Syarat eksplisit Finance: Finance yang menerbitkan kejadian akuntansinya |

Trace **`BKC-DEC-106`–`109`**, `BKC-DES-036`–`041`, `PHA-DEC-063`–`070`, `FIN-DEC-005`–`006`.
Tests `BIL-AT-135`–`BIL-AT-142`.

---

## Amendment 24 September 2026 — Integrasi Rawat Inap ↔ Billing Management

`last_changed_in: BIL-INTEGRATION-1.2` · status **draft** · owner Produsen/Konsumen: Rawat Inap (Muhammad Hamzah) & Billing (Yasmina) · input `BKC-DEC-112`–`119`, `BKC-DES-042`–`050`, `RWI-DEC-156`–`161`.

| ID | Producer → Consumer | Trigger / Payload Minimum | Idempotency | Failure / Retry | Security / Privacy |
| --- | --- | --- | --- | --- | --- |
| `BIL-INT-015` | Inpatient Bed Occupancy → Billing (`ContractBillingChargeSourceAdapter`) | Event `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED`; `EncounterId`, `PlacementId`, `RoomId`, `BedId`, `PatientClassId`, `StartAtUtc`, `EndAtUtc`, `Version` | `{SourceDomain}:{SourceType}:{PlacementId}:{Version}` (contoh: `INPATIENT:ROOM_STAY:PLC-08891:1`) | Unique index mencegah duplikasi room charge; retry aman (*no-op return existing*) | Hanya menyangkut waktu hunian dan kelas kamar fisik tanpa narasi klinis/diagnosa medis |
| `BIL-INT-016` | Billing (`BilConsumerHandoffService`) → Rawat Inap (`InpatientDischargeService`) | Sinyal perubahan kelayakan `ClearanceApproved` (`CLEARED`) atau `ClearanceRevoked` (`REVOKED`); `EncounterId`, `ClearanceStatus`, `Reason`, `ClearedAtUtc`, `CashierUserId` | `EncounterId` + `HandoffVersion` | Retry dengan exponential backoff; UI bangsal terkunci (*Fail-Safe Locked*) jika timeout kueri | Rawat Inap hanya menerima status kelayakan dan daftar blocker operasional; rincian nominal per item disembunyikan dari perawat |
| `BIL-INT-017` | Billing (`BillingSettlementService`) → Multi-Unit Cashier | Konsolidasi transaksi pelunasan alihan IGD → Rawat Inap; `EncounterId`, `IgdInvoiceId`, `RanapInvoiceId`, `SettlementAmount`, `TenderAllocations[]` | `EncounterId` + `SettlementId` | Transaksi atomik dalam satu `SaveChanges` database; rollback jika gagal separuh jalan | Satu kuitansi settlement gabungan merinci pendapatan per unit secara terpisah |

### 1. Kontrak Penanganan Kegagalan (Failure & Resilience Contract)

| Skenario Kegagalan | Perilaku Modul Rawat Inap | Perilaku Modul Billing |
|---|---|---|
| **Koneksi database/worker terputus saat pengiriman event hunian** | Event tersimpan di `InpIntegrationOutbox` dengan status `Failed`. Operasional fisik bangsal (penempatan bed/mutasi) tetap berjalan normal tanpa terhenti. | Saat dispatcher pulih, event diproses berurutan. Duplikasi event ditolak oleh unique index `IdempotencyKey`. |
| **Kueri sinkron ringkasan tagihan timeout (> 5 detik)** | UI bangsal menampilkan status fallback: *"Status Kasir Sementara Tidak Dapat Diperiksa"*; tombol pemulangan fisik tetap terkunci secara aman (*Fail-Safe Locked*). | Staf kasir dapat dihubungi melalui interkom/telepon internal rumah sakit. |
| **Pencabutan kelayakan pulang akibat tagihan susulan (`REVOKED`)** | Seketika mengeksekusi **Auto-Reblock**: status clearance di bangsal berubah menjadi `Revoked`, tombol pemulangan terkunci merah, dan nama blocker ditampilkan. | Billing menerbitkan surat pencabutan kelayakan (`REVOKED`) beralasan wajib saat charge baru masuk pada invoice yang sudah sempat disetujui. |
| **Pemulangan darurat klinis saat Billing sedang maintenance** | Supervisor bangsal dapat menggunakan **Supervisor Override** beralasan wajib untuk memulangkan pasien secara fisik demi keselamatan medis. | Billing menerima event `BED_RELEASED` dengan `isSupervisorOverridden = true` dan memproses penagihan piutang susulan melalui bagian Keuangan/AR. |

Trace `BKC-DEC-112`–`119`, `BKC-DES-042`–`050`, `RWI-DEC-156`–`161`. Tests `BIL-AT-143`–`BIL-AT-152`.

