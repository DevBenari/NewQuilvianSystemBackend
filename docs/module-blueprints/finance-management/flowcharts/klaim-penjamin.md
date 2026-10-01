# Alur Proses — Klaim Penjamin atas Tagihan Gabungan

Status: `draft`, 1 Oktober 2026. Diturunkan dari `FIN-DEC-095`, `FIN-DEC-097`, `FIN-DEC-098`;
dirancang `FIN-DES-070`, `FIN-DES-071`.

Nama keadaan pada diagram ini sama persis dengan `contracts/state-transition-matrix.md` bagian
`D.1` (sumbu klaim) dan `B.7` (sumbu dokumen dan pelunasan).

> **Catatan struktur.** Blueprint ini dibangun sebelum folder `flowcharts/` menjadi bagian kontrak
> keluaran, sehingga diagram lama tinggal di `erd/`. Berkas ini mengikuti kontrak yang berlaku
> sekarang. Pemindahan diagram lama adalah pekerjaan tersendiri, bukan efek samping amendment ini.

## 1. Alur pokok — dari tagihan terbit sampai klaim ditutup

```mermaid
flowchart TD
    subgraph ar[Petugas AR]
        A([Tagihan gabungan sudah disusun]) --> B[Terbitkan tagihan ke penjamin]
    end
    subgraph sistem[Sistem]
        B --> C[(Klaim Diajukan)]
    end
    subgraph penjamin[Penjamin, di luar sistem]
        C --> D[Penjamin memeriksa berkas]
    end
    subgraph ar2[Petugas AR]
        D --> E{Berkas dinyatakan lengkap?}
        E -- Belum --> E1[/Berkas dikembalikan, klaim belum bergerak/]
        E1 --> D
        E -- Sudah --> F[(Klaim Diverifikasi Penjamin)]
        F --> G[Catat nominal yang disetujui penjamin]
        G --> H{Disetujui penuh?}
        H -- Ya --> I[(Klaim Disetujui, tanpa selisih)]
        H -- Tidak --> J[(Klaim Disetujui, ada selisih)]
        J --> K[Tindak lanjuti selisih lewat penghapusan piutang]
        I --> L[Tutup klaim]
        K --> L
        L --> M[(Klaim Ditutup)]
    end
    M --> N([Selesai])
```

**Yang sengaja tidak digambar di sini:** masuknya uang dari penjamin. Pelunasan berjalan pada sumbu
yang berbeda dan tidak menunggu klaim ditutup — lihat bagian 3.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Terbitkan tagihan ke penjamin | Petugas AR | Tagihan gabungan berisi minimal satu piutang | Tagihan terbit; klaim berstatus Diajukan | Tagihan kosong ditolak; petugas melengkapi anggotanya lebih dulu |
| Tandai berkas diterima penjamin | Petugas AR | Konfirmasi dari penjamin | Klaim berstatus Diverifikasi Penjamin | Bila tagihannya ternyata belum terbit, petugas menerbitkannya lebih dulu |
| Catat nominal yang disetujui | Petugas AR | Surat atau berita acara persetujuan penjamin, beserta alasan bila nominalnya lebih kecil | Klaim berstatus Disetujui beserta nominal dan selisihnya | Nominal melebihi tagihan atau alasan kosong ditolak; petugas membetulkan isiannya |
| Tindak lanjuti selisih | Petugas AR, lalu penyetuju | Piutang anggota yang nilainya tidak disetujui penjamin | Pengajuan penghapusan piutang, menunggu penyetuju | Selisih tetap terbaca sebagai pekerjaan yang belum selesai |
| Tutup klaim | Petugas AR | Tidak ada tindak lanjut lagi dari penjamin | Klaim berstatus Ditutup | Klaim yang sudah ditutup tidak dapat dibuka lagi |

## 2. Jalur pengecualian — penjamin menyetujui lebih kecil dari tagihan

```mermaid
flowchart TD
    subgraph ar[Petugas AR]
        A([Penjamin menyetujui lebih kecil]) --> B[Catat nominal dan alasannya]
        B --> C{Alasan diisi?}
        C -- Belum --> C1[/Ditolak, alasan wajib diisi/]
        C1 --> B
        C -- Sudah --> D[(Klaim Disetujui, ada selisih)]
        D --> E[Buka piutang yang tidak disetujui]
        E --> F[Ajukan penghapusan sebesar selisih]
    end
    subgraph penyetuju[Penyetuju Finance]
        F --> G{Penghapusan disetujui?}
        G -- Tidak --> G1[/Ditolak, piutang tetap utuh/]
        G1 --> E
        G -- Ya --> H[(Sisa piutang berkurang)]
    end
    H --> I([Selisih selesai ditindaklanjuti])
```

**Inti jalur ini:** persetujuan penjamin **tidak** menghapus piutang. Yang menghapus adalah
keputusan penyetuju Finance pada jalur penghapusan yang sudah berjalan sejak awal. Menyatakan apa
kata penjamin dan menghapus piutang dari buku adalah dua keputusan berbeda, dengan pemutus berbeda.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Catat nominal dan alasan | Petugas AR | Nominal persetujuan, alasan selisih | Klaim Disetujui beserta selisihnya | Alasan kosong ditolak; petugas melengkapinya |
| Ajukan penghapusan | Petugas AR | Piutang anggota yang terdampak | Pengajuan penghapusan menunggu penyetuju | Pengajuan tidak terbentuk; selisih tetap tercatat menunggu |
| Putuskan penghapusan | Penyetuju Finance | Pengajuan beserta alasannya | Sisa piutang berkurang, atau pengajuan ditolak | Bila ditolak, piutang tetap utuh dan selisih tetap terbaca sebagai pekerjaan |

## 3. Dua sumbu berjalan sendiri-sendiri

```mermaid
flowchart TD
    subgraph klaim[Sumbu jawaban penjamin, diisi petugas]
        A1[(Klaim Diajukan)] --> A2[(Klaim Diverifikasi Penjamin)]
        A2 --> A3[(Klaim Disetujui)]
        A3 --> A4[(Klaim Ditutup)]
    end
    subgraph bayar[Sumbu pelunasan, diisi sistem dari uang yang masuk]
        B1[(Tagihan Terbit)] --> B2[(Dibayar Sebagian)]
        B2 --> B3[(Lunas)]
    end
    A1 -.->|keduanya bermula dari penerbitan yang sama| B1
```

Keduanya **tidak** saling menunggu. Keadaan "penjamin sudah menyetujui tetapi uang belum masuk
sama sekali" dan "uang sudah masuk sebagian walau klaim belum resmi disetujui" keduanya sah, dan
keduanya sering ditemui petugas.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Alokasikan uang yang masuk | Petugas AR | Penerimaan dari penjamin | Sisa piutang anggota berkurang; sumbu pelunasan bergerak sendiri | Alokasi ditolak bila melebihi sisa; petugas membetulkan nominalnya |
