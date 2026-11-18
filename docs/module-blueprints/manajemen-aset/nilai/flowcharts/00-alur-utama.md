# Nilai Aset — Alur Utama

```mermaid
flowchart TD
    subgraph akuntansi[Kepala Akuntansi]
        A([Sebelum go-live]) --> B[Tetapkan batas kapitalisasi, umur, residu, akun kontrol]
    end
    subgraph register[Register]
        B --> C[Aset disetujui]
        C --> D[(Aktif)]
    end
    subgraph petugas[Petugas Aset]
        D --> E[Akhir bulan: buka pratinjau penyusutan]
        E --> F[Proses penyusutan bulan itu]
    end
    subgraph sistem[Sistem]
        F --> G[Nilai buku setiap aset berkurang]
        G --> H[Kejadian dan saldo masuk kotak keluar]
        H --> I[Pekerja mengirim ke Akuntansi]
    end
    I --> J([Jurnal penyusutan dan saldo rekonsiliasi tersedia di Akuntansi])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Tetapkan parameter | Kepala Akuntansi | Angka batas, umur, residu, kode akun | Parameter berlaku | Tanpa parameter, aset tidak dapat disetujui |
| Aset disetujui | `register` | — | Buku nilai `Aktif` | — |
| Pratinjau | Petugas Aset | Periode | Ringkasan dan galat | Selesaikan galat (pusat biaya, kebijakan) |
| Proses | Petugas Aset | Pratinjau bersih | Proses tersimpan, buku diperbarui | Periode sebelumnya belum diproses → proses dulu |
| Kirim | Pekerja | Kotak keluar | `Acknowledged` | `Failed` → Kepala Akuntansi mencoba ulang |
