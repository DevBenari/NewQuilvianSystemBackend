# Pelepasan dan Penahanan Resep

Proses yang menentukan kapan sebuah resep boleh dikerjakan Farmasi, dan apa yang terjadi ketika
izin itu dicabut di tengah pekerjaan. Memuat seluruh percabangan beserta jalur pengecualiannya.

Masukan `PHA-DEC-063`–`070`. Status **draft**, 21 September 2026.

## 1. Pelepasan — resep menjadi boleh dikerjakan

```mermaid
flowchart TD
    subgraph Billing
        A1[Pernyataan resep boleh dikerjakan dikirim]
    end

    subgraph Farmasi
        B1[Terima pernyataan]
        B2{Nomor urutnya lebih baru dari yang tersimpan?}
        B3[Abaikan, salinan tetap seperti semula]
        B4[Perbarui salinan menjadi CLEARED]
        B5{Resep masih menunggu pembayaran?}
        B6[Pindahkan ke antrean apoteker]
        B7[Biarkan pada keadaan sekarang]
    end

    A1 --> B1 --> B2
    B2 -->|Tidak| B3
    B2 -->|Ya| B4 --> B5
    B5 -->|Ya| B6
    B5 -->|Tidak| B7
```

Cabang "abaikan" adalah keadaan **normal**, bukan kegagalan. Pernyataan dapat tiba tidak
berurutan, dan yang lebih tua memang harus diabaikan supaya resep yang sudah dicabut tidak
kembali terbaca boleh dikerjakan.

### Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Menerima pernyataan | Sistem | Pernyataan dari Billing | Salinan diperbarui | Dicatat sebagai kegagalan beserta jumlah percobaan; pernyataan tetap tersimpan di Billing |
| Memeriksa urutan | Sistem | Nomor urut pernyataan | Diterima atau diabaikan | — |
| Memindahkan ke antrean | Sistem | Resep yang masih menunggu | Resep tampil di antrean apoteker | Salinan tetap benar; petugas dapat memeriksa ulang |

## 2. Penahanan — izin dicabut di tengah pekerjaan

```mermaid
flowchart TD
    subgraph Billing
        A1[Pernyataan pencabutan dikirim]
    end

    subgraph Farmasi
        B1[Perbarui salinan menjadi REVOKED]
        B2{Sejauh mana pekerjaan sudah berjalan?}
        B3[Belum disentuh di antrean]
        B4[Sedang disiapkan atau sudah diracik]
        B5[Sudah siap serah]
        B6[Sudah diserahkan ke pasien]
        B7[Tahan di tempat, kemajuan dikunci]
        B8[Tidak ada yang berubah]
    end

    subgraph Petugas
        C1[Melihat alasan penahanan di layar kerja]
    end

    A1 --> B1 --> B2
    B2 --> B3 --> B7
    B2 --> B4 --> B7
    B2 --> B5 --> B7
    B2 --> B6 --> B8
    B7 --> C1
```

Tiga cabang pertama berakhir sama: **ditahan di tempat, tidak ditarik mundur**. Obat yang sudah
diracik tetap tercatat sudah diracik dan tidak dikembalikan menjadi bahan — racikan memang tidak
dapat dibatalkan secara fisik.

Cabang keempat berbeda: obat yang sudah berpindah ke tangan pasien tidak dapat ditarik. Tidak
ada yang berubah di Farmasi, dan kewajiban finansialnya menjadi urusan Billing.

### Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Memperbarui salinan | Sistem | Pernyataan pencabutan | Salinan menyatakan dicabut | Dicatat; salinan lama tetap berlaku sampai berhasil diperbarui |
| Mengunci kemajuan | Sistem | Keadaan pekerjaan saat itu | Pekerjaan berhenti di tempat | — |
| Melihat alasan | Petugas Farmasi | Layar kerja | Petugas tahu mengapa pekerjaannya terhenti | Petugas menghubungi kasir tanpa tahu sebabnya — inilah yang dicegah |

## 3. Pemulihan — izin kembali diberikan

```mermaid
flowchart TD
    subgraph Kasir
        A1[Pasien melunasi kekurangannya]
    end

    subgraph Billing
        B1[Pernyataan pemulihan dikirim]
    end

    subgraph Farmasi
        C1[Salinan kembali menyatakan boleh]
        C2[Penahanan dilepas]
        C3[Pekerjaan dilanjutkan dari titik terakhir]
    end

    A1 --> B1 --> C1 --> C2 --> C3
```

Resep **tidak** mengulang antrean, telaah, maupun penyiapan yang sudah selesai. Apoteker yang
sudah meracik tidak meracik untuk kedua kalinya.

### Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Melunasi kekurangan | Pasien di kasir | Pembayaran | Tagihan kembali lunas | Resep tetap tertahan |
| Melepas penahanan | Sistem | Pernyataan pemulihan | Pekerjaan dapat dilanjutkan | Petugas memeriksa ulang keadaan resep |

## 4. Jalur pengecualian — pernyataan tidak sampai

```mermaid
flowchart TD
    subgraph Farmasi
        A1[Proses penerimaan pernyataan gagal]
        A2[Salinan ditandai bermasalah]
        A3[Seluruh gerbang menolak melanjutkan]
        A4[Petugas melihat keadaan tidak dapat dipastikan]
        A5[Sistem memeriksa ulang ke Billing]
        A6[Salinan diperbarui sesuai jawaban Billing]
    end

    A1 --> A2 --> A3 --> A4
    A2 --> A5 --> A6 --> A3
```

Keadaan "tidak dapat dipastikan" **tidak pernah** berubah menjadi izin. `PHA-DEC-067` menutup
seluruh jalur override — tidak ada tombol yang dapat ditekan petugas, Kepala Farmasi, maupun
Supervisor.

Ini keputusan yang disadari akibatnya: ketika sambungan ke Billing bermasalah, pasien yang sudah
membayar akan menunggu. Yang dipilih adalah menunda pelayanan, bukan mengambil risiko obat
keluar tanpa dasar.

### Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Menandai bermasalah | Sistem | Kegagalan berulang | Salinan dinyatakan tidak dapat dipastikan | — |
| Memeriksa ulang | Sistem | Identitas resep | Keadaan terkini menurut Billing | Tetap tidak dapat dipastikan; pemeriksaan diulang |
| Menunggu | Petugas Farmasi | Keterangan di layar | Pekerjaan ditunda | Tidak ada jalan pintas yang tersedia |

## Yang sengaja tidak digambarkan

| Hal | Sebabnya |
| --- | --- |
| Jalur darurat klinis | Kebijakannya belum disahkan (`PHA-OQ-027`). Menggambarnya sekarang berarti menggambar sesuatu yang belum ada |
| Keputusan Billing menentukan sebab pencabutan | Itu proses milik Billing; lihat flowchart penerbitan pada blueprint `billing-kasir` |
| Penyerahan bertahap dan retur | Slice tersendiri yang requirement-nya masih terbuka |

Trace `PHA-DEC-063`, `PHA-DEC-067`, `PHA-DEC-068`, `PHA-DEC-069`.
