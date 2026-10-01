# Finance Management — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian V2 — Sistem Informasi Rumah Sakit |
| Modul | Finance Management (`finance-management`), kode modul `FIN` |
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Contract version | `FIN-MVP-1.4` — `locked` 26 September 2026 (revisi 5 disetujui Yasmin bersama `FIN-DES-045`..`050`) |
| `last_changed_in` | `FIN-MVP-1.4` — 25 September 2026, AMENDMENT REVISI 5: `FR-FIN-085`, `088`, `095` diperbarui; `FR-FIN-096`..`099` baru (`FIN-DEC-057`..`062`). Sebelumnya `FIN-MVP-1.3` — 25 September 2026: (1.2) AMENDMENT REVISI 4 (`EPIC FIN-15`, `FIN-16`, `FIN-17` baru — Purchasing/AP, AR Invoice Agregat, Potongan AR; `FR-FIN-081`..`095`; `EPIC FIN-07` digantikan `EPIC FIN-15`); (1.3) `FIN-DEC-056` mempersempit gerbang `FIN-OQ-020` — `EPIC FIN-15` kini boleh masuk `/plan-module-delivery` tanpa menunggu ratifikasi Accounting, hanya worker pengiriman kode PPN Masukan yang tertahan |
| Status | `approved` dan `locked` untuk `MVP-0`..`MVP-5` (cakupan disetujui Yasmin 20 September 2026; revisi 1.1 dikunci 25 September 2026). **Bagian AMENDMENT REVISI 4 (`EPIC FIN-15`-`17`, `FIN-MVP-1.3`) disetujui dan dikunci Yasmin 25 September 2026** bersama `FIN-DES-037`..`044`, sebelum `/plan-module-delivery` dijalankan |
| Penguncian kontrak | Enam kontrak turunan dan dokumen ini dikunci ke `1.0` oleh owner 20 September 2026. Tiga permukaan tetap TIDAK terkunci karena bergantung pihak luar: `BilCollectionHandoff`, perluasan `BilArHandoff` untuk manfaat karyawan, dan pengiriman kejadian ke Accounting |
| Repository target | `NewQuilvianSystemBackend` (branch `Yasmina`), `QuilvianSystemFrontendDev` |
| Commit SHA baseline | Backend `09101d05`, Frontend `abed49b03` |
| Ringkasan cakupan | Rilis pertama membangun rantai utuh dari uang yang diterima kasir sampai piutang lunas, beserta kotak keluar kejadian ke Accounting yang terisi tetapi belum dikirim |

## 2. Ringkasan eksekutif

Hari ini rumah sakit sudah bisa menerima uang dari pasien — Billing dan kasir sudah berjalan
penuh. Yang belum ada adalah **buku keuangannya**: tidak ada satu pun tempat di sistem yang
mencatat berapa piutang yang masih menggantung di penjamin, berapa uang yang sudah masuk hari
ini, dan berapa yang sudah disetor ke bank.

Akibatnya, pertanyaan sesederhana "penjamin A masih berutang berapa?" hanya bisa dijawab dengan
membuka Excel di luar sistem. Setiap jawaban seperti itu adalah jawaban yang tidak dapat
diaudit.

Rilis pertama modul ini menutup satu rantai sampai selesai: uang yang dibayar pasien di kasir
tercatat sebagai penerimaan, sisa tanggungan penjamin tercatat sebagai piutang, penjamin
membayar, piutang lunas — seluruhnya di dalam sistem, dan setiap rupiahnya dapat ditelusuri
balik ke tagihan asalnya.

Yang sengaja **belum** dikerjakan rilis pertama: utang supplier, utang dokter, dan pengiriman
jurnal otomatis ke Accounting. Ketiganya punya alasan yang berbeda, dan seluruhnya dijelaskan
pada bagian 8.

## 3. Masalah produk

### 3.1 Yang sudah ada

Diverifikasi langsung dari source pada `09101d05`:

| Sudah ada | Bukti |
|---|---|
| Tagihan, kasir, tender, alokasi pembayaran, shift kasir | `BilInvoice`, `BilTender`, `BilSettlement`, `BilPaymentAllocation`, `BilCashierShift` |
| Fakta piutang dan utang dokter yang disiapkan Billing saat finalisasi | `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment` |
| Kas kecil lengkap: kategori, anggaran per periode, ledger mutasi | `MstPettyCashCategory`, `FinPettyCashBudget`, `FinPettyCashBudgetMovement`, 9 endpoint aktif |
| Master supplier lengkap dengan termin dan limit kredit | `MstSupplier` milik Administrator |
| Master aturan fee dokter | `MstDoctorServiceRule` |
| Buku besar, jurnal, periode, aturan posting | Modul Accounting, approved revisi 11 |

### 3.2 Yang belum ada

| Belum ada | Bukti |
|---|---|
| Konsumen di Finance untuk fakta Billing | Pencarian `BilArHandoff` di `Areas/Corporate` — nol hasil |
| Piutang, penerimaan, alokasi | Pencarian `FinReceivable`, `FinReceipt` — nol hasil |
| Utang supplier dan utang dokter | Pencarian `FinSupplierPayable`, `FinDoctorPayable` — nol hasil |
| Setoran bank dan kas harian | Pencarian `FinBankDeposit`, `FinDailyCashSnapshot` — nol hasil |
| Kotak keluar kejadian Accounting | Pencarian `FinAccountingEventOutbox` — nol hasil |
| Kontrak fakta penerimaan dari Billing | Pencarian `BilCollectionHandoff` — nol hasil |
| Modul Medical Fee dan hasil fee yang disetujui | Pencarian `DoctorServiceFee` dan folder `*MedicalFee*` — nol hasil (`FIN-CAP-021`) |
| Modul Purchasing | Pencarian `PurchaseOrder`, `ReceiveOrder`, `SupplierInvoice` — nol hasil |
| Layar Finance selain kas kecil | `src/app/finance/` hanya berisi dua rute kas kecil |

### 3.3 Akibat nyata dari keadaan sekarang

1. Uang yang sudah diterima kasir tidak punya catatan di sisi keuangan — hanya ada di Billing.
2. Tanggungan penjamin yang belum dibayar tidak tercatat sebagai piutang di mana pun.
3. Tidak ada satu pun kejadian keuangan yang sampai ke Accounting, sehingga buku besar tidak
   pernah menerima transaksi operasional.
4. Rekonsiliasi kas kasir dengan setoran bank dikerjakan manual.

## 4. Visi produk

Rantai keterhubungan yang ingin dicapai, ditulis sebagai urutan:

1. Pasien dilayani, Billing menghitung tagihan.
2. Pasien membayar di kasir; Billing mencatat tender yang berhasil.
3. **Finance mencatat penerimaan** dengan nomor tender, kwitansi, dan shift kasirnya.
4. Billing memfinalisasi tagihan dan menyiapkan fakta piutang untuk sisa tanggungan penjamin.
5. **Finance mencatat piutang** — hanya untuk sisa yang memang belum dibayar, tidak pernah untuk
   jumlah yang sudah masuk di langkah 3.
6. Penjamin membayar; **Finance mengalokasikan** pembayaran itu ke piutang yang dipilih petugas.
7. Piutang lunas; sisa menjadi nol.
8. **Setiap langkah di atas menulis satu kejadian** ke kotak keluar, siap dikirim ke Accounting.
9. Kas hari itu ditutup; uang tunai disetor ke bank dengan nominal yang tidak mungkin melebihi
   kas yang benar-benar ada.
10. Accounting menerima kejadian dan menjurnalnya — **langkah ini belum aktif di rilis pertama**.

Kunci dari seluruh rantai ini ada di langkah 3 dan 5: uang yang sudah dibayar **tidak pernah**
dicatat ulang sebagai piutang.

## 5. Batas MVP

### 5.1 Titik mulai

1. Data induk Finance terisi: sekurang-kurangnya satu mata uang `IDR`, satu bank, satu rekening
   bertipe `COLLECTION`, dan satu rekening bertipe `PAYMENT`.
2. Billing sudah menghasilkan `BilArHandoff` saat memfinalisasi tagihan — sudah berjalan hari
   ini.
3. Billing sudah menyediakan fakta penerimaan (`BilCollectionHandoff`) sesuai kontrak pada
   `integration-contract.md` bagian 2.
4. Enam folder submodul baru sudah terdaftar di registry kepemilikan modul.

### 5.2 Titik akhir

1. Satu tagihan pasien berpenjamin dapat berjalan utuh: pasien membayar sebagian di kasir, sisa
   tanggungan penjamin menjadi piutang, penjamin melunasi, piutang berstatus lunas.
2. Jumlah tagihan dapat dibuktikan sama dengan penerimaan ditambah piutang ditambah koreksi yang
   disetujui — tanpa satu rupiah pun terhitung dua kali.
3. Koreksi dan penghapusan piutang hanya dapat disetujui pengguna selain pengajunya.
4. Kas harian dapat ditutup, dan setoran bank tidak mungkin melebihi kas yang benar-benar ada.
5. Setiap fakta keuangan di atas sudah menulis satu baris kejadian di kotak keluar, siap dikirim
   begitu Accounting membuka pintunya.
6. Tidak ada layar Finance yang memiliki COA, jurnal, atau periode akuntansi.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di dalam MVP |
|---|---|
| Finance AR Staff | Memantau fakta masuk dari Billing, melengkapi berkas klaim, mengalokasikan penerimaan ke piutang, mengajukan koreksi dan penghapusan |
| Finance Supervisor | Menyetujui atau menolak koreksi dan penghapusan piutang yang diajukan orang lain |
| Cashier/Treasury | Memposting setoran bank, menutup kas harian |
| Finance Admin | Mengelola bank, rekening, dan mata uang |
| Auditor/Manager | Menelusuri piutang dan penerimaan kembali ke tagihan asalnya, tanpa dapat mengubah apa pun |
| Accounting Staff | Memantau kejadian yang tertahan di kotak keluar; **tidak** memiliki hak apa pun atas subledger Finance |

Peran **di luar** MVP: Finance AP Staff dan Medical Fee Admin — keduanya baru berperan saat
rumpun utang dikerjakan.

## 7. Pemilihan kemampuan MVP

Setiap kemampuan diuji dua pertanyaan: tanpa ini, apakah satu kasus nyata bisa selesai dari awal
sampai akhir? Kalau tidak, adakah jalan sementara yang aman dan dapat diaudit?

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Data induk Finance: bank, rekening, mata uang | `FIN-CAP-009` (`Missing`) | Wajib; tanpa rekening, setoran dan pembayaran tidak punya tujuan |
| Konsumsi fakta piutang dari Billing | `FIN-CAP-001`, `FIN-CAP-008` | Wajib; ini satu-satunya pintu masuk piutang |
| Piutang: daftar, rincian, umur piutang | `FIN-CAP-009` | Wajib; tanpa ini tidak ada buku piutang sama sekali |
| Penerimaan dari tender kasir | `FIN-CAP-004`, `FIN-CAP-007`, `FIN-CAP-009` | Wajib; tanpa ini uang yang masuk tidak punya catatan keuangan |
| Pembagian bayar-vs-piutang | `FIN-CAP-005`, `FIN-CAP-009` | Wajib; inilah yang mencegah dobel-hitung, dan tidak ada jalan manual yang aman untuk ini |
| Alokasi penerimaan ke piutang | `FIN-CAP-009` | Wajib; tanpa ini piutang tidak pernah bisa lunas |
| Koreksi dan penghapusan piutang berjenjang | `FIN-CAP-003`, `FIN-CAP-009` | Wajib; piutang yang tidak dapat dikoreksi akan dikoreksi di luar sistem |
| Setoran bank dan kas harian | `FIN-CAP-006`, `FIN-CAP-009` | Wajib; uang tunai yang masuk harus punya jalur keluar yang terkendali |
| Kotak keluar kejadian Accounting | `FIN-CAP-009`, `FIN-CAP-018` | Wajib; `FIN-DEC-004` mengharuskan kotak keluar ada sejak penerimaan pertama dibuat, walau pengirimannya belum aktif |

## 8. Kemampuan yang ditunda

Setiap baris menyebut **alasan bersebab** dan **pengganti selama MVP berjalan**. Menunda tanpa
pengganti berarti membuat pengguna kehilangan pekerjaan yang selama ini bisa dilakukan.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Utang supplier dan pembayarannya, kini siklus Purchasing/AP penuh (`EPIC FIN-15`) | `FIN-CAP-026`..`036` | **Diperbarui 25 September 2026.** Rantai PO-terima barang-faktur **sudah dirancang** (`02-backend-architecture.md` AMENDMENT REVISI 4), sehingga alasan "tidak ada rantai yang dapat divalidasi" sudah tidak berlaku lagi. Yang masih menahan: tetap `POST-MVP` per keputusan wave sebelumnya, belum ada permintaan owner memajukan urutan gelombang. **Gerbang `FIN-OQ-020` sudah dipersempit** (`FIN-DEC-056`) — tidak lagi menahan epic ini masuk `/plan-module-delivery`, hanya menahan aktivasi worker pengiriman kode `PPN-MASUKAN-PEMBELIAN` | Proses yang berjalan hari ini tetap dipakai. Jalur input manual `FinSupplierPayable` tetap ada sebagai fallback permanen (`FIN-DES-040`), bukan hanya sementara |
| Utang dokter dan pembayaran rekapnya | `FIN-CAP-021` (`Missing`) | **Ketergantungan eksternal keras.** `FinDoctorPayable` memakai `SourceDoctorServiceFeeId` sebagai kunci, dan `DoctorServiceFee` belum ada satu baris pun — modul Medical Fee belum dibangun | Perhitungan dan pembayaran fee dokter tetap berjalan seperti sekarang. Desainnya sudah lengkap dan siap dikerjakan begitu Medical Fee ada |
| Pengiriman kejadian ke Accounting | `FIN-CAP-018` (`Missing`) | Endpoint penerima di Accounting belum dibangun, diverifikasi langsung ke source | Kejadian tetap **ditulis** ke kotak keluar sejak MVP, sehingga tidak ada satu pun fakta keuangan yang hilang. Begitu endpoint ada, seluruh antrean tinggal dikirim |
| Saldo subledger per periode | `FIN-CAP-009` | Nama field pesannya belum dikonfirmasi Accounting (`FIN-OQ-011`), dan fiturnya baru berguna setelah pengiriman kejadian aktif | Rekonsiliasi periode dikerjakan manual seperti sekarang |
| Penyelarasan frontend kas kecil ke rute Finance | `FIN-CAP-015`, `FIN-CAP-016` | Halaman kas kecil **sudah berfungsi penuh** di rute lama; memindahkannya tidak menambah kemampuan apa pun bagi pengguna | Pengguna tetap memakai halaman kas kecil yang ada sekarang, termasuk voucher di rute `health-services/billing-management` |
| Piutang manfaat karyawan | `FIN-CAP-001` | Menunggu konfirmasi Billing **dan** HR atas `FIN-DEC-006` dan `FIN-DEC-016`. Kolomnya sudah disiapkan sehingga skema tidak perlu diubah lagi nanti | Piutang pegawai dicatat sebagai piutang penjamin biasa, dengan catatan tertulis pada keterangan — dapat diaudit walau belum dikelompokkan per pegawai |

## 9. Alur bisnis target

**`FLOW-FIN-MVP-001` — Dari pasien membayar sampai piutang penjamin lunas**

**Tujuan.** Setiap rupiah yang menjadi tanggungan rumah sakit tercatat tepat satu kali, dan
dapat ditelusuri balik ke tagihan asalnya.

**Pelaku.** Kasir (Billing), Finance AR Staff, Finance Supervisor, Treasury.

**Pemicu.** Pasien membayar di loket kasir.

**Prasyarat.** Tagihan sudah terbentuk di Billing; data induk Finance sudah terisi.

**Langkah utama:**

1. Kasir menerima pembayaran pasien; Billing mencatat tender berhasil.
2. Billing menyiapkan fakta penerimaan berisi nomor tender, nominal, kwitansi, dan shift kasir.
3. Finance mengolah fakta itu dan membuat satu penerimaan. Bila tagihan belum final, kejadian
   akuntansinya ditahan — uangnya tetap tercatat penuh di Finance.
4. Billing memfinalisasi tagihan dan menyiapkan fakta piutang untuk sisa tanggungan penjamin.
5. Finance mengolah fakta itu dan membuat piutang **hanya sebesar sisa tanggungan** — bukan
   sebesar seluruh tagihan.
6. Petugas AR melengkapi berkas klaim. Kelengkapan berkas **tidak** menahan pengakuan piutang.
7. Penjamin membayar. Petugas AR mencatat penerimaan dan memilih sendiri piutang mana yang
   dilunasi beserta nominalnya.
8. Sisa piutang berkurang. Bila mencapai nol, piutang berstatus lunas.
9. Treasury menutup kas hari itu dan menyetor uang tunai ke bank.

**Aturan bisnis yang berlaku:**

- Jumlah yang sudah dibayar pasien **tidak pernah** dicatat ulang sebagai piutang.
- Nilai dari Billing disalin apa adanya; Finance tidak menghitung ulang.
- Koreksi dan penghapusan piutang wajib disetujui pengguna selain pengajunya.
- Setoran bank tidak boleh melebihi kas yang benar-benar tersedia.

**Perubahan status:** lihat `contracts/state-transition-matrix.md`.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
|---|---|
| Fakta dari Billing gagal diolah | Tercatat berstatus `ERROR` beserta pesannya; dapat diulang tanpa membuat catatan ganda |
| Fakta yang sama terkirim dua kali | Ditolak kunci idempotensi; tidak ada piutang atau penerimaan kedua |
| Tender dibatalkan Billing setelah dialokasikan | Sistem membuat baris pembalik dan piutang **otomatis** terbuka kembali |
| Pasien membayar sebelum tagihan final | Penerimaan tetap dicatat; kejadian akuntansinya ditahan sampai tagihan final |
| Dua petugas menyetor bersamaan | Satu berhasil, satu ditolak; saldo tidak pernah negatif |
| Transaksi masuk setelah kas ditutup | Masuk ke tanggal berikutnya; angka kemarin tidak berubah |

**Hasil akhir.** Piutang berstatus lunas, penerimaan teralokasi penuh, kas harian tertutup, dan
setiap langkah sudah menulis kejadian di kotak keluar.

## 10. Epic dan functional requirement

### EPIC FIN-01 — Fondasi data induk Finance

**Tujuan.** Menyediakan bank, rekening, dan mata uang yang dipakai seluruh rumpun lain.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-001` — Rekening bank bertipe jelas**
> Sistem menyimpan rekening dengan tipe `OPERATIONAL`, `COLLECTION`, atau `PAYMENT`.
> **Contoh:** rekening `1234567890` atas nama RS Sehat Sentosa di Bank Contoh ditandai
> `COLLECTION`. Saat Treasury membuat setoran, hanya rekening bertipe `COLLECTION` yang muncul
> sebagai pilihan tujuan.

> **`FR-FIN-002` — Nomor rekening tidak boleh ganda dalam satu bank**
> **Contoh:** rekening `1234567890` di Bank Contoh sudah ada. Petugas menambahkan nomor yang
> sama di bank yang sama → ditolak `409`. Nomor sama di bank berbeda → diterima.

> **`FR-FIN-003` — Satu mata uang dasar**
> **Contoh:** `IDR` sudah ditandai sebagai mata uang dasar. Petugas menandai `USD` juga sebagai
> dasar → ditolak; hanya satu yang boleh.

> **`FR-FIN-004` — Data induk yang sudah dipakai dinonaktifkan, bukan dihapus**
> **Contoh:** rekening `1234567890` sudah dipakai tiga setoran. Petugas menekan Hapus → ditolak;
> yang tersedia adalah menonaktifkan, sehingga riwayat setoran tetap dapat dibaca.

### EPIC FIN-02 — Pintu masuk fakta dari Billing

**Tujuan.** Menerima fakta piutang dan penerimaan dari Billing tepat satu kali, dengan kegagalan
yang terlihat.
**Disposisi backend:** `MISSING / NEW` untuk konsumen; `EXISTING / REUSE` untuk sumbernya
(`BilArHandoff`); `EXTEND` untuk `BilCollectionHandoff` yang dibangun tim Billing

> **`FR-FIN-010` — Satu fakta hanya diolah satu kali**
> **Contoh:** fakta piutang dengan `HandoffKey` `a1b2…` diolah dan menghasilkan piutang
> `AR-2026-09-00871`. Fakta dengan `HandoffKey` yang sama dikirim lagi → dibalas `409`, dan
> jumlah baris piutang tetap satu.

> **`FR-FIN-011` — Kegagalan pengolahan terlihat, bukan hilang**
> **Contoh:** fakta piutang datang menyebut penjamin yang tidak dikenal. Pengolahan gagal;
> barisnya tersimpan berstatus `ERROR` dengan pesan yang menyebut penyebabnya. Tidak ada piutang
> setengah jadi yang tersimpan.

> **`FR-FIN-012` — Fakta yang gagal dapat diulang**
> **Contoh:** setelah penjamin didaftarkan, petugas menekan Ulangi pada baris `ERROR` tadi.
> Piutang terbentuk, status menjadi `CONSUMED`, dan penghitung pengulangan bertambah satu.

> **`FR-FIN-013` — Fakta yang sudah berhasil tidak dapat diulang**
> **Contoh:** petugas menekan Ulangi pada baris berstatus `CONSUMED` → ditolak `422`. Tanpa
> aturan ini, satu tagihan dapat melahirkan dua piutang.

### EPIC FIN-03 — Buku piutang

**Tujuan.** Menyediakan daftar, rincian, dan umur piutang yang dapat ditelusuri ke tagihan.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-020` — Piutang dibuat hanya sebesar sisa tanggungan**
> **Contoh:** tagihan Rp 5.000.000; pasien membayar Rp 1.500.000 di kasir; tanggungan penjamin
> Rp 3.500.000. Piutang yang terbentuk Rp 3.500.000, **bukan** Rp 5.000.000.

> **`FR-FIN-021` — Nilai piutang selalu seimbang**
> **Contoh:** piutang Rp 3.500.000 sudah dilunasi Rp 1.000.000 dan dikoreksi turun Rp 500.000.
> Sisa Rp 2.000.000. Jumlah 2.000.000 + 1.000.000 + 500.000 = 3.500.000 — selalu sama dengan
> nilai aslinya.

> **`FR-FIN-022` — Umur piutang memakai empat kelompok**
> **Contoh:** piutang jatuh tempo 15 Juli 2026 dilihat pada 20 September 2026 — sudah lewat 67
> hari, masuk kelompok "61-90 hari". Piutang yang jatuh tempo tepat 30 hari lalu masuk "0-30",
> bukan "31-60".

> **`FR-FIN-023` — Berkas klaim tidak menahan pengakuan piutang**
> **Contoh:** fakta piutang penjamin datang tanpa satu pun berkas. Piutang **tetap** terbentuk
> berstatus `OUTSTANDING` dengan status klaim `INCOMPLETE`. Nilainya tercatat penuh sejak hari
> pertama.

> **`FR-FIN-024` — Piutang dapat ditelusuri ke tagihan asalnya**
> **Contoh:** dari piutang `AR-2026-09-00871`, petugas dapat melihat nomor tagihan, daftar
> pasien, dan kunjungan penyusunnya.

### EPIC FIN-04 — Piutang manfaat karyawan

**Disposisi backend:** `OPEN DECISION` — menunggu konfirmasi Billing dan HR atas `FIN-DEC-006`
dan `FIN-DEC-016`.

**Epic ini MUST NOT masuk gelombang pengiriman mana pun** sampai keputusannya turun. Kolom
`BenefitOwnerId` dan `BenefitRelationship` sudah disiapkan pada skema sehingga tidak diperlukan
perubahan tabel saat epic ini dibuka.

### EPIC FIN-05 — Penerimaan dan pembagian bayar-vs-piutang

**Tujuan.** Mencatat uang yang benar-benar diterima, dan memastikan uang itu tidak pernah
dicatat ulang sebagai piutang.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-030` — Satu tender berhasil menghasilkan satu penerimaan**
> **Contoh:** tender `T-9001` berhasil sebesar Rp 1.500.000. Fakta penerimaannya dikirim dua
> kali karena gangguan jaringan. Yang tersimpan tetap satu penerimaan Rp 1.500.000; kiriman
> kedua dibalas `409`.

> **`FR-FIN-031` — Pasien yang membayar lunas tidak melahirkan piutang**
> **Contoh:** tagihan Rp 500.000 dibayar lunas tunai. Yang terbentuk: satu penerimaan
> Rp 500.000. Piutang yang terbentuk: **nol**.

> **`FR-FIN-032` — Nominal disalin apa adanya dari kasir**
> **Contoh:** tender tercatat Rp 1.500.000 di Billing. Penerimaan di Finance juga Rp 1.500.000
> — Finance tidak menghitung ulang dari tarif atau dari sisa tagihan.

> **`FR-FIN-033` — Penerimaan tunai wajib menyebut shift kasirnya**
> **Contoh:** fakta penerimaan tunai datang tanpa nomor shift → ditolak `422`. Tanpa nomor
> shift, rekonsiliasi kas tidak mungkin dilakukan.

> **`FR-FIN-034` — Penerimaan sebelum tagihan final tetap tercatat**
> **Diperbarui 25 September 2026 (`FIN-DEC-030`).** Contoh lama (kejadiannya ditahan berstatus
> `HELD_FOR_FINALIZATION` sampai tagihan final) **tidak lagi berlaku**.
> **Contoh yang berlaku:** pasien membayar Rp 1.000.000 saat tagihan masih terbuka. Penerimaan
> langsung terlihat di Finance, dan kejadian akuntansinya **langsung siap kirim** dengan jenis
> `PENERIMAAN-UANG-MUKA` — dibukukan Accounting sebagai Uang Muka Pasien, bukan sebagai
> pendapatan. Setelah tagihan final dan uang muka itu dipakai melunasi piutangnya, terbit
> kejadian **baru** jenis `PEMAKAIAN-UANG-MUKA-DEPOSIT`; kejadian penerimaan yang pertama tidak
> diubah. Alasannya: uangnya sudah ada di kasir sejak diterima, jadi menahan jurnalnya hanya
> membuat kas buku besar berselisih dari kas fisik saat tutup buku.

> **`FR-FIN-035` — Total tagihan dapat dibuktikan tidak dobel**
> **Contoh:** tagihan Rp 5.000.000 = penerimaan kasir Rp 1.500.000 + piutang penjamin
> Rp 3.500.000. Sistem dapat menampilkan ketiga angka itu berdampingan untuk satu tagihan.

### EPIC FIN-06 — Pelunasan, koreksi, dan penghapusan piutang

**Tujuan.** Menutup piutang lewat pelunasan, koreksi, atau penghapusan yang seluruhnya terkendali.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-040` — Alokasi dipilih petugas, bukan dicocokkan otomatis**
> **Contoh:** penjamin membayar Rp 5.000.000 untuk tiga piutang. Petugas menentukan sendiri
> Rp 2.000.000 untuk piutang A, Rp 2.000.000 untuk B, dan Rp 1.000.000 untuk C. Sistem tidak
> pernah membagi sendiri berdasarkan piutang tertua.

> **`FR-FIN-041` — Alokasi tidak boleh melebihi uang yang ada**
> **Contoh:** penerimaan Rp 5.000.000 sudah dialokasikan Rp 4.000.000. Petugas mengalokasikan
> Rp 1.500.000 lagi → ditolak `422` dengan pesan menyebut sisa Rp 1.000.000.

> **`FR-FIN-042` — Alokasi tidak boleh melebihi sisa piutang**
> **Contoh:** sisa piutang B Rp 2.000.000. Petugas mengalokasikan Rp 3.000.000 → ditolak dengan
> pesan yang menyebut sisa sebenarnya.

> **`FR-FIN-043` — Pengaju tidak boleh menyetujui permohonannya sendiri**
> **Contoh:** Petugas Rina mengajukan koreksi turun Rp 500.000. Rina menekan Setujui → ditolak
> `422` dengan pesan "Pengaju tidak boleh menyetujui permohonannya sendiri." Petugas Dimas yang
> berwenang menyetujui → diterima, dan sisa piutang baru berubah saat itu.

> **`FR-FIN-044` — Nilai piutang belum berubah selama permohonan belum diputus**
> **Contoh:** koreksi Rp 500.000 masih berstatus diajukan. Sisa piutang tetap seperti semula.

> **`FR-FIN-045` — Pembalikan tidak menghapus riwayat**
> **Contoh:** tender yang sudah dialokasikan ke piutang dibatalkan Billing. Sistem membuat baris
> alokasi pembalik; piutang kembali berstatus belum lunas. Baris alokasi yang lama tetap
> terbaca.

> **`FR-FIN-046` — Piutang lunas tidak dapat dihapusbukukan**
> **Contoh:** piutang berstatus lunas diajukan penghapusan → ditolak `422`.

### EPIC FIN-07 — Utang supplier

**Digantikan `EPIC FIN-15` (AMENDMENT REVISI 4, 25 September 2026).** Audit `Keuangan.md`
menemukan bahwa "input faktur supplier" yang dibayangkan epic ini sesungguhnya bagian akhir dari
siklus Purchasing/AP penuh yang sebelumnya tidak diketahui cakupannya (`FIN-DEC-045`). Jalur
input manual yang epic ini rancang **tetap ada** sebagai fallback (`FIN-DES-040`), tetapi
desainnya sekarang tercakup di `EPIC FIN-15`, bukan berdiri sendiri di sini.

**Disposisi backend:** `MISSING / NEW` — **ditunda ke `POST-MVP`**, lihat bagian 8.

### EPIC FIN-08 — Utang jasa tenaga medis

**Diperbarui revisi 2, 20 September 2026.** Semula bernama "Utang dokter". Setelah `MF-DEC-002`
memperluas penerima jasa menjadi dokter **dan** tenaga kesehatan lain, dan `MF-DEC-008`
menggantikan `FinDoctorPayable` dengan `FinMedicalServicePayable`, epic ini berganti nama dan
cakupan.

**Disposisi backend:** `MISSING / NEW` dengan ketergantungan eksternal keras pada modul Medical
Fee (`FIN-CAP-021`) — **tetap ditunda ke `POST-MVP`**.

Ketergantungannya tidak berubah: `FinMedicalServicePayable` memakai `SourceMedicalServiceFeeId`
sebagai kunci, dan hasil jasa yang disetujui itu baru ada setelah modul Medical Fee dibangun.

### EPIC FIN-09 — Pembayaran keluar

**Disposisi backend:** `MISSING / NEW` — **ditunda ke `POST-MVP`**, karena tidak ada utang yang
dapat dibayar sebelum `EPIC FIN-07` atau `FIN-08` ada.

**Diperluas revisi 2, 20 September 2026.** Karena `MF-DEC-005` memutuskan Medical Fee
menyerahkan jasa **kotor**, epic ini kini juga mencakup potongan dan tambahan pada pembayaran
(`FIN-DES-026`, `FIN-DES-027`). Dua functional requirement ditambahkan:

> **`FR-FIN-050` — Uang yang keluar berbeda dari utang yang lunas**
> Sistem menyimpan dua angka terpisah: jumlah utang yang dilunasi, dan uang yang benar-benar
> ditransfer.
> **Contoh:** jasa dr. Andi (nama samaran) Rp 24.500.000; potongan PPh 21 Rp 1.225.000, kasbon
> Rp 3.000.000, iuran Rp 100.000; tambahan sitting fee Rp 500.000. Yang ditransfer
> Rp 20.675.000. Ketiga utang jasanya tetap berstatus **lunas penuh**.

> **`FR-FIN-051` — Potongan tidak menyisakan utang**
> Sisa utang jasa berkurang sebesar alokasinya, bukan sebesar uang yang ditransfer.
> **Contoh:** dari contoh di atas, sisa utang dr. Andi menjadi **nol**, bukan Rp 3.825.000.
> Bila sisanya bukan nol, sistem akan menganggap rumah sakit masih berutang padahal
> kewajibannya sudah selesai.

### EPIC FIN-10 — Setoran bank dan kas harian

**Tujuan.** Mengendalikan uang tunai dari laci kasir sampai rekening bank.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-060` — Setoran tidak boleh melebihi kas yang tersedia**
> **Contoh:** kasir menerima tunai Rp 15.000.000 hari itu; Treasury sudah menyetor
> Rp 10.000.000; ada selisih kurang Rp 200.000 yang sudah disahkan. Saldo tersedia
> Rp 4.800.000. Treasury memposting setoran Rp 5.000.000 → ditolak `422` dengan pesan "Nominal
> setoran melebihi kas yang tersedia. Saldo tersedia saat ini Rp 4.800.000."

> **`FR-FIN-061` — Setoran sebagian diperbolehkan**
> **Contoh:** dari saldo tersedia Rp 4.800.000, Treasury menyetor Rp 3.000.000 → diterima; saldo
> tersedia menjadi Rp 1.800.000.

> **`FR-FIN-062` — Saldo dihitung ulang saat posting, bukan saat layar dibuka**
> **Contoh:** dua petugas membuka layar setoran bersamaan, keduanya melihat saldo tersedia
> Rp 4.800.000. Petugas pertama memposting Rp 4.800.000 dan berhasil. Petugas kedua memposting
> Rp 4.800.000 → ditolak, karena saat posting saldo sudah nol. Saldo tidak pernah menjadi
> negatif.

> **`FR-FIN-063` — Kas kecil tidak memengaruhi kas kasir**
> **Contoh:** hari itu ada pencairan kas kecil Rp 500.000. Posisi kas harian kasir **tidak
> berubah sama sekali** — keduanya kolam terpisah.

> **`FR-FIN-064` — Penutupan hari menolak setoran yang belum diposting**
> **Contoh:** masih ada dua setoran berstatus draf. Treasury menutup kas → ditolak `422` dengan
> pesan menyebut jumlahnya.

> **`FR-FIN-065` — Angka yang sudah ditutup dibekukan**
> **Contoh:** kas 19 September sudah ditutup dengan saldo akhir Rp 2.000.000. Ada penerimaan
> terlambat tercatat untuk tanggal itu. Angka 19 September **tidak berubah**; penerimaan itu
> masuk ke 20 September dan terlihat sebagai selisih.

### EPIC FIN-11 — Kotak keluar kejadian Accounting

**Tujuan.** Memastikan tidak ada satu pun fakta keuangan yang hilang sebelum Accounting siap
menerima.
**Disposisi backend:** `MISSING / NEW`

> **`FR-FIN-070` — Fakta dan kejadiannya tersimpan bersama atau batal bersama**
> **Contoh:** penyimpanan piutang gagal di tengah jalan. Yang tersimpan: **nol** piutang dan
> **nol** baris kejadian. Tidak mungkin ada piutang tanpa kejadian, atau sebaliknya.

> **`FR-FIN-071` — Satu fakta menghasilkan satu kejadian**
> **Contoh:** piutang `AR-2026-09-00871` diakui. Yang terbentuk: satu baris kejadian
> `PENGAKUAN-PIUTANG`. Percobaan menulis kejadian kedua untuk transaksi, jenis, dan versi yang
> sama ditolak.

> **`FR-FIN-072` — Koreksi memakai versi baru**
> **Contoh:** piutang di atas dikoreksi. Kejadiannya dikirim dengan versi `2`. Bila dikirim
> dengan versi `1`, Accounting akan membacanya sebagai kiriman ulang dan koreksinya tidak akan
> pernah dijurnal.

> **`FR-FIN-073` — Data pasien tidak ikut ke Accounting**
> **Contoh:** piutang memuat tiga pasien. Payload kejadiannya tidak memuat satu pun nomor rekam
> medis, nama pasien, atau nomor kunjungan — hanya nomor transaksi Finance dan pengenal rantai.

> ~~**`FR-FIN-074` — Kejadian tertahan tidak terkirim**~~
> **DICABUT 25 September 2026** (`FIN-DEC-030` menggantikan `FIN-DEC-004`). Tidak ada lagi
> kejadian yang ditahan; yang membedakan penerimaan pra-final adalah **jenis kejadiannya**, bukan
> status pengirimannya. Penggantinya `FR-FIN-076` di bawah.

> **`FR-FIN-075` — Jasa medis tidak terbukukan dua kali**
> **Contoh:** tagihan memuat jasa medis dokter Rp 3.000.000. Kejadian `PENGAKUAN-PIUTANG`
> **tidak** memuat komponen `JASA_MEDIS`. Komponen itu hanya muncul lewat kejadian
> `PENGAKUAN-HUTANG-DOKTER` setelah fee-nya disetujui.

> **`FR-FIN-076` — Jenis kejadian penerimaan ditentukan status tagihan saat uang diterima**
> *(baru, `FIN-DEC-030`, `FIN-DES-034`)*
> **Contoh:** pasien membayar Rp 5.000.000 saat tagihan masih terbuka → kejadian
> `PENERIMAAN-UANG-MUKA`. Pasien lain membayar Rp 5.000.000 saat tagihannya sudah final →
> kejadian `PENERIMAAN-KASIR`. Keduanya langsung siap kirim. Bila penerimaan pertama kelak
> dibatalkan, pembalikannya `PEMBALIKAN-PENERIMAAN-UANG-MUKA` — **tetap begitu walaupun
> tagihannya sudah final saat pembalikan terjadi**, karena kode pembalikan mengikuti penerimaan
> aslinya.

> **`FR-FIN-077` — Uang titipan pasien tidak pernah terbukukan sebagai pendapatan**
> *(baru, `FIN-DEC-031`, `040`, `041`)*
> **Contoh:** pasien menitipkan deposit Rp 10.000.000, dipakai Rp 7.500.000 untuk tagihannya,
> sisa Rp 2.500.000 dikembalikan tunai. Tiga kejadian terbit dengan jenis berbeda:
> `PENERIMAAN-UANG-MUKA`, `PEMAKAIAN-UANG-MUKA-DEPOSIT`, dan `PENGEMBALIAN-UANG-MUKA`. Yang kedua
> mengurangi piutang tanpa menyentuh kas; yang ketiga mengeluarkan kas. Memakai satu jenis untuk
> keduanya membuat salah satunya pasti salah jurnal.

> **`FR-FIN-078` — Kelebihan bayar menjadi kewajiban ke pasien sejak diakui**
> *(baru, `FIN-DEC-042`)*
> **Contoh:** pasien membayar Rp 500.000 untuk tagihan Rp 450.000. Saat Billing mengakui
> kelebihan Rp 50.000, terbit kejadian `PENGAKUAN-KELEBIHAN-BAYAR` bernilai Rp 50.000 yang
> memindahkannya menjadi Uang Muka Pasien **tanpa menyentuh kas** — kasnya sudah didebit penuh
> saat penerimaan. Bila kelak dikembalikan, terbit `PENGEMBALIAN-UANG-MUKA`.

> **`FR-FIN-079` — Selisih kas shift hanya dikirim setelah disahkan**
> *(baru, `FIN-DEC-043`, `FIN-DES-036`)*
> **Contoh:** shift kasir ditutup dengan kas fisik Rp 30.000 lebih kecil dari catatan sistem.
> Selama selisih itu belum disahkan penyelia, **tidak ada** kejadian yang dikirim. Setelah
> disahkan, terbit `SELISIH-KAS-SHIFT` bernilai `-30.000` dengan tanggal pembukuan **tanggal
> shift**, bukan tanggal pengesahan. Shift yang ditutup tanpa selisih tidak menghasilkan kejadian
> apa pun.

> **`FR-FIN-080` — Saldo subledger dikirim dengan bentuk pesan yang sama**
> *(baru, `FIN-DEC-035`)*
> **Contoh:** saat Finance menutup November, terbit kejadian `SALDO-SUBLEDGER` bernilai
> Rp 425.000.000 untuk akun kontrol piutang, periode `2026-11`, tanggal cut-off 30 November.
> Nilainya **boleh nol atau negatif**. Pesan ini tidak pernah menjadi jurnal — Accounting hanya
> mencocokkannya dengan buku besar. Bila saldonya dinyatakan ulang, versinya naik menjadi `2`.

### EPIC FIN-14 — Uang muka, deposit, kelebihan bayar, dan selisih kas *(baru)*

**Tujuan.** Menutup gerbang cutover `G6` milik Accounting: memastikan uang titipan pasien,
kelebihan bayar, dan selisih kas kasir masuk ke buku besar dengan jenis kejadian yang benar,
tanpa Finance membuat satu pun tabel baru.

**Disposisi backend:** `OPEN DECISION` — tujuh kode kejadian barunya **menunggu ratifikasi
Accounting** (`FIN-OQ-017`). Seluruh bukti teknisnya sudah siap (`FIN-CAP-022`..`025`,
`FIN-DES-029`..`036`), dan hanya satu migration yang dibutuhkan, tetapi nama kode belum boleh
dianggap final sepihak.

**Functional requirement:** `FR-FIN-076` sampai `FR-FIN-080` di atas.

**Epic ini MUST NOT masuk gelombang pengiriman mana pun** sampai `FIN-OQ-017` tertutup. Yang
menahan hanya nama kodenya — bukan pemodelan data, bukan jalur datanya, dan bukan keputusan
bisnisnya.

### EPIC FIN-12 — Pengiriman ke Accounting

**Disposisi backend:** `OPEN DECISION` — endpoint penerima belum dibangun (`FIN-CAP-018`,
diverifikasi ulang belum ada pada `d6cdfaf9`) dan mekanisme autentikasi belum final
(`FIN-OQ-016`; tiga syarat organisasinya sudah ditetapkan `FIN-DEC-036`).

**Epic ini MUST NOT masuk gelombang pengiriman mana pun.**

### EPIC FIN-13 — Penyelarasan frontend kas kecil

**Disposisi backend:** `EXTEND` — **ditunda ke `POST-MVP`**, lihat bagian 8.

### EPIC FIN-15 — Purchasing dan siklus utang supplier penuh *(baru, AMENDMENT REVISI 4)*

**Tujuan.** Menutup rumpun `FIN-SC-008`: Purchase Order sampai Purchasing Invoice tercatat
sebagai utang supplier, lengkap dengan approval berjenjang, retur pembelian, dan laporan —
digantikan dari `EPIC FIN-07` yang semula hanya membayangkan input manual.

**Disposisi backend:** `MISSING / NEW` — **ditunda ke `POST-MVP`**, sama seperti pendahulunya.

**Gerbang dipersempit 25 September 2026 (`FIN-DEC-056`).** Semula `FIN-DEC-046` menuntut seluruh
epic ini menunggu ratifikasi Accounting atas `PPN-MASUKAN-PEMBELIAN` sebelum masuk
`/plan-module-delivery`. Owner meninjau ulang setelah desain arsitektur selesai dan
menyimpulkan: baris kejadian PPN Masukan sudah ditulis `PENDING` sejak Purchasing Invoice
disetujui (`FIN-DES-043`), dan HANYA worker pengirimannya yang tertahan (`FIN-VAL-122`) — pola
yang **identik** dengan `EPIC FIN-11` yang sudah berjalan penuh di `MVP-0`..`MVP-5` walau
endpoint penerima Accounting (`FIN-CAP-018`) sendiri belum ada. Alasan tambahan owner: Finance
adalah **titik asal (upstream)** bagi Accounting, sehingga kesiapan Finance **MUST NOT**
digantungkan pada kecepatan ratifikasi hilir.

**Akibatnya:** `EPIC FIN-15` — PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur
Pembelian, Deposit Retur, approval berjenjang, dan seluruh laporan — **boleh** diteruskan penuh
ke `/plan-module-delivery` **tanpa** menunggu `FIN-OQ-020`. Yang **tetap** tertahan, sempit dan
eksplisit: **aktivasi worker pengiriman kejadian `PPN-MASUKAN-PEMBELIAN`** ke Accounting saja.

> **`FR-FIN-081` — Approval PO dan Purchasing Invoice berjenjang berdasarkan nominal**
> **Contoh:** PO senilai Rp 62.000.000 diajukan Petugas AP. Supervisor Finance mencoba
> menyetujui → ditolak; hanya Manajer Finance yang berhak karena di atas ambang Rp 50.000.000
> (`FIN-DEC-052`). PO senilai Rp 40.000.000 dapat disetujui Supervisor Finance.

> **`FR-FIN-082` — Tukar Faktur adalah checkpoint dokumen terpisah sebelum nilai final**
> **Contoh:** barang diterima 1 November (GR dicatat), dokumen faktur fisik dari supplier baru
> sampai 5 November (Tukar Faktur dicatat saat itu). Estimasi jatuh tempo dihitung otomatis dari
> TOP supplier (30 hari) terhitung sejak 5 November — **bukan** dari tanggal PO maupun tanggal
> barang diterima (`FIN-DEC-051`).

> **`FR-FIN-083` — Satu Tukar Faktur menghasilkan tepat satu Purchasing Invoice**
> **Contoh:** Purchasing Invoice sudah dibuat dari Tukar Faktur `TF-2026-09-00142`. Petugas
> mencoba membuat invoice kedua dari Tukar Faktur yang sama → ditolak, walau dua permintaan
> dikirim hampir bersamaan.

> **`FR-FIN-084` — Purchasing Invoice yang disetujui otomatis menjadi utang supplier**
> **Contoh:** Purchasing Invoice senilai barang Rp 10.000.000 + PPN 11% Rp 1.100.000 disetujui
> Manajer Finance. Seketika itu juga: `FinSupplierPayable` Rp 11.100.000 tercipta, Tukar Faktur
> sumbernya terkunci, dan satu baris kejadian akuntansi PPN Masukan Rp 1.100.000 tersimpan
> berstatus menunggu (belum terkirim — lihat `FR-FIN-087`).

> **`FR-FIN-085` — Retur pembelian menerbitkan kredit yang dapat dipakai lintas invoice**
> *(contoh diperbarui revisi 5, `FIN-DEC-057`)*
> **Contoh:** retur senilai Rp 2.000.000 atas Purchasing Invoice A menerbitkan Deposit Retur
> Rp 2.000.000. Saat petugas menyusun pembayaran untuk utang Purchasing Invoice B (supplier yang
> sama), ia memilih deposit itu sebagai sumber dana sebesar Rp 1.200.000. Utang B lunas penuh;
> uang yang ditransfer berkurang Rp 1.200.000; sisa deposit Rp 800.000 tetap tersedia untuk
> pembayaran berikutnya.

> **`FR-FIN-086` — Jalur input utang supplier manual tetap tersedia**
> **Contoh:** supplier lama yang belum diajak masuk alur PO tetap dapat dicatat utangnya secara
> manual seperti sebelumnya. `SourcePurchasingInvoiceId` baris itu kosong — bukan kegagalan,
> keadaan sah (`FIN-DES-040`).

> **`FR-FIN-087` — Kejadian PPN Masukan tidak terkirim sebelum ratifikasi**
> **Contoh:** Purchasing Invoice pertama rumpun ini disetujui. Baris kejadian
> `PPN-MASUKAN-PEMBELIAN` tersimpan di kotak keluar berstatus menunggu, **tidak** ikut terkirim
> oleh worker walau worker untuk kode lain sudah aktif — sampai Accounting meratifikasi
> (`evidence/06`, `FIN-OQ-020`).

> **`FR-FIN-088` — Laporan Purchasing/AP tidak punya tabel sendiri**
> **Contoh:** Rekap, Laporan Tukar Faktur, Laporan Jatuh Tempo, dan Rekonsiliasi Tagihan
> seluruhnya dihitung langsung dari `FinPurchaseOrder`, `FinInvoiceExchange`, dan
> `FinPurchasingInvoice` yang sudah ada — tidak ada satu pun tabel laporan baru (`FIN-DES-044`).
> **Aging AP tidak dibangun ulang** (`FIN-DEC-059`): layar dan endpoint aging utang supplier yang
> sudah berjalan tetap satu-satunya, dan otomatis memuat utang yang lahir dari Purchasing Invoice.

> **`FR-FIN-096` — Pembayaran boleh dilunasi sebagian atau seluruhnya dari Deposit Retur**
> *(baru, revisi 5, `FIN-DEC-057`, `061`)*
> **Contoh:** pembayaran Rp 10.000.000 memakai deposit Rp 2.500.000. Yang ditransfer
> Rp 7.500.000; kedua utang lunas penuh. Kejadian akuntansi yang terbit: pembayaran utang
> Rp 7.500.000 dan pemakaian deposit Rp 2.500.000 — **bukan** pembayaran utang Rp 10.000.000.
> Bila deposit menutup seluruh pembayaran, tidak ada nomor bukti transfer yang diminta.

> **`FR-FIN-097` — Deposit yang sedang dicadangkan tidak dapat dipakai dua kali**
> *(baru, revisi 5, `FIN-DES-046`)*
> **Contoh:** dua petugas menyusun dua pembayaran berbeda dan memilih deposit Rp 2.500.000 yang
> sama, masing-masing penuh. Yang kedua ditolak karena saldo sudah dicadangkan pembayaran
> pertama. Bila pembayaran pertama kemudian ditolak penyetuju, saldo deposit kembali dan dapat
> dipakai pembayaran lain.

> **`FR-FIN-098` — Retur pembelian tercatat di buku besar sejak dikonfirmasi**
> *(baru, revisi 5, `FIN-DEC-061`)*
> **Contoh:** retur Rp 2.500.000 dikonfirmasi. Satu kejadian `RETUR-PEMBELIAN` Rp 2.500.000
> tersimpan di kotak keluar, menunggu ratifikasi Accounting (`FIN-OQ-026`).

### EPIC FIN-16 — AR Invoice Agregat ke penjamin *(baru, AMENDMENT REVISI 4)*

**Tujuan.** Menutup rumpun `FIN-SC-009`: menggabungkan banyak piutang atas nama penjamin yang
sama menjadi satu dokumen tagihan resmi, tanpa mengubah peran `FinReceivable` sebagai satuan
piutang internal.

**Disposisi backend:** `MISSING / NEW`. Tidak bergantung modul lain — seluruh datanya diambil
dari `FinReceivable` yang sudah ada di dalam Finance sendiri.

> **`FR-FIN-089` — Batch hanya boleh berisi piutang penjamin yang sama**
> **Contoh:** petugas mencoba membuat satu batch berisi piutang PT Asuransi A dan PT Asuransi B
> sekaligus → ditolak. Batch dipisah menjadi dua.

> **`FR-FIN-090` — Piutang tidak boleh tergabung dua batch aktif sekaligus**
> **Contoh:** piutang `AR-2026-09-00871` sudah masuk Batch `BATCH-2026-09-001` yang masih
> `DRAFT`. Petugas mencoba memasukkannya ke batch lain → ditolak. Setelah batch pertama
> dibatalkan, piutang itu bebas digabung batch baru.

> **`FR-FIN-091` — Dokumen batch merujuk dokumen per-invoice yang sudah ada, bukan menyalinnya**
> **Contoh:** Batch berisi tiga `FinReceivable`. Dokumen gabungannya menampilkan tiga rincian
> baris, masing-masing diambil langsung dari layanan dokumen per-invoice Billing yang sudah
> berjalan (`BillingCompanyGuarantorInvoiceDocumentService`) — Finance tidak menyalin ulang
> nilainya ke tabel sendiri.

> **`FR-FIN-092` — Status batch mengikuti pelunasan anggotanya, bukan sumber kebenaran baru**
> **Contoh:** dari tiga piutang anggota batch, dua sudah lunas lewat alokasi penerimaan biasa.
> Status batch otomatis menjadi "sebagian lunas" — tanpa satu pun endpoint batch yang secara
> langsung mengubah nilai piutang.

### EPIC FIN-17 — Potongan sisi penerimaan piutang *(baru, AMENDMENT REVISI 4)*

**Tujuan.** Menutup rumpun `FIN-SC-010`: PPh 23 dan biaya admin bank yang mengurangi piutang
sebagai pembayaran non-tunai, arah berlawanan dari potongan sisi pembayaran (`EPIC FIN-09`).

**Disposisi backend:** `MISSING / NEW`. Tidak bergantung modul lain.

> **`FR-FIN-093` — Potongan mengurangi sisa piutang, kebalikan potongan pembayaran**
> **Contoh:** piutang penjamin Rp 10.000.000 diselesaikan dengan penerimaan tunai Rp 9.770.000
> ditambah potongan PPh 23 Rp 230.000. Sisa piutang menjadi **nol** — bukan Rp 230.000. Ini
> kebalikan tepat `FR-FIN-051`: di sana potongan tidak mengurangi utang yang dibayar; di sini
> potongan justru melunasi piutang.

> **`FR-FIN-094` — Pos lain-lain wajib menyebut alasan**
> **Contoh:** petugas mencatat potongan jenis "lain-lain" tanpa mengisi alasannya → ditolak.

> **`FR-FIN-095` — Potongan dicatat bersama alokasinya, dan dibetulkan lewat pembalikan alokasi**
> *(diperbarui revisi 5, `FIN-DES-048`, `049`, `FIN-DEC-062`)*
> **Contoh:** transfer penjamin Rp 9.745.000 untuk piutang Rp 10.000.000. Dalam **satu** langkah
> alokasi petugas mencatat uang Rp 9.745.000, PPh 23 Rp 230.000, dan biaya bank Rp 25.000 —
> piutang lunas. Tidak ada tombol "tambah potongan" sesudahnya. Bila PPh 23 ternyata keliru,
> alokasinya dibalik (potongannya ikut terbalik, piutang terbuka kembali) lalu dicatat ulang.
> Bila tender sumbernya dibatalkan Billing, pembalikan yang sama terjadi **otomatis**.

> **`FR-FIN-099` — Potongan tidak pernah tercatat sebagai uang masuk**
> *(baru, revisi 5, `FIN-DEC-058`, `062`)*
> **Contoh:** dari contoh di atas, kotak keluar memuat kejadian penerimaan Rp 9.745.000 dan dua
> kejadian `POTONGAN-PIUTANG-NON-TUNAI` (Rp 230.000 dan Rp 25.000) — **bukan** kejadian penerimaan
> Rp 10.000.000. Buku besar tidak mencatat kas Rp 255.000 yang tidak pernah diterima.

## 11. Model status yang diusulkan

Daftar lengkap beserta transisi sahnya ada di `contracts/state-transition-matrix.md`. Ringkasan
untuk entity MVP:

| Domain | Status | Invariant utama |
|---|---|---|
| Fakta masuk Billing | `NEW` → `CONSUMED` → `ACKNOWLEDGED`, atau `ERROR` | Satu kunci sumber hanya diolah satu kali |
| Piutang | `OUTSTANDING` → `PARTIAL` → `SETTLED`, atau `WRITTEN_OFF`, `CANCELLED` | Nilai asli selalu sama dengan sisa + terbayar + koreksi + dihapusbukukan; sisa tidak pernah negatif |
| Koreksi dan penghapusan | `REQUESTED` → `APPROVED` atau `REJECTED` | Penyetuju selalu berbeda dari pengaju; nilai berubah hanya saat disetujui |
| Penerimaan | `RECEIVED` → `ALLOCATED` → `RECONCILED`, atau `REVERSED` | Satu tender berhasil = satu penerimaan; nominal = teralokasi + belum teralokasi |
| Setoran bank | `DRAFT` → `POSTED` → `VERIFIED`, atau `CANCELLED` | Nominal tidak pernah melebihi kas tersedia saat posting |
| Kas harian | `OPEN` → `CLOSED` | Satu baris per tanggal; angka yang tertutup dibekukan |
| Kejadian Accounting | `PENDING` atau `HELD_FOR_FINALIZATION` → `SENT` → `ACKNOWLEDGED`, atau `HELD`, `FAILED` | Dua lapis kunci mencegah jurnal ganda |

## 12. Sasaran arsitektur

| Yang dipakai ulang | Yang diperluas | Yang baru |
|---|---|---|
| `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment` sebagai sumber fakta | `BilArHandoff` bertambah dua kolom nullable untuk manfaat karyawan — **milik Billing**, `OPEN DECISION` | 24 tabel Finance baru, seluruhnya di `Areas/Corporate/FinanceManagement/` |
| `BilTender`, `BilSettlement`, `BilPaymentAllocation`, `BilCashierShift` sebagai rujukan baca | `BilCollectionHandoff` sebagai tabel baru **milik Billing** | Enam submodul baru, wajib didaftarkan registry lebih dulu |
| `MstSupplier` milik Administrator | — | Empat master Finance: bank, rekening, mata uang, kurs |
| Pola Petty Cash: `Guid RowVersion`, `Idempotency-Key`, transaksi `Serializable`, partial unique index | — | Kotak keluar kejadian dengan dua lapis kunci anti-dobel |
| Modul Accounting sebagai rujukan kode | — | — |

Rinciannya ada di `02-backend-architecture.md`, termasuk tabel kepemilikan data yang menjadi
dasar kolom "Yang baru".

## 13. Sasaran kemampuan API

Seluruh endpoint di bawah adalah bagian dari `contracts/api-contract.md` dan tidak melebihinya.

### Corporate / Finance Management / Receivable

Base URL: `api/v1/corporate/finance-management/receivables`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar piutang | `FinanceReceivable : Read` | `ReceivableQuery` | `ApiResponse<PagedResult<ReceivableResponse>>` | `EPIC FIN-03` | **Rencana (belum tersedia)** |
| `GET` | `/aging` | Umur piutang empat kelompok | `FinanceReceivable : Read` | `ReceivableAgingQuery` | `ApiResponse<ReceivableAgingResponse>` | `EPIC FIN-03` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/adjustments` | Ajukan koreksi | `FinanceReceivable : RequestAdjustment` | `CreateReceivableAdjustmentRequest` | `ApiResponse<ReceivableAdjustmentResponse>` | `EPIC FIN-06` | **Rencana (belum tersedia)** |
| `POST` | `/adjustments/{id:guid}/approve` | Setujui koreksi | `FinanceReceivable : ApproveAdjustment` | `ApproveRequest` | `ApiResponse<ReceivableAdjustmentResponse>` | `EPIC FIN-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/write-offs` | Ajukan penghapusan | `FinanceReceivable : RequestWriteOff` | `CreateWriteOffRequest` | `ApiResponse<ReceivableWriteOffResponse>` | `EPIC FIN-06` | **Rencana (belum tersedia)** |

### Corporate / Finance Management / Receipt

Base URL: `api/v1/corporate/finance-management/receipts`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/register` | Buku penerimaan kasir | `FinanceReceipt : Read` | `ReceiptRegisterQuery` | `ApiResponse<PagedResult<ReceiptRegisterResponse>>` | `EPIC FIN-05` | **Rencana (belum tersedia)** |
| `GET` | `/shift-reconciliation` | Rekonsiliasi shift kasir | `FinanceReceipt : Read` | `ShiftReconciliationQuery` | `ApiResponse<ShiftReconciliationResponse>` | `EPIC FIN-05` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/allocations` | Alokasi ke piutang | `FinanceReceipt : Allocate` | `AllocateReceiptRequest` | `ApiResponse<ReceiptDetailResponse>` | `EPIC FIN-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/reverse` | Balik penerimaan | `FinanceReceipt : Reverse` | `ReverseReceiptRequest` | `ApiResponse<ReceiptResponse>` | `EPIC FIN-06` | **Rencana (belum tersedia)** |

### Corporate / Finance Management / Bank Deposit

Base URL: `api/v1/corporate/finance-management/bank-deposits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/available-balance` | Saldo yang boleh disetor | `FinanceBankDeposit : Read` | `AvailableBalanceQuery` | `ApiResponse<AvailableCashBalanceResponse>` | `EPIC FIN-10` | **Rencana (belum tersedia)** |
| `POST` | `/{id:guid}/post` | Posting setoran | `FinanceBankDeposit : Post` | `PostBankDepositRequest` | `ApiResponse<BankDepositResponse>` | `EPIC FIN-10` | **Rencana (belum tersedia)** |

### Corporate / Finance Management / Accounting Event Monitor

Base URL: `api/v1/corporate/finance-management/accounting-events`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kejadian beserta statusnya | `FinanceAccountingEvent : Read` | `AccountingEventQuery` | `ApiResponse<PagedResult<AccountingEventResponse>>` | `EPIC FIN-11` | **Rencana (belum tersedia)** |

Daftar lengkap seluruh grup ada di `contracts/api-contract.md`.

## 14. Matriks kewenangan

String permission ditulis persis seperti pada `contracts/permission-audit-matrix.md`.

| Tindakan | String yang dipakai | Peran yang diusulkan |
|---|---|---|
| Melihat piutang dan penerimaan | `[AccessPermission("FinanceReceivable", "Read")]`, `[AccessPermission("FinanceReceipt", "Read")]` | AR Staff, Supervisor, Treasury, Auditor |
| Mengalokasikan penerimaan | `[AccessPermission("FinanceReceipt", "Allocate")]` | AR Staff |
| Mengajukan koreksi | `[AccessPermission("FinanceReceivable", "RequestAdjustment")]` | AR Staff |
| Menyetujui koreksi | `[AccessPermission("FinanceReceivable", "ApproveAdjustment")]` | Supervisor |
| Mengajukan penghapusan | `[AccessPermission("FinanceReceivable", "RequestWriteOff")]` | AR Staff |
| Menyetujui penghapusan | `[AccessPermission("FinanceReceivable", "ApproveWriteOff")]` | Supervisor |
| Memposting setoran | `[AccessPermission("FinanceBankDeposit", "Post")]` | Treasury |
| Menutup kas harian | `[AccessPermission("FinanceDailyCash", "Close")]` | Treasury |
| Memantau kejadian Accounting | `[AccessPermission("FinanceAccountingEvent", "Read")]` | Supervisor, Accounting Staff, Auditor |

**Aturan penyusunan peran:** satu pengguna **MUST NOT** memegang `RequestAdjustment` dan
`ApproveAdjustment` sekaligus untuk Resource yang sama.

## 15. Batas integrasi dan billing

Yang modul ini **MUST NOT** buat sendiri:

| Yang tidak dibuat | Pemiliknya | Cara memakainya |
|---|---|---|
| Tagihan, perhitungan, tender, shift kasir | Billing | Dibaca lewat fakta handoff; Finance tidak pernah menulis |
| COA, jurnal, periode akuntansi, aturan posting | Accounting | Finance hanya mengirim kejadian; tidak pernah menyimpan nomor akun |
| Perhitungan dan persetujuan fee dokter | Medical Fee | Finance menerima hasil yang sudah disetujui |
| Master supplier | Administrator | Dirujuk lewat `SupplierId` |
| Identitas pasien, dokter, pegawai | Modul masing-masing | Dirujuk lewat Id |
| Aturan operasional kas kecil | Blueprint `billing-kasir` | `FIN-DEC-009` |

**Dampak pada tagihan pasien: nol.** Modul ini tidak pernah mengubah nilai tagihan, status
tagihan, maupun saldo shift kasir.

## 16. Guardrail regulasi

| Kewajiban | Wujudnya di modul ini |
|---|---|
| Kerahasiaan data pasien | `PatientId`, `EncounterId`, `BenefitOwnerId`, `DoctorId`, dan nomor rekening ditandai **Sensitif** pada kamus data; seluruhnya dilarang masuk log dan dilarang menyeberang ke Accounting |
| Jejak audit keuangan | Setiap perubahan nilai uang berupa baris baru, bukan pembaruan. Penghapusan bersifat penandaan; baris tidak pernah hilang |
| Pemisahan tugas | Pengaju dan penyetuju wajib berbeda orang, ditegakkan tiga lapis: hak akses terpisah, pemeriksaan service, dan check constraint database |
| Ketertelusuran | Setiap piutang dan penerimaan dapat ditelusuri ke tagihan Billing lewat pengenal rantai |
| Mata uang pelaporan | Hanya rupiah yang dikirim ke Accounting |

Modul ini **tidak** menyimpan rekam medis dan tidak mengambil keputusan klinis apa pun.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Wujudnya |
|---|---|---|
| `NFR-001` | Satu fakta bisnis tersimpan utuh atau tidak sama sekali | Transaksi mencakup fakta, saldo induknya, dan baris kejadiannya sekaligus |
| `NFR-002` | Dua petugas yang bekerja bersamaan tidak saling menimpa | `Guid RowVersion` beserta `ExpectedRowVersion`; balasan `409` bila basi |
| `NFR-003` | Perebutan saldo tidak menghasilkan angka negatif | Transaksi `Serializable` pada seluruh perintah yang menggerakkan uang |
| `NFR-004` | Menekan Simpan dua kali tidak membuat catatan ganda | `Idempotency-Key` wajib pada perintah uang |
| `NFR-005` | Koreksi tidak pernah menghapus riwayat | Pembalikan selalu berupa baris baru |
| `NFR-006` | Angka uang tidak kehilangan ketelitian | Seluruh kolom uang `decimal(18,2)`; tidak ada bilangan pecahan mengambang |
| `NFR-007` | Waktu tercatat beserta zonanya | Seluruh kolom waktu `timestamp with time zone` |
| `NFR-008` | Daftar dapat disaring dan dihalamankan | Seluruh endpoint daftar memakai `PagedResult<T>`, batas halaman 1–100 |
| `NFR-009` | Kegagalan integrasi terlihat, bukan hilang | Fakta gagal berstatus `ERROR` dengan pesannya; kejadian gagal punya riwayat percobaan |
| `NFR-010` | Data sensitif tidak bocor lewat log | `GET` tidak dicatat; payload log hanya pengenal, controller, action, dan status |

## 18. Skenario UAT

Setiap epic `MUST HAVE` punya sekurang-kurangnya satu skenario berhasil dan satu skenario gagal.

> **`UAT-01` — Data induk siap dipakai** (`EPIC FIN-01`, berhasil)
> **Kondisi awal:** modul baru dipasang, data induk kosong.
> **Langkah:** Admin menambahkan mata uang `IDR` sebagai dasar, satu bank, satu rekening
> `COLLECTION`, dan satu rekening `PAYMENT`.
> **Hasil:** keempatnya tersimpan; rekening `COLLECTION` muncul sebagai pilihan saat Treasury
> membuat setoran.

> **`UAT-02` — Nomor rekening ganda ditolak** (`EPIC FIN-01`, gagal)
> **Kondisi awal:** rekening `1234567890` di Bank Contoh sudah ada.
> **Langkah:** Admin menambahkan nomor yang sama di bank yang sama.
> **Hasil:** ditolak dengan pesan terbaca; jumlah rekening tetap satu.

> **`UAT-03` — Fakta piutang menjadi piutang** (`EPIC FIN-02`, `FIN-03`, berhasil)
> **Kondisi awal:** Billing memfinalisasi tagihan Rp 5.000.000; pasien sudah membayar
> Rp 1.500.000; tanggungan penjamin Rp 3.500.000.
> **Langkah:** petugas membuka layar fakta masuk dan menjalankan pengolahan.
> **Hasil:** terbentuk satu piutang Rp 3.500.000 berstatus belum lunas, dengan rincian pasien
> dan nomor tagihannya. Fakta berstatus sudah diolah.

> **`UAT-04` — Fakta yang sama dikirim dua kali** (`EPIC FIN-02`, gagal)
> **Kondisi awal:** fakta pada `UAT-03` sudah diolah.
> **Langkah:** fakta dengan kunci sumber yang sama dikirim lagi.
> **Hasil:** ditolak; jumlah piutang tetap satu. Tidak ada piutang Rp 7.000.000.

> **`UAT-05` — Pasien bayar lunas tidak melahirkan piutang** (`EPIC FIN-05`, berhasil)
> **Kondisi awal:** tagihan Rp 500.000, dibayar lunas tunai di kasir.
> **Langkah:** fakta penerimaannya diolah Finance.
> **Hasil:** satu penerimaan Rp 500.000 tercatat lengkap dengan nomor kwitansi dan shift.
> Jumlah piutang yang terbentuk: **nol**.

> **`UAT-06` — Tender yang sama dikirim dua kali** (`EPIC FIN-05`, gagal)
> **Kondisi awal:** penerimaan pada `UAT-05` sudah tercatat.
> **Langkah:** fakta dengan nomor tender yang sama dikirim lagi.
> **Hasil:** ditolak; tetap satu penerimaan Rp 500.000.

> **`UAT-07` — Bayar sebelum tagihan final** (`EPIC FIN-05`, `FIN-11`, berhasil)
> **Kondisi awal:** tagihan masih terbuka; pasien membayar Rp 1.000.000.
> **Langkah:** fakta penerimaannya diolah; kemudian Billing memfinalisasi tagihan.
> **Hasil:** penerimaan langsung terlihat sejak awal. Kejadiannya berstatus tertahan, lalu
> berubah menjadi siap kirim setelah tagihan final.

> **`UAT-08` — Penjamin melunasi piutang** (`EPIC FIN-06`, berhasil)
> **Kondisi awal:** piutang Rp 3.500.000 dari `UAT-03`.
> **Langkah:** petugas mencatat penerimaan dari penjamin Rp 3.500.000 dan mengalokasikannya ke
> piutang itu.
> **Hasil:** sisa piutang nol, status lunas. Penerimaan teralokasi penuh.

> **`UAT-09` — Alokasi melebihi sisa piutang** (`EPIC FIN-06`, gagal)
> **Kondisi awal:** sisa piutang Rp 2.000.000.
> **Langkah:** petugas mengalokasikan Rp 3.000.000.
> **Hasil:** ditolak dengan pesan menyebut sisa sebenarnya. Sisa piutang tidak berubah.

> **`UAT-10` — Pengaju menyetujui permohonannya sendiri** (`EPIC FIN-06`, gagal)
> **Kondisi awal:** Petugas Rina mengajukan koreksi turun Rp 500.000.
> **Langkah:** Rina menekan Setujui.
> **Hasil:** ditolak dengan pesan "Pengaju tidak boleh menyetujui permohonannya sendiri." Nilai
> piutang tidak berubah.

> **`UAT-11` — Orang kedua menyetujui koreksi** (`EPIC FIN-06`, berhasil)
> **Kondisi awal:** koreksi pada `UAT-10` masih menunggu.
> **Langkah:** Petugas Dimas yang berwenang menekan Setujui.
> **Hasil:** koreksi disetujui; sisa piutang berkurang Rp 500.000 saat itu juga, bukan sebelumnya.

> **`UAT-12` — Tender dibatalkan setelah piutang lunas** (`EPIC FIN-06`, berhasil)
> **Kondisi awal:** piutang sudah lunas oleh penerimaan Rp 3.500.000.
> **Langkah:** Billing membatalkan tender penerimaan itu.
> **Hasil:** sistem membuat baris pembalik; piutang otomatis kembali belum lunas dengan sisa
> Rp 3.500.000. Baris alokasi lama tetap terbaca di riwayat.

> **`UAT-13` — Setoran melebihi kas tersedia** (`EPIC FIN-10`, gagal)
> **Kondisi awal:** kas tersedia Rp 4.800.000.
> **Langkah:** Treasury memposting setoran Rp 5.000.000.
> **Hasil:** ditolak dengan pesan menyebut saldo tersedia Rp 4.800.000. Tidak ada setoran
> tersimpan.

> **`UAT-14` — Dua petugas menyetor bersamaan** (`EPIC FIN-10`, gagal)
> **Kondisi awal:** kas tersedia Rp 4.800.000; dua petugas membuka layar setoran bersamaan dan
> keduanya melihat angka itu.
> **Langkah:** keduanya memposting Rp 4.800.000 pada waktu hampir bersamaan.
> **Hasil:** satu berhasil, satu ditolak. Saldo kas tidak pernah negatif.

> **`UAT-15` — Menutup kas harian** (`EPIC FIN-10`, berhasil)
> **Kondisi awal:** seluruh setoran hari itu sudah diposting.
> **Langkah:** Treasury menutup kas.
> **Hasil:** saldo akhir tersimpan dan dibekukan. Saldo awal hari berikutnya sama dengan saldo
> akhir hari ini.

> **`UAT-16` — Kas kecil tidak mengganggu kas kasir** (`EPIC FIN-10`, berhasil)
> **Kondisi awal:** ada pencairan kas kecil Rp 500.000 hari itu.
> **Langkah:** Treasury membuka posisi kas harian.
> **Hasil:** angka kas kasir tidak terpengaruh sama sekali oleh pencairan itu.

> **`UAT-17` — Setiap fakta menulis kejadian** (`EPIC FIN-11`, berhasil)
> **Kondisi awal:** piutang dari `UAT-03` baru terbentuk.
> **Langkah:** petugas membuka layar pemantauan kejadian.
> **Hasil:** ada tepat satu kejadian pengakuan piutang untuk transaksi itu, dengan nilai yang
> sama dengan piutangnya.

> **`UAT-18` — Kejadian kedua untuk fakta yang sama ditolak** (`EPIC FIN-11`, gagal)
> **Kondisi awal:** kejadian pada `UAT-17` sudah ada.
> **Langkah:** sistem mencoba menulis kejadian kedua untuk transaksi, jenis, dan versi yang sama.
> **Hasil:** ditolak. Ini yang mencegah satu piutang menjadi dua jurnal.

> **`UAT-19` — Data pasien tidak ikut ke Accounting** (`EPIC FIN-11`, berhasil)
> **Kondisi awal:** piutang memuat tiga pasien.
> **Langkah:** petugas membuka isi pesan kejadiannya.
> **Hasil:** tidak ada nama pasien, nomor rekam medis, maupun nomor kunjungan di dalamnya.

> **`UAT-20` — Membuktikan tidak ada dobel-hitung** (`EPIC FIN-05`, berhasil)
> **Kondisi awal:** tagihan Rp 5.000.000; dibayar Rp 1.500.000 di kasir; piutang penjamin
> Rp 3.500.000.
> **Langkah:** auditor membuka ringkasan tagihan itu di Finance.
> **Hasil:** terbaca penerimaan Rp 1.500.000 dan piutang Rp 3.500.000, berjumlah tepat
> Rp 5.000.000. Tidak ada piutang Rp 5.000.000 yang menghitung ulang uang yang sudah dibayar.

## 19. Definition of Done

Setiap butir dijawab "ya" atau "belum", beserta buktinya.

| Butir | Bukti |
|---|---|
| Satu tagihan berpenjamin dapat berjalan dari pasien membayar sampai piutang lunas | `UAT-03`, `UAT-08` |
| Jumlah yang sudah dibayar tidak pernah menjadi piutang | `UAT-05`, `UAT-20` |
| Fakta dari Billing yang sama tidak melahirkan catatan ganda | `UAT-04`, `UAT-06` |
| Kegagalan pengolahan fakta terlihat dan dapat diulang | `FR-FIN-011`, `FR-FIN-012` |
| Koreksi dan penghapusan tidak dapat disetujui pengajunya sendiri | `UAT-10`, `UAT-11` |
| Nilai piutang tidak berubah selama permohonan belum diputus | `FR-FIN-044` |
| Pembalikan tidak menghapus riwayat, dan piutang terbuka kembali otomatis | `UAT-12` |
| Setoran bank tidak mungkin melebihi kas yang benar-benar ada | `UAT-13`, `UAT-14` |
| Kas kecil tidak memengaruhi posisi kas kasir | `UAT-16` |
| Angka kas yang sudah ditutup tidak berubah | `FR-FIN-065` |
| Setiap fakta keuangan menulis tepat satu kejadian di kotak keluar | `UAT-17`, `UAT-18` |
| Data pasien tidak pernah masuk pesan ke Accounting | `UAT-19` |
| Jasa medis tidak terbukukan dua kali | `FR-FIN-075` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 8 |
| Tidak ada layar Finance yang memiliki COA, jurnal, atau periode akuntansi | Tabel kepemilikan data pada `02-backend-architecture.md` bagian 1.2 |
| Enam submodul baru sudah terdaftar di registry kepemilikan modul | `FIN-DES-002` |

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang pengiriman

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | `EPIC FIN-01` — data induk, enam submodul terdaftar, migration master | Blueprint disetujui dan registry diperbarui |
| `MVP-1` | `EPIC FIN-02` dan `EPIC FIN-03` — pintu masuk fakta Billing dan buku piutang | `MVP-0` selesai |
| `MVP-2` | `EPIC FIN-05` — penerimaan dan pembagian bayar-vs-piutang | `MVP-1` selesai **dan** `BilCollectionHandoff` tersedia dari tim Billing |
| `MVP-3` | `EPIC FIN-06` — alokasi, koreksi, penghapusan | `MVP-2` selesai |
| `MVP-4` | `EPIC FIN-10` — setoran bank dan kas harian | `MVP-2` selesai |
| `MVP-5` | `EPIC FIN-11` — kotak keluar kejadian | Dapat berjalan paralel sejak `MVP-1`, tetapi **selesai** setelah `MVP-4` agar seluruh jenis kejadian tercakup |
| `POST-MVP` | `EPIC FIN-15` (menggantikan `FIN-07`), `FIN-09` — Purchasing/AP dan pembayaran | Setelah MVP; **tidak lagi** menunggu ratifikasi `PPN-MASUKAN-PEMBELIAN` (`FIN-DEC-056` mempersempit `FIN-OQ-020`, 25 September 2026) — hanya aktivasi worker pengirimannya yang tertahan, dapat direncanakan terpisah setelah epic ini berjalan |
| `POST-MVP` | `EPIC FIN-08` — utang dokter | **Menunggu modul Medical Fee dibangun** (`FIN-CAP-021`) |
| `POST-MVP` | `EPIC FIN-13` — penyelarasan frontend kas kecil | Kapan saja; tidak mengunci apa pun |
| `POST-MVP` | `EPIC FIN-16` — AR Invoice Agregat *(baru)* | Setelah MVP; tidak menunggu pihak luar — dapat dipercepat bila owner memintanya lebih awal karena datanya sepenuhnya internal Finance |
| `POST-MVP` | `EPIC FIN-17` — Potongan sisi penerimaan AR *(baru)* | Setelah MVP; tidak menunggu pihak luar |

**Tidak masuk gelombang mana pun:** `EPIC FIN-04` (piutang manfaat karyawan),
`EPIC FIN-12` (pengiriman ke Accounting), dan `EPIC FIN-14` (uang muka, deposit, kelebihan
bayar, selisih kas). Keempatnya berstatus `OPEN DECISION`.

**`EPIC FIN-15`, `FIN-16`, `FIN-17` ditambahkan AMENDMENT REVISI 4 (25 September 2026)** sebagai
`POST-MVP`, bukan `OPEN DECISION` — keputusan bisnisnya sudah lengkap dan `approved`
(`FIN-DEC-045`..`055`). **Tidak satu pun dari ketiganya punya gerbang yang menahan masuk
`/plan-module-delivery`** — gerbang `FIN-OQ-020` yang semula menahan `EPIC FIN-15` sudah
dipersempit `FIN-DEC-056` (25 September 2026) menjadi hanya menahan aktivasi worker pengiriman
`PPN-MASUKAN-PEMBELIAN`, bukan epic-nya.

~~`MVP-2` adalah satu-satunya gelombang yang bergantung pada tim lain.~~ **Diperbarui
25 September 2026:** `BilCollectionHandoff` **sudah tersedia** dan sudah dikonsumsi Finance
(`FIN-CAP-007`, `FIN-CAP-025`), sehingga syarat luar `MVP-2` sudah terpenuhi. Yang tersisa
hanya verifikasi bahwa bentuk kolomnya cocok dengan yang diminta bagian 2.1
`contracts/integration-contract.md` — bukan lagi menunggu tim lain membangunnya.

### 20.2 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| ~~Apakah owner menyetujui `FIN-DES-001`..`024`?~~ | Yasmin | — | **TERJAWAB 20 September 2026** — disetujui seluruhnya |
| ~~Apakah cakupan MVP pada bagian 7 dan 8 disetujui owner?~~ | Yasmin | — | **TERJAWAB 20 September 2026** — cakupan dan urutan gelombang dikunci apa adanya |
| ~~Apakah owner Billing menyetujui bentuk `BilCollectionHandoff` sesuai `FIN-DEC-005`?~~ | Billing Owner | — | **TERJAWAB 22 September 2026** — tabelnya dibangun (`BKC-DES-037`) dan sudah dikonsumsi `FinanceBillingIntakeService`; diverifikasi impact scan 25 September 2026 (`FIN-CAP-007`) |
| Apakah owner Billing dan HR menyetujui perluasan `BilArHandoff` untuk manfaat karyawan? | Billing Owner + HR Owner | `EPIC FIN-04` tetap `OPEN DECISION` | Tidak — epic-nya sudah dikeluarkan dari seluruh gelombang |
| ~~Berapa ambang nominal jenjang persetujuan pembayaran AP?~~ (`FIN-OQ-010`) | Yasmin | — | **TERJAWAB 25 September 2026** — Rp 50.000.000, dipakai seragam untuk approval pembayaran (`EPIC FIN-09`) **dan** approval PO/Purchasing Invoice (`FIN-DEC-052`) |
| ~~Apakah Accounting menyetujui nama field saldo subledger? (`FIN-OQ-011`)~~ | Rizki (Accounting) | — | **TERJAWAB 24-25 September 2026** — Accounting menetapkan bentuknya, Finance menerima (`FIN-DEC-035`) |
| ~~Apakah Accounting meratifikasi katalog 17 jenis kejadian?~~ | Rizki (Accounting) | — | **TERJAWAB 24 September 2026** — ketujuh belas kode diratifikasi apa adanya (`ACC-DEC-083`, `FIN-DEC-039`) |
| **Apakah Accounting meratifikasi TUJUH kode kejadian baru?** (`FIN-OQ-017`) | Rizki (Accounting) | `EPIC FIN-14` tetap `OPEN DECISION`; gerbang cutover `G6` tidak tuntas; perubahan `FIN-DEC-030` tidak dapat dieksekusi di source | **Ya, untuk `EPIC FIN-14` dan implementasi `FIN-DEC-030`** — epic-nya sudah dikeluarkan dari seluruh gelombang, jadi tidak memblokir `MVP-0`..`MVP-5` |
| Apakah pengesahan selisih kas shift punya batas waktu sebelum tutup buku? | Rizki (Accounting) + owner Billing | Selisih yang disahkan setelah periodenya ditutup tidak dapat dijurnalkan ke periode yang benar | Tidak — perlu kesepakatan operasional, bukan keputusan desain |
| Bagaimana lawan jurnal refund `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN`? (`FIN-OQ-018`) | Yasmin (Finance) | Refund kategori itu tidak diterbitkan sebagai kejadian apa pun | Tidak — sengaja di luar cakupan `EPIC FIN-14` |
| Kapan endpoint penerima Accounting dibangun? | Rizki (Accounting) | `EPIC FIN-12` tetap `OPEN DECISION` | Tidak — sudah dikeluarkan dari gelombang |
| Kapan modul Medical Fee dibangun? (`FIN-CAP-021`) | Owner Medical Fee | `EPIC FIN-08` tidak dapat dimulai | Tidak — sudah `POST-MVP` |
| Apakah Accounting meratifikasi kode `PPN-MASUKAN-PEMBELIAN`? (`FIN-OQ-020`) | Rizki (Accounting) | Worker pengiriman kode ini tetap dimatikan sampai dijawab | **Tidak** — `FIN-DEC-056` (25 September 2026) mempersempit dampaknya murni ke aktivasi worker. `EPIC FIN-15` sendiri sudah boleh diteruskan `/plan-module-delivery` tanpa menunggu jawaban ini |
| Apakah Accounting meratifikasi kode ke-26 s.d. 29 — potongan AR, pembaliknya, retur, pemakaian deposit? (`FIN-OQ-026`) | Rizki (Accounting) | Worker pengiriman keempat kode itu tetap dimatikan. **Surat belum dikirim** | **Tidak** — pola `FIN-DEC-056`; `EPIC FIN-15`/`FIN-17` tetap boleh dibangun |

**Keadaan pada 20 September 2026 sesudah approval owner:** kedua blocker tingkat dokumen sudah
tercabut. Yang tersisa hanya satu, dan ia memblokir **satu gelombang saja**:

- `MVP-0` dan `MVP-1` — **siap diteruskan ke `/plan-module-delivery`.** Tidak ada blocker.
- `MVP-2` — menunggu konfirmasi owner Billing atas `BilCollectionHandoff`.
- `MVP-3` — mengikuti `MVP-2`.
- `MVP-4` — bergantung pada `MVP-2` hanya untuk angka penerimaan tunai; dapat direncanakan
  bersamaan dengan `MVP-1`.
- `MVP-5` — dapat berjalan paralel sejak `MVP-1`.

Paket permintaan ke owner Billing sudah dikirim lewat
`evidence/02-permintaan-kontrak-untuk-owner-billing.md`.

**Keadaan diperbarui 25 September 2026 (AMENDMENT REVISI 3):**

- `MVP-2` dan `MVP-3` **tidak lagi tertahan tim lain** — `BilCollectionHandoff` sudah tersedia
  dan sudah dikonsumsi (`FIN-CAP-007`, `FIN-CAP-025`). Keduanya kini setara `MVP-0`/`MVP-1`
  dari sisi ketergantungan luar.
- `EPIC FIN-14` **baru** ditambahkan dan berstatus `OPEN DECISION` — di luar seluruh gelombang,
  menunggu ratifikasi tujuh kode kejadian oleh Accounting (`FIN-OQ-017`).
- `FR-FIN-034` diperbarui dan `FR-FIN-074` dicabut mengikuti `FIN-DEC-030`. Implementasi
  perubahan itu **menyentuh source yang sudah berjalan**, dan tertahan `FIN-OQ-017` yang sama.

**Status penerusan:** dokumen ini **boleh** diteruskan ke `/plan-module-delivery` untuk
`MVP-0` sampai `MVP-5`. `EPIC FIN-04`, `EPIC FIN-12`, dan `EPIC FIN-14` **MUST NOT** masuk
perencanaan pengiriman sampai keputusan yang menahannya turun.

**Ditambahkan AMENDMENT REVISI 4 (25 September 2026):**

- `EPIC FIN-15`, `FIN-16`, `FIN-17` (Purchasing/AP, AR Invoice Agregat, Potongan AR) ditambahkan
  berstatus `POST-MVP`, **bukan** `OPEN DECISION` — keputusan bisnisnya sudah `approved` penuh
  (`FIN-DEC-045`..`055`) dan rancangan teknisnya sudah lengkap
  (`02-backend-architecture.md` AMENDMENT REVISI 4).
- ~~`EPIC FIN-15` MUST NOT diteruskan `/plan-module-delivery` sampai `FIN-OQ-020` turun.~~
  **Dipersempit `FIN-DEC-056`:** `EPIC FIN-15` **boleh** diteruskan `/plan-module-delivery`; yang
  tetap menunggu `FIN-OQ-020` hanya aktivasi worker pengiriman kode `PPN-MASUKAN-PEMBELIAN`.
- `EPIC FIN-16` dan `EPIC FIN-17` **boleh** diteruskan `/plan-module-delivery` tanpa gerbang
  tambahan — keduanya murni internal Finance.
- Bagian ini (epic, FR, dan perubahan bagian 8 dan 20) **disetujui dan dikunci Yasmin
  25 September 2026** sebagai `FIN-MVP-1.3`, bersama `FIN-DES-037`..`044`.

---

**Status dokumen:** `MVP-0`..`MVP-5` dan AMENDMENT REVISI 4 `approved` dan `locked` oleh Yasmin.
Approval dicatat apa adanya dari pernyataan owner; skill tidak menetapkannya sendiri.

---

# AMENDMENT REVISI 6 — `FIN-MVP-1.5` (`draft`, 28 September 2026)

| Field | Nilai |
|---|---|
| Contract version | `FIN-MVP-1.5` — **`draft`, belum dikunci owner** |
| `last_changed_in` | `FIN-MVP-1.5` — 28 September 2026: `EPIC FIN-14` dan `EPIC FIN-12` diperbarui; `FR-FIN-100`..`107` baru; bagian 20.2 diperbarui |
| Diturunkan dari | `FIN-DEC-063`..`071`; `02-backend-architecture.md` AMENDMENT REVISI 6 (`FIN-DES-051`..`058`); `contracts/integration-contract.md` bagian 5.10 |
| Entity/status/permission/endpoint baru | **Nol** yang lahir di sini. Satu kolom (`FinSupplierReturn.PPNAmount`) sudah tercatat `02-backend-architecture.md` `E.9` dan `erd/data-dictionary.md` C.8 lebih dulu |

## 21. Apa yang berubah pada rilis karena ratifikasi Accounting

Ratifikasi owner Accounting (`evidence/14`) **tidak menambah kemampuan baru** bagi pengguna. Ia
mengubah dua hal yang menentukan kapan sebuah kemampuan boleh dinyatakan selesai: nama kode
kejadian, dan daftar keadaan yang sengaja tidak diterbitkan sebagai kejadian.

| Epic | Perubahan status | Alasan |
|---|---|---|
| `EPIC FIN-14` (uang muka, deposit, kelebihan bayar, selisih kas) | **Tetap `OPEN DECISION`, tetapi sebabnya berganti** — bukan lagi `FIN-OQ-017` (sudah tertutup), melainkan `FIN-OQ-027`, `030`, `031`, `032`, `034` | Ketujuh kode aslinya sudah diratifikasi; yang terbuka sekarang kode-kode **baru** yang lahir dari pemecahan dan dari dua temuan source (penanda shift, gap pembalikan tender) |
| `EPIC FIN-12` (pengiriman ke Accounting) | **Satu penghalang tertutup** — endpoint penerima Accounting **sudah ada** pada `cba60cb0` (`FIN-CAP-018` `stale`). Tetap `OPEN DECISION` karena `FIN-OQ-016` (kredensial akun layanan) belum turun | Gerbang `G1` sisi keberadaan endpoint tidak lagi menahan; UAT-nya milik Accounting |
| `EPIC FIN-15` (Purchasing/AP) | Tetap seperti REVISI 4-5. **Satu kolom baru** dan **satu kejadian baru** masuk cakupannya (`FR-FIN-102`, `103`) | Porsi PPN retur (`FIN-DEC-068`) |
| `EPIC FIN-17` (Potongan AR) | Tetap seperti REVISI 5, dengan **cakupan menyempit**: jenis potongan "lain-lain" dikeluarkan | `FIN-VAL-137`; tidak ada akun debit yang sah untuk jenis itu |

`EPIC FIN-14` dan `EPIC FIN-12` **MUST NOT** masuk gelombang pengiriman mana pun selama masih
`OPEN DECISION`. Gelombang `MVP-0`..`MVP-5` tidak berubah oleh amendment ini.

## 22. Functional requirement baru

Seluruhnya dapat diuji, dan seluruhnya menurunkan dari arsitektur/kontrak yang sudah berdiri.

| ID | Functional requirement | Disposisi | Bukti uji |
|---|---|---|---|
| `FR-FIN-100` | Setiap kejadian yang ditulis Finance memakai nama dari katalog resmi; kelima alias `AR_*`/`AP_*` tidak dapat lagi ditulis karena konstantanya dihapus | `EXTEND` — lima titik tulis pada kode yang sudah berjalan | `acceptance-test-matrix.md` D.1 |
| `FR-FIN-101` | Pesan kejadian yang tidak punya rincian komponen tidak memuat properti komponen sama sekali | `EXTEND` — satu method pada kode yang sudah berjalan | D.2 |
| `FR-FIN-102` | Retur pembelian dapat mencatat porsi PPN terpisah dari nilai pokoknya | `MISSING / NEW` — satu kolom, satu migration | D.5 |
| `FR-FIN-103` | Kredit retur yang lahir dari retur bernilai pokok + PPN, dan porsi PPN-nya terbit sebagai kejadian tersendiri | `EXTEND` — satu service yang sudah berjalan | D.5 |
| `FR-FIN-104` | Potongan piutang dicatat sebagai PPh 23 **atau** biaya administrasi bank, masing-masing dengan kejadian sendiri; jenis lain-lain ditolak dengan pesan yang menyebut jalan keluarnya | `MISSING / NEW` — `BE-FIN-040` | D.4 |
| `FR-FIN-105` | Setiap shift kasir yang tertutup final menerbitkan tepat satu penanda tertutup, termasuk shift yang kasnya pas; shift yang dibuka kembali menerbitkan penanda pembalik | `MISSING / NEW` — bergantung `FIN-OQ-030`/`032` | D.3 |
| `FR-FIN-106` | Selisih kas yang disahkan menerbitkan tepat satu kejadian per siklus tutup, walaupun pengesahannya melewati tahap tindak lanjut | `MISSING / NEW` — bergantung `FIN-OQ-030` | D.3 |
| `FR-FIN-107` | Fakta Billing yang tidak dapat diterbitkan sebagai kejadian — refund yang perlakuan akuntansinya belum ditetapkan, dan pembalikan tender top-up deposit tanpa mutasi pembalik — tercatat sebagai kegagalan yang terbaca petugas beserta sebabnya, bukan dilewati diam-diam | `MISSING / NEW` — bergantung `FIN-OQ-031`/`034` | D.6 |

## 23. Skenario UAT tambahan

Setiap butir memuat jalur berhasil **dan** jalur gagal, sesuai kontrak dokumen ini.

| # | Skenario | Jalur berhasil | Jalur gagal |
|---:|---|---|---|
| 1 | Kasir menutup shift yang kasnya **pas** | Shift tertutup; penanda tertutup terbit; Accounting dapat menutup bulan | Bila penanda tidak terbit, tutup bulan tertahan tanpa sebab yang terlihat — inilah skenario yang **MUST** diuji lebih dulu, karena inilah yang paling mudah terlewat |
| 2 | Kasir menutup shift dengan selisih Rp 30.000, supervisor menyatakan perlu tindak lanjut, dua hari kemudian diselesaikan | Tepat **satu** kejadian selisih terbit, bertanggal shift; penanda tertutup terbit sesudah selesai | Bila dua kejadian terbit, buku besar mencatat selisih dua kali lipat. Uji **MUST** membuktikan jumlahnya satu |
| 3 | Penjamin membayar piutang dipotong PPh 23 dan biaya bank | Piutang lunas penuh; tiga kejadian terbit dengan kode yang benar | Petugas memilih jenis potongan "lain-lain": ditolak dengan pesan yang menyebut jalan keluarnya, dan **seluruh** alokasi ditolak — tidak ada piutang yang berkurang tanpa kejadian |
| 4 | Petugas mencatat retur obat yang membawa PPN | Kredit retur bernilai pokok + PPN; dua kejadian terbit dengan nilai terpisah | PPN diisi negatif: ditolak sebelum retur tersimpan |
| 5 | Pasien dikembalikan uang dari kredit biaya administrasi rawat jalan | — | Tidak ada kejadian yang terbit; baris kegagalan muncul di layar pemantauan beserta sebabnya, dan tombol "ulangi" **tidak** ditawarkan karena mengulang tidak akan pernah berhasil |
| 6 | Kartu pasien ditarik kembali penerbit sesudah uang mukanya dipakai melunasi tagihan | — | Baris kegagalan muncul beserta sebabnya; Finance **tidak** menulis apa pun ke tabel Billing. Perbaikannya menunggu owner Billing |

## 24. Definition of Done tambahan

| # | Butir | Dijawab dengan |
|---:|---|---|
| 1 | Apakah seluruh kejadian yang ditulis memakai nama katalog, dan kelima alias sudah dihapus? | Pencarian kode: nol kemunculan konstanta alias; nol `EventTypeCode` di luar katalog |
| 2 | Apakah pesan tanpa komponen sudah tidak memuat properti komponen? | Satu uji unit atas `PayloadJson` |
| 3 | Apakah shift yang kasnya pas menerbitkan penanda tertutup? | Uji integrasi skenario UAT nomor 1 |
| 4 | Apakah satu shift tidak pernah menerbitkan dua kejadian selisih? | Uji integrasi skenario UAT nomor 2, termasuk jalur tindak lanjut |
| 5 | Apakah kedua keadaan yang sengaja tidak diterbitkan terlihat di layar pemantauan beserta sebabnya? | Uji integrasi skenario UAT nomor 5 dan 6 |
| 6 | Apakah kolom `PPNAmount` sudah ada dan kredit retur sudah mencakupnya? | Migration terpasang + uji integrasi skenario UAT nomor 4 |
| 7 | Apakah setiap kode pada katalog final sudah terdaftar sebagai jenis kejadian di sisi Accounting? | Konfirmasi tertulis dari owner Accounting — **di luar kendali Finance**, dan MUST dipastikan sebelum worker diaktifkan |

## 25. Pertanyaan terbuka sebelum development lock — pembaruan

Menggantikan baris yang bersesuaian pada bagian 20.2.

| Pertanyaan | Pemilik | Bila belum terjawab | Memblokir? |
|---|---|---|---|
| ~~Apakah Accounting meratifikasi tujuh kode kejadian baru? (`FIN-OQ-017`)~~ | Rizki | **TERTUTUP** 28 September 2026 | — |
| ~~Apakah Accounting meratifikasi `PPN-MASUKAN-PEMBELIAN`? (`FIN-OQ-020`)~~ | Rizki | **TERTUTUP** 28 September 2026 | — |
| ~~Kapan endpoint penerima Accounting dibangun?~~ | Rizki | **TERTUTUP** — sudah ada pada `cba60cb0` | — |
| Ratifikasi kode `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` (`FIN-OQ-027`) | Rizki | Worker kode itu tidak diaktifkan | Tidak — dan pemicunya sendiri masih tertahan `FIN-OQ-034` |
| Ratifikasi empat kode potongan piutang (`FIN-OQ-028`) | Rizki | Worker keempat kode tidak diaktifkan | Tidak — `BE-FIN-040` tetap boleh dibangun |
| Ratifikasi `PPN-MASUKAN-RETUR-PEMBELIAN` (`FIN-OQ-029`) | Rizki | Worker retur ber-PPN tidak diaktifkan | Tidak |
| Ratifikasi kedua kode penanda shift **dan** konfirmasi bahwa `Amount = 0` diterima (`FIN-OQ-030`, `032`) | Rizki | Penegakan `ACC-DEC-065` tidak dapat dijalankan | **Ya, untuk `FR-FIN-105`** — bila nilai nol tidak diterima, bentuk penandanya MUST dirancang ulang, jadi membangunnya lebih dulu berisiko dibongkar |
| Akun debit refund `REFERRED_OUTPATIENT_ADMIN` (`FIN-OQ-031`) | Billing Owner + Rizki | Refund kategori itu tercatat gagal, tidak terjurnal | Tidak — jalur `ERROR` sudah dirancang sebagai keadaan sah sementara |
| Kode potongan AR untuk jenis lain-lain (`FIN-OQ-033`) | Rizki | Jenis "lain-lain" tetap ditolak | Tidak |
| **Perbaikan gap pembalikan tender top-up deposit (`FIN-OQ-034`)** | **Billing Owner** | Saldo deposit dapat kelebihan catat tanpa jejak; kode 37 tidak pernah punya baris untuk dikirim | **Ya, untuk kelengkapan `EPIC FIN-14`** — bukan untuk gelombang `MVP-0`..`MVP-5` |
| Kredensial akun layanan (`FIN-OQ-016`) | Platform + Accounting | Worker pengiriman tidak dapat diaktifkan | **Ya, untuk `EPIC FIN-12`** |

**Status dokumen amendment ini:** `draft`. `FIN-MVP-1.5` **belum** dikunci owner, dan karena
bagian 25 memuat dua pertanyaan bertanda memblokir, bagian AMENDMENT REVISI 6 **MUST NOT**
diteruskan ke `/plan-module-delivery` sampai keduanya turun. Bagian dokumen yang sudah `locked`
sebelumnya tidak terpengaruh.

---

# AMENDMENT REVISI 7 — `FIN-MVP-1.6` (`draft`, 28 September 2026)

| Field | Nilai |
|---|---|
| Contract version | `FIN-MVP-1.6` — **`draft`** |
| `last_changed_in` | `FIN-MVP-1.6` — 28 September 2026: `FR-FIN-105` gerbangnya dipindah; `FR-FIN-108`..`110` baru; bagian 25 diperbarui |
| Diturunkan dari | `FIN-DEC-072`..`076`; `02-backend-architecture.md` AMENDMENT REVISI 7 (`FIN-DES-059`, `060`) |
| Entity/endpoint baru | **Nol.** Kelima endpoint daftar Purchasing **sudah** tercatat `contracts/api-contract.md` `B.1`-`B.5` sejak revisi 4 |

## 26. Apa yang berubah pada rilis

`FIN-DES-051`..`058` **disetujui owner** 28 September 2026 ("Saya approve"). Tiga koreksi yang
sebelumnya menunggu pengakuan kini tertutup (`FIN-DEC-072`..`074`), sehingga bagian AMENDMENT
REVISI 6 tidak lagi menyimpan asumsi yang belum diakui.

Yang **belum** selesai bukan lagi soal keputusan bisnis, melainkan tiga hal yang bergantung pihak
lain atau pada pekerjaan yang belum dijadwalkan:

| Epic | Status | Perubahan dari revisi 6 |
|---|---|---|
| `EPIC FIN-14` | Tetap `OPEN DECISION` | Sebabnya menyempit: `FIN-OQ-027`, `030`, `031`, `032`, `034`, dan `035`. Ketiga koreksi asumsi sudah tidak menjadi sebab lagi |
| `EPIC FIN-15` (Purchasing/AP) | Tetap seperti revisi 4-6 | **Satu celah implementasi dinyatakan eksplisit:** kelima `GET /` berpaging sudah dikontrak tetapi belum dibangun (`FR-FIN-109`) |
| `EPIC FIN-16` (Batch Tagihan AR) | Tetap `POST-MVP` | Butir menu "Tagihan Gabungan Penjamin" **MUST NOT** didaftarkan sebelum entity-nya ada (`FIN-CAP-032`, masih nol baris) |

## 27. Functional requirement baru dan yang gerbangnya berubah

| ID | Functional requirement | Disposisi | Bukti uji |
|---|---|---|---|
| `FR-FIN-105` | *(diperbarui)* Penanda shift tertutup terbit untuk shift `CLOSED` maupun `REVIEWED`; shift yang dibuka kembali menerbitkan penanda pembalik. **Gerbangnya berpindah** dari `FIN-OQ-030`/`032` menjadi **`FIN-OQ-035`** — baris outbox tetap ditulis, hanya worker pengirimannya yang tertahan | `MISSING / NEW` | `acceptance-test-matrix.md` D.3 |
| `FR-FIN-108` | Butir menu Purchasing mengikuti `FIN-DEC-060`: submenu "Pembelian" berlabel Indonesia, "Faktur Pembelian", dan butir flat "Tagihan Gabungan Penjamin" | `EXTEND` — frontend, murni relabel/restrukturisasi | `E.1` di bawah |
| `FR-FIN-109` | Kelima daftar berpaging Purchasing (`PO`, Tanda Terima Barang, Tukar Faktur, Faktur Pembelian, Retur) dapat dibuka petugas dengan penyaringan sesuai kontrak | `MISSING / NEW` — **kontraknya sudah ada**, kodenya belum | `E.1` di bawah |
| `FR-FIN-110` | Butir menu yang terlihat petugas tidak pernah mengarah ke layar yang menolaknya di endpoint | `OPEN DECISION` — menunggu `FIN-OQ-036` | `E.1` di bawah |

## 28. Skenario UAT tambahan

| # | Skenario | Jalur berhasil | Jalur gagal |
|---:|---|---|---|
| 1 | Petugas membuka daftar Purchase Order dari menu | Daftar tampil berpaging dengan penyaringan supplier/status/tanggal | Sebelum `FR-FIN-109` selesai: butir menu **MUST NOT** didaftarkan. Menu yang mengarah ke layar kosong dihitung **gagal**, bukan "menyusul" |
| 2 | Supervisor menutup shift, lalu Accounting menutup bulan | Penanda terbit dan tersimpan `PENDING`; sesudah `FIN-OQ-035` dijawab, terkirim dan tutup bulan berjalan | Selama `FIN-OQ-035` belum dijawab: baris menumpuk `PENDING` dan **tidak** ditandai `FAILED`. Worker yang mencoba mengirimnya lalu menandai `FAILED` dihitung gagal |
| 3 | Petugas ber-hak akses AP terbatas membuka menu Pembelian | Butir yang terlihat hanya yang benar-benar dapat dibuka | Petugas melihat butir lalu ditolak endpoint — inilah yang `FIN-OQ-036` harus mencegah |

## 29. Definition of Done tambahan

| # | Butir | Dijawab dengan |
|---:|---|---|
| 1 | Apakah label menu Purchasing sudah Bahasa Indonesia dan berada di submenu "Pembelian"? | Tangkapan layar menu + `menu-items.jsx` |
| 2 | Apakah kelima `GET /` berpaging sudah ada dan dipakai layar? | Uji integrasi + pemanggilan nyata dari frontend |
| 3 | Apakah butir menu hanya tampil bagi pengguna yang endpoint-nya benar-benar mengizinkan? | Keputusan `FIN-OQ-036` diterapkan + uji dengan dua peran berbeda |
| 4 | Apakah baris penanda shift menumpuk `PENDING` tanpa ditandai `FAILED` selama gerbang tertutup? | Uji integrasi worker dengan gerbang aktif |

## 30. Pertanyaan terbuka — pembaruan

Menggantikan baris yang bersesuaian pada bagian 25.

| Pertanyaan | Pemilik | Bila belum terjawab | Memblokir? |
|---|---|---|---|
| ~~Apakah kotak masuk Accounting menerima `Amount = 0`?~~ | Rizki | **TERJAWAB 28 September 2026 — TIDAK.** Dibaca langsung dari source; kedua jalur menolak | — |
| **Permintaan perluasan validasi `Amount = 0`, atau pengaktifan jalur pesan saldo (`FIN-OQ-035`)** | Rizki (Accounting) | Worker penanda shift tidak diaktifkan; baris tetap `PENDING` | **Ya, untuk aktivasi `FR-FIN-105`** — tidak memblokir pembangunannya |
| Ratifikasi `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` (`FIN-OQ-032`) | Rizki | Worker kode itu tidak diaktifkan | Tidak |
| **Resource hak akses butir menu Purchasing (`FIN-OQ-036`)** | Yasmin + Security Owner | Petugas dapat melihat butir menu yang endpoint-nya menolaknya | **Ya, untuk `FR-FIN-110`** — tidak memblokir `FR-FIN-108`/`109` |
| Perbaikan gap pembalikan tender top-up deposit (`FIN-OQ-034`) | **Billing Owner** | Kode 37 tidak pernah punya baris untuk dikirim | Ya, untuk kelengkapan `EPIC FIN-14` |
| Akun debit refund `REFERRED_OUTPATIENT_ADMIN` (`FIN-OQ-031`) | Billing + Rizki | Refund kategori itu tercatat `ERROR`, tidak terjurnal | Tidak |
| Kredensial akun layanan (`FIN-OQ-016`) | Platform + Accounting | Worker pengiriman tidak dapat diaktifkan | **Ya, untuk `EPIC FIN-12`** |

**Status dokumen amendment ini:** `draft`. Berbeda dari revisi 6, bagian AMENDMENT REVISI 7 **boleh**
diteruskan ke `/plan-module-delivery` untuk `FR-FIN-108` dan `FR-FIN-109` — keduanya tidak menunggu
pihak luar dan kontraknya sudah ada. `FR-FIN-105` boleh **dibangun** tetapi worker-nya digerbang;
`FR-FIN-110` **MUST NOT** masuk gelombang sampai `FIN-OQ-036` turun.

---

# AMENDMENT REVISI 13 — `FIN-MVP-1.7` (`draft`, 1 Oktober 2026)

Diturunkan dari `FIN-DEC-094`..`FIN-DEC-098` dan `FIN-DES-070`..`FIN-DES-073`.
Seluruh entity, status, hak akses, dan endpoint yang disebut di bawah **sudah** tercatat pada
`02-backend-architecture.md` AMENDMENT REVISI 13, `erd/data-dictionary.md`, dan `contracts/`.
Dokumen ini menurunkan, tidak menciptakan.

## 31. Apa yang berubah pada rilis

| Hal | Sebelum | Sesudah amendment ini |
|---|---|---|
| Bentuk navigasi Keuangan | Submenu "Pembelian" beserta beberapa kemampuan yang dikonsolidasikan ke satu layar (`FIN-DEC-060`) | Dua grup datar "Transaksi A/R" dan "Transaksi A/P" mengikuti sistem produksi V1 yang sudah lolos UAT (`FIN-DEC-094`) |
| Jawaban penjamin atas tagihan | Tidak terlacak di sistem | Terlacak sebagai sumbu kedua pada Batch Tagihan AR (`FIN-DEC-097`) |
| Selisih nominal yang tidak disetujui penjamin | — | Tampil sebagai pekerjaan yang menunggu write-off manual, **tidak** mengurangi piutang sendiri (`FIN-DES-071`) |
| Ayat Silang | Tercatat sebagai gap terbuka sejak audit awal | **Ditutup** — dilayani alokasi penerimaan yang sudah ada (`FIN-DEC-096`) |

**Yang tidak berubah:** seluruh aturan bisnis, alur persetujuan, dan endpoint yang sudah berjalan.
Pemecahan layar menambah jalan masuk, **tidak** memindahkan alur pencatatan mana pun.

## 32. `EPIC FIN-18` — Pelacakan klaim penjamin dan penyelarasan navigasi ke V1

| Kemampuan `MUST HAVE` | ID kemampuan asal | Disposisi backend |
|---|---|---|
| Petugas AR melacak jawaban penjamin atas tagihan gabungan | Baru — tidak ada di `01-existing-capability-map.md` maupun V1 yang berfungsi | **EXTEND** — tujuh kolom pada `FinReceivableInvoiceBatch`, tiga endpoint aksi |
| Petugas AR melihat selisih yang belum dihapus dari buku | Turunan dari kemampuan di atas | **EXISTING / REUSE** — dihitung pada response, nol penyimpanan baru |
| Petugas AR membuka daftar penghapusan piutang lintas piutang | `FIN-CAP` write-off (`BE-FIN-018`) | **MISSING / NEW** — satu endpoint baca |
| Petugas AR membuka daftar alokasi penerimaan yang dibalik | `FIN-CAP` alokasi (`BE-FIN-016`..`018`) | **MISSING / NEW** — satu endpoint baca |
| Seluruh layar Transaksi A/R dan A/P terjangkau dari sidebar sesuai bentuk V1 | `FIN-DEC-094` | **EXISTING / REUSE** — nol endpoint baru untuk sebelas layar hasil pemecahan |
| Ayat Silang | `FIN-DEC-096` | **LEGACY REFERENCE** — dilayani alokasi penerimaan; nol pekerjaan backend |

### Functional requirement

| ID | Requirement | Dapat diuji lewat |
|---|---|---|
| `FR-FIN-111` | Saat batch tagihan diterbitkan, sistem menandai klaimnya sebagai sudah diajukan ke penjamin tanpa tindakan tambahan petugas | `state-transition-matrix.md` `D.1` baris pertama |
| `FR-FIN-112` | Petugas AR dapat menandai berkas klaim sudah diterima dan dinyatakan lengkap oleh penjamin | `FIN-VAL-147`, `FIN-VAL-152` |
| `FR-FIN-113` | Petugas AR dapat mencatat nominal yang disetujui penjamin, termasuk nol bila klaim ditolak penuh | `FIN-VAL-148`..`150` |
| `FR-FIN-114` | Nominal yang disetujui lebih kecil dari tagihan wajib disertai alasan | `FIN-VAL-151` |
| `FR-FIN-115` | Selisih antara tagihan dan nominal yang disetujui ditampilkan dari backend, dan **tidak** mengurangi sisa piutang sampai petugas menghapusnya lewat jalur write-off yang sudah ada | `FIN-DES-071`; UAT 33.2 |
| `FR-FIN-116` | Status klaim dan status pelunasan ditampilkan berdampingan dan bergerak sendiri-sendiri | `FIN-DES-070`; UAT 33.3 |
| `FR-FIN-117` | Petugas AR dapat menutup klaim, dan klaim yang sudah ditutup tidak dapat diubah lagi | `FIN-VAL-152` |
| `FR-FIN-118` | Perubahan status klaim oleh dua petugas bersamaan ditolak pada petugas kedua, bukan saling menimpa | `FIN-VAL-153` |
| `FR-FIN-119` | Seluruh butir menu pada peta `03-frontend-architecture.md` bagian 17.2 terjangkau dari sidebar dan membuka layar yang benar | UAT 33.5 |
| `FR-FIN-120` | Layar pandangan tersaring mengirim saringan bawaannya ke backend, bukan menyaring di klien | Review kode; paginasi benar pada data lebih dari satu halaman |

## 33. Skenario UAT

| # | Jalur | Skenario | Hasil yang diharapkan |
|---|---|---|---|
| 33.1 | **Berhasil** | Terbitkan batch, tandai diverifikasi, catat persetujuan penuh, tutup klaim | Keempat perpindahan tersimpan beserta tanda waktunya; selisih nol |
| 33.2 | **Berhasil** | Penjamin menyetujui Rp 118,5 juta dari tagihan Rp 120 juta disertai alasan | Selisih Rp 1,5 juta tampil sebagai pekerjaan menunggu; **sisa piutang anggota tidak berubah sama sekali** sampai write-off disetujui |
| 33.3 | **Berhasil** | Batch berstatus klaim "Disetujui" menerima pembayaran sebagian | Status pelunasan berpindah ke "Dibayar Sebagian" oleh sistem; status klaim **tetap** "Disetujui" |
| 33.4 | **Gagal** | Aksi klaim dijalankan pada batch yang masih draft | Ditolak beserta pesan bahwa tagihan belum diterbitkan; nol perubahan tersimpan |
| 33.5 | **Berhasil** | Seluruh butir Transaksi A/R dan A/P dibuka satu per satu dari sidebar | Setiap butir membuka layar yang benar; nol butir menuju rute yang tidak ada; nol butir tersembunyi bagi peran yang berhak |
| 33.6 | **Gagal** | Petugas tanpa hak mengelola batch membuka Manajemen Klaim | Ketiga tombol aksi tidak tampil; pemanggilan langsung endpoint ditolak |
| 33.7 | **Gagal** | Nominal disetujui diisi melebihi total tagihan | Ditolak; nilai lama tidak tertimpa |

## 34. Definition of Done tambahan

| Butir | Dijawab "ya" bila |
|---|---|
| Migration kolom klaim dibuat **dan** dijalankan atas izin eksplisit pemilik repository | Ada laporan task yang mencatat keduanya terpisah |
| Nol penulis baru terhadap `OutstandingAmount` | Review kode membuktikan aksi klaim tidak memanggil service piutang |
| Nol resource dan nol action hak akses baru | Diff atribut `[AccessAction]` kosong untuk amendment ini |
| Seluruh butir menu bagian 17.2 terdaftar di `menu-items.jsx` | Peta menu dan berkas menu dapat diperiksa silang baris per baris |
| Saringan bawaan tiap layar tersaring dikirim ke backend | Review kode; nol `.filter()` atas hasil response untuk saringan bawaan |
| `FIN-OQ-040` sudah terjawab **atau** butir "Umur Piutang (A/R Aging)" sengaja belum dibuat dan dicatat sebagai sisa pekerjaan | Roadmap menyebut statusnya apa adanya, bukan didiamkan |

## 35. Urutan pengiriman

| Gelombang | Isi | Alasan urutan |
|---|---|---|
| `MVP-13A` | Menu dan layar yang **nol pekerjaan backend**: sebelas layar pandangan tersaring, Penerima Pesanan, Ayat Silang, serta penyusunan ulang kedua grup menu | Tidak bergantung pada migration mana pun; memberi hasil terlihat paling cepat |
| `MVP-13B` | Dua endpoint baca baru (`write-offs`, `reversed-allocations`) beserta kedua layarnya | Bergantung pada backend, tidak bergantung pada migration |
| `MVP-13C` | Kolom klaim, migration, tiga endpoint aksi, dan layar Manajemen Klaim | Satu-satunya yang menyentuh skema; dikerjakan terakhir supaya gelombang sebelumnya tidak tertahan izin migration |
| `MVP-13A` | Butir "Umur Piutang — Kasir" beserta saringan segmen pada `GET /receivables/aging` | `FIN-OQ-040` sudah terjawab; segmen Kasir tidak menuntut sumber piutang baru |
| **Di luar gelombang** | Butir "Umur Piutang — Parkir" dan "— Tenant" | Tertahan `FIN-OQ-043`. **MUST NOT** masuk gelombang mana pun sampai kepemilikan modul dan aturan bisnis sewa diputuskan |

`EPIC FIN-18` **tidak** berstatus `OPEN DECISION` — seluruh keputusan bisnisnya sudah turun
(`FIN-DEC-094`..`098`). Satu pertanyaan terbuka (`FIN-OQ-040`) memblokir **satu butir menu saja**
dan sudah dikeluarkan ke `POST-MVP`, sehingga tidak menahan gelombang mana pun.

## 36. Pertanyaan terbuka sebelum development lock — pembaruan

| ID | Memblokir | Keterangan |
|---|---|---|
| ~~`FIN-OQ-040`~~ | — | **CLOSED 1 Oktober 2026** — isi grupnya: Kasir, Parkir, Tenant |
| `FIN-OQ-043` | **Dua butir menu saja** (Umur Piutang Parkir dan Tenant) | Penagihan sewa parkir dan tenant adalah sumber piutang yang **belum ada sama sekali** di sistem governed, dan V1 tidak punya aturan bisnis yang bisa dirujuk (layarnya data contoh). Butuh `/grill-me` tersendiri sebelum dirancang. Tidak menahan `EPIC FIN-18` |
| `FIN-OQ-041` | Tidak | Empat pasang butir yang tampak kembar; sementara dibuat sesuai V1 apa adanya |
| `FIN-OQ-042` | Tidak | Kolom `PayerClaimReference` adalah kesimpulan desain, bukan permintaan owner |
| `FIN-OQ-039` | **Tidak untuk amendment ini** | Perluasan registry resource tanpa endpoint; amendment ini sengaja dirancang nol resource baru supaya tidak bergantung padanya (`FIN-DES-072`) |

---

# AMENDMENT REVISI 13 (lanjutan) — `FIN-MVP-1.8` (`draft`, 1 Oktober 2026)

Diturunkan dari `FIN-DEC-099`..`FIN-DEC-104` dan `FIN-DES-074`..`FIN-DES-077`.
Seluruh entity, status, hak akses, dan endpoint yang disebut sudah tercatat pada
`02-backend-architecture.md` bagian `K`, `erd/data-dictionary.md`, dan `contracts/`.

## 37. `EPIC FIN-19` — Piutang sewa non-pasien (Parkir dan Tenant)

**Batas MVP.** Mulai: petugas AR mencatat satu tagihan sewa untuk satu periode. Selesai: tagihan itu
terbaca pada laporan umur piutang sewa, dan dapat dilunasi, dihapus, atau dibatalkan.
**Di luar batas:** pencatatan uang sewa sebagai kas masuk, dan penerbitan kejadian akuntansi atas
pendapatan sewa — keduanya tertahan `FIN-OQ-044`.

| Kemampuan `MUST HAVE` | ID kemampuan asal | Disposisi backend |
|---|---|---|
| Mencatat tagihan sewa parkir/tenant per periode | Baru — tidak ada di `01-existing-capability-map.md`; V1 hanya punya layar berisi data contoh | **MISSING / NEW** — dua tabel, satu controller, satu service |
| Mencatat pelunasan dari penyewa | Baru | **MISSING / NEW** |
| Mencatat denda keterlambatan | Baru | **MISSING / NEW** — kolom nominal, bukan perhitungan |
| Menghapus piutang sewa yang tidak tertagih | Baru | **MISSING / NEW** — satu aksi, tanpa jenjang (`FIN-DEC-103`) |
| Melihat umur piutang sewa per kategori | Baru | **MISSING / NEW** endpoint; **EXISTING / REUSE** untuk definisi kelompok umurnya |

### Functional requirement

| ID | Requirement | Dapat diuji lewat |
|---|---|---|
| `FR-FIN-121` | Petugas AR dapat mencatat tagihan sewa berkategori Parkir atau Tenant untuk satu periode, lengkap dengan penyewa, objek sewa, jatuh tempo, dan nominal | `FIN-VAL-154`..`157`; UAT 38.1 |
| `FR-FIN-122` | Setiap periode dicatat sebagai tagihan tersendiri; sistem **tidak** menerbitkan tagihan otomatis dan **tidak** menyimpan kontrak sewa | UAT 38.2; review skema — nol tabel master kontrak |
| `FR-FIN-123` | Petugas AR dapat mencatat denda keterlambatan sebagai nominal, dan sistem **tidak pernah** menghitungnya sendiri | UAT 38.3 |
| `FR-FIN-124` | Petugas AR dapat mencatat pelunasan sebagian maupun penuh, dan membetulkan pelunasan keliru lewat pencatatan bernilai minus | `FIN-VAL-161`, `162`; UAT 38.4 |
| `FR-FIN-125` | Petugas AR dapat menghapus piutang sewa dalam satu aksi tanpa persetujuan siapa pun, dengan alasan yang wajib diisi | `FIN-VAL-158`; UAT 38.5 |
| `FR-FIN-126` | Tagihan yang sudah menerima pembayaran tidak dapat dikoreksi maupun dibatalkan | `FIN-VAL-159`, `160`; UAT 38.6 |
| `FR-FIN-127` | Umur piutang sewa ditampilkan per kategori memakai kelompok umur yang sama persis dengan umur piutang pasien, tetapi angkanya terpisah | UAT 38.7 |
| `FR-FIN-128` | Pencatatan piutang sewa **tidak menambah satu baris pun** pada piutang pasien, dan **tidak** menerbitkan kejadian akuntansi | UAT 38.8; review kode |

## 38. Skenario UAT

| # | Jalur | Skenario | Hasil yang diharapkan |
|---|---|---|---|
| 38.1 | **Berhasil** | Catat tagihan sewa Tenant untuk unit Lt.1 A-01, periode satu bulan | Tagihan tersimpan berstatus Belum Dibayar, bernomor, dan muncul pada daftar |
| 38.2 | **Berhasil** | Catat tagihan periode berikutnya untuk penyewa yang sama | Tersimpan sebagai baris baru yang berdiri sendiri; nol kontrak terbentuk |
| 38.3 | **Berhasil** | Tambahkan denda pada tagihan yang lewat jatuh tempo | Sisa tagihan bertambah persis sebesar nominal yang diketik; nol perhitungan otomatis |
| 38.4 | **Berhasil** | Catat pelunasan sebagian, lalu pelunasan minus untuk membetulkan kekeliruan | Sisa tagihan bergerak sesuai; kedua baris pelunasan tetap terbaca di riwayat |
| 38.5 | **Berhasil** | Hapus piutang sewa yang tidak tertagih disertai alasan | Status menjadi Dihapus **seketika**, tanpa antrean persetujuan |
| 38.6 | **Gagal** | Batalkan tagihan yang sudah menerima pembayaran | Ditolak; petugas diarahkan memakai pelunasan minus |
| 38.7 | **Berhasil** | Buka umur piutang Parkir dan Tenant | Kelompok umurnya sama persis dengan umur piutang pasien; angkanya terpisah dan tidak bercampur |
| 38.8 | **Berhasil — pemeriksaan batas** | Sesudah mencatat tagihan dan pelunasan sewa, buka kas harian, setoran bank, dan pemantauan kejadian | Sisa tagihan sewa berkurang, **tetapi** uangnya tidak muncul di ketiganya. Perilaku ini **dirancang**, dan menjadi bukti bahwa `FIN-OQ-044` perlu diputuskan sebelum dipakai pada data sungguhan |
| 38.9 | **Gagal** | Hapus piutang tanpa mengisi alasan | Ditolak; status tidak berubah |

## 39. Definition of Done tambahan

| Butir | Dijawab "ya" bila |
|---|---|
| Dua tabel baru dibuat lewat migration, dan migration dijalankan atas izin eksplisit pemilik | Laporan task mencatat keduanya terpisah |
| Nol baris bertambah pada `FinReceivable` akibat kapabilitas ini | Review kode: service ini tidak pernah memanggil `FinanceReceivableService` |
| Definisi kelompok umur dipakai ulang, bukan disalin | Review kode: memakai `ReceivableAgingBuckets` yang sudah ada |
| Layar menyatakan terang bahwa pelunasan sewa belum tercatat sebagai kas masuk | Terbaca pada layar, bukan hanya pada dokumen |
| Konfirmasi hapus/batal menyebut ketiadaan persetujuan | Terbaca pada layar |
| `FIN-OQ-044` sudah dijawab, **atau** kapabilitas ini dinyatakan dipakai terbatas sampai jawabannya turun | Tercatat apa adanya pada roadmap, bukan didiamkan |

## 40. Urutan pengiriman

| Gelombang | Isi | Alasan urutan |
|---|---|---|
| `MVP-13D` | Dua tabel, migration, controller, service, dan ketiga layar (umur piutang Parkir/Tenant, daftar tagihan sewa, rincian) | Berdiri sendiri penuh — tidak bergantung pada gelombang `MVP-13A`..`13C` maupun sebaliknya |
| `POST-MVP` | Penyambungan pelunasan sewa ke kas harian, setoran bank, dan kotak keluar Accounting | Tertahan `FIN-OQ-044`; menuntut ratifikasi kode kejadian oleh Accounting |

`EPIC FIN-19` **tidak** berstatus `OPEN DECISION` — seluruh keputusan bisnisnya sudah turun
(`FIN-DEC-099`..`104`). `FIN-OQ-044` menahan **kelengkapan akuntansinya**, bukan pembangunan
kapabilitasnya, sehingga `MVP-13D` boleh berjalan.

## 41. Pertanyaan terbuka — pembaruan

| ID | Memblokir | Keterangan |
|---|---|---|
| `FIN-OQ-044` | **Kelengkapan akuntansi `EPIC FIN-19`**, bukan pembangunannya | Uang sewa yang diterima belum tercatat sebagai kas masuk dan belum menerbitkan kejadian akuntansi. Tiga hal MUST diputuskan: (a) apakah pelunasan sewa masuk kas harian/setoran bank, dan lewat jalur apa mengingat `FinReceipt` tidak punya jalur manual; (b) kode kejadian akuntansi apa yang terbit untuk pendapatan sewa dan pelunasannya — **menuntut ratifikasi Accounting**, mengikuti pola `FIN-DEC-053`; (c) apakah rilis pertama boleh berjalan tanpa keduanya. Pemilik: Yasmin (Finance) untuk (a) dan (c); Rizki (Accounting) untuk (b) |
| ~~`FIN-OQ-043`~~ | — | **CLOSED 1 Oktober 2026** oleh `FIN-DEC-099`..`104` |
