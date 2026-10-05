# Balasan Accounting atas Surat Finance 22 dan Permintaan Kode Sewa

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tembusan | Owner Billing — untuk butir 22.1, karena `ACC-DEC-062` kami sepakati bersama Billing |
| Tanggal | 5 Oktober 2026 |
| Menjawab | `finance-management/evidence/22-balasan-finance-atas-lima-pertanyaan-accounting.md` bagian 10 (butir 22.1 sampai 22.3) dan `accounting/evidence/17-permintaan-ratifikasi-kode-sewa-dari-finance.md` (`FIN-OQ-044(b)`) |
| Sifat | **Jawaban, satu ratifikasi, satu usulan bentuk, satu usulan katalog, dan empat permintaan** |
| Dasar keputusan | `docs/module-blueprints/accounting/00-interview-decisions.md` revision 18, keputusan `ACC-DEC-132` sampai `ACC-DEC-140`, seluruhnya `approved` sisi Accounting 5 Oktober 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.7` ([cross-module-contract.md](../contracts/cross-module-contract.md)), 5 Oktober 2026 — bagian 3a.2b sampai 3a.2d, 9, dan 13 menuliskan isi surat ini. Bila berkas ini berbeda dari decision log Accounting, decision log yang berlaku |

Berkas ini berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Accounting yang lain.

---

## 1. Ringkasan satu paragraf

Terima kasih untuk surat 22, terutama bagian 3 dan 4. Mengabarkan sendiri bahwa sebuah janji
belum punya kode, sebelum kami menemukannya saat cutover, adalah hal yang paling membantu kami
merencanakan dengan jujur. Jawaban kami: **kas kasir kami minta tetap diringkas per shift**,
dipisah per metode bayar, dengan posisi cadangan bila per kuitansi memang dibutuhkan;
**`PEMBUKAAN-SHIFT-KASIR` kami ratifikasi**; dan **konvensi tanggal WIB kami terima penuh**. Untuk
sewa, keenam peristiwa kami beri kode, dipisah antara tenant dan parkir. Semua jawaban itu punya
satu tujuan: menjaga agar tutup buku kita bersama tidak macet di rekonsiliasi. Bagian 3
menjelaskan secara teknis kenapa bentuk per kuitansi, yang sekilas sama benarnya, justru membuat
Kas Kasir berselisih.

---

## 2. Yang kami terima dari surat 22 tanpa catatan

| Hal | Tanggapan |
|---|---|
| Saldo per akun control, termasuk utang jasa medis bernilai `0.00` (16.2) | Diterima. Kami juga memeriksa `SourceTransactionId` saldo Anda (`SUBLEDGER-{periode}-{kode akun}`): karena unik per akun, pernyataan ulang satu akun tidak akan tertelan deteksi kiriman ulang di kotak masuk kami |
| Saldo negatif dikirim apa adanya (16.3) | Diterima, berikut catatan Anda bahwa kandidat nyatanya tinggal Kas Kasir |
| Pernyataan ulang otomatis, hanya untuk akun yang nilainya berubah (16.4) | Diterima. Kotak masuk kami menyimpan saldo per akun per versi, jadi akun yang tidak dikirim ulang tetap memakai versi terakhirnya |
| Pelurusan surat 21 tentang Kas Kasir | Dicatat. Rumus baru di bagian 3 surat Anda kami pakai sebagai pembanding rekonsiliasi |
| G4 bersyarat | Dicatat. Rencana cutover kami tidak menghitung G4 selesai sampai Anda menyatakannya |
| Rekening bank bukan control account Finance (9.2) | Sejalan dengan pembacaan kami atas bagan akun |
| Migrasi tagihan lama tanpa kejadian (9.3) | Setuju. Mengirimnya akan menghitung ganda saldo awal manual G5 |
| `FIN-INTEGRATION-1.7` | Kami membacanya masih berstatus `draft`. Begitu Anda menyetujuinya dengan isi yang sesuai surat ini, kami mencatatnya sebagai pasangan `ACC-XMOD-0.7` |

---

## 3. Butir 22.1 — kas kasir: kami minta tetap per shift

### 3.1 Satu hal tentang mesin kami yang perlu kami luruskan dulu

Surat 22 bagian 5 menyebut kami "dapat meringkasnya menjadi satu jurnal per shift per metode lewat
aturan posting". Sayangnya mesin kami belum bisa. Cara kerjanya:

| Langkah | Perilaku |
|---|---|
| Satu kejadian diterima | Menghasilkan tepat **satu** jurnal. Tidak ada penggabungan antarkejadian |
| Aturan posting dipilih | Menurut badan hukum dan **jenis kejadian** saja |
| Baris jurnal disusun | Setiap baris aturan memetakan satu komponen ke **satu akun tetap** |
| Ruas tambahan (`CashierShiftId`, `PaymentMethodCode`, dan lainnya) | Diterima dan disimpan di pesan asli untuk penelusuran, tetapi **tidak dibaca** saat menyusun jurnal |

Pernyataan Anda bahwa kelima ruas itu tidak akan ditolak **benar**; kami sudah memeriksanya,
termasuk terhadap penyaring identitas pasien. Yang tidak terjadi hanyalah pemakaiannya untuk
memilih akun.

### 3.2 Dua masalah bila per kuitansi dipakai apa adanya

**Pertama — transfer ikut masuk Kas Kasir.** Aturan posting tidak membaca `PaymentMethodCode`,
sehingga semua `PENERIMAAN-KASIR` mendebit akun yang sama. Penerimaan transfer ikut menambah Kas
Kasir di buku besar, padahal subledger Kas Kasir Anda hanya menghitung kas fisik. Selisih muncul
setiap hari ada transfer, sedangkan toleransi rekonsiliasi kita nol.

**Kedua — shift yang melewati tengah malam.** Buku mutasi kas Anda mencatat kas shift dengan
**tanggal pembukaan shift** (`FinanceBillingIntakeService.cs` baris 1141), sedangkan kejadian per
kuitansi bertanggal **kuitansi** (`FinanceReceiptService.cs` baris 138). Contoh: shift dibuka
31 Oktober pukul 22.00 dan menerima kuitansi tunai Rp 1.000.000 pukul 02.00 tanggal 1 November.

| Sisi | Oktober | November |
|---|---:|---:|
| Subledger Kas Kasir — tanggal pembukaan shift | + Rp 1.000.000 | — |
| Buku besar — tanggal kuitansi | — | + Rp 1.000.000 |
| Rekonsiliasi | Selisih Rp 1.000.000 | Selisih Rp 1.000.000 |

Pernyataan ulang saldo tidak menolong, karena kedua angka itu benar menurut aturannya
masing-masing. Kedua bulan tertahan sampai salah satu aturan tanggal diubah.

### 3.3 Posisi utama kami: per shift, dipisah per metode bayar

| Hal | Isi |
|---|---|
| Satuan kejadian | Satu kejadian penerimaan per shift **untuk tiap metode bayar** |
| `SourceTransactionId` | Id shift, sama dengan ketiga penanda shift |
| `AccountingDate` | Tanggal pembukaan shift dalam WIB, sama dengan tanggal di buku mutasi kas Anda |
| Kode | Dipisah per metode bayar. Pola nama usulan: akhiran `-TUNAI` dan `-NONTUNAI`, misalnya `PENERIMAAN-KASIR-TUNAI` dan `PENERIMAAN-KASIR-NONTUNAI` |
| Pembalikan | Diringkas di shift pembalikan (`FIN-DEC-120`), dengan kode pembalik per metode bayar |
| Lima ruas dimensi | Silakan tetap dikirim sebagai keterangan penelusuran |

Kenapa bentuk ini mengamankan tutup buku kita bersama:

- Tanggal di buku besar dan di subledger **selalu sama**, sehingga shift lintas tengah malam tidak
  lagi menciptakan selisih.
- Tunai dan non-tunai punya kode sendiri, sehingga aturan posting dapat mendebit Kas Kasir dan bank
  ke akun yang benar tanpa perubahan mesin.
- Volume jurnal turun dari ratusan per hari menjadi beberapa per shift.
- Jejak setiap kuitansi tetap utuh di subledger Anda. Itulah pola subledger dan control account
  yang sudah kita sepakati (`ACC-DEC-062`).

Kejadian tunai dan non-tunai dari shift yang sama tidak saling bertabrakan, karena kunci
anti-ganda kami memuat `EventTypeCode`.

**Contoh.** Shift dibuka 31 Oktober pukul 22.00, menerima tunai Rp 3.000.000 (termasuk kuitansi
pukul 02.00 tadi) dan transfer Rp 1.000.000. Dikirim dua kejadian bertanggal 31 Oktober: tunai
Rp 3.000.000 dan non-tunai Rp 1.000.000. Kas Kasir Oktober di buku besar dan di subledger sama-sama
bertambah Rp 3.000.000.

**Tentang alasan ketiga Anda — shift yang dibuka kembali.** Kekhawatiran Anda tepat, dan ada satu
perilaku kotak masuk kami yang perlu Anda ketahui: untuk kejadian transaksi, `SourceVersion` yang
lebih tinggi **tidak menggantikan** versi sebelumnya. Ia diproses sebagai kejadian tersendiri dan
menghasilkan jurnal penuh lagi; hanya pesan saldo yang membandingkan versi. Jadi tambahan dari
shift yang dibuka kembali perlu dikirim sebagai kejadian tambahan, bukan sebagai versi baru dari
ringkasan yang sama. Salah satu cara yang terlihat alami adalah ringkasan per **siklus** shift,
yang hanya memuat penerimaan pada siklus itu, selaras dengan nomor siklus yang sudah Anda pakai
untuk penanda. Rancangannya kami serahkan kepada Anda.

### 3.4 Posisi cadangan — bila per kuitansi memang dibutuhkan

Kami memahami alasan Anda di bagian 5: jejak satu-ke-satu dan idempotensi yang sudah berjalan.
Bila Finance dan Billing punya kebutuhan operasional yang mendesak untuk tetap per kuitansi, kami
siap mengakomodasinya dengan dua syarat:

| # | Syarat | Masalah yang ditutup |
|---|---|---|
| a | Kode terpisah per metode bayar, seperti posisi utama | Transfer masuk Kas Kasir |
| b | `AccountingDate` setiap kuitansi = tanggal pembukaan shift tempat kuitansi itu tercatat, termasuk kuitansi pembalik yang memakai shift pembalikan (`FIN-DEC-120`) | Selisih shift lintas tengah malam |

Dengan posisi cadangan, `ACC-DEC-062` perlu kami ubah lewat keputusan baru bersama owner Billing.
Satu akibat tetap tinggal: ratusan jurnal per hari di buku besar.

### 3.5 Prinsip yang sama untuk kode lain

`PaymentMethodCode` juga dibawa `PENERIMAAN-PIUTANG` dan `PEMBAYARAN-HUTANG-SUPPLIER`
(`FIN-INTEGRATION-1.7` bagian 5.12.1). Alasannya sama: mesin kami tidak memilih akun dari isi
pesan, sehingga kedua kode itu juga perlu dipisah per metode bayar. Daftar final kodenya mohon
Anda usulkan, lalu kami ratifikasi.

Satu hal masih kami bereskan di sisi kami sendiri: satu kode non-tunai berarti satu akun debit.
Bila rumah sakit memakai beberapa rekening penampung, cara memisahkannya kami putuskan bersama
bagan akun (G2). Bila hasilnya menuntut sesuatu dari Anda, kami kabarkan.

---

## 4. Butir 22.2 — `PEMBUKAAN-SHIFT-KASIR` kami ratifikasi

Argumen Anda di bagian 8 meyakinkan: kami hanya dapat menahan shift yang kami ketahui terbuka.
Kode ini menutup pertanyaan yang kami bawa sejak surat 14.

| Hal | Keputusan |
|---|---|
| Nama | `PEMBUKAAN-SHIFT-KASIR`, apa adanya |
| Sifat | Penanda status: tanpa jurnal, tanpa aturan posting, `Amount` nol |
| Pemicu, `SourceVersion`, `AccountingDate` | Sesuai usulan Anda (`FIN-DEC-121`): pertama kali terlihat belum final; nomor siklus; tanggal shift dalam WIB |
| Daftar tertutup nilai nol | Menjadi tiga kode: `PEMBUKAAN-SHIFT-KASIR`, `PENUTUPAN-SHIFT-KASIR`, dan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` |
| Kapan diterima | Bersama penegakan shift kasir di G6. Sampai itu, mohon pengirim ketiganya tetap ditahan, seperti sekarang |
| Dasar penegakan kami | Periode tertahan selama ada `PEMBUKAAN-SHIFT-KASIR` tanpa `PENUTUPAN-SHIFT-KASIR` pada shift dan siklus yang sama, bertanggal di periode itu |

Kunci anti-ganda kami memuat `EventTypeCode`, sehingga ketiga penanda untuk Id shift dan siklus
yang sama tidak bertabrakan.

**Satu pelurusan kecil.** Surat 22 menyebut `SourceTransactionId` penanda sebagai "nomor shift".
Kontrak dan source Anda memakai **Id shift** (`BilCashierShift.Id`), dan kami mengikutinya. Kami
menyebutnya supaya contoh di kedua sisi tidak berbeda.

**Satu pertanyaan tentang waktu.** Penegakan ini hanya secepat Finance melihat shift yang terbuka.
Shift yang dibuka lalu berakhir bermasalah di antara dua pemeriksaan tetap tertangkap, karena
`CLOSED_WITH_VARIANCE` juga belum final. Tetapi bila pemeriksaan berjalan jarang, penandanya bisa
datang sesudah kami mengajukan tutup bulan. Bolehkah kami tahu perkiraan kapan worker, penjadwal,
dan pemicu otomatis penanda shift dinyalakan, dan seberapa sering status shift diperiksa? Ini
bukan tenggat; kami hanya ingin rencana G4 dan G6 kami realistis.

---

## 5. Butir 22.3 — konvensi tanggal WIB kami terima penuh

`AccountingDate` dan batas periode memakai kalender WIB; `EventOccurredAt` tetap waktu lengkap
dengan zona. Kotak masuk kami sudah menyimpan `AccountingDate` sebagai tanggal tanpa jam dan
menampilkan waktu kejadian dalam WIB, jadi tidak ada yang perlu diubah di sisi kami. Terima kasih
sudah menemukan 20 titik itu sebelum kejadian pertama mengalir.

---

## 6. Satu langkah tambahan dari sisi kami — saldo periode pertama

Saat menimbang aturan gagal tertutup Anda (bagian 6), kami menemukan celah di sisi kami sendiri,
bukan di sisi Anda. Rekonsiliasi kami baru menahan tutup bulan sejak periode saldo **pertama** yang
diterima. Bila snapshot periode pertama sesudah cutover tertahan di sisi Anda karena pemetaan belum
lengkap, kami tidak menerima apa pun, dan periode itu dapat kami tutup tanpa rekonsiliasi sama
sekali.

Celah itu kami tutup dengan daftar periksa cutover: cutover baru kami nyatakan selesai sesudah
**sedikitnya satu** snapshot `SALDO-SUBLEDGER` untuk periode pertama berstatus Tercatat di kotak
masuk kami. Satu sudah cukup, karena begitu satu saldo masuk, sistem kami sendiri menuntut saldo
untuk setiap akun control lainnya.

Dari Anda tidak ada yang diminta. Kami menyampaikannya supaya Anda tahu: bila snapshot periode
pertama tertahan karena pemetaan, tutup buku periode itu di sisi kami ikut menunggu, dan memang
itulah yang kami inginkan.

---

## 7. Jawaban atas permintaan kode sewa (surat 17)

Terima kasih sudah tidak menetapkan kode sepihak. Itu memudahkan kami merancangnya dengan benar.

### 7.1 Jawaban delapan pertanyaan

| # | Pertanyaan | Jawaban Accounting |
|---:|---|---|
| 1 | Perlukah keenam peristiwa menerbitkan kejadian akuntansi? | **Ya, keenamnya.** Piutang sewa kami akui di buku besar saat tagihan dicatat, sehingga pelunasan, denda, pembatalan, dan penghapusannya perlu lawan di buku besar |
| 2 | Kode untuk tiap kejadian | Dua belas kode, lihat bagian 7.2 |
| 3 | Kapan pendapatan sewa diakui? | Piutangnya diakui saat tagihan dicatat. Apakah kreditnya langsung ke pendapatan, atau ke pendapatan diterima di muka untuk tagihan tahunan yang lalu kami akui bulanan lewat jurnal berulang, kami tetapkan bersama pemilik proses akuntansi saat bagan akun disahkan (G2). Kedua pilihan itu **tidak** mengubah kode yang Anda kirim |
| 4 | Parkir dan tenant memakai kode yang sama atau terpisah? | **Terpisah**, karena akun pendapatan dan perlakuan pajaknya berbeda, dan aturan posting kami tidak dapat memilih akun dari isi pesan (bagian 3.1) |
| 5 | Apakah dikenai PPN? | Panduan awal di bagian 7.4; ketetapan akhir bersama PIC Pajak rumah sakit |
| 6 | Bagaimana denda dibukukan? | Denda punya kode sendiri (bagian 7.2), sehingga akunnya — pendapatan sewa atau pendapatan lain-lain — dapat kami tetapkan saat G2 tanpa Anda mengubah apa pun |
| 7 | Bagaimana penghapusan piutang dibukukan? | Dengan kode sendiri, dan di sisi kami jurnalnya masuk sebagai **Draft** (bagian 7.3). Akun debitnya ditetapkan saat G2 |
| 8 | Bagaimana dicocokkan dengan rekening koran? | **Di luar lingkup rilis ini** (bagian 7.5) |

### 7.2 Dua belas kode yang kami usulkan

| Peristiwa di Finance | Sewa unit tenant | Sewa lahan parkir |
|---|---|---|
| Tagihan dicatat | `PENGAKUAN-PIUTANG-SEWA-TENANT` | `PENGAKUAN-PIUTANG-SEWA-PARKIR` |
| Tagihan dibatalkan | `PEMBALIKAN-PENGAKUAN-PIUTANG-SEWA-TENANT` | `PEMBALIKAN-PENGAKUAN-PIUTANG-SEWA-PARKIR` |
| Denda ditambahkan | `PENGAKUAN-DENDA-SEWA-TENANT` | `PENGAKUAN-DENDA-SEWA-PARKIR` |
| Pelunasan | `PENERIMAAN-PIUTANG-SEWA-TENANT` | `PENERIMAAN-PIUTANG-SEWA-PARKIR` |
| Pelunasan dibetulkan | `PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-TENANT` | `PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-PARKIR` |
| Piutang dihapus | `PEMUTIHAN-PIUTANG-SEWA-TENANT` | `PEMUTIHAN-PIUTANG-SEWA-PARKIR` |

Bila Anda punya nama yang lebih pas, silakan usulkan. Seperti pada
`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`, kami akan memakai nama final Anda. Kode terpanjang 41
karakter, masih di bawah batas 50.

**Baris minus tidak dikirim.** Kotak masuk kami menolak kejadian bernilai nol atau negatif dengan
`400`. Pembetulan pelunasan dan pembatalan tagihan karena itu dikirim sebagai kode pembalik
bernilai **positif**. Contoh: pelunasan tercatat Rp 5.200.000 padahal yang diterima Rp 5.100.000.
Di layar Anda tetap tampil baris −Rp 100.000; kepada kami dikirim
`PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-TENANT` Rp 100.000.

**Seperti biasa, ratifikasi bukan aturan posting.** Kejadian berkode sah tersimpan Tertahan sampai
aturan postingnya kami susun di G2. Kejadian itu tidak ditolak dan tidak perlu dikirim ulang.

### 7.3 Akun control dan penghapusan

**Piutang Sewa bukan control account, untuk sekarang.** Snapshot kelompok PIUTANG Anda dihitung
dari buku mutasi piutang pasien, sedangkan piutang sewa tersimpan terpisah (`FIN-DEC-101`). Bila
akun Piutang Sewa kami tandai control account, sistem kami menuntut saldonya setiap periode, dan
tutup buku tertahan tanpa ada yang dapat mengirimnya. Karena itu:

- Kejadian sewa kami bukukan ke akun Piutang Sewa **tersendiri**, terpisah dari akun piutang
  pasien dan penjamin, supaya saldo PIUTANG yang Anda kirim tetap cocok.
- Akun itu **tidak** kami tandai control account.
- Bila kelak Anda menambahkan kelompok saldo untuk piutang sewa ke snapshot, kami siap
  menjadikannya control account. Ini tidak kami minta sekarang.

**Penghapusan masuk sebagai Draft.** Kami memahami keputusan owner Finance agar staf AR dapat
menghapus piutang sewa tanpa jenjang persetujuan (`FIN-DEC-103`). Di sisi kami, jurnal
penghapusannya dibuat sebagai Draft dan baru masuk buku besar sesudah diajukan dan disetujui dengan
aturan empat mata. Ini bukan penolakan atas keputusan Anda, melainkan pemeriksa kedua yang kita
jalankan bersama. Bila kami tidak dapat menyetujuinya, kami menghubungi Anda untuk membereskannya
bersama. Jurnal yang tertolak tetap menahan tutup bulan kami sampai beres, sehingga tidak ada kasus
yang terlupakan.

### 7.4 Pajak — panduan awal

| Objek | Panduan awal | Yang masih dipastikan PIC Pajak |
|---|---|---|
| Sewa unit tenant | PPN, dan PPh Pasal 4 ayat (2) final 10% atas sewa tanah/bangunan | Tarif dan dasar pengenaannya |
| Parkir | Pajak daerah, bukan PPN | Catatan Anda menyebut yang ditagih adalah **sewa lahan parkir** (area basement, outdoor, karyawan). Pengecualian PPN untuk parkir umumnya menyangkut penyediaan tempat parkir kepada pemilik kendaraan, sedangkan sewa lahan kepada pengelola parkir dapat diperlakukan seperti sewa tanah/bangunan. Butir ini paling perlu dipastikan |

Bila panduan itu dikonfirmasi, akibatnya bagi Anda:

- Bila PPN berlaku, kejadian pengakuan membawa dua komponen, yaitu nilai sewa dan PPN, supaya kami
  dapat mengkredit pendapatan dan PPN Keluaran secara terpisah. Nama komponennya kami tetapkan
  bersama aturan postingnya.
- Bila PPh Pasal 4 ayat (2) dipotong penyewa saat membayar, dibutuhkan satu kode potongan
  tersendiri, serupa `POTONGAN-PPH23-PIUTANG`. Kode itu kami usulkan sesudah konfirmasi PIC Pajak,
  bukan sekarang.

Pemisahan kode tenant dan parkir tetap berlaku apa pun hasilnya.

### 7.5 Rekening koran — di luar lingkup rilis ini

Pencocokan rekening koran belum ada, baik di Accounting maupun di Finance. Pada rilis ini sistem
tidak menyediakan pencocokan pelunasan sewa terhadap rekening koran, dan Anda tidak perlu
menyiapkan apa pun untuknya. Keputusan Anda agar pelunasan sewa tetap di luar kas harian dan
setoran bank (`FIN-DEC-109`) tidak perlu diubah.

---

## 8. Yang Accounting butuhkan dari Finance

| # | Permintaan | Kenapa penting | Menahan |
|---:|---|---|---|
| 18.1 | **Pilihan bentuk kas kasir** — posisi utama per shift, atau posisi cadangan per kuitansi beserta kebutuhan operasionalnya — dan **daftar final kode per metode bayar** untuk setiap kode yang membawa `PaymentMethodCode` (bagian 3) | Aturan posting kas dan bank tidak dapat disusun sebelum kodenya pasti | G4 |
| 18.2 | **Perkiraan waktu pengaktifan** worker, penjadwal, dan pemicu otomatis penanda shift, serta **frekuensi pemeriksaan** status shift (bagian 4) | Supaya rencana G4 dan G6 kami realistis | G4, G6 |
| 18.3 | **Saluran pelunasan sewa**: transfer saja, atau juga tunai? | Menentukan apakah kode pelunasan sewa juga perlu dipisah per metode bayar | Aturan posting sewa (G2) |
| 18.4 | **Nama final kode sewa**, bila berbeda dari usulan bagian 7.2 | Supaya katalog di kedua sisi sama | Tidak menahan |

Jawaban boleh digabung dalam satu surat, dan tidak satu pun menahan pembangunan di sisi Anda.
Sementara itu, kami mengerjakan bagian kami sendiri: akun piutang sewa, rekening non-tunai, dan
aturan posting bersama pemilik proses akuntansi (G2), serta konfirmasi pajak bersama PIC Pajak.

---

## 9. Keadaan gerbang cutover

| Gerbang | Isi | Keadaan 5 Oktober 2026 |
|---|---|---|
| G1 | Kotak masuk Accounting dibangun dan diuji | Development selesai dan sudah di integration; UAT belum |
| G2 | Bagan akun sah + aturan posting setiap kode aktif | Belum. Bertambah: akun piutang sewa dan rekening non-tunai |
| G3 | Akun layanan aktif, mekanismenya diputuskan | Masih terbuka bersama Platform |
| G4 | Pengirim Finance siap | **Bersyarat**, sesuai pernyataan Anda; menunggu jawaban 18.1 dan 18.2 |
| G5 | Saldo awal manual siap | Bergantung G2; angka Finance dan Accounting wajib sama (`FIN-DEC-128`) |
| G6 | Deposit, kelebihan bayar, selisih kas, uang muka, refund, dan penegakan shift kasir | Tiga kode penanda disepakati. Penegakan shift dan nilai nol penanda belum dibangun; akun refund `REFERRED_OUTPATIENT_ADMIN` belum diputuskan |
| Sesudah pengiriman dimulai | Snapshot saldo periode pertama berstatus Tercatat | Daftar periksa baru (bagian 6) |

---

## 10. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/accounting/00-interview-decisions.md` bagian *Jawaban atas surat Finance 22 dan permintaan kode sewa* | `ACC-DEC-132` sampai `ACC-DEC-140` beserta alasan dan contohnya |
| `docs/module-blueprints/accounting/contracts/cross-module-contract.md` | Kontrak `ACC-XMOD-0.7`: penanda shift (bagian 3a.2b), kode piutang sewa (3a.2c), pemisahan per metode bayar (3a.2d), sisa terbuka (9), serta gerbang cutover dan daftar periksa saldo periode pertama (13) |
| `docs/module-blueprints/accounting/evidence/16` | Surat kami sebelumnya |
| Source Accounting yang kami rujuk | `AccAccountingEventService.cs` (penyusunan jurnal dan validasi nilai), `AccPostingRuleLine.cs` (satu akun per baris aturan), `AccAccountingEventConfiguration.cs` (kunci anti-ganda) |
| Source Finance yang kami rujuk | `FinanceBillingIntakeService.cs` baris 1141, `FinanceReceiptService.cs` baris 138, `FinanceSubledgerSnapshotService.cs` baris 181, `FinanceSubledgerBalanceCalculator.cs` baris 103–107 |
