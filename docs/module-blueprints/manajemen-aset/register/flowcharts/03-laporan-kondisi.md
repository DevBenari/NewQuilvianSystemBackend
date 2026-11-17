# Laporan dan Penetapan Kondisi Aset

**Tujuan:** kondisi aset di sistem sesuai kenyataan dan dapat dipertanggungjawabkan.
**Pemicu:** unit melihat kerusakan, Bagian Aset memeriksa, pinjaman kembali rusak, atau tindak lanjut opname.
**Prasyarat:** aset `Terdaftar`; tidak ada laporan lain yang masih terbuka.
**Hasil akhir:** kondisi baru berlaku dan tercatat di linimasa.

```mermaid
flowchart TD
    subgraph unit[Penanggung jawab atau Kepala Unit]
        A([Kondisi aset berubah]) --> B[Laporkan kondisi dan foto]
        B --> C{Sudah ada laporan terbuka?}
        C -- Ya --> C1[/Ditolak, tunggu laporan sebelumnya diputuskan/]
        C -- Tidak --> D[(Dilaporkan)]
    end
    subgraph petugas[Petugas Aset]
        D --> E{Laporan Hilang?}
        E -- Tidak --> F{Sesuai hasil pemeriksaan?}
        F -- Tidak --> F1[Tolak dengan alasan]
        F1 --> G[(Ditolak)]
        F -- Ya --> H[Tetapkan kondisi]
        H --> I[(Ditetapkan)]
    end
    subgraph kepala[Kepala Bagian Aset]
        E -- Ya --> J{Berita acara kehilangan terlampir?}
        J -- Tidak --> J1[/Ditolak, lampirkan berita acara/]
        J1 --> J
        J -- Ya --> K{Pelapor adalah saya?}
        K -- Ya --> K1[/Ditolak, tidak boleh menyetujui sendiri/]
        K -- Tidak --> L[Setujui Hilang]
        L --> I
    end
    I --> M([Kondisi baru berlaku; dashboard dan daftar kondisi diperbarui])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Laporkan | Penanggung jawab, Kepala Unit, Petugas Aset, atau sistem | Kondisi usulan, catatan, foto (wajib bila dari unit) | Laporan `Dilaporkan`; kondisi aset **belum** berubah | Menunggu laporan sebelumnya |
| Tetapkan | Petugas Aset | Hasil pemeriksaan | Kondisi aset berubah | — |
| Setujui Hilang | Kepala Bagian Aset (bukan pelapor) | Berita acara kehilangan | Kondisi `Hilang` | Melampirkan berita acara |
| Ditemukan kembali | Pelapor mana pun | Laporan bertanda "ditemukan" | Kondisi kembali sesuai pemeriksaan | — |

**Contoh.** Kepala Ruang IGD melaporkan monitor `AST-ALKMED-000123` "Rusak Ringan" dengan foto layar
retak pada 3 Juni. Sampai Petugas Aset menetapkannya pada 4 Juni, monitor tetap tercatat `Aktif`.
