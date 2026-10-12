# Alur Proses — Menetapkan Dokter Penanggung Jawab IGD

| Field | Nilai |
| --- | --- |
| Status | `approved` (`IGD-DEC-240`, Rizki Gunawan, 11 Oktober 2026) — Rencana (belum tersedia) |
| Sumber | `IGD-DEC-141`, `237`…`239` |
| Pelaku | Perawat triage / perawat IGD; backend IGD |

Alur ini berlaku pada penetapan DPJP pertama sesudah triage tersimpan dan pada pengalihan DPJP. Aturan rincinya ada di
validation §13.

```mermaid
flowchart TD
    A[Perawat membuka pemilih DPJP] --> B[Backend menyusun daftar pada waktu itu]
    B --> C{Ada dokter berjadwal di unit bertipe Emergency?}
    C -- Ya --> D[Kelompok 1: dokter jaga IGD saat ini]
    C -- Tidak --> E[Kelompok 1 kosong, layar memberi keterangan]
    D --> F[Kelompok 2: dokter umum/emergency di luar jadwal]
    E --> F
    F --> G{Kelompok 2 kosong?}
    G -- Ya --> H[Kelompok 2 diisi semua dokter aktif yang tidak jaga]
    G -- Tidak --> I[Perawat memilih dokter]
    H --> I
    I --> J{Dokter dari kelompok 1?}
    J -- Ya --> K[Simpan tanpa alasan override]
    J -- Tidak --> L[Perawat menulis alasan di luar jadwal]
    L --> M[Simpan]
    K --> N[Backend memeriksa ulang kelayakan pada waktu penugasan]
    M --> N
    N --> O{Hasil}
    O -- Dokter jaga --> P[Tersimpan, bukan override]
    O -- Di luar jadwal dan beralasan --> Q[Tersimpan dengan penanda dan alasan]
    O -- Di luar jadwal tanpa alasan --> R[Ditolak 400: isi alasan]
    O -- Bukan keduanya --> S[Ditolak 400: pilih dokter lain]
    R --> L
    S --> B
```

| No | Langkah | Pelaku | Masukan | Keluaran |
| ---: | --- | --- | --- | --- |
| 1 | Buka pemilih DPJP | Perawat | Kunjungan IGD | Daftar dua kelompok dari backend |
| 2 | Pilih dokter | Perawat | Daftar | Dokter terpilih; isian alasan muncul bila di luar jadwal |
| 3 | Simpan | Perawat | Dokter; alasan bila di luar jadwal; alasan pengalihan bila mengalihkan | Permintaan ke backend |
| 4 | Periksa ulang | Backend | Dokter, waktu penugasan | Diterima, atau ditolak `400` |
| 5 | Catat | Backend | — | Baris riwayat penugasan, dengan penanda override bila di luar jadwal |

*Contoh jalur tidak normal:* daftar dibuka pukul 13.58 dan dr. Sari masih berlabel shift pagi (07.00–14.00). Perawat
menyimpan pukul 14.01 tanpa alasan. Backend menghitung ulang — dr. Sari sudah di luar jadwal — dan menolak karena alasan
kosong. Daftar dimuat ulang, dan dr. Sari kini berada di kelompok kedua.
