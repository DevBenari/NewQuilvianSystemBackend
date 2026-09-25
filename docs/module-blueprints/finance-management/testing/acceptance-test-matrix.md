# Matriks Uji Penerimaan — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-TEST-1.1` |
| `last_changed_in` | `FIN-TEST-1.1` — amendment 25 September 2026 (bagian 8 dan 8a baru) |
| Status | `approved` dan `locked` — revisi 1.1 disetujui dan dikunci owner 25 September 2026 |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-25 |
| Input revision | `contracts/validation-matrix.md` `FIN-VAL-1.1`, `contracts/state-transition-matrix.md` `FIN-STATE-1.1`, `02-backend-architecture.md` AMENDMENT REVISI 3 |
| Catatan project test | Project test terpisah belum terdeteksi di repository pada `09101d05`. Kolom "Jenis test" menyatakan **jenis yang seharusnya**, bukan yang sudah tersedia |

Matriks ini memuat jalur gagal, bukan hanya jalur berhasil. Uji yang hanya membuktikan jalur
berhasil tidak membuktikan apa pun tentang uang.

---

## 1. Fakta masuk dari Billing

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-001` | Fakta AR yang sama dikirim dua kali | Integrasi | Hanya satu `FinBillingHandoffIntake` dan satu `FinReceivable`; permintaan kedua dibalas `409` |
| `FIN-VAL-001` | Dua permintaan pengolahan berjalan hampir bersamaan untuk fakta yang sama | Integrasi, konkuren | Tepat satu piutang tersimpan; satunya gagal karena unique index, bukan karena pemeriksaan di kode |
| `FIN-VAL-002` | Fakta berstatus `CONSUMED` diminta diulang | Integrasi | `422`; tidak ada piutang kedua |
| `FIN-STATE` | Pengolahan gagal di tengah jalan | Integrasi | Status `ERROR`, `ErrorMessage` terisi, **tidak ada** piutang setengah jadi |
| `FIN-STATE` | Fakta `ERROR` diulang dan kali ini berhasil | Integrasi | Status `CONSUMED`, `RetryCount` bertambah satu |

## 2. Piutang

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-011` | Piutang Rp 10.000.000 dilunasi Rp 4.000.000 | Integrasi | Sisa Rp 6.000.000, terbayar Rp 4.000.000, status `PARTIAL`, jumlahnya tetap Rp 10.000.000 |
| `FIN-VAL-010` | Alokasi Rp 12.000.000 ke piutang Rp 10.000.000 | Integrasi | `422`; sisa piutang tidak berubah sama sekali |
| `FIN-VAL-012` | Fakta AR dengan `SourceHandoffKey` yang sudah dipakai | Integrasi | `409`; jumlah baris piutang tetap |
| `FIN-VAL-013` | Fakta AR penjamin datang tanpa satu pun berkas klaim | Integrasi | Piutang **tetap dibuat** berstatus `OUTSTANDING`, `ClaimStatus` = `INCOMPLETE` |
| `FIN-VAL-014` | Penghapusan diajukan atas piutang berstatus `SETTLED` | Integrasi | `422` |
| `FIN-VAL-016` | Fakta AR `EMPLOYEE_BENEFIT` tanpa `BenefitOwnerId` | Integrasi | `422`; piutang tidak dibuat |
| `FIN-STATE` | Aging dihitung untuk piutang jatuh tempo 95 hari lalu | Unit | Masuk kelompok "di atas 90 hari", bukan "61-90" |
| `FIN-STATE` | Aging untuk piutang jatuh tempo tepat 30 hari lalu | Unit | Masuk kelompok "0-30", bukan "31-60" |

## 3. Maker-checker

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-020` | Pengaju koreksi menyetujui koreksinya sendiri | Integrasi | `422` dengan pesan "Pengaju tidak boleh menyetujui permohonannya sendiri" |
| `FIN-VAL-020` | Koreksi disimpan langsung ke database dengan `ApprovedBy` = `RequestedBy` | Integrasi database | Ditolak check constraint, bukan hanya oleh service |
| `FIN-VAL-020` | Pengguna lain yang berwenang menyetujui | Integrasi | Status `APPROVED`, sisa piutang berubah sesuai arah koreksi |
| `FIN-VAL-024` | Koreksi pengurang Rp 5.000.000 atas sisa piutang Rp 3.000.000 | Integrasi | `422`; sisa piutang tetap Rp 3.000.000 |
| `FIN-VAL-025` | Koreksi yang sudah `APPROVED` disetujui lagi | Integrasi | `422`; nilai piutang tidak berubah dua kali |
| `FIN-STATE` | Koreksi masih `REQUESTED` | Integrasi | Sisa piutang **belum** berubah sama sekali |

## 4. Penerimaan dan pembagian bayar-vs-piutang

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-030` | Tender berhasil dikirim dua kali | Integrasi | Satu `FinReceipt`; yang kedua `409` |
| `FIN-VAL-030` | Dua permintaan bersamaan untuk tender yang sama | Integrasi, konkuren | Tepat satu penerimaan; dijaga partial unique index |
| `FIN-DES-011` | Pasien membayar lunas Rp 500.000 di kasir | Integrasi | Satu `FinReceipt` Rp 500.000; **nol** `FinReceivable` untuk jumlah itu |
| `FIN-DES-011` | Pasien membayar Rp 300.000 dari tagihan Rp 500.000, sisanya jadi piutang penjamin | Integrasi | Satu penerimaan Rp 300.000 **dan** satu piutang Rp 200.000. Totalnya Rp 500.000, tanpa dobel-hitung |
| `FIN-VAL-032` | Alokasi Rp 2.500.000 dari penerimaan yang sisa uangnya Rp 2.000.000 | Integrasi | `422` dengan pesan yang menyebut sisa Rp 2.000.000 |
| `FIN-VAL-036` | Penerimaan tunai tanpa `CashierShiftId` | Integrasi | `422` |
| `FIN-VAL-031` | Nominal penerimaan berbeda dari `BilTender.Amount` | Integrasi | `422`; Finance tidak boleh menyimpan nilai yang berbeda |

## 5. Pembalikan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-021` | Penerimaan yang sudah dialokasikan ke piutang dibalik | Integrasi | Baris alokasi pembalik dibuat; piutang **otomatis** kembali `OUTSTANDING`; baris lama tetap ada |
| `FIN-DES-012` | Penerimaan dibalik | Integrasi | Jumlah baris `FinReceipt` **bertambah**, tidak berkurang; baris asli tetap terbaca |
| `FIN-VAL-035` | Alokasi diminta atas penerimaan berstatus `REVERSED` | Integrasi | `422` |
| `FIN-STATE` | Penerimaan `RECONCILED` dibalik | Integrasi | Diizinkan dengan alasan tertulis; menghasilkan baris pembalik |
| `FIN-DES-012` | Riwayat ditelusuri setelah pembalikan | Integrasi | Penerimaan asli, alokasi asli, dan keduanya versi pembalik semuanya terbaca berurutan |

## 6. Utang dan pembayaran rekap

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-040` | Faktur supplier dengan nomor yang sama diinput dua kali | Integrasi | `409`; hanya satu utang |
| `FIN-VAL-041` | Rincian Rp 9.000.000 untuk faktur Rp 10.000.000 | Integrasi | `422` dengan pesan yang menyebut kedua angka |
| `FIN-VAL-042` | Fee dokter yang sama dicatat dua kali | Integrasi | `409`; satu utang dokter |
| `FIN-VAL-043` | Fee dokter yang belum disetujui dicatat sebagai utang | Integrasi | `422` |
| `FIN-VAL-050` | Pembayaran rekap Rp 24.500.000 hanya mengalokasikan Rp 21.500.000 | Integrasi | `422` saat diajukan; pesan menyebut kedua angka |
| `FIN-DEC-019` | Pembayaran rekap melunasi tiga utang dokter sekaligus | Integrasi | Satu `FinPayment`, tiga `FinPaymentAllocation`, ketiga utang menjadi `PAID` |
| `FIN-DEC-019` | Telusur balik dari satu pembayaran rekap | Integrasi | Ketiga fee penyusunnya terbaca beserta nominalnya masing-masing |
| `FIN-VAL-057` | Satu utang dipilih dua kali dalam satu pembayaran | Integrasi | `400`; pembayaran tidak tersimpan |
| `FIN-VAL-054` | Baris alokasi menunjuk utang supplier **dan** utang dokter sekaligus | Integrasi database | Ditolak check constraint |
| `FIN-VAL-051` | Pengaju pembayaran menyetujui pembayarannya sendiri | Integrasi | `422` |
| `FIN-STATE` | Pembayaran berstatus `APPROVED` tetapi belum `PAID` | Integrasi | Sisa utang **belum** berkurang |
| `FIN-STATE` | Pembayaran ditandai `PAID` | Integrasi | Sisa utang baru berkurang saat ini |

## 7. Kas

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-060` | Setoran Rp 5.000.000 padahal saldo tersedia Rp 4.800.000 | Integrasi | `422` dengan pesan menyebut Rp 4.800.000; tidak ada setoran tersimpan |
| `FIN-VAL-061` | Setoran Rp 3.000.000 dari saldo tersedia Rp 4.800.000 | Integrasi | Diterima; saldo tersedia menjadi Rp 1.800.000 |
| `FIN-VAL-060` | Dua petugas memposting setoran hampir bersamaan dari saldo yang sama | Integrasi, konkuren | Satu berhasil, satu ditolak; saldo **tidak pernah** menjadi negatif |
| `FIN-VAL-062` | Layar mengirim saldo tersedia yang sudah basi | Integrasi | Nilai dari layar diabaikan; backend menghitung ulang |
| `FIN-VAL-064` | Penutupan hari sementara masih ada setoran `DRAFT` | Integrasi | `422` dengan pesan menyebut jumlah setoran yang tertunda |
| `FIN-VAL-063` | Penerimaan terlambat masuk untuk tanggal yang sudah `CLOSED` | Integrasi | Angka tanggal itu **tidak berubah**; penerimaan masuk tanggal berikutnya |
| `FIN-DEC-020` | Pencairan kas kecil terjadi pada hari yang sama | Integrasi | `FinDailyCashSnapshot` **tidak berubah** sama sekali |
| `FIN-VAL-066` | Saldo awal hari ini dibandingkan saldo akhir kemarin | Integrasi | Sama persis |

## 8. Kejadian ke Accounting

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-017` | Penyimpanan piutang gagal di tengah transaksi | Integrasi | **Nol** piutang dan **nol** baris kejadian; keduanya batal bersama |
| `FIN-DES-017` | Piutang berhasil disimpan | Integrasi | Tepat satu baris kejadian `PENGAKUAN-PIUTANG` ikut tersimpan |
| ~~`FIN-DEC-004`~~ | ~~Tender berhasil saat tagihan masih `OPEN` → baris `HELD_FOR_FINALIZATION`~~ | — | **DICABUT revisi 1.1** — digantikan `FIN-DEC-030` di bawah |
| ~~`FIN-DEC-004`~~ | ~~Tagihan kemudian menjadi `FINAL` → status berubah `PENDING`~~ | — | **DICABUT revisi 1.1** |
| ~~`FIN-VAL-076`~~ | ~~Worker melewati baris tertahan~~ | — | **DICABUT revisi 1.1**; digantikan `FIN-VAL-078` untuk baris warisan saja |
| `FIN-VAL-072` | Dua kejadian dengan `EventNumber` sama | Integrasi database | Ditolak unique index |
| `FIN-VAL-073` | Kejadian dengan nomor berbeda tetapi identitas sumber sama | Integrasi database | Ditolak unique index lapis kedua |
| `FIN-VAL-074` | Koreksi dikirim dengan `SourceVersion` yang sama | Integrasi | `422`; koreksi tidak terkirim sebagai duplikat |
| `FIN-VAL-071` | Payload memuat `PatientId` | Unit | `422`; kejadian tidak tersimpan |
| `FIN-VAL-070` | Kejadian bermata uang selain `IDR` | Integrasi database | Ditolak check constraint |
| `FIN-DEC-003` | Kejadian `PENGAKUAN-PIUTANG` untuk tagihan yang memuat jasa medis | Integrasi | Payload **tidak** memuat komponen `JASA_MEDIS` |
| `FIN-DEC-003` | Fee dokter kemudian disetujui | Integrasi | Kejadian `PENGAKUAN-HUTANG-DOKTER` terbit terpisah |
| `FIN-STATE` | Accounting membalas `422` | Integrasi | Status `HELD`, **tidak** membuat kejadian baru |
| `FIN-STATE` | Kejadian `FAILED` dikirim ulang manual | Integrasi | Nomor kejadian tetap sama; tidak ada baris baru |

### 8a. Uang muka, deposit, kelebihan bayar, dan selisih kas — AMENDMENT REVISI 3

Seluruh baris di bawah **belum dapat dijalankan** sampai tujuh kode baru diratifikasi Accounting
(`FIN-OQ-017`). Ditulis sekarang supaya kriterianya sudah terkunci sebelum kodenya disentuh.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-030` | Tender berhasil saat tagihan masih `OPEN` | Integrasi | Penerimaan terlihat penuh di Finance; baris kejadian **`PENERIMAAN-UANG-MUKA` berstatus `PENDING`**, bukan tertahan |
| `FIN-DEC-030` | Tender berhasil saat tagihan sudah `FINAL` | Integrasi | Baris kejadian `PENERIMAAN-KASIR` berstatus `PENDING` |
| `FIN-DEC-030` | Tagihan kemudian `FINAL` dan uang muka dipakai melunasi piutang | Integrasi | Baris kejadian **baru** `PEMAKAIAN-UANG-MUKA-DEPOSIT`; baris `PENERIMAAN-UANG-MUKA` **tidak** diubah |
| `FIN-DES-034`, `FIN-VAL-085` | Penerimaan `PENERIMAAN-UANG-MUKA` dibalik **setelah** tagihannya menjadi `FINAL` | Integrasi | Pembalikannya `PEMBALIKAN-PENERIMAAN-UANG-MUKA` — **bukan** `PEMBALIKAN-PENERIMAAN-KASIR`, walaupun tagihan sekarang `FINAL` |
| `FIN-DES-034` | Penerimaan `PENERIMAAN-KASIR` dibalik | Integrasi | Pembalikannya `PEMBALIKAN-PENERIMAAN-KASIR`, tidak berubah dari perilaku lama |
| `FIN-DEC-040`, `FIN-DES-029` | `BilDepositMovement` `ALLOCATION` disinkronkan | Integrasi | Satu baris intake `DEPOSIT_MOVEMENT` berstatus `CONSUMED`; satu baris kejadian `PEMAKAIAN-UANG-MUKA-DEPOSIT`; **tidak ada** `FinReceipt` baru |
| `FIN-DEC-041` | `BilDepositMovement` `RELEASE` disinkronkan | Integrasi | Baris kejadian `PENGEMBALIAN-UANG-MUKA`, **bukan** kode pemakaian |
| `FIN-DEC-041` | `BilRefundCase` `EXECUTED` bersumber `ALLOCATION_EXCESS` | Integrasi | Baris kejadian `PENGEMBALIAN-UANG-MUKA` |
| `FIN-DEC-041`, `FIN-OQ-018` | `BilRefundCase` `EXECUTED` bersumber `SETTLEMENT` | Integrasi | **Nol** baris kejadian — sengaja di luar cakupan; tidak boleh diam-diam ikut terkirim |
| `FIN-DEC-042` | `BilRefundableCredit` `ALLOCATION_EXCESS` diakui | Integrasi | Baris kejadian `PENGAKUAN-KELEBIHAN-BAYAR` bernilai selisihnya saja, bukan nilai penerimaan penuh |
| `FIN-DEC-043`, `FIN-VAL-086` | Shift kasir ditutup dengan selisih, **belum** disahkan | Integrasi | **Nol** baris kejadian `SELISIH-KAS-SHIFT` |
| `FIN-DEC-043`, `FIN-DES-036` | Selisih shift disahkan (`BilCashVarianceReview` terbentuk) | Integrasi | Satu baris kejadian `SELISIH-KAS-SHIFT`; `SourceTransactionId` = `BilCashVarianceReview.Id`; `AccountingDate` = **tanggal shift**, bukan tanggal pengesahan |
| `FIN-VAL-080` | Shift ditutup tanpa selisih (`Variance` = 0) | Integrasi | **Nol** baris kejadian |
| `FIN-VAL-079`, `FIN-DES-031` | Selisih kas berupa kekurangan Rp 30.000 | Unit | Kejadian tersimpan dengan `Amount` = `-30000.00`; **tidak** ditolak validasi |
| `FIN-VAL-079` | Kejadian `PENERIMAAN-UANG-MUKA` bernilai nol | Unit | `400`; kejadian tidak tersimpan |
| `FIN-DEC-035`, `FIN-VAL-081` | Pesan `SALDO-SUBLEDGER` tanpa `SubledgerBalance` | Unit | `400`; kejadian tidak tersimpan |
| `FIN-DEC-035`, `FIN-VAL-082` | `AccountingPeriodCode` berisi `2026-11-30` | Unit | `400`; bentuk wajib `YYYY-MM` |
| `FIN-DEC-035`, `FIN-DES-032` | Pesan `SALDO-SUBLEDGER` bersaldo nol | Integrasi | Tersimpan dan terkirim; `Amount` = `0.00` tidak ditolak |
| `FIN-VAL-083` | Pernyataan ulang saldo periode yang sama dengan versi lebih rendah | Integrasi | `422`; koreksi tidak menimpa versi yang lebih tinggi |
| `FIN-VAL-084` | `SubledgerBalance` terisi pada kejadian `PENERIMAAN-KASIR` | Unit | `400`; rincian saldo hanya untuk pesan saldo |
| `FIN-DES-029` | Satu `BilDepositMovement` disinkronkan dua kali | Integrasi database | Baris intake kedua ditolak unique index `(HandoffType, SourceHandoffKey)`; **nol** kejadian kedua |
| `FIN-DES-030` | Satu `BilCashVarianceReview` disinkronkan dua kali | Integrasi database | Ditolak unique index lapis kedua kotak keluar; tidak ada jurnal ganda |
| `FIN-DEC-039` | Accounting membalas `201` dengan `JournalNumber` kosong | Integrasi | Status **`ACKNOWLEDGED`**; `AccountingJournalNumber` kosong; **tidak** ditandai `FAILED` dan **tidak** dikirim ulang otomatis |
| `FIN-VAL-078` | Worker berjalan sementara ada baris warisan `HELD_FOR_FINALIZATION` | Integrasi | Baris dilewati **dan dilaporkan**, tidak diam-diam diabaikan |
| Aturan bisnis #9 | Sinkronisasi deposit/refund/selisih kas dijalankan | Integrasi database | **Nol** perubahan pada tabel `Bil*` mana pun — dibuktikan dengan membandingkan `RowVersion`/`UpdateDateTime` sebelum dan sesudah |

## 9. Ketahanan terhadap pengulangan dan perebutan data

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-085` | Petugas menekan Simpan dua kali dengan `Idempotency-Key` sama | Integrasi | Satu catatan tersimpan; permintaan kedua mengembalikan hasil yang sama, bukan galat |
| `FIN-VAL-084` | Perintah uang dikirim tanpa `Idempotency-Key` | Integrasi | `400` |
| `FIN-VAL-083` | Dua petugas mengubah piutang yang sama; yang kedua memakai versi lama | Integrasi | `409` dengan pesan "Data telah berubah. Muat ulang sebelum melanjutkan." |
| `FIN-VAL-082` | Perintah pengubah tanpa `ExpectedRowVersion` | Integrasi | `400` |
| `FIN-VAL-086` | Data Finance dihapus lewat API | Integrasi | Baris tetap ada dengan `IsDelete = true`; tidak hilang dari tabel |

## 10. Hak akses

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-PERM` | Auditor membuka daftar piutang | Integrasi | `200` |
| `FIN-PERM` | Auditor mencoba menyetujui penghapusan piutang | Integrasi | `403` |
| `FIN-PERM` | Staf AR mencoba menyetujui koreksinya sendiri | Integrasi | `403` bila tidak punya butir `ApproveAdjustment`; `422` bila punya tetapi dialah pengajunya |
| `FIN-VAL-052` | Penyetuju dengan jenjang di bawah nominal pembayaran | Integrasi | `403` |
| `FIN-PERM` | Staf Accounting mencoba mengubah piutang Finance | Integrasi | `403` |
| `FIN-PERM` | Catatan log diperiksa setelah persetujuan penghapusan | Integrasi | Log memuat `EntityId`, controller, action, pelaku — **tanpa** `PatientId` maupun `BenefitOwnerId` |

## 11. Yang sengaja belum dapat diuji

| Hal | Alasan | Kapan dapat diuji |
|---|---|---|
| Pengiriman nyata ke Accounting | Endpoint penerima belum dibangun (`FIN-CAP-018`) | Setelah Accounting membangunnya |
| Penanganan balasan `200`/`201`/`400`/`403`/`409` | Sama seperti di atas | Sama |
| Konsumsi `BilCollectionHandoff` | Tabelnya milik Billing dan belum ada | Setelah Billing membangunnya |
| Rumpun manfaat karyawan | `OPEN DECISION` sampai konfirmasi Billing + HR | Setelah konfirmasi turun |
| Ambang nominal jenjang persetujuan AP | `FIN-OQ-010` belum dijawab | Setelah angka ambangnya ditetapkan |

Lima hal di atas **MUST NOT** ditandai lulus dengan uji tiruan yang seolah-olah membuktikannya.
Menguji dengan endpoint palsu hanya membuktikan bahwa kode Finance memanggil sesuatu — bukan
bahwa kontraknya cocok dengan yang sebenarnya dibangun modul sebelah.

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Contract version | `FIN-TEST-0.2` — status `locked` 20 September 2026 |
| Tanggal | 20 September 2026 |
| Keputusan | `FIN-DES-025`..`028` |

## A.1 Utang jasa tenaga medis

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-042` | Hasil jasa yang sama dicatat dua kali | Integrasi | `409`; satu utang jasa |
| `FIN-VAL-043` | Hasil jasa yang belum disetujui Medical Fee dicatat sebagai utang | Integrasi | `422` |
| `FIN-DES-025` | Utang dibuat untuk perawat, bukan dokter | Integrasi | Tersimpan dengan `PayeeType` = `NURSE`; **tidak** memerlukan tabel terpisah |
| `FIN-DES-025` | Satu pembayaran merekap utang dokter **dan** perawat sekaligus | Integrasi | Satu `FinPayment`, alokasi ke keduanya, semuanya lunas |

## A.2 Potongan dan nilai transfer

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-027` | Jasa Rp 24.500.000, potongan Rp 4.325.000, tambahan Rp 500.000 | Unit | `NetTransferAmount` = Rp 20.675.000; `TotalAmount` tetap Rp 24.500.000 |
| `FIN-DES-028` | Pembayaran di atas ditandai sudah dibayar | Integrasi | Ketiga utang jasa berstatus **lunas penuh**; sisa nol — **bukan** Rp 4.325.000 |
| `FIN-VAL-090` | Kasbon Rp 5.000.000 atas jasa Rp 3.000.000 | Integrasi | `422` dengan pesan menyebut kedua angka; pembayaran tidak dapat diajukan |
| `FIN-VAL-091` | Potongan persis menghabiskan seluruh jasa | Integrasi | `422`; diarahkan memakai koreksi utang |
| `FIN-VAL-093` | Pos `OTHER` tanpa alasan | Integrasi database | Ditolak check constraint, bukan hanya oleh service |
| `FIN-VAL-094` | Potongan ditambahkan pada pembayaran berstatus `SUBMITTED` | Integrasi | `422` |
| `FIN-DES-027` | Baris potongan dihapus saat masih draf | Integrasi | `NetTransferAmount` dihitung ulang seketika |
| `FIN-VAL-096` | Pembayaran jenis jasa medis dialokasikan ke utang supplier | Integrasi | `400` |

## A.3 Privasi yang bertambah

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-PERM` A.4 | Pembayaran jasa medis disimpan, lalu log diperiksa | Integrasi | Log **tidak memuat** `PayeeReferenceId`, `PayeeType`, maupun `Reason` potongan |
| `FIN-PERM` A.4 | Auditor membuka rincian pembayaran jasa medis | Integrasi | Total potongan terlihat; **rincian per pos tidak** |
| `FIN-VAL-071` | Kejadian akuntansi untuk pembayaran jasa medis | Unit | Payload tidak memuat identitas penerima maupun rincian potongan |

Baris terakhir penting: potongan seperti kasbon dan PPh 21 adalah urusan rumah sakit dengan
penerimanya. Accounting cukup menerima nilai yang relevan bagi jurnal, bukan alasan di balik
tiap potongan.
