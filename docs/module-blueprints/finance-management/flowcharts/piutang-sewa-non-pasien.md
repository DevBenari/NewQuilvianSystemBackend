# Alur Proses — Piutang Sewa Non-Pasien (Parkir dan Tenant)

Status: `draft`, 1 Oktober 2026. Diturunkan dari `FIN-DEC-099`..`FIN-DEC-104`;
dirancang `FIN-DES-074`..`FIN-DES-077`.

Nama keadaan pada diagram ini sama persis dengan `contracts/state-transition-matrix.md` bagian `E.1`.

## 1. Alur pokok — dari tagihan dicatat sampai lunas

```mermaid
flowchart TD
    subgraph ar[Petugas AR]
        A([Periode sewa berjalan]) --> B[Catat tagihan sewa periode ini]
        B --> C[(Belum Dibayar)]
        C --> D{Penyewa membayar?}
        D -- Belum, lewat jatuh tempo --> E[Catat denda keterlambatan]
        E --> C
        D -- Sebagian --> F[Catat pelunasan sebagian]
        F --> G[(Dibayar Sebagian)]
        G --> D
        D -- Penuh --> H[Catat pelunasan penuh]
        H --> I[(Lunas)]
    end
    I --> J([Selesai])
```

Setiap periode adalah tagihan baru yang dicatat sendiri — tidak ada kontrak yang menerbitkannya
otomatis. Risiko periode terlewat ditanggung ketelitian petugas, dan itu keputusan sadar pemilik.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Catat tagihan sewa | Petugas AR | Jenis sewa, nama penyewa, objek sewa, periode, jatuh tempo, nominal | Tagihan berstatus Belum Dibayar | Nominal nol atau tanggal tidak masuk akal ditolak; petugas membetulkan isiannya |
| Catat denda keterlambatan | Petugas AR | Nominal denda menurut kebijakan yang berlaku | Sisa tagihan bertambah sebesar denda | Nominal minus ditolak |
| Catat pelunasan | Petugas AR | Tanggal terima, nominal, cara bayar | Sisa tagihan berkurang | Pelunasan melebihi sisa ditolak; petugas memeriksa kembali nominalnya |

## 2. Jalur pengecualian — tagihan tidak tertagih, atau salah dicatat

```mermaid
flowchart TD
    subgraph ar[Petugas AR]
        A([Tagihan bermasalah]) --> B{Sudah pernah menerima uang?}
        B -- Belum, dan tagihannya memang salah --> C[Batalkan tagihan]
        C --> C1{Alasan diisi?}
        C1 -- Belum --> C2[/Ditolak, alasan wajib diisi/]
        C2 --> C
        C1 -- Sudah --> D[(Dibatalkan)]
        B -- Belum, tetapi penyewa tidak akan membayar --> E[Hapus piutang]
        E --> E1{Alasan diisi?}
        E1 -- Belum --> E2[/Ditolak, alasan wajib diisi/]
        E2 --> E
        E1 -- Sudah --> F[(Dihapus)]
        B -- Sudah --> G[/Tidak boleh dibatalkan/]
        G --> H[Catat pelunasan bernilai minus untuk membetulkan]
        H --> I[(Dibayar Sebagian)]
    end
```

**Yang membedakan kapabilitas ini dari piutang pasien:** penghapusan di sini **selesai seketika**,
tanpa pengajuan dan tanpa penyetuju. Satu-satunya penahan yang tersisa adalah alasan yang wajib
diisi dan jejak audit. Ini keputusan sadar pemilik, dan **tidak berlaku** untuk piutang pasien.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Batalkan tagihan | Petugas AR | Alasan pembatalan | Tagihan berstatus Dibatalkan | Ditolak bila sudah pernah menerima uang; petugas memakai pelunasan minus |
| Hapus piutang | Petugas AR | Alasan penghapusan | Tagihan berstatus Dihapus | Ditolak bila alasan kosong |
| Betulkan pelunasan keliru | Petugas AR | Nominal minus sebesar kekeliruannya | Sisa tagihan kembali bertambah | Ditolak bila pembatalan melebihi pembayaran yang pernah tercatat |

## 3. Batas yang MUST diketahui petugas

```mermaid
flowchart TD
    subgraph ar[Petugas AR]
        A[Catat pelunasan sewa] --> B[(Sisa tagihan berkurang)]
    end
    subgraph belum[Belum tersambung pada rilis pertama]
        B -.->|tidak mengalir| C[Kas harian]
        B -.->|tidak mengalir| D[Setoran bank]
        B -.->|tidak mengalir| E[Antrean kejadian akuntansi]
    end
```

Uang sewa yang dicatat di sini **mengurangi sisa tagihan**, tetapi **belum** tercatat sebagai kas
masuk di mana pun. Ini bukan kekeliruan implementasi — ini batas yang dipilih saat memisahkan jalur
piutang sewa dari jalur penerimaan Billing, dan menjadi isi pertanyaan terbuka yang belum dijawab.

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Rekonsiliasi uang sewa ke kas | **Belum dirancang** | — | — | Selama belum diputuskan, pencocokan dilakukan di luar sistem, dan layar **MUST** menyatakannya terang kepada petugas |
