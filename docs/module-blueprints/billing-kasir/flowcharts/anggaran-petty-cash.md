# Billing dan Kasir — Alur Anggaran Kas Kecil (Petty Cash)

> Revisi `1.0`, status **draft**. Berkas ini menggambarkan **satu proses beserta seluruh percabangan dan jalur pengecualiannya**: bagaimana anggaran kas kecil diisi, dikoreksi, dan berkurang.
>
> Nama status pada diagram sama persis dengan [`../contracts/state-transition-matrix.md`](../contracts/state-transition-matrix.md). Diagram sengaja **tidak** memuat nama tabel, kolom, endpoint, maupun nama kelas. Kalimat penolakan yang sebenarnya ada di [`../contracts/validation-matrix.md`](../contracts/validation-matrix.md).
>
> Perjalanan satu voucher ada di [`voucher-petty-cash.md`](voucher-petty-cash.md).

## Yang digambarkan alur ini

Angka besar bertuliskan **TOTAL PETTY CASH** di layar adalah saldo yang berjalan: uang yang dimasukkan Finance, dikurangi uang yang sudah benar-benar diserahkan kepada penerima. Alur ini menggambarkan dari mana angka itu datang dan kapan ia bergerak.

**Pemicu:** Finance mengisi kas kecil, mengoreksi saldo, atau kasir menyerahkan uang untuk sebuah voucher.

**Prasyarat:** kolam anggaran kas kecil sudah ada. Rumah sakit memakai **satu** kolam untuk seluruh unit pada rilis pertama.

**Hasil akhir:** saldo yang tampil di layar selalu dapat dijelaskan baris per baris dari riwayat pergerakannya.

```mermaid
flowchart TD
    subgraph Finance["Finance"]
        A([Uang kas kecil perlu diisi]) --> B[Masukkan nominal dan alasan]
        B --> C{Nominal dan alasan terisi?}
        C -- Belum --> C1[/Ditolak, nominal dan alasan wajib diisi/]
        C1 --> B
        C -- Sudah --> D[Saldo bertambah]
        D --> E[(Riwayat mencatat satu penambahan)]

        F([Uang di laci ternyata berbeda dari catatan]) --> G[Masukkan koreksi beserta alasannya]
        G --> H{Hasil koreksi masih menutupi yang sudah dijanjikan?}
        H -- Tidak --> H1[/Ditolak, saldo akan kurang dari yang sudah disetujui/]
        H1 --> G
        H -- Ya --> I[Saldo dikoreksi]
        I --> E
    end

    subgraph Persetujuan["Kepala Kasir"]
        J([Sebuah voucher hendak disetujui]) --> K{Nominalnya muat di sisa yang benar-benar bebas?}
        K -- Tidak --> K1[/Ditolak, sisa anggaran tidak cukup/]
        K1 --> L([Menunggu anggaran ditambah lebih dulu])
        L --> A
        K -- Ya --> M[(Disetujui — nominalnya dijanjikan, saldo belum berkurang)]
    end

    subgraph Kasir["Kasir"]
        M --> N[Serahkan uang kepada penerima]
        N --> O{Saldo saat ini masih cukup?}
        O -- Tidak --> O1[/Ditolak, saldo sudah berubah/]
        O1 --> L
        O -- Ya --> P[Saldo berkurang]
        P --> Q[(Uang Diterima — riwayat mencatat satu pengeluaran)]
        Q --> R([Angka TOTAL PETTY CASH di layar ikut turun])
    end
```

## Tabel langkah

| No | Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal, petugas melakukan |
| ---: | --- | --- | --- | --- | --- |
| 1 | Isi anggaran kas kecil | Finance | Nominal yang dimasukkan ke laci dan alasannya | Saldo bertambah; satu baris riwayat penambahan tercatat | Melengkapi nominal atau alasan yang kosong. Alasan wajib karena baris inilah yang dibaca auditor |
| 2 | Koreksi saldo | Finance | Nominal koreksi, arah menambah atau mengurangi, dan alasannya | Saldo dikoreksi; satu baris riwayat koreksi tercatat | Bila koreksi ditolak karena akan membuat saldo kurang dari yang sudah dijanjikan, memeriksa voucher mana yang sudah disetujui tetapi belum dicairkan, lalu menyelesaikannya lebih dulu |
| 3 | Periksa sisa saat menyetujui | Kepala Kasir | Voucher yang menunggu, saldo, dan nominal yang sudah dijanjikan voucher lain | Keputusan menyetujui atau menolak | Bila sisa tidak cukup, meminta Finance mengisi anggaran lebih dulu. Voucher tetap menunggu dan tidak perlu dibuat ulang |
| 4 | Serahkan uang | Kasir | Voucher yang sudah disetujui, uang tunai di laci | Saldo berkurang; satu baris riwayat pengeluaran tercatat | Bila ditolak karena saldo sudah berubah, menghubungi Finance. Uang **tidak** boleh diserahkan lebih dulu lalu dicatat belakangan |
| 5 | Baca riwayat pergerakan | Finance | Rentang tanggal atau jenis pergerakan | Daftar penambahan, koreksi, dan pengeluaran beserta saldo sebelum dan sesudah tiap barisnya | Bila sebuah pergerakan tidak dapat dijelaskan, menelusuri voucher yang menyebabkannya lewat nomor voucher pada baris itu |

## Dua angka yang berbeda, dan bedanya penting

Layar menampilkan **saldo**, sedangkan yang dipakai memeriksa persetujuan adalah **sisa yang benar-benar bebas**. Keduanya berbeda ketika ada voucher yang sudah disetujui tetapi uangnya belum diserahkan.

| Angka | Artinya | Kapan berubah |
| --- | --- | --- |
| **Saldo** — angka besar TOTAL PETTY CASH | Uang yang secara catatan masih ada di laci | Bertambah saat Finance mengisi; berkurang saat kasir **menyerahkan** uang |
| **Sudah dijanjikan** | Jumlah nominal voucher yang sudah disetujui tetapi belum diserahkan | Bertambah saat Kepala Kasir menyetujui; berkurang saat uangnya diserahkan |
| **Sisa yang benar-benar bebas** | Saldo dikurangi yang sudah dijanjikan | Ikut berubah mengikuti keduanya |

> **Contoh berangka.** Saldo Rp 5.000.000. Satu voucher Rp 300.000 sudah disetujui pagi ini, tetapi penerimanya belum datang mengambil uangnya.
>
> | Angka | Nilai |
> | --- | ---: |
> | Saldo — yang tampil di kartu | Rp 5.000.000 |
> | Sudah dijanjikan | Rp 300.000 |
> | Sisa yang benar-benar bebas | Rp 4.700.000 |
>
> Uangnya memang masih Rp 5.000.000 di laci, jadi itulah yang ditampilkan. Tetapi Rp 300.000 di antaranya sudah punya pemilik, sehingga persetujuan berikutnya hanya boleh sampai Rp 4.700.000.
>
> **Kalau perbedaan ini tidak dijaga:** voucher kedua senilai Rp 4.800.000 akan lolos karena dibandingkan dengan Rp 5.000.000. Kedua penerima lalu datang, keduanya berhak, dan uang di laci kurang Rp 100.000.

## Jalur pengecualian yang paling sering ditemui

| Keadaan | Yang dilihat petugas | Yang dilakukan berikutnya |
| --- | --- | --- |
| Persetujuan ditolak karena sisa tidak cukup | Penolakan yang menyebut sisa yang benar-benar dapat dipakai | Finance mengisi anggaran, lalu Kepala Kasir menyetujui ulang voucher yang sama |
| Penyerahan uang ditolak karena saldo sudah berubah | Penolakan saat menekan Uang Diberikan | Menghubungi Finance untuk memeriksa koreksi yang baru terjadi. Voucher tetap menunggu diserahkan |
| Koreksi ditolak karena akan mengingkari janji | Penolakan yang menyebut nominal yang sudah dijanjikan | Menyelesaikan voucher yang sudah disetujui lebih dulu — diserahkan uangnya atau dibiarkan tidak dicairkan — baru mengoreksi saldo |
| Uang di laci lebih sedikit daripada catatan | Tidak ada peringatan otomatis; sistem tidak menghitung uang fisik | Finance memasukkan koreksi beralasan. Selisihnya tercatat sebagai baris riwayat, bukan dihapus |
| Permintaan pengisian anggaran terkirim dua kali | Layar menampilkan hasil yang sama seperti pengiriman pertama | Tidak ada tindakan. Saldo bertambah satu kali |
| Anggaran ingin dipisah per unit atau departemen | Tidak tersedia pada rilis pertama | Seluruh unit memakai satu kolam bersama. Pemisahan sudah dicatat sebagai kandidat rilis berikutnya |

## Yang tidak digambarkan di sini

Perjalanan satu voucher dari pengajuan sampai bukti nota ada di [`voucher-petty-cash.md`](voucher-petty-cash.md).

Alur ini **tidak** bersinggungan dengan penutupan shift kasir. Uang kas kecil dan uang kas shift adalah dua kantong yang berbeda: mengisi kas kecil tidak menambah kas shift, dan menyerahkan uang kas kecil tidak mengurangi kas shift maupun memunculkan selisih saat shift ditutup.

Sistem juga **tidak** menghitung uang fisik kas kecil seperti pada penutupan shift. Kecocokan antara saldo di layar dan uang di laci dijaga Finance secara manual, dan selisihnya diselesaikan lewat koreksi beralasan.
