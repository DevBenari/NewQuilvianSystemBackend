# Matriks Perubahan Status — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-STATE-1.1` |
| `last_changed_in` | `FIN-STATE-1.1` — amendment 25 September 2026 (bagian 1 dan 9) |
| Status | `approved` dan `locked` — revisi 1.1 disetujui dan dikunci owner 25 September 2026 |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-25 |
| Input revision | `00-interview-decisions.md` — `FIN-DEC-001`..`044` |
| Dampak kompatibilitas | **Satu transisi dicabut** (`HELD_FOR_FINALIZATION` tidak lagi dihasilkan, bagian 9) dan **empat nilai `HandoffType` ditambahkan** (bagian 1). Tidak ada nilai status yang dihapus dari basis data |

Transisi yang **tidak sah** ikut dicantumkan. Matriks yang hanya memuat jalur sah tidak dapat
dipakai menguji apa pun.

---

## 1. Fakta masuk dari Billing — `FinBillingHandoffIntake`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Fakta baru terbaca dari Billing | `NEW` | Sistem | Belum ada baris dengan `HandoffType` + `SourceHandoffKey` yang sama | Baris kedua ditolak unique index, fakta dianggap sudah masuk |
| `NEW` | Olah fakta | `CONSUMED` | Sistem | Piutang, utang, atau penerimaan berhasil dibuat | Bila gagal, status menjadi `ERROR`, bukan tetap `NEW` |
| `NEW` | Olah fakta, tetapi gagal | `ERROR` | Sistem | — | `ErrorMessage` wajib terisi |
| `ERROR` | Ulangi pengolahan | `CONSUMED` | Petugas Finance | `RetryCount` bertambah | — |
| `CONSUMED` | Kirim ACK ke Billing | `ACKNOWLEDGED` | Sistem | Billing menerima ACK | Bila gagal, tetap `CONSUMED` dan dicoba lagi |
| `ACKNOWLEDGED` | Apa pun | — | — | **Status akhir.** Tidak ada transisi keluar | Permintaan ditolak `422` |
| `CONSUMED` | Ulangi pengolahan | — | — | **Tidak sah.** Fakta yang sudah diolah tidak boleh diolah ulang | `409` — akan melahirkan piutang kedua |

**Empat jenis fakta ditambahkan revisi 1.1** (`FIN-DEC-040`..`044`). Siklus statusnya **sama
persis** dengan tabel di atas — yang bertambah hanya nilai `HandoffType` yang sah, sehingga
tidak ada transisi baru yang perlu diuji tersendiri:

| `HandoffType` | Sumber fakta di Billing | `SourceHandoffKey` diambil dari | Melahirkan |
|---|---|---|---|
| `DEPOSIT_MOVEMENT` | `BilDepositMovement` (`TOP_UP`/`ALLOCATION`/`RELEASE`/`REVERSAL`) | `IdempotencyKey` | Baris kotak keluar; **tidak** melahirkan `FinReceipt` untuk tipe `ALLOCATION`/`RELEASE` |
| `REFUNDABLE_CREDIT` | `BilRefundableCredit` `ALLOCATION_EXCESS` berstatus `AVAILABLE` | `Id` (sumber tidak punya kunci sendiri) | Baris kotak keluar `PENGAKUAN-KELEBIHAN-BAYAR` |
| `REFUND_CASE` | `BilRefundCase` berstatus `EXECUTED`, sumber `ALLOCATION_EXCESS` | `IdempotencyKey` | Baris kotak keluar `PENGEMBALIAN-UANG-MUKA` |
| `CASH_VARIANCE_REVIEW` | `BilCashVarianceReview` | `Id` (sumber tidak punya kunci sendiri) | Baris kotak keluar `SELISIH-KAS-SHIFT` |

**`TargetEntityId` boleh kosong untuk keempat jenis ini** pada kasus yang tidak melahirkan entity
Finance baru (mis. `ALLOCATION` hanya menerbitkan kejadian, tidak membuat penerimaan). Kolomnya
sudah nullable, jadi tidak ada perubahan skema untuk itu.

**ACK ke Billing tidak berlaku untuk keempat jenis ini.** `BilDepositMovement`,
`BilRefundableCredit`, `BilRefundCase`, dan `BilCashVarianceReview` **tidak punya kolom status
handoff** yang bisa ditandai, dan Finance dilarang menulis ke tabel Billing. Karena itu keempatnya
berhenti di `CONSUMED` sebagai status akhir praktisnya:

| Dari status | Tindakan | Ke status | Syarat | Bila dilanggar |
|---|---|---|---|---|
| `CONSUMED` | — | — | **Status akhir** untuk `DEPOSIT_MOVEMENT`, `REFUNDABLE_CREDIT`, `REFUND_CASE`, `CASH_VARIANCE_REVIEW` | Mencoba mengirim ACK ke Billing untuk keempat jenis ini melanggar larangan tulis lintas modul (`integration-contract.md` bagian 6) |

---

## 2. Piutang — `FinReceivable`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Fakta AR dari Billing diolah | `OUTSTANDING` | Sistem | `SourceHandoffKey` belum pernah dipakai | `409` |
| `OUTSTANDING` | Alokasi penerimaan sebagian | `PARTIAL` | Petugas AR | Jumlah alokasi < `OutstandingAmount` | — |
| `OUTSTANDING` | Alokasi penerimaan penuh | `SETTLED` | Petugas AR | `OutstandingAmount` menjadi nol | — |
| `PARTIAL` | Alokasi penerimaan penuh | `SETTLED` | Petugas AR | `OutstandingAmount` menjadi nol | — |
| `PARTIAL` | Pembalikan alokasi | `OUTSTANDING` | Petugas AR | Seluruh alokasi dibalik | Baris alokasi pembalik dibuat, bukan dihapus |
| `SETTLED` | Pembalikan alokasi | `PARTIAL` atau `OUTSTANDING` | Sistem | Tender aslinya dibalik Billing (`FIN-DEC-021`) | Piutang **otomatis** terbuka kembali |
| `OUTSTANDING` atau `PARTIAL` | Penghapusan piutang disetujui | `WRITTEN_OFF` | Penyetuju Finance/AR | Penyetuju ≠ pengaju | `422` |
| `OUTSTANDING` | Pembatalan | `CANCELLED` | Penyetuju Finance/AR | Belum ada alokasi sama sekali | `422` bila sudah ada penerimaan yang dialokasikan |
| `SETTLED` | Penghapusan piutang | — | — | **Tidak sah.** Piutang lunas tidak dapat dihapusbukukan | `422` |
| `WRITTEN_OFF` | Alokasi penerimaan | — | — | **Tidak sah.** Gunakan pembalikan penghapusan lebih dahulu | `422` |
| `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

**Contoh transisi yang paling sering ditanya.** Piutang penjamin Rp 10.000.000 berstatus
`OUTSTANDING`. Penjamin membayar Rp 4.000.000 → status `PARTIAL`, sisa Rp 6.000.000. Penjamin
melunasi Rp 6.000.000 → status `SETTLED`, sisa nol. Ternyata transfer yang pertama ditarik
kembali oleh bank dan Billing membalik tendernya → sistem membuat alokasi pembalik
Rp 4.000.000, piutang kembali `PARTIAL` dengan sisa Rp 4.000.000. Tidak ada satu pun baris lama
yang diubah atau dihapus.

---

## 3. Koreksi dan penghapusan piutang — `FinReceivableAdjustment`, `FinReceivableWriteOff`

Keduanya memakai matriks yang sama.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Ajukan | `REQUESTED` | Petugas Billing/AR yang punya hak pengajuan | Alasan wajib diisi | `400` |
| `REQUESTED` | Setujui | `APPROVED` | Pengguna lain yang punya hak persetujuan Finance/AR | **Penyetuju MUST NOT sama dengan pengaju** (`FIN-DEC-012`) | `422` dengan pesan "Pengaju tidak boleh menyetujui permohonannya sendiri" |
| `REQUESTED` | Tolak | `REJECTED` | Pengguna lain yang punya hak persetujuan | Alasan penolakan wajib | `400` |
| `APPROVED` | Apa pun | — | — | **Status akhir.** Koreksi yang salah dibetulkan dengan koreksi baru arah sebaliknya | `422` |
| `REJECTED` | Apa pun | — | — | **Status akhir.** Ajukan baru bila masih diperlukan | `422` |

`OutstandingAmount` piutang **hanya** berubah saat status menjadi `APPROVED`. Selama masih
`REQUESTED`, nilai piutang tidak bergerak sama sekali.

---

## 4. Penerimaan — `FinReceipt`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Tender Billing berhasil | `RECEIVED` | Sistem | `SourceTenderId` belum pernah dipakai | `409` — tidak membuat penerimaan kedua |
| — | Catat penerimaan non-kasir | `RECEIVED` | Petugas AR | Nominal > 0 | `400` |
| `RECEIVED` | Alokasi sebagian | `RECEIVED` | Petugas AR | `UnallocatedAmount` masih > 0 | — |
| `RECEIVED` | Alokasi penuh | `ALLOCATED` | Petugas AR | `UnallocatedAmount` menjadi nol | — |
| `ALLOCATED` | Cocokkan dengan rekening koran | `RECONCILED` | Treasury | — | — |
| `RECEIVED` atau `ALLOCATED` | Balik penerimaan | `REVERSED` | Sistem atau Treasury | Tender aslinya dibalik Billing | Seluruh alokasinya ikut dibalik otomatis (`FIN-DEC-021`) |
| `REVERSED` | Alokasi | — | — | **Tidak sah.** Uangnya sudah tidak ada | `422` |
| `RECONCILED` | Balik penerimaan | `REVERSED` | Treasury | Perlu alasan tertulis | Tetap membuat baris pembalik, bukan menghapus |
| Mana pun | Hapus baris | — | — | **Tidak pernah sah.** Pembalikan selalu berupa baris baru (`FIN-DES-012`) | Permintaan tidak disediakan API-nya |

---

## 5. Utang supplier dan utang dokter

Keduanya memakai matriks yang sama.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Input faktur supplier | `OUTSTANDING` | Petugas AP | Pasangan supplier + nomor faktur belum pernah ada | `409` dengan pesan "Faktur supplier ini sudah pernah diinput" |
| — | Bentuk dari fee dokter yang disetujui | `OUTSTANDING` | Sistem | `SourceDoctorServiceFeeId` belum pernah dipakai | `409` |
| `OUTSTANDING` | Pembayaran sebagian | `PARTIAL` | Sistem | Alokasi < `OutstandingAmount` | — |
| `OUTSTANDING` atau `PARTIAL` | Pembayaran penuh | `PAID` | Sistem | `OutstandingAmount` menjadi nol | — |
| `PARTIAL` | Pembalikan alokasi pembayaran | `OUTSTANDING` | Petugas AP | Seluruh alokasi dibalik | — |
| `OUTSTANDING` | Batalkan | `CANCELLED` | Penyetuju AP | Belum ada pembayaran sama sekali | `422` bila sudah dibayar sebagian |
| `PAID` | Batalkan | — | — | **Tidak sah.** Pakai koreksi utang | `422` |
| `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

---

## 6. Pembayaran keluar — `FinPayment`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Susun pembayaran | `DRAFT` | Petugas AP | Rekening sumber wajib dipilih | `400` |
| `DRAFT` | Ubah rincian | `DRAFT` | Petugas AP | — | — |
| `DRAFT` | Ajukan | `SUBMITTED` | Petugas AP | `TotalAmount` = jumlah seluruh alokasi | `422` dengan pesan yang menyebut selisihnya |
| `SUBMITTED` | Setujui | `APPROVED` | Penyetuju sesuai `ApprovalTier` | **Penyetuju MUST NOT sama dengan pengaju**; jenjangnya sesuai total nominal (`FIN-DEC-022`) | `422` |
| `SUBMITTED` | Tolak | `REJECTED` | Penyetuju | Alasan wajib | `400` |
| `APPROVED` | Tandai sudah dibayar | `PAID` | Treasury | Nomor bukti transfer wajib | `400` |
| `DRAFT` atau `SUBMITTED` | Batalkan | `CANCELLED` | Petugas AP | — | — |
| `APPROVED` | Batalkan | `CANCELLED` | Penyetuju | Belum ditandai dibayar | `422` bila sudah `PAID` |
| `DRAFT` | Tandai sudah dibayar | — | — | **Tidak sah.** Wajib lewat pengajuan dan persetujuan | `422` |
| `PAID` | Apa pun | — | — | **Status akhir.** Kekeliruan dibetulkan lewat koreksi utang, bukan dengan mengubah pembayaran | `422` |
| `REJECTED` | Ajukan ulang | — | — | **Tidak sah.** Susun pembayaran baru | `422` |

`OutstandingAmount` utang **hanya** berkurang saat pembayaran menjadi `PAID`. Pembayaran yang
masih `APPROVED` belum mengurangi utang, karena uangnya memang belum keluar.

---

## 7. Setoran bank — `FinBankDeposit`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat draf setoran | `DRAFT` | Treasury | Rekening tujuan wajib dipilih | `400` |
| `DRAFT` | Posting | `POSTED` | Treasury | Nominal ≤ saldo kas tersedia, dihitung ulang saat itu juga (`FIN-DEC-017`) | `422` dengan pesan yang menyebut saldo tersedia |
| `POSTED` | Verifikasi dengan rekening koran | `VERIFIED` | Treasury | — | — |
| `DRAFT` | Batalkan | `CANCELLED` | Treasury | — | — |
| `POSTED` | Batalkan | `CANCELLED` | Penyetuju Finance | Kas harian tanggal itu belum ditutup | `422` bila hari sudah ditutup |
| `VERIFIED` | Batalkan | — | — | **Tidak sah.** Setoran yang sudah cocok dengan bank tidak dibatalkan | `422` |
| `CANCELLED` | Posting | — | — | **Tidak sah.** Buat setoran baru | `422` |

---

## 8. Kas harian — `FinDailyCashSnapshot`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Hari berjalan dimulai | `OPEN` | Sistem | Belum ada baris untuk tanggal itu | `409` |
| `OPEN` | Tutup hari | `CLOSED` | Treasury | Seluruh setoran hari itu sudah `POSTED` atau `CANCELLED` | `422` bila masih ada setoran berstatus `DRAFT` |
| `CLOSED` | Ubah angka | — | — | **Tidak sah.** Angka yang sudah ditutup dibekukan (`FIN-DES-022`) | `422` |
| `CLOSED` | Buka kembali | — | — | **Tidak sah.** Transaksi terlambat masuk ke tanggal berikutnya | `422` |

---

## 9. Kejadian ke Accounting — `FinAccountingEventOutbox`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Fakta Finance tercatat, apa pun status tagihan sumbernya | `PENDING` | Sistem | Ditulis di transaksi yang sama (`FIN-DES-017`). **Kode kejadiannya** yang berbeda menurut status tagihan, bukan status pengirimannya — lihat `integration-contract.md` bagian 5.5 | Transaksi dibatalkan seluruhnya |
| ~~—~~ | ~~Fakta Finance tercatat, tagihan masih `OPEN`~~ | ~~`HELD_FOR_FINALIZATION`~~ | ~~Sistem~~ | **DICABUT revisi 1.1** — `FIN-DEC-004` `superseded` oleh `FIN-DEC-030`. Penerimaan pra-finalisasi kini terbit segera sebagai `PENERIMAAN-UANG-MUKA` berstatus `PENDING` | Kode baru **MUST NOT** menghasilkan status ini lagi |
| `HELD_FOR_FINALIZATION` | Baris warisan ditemukan | `PENDING` | Petugas Finance (satu kali, saat migrasi data) | Hanya untuk baris yang tersimpan **sebelum** `FIN-DEC-030` berlaku. Kode kejadiannya ikut diperiksa: bila tagihan sumbernya masih `OPEN`, baris itu MUST dibetulkan menjadi `PENERIMAAN-UANG-MUKA` | Bila dibiarkan, baris itu tidak akan pernah terkirim karena tidak ada lagi pemicu pelepasan |
| `PENDING` | Kirim ke Accounting | `SENT` | Worker | Endpoint Accounting tersedia | Selama endpoint belum ada, worker dimatikan (`FIN-DES-020`) |
| `SENT` | Terima balasan `201` atau `200` | `ACKNOWLEDGED` | Worker | **Nomor jurnal disimpan bila ada** — `JournalNumber` kosong pada `201` bukan kegagalan (`FIN-DEC-039`) | Menandai `FAILED` hanya karena nomor jurnal kosong adalah kekeliruan; baris akan dikirim ulang tanpa perlu |
| `SENT` | Terima balasan `422` | `HELD` | Worker | Accounting belum punya aturan posting | **MUST NOT** kirim ulang sebagai kejadian baru; Accounting yang melengkapi aturannya |
| `SENT` | Terima balasan `400` | `FAILED` | Worker | Ada isian yang tidak sah | Perbaiki data sumber, lalu kirim ulang baris yang sama |
| `SENT` | Terima balasan `403` atau `409` | `FAILED` | Worker | Masalah hak akses atau mata uang | **MUST NOT** dicoba ulang otomatis |
| `FAILED` | Kirim ulang manual | `PENDING` | Petugas Finance | — | Nomor kejadian tetap sama, tidak membuat baris baru |
| `ACKNOWLEDGED` | Apa pun | — | — | **Status akhir** | `422` |
| `HELD` | Kirim kejadian baru untuk hal yang sama | — | — | **Tidak sah.** Kejadiannya sudah tersimpan di Accounting | Ditangkap unique index lapis kedua |

**Mengapa `422` tidak menjadi `FAILED`.** Balasan `422` berarti pesannya sah dan **sudah
tersimpan** di Accounting, hanya aturan postingnya yang belum ada. Kalau Finance
memperlakukannya sebagai gagal lalu mengirim ulang sebagai kejadian baru, Accounting akan punya
dua kejadian untuk satu fakta. Karena itu statusnya `HELD` — menunggu Accounting, bukan
menunggu Finance.

---

## 10. Ringkasan status akhir

Status berikut tidak punya transisi keluar sama sekali. Kekeliruan setelah status ini dibetulkan
dengan **fakta baru**, bukan dengan mengubah yang lama.

| Entity | Status akhir |
|---|---|
| `FinBillingHandoffIntake` | `ACKNOWLEDGED` untuk `AR`/`AP`/`COLLECTION`/`ADJUSTMENT`; **`CONSUMED`** untuk `DEPOSIT_MOVEMENT`/`REFUNDABLE_CREDIT`/`REFUND_CASE`/`CASH_VARIANCE_REVIEW` (revisi 1.1 — sumbernya tidak punya kolom ACK, lihat bagian 1) |
| `FinReceivable` | `CANCELLED` |
| `FinReceivableAdjustment`, `FinReceivableWriteOff`, `FinPayableAdjustment` | `APPROVED`, `REJECTED` |
| `FinReceipt` | `REVERSED` |
| `FinSupplierPayable`, `FinDoctorPayable` | `CANCELLED` |
| `FinPayment` | `PAID`, `REJECTED` |
| `FinBankDeposit` | `VERIFIED` |
| `FinDailyCashSnapshot` | `CLOSED` |
| `FinAccountingEventOutbox` | `ACKNOWLEDGED` |

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Contract version | `FIN-STATE-0.2` — status `locked` 20 September 2026 |
| Tanggal | 20 September 2026 |
| Keputusan | `FIN-DES-025`..`028` |

## A.1 Utang jasa tenaga medis

Matriks pada bagian 5 di atas **tetap berlaku apa adanya**; yang berubah hanya nama entity —
`FinDoctorPayable` menjadi `FinMedicalServicePayable`. Satu baris ditambahkan:

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Bentuk dari hasil jasa yang disetujui Medical Fee | `OUTSTANDING` | Sistem | `SourceMedicalServiceFeeId` belum dipakai, **dan** hasil jasanya berstatus disetujui di Medical Fee | `409` untuk kunci ganda; `422` bila hasil jasa belum disetujui |

## A.2 Potongan pembayaran

`FinPaymentDeduction` tidak punya status sendiri — ia hidup mengikuti pembayaran induknya.
Yang diatur adalah **kapan** ia boleh diubah:

| Status `FinPayment` | Potongan boleh ditambah | Potongan boleh dihapus | Alasan |
|---|:---:|:---:|---|
| `DRAFT` | Ya | Ya | Masih disusun |
| `SUBMITTED` | Tidak | Tidak | Nilai transfer sudah diajukan untuk disetujui |
| `APPROVED` | Tidak | Tidak | Yang disetujui adalah nilai transfer tertentu |
| `PAID` | Tidak | Tidak | Uang sudah keluar |
| `REJECTED`, `CANCELLED` | Tidak | Tidak | Sudah berakhir |

Bila ada potongan yang keliru dan pembayaran sudah melewati `DRAFT`, jalurnya adalah
membatalkan pembayaran lalu menyusun yang baru — **bukan** mengubah potongan pada pembayaran
yang sudah diajukan. Ini menjaga agar angka yang disetujui penyetuju tidak pernah berubah
diam-diam setelah persetujuannya turun.

## A.3 Transisi pembayaran yang diperketat

Dua baris pada bagian 6 di atas mendapat syarat tambahan:

| Dari status | Tindakan | Ke status | Syarat tambahan | Bila dilanggar |
|---|---|---|---|---|
| `DRAFT` | Ajukan | `SUBMITTED` | `NetTransferAmount` MUST lebih besar dari nol | `422` — pembayaran yang seluruhnya habis oleh potongan tidak perlu ditransfer |
| `APPROVED` | Tandai sudah dibayar | `PAID` | Nilai yang dicatat keluar adalah `NetTransferAmount`, bukan `TotalAmount` | Utang tetap lunas sebesar alokasinya (`FIN-DES-028`) |

Baris kedua adalah tempat kekeliruan paling mudah terjadi: menandai pembayaran lunas sebesar
uang yang keluar akan membuat utang jasa tidak pernah mencapai nol.


---

# AMENDMENT REVISI 4

| Field | Nilai |
|---|---|
| Contract version | `FIN-STATE-1.2` — status `locked` 25 September 2026 (disetujui Yasmin bersama `FIN-DES-037`..`044`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-045`..`055`, `FIN-DES-037`..`044` |
| Dampak kompatibilitas | **Nol.** Seluruh entity di bawah baru; tidak ada status entity `FIN-STATE-1.1` yang berubah |

## B.1 Purchase Order — `FinPurchaseOrder`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat PO | `DRAFT` | Petugas AP | Minimal satu baris item | `400` |
| `DRAFT` | Ubah rincian | `DRAFT` | Petugas AP | — | — |
| `DRAFT` | Ajukan | `PENDING_APPROVAL` | Petugas AP | `ApprovalTier` dihitung dari `TotalAmount` | — |
| `PENDING_APPROVAL` | Setujui | `APPROVED` | Supervisor/Manajer Finance sesuai `ApprovalTier` | **Penyetuju MUST NOT sama dengan pengaju**; jenjang sesuai ambang Rp 50.000.000 (`FIN-DEC-052`) | `422` |
| `PENDING_APPROVAL` | Tolak | `REJECTED` | Penyetuju sesuai tier | Alasan wajib | `400` |
| `APPROVED` | Barang mulai diterima sebagian | `PARTIALLY_RECEIVED` | Sistem | Ada `FinGoodsReceipt` dengan total < seluruh PO | — |
| `APPROVED` atau `PARTIALLY_RECEIVED` | Seluruh barang diterima | `FULLY_RECEIVED` | Sistem | Jumlah `ReceivedQuantity` = jumlah `Quantity` seluruh baris | — |
| `FULLY_RECEIVED` | Tutup PO | `CLOSED` | Petugas AP | Seluruh Tukar Faktur turunannya sudah `LINKED_TO_INVOICE` atau `CANCELLED` | — |
| `DRAFT` atau `PENDING_APPROVAL` | Batalkan | `CANCELLED` | Petugas AP/Penyetuju | Belum ada GR sama sekali | `422` bila sudah ada GR |
| `PARTIALLY_RECEIVED` atau `FULLY_RECEIVED` | Batalkan | — | — | **Tidak sah.** Barang sudah diterima fisik | `422` |
| `CLOSED`, `REJECTED`, `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

## B.2 Tanda Terima Barang — `FinGoodsReceipt`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Catat penerimaan terhadap PO `APPROVED`/`PARTIALLY_RECEIVED` | `RECEIVED` | Petugas gudang/AP | PO belum `CLOSED`/`CANCELLED` | `422` |
| `RECEIVED` | Batalkan | `CANCELLED` | Petugas AP | Belum menjadi Tukar Faktur | `422` bila sudah dirujuk `FinInvoiceExchange` |
| `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

## B.3 Tukar Faktur — `FinInvoiceExchange`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Catat Tukar Faktur | `RECEIVED` | Petugas AP | PO/GR boleh kosong (`FIN-DEC-051`); `EstimatedDueDate` dihitung sistem | — |
| `RECEIVED` | Terhubung ke Purchasing Invoice | `LINKED_TO_INVOICE` | Sistem | `FinPurchasingInvoice` berhasil dibuat | Unique constraint `InvoiceExchangeId` mencegah dua invoice untuk satu Tukar Faktur |
| `RECEIVED` | Batalkan | `CANCELLED` | Petugas AP | Belum terhubung invoice | `422` bila sudah `LINKED_TO_INVOICE` |
| `LINKED_TO_INVOICE` | Apa pun | — | — | **Status akhir.** Koreksi lewat Purchasing Invoice, bukan Tukar Faktur | `422` |
| `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

## B.4 Purchasing Invoice — `FinPurchasingInvoice`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat dari Tukar Faktur `RECEIVED` | `DRAFT` | Petugas AP | Tukar Faktur belum terpakai | `409` |
| `DRAFT` | Ubah rincian (PPN, diskon, DP, potongan) | `DRAFT` | Petugas AP | — | — |
| `DRAFT` | Ajukan | `PENDING_APPROVAL` | Petugas AP | `ApprovalTier` dihitung dari `TotalAmount`, ambang sama dengan PO (`FIN-DEC-052`) | — |
| `PENDING_APPROVAL` | Setujui | `APPROVED` | Supervisor/Manajer Finance sesuai `ApprovalTier` | **Penyetuju MUST NOT sama dengan pengaju** | `422` |
| `PENDING_APPROVAL` | Tolak | `REJECTED` | Penyetuju sesuai tier | Alasan wajib | `400` |
| `APPROVED` | — (efek samping) | — | Sistem | Membuat `FinSupplierPayable` (`FIN-DEC-045`); Tukar Faktur sumber → `LINKED_TO_INVOICE`; baris outbox `PPN-MASUKAN-PEMBELIAN` ditulis `PENDING` (pengiriman tertahan, `FIN-OQ-020`) | Seluruhnya satu transaksi — bila gagal, status tetap `PENDING_APPROVAL` |
| `DRAFT` atau `PENDING_APPROVAL` | Batalkan | `CANCELLED` | Petugas AP/Penyetuju | — | — |
| `APPROVED` | Apa pun | — | — | **Status akhir.** Koreksi lewat Retur Pembelian, bukan mengubah invoice | `422` |
| `REJECTED`, `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

## B.5 Retur Pembelian — `FinSupplierReturn`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Ajukan retur atas invoice `APPROVED` | `DRAFT` | Petugas AP | Minimal satu baris item, alasan wajib | `400` |
| `DRAFT` | Konfirmasi | `CONFIRMED` | Petugas AP | — | — |
| `CONFIRMED` | — (efek samping) | — | Sistem | Menerbitkan `FinSupplierReturnDeposit` sebesar `TotalAmount` (`FIN-DEC-047`) | Satu transaksi dengan perubahan status |
| `DRAFT` | Batalkan | `CANCELLED` | Petugas AP | — | — |
| `CONFIRMED` | Apa pun | — | — | **Status akhir** | `422` |
| `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

## B.6 Deposit Retur — `FinSupplierReturnDeposit`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Diterbitkan otomatis saat retur `CONFIRMED` | `AVAILABLE` | Sistem | `AvailableAmount = OriginalAmount` | — |
| `AVAILABLE` | Pakai sebagian ke Purchasing Invoice lain | `AVAILABLE` | Petugas AP | `UsedAmount` ≤ `AvailableAmount` sisa | `422` bila melebihi sisa |
| `AVAILABLE` | Pakai sampai habis | `EXHAUSTED` | Petugas AP | `AvailableAmount` menjadi nol | — |
| `AVAILABLE` | Batalkan (retur sumbernya keliru) | `CANCELLED` | Penyetuju Finance | Belum pernah dipakai sama sekali | `422` bila sudah ada `FinSupplierReturnDepositUsage` |
| `EXHAUSTED`, `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

Tidak ada transisi "pembalikan pemakaian" — bila pemakaian keliru, jalurnya adalah membatalkan
Purchasing Invoice tujuan (bila masih memungkinkan) dan mencatat pemakaian baru, mengikuti pola
"tidak pernah menghapus, selalu menambah baris baru" yang berlaku di seluruh blueprint ini.

## B.7 Batch Tagihan AR — `FinReceivableInvoiceBatch`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat batch dari `FinReceivable` terpilih | `DRAFT` | Petugas AR | Setiap `FinReceivable` terpilih belum tergabung batch aktif lain | `409` |
| `DRAFT` | Ubah anggota (tambah/kurangi) | `DRAFT` | Petugas AR | — | — |
| `DRAFT` | Terbitkan | `ISSUED` | Petugas AR | Minimal satu anggota | `422` bila kosong |
| `ISSUED` | Sebagian anggota lunas | `PARTIALLY_PAID` | Sistem | Sebagian `FinReceivable` anggota `SETTLED`, sebagian belum | — |
| `ISSUED` atau `PARTIALLY_PAID` | Seluruh anggota lunas | `PAID` | Sistem | Seluruh `FinReceivable` anggota `SETTLED` | — |
| `DRAFT` | Batalkan | `CANCELLED` | Petugas AR | — | Anggotanya bebas digabung batch lain |
| `ISSUED` | Apa pun selain pelunasan alami | — | — | **Tidak sah.** Anggota yang sudah `ISSUED` terkunci — kekeliruan dibetulkan lewat koreksi `FinReceivable` yang mendasarinya, bukan mengubah batch | `422` |
| `PAID`, `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

**Status `FinReceivableInvoiceBatch` tidak pernah mengubah status `FinReceivable` anggotanya
secara langsung.** Pelunasan tetap tercatat lewat alokasi penerimaan pada `FinReceivable`
masing-masing (bagian 2 di atas) — status batch murni **mengikuti/meringkas** status anggotanya,
bukan sumber kebenaran baru untuk pelunasan.

## B.8 Potongan penerimaan — `FinReceiptDeduction`

Sama seperti `FinPaymentDeduction` (bagian A.2), `FinReceiptDeduction` tidak punya status
sendiri — hidup mengikuti penerimaan induknya:

| Status `FinReceipt` | Potongan boleh ditambah | Potongan boleh dihapus | Alasan |
|---|:---:|:---:|---|
| `RECEIVED` (belum `ALLOCATED` penuh) | Ya | Ya | Masih dalam proses alokasi |
| `ALLOCATED` | Tidak | Tidak | Alokasi (termasuk efek potongan ke `OutstandingAmount`) sudah final |
| `RECONCILED` | Tidak | Tidak | Sudah dicocokkan rekening koran |
| `REVERSED` | Tidak | Tidak | Penerimaannya sendiri sudah dibalik |

Bila potongan keliru setelah penerimaan `ALLOCATED`, jalurnya adalah membalik penerimaan
(bagian 4) lalu mencatat ulang — **bukan** menghapus baris `FinReceiptDeduction` yang sudah ikut
mengurangi `OutstandingAmount` piutang.

---

# AMENDMENT REVISI 5

| Field | Nilai |
|---|---|
| Contract version | `FIN-STATE-1.3` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-057`, `061`, `062`; `FIN-DES-045`..`049` |
| Menggantikan | B.6 (Deposit Retur) sebagian — pemakaian kini lewat pembayaran, bukan langsung ke Purchasing Invoice. B.8 sebagian — potongan melekat pada alokasi |

## C.1 Baris pemakaian deposit — `FinSupplierReturnDepositUsage`

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Tambah deposit ke pembayaran | `RESERVED` | Petugas AP | Pembayaran `DRAFT`, `PaymentType = SUPPLIER`, deposit milik supplier yang sama, `UsedAmount` ≤ `AvailableAmount` | `422` |
| `RESERVED` | Lepas oleh petugas | `RELEASED` | Petugas AP | Pembayaran masih `DRAFT` | `422` bila sudah diajukan |
| `RESERVED` | Pembayaran `REJECTED` atau `CANCELLED` | `RELEASED` | Sistem | Dalam transaksi yang sama dengan perubahan status pembayaran | Transaksi dibatalkan seluruhnya |
| `RESERVED` | Pembayaran `PAID` | `APPLIED` | Sistem | Dalam transaksi yang sama dengan `MarkPaid` | Idem |
| `APPLIED` | Apa pun | — | — | **Status akhir.** Pembayaran `PAID` tidak dapat diubah | `422` |
| `RELEASED` | Apa pun | — | — | **Status akhir.** Pakai lagi = baris baru | `422` |

**Mengapa dicadangkan sejak `DRAFT`** (`FIN-DES-046`): bila saldo baru dikurangi saat `PAID`, dua
pembayaran `DRAFT` dapat memakai deposit yang sama dan baru bertabrakan setelah keduanya
disetujui penyetuju — kesalahan yang terlambat ketahuan.

## C.2 Deposit Retur — `FinSupplierReturnDeposit` (menggantikan tiga baris B.6)

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| `AVAILABLE` | Pencadangan menghabiskan saldo | `EXHAUSTED` | Sistem | `AvailableAmount` menjadi 0 | — |
| `EXHAUSTED` | Baris `RESERVED` dilepas | `AVAILABLE` | Sistem | `AvailableAmount` kembali > 0 | — |
| `AVAILABLE` | Batalkan | `CANCELLED` | Penyetuju Finance | **Tidak ada** baris `RESERVED` maupun `APPLIED` | `422` |

Transisi `EXHAUSTED → AVAILABLE` **baru** pada revisi ini: sebelumnya `EXHAUSTED` status akhir,
tetapi pencadangan yang dilepas harus bisa mengembalikan saldo. Deposit yang habis karena baris
`APPLIED` tetap `EXHAUSTED` selamanya, karena baris `APPLIED` tidak pernah dilepas.

## C.3 Pembayaran keluar — tambahan pada bagian 6

| Dari status | Tindakan | Ke status | Syarat tambahan | Bila dilanggar |
|---|---|---|---|---|
| `DRAFT` | Ajukan | `SUBMITTED` | `NetTransferAmount = 0` **diizinkan** bila `DepositAppliedAmount > 0` — pengecualian `FIN-VAL-091` | — |
| `APPROVED` | Tandai sudah dibayar | `PAID` | Nomor bukti transfer wajib **hanya** bila `NetTransferAmount > 0` | `400` |
| `SUBMITTED`/`APPROVED` | — | — | Baris deposit **tidak** dapat ditambah atau dilepas | `422` |

## C.4 Potongan penerimaan — menggantikan B.8

`FinReceiptDeduction` tidak punya status; hidupnya mengikuti **baris alokasinya**:

| Kejadian pada alokasi | Efek pada potongannya |
|---|---|
| Alokasi dibuat bersama potongan | Potongan tercatat; piutang berkurang sebesar uang + potongan; `POTONGAN-PIUTANG-NON-TUNAI` terbit per potongan |
| Alokasi dibalik (manual atau otomatis karena tender dibatalkan, `FIN-DEC-021`) | Setiap potongan mendapat baris pembalik; piutang terbuka kembali; `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` terbit per baris pembalik |
| Potongan keliru, alokasi masih benar | **Tidak ada transisi sendiri.** Balik alokasinya lalu catat ulang |
| Potongan yang sudah dibalik dibalik lagi | **Tidak sah** — ditolak unique index `ReversalOfDeductionId` |

## D.1 Sumbu klaim penjamin pada Batch Tagihan AR — `FinReceivableInvoiceBatch.ClaimStatus`

`last_changed_in`: `FIN-STATE-1.4` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-095`, `FIN-DEC-097`, `FIN-DEC-098`; dirancang `FIN-DES-070`.

Bagian ini **menambah sumbu kedua**, dan **tidak mengubah satu baris pun** pada `B.7`. Keduanya
hidup bersamaan pada entity yang sama:

| Sumbu | Kolom | Menjawab | Penulis |
|---|---|---|---|
| Dokumen dan pelunasan (`B.7`, tidak berubah) | `Status` | Apakah tagihan sudah diterbitkan, dan apakah uangnya sudah masuk | Petugas AR (terbit/batal) dan **Sistem** (pelunasan) |
| Jawaban penjamin (**baru**) | `ClaimStatus` | Apa kata penjamin atas tagihan itu | **Hanya** petugas AR, manual — kecuali `SUBMITTED` |

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| kosong | Batch diterbitkan | `SUBMITTED` | **Sistem**, menumpang `POST /{id}/issue` yang sudah ada | `Status` berpindah ke `ISSUED` | — |
| `SUBMITTED` | Tandai berkas diterima penjamin | `PAYER_VERIFIED` | Petugas AR | — | `422` |
| `SUBMITTED` | Catat nominal disetujui, melewati verifikasi | `APPROVED` | Petugas AR | Diizinkan — sebagian penjamin langsung menerbitkan berita acara persetujuan tanpa tahap konfirmasi berkas terpisah | — |
| `PAYER_VERIFIED` | Catat nominal disetujui | `APPROVED` | Petugas AR | `ApprovedAmount` wajib diisi, antara nol dan `TotalAmount` | `422` |
| `APPROVED` | Tutup klaim | `CLOSED` | Petugas AR | — | `422` |
| `APPROVED` | Catat ulang nominal disetujui | `APPROVED` | Petugas AR | Penjamin merevisi keputusannya; nominal lama tertimpa, perubahannya terbaca pada kolom audit | — |
| `CLOSED` | Apa pun | — | — | **Status akhir** | `422` |
| kosong | Tindakan klaim apa pun | — | — | **Tidak sah.** Klaim belum ada selama tagihannya belum dikirim ke penjamin | `422` |
| Apa pun | Mundur ke status sebelumnya | — | — | **Tidak sah.** Pembetulan dilakukan dengan mencatat ulang pada status yang sama, bukan memundurkan | `422` |

### Yang **tidak** terjadi pada sumbu ini

| Yang mungkin disangka | Kenyataannya |
|---|---|
| `ClaimStatus = APPROVED` membuat batch menjadi `PAID` | **Tidak.** Pelunasan tetap hanya dari alokasi penerimaan pada piutang anggota (`B.7`) |
| `ApprovedAmount` lebih kecil dari `TotalAmount` mengurangi `OutstandingAmount` | **Tidak.** Selisihnya menunggu write-off manual petugas (`FIN-DES-071`) |
| `ClaimStatus = CLOSED` mengunci batch dari pelunasan | **Tidak.** Uang yang masuk belakangan tetap dialokasikan seperti biasa |
| Batch `CANCELLED` ikut membatalkan klaimnya | **Tidak otomatis.** Pembatalan batch hanya sah saat `DRAFT` (`B.7`), dan pada keadaan itu `ClaimStatus` masih kosong |

## E.1 Piutang sewa non-pasien — `FinNonPatientReceivable`

`last_changed_in`: `FIN-STATE-1.5` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-100`..`FIN-DEC-103`; dirancang `FIN-DES-074`..`FIN-DES-076`.

Berbeda dari `FinReceivable`, kapabilitas ini **tidak** punya jenjang persetujuan apa pun
(`FIN-DEC-103`) — setiap perpindahan selesai dalam satu aksi oleh staf AR.

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Catat tagihan sewa baru | `OUTSTANDING` | Petugas AR | Nominal tagihan lebih besar dari nol | `400` |
| `OUTSTANDING` | Koreksi isi tagihan | `OUTSTANDING` | Petugas AR | **Belum pernah** menerima pembayaran sama sekali | `422` bila sudah ada pelunasan |
| `OUTSTANDING` | Catat pelunasan sebagian | `PARTIALLY_SETTLED` | Petugas AR | Jumlah pelunasan masih kurang dari total tagihan | — |
| `OUTSTANDING` atau `PARTIALLY_SETTLED` | Catat pelunasan sampai lunas | `SETTLED` | Petugas AR | Jumlah pelunasan mencapai total tagihan | — |
| `PARTIALLY_SETTLED` | Catat pelunasan bernilai negatif (membatalkan pelunasan sebelumnya) | `PARTIALLY_SETTLED` atau `OUTSTANDING` | Petugas AR | Jumlah pelunasan sesudahnya tidak boleh kurang dari nol | `422` |
| `SETTLED` | Catat pelunasan bernilai negatif | `PARTIALLY_SETTLED` | Petugas AR | Pembetulan pelunasan yang keliru | — |
| `OUTSTANDING` atau `PARTIALLY_SETTLED` | Hapus piutang (tidak tertagih) | `WRITTEN_OFF` | Petugas AR, **tanpa penyetuju** | Alasan wajib diisi | `400` bila alasan kosong |
| `OUTSTANDING` | Batalkan (salah catat) | `CANCELLED` | Petugas AR | **Belum pernah** menerima pembayaran, dan alasan wajib diisi | `422` bila sudah ada pelunasan |
| `PARTIALLY_SETTLED` atau `SETTLED` | Batalkan | — | — | **Tidak sah.** Tagihan yang sudah menerima uang dibetulkan lewat pelunasan negatif, bukan dibatalkan | `422` |
| `SETTLED` | Hapus piutang | — | — | **Tidak sah.** Tidak ada sisa yang bisa dihapus | `422` |
| `WRITTEN_OFF`, `CANCELLED` | Apa pun | — | — | **Status akhir** | `422` |

### Perbedaan yang disengaja dari piutang pasien

| Hal | `FinReceivable` (piutang pasien) | `FinNonPatientReceivable` (sewa) |
|---|---|---|
| Penghapusan piutang | Pengajuan lalu persetujuan (maker-checker, `BE-FIN-018`) | **Satu aksi langsung**, tanpa penyetuju (`FIN-DEC-103`) |
| Asal baris | Wajib dari serah terima Billing | Dicatat manual petugas (`FIN-DEC-100`) |
| Pelunasan | Lewat alokasi `FinReceipt` | Baris pelunasan sendiri (`FIN-DES-075`) |

Perbedaan pertama **MUST NOT** menular: kelonggaran di sini berlaku **hanya** untuk piutang sewa,
dan tidak pernah menjadi alasan melonggarkan maker-checker piutang pasien.

---

# Bagian F — Revisi 14: saldo awal, batch migrasi, dan siklus penanda shift

| Field | Nilai |
|---|---|
| Contract version | `FIN-STATE-1.6` — status **`draft`** |
| Naik dari | `FIN-STATE-1.5` (`approved` 1 Oktober 2026) |
| Traceability | `FIN-DEC-121`, `128`, `129`, `136`; `FIN-DES-084`, `088`, `089` |
| Dampak kompatibilitas | Aditif — nol status lama berubah |

## F.1 `FinOpeningBalance` — saldo awal cutover

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| *(belum ada)* | Mencatat saldo awal | `DRAFT` | `FinanceSubledgerSetup : Create` | Kelompok belum punya baris aktif; `Reason` dan rujukan dokumen Accounting terisi; kelompok item migrasi bernilai nol | `409` bila sudah ada; `422` bila nilainya melanggar |
| `DRAFT` | Mengoreksi | `DRAFT` | `FinanceSubledgerSetup : Update` | — | — |
| `DRAFT` | Menyetujui | `APPROVED` | `FinanceSubledgerSetup : Approve` | Seluruh ruas wajib terisi | `422` |
| `APPROVED` | Mengunci | `LOCKED` | `FinanceSubledgerSetup : Approve` | `CutoverDate` tidak melebihi hari ini | `422` |
| `LOCKED` | — | — | **Tidak seorang pun** | Nilai **MUST NOT** berubah | `409` |

Transisi yang **tidak sah** dan MUST ditolak:

| Yang dicoba | Kenapa ditolak |
|---|---|
| `APPROVED` → `DRAFT` | Persetujuan tidak dapat ditarik; koreksi menuntut keputusan baru |
| `LOCKED` → status apa pun | Posisi seluruh buku dihitung dari titik ini |
| Mengubah `Amount` saat `APPROVED` | Nilai yang disetujui adalah nilai yang disetujui |
| Menghapus baris `LOCKED` | Penghapusan penandaan sekalipun akan membuat posisi tidak dapat dihitung |

**Akibat `LOCKED` yang MUST dipahami.** Mengunci kelompok `KAS-KASIR` menerbitkan satu mutasi kas
`SALDO-AWAL` bertanggal `CutoverDate`. Mutasi itu juga tidak dapat dibatalkan.

## F.2 `FinOpeningItemBatch` — batch migrasi tagihan lama

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| *(belum ada)* | Mengunggah spreadsheet | `DRAFT` | `FinanceOpeningItemBatch : Create` | Berkas terbaca; kolom templat lengkap; `ItemKind` ditetapkan | `400` |
| `DRAFT` | Mengunggah ulang berkas | `DRAFT` | `FinanceOpeningItemBatch : Update` | Hasil validasi sebelumnya dibuang | — |
| `DRAFT` | Menjalankan validasi | `VALIDATED` | `FinanceOpeningItemBatch : Update` | **Nol** baris bergalat | Tetap `DRAFT` beserta daftar galat per baris |
| `DRAFT` atau `VALIDATED` | Menyatakan saldo awal Accounting | status tidak berubah | `FinanceOpeningItemBatch : Update` | Nominal dan rujukan dokumen terisi | `422` |
| `VALIDATED` | Menyetujui | `APPROVED` lalu **otomatis** `LOCKED` | `FinanceOpeningItemBatch : Approve` | `TotalOutstandingAmount` **sama dengan** `DeclaredAccountingOpeningAmount`; rujukan dokumen terisi; `CutoverDate` sama dengan `FinOpeningBalance` | `422` beserta kedua angka yang dibandingkan |
| `DRAFT` atau `VALIDATED` | Menolak | `REJECTED` | `FinanceOpeningItemBatch : Update` | Alasan terisi | `422` |
| `LOCKED` | — | — | **Tidak seorang pun** | — | `409` |
| `REJECTED` | — | — | **Tidak seorang pun** | Batch baru diunggah tersendiri | `409` |

Transisi yang **tidak sah** dan MUST ditolak:

| Yang dicoba | Kenapa ditolak |
|---|---|
| `VALIDATED` → `APPROVED` dengan selisih rekonsiliasi | Inti `FIN-DEC-129` butir 1 |
| `APPROVED` tanpa melewati `VALIDATED` | Item akan lahir tanpa pemeriksaan per baris |
| `REJECTED` → `DRAFT` | Batch yang ditolak ditutup; perbaikan diunggah sebagai batch baru |
| `LOCKED` → `REJECTED` | Item sudah lahir; pembatalannya menuntut keputusan dan jalur tersendiri |
| Mengubah item piutang atau utang yang `OpeningItemBatchId`-nya menunjuk batch `LOCKED`, lewat jalur batch | Item sudah menjadi tagihan biasa; koreksinya lewat penyesuaian dan penghapusan yang sudah ada |

**Satu hal yang MUST diperhatikan pada perpindahan ke `APPROVED`.** Pada perpindahan inilah item
piutang atau utang **lahir**, beserta mutasi pembukanya, dalam **satu transaksi**. Bila satu baris
gagal, seluruh batch gagal — tidak ada batch setengah jadi. Selama `DRAFT` dan `VALIDATED`, **nol**
baris piutang atau utang ada.

**Dan satu hal yang MUST NOT terjadi:** perpindahan ini **tidak** menerbitkan kejadian akuntansi apa
pun (`FIN-DEC-129` butir 3). Nilainya sudah tercakup saldo awal Accounting; menerbitkannya akan
menghitung ganda.

## F.3 Siklus penanda shift kasir — diperluas

Bukan status baru pada entity Finance; ini **siklus penanda** yang diterbitkan Finance atas shift
milik Billing. `FIN-DEC-121` memperluas pemicunya.

| Status shift (milik Billing) | Masuk kelompok | Penanda yang diterbitkan Finance |
|---|---|---|
| `OPEN` | **Belum final** | `PEMBUKAAN-SHIFT-KASIR` |
| `HANDED_OVER` | **Belum final** | `PEMBUKAAN-SHIFT-KASIR` |
| `CLOSED_WITH_VARIANCE` | **Belum final** | `PEMBUKAAN-SHIFT-KASIR` |
| `PERLU_TINDAK_LANJUT` | **Belum final** | `PEMBUKAAN-SHIFT-KASIR` |
| `REOPENED` | **Belum final** | `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bila siklusnya pernah tertutup, lalu `PEMBUKAAN-SHIFT-KASIR` siklus berikutnya |
| `CLOSED` | **Final** | `PENUTUPAN-SHIFT-KASIR` + satu mutasi kas `KAS-SHIFT` |
| `REVIEWED` | **Final** | `PENUTUPAN-SHIFT-KASIR` + satu mutasi kas `KAS-SHIFT` |

| Aturan siklus | Isi |
|---|---|
| Nomor siklus | Jumlah penanda pembalik yang sudah terbit untuk shift itu ditambah satu |
| Idempotensi penanda | Satu shift pada satu siklus menghasilkan paling banyak satu penanda per jenis — dijaga unique index outbox |
| Idempotensi mutasi kas | Dijaga unique index (`SourceReferenceType`, `SourceReferenceId`, `MovementType`) pada `FinCashMovement` |
| Yang **tidak** terjadi | Finance **MUST NOT** menulis apa pun ke tabel `Bil*`. Status shift tetap sepenuhnya milik Billing |

**Urutan yang MUST dijaga saat shift dibuka kembali lalu ditutup lagi:** pembalik penutupan siklus
lama terbit lebih dulu, baru penanda pembukaan siklus baru. Terbalik, Accounting akan melihat dua
shift terbuka untuk satu shift yang sama.

## F.4 Kedudukan `FinDailyCashSnapshot` — status tidak berubah, artinya berubah

Status `OPEN` dan `CLOSED` **tidak berubah**, dan nol transisi baru ditambahkan. Yang berubah
kedudukannya:

| Hal | Sebelum | Sesudah |
|---|---|---|
| Penutupan hari | Direncanakan menjadi syarat snapshot | **Bukan** syarat. Boleh ditutup walau masih ada shift belum final (`FIN-DEC-124`) |
| `CLOSED` yang tidak dapat diubah | Tetap tidak dapat diubah | **Tetap**, dan kini **tidak menjadi masalah** karena posisi kas tidak lagi dihitung darinya |
| Selisih terhadap posisi kas | Tidak ada konsepnya | **Sah dan diharapkan**; ditampilkan lewat permukaan baca selisih (`FIN-DEC-125`) |

Karena itu **nol** transisi baru, **nol** jalur pembukaan kembali, dan **nol** perubahan skema untuk
tabel ini. Pembukaan kembali rekap harian ditolak sebagai desain — lihat `FIN-DES-081`.
