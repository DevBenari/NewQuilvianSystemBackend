# Proses — Simulasi Penggajian

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1`, status `approved` (Yasmin, 2026-11-12) |
| `last_changed_in` | `PGJ-C1` |
| Traceability | `EPIC PGJ-05`; PRL-DEC-32 |

Simulasi tidak punya status tersimpan, sehingga diagram tidak memuat node keadaan.

```mermaid
flowchart TD
    subgraph petugas[Petugas Payroll]
        A([Ingin melihat hasil sebelum run resmi]) --> B[Pilih periode dan pegawai]
        B --> C{Jumlah pegawai melebihi batas?}
        C -- Ya --> C1[/Ditolak, kurangi jumlah pegawai/]
        C1 --> B
        C -- Tidak --> D[Jalankan simulasi]
    end
    subgraph sistem[Sistem]
        D --> E{Ada pegawai dengan penetapan gaji berlaku?}
        E -- Tidak --> E1[/Ditolak, tidak ada pegawai layak dihitung/]
        E -- Ya --> F[Hitung dengan aturan yang sama dengan run resmi]
        F --> G[Tampilkan hasil, peringatan, dan galat per pegawai]
        G --> H[Buang seluruh data percobaan]
    end
    E1 --> B
    H --> I{Hasil sudah sesuai?}
    I -- Belum --> J[Perbaiki data sumber: struktur, transport, input variabel]
    J --> B
    I -- Sudah --> K([Lanjut membuat atau menghitung run resmi])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih periode dan pegawai | Petugas Payroll | Periode; daftar pegawai opsional | Permintaan simulasi | Petugas mengurangi jumlah pegawai |
| Jalankan simulasi | Sistem | Data aktual periode | Hasil per pegawai; tidak ada data tersimpan | Tidak ada pegawai layak: petugas memeriksa penetapan gaji |
| Tinjau hasil | Petugas Payroll | Hasil simulasi | Keputusan memperbaiki data atau lanjut | — |
