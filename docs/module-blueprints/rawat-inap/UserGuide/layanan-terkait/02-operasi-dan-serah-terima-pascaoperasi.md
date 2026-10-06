# Operasi dan Serah Terima Pascaoperasi

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan prasyarat

Petugas bangsal berwenang memesan ruang bedah dari instruksi tindakan yang sah, melengkapi persiapan, dan mencatat penerimaan kembali dari operasi.

Prasyarat: episode Admitted, pesanan tindakan yang aktif dan sesuai kunjungan, hak operasi yang dibutuhkan, serta kasus operasi yang benar. Jadwal dan status kasus dikendalikan modul Operasi.

## Pemesanan dari bangsal

1. Buka Keperawatan → **Pemesanan Ruangan Bedah**.
2. Pilih **Bedah Operasi** atau **Bedah Obgyn** sesuai pelayanan.
3. Pilih order tindakan dari episode yang benar.
4. Isi unit/ruang dan rincian wajib yang tersedia pada formulir.
5. Periksa identitas, order, dan jenis bedah sebelum menyimpan.
6. Simpan satu kali. Baca nomor dan status kasus pada daftar kasus operasi pasien.
7. Jika koneksi putus, periksa daftar terlebih dahulu. Sistem menggunakan kunci permintaan untuk mencegah kasus ganda; pengguna tidak perlu membuat pesanan baru sebelum memeriksa hasil lama.

## Persiapan praoperasi

1. Pilih kasus pada daftar pasien dan buka panel persiapan praoperasi yang tersedia.
2. Tinjau informasi kasus, instruksi, checklist, dan data terkait.
3. Lengkapi item berdasarkan pemeriksaan nyata.
4. Simpan dan periksa hasil/status persiapan.
5. Koordinasikan tindak lanjut bila operasi ditolak, ditunda, atau dibatalkan. Jangan mencatat persiapan sebagai bukti operasi sudah dilaksanakan.

## Penerimaan pascaoperasi

1. Buka kasus yang benar dari daftar dan panel ringkasan pascaoperasi.
2. Baca operasi, pemulihan, tujuan perawatan, waktu pengiriman, dan catatan serah terima.
3. Pastikan pasien benar-benar telah diterima di unit tujuan.
4. Gunakan aksi penerimaan serah terima yang tersedia bagi akun dan isi waktu/catatan yang diperlukan.
5. Simpan dan periksa status penerimaan serta petugas penerimanya.
6. Periksa penempatan bed. Bila tujuan rawat berubah, gunakan proses transfer/admisi yang tepat; menerima serah terima tidak otomatis membetulkan seluruh penempatan.
7. Tindak lanjuti Surveilans ILO dan instruksi pascaoperasi pada pekerjaan perawat yang terkait.

## Status, validasi, dan contoh

| Kejadian | Hasil yang diperiksa |
| --- | --- |
| Booking berhasil | Kasus operasi tersedia, awalnya Diminta menurut proses booking. |
| Kasus ditolak | Baca alasan; jangan menganggap jadwalnya masih aktif. |
| Persiapan tersimpan | Checklist/status persiapan tercatat, bukan status selesai operasi. |
| Serah terima dikirim | Bangsal masih perlu mencatat penerimaan. |
| Serah terima diterima | Pelaku dan waktu penerimaan dapat ditelusuri. |

Siti berangkat operasi pukul 08.00, kembali ke bangsal pukul 11.00, lalu diterima oleh perawat Wati. Penerimaan dicatat setelah kejadian pukul 11.00, bukan saat pemesanan ruang operasi dibuat.

Kode INP-SRG-001 menunjukkan order tidak sah/tidak sesuai kunjungan. INP-SRG-002 menunjukkan episode bukan Admitted. Periksa episode/order; jangan membuat transaksi yang sama lewat pasien lain.

## Pasien yang sebelumnya belum dirawat inap

Pasien yang memerlukan rawat inap setelah kamar pulih menggunakan [Admisi dari Kamar Pulih](../admisi/03-pasien-dari-kamar-pulih.md). Membuat permintaan admisi, membuka episode, menempatkan pasien, dan menerima serah terima adalah pekerjaan berbeda.

## Kendala, hasil akhir, dan rujukan

Jika ringkasan/panel tidak muncul, periksa status kasus dan hak Operasi. Jika penerimaan berubah oleh petugas lain, muat ulang sebelum mengirim. Bila kasus ditolak dan perlu pemesanan ulang, gunakan aksi yang disediakan dengan order yang benar.

Hasil akhir: kasus, persiapan, dan penerimaan pascaoperasi dapat ditelusuri; lokasi bangsal sesuai keadaan nyata.

Lanjutkan [transfer/penempatan](../perawatan/01-tempat-tidur-transfer-dan-penanggung-jawab.md) dan [pemantauan khusus](../keperawatan/04-pemantauan-khusus.md). Bukti: [OPERASI](../99-sumber-dan-status-panduan.md#operasi).

