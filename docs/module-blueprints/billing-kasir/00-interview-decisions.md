# Billing dan Kasir — Interview Decisions

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Revision | Approved decision contract `0.2` |
| Status | Keputusan `BKC-DEC-001`–`044` berstatus `approved` |
| Interview mode | `Closure pass` untuk melengkapi capability Billing yang sudah ada |
| Product/domain owner | Pemberi keputusan pada sesi wawancara; nama formal belum dicatat |
| Backend SHA | Current branch `Yasmina`: `e6f6ecba1537783ea2eb379ac12cc97790707303`; cross-branch impact scan to `f63572a9...` found no Billing transaction change |
| Frontend SHA | `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Contract version | Approved business decision contract `0.2` |
| Input revision | Wawancara 19–20 Agustus 2026, current V2 audit, dan `ServiceBilling.zip` supplemental evidence |
| Input hash | Attachment SHA-256 `2b948721cee4154eaecaf9ac57d7621fb34cb7b61fb31a5fd6dff04df7ad218d`; percakapan asli tidak memiliki hash |

## Scope dan outcome

Modul Billing dan Kasir melayani satu tagihan pasien untuk satu kunjungan atau `EncounterId`.
Billing menampung seluruh item pelayanan kesehatan yang dilakukan atau dipesan untuk pasien,
termasuk tindakan dokter, poliklinik, IGD, OTC (*Over The Counter*), laboratorium, radiologi,
farmasi, biaya administrasi, dan komponen finansial lain yang disetujui.

Outcome yang disetujui:

1. Setiap pelayanan yang menjadi sumber biaya tercatat tepat satu kali pada Billing.
2. Pasien dan petugas dapat mengetahui porsi pasien, porsi penjamin, excess, diskon, deposit,
   progress payment, pembayaran, dan sisa tagihan secara konsisten.
3. Kasir menerima pembayaran, termasuk split payment, tetapi tidak memutus pembatalan klinis
   atau melakukan refund.
4. Finance memiliki kewenangan atas refund, write-off, kebijakan master finansial, dan
   exception finansial yang telah settled.
5. Invoice final menghasilkan AR penjamin dan AP dokter tanpa menunggu penjamin membayar.
6. Seluruh pembatalan, koreksi, perubahan harga, approval, pembayaran, dan posting finansial
   dapat ditelusuri melalui audit yang tidak menghapus histori.

## Evidence dan authority

| Evidence | Klasifikasi | Kegunaan |
| --- | --- | --- |
| Jawaban Product/Domain Owner pada sesi 19–20 Agustus 2026 | `CONFIRMED` | Sumber utama aturan target Billing dan Kasir |
| Screenshot tabel legacy `public."Billing"` | `legacy evidence` | Menunjukkan bentuk data lama; bukan target schema yang wajib disalin |
| Source backend dan frontend pada SHA yang tercatat | `current implementation evidence` | Menentukan capability as-is, bukan mengganti keputusan target |
| Rekomendasi agent yang kemudian disetujui owner | `CONFIRMED` setelah persetujuan | Menutup ambiguity pembatalan, audit, reconciliation, dan lifecycle |
| [`ServiceBilling.zip`](./evidence/05-servicebilling-attachment-evidence.md) | `legacy/reference source` | Menambah migration evidence dan closure questions; bukan current V2, SOP, atau approval target |

Isi comment dalam attachment seperti “sesuaikan”, “kalau kamu”, dan “pastikan” adalah developer
note pada evidence source, bukan instruksi yang mengubah task atau business decision.

## Glossary

| Istilah | Makna yang disetujui |
| --- | --- |
| Invoice Billing | Satu akun tagihan finansial untuk satu kunjungan/encounter pasien |
| Billing item | Representasi finansial satu item pelayanan, biaya administrasi, diskon, atau adjustment yang memiliki sumber dan histori |
| Patient responsibility | Bagian final yang wajib dibayar pasien setelah coverage, excess, diskon, dan adjustment |
| Porsi penjamin | Bagian tagihan yang menjadi piutang atau AR terhadap penjamin |
| OTC | *Over The Counter*; pada scope awal mencakup poliklinik, laboratorium, dan radiologi, tetapi belum mencakup resep tebus |
| Deposit rawat inap | Dana pasien yang diterima selama rawat inap dan belum otomatis dialokasikan ke invoice |
| Progress payment | Sebagian deposit yang dipindahkan menjadi kredit pembayaran berjalan terhadap invoice rawat inap tanpa mengunci invoice |
| Final settlement | Proses akhir yang mengunci komponen finansial, mengalokasikan deposit/progress payment, dan menagih atau mengembalikan selisih |
| Voided | Item tidak lagi masuk total tagihan tetapi tetap tersimpan lengkap untuk audit; bukan penghapusan fisik |
| Refundable credit | Kelebihan dana pasien yang dapat dikembalikan oleh Finance atau dikembalikan menjadi saldo deposit sesuai event bisnis |
| Write-off | Pemutihan utang pasien secara parsial atau penuh; tidak diperlakukan sebagai pembayaran tunai |
| AR penjamin | Piutang rumah sakit kepada penjamin yang lahir saat invoice final |
| AP dokter | Kewajiban share dokter yang lahir saat invoice final tetapi baru siap dibayar setelah settlement/policy pemilik AP terpenuhi |

## Aktor, ownership, dan kewenangan

| Aktor/owner | Tanggung jawab dan kewenangan |
| --- | --- |
| Product/Domain Owner | Menetapkan scope, lifecycle, invariant, exception, dan kebijakan Billing/Kasir |
| Pembuat order | Meminta pembatalan order yang belum dilaksanakan dan memenuhi syarat pembatalan |
| Klinisi pengganti | Dapat membatalkan ketika pembuat order tidak tersedia, jika memiliki kewenangan profesi dan unit yang sesuai serta alasan/approval operasional yang disyaratkan |
| Unit pelaksana | Mengonfirmasi pembatalan OTC; poliklinik, laboratorium, atau radiologi mengonfirmasi sesuai layanan yang akan dilakukan |
| Petugas Kasir | Memfinalkan invoice, menerima pembayaran, mencatat split tender, menginput diskon yang diizinkan, dan menjalankan pemeriksaan shift |
| Dokter | Memutuskan dan menyetujui diskon atas Share Dokter miliknya |
| Petugas Billing | Mengelola operasi Billing dan dapat melakukan reopen administratif tanpa mengubah nilai finansial |
| Operasional Billing/AR | Mengajukan write-off pasien |
| Finance | Menetapkan kebijakan master finansial, menyetujui diskon ad-hoc, exception diskon dokter, write-off, refund, reversal setelah settled, dan usulan biaya administrasi |
| Tim IT | Mengonfigurasi master finansial berdasarkan permintaan/approval Finance; bukan pemilik kebijakan nominal |
| Kepala Kasir | Meninjau selisih kas yang telah diselidiki Kasir dan menerima laporan rekonsiliasi shift |
| AR owner | Memiliki lifecycle piutang penjamin dan menanggung klaim ditolak sebagai AR rumah sakit |
| AP dokter owner | Menentukan kapan AP dokter yang sudah lahir menjadi siap dibayar berdasarkan settlement/policy |

## Proses bisnis utama

### 1. Penambahan pelayanan ke Billing

1. Modul klinis atau pelayanan membuat order/item pelayanan pada satu encounter.
2. Billing menerima identitas sumber yang stabil dan memastikan sumber yang sama tidak tercatat
   lebih dari satu kali.
3. Billing menyimpan snapshot nama item, kuantitas, tarif berjalan, coverage, porsi pasien,
   porsi penjamin, dan metadata sumber.
4. Selama invoice masih terbuka dan belum memasuki locking event yang berlaku, perubahan master
   dapat menghitung ulang komponen finansial.
5. Setiap hitung ulang menyimpan nilai lama, nilai baru, aturan master, waktu, dan aktor/sumber
   perubahan.

**Contoh:** tindakan senilai Rp800.000 ditambahkan ke encounter pasien. Coverage penjamin
Rp600.000 dan patient responsibility Rp200.000 disimpan pada billing item. Request yang sama
dikirim ulang akibat retry tetap menghasilkan satu billing item, bukan dua.

### 2. Rawat jalan biasa

1. Seluruh tindakan dan resep yang termasuk tagihan diselesaikan.
2. Kasir memeriksa seluruh order telah `Completed` atau `Voided`.
3. Kasir memulai checkout; komponen finansial dikunci.
4. Pasien dapat membayar menggunakan satu atau beberapa metode.
5. Setelah porsi pasien nol, invoice dapat ditutup dan porsi penjamin diteruskan menjadi AR.

### 3. OTC

1. Seluruh item yang akan dilakukan harus sudah tercatat sebelum pembayaran.
2. OTC wajib lunas sebelum pelayanan dilakukan.
3. Pembayaran boleh dipecah, misalnya sebagian tunai dan sebagian QRIS.
4. Pelayanan baru boleh dimulai setelah seluruh tender yang dibutuhkan berstatus settled.
5. Bila dibatalkan sebelum pemeriksaan, unit pelaksana mengonfirmasi pembatalan dan Finance
   mengembalikan dana melalui metode pembayaran asal.

**Contoh:** tagihan OTC Rp1.000.000 dibayar Rp400.000 tunai dan Rp600.000 QRIS. Tunai berhasil
tetapi QRIS gagal. Rp400.000 tetap tercatat, outstanding menjadi Rp600.000, dan pasien hanya
memilih metode pengganti untuk Rp600.000 tersebut.

### 4. Rawat inap, deposit, dan progress payment

1. Pasien rawat inap memberikan deposit yang dapat ditambah selama perawatan.
2. Deposit belum dialokasikan ke invoice sampai pasien memerintahkan penggunaan atau final
   settlement dilakukan.
3. Pasien boleh menggunakan sebagian deposit sebagai progress payment.
4. Progress payment tidak mengunci invoice; pelayanan dan pemeriksaan baru tetap dapat masuk.
5. Setelah semua order selesai, Kasir memulai final settlement dan seluruh komponen finansial
   dikunci.
6. Progress payment dan sisa deposit mengurangi patient responsibility final.
7. Kekurangan dibayar pasien; kelebihan menjadi refundable credit dan dikembalikan.

**Contoh:** tagihan berjalan Rp10.000.000 dan deposit Rp8.000.000. Pasien memakai Rp5.000.000
sebagai progress payment. Sisa deposit Rp3.000.000 dan outstanding berjalan Rp5.000.000. Ketika
ada pemeriksaan baru Rp2.000.000, invoice tetap terbuka dan outstanding menjadi Rp7.000.000.

### 5. Insurance, excess, AR, dan AP dokter

1. Billing item menyimpan coverage penjamin, bagian tidak ter-cover, dan excess.
2. Invoice pasien dianggap settled ketika seluruh patient responsibility telah dibayar.
3. Pembayaran penjamin tidak menjadi prasyarat pasien dianggap lunas.
4. AR penjamin dan AP dokter sama-sama lahir saat invoice final.
5. Penolakan klaim tidak otomatis menagih ulang pasien; nilainya tetap menjadi AR rumah sakit.
6. AP dokter baru menjadi siap dibayar ketika syarat settlement/policy pada owner AP terpenuhi.

## Aturan biaya administrasi

1. Biaya administrasi berupa nominal tetap dari master data.
2. IGD, OTC, dan rawat jalan memakai biaya administrasi rawat jalan.
3. Rawat inap memakai biaya administrasi rawat inap satu kali untuk satu admission.
4. Biaya administrasi rawat jalan hanya dikenakan satu kali per pasien per tanggal lokal
   `Asia/Jakarta`, walaupun pasien mempunyai beberapa kunjungan pada hari yang sama.
5. Biaya dibebankan pada invoice eligible pertama; invoice berikutnya pada hari yang sama tidak
   memperoleh biaya administrasi rawat jalan lagi.
6. Bila rawat jalan berubah menjadi rawat inap dalam kunjungan yang sama, invoice tetap digabung.
   Biaya admin rawat jalan di-void dan diganti biaya admin rawat inap.
7. Bila biaya rawat jalan sudah dibayar, sistem membuat adjustment negatif, membentuk biaya
   rawat inap, lalu menagih atau mengembalikan selisih.
8. Biaya administrasi dapat ditanggung penjamin atau pasien tetapi tidak boleh menerima diskon.
9. Finance mengusulkan kebijakan/nominal; IT mengonfigurasi sistem berdasarkan approval Finance.
10. Perubahan harga berlaku pada invoice yang masih terbuka menurut locking rule dan wajib
    meninggalkan histori nilai lama serta baru.

## Aturan diskon

| Jenis diskon | Dampak | Approval |
| --- | --- | --- |
| Master promo | Mengurangi total/porsi eligible sesuai master; default hanya mengurangi porsi pasien | Tidak memerlukan approval per transaksi karena master sudah disetujui Finance |
| Master pemeriksaan | Mengurangi pemeriksaan tertentu sesuai master | Tidak memerlukan approval per transaksi |
| Diskon ad-hoc | Penyesuaian di luar master | Wajib approval Finance dan tidak boleh self-approval |
| Diskon dokter | Hanya mengurangi komponen Share Dokter, bukan seluruh harga tindakan | Dokter memutuskan; Kasir menginput; dokter yang sama melakukan approval satu layer |
| Exception diskon dokter | Memengaruhi bagian rumah sakit, melewati batas, dilakukan setelah settlement dimulai, atau setelah AP dokter terbentuk | Wajib approval Finance |

Biaya administrasi tidak pernah menjadi basis diskon.

## Pembatalan, koreksi, refund, dan write-off

1. Item boleh dibatalkan bila pelayanan belum dilakukan dan belum memiliki pembayaran yang
   membuatnya tidak dapat dibatalkan melalui jalur normal.
2. Pembatalan tidak menghapus record; billing item menjadi `Voided` dan tidak masuk total.
3. Pembatalan klinis berasal dari pembuat order atau klinisi pengganti yang berwenang.
4. Kasir tidak memutus pembatalan dan tidak melakukan refund.
5. Unit pelaksana mengonfirmasi pembatalan OTC; Finance mengeksekusi refund.
6. Refund terhubung ke transaksi asli dan dikembalikan ke metode pembayaran asal.
7. Kesalahan setelah pelayanan dilakukan dikoreksi melalui amendment/adjustment; data lama tidak
   ditimpa dan tidak dihapus.
8. Perubahan harga, kuantitas, atau coverage setelah pembayaran membutuhkan adjustment dan
   approval yang berlaku.
9. Write-off pasien dapat parsial atau penuh, diajukan Operasional Billing/AR, dan disetujui
   Finance. Write-off tidak mengubah status menjadi seolah-olah dibayar tunai.
10. Bila pelayanan ditemukan setelah invoice ditutup, pasien tidak ditagih ulang; kasus menjadi
    insiden internal rumah sakit. Karena itu closure gate wajib memastikan tidak ada sumber biaya
    yang tertinggal.

## Pembayaran dan reconciliation

1. Split payment menyimpan setiap tender secara independen.
2. Tender sukses tidak dibatalkan hanya karena tender lain gagal.
3. Setiap percobaan pembayaran memiliki ID idempotency dan referensi provider yang unik.
4. Lifecycle tender eksternal adalah `Initiated`, `Pending`, lalu salah satu dari `Settled`,
   `Failed`, atau `Expired`; transaksi settled dapat menjadi `Reversed` melalui kewenangan Finance.
5. Timeout atau respons terputus menjadi `Pending Reconciliation`, bukan langsung dianggap gagal.
6. Kasir tidak boleh menagih ulang tender yang masih pending.
7. Inquiry provider, webhook idempotent, dan rekonsiliasi settlement harus dapat menyelesaikan
   status tanpa membuat pembayaran ganda.
8. Salah input yang belum settled boleh di-void Kasir pada shift yang sama. Setelah settled,
   hanya Finance yang boleh melakukan reversal/refund.

## Shift Kasir

1. Setiap penerimaan Kasir terikat pada shift/register yang terbuka.
2. Riwayat shift memuat saldo awal, saldo akhir, pendapatan cash menurut sistem, kas fisik, dan
   selisih.
3. Kasir wajib menyelidiki sumber selisih terlebih dahulu.
4. Jika selisih tetap ada, Kasir melaporkan kepada Kepala Kasir untuk ditinjau dan diselesaikan.
5. Batas toleransi selisih dapat dikonfigurasi tanpa mengubah invariant pencatatan dan audit.

## Lifecycle invoice

| Dari status | Tindakan | Ke status | Authority | Syarat utama |
| --- | --- | --- | --- | --- |
| — | Membuat invoice pertama untuk encounter | `OPEN` | Sistem Billing | Belum ada invoice aktif untuk encounter tersebut |
| `OPEN` | Seluruh order selesai/void dan siap diperiksa | `READY_FOR_SETTLEMENT` | Sistem/Kasir | Semua sumber biaya sudah direkonsiliasi |
| `READY_FOR_SETTLEMENT` | Memulai checkout rawat jalan/OTC | `SETTLEMENT_IN_PROGRESS` | Kasir | Snapshot finansial dikunci |
| `OPEN` | Menerima deposit/progress payment rawat inap | `OPEN` | Kasir | Tidak mengunci invoice; histori dana tetap tercatat |
| `READY_FOR_SETTLEMENT` | Memulai final settlement rawat inap | `SETTLEMENT_IN_PROGRESS` | Kasir | Semua order selesai dan snapshot dikunci |
| `SETTLEMENT_IN_PROGRESS` | Patient responsibility menjadi nol | `PATIENT_SETTLED` | Sistem | Semua tender wajib telah terselesaikan |
| `PATIENT_SETTLED` | Membentuk posting AR/AP dan menyelesaikan finalisasi | `CLOSED` | Sistem | Posting idempotent berhasil direkam |

Reopen oleh Petugas Billing hanya untuk koreksi administratif nonfinansial dengan alasan dan
audit. Invoice tertutup tidak boleh menerima billing item baru.

## Business invariants

1. Satu `EncounterId` memiliki tepat satu invoice Billing.
2. Satu sumber pelayanan memiliki paling banyak satu billing item aktif melalui identitas sumber
   yang stabil.
3. Kegagalan membuat billing item tidak boleh menghasilkan pelayanan sukses yang tidak terlihat
   oleh Billing; transaksi harus atomik atau memiliki recovery/reconciliation yang terbukti.
4. Tidak ada hard delete untuk item, pembayaran, deposit, diskon, adjustment, refund, write-off,
   AR, AP, atau shift.
5. Semua order harus `Completed` atau `Voided` sebelum final settlement.
6. Tidak boleh ada pembayaran `Pending Reconciliation` ketika invoice ditutup.
7. `PATIENT_SETTLED` berarti patient responsibility nol, bukan berarti AR penjamin telah dibayar.
8. Deposit dan progress payment rawat inap tidak mengunci invoice.
9. Locking event rawat jalan/OTC adalah dimulainya checkout; locking event rawat inap adalah
   dimulainya final settlement.
10. Invoice yang sudah dikunci hanya berubah melalui adjustment/reversal yang berwenang.
11. Perubahan harga invoice terbuka wajib dapat menjelaskan nilai lama, nilai baru, sumber aturan,
    waktu, dan aktor.
12. Master fee/discount yang sudah berubah tidak menghitung ulang invoice tertutup.
13. Kasir tidak boleh mengesahkan pembatalan klinis atau refund.
14. AR dan AP lahir tepat sekali saat invoice final melalui proses idempotent.
15. AP dokter tidak otomatis siap dibayar hanya karena record AP sudah terbentuk.

## Fakta capability existing dari source

1. `BillingManagement` saat audit hanya memiliki master metode pembayaran dan kategori billing
   item; transaksi invoice, pembayaran pasien, deposit, refund, dan shift Kasir belum tersedia.
2. `TrxPatientProcedure` sudah menyimpan snapshot tarif, coverage, patient pay, `BillingItemId`,
   `IsBillingGenerated`, dan `BillingGeneratedAt`.
3. Pembuatan tindakan menghitung tarif/coverage tetapi selalu membuat
   `IsBillingGenerated = false`; tidak ditemukan proses yang mengubahnya menjadi `true`.
4. Endpoint tindakan melarang perubahan/pembatalan ketika `IsBillingGenerated`, tetapi marker
   tersebut belum pernah dibentuk oleh proses Billing aktual.
5. Frontend tindakan dokter memanggil endpoint select procedure dan menyatakan tindakan sudah
   tersimpan; pernyataan itu belum berarti billing item sudah dibuat.
6. Resep memiliki workflow marker Billing tetapi marker tersebut belum membentuk transaksi
   invoice yang authoritative.
7. Detail order laboratorium/radiologi/penunjang masih dinyatakan akan berada pada
   `OrderManagement`; capability tersebut belum tersedia pada audit ini.

## Decision log

| Decision ID | Type | Item | Owner | Status | Approval evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-001` | Decision | Pass ini melengkapi Billing yang sudah ada, bukan membuat kebijakan tanpa melihat capability existing | Product/Domain Owner | `approved` | Jawaban owner 19 Agustus 2026 |
| `BKC-DEC-002` | Decision | Pemberi jawaban berwenang menentukan alur Billing dan Kasir | Product/Domain Owner | `approved` | Pernyataan eksplisit owner |
| `BKC-DEC-003` | Decision | Satu kunjungan/encounter memiliki satu invoice | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-004` | Decision | Semua item pelayanan yang ditagihkan harus masuk Billing tepat satu kali | Product/Domain Owner | `approved` | Jawaban owner dan approval blueprint |
| `BKC-DEC-005` | Decision | Pembatalan menggunakan `Voided`, bukan penghapusan fisik | Product/Domain Owner | `approved` | Owner menyetujui rekomendasi audit |
| `BKC-DEC-006` | Decision | Kasir mendukung split payment; tender sukses bertahan ketika tender lain gagal | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-007` | Decision | Patient settled ditentukan oleh lunasnya porsi pasien; penjamin tetap menjadi AR | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-008` | Superseded | Pembayaran pertama selalu mengunci seluruh invoice | Product/Domain Owner | `superseded` | Diganti oleh `BKC-DEC-009` dan `BKC-DEC-010` |
| `BKC-DEC-009` | Decision | Checkout rawat jalan/OTC mengunci snapshot finansial | Product/Domain Owner | `approved` | Klarifikasi owner |
| `BKC-DEC-010` | Decision | Deposit/progress payment rawat inap tidak mengunci invoice; final settlement yang mengunci | Product/Domain Owner | `approved` | Contoh Rp10 juta/Rp8 juta/Rp5 juta disetujui |
| `BKC-DEC-011` | Decision | OTC mencakup poliklinik, lab, dan radiologi; resep tebus belum termasuk | Product/Domain Owner | `approved` | Koreksi istilah OTV menjadi OTC |
| `BKC-DEC-012` | Decision | Pembatalan OTC dikonfirmasi unit pelaksana dan refund dilakukan Finance | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-013` | Decision | Tarif, coverage, diskon, dan responsibility dinamis selama invoice belum dikunci | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-014` | Decision | Master discount otomatis; diskon ad-hoc memerlukan Finance | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-015` | Decision | Diskon dokter hanya mengurangi Share Dokter dan memerlukan approval dokter | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-016` | Decision | Rawat inap memakai deposit, top-up, progress payment, final allocation, dan refund sisa | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-017` | Decision | Biaya admin nominal tetap; rawat jalan satu kali per pasien per hari dan rawat inap satu kali per admission | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-018` | Decision | Transfer rawat jalan ke rawat inap pada encounter yang sama mengganti biaya admin rawat jalan dengan rawat inap | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-019` | Decision | Biaya admin dapat ditanggung pasien/penjamin tetapi tidak dapat didiskon | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-020` | Decision | Write-off pasien parsial/penuh diajukan Billing/AR dan disetujui Finance | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-021` | Decision | Kasir memiliki shift/register dengan rekonsiliasi dan eskalasi selisih ke Kepala Kasir | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-022` | Decision | Payment timeout menjadi `Pending Reconciliation` dan tidak boleh langsung ditagih ulang | Product/Domain Owner | `approved` | Owner menerima solusi |
| `BKC-DEC-023` | Decision | Tidak ada late charge setelah invoice ditutup; item tertinggal menjadi insiden internal | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-024` | Decision | AR penjamin dan AP dokter lahir saat invoice final | Product/Domain Owner | `approved` | Klarifikasi terakhir owner |
| `BKC-DEC-025` | Decision | AP dokter baru siap dibayar setelah settlement/policy pemilik AP terpenuhi | Product/Domain Owner | `approved` | Klarifikasi terakhir owner |
| `BKC-DEC-026` | Decision | Kasir memfinalkan invoice setelah seluruh order complete/void dan source biaya direkonsiliasi | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-027` | Decision | Petugas Billing boleh reopen administratif nonfinansial tanpa approval supervisor | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-028` | Decision | Finance mengusulkan master biaya; IT melakukan konfigurasi berdasarkan approval Finance | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-029` | Decision | Klaim penjamin ditolak tetap menjadi AR rumah sakit dan tidak otomatis dialihkan ke pasien | Product/Domain Owner | `approved` | Jawaban owner |
| `BKC-DEC-030` | Decision | Blueprint `BIL-CASH-001` revision `0.1` disetujui | Product/Domain Owner | `approved` | Pernyataan eksplisit 20 Agustus 2026 |
| `BKC-DEC-031` | Decision | Read access dipisah: Kasir untuk invoice/porsi pasien/deposit/payment/discount/reference; Billing untuk invoice/coverage/adjustment/reconciliation lintas unit; Finance/AR untuk seluruh financial data; dokter/unit hanya item miliknya dan settlement status; Kepala Kasir untuk transaksi/shift seluruh kasir | Product/Domain Owner + Security/Finance | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-032` | Decision | Tidak ada bypass pembayaran dalam OTC; kondisi darurat dialihkan menjadi encounter IGD/emergency sehingga pelayanan keselamatan tidak tertahan | Product/Domain Owner + Clinical/Billing | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-033` | Decision | Normal departure mengikuti clinical discharge, charge reconciliation, invoice final, dan pelunasan; kematian, transfer darurat, serta DAMA/APS tidak ditahan oleh outstanding, yang menjadi AR kepada pasien/penanggung sah dengan alasan dan debtor tercatat | Product/Domain Owner + Inpatient/Registration/Billing | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-034` | Decision | Pengganti pembatal harus klinisi dengan profesi/unit sama, disetujui kepala unit/koordinator shift, mencatat alasan, dan unit pelaksana mengonfirmasi pelayanan belum dilakukan; Finance hanya menangani refund | Clinical Governance Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-035` | Decision | Refund split dialokasikan proporsional ke metode asal; keberhasilan parsial dipertahankan, kegagalan menjadi `REFUND_PENDING`, dan metode pengganti hanya oleh Finance setelah kegagalan serta identitas pasien diverifikasi | Finance/Payment Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-036` | Decision | Write-off tidak pernah `PAID`; full write-off menjadi `SETTLED_BY_WRITE_OFF`, partial mengurangi outstanding, dan reversal membuka kembali AR melalui entry koreksi tanpa menghapus histori | Finance/AR + Product/Domain Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-037` | Decision | Share Dokter bersumber dari komponen tarif billing item final, dapat dihitung ulang sebelum final, dan doctor discount hanya menguranginya; AP lahir saat final, self-pay ready setelah porsi pasien lunas, insured ready setelah porsi pasien lunas dan claim approved, dengan policy payer dapat mensyaratkan claim paid | AP/Finance + Medical Service Fee Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-038` | Decision | Kasir membuka satu shift aktif dengan saldo awal; close mencatat system cash, physical cash, ending balance, variance; variance direview Kepala Kasir, reopen berotorisasi/audit, handover melibatkan dua kasir, dan late noncash settlement tidak mengubah physical cash shift tertutup | Kepala Kasir/Finance Operations | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-039` | Decision | Billing item unik/idempotent pada `(SourceDomain, SourceDetailId)`; procedure/lab/radiology terbentuk saat order confirmed/accepted, performed menutup normal cancellation, pharmacy final mengikuti dispensed quantity, consumable per usage detail, admin dari Billing rule, dan room charge dipisah ke `043` | Producer Owners + Billing Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-040` | Decision | Posting final immutable; koreksi memakai Finance-approved adjustment version, debit/credit AR/AP, outstanding/refundable credit pasien, serta correlation/idempotency key | Billing/AR/AP/Finance Accounting | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-041` | Decision | Pajak tidak global; effective-dated tax master menentukan taxable item/rate/basis, dihitung setelah item discount, dialokasikan menurut patient/payer responsibility dan contract, memakai decimal serta rounding konsisten | Finance/Tax Owner + Product/Domain Owner | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-042` | Decision | Primary dihitung lebih dulu; excess hanya menilai residual dengan kontraknya sendiri; total coverage tidak melebihi eligible charge; AR final per debtor; rejected claim tidak otomatis pindah ke pasien kecuali contract/policy sah mengizinkan | Payer/Insurance + Finance/AR | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026. **Diamendemen sebagian oleh `BKC-DEC-062` (approved Product/Domain Owner, 2 September 2026 — TANPA konfirmasi terpisah Payer/Insurance+Finance/AR, lihat caveat pada baris `BKC-DEC-062`)** untuk kasus spesifik rule `CoverageStatus=Covered` yang butuh approval/surat jaminan — lihat amendment lanjutan di bawah. |
| `BKC-DEC-043` | Decision | Occupancy timeline adalah source of truth; policy 24 jam, minimum satu hari, rounding sisa, tarif awal periode, leave, dan variasi kontrak dibuat configurable/effective-dated; transfer tidak overlap/reset minimum dan correction memakai adjustment | Inpatient + Billing/Finance | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
| `BKC-DEC-044` | Decision | `InvoiceDate` ditetapkan saat final dan tidak berubah karena payment; self-pay due pada invoice date, payer due sesuai contract setelah claim diterima, AR age mulai posting, overdue terhadap `DueDate`, dan payment date hanya settlement | Billing/AR/Finance | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |

## Acceptance criteria amendment `0.2`

| Decision | Testable acceptance criterion |
| --- | --- |
| `BKC-DEC-031` | Setiap actor hanya menerima financial fields dan scope unit/register yang diizinkan; dokter/unit tidak menerima deposit, payment detail, atau AR |
| `BKC-DEC-032` | OTC unsettled tidak dapat release service; emergency conversion menghasilkan encounter IGD dan tidak menahan pelayanan klinis |
| `BKC-DEC-033` | Normal departure menolak outstanding; death/emergency transfer/DAMA mencatat exception reason, lawful debtor, dan AR tanpa menahan departure |
| `BKC-DEC-034` | Substitute cancellation ditolak tanpa same-profession/unit authority, approval, reason, dan not-performed confirmation |
| `BKC-DEC-035` | Split refund menghasilkan nominal proporsional per original tender; partial failure menyisakan `REFUND_PENDING` tanpa menggandakan refund sukses |
| `BKC-DEC-036` | Full write-off menghasilkan `SETTLED_BY_WRITE_OFF`, partial menyisakan balance, dan reversal memulihkan AR melalui compensating entry |
| `BKC-DEC-037` | Final billing item menghasilkan satu AP basis; discount hanya mengurangi doctor share dan readiness mengikuti source/payment policy yang disetujui |
| `BKC-DEC-038` | Shift close menyimpan system/physical/variance; unresolved variance tetap terlihat, reopen/handover tervalidasi, dan late noncash tidak mengubah closed cash balance |
| `BKC-DEC-039` | Retry `(SourceDomain, SourceDetailId)` menghasilkan satu active item; quantity farmasi sama dengan dispensed quantity dan performed source tidak dapat normal-cancel |
| `BKC-DEC-040` | Koreksi final tidak memutasi posting lama; retry adjustment tidak menggandakan AR/AP dan patient delta menjadi outstanding/refundable credit yang tepat |
| `BKC-DEC-041` | Hanya item dengan effective tax rule yang menghasilkan tax; calculation memakai post-discount basis dan allocation/rounding yang dapat direproduksi |
| `BKC-DEC-042` | Excess tidak pernah auto-cover tanpa kontrak; primary + excess tidak melebihi eligible charge dan AR dipisahkan per debtor |
| `BKC-DEC-043` | Occupancy segments tidak overlap; effective policy menghasilkan room charge deterministik dan correction menambah adjustment tanpa overwrite history |
| `BKC-DEC-044` | Payment tidak mengubah `InvoiceDate`/AR age; self-pay dan payer memiliki `DueDate` sesuai policy dan overdue hanya setelah due date |

Semua acceptance criteria amendment revision `0.2` disetujui. Approval keputusan ini belum menjadi
task implementation atau write authority aplikasi.

## Koreksi, konflik, dan keputusan yang disupersede

| Item awal | Koreksi final | Dampak |
| --- | --- | --- |
| Refund disebut hanya untuk rawat jalan | Yang dimaksud adalah refund sisa deposit rawat inap; OTC yang dibatalkan sebelum pelayanan juga dapat direfund | Refund tidak dibatasi ke satu jenis encounter |
| Integrasi penjamin disebut AP | Yang benar adalah AR | Porsi penjamin menjadi piutang rumah sakit |
| Istilah OTV | Yang benar adalah OTC, *Over The Counter* | Scope OTC dikunci ke poliklinik, lab, dan radiologi |
| Item “dihapus” dari Billing | Item menjadi `Voided` dan dikeluarkan dari total tanpa hard delete | Audit finansial tetap utuh |
| Pembayaran pertama mengunci invoice | Berlaku pada checkout rawat jalan/OTC; rawat inap boleh progress payment selama invoice tetap terbuka | Mendukung cicilan selama perawatan tanpa kehilangan charge baru |
| AP/AR dibentuk setelah semuanya dibayar | AR dan AP lahir saat invoice final; AP readiness serta pelunasan AR memiliki lifecycle sendiri | Menghindari AR dibuat setelah receivable sudah dibayar |

Tidak ada konflik di antara keputusan `BKC-DEC-001`–`030` yang sudah approved. Attachment baru
memperlihatkan legacy behavior yang bertentangan dengan target—auto white-off 90 hari, paid boolean,
doctor FoC, excess fallback, dan invoice-on-payment—serta memicu `BKC-DEC-041`–`044`. Legacy
behavior tidak mengungguli keputusan approved.

## Open questions dan blockers

### Tidak memblokir domain design

1. Format nomor invoice final.
2. Aturan pembulatan nominal rupiah.
3. Nilai toleransi selisih shift.
4. Parameter dan batas nominal master discount.
5. Detail policy yang mengubah AP dokter dari terbentuk menjadi siap dibayar; policy ini dimiliki
   context AP dokter, bukan Billing.
6. Format nomor invoice legacy `INVB{sequence}{ddMMyyyy}`; format tidak otomatis diadopsi.

Keenam item tersebut harus dikunci sebagai konfigurasi/kontrak sebelum task implementasi yang
terdampak dianggap selesai, tetapi tidak mengubah ownership atau lifecycle inti Billing.

### Reassessment dependency

Tidak ada lagi open business decision pada `BKC-DEC-031`–`044`. Requirement gate, domain
architecture, dan business-module blueprint revision sebelumnya masih menggambarkan decisions
tersebut sebagai blocker sehingga wajib menjalani reassessment/recomposition sebelum delivery plan.

Backend engineering contract dan ownership registry yang dahulu tidak tersedia sekarang ada;
registry mencatat `BillingManagement / Billing` prefix `Bil`. Blocker governance lama ditutup,
tetapi tidak memberikan implementation authority dan tidak menutup keputusan bisnis di atas.

## Acceptance criteria bisnis

1. Menambahkan pelayanan menghasilkan tepat satu billing item atau seluruh transaksi gagal dan
   dapat dipulihkan tanpa charge tersembunyi.
2. Retry request tidak membuat invoice atau billing item ganda.
3. Invoice terbuka memperbarui tarif/coverage sesuai master dan menyimpan audit nilai lama/baru.
4. Checkout rawat jalan/OTC mengunci snapshot; progress payment rawat inap tidak menguncinya.
5. OTC tidak dapat dilayani sebelum seluruh patient responsibility settled; kondisi darurat harus
   dikonversi ke encounter IGD/emergency tanpa menahan pelayanan klinis.
6. Pembatalan sebelum pelayanan membuat item `Voided`; data lama tetap terlihat auditor.
7. Split tender menghitung outstanding hanya dari bagian yang belum settled.
8. Tender eksternal yang tidak pasti masuk `Pending Reconciliation` dan tidak ditagih ulang.
9. Final settlement rawat inap mengalokasikan progress payment dan deposit sebelum menagih
   kekurangan atau membentuk refundable credit.
10. Biaya admin rawat jalan tidak muncul pada invoice kedua pasien di tanggal lokal yang sama.
11. Transfer menjadi rawat inap mengganti biaya admin tanpa menghasilkan dua biaya aktif.
12. Invoice tidak dapat ditutup jika order belum complete/void, ada source biaya belum masuk,
    atau pembayaran masih pending.
13. Invoice final membentuk posting AR idempotent per debtor/final version dan satu basis AP dokter
    per eligible billing item tanpa duplicate effect.
14. Refund hanya dapat dieksekusi Finance dan selalu terhubung ke pembayaran asal.
15. Write-off tidak tercatat sebagai pembayaran tunai dan memiliki maker/approver serta alasan.
16. Tutup shift menampilkan saldo awal/akhir, cash sistem, kas fisik, dan selisih untuk Kepala
    Kasir.

## UI decision authority

Aturan keamanan, privasi, invariant finansial, approval, dan state backend mengungguli pilihan
UI. Blueprint belum menetapkan menu, route, layout, warna, atau komposisi visual. Area tersebut
berstatus `DEV_DISCRETION` selama UI:

1. tidak menyembunyikan status pending/reconciliation;
2. tidak menghitung ulang aturan finansial secara mandiri;
3. menampilkan asal perubahan, approval, dan alasan pada tindakan high-risk;
4. mencegah double submit dan menjelaskan outstanding yang sebenarnya;
5. memisahkan dengan jelas saldo deposit, progress payment, payment settled, AR, dan refundable
   credit.

## Approval 20 Agustus 2026

| Field | Nilai |
| --- | --- |
| Yang menyetujui | Product/Domain Owner pada sesi wawancara; nama formal belum dicatat |
| Tanggal | 20 Agustus 2026 |
| Bentuk persetujuan | Pernyataan eksplisit: “Saya setujui blueprint BIL-CASH-001 revision 0.1.” |
| Cakupan | Scope, actors, ownership, lifecycle, cancellation, payment, deposit/progress payment, insurance, AR/AP, biaya admin, diskon, write-off, shift Kasir, audit, dan acceptance criteria |
| Pengecualian | Approval ini tidak menggantikan governance owner/prefix backend dan tidak mengizinkan eksekusi database |

## Approval amendment 20 Agustus 2026

| Field | Nilai |
| --- | --- |
| Yang menyetujui | Product/Domain Owner pada sesi wawancara; nama formal belum dicatat |
| Tanggal | 20 Agustus 2026 |
| Bentuk persetujuan | Pernyataan eksplisit: “Saya menyetujui amendment keputusan BIL-CASH-001 revision 0.2.” |
| Cakupan | `BKC-DEC-031`–`044` dan acceptance criteria amendment `0.2` |
| Pengecualian | Tidak menyetujui source implementation, migration/database execution, deployment, atau Git publication |

## Handoff

Closure interview dan approval `BKC-DEC-031`–`044` selesai. Next action adalah menjalankan kembali
`requirement-completeness-gate`, lalu `hospital-domain-architect` dan `design-business-module`
untuk memperbarui kontrak terdampak sebelum `plan-module-delivery`.

Delivery planning dan implementation tetap berhenti sampai amendment approved, blueprint target
disetujui, dan task/write authority diberikan secara terpisah.

## Amendment 27 Agustus 2026 — Layar Menu Pembayaran Kasir

Sesi wawancara `/grill-me` mode **Amendment pass** (blueprint `BIL-CASH-001` sudah `approved`
revision `0.4`). Backend SHA yang tercatat pada `blueprint-manifest.md` (`c99f0a51...`) sudah
berbeda dari `HEAD` saat sesi ini (`e047e39`) — map berpotensi sebagian basi, tapi wawancara tetap
dijalankan karena tidak ada perubahan struktural yang diketahui pada domain terkait keputusan di
bawah ini.

### Batas scope pass ini

**Di dalam scope**: layar "Menu Pembayaran" kasir — ringkasan tagihan (tindakan/resep/kamar),
"Tambah Biaya Lain-lain", Promo/Voucher, Diskon Dokter, Catatan, Ringkasan Pembayaran (Subtotal
Mandiri/Asuransi/Pajak/Harus Dibayar + status Lunas), modal "Pilih Metode Pembayaran"
(Tunai/QRIS/Transfer Bank/Metode Lainnya + Nomor Referensi), split payment lintas metode, dan
tombol Proses Pembayaran. Dikonfirmasi pengguna agar tetap memperhatikan konfigurasi/master data
billing yang belum lengkap, mengacu pada referensi legacy `KasirQuilvian1/` (`BeKasir`,
`FE kasir app`, `FE kasir view` — implementasi kasir versi sebelumnya, dipakai sebagai bukti
perilaku as-is, bukan kontrak yang otomatis mengikat).

**Di luar scope — untuk pass/modul lain**:

- Layar worklist Kasir IGD/Rawat Jalan/Rawat Inap (daftar antrian kasir) — entry point ke Menu
  Pembayaran, tapi punya keputusan UI/filter sendiri; belum digali di pass ini.
- Riwayat Pembayaran (layar riwayat terpisah) — belum digali.
- Shift Kasir — sudah diputuskan `BKC-DEC-038`, sudah dibangun; tidak dibuka ulang.
- Master Diskon (CRUD kebijakan diskon) — sudah ada (`MstDiscountPolicy` + layanan terkait);
  pass ini hanya menyangkut cara Menu Pembayaran MEMAKAI kebijakan yang sudah ada, bukan
  aturan pembuatan kebijakannya.
- Invoice & Billing (layar daftar invoice back-office) — sudah ada dari pekerjaan FE
  sebelumnya (`FE-BKC-003`–`010`).
- Layar console approval Diskon Dokter/Diskon Direksi (antrian approval terpisah) — aturan
  approval-nya sudah diputuskan (lihat `BKC-DEC-046`), tapi UI antrian approval itu sendiri
  adalah slice terpisah dari Menu Pembayaran.
- **Petty Cash** — tidak disebut di dokumen blueprint billing-kasir manapun sebelum pass ini.
  Pengguna menyatakan ini bagian dari billing-kasir dan perlu digali pada sesi `/grill-me`
  lanjutan; dicatat sebagai antrian, bukan diputuskan di sini.

### Keputusan baru

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-045` | Decision | Diskon Dokter dan Promo/Voucher pada Menu Pembayaran memakai mekanisme `MstDiscountPolicy` + `BilDiscountApplication` yang sudah ada (endpoint `POST .../billing/invoices/{id}/discounts` dan `POST .../discounts/{discountId}/approve`); kode voucher pada Menu Pembayaran adalah pencarian/filter terhadap `MstDiscountPolicy.Code` yang sudah ada, bukan mekanisme redemption baru. Tidak ada field atau endpoint backend baru yang diperlukan untuk keperluan ini. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; dikonfirmasi dari source `MstDiscountPolicy.cs` (field `Code`, `RequiresApproval`, `ApproverRole` sudah ada, tidak ada field redemption/single-use) |
| `BKC-DEC-046` | Decision | Selama ada Diskon Dokter berstatus menunggu approval (`RequiresApproval = true`, belum disetujui/ditolak) pada invoice yang sedang dibayar, tombol Proses Pembayaran WAJIB dinonaktifkan/diblokir sampai approval selesai (disetujui atau ditolak). Kasir tidak dapat memproses pembayaran dengan diskon yang masih menggantung. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 |
| `BKC-DEC-047` | Decision | "Tambah Biaya Lain-lain" mengizinkan kasir mengisi nama item/layanan dan harga secara BEBAS (tidak dibatasi katalog/master tarif resmi). Sebagai kompensasi kontrol, setiap baris biaya bebas WAJIB tercatat pada audit log (identitas kasir, waktu, nominal, kategori, keterangan) melalui `LoggerService.AuditAsync`, tanpa gerbang approval tambahan sebelum bisa dibayarkan. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 |
| `BKC-DEC-048` | Decision | Field "Catatan (Opsional)" pada Menu Pembayaran bersifat internal — hanya terlihat oleh kasir/petugas billing pada layar dan riwayat internal, dan TIDAK PERNAH dicetak pada struk/Dokumen Kasir yang diserahkan ke pasien. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 |
| `BKC-DEC-049` | Decision | Kasir BOLEH secara sengaja memasukkan nominal pembayaran lebih kecil dari "Harus Dibayar" dan menekan Proses Pembayaran, menghasilkan status Pembayaran Sebagian (`BillingSettlementStatuses.PartiallySettled`) yang sudah ada di source. Ini bukan status yang hanya muncul dari kegagalan sistem/split payment — melainkan pilihan sah kasir untuk kasus pasien yang hanya mampu membayar sebagian saat itu; sisanya tetap tercatat sebagai outstanding invoice. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; dikonfirmasi `BilSettlement.Status` sudah memiliki nilai `PARTIALLY_SETTLED` di source |
| `BKC-DEC-050` | Decision | Saat split payment sebagian gagal: tender yang sudah `SUCCEEDED` TETAP dipertahankan dan tidak di-rollback otomatis; tender yang gagal dicatat `FAILED`, dan outstanding invoice dihitung hanya dari sisa yang belum berhasil. Kasir hanya boleh memilih metode pembayaran lain untuk SISA yang belum berhasil itu — bukan mengulang seluruh nominal. Khusus tender berstatus `PENDING` (belum pasti berhasil/gagal, mis. menunggu callback provider): kasir DILARANG memproses pembayaran ulang untuk porsi itu sampai proses reconciliation memastikan status akhirnya (`SUCCEEDED` atau `FAILED`) — mencegah pasien tertagih dua kali untuk porsi yang sama. Invoice baru dianggap Lunas setelah seluruh porsi tanggung jawab pasien (`patient responsibility`) bernilai nol. | Product/Domain Owner | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 (detail); konsisten dengan `BillingTenderStatuses` yang sudah ada di source (`CREATED`, `PENDING`, `SUCCEEDED`, `FAILED`, `EXPIRED`, `REVERSED` — satu status independen per tender) |
| `BKC-DEC-051` | `DEV_DISCRETION` | "Metode Lainnya" pada modal Pilih Metode Pembayaran adalah pengelompokan TAMPILAN saja atas `MstPaymentMethod` yang tidak masuk kategori Tunai/QRIS/Transfer Bank. Perilaku tiap metode di dalamnya (butuh approval, butuh nomor referensi, dst.) tetap mengikuti flag per-metode yang sudah ada (`IsNeedApproval`, `IsNeedReferenceNumber`, dll.) — TIDAK ADA aturan tambahan berdasarkan nama grup. | Product/Domain Owner (delegasi ke dev discretion) | `draft` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 |

### Amendment lanjutan 27 Agustus 2026 — Dokumen Kasir (Kwitansi)

Implementasi Menu Pembayaran (`BKC-DEC-045`–`051`) memunculkan kebutuhan "Dokumen Kasir" yang
awalnya dibangun sebagai placeholder non-aktif. Pengguna meminta digali sekarang juga dalam pass
yang sama.

**Batas scope tambahan**: "Dokumen Kasir" pada referensi mencakup enam dokumen — SPT, Claim
Letter, LML, LMA, Resep Obat, dan Bukti Pembayaran (Kwitansi) — yang semuanya diambil dari
transaksi pelayanan kesehatan. Hanya **Kwitansi** yang menjadi tanggung jawab billing-kasir untuk
dibangun sekarang; lima dokumen lain (SPT, Claim Letter, LML, LMA, Resep Obat) adalah milik
modul klinis/farmasi/asuransi masing-masing dan HANYA ditautkan (tab) dari Menu Pembayaran, bukan
dibangun ulang logikanya di sini.

**Di luar scope — untuk modul lain**: pembuatan/pengisian konten SPT, Claim Letter, LML, LMA, dan
Resep Obat (perlu `/grill-me` tersendiri per modul pemilik, bila belum ada) — hanya SHELL tab
placeholder yang dibangun di Menu Pembayaran untuk kelimanya.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-052` | Decision | Dari enam dokumen pada "Dokumen Kasir", hanya Kwitansi (Bukti Pembayaran) yang menjadi tanggung jawab billing-kasir. SPT, Claim Letter, LML, LMA, dan Resep Obat adalah dokumen milik modul lain (klinis/farmasi/asuransi) — Menu Pembayaran hanya menyediakan tab/tautan ke dokumen itu (placeholder sampai modul pemiliknya membangunnya), tidak menduplikasi logikanya. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |
| `BKC-DEC-053` | Decision | Dokumen Kwitansi dirender sebagai PDF di sisi frontend (bukan endpoint backend penghasil dokumen) dari data invoice/settlement/patient yang sudah dimuat Menu Pembayaran. Backend HANYA bertanggung jawab atas alokasi nomor Kwitansi (lihat `BKC-DEC-054`), bukan atas pembuatan dokumennya. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |
| `BKC-DEC-054` | Decision | ~~Kwitansi WAJIB memiliki nomor dokumen resmi yang tersimpan di database, dialokasikan memakai mekanisme penomoran berurutan bereset harian yang SUDAH ADA (`BillingNumberSeriesService`/`BilNumberSeries`, prefix baru `KWS`) — bukan tabel sequence baru terpisah. Nomor dialokasikan HANYA SEKALI per invoice (saat Kwitansi pertama kali diminta) dan disimpan pada invoice; permintaan berikutnya (reprint) untuk invoice yang sama mengembalikan nomor yang sama.~~ **`superseded` oleh `BKC-DEC-057`** — granularitas per-invoice ternyata tidak konsisten dengan bukti legacy (`KasirQuilvian1`) yang baru ditemukan lewat `/trace-existing-capabilities` 27 Agustus 2026, dan sudah dikoreksi. | Product/Domain Owner | `superseded` | Jawaban eksplisit sesi wawancara 27 Agustus 2026 (termasuk contoh kode acuan pengguna); direkomendasikan salah alih-alih diperiksa dulu terhadap pola legacy — lihat `BKC-DEC-057` |
| `BKC-DEC-055` | Decision | Kwitansi boleh dicetak/dikirim kapan pun setelah ada pembayaran (penuh atau sebagian) — badge status pada dokumen menyesuaikan (LUNAS/PAID IN FULL vs status sebagian), bukan hanya setelah outstanding invoice bernilai nol. Konsisten dengan `BKC-DEC-049` yang sudah mengizinkan pembayaran sebagian sebagai pilihan sah kasir. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |
| `BKC-DEC-056` | Decision | Tombol WhatsApp/Email pada Dokumen Kasir TIDAK mengirim file terlampir secara otomatis dalam satu klik (Web Share API untuk file tidak didukung merata di browser desktop kasir). Perilakunya: unduh PDF Kwitansi terlebih dahulu, lalu tombol WhatsApp/Email membuka aplikasi (wa.me/mailto) dengan teks pesan siap pakai; kasir melampirkan file yang sudah terunduh secara manual. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |

### Acceptance criteria bisnis (lanjutan, no. 22 dst. — Dokumen Kasir)

22. ~~Nomor Kwitansi yang sama selalu dikembalikan untuk invoice yang sama pada permintaan
    berulang (reprint); tidak pernah ada dua nomor Kwitansi berbeda untuk satu invoice yang
    sama.~~ **Diganti (`BKC-DEC-057`)**: setiap tender yang berhasil ditambahkan pada suatu
    settlement mendapat SATU nomor Kwitansi sendiri, dialokasikan sekali saat tender dibuat;
    reprint pada tender yang sama selalu mengembalikan nomor yang sama, tidak pernah
    mengonsumsi nomor baru. Satu invoice dengan banyak tender (split payment) sah memiliki
    banyak nomor Kwitansi berbeda — satu per tender.
23. Kwitansi dapat dihasilkan untuk tender dengan status apa pun (`SUCCEEDED`, `PENDING`,
    `FAILED`); badge status pada dokumen mencerminkan status tender sesungguhnya, bukan selalu
    "LUNAS"/"DITERIMA".
24. Field Catatan (internal, `BKC-DEC-048`) tidak pernah muncul pada konten PDF Kwitansi.
25. Tab SPT/Claim Letter/LML/LMA/Resep Obat pada Dokumen Kasir tampil sebagai placeholder yang
    jujur (bukan konten kosong yang terlihat seperti bug) sampai modul pemiliknya membangun
    kontennya. Struk Pasien BUKAN bagian dari placeholder ini (lihat `BKC-DEC-058`) — tab itu
    fungsional dan dibangun billing-kasir sendiri.
26. Struk Pasien menampilkan rincian tagihan (obat/tindakan/racikan/biaya admin) yang identik
    dengan tabel Tagihan Pasien pada Menu Pembayaran — tidak ada sumber data baru, tidak ada
    field finansial yang tidak konsisten antara kedua tampilan itu.

### Acceptance criteria bisnis (lanjutan, no. 17 dst.)

17. Menu Pembayaran menolak submit (tombol nonaktif) selama ada Diskon Dokter berstatus
    pending approval pada invoice yang sama; begitu disetujui/ditolak, tombol aktif kembali.
18. Setiap baris "Biaya Lain-lain" yang tersimpan menghasilkan satu entri audit log yang
    memuat identitas kasir, waktu, kategori, nama item, dan nominal — tanpa entri audit
    berarti baris itu tidak sah dianggap tersimpan.
19. Nilai field Catatan tidak pernah muncul pada payload/template pencetakan Dokumen Kasir;
    field ini hanya boleh dikembalikan pada endpoint yang diakses kasir/petugas billing.
20. Proses Pembayaran dengan nominal kurang dari Harus Dibayar berhasil tersimpan sebagai
    Pembayaran Sebagian dan invoice tetap menampilkan sisa outstanding yang benar — bukan
    ditolak sebagai error validasi.
21. Voucher/kode promo yang dicari pada Menu Pembayaran mengembalikan `MstDiscountPolicy`
    yang sudah ada (tidak membuat entitas/redemption baru); pencarian kode yang tidak
    ditemukan menghasilkan pesan "tidak ditemukan", bukan crash atau silent-fail.
22. Tender `SUCCEEDED` dalam satu split payment tidak pernah berubah status akibat kegagalan
    tender lain pada settlement yang sama; outstanding invoice selalu dihitung dari sisa
    `patient responsibility` yang belum tertutup tender `SUCCEEDED`, bukan dari total nominal
    semula.
23. Selama ada tender berstatus `PENDING` pada suatu settlement, sistem menolak upaya membuat
    tender baru untuk porsi nominal yang sama sampai tender `PENDING` itu berubah menjadi
    `SUCCEEDED`, `FAILED`, atau `EXPIRED` — mencegah pasien tertagih dua kali untuk porsi
    yang sama.

### Open question / belum diputuskan

Tidak ada open question tersisa dari pass ini. Kedua butir yang sebelumnya terbuka
(perilaku split payment sebagian gagal, dan kontrol "Metode Lainnya") sudah dijawab eksplisit
dan tercatat sebagai `BKC-DEC-050` dan `BKC-DEC-051`.

### Amendment lanjutan 27 Agustus 2026 (lanjutan) — Koreksi hasil `/trace-existing-capabilities`

`/trace-existing-capabilities` yang dijalankan setelah pass Kwitansi menemukan dua conflict antara
implementasi yang baru dibangun dan bukti legacy `KasirQuilvian1` (dicatat di
`01-existing-capability-map.md` bagian 15.2.B dan 15.2.C). Pass ini menutup kedua conflict
tersebut. Satu temuan ketiga (integrasi `ClinicalMilestoneFactProducer` milik blueprint
`rawat-jalan` dengan `BillingChargeSourceAdapter` milik billing-kasir, bagian 15.2.A) TIDAK
ditutup di sini — lintas modul, di luar scope pass ini, tetap terbuka sebagai open question lintas
modul.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-057` | Decision | Nomor Kwitansi digenerate PER TENDER (per pembayaran/angsuran), bukan per invoice — setiap kali kasir berhasil menambahkan tender baru pada suatu settlement, satu nomor Kwitansi baru otomatis dialokasikan dan disimpan pada tender itu (bukan diminta terpisah lewat endpoint "get or allocate"). Reprint pada tender yang sama mengembalikan nomor yang sama (tender adalah unit immutable setelah dibuat, sehingga tidak ada risiko re-generate). Satu invoice dengan banyak tender (split payment) menghasilkan banyak Kwitansi berbeda, satu per tender — bukan satu Kwitansi mewakili seluruh invoice. Mekanisme alokasi (`BillingNumberSeriesService`, prefix `KWS`, reset harian) TETAP dipakai, hanya titik pemanggilannya berpindah dari `BillingInvoiceService`/endpoint terpisah ke `BillingSettlementService.AddTenderAsync`. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026: "NoKwitansi akan digenerate setiap user melakukan pembayaran" — dipilih sebagai opsi yang konsisten dengan bukti legacy `MainKasirController.cs` ("kwitansi unik per baris"); menggantikan `BKC-DEC-054`; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |
| `BKC-DEC-058` | Decision | Struk Pasien (rincian tagihan tercetak — obat/tindakan/racikan/biaya admin) MASUK scope billing-kasir untuk dibangun sekarang, bukan placeholder tab milik modul lain. Berbeda dari lima dokumen lain (SPT, Claim Letter, LML, LMA, Resep Obat) yang TETAP di luar scope dan tetap placeholder, karena Struk Pasien datanya sudah tersedia penuh di tabel Tagihan Pasien pada Menu Pembayaran (`BilInvoiceItem` yang sudah dimuat) — tidak memerlukan data dari modul lain. `Dokumen Pasien` (tab kedelapan pada referensi UI) TIDAK dibahas eksplisit pada pertanyaan ini dan tetap placeholder sampai ditanyakan terpisah. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 27 Agustus 2026, menutup conflict 15.2.C pada `01-existing-capability-map.md`; memperluas (bukan mengganti) `BKC-DEC-052`; disetujui Product/Domain Owner 28 Agustus 2026 ("saya setuju BKC-DEC-052–058") |

### Open question lintas modul — TIDAK diputuskan pada pass ini

Integrasi `ClinicalMilestoneFactProducer`/`TrxClinicalMilestoneFact` (blueprint `rawat-jalan`,
`RJ-BIL-BE-002`) dengan `BillingChargeSourceAdapter` milik billing-kasir (`PROCEDURE`,
`LABORATORY`, `RADIOLOGY`, `PHARMACY`, `CONSUMABLE`) — keduanya sudah dibangun tetapi tidak saling
memanggil, sehingga jalur "order pelayanan -> billing item" untuk kelima domain itu masih belum
terbukti end-to-end. Ini keputusan lintas modul (siapa memanggil siapa) yang perlu melibatkan
pemilik `rawat-jalan`, bukan keputusan sepihak `billing-kasir`. Lihat `01-existing-capability-map.md`
bagian 15.2.A untuk detail bukti.

### Langkah berikutnya

Keputusan `BKC-DEC-045`–`051` masih berstatus `draft` — belum ada pernyataan approval eksplisit
dari Product/Domain Owner untuk amendment ini (berbeda dengan `BKC-DEC-031`–`044` yang sudah
`approved`). Sebelum desain final: (1) dapatkan approval eksplisit untuk ketujuh keputusan ini,
(2) jalankan `/trace-existing-capabilities` untuk memetakan `KasirQuilvian1/` (BeKasir,
FE kasir app/view) terhadap kapabilitas backend/frontend saat ini secara rinci — pass ini baru
memverifikasi beberapa fakta source (`MstPaymentMethod`, `MstDiscountPolicy`, `BilTender`,
`BilSettlement`) secara ad-hoc, belum melakukan audit menyeluruh gaya `01-existing-capability-map.md`
khusus untuk Menu Pembayaran.

## Amendment lanjutan 2 September 2026 — Entri manual berbasis katalog tarif + coverage per item

**Pemicu:** Permintaan pemilik produk untuk merombak form "Buat Invoice Manual (Testing)"
(`create-manual-invoice-view.jsx`) agar item/harga terikat `MstTariff`/`MstTariffCategory`
(bukan free-text/free-price seperti sekarang), menampilkan status coverage per item untuk pasien
asuransi, dan memisah subtotal mandiri/asuransi (termasuk perlakuan pajaknya) pada Menu
Pembayaran. Pass ini masih **berjalan** — belum ditutup, belum ada approval formal. Sesi
sebelumnya dicatat sebagai fakta di conversation, bukan di file ini; ringkasannya dituliskan di
sini supaya tidak hilang.

⚠️ SHA di `blueprint-manifest.md` (`backend_commit_sha: c99f0a5…`, `frontend_commit_sha: e555bf2…`)
berbeda dari HEAD saat pass ini dimulai (`17b9c0e21e32b41a8dfd6dbde31462d52717646b` BE,
`60febdcdbb39de6cebc2d825906bce949f3b5af3` FE) — capability map berpotensi basi. Interview tetap
dijalankan; `/trace-existing-capabilities` disarankan sebelum desain final.

### Fakta source terverifikasi (bukan keputusan bisnis, tercatat sebagai evidence)

1. `BilInvoiceItem.CategoryId` sudah FK langsung ke `MstTariffCategory`
   (`BilInvoiceItem.cs:19,31`) — bukan ke entity kategori lain.
2. Form "Buat Invoice Manual (Testing)" SUDAH mengambil Kategori Biaya dari `MstTariffCategory`
   lewat `getTariffCategoryOptions` (`use-create-manual-invoice.js:78`); yang belum ada: dropdown
   item dari `MstTariff` (masih free-text `description`) dan harga otomatis (masih free-input
   `unitPrice`).
3. `MstTariff` punya `NormalPrice`, `TariffCategoryId`, `IsTaxable`, plus scoping opsional
   `ServiceUnitId`/`ClinicId`/`PatientClassId` — satu nama layanan bisa berupa beberapa baris
   tarif berbeda tergantung unit/klinik/kelas pasien.
4. Mesin coverage per-item sudah ada dan sudah dipakai kalkulasi: `MstInsuranceCoverageRule`
   (per `InsuranceProviderId` + `TariffId`/`TariffCategoryId`/dll., dengan `CoverageStatus`
   Covered/NotCovered/NeedApproval, `CoveragePercent`, `CoPaymentPercent/Amount`,
   `MaxCoverageAmount`, dll.) dikonsumsi oleh `RegistrationBillingCoverageAdapter.ResolveAsync`
   (`BillingCoverageAdapter.cs`) yang sudah mencocokkan tiap `BillingCoverageComponent` satu per
   satu (`Matches()`, `CalculateCoveredAmount()`). Namun `BillingCoverageDecision` yang
   dikembalikan HANYA agregat (`PrimaryAmount`/`ExcessAmount`/`UnresolvedAmount` total) — status
   coverage per item tidak diekspos ke API/UI manapun saat ini.
5. Pajak sudah dialokasikan per komponen lewat `BillingCalculationService.ApplyInvoiceTax`, dan
   `MstTaxRule.AllocationRule = "PATIENT"` sudah membuat seluruh pajak jadi tanggungan pasien,
   tidak coverable asuransi sama sekali (`TaxComponentCoverable`,
   `BillingCalculationService.cs:973-979`). Kemungkinan besar permintaan "pajak hanya di porsi
   mandiri" sudah tercapai lewat konfigurasi master data, bukan kode baru — yang baru murni
   tampilan split subtotal di Menu Pembayaran (saat ini hanya satu "Subtotal Tagihan" gabungan).
6. Form "Buat Invoice Manual (Testing)" secara eksplisit berlabel testing tool di 3 tempat
   berbeda pada source (komentar kode, eyebrow UI, alert "jangan dipakai untuk data produksi") —
   pengganti sementara integrasi Rajal→Billing yang belum tersambung
   (`create-manual-invoice-view.jsx:19-20,114,138-139`).
7. Form ini dan panel "Tambah Biaya Lain-lain" di Menu Pembayaran memakai jalur ADHOC yang sama
   (`BillingChargeSourceAdapter`), tapi "Tambah Biaya Lain-lain" terikat `BKC-DEC-047` (item/harga
   sengaja bebas tanpa katalog, dikompensasi wajib audit log).
8. `BKC-DEC-013` (approved) sudah menyatakan "Tarif, coverage, diskon, dan responsibility dinamis
   selama invoice belum dikunci" — jadi status coverage per item yang akan ditampilkan di Menu
   Pembayaran sudah seharusnya dihitung ulang tiap kalkulasi (live), bukan snapshot tetap saat
   item ditambahkan. Tidak perlu keputusan baru untuk poin ini, cukup diterapkan konsisten.

### Keputusan pass ini

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-059` | Decision | Perombakan katalog tarif HANYA berlaku pada form "Buat Invoice Manual (Testing)". Form tetap berlabel testing/development persis seperti sekarang (TIDAK naik kelas jadi fitur produksi permanen). Panel "Tambah Biaya Lain-lain" di Menu Pembayaran TIDAK disentuh — `BKC-DEC-047` tetap berlaku apa adanya. Konsekuensi teknis: perlu jalur/endpoint ADHOC baru khusus form ini, tidak lagi 100% berbagi command dengan `addBillingOtherCharge`, supaya perubahan tidak diam-diam mengubah perilaku "Tambah Biaya Lain-lain" yang sudah dipakai kasir produksi. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 2 September 2026: "pilihan A" atas pertanyaan cakupan/status perombakan form; disetujui Product/Domain Owner 2 September 2026 13:53 WIB ("approval eksplisit sekarang untuk BKC-DEC-059–062") |
| `BKC-DEC-060` | Decision | Badge coverage per item pada dropdown dihitung LIVE dengan menggunakan ulang logika pencocokan rule yang sudah ada (`Matches()`/`CalculateCoveredAmount()` di `BillingCoverageAdapter.cs`), direpresentasikan sebagai TIGA status, bukan biner: "Tercover" (100%, tanpa syarat approval), "Tercover Sebagian/Bersyarat" (persentase < 100, ada co-payment, dan/atau butuh approval/surat jaminan), "Tidak Tercover" (rule eksplisit NotCovered, atau tidak ada rule yang cocok). Perlu endpoint preview coverage baru (dipanggil saat dropdown item dibuka, berdasar guarantor pasien terpilih pada encounter). | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 2 September 2026: "pilihan A" atas pertanyaan mekanisme/representasi badge coverage; disetujui Product/Domain Owner 2 September 2026 13:53 WIB ("approval eksplisit sekarang untuk BKC-DEC-059–062") |
| `BKC-DEC-061` | Decision | Dropdown item difilter otomatis berdasar konteks encounter terpilih: hanya tampilkan baris `MstTariff` yang scoping `ServiceUnitId`/`ClinicId`/`PatientClassId`-nya NULL (berlaku umum) atau persis cocok dengan encounter. Bila masih tersisa >1 baris untuk nama yang sama setelah difilter, tampilkan semua sebagai opsi terpisah berlabel scope (mis. "Konsultasi Dokter Umum — RSUD Melati") — tidak memilih diam-diam. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 2 September 2026: "pilihan A" atas pertanyaan disambiguasi baris `MstTariff` bernama sama; disetujui Product/Domain Owner 2 September 2026 13:53 WIB ("approval eksplisit sekarang untuk BKC-DEC-059–062") |
| `BKC-DEC-062` | Decision | **Formula subtotal:** Subtotal Mandiri = item berstatus "Tidak Tercover" (rule eksplisit NotCovered atau tidak ada rule cocok) + co-payment dari item "Tercover Sebagian". Subtotal Asuransi = item "Tercover" penuh + porsi `CoveragePercent` dari item "Tercover Sebagian". **Perubahan mesin coverage (GLOBAL, bukan cuma form testing):** `RegistrationBillingCoverageAdapter.ResolveAsync` diubah — rule dengan `CoverageStatus=Covered` yang cocok SELALU dihitung tercover sesuai `CoveragePercent`-nya, TIDAK LAGI digeser ke "unresolved" hanya karena `IsNeedApproval`/`IsNeedGuaranteeLetter` bernilai true. `CoverageStatus=NeedApproval` dan `MaxAmountPerMonth`/`MaxQuantityPerMonth` TETAP menjadi gate (lihat catatan interpretasi — scope final dipersempit saat desain `02-backend-architecture.md`, tidak mencakup limit bulanan). Berlaku untuk SEMUA invoice (bukan cuma item dari form testing), karena `ResolveAsync` adalah satu mesin yang sama dipakai kalkulasi seluruh invoice. Rasional pemilik produk: begitu suatu tarif sudah dipetakan `Covered` di `MstInsuranceCoverageRule`, itu dianggap keputusan final data master ("beneran dicover"), bukan kondisi yang masih menunggu approval manual — dan pola ini akan jadi cetak biru untuk halaman input tindakan/obat resmi (belum dibangun) ketika integrasi Rajal→Billing selesai nanti, bukan cuma dipakai form testing. | Product/Domain Owner | `approved` | Jawaban eksplisit sesi wawancara 2 September 2026: "Pilihan b — karena nnt jika page input tindakan/obat2an dah jadi, maka pas milih akan ketahuan item yg dicover asuransi pasien dan tidak... saya ingin pada page testing input manual menjadi gambaran ketika digunakan secara global nanti". **Interpretasi dipersempit saat desain** (`02-backend-architecture.md` amendment 2 September 2026): cakupan "abaikan gating" HANYA mencakup `IsNeedApproval`/`IsNeedGuaranteeLetter` — `MaxAmountPerMonth`/`MaxQuantityPerMonth` TETAP gating karena belum pernah dikonfirmasi eksplisit terpisah dari flag approval; dicatat sebagai kemampuan yang ditunda di `04-prd-to-mvp.md` § 8, bukan bagian keputusan yang disetujui di sini. **CAVEAT WEWENANG:** keputusan ini mengamendemen sebagian `BKC-DEC-042` yang owner tercatatnya adalah Payer/Insurance + Finance/AR (bukan Product/Domain Owner generik). Disetujui Product/Domain Owner 2 September 2026 13:53 WIB ("approval eksplisit sekarang untuk BKC-DEC-059–062") TANPA konfirmasi terpisah dari Payer/Insurance + Finance/AR — dicatat apa adanya sebagai bukti provenance approval, bukan disembunyikan; bila di kemudian hari pemilik asli keberatan, `BKC-DEC-062` perlu direvisi ulang, bukan dianggap final selamanya. **Risiko operasional yang perlu diketahui:** bila klaim yang sudah dianggap tercover ternyata ditolak asuransi di dunia nyata, koreksinya lewat mekanisme Pengecualian Finansial yang sudah ada (refund/adjustment/write-off, `DEC-032`–`035`), bukan otomatis — ini pola "koreksi belakangan", bukan lagi "tunggu sampai jelas" seperti semula.

### Status pass ini

Keempat keputusan kritis (mekanisme tercover/tidak tercover, disambiguasi `MstTariff`,
formula subtotal, cakupan pelepasan gating approval) sudah dijawab eksplisit DAN disetujui
Product/Domain Owner 2 September 2026 13:53 WIB — lihat `BKC-DEC-059`–`062` di atas, status
`approved`. `BKC-DEC-062` disetujui dengan CAVEAT wewenang tercatat pada barisnya sendiri:
owner asli `BKC-DEC-042` yang diamendemen sebagian adalah **Payer/Insurance + Finance/AR**,
dan approval yang diberikan adalah dari Product/Domain Owner TANPA konfirmasi terpisah dari
pemilik tsb — bukan berarti belum disetujui, tapi provenance-nya dicatat apa adanya supaya bisa
ditinjau ulang bila pemilik asli keberatan di kemudian hari.

Sisa yang belum tertutup (tidak memblokir status `approved` di atas, tapi relevan sebelum
implementasi selesai — lihat `04-prd-to-mvp.md` § 20 untuk daftar lengkap dengan status
memblokir/tidak):

- `/trace-existing-capabilities` (impact scan § 16, `01-existing-capability-map.md`) dan
  `/design-business-module` (`02-backend-architecture.md`, `03-frontend-architecture.md`,
  `erd/`, `contracts/`, `04-prd-to-mvp.md`, semua amendment 2 September 2026) sudah dijalankan
  dan sudah menutup pertanyaan cakupan kontrak endpoint, field encounter yang dipakai
  `BKC-DEC-061` (`ServiceUnitId`/`ClinicId`/`PatientClassId` pada `TrxPatientEncounter`,
  di-extend ke `ActiveEncounterOptionResponse`), dan interpretasi cakupan gating `BKC-DEC-062`
  (dipersempit HANYA ke `IsNeedApproval`/`IsNeedGuaranteeLetter` — lihat baris `BKC-DEC-062`
  di atas dan `04-prd-to-mvp.md` § 8 untuk `MaxAmountPerMonth`/`MaxQuantityPerMonth` yang tetap
  gating).
- Nilai `AllocationRule` pada `MstTaxRule` yang aktif saat ini masih **belum diverifikasi**
  (`CAP-07`) — eksplisit ditunda atas permintaan Product/Domain Owner 2 September 2026 ("bisa
  menunggu keputusan bisnis lebih lanjut"), TIDAK memblokir implementasi, dicatat sebagai
  pertanyaan tidak memblokir di `04-prd-to-mvp.md` § 20.
- Wewenang tulis backend (task mode, branch) untuk mulai implementasi endpoint/service baru —
  belum ditanyakan; ini prasyarat prosedural sebelum `build-module-backend` dijalankan, bukan
  bagian dari interview kebutuhan bisnis.

### Amendment lanjutan 3 September 2026 — Dokumen Kasir: modal menjadi halaman terpisah

Pengguna meminta tombol "Dokumen Kasir" pada Menu Pembayaran tidak lagi membuka modal
(`dokumen-kasir-modal.jsx`), melainkan menavigasi ke halaman tersendiri dengan isi identik
(tab Kwitansi, Struk Pasien, enam tab placeholder — lihat `BKC-DEC-052` dst.), ditambah tombol
Cetak dan tombol Kembali ke Menu Pembayaran. Ini murni perubahan wadah presentasi (modal →
page); tidak ada perubahan data, endpoint, atau aturan bisnis Kwitansi/Struk Pasien yang sudah
terkunci di `BKC-DEC-052`–`058`.

**Batas scope**: hanya mengubah wadah tampilan (`dokumen-kasir-modal.jsx` → route halaman baru)
dan titik pemicu (`openDokumenKasir`/`openKwitansiForTender` di `menu-pembayaran-view.jsx`,
`use-dokumen-kasir.js`). **Di luar scope**: isi/aturan bisnis Kwitansi dan Struk Pasien itu
sendiri (tetap seperti `BKC-DEC-052`–`058`), keenam tab placeholder (tetap placeholder milik
modul lain, `BKC-DEC-052`), dan module lain di luar `billing-kasir`.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-063` | Decision | Mekanisme cetak dan share pada halaman Dokumen Kasir baru TETAP memakai `html2pdf.js` (bukan `window.print()`), dan tombol WhatsApp/Email pada tab Kwitansi TETAP dipertahankan persis seperti modal sekarang. Alasan: `BKC-DEC-056` (WA/Email butuh file PDF ter-unduh sebagai lampiran manual) masih berlaku sepenuhnya pada wadah halaman; mengganti ke `window.print()` akan menghilangkan kemampuan share yang sudah dipakai kasir tanpa ada permintaan eksplisit untuk itu. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini, opsi "Pertahankan mekanisme existing (html2pdf.js)" dari 3 opsi bertanda rekomendasi |
| `BKC-DEC-064` | Decision | Halaman Dokumen Kasir baru adalah SATU route yang dipakai oleh kedua titik pemicu (tombol umum "Dokumen Kasir" di Ringkasan Pembayaran, dan tombol "Cetak Kwitansi" per baris tender di panel Split Tender). Tab aktif dan tender terpilih dikirim lewat query string (mis. `?tab=KWITANSI&tenderId=...` atau `?tab=STRUK_PASIEN`), bukan dua route terpisah. Kasir tetap bisa berpindah tab secara manual di halaman itu seperti pada modal sekarang. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini, opsi "Satu halaman, state lewat query string" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: kedua keputusan kritis (mekanisme print/share, struktur routing) sudah
dijawab eksplisit dan disetujui langsung oleh pengguna dalam percakapan sesi ini — tidak ada
pengambil keputusan terpisah yang perlu dikonfirmasi lagi karena scope-nya presentational murni
di dalam modul yang sama, bukan amendemen aturan bisnis modul lain. Item DEV_DISCRETION yang
tidak perlu ditanyakan lebih lanjut: path URL literal halaman baru (mengikuti konvensi
`[slug]/pembayaran` yang sudah ada), dan penghapusan `dokumen-kasir-modal.jsx` sebagai dead code
setelah kedua titik pemicu dipindah (konsisten dengan aturan repo: tidak menyisakan kode yang
sudah pasti tidak terpakai).

### Amendment lanjutan 3 September 2026 — Dokumen Kasir: dokumen baru "Invoice Asuransi"

Pengguna meminta satu jenis dokumen baru pada Dokumen Kasir: "Invoice Asuransi", berisi identitas
pasien, informasi perusahaan asuransi, dan rincian item yang dicover asuransi — bisa dicetak dan
diunduh.

**Temuan batas scope sebelum bertanya**: `Dokumen Kasir` sudah dikunci berisi enam tab
(`SPT`, `Claim Letter`, `LML`, `LMA`, `Resep Obat`, `Kwitansi` — lihat amendment 27 Agustus 2026 di
atas). `Claim Letter` adalah tab yang paling dekat maknanya dengan "invoice asuransi", TAPI sudah
eksplisit dicatat milik modul lain (klinis/farmasi/asuransi), dan modul `InsuranceManagement/Ins`
pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` masih berstatus `PLANNED` (belum ada wewenang
implementasi). Temuan ini dikonfirmasikan ke pengguna sebelum pertanyaan lain diajukan.

**Batas scope**: dokumen baru ini dibangun sebagai kepemilikan `billing-kasir` sendiri — tab
ketiga sejajar Kwitansi/Struk Pasien di halaman Dokumen Kasir, BUKAN mengisi slot tab
`Claim Letter` yang dicadangkan. **Di luar scope — untuk modul lain**: konten resmi `Claim Letter`
untuk pengajuan klaim formal ke asuransi (tetap placeholder milik `InsuranceManagement`, butuh
aktivasi modul + `/grill-me` tersendiri bila kelak diperlukan).

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-065` | Decision | "Invoice Asuransi" dibangun sebagai dokumen milik `billing-kasir` sendiri (tab baru di halaman Dokumen Kasir, pola presentasi sama dengan Kwitansi — render HTML lalu `html2pdf.js` untuk cetak/unduh), bukan mengisi tab `Claim Letter` yang sudah dicadangkan untuk modul `InsuranceManagement` (`PLANNED`, belum ada wewenang implementasi). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini, opsi "Dokumen baru milik billing-kasir sendiri" dari 2 opsi + Other bertanda rekomendasi |
| `BKC-DEC-066` | Decision | Dokumen ini ditujukan untuk tiga pihak sekaligus — pasien, internal rumah sakit, dan pihak asuransi — bukan sekadar rekap informal internal. Konsekuensinya: kontennya harus cukup meyakinkan/lengkap untuk dipakai pihak asuransi (lihat `BKC-DEC-069` soal rincian rupiah per item), bukan hanya badge status. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini: "Sebenarnya untuk semua, pasien, rs, dan pihak asuransi" |
| `BKC-DEC-067` | Decision | Sumber data "informasi perusahaan" pada dokumen ini adalah `MstInsuranceProvider` (perusahaan asuransi, mis. Allianz Indonesia) — BUKAN `MstCompanyGuarantor` (penjamin perusahaan tempat kerja pasien). Dua entity ini berbeda; dukungan untuk Company Guarantor tidak termasuk dalam slice ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini, opsi "Perusahaan asuransi / Insurance Provider" dari 2 opsi + Other bertanda rekomendasi |
| `BKC-DEC-068` | Decision | Dokumen ini HANYA menampilkan item yang benar-benar tercover asuransi (status "Penjamin") — item yang dibayar tunai/mandiri TIDAK ditampilkan sama sekali. Ini mengoreksi jawaban awal pengguna ("semua item + status per baris seperti Menu Pembayaran") yang diralat eksplisit menjadi "semua item yg dicover oleh asuransi saja" pada giliran berikutnya dalam sesi yang sama. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, dengan ralat) | `approved` | Jawaban awal lalu ralat eksplisit: "Maksud saya semua item yg dicover oleh asuransi saja" |
| `BKC-DEC-069` | Decision | Setiap baris item pada dokumen ini WAJIB menampilkan kolom rupiah yang dicover asuransi per item (bukan hanya badge status). Backend SAAT INI belum mengekspos pecahan rupiah per item — `RegistrationBillingCoverageAdapter.ResolveAsync` (`BillingCoverageAdapter.cs`) sudah menghitung `covered` per komponen secara internal di dalam loop (`CalculateCoveredAmount`), tapi hanya total (`primary`) yang dikembalikan ke `BillingCoverageDecision`; pecahan per komponennya dibuang. Kontrak API/DTO baru untuk mengekspos pecahan ini BELUM dirancang — ini bukan pekerjaan frontend murni, perlu slice backend terlebih dahulu dengan kontrak yang dikunci sebelum frontend membangun tampilannya. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit sesi ini, opsi "Wajib ada kolom rupiah dicover per baris item" dari 2 opsi + Other |

**Status pass ini**: lima keputusan kritis di atas sudah dijawab eksplisit dan disetujui langsung
oleh pengguna. **Blocker desain yang masih terbuka** (memblokir `IMPLEMENTATION`, tidak memblokir
`DESIGN` lanjutan):

- Bentuk kontrak API/DTO persis untuk pecahan rupiah per item (`BKC-DEC-069`) — field baru pada
  `CalculationItemResponse`/breakdown mana, apakah dipersist di `BreakdownSnapshot` atau dihitung
  ulang saat diminta, dan bagaimana penanganannya untuk komponen non-item (biaya administrasi,
  room charge) yang juga bisa dicover tapi bukan `BilInvoiceItem`. Ini keputusan arsitektur
  backend, bukan requirement bisnis — cocok dilanjutkan lewat `/design-business-module` atau
  langsung `/plan-module-delivery` bila pemilik backend sudah cukup yakin dengan pendekatan
  "expose per-component breakdown yang sudah dihitung, jangan hitung ulang logika baru".
- Nomor dokumen: belum diputuskan apakah "Invoice Asuransi" perlu nomor tersendiri yang
  dialokasikan backend (pola sama dengan Kwitansi, `BKC-DEC-054`) atau cukup memakai
  `InvoiceNumber` yang sudah ada. Ditandai `DEV_DISCRETION` sementara dengan rekomendasi memakai
  `InvoiceNumber` (risiko rendah, konsisten dengan sifat dokumen ini yang bukan `Claim Letter`
  formal) — bisa diubah bila pemilik produk keberatan.
- Layout/field detail persis (letterhead, blok tanda tangan, format tabel) belum digali — mengikuti
  pola visual Kwitansi yang sudah ada (letterhead RS, blok identitas, tabel rincian, total) sebagai
  `DEV_DISCRETION` kecuali pengguna menentukan lain.

**Di luar scope — untuk modul lain**: konten resmi tab `Claim Letter` (milik `InsuranceManagement`,
`PLANNED`); dukungan Company Guarantor pada dokumen ini (bisa jadi amendment terpisah bila
dibutuhkan kelak).
