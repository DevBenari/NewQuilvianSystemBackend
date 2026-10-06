# Pemberian Obat dan Pemakaian Alat

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan prasyarat

Perawat berwenang mencatat pemberian obat serta pemakaian alat berdasarkan resep/instruksi yang sah. Pemicu adalah waktu dosis, kebutuhan obat sesuai instruksi, atau mulai/selesainya pemakaian alat.

Siapkan episode, resep aktif, tanggal MAR, waktu kejadian, dan petugas pemeriksa kedua bila diperlukan. **MAR** adalah catatan pemberian obat; membuat resep tidak berarti obat sudah diberikan.

## Buka pekerjaan

Buka Keperawatan → Asuhan Keperawatan → Obat & Alkes. Panel dan menu Pemakaian Alat menyediakan pekerjaan terkait; gunakan panel yang sesuai dengan jenis pencatatan.

## Catatan pemberian obat

1. Periksa identitas, alergi, resep aktif, rute, dan instruksi pemberian yang tampil.
2. Pilih tanggal MAR yang benar. Periksa jadwal/dosis dan apakah item resep telah dihentikan.
3. Pilih sel dosis atau aksi pencatatan yang tersedia.
4. Pilih hasil sebenarnya. Kontrak pencatatan menyediakan Administered (diberikan), Held (ditahan), Refused (ditolak), atau Missed (terlewat).
5. Isi waktu nyata, catatan, serta alasan untuk hasil yang bukan diberikan sesuai validasi layar.
6. Jika sistem meminta cek ganda, periksa status Menunggu Cek Ganda. Petugas kedua menggunakan akun sendiri untuk mengonfirmasi atau menolak dengan alasan.
7. Simpan satu kali, kemudian periksa status dosis dan riwayat. Jangan menyimpulkan Administered sebelum jawaban server menyatakan demikian.
8. Untuk pemberian PRN/sesuai kebutuhan, gunakan aksi yang disediakan dan catat evaluasi tindak lanjut pada panelnya.
9. Bila tersedia pelaksanaan sliding scale, gunakan order/version yang aktif, hasil pemeriksaan, dan langkah verifikasi yang ditampilkan. Jangan membuat rentang dosis baru lewat catatan perawat.

## Obat dari rumah dan alat

| Pekerjaan | Langkah pengguna |
| --- | --- |
| Obat dari rumah | Catat obat yang dilaporkan pasien melalui rekonsiliasi. Periksa keputusan dokter apakah diteruskan/dihentikan; laporan pasien tidak otomatis menjadi resep aktif. |
| Alkes habis pakai/order | Pilih produk, jumlah, dan catatan sesuai instruksi melalui Order Alat Kesehatan; periksa riwayat setelah simpan. |
| Alat yang dipakai selama periode waktu | Buka pencatatan pemakaian alat, pilih alat/instruksi, isi waktu mulai, kemudian periksa catatan aktif. |
| Pemakaian berakhir | Gunakan aksi penghentian/penyelesaian yang tersedia; isi waktu nyata dan alasan/catatan sesuai kebutuhan. |
| Pasien keluar ruangan | Sistem mempunyai tindak lanjut penutupan pemakaian alat berjalan. Bila muncul peringatan kegagalan tindak lanjut, periksa riwayat dan laporkan; jangan mencatat kepergian dua kali. |

## Status, contoh, dan validasi

| Peristiwa | Yang diperiksa |
| --- | --- |
| Dosis baru dijadwalkan | Belum merupakan pemberian. |
| Cek ganda Pending | Pemeriksaan belum selesai; bukan sinonim obat telah diberikan. |
| Pencatatan berhasil | Status, waktu, penulis, dan alasan pada dosis sesuai hasil nyata. |
| Koreksi | Riwayat revisi beralasan dapat ditelusuri. |
| Alat dihentikan | Waktu akhir dan riwayat penggunaan tersedia. |

Contoh: dosis Budi dijadwalkan 08.00 tetapi pasien menolak. Perawat memilih Refused dan mengisi alasan serta waktu; ia tidak memilih Administered agar tabel terlihat lengkap. Bila koreksi waktu diperlukan, gunakan koreksi pada dosis tersebut.

- Periksa tanggal sebelum memasukkan pemberian; sel pada hari lain merupakan kejadian berbeda.
- Penolakan cek ganda perlu alasan. Jangan memakai akun pemberi pertama sebagai akun petugas kedua.
- Resep dihentikan tidak dapat dianggap tetap aktif hanya karena baris lama masih terlihat.
- Bila jumlah/waktu tidak sah atau data berubah bersamaan, baca pesan, muat ulang, lalu periksa dosis yang sudah tercatat.
- Reaksi obat dicatat melalui pekerjaan yang sesuai; lihat [pemantauan khusus](04-pemantauan-khusus.md).

## Hasil akhir dan rujukan

Pemberian/tidak diberikannya obat serta pemakaian alat mempunyai waktu, pelaku, status, dan riwayat yang jelas. Pelaksanaan klinis mengikuti instruksi yang disahkan.

Resep dibuat melalui [panduan dokter](../dokter/03-resep-obat-dan-tindakan.md). Bukti dan API: [OBAT DAN ALAT](../99-sumber-dan-status-panduan.md#obat-alat).

