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
