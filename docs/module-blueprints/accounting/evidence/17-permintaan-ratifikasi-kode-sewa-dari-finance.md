# Permintaan ratifikasi Accounting — Kode kejadian akuntansi pendapatan sewa (`FIN-OQ-044(b)`)

| | |
|---|---|
| Kepada | Rizki (Accounting) |
| Dari | Yasmin (Product Owner Finance) |
| Tanggal | 1 Oktober 2026 |
| Modul | Finance Management — `EPIC FIN-19`, Piutang Sewa Non-Pasien (Parkir dan Tenant) |
| Yang diminta | Keputusan Accounting atas kode kejadian akuntansi, atau jawaban bahwa kejadian akuntansi tidak diperlukan |
| Mengikat atau tidak | Tidak menahan rilis. Menahan **kelengkapan akuntansi** saja |

## 1. Konteks singkat

Finance kini bisa mencatat tagihan sewa parkir dan sewa unit tenant, melunasinya, mendendanya, dan
menghapusnya bila tidak tertagih. Semuanya dicatat manual oleh petugas AR, per periode, tanpa master
kontrak sewa.

Sekarang, pencatatan itu **tidak menerbitkan kejadian akuntansi apa pun** ke Accounting. Alasannya:
kotak keluar Accounting hanya menerima kode kejadian yang sudah Anda ratifikasi, dan sewa belum
punya kode.

Keputusan Finance pada 1 Oktober 2026 yang relevan bagi Anda:

- Pelunasan sewa **tidak** masuk kas harian maupun setoran bank. Piutang sewa dikelola terpisah
  (`FIN-DEC-109`).
- Rilis pertama boleh berjalan dengan banner peringatan di layar, sampai pertanyaan ini dijawab
  (`FIN-DEC-110`).

## 2. Yang dicatat sistem pada setiap kejadian

| Kejadian di Finance | Nilai yang tercatat | Kejadian akuntansi saat ini |
|---|---|---|
| Tagihan sewa dicatat (satu periode, kategori Parkir atau Tenant) | Nominal tagihan, penyewa, objek sewa, jatuh tempo | Tidak ada |
| Denda keterlambatan ditambahkan | Nominal yang diketik petugas, bukan hasil hitung sistem | Tidak ada |
| Pelunasan sebagian atau penuh | Nominal, tanggal | Tidak ada |
| Pelunasan dibetulkan | Baris pelunasan bernilai minus | Tidak ada |
| Piutang dihapus (tidak tertagih) | Sisa nominal, alasan wajib | Tidak ada |
| Tagihan dibatalkan (hanya bila belum menerima pembayaran) | Nominal tagihan | Tidak ada |

## 3. Yang kami butuhkan dari Accounting

Kami tidak menentukan kode atau akun. Mohon putuskan, atau tolak bila tidak perlu:

| # | Pertanyaan | Jawaban Accounting |
|---|---|---|
| 1 | Apakah enam kejadian di atas perlu menerbitkan kejadian akuntansi ke Accounting? Bila hanya sebagian, yang mana? | |
| 2 | Kode kejadian apa untuk tiap kejadian yang diperlukan? (Pola yang sudah ada: `PPN-MASUKAN-PEMBELIAN`, `FIN-DEC-053`) | |
| 3 | Kapan pendapatan sewa diakui: saat tagihan dicatat, atau saat uang diterima? | |
| 4 | Apakah sewa parkir dan sewa tenant memakai kode atau akun pendapatan yang sama, atau terpisah? | |
| 5 | Apakah sewa dikenakan PPN? Bila ya, berapa dan bagaimana dicatat? | |
| 6 | Bagaimana denda keterlambatan dibukukan: pendapatan sewa, atau pendapatan lain? | |
| 7 | Bagaimana penghapusan piutang sewa dibukukan? | |
| 8 | Karena uang sewa tidak lewat kas harian dan setoran bank, bagaimana Accounting mencocokkannya dengan rekening koran? | |

## 4. Yang perlu diketahui sebelum menjawab

- Kejadian akuntansi hanya bisa diterbitkan setelah kodenya Anda ratifikasi. Finance tidak menetapkan
  kode sepihak.
- Pelunasan sewa **tidak** melewati kas harian. Bila Accounting memerlukan pelunasan itu tampil di
  kas, itu perubahan atas `FIN-DEC-109` dan harus dikembalikan ke Finance.
- Tidak ada master kontrak sewa. Tiap periode adalah baris tersendiri, sehingga sistem tidak dapat
  menerbitkan tagihan atau pendapatan otomatis per periode.
- Pemilik sistem tidak akan membuat task integrasi ke kotak keluar Accounting sebelum jawaban ini
  turun.

## 5. Cara menjawab

Isi kolom jawaban di bagian 3 dan kirim kembali ke Yasmin. Jawaban Anda dicatat sebagai keputusan
baru pada `00-interview-decisions.md` dan menutup `FIN-OQ-044(b)`.
