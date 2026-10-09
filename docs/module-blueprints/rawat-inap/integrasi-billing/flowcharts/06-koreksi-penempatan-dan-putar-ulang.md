# Flowchart — Koreksi salah catat penempatan dan putar ulang saat rilis

| Field | Nilai |
|---|---|
| Sub-modul | `integrasi-billing` |
| Kontrak | `1.1.0` — `draft` |
| Requirement | `FR-RWF-017`, `FR-RWF-019` |
| Keputusan | `RWI-DEC-157`, `166`, `169`, `192` (g) |

## 1. Koreksi salah catat penempatan

```mermaid
flowchart TD
    subgraph karu[Kepala ruangan atau petugas admisi]
        A([Ditemukan salah catat kamar, bed, kelas, atau waktu]) --> B[Buka riwayat penempatan]
        B --> C[Pilih baris yang berlaku, isi koreksi dan alasan]
    end
    subgraph sistem[Sistem Rawat Inap]
        C --> D{Tagihan rawat inap masih terbuka?}
        D -- Tidak --> D1[/Ditolak, tagihan sudah final/]
        D1 --> D2([Kasir memakai penyesuaian Billing])
        D -- Tidak terbaca --> D3[/Ditolak, status tagihan tidak terbaca/]
        D3 --> C
        D -- Ya --> E{Bed tujuan layak dan waktu tidak bertabrakan?}
        E -- Tidak --> E1[/Ditolak, alasan kelayakan/]
        E1 --> C
        E -- Ya --> F[(Baris lama digantikan, baris koreksi berlaku)]
        F --> G[Ketuk pintu Billing]
    end
    subgraph billing[Sistem Billing]
        G --> H[Hitung ulang tarif kamar tanpa baris lama]
    end
    H --> I([Tarif kamar sesuai linimasa yang benar])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih baris | Kepala ruangan, admisi | Riwayat penempatan | Formulir koreksi | Baris yang sudah dikoreksi tidak punya tombol koreksi |
| Simpan koreksi | Sama | Koreksi dan alasan | Baris lama tetap tersimpan, baris koreksi berlaku | Tagihan final: hubungi kasir; versi berubah: muat ulang |
| Hitung ulang | Sistem Billing | Ketukan pintu | Tarif kamar baru | Ketukan gagal: dicoba ulang otomatis |

## 2. Putar ulang saat rilis

```mermaid
flowchart TD
    subgraph ti[Tim TI dengan wewenang tertulis]
        A([Rilis perbaikan tagihan]) --> B[Jalankan putar ulang mode uji coba]
        B --> C{Daftar episode sesuai harapan?}
        C -- Tidak --> C1([Hentikan dan laporkan])
        C -- Ya --> D[Jalankan putar ulang sungguhan]
    end
    subgraph sistem[Sistem Rawat Inap]
        D --> E[Antrekan ulang admisi lalu setiap bed aktif dengan kunci asli]
    end
    subgraph billing[Sistem Billing]
        E --> F[Buka invoice dan hitung tarif kamar sejak waktu masuk asli]
        F --> G{Ada biaya kamar manual?}
        G -- Ya --> H[Tandai perlu diperiksa]
        G -- Tidak --> I[Tidak ada tindak lanjut]
    end
    subgraph kasir[Kasir]
        H --> J[Batalkan biaya yang dobel lalu nyatakan selesai diperiksa]
        J --> J1{Masih ada biaya kamar dobel?}
        J1 -- Ya --> J2[/Ditolak, batalkan salah satu dulu/]
        J2 --> J
    end
    J1 -- Tidak --> K([Invoice dapat difinalkan])
    I --> K
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Uji coba | Tim TI | Alasan | Daftar episode yang akan diproses | Daftar janggal: berhenti |
| Putar ulang | Tim TI | Alasan, wewenang tertulis | Pesan diantrekan ulang | Menjalankan dua kali tidak mengubah apa pun |
| Tandai perlu diperiksa | Sistem Billing | Biaya kamar manual dan otomatis | Invoice ditahan dari finalisasi | — |
| Selesai diperiksa | Kasir | Biaya dobel sudah dibatalkan | Invoice dapat difinalkan | Masih dobel: ditolak |
