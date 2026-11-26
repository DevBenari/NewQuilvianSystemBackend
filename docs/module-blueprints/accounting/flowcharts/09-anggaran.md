# Accounting — Menyusun, Menyetujui, dan Merevisi Anggaran

| Field | Nilai |
|---|---|
| Status | **`draft`** — menunggu gerbang desain Rizki |
| Dasar | `ACC-DEC-185`..`199` |
| Lingkup | Membuat anggaran tahun buku, mengisi baris, mengajukan, menyetujui atau menolak, dan merevisi di tengah tahun |

## Tujuan

Staf anggaran menyusun anggaran pendapatan dan beban satu tahun buku; pengguna lain yang berwenang
menyetujuinya sehingga target resmi berlaku. Bila perlu diubah di tengah tahun, dibuat versi baru tanpa
mengubah target bulan yang sudah ditutup. Alur ini tidak membuat jurnal.

## Alur 1 — Menyusun dan menyetujui

```mermaid
flowchart TD
    subgraph Penyusun["Staf anggaran"]
        A[Buat anggaran tahun buku] --> B[Isi baris di layar atau unggah CSV]
        B --> C[Ajukan]
        H[Perbaiki baris] --> C
        T[Tarik pengajuan] --> B
    end
    subgraph Sistem["Sistem"]
        A --> A1{Tahun buku sudah dibangkitkan dan belum punya anggaran?}
        A1 -- Tidak --> A2[Tolak dengan saran]
        B --> B1{Semua baris sah?}
        B1 -- Tidak --> B2[Tunjukkan baris yang salah, tidak ada yang disimpan]
        B2 --> B
        C --> C1{Ada baris?}
        C1 -- Tidak --> C2[Tolak pengajuan]
    end
    subgraph Penyetuju["Penyetuju (bukan penyusun)"]
        D{Setujui?}
        D -- Ya --> E[Anggaran berlaku sebagai target resmi]
        D -- Tidak --> F[Tolak dengan alasan]
    end
    C1 -- Ya --> D
    F --> H
```

## Alur 2 — Revisi di tengah tahun

```mermaid
flowchart TD
    subgraph Penyusun["Staf anggaran"]
        R[Buat revisi dari anggaran yang berlaku] --> S[Ubah nilai bulan yang masih terbuka]
        S --> U[Ajukan]
    end
    subgraph Sistem["Sistem"]
        R --> R1{Masih ada versi lain yang belum selesai?}
        R1 -- Ya --> R2[Tolak: selesaikan atau hapus versi itu dulu]
        S --> S1{Ada nilai bulan tertutup yang berubah?}
        S1 -- Ya --> S2[Tolak dan sebut barisnya]
        S2 --> S
    end
    subgraph Penyetuju["Penyetuju"]
        V{Setujui?}
        V -- Ya --> W[Versi baru berlaku, versi lama digantikan]
        V -- Tidak --> X[Tolak dengan alasan]
    end
    S1 -- Tidak --> U
    U --> V
    X --> S
```

## Langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buat anggaran | Staf anggaran | Tahun buku, keterangan | Versi 1 Draft | Tahun belum dibangkitkan: bangkitkan di Periode Akuntansi. Sudah ada: buka versi yang ada atau buat revisi |
| Isi baris | Staf anggaran | Akun, unit biaya, nilai per bulan / CSV | Baris tersimpan | Baris salah disebut nomornya; perbaiki lalu simpan/unggah ulang |
| Ajukan | Staf anggaran | — | Diajukan | Tanpa baris: isi baris dulu |
| Tarik | Pengaju | — | Kembali Draft | Hanya pengaju yang bisa |
| Setujui / Tolak | Penyetuju | Alasan bila menolak | Disetujui / Ditolak | Pengaju tidak bisa menyetujui sendiri: minta penyetuju lain |
| Revisi | Staf anggaran | — | Versi baru Draft berisi salinan baris | Masih ada versi lain yang belum selesai: selesaikan atau hapus |
| Hapus | Staf anggaran | — | Versi terhapus | Hanya Draft/Ditolak |
