# Matriks Validasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-VAL-1.0` |
| Status | `draft` |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | — / — |
| Input revision | `00-interview-decisions.md` revisi 1, `02-backend-architecture.md` revisi 1 |
| Dampak kompatibilitas | Nol — seluruh aturan berlaku pada endpoint baru |

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
| `FIN-VAL-075` | Jenis kejadian harus terdaftar | Penulisan kejadian | `EventTypeCode` di luar 17 kode yang disepakati | "Jenis kejadian ini belum terdaftar dalam kesepakatan dengan Accounting." | `422` |
| `FIN-VAL-076` | Kejadian tertahan tidak dikirim | Worker pengiriman | `DeliveryStatus` = `HELD_FOR_FINALIZATION` | Tidak ada pesan — baris dilewati (`FIN-DEC-004`) | — |
| `FIN-VAL-077` | Balasan `422` tidak memicu kejadian baru | Penanganan balasan | Accounting membalas `422` | Tidak ada pesan — status menjadi `HELD` | — |

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
