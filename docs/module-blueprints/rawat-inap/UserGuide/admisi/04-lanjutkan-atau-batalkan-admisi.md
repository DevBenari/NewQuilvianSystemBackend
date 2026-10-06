# Melanjutkan atau Membatalkan Admisi

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan pemicu

Petugas admisi menangani alur yang terhenti, pemesanan bed yang tidak berlaku, atau admisi yang tidak jadi dilanjutkan. Pemeriksaan dimulai dari data yang sudah tersimpan, sehingga pasien/kunjungan/episode tidak dibuat dua kali.

## Tentukan posisi terakhir

| Alur terhenti setelah | Data yang mungkin sudah ada | Jalan lanjut |
| --- | --- | --- |
| Pendaftaran pasien | Pasien dan RM; belum ada episode | Cari melalui pasien lama. |
| Simpan langkah Dokter | Kunjungan dan episode Draft | Buka daftar episode, gunakan Lanjutkan Admisi. |
| Booking Bed | Draft dengan reservasi | Periksa reservasi aktif dan sisa waktu. |
| Konfirmasi/cetak | Draft dan dokumen cetak | Catat penempatan saat pasien benar-benar tiba. |
| Penempatan | Admitted | Bekerja melalui Detail Episode/ruang kerja; bukan pelanjutan Draft. |

## Melanjutkan Draft

1. Buka daftar episode di `/health-services/inpatient-management/episodes`.
2. Cari pasien/nomor episode, lalu gunakan penyaring status Draft bila tersedia.
3. Cocokkan identitas dan waktu pembukaan episode.
4. Klik **Lanjutkan Admisi** pada episode Draft yang benar.
5. Tunggu pembacaan episode dan reservasi. Episode tanpa reservasi dilanjutkan ke Pilih Bed; reservasi yang masih aktif dapat membawa pengguna ke Konfirmasi.
6. Periksa penjamin, unit, kelas, DPJP, kebutuhan isolasi, dan masa reservasi yang ditampilkan.
7. Selesaikan pemesanan/konfirmasi/cetak yang belum selesai. Jangan membuat episode kedua.
8. Lanjutkan penempatan terpisah saat pasien tiba.

## Pemesanan kedaluwarsa atau diganti

1. Muat ulang ketersediaan bed.
2. Jika ingin mengganti pemesanan yang masih aktif, batalkan pemesanan lama melalui aksi yang tersedia sebelum memesan bed lain.
3. Pilih bed yang layak berdasarkan jawaban server.
4. Periksa ringkasan dan waktu berlaku reservasi baru.

Reservasi Expired tidak otomatis berarti episode Cancelled. Jika bed sudah diambil pasien lain, pemesanan/penempatan dapat ditolak sementara episode tetap Draft. Tidak ada durasi universal dalam panduan ini; durasi berasal dari Pengaturan Rawat Inap.

## Membatalkan admisi

1. Buka Detail Episode yang benar.
2. Pilih aksi pembatalan jika akun dan status mengizinkan.
3. Isi alasan yang menjelaskan kejadian, misalnya “Pasien tidak jadi dirawat; keluarga memilih fasilitas lain”.
4. Baca dampak pada dialog lalu konfirmasi satu kali.
5. Pastikan status menjadi Cancelled dan reservasi/penempatan terkait dibebaskan sesuai hasil server.

| Dari | Tindakan | Ke | Batas |
| --- | --- | --- | --- |
| Draft | Lanjutkan/pesan ulang | Draft | Belum ada penempatan. |
| Draft | Batalkan dengan alasan | Cancelled | Hak pembatalan diperlukan. |
| Admitted | Batalkan | Cancelled | Kewenangan supervisor/kepala ruangan dan belum ada catatan klinis; server memeriksa. |
| Cancelled | Lanjutkan | Ditolak | Status akhir. |
| DischargePending/Closed | Batalkan admisi | Ditolak | Gunakan proses pemulangan/koreksi yang sesuai. |

## Isian salah dan tombol Kembali

Tombol Kembali mengubah langkah layar; ia tidak menghapus data yang sudah tersimpan. Penjamin/DPJP yang sudah terikat pada kunjungan/episode tidak boleh dianggap dapat diubah hanya dengan mundur. Perubahan unit/kelas/catatan harus mengikuti aksi yang disediakan, dan perubahan yang memengaruhi kelayakan bed perlu diperiksa ulang.

Jika pasien sudah benar-benar dirawat, pengalihan DPJP dilakukan melalui penugasan pada Detail Episode. Pembetulan uang dilakukan oleh Billing; mengubah angka Deposit lokal tidak membalik transaksi.

## Contoh dan hasil akhir

Admisi Budi tersimpan pada 08.15, tetapi layar ditutup pukul 08.25. Pada 09.00 petugas mencari episode Draft dan menggunakan Lanjutkan Admisi. Bila reservasi tidak lagi aktif, pilih/pesan bed kembali. RM dan episode lama tetap digunakan.

Hasil akhirnya salah satu dari: Draft berhasil dilanjutkan untuk penempatan, atau admisi Cancelled dengan alasan dan jejak pembatalan. Jika hasil simpan tidak jelas karena koneksi putus, baca ulang detail sebelum mengulang aksi.

Bukti: [ADMISI](../99-sumber-dan-status-panduan.md#admisi) dan [EPISODE](../99-sumber-dan-status-panduan.md#episode). Bantuan lain: [kendala dan koreksi](../bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md).

