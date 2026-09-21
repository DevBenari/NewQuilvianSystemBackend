# Finance Management — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian V2 — Sistem Informasi Rumah Sakit |
| Modul | Finance Management (`finance-management`), kode modul `FIN` |
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Contract version | `FIN-MVP-1.0` |
| Status | `approved` untuk **cakupan MVP dan urutan gelombang** — disetujui Yasmin (Product/Domain Owner Finance) 20 September 2026. Approval dicatat apa adanya, tidak ditetapkan skill |
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
| Utang supplier dan pembayarannya | `FIN-CAP-009` | Tidak ada modul Purchasing sama sekali, sehingga tidak ada rantai PO-terima barang-faktur yang dapat divalidasi. Membangunnya lebih dulu berarti membangun jalur input manual yang belum tentu cocok dengan Purchasing nanti | Proses yang berjalan hari ini tetap dipakai — AP memang belum pernah ada di sistem, jadi tidak ada pekerjaan yang hilang. `MstSupplier` yang sudah ada tetap menjadi rujukan |
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
> **Contoh:** pasien membayar Rp 1.000.000 saat tagihan masih terbuka. Penerimaan langsung
> terlihat di Finance; baris kejadian akuntansinya berstatus `HELD_FOR_FINALIZATION`. Setelah
> tagihan final, status kejadian berubah menjadi siap kirim.

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

> **`FR-FIN-074` — Kejadian tertahan tidak terkirim**
> **Contoh:** penerimaan atas tagihan yang belum final menghasilkan kejadian berstatus tertahan.
> Ketika pengiriman diaktifkan nanti, baris itu **dilewati** sampai tagihannya final.

> **`FR-FIN-075` — Jasa medis tidak terbukukan dua kali**
> **Contoh:** tagihan memuat jasa medis dokter Rp 3.000.000. Kejadian `PENGAKUAN-PIUTANG`
> **tidak** memuat komponen `JASA_MEDIS`. Komponen itu hanya muncul lewat kejadian
> `PENGAKUAN-HUTANG-DOKTER` setelah fee-nya disetujui.

### EPIC FIN-12 — Pengiriman ke Accounting

**Disposisi backend:** `OPEN DECISION` — endpoint penerima belum dibangun (`FIN-CAP-018`) dan
mekanisme autentikasi belum final (`FIN-DEC-007`).

**Epic ini MUST NOT masuk gelombang pengiriman mana pun.**

### EPIC FIN-13 — Penyelarasan frontend kas kecil

**Disposisi backend:** `EXTEND` — **ditunda ke `POST-MVP`**, lihat bagian 8.

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
| `POST-MVP` | `EPIC FIN-07`, `FIN-09` — utang supplier dan pembayaran | Setelah MVP; tidak menunggu modul lain |
| `POST-MVP` | `EPIC FIN-08` — utang dokter | **Menunggu modul Medical Fee dibangun** (`FIN-CAP-021`) |
| `POST-MVP` | `EPIC FIN-13` — penyelarasan frontend kas kecil | Kapan saja; tidak mengunci apa pun |

**Tidak masuk gelombang mana pun:** `EPIC FIN-04` (piutang manfaat karyawan) dan
`EPIC FIN-12` (pengiriman ke Accounting). Keduanya berstatus `OPEN DECISION`.

`MVP-2` adalah satu-satunya gelombang yang bergantung pada tim lain. Bila `BilCollectionHandoff`
belum tersedia saat `MVP-1` selesai, gelombang `MVP-4` dapat dikerjakan lebih dahulu tanpa
mengubah urutan yang lain.

### 20.2 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| ~~Apakah owner menyetujui `FIN-DES-001`..`024`?~~ | Yasmin | — | **TERJAWAB 20 September 2026** — disetujui seluruhnya |
| ~~Apakah cakupan MVP pada bagian 7 dan 8 disetujui owner?~~ | Yasmin | — | **TERJAWAB 20 September 2026** — cakupan dan urutan gelombang dikunci apa adanya |
| Apakah owner Billing menyetujui bentuk `BilCollectionHandoff` sesuai `FIN-DEC-005`? | Billing Owner | `MVP-2` tidak dapat dimulai | **Ya, untuk `MVP-2` saja** |
| Apakah owner Billing dan HR menyetujui perluasan `BilArHandoff` untuk manfaat karyawan? | Billing Owner + HR Owner | `EPIC FIN-04` tetap `OPEN DECISION` | Tidak — epic-nya sudah dikeluarkan dari seluruh gelombang |
| Berapa ambang nominal jenjang persetujuan pembayaran AP? (`FIN-OQ-010`) | Finance Supervisor + Yasmin | `EPIC FIN-09` tidak dapat mengunci aturan validasi angkanya | Tidak — `EPIC FIN-09` sudah `POST-MVP` |
| Apakah Accounting menyetujui nama field saldo subledger? (`FIN-OQ-011`) | Rizki (Accounting) | Rumpun saldo subledger tertunda | Tidak — sudah `POST-MVP` |
| Apakah Accounting meratifikasi katalog 17 jenis kejadian? | Rizki (Accounting) | Kejadian yang jenisnya belum terdaftar akan tertahan di sisi Accounting saat pengiriman aktif | Tidak untuk MVP — kotak keluar tetap terisi |
| Kapan endpoint penerima Accounting dibangun? | Rizki (Accounting) | `EPIC FIN-12` tetap `OPEN DECISION` | Tidak — sudah dikeluarkan dari gelombang |
| Kapan modul Medical Fee dibangun? (`FIN-CAP-021`) | Owner Medical Fee | `EPIC FIN-08` tidak dapat dimulai | Tidak — sudah `POST-MVP` |

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

**Status penerusan:** dokumen ini **boleh** diteruskan ke `/plan-module-delivery` untuk
`MVP-0`, `MVP-1`, `MVP-4`, dan `MVP-5`. `MVP-2` dan `MVP-3` ditahan sampai owner Billing
menjawab.

---

**Status dokumen: `draft`.** Approval adalah tindakan manusia dan belum diberikan. Skill tidak
pernah menandai dokumen ini `approved`.
