# Pengkajian Keperawatan dan Perencanaan Pulang

Versi 1.0 · 6 Oktober 2026.

## Tujuan, pelaku, dan pemicu

Perawat yang berwenang mencatat pengkajian awal/ulang dan kebutuhan pemulangan. Mulai setelah pasien diterima, saat diperlukan pengkajian ulang, atau ketika tenggat pengkajian yang ditampilkan memerlukan tindak lanjut.

Prasyarat: episode dan penugasan sah, identitas pasien terbaca, serta instrumen klinis yang diperlukan tersedia. Hasil instrumen mengikuti versi yang ditetapkan rumah sakit.

## Buka ruang kerja

Dari daftar episode, pilih pasien lalu buka Keperawatan. Route: `/health-services/inpatient-management/episodes/{id}/nursing`. Buka **Pengkajian Pasien** dan pilih bagian yang dibutuhkan.

| Bagian layar | Isi pekerjaan |
| --- | --- |
| Kajian Umum | Pilih kajian awal/ulang; isi bagian formulir yang tampil dan sumber informasi pasien. |
| Resiko Jatuh | Isi instrumen risiko jatuh sesuai pasien dan versi aktif. |
| Monitoring Nyeri | Isi pengukuran dan pemantauan nyeri melalui instrumen yang tersedia. |
| Asesmen Edukasi | Catat kebutuhan/kesiapan edukasi dan bagian yang diminta formulir. |
| Pengawasan Harian Pasien | Catat pengawasan berkala melalui panelnya; bukan membuat pengkajian awal kedua. |
| Evaluasi Awal | Lengkapi evaluasi yang diperlukan melalui panelnya. |
| Perencanaan Pulang | Catat kebutuhan bantuan, edukasi, dukungan, dan persiapan pemulangan sesuai isian layar. |

“Resiko Jatuh” adalah label menu pada source; istilah risiko digunakan dalam penjelasan.

## Langkah penggunaan

1. Cocokkan identitas, episode, lokasi, dan penugasan.
2. Baca status pengkajian, tenggat, dan progres sebelum membuat dokumen.
3. Pada Kajian Umum, pilih Awal atau Ulang sesuai kejadian. Periksa dokumen yang sudah ada agar tidak membuat pengkajian awal duplikat.
4. Buka bagian pengkajian yang diperlukan dan isi kolom wajib. Jika instrumen otomatis ditampilkan, gunakan formulir itu.
5. Periksa unit angka, tanggal/waktu, sumber informasi, serta jawaban sebelum menyimpan.
6. Simpan draf jika pekerjaan belum lengkap. Buka kembali dokumen yang tersimpan ketika melanjutkan.
7. Tinjau indikator progres dan pesan validasi. Lengkapi setiap bagian wajib yang disebut.
8. Gunakan aksi penyelesaian dokumen, baca dialog, lalu konfirmasi sesuai kewenangan.
9. Periksa status dokumen dan Riwayat Dokumen Pasien. Ulangi untuk dokumen instrumen lain yang masih diperlukan.
10. Isi Perencanaan Pulang selama perawatan sesuai kebutuhan; pengisian ini tidak membuat keputusan pulang DPJP.

## Status dan aturan

| Keadaan | Aksi | Hasil |
| --- | --- | --- |
| Draf/Sedang diisi | Simpan perubahan | Dokumen masih dapat dilanjutkan sesuai hak penulis. |
| Dokumen lengkap dan memenuhi validasi | Selesaikan | Dokumen menjadi Selesai sesuai hasil server. |
| Dokumen sudah final | Koreksi/addendum bila tersedia | Catatan awal dan jejak tambahan tetap tersimpan. |
| Episode Closed | Penulisan rutin | Ruang kerja hanya baca; jangan menggunakan formulir biasa untuk mengubahnya. |

Menyelesaikan satu instrumen tidak menyelesaikan seluruh pengkajian pasien. Progres harus dibaca per dokumen/bagian. Skor yang tampil bukan petunjuk untuk mengubah jawaban agar nilai tertentu tercapai.

## Contoh dan kendala

Pasien fiktif Siti diterima pukul 11.00. Perawat menyimpan Kajian Umum pada 11.20 tetapi instrumen edukasi belum lengkap. Status satu dokumen yang selesai tidak berarti semua persyaratan pengkajian telah terpenuhi.

- **Instrumen tidak ditemukan:** minta administrator memeriksa versi/policy; jangan menyalin skor manual dari formulir pasien lain.
- **Selesaikan ditolak:** baca daftar isian yang belum terpenuhi dan kembali ke bagian itu.
- **Sudah ada pengkajian awal:** buka riwayat; pilih ulang sesuai kebutuhan, jangan menggandakan dokumen.
- **Pasien/penugasan gagal dimuat:** hentikan penulisan sampai konteks dapat diverifikasi.
- **Dokumen final salah:** gunakan koreksi beralasan sesuai hak, bukan menghapus riwayat.

Hasil akhir: dokumen pengkajian tersimpan pada episode yang benar, progres dapat diperiksa, dan kebutuhan pulang terdokumentasi. Lanjutkan [rencana asuhan/catatan harian](02-rencana-asuhan-dan-catatan-harian.md).

Bukti dan API: [PENGKAJIAN](../99-sumber-dan-status-panduan.md#pengkajian).

