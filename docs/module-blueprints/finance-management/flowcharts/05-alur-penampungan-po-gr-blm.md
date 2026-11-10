# Alur Penampungan PO/GR dari BLM

```yaml
last_changed_in: Revisi 23 putaran 2 (9 November 2026)
status: approved   # 2026-11-09, pemilik (pengguna sesi), termasuk FIN-DES-141
input_revision: 00-interview-decisions.md FIN-DEC-261..278; 02-backend-architecture.md bagian W
```

Alur ini menggambarkan **arti** tabel penampung dan aturan penerimaan Revisi 23. Pengolah kiriman belum dibangun.
Nama status sama dengan `contracts/state-transition-matrix.md` bagian W.

## 1. Kiriman PO atau GR dari BLM

```mermaid
flowchart TD
    subgraph BLM
        A[PO disetujui atau GR dicatat] --> B[Kirim ke Quilvian beserta isi per satuan tiap item]
    end
    subgraph Sistem[Quilvian]
        B --> C{Nomor kejadian sudah pernah diterima?}
        C -- Ya, isi sama --> C1[Abaikan sebagai ulangan]
        C -- Ya, isi berbeda --> K1[Catat konflik isi berbeda]
        C -- Belum --> D[Simpan di kotak masuk: Diterima]
        D --> E{Lebih lama dari data yang sudah diterapkan?}
        E -- Ya --> E1[Tandai Usang]
        E -- Tidak --> F{Supplier sudah dipetakan?}
        F -- Belum --> G[Menunggu Pemetaan]
        F -- Sudah --> V{Konversi satuan setiap item pasti?}
        V -- Tidak --> K3[Catat konflik konversi; dokumen tidak diterapkan]
        V -- Ya --> H{Nomor dokumen dipakai dokumen lain?}
        H -- Ya --> K2[Catat konflik tabrakan nomor]
        H -- Tidak --> I[Buat atau perbarui PO/GR bertanda sumber BLM]
        I --> S[Hitung ulang penerimaan sah per item dan status PO]
        S --> J[Kotak masuk: Diterapkan]
    end
    subgraph Finance[Petugas Finance]
        G --> G1[Isi pemetaan supplier]
        G1 --> F
        K1 --> L[Tinjau dan selesaikan konflik]
        K2 --> L
        K3 --> L
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Kirim | BLM | PO atau GR beserta item, satuan, dan isi per satuan | Kiriman bernomor kejadian | BLM mengirim ulang dengan nomor kejadian sama |
| Simpan | Sistem | Kiriman | Kotak masuk "Diterima" | Tidak tersimpan → BLM tidak menerima tanda terima dan mengulang |
| Periksa pemetaan | Sistem | Supplier BLM | Lanjut atau "Menunggu Pemetaan" | Petugas memetakan supplier; kiriman dicoba lagi |
| Periksa konversi | Sistem | Isi per satuan dan satuan dasar tiap item | Lanjut atau konflik | Konflik konversi; BLM mengirim kiriman baru yang lengkap. PO tidak pernah dianggap diterima penuh dari data yang tidak pasti |
| Terapkan dan hitung | Sistem | Kiriman lengkap | PO/GR bersumber BLM; status PO dari jumlah sah per item | Tabrakan nomor → konflik |

## 2. Penerimaan bertahap dengan satuan berbeda

```mermaid
flowchart TD
    subgraph BLM
        A[PO 1 BOX, isi 12 PCS] --> B[GR pertama 6 PCS kondisi baik]
        B --> C[GR kedua 6 PCS]
    end
    subgraph Sistem[Quilvian]
        A --> P[Dipesan 12 PCS]
        B --> Q[Diterima sah 6 dari 12: diterima sebagian]
        C --> R{Kondisi GR kedua?}
        R -- Semua baik --> R1[Diterima sah 12 dari 12: diterima penuh]
        R -- Sebagian menunggu periksa --> R2[Diterima sah kurang dari 12: tetap diterima sebagian]
        R2 --> R3[Setelah lolos periksa, BLM mengirim ulang baris itu sebagai baik]
        R3 --> R1
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| PO diterapkan | Sistem | 1 BOX, isi 12 | Dipesan 12 dalam satuan dasar | Isi per satuan tidak ada → konflik konversi |
| GR pertama | Sistem | 6 PCS baik | Sah 6, sisa 6, diterima sebagian | — |
| GR kedua | Sistem | 6 PCS | Penuh bila semua baik; sebagian bila ada yang rusak atau menunggu periksa | Barang rusak diselesaikan lewat penggantian, retur, atau koreksi PO |

## 3. GR dibatalkan di BLM setelah utang diakui

```mermaid
flowchart TD
    subgraph BLM
        A[Pengguna BLM membatalkan GR] --> B[Kirim pemberitahuan batal]
    end
    subgraph Sistem[Quilvian]
        B --> C[Simpan di kotak masuk dan catatan pembatalan]
        C --> D{GR sudah punya Tukar Faktur, invoice, utang, atau pembayaran?}
        D -- Tidak --> E[GR ditandai Dibatalkan dan status PO dihitung ulang]
        D -- Ya --> F[Catat konflik; GR dan utang tidak diubah otomatis]
    end
    subgraph Finance[Petugas Finance]
        F --> G[Tinjau invoice, utang, pembayaran, dan dampak Accounting]
        G --> H[Catat koreksi resmi di modulnya]
        H --> I[Tandai konflik selesai dengan kode penyelesaian]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Batal di BLM | Pengguna BLM | GR | Pemberitahuan batal | Seharusnya dicegah di BLM bila ada turunan AP (`FIN-OQ-141`) |
| Periksa turunan | Sistem | GR | Dibatalkan atau konflik | — |
| Tinjau dan selesaikan | Finance | Konflik | Koreksi dan konflik selesai | Konflik tetap terbuka sampai diselesaikan |
