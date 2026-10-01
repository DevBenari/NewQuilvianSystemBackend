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

