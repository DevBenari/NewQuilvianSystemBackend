# Proses — Pembayaran dan Penjamin Utama

Bagian dari step `PAYMENT` (`contracts/state-transition-matrix.md` §2).

```mermaid
flowchart TD
    subgraph pasien[Pasien]
        A([Masuk step Pembayaran]) --> B{Metode?}
        B -- Tunai --> T1[Pilih metode bayar tunai]
        B -- Asuransi atau Perusahaan --> C{Penjamin milik pasien?}
    end
    subgraph sistem[Sistem]
        C -- Hanya asuransi --> KA[Kondisi A: pilih kartu asuransi seperti biasa]
        C -- Hanya perusahaan --> KB[Kondisi B: pilih perusahaan seperti biasa]
        C -- Asuransi dan perusahaan --> KC[Kondisi C: tampilkan Pilih Penjamin Utama tanpa pilihan otomatis]
        C -- Tidak punya --> KN[/Belum ada penjamin terdaftar, pilih Tunai atau hubungi petugas/]
    end
    KN --> B
    KC --> P{Sudah memilih satu?}
    P -- Belum --> P1[/Wajib pilih satu penjamin utama/] --> KC
    P -- Sudah --> OK[(PAYMENT selesai: satu penanggung untuk kunjungan ini)]
    KA --> OK
    KB --> OK
    T1 --> OK
    OK --> NX[Lanjut ke Layanan & Dokter]
    NX --> CF[Konfirmasi dan Daftar]
    CF --> V{Penjamin masih sah?}
    V -- Tidak --> V1[/Penjamin tidak dapat dipakai/] --> A
    V -- Ya --> DONE([Kunjungan terdaftar dengan satu penanggung])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih metode | Pasien | Tunai / Asuransi / Perusahaan | — | — |
| Kondisi A / B | Pasien | Kartu asuransi **atau** perusahaan milik pasien | Satu penanggung | Tidak ada kartu aktif → pilih Tunai atau hubungi petugas |
| Kondisi C | Pasien | Kartu asuransi **dan** perusahaan | Satu penanggung dipilih sendiri; default di data pasien **tidak** berubah | Belum memilih → tidak bisa lanjut |
| Konfirmasi | Sistem | Penanggung terpilih | Kunjungan dengan satu penanggung | Penjamin tidak aktif / tidak eligible / habis masa berlaku → pasien kembali ke Pembayaran atau ke petugas |
| Setelah terdaftar | — | — | Penjamin tidak bisa diganti dari Kiosk | Pasien menemui petugas/kasir |
