# Flowchart — Pemakaian alat medis besar

| Field | Nilai |
|---|---|
| Sub-modul | `keperawatan` |
| Kontrak | `0.6.0` — `draft` |
| Proses | `BP-RWF-04` |
| Keputusan | `RWI-DEC-179`, `RWI-DEC-180` |

```mermaid
flowchart TD
    subgraph perawat[Perawat]
        A([Dokter menginstruksikan alat dipasang]) --> B[Pilih jenis alat dan dokter penanggung jawab, catat waktu mulai]
    end
    subgraph sistem[Sistem]
        B --> C{Dokter berpenugasan aktif?}
        C -- Tidak --> C1[/Ditolak, pilih dokter yang menangani/]
        C1 --> B
        C -- Ya --> D[(Running)]
    end
    subgraph selesai[Perawat atau sistem]
        D --> E{Bagaimana pemakaian berakhir?}
        E -- Alat dilepas --> F[Catat waktu selesai]
        E -- Pasien keluar ruangan --> G[Ditutup otomatis pada waktu keluar, perlu diperiksa]
        E -- Salah pilih alat --> H[Batalkan dengan alasan]
    end
    subgraph tagih[Sistem dan Billing]
        F --> I[Hitung unit dari satuan dan pembulatan, kirim ke tagihan]
        G --> I
        I --> J[(Completed)]
        J --> K{Tarif untuk kelas pasien ada?}
        K -- Tidak --> K1[Baris tagihan bertanda tarif belum ada]
        K -- Ya --> K2[Baris tagihan berharga]
        H --> L{Tagihan masih terbuka?}
        L -- Tidak --> L1[/Ditolak, hubungi kasir/]
        L -- Ya --> M[(Cancelled)]
    end
    K1 --> N([Kasir melihat peringatan sebelum finalisasi])
    K2 --> O([Pemakaian tercatat dan tertagih])
    M --> O
    L1 --> N
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Mulai | Perawat | Jenis alat, dokter, waktu mulai, jumlah bila per pakai | `Running` | Dokter tidak berpenugasan: pilih ulang |
| Selesai | Perawat | Waktu selesai | `Completed`, unit dihitung server, tagihan | Waktu selesai sebelum mulai: perbaiki |
| Tutup otomatis | Sistem | Pasien keluar ruangan | `Completed` bertanda perlu diperiksa | Gagal: pemakaian tampil di Daftar Pantau |
| Batal atau koreksi | Perawat, kepala ruangan | Alasan | Hanya tagihan pemakaian itu yang berubah | Tagihan final: hubungi kasir |
