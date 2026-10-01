# Balasan Finance atas Ratifikasi Accounting — Jawaban Tujuh Pertanyaan, Nama Final, dan Empat Kode Baru

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Rizki, owner modul Accounting |
| Tanggal | 28 September 2026 |
| Menjawab | `docs/module-blueprints/accounting/evidence/14-balasan-accounting-atas-kode-finance-05-06-07.md` — seluruh bagian, termasuk tujuh butir pada bagian 7 |
| Sifat | **Jawaban, penetapan nama final, usulan empat kode baru, dan dua koreksi atas anggapan yang kita berdua pakai** |
| Dasar keputusan | `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-063` sampai `FIN-DEC-076`, seluruhnya `approved` 28 September 2026. Arsitekturnya `02-backend-architecture.md` AMENDMENT REVISI 6 dan 7 (`FIN-DES-051`..`060`), `approved` 28 September 2026 |
| Surat pendamping | **`evidence/16`** — satu permintaan yang sengaja dipisah dari surat ini karena sifatnya berbeda: ia meminta perubahan aturan validasi di kotak masuk Anda, bukan ratifikasi nama kode |
| Kontrak yang berlaku | `ACC-XMOD-0.3`. Bentuk amplop 12 field dipakai apa adanya; tidak ada field baru yang diminta |

Berkas ini sengaja berdiri sendiri. Anda tidak perlu membuka dokumen blueprint Finance yang lain
untuk membacanya.

---

## 1. Ringkasan satu paragraf

Terima kasih. Ratifikasi Anda kami terima **seluruhnya**, termasuk ketiga permintaan pemecahan dan
kedua ratifikasi bersyarat — tidak ada satu pun yang kami tawar. Nama final untuk enam kode
pecahan mengikuti usulan Anda apa adanya, dan saran nama `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` kami
ikuti. Tujuh pertanyaan pada bagian 7 dijawab di bagian 4 surat ini, dan empat kode baru diusulkan
di bagian 5. Ada **dua hal yang perlu kami koreksi** dari anggapan yang kita berdua pakai (bagian
6) — keduanya kami temukan dengan membaca source, bukan dengan menebak, dan satu di antaranya akan
membuat penegakan `ACC-DEC-065` gagal untuk mayoritas shift bila tidak dibetulkan. Satu hal lagi
kami pisahkan ke `evidence/16` karena jawabannya ada di tangan Anda dan bentuknya permintaan, bukan
pertanyaan.

---

## 2. Nama final kode pecahan — usulan Anda kami pakai apa adanya

| Kode lama | Nama final | Dasar |
|---|---|---|
| `SELISIH-KAS-SHIFT` (ditolak) | `SELISIH-KAS-KURANG` dan `SELISIH-KAS-LEBIH` | `FIN-DEC-064` |
| `POTONGAN-PIUTANG-NON-TUNAI` (ditolak) | `POTONGAN-PPH23-PIUTANG` dan `POTONGAN-BIAYA-BANK-PIUTANG` | `FIN-DEC-065` |
| `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` (ditolak) | `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` dan `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | `FIN-DEC-065` |
| `PEMAKAIAN-DEPOSIT-RETUR` | `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` — saran Anda kami ikuti | `FIN-DEC-066` |

Alasan pemecahan Anda benar dan kami tidak berdebat soal itu: PPh 23 aset, biaya bank beban, dan
satu kode tidak boleh punya dua kemungkinan akun debit. Pemecahan itu sekaligus menjawab pertanyaan
kami sendiri di `evidence/07` bagian 5 butir 3 — jadi butir itu tertutup.

**Satu konsekuensi pemecahan yang perlu Anda ketahui.** Tabel potongan piutang kami mengenal tiga
jenis: PPh 23, biaya administrasi bank, dan **lain-lain**. Sebelum dipecah, jenis "lain-lain"
menumpang `POTONGAN-PIUTANG-NON-TUNAI`. Sesudah dipecah, ia tidak punya akun debit yang sah.
Keputusan kami: **potongan berjenis "lain-lain" kami tolak** di validasi sampai Anda meratifikasi
kode ketiga. Kami **tidak** menerima barisnya lalu diam-diam tidak menerbitkan kejadian — itu akan
mengurangi piutang tanpa jejak di buku besar Anda. Bila kasus itu memang terjadi di lapangan, kami
akan mengusulkan kodenya; sementara ini petugas diarahkan memakai salah satu dari dua jenis yang
sudah ada, atau menghubungi bagian akuntansi. Lihat bagian 7 butir 3.

---

## 3. Empat pelurusan bagian 5 — diterima, dengan satu koreksi teknis

| # | Pelurusan Anda | Tanggapan kami |
|---:|---|---|
| 1 | Kosongkan `Components`, jangan tulis teks `"TOTAL"` | **Diterima dan akan dikerjakan.** Satu koreksi teknis: kami memeriksa kotak masuk Anda dan menemukan `null` **diterima** — `AccAccountingEventService.cs` menormalkan `Components` bernilai `null` menjadi daftar kosong. Kode kami hari ini memang mengirim `null`, bukan teks `"TOTAL"`; teks itu hanya pernah muncul di **contoh dokumen** kami, dan itulah yang Anda baca. Jadi pesan kami tidak akan ditolak; pelurusannya tetap kami kerjakan supaya bentuknya persis seperti yang Anda minta, tetapi **bukan** pemblokir seperti yang kami khawatirkan |
| 2 | Hanya pesan saldo yang boleh nol atau negatif | **Diterima.** `SELISIH-KAS-SHIFT` bernilai bertanda sudah tidak ada lagi karena dipecah. Klaim `evidence/07` bagian 2.1 kami cabut. **Kecuali satu hal** — dua kode penanda pada bagian 5 surat ini memang perlu bernilai nol; itu isi `evidence/16` |
| 3 | Setiap `EventTypeCode` wajib sama persis dengan katalog | **Diterima, dan dugaan Anda tepat seluruhnya.** Kami verifikasi ke kontrak kami sendiri: `AP_CREATED` → `PENGAKUAN-HUTANG-SUPPLIER`, `AP_PAYMENT` → `PEMBAYARAN-HUTANG-SUPPLIER`, `AR_PAYMENT` → `PENERIMAAN-PIUTANG`, `AR_WRITEOFF` → `PEMUTIHAN-PIUTANG`. Lima titik tulis di kode kami akan diubah, dan kelima konstanta alias **dihapus** supaya tidak ada yang memakainya lagi |
| 4 | Contoh `evidence/06` Rp 11.000.000 seharusnya Rp 11.100.000 | **Diterima**, sudah dibetulkan di dokumen kami |

**Tentang baris yang sudah tertulis dengan nama pendek.** Seluruhnya masih berstatus belum terkirim
dan belum pernah sampai ke Anda, karena worker pengiriman kami belum pernah hidup. Baris itu
**tidak** kami timpa — ia salinan pesan yang memang pernah disusun begitu. Apa yang dilakukan
terhadapnya (dibuang, atau ditulis ulang sebagai versi baru) akan kami putuskan sebagai langkah
operasional sebelum worker diaktifkan, dan tidak akan mengejutkan Anda di tengah jalan.

---

## 4. Jawaban atas tujuh pertanyaan bagian 7

### 4.1 Pembatalan tender uang muka sesudah uang mukanya dipakai (pertanyaan 7.1)

**Hasil akhir yang Anda minta — debit Piutang, kredit Kas — kami setujui.** Cara kami mencapainya:
mengirim **dua** kejadian, bukan mengubah lawan jurnal kode yang sudah ada.

| Kejadian | Nilai | Lawan jurnal |
|---|---|---|
| `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` **(kode baru, bagian 5)** | Porsi yang sudah terpakai | Debit Piutang, kredit Uang Muka Pasien |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` (sudah diratifikasi) | Nilai tender | Debit Uang Muka Pasien, kredit Kas |

**Contoh Anda, dijawab dengan angka.** Uang muka Rp 20.000.000 dipakai melunasi tagihan
Rp 32.000.000, lalu tender Rp 20.000.000 dibatalkan:

| Kejadian | Debit | Kredit |
|---|---|---|
| `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` Rp 20.000.000 | Piutang 20.000.000 | Uang Muka Pasien 20.000.000 |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` Rp 20.000.000 | Uang Muka Pasien 20.000.000 | Kas 20.000.000 |
| **Bersih** | **Piutang 20.000.000** | **Kas 20.000.000** |

Uang Muka Pasien kembali nol — tidak minus seperti yang Anda khawatirkan — dan piutang kembali
`OUTSTANDING` sesuai `FIN-DEC-021`. Tidak ada lawan jurnal kode manapun yang berubah, sehingga
aturan "satu kode satu lawan jurnal" tetap utuh.

**Satu hal yang harus kami sampaikan dengan jujur:** skenario ini **belum dapat terjadi** di sistem
hari ini. Penjelasannya di bagian 6.2 — ini gap di Billing, bukan di Finance, dan sudah kami
teruskan ke owner Billing. Kode di atas kami siapkan supaya begitu gap itu ditutup, aturan posting
Anda sudah ada.

### 4.2 Tiga syarat `PENGAKUAN-KELEBIHAN-BAYAR` (pertanyaan 7.2)

**Ketiganya kami setujui apa adanya** (`FIN-DEC-067`):

1. Lawan jurnal tetap debit Piutang, kredit Uang Muka Pasien.
2. Kode hanya terbit bila kelebihan lahir dari pembayaran yang **mengkredit Piutang**.
3. Kode **tidak terbit** bila pembayaran asalnya `PENERIMAAN-UANG-MUKA` — kelebihannya sudah berada
   di Uang Muka Pasien, dan menerbitkannya akan mencatat kewajiban yang sama dua kali.

Syarat ketiga kami tegakkan dengan memeriksa asal kreditnya sebelum menerbitkan kejadian, dan sudah
kami tulis sebagai kriteria uji: skenario "kelebihan berasal dari uang muka pasien" **wajib**
menghasilkan nol kejadian.

**Satu perluasan yang kami usulkan, dan alasannya.** Syarat keempat Anda menyebut kelebihan dari
pembayaran yang mengkredit akun lain butuh kode tersendiri. Kami memeriksa sumber kelebihan bayar di
Billing dan menemukan satu jenis yang **ekonominya identik** dengan kasus umum: kredit bertipe
`SETTLEMENT`, yang lahir ketika uang yang benar-benar diterima melebihi tagihan yang dapat
dilunasi. Itu persis kasus "pembayaran yang mengkredit Piutang" — hanya berbeda pada **kapan**
Billing menyadarinya. Karena itu kami **tidak** mengusulkan kode baru untuknya; kami memperluas
cakupan `PENGAKUAN-KELEBIHAN-BAYAR` dan `PENGEMBALIAN-UANG-MUKA` untuk mencakupnya (`FIN-DEC-074`).
Bila menurut Anda ini perlu kode tersendiri, mohon dikoreksi — tetapi menurut kami menambah kode
untuk ekonomi yang sama hanya akan memecah laporan yang seharusnya satu.

### 4.3 Retur pembelian: PPN dan pencairan tunai (pertanyaan 7.3)

**Butir pertama — apakah kredit retur memuat PPN: ya.** Barang yang diretur lazimnya barang yang
PPN Masukannya sudah diakui saat pembelian, jadi porsi PPN memang ada (`FIN-DEC-068`). Kode untuk
porsi itu kami usulkan di bagian 5: `PPN-MASUKAN-RETUR-PEMBELIAN`, cermin persis
`PPN-MASUKAN-PEMBELIAN` seperti Anda minta.

Pemeriksaan kami menemukan tabel retur kami **belum** memisahkan PPN dari nilai pokok — hanya ada
satu kolom nilai. Kami menambahkan satu kolom `PPNAmount`, mengikuti nama yang sudah dipakai pada
faktur pembelian. Konsekuensinya, dan ini yang perlu Anda ketahui: **nilai kredit retur yang kami
catat naik menjadi pokok + PPN**, karena itulah yang supplier akui. Nilai kejadian
`RETUR-PEMBELIAN` sendiri **tidak berubah** — tetap pokok tanpa PPN, sudah sesuai syarat Anda.

**Contoh Anda, dijawab dengan angka.** Obat pokok Rp 1.000.000 + PPN Rp 110.000 diretur; supplier
memberi kredit retur Rp 1.110.000:

| Yang tercatat / terkirim | Nilai |
|---|---|
| Nilai pokok retur | Rp 1.000.000 |
| Porsi PPN retur | Rp 110.000 |
| Kredit retur yang diakui ke supplier | **Rp 1.110.000** |
| Kejadian `RETUR-PEMBELIAN` | Rp 1.000.000 — debit Piutang Retur Supplier, kredit Persediaan |
| Kejadian `PPN-MASUKAN-RETUR-PEMBELIAN` | Rp 110.000 — debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN` |

Piutang Retur Supplier bertambah Rp 1.110.000, sama dengan kredit retur yang supplier akui — persis
seperti contoh Anda.

**Butir kedua — apakah supplier pernah mencairkan kredit retur secara tunai: tidak pernah**
(`FIN-DEC-069`). Kredit retur hanya dipakai mengurangi pembayaran utang supplier berikutnya. Tidak
perlu kode debit Kas / kredit Piutang Retur Supplier. Bila kelak praktiknya berubah, kami akan
mengusulkan kodenya lebih dulu, tidak memakai kode yang ada.

### 4.4 Kode penanda shift tertutup (pertanyaan 7.4)

Kami usulkan `PENUTUPAN-SHIFT-KASIR` — **penanda status, tanpa lawan jurnal, bernilai nol**, pola
yang sama dengan pesan saldo. Kami juga mengusulkan pasangan pembaliknya, karena shift yang sudah
tertutup **dapat dibuka kembali** di sistem kami; tanpa penanda pembalik Anda akan terus menganggap
shift itu tertutup dan mengizinkan tutup bulan atas periode yang sebenarnya kembali terbuka.

Keduanya ada di bagian 5. **Dua hal penting tentang kode ini:**

1. Pemicunya **bukan** hanya `REVIEWED` seperti yang tersirat pada bagian 4.1 surat Anda — lihat
   koreksi di bagian 6.1. Ini yang paling mendesak dari seluruh surat ini.
2. Nilai nol ternyata **ditolak** kotak masuk Anda hari ini. Kami tidak mengubah bentuknya menjadi
   nilai palsu; permintaan kami ada di `evidence/16`.

### 4.5 Nama final dua kode selisih kas dan empat kode potongan (pertanyaan 7.5)

Sudah dijawab di bagian 2 — usulan Anda kami pakai apa adanya, keenamnya.

### 4.6 Sifat refund `SETTLEMENT` dan `REFERRED_OUTPATIENT_ADMIN` (pertanyaan 7.6)

Kami periksa keduanya ke source Billing, dan ternyata **keduanya berbeda asal** — tidak bisa
dijawab sebagai satu kelompok seperti pertanyaan Anda mengandaikan.

| Kategori | Asalnya | Jawaban kami |
|---|---|---|
| `SETTLEMENT` | Uang yang **benar-benar diterima** melebihi tagihan yang dapat dilunasi | Ya, bisa keluar kas. **Tidak butuh kode baru** — ekonominya identik kelebihan bayar biasa, jadi masuk `PENGAKUAN-KELEBIHAN-BAYAR` lalu `PENGEMBALIAN-UANG-MUKA`. Lihat bagian 4.2 |
| `REFERRED_OUTPATIENT_ADMIN` | Biaya administrasi rawat jalan yang **sudah dibayar**, lalu dialihkan menjadi kredit pada tagihan rawat inap. Baris biaya aslinya **tidak** dibatalkan — pendapatan administrasinya tetap terbuku | Ya, bisa keluar kas, **tetapi kami tidak dapat menentukan akun debitnya** |

**Kenapa yang kedua kami tahan.** Akun debit saat kredit itu dicairkan tunai bergantung pada satu
hal yang bukan wewenang Finance: apakah pendapatan administrasi rawat jalan itu dibalik saat
kreditnya lahir, atau kredit itu kewajiban baru di atas pendapatan yang tetap berdiri. Yang pertama
kebijakan Billing, yang kedua kebijakan Anda. Menebaknya berarti kami menetapkan keduanya sekaligus.

**Yang kami lakukan sementara ini, dan mohon dinilai apakah cukup:** setiap pengembalian tunai atas
kredit jenis ini kami catat sebagai **kegagalan sinkronisasi yang berbunyi** — barisnya muncul di
layar pantauan kami beserta sebabnya, dan **nol** kejadian diterbitkan. Kami **tidak** memakai
`PENGEMBALIAN-UANG-MUKA` untuk ini, karena lawan jurnalnya salah: tidak pernah ada uang muka yang
diakui untuk kredit tersebut. Jurnal yang seimbang tetapi keliru lebih sulit Anda temukan daripada
baris kegagalan yang terlihat sejak hari pertama.

Kami sadar ini belum menutup kekhawatiran Anda di bagian 4.2 — kas memang bisa keluar tanpa
kejadian. Bedanya, sekarang lubangnya **terlihat**, bukan senyap. Untuk menutupnya kami butuh
keputusan bersama Anda dan owner Billing; kami tidak dapat memulainya sendiri.

### 4.7 Empat pelurusan bagian 5 (pertanyaan 7.7)

Sudah dijawab di bagian 3, seluruhnya diterima.

---

## 5. Empat kode baru yang kami usulkan

Seluruhnya memakai amplop 12 field `ACC-XMOD-0.3` apa adanya. Nomor katalog kami serahkan ke Anda,
sesuai catatan Anda bahwa identitas kejadian adalah kodenya, bukan nomor urutnya.

| Kode | Dipicu oleh | `SourceTransactionId` | `Amount` | Lawan jurnal yang diusulkan |
|---|---|---|---|---|
| `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` | Pembalikan pemakaian uang muka untuk melunasi piutang | Nomor mutasi deposit | Nilai yang dibalik | Debit Piutang, kredit Uang Muka Pasien |
| `PPN-MASUKAN-RETUR-PEMBELIAN` | Retur pembelian dikonfirmasi dan membawa porsi PPN | Nomor retur | Porsi PPN | Debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN` |
| `PENUTUPAN-SHIFT-KASIR` | Shift kasir mencapai keadaan tertutup final | Nomor shift kasir | **`0`** | **Tidak ada** — penanda status, bukan transaksi |
| `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Shift yang sudah tertutup dibuka kembali | Nomor shift kasir | **`0`** | **Tidak ada** — penanda status |

### 5.1 Contoh pesan — `PPN-MASUKAN-RETUR-PEMBELIAN`

```json
{
  "EventNumber": "EVT-20261120-3f7a1c8e4b2d4a6f9c1e8d5b7a3f2e1c",
  "EventTypeCode": "PPN-MASUKAN-RETUR-PEMBELIAN",
  "SourceModule": "Finance",
  "SourceTransactionId": "RTR-2026-11-00014",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-11-20T10:05:00+07:00",
  "AccountingDate": "2026-11-20",
  "Amount": 110000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "7d6c5b4a-3e2f-4a1b-8c9d-0e1f2a3b4c5d",
  "CausationId": "7d6c5b4a-3e2f-4a1b-8c9d-0e1f2a3b4c5d"
}
```

Perhatikan: `Components` **tidak ada** sama sekali, sesuai pelurusan Anda.

### 5.2 Gerbang yang kami pasang sendiri

Untuk keempat kode ini kami memakai pola yang sama seperti `PPN-MASUKAN-PEMBELIAN`: baris kotak
keluar **ditulis sejak transaksinya terjadi**, tetapi **worker pengirimannya tidak kami aktifkan**
sebelum Anda meratifikasi. Anda tidak akan menerima kejadian berkode yang belum Anda daftarkan.

---

## 6. Dua koreksi atas anggapan yang kita berdua pakai

Keduanya kami temukan dengan membaca source, bukan menebak. Kami sampaikan apa adanya karena
keduanya menyentuh aturan yang Anda tegakkan.

### 6.1 Penanda shift tertutup: pemicunya tidak bisa hanya `REVIEWED`

Bagian 4.1 surat Anda meminta kami menerbitkan penanda shift tertutup **"pada saat shift menjadi
`REVIEWED`"**. Kami memeriksa siklus hidup shift kasir dan menemukan masalah serius pada anggapan
itu:

| Keadaan shift | Statusnya | Apakah mencapai `REVIEWED`? |
|---|---|---|
| Kas fisik **pas** (tidak ada selisih) | `CLOSED`, dan berhenti di situ | **Tidak pernah** |
| Ada selisih, sudah disahkan | `REVIEWED` | Ya |
| Ada selisih, belum disahkan | `CLOSED_WITH_VARIANCE` | Belum |
| Selisih perlu tindak lanjut | `PERLU_TINDAK_LANJUT` | Belum |

**Shift yang kasnya pas — yang seharusnya mayoritas shift harian — tidak pernah melewati
`REVIEWED`.** Bila kami mengikuti permintaan Anda apa adanya, penanda tertutup hanya terbit untuk
shift yang **bermasalah**, dan setiap shift yang bersih akan Anda perlakukan sebagai shift yang
belum ditutup. Akibatnya `ACC-DEC-065` menahan tutup bulan **selamanya**, dan justru shift yang
paling tidak bermasalah yang menyebabkannya.

**Yang kami kerjakan (`FIN-DEC-072`):** penanda terbit pada **kedua** keadaan tertutup final —
`CLOSED` (tanpa selisih) **dan** `REVIEWED` (selisih sudah disahkan). Dua keadaan sisanya,
`CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT`, **tidak** menerbitkan apa pun — keduanya memang
harus tetap menahan tutup bulan Anda, dan itu sesuai maksud `ACC-DEC-065`.

Mohon dikonfirmasi bahwa ini sesuai maksud Anda. Bila Anda memang hanya ingin penanda untuk shift
yang pernah bermasalah, tolong beri tahu kami — tetapi kami menduga itu bukan yang Anda maksud.

### 6.2 Skenario pertanyaan 7.1 belum dapat terjadi, dan penyebabnya di Billing

Kami memeriksa kedua jalur yang bisa menghasilkan pembatalan uang muka:

| Jalur | Apa yang terjadi |
|---|---|
| Pembalikan top-up deposit lewat menu deposit | **Ditolak sistem** bila dananya sudah terpakai — dan itu tepat kondisi pada pertanyaan Anda |
| Pembatalan tender yang mendanai top-up (mis. kartu ditarik penerbit) | **Tidak menulis mutasi deposit apa pun.** Saldo deposit tetap mencatat uang yang sebenarnya tidak pernah jadi diterima |

Jalur kedua adalah gap yang lebih besar dari pertanyaan Anda: bukan hanya
`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` yang tidak punya pemicu — `PEMBALIKAN-PENERIMAAN-UANG-MUKA`
yang **sudah** Anda ratifikasi pun tidak akan terbit pada kasus itu, padahal itu justru kasus yang
paling membutuhkannya.

Ini **tidak dapat kami perbaiki dari Finance**: menulis mutasi pembalik berarti kami menulis ke
tabel milik Billing, dan itu dilarang batas modul kami. Yang kami pasang adalah **pendeteksinya**:
setiap pembatalan tender top-up deposit yang tidak punya mutasi pembalik akan muncul sebagai
kegagalan sinkronisasi yang berbunyi di layar pantauan kami, bukan dilewati senyap. Permintaan
perbaikan sudah kami teruskan ke owner Billing sebagai surat tersendiri.

Kami sampaikan ini supaya Anda tahu: aturan posting untuk kode itu boleh Anda susun sekarang, tetapi
kejadiannya belum akan mengalir sampai Billing menutup gap-nya.

---

## 7. Yang Finance butuhkan dari Accounting

| # | Butuh | Menahan apa |
|---:|---|---|
| 1 | Ratifikasi **empat kode baru** pada bagian 5 beserta lawan jurnalnya | Aktivasi worker pengiriman keempat kode. Tidak menahan pembangunannya |
| 2 | **Konfirmasi koreksi bagian 6.1** — penanda shift terbit pada `CLOSED` **dan** `REVIEWED` | Penegakan `ACC-DEC-065`. Ini yang paling mendesak dari seluruh surat ini |
| 3 | **Kode untuk potongan piutang jenis "lain-lain"**, bila Anda menilai jenis itu perlu ada. Bila menurut Anda tidak perlu, pernyataan tertulis bahwa dua jenis (PPh 23 dan biaya bank) sudah cukup juga menyelesaikannya | Selama belum ada, potongan jenis itu kami tolak di validasi |
| 4 | **Akun debit refund `REFERRED_OUTPATIENT_ADMIN`** (bagian 4.6) — keputusan bersama Anda dan owner Billing | Refund kategori itu tercatat sebagai kegagalan, tidak terjurnal |
| 5 | Penilaian atas **perluasan cakupan `SETTLEMENT`** (bagian 4.2/4.6): kami tidak mengusulkan kode baru untuknya. Bila Anda tidak setuju, mohon dikoreksi | Tidak menahan apa pun; kami sudah berjalan dengan tafsir ini |
| 6 | Hal yang ada di **`evidence/16`** — permintaan agar kotak masuk Anda menerima nilai nol untuk kedua kode penanda, atau mengaktifkan jalur pesan saldo | Aktivasi worker kedua kode penanda |

---

## 8. Tiga perubahan perilaku pada kejadian yang sudah berjalan

Kami sampaikan supaya tidak mengejutkan Anda saat pengiriman hidup.

| # | Yang berubah | Sebelum | Sesudah |
|---:|---|---|---|
| 1 | Nama empat `EventTypeCode` | `AP_CREATED`, `AP_PAYMENT`, `AR_PAYMENT`, `AR_WRITEOFF` | Nama katalog (bagian 3 butir 3) |
| 2 | Nilai kredit retur supplier | Pokok saja | Pokok + PPN (bagian 4.3). **Nilai kejadian `RETUR-PEMBELIAN` tidak berubah** |
| 3 | Bentuk pesan | Selalu memuat `"Components": null` | Properti dihilangkan bila tidak ada komponen |

Ditambah satu yang sudah kami sampaikan di `evidence/07` dan tetap berlaku: nilai
`PEMBAYARAN-HUTANG-SUPPLIER` berkurang sebesar porsi kredit retur yang dipakai, dan porsi itu pindah
ke `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`. Pembayaran yang tidak memakai kredit retur tidak berubah.

---

## 9. Satu catatan tentang kotak masuk Anda yang sudah kami baca

Anda menyebut di bagian 4.4 bahwa kotak masuk sudah dibangun dan diuji pengembang. Kami sudah
membacanya untuk menyiapkan worker kami, dan ada **satu hal yang ingin kami pastikan** supaya tidak
salah menanganinya:

Pesan yang sudah pernah diterima dijawab **`200`** beserta keterangan bahwa tidak ada jurnal baru
yang dibuat, sedangkan pesan baru dijawab `201`. Kami memperlakukan **keduanya sebagai sukses**.
Bila worker kami memperlakukan apa pun selain `201` sebagai kegagalan, ia akan mengulang pesan yang
sudah Anda terima dan akhirnya menandainya gagal padahal jurnalnya sudah terbentuk di sisi Anda.
Mohon dikoreksi bila tafsir kami salah.

Satu hal kecil: tanda terima Anda memuat `AccountingEventId` dan `EventNumber`, tetapi tidak ada
"nomor tanda terima" tersendiri. Kolom kami bernama `AccountingReceiptNumber` — kami akan mengisinya
dari `AccountingEventId`. Penamaan kolom kami yang kurang tepat, bukan field Anda yang kurang.

---

## 10. Keadaan pekerjaan Finance saat ini

| Hal | Keadaan 28 September 2026 |
|---|---|
| Keputusan bisnis | `FIN-DEC-063`..`076` — seluruhnya `approved` |
| Rancangan arsitektur | `FIN-DES-051`..`060` — `approved` 28 September 2026 |
| Katalog kode | Enam kode pecahan dinamai final; empat kode baru diusulkan surat ini; lima nama pendek akan dihapus |
| Kolom baru | Satu — porsi PPN pada retur pembelian. Satu migration, belum dijalankan |
| Worker pengiriman | **Belum hidup.** Selain ratifikasi Anda, masih menunggu mekanisme kredensial akun layanan bersama Platform |

---

## 11. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/00-interview-decisions.md` | `FIN-DEC-063`..`076` beserta alasan dan contohnya |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` bagian 5.10 dan 5.11 | Katalog final, empat pelurusan, dan kontrak as-is kotak masuk Anda sebagaimana kami baca |
| `docs/module-blueprints/finance-management/02-backend-architecture.md` AMENDMENT REVISI 6 dan 7 | Kapan tepatnya setiap kejadian ditulis, beserta bukti source untuk kedua koreksi bagian 6 |
| `docs/module-blueprints/finance-management/evidence/16-permintaan-perluasan-validasi-kotak-masuk-accounting.md` | Permintaan yang sengaja dipisah dari surat ini |
