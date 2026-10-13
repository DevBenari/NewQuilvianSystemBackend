# Alur Pelunasan Gabungan Lintas Kunjungan

Masukan `BKC-DEC-118`, `BKC-AC-086`, dan keputusan arsitektur `BKC-DES-057`–`BKC-DES-062`.
Status **draft**, 13 Oktober 2026.

Dokumen ini memetakan urutan kerja petugas kasir ketika satu pasien memiliki tagihan dari
lebih dari satu kunjungan — misalnya tagihan IGD dan tagihan rawat inap — dan keluarga pasien
ingin menyelesaikannya dalam satu kali pembayaran.

## Mengapa Alur Ini Ada

Pasien yang masuk lewat IGD lalu dirawat inap tercatat sebagai **dua kunjungan terpisah** di
sistem, sehingga tagihannya pun dua. Tanpa alur ini, keluarga pasien harus bertransaksi dua kali
di loket kasir saat pemulangan.

Yang **tidak** dilakukan alur ini: menggabungkan kedua tagihan menjadi satu tagihan. Tagihan
tetap dua, karena rumah sakit wajib dapat melaporkan pendapatan unit IGD secara terpisah dari
unit rawat inap. Yang digabungkan hanya **transaksi pembayarannya**.

Alur ini juga bukan kewajiban. Keluarga pasien tetap boleh membayar tagihan IGD lebih dulu dan
melunasi tagihan rawat inap di kemudian hari, atau menggabungkan sebagian tagihan saja.

## Istilah

| Istilah | Artinya di dokumen ini |
| --- | --- |
| Tagihan jangkar | Tagihan yang pertama kali dibuka kasir di layar, menjadi titik awal pencarian tagihan lain |
| Tagihan kandidat | Tagihan lain milik pasien yang sama yang masih punya sisa dan boleh diikutkan |
| Pembayaran gabungan | Satu kali transaksi pembayaran yang menyelesaikan beberapa tagihan sekaligus |
| Urutan pelunasan | Urutan tagihan mana yang dilunasi lebih dulu bila uang tidak cukup menutup semuanya |

---

## 1. Alur Pokok — Keluarga Pasien Memilih Membayar Gabungan

```mermaid
flowchart TD
    subgraph KeluargaPasien
        A1[Keluarga pasien datang ke loket kasir saat pemulangan]
        A2[Keluarga memilih menyelesaikan seluruh tagihan sekaligus]
        A3[Keluarga menyerahkan pembayaran]
        A4[Keluarga menerima satu kwitansi berisi rincian per unit layanan]
    end

    subgraph Kasir
        B1[Kasir membuka tagihan pasien di layar]
        B2[Kasir meminta sistem menampilkan tagihan lain pasien ini]
        B3[Kasir menunjukkan daftar tagihan beserta sisa masing-masing]
        B4[Kasir menandai tagihan mana saja yang ikut dibayar]
        B5[Kasir memasukkan rincian pembayaran]
        B6[Kasir mencetak kwitansi gabungan]
    end

    subgraph SistemBilling
        C1[Sistem mencari tagihan lain pasien lewat rantai rujukan kunjungan]
        C2[Sistem menghitung sisa setiap tagihan saat ini]
        C3[Sistem mencatat pilihan keluarga pasien beserta urutan pelunasan]
        C4[Sistem menerima pembayaran dan mencatatnya sebagai satu transaksi]
        C5[Sistem melunasi tagihan terbit paling awal lebih dulu sampai habis]
        C6[Sistem meneruskan sisa uang ke tagihan berikutnya]
        C7[Sistem memperbarui status setiap tagihan secara terpisah]
    end

    A1 --> B1 --> B2 --> C1 --> C2 --> B3
    B3 --> A2 --> B4 --> C3
    C3 --> B5 --> A3 --> C4
    C4 --> C5 --> C6 --> C7
    C7 --> B6 --> A4
```

### Tabel Langkah Alur Pokok

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal / bermasalah |
| --- | --- | --- | --- | --- |
| 1. Buka tagihan pasien | Kasir | Identitas pasien atau nomor kunjungan | Tagihan jangkar tampil di layar | Kasir mencari ulang memakai nomor rekam medis bila nomor kunjungan tidak ditemukan |
| 2. Tampilkan tagihan lain | Kasir | Tagihan jangkar | Daftar tagihan kandidat beserta sisa masing-masing | Bila daftar kosong, kasir melanjutkan sebagai pembayaran satu tagihan biasa |
| 3. Tawarkan pilihan ke keluarga | Kasir | Daftar tagihan kandidat | Keputusan keluarga pasien: gabung semua, gabung sebagian, atau pisah | Keluarga berhak menolak penggabungan; kasir melanjutkan jalur pembayaran satu tagihan |
| 4. Tandai tagihan yang ikut | Kasir | Pilihan keluarga pasien | Daftar peserta pembayaran tercatat beserta urutan pelunasannya | Tagihan yang sedang diproses kasir lain ditolak ikut; kasir menunggu atau mengeluarkannya dari daftar |
| 5. Masukkan pembayaran | Kasir | Uang tunai, kartu, atau pemindahan saldo deposit | Rincian pembayaran tercatat | Metode pembayaran yang gagal diulang atau diganti tanpa membatalkan pilihan peserta |
| 6. Lunasi berurutan | Sistem Billing | Pembayaran yang berhasil | Setiap tagihan berkurang sisanya sesuai urutan | Bila uang kurang, tagihan terakhir dalam urutan tetap bersisa dan itu keadaan sah |
| 7. Perbarui status tagihan | Sistem Billing | Hasil pelunasan | Status tiap tagihan diperbarui sendiri-sendiri | Tagihan yang belum lunas tetap terbuka dan muncul lagi di pencarian berikutnya |
| 8. Cetak kwitansi | Kasir | Hasil transaksi | Satu kwitansi memuat rincian beban per unit layanan | Kwitansi dapat dicetak ulang dari riwayat pembayaran tanpa membuat transaksi baru |

### Contoh Berangka

Pasien Ibu **Sari Mandasari** (nama samaran) memiliki dua tagihan:

| Tagihan | Unit layanan | Tanggal terbit | Sisa tagihan pasien |
| --- | --- | --- | --- |
| Tagihan pertama | IGD | 9 Oktober 2026 | Rp 1.500.000 |
| Tagihan kedua | Rawat Inap | 9 Oktober 2026 (sore) | Rp 8.000.000 |

Total sisa Rp 9.500.000. Keluarga membayar **Rp 6.000.000** dan memilih menggabungkan keduanya.

Karena urutan pelunasan adalah tagihan terbit paling awal lebih dulu:

1. Tagihan IGD dilunasi lebih dulu: Rp 1.500.000 terpakai, tagihan IGD **lunas**, sisa uang
   Rp 6.000.000 − Rp 1.500.000 = Rp 4.500.000.
2. Sisa Rp 4.500.000 dialokasikan ke tagihan rawat inap: Rp 8.000.000 − Rp 4.500.000 =
   **Rp 3.500.000 masih bersisa**.

Hasil akhir: satu kwitansi senilai Rp 6.000.000 yang merinci Rp 1.500.000 untuk unit IGD dan
Rp 4.500.000 untuk unit rawat inap. Tagihan IGD berstatus lunas, tagihan rawat inap masih
terbuka dengan sisa Rp 3.500.000.

---

## 2. Jalur Pengecualian 1 — Keluarga Pasien Memilih Membayar Terpisah

Alur ini terjadi ketika keluarga pasien ingin membayar tagihan IGD lebih dulu, misalnya karena
penjamin tagihan rawat inap belum menerbitkan surat jaminan.

```mermaid
flowchart TD
    subgraph KeluargaPasien
        A1[Keluarga menyatakan hanya membayar tagihan IGD dulu]
        A2[Keluarga menerima kwitansi tagihan IGD saja]
        A3[Keluarga kembali di kemudian hari melunasi tagihan rawat inap]
    end

    subgraph Kasir
        B1[Kasir menampilkan daftar tagihan pasien]
        B2[Kasir tidak membentuk pembayaran gabungan]
        B3[Kasir memproses pembayaran satu tagihan saja]
        B4[Kasir memproses pelunasan tagihan rawat inap di kunjungan berikutnya]
    end

    subgraph SistemBilling
        C1[Sistem mencatat pembayaran untuk satu tagihan]
        C2[Sistem menandai tagihan IGD lunas]
        C3[Sistem membiarkan tagihan rawat inap tetap terbuka]
        C4[Sistem menampilkan tagihan rawat inap sebagai tagihan terbuka pada pencarian berikutnya]
    end

    B1 --> A1 --> B2 --> B3 --> C1
    C1 --> C2 --> C3 --> A2
    C3 --> C4 --> A3 --> B4
```

### Tabel Langkah Pembayaran Terpisah

| Langkah | Pelaku | Masukan | Keluaran | Catatan penting |
| --- | --- | --- | --- | --- |
| 1. Pilih bayar terpisah | Keluarga pasien | Daftar tagihan | Keputusan membayar satu tagihan saja | Ini **hak** keluarga pasien, bukan pengecualian teknis |
| 2. Proses satu tagihan | Kasir | Tagihan IGD | Pembayaran satu tagihan | Memakai jalur pembayaran yang sudah berjalan, tanpa pembentukan grup |
| 3. Biarkan tagihan lain terbuka | Sistem Billing | Hasil pembayaran | Tagihan rawat inap tetap terbuka | Tidak ada penalti, tidak ada penguncian |
| 4. Lunasi di kemudian hari | Kasir | Tagihan rawat inap | Pelunasan terpisah | Boleh digabung dengan tagihan lain yang muncul kemudian, boleh juga sendiri |

---

## 3. Jalur Pengecualian 2 — Tagihan Sedang Diproses Kasir Lain

Alur ini terjadi ketika dua petugas kasir hampir bersamaan mencoba memproses tagihan yang sama,
misalnya satu kasir di loket rawat inap dan satu kasir di loket IGD.

```mermaid
flowchart TD
    subgraph KasirPertama
        A1[Kasir pertama membentuk pembayaran gabungan memuat tagihan IGD]
        A2[Kasir pertama belum menyelesaikan pembayarannya]
    end

    subgraph KasirKedua
        B1[Kasir kedua mencoba memasukkan tagihan IGD ke pembayaran lain]
        B2[Kasir kedua menerima penolakan beserta keterangan loket yang memegangnya]
        B3[Kasir kedua mengeluarkan tagihan itu dari daftar atau menunggu]
        B4[Kasir kedua melanjutkan pembayaran tanpa tagihan tersebut]
    end

    subgraph SistemBilling
        C1[Sistem mencatat tagihan IGD sedang terikat pembayaran berjalan]
        C2[Sistem menolak pengikatan kedua atas tagihan yang sama]
        C3[Sistem melepas ikatan saat pembayaran pertama selesai atau dibatalkan]
    end

    A1 --> C1 --> A2
    B1 --> C2 --> B2 --> B3 --> B4
    A2 --> C3
    C3 -.->|tagihan bebas kembali| B1
```

### Tabel Langkah Bentrokan Antar Loket

| Langkah | Pelaku | Masukan | Keluaran | Yang dilakukan petugas bila terjadi |
| --- | --- | --- | --- | --- |
| 1. Ikat tagihan | Sistem Billing | Pembentukan pembayaran gabungan | Tagihan tercatat sedang diproses | — |
| 2. Tolak ikatan kedua | Sistem Billing | Permintaan kasir kedua | Penolakan beserta keterangan loket pemegangnya | Kasir kedua menghubungi loket yang disebut untuk memastikan |
| 3. Keluarkan atau tunggu | Kasir kedua | Penolakan | Daftar peserta disesuaikan | Kasir kedua tetap dapat melanjutkan untuk tagihan lain yang bebas |
| 4. Lepas ikatan | Sistem Billing | Pembayaran pertama selesai atau dibatalkan | Tagihan bebas kembali | Bila kasir pertama meninggalkan transaksi menggantung, supervisor kasir membatalkannya |

---

## 4. Jalur Pengecualian 3 — Pembayaran Gagal di Tengah Transaksi Gabungan

Alur ini terjadi ketika sebagian metode pembayaran berhasil dan sebagian gagal, misalnya kartu
debit ditolak bank sementara uang tunai sudah diterima.

```mermaid
flowchart TD
    subgraph Kasir
        A1[Kasir memasukkan dua metode pembayaran tunai dan kartu]
        A2[Kasir melihat kartu ditolak sementara tunai berhasil]
        A3[Kasir menawarkan metode pengganti ke keluarga pasien]
        A4[Kasir mencetak kwitansi sesuai nominal yang benar-benar diterima]
    end

    subgraph SistemBilling
        B1[Sistem mencatat setiap metode pembayaran secara terpisah]
        B2[Sistem hanya mengalokasikan nominal yang berhasil]
        B3[Sistem menahan daftar peserta tetap utuh]
        B4[Sistem menandai transaksi sebagai terbayar sebagian]
        B5[Sistem menerima metode pengganti pada transaksi yang sama]
    end

    subgraph KeluargaPasien
        C1[Keluarga menyerahkan metode pembayaran pengganti]
        C2[Keluarga menerima kwitansi yang nominalnya sesuai uang diserahkan]
    end

    A1 --> B1 --> B2 --> A2
    B2 --> B3 --> B4
    A2 --> A3 --> C1 --> B5
    B5 --> B2
    B4 --> A4 --> C2
```

### Tabel Langkah Pembayaran Gagal Sebagian

| Langkah | Pelaku | Masukan | Keluaran | Yang dilakukan petugas bila terjadi |
| --- | --- | --- | --- | --- |
| 1. Catat tiap metode | Sistem Billing | Beberapa metode pembayaran | Setiap metode punya catatan sendiri | — |
| 2. Alokasikan yang berhasil saja | Sistem Billing | Metode yang berhasil | Tagihan berkurang sesuai uang yang benar-benar diterima | Uang tunai yang sudah diterima **tidak** dikembalikan hanya karena kartu gagal |
| 3. Tahan daftar peserta | Sistem Billing | Transaksi berjalan | Pilihan keluarga pasien tidak hilang | Kasir tidak perlu memilih ulang tagihan dari awal |
| 4. Terima metode pengganti | Kasir | Metode pembayaran baru | Alokasi berlanjut pada transaksi yang sama | Bila keluarga tidak punya pengganti, transaksi ditutup sebagai terbayar sebagian |
| 5. Cetak kwitansi | Kasir | Nominal yang berhasil | Kwitansi sesuai uang diterima | Kwitansi **MUST** mencerminkan uang nyata, bukan nominal yang diminta |

---

## 5. Jalur Pengecualian 4 — Tagihan Susulan Tiba Setelah Pembayaran Gabungan

Alur ini terjadi ketika petugas farmasi atau ruangan memasukkan biaya yang terlambat dicatat,
sesudah keluarga pasien menyelesaikan pembayaran gabungan.

```mermaid
flowchart TD
    subgraph PetugasLayanan
        A1[Petugas memasukkan biaya yang terlambat dicatat]
    end

    subgraph SistemBilling
        B1[Sistem menerima biaya baru pada tagihan rawat inap]
        B2{Tagihan sudah terkunci permanen?}
        B3[Sistem menolak biaya susulan]
        B4[Sistem menambahkan biaya ke tagihan rawat inap]
        B5[Sistem menghitung ulang sisa tagihan]
        B6[Sistem mencabut kelayakan pulang bila sebelumnya sudah disetujui]
        B7[Sistem membiarkan kwitansi gabungan sebelumnya tetap sah]
    end

    subgraph Kasir
        C1[Kasir melihat tagihan rawat inap terbuka kembali]
        C2[Kasir memproses pelunasan selisih sebagai pembayaran baru]
    end

    A1 --> B1 --> B2
    B2 -->|Ya| B3
    B2 -->|Belum| B4 --> B5 --> B6
    B5 --> B7
    B6 --> C1 --> C2
```

### Tabel Langkah Tagihan Susulan

| Langkah | Pelaku | Masukan | Keluaran | Yang dilakukan petugas bila terjadi |
| --- | --- | --- | --- | --- |
| 1. Masukkan biaya susulan | Petugas farmasi atau ruangan | Layanan yang terlambat dicatat | Biaya baru masuk ke tagihan kunjungan terkait | Petugas wajib memberi catatan alasan keterlambatan |
| 2. Periksa penguncian | Sistem Billing | Status tagihan | Diterima atau ditolak | Bila ditolak, dibutuhkan pembukaan kunci oleh supervisor kasir |
| 3. Hitung ulang sisa | Sistem Billing | Biaya baru | Sisa tagihan bertambah | Kwitansi gabungan yang sudah tercetak **tetap sah** dan tidak dibatalkan |
| 4. Cabut kelayakan pulang | Sistem Billing | Sisa tagihan bertambah | Kelayakan pemulangan dicabut | Bangsal menahan kepulangan dan mengarahkan keluarga ke loket kasir |
| 5. Lunasi selisih | Kasir | Sisa tagihan baru | Pembayaran baru, terpisah dari yang sebelumnya | Boleh dibayar sendiri, boleh digabung dengan tagihan lain yang masih terbuka |

> Catatan hubungan dengan kelayakan pemulangan: apakah sisa tagihan pada kunjungan **IGD** juga
> menahan kelayakan pemulangan rawat inap masih menjadi pertanyaan terbuka `BKC-OQ-109` dan
> belum digambarkan di sini.
