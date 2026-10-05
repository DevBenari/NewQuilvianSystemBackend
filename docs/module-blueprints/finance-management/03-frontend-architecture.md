# Finance Management — Arsitektur Frontend

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Frontend SHA | `abed49b03` |
| Backend SHA | `09101d05` |
| Wewenang UI | **Belum ada UI brief yang disetujui.** Dokumen ini menetapkan kontrak fungsional, bukan tampilan |

Dokumen ini menetapkan **apa yang harus bisa dilakukan layar** dan **data apa yang dikonsumsi**.
Ia sengaja tidak menetapkan tata letak, warna, urutan menu, atau pilihan antara modal dan
halaman penuh — semua itu memerlukan brief yang sah atau didelegasikan sebagai
`DEV_DISCRETION`.

## 1. Urutan wewenang yang berlaku

```text
keamanan/privasi/invariant
  -> brief produk/UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Lapisan pertama tidak dapat ditawar oleh lapisan mana pun di bawahnya. Contoh nyata: layar
**MUST NOT** menampilkan tombol Setujui kepada pengguna yang mengajukan permohonan itu, walau
secara tata letak lebih rapi bila tombolnya selalu ada.

## 2. Keadaan frontend saat ini

Diverifikasi langsung pada `abed49b03`:

| Yang sudah ada | Keadaan |
|---|---|
| Menu sidebar "Keuangan" (`corporateFinance`) | Ada, berisi **dua** butir saja: Kategori Petty Cash dan Anggaran Petty Cash |
| `/finance/petty-cash-budget` | Halaman nyata, 687 baris, berfungsi penuh |
| `/finance/master-data/petty-cash-category` | Halaman nyata beserta create, detail, dan update |
| Redux slice Petty Cash | Masih di `src/lib/state/slice/health-services/billing-management/` |
| Hook Petty Cash | Masih di `src/lib/hooks/health-services/billing-management/petty-cash/` |
| Halaman voucher di bawah `/finance/` | **Belum ada** — hanya ada di rute `health-services/billing-management` |
| Layar AR, AP, pembayaran, setoran, kas harian | **Belum ada satu pun** |

Konsekuensinya: pekerjaan frontend modul ini adalah **membangun dari nol** untuk AR, AP, kas,
dan pemantauan integrasi, ditambah **merapikan** yang sudah ada untuk Petty Cash.

## 3. Layar yang dibutuhkan

Kolom "Kebutuhan minimum" adalah kontrak fungsional. Bentuk tampilannya `DEV_DISCRETION`.

### 3.1 Piutang

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar piutang | Cari dan saring menurut penjamin, status, dan rentang jatuh tempo; menampilkan nilai asli dan sisa | `GET /receivables`, `GET /receivables/filters/metadata` |
| Rincian piutang | Menampilkan item pasien, daftar berkas klaim, riwayat pelunasan, koreksi, dan penghapusan dalam satu halaman | `GET /receivables/{id}` |
| Umur piutang | Empat kelompok 0-30, 31-60, 61-90, di atas 90 hari; dapat disaring per penjamin | `GET /receivables/aging` |
| Berkas klaim | Menandai berkas sudah diterima; mengubah status kelengkapan | `POST`/`PATCH .../documents`, `PATCH .../claim-status` |
| Ajukan koreksi | Memilih arah, nominal, dan alasan wajib | `POST /receivables/{id}/adjustments` |
| Setujui atau tolak koreksi | Terpisah dari layar pengajuan; menampilkan siapa yang mengajukan | `POST .../approve`, `POST .../reject` |
| Ajukan penghapusan | Nominal dan alasan wajib | `POST /receivables/{id}/write-offs` |
| Setujui atau tolak penghapusan | Sama seperti koreksi | `POST .../approve`, `POST .../reject` |

### 3.2 Penerimaan

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar penerimaan | Saring menurut metode, tanggal, dan shift kasir | `GET /receipts` |
| Buku penerimaan kasir | Telusur dari penerimaan ke nomor tender, kwitansi, dan tagihan | `GET /receipts/register` |
| Rekonsiliasi shift | Membandingkan total penerimaan tunai Finance dengan `SystemCash` Billing; menampilkan selisih | `GET /receipts/shift-reconciliation` |
| Alokasi penerimaan | Memilih sendiri piutang mana yang dilunasi dan berapa; **tidak ada** pencocokan otomatis (`FIN-DEC-011`) | `POST /receipts/{id}/allocations` |
| Pembalikan | Membalik satu alokasi atau seluruh penerimaan, dengan alasan | `POST .../reverse` |

### 3.3 Utang dan pembayaran

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar utang supplier | Saring menurut supplier, status, dan jatuh tempo | `GET /supplier-payables` |
| Input faktur supplier | Memilih supplier dari master existing, mengisi nomor faktur, tanggal, termin, dan rincian baris | `POST /supplier-payables` |
| Umur utang | Kelompok jatuh tempo | `GET /supplier-payables/aging` |
| Daftar utang dokter | Saring menurut dokter dan periode | `GET /doctor-payables` |
| **Ringkasan fee dokter belum dibayar** | Per dokter: berapa yang belum dibayar dan sejak periode kapan | `GET /doctor-payables/unpaid-summary` |
| Susun pembayaran rekap | Memilih **banyak** utang dalam satu pembayaran; menampilkan selisih antara total dan jumlah alokasi secara langsung | `POST /payments` |
| Ajukan, setujui, tolak | Terpisah sesuai hak akses | `POST .../submit`, `/approve`, `/reject` |
| Tandai sudah dibayar | Nomor bukti transfer wajib | `POST .../mark-paid` |

Layar "Ringkasan fee dokter belum dibayar" adalah jawaban langsung atas keluhan yang disebut
owner pada `FIN-DEC-019`. Ia **MUST** ada sejak rilis pertama rumpun dokter; tanpa layar itu,
rekonsiliasi kembali jatuh ke Excel.

### 3.4 Kas

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Setoran bank | Menampilkan **saldo tersedia** sebelum petugas mengisi nominal | `GET /bank-deposits/available-balance`, `POST /bank-deposits` |
| Posting setoran | Menampilkan penolakan beserta saldo tersedia bila nominal melebihi | `POST /bank-deposits/{id}/post` |
| Kas harian | Posisi hari berjalan dan riwayat per tanggal | `GET /daily-cash/current`, `GET /daily-cash` |
| Rincian kas harian | Telusur dari angka ringkasan ke penerimaan dan setoran penyusunnya | `GET /daily-cash/{cashDate}/breakdown` |
| Tutup kas | Menolak bila masih ada setoran `DRAFT`, dengan pesan yang menyebut jumlahnya | `POST /daily-cash/{cashDate}/close` |

### 3.5 Data induk dan pemantauan

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Bank dan rekening | CRUD beserta aktif/nonaktif | Grup `master-data/banks`, `master-data/bank-accounts` |
| Mata uang dan kurs | CRUD beserta riwayat kurs | Grup `master-data/currencies` |
| Pemantauan fakta Billing | Melihat yang gagal diolah dan mengulangnya | `GET /billing-intake`, `POST .../retry` |
| Pemantauan kejadian Accounting | Melihat status kirim, termasuk yang tertahan; mengirim ulang yang gagal | `GET /accounting-events`, `POST .../retry` |

Layar pemantauan kejadian **MUST** membedakan `HELD` dari `FAILED`, karena tindakan penggunanya
berbeda: yang pertama menunggu Accounting melengkapi aturan posting, yang kedua menunggu Finance
sendiri memperbaiki datanya.

**Diperbarui 25 September 2026 (`FIN-DEC-030`).** Status `HELD_FOR_FINALIZATION` **tidak lagi
dihasilkan** — penerimaan sebelum tagihan final kini langsung siap kirim dengan jenis kejadian
yang berbeda, bukan ditahan. Layar pemantauan tetap **MUST** dapat menampilkan status itu untuk
baris warisan (bila ada), diberi keterangan bahwa ia peninggalan kebijakan lama yang perlu
dibetulkan, bukan keadaan normal yang menunggu Billing.

## 4. Aksi per peran

| Peran | Yang dapat dilakukan | Yang **MUST NOT** terlihat |
|---|---|---|
| Finance AR Staff | Melihat piutang, mengelola berkas klaim, mengalokasikan penerimaan, mengajukan koreksi dan penghapusan | Tombol Setujui pada permohonan mana pun |
| Finance AP Staff | Input faktur supplier, menyusun dan mengajukan pembayaran | Tombol Setujui pembayaran |
| Finance Supervisor | Menyetujui atau menolak koreksi, penghapusan, dan pembayaran | Tombol Setujui pada permohonan yang **dia sendiri** ajukan |
| Cashier/Treasury | Setoran bank, tutup kas harian, menandai pembayaran sudah dibayar | Pengajuan atau persetujuan koreksi piutang |
| Auditor/Manager | Seluruh layar dalam mode baca | Seluruh tombol yang mengubah data |
| Accounting Staff | Layar pemantauan kejadian | Seluruh layar subledger Finance |

**Aturan yang paling mudah dilanggar frontend.** Menyembunyikan tombol Setujui dari pengaju
adalah bantuan tampilan, **bukan** pengaman. Backend tetap menolak lewat `FIN-VAL-020`. Frontend
**MUST NOT** pernah dianggap sebagai lapisan keamanan — menampilkan tombolnya pun tidak akan
membuat permohonannya lolos.

## 5. Penanganan keadaan layar

| Keadaan | Ketentuan |
|---|---|
| Sedang memuat | Menampilkan penanda muat; **MUST NOT** menampilkan angka nol seolah-olah itu nilai sebenarnya |
| Kosong | Membedakan "belum ada data" dari "tidak ada hasil untuk penyaring ini" |
| Gagal | Menampilkan pesan dari backend apa adanya — pesan itu sudah ditulis untuk pengguna (`validation-matrix.md`) |
| Coba lagi | Tersedia untuk kegagalan jaringan; **MUST NOT** otomatis mengulang perintah yang memindahkan uang |
| Data basi | Setelah `409` "Data telah berubah", layar **MUST** memuat ulang sebelum mengizinkan simpan berikutnya |
| Kirim ganda | Tombol simpan dinonaktifkan selama permintaan berjalan, **dan** setiap perintah uang membawa `Idempotency-Key` yang sama selama percobaan itu |

Butir terakhir penting: menonaktifkan tombol saja tidak cukup. Kalau jaringan putus lalu
pengguna menekan ulang, `Idempotency-Key` yang sama-lah yang mencegah catatan kedua
(`FIN-VAL-085`).

## 6. Angka yang MUST NOT dihitung frontend

| Angka | Diambil dari |
|---|---|
| Sisa piutang dan sisa utang | Response backend, bukan hasil pengurangan di layar |
| Kelompok umur piutang | `GET /receivables/aging` |
| Saldo kas tersedia untuk disetor | `GET /bank-deposits/available-balance`, dihitung ulang backend saat posting |
| Posisi kas harian | `GET /daily-cash/current` |
| Selisih antara total pembayaran dan jumlah alokasi | Boleh ditampilkan langsung di layar sebagai bantuan, tetapi keputusan diterima atau ditolak tetap milik backend |
| Total penerimaan per shift | `GET /receipts/shift-reconciliation` |

Alasannya sama untuk semuanya: dua tempat yang menghitung angka uang pasti berbeda setelah
revisi pertama.

## 7. Privasi di layar

| Data | Ketentuan |
|---|---|
| `PatientId`, `EncounterId` | Boleh ditampilkan di rincian piutang Finance; **MUST NOT** ikut ke layar pemantauan kejadian Accounting |
| `BenefitOwnerId`, hubungan keluarga | Hanya pada layar piutang manfaat karyawan, hanya untuk peran yang berhak |
| Nomor rekening bank | Ditampilkan sebagian pada daftar; lengkap hanya pada layar master untuk peran yang berhak |
| `DoctorId` | Boleh pada layar utang dokter; **MUST NOT** pada layar kejadian Accounting |

## 8. Rute

**Belum ada brief yang menetapkan rute final.** Yang mengikat hanyalah dua hal:

1. Rute baru mengikuti pola yang sudah berjalan, yaitu di bawah `/finance/...` — bukan di bawah
   `/health-services/billing-management/...`. Ini konsisten dengan dua rute Finance yang sudah
   ada.
2. Butir menu baru masuk ke group `corporateFinance` yang **sudah ada** di
   `src/utils/menu-sidebar/menu-items.jsx`. Tidak perlu membuat group baru.

Struktur rute selebihnya, termasuk apakah koreksi dan penghapusan memakai halaman terpisah atau
modal, adalah `DEV_DISCRETION` sampai ada brief.

## 9. Pekerjaan merapikan Petty Cash

Bukan membangun ulang — halaman `/finance/petty-cash-budget` sudah berfungsi penuh.

| Pekerjaan | Sifat |
|---|---|
| Memindahkan hook dan Redux slice Petty Cash ke path Finance | Rapi-rapi, bukan fitur baru |
| Menambah halaman voucher di bawah `/finance/` | Memakai ulang modal yang sudah ada (`create-voucher-modal`, `voucher-detail-modal`, `reverse-voucher-modal`) |
| Menambah butir menu voucher pada group Keuangan | Satu baris di `menu-items.jsx` |

Aturan bisnis Petty Cash **MUST NOT** diubah dalam pekerjaan ini. Keputusannya milik blueprint
`billing-kasir` (`FIN-DEC-009`).

## 10. Matriks `DEV_DISCRETION`

| Keputusan | Wewenang | Catatan |
|---|---|---|
| Tata letak halaman, urutan kolom tabel | `DEV_DISCRETION` | Mengikuti pola halaman Finance yang sudah ada |
| Modal atau halaman penuh untuk koreksi dan penghapusan | `DEV_DISCRETION` | — |
| Urutan butir menu di dalam group Keuangan | `DEV_DISCRETION` | Group-nya sendiri sudah ditetapkan |
| Warna, tipografi, spacing | Design system project | Bukan wewenang modul ini |
| Apakah tombol Setujui disembunyikan atau dinonaktifkan bagi pengaju | `DEV_DISCRETION` | Keduanya sah; yang **MUST** adalah backend tetap menolak |
| Apakah rekonsiliasi shift satu halaman atau tab | `DEV_DISCRETION` | — |
| Bentuk penyajian umur piutang | `DEV_DISCRETION` | Kelompoknya sudah ditetapkan `FIN-DEC-010` dan **MUST NOT** diubah layar |
| Kapan angka dimuat ulang otomatis | `DEV_DISCRETION` | Kecuali setelah `409`, yang **MUST** memuat ulang |

## 11. Ketergantungan pada backend

Seluruh layar pada bagian 3 menunggu endpoint yang saat ini berlabel
**Rencana (belum tersedia)** di `contracts/api-contract.md`. Frontend **MUST NOT** dimulai
dengan data tiruan yang bentuknya ditebak — kontrak API sudah tertulis, dan bentuk itulah yang
dipakai.

Dua layar menunggu pihak ketiga, bukan menunggu Finance:

| Layar | Menunggu |
|---|---|
| Piutang manfaat karyawan | Konfirmasi Billing + HR (`FIN-CQ-03`) |
| Status kirim nyata ke Accounting | Endpoint Accounting dibangun (`FIN-CAP-018`) |

## 12. Amendment 25 September 2026 — Purchasing/AP, AR Invoice Agregat, Potongan AR

Menambah kebutuhan layar untuk tiga rumpun baru (`FIN-SC-008`, `FIN-SC-009`, `FIN-SC-010`),
disetujui lewat `FIN-DEC-045`..`055` dan dirancang backend-nya pada `02-backend-architecture.md`
bagian AMENDMENT REVISI 4. Sama seperti seluruh dokumen ini, bagian ini menetapkan kontrak
fungsional, bukan tata letak — belum ada UI brief baru yang disetujui untuk rumpun ini.

### 12.1 Layar Purchasing/AP

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar Purchase Order | Cari dan saring menurut supplier, status, rentang tanggal | `GET /purchase-orders` |
| Buat Purchase Order | Memilih supplier dari master existing, mengisi baris item (kategori, nama, satuan, kuantitas, harga) | `POST /purchase-orders` |
| Rincian Purchase Order | Menampilkan status, riwayat approval, daftar Tanda Terima Barang terkait | `GET /purchase-orders/{id}` |
| Ajukan, setujui, tolak PO | Terpisah sesuai hak akses; menampilkan `ApprovalTier` yang dihitung backend, **bukan** dihitung layar | `POST .../submit`, `/approve`, `/reject` |
| Tanda Terima Barang | Mencatat kuantitas diterima per baris PO, catatan kondisi barang | `POST /goods-receipts` |
| Daftar Tukar Faktur | Saring menurut supplier dan status | `GET /invoice-exchanges` |
| Buat Tukar Faktur | Memilih PO/GR (opsional — boleh kosong per `FIN-DEC-051`), mengisi nomor dan tanggal faktur supplier; **estimasi jatuh tempo dihitung backend** dari TOP supplier | `POST /invoice-exchanges` |
| Daftar Purchasing Invoice | Saring menurut supplier, status, jatuh tempo | `GET /purchasing-invoices` |
| Buat Purchasing Invoice | Wajib memilih satu Tukar Faktur yang belum terpakai; mengisi PPN, diskon, DP, potongan lain per baris | `POST /purchasing-invoices` |
| Ajukan, setujui, tolak Purchasing Invoice | Sama seperti PO — `ApprovalTier` dari backend | `POST .../submit`, `/approve`, `/reject` |
| Retur Pembelian | Memilih Purchasing Invoice sumber, mengisi baris retur dan alasan | `POST /supplier-returns` |
| Deposit Retur | Menampilkan saldo tersedia per supplier (baca saja). ~~Memilih Purchasing Invoice tujuan saat dipakai~~ — **diganti bagian 13.1**: deposit dipakai di layar pembayaran | `GET /supplier-returns/deposits` |
| ~~Aging AP (Purchasing)~~ | **Dicabut** (`FIN-DEC-059`) — layar `/finance/ap-aging` yang sudah ada tetap satu-satunya | — |
| Rekap Purchasing AP | Ringkasan periodik | `GET /purchasing-reports/summary` |
| Laporan Tukar Faktur | Daftar Tukar Faktur beserta status keterkaitan ke Purchasing Invoice | `GET /purchasing-reports/invoice-exchanges` |
| Laporan Jatuh Tempo | Purchasing Invoice mendekati/lewat jatuh tempo | `GET /purchasing-reports/due-dates` |
| Rekonsiliasi Tagihan | Tukar Faktur yang belum menghasilkan Purchasing Invoice | `GET /purchasing-reports/reconciliation` |

Seluruh angka pada delapan layar laporan **MUST NOT** dihitung ulang di frontend — sama seperti
aturan bagian 6, kelompok umur dan status jatuh tempo diambil apa adanya dari response backend.

### 12.2 Layar AR Invoice Agregat

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Daftar Batch Tagihan | Saring menurut penjamin dan status | `GET /receivable-invoice-batches` |
| Buat Batch Tagihan | Memilih penjamin dan periode; sistem menampilkan daftar `FinReceivable` yang memenuhi syarat pengelompokan (backend yang menentukan kelayakan, bukan layar) | `POST /receivable-invoice-batches` |
| Rincian Batch Tagihan | Menampilkan daftar invoice anggota, masing-masing dapat ditelusur ke dokumen per-invoice (`BillingCompanyGuarantorInvoiceDocumentService`) sebagai rincian baris | `GET /receivable-invoice-batches/{id}` |
| Terbitkan Batch | Mengubah status jadi `Issued`, mengunci daftar anggotanya | `POST .../issue` |
| Cetak/unduh dokumen Batch | Menggabungkan header batch dengan rincian per-invoice | `GET .../document` |

### 12.3 Layar Potongan AR

Potongan sisi penerimaan (PPh 23, biaya admin bank) **MUST** menyatu dengan layar Alokasi
Penerimaan yang sudah ada (bagian 3.2), bukan layar terpisah — potongan hanya bermakna dalam
konteks satu penerimaan yang sedang dialokasikan.

| Layar | Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|---|
| Alokasi penerimaan (diperbarui) | Baris tambahan opsional untuk mencatat potongan: jenis (PPh 23/biaya admin bank/lain), nominal, alasan. Sisa piutang setelah potongan **MUST** ditampilkan dari response backend, bukan hasil pengurangan di layar | `POST /receipts/{id}/allocations` (payload diperluas). ~~`POST /receipts/{id}/deductions`~~ dicabut — lihat bagian 13.2 |

### 12.4 Aksi per peran (tambahan)

| Peran | Yang dapat dilakukan | Yang **MUST NOT** terlihat |
|---|---|---|
| Finance AP Staff | Input PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian | Tombol Setujui PO/Purchasing Invoice |
| Supervisor Finance | Menyetujui PO/Purchasing Invoice di bawah ambang (`FIN-DEC-052`) | Tombol Setujui untuk PO/Purchasing Invoice yang **dia sendiri** ajukan |
| Manajer Finance | Menyetujui PO/Purchasing Invoice di atas ambang | Sama seperti Supervisor Finance untuk pengajuan sendiri |
| Finance AR Staff | Membuat dan menerbitkan Batch Tagihan, mencatat potongan AR saat alokasi penerimaan | — |

Checkpoint approval PO/Purchasing Invoice (`FIN-DES-039`) terpisah dari checkpoint approval
pembayaran (bagian 4 existing) — frontend **MUST** memperlakukannya sebagai dua permohonan
berbeda meski ambang nominalnya sama, karena aktor dan record persetujuannya berbeda.

### 12.5 Rute (tambahan)

Mengikuti aturan bagian 8 yang sudah ada — seluruh rute baru masuk di bawah `/finance/...` dan
butir menunya masuk ke group `corporateFinance` yang sudah ada. Tidak ada group menu baru.

### 12.6 Matriks `DEV_DISCRETION` (tambahan)

| Keputusan | Wewenang | Catatan |
|---|---|---|
| Tata letak layar Purchasing/AP dan Batch Tagihan | `DEV_DISCRETION` | Mengikuti pola halaman Finance yang sudah ada |
| Apakah potongan AR memakai baris tambahan pada form alokasi atau modal terpisah | `DEV_DISCRETION` | Yang **MUST** adalah tetap dalam alur satu penerimaan yang sama |
| Urutan langkah wizard PO → GR → Tukar Faktur → Purchasing Invoice, satu alur atau layar terpisah | `DEV_DISCRETION` | Urutan datanya sendiri **MUST NOT** diubah (`FIN-DEC-051`) |

### 12.7 Ketergantungan pada backend (tambahan)

Seluruh layar bagian 12.1-12.3 menunggu endpoint berlabel **Rencana (belum tersedia)** di
`contracts/api-contract.md`. Layar Purchasing Invoice khususnya **MUST NOT** menampilkan status
kirim kejadian akuntansi PPN Masukan sebagai "berhasil" sebelum Accounting meratifikasi kode
`PPN-MASUKAN-PEMBELIAN` (`FIN-OQ-020`) — layar pemantauan kejadian (bagian 3.5) yang sudah ada
sudah cukup untuk menampilkan status `PENDING`/`HELD` baris ini, tidak perlu indikator baru.

## 13. Amendment 25 September 2026 (revisi 5) — deposit di layar pembayaran, potongan di form alokasi, label menu

Menindaklanjuti `FIN-DEC-057`..`062` dan `02-backend-architecture.md` AMENDMENT REVISI 5. Seperti
bagian 12, ini kontrak fungsional, bukan tata letak.

### 13.1 Deposit Retur dipakai di layar Susun Pembayaran

Deposit **tidak lagi** dipakai dari layar Deposit Retur ke sebuah Purchasing Invoice. Ia dipilih
sebagai **sumber dana** saat petugas AP menyusun pembayaran supplier (layar bagian 3.3
"Susun pembayaran rekap").

| Kebutuhan minimum | Endpoint yang dikonsumsi |
|---|---|
| Saat pembayaran `DRAFT` dan `PaymentType = SUPPLIER`, tampilkan Deposit Retur `AVAILABLE` milik supplier penerima beserta saldonya | `GET /supplier-returns/deposits?supplierId=…` |
| Tambah deposit sebagai sumber dana; lepas bila keliru | `POST`/`DELETE /payments/{id}/return-deposits` |
| Tampilkan **empat angka** dari response, tidak dihitung layar: utang yang dilunasi (`totalAmount`), potongan/tambahan, dari deposit (`depositAppliedAmount`), dan yang ditransfer (`netTransferAmount`) | `GET /payments/{id}` |
| Pembayaran yang seluruhnya dari deposit (`netTransferAmount = 0`) **tidak** meminta nomor bukti transfer saat ditandai dibayar | `POST /payments/{id}/mark-paid` |
| Riwayat baris deposit yang dilepas tetap terlihat, dengan status `RELEASED` | `GET /payments/{id}/return-deposits` |

**Yang MUST NOT dilakukan layar:** menghitung sendiri sisa deposit sesudah dipilih — dua petugas
dapat memilih deposit yang sama, dan hanya backend yang tahu siapa yang lebih dulu. Setelah
`422` saldo kurang, layar MUST memuat ulang daftar deposit.

### 13.2 Potongan AR di dalam form alokasi

Menegaskan bagian 12.3: potongan dicatat **di baris alokasi yang sama**, dan terkirim dalam satu
permintaan `POST /receipts/{id}/allocations`. Tidak ada tombol "tambah potongan" terpisah
sesudah alokasi tersimpan.

| Kebutuhan minimum | Catatan |
|---|---|
| Baris potongan hanya muncul pada baris alokasi ke **piutang** | Bukan pada alokasi langsung ke tagihan (`INVOICE_DIRECT`) |
| Satu baris alokasi boleh membawa lebih dari satu potongan | Contoh: PPh 23 **dan** biaya bank |
| Sisa piutang sesudah uang + potongan diambil dari response | Bantuan tampilan selisih boleh, keputusan diterima/ditolak milik backend |
| Riwayat penerimaan menampilkan potongan dan pembaliknya | `GET /receipts/{id}/deductions` |

### 13.3 Label menu (`FIN-DEC-060`)

| Butir | Letak | Layar |
|---|---|---|
| Submenu **"Pembelian"** | Group `corporateFinance`, sejajar submenu Master Data | Purchase Order, Tanda Terima Barang, Tukar Faktur, **Faktur Pembelian** (Purchasing Invoice), Retur & Deposit Retur, Laporan Pembelian |
| **"Tagihan Gabungan Penjamin"** | Butir flat, sejajar "Piutang" | Batch Tagihan AR |
| "Faktur & Tagihan Supplier" | **Tidak berubah** — tetap di tempatnya | Daftar utang supplier (`/finance/payable/invoice`) |

Urutan butir di dalam submenu "Pembelian" dan rute persisnya tetap `DEV_DISCRETION`, mengikuti
pola `FIN-DEC-024` (`/finance/<capability>`).

### 13.4 Status kirim kejadian

Sama seperti bagian 12.7: layar **MUST NOT** menampilkan kejadian `POTONGAN-PIUTANG-NON-TUNAI`,
`PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI`, `RETUR-PEMBELIAN`, atau `PEMAKAIAN-DEPOSIT-RETUR` sebagai
"terkirim" sebelum Accounting meratifikasinya (`FIN-OQ-026`). Layar pemantauan bagian 3.5 sudah
cukup — tidak perlu indikator baru.

## 14. Amendment 28 September 2026 (revisi 6) — katalog kejadian final dan fakta yang gagal disinkronkan

Menurunkan `FIN-DES-051`..`058`. **Nol layar baru, nol butir menu baru, nol rute baru.** Seluruh
perubahan menempel pada layar pemantauan yang sudah ada (bagian 3.5) dan pada satu form yang sudah
dirancang (bagian 13.2).

### 14.1 Nama kejadian pada layar pemantauan

Empat kode yang sudah berjalan **berganti nama** dan lima nama lain **tidak ada lagi**
(`integration-contract.md` bagian 5.10.2). Konsekuensi bagi frontend:

| Hal | Ketentuan |
|---|---|
| Daftar pilihan filter jenis kejadian | **MUST** diambil dari `GET /accounting-events/filter-metadata`, **MUST NOT** ditulis tangan di frontend. Hari ini daftar itu tertinggal 17 kode di backend; sesudah revisi ini ia mengikuti katalog. Frontend yang menyalin daftar sendiri akan tertinggal setiap kali katalog bergerak |
| Label yang ditampilkan | Kode kejadian ditampilkan **apa adanya** dari backend. Frontend **MUST NOT** menerjemahkan atau memperindah nama kode — nama itu kontrak dengan Accounting, dan label karangan frontend membuat petugas dan akuntan menyebut satu kejadian dengan dua nama berbeda |
| Baris lama bernama pendek | Baris outbox lama (`AP_CREATED` dan kawan-kawan) **tetap** ada dan **tetap** tampil dengan nama lamanya. Layar **MUST NOT** menyembunyikannya — itu data nyata yang belum terkirim |

### 14.2 Dua kejadian bernilai nol

`PENUTUPAN-SHIFT-KASIR` dan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bernilai `0` karena keduanya penanda
status, bukan transaksi (`FIN-DES-054`).

| Keadaan | Ketentuan layar |
|---|---|
| Menampilkan nilai | Kolom nilai menampilkan `0` apa adanya. Layar **MUST NOT** menyembunyikan baris bernilai nol dan **MUST NOT** menandainya sebagai kesalahan |
| Penjelasan bagi petugas | Kedua kode ini **MUST** dapat dibedakan dari kejadian transaksi oleh pembacanya. Caranya `DEV_DISCRETION` — boleh lewat pengelompokan, penanda, atau keterangan; yang **tidak** boleh adalah membuat petugas menyimpulkan sendiri bahwa nilai nol berarti gagal |

### 14.3 Fakta yang gagal disinkronkan — dua sebab baru

`FIN-VAL-141` dan `FIN-VAL-142` menghasilkan baris intake berstatus `ERROR` yang **sengaja** tidak
menerbitkan kejadian: refund kredit yang perlakuan akuntansinya belum ditetapkan, dan pembalikan
tender top-up deposit yang tidak punya mutasi pembalik di Billing.

| Hal | Ketentuan |
|---|---|
| Layar yang dipakai | Pemantauan fakta Billing yang **sudah ada** (bagian 3.5, `GET /billing-intake`) — bukan layar baru |
| Yang **MUST** terbaca | Sebab kegagalannya, dalam bahasa yang dipahami petugas keuangan, beserta jenis fakta dan nomor rujukannya |
| Tombol "ulangi" | **MUST NOT** ditawarkan untuk kedua sebab ini. Mengulangnya tidak akan pernah berhasil — yang kurang adalah keputusan akuntansi atau perbaikan di Billing, bukan percobaan ulang. Menawarkan tombol yang pasti gagal membuat petugas menekan berulang dan menyimpulkan sistemnya rusak |
| Yang **MUST NOT** terjadi | Baris `ERROR` jenis ini hilang dari daftar, tergabung diam-diam ke penghitung "gagal" tanpa sebab, atau disamakan dengan kegagalan teknis yang memang layak diulang |

### 14.4 Jenis potongan yang ditolak di form alokasi

Pilihan jenis potongan pada form alokasi (bagian 13.2) **menyempit**: hanya PPh 23 dan biaya
administrasi bank (`FIN-VAL-137`).

| Hal | Ketentuan |
|---|---|
| Pilihan yang ditawarkan | Dua jenis saja. Jenis "lain-lain" **MUST NOT** ditawarkan sebagai pilihan yang dapat dipilih |
| Bila petugas memang menghadapi potongan jenis lain | Layar **MUST** memberi jalan keluar yang jujur: sebutkan bahwa jenis itu belum dapat dicatat dan arahkan ke bagian akuntansi. Bentuk penyampaiannya `DEV_DISCRETION` |
| Yang **MUST NOT** terjadi | Petugas memilih "lain-lain", mengisi seluruh form, lalu baru ditolak backend sesudah menekan simpan — dan seluruh alokasinya hangus bersamaan |

### 14.5 Ketergantungan pada backend (tambahan)

| Yang dibutuhkan | Keadaan |
|---|---|
| `GET /accounting-events/filter-metadata` mengikuti katalog, bukan 17 kode | Menunggu `FIN-DES-058` diimplementasikan |
| Sebab kegagalan intake terbaca lewat API, bukan hanya di database | Menunggu `FIN-VAL-141`/`142` diimplementasikan |
| Jenis potongan yang sah dapat dibaca dari backend | **Belum ada.** Selama belum ada, daftar dua jenis itu **DEV_DISCRETION** di frontend, dan **MUST** diselaraskan ulang begitu kode ketiga diratifikasi (`FIN-OQ-033`) |

## 15. Amendment 28 September 2026 (revisi 7) — penyelarasan menu ke `FIN-DEC-060`

Menurunkan `FIN-DEC-076` dan `FIN-DES-060`. **Nol layar baru, nol endpoint baru, nol hak akses
baru.** Seluruhnya relabel dan restrukturisasi butir menu.

### 15.1 Kenapa bagian ini ada

`/trace-existing-capabilities` menemukan menu Finance yang **sudah dibangun** menyimpang dari
`FIN-DEC-060` — keputusan yang sudah disetujui owner **sebelum** menu itu dibangun. Owner memilih
menegakkan keputusannya (`FIN-DEC-076`), bukan merevisi keputusan mengikuti implementasi.

Ini dicatat sebagai **penyimpangan implementasi yang dikoreksi**, bukan sebagai keputusan UI baru.
Wewenangnya jelas: `approved product brief` (`FIN-DEC-060`) berada di atas `project convention` dan
`DEV_DISCRETION` pada hierarki kewenangan bagian 10.

### 15.2 Pemetaan yang MUST dikerjakan

| Yang ada sekarang | Yang seharusnya (`FIN-DEC-060`) | Sifat perubahan |
|---|---|---|
| Grup **"Account Payable"** memuat langsung enam butir | Submenu **"Pembelian"** memuat lima layar AP Purchasing; butir AP lain tetap di tempatnya | Restrukturisasi satu tingkat |
| "Purchase Order" | Label Indonesia, di dalam submenu "Pembelian" | Relabel + pindah |
| "Receiving" | Label Indonesia untuk Tanda Terima Barang, di dalam submenu "Pembelian" | Relabel + pindah |
| "Supplier Invoice" | **"Faktur Pembelian"**, di dalam submenu "Pembelian" | Relabel + pindah |
| — (tidak ada) | **"Tagihan Gabungan Penjamin"** sebagai butir **flat**, sejajar "Piutang" — bukan di dalam submenu | **Butir baru** |
| "Faktur & Tagihan Supplier" (`/finance/payable/invoice`) | **Tidak berubah** — tetap di tempatnya (`FIN-DEC-060` menyebut ini eksplisit) | Nol |

**Label Indonesia persis untuk "Purchase Order" dan "Receiving" belum ditetapkan `FIN-DEC-060`.**
Keputusan itu hanya menyebut submenu "Pembelian", label "Faktur Pembelian", dan butir "Tagihan
Gabungan Penjamin". Karena itu kedua label sisanya **`DEV_DISCRETION`** dengan satu batas: **MUST
Bahasa Indonesia**, konsisten dengan tiga label yang sudah ditetapkan. Agent **tidak** menetapkannya
di sini.

### 15.3 Urutan yang MUST diikuti — menu paling akhir

Butir "Purchase Order" dan "Receiving" yang ada sekarang mengarah ke `/finance/payable?tab=po` dan
`?tab=receiving`, dan **belum ada** pemanggilan endpoint Purchasing mana pun dari frontend
(`FIN-CQ-07`). Penyebabnya di backend: kelima endpoint `GET /` berpaging **sudah dikontrak**
(`contracts/api-contract.md` `B.1`-`B.5`, berlabel `Rencana`) tetapi **belum dibangun**.

| Urutan | Pekerjaan | Kenapa urutannya begitu |
|---:|---|---|
| 1 | Backend: bangun kelima `GET /` berpaging | Tanpa ini layar daftar tidak punya sumber data |
| 2 | Frontend: bangun layar Purchasing (bagian 12.1) | Butir menu perlu tujuan yang benar-benar menampilkan data |
| 3 | Frontend: selaraskan butir menu (bagian 15.2) | Menyelaraskan lebih dulu hanya memindahkan butir yang tetap kosong |

Ketiganya **MUST** dijadwalkan sebagai satu rangkaian, bukan tiga task lepas. Butir menu
"Tagihan Gabungan Penjamin" punya rangkaiannya sendiri dan **MUST NOT** didaftarkan sebelum layar
Batch Tagihan AR ada — entity-nya (`FinReceivableInvoiceBatch`) masih nol baris (`FIN-CAP-032`).

### 15.4 Penyaringan hak akses butir menu — DIKOREKSI 29 September 2026 (`FIN-DEC-082`, `FIN-DES-069`)

> **KOREKSI BERBASIS BUKTI SOURCE.** Isi bagian ini sebelumnya bersandar pada dua premis yang
> **terbukti keliru** saat impact scan frontend `a31da3c21` dijalankan pada pass desain 29 September
> 2026 (`02-backend-architecture.md` AMENDMENT REVISI 11, `I.1`). Teks lama dipertahankan di bawah
> sebagai riwayat, dengan koreksinya di atas — bukan dihapus.

**Premis lama (1): "butir menu Finance dijaga payung, sedangkan endpoint Purchasing menuntut
granular, sehingga ada risiko `403`."** Kenyataannya, seluruh butir menu yang dijaga
`Finance.AP`/`Finance.AR` menunjuk rute **V2** (`/finance/payable*`, `/finance/receivable*`) dengan
aksi `View`/`Payment`/`Report` — yaitu aksi milik `FinanceApController`/`FinanceArController` sendiri.
Menu dan endpointnya **konsisten**; tidak ada risiko `403` di sana. Butir menu Purchasing
("Pembelian") **belum ada sama sekali** — ia baru akan dibangun `FE-FIN-008`..`014`.

**Premis lama (2): berkas penyaringnya `src/utils/menu-sidebar/corporateFinance.js`.** Berkas itu
**tidak ada** (`FIN-CQ-10`). Yang nyata: `src/utils/menu-sidebar/menu-items.jsx` (deklarasi
`requiredPermission` per butir) dan `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx`
(fungsi penyaringnya, *fail-closed* — hanya `true` yang menampilkan).

| Aspek | Ketetapan yang BERLAKU sesudah koreksi |
|---|---|
| Keputusan yang berlaku | `FIN-DEC-082` dan `FIN-DEC-083` (approved 29 September 2026), diturunkan ke `FIN-DES-066`..`069` (`draft`) |
| Nama resource payung | **`Finance.AP.Umbrella`** dan **`Finance.AR.Umbrella`** — **bukan** `Finance.AP`/`Finance.AR`, yang sudah menjadi milik dua controller V2 yang berjalan |
| Hak akses butir menu baru (`FE-FIN-008`..`014`) | Dijaga resource **granular** yang sama persis dengan yang dituntut endpointnya — mis. butir "Purchase Order" dijaga `FinancePurchaseOrder : Read`. Payung **tidak pernah** diperiksa penyaring menu (`FIN-DES-069`) |
| Butir menu V2 yang sudah ada | **Tidak disentuh.** Tetap dijaga `Finance.AP`/`Finance.AR` milik controller V2 |
| Dampak pada kode frontend hari ini | **NOL perubahan** — tetapi untuk alasan yang berbeda dari klaim lama: bukan karena payung dipakai penyaring menu, melainkan karena butir menu yang dijaga payung memang milik V2 dan tidak berubah, sementara butir menu Purchasing belum pernah ada |
| Jaminan tidak `403` | Berlaku karena menu memakai resource granular yang **sama persis** dengan yang dituntut endpoint. Pemegang payung memiliki pasangan granular itu sebagai baris `SysAccessPolicy` sungguhan hasil materialisasi (`FIN-DES-067`), sehingga terbaca `GET auth/permissions` tanpa perlakuan khusus |
| Status temuan | `FIN-OQ-036` **dibuka kembali lalu ditutup ulang** oleh `FIN-DES-069` dengan jawaban yang berpijak pada source. `FIN-CAP-040` tetap `CLOSED`. Dua temuan baru dibuka: `FIN-CQ-09` (butir menu "Report AR"/"Report AP" dijaga aksi yang tidak pernah terdaftar, sehingga tersembunyi permanen bagi semua orang) dan `FIN-CQ-10` (nama berkas penyaring pada dokumen) |
| Gerbang | `FIN-OQ-038` — pembawa resource payung di registry; menahan implementasi ekspansi, **tidak** menahan pekerjaan frontend mana pun |

---

## 16. Layar / Tab Snapshot Saldo Subledger Bulanan (`FE-FIN-015`)

Menjawab kebutuhan rekonsiliasi tutup buku bulanan Accounting (`accounting/evidence/15` Butir 15.1 s/d 15.4, `ACC-DEC-108`, `FIN-DEC-090`, `contracts/integration-contract.md` §5.6), backend telah menyelesaikan `BE-FIN-048` (pengetatan validasi outbox) dan `BE-FIN-049` (layanan kalkulasi agregasi 4 akun kontrol dan pementasan kejadian `SALDO-SUBLEDGER`).

### 16.1 Kebutuhan Fungsional Minimum

| Elemen UI | Kebutuhan Minimum | Endpoint yang Dikonsumsi |
|---|---|---|
| Pemilih Periode | Memilih periode akuntansi (format `YYYY-MM`), default bulan aktif/sebelumnya. | — |
| Status Kelengkapan | Menampilkan status kelengkapan 4 akun kontrol (`IsComplete = true/false`), total agregat nominal saldo, dan tanggal akhir periode akuntansi (`AccountingDate`). | `GET api/v1/corporate/finance-management/accounting-events/subledger-balances/{accountingPeriodCode}` |
| Tabel 4 Akun Kontrol | Menampilkan daftar 4 akun kontrol: Kas Kasir (`1-1002`), Kas Kecil (`1-1003`), Piutang Pasien & Penjamin (`1-2001`), Utang Supplier (`2-1001`), beserta nominal saldo (selalu non-negatif `>= 0.00`), nomor kejadian outbox (`EventNumber`), versi (`SourceVersion`), dan status pengiriman (`DeliveryStatus`). | `GET api/v1/corporate/finance-management/accounting-events/subledger-balances/{accountingPeriodCode}` |
| Tombol Pemicu Kalkulasi | Tombol *"Kalkulasi & Terbitkan Saldo Subledger"* untuk mengeksekusi kalkulasi ulang/pernyataan saldo. Membuka modal konfirmasi dengan catatan operasional opsional sebelum `POST`. | `POST api/v1/corporate/finance-management/accounting-events/subledger-balances/generate` |

### 16.2 Aturan Otorisasi & Penjagaan Antarmuka
1. **Hak Akses:**
   - Hak baca query snapshot dijaga oleh `FinanceAccountingEvent : Read`.
   - Hak tombol pemicu generate snapshot dijaga oleh `FinanceAccountingEvent : Create`.
   - Tombol pemicu **wajib disembunyikan atau dinonaktifkan** bagi pengguna tanpa izin `Create`.
2. **Penanganan Respon & Error:**
   - Respons `200 OK` menampilkan notifikasi sukses dan memuat ulang tabel data.
   - Respons `400 Bad Request` menampilkan pesan galat dari backend apa adanya (misal bila format periode tidak valid).
   - Nilai uang **MUST NOT** dihitung atau dibulatkan ulang di klien; seluruh angka murni dibaca dari backend.
3. **Penempatan Navigasi:**
   - Dapat ditempatkan sebagai tab terdedikasi *"Saldo Subledger Bulanan"* pada halaman Pemantauan Finance (`/finance/monitoring?tab=subledger`) atau sebagai rute terdedikasi (`/finance/subledger-balances`) — **`DEV_DISCRETION`**.


---

## 17. Amendment 1 Oktober 2026 (revisi 13) — penempatan halaman mengikuti V1 dan layar Manajemen Klaim

| Field | Nilai |
|---|---|
| Keputusan yang diturunkan | `FIN-DEC-094` (**supersedes `FIN-DEC-060`**), `FIN-DEC-095`, `FIN-DEC-096`, `FIN-DEC-097`, `FIN-DEC-098` |
| Rancangan backend | `FIN-DES-070`..`FIN-DES-073`, `02-backend-architecture.md` AMENDMENT REVISI 13 |
| Wewenang UI | `FIN-DEC-094` adalah **approved product brief** — ia mengunci *layar mana yang berdiri sendiri* dan *butir menu mana yang ada*, karena bentuk itu sudah disetujui pengguna lewat UAT pada sistem produksi V1. Tata letak, warna, ikon, urutan visual, dan component library **tetap `DEV_DISCRETION`** |
| Yang dicabut | Bagian 15 (submenu "Pembelian" menurut `FIN-DEC-060`) **tidak lagi berlaku** sebagai struktur menu. Isinya tetap dibaca sebagai riwayat, bukan sebagai ketetapan |

### 17.1 Apa yang berubah dari bagian 15, dan mengapa

Bagian 15 menurunkan `FIN-DEC-060`: lima layar Purchasing dikelompokkan ke submenu "Pembelian",
beberapa kemampuan sengaja **dikonsolidasikan** ke dalam satu layar (tanda terima barang menjadi
modal di detail Purchase Order; pembatalan, penghapusan, dan laporan AR menjadi modal/tab di dalam
buku piutang). Konsolidasi itu dibangun dan berjalan.

`FIN-DEC-094` membalik arah itu atas dasar yang lebih kuat daripada preferensi: bentuk V1 **sudah
diuji dan disetujui pengguna lewat UAT**, dan pengguna yang sama akan memakai sistem ini. Karena
itu setiap kemampuan yang di V1 berdiri sebagai halaman sendiri **MUST** berdiri sebagai halaman
sendiri di sini juga.

Yang **tidak** ikut berubah, dan ini penting supaya pemecahan tidak disalahartikan sebagai
pembongkaran:

| Tetap seperti sekarang | Alasan |
|---|---|
| Seluruh endpoint, aturan bisnis, dan alur persetujuan | Pemecahan ini soal keterjangkauan layar, bukan soal aturan |
| Layar konsolidasi yang sudah ada | **Tidak dihapus.** Ia tetap menjadi jalan masuk lengkap; halaman berdiri sendiri adalah jalan masuk **tambahan** yang terfokus |
| Nol perhitungan uang di klien | Berlaku penuh, termasuk pada selisih klaim |

### 17.2 Peta butir menu — Keuangan

Berkas yang disunting saat implementasi: `src/utils/menu-sidebar/menu-items.jsx`. Resolver membaca
`subMenu` pada tingkat 0 dan `subItems` pada grup di bawahnya.

```text
Keuangan                                     <- tingkat 0
├── Master Data                              <- grup, tidak berubah
├── Transaksi A/R                            <- grup, BENTUK BARU
│   ├── Tagihan/Billing
│   ├── Receivable AR/Invoice
│   ├── Ayat Silang
│   ├── Canceled Invoice
│   ├── Report Canceled Invoice
│   ├── Receiveable AR Canceled
│   ├── Report Receiveable AR
│   ├── Report Payment AR
│   ├── Settlement AR
│   ├── Report Closed Billing
│   ├── Report AR
│   ├── Report AR Created
│   ├── Laporan Aging AR
│   ├── Piutang Korporat/Penjamin
│   ├── Manajemen Klaim
│   ├── Umur Piutang (A/R Aging)             <- GRUP (FIN-OQ-040 terjawab)
│   │   ├── Kasir                            -> dapat dikerjakan
│   │   ├── Parkir                           -> TERTAHAN FIN-OQ-043
│   │   └── Tenant                           -> TERTAHAN FIN-OQ-043
│   ├── Pemutihan Piutang
│   └── Piutang Tagihan
├── Transaksi A/P                            <- grup, BENTUK BARU
│   ├── Pembelian Pesanan
│   ├── Penerima Pesanan
│   ├── Retur Produk
│   ├── Tukar Faktur
│   ├── Purchasing Invoice
│   ├── Laporan Aging AP
│   ├── Retur Pembelian Supplier
│   ├── Purchasing Payment
│   ├── Rekap Purchasing AP
│   ├── Laporan Tukar Faktur
│   ├── Laporan Jatuh Tempo
│   ├── Laporan Pembayaran AP
│   ├── Penerimaan Invoice
│   ├── Utang Usaha (A/P Aging)
│   └── Rekonsiliasi Tagihan
├── Manajemen Kas                            <- tidak berubah
└── Pemantauan                               <- tidak berubah
```

**"Jasa Medis" sengaja tidak dibuat.** Butir itu ada pada menu V1, tetapi kepemilikannya sudah
diputuskan milik modul Medical Fee (`FIN-OQ-012`/`FIN-OQ-013`) — membuatnya di sini berarti
membuka kembali keputusan yang sudah tertutup.

#### Tabel butir menu — Transaksi A/R

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status layar |
|---|:---:|---|---|---|---|---|
| Transaksi A/R | 1 | Keuangan | — | — | — | Baru (grup) |
| Tagihan/Billing | 2 | Transaksi A/R | `/finance/receivable-invoice-batches` | `FIN-LYR-AR-01` | `FinanceReceivableInvoiceBatch : Read` | **Sudah ada** (`FE-FIN-012`) |
| Receivable AR/Invoice | 2 | Transaksi A/R | `/finance/receivable` | `FIN-LYR-AR-02` | `Finance.AR : View` *(dikoreksi `FE-FIN-017`, lihat laporan §3.3 — bukan `FinanceReceivable : Read` seperti tertulis sebelumnya: halaman ini memanggil `FinanceArController` V2 legacy, bukan `FinanceReceivablesController` governed)* | **Sudah ada** (`FE-FIN-002`) |
| Ayat Silang | 2 | Transaksi A/R | `/finance/receipts?debtorType=PAYER` | `FIN-LYR-AR-03` | `FinanceReceipt : Read` | **Sudah ada**, pandangan tersaring (`FIN-DEC-096`) |
| Canceled Invoice | 2 | Transaksi A/R | `/finance/receivable-invoice-batches/canceled` | `FIN-LYR-AR-04` | `FinanceReceivableInvoiceBatch : Read` | **Baru** — pandangan tersaring |
| Report Canceled Invoice | 2 | Transaksi A/R | `/finance/ar-report/canceled-invoice` | `FIN-LYR-AR-05` | `FinanceReceivableInvoiceBatch : Read` | **Baru** — pandangan tersaring |
| Receiveable AR Canceled | 2 | Transaksi A/R | `/finance/receipts/reversed-allocations` | `FIN-LYR-AR-06` | `FinanceReceipt : Read` | **Baru** — butuh endpoint baru |
| Report Receiveable AR | 2 | Transaksi A/R | `/finance/ar-report/receivable` | `FIN-LYR-AR-07` | `FinanceReceivable : Read` | **Baru** — pandangan tersaring |
| Report Payment AR | 2 | Transaksi A/R | `/finance/ar-report/payment` | `FIN-LYR-AR-08` | `FinanceReceipt : Read` | **Baru** — pandangan tersaring atas `GET /receipts/register` |
| Settlement AR | 2 | Transaksi A/R | `/finance/receipts` | `FIN-LYR-AR-09` | `FinanceReceipt : Read` | **Sudah ada** (`FE-FIN-004`) |
| Report Closed Billing | 2 | Transaksi A/R | `/finance/ar-report/closed-billing` | `FIN-LYR-AR-10` | `FinanceReceivable : Read` | **Baru** — pandangan tersaring |
| Report AR | 2 | Transaksi A/R | `/finance/ar-report` | `FIN-LYR-AR-11` | `Finance.AR : View` | **Sudah ada** |
| Report AR Created | 2 | Transaksi A/R | `/finance/ar-report/created` | `FIN-LYR-AR-12` | `FinanceReceivable : Read` | **Baru** — pandangan tersaring |
| Laporan Aging AR | 2 | Transaksi A/R | `/finance/ar-aging` | `FIN-LYR-AR-13` | `Finance.AR : View` | **Sudah ada** |
| Piutang Korporat/Penjamin | 2 | Transaksi A/R | `/finance/receivable/corporate` | `FIN-LYR-AR-14` | `FinanceReceivable : Read` | **Baru** — pandangan tersaring |
| Manajemen Klaim | 2 | Transaksi A/R | `/finance/receivable-invoice-batches/claims` | `FIN-LYR-AR-15` | `FinanceReceivableInvoiceBatch : Read` | **Baru — kapabilitas baru** |
| Umur Piutang (A/R Aging) | 2 | Transaksi A/R | — (grup) | — | — | Grup; isinya terjawab `FIN-OQ-040` |
| Umur Piutang — Kasir | 3 | Umur Piutang (A/R Aging) | `/finance/receivable/aging` | `FIN-LYR-AR-18` | `FinanceReceivable : Read` | **Baru** — `GET /receivables/aging` dipanggil **tanpa saringan** (lihat koreksi `BE-FIN-055` §3.3: tidak ada nilai "KASIR" pada `FinReceivable.DebtorType`; "Kasir" = seluruh `FinReceivable` sejak Parkir/Tenant dipisah ke `FinNonPatientReceivable`) |
| Umur Piutang — Parkir | 3 | Umur Piutang (A/R Aging) | — | — | — | **TERTAHAN `FIN-OQ-043`** — sumber piutangnya belum ada |
| Umur Piutang — Tenant | 3 | Umur Piutang (A/R Aging) | — | — | — | **TERTAHAN `FIN-OQ-043`** — sumber piutangnya belum ada |
| Pemutihan Piutang | 2 | Transaksi A/R | `/finance/receivable/write-offs` | `FIN-LYR-AR-16` | `FinanceReceivable : Read` | **Baru** — butuh endpoint baru |
| Piutang Tagihan | 2 | Transaksi A/R | `/finance/receivable?view=billed` | `FIN-LYR-AR-17` | `Finance.AR : View` *(dikoreksi `FE-FIN-017`, sama alasan dengan `FIN-LYR-AR-02` — halaman sama, `/finance/receivable`)* | **Sudah ada**, pandangan tersaring (lihat `FIN-OQ-041`). **Catatan:** parameter `?view=billed` saat ini **inert** — `finance-receivable-view.jsx` tidak membacanya sama sekali (temuan `FE-FIN-016`/`017`) |

#### Tabel butir menu — Transaksi A/P

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status layar |
|---|:---:|---|---|---|---|---|
| Transaksi A/P | 1 | Keuangan | — | — | — | Baru (grup) |
| Pembelian Pesanan | 2 | Transaksi A/P | `/finance/purchasing/purchase-orders` | `FIN-LYR-AP-01` | `FinancePurchaseOrder : Read` | **Sudah ada** (`FE-FIN-008`) |
| Penerima Pesanan | 2 | Transaksi A/P | `/finance/purchasing/goods-receipts` | `FIN-LYR-AP-02` | `FinanceGoodsReceipt : Read` | **Baru** — endpoint sudah ada |
| Retur Produk | 2 | Transaksi A/P | `/finance/purchasing/supplier-returns/items` | `FIN-LYR-AP-03` | `FinanceSupplierReturn : Read` | **Baru** — pandangan per baris barang |
| Tukar Faktur | 2 | Transaksi A/P | `/finance/purchasing/invoice-exchanges` | `FIN-LYR-AP-04` | `FinanceInvoiceExchange : Read` | **Sudah ada** (`FE-FIN-008`) |
| Purchasing Invoice | 2 | Transaksi A/P | `/finance/purchasing/purchasing-invoices` | `FIN-LYR-AP-05` | `FinancePurchasingInvoice : Read` | **Sudah ada** (`FE-FIN-009`) |
| Laporan Aging AP | 2 | Transaksi A/P | `/finance/ap-aging` | `FIN-LYR-AP-06` | `Finance.AP : View` | **Sudah ada** |
| Retur Pembelian Supplier | 2 | Transaksi A/P | `/finance/purchasing/supplier-returns` | `FIN-LYR-AP-07` | `FinanceSupplierReturn : Read` | **Sudah ada** (`FE-FIN-009`) |
| Purchasing Payment | 2 | Transaksi A/P | `/finance/payment-ap` | `FIN-LYR-AP-08` | `FinancePayment : Read` | **Sudah ada** (`FE-FIN-010`) |
| Rekap Purchasing AP | 2 | Transaksi A/P | `/finance/purchasing/reports/summary` | `FIN-LYR-AP-09` | `FinancePurchasingReport : Read` | **Baru** — pecahan dari tab |
| Laporan Tukar Faktur | 2 | Transaksi A/P | `/finance/purchasing/reports/invoice-exchanges` | `FIN-LYR-AP-10` | `FinancePurchasingReport : Read` | **Baru** — pecahan dari tab |
| Laporan Jatuh Tempo | 2 | Transaksi A/P | `/finance/purchasing/reports/due-dates` | `FIN-LYR-AP-11` | `FinancePurchasingReport : Read` | **Baru** — pecahan dari tab |
| Laporan Pembayaran AP | 2 | Transaksi A/P | `/finance/payable/report` | `FIN-LYR-AP-12` | `Finance.AP : View` | **Sudah ada** |
| Penerimaan Invoice | 2 | Transaksi A/P | `/finance/purchasing/purchasing-invoices?stage=intake` | `FIN-LYR-AP-13` | `FinancePurchasingInvoice : Read` | **Sudah ada**, pandangan tersaring |
| Utang Usaha (A/P Aging) | 2 | Transaksi A/P | `/finance/payable/invoice` | `FIN-LYR-AP-14` | `Finance.AP : View` | **Sudah ada** |
| Rekonsiliasi Tagihan | 2 | Transaksi A/P | `/finance/purchasing/reports/reconciliation` | `FIN-LYR-AP-15` | `FinancePurchasingReport : Read` | **Baru** — pecahan dari tab |

**Catatan hak akses.** Lima butir masih dijaga resource payung V2 (`Finance.AR : View`,
`Finance.AP : View`) karena layar tujuannya memang milik controller V2 — itu benar, bukan
kelalaian. Butir baru seluruhnya memakai resource granular yang sama persis dengan endpointnya
(`FIN-DES-069`, tetap berlaku). Nol resource dan nol action baru dibutuhkan (`FIN-DES-072`).

### 17.3 Skema fitur — layar baru yang penting

#### `FIN-LYR-AR-15` Manajemen Klaim — satu-satunya kapabilitas yang benar-benar baru

```text
+- Manajemen Klaim Penjamin ------------------------------ FIN-LYR-AR-15 -+
| [cari no. tagihan / no. klaim]  [Penjamin v] [Status Klaim v] [Periode v]|
+--------------------------------------------------------------------------+
| No. Tagihan | Penjamin | Periode | Total | Disetujui | Selisih | Klaim |  |
| ----------- | -------- | ------- | ----- | --------- | ------- | chip  |  |
|             |          |         |       |           |         |       |[Detail]
+--------------------------------------------------------------------------+
| memuat -> kerangka baris                                                  |
| kosong -> "Belum ada tagihan penjamin pada saringan ini."    [Atur ulang] |
| gagal  -> "Data gagal dimuat."                               [Coba lagi]  |
+- Halaman 1 dari n ------------------------- [< Sebelumnya] [Berikutnya >] +
```

```text
+- Klaim BATCH-2026-000045 - BPJS Kesehatan --------------- FIN-LYR-AR-15 -+
| Tagihan Rp 120.000.000   Disetujui Rp 118.500.000   Selisih Rp 1.500.000 |
| Status tagihan: Dibayar Sebagian    Status klaim: Disetujui              |
| [Tandai Diverifikasi] [Catat Persetujuan] [Tutup Klaim]                  |
+--------------------------------------------------------------------------+
| Anggota tagihan (piutang) + sisa masing-masing                           |
| Selisih yang belum dihapus dari buku -> [Buka piutangnya]                |
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar | Nomor tagihan, penjamin, periode, total, nominal disetujui, selisih, status klaim | `GET /receivable-invoice-batches` (response bertambah field klaim) | `FinanceReceivableInvoiceBatch : Read` | Kosong: "Belum ada tagihan penjamin pada saringan ini." Gagal: seluruh daftar diganti pesan beserta tombol coba lagi |
| Kepala rincian | Total tagihan, nominal disetujui, selisih, **dua status berdampingan** | `GET /receivable-invoice-batches/{id}` | `FinanceReceivableInvoiceBatch : Read` | Gagal: seluruh layar diganti pesan beserta tombol coba lagi |
| Tombol aksi | Tandai Diverifikasi, Catat Persetujuan, Tutup Klaim | `POST /{id}/claim/verify`, `/claim/approve`, `/claim/close` | `FinanceReceivableInvoiceBatch : Update` | Tombol yang tidak berhak **MUST** disembunyikan. Tombol yang tidak sah pada status saat ini **MUST** dinonaktifkan, bukan disembunyikan — supaya petugas tahu langkahnya ada tetapi belum waktunya |
| Daftar anggota | Piutang anggota beserta sisa masing-masing | `GET /receivable-invoice-batches/{id}` | `FinanceReceivableInvoiceBatch : Read` | Kosong tidak mungkin terjadi — batch wajib punya minimal satu anggota |
| Panel selisih | Nominal selisih dan tautan ke piutang yang perlu dihapus | `ClaimVarianceAmount` dari response | `FinanceReceivable : Read` untuk tautannya | Kosong bila disetujui penuh: "Penjamin menyetujui seluruh nilai tagihan." |

**Dua status ditampilkan berdampingan, bukan digabung menjadi satu chip.** Ini turunan langsung
`FIN-DES-070`: sumbu dokumen/pelunasan dan sumbu jawaban penjamin bergerak sendiri-sendiri, dan
petugas perlu melihat keduanya sekaligus — "penjamin sudah setuju tetapi uang belum masuk" adalah
keadaan yang paling sering ditindaklanjuti.

**Yang MUST NOT ada di layar ini:** tombol yang menghapus selisih langsung dari sini. Penghapusan
piutang tetap lewat jalur write-off beserta maker-checker-nya (`FIN-DES-071`); layar ini hanya
menautkan ke sana.

#### Layar pandangan tersaring — digambar sekali, dipakai sembilan layar

Sembilan layar baru (`FIN-LYR-AR-04`, `05`, `07`, `08`, `10`, `12`, `14`, `AP-09`, `AP-10`,
`AP-11`, `AP-15`) berbagi satu bentuk: **daftar bersaring, baca saja**, memakai endpoint yang sudah
ada dengan saringan bawaan yang terkunci layar.

```text
+- <judul layar> ------------------------------------------- <ID layar> -+
| saringan bawaan terkunci (tidak dapat diubah pengguna)                 |
| [saringan bebas: periode, penjamin/supplier, cari]                     |
+------------------------------------------------------------------------+
| kolom mengikuti response endpoint sumbernya                            |
+------------------------------------------------------------------------+
| memuat / kosong / gagal -> sama seperti layar daftar lain              |
+- Halaman 1 dari n -------------------- [< Sebelumnya] [Berikutnya >]   +
```

| Layar | Endpoint sumber | Saringan bawaan yang terkunci |
|---|---|---|
| `FIN-LYR-AR-04` Canceled Invoice | `GET /receivable-invoice-batches` | `Status = CANCELLED` |
| `FIN-LYR-AR-05` Report Canceled Invoice | `GET /receivable-invoice-batches` | `Status = CANCELLED`, tampilan laporan |
| `FIN-LYR-AR-07` Report Receiveable AR | `GET /receivables` | — (laporan penuh) |
| `FIN-LYR-AR-08` Report Payment AR | `GET /receipts/register` | — |
| `FIN-LYR-AR-10` Report Closed Billing | `GET /receivables` | `Status = SETTLED` |
| `FIN-LYR-AR-12` Report AR Created | `GET /receivables` | diurutkan tanggal terbit |
| `FIN-LYR-AR-14` Piutang Korporat/Penjamin | `GET /receivables` | `DebtorType = PAYER` |
| `FIN-LYR-AP-09`/`10`/`11`/`15` | Keempat endpoint `purchasing/reports` | — (endpointnya memang sudah terpisah) |

**Aturan yang mengikat seluruhnya:** saringan bawaan **MUST** dikirim sebagai parameter ke backend,
**MUST NOT** disaring di klien sesudah data diterima. Menyaring di klien membuat paginasi dan
jumlah baris menjadi salah.

#### `FIN-LYR-AR-06` Receiveable AR Canceled dan `FIN-LYR-AR-16` Pemutihan Piutang

Keduanya berbentuk daftar bersaring yang sama, tetapi memakai **endpoint baru**
(`GET /receipts/reversed-allocations` dan `GET /receivables/write-offs`). Keduanya **baca saja**:
pembalikan alokasi dan pembuatan write-off tetap dilakukan dari layar asalnya beserta jenjang
persetujuannya.

#### `FIN-LYR-AP-02` Penerima Pesanan

Daftar tanda terima barang berdiri sendiri, memakai `GET /goods-receipts` yang **sudah ada**.
Pencatatan tanda terima baru **tetap** dilakukan dari detail Purchase Order seperti sekarang —
layar ini menambah jalan masuk untuk melihat dan menelusuri, bukan memindahkan alur pencatatannya.

### 17.4 Kewenangan UI

| Hal | Wewenang | Dasar |
|---|---|---|
| Layar mana yang berdiri sendiri | **Mengikat** | `FIN-DEC-094` — bentuk V1 sudah lolos UAT |
| Butir menu mana yang ada, dan rutenya | **Mengikat** | `FIN-DEC-094` beserta tabel 17.2 |
| Label butir menu | **Mengikat** | Disalin apa adanya dari menu V1, termasuk ejaan "Receiveable" dan campuran Indonesia/Inggris — supaya pengguna lama mengenalinya tanpa belajar ulang |
| Urutan butir di dalam grup | **Mengikat** | Mengikuti urutan tangkapan layar V1 |
| Dua status klaim ditampilkan berdampingan | **Mengikat** | `FIN-DES-070` — ini soal kebenaran informasi, bukan rupa |
| Saringan bawaan dikirim ke backend | **Mengikat** | Kebenaran paginasi |
| Tata letak, warna, ikon, jarak, component library | `DEV_DISCRETION` | — |
| Bentuk kontrol aksi klaim (modal konfirmasi, drawer, atau form inline) | `DEV_DISCRETION` | Yang dikunci hanya keberadaan aksinya dan hak akses penjaganya |
| Bentuk tampilan laporan (tabel, kartu ringkas, atau keduanya) | `DEV_DISCRETION` | — |

### 17.5 Pertanyaan terbuka yang lahir dari pass ini

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| ~~`FIN-OQ-040`~~ | ~~Apa saja butir di dalam grup "Umur Piutang (A/R Aging)"?~~ **TERJAWAB 1 Oktober 2026: Kasir, Parkir, Tenant.** | Yasmin (Product Owner Finance) | **CLOSED** — digantikan `FIN-OQ-043` untuk dua dari tiga butirnya |
| `FIN-OQ-043` | Piutang **Parkir** dan **Tenant** adalah penagihan sewa berulang, bukan tagihan pasien. Model piutang yang berjalan **tidak dapat menampungnya**: `FinReceivable` hanya mengenal `PAYER`/`PATIENT_GUARANTOR`/`EMPLOYEE_BENEFIT`, dan setiap barisnya wajib berasal dari serah terima tagihan Billing (`SourceHandoffKey`, `InvoiceId`). Layar V1-nya pun murni data contoh, nol panggilan API. Lima hal MUST diputuskan: kepemilikan modul, data induk kontrak/objek sewa/tarif, cara penerbitan tagihan berulang, aturan denda keterlambatan, dan bentuk penyimpanan piutangnya | Yasmin (Product Owner Finance) | **MEMBLOKIR dua butir saja** (Parkir, Tenant). Butir Kasir dan seluruh isi `EPIC FIN-18` lainnya berjalan terus. Rinciannya pada `00-interview-decisions.md` addendum 1 Oktober 2026 |
| ~~`FIN-OQ-041`~~ **CLOSED 1 Oktober 2026 oleh `FIN-DEC-107`: keempat pasang adalah layar berbeda, seluruhnya dipertahankan.** | Empat pasang butir V1 tampak mengarah ke kemampuan yang sama: (a) "Laporan Aging AR" dan "Umur Piutang (A/R Aging)"; (b) "Receivable AR/Invoice" dan "Piutang Tagihan"; (c) "Retur Produk" dan "Retur Pembelian Supplier"; (d) "Ayat Silang" dan "Settlement AR" (`FIN-DEC-096` menyatakan keduanya dilayani alokasi penerimaan). Apakah keempat pasang itu memang dua layar berbeda di V1, atau salah satunya peninggalan yang sebaiknya tidak dibawa? | Yasmin (Product Owner Finance) | **Tidak memblokir.** Sementara belum dijawab, keduanya dibuat sesuai tabel 17.2 dengan saringan bawaan yang berbeda, mengikuti perintah "ikuti V1 apa adanya" |
| ~~`FIN-OQ-042`~~ **CLOSED 1 Oktober 2026 oleh `FIN-DEC-108`: kolom dipertahankan sebagai kolom opsional.** | Kolom `PayerClaimReference` (nomor rujukan klaim milik penjamin) **tidak** disebut pada `FIN-DEC-097`; ia kesimpulan desain karena pelacakan klaim tanpa nomor rujukan penjamin sulit dicocokkan saat berkorespondensi. Apakah kolom ini memang dibutuhkan? | Yasmin (Product Owner Finance) | **Tidak memblokir.** Kolomnya opsional dan tidak membawa aturan bisnis; bila ditolak, cukup dihapus dari migration sebelum dijalankan |

---

## 18. Amendment 1 Oktober 2026 (revisi 13, lanjutan) — layar piutang sewa non-pasien

| Field | Nilai |
|---|---|
| Keputusan yang diturunkan | `FIN-DEC-099`..`FIN-DEC-104` |
| Rancangan backend | `FIN-DES-074`..`FIN-DES-077`, `02-backend-architecture.md` bagian `K` |
| Menutup | `FIN-OQ-043` — butir "Umur Piutang — Parkir" dan "— Tenant" kini dapat dikerjakan |
| Wewenang UI | Keberadaan layar dan butir menunya **mengikat** (`FIN-DEC-094` beserta tangkapan layar V1). Tata letak, warna, ikon, dan bentuk kontrol tetap `DEV_DISCRETION` |

### 18.1 Butir menu yang terbuka kembali

Menggantikan tiga baris `TERTAHAN FIN-OQ-043` pada tabel bagian 17.2:

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status layar |
|---|:---:|---|---|---|---|---|
| Umur Piutang — Kasir | 3 | Umur Piutang (A/R Aging) | `/finance/receivable/aging` | `FIN-LYR-AR-18` | `FinanceReceivable : Read` | **Baru** — `GET /receivables/aging` tanpa saringan (lihat koreksi `BE-FIN-055` §3.3) |
| Umur Piutang — Parkir | 3 | Umur Piutang (A/R Aging) | `/finance/non-patient-receivables/aging?category=PARKING` | `FIN-LYR-AR-19` | `FinanceNonPatientReceivable : Read` | **Baru** |
| Umur Piutang — Tenant | 3 | Umur Piutang (A/R Aging) | `/finance/non-patient-receivables/aging?category=TENANT` | `FIN-LYR-AR-20` | `FinanceNonPatientReceivable : Read` | **Baru** |

Dua layar kerja berikut **tidak** mendapat butir menu sendiri pada bentuk V1, dan karena itu
dinyatakan sebagai **layar anak** beserta jalan masuknya — tanpa pernyataan ini, keduanya hanya
dapat dibuka lewat URL langsung dan dihitung belum selesai:

| Layar | ID | Jalan masuk | Alasan tidak mendapat butir menu |
|---|---|---|---|
| Daftar tagihan sewa (catat, koreksi, lunasi, hapus) | `FIN-LYR-AR-21` | Tombol pada layar umur piutang Parkir/Tenant | Menu V1 hanya memuat laporan umurnya; pencatatan tagihan tidak punya butir sendiri di sana |
| Rincian satu tagihan sewa beserta riwayat pelunasan | `FIN-LYR-AR-22` | Baris pada `FIN-LYR-AR-21` | Layar rincian memang selalu anak dari daftarnya |

**Catatan yang MUST disampaikan ke pemilik saat approval:** bentuk V1 hanya menyediakan jalan masuk
lewat laporan umur piutang. Bila petugas diharapkan mencatat tagihan sewa setiap periode, jalan
masuk lewat laporan terasa berputar. Menambah butir menu "Tagihan Sewa" tersendiri adalah
penyimpangan dari V1 dan **MUST** diputuskan pemilik, bukan ditambahkan agent atas nama kenyamanan.

### 18.2 Skema fitur

#### `FIN-LYR-AR-19` dan `FIN-LYR-AR-20` — Umur piutang sewa

Keduanya satu layar yang sama dengan saringan kategori berbeda; digambar sekali.

```text
+- Umur Piutang Sewa - <Parkir|Tenant> ------------------ FIN-LYR-AR-19/20 -+
| kategori terkunci sesuai butir menu     [Per tanggal v]                   |
+---------------------------------------------------------------------------+
| 0-30 hari | 31-60 hari | 61-90 hari | di atas 90 hari | Total             |
| Rp ...    | Rp ...     | Rp ...     | Rp ...          | Rp ...            |
+---------------------------------------------------------------------------+
| Rincian tagihan pada kelompok terpilih                                    |
| No. Tagihan | Penyewa | Objek Sewa | Periode | Jatuh Tempo | Sisa |        |
+---------------------------------------------------------------------------+
| memuat -> kerangka baris                                                  |
| kosong -> "Belum ada piutang sewa pada saringan ini."       [Atur ulang]   |
| gagal  -> "Data gagal dimuat."                              [Coba lagi]    |
|                                      [Kelola Tagihan Sewa] -> FIN-LYR-AR-21|
+---------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kelompok umur | Empat kelompok beserta nominalnya | `GET /non-patient-receivables/aging` | `FinanceNonPatientReceivable : Read` | Gagal: seluruh layar diganti pesan beserta tombol coba lagi |
| Rincian tagihan | Daftar tagihan pada kelompok terpilih | `GET /non-patient-receivables` | `FinanceNonPatientReceivable : Read` | Kosong: "Belum ada piutang sewa pada saringan ini." |
| Tombol Kelola Tagihan Sewa | Jalan masuk ke layar pencatatan | — | `FinanceNonPatientReceivable : Read` | Disembunyikan bila tidak berhak |

Saringan kategori **MUST** dikirim ke backend sebagai parameter, **MUST NOT** disaring di klien.

#### `FIN-LYR-AR-21` — Daftar dan pencatatan tagihan sewa

```text
+- Tagihan Sewa ------------------------------------------ FIN-LYR-AR-21 -+
| [cari penyewa / objek sewa]  [Kategori v] [Status v] [Periode v]        |
|                                                   [+ Catat Tagihan]     |
+-------------------------------------------------------------------------+
| No. | Kategori | Penyewa | Objek | Periode | Tagihan | Denda | Sisa |    |
|     | chip     |         |       |         |         |       |     |[...]|
+-------------------------------------------------------------------------+
| memuat / kosong / gagal -> pola layar daftar yang berlaku di modul ini  |
+- Halaman 1 dari n ---------------------- [< Sebelumnya] [Berikutnya >]  +
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar | Nomor, kategori, penyewa, objek sewa, periode, nominal, denda, sisa, status | `GET /non-patient-receivables` | `FinanceNonPatientReceivable : Read` | Kosong: "Belum ada tagihan sewa pada saringan ini." |
| Tombol Catat Tagihan | Membuka isian tagihan baru | `POST /non-patient-receivables` | `FinanceNonPatientReceivable : Create` | Disembunyikan bila tidak berhak |
| Aksi per baris | Koreksi, Catat Pelunasan, Hapus Piutang, Batalkan | `PUT /{id}`, `POST /{id}/settlements`, `/write-off`, `/cancel` | `FinanceNonPatientReceivable : Update` | Aksi yang tidak sah pada status saat ini **MUST** dinonaktifkan, bukan disembunyikan — supaya petugas tahu langkahnya ada tetapi belum waktunya |

**Dua hal yang MUST ada di layar ini, dan keduanya soal kejujuran kepada petugas:**

1. Tombol **Hapus Piutang** dan **Batalkan** **MUST** memakai konfirmasi yang menyebut terang bahwa
   tindakan ini **tidak melewati persetujuan siapa pun** dan alasannya wajib diisi. Ini satu-satunya
   penahan yang tersisa setelah `FIN-DEC-103` meniadakan jenjang approval.
2. Layar **MUST** menyatakan bahwa pelunasan yang dicatat di sini **tidak** tercatat sebagai kas
   masuk di kas harian maupun setoran bank, karena piutang sewa dikelola terpisah (`FIN-DEC-109`;
   sebelumnya "belum", sebelum diputuskan). Membiarkan petugas
   menyangka uangnya sudah tercatat di kas adalah kekeliruan yang mahal dan sulit ditelusuri
   belakangan.

Bunyi persis kedua pernyataan itu `DEV_DISCRETION`; **keberadaannya** mengikat.

#### `FIN-LYR-AR-22` — Rincian tagihan sewa

Bentuknya mengikuti pola layar rincian yang sudah berlaku di modul ini: kepala berisi identitas dan
angka tagihan, diikuti daftar riwayat pelunasan dari
`GET /non-patient-receivables/{id}`. Pelunasan bernilai minus **MUST** terbaca jelas sebagai
pembatalan pembayaran, bukan sebagai pembayaran biasa.

### 18.3 Kewenangan UI

| Hal | Wewenang | Dasar |
|---|---|---|
| Keberadaan ketiga butir menu umur piutang | **Mengikat** | `FIN-DEC-094`, tangkapan layar V1 |
| Saringan kategori dikirim ke backend | **Mengikat** | Kebenaran angka dan paginasi |
| Konfirmasi yang menyebut ketiadaan persetujuan pada Hapus/Batalkan | **Mengikat** | `FIN-DEC-103` meniadakan penahan lain |
| Pernyataan bahwa pelunasan sewa tidak masuk kas harian/setoran bank | **Mengikat** (permanen) | `FIN-DES-075`, `FIN-DEC-109`, `FIN-DEC-110` |
| Butir menu tersendiri untuk "Tagihan Sewa" | **Menunggu keputusan pemilik** | Penyimpangan dari V1 — lihat 18.1 |
| Tata letak, warna, ikon, bentuk kontrol, bunyi kalimat | `DEV_DISCRETION` | — |

---

## 19. Amendment 1 Oktober 2026 (revisi 14) — buku mutasi, cutover, dan pembayaran langsung berkontrol

| Field | Nilai |
|---|---|
| Diturunkan dari | `FIN-DEC-111`..`137`; `FIN-DES-078`..`091` |
| Frontend SHA yang diaudit | `0b54fdce6` — **bergerak** dari `85578363b`; pass ini **tidak** mengaudit ulang frontend secara menyeluruh |
| Sifat | **Ada perubahan memutus** pada dua layar yang sudah berjalan |
| Wewenang UI | Hierarki yang berlaku: keamanan dan invariant → brief produk yang disetujui → konvensi project → `DEV_DISCRETION` |

### 19.1 Peringatan: dua layar yang sudah berjalan akan rusak bila backend naik lebih dulu

Ini bagian terpenting pada amandemen frontend ini, dan karena itu ditulis lebih dulu.

| Layar | Endpoint | Apa yang rusak |
|---|---|---|
| Pembayaran langsung piutang | `POST /receivables/{id}/payment` | `PaymentMethod` menjadi **wajib**, ditambah sumber dana, nomor rujukan, dan `ProofId` wajib. Permintaan lama akan dijawab `400`/`422` |
| Pembayaran langsung utang supplier | `POST /supplier-payables/{id}/direct-payment` | Ruas yang sama menjadi wajib |

Keduanya juga mulai ditolak `404` bila ambang belum ditetapkan, dan `422` bila nominalnya melewati
ambang. Jadi urutan rilisnya **MUST** dijaga: layar disesuaikan **sebelum atau bersamaan** dengan
backend, **tidak** sesudahnya. Bila tidak, petugas kehilangan kemampuan mencatat pembayaran yang
selama ini berjalan.

### 19.2 Kebutuhan fungsional per layar

| # | Layar | Sifat | Kebutuhan |
|---:|---|---|---|
| 1 | **Pembayaran langsung piutang** | **Diperbarui** | Pilihan metode wajib; pilihan rekening sumber bila transfer; kolom nomor rujukan; unggah bukti wajib; peringatan bila nominal melewati ambang **sebelum** dikirim |
| 2 | **Pembayaran langsung utang supplier** | **Diperbarui** | Sama dengan nomor 1 |
| 3 | **Riwayat mutasi piutang** | **Baru** | Daftar mutasi satu piutang berurut tanggal: jenis, nominal bertanda, saldo sebelum dan sesudah, metode, bukti |
| 4 | **Riwayat mutasi utang supplier** | **Baru** | Sama dengan nomor 3 |
| 5 | **Buku kas** | **Baru** | Mutasi kas bersaring tanggal, arah, dan jenis; menampilkan posisi berjalan |
| 6 | **Rekap kas harian** | **Diperbarui** | Menambah keterangan bahwa angkanya **laporan operasional**, bukan angka yang dikirim ke Accounting; menampilkan selisih terhadap posisi terhitung |
| 7 | **Pemetaan akun control** | **Baru** | Daftar kelompok dan segmen beserta kode akunnya; penanda kelompok yang **belum** terpetakan |
| 8 | **Saldo awal cutover** | **Baru** | Lima kelompok; alur catat, setujui, kunci; kolom alasan dan rujukan dokumen Accounting wajib |
| 9 | **Batch migrasi tagihan lama** | **Baru** | Unduh templat, unggah berkas, lihat galat per baris, nyatakan saldo awal Accounting, setujui |
| 10 | **Ambang pembayaran langsung** | **Baru** | Satu nilai beserta alasan perubahan wajib |
| 11 | **Snapshot saldo subledger** (bagian 16) | **Diperbarui** | Jumlah baris tidak lagi tetap empat; nilai negatif ditampilkan apa adanya; pesan penolakan gagal tertutup ditampilkan beserta kelompok dan segmennya |

### 19.3 Aksi per peran

| Peran | Boleh | Tidak boleh |
|---|---|---|
| Staf AR / AP | Mencatat pembayaran langsung di bawah ambang beserta bukti; melihat riwayat mutasi dan buku kas | Mengubah ambang; menyetujui saldo awal; menyetujui batch migrasi |
| Petugas kas | Menutup rekap kas harian; melihat buku kas dan selisih | Mengubah pemetaan akun control |
| Penyiap cutover | Mencatat pemetaan akun control, saldo awal, dan mengunggah batch migrasi | **Menyetujui** dan **mengunci** keduanya, bila haknya tidak mencakup `Approve` |
| Penyetuju cutover | Menyetujui dan mengunci saldo awal serta batch migrasi | — |
| Pejabat berwenang ambang | Mengubah ambang beserta alasan | — |

> **Batas yang MUST disampaikan saat menyerahkan modul.** Mesin hak akses **tidak** mencegah satu
> orang memegang `Create` dan `Approve` sekaligus. Pemisahan penyiap dan penyetuju bergantung pada
> **pemberian hak oleh admin**, bukan pada kode. Layar **MUST NOT** menyiratkan pemisahan itu
> dijamin sistem.

### 19.4 Data, status, dan galat yang dikonsumsi

| Keadaan | Yang ditampilkan |
|---|---|
| Ambang belum ditetapkan (`404`) | Layar pembayaran langsung **dinonaktifkan** beserta keterangan bahwa ambang belum ditetapkan dan kepada siapa memintanya. **Bukan** pesan galat teknis |
| Nominal melewati ambang (`422`) | Peringatan beserta arahan memakai jalur pembayaran berjenjang, dengan tautan ke layarnya |
| Snapshot gagal tertutup (`422`) | Daftar kelompok dan segmen yang belum terpetakan, beserta tautan ke layar pemetaan |
| Posisi diminta sebelum cutover (`422`) | Keterangan bahwa buku mutasi belum berjalan pada tanggal itu — **bukan** angka nol |
| Batch migrasi bergalat per baris | Tabel baris bergalat beserta nomor baris dan alasannya; tombol setujui **dinonaktifkan** |
| Selisih rekonsiliasi batch (`422`) | **Kedua angka** ditampilkan berdampingan, bukan hanya pesan "tidak cocok" |
| Saldo negatif pada snapshot | Ditampilkan **apa adanya** beserta keterangan bahwa nilai itu berlawanan dengan saldo normal akun. **MUST NOT** disembunyikan atau ditampilkan sebagai nol |
| Baris `LOCKED` | Seluruh kendali ubah **dinonaktifkan**, bukan disembunyikan — supaya terlihat bahwa barisnya ada dan memang tidak dapat diubah |

### 19.5 Penanganan state

| Hal | Aturan |
|---|---|
| Loading | Pola yang sudah berlaku di modul ini |
| Empty | Buku mutasi kosong menampilkan keterangan bahwa buku mutasi baru berjalan sejak tanggal cutover, **bukan** "tidak ada data" |
| Error dan retry | Pola yang sudah berlaku |
| Stale | Saldo awal dan batch migrasi memakai `RowVersion`; benturan menampilkan keterangan bahwa baris sudah diubah orang lain dan meminta memuat ulang |
| Duplicate submit | Tombol setujui dan kunci **MUST** dinonaktifkan sejak permintaan dikirim. Keduanya tidak dapat ditarik |
| Unggah berkas | Satu unggahan menghasilkan satu `ProofId`; mengirim pembayaran dua kali dengan `ProofId` yang sama dijawab `409` dan **MUST** ditampilkan sebagai "bukti sudah terpakai", bukan galat teknis |

### 19.6 Privasi

| Hal | Aturan |
|---|---|
| Catatan mutasi (`Notes`) | Dapat memuat nama pihak ketiga. **MUST NOT** ditampilkan pada ringkasan atau ekspor yang dibagikan luas |
| Hasil validasi batch migrasi | Dapat memuat nama debitur dan supplier. Hanya ditampilkan pada layar batch bagi pemegang haknya |
| Berkas bukti | Diunduh lewat endpoint berhak akses. Tautan langsung ke berkas **MUST NOT** ditempelkan di layar lain |

### 19.7 Yang didelegasikan dan yang tidak

| Hal | Wewenang |
|---|---|
| Penempatan butir menu untuk tujuh layar baru | **BUKAN `DEV_DISCRETION`.** `FIN-DEC-094` mengikat menu Transaksi A/R dan A/P pada bentuk V1, dan tujuh layar ini **tidak ada** di V1. Penempatannya **MUST** diputuskan pemilik — dicatat `FIN-OQ-079` |
| Apakah layar cutover berada di menu pengaturan atau menu Finance | Turunan `FIN-OQ-079` |
| Bentuk tabel, lebar kolom, urutan kolom, penempatan saringan | `DEV_DISCRETION` |
| Tab atau modal atau drawer untuk riwayat mutasi | `DEV_DISCRETION` |
| Warna dan ikon | `DEV_DISCRETION` |
| Penandaan nilai negatif | **Bukan** `DEV_DISCRETION` pada maknanya: nilainya **MUST** terbaca negatif. Caranya menampilkan `DEV_DISCRETION` |
| Kata-kata peringatan rekap kas harian | Brief singkat diperlukan; sampai turun, pemasangannya ditahan |

### 19.8 Pertanyaan terbuka frontend

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-079` | Penempatan tujuh butir menu baru, mengingat `FIN-DEC-094` mengikat menu pada bentuk V1 yang tidak memuatnya | Yasmin | Layar baru **boleh dibangun**; penempatan menunya tertahan |
| `FIN-OQ-080` | Apakah layar pembayaran langsung perlu menampilkan ambang kepada staf AR/AP, atau cukup menolak saat melewati | Yasmin | `DESIGN` layar nomor 1 dan 2 |

### 19.9 Nol layar untuk hosted service

Ketiga hosted service **tidak** mendapat layar apa pun: tidak ada tombol "kirim sekarang" dan tidak
ada tombol "jalankan snapshot sekarang" di luar yang sudah ada. Pemicu manual sengaja tidak dibuat
supaya tidak menjadi jalan memutar gerbang `FIN-DES-078`. Keadaan pengiriman terbaca dari layar
Snapshot Saldo Subledger dan daftar kejadian akuntansi yang sudah ada.

---

## 20. Amendment 2 Oktober 2026 (revisi 15) — aturan berkas bukti dan pilihan format migrasi

| Field | Nilai |
|---|---|
| Keputusan yang diturunkan | `FIN-DEC-139`, `FIN-DEC-140` |
| Rancangan backend | `FIN-DES-092`, `FIN-DES-093`, `02-backend-architecture.md` AMENDMENT REVISI 15 |
| Layar yang terdampak | **Dua**, keduanya sudah digambar bagian 19 — tidak ada layar baru pada revisi ini: layar 1 (Pembayaran langsung piutang/utang) dan layar 9 (Batch migrasi tagihan lama) |
| Wewenang UI | Aturan berkas dan paritas format adalah **invariant**, bukan pilihan rupa. Letak kontrol, bunyi tombol, dan tata letaknya tetap `DEV_DISCRETION`. Penempatan menu tetap `FIN-OQ-079`, **belum** diputuskan |

### 20.1 Layar 1 — Pembayaran langsung: tiga akibat yang mengubah bentuk layar

| # | Akibat | Kenapa ia mengubah layar, bukan hanya pesan galat |
|---:|---|---|
| 1 | **Nol tombol "Ganti Bukti"** | `FIN-DEC-139` melarang penggantian sesudah mutasi tertulis, dan `FIN-DES-092` meniadakan endpointnya. Tombol yang memanggil endpoint yang tidak ada adalah tautan mati. Layar **MUST** menyediakan jalan yang benar: **batalkan/balik pembayarannya, lalu catat ulang** |
| 2 | **Jenis dan batas ukuran ditampilkan sebelum memilih berkas** | Petugas memotret kuitansi dengan ponsel; berkas foto modern mudah melewati batas. Memberitahu batasnya **sesudah** unggah gagal berarti menunggu unggahan besar selesai hanya untuk ditolak |
| 3 | **Keadaan "sistem belum siap" dibedakan dari "berkas Anda salah"** | `503` (`FIN-VAL-220`) berarti administrator belum mengisi batas ukuran. Layar **MUST** menampilkannya sebagai masalah konfigurasi beserta arahan menghubungi administrator — **MUST NOT** menyarankan petugas mengganti berkas |

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Pemilih berkas bukti | Satu berkas; menyebut jenis yang diterima (PDF/JPG/JPEG/PNG) dan batas ukurannya | Jenis dari kontrak; batas ukuran **tidak** dibaca layar dari konfigurasi — layar menampilkan batas yang dikembalikan backend bila tersedia, dan menahan diri bila tidak | `FinanceTransactionProof : Create` | `400` → pesan backend ditampilkan apa adanya; `413` → "Ukuran berkas melewati batas"; `503` → pesan konfigurasi beserta arahan ke administrator |
| Keterangan bukti terunggah | Nama berkas, ukuran, waktu unggah | `POST /transaction-proofs` response | `FinanceTransactionProof : Create` | — |
| Tautan unduh bukti pada riwayat mutasi | Satu tautan per mutasi yang punya `ProofId` | `GET /transaction-proofs/{id}` | `FinanceTransactionProof : Read` | Mutasi tanpa bukti menampilkan tanda hubung, bukan tautan mati |
| Aksi koreksi | **Balik pembayaran lalu catat ulang** | Jalur pembalikan yang sudah ada | Sesuai hak jalur pembalikan | — |

**Satu hal yang MUST NOT dilakukan layar.** Menyembunyikan tautan unduh bukti milik transaksi orang
lain **bukan** pengganti pembatasan hak akses. Batas per pemilik transaksi memang belum ada
(`permission-audit-matrix.md` `H.3`); menyembunyikannya di layar hanya menyamarkan keadaan sebenarnya,
karena URL unduhnya tetap dapat dipanggil langsung. Layar menampilkan apa yang backend izinkan.

### 20.2 Layar 9 — Batch migrasi: pilihan format pada dua tempat

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Unduh templat | Pilihan **jenis item** (piutang/utang) **dan format** (CSV/XLSX); keduanya wajib dipilih sebelum tombol unduh aktif | `GET /opening-item-batches/template?itemKind=&format=` | `FinanceOpeningItemBatch : Read` | `400` bila salah satu belum dipilih — dicegah di layar dengan menonaktifkan tombolnya |
| Unggah berkas | **Satu** pemilih berkas yang menerima CSV **dan** XLSX | `POST /opening-item-batches` | `FinanceOpeningItemBatch : Create` | `400` → pesan menyebut kedua format yang diterima |
| Asal berkas pada rincian batch | Menampilkan `sourceFormat` (CSV/XLSX) | `OpeningItemBatchResponse.sourceFormat` | `FinanceOpeningItemBatch : Read` | — |
| Galat per baris | Daftar galat beserta **nomor baris** | `GET /opening-item-batches/{id}` | `FinanceOpeningItemBatch : Read` | Nol galat → batch `VALIDATED` |

**Dua pemilih format, bukan satu.** Unduh templat **meminta** format dari pengguna (ia belum punya
berkas). Unggah **tidak** meminta format — format ditentukan backend dari berkasnya (`FIN-DES-093`).
Menambahkan pemilih format pada unggah berarti membuka jalan pengguna memaksa pembaca yang salah,
dan itu **MUST NOT** dibuat.

### 20.3 Penanganan state yang ditambahkan

| State | Yang dilihat pengguna |
|---|---|
| `413` pada unggah bukti | "Ukuran berkas melewati batas yang diizinkan." Berkas **tidak** terkirim ulang otomatis |
| `503` pada unggah bukti | Pesan konfigurasi belum lengkap beserta arahan menghubungi administrator. Tombol unggah **dinonaktifkan** selama keadaan itu, bukan dibiarkan dicoba berulang |
| `409` pada pembayaran karena bukti terpakai | "Bukti ini sudah dipakai pada pembayaran lain. Unggah bukti baru." — bukan galat teknis |
| Galat baris berkas migrasi | Nomor baris **selalu** ditampilkan; daftar galat dapat diunduh atau disalin supaya petugas memperbaiki berkasnya di luar sistem |

### 20.4 Privasi

| Hal | Aturan |
|---|---|
| Isi berkas bukti | **MUST NOT** masuk log klien, telemetri, maupun pesan galat yang dikirim ke pihak ketiga |
| Pratayang bukti di layar | Diizinkan bagi pemegang `Read`; **MUST NOT** disimpan ke cache yang dapat dibaca pengguna lain pada perangkat bersama |
| Hasil validasi batch | Dapat memuat nama debitur dan supplier (bertanda **Sensitif** pada kamus data). Hanya ditampilkan pada layar batch bagi pemegang haknya — tidak berubah dari 19.6 |

### 20.5 Pertanyaan terbuka frontend yang tetap terbuka

| ID | Pertanyaan | Akibat selama belum dijawab |
|---|---|---|
| `FIN-OQ-079` | Penempatan menu tujuh layar baru revisi 14, termasuk layar batch migrasi | Layar dapat dibangun; **butir menunya** belum dapat didaftarkan. **MUST NOT** diputuskan sendiri — `FIN-DEC-094` mengikat menu pada bentuk V1, dan ketujuh layar ini tidak ada di V1 |
| `FIN-OQ-080` | Apakah ambang pembayaran ditampilkan kepada staf | Layar 1 menahan diri menampilkan angka ambang; peringatan "melewati ambang" tetap ditampilkan tanpa menyebut angkanya |
| `FIN-OQ-082` | Nilai awal batas ukuran berkas | Layar **MUST** menangani `503` sebagai keadaan nyata, bukan kasus teoretis |


---

## 21. Amendment 4 Oktober 2026 (revisi 16) — penempatan menu, angka ambang bagi staf, dan empat penyesuaian layar

```yaml
input_revision: 00-interview-decisions.md — dua Amendment pass 4 Oktober 2026 (FIN-DEC-141..FIN-DEC-160)
input_design: 02-backend-architecture.md AMENDMENT REVISI 16; contracts/api-contract.md Bagian G
frontend_source_sha: ae2ed334e
status: approved — Yasmin, 4 Oktober 2026
```

**Sifat amandemen ini.** Nol layar baru. Yang ada: satu gerbang UI yang akhirnya terbuka (penempatan menu),
satu keputusan yang **melonggarkan** pembatasan yang sempat dipasang layar (angka ambang), dan empat
penyesuaian pada layar yang sudah dibangun karena kontraknya berubah.

### 21.1 Dua gerbang UI yang akhirnya tertutup

| Gerbang | Keadaan sebelum | Keputusan |
|---|---|---|
| `FIN-OQ-079` — penempatan butir menu tujuh layar baru | Layar dibangun, butir menunya **tertahan**. Dicapai hanya lewat alamat langsung dan tautan silang | **CLOSED** oleh `FIN-DEC-148` dan `FIN-DEC-160` |
| `FIN-OQ-080` — apakah angka ambang ditampilkan kepada staf AR/AP | Layar ambang **membatasi diri**: hanya untuk pemegang hak ubah, dan layar pembayaran tidak menyebut angkanya | **CLOSED** oleh `FIN-DEC-147`: **angka ditampilkan** |

### 21.2 Penempatan butir menu (`FIN-DEC-148`, `FIN-DEC-160`)

Wewenang ini **milik pemilik**, bukan `DEV_DISCRETION` — `FIN-DEC-094` mengikat menu Transaksi A/R dan A/P
pada bentuk V1, dan layar-layar ini tidak ada di V1.

| Submenu | Butir | Urutan | Rute |
|---|---|---:|---|
| **Cutover & Subledger** (submenu **baru** pada grup Keuangan) | Pemetaan Akun Control | 1 | `/finance/subledger-setup/control-accounts` |
| idem | Saldo Awal Cutover | 2 | `/finance/subledger-setup/opening-balances` |
| idem | Batch Migrasi Tagihan Lama | 3 | `/finance/subledger-setup/opening-item-batches` |
| **Master Data** (submenu yang **sudah ada**) | Ambang Pembayaran Langsung | menyusul butir yang sudah ada | `/finance/master-data/direct-payment-threshold` |

**Urutan butir mengikuti urutan kerja petugas** (`FIN-DEC-160`): memetakan akun → mencatat saldo awal →
memindahkan tagihan lama. Urutan itu bukan selera: batch migrasi **tidak dapat** diunggah sebelum saldo awal
cutover dicatat, dan snapshot **ditolak** bila pemetaan akun belum lengkap. Menu membaca seperti alurnya.

**Dua layar yang sengaja TIDAK diberi butir menu:**

| Layar | Cara mencapainya | Alasan |
|---|---|---|
| Buku Mutasi Kas | Dari layar Kas & Bank yang sudah ada | Ia permukaan baca milik rumpun kas, bukan layar berdiri sendiri |
| Riwayat Mutasi Piutang dan Utang Supplier | Dari layar rincian piutang dan rincian utang | Ia riwayat satu dokumen, bukan daftar yang dicari dari menu |

**Yang MUST dijaga:** menu Transaksi A/R dan Transaksi A/P **tidak berubah sama sekali** (`FIN-DEC-094`).
Submenu baru berdiri di sampingnya, bukan menyisipkan butir ke dalamnya.

### 21.3 Angka ambang ditampilkan kepada staf AR/AP (`FIN-DEC-147`)

Keputusan ini **mencabut** pembatasan yang sempat dipasang layar `FE-FIN-031` ketika `FIN-OQ-080` masih
terbuka. Jawaban pemilik berbeda dari rekomendasi yang diajukan, dan keputusan pemilik yang berlaku.

| Layar | Sebelum | Sesudah |
|---|---|---|
| Master Ambang (`FE-FIN-031`) | Hanya terbuka bagi pemegang `Read` **dan** `Update` | Terbuka bagi pemegang **`Read`**. Kendali ubah hanya tampil bagi pemegang `Update` |
| idem | Banner menyatakan layar dibatasi karena `FIN-OQ-080` belum diputuskan | Banner itu **dihapus** — pertanyaannya sudah dijawab |
| Pembayaran Langsung (`FE-FIN-030`) | Peringatan melewati ambang **tanpa** menyebut angka | Peringatan **menyebut angkanya**, misalnya *"Nominal melewati ambang pembayaran langsung (Rp 10.000.000). Gunakan jalur pembayaran berjenjang."* |

**Yang MUST tetap ada:** pernyataan bahwa perubahan ambang **tidak melalui jenjang persetujuan**
(`FIN-PERM-1.7` G.5). Layar **MUST NOT** menjanjikan pengawasan yang tidak ada.

**Prasyarat yang berada di luar kendali layar.** Agar angka benar-benar terbaca staf, peran staf AR/AP
**MUST** diberi hak `MstDirectPaymentThreshold : Read`. Pemberian hak itu milik admin. Tanpanya layar
pembayaran jatuh kembali ke peringatan tanpa angka — dan layar **MUST** menangani keadaan itu tanpa
menampilkan galat teknis.

### 21.4 Empat penyesuaian layar karena kontraknya berubah

| # | Layar | Yang berubah | Keputusan |
|---:|---|---|---|
| 1 | Master Ambang (`FE-FIN-031`) | Kolom tanggal berlaku **dihapus** dari form dan tampilan. Menampilkan penanda versi tidak perlu, tetapi layar **MUST** mengirimnya kembali saat menyimpan, dan **MUST** menangani `409` dengan memuat ulang lalu memberi tahu bahwa ambang sudah diubah orang lain | `FIN-DEC-145`, `146`, `153` |
| 2 | Master Ambang | Menampilkan **nama** pengubah terakhir, bukan hanya waktunya. Bila nama tidak tersedia, tulis keterangannya — **MUST NOT** menampilkan ID mentah | `FIN-DEC-151` |
| 3 | Batch Migrasi (`FE-FIN-032`) | Pemilih berkas hanya menawarkan **CSV**. Pilihan XLSX pada unduh templat tetap **nonaktif** beserta keterangannya. Menampilkan **nama penyetuju** batch. Keadaan `400` batas 10.000 baris ditampilkan sebagai keterangan yang dapat ditindaklanjuti: sebutkan batasnya dan sarankan memecah berkas | `FIN-DEC-149`, `150`, `151`, `155` |
| 4 | Snapshot Saldo Subledger (`FE-FIN-029`) | Berhenti **menyimpulkan** keadaan "belum ada rekap" dari tanggal rekap yang kosong; mulai membaca penanda resmi `HasDailyCashSnapshot`. Saldo penutupan dan selisih kini **boleh kosong** dan **MUST NOT** ditampilkan sebagai `Rp 0` | `FIN-DEC-152` |

**Catatan untuk penyesuaian nomor 4.** Layar ini sudah menangani keadaan "belum ada rekap" dengan benar
hari ini, jadi penyesuaiannya **bukan** perbaikan cacat melainkan memindahkan dasar kesimpulannya dari
terkaan ke penanda resmi. Perilaku yang dilihat pengguna tidak berubah.

### 21.5 Penanganan state yang ditambahkan

| State | Yang dilihat pengguna |
|---|---|
| `409` saat menyimpan ambang | *"Ambang sudah diubah oleh orang lain. Muat ulang sebelum melanjutkan."* Layar memuat ulang dan menampilkan nilai, alasan, serta nama pengubah terbaru — supaya pejabat dapat memutuskan apakah tetap mengubahnya |
| `400` batas 10.000 baris saat unggah batch | Keterangan menyebut batasnya dan menyarankan memecah berkas. Berkas **tidak** terkirim ulang otomatis |
| `503` berkas XLSX | Keterangan bahwa pembaca XLSX belum tersedia dan menyarankan CSV — **bukan** galat teknis dan **bukan** "berkas Anda salah" |
| Periode tanpa rekap kas harian | Saldo penutupan dan selisih berbunyi *"Belum ada rekap"* dan *"Tidak dapat dinyatakan"*. **MUST NOT** menampilkan `Rp 0` |
| Ambang belum ditetapkan (`404`) | Tetap seperti sebelumnya: keadaan nyata bahwa seluruh pembayaran langsung sedang ditolak, beserta form penetapan pertama |

### 21.6 Hierarki wewenang untuk amandemen ini

| Keputusan | Lapisan wewenang | Pemilik |
|---|---|---|
| Angka ambang terlihat staf; layar tidak menjanjikan jenjang persetujuan | **security/privacy/invariant** | Pemilik (`FIN-DEC-147`, `FIN-PERM-1.7` G.5) |
| Saldo penutupan kosong **MUST NOT** tampil sebagai `Rp 0` | **invariant** | Pemilik (`FIN-DEC-152`) |
| Penempatan submenu dan urutan butirnya | **approved product/UI brief** | Pemilik (`FIN-DEC-148`, `160`) — **bukan** `DEV_DISCRETION` |
| Pemilih unggah hanya CSV | **approved product/UI brief** | Pemilik (`FIN-DEC-150`) |
| Ikon submenu, lebar kolom, warna, tata letak kartu | `DEV_DISCRETION` | — |
| Redaksi persis pesan batas baris dan `503` | `DEV_DISCRETION` dalam batas: **MUST** menyebut batas yang dilanggar dan langkah perbaikannya | — |

### 21.7 Pertanyaan terbuka frontend sesudah amandemen ini

| ID | Keadaan |
|---|---|
| `FIN-OQ-079` | **CLOSED** (21.2) |
| `FIN-OQ-080` | **CLOSED** (21.3) |
| `FIN-OQ-082` | **CLOSED** — nilai batas ukuran berkas bukti diputuskan `FIN-DEC-156`. Layar **MUST** tetap menangani `503` karena nilai itu baru berlaku setelah administrator mengisinya |
| `FIN-OQ-081` | **DITUNDA.** Selama pembaca XLSX belum ada, pilihan XLSX tetap nonaktif (21.4 nomor 3) |

### 21.8 Yang sengaja TIDAK dibuat pada amandemen ini

| Yang dipertimbangkan | Alasan ditolak |
|---|---|
| Butir menu untuk Buku Mutasi Kas dan riwayat mutasi | Keduanya permukaan baca di dalam layar induknya (21.2) |
| Menampilkan penanda versi ambang kepada pengguna | Ia urusan mesin, bukan informasi yang berguna bagi pejabat. Yang perlu dilihat pengguna adalah nilai, alasan, nama pengubah, dan waktunya |
| Layar riwayat perubahan ambang | Tidak ada datanya — tabel riwayat ditolak `FIN-DES-086`, dan `FIN-DEC-145` menghapus alasan teknis terakhir untuk membuatnya |
| Tombol menghidupkan penjadwal dan worker dari layar | Menjadi jalan memutar gerbang G3 dan keputusan operasional |
| Memberi layar pembayaran langsung kemampuan mengubah ambang | Memisahkan wewenang: staf mencatat pembayaran, pejabat mengubah ambang |
