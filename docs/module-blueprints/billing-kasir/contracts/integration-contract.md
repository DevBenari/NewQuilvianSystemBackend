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
