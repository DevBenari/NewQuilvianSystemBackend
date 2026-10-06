# Admisi Pasien dari Kamar Pulih

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan pemicu

Petugas admisi menindaklanjuti permintaan rawat inap dari kamar pulih setelah dokter anestesi menentukan pasien membutuhkan rawat inap atau ICU. Permintaan berasal dari proses Operasi; petugas admisi memilih permintaan yang tersedia.

Prasyarat: pasien dan kasus operasi dikenali; permintaan admisi masih Pending; data tingkat perawatan/unit tujuan, penjamin, dan DPJP dapat diperiksa.

## Buka dan pilih permintaan

1. Buka Admisi Rawat Inap.
2. Pilih **Admisi dari Kamar Pulih**.
3. Pada **Rujukan Kamar Pulih**, cari pasien dan baca daftar permintaan. Periksa nama/RM, riwayat operasi, waktu permintaan, serta kebutuhan rawat.
4. Pilih permintaan yang benar. Periksa rincian sebelum melanjutkan; jangan memasukkan pasien yang sama lewat admisi biasa untuk menggantikan permintaan ini.
5. Pada **Informasi Pasien**, cocokkan identitas dengan dokumen serah terima.
6. Lanjutkan **Tipe Pasien** dan **Pembayaran**, termasuk penjamin serta kelas yang berlaku.
7. Bila Deposit tampil, pahami batas isian pada panduan keuangan.
8. Tinjau langkah **Dokter**. Periksa data yang diisi dari rujukan, DPJP, unit tujuan, dan kebutuhan isolasi. Jangan menganggap semua isian otomatis pasti sesuai keadaan saat penerimaan.
9. Simpan pembukaan episode; periksa nomor episode dan hubungan dengan permintaan.
10. Pilih/pesan bed yang sesuai, periksa ringkasan Konfirmasi, lalu cetak persetujuan.
11. Saat pasien tiba di bangsal, catat penempatan dan selesaikan [serah terima pascaoperasi](../layanan-terkait/02-operasi-dan-serah-terima-pascaoperasi.md) sesuai kewenangan.

## Status permintaan dan episode

| Kejadian | Status permintaan | Status episode | Makna |
| --- | --- | --- | --- |
| Permintaan dibuat oleh proses kamar pulih | Pending | Belum ada | Admisi menunggu tindak lanjut. |
| Episode berhasil dibuka dari permintaan | Completed | Draft | Permintaan sudah mempunyai episode tujuan; pasien belum tentu tiba. |
| Pasien ditempatkan | Completed | Admitted | Perawatan di bangsal telah aktif. |
| Permintaan dibatalkan oleh proses pemilik sebelum dipenuhi | Cancelled | Belum ada | Jangan memakai permintaan itu untuk membuka admisi. |

Permintaan Completed bukan bukti serah terima bangsal sudah diterima. Kedua pekerjaan memiliki catatan dan kewenangan tersendiri.

## Contoh

Pasien fiktif Siti, RM CONTOH-003, selesai operasi pada 10.00. Permintaan admisi dibuat dari kamar pulih pada 10.15. Petugas memilih permintaan itu pukul 10.25 dan memperoleh episode Draft. Pada 11.00 bangsal menerima pasien, mencatat penempatan, dan memeriksa serah terima operasi. Kunjungan asal operasi tetap dapat ditelusuri dari hubungan permintaan tersebut.

## Jalur tidak normal dan validasi

| Pesan/kondisi | Artinya | Tindakan |
| --- | --- | --- |
| INP-ADM-REF-001 pada admisi biasa | Pasien memiliki permintaan kamar pulih yang harus digunakan. | Kembali ke jalur kamar pulih dan pilih permintaan yang benar. |
| INP-ADM-REF-002/permintaan tidak sesuai | Permintaan tidak sah, tidak Pending, atau tidak cocok dengan pasien. | Muat ulang detail; periksa bersama petugas kamar pulih. |
| Permintaan hilang setelah dipilih | Petugas lain dapat sudah memenuhinya/membatalkannya. | Periksa episode dan status terbaru sebelum mencoba lagi. |
| Bed tujuan tidak layak | Kebutuhan pasien dan bed tidak memenuhi pemeriksaan server. | Pilih bed/unit yang sesuai, jangan mengubah kebutuhan rawat semata untuk lolos. |
| Tagihan operasi belum terlihat | Hubungan dan pemrosesan Billing perlu diperiksa. | Minta kasir memeriksa kunjungan asal serta tagihan; jangan memasukkan biaya kedua tanpa pemeriksaan. |

## Hasil akhir dan rujukan

Permintaan terhubung dengan episode yang benar; riwayat operasi/kunjungan asal dapat ditelusuri. Verifikasi tampilan dan hubungan tagihan pada lingkungan target sebelum mengandalkan pengisian otomatis.

Lanjutkan [penempatan](../perawatan/01-tempat-tidur-transfer-dan-penanggung-jawab.md). Bukti source dan API: [KAMAR PULIH](../99-sumber-dan-status-panduan.md#kamar-pulih).

