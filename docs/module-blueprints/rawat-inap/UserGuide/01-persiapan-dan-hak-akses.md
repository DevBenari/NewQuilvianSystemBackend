# Persiapan Penggunaan dan Hak Akses

Versi 1.0 · 6 Oktober 2026.

## Tujuan dan pelaku

Administrator menyiapkan data dan akses sebelum petugas admisi, perawat, dokter, kepala ruangan, serta kasir menggunakan modul. Persiapan dilakukan saat pengguna mulai bekerja atau ketika unit/kamar/peran berubah.

## Daftar persiapan

| Yang diperiksa | Cara memeriksa | Kondisi yang diharapkan |
| --- | --- | --- |
| Akun pengguna | Masuk dengan akun petugas sendiri | Nama pengguna sesuai; jangan menggunakan akun rekan untuk menandatangani catatan. |
| Menu Rawat Inap | Buka menu atau alamat halaman terkait | Halaman dapat dibaca sesuai hak akun. |
| Identitas tenaga klinis | Cocokkan akun dengan profil dokter/pegawai | Server mengenali petugas dan hubungan penugasannya. |
| Unit layanan rawat inap | Periksa pengaturan unit | Unit rawat inap aktif; konfigurasi antrean sesuai rawat inap, yang tidak memakai antrean pendaftaran. |
| Kelas, kamar, bed | Periksa data master dan Papan Tempat Tidur | Bed aktif, sesuai unit/kelas, serta memiliki keadaan yang dapat digunakan. |
| Dokter dan perawat | Periksa pilihan petugas dan penugasan episode | Penanggung jawab dapat dipilih dan penugasan aktif dapat dibaca. |
| Penjamin | Periksa data pembayaran/polis/perusahaan pasien | Pilihan sesuai kategori bayar pasien. |
| Instrumen pengkajian | Buka formulir kajian yang relevan | Versi instrumen disetujui dan sesuai konteks pasien. |
| Tarif dan Billing | Koordinasikan dengan administrator/kasir | Status tagihan terbaca; tarif tidak ditentukan ulang di bangsal. |
| Cetak | Uji pratinjau dengan data fiktif | Identitas dan halaman dokumen terbaca lengkap pada printer target. |

## Langkah persiapan pengguna

1. Masuk ke Quilvian menggunakan akun sendiri.
2. Buka Rawat Inap dan halaman kerja yang dibutuhkan.
3. Pastikan identitas pengguna sesuai. Pada ruang kerja pasien, cocokkan nama, RM, episode, lokasi rawat, dan DPJP.
4. Periksa bahwa data pasien serta alergi/kewenangan berhasil dimuat. Pesan gagal membaca data berbeda dari pernyataan bahwa datanya tidak ada.
5. Bila halaman hanya dapat dibaca, periksa status episode dan penugasan sebelum meminta tambahan akses.
6. Minta administrator memberi hak pada pekerjaan yang diperlukan. Jangan menyiasati penolakan dengan URL atau akun petugas lain.
7. Setelah akses berubah, ikuti prosedur masuk ulang/penyegaran sesi yang berlaku di lingkungan target.

## Hak menu dan kewenangan pasien

| Pekerjaan | Contoh hak API yang terkait | Pemeriksaan tambahan |
| --- | --- | --- |
| Membaca/membuka episode | InpatientEpisode : Read / Create | Data kunjungan dan pasien harus sah. |
| Menempatkan/transfer | InpatientBedOccupancy : Create / Transfer | Status episode, kelayakan bed, dan kewenangan pelaku. |
| Menulis kajian | PatientAssessment : Create / Update / Complete | Identitas profesi, penugasan, status dokumen/episode. |
| Verifikasi CPPT | PatientIntegratedProgressNote : Verify | DPJP aktif; hak Verify sendiri tidak menggantikan hubungan DPJP. |
| Keputusan pulang | InpatientDischarge : Update | DPJP aktif pada episode. |
| Menandatangani resume | InpatientDischarge : Sign | Pemeriksaan penulis dan DPJP oleh server. |
| Menutup episode | InpatientDischarge : Close | Semua syarat penutupan harus terpenuhi. |
| Penutupan tanpa izin kasir | InpatientDischarge : CloseOverride | Supervisor; empat syarat lainnya tetap harus terpenuhi. |

Tabel ini merupakan contoh hak yang diperiksa pada controller, bukan paket role rumah sakit yang harus diterapkan otomatis. Administrator mengikuti kebijakan akses dan penugasan yang telah disetujui.

## Contoh dan status

Dokter fiktif dr. Andi dapat membaca pasien Budi, tetapi belum tercatat sebagai DPJP episode Budi. Membaca halaman tidak memberi hak membuat keputusan pulang. Kepala ruangan perlu memeriksa penugasan; administrator tidak cukup menambahkan menu.

Persiapan tidak mengubah status episode. Penugasan adalah riwayat tersendiri; pergantian penanggung jawab tidak menciptakan episode baru.

## Kendala dan hasil akhir

- **Menu tidak terlihat:** periksa hak menu dan akses API dengan administrator.
- **403/tindakan ditolak:** periksa peran, profesi, penugasan, serta status pasien.
- **Data pasien/alergi gagal dimuat:** muat ulang sumber yang gagal. Jangan menulis berdasarkan konteks pasien yang belum dapat diverifikasi.
- **Bed/polis/instrumen kosong:** periksa master data; jangan membuat nilai pengganti secara sembarang.

Hasil persiapan: setiap petugas dapat membuka pekerjaan yang sesuai, data pendukung tersedia, dan kewenangan pada pasien dapat diverifikasi.

Lanjutkan ke [alur lengkap](00-alur-lengkap-rawat-inap.md). Bukti dan daftar hak: [sumber panduan](99-sumber-dan-status-panduan.md#akses).

