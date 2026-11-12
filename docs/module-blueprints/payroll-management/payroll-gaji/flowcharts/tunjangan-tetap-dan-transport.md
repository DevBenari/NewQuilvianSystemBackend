# Proses — Tunjangan Tetap dari Struktur Gaji dan Tunjangan Transportasi

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1`, status `approved` (Yasmin, 2026-11-12) |
| `last_changed_in` | `PGJ-C1` |
| Traceability | `EPIC PGJ-01`, `PGJ-02`, `PGJ-03`; PRL-DEC-02, 04, 12, 13, 46 |

## 1. Mengatur Struktur Gaji dan tunjangannya

```mermaid
flowchart TD
    subgraph petugas[Petugas Payroll]
        A([Struktur gaji perlu diatur]) --> B[Isi batas gaji struktur]
        B --> C{Batas berada di dalam rentang grade?}
        C -- Tidak --> C1[/Ditolak, batas keluar rentang grade/]
        C1 --> B
        C -- Ya --> D[Struktur tersimpan]
        D --> E[Tambah jenis tunjangan dan cara hitungnya]
        E --> F{Jenis sudah ada di struktur ini?}
        F -- Ya --> F1[/Ditolak, ubah baris yang sudah ada/]
        F1 --> E
        F -- Tidak --> G{Jenis dihitung dari aktivitas kerja?}
        G -- Ya --> G1[/Ditolak, bukan tunjangan tetap/]
        G1 --> E
        G -- Tidak --> H[Baris tunjangan tersimpan]
    end
    subgraph hr[Petugas HRD]
        I([Rentang grade perlu diubah]) --> J{Struktur aktif tetap di dalam rentang baru?}
        J -- Tidak --> J1[/Ditolak beserta daftar struktur yang terdampak/]
        J1 --> K[Sesuaikan struktur dulu]
        K --> J
        J -- Ya --> L[Grade tersimpan]
    end
    H --> M([Siap dipakai saat run dihitung])
    L --> M
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi batas gaji struktur | Petugas Payroll | Grade struktur | Struktur tersimpan | Petugas menyesuaikan batas ke dalam rentang grade |
| Tambah jenis tunjangan | Petugas Payroll | Jenis tunjangan aktif, nominal atau persen | Baris tunjangan | Petugas mengubah baris yang sudah ada, atau memakai Penghasilan Variabel untuk tunjangan berbasis aktivitas |
| Ubah rentang grade | Petugas HRD | Rentang baru | Grade tersimpan | Petugas menyesuaikan struktur yang disebut, lalu mengulang |

## 2. Saat run dihitung

```mermaid
flowchart TD
    subgraph sistem[Sistem, per pegawai]
        A([Run dihitung]) --> B{Struktur pada penetapan gaji memuat tunjangan?}
        B -- Ya --> C[Hitung tiap tunjangan: nominal atau persen, batas maksimum, prorata]
        C --> C1{Profil juga punya angka tunjangan?}
        C1 -- Ya --> C2[Catat peringatan, angka profil diabaikan]
        C1 -- Tidak --> D
        C2 --> D
        B -- Tidak --> E{Saklar cadangan profil menyala dan profil berisi angka?}
        E -- Ya --> E1[Pakai angka profil, catat peringatan]
        E -- Tidak --> E2[Tanpa tunjangan tetap]
        E1 --> D
        E2 --> D
        D{Ada tunjangan transport aktif?} -- Tidak --> H
        D -- Ya --> F{Lebih dari satu transport bulanan beririsan?}
        F -- Ya --> F1[/Pegawai gagal dihitung, perbaiki tunjangan transport/]
        F -- Tidak --> G{Metode bulanan tetap?}
        G -- Ya --> G1[Hitung transport, batas bulanan, prorata]
        G -- Tidak --> G2[Catat peringatan, masukkan lewat Penghasilan Variabel]
        G1 --> H
        G2 --> H
        H([Baris hasil hitung tersimpan])
    end
    F1 --> X([Petugas memperbaiki lalu menghitung ulang])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Hitung tunjangan struktur | Sistem | Penetapan gaji disetujui, baris tunjangan aktif | Satu baris per jenis tunjangan | — |
| Cadangan profil | Sistem | Struktur kosong, saklar menyala | Satu baris "Tunjangan Tetap" dan peringatan | Petugas mengisi struktur lalu menghitung ulang |
| Hitung transport | Sistem | Tunjangan transport aktif metode bulanan tetap | Satu baris transport | Dua tunjangan beririsan: pegawai gagal dihitung; petugas menonaktifkan salah satunya |
| Metode transport lain | Sistem | Metode per kehadiran, per hari, penggantian | Peringatan | Petugas memasukkan nominal lewat Penghasilan Variabel |
