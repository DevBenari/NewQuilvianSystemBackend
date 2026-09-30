# Proses — Cek Nomor Rekam Medis

Keadaan pada node `[(...)]` sama persis dengan `contracts/state-transition-matrix.md` §1.

```mermaid
flowchart TD
    subgraph pasien[Pasien]
        A([Buka Cek Nomor Rekam Medis]) --> B[Pilih No. KTP atau No. HP]
        B --> C[Isi nomor lalu tekan Cek]
        C --> V{Isian sah?}
        V -- Tidak --> V1[/Pesan validasi di bawah isian/]
        V1 --> C
    end
    subgraph sistem[Sistem]
        V -- Ya --> K[(CHECKING)]
        K --> T{Pemeriksaan berhasil?}
        T -- Tidak --> ER[(ERROR)]
        T -- Ya --> R{Berapa pasien cocok?}
        R -- Nol --> NF[(NOT_FOUND)]
        R -- Lebih dari satu --> MM[(MULTIPLE_MATCH)]
        R -- Tepat satu --> S{Pasien aktif?}
        S -- Ya --> FD[(FOUND)]
        S -- Tidak --> CS[(CONTACT_STAFF)]
    end
    ER --> ER1[Tekan Coba Lagi] --> K
    FD --> F1[Lanjut Pendaftaran Pasien Lama]
    NF --> N1[Daftar Sebagai Pasien Baru]
    MM --> M1{Dicari lewat No. HP?}
    M1 -- Ya --> M2[Cari dengan No. KTP] --> B
    M1 -- Tidak --> P1[Hubungi petugas pendaftaran]
    CS --> P1
    F1 --> X([Masuk Identifikasi Pasien Lama])
    N1 --> Y([Masuk Pendaftaran Pasien Baru])
    P1 --> Z([Kembali ke Beranda])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih metode | Pasien | — | Satu isian muncul | — |
| Isi dan cek | Pasien | Nomor | Permintaan pemeriksaan | Isian tidak sah → pesan, pasien memperbaiki |
| Pemeriksaan | Sistem | Nomor yang dinormalkan | Salah satu dari lima hasil | Jaringan/server gagal atau terlalu banyak percobaan → `ERROR`, pasien menekan Coba Lagi. **Tidak pernah** diarahkan ke Pasien Baru |
| Ditemukan | Pasien | Kartu Pasien | Masuk Pasien Lama dengan pasien terisi | Detail gagal dimuat → Identifikasi kosong + pesan, pasien mencari manual |
| Belum terdaftar | Pasien | — | Masuk Pasien Baru | — |
| Cocok ganda via HP | Pasien | — | Kembali ke isian dengan metode KTP | Tidak punya KTP → hubungi petugas |
| Cocok ganda via KTP / tidak aktif | Pasien | — | Arahan ke petugas tanpa alasan | Petugas memeriksa di layar pendaftaran |
| Tidak ada sentuhan 120 detik | Sistem | — | Data dibersihkan, kembali ke Beranda | — |
