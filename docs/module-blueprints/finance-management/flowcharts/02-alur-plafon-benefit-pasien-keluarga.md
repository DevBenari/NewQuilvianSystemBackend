# Alur Proses — Plafon Benefit Pasien Keluarga Pegawai: Mendaftar, Menagih, dan Mengoreksi

| Field | Nilai |
|---|---|
| Slice | `EPIC FIN-04` — celah implementasi `REV-19` |
| Domain Terkait | HR Benefit, Registrasi Pasien, Billing & Kasir, Finance AR |
| Keputusan yang Diturunkan | `FIN-DEC-204` s.d. `FIN-DEC-210` |
| Rujukan Arsitektur | `02-backend-architecture.md` Bagian R; `contracts/integration-contract.md` Bagian R (`FIN-INTEGRATION-1.9`) |
| Status | `draft` — menunggu persetujuan pemilik |
| Catatan Kepatuhan | Diagram **tidak** memuat nama tabel, kolom, endpoint, atau nama class. Pesan penolakan rinci ada di `contracts/validation-matrix.md` bagian `R` |

Melengkapi `01-alur-piutang-manfaat-karyawan-end-to-end.md`: berkas itu menggambar ujung ke ujung; berkas ini
memerinci tiga bagian yang belum tersambung di kode.

---

## 1. Mendaftarkan Keluarga di HR, Lalu Menerbitkan Kartu di Registrasi

```mermaid
flowchart TD
    subgraph HR["Petugas dan Pejabat HR"]
        H1[Mendaftarkan pegawai ke plan benefit]
        H2[Pejabat menyetujui dan mengaktifkan]
        H3[Mendaftarkan anggota keluarga sebagai tanggungan]
        H4{Hubungan dan jumlah tanggungan sesuai plan?}
        H5[Mengajukan verifikasi]
        H6[Pejabat memverifikasi masa berlaku dan batas plafon]
        H7[Tanggungan ditolak dengan alasan]
    end

    subgraph REG["Petugas Registrasi"]
        R1[Mencari pegawai dengan NIP, NIK, atau nama]
        R2[Memilih anggota keluarga yang berobat]
        R3{Sistem memeriksa: sudah terdaftar dan terverifikasi?}
        R4[Kartu penjamin internal terbit dengan hubungan dari HR]
        R5[Kartu ditolak dengan alasan yang jelas]
        R6[Mendaftarkan pasien sebagai Pasien Umum tunai]
        R7[Hubungan tersalin ke data kunjungan]
    end

    H1 --> H2 --> H3 --> H4
    H4 -- Sesuai --> H5 --> H6
    H4 -- Tidak sesuai --> H7
    R1 --> R2 --> R3
    H6 -.-> R3
    R3 -- Ya --> R4 --> R7
    R3 -- Tidak --> R5 --> R6
    H7 -.-> R5
```

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 1 | Mendaftarkan pegawai ke plan benefit | Petugas HR | Pegawai, plan, masa berlaku | Enrollment berstatus `Draft` | Plan tanpa batas plafon ditolak; lengkapi plan |
| 2 | Menyetujui dan mengaktifkan | Pejabat HR (bukan pengaju) | Enrollment `Submitted` | Enrollment `Active`, saldo awal = batas plan | Pengaju yang sama menyetujui: ditolak |
| 3 | Mendaftarkan tanggungan | Petugas HR | Anggota keluarga aktif milik pegawai | Tanggungan `Draft` | Bukan anggota keluarga pegawai itu, hubungan tidak dibolehkan plan, atau melebihi batas jumlah: ditolak |
| 4 | Memverifikasi | Pejabat HR | Tanggungan `Requested`, masa berlaku, batas plafon | Tanggungan `Verified` | Ditolak dengan alasan wajib; mengajukan ulang berarti membuat tanggungan baru |
| 5 | Mencari pegawai dan memilih keluarga | Petugas Registrasi | Minimal 3 karakter kata kunci | Daftar pegawai (NIK tersamar) dan keluarganya | Data HR tidak terbaca: ulangi atau daftarkan sebagai Pasien Umum |
| 6 | Memeriksa kelayakan | Sistem | Pegawai, anggota keluarga, tanggal layanan | Layak atau tidak beserta alasan | Tidak layak: kartu ditolak, jalur Pasien Umum tunai |
| 7 | Menerbitkan kartu | Petugas Registrasi | Hasil kelayakan `OK` | Kartu tertaut ke pegawai dan keluarga; hubungan diisi sistem | — |

**Contoh.** Sari (anak Budi, nama samaran) sudah terverifikasi: kartu terbit, hubungan "anak". Pak Andi (paman) tidak
terdaftar: tidak bisa dipilih, jadi petugas mendaftarkannya sebagai Pasien Umum tunai.

---

## 2. Finalisasi Tagihan: Membagi Porsi dan Pilihan Pegawai

```mermaid
flowchart TD
    subgraph KSR["Kasir dan Pegawai"]
        K1[Membuka pratinjau finalisasi tagihan]
        K2[Pegawai memilih untuk sisa tagihan pasien: bayar lunas di kasir atau jadikan utang]
        K3[Kasir mencatat pilihan dan mengesahkan finalisasi]
        K4[Kasir menerima pembayaran tunai sisa tagihan]
        K5[Finalisasi ditahan, menunggu konfirmasi HR]
    end

    subgraph BIL["Sistem Billing"]
        B1[Membaca sisa plafon orang yang berobat]
        B2{Plafon dapat dibaca dan orang masih layak?}
        B3[Menghitung porsi RS Benefit dan kelebihan di atas plafon]
        B4[Menerbitkan serah terima: porsi RS Benefit dan porsi pegawai]
        B5[Mengurangi plafon di HR dalam transaksi yang sama]
        B6{Plafon masih cukup saat dikurangi?}
        B7[Menghitung ulang dari sisa terkini]
    end

    subgraph FIN["Finance"]
        F1[Mencatat piutang RS Benefit]
        F2[Mencatat piutang pegawai]
        F3[Pegawai melunasi atau mengajukan cicilan]
    end

    K1 --> B1 --> B2
    B2 -- Tidak --> K5
    B2 -- Ya --> B3 --> K2 --> K3 --> B4 --> B5 --> B6
    B6 -- Cukup --> F1
    B6 -- Tidak cukup --> B7 --> B3
    B4 --> F2 --> F3
    K2 -. sisa dibayar tunai .-> K4
```

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 1 | Membaca sisa plafon | Sistem Billing | Pegawai dan anggota keluarga, tanggal layanan | Sisa plafon orang itu | Tidak terbaca atau tidak layak: finalisasi **ditahan**, hubungi HR; tidak ada tebakan "semua ditanggung RS" atau "semua ditanggung pegawai" |
| 2 | Menghitung dua porsi | Sistem Billing | Porsi yang ditanggung penjamin menurut aturan, sisa plafon | Porsi RS Benefit dan kelebihan di atas plafon | Ada penjamin kedua: ditolak, belum didukung |
| 3 | Memilih untuk sisa tagihan pasien | Pegawai (dicatat kasir) | Sisa tagihan pasien (co-pay, item di luar cakupan) | Nominal yang dijadikan utang, 0 sampai seluruhnya | Melebihi sisa tagihan: ditolak |
| 4 | Menerbitkan serah terima | Sistem Billing | Hasil langkah 2 dan 3 | Satu sampai dua baris, tidak pernah bernilai nol | — |
| 5 | Mengurangi plafon | Sistem Billing | Porsi RS Benefit | Saldo HR berkurang tepat sebesar porsi itu | Tidak cukup karena finalisasi lain: hitung ulang sekali; bila masih gagal, finalisasi dibatalkan seluruhnya |
| 6 | Mencatat piutang | Finance | Serah terima | Piutang RS Benefit dan piutang pegawai | Tanpa pemilik manfaat yang valid: ditolak |

**Contoh.** Tagihan Rp 5.000.000; aturan penjamin RS Benefit menanggung 80% (Rp 4.000.000); sisa plafon Rp 3.000.000;
sisa tagihan pasien Rp 1.000.000. Porsi RS Benefit Rp 3.000.000, kelebihan di atas plafon Rp 1.000.000. Bila pegawai memilih
membayar sisa Rp 1.000.000 tunai: piutang pegawai Rp 1.000.000. Bila pegawai menjadikannya utang: piutang pegawai Rp 2.000.000.

---

## 3. Koreksi Salah Orang dan Pengembalian Plafon

```mermaid
flowchart TD
    subgraph LKT["Petugas Loket atau Kasir"]
        L1[Menemukan salah memilih pegawai atau anggota keluarga]
        L2{Tagihan sudah final?}
        L3[Mengganti penanggung langsung pada kunjungan]
        L4[Mengajukan Berita Acara Koreksi]
    end

    subgraph BIL["Sistem Billing"]
        B1[Menerbitkan pembalik untuk serah terima lama]
        B2[Mengembalikan plafon orang yang salah]
        B3[Menerbitkan serah terima pengganti untuk orang yang benar]
        B4[Memakai plafon orang yang benar dengan kunci baru]
    end

    subgraph FIN["Finance dan HR"]
        F1[Membatalkan piutang lama]
        F2[Menerbitkan piutang baru atas pegawai yang benar]
        F3{Cicilan salah orang sudah terpotong dari gaji?}
        F4[HR menambah gaji periode berikutnya]
    end

    L1 --> L2
    L2 -- Belum --> L3
    L2 -- Sudah --> L4 --> B1 --> B2 --> B3 --> B4
    B1 --> F1 --> F3
    B3 --> F2
    F3 -- Ya --> F4
```

| # | Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---:|---|---|---|---|---|
| 1 | Menandai kesalahan | Petugas loket atau kasir | Kunjungan dan orang yang benar | Berita Acara Koreksi (bila sudah final) | Tanpa alasan: ditolak |
| 2 | Membalik serah terima lama | Sistem Billing | Berita Acara | Pembalik; plafon orang yang salah **pulih penuh** | Sudah pernah dikembalikan: tidak berubah, aman diulang |
| 3 | Menerbitkan pengganti | Sistem Billing | Orang yang benar | Serah terima baru dengan kunci baru; plafon orang yang benar berkurang | Orang yang benar tidak layak atau plafon kurang: koreksi ditahan |
| 4 | Membatalkan dan menerbitkan piutang | Finance | Pembalik dan pengganti | Piutang lama `CANCELLED`, piutang baru terbit | Tidak pernah mengubah nama debitur secara manual |
| 5 | Mengembalikan potongan gaji | HR | Peristiwa pembalikan angsuran | Penyesuaian penambah gaji pada slip berikutnya | Finance tidak mengeluarkan kas manual |

**Contoh.** Tagihan Rp 2.000.000 salah dibebankan ke pegawai A (plafon terpakai Rp 2.000.000). Setelah koreksi, plafon A kembali
Rp 2.000.000 dan plafon pegawai B yang benar berkurang Rp 2.000.000. Bila Rp 500.000 sudah terpotong dari gaji A, slip gaji A bulan
depan bertambah Rp 500.000.
