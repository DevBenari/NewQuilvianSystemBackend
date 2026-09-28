# Module Artifact — Keuangan

## 1. Ringkasan Modul

Modul **Keuangan** pada aplikasi rumah sakit mencakup dua domain utama yang terlihat pada bukti: **Accounts Payable (AP / Utang Usaha)** dan **Accounts Receivable (AR / Piutang Usaha)**.

- **AP** mengelola kewajiban rumah sakit kepada supplier sejak Purchase Order, Tukar Faktur, Purchasing Invoice, penerimaan/tanda terima, aging dan jatuh tempo, pembayaran, retur pembelian/deposit retur, hingga pelaporan dan rekap hutang supplier.
- **AR** mengelola piutang pasien kepada perusahaan/asuransi/penjamin sejak tagihan belum diinvoiskan, pembuatan invoice, diskon, penerimaan pembayaran dan penerimaan lain, settlement, pembatalan, hingga pelaporan dan aging piutang.

Kedua area sama-sama bergantung pada data transaksi operasional, master pihak eksternal, COA, pajak, pembayaran, status transaksi, serta laporan aging. Namun bukti tidak menunjukkan satu alur end-to-end yang secara langsung menghubungkan AP dan AR sebagai satu transaksi yang sama.

Batas bukti penting:
- Bukti AP berasal dari beberapa video yang sebagian juga berisi segmen AR dan satu video berlabel AP tetapi secara visual berisi Radiologi. Segmen yang tidak relevan tidak dijadikan kapabilitas AP.
- Bukti AR memperlihatkan dua antarmuka: HiSys lama dan aplikasi AR baru. Bukti tidak menetapkan pemetaan migrasi satu-ke-satu atau sistem mana yang menjadi sumber data utama.
- Audio tersedia pada sumber video, tetapi tidak ditranskripsikan dalam artifact asal. Karena itu rangkuman ini hanya menggabungkan bukti visual yang telah tercatat pada artifact AP dan AR.

---

## 2. Submodul Keuangan

### 2.1 Accounts Payable (AP)

Fokus: kewajiban kepada supplier/vendor.

Kapabilitas utama:
1. Purchase Order.
2. Tukar Faktur.
3. Detail dan proses Tukar Faktur.
4. Set Purchasing Invoice.
5. Tanda Terima Barang.
6. Daftar Purchasing Invoice.
7. Aging AP.
8. Pembayaran Manual.
9. Retur Pembelian Supplier dan Deposit Retur.
10. Rekap Purchasing AP.
11. Laporan Tukar Faktur.
12. Laporan Jatuh Tempo.
13. Purchasing Payment.
14. Rekonsiliasi Tagihan.

### 2.2 Accounts Receivable (AR)

Fokus: piutang pasien kepada perusahaan/asuransi/penjamin dan piutang internal.

Kapabilitas utama:
1. Inventaris tagihan belum diinvoiskan.
2. Pembuatan AR Invoice.
3. Pengelolaan daftar invoice.
4. Detail billing dan draft nilai terbayar.
5. Diskon invoice.
6. Penerimaan pembayaran.
7. Penerimaan/potongan lain.
8. Settlement AR.
9. Pembatalan invoice/settlement/pembayaran.
10. Aging piutang.
11. Penyelesaian piutang karyawan.
12. Pelaporan AR.
13. Dokumen dan ekspor.
14. Pemutihan Piutang dan Manajemen Klaim, dengan bukti detail yang masih terbatas.

---

## 3. Actors / Roles

### AP

- **Admin Purchasing** — memproses Tukar Faktur dan detail transaksi AP.
- **Supplier** — pihak eksternal terkait PO, invoice, TOP, PPN, retur, dan pembayaran.
- **Petugas Retur** — peran/kolom yang terlihat pada daftar Retur Pembelian Supplier.
- **Pengguna Keuangan/AP** — pengguna yang mengelola transaksi, monitoring, pembayaran, dan laporan AP; nama role formal tidak diperlihatkan.

### AR

- **Pengguna Keuangan/AR** — membuat dan mengelola invoice, pembayaran, diskon, settlement, pembatalan, dan laporan.
- **Perusahaan/Asuransi/Penjamin** — pihak tujuan tagihan dan pembayaran AR.
- **Pasien** — subjek billing yang digabungkan ke invoice dan settlement.
- **Karyawan/Dokter/Pegawai** — pihak yang dapat memiliki piutang internal dan diproses pada penyelesaian piutang karyawan.

### Peran Bersama yang Terlihat Secara Fungsional

Artifact AP dan AR sama-sama menunjukkan kebutuhan pengguna keuangan untuk:
- pencarian transaksi,
- pengelolaan invoice/tagihan,
- penggunaan COA,
- pencatatan pembayaran,
- pemantauan saldo,
- dan pelaporan.

Namun matriks role/otorisasi formal lintas AP dan AR tidak tersedia pada bukti.

---

## 4. Capability / Feature Registry Terpadu

| ID | Area | Capability / Feature | Ringkasan | Confidence |
|---|---|---|---|---|
| FIN-AP-001 | AP | Purchase Order | Membuat PO dengan nomor otomatis, supplier, TOP, expired date, produk, qty, harga, dan diskon. | High |
| FIN-AP-002 | AP | Tukar Faktur | Membuat tukar faktur dari PO maupun bukan PO, supplier, invoice/PO, tanggal terima, dan jatuh tempo. | High |
| FIN-AP-003 | AP | Detail Tukar Faktur | Meninjau supplier, status tagihan, tanggal, PO/invoice, nilai, serta aksi Tanda Terima atau Set Invoice. | High |
| FIN-AP-004 | AP | Set Purchasing Invoice | Mengatur termin/DP, diskon, PPN, biaya tambahan/pengurang, COA PPN, faktur pajak, dan nilai item. | High |
| FIN-AP-005 | AP | Tanda Terima Barang | Membuka atau mencetak dokumen tanda terima terkait nomor AP dan produk. | High |
| FIN-AP-006 | AP | Purchasing Invoice List | Mencari/filter invoice serta melihat total tagihan, jumlah data, dan nilai yang disetujui. | High |
| FIN-AP-007 | AP | Aging AP | Analisis hutang supplier menurut periode, COA, supplier, dan bucket umur/jatuh tempo. | High |
| FIN-AP-008 | AP | Pembayaran Manual | Mencatat pembayaran supplier dengan/tanpa referensi PO/GR beserta pajak, currency, invoice vendor, dan catatan. | High |
| FIN-AP-009 | AP | Retur Pembelian Supplier | Memantau retur, detail PO/penerimaan/produk, konfirmasi, dan deposit/refund retur. | High |
| FIN-AP-010 | AP | Rekap Purchasing AP | Rekap hutang supplier meliputi penerimaan, pengakuan, PPN, variance, diskon, retur, pembayaran, dan saldo belum dibayar. | High |
| FIN-AP-011 | AP | Laporan Tukar Faktur | Filter dan ekspor laporan tukar faktur dengan harga beli, DPP, PPN, diskon, dan total. | High |
| FIN-AP-012 | AP | Laporan Jatuh Tempo | Menu laporan jatuh tempo terlihat, tetapi isi layar belum terbukti. | Low |
| FIN-AP-013 | AP | Purchasing Payment | Menu pembayaran tersedia; alur reguler belum tertangkap lengkap. | Medium |
| FIN-AP-014 | AP | Rekonsiliasi Tagihan | Menu tersedia, tetapi layar dan tindakannya belum dibuka. | Low |
| FIN-AR-001 | AR | Tagihan Belum Diinvoiskan | Menampilkan dan memfilter billing pasien yang belum diproses menjadi invoice. | High |
| FIN-AR-002 | AR | Pembuatan AR Invoice | Membentuk invoice dari perusahaan/penjamin, periode, billing pasien, tanggal invoice, due date, dan keterangan. | High |
| FIN-AR-003 | AR | Daftar Invoice | Mencari invoice menurut periode, perusahaan, pasien/billing, jenis pasien, nomor invoice, total/netto, dan status. | High |
| FIN-AR-004 | AR | Detail Billing & Draft | Mengisi nominal terbayar per billing, autosave sebagai draft, dan dapat menghapus draft sebelum selesai. | High |
| FIN-AR-005 | AR | Diskon Invoice | Memilih COA diskon, persen/nominal, dan keterangan. | High |
| FIN-AR-006 | AR | Penerimaan Pembayaran | Mencatat kas/bank, tanggal pembayaran, total penerimaan, dan keterangan dari detail invoice. | High |
| FIN-AR-007 | AR | Penerimaan/Potongan Lain | Mencatat penerimaan tambahan, PPh 23, dan biaya administrasi bank dengan COA. | High |
| FIN-AR-008 | AR | Settlement AR | Menampilkan saldo awal, mutasi/bill, pembayaran, saldo akhir, tanggal, user, dan riwayat penyelesaian. | High |
| FIN-AR-009 | AR | Pembatalan | Pembatalan invoice, settlement, dan penerimaan pembayaran dengan jejak status/tanggal/user. | High |
| FIN-AR-010 | AR | Aging Piutang | Monitoring saldo berdasarkan umur dan due-date period. | High |
| FIN-AR-011 | AR | Piutang Karyawan | Memproses piutang karyawan/dokter/pegawai berdasarkan status tagihan dan pembayaran. | High |
| FIN-AR-012 | AR | Pelaporan AR | Laporan proses invoice, settlement, pembayaran guarantor, pembatalan, dan aging. | High |
| FIN-AR-013 | AR | Dokumen dan Ekspor | Upload/unduh dokumen, PDF, Excel, dan data export. | High |
| FIN-AR-014 | AR | Pemutihan & Manajemen Klaim | Menu tersedia, namun alur detail belum terbukti memadai. | Medium |

---

## 5. Menu / Screen / UI Elements

### 5.1 Navigasi AP yang Terlihat

- Pembelian Pesanan / Purchase Order
- Penerima Pesanan / Penerimaan Invoice
- Retur Produk / Retur Pembelian Supplier
- Tukar Faktur
- Purchasing Invoice
- Laporan Tukar Faktur
- Jasa Medis
- Utang Usaha (A/P Aging) / Laporan Aging AP
- Pembayaran A/P / Purchasing Payment
- Rekap Purchasing AP
- Laporan Jatuh Tempo
- Rekonsiliasi Tagihan

Menu AP berbeda antarrekaman dan tidak semua menu dibuka atau dibuktikan sebagai alur lengkap.

### 5.2 Navigasi AR Baru yang Terlihat

- Tagihan / Billing
- Receivable AR / Invoice
- Canceled Invoice / Receivable AR Canceled
- Settlement AR
- Piutang Korporat / Penjamin
- Manajemen Klaim
- Umur Piutang / A/R Aging
- Pemutihan Piutang

### 5.3 Navigasi AR HiSys Lama yang Terlihat

- Guarantor
- Create A/R Guarantor
- Internal Receivables
- Receivable Guarantor
- Jurnal A/R
- Settlement A/R Guarantor
- List Payment A/R Guarantor

Laporan AR lama yang terlihat:
- Report Cancel Insurance Company Bill
- Report Cancel Receive Payment
- Report Aging A/R Guarantor
- Laporan Proses Invoice
- Report Payment A/R Guarantor

---

## 6. Layar Utama dan Elemen Data

### AP

#### Purchase Order
- PO Number
- Supplier
- Term of Payment
- Expired Date
- Keterangan
- Kategori Produk
- Nama Produk
- Satuan
- Harga Satuan
- Qty
- Diskon

#### Tukar Faktur
Variasi layar memperlihatkan kombinasi:
- Tanggal Registrasi
- Tanggal Tukar Faktur / Tanggal Terima Faktur
- Tanggal Jatuh Tempo
- Tipe Registrasi Dari PO/Bukan Dari PO
- Supplier
- No. PO
- No. Invoice
- Mata Uang
- Status Invoice
- Keterangan
- Daftar PO/Invoice Supplier

#### Set Purchasing Invoice
- Down Payment / Termin
- Diskon % dan nominal
- PPN % dan nominal
- Outstanding DP
- Ongkos Kirim
- Materai
- COA PPN
- Pembulatan
- Potongan
- No./Tanggal Faktur Pajak
- Detail item dan pajak

#### Aging / Rekap / Pembayaran / Retur
- Filter tanggal, supplier, COA, aging
- Invoice dan jatuh tempo
- Currency/rate
- Referensi dan invoice vendor
- Pajak/faktur pajak/AP Tax
- Saldo dibayar/belum dibayar
- Detail retur dan Deposit Retur

### AR

#### Data Tagihan/Billing
- Perusahaan/asuransi/penjamin
- Periode
- Tanggal awal/akhir
- Jenis pasien
- Billing
- Registrasi
- RM/MRN
- Pasien
- Jumlah tagihan

#### Buat AR Invoice
- Tujuan tagihan
- Status tagihan
- Jenis pasien
- Perusahaan
- Tanggal invoice
- Due date
- Keterangan
- Pilihan billing
- Total piutang
- Diskon
- Total setelah diskon

#### Detail Invoice & Pembayaran
- Daftar billing pasien
- Draft nilai terbayar
- Diskon
- Penerimaan pembayaran
- Penerimaan lain
- PPh 23
- Biaya admin bank
- Kas/bank account
- Ringkasan pembayaran
- Dokumen

#### Settlement
- Pasien
- Invoice
- Saldo awal
- Mutasi/bill
- Pembayaran
- Saldo akhir
- Tanggal pembuatan
- Tanggal settlement
- User keuangan
- Status cancel

---

## 7. Business Process / Ordered Flow

## 7.1 AP — Purchase Order sampai Monitoring Hutang

1. **Membuat Purchase Order** dengan supplier, produk, qty, harga, diskon, TOP, dan expired date.
2. **Membuat Tukar Faktur** dari PO atau bukan dari PO dengan supplier, invoice, mata uang, tanggal terima/tukar, dan jatuh tempo.
3. **Meninjau detail Tukar Faktur** untuk supplier, invoice, PO, nilai, status, dan tanggal.
4. **Memilih Tanda Terima atau Set Invoice.**
5. **Menetapkan Purchasing Invoice** termasuk termin/DP, diskon, PPN, biaya/potongan, COA PPN, dan faktur pajak.
6. **Memonitor daftar Purchasing Invoice dan Laporan Tukar Faktur.**
7. **Mencatat pembayaran** melalui alur pembayaran manual atau menu pembayaran yang tersedia.
8. **Memproses retur** dan Deposit Retur bila terjadi pengembalian barang.
9. **Memonitor Aging AP, Rekap Purchasing AP, dan jatuh tempo** untuk mengetahui hutang supplier, umur tagihan, pembayaran, serta saldo belum dibayar.

Catatan: approval, posting jurnal, dan lifecycle status lengkap belum terbukti dari artifact.

## 7.2 AR — Billing sampai Settlement Piutang

1. **Mengambil billing yang belum diinvoiskan** dan memfilter berdasarkan perusahaan/penjamin, periode, pasien, billing, registrasi, atau RM.
2. **Membentuk invoice** dengan memilih perusahaan/penjamin, jenis pasien, tanggal invoice, jatuh tempo, dan satu atau banyak billing.
3. **Mengisi diskon bila diperlukan** dan menyimpan invoice.
4. **Membuka detail invoice** untuk melihat billing dan nilai piutang.
5. **Mengisi nominal terbayar per billing**, yang dapat tersimpan sebagai draft.
6. **Mencatat penyesuaian** seperti diskon, penerimaan lain, PPh 23, atau biaya admin bank melalui COA terkait.
7. **Mencatat penerimaan pembayaran** melalui kas/bank, tanggal, nominal, dan keterangan.
8. **Mencerminkan transaksi pada Settlement AR** sebagai bill/payment dan perubahan saldo.
9. **Melakukan pembatalan bila diperlukan**, dengan jejak transaksi yang terlihat pada daftar/report.
10. **Memonitor Aging AR dan laporan** proses invoice, settlement, pembayaran, serta pembatalan.

## 7.3 AR — Piutang Karyawan

1. Memilih periode, dokter/pegawai, status tagihan, dan status pembayaran.
2. Menampilkan tagihan, pembayaran, dan sisa piutang per karyawan.
3. Aksi bayar tersedia, tetapi detail konfirmasi dan hasil akhirnya tidak terbukti lengkap.

---

## 8. Business Rules, Validation, and Exceptions

### 8.1 AP

| ID | Rule / Validation / Exception | Confidence |
|---|---|---|
| FIN-AP-RULE-001 | Field bertanda `*` wajib pada form Tukar Faktur, PO, dan Pembayaran Manual. | High |
| FIN-AP-RULE-002 | Nomor PO dibuat otomatis; TOP dan expired date mengikuti supplier yang dipilih. | High |
| FIN-AP-RULE-003 | Jatuh tempo Tukar Faktur dihitung dari tanggal terima ditambah TOP supplier; contoh menunjukkan 30 hari. | High |
| FIN-AP-RULE-004 | Tukar Faktur dapat berasal dari PO atau bukan dari PO pada salah satu variasi layar. | High |
| FIN-AP-RULE-005 | Pembayaran manual tanpa referensi digunakan untuk transaksi tanpa PO/GR seperti biaya administrasi, jasa, utilitas, atau invoice vendor tanpa PO. | High |
| FIN-AP-RULE-006 | Total Deposit Retur mengikuti grand total item retur. | High |
| FIN-AP-RULE-007 | Contoh layar menggunakan PPN 11%, tetapi bukti tidak cukup untuk menjadikannya tarif final atau permanen. | Medium |
| FIN-AP-RULE-008 | Status yang terlihat antara lain `Belum Lunas`, `Belum Diproses`, dan `Terkonfirmasi`; transisi lengkap belum terbukti. | Medium |
| FIN-AP-RULE-009 | Terdapat kondisi lingkungan rekaman di mana `tipeRegistrasi`, `mataUang`, dan produk belum sepenuhnya tersedia via API. Status terkini tidak diverifikasi oleh artifact. | High |

### 8.2 AR

| ID | Rule / Validation / Exception | Confidence |
|---|---|---|
| FIN-AR-RULE-001 | Tagihan awal adalah piutang pasien yang belum diproses menjadi invoice. | High |
| FIN-AR-RULE-002 | Diskon dibuat per invoice. | High |
| FIN-AR-RULE-003 | Penerimaan pembayaran dilakukan dari detail invoice. | High |
| FIN-AR-RULE-004 | Nominal terbayar per billing dapat autosave sebagai draft dan draft dapat dihapus. | High |
| FIN-AR-RULE-005 | Settlement mengelompokkan data berdasarkan pasien, nomor registrasi, dan nomor invoice. | High |
| FIN-AR-RULE-006 | Due date/due-date period menjadi atribut invoice/billing dan dasar umur piutang. | High |
| FIN-AR-RULE-007 | Pembatalan mempertahankan jejak tanggal dan pengguna pada laporan HiSys lama. | High |
| FIN-AR-RULE-008 | Invoice dapat berisi satu atau banyak pasien/billing dan dibedakan menurut jenis pasien. | High |
| FIN-AR-RULE-009 | Formula final netto, grand total, total terbayar, sisa piutang, dan penyesuaian belum dapat dipastikan dari bukti. | Medium |

---

## 9. Data / Input / Output Observed

### 9.1 Master dan Referensi

- Supplier
- Perusahaan/asuransi/penjamin
- Pasien
- Karyawan/dokter/pegawai
- Produk
- COA
- Cash/Bank Account
- Pajak/Faktur Pajak
- Registrasi, billing, perawatan
- Purchase Order dan Goods Receipt/Penerimaan

### 9.2 Data AP

- Supplier: kode/nama, PIC, TOP, lead time, diskon, PPN, mata uang.
- PO: nomor, tanggal, supplier, TOP, expired date, item, satuan, harga, qty, diskon.
- Tukar Faktur: no. tagihan/AP, tipe registrasi, supplier, tanggal, jatuh tempo, PO, invoice, mata uang, status.
- Purchasing Invoice: termin/DP, DPP, diskon, PPN, ongkos kirim, materai, pembulatan, potongan, COA PPN, faktur pajak.
- Pembayaran: document ID, tanggal, supplier, currency/rate, referensi, invoice vendor, pajak, catatan, total.
- Retur: kode/tanggal, PO/penerimaan/faktur, supplier, produk, jumlah, harga, diskon, subtotal, status, deposit retur.

### 9.3 Data AR

- Identitas pasien/billing: pasien, RM/MRN, registrasi, billing, tanggal masuk/keluar/registrasi.
- Invoice: no. invoice, perusahaan/penjamin, jenis pasien, tanggal invoice, jatuh tempo, due-date period, status rekap/cancel.
- Nilai: total piutang, diskon, netto, penerimaan lain, PPh 23, biaya admin bank, total terbayar, sisa/saldo.
- Pembayaran: kas/bank, tanggal, jumlah, keterangan, COA, user pencatat.
- Settlement: saldo awal, mutasi/bill, pembayaran, saldo akhir, tanggal, user, status cancel.
- Dokumen: komponen biaya, berkas tagihan, upload, PDF, Excel/data export, laporan AR.

### 9.4 Output / Dokumen

#### AP
- Tanda Terima Barang.
- Daftar/detail Tukar Faktur.
- Daftar/detail Purchasing Invoice.
- Laporan Tukar Faktur.
- Aging AP.
- Rekap Purchasing AP.
- Status dan agregat hutang supplier.

#### AR
- AR Invoice.
- Daftar/detail billing dan invoice.
- Settlement AR.
- Laporan pembayaran/pembatalan/aging.
- PDF dan Excel/data export.
- Status dan agregat piutang.

---

## 10. Integrations / Dependencies Mentioned

### AP

- **Master Supplier** untuk TOP, lead time, PPN, currency, diskon, dan identitas supplier.
- **Purchase Order / Goods Receipt / Penerimaan** sebagai referensi invoice, pembayaran, dan retur.
- **Master Produk** untuk item PO/invoice.
- **COA** untuk Aging AP dan COA PPN.
- **Data pajak/faktur pajak** untuk purchasing invoice dan pembayaran manual.

### AR

- **Data pasien, registrasi, billing, dan perawatan** sebagai sumber tagihan.
- **Master perusahaan/asuransi/penjamin** sebagai tujuan invoice dan filter laporan.
- **COA** untuk diskon dan penerimaan lain.
- **Cash/Bank Account** untuk pembayaran.
- **Internal Receivables** untuk piutang karyawan/dokter/pegawai.
- **Jurnal A/R, Manajemen Klaim, dan Pemutihan Piutang** terlihat pada menu, tetapi interaksi detail belum terbukti.

### Integrasi Keuangan yang Sama-sama Terlihat

AP dan AR sama-sama membutuhkan:
- COA,
- tanggal transaksi,
- invoice/tagihan,
- nilai pajak/penyesuaian,
- pembayaran,
- saldo,
- user pencatat,
- aging/jatuh tempo,
- dan laporan.

Artifact asal belum membuktikan posting jurnal otomatis atau alur integrasi General Ledger secara lengkap.

---

## 11. Persamaan dan Perbedaan AP vs AR

| Aspek | AP | AR |
|---|---|---|
| Fokus | Hutang rumah sakit | Piutang rumah sakit |
| Counterparty utama | Supplier/vendor | Perusahaan/asuransi/penjamin, pasien, karyawan |
| Sumber transaksi | PO, penerimaan, invoice supplier | Billing pasien, registrasi, perawatan |
| Dokumen inti | Tukar Faktur, Purchasing Invoice | AR Invoice |
| Pembayaran | Rumah sakit membayar supplier | Rumah sakit menerima pembayaran |
| Aging | Aging hutang supplier | Aging piutang |
| Penyesuaian | DP, diskon, PPN, ongkos, materai, potongan | Diskon, PPh 23, biaya admin bank, penerimaan lain |
| Retur/Cancel | Retur pembelian dan deposit retur | Cancel invoice, payment, settlement |
| Settlement/Rekap | Rekap Purchasing AP | Settlement AR |
| Ekspor/Laporan | Laporan Tukar Faktur, Aging, Rekap | Invoice, payment, settlement, cancel, aging |

---

## 12. Konflik Bukti

### AP

1. Satu sumber berlabel AP secara visual berisi Radiologi dan tidak digunakan sebagai bukti capability AP.
2. Beberapa video AP juga mengandung segmen AR; segmen tersebut dikecualikan dari registry AP.
3. Form Tukar Faktur berbeda antarrekaman. Bukti tidak menetapkan apakah variasi tersebut berasal dari versi, skenario, atau perubahan rancangan.
4. Status awal terlihat sebagai `Belum Lunas` dan `Belum Diproses`; hubungan semantik dan urutannya belum terbukti.

### AR

1. HiSys lama dan aplikasi AR baru menggunakan menu, istilah, dan detail layar berbeda.
2. Bukti tidak menentukan apakah HiSys masih aktif, menjadi referensi migrasi, atau berjalan paralel.
3. Menu cancel tidak selalu tampil konsisten antarvideo baru meskipun status cancel terlihat pada daftar.
4. Nilai contoh pada ringkasan pembayaran belum cukup konsisten untuk menurunkan formula keuangan final.

### Lintas AP dan AR

Artifact asal menunjukkan AP dan AR berada di area keuangan, tetapi tidak menunjukkan secara eksplisit satu struktur menu induk final, shared service, model posting jurnal bersama, atau satu state machine lintas keduanya.

---

## 13. Unresolved Questions

### 13.1 AP

1. Apakah video AP yang berisi Radiologi salah label dan perlu diganti?
2. Variasi form Tukar Faktur mana yang menjadi acuan final?
3. Apa lifecycle resmi status Tukar Faktur/Purchasing Invoice hingga lunas?
4. Siapa yang berwenang approval invoice, retur, pembayaran, dan rekonsiliasi?
5. Bagaimana alur lengkap Purchasing Payment, Rekonsiliasi Tagihan, Penerimaan Invoice, Jasa Medis, dan Laporan Jatuh Tempo?
6. Apakah tarif PPN dapat dikonfigurasi?
7. Apakah keterbatasan API yang terlihat pada rekaman masih berlaku?

### 13.2 AR

1. Apa pemetaan resmi fitur/data antara HiSys lama dan aplikasi AR baru?
2. Apa matriks otorisasi untuk invoice, pembayaran, settlement, diskon, cancel, dan pemutihan?
3. Apa validasi pembayaran lebih/kurang, tanggal mundur, duplikasi invoice, dan batas diskon?
4. Apa formula final netto, grand total, total terbayar, sisa piutang, PPh 23, biaya admin bank, dan penerimaan lain?
5. Apa arti dan transisi status Rekap, Draft, Canceled, Belum Dibuat/Belum Proses Invoice, dan settlement?
6. Bagaimana alur lengkap Manajemen Klaim, Pemutihan Piutang, Jurnal A/R, dan approval piutang karyawan?
7. Dokumen apa yang wajib sebelum invoice, pembayaran, atau settlement diselesaikan?

### 13.3 Keuangan Terpadu

1. Bagaimana struktur Menu/Submenu final untuk modul Keuangan yang menaungi AP dan AR?
2. Apakah AP dan AR menggunakan service pembayaran, jurnal, approval, dan audit trail yang sama?
3. Bagaimana integrasi kedua submodul dengan General Ledger?
4. Apakah seluruh transaksi finansial membutuhkan approval maker-checker?
5. Apa role matrix lintas AP, AR, Purchasing, Kasir, Accounting, dan Finance Manager?
6. Apa aturan periode tutup buku terhadap create/update/cancel transaksi AP dan AR?
7. Bagaimana standardisasi status transaksi lintas AP dan AR?
8. Apakah aging AP dan aging AR menggunakan kalender, cut-off date, dan bucket yang sama?

Pertanyaan pada bagian 13.3 adalah kebutuhan pemetaan lanjutan yang timbul saat AP dan AR disatukan sebagai satu Modul Keuangan; jawabannya belum tersedia pada artifact sumber.

---

## 14. Source Coverage

### AP

- `01-AP-2.mp4` — Aging AP, pembayaran manual, rekap purchasing, retur/deposit; sebagian segmen AR dikecualikan.
- `02-AP-1.mp4` — Tukar Faktur, Set Purchasing Invoice, laporan, tanda terima; sebagian segmen AR/diskusi dikecualikan.
- `03-AP-3.mp4` — mismatch; secara visual Radiologi, tidak ada capability AP yang diambil.
- `04-AP-4.mp4` — PO dan Tukar Faktur; sebagian segmen AR dikecualikan.

### AR

- `01-AR-3.mp4` — antarmuka AR baru dan cuplikan HiSys lama; billing, invoice, diskon/pembayaran, settlement, pembatalan.
- `02-AR-2.mp4` — antarmuka AR baru; pembuatan invoice, draft detail billing, penerimaan, settlement.
- `03-AR-1.mp4` — HiSys lama; guarantor/perusahaan, pembayaran, settlement, piutang karyawan, aging, laporan.

---

## 15. Kesimpulan Modul Keuangan

Berdasarkan kedua artifact, Modul **Keuangan** dapat dirangkum sebagai modul yang mengelola dua arus finansial utama:

1. **Arus keluar / kewajiban (AP)** — dari supplier, PO, invoice, jatuh tempo, pembayaran, retur, sampai aging hutang.
2. **Arus masuk / piutang (AR)** — dari billing pasien, pembentukan invoice kepada penjamin/perusahaan, pembayaran, settlement, cancel, sampai aging piutang.

Bukti cukup kuat untuk menyatakan bahwa AP dan AR masing-masing memiliki fungsi transaksi, pembayaran, saldo, aging, dan laporan. Namun struktur otorisasi, lifecycle status final, formula finansial lengkap, posting jurnal ke General Ledger, shared payment engine, serta desain struktur Menu/Submenu Keuangan terpadu masih belum terbukti dari sumber yang tersedia.
