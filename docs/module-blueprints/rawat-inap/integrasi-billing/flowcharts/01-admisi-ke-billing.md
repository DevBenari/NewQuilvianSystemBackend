# Flowchart Alur Admisi Masuk & Inisiasi Sewa Kamar

Dokumen ini memodelkan proses sejak pasien disahkan masuk rawat inap, pembuatan folio tagihan di kasir, hingga dimulainya perhitungan sewa kamar harian saat tempat tidur terisi secara fisik.

---

## 1. Diagram Alur Admisi & Sewa Kamar (Mermaid Flowchart)

```mermaid
flowchart TD
    subgraph Admisi_Dan_Perawat["Petugas Admisi & Perawat Bangsal"]
        P1["Verifikasi Berkas Masuk Pasien"] --> P2{"Status Pasien Resmi Dirawat?"}
        P2 -- "Tidak (Booking/Draft)" --> P3["Tunggu Pasien Tiba di RS"]
        P2 -- "Ya (Admitted)" --> P4["Catat Pasien Masuk Resmi"]
        P4 --> P5["Kirim Sinyal Admisi ke Kasir"]
        P5 --> P6["Antar Pasien ke Bangsal Rawat Inap"]
        P6 --> P7["Pasien Menempati Tempat Tidur"]
        P7 --> P8["Konfirmasi Tempat Tidur Terisi Fisik"]
        P8 --> P9["Kirim Sinyal Tempat Tidur Terisi"]
    end

    subgraph Kasir_Keuangan["Sistem Billing & Kasir"]
        K1["Terima Sinyal Admisi Pasien"] --> K2["Buka Lembar Tagihan Pasien"]
        K2 --> K3["Cek Kebijakan Deposit Penjamin"]
        K3 --> K4["Menunggu Pasien Menempati Bed"]
        K5["Terima Sinyal Bed Terisi Fisik"] --> K6["Catat Waktu Mulai Hunian"]
        K6 --> K7["Mulai Hitung Tarif Sewa Kamar"]
    end

    P5 -.-> K1
    P9 -.-> K5
```

---

## 2. Tabel Rincian Langkah Kerja

| No | Langkah Kerja | Pelaku / Aktor | Masukan (Input) | Keluaran (Output) | Tindakan Bila Mengalami Hambatan / Gagal |
|---:|---|---|---|---|---|
| 1 | Verifikasi Berkas Admisi | Petugas Admisi | Rujukan dokter & persetujuan rawat inap | Status admisi siap disahkan | Bila penjamin/asuransi belum jelas, arahkan keluarga ke loket admisi penjamin. |
| 2 | Pengesahan Pasien Masuk | Petugas Admisi | Tombol konfirmasi admisi masuk | Status pasien resmi dirawat; sinyal diterbitkan | Bila koneksi terputus, data tersimpan di antrean lokal dan dikirim saat jaringan stabil. |
| 3 | Pembukaan Folio Kasir | Sistem Billing | Sinyal admisi dari rawat inap | Folio tagihan terbuka berstatus aktif | Tagihan belum mengenakan biaya kamar pada tahap ini. |
| 4 | Penempatan Fisik di Bed | Perawat Bangsal | Pasien berbaring di bed ruangan | Waktu mulai hunian fisik tercatat | Pastikan bed yang ditempati sesuai dengan data sistem. |
| 5 | Inisiasi Sewa Kamar | Sistem Billing | Sinyal penempatan tempat tidur | Perhitungan tarif kamar harian mulai aktif | Jika jam masuk melewati batas malam (misal >18.00 atau >22.00), sistem billing menerapkan diskon jam masuk otomatis. |
