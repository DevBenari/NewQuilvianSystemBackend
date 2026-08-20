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
| `BKC-DEC-042` | Decision | Primary dihitung lebih dulu; excess hanya menilai residual dengan kontraknya sendiri; total coverage tidak melebihi eligible charge; AR final per debtor; rejected claim tidak otomatis pindah ke pasien kecuali contract/policy sah mengizinkan | Payer/Insurance + Finance/AR | `approved` | Pernyataan eksplisit approval amendment revision `0.2`, 20 Agustus 2026 |
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
