# Kendala, Koreksi, dan Pertanyaan Umum

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan cara memakai

Petugas menemukan langkah tindak lanjut tanpa menggandakan data atau mengubah proses yang sudah final. Mulai dari pesan layar, RM/episode yang dipilih, aksi terakhir, waktu kejadian, dan riwayat hasil simpan.

## Kendala yang sering ditemui

| Kondisi | Arti yang perlu diperiksa | Tindak lanjut |
| --- | --- | --- |
| Menu/tombol tidak ada | Hak, penugasan, atau status tidak sesuai. | Periksa [akses](../01-persiapan-dan-hak-akses.md) dengan administrator/pemilik penugasan. |
| 401 | Sesi tidak terautentikasi. | Masuk kembali sesuai prosedur lingkungan target. |
| 403 | Pengguna tidak berwenang untuk tindakan itu. | Periksa role, profesi, penugasan aktif, dan hak API. |
| 404 | Data/alamat tidak ditemukan atau tidak tersedia. | Periksa pasien, episode, dan sumber tautan. |
| 400 | Isian tidak sah/tidak lengkap. | Perbaiki kolom yang disebut dan format/satuan yang diminta. |
| 409 | Konflik, data berubah, aksi sudah dilakukan, atau perlu pengakuan peringatan. | Baca kode/pesan; muat ulang detail sebelum mengulangi. |
| 422 | Aturan proses belum terpenuhi. | Selesaikan syarat yang disebut; jangan mengubah data fiktif untuk melewati syarat. |
| Bed tidak bisa dipilih | Terisi, dipesan, atau tidak layak menurut unit/kelas/isolasi. | Baca alasan dan muat ulang ketersediaan. |
| Simpan tidak jelas karena koneksi putus | Permintaan mungkin sudah tersimpan. | Baca riwayat/detail sebelum mengirim kembali. |
| Tidak ada hasil penunjang | Belum selesai atau pembacaan gagal. | Periksa status layanan dan pesan kegagalan. |
| Status kasir gagal dibaca | Izin tidak dapat diverifikasi saat itu. | Jangan menyebut lunas; hubungi kasir dan gunakan proses yang sesuai. |
| Layar hanya baca | Episode/dokumen final atau akses penulisan tidak sah. | Gunakan koreksi berwenang bila memang perlu. |

## Koreksi penempatan yang salah

Gunakan untuk kesalahan catatan waktu/bed/kelas, bukan perpindahan pasien nyata.

1. Buka Detail Episode dan riwayat penempatan.
2. Pilih baris yang salah serta aksi koreksi bila hak akun mengizinkan.
3. Periksa status invoice. Source mengizinkan koreksi ketika invoice OPEN atau belum terbentuk (NONE); invoice final/tidak terbaca perlu tindak lanjut Billing.
4. Isi koreksi dan alasan yang bermakna, misalnya “Kelas pada penempatan awal salah pilih; data diperiksa terhadap catatan penerimaan”.
5. Baca ringkasan versi yang akan diganti lalu simpan.
6. Periksa riwayat: baris awal ditandai dikoreksi dan penggantinya dapat ditelusuri.
7. Koordinasikan pembacaan ulang tarif oleh kasir. Jangan membuat transfer palsu atau biaya kamar kedua.

Contoh: Budi sebenarnya di VIP sejak 08.00 tetapi tercatat kelas 1. Koreksi mengubah catatan penempatan yang salah; transfer pada 14.00 akan berarti pasien benar-benar pindah dan tidak menggambarkan kejadian tersebut.

## Koreksi dokumen dan episode Closed

| Jenis kesalahan | Jalur yang sesuai |
| --- | --- |
| Dokumen masih draf | Perbarui dokumen yang sama sesuai hak penulis. |
| Dokumen klinis final | Addendum/amandemen melalui fitur dokumen yang tersedia; penulis, alasan, dan versi dipertahankan. |
| Episode Closed perlu koreksi administrasi yang diizinkan | Supervisor membuka sesi koreksi beralasan melalui halaman correction, melakukan perubahan yang diizinkan, lalu menutup sesi dengan ringkasan. |
| Kepergian fisik sudah tercatat | Tidak disediakan pembatalan rutin. Periksa kejadian bersama petugas berwenang; jangan mencoba mengisi ulang sebagai pengganti. |

Sesi koreksi tidak membuat status episode keenam: episode tetap Closed, bed tidak diambil kembali, pasien tidak kembali ke sensus aktif. Sesi episode juga tidak memberi hak menimpa semua dokumen klinis; tiap dokumen mempunyai aturan sendiri.

## Pertanyaan umum

**Konfirmasi admisi selesai, mengapa masih Draft?**  
Penempatan pasien saat tiba belum dicatat. Buka Papan Tempat Tidur/Detail Episode.

**Reservasi habis, apakah pasien harus didaftarkan ulang?**  
Tidak otomatis. Periksa Draft dan gunakan Lanjutkan Admisi untuk pemesanan/penempatan yang benar.

**Mengapa pasien sudah keluar tetapi belum Closed?**  
Kepergian melepas bed; penutupan masih memerlukan lima syarat.

**Apakah menyimpan SOAP sudah mencatat visite?**  
Tidak. Visite merupakan kejadian tersendiri lewat Catat Visite.

**Apakah angka Deposit membuktikan uang sudah masuk?**  
Tidak pada source alur admisi yang diperiksa. Periksa transaksi dan bukti penerimaan Billing.

**Apakah semua layanan yang ada di tab sudah terhubung?**  
Tidak. Resume ODC belum menyediakan formulir terhubung; Rehab Medik di layar perawat masih menampilkan belum tersedia. Ketersediaan per profesi diperiksa pada [sumber panduan](../99-sumber-dan-status-panduan.md).

## Menyampaikan kendala dan hasil akhir

Catat halaman/aksi, waktu, kode pesan, dan nomor episode sesuai kanal internal yang berwenang. Sertakan langkah terakhir dan apakah data sudah terbaca pada riwayat. Jangan menyertakan password/token atau menyebarkan data pasien melalui kanal yang tidak berwenang.

Hasil tindak lanjut harus diperiksa pada data terbaru: tindakan selesai, koreksi tercatat, atau kendala masih jelas pemiliknya. Bukti/API: [KOREKSI](../99-sumber-dan-status-panduan.md#koreksi).

