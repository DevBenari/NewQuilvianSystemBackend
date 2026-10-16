# Alur Proses — Form Permintaan Rawat Inap dari IGD

| Field | Nilai |
| --- | --- |
| Status | `approved` (`IGD-DEC-294`, Rizki Gunawan, 12 Oktober 2026) — Rencana (belum tersedia) |
| Sumber | `IGD-DEC-267`…`293` |
| Pelaku | Dokter IGD, Perawat IGD, DPJP IGD aktif, petugas admisi Rawat Inap, sistem |

Alur berlaku untuk tindak lanjut yang jenisnya mewajibkan Form Permintaan Rawat Inap (rawat inap). Nama keadaan sama dengan
state §11: *Draf*, *Menunggu TTD*, *Ditandatangani* (tampil *Terkirim*), *Selesai*, *Dibatalkan*. Aturan rinci di validation
§14.

## 1. Jalur utama dan jalur pengembalian

```mermaid
flowchart TD
    subgraph DOK["Dokter IGD"]
        A[Membuat dan mengonfirmasi tindak lanjut rawat inap]
    end
    subgraph PRW["Perawat IGD"]
        B[Membuka form dari tindak lanjut itu]
        C[Melengkapi kelas, DPJP usulan, lama rawat, kebutuhan khusus]
        D{Isian wajib lengkap dan ada DPJP IGD aktif berakun?}
        E[Minta TTD]
        L[Jalankan tindak lanjut, siapkan kepergian]
        R[Memperbaiki form]
    end
    subgraph DPJ["DPJP IGD aktif"]
        F[Melihat Perlu TTD dan membaca ringkasan]
        G{Setuju isinya?}
        H[Tandatangani]
        K[Kembalikan dengan catatan]
    end
    subgraph SIS["Sistem"]
        I[Bekukan isi, catat atestasi, lahirkan permintaan admisi dalam satu transaksi]
        J{Berhasil?}
    end
    subgraph ADM["Petugas admisi"]
        M[Membuka admisi dari permintaan]
        N[Selesai]
    end
    A --> B --> C --> D
    D -- Belum --> C
    D -- Ya --> E --> F --> G
    G -- Ya --> H --> I --> J
    G -- Tidak --> K --> R --> C
    J -- Ya, Terkirim --> L
    J -- Ya, Terkirim --> M --> N
    J -- Gagal --> F
```

| No | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| ---: | --- | --- | --- | --- | --- |
| 1 | Konfirmasi tindak lanjut rawat inap | Dokter IGD | Diagnosis tercatat | Tindak lanjut terkonfirmasi | Konfirmasi tertahan diagnosis — dokter menulis catatan dokter dulu |
| 2 | Buka form | Perawat | Tindak lanjut terkonfirmasi | Form *Draf* terisi unit, indikasi, instruksi | Sudah ada form aktif → buka form yang ada |
| 3 | Lengkapi | Perawat | Kelas, DPJP usulan, lama rawat, CITO, kebutuhan khusus | Draf tersimpan | Isian tidak sah → perbaiki sesuai pesan |
| 4 | Minta TTD | Perawat | Draf lengkap | *Menunggu TTD* di daftar DPJP aktif | Tanpa DPJP / DPJP tanpa akun → tetapkan atau alihkan DPJP dulu |
| 5 | Baca dan putuskan | DPJP IGD aktif | Ringkasan | TTD atau dikembalikan | Bukan DPJP aktif → tombol tidak tersedia |
| 6 | Bekukan dan kirim | Sistem | Isi form, data pasien terkini | *Terkirim*; permintaan admisi menunggu | Data pasien tak terbaca / permintaan lain masih menunggu → TTD ditolak, form tetap *Menunggu TTD* |
| 7 | Jalankan | Perawat | Form terkirim | Kepergian fisik dimulai (alur kepergian yang sudah ada) | Form belum terkirim → *Jalankan* tertahan |
| 8 | Buka admisi | Petugas admisi | Permintaan | Episode rawat inap; permintaan *Selesai* | Lihat jalur 2 |

*Contoh:* dr. Andi mengonfirmasi rawat inap Tn. Budi 10.05. Dimas membuka form 10.10, memilih kelas 2 dan dr. Sinta, lalu
meminta TTD 10.12. dr. Andi mengembalikan 10.14 dengan catatan *"Minta ICU"*; Dimas mencentang Perawatan Intensif dan
meminta TTD lagi 10.16. dr. Andi menandatangani 10.18; Sari melihat permintaan CITO di urutan teratas dan membuka admisi
10.40.

## 2. Jalur pembatalan

```mermaid
flowchart TD
    subgraph PRW["Perawat IGD"]
        A1[Batalkan form dengan alasan]
        A2[Buat form baru bila perlu]
    end
    subgraph DOK["Dokter IGD"]
        B1[Batalkan tindak lanjut rawat inap]
    end
    subgraph ADM["Petugas admisi"]
        C1[Batalkan permintaan dengan alasan]
    end
    subgraph SIS["Sistem"]
        D1{Admisi sudah dibuka?}
        D2[Form dan permintaan menunggu dibatalkan]
        D3[Ditolak: batalkan admisi di Rawat Inap dulu]
        D4[Jalankan tertahan lagi; IGD melihat Dibatalkan admisi]
    end
    A1 --> D1
    B1 --> D1
    D1 -- Belum --> D2 --> A2
    D1 -- Sudah --> D3
    C1 --> D4 --> A2
```

| No | Kejadian | Pelaku | Akibat | Bila ditolak |
| ---: | --- | --- | --- | --- |
| 1 | Perawat membatalkan form (salah isi) | Perawat | Form dan permintaan menunggu batal; form baru boleh dibuat | Admisi sudah dibuka → tidak dapat dibatalkan dari IGD |
| 2 | Dokter membatalkan tindak lanjut rawat inap | Dokter | Form dan permintaan menunggu ikut batal dengan alasan yang sama | Admisi sudah dibuka → batalkan admisi di Rawat Inap dulu |
| 3 | Petugas admisi membatalkan permintaan | Petugas admisi | IGD melihat *Dibatalkan admisi — alasan*; *Jalankan* tertahan lagi | — |

*Contoh jalur tidak normal:* Sari membatalkan permintaan Tn. Budi 10.50 karena kelas 2 Bangsal Melati penuh. Di layar
Dimas, form tampil *Dibatalkan admisi — Kelas 2 Bangsal Melati penuh* dan *Jalankan* kembali nonaktif. Dimas membuat form
baru untuk Bangsal Mawar kelas 2 dan meminta TTD ulang.

## 3. Amandemen 14 Oktober 2026 — surat pengantar ke Antrean Transfer IGD/Poli (`IGD-DEC-304`, `305`) — **`approved`** (`IGD-DEC-306`)

Menggantikan langkah petugas admisi di bagian 1 dan cabang "petugas admisi membatalkan" di bagian 2. Langkah lain tetap.
Pelaku bertambah: perawat ruangan rawat inap (penerima serah terima).

```mermaid
flowchart TD
    subgraph DOK["Dokter IGD"]
        A1["Tetapkan dan konfirmasi tindak lanjut rawat inap<br/>dengan unit tujuan rawat inap"]
    end
    subgraph PRW["Perawat IGD"]
        B1["Buat dan lengkapi form"]
        B2["Minta tanda tangan DPJP"]
        B3["Batalkan form bila rencana berubah"]
        B4["Antar pasien dengan serah terima"]
    end
    subgraph DPJ["DPJP IGD aktif"]
        C1{"Isi form benar?"}
        C2["Tandatangani"]
        C3["Kembalikan dengan catatan"]
    end
    subgraph ADM["Petugas admisi Rawat Inap"]
        D1["Lihat pasien di Antrean Transfer IGD/Poli"]
        D2["Proses Admisi:<br/>verifikasi, pembayaran, dokter, tempat tidur"]
    end
    subgraph RNG["Perawat ruangan"]
        E1["Terima serah terima dan catat tiba"]
    end
    A1 --> B1 --> B2 --> C1
    C1 -- Ya --> C2 --> D1
    C1 -- Tidak --> C3 --> B1
    D1 --> D2 --> B4 --> E1
    D1 -. belum diproses .-> B3
    E1 --> F1(["Kunjungan IGD selesai"])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Konfirmasi tindak lanjut | Dokter IGD | Keputusan rawat inap, unit tujuan rawat inap | Tindak lanjut terkonfirmasi | Unit tujuan bukan unit rawat inap: form tidak dapat dibuat — dokter memperbaiki tindak lanjut |
| Buat dan lengkapi form | Perawat IGD | Tindak lanjut terkonfirmasi | Form *Draf* | Sudah ada form aktif: buka form itu |
| Tandatangani | DPJP IGD aktif | Form *Menunggu TTD* | Form *Terkirim*; pasien tampil di antrean | Bukan DPJP aktif: minta DPJP yang benar menandatangani |
| Lihat antrean | Petugas admisi | Form bertanda tangan | Baris pasien asal IGD | Pasien tidak tampil: periksa apakah form sudah ditandatangani |
| Proses Admisi | Petugas admisi | Baris antrean | Admisi terbuka; tindak lanjut *Executed*; form *Selesai* | Bed atau penjamin bermasalah: diselesaikan di langkah admisi Rawat Inap |
| Batalkan form | Perawat IGD | Form *Terkirim* belum diproses | Form *Dibatalkan*; pasien keluar dari antrean | Sudah diproses: pembatalan ditolak; batalkan admisi di Rawat Inap lebih dulu |
| Antar dengan serah terima | Perawat IGD | Admisi sudah diproses | Kepergian dengan SBAR | Admisi belum diproses: tunggu, jangan antar dulu — kunjungan IGD tidak akan tertutup otomatis |
| Terima dan catat tiba | Perawat ruangan | Kepergian dari IGD | Kunjungan IGD selesai | Dokumen tidak lengkap: tolak dengan alasan, perawat IGD melengkapi |
