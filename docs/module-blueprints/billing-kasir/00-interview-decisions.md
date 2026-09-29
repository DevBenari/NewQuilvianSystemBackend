# Billing dan Kasir — Interview Decisions

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Revision | Approved decision contract `0.3` (Pass B — Integrasi Rawat Inap ↔ Billing) |
| Status | Keputusan `BKC-DEC-001`–`119` berstatus `approved` |
| Interview mode | `Pass B: Integrasi Rawat Inap ↔ Billing (Yasmina / Billing Management)` disahkan 24 September 2026 |
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
- Shift Kasir — sudah diputuskan `BKC-DEC-038`, sudah dibangun. **Dibuka ulang sebagian** 25
  September 2026 lewat `/grill-me` amendment (lihat "Amendment 25 September 2026 — Shift
  Kasir" di akhir dokumen ini) — `BKC-DEC-038` TETAP berlaku sebagai baseline, hanya dua celah
  penegakan yang ditutup (`BKC-DEC-123`, `BKC-DEC-124`), bukan desain ulang menyeluruh.
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

### Amendment lanjutan 4 September 2026 — Penghapusan bucket "Penjamin Belum Terverifikasi"

Pengguna meminta label "Penjamin Belum Terverifikasi" (`unresolvedCoverageAmount`) pada Menu
Pembayaran dihapus, dengan alasan awal: penjamin/asuransi pasien sudah diverifikasi dan
mengikat sejak pendaftaran, dan dokter/perawat sudah tahu status coverage saat memilih
tindakan, sehingga bucket "menunggu verifikasi" dianggap tidak relevan lagi — "jika item adalah
item yg dicover asuransi maka, langsung masuk ke sub total asuransi."

**Temuan sebelum bertanya**: `RegistrationBillingCoverageAdapter.ResolveAsync`
(`BillingCoverageAdapter.cs`) menggeser amount ke `unresolved` lewat LIMA jalur berbeda, bukan
satu: (1) tidak ada rule yang match, (2) rule eksplisit `CoverageStatus=NotCovered`, (3) rule
`CoverageStatus=NeedApproval` atau ada `MaxAmountPerMonth`/`MaxQuantityPerMonth`, (4)
`payment source` tidak `IsEligible`/`IsPolicyActive`/`InsuranceProviderId` kosong, (5) residual
dari `CalculateCoveredAmount` (`CoveragePercent<100`, `CoPayment`, `MaxCoverageAmount`) saat
`!IsAllowExcessPaymentByPatient`. Temuan ini dikonfirmasikan lewat serangkaian pertanyaan
klarifikasi sebelum keputusan final diambil, karena kelima jalur itu punya semantik bisnis
yang berbeda dan sebagian bertentangan dengan keputusan yang **sudah approved**
(`BKC-DEC-062`, formula Subtotal Mandiri/Asuransi).

**Konflik yang ditemukan dan diluruskan selama sesi**: jawaban awal pengguna sempat meminta
jalur (1) "tidak ada rule match" dan jalur (2) "NotCovered" ikut dipaksa masuk Subtotal
Asuransi — ini membalik separuh formula `BKC-DEC-062` yang sudah disetujui Product/Domain
Owner 2 September 2026 ("Tidak Tercover" = NotCovered ATAU tidak ada rule cocok → Subtotal
Mandiri). Setelah konflik ini ditunjukkan eksplisit, pengguna mengoreksi jawabannya: kedua
jalur itu TETAP mengikuti `BKC-DEC-062` apa adanya (Subtotal Mandiri), tidak berubah. Pesan
lanjutan pengguna pada giliran berikutnya ("perhitungan sub total asuransi mengikuti aturan
coverage dan copayment di atas", disertai tangkapan layar form master data Insurance Coverage
Rule bagian "Coverage dan Co-Payment": Persentase Coverage, Maksimal Coverage, Persentase
Co-Payment, Nominal Co-Payment) menegaskan ulang bahwa jalur (5) — residual dari field-field
itu — juga TETAP ke pasien, tidak berubah dari `CalculateCoveredAmount` yang sudah ada.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-070` | Decision | Subtotal Asuransi tetap dihitung PERSIS mengikuti field Coverage dan Co-Payment pada `MstInsuranceCoverageRule` yang match (`CoveragePercent`, `MaxCoverageAmount`, `CoPaymentPercent`, `CoPaymentAmount`, lewat `CalculateCoveredAmount` yang sudah ada) — TIDAK berubah dari perilaku saat ini. Residual dari pembagian ini (co-payment, cap `MaxCoverageAmount`) tetap menjadi Subtotal Mandiri/tanggungan pasien, konsisten `BKC-DEC-062`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "perhitungan sub total asuransi mengikuti aturan coverage dan copayment di atas" + tangkapan layar form Coverage dan Co-Payment |
| `BKC-DEC-071` | Decision | Gating `CoverageStatus=NeedApproval`/`IsNeedGuaranteeLetter` DAN `MaxAmountPerMonth`/`MaxQuantityPerMonth` dihapus TOTAL dari `ResolveAsync`. Rule `Covered` yang match SELALU langsung dihitung sesuai `BKC-DEC-070` dan masuk Subtotal Asuransi, tidak lagi ditunda ke `unresolved` menunggu approval atau pemeriksaan limit bulanan. Ini MENGGANTIKAN bagian `BKC-DEC-062` yang sebelumnya secara eksplisit mempersempit pelepasan gating HANYA ke flag approval dan tetap mempertahankan `MaxAmountPerMonth`/`MaxQuantityPerMonth` sebagai gate — pemeriksaan limit bulanan itu sekarang dihapus juga. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Jawaban eksplisit: "tidak lagi digeser ke bucket terpisah — semua ikut masuk subtotal asuransi juga", dikonfirmasi ulang setelah pertanyaan klarifikasi |
| `BKC-DEC-072` | Decision | Jalur (1) "tidak ada rule match" dan jalur (2) rule eksplisit `NotCovered` TETAP TIDAK masuk Subtotal Asuransi — tetap Subtotal Mandiri, TIDAK BERUBAH dari `BKC-DEC-062`. Alasan: tidak ada rule berarti item belum dipetakan di master data (bukan bukti dicover), dan `NotCovered` adalah keputusan eksplisit bahwa asuransi tidak menanggung; memaksa keduanya ke Subtotal Asuransi berisiko klaim ditolak insurer dan piutang tak tertagih. Jawaban awal pengguna yang sempat meminta sebaliknya untuk kedua jalur ini DIRALAT eksplisit setelah konflik dengan `BKC-DEC-062` ditunjukkan. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, dengan ralat) | `approved` | Jawaban awal lalu diralat setelah konflik dgn `BKC-DEC-062` ditunjukkan eksplisit; jawaban final: "NotCovered TETAP tidak masuk subtotal asuransi" dan "Tetap Subtotal Mandiri, konsisten BKC-DEC-062" |
| `BKC-DEC-073` | Decision | Jalur (4) — precondition `payment source` tidak `IsEligible`/tidak `IsPolicyActive`/`InsuranceProviderId` kosong — dan kasus encounter data tidak ditemukan (`encounter is null`) TETAP DICEK di code, TIDAK dihapus. Keduanya direklasifikasi sebagai kategori "anomali data" terpisah dari Subtotal Mandiri/Asuransi normal (bentuk teknis persis — field/flag/log — adalah keputusan desain, bukan keputusan bisnis pass ini). Alasan: kondisi ini bukan "item tidak dicover", melainkan tanda ada yang salah di data pendaftaran/registrasi; menghapus pengecekannya berarti data provider tidak eligible akan diam-diam diklaim ke asuransi sebagai valid. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tetap dicek, tapi jadi kategori 'anomali data' terpisah" |
| `BKC-DEC-074` | Decision | Field `IsAllowExcessPaymentByPatient` pada `MstInsuranceCoverageRule` TETAP dipakai runtime (bukan dead field) — tetap relevan sebagai penentu residual `NotCovered`/co-payment (`BKC-DEC-070`, `BKC-DEC-072`) menjadi tanggungan pasien atau tidak, karena kedua jalur itu tetap berlaku seperti sebelumnya. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tetap dipertahankan dan tetap dipakai runtime" |
| `BKC-DEC-075` | Decision | Tampilan "Penjamin Belum Terverifikasi" pada Menu Pembayaran (FE, `menu-pembayaran-view.jsx`) dan field `UnresolvedAmount`/`ExcessAmount` pada `BillingCoverageDecision` DIHAPUS dari tampilan/kalkulasi normal — nilai `unresolved` yang tersisa setelah `BKC-DEC-070`–`072` diterapkan seharusnya mendekati nol untuk alur normal (hanya `BKC-DEC-073`, anomali data, yang masih bisa mengisinya). Field/response itu TIDAK dihapus total dari kontrak backend; disisakan sebagai sinyal anomali data (`BKC-DEC-073`), bukan ditampilkan sebagai baris subtotal pembayaran normal ke user. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Dihapus dari tampilan normal, tapi tetap ada fallback untuk error data" |

**Status pass ini**: keenam keputusan di atas (`BKC-DEC-070`–`075`) sudah dijawab eksplisit,
termasuk satu konflik dengan keputusan approved (`BKC-DEC-062`, jalur no-rule-match) yang
ditunjukkan ke pengguna dan diluruskan lewat ralat eksplisit sebelum dikunci — bukan diam-diam
diselaraskan oleh agent. **Dampak bersih terhadap kode saat ini** ternyata jauh lebih sempit
dari permintaan awal ("hilangkan itu" untuk seluruh bucket): yang benar-benar berubah hanya
jalur (3) — gating `NeedApproval`/limit bulanan dihapus total — dan reklasifikasi jalur (4)
jadi kategori anomali data terpisah. Jalur (1), (2), (5) tidak berubah dari perilaku approved
saat ini.

**Blocker desain yang masih terbuka** (memblokir `IMPLEMENTATION`, tidak memblokir `DESIGN`
lanjutan):

- Bentuk teknis persis "kategori anomali data" (`BKC-DEC-073`) — apakah tetap field
  `UnresolvedAmount`/`ExcessAmount` yang sama dengan makna dipersempit, field baru terpisah
  (mis. `DataAnomalyAmount`), atau exception/log tanpa mengubah bentuk angka pada response —
  belum dirancang. Ini keputusan arsitektur backend, cocok dilanjutkan lewat
  `/design-business-module`.
- Apakah label "Penjamin Belum Terverifikasi" di FE dihapus total dari markup, atau diganti
  label lain untuk kasus anomali data (`BKC-DEC-073`) bila baris itu ternyata > 0 — belum
  digali; `DEV_DISCRETION` sementara dengan rekomendasi dihapus total dari tampilan normal dan
  anomali data dilaporkan lewat jalur lain (log/alert), bukan baris subtotal pembayaran yang
  dilihat kasir/pasien.
- `BKC-DEC-071` menggantikan sebagian `BKC-DEC-062` (pelepasan gating limit bulanan). Karena
  `BKC-DEC-062` disetujui dengan CAVEAT wewenang (owner asli `BKC-DEC-042` adalah
  Payer/Insurance + Finance/AR, approval yang diberikan dari Product/Domain Owner tanpa
  konfirmasi terpisah — lihat baris `BKC-DEC-062`), caveat yang sama berlaku untuk
  `BKC-DEC-071`: dicatat apa adanya sebagai provenance, bukan disembunyikan; bila pemilik asli
  keberatan di kemudian hari, perlu ditinjau ulang.
- `blueprint-manifest.md` (revision `0.6`) belum diperbarui untuk mencantumkan
  `BKC-DEC-070`–`075`; pembaruannya milik `/manage-module-blueprint` atau
  `/design-business-module`, bukan pass ini.

**Di luar scope — untuk modul lain**: mekanisme verifikasi penjamin saat pendaftaran pasien
(modul Registrasi/Pendaftaran) — pass ini hanya mengubah cara `billing-kasir` MENAMPILKAN dan
MENGKALKULASI hasil dari status penjamin yang sudah ada, bukan proses verifikasi itu sendiri.

### Amendment lanjutan 4 September 2026 — Penutupan `CAP-07` (alokasi PPN obat/alkes)

`CAP-07` ("Nilai `AllocationRule` pada `MstTaxRule` yang aktif saat ini masih belum
diverifikasi") sebelumnya ditunda eksplisit atas permintaan Product/Domain Owner 2 September
2026. Pengguna membuka kembali topik ini di sesi yang sama dengan `BKC-DEC-070`–`075` (basis
Subtotal Asuransi), sehingga ditutup sekarang selagi konteksnya sama.

Pengguna menyatakan: "Pajak PPN itu akan dikenakan untuk obat/alkes tidak peduli di ranap,
rajal, igd dll. dan tidak memperdulikan pasien tunai dan pasien asuransi. selain obat dan alkes
maka ga akan dikenai pajak PPN, sesuai UU di indonesia." Pernyataan ini menjawab KAPAN PPN
berlaku (basis pajak = kategori item, bukan unit atau tipe pasien) — sudah konsisten dengan
implementasi berjalan (`ApplyInvoiceTax`, basis dibatasi ke `item.IsPharmacy=true`, lihat
komentar kode `BillingCalculationService.cs:740-747` yang mengutip Pasal 4A UU PPN). Ini BUKAN
jawaban untuk pertanyaan `CAP-07` yang sebenarnya (kemana rupiah PPN itu ikut dialokasikan
setelah dihitung) — dua hal berbeda, diklarifikasi terpisah ke pengguna sebelum dikunci.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-076` | Decision | Basis PPN TETAP hanya item `IsPharmacy=true` (obat/alat kesehatan), berlaku sama di seluruh unit (ranap/rajal/IGD/dll.) dan tidak bergantung tipe pasien (tunai/asuransi) — mengonfirmasi implementasi berjalan, tidak ada perubahan kode untuk poin ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Pajak PPN itu akan dikenakan untuk obat/alkes tidak peduli di ranap, rajal, igd dll. dan tidak memperdulikan pasien tunai dan pasien asuransi... sesuai UU di indonesia" |
| `BKC-DEC-077` | Decision | Menutup `CAP-07`, DIREVISI setelah koreksi teknis (lihat evidence): (1) pasien tunai — PPN obat/alkes dibebankan ke pasien; (2) pasien asuransi, obat/alkes DICOVER — PPN-nya ikut ditanggung asuransi; (3) pasien asuransi, obat/alkes TIDAK dicover (rule `NotCovered`/tidak ada rule match, `BKC-DEC-072`) — PPN-nya tetap dibebankan ke pasien, BUKAN ke asuransi. Ini setara `MstTaxRule.AllocationRule = PROPORTIONAL` pada rule PPN yang aktif, BUKAN `GUARANTOR` — jawaban pertama pengguna sempat disederhanakan agent jadi "ikut ke asuransi" secara blanket dan dipetakan ke `GUARANTOR`, tapi `GUARANTOR` berarti PPN SELALU ke asuransi walau item tidak dicover/pasien tunai, yang salah untuk kasus (1) dan (3). Kesalahan pemetaan ini ditemukan dan dikoreksi sebelum dikunci. Mekanisme `TaxComponentCoverable()` dengan default case `PROPORTIONAL` (`BillingCalculationService.cs:1029-1037`) sudah mendukung persis perilaku ini di kode — komponen `TAX` memakai granularitas identik dengan item obat/alkes-nya (`tariffId`/`drugId` sama, `BillingCalculationService.cs:883`) sehingga otomatis kena rule coverage dan `CoveragePercent` yang sama. Ini murni keputusan NILAI MASTER DATA (rule PPN aktif harus di-set/diverifikasi `AllocationRule=PROPORTIONAL`), bukan perubahan kode. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, dengan revisi) | `approved` | Revisi eksplisit: "1. Apabila pasien tunai... akan dikenakan ke pasien. 2. Apabila pasien pakai asuransi, pajak ppn dari obat/alkes yg dicover asuransi maka akan ditanggung asuransi. Namun, jika pasien asuransi mendapatkan obat/alkes yg tidak dicover, maka pajak ppnnya ditanggung oleh pasien itu sendiri" |

**Status**: `CAP-07` DITUTUP oleh `BKC-DEC-076`–`077` (nilai final `PROPORTIONAL`, BUKAN
`GUARANTOR` — koreksi dicatat apa adanya sebagai bagian dari provenance keputusan, bukan
disembunyikan). **Blocker verifikasi yang masih terbuka** (bukan keputusan bisnis, cocok untuk
`/trace-existing-capabilities` atau pengecekan data langsung sebelum implementasi selesai):
memastikan baris `MstTaxRule` yang aktif sekarang di database benar-benar bernilai
`AllocationRule=PROPORTIONAL` — bila ternyata `Patient` atau `Guarantor`, perlu dikoreksi lewat
mekanisme master data (bukan hardcode di kode), karena `LoadInvoiceTaxRuleAsync` mengambil rule
aktif apa adanya dari tabel.

### Amendment lanjutan 4 September 2026 — Revisi `BKC-DEC-076`: faktor rawat jalan vs rawat inap

Pengguna merevisi `BKC-DEC-076` pada giliran berikutnya di sesi yang sama, disertai tabel:
"Jadi faktor penentunya cuma satu: rawat jalan vs rawat inap." Ini BUKAN penyempurnaan kecil —
bertentangan langsung dengan klaim eksplisit `BKC-DEC-076` bahwa PPN "berlaku sama di seluruh
unit (ranap/rajal/IGD/dll.)". Sebelum dikunci, ditanyakan satu titik yang belum tercakup tabel
pengguna: posisi IGD (sebelumnya eksplisit disebut `BKC-DEC-076`, sedangkan tabel revisi cuma
memuat dua baris rajal/ranap).

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-078` | Decision | **MEREVISI dan MENGGANTIKAN bagian `BKC-DEC-076`** yang menyatakan unit tidak berpengaruh. Faktor penentu kena/bebas PPN obat-alkes BUKAN semata `item.IsPharmacy=true`, melainkan DITAMBAH care setting encounter: **Rawat Jalan (termasuk IGD)** → kena PPN, berlaku baik item dicover asuransi maupun dibayar mandiri/excess. **Rawat Inap** → PPN DIBEBASKAN sepenuhnya, berlaku baik item dicover asuransi maupun dibayar mandiri — tipe pembayaran sama sekali tidak relevan untuk rawat inap karena tidak ada PPN yang dikenakan. `BKC-DEC-077` (alokasi `PROPORTIONAL` berdasar payer) TETAP berlaku tapi jadi HANYA relevan untuk transaksi rawat jalan/IGD yang memang kena PPN — untuk rawat inap tidak ada nominal PPN sama sekali untuk dialokasikan. **Dampak teknis (bukan lagi murni master data seperti catatan `BKC-DEC-077` sebelumnya)**: basis pajak di `ApplyInvoiceTax`/`BillingCalculationService.cs` saat ini HANYA mengecek `item.IsPharmacy`, tidak pernah melihat encounter/`PatientClass`/care setting sama sekali — kondisi rawat jalan vs rawat inap ini adalah GERBANG BARU yang belum ada di kode dan perlu dirancang (bukan sekadar nilai `AllocationRule` yang sudah didukung). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Tabel eksplisit: "rawat jalan, dicover asuransi → Ya, tetap kena PPN; rawat jalan, dibayar mandiri (excess) → Ya, tetap kena PPN; rawat inap, dicover asuransi → Dibebaskan; rawat inap, dibayar mandiri → Dibebaskan" |
| `BKC-DEC-079` | Decision | IGD diperlakukan SAMA seperti rawat jalan untuk tujuan gerbang PPN ini (kena PPN) — bukan seperti rawat inap. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "IGD diperlakukan seperti rawat jalan (kena PPN)" dari 2 opsi bertanda rekomendasi |

**Status**: `BKC-DEC-076` berstatus `superseded` untuk bagian klaim "berlaku sama di seluruh
unit" — DIGANTIKAN oleh `BKC-DEC-078`–`079`. Bagian `BKC-DEC-076` yang TIDAK direvisi (basis
tetap `item.IsPharmacy=true`, bukan seluruh kategori item) TETAP berlaku. `BKC-DEC-077` (alokasi
`PROPORTIONAL` per payer) TETAP berlaku tanpa perubahan, hanya ruang lakunya menyempit ke
transaksi yang memang kena PPN (rawat jalan/IGD).

**PERINGATAN OPERASIONAL**: sebuah subagent desain (`/design-business-module`, dijalankan
sesaat sebelum revisi ini) sedang/sudah merancang berdasarkan `BKC-DEC-076` versi LAMA yang
menyatakan unit tidak berpengaruh. Rancangan itu perlu ditinjau ulang/dikoreksi terhadap
`BKC-DEC-078`–`079` sebelum dianggap final — jangan diteruskan ke `/plan-module-delivery` tanpa
peninjauan ulang ini.

**Sumber definisi "Rawat Jalan" vs "Rawat Inap" untuk gerbang ini**: belum digali eksplisit —
apakah berdasar `PatientClass`/`ServiceUnitId` pada `TrxPatientEncounter`, atau field lain.
Ini blocker desain untuk `/design-business-module`, bukan keputusan bisnis pass ini.

**Update**: `/design-business-module` sudah dijalankan setelah revisi ini (dikabari eksplisit
lewat pesan lintas-agent sebelum agent tsb selesai) dan menutup pertanyaan sumber definisi di
atas — gerbang dirancang membaca `BilInvoice.ServiceType` (field billing-owned yang sudah ada,
sudah dipakai konsisten untuk room charge), dengan RANAP sebagai satu-satunya nilai yang
dibebaskan; unit yang belum jelas statusnya default kena pajak. Lihat `02-backend-architecture.md`
revisi `0.7` dan `flowcharts/ppn-obat-alkes.md`. **PERINGATAN OPERASIONAL di atas SUDAH SELESAI
ditangani** — dicatat di sini agar terbaca statusnya, bukan dibiarkan menggantung.

## Amendment lanjutan 4 September 2026 — Penutupan lima open question hasil `/design-business-module`

`/design-business-module` (revisi blueprint `0.7`, draft) menghasilkan lima open question yang
memblokir `/plan-module-delivery`: `BKC-OQ-082`–`085` dan `BKC-OQ-090`. Pass ini menutup
seluruhnya lewat wawancara singkat ke Product/Domain Owner.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-080` | Decision | Menutup `BKC-OQ-084` (konflik nyata antara `BKC-DEC-070` dan `BKC-DEC-074`, ditemukan dan dilaporkan apa adanya oleh subagent desain, bukan diam-diam ditebak). Untuk rule yang match dengan residual (`CoveragePercent<100%`/`CoPayment`/`MaxCoverageAmount`) DAN `IsAllowExcessPaymentByPatient=false` pada rule tsb — residualnya BUKAN dibebankan ke pasien, melainkan menjadi kategori tidak-bisa-ditagih yang diproses lewat mekanisme Pengecualian Finansial/write-off yang sudah ada (`BKC-DEC-036`, `SETTLED_BY_WRITE_OFF`/pengurangan outstanding). `BKC-DEC-070` (residual ke pasien) TETAP berlaku sebagai DEFAULT hanya ketika `IsAllowExcessPaymentByPatient=true`; `BKC-DEC-074` (field tetap dipakai runtime) sekarang punya efek nyata yang konsisten alih-alih dekoratif. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Jadi write-off/Pengecualian Finansial, BUKAN ke pasien" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-081` | Decision | Menutup `BKC-OQ-090`. Kode belum ter-commit `BE-BKC-FIX-003`/`FE-BKC-FIX-008` (`BillingCoverageComponentOutcome`, sudah memuat perbaikan bug tabrakan `ComponentId` pada komponen TAX) DIJADIKAN BASIS implementasi — bukan dibuang. Sebelum di-commit lewat alur task normal, WAJIB ditinjau ulang kesesuaiannya terhadap seluruh keputusan sesi ini yang belum ada saat kode itu ditulis, khususnya gerbang PPN rawat jalan/rawat inap (`BKC-DEC-078`–`079`) dan kategori write-off (`BKC-DEC-080`). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Jadikan basis implementasi" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-082` | Decision | **KOREKSI SCOPE** (lihat catatan di bawah tabel) — TIDAK sepenuhnya menutup `BKC-OQ-085` seperti sempat diklaim saat pertanyaan diajukan ke pengguna. `BKC-OQ-085` di `blueprint-manifest.md` aslinya berbunyi "Hitungan dampak penurunan total tagihan rawat inap yang masih OPEN" — pertanyaan ANALISIS DAMPAK FINANSIAL (berapa rupiah pendapatan/PPN yang berkurang akibat pembebasan rawat inap), bukan pertanyaan MITIGASI RISIKO SALAH KATEGORI. Agent salah memparafrasekan pertanyaannya sebelum ditanyakan. Yang benar-benar diputuskan di sini HANYA mitigasi risiko salah kategori: ditambahkan validasi/warning eksplisit ketika `ServiceType` invoice RANAP diubah SETELAH item obat/alkes sudah ditambahkan ke invoice tsb. Bentuk teknis persis (blocking validation vs warning yang bisa diabaikan) adalah keputusan desain lanjutan. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tambahkan validasi/warning eksplisit" dipilih dari 2 opsi (bukan opsi rekomendasi "tidak perlu tambahan") |
| `BKC-DEC-083` | Decision | Menutup `BKC-OQ-083`. MCU, Telemedicine, dan OTC diperlakukan SAMA seperti rawat jalan untuk gerbang PPN (`BKC-DEC-078`) — kena PPN, konsisten dengan aturan "hanya RANAP yang dibebaskan, selebihnya default kena pajak." Tidak ada pengecualian khusus untuk ketiga kanal ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ketiganya rawat jalan - kena PPN" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-084` | Decision | Menutup `BKC-OQ-082` SEBAGIAN — lihat CAVEAT WEWENANG di bawah tabel. `BKC-DES-010`–`020` (kategori anomali data, kontrak breakdown per-item, gerbang PPN rawat jalan/rawat inap) DISETUJUI Product/Domain Owner berdasarkan ringkasan hasil desain yang disampaikan dalam percakapan — status naik dari `draft` menjadi `approved`. Dasarnya: seluruh keputusan bisnis di balik `BKC-DES-010`–`020` (`BKC-DEC-070`–`083`) sudah dikunci eksplisit oleh Product/Domain Owner sepanjang sesi ini sebelum desain dijalankan; `BKC-DES-010`–`020` murni menerjemahkannya jadi bentuk teknis tanpa keputusan bisnis baru yang belum pernah dilihat owner. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Setujui sekarang berdasarkan ringkasan yang sudah disampaikan" dari 2 opsi bertanda rekomendasi |

**CAVEAT WEWENANG untuk `BKC-DEC-084`**: `blueprint-manifest.md` mencatat `BKC-OQ-082` sebagai
"Approval `BKC-DES-010`..`020` oleh Product/Domain Owner **DAN Finance/AR**, khususnya
`BKC-DES-011`" — bukan Product/Domain Owner saja. `BKC-DES-011` secara spesifik menolak
membuat "bucket uang ketiga" untuk nominal anomali dan membuat `PatientAmount` menyerap
`DataAnomalyAmount` sepenuhnya (`BIL-VAL-036`) — artinya PASIEN yang menanggung kegagalan data
milik sistem/registrasi, bukan rumah sakit yang menulis-off-kannya. Ini keputusan kebijakan
finansial yang wilayahnya Finance/AR, sama seperti pola caveat `BKC-DEC-062`/`BKC-DEC-071`
sebelumnya. Approval yang diberikan sesi ini tetap dari Product/Domain Owner TANPA konfirmasi
terpisah dari Finance/AR — dicatat apa adanya sebagai provenance, bukan disembunyikan; bila
Finance/AR keberatan di kemudian hari, `BKC-DES-011` (dan turunannya) perlu ditinjau ulang.

**Status pass ini**: `BKC-OQ-082`, `083`, `084`, `090` DITUTUP PENUH oleh `BKC-DEC-080`,
`081`, `083`, `084` (dengan caveat wewenang Finance/AR di atas untuk `BKC-DEC-084`).
`BKC-OQ-085` **BELUM tertutup** — `BKC-DEC-082` hanya menjawab mitigasi risiko salah kategori
(pertanyaan baru yang berguna), sedangkan pertanyaan analisis dampak finansial ASLI pada
`BKC-OQ-085` masih terbuka, butuh Finance untuk menghitung proyeksi penurunan pendapatan/PPN
akibat pembebasan rawat inap — bukan sesuatu yang bisa dijawab lewat wawancara keputusan
bisnis biasa.

**Tindak lanjut administratif** (bukan keputusan bisnis, dicatat sebagai pengingat): status
`design_decision_status` pada `blueprint-manifest.md` dan baris status `BKC-DES-010`–`020` pada
`02-backend-architecture.md` masih bertuliskan `draft` per revisi `0.7` — perlu disinkronkan
jadi `approved` untuk mencerminkan `BKC-DEC-084`, beserta catatan caveat Finance/AR di atas.
Ini update metadata murni, bukan desain ulang.

## Amendment lanjutan 4 September 2026 — Penutupan caveat Finance/AR dan approval `BKC-DES-001`–`009`

Pengguna secara eksplisit meminta caveat wewenang Finance/AR (`BKC-DEC-084`) dan approval
`BKC-DES-001`–`009` (peninggalan revisi `0.6`, blocking question sejak amendment 3 September
2026) ditangani sekarang, sebelum lanjut ke revisi desain berikutnya.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-085` | Decision | Menutup CAVEAT WEWENANG pada `BKC-DEC-084`. Pengguna mengonfirmasi eksplisit memegang wewenang Finance/AR SEKALIGUS Product/Domain Owner untuk modul `billing-kasir`. `BKC-DES-011` (nominal `DataAnomalyAmount` jatuh penuh ke pasien, TIDAK ditulis-off rumah sakit) dan seluruh `BKC-DES-010`–`020` kini disetujui PENUH dari kedua sisi wewenang — caveat DITUTUP, bukan sekadar dicatat sebagai risiko terbuka. | Product/Domain Owner + Finance/AR (persetujuan eksplisit dalam percakapan, wewenang ganda dikonfirmasi langsung) | `approved` | "Ya, saya berwenang - setujui sekarang" dari 2 opsi |
| `BKC-DEC-086` | Decision | Menaikkan `BKC-DES-007` dari `DEV_DISCRETION` sementara menjadi keputusan terkonfirmasi: dokumen Invoice Asuransi memakai `InvoiceNumber` yang sudah ada, TIDAK membuat seri nomor dokumen baru (`BilNumberSeries`) khusus untuk dokumen ini. Alasan: dokumen ini bukan `Claim Letter` formal untuk pengajuan klaim resmi ke asuransi (tetap milik `InsuranceManagement`, `PLANNED`); `InvoiceNumber` sudah unik dan sudah tercetak konsisten di Kwitansi maupun Struk Pasien. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, pakai InvoiceNumber yang sudah ada" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-087` | Decision | Menyetujui BLANKET sisa `BKC-DES-001`, `003`–`006`, `008`–`009` (format `ComponentKey` teks beralamat komponen, flag boolean `IsPerItemAllocationAvailable`, porsi pajak dilipat ke baris induk lewat kolom `CoveredTaxAmount`, respons `200`+`PayerKind`+`IsPrintable=false` untuk kunjungan tunai/penjamin perusahaan alih-alih `422`, data polis dari kolom snapshot `TrxPatientEncounterGuarantor` bukan `MstPatientInsurance` terkini). Seluruhnya keputusan teknis yang menurunkan langsung dari `BKC-DEC-065`–`069` yang sudah disetujui 3 September 2026, tanpa keputusan bisnis baru di dalamnya. (`BKC-DES-002` TIDAK termasuk — sudah digugurkan dan digantikan `BKC-DES-015` sebelum pass ini.) | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Setujui blanket berdasarkan ringkasan" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: caveat Finance/AR pada `BKC-DEC-084` DITUTUP oleh `BKC-DEC-085`. Seluruh
`BKC-DES-001`, `003`–`009` (blocking question sejak revisi `0.6`) DITUTUP oleh `BKC-DEC-086`–`087`.
`BKC-DES-002` tidak relevan lagi (superseded). Dengan ini, **seluruh `BKC-DES-001`–`020` berstatus
`approved`**, tidak ada satupun yang tersisa `draft`.

**Yang TETAP terbuka setelah pass ini** (tidak disentuh, di luar permintaan eksplisit pengguna
kali ini): pertanyaan analisis dampak finansial asli `BKC-OQ-085` (proyeksi penurunan
pendapatan/PPN rawat inap — butuh Finance, bukan wawancara keputusan bisnis), ratifikasi bentuk
blueprint `BKC-OQ-091` (ditandai tidak memblokir), dan gap desain `BKC-DEC-080` (write-off
untuk residual non-billable) yang belum tertulis implementasinya di `02-backend-architecture.md`
revisi `0.7`.

## Amendment lanjutan 4 September 2026 — Approval `BKC-DES-021`–`025` (revisi `0.8`, addendum `BKC-DEC-080`)

`/design-business-module` dijalankan sebagai addendum bedah (bukan pass baru) untuk menutup gap
desain `BKC-DEC-080` yang tersisa dari amendment sebelumnya — hasilnya blueprint revisi `0.8`
(draft) dengan lima keputusan desain baru `BKC-DES-021`–`025`, seluruhnya berdasar `BKC-DEC-080`
yang sudah `approved`. Ringkasan (detail lengkap di `02-backend-architecture.md` §
"Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off"):
`BKC-DES-021` (field baru `NonBillableResidualAmount`, terpisah dari `DataAnomalyAmount` dan
`UnresolvedAmount`), `BKC-DES-022` (ditangkap di jalur residual `ResolveAsync`, tidak ada nilai
tagihan yang bergeser), `BKC-DES-023` (pemicu write-off MANUAL oleh Finance, bukan otomatis —
sistem hanya mendeteksi dan pre-fill), `BKC-DES-024` (kolom baru `Category` pada
`BilWriteOffCase`, satu migration dua kolom satu index), `BKC-DES-025` (plafon dibaca dari
kolom `BilCalculationVersion`, bukan JSON snapshot).

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-088` | Decision | Menyetujui `BKC-DES-021`–`025` secara utuh, termasuk dua keputusan teknis dengan bobot kebijakan nyata yang secara eksplisit ditonjolkan sebelum approval: (a) `BKC-DES-023` — trigger write-off MANUAL oleh petugas Finance (bukan otomatis oleh sistem saat kalkulasi), dengan alasan `CreateWriteOffAsync` menuntut `RequestedBy`/`Reason`/`Idempotency-Key` dan residual belum final sampai invoice final; (b) `BKC-DES-024` — kolom `Category` (`PATIENT_AR`/`NON_BILLABLE_RESIDUAL`) pada `BilWriteOffCase`, yang berarti SATU MIGRATION (dua kolom, satu index, `NOT NULL` berdefault, tanpa downtime) — persetujuan ini BUKAN persetujuan untuk membuat/menjalankan migration itu sendiri; pembuatan dan eksekusi migration tetap memerlukan otorisasi eksplisit terpisah per `AGENTS.md`/`CLAUDE.md` repo ini saat masuk tahap implementasi. | Product/Domain Owner + Finance/AR (persetujuan eksplisit dalam percakapan, wewenang ganda sudah dikonfirmasi `BKC-DEC-085`) | `approved` | "saya approve" |

**Status pass ini**: `BKC-DES-021`–`025` DISETUJUI PENUH. Dengan ini seluruh `BKC-DES-001`–`025`
(kecuali `BKC-DES-002` yang superseded) berstatus `approved`. Blocking question revisi `0.8`
baris "Approval `BKC-DES-021`–`025`..." pada `blueprint-manifest.md` DITUTUP.

**Yang TETAP terbuka, tidak disentuh pass ini** (dua di antaranya sengaja ditandai
non-blocking oleh desain, bukan diabaikan): `BKC-OQ-093` (jalur `NotCovered` +
`IsAllowExcessPaymentByPatient=false` belum ikut dirutekan ke write-off — rekomendasi desain:
sebaiknya ikut, tapi di luar cakupan `BKC-DEC-080` yang literal hanya bicara residual
perhitungan), `BKC-OQ-094` (apakah finalisasi invoice diblokir selama residual belum
ditulis-off; perlakuan `NON_BILLABLE_RESIDUAL` pada AR/AP handoff), `BKC-OQ-085` asli (analisis
dampak finansial, butuh Finance), `BKC-OQ-091` (ratifikasi bentuk blueprint, non-blocking), dan
review Security atas `BillingInvoice:Read` (peninggalan revisi `0.6`).

## Amendment lanjutan 4 September 2026 — Penutupan `BKC-OQ-093` dan `BKC-OQ-094`

Pengguna meminta seluruh open question yang tersisa diselesaikan sebelum lanjut ke
`/plan-module-delivery`. Dua di antaranya (`BKC-OQ-093`, `BKC-OQ-094a`/`094b`) adalah keputusan
bisnis yang bisa ditutup lewat wawancara singkat; tiga sisanya (`BKC-OQ-085` asli, review
Security, kelengkapan master data UAT) BUKAN keputusan bisnis — butuh analisis Finance,
penilaian tim Security, dan pengecekan data langsung, dijelaskan terpisah di luar tabel ini.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-089` | Decision | Menutup `BKC-OQ-093`. Rule eksplisit `CoverageStatus=NotCovered` dengan `IsAllowExcessPaymentByPatient=false` pada rule tsb — nominalnya IKUT DIRUTEKAN ke mekanisme write-off yang sama dengan residual (`BKC-DEC-080`), BUKAN tetap di jalur lama. Alasan: kedua kasus sama-sama "insurer tidak membayar DAN melarang menagih pasien" — tidak ada dasar membedakan perlakuannya hanya karena sumber angkanya (NotCovered vs residual persentase). Ini memperluas cakupan literal `BKC-DEC-080` yang semula hanya menyebut residual perhitungan. | Product/Domain Owner + Finance/AR (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, ikut dirutekan ke write-off" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-090` | Decision | Menutup `BKC-OQ-094a`. Finalisasi invoice TIDAK diblokir oleh adanya nominal non-billable residual yang belum ditulis-off — sistem menampilkan warning eksplisit ke kasir/Finance, tapi proses finalisasi tetap boleh jalan. Konsisten dengan pola warning yang sudah dipakai untuk kasus anomali data lain (`BKC-DES-010`). | Product/Domain Owner + Finance/AR (persetujuan eksplisit dalam percakapan) | `approved` | "Boleh, dengan warning" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-091` | Decision | Menutup `BKC-OQ-094b`. Nominal non-billable residual (menunggu write-off) berada SEPENUHNYA DI LUAR alur AR/AP — tidak menjadi syarat "porsi pasien lunas" sebelum AP Dokter atau klaim asuransi dianggap ready (`BKC-DEC-037`). Konsisten dengan `BKC-DES-024`: nominal ini sejak awal tidak pernah masuk `PatientAmount`, sehingga secara struktural memang tidak pernah membebani status readiness tsb. | Product/Domain Owner + Finance/AR (persetujuan eksplisit dalam percakapan) | `approved` | "Di luar alur AR/AP sepenuhnya" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: `BKC-OQ-093` dan `BKC-OQ-094` (a dan b) DITUTUP PENUH oleh `BKC-DEC-089`–`091`.

**Yang TIDAK ditutup pass ini, dan TIDAK BISA ditutup lewat wawancara keputusan bisnis** —
masing-masing butuh tindakan konkret di luar percakapan:

1. **`BKC-OQ-085` asli** (dampak finansial): perlu Finance menghitung proyeksi penurunan
   pendapatan/PPN akibat pembebasan rawat inap dari data invoice riil (volume obat/alkes
   rawat inap historis × tarif PPN) — bukan sesuatu yang bisa dijawab dari percakapan tanpa
   akses data laporan keuangan aktual.
2. **Review Security** atas pemakaian ulang `BillingInvoice:Read` untuk dokumen berisi nomor
   polis (Invoice Asuransi) — perlu penilaian tim Security yang berwenang, bukan keputusan
   Product/Domain Owner/Finance.
3. **Kelengkapan `MstInsuranceProvider`/`MstInsuranceCoverageRule` untuk UAT** — perlu
   pengecekan langsung ke data master yang sudah tersimpan di database, bukan keputusan
   kebijakan.

`BKC-OQ-091` (ratifikasi bentuk blueprint `SINGLE`) tetap ditandai non-blocking, sengaja tidak
disentuh karena di luar permintaan eksplisit pengguna sepanjang sesi ini.

## Amendment 5 September 2026 — Penutupan review Security atas pemakaian ulang `BillingInvoice:Read`

Menutup satu dari tiga item pada daftar "Yang TIDAK ditutup pass sebelumnya" di atas (butir 2):
review Security atas pemakaian ulang `BillingInvoice:Read` untuk lembar Invoice Asuransi berisi
nomor polis (`BKC-GATE-03`, menahan `BE-BKC-023` dan `FE-BKC-018`).

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-092` | Decision | Menutup `BKC-GATE-03`. Hak akses `BillingInvoice:Read` yang sudah ada **dipakai ulang apa adanya** untuk membaca dan mencetak Lembar Invoice Asuransi (berisi nomor polis) — **tidak** dibuat permission tersendiri (mis. `BillingInvoice:ReadInsuranceDocument`). Siapa pun yang sudah berwenang membaca invoice biasa kini juga berwenang membaca/mencetak lembar ini; tidak ada perubahan kode otorisasi maupun remapping role yang diperlukan. Risiko yang tetap terbuka dan **bukan** bagian keputusan ini: task `BE-BKC-023`/`FE-BKC-018` tetap **MUST** memastikan nama berkas PDF memakai nomor tagihan (bukan nama pasien) dan tidak ada nomor polis pada log peramban/audit — mitigasi teknis, bukan syarat gate permission. | Security Owner (pengguna, wewenang dikonfirmasi eksplisit saat pertanyaan diajukan — opsi yang dipilih secara sadar mensyaratkan wewenang Security untuk modul ini) | `approved` | "Pakai ulang BillingInvoice:Read apa adanya (Recommended)" dari 2 opsi bertanda rekomendasi, 5 September 2026 |

**Status pass ini**: `BKC-GATE-03` DITUTUP PENUH oleh `BKC-DEC-092`. Dua item tersisa pada daftar
"Yang TIDAK ditutup" di atas (`BKC-OQ-085` asli, kelengkapan master data UAT) **tetap terbuka** —
keduanya butuh analisis Finance/pengecekan data langsung, bukan keputusan kebijakan yang dapat
ditutup lewat wawancara.

## Amendment 7 September 2026 — Penutupan `BKC-GAP-09`–`12` (Struk Pasien vs PDF referensi staging)

`/plan-module-delivery` (roadmap revisi 3) menandai task `FE-BKC-022` `BLOCKED` penuh setelah
pemilik modul menunjukkan PDF "Struk Pembayaran" dari lingkungan staging
(`staging.quilvian-mmchospital.com`) yang formatnya lebih kaya dari implementasi Struk Pasien
lokal saat ini. Empat elemen di PDF itu dicocokkan satu per satu terhadap decision log
(`requirement-traceability.md` § Amendment 7 September 2026) — tidak satupun punya keputusan
yang mengikat penempatannya di Struk Pasien, sehingga dicatat sebagai `BKC-GAP-09`–`12` dan
dikembalikan ke `/grill-me`. **Fakta source yang diverifikasi sebelum bertanya** (bukan
keputusan bisnis): data Company Guarantor (`MstCompanyGuarantor`,
`TrxPatientEncounterGuarantor.CompanyGuarantorId`) sudah ada dan tercatat di sistem sejak
migration 31 Agustus 2026 — pertanyaan `BKC-GAP-10` murni soal penempatan tampilan, bukan
soal data yang belum tersedia.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `BKC-DEC-093` | Decision | Menutup `BKC-GAP-09`. Struk Pasien DITAMBAHKAN breakdown berjenjang Subtotal Mandiri / Subtotal Penjamin / Pajak / Harus Dibayar, sejalan dengan PDF referensi staging. Formula dan datanya sudah `approved` dan sudah terekspos backend (`GET /{id}/calculation-preview`, dipakai Ringkasan Pembayaran) — keputusan ini murni soal penempatan konten dokumen, TIDAK ada formula/kalkulasi baru. Ini MEMPERLUAS cakupan `BKC-DEC-058` AC#26 yang sebelumnya membatasi kewajiban Struk Pasien hanya pada tabel item baris. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, tambahkan breakdown ini" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-094` | Decision | Menutup `BKC-GAP-10`. Struk Pasien MENAMBAHKAN field `Penjamin` (nama Company Guarantor, dari `MstCompanyGuarantor` via `TrxPatientEncounterGuarantor.CompanyGuarantorId`) berdampingan dengan field `Asuransi` (nama `MstInsuranceProvider`) yang sudah ada — TAPI HANYA bila encounter memang punya company guarantor tercatat; bila tidak, field ini kosong/disembunyikan, bukan menampilkan placeholder kosong yang membingungkan. Ini keputusan BARU murni untuk dokumen Struk Pasien — TIDAK menarik atau memperluas `BKC-DEC-067` (yang mengunci Company Guarantor di luar scope untuk dokumen Invoice Asuransi, dokumen BERBEDA). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, tampilkan keduanya bila encounter punya company guarantor" dari 2 opsi bertanda rekomendasi |
| `BKC-DEC-095` | Decision | Menutup `BKC-GAP-11` — DITUNDA, BUKAN ditolak. QR code "Scan untuk verifikasi pembayaran" TIDAK dibangun pada slice ini. Alasan: elemen ini butuh desain mekanisme verifikasi tersendiri (QR memvalidasi ke sistem/endpoint apa, siapa yang men-generate, apa isi payload-nya, apakah publik atau berotentikasi) yang belum pernah dibahas sama sekali di modul ini — bukan sekadar elemen visual yang bisa ditiru dari PDF referensi. Keputusan fail-closed: jangan dibangun sampai mekanismenya digali terpisah lewat `/grill-me` khusus, bila memang dibutuhkan di kemudian hari. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tunda dulu, jangan dibangun sekarang" dari 2 opsi, ditandai fail-closed |
| `BKC-DEC-096` | Decision | Menutup `BKC-GAP-12`. Struk Pasien DITAMBAHKAN blok tanda tangan "Kasir" dan "Penerima" sebagai elemen cetak sederhana — nama kasir diambil dari sesi pengguna yang mencetak, kolom "Penerima" dikosongkan untuk diisi tanda tangan/nama fisik saat dicetak. Ini TIDAK terkait dan TIDAK menarik keputusan `BKC-DEC-069` (blok tanda tangan pada Invoice Asuransi, `DEV_DISCRETION` untuk dokumen itu) — keputusan ini berdiri sendiri khusus untuk Struk Pasien. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, tambahkan sebagai elemen cetak sederhana" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: `BKC-GAP-09`, `10`, `12` DITUTUP PENUH dengan keputusan "ya, bangun" —
`FE-BKC-022` bisa direncanakan ulang untuk ketiga elemen ini. `BKC-GAP-11` (QR code) DITUTUP
sebagai keputusan eksplisit "tunda", BUKAN dibiarkan menggantung — `FE-BKC-022` untuk elemen ini
TETAP `BLOCKED` sampai ada kebutuhan bisnis yang jelas dan mekanisme verifikasinya digali
terpisah, tapi ini keputusan sadar, bukan open question yang lupa dijawab.

**Blocker desain yang masih terbuka sebelum implementasi** (bukan keputusan bisnis, cocok untuk
`/design-business-module` touch-up kecil sebelum `/plan-module-delivery` menandai `FE-BKC-022`
siap penuh): kontrak API/DTO untuk mengekspos field Company Guarantor (`BKC-DEC-094`) pada
response yang dipakai Struk Pasien saat ini — perlu dicek apakah `GET /{id}/calculation-preview`
atau endpoint lain yang sudah dikonsumsi frontend untuk Struk Pasien sudah membawa field ini,
atau perlu field baru ditambahkan.

**Di luar scope — tidak disentuh pass ini**: mekanisme verifikasi QR (`BKC-GAP-11`, sengaja
ditunda); Invoice Asuransi (`FE-BKC-018`, sudah siap dibangun ulang lewat `build-module-frontend`
tanpa perlu wawancara tambahan).

## Amendment 7 September 2026 — Kapabilitas baru: Petty Cash (Voucher Kas Kecil)

Pengguna meminta kapabilitas baru: monitoring dan approval voucher Petty Cash (kas kecil),
disertai referensi tampilan (daftar voucher dengan filter, modal "Buat Voucher", modal "Bukti
Nota/Kasir", kartu ringkasan "Total Petty Cash"). Ini sudah tercatat sebagai antrian sejak
amendment sebelumnya ("Petty Cash — tidak disebut di dokumen blueprint billing-kasir manapun
sebelum pass ini... dicatat sebagai antrian") — pass ini menutup antrian tersebut.

**Batas scope**: siklus hidup voucher Petty Cash — pengajuan, approval/penolakan, pencairan,
bukti nota, dan sumber anggaran kumulatif. **Di dalam scope**. **Di luar scope — tidak
ditemukan titik singgung ke modul lain sejauh ini**, kecuali satu titik sentuh yang secara
eksplisit DIPUTUSKAN TIDAK terhubung (lihat `BKC-DEC-097`).

**Fakta source diverifikasi sebelum bertanya**: modul ini sudah punya kapabilitas Shift Kasir
(`BilCashierShift`, `CashierShiftService`, `BKC-DEC-038`) yang melacak kas fisik per shift —
dicek apakah Petty Cash memakai kas yang sama sebelum pertanyaan diajukan.

**Bentuk blueprint**: Petty Cash dinilai sebagai rumpun baru yang MEMENUHI 4 dari 5 syarat
pemecahan (bounded context sendiri, kosakata status sendiri, dapat dirilis mandiri, master data
dan approval sendiri — hanya syarat "pemilik peran sendiri" yang tidak penuh karena approver-nya
sama dengan Shift Kasir, Kepala Kasir/Finance Operations). TIDAK diusulkan `COMPOSITE` untuk
modul secara keseluruhan: `billing-kasir` sudah punya banyak rumpun setara-berbeda (Shift Kasir,
Diskon, Deposit, Refund, Pengecualian Finansial, dll.) yang seluruhnya hidup sebagai rumpun di
dalam SATU blueprint `SINGLE` sejak revisi `0.2`, tanpa pernah dipecah meski masing-masing bisa
lolos uji serupa. Petty Cash mengikuti preseden yang sama: rumpun baru di dalam struktur `SINGLE`
yang sudah ada. Ini TIDAK mengubah status `BKC-OQ-091` (ratifikasi bentuk modul keseluruhan,
masih pending, tidak memblokir).

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `PC-DEC-001` | Decision | Dana Petty Cash yang dicairkan berasal dari ANGGARAN PETTY CASH TERPISAH — TIDAK terhubung ke kas fisik Shift Kasir (`BilCashierShift`) manapun. Pencairan voucher TIDAK memengaruhi perhitungan variance/selisih kas saat shift ditutup (`BKC-DEC-038`). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Anggaran Petty Cash terpisah, TIDAK terhubung ke kas fisik shift kasir manapun" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-002` | Decision | "Total Petty Cash" adalah SALDO BERJALAN: nilai anggaran yang di-set/di-top-up manual oleh Finance, dikurangi voucher yang sudah berstatus "Uang Diterima" (lihat `PC-DEC-009` soal titik pengurangan persis). Bukan sekadar akumulasi historis nominal seluruh voucher. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Saldo berjalan: anggaran yang di-set manual dikurangi voucher yang sudah cair" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-003` | Decision | Voucher berstatus "Ditolak" TIDAK BISA diedit/diajukan ulang — catatan permanen sebagai audit trail keputusan approval. Kalau kebutuhan sama masih ada, pemohon membuat pengajuan baru dari awal. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Harus buat voucher baru" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-004` | Decision | Approval ("Disetujui"/"Ditolak") berwenang WAJIB Kepala Kasir/Finance Operations — SATU jenjang approval, TANPA eskalasi berjenjang berdasar nominal voucher. Owner sama dengan kapabilitas Shift Kasir yang sudah ada. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Kepala Kasir/Finance Operations" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-005` | Decision | Aksi "Uang Diberikan" adalah SATU LANGKAH oleh kasir/petugas — begitu ditekan, status LANGSUNG menjadi "Uang Diterima" tanpa konfirmasi terpisah dari penerima (tidak ada tanda tangan digital/OTP penerima pada slice ini). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Satu langkah oleh kasir" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-006` | Decision | Voucher berstatus "Uang Diterima" yang bukti notanya TIDAK PERNAH diinput TETAP MENGGANTUNG tanpa batas waktu pada MVP ini — TIDAK ADA mekanisme pemaksaan, eskalasi, atau pemblokiran otomatis. Finance memantau manual di luar sistem. Ditandai eksplisit sebagai kandidat penyempurnaan rilis berikutnya, BUKAN kelalaian. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tetap menggantung tanpa batas waktu, tidak ada mekanisme paksa" dari 2 opsi, ditandai rekomendasi untuk MVP |
| `PC-DEC-007` | Decision | Pemohon BOLEH membatalkan pengajuannya sendiri SELAMA masih berstatus "Menunggu Persetujuan" (`PC-DEC-013`). Begitu sudah diputuskan (Disetujui/Ditolak), TIDAK BISA dibatalkan lagi. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Bisa, selama masih berstatus pending/belum diputuskan" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-008` | Decision | Nominal voucher DIVALIDASI terhadap sisa saldo anggaran SEBELUM/SAAT approval — sistem mencegah atau memperingatkan approver secara eksplisit kalau nominal yang mau disetujui akan membuat saldo anggaran negatif. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, ditolak/diperingatkan otomatis kalau melebihi sisa anggaran" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-009` | Decision | Saldo anggaran Petty Cash BERKURANG PERSIS pada saat status berubah menjadi "Uang Diterima" — bukan menunggu status "Selesai" (bukti nota). Status "Selesai" murni soal kelengkapan administratif (bukti sudah lengkap), TIDAK mengubah saldo lagi. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Saat status menjadi 'Uang Diterima'" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-010` | Decision | HANYA SATU pool anggaran Petty Cash untuk SELURUH rumah sakit pada MVP — bukan per unit/departemen/cabang. Pemisahan multi-pool eksplisit ditunda sebagai kandidat rilis berikutnya, bukan ditolak permanen. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Satu pool tunggal untuk seluruh rumah sakit" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-011` | Decision | Voucher DIBUAT oleh kasir/petugas administrasi (peran dengan akses ke form "Buat Voucher") ATAS NAMA siapa saja — field "Nama Penerima" tetap berupa teks bebas seperti pada rujukan tampilan, BUKAN dipilih dari daftar pegawai HR, dan BUKAN model self-service tempat setiap pegawai mengajukan untuk dirinya sendiri. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Kasir/petugas administrasi input atas nama siapa saja" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-012` | Decision | Daftar "Kategori" voucher (Transport, Operasional, Konsumsi, Maintenance, ATK, dst.) adalah MASTER DATA yang dapat dikelola (tambah/ubah/nonaktifkan) oleh Finance lewat menu tersendiri — BUKAN daftar tetap yang di-hardcode di kode aplikasi. Pola konsisten dengan master data lain di modul ini (`MstTaxRule`, `MstInsuranceCoverageRule`, dst.). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Master data yang bisa dikelola Finance" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-013` | Decision | Status awal voucher saat pertama dibuat (sebelum diputuskan) berlabel **"Menunggu Persetujuan"** — bukan "Diajukan". Kosakata status lengkap yang dikunci pass ini: `Menunggu Persetujuan` → (`Disetujui` → `Uang Diterima` → `Selesai`) ATAU (`Ditolak`, terminal). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "\"Menunggu Persetujuan\"" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: `PC-DEC-001`–`013` (13 keputusan) MENGUNCI seluruh siklus hidup, wewenang,
formula saldo, dan sumber master data untuk MVP Petty Cash. Prefix `PC-` dipakai (bukan `BKC-`)
karena ini rumpun baru dengan kosakata sendiri di dalam blueprint `billing-kasir` yang sama —
konsisten dengan penomoran per-rumpun yang sudah dipraktikkan modul ini secara implisit
(`BKC-DES-*` untuk keputusan desain, `BKC-GAP-*`/`BKC-OQ-*` untuk gap terpisah dari `BKC-DEC-*`
keputusan bisnis inti). Jejak traceability tetap ke `blueprint_id: BIL-CASH-001` yang sama.

**Yang TIDAK ditutup pass ini, dicatat eksplisit sebagai kandidat rilis berikutnya (BUKAN
diabaikan)**: pengingat/eskalasi otomatis untuk bukti nota yang terlambat (`PC-DEC-006`);
pemisahan anggaran multi-pool per unit/departemen (`PC-DEC-010`); model self-service pegawai
mengajukan untuk diri sendiri (`PC-DEC-011`); approval berjenjang berdasar nominal (`PC-DEC-004`).

**Di luar scope — untuk modul lain**: tidak ditemukan titik singgung ke modul lain pada pass
ini, selain konfirmasi eksplisit TIDAK terhubung ke Shift Kasir (`PC-DEC-001`).

**Langkah berikutnya**: kapabilitas ini sepenuhnya BARU (tidak ada source code sama sekali,
sudah dicek — nol hasil untuk "PettyCash" di backend maupun frontend selain catatan antrian di
file ini). Sesuai `CLAUDE.md`, langkah berikut yang sesuai adalah `trace-existing-capabilities`
(audit cepat memastikan tidak ada kapabilitas serupa yang terlewat, mis. modul Expense/Kas Kecil
di area Corporate), lalu `design-business-module` untuk arsitektur backend/frontend penuh,
sebelum `plan-module-delivery`.

## Amendment 7 September 2026 — Penutupan `PC-OQ-001` dan approval `PC-DES-001`–`014`

`/design-business-module` (revisi blueprint `1.0`, draft) menghasilkan 14 keputusan desain
(`PC-DES-001`–`014`) dan satu open question teknis: `PC-OQ-001`, penamaan master data kategori
— agent desain menamainya `MstPettyCashCategory` (pola `Mst*` yang dipakai 6 dari 6 master data
lain di modul ini), sementara pengarahan awal sempat menyebut prefix `Bil*` karena kepemilikan
modulnya. Pengguna diberi pilihan eksplisit sebelum dikunci.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `PC-DEC-014` | Decision | Menutup `PC-OQ-001`. Master data kategori Petty Cash memakai nama `MstPettyCashCategory` (BUKAN `BilPettyCashCategory`) — konsisten dengan konvensi `Mst*` yang menandai JENIS data (master/reference), bukan kepemilikan modul, dan sudah dipakai seluruh master data lain di `billing-kasir` (`MstTaxRule`, `MstPaymentMethod`, dst.) walau sama-sama dimiliki modul ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "MstPettyCashCategory" dari 2 opsi bertanda rekomendasi |
| `PC-DEC-015` | Decision | Menyetujui `PC-DES-001`–`014` secara utuh — arsitektur backend (`BilPettyCashVoucher`, `BilPettyCashVoucherCommand`, `BilPettyCashBudget`, `BilPettyCashBudgetMovement`, `MstPettyCashCategory`), perluasan `BilNumberSeries` untuk penomoran voucher, mekanisme saldo `CurrentBalance` + ledger dengan penjaga dua lapis (approval dan pencairan), pembatalan lewat `IsCancel` (bukan status keenam), serta permission `PettyCashVoucher`/`PettyCashBudget`/`PettyCashCategory`. Status naik dari `draft` menjadi `approved` untuk revisi `1.0`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | Konfirmasi eksplisit setelah `PC-OQ-001` diluruskan |

**Status pass ini**: `PC-OQ-001` DITUTUP. `PC-DES-001`–`014` DISETUJUI PENUH. Tidak ada open
question bisnis tersisa untuk rumpun Petty Cash. `PC-OQ-003` (baris registry kepemilikan modul
untuk folder `PettyCash/`, `QBE-MOD-003`) TETAP terbuka tapi TIDAK memblokir `plan-module-delivery`
— ia memblokir penulisan file model pertama saat implementasi, dicatat sebagai prasyarat
`build-module-backend`, bukan blocker perencanaan.

## Amendment 11 September 2026 — Kapabilitas baru: Edit Tagihan & Multi-Payer Coverage

Pengguna mengajukan dokumen BRD/PRD eksternal ("Edit Tagihan & Multi-Payer Coverage", versi 1.0,
11 September 2026) yang mengusulkan: (1) satu encounter dapat memakai insurance pribadi DAN
company guarantor sekaligus (payment source *one-to-many*); (2) master baru
`MstCompanyGuarantorReimbursementRoute` dan `MstCompanyGuarantorCoverageRule`; (3) tiga mode Edit
Tagihan pada Menu Pembayaran — Edit Asuransi, Edit Status Tagihan (penanggung per item), Edit
Billing (penebusan obat); (4) dokumen Invoice Company Guarantor terpisah dari Invoice Asuransi.
Sebelum pertanyaan bisnis diajukan, dilakukan audit read-only terhadap blueprint ini dan modul
tetangga.

**Fakta source diverifikasi sebelum bertanya** (bukan keputusan bisnis):

1. `RegPatientEncounterGuarantor` dan `EncounterInsuranceService` COCOK dengan source aktual —
   dikonfirmasi LANGSUNG dari kode:
   [Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs](../../../Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs)
   dan
   [Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs](../../../Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs)
   dua-duanya ADA dan aktif dipakai. `EncounterInsuranceService` persis seperti klaim PDF: hanya
   menangani `PaymentType.Cash`/`.Insurance`, menolak tipe lain eksplisit ("Tipe pembayaran
   encounter tidak didukung.").
2. `MstPatientCompanyGuarantor` ([Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs](../../../Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs))
   sudah punya field perusahaan, nomor karyawan, benefit plan, masa berlaku, dan eligibility.
   `MstCompanyGuarantor` (master perusahaan itu sendiri) ada di
   [Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs](../../../Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs)
   — **milik area `Administrator`, bukan `HealthServices/MasterData`** seperti diasumsikan awal.
   Menambah satu owner relevan untuk `MPY-OQ-002` di bawah.
3. Field `ExcessAmount`/`ExcessStatus` pada kontrak kalkulasi `billing-kasir` sudah ada sejak
   awal sebagai cadangan untuk "penjamin kedua", tapi sengaja dikunci permanen ke
   `0`/`"NOT_CONFIGURED"` lewat `BKC-DES-014` (approved,
   [02-backend-architecture.md](./02-backend-architecture.md)) — bukti bahwa konsep payer kedua
   pernah muncul di level kontrak dan secara sadar diputuskan tetap dorman.
4. Blueprint `billing-kasir` sendiri belum pernah membahas konsep Edit Tagihan/Edit Asuransi/Edit
   Billing/`IsPrimary`/switch insurer sama sekali sebelum pass ini (grep penuh berkas ini sebelum
   amendment ini, nol hasil).
5. **Relasi 1:1 dikonfirmasi LANGSUNG dari EF configuration**, bukan dari prosa blueprint mana
   pun:
   [Repositories/Configurations/HealthServices/RegPatientEncounterGuarantorConfiguration.cs](../../../Repositories/Configurations/HealthServices/RegPatientEncounterGuarantorConfiguration.cs)
   — `entity.HasOne(x => x.Encounter).WithOne(x => x.PaymentSource)...`, dan
   `entity.HasIndex(x => x.EncounterId).IsUnique()` dengan komentar source asli persis:
   *"Menjamin satu encounter hanya mempunyai satu sumber pembayaran."* `RegPatientEncounter.PaymentSource`
   dikonfirmasi properti tunggal (`RegPatientEncounterGuarantor? PaymentSource`, BUKAN
   collection) di `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs`.

**Catatan koreksi (11 September 2026)**: draft awal amendment ini sempat keliru menyimpulkan
istilah teknis PDF "tidak cocok source", karena riset awal membaca dokumentasi prosa
`billing-kasir` sendiri (`erd/00-context-erd.md`, yang memakai nama lama
`TrxPatientEncounterGuarantor` — git log mengonfirmasi model ini memang pernah berprefix `Trx*`
sebelum di-rename ke `Reg*` mengikuti registry, commit `946b95a7` lalu `58c61a5b`) alih-alih
membaca source aktual. Pengguna mengoreksi ini secara langsung, dan fakta #1 serta #5 di atas
sudah diperbaiki dengan bukti langsung dari source terkini. **Kesimpulan `MPY-DEC-001` di bawah
TIDAK berubah — justru makin kuat**, karena unique index `EncounterId` kini terverifikasi
langsung dari EF configuration, bukan dari kutipan dokumen pihak lain. Yang berubah hanya
atribusi: dokumentasi internal `billing-kasir` (`erd/00-context-erd.md`) belum diperbarui
mengikuti rename `Trx→Reg`, bukan dokumen PDF pengguna — dicatat sebagai temuan data-hygiene
terpisah untuk blueprint ini, bukan bagian keputusan bisnis pass ini.

**Konflik ditemukan sebelum bertanya**: premis inti dokumen PDF (dua payer aktif bersamaan per
encounter) bertentangan langsung dengan model yang terverifikasi langsung dari source (fakta #5)
DAN dengan kontrak lintas modul `RWI-ENC-PAYER-001` (v1.0.0, **approved**, disetujui Muhammad
Hamzah 31 Agustus 2026, pemilik `RegistrationManagement`) — *"satu encounter hanya boleh
mempunyai satu sumber pembayaran"*, ditegakkan unique index `EncounterId` pada
`RegPatientEncounterGuarantor` yang **sudah live di database dev** sejak task `BE-RWI-035`
(selesai, migration diterapkan 31 Agustus 2026). Topik "prioritas/multiple coverage antar payer"
juga sudah tercatat sebagai open decision (`INS-DEC-004`) di modul `insurance-management` yang
terpisah — modul itu sendiri berstatus pilot/belum diotorisasi produksi.

**Batas scope**: modul `billing-kasir`. **Di dalam scope** — kasir dapat mengoreksi payer aktif
encounter (switch, bukan tambah) dan penanggung per item tagihan, mengatur penebusan obat,
sebelum invoice dibayar; coverage rule dan reimbursement route Company Guarantor; dokumen Invoice
Company Guarantor terpisah. **Di luar scope — untuk modul lain**: kepemilikan
`MstPatientInsurance`/`MstPatientCompanyGuarantor`/`TrxPatientEncounterGuarantor`/
`InsuranceCoverageService` (RegistrationManagement/PatientManagement/ClinicalManagement);
prioritas/multiple coverage antar payer (`INS-DEC-004`, insurance-management). **Di luar scope —
sesuai PDF sendiri, dikonfirmasi selaras temuan repo**: Edit Company Guarantor pasien dari
billing (tetap di Data Pasien/Registrasi, konsisten dengan gap `RWI-CAP-002` yang juga belum
tertutup di modul asalnya), company tariff book baru, partial quantity redemption obat, payer
switch pasca-pembayaran.

**Bentuk blueprint**: rumpun baru ini dinilai sebagai SATU rumpun (bukan pecahan sub-modul) —
kelima kemampuannya (Edit Asuransi, Edit Status Tagihan, Edit Billing, coverage rule Company
Guarantor, invoice Company Guarantor) berbagi `BilInvoice` yang sama, lifecycle OPEN/pre-payment
yang sama, dan aktor kasir yang sama; tidak ada dua sub-rumpun yang lolos 3 dari 5 syarat
pemecahan. Mengikuti preseden rumpun Petty Cash: rumpun baru di dalam struktur `SINGLE`
`billing-kasir` yang sudah ada, dengan prefix keputusan sendiri `MPY-*` (Multi-PaYer) — bukan
melanjutkan sekuens `BKC-DEC-*`.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MPY-DEC-001` | Decision | Menutup konflik model payer. Encounter TETAP tepat SATU payment source aktif — `RWI-ENC-PAYER-001` TIDAK diusulkan berubah, TIDAK perlu approval `RegistrationManagement`. "Edit Asuransi" berarti MENGGANTI (switch) payer aktif encounter, BUKAN menambah payer kedua. Opsi "Penjamin" pada Edit Status Tagihan per item HANYA muncul sebagai target valid bila payment source encounter itu sendiri memang `COMPANY_GUARANTOR` — bukan pilihan tambahan yang hidup berdampingan dengan `INSURANCE` pada encounter yang sama. Premis "one-to-many payment source" dari dokumen PDF DITOLAK untuk MVP ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Satu payer aktif, tetap switchable (Direkomendasikan)" dari 3 opsi bertanda rekomendasi |
| `MPY-DEC-002` | Decision | Bentuk blueprint: rumpun baru "Edit Tagihan & Multi-Payer Coverage" tetap di dalam `billing-kasir` `SINGLE` (bukan blueprint/modul terpisah), memakai prefix keputusan `MPY-DEC-*`/`MPY-DES-*` mengikuti preseden `PC-DEC-*`/`PC-DES-*` (Petty Cash) — rumpun baru dengan kosakata sendiri di dalam blueprint yang sama. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Prefix baru MPY-DEC-*/MPY-DES-* (Direkomendasikan)" dari 2 opsi bertanda rekomendasi |
| `MPY-DEC-003` | Decision | Edit Asuransi mendukung switch PaymentType APAPUN ke APAPUN (Tunai/Asuransi/Penjamin) pada encounter yang sama, SELAMA target payer sudah terdaftar valid di profil pasien (`MstPatientInsurance` aktif+eligible untuk Asuransi, `MstPatientCompanyGuarantor` aktif+eligible untuk Penjamin) — TIDAK PERNAH membuat/menambah data penjamin baru dari billing, hanya memilih yang sudah tercatat. Menutup skenario "pasien lupa bawa kartu asuransi saat registrasi, ketahuan sebelum bayar". Tidak bertentangan dengan "Edit Penjamin out of scope" (PDF §9.5/BR-16) karena itu soal MENGUBAH/MENAMBAH data penjamin pasien (tetap di Data Pasien/Registrasi), sementara ini MEMILIH opsi yang sudah ada di profil untuk encounter ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Semua PaymentType bisa saling diubah (Direkomendasikan)" dari 3 opsi bertanda rekomendasi |

| `MPY-DEC-004` | Decision | Edit Status Tagihan TIDAK menggerbang opsi "Asuransi" berdasarkan hasil evaluasi coverage per item. Availability dropdown payer per item HANYA berdasarkan payer source apa yang aktif pada encounter (Pribadi selalu ada; Asuransi ada bila `PaymentType=Insurance`; Penjamin ada bila `PaymentType=CompanyGuarantor`) — BUKAN berdasarkan apakah item itu secara spesifik tercover rule. Item yang ditugaskan ke Asuransi tapi ternyata `NotCovered` tetap dihitung otomatis oleh coverage engine existing (hasil 0% coverage, pasien bayar penuh) — TIDAK diblokir di level UI. Konsisten dengan `FR-07` PDF dan cara kerja `InsuranceCoverageService` existing yang selalu menghasilkan persentase, bukan gate biner. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Asuransi tetap bisa dipilih walau tidak tercover (Direkomendasikan)" dari 2 opsi bertanda rekomendasi |

| `MPY-DEC-005` | Decision | Edit Asuransi/Edit Status Tagihan/Edit Billing TIDAK butuh approval kedua — satu kasir berwenang (yang sudah punya akses fitur ini) dapat langsung menyimpan perubahan, konsisten dengan pola registrasi (satu petugas admisi menentukan payer awal tanpa approval berjenjang). Kontrol tetap ditegakkan lewat audit lengkap SETELAH fakta (actor, timestamp, reason, row version, correlation/causation — sudah jadi NFR wajib di `FR`/`NFR` PDF §6.3–6.4), BUKAN lewat pencegahan dua-tahap SEBELUM fakta seperti write-off (`BIL-VAL-017`) atau Petty Cash (`PC-DEC-004`). Alasan pembeda: write-off/Petty Cash bukan proses real-time di depan pasien yang menunggu, sedangkan Edit Tagihan adalah alur kasir yang harus cepat. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Satu kasir langsung menyimpan, tanpa approval kedua (Direkomendasikan)" dari 2 opsi bertanda rekomendasi |

| `MPY-DEC-006` | Decision | Dokumen Invoice Company Guarantor memakai ulang permission `BillingInvoice:Read` yang sudah ada — TIDAK dibuat permission baru, mengikuti preseden persis `BKC-DEC-092` (Invoice Asuransi). Siapa pun yang sudah berwenang membaca invoice biasa juga berwenang membaca/mencetak lembar ini. Mitigasi teknis yang sama seperti Invoice Asuransi tetap berlaku: nama berkas PDF memakai nomor tagihan (bukan nama pasien/perusahaan), tidak ada data sensitif pada log peramban/audit. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Pakai ulang BillingInvoice:Read (Direkomendasikan)" dari 2 opsi bertanda rekomendasi |

**Status pass ini**: enam keputusan inti rumpun "Edit Tagihan & Multi-Payer Coverage" TERKUNCI
(`MPY-DEC-001`–`006`) — model payer, bentuk blueprint, cakupan Edit Asuransi, gate coverage per
item, model approval, dan permission dokumen Company Guarantor.

**Asumsi yang diwarisi dari dokumen PDF pengguna** (tidak ditanyakan ulang karena sudah cukup
jelas dan tidak kontroversial pada dokumen sumber; ditinjau ulang bila `design-business-module`
menemukan kontradiksi dengan source):

- Edit Billing (penebusan obat) murni flag inklusi billing, TIDAK menyentuh data klinis
  resep/dispensing (`BR-09` PDF) — item `EXCLUDED` tidak masuk total tagihan pasien, konsisten
  dengan pola `Voided` yang sudah ada di glosarium modul ini (tersimpan lengkap untuk audit,
  tidak dihapus fisik, tidak masuk total).
- Sinyal eligibility RAJAL/IGD/OTC vs RANAP untuk Edit Billing SEBAIKNYA memakai ulang sinyal
  `ServiceType` yang sudah dipakai gerbang PPN rawat inap/rawat jalan (`EPIC BKC-08`,
  `BKC-DEC-081`/`082`) — bukan sinyal baru. Ini rekomendasi reuse, bukan keputusan bisnis; MUST
  diverifikasi field persisnya saat `design-business-module`.
- Ringkasan tagihan MUST membedakan `Subtotal Mandiri`/`Subtotal Asuransi`/`Subtotal Penjamin`
  secara eksplisit (bukan menggabungkan Penjamin ke label Asuransi) — ini konsekuensi langsung
  dan tidak ambigu dari `MPY-DEC-001`/`003`, bukan keputusan terpisah yang perlu ditanya ulang.

**Open question / dependency lintas modul — TIDAK memblokir wawancara, MUST diselesaikan sebelum
implementasi**:

- `MPY-OQ-001` — Edit Asuransi butuh menulis ke `RegPatientEncounterGuarantor` (kolom payer
  aktif), tabel milik `RegistrationManagement`. `MPY-DEC-001`/`003` memastikan TIDAK ada
  perubahan skema/kontrak `RWI-ENC-PAYER-001`, tapi endpoint/service yang benar-benar melakukan
  penulisan (dipanggil dari konteks billing-kasir, atau disediakan `RegistrationManagement`
  untuk dipanggil billing-kasir) belum ditentukan. Perlu koordinasi dengan pemilik
  `RegistrationManagement` (kontak tercatat: Muhammad Hamzah) sebelum desain backend dikunci.
  Penjawab: pemilik arsitektur backend + pemilik `RegistrationManagement`.
- `MPY-OQ-002` — Nama entity master baru (`MstCompanyGuarantorReimbursementRoute`,
  `MstCompanyGuarantorCoverageRule`) memakai prefix `Mst`, tapi `MstCompanyGuarantor` (parent-nya)
  terdaftar milik area `Administrator`, sementara `MstInsuranceCoverageRule` (pola yang ditiru)
  ada di `HealthServices/MasterData`. Penempatan folder entity baru — ikut `Administrator`
  (co-locate dengan parent) atau `HealthServices/MasterData` (co-locate dengan pola yang ditiru)
  — belum ditentukan, dan menentukan siapa yang sign-off pembuatan entity-nya. Penjawab: pemilik
  arsitektur backend.
- `MPY-OQ-003` — Modul `pharmacy` (sibling blueprint) mungkin sudah punya kapabilitas tracking
  penebusan/dispensing resep sendiri. Belum diverifikasi apakah "Edit Billing" di rumpun ini
  akan tumpang tindih atau melengkapi kapabilitas itu. TIDAK memblokir pass ini karena `BR-09`
  PDF sudah eksplisit membatasi Edit Billing hanya sebagai flag inklusi billing (bukan klaim atas
  data dispensing) — tapi MUST dicek `trace-existing-capabilities` terhadap modul `pharmacy`
  sebelum `design-business-module` mengunci kontrak field eligibility-nya. Penjawab: pemilik
  arsitektur backend.

**Acceptance criteria awal** (turunan langsung `MPY-DEC-001`–`006`, akan diperkaya
`design-business-module`):

1. Encounter tetap punya tepat SATU `RegPatientEncounterGuarantor` aktif per waktu — unique
   index `EncounterId` TIDAK pernah dilonggarkan oleh rumpun ini.
2. Edit Asuransi berhasil men-switch Tunai↔Asuransi↔Penjamin HANYA jika target payer sudah
   terdaftar aktif+eligible milik pasien yang sama (`MstPatientInsurance`/
   `MstPatientCompanyGuarantor`); gagal (`422`) bila tidak.
3. Dropdown "Asuransi" pada Edit Status Tagihan tetap enabled untuk item `NotCovered` — hasil
   kalkulasi tetap jalan otomatis (0% coverage, pasien bayar penuh), tidak diblokir UI.
4. Dropdown "Penjamin" pada Edit Status Tagihan hanya muncul ketika `PaymentType` encounter
   persis `CompanyGuarantor`.
5. Save Edit Asuransi/Edit Status Tagihan/Edit Billing berhasil dengan satu kasir berwenang,
   TANPA langkah approval kedua — tapi menghasilkan audit trail lengkap (actor, waktu, reason,
   row version, correlation/causation).
6. Invoice Company Guarantor dapat dibaca/dicetak oleh siapa pun yang memegang
   `BillingInvoice:Read`, tanpa permission baru.
7. Stale row version pada save manapun mengembalikan `409` tanpa partial write.

**Langkah berikutnya**: enam keputusan bisnis inti sudah terkunci. Karena `01-existing-capability-map.md`
modul ini terakhir diaudit pada SHA yang sudah beda dari HEAD saat ini (dikonfirmasi pass ini —
lihat catatan koreksi di atas), langkah yang disarankan sebelum `design-business-module` adalah
`trace-existing-capabilities` mode impact scan, khusus menutup `MPY-OQ-001`–`003` dan memverifikasi
tidak ada kapabilitas lain yang bergeser sejak audit terakhir.

## Amendment 11 September 2026 (lanjutan) — Penutupan `MPY-OQ-001`–`003`

Pengguna menjawab ketiga open question di atas. Sebelum dicatat sebagai closed, setiap klaim
teknis baru pada jawaban (nama class/file yang belum pernah diverifikasi pass ini) dicek LANGSUNG
ke source — bukan diterima apa adanya — mengikuti disiplin yang sama seperti koreksi sebelumnya
pada amendment ini.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MPY-DEC-007` | Decision | Menutup `MPY-OQ-001` SEBAGIAN. Write authority `RegPatientEncounterGuarantor` TETAP pada `RegistrationManagement` — billing-kasir TIDAK PERNAH menulis langsung ke aggregate ini. Edit Asuransi (`MPY-DEC-003`) mengorkestrasi perubahan lewat service milik `RegistrationManagement` (endpoint/service tsb BELUM ada di source, MUST dibangun sebagai bagian slice ini, atas otorisasi terpisah dari pemilik `RegistrationManagement`). Perubahan model dari one-to-one menjadi multi-payer (bila kelak dibutuhkan) MUST melalui perubahan kontrak `RegistrationManagement`, bukan keputusan sepihak billing-kasir — memperkuat `MPY-DEC-001`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` (posisi arsitektur) | "Write authority tetap RegistrationManagement; Billing mengorkestrasi, tidak menulis langsung" |
| `MPY-DEC-008` | Decision | Menutup `MPY-OQ-002` PENUH. `MstCompanyGuarantorReimbursementRoute` → `Areas/Administrator/MasterData/` — co-locate `MstCompanyGuarantor` DAN `MstInsuranceProvider`, DIKONFIRMASI LANGSUNG dari source keduanya ada di folder yang sama ([MstCompanyGuarantor.cs](../../../Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs), [MstInsuranceProvider.cs](../../../Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs)). `MstCompanyGuarantorCoverageRule` → `Areas/HealthServices/MasterData/` — co-locate `MstInsuranceCoverageRule`, DIKONFIRMASI LANGSUNG ada di folder itu ([MstInsuranceCoverageRule.cs](../../../Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs)). Pola boundary konsisten dan terverifikasi: entity "pihak/kontrak" (Provider, Guarantor) → `Administrator`; entity "aturan eligibility layanan kesehatan" (CoverageRule) → `HealthServices/MasterData`. Sign-off pembuatan entity baru mengikuti pemilik masing-masing area. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, diverifikasi langsung ke source) | `approved` | "MstCompanyGuarantorReimbursementRoute → Administrator/MasterData; MstCompanyGuarantorCoverageRule → HealthServices/MasterData" — dikonfirmasi cocok 3/3 file source |
| `MPY-DEC-009` | Decision | Menutup `MPY-OQ-003` PENUH, DENGAN DAMPAK DESAIN. Edit Billing TIDAK PERNAH menulis/mengubah status dispensing Pharmacy (`PhmDrugUsage`/`PhmDrugUsageItem`/`PrescriptionDispensingService` — DIKONFIRMASI LANGSUNG ada di [Areas/HealthServices/PharmacyManagement/](../../../Areas/HealthServices/PharmacyManagement/Models/PhmDrugUsage.cs), lengkap dengan histori penyerahan, `PrescriptionItemId` yang menautkan ke baris resep, quantity sisa dihitung dari penjumlahan seluruh baris penyerahan yang menunjuk baris resep sama, dan `DrugUsageStatus`). Source `PhmDrugUsage` bahkan eksplisit menyatakan batasnya sendiri pada doc-comment: *"Pencatatan ini berhenti sebagai transaksi yang **dapat** ditagihkan. Keputusan menagih beserta aturannya milik Billing."* Edit Billing HANYA menentukan financial inclusion (`INCLUDED`/`EXCLUDED`) atas prescription item yang SUDAH tercatat dispensing-nya oleh Pharmacy — TIDAK PERNAH membuat/mengubah baris `PhmDrugUsageItem`. Scope IGD WAJIB dipisah dari RAJAL untuk Edit Billing — lifecycle billing obat IGD yang sudah disetujui berbeda dari RAJAL; eligibility signal TIDAK BOLEH menyamakan keduanya di bawah satu bucket "outpatient" generik. **Ini MENGOREKSI** bullet "Sinyal eligibility RAJAL/IGD/OTC..." pada "Asumsi yang diwarisi dari dokumen PDF pengguna" di atas — RAJAL dan IGD TIDAK boleh disamakan. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, diverifikasi langsung ke source) | `approved` | "Pharmacy sudah authoritative dispensing... Edit Billing hanya financial inclusion... Scope IGD wajib dipisah dari rawat jalan" — dikonfirmasi cocok penuh dengan source, termasuk doc-comment eksplisit |

**Catatan teknis tambahan ditemukan saat verifikasi `MPY-DEC-009`** (bukan keputusan bisnis, MUST
dibawa ke `design-business-module`): `PhmDrugUsage` sudah punya kolom `BilledAt` (nullable
`DateTime`) — kemungkinan titik integrasi yang sudah disiapkan Pharmacy untuk menandai kapan satu
baris pemakaian sudah ditagih. Belum jelas apakah kolom ini sudah dipakai proses lain atau masih
dorman; MUST dicek sebelum Edit Billing dirancang supaya tidak membuat mekanisme paralel yang
bertentangan dengan kolom yang sudah ada.

**Status pass ini**: sembilan keputusan (`MPY-DEC-001`–`009`) terkunci.

- `MPY-OQ-002` dan `MPY-OQ-003` **CLOSED PENUH** — seluruh klaim teknis pendukungnya diverifikasi
  langsung ke source pass ini, bukan diterima dari kutipan.
- `MPY-OQ-001` **PARTIALLY CLOSED** — posisi arsitektur terkunci (`MPY-DEC-007`), tapi residual
  governance TETAP OPEN: Product/Domain Owner manusia untuk `RegistrationManagement` belum
  tercatat di `docs/module-blueprints/` manapun (tidak ada blueprint `registration`/
  `patient-management`). Kandidat kontak: Muhammad Hamzah, yang pernah menyetujui kontrak serupa
  milik `RegistrationManagement` (`RWI-ENC-PAYER-001`) lewat proses addendum modul `rawat-inap` —
  BELUM dikonfirmasi sebagai pemilik resmi `RegistrationManagement` secara umum. Residual ini
  TIDAK memblokir `design-business-module` (arsitekturnya sudah cukup jelas untuk didesain), tapi
  MEMBLOKIR implementasi endpoint sisi `RegistrationManagement` sampai owner diidentifikasi dan
  menyetujui.

**Langkah berikutnya (diperbarui)**: dengan `MPY-OQ-002`/`003` tertutup penuh berbukti langsung,
kebutuhan `trace-existing-capabilities` impact scan yang disebutkan di amendment sebelumnya
MENYEMPIT — cakupan yang tersisa hanya perlu memverifikasi ulang bagian `01-existing-capability-map.md`
yang belum disentuh pass ini (di luar area payer/guarantor/pharmacy yang sudah diaudit langsung).
Blocker satu-satunya sebelum implementasi (bukan sebelum desain) adalah residual `MPY-OQ-001`:
identifikasi Product/Domain Owner `RegistrationManagement`.

## Amendment 11 September 2026 (lanjutan 2) — Penutupan penuh `MPY-OQ-001`

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MPY-DEC-010` | Decision | Menutup `MPY-OQ-001` PENUH. Product/Domain Owner untuk `RegistrationManagement` dikonfirmasi: **Muhammad Hamzah** — orang yang sama dengan penyetuju `RWI-ENC-PAYER-001` (kontrak yang menjaga invariant satu payer aktif per encounter, jadi sudah pernah memutuskan tepat di titik singgung yang sama). Governance untuk membangun/mengorkestrasi service Edit Asuransi lewat `RegistrationManagement` (`MPY-DEC-007`) kini punya jalur approval yang jelas — bukan lagi kandidat, melainkan owner terkonfirmasi. | Product/Domain Owner billing-kasir (persetujuan eksplisit dalam percakapan) | `approved` | "iya betul muhammad hamzah" |

**Status pass ini**: SEMUA open question (`MPY-OQ-001`–`003`) CLOSED PENUH. Sepuluh keputusan
(`MPY-DEC-001`–`010`) mengunci: model payer, bentuk blueprint, cakupan Edit Asuransi, gate
coverage per item, model approval, permission invoice, write authority + orkestrasi lintas
modul, penempatan master data baru, boundary dengan Pharmacy, dan owner `RegistrationManagement`.
Tidak ada blocker desain tersisa untuk rumpun "Edit Tagihan & Multi-Payer Coverage". Blocker
implementasi yang tersisa (endpoint baru sisi `RegistrationManagement`) kini punya jalur approval
jelas ke Muhammad Hamzah, tapi approval itu sendiri belum diminta — dicatat sebagai prasyarat
`build-module-backend`, bukan blocker perencanaan/desain, mengikuti pola yang sama seperti
`PC-OQ-003` pada rumpun Petty Cash.

**Langkah berikutnya**: interview pass ini SELESAI. Siap lanjut `design-business-module` untuk
arsitektur backend/frontend penuh rumpun "Edit Tagihan & Multi-Payer Coverage", atas permintaan
eksplisit pengguna.

## Amendment 11 September 2026 (lanjutan 3) — Approval `MPY-DES-001`–`017` dan penutupan `MPY-OQ-004`

`design-business-module` (revisi blueprint `1.1`, draft) menghasilkan 17 keputusan arsitektur
(`MPY-DES-001`–`017`) beserta satu pertanyaan bertanda memblokir dengan cakupan terbatas pada
gelombang `MVP-17`: persetujuan pemilik `RegistrationManagement` atas pembangunan
`EncounterPaymentSourceService` di modulnya. Keduanya ditutup pada pass ini.

| ID | Tipe | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| `MPY-DEC-011` | Decision | Menutup `MPY-OQ-004`. Pemilik `RegistrationManagement` **menyetujui** pembangunan `EncounterPaymentSourceService` di modulnya sebagai satu-satunya jalur tulis `RegPatientEncounterGuarantor` dari konteks billing (`MPY-DES-004`), beserta bentuk yang dirancang: **satu layanan generik** yang menerima jenis payer apa pun (`MPY-DES-001`), memperbarui baris yang ada di tempat tanpa menyentuh index unik `EncounterId` (`MPY-DES-002`), dan ikut transaksi pemanggil tanpa membuka transaksinya sendiri. Dengan ini gelombang `MVP-17` TIDAK lagi terblokir. | Muhammad Hamzah (pemilik `RegistrationManagement`, `MPY-DEC-010`) | `approved` | "Muhammad Hamzah dah setuju" |
| `MPY-DEC-012` | Decision | Menyetujui `MPY-DES-001`–`MPY-DES-017` secara utuh — termasuk empat keputusan berbobot kebijakan yang ditonjolkan sebelum approval: (a) `MPY-DES-002`, ganti payer memperbarui baris di tempat karena index unik `EncounterId` tidak difilter, sehingga kontrak `RWI-ENC-PAYER-001` tidak tersentuh; (b) `MPY-DES-007`, adapter tanggungan berhenti mengeluarkan anomali `INSURANCE_PROVIDER_MISSING` untuk kunjungan berpenjamin perusahaan — ini MENGUBAH NILAI tagihan kunjungan semacam itu, dan memang itulah perbaikannya; (c) `MPY-DES-009`, ganti payer mereset penanggung baris yang jenisnya tidak lagi tersedia; (d) `MPY-DES-017`, tidak ada ember rupiah baru, yang ditambahkan penanda jenis payer. Status naik dari `draft` menjadi `approved` untuk revisi `1.1`. **Persetujuan ini BUKAN otorisasi membuat maupun menjalankan migration** — keduanya tetap memerlukan konfirmasi terpisah saat implementasi. | Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`) | `approved` | "Sayapun setuju" |

**Catatan provenance yang dicatat apa adanya.** Persetujuan `MPY-DEC-011` disampaikan kepada agent
**melalui Product/Domain Owner dalam percakapan**, bukan sebagai pernyataan langsung Muhammad
Hamzah pada sesi ini. Ini dicatat sebagai provenance, bukan disembunyikan — pola yang sama sudah
dipakai `BKC-DEC-062`/`BKC-DEC-071` untuk approval lintas-owner. Bila pemilik
`RegistrationManagement` kelak meminta bukti tertulis, butir ini perlu ditegaskan ulang lewat
jalur approval modul itu sendiri, bukan dianggap final selamanya atas dasar baris ini saja.

**Status pass ini**: SELURUH keputusan rumpun "Edit Tagihan & Multi-Payer Coverage" kini
`approved` — sepuluh keputusan bisnis (`MPY-DEC-001`–`010`), dua keputusan penutup
(`MPY-DEC-011`–`012`), dan tujuh belas keputusan arsitektur (`MPY-DES-001`–`017`). **Tidak ada
lagi pertanyaan bertanda memblokir.** Keempat gelombang `MVP-16`–`MVP-19` dapat diteruskan ke
`plan-module-delivery`.

**Yang TETAP terbuka, TIDAK memblokir perencanaan**: `MPY-OQ-005` (kolom penanda "sudah ditagih"
pada catatan penyerahan obat Farmasi — MUST dicek lewat pembacaan source sebelum `MVP-18`
dimulai); `MPY-OQ-006` (pengisian aturan tanggungan per perusahaan — memblokir aktivasi fitur,
bukan pembangunannya); `MPY-CQ-03` (koordinasi urutan commit dengan pekerjaan "Payment Reminder"
yang working tree-nya menyentuh tiga berkas yang sama).

**Langkah berikutnya**: `plan-module-delivery` untuk keempat gelombang, atas permintaan eksplisit
pengguna.

**Open question / dependency lintas modul — TIDAK memblokir wawancara, MUST diselesaikan sebelum
implementasi**:

- `MPY-OQ-001` — Edit Asuransi butuh menulis ke `TrxPatientEncounterGuarantor` (kolom payer
  aktif), tabel milik `RegistrationManagement`. `MPY-DEC-001` memastikan TIDAK ada perubahan
  skema/kontrak `RWI-ENC-PAYER-001`, tapi endpoint/service yang benar-benar melakukan penulisan
  (dipanggil dari konteks billing-kasir, atau disediakan `RegistrationManagement` untuk dipanggil
  billing-kasir) belum ditentukan. Perlu koordinasi dengan pemilik `RegistrationManagement`
  (kontak tercatat: Muhammad Hamzah) sebelum desain backend dikunci. Penjawab: pemilik arsitektur
  backend + pemilik `RegistrationManagement`.
- `MPY-OQ-002` — Nama entity master baru (`MstCompanyGuarantorReimbursementRoute`,
  `MstCompanyGuarantorCoverageRule`) memakai prefix `Mst`, tapi folder tempatnya
  (`Areas/HealthServices/MasterData/`) terdaftar milik `Master/Reference/MasterData`, bukan
  `billing-kasir` (`Bil`), di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Pembuatan entity baru di
  folder itu butuh sign-off pemilik `MasterData`, bukan otomatis wewenang `billing-kasir`.
  Penjawab: pemilik arsitektur backend.

**Langkah berikutnya**: lanjutkan wawancara pada pass yang sama untuk mengunci aturan bisnis
rinci (Edit Asuransi, Edit Status Tagihan, Edit Billing, coverage rule Company Guarantor, invoice
document).

## Amendment 15 September 2026 — Revisi Petty Cash: kasir cairkan langsung, anggaran per periode, satu halaman

Pengguna mengajukan paket dokumen eksternal ("Petty Cash Revisi — BRD, PRD, dan MVP Final",
15 September 2026: `00-Petty-Cash-BRD-PRD-MVP-Final.md`, `01-BRD-Petty-Cash-Revisi.md`,
`02-PRD-Petty-Cash-Revisi.md`, `03-MVP-Petty-Cash-Revisi.md`) yang merevisi rumpun Petty Cash.
Rumpun ini BUKAN modul baru — sudah `approved` penuh sejak 7 September 2026 (`PC-DEC-001`–`015`,
`PC-DES-001`–`014`), dengan backend (`BilPettyCashVoucher`, `BilPettyCashBudget`,
`BilPettyCashBudgetMovement`, `MstPettyCashCategory`, migration
`20260907062238_AddTablePettyCashModule`) dan frontend (`/petty-cash/budget`,
`/petty-cash/vouchers` — dua halaman terpisah, persis yang disebut dokumen baru sebagai
"rancangan sebelumnya") sudah berjalan. Amendment ini memakai prefix `PC-DEC-*` yang sama,
melanjutkan nomor dari `PC-DEC-015`, BUKAN rumpun/prefix baru.

Sebelum wawancara ini, ditemukan bahwa dokumen revisi — bila diikuti apa adanya sebagai
"revisi" — sebenarnya MEMBALIK tiga keputusan bisnis yang sudah `approved`, bukan sekadar
menambah. Ketiganya diklarifikasi eksplisit ke pemilik keputusan; hasilnya di bawah.

### Scope dan outcome

**Di dalam scope**: penghapusan gate approval Petty Cash (alur create-to-disburse langsung oleh
kasir); penggabungan halaman Anggaran + Monitoring/Voucher menjadi satu page; perubahan model
anggaran dari pool tunggal menjadi per periode beserta aturan penutupan periode; perilaku
Evidence/bukti menyusul; wewenang reversal/return; titik sentuh baru ke Accounting/subledger.

**Di luar scope — untuk modul lain**: `TrxExpenseClaim`/`TrxTravelAdvanceRequest`
(`Corporate/HumanResource`) — sudah ditutup pada audit 18.3 (7 September 2026), pola bisnis
berbeda (reimbursement/advance formal vs Petty Cash bayar-dulu-nota-menyusul non-formal), tidak
dibuka ulang. `BilCashierShift` (kas fisik shift kasir) — `PC-DEC-001` tetap berlaku penuh; Petty
Cash tetap TIDAK terhubung ke kas fisik shift kasir manapun, dokumen baru tidak memintanya
berubah. Mapping akun/COA final Accounting — dimiliki modul Accounting, bukan diputuskan di sini
(lihat `PC-OQ-004`).

### Decision Log

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `PC-DEC-016` | Decision | **Supersedes `PC-DEC-004`, `PC-DEC-013` (gate approval), dan bagian "penjaga dua lapis" pada `PC-DEC-015`.** Gate approval Kepala Kasir/Finance Operations DIHAPUS dari alur utama. Kasir/petugas administrasi dapat mencairkan Petty Cash langsung tanpa persetujuan terpisah. Kontrol bergeser dari cegah-sebelum-fakta (approval dua lapis) menjadi audit-setelah-fakta (ledger immutable + actor + reason), pola yang sama dengan `MPY-DEC-005` (Edit Tagihan). Status `WAITING_APPROVAL`/`APPROVED`/`REJECTED` tidak lagi menjadi bagian alur utama MVP; kosakata status baru mengikuti dokumen revisi: `REQUESTED` → `DISBURSED_PENDING_EVIDENCE` → `EVIDENCE_SUBMITTED` → `SETTLED`, atau `CANCELLED` (sebelum pencairan). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Hapus approval, kasir cairkan langsung" dari 3 opsi (opsi rekomendasi mempertahankan approval TIDAK dipilih) | 15 September 2026 |
| `PC-DEC-017` | Decision | **Supersedes `PC-DEC-010` (pool tunggal).** Anggaran Petty Cash beralih dari satu pool statis (`HOSPITAL_MAIN`, tanpa periode) menjadi anggaran per periode yang dibuat Finance (`PeriodStart`/`PeriodEnd`/`BudgetAmount`/Status `Draft`/`Active`/`Closed`), sesuai dokumen revisi. Migrasi data pool `HOSPITAL_MAIN` existing ke periode pertama menjadi keputusan desain/build, dicatat sebagai `PC-OQ-005`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ganti ke anggaran per periode (ikuti dokumen)" dari 3 opsi bertanda rekomendasi lain | 15 September 2026 |
| `PC-DEC-018` | Decision | Saat Finance menutup periode anggaran (status `Closed`), sisa `CurrentBalance` yang belum terpakai OTOMATIS dibawa ke periode berikutnya sebagai saldo awal (tercatat sebagai movement `CARRY_FORWARD` pada ledger) — BUKAN wajib dikembalikan/di-nol-kan manual dulu. Konsisten dengan `PC-DEC-002` (saldo berjalan): ini uang kas fisik sungguhan, bukan alokasi yang hangus per periode administratif. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Sisa saldo otomatis dibawa ke periode berikutnya" dari 3 opsi bertanda rekomendasi | 15 September 2026 |
| `PC-DEC-019` | Decision | **Menegaskan kembali `PC-DEC-011`, TIDAK disupersede.** "Requester" pada dokumen revisi bukan aktor self-service baru — tetap kasir/petugas yang mengeksekusi pemberian uang; penerima ("bisa siapa saja") dan keperluannya tetap dicatat sebagai teks bebas tanpa akun/login terpisah. Tidak ada perubahan model aktor dari yang sudah berjalan. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Jadi yang memberi uang petty cashnya itu di petugas kasir. tpi penberimanya itu bisa siapa saja. Tpi nama penerima dan keperluannya tetap dicatat" | 15 September 2026 |
| `PC-DEC-020` | Decision | **Menegaskan kembali `PC-DEC-012`, TIDAK disupersede.** Field Kategori (`MstPettyCashCategory`) tetap dipertahankan sebagai field WAJIB pada request/voucher, berdampingan dengan `Purpose` (teks bebas). Dokumen revisi tidak menyebut Kategori sama sekali, dinilai sebagai kealpaan penulisan dokumen, bukan keputusan sengaja menghapusnya — pelaporan Finance per kategori tetap dibutuhkan. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tetap dipertahankan sebagai field wajib" dari 3 opsi bertanda rekomendasi | 15 September 2026 |
| `PC-DEC-021` | Decision | Evidence/bukti menyusul TETAP berbentuk field teks referensi (pola `ProofReferenceNumber` existing), BUKAN entity Evidence baru dengan upload file/foto nota seperti diusulkan dokumen revisi (PRD §7.4). Perilakunya berubah — tidak lagi memblokir pencairan (lihat `PC-DEC-016`) — tetapi bentuk datanya tidak berubah. Tidak ada kebutuhan integrasi storage service baru pada revisi ini. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tetap teks/nomor referensi saja, tanpa upload file" dari 3 opsi (opsi rekomendasi upload file TIDAK dipilih) | 15 September 2026 |
| `PC-DEC-022` | Decision | Transaksi `RETURN`/`REVERSAL` (koreksi transaksi yang sudah cair) TIDAK dibatasi ke role Finance/Supervisor seperti disebut dokumen revisi (PRD §6 Permission Matrix) — kasir biasa dengan akses Petty Cash yang sama juga berwenang, konsisten dengan semangat `PC-DEC-016` (satu lapis kontrol, audit-setelah-fakta). Kontrol tetap ditegakkan lewat ledger immutable + reason wajib + actor tercatat, bukan lewat pembatasan role terpisah. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Kasir biasa juga boleh (konsisten dgn tanpa-approval)" dari 3 opsi bertanda rekomendasi | 15 September 2026 |

### Assumption tercatat (bukan ditanyakan langsung, konsekuensi logis dari `PC-DEC-019`/`PC-DEC-022`)

- Karena tidak ada aktor "Requester" terpisah dengan login sendiri (`PC-DEC-019`), pembatalan
  request sebelum pencairan (status `CANCELLED`, mengganti semangat `PC-DEC-007` yang dulu
  terikat status "Menunggu Persetujuan") tersedia untuk kasir/petugas mana pun yang memiliki
  akses Petty Cash — bukan dibatasi ke "kasir yang membuat request itu saja". Konsisten dengan
  `PC-DEC-022` (satu lapis kontrol operasional, bukan silo akuntabilitas individual). Bila asumsi
  ini salah, tandai sebagai koreksi sebelum `design-business-module` mengunci permission matrix.

### Status yang menjadi tidak berlaku (bukan dihapus, catatan historis)

- `PC-DEC-003` (voucher `REJECTED` tidak bisa diedit/diajukan ulang), `PC-DEC-004` (approval satu
  jenjang Kepala Kasir/Finance Operations), `PC-DEC-008` (validasi saldo saat approval), dan
  bagian `PC-DEC-013` yang menyebut status `Menunggu Persetujuan`/`Disetujui`/`Ditolak` menjadi
  TIDAK BERLAKU untuk alur utama MVP setelah `PC-DEC-016` — status `REJECTED`/`APPROVED` tidak
  lagi bagian kosakata alur utama. Baris-baris ini TIDAK dihapus dari log (jejak historis kenapa
  approval pernah dirancang), dan validasi saldo pada `PC-DEC-008` bergeser sepenuhnya ke titik
  pencairan (`PC-DEC-009` tetap berlaku: saldo berkurang persis saat "Uang Diterima"/pencairan).

### Open Question / dependency lintas modul — TIDAK memblokir wawancara, MUST diselesaikan sebelum implementasi

- `PC-OQ-004` — Integrasi Accounting/subledger untuk Petty Cash (BR-PC-016, PRD §9) adalah titik
  sentuh BARU — nol referensi Journal/Posting/Subledger ditemukan pada source Petty Cash saat ini
  (diverifikasi lewat pencarian source, bukan asumsi). Mapping COA/akun kontrol final harus
  ditentukan pemilik modul Accounting, bukan dikarang di sini. Penjawab: pemilik arsitektur
  backend + pemilik modul Accounting.
- `PC-OQ-005` — Migrasi data pool anggaran `HOSPITAL_MAIN` existing (satu baris `BilPettyCashBudget`
  aktif dengan saldo berjalan) ke model anggaran per periode (`PC-DEC-017`) adalah keputusan
  desain/build: apakah pool existing menjadi "periode pertama" otomatis, atau memerlukan langkah
  migrasi eksplisit. Bukan blocker wawancara — dilempar ke `design-business-module`. Penjawab:
  pemilik arsitektur backend.
- `PC-OQ-006` — Taksonomi movement type `RETURN` vs `ADJUSTMENT` (existing hanya punya
  `ADJUSTMENT`; dokumen revisi minta `RETURN` terpisah) adalah keputusan penamaan/desain, bukan
  keputusan bisnis — apakah `RETURN` menjadi `MovementType` baru atau sub-kategori `ADJUSTMENT`
  dilempar ke `design-business-module`.

**Status pass ini**: `PC-DEC-016`–`022` (7 keputusan) MENGUNCI seluruh perubahan bisnis rumpun
Petty Cash pada revisi ini. Tiga keputusan lama yang berpotensi konflik (`PC-DEC-004`,
`PC-DEC-010`, `PC-DEC-011`) sudah diklarifikasi eksplisit — dua disupersede (`PC-DEC-016`,
`PC-DEC-017`), satu ditegaskan tetap berlaku (`PC-DEC-019`, juga menegaskan `PC-DEC-012` lewat
`PC-DEC-020`). Tidak ada open question bisnis yang memblokir desain; `PC-OQ-004`–`006` adalah
dependency arsitektur/lintas-modul yang MUST diselesaikan sebelum implementasi, bukan sebelum
desain.

### Penutupan `PC-CQ-03` dan `PC-CQ-04` (15 September 2026, setelah impact scan section 20)

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `PC-DEC-023` | Decision | **Menutup `PC-CQ-03`/`PC-OQ-004`.** Integrasi posting ke Accounting DITUNDA ke rilis berikutnya, TIDAK masuk MVP revisi ini. `BilPettyCashBudgetMovement` (ledger Petty Cash sendiri) diperlakukan sebagai subledger yang memadai untuk MVP; tidak ada pemanggilan `AccJournalService` dari Petty Cash. Alasan: memakai `AccJournalService` apa adanya akan memasukkan kembali approval manual (siklus `Draft`→`Submit`→`Approve`→`Post`) yang baru saja dihapus dari Petty Cash lewat `PC-DEC-016` — kontradiksi langsung dengan BR-PC-016, dan jalur posting otomatis adalah perubahan pada modul Accounting yang butuh sign-off pemiliknya sendiri, bukan wewenang `billing-kasir`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tunda ke rilis berikutnya" dari 3 opsi bertanda rekomendasi | 15 September 2026 |
| `PC-DEC-024` | Decision | **Menutup `PC-CQ-04`.** Action dan permission `PettyCashVoucher.Approve` serta `PettyCashVoucher.Reject` DIHAPUS dari `PettyCashVouchersController` beserta registry permission-nya — bukan dibiarkan dormant. Alasan: permission yang tidak lagi punya alur aktif berisiko membingungkan admin pengelola role. Prasyarat implementasi: cek lebih dulu apakah ada role yang HANYA berisi kedua permission ini sebelum penghapusan, supaya tidak meninggalkan role yatim tanpa kemampuan apa pun. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Hapus dari registry permission" dari 2 opsi bertanda rekomendasi | 15 September 2026 |

| `PC-DEC-025` | Decision | **Mempersempit kosakata status pada `PC-DEC-016`.** Dua status terpisah `EVIDENCE_SUBMITTED` dan `SETTLED` yang disebut dokumen revisi DIGABUNG menjadi SATU status terminal `COMPLETED` (label `Selesai`) — begitu nomor nota diinput, voucher langsung selesai. Alasan: dokumen revisi sendiri menulis `SETTLED` sebagai kondisional ("jika proses pertanggungjawaban final digunakan") dan tidak pernah mendefinisikan peristiwa pemicunya; setelah `PC-DEC-021` menetapkan bukti hanya berupa nomor referensi teks (tanpa nominal nota, tanpa verifikasi, tanpa approval bukti), tidak ada peristiwa apa pun yang dapat memindahkan voucher dari status pertama ke status kedua — ia akan menjadi status mati yang tidak pernah tercapai. Ini juga mempertahankan perilaku dan label yang sudah berjalan (`PC-DES-012`). | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Satu status akhir saja: Selesai" dari 3 opsi bertanda rekomendasi | 15 September 2026 |

| `PC-DEC-026` | Decision | **Menyetujui `PC-DES-015`–`PC-DES-025` secara utuh** — kosakata status baru beserta perlakuan berbeda untuk ketiga jenis kode lama (`PC-DES-015`), pencabutan mekanisme komitmen `ReservedAmount` (`PC-DES-016`), perubahan `BilPettyCashBudget` menjadi baris periode (`PC-DES-017`), carry-forward dua baris ledger (`PC-DES-018`), empat nilai `MovementType` baru (`PC-DES-019`), pemisahan perlakuan `RETURN` dan `REVERSAL` (`PC-DES-020`), penghapusan dan penambahan butir hak akses (`PC-DES-021`), dua endpoint terpisah untuk buat dan cairkan (`PC-DES-022`), ketiadaan sambungan Accounting yang disengaja (`PC-DES-023`), pemutakhiran data di dalam migration (`PC-DES-024`), dan satu halaman kanonik (`PC-DES-025`). Dengan ini keempat keputusan lama yang digantikan resmi berstatus `superseded`/dipersempit: `PC-DES-003`, `PC-DES-005`, `PC-DES-013`, `PC-DES-014`. Status revisi `1.2` naik dari `draft` menjadi `approved`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan: "Saya menyetujui PC-DES-015–025") | `approved` | Konfirmasi eksplisit setelah desain lengkap disajikan | 15 September 2026 |

> **Approval ini BUKAN otorisasi membuat maupun menjalankan migration.** Migration
> `RevisePettyCashDirectDisbursementAndBudgetPeriod` memuat pemutakhiran data status yang tidak
> dapat dimundurkan secara sempurna (`PC-DES-015`, langkah 6). Pembuatan dan eksekusinya tetap
> menuntut konfirmasi eksplisit tersendiri saat implementasi, sesuai `AGENTS.md` bagian Aturan
> Entity Framework dan Akses Data.
>
> **Approval ini juga BUKAN otorisasi menghapus butir hak akses.** `PC-OQ-007` (pemeriksaan
> peran yang hanya memegang `Approve`/`Reject`) MUST diselesaikan lebih dulu, sesuai
> `PC-DEC-024` sendiri.

**Status setelah penutupan ini**: seluruh closure question rumpun Petty Cash TERTUTUP
(`PC-OQ-004`/`PC-CQ-03` lewat `PC-DEC-023`; `PC-CQ-04` lewat `PC-DEC-024`). Yang tersisa murni
keputusan desain teknis yang memang menjadi wewenang `design-business-module`: `PC-OQ-005`
(migrasi pool `HOSPITAL_MAIN` ke periode pertama) dan `PC-OQ-006` (taksonomi `RETURN` vs
`ADJUSTMENT`). Tidak ada lagi keputusan bisnis yang memblokir desain.

**Langkah berikutnya**: capability map (`01-existing-capability-map.md` section 18) sudah
mencakup rumpun Petty Cash existing secara menyeluruh dan SHA yang tercatat di sana (backend
`dd31bc91...`) kemungkinan sudah basi setelah pekerjaan sesi-sesi berikutnya (Edit Tagihan/MPY,
perbaikan migration, dll). Sebelum `design-business-module` mengunci arsitektur revisi ini,
jalankan `trace-existing-capabilities` mode impact scan untuk memastikan tidak ada perubahan
lain pada `BilPettyCashVoucher`/`BilPettyCashBudget`/`BilPettyCashBudgetMovement` sejak SHA
tersebut yang belum tercatat.

### Penutupan `PC-OQ-008` (15 September 2026, amendment pass — dipicu blocker implementasi `BE-BKC-059`)

**Konteks.** `PC-OQ-008` pertama kali muncul di `04-prd-to-mvp.md` (bagian blocking question)
dan `blueprint-manifest.md` (prasyarat implementasi revisi `1.2`), bukan lahir dari pass
wawancara ini — sehingga belum pernah punya baris di Decision Log ini. Pertanyaannya: siapa yang
membuat periode anggaran pertama setelah rilis, kapan, dan berapa plafonnya. Ini memblokir
**aktivasi** task `BE-BKC-059` (murni pengisian data lewat layar, `PC-DES-024`/`PC-OQ-005`
menetapkan `BE-BKC-053` sudah memindahkan kolam warisan `HOSPITAL_MAIN` menjadi periode pertama
dengan plafon **turunan** dari `TotalTopUpAmount` historis — bukan angka yang sengaja diputuskan
Finance), bukan pembangunan source-nya.

Amendment pass ini dipicu Product/Domain Owner secara langsung dalam percakapan implementasi
`BE-BKC-059`, di luar sesi wawancara utama revisi `1.2`.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `PC-DEC-027` | Decision | **Menutup `PC-OQ-008`.** Tidak ada aktor khusus, tanggal, maupun plafon yang dipatok di depan untuk periode anggaran pertama. Finance membuat dan mengaktifkan periode anggaran riilnya sendiri kapan pun setelah rilis, memakai endpoint self-service yang sudah ada (`POST /budget/periods`, `POST /budget/periods/{id}/activate`, dibangun `BE-BKC-054`) — bukan proses/aktor khusus di luar alur normal. Selama Finance belum bertindak, sistem **tetap berjalan** memakai periode warisan hasil migrasi (`PC-OQ-005`) dengan plafon turunannya; **tidak ada tenggat wajib** dan **tidak ada mekanisme pemblokiran otomatis** (`BIL-VAL-106` tidak ikut disentuh) maupun pengingat sistem yang menandai periode itu sebagai "masih warisan migrasi". Konsekuensi yang disadari dan diterima: plafon turunan bisa terpakai dalam jangka waktu berapa pun bila Finance menunda, tanpa ada peringatan otomatis apa pun. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Tidak ada tenggat wajib" dari 3 opsi bertanda rekomendasi (opsi pengingat dan opsi tenggat keras/pemblokiran TIDAK dipilih) | 15 September 2026 |

**Dampak ke task backend.** `BE-BKC-059` (roadmap `backend-roadmap.md`) tetap `⛔` — keputusan ini
menjawab **kebijakannya**, tetapi belum ada **eksekusi nyata**: belum ada satu pun periode
dibuat/diaktifkan di database manapun dengan plafon riil Finance, dan task itu sendiri menuntut
verifikasi manual database dengan wewenang eksplisit terpisah (`AGENTS.md` bagian Keselamatan
Database) sebelum boleh ditandai selesai. `PC-DEC-027` menghapus **blocker keputusan bisnisnya**,
bukan langkah eksekusinya.

**Tidak ada keputusan arsitektur (`PC-DES`) baru yang lahir dari penutupan ini** — opsi yang
dipilih sengaja TIDAK menambah field, job terjadwal, maupun logika baru pada
`BilPettyCashBudget`; seluruh mekanisme yang dibutuhkan (endpoint create/activate periode, gerbang
`BIL-VAL-106`) sudah ada dari `BE-BKC-054`.

### Amendment lanjutan 16 September 2026 — Penghapusan kolom `TaxableCategory` pada `MstTaxRule`

**Konteks.** User (module owner) bertanya makna kolom `TaxableCategory` pada `MstTaxRule` di luar
sesi wawancara utama. Audit read-only membuktikan kolom ini sudah tidak lagi punya konsekuensi
kalkulasi pajak sejak `BKC-DEC-078`/`079` mengubah basis gerbang PPN menjadi
`item.IsPharmacy` + `BilInvoice.ServiceType` (rawat jalan vs rawat inap) — bukan lagi kategori pada
tax rule (komentar eksplisit `BillingCalculationService.cs:790-793`: "Isi TaxableCategory kini
murni label bagi pengguna dan tidak memengaruhi perhitungan sama sekali"). Satu-satunya pemakaian
aktif yang tersisa: (1) label/filter UI, (2) overlap-check periode — dua tax rule aktif dengan
`TaxableCategory` sama tidak boleh periodenya tumpang tindih (`TaxRuleService.cs` `ValidateAsync`
baris 249-252, `ActivateAsync` baris 176-179). Audit lanjutan tidak menemukan satu pun tempat lain
(laporan, snapshot histori, `TaxCalculationResponse`, dsb.) yang menyimpan atau membaca nilai
`TaxableCategory` — hanya komentar kode yang menyebut namanya, bukan pemakaian nilai.

**Scope pass ini**: Amendment terhadap blueprint yang sudah `APPROVED` (revisi `0.8`). Di dalam
scope: keputusan hapus/pertahankan kolom `TaxableCategory` pada `MstTaxRule` (schema, DTO, service,
frontend form/tampilan) dan pengganti overlap-check periode. Di luar scope — tidak disentuh: gerbang
PPN rawat jalan/rawat inap (`BKC-DEC-078`/`079`) dan alokasi PPN `PROPORTIONAL` (`BKC-DEC-077`).

Pertanyaan pertama sempat dijawab "hapus permanen lewat migration" lalu DIREVISI Owner sendiri pada
giliran berikutnya sebelum dikunci — dicatat apa adanya sebagai provenance keputusan, bukan
disembunyikan.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `BKC-DEC-098` | Decision | **Kolom `TaxableCategory` DIHAPUS TOTAL dari entity model (`MstTaxRule.cs`), DTO (`TaxRuleDtos.cs`: `TaxRuleQuery`, `CreateTaxRuleRequest`/`UpdateTaxRuleRequest`, `TaxRuleResponse`, `TaxRuleOptionResponse`, `TaxRuleDefaultFilterResponse`, `TaxRuleFilterMetadataResponse.TaxableCategories`), service (`TaxRuleService.cs`, termasuk filter-by-category dan `GetFilterMetadataAsync`), dan frontend (form create/update, tampilan detail/list, filter) — field tidak lagi muncul di CRUD sama sekali, request/response API juga tidak lagi membawanya. Kolom fisik di database database TETAP DIPERTAHANKAN sebagai orphan (tidak dimapping `MstTaxRuleConfiguration`, tidak dibaca/ditulis EF Core sama sekali) — **BUKAN** drop column via migration. Jawaban pertama Owner ("hapus permanen lewat migration") DIREVISI menjadi ini pada giliran berikutnya di sesi yang sama, dengan alasan: menghindari kebutuhan otorisasi migration terpisah sekarang dan tetap reversibel (kolom fisik masih ada bila suatu saat ingin dipakai ulang). | Product/Domain Owner (persetujuan eksplisit dalam percakapan, dengan revisi) | `approved` | Revisi eksplisit: opsi B ("Pertahankan kolom di database, tapi lepas dari model/DTO/validasi/UI... kolom jadi orphan") dari 2 opsi bertanda rekomendasi, lalu diperjelas lanjutan: "option A" untuk klarifikasi field dihapus total dari form/tampilan (bukan cuma jadi opsional) | 16 September 2026 |
| `BKC-DEC-099` | Decision | **Overlap-check periode tax rule yang sebelumnya per-`TaxableCategory`** (dua tax rule aktif tidak boleh periodenya tumpang tindih untuk kategori yang sama) **diganti jadi overlap-check GLOBAL** — dua tax rule aktif tidak boleh periodenya tumpang tindih sama sekali, tanpa dibedakan kategori apa pun. Berlaku di `TaxRuleService.ValidateAsync` (baris 249-252) dan `ActivateAsync` (baris 176-179). Alasan: sistem sudah membatasi hanya SATU tax rule aktif secara global sepanjang waktu (`LoadInvoiceTaxRuleAsync` di `BillingCalculationService.cs` melempar exception bila lebih dari satu aktif), jadi overlap-check global ini hanya menyamakan validasi dini saat create/update/activate dengan aturan yang sudah berlaku saat kalkulasi tagihan berjalan — bukan aturan bisnis baru. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "pilihan A" dari 2 opsi bertanda rekomendasi | 16 September 2026 |

**Catatan konsekuensi teknis (bukan keputusan baru, turunan `BKC-DEC-098`)**: karena kolom fisik
dipertahankan (bukan drop), tidak ada kebutuhan migration EF Core untuk pass ini — otorisasi
migration terpisah sesuai `AGENTS.md` bagian Aturan Entity Framework dan Akses Data TIDAK
diperlukan untuk keputusan ini. Index gabungan existing `(TaxableCategory, EffectiveFrom,
EffectiveTo, IsActive, IsDelete)` pada `MstTaxRuleConfiguration.cs:35` ikut menjadi pertimbangan
desain (apakah dipertahankan apa adanya karena kolom underlying masih fisik ada, atau diganti index
tanpa `TaxableCategory` mengikuti overlap-check global `BKC-DEC-099`) — ini keputusan arsitektur
teknis, dilempar ke `/design-business-module` atau langsung ke `/plan-module-delivery`+
`/build-module-backend` mengingat cakupannya sudah sangat sempit dan tidak ada open question bisnis
lain yang tersisa.

**Status**: Tidak ada open question bisnis yang tersisa untuk topik ini. `BKC-DEC-098`–`099`
MENGUNCI seluruh keputusan bisnis penghapusan kolom `TaxableCategory`.

## Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED` (invoice lunas macet permanen)

**Konteks.** Pengguna (Product/Domain Owner) melaporkan invoice yang sudah dibayar lunas dan
jumlahnya sudah sesuai tetap menampilkan status `FINAL`, bukan status yang berarti lunas.
Audit read-only membuktikan ini bukan bug tampilan, melainkan gap yang sudah pernah didokumentasikan
tapi belum pernah ditutup:

- Kontrak `BIL-STATE-0.4` (`state-transition-matrix.md` baris 12) mensyaratkan transisi
  `FINAL` → `CLOSED` terjadi setelah **"AR/AP posting sukses"**.
- `BillingArApHandoffService.cs` (doc-comment kelas, baris 12–15) menyatakan eksplisit: *"belum
  ada konsumen AR/AP nyata di repository ini, sehingga tidak ada pengiriman aktif yang dibangun."*
  Peristiwa "AR/AP posting sukses" karena itu **tidak pernah terjadi** di sistem ini.
- Grep menyeluruh atas `BillingInvoiceStatuses.Closed` di seluruh backend hanya menemukan SATU
  titik yang MEMBACA nilai ini (`BillingFinancialExceptionService.cs:796`); **tidak ada satu pun**
  titik yang MENULISNYA. `BilInvoice.ClosedAt` juga tidak pernah di-set di manapun.
- Ini sudah tercatat sebagai temuan lintas modul sebelumnya:
  `docs/module-blueprints/laboratorium/approval-requests/2026-09-02-temuan-billing-final-closed.md`
  (status `terbuka — menunggu keputusan pemilik Billing`). "Pilihan A" dokumen itu (finalisasi
  selalu `FINAL`) sudah diadopsi di `BillingFinalizationService.cs:125-127`
  (komentar source: *"Kontrak BIL-STATE-0.4: finalisasi selalu menghasilkan FINAL. CLOSED hanya
  terjadi setelah AR/AP posting sukses."*), tetapi bagian kedua — jalur nyata yang memindahkan
  `FINAL` → `CLOSED` — memang belum pernah dibangun, persis risiko yang diperingatkan dokumen
  temuan itu sendiri di bagian "Pilihan A": *"Bila belum ada, invoice lunas akan berhenti di
  FINAL selamanya."*

**Scope pass ini**: amendment atas rumpun status invoice inti (bukan Petty Cash, bukan Edit
Tagihan/Multi-Payer). Di dalam scope: syarat baru transisi `FINAL`→`CLOSED`, kebijakan invoice
lama yang sudah lunas, perlakuan guard `RecordCorrectionIfLinkedAsync`, dan perlakuan invoice
departure exception. Di luar scope — tidak disentuh: integrasi AR/AP nyata (`BKC-BLK-INT-001`),
integrasi payment provider (`BKC-BLK-PROV-001`), mesin kalkulasi tagihan, dan jalur
`SETTLED_BY_WRITE_OFF` yang sudah punya aturannya sendiri (`BKC-DEC-036`).

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `BKC-DEC-100` | Decision | **Mengganti syarat transisi `FINAL`→`CLOSED` pada `BIL-STATE-0.4`.** Syarat lama "AR/AP posting sukses" (yang tidak pernah dan tidak bisa terjadi — tidak ada konsumen AR/AP nyata) DIGANTI menjadi **"outstanding invoice mencapai 0 dari akumulasi tender berstatus `SUCCEEDED`"**. Pelaku transisi TETAP Sistem, tidak berubah dari kontrak lama. Invoice `FINAL` yang SEKARANG di database sudah lunas penuh ikut di-backfill ke `CLOSED` lewat satu migration data terpisah — BUKAN dibiarkan macet di `FINAL` menunggu pelunasan baru. Backfill ini murni kebijakan pass ini; PEMBUATAN dan EKSEKUSI migration-nya tetap menuntut otorisasi eksplisit terpisah sesuai `AGENTS.md` bagian Aturan Entity Framework dan Akses Data serta Keselamatan Database. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Backfill: ikut dipindah ke CLOSED" dari 2 opsi bertanda rekomendasi | 18 September 2026 |
| `BKC-DEC-101` | Decision | **Memperluas guard `RecordCorrectionIfLinkedAsync`** (`BillingArApHandoffService.cs:150`) supaya menerima invoice berstatus `FINAL` **maupun** `CLOSED` — bukan hanya `FINAL` seperti sekarang. Alasan: begitu `BKC-DEC-100` membuat invoice lunas otomatis pindah ke `CLOSED`, adjustment/write-off yang diposting SETELAH invoice lunas (kasus paling umum — kebanyakan koreksi ketahuan setelah pasien sudah bayar, persis contoh Tn. Budi pada dokumen temuan 2 September 2026) akan kembali gagal tercatat sebagai koreksi AR bila guard tetap hanya menerima `FINAL`. Ini menutup lubang koreksi AR yang menjadi alasan utama temuan itu ditulis, bukan sekadar memindahkannya. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, guard menerima FINAL maupun CLOSED" dari 2 opsi bertanda rekomendasi | 18 September 2026 |
| `BKC-DEC-102` | Decision | **Invoice departure exception TIDAK dikecualikan dari `BKC-DEC-100`.** Invoice yang difinalisasi lewat jalur departure exception (`isDepartureException = true`, `BillingFinalizationService.cs:55`) sengaja tetap `FINAL` saat outstanding-nya 0 pada momen finalisasi, karena masih ada piutang penjamin (`BilArHandoff` `DebtorType = PatientGuarantor`) yang ditagihkan belakangan — itu TIDAK berubah. Tapi begitu piutang itu KELAK benar-benar tertagih dan outstanding invoice ini mencapai 0 di kemudian hari, invoice ini ikut aturan `BKC-DEC-100` yang sama dan boleh otomatis pindah ke `CLOSED` — tidak ada perlakuan berbeda berdasarkan riwayat departure exception-nya. **Konsekuensi desain yang MUST dibawa ke `design-business-module`**: `BilArHandoff` milik invoice itu (`DebtorType = PatientGuarantor`) MUST ikut ditandai selesai/collected pada transaksi yang sama saat invoice pindah ke `CLOSED`, supaya tidak ada piutang yang tercatat closed di invoice tapi masih open di catatan AR handoff. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Ya, ikut aturan yang sama begitu benar-benar lunas" dari 2 opsi bertanda rekomendasi | 18 September 2026 |

**Open question — MUST diselesaikan sebelum implementasi backfill, TIDAK memblokir desain**:

- `BKC-OQ-100` — Dokumen temuan 2 September 2026 sudah mengangkat pertanyaan yang belum terjawab:
  *"berapa banyak adjustment dan write-off yang sudah diposting atas invoice berstatus `CLOSED`
  sejak perilaku ini berlaku."* Ini menentukan apakah backfill `BKC-DEC-100` butuh langkah koreksi
  AR susulan (bukan cuma pemindahan status) untuk invoice yang sudah kadung di-adjust/write-off
  saat masih `CLOSED` tanpa koreksi AR tercatat. Penjawab: pemilik modul Billing/Finance, lewat
  query rekonsiliasi read-only terhadap `BilHandoffAdjustment` vs `BilAdjustment`/`BilWriteOffCase`
  pada invoice yang terkena backfill.

**Status pass ini**: tiga keputusan (`BKC-DEC-100`–`102`) MENGUNCI seluruh keputusan bisnis untuk
gap `FINAL`→`CLOSED`. Tidak ada open question bisnis yang memblokir desain; `BKC-OQ-100` adalah
pekerjaan rekonsiliasi data yang MUST diselesaikan sebelum backfill dieksekusi, bukan sebelum
desain.

**Langkah berikutnya**: `01-existing-capability-map.md` blueprint ini terakhir diaudit pada SHA
`0ca85ba4`/`1f2f2c93c` (lihat `blueprint-manifest.md`); HEAD backend saat ini `21b47331`, sudah
bergerak. Sebelum `design-business-module` mengunci arsitektur perubahan ini (titik kode tepat
untuk recompute outstanding, penanganan idempotency bila tender di-reversal setelah invoice
sempat `CLOSED`, dan detail migration backfill), jalankan `trace-existing-capabilities` mode
impact scan untuk memverifikasi tidak ada perubahan lain pada jalur settlement/finalisasi sejak
SHA tersebut.

### Penutupan `BKC-CQ-01` dan penegasan cakupan penyelarasan (18 September 2026, sesudah pass desain)

**Konteks.** Impact scan (`01-existing-capability-map.md` § 21) dan pass desain
(`02-backend-architecture.md` amendment 18 September 2026) sama-sama menemukan bahwa dua keputusan
bisnis di atas tidak dapat diterjemahkan apa adanya menjadi arsitektur tanpa satu keputusan
tambahan. Keduanya diangkat eksplisit kepada Owner, **bukan** diputuskan sendiri oleh pass desain,
karena keduanya menyimpang dari bunyi harfiah keputusan yang sudah `approved`.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `BKC-DEC-103` | Decision | **Menutup `BKC-CQ-01` PENUH, dengan MEMPERSEMPIT konsekuensi `BKC-DEC-102`.** `BilArHandoff` **tidak disentuh sama sekali**: tidak ada nilai status `COLLECTED` baru, tidak ada kolom `CollectedAt` baru, tidak ada migration skema. Dasarnya dua fakta source yang diverifikasi langsung: (1) `BillingHandoffStatuses` (`BilArHandoff.cs:35-39`) hanya mengenal `CREATED` dan `ACKNOWLEDGED`, dan keduanya menggambarkan **penyerahan fakta ke AR** — bukan tertagihnya piutang; (2) satu-satunya penulisan status itu di seluruh source adalah `Status = Created` saat baris handoff dibuat. Konsekuensi `BKC-DEC-102` ("tidak boleh ada piutang yang closed di invoice tapi masih open di catatan AR") dipenuhi dengan cara berbeda: catatan AR memang tidak pernah mengklaim "masih berjalan" sejak awal. Sumber kebenaran "tagihan ini lunas" adalah `BilInvoice.Status`/`ClosedAt`. Sumbu status penagihan piutang dirancang bersama pemilik konsumen AR/AP ketika konsumen itu benar-benar dibangun (`BKC-BLK-INT-001`), **MUST NOT** ditebak sekarang. Dengan ini `BKC-DES-033` berstatus `approved`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, sesudah ketiga opsi beserta konsekuensinya disajikan) | `approved` | "Terima BKC-DES-033: BilArHandoff tidak disentuh (Direkomendasikan)" dari 3 opsi bertanda rekomendasi — opsi status `COLLECTED` dan opsi kolom `CollectedAt` TIDAK dipilih | 18 September 2026 |
| `BKC-DEC-104` | Decision | **MEMPERLUAS cakupan harfiah `BKC-DEC-100`.** Penyelarasan status dipasang pada **enam** peristiwa yang benar-benar menggerakkan sisa tagihan pasien — pembayaran, pembalikan pembayaran, alokasi deposit, penyesuaian arah `Credit`, penyesuaian arah `Debit`, serta write-off beserta kedua jalur pembalikannya — **bukan** hanya pada tender `SUCCEEDED` yang disebut teks `BKC-DEC-100`. Alasan yang diterima Owner: memasang penyelarasan hanya di jalur tender akan meninggalkan lubang yang **jenisnya sama persis** dengan gap yang sedang ditutup — tagihan yang dilunasi seluruhnya dari deposit pasien, atau yang sisa tagihannya dinolkan penyesuaian `Credit`, akan tetap macet di `FINAL` tanpa batas waktu beserta lubang koreksi AR-nya. Perluasan ini melayani **maksud** `BKC-DEC-100`, bukan menggantikannya. Dengan ini `BKC-DES-029` berstatus `approved`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan) | `approved` | "Enam peristiwa — BKC-DES-029 (Direkomendasikan)" dari 2 opsi bertanda rekomendasi — opsi "hanya jalur tender, persis bunyi keputusan" TIDAK dipilih | 18 September 2026 |

**Status pass ini**: `BKC-CQ-01` **DITUTUP PENUH**. Dua keputusan arsitektur yang menyimpang dari
bunyi harfiah keputusan bisnis (`BKC-DES-029`, `BKC-DES-033`) kini `approved` secara eksplisit,
bukan lolos diam-diam.

**Yang MASIH terbuka**, dan sengaja tidak ikut ditutup pass ini:

- `BKC-OQ-100` — jumlah penyesuaian/write-off yang terlanjur diposting tanpa koreksi AR. Dijawab
  keluaran **dry-run baca-saja** (`BKC-DES-034`), bukan perkiraan. Tidak memblokir gelombang
  `MVP-24`; memblokir penutupan `MVP-25`.
- Approval menyeluruh atas `BKC-DES-028`, `030`, `031`, `032`, `034`, `035` — keenamnya keputusan
  teknis turunan langsung dari keputusan bisnis yang sudah `approved`, tidak ada yang menyimpang
  dari bunyi keputusan mana pun. Statusnya tetap `draft` sampai Owner menyatakan approval
  menyeluruh, karena approval adalah tindakan manusia dan tidak boleh disimpulkan dari jawaban
  atas dua pertanyaan yang berbeda.
- Otorisasi membuat dan menjalankan migration backfill — **terpisah**, sesuai `AGENTS.md` bagian
  Keselamatan Database. `BKC-DEC-103`/`104` **bukan** otorisasi itu.

### Approval menyeluruh keputusan arsitektur revisi `1.3` (18 September 2026)

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `BKC-DEC-105` | Decision | **Menyetujui `BKC-DES-028`–`BKC-DES-035` secara utuh** — satu service `BillingInvoiceClosureService` yang memegang perhitungan sisa tagihan sekaligus penyelarasan status (`BKC-DES-028`), penyelarasan di enam peristiwa (`BKC-DES-029`, sudah disetujui terpisah lewat `BKC-DEC-104`), pola `SaveChanges`→selaraskan→`SaveChanges` di dalam satu transaksi (`BKC-DES-030`), transisi balik `CLOSED`→`FINAL` saat sisa tagihan naik lagi (`BKC-DES-031`), pemakaian ulang kunci penasihat `BIL_INVOICE_LEDGER_*` beserta invariant urutan pengambilannya (`BKC-DES-032`), `BilArHandoff` tidak disentuh (`BKC-DES-033`, sudah disetujui terpisah lewat `BKC-DEC-103`), migration backfill yang didahului dry-run baca-saja (`BKC-DES-034`), dan perluasan penjaga koreksi AR menerima `FINAL` maupun `CLOSED` (`BKC-DES-035`). Dengan ini keenam sumbu kontrak revisi `1.3` (`BIL-API-1.2`, `BIL-STATE-1.1`, `BIL-VALIDATION-1.1`, `BIL-INTEGRATION-1.0`, `BIL-PERMISSION-1.0`, `BIL-TEST-1.2`) naik dari `draft` menjadi `approved`; sumbu `calculation` tidak bergerak. Status revisi `1.3` naik dari `draft` menjadi `approved`. | Product/Domain Owner (persetujuan eksplisit dalam percakapan, wewenang ganda Finance/AR `BKC-DEC-085`) | `approved` | "Saya approve BKC-DES-028–035 — susun roadmap siap eksekusi" dari 3 opsi, dipilih sesudah konsekuensinya disajikan | 18 September 2026 |

> **Approval ini BUKAN otorisasi membuat maupun menjalankan migration.** Migration
> `BackfillClosedInvoicesFromFullySettledFinal` memuat pemutakhiran data yang **tidak dapat
> dimundurkan secara selektif** (`BKC-DES-034`): sesudah aplikasi berjalan, invoice `CLOSED` hasil
> backfill tidak dapat dibedakan dari invoice `CLOSED` yang lahir normal. Pembuatan dan eksekusinya
> menuntut konfirmasi eksplisit tersendiri, sesudah backup, sesuai `AGENTS.md` bagian Aturan Entity
> Framework dan Akses Data serta Keselamatan Database.

**Status setelah approval ini**: seluruh keputusan bisnis dan arsitektur revisi `1.3` tertutup.
Satu-satunya yang masih terbuka adalah `BKC-OQ-100`, yang **dijawab keluaran dry-run** — bukan
keputusan manusia, melainkan angka yang harus diukur lebih dulu. Ia tidak memblokir gelombang
`MVP-24`; ia memblokir penutupan `MVP-25`.

### Penutupan `BKC-OQ-100` (18 September 2026, keluaran dry-run `BE-BKC-064`)

**Konteks.** Dry-run baca-saja (`BE-BKC-064`) dijalankan pengguna sendiri di database dev/lokal,
memakai query yang kriterianya identik dengan `BillingInvoiceClosureService.CalculateOutstandingAsync`
(lihat `task/report/backend/be-bkc-064-dry-run-dampak-backfill.md`). Bukan keputusan yang
diputuskan manusia — murni angka yang harus diukur lebih dulu sebelum keputusan lanjutannya bisa
diambil dengan dasar, bukan dugaan.

| Metric | Nilai | Artinya |
| --- | --- | --- |
| `candidates_final_zero_outstanding` | **1** | Hanya satu invoice `FINAL` yang sisa tagihannya sudah nol pada database yang diukur — akan berpindah ke `CLOSED` bila `BE-BKC-065` dijalankan |
| `candidates_missing_ar_correction` | **0** | **Tidak ada** invoice di antara kandidat itu yang kehilangan koreksi AR — tidak dibutuhkan koreksi piutang susulan |
| `legacy_closed_missing_closed_at` | **0** | Tidak ada invoice `CLOSED` warisan dengan `ClosedAt` kosong pada database ini |

**Jawaban `BKC-OQ-100`**: **tidak dibutuhkan koreksi AR susulan.** Skala keterpaparan gap ini pada
database yang diukur jauh lebih kecil daripada skenario "seluruh invoice yang dibayar lunas" yang
diperingatkan temuan 2 September 2026 — kemungkinan karena database ini masih tahap pengembangan
dengan volume data terbatas, bukan bukti bahwa gap-nya tidak serius di produksi. `BE-BKC-065`
karena itu murni pemindahan status untuk **satu baris**, tanpa langkah koreksi tambahan.

**Catatan penting yang MUST dibawa ke eksekusi `BE-BKC-065`**: angka ini berasal dari database
tempat dry-run dijalankan. Bila migration nanti dijalankan ke database **lain** (staging/production
dengan volume data berbeda), dry-run ini **MUST diulang** pada database tujuan sebelum migration
dieksekusi di sana — angka `1`/`0`/`0` ini tidak otomatis berlaku untuk database lain.

**Status**: `BKC-OQ-100` **DITUTUP**. `BE-BKC-065` tidak lagi tertahan oleh pertanyaan ini — yang
tersisa murni otorisasi pembuatan dan eksekusi migration (`AGENTS.md` bagian Keselamatan Database),
belum diberikan pada pass ini.

### Eksekusi `BE-BKC-065` (18 September 2026) — backfill selesai, jalur berubah dari migration ke SQL langsung

Otorisasi diberikan bertahap sesuai `AskUserQuestion`: (1) `dotnet ef migrations add` diizinkan
khusus task ini — **dicoba, build internalnya gagal** sebelum satu file pun terbentuk (root cause
belum didiagnosis, dicatat sebagai temuan terbuka); (2) pengguna kemudian meminta eksplisit
"tidak usah pake migration dotnet" dan "lewat update query sql saja" — perubahan pendekatan yang
disetujui di tengah percakapan, bukan penyimpangan sepihak agent.

Skrip SQL baca-tulis (dibungkus `BEGIN`/`COMMIT` supaya dapat ditinjau) disusun dengan kriteria
identik dry-run `BE-BKC-064`, diserahkan sebagai teks, **dieksekusi pengguna sendiri** di tool
database miliknya. Hasil: **1 baris** (`BIL-20260903-00000004`) berpindah `FINAL`→`CLOSED`,
`ClosedAt` terisi `2026-09-10 11:45:53 +0700` (waktu pelunasan sebenarnya, terverifikasi BUKAN
waktu eksekusi script) — persis sesuai prediksi `BE-BKC-064`. Pengguna mengonfirmasi eksplisit
"sudah saya commit".

**Konsekuensi yang MUST dicatat**: backfill ini **tidak tercatat di `__EFMigrationsHistory`**
karena bukan EF Core migration. Jejak satu-satunya adalah laporan task
(`task/report/backend/be-bkc-065-backfill-data-invoice-lunas.md`), baris `00-interview-decisions.md`
ini, dan `UpdateBy = Guid.Empty` pada baris `BilInvoice` yang bersangkutan. Bila kelak dibutuhkan
jejak formal EF Core, itu keputusan terpisah.

**Temuan yang MUST ditindaklanjuti terpisah**: `dotnet build` untuk source `BE-BKC-060`–`063` GAGAL
saat dicoba (lewat build internal `dotnet ef migrations add`). Root cause belum didiagnosis pada
pass ini — keempat task itu masih berstatus "source lengkap, build belum diverifikasi" di roadmap,
dan sekarang punya bukti konkret bahwa build-nya memang bermasalah, bukan sekadar belum dicoba.

**Status**: Gelombang `MVP-24`+`MVP-25` (`BE-BKC-060`–`065`) selesai secara data dan keputusan.
Satu blocker teknis tersisa: kegagalan build source yang belum didiagnosis.

## Amandemen 21 September 2026 — Penerbitan fakta finansial ke modul konsumen

### Mengapa pass ini ada

Dua modul menunggu Billing menerbitkan fakta, dan **blueprint ini belum mencatat satu pun dari
keduanya**. Penelusuran menyeluruh atas folder `billing-kasir/` pada 21 September 2026 tidak
menemukan sebutan `BilCollectionHandoff` maupun kewajiban apa pun terhadap Farmasi.

Akibatnya nyata dan sedang berjalan hari ini:

| Modul | Yang tertahan | Sejak |
| --- | --- | --- |
| Finance (AR/AP) | Task `BE-FIN-016`, `BE-FIN-017`, dan turunannya `BE-FIN-018` berstatus `BLOCKED` | Permintaan 20 September 2026, belum dijawab |
| Farmasi | Seluruh resep rawat jalan macet permanen di `WaitingForPayment`; telaah apoteker tidak dapat dimulai sama sekali | Sejak `RJ-BIL-BE-002` menutup jalur lama, 24 Agustus 2026 |

### Dua konsumen, satu sumber peristiwa

**Finance** meminta jalur pemberitahuan bahwa sebuah tender berhasil. Tanpa itu, Finance tidak
punya cara resmi mengetahui pasien sudah membayar, sehingga uang yang sudah diterima kasir
berisiko dicatat ulang sebagai piutang — satu tagihan terhitung dua kali. Finance menolak membaca
`BilTender` langsung karena itu melanggar batas modul. Rincian lengkapnya pada
`finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md`, berdasar `FIN-DEC-005`
dan `FIN-DEC-006`. Finance sudah memverifikasi seluruh bidang yang dibutuhkannya **sudah ada** di
`BilTender` dan `BilSettlement` — permintaan ini tidak menuntut Billing menyimpan data baru.

**Farmasi** membutuhkan pernyataan clearance per resep, dengan aturan yang sudah dikunci pada
`pharmacy/00-interview-decisions.md` `PHA-DEC-063` sampai `PHA-DEC-070`. Yang penting bagi Billing:
clearance **tidak** dicabut hanya karena invoice kembali `FINAL` (`PHA-DEC-068`), dan tiga sebab
berbasis penarikan uang selalu mencabut secara fail-closed (`PHA-DEC-068-A`).

Keduanya bertumpu pada peristiwa yang sama dan data yang sama. `PaymentMethodId` sekaligus
membedakan tunai dari non-tunai bagi Finance **dan** menentukan hasil `Paid` versus
`InsuranceApproved` bagi Farmasi lewat flag `IsInsurance`/`IsCompanyGuarantor` (`PHA-DEC-065`).

### Keputusan

| ID | Keputusan | Owner | Status | Evidence |
| --- | --- | --- | --- | --- |
| `BKC-DEC-106` | Satu titik deteksi peristiwa, dua jenis surat berkolom tegas | Owner Billing | `approved` | Pilihan eksplisit user "Satu titik deteksi, dua jenis surat" dari 3 opsi, 21 September 2026 |
| `BKC-DEC-107` | Surat clearance terbit saat keadaan berubah, ditambah jalur pemeriksaan ulang | Owner Billing | `approved` | Pilihan eksplisit user "Terbit saat berubah, plus jalur pemeriksaan ulang" dari 3 opsi |
| `BKC-DEC-108` | Pengambilan surat dicatat dan terlihat; Billing tidak menggantungkan apa pun padanya | Owner Billing | `approved` | Pilihan eksplisit user "Dicatat dan terlihat, Billing tidak menggantungkan apa pun" dari 2 opsi |
| `BKC-DEC-109` | Baris handoff disimpan selamanya | Owner Billing | `approved` | Pilihan eksplisit user "Disimpan selamanya" dari 2 opsi |

Seluruhnya disetujui oleh user sesi ini yang menyatakan eksplisit dapat menyetujui atas nama owner
modul Billing, sejalan dengan `PHA-DEC-066` yang mencatatnya mewakili Product/Domain Owner,
Billing/Payer owner, dan Clinical Governance.

#### `BKC-DEC-106` — Satu titik deteksi, dua jenis surat

Billing mendeteksi peristiwa finansialnya **sekali di satu tempat**, lalu menerbitkan surat
terpisah sesuai konsumennya: satu berisi rincian uang untuk Finance, satu berisi pernyataan
clearance per resep untuk Farmasi.

Jaminan yang diberikan bentuk ini: mustahil terjadi keadaan Finance mengetahui sebuah pembayaran
sementara Farmasi tidak, atau sebaliknya — keduanya lahir dari deteksi yang sama.

**Bentuk transportnya tabel handoff persisted berkolom tegas**, mengikuti pola `BilArHandoff` yang
sudah terbukti. Ini konsekuensi langsung dari keputusan di atas dan didukung tiga hal: tiga jalur
handoff existing seluruhnya berbentuk demikian; Finance sendiri mengusulkannya dan menyerahkan
pilihan akhirnya ke Billing; dan `FinBillingHandoffIntake` di sisi konsumen sudah dibangun untuk
membaca baris, bukan menerima event — bahkan `HandoffType`-nya sudah memuat nilai `COLLECTION`.

Opsi tabel serba-guna bermuatan bebas **ditolak**: muatan bebas menghilangkan penjagaan bentuk,
sehingga kesalahan isi baru ketahuan saat dibaca konsumen.

#### `BKC-DEC-107` — Terbit saat berubah, plus jalur pemeriksaan ulang

Surat clearance untuk Farmasi terbit **hanya ketika keadaan clearance sebuah resep benar-benar
berubah** — menjadi boleh dikerjakan, atau ditahan kembali. Satu surat berarti satu perubahan
nyata; tidak ada surat berisi kabar yang sama berulang-ulang.

Karena surat bisa gagal diproses — konsumen error, sempat mati, atau baris terlewat — Billing
**juga menyediakan cara bagi Farmasi memeriksa ulang keadaan clearance terkini sebuah resep**,
tanpa menunggu perubahan berikutnya.

Ini bukan tambahan baru melainkan pemenuhan janji yang sudah disetujui: `PHA-DEC-063` menuntut
proyeksi di sisi Farmasi bersifat *reconcilable*, dan tanpa permukaan baca dari Billing, janji itu
tidak dapat dipenuhi.

**Konsekuensi yang dibawa ke desain**: Billing perlu menyediakan satu permukaan baca keadaan
clearance, bukan hanya menerbitkan surat. Bentuk teknisnya wewenang `design-business-module`.

Contoh mengapa ini penting. Pasien lunas pukul 09.00 dan suratnya terbit, tetapi proses di sisi
Farmasi sedang bermasalah sehingga surat itu tidak terbaca. Tanpa jalur pemeriksaan ulang, resep
akan tertahan sampai ada perubahan finansial berikutnya — yang mungkin tidak pernah terjadi,
karena tagihannya memang sudah lunas. Pasien menunggu di loket tanpa ada yang menyadari sebabnya.

#### `BKC-DEC-108` — Pengambilan surat dicatat, tetapi tidak menggantungkan apa pun

Billing mencatat kapan sebuah surat diambil konsumen, dan surat yang belum diambil dapat dilihat
sebagai daftar yang bisa diperiksa.

Billing sendiri **tidak menahan apa pun dan tidak mengubah perilakunya** karena surat belum
diambil. Uang sudah diterima, dan pelayanan tidak boleh tertahan karena urusan teknis antar modul.

Konsisten dengan `BKC-DEC-103`, yang sudah memutuskan status pada `BilArHandoff` menggambarkan
**penyerahan fakta**, bukan hasil di sisi konsumen. Sumber kebenaran "tagihan ini lunas" tetap
`BilInvoice.Status`, bukan status handoff.

Batas yang disadari: keterlihatan hanya berguna bila ada yang memeriksa daftarnya. Peringatan aktif
dicatat terbuka sebagai `BKC-OQ-101`.

#### `BKC-DEC-109` — Baris handoff disimpan selamanya

Tidak ada pembersihan maupun pengarsipan. Baris handoff adalah **jejak audit lintas modul**: ia
membuktikan Billing pernah memberi tahu, dan kapan persisnya.

Sejalan dengan tiga jalur handoff existing yang memang tidak punya mekanisme pembersihan.
Volumenya sebanding jumlah transaksi, bukan sesuatu yang meledak tanpa batas.

### Pertanyaan terbuka

| ID | Pertanyaan | Dampak | Owner | Status |
| --- | --- | --- | --- | --- |
| `BKC-OQ-101` | Berapa lama sebuah surat boleh menggantung sebelum dianggap tidak wajar, dan siapa yang menerima peringatannya? | `NON_BLOCKING` — desain dapat berjalan dengan keterlihatan pasif sesuai `BKC-DEC-108` | Operasional Billing + Finance + Farmasi | `open` |

Pertanyaan ini sejenis dengan butir terbuka di Farmasi soal durasi outage sebelum eskalasi.
Keduanya keputusan operasional yang lebih baik ditetapkan bersama setelah jalurnya berjalan dan
volumenya terlihat, bukan ditebak sekarang.

### Acceptance criteria

1. Satu pembayaran yang berhasil menghasilkan surat untuk Finance berisi rincian uang, dan — bila
   keadaan clearance resep ikut berubah — surat untuk Farmasi, keduanya lahir dari deteksi
   peristiwa yang sama.
2. Pembayaran yang berhasil tetapi belum melunasi tagihan menghasilkan surat untuk Finance tanpa
   surat clearance untuk Farmasi, karena keadaan resep belum berubah.
3. Tagihan yang lunas lewat penghapusan tagihan menghasilkan surat clearance berhasil finansial
   `PaymentWaived` walau tidak ada uang yang masuk.
4. Penambahan biaya tindakan pada tagihan yang sudah lunas **tidak** menerbitkan surat pencabutan
   clearance untuk resep pada tagihan itu.
5. Pembalikan pembayaran menerbitkan surat pencabutan clearance untuk seluruh resep pada tagihan
   itu, dan bagi Finance menerbitkan baris baru — bukan mengubah baris lama.
6. Surat yang sama diterbitkan dua kali untuk peristiwa yang sama menghasilkan tepat satu baris
   efektif di sisi konsumen, dikunci oleh kunci idempotensi.
7. Farmasi dapat menanyakan keadaan clearance terkini sebuah resep dan memperoleh jawaban yang
   sama dengan surat terakhir yang sah, walau surat itu belum pernah terbaca.
8. Surat yang belum diambil konsumen dapat ditemukan dan dihitung, tanpa mengubah perilaku Billing
   mana pun.

### Yang sengaja tidak diubah

| Hal | Alasan |
| --- | --- |
| Cara Billing menghitung sisa tagihan dan menutup invoice | Sudah terkunci `BKC-DEC-100` dan `BKC-DEC-105`; pass ini tidak mengusiknya |
| `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment` | Tetap apa adanya; `BKC-DEC-103` sudah memutuskan `BilArHandoff` tidak disentuh |
| Alokasi uang per baris invoice | Ditolak dua kali sebelumnya — `PHA-DEC-064` dan `PHA-DEC-068-A`. `BilPaymentAllocation` tetap hanya mengenal sasaran `INVOICE` |
| Aturan internal Finance soal piutang dan jurnal | Milik `finance-management` |
| Aturan internal Farmasi soal telaah, penyiapan, dan penyerahan | Milik `pharmacy` |

### Yang belum ada dan dibutuhkan

Belum ada task roadmap di `billing-kasir` untuk membangun kedua jalur penerbitan ini. Task-nya
perlu dibuat lewat `plan-module-delivery`, dan penyelesaiannya membuka `BE-FIN-016`, `BE-FIN-017`,
`BE-FIN-018` di Finance sekaligus slice financial clearance handoff di Farmasi yang sudah berstatus
`READY_FOR_DOMAIN_DESIGN`.

### `BKC-DEC-110` — Approval keputusan arsitektur dan penempatan pekerjaan pemulihan

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menyetujui `BKC-DES-036`–`BKC-DES-041` secara utuh** — dua tabel handoff berkolom tegas (`036`), surat penerimaan sebagai aggregate root tersendiri tanpa ketergantungan pada finalisasi (`037`), satu service penerbit dipanggil dari empat titik yang sama dengan penyelaras status (`038`), nomor versi finansial monoton per resep yang dilindungi kunci penasihat yang sudah ada (`039`), pembacaan keadaan clearance berbentuk pemanggilan dalam proses (`040`), dan hasil penyelarasan yang membawa keterangan sebab (`041`). Dengan ini keenam sumbu kontrak revisi `1.4` — `BIL-API-1.3`, `BIL-STATE-1.2`, `BIL-VALIDATION-1.2`, `BIL-INTEGRATION-1.1`, `BIL-PERMISSION-1.1`, `BIL-TEST-1.3` — naik dari `draft` menjadi `approved`. Sumbu `calculation` tidak bergerak dan berkasnya tidak disunting |
| Owner | Product/Domain Owner, Billing/Payer owner, Clinical Governance (ketiganya per `PHA-DEC-066`) |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Saya setujui desainnya sekarang" atas pertanyaan gerbang `plan-module-delivery`, 21 September 2026 |
| Batas approval | Approval ini **bukan** wewenang menulis source, membuat migration, maupun menjalankannya. Ketiganya tetap diminta terpisah per task saat eksekusi, sesuai `AGENTS.md` |

### `BKC-DEC-111` — Pemulihan resep yang terlanjur macet masuk gelombang yang sama

| Field | Isi |
|---|---|
| Type | Decision |
| Item | Pekerjaan memulihkan resep yang sudah terlanjur tertahan — yang tagihannya sudah lunas sebelum jalur penerbitan berdiri — **masuk gelombang yang sama** dengan pembangunan jalurnya, bukan menyusul sebagai pekerjaan terpisah. Pemulihan dikerjakan dengan memanggil permukaan pemeriksaan ulang (`BKC-DEC-107`) untuk resep yang masih menunggu pembayaran, **bukan** dengan skrip pemutakhiran data langsung |
| Owner | Product/Domain Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Gelombang yang sama" dari 2 opsi, 21 September 2026 |
| Alasan | Menyalakan jalur baru sementara resep lama tetap macet akan membuat dua jenis resep berperilaku berbeda tanpa sebab yang terlihat petugas — dan petugas akan menyimpulkan fiturnya tidak bekerja |
| Konsekuensi | Satu task tambahan pada gelombang yang sama. Karena pemulihan membaca dari Billing dan tidak menulis data secara langsung, ia **tidak** menuntut otorisasi pemutakhiran data terpisah — berbeda dari backfill `BE-BKC-065` |

---

## Pass B — Integrasi Rawat Inap ↔ Billing (Pass B — Yasmina / Billing Management)

Disahkan 24 September 2026 melalui sesi `/grill-me`. Pass ini menutup seluruh celah integrasi (*gap*) dan keputusan bisnis terbuka (*Open Business Decisions*) yang tercantum pada dokumen `docs/Modul-RS/Rawat-Inap-To-Billing/PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 48 (`DEC-BILL-001`, `DEC-INT-001` s.d. `004`, `DEC-BILL-002`, dan arsitektur `Financial Clearance`).

### Batas Scope Pass B
1. **Di dalam scope:**
   - Penerimaan fakta occupancy Rawat Inap (`SourceDomain = "INPATIENT"`, `SourceType = "ROOM_STAY"`) pada adapter Billing (`ContractBillingChargeSourceAdapter`).
   - Mesin Perhitungan Tarif Kamar (*Room Charge Engine*) dengan formula jam masuk hari pertama, transfer kamar, dan mutasi kamar.
   - Kebijakan deposit Rawat Inap (minimal 30% estimasi awal dan 100% dari porsi tanggung jawab pasien sebelum tindakan besar).
   - Biaya Administrasi Rawat Inap 7% (cap Rp6.000.000).
   - Penerbitan status *Financial Clearance* (`PENDING`, `BLOCKED`, `CLEARED`, `REVOKED`) sebagai *Source of Truth* di Billing dan penyaluran sinyal/webhook ke Rawat Inap.
   - Konsolidasi non-destruktif tagihan pasien alihan IGD ke Rawat Inap pada final settlement.
2. **Di luar scope:**
   - Alur operasional klinis bangsal, tata kelola tempat tidur, dan pengkajian perawat/dokter (domain eksklusif `InPatientManagement`).
   - Algoritme availability bed bangsal.

---

### `BKC-DEC-112` — Ratifikasi Batas Scope Pass B Integrasi Rawat Inap ↔ Billing

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Mengunci batas wewenang modul Billing/Kasir pada Pass B.** Billing bertindak sebagai *consumer* atas fakta pelayanan dan hunian fisik pasien dari Rawat Inap, serta menjadi *single source of truth* atas seluruh nominal finansial, tarif kamar, deposit, invoice, penjamin, biaya administrasi, dan status kelayakan keuangan (*Financial Clearance*). Billing tidak mengelola data tempat tidur atau rekam medis rawat inap |
| Owner | Yasmina (Billing Owner) & Business Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user pada grill-me 24 September 2026 |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 3, 4, 8, 12, dan 13 |

---

### `BKC-DEC-113` — Kebijakan Biaya Administrasi Rawat Inap (`DEC-BILL-001`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menyatakan aturan lama biaya administrasi nominal tetap (flat fee) di V2 sebagai `superseded` untuk pelayanan Rawat Inap.** Biaya Administrasi Rawat Inap dihitung dengan formula proporsional: **`AdminFee = MIN(EligibleBillAmount × 7%, Rp 6.000.000)`**. Komponen *eligible bill* mencakup sewa kamar, visite dokter, tindakan, dan penunjang medis non-farmasi tertentu sesuai master tarif. Tagihan rawat jalan/IGD murni tetap dapat menggunakan ketentuan tarifnya masing-masing |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Supersede aturan lama: Tetapkan Biaya Administrasi Rawat Inap sebesar 7% dari tagihan eligible dengan batas maksimal (cap) Rp6.000.000", 24 September 2026 |
| Alasan | Standar operasional rumah sakit modern menetapkan biaya administrasi rawat inap berbasis persentase untuk mencerminkan kompleksitas koordinasi berkas dan klaim, dengan perlindungan plafon (cap) agar tidak membebani pasien perawatan intensif |
| Konsekuensi | Calculation engine Billing menambahkan formula `AdminFeeCalculation` dengan parameter persentase (7%) dan cap (Rp 6.000.000). Master data lama untuk biaya administrasi rawat inap berstatus nominal tetap dipensiunkan |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 29 dan Bagian 48 `DEC-BILL-001` |

---

### `BKC-DEC-114` — Batas Waktu Masuk Kamar Hari Pertama (`DEC-INT-001`, `DEC-INT-002`, `DEC-INT-003`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan aturan jam masuk hari pertama menggunakan operator inklusif batas bawah (`>=`):**<br>1. Masuk kamar pukul **`00:00:00` s.d. `< 18:00:00`**: Dikenakan **100%** tarif kamar hari pertama.<br>2. Masuk kamar pukul **`18:00:00` s.d. `< 22:00:00`**: Dikenakan potongan sehingga menjadi **50%** tarif kamar hari pertama.<br>3. Masuk kamar pukul **`22:00:00` s.d. `< 24:00:00` (sebelum tengah malam)**: Dikenakan potongan sehingga menjadi **20%** tarif kamar hari pertama.<br>4. Masuk kamar tepat pukul **`00:00:00`** dihitung sebagai awal tanggal kalender baru (beban sewa kamar untuk hari kalender sebelumnya adalah **0%**) |
| Owner | Yasmina (Billing) & Muhammad Hamzah (Rawat Inap) |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Inklusif batas bawah (>=)", 24 September 2026 |
| Alasan | Operator `>=` pada detik ke-00 memberikan kepastian fail-safe dan keuntungan administratif bagi pasien yang tiba di bangsal tepat pada pergantian jam batas kebijakan |
| Konsekuensi | `RoomChargePolicyService` dan engine perhitungan room charge menerapkan evaluasi interval waktu lokal berbasis time range inklusif batas bawah |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 17, 18, dan Bagian 48 `DEC-INT-001`, `002`, `003` |

---

### `BKC-DEC-115` — Formula Pembagian Tarif Perpindahan Kamar Multipel di Hari yang Sama (`DEC-INT-004`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan skema pro-rata berbasis durasi jam hunian riil pasien untuk kasus perpindahan kamar lebih dari 1 kali dalam hari kalender yang sama:**<br>1. Jika perpindahan tepat 1 kali (2 kamar: A → B): Berlaku aturan standar split 50% Kamar A + 50% Kamar B.<br>2. Jika perpindahan > 1 kali (contoh 3 kamar: Kamar Reguler A → Kamar Observasi B → ICU C): Setiap segmen kamar dikenakan tarif secara proporsional sesuai perbandingan durasi jam hunian riil di kamar tersebut terhadap total jam rawat pada hari itu: **`ChargeKamar = (DurasiMenitSegmen / TotalMenitRawatHariItu) × TarifHarianKamar`** |
| Owner | Yasmina (Billing) & Product Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Pro-rata durasi jam riil", 24 September 2026 |
| Alasan | Adil secara objektif bagi pasien/penjamin dan rumah sakit, serta mencegah pembebanan kamar transit bernilai tinggi secara berlebihan saat pasien hanya singgah dalam waktu singkat sebelum dipindahkan ke unit intensif |
| Konsekuensi | Billing menghitung durasi per segmen hunian dari timestamp `StartAt` dan `EndAt` yang dikirimkan oleh event `ROOM_TRANSFERRED` Rawat Inap |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 19, 20, dan Bagian 48 `DEC-INT-004` |

---

### `BKC-DEC-116` — Dasar Perhitungan Deposit 100% Sebelum Tindakan Besar (`DEC-BILL-002`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan dasar perhitungan kewajiban deposit 100% sebelum tindakan besar adalah dari Porsi Tanggung Jawab Pasien (Patient Responsibility / Excess).**<br>1. Untuk pasien umum (mandiri / *self-pay*), porsi tanggung jawab pasien adalah 100% dari total estimasi bruto tindakan.<br>2. Untuk pasien dengan asuransi/perusahaan penjamin/BPJS, porsi tanggung jawab pasien adalah selisih estimasi biaya tindakan yang tidak ditanggung atau melebihi plafon penjamin (excess).<br>3. Pasien tidak dapat dijadwalkan/diberangkatkan ke ruang operasi/tindakan besar selama saldo deposit pasien belum menutup 100% dari porsi tanggung jawab pasien tersebut, kecuali ada *Emergency Financial Override* resmi dari Manajemen/Direksi |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "100% dari porsi tanggung jawab pasien (Patient Responsibility / Excess)", 24 September 2026 |
| Alasan | Menghindarkan keluarga pasien asuransi dari keharusan menyetor uang muka tunai sebesar nilai bruto tindakan yang sebenarnya telah dijamin oleh surat jaminan (GL) penjamin |
| Konsekuensi | Layanan deposit Billing menghitung `DepositShortfall` tindakan besar dengan rumus: `EstimasiTindakan - EstimasiCoveredPenjamin - SaldoDepositTersedia` |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 26, 27, dan Bagian 48 `DEC-BILL-002` |

---

### `BKC-DEC-117` — Arsitektur Financial Clearance dan Integrasi Handoff ke Rawat Inap

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Billing Management adalah *Single Source of Truth* untuk kelayakan keuangan pemulangan pasien (*Financial Clearance*). Kode Billing yang sebelumnya membaca tabel Rawat Inap `InpFinancialClearance` dinyatakan keliru secara arsitektur dan diganti sepenuhnya.**<br>1. **Entitas Internal Billing:** Billing mencatat status kelayakan pada tabel internal (mengikuti pola `BilPrescriptionClearanceHandoff` yang sudah sukses di Farmasi).<br>2. **Status Resmi:** `PENDING` (menunggu penyelesaian tagihan), `BLOCKED` (tertahan karena ada blocker finansial), `CLEARED` (lunas / disetujui pulang), dan `REVOKED` (persetujuan dicabut karena tagihan susulan).<br>3. **Kueri Sinkron:** Billing menyediakan endpoint `GET /api/v1/health-services/billing-management/billing/invoices/encounter/{encounterId}/summary` yang menyajikan status clearance, daftar blocker operasional (tanpa privasi rupiah untuk perawat), saldo deposit, dan sisa tagihan.<br>4. **Sinyal Handoff / Webhook:** Saat kasir menyelesaikan transaksi pelunasan (`CLEARED`) atau mencabut kelayakan (`REVOKED`), Billing menerbitkan sinyal yang dikonsumsi Rawat Inap untuk membuka kunci pemulangan atau mengeksekusi *Auto-Reblock* seketika di bangsal |
| Owner | Yasmina (Billing) & Muhammad Hamzah (Rawat Inap) |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Pola Handoff & Event Terpadu", 24 September 2026 |
| Alasan | Menegakkan integritas data transaksi keuangan dan memastikan bangsal rawat inap tidak memulangkan pasien yang tagihannya belum beres atau mengalami tagihan susulan |
| Konsekuensi | Menghilangkan pembacaan `InpFinancialClearance` dari `PatientBillingSummaryService.cs:88` dan membangun service penilai kelayakan di dalam modul Billing |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 8.8, 10.4, 10.5, 34, 37; selaras `RWI-DEC-158` dan `RWI-DEC-160` |

---

### `BKC-DEC-118` — Konsolidasi Tagihan Alihan IGD ke Rawat Inap (`BILL-INT-007`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan mekanisme konsolidasi tagihan IGD ke Rawat Inap menggunakan skema Multidomain Non-Destruktif pada satu settlement terpadu.**<br>1. Seluruh item tagihan selama di IGD dan di Bangsal Rawat Inap tetap mencatat `SourceDomain` aslinya (`EMERGENCY` vs `INPATIENT`) dan tidak digabungkan secara destruktif.<br>2. Pada saat pasien dinyatakan pulang dari rawat inap, kasir dapat menyelesaikan tagihan IGD dan Rawat Inap dalam satu transaksi *final settlement* terpadu dengan satu kwitansi resmi yang merinci beban per unit pelayanan |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user "Konsolidasi Settlement Non-Destruktif", 24 September 2026 |
| Alasan | Memudahkan keluarga pasien menyelesaikan administrasi di satu loket kasir saat kepulangan tanpa merusak audit penelusuran pendapatan unit IGD vs Rawat Inap |
| Konsekuensi | Billing settlement service mendukung penutupan multi-folio/multi-invoice yang tertaut pada satu `EncounterId` pasien |
| Trace | `PRD Integrasi-Rawat-Inap-dengan-Billing.md` Bagian 31 dan Bagian 43 `BILL-INT-007` |

---

### Acceptance Criteria Tambahan Pass B
- `BKC-AC-080`: Panggilan kueri summary billing untuk encounter rawat inap mengembalikan status clearance dan blocker list yang dihitung dari status invoice Billing, bukan dari tabel rawat inap (`BKC-DEC-117`).
- `BKC-AC-081`: Pasien rawat inap dengan tagihan eligible Rp 10.000.000 dikenakan admin fee Rp 700.000 (7%), dan tagihan Rp 100.000.000 dikenakan admin fee maksimal Rp 6.000.000 (`BKC-DEC-113`).
- `BKC-AC-082`: Pasien masuk kamar pukul 18:00:00 tepat dikenakan room charge 50% hari pertama; masuk kamar pukul 22:00:00 tepat dikenakan 20%; masuk pukul 00:00:00 dihitung tanggal baru dengan beban hari sebelumnya 0% (`BKC-DEC-114`).
- `BKC-AC-083`: Pasien pindah 3 kamar di hari yang sama ditagih pro-rata sebanding menit riil di tiap kamar terhadap total menit rawat hari itu (`BKC-DEC-115`).
- `BKC-AC-084`: Pasien asuransi dengan tindakan besar Rp 20.000.000 yang dijamin Rp 15.000.000 hanya diwajibkan deposit tindakan sebesar Rp 5.000.000 (100% patient excess) (`BKC-DEC-116`).
- `BKC-AC-085`: Tagihan susulan yang muncul setelah invoice lunas memicu pencabutan kelayakan (`REVOKED`) dan menerbitkan sinyal auto-reblock ke Rawat Inap (`BKC-DEC-117`).
- `BKC-AC-086`: Tagihan IGD dan Rawat Inap pada satu encounter dapat diselesaikan dalam satu settlement terpadu dengan pelaporan per unit yang tetap terpisah (`BKC-DEC-118`).
- `BKC-AC-087`: Pasien alihan rawat jalan ke rawat inap (Rajal → Ranap) tidak dikenakan biaya administrasi ganda. Biaya admin rajal otomatis gugur (batal/terkreditkan) dan digantikan oleh biaya admin ranap 7% cap Rp 6.000.000 (`BKC-DEC-119`).

---

### `BKC-DEC-119` — Aturan Penggantian Biaya Administrasi saat Alihan Rawat Jalan ke Rawat Inap (Rajal → Ranap)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan aturan penggantian (*replacement / offset*) biaya administrasi saat pasien rawat jalan dialihkan menjadi rawat inap:**<br>1. **Prinsip Beban Tunggal:** Pasien yang menjalani admisi rawat inap dari rujukan poliklinik rawat jalan pada hari/episode pelayanan yang sama **TIDAK BOLEH** dikenakan biaya administrasi ganda (rajal + ranap). Biaya administrasi rawat jalan dinyatakan **gugur** dan digantikan oleh Biaya Administrasi Rawat Inap (7% cap Rp6.000.000 sesuai `BKC-DEC-113`).<br>2. **Kasus Belum Dibayar (Tagihan Rajal Terbuka):** Item biaya administrasi rawat jalan pada invoice langsung dibatalkan (*voided / waived*) dengan alasan sistem `"SUPERSEDED_BY_INPATIENT_ADMISSION"`, lalu digantikan oleh komponen biaya administrasi rawat inap.<br>3. **Kasus Sudah Terlanjur Dibayar di Kasir Poliklinik:** Nominal biaya administrasi rawat jalan yang telah dibayarkan **wajib diperhitungkan sebagai kredit pembayaran berjalan (*credit offset / progress payment*)** terhadap total tagihan akhir rawat inap saat final settlement, sehingga pasien tidak dirugikan.<br>4. **Audit Trail Wajib:** Pembatalan atau pengkreditan biaya administrasi rajal wajib tercatat dalam audit log transaksi finansial tanpa *hard delete* |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Penegasan langsung pemilik kebutuhan / user pada sesi 24 September 2026: *"jika sebelumnya ada biaya admin rajal, trus pasien pindah ke ranap. berarti biaya admin rajal menjadi gugur dan digantikan oleh biaya admin ranap"* |
| Alasan | Mencegah tagihan ganda (*double administrative charging*) yang membebani pasien dan berpotensi memicu sengketa (*dispute*) verifikasi klaim BPJS/asuransi penjamin |
| Konsekuensi | Billing invoice engine menambahkan event listener atau handler saat admisi ranap dikonfirmasi untuk mendeteksi apakah invoice encounter tersebut sebelumnya memuat item administrasi rajal, kemudian mengeksekusi pembatalan atau pengkreditan otomatis |
| Trace | Konfirmasi user 24 September 2026; memperkuat `BKC-DEC-113` dan menutup catatan gap `CAP-10` pada `01-existing-capability-map.md` baris 88 & 136 |

---

### `BKC-DEC-120` — Batas Waktu dan Penanganan Tagihan Susulan Pasca-Izin Pulang (`BKC-OQ-102`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan batas waktu penegakan *Auto-Reblock* dan aturan penerimaan tagihan susulan (*late charges*):**<br>1. **Jendela Auto-Reblock Aktif:** Selama invoice rawat inap masih berstatus `OPEN` (termasuk saat pasien telah dinyatakan `CLEARED` di kasir namun proses administratif/serah terima pemulangan di bangsal masih berlangsung di hari yang sama), setiap tagihan susulan yang masuk akan **secara otomatis membatalkan status clearance menjadi `REVOKED`**, mencatat `RevocationReason = "LATE_CHARGE_POSTED"`, menaikkan nomor versi finansial, dan mengirim sinyal pemblokiran seketika ke bangsal rawat inap.<br>2. **Penguncian Permanen saat CLOSED:** Setelah invoice resmi mencapai status `CLOSED` (seluruh pembayaran tuntas dan pasien resmi keluar secara administratif), invoice rawat inap berstatus terkunci tetap (*immutable*). Tagihan baru yang mencoba masuk setelah invoice `CLOSED` akan **ditolak secara otomatis oleh sistem** (`BIL-VAL-024` / `BIL-VAL-127`).<br>3. **Pengecualian / Reopen Terkendali:** Tagihan susulan pasca-`CLOSED` hanya dapat dimasukkan apabila ada otorisasi tertulis dan pembukaan kembali (*reopen override*) oleh Supervisor Kasir / Kepala Kasir dengan pencatatan audit alasan bisnis yang lengkap |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user pada sesi `/grill-me` 24 September 2026: *"Auto-Reblock berlaku selama invoice masih OPEN / dalam masa pemulangan hari yang sama; jika invoice sudah resmi CLOSED/Finalized, tagihan susulan ditolak otomatis kecuali dibuka kembali lewat otorisasi Supervisor Kasir"* |
| Alasan | Menjaga keseimbangan antara pencegahan kebocoran pendapatan (*revenue leakage*) saat pasien masih di RS dengan kepastian pembukuan kasir harian agar invoice lunas tidak terbuka kembali tanpa kendali |
| Konsekuensi | Adapter intake biaya memvalidasi status invoice: jika `OPEN` dan sebelumnya `CLEARED`, picu auto-reblock; jika `CLOSED`, tolak permintaan dengan kode galat `422 Unprocessable Entity` kecuali disertai token otorisasi supervisor |
| Trace | `04-prd-to-mvp.md` pertanyaan terbuka `BKC-OQ-102` (ditutup) |

---

### `BKC-DEC-121` — Ketentuan Pengecualian dan Variasi Biaya Administrasi Rawat Inap (`BKC-OQ-103`)

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan pengelolaan variasi dan pengecualian biaya administrasi rawat inap secara deklaratif melalui Master Data Policy:**<br>1. **Aturan Default Ranap:** Nilai persentase 7% dengan pagu (cap) Rp6.000.000 adalah aturan dasar (*default*) untuk seluruh layanan rawat inap umum.<br>2. **Deklaratif via Master Data:** Kasus khusus seperti One Day Care (ODC), rawat inap singkat, atau kelas perawatan tertentu dikonfigurasi melalui baris aturan terpisah di tabel master `MstAdministrationFeePolicy` dengan filter kriteria (`ServiceType`, `PatientClass`) tanpa mengubah kode program sistem.<br>3. **Perlakuan Pasien BPJS / Paket Klaim:** Untuk pasien dengan penjaminan sistem paket (seperti BPJS Kesehatan / INA-CBGs), biaya administrasi tidak ditagihkan kepada pasien secara mandiri karena telah menjadi satu kesatuan inklusif dalam paket klaim penjaminan |
| Owner | Yasmina (Billing) & Tarif/Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user pada sesi `/grill-me` 24 September 2026: *"Dikelola deklaratif lewat Master Data Policy: 7% cap Rp6 juta adalah aturan default ranap umum; kasus khusus (misal ODC atau kelas tertentu) diatur lewat baris aturan terpisah di tabel master tanpa ubah kode program; untuk pasien BPJS/paket klaim, admin fee tidak ditagihkan ke pasien (inklusif klaim)"* |
| Alasan | Memberikan fleksibilitas penuh bagi manajemen rumah sakit untuk menyesuaikan tarif tanpa bergantung pada rilis kode teknis baru, serta mematuhi regulasi jaminan kesehatan nasional |
| Konsekuensi | Service `AdministrationFeeCalculationService` mengevaluasi baris kebijakan `MstAdministrationFeePolicy` yang paling spesifik berdasarkan prioritas kecocokan dan memeriksa tipe penjaminan pasien sebelum membebankan biaya admin ke porsi pasien |
| Trace | `04-prd-to-mvp.md` pertanyaan terbuka `BKC-OQ-103` (ditutup) |

---

### `BKC-DEC-122` — Tanggal Efektif Pemberlakuan Kebijakan Biaya Administrasi Baru

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menetapkan bahwa kebijakan Biaya Administrasi Rawat Inap baru (7% cap Rp6.000.000) diberlakukan untuk semua pasien yang dipulangkan (*discharged*) pada atau setelah tanggal efektif aktivasi (`EffectiveFrom`), terlepas dari tanggal awal masuk admisi pasien.**<br>Pasien yang masuk sebelum tanggal aktivasi namun baru menyelesaikan administrasi pemulangan setelah tanggal aktivasi akan dikenakan perhitungan persentase 7% cap Rp6.000.000 pada saat perhitungan final billing |
| Owner | Yasmina (Billing) & Finance Owner |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit user pada sesi `/grill-me` 24 September 2026: *"Berlaku untuk semua pasien yang dipulangkan (discharge) pada atau setelah tanggal efektif aktivasi, terlepas dari tanggal awal masuknya"* |
| Alasan | Standar operasional rumah sakit menetapkan bahwa tarif administrasi pemulangan dihitung pada saat penyelesaian tagihan akhir (*discharge calculation*), bukan pada saat admisi masuk |
| Konsekuensi | Mesin kalkulasi membandingkan waktu evaluasi billing / discharge dengan `EffectiveFrom` pada master policy administrasi ranap |
| Trace | Sesi wawancara aktivasi tanggal efektif 24 September 2026 |

---

### Acceptance Criteria Tambahan Penutupan Open Questions
- `BKC-AC-088`: Tagihan susulan yang masuk pada invoice ranap `OPEN` yang sudah `CLEARED` memicu auto-reblock menjadi `REVOKED`. Tagihan susulan yang masuk pada invoice ranap berstatus `CLOSED` otomatis ditolak oleh adapter dengan pesan penolakan yang menyatakan tagihan telah dikunci (`BKC-DEC-120`).
- `BKC-AC-089`: Evaluasi administrasi ranap mencocokkan baris kebijakan teraktif pada `MstAdministrationFeePolicy`; pasien BPJS mencatat nominal administrasi Rp 0 pada kewajiban pasien (*Patient Responsibility*) (`BKC-DEC-121`).
- `BKC-AC-090`: Pasien dengan tanggal pemulangan setelah tanggal aktif kebijakan dikenakan skema 7% cap Rp6.000.000 secara otomatis saat final billing (`BKC-DEC-122`).




## Amendment 24 September 2026 — Revisi UI Billing: Filter, Default, Asuransi, Diskon Dokter, Refund (`BUI-DEC-001`–`013`)

Disahkan lewat sesi `/grill-me` 24 September 2026, dipicu permintaan langsung Product/Domain
Owner (Yasmin) berupa 14 poin revisi tampilan pada modul Billing frontend. Prefix keputusan
`BUI-DEC` (Billing UI) dipakai terpisah dari `BKC-DEC` supaya penelusuran tetap jelas — pola
yang sama dengan `PC-DEC` (Petty Cash) dan `MPY-DEC` (Multi-Payer) sebelumnya.

### Batas Scope Pass Ini

1. **Di dalam scope:**
   - Filter tanggal, default data, penamaan label, tata letak formulir pada layar Billing dan
     turunannya (Perbandingan Asuransi, Edit Asuransi, Edit Status Tagihan, Diskon Dokter,
     Ajukan Refund, Riwayat Pembayaran).
   - Aturan tampil yang bersumber dari data yang **sudah** dikembalikan backend (status
     coverage per item, sisa deposito, daftar item billing).
   - Pemindahan tombol aksi (Refund/Adjustment/Write-Off) antar halaman.
2. **Di luar scope — untuk modul lain:**
   - Aturan coverage asuransi itu sendiri (siapa/apa yang menentukan suatu item tercover) —
     milik Insurance/Clinical (`InsuranceCoverageService`), di sini hanya **dibaca** hasilnya.
   - Integrasi Rawat Inap ↔ Billing (`BKC-DEC-112`–`122`, Pass B) — sedang berjalan terpisah,
     tidak tersentuh sama sekali oleh pass ini.
   - Alur persetujuan refund/adjustment/write-off — hanya tombolnya yang dipindah
     (`BUI-DEC-013`), alurnya sendiri **tidak diubah** (`BUI-DEC-012`).
   - Perubahan skema database, migration, atau endpoint baru — bila trace lanjutan menemukan
     backend memang perlu berubah, itu dicatat sebagai dependency lintas modul, bukan
     dikerjakan sebagai bagian pass frontend ini.

---

### `BUI-DEC-001` — Filter Tanggal pada Layar Billing

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menambahkan filter Tanggal Awal dan Tanggal Akhir** pada layar Billing, mengikuti pola visual filter yang sudah berjalan di layar itu (tidak membuat komponen filter baru) |
| Owner | Yasmin (Product/Domain Owner) |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 1 |
| Konsekuensi | Bentuk komponen filter (posisi, lebar, pemicu apply — otomatis atau tombol) `DEV_DISCRETION`, mengikuti pola filter existing |

---

### `BUI-DEC-002` — Default Data Saat Layar Billing Dibuka

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Saat layar Billing pertama dibuka (belum ada filter diterapkan pengguna), data yang tampil adalah invoice dengan tanggal hari ini dan status `OPEN`** — bukan seluruh riwayat invoice |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 2 |
| Alasan | Kasir paling sering butuh melihat tagihan yang masih berjalan hari itu, bukan riwayat lengkap; mengurangi beban muat data yang tidak relevan saat layar dibuka |
| Konsekuensi | Filter tanggal (`BUI-DEC-001`) dan filter status wajib punya nilai bawaan yang bisa diprogram, bukan hanya kosong |

---

### `BUI-DEC-003` — Penggantian Label "Drug" Menjadi "Obat / Medicine"

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Seluruh label tampilan bertuliskan "Drug" diganti menjadi "Obat / Medicine"** |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 3 |
| Assumption | Cakupannya adalah **label/teks statis UI** (judul kolom, judul section, placeholder, tombol) — **bukan** nilai data master obat itu sendiri (nama obat seperti "Paracetamol" pada master data tetap apa adanya). Asumsi ini dipakai karena permintaan menyebut "tampilan", bukan "data". Bila keliru, MUST dikoreksi sebelum implementasi — lihat Open Question `BUI-OQ-01` |
| Konsekuensi | Perubahan murni pada string/label komponen, nol dampak pada struktur data atau kontrak API |

---

### `BUI-DEC-004` — Perbandingan Asuransi Hanya Menampilkan Insurance Provider

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Pada modal/layar perbandingan asuransi, pilihan yang ditampilkan hanya "Insurance provider". Pilihan "pribadi" dan "perusahaan" disembunyikan** dari layar perbandingan ini |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 4 |
| Batas | Ini **hanya** menyangkut daftar pilihan pada layar/modal **perbandingan** asuransi. Payment method "Tunai" dan "Penjamin Perusahaan" pada Edit Asuransi (`BUI-DEC-006`) **tidak tersentuh** — keduanya tetap ada sebagai payment method, hanya tidak muncul sebagai kandidat yang **dibandingkan** dengan asuransi |
| Konsekuensi | Layar perbandingan memfilter sumber datanya ke daftar penyedia asuransi saja sebelum dirender |

---

### `BUI-DEC-005` — Penyaringan Asuransi yang Sedang Dipakai dari Daftar Pembanding

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Asuransi yang sedang menjadi penjamin aktif pasien tidak muncul lagi sebagai pilihan pada daftar pembanding** (contoh: pasien memakai Allianz → Allianz tidak muncul di daftar pembanding, hanya asuransi lain yang tampil) |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 5 |
| Assumption | Kunci pencocokan yang dipakai adalah **identitas penyedia asuransi (provider) pada polis aktif pasien saat ini** — bukan nomor polis atau plan spesifik. Bila pasien punya lebih dari satu polis aktif dari provider berbeda, seluruh provider yang sedang aktif dikecualikan, bukan hanya salah satu |
| Konsekuensi | Daftar pembanding difilter terhadap `providerId` (atau field setara) milik polis aktif pasien sebelum ditampilkan |

---

### `BUI-DEC-006` — Tata Letak Payment Method pada Edit Asuransi

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Payment method pada Edit Asuransi diubah dari susunan vertikal (Tunai / Asuransi / Penjamin Perusahaan bertumpuk) menjadi satu baris horizontal tiga tombol** |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 6 |
| Konsekuensi | Murni perubahan tata letak, nol perubahan pada opsi yang tersedia (tetap tiga: Tunai, Asuransi, Penjamin Perusahaan) atau perilaku saat dipilih. Gaya tombol (ukuran, warna aktif) `DEV_DISCRETION` mengikuti pola tombol existing |

---

### `BUI-DEC-007` — Aturan Default Status Tagihan Mengikuti Coverage Backend

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Default status yang tersorot pada Edit Status Tagihan MUST mengikuti hasil evaluasi coverage per item dari backend, dengan aturan: default "Asuransi" hanya bila SELURUH item layanan pada tagihan berstatus tercover. Bila ada satu saja item yang tidak tercover, default jatuh ke "Pribadi".** Nilai ini **tidak boleh di-hardcode** berdasarkan jenis penjamin yang terdaftar di pendaftaran pasien |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada `/grill-me` 24 September 2026, opsi "Seluruh item harus tercover baru default Asuransi", dari tiga opsi yang diajukan (direkomendasikan) |
| Alasan | Cocok dengan contoh yang diberikan owner sendiri: pasien Allianz tapi seluruh layanan tidak tercover → default MUST "Pribadi", bukan "Asuransi". Mencegah piutang salah tercatat ke penjamin padahal semestinya ditagih ke pasien |
| Contoh | Tagihan punya 3 item: item A dan B tercover Allianz, item C tidak tercover. Default status = **Pribadi** (karena tidak seluruh item tercover), bukan Asuransi. Bila ketiganya tercover, default = **Asuransi** |
| Konsekuensi | Frontend membaca field status coverage **per item** dari response backend (bukan flag tunggal di level invoice) dan menghitung sendiri apakah seluruhnya tercover sebelum menentukan default. Bila backend belum mengembalikan status coverage per item pada endpoint yang dipakai layar ini, itu **gap yang MUST dicatat saat trace**, bukan diselesaikan dengan menebak di frontend |

---

### `BUI-DEC-008` — Optimasi Tata Letak Card Billing dan Card Status Tagihan

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Card Billing dan Card Status Tagihan dirapikan: kurangi whitespace kosong, layout lebih compact, datatable dipindah naik ke atas.** Responsivitas MUST dipertahankan |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 8 |
| Konsekuensi | Murni penataan ulang komponen yang sudah ada (`Pertahankan component existing`), bukan redesign. Susunan elemen persis `DEV_DISCRETION` mengikuti prinsip di atas |

---

### `BUI-DEC-009` — Section Catatan Penting Berbentuk Timeline

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menambahkan section "Catatan Penting" berbentuk timeline**, menampilkan seluruh note/catatan pasien secara berurutan (contoh alur yang diberikan owner: Kiosk → Admisi → IGD → Rawat Inap) |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 9 |
| Konsekuensi | Bentuk visual timeline (vertikal/horizontal, ikon per tahap) `DEV_DISCRETION`. Sumber data note per pasien MUST diverifikasi saat trace — dicatat sebagai open question bila belum ada endpoint konsolidasi lintas modul (`BUI-OQ-02`) |

---

### `BUI-DEC-010` — Kewajiban Upload Memo Dokter TTD pada Diskon Dokter

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Upload Memo Dokter TTD wajib diisi untuk SETIAP pengajuan diskon dokter, tanpa kecuali.** Submit form diskon dokter MUST ditolak di sisi frontend selama memo belum diunggah |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada `/grill-me` 24 September 2026, opsi "Wajib untuk setiap pengajuan diskon dokter, tanpa kecuali", dari tiga opsi yang diajukan (direkomendasikan) |
| Alasan | Paling aman untuk audit — setiap potongan pendapatan dokter selalu berjejak persetujuan tertulis, tanpa perlu aturan ambang nominal atau jenis diskon yang bisa berubah-ubah dan sulit dijaga konsisten |
| Konsekuensi | Component upload (file + preview + remove) menjadi field wajib pada form, tervalidasi sebelum tombol submit aktif. Validasi ini murni di frontend; validasi ulang di backend **di luar scope** pass ini dan MUST dicatat sebagai rekomendasi terpisah, karena validasi UI saja tidak mencegah pengajuan lewat jalur API langsung |

---

### `BUI-DEC-011` — Refundable Credit Dihapus dari UI, Backend Tidak Disentuh

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Field, card, dan kalkulasi refundable credit dihilangkan dari UI.** Ini murni penyembunyian tampilan — backend MUST TETAP menghitung dan menyimpan nilai refundable credit apa adanya, tidak ada perubahan pada logika atau skema backend |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada `/grill-me` 24 September 2026, opsi "Murni sembunyikan di UI, backend tetap jalan apa adanya", dari dua opsi yang diajukan (direkomendasikan) |
| Alasan | Sesuai instruksi eksplisit owner "ubah hanya bagian yang diperlukan, pertahankan component existing". Menjaga scope pass ini tetap murni frontend — tidak berisiko ke modul lain yang mungkin masih membaca field refundable credit dari response yang sama |
| Konsekuensi | Frontend berhenti merender field/card/kalkulasi ini dan berhenti memanggilnya sebagai bagian tampilan, tetapi **tidak** menghapus field dari payload yang diterima maupun mengubah request ke backend |

---

### `BUI-DEC-012` — Dua Sumber pada Modal Ajukan Refund, Alur Persetujuan Tidak Berubah

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Modal Ajukan Refund dirapikan dan diberi radio pilihan sumber: Billing atau Deposito.** Pilih **Billing** → tampil datatable/selectable item billing (kolom: checkbox, nama item, tanggal, nominal), mendukung multi-select per item utuh (bukan sebagian nominal per item), total refund terhitung otomatis dari item terpilih. Pilih **Deposito** → tampil sisa deposito, nominal terisi otomatis. **Pemilihan sumber ini murni menentukan item/nominal yang diajukan — alur persetujuan refund (siapa approve, berapa tahap) MUST tetap identik dengan yang sudah berjalan sekarang untuk kedua sumber** |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada `/grill-me` 24 September 2026, opsi "Murni menentukan item/nominal, alur persetujuan tetap identik untuk kedua sumber", dari dua opsi yang diajukan (direkomendasikan) |
| Alasan | Risiko paling kecil — tidak mengubah kontrak backend approval yang sudah ada. Konsisten dengan sifat permintaan ini sebagai revisi UI, bukan modul baru dengan alur persetujuan baru |
| Konsekuensi | Frontend hanya mengubah **bentuk pengumpulan data pengajuan** (item terpilih vs nominal deposito), request yang dikirim ke endpoint pengajuan refund yang sudah ada MUST tetap kompatibel dengan bentuk yang diterima backend saat ini — diverifikasi saat trace |

---

### `BUI-DEC-013` — Pemindahan Tombol Aksi ke Halaman Riwayat Pembayaran

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Tombol Ajukan Refund, Ajukan Adjustment, dan Ajukan Write-Off dipindahkan dari halaman detail lama ke bagian Aksi pada halaman Riwayat Pembayaran.** Ketiga tombol **tidak lagi tampil** di halaman detail yang lama |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Permintaan langsung owner, 24 September 2026, poin 13 |
| Batas | Ini murni pemindahan **lokasi tombol beserta pemicu aksinya** (modal/handler yang sama dipindah tempat pemanggilannya). Kewenangan siapa yang boleh melihat/menekan ketiga tombol ini **tidak berubah** dari yang berlaku sekarang |
| Konsekuensi | Halaman detail lama kehilangan tiga tombol ini tetapi konten lain di halaman itu tidak tersentuh. Halaman Riwayat Pembayaran MUST punya section/area "Aksi" tempat ketiganya dipasang |

---

## Frontend Decision Authority — Amendment Ini

| Decision ID | Area | Owner | Status | Allowed range | Evidence |
|---|---|---|---|---|---|
| `BUI-DEC-001` | Layout filter tanggal | Developer | `approved` | Mengikuti pola filter existing di layar Billing | `BUI-DEC-001` |
| `BUI-DEC-006` | Gaya tombol payment method | Developer | `approved` | Mengikuti pola tombol existing | `BUI-DEC-006` |
| `BUI-DEC-008` | Susunan elemen card compact | Developer | `approved` | Component existing dipertahankan, hanya ditata ulang | `BUI-DEC-008` |
| `BUI-DEC-009` | Bentuk visual timeline | Developer | `approved` | Vertikal/horizontal, bebas dipilih | `BUI-DEC-009` |

---

## Decision Log — Amendment Ini

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `BUI-DEC-001` | Decision | Filter Tanggal Awal/Akhir pada layar Billing | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 1 |
| `BUI-DEC-002` | Decision | Default data: invoice hari ini, status OPEN | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 2 |
| `BUI-DEC-003` | Decision | Label "Drug" → "Obat / Medicine" (UI label saja) | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 3 |
| `BUI-DEC-004` | Decision | Perbandingan asuransi hanya tampilkan Insurance provider | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 4 |
| `BUI-DEC-005` | Decision | Asuransi aktif pasien dikecualikan dari daftar pembanding | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 5 |
| `BUI-DEC-006` | Decision | Payment method Edit Asuransi jadi satu baris horizontal | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 6 |
| `BUI-DEC-007` | Decision | Default status tagihan: seluruh item tercover baru default Asuransi | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 7 + pilihan `/grill-me` |
| `BUI-DEC-008` | Decision | Card Billing/Status Tagihan dibuat compact | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 8 |
| `BUI-DEC-009` | Decision | Section Catatan Penting berbentuk timeline | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 9 |
| `BUI-DEC-010` | Decision | Memo Dokter TTD wajib untuk setiap diskon dokter | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 10 + pilihan `/grill-me` |
| `BUI-DEC-011` | Decision | Refundable credit: hapus UI saja, backend tidak disentuh | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 11 + pilihan `/grill-me` |
| `BUI-DEC-012` | Decision | Modal refund dua sumber, alur persetujuan tidak berubah | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 12 + pilihan `/grill-me` |
| `BUI-DEC-013` | Decision | Tombol aksi dipindah ke Riwayat Pembayaran, dihapus dari halaman lama | Yasmin | `approved` | Yasmin, 24 Sept 2026 | Poin 13 |

---

## Acceptance Criteria — Amendment Ini

- `BUI-AC-01`: Layar Billing dibuka tanpa filter apa pun diterapkan pengguna → data yang tampil adalah invoice tanggal hari ini berstatus `OPEN` (`BUI-DEC-002`).
- `BUI-AC-02`: Filter Tanggal Awal/Tanggal Akhir diterapkan → hanya invoice pada rentang itu yang tampil, menggantikan default hari ini (`BUI-DEC-001`).
- `BUI-AC-03`: Seluruh teks berlabel "Drug" pada layar Billing dan turunannya terbaca "Obat / Medicine"; nilai data master obat (nama obat) tidak berubah (`BUI-DEC-003`).
- `BUI-AC-04`: Modal perbandingan asuransi menampilkan daftar penyedia asuransi saja — tidak ada pilihan "pribadi" atau "perusahaan" pada daftar itu (`BUI-DEC-004`).
- `BUI-AC-05`: Pasien dengan polis aktif Allianz membuka daftar pembanding → Allianz tidak muncul di daftar; provider lain tetap muncul (`BUI-DEC-005`).
- `BUI-AC-06`: Edit Asuransi menampilkan tiga tombol payment method sejajar dalam satu baris, bukan bertumpuk (`BUI-DEC-006`).
- `BUI-AC-07`: Pasien Allianz dengan seluruh item layanan berstatus tidak tercover membuka Edit Status Tagihan → status default yang tersorot adalah "Pribadi" (`BUI-DEC-007`).
- `BUI-AC-08`: Pasien dengan seluruh item layanan berstatus tercover membuka Edit Status Tagihan → status default yang tersorot adalah "Asuransi" (`BUI-DEC-007`).
- `BUI-AC-09`: Card Billing dan Card Status Tagihan tetap responsive pada breakpoint mobile/tablet/desktop setelah dirapikan (`BUI-DEC-008`).
- `BUI-AC-10`: Section Catatan Penting menampilkan seluruh note pasien tersusun sebagai timeline berurutan waktu (`BUI-DEC-009`).
- `BUI-AC-11`: Form diskon dokter tanpa memo terunggah → tombol submit tertahan/tertolak dengan pesan yang menyebut memo wajib (`BUI-DEC-010`).
- `BUI-AC-12`: Form diskon dokter dengan memo terunggah, lalu memo dihapus (remove) sebelum submit → submit kembali tertahan (`BUI-DEC-010`).
- `BUI-AC-13`: Tidak ada field, card, atau angka refundable credit yang tampil di layar mana pun pada modul ini (`BUI-DEC-011`).
- `BUI-AC-14`: Modal Ajukan Refund dengan sumber "Billing" dipilih, dua item dicentang → total refund yang tampil adalah penjumlahan nominal kedua item, otomatis dan tanpa input manual (`BUI-DEC-012`).
- `BUI-AC-15`: Modal Ajukan Refund dengan sumber "Deposito" dipilih → nominal terisi otomatis sebesar sisa deposito, field nominal tidak dapat diketik manual melebihi sisa itu (`BUI-DEC-012`).
- `BUI-AC-16`: Halaman detail lama tidak lagi menampilkan tombol Ajukan Refund/Adjustment/Write-Off (`BUI-DEC-013`).
- `BUI-AC-17`: Halaman Riwayat Pembayaran menampilkan ketiga tombol itu pada area Aksi dan memicu modal/alur yang sama seperti sebelum dipindah (`BUI-DEC-013`).

---

## Open Questions — Amendment Ini

| ID | Pertanyaan | Pemilik jawaban | Status |
|---|---|---|---|
| `BUI-OQ-01` | Apakah benar cakupan poin 3 (rename "Drug") hanya label UI, atau termasuk nilai data master obat yang ditampilkan? | Yasmin | Diasumsikan **label saja** (`BUI-DEC-003`), MUST dikonfirmasi sebelum implementasi bila asumsi ini keliru |
| `BUI-OQ-02` | Apakah sudah ada endpoint yang mengonsolidasikan seluruh note pasien lintas tahap (Kiosk/Admisi/IGD/Rawat Inap) untuk mengisi Catatan Penting, atau perlu agregasi baru? | Backend/API Owner | Terbuka — dijawab saat `/trace-existing-capabilities` |
| `BUI-OQ-03` | Apakah endpoint yang dipakai Edit Status Tagihan saat ini sudah mengembalikan status coverage **per item**, atau baru flag tunggal per invoice? | Backend/API Owner | Terbuka — menentukan apakah `BUI-DEC-007` murni frontend atau perlu perluasan response backend |
| `BUI-OQ-04` | Apakah daftar "Insurance provider" pada perbandingan (`BUI-DEC-004`) menampilkan seluruh provider di master data, atau hanya yang punya kontrak/tarif aktif dengan rumah sakit? | Yasmin / Insurance Owner | Terbuka — tidak memblokir desain UI, tapi memengaruhi sumber data yang dipanggil |
| `BUI-OQ-05` | Apakah endpoint pengajuan refund yang sudah ada saat ini menerima bentuk payload "daftar item terpilih" dan "nominal dari deposito", atau perlu penyesuaian bentuk request? | Backend/API Owner | Terbuka — dijawab saat `/trace-existing-capabilities`; bila perlu penyesuaian, itu dependency lintas modul, bukan scope frontend murni |

Tidak satu pun open question di atas memblokir `/trace-existing-capabilities` untuk dimulai —
seluruhnya dijawab bukti source code, bukan keputusan bisnis yang masih menunggu manusia.

## Amendment lanjutan 24 September 2026 — Penutupan `BUI-CQ-02`, `BUI-CQ-03`, `BUI-CQ-04`

Dipicu temuan `/trace-existing-capabilities` (`01-existing-capability-map.md` bagian 23.4):
backend yang sudah berjalan memakai aturan default status berbeda dari `BUI-DEC-007`, dan
field `PaymentMethodRow` yang cocok untuk `BUI-DEC-006` ternyata belum pernah dipakai frontend.
Ketiga closure question ditutup di sesi yang sama, sebelum `/design-business-module` dimulai.

---

### `BUI-DEC-014` — Penegasan Aturan "Seluruh Item Tercover" dan Otorisasi Perbaikan Backend

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menutup `BUI-CQ-02` dan `BUI-CQ-03`.** Aturan `BUI-DEC-007` ("default Asuransi hanya jika SELURUH item layanan tercover") DIPERTAHANKAN apa adanya, termasuk untuk kasus coverage SEBAGIAN — bukan hanya kasus nol/seluruh tercover. **Perbaikan logika backend pada `BillingPayerEditService.cs:145` DIOTORISASI sebagai bagian pass revisi UI ini**: kondisi `anyItemCoveredByInsurance` (default Asuransi bila SATU item tercover) MUST diganti logika "SELURUH item aktif tercover" sebelum `suggestedBillingStatus` bernilai `"INSURANCE"`. Otorisasi ini **terbatas pada perubahan kondisi/logika di baris itu** — TIDAK mencakup perubahan skema database, DTO, endpoint, atau migration |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada closure pass 24 September 2026: "Tetap SELURUH item harus tercover baru default Asuransi" (menutup `BUI-CQ-02`), diikuti "Otorisasi sekarang — perbaikan logika saja, bagian dari pass ini" (menutup `BUI-CQ-03`) |
| Alasan | Konsisten dengan tujuan awal `BUI-DEC-007`: mencegah piutang salah tercatat ke penjamin pada kasus coverage sebagian — justru kasus yang paling sering terjadi pada asuransi swasta, bukan kasus tepi yang jarang muncul |
| Konsekuensi | `BE-BUI-*` (task backend, menyusul dari `/plan-module-delivery`) MUST mencakup perbaikan baris ini sebagai prasyarat sebelum `BUI-DEC-006`/`BUI-DEC-007` dianggap selesai di frontend — keduanya bergantung pada nilai `suggestedBillingStatus`/`effectivePaymentType` yang benar. Ini SATU-SATUNYA titik pass ini yang menyentuh backend; seluruh 12 keputusan lain murni frontend |
| Trace | `01-existing-capability-map.md` bagian 23.2 `CAP-BUI-07`, bagian 23.4 |

---

### `BUI-DEC-015` — Sumber Data Tombol Payment Method: `PaymentMethodRow`

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menutup `BUI-CQ-04`.** Tiga tombol payment method horizontal (`BUI-DEC-006`) MUST dirender dari field `PaymentMethodRow` pada response `GET /{id}/edit-context` (sudah berisi `Code`, `Label` Indonesia, `IsSelected`, `IsEnabled`) — **bukan** dari `BasePayerCategorySelector`/`panel.categories` yang saat ini dipakai `edit-asuransi-panel.jsx` untuk keperluan lain (pemilihan kandidat pembanding) |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada closure pass 24 September 2026: "Pakai PaymentMethodRow dari server" |
| Alasan | Konsisten dengan prinsip yang owner tegaskan sendiri di poin 7 ("Jangan hardcode. Gunakan response backend."). Menghindari dua logika kategori payer yang berjalan sejajar (satu di server via `PaymentMethodRow`, satu lagi tersirat di hook client `useEditAsuransiPanel`) yang berisiko drift |
| Batas | `BasePayerCategorySelector` **tetap dipakai apa adanya** untuk keperluan aslinya (memilih KANDIDAT saat mengganti penjamin/perbandingan, `BUI-DEC-004`/`005`) — keputusan ini HANYA mengatur sumber data untuk blok "Payment Method" tiga tombol pada `BUI-DEC-006`, dua kebutuhan UI yang berbeda secara fungsi walau sama-sama ada di layar Edit Asuransi |
| Konsekuensi | `IsSelected` pada `PaymentMethodRow` otomatis benar begitu `BUI-DEC-014` (perbaikan backend) selesai — frontend tidak perlu menghitung ulang status terpilih sendiri |
| Trace | `01-existing-capability-map.md` bagian 23.2 `CAP-BUI-06b` |

---

### Status Penutupan

| ID | Status sebelumnya | Status sekarang |
|---|---|---|
| `BUI-CQ-02` | Terbuka | **Tertutup** — `BUI-DEC-014` |
| `BUI-CQ-03` | Terbuka | **Tertutup** — `BUI-DEC-014` |
| `BUI-CQ-04` | Terbuka | **Tertutup** — `BUI-DEC-015` |
| `BUI-CQ-05` (Catatan Penting, cakupan) | Terbuka | **Tetap terbuka** — tidak memblokir desain (`BUI-DEC-009` boleh didesain dengan scope dipersempit) |
| `BUI-CQ-06` (mekanisme upload memo) | Terbuka | **Tetap terbuka** — tidak memblokir desain (`BUI-DEC-010` boleh didesain dengan kontrak upload ditandai `TBD`) |

**Tidak ada lagi closure question yang memblokir `/design-business-module`.** Seluruh
`BUI-DEC-001`–`015` `approved`. Satu titik sentuh backend (`BUI-DEC-014`) dicatat eksplisit
supaya tidak lolos diam-diam sebagai "revisi frontend murni".

---

## Amendment 25 September 2026 — Shift Kasir: Blocking Selisih Kas dan Status Tindak Lanjut (reopening `BKC-DEC-038`)

**Trigger.** Pemilik modul membawa dokumen `Shift Kasir (3).md` (artifact requirement pihak
ketiga, 14 capability/19 rule/7 flow) dan meminta `/grill-me` untuk memeriksa gap terhadap
`BKC-DEC-038` (`approved`, sebelumnya ditandai "tidak dibuka ulang" — lihat Bagian "Di luar
scope" di atas). Sebelum bertanya, dilakukan pembacaan source aktual
(`Areas/HealthServices/BillingManagement/Cashier/{Models,Services}/*.cs`) untuk memverifikasi
klaim dokumen terhadap implementasi nyata, bukan menduga.

**`BKC-DEC-038` TIDAK dibatalkan.** Amendment ini menambah dua penegakan yang sebelumnya hanya
tertulis sebagai niat (`BKC-DEC-038`/dokumen baru) tapi belum benar-benar dijalankan kode.
Seluruh keputusan `BKC-DEC-038` yang lain (buka shift dengan saldo awal, close mencatat
system/physical/variance, handover dua kasir, late noncash settlement tidak mengubah physical
cash shift tertutup) tetap berlaku apa adanya.

### Fact — klaim dokumen `Shift Kasir (3).md` yang sudah terjawab source, tidak perlu keputusan baru

| Klaim/pertanyaan dokumen | Bukti source | Kesimpulan |
| --- | --- | --- |
| Bagian 12 butir 2 — "Apakah serah terima menutup shift lama dan membuka shift baru, atau memindahkan penanggung jawab dalam shift yang sama?" | `CashierShiftService.HandoverAsync` (baris 401-460): shift sumber → status `HANDED_OVER` (terminal, `ClosedAt` diisi); shift BARU dibuat untuk kasir penerima, `OpeningCash` = `OpeningCash` lama + `SystemCash` lama (saldo dibawa maju); ditautkan lewat `BilCashierShiftHandover.SourceShiftId`/`ReceivingShiftId` | **Sudah terjawab**: menutup shift lama + membuka shift baru. Tidak perlu keputusan baru |
| Bagian 12 butir 3 — "Apakah aksi finansial Petty Cash memakai Shift Kasir?" | `PC-DEC-001` (`approved`): "Petty Cash TIDAK terhubung ke kas fisik Shift Kasir manapun" | **Sudah terjawab** oleh keputusan lain yang sudah `approved`. Di luar scope amendment ini |
| RULE-004/CAP-005 dokumen — "Status shift hanya `OPEN` dan `CLOSED`" | `BilCashierShift.cs`: enum `CashierShiftStatuses` = `OPEN`, `HANDED_OVER`, `CLOSED`, `CLOSED_WITH_VARIANCE`, `REVIEWED`, `REOPENED` (enam nilai, bukan dua) | **Konflik** — dokumen tidak akurat terhadap implementasi. Model status existing (enam nilai) yang berlaku; dokumen dianggap salah pada poin ini, bukan sistem yang perlu disederhanakan |
| BP-003 dokumen — hasil rekonsiliasi `SESUAI`/`KURANG`/`LEBIH` (tiga nilai) | `CashierShiftService.CloseAsync` (baris 541-544): `Variance` disimpan sebagai `decimal` bertanda (negatif = kurang, positif = lebih); status hanya dua jalur (`Closed` bila `Variance == 0`, `ClosedWithVariance` bila tidak) | **Bukan gap** — arah selisih (kurang/lebih) sudah terbaca dari tanda `Variance`, dan `RULE-010` dokumen sendiri memperlakukan `KURANG`/`LEBIH` SAMA (sama-sama `Menunggu Verifikasi`). Tidak perlu status terpisah untuk arah selisih |

### Gap ditemukan — belum diimplementasikan, DIKONFIRMASI ditutup lewat pass ini

---

#### `BKC-DEC-123` — Shift `CLOSED_WITH_VARIANCE` yang Belum Direview Memblokir Shift Berikutnya

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Gap implementasi ditemukan dan ditutup**: `CashierShiftService.OpenAsync` (baris 67-71) saat ini HANYA menolak pembukaan shift baru bila kasir/register memiliki shift berstatus `OPEN` atau `REOPENED` (`CashierShiftStatuses.IsActive`). Shift berstatus `CLOSED_WITH_VARIANCE` yang BELUM direview supervisor TIDAK dianggap aktif, sehingga TIDAK memblokir — bertentangan dengan `RULE-012`/`BP-007` dokumen `Shift Kasir (3).md` ("Menunggu Verifikasi dan Perlu Tindak Lanjut memblokir shift berikutnya") dan semangat `BKC-DEC-038` (variance direview Kepala Kasir sebelum shift dianggap tuntas). **Diputuskan**: `OpenAsync` MUST ditambah pengecekan — tolak pembukaan shift baru bila kasir ATAU register yang sama memiliki shift berstatus `CLOSED_WITH_VARIANCE` **atau** `PERLU_TINDAK_LANJUT` (lihat `BKC-DEC-124`) yang belum berstatus `REVIEWED`. Pesan penolakan mengikuti pola existing: `"Kasir atau register masih memiliki shift yang menunggu review selisih kas."` |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada sesi `/grill-me` 25 September 2026: "Implementasikan blocking-nya sekarang (Direkomendasikan)" atas pertanyaan yang menyertakan bukti baris kode `OpenAsync` persis |
| Alasan | Kontrol pertanggungjawaban kas adalah tujuan inti `BKC-DEC-038` — membiarkan kasir/register membuka shift baru sementara selisih shift sebelumnya belum diperiksa membuat kontrol itu longgar secara struktural, bukan sekadar celah kecil |
| Konsekuensi | `CashierShiftService.OpenAsync` MUST diubah (query tambahan atas `BilCashierShifts` untuk status `CLOSED_WITH_VARIANCE`/`PERLU_TINDAK_LANJUT`); pesan error baru; TIDAK ada perubahan schema/migration (status sudah berupa `string` bebas, hanya menambah nilai konstanta baru pada `CashierShiftStatuses` untuk `BKC-DEC-124`). Task implementasi menyusul lewat `build-module-backend` setelah blueprint/roadmap Shift Kasir diperbarui (`/design-business-module` atau `/plan-module-delivery`, sesuai kebutuhan) |
| Trace | `Shift Kasir (3).md` RULE-012, BP-007; `BKC-DEC-038`; source `CashierShiftService.cs` baris 67-71 (bukti gap), 496-551 (`CloseAsync`, asal status `CLOSED_WITH_VARIANCE`) |

---

#### `BKC-DEC-124` — Status `PERLU_TINDAK_LANJUT` Terpisah dari `REVIEWED` pada Review Variance

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Gap implementasi ditemukan dan ditutup**: `CashierShiftService.ReviewVarianceAsync` (baris 553-670) saat ini HANYA punya satu hasil — begitu supervisor mengisi `Resolution`/`Reason`, status langsung berubah ke `REVIEWED` (lepas blokir sepenuhnya). Tidak ada cara bagi supervisor menyatakan "sudah diperiksa, tapi belum tuntas, tetap perlu ditindaklanjuti" — bertentangan dengan `BP-005`/`RULE-011` dokumen (`Perlu Tindak Lanjut` sebagai status terpisah dari `Terverifikasi`, wajib catatan, dan baru menjadi `Terverifikasi` setelah tindak lanjut selesai). Tanpa status ini, keputusan `BKC-DEC-123` (blocking) jadi longgar — supervisor bisa "mereview" sekadar formalitas dan langsung melepas blokir tanpa benar-benar menuntaskan selisih. **Diputuskan**: tambah nilai `CashierShiftStatuses.PerluTindakLanjut` (`"PERLU_TINDAK_LANJUT"`). `ReviewVarianceAsync` MUST menerima parameter hasil review (mis. `outcome`: `Verified` atau `NeedsFollowUp`) yang menentukan status akhir (`REVIEWED` vs `PERLU_TINDAK_LANJUT`) — bukan selalu `REVIEWED`. Diperlukan SATU aksi susulan baru (nama tentatif `CompleteFollowUpAsync`/`ResolveFollowUpAsync`, ditentukan saat desain) yang memindahkan shift dari `PERLU_TINDAK_LANJUT` ke `REVIEWED` setelah tindak lanjut benar-benar selesai, mewajibkan catatan penyelesaian |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Pilihan eksplisit owner pada sesi `/grill-me` 25 September 2026: "Tambah status PERLU_TINDAK_LANJUT terpisah dari REVIEWED (Direkomendasikan)" |
| Alasan | Sejalan langsung dengan `BKC-DEC-123` — blocking tanpa kemampuan menahan status "belum tuntas" adalah kontrol kosong; supervisor butuh cara membedakan "selesai" dari "masih menggantung" |
| Konsekuensi | Nama pasti aksi susulan, bentuk request (field wajib catatan penyelesaian), dan wewenang siapa yang boleh menjalankannya (Kepala Kasir sama seperti `ReviewVarianceAsync`, atau berjenjang ke Manajemen sesuai `RULE-014`/`BKC-DEC-038` soal keputusan penyelesaian selisih) **belum ditentukan pada pass ini** — dicatat sebagai `BKC-OQ-104` di bawah, TIDAK memblokir implementasi `BKC-DEC-123` (blocking bisa berjalan lebih dulu dengan hanya `REVIEWED` sebagai status pelepas, `PERLU_TINDAK_LANJUT` menyusul) tapi MUST diselesaikan sebelum task aksi-susulan itu sendiri diimplementasikan |
| Trace | `Shift Kasir (3).md` BP-005, RULE-011; `BKC-DEC-038`; source `CashierShiftService.cs` baris 553-670 (`ReviewVarianceAsync`, bukti hanya satu hasil) |

---

### Open Question — tidak memblokir `BKC-DEC-123`/`124`, perlu diputuskan sebelum task terkait dimulai

| ID | Pertanyaan | Owner | Status |
|---|---|---|---|
| `BKC-OQ-104` | Aksi susulan penyelesaian `PERLU_TINDAK_LANJUT` (`BKC-DEC-124`): siapa yang berwenang menjalankannya (Kepala Kasir sama seperti review awal, atau eskalasi ke Manajemen bila nominal selisih melewati ambang tertentu — `RULE-014` dokumen menyebut keputusan penyelesaian selisih ada di Manajemen, tapi tidak merinci ambang), dan field apa saja yang wajib diisi pada penyelesaiannya? | Yasmin / Kepala Kasir / Finance Operations | Terbuka — memblokir implementasi aksi susulan `BKC-DEC-124`, TIDAK memblokir `BKC-DEC-123` (blocking berbasis `CLOSED_WITH_VARIANCE`/`REVIEWED` saja bisa jalan lebih dulu) |
| `BKC-OQ-105` | Dokumen `Shift Kasir (3).md` Bagian 12 butir 1 dan 4 (daftar field/format/pesan error lengkap tiap form; matriks hak akses rinci per aksi bukan hanya per menu) — apakah perlu diputuskan formal pada pass `/grill-me` terpisah, atau cukup mengikuti konvensi `role-access-rules.md`/`[AccessAction]` per-endpoint yang sudah baku di seluruh modul (yang secara struktural SUDAH memberi hak akses per-aksi, bukan per-menu, tanpa perlu matriks tertulis terpisah)? | Yasmin | Terbuka — tidak memblokir `BKC-DEC-123`/`124`. Butir field/pesan error (butir 1) murni detail UI, `DEV_DISCRETION` mengikuti pola form existing kecuali owner ingin mengunci teks tertentu |
| `BKC-OQ-106` | Bagian 10 dokumen (Rekomendasi Aktivitas Tanpa Shift Aktif) eksplisit ditandai REKOMENDASI dari rujukan Permenkes/SATUSEHAT, bukan ketentuan terkunci. Apakah matriks itu (mis. tolak pencatatan Petty Cash tanpa shift `OPEN`) mau dikunci sebagai `RULE` resmi Shift Kasir, atau dibiarkan sebagai rekomendasi non-mengikat? | Yasmin | Terbuka — tidak memblokir `BKC-DEC-123`/`124`; berkaitan dengan modul Petty Cash yang sudah punya keputusan sendiri (`PC-DEC-*`), bukan Shift Kasir murni |

---

### `BKC-DEC-125` — Wewenang dan Field Penyelesaian `PERLU_TINDAK_LANJUT`

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menutup `BKC-OQ-104`.** Aksi susulan yang memindahkan shift dari `PERLU_TINDAK_LANJUT` ke `REVIEWED` (`BKC-DEC-124`) dijalankan oleh **Supervisor/Kepala Kasir** — wewenang SAMA dengan `ReviewVarianceAsync` awal, TIDAK eskalasi ke Manajemen sebagai syarat penyelesaian aksi ini (keterlibatan Manajemen pada `RULE-014`/`BKC-DEC-038` tetap berlaku sebagai kebijakan umum penentuan *hasil* penyelesaian selisih — misalnya siapa menanggung nominal — bukan syarat *siapa yang boleh menekan tombol selesai* pada sistem). Field wajib pada aksi penyelesaian: `VerificationNote` (catatan penyelesaian, wajib diisi), `VerifiedBy` (diisi otomatis dari actor yang menjalankan aksi), `VerifiedDate` (diisi otomatis waktu aksi dijalankan) |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Jawaban eksplisit owner: "Penyelesaian `PERLU_TINDAK_LANJUT` dilakukan Supervisor/Kepala Kasir dengan field `VerificationNote`, `VerifiedBy`, `VerifiedDate`" |
| Alasan | Konsisten dengan wewenang review variance awal (`ReviewVarianceAsync`) yang sudah dipegang Supervisor/Kepala Kasir — tidak menambah jenjang approval baru untuk aksi lanjutan atas kasus yang sama |
| Konsekuensi | Aksi susulan (nama tentatif `CompleteFollowUpAsync`/`ResolveFollowUpAsync`, dikunci saat desain) menerima request dengan field `VerificationNote` wajib; `VerifiedBy`/`VerifiedDate` TIDAK dikirim client, diisi server dari `actorUserId`/waktu transaksi — pola sama dengan `ReviewedAt`/`ReviewerId` pada `BilCashVarianceReview` yang sudah ada. Kemungkinan field ini disimpan pada `BilCashVarianceReview` yang sudah ada (menambah kolom) atau baris review kedua — keputusan model data persis menyusul di `design-business-module` |
| Trace | Menutup `BKC-OQ-104`; `Shift Kasir (3).md` BP-005 butir 3-5; `BKC-DEC-124`; source `BilCashVarianceReview.cs` (pola `ReviewerId`/`ReviewedAt` existing yang diikuti) |

---

### `BKC-DEC-126` — Tanpa Matriks Hak Akses Terpisah; Pesan Error Standar Field Wajib

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menutup `BKC-OQ-105`.** Tidak dibuat matriks hak akses per-aksi terpisah untuk Shift Kasir — cukup mengikuti konvensi `role-access-rules.md`/`[AccessAction]`+`[AccessPermission]` per-endpoint yang sudah baku di seluruh modul (setiap endpoint baru pada `BKC-DEC-123`/`124`/`125` WAJIB tetap diberi `[AccessAction]`/`[AccessPermission]` seperti aksi existing, sesuai kontrak yang sudah mengikat, bukan pengecualian). Pesan error standar untuk field wajib yang belum diisi pada form Buka/Tutup/Serah Terima Shift: **`"{Nama Field} wajib diisi."`** — pola yang sudah direkomendasikan dokumen (Bagian 5 "Form Shift") kini dikunci sebagai ketentuan, bukan rekomendasi |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Jawaban eksplisit owner: "Tidak perlu matriks hak akses terpisah; gunakan `AccessAction` per endpoint. Pesan error standar `{Field} wajib diisi.`" |
| Alasan | Matriks tertulis terpisah akan menduplikasi apa yang sudah ditegakkan otomatis oleh `[AccessAction]`/layar Manajemen Role → Akses Role; menjaga satu sumber kebenaran wewenang (kode + layar admin), bukan dua |
| Konsekuensi | `BKC-OQ-105` ditutup penuh — tidak ada artefak matriks tambahan yang perlu dibuat. Pesan error `"{Nama Field} wajib diisi."` MUST dipakai konsisten di seluruh validasi field wajib form Shift Kasir (backend maupun frontend), menggantikan status "rekomendasi UX" pada dokumen asli |
| Trace | Menutup `BKC-OQ-105`; `Shift Kasir (3).md` Bagian 5 "Form Shift", RULE-005; `role-access-rules.md` (konvensi `[AccessAction]`/`[AccessPermission]` yang sudah mengikat) |

---

### `BKC-DEC-127` — Bagian 10 Dokumen Tetap Rekomendasi; Aksi Finansial Tetap Wajib Shift `OPEN`

| Field | Isi |
|---|---|
| Type | Decision |
| Item | **Menutup `BKC-OQ-106`.** Matriks Bagian 10 dokumen (Rekomendasi Aktivitas Tanpa Shift Aktif — akses baca/administratif Invoice & Billing Kasir, Riwayat Pembayaran, Petty Cash tanpa shift `OPEN`) **TETAP berstatus rekomendasi non-mengikat**, TIDAK dikunci menjadi `RULE` resmi Shift Kasir pada pass ini. Yang TETAP wajib (bukan keputusan baru, konfirmasi ulang atas `RULE-002`/`RULE-019` yang sudah `approved`): **aksi finansial apa pun** (penerimaan pembayaran, refund, pengeluaran/pemasukan kas, koreksi finansial, settlement, atau perubahan posisi kas lainnya) **tetap wajib shift `OPEN`**, terlepas dari menu tempat aksi itu dipicu |
| Owner | Yasmin |
| Status | `approved` |
| Approval evidence | Jawaban eksplisit owner: "Aktivitas tanpa shift `OPEN` tetap rekomendasi, bukan `RULE` resmi. Aksi finansial tetap wajib shift `OPEN`" |
| Alasan | Mengunci rekomendasi berbasis inferensi Permenkes/SATUSEHAT (bukan ketentuan eksplisit regulasi) menjadi `RULE` mengikat berisiko menciptakan kewajiban yang tidak benar-benar berasal dari keputusan bisnis pemilik modul. Batas yang sungguh kritis (aksi finansial wajib shift `OPEN`) sudah cukup ditegakkan lewat `RULE-002`/`RULE-019` yang sudah ada — tidak perlu memperluas cakupan `RULE` resmi ke aktivitas non-finansial |
| Konsekuensi | Implementasi menu Invoice & Billing Kasir/Riwayat Pembayaran/Petty Cash TANPA shift `OPEN` mengikuti Bagian 10 dokumen sebagai PANDUAN desain (bukan gerbang validasi wajib) — pengecualian/penyesuaian pada implementasinya tidak dianggap pelanggaran `RULE`. Validasi shift `OPEN` pada aksi finansial (`RULE-002`/`019`) TIDAK berubah dan tetap ditegakkan seperti sekarang |
| Trace | Menutup `BKC-OQ-106`; `Shift Kasir (3).md` Bagian 10, RULE-002, RULE-017–019 (sudah `approved` sebelumnya, dikonfirmasi ulang di sini) |

---

### Status Penutupan

| ID | Status sebelumnya | Status sekarang |
|---|---|---|
| Dokumen `Shift Kasir (3).md` Bagian 12 butir 2 | Belum ditetapkan | **Tertutup** — terjawab source, lihat tabel Fact di atas |
| Dokumen `Shift Kasir (3).md` Bagian 12 butir 3 | Belum ditetapkan | **Tertutup** — terjawab `PC-DEC-001` |
| Gap blocking shift berikutnya (RULE-012/BP-007) | Tidak terdeteksi sebelumnya | **Tertutup** — `BKC-DEC-123` |
| Gap hasil review variance tunggal (BP-005/RULE-011) | Tidak terdeteksi sebelumnya | **Tertutup** — `BKC-DEC-124` |
| Dokumen `Shift Kasir (3).md` Bagian 12 butir 1 dan 4 | Belum ditetapkan | **Tertutup** — `BKC-DEC-126` |
| Bagian 10 dokumen (rekomendasi tanpa shift) | Rekomendasi, belum dikunci | **Tertutup** — `BKC-DEC-127` (tetap rekomendasi, dikonfirmasi sengaja tidak dikunci) |
| Aksi susulan penyelesaian `PERLU_TINDAK_LANJUT` | Baru muncul dari `BKC-DEC-124` | **Tertutup** — `BKC-DEC-125` |

**Seluruh open question amendment ini (`BKC-OQ-104`–`106`) sudah tertutup** lewat
`BKC-DEC-125`–`127`. `BKC-DEC-123`–`127` cukup untuk memulai implementasi backend penuh:
perubahan `CashierShiftService.OpenAsync` (blocking), `ReviewVarianceAsync` (dua hasil), aksi
susulan baru (penyelesaian follow-up dengan `VerificationNote`/`VerifiedBy`/`VerifiedDate`),
serta `[AccessAction]`/`[AccessPermission]` standar pada seluruh endpoint yang tersentuh. Tidak
ada open question tersisa yang memblokir. Belum ada task roadmap resmi untuk perubahan ini —
langkah berikutnya lihat penutup pass di bawah.
