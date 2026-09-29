# Balasan Accounting atas Usulan Kode Kejadian Finance — `evidence/05`, `06`, dan `07`

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tanggal | 28 September 2026 |
| Menjawab | `finance-management/evidence/04-jawaban-atas-balasan-accounting.md` beserta koreksinya `05-koreksi-jawaban-atas-balasan-accounting.md` (`FIN-OQ-017`); `06-usulan-kode-ppn-masukan-untuk-accounting.md` (`FIN-OQ-020`); `07-usulan-empat-kode-potongan-retur-deposit-untuk-accounting.md` (`FIN-OQ-026`) |
| Sifat | **Ratifikasi, koreksi, dan pertanyaan balik — tiga surat dijawab dalam satu balasan**, sesuai tawaran Anda di `evidence/07` bagian 6 |
| Dasar keputusan | `docs/module-blueprints/accounting/00-interview-decisions.md` revision 13, keputusan `ACC-DEC-098` sampai `ACC-DEC-106`, seluruhnya `approved` sisi Accounting 28 September 2026 |
| Kontrak yang berlaku | `ACC-XMOD-0.3` ([cross-module-contract.md](../contracts/cross-module-contract.md)). Bila berkas ini berbeda dari decision log Accounting, decision log yang berlaku |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint
Accounting yang lain.

---

## 1. Ringkasan satu paragraf

Terima kasih atas ketiga surat dan kesabarannya. Dari **dua belas** kode yang Anda usulkan,
**tujuh diratifikasi apa adanya**, **dua diratifikasi bersyarat**, dan **tiga diminta dipecah**
karena satu kode membawa lebih dari satu lawan jurnal — pola yang sama-sama kita hindari sejak
`PENERIMAAN-KASIR`. Bentuk pesan saldo subledger yang Anda terima (`FIN-DEC-035`) sudah membuka
gerbang Wave D kami. Empat kode uang muka **sudah diratifikasi**, jadi perubahan `FIN-DEC-030` di
`FinanceReceiptService` boleh Anda mulai sekarang. Ada empat pelurusan kecil yang perlu diperbaiki
sebelum worker pengiriman Anda hidup (bagian 5), dan beberapa pertanyaan balik (bagian 7).

---

## 2. Keputusan per kode

"Diratifikasi" berarti nama dan lawan jurnalnya disepakati. **Ratifikasi bukan aturan posting**:
kejadian berkode sah tetap tersimpan **Tertahan** sampai aturan postingnya kami susun di atas bagan
akun rumah sakit yang sah (gerbang G2). Akun persis pada kolom lawan jurnal ditetapkan saat itu.

| Kode usulan Finance | Keputusan Accounting | Lawan jurnal | Yang perlu Finance lakukan | Dasar |
|---|---|---|---|---|
| `SALDO-SUBLEDGER` | **Disepakati** — bentuk pesan persis bagian 3.3 surat kami | Bukan jurnal; dicocokkan dengan buku besar saat tutup bulan | Tidak ada | `ACC-DEC-087`, `FIN-DEC-035` |
| `PENERIMAAN-UANG-MUKA` | **Diratifikasi** | Debit Kas, kredit Uang Muka Pasien | Boleh mulai mengubah kode | `ACC-DEC-098` |
| `PEMAKAIAN-UANG-MUKA-DEPOSIT` | **Diratifikasi** | Debit Uang Muka Pasien, kredit Piutang | Boleh mulai | `ACC-DEC-098` |
| `PENGEMBALIAN-UANG-MUKA` | **Diratifikasi** | Debit Uang Muka Pasien, kredit Kas | Boleh mulai | `ACC-DEC-098` |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | **Diratifikasi**, dengan satu pertanyaan balik | Debit Uang Muka Pasien, kredit Kas | Jawab pertanyaan 7.1 sebelum G6 | `ACC-DEC-098` |
| `PENGAKUAN-KELEBIHAN-BAYAR` | **Diratifikasi bersyarat** | Debit Piutang, kredit Uang Muka Pasien | Konfirmasi syarat bagian 3.2 | `ACC-DEC-100` |
| `SELISIH-KAS-SHIFT` | **Tidak diratifikasi** dalam bentuk bernilai bertanda — **dipecah dua** | Lihat bagian 3.3 | Tetapkan nama final dua kode | `ACC-DEC-099` |
| `PPN-MASUKAN-PEMBELIAN` | **Diratifikasi**; akun debit ditetapkan di G2 | Debit PPN Masukan **atau** beban PPN tak dapat dikreditkan, kredit Utang Supplier | Tidak ada | `ACC-DEC-103` |
| `POTONGAN-PIUTANG-NON-TUNAI` | **Tidak diratifikasi** sebagai satu kode — **dipecah dua** | Lihat bagian 3.5 | Tetapkan nama final dan pecah sebelum aktivasi | `ACC-DEC-104` |
| `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` | Ikut **dipecah dua** | Kebalikan pasangannya | Sama dengan baris di atas | `ACC-DEC-104` |
| `RETUR-PEMBELIAN` | **Diratifikasi bersyarat** — nilai pokok tanpa PPN | Debit Piutang Retur Supplier, kredit Persediaan | Jawab pertanyaan 7.3 | `ACC-DEC-105` |
| `PEMAKAIAN-DEPOSIT-RETUR` | **Diratifikasi** | Debit Utang Supplier, kredit Piutang Retur Supplier | Opsional: pertimbangkan nama baru | `ACC-DEC-106` |

**Nomor urut katalog tidak mengikat.** Anda menanyakan urutan 18–29 (`evidence/06` dan `07`
bagian 5 butir 2). Identitas sebuah kejadian adalah **kodenya**, bukan nomor urutnya — nomor Anda
sudah tidak cocok sejak `SELISIH-KAS-SHIFT` dipecah dua. Urutan resmi kami catat di `ACC-XMOD`
bagian 3a saat katalog diperbarui.

---

## 3. Rincian kode yang bersyarat atau dipecah

### 3.1 Syarat keempat kode uang muka

Saldo Uang Muka Pasien **hanya boleh berkurang** lewat tiga kode: `PEMAKAIAN-UANG-MUKA-DEPOSIT`,
`PENGEMBALIAN-UANG-MUKA`, atau `PEMBALIKAN-PENERIMAAN-UANG-MUKA`. Tidak ada kode lain yang mendebit
Uang Muka Pasien.

### 3.2 `PENGAKUAN-KELEBIHAN-BAYAR` — satu lawan jurnal, tiga syarat

`evidence/05` bagian 3.3 menulis lawan jurnalnya "biasanya Piutang atau Pendapatan". Satu jenis
kejadian hanya punya satu aturan posting aktif, dan karena seluruh pesan Anda bernilai `TOTAL`,
aturan itu hanya dapat memuat **satu** akun debit. Karena itu:

1. Lawan jurnalnya tetap: **debit Piutang, kredit Uang Muka Pasien**.
2. Kode ini hanya terbit bila kelebihan lahir dari pembayaran yang **mengkredit Piutang** — kasus
   umumnya `ALLOCATION_EXCESS` sesudah pembayaran dialokasikan ke tagihan final.
3. Kode ini **tidak boleh** terbit bila pembayaran asalnya `PENERIMAAN-UANG-MUKA`, karena
   kelebihannya sudah berada di Uang Muka Pasien.
4. Kelebihan dari pembayaran yang mengkredit akun lain, misalnya Pendapatan, butuh kode tersendiri
   — mohon diusulkan bila kasus itu memang terjadi.

**Contoh.** Pasien membayar Rp 500.000 untuk tagihan final Rp 450.000 lewat `PENERIMAAN-PIUTANG`
(debit Kas, kredit Piutang Rp 500.000), sehingga Piutang lebih kredit Rp 50.000. Anda mengirim
`PENGAKUAN-KELEBIHAN-BAYAR` Rp 50.000: debit Piutang, kredit Uang Muka Pasien. Piutang tagihan itu
kembali nol, dan Rp 50.000 tercatat sebagai kewajiban kepada pasien.

**Contoh yang dicegah syarat 3.** Pasien menitip uang muka Rp 500.000 (`PENERIMAAN-UANG-MUKA`),
tagihan finalnya Rp 450.000, dan pemakaian uang muka Rp 450.000 terbit. Sisa Rp 50.000 **sudah**
berada di Uang Muka Pasien. Bila `PENGAKUAN-KELEBIHAN-BAYAR` tetap terbit, Uang Muka Pasien
bertambah Rp 50.000 lagi — kewajiban yang sama tercatat dua kali — dan Piutang bertambah Rp 50.000
yang tidak pernah ditagih kepada siapa pun.

### 3.3 `SELISIH-KAS-SHIFT` — dipecah menjadi dua kode bernilai positif

`evidence/04` mengirim selisih kas sebagai **satu kode bernilai bertanda** (`Amount: -30000.00`
untuk kekurangan), dengan lawan jurnal "akun selisih kas (suspense)". Ada tiga benturan:

| Benturan | Aturan Accounting |
|---|---|
| Nilai negatif | Kejadian transaksi bernilai nol atau negatif ditolak `400` "Nilai kejadian harus lebih besar dari nol." Hanya **pesan saldo** yang boleh nol atau negatif |
| Satu kode dua arah | Aturan posting punya sisi debit dan kredit yang tetap, jadi tanda nilai tidak dapat membalik arah jurnal |
| Akun sementara | Accounting tidak memakai akun sementara (*suspense*) sama sekali (`ACC-DEC-046`) |

**Permintaan kami** — nama final dari Anda:

| Usulan nama | Kapan | Lawan jurnal | `Amount` |
|---|---|---|---|
| `SELISIH-KAS-KURANG` | Kas fisik lebih kecil dari catatan sistem | Debit beban selisih kas, kredit Kas | Selisihnya, positif |
| `SELISIH-KAS-LEBIH` | Kas fisik lebih besar dari catatan sistem | Debit Kas, kredit pendapatan selisih kas | Selisihnya, positif |

**Contoh.** Kas shift 20 November 2026 kurang Rp 30.000. Anda mengirim `SELISIH-KAS-KURANG` bernilai
`30000.00`: debit beban selisih kas, kredit Kas. Bila bentuk lamanya yang terkirim —
`SELISIH-KAS-SHIFT` bernilai `-30000.00` — kejadian itu ditolak `400` dan tidak tersimpan.

Pemicu dan tanggal akuntansinya (`FIN-DEC-043`: terbit saat `REVIEWED`, bertanggal shift) tidak
kami ubah — itu wewenang Anda.

### 3.4 `PPN-MASUKAN-PEMBELIAN` — akun debit menunggu pemilik pajak

Kode dan sisi kreditnya (Utang Supplier) kami ratifikasi. Pemisahan nilai pokok dan PPN yang sudah
Anda bangun (`AP_CREATED` bernilai total dikurangi PPN) juga tepat. Yang **belum** kami kunci adalah
akun debitnya:

- bila PPN Masukan **dapat dikreditkan**, debitnya aset PPN Masukan;
- bila **tidak dapat**, debitnya beban PPN yang tidak dapat dikreditkan.

**Kenapa belum dikunci.** `FIN-DEC-054` memperlakukan tagihan rumah sakit ke penjamin sebagai bebas
PPN. Bila rumah sakit hampir tidak punya PPN Keluaran, PPN Masukan umumnya tidak dapat dikreditkan —
mencatatnya sebagai aset membuat nilainya menggantung di neraca tanpa pernah terpakai. Keputusan ini
milik pemilik proses akuntansi bersama pemilik pajak rumah sakit, dan diambil saat aturan posting
disusun (G2). **Bagi Finance tidak ada yang berubah**: bentuk pesan dan kodenya sama di kedua
kemungkinan.

**Contoh.** Faktur obat berharga barang Rp 10.000.000 ditambah PPN 11% Rp 1.100.000, total
Rp 11.100.000. Pengakuan utang Rp 10.000.000 dan `PPN-MASUKAN-PEMBELIAN` Rp 1.100.000; Utang
Supplier bertambah Rp 11.100.000. *(Contoh `evidence/06` menulis total Rp 11.000.000 untuk angka
yang sama — mohon dibetulkan di dokumen Anda.)*

Pertanyaan Anda soal "langsung mengurangi PPN Keluaran bulan berjalan atau menunggu SPT Masa"
berada di luar kotak masuk kejadian dan tidak kami putuskan dalam surat ini.

### 3.5 Potongan piutang — dua pasang kode

PPh 23 yang dipotong penjamin adalah **aset** (kredit pajak), sedangkan biaya administrasi bank
adalah **beban**. Satu kode hanya dapat punya satu akun debit, jadi kami menerima tawaran Anda di
`evidence/07` bagian 5 butir 3 untuk memecahnya. Usulan nama — nama final dari Anda:

| Usulan nama | Lawan jurnal |
|---|---|
| `POTONGAN-PPH23-PIUTANG` | Debit PPh 23 dibayar di muka, kredit Piutang |
| `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` | Debit Piutang, kredit PPh 23 dibayar di muka |
| `POTONGAN-BIAYA-BANK-PIUTANG` | Debit beban administrasi bank, kredit Piutang |
| `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | Debit Piutang, kredit beban administrasi bank |

Keempatnya bernilai positif. Pemicunya, termasuk pembalikan otomatis karena tender Billing
dibatalkan, mengikuti `FIN-DEC-055` dan `062` apa adanya.

**Contoh.** Piutang PT Asuransi Rp 10.000.000; yang masuk ke rekening Rp 9.745.000. Terbit tiga
kejadian: pelunasan piutang Rp 9.745.000, `POTONGAN-PPH23-PIUTANG` Rp 230.000, dan
`POTONGAN-BIAYA-BANK-PIUTANG` Rp 25.000. Piutang lunas Rp 10.000.000; kredit pajak Rp 230.000 tampak
di neraca, beban bank Rp 25.000 di laba rugi.

Kami sengaja tidak memilih jalan "satu kode + rincian `Components`": jalan itu menuntut Anda
mengubah `FIN-DEC-038`, dan aturan dua komponen akan menahan setiap potongan yang hanya membawa satu
komponen.

### 3.6 Retur pembelian — pokok tanpa PPN

`RETUR-PEMBELIAN` kami ratifikasi dengan dua syarat:

1. Kreditnya **satu** akun, Persediaan — bukan "Persediaan/Pembelian". Akun persisnya ditetapkan
   di G2.
2. Nilainya **pokok tanpa PPN**. Bila barang yang diretur membawa PPN, **porsi PPN dikirim lewat
   kode terpisah** — cermin `PPN-MASUKAN-PEMBELIAN`: debit Piutang Retur Supplier, kredit akun yang
   sama dengan debit `PPN-MASUKAN-PEMBELIAN`. Mohon diusulkan kodenya.

**Contoh.** Obat berharga Rp 1.000.000 + PPN Rp 110.000 diretur; supplier memberi kredit retur
Rp 1.110.000. `RETUR-PEMBELIAN` Rp 1.000.000 dan kode PPN retur Rp 110.000; Piutang Retur Supplier
Rp 1.110.000 sama dengan kredit returnya. **Yang dicegah:** bila `RETUR-PEMBELIAN` mengkredit
Persediaan Rp 1.110.000, persediaan berkurang Rp 110.000 terlalu banyak, sementara PPN Masukan
Rp 110.000 tetap tercatat.

`PEMAKAIAN-DEPOSIT-RETUR` kami ratifikasi apa adanya, termasuk turunnya nilai pembayaran utang
supplier sebesar porsi kredit retur (`FIN-DEC-061`). **Saran nama, tidak mengikat:** kata "DEPOSIT"
mudah tertukar dengan deposit pasien (`PEMAKAIAN-UANG-MUKA-DEPOSIT`); nama seperti
`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` lebih jelas. Bila Anda tetap memakai nama semula, ratifikasinya
tetap berlaku.

---

## 4. Jawaban atas pertanyaan Anda

### 4.1 Batas waktu pengesahan selisih kas shift (`evidence/05` bagian 3.4)

**Sebelum Accounting mengajukan penutupan periode tempat shift itu jatuh** (`ACC-DEC-101`). Shift
yang selisihnya belum disahkan kami perlakukan sebagai shift yang belum ditutup — dan shift yang
belum ditutup menahan tutup bulan (`ACC-DEC-065`). Untuk menegakkannya, kami meminta Finance
menerbitkan kejadian penanda shift tertutup **pada saat shift menjadi `REVIEWED`**; kode dan bentuknya
kita sepakati kemudian (lihat pertanyaan 7.4).

**Satu anggapan yang perlu diluruskan.** Kejadian untuk periode yang sudah ditutup **tetap
dijurnal**, yaitu ke periode terbuka berikutnya dengan tanggal dokumen asli (`ACC-DEC-047`). Tanda
terima Anda memuat `AccountingPeriodCode` tempat jurnal itu jatuh. Kerugiannya hanya satu: angka
bulan asalnya kurang selamanya. **Contoh:** shift 31 Oktober kurang Rp 30.000 dan baru disahkan
10 November sesudah Oktober ditutup. Jurnalnya jatuh di November dengan tanggal dokumen 31 Oktober.

### 4.2 Refund kategori `SETTLEMENT` dan `REFERRED_OUTPATIENT_ADMIN` (`evidence/05` bagian 5 butir 3)

**Perlu, dan menjadi bagian gerbang G6** (`ACC-DEC-102`). Setiap refund yang mengeluarkan uang
tunai harus sampai ke buku besar. **Contoh bahaya:** kasir mengembalikan Rp 750.000 kepada pasien
rujukan tanpa kejadian apa pun. Kas di buku besar Rp 750.000 lebih besar dari kas Finance, dan
begitu rekonsiliasi toleransi nol kami berjalan (`ACC-DEC-076`), tutup bulan tertahan tanpa ada yang
tahu sebabnya. Bila ternyata kedua kategori itu **tidak pernah** mengeluarkan kas, pernyataan
tertulis dari Anda sudah cukup.

### 4.3 Baris `HELD_FOR_FINALIZATION` di "kontrak `ACC-XMOD` bagian 5.5" (`evidence/04` bagian 3 dan 9)

Baris itu **tidak ada** di kontrak Accounting mana pun; bagian 5 `ACC-XMOD` berisi jaminan Accounting
kepada penerbit. Yang Anda maksud adalah kontrak integrasi Finance sendiri, bagian 5.5. Tidak ada
yang perlu kami hapus.

### 4.4 Kotak masuk Accounting — gerbang G1 (`evidence/04` bagian 9 butir 3)

Kotak masuk **sudah dibangun dan diuji pengembang** (Wave B selesai 28 September 2026); uji
penerimaan (UAT) oleh tim terpisah **belum** dijalankan, jadi G1 belum kami nyatakan lolos. Kodenya
ada di branch Accounting dan masuk integration lewat pull request berikutnya.

#### Corporate - Accounting - Accounting Event

Base URL: `api/v1/corporate/accounting/accounting-events`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Menerima satu kejadian keuangan dari Finance | `AccountingEvent : Receive` — hanya akun layanan | `ReceiveAccountingEventRequest` | `ApiResponse<AccountingEventReceiptDto>` — `201`, `200`, `400`, `403`, `409`, `422` sesuai `ACC-XMOD-0.3` bagian 4 |

---

## 5. Empat pelurusan sebelum worker pengiriman Anda hidup

| # | Yang ada di dokumen atau kode Finance | Yang benar | Akibat bila dibiarkan |
|---:|---|---|---|
| 1 | Seluruh contoh pesan menulis `"Components": "TOTAL"` sebagai **teks** | **Kosongkan** `Components`. Pesan tanpa `Components` sudah berarti seluruh nilai memakai komponen `TOTAL`. `Components` adalah daftar `{ComponentCode, Amount}`; pesan saldo tidak boleh membawanya sama sekali | Setiap pesan ditolak `400` |
| 2 | `evidence/07` bagian 2.1: `SELISIH-KAS-SHIFT` dan `SALDO-SUBLEDGER` "boleh bernilai negatif, seperti yang sudah disepakati" | Hanya `SALDO-SUBLEDGER` yang boleh nol atau negatif. `SELISIH-KAS-SHIFT` dipecah (bagian 3.3) | Kejadian bernilai negatif ditolak `400` |
| 3 | Kotak keluar memakai nama pendek `AP_CREATED`, `AP_PAYMENT`, `AR_PAYMENT`, `AR_WRITEOFF` (diakui `evidence/07` bagian 5 butir 4) | Setiap `EventTypeCode` wajib sama persis dengan katalog. Pemetaannya Anda yang menetapkan; dugaan kami `PENGAKUAN-HUTANG-SUPPLIER`, `PEMBAYARAN-HUTANG-SUPPLIER`, `PENERIMAAN-PIUTANG`, `PEMUTIHAN-PIUTANG`. Ini menjadi syarat gerbang G4 | Kejadian tersimpan **Tertahan** `EVENT_TYPE_NOT_REGISTERED` dan tidak pernah menjadi jurnal |
| 4 | Contoh `evidence/06` total Rp 11.000.000 untuk barang Rp 10.000.000 + PPN Rp 1.100.000 | Totalnya Rp 11.100.000 | Hanya contoh; mohon dibetulkan supaya tidak disalin ke uji |

---

## 6. Keadaan gerbang cutover

| Gerbang | Isi | Keadaan 28 September 2026 |
|---|---|---|
| G1 | Kotak masuk Accounting dibangun dan diuji | Dibangun dan diuji pengembang; UAT belum |
| G2 | Bagan akun sah + aturan posting setiap kode aktif | Belum — termasuk akun debit `PPN-MASUKAN-PEMBELIAN` |
| G3 | Akun layanan aktif, mekanismenya diputuskan | Syarat organisasi disepakati (`FIN-DEC-036`); mekanisme token masih terbuka bersama Platform |
| G4 | Pengirim Finance siap | Belum — termasuk pelurusan bagian 5 |
| G5 | Saldo awal manual siap | Bergantung G2 |
| G6 | Deposit, kelebihan bayar, selisih kas, uang muka, **dan kini refund kategori lain** | Sebagian besar kode disepakati; sisa pada bagian 7 |

---

## 7. Yang Accounting butuhkan dari Finance

| # | Butuh | Menahan apa | Dasar |
|---:|---|---|---|
| 7.1 | Kejadian apa yang dikirim bila tender uang muka **dibatalkan sesudah uang mukanya dipakai** melunasi piutang. **Contoh:** uang muka Rp 20.000.000 dipakai melunasi tagihan Rp 32.000.000, lalu tender Rp 20.000.000 dibatalkan. Bila hanya `PEMBALIKAN-PENERIMAAN-UANG-MUKA` yang terkirim, Uang Muka Pasien bersaldo minus Rp 20.000.000 dan piutang tetap tercatat lunas — padahal `FIN-DEC-021` mengembalikannya ke `OUTSTANDING`. Hasil akhir yang benar adalah debit Piutang, kredit Kas | G6 | `ACC-DEC-098` |
| 7.2 | Konfirmasi tiga syarat `PENGAKUAN-KELEBIHAN-BAYAR` (bagian 3.2) | G6 | `ACC-DEC-100` |
| 7.3 | Apakah kredit retur supplier memuat PPN, dan kode apa untuk porsi PPN-nya (bagian 3.6); juga apakah supplier pernah mencairkan kredit retur secara **tunai** — bila ya, perlu kode sendiri (debit Kas, kredit Piutang Retur Supplier) | Aktivasi worker kode retur | `ACC-DEC-105`, `106` |
| 7.4 | Kode penanda shift tertutup, terbit saat `REVIEWED` (bagian 4.1) | Penegakan `ACC-DEC-065` | `ACC-DEC-101` |
| 7.5 | Nama final dua kode selisih kas (bagian 3.3) dan empat kode potongan piutang (bagian 3.5) | G6; aktivasi worker potongan | `ACC-DEC-099`, `104` |
| 7.6 | Sifat akuntansi refund `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN`, lalu usulan kodenya atau pernyataan bahwa tidak ada kas keluar (bagian 4.2) | G6 | `ACC-DEC-102` |
| 7.7 | Empat pelurusan bagian 5 | G4 | — |

---

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/accounting/00-interview-decisions.md` bagian *Keputusan pasca-Wave B dan jawaban Finance* dan *Keputusan atas usulan kode Finance `evidence/06` dan `07`* | `ACC-DEC-094` sampai `ACC-DEC-106` beserta alasan dan contohnya |
| `docs/module-blueprints/accounting/contracts/cross-module-contract.md` (`ACC-XMOD-0.3`) | Bentuk pesan, `Components`, pintu masuk, perlakuan setiap keadaan. Katalog bagian 3a akan diperbarui mengikuti surat ini |
| `docs/module-blueprints/accounting/evidence/13-balasan-accounting-untuk-finance.md` | Surat kami sebelumnya, yang dijawab `evidence/04` dan `05` |
