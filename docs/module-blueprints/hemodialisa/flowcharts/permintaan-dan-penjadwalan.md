# Hemodialisa — Permintaan dan Penjadwalan

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — status `draft` |
| Owner | Muhammad Hamzah |

Proses ini mencakup sejak permintaan cuci darah masuk sampai sesi punya tanggal, mesin, station,
perawat, dan dokter penanggung jawab.

Berbeda dari `00-alur-utama.md`, diagram di sini memuat **seluruh percabangan dan jalur
pengecualiannya**.

---

## 1. Menerima permintaan HD

```mermaid
flowchart TD
    subgraph peminta[Dokter atau perawat unit peminta]
        A([Pasien perlu cuci darah]) --> B[Isi alasan klinis dan tingkat kesegeraan]
        B --> C{Kunjungan pasien sah?}
        C -- Tidak --> C1[/Ditolak, konteks kunjungan tidak ditemukan/]
        C1 --> C2[Minta petugas pendaftaran melengkapi kunjungan]
        C2 --> B
        C -- Ya --> D[(Permintaan Requested)]
    end

    subgraph koordinator[Koordinator unit HD]
        D --> E{Unit dapat menerima pasien ini?}
        E -- Ya --> F[(Permintaan Accepted)]
        E -- Belum, alasan operasional --> G[Isi alasan penahanan]
        G --> H[(Permintaan OnHold)]
        H --> I{Hambatan sudah teratasi?}
        I -- Ya --> J[Lepas tahanan]
        J --> D
        I -- Perlu keputusan klinis --> K[Teruskan ke dokter]
    end

    subgraph dokter[Dokter]
        E -- Perlu keputusan klinis --> K
        K --> L{Cuci darah memang diindikasikan?}
        L -- Ya --> F
        L -- Tidak --> M[Isi alasan klinis penolakan]
        M --> N[(Permintaan Rejected)]
        N --> N1([Selesai, unit peminta diberi tahu lewat daftar])
    end

    F --> O([Lanjut ke penjadwalan])
```

### Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi alasan klinis dan kesegeraan | Dokter atau perawat unit peminta | Pasien, kunjungan sah, alasan klinis, rutin atau cito | Permintaan `Requested` | Petugas melengkapi alasan klinis lalu mengirim ulang |
| Nilai apakah unit dapat menerima | Koordinator unit HD | Permintaan yang menunggu, kapasitas unit | Permintaan `Accepted` atau `OnHold` | Bila permintaan sudah diproses orang lain, koordinator memuat ulang daftarnya |
| Tahan permintaan | Koordinator unit HD | Alasan operasional | Permintaan `OnHold`; status sebelumnya disimpan | Alasan wajib diisi; tanpa itu penahanan tidak tersimpan |
| Lepas tahanan | Koordinator unit HD | Permintaan yang sedang ditahan | Kembali ke status sebelum ditahan | Bila permintaan tidak sedang ditahan, tidak ada yang berubah |
| Tolak permintaan | **Dokter** | Alasan klinis | Permintaan `Rejected`, bersifat akhir | Koordinator **tidak** dapat melakukan ini; menolak adalah keputusan klinis |
| Batalkan permintaan | Pembuat permintaan | Permintaan yang belum diterima unit HD | Permintaan `Cancelled` | Bila sudah diterima unit HD, pembatalan lewat koordinator |

### Yang perlu diperhatikan

**Menahan dan menolak adalah dua hal berbeda.** Menahan berarti "belum bisa sekarang, alasannya
operasional" — misalnya seluruh mesin penuh hari ini. Menolak berarti "cuci darah tidak
diindikasikan untuk pasien ini" — dan itu keputusan medis. Karena itu koordinator boleh menahan,
tetapi hanya dokter yang boleh menolak.

**Penolakan bersifat akhir.** Bila kondisi pasien berubah, dibuat permintaan baru, supaya
riwayat penolakan sebelumnya tetap terbaca.

---

## 2. Menjadwalkan sesi

```mermaid
flowchart TD
    subgraph koordinator[Koordinator unit HD]
        A([Permintaan diterima, atau pasien rutin terjadwal]) --> B{Pasien punya program HD aktif?}
        B -- Belum --> B1[Minta petugas administrasi membuka program HD]
        B1 --> B
        B -- Ya --> C{Ada instruksi cuci darah yang aktif?}
        C -- Belum --> C1[Minta dokter membuat dan mengaktifkan instruksi]
        C1 --> C
        C -- Ya --> D[(Sesi Planned)]
        D --> E[Pilih tanggal, shift, dan jam]
        E --> F{Pasien sudah terjadwal pada jam itu?}
        F -- Ya --> F1[/Ditolak, pasien bentrok/]
        F1 --> E
        F -- Tidak --> G[Pilih mesin]
    end

    subgraph sistem[Sistem]
        G --> H{Mesin siap dipakai?}
        H -- Tidak --> H1[/Ditolak, mesin sedang tidak dapat digunakan/]
        H1 --> G
        H -- Ya --> I{Pasien perlu mesin atau ruang khusus?}
        I -- Ya --> J{Mesin dan station memenuhi kebutuhan itu?}
        J -- Tidak --> J1[/Ditolak, tidak memenuhi kebutuhan khusus/]
        J1 --> G
        J -- Ya --> K{Mesin atau station sudah dipakai pada jam itu?}
        I -- Tidak --> K
        K -- Ya --> K1[/Ditolak, sumber daya bentrok/]
        K1 --> G
        K -- Tidak --> L[(Sesi Scheduled)]
    end

    subgraph penugasan[Koordinator unit HD]
        L --> M[Tetapkan perawat dan dokter penanggung jawab]
        M --> N{Kewenangan petugas dapat diperiksa?}
        N -- Belum tersedia --> N1[Catat sebagai belum dapat diverifikasi]
        N1 --> P([Sesi siap menunggu hari pelaksanaan])
        N -- Ya, dan berwenang --> N2[Catat sebagai terverifikasi]
        N2 --> P
        N -- Ya, dan tidak berwenang --> O{Penegakan kewenangan menyala?}
        O -- Ya --> O1[/Ditolak, petugas tidak berwenang/]
        O1 --> M
        O -- Tidak --> O2[Catat sebagai tidak berwenang, tampilkan peringatan]
        O2 --> P
    end
```

### Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Periksa program dan instruksi | Koordinator unit HD | Program HD aktif, instruksi cuci darah aktif | Sesi `Planned` | Koordinator meminta petugas administrasi membuka program, atau dokter mengaktifkan instruksi |
| Pilih tanggal, shift, dan jam | Koordinator unit HD | Kalender unit | Rentang waktu terpilih | Bila pasien sudah terjadwal pada jam bertumpang tindih, koordinator memilih jam lain |
| Pilih mesin dan station | Koordinator unit HD | Daftar mesin dan station beserta statusnya | Mesin dan station terpilih | Bila mesin sedang diblokir, dalam perawatan, atau tidak laik, koordinator memilih yang lain |
| Periksa kebutuhan khusus | Sistem | Keputusan isolasi pasien yang sedang berlaku | Mesin dan station yang sesuai | Bila tidak memenuhi, koordinator memilih mesin atau station khusus |
| Periksa tabrakan sumber daya | Sistem | Jadwal seluruh sesi pada rentang waktu itu | Sesi `Scheduled` | Bila bentrok, koordinator memilih sumber daya atau jam lain |
| Tetapkan perawat dan dokter penanggung jawab | Koordinator unit HD | Daftar petugas yang bertugas pada shift itu | Penugasan tersimpan beserta status kewenangannya | Bila penegakan kewenangan menyala dan petugas tidak berwenang, koordinator memilih petugas lain |
| Batalkan sesi | Koordinator unit HD | Sesi yang belum dimulai, alasan pembatalan | Sesi `Cancelled` | Sesi yang sudah berjalan **tidak** dapat dibatalkan; yang tersedia adalah menghentikannya |

### Yang perlu diperhatikan

**Tiga jenis tabrakan diperiksa sekaligus:** pasien tidak boleh punya dua sesi bertumpang
tindih, satu mesin tidak boleh dipakai dua pasien, dan satu station tidak boleh dipakai dua
pasien. Ketiganya diperiksa di dalam satu transaksi, sehingga dua koordinator yang menjadwalkan
pada saat hampir bersamaan tidak dapat menghasilkan jadwal ganda.

**Kewenangan petugas punya tiga kemungkinan, bukan dua.** Selain "berwenang" dan "tidak
berwenang", ada keadaan ketiga: **belum dapat diperiksa**, karena sumber datanya belum tersedia.
Keadaan ketiga itu dicatat apa adanya, tidak ditulis seolah-olah sudah diperiksa.

---

## 3. Kesiapan unit sebelum shift dimulai

```mermaid
flowchart TD
    subgraph koordinator[Koordinator unit HD]
        A([Sebelum shift dimulai]) --> B[Buka lembar pemeriksaan kesiapan]
        B --> C{Lembar untuk tanggal dan shift ini sudah ada?}
        C -- Ya --> C1[/Ditolak, lembar sudah ada/]
        C1 --> D[Buka lembar yang sudah ada]
        C -- Tidak --> E[(Kesiapan unit Draft)]
        D --> E
        E --> F[Periksa mesin, station, pengolahan air, obat dan bahan, serta staf]
        F --> G{Seluruh butir wajib terpenuhi?}
        G -- Tidak --> H[Isi alasan unit tidak siap]
        H --> I[(Kesiapan unit NotReady)]
        I --> I1([Sesi pada shift ini tidak dapat dinyatakan siap])
        G -- Ya --> J{Hasil pemeriksaan air masih berlaku?}
        J -- Tidak --> J1[/Ditolak, hasil pemeriksaan air kedaluwarsa/]
        J1 --> K[Perbarui hasil pemeriksaan air]
        K --> F
        J -- Ya --> L[(Kesiapan unit Ready)]
        L --> M([Shift boleh berjalan])
    end
```

### Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka lembar pemeriksaan | Koordinator unit HD | Tanggal dan shift | Lembar `Draft` | Bila lembarnya sudah ada, koordinator membuka yang sudah ada alih-alih membuat baru |
| Periksa seluruh butir | Koordinator unit HD | Kondisi mesin, station, hasil pemeriksaan air, ketersediaan obat dan bahan, jumlah staf | Hasil tiap butir tersimpan | Butir yang belum diperiksa tetap berstatus belum diperiksa; koordinator melanjutkan |
| Nyatakan unit siap | Koordinator unit HD | Seluruh butir wajib terpenuhi, hasil pemeriksaan air masih dalam masa berlaku | Kesiapan unit `Ready` | Bila hasil air kedaluwarsa, koordinator memperbaruinya lebih dulu |
| Nyatakan unit tidak siap | Koordinator unit HD | Alasan | Kesiapan unit `NotReady` | Alasan wajib diisi |

### Yang perlu diperhatikan

**Unit yang dinyatakan tidak siap di tengah shift tidak menghentikan sesi yang sudah berjalan.**
Menghentikan cuci darah yang sedang berlangsung adalah keputusan klinis yang diambil perawat dan
dokter di tempat, bukan akibat otomatis dari perubahan status administratif.

**Masa berlaku hasil pemeriksaan air adalah pengaturan, bukan angka tetap.** Nilai awalnya 30
hari, dan setiap unit boleh mengubahnya sesuai kebijakan rumah sakit.
