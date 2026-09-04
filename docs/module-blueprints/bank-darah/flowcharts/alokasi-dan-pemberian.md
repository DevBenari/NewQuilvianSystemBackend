# Proses: Alokasi, Bukti Kecocokan, dan Pemberian — dengan jalur pengecualian

```mermaid
flowchart TD
    subgraph bdrs[Petugas Bank Darah]
        A([Kantong Available, order aktif]) --> B[Alokasikan ke baris kebutuhan]
        B --> B1{Kantong sudah disimpan dan lokasinya masih aktif?}
        B1 -- Tidak --> B2[/Ditolak, simpan atau pindahkan kantong dulu/]
        B1 -- Ya --> C{Kantong sudah punya alokasi aktif lain?}
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
        I --> J0{Lokasi penyimpanan kantong masih aktif SEKARANG?}
        J0 -- Tidak --> J2[/Ditolak, pindahkan ke lokasi aktif dulu/]
        J0 -- Ya --> J{Ada bukti untuk pasien ini dan belum lewat masa berlaku?}
        J -- Tidak --> J1[/Ditolak, butuh bukti kecocokan baru/]
        J1 --> I
        J -- Ya --> K[Berikan kantong]
        K --> L[(Kantong Issued)]
    end
    subgraph darurat[Jalur darurat - peran berwenang]
        D --> M{Butuh segera, sebelum uji cocok atau sebelum sempat dipindahkan?}
        M -- Ya --> N[Otorisasi darurat, alasan wajib, sebutkan yang dilewati]
        N --> O[(Kantong Issued, ditandai melewati gerbang)]
        O --> P[Muncul di daftar tunggakan bukti]
    end
    L --> Q{Pencatatan pemberian keliru?}
    Q -- Ya --> R[Petugas ajukan koreksi, pemberian asal tetap ada]
    R --> R1[(Koreksi menunggu persetujuan)]
    R1 --> R2{Dokter Bank Darah memutuskan}
    R2 -- Pengaju sama dengan pemutus --> R3[/Ditolak, harus diputuskan orang lain/]
    R2 -- Setuju --> R4[(Koreksi berlaku, angka pemenuhan dihitung ulang)]
    R2 -- Tolak --> R5[(Koreksi ditolak, tetap tersimpan, rekam tidak berubah)]
    R4 --> L
    R5 --> L
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Alokasikan kantong | Petugas | Kantong `Available`, order aktif, sudah disimpan, lokasinya aktif | Kantong `Allocated` | Ditolak bila kantong sudah punya alokasi aktif, belum disimpan, atau lokasinya nonaktif — lihat `penyimpanan-kantong.md` |
| Batalkan alokasi | Petugas | Kantong belum diberikan, alasan | Kantong kembali `Available` atau `PendingReview` | Ditolak bila kantong sudah `Issued`; pakai catatan koreksi |
| Catat bukti kecocokan | Petugas berwenang | Kantong dialokasikan, pasien tujuan | Bukti tercatat | — |
| Periksa gerbang pemberian | Sistem | Tiga syarat sekaligus: sudah disimpan, lokasi terakhir masih aktif **saat itu**, bukti untuk pasien tujuan belum lewat masa berlaku | Lolos | Tertahan bukti → catat bukti baru. Tertahan lokasi → pindahkan kantong ke lokasi aktif |
| Berikan kantong | Petugas | Gerbang lolos | Kantong `Issued` | — |
| Jalur darurat | Peran berwenang | Alasan wajib + keterangan gerbang yang dilewati (bukti, lokasi nonaktif, atau keduanya) | Kantong `Issued` ditandai melewati gerbang | Ditolak bila bukan peran berwenang, alasan kosong, atau keterangan gerbang tidak diisi |
| Ajukan koreksi | Petugas BDRS | Pemberian yang ada, alasan terkendali, bukti pendukung | Koreksi tersimpan **menunggu persetujuan**; pemenuhan belum berubah | Ditolak bila dipakai memindah pemberian ke pasien lain, atau bukti pendukung kosong |
| Putuskan koreksi | Dokter BDRS | Koreksi yang menunggu | Disetujui → koreksi berlaku dan pemenuhan dihitung ulang. Ditolak → rekam tidak berubah, permintaan tetap terbaca | Ditolak bila pemutus adalah pengaju yang sama, atau koreksi sudah pernah diputuskan |

Pemberian tidak pernah dihapus atau dibalik. Pengalihan kantong hanya sah lewat jalur `Reallocated`
pada kantong yang **belum** diberikan — lihat `penyelesaian-kantong.md`.

Koreksi pencatatan **tidak berlaku saat diajukan**. Angka pemenuhan order baru bergerak setelah Dokter
Bank Darah menyetujui, dan permintaan yang ditolak tetap tersimpan supaya terbaca bahwa seseorang pernah
menyatakan catatan itu keliru.

Gerbang lokasi dinilai **dua kali**: saat alokasi dan sekali lagi saat pemberian. Sebuah kantong karena
itu dapat lolos dialokasikan lalu tertahan saat hendak diberikan, bila lokasinya dinonaktifkan di
antara keduanya. Itu perilaku yang benar, bukan ketidakkonsistenan — seluruh jalurnya ada di
`penyimpanan-kantong.md`.
