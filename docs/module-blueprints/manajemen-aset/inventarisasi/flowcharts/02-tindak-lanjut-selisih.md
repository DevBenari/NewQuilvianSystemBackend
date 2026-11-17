# Tindak Lanjut Selisih

**Tujuan:** setiap selisih yang sudah diverifikasi diselesaikan lewat alur pemiliknya, tanpa mengubah data
aset diam-diam (`AST-DEC-019`). **Pemicu:** sesi `Diverifikasi`. **Hasil akhir:** setiap usulan `Diproses`
(dokumen tujuan dibuat) atau `Diabaikan` dengan alasan.

```mermaid
flowchart TD
    subgraph sistem[Sistem]
        A([Sesi diverifikasi]) --> B[Buat usulan per selisih]
        B --> C[(Usulan)]
    end
    subgraph petugas[Petugas Aset]
        C --> D{Jenis usulan}
        D -- Beda lokasi, unit lain --> E[Buat mutasi koreksi opname]
        D -- Beda ruangan, unit sama --> F[Buat penyerahan koreksi opname]
        D -- Tidak ditemukan --> G[Buka laporan kondisi Hilang]
        D -- Beda kondisi --> H[Buka laporan kondisi]
        D -- Tidak terdaftar --> I[Buat draft aset dari temuan]
        E --> J{Aset sedang dalam proses lain?}
        F --> J
        G --> J
        H --> J
        I --> K[(Diproses)]
        J -- Ya --> J1[/Belum dapat diproses/]
        J1 --> C
        J -- Tidak --> K
    end
    subgraph kepala[Kepala Bagian Aset]
        C --> L{Selisih tidak perlu ditindaklanjuti?}
        L -- Ya --> M[Abaikan dengan alasan]
        M --> N[(Diabaikan)]
    end
    K --> O([Kemajuan dibaca dari dokumen tujuan])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Proses beda unit | Petugas Aset | Usulan | Mutasi `Diajukan` beralasan koreksi opname | Aset terkunci → selesaikan proses itu, lalu proses ulang |
| Proses beda ruangan | Petugas Aset | Usulan | Penyerahan `Diserahkan` | sama |
| Proses tidak ditemukan | Petugas Aset | Usulan | Laporan kondisi Hilang; tetap butuh berita acara kehilangan sebelum disetujui | — |
| Proses temuan | Petugas Aset | Usulan + foto | Draft aset | — |
| Abaikan | Kepala Bagian Aset | Alasan | `Diabaikan` | — |

**Contoh.** Dua kursi tidak ditemukan → dua laporan kondisi Hilang. Satu kursi ternyata ditemukan di gudang
sebelum disetujui; Petugas Aset menolak laporan itu dan aset tetap `Aktif`. Kursi kedua disetujui Hilang
setelah berita acara kehilangan dilampirkan.
