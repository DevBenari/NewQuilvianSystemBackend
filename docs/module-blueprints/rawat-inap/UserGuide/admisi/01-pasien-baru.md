# Admisi Pasien Baru

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan prasyarat

Petugas admisi mendaftarkan pasien yang belum mempunyai nomor rekam medis, lalu menyiapkan episode rawat inap. Mulai ketika ada kebutuhan rawat inap. Siapkan identitas pasien, kontak, cara bayar/penjamin, kelas, unit tujuan, dan DPJP.

Cari pasien terlebih dahulu bila ada kemungkinan pernah terdaftar. Pasien yang sudah mempunyai RM memakai [jalur pasien lama](02-pasien-lama.md).

## Buka halaman

Buka **Admisi Rawat Inap**, lalu pilih **Pendaftaran Pasien Baru**. Alamat halaman: `/health-services/inpatient-management/admissions`.

## Langkah penggunaan

| Langkah | Yang dikerjakan petugas | Yang perlu dipastikan |
| --- | --- | --- |
| 1. Tipe Pasien | Pilih kategori yang sesuai: Umum, Ibu, Bayi Baru Lahir, Anak, Pegawai, atau Korporat. | Kategori mengikuti pasien nyata. Bayi Baru Lahir memerlukan pilihan Episode Ibu aktif. |
| 2. Pendaftaran | Gunakan pemindaian KTP jika tersedia, lalu periksa/isi kolom wajib identitas, alamat, dokumen, dan kontak darurat. Simpan lewat aksi langkah tersebut. | Hasil pindai tetap diperiksa. Setelah pasien tersimpan, pengulangan proses memakai RM yang sudah terbentuk. |
| 3. Pembayaran | Pilih cara bayar, penjamin/kartu yang sesuai bila diperlukan, serta kelas perawatan. | Pasien, polis/perusahaan, dan kelas sesuai dokumen yang diperiksa. |
| 4. Deposit, bila tampil | Isi Nominal Diterima jika diperlukan, lalu pilih Lanjut ke Dokter. | Pada source yang diperiksa, langkah ini tampil untuk Tunai/Umum dan dilewati untuk Asuransi/Perusahaan. Isian belum menjadi transaksi penerimaan. |
| 5. Dokter | Pilih unit layanan dan DPJP, isi catatan admisi; nyalakan kebutuhan isolasi dan isi keterangan bila memang diperlukan. Simpan dan lanjutkan. | Kunjungan dan episode Draft terbentuk pada langkah ini. Catat nomor episode yang tampil. |
| 6. Pilih Bed | Pilih bed sesuai hasil ketersediaan server. | Periksa unit, kamar, kelas, dan syarat isolasi; jangan menggunakan bed yang ditolak. |
| 7. Booking Bed | Jalankan aksi pemesanan dan periksa ringkasan serta sisa waktunya. | Bed dipesan untuk episode ini; belum merupakan penempatan pasien. |
| 8. Konfirmasi | Baca ulang pasien, episode, DPJP, penjamin, kelas, dan bed. Simpan perubahan yang masih diizinkan, lalu lanjut ke cetak. | Konfirmasi admisi masih menghasilkan Draft. Ikuti petunjuk Konfirmasi Masuk melalui Papan Tempat Tidur saat pasien tiba. |
| 9. Cetak Persetujuan Pasien Ranap | Lengkapi informasi yang diminta formulir dan buka cetak/pratinjau. | Cetak ini tidak membuktikan tanda tangan kertas sudah tersimpan di sistem. |
| 10. Kartu Pasien | Cetak kartu pasien dan periksa identitasnya. | RM yang tercetak sama dengan pasien yang baru didaftarkan. |

Jumlah langkah yang terlihat dapat berkurang bila Deposit dilewati. Ikuti nama langkah, bukan mengandalkan nomor urut semata.

## Kapan data tersimpan dan status berubah

| Kejadian | Hasil |
| --- | --- |
| Pendaftaran berhasil disimpan | Pasien/RM terbentuk; belum berarti episode sudah ada. |
| Langkah Dokter berhasil | Kunjungan dan episode Draft terbentuk. |
| Pemesanan berhasil | Reservasi bed aktif sesuai waktu server. |
| Konfirmasi/cetak selesai | Administrasi admisi selesai, episode masih Draft. |
| Pasien tiba dan penempatan berhasil | Episode menjadi Admitted; bed menjadi terisi. |

## Contoh

Budi, RM fiktif CONTOH-001, memilih Tunai/Umum dan kelas yang sesuai. Petugas mengetik Rp500.000 pada langkah Deposit pukul 08.10. Angka ini belum merupakan bukti uang tercatat di Billing. Pada 08.15 langkah Dokter membentuk episode Draft. Bed dipesan 08.20. Penempatan nyata pukul 09.00 mengubah episode menjadi Admitted.

## Validasi dan jalur tidak normal

- **Bayi Baru Lahir:** pilih episode ibu yang benar pada panel Episode Ibu. Jika tidak tersedia/gagal dibaca, hentikan langkah dan minta pemeriksaan data; jangan mengganti kategori menjadi Anak/Umum untuk melewati syarat.
- **Kunjungan gagal dibuat:** proses berhenti sebelum episode dibuka. Periksa pesan dan keberadaan data sebelum mengulang.
- **Kunjungan tersimpan, episode gagal:** ikuti pesan layar dan periksa kunjungan yang sudah terbentuk; jangan membuat pasien baru kedua.
- **Kebutuhan isolasi gagal tersimpan:** episode dapat sudah terbentuk. Periksa ulang isolasi di Detail Episode sebelum penempatan.
- **Bed kedaluwarsa/terisi:** periksa ulang ketersediaan; Draft tetap dapat dilanjutkan.
- **Deposit:** batas implementasi dan cara menindaklanjutinya ada pada [panduan keuangan](../keuangan/01-deposit-tagihan-dan-status-kasir.md).

## Hasil akhir dan proses berikutnya

Ada pasien terdaftar, episode Draft, dan dokumen cetak admisi. Serahkan nomor episode serta kebutuhan rawat ke petugas bangsal. Lanjutkan [penempatan tempat tidur](../perawatan/01-tempat-tidur-transfer-dan-penanggung-jawab.md).

Bukti urutan, titik simpan, dan API: [ADMISI](../99-sumber-dan-status-panduan.md#admisi).

