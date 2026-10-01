# Matriks Uji Penerimaan — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-TEST-1.6` |
| `last_changed_in` (1.6) | `FIN-TEST-1.6` — AMENDMENT REVISI 9, 29 September 2026 (bagian F baru: sepuluh baris uji untuk `FIN-DES-064`/`065` dan `FIN-VAL-144`..`146`). Status **`approved`** — disahkan oleh Yasmin via `FIN-DEC-080` dan `FIN-DEC-081` (mengoreksi `FIN-DEC-041`) |
| `last_changed_in` | `FIN-TEST-1.5` — AMENDMENT REVISI 7, 28 September 2026 (bagian E baru). Sebelumnya `1.4` — AMENDMENT REVISI 6 (bagian D) |
| Status | Revisi 1.1 `approved` dan `locked` 25 September 2026; 1.2/1.3 mengikuti AMENDMENT REVISI 4/5; 1.4 disetujui owner 28 September 2026. **`1.6` (bagian F) `approved` 29 September 2026 bersama `FIN-DEC-080`/`081`** |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-29 (untuk `1.6`) |
| Input revision | `contracts/validation-matrix.md` `FIN-VAL-1.5`, `contracts/integration-contract.md` `FIN-INTEGRATION-1.6`, `02-backend-architecture.md` bagian H (`FIN-DES-064`, `FIN-DES-065`), `00-interview-decisions.md` `FIN-DEC-080`..`081` |
| Catatan project test | Project test terpisah belum terdeteksi di repository pada `09101d05`, dan **belum diperiksa ulang** pada `cba60cb0`. Kolom "Jenis test" menyatakan **jenis yang seharusnya**, bukan yang sudah tersedia |

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
| `FIN-DEC-041` | `BilDepositMovement` `RELEASE` disinkronkan | Integrasi | ~~Baris kejadian `PENGEMBALIAN-UANG-MUKA`, bukan kode pemakaian~~ — **baris uji ini DIGANTIKAN §F.1** oleh AMENDMENT REVISI 9 (`FIN-DES-064`): yang benar justru `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, karena mutasi `RELEASE` tidak mengeluarkan kas. Memakai baris ini apa adanya akan **meloloskan** kesalahan yang dicegah `FIN-VAL-144` |
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


---

# AMENDMENT REVISI 4

| Field | Nilai |
|---|---|
| Contract version | `FIN-TEST-1.2` — status `locked` 25 September 2026 (disetujui Yasmin bersama `FIN-DES-037`..`044`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-045`..`055` |

## B.1 Purchase Order dan approval berjenjang

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-100` | PO diajukan tanpa satu pun baris item | Integrasi | `400` |
| `FIN-VAL-101` | Pengaju PO menyetujui PO-nya sendiri | Integrasi | `422` |
| `FIN-VAL-101` | PO disimpan langsung ke database dengan `ApprovedByUserId` = `RequestedByUserId` | Integrasi database | Ditolak check constraint |
| `FIN-VAL-102` | PO Rp 62.000.000 diajukan, Supervisor Finance mencoba menyetujui | Integrasi | `403`; hanya Manajer Finance yang berhak |
| `FIN-VAL-102` | PO Rp 40.000.000 disetujui Supervisor Finance | Integrasi | `200`; status `APPROVED` |
| `FIN-DEC-052` | PO, Purchasing Invoice, **dan pembayaran** tepat Rp 50.000.000 diajukan | Unit | Ketiganya `ApprovalTier = TIER_2` — **bukan** `TIER_1`. Nilai batas ini yang berubah dari kode sebelum revisi 4 (`FIN-DES-039` koreksi) |
| `FIN-DEC-052` | Pembayaran tepat Rp 50.000.000 yang sudah `SUBMITTED` sebelum resolver diganti | Review diff + verifikasi data | `ApprovalTier` tersimpannya **tidak** dihitung ulang; tetap `TIER_1` sesuai saat diajukan |
| `FIN-VAL-103` | PO yang sudah ada GR dibatalkan | Integrasi | `422` |
| `FIN-STATE` B.1 | PO `APPROVED`, GR sebagian dicatat | Integrasi | Status PO menjadi `PARTIALLY_RECEIVED`, bukan `FULLY_RECEIVED` |

## B.2 Tanda Terima Barang dan Tukar Faktur

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-104` | GR mencatat kuantitas melebihi sisa baris PO | Integrasi | `422`; kuantitas diterima PO tidak berubah |
| `FIN-STATE` B.3 | Tukar Faktur dicatat tanpa PO maupun GR | Integrasi | Berhasil disimpan, `PurchaseOrderId` dan `GoodsReceiptId` `NULL` (`FIN-DEC-051`) |
| `FIN-DES-037` | Tukar Faktur dicatat dengan `MstSupplier.PaymentTermDays` = 30, `ReceivedDate` = 1 November | Unit | `EstimatedDueDate` = 1 Desember, dihitung backend bukan diterima dari layar |

## B.3 Purchasing Invoice

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-105` | Dua Purchasing Invoice dibuat dari satu Tukar Faktur yang sama, hampir bersamaan | Integrasi, konkuren | Tepat satu invoice tersimpan; yang kedua `409` dijaga unique index, bukan hanya pemeriksaan service |
| `FIN-VAL-106` | Purchasing Invoice dibuat dari Tukar Faktur berstatus `CANCELLED` | Integrasi | `422` |
| `FIN-VAL-107` | `TotalAmount` dikirim tidak sama dengan `Subtotal - Discount + PPN - DownPayment - OtherDeduction` | Integrasi | `422` |
| `FIN-STATE` B.4 | Purchasing Invoice disetujui | Integrasi | `FinSupplierPayable` baru tercipta dengan `SourcePurchasingInvoiceId` terisi; Tukar Faktur sumber menjadi `LINKED_TO_INVOICE`; baris outbox `PPN-MASUKAN-PEMBELIAN` `PENDING` — seluruhnya dalam satu transaksi |
| `FIN-VAL-122` | Worker pengiriman berjalan sebelum Accounting meratifikasi `PPN-MASUKAN-PEMBELIAN` | Integrasi | Baris outbox tetap `PENDING`, **tidak** dikirim; baris kejadian lain yang sudah teratifikasi tetap terkirim normal |
| `FIN-DES-040` | Input utang supplier manual (tanpa Purchasing Invoice) tetap dipakai | Integrasi | Berhasil; `SourcePurchasingInvoiceId` `NULL` — jalur lama tidak rusak |

## B.4 Retur Pembelian dan Deposit Retur

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-110` | Retur diajukan atas invoice berstatus `DRAFT` | Integrasi | `422` |
| `FIN-VAL-111` | Nilai retur melebihi nilai invoice sumber | Integrasi | `422` |
| `FIN-STATE` B.5 | Retur dikonfirmasi | Integrasi | `FinSupplierReturnDeposit` baru tercipta, `AvailableAmount` = `TotalAmount` retur |
| `FIN-VAL-112` | Deposit Retur saldo Rp 2.000.000 dipakai Rp 3.000.000 | Integrasi | `422` dengan pesan menyebut saldo Rp 2.000.000 |
| `FIN-VAL-113` | Deposit Retur milik Supplier A dipakai ke invoice Supplier B | Integrasi | `422` |
| `FIN-DEC-047` | Deposit Retur dipakai sebagian ke satu invoice, sisanya ke invoice lain (lintas Purchasing Invoice) | Integrasi | Dua baris `FinSupplierReturnDepositUsage`; `AvailableAmount` berkurang sesuai jumlah keduanya, tidak pernah negatif |

## B.5 Batch Tagihan AR

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-114` | Piutang yang sudah tergabung batch `DRAFT` dimasukkan ke batch lain | Integrasi | `409` |
| `FIN-VAL-115` | Batch dibuat dari piutang dua penjamin berbeda | Integrasi | `400` |
| `FIN-VAL-116` | Batch kosong diterbitkan | Integrasi | `422` |
| `FIN-STATE` B.7 | Batch `DRAFT` dibatalkan | Integrasi | Anggotanya bisa langsung digabung ke batch baru lain — tidak terkunci selamanya |
| `FIN-DEC-048` | Batch diterbitkan, dokumennya diminta | Integrasi | Response memuat rincian per `FinReceivable` anggota, masing-masing merujuk dokumen `BillingCompanyGuarantorInvoiceDocumentService` — **bukan** data yang disalin ulang ke tabel Finance |
| `FIN-STATE` B.7 | Sebagian piutang anggota batch dilunasi lewat alokasi penerimaan biasa | Integrasi | Status batch otomatis `PARTIALLY_PAID`; status `FinReceivable` anggota berubah lewat jalur alokasi yang sudah ada, **bukan** endpoint batch |

## B.6 Potongan penerimaan (AR)

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-118` | Potongan PPh 23 dengan `Amount` = 0 dicatat | Integrasi | `400` |
| `FIN-VAL-119` | Potongan jenis `OTHER` tanpa `Reason` | Integrasi database | Ditolak check constraint |
| `FIN-DEC-055` | Piutang sisa Rp 10.000.000 diselesaikan dengan penerimaan tunai Rp 9.770.000 + potongan PPh 23 Rp 230.000 | Integrasi | `OutstandingAmount` piutang menjadi **nol** — bukan Rp 230.000; `AllocatedAmount` memuat kedua komponen |
| `FIN-VAL-120` | Potongan ditambahkan setelah penerimaan `ALLOCATED` | Integrasi | `422` |
| `FIN-VAL-071` | Kejadian akuntansi untuk penerimaan dengan potongan | Unit | Payload tidak memuat `Reason` potongan bila memuat keterangan pihak ketiga (Sensitif) |

## B.7 Ringkasan arah aliran yang paling mudah tertukar

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-028` vs `FIN-DEC-055` | Potongan sisi Payable (`FinPaymentDeduction`) dan potongan sisi AR (`FinReceiptDeduction`) dicatat pada nilai yang sama, lalu efeknya ke saldo dibandingkan | Integrasi | Sisi Payable: `OutstandingAmount` utang **tidak berkurang** oleh potongan. Sisi AR: `OutstandingAmount` piutang **berkurang** oleh potongan. Keduanya harus terbukti dalam satu skenario perbandingan, bukan diuji terpisah — inilah kesalahan paling mahal pada amendment ini (`02-backend-architecture.md` C.11) |

---

# AMENDMENT REVISI 5

| Field | Nilai |
|---|---|
| Contract version | `FIN-TEST-1.3` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-057`, `058`, `061`, `062`; `FIN-DES-045`..`050` |
| Menggantikan | B.4 tiga baris terakhir (pemakaian deposit lewat `apply`) dan B.6 (potongan lewat endpoint terpisah) |

## C.1 Deposit Retur sebagai sumber dana pembayaran

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-045` | Pembayaran dua faktur Rp 6.000.000 + Rp 4.000.000, deposit Rp 2.500.000 ditambahkan | Integrasi | `DepositAppliedAmount` = Rp 2.500.000; `NetTransferAmount` = Rp 7.500.000; tetap **dua** baris alokasi, bukan tiga |
| `FIN-DES-045` | Pembayaran di atas ditandai `PAID` | Integrasi | Kedua utang `OutstandingAmount = 0`; deposit `AvailableAmount = 0`, status `EXHAUSTED`; baris pemakaian `APPLIED` |
| `FIN-DES-046` | Dua pembayaran `DRAFT` hampir bersamaan memakai deposit Rp 2.500.000 yang sama, masing-masing Rp 2.500.000 | Integrasi, konkuren | Tepat satu berhasil; yang kedua `422` saldo kurang; `AvailableAmount` tidak pernah negatif |
| `FIN-DES-046` | Pembayaran ber-deposit ditolak penyetuju | Integrasi | Baris pemakaian `RELEASED`, `ReleasedAt` terisi; saldo deposit kembali penuh; deposit kembali `AVAILABLE` |
| `FIN-DES-046` | Petugas melepas baris deposit saat pembayaran masih `DRAFT` | Integrasi | Baris **tidak terhapus**, status `RELEASED`; `DepositAppliedAmount` kembali 0 |
| `FIN-VAL-091` diubah | Pembayaran Rp 2.000.000 seluruhnya dari deposit Rp 2.000.000, diajukan | Integrasi | Diterima walau `NetTransferAmount = 0` |
| `FIN-VAL-091` tidak berubah | Pembayaran tanpa deposit dengan `NetTransferAmount = 0` karena potongan | Integrasi | Tetap ditolak `422` seperti sebelumnya |
| `FIN-VAL-056` diubah | Pembayaran seluruhnya dari deposit ditandai `PAID` tanpa nomor bukti transfer | Integrasi | Diterima |
| `FIN-VAL-113` | Deposit milik Supplier A ditambahkan ke pembayaran Supplier B | Integrasi | `422` |
| `FIN-VAL-123` | Deposit ditambahkan ke pembayaran `MEDICAL_SERVICE` | Integrasi | `422` |
| `FIN-VAL-124` | Deposit ditambahkan ke pembayaran `SUBMITTED` | Integrasi | `422` |
| `FIN-VAL-126` | Deposit Rp 12.000.000 pada pembayaran yang butuh Rp 10.000.000 | Integrasi | `422` menyebut maksimum Rp 10.000.000 |
| `FIN-DES-046` | Deposit yang punya baris `RESERVED` dibatalkan | Integrasi | `422` |

## C.2 Kejadian Accounting retur dan deposit

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-061` | Retur Rp 2.500.000 dikonfirmasi | Integrasi | Satu baris outbox `RETUR-PEMBELIAN` Rp 2.500.000 `PENDING`, satu transaksi dengan penerbitan deposit |
| `FIN-VAL-131` | Pembayaran Rp 10.000.000 dengan deposit Rp 2.500.000 ditandai `PAID` | Integrasi | Dua baris: `AP_PAYMENT` Rp 7.500.000 dan `PEMAKAIAN-DEPOSIT-RETUR` Rp 2.500.000; jumlah = `TotalAmount` |
| `FIN-VAL-131` | Pembayaran seluruhnya dari deposit ditandai `PAID` | Integrasi | **Nol** baris `AP_PAYMENT`; satu `PEMAKAIAN-DEPOSIT-RETUR` |
| Regresi | Pembayaran tanpa deposit ditandai `PAID` | Integrasi | `AP_PAYMENT` bernilai `TotalAmount` — **identik** dengan sebelum revisi 5; nol baris `PEMAKAIAN-DEPOSIT-RETUR` |

## C.3 Potongan AR bersama alokasinya

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-048` | Piutang Rp 10.000.000; satu permintaan alokasi: uang Rp 9.745.000 + PPh 23 Rp 230.000 + biaya bank Rp 25.000 | Integrasi | Piutang `SETTLED`; `UnallocatedAmount` penerimaan 0; **dua** baris `FinReceiptDeduction` dengan `DeductionNumber` berbeda |
| `FIN-DES-050` | Idem | Integrasi | Dua baris outbox `POTONGAN-PIUTANG-NON-TUNAI` (Rp 230.000 dan Rp 25.000) dengan `SourceTransactionId` berbeda; nol `AR_PAYMENT`/`PENYESUAIAN-PIUTANG` untuk potongan |
| `FIN-VAL-033` diperluas | Uang Rp 9.800.000 + potongan Rp 230.000 ke piutang Rp 10.000.000 | Integrasi | `422`; tidak ada satu pun baris alokasi maupun potongan tersimpan |
| `FIN-VAL-128` | Potongan pada baris `INVOICE_DIRECT` | Integrasi | `400` |
| `FIN-DES-048` | Permintaan alokasi tanpa field `deductions` | Integrasi | Perilaku persis sebelum revisi 5 — regresi nol |
| `FIN-DES-049` | Alokasi ber-potongan di atas dibalik manual | Integrasi | Dua baris potongan pembalik; piutang kembali Rp 10.000.000; dua `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` |
| `FIN-DES-049` | Tender Billing sumber penerimaan dibatalkan | Integrasi | Idem, **otomatis** — potongan ikut terbalik tanpa tindakan petugas |
| `FIN-DES-049` | Potongan yang sudah dibalik dibalik lagi | Integrasi database | Ditolak unique index `ReversalOfDeductionId` |
| `FinanceReceivableService` satu penulis | Review diff | Review | `FinanceReceiptService` tidak menulis kolom `FinReceivable` langsung; seluruh efek lewat `ApplyAllocationAsync`/`ReverseAllocationAsync` |

## C.4 Gerbang ratifikasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-132` | Worker berjalan sebelum `FIN-OQ-026` turun | Integrasi | Baris keempat kode tetap `PENDING`, tidak terkirim; baris kode yang sudah teratifikasi tetap terkirim |

---

# D. AMENDMENT REVISI 6 — Katalog final kejadian

Menurunkan `FIN-DES-051`..`058`. Setiap bagian memuat jalur berhasil **dan** jalur gagal.

## D.1 Penamaan ulang dan penghapusan alias

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-058` | Utang supplier diakui | Integrasi | Baris outbox ber-`EventTypeCode = PENGAKUAN-HUTANG-SUPPLIER`; **tidak ada** baris `AP_CREATED` |
| `FIN-DES-058` | Pembayaran supplier ditandai sudah dibayar | Integrasi | `PEMBAYARAN-HUTANG-SUPPLIER`; **tidak ada** `AP_PAYMENT` |
| `FIN-DES-058` | Penerimaan piutang dicatat | Integrasi | `PENERIMAAN-PIUTANG`; **tidak ada** `AR_PAYMENT` |
| `FIN-DES-058` | Write-off piutang disetujui | Integrasi | `PEMUTIHAN-PIUTANG`; **tidak ada** `AR_WRITEOFF` |
| `FIN-DES-051` | **Jalur gagal:** kode mencoba menulis nama yang sudah dihapus | Unit/kompilasi | Konstanta alias tidak ada lagi, sehingga penulisannya **tidak dapat dikompilasi** — bukan gagal saat berjalan |
| `FIN-DES-058` | Baris outbox lama bernama pendek | Integrasi | Baris lama **tetap** bernama lama dan tetap `PENDING`; tidak ada migration yang menimpanya |

## D.2 Bentuk pesan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-139` | Kejadian tanpa komponen diterbitkan | Unit | `PayloadJson` **tidak memuat** kunci `Components` sama sekali; `ComponentsJson` kolom tetap `null` |
| `FIN-VAL-139` | Kejadian **dengan** komponen diterbitkan | Unit | `PayloadJson` memuat `Components` berisi daftar `{ComponentCode, Amount}` |
| `FIN-VAL-138` | **Jalur gagal:** kejadian transaksi bernilai nol | Unit | Ditolak `AccountingOutboxException`; baris outbox tidak dibuat |
| `FIN-VAL-138` | Kode penanda bernilai nol | Unit | Diterima; baris outbox dibuat dengan `Amount = 0` |
| `FIN-DES-058` | **Jalur gagal:** kejadian bernilai negatif | Unit | Ditolak; tidak ada baris outbox |

## D.3 Selisih kas dan penanda shift

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-053` | Shift kurang Rp 30.000, disahkan sekali langsung selesai | Integrasi | Satu `SELISIH-KAS-KURANG` bernilai `30000.00`, `AccountingDate` = tanggal shift, `SourceTransactionId` = Id shift |
| `FIN-DES-053` | Shift lebih Rp 15.000, disahkan | Integrasi | Satu `SELISIH-KAS-LEBIH` bernilai `15000.00` |
| `FIN-VAL-140` | **Jalur gagal:** pengesahan pertama "perlu tindak lanjut", lalu diselesaikan | Integrasi | **Nol** kejadian dari pengesahan pertama; **tepat satu** dari yang menyelesaikan. Total kejadian selisih untuk shift itu = 1 |
| `FIN-VAL-140` | **Jalur gagal:** kode mencoba menerbitkan kejadian selisih kedua | Integrasi | Ditolak unique index outbox (`409`), bukan tersimpan sebagai versi 2 |
| `FIN-DES-054` | Shift ditutup dengan kas **pas** (`CLOSED`) | Integrasi | Satu `PENUTUPAN-SHIFT-KASIR` bernilai `0` — inilah skenario yang paling mudah terlewat |
| `FIN-DES-054` | Shift dengan selisih, sesudah disahkan (`REVIEWED`) | Integrasi | Satu `PENUTUPAN-SHIFT-KASIR` **dan** satu `SELISIH-KAS-*` |
| `FIN-DES-054` | **Jalur gagal:** shift masih `CLOSED_WITH_VARIANCE` | Integrasi | **Nol** penanda — shift itu memang harus tetap menahan tutup bulan |
| `FIN-DES-054` | **Jalur gagal:** shift `PERLU_TINDAK_LANJUT` | Integrasi | **Nol** penanda |
| `FIN-DES-054` | Shift tertutup lalu dibuka kembali | Integrasi | Satu `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bernilai `0`; penanda penutupan berikutnya boleh terbit lagi pada siklus baru |

## D.4 Potongan piutang sesudah dipecah

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-052` | Piutang Rp 10.000.000, masuk Rp 9.745.000, PPh 23 Rp 230.000, biaya bank Rp 25.000 | Integrasi | Tiga kejadian: penerimaan Rp 9.745.000, `POTONGAN-PPH23-PIUTANG` Rp 230.000, `POTONGAN-BIAYA-BANK-PIUTANG` Rp 25.000. Piutang `SETTLED` |
| `FIN-DES-052` | Alokasi pembawa kedua potongan dibalik | Integrasi | Dua kejadian pembalik dengan kode yang bersesuaian, keduanya bernilai positif; piutang terbuka kembali Rp 255.000 |
| `FIN-VAL-137` | **Jalur gagal:** potongan berjenis lain-lain | API | `400` dengan pesan yang menyebut jenis yang tersedia; **seluruh** permintaan alokasi ditolak, nol piutang berkurang |
| `FIN-VAL-130` | **Jalur gagal:** potongan ditulis dengan kode kas | Unit | Tertangkap sebagai kesalahan kode; tidak ada baris outbox berkode kas untuk potongan |

## D.5 Retur pembelian dan porsi PPN

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-055` | Retur pokok Rp 1.000.000 + PPN Rp 110.000 dikonfirmasi | Integrasi | `RETUR-PEMBELIAN` Rp 1.000.000 **dan** `PPN-MASUKAN-RETUR-PEMBELIAN` Rp 110.000; kredit retur `AvailableAmount` Rp 1.110.000 |
| `FIN-DES-055` | Retur tanpa PPN dikonfirmasi | Integrasi | Hanya `RETUR-PEMBELIAN`; **nol** kejadian PPN; kredit retur = nilai pokok |
| `FIN-VAL-133` | **Jalur gagal:** PPN retur negatif | API | `400`, retur tidak tersimpan |
| `FIN-VAL-134` | **Jalur gagal:** nilai pokok tidak sama dengan jumlah barisnya | API | `422`, retur tidak dikonfirmasi |
| `FIN-VAL-136` | **Jalur gagal:** kejadian retur dikirim termasuk PPN | Unit | Tertangkap sebagai kesalahan kode |
| `FIN-VAL-143` | **Jalur gagal:** retur pokok sebesar total faktur ber-PPN, lalu PPN ditambahkan di atasnya | API | `400`; retur tidak tersimpan. Tanpa aturan ini kredit retur melebihi nilai faktur yang diretur |
| `FIN-DES-055` | Kredit retur dipakai melunasi utang supplier | Integrasi | `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` sebesar porsi yang dipakai; **tidak ada** baris bernama `PEMAKAIAN-DEPOSIT-RETUR` |

## D.6 Refund kas dan gap Billing

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-056` | Kelebihan bayar `SETTLEMENT` diakui | Integrasi | `PENGAKUAN-KELEBIHAN-BAYAR` terbit — cakupan diperluas, bukan kode baru |
| `FIN-DES-056` | Refund tunai atas kredit `SETTLEMENT` | Integrasi | `PENGEMBALIAN-UANG-MUKA` terbit |
| `FIN-DEC-067` | **Jalur gagal:** kelebihan berasal dari uang muka pasien | Integrasi | **Nol** `PENGAKUAN-KELEBIHAN-BAYAR` — syarat ketiga; kewajiban tidak tercatat dua kali |
| `FIN-VAL-141` | **Jalur gagal:** refund kredit `REFERRED_OUTPATIENT_ADMIN` dieksekusi | Integrasi | Baris intake `ERROR` dengan sebab yang menyebut jenis kreditnya; **nol** kejadian; baris terlihat di layar pantauan |
| `FIN-VAL-142` | **Jalur gagal:** tender top-up deposit dibalik tanpa mutasi pembalik | Integrasi | Baris intake `ERROR` menunjuk `FIN-OQ-034`; **nol** kejadian; nol tulisan ke tabel Billing |
| `FIN-DES-057` | Pembalikan top-up deposit yang dananya **belum** terpakai | Integrasi | `PEMBALIKAN-PENERIMAAN-UANG-MUKA` terbit seperti rancangan yang sudah ada |

## D.7 Layar pantauan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-058` | Petugas membuka filter jenis kejadian | API | Daftar pilihan memuat **seluruh** kode katalog, termasuk yang ditambahkan revisi 4, 5, dan 6 |
| `FIN-VAL-141`/`142` | Petugas mencari fakta yang gagal disinkronkan | API | Baris intake `ERROR` dapat ditemukan beserta sebabnya, tanpa membaca database langsung |

---

# E. AMENDMENT REVISI 7 — gerbang penanda dan penyelarasan menu

Menurunkan `FIN-DES-059`, `FIN-DES-060`, dan `FR-FIN-108`..`110`.

## E.1 Gerbang worker kode penanda

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-059` | Shift tertutup selama gerbang `FIN-OQ-035` masih tertutup | Integrasi | Baris outbox penanda ditulis `PENDING`; **tidak** dikirim; **tidak** ditandai `FAILED`; `AttemptCount` tetap `0` |
| `FIN-DES-059` | **Jalur gagal:** worker mencoba mengirim kode penanda walau gerbang tertutup | Integrasi | Baris dilewati (pola `FIN-VAL-132`), bukan dikirim lalu gagal `400` |
| `FIN-DES-059` | Gerbang dibuka sesudah Accounting menjawab | Integrasi | Seluruh baris penanda yang menumpuk terkirim berurutan; yang sudah pernah diterima dijawab `200` dan **tetap dihitung sukses** |
| `5.11` | Kotak masuk menjawab `200` untuk pesan yang sudah pernah diterima | Unit/integrasi | Worker menandainya `ACKNOWLEDGED`, **bukan** `FAILED` dan **bukan** diulang — kesalahan yang paling mudah terjadi |

## E.2 Penyelarasan menu dan daftar Purchasing

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FR-FIN-109` | Petugas membuka daftar PO, Tanda Terima Barang, Tukar Faktur, Faktur Pembelian, Retur | Integrasi | Kelima endpoint `GET /` mengembalikan `PagedResult` sesuai `api-contract.md` `B.1`-`B.5` |
| `FR-FIN-109` | **Jalur gagal:** penyaringan memakai nilai status yang tidak dikenal | API | `400` dengan pesan yang menyebut nilai yang sah |
| `FR-FIN-108` | Petugas membuka sidebar Keuangan | Manual/visual | Submenu "Pembelian" ada, label Bahasa Indonesia, "Faktur Pembelian" terbaca, butir "Faktur & Tagihan Supplier" **tidak** berpindah |
| `FR-FIN-108` | **Jalur gagal:** butir menu didaftarkan sebelum layarnya punya sumber data | Manual | Dihitung **gagal** — urutan `03-frontend-architecture.md` bagian 15.3 dilanggar |
| `FR-FIN-110` | Petugas dengan hak akses AP terbatas membuka menu | Integrasi | Butir yang tampil hanya yang endpoint-nya mengizinkan. **Belum dapat diuji** sampai `FIN-OQ-036` turun |
| `FIN-CAP-032` | **Jalur gagal:** butir "Tagihan Gabungan Penjamin" didaftarkan | Manual | Dihitung **gagal** — entity `FinReceivableInvoiceBatch` masih nol baris |

---

# F. AMENDMENT REVISI 9 — arti tunggal mutasi `RELEASE`

Menurunkan `FIN-DES-064` dan `FIN-DES-065` via `FIN-DEC-080` dan `FIN-DEC-081`. Seluruh baris di bawah berstatus **`approved`** (29 September 2026).

## F.1 Pembatalan alokasi uang muka

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-064` | Tender top-up deposit dibalik **sesudah** dananya dipakai melunasi tagihan | Integrasi | **Dua** kejadian dari satu pembalikan: `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` sebesar porsi yang terpakai **dan** `PEMBALIKAN-PENERIMAAN-UANG-MUKA` sebesar nilai tender |
| `FIN-DES-064` | Nilai kedua kejadian itu | Integrasi | Hasil bersihnya debit Piutang dan kredit Kas; saldo Uang Muka Pasien kembali nol — **bukan** minus |
| `FIN-VAL-144` | **Jalur gagal:** mutasi `RELEASE` dikirim sebagai `PENGEMBALIAN-UANG-MUKA` | Unit | Tertangkap sebagai kesalahan kode. Nol baris outbox berkode `PENGEMBALIAN-UANG-MUKA` untuk mutasi `RELEASE` mana pun |
| `FIN-VAL-146` | **Jalur gagal:** hanya satu dari dua kejadian yang terbit | Integrasi | Dihitung **gagal**. Kas dikredit tanpa piutang terbuka kembali, atau sebaliknya |
| `FIN-DES-064` | Tender top-up dibalik **sebelum** dananya dipakai | Integrasi | Hanya `PEMBALIKAN-PENERIMAAN-UANG-MUKA`; **nol** `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, karena tidak ada alokasi yang dibatalkan |

## F.2 Mutasi pelepasan yang asalnya belum dikenal

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-VAL-145` | **Jalur gagal:** mutasi `RELEASE` tanpa mutasi `REVERSAL` ber-`SettlementId` sama | Integrasi | Baris intake `ERROR` dengan sebab yang menyebut jenis mutasinya dan menunjuk `FIN-OQ-037`; **nol** kejadian; baris terlihat di layar pantauan |
| `FIN-VAL-145` | Baris `ERROR` itu dicari petugas | API | Dapat ditemukan beserta sebabnya tanpa membaca database — jalur yang sama dengan `FIN-VAL-141`/`142` pada §D.7 |
| `FIN-DES-065` | **Jalur gagal:** kode menebak lawan jurnal dari kolom `Reason` | Unit | Dihitung **gagal**. `Reason` adalah teks bebas dan **MUST NOT** menjadi kunci logika akuntansi |

## F.3 Kode yang cakupannya dipersempit

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DES-064` | Refund kas atas kredit `ALLOCATION_EXCESS` atau `SETTLEMENT` dieksekusi | Integrasi | `PENGEMBALIAN-UANG-MUKA` **tetap** terbit — kode ini tidak mati, hanya pemicunya dipersempit ke `BilRefundCase` |
| `FIN-DES-064` | Seluruh mutasi `RELEASE` pada satu periode | Integrasi | **Nol** di antaranya menerbitkan `PENGEMBALIAN-UANG-MUKA` |

## G.1 Pelacakan klaim penjamin dan pemecahan layar ke bentuk V1

`last_changed_in`: `FIN-TEST-1.6` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-094`..`FIN-DEC-098`.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-097` | **Berhasil.** Batch `ISSUED` ditandai berkasnya diterima penjamin, lalu dicatat disetujui penuh sebesar total tagihan, lalu ditutup | Manual/runtime | `ClaimStatus` berpindah `SUBMITTED` ke `PAYER_VERIFIED` ke `APPROVED` ke `CLOSED`; ketiga tanda waktu terisi; `ClaimVarianceAmount` bernilai nol |
| `FIN-DEC-097` | **Berhasil.** Penjamin menyetujui lebih kecil dari tagihan, disertai alasan | Manual/runtime | `ApprovedAmount` tersimpan; `ClaimVarianceAmount` sama dengan selisihnya; `OutstandingAmount` seluruh piutang anggota **tidak berubah sama sekali** |
| `FIN-DES-071` | **Berhasil.** Petugas menindaklanjuti selisih lewat write-off pada piutang anggota | Manual/runtime | Write-off tetap melewati maker-checker yang sudah ada; `OutstandingAmount` baru berkurang setelah write-off **disetujui**, bukan saat klaim disetujui |
| `FIN-VAL-147` | **Gagal.** Aksi klaim dijalankan pada batch yang masih `DRAFT` | Manual/runtime | `422` beserta pesan bahwa tagihan belum diterbitkan; `ClaimStatus` tetap kosong |
| `FIN-VAL-150` | **Gagal.** Nominal disetujui melebihi total tagihan | Manual/runtime | `422`; nilai lama tidak tertimpa |
| `FIN-VAL-151` | **Gagal.** Nominal disetujui lebih kecil dari tagihan tanpa alasan | Manual/runtime | `400`; perpindahan status tidak terjadi |
| `FIN-VAL-152` | **Gagal.** Klaim yang sudah `CLOSED` dicoba diubah lagi | Manual/runtime | `422`; status tetap `CLOSED` |
| `FIN-VAL-153` | **Gagal.** Dua petugas mengubah klaim batch yang sama dari layar yang dibuka bersamaan | Manual/runtime | Petugas kedua menerima `409` beserta ajakan memuat ulang; nol perubahan tertimpa diam-diam |
| `FIN-DES-070` | **Berhasil.** Batch berstatus klaim `APPROVED` menerima pembayaran sebagian dari penjamin | Manual/runtime | `Status` berpindah ke `PARTIALLY_PAID` oleh sistem, sementara `ClaimStatus` **tetap** `APPROVED` — kedua sumbu bergerak sendiri-sendiri |
| `FIN-DES-072` | **Gagal.** Pengguna tanpa `FinanceReceivableInvoiceBatch : Update` membuka layar Manajemen Klaim | Manual/runtime | Tombol aksi klaim tidak tampil; pemanggilan langsung endpoint ditolak `403` |
| `FIN-DEC-094` | **Berhasil.** Seluruh butir menu Transaksi A/R dan Transaksi A/P dapat dibuka dari sidebar | Manual/runtime | Setiap butir pada peta menu `03-frontend-architecture.md` bagian 17 membuka layar yang benar; nol butir mengarah ke rute yang tidak ada |
| `FIN-DEC-096` | **Berhasil.** Butir menu "Ayat Silang" membuka layar alokasi penerimaan yang sudah ada | Manual/runtime | Nol endpoint baru dipanggil; layar yang terbuka sama dengan yang dipakai `FE-FIN-004` |
| `FIN-DEC-094` | **Gagal.** Pengguna tanpa hak akses pada salah satu layar hasil pemecahan | Manual/runtime | Butir menunya tersembunyi (penyaring *fail-closed*), bukan tampil lalu ditolak |

## H.1 Piutang sewa non-pasien (Parkir dan Tenant)

`last_changed_in`: `FIN-TEST-1.7` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-099`..`FIN-DEC-104`.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `FIN-DEC-104` | **Berhasil.** Catat tagihan sewa kategori Parkir dan satu lagi kategori Tenant | Manual/runtime | Keduanya tersimpan pada tabel yang sama dengan `Category` berbeda; keduanya muncul pada daftar saat disaring kategorinya masing-masing |
| `FIN-DEC-100` | **Berhasil.** Catat tagihan untuk periode berikutnya bagi penyewa yang sama | Manual/runtime | Tersimpan sebagai baris baru yang berdiri sendiri; **nol** entity kontrak yang terbentuk |
| `FIN-DEC-101` | **Berhasil.** Sesudah mencatat beberapa tagihan sewa | Review kode + runtime | Tabel `FinReceivable` **tidak bertambah satu baris pun**; umur piutang pasien tidak berubah angkanya |
| `FIN-DES-074` | **Berhasil.** Buka umur piutang sewa dan umur piutang pasien | Manual/runtime | Kedua laporan memakai kelompok yang sama persis (`0-30`, `31-60`, `61-90`, `di atas 90 hari`), tetapi angkanya terpisah |
| `FIN-DEC-102` | **Berhasil.** Catat denda keterlambatan pada tagihan yang lewat jatuh tempo | Manual/runtime | Nominal denda tersimpan apa adanya; **nol** perhitungan otomatis terjadi walau tagihan sudah lama lewat tempo |
| `FIN-DEC-103` | **Berhasil.** Staf AR menghapus piutang sewa yang tidak tertagih | Manual/runtime | Status menjadi `WRITTEN_OFF` **dalam satu aksi**, tanpa antrean persetujuan; alasan tersimpan |
| `FIN-VAL-158` | **Gagal.** Hapus piutang tanpa mengisi alasan | Manual/runtime | `400`; status tidak berubah |
| `FIN-VAL-159` | **Gagal.** Koreksi tagihan yang sudah menerima pembayaran | Manual/runtime | `422`; isi tagihan tidak berubah |
| `FIN-VAL-161` | **Gagal.** Catat pelunasan melebihi nilai tagihan | Manual/runtime | `422`; sisa tagihan tidak berubah |
| `FIN-VAL-162` | **Gagal.** Catat pelunasan minus melebihi pembayaran yang pernah tercatat | Manual/runtime | `422` |
| `FIN-VAL-164` | **Gagal.** Dua petugas mengubah tagihan yang sama dari layar yang dibuka bersamaan | Manual/runtime | Petugas kedua menerima `409`; nol perubahan tertimpa diam-diam |
| `FIN-DES-075` | **Berhasil — dan inilah yang MUST diperiksa pemilik.** Catat pelunasan sewa, lalu buka kas harian, setoran bank, dan pemantauan kejadian akuntansi | Manual/runtime | Sisa tagihan sewa berkurang, **tetapi** uangnya **tidak muncul** pada ketiga layar itu, dan **nol** baris kotak keluar terbit. Ini perilaku yang dirancang, bukan cacat — dan menjadi bukti nyata mengapa `FIN-OQ-044` perlu diputuskan sebelum dipakai pada data sungguhan |
| `FIN-DES-077` | **Gagal.** Pengguna tanpa `FinanceNonPatientReceivable : Update` membuka layar tagihan sewa | Manual/runtime | Tombol catat pelunasan, hapus, dan batalkan tidak tampil; pemanggilan langsung endpoint ditolak `403` |
| `FIN-DEC-104` | **Berhasil.** Butir menu "Umur Piutang — Parkir" dan "— Tenant" | Manual/runtime | Keduanya membuka laporan umur piutang yang sama dengan saringan kategori berbeda; **nol** endpoint terpisah dipanggil |
