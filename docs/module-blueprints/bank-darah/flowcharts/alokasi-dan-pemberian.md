# Proses: Alokasi, Bukti Kecocokan, dan Pemberian — dengan jalur pengecualian

```mermaid
flowchart TD
    subgraph bdrs[Petugas Bank Darah]
        A([Kantong Available, order aktif]) --> B[Alokasikan ke baris kebutuhan]
        B --> C{Kantong sudah punya alokasi aktif lain?}
        C -- Ya --> C1[/Ditolak, kantong sudah dialokasikan/]
        C -- Tidak --> D[(Kantong Allocated)]
        D --> E{Salah pilih kantong?}
        E -- Ya --> F[Batalkan alokasi dengan alasan]
        F --> G{Order asal masih aktif?}
        G -- Ya --> A
        G -- Tidak --> H[(Kantong PendingReview)]
        E -- Tidak --> I[Catat bukti kecocokan untuk pasien tujuan]
    end
    subgraph gerbang[Gerbang pemberian]
        I --> J{Ada bukti untuk pasien ini dan belum lewat masa berlaku?}
        J -- Tidak --> J1[/Ditolak, butuh bukti kecocokan baru/]
        J1 --> I
        J -- Ya --> K[Berikan kantong]
        K --> L[(Kantong Issued)]
    end
    subgraph darurat[Jalur darurat - peran berwenang]
        D --> M{Butuh segera sebelum uji cocok?}
        M -- Ya --> N[Otorisasi darurat, alasan wajib]
        N --> O[(Kantong Issued, ditandai tanpa bukti)]
        O --> P[Muncul di daftar tunggakan bukti]
    end
    L --> Q{Pencatatan pemberian keliru?}
    Q -- Ya --> R[Buat catatan koreksi, pemberian asal tetap ada]
    R --> L
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Alokasikan kantong | Petugas | Kantong `Available`, order aktif | Kantong `Allocated` | Ditolak bila kantong sudah punya alokasi aktif; muat ulang |
| Batalkan alokasi | Petugas | Kantong belum diberikan, alasan | Kantong kembali `Available` atau `PendingReview` | Ditolak bila kantong sudah `Issued`; pakai catatan koreksi |
| Catat bukti kecocokan | Petugas berwenang | Kantong dialokasikan, pasien tujuan | Bukti tercatat | — |
| Periksa gerbang pemberian | Sistem | Bukti untuk pasien tujuan, belum lewat masa berlaku | Lolos | Ditolak; catat bukti baru |
| Berikan kantong | Petugas | Gerbang lolos | Kantong `Issued` | — |
| Jalur darurat | Peran berwenang | Alasan wajib | Kantong `Issued` ditandai tanpa bukti | Ditolak bila bukan peran berwenang atau alasan kosong |
| Catat koreksi | Peran berwenang | Pemberian yang ada, alasan | Koreksi melekat, pemenuhan dihitung ulang | Ditolak bila dipakai memindah pemberian ke pasien lain |

Pemberian tidak pernah dihapus atau dibalik. Pengalihan kantong hanya sah lewat jalur `Reallocated`
pada kantong yang **belum** diberikan — lihat `penyelesaian-kantong.md`.
