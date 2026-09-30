# Permintaan Perbaikan untuk Billing — Pembalikan Tender Top-Up Deposit Tidak Menulis Mutasi

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Untuk | Owner modul Billing / Kasir (`billing-kasir`, `BIL-CASH-001`) |
| Tanggal | 28 September 2026 |
| Sifat | **Laporan temuan beserta permintaan perbaikan.** Bukan permintaan kontrak baru, bukan permintaan tabel baru — yang kami minta adalah satu mutasi yang hari ini tidak pernah ditulis |
| Dasar temuan | Pemeriksaan source read-only pada `cba60cb0`, dicatat di `docs/module-blueprints/finance-management/01-existing-capability-map.md` bagian 15.4 (`FIN-CQ-04` dan sekitarnya) dan `02-backend-architecture.md` `E.1` temuan C |
| Dicatat sebagai | `FIN-OQ-034` pada decision log Finance |
| Tembusan konteks | Owner Accounting sudah kami beri tahu lewat `evidence/15` bagian 6.2, karena aturan posting untuk kasus ini menyangkut dia |

Berkas ini berdiri sendiri. Anda tidak perlu membuka dokumen blueprint Finance untuk membacanya.

---

## 1. Ringkasan satu paragraf

Saat tender yang mendanai top-up deposit pasien berubah menjadi `REVERSED` — misalnya kartu ditarik
kembali oleh penerbit, atau pembayaran dibatalkan penyedia — `BillingSettlementService` **tidak
menulis mutasi deposit apa pun**. Akibatnya saldo deposit pasien tetap mencatat uang yang sebenarnya
tidak pernah jadi diterima rumah sakit. Ini bukan persoalan pelaporan: uang itu **bisa sudah
terpakai** melunasi tagihan, dan satu-satunya jalur pembalikan yang tersedia justru menolak kasus
itu. Kami menemukannya saat menyiapkan kejadian akuntansi untuk pembatalan uang muka, dan kami
**tidak dapat memperbaikinya dari Finance** karena Finance dilarang menulis ke tabel Billing.
Bagian 5 memuat permintaan kami, bagian 6 memuat apa yang kami lakukan sementara ini.

---

## 2. Apa yang kami temukan

Kami membaca dua jalur yang dapat membalik uang muka pasien, keduanya milik Billing.

### 2.1 Jalur pertama — pembalikan top-up lewat layanan deposit

`BillingDepositService`, operasi pembalikan mutasi:

| Aturan di source | Akibatnya |
|---|---|
| Hanya mutasi bertipe **top-up** yang dapat dibalik | Mutasi **alokasi** (pemakaian deposit untuk melunasi tagihan) tidak dapat dibalik lewat jalur ini |
| Pembalikan **ditolak** bila saldo tersedia lebih kecil dari nilai top-up aslinya | Begitu dananya sudah terpakai, pembalikan tidak mungkin dilakukan |

Jalur ini **berfungsi dengan benar** untuk kasus "top-up dibalik sebelum dananya terpakai". Yang
tidak ia tangani adalah kasus "dananya sudah terpakai" — dan penolakannya bahkan masuk akal sebagai
pengaman, karena membalik top-up yang sudah terpakai akan membuat saldo deposit negatif.

### 2.2 Jalur kedua — pembatalan tender yang mendanai top-up

`BillingSettlementService`, saat status tender berubah:

| Keadaan | Yang terjadi pada mutasi deposit |
|---|---|
| Tender menjadi **`SUCCEEDED`** dan tujuan settlement adalah top-up deposit | Mutasi **top-up ditulis**, saldo deposit bertambah |
| Tender menjadi **`REVERSED`** | **Tidak ada mutasi deposit yang ditulis sama sekali** |

Inilah inti masalahnya. Rekonsiliasi alokasi yang berjalan sesudah perubahan status hanya menyentuh
alokasi ke invoice; settlement bertujuan top-up deposit memang tidak punya invoice untuk
diselaraskan. Jadi tidak ada mekanisme lain yang mengejar ketertinggalan ini.

---

## 3. Contoh berangka — kenapa ini lebih dari soal pencatatan

Pasien menitipkan uang muka Rp 20.000.000 dengan kartu debit. Tagihannya Rp 32.000.000.

| Langkah | Keadaan sesudahnya |
|---|---|
| 1. Tender kartu `SUCCEEDED`, tujuan top-up deposit | Mutasi top-up Rp 20.000.000 ditulis; saldo deposit Rp 20.000.000 |
| 2. Deposit dipakai melunasi sebagian tagihan | Mutasi alokasi Rp 20.000.000; saldo deposit Rp 0; piutang turun Rp 20.000.000 |
| 3. Penerbit kartu **menarik kembali** pembayaran; tender menjadi `REVERSED` | **Tidak ada mutasi deposit.** Saldo tetap Rp 0, dan tagihan tetap tercatat sudah dilunasi Rp 20.000.000 |

Hasil akhirnya: rumah sakit **tidak pernah menerima** Rp 20.000.000, tetapi tagihan pasien tercatat
sudah berkurang sebesar itu. Bila operator lalu mencoba membalik top-up lewat jalur 2.1, sistem
menolaknya karena saldonya sudah nol.

**Yang ingin kami pastikan:** apakah ada mekanisme lain di Billing — di luar kedua jalur yang kami
baca — yang menangani kasus ini? Bila ada dan kami melewatkannya, mohon ditunjukkan; kami akan
memperbaiki catatan kami dan surat ini selesai.

---

## 4. Akibatnya di sisi Finance dan Accounting

| Yang seharusnya terjadi | Yang terjadi hari ini |
|---|---|
| Finance menerbitkan kejadian pembalikan penerimaan uang muka ke Accounting | **Tidak terbit** — Finance memicu kejadian itu dari mutasi deposit, dan mutasinya tidak ada |
| Finance menerbitkan kejadian pembalikan pemakaian uang muka | **Tidak terbit**, karena alasan yang sama |
| Buku besar mencatat kas berkurang dan piutang kembali terbuka | **Tidak tercatat** — buku besar tetap menganggap uang itu diterima |

Owner Accounting menanyakan skenario ini kepada kami secara spesifik, dan lawan jurnal yang benar
sudah kami sepakati bersamanya: debit Piutang, kredit Kas. Kodenya sudah kami siapkan. Yang belum
ada adalah **faktanya** — dan fakta itu lahir di Billing, bukan di Finance.

Perlu kami tegaskan: kejadian `PEMBALIKAN-PENERIMAAN-UANG-MUKA` yang **sudah diratifikasi**
Accounting pun tidak terbit pada kasus ini. Jadi ini bukan soal satu kode baru yang belum jadi;
ini lubang pada jalur yang kita anggap sudah selesai.

---

## 5. Yang kami minta

Kami **tidak** mengusulkan bentuk perbaikannya — itu wewenang Anda sebagai pemilik data deposit dan
tender. Yang kami minta adalah keputusan atas satu pertanyaan:

> **Ketika tender yang mendanai top-up deposit menjadi `REVERSED`, apa yang seharusnya terjadi pada
> saldo deposit pasien — terutama bila dananya sudah terpakai melunasi tagihan?**

Tiga kemungkinan yang kami bayangkan, semata sebagai bahan diskusi:

| Kemungkinan | Konsekuensi yang kami lihat |
|---|---|
| Billing menulis mutasi pembalik otomatis saat tender `REVERSED`, dan mengizinkan saldo menjadi negatif untuk kasus ini | Finance langsung dapat menerbitkan kedua kejadian pembalikan; saldo negatif menjadi penanda tagihan yang perlu ditagih ulang |
| Billing membalik alokasinya lebih dulu (tagihan kembali terbuka), lalu membalik top-up | Saldo deposit tidak pernah negatif; urutannya lebih bersih secara akuntansi, tetapi menyentuh status tagihan |
| Billing menandainya sebagai kasus yang memerlukan tindakan manual petugas | Perlu kejelasan siapa yang mengerjakan dan dalam batas waktu berapa, supaya tidak menggantung |

Bila pilihan Anda menuntut Finance menyesuaikan pemicu kejadiannya, kami siap menyesuaikan — beri
tahu bentuk mutasi yang akan Anda tulis, dan kami petakan ke kode kejadian yang sudah diratifikasi.

---

## 6. Yang Finance lakukan sementara ini

Kami **tidak** menulis apa pun ke tabel Billing, dan tidak akan. Yang kami pasang adalah pendeteksi
**baca-saja**:

| Hal | Perilaku |
|---|---|
| Pemeriksaan | Untuk setiap tender berstatus `REVERSED` yang tujuan settlement-nya top-up deposit, Finance memeriksa apakah ada mutasi deposit pembalik yang bersesuaian |
| Bila tidak ada | Baris sinkronisasi Finance ditandai **gagal** beserta sebabnya, dan **nol** kejadian diterbitkan ke Accounting |
| Yang dilihat petugas | Baris gagal itu muncul di layar pantauan fakta Billing milik Finance, beserta keterangan bahwa penyebabnya menunggu perbaikan di Billing |
| Yang **tidak** kami lakukan | Melewatkannya diam-diam, atau menerbitkan kejadian dengan lawan jurnal yang ditebak-tebak |

Kami memilih membuat lubangnya **berbunyi** daripada menyembunyikannya. Selama perbaikan belum
turun, kasus ini akan terlihat sebagai kegagalan sinkronisasi — itu memang keadaan yang sebenarnya,
dan kami lebih memilih itu daripada buku besar yang tampak rapi tetapi salah.

---

## 7. Yang tertahan, dan yang tidak

| Tertahan | Tidak tertahan |
|---|---|
| Kelengkapan rumpun uang muka/deposit Finance ke Accounting — satu kasus tepi tetap tidak terjurnal | Seluruh gelombang rilis Finance yang sedang berjalan. Rumpun ini sudah berada di luar gelombang karena sebab lain |
| Kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` tidak pernah punya baris untuk dikirim | Pembangunan dan pengujian sisi Finance; kodenya sudah siap menunggu fakta |

Kami menyampaikan ini sekarang, bukan menunggu sampai pengiriman ke Accounting diaktifkan, supaya
perbaikannya dapat Anda jadwalkan tanpa tekanan tenggat.

---

## 8. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/finance-management/01-existing-capability-map.md` bagian 15.4 | Temuan lengkap beserta rujukan baris source yang kami baca |
| `docs/module-blueprints/finance-management/02-backend-architecture.md` `E.1` temuan C dan `FIN-DES-057` | Pendeteksi baca-saja yang kami pasang, dan alasan Finance tidak memperbaikinya sendiri |
| `docs/module-blueprints/finance-management/evidence/15-balasan-finance-atas-ratifikasi-accounting.md` bagian 4.1 dan 6.2 | Apa yang sudah kami sampaikan ke owner Accounting tentang kasus ini |
