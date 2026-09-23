# Proses: Resep Harian, Rekonsiliasi Obat, Template, dan Sliding Scale

| Field | Nilai |
| --- | --- |
| Sub-modul | `dokter-rawat-inap` |
| Revision | `0.3` — berkas baru, blueprint revision `7` |
| Status | `draft` — belum disetujui manusia |
| Isi | Seluruh percabangan beserta jalur pengecualiannya |
| Kemampuan | `CAP-023-RSP` |
| Keputusan | `RWI-DEC-121`, `RWI-DEC-122`, `RWI-DEC-132` s.d. `RWI-DEC-135`, `RWI-DEC-145` s.d. `RWI-DEC-147` |
| Kontrak | Status sebaris dengan `contracts/state-transition-matrix.md` bagian 8.4 s.d. 8.7 |

Pemberian obat, pencatatan GDS, dan pelaksanaan sliding scale oleh perawat ada pada
`../../keperawatan/flowcharts/03-obat-mar-dan-sliding-scale.md`.

---

## 1. Rekonsiliasi obat saat admisi

```mermaid
flowchart TD
    subgraph perawat[Perawat]
        A([Pasien masuk membawa obat dari rumah]) --> B[Mencari obat di master obat]
        B --> C{Obat ditemukan?}
        C -- Tidak --> C1[Meminta pendaftaran obat non-formularium]
        C -- Ya --> D[Mencatat dosis, bentuk, frekuensi, rute]
        D --> E[(Obat bawaan Pending)]
    end
    subgraph pendaftar[Pengguna berhak mendaftarkan obat]
        C1 --> P1[Mendaftarkan nama dan kategori]
        P1 --> B
    end
    subgraph dokter[Dokter]
        E --> F[Membuka Resep lalu Rekonsiliasi]
        F --> G{Keputusan untuk obat ini}
        G -- Lanjut Sama atau Lanjut Ubah --> H[Butir masuk draft resep]
        G -- Hentikan --> I[(Keputusan Stopped)]
        H --> J{Resep diselesaikan?}
        J -- Belum --> J1[Butir tetap draft, belum ada dosis pemberian]
        J -- Sudah --> K[(Resep aktif dan dosis terjadwal terbentuk)]
        J1 --> L{Ingin mengganti keputusan?}
        L -- Ya --> G
        L -- Tidak --> J
    end
    I --> Z([Apoteker melihat keputusan saat menyiapkan obat])
    K --> Z
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Mencari obat | Perawat | Nama obat | Obat terpilih dari master | Tidak ditemukan → perawat meminta pengguna berhak mendaftarkannya; di luar jam kerja pencatatan menunggu, dan itu syarat operasional yang sudah diterima pemilik |
| Mendaftarkan obat non-formularium | Pengguna berhak mendaftarkan obat | Nama dan kategori | Obat baru bertanda non-formularium | Nama atau kategori kosong → dilengkapi |
| Mencatat obat bawaan | Perawat | Dosis, bentuk, frekuensi, rute | Obat bawaan `Pending` | Perawat mengira dapat mengetik nama bebas → tidak ada isiannya; kembali mencari di master |
| Memutuskan | Dokter | Obat bawaan | Keputusan tercatat; butir draft untuk "Lanjut" | Perawat mencoba memutuskan → ditolak; dokter yang memutuskan |
| Mengganti keputusan | Dokter | Butir hasil keputusan masih draft | Keputusan baru menunjuk yang lama | Resep sudah aktif → terapi diubah lewat resep atau penghentian butir |

## 2. Resep Harian dan penghentian butir

```mermaid
flowchart TD
    subgraph dokter[Dokter]
        A([Membuka Resep Harian]) --> B[Memilih periode]
        B --> C[Memilih butir yang akan dihentikan]
        C --> D{Dokter bertugas atas pasien?}
        D -- Tidak --> D1[/Ditolak: tidak bertugas/]
        D -- Ya --> E{Alasan diisi?}
        E -- Tidak --> E1[/Ditolak: alasan wajib/]
        E1 --> E
        E -- Ya --> F{Butir sudah dihentikan?}
        F -- Ya --> F1[/Ditolak: sudah dihentikan/]
        F -- Tidak --> G[(Butir dihentikan)]
    end
    subgraph sistem[Sistem]
        G --> H[Dosis Due sesudahnya dibatalkan]
        H --> I{Butir insulin berprotokol sliding scale?}
        I -- Ya --> J[(Protokol Stopped)]
        I -- Tidak --> K[Selesai]
        J --> K
    end
    K --> Z([Perawat melihat butir dihentikan tanpa tombol hentikan])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Memilih periode | Dokter | Hari ini, minggu ini, bulan ini, atau rentang | Daftar resep termasuk racikan dan obat pulang | Daftar gagal → Coba Lagi |
| Menghentikan butir | Dokter bertugas | Alasan | Butir dihentikan; dosis `Due` dibatalkan dalam satu simpanan | Tidak bertugas → dokter yang bertugas yang menghentikan |
| Akibat pada protokol | Sistem | Butir insulin berdosis skala | Protokol `Stopped` | — |

## 3. Template resep

```mermaid
flowchart TD
    subgraph dokter[Dokter]
        A([Membuka Template Resep]) --> B[Melihat template miliknya saja]
        B --> C[Memilih template untuk dipakai]
        C --> D{Template milik dokter ini?}
        D -- Tidak --> D1[/Ditolak dari ruang kerja rawat inap/]
        D -- Ya --> E[Butir masuk draft resep]
        E --> F{Ada butir bentrok alergi atau tidak tersedia?}
        F -- Ya --> G[Butir bertanda]
        G --> H[Menghapus atau mengganti butir bertanda]
        H --> F
        F -- Tidak --> I[(Draft resep siap disimpan)]
    end
    I --> Z([Dokter meninjau lalu menyelesaikan resep])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Melihat template | Dokter | Akun tertaut dokter | Template milik sendiri | Akun tanpa tautan dokter → ditolak; administrator menautkan akun |
| Memakai template | Dokter | Draft resep pasien rawat inap | Butir draft dengan penanda | Template dokter lain → tidak dapat dipakai dari rawat inap |
| Menyimpan draft | Dokter | Butir tanpa penanda | Draft tersimpan | Masih ada penanda → dokter menghapus atau mengganti butir |

## 4. Protokol sliding scale — pengesahan dan pemesanan

```mermaid
flowchart TD
    subgraph pengubah[Pengubah konfigurasi]
        A([Menyiapkan protokol]) --> B[Menulis rentang dan dosis]
        B --> C{Rentang menutup seluruh nilai tanpa tumpuk?}
        C -- Tidak --> C1[/Ditolak: rentang tidak sah/]
        C1 --> B
        C -- Ya --> D[(Versi Draft)]
    end
    subgraph pengesah[Pengesah konfigurasi]
        D --> E{Pengesah bukan pengubah terakhir?}
        E -- Tidak --> E1[/Ditolak: pengesah harus orang lain/]
        E -- Ya --> F[(Versi Approved, versi lama Retired)]
    end
    subgraph dokter[Dokter]
        F --> G[Menambah insulin berdosis skala pada draft resep]
        G --> H[Memilih versi protokol sah]
        H --> I{Menyesuaikan rentang atau dosis?}
        I -- Ya --> J{Alasan diisi?}
        J -- Tidak --> J1[/Ditolak: alasan wajib/]
        J1 --> J
        J -- Ya --> K[(Protokol pasien Active)]
        I -- Tidak --> K
    end
    K --> Z([Perawat melaksanakan dari protokol pasien])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Menulis rentang | Pengubah konfigurasi | Satuan gula darah, rentang, dosis | Versi `Draft` | Rentang bertumpuk atau berlubang → baris bermasalah ditunjukkan; pengubah memperbaiki |
| Mengesahkan | Pengesah | Versi `Draft` | Versi `Approved` | Pengesah sama dengan pengubah terakhir → pengguna lain yang mengesahkan |
| Memesan | Dokter | Butir insulin draft, versi sah | Protokol pasien `Active` versi 1 | Belum ada versi sah → sliding scale tidak dapat dipesan; menunggu pengesahan |
| Menyesuaikan | Dokter | Alasan | Versi protokol pasien baru | Dokter lain lebih dulu menyesuaikan → muat ulang lalu periksa |
| Pemakaian untuk pasien sungguhan | — | Isi protokol disahkan pemilik klinis | — | **Gerbang produksi**: belum ada pemilik klinis yang ditunjuk |
