# Formasi dan Rencana Tahunan

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Jenis | Flowchart proses |
| Slice | `S-D1` perencanaan tenaga kerja |
| Status | `approved` — Pengguna, 2026-11-03, bersama desain `S-D1` |
| Kesiapan | `READY FOR DESIGN` |
| Keputusan | `HRD-DEC-062`, `HRD-DEC-064`, `HRD-DEC-065`, `HRD-DEC-068`, `HRD-DEC-069`, `HRD-DEC-070` |

---

## 1. Apa yang digambar

Dua jalan yang melahirkan formasi: **batch formasi awal** untuk rumah sakit yang belum punya formasi
di sistem, dan **rencana tahunan beserta addendumnya**. Formasi sendiri tidak pernah disunting
langsung; ia hanya lahir dari dokumen yang disetujui. Status pada node keadaan sama persis dengan
`contracts/state-transition-matrix.md` bagian 11.2 dan 11.3.

## 2. Diagram — batch formasi awal

```mermaid
flowchart TD
    subgraph admin[HR Admin]
        A([Rumah sakit punya dokumen formasi resmi]) --> B[Unduh templat, isi per unit dan jabatan]
        B --> C{Seluruh baris valid?}
        C -- Belum --> C1[/Ditolak, ada baris yang belum valid/]
        C1 --> B
        C -- Valid --> D{Scan dokumen sudah diunggah?}
        D -- Belum --> D1[/Ditolak, scan dokumen wajib/]
        D1 --> D2[Unggah scan]
        D2 --> D
        D -- Sudah --> E[Ajukan]
    end
    subgraph penyetuju[HR Manager, Finance, Direktur Utama]
        E --> F{Disetujui semua langkah?}
        F -- Perlu diperbaiki --> F1[(Returned)]
        F1 --> B
        F -- Tidak --> F2[(Rejected)]
    end
    subgraph sistem[Sistem]
        F -- Ya --> G[(Applied)]
        G --> H([Formasi versi 1 berlaku untuk setiap baris])
    end
```

## 3. Diagram — rencana tahunan dan addendum

```mermaid
flowchart TD
    subgraph admin[HR Admin]
        A([Penyusunan rencana tahun depan]) --> B[Buat rencana atau addendum]
        B --> C[Isi usulan per unit dan jabatan beserta grade]
        C --> D[Hitung ulang: formasi saat ini, rekomendasi, estimasi biaya]
        D --> E{Usulan berbeda dari rekomendasi?}
        E -- Ya, tanpa alasan --> E1[/Ditolak, alasan wajib/]
        E1 --> C
        E -- Tidak, atau ada alasan --> F[Ajukan; potret gap terkunci]
    end
    subgraph hrm[HR Manager]
        F --> G{Periksa}
        G -- Perlu diperbaiki --> G1[(Returned)]
        G1 --> C
        G -- Tidak --> G2[(Rejected)]
    end
    subgraph finance[Finance]
        G -- Layak --> H{Ada baris tanpa tarif?}
        H -- Ya --> H1[Isi angka koreksi beserta alasan]
        H1 --> I{Anggaran cukup?}
        H -- Tidak --> I
        I -- Tidak --> G1
    end
    subgraph dirut[Direktur Utama]
        I -- Ya --> J{Sahkan?}
        J -- Tidak --> G2
        J -- Perlu diperbaiki --> G1
    end
    subgraph sistem[Sistem]
        J -- Ya --> K[(Approved)]
        K --> L[Versi formasi baru per baris pada tanggal berlakunya]
        L --> M{Ada permintaan berjalan yang kini melebihi formasi?}
        M -- Ya --> N[Tandai permintaan dan bekukan kursi berlebih]
        M -- Tidak --> O([Formasi baru berlaku])
        N --> O
    end
```

## 4. Tabel langkah

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Isi batch formasi awal | HR Admin | Dokumen formasi resmi, misalnya SK Direktur; unit, jabatan, jumlah, tanggal berlaku | Batch `Draft` | Unit × jabatan sudah punya formasi → koreksi lewat addendum |
| Ajukan batch | HR Admin | Seluruh baris valid; scan dokumen | Batch `Submitted` | Masih ada baris tidak valid atau scan belum ada → perbaiki dulu |
| Setujui batch | HR Manager, Finance, Direktur Utama | Batch | `Applied`; formasi versi 1 | Dikembalikan → HR Admin memperbaiki |
| Buat rencana | HR Admin | Tahun; tanggal acuan | Rencana `Draft` | Sudah ada rencana tahun itu → pakai addendum |
| Buat addendum | HR Admin | Rencana induk yang sudah disetujui | Addendum `Draft` memuat baris yang berubah saja | Induk belum disetujui → tunggu persetujuan induk |
| Hitung ulang | HR Admin | Usulan per baris | Formasi saat ini, rekomendasi, estimasi biaya terisi | Grade tanpa tarif → biaya tampil "tarif belum tersedia" |
| Ajukan rencana | HR Admin | Alasan; setiap baris bergrade; alasan bila berbeda dari rekomendasi | `Submitted`; potret gap terkunci | Isian kurang → layar menyebut baris yang kurang |
| Periksa anggaran | Finance | Rencana | Diteruskan atau dikembalikan; angka koreksi bila perlu | Baris tanpa tarif belum dikoreksi → Finance wajib mengisi angka koreksi |
| Sahkan | Direktur Utama | Rencana | `Approved`; versi formasi baru | Dikembalikan → HR Admin memperbaiki |
| Terapkan | Sistem | Rencana `Approved` | Formasi baru; permintaan berlebih ditandai | Gagal pada satu baris → seluruh penerapan batal; rencana tetap di langkah terakhir |

## 5. Contoh

Formasi ICU Perawat Pelaksana: 28 (batch awal, SK Direktur 12/2026) → 30 (Rencana 2027) → 32
(permintaan di luar formasi, April 2027) → 30 (Addendum 01/2027, 2 bed dialihkan ke HCU, Agustus
2027). Pada 1 Agustus ICU masih terisi 32, sehingga tampil kelebihan 2 sampai SK mutasi ke HCU
berlaku.

## 6. Yang sengaja tidak ada di diagram ini

| Yang tidak digambar | Di mana |
| --- | --- |
| Permintaan headcount | `permintaan-headcount.md` |
| Perpindahan orang karena formasi turun | SK di Workforce Core — formasi tidak memindahkan orang |
| Rumus biaya dan lapisan gap | `02-backend-architecture.md` bagian 16.9 |

## 7. Traceability

| Bagian alur | Decision | Acceptance test |
| --- | --- | --- |
| Batch formasi awal | `HRD-DEC-070` | `AT-HRD-D1-17` |
| Formasi hanya dari dokumen | `HRD-DEC-062` | `AT-HRD-D1-06`, `AT-HRD-D1-07` |
| Rencana terkunci dan addendum | `HRD-DEC-068` | `AT-HRD-D1-15` |
| Biaya dan koreksi Finance | `HRD-DEC-065` | `AT-HRD-D1-11`, `AT-HRD-D1-21` |
| Potret saat pengajuan | `HRD-DEC-069` | `AT-HRD-D1-16` |
