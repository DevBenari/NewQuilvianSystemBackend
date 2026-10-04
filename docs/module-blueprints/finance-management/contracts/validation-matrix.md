# Matriks Validasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-1.5` |
| `last_changed_in` (1.5) | `FIN-VAL-1.5` — AMENDMENT REVISI 9, 29 September 2026 (bagian E: `FIN-VAL-144`..`146`). Status **`approved`** — disahkan oleh Yasmin via `FIN-DEC-080` dan `FIN-DEC-081` (mengoreksi `FIN-DEC-041`) |
| `last_changed_in` | `FIN-VAL-1.4` — AMENDMENT REVISI 6, 28 September 2026 (bagian D: `FIN-VAL-133`..`143`; `FIN-VAL-130`..`132` diperbarui mengikuti katalog final) |
| Status | Revisi 1.1 `approved` dan `locked` 25 September 2026; 1.2/1.3 mengikuti AMENDMENT REVISI 4/5. **Revisi 1.5 (bagian E) `approved` 29 September 2026 bersama `FIN-DEC-080`/`081`** |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-29 (untuk `1.5`) |
| Input revision | `00-interview-decisions.md` — `FIN-DEC-001`..`081`; `02-backend-architecture.md` bagian H (`FIN-DES-064`, `FIN-DES-065`) |
| Dampak kompatibilitas | Revisi 1.4: **sebelas aturan ditambahkan** (`FIN-VAL-133`..`143`) dan **tiga aturan diperbarui** (`FIN-VAL-130`, `131`, `132`) karena nama kode yang dirujuknya sudah tidak ada lagi. Sebelumnya: satu aturan dicabut (`FIN-VAL-076`), sembilan ditambahkan (`FIN-VAL-078`..`086`) |

Pesan ditulis dalam bahasa yang dipahami pengguna, bukan istilah teknis. Kolom "Kode" adalah
kode HTTP yang dikembalikan.

---

## 1. Fakta masuk dari Billing

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-001` | Satu fakta hanya diolah satu kali | `POST /billing-intake/consume` | Sudah ada baris dengan `HandoffType` dan `SourceHandoffKey` yang sama | "Data dari Billing ini sudah pernah diproses sebelumnya." | `409` |
| `FIN-VAL-002` | Fakta yang sudah diolah tidak boleh diulang | `POST /billing-intake/{id}/retry` | Status baris `CONSUMED` atau `ACKNOWLEDGED` | "Data ini sudah berhasil diproses. Pengulangan akan membuat catatan ganda." | `422` |
| `FIN-VAL-003` | Nilai dari Billing disalin apa adanya | Seluruh pengolahan fakta | Nilai yang diolah berbeda dari nilai di handoff | "Nilai dari Billing tidak boleh diubah di Finance." | `422` |

## 2. Piutang

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-010` | Sisa piutang tidak boleh negatif | Seluruh perubahan piutang | Hasil perhitungan < 0 | "Sisa piutang tidak boleh kurang dari nol. Periksa kembali nilai yang dimasukkan." | `422` |
| `FIN-VAL-011` | Nilai asli piutang selalu seimbang | Seluruh perubahan piutang | `OriginalAmount` ≠ sisa + terbayar + koreksi + dihapusbukukan | "Rincian piutang tidak seimbang dengan nilai aslinya." | `422` |
| `FIN-VAL-012` | Satu fakta AR menghasilkan satu piutang | Pengolahan fakta AR | `SourceHandoffKey` sudah dipakai piutang lain | "Piutang untuk tagihan ini sudah pernah dibuat." | `409` |
| `FIN-VAL-013` | Kelengkapan berkas tidak menahan pengakuan piutang | Pengolahan fakta AR | — | Tidak ada pesan — piutang tetap dibuat (`FIN-DEC-013`) | — |
| `FIN-VAL-014` | Piutang lunas tidak boleh dihapusbukukan | `POST /receivables/{id}/write-offs` | Status piutang `SETTLED` | "Piutang yang sudah lunas tidak dapat dihapusbukukan." | `422` |
| `FIN-VAL-015` | Piutang yang sudah ada pelunasan tidak boleh dibatalkan | Pembatalan piutang | Sudah ada alokasi penerimaan | "Piutang ini sudah menerima pembayaran, jadi tidak dapat dibatalkan. Gunakan koreksi." | `422` |
| `FIN-VAL-016` | Pemilik manfaat wajib untuk piutang karyawan | Pengolahan fakta AR | `DebtorType` = `EMPLOYEE_BENEFIT` tetapi `BenefitOwnerId` kosong | "Piutang manfaat karyawan wajib menyebut pegawai pemiliknya." | `422` |

## 3. Koreksi dan penghapusan piutang

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-020` | Pengaju tidak boleh menyetujui permohonannya sendiri | Persetujuan koreksi dan penghapusan | `ApprovedBy` sama dengan `RequestedBy` | "Pengaju tidak boleh menyetujui permohonannya sendiri. Mintalah persetujuan pengguna lain yang berwenang." | `422` |
| `FIN-VAL-021` | Alasan wajib diisi | Pengajuan koreksi dan penghapusan | `Reason` kosong atau hanya spasi | "Alasan wajib diisi." | `400` |
| `FIN-VAL-022` | Alasan penolakan wajib diisi | Penolakan | `RejectionReason` kosong | "Alasan penolakan wajib diisi." | `400` |
| `FIN-VAL-023` | Nominal koreksi harus lebih dari nol | Pengajuan koreksi | `Amount` ≤ 0 | "Nominal harus lebih dari nol." | `400` |
| `FIN-VAL-024` | Koreksi pengurang tidak boleh melebihi sisa piutang | Persetujuan koreksi arah `CREDIT` | `Amount` > `OutstandingAmount` | "Nilai koreksi melebihi sisa piutang. Sisa saat ini Rp {sisa}." | `422` |
| `FIN-VAL-025` | Keputusan hanya boleh sekali | Persetujuan atau penolakan | Status bukan `REQUESTED` | "Permohonan ini sudah diputuskan sebelumnya." | `422` |

**Contoh `FIN-VAL-024`.** Sisa piutang penjamin Rp 3.000.000. Petugas mengajukan koreksi
pengurang Rp 5.000.000. Saat penyetuju menekan Setujui, sistem menolak dengan pesan "Nilai
koreksi melebihi sisa piutang. Sisa saat ini Rp 3.000.000." Piutang tidak berubah sama sekali.

## 4. Penerimaan

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-030` | Satu tender berhasil menghasilkan satu penerimaan | Pengolahan fakta penerimaan | `SourceTenderId` sudah dipakai | "Pembayaran ini sudah tercatat di Finance." | `409` |
| `FIN-VAL-031` | Nominal penerimaan disalin dari Billing | Pengolahan fakta penerimaan | Nominal berbeda dari `BilTender.Amount` | "Nominal penerimaan tidak boleh berbeda dari pembayaran di kasir." | `422` |
| `FIN-VAL-032` | Alokasi tidak boleh melebihi uang yang tersedia | `POST /receipts/{id}/allocations` | Total alokasi > `UnallocatedAmount` | "Nilai alokasi melebihi sisa uang pada penerimaan ini. Sisa Rp {sisa}." | `422` |
| `FIN-VAL-033` | Alokasi tidak boleh melebihi sisa piutang | `POST /receipts/{id}/allocations` | Alokasi ke satu piutang > sisa piutang itu | "Nilai alokasi melebihi sisa piutang {nomor}. Sisa Rp {sisa}." | `422` |
| `FIN-VAL-034` | Alokasi ke piutang wajib menyebut piutangnya | `POST /receipts/{id}/allocations` | `TargetType` = `RECEIVABLE` tetapi `ReceivableId` kosong | "Pilih piutang yang akan dilunasi." | `400` |
| `FIN-VAL-035` | Penerimaan yang sudah dibalik tidak boleh dialokasikan | `POST /receipts/{id}/allocations` | Status `REVERSED` | "Penerimaan ini sudah dibatalkan." | `422` |
| `FIN-VAL-036` | Shift kasir wajib untuk penerimaan tunai | Pengolahan fakta penerimaan | Metode tunai tetapi `CashierShiftId` kosong | "Penerimaan tunai wajib menyebut shift kasirnya." | `422` |
| `FIN-VAL-037` | Pembalikan tidak menghapus baris | Seluruh pembalikan | — | Tidak ada pesan — baris pembalik dibuat (`FIN-DES-012`) | — |

**Contoh `FIN-VAL-032`.** Penerimaan Rp 5.000.000 sudah dialokasikan Rp 3.000.000. Petugas
mencoba mengalokasikan Rp 2.500.000 lagi. Sistem menolak: "Nilai alokasi melebihi sisa uang pada
penerimaan ini. Sisa Rp 2.000.000."

## 5. Utang

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-040` | Satu faktur supplier hanya boleh diinput sekali | `POST /supplier-payables` | Pasangan supplier + nomor faktur sudah ada | "Faktur supplier ini sudah pernah diinput." | `409` |
| `FIN-VAL-041` | Rincian harus sama dengan nilai faktur | `POST /supplier-payables` | Jumlah item ≠ `OriginalAmount` | "Jumlah rincian Rp {jumlah} tidak sama dengan nilai faktur Rp {nilai}." | `422` |
| `FIN-VAL-042` | Satu fee dokter yang disetujui menghasilkan satu utang | `POST /doctor-payables/recognize` | `SourceDoctorServiceFeeId` sudah dipakai | "Jasa medis ini sudah pernah dicatat sebagai utang." | `409` |
| `FIN-VAL-043` | Hanya fee yang sudah disetujui yang boleh menjadi utang | `POST /doctor-payables/recognize` | Status fee belum disetujui | "Jasa medis ini belum disetujui, jadi belum dapat dibayarkan." | `422` |
| `FIN-VAL-044` | Utang yang sudah dibayar tidak boleh diubah | `PUT /supplier-payables/{id}` | Sudah ada pembayaran | "Utang yang sudah dibayar tidak dapat diubah. Gunakan koreksi." | `422` |
| `FIN-VAL-045` | Sisa utang tidak boleh negatif | Seluruh perubahan utang | Hasil perhitungan < 0 | "Sisa utang tidak boleh kurang dari nol." | `422` |
| `FIN-VAL-046` | Supplier harus aktif | `POST /supplier-payables` | `MstSupplier.IsActive` = false atau `IsBlacklisted` = true | "Supplier ini sedang tidak aktif." | `422` |

## 6. Pembayaran keluar

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-050` | Total pembayaran harus sama dengan rincian utang yang dilunasi | `POST /payments/{id}/submit` | `TotalAmount` ≠ jumlah alokasi | "Total pembayaran Rp {total} tidak sama dengan jumlah utang yang dipilih Rp {alokasi}." | `422` |
| `FIN-VAL-051` | Pengaju tidak boleh menyetujui pembayarannya sendiri | `POST /payments/{id}/approve` | `ApprovedBy` = `RequestedBy` | "Pengaju tidak boleh menyetujui pembayarannya sendiri." | `422` |
| `FIN-VAL-052` | Penyetuju harus sesuai jenjang nominal | `POST /payments/{id}/approve` | Hak penyetuju di bawah `ApprovalTier` | "Nominal pembayaran ini memerlukan persetujuan tingkat yang lebih tinggi." | `403` |
| `FIN-VAL-053` | Alokasi tidak boleh melebihi sisa utang | `POST /payments` | Alokasi ke satu utang > sisanya | "Nilai pembayaran melebihi sisa utang {nomor}. Sisa Rp {sisa}." | `422` |
| `FIN-VAL-054` | Satu jenis utang per baris alokasi | `POST /payments` | Baris alokasi menunjuk dua jenis utang, atau tidak menunjuk apa pun | "Setiap baris pembayaran harus menunjuk tepat satu utang." | `400` |
| `FIN-VAL-055` | Rekening sumber harus aktif | `POST /payments` | `MstBankAccount.IsActive` = false | "Rekening sumber pembayaran sedang tidak aktif." | `422` |
| `FIN-VAL-056` | Nomor bukti wajib saat menandai sudah dibayar | `POST /payments/{id}/mark-paid` | `ReferenceNumber` kosong | "Nomor bukti transfer wajib diisi." | `400` |
| `FIN-VAL-057` | Utang yang sama tidak boleh dibayar dua kali dalam satu pembayaran | `POST /payments` | Ada dua baris alokasi menunjuk utang yang sama | "Utang {nomor} muncul lebih dari sekali dalam pembayaran ini." | `400` |

**Contoh `FIN-VAL-050`.** Petugas menyusun pembayaran rekap dr. Andi senilai Rp 24.500.000 tetapi
hanya memilih dua dari tiga utang, totalnya Rp 21.500.000. Saat mengajukan, sistem menolak:
"Total pembayaran Rp 24.500.000 tidak sama dengan jumlah utang yang dipilih Rp 21.500.000."

## 7. Kas

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-060` | Setoran tidak boleh melebihi kas yang tersedia | `POST /bank-deposits/{id}/post` | Nominal > saldo tersedia | "Nominal setoran melebihi kas yang tersedia. Saldo tersedia saat ini Rp {saldo}." | `422` |
| `FIN-VAL-061` | Setoran sebagian diperbolehkan | `POST /bank-deposits/{id}/post` | Nominal < saldo tersedia | Tidak ada pesan — diterima (`FIN-DEC-017`) | — |
| `FIN-VAL-062` | Saldo tersedia dihitung backend, bukan dikirim layar | `POST /bank-deposits/{id}/post` | — | Tidak ada pesan — nilai dari layar diabaikan | — |
| `FIN-VAL-063` | Kas yang sudah ditutup tidak boleh berubah | Seluruh perubahan kas harian | Status tanggal itu `CLOSED` | "Kas tanggal ini sudah ditutup dan tidak dapat diubah." | `422` |
| `FIN-VAL-064` | Setoran belum diposting menahan penutupan hari | `POST /daily-cash/{cashDate}/close` | Masih ada setoran `DRAFT` tanggal itu | "Masih ada {jumlah} setoran yang belum diposting. Selesaikan dahulu sebelum menutup kas." | `422` |
| `FIN-VAL-065` | Kas kecil tidak ikut dihitung | Perhitungan kas harian | — | Tidak ada pesan — pergerakan kas kecil diabaikan (`FIN-DEC-020`) | — |
| `FIN-VAL-066` | Saldo awal mengikuti penutupan hari sebelumnya | Pembukaan hari | Saldo awal ≠ saldo akhir kemarin | "Saldo awal tidak sesuai dengan saldo akhir hari sebelumnya." | `422` |

## 8. Kejadian ke Accounting

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-070` | Hanya rupiah yang boleh dikirim | Penulisan kejadian | `CurrencyCode` ≠ `IDR` | "Hanya transaksi rupiah yang dapat dikirim ke Accounting." | `422` |
| `FIN-VAL-071` | Data pasien tidak boleh ikut | Penulisan kejadian | Payload memuat `PatientId`, nomor rekam medis, atau `DoctorId` | "Data pasien tidak boleh dikirim ke Accounting." | `422` |
| `FIN-VAL-072` | Nomor kejadian unik | Penulisan kejadian | `EventNumber` sudah dipakai | "Kejadian dengan nomor ini sudah ada." | `409` |
| `FIN-VAL-073` | Identitas sumber unik | Penulisan kejadian | Kombinasi modul, transaksi, jenis, dan versi sudah ada | "Kejadian untuk transaksi ini sudah pernah dibuat." | `409` |
| `FIN-VAL-074` | Koreksi menaikkan versi sumber | Penulisan kejadian koreksi | `SourceVersion` sama dengan yang sudah terkirim | "Koreksi harus memakai versi baru, bukan versi yang sama." | `422` |
| `FIN-VAL-075` | Jenis kejadian harus terdaftar | Penulisan kejadian | `EventTypeCode` di luar **24** kode yang disepakati (diperbarui revisi 1.1) | "Jenis kejadian ini belum terdaftar dalam kesepakatan dengan Accounting." | `422` |
| ~~`FIN-VAL-076`~~ | ~~Kejadian tertahan tidak dikirim~~ | ~~Worker pengiriman~~ | **DICABUT revisi 1.1** — `FIN-DEC-004` `superseded` oleh `FIN-DEC-030`, tidak ada lagi baris baru berstatus `HELD_FOR_FINALIZATION`. Untuk baris warisan, lihat `FIN-VAL-078` | — | — |
| `FIN-VAL-077` | Balasan `422` tidak memicu kejadian baru | Penanganan balasan | Accounting membalas `422` | Tidak ada pesan — status menjadi `HELD` | — |
| `FIN-VAL-078` | Baris warisan tertahan tidak dikirim apa adanya | Worker pengiriman | `DeliveryStatus` = `HELD_FOR_FINALIZATION` (hanya mungkin untuk baris sebelum `FIN-DEC-030`) | Tidak ada pesan bagi pengguna — baris dilewati dan **dilaporkan** sebagai butir yang perlu dibetulkan, bukan didiamkan (`02-backend-architecture.md` bagian B.6) | — |
| `FIN-VAL-079` | Nilai kejadian harus lebih dari nol, **kecuali** dua jenis | Penulisan kejadian | `Amount` ≤ 0 untuk `EventTypeCode` selain `SELISIH-KAS-SHIFT` dan `SALDO-SUBLEDGER` | "Nominal kejadian harus lebih dari nol." | `400` |
| `FIN-VAL-080` | Selisih kas boleh negatif, tetapi tidak boleh nol | Penulisan kejadian `SELISIH-KAS-SHIFT` | `Amount` = 0 | "Tidak ada selisih kas yang perlu dikirim ke Accounting." | `400` |
| `FIN-VAL-081` | Pesan saldo wajib membawa rincian saldonya | Penulisan kejadian `SALDO-SUBLEDGER` | `SubledgerBalance` kosong, atau `AccountingPeriodCode`/`ControlAccountCode` kosong | "Pesan saldo subledger wajib menyebutkan periode dan akun kontrolnya." | `400` |
| `FIN-VAL-082` | Periode saldo memakai bentuk tahun-bulan | Penulisan kejadian `SALDO-SUBLEDGER` | `AccountingPeriodCode` tidak berbentuk `YYYY-MM`, atau lebih dari 7 karakter | "Kode periode harus berbentuk tahun-bulan, contoh 2026-11." | `400` |
| `FIN-VAL-083` | Versi pesan saldo wajib bilangan bulat positif yang naik | Penulisan kejadian `SALDO-SUBLEDGER` | `SourceVersion` bukan bilangan bulat positif, atau tidak lebih besar dari versi terakhir untuk periode dan akun kontrol yang sama | "Pernyataan ulang saldo harus memakai versi yang lebih baru." | `422` |
| `FIN-VAL-084` | Rincian saldo hanya untuk pesan saldo | Penulisan kejadian | `SubledgerBalance` terisi padahal `EventTypeCode` bukan `SALDO-SUBLEDGER` | "Rincian saldo subledger hanya berlaku untuk pesan saldo." | `400` |
| `FIN-VAL-085` | Kode pembalikan mengikuti penerimaan aslinya | Penulisan kejadian pembalikan penerimaan | Kode pembalikan tidak cocok dengan `EventTypeCode` penerimaan asli — `PEMBALIKAN-PENERIMAAN-KASIR` untuk penerimaan `PENERIMAAN-UANG-MUKA`, atau sebaliknya | "Pembalikan harus memakai jenis kejadian yang sepasang dengan penerimaan aslinya." | `422` |
| `FIN-VAL-086` | Selisih kas hanya dikirim setelah disahkan | Penulisan kejadian `SELISIH-KAS-SHIFT` | Shift sumbernya belum berstatus `REVIEWED` | "Selisih kas baru dapat dikirim setelah disahkan penyelia." | `422` |

**Contoh `FIN-VAL-079` dan `FIN-VAL-080`.** Shift kasir ditutup dengan kas fisik Rp 30.000
**lebih kecil** dari catatan sistem. Nilai kejadiannya `-30000.00` — negatif, dan itu sah karena
jenisnya `SELISIH-KAS-SHIFT`. Bila shift lain ditutup tanpa selisih sama sekali (`0`), tidak ada
kejadian yang dikirim: buku besar tidak perlu jurnal untuk sesuatu yang nilainya nol.

**Contoh `FIN-VAL-085`.** Pasien membayar Rp 5.000.000 saat tagihan masih `OPEN`, sehingga terbit
`PENERIMAAN-UANG-MUKA`. Tiga hari kemudian tagihannya difinalisasi, lalu tendernya dibatalkan
Billing. Pembalikannya **tetap** `PEMBALIKAN-PENERIMAAN-UANG-MUKA`, mengikuti penerimaan aslinya
— bukan `PEMBALIKAN-PENERIMAAN-KASIR` walaupun tagihannya sekarang sudah `FINAL`. Memakai kode
kasir akan mendebit akun pendapatan/piutang yang tidak pernah dikredit saat penerimaan.

**Contoh `FIN-VAL-074`.** Piutang `AR-2026-09-00871` sudah dikirim sebagai kejadian versi `1`.
Kemudian nilainya dikoreksi. Kalau sistem mengirim ulang dengan versi `1`, Accounting
membacanya sebagai kiriman ulang dan **tidak menjurnal koreksinya**. Karena itu Finance wajib
menaikkan versi menjadi `2`.

## 9. Aturan yang berlaku di seluruh modul

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-080` | Nominal uang maksimal 16 digit dengan 2 desimal | Seluruh isian uang | Melebihi `9999999999999999.99` | "Nominal melebihi batas yang didukung sistem." | `400` |
| `FIN-VAL-081` | Nominal uang harus lebih dari nol | Seluruh isian uang | ≤ 0 | "Nominal harus lebih dari nol." | `400` |
| `FIN-VAL-082` | Versi data wajib dikirim saat mengubah | Seluruh perintah pengubah | `ExpectedRowVersion` kosong | "Versi data wajib disertakan." | `400` |
| `FIN-VAL-083` | Data yang sudah diubah orang lain tidak ditimpa | Seluruh perintah pengubah | `ExpectedRowVersion` tidak cocok | "Data telah berubah. Muat ulang sebelum melanjutkan." | `409` |
| `FIN-VAL-084` | Kunci pengulangan wajib untuk perintah yang memindahkan uang | Perintah uang | Header `Idempotency-Key` kosong | "Kunci pengulangan wajib disertakan." | `400` |
| `FIN-VAL-085` | Kiriman ulang mengembalikan hasil yang sama | Perintah uang | `Idempotency-Key` sudah pernah dipakai | Tidak ada pesan galat — hasil sebelumnya dikembalikan | `200` |
| `FIN-VAL-086` | Penghapusan bersifat penandaan | Seluruh penghapusan | — | Tidak ada pesan — baris ditandai `IsDelete`, tidak dihapus | — |

`FIN-VAL-085` penting dibaca bersama `FIN-VAL-084`: bila petugas menekan tombol Simpan dua kali
karena jaringan lambat, permintaan kedua **tidak** membuat catatan kedua. Sistem mengembalikan
hasil yang pertama.

---

# AMENDMENT REVISI 2

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-0.2` — status `locked` 20 September 2026 |
| Tanggal | 20 September 2026 |
| Keputusan | `FIN-DES-025`..`028` |

## A.1 Aturan yang diganti namanya

`FIN-VAL-042` dan `FIN-VAL-043` pada bagian 5 tetap berlaku isinya; yang berubah hanya
entitasnya — `FinDoctorPayable` menjadi `FinMedicalServicePayable`, dan "fee dokter" menjadi
"hasil jasa tenaga medis".

## A.2 Aturan baru — potongan pembayaran

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-090` | Nilai transfer tidak boleh negatif | `POST /payments/{id}/submit` | Potongan melebihi jumlah jasa yang dibayarkan | "Total potongan Rp {potongan} melebihi jasa yang dibayarkan Rp {total}. Periksa kembali rincian potongan." | `422` |
| `FIN-VAL-091` | Nilai transfer harus lebih dari nol | `POST /payments/{id}/submit` | `NetTransferAmount` = 0 | "Seluruh jasa habis oleh potongan, sehingga tidak ada uang yang perlu ditransfer. Selesaikan lewat koreksi utang, bukan pembayaran." | `422` |
| `FIN-VAL-092` | Nominal potongan harus lebih dari nol | `POST /payments/{id}/deductions` | `Amount` ≤ 0 | "Nominal potongan harus lebih dari nol." | `400` |
| `FIN-VAL-093` | Pos lain-lain wajib menyebut alasan | `POST /payments/{id}/deductions` | `DeductionType` = `OTHER` dan `Reason` kosong | "Potongan jenis lain-lain wajib menyebutkan alasannya." | `400` |
| `FIN-VAL-094` | Potongan hanya dapat diubah selama masih draf | `POST`/`DELETE .../deductions` | Status pembayaran bukan `DRAFT` | "Pembayaran ini sudah diajukan, sehingga potongannya tidak dapat diubah. Batalkan pembayaran lalu susun yang baru." | `422` |
| `FIN-VAL-095` | Potongan tidak mengurangi sisa utang | Penandaan pembayaran sudah dibayar | — | Tidak ada pesan — utang berkurang sebesar alokasinya, bukan sebesar transfer (`FIN-DES-028`) | — |
| `FIN-VAL-096` | Jenis penerima harus cocok dengan jenis pembayaran | `POST /payments` | `PaymentType` = `MEDICAL_SERVICE` tetapi alokasinya menunjuk utang supplier | "Jenis pembayaran tidak cocok dengan utang yang dipilih." | `400` |

**Contoh `FIN-VAL-090`.** Jasa dr. Andi bulan ini Rp 3.000.000, tetapi kasbonnya Rp 5.000.000.
Petugas menyusun pembayaran dengan potongan penuh. Sistem menolak: "Total potongan Rp 5.000.000
melebihi jasa yang dibayarkan Rp 3.000.000. Periksa kembali rincian potongan." Sisa kasbon yang
belum tertutup diselesaikan pada periode berikutnya — bukan dengan memaksakan nilai transfer
negatif.

**Contoh `FIN-VAL-095`.** Jasa Rp 24.500.000, potongan Rp 4.500.000, transfer Rp 20.000.000.
Setelah ditandai dibayar, sisa utang jasa dr. Andi menjadi **nol** — bukan Rp 4.500.000. Kalau
sisanya Rp 4.500.000, sistem akan menganggap rumah sakit masih berutang padahal kewajibannya
sudah selesai.


---

# AMENDMENT REVISI 4

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-1.2` — status `locked` 25 September 2026 (disetujui Yasmin bersama `FIN-DES-037`..`044`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-045`..`055` |

## B.1 Purchase Order dan Purchasing Invoice

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-100` | PO wajib minimal satu baris item | `POST /purchasing/purchase-orders` | Tidak ada baris item | "Purchase Order wajib memiliki minimal satu baris item." | `400` |
| `FIN-VAL-101` | Pengaju tidak boleh menyetujui PO sendiri | `POST .../purchase-orders/{id}/approve` | `ApprovedByUserId` = `RequestedByUserId` | "Pengaju tidak boleh menyetujui Purchase Order permohonannya sendiri." | `422` |
| `FIN-VAL-102` | Penyetuju PO harus sesuai jenjang nominal | `POST .../purchase-orders/{id}/approve` | Hak penyetuju di bawah `ApprovalTier` (ambang Rp 50.000.000, `FIN-DEC-052`) | "Nominal Purchase Order ini memerlukan persetujuan Manajer Finance." | `403` |
| `FIN-VAL-103` | PO yang sudah ada penerimaan barang tidak boleh dibatalkan | `POST .../purchase-orders/{id}/cancel` | Sudah ada `FinGoodsReceipt` | "Purchase Order ini sudah memiliki penerimaan barang dan tidak dapat dibatalkan." | `422` |
| `FIN-VAL-104` | Kuantitas diterima tidak boleh melebihi sisa PO | `POST /purchasing/goods-receipts` | `ReceivedQuantity` (akumulasi) > `Quantity` baris PO | "Kuantitas diterima melebihi kuantitas yang dipesan pada baris ini." | `422` |
| `FIN-VAL-105` | Satu Tukar Faktur menghasilkan tepat satu Purchasing Invoice | `POST /purchasing/purchasing-invoices` | `InvoiceExchangeId` sudah dipakai invoice lain | "Tukar Faktur ini sudah memiliki Purchasing Invoice." | `409` |
| `FIN-VAL-106` | Tukar Faktur harus siap sebelum dijadikan invoice | `POST /purchasing/purchasing-invoices` | Status Tukar Faktur bukan `RECEIVED` | "Tukar Faktur ini sudah dibatalkan atau sudah punya invoice." | `422` |
| `FIN-VAL-107` | Rincian nilai invoice harus seimbang | `POST`/`PUT .../purchasing-invoices` | `TotalAmount` ≠ `Subtotal - Discount + PPN - DownPayment - OtherDeduction` | "Rincian nilai invoice tidak seimbang dengan totalnya." | `422` |
| `FIN-VAL-108` | Pengaju tidak boleh menyetujui Purchasing Invoice sendiri | `POST .../purchasing-invoices/{id}/approve` | `ApprovedByUserId` = `RequestedByUserId` | "Pengaju tidak boleh menyetujui Purchasing Invoice permohonannya sendiri." | `422` |
| `FIN-VAL-109` | Penyetuju Purchasing Invoice harus sesuai jenjang nominal | `POST .../purchasing-invoices/{id}/approve` | Hak penyetuju di bawah `ApprovalTier` | "Nominal Purchasing Invoice ini memerlukan persetujuan Manajer Finance." | `403` |

**Contoh `FIN-VAL-102`/`FIN-VAL-109`.** PO senilai Rp 62.000.000 diajukan Petugas AP. Supervisor
Finance mencoba menyetujui — sistem menolak `403` karena nominalnya di atas ambang Rp 50.000.000
(`FIN-DEC-052`); hanya Manajer Finance yang berhak. Ambang ini **sama persis** dengan ambang
persetujuan pembayaran (`FIN-VAL-052`), tetapi keduanya tetap checkpoint terpisah — menyetujui
PO tidak ikut menyetujui pembayaran yang lahir darinya nanti.

## B.2 Retur Pembelian dan Deposit Retur

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-110` | Retur hanya atas invoice yang sudah disetujui | `POST /purchasing/supplier-returns` | Status `FinPurchasingInvoice` bukan `APPROVED` | "Retur hanya dapat diajukan atas Purchasing Invoice yang sudah disetujui." | `422` |
| `FIN-VAL-111` | Nilai retur tidak boleh melebihi nilai invoice sumber | `POST /purchasing/supplier-returns` | `TotalAmount` retur > `TotalAmount` invoice | "Nilai retur melebihi nilai Purchasing Invoice sumber." | `422` |
| `FIN-VAL-112` | Pemakaian Deposit Retur tidak boleh melebihi saldo tersedia | `POST .../deposits/{id}/apply` | `UsedAmount` > `AvailableAmount` | "Nilai pemakaian melebihi saldo Deposit Retur yang tersedia. Saldo saat ini Rp {saldo}." | `422` |
| `FIN-VAL-113` | Deposit Retur hanya dipakai untuk supplier yang sama | `POST .../deposits/{id}/apply` | `SupplierId` invoice tujuan ≠ `SupplierId` deposit | "Deposit Retur ini hanya dapat dipakai untuk supplier yang sama." | `422` |

## B.3 Batch Tagihan AR

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-114` | Piutang yang sudah tergabung batch aktif tidak boleh digabung lagi | `POST /receivable-invoice-batches` | `ReceivableId` sudah ada di batch yang belum `CANCELLED` | "Piutang {nomor} sudah tergabung dalam batch tagihan lain." | `409` |
| `FIN-VAL-115` | Batch hanya boleh berisi piutang penjamin yang sama | `POST /receivable-invoice-batches` | Ada anggota dengan `DebtorReferenceId` berbeda | "Seluruh piutang dalam satu batch harus milik penjamin yang sama." | `400` |
| `FIN-VAL-116` | Batch kosong tidak boleh diterbitkan | `POST .../receivable-invoice-batches/{id}/issue` | Jumlah anggota = 0 | "Batch tidak memiliki anggota piutang untuk diterbitkan." | `422` |
| `FIN-VAL-117` | Anggota batch yang sudah diterbitkan tidak dapat diubah | Penambahan/pengurangan anggota | Status batch bukan `DRAFT` | "Batch ini sudah diterbitkan dan anggotanya tidak dapat diubah." | `422` |

## B.4 Potongan penerimaan (AR)

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-118` | Nominal potongan harus lebih dari nol | `POST /receipts/{id}/deductions` | `Amount` ≤ 0 | "Nominal potongan harus lebih dari nol." | `400` |
| `FIN-VAL-119` | Pos lain-lain wajib menyebut alasan | `POST /receipts/{id}/deductions` | `DeductionType` = `OTHER` dan `Reason` kosong | "Potongan jenis lain-lain wajib menyebutkan alasannya." | `400` |
| `FIN-VAL-120` | Potongan hanya dapat dicatat sebelum alokasi final | `POST /receipts/{id}/deductions` | Status `FinReceipt` = `ALLOCATED`, `RECONCILED`, atau `REVERSED` | "Penerimaan ini sudah selesai dialokasikan, potongan tidak dapat ditambah." | `422` |
| `FIN-VAL-121` | Potongan mengurangi sisa piutang, bukan sisa penerimaan | Alokasi penerimaan dengan potongan | — | Tidak ada pesan — `FinReceiptDeduction.Amount` ikut mengurangi `FinReceivable.OutstandingAmount` lewat `AllocatedAmount`, kebalikan `FIN-VAL-095` (`FIN-DEC-055`) | — |

## B.5 Gerbang PPN Masukan Pembelian

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-122` | Kejadian PPN Masukan tidak dikirim sebelum ratifikasi Accounting | Worker pengiriman | `EventTypeCode = PPN-MASUKAN-PEMBELIAN` dan Accounting belum meratifikasi kode ini (`FIN-OQ-020`) | Tidak ada pesan bagi pengguna — worker melewati baris ini, tetap `PENDING` | — |

`FIN-VAL-122` **bukan** aturan yang menolak permintaan pengguna — Purchasing Invoice tetap bisa
disetujui dan baris outbox tetap ditulis. Yang dicegah hanyalah pengiriman baris itu ke
Accounting, persis pola `FIN-DES-029` untuk kode yang pernah menunggu ratifikasi sebelumnya.

---

# AMENDMENT REVISI 5

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-1.3` — status `locked` 26 September 2026 (disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| Tanggal | 25 September 2026 |
| Keputusan | `FIN-DEC-057`, `058`, `061`, `062`; `FIN-DES-045`..`050` |

## C.1 Aturan lama yang berubah

| ID | Perubahan |
|---|---|
| `FIN-VAL-091` | Berlaku **hanya** bila `DepositAppliedAmount = 0`. Pembayaran yang seluruhnya dilunasi deposit sah dengan `NetTransferAmount = 0` |
| `FIN-VAL-056` | Nomor bukti transfer wajib **hanya** bila `NetTransferAmount > 0` |
| `FIN-VAL-112`, `113` | Dipindah dari `POST .../deposits/{id}/apply` (dicabut) ke `POST /payments/{id}/return-deposits`. Isi aturannya tidak berubah |
| `FIN-VAL-118`, `119` | Dipindah dari `POST /receipts/{id}/deductions` (dicabut) ke `POST /receipts/{id}/allocations`, per potongan |
| `FIN-VAL-120` | **Dicabut.** Tidak lagi relevan: potongan dicatat bersama alokasinya, sehingga tidak ada lagi "menambah potongan sesudah alokasi" |
| `FIN-VAL-033` | Diperluas: **uang alokasi + seluruh potongan** pada satu baris ≤ sisa piutang |

## C.2 Aturan baru — Deposit Retur sebagai sumber dana

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-123` | Deposit hanya untuk pembayaran supplier | `POST /payments/{id}/return-deposits` | `PaymentType` bukan `SUPPLIER` | "Deposit Retur hanya dapat dipakai pada pembayaran supplier." | `422` |
| `FIN-VAL-124` | Deposit hanya ditambah/dilepas selama draf | `POST`/`DELETE .../return-deposits` | Status pembayaran bukan `DRAFT` | "Pembayaran ini sudah diajukan, sehingga sumber dananya tidak dapat diubah." | `422` |
| `FIN-VAL-125` | Deposit yang sama tidak dua kali dalam satu pembayaran | `POST .../return-deposits` | Sudah ada baris aktif untuk deposit itu | "Deposit Retur ini sudah dipakai di pembayaran ini." | `409` |
| `FIN-VAL-126` | Nilai transfer tidak boleh negatif karena deposit | `POST .../return-deposits` | `DepositAppliedAmount` sesudah ditambah > `TotalAmount − DeductionAmount + AdditionAmount` | "Nilai deposit melebihi yang perlu dibayar. Maksimum Rp {sisa}." | `422` |
| `FIN-VAL-127` | Deposit yang dibatalkan tidak dapat dipakai | `POST .../return-deposits` | Status deposit `CANCELLED` | "Deposit Retur ini sudah dibatalkan." | `422` |

## C.3 Aturan baru — Potongan AR dan kejadiannya

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-128` | Potongan hanya pada alokasi ke piutang | `POST /receipts/{id}/allocations` | Baris ber-`targetType = INVOICE_DIRECT` membawa potongan | "Potongan hanya dapat dicatat pada pelunasan piutang." | `400` |
| `FIN-VAL-129` | Potongan hanya melekat pada alokasi asli, bukan baris pembalik | Service | Alokasi `IsReversal = true` | Tidak ada pesan — ditolak sebagai kesalahan internal | `422` |
| `FIN-VAL-130` | Kode kejadian potongan AR tidak pernah kode kas | Penulisan kejadian | Potongan menulis `AR_PAYMENT`, `PENERIMAAN-PIUTANG`, atau `PENYESUAIAN-PIUTANG` | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-131` | Kejadian pembayaran utang tidak memuat porsi deposit | Penulisan kejadian | `AP_PAYMENT.Amount` ≠ `TotalAmount − DepositAppliedAmount`, atau `AP_PAYMENT` tertulis dengan nilai nol | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-132` | Empat kode baru tidak dikirim sebelum ratifikasi | Worker pengiriman | `EventTypeCode` salah satu dari `POTONGAN-PIUTANG-NON-TUNAI`, `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI`, `RETUR-PEMBELIAN`, `PEMAKAIAN-DEPOSIT-RETUR` dan `FIN-OQ-026` belum turun | Tidak ada pesan — baris dilewati, tetap `PENDING` (pola `FIN-VAL-122`) | — |

**Contoh `FIN-VAL-126`.** Pembayaran dua faktur Rp 10.000.000 dengan potongan Rp 0. Petugas
menambahkan deposit Rp 12.000.000. Ditolak: "Nilai deposit melebihi yang perlu dibayar. Maksimum
Rp 10.000.000." Saldo deposit tidak berubah.

---

# D. AMENDMENT REVISI 6 — Aturan baru dan tiga aturan yang diperbarui

Menurunkan `FIN-DES-051`..`058` (`02-backend-architecture.md` AMENDMENT REVISI 6) dan
`FIN-DEC-063`..`071`.

## D.1 Tiga aturan yang diperbarui

Ketiganya merujuk nama kode yang **sudah tidak ada lagi** sesudah ratifikasi Accounting
(`integration-contract.md` bagian 5.10.2). Isinya tidak berubah artinya; hanya nama kodenya.

| ID | Yang diperbarui |
|---|---|
| `FIN-VAL-130` | Kode kejadian potongan AR tidak pernah kode kas — nama kode yang benar sekarang `POTONGAN-PPH23-PIUTANG` atau `POTONGAN-BIAYA-BANK-PIUTANG`, bukan `POTONGAN-PIUTANG-NON-TUNAI` |
| `FIN-VAL-131` | Kejadian pembayaran utang — kodenya `PEMBAYARAN-HUTANG-SUPPLIER`, bukan `AP_PAYMENT` |
| `FIN-VAL-132` | Daftar kode yang tertahan ratifikasi menjadi: `POTONGAN-PPH23-PIUTANG`, `PEMBALIKAN-POTONGAN-PPH23-PIUTANG`, `POTONGAN-BIAYA-BANK-PIUTANG`, `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG`, `RETUR-PEMBELIAN`, `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`, `PPN-MASUKAN-RETUR-PEMBELIAN`, `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, `PENUTUPAN-SHIFT-KASIR`, `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, `SELISIH-KAS-KURANG`, `SELISIH-KAS-LEBIH` — masing-masing tertahan open question-nya sendiri (`FIN-OQ-027`..`032`) |

## D.2 Aturan baru

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-133` | Nilai PPN retur tidak boleh negatif | `POST /purchasing/supplier-returns`, `PUT .../{id}` | `ppnAmount < 0` | "Nilai PPN retur tidak boleh kurang dari nol." | `400` |
| `FIN-VAL-134` | Nilai pokok retur adalah jumlah barisnya, tanpa PPN | Service konfirmasi retur | `TotalAmount` ≠ jumlah `LineTotal` barisnya | "Nilai retur tidak sama dengan jumlah rincian barangnya." | `422` |
| `FIN-VAL-135` | Kredit retur yang lahir mencakup PPN | Service konfirmasi retur | `AvailableAmount` awal ≠ `TotalAmount + PPNAmount` | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-136` | Kejadian retur bernilai pokok saja | Penulisan kejadian | `RETUR-PEMBELIAN.Amount` ≠ `TotalAmount` | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-137` | Potongan piutang berjenis lain-lain belum dapat dicatat | `POST /receipts/{id}/allocations` | Baris potongan ber-`deductionType = OTHER` | "Jenis potongan ini belum dapat dicatat karena perlakuan akuntansinya belum ditetapkan. Pakai PPh 23 atau biaya administrasi bank, atau hubungi bagian akuntansi." | `400` |
| `FIN-VAL-138` | Nilai nol hanya untuk kode penanda | `FinanceAccountingOutboxService.ValidateRequest` | `Amount = 0` untuk kode **di luar** daftar tertutup kode penanda | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-139` | Pesan tanpa komponen tidak memuat properti komponen | Penyusunan `PayloadJson` | `PayloadJson` memuat properti `Components` padahal tidak ada komponen | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-140` | Satu shift satu kejadian selisih kas per siklus tutup | Penulisan kejadian | Kejadian `SELISIH-KAS-*` kedua untuk `BilCashierShift.Id` yang sama dalam siklus yang sama | Tidak ada pesan — ditolak unique index outbox | `409` |
| `FIN-VAL-141` | Refund kredit yang perlakuan akuntansinya belum ditetapkan tidak diterbitkan, tetapi tercatat | Sinkronisasi intake | `BilRefundCase` `EXECUTED` atas kredit ber-`SourceType = REFERRED_OUTPATIENT_ADMIN` | Tidak ada pesan bagi pengguna akhir; baris intake menjadi `ERROR` dengan sebab yang terbaca di layar pantauan | — |
| `FIN-VAL-142` | Pembalikan tender top-up deposit tanpa mutasi pembalik ditandai, bukan didiamkan | Sinkronisasi intake | `BilTender` `REVERSED` bertujuan top-up deposit, tanpa mutasi deposit pembalik yang bersesuaian | Tidak ada pesan bagi pengguna akhir; baris intake menjadi `ERROR` menunjuk `FIN-OQ-034` | — |
| `FIN-VAL-143` | Nilai retur beserta PPN-nya tidak melebihi nilai faktur yang diretur | `POST /purchasing/supplier-returns`, `PUT .../{id}` | `TotalAmount + PPNAmount` > `FinPurchasingInvoice.TotalAmount` | "Nilai retur beserta PPN-nya melebihi nilai faktur pembelian ini." | `400` |

## D.3 Contoh dua aturan yang paling mudah salah

**Contoh `FIN-VAL-137`.** Penjamin membayar piutang Rp 10.000.000 dan memotong Rp 15.000 dengan
keterangan "biaya materai". Petugas memilih jenis potongan "lain-lain". Ditolak: *"Jenis potongan
ini belum dapat dicatat karena perlakuan akuntansinya belum ditetapkan. Pakai PPh 23 atau biaya
administrasi bank, atau hubungi bagian akuntansi."* Alokasi **tidak** tersimpan sebagian — seluruh
permintaan ditolak, sehingga tidak ada piutang yang berkurang tanpa kejadian pendampingnya.

**Contoh `FIN-VAL-140`.** Shift 20 November 2026 kurang Rp 30.000. Pengesahan pertama memilih
"perlu tindak lanjut" — **tidak ada kejadian**. Pengesahan kedua menyelesaikannya: terbit satu
`SELISIH-KAS-KURANG` Rp 30.000. Bila kode mencoba menerbitkannya lagi dari baris pengesahan
pertama, database menolak lewat unique index `(SourceModule, SourceTransactionId, EventTypeCode,
SourceVersion)` — bukan bergantung pada kebenaran logika pemanggil.

---

# E. AMENDMENT REVISI 9 — arti tunggal mutasi `RELEASE`

Menurunkan `FIN-DES-064` dan `FIN-DES-065` via `FIN-DEC-080` dan `FIN-DEC-081`. Ketiganya berstatus **`approved`** (29 September 2026).

| ID | Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|---|
| `FIN-VAL-144` | Pembatalan alokasi uang muka tidak pernah dibukukan sebagai kas keluar | Penulisan kejadian | Mutasi `BilDepositMovement` `RELEASE` menerbitkan `PENGEMBALIAN-UANG-MUKA` | Tidak ada pesan — kesalahan kode | — |
| `FIN-VAL-145` | Mutasi pelepasan yang asalnya belum dikenal tidak diterbitkan, tetapi tercatat | Sinkronisasi intake | Mutasi `RELEASE` **tanpa** mutasi `REVERSAL` ber-`SettlementId` sama | Tidak ada pesan bagi pengguna akhir; baris intake menjadi `ERROR` dengan sebab yang menyebut jenis mutasinya dan menunjuk `FIN-OQ-037` | — |
| `FIN-VAL-146` | Pembalikan tender top-up yang dananya sudah terpakai menerbitkan **dua** kejadian, bukan satu | Penulisan kejadian | Mutasi `RELEASE` dan `REVERSAL` lahir dari satu `SettlementId`, tetapi hanya satu kejadian yang terbit | Tidak ada pesan — kesalahan kode | — |

**Contoh `FIN-VAL-144`, dan kenapa ia aturan yang paling mahal bila dilanggar.** Pasien menitip
uang muka Rp 20.000.000 lewat kartu, uang muka itu dipakai melunasi tagihan Rp 32.000.000, lalu
kartunya ditarik penerbit. Billing membatalkan alokasi tagihan dan menulis mutasi `RELEASE`
Rp 20.000.000, lalu menarik top-up-nya dengan mutasi `REVERSAL` Rp 20.000.000.

| Yang diterbitkan | Hasil di buku besar |
|---|---|
| **Benar** — `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` + `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Debit Piutang Rp 20.000.000, kredit Kas Rp 20.000.000. Tagihan terbuka kembali, kas berkurang sebesar uang yang tidak pernah jadi diterima |
| **Salah** — `PENGEMBALIAN-UANG-MUKA` + `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Kas dikredit **dua kali** Rp 40.000.000 untuk uang Rp 20.000.000, dan piutang Rp 20.000.000 tidak pernah terbuka kembali |

Jurnal versi salah itu **tetap seimbang**, sehingga tidak tertangkap pemeriksaan neraca. Ia baru
terlihat saat rekonsiliasi kas toleransi nol Accounting (`ACC-DEC-076`) gagal — berbulan kemudian,
tanpa petunjuk sebabnya.

**Contoh `FIN-VAL-145`.** Bila kelak Billing menambah fitur pengembalian uang muka tunai dan
memakai mutasi `RELEASE` untuknya, mutasi itu datang **tanpa** pasangan `REVERSAL`. Finance tidak
menebak: baris intake ditandai `ERROR` dan muncul di layar pantauan, sehingga lubangnya terlihat
pada hari pertama alih-alih menjadi kas keluar palsu di buku besar.


# AMENDMENT REVISI 13 — Aturan pelacakan klaim penjamin

`last_changed_in`: `FIN-VAL-1.5` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-097`, `FIN-DEC-098`; dirancang `FIN-DES-070`, `FIN-DES-071`.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-147` | `POST /receivable-invoice-batches/{id}/claim/verify`, `/claim/approve`, `/claim/close` | Batch belum pernah diterbitkan ke penjamin (`ClaimStatus` masih kosong dan `Status` belum `ISSUED`) | "Tagihan ini belum diterbitkan ke penjamin, jadi belum ada klaim yang bisa ditindaklanjuti. Terbitkan tagihannya lebih dulu." | `422` |
| `FIN-VAL-148` | `POST /claim/approve` | `ApprovedAmount` tidak diisi | "Nominal yang disetujui penjamin wajib diisi." | `400` |
| `FIN-VAL-149` | `POST /claim/approve` | `ApprovedAmount` bernilai minus | "Nominal yang disetujui tidak boleh kurang dari nol." | `400` |
| `FIN-VAL-150` | `POST /claim/approve` | `ApprovedAmount` melebihi `TotalAmount` batch | "Nominal yang disetujui penjamin tidak boleh melebihi total tagihan." | `422` |
| `FIN-VAL-151` | `POST /claim/approve` | `ApprovedAmount` lebih kecil dari `TotalAmount` tetapi `ClaimNote` kosong | "Karena penjamin menyetujui lebih kecil dari tagihan, tuliskan alasannya supaya selisihnya bisa ditindaklanjuti." | `400` |
| `FIN-VAL-152` | Ketiga aksi klaim | Perpindahan status tidak sah menurut `state-transition-matrix.md` `D.1` — termasuk upaya memundurkan status dan mengubah klaim yang sudah `CLOSED` | "Langkah ini tidak bisa dilakukan dari status klaim saat ini." | `422` |
| `FIN-VAL-153` | Ketiga aksi klaim | `ExpectedRowVersion` tidak cocok dengan data terkini | "Data tagihan ini sudah diubah pengguna lain. Muat ulang halaman sebelum melanjutkan." | `409` |

## Yang sengaja **tidak** divalidasi

| Yang tidak dilarang | Alasan |
|---|---|
| `ApprovedAmount` bernilai nol | Penjamin menolak klaim sepenuhnya adalah jawaban yang sah. Seluruh nilai tagihan menjadi selisih yang menunggu write-off |
| Menutup klaim walaupun uang belum masuk sama sekali | Klaim yang ditolak penuh tetap perlu ditutup. Pelunasan ada pada sumbu lain (`B.7`) |
| Mencatat ulang persetujuan pada klaim yang sudah `APPROVED` | Penjamin dapat merevisi keputusannya. Nilai lama tertimpa; perubahannya terbaca pada kolom audit |
| Selisih yang belum di-write-off menahan penutupan klaim | `FIN-DEC-097` menempatkan write-off sebagai pekerjaan terpisah milik petugas, bukan syarat penutupan klaim. Menahan penutupan akan memaksa petugas menghapus piutang hanya agar klaimnya bisa ditutup |

# AMENDMENT REVISI 13 (lanjutan) — Aturan piutang sewa non-pasien

`last_changed_in`: `FIN-VAL-1.6` — status `draft`, 1 Oktober 2026.
Diturunkan dari `FIN-DEC-100`..`FIN-DEC-104`; dirancang `FIN-DES-074`..`FIN-DES-077`.

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-154` | `POST /non-patient-receivables` | `Category` bukan `PARKING` atau `TENANT` | "Jenis sewa hanya boleh Parkir atau Tenant." | `400` |
| `FIN-VAL-155` | `POST`, `PUT /non-patient-receivables` | `BilledAmount` bernilai nol atau minus | "Nominal tagihan harus lebih besar dari nol." | `400` |
| `FIN-VAL-156` | `POST`, `PUT /non-patient-receivables` | `LateFeeAmount` bernilai minus | "Nominal denda tidak boleh kurang dari nol." | `400` |
| `FIN-VAL-157` | `POST`, `PUT /non-patient-receivables` | `PeriodEnd` lebih awal dari `PeriodStart`, atau `DueDate` lebih awal dari `PeriodStart` | "Periode tagihan dan tanggal jatuh tempo tidak masuk akal. Periksa kembali tanggalnya." | `400` |
| `FIN-VAL-158` | `POST /{id}/write-off`, `POST /{id}/cancel` | `Reason` kosong | "Tuliskan alasannya — penghapusan dan pembatalan tagihan sewa tidak melewati persetujuan siapa pun, jadi alasannya wajib tercatat." | `400` |
| `FIN-VAL-159` | `PUT /non-patient-receivables/{id}` | Tagihan sudah pernah menerima pembayaran | "Tagihan yang sudah menerima pembayaran tidak bisa dikoreksi. Betulkan lewat pencatatan pelunasan." | `422` |
| `FIN-VAL-160` | `POST /{id}/cancel` | Tagihan sudah pernah menerima pembayaran | "Tagihan yang sudah menerima pembayaran tidak bisa dibatalkan. Betulkan lewat pencatatan pelunasan bernilai minus." | `422` |
| `FIN-VAL-161` | `POST /{id}/settlements` | Pelunasan membuat jumlah seluruh pembayaran melebihi total tagihan | "Jumlah pembayaran melebihi nilai tagihan. Periksa kembali nominalnya." | `422` |
| `FIN-VAL-162` | `POST /{id}/settlements` | Pelunasan bernilai minus membuat jumlah seluruh pembayaran menjadi kurang dari nol | "Pembatalan pembayaran ini melebihi pembayaran yang pernah tercatat." | `422` |
| `FIN-VAL-163` | Seluruh aksi | Perpindahan status tidak sah menurut `state-transition-matrix.md` bagian `E` | "Langkah ini tidak bisa dilakukan dari status tagihan saat ini." | `422` |
| `FIN-VAL-164` | Seluruh aksi yang mengubah data | `ExpectedRowVersion` tidak cocok dengan data terkini | "Data tagihan ini sudah diubah pengguna lain. Muat ulang halaman sebelum melanjutkan." | `409` |

## Yang sengaja **tidak** divalidasi

| Yang tidak dilarang | Alasan |
|---|---|
| Dua tagihan dengan penyewa, objek sewa, dan periode yang sama persis | `FIN-DEC-100` meniadakan master kontrak, sehingga sistem tidak punya dasar menyatakan mana yang duplikat dan mana yang memang dua tagihan berbeda. Risiko tagihan ganda **diterima sadar** dan ditangani ketelitian petugas |
| Nama penyewa yang dieja berbeda untuk penyewa yang sama | Turunan langsung dari ketiadaan master penyewa. Lihat catatan "Rencana data master awal" pada `02-backend-architecture.md` `K.7` |
| Tagihan periode yang terlewat tidak dicatat sama sekali | Tidak ada kontrak yang bisa dijadikan acuan "seharusnya ada tagihan bulan ini". `FIN-DEC-100` menerima risiko ini apa adanya |
| Denda yang tidak sebanding dengan lama keterlambatan | `FIN-DEC-102` menetapkan denda sebagai nominal yang diketik petugas, bukan hasil perhitungan — tidak ada rumus yang bisa dijadikan pembanding |

---

# Bagian F — Revisi 14: validasi buku mutasi, cutover, dan pembayaran langsung

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-1.7` — status **`draft`** |
| Naik dari | `FIN-VAL-1.6` (`approved` 1 Oktober 2026) |
| Aturan baru | `FIN-VAL-165`..`FIN-VAL-196` |
| Traceability | `FIN-DEC-111`..`137`; `FIN-DES-078`..`091` |

Pesan ditulis sebagaimana dibaca pengguna.

## F.1 Buku mutasi dan posisi saldo

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-165` | Setiap penulisan mutasi | `BalanceAfter` tidak sama dengan `BalanceBefore + Amount` | *"Perhitungan saldo mutasi tidak konsisten. Hubungi pengelola sistem."* | `500` — ini cacat program, bukan kesalahan pengguna; dijaga juga check constraint |
| `FIN-VAL-166` | Setiap penulisan mutasi | Pemanggil belum memegang advisory lock atas agregatnya | *"Perubahan saldo sedang diproses. Coba lagi beberapa saat."* | `409` |
| `FIN-VAL-167` | Mutasi piutang dan utang | `BalanceAfter` baris terakhir berbeda dari `OutstandingAmount` agregatnya | *"Saldo buku mutasi tidak cocok dengan sisa tagihan. Hubungi pengelola sistem."* | `500` — pemeriksaan ini menangkap jalur yang lupa menulis mutasi |
| `FIN-VAL-168` | `FinCashMovement` | `Amount` nol atau negatif. **Satu-satunya pengecualian:** mutasi `SALDO-AWAL` boleh bernilai **nol** (saldo awal kas kosong, diputuskan pemilik 3 Oktober 2026); nilai negatif tetap ditolak untuk semua jenis | *"Nominal mutasi kas harus lebih besar dari nol."* | `400` |
| `FIN-VAL-169` | `FinCashMovement` | Pasangan (`SourceReferenceType`, `SourceReferenceId`, `MovementType`) sudah ada | *"Mutasi kas untuk sumber ini sudah tercatat."* | `409` — perilaku idempoten, bukan galat bagi penjadwal |
| `FIN-VAL-170` | Permintaan posisi saldo | Tanggal yang diminta lebih awal daripada `CutoverDate` | *"Posisi saldo sebelum tanggal cutover tidak dapat dihitung karena buku mutasi belum berjalan pada tanggal itu."* | `422` |
| `FIN-VAL-171` | Mutasi bukan pembayaran | `PaymentMethodCode`, `FundingSourceType`, atau `ProofId` terisi | *"Metode pembayaran hanya berlaku untuk mutasi pembayaran."* | `400` |

## F.2 Pemetaan akun control

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-172` | Pemetaan | `BalanceGroup` di luar lima nilai yang sah | *"Kelompok saldo tidak dikenal."* | `400` |
| `FIN-VAL-173` | Pemetaan | `SegmentKey` tidak sah bagi kelompok itu | *"Segmen ini tidak berlaku untuk kelompok saldo yang dipilih."* | `400` |
| `FIN-VAL-174` | Pemetaan | Kelompok dan segmen yang sama sudah punya baris aktif | *"Kelompok dan segmen ini sudah dipetakan."* | `409` |
| `FIN-VAL-175` | Pemetaan | Kode akun control sudah dipakai baris aktif lain | *"Kode akun ini sudah dipakai pemetaan lain."* | `409` |
| `FIN-VAL-176` | Pemetaan | Kelompok itu sudah punya baris tanpa segmen, lalu ditambahkan baris bersegmen — atau sebaliknya | *"Satu kelompok saldo tidak boleh memakai pemetaan menyeluruh dan pemetaan per segmen sekaligus."* | `422` |
| `FIN-VAL-177` | Snapshot saldo | Ada kelompok tanpa pemetaan aktif | *"Snapshot tidak dapat diterbitkan: kelompok <nama> belum punya kode akun."* | `422` — **nol** baris outbox ditulis |
| `FIN-VAL-178` | Snapshot saldo | Ada kelompok yang segmennya terpetakan sebagian | *"Snapshot tidak dapat diterbitkan: segmen <nama> pada kelompok <nama> belum punya kode akun."* | `422` — **nol** baris outbox |
| `FIN-VAL-179` | Pemetaan | Panjang kode akun melebihi 50 karakter | *"Kode akun maksimal 50 karakter."* | `400` |

## F.3 Saldo awal cutover

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-180` | Saldo awal | Kelompok sudah punya baris aktif | *"Saldo awal untuk kelompok ini sudah pernah dicatat."* | `409` |
| `FIN-VAL-181` | Saldo awal | Kelompok `PIUTANG`, `UTANG-SUPPLIER`, atau `UTANG-JASA-MEDIS` bernilai selain nol | *"Saldo awal kelompok ini harus nol karena rinciannya datang dari migrasi tagihan lama."* | `422` |
| `FIN-VAL-182` | Saldo awal | `Reason` atau rujukan dokumen Accounting kosong | *"Alasan dan rujukan dokumen saldo awal wajib diisi."* | `422` |
| `FIN-VAL-183` | Saldo awal | Mengubah baris berstatus `APPROVED` atau `LOCKED` | *"Saldo awal yang sudah disetujui tidak dapat diubah."* | `409` |
| `FIN-VAL-184` | Saldo awal | `CutoverDate` melebihi hari ini saat dikunci | *"Tanggal cutover tidak boleh melewati hari ini."* | `422` |
| `FIN-VAL-185` | Saldo awal | Nilai negatif pada kelompok kas | *"Saldo awal kas tidak boleh negatif."* | `422` |

## F.4 Batch migrasi tagihan lama

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-186` | Unggah batch | Berkas tidak terbaca atau kolom templat tidak lengkap | *"Berkas tidak dapat dibaca. Gunakan templat yang disediakan."* | `400` |
| `FIN-VAL-187` | Validasi baris piutang | Jenis debitur di luar tiga nilai yang sah | *"Baris <n>: jenis debitur tidak dikenal."* | Tercatat sebagai galat baris, batch tetap `DRAFT` |
| `FIN-VAL-188` | Validasi baris utang | Supplier tidak ditemukan di master | *"Baris <n>: supplier tidak ditemukan."* | Galat baris |
| `FIN-VAL-189` | Validasi baris | Sisa tagihan nol atau negatif | *"Baris <n>: sisa tagihan harus lebih besar dari nol."* | Galat baris |
| `FIN-VAL-190` | Validasi baris | Tanggal dokumen melebihi `CutoverDate` | *"Baris <n>: tanggal dokumen tidak boleh melewati tanggal cutover."* | Galat baris |
| `FIN-VAL-191` | Validasi baris | Nomor dokumen kembar di dalam satu berkas | *"Baris <n>: nomor dokumen kembar dengan baris <m>."* | Galat baris |
| `FIN-VAL-192` | Persetujuan batch | `TotalOutstandingAmount` berbeda dari `DeclaredAccountingOpeningAmount` | *"Total sisa tagihan <A> tidak sama dengan saldo awal Accounting yang dinyatakan <B>. Batch tidak dapat disetujui."* | `422` |
| `FIN-VAL-193` | Persetujuan batch | Masih ada baris bergalat | *"Masih ada <n> baris yang bermasalah. Perbaiki dahulu."* | `422` |
| `FIN-VAL-194` | Persetujuan batch | `CutoverDate` batch berbeda dari `FinOpeningBalance` | *"Tanggal cutover batch berbeda dari tanggal cutover saldo awal."* | `422` |
| `FIN-VAL-195` | Persetujuan batch | Rujukan dokumen Accounting kosong | *"Rujukan dokumen saldo awal Accounting wajib diisi."* | `422` |
| `FIN-VAL-196` | Batch | Tindakan pada batch `LOCKED` atau `REJECTED` | *"Batch ini sudah final dan tidak dapat diubah."* | `409` |

## F.5 Pembayaran langsung piutang dan utang

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-197` | Pembayaran langsung | Tidak ada baris ambang aktif | *"Ambang pembayaran langsung belum ditetapkan, sehingga jalur ini belum dapat dipakai."* | `404` — **fail-closed**, bukan dianggap tak terbatas |
| `FIN-VAL-198` | Pembayaran langsung | Nominal melewati ambang aktif | *"Nominal melewati batas pembayaran langsung. Gunakan jalur pembayaran berjenjang."* | `422` |
| `FIN-VAL-199` | Pembayaran langsung | `PaymentMethod` kosong atau di luar `TRANSFER`/`CASH` | *"Metode pembayaran wajib dipilih."* | `400` |
| `FIN-VAL-200` | Pembayaran langsung | Metode `TRANSFER` tanpa rekening sumber | *"Rekening sumber dana wajib dipilih untuk pembayaran transfer."* | `422` |
| `FIN-VAL-201` | Pembayaran langsung | Metode `CASH` tetapi rekening bank diisi | *"Pembayaran tunai tidak memakai rekening bank."* | `400` |
| `FIN-VAL-202` | Pembayaran langsung | `ProofId` kosong | *"Bukti pembayaran wajib dilampirkan."* | `422` |
| `FIN-VAL-203` | Pembayaran langsung | `ProofId` sudah dipakai mutasi lain | *"Bukti ini sudah dipakai pada pembayaran lain."* | `409` |
| `FIN-VAL-204` | Pembayaran langsung | Nominal melebihi sisa tagihan | *"Nominal melebihi sisa tagihan."* | `422` — aturan yang sudah ada, dipertahankan |
| `FIN-VAL-205` | Ambang | `ChangeReason` kosong saat mengubah | *"Alasan perubahan ambang wajib diisi."* | `422` |
| `FIN-VAL-206` | Ambang | Nilai nol atau negatif | *"Ambang harus berupa angka lebih besar dari nol."* | `400` |

## F.6 Bukti pembayaran

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| ~~`FIN-VAL-207`~~ | Unggah bukti | ~~Jenis berkas di luar daftar yang diterima~~ | — | **SUPERSEDED revisi 15** oleh `FIN-VAL-217` (daftar ekstensi) dan `FIN-VAL-218` (tipe media) pada bagian `G.1`. Daftarnya sudah turun lewat `FIN-DEC-139`, dan satu aturan dipecah dua karena ekstensi dan tipe media diperiksa terpisah |
| ~~`FIN-VAL-208`~~ | Unggah bukti | ~~Ukuran melebihi batas~~ | — | **SUPERSEDED revisi 15** oleh `FIN-VAL-219` (`413`) dan `FIN-VAL-220` (`503`, batas belum dikonfigurasi). **Kode statusnya berubah dari `400` menjadi `413`** — perubahan kontrak yang disengaja, dicatat terbuka: `400` tidak membedakan "berkas Anda terlalu besar" dari "isian Anda salah" |
| `FIN-VAL-209` | Unggah bukti | Jalur simpan keluar dari akar penyimpanan | *"Berkas tidak dapat disimpan."* | `400` — pemeriksaan keamanan, pesannya sengaja tidak merinci. Diperjelas `FIN-VAL-221` (`G.1`): percobaannya **MUST** dicatat |

## F.7 Tanggal WIB dan nilai saldo

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `FIN-VAL-210` | Pesan saldo subledger | `AccountingDate` bukan tanggal akhir periode **menurut kalender WIB** | *"Tanggal akuntansi pesan saldo periode <kode> wajib tanggal akhir periode (<tanggal>)."* | `400` — aturan yang sudah ada (`FIN-VAL-082`), **pembandingnya** kini WIB |
| `FIN-VAL-211` | Pesan saldo subledger | Nilai negatif | **Tidak lagi ditolak.** Mencabut sebagian `FIN-VAL-081` | — |
| `FIN-VAL-212` | Kejadian bukan pesan saldo dan bukan penanda | `Amount <= 0` | *"Nilai kejadian harus lebih besar dari nol."* | `400` — aturan yang sudah ada, dipertahankan |
| `FIN-VAL-213` | Penanda `PEMBUKAAN-SHIFT-KASIR` | `Amount` bukan nol | *"Penanda shift tidak membawa nilai."* | `400` |

## F.8 Yang sengaja TIDAK divalidasi

Daftar ini ditulis supaya ketiadaannya tidak disangka kelalaian.

| Yang tidak divalidasi | Alasan |
|---|---|
| Pembayaran yang dipecah-pecah agar tetap di bawah ambang | `FIN-DEC-134` menolak kriteria "berisiko" pada rilis ini. Mitigasinya jejak mutasi dan laporan |
| Kebenaran `DeclaredAccountingOpeningAmount` | Sistem hanya membandingkannya dengan total item. Salah ketik yang **kebetulan** sama dengan total item **tidak** terdeteksi (`FIN-DES-090`) |
| Kecocokan kode akun control terhadap bagan akun Accounting | Finance tidak membaca bagan akun. Kode yang salah ketik baru tertangkap di kotak masuk Accounting |
| Kelengkapan tagihan lama yang dimigrasikan | Sistem tidak tahu tagihan apa saja yang *seharusnya* ada. Hanya totalnya yang direkonsiliasi |
| Kesesuaian metode pembayaran terhadap akun debit atau kredit | Milik aturan posting Accounting |
| Saldo rekening bank mencukupi sebelum pembayaran transfer | Ditolak `FIN-DEC-137` — Finance tidak memegang saldo bank |
| Nama penyewa, supplier, atau debitur kembar karena ejaan berbeda pada berkas migrasi | Hanya nomor dokumen yang diperiksa kembar |

---

# AMENDMENT REVISI 15 — Berkas bukti dan dua format migrasi

`last_changed_in`: `FIN-VAL-1.8` — status `draft`, 2 Oktober 2026.
Diturunkan dari `FIN-DEC-139`, `FIN-DEC-140`; dirancang `FIN-DES-092`, `FIN-DES-093`.
Dampak kompatibilitas: **aditif murni** — nol aturan lama diubah atau dicabut.

## G.1 Berkas bukti pembayaran (`FIN-DES-092`)

Kedelapan pemeriksaan berjalan **berurut**, dan yang pertama gagal menghentikan sisanya. Urutannya
penting: petugas yang mengunggah berkas 40 MB berekstensi `.exe` **MUST** diberi tahu soal
ekstensinya, bukan soal ukurannya.

| ID | Endpoint | Aturan | Pesan | Kode |
|---|---|---|---|---|
| `FIN-VAL-214` | `POST /transaction-proofs` | Berkas tidak dilampirkan, atau ukurannya nol byte | *"Berkas bukti wajib dilampirkan dan tidak boleh kosong."* | `400` |
| `FIN-VAL-215` | `POST /transaction-proofs` | Nama berkas lebih dari 255 karakter | *"Nama berkas terlalu panjang. Maksimal 255 karakter."* | `400` |
| `FIN-VAL-216` | `POST /transaction-proofs` | Berkas tanpa ekstensi, atau ekstensinya ada pada daftar terlarang | *"Jenis berkas ini tidak dapat diunggah."* | `400` |
| `FIN-VAL-217` | `POST /transaction-proofs` | Ekstensi di luar `FinanceManagement:TransactionProof:AllowedExtensions` | *"Bukti hanya dapat berupa PDF, JPG, JPEG, atau PNG."* | `400` |
| `FIN-VAL-218` | `POST /transaction-proofs` | Ekstensi lolos tetapi **tipe medianya tidak cocok** dengan ekstensi itu | *"Isi berkas tidak sesuai dengan jenis berkasnya. Unggah ulang berkas aslinya."* | `400` |
| `FIN-VAL-219` | `POST /transaction-proofs` | Ukuran melewati `MaxFileSizeBytes` | *"Ukuran berkas melewati batas yang diizinkan."* | `413` |
| `FIN-VAL-220` | `POST /transaction-proofs` | `MaxFileSizeBytes` **belum dikonfigurasi** | *"Unggah bukti belum dapat dipakai: batas ukuran berkas belum dikonfigurasi. Hubungi administrator."* | `503` |
| `FIN-VAL-221` | `POST /transaction-proofs` | Jalur simpan hasil penormalan mengarah **keluar** `FileStorage:UploadRootPath` | *"Nama berkas tidak dapat diterima."* — dan percobaannya **MUST** dicatat | `400` — **memperjelas `FIN-VAL-209`** (`F.6`), bukan menggantikannya: yang ditambahkan hanya kewajiban mencatat percobaannya |
| `FIN-VAL-222` | `POST api/finance/receivable/payment`, `POST api/finance/payable/payment` | `ProofId` menunjuk bukti yang **sudah** terpakai satu baris mutasi | *"Bukti ini sudah dipakai pada pembayaran lain. Unggah bukti baru."* | `409` |
| `FIN-VAL-223` | `POST api/finance/receivable/payment`, `POST api/finance/payable/payment` | `ProofId` tidak ditemukan, atau sudah bertanda `IsDelete` | *"Bukti tidak ditemukan. Unggah ulang buktinya."* | `404` |

**Kenapa `FIN-VAL-220` memakai `503` dan bukan `400`.** Keduanya menolak unggahan, tetapi artinya
berlawanan bagi petugas: `400` berarti *perbaiki berkas Anda*, sedangkan `503` berarti *tidak ada
berkas yang akan diterima sampai administrator mengisi konfigurasi*. Menyatukannya membuat petugas
mencoba berkas demi berkas untuk masalah yang bukan miliknya.

## G.2 Dua format berkas migrasi (`FIN-DES-093`)

| ID | Endpoint | Aturan | Pesan | Kode |
|---|---|---|---|---|
| `FIN-VAL-224` | `GET /opening-item-batches/template` | `format` tidak diisi, atau di luar `CSV`/`XLSX` | *"Pilih format templat: CSV atau XLSX."* | `400` |
| `FIN-VAL-225` | `POST /opening-item-batches` | Tipe media dan ekstensi berkas bukan CSV maupun XLSX | *"Berkas migrasi hanya dapat berupa CSV atau XLSX."* | `400` |
| `FIN-VAL-226` | `POST /opening-item-batches` | Berkas CSV: nilai angka atau tanggal pada satu baris tidak sesuai format templat | *"Baris &lt;n&gt;: format angka atau tanggal tidak sesuai templat."* — **MUST** menyebut nomor barisnya, **MUST NOT** menebak nilainya | Galat baris, batch tetap `DRAFT` |
| `FIN-VAL-227` | `POST /opening-item-batches` | Berkas XLSX: sel yang seharusnya angka atau tanggal bertipe teks dan tidak dapat diurai | *"Baris &lt;n&gt;: isi sel tidak dapat dibaca sebagai angka atau tanggal."* | Galat baris, batch tetap `DRAFT` |

**Dua bentuk kegagalan yang berbeda, dan keduanya disengaja dicatat terpisah** (`FIN-VAL-226` vs
`FIN-VAL-227`). CSV membawa seluruh nilai sebagai teks, sehingga kegagalannya berbentuk *format tidak
sesuai templat*. XLSX membawa sel bertipe, sehingga kegagalannya berbentuk *tipe sel tidak sesuai
harapan*. Menyatukan keduanya menjadi satu aturan menyembunyikan fakta bahwa keduanya menuntut kasus
uji sendiri — dan itu justru risiko utama yang dicatat `FIN-DEC-140`.

## G.3 Yang sengaja TIDAK divalidasi pada revisi ini

| Yang tidak divalidasi | Alasan |
|---|---|
| Isi berkas bukti benar-benar menggambarkan pembayaran itu | Sistem tidak membaca isi gambar maupun PDF. Bukti palsu atau salah foto **tidak** terdeteksi — hanya jejak pengunggah dan waktunya yang tercatat |
| Berkas bukti kembar (dua unggahan berisi gambar yang sama) | Tidak ada pembandingan isi berkas. Unique index hanya menjaga satu bukti tidak dipakai **dua mutasi** |
| Paritas CSV/XLSX pada waktu berjalan | Paritas dijaga **kasus uji** (`FIN-TEST-1.9` `J.3`), bukan pemeriksaan runtime. Sistem tidak menerima dua berkas sekaligus untuk dibandingkan |
| Kesinkronan kolom antar keempat berkas templat | Dijaga kasus uji dan review, bukan kode. Templat adalah berkas statis |
| Ekstensi pada `UploadedFileName` cocok dengan `SourceFormat` | Sengaja dibiarkan: justru ketidakcocokannya yang membuat `SourceFormat` berguna. Format ditentukan dari tipe media, bukan dari nama berkas |


---

# AMENDMENT REVISI 16 — Benturan versi ambang dan batas baris berkas migrasi

```yaml
contract_version: FIN-VAL-1.9
status: approved
owner: Yasmin (Product/Domain Finance)
approved_by: Yasmin (Product/Domain Finance)
approved_at: 2026-10-04
input_revision: 00-interview-decisions.md — dua Amendment pass 4 Oktober 2026 (FIN-DEC-141..FIN-DEC-160)
input_design: 02-backend-architecture.md AMENDMENT REVISI 16 (FIN-DES-094, FIN-DES-096, FIN-DES-098)
naik_dari: FIN-VAL-1.8 (approved 2 Oktober 2026)
dampak_kompatibilitas: Aditif — dua aturan baru; satu redaksi diperjelas; satu aturan diberi pengecualian
```

## H.1 Dua aturan baru

| Kode | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode status |
|---|---|---|---|---|
| `FIN-VAL-228` | `PUT /master-data/direct-payment-threshold` | `ExpectedRowVersion` tidak dikirim atau tidak cocok, **sementara baris ambang aktif sudah ada** | *"Ambang sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan."* | `409` |
| `FIN-VAL-229` | `POST /opening-item-batches`, `POST /opening-item-batches/{id}/reupload` | Berkas memuat lebih dari **10.000 baris data** | *"Berkas memuat lebih dari 10.000 baris. Pecah menjadi beberapa berkas, lalu unggah masing-masing sebagai batch tersendiri."* | `400` |

### `FIN-VAL-228` — satu pengecualian yang MUST ditegakkan

| Keadaan | Perilaku |
|---|---|
| Belum ada baris ambang aktif (penetapan pertama) | `ExpectedRowVersion` **tidak** diperiksa. Permintaan diterima |
| Sudah ada baris ambang aktif | `ExpectedRowVersion` **wajib** dan **MUST** cocok |

**Kenapa.** Pada penetapan pertama belum ada versi yang dapat dibaca klien. Tanpa pengecualian ini ambang
tidak akan pernah dapat ditetapkan, dan seluruh pembayaran langsung terkunci permanen (`FIN-DES-086`).

**Urutan pemeriksaan pada `PUT` sesudah revisi ini** — penting karena menentukan kode status yang diterima
pengguna ketika ada lebih dari satu kesalahan sekaligus:

| Urutan | Yang diperiksa | Bila gagal |
|---:|---|---|
| 1 | `ChangeReason` tidak kosong | `422` (`FIN-VAL-205`) |
| 2 | `Amount` lebih besar dari nol | `400` (`FIN-VAL-206`) |
| 3 | Baris ambang aktif dicari | — |
| 4 | Bila baris ada: `ExpectedRowVersion` cocok | `409` (`FIN-VAL-228`) |

Urutan 1 dan 2 **MUST** tetap mendahului 4, mengikuti urutan yang sudah berlaku pada `BE-FIN-076`: isian
yang jelas salah dijawab lebih dulu, sebelum pengguna disuruh memuat ulang karena benturan versi.

### `FIN-VAL-229` — kapan diperiksa, dan kenapa di situ

| Hal | Ketentuan |
|---|---|
| Kapan | **Sesudah** berkas diurai menjadi baris, **sebelum** berkas disimpan ke disk dan basis data |
| Kenapa sesudah diurai | Jumlah baris sebenarnya hanya diketahui setelah penguraian. Menghitung dari ukuran berkas adalah terkaan, dan `FIN-DEC-140` melarang menebak |
| Kenapa sebelum disimpan | Berkas yang ditolak **MUST NOT** meninggalkan jejak, baik berkas fisik maupun baris batch |
| Yang dihitung | **Baris data**, bukan baris berkas. Baris judul tidak ikut dihitung |
| Berkas tepat 10.000 baris | **Diterima.** Batasnya inklusif |

## H.2 Satu aturan yang diberi pengecualian

| Kode | Perubahan |
|---|---|
| `FIN-VAL-168` | Sudah diberi pengecualian pada revisi sebelumnya dan ditegaskan di sini: mutasi kas bernilai **nol** diterima **hanya** untuk jenis `SALDO-AWAL` (`FIN-DEC-143`). Nilai **negatif** tetap ditolak untuk semua jenis; nilai nol untuk jenis lain tetap ditolak `400`. Pengecualian dijaga **dua lapis**: pemeriksaan service dan batasan basis data `CK_FinCashMovement_Amount` |

## H.3 Satu redaksi yang diperjelas

| Kode | Redaksi sesudah revisi ini | Yang berubah |
|---|---|---|
| `FIN-VAL-192` | *"Total sisa tagihan (Rp A) tidak sama dengan saldo awal Accounting yang dinyatakan (Rp B). Batch tidak dapat disetujui."* | Kedua angka ditulis dengan **dua desimal** dan pemisah Indonesia, bukan dibulatkan ke satuan rupiah |

**Kenapa dua desimal penting.** Dengan pembulatan ke satuan, selisih di bawah Rp 1 **hilang dari pesan**:
pengguna membaca dua angka yang tampak sama persis, lalu diberi tahu keduanya tidak sama. Selisih sekecil
itu nyata terjadi karena nilai uang disimpan dua desimal.

**Catatan bagi pembaca pesan ini.** Layar `FE-FIN-032` **tidak** mengurai angka dari teks pesan ini; ia
menampilkan kedua angka dari data batch. Pesan ini untuk dibaca manusia, bukan untuk diurai mesin.

## H.4 Aturan yang sengaja TIDAK dibuat

| Yang dipertimbangkan | Alasan ditolak |
|---|---|
| Menolak ambang baru yang nilainya sama dengan ambang berlaku | Mengubah ambang ke nilai yang sama beserta alasan baru adalah tindakan sah — misalnya menegaskan ulang kebijakan setelah ditinjau. Menolaknya akan menghalangi pencatatan yang benar |
| Batas minimum atau maksimum nilai ambang | `FIN-DEC-154` menyerahkan angkanya kepada pejabat berwenang. Batas yang dikarang desain akan menjadi kebijakan yang tidak pernah diputuskan siapa pun |
| Batas jumlah batch aktif per jenis item | Belum ada bukti ia menjadi masalah. Batch yang menumpuk dapat ditolak, dan `FIN-DEC-155` sudah menjawab kebutuhan memecah berkas besar |
| Memeriksa batas baris dari ukuran berkas sebelum diurai | Terkaan; berkas CSV dan XLSX dengan jumlah baris sama dapat berbeda ukuran jauh |
| Menolak penguncian saldo awal Kas Kasir bernilai nol | Diputuskan sebaliknya (`FIN-DEC-143`) |
