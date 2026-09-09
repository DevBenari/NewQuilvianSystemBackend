# Proses: Pemeriksaan Golongan Darah dan Penyelesaian Konflik — dengan jalur pengecualian

```mermaid
flowchart TD
    subgraph bdrs[Petugas Bank Darah]
        A([Pasien perlu golongan darah sah]) --> B[Ambil sampel]
        B --> C[(SampleTaken)]
        C --> D[Catat hasil ABO dan Rhesus]
        D --> E[(ResultRecorded)]
    end
    subgraph val[Validator]
        E --> F[Validasi hasil]
        F --> G{Berbeda dari hasil sah sebelumnya?}
        G -- Tidak --> H[(Validated, hasil sah berlaku)]
        G -- Ya --> I[/Ditahan, pasien tak punya golongan darah sah/]
    end
    subgraph konflik[Penyelesaian konflik - di layar pemeriksaan]
        I --> J[Ambil sampel ulang]
        J --> K[Catat hasil baru]
        K --> L[Validasi hasil ulang]
        L --> M{Validator menyatakan hasil ulang yang berlaku?}
        M -- Tidak --> I
        M -- Ya --> N[(Validated, satu hasil sah kembali berlaku)]
    end
    H --> O([Golongan darah sah dapat dipakai alur Bank Darah])
    N --> O
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Ambil sampel | Petugas pengambil | Pasien | `SampleTaken` | — |
| Catat hasil | Pemeriksa | ABO & Rhesus | `ResultRecorded` | Ditolak bila pemeriksa/waktu kosong |
| Validasi | Validator | Hasil tercatat | `Validated` sah, atau konflik ditahan | Ditolak bila bukan validator |
| Deteksi konflik | Sistem | Hasil sah sebelumnya | Ditahan bila berbeda | Selama ditahan, gerbang klinis tertutup |
| Ambil & catat pemeriksaan ulang | Petugas + validator | Sampel baru | Hasil ulang tervalidasi | — |
| Selesaikan konflik | Validator | Pemeriksaan ulang tervalidasi | Satu hasil sah berlaku | Ditolak bila tanpa pemeriksaan ulang, atau bila sistem diminta memilih mayoritas |

Sistem tidak pernah menentukan hasil mana yang benar; ia menahan dan memanggil validator. Hasil ulang
yang berbeda dari kedua hasil lama tetap boleh menjadi sah bila validator menyatakannya. Tidak ada
daftar kerja keempat — penyelesaian hidup di layar pemeriksaan golongan darah.
