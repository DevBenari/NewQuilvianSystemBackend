# Alur proses — Perjanjian cicilan piutang karyawan

| Field | Nilai |
|---|---|
| Slice | `S2a` (perjanjian dan jadwalnya), `S2b` (penumpukan tunggakan) |
| Keputusan yang diturunkan | `FIN-DEC-165` (boleh lunas sekali bayar, boleh dicicil dengan perjanjian), `FIN-DEC-166` (pengaju tidak menyetujui sendiri), `FIN-DEC-168` (tunggakan tetap terhutang dan ikut periode berikutnya sampai lunas), `FIN-DEC-179` (HR menegakkan batas potongan) |
| Rancangan | `FIN-DES-099`, `FIN-DES-100`, `02-backend-architecture.md` bagian O |
| Status | `approved` — disetujui pemilik (Yasmin, 5 Oktober 2026) |
| Catatan | Diagram ini **tidak** memuat nama tabel, kolom, endpoint, maupun class. Pesan penolakan persisnya ada di `contracts/validation-matrix.md` bagian `O.1` dan `O.2`, tidak disalin ke sini |

## Alur normal beserta jalur gagalnya

```mermaid
flowchart TD
    subgraph Staf["Staf Finance"]
        A1[Membuka kartu piutang pegawai]
        A2[Mengajukan perjanjian: total, jumlah angsuran, periode gaji pertama]
        A3[Memperbaiki angka lalu mengajukan ulang]
        A4[Menarik pengajuannya]
        A5[Meminta pejabat lain memeriksa]
    end

    subgraph Pejabat["Pejabat berwenang Finance"]
        P1[Memeriksa pengajuan]
        P2[Menyetujui]
        P3[Menolak beserta alasannya]
    end

    subgraph Sistem["Sistem Finance"]
        B1{Piutang berjenis manfaat karyawan?}
        B2{Piutang masih terbuka?}
        B3{Sudah ada perjanjian berjalan?}
        B4{Angka dan periode masuk akal?}
        B5[Menyimpan pengajuan, menunggu persetujuan]
        B6{Penyetuju berbeda dari pengaju?}
        B7{Sisa piutang masih sama dengan total disepakati?}
        B8[Membangkitkan seluruh baris jadwal]
        B9[Perjanjian berjalan]
        C1[Menolak: bukan piutang manfaat karyawan]
        C2[Menolak: piutang sudah tidak terbuka]
        C3[Menolak: sudah ada perjanjian berjalan]
        C4[Menolak: angka atau periode tidak sah]
        C5[Menolak: tidak boleh menyetujui pengajuan sendiri]
        C6[Menolak: sisa piutang sudah berubah]
    end

    A1 --> A2 --> B1
    B1 -- Tidak --> C1
    B1 -- Ya --> B2
    B2 -- Tidak --> C2
    B2 -- Ya --> B3
    B3 -- Sudah --> C3
    B3 -- Belum --> B4
    B4 -- Tidak --> C4 --> A3 --> B1
    B4 -- Ya --> B5 --> P1
    P1 --> P3
    P1 --> P2 --> B6
    B6 -- Sama --> C5 --> A5 --> P1
    B6 -- Berbeda --> B7
    B7 -- Berubah --> C6 --> A3
    B7 -- Sama --> B8 --> B9
    B5 -. "berubah pikiran sebelum diputuskan" .-> A4
```

## Alur setelah perjanjian berjalan: potongan dan tunggakan

```mermaid
flowchart TD
    subgraph Finance["Sistem Finance"]
        D1[Mengirim jadwal potongan periode ini]
        D4{Hasil potongan yang diterima?}
        D5[Mencatat angsuran terbayar penuh]
        D6[Mencatat terbayar sebagian, sisanya tetap terhutang]
        D7[Menandai angsuran tertunggak]
        D8[Menambahkan sisa dan tunggakan ke jadwal periode berikutnya]
        D9{Seluruh angsuran sudah terbayar?}
        D10[Perjanjian selesai, piutang lunas]
    end

    subgraph HR["HR / Payroll"]
        E1[Menegakkan batas potongan terhadap gaji]
        E2[Memotong sebesar yang mampu ditanggung gaji]
        E3[Mengirim hasil potongan apa adanya]
    end

    D1 --> E1 --> E2 --> E3 --> D4
    D4 -- Penuh --> D5 --> D9
    D4 -- Sebagian --> D6 --> D8
    D4 -- Gagal --> D7 --> D8
    D8 --> D1
    D9 -- Belum --> D1
    D9 -- Sudah --> D10
```

## Tabel langkah — pengajuan sampai disetujui

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 1 | Membuka kartu piutang pegawai | Staf Finance | Nomor piutang atau pencarian | Kartu piutang beserta sisanya | — |
| 2 | Memeriksa kelayakan piutang | Sistem | Jenis debitur dan status piutang | Lanjut, atau penolakan | Piutang penjamin atau piutang sewa **tidak** memakai jalur ini. Piutang yang sudah lunas atau dibatalkan juga tidak |
| 3 | Memeriksa perjanjian yang sudah ada | Sistem | Perjanjian pada kartu piutang itu | Lanjut, atau penolakan | Staf membatalkan dulu perjanjian yang berjalan. Dua perjanjian berjalan atas satu piutang berarti satu utang dijadwalkan dua kali |
| 4 | Mengisi syarat perjanjian | Staf Finance | Total, jumlah angsuran, periode gaji pertama, catatan, berkas perjanjian bila ada | Pengajuan menunggu persetujuan | Angka yang tidak cocok atau periode yang sudah lewat ditolak beserta sebabnya |
| 5 | Memeriksa pengajuan | Pejabat berwenang | Pengajuan beserta kartu piutangnya | Persetujuan atau penolakan beserta alasan | — |
| 6 | Memeriksa pengaju dan penyetuju berbeda | Sistem | Identitas keduanya | Lanjut, atau penolakan | Staf meminta pejabat lain. Ini **bukan** masalah hak akses yang dapat diperbaiki administrator — satu orang memang tidak boleh melakukan keduanya |
| 7 | Memeriksa sisa piutang masih sama | Sistem | Sisa piutang saat ini dan total yang disepakati | Lanjut, atau penolakan | Staf mengajukan ulang dengan angka yang benar. Keadaan ini wajar: pegawai mungkin menyetor tunai setelah pengajuan dibuat |
| 8 | Membangkitkan jadwal | Sistem | Syarat perjanjian yang disetujui | Seluruh baris jadwal sekaligus | Sebelum langkah ini **belum ada** jadwal. Jadwal lahir dari persetujuan, bukan dari pengajuan |

## Tabel langkah — potongan dan tunggakan

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 9 | Mengirim jadwal periode ini | Finance | Angsuran yang jatuh pada periode itu beserta sisa dan tunggakannya | Jadwal potongan | — |
| 10 | Menegakkan batas potongan | HR | Jadwal, gaji, dan aturan batas milik HR | Nominal yang benar-benar dipotong | **Finance tidak memeriksa batas ini.** Bila HR memotong lebih kecil daripada jadwal, Finance menerimanya sebagai potongan sebagian |
| 11 | Mengirim hasil potongan | HR | Hasil per pegawai | Hasil penuh, sebagian, atau gagal | Kiriman ganda **tidak** mengurangi piutang dua kali |
| 12 | Mencatat hasil | Finance | Hasil potongan | Angsuran terbayar, terbayar sebagian, atau tertunggak | — |
| 13 | Menumpuk sisa ke periode berikutnya | Finance | Sisa dan tunggakan | Jadwal periode berikutnya yang membawa tunggakan | **Tunggakan tidak hangus dan tidak dihapus otomatis.** Ia ikut sampai lunas |
| 14 | Menutup perjanjian | Finance | Seluruh angsuran terbayar | Perjanjian selesai, piutang lunas | Bila pegawai berhenti kerja sebelum lunas, sisa piutangnya **tidak** dihapus otomatis; penghapusan hanya lewat pengajuan dan persetujuan |

## Jalur gagal yang paling sering ditemui petugas

| Keadaan | Yang dilihat petugas | Yang **MUST NOT** terjadi |
|---|---|---|
| Staf yang sama mencoba menyetujui pengajuannya | Keterangan bahwa pengajuan sendiri tidak dapat disetujui, beserta arahan meminta pejabat lain | Tombol setujui tampil lalu permintaannya gagal dengan galat teknis |
| Pegawai menyetor tunai setelah pengajuan dibuat | Keterangan bahwa sisa piutang sudah berubah, beserta angka barunya | Perjanjian disetujui dengan total lama, sehingga gaji dipotong lebih besar daripada utangnya |
| Gaji tidak cukup menanggung angsuran bulan ini | Angsuran tercatat terbayar sebagian, dan sisanya terlihat ikut periode berikutnya | Angsuran dianggap lunas, atau tunggakannya hilang dari jadwal |
| Potongan gagal sama sekali satu periode | Angsuran tertunggak, dan jumlah periode tertunggak terlihat di rincian perjanjian | Perjanjian otomatis dibatalkan karena satu kali gagal |
| Dua staf mengajukan perjanjian atas piutang yang sama | Yang kedua ditolak beserta keterangan ada perjanjian berjalan | Dua perjanjian berjalan atas satu utang |
| Pegawai berhenti kerja dengan sisa cicilan | Sisa piutang tetap terbuka, dan status bebas tanggungannya **tidak** terbit | Sisa dihapus otomatis, sehingga penghapusan buku terjadi tanpa persetujuan siapa pun |
