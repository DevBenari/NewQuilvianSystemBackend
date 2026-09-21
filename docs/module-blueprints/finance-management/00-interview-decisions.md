# Finance Management — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `FIN-BP-001` |
| Revision | `1` |
| Status | `approved` untuk 23 keputusan (`FIN-DEC-001`..`023`) — seluruh blocker Phase 0 dan aturan inti AR/AP/Cash Management tertutup. Dua item non-blocking tetap `open`: `FIN-OQ-010` (ambang nominal AP) dan `FIN-OQ-011` (konfirmasi Accounting atas draf field subledger). Lihat `FIN-CQ-02` untuk satu closure question yang didelegasikan ke `/design-business-module`. |
| Pass | `Scope pass` — selesai 20 September 2026 · `Closure pass` — selesai 20 September 2026 |
| Product/domain owner | Yasmin (owner/penggarap modul Finance AR/AP, sesuai `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md`) |
| Backend SHA | `09101d05` (branch `Yasmina`, `NewQuilvianSystemBackend`) |
| Frontend SHA | `abed49b03` (branch, `QuilvianSystemFrontendDev`) |
| Masukan | Empat dokumen analisis milik owner: `FIN-BRD-V2-0.3`, `FIN-PRD-V2-0.3`, `FIN-MVP-PRD-V2-0.3`, `FIN-ACC-XMOD-V2-0.3` — seluruhnya bertanggal analisis 18 September 2026 |
| Tanggal mulai | 20 September 2026 |

## Catatan cara kerja

Wawancara ini menjawab pertanyaan **"aturan bisnisnya bagaimana"**, bukan "apa yang sudah ada
di sistem".

**Scope dikunci tanpa audit kemampuan existing formal.** Belum ada
`01-existing-capability-map.md` untuk modul ini. Empat dokumen masukan sudah mengutip file
sumber konkret (`BilArHandoff.cs`, `BilApHandoff.cs`, `BilTender.cs`, `BilCashierShift.cs`,
`FinPettyCashBudget.cs`, `FinPettyCashBudgetMovement.cs`, `MstPettyCashCategory.cs`, model-model
`Acc*` Accounting, dsb.) sehingga bukan tebakan kosong, tetapi ini belum setara audit canonical
`/trace-existing-capabilities`. Risiko yang belum diperiksa: field persis apa yang benar-benar
ada hari ini pada `BilArHandoff`/`BilApHandoff` (mis. apakah versi/idempotency key persis seperti
diasumsikan dokumen), dan endpoint Petty Cash mana yang benar-benar sudah live vs masih
kompatibilitas alias.

Rekomendasi pada setiap pertanyaan **bukan keputusan dan bukan persetujuan**. Owner yang
berwenang tetap harus memilih. Setiap pertanyaan selalu punya opsi tambahan
`Other — tuliskan pilihan atau batasan lain`.

---

## Scope dan Outcome

**Modul:** Finance Management (`finance-management`)

**Satu kalimat batas scope:** Finance Management mengelola subledger operasional keuangan
rumah sakit — piutang (AR), utang (AP), penerimaan kasir, dan kas kecil — sebagai konsumen
fakta dari Billing dan Medical Fee, serta penerbit kejadian akuntansi ke Accounting, tanpa
pernah memiliki COA, jurnal, atau periode akuntansi.

**Konfirmasi scope:** disetujui apa adanya oleh owner pada 20 September 2026 (lihat
`FIN-DEC-000` di Decision Log).

### Di dalam scope

| ID | Kemampuan | Keterangan |
|---|---|---|
| `FIN-SC-001` | Billing Handoff Inbox | Konsumsi `BilArHandoff`/`BilApHandoff` secara idempoten |
| `FIN-SC-002` | Account Receivable | Receivable, aging, settlement, write-off, adjustment, employee benefit |
| `FIN-SC-003` | Collection / Receipt | `FinReceipt` dari tender Billing sukses, alokasi, split paid-vs-AR |
| `FIN-SC-004` | Account Payable | Supplier payable, doctor payable (konsumen dari fee yang sudah approved), payment |
| `FIN-SC-005` | Cash Management | Petty Cash (lengkapi FE di atas baseline backend existing), Bank Deposit, Daily Cash, rekonsiliasi shift kasir (read-only) |
| `FIN-SC-006` | Finance-owned master data | Bank/BankAccount, Petty Cash Category (sudah ada) |
| `FIN-SC-007` | Accounting Integration Outbox | `FinAccountingEventOutbox`, monitoring/retry — bukan endpoint penerima Accounting |

### Di luar scope — untuk modul lain

| ID | Kemampuan | Pemilik | Titik sentuh yang tetap dibahas |
|---|---|---|---|
| `FIN-OOS-001` | Kalkulasi invoice, `BilSettlement`/`BilTender`/`BilPaymentAllocation`, `BilCashierShift` | Billing/Kasir | Hanya bentuk kontrak handoff & receipt yang diterima Finance |
| `FIN-OOS-002` | COA, `AccEventType`, `AccPostingRule`, `AccAccountingPeriod`, Journal/GL | Accounting | Hanya bentuk kejadian yang diterbitkan Finance |
| `FIN-OOS-003` | `MstDoctorServiceRule`, kalkulasi/verifikasi/approval `DoctorServiceFee` | Medical Fee | Hanya titik terima fee yang sudah approved |
| `FIN-OOS-004` | Legal Entity, Cost Center | Corporate/HR | Hanya rujukan |

Dua ownership yang PRD tandai "to confirm" — **Supplier master** dan **Currency/Exchange
rate** — belum diputuskan; dicatat sebagai `FIN-OQ-002` dan `FIN-OQ-003`.

## Aktor dan Tanggung Jawab

Sumber: `FIN-PRD-V2-0.3` bagian 4 (Fact, belum digali ulang secara kritis pada pass ini).

| Peran | Tindakan utama |
|---|---|
| Finance AR Staff | Konsumsi handoff AR, verifikasi receivable, kelola klaim, alokasi penerimaan, settlement |
| Finance AP Staff | Validasi payable supplier/dokter, siapkan pembayaran, kelola adjustment |
| Finance Supervisor | Menyetujui write-off/adjustment/pembayaran sesuai kebijakan; memonitor rekonsiliasi |
| Medical Fee Admin | Menghitung, memverifikasi, dan menyesuaikan fee dokter sebelum approval (di luar scope modul ini) |
| Cashier/Treasury | Mencatat penerimaan, petty cash, setoran bank, posisi kas |
| Accounting Staff/Approver | Memelihara master/posting rule Accounting dan meninjau jurnal hasil; tidak mengubah subledger Finance |
| Auditor/Manager | Trace dan laporan read-only lintas source → Finance → Accounting |

**Belum digali:** siapa persisnya yang berwenang approve write-off/adjustment per nilai ambang
(mis. batas nominal yang butuh approval berjenjang), dan apakah role di atas sudah final atau
masih berubah. Dicatat sebagai bagian `FIN-OQ-004`.

## Business Rules dan Invariants

Baris bertipe **Fact** berasal dari `FIN-BRD-V2-0.3` bagian 10 (15 aturan bisnis yang sudah
dirumuskan owner sendiri sebelum sesi ini). Baris bertipe **Decision** adalah hasil ratifikasi
eksplisit pada sesi wawancara ini.

| # | Tipe | Aturan |
|---|---|---|
| 1 | Fact | Sumber Billing yang sudah final hanya boleh membuat Finance AR/AP tepat satu kali per handoff/version. |
| 2 | Fact | Perubahan Finance setelah finalisasi memakai record adjustment/write-off; jumlah sumber asli tidak pernah ditimpa diam-diam. |
| 3 | Fact | Alokasi pembayaran boleh parsial dan many-to-many bila bisnis membutuhkan. |
| 4 | Fact | Transaksi Finance tidak dianggap sudah terposting ke Accounting hanya karena sudah ada di Finance. |
| 5 | Decision (`FIN-DEC-001`) | Publikasi ke Accounting wajib idempotent dan tertelusur lewat `EventNumber`, `SourceTransactionId`, `SourceVersion`, `CorrelationId`, `CausationId` — kontrak `ACC-XMOD-0.2` diratifikasi apa adanya. |
| 6 | Fact | Identifier pasien tidak pernah dikirim dalam pesan Finance → Accounting. |
| 7 | Fact | Untuk kontrak Accounting saat ini, hanya IDR yang boleh dikirim. |
| 8 | Fact | Reversal bersifat kompensasi: fakta asli (tender/receipt/alokasi) tetap tertelusur, fakta reversal baru mengoreksi saldo. |
| 9 | Fact | `BilCashierShift.SystemCash` milik Billing. Finance membaca/merekonsiliasi, tidak pernah menghitung ulang atau menulis saldo shift kasir. |
| 10 | Decision (`FIN-DEC-004`) | Bila tender sukses terjadi sementara invoice sumber belum FINAL, Finance tetap mencatat `FinReceipt` secara operasional, tetapi kejadian Accounting-nya berstatus `HELD_FOR_FINALIZATION` sampai invoice final atau kebijakan pengakuan disetujui. |
| 11 | Fact | Jumlah yang sudah dibayar pasien tidak boleh dibuat ulang sebagai Finance AR. AR hanya dibuat untuk kewajiban yang masih outstanding sesuai handoff Billing yang disetujui. |
| 12 | Fact | Setiap tender Billing sukses yang masuk scope Finance membuat maksimal satu `FinReceipt`; replay dari `TenderId`/provider event/reversal source harus aman (tidak dobel). |
| 13 | Decision (`FIN-DEC-003`) | Titik pengakuan fee dokter memakai Opsi B: event AR hanya memposting AR/pendapatan; `PENGAKUAN-HUTANG-DOKTER` baru terbit setelah `DoctorServiceFee` disetujui — mencegah pemostingan ganda komponen `JASA_MEDIS`. |
| 14 | Decision (`FIN-DEC-006`) | `BilArHandoff` diperluas dengan `DebtorType` baru `EMPLOYEE_BENEFIT` beserta field pemilik manfaat (`BenefitOwnerId`) dan relasi (`SELF`/`SPOUSE`/`CHILD`/dst), bukan tabel handoff terpisah. |
| 15 | Fact | Uang kas kasir pasien dan Petty Cash adalah dua pool terpisah; disbursement Petty Cash tidak boleh mengubah `BilCashierShift.SystemCash`/`PhysicalCash`, dan penerimaan kasir tidak boleh mengubah `FinPettyCashBudget.CurrentBalance`. |

## Skenario Normal dan Exception

Belum digali secara mendalam pada pass ini — baru tercakup sepanjang yang dibutuhkan untuk
menutup klaster lintas-modul (lihat `FIN-DEC-004` untuk skenario "bayar sebelum invoice
final", dan `FIN-DEC-003` untuk skenario fee dokter). Skenario normal/pembatalan/koreksi untuk
AR, AP, dan Cash Management secara rinci ada di `FIN-OQ-004`.

## Frontend Decision Authority

Belum digali pada pass ini — menunggu klaster AR/AP/Cash Management dan
`/design-business-module`.

## Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `FIN-DEC-000` | Decision | Batas scope modul Finance Management (`FIN-SC-001`..`007` di dalam scope; `FIN-OOS-001`..`004` di luar scope) dikonfirmasi apa adanya. | Yasmin | `approved` | Yasmin, 20 September 2026 | Sesi wawancara ini; `FIN-BRD-V2-0.3` bagian 4 |
| `FIN-DEC-001` | Decision | Finance meratifikasi kontrak `ACC-XMOD-0.2`: arah satu arah Billing→Finance→Accounting, Finance sebagai satu-satunya penerbit kejadian, bentuk pesan 12 field wajib diterima apa adanya. | Yasmin (Finance) — menjawab pertanyaan #1 dan #2 paket Rizki | `approved` | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 3, 8; `FIN-ACC-XMOD-V2-0.3` bagian 4 |
| `FIN-DEC-002` | Decision | Katalog 17 `EventTypeCode` yang diusulkan di `FIN-ACC-XMOD-V2-0.3` bagian 6-7 (PENGAKUAN-PIUTANG s.d. PEMBALIKAN-PENERIMAAN-KASIR) diajukan sekaligus ke Accounting sebagai starting point, bukan bertahap per fase. | Yasmin (Finance) — menjawab pertanyaan #3 paket Rizki; **butuh ratifikasi bersama Accounting**, belum final dari sisi Accounting | `approved` (sisi Finance) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 3; `FIN-ACC-XMOD-V2-0.3` bagian 6-7 |
| `FIN-DEC-003` | Decision | Titik pengakuan fee dokter = Opsi B: event AR hanya AR/pendapatan; `PENGAKUAN-HUTANG-DOKTER` terbit terpisah setelah `DoctorServiceFee` disetujui. | Yasmin (Finance) + Medical Fee + Accounting (titik sentuh) | `approved` (sisi Finance) | Yasmin, 20 September 2026 | `FIN-ACC-XMOD-V2-0.3` bagian 7; `FIN-BRD-V2-0.3` bagian 12 |
| `FIN-DEC-004` | Decision | Kebijakan publikasi pra-finalisasi = `HELD_FOR_FINALIZATION`: `FinReceipt` tercatat penuh di Finance saat diterima, kejadian Accounting ditahan sampai invoice FINAL/kebijakan pengakuan disetujui. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-BIL-011; `FIN-MVP-PRD-V2-0.3` FIN-COL-05 |
| `FIN-DEC-005` | Decision | Bentuk kontrak Billing collection→Finance receipt yang diminta Finance ke Billing = tabel handoff persisted `BilCollectionHandoff`, pola sama dengan `BilArHandoff`/`BilApHandoff`, dikunci `TenderId`. | Yasmin (Finance) — **titik sentuh, butuh konfirmasi owner Billing** | `approved` (sisi Finance, permintaan ke Billing) | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` bagian 5.1; `FIN-BRD-V2-0.3` bagian 5.2 |
| `FIN-DEC-006` | Decision | `BilArHandoff` diperluas dengan `DebtorType EMPLOYEE_BENEFIT` + field `BenefitOwnerId`/relasi, bukan handoff terpisah. | Yasmin (Finance) — **titik sentuh, butuh konfirmasi Billing + HR** | `approved` (sisi Finance, permintaan ke Billing/HR) | Yasmin, 20 September 2026 | `FIN-BRD-V2-0.3` bagian 6.3; `FIN-BRD-V2-0.3` bagian 12 (Open Decision "Employee-benefit handoff fields") |
| `FIN-DEC-007` | Decision | Preferensi Finance untuk autentikasi pengiriman kejadian ke Accounting = service account/API key internal lewat mekanisme auth existing (bukan skema OAuth baru). | Yasmin (Finance) — menjawab pertanyaan #4 paket Rizki; **keputusan final bersama Accounting + Platform** | `draft` (preferensi Finance, belum final tiga pihak) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 4; `FIN-BRD-V2-0.3` bagian 12 |
| `FIN-DEC-008` | Decision | Cutover kejadian Finance→Accounting mulai berlaku sejak tanggal go-live integrasi (Phase 5) diaktifkan, tidak retroaktif; saldo AR/AP/kas sebelum tanggal itu diinput Accounting sebagai saldo awal (opening balance) manual satu kali. | Yasmin (Finance) — menjawab pertanyaan #6 paket Rizki | `approved` (tanggal pasti masih menunggu Phase 5 siap) | Yasmin, 20 September 2026 | `docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md` bagian 8 poin 6 |
| `FIN-DEC-009` | Decision (`FIN-CQ-01`) | Pencatatan keputusan Petty Cash dipisah berdasarkan jenis: aturan OPERASIONAL Petty Cash sendiri (siklus voucher, status budget, jenis movement) TETAP di blueprint `billing-kasir` (`PC-DEC-*`/`PC-DES-*`) dan TIDAK dibuka ulang di blueprint ini. Blueprint `finance-management` hanya mencatat keputusan baru yang memang milik batas Finance→Accounting (katalog event, pemetaan kategori-ke-akun — lihat `FIN-DEC-002`) dan pekerjaan FE parity di rute `/finance/`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` §9 `FIN-CQ-01`; `docs/module-blueprints/billing-kasir/blueprint-manifest.md` (`blueprint_shape: SINGLE`) |
| `FIN-DEC-010` | Decision | Bucket aging AR memakai 4 kelompok berbasis hari sejak `DueDate`: 0-30, 31-60, 61-90, dan lebih dari 90 hari. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-AR-007 |
| `FIN-DEC-011` | Decision | Alokasi pembayaran payer/asuransi/corporate yang diterima SETELAH AR tercatat dilakukan MANUAL PENUH — staf AR memilih sendiri item piutang mana yang dibayar pada setiap penerimaan, bukan pencocokan otomatis (FIFO). | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-AR-005 |
| `FIN-DEC-012` | Decision | Write-off dan adjustment AR memakai pola maker-checker: diajukan petugas Billing/AR, disetujui pengguna LAIN yang memegang wewenang approval Finance/AR (pengaju tidak boleh menyetujui permohonan sendiri). Wewenang approval ditentukan lewat konfigurasi permission/role, bukan nama jabatan yang di-hardcode. Tidak memakai ambang nominal bertingkat. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026 |
| `FIN-DEC-013` | Decision | AR untuk payer/asuransi diakui SEGERA saat handoff Billing (`BilArHandoff` tipe `PAYER`) diterima, terlepas dari kelengkapan dokumen klaim. Status kelengkapan dokumen klaim dilacak sebagai atribut terpisah (`FinReceivableDocument`) yang tidak pernah mengubah jumlah piutang asli. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-BRD-V2-0.3` bagian 6.1; `FIN-PRD-V2-0.3` FIN-AR-004 |
| `FIN-DEC-014` | Decision | Finance AP memakai master `MstSupplier` existing (`Areas/Administrator/MasterData/Models/MstSupplier.cs`, milik Administrator) apa adanya lewat `SupplierId`. Tidak membuat master Supplier baru di Finance. Menutup `FIN-OQ-002`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` temuan langsung `MstSupplier.cs` |
| `FIN-DEC-015` | Decision | `FinSupplierPayable` untuk rilis pertama dipicu lewat INPUT MANUAL staf Finance AP (nomor invoice supplier, nominal, termin) — tidak menunggu modul Purchase Order/Receive Order/Supplier Invoice yang saat ini belum ada satu barisnya pun di backend. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `01-existing-capability-map.md` — konfirmasi langsung tidak ada modul Purchasing; `FIN-BRD-V2-0.3` bagian 7.1 |
| `FIN-DEC-016` | Decision | Identitas pemilik manfaat karyawan (`BenefitOwnerId`) ditentukan di sumber kunjungan (Registrasi/Billing) berdasarkan hubungan pasien-karyawan dan eligibilitas yang berlaku saat pelayanan. Finance TIDAK menentukan ulang identitas ini; ketidaksesuaian dikoreksi lewat workflow koreksi (bukan Finance mengubah sendiri). | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; konsisten dengan `FIN-DEC-006` |
| `FIN-DEC-017` | Decision | Nominal setoran Bank Deposit divalidasi otomatis terhadap saldo kas yang benar-benar masih tersedia untuk disetor (kas fisik dikurangi setoran sebelumnya dan koreksi/selisih yang sudah disahkan). Partial deposit diperbolehkan. Validasi WAJIB dijalankan backend saat posting untuk mencegah double allocation atau saldo menjadi minus. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; `FIN-PRD-V2-0.3` FIN-CASH-002 |
| `FIN-DEC-018` | Decision | Finance tetap memiliki master operasional `FinCurrency`/`FinExchangeRate` sendiri untuk mencatat transaksi non-IDR di level operasional, walau kontrak Accounting saat ini hanya menerima IDR. Menutup `FIN-OQ-003`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` bagian 10 |
| `FIN-DEC-019` | Decision | Doctor Payment BOLEH direkap periodik — satu `DoctorPayment` dapat melunasi banyak `DoctorPayable` sekaligus lewat `FinPaymentAllocation`, agar tiap payment tetap tertelusur balik ke fee-fee individual penyusunnya. Alasan owner: mencegah masalah sistem lama — dokter dobel/lupa dibayar karena rekonsiliasi manual lewat Excel terpisah dari sistem utama. Menutup `FIN-OQ-006`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026. **Diperkuat bukti lapangan** (`evidence/03-referensi-meeting-rs-mmc.md`): transcript "Akutansi Zoom Meeting" 15 Juli 2026 menunjukkan praktik RS MMC memang rekap bulanan per dokter lewat menu Ap Dokter, dengan volume ±220 dokter/bulan dan ±4 hari kerja karena approve satu-per-satu |
| `FIN-DEC-020` | Decision | Formula Daily Cash final apa adanya: opening + penerimaan kasir (`FinReceipt`) + penerimaan lain yang disetujui − disbursement − setoran bank = closing. Petty Cash top-up/disbursement TIDAK masuk formula ini (pool terpisah, konsisten dengan aturan bisnis #15). Menutup `FIN-OQ-007`. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | `FIN-PRD-V2-0.3` FIN-CASH-003 |
| `FIN-DEC-021` | Decision | Bila `FinReceipt` yang sudah dialokasikan ke AR kemudian tender aslinya di-reverse Billing, sistem WAJIB membuat baris alokasi pembalik (reversal allocation, bukan menghapus histori) dan AR yang tadinya terbayar otomatis kembali berstatus `OUTSTANDING`. Menutup `FIN-OQ-008`. **PERINGATAN: sebagian keputusan ini adalah desain baru, bukan meniru pola yang sudah terbukti** — lihat kolom Evidence. | Yasmin (Finance) | `approved` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; konsisten aturan bisnis #8. **Bukti lapangan bersifat SEBAGIAN** (`evidence/03-referensi-meeting-rs-mmc.md`): transcript "Keuangan AP Zoom Meeting" 10 Juni 2026 memang menunjukkan pola reversal-bukan-delete dan tagihan otomatis dapat di-settle ulang — TETAPI hanya untuk skenario **full** cancel/settlement. Skenario **partial** (bayar sebagian + sisa jadi piutang) belum pernah ada di sistem lama, dan fitur Batal Settlement sendiri diakui tim RS MMC belum matang. Penanganan partial karena itu MUST diuji khusus, tidak boleh diasumsikan aman karena "sudah begitu di sistem lama" |
| `FIN-DEC-022` | Decision | Approval pembayaran AP (Supplier dan Doctor) TIDAK memakai pola maker-checker sederhana yang sama seperti AR (`FIN-DEC-012`). AP memakai approval berjenjang berdasarkan total nominal rekap pembayaran. Ambang nominal pastinya BELUM ditentukan — dicatat sebagai `FIN-OQ-010`, bukan diputuskan sekarang. Menutup `FIN-OQ-009` (bentuk aturan), membuka `FIN-OQ-010` (nilai ambang). | Yasmin (Finance) | `approved` (bentuk aturan); ambang nominal `open` | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026. **Diperkuat bukti lapangan** (`evidence/03-referensi-meeting-rs-mmc.md`): di sistem lama TIDAK ADA pemisahan pengaju/penyetuju sama sekali untuk fee dokter — tiga orang tim akuntansi sama-sama membuat dan menyetujui, dan tim mengakui sendiri risikonya ("bisa saling membantu tanpa sadar dobel kerjakan dokter yang sama"). Pembayaran supplier bahkan tidak punya langkah approval terpisah. Jadi keputusan ini **menutup gap kontrol yang nyata**, bukan sekadar memilih pola berbeda |
| `FIN-DEC-023` | Decision | Finance mengajukan draft nama field pesan saldo subledger per periode — `LegalEntityId`, `AccountingPeriodCode`, `ControlAccountCode`, `SubledgerBalance`, `AsOfDate` — mengikuti pola penamaan 12-field `ACC-XMOD-0.2` yang sudah diratifikasi (`FIN-DEC-001`), lalu dikirim ke Accounting (Rizki) untuk dikonfirmasi/diubah. Bukan keputusan final sepihak. Menutup `FIN-OQ-001` (sisi Finance); menunggu konfirmasi Accounting. | Yasmin (Finance) — draft, **butuh konfirmasi Accounting** | `draft` (diajukan, belum dikonfirmasi Accounting) | Yasmin, 20 September 2026 | Jawaban langsung owner, 20 September 2026; `FIN-ACC-XMOD-V2-0.3` bagian 12 |

## Acceptance Criteria

Belum dirumuskan untuk AR/AP/Cash — akan ditambahkan setelah klaster berikutnya digali.
Untuk klaster lintas-modul yang sudah ditutup, acceptance criteria yang sudah dapat diuji:

1. Kejadian yang dikirim Finance ke Accounting memuat seluruh 12 field wajib `ACC-XMOD-0.2`
   tanpa data pasien (`FIN-DEC-001`).
2. Kejadian `PENGAKUAN-PIUTANG` untuk tagihan yang mengandung jasa medis dokter **tidak**
   membawa komponen `JASA_MEDIS`; komponen itu hanya muncul lewat kejadian
   `PENGAKUAN-HUTANG-DOKTER` setelah `DoctorServiceFee` berstatus Approved (`FIN-DEC-003`).
3. `FinReceipt` untuk tender sukses pada invoice yang masih OPEN tetap tercatat dan terlihat di
   Finance, tetapi baris outbox terkait berstatus `HELD_FOR_FINALIZATION` sampai invoice FINAL
   (`FIN-DEC-004`).
4. Tidak ada transaksi Finance sebelum tanggal go-live integrasi yang terkirim sebagai kejadian
   Accounting; saldo sebelum tanggal itu tidak direkonstruksi otomatis dari histori Finance
   (`FIN-DEC-008`).

## Open Questions dan Blocker

Ditutup pada Closure pass 20 September 2026: ~~`FIN-OQ-001`~~ (`FIN-DEC-023`, draft sisi
Finance — status detail tetap dipantau di `FIN-OQ-011`), ~~`FIN-OQ-002`~~ (`FIN-DEC-014`),
~~`FIN-OQ-003`~~ (`FIN-DEC-018`), ~~`FIN-OQ-005`~~ (`FIN-DEC-012`), ~~`FIN-OQ-006`~~
(`FIN-DEC-019`), ~~`FIN-OQ-007`~~ (`FIN-DEC-020`), ~~`FIN-OQ-008`~~ (`FIN-DEC-021`),
~~`FIN-OQ-009`~~ (`FIN-DEC-022`, bentuk aturan — nilai ambang tetap terbuka di `FIN-OQ-010`).
`FIN-OQ-004` sepenuhnya terurai menjadi keputusan `FIN-DEC-010` s.d. `023`.

Sisa yang masih genuinely terbuka:

| ID | Pertanyaan | Owner | Memblokir |
|---|---|---|---|
| `FIN-OQ-010` | Ambang nominal approval berjenjang AP — bentuk aturannya sudah diputuskan `FIN-DEC-022`, angka rupiah persisnya belum. **Diperluas 20 September 2026** setelah bukti meeting masuk: ambangnya perlu ditetapkan **per jenis transaksi secara terpisah** (supplier vs dokter vs write-off AR), karena karakter risikonya berbeda — honor dokter adalah rekap bulanan yang menggabungkan puluhan sampai ratusan fee per dokter dengan nominal total besar, sedangkan write-off AR umumnya keputusan per-transaksi tunggal. Jejak audit juga MUST mencatat **tingkat approval mana** yang berlaku untuk tiap transaksi, bukan hanya siapa yang menyetujui. | Yasmin (Finance) + Finance Supervisor | `DESIGN` untuk `EPIC FIN-09` — tidak memblokir pemodelan data, hanya aturan validasi angka. `EPIC FIN-09` sudah `POST-MVP` |
| `FIN-OQ-011` | Konfirmasi Accounting (Rizki) atas draf nama field saldo subledger per periode (`FIN-DEC-023`). | Accounting | `LATER SLICE` — `FIN-ACC-05`, bukan Phase 0 |
| `FIN-CQ-02` (dari `01-existing-capability-map.md` §9) | DTO/response shape persis dan daftar `[AccessPermission]` per endpoint Petty Cash belum dibaca detail — dibutuhkan sebagai acuan pola saat mendesain endpoint AR/AP/Doctor Payment supaya konsisten. | Yasmin (Finance) | ~~`DESIGN`~~ — **DITUTUP 20 September 2026** oleh pass `/design-business-module` |
| ~~`FIN-OQ-013`~~, ~~`FIN-OQ-014`~~ | Keduanya **DITUTUP 20 September 2026** oleh amendment revisi 2 pada `FIN-BP-001`. `FIN-DES-025` menggantikan `FinDoctorPayable` dengan `FinMedicalServicePayable`; `FIN-DES-026`..`028` menambahkan `FinPaymentDeduction`, kolom `NetTransferAmount`, dan aturan bahwa potongan tidak mengurangi sisa utang. Rinciannya ada pada bagian AMENDMENT REVISI 2 di `02-backend-architecture.md`. Keputusan arsitekturnya masih `draft` dan menunggu persetujuan owner. | — | — |
| `FIN-OQ-014` | **Amendment yang dituntut pass Medical Fee, 20 September 2026.** `MF-DEC-002` memperluas penerima jasa menjadi dokter **dan** tenaga kesehatan lain, dan `MF-DEC-008` memutuskan `FinDoctorPayable` **digantikan** satu entity utang jasa tenaga medis yang membedakan jenis penerima lewat kolom — mengikuti pola `FinPayment` yang sudah melayani supplier dan dokter sekaligus. Berkas `FIN-BP-001` yang terdampak: `02-backend-architecture.md`, `erd/payable.md`, `erd/data-dictionary.md`, `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, `contracts/state-transition-matrix.md`, `04-prd-to-mvp.md` (`EPIC FIN-08`). | Yasmin (owner Finance) | `DESIGN` untuk `EPIC FIN-08` — sudah `POST-MVP` dan **nol baris kode**, sehingga tidak membongkar apa pun. Lihat `MF-CQ-02` pada blueprint `medical-fee` |
| `FIN-OQ-013` | **Gap yang dibuka pass Medical Fee, 20 September 2026.** `MF-DEC-005` memutuskan Medical Fee menyerahkan jasa **kotor**, dan seluruh potongan per penerima per periode — PPh 21, kasbon, potongan hutang pasien yang dijamin potong honor, sitting fee, KSO, iuran kerohanian — diterapkan **Finance** saat menyusun rekap pembayaran. `FinPayment` hasil rancangan `FIN-DES-015` **tidak punya tempat sama sekali** untuk itu: ia hanya memiliki `TotalAmount` dan alokasi ke utang. Konsekuensi yang MUST diselesaikan: (a) Finance perlu entity potongan/tambahan per pembayaran; (b) invariant `FinPayment.TotalAmount = Σ alokasi` pada `FIN-DES-015` **tidak lagi berlaku apa adanya** — nilai transfer bersih berbeda dari jumlah utang yang dilunasi. | Yasmin (owner Finance + Medical Fee) | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP` sehingga **tidak menahan MVP**, tetapi MUST ditutup sebelum rumpun pembayaran dibangun. Lihat `MF-CQ-01` pada blueprint `medical-fee` |
| `FIN-OQ-012` | Pembayaran honor dokter di RS MMC dipisah dua tahap — tanggal 5 untuk pasien pribadi dan asuransi yang sudah membayar ke rumah sakit, tanggal 10 untuk sisanya termasuk kasus rumah sakit menalangi dulu. Apakah pola itu dipertahankan di V2? Bila ya, Finance perlu cara mengetahui status bayar tagihan sumber untuk setiap utang dokter, dan jalur datanya belum dirancang sama sekali. | Yasmin (Finance) + Billing | `DESIGN` untuk `EPIC FIN-09` — sudah `POST-MVP`, tidak menahan MVP. Dibuka 20 September 2026 dari bukti meeting (`evidence/03-referensi-meeting-rs-mmc.md`) |

## Langkah berikutnya

**Status akhir sesi 20 September 2026:** Scope pass dan Closure pass sama-sama selesai dalam
satu sesi. 23 keputusan (`FIN-DEC-001`..`023`) `approved`, mencakup:

- Seluruh 6 pertanyaan paket Rizki (Accounting) — siap dikirim balik sebagai jawaban resmi.
- Titik pengakuan fee dokter, kebijakan pra-finalisasi, kontrak Billing collection, perluasan
  employee benefit, autentikasi, cutover.
- Kepemilikan decision log Petty Cash (dipisah dari billing-kasir — `FIN-CQ-01` tertutup).
- Aturan inti AR: aging, alokasi, wewenang write-off (maker-checker), gerbang klaim.
- Aturan inti AP: master Supplier (reuse existing), trigger payable (manual MVP), pola
  pembayaran dokter (rekap periodik + `FinPaymentAllocation`), approval berjenjang (bentuk).
- Aturan inti Cash: employee benefit owner, validasi Bank Deposit, formula Daily Cash,
  reversal setelah alokasi, master Currency.

**Yang TIDAK memblokir lanjut ke desain:** `FIN-OQ-010` (angka ambang approval AP) dan
`FIN-OQ-011` (konfirmasi Accounting atas draf field subledger) — keduanya bisa diselesaikan
paralel saat `/design-business-module` berjalan, tidak perlu menghentikan pekerjaan.

Tidak ada Conflict yang tersisa. Tidak ada keputusan bisnis/klinis/finansial kritis yang masih
tertutup rapat. Opsi lanjutan ditawarkan ke owner — lihat pesan chat.

**Keputusan owner pada 20 September 2026:** jalankan `/trace-existing-capabilities` lebih
dahulu untuk membentuk `01-existing-capability-map.md` sebelum melanjutkan wawancara AR/AP/Cash
Management, karena field persis `BilArHandoff`/`BilApHandoff`/`BilTender`/Petty Cash API belum
diaudit secara canonical (lihat "Catatan cara kerja" di atas). Setelah capability map tersedia,
lanjutkan `/grill-me` (Closure pass) untuk menutup `FIN-OQ-002` s.d. `FIN-OQ-005`.

**Status 20 September 2026 (lanjutan):** `01-existing-capability-map.md` selesai dibentuk.
Temuan yang mengubah urutan Closure pass berikutnya:

- `FIN-CQ-01` (baru): keputusan bisnis/arsitektur Petty Cash (`PC-DEC-*`/`PC-DES-*`) ternyata
  sudah `approved` di blueprint **billing-kasir**, bukan di blueprint ini, walau kodenya sudah
  pindah folder ke `Corporate/FinanceManagement`. Perlu diputuskan lebih dulu: Cash Management
  pada blueprint ini merujuk ke billing-kasir untuk Petty Cash (tidak membuka ulang
  keputusannya), atau memindahkan pencatatannya ke sini.
- `FIN-OQ-004`/`FIN-OQ-005` (AR/AP) TIDAK punya keputusan tertunda serupa di blueprint lain —
  aman dilanjutkan sepenuhnya di blueprint ini.
- Field `BilArHandoff.DebtorType` dan `BillingHandoffStatuses` terverifikasi langsung dari
  source: hanya `PAYER`/`PATIENT_GUARANTOR` dan `CREATED`/`ACKNOWLEDGED`. Ini memperkuat
  `FIN-DEC-005` dan `FIN-DEC-006` sebagai perluasan yang benar-benar dibutuhkan, bukan sekadar
  asumsi dokumen.

Detail lengkap ada di `01-existing-capability-map.md`.
