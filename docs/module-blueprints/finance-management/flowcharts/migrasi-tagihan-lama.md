# Alur proses — Migrasi tagihan lama (dua format berkas)

| Field | Nilai |
|---|---|
| Keputusan yang diturunkan | `FIN-DEC-129`, `FIN-DEC-136` (jalannya: spreadsheet, divalidasi, disetujui per batch), `FIN-DEC-140` (dua format: CSV dan XLSX) |
| Rancangan | `FIN-DES-089`, `FIN-DES-090`, `FIN-DES-093`, `02-backend-architecture.md` AMENDMENT REVISI 15 |
| Status | `draft` — menunggu persetujuan owner |
| Catatan | Diagram ini **tidak** memuat nama tabel, kolom, endpoint, maupun class. Pesan penolakan persisnya ada di `contracts/validation-matrix.md` bagian `G.2`, tidak disalin ke sini |

## Alur normal beserta jalur gagalnya

```mermaid
flowchart TD
    subgraph Penyiap["Penyiap cutover"]
        A1[Memilih jenis tagihan: piutang atau utang]
        A2[Memilih format templat: CSV atau XLSX]
        A3[Mengunduh templat]
        A4[Mengisi tagihan lama di luar sistem]
        A5[Mengunggah berkas]
        A6[Memperbaiki baris yang bergalat]
        A7[Menyatakan saldo awal dari dokumen Accounting]
        A11[Memeriksa ulang isi berkas atau angka yang dinyatakan]
    end

    subgraph Sistem["Sistem Finance"]
        B1{Jenis dan format dipilih keduanya?}
        B2[Mengirim templat sesuai jenis dan format]
        B3{Berkas berformat CSV atau XLSX?}
        B4[Menguraikan berkas menjadi baris bernomor]
        B5{Seluruh baris lolos validasi?}
        B6[Batch siap: menunggu pernyataan saldo awal]
        B7{Total sisa sama dengan saldo awal yang dinyatakan?}
        B8[Membuat item tagihan beserta mutasi pembukanya]
        B9[Batch terkunci; tagihan lama masuk penagihan normal]
        C1[Menolak: pilih jenis dan format lebih dulu]
        C2[Menolak: format berkas tidak diterima]
        C3[Mencatat galat per baris beserta nomornya]
        C4[Menolak: kedua angka ditampilkan berdampingan]
    end

    subgraph Penyetuju["Penyetuju cutover"]
        D1[Menyetujui batch]
    end

    A1 --> A2 --> B1
    B1 -- Belum --> C1 --> A1
    B1 -- Sudah --> B2 --> A3 --> A4 --> A5 --> B3
    B3 -- Bukan --> C2 --> A5
    B3 -- Ya --> B4 --> B5
    B5 -- Tidak --> C3 --> A6 --> A5
    B5 -- Ya --> B6 --> A7 --> B7
    B7 -- Tidak --> C4 --> A11
    A11 --> A7
    B7 -- Ya --> D1 --> B8 --> B9
```

## Tabel langkah

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 1 | Memilih jenis dan format templat | Penyiap | Jenis tagihan, format berkas | Templat terunduh | Keduanya wajib. Tanpa salah satunya, unduhan ditolak |
| 2 | Mengisi tagihan lama | Penyiap | Daftar tagihan lama dari pembukuan luar sistem | Berkas terisi | Format angka dan tanggal **MUST** mengikuti templat — khususnya pada CSV |
| 3 | Mengunggah berkas | Penyiap | Berkas CSV atau XLSX | Batch baru beserta asal formatnya | Format lain ditolak. Format ditentukan sistem dari berkasnya, **bukan** dari pilihan pengguna |
| 4 | Menguraikan dan memvalidasi | Sistem | Berkas | Baris bernomor beserta galatnya | Galat dicatat **per baris beserta nomornya**. Batch tetap tertahan, tidak hilang |
| 5 | Memperbaiki baris bergalat | Penyiap | Daftar galat beserta nomor baris | Berkas perbaikan | Petugas memperbaiki di luar sistem lalu mengunggah ulang |
| 6 | Menyatakan saldo awal Accounting | Penyiap | Angka dari dokumen Accounting beserta rujukan dokumennya | Batch siap disetujui | Angka yang dinyatakan **MUST** berasal dari dokumen Accounting, bukan dihitung sistem |
| 7 | Merekonsiliasi | Sistem | Total sisa item vs saldo awal yang dinyatakan | Lanjut, atau penolakan | Penolakan menampilkan **kedua angka berdampingan**, bukan hanya "tidak cocok" |
| 8 | Menyetujui batch | Penyetuju | Batch yang sudah direkonsiliasi | Item tagihan beserta mutasi pembukanya, dalam satu transaksi | Penyiap **tidak** dapat menyetujui batch yang disiapkannya sendiri bila haknya tidak mencakup persetujuan |
| 9 | Penagihan normal | Petugas AR/AP | Tagihan lama yang sudah terbit | Penagihan, pelunasan, umur piutang seperti tagihan biasa | — |

## Jalur gagal yang paling sering ditemui petugas

| Keadaan | Yang dilihat petugas | Yang **MUST NOT** terjadi |
|---|---|---|
| Excel lokal Indonesia menulis `1.500.000,00` pada CSV | Baris itu ditolak beserta nomornya | Angkanya **ditebak** menjadi nilai lain |
| Berkas disimpan sebagai `.ods` | Pesan menyebut CSV dan XLSX | Berkas diterima lalu gagal di tengah penguraian |
| Satu nomor dokumen kembar di dalam satu berkas | Baris kedua ditolak beserta nomor baris pasangannya | Keduanya masuk, melahirkan tagihan ganda |
| Saldo awal yang dinyatakan berbeda dari total item | Kedua angka ditampilkan berdampingan | Batch disetujui dengan selisih yang tidak dijelaskan |
| Berkas yang sama diunggah sebagai CSV dan sebagai XLSX | Hasil uraian **identik**, hanya asal formatnya berbeda | Kedua format menghasilkan jumlah baris atau nomor baris yang berbeda |
