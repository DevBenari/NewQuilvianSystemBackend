# Alur — Tautan Kunjungan Asal Jalur Transfer

> Revisi blueprint `2.2`, status **draft**. Masukan: `BKC-DEC-155`, `BKC-DES-077`–`BKC-DES-080`.

Alur ini berjalan otomatis, tanpa petugas, setiap kali Billing menerima kabar bahwa seorang pasien
resmi dirawat inap. Petugas baru terlibat di ujung, bila tautannya tidak dapat dibuat.

```mermaid
flowchart TD
  subgraph RI[Rawat Inap]
    A[Pasien IGD atau poli ditransfer ke rawat inap] --> B[Pasien menyetujui rawat inap]
    B --> C[Kabar rawat inap dikonfirmasi dikirim ke Billing]
  end
  subgraph BL[Billing - otomatis]
    C --> D[Buka atau temukan tagihan rawat inap]
    D --> E[Tautkan kunjungan asal dari rujukan operasi]
    E --> F{Rawat inap ini lahir dari transfer?}
    F -- Tidak --> Z[Selesai tanpa tautan transfer]
    F -- Ya --> G{Kunjungan asal sudah tertaut?}
    G -- Ya --> Z2[Selesai, tautan lama dipertahankan]
    G -- Tidak --> H{Pasien sama, kunjungan asal tidak batal, jenis sumber dikenal?}
    H -- Ya --> I[Tulis tautan dengan sebab IGD atau Rawat Jalan]
    H -- Tidak --> J[Lewati tautan dan catat peringatan]
    I --> K[Tagihan asal ikut di rincian rawat inap]
  end
  subgraph KS[Kasir - saat kelayakan pulang dinilai]
    J --> L[Kelayakan berstatus perlu rekonsiliasi]
    M[Transfer lama sebelum fitur rilis] --> L
    L --> N{Kasir memutuskan}
    N -- Ikutkan --> O[Tagihan asal dihitung, permanen]
    N -- Kecualikan --> P[Tagihan asal tidak dihitung, permanen]
  end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka tagihan rawat inap | Billing (otomatis) | Kabar rawat inap dikonfirmasi | Tagihan rawat inap terbuka | Kunjungan tidak dikenal → kabar ditolak dan masuk antrean gagal milik Rawat Inap (perilaku yang sudah ada) |
| Tautkan rujukan operasi | Billing (otomatis) | Rujukan operasi yang selesai | Tautan sebab operasi | Tidak berubah dari perilaku yang sudah ada |
| Tulis tautan transfer | Billing (otomatis) | Asal transfer pada kunjungan rawat inap | Satu tautan sebab IGD atau Rawat Jalan | Pasien berbeda, kunjungan asal batal, atau jenis sumber tak dikenal → tautan dilewati, peringatan dicatat, tagihan rawat inap tetap terbuka |
| Nilai kelayakan pulang | Billing (otomatis, `MVP-37`) | Rantai kunjungan | Status kelayakan | Asal transfer tanpa tautan → perlu rekonsiliasi; kasir membuka layar rekonsiliasi |
| Putuskan rekonsiliasi | Kasir atau Supervisor Kasir | Kejanggalan asal transfer belum tertaut | Keputusan permanen | Keputusan tidak dapat dibalik; kasir **MUST** membaca konfirmasi sebelum menekan |
